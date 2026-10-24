using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Get the information from object.
/// </summary>
public static class ObjectHelper
{
	private static readonly EventHandlerContent[] _eventType =
	[
		EventHandlerContent.TreasureHuntDirector,
		EventHandlerContent.BattleLeveDirector,
		EventHandlerContent.CompanyLeveDirector,
		EventHandlerContent.Quest,
	];

	private static readonly ConcurrentDictionary<string, Regex> _regexCache = [];

	private static Regex GetCachedRegex(string pattern)
	{
		return _regexCache.GetOrAdd(pattern, p => new Regex(p, RegexOptions.Compiled));
	}

	internal static BNpcBase? GetObjectNPC(this IBattleChara battleChara)
	{
		return battleChara == null ? null : Service.GetSheet<Lumina.Excel.Sheets.BNpcBase>().GetRow(battleChara.BaseId);
	}

	/// <summary>
	/// Returns true if any current hostile target has the specified BNpc NameId.
	/// </summary>
	private static bool AnyHostileHasNameId(uint nameId)
	{
		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null || hostiles.Count == 0)
		{
			return false;
		}

		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var h = hostiles[i];
			if (h != null && h.NameId == nameId)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Should this enemy be provoked? <paramref name="autoProvokeAllowed"/> answers the setting "Auto provoke
	/// when there is another tank in party" for the cases it names - an enemy on a non-tank, and "Provoke
	/// anything" - and is asked only when one of them applies (it walks the alliance). Taking the enemy from a
	/// co-tank in danger follows its own setting (<see cref="TakesEnemyFromCoTank"/>), which that text does not name.
	/// </summary>
	internal static bool CanProvoke(this IBattleChara target, Func<bool>? autoProvokeAllowed = null)
	{
		if (target == null)
		{
			return false;
		}

		// "Provoke anything not on the no provoke list": the list applies to it as well. It returned before
		// the list was read, so a listed enemy was provoked all the same.
		if (!Service.Config.ProvokeAnything)
		{
			if (!target.IsAttackable())
			{
				return false;
			}

			if (DataCenter.PlayerFateId != 0 && target.FateId() == DataCenter.PlayerFateId)
			{
				return false;
			}
		}

		// Removed the listed names.
		if (OtherConfiguration.NoProvokeNames.TryGetValue(Svc.ClientState.TerritoryType, out var ns1))
		{
			foreach (var n in ns1)
			{
				if (!string.IsNullOrEmpty(n) && GetCachedRegex(n).IsMatch(target.Name?.GetText() ?? string.Empty))
				{
					return false;
				}
			}
		}

		if (Service.Config.ProvokeAnything)
		{
			return (autoProvokeAllowed?.Invoke() ?? true) || TakesEnemyFromCoTank(target);
		}

		// Target can move or too big and has a target
		if ((target.GetObjectNPC()?.Unknown0 == 0 || target.HitboxRadius >= 5) // Unknown12 used to be the flag checked for the mobs ability to move, honestly just guessing on this one
			&& (target.TargetObject?.IsValid() ?? false))
		{
			// The target is not a tank role
			if (Svc.Objects.SearchById(target.TargetObjectId) is IBattleChara targetObject
				&& !targetObject.IsJobCategory(JobRole.Tank)
				&& (Vector3.Distance(target.Position, Player.Object?.Position ?? Vector3.Zero) > 5))
			{
				return autoProvokeAllowed?.Invoke() ?? true;
			}

			return TakesEnemyFromCoTank(target);
		}
		return false;
	}

	/// <summary>
	/// The setting "Take the enemy from a co-tank in danger" (concept 09, "Tankwechsel von der anderen Seite").
	/// Two readings, both of the co-tank the enemy attacks:
	/// <list type="bullet">
	/// <item>At the HP the user set for a tank in danger, unless he rides an invulnerability (Superbolide leaves him
	/// at 1 HP on purpose), and not while the player himself is in danger after a tankbuster - the owner's rule of
	/// 04.10.2026: the enemy comes back only once his debuff and his critical state are over.</item>
	/// <item>What <see cref="TankSwapWatch"/> names to be taken: he shirked to the player, or BossModReborn announces
	/// the next tankbuster and he would take it vulnerable or not survive it - before its target is fixed.</item>
	/// </list>
	/// No distance gate: any healthy tank should react.
	/// </summary>
	private static bool TakesEnemyFromCoTank(IBattleChara enemy)
	{
		if (!Service.Config.TakeEnemyFromCoTankInDanger)
		{
			return false;
		}

		if (enemy.GameObjectId == TankSwapWatch.TakeOverSourceId)
		{
			return true;
		}

		return Svc.Objects.SearchById(enemy.TargetObjectId) is IBattleChara holder
			&& holder.IsJobCategory(JobRole.Tank)
			&& !holder.IsDead
			&& holder.GameObjectId != Player.Object?.GameObjectId
			&& holder.CanBeHealed()
			&& holder.GetEffectiveHpPercent() <= Service.Config.HealthForDyingTanks * 100f
			&& !TankSwapWatch.PlayerInDanger;
	}

	internal static bool HasPositional(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		// Validate liveness before the native BaseId read below; see the note
		// in IsEnemy() for why the try/catch alone can't cover this.
		if (!battleChara.IsValid() || battleChara.Address == nint.Zero)
		{
			return false;
		}

		try
		{
			// battleChara can be a reference captured a frame or two earlier (e.g. the next GCD's
			// target). It can go stale between the validity check above and the StatusList read
			// inside HasStatus() below, most commonly right as the target dies and its object table
			// entry gets torn down - that's a hard crash, not a catchable exception, so re-resolve
			// from the live object table immediately before touching it instead of trusting the
			// reference we were handed. Same approach as StatusHelper.DoomNeedHealing.
			if (Svc.Objects.SearchById(battleChara.GameObjectId) is not IBattleChara fresh
				|| !fresh.IsValid() || fresh.Address == nint.Zero || fresh.IsDead)
			{
				return false;
			}

			if (!fresh.IsEnemy())
			{
				return false;
			}

			if (fresh.HasStatus(false, StatusID.DirectionalDisregard))
			{
				return false;
			}

			return Svc.Data.GetExcelSheet<BNpcBase>().TryGetRow(fresh.BaseId, out var dataRow) && !dataRow.IsOmnidirectional;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Checks if the battle character has the specified NPC name.
	/// </summary>
	/// <param name="battleChara">The battle character to check.</param>
	/// <param name="npcName">The NPC name to compare against.</param>
	/// <returns>True if the battle character's NameId matches the specified NPC name; otherwise, false.</returns>
	public static bool IsNamed(this IBattleChara battleChara, NPCName npcName)
	{
		return battleChara != null && battleChara.NameId == (uint)npcName;
	}

	internal static bool IsOthersPlayersMob(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		// If the mob has a nameplate icon it is tagged as belonging to the current player's quest/leve/treasure.
		if (battleChara.GetNamePlateIcon() != 0)
		{
			return false;
		}

		// If the mob is actively targeting the player or the player's pet it cannot be treated as
		// another player's mob regardless of event type — it is a direct threat to us.
		if (battleChara.TargetObject == Player.Object)
		{
			return false;
		}

		if (Player.Object != null && battleChara.TargetObject?.OwnerId == Player.Object.GameObjectId)
		{
			return false;
		}

		// If the mob's owner is a party member it belongs to our party and can be attacked
		if (Player.Object != null && battleChara.OwnerId != 0 && PartyEntityIds.Contains(battleChara.OwnerId))
		{
			return false;
		}

		// SpecialType but no NamePlateIcon — check whether the mob's event type matches one of the
		// player-owned content directors that can produce mobs belonging to OTHER players.
		var ev = battleChara.GetEventType();
		for (var i = 0; i < _eventType.Length; i++)
		{
			if (_eventType[i] == ev)
			{
				return true;
			}
		}

		return false;
	}

	private static bool _sipInRangeCache;
	private static long _sipInRangeCacheTick = long.MinValue;
	private const long SipInRangeTtlMs = 15;

	// Whether any Forlorn enemy is within range of the player. Independent of the specific target
	// being checked, so it's computed once per frame instead of once per IsAttackable() call.
	private static bool IsForlornSipInRange()
	{
		var now = Environment.TickCount64;
		if (_sipInRangeCacheTick != long.MinValue && now - _sipInRangeCacheTick < SipInRangeTtlMs)
		{
			return _sipInRangeCache;
		}

		const float sipRange = 25f;
		var sipInRange = false;
		if (Player.Object != null)
		{
			foreach (var o in Svc.Objects)
			{
				if (o is IBattleChara c && c.IsEnemy() && c.IsTargetable && c.IsForlorn()
					&& Vector3.Distance(c.Position, Player.Object.Position) <= sipRange)
				{
					sipInRange = true;
					break;
				}
			}
		}

		_sipInRangeCache = sipInRange;
		_sipInRangeCacheTick = now;
		return sipInRange;
	}

	internal static bool IsAttackable(this IBattleChara battleChara)
	{
		if (Player.Object == null)
		{
			return false;
		}

		if (battleChara.IsAllianceMember())
		{
			return false;
		}

		if (!battleChara.IsEnemy())
		{
			return false;
		}

		if (battleChara.IsSpecialExceptionImmune())
		{
			return false; // For specific named mobs that are immune to everything.
		}

		if (battleChara.IsSpecialImmune())
		{
			return false; // For conditionally immune mobs
		}

		if (battleChara.GetEventType() == EventHandlerContent.DpsChallengeDirector && Player.Object.GetEventType() != EventHandlerContent.DpsChallengeDirector)
		{
			return false;
		}

		// Dead.
		if (Service.Config.FilterOneHpInvincible && battleChara.CurrentHp <= 1)
		{
			return false;
		}

		foreach (var status in battleChara.StatusList)
		{
			if (StatusHelper.IsInvincible(status) && ((DataCenter.IsPvP && !Service.Config.IgnorePvPInvincibility) || !DataCenter.IsPvP))
			{
				return false;
			}
		}

		// In No Hostiles Names
		if (OtherConfiguration.NoHostileNames != null &&
			OtherConfiguration.NoHostileNames.TryGetValue(Svc.ClientState.TerritoryType, out var ns1))
		{
			foreach (var n in ns1)
			{
				if (!string.IsNullOrEmpty(n) && GetCachedRegex(n).IsMatch(battleChara.Name.TextValue))
				{
					return false;
				}
			}
		}

		// Fate
		if (Service.Config.IgnoreNonFateInFate && DataCenter.Territory?.ContentType != TerritoryContentType.Eureka)
		{
			if (battleChara.FateId() != DataCenter.PlayerFateId)
			{
				return false;
			}
		}

		if (DataCenter.IsInBozjanFieldOp)
		{
			var isInCE = DataCenter.IsInBozjanFieldOpCE;

			if (isInCE)
			{
				if (!battleChara.IsBozjanCEMob() && battleChara.GetBattleNPCSubKind() != BattleNpcSubKind.BNpcPart)
				{
					return false;
				}
			}

			if (!isInCE)
			{
				if (battleChara.IsBozjanCEMob())
				{
					return false;
				}
			}
		}

		if (Service.Config.TargetQuestThings3 && battleChara.IsOthersPlayersMob())
		{
			return false;
		}

		if (Service.Config.ForlornPriority && DataCenter.IsInFate)
		{
			if (Player.Object == null)
			{
				return false;
			}

			if (IsForlornSipInRange() && !battleChara.IsForlorn())
			{
				return false;
			}
		}

		if (battleChara.IsTopPriorityNamedHostile())
		{
			return true;
		}

		if (battleChara.IsTopPriorityHostile())
		{
			return true;
		}

		if (Service.CountDownTime > 0 || DataCenter.IsPvP)
		{
			return true;
		}

		//Special cases for Black Star and Mythic Idol, which do not have valid target objects but are still attackable.
		if (battleChara.IsNamed(NPCName.BlackStar) || battleChara.IsNamed(NPCName.MythicIdol))
		{
			return true;
		}

		// Tar on me
		return (battleChara.TargetObject == Player.Object)
			|| (Player.Object != null && battleChara.TargetObject?.OwnerId == Player.Object.GameObjectId)
			|| DataCenter.IsHenched
			|| DataCenter.CurrentTargetToHostileType switch
			{
				TargetHostileType.AllTargetsCanAttack => true,
				TargetHostileType.TargetsHaveTarget => battleChara.TargetObject is not null,
				TargetHostileType.AllTargetsWhenSolo => DataCenter.PartyMembers.Count == 1 || battleChara.TargetObject is not null,
				TargetHostileType.AllTargetsWhenSoloInDuty => (DataCenter.PartyMembers.Count == 1 && (Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56]))
									|| battleChara.TargetObject is not null,
				TargetHostileType.SoloDeepDungeonSmart => IsSoloDeepDungeonSmartAttackable(battleChara),
				_ => true,
			};
	}

	internal static bool IsBozjanCEMob(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (!battleChara.IsEnemy())
		{
			return false;
		}

		if (!DataCenter.IsInBozjanFieldOp)
		{
			return false;
		}

		// Get the EventId of the mob
		return battleChara.GetEventType() == EventHandlerContent.PublicContentDirector;
	}

	private static bool IsSoloDeepDungeonSmartAttackable(IBattleChara battleChara)
	{
		// In combat: only previously engaged targets.
		if (DataCenter.InCombat)
		{
			return battleChara.TargetObject is not null;
		}

		if (DataCenter.PartyMembers.Count > 1)
		{
			return battleChara.TargetObject is not null;
		}

		// Out of combat: if any previously engaged targets are nearby, only attack those; otherwise, only the nearest single enemy.
		var hasEngagedNearby = false;
		var hostiles = DataCenter.AllHostileTargets;
		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var h = hostiles[i];
			if (h != null && h.TargetObject != null && h.DistanceToPlayer() < 25f)
			{
				hasEngagedNearby = true;
				break;
			}
		}

		if (hasEngagedNearby)
		{
			return battleChara.TargetObject is not null;
		}

		IBattleChara? nearest = null;
		var best = float.MaxValue;
		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var h = hostiles[i];
			if (h == null)
			{
				continue;
			}

			var d = h.DistanceToPlayer();
			if (d < best)
			{
				best = d;
				nearest = h;
			}
		}
		return nearest != null && battleChara.GameObjectId == nearest.GameObjectId;
	}

	internal static bool IsOccultCEMob(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (!battleChara.IsEnemy())
		{
			return false;
		}

		if (!DataCenter.IsInOccultCrescentOp)
		{
			return false;
		}

		// Get the EventId of the mob
		return battleChara.GetEventType() == EventHandlerContent.PublicContentDirector;
	}

	internal static bool IsOccultFateMob(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (!battleChara.IsEnemy())
		{
			return false;
		}

		if (!DataCenter.IsInOccultCrescentOp)
		{
			return false;
		}

		// Get the EventId of the mob
		return battleChara.GetEventType() == EventHandlerContent.FateDirector;
	}

	internal static bool IsSpecialExceptionImmune(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (battleChara.IsNamed(NPCName.CastrumGate))
		{
			return true;
		}

		return false;
	}

	private static string RemoveControlCharacters(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}

		// Use a StringBuilder for efficient string manipulation
		StringBuilder output = new(input.Length);
		foreach (var c in input)
		{
			// Exclude control characters and private use area characters
			if (!char.IsControl(c) && (c < '\uE000' || c > '\uF8FF'))
			{
				_ = output.Append(c);
			}
		}
		return output.ToString();
	}

	/// <summary>
	/// Whether the native object behind this character carries the cast and status state Dalamud's getters read.
	/// <c>false</c> means <c>IsCasting</c>, the other cast getters and <c>StatusList</c> would throw.
	/// </summary>
	/// <remarks>
	/// Dalamud reads both through virtual functions of the native object and checks neither result:
	/// <c>IsCasting</c> is <c>Struct->GetCastInfo()->IsCasting</c>, <c>StatusList</c> wraps
	/// <c>GetStatusManager()</c>. An object in the table without a virtual table, or of a class that returns no
	/// cast info, throws a NullReferenceException wherever it is read. One did in
	/// <c>TargetUpdater.UpdateAreaCasters</c>, and that ended the target update before the death, dispel, provoke
	/// and interrupt targets and the time to kill were refreshed (A291). FFXIVClientStructs guards its own
	/// <c>Character.IsCasting</c> against the same two cases (9ff7ae79, 03.03.2026). The check sits where the
	/// target lists are built, so every reader of the lists gets objects that pass it.
	/// </remarks>
	internal static unsafe bool HasNativeState(this IBattleChara battleChara)
	{
		var gameObject = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)battleChara.Address;
		if (gameObject == null || gameObject->VirtualTable == null)
		{
			return false;
		}

		var character = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)gameObject;
		return character->GetCastInfo() != null && character->GetStatusManager() != null;
	}

	internal static unsafe bool IsEnemy(this IGameObject obj)
	{
		// Validate liveness before any native read/call: a try/catch around
		// AccessViolationException does not protect us here,
		// so the object must be confirmed live beforehand.
		if (obj == null)
		{
			return false;
		}

		if (!obj.IsValid())
		{
			return false;
		}

		if (obj.Address == nint.Zero)
		{
			return false;
		}

		if (!obj.IsTargetable)
		{
			return false;
		}

		if (ActionManager.CanUseActionOnTarget((uint)ActionID.BlizzardPvE, obj.Struct()))
		{
			return true;
		}

		return false;
	}

	internal static uint TargetCharaCondition(this IBattleChara obj)
	{
		var statusId = obj.OnlineStatus.RowId;
		if (statusId != 0)
		{
			return statusId;
		}

		return 0;
	}

	internal static bool IsConditionCannotTarget(this IBattleChara obj)
	{
		var statusId = obj.OnlineStatus.RowId;
		if (statusId == 15 || statusId == 5)
		{
			return true;
		}

		return false;
	}

	internal static unsafe bool IsFriendly(this IGameObject obj)
	{
		if (obj == null)
		{
			return false;
		}

		if (!obj.IsTargetable)
		{
			return false;
		}

		if (ActionManager.CanUseActionOnTarget((uint)ActionID.CurePvE, obj.Struct()))
		{
			return true;
		}

		if (ActionManager.CanUseActionOnTarget((uint)ActionID.RaisePvE, obj.Struct()))
		{
			return true;
		}

		return false;
	}

	internal static unsafe bool IsAllianceMember(this ICharacter obj)
	{
		return obj.GameObjectId is not 0
			&& !DataCenter.IsPvP && (DataCenter.IsInAllianceRaid || DataCenter.IsInBozjanFieldOpCE || DataCenter.IsInOccultCrescentOp) && obj is IPlayerCharacter
			&& (ActionManager.CanUseActionOnTarget((uint)ActionID.RaisePvE, (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Struct())
			|| ActionManager.CanUseActionOnTarget((uint)ActionID.CurePvE, (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Struct()));
	}

	internal static unsafe bool IsOtherPlayerOutOfDuty(this ICharacter obj)
	{
		return obj.GameObjectId is not 0
			&& !DataCenter.IsPvP && obj is IPlayerCharacter
			&& (ActionManager.CanUseActionOnTarget((uint)ActionID.RaisePvE, (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Struct())
			|| ActionManager.CanUseActionOnTarget((uint)ActionID.CurePvE, (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Struct()));
	}

	internal static unsafe bool CanBeRaised(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (!battleChara.IsTargetable)
		{
			return false;
		}

		return ActionManager.CanUseActionOnTarget((uint)ActionID.RaisePvE, (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)battleChara.Struct());
	}

	internal static unsafe bool IsPlayer(this IBattleChara battleChara)
	{
		return battleChara == Player.Object;
	}

	/// <summary>
	///
	/// </summary>
	public static bool IsPlayerInParty()
	{
		if (Player.Object == null)
		{
			return false;
		}

		return Player.Object.IsTargetable && PartyEntityIds.Contains(Player.Object.EntityId);
	}

	private static readonly HashSet<uint> _partyEntityIds = [];
	private static long _partyEntityIdsTick = long.MinValue;

	// Ids of the real party members, read once per frame: membership is checked for every candidate of every action.
	private static HashSet<uint> PartyEntityIds
	{
		get
		{
			var now = Environment.TickCount64;
			if (_partyEntityIdsTick != long.MinValue && now - _partyEntityIdsTick < 15)
			{
				return _partyEntityIds;
			}

			_partyEntityIds.Clear();
			foreach (var p in Svc.Party)
			{
				if (p.EntityId is not 0 and not 0xE0000000)
				{
					_ = _partyEntityIds.Add(p.EntityId);
				}
			}

			_partyEntityIdsTick = now;
			return _partyEntityIds;
		}
	}

	/// <summary>
	///
	/// </summary>
	public static bool IsParty(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (Player.Object == null)
		{
			return false;
		}

		if (battleChara.GameObjectId == Player.Object.GameObjectId)
		{
			return true;
		}

		if (!battleChara.IsTargetable)
		{
			return false;
		}

		if (battleChara.IsPet())
		{
			return false;
		}

		if (PartyEntityIds.Contains(battleChara.EntityId))
		{
			return true;
		}

		if (DataCenter.FriendlyPartyNpcHealRaiseEnabled && battleChara.IsNpcPartyMember())
		{
			return true;
		}

		if (Service.Config.ChocoboPartyMember && battleChara.IsPlayerCharacterChocobo())
		{
			return true;
		}

		if (Service.Config.FriendlyBattleNpcHeal && battleChara.IsFriendlyBattleNPC())
		{
			return true;
		}

		if (Service.Config.FocusTargetIsParty && battleChara.IsFocusTarget() && battleChara.IsAllianceMember())
		{
			return true;
		}

		return false;
	}

	// Finds an object the player placed, e.g. the marker an object-based movement action teleports to.
	internal static bool TryGetOwnedObjectPosition(uint baseId, out Vector3 position)
	{
		position = default;
		var playerId = Player.Object?.GameObjectId ?? 0;
		if (playerId == 0)
		{
			return false;
		}

		foreach (var obj in Svc.Objects)
		{
			if (obj != null && obj.BaseId == baseId && obj.OwnerId == playerId)
			{
				position = obj.Position;
				return true;
			}
		}

		return false;
	}

	internal static bool IsPet(this IBattleChara battleChara)
	{
		if (battleChara == null || Svc.Buddies.PetBuddy == null)
		{
			return false;
		}

		return battleChara.GameObjectId == Svc.Buddies.PetBuddy.GameObject?.GameObjectId;
	}

	/// <summary>
	/// Whether this figure stands on the enemy side without having to be on the hostile list: a battle NPC of an
	/// enemy kind - a combatant, a part of an enemy, a helper, or one without a kind - that is no party member.
	/// </summary>
	/// <remarks>
	/// The hostile list holds what can be targeted and attacked. The invisible helpers that cast and resolve many
	/// raidwides, tankbusters and knockbacks, the parts of a large boss and a boss off the field are not on it, and
	/// they come in several kinds (FFXIVClientStructs <c>BattleNpcSubKind</c>: <c>Combatant</c> "Enemies, Guards",
	/// <c>BNpcPart</c> "Weak Spots"; <see cref="HelperSubKind"/>). Read as combatants only, a dungeon boss's every hit
	/// on the party went unread, among them one of 89 % right after a tankbuster marker, and the marker was learned as
	/// followed by no hit (trace of 18.10.2026, A327); read without helpers, 127 actions of helpers in trials, raids
	/// and dungeons still did, up to 88 % on eight members (traces of 19.-22.10.2026, A334). Guards are combatants too,
	/// so the kind alone is no proof of hostility: the caller adds its own evidence - a listed hostile cast, or an
	/// action that reached a party member.
	/// </remarks>
	internal static bool IsEnemySideFigure(this IBattleChara battleChara)
	{
		return battleChara != null
			&& battleChara.ObjectKind == ObjectKind.BattleNpc
			&& battleChara.GetBattleNPCSubKind() is BattleNpcSubKind.Combatant or BattleNpcSubKind.BNpcPart
				or BattleNpcSubKind.None or HelperSubKind
			&& !battleChara.IsParty();
	}

	/// <summary>
	/// The battle NPC kind of a boss's invisible helpers. FFXIVClientStructs names no kind 11; BossModReborn calls it
	/// <c>Helper</c> (<c>ActorType.Helper = 0x20B</c>, battle NPC kind 11), and every unread hit on the player in his
	/// traces of 19.-22.10.2026 came from an untargetable figure of this kind (A334).
	/// </summary>
	private const BattleNpcSubKind HelperSubKind = (BattleNpcSubKind)11;

	internal static bool IsNpcPartyMember(this IBattleChara battleChara)
	{
		if (battleChara.IsPet())
		{
			return false;
		}

		return battleChara.GetBattleNPCSubKind() == BattleNpcSubKind.NpcPartyMember;
	}

	internal static bool IsFriendlyBattleNPC(this IBattleChara battleChara)
	{
		if (DataCenter.TerritoryID == 952)
		{
			return false;
		}

		if (battleChara.IsPet())
		{
			return false;
		}

		return battleChara.GetNameplateKind() == NameplateKind.FriendlyBattleNPC;
	}

	internal static bool IsPlayerCharacterChocobo(this IBattleChara battleChara)
	{
		return battleChara.GetBattleNPCSubKind() == BattleNpcSubKind.Buddy;
	}

	internal static bool IsFocusTarget(this IBattleChara battleChara)
	{
		return Svc.Targets.FocusTarget != null && Svc.Targets.FocusTarget.GameObjectId == battleChara.GameObjectId;
	}

	internal static bool PlayerIsTargetOnSelf()
	{
		if (Player.Object == null)
		{
			return false;
		}

		return Player.Object.TargetObject?.TargetObject == Player.Object;
	}

	internal static bool IsTargetOnSelf(this IBattleChara battleChara)
	{
		return battleChara.TargetObject?.TargetObject == battleChara;
	}

	internal static bool PlayerIsAlive()
	{
		if (Player.Object == null)
		{
			return false;
		}

		if (Player.Object.IsDead)
		{
			return false;
		}

		if (!Player.Object.IsTargetable)
		{
			return false;
		}

		if (Player.Object.CurrentHp == 0)
		{
			return false;
		}

		return true;
	}

	internal static bool IsAlive(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (battleChara.IsDead)
		{
			return false;
		}

		if (!battleChara.IsTargetable)
		{
			return false;
		}

		if (battleChara.CurrentHp == 0)
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Get the object kind.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static unsafe ObjectKind GetObjectKind(this IGameObject obj)
	{
		var s = obj.Struct();
		return s == null ? default : (ObjectKind)s->ObjectKind;
	}

	/// <summary>
	/// Gets the priority value for treasure hunt nameplate icons.
	/// Lower values indicate higher priority (1 is highest priority).
	/// </summary>
	/// <param name="icon">The nameplate icon ID.</param>
	/// <returns>
	/// Priority value (1-5) for treasure hunt icons, or int.MaxValue if not a treasure hunt priority icon.
	/// </returns>
	internal static int GetNamePlateIconPriority(uint icon)
	{
		return icon switch
		{
			60687 => 1, // Treasure hunt icon 1
			60688 => 2, // Treasure hunt icon 2
			60689 => 3, // Treasure hunt icon 3
			60690 => 4, // Treasure hunt icon 4
			60691 => 5, // Treasure hunt icon 5
			_ => int.MaxValue
		};
	}

	/// <summary>
	/// Determines whether the specified game object is a top priority hostile target based on its name being listed.
	/// </summary>
	/// <param name="battleChara">The battleChara to check.</param>
	/// <returns>
	/// <c>true</c> if the game object is a top priority named hostile, target; otherwise, <c>false</c>.
	/// </returns>
	internal static bool IsTopPriorityNamedHostile(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		foreach (var id in DataCenter.PrioritizedNameIds)
		{
			if (battleChara.NameId == id)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether the specified game object is a top priority hostile target.
	/// </summary>
	/// <param name="battleChara">The game object to check.</param>
	/// <returns>
	/// <c>true</c> if the game object is a top priority hostile target; otherwise, <c>false</c>.
	/// </returns>
	internal static bool IsTopPriorityHostile(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		// After the null check: GetNamePlateIcon dereferences the object's struct without one of its own.
		var icon = battleChara.GetNamePlateIcon();

		if (battleChara.IsAllianceMember() || battleChara.IsParty())
		{
			return false;
		}

		if (Service.Config.Treasuredungeonnumbered && DataCenter.IsInTreasureHunt)
		{
			if (icon == 60687 || icon == 60688 || icon == 60689 || icon == 60690 || icon == 60691)
			{
				return true;
			}
		}

		//TODO: 14542 - Mage in board 1 of crucible, likely needs prio logic

		if (Service.Config.Treasuredungeontimed && battleChara.TreasureDungeonPrio())
		{
			return true;
		}

		if (DataCenter.IsInFate && battleChara.IsForlorn())
		{
			return true;
		}

		if (battleChara.IsM10SavagePriority())
		{
			return true;
		}

		if (battleChara.IsM9SavagePriority())
		{
			return true;
		}

		if (battleChara.IsCrucibleThirdBoardPriority())
		{
			return true;
		}

		// Check IBattleChara bespoke IsSpecialInclusionPriority method
		if (battleChara.IsSpecialInclusionPriority())
		{
			return true;
		}

		if (DataCenter.IsInOccultCrescentOp)
		{
			if (battleChara.IsOccultCEMob() && battleChara.NameId != 14717)
			{
				return true;
			}

			if (DataCenter.IsInNorthHorn)
			{
				if (battleChara.NameId == 14719)
				{
					return true;
				}
			}
		}

		// MCH prio targeting for Wildfire
		if (Player.Job == Job.MCH && (battleChara.HasStatus(true, StatusID.Wildfire) || battleChara.HasStatus(true, StatusID.Wildfire_1323)))
		{
			return true;
		}

		if (Service.Config.PrioAtomelith && DataCenter.IsPvP)
		{
			var IceBoundTomeLithA1 = battleChara.NameId == 4822;
			var IceBoundTomeLithA2 = battleChara.NameId == 4823;
			var IceBoundTomeLithA3 = battleChara.NameId == 4824;
			var IceBoundTomeLithA4 = battleChara.NameId == 4825;
			if (IceBoundTomeLithA1 || IceBoundTomeLithA2 || IceBoundTomeLithA3 || IceBoundTomeLithA4)
			{
				return true;
			}
		}

		if (Service.Config.PrioBtomelith && DataCenter.IsPvP)
		{
			var IceBoundTomeLithB1 = battleChara.NameId == 4826;
			var IceBoundTomeLithB2 = battleChara.NameId == 4827;
			var IceBoundTomeLithB3 = battleChara.NameId == 4828;
			var IceBoundTomeLithB4 = battleChara.NameId == 4829;
			var IceBoundTomeLithB5 = battleChara.NameId == 4830;
			var IceBoundTomeLithB6 = battleChara.NameId == 4831;
			var IceBoundTomeLithB7 = battleChara.NameId == 4832;
			var IceBoundTomeLithB8 = battleChara.NameId == 4833;
			var IceBoundTomeLithB9 = battleChara.NameId == 4834;
			var IceBoundTomeLithB10 = battleChara.NameId == 4835;
			var IceBoundTomeLithB11 = battleChara.NameId == 4836;
			var IceBoundTomeLithB12 = battleChara.NameId == 4837;
			var IceBoundTomeLithB13 = battleChara.NameId == 4840;
			var IceBoundTomeLithB14 = battleChara.NameId == 4841;
			var IceBoundTomeLithB15 = battleChara.NameId == 4842;
			var IceBoundTomeLithB16 = battleChara.NameId == 4843;
			var IceBoundTomeLithB17 = battleChara.NameId == 4844;
			var IceBoundTomeLithB18 = battleChara.NameId == 4845;

			if (IceBoundTomeLithB1 || IceBoundTomeLithB2 || IceBoundTomeLithB3 || IceBoundTomeLithB4 ||
				IceBoundTomeLithB5 || IceBoundTomeLithB6 || IceBoundTomeLithB7 || IceBoundTomeLithB8 ||
				IceBoundTomeLithB9 || IceBoundTomeLithB10 || IceBoundTomeLithB11 || IceBoundTomeLithB12 ||
				IceBoundTomeLithB13 || IceBoundTomeLithB14 || IceBoundTomeLithB15 || IceBoundTomeLithB16 ||
				IceBoundTomeLithB17 || IceBoundTomeLithB18)
			{
				return true;
			}
		}

		// Ensure StatusList is not null before iterating
		if (battleChara.StatusList != null)
		{
			foreach (var status in battleChara.StatusList)
			{
				if (StatusHelper.IsPriority(status))
				{
					return true;
				}
			}
		}

		if (Service.Config.ChooseAttackMark)
		{
			var targets = MarkingHelper.GetAttackSignTargets();
			if (targets != null)
			{
				foreach (var id in targets)
				{
					if (id != 0 && id == (long)battleChara.GameObjectId && battleChara.IsEnemy())
					{
						return true;
					}
				}
			}
		}

		if (Service.Config.TargetFatePriority && DataCenter.PlayerFateId != 0 && battleChara.FateId() == DataCenter.PlayerFateId)
		{
			return true;
		}

		if (Service.Config.TargetHuntingRelicLevePriority && (icon == 60092 || icon == 60094 || icon == 60096 || icon == 60097 || icon == 60098 || icon == 71244))
		{
			return true;
		}
		//60092 Hunt Log
		//60094 Treasure Mob
		//60096 Relic Weapon
		//60097 Hunt Bill
		//60098 Crescent
		//71244 Leve Target

		// Quest
		if (Service.Config.TargetQuestPriority && (icon == 71204 || icon == 71144 || icon == 71224 || icon == 71344 || (battleChara.GetEventType() == EventHandlerContent.Quest) && battleChara.GetNamePlateIcon() != 0))
		{
			return true;
		}
		//71204 Main Quest
		//71144 Major Quest
		//71224 Other Quest
		//71344 Major Quest

		// Check if the object is a BattleNpcPart
		if (Service.Config.PrioEnemyParts && battleChara.GetBattleNPCSubKind() == BattleNpcSubKind.BNpcPart)
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// True if a Deadly Doornail (NameId 14303) is currently in AllHostileTargets.
	/// </summary>
	public static bool HasDeadlyDoornail => AnyHostileHasNameId(14303);

	/// <summary>
	/// True if a Fatal Flail (NameId 14302) is currently in AllHostileTargets.
	/// </summary>
	public static bool HasFatalFlail => AnyHostileHasNameId(14302);

	/// <summary>
	/// M9S add priority: the Charnel Cell matching the player's Hell in a Cell, and Deadly Doornail or Fatal Flail depending on role.
	/// </summary>
	public static bool IsM9SavagePriority(this IBattleChara battleChara)
	{
		if (Player.Object == null || !Service.Config.M9SAdsTargeting || !DataCenter.IsInM9S)
		{
			return false;
		}

		var role = DataCenter.Role;
		var isMeleeOrTank = role is JobRole.Melee or JobRole.Tank;
		var priority = battleChara.NameId switch
		{
			14304 => IsPlayersCharnelCell(battleChara),
			// Deadly Doornail: ranged and healers always, melee and tanks only when it's close and no Fatal Flail is up.
			14303 => role is JobRole.RangedPhysical or JobRole.RangedMagical or JobRole.Healer
				|| (isMeleeOrTank && battleChara.DistanceToPlayer() < 5f && !HasFatalFlail),
			// Fatal Flail: melee and tanks always, everyone else only when no Deadly Doornail is up.
			14302 => isMeleeOrTank || !HasDeadlyDoornail,
			_ => false,
		};

		if (priority && Service.Config.InDebug)
		{
			PluginLog.Information($"IsM9SavagePriority: {battleChara.NameId} is a priority target for {role}");
		}

		return priority;
	}

	// Each Heel in a Cell on a Charnel Cell pairs with the Hell in a Cell at the same index on the player.
	private static bool IsPlayersCharnelCell(IBattleChara cell)
	{
		for (var i = 0; i < StatusHelper.HeelOfTheCellStatus.Length; i++)
		{
			if (cell.HasStatus(false, StatusHelper.HeelOfTheCellStatus[i]) && StatusHelper.PlayerHasStatus(false, StatusHelper.HellInACellStatus[i]))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// M10 (Normal and Savage) priority: Red Hot while the player has Firesnaking, Deep Blue while the player has Watersnaking.
	/// </summary>
	public static bool IsM10SavagePriority(this IBattleChara battleChara)
	{
		if (Player.Object == null || !Service.Config.M10SBroTargeting || !(DataCenter.IsInM10S || DataCenter.TerritoryID == 1322))
		{
			return false;
		}

		var priority = battleChara.NameId switch
		{
			14370 => StatusHelper.PlayerHasStatus(false, StatusID.Firesnaking),
			14369 => StatusHelper.PlayerHasStatus(false, StatusID.Watersnaking),
			_ => false,
		};

		if (priority && Service.Config.InDebug)
		{
			PluginLog.Information($"IsM10SavagePriority: {battleChara.NameId} is a priority target");
		}

		return priority;
	}

	// Crucible third board: Sahagin Piece until it has Paralyzing Spikes, then Ymir Shell, then Ymir Piece; Crawling Piece always.
	internal static bool IsCrucibleThirdBoardPriority(this IBattleChara battleChara)
	{
		if (!DataCenter.IsinThirdBoard)
		{
			return false;
		}

		return battleChara.NameId switch
		{
			14571 => Service.Config.CrucibleYmir && !battleChara.HasStatus(false, StatusID.ParalyzingSpikes), // Sahagin Piece
			14570 => Service.Config.CrucibleYmir && !HasUnspikedSahagin(), // Ymir Shell
			14569 => Service.Config.CrucibleYmir && !HasUnspikedSahagin() && !AnyLivingHostile(14570), // Ymir Piece
			14586 => Service.Config.CrucibleSiren, // Crawling Piece
			_ => false,
		};

		static bool HasUnspikedSahagin()
		{
			return AnyLivingHostile(14571, static s => !s.HasStatus(false, StatusID.ParalyzingSpikes));
		}
	}

	private static bool AnyLivingHostile(uint nameId, Func<IBattleChara, bool>? predicate = null)
	{
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && hostile.NameId == nameId && hostile.CurrentHp > 0 && (predicate == null || predicate(hostile)))
			{
				return true;
			}
		}

		return false;
	}

	internal static bool IsSpecialInclusionPriority(this IBattleChara battleChara)
	{
		if (battleChara.NameId == 8145
			|| battleChara.NameId == 10259
			|| battleChara.NameId == 12704)
		{
			return true;
		}
		//8145 Root in Dohn Meg boss 2
		//10259 Cinduruva in The Tower of Zot
		//12704 Crystalline Debris

		//14052 hellmaker, only for the Cell Block C and D prisoners
		if (DataCenter.TerritoryID == 1292 && battleChara.NameId == 14052
			&& StatusHelper.PlayerHasStatus(false, (StatusID)4544, (StatusID)4545))
		{
			return true;
		}

		// striking shrublet - Floor 10 boss ads
		if (DataCenter.TerritoryID == 1281)
		{
			if (battleChara.NameId == 13980)
			{
				return true;
			}
		}

		// forgiven adulation - Floor 30 boss ads
		if (DataCenter.TerritoryID == 1284)
		{
			if (battleChara.NameId == 13978)
			{
				return true;
			}
		}

		return false;
	}

	internal static bool TreasureDungeonPrio(this IBattleChara battleChara)
	{
		if (DataCenter.IsInTreasureHunt)
		{
			if (DataCenter.IsInTheLostCanalsofUznair)
			{
				if (battleChara.IsNamed(NPCName.NamazuStickywhisker))
				{
					return true;
				}
			}

			if (DataCenter.IsInTheShiftingAltarsofUznair)
			{
				if (battleChara.IsNamed(NPCName.GoldWhisker))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.GoldWhisker_7625))
				{
					return true;
				}
			}

			if (DataCenter.IsInTheHiddenCanalsofUznair)
			{
				if (battleChara.IsNamed(NPCName.NamazuStickywhisker))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.Abharamu))
				{
					return true;
				}
			}

			if (DataCenter.IsInTheDungeonsofLyheGhiah)
			{
				if (battleChara.IsNamed(NPCName.FuathTrickster))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.TheKeeperOfTheKeys))
				{
					return true;
				}
			}

			if (DataCenter.IsInTheShiftingOubliettesofLyheGhiah)
			{
				if (battleChara.IsNamed(NPCName.FuathTrickster_9774))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.TheKeeperOfTheKeys_9773))
				{
					return true;
				}
			}

			if (DataCenter.IsInTheExcitatron6000)
			{
				if (battleChara.IsNamed(NPCName.RainbowGolem))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.GoldenSupporter))
				{
					return true;
				}
			}

			if (DataCenter.IsInTheShiftingGymnasionAgonon)
			{
				if (battleChara.IsNamed(NPCName.GymnasiouLampas))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.GymnasiouLyssa))
				{
					return true;
				}
			}

			if (DataCenter.IsInCenoteJaJaGural)
			{
				if (battleChara.IsNamed(NPCName.AlpacaOfFortune))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.UolonOfFortune))
				{
					return true;
				}
			}

			if (DataCenter.IsInVaultOneiron)
			{
				if (battleChara.IsNamed(NPCName.Vaultkeeper))
				{
					return true;
				}

				if (battleChara.IsNamed(NPCName.GoldyCat))
				{
					return true;
				}
			}
		}

		return false;
	}

	internal static bool IsForlorn(this IBattleChara battleChara)
	{
		if (battleChara.NameId == 6737
			|| battleChara.NameId == 6738)
		{
			return true;
		}
		//6737 forlorn maiden
		//6738 forlorn maiden

		return false;
	}

	/// <summary>
	/// List of NameIds that Undead enemies in Occult Crecent.
	/// </summary>
	private static readonly HashSet<uint> IsOCUndeadSet =
	[
		13741, //Lifereaper
        13924, //Armor
        13922, //Ghost
        13921, //Caoineag
        13926, //Gourmand
        13925, //Troubadour
        13923, //Geshunpest
        13927, //Dullahan
    ];

	/// <summary>
	/// Check to see if Occult Crecent target is Undead.
	/// </summary>
	public static bool IsOCUndeadTarget(this IBattleChara battleChara)
	{
		return IsOCUndeadSet.Contains(battleChara.NameId);
	}

	/// <summary>
	/// List of NameIds that are immune to OC Slowga.
	/// </summary>
	private static readonly HashSet<uint> IsOCSlowgaImmuneSet =
	[
		13933, //Marolith
        13893, //AnimatedDoll
        13894, //AnimatedDoll
        13905, //Meraevis
        13888, //Triceratops

        13918, //Diplocaulus

        13936, //Zaratan
        13902, //Cetus
        13879, //Monk
        13911, //Panther

        13666, //Cloister Demon
        13668, //Cloister Torch
        13729, //Megaloknight
    ];

	/// <summary>
	/// Check to see if target is immune to Slowga.
	/// </summary>
	public static bool IsOCSlowgaImmuneTarget(this IBattleChara battleChara)
	{
		return IsOCSlowgaImmuneSet.Contains(battleChara.NameId);
	}

	/// <summary>
	/// List of NameIds that are immune to OC Doom.
	/// </summary>
	private static readonly HashSet<uint> IsOCDoomImmuneSet =
	[
		13893, //AnimatedDoll
        13894, //AnimatedDoll
        13917, //Sculpture
        13888, //Triceratops
        13887, //Rosebear
        13935, //Harpuia
        13886, //Aetherscab
        13912, //Havoc

        13937, //Apa
        13743, //Goobbue
        13878, //Goobbue
        13892, //Dirty Eye
        13898, //Blackguard
        13901, //Demon Pawn
        13745, //Headstone
        13881, //Headstone
        13908, //Blood Demon

        13666, //Cloister Demon
        13668, //Cloister Torch
        13729, //Megaloknight
    ];

	/// <summary>
	/// Check to see if target is immune to Phantom Doom.
	/// </summary>
	public static bool IsOCDoomImmuneTarget(this IBattleChara battleChara)
	{
		return IsOCDoomImmuneSet.Contains(battleChara.NameId);
	}

	/// <summary>
	/// List of NameIds that are immune to OC Stun.
	/// </summary>
	private static readonly HashSet<uint> IsOCStunImmuneSet =
	[
		13873, //Tormentor
        13891, //LionStatant
        13916, //Brachiosaur
        13887, //Rosebear
        13895, //Byblos
        13912, //Havoc

        13937, //Apa
        13928, //Crecent Golem
        13745, //Headstone
        13881, //Headstone
        13879, //Monk

        13666, //Cloister Demon
        13668, //Cloister Torch
        13729, //Megaloknight
    ];

	/// <summary>
	/// Check to see if target is immune to Stun.
	/// </summary>
	public static bool IsOCStunImmuneTarget(this IBattleChara battleChara)
	{
		return IsOCStunImmuneSet.Contains(battleChara.NameId);
	}

	/// <summary>
	/// List of NameIds that are immune to OC Freeze.
	/// </summary>
	private static readonly HashSet<uint> IsOCFreezeImmuneSet =
	[
		13876, //Fan
        13917, //Sculpture
        13916, //Brachiosaur
        13887, //Rosebear
        13744, //Taurus
        13880, //Taurus
        13909, //Dahak

        13919, //Zaghnal
        13934, //Uragnite
        13898, //Blackguard
        13901, //Demon Pawn
        13745, //Headstone
        13881, //Headstone

        13666, //Cloister Demon
        13668, //Cloister Torch
        13729, //Megaloknight
    ];

	/// <summary>
	/// Check to see if target is immune to Freeze.
	/// </summary>
	public static bool IsOCFreezeImmuneTarget(this IBattleChara battleChara)
	{
		return IsOCFreezeImmuneSet.Contains(battleChara.NameId);
	}

	/// <summary>
	/// List of NameIds that are immune to OC Blind.
	/// </summary>
	private static readonly HashSet<uint> IsOCBlindImmuneSet =
	[
		13931, //Chaochu
        13874, //Snapweed
        13932, //Leshy
        13933, //Marolith
        13893, //AnimatedDoll
        13894, //AnimatedDoll
        13887, //Rosebear

        13930, //Flame
        13928, //Crecent Golem
        13745, //Headstone
        13881, //Headstone

        13666, //Cloister Demon
        13668, //Cloister Torch
        13729, //Megaloknight
    ];

	/// <summary>
	/// Check to see if target is immune to Blind.
	/// </summary>
	public static bool IsOCBlindImmuneTarget(this IBattleChara battleChara)
	{
		return IsOCBlindImmuneSet.Contains(battleChara.NameId);
	}

	/// <summary>
	/// List of NameIds that are immune to OC Paralysis.
	/// </summary>
	private static readonly HashSet<uint> IsOCParalysisImmuneSet =
	[
		13931, //Chaochu
        13874, //Snapweed
        13932, //Leshy
        13933, //Marolith
        13917, //Sculpture
        13916, //Brachiosaur
        13871, //Foper
        13909, //Dahak

        13930, //Flame
        13919, //Zaghnal
        13928, //Crecent Golem
        13904, //Bachelor
        13898, //Blackguard
        13745, //Headstone
        13881, //Headstone
        13879, //Monk
        13911, //Panther

        13666, //Cloister Demon
        13668, //Cloister Torch
        13729, //Megaloknight
    ];

	/// <summary>
	/// Check to see if target is immune to Paralysis.
	/// </summary>
	public static bool IsOCParalysisImmuneTarget(this IBattleChara battleChara)
	{
		return IsOCParalysisImmuneSet.Contains(battleChara.NameId);
	}

	internal static unsafe uint GetNamePlateIcon(this IBattleChara battleChara)
	{
		return battleChara.Struct()->NamePlateIconId;
	}

	internal static unsafe EventHandlerContent GetEventType(this IBattleChara battleChara)
	{
		return battleChara.Struct()->EventId.ContentId;
	}

	internal static unsafe BattleNpcSubKind GetBattleNPCSubKind(this IBattleChara battleChara)
	{
		return (BattleNpcSubKind)battleChara.Struct()->SubKind;
	}

	internal static unsafe uint FateId(this IBattleChara battleChara)
	{
		return battleChara.Struct()->FateId;
	}

	// The candidate methods are resolved once via reflection and reused, since the shape of the
	// external TetherInfo API can't change at runtime.
	private static MethodInfo[]? _tetherProviderCandidates;

	/// <summary>
	/// Attempts to retrieve all tethers currently present using the ECommons TetherInfo API via reflection.
	/// </summary>
	public static IReadOnlyList<TetherInfo> GetAllTethers()
	{
		try
		{
			if (_tetherProviderCandidates == null)
			{
				var methods = typeof(TetherInfo).GetMethods(BindingFlags.Public | BindingFlags.Static);
				List<MethodInfo> matching = [];
				foreach (var m in methods)
				{
					if (m.ReturnType == typeof(TetherInfo[]) || typeof(System.Collections.IEnumerable).IsAssignableFrom(m.ReturnType))
					{
						matching.Add(m);
					}
				}
				_tetherProviderCandidates = [.. matching];
			}
			var candidates = _tetherProviderCandidates;

			foreach (var m in candidates)
			{
				var res = m.Invoke(null, null);
				if (res == null)
				{
					continue;
				}

				if (res is TetherInfo[] arr)
				{
					return arr;
				}

				if (res is System.Collections.IEnumerable ie)
				{
					var list = new List<TetherInfo>();
					foreach (var o in ie)
					{
						if (o is TetherInfo ti)
						{
							list.Add(ti);
						}
					}
					return list;
				}
			}
		}
		catch
		{
			// ignore and fallthrough to empty
		}

		return [];
	}

	/// <summary>
	/// Returns tethers where the object is either source or target.
	/// </summary>
	public static IReadOnlyList<TetherInfo> GetTethersFor(this IGameObject obj)
	{
		if (obj == null)
		{
			return [];
		}

		var all = GetAllTethers();
		var id = obj.GameObjectId;
		var result = new List<TetherInfo>();
		foreach (var t in all)
		{
			if (t == null)
			{
				continue;
			}

			if (ExtractTetherId(t, out _, out var src, out var tgt) && (src == id || tgt == id))
			{
				result.Add(t);
			}
		}
		return result;
	}

	/// <summary>
	/// Returns outgoing tethers (where object is source).
	/// </summary>
	public static IReadOnlyList<TetherInfo> GetOutgoingTethers(this IGameObject obj)
	{
		if (obj == null)
		{
			return [];
		}

		var all = GetAllTethers();
		var id = obj.GameObjectId;
		var result = new List<TetherInfo>();
		foreach (var t in all)
		{
			if (t == null)
			{
				continue;
			}

			if (ExtractTetherId(t, out _, out var src, out _) && src == id)
			{
				result.Add(t);
			}
		}
		return result;
	}

	/// <summary>
	/// Returns incoming tethers (where object is target).
	/// </summary>
	public static IReadOnlyList<TetherInfo> GetIncomingTethers(this IGameObject obj)
	{
		if (obj == null)
		{
			return [];
		}

		var all = GetAllTethers();
		var id = obj.GameObjectId;
		var result = new List<TetherInfo>();
		foreach (var t in all)
		{
			if (t == null)
			{
				continue;
			}

			if (ExtractTetherId(t, out _, out _, out var tgt) && tgt == id)
			{
				result.Add(t);
			}
		}
		return result;
	}

	/// <summary>
	/// Try to extract common tether fields (tether id, source object id, target object id) using reflection.
	/// Returns true if at least source/target were obtained.
	/// </summary>
	// Possible names for fields/properties on the external TetherInfo type, and the members
	// resolved from them - resolved once via reflection and reused, rather than re-scanning
	// type metadata on every tether processed.
	private static readonly string[] TetherIdNames = ["TetherId", "Id", "Tether"];
	private static readonly string[] TetherSourceNames = ["SourceObjectId", "SourceId", "Source", "SourceActorId"];
	private static readonly string[] TetherTargetNames = ["TargetObjectId", "TargetId", "Target", "TargetActorId"];
	private static MemberInfo[]? _tetherIdMembers;
	private static MemberInfo[]? _tetherSourceMembers;
	private static MemberInfo[]? _tetherTargetMembers;

	private static bool ExtractTetherId(TetherInfo t, out uint tetherId, out ulong sourceObjectId, out ulong targetObjectId)
	{
		tetherId = 0;
		sourceObjectId = 0;
		targetObjectId = 0;
		try
		{
			var tType = typeof(TetherInfo);
			_tetherIdMembers ??= ResolveTetherMembers(tType, TetherIdNames);
			_tetherSourceMembers ??= ResolveTetherMembers(tType, TetherSourceNames);
			_tetherTargetMembers ??= ResolveTetherMembers(tType, TetherTargetNames);

			object? val;

			val = TryGetMemberValue(t, _tetherIdMembers);
			if (val != null)
			{
				if (val is uint ui)
				{
					tetherId = ui;
				}
				else if (val is int i)
				{
					tetherId = (uint)i;
				}
				else if (uint.TryParse(val.ToString(), out var parsed))
				{
					tetherId = parsed;
				}
			}

			val = TryGetMemberValue(t, _tetherSourceMembers);
			if (val != null)
			{
				if (val is ulong ul)
				{
					sourceObjectId = ul;
				}
				else if (val is uint u)
				{
					sourceObjectId = u;
				}
				else if (ulong.TryParse(val.ToString(), out var pul))
				{
					sourceObjectId = pul;
				}
			}

			val = TryGetMemberValue(t, _tetherTargetMembers);
			if (val != null)
			{
				if (val is ulong ul2)
				{
					targetObjectId = ul2;
				}
				else if (val is uint u2)
				{
					targetObjectId = u2;
				}
				else if (ulong.TryParse(val.ToString(), out var pul2))
				{
					targetObjectId = pul2;
				}
			}

			return sourceObjectId != 0 || targetObjectId != 0 || tetherId != 0;
		}
		catch
		{
			return false;
		}
	}

	private static MemberInfo[] ResolveTetherMembers(Type tType, string[] names)
	{
		List<MemberInfo> members = [];
		foreach (var n in names)
		{
			var f = tType.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (f != null)
			{
				members.Add(f);
				continue;
			}
			var p = tType.GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (p != null)
			{
				members.Add(p);
			}
		}
		return [.. members];
	}

	private static object? TryGetMemberValue(object instance, MemberInfo[] members)
	{
		foreach (var m in members)
		{
			var v = m is FieldInfo f ? f.GetValue(instance) : ((PropertyInfo)m).GetValue(instance);
			if (v != null)
			{
				return v;
			}
		}
		return null;
	}

	/// <summary>
	/// Returns true if the specified battleChara is tethered to the player.
	/// </summary>
	public static bool IsTetheredToPlayer(this IBattleChara battleChara)
	{
		if (battleChara == null || Player.Object == null)
		{
			return false;
		}

		var playerId = Player.Object.GameObjectId;
		var tethers = GetTethersFor(battleChara);
		foreach (var t in tethers)
		{
			if (t == null)
			{
				continue;
			}

			if (ExtractTetherId(t, out _, out var src, out var tgt) && (src == playerId || tgt == playerId))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns true if the specified checkedtarget is tethered to checkedtarget.
	/// </summary>
	public static bool IsTetheredToSpecificTarget(this IBattleChara battleChara, IBattleChara checkedtarget)
	{
		if (battleChara == null || checkedtarget == null)
		{
			return false;
		}

		var memberId = checkedtarget.GameObjectId;
		var tethers = GetTethersFor(battleChara);
		foreach (var t in tethers)
		{
			if (t == null)
			{
				continue;
			}

			if (ExtractTetherId(t, out _, out var src, out var tgt) && (src == memberId || tgt == memberId))
			{
				return true;
			}
		}
		return false;
	}

	private static readonly ConcurrentDictionary<uint, bool> _effectRangeCheck = [];

	/// <summary>
	/// Determines whether the specified game object can be interrupted.
	/// </summary>
	/// <param name="battleChara">The game object to check.</param>
	/// <returns>
	/// <c>true</c> if the game object can be interrupted; otherwise, <c>false</c>.
	/// </returns>
	internal static bool CanInterrupt(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		// Ensure the IBattleChara object is valid before accessing its properties
		unsafe
		{
			if (battleChara.Struct() == null)
			{
				return false;
			}
		}

		var baseCheck = battleChara.IsCasting && battleChara.IsCastInterruptible && battleChara.TotalCastTime >= 2;
		if (!baseCheck)
		{
			return false;
		}

		if (!Service.Config.InterruptibleMoreCheck)
		{
			return false;
		}

		if (_effectRangeCheck.TryGetValue(battleChara.CastActionId, out var check))
		{
			return check;
		}

		var act = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRow(battleChara.CastActionId);
		return act.RowId == 0
			? (_effectRangeCheck[battleChara.CastActionId] = false)
			: (CastType)act.CastType == CastType.Cone || (CastType)act.CastType == CastType.Donut || (act.EffectRange > 0 && act.EffectRange < 8)
			? (_effectRangeCheck[battleChara.CastActionId] = false)
			: (_effectRangeCheck[battleChara.CastActionId] = true);
	}

	internal static bool IsDummy(this IBattleChara battleChara)
	{
		return battleChara?.NameId == 541;
	}

	/// <summary>
	/// Checks if the target is immune due to any special mob/boss.
	/// </summary>
	/// <param name="battleChara">The object to check.</param>
	/// <returns>True if the target is immune due to any special mechanic; otherwise, false.</returns>
	public static bool IsSpecialImmune(this IBattleChara battleChara)
	{
		return EncounterImmunity.IsImmune(battleChara);
	}

	/// <summary>
	/// Is target a boss depends on the ttk.
	/// </summary>
	/// <param name="battleChara">the object.</param>
	/// <returns></returns>
	public static bool IsBossFromTTK(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (Service.Config.DummyBoss && battleChara.IsDummy())
		{
			return true;
		}

		//Fate
		return battleChara.GetTTK(true) >= Service.Config.BossTimeToKill;
	}

	/// <summary>
	/// Is target a boss depends on the icon.
	/// </summary>
	/// <param name="battleChara">the object.</param>
	/// <returns></returns>
	public static bool IsBossFromIcon(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (Service.Config.DummyBoss && battleChara.IsDummy())
		{
			return true;
		}

		return Svc.Data.GetExcelSheet<BNpcBase>().TryGetRow(battleChara.BaseId, out var dataRow) && dataRow.Rank is 2 or 6;
	}

	/// <summary>
	/// Returns object's calculated shield value.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static uint GetObjectShield(this IBattleChara battleChara)
	{
		return battleChara is ICharacter character && character.MaxHp > 0 && character.ShieldPercentage > 0
			? character.MaxHp * character.ShieldPercentage / 100
			: 0;
	}

	/// <summary>
	/// Returns object's calculated effective HP.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static uint GetEffectiveHp(this IBattleChara battleChara)
	{
		return battleChara is ICharacter
			? battleChara.CurrentHp + GetObjectShield(battleChara)
			: 0;
	}

	/// <summary>
	/// Returns object's calculated effective HP as a percentage of Max HP, rounded down to the nearest whole number.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns>Effective HP percentage (0-100). Returns 0 if MaxHp is 0 or not an ICharacter.</returns>
	public static int GetEffectiveHpPercent(this IBattleChara battleChara)
	{
		if (battleChara is not ICharacter character || character.MaxHp == 0)
		{
			return 0;
		}

		var effectiveHp = character.CurrentHp + GetObjectShield(battleChara);
		return (int)Math.Floor((float)effectiveHp / character.MaxHp * 100f);
	}

	/// <summary>
	/// Is object dying.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static bool IsDying(this IBattleChara battleChara)
	{
		return battleChara != null && !battleChara.IsDummy() && (battleChara.GetTTK() <= Service.Config.DyingTimeToKill || battleChara.GetHealthRatio() < Service.Config.IsDyingConfig);
	}

	/// <summary>
	/// Determines whether the specified battle character is currently in combat.
	/// </summary>
	/// <param name="battleChara">The battle character to check.</param>
	/// <returns>
	/// <c>true</c> if the battle character is in combat; otherwise, <c>false</c>.
	/// </returns>
	internal static unsafe bool InCombat(this IBattleChara battleChara)
	{
		return battleChara != null && battleChara.Struct() != null && battleChara.Struct()->Character.InCombat;
	}

	private static readonly Dictionary<ulong, Vector3> LastPositions = [];
	internal static bool IsTargetMoving(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (Svc.Condition[ConditionFlag.BetweenAreas] || LastPositions.Count > 4096)
		{
			LastPositions.Clear();
		}

		var id = battleChara.GameObjectId;
		var currentPos = battleChara.Position;
		if (LastPositions.TryGetValue(id, out var lastPos))
		{
			// You can adjust the threshold as needed
			var isMoving = Vector3.Distance(currentPos, lastPos) > 0.01f;
			LastPositions[id] = currentPos;
			return isMoving;
		}
		else
		{
			LastPositions[id] = currentPos;
			return false; // First check, assume not moving
		}
	}

	private static readonly TimeSpan CheckSpan = TimeSpan.FromSeconds(2.5);

	/// <summary>
	/// Calculates the estimated time to kill the specified battle character. Only applicable after the first 2.5 seconds, and uses a moving average of the last 15 seconds of health ratios
	/// </summary>
	/// <param name="battleChara">The battle character to calculate the time to kill for.</param>
	/// <param name="wholeTime">If set to <c>true</c>, calculates the total time to kill; otherwise, calculates the remaining time to kill.</param>
	/// <returns>
	/// The estimated time to kill the battle character in seconds, or <see cref="float.NaN"/> if the calculation cannot be performed.
	/// </returns>
	internal static float GetTTK(this IBattleChara battleChara, bool wholeTime = false)
	{
		if (battleChara == null)
		{
			return float.NaN;
		}

		if (battleChara.IsDummy())
		{
			return 999.99f;
		}

		// Once per character and frame: the target selection asks it of every hostile candidate on every attack
		// it checks, and each answer walks the whole recorded history (A325). Within a frame the history does
		// not change, except where a new sample is recorded, and that clears the cache (ForgetTimeToKill). Both
		// readings come out of the same walk, so both are kept.
		var now = Environment.TickCount64;
		var id = battleChara.GameObjectId;
		if (_ttkCache.TryGetValue(id, out var cached) && now - cached.Tick < DataCenter.FrameCacheMs)
		{
			return wholeTime ? cached.Whole : cached.Remaining;
		}

		if (_ttkCache.Count > 512)
		{
			_ttkCache.Clear();
		}

		var whole = ComputeTTK(battleChara, out var remaining);
		_ttkCache[id] = (now, whole, remaining);
		return wholeTime ? whole : remaining;
	}

	private static readonly Dictionary<ulong, (long Tick, float Whole, float Remaining)> _ttkCache = [];

	/// <summary>
	/// Drops the time-to-kill answers of this frame; called when a new health sample is recorded.
	/// </summary>
	internal static void ForgetTimeToKill()
	{
		_ttkCache.Clear();
	}

	private static float ComputeTTK(IBattleChara battleChara, out float remaining)
	{
		remaining = float.NaN;
		const int movingAverageWindow = 5;
		var objId = battleChara.GameObjectId;

		var startTime = DateTime.MinValue;
		var initialHpRatio = 0f;

		// Small fixed-size window for last ratios without copying the whole queue
		Span<float> window = stackalloc float[movingAverageWindow];
		var wCount = 0;

		foreach ((var time, var hpRatiosDict) in DataCenter.RecordedHP)
		{
			if (hpRatiosDict != null && hpRatiosDict.TryGetValue(objId, out var ratio) && ratio != 1f)
			{
				if (startTime == DateTime.MinValue)
				{
					startTime = time;
					initialHpRatio = ratio;
				}

				if (wCount < movingAverageWindow)
				{
					window[wCount++] = ratio;
				}
				else
				{
					// shift left by one; window is very small so this is cheap (CopyTo handles the overlap)
					window[1..].CopyTo(window);
					window[movingAverageWindow - 1] = ratio;
				}
			}
		}

		if (startTime == DateTime.MinValue || (DateTime.Now - startTime) < CheckSpan)
		{
			return float.NaN;
		}

		var currentHealthRatio = battleChara.GetHealthRatio();
		if (float.IsNaN(currentHealthRatio))
		{
			return float.NaN;
		}

		var sum = 0f;
		for (var i = 0; i < wCount; i++)
		{
			sum += window[i];
		}

		var avg = wCount > 0 ? sum / wCount : 0f;

		var hpRatioDifference = initialHpRatio - avg;
		if (hpRatioDifference <= 0)
		{
			return float.NaN;
		}

		var elapsedTime = (float)(DateTime.Now - startTime).TotalSeconds;
		var whole = elapsedTime / hpRatioDifference;
		remaining = whole * currentHealthRatio;
		return whole;
	}

	private static readonly ConcurrentDictionary<ulong, (DateTime At, float Ttk, float Ratio)> _ttkForecast = [];
	private static readonly ConcurrentDictionary<ulong, float> _ttkBias = [];

	/// <summary>
	/// How far <see cref="GetTTK"/> has been off for this character lately: 1 means its forecasts
	/// held, above 1 means health fell faster than predicted, below 1 slower.
	/// </summary>
	/// <remarks>
	/// The estimate does not need an outside observer to be judged. It made a prediction a moment
	/// ago - "this one reaches zero in N seconds" - and the health since then either matches it or
	/// does not. Comparing the two is arithmetic the plugin can do on itself, every second, for
	/// every member, which is the whole of what a play session would have contributed and more
	/// besides: it never looks away.
	///
	/// Why it matters for the thing being built rather than for the statistic: a forecast that runs
	/// optimistic is worse than none, because the rule hanging on it would step in too late. With
	/// the bias measured, the same forecast divided by it is the corrected one, and the correction
	/// costs no configured value - it comes out of the observation. That is the adaptation the user
	/// asks for in the other concepts as well: assume, watch, correct.
	///
	/// Only falls are compared. A rising ratio means healing reached the target, which says nothing
	/// about how well the fall was predicted, so those samples are skipped rather than folded in as
	/// a negative. The value is smoothed across samples so that one spike does not move it.
	/// </remarks>
	internal static float GetTtkBias(this IBattleChara battleChara)
	{
		return battleChara != null && _ttkBias.TryGetValue(battleChara.GameObjectId, out var bias)
			? bias
			: 1f;
	}

	// Three samples at the 1 Hz recording rate. A forecast older than that belongs to a character
	// the sampling loop no longer visits - out of the object table, or out of the party - and a
	// stale time to death is worse than none, because it would keep answering confidently.
	private static readonly TimeSpan ForecastFreshness = TimeSpan.FromSeconds(3);

	/// <summary>
	/// <see cref="GetTTK"/> corrected by how wrong it has been lately - the value a rule should read.
	/// </summary>
	/// <remarks>
	/// Reads the forecast that <see cref="ScoreTtkForecast"/> filed rather than recomputing it.
	/// GetTTK walks the whole recorded history, which is four minutes of samples; the heal target
	/// selection asks this question several times per member and runs in the combat path, so
	/// recomputing there would put a four-minute walk per member into every frame. The filed value
	/// is at most a second old, and the quantity it carries averages over far longer than that.
	/// </remarks>
	internal static float GetCorrectedTTK(this IBattleChara battleChara)
	{
		if (battleChara == null
			|| !_ttkForecast.TryGetValue(battleChara.GameObjectId, out var last)
			|| float.IsNaN(last.Ttk)
			|| DateTime.Now - last.At > ForecastFreshness)
		{
			return float.NaN;
		}

		var bias = battleChara.GetTtkBias();
		return bias > 0f ? last.Ttk / bias : last.Ttk;
	}

	/// <summary>
	/// The share of its health a character is expected to still hold by the time a heal begun now
	/// could land: 1 when nothing says it is falling, 0 when it would be dead before then.
	/// </summary>
	/// <remarks>
	/// The whole of the forward-looking behaviour sits in this one number, and every rule below
	/// inherits it by multiplying its own quantity with it. That is deliberate: the alternative was
	/// a second trigger and a second rank beside the existing ones, and two mechanisms deciding the
	/// same thing drift apart. Here the question a rule asks changes from "how does this member
	/// stand" to "how will it stand when my heal arrives", and every threshold, rank and short-cut
	/// that already exists follows without being touched.
	///
	/// Why a level threshold alone is late: it is a level, and the danger is a rate. A tank falling
	/// at 33% of its pool per second passes the 70% mark with 2.1 seconds left, while the heal that
	/// mark triggers needs the rest of the GCD plus a cast to land. The cast goes out after the
	/// death. At a gentle rate the same threshold is early enough, which is why the correction has
	/// to scale with the rate rather than be a fixed offset.
	///
	/// The lead time is read from the game rather than configured: what remains of the current GCD
	/// plus one full GCD for the cast. A healer under Presence of Mind therefore looks ahead less
	/// far, which is correct - they can act sooner.
	///
	/// Self-limiting in the case that matters for cost: while health is not falling on balance,
	/// GetTTK answers NaN and this returns 1, so a party held steady sees no change at all. The
	/// look-ahead appears exactly when the net trend is downward, and grows as it steepens.
	///
	/// On by default (the owner's rule for defaults, 29.09.2026). The effect is not shown in play -
	/// static analysis and a compile say nothing about whether a tank lives.
	/// </remarks>
	internal static float GetForecastSurvivingShare(this IBattleChara battleChara)
		=> battleChara.GetForecastSurvivingShare(false);

	/// <summary>
	/// <see cref="GetForecastSurvivingShare(IBattleChara)"/> for a heal of the given kind.
	/// </summary>
	/// <param name="battleChara">The member.</param>
	/// <param name="instant">True for an off-GCD heal, which lands as soon as the current animation
	/// lock and any cast in progress let it go out; false for a GCD heal, which needs the rest of this GCD and its cast. The
	/// setting's text promises the health "by the time a heal started now would land", and for an
	/// instant heal that is now: read with the GCD lead, Benediction or Essential Dignity went out up
	/// to a GCD before they were needed (A213). An off-GCD action that heals only with the next GCD
	/// (Synastry, Krasis, Soteria, Emergency Tactics) passes false (A221).</param>
	internal static float GetForecastSurvivingShare(this IBattleChara battleChara, bool instant)
	{
		if (battleChara == null || !Service.Config.HealAheadOfDamage)
		{
			return 1f;
		}

		var ttk = battleChara.GetCorrectedTTK();
		if (float.IsNaN(ttk) || ttk <= 0f)
		{
			return 1f;
		}

		var lead = instant ? GetInstantHealLeadTime() : GetHealLeadTime();
		return lead <= 0f ? 1f : Math.Clamp(1f - (lead / ttk), 0f, 1f);
	}

	/// <summary>
	/// How long an off-GCD heal decided on now takes to go out: until the current animation lock and
	/// any cast in progress have ended - nothing is woven over a hard cast (concept 08, A217).
	/// </summary>
	internal static float GetInstantHealLeadTime()
	{
		var player = Player.Object;
		var castLeft = player != null && player.IsCasting
			? Math.Max(0f, player.TotalCastTime - player.CurrentCastTime)
			: 0f;
		return Math.Max(Math.Max(0f, DataCenter.AnimationLock), castLeft);
	}

	private static long _healLeadCacheTick = long.MinValue;
	private static float _healLeadCache;

	// Same frame cache as DataCenter's party HP statistics, and for the same reason: the heal target
	// selection asks this several times per member per frame, and each read of the GCD figures goes
	// to the action manager. One value per frame is as current as the question can be - read from the
	// shared figure instead of a copy of it (A340).
	private static long HealLeadTtlMs => DataCenter.FrameCacheMs;

	/// <summary>
	/// How long a heal decided on now takes to land: what is left of this GCD, plus one GCD for the
	/// cast. Read from the game, so a shortened GCD shortens the look-ahead with it.
	/// </summary>
	internal static float GetHealLeadTime()
	{
		var now = Environment.TickCount64;
		if (_healLeadCacheTick != long.MinValue && now - _healLeadCacheTick < HealLeadTtlMs)
		{
			return _healLeadCache;
		}

		_healLeadCache = DataCenter.DefaultGCDRemain + DataCenter.DefaultGCDTotal;
		_healLeadCacheTick = now;
		return _healLeadCache;
	}

	/// <summary>
	/// Is anything actually threatening this character right now?
	/// </summary>
	/// <remarks>
	/// The user's requirement, in his words: an emergency heal on a freshly raised player is right
	/// "falls Gefahr bevorsteht, z.B. grosser heftiger AoE" - but "wenn der Spieler aber keine Aggro
	/// hat, kein AoE ansteht, oder kein sonstiger Schaden ansteht, wuerde doch HoT oder kleinere
	/// Heals bzw. beides reichen".
	///
	/// That is three questions, and the tree can answer all three:
	/// <list type="bullet">
	/// <item>aimed at - an enemy is attacking this member, or casting something that will land on
	/// them (<see cref="DataCenter.TargetedPartyMembers"/>)</item>
	/// <item>an announced area cast - <see cref="DataCenter.IsHostileCastingAOE"/></item>
	/// <item>damage actually arriving - the health trend has a finite time to zero</item>
	/// </list>
	///
	/// Deliberately not a rule about raising. A player who has just been resurrected is only the
	/// most visible case: he holds a few percent, carries no aggro and takes no damage, so every
	/// threshold in the tree reads him as the most urgent member in the party while nothing at all
	/// is happening to him. The same is true of a damage dealer who has just run out of an area
	/// effect. The user's requirement is about danger, not about raising, and so is this.
	///
	/// The third arm is why this is worth the name: aggro and an announced cast are both predictions
	/// that can miss - a boss mechanic that neither casts nor retargets shows up in neither - and
	/// the health trend catches what they miss, one sample after the first hit lands. An emergency
	/// heal is an ability without a cast time, so the cost of finding out that way is a delay, not a
	/// death.
	///
	/// Errs towards "threatened": every arm that cannot answer says yes by staying silent, and the
	/// caller then behaves exactly as it does today.
	/// </remarks>
	/// <summary>
	/// Is anybody in the party going to fall before a heal decided on now could reach them?
	/// </summary>
	/// <remarks>
	/// The user's bound on every rule that yields a GCD: suspending something is only right while
	/// the incoming damage is still manageable. Manageable is not a number of enemies and not a sum
	/// of their output - it is whether the group holds, and that is measured rather than set.
	///
	/// A rule that gives up a GCD costs the party exactly that GCD. So the question is whether
	/// everyone survives it with enough left over to be healed afterwards, which is the same lead
	/// time the forward-looking health uses: the rest of this GCD plus one for the cast.
	///
	/// Nobody falling means NaN everywhere, and NaN is not a short time - it is no death in sight.
	/// So a party being held steady never trips this, however many enemies are standing on it, and
	/// a single member actually going down stops the hold even if the pack is small. That is the
	/// intended shape: the old proxy measured the pack, and a big pack that is being healed through
	/// is precisely where stretching the throttle pays.
	///
	/// It answers for the whole party rather than for the tank alone, which is deliberately
	/// conservative: a damage dealer standing in fire blocks the hold even though the stun would not
	/// have helped him. The cost of that is one GCD of stun stretching, and the user's standing
	/// order puts the group's survival ahead of output.
	/// </remarks>
	internal static bool AnyPartyMemberFallingWithinHealWindow()
	{
		var lead = GetHealLeadTime();
		if (lead <= 0f)
		{
			return false;
		}

		var party = DataCenter.PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || member.IsDead)
			{
				continue;
			}

			var ttk = member.GetCorrectedTTK();
			if (!float.IsNaN(ttk) && ttk <= lead)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsUnderThreat(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return true;
		}

		if (DataCenter.TargetedPartyMembers.Contains(battleChara.GameObjectId))
		{
			return true;
		}

		if (DataCenter.IsHostileCastingAOE)
		{
			return true;
		}

		// A finite time to zero means the health is falling on balance - net of every mitigation,
		// barrier and foreign heal. NaN means it is not, which is the answer this asks for.
		return !float.IsNaN(battleChara.GetCorrectedTTK());
	}

	/// <summary>
	/// <see cref="GetHealthRatio"/> carried forward to the moment a heal begun now would land. With
	/// "Heal ahead of incoming damage" off it is exactly <see cref="GetHealthRatio"/>. Every healing
	/// threshold reads this, as that setting's text promises.
	/// </summary>
	public static float GetForecastHealthRatio(this IBattleChara battleChara)
	{
		return battleChara.GetHealthRatio() * battleChara.GetForecastSurvivingShare();
	}

	/// <summary>
	/// <see cref="GetForecastHealthRatio(IBattleChara)"/> for a heal of the given kind: an off-GCD heal
	/// (<paramref name="instant"/>) looks ahead only as far as the current animation lock.
	/// </summary>
	public static float GetForecastHealthRatio(this IBattleChara battleChara, bool instant)
	{
		return battleChara.GetHealthRatio() * battleChara.GetForecastSurvivingShare(instant);
	}

	/// <summary>
	/// The health ratio this character is expected to hold <paramref name="seconds"/> from now, at the
	/// measured net rate - every heal, barrier and mitigation already in it - with the forecast error of
	/// the time to death taken out (<see cref="GetCorrectedTTK"/>). Today's ratio while the health is not
	/// falling on balance. For a cooldown whose worth lasts that long, asked independently of "Heal ahead
	/// of incoming damage", whose text promises the moment a heal lands.
	/// </summary>
	internal static float GetHealthRatioIn(this IBattleChara battleChara, float seconds)
	{
		var ratio = battleChara.GetHealthRatio();
		var ttk = battleChara.GetCorrectedTTK();
		if (float.IsNaN(ttk) || ttk <= 0f || seconds <= 0f)
		{
			return ratio;
		}

		return ratio * Math.Clamp(1f - (seconds / ttk), 0f, 1f);
	}

	/// <summary>
	/// <see cref="GetEffectiveHp"/> carried forward to the moment a heal begun now would land.
	/// </summary>
	internal static uint GetForecastEffectiveHp(this IBattleChara battleChara, bool instant = false)
	{
		return (uint)(battleChara.GetEffectiveHp() * battleChara.GetForecastSurvivingShare(instant));
	}

	/// <summary>
	/// Whether this member stands in the heal chain's critical class (concept 07): unprotected by an
	/// invulnerability, and at or below <c>HealthForDyingTanks</c> in effective health carried forward
	/// to the moment a heal begun now would land. One definition for every reader - the heal target
	/// order, the defense holds and Lux Solaris - so they cannot disagree about who is in danger.
	/// Unprotected as the heal decision reads it (<see cref="IsUnprotectedForHealing"/>).
	/// </summary>
	internal static bool IsInCriticalClass(this IBattleChara battleChara)
	{
		return battleChara.IsInCriticalClass(battleChara.IsUnprotectedForHealing());
	}

	/// <summary>
	/// <see cref="IsInCriticalClass(IBattleChara)"/> for a caller that has already read whether the
	/// member is protected, so the status list is not walked twice and the answer cannot change
	/// between the two reads.
	/// </summary>
	internal static bool IsInCriticalClass(this IBattleChara battleChara, bool unprotected)
	{
		return unprotected
			&& battleChara.GetForecastEffectiveHpPercent() <= Service.Config.HealthForDyingTanks * 100f;
	}

	/// <summary>
	/// <see cref="GetEffectiveHpPercent"/> carried forward to the moment a heal begun now would land.
	/// </summary>
	internal static int GetForecastEffectiveHpPercent(this IBattleChara battleChara)
	{
		return (int)(battleChara.GetEffectiveHpPercent() * battleChara.GetForecastSurvivingShare());
	}

	/// <summary>
	/// <see cref="GetPlayerHealthRatio"/> carried forward the same way, for the paths that ask about
	/// the player without holding an object reference.
	/// </summary>
	internal static float GetForecastPlayerHealthRatio(bool instant = false)
	{
		return Player.Object == null
			? GetPlayerHealthRatio()
			: GetPlayerHealthRatio() * Player.Object.GetForecastSurvivingShare(instant);
	}

	/// <summary>
	/// Scores the forecast made for this character earlier against what its health actually did, and
	/// files a fresh forecast for the next round. Called once per history sample.
	/// </summary>
	internal static void ScoreTtkForecast(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return;
		}

		var id = battleChara.GameObjectId;
		var now = DateTime.Now;
		var ratio = battleChara.GetHealthRatio();
		var ttk = battleChara.GetTTK();

		if (_ttkForecast.TryGetValue(id, out var last))
		{
			var elapsed = (float)(now - last.At).TotalSeconds;
			var actualFall = last.Ratio - ratio;

			// Only a fall says anything about a forecast of falling. A rise means a heal landed.
			// The forecast also has to have been a number, and enough time has to have passed for
			// the difference to be more than sampling noise.
			if (!float.IsNaN(last.Ttk) && last.Ttk > 0f && elapsed > 0f && actualFall > 0f)
			{
				var expectedFall = elapsed * (last.Ratio / last.Ttk);
				if (expectedFall > 0f)
				{
					var sample = actualFall / expectedFall;
					var previous = _ttkBias.TryGetValue(id, out var b) ? b : 1f;

					// Smoothed, so a single spike does not take over. Bounded because the ratio can
					// run away when a forecast was very long and a burst lands.
					var smoothed = (previous * 3f + sample) / 4f;
					_ttkBias[id] = Math.Clamp(smoothed, 0.25f, 4f);
				}
			}
		}

		_ttkForecast[id] = (now, ttk, ratio);
	}

	private static readonly TimeSpan TtkForecastLifetime = TimeSpan.FromSeconds(DataCenter.HP_RECORD_TIME);

	/// <summary>
	/// Drops the scoring state of characters that have not been seen for a while.
	/// </summary>
	/// <remarks>
	/// Both dictionaries are keyed by object id and would otherwise grow for every member of every
	/// party of a session. Pruning against the current party instead would be cheaper but wrong: a
	/// member who walks out of the object table for a few seconds would come back with the learned
	/// bias reset to 1, which is the one moment - a pull spread over a wide room - where the
	/// correction is worth the most. Age is the honest criterion, and it is the same window the
	/// health history itself keeps.
	/// </remarks>
	internal static void ForgetStaleTtkForecasts()
	{
		// Runs once per history sample over a table about the size of the party; a size below which the
		// scan was skipped only kept stale entries longer (A340).
		var cutoff = DateTime.Now - TtkForecastLifetime;
		foreach ((var id, var entry) in _ttkForecast)
		{
			if (entry.At < cutoff)
			{
				_ = _ttkForecast.TryRemove(id, out _);
				_ = _ttkBias.TryRemove(id, out _);
			}
		}
	}

	private static readonly ConcurrentDictionary<ulong, DateTime> _aliveStartTimes = [];
	private static readonly ConcurrentDictionary<ulong, DateTime> _deadStartTimes = [];

	/// <summary>
	/// Gets how long the battleChara has been alive in seconds since their last death, capped at 30.
	/// </summary>
	/// <param name="battleChara">The battle character to check.</param>
	/// <returns>
	/// The time in seconds since the character's last death or first appearance, or float.NaN if unable to determine.
	/// </returns>
	internal static float TimeAlive(this IBattleChara? battleChara)
	{
		if (battleChara == null)
		{
			return float.NaN;
		}

		// If the character is dead, reset their alive time
		if (battleChara.IsDead || Svc.Condition[ConditionFlag.BetweenAreas])
		{
			_ = _aliveStartTimes.TryRemove(battleChara.GameObjectId, out _);
			return 0;
		}

		return SecondsSinceFirstSeen(_aliveStartTimes, battleChara.GameObjectId);
	}

	/// <summary>
	/// Gets how long the character has been dead in seconds, capped at 30.
	/// </summary>
	/// <param name="battleChara">The battle character to check.</param>
	/// <returns>
	/// The time in seconds since the character's death, 0 while alive, or float.NaN if unable to determine.
	/// </returns>
	internal static float TimeDead(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return float.NaN;
		}

		// If the character is alive, reset their dead time
		if (!battleChara.IsDead)
		{
			_ = _deadStartTimes.TryRemove(battleChara.GameObjectId, out _);
			return 0;
		}

		return SecondsSinceFirstSeen(_deadStartTimes, battleChara.GameObjectId);
	}

	private static float SecondsSinceFirstSeen(ConcurrentDictionary<ulong, DateTime> startTimes, ulong id)
	{
		var start = startTimes.GetOrAdd(id, static _ => DateTime.Now);
		return MathF.Min(30, (float)(DateTime.Now - start).TotalSeconds);
	}

	/// <summary>
	/// Determines whether the player has been hitting the specified battle character for at least a second,
	/// counted from the player's first damaging hit on it.
	/// </summary>
	/// <param name="battleChara">The battle character to check.</param>
	/// <returns>
	/// <c>true</c> if the player's first hit on the battle character is at least one second old; otherwise, <c>false</c>.
	/// </returns>
	internal static bool IsEngagedByPlayer(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		var now = DateTime.Now;
		foreach ((var id, var time) in DataCenter.AttackedTargets)
		{
			if (id == battleChara.GameObjectId)
			{
				return now - time >= TimeSpan.FromSeconds(1);
			}
		}
		return false;
	}

	private struct LosCacheEntry
	{
		public Vector3 PlayerPos;
		public Vector3 TargetPos;
		public long ExpiresAt;
		public bool Visible;
	}
	private static readonly ConcurrentDictionary<ulong, LosCacheEntry> _losCache = [];
	private const float LosPosEpsilonSq = 0.04f;   // ~20 cm tolerance
	private const long LosTtlMs = 33;      // one 30–60 FPS frame

	// Optional: clear cache when swapping areas or if it grows too large
	private static void MaybeResetLosCache()
	{
		if (Svc.Condition[ConditionFlag.BetweenAreas] || _losCache.Count > 4096)
		{
			_losCache.Clear();
		}
	}

	// New overload to allow caller to supply eye position once per loop
	internal static unsafe bool CanSeeFrom(this IBattleChara battleChara, Vector3 playerEyePos, float targetYOffset = 2.0f)
	{
		if (battleChara == null || Player.Object == null)
		{
			return false;
		}

		var targetStruct = battleChara.Struct();
		if (targetStruct == null)
		{
			return false;
		}

		MaybeResetLosCache();

		var targetPos = battleChara.Position;
		targetPos.Y += targetYOffset;
		var id = battleChara.GameObjectId;
		var now = Environment.TickCount64;

		if (_losCache.TryGetValue(id, out var entry))
		{
			if (now <= entry.ExpiresAt &&
				Vector3.DistanceSquared(playerEyePos, entry.PlayerPos) <= LosPosEpsilonSq &&
				Vector3.DistanceSquared(targetPos, entry.TargetPos) <= LosPosEpsilonSq)
			{
				return entry.Visible;
			}
		}

		var offset = targetPos - playerEyePos;
		var maxDist = offset.Length();
		if (maxDist < 0.01f)
		{
			return true;
		}

		var direction = offset / maxDist;

		RaycastHit hit;
		var materialFilter = stackalloc int[] { 0x4000, 0, 0x4000, 0 };
		var module = Framework.Instance()->BGCollisionModule;
		var blocked = module->RaycastMaterialFilter(&hit, &playerEyePos, &direction, maxDist, 1, materialFilter);

		var visible = !blocked;
		_losCache[id] = new LosCacheEntry
		{
			PlayerPos = playerEyePos,
			TargetPos = targetPos,
			ExpiresAt = now + LosTtlMs,
			Visible = visible
		};
		return visible;
	}

	// Backward-compatible existing API calls the overload
	internal static unsafe bool CanSee(this IBattleChara battleChara, float playerYOffset = 2.0f, float targetYOffset = 2.0f)
	{
		if (battleChara == null || Player.Object == null)
		{
			return false;
		}

		var playerPos = Player.Object.Position;
		playerPos.Y += playerYOffset;
		return CanSeeFrom(battleChara, playerPos, targetYOffset);
	}

	/// <summary>
	/// Get the Player's current MP percentage.
	/// </summary>
	/// <returns></returns>
	public static float GetPlayerMPRatio()
	{
		if (Player.Object == null)
		{
			return 0;
		}

		if (Player.Object.MaxMp == 0)
		{
			return 0; // Avoid division by zero
		}

		return (float)Player.Object.CurrentMp / Player.Object.MaxMp;
	}

	/// <summary>
	/// Whether healing is being withheld from <paramref name="battleChara"/> so that the death its
	/// own ability is waiting for can happen - the user's <c>WithholdHealingForLivingDead</c>
	/// setting together with <see cref="StatusHelper.InDeathTriggerWindow"/>.
	/// <para>
	/// The pairing lives here rather than in <see cref="StatusHelper"/>, which deliberately reads no
	/// configuration, and it lives in one place rather than at each decision because the hold has to
	/// hold on every path a heal can reach the bearer through. Missing one of them does not weaken
	/// the hold, it defeats it: the bearer is healed out of the trigger and the ability is spent for
	/// nothing.
	/// </para>
	/// </summary>
	public static bool IsHeldForDeathTrigger(this IBattleChara battleChara)
	{
		return battleChara != null
			&& Service.Config != null
			&& Service.Config.WithholdHealingForLivingDead
			&& battleChara.InDeathTriggerWindow();
	}

	/// <inheritdoc cref="IsHeldForDeathTrigger(IBattleChara)"/>
	public static bool PlayerIsHeldForDeathTrigger()
	{
		return Service.Config != null
			&& Service.Config.WithholdHealingForLivingDead
			&& StatusHelper.PlayerInDeathTriggerWindow();
	}

	/// <summary>
	/// Whether this member counts as unprotected for the heal decision - its threshold, the heal target
	/// order, the critical class and the heal-first reading of an announced hit. That is
	/// <see cref="StatusHelper.CanBeHealed"/>, except that with <c>WithholdHealingForLivingDead</c> off
	/// Living Dead does not count as protection.
	/// <para>
	/// Living Dead does not spare its bearer the fall to zero, it turns it into Walking Dead, after which
	/// healing worth his whole maximum HP has to go in within ten seconds or he is KO'd (ActionId.resx,
	/// action 3638). The setting's text promises for Off that he is healed normally and the death never
	/// happens. Read as an invulnerability, his threshold dropped to HealthProtectedRatio for the first
	/// half of the effect, so a member RSR had just pulled above it at the last-ditch line - the very use
	/// the text names as the reason for Off - was left there, and the next hit converted him (A306). With
	/// the setting on, the hold (<see cref="IsHeldForDeathTrigger"/>) governs Living Dead, and this reads
	/// as CanBeHealed.
	/// </para>
	/// <para>
	/// The readers that ask whether a hit can kill him now keep CanBeHealed: the co-tank provoke, which
	/// does not pull the enemy off a tank under Living Dead (451d9e90), and the own tankbuster cooldowns.
	/// The setting speaks of healing.
	/// </para>
	/// </summary>
	/// <remarks>
	/// With the setting off, a member carrying no other invulnerability is unprotected; one who carries
	/// another beside Living Dead is read by CanBeHealed over the whole list, as before.
	/// </remarks>
	public static bool IsUnprotectedForHealing(this IBattleChara battleChara)
	{
		return (Service.Config != null && !Service.Config.WithholdHealingForLivingDead
				&& !battleChara.HasStatus(false, StatusHelper.NoNeedHealingStatusBesidesDeathTrigger))
			|| battleChara.CanBeHealed();
	}

	/// <inheritdoc cref="IsUnprotectedForHealing(IBattleChara)"/>
	public static bool PlayerIsUnprotectedForHealing()
	{
		if (Player.Object == null)
		{
			return false;
		}

		return (Service.Config != null && !Service.Config.WithholdHealingForLivingDead
				&& !StatusHelper.PlayerHasStatus(false, StatusHelper.NoNeedHealingStatusBesidesDeathTrigger))
			|| StatusHelper.PlayerCanBeHealed();
	}

	/// <summary>
	/// Get the Player's current HP percentage.
	/// </summary>
	/// <returns></returns>
	public static float GetPlayerHealthRatio()
	{
		if (Player.Object == null)
		{
			return 0; // This may need to be changed to 100
		}

		if (DataCenter.RefinedHP.TryGetValue(Player.Object.GameObjectId, out var hp))
		{
			return hp;
		}

		if (Player.Object.MaxHp == 0)
		{
			return 0; // Avoid division by zero
		}

		return (float)Player.Object.CurrentHp / Player.Object.MaxHp;
	}

	/// <summary>
	/// Get the <paramref name="battleChara"/>'s current HP percentage.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static float GetHealthRatio(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return 0; // This may need to be changed to 100
		}

		if (!battleChara.IsValid())
		{
			return 0; // This may need to be changed to 100
		}

		// Called for every candidate of every action, sorts included, so each target is worked out once per frame.
		var id = battleChara.GameObjectId;
		var now = Environment.TickCount64;
		if (_healthRatioCache.TryGetValue(id, out var cached) && now - cached.Tick < HealthRatioTtlMs)
		{
			return cached.Ratio;
		}

		if (_healthRatioCache.Count > 512)
		{
			_healthRatioCache.Clear();
		}

		var ratio = ComputeHealthRatio(battleChara);
		_healthRatioCache[id] = (now, ratio);
		return ratio;
	}

	private const long HealthRatioTtlMs = 15;
	private static readonly Dictionary<ulong, (long Tick, float Ratio)> _healthRatioCache = [];

	private static float ComputeHealthRatio(IBattleChara battleChara)
	{
		if (battleChara.IsParty() && battleChara.DoomNeedHealing())
		{
			return 0.01f;
		}

		if (DataCenter.RefinedHP.TryGetValue(battleChara.GameObjectId, out var hp))
		{
			return hp;
		}

		try
		{
			return battleChara.MaxHp == 0 ? 0 : (float)battleChara.CurrentHp / battleChara.MaxHp;
		}
		catch (Exception)
		{
			return 0;
		}
	}

	/// <summary>
	/// Determines the positional relationship of the player relative to the enemy.
	/// </summary>
	/// <param name="enemy">The enemy game object.</param>
	/// <returns>
	/// An <see cref="EnemyPositional"/> value indicating whether the player is in front, at the rear, or on the flank of the enemy.
	/// </returns>
	public static EnemyPositional FindEnemyPositional(this IBattleChara enemy)
	{
		// Validate liveness before the native Position/Rotation reads below;
		// see the note in IsEnemy() for why the try/catch alone can't cover this.
		if (enemy == null || !enemy.IsValid() || enemy.Address == nint.Zero || Player.Object == null)
		{
			return EnemyPositional.None;
		}

		var faceVec = enemy.GetFaceVector();

		// Positionals are judged on the ground plane, so height differences must not skew the angle.
		var offset = Player.Object.Position - enemy.Position;
		var dir = new Vector2(offset.X, offset.Z);
		if (dir.LengthSquared() < 0.0001f)
		{
			return EnemyPositional.None;
		}

		var dotProduct = Vector2.Dot(Vector2.Normalize(dir), new Vector2(faceVec.X, faceVec.Z));

		// 90° sectors: front and rear are within 45° of the facing axis.
		const float sectorCos = 0.70710677f;

		if (dotProduct > sectorCos)
		{
			return EnemyPositional.Front;
		}
		else if (dotProduct < -sectorCos)
		{
			return EnemyPositional.Rear;
		}

		return EnemyPositional.Flank;
	}

	/// <summary>
	/// Gets the facing direction vector of the game object.
	/// </summary>
	/// <param name="battleChara">The game object.</param>
	/// <returns>
	/// A <see cref="Vector3"/> representing the facing direction of the game object.
	/// </returns>
	internal static Vector3 GetFaceVector(this IBattleChara battleChara)
	{
		if (battleChara == null || !battleChara.IsValid() || battleChara.Address == nint.Zero)
		{
			return Vector3.Zero;
		}

		var rotation = battleChara.Rotation;
		return new Vector3((float)Math.Sin(rotation), 0, (float)Math.Cos(rotation));
	}

	/// <summary>
	/// Calculates the angle between two vectors.
	/// </summary>
	/// <param name="vec1">The first vector.</param>
	/// <param name="vec2">The second vector.</param>
	/// <returns>
	/// The angle in radians between the two vectors.
	/// </returns>
	internal static double AngleTo(this Vector3 vec1, Vector3 vec2)
	{
		double lengthProduct = vec1.Length() * vec2.Length();
		if (lengthProduct == 0)
		{
			return 0;
		}

		double dotProduct = Vector3.Dot(vec1, vec2);
		return Math.Acos(dotProduct / lengthProduct);
	}

	/// <summary>
	/// The distance from <paramref name="battleChara"/> to the player
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static float DistanceToPlayer(this IBattleChara battleChara)
	{
		if (battleChara == null || Player.Object == null)
		{
			return float.MaxValue;
		}

		var playerPos = Player.Object.Position;
		var targetPos = battleChara.Position;

		// Check vertical distance first - if too far vertically, the target is unreachable
		var dy = MathF.Abs(targetPos.Y - playerPos.Y);
		if (dy > 30f)
		{
			return dy;
		}

		// Use XZ-plane (horizontal) distance only — the game engine measures action range
		// purely on the horizontal plane, ignoring Y-axis differences (when within vertical threshold).
		var dx = targetPos.X - playerPos.X;
		var dz = targetPos.Z - playerPos.Z;
		var distance = MathF.Sqrt(dx * dx + dz * dz) - (Player.Object.HitboxRadius + battleChara.HitboxRadius);
		return MathF.Max(0f, distance);
	}

	/// <summary>
	/// The distance from <paramref name="battleChara"/> to the player's pet
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static float DistanceToPet(IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return float.MaxValue;
		}
		var pet = DataCenter.GetPet();
		if (pet == null)
		{
			return float.MaxValue;
		}

		var pdx = battleChara.Position.X - pet.Position.X;
		var pdz = battleChara.Position.Z - pet.Position.Z;
		return MathF.Max(0f, MathF.Sqrt(pdx * pdx + pdz * pdz) - battleChara.HitboxRadius);
	}

}