using ECommons.ExcelServices;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.Sheets;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Get the information from object.
/// </summary>
public static partial class ObjectHelper
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
		return battleChara == null ? null : Service.GetSheet<Lumina.Excel.Sheets.BNpcBase>().GetRowOrDefault(battleChara.BaseId);
	}

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
	/// Should this enemy be provoked? <paramref name="autoProvokeAllowed"/> answers the setting "Provoke when
	/// there is another tank in the party" for the cases it names - an enemy on a non-tank, and "Provoke any
	/// enemy not on the Don't provoke list" - and is asked only when one of them applies (it walks the alliance). Taking the enemy from a
	/// co-tank in danger follows its own setting (<see cref="TakesEnemyFromCoTank"/>), which that text does not name.
	/// </summary>
	internal static bool CanProvoke(this IBattleChara target, Func<bool>? autoProvokeAllowed = null)
	{
		if (target == null)
		{
			return false;
		}

		// "Provoke any enemy not on the Don't provoke list": the list applies to it as well. It returned before
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

		if ((target.GetObjectNPC()?.Unknown0 == 0 || target.HitboxRadius >= 5)
			&& (target.TargetObject?.IsValid() ?? false))
		{
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
	/// The setting "Take the enemy from a co-tank in danger" (concept 09, "Tankwechsel nach einem Tankbuster").
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

		if (!battleChara.IsValid() || battleChara.Address == nint.Zero)
		{
			return false;
		}

		try
		{
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

		if (battleChara.GetNamePlateIcon() != 0)
		{
			return false;
		}

		if (battleChara.TargetObject == Player.Object)
		{
			return false;
		}

		if (Player.Object != null && battleChara.TargetObject?.OwnerId == Player.Object.GameObjectId)
		{
			return false;
		}

		if (Player.Object != null && battleChara.OwnerId != 0 && PartyEntityIds.Contains(battleChara.OwnerId))
		{
			return false;
		}

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

	private static readonly uint[] ZuEggNameIds = [14575, 14576];

	private static uint _attackKindsJob = uint.MaxValue;

	private static AttackKinds _attackKinds = AttackKinds.Any;

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

		// Concept 23 (A834): in a deep dungeon fight no new target that is not fighting yet.
		if (DeepDungeonCrowd.KeepsOut(battleChara))
		{
			return false;
		}

		if (battleChara.IsSpecialExceptionImmune())
		{
			return false;
		}

		if (battleChara.IsSpecialImmune())
		{
			return false;
		}

		if (battleChara.GetEventType() == EventHandlerContent.DpsChallengeDirector && Player.Object.GetEventType() != EventHandlerContent.DpsChallengeDirector)
		{
			return false;
		}

		if (Service.Config.FilterOneHpInvincible && battleChara.CurrentHp <= 1)
		{
			return false;
		}

		if ((!DataCenter.IsPvP || !Service.Config.IgnorePvPInvincibility) && battleChara.HasInvincibleStatus())
		{
			return false;
		}

		if (battleChara.HasSpikes() || battleChara.IsGuardedZuEgg() || KillOrder.IsHeld(battleChara))
		{
			return false;
		}

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

		// Concept 03, "Den Kampf eröffnen" (A879): no fight opened while a ready check waits for an answer - before every
		// priority rule and the engage setting, which would open one.
		if (PullGuard.HoldsPull(battleChara))
		{
			return false;
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

		if (battleChara.IsNamed(NPCName.BlackStar) || battleChara.IsNamed(NPCName.MythicIdol))
		{
			return true;
		}

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

		return battleChara.GetEventType() == EventHandlerContent.PublicContentDirector;
	}

	private static bool IsSoloDeepDungeonSmartAttackable(IBattleChara battleChara)
	{
		if (DataCenter.InCombat)
		{
			return battleChara.TargetObject is not null;
		}

		if (DataCenter.PartyMembers.Count > 1)
		{
			return battleChara.TargetObject is not null;
		}

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

	internal static unsafe bool IsEnemy(this IGameObject obj)
	{
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

	internal static bool IsPlayer(this IBattleChara battleChara)
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
}
