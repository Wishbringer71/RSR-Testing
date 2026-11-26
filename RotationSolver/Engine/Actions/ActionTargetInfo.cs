using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using static RotationSolver.Configuration.ConfigTypes;
using AttackType = RotationSolver.Engine.Data.AttackType;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Player = ECommons.GameHelpers.Player;

namespace RotationSolver.Engine.Actions;

/// <summary>
/// The target info
/// </summary>
/// <param name="action">the input action.</param>
public partial struct ActionTargetInfo(IBaseAction action)
{
	/// <summary>
	/// The range of this action.
	/// </summary>
	public readonly float Range => ActionBasicInfo.IsInstinctualAxe(action.Info.ID) ? 3f : ActionManager.GetActionRange(action.Info.ID);

	/// <summary>
	/// The effect range of this action.
	/// </summary>
	public readonly float EffectRange => (ActionID)action.Info.ID == ActionID.LiturgyOfTheBellPvE ? 20 : action.Action.EffectRange;

	/// <summary>
	/// Is this action single target.
	/// </summary>
	public readonly bool IsSingleTarget => action.Action.CastType == 1 && SelfAimedShape == null;

	// A channel aimed by the player's facing - Flamethrower, Apokalypsis - is typed in the sheet as aimed at the caster
	// (cast type 1, range 0, an effect range); its effect text names the shape before him. Read only for that form (A380).
	private readonly CastType? SelfAimedShape => action.Action.CastType == 1 && action.Action.EffectRange > 0
		&& action.Action.CanTargetSelf && Range == 0
		? ActionTooltip.StatedShapeBeforeYou(action.Info.ID)
		: null;
	/// <summary>
	/// Is this action target area.
	/// </summary>
	public readonly bool IsTargetArea => action.Action.TargetArea;

	/// <summary>
	/// Is this action friendly.
	/// </summary>
	public readonly bool IsTargetFriendly => action.Setting.IsFriendly;

	// The line the heal-need filter (AutoHealCheck) reads: the action's own heal ratio, unless the running heal path sets
	// its own (the downtime heal, IBaseAction.HealRatioOverride).
	private readonly float HealRatio => IBaseAction.HealRatioOverride ?? action.Config.AutoHealRatio;

	#region Targetting Behaviour

	/// <summary>
	/// Retrieves a collection of valid battle characters that can be targeted based on the specified criteria.
	/// </summary>
	/// <param name="skipStatusProvideCheck">If set to <c>true</c>, skips the status provide check.</param>
	/// <param name="skipTargetStatusNeedCheck">If set to <c>true</c>, skips the target status need check.</param>
	/// <param name="type">The type of target to filter (e.g., Heal).</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="List{IBattleChara}"/> containing the valid targets.
	/// </returns>
	private readonly List<IBattleChara> GetCanTargets(bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck, TargetType type, TargetType targetOverride, Func<IBattleChara, bool>? targetFilter = null)
	{
		if (DataCenter.AllTargets == null)
		{
			return [];
		}

		if (Player.Object == null)
		{
			return [];
		}

		if (targetOverride == TargetType.Self)
		{
			return [Player.Object];
		}

		var range = Range;
		var config = action.Config;
		var restrictedDot = config.IsRestrictedDOT;
		var timeToKill = config.TimeToKill;

		List<IBattleChara> validTargets = [];
		foreach (var target in TargetHelper.GetTargetsByRange(range, action.Setting.IsFriendly))
		{
			if (type == TargetType.Heal && target.GetHealthRatio() >= 1)
			{
				continue;
			}

			if (restrictedDot && DataCenter.RestrictedDotNameIds.Contains(target.NameId))
			{
				continue;
			}

			if (!IsInPvPPlayerRange(target))
			{
				continue;
			}

			if (!CheckStatus(target, skipStatusProvideCheck, skipTargetStatusNeedCheck) || !CheckTimeToKill(target, timeToKill) || !CheckResistance(target, range))
			{
				continue;
			}

			if (!DataCenter.IsManual || IsTargetFriendly || target.GameObjectId == Svc.Targets.Target?.GameObjectId || target.GameObjectId == Player.Object.GameObjectId)
			{
				// "Only attack targets on screen" is about attacking; a party member behind the camera is
				// still on the party list and still needs the heal.
				if ((IsTargetFriendly || TargetOnScreen(target)) && CanUseTo(target) && action.Setting.CanTarget(target) && (targetFilter?.Invoke(target) ?? true)
					&& (!config.MinHPFeature || target.GetHealthRatio() > config.MinHPPercent))
				{
					validTargets.Add(target);
				}
			}
		}

		// Figures only healing defeats (concept 07, A516) are candidates of a single heal and of nothing else: one target,
		// no effect radius and no ground target - an area heal does nothing to them -, no heal over time, and only where the
		// game lets this action reach them, which CanUseTo asks it per action.
		if (type == TargetType.Heal && action.Setting.IsFriendly && IsSingleTarget && EffectRange == 0 && !IsTargetArea
			&& DataCenter.HealToDefeatTargets.Count > 0 && !GrantsSingleHot())
		{
			foreach (var figure in DataCenter.HealToDefeatTargets.GetObjectInRadius(Range))
			{
				if (CheckStatus(figure, skipStatusProvideCheck, skipTargetStatusNeedCheck) && CanUseTo(figure)
					&& action.Setting.CanTarget(figure))
				{
					validTargets.Add(figure);
				}
			}
		}

		// Enemies whose spikes damage their attackers (concept 03, A571) are candidates only of an action that removes one of
		// their statuses - Quelling Wave on Blaze Spikes; every other action leaves them alone.
		if (!action.Setting.IsFriendly && action.Setting.TargetStatusNeed is { Length: > 0 } && DataCenter.SpikedHostiles.Count > 0
			&& !skipTargetStatusNeedCheck)
		{
			foreach (var spiked in DataCenter.SpikedHostiles.GetObjectInRadius(Range))
			{
				if (CheckStatus(spiked, skipStatusProvideCheck, skipTargetStatusNeedCheck) && CanUseTo(spiked)
					&& action.Setting.CanTarget(spiked))
				{
					validTargets.Add(spiked);
				}
			}
		}

		// In PvP an action whose text sets a crowd control prefers a target that does not nullify it; with none left it
		// still hits an immune one for its damage (concept 25, "PvP: Wechselwirkungen und Synergien", A757).
		if (DataCenter.IsPvP && !action.Setting.IsFriendly && validTargets.Count > 1 && ActionTooltip.AppliesCrowdControl(action.Info.ID))
		{
			var anyOpen = false;
			foreach (var target in validTargets)
			{
				if (!target.IsCrowdControlImmune())
				{
					anyOpen = true;
					break;
				}
			}

			if (anyOpen)
			{
				_ = validTargets.RemoveAll(static t => t.IsCrowdControlImmune());
			}
		}

		// A770: an action that turns the player to its target takes no target that would turn him into a gaze reaching him
		// (concept 13, "Nicht in einen Blick drehen").
		// A789: melee actions carry range -1 in the sheet, 0 is around oneself; an action that moves the player to its target
		// faces it until it lands, with or without NeedToFaceTarget (Thunderclap, Slither).
		var moves = action.Action.AffectsPosition;
		if (((action.Info.NeedToFaceTarget && action.Action.Range != 0) || moves) && validTargets.Count > 0)
		{
			// A786: counted from when the game runs it; an action that moves the player keeps its facing until it lands.
			var castTime = moves
				? Math.Max(action.Info.CastTime, ActionManagerEx.CurrentLockTweak?.AbilityWindow(action.ID) ?? 0f)
				: action.Info.CastTime;
			var startsIn = GazeWatch.StartsIn(action.Info.IsRealGCD);

			// A809: and the while the server keeps the facing after it goes off - its lock (a move's is in its cast time).
			var keeps = moves ? 0f : ActionManagerEx.CurrentLockTweak?.AbilityWindow(action.ID) ?? 0f;
			for (var i = validTargets.Count - 1; i >= 0; i--)
			{
				if (GazeWatch.TurnsIntoGaze(validTargets[i].Position, castTime, startsIn, moves, keeps))
				{
					validTargets.RemoveAt(i);
				}
			}
		}

		return validTargets;
	}

	/// <summary>
	/// Retrieves a list of battle characters that can be affected based on the specified criteria.
	/// </summary>
	/// <param name="skipStatusProvideCheck">If set to <c>true</c>, skips the status provide check.</param>
	/// <param name="skipTargetStatusNeedCheck">If set to <c>true</c>, skips the target status need check.</param>
	/// <param name="type">The type of target to filter (e.g., Heal).</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="List{IBattleChara}"/> containing the valid targets.
	/// </returns>
	private readonly List<IBattleChara> GetCanAffects(bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck, TargetType type, TargetType targetOverride)
	{
		if (EffectRange == 0)
		{
			return [];
		}

		if (Player.Object == null)
		{
			return [];
		}

		if ((action.Setting.IsFriendly
			? DataCenter.PartyMembers
			: DataCenter.AllHostileTargets) == null)
		{
			return [];
		}

		if (targetOverride == TargetType.Self)
		{
			return [Player.Object];
		}

		var range = Range;
		var config = action.Config;
		var restrictedDot = config.IsRestrictedDOT;
		var timeToKill = config.TimeToKill;
		var items = TargetHelper.GetTargetsByRange(range + EffectRange, action.Setting.IsFriendly);

		List<IBattleChara> validTargets = [];
		foreach (var tar in items)
		{
			if (type == TargetType.Heal && tar.GetHealthRatio() >= 1)
			{
				continue;
			}

			if ((restrictedDot && DataCenter.RestrictedDotNameIds.Contains(tar.NameId))
				|| (DataCenter.IsInM9S && action.AdjustedID == 36982 && DataCenter.RestrictedActionNameIds.Contains(tar.NameId)))
			{
				continue;
			}

			if (CheckStatus(tar, skipStatusProvideCheck, skipTargetStatusNeedCheck) && CheckTimeToKill(tar, timeToKill) && CheckResistance(tar, range)
				&& LivesUntilItLands(tar))
			{
				validTargets.Add(tar);
			}
		}
		return validTargets;
	}

	private static readonly Dictionary<ulong, (long Tick, bool OnScreen)> _onScreenCache = [];

	private static bool TargetOnScreen(IBattleChara battleChara)
	{
		if (!Service.Config.OnlyAttackInView && !Service.Config.OnlyAttackInVisionCone)
		{
			return true;
		}

		var now = Environment.TickCount64;
		var id = battleChara.GameObjectId;
		if (_onScreenCache.TryGetValue(id, out var cached) && now - cached.Tick < 15)
		{
			return cached.OnScreen;
		}

		if (_onScreenCache.Count > 512)
		{
			_onScreenCache.Clear();
		}

		var onScreen = IsOnScreen(battleChara);
		_onScreenCache[id] = (now, onScreen);
		return onScreen;
	}

	private static bool IsOnScreen(IBattleChara battleChara)
	{
		if (Service.Config.OnlyAttackInView)
		{
			if (!Svc.GameGui.WorldToScreen(battleChara.Position, out _))
			{
				return false;
			}
		}

		if (Service.Config.OnlyAttackInVisionCone && Player.Object != null)
		{
			var dir = battleChara.Position - Player.Object.Position;
			var faceVec = Player.Object.GetFaceVector();
			dir = Vector3.Normalize(dir);
			faceVec = Vector3.Normalize(faceVec);

			double dot = Vector3.Dot(faceVec, dir);
			var thresholdCos = Math.Cos(Math.PI * Service.Config.AngleOfVisionCone / 360);
			if (dot < thresholdCos)
			{
				return false;
			}
		}

		return true;
	}

	private readonly unsafe bool CanUseTo(IBattleChara tar)
	{
		if (tar == null)
		{
			return false;
		}

		if (Player.Object == null)
		{
			return false;
		}

		if (!Player.Available)
		{
			return false;
		}

		if (!(tar.GameObjectId != 0 && tar.Struct() != null))
		{
			return false;
		}

		var specialAbilityCheck = IsSpecialAbility(action.Info.ID);
		var actionmanagerCheck = ActionManager.CanUseActionOnTarget(action.Info.AdjustedID, (GameObject*)tar.Struct());

		if (!(specialAbilityCheck || actionmanagerCheck))
		{
			return false;
		}
		if (!IsTargetFriendly)
		{
			return tar.GameObjectId == Player.Object.GameObjectId || DataCenter.AllHostileTargetIds.Contains(tar.GameObjectId);
		}
		else
		{
			// Concept 03, "Sichtlinie: das Urteil des Spiels zuerst" (A621): the game decides whether the action sees the
			// target; a spot on the ground keeps RSR's ray, as nothing states how the game answers for one.
			return IsTargetArea ? tar.CanSee() : tar.InSightFor(action.Info.AdjustedID);
		}
	}

	private readonly bool IsInPvPPlayerRange(IBattleChara tar)
	{
		if (!DataCenter.IsPvP || !IsSingleTarget || tar is not IPlayerCharacter
			|| Player.Object == null || tar.GameObjectId == Player.Object.GameObjectId)
		{
			return true;
		}

		return tar.DistanceToPlayer() + Player.Object.HitboxRadius <= Range;
	}

	private static readonly HashSet<ActionID> _specialActions =
	[
		ActionID.AethericMimicryPvE,
		ActionID.EruptionPvE,
		ActionID.BishopAutoturretPvP,
		ActionID.CometPvP,
		ActionID.DotonPvE,
		ActionID.DotonPvE_18880,
		ActionID.FeatherRainPvE,
		ActionID.SaltAndDarknessPvP,
		ActionID.AvalancheAxePvE,
		ActionID.SpinningAxePvE,
		ActionID.MistralAxePvE,
		ActionID.GaleAxePvE,
	];

	private static bool IsSpecialAbility(uint iD)
	{
		return _specialActions.Contains((ActionID)iD);
	}

	private readonly bool CheckStatus(IBattleChara battleChara, bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (action.Setting.TargetStatusNeed != null && !skipTargetStatusNeedCheck && action.Config.ShouldCheckTargetStatus)
		{
			if (DataCenter.IsInOccultCrescentOp && battleChara.NameId != 0)
			{
				foreach (var id in action.Setting.TargetStatusNeed)
				{
					if (StatusHelper.IsOccultWeaknessStatus(id) && StatusHelper.HasKnownOccultWeakness(battleChara.NameId, id))
					{
						return true;
					}
				}
			}

			// As on the player side (ActionBasicInfo.IsStatusNeeded): the needed status has to last until the
			// action goes off and its cast ends, not the refresh horizon of a provided one.
			if (battleChara.WillStatusEnd(action.Info.NeededStatusMargin, action.Setting.StatusFromSelf, action.Setting.TargetStatusNeed))
			{
				return false;
			}
		}

		if (action.Setting.TargetStatusProvide != null && !skipStatusProvideCheck && action.Config.ShouldCheckStatus)
		{
			if (!NeedsTargetStatus(battleChara) || (Service.Config.Statuscap2 && StatusHelper.IsStatusCapped(battleChara)))
			{
				return false;
			}
		}

		return true;
	}

	private readonly bool NeedsTargetStatus(IBattleChara battleChara)
	{
		var setting = action.Setting;
		return Service.Config.UseDotTickTiming && setting.Dot != null
			? DotPlanner.ShouldApply(action, battleChara)
			: battleChara.WillStatusEndGCD(action.Config.StatusRefreshGcdCount, 0, setting.StatusFromSelf, setting.TargetStatusProvide);
	}

	private readonly bool CheckResistance(IBattleChara battleChara, float range)
	{
		if (battleChara == null)
		{
			return false;
		}

		// A762: each block holds only an enemy the briefing lists as immune; an interrupt goes all the same while the game
		// reports the target's cast interruptible (concept 22, "Die Kontrollfelder bedeuten Immunität").
		if (DataCenter.Job == Job.BLU && DataCenter.IsInMaskedCarnivale)
		{
			if (Service.Config.BlockflatdamagedeathimmuneBlu && action.Setting.IsFlatDamageDeath
			&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.FlatOrDeath))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to flat/death — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockslowimmuneBlu && action.Setting.IsSlowSpell
			&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Slow))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to slow — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockpetrificationimmuneBlu && action.Setting.IsPetrificationSpell
				&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Petrification))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to petrification — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockparalysisimmuneBlu && action.Setting.IsParalysisSpell
				&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Paralysis))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to paralysis — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockinterruptimmuneBlu && action.Setting.IsInterruptSpell
			&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Interruption)
			&& !(battleChara.IsCasting && battleChara.IsCastInterruptible))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to interruption — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockblindimmuneBlu && action.Setting.IsBlindSpell
				&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Blind))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to blind — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockstunimmuneBlu && action.Setting.IsStunSpell
				&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Stun))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to stun — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlocksleepimmuneBlu && action.Setting.IsSleepSpell
				&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Sleep))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to sleep — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockbindimmuneBlu && action.Setting.IsBindSpell
				&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Bind))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to bind — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockheavyimmuneBlu && action.Setting.IsHeavySpell
				&& MaskedCarnivaleHelper.IsImmuneTo(battleChara, CarnivaleControl.Heavy))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to heavy — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockimmuneBlu && !MaskedCarnivaleHelper.IsActionAspectAllowed(battleChara, action))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is immune to action {action.Info.Name} aspect (BlockimmuneBLU).");
				}

				return false;
			}

			if (Service.Config.BlocknonweakBlu && !MaskedCarnivaleHelper.HasNoResistancesOrImmunities(battleChara))
			{

				var shouldCheckWeakness = false;
				if (action.Info.Aspects != null)
				{
					for (var i = 0; i < action.Info.Aspects.Count; i++)
					{
						if ((action.Info.Aspects[i] != Aspect.Unaspected && Service.Config.AllowunaspectedBlu) || !Service.Config.AllowunaspectedBlu)
						{
							shouldCheckWeakness = true;
							break;
						}
					}
				}
				if (shouldCheckWeakness && !MaskedCarnivaleHelper.IsActionAspectWeak(battleChara, action))
				{
					return false;
				}
			}
		}

		// The Blue Mage's fixed, percentage and lethal spells and Level 5 Petrify: their effect text says they do nothing to an
		// enemy of higher level. A kind the class has been seen to do nothing to is not tried again in this session (concept
		// 01, "Gelernte Immunität", A507).
		if (DataCenter.Job == Job.BLU
			&& ((ActionTooltip.IgnoresHigherLevel(action.ID) && battleChara.Level > DataCenter.PlayerSyncedLevel())
				|| (action.Setting.IsFlatDamageDeath && OwnHitWatch.FlatOrLethalDoesNothing(battleChara))))
		{
			return false;
		}

		// "Enemy level must be a multiple of 5" (Level 5 Death, Level 5 Petrify): the effect text states the multiple (A507).
		if (DataCenter.Job == Job.BLU && ActionTooltip.TryLevelMultiple(action.ID, out var multiple) && battleChara.Level % multiple != 0)
		{
			return false;
		}

		try
		{
			if (DataCenter.IsPvP && !action.Setting.IgnoreGuard)
			{
				// Both rows of Guard (A756); an action whose text says it ignores, removes or halves Guard may choose the target
				// (concept 25, "PvP: Ziele unter Guard und Unverwundbarkeit", A757).
				if (battleChara.HasStatus(false, StatusHelper.PvPGuardStatuses)
					&& !battleChara.WillStatusEnd((float)action.Info.CastTime, false, StatusHelper.PvPGuardStatuses)
					&& !ActionTooltip.HandlesGuard(action.Info.ID))
				{
					return false;
				}
			}
			if (action.Info.HasAttackType(AttackType.Magic))
			{
				if (battleChara.HasStatus(false, StatusHelper.MagicResistance))
				{
					return false;
				}
			}
			if (action.Info.HasAttackType(AttackType.Physical) || action.Info.HasAttackType(AttackType.Slashing) || action.Info.HasAttackType(AttackType.Piercing) || action.Info.HasAttackType(AttackType.Blunt)) // Physical
			{
				if (battleChara.HasStatus(false, StatusHelper.PhysicalResistance))
				{
					return false;
				}
			}
			if (range >= 20) // Range
			{
				if (battleChara.HasStatus(false, StatusID.EnergyField))
				{
					return false;
				}
			}
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Error checking resistance: {ex.Message}");
			return false;
		}

		return true;
	}

	/// <summary>
	/// The order in which this action spreads its damage over time, or null when it is not spreading one.
	/// </summary>
	/// <remarks>
	/// The owner's rule (16.10.2026): "bei der dot-verteilung sollte immer geschaut werden, welcher gegner die
	/// aktuell höchsten hp hat, so dass der maximale effekt beim schaden erzielt werden kann. danach immer der
	/// reihe nach wie die dots auslaufen." First the candidate whose own damage over time runs out first - one
	/// without it counts as run out, one just sent as never - and among equals the one with the most HP left,
	/// so a fresh pack gets it highest HP first and a dotted one has it renewed in the order it falls off
	/// (concept 03, "Wahl des DoT-Ziels", A295). The candidates are already those that need it
	/// (<see cref="CheckStatus"/>), so the order applies only where the status check chose them: a rotation
	/// that skips it uses the action for something else (a combo finisher, an upkeep while moving), and an
	/// explicit target override or command keeps its own choice. Damage over time is read from the status's
	/// own text (<see cref="StatusHelper.IsDamageOverTime"/>); a debuff such as Death's Design belongs on the
	/// enemy being attacked and is not spread this way.
	/// </remarks>
	private readonly Comparison<IBattleChara>? DotSpreadOrder(bool skipStatusProvideCheck, TargetType targetOverride, TargetingType? commandTargeting)
	{
		var provides = action.Setting.TargetStatusProvide;
		if (!Service.Config.DotTargetByHpThenExpiry || action.Setting.IsFriendly || provides == null
			|| skipStatusProvideCheck || !action.Config.ShouldCheckStatus || targetOverride != default
			|| commandTargeting.HasValue || !StatusHelper.IsDamageOverTime(provides))
		{
			return null;
		}

		StatusID[] statuses = provides;
		var fromSelf = action.Setting.StatusFromSelf;
		return (a, b) =>
		{
			var cmp = a.StatusTime(fromSelf, statuses).CompareTo(b.StatusTime(fromSelf, statuses));
			return cmp != 0 ? cmp : b.CurrentHp.CompareTo(a.CurrentHp);
		};
	}

	private readonly bool CheckTimeToKill(IBattleChara battleChara, float timeToKill)
	{
		if (battleChara == null)
		{
			return false;
		}

		// An attack question, and only an attack question: "is this target going to live long enough
		// for the cast to be worth it". Asked of a friendly target it inverts into nonsense - the
		// party member closest to dying would be the one dropped from the candidate list, which is
		// exactly the one a heal is for.
		//
		// The condition is stated outright because the accident that used to cover it is gone. It
		// once read right for the wrong reason: RecordedHP held hostiles only, so GetTTK returned
		// NaN for every party member and the NaN branch let them through. Since A91 the history
		// carries the party as well (TargetUpdater fills both), so GetTTK answers for members - and
		// without this line the member closest to dying would be dropped from the heal candidates,
		// which is precisely the one the heal is for.
		if (action.Setting.IsFriendly)
		{
			return true;
		}

		if (timeToKill <= 0)
		{
			return true;
		}

		var time = battleChara.GetTTK();
		return float.IsNaN(time) || time >= timeToKill;
	}

	#endregion
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public enum TargetType : byte
{
	Big,
	Small,
	HighHP,
	LowHP,
	HighMaxHP,
	LowMaxHP,
	Interrupt,
	Provoke,
	Death,
	Dispel,
	Move,
	FriendMove,
	BeAttacked,
	Heal,
	Tank,
	Melee,
	Range,
	Physical,
	Magical,
	Self,
	DancePartner,
	MimicryTarget,
	TheBalance,
	TheSpear,
	Kardia,
	Deployment,
	PhantomBell,
	PhantomRespite,
	DarkCannon,
	ShockCannon,
	Farthest,
	Nearest,
	PvPHealers,
	PvPTanks,
	PvPDPS,
	HighHPPercent,
	LowHPPercent,
	Tankbuster,

	// Unused, and dropped upstream in 7.5.7.5; kept so TankSwap keeps its value for rotations built against this package.
	Capture,
	TankSwap,

	// The member a heal over time against an enemy's damage over time goes on (A432); last, so every value before it stays.
	HotAgainstDot
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// The target result
/// </summary>
/// <param name="Target">the target.</param>
/// <param name="AffectedTargets">the targets that be affected by this action.</param>
/// <param name="Position">the position to use this action.</param>
public readonly record struct TargetResult(IBattleChara Target, IBattleChara[] AffectedTargets, Vector3? Position);
