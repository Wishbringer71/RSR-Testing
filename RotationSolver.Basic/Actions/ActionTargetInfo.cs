using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Rotations.Duties;
using static RotationSolver.Basic.Configuration.ConfigTypes;
using AttackType = RotationSolver.Basic.Data.AttackType;
using CombatRole = RotationSolver.Basic.Data.CombatRole;
using Player = ECommons.GameHelpers.Player;

namespace RotationSolver.Basic.Actions;

/// <summary>
/// The target info
/// </summary>
/// <param name="action">the input action.</param>
public struct ActionTargetInfo(IBaseAction action)
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
	public readonly bool IsSingleTarget => action.Action.CastType == 1;
	/// <summary>
	/// Is this action target area.
	/// </summary>
	public readonly bool IsTargetArea => action.Action.TargetArea;

	/// <summary>
	/// Is this action friendly.
	/// </summary>
	public readonly bool IsTargetFriendly => action.Setting.IsFriendly;

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
	private readonly List<IBattleChara> GetCanTargets(bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck, TargetType type, TargetType targetOverride)
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

		List<IBattleChara> validTargets = [];
		foreach (var target in TargetHelper.GetTargetsByRange(Range, action.Setting.IsFriendly))
		{
			if (type == TargetType.Heal && target.GetHealthRatio() >= 1)
			{
				continue;
			}

			if (IsRestrictedDotTarget(target))
			{
				continue;
			}

			if (!IsInPvPPlayerRange(target))
			{
				continue;
			}

			if (!CheckStatus(target, skipStatusProvideCheck, skipTargetStatusNeedCheck) || !CheckTimeToKill(target) || !CheckResistance(target))
			{
				continue;
			}

			// If auto targeting is enabled
			// or the skill is friendly
			// or the target is the currently selected hard-target
			// or the target is ourselves
			// then => Check target is in the view && ActionManager.CanUse on target && CanSee Target && action.setting predicate is true of target
			if (!DataCenter.IsManual || IsTargetFriendly || target.GameObjectId == Svc.Targets.Target?.GameObjectId || target.GameObjectId == Player.Object.GameObjectId)
			{
				// "Only attack targets on screen" is about attacking; a party member behind the camera is
				// still on the party list and still needs the heal.
				if ((IsTargetFriendly || TargetOnScreen(target)) && CanUseTo(target) && action.Setting.CanTarget(target)
					&& (!action.MinHPFeature || target.GetHealthRatio() > action.MinHPPercent))
				{
					validTargets.Add(target);
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

		IEnumerable<IBattleChara> items = TargetHelper.GetTargetsByRange(Range + EffectRange, action.Setting.IsFriendly);

		List<IBattleChara> validTargets = [];
		foreach (var tar in items)
		{
			if (type == TargetType.Heal && tar.GetHealthRatio() >= 1)
			{
				continue;
			}

			if (IsRestrictedDotTarget(tar)
				|| (DataCenter.IsInM9S && action.AdjustedID == 36982 && DataCenter.RestrictedActionNameIds.Contains(tar.NameId)))
			{
				continue;
			}

			if (CheckStatus(tar, skipStatusProvideCheck, skipTargetStatusNeedCheck) && CheckTimeToKill(tar) && CheckResistance(tar))
			{
				validTargets.Add(tar);
			}
		}
		return validTargets;
	}

	private readonly bool IsRestrictedDotTarget(IBattleChara target)
	{
		return action.IsRestrictedDOT && DataCenter.RestrictedDotNameIds.Contains(target.NameId);
	}

	private static readonly Dictionary<ulong, (long Tick, bool OnScreen)> _onScreenCache = [];

	// Same answer for every action this frame, so it is cached per target.
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

			// Compare cosines instead of acos
			double dot = Vector3.Dot(faceVec, dir);
			var thresholdCos = Math.Cos(Math.PI * Service.Config.AngleOfVisionCone / 360);
			if (dot < thresholdCos)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Determines whether the specified game object can be targeted and used for an action.
	/// </summary>
	/// <param name="tar">The game object to check.</param>
	/// <returns>
	/// <c>true</c> if the game object can be targeted and used for an action; otherwise, <c>false</c>.
	/// </returns>
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
		if (!IsTargetFriendly) // canSee check is already done as part of finding hostile targets
		{
			return tar.GameObjectId == Player.Object.GameObjectId || DataCenter.AllHostileTargetIds.Contains(tar.GameObjectId);
		}
		else // For friendly targets we still need to check CanSee
		{
			return tar.CanSee();
		}
	}

	// In PvP zones the game leaves the caster's hitbox out of the range check for single-target actions on other players.
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

	/// <summary>
	/// Checks the status of the specified game object to determine if it meets the criteria for the action.
	/// </summary>
	/// <param name="battleChara">The game object to check.</param>
	/// <param name="skipStatusProvideCheck">If set to <c>true</c>, skips the status provide check.</param>
	/// <param name="skipTargetStatusNeedCheck">If set to <c>true</c>, skips the target status need check.</param>
	/// <returns>
	/// <c>true</c> if the game object meets the status criteria for the action; otherwise, <c>false</c>.
	/// </returns>
	private readonly bool CheckStatus(IBattleChara battleChara, bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (action.Setting.TargetStatusNeed != null && !skipTargetStatusNeedCheck)
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

		// ShouldCheckStatus is the user's "does this action check status effects" toggle. It belongs on
		// the provide side only: that check skips the action because the status it grants is already
		// there, which is what the option is meant to switch off. The need side is a precondition, not
		// redundancy, so it stays in force either way.
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

	/// <summary>
	/// Checks the resistance status of the specified game object to determine if it meets the criteria for the action.
	/// </summary>
	/// <param name="battleChara">The game object to check.</param>
	/// <returns>
	/// <c>true</c> if the game object meets the resistance criteria for the action; otherwise, <c>false</c>.
	/// </returns>
	private readonly bool CheckResistance(IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}
		if (battleChara.StatusList == null)
		{
			return false;
		}

		if (DataCenter.Job == Job.BLU && DataCenter.IsInMaskedCarnivale)
		{
			if (Service.Config.BlockflatdamagedeathimmuneBlu && action.Setting.IsFlatDamageDeath
			&& !MaskedCarnivaleHelper.IsVulnerableToFlatOrDeath(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to flat/death — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockslowimmuneBlu && action.Setting.IsSlowSpell
			&& !MaskedCarnivaleHelper.IsVulnerableToSlow(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to slow — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockpetrificationimmuneBlu && action.Setting.IsPetrificationSpell
				&& !MaskedCarnivaleHelper.IsVulnerableToPetrification(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to petrification — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockparalysisimmuneBlu && action.Setting.IsParalysisSpell
				&& !MaskedCarnivaleHelper.IsVulnerableToParalysis(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to paralysis — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockinterruptimmuneBlu && action.Setting.IsInterruptSpell
			&& !MaskedCarnivaleHelper.IsVulnerableToInterruption(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to interruption — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockblindimmuneBlu && action.Setting.IsBlindSpell
				&& !MaskedCarnivaleHelper.IsVulnerableToBlind(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to blind — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockstunimmuneBlu && action.Setting.IsStunSpell
				&& !MaskedCarnivaleHelper.IsVulnerableToStun(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to stun — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlocksleepimmuneBlu && action.Setting.IsSleepSpell
				&& !MaskedCarnivaleHelper.IsVulnerableToSleep(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to sleep — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockbindimmuneBlu && action.Setting.IsBindSpell
				&& !MaskedCarnivaleHelper.IsVulnerableToBind(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to bind — skipping {action.Info.Name}.");
				}

				return false;
			}

			if (Service.Config.BlockheavyimmuneBlu && action.Setting.IsHeavySpell
				&& !MaskedCarnivaleHelper.IsVulnerableToHeavy(battleChara))
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[CheckResistance] {battleChara.Name} is not vulnerable to heavy — skipping {action.Info.Name}.");
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

				// shouldCheckWeakness is true if the action has at least one non-unaspected aspect
				// (or any aspect when AllowunaspectedBlu is off).
				// When AllowunaspectedBlu is on, purely unaspected actions leave this false,
				// bypassing BlocknonweakBlu so they are always allowed regardless of mob weakness.
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

		try
		{
			if (DataCenter.IsPvP && !action.Setting.IgnoreGuard)
			{
				if (battleChara.HasStatus(false, StatusID.Guard) && !battleChara.WillStatusEnd((float)action.Info.CastTime, false, StatusID.Guard))
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
			if (Range >= 20) // Range
			{
				if (battleChara.HasStatus(false, StatusID.EnergyField))
				{
					return false;
				}
			}
		}
		catch (Exception ex)
		{
			// Log the exception for debugging purposes
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

	/// <summary>
	/// Checks the time to kill of the specified game object to determine if it meets the criteria for the action.
	/// </summary>
	/// <param name="battleChara">The game object to check.</param>
	/// <returns>
	/// <c>true</c> if the game object meets the time to kill criteria for the action; otherwise, <c>false</c>.
	/// </returns>
	private readonly bool CheckTimeToKill(IBattleChara battleChara)
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

		var time = battleChara.GetTTK();
		return float.IsNaN(time) || time >= action.Config.TimeToKill;
	}

	#endregion

	#region Target Result

	/// <summary>
	/// Finds the target based on the specified criteria.
	/// </summary>
	/// <param name="skipAoeCheck">If set to <c>true</c>, skips the AoE check.</param>
	/// <param name="skipStatusProvideCheck">If set to <c>true</c>, skips the status provide check.</param>
	/// <param name="skipTargetStatusNeedCheck">If set to <c>true</c>, skips the target status need check.</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="TargetResult"/> containing the target and affected characters, or <c>null</c> if no target is found.
	/// </returns>
	internal readonly TargetResult? FindTarget(bool skipAoeCheck, bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck, TargetType targetOverride)
	{
		if (action == null || action.Setting == null || action.Config == null)
		{
			return null;
		}

		if (Player.Object == null)
		{
			return null;
		}

		if (Range == 0 && EffectRange == 0)
		{
			return new TargetResult(Player.Object, [], Player.Object.Position);
		}

		// The character teleports to the position of a previously-placed in-world object. Resolve that object's position and
		// validate it for safety.
		if (action.Setting.SpecialType == SpecialActionType.ObjectBasedMovement)
		{
			if (ShouldCheckMoveSafety() && action.Setting.ObjectBasedMovementObjectOID != 0)
			{
				if (!ObjectHelper.TryGetOwnedObjectPosition(action.Setting.ObjectBasedMovementObjectOID, out var position))
				{
					return null;
				}

				if (!CheckMovementSafety(position))
				{
					return null;
				}

				return new TargetResult(Player.Object, [], position);
			}
		}

		var type = action.Setting.TargetType;

		// For self-cast area/cone hostile actions (Range == 0, EffectRange > 0), such as Surpanakha,
		// the attack fires a cone from the player's position toward the current target direction.
		// There is no explicit enemy to "target" at range 0, so bypass the normal target-finding
		// path and source candidates directly from GetCanAffects (enemies within EffectRange).
		if (Range == 0 && EffectRange > 0 && !IsSingleTarget && !IsTargetArea && !action.Setting.IsFriendly)
		{
			if (Service.Config.AoEType == AoEType.Off)
			{
				return null;
			}

			var required = skipAoeCheck ? 0 : action.Config.AoeCount;

			// Mirror the cleave check in GetMostCanTargetObjects: in Cleave mode, block if aoeCount > 1
			if (required > 1 && (Service.Config.AoEType == AoEType.Cleave || (DataCenter.IsInM9S && Service.Config.M9SCleaveOnly)))
			{
				return null;
			}

			var selfAffects = GetCanAffects(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);
			// For directional self-cast AOEs (Cone, StraightLine), only count enemies that are
			// actually within the player's current facing direction. Without this, the action would
			// fire even when no enemies are in the cone (e.g. Breath of Magic spamming infinitely).
			selfAffects = FilterAffectsByFacing(selfAffects);
			if (selfAffects.Count < Math.Max(1, required))
			{
				return null;
			}

			return new TargetResult(Player.Object, [.. selfAffects], Player.Object.Position);
		}

		// The friendly counterpart: an area heal centred on the caster (Medica, Helios, Succor, Lux
		// Solaris and the rest - Range 0, an effect radius, no ground target). The general path below
		// looks for a heal *target* within Range, and with Range 0 that is the caster plus whoever's
		// hitbox touches theirs; the candidate then has to be under the action's own heal ratio. So a
		// healer standing full and a few yalms from the party never cast it, however hurt the party
		// was - the need was measured on the anchor instead of on the people the heal lands on.
		//
		// Here the anchor is the caster, always, and the need is read where the heal lands: the
		// hurt members inside the effect radius (GetCanAffects drops the full ones for a heal, as it
		// does for the general path), at least AoeCount of them, and at least one of those under
		// the heal ratio when the auto-heal check is on - the same two tests the general path applies,
		// moved from the anchor to the affected. A member held for a death trigger, or carried through
		// Walking Dead by his own attacks, is healed if he stands in the radius - an area heal cannot
		// skip him - but he never counts as the reason.
		//
		// Only for the heal target type, and not when a caller names the caster outright
		// (targetOverride Self), which the general path answers with the caster unconditionally. A
		// friendly Range-0 action asked for anything else keeps the general path, because only the
		// heal question depends on who needs it. The global AoE type (Off, Cleave, Full) does not
		// apply: it is about attacks (see GetMostCanTargetObjects). The action's own AoeCount does.
		if (Range == 0 && EffectRange > 0 && !IsSingleTarget && !IsTargetArea && action.Setting.IsFriendly
			&& type == TargetType.Heal && targetOverride != TargetType.Self)
		{
			// The dead and those who cannot be healed stay out, as GeneralHealTarget keeps them out:
			// a corpse reads 0 health, so it would count towards AoeCount and pass the heal ratio on
			// its own, and the cast would go to whoever stands next to it.
			var inRadius = GetCanAffects(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);
			inRadius.RemoveAll(member => member.IsDead || member.HasStatus(false, StatusHelper.HealingIneffectiveStatus));
			var required = skipAoeCheck ? 1 : Math.Max(1, (int)action.Config.AoeCount);
			if (inRadius.Count < required)
			{
				DataCenter.LastSelfCentredHeal = new(action.Name, inRadius.Count, required, false, DateTime.Now);
				return null;
			}

			var anyInNeed = false;
			foreach (var member in inRadius)
			{
				if (member.IsHeldForDeathTrigger() || member.WalkingDeadCarriedBySelfHeal(out _))
				{
					continue;
				}

				if (!IBaseAction.AutoHealCheck || member.GetForecastHealthRatio(InstantHeal) < action.Config.AutoHealRatio)
				{
					anyInNeed = true;
					break;
				}
			}

			DataCenter.LastSelfCentredHeal = new(action.Name, inRadius.Count, required, anyInNeed, DateTime.Now);
			return anyInNeed ? new TargetResult(Player.Object, [.. inRadius], Player.Object.Position) : null;
		}

		IEnumerable<IBattleChara> canTargets = GetCanTargets(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);
		var canAffects = GetCanAffects(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);

		// A Walking Dead bearer who is carried by his own attacks is supported with a HoT only, the
		// owner's rule (concept 09): a heal action that grants no HoT does not take him as its target
		// until StatusHelper.WalkingDeadCarriedBySelfHeal releases him. Here rather than in
		// FindHealTarget, because only this layer knows which action is asking, and FindTargetByType
		// is public and has no action to ask.
		if (type == TargetType.Heal && canTargets is List<IBattleChara> healCandidates && !GrantsSingleHot())
		{
			_ = healCandidates.RemoveAll(member => member.WalkingDeadCarriedBySelfHeal(out _));
		}

		if (canTargets == null || canAffects == null)
		{
			return null;
		}

		// If AoE is disabled, block all hostile non-single-target actions (including ground-targeted)
		if (!action.Setting.IsFriendly && Service.Config.AoEType == AoEType.Off && !IsSingleTarget)
		{
			return null;
		}

		var commandTarget = DataCenter.GetCommandTarget(action);
		if (commandTarget is { ObjectId: not 0 } objectTarget
			&& FindCommandObjectTarget(objectTarget.ObjectId, canAffects, skipStatusProvideCheck, skipTargetStatusNeedCheck) is { } commandResult)
		{
			return commandResult;
		}

		if (IsTargetArea)
		{
			return FindTargetArea(canTargets, canAffects, Range, Player.Object, targetOverride);
		}

		List<IBattleChara> targetsList = [.. GetMostCanTargetObjects(canTargets, canAffects, skipAoeCheck ? 0 : action.Config.AoeCount)];

		var commandTargeting = commandTarget?.TargetingType;
		var dotOrder = DotSpreadOrder(skipStatusProvideCheck, targetOverride, commandTargeting);
		var target = FindTargetByType(targetsList, type, action.Config.AutoHealRatio, action.Setting.SpecialType, targetOverride, action.Setting.IsFriendly, InstantHeal, commandTargeting, dotOrder);
		if (target == null)
		{
			return null;
		}

		// For non-area movement actions, validate the destination position with BossModReborn IPC
		if (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position, target))
		{
			return null;
		}

		return new TargetResult(target, [.. GetAffectsTarget(target, canAffects)], target.Position);
	}

	// User-picked target: skips RSR's preference filters, only checks the action can land on it.
	private readonly TargetResult? FindCommandObjectTarget(ulong objectId, IEnumerable<IBattleChara> canAffects, bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck)
	{
		if (Player.Object == null || Svc.Objects.SearchById(objectId) is not IBattleChara target)
		{
			return null;
		}

		if (IsTargetArea)
		{
			if (Vector3.Distance(Player.Object.Position, target.Position) > Range
				|| (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position)))
			{
				return null;
			}

			return new TargetResult(target, [.. GetAffectsVector(target.Position, canAffects)], target.Position);
		}

		if (target.DistanceToPlayer() > Range
			|| !IsInPvPPlayerRange(target)
			|| !CanUseTo(target)
			|| !CheckStatus(target, skipStatusProvideCheck, skipTargetStatusNeedCheck)
			|| !action.Setting.CanTarget(target)
			|| (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position, target)))
		{
			return null;
		}

		return new TargetResult(target, [.. GetAffectsTarget(target, canAffects)], target.Position);
	}

	/// <summary>
	/// Finds the target area based on the specified criteria.
	/// </summary>
	/// <param name="canTargets">The potential targets that can be affected.</param>
	/// <param name="canAffects">The potential characters that can be affected.</param>
	/// <param name="range">The range within which to find the target area.</param>
	/// <param name="player">The player character.</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="TargetResult"/> containing the target area and affected characters, or <c>null</c> if no target area is found.
	/// </returns>
	private readonly TargetResult? FindTargetArea(IEnumerable<IBattleChara> canTargets, IEnumerable<IBattleChara> canAffects,
	float range, IPlayerCharacter player, TargetType targetOverride)
	{
		if (player == null || canTargets == null || canAffects == null)
		{
			return null;
		}

		if (action.Setting.TargetType == TargetType.Move)
		{
			return FindTargetAreaMove(range, targetOverride);
		}
		else
		{
			if (action.Setting.IsFriendly)
			{
				if (!Service.Config.UseGroundBeneficialAbility)
				{
					return null;
				}
				else
				{
					if (!Service.Config.UseGroundBeneficialAbilityWhenMoving && DataCenter.IsMoving)
					{
						return null;
					}
					else
					{
						return FindTargetAreaFriend(range, canAffects, player, targetOverride);
					}
				}
			}
			else
			{
				return FindTargetAreaHostile(canTargets, canAffects, action.Config.AoeCount, targetOverride);
			}
		}
	}

	/// <summary>
	/// Finds the hostile target area based on the specified criteria.
	/// </summary>
	/// <param name="canTargets">The potential targets that can be affected.</param>
	/// <param name="canAffects">The potential characters that can be affected.</param>
	/// <param name="aoeCount">The number of targets to consider for AoE.</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="TargetResult"/> containing the target and affected characters, or <c>null</c> if no target is found.
	/// </returns>
	private readonly TargetResult? FindTargetAreaHostile(IEnumerable<IBattleChara> canTargets, IEnumerable<IBattleChara> canAffects, int aoeCount, TargetType targetOverride)
	{
		if (canAffects == null || canTargets == null)
		{
			return null;
		}

		List<IBattleChara> tiedAnchors = [.. GetMostCanTargetObjects(canTargets, canAffects, aoeCount)];
		if (tiedAnchors.Count == 0)
		{
			return null;
		}

		// Anchors can tie on hit count. Resolve the tie the same way the target-based AoE path does,
		// so a ground-targeted AoE anchors on the same enemy a target-based one would pick. Falling
		// back to the first tied anchor keeps the old guarantee that a non-empty tie always yields one.
		var target = FindTargetByType(tiedAnchors, action.Setting.TargetType, action.Config.AutoHealRatio,
			action.Setting.SpecialType, targetOverride, false) ?? tiedAnchors[0];

		List<IBattleChara> affectedTargets = [];
		foreach (var t in canAffects)
		{
			if (Vector3.Distance(target.Position, t.Position) - t.HitboxRadius <= EffectRange)
			{
				affectedTargets.Add(t);
			}
		}

		var affectedTargetsArray = new IBattleChara[affectedTargets.Count];
		for (var i = 0; i < affectedTargets.Count; i++)
		{
			affectedTargetsArray[i] = affectedTargets[i];
		}
		return new TargetResult(target, affectedTargetsArray, target.Position);
	}

	/// <summary>
	/// Finds the target area for movement based on the specified range.
	/// </summary>
	/// <param name="range">The range within which to find the target area.</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="TargetResult"/> containing the target area and affected characters, or <c>null</c> if no target area is found.
	/// </returns>
	private readonly TargetResult? FindTargetAreaMove(float range, TargetType targetOverride)
	{
		if (Player.Object == null)
		{
			return null;
		}

		if (Service.Config.MoveAreaActionFarthest)
		{
			var pPosition = Player.Object.Position;
			if (Service.Config.MoveTowardsScreenCenter)
			{
				unsafe
				{
					var cameraManager = CameraManager.Instance();
					if (cameraManager == null)
					{
						return null;
					}

					var camera = cameraManager->CurrentCamera;
					var tar = camera->LookAtVector - camera->Object.Position;
					tar.Y = 0;
					var length = ((Vector3)tar).Length();
					if (length == 0)
					{
						return null;
					}

					tar = tar / length * range;
					var dest1 = new Vector3(pPosition.X + tar.X, pPosition.Y, pPosition.Z + tar.Z);
					if (ShouldCheckMoveSafety() && !CheckMovementSafety(dest1))
					{
						return null;
					}

					return new TargetResult(Player.Object, [], dest1);
				}
			}
			else
			{
				var rotation = Player.Object.Rotation;
				var dest2 = new Vector3(pPosition.X + ((float)Math.Sin(rotation) * range), pPosition.Y, pPosition.Z + ((float)Math.Cos(rotation) * range));
				if (ShouldCheckMoveSafety() && !CheckMovementSafety(dest2))
				{
					return null;
				}

				return new TargetResult(Player.Object, [], dest2);
			}
		}
		else
		{
			List<IBattleChara> availableCharas = [];
			foreach (var availableTarget in DataCenter.AllTargets)
			{
				if (availableTarget.GameObjectId != Player.Object.GameObjectId)
				{
					availableCharas.Add(availableTarget);
				}
			}

			var targetList = TargetFilter.GetObjectInRadius(availableCharas, range);
			var target = FindTargetByType(targetList, TargetType.Move, action.Config.AutoHealRatio, action.Setting.SpecialType, targetOverride, true);
			if (target == null)
			{
				return null;
			}

			if (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position))
			{
				return null;
			}

			return new TargetResult(target, [], target.Position);
		}
	}

	// An off-GCD heal lands as soon as the animation lock lets it go out, so its heal-ahead reading
	// looks no further than that; a GCD heal looks ahead by the rest of the GCD and its cast (A213).
	// An off-GCD heal that lands with the next GCD looks ahead like the GCD (A221).
	private readonly bool InstantHeal => !action.Info.IsRealGCD && !action.Setting.HealsWithNextGcd;

	/// <summary>
	/// Determines whether the movement safety check should be performed for the current action.
	/// </summary>
	/// <returns><c>true</c> if the movement safety check should be performed; otherwise, <c>false</c>.</returns>
	public readonly bool ShouldCheckMoveSafety()
		=> (IsMovingSpecialType(action.Setting.SpecialType))
		   && !action.Config.SkipPositionSafetyCheck && Service.Config.BmrSafetyCheckAuto;

	/// <summary>
	/// Returns true for any <see cref="SpecialActionType"/> that physically moves the character,
	/// regardless of whether the movement is a pure repositioning action or an attack with built-in movement.
	/// The one list: the action settings window reads it too, so a new movement type cannot be checked
	/// here and lack its "skip safety check" switch there (it did, for the backstep attacks of A193).
	/// </summary>
	internal static bool IsMovingSpecialType(SpecialActionType type)
		=> type is SpecialActionType.FixedDistanceMoveForward
				or SpecialActionType.FixedDistanceMoveBackward
				or SpecialActionType.HostileMovingForward
				or SpecialActionType.FriendlyMovingForward
				or SpecialActionType.HostileFriendlyMovingForward
				or SpecialActionType.HostileMovingAttack
				or SpecialActionType.ObjectBasedMovement
				or SpecialActionType.HostileAttackBackstep;

	/// <summary>
	/// Checks if a movement destination is safe using the appropriate BMR safety function for the movement type.
	/// </summary>
	/// <param name="destination">The destination position.</param>
	/// <param name="target">Optional target for target-based movement types.</param>
	/// <returns><c>true</c> if the movement is safe; otherwise, <c>false</c>.</returns>
	private readonly bool CheckMovementSafety(Vector3 destination, IBattleChara? target = null)
	{
		if (Player.Object == null)
		{
			return false;
		}

		var playerPos = Player.Object.Position;
		var specialType = action.Setting.SpecialType;

		switch (specialType)
		{
			case SpecialActionType.FixedDistanceMoveForward:
			case SpecialActionType.FixedDistanceMoveBackward:
			{
				// The game moves the character along its facing by the distance the effect text states ("Quickly
				// dash 15 yalms forward"). The target is the player himself, so the destination handed in is where he
				// stands; the landing point is computed here (A313). Without a known distance there is nothing to
				// measure: refused while BossModReborn could answer, as before, allowed without it.
				var distance = action.Setting.BackstepDistance;
				if (distance <= 0f)
				{
					return DataCenter.BMRIsFixedDashSafe == null
						|| Refused("the action's distance is not known, so its landing point is not either", measured: false);
				}

				var facing = Player.Object.Rotation;
				var along = specialType == SpecialActionType.FixedDistanceMoveBackward ? -distance : distance;
				var landingPoint = new Vector3(playerPos.X + (MathF.Sin(facing) * along), playerPos.Y,
					playerPos.Z + (MathF.Cos(facing) * along));
				return DataCenter.IsFixedDashSafe(playerPos, landingPoint)
					|| Refused("the landing point is in a danger zone");
			}

			case SpecialActionType.HostileMovingForward:
			case SpecialActionType.FriendlyMovingForward:
			case SpecialActionType.HostileFriendlyMovingForward:
			case SpecialActionType.HostileMovingAttack:
				// Target-based dash: use IsDashSafe with calculated destination (stopped at target hitbox)
				if (target == null)
				{
					return Refused("no target to measure the dash against", measured: false);
				}

				// Standing at the target, the dash does not move the player, and there is nothing
				// for this check to decide. Owner's report, Summoner: "wenn der beschwörer bereits
				// beim boss steht (0 yalm), dann wäre der gapcloser nur noch damage und kein risiko".
				// Asking whether the line to the target is safe measured a path that is never taken,
				// and an area under the boss withheld the ability from a player already standing in
				// it. Where he stands is a different question, and refusing the action does not
				// answer it: he is already there.
				//
				// 0 yalms means hitbox to hitbox, the distance the game shows (StandsAtTarget). This
				// used to exempt only a player whose centre was inside the target's ring, and kept
				// measuring a path shorter than his own hitbox when the rings merely touched. The
				// Summoner's fallback block reads the same measure. As soon as any distance remains,
				// the path to the hitbox edge is measured; how far a player may run up at all is the
				// rotation's own setting (for the Summoner AddCrimsonCyclone and CrimsonCycloneDistance).
				if (StandsAtTarget(target))
				{
					return true;
				}

				var toTarget = target.Position - playerPos;
				var direction = toTarget / toTarget.Length();
				var finalDestination = target.Position - direction * target.HitboxRadius;
				return DataCenter.IsDashSafe(playerPos, finalDestination)
					|| Refused($"the dash to {target.Name} crosses a danger zone ({target.DistanceToPlayer():F1} y)");

			case SpecialActionType.ObjectBasedMovement:
				// Object-based movement: use IsDashSafe from player to object
				return DataCenter.IsDashSafe(playerPos, destination)
					|| Refused("the way to the placed object crosses a danger zone");

			case SpecialActionType.HostileAttackBackstep:
				// The attack itself does not move the player towards the target; the backstep carries
				// him away from it, and that landing point is what can put him into an area. The
				// target is the reference for the direction: facing it is how the attack is made.
				if (target == null)
				{
					return Refused("no target to measure the backstep against", measured: false);
				}

				var away = playerPos - target.Position;
				away.Y = 0;
				var length = away.Length();
				if (length <= 0f)
				{
					return Refused("standing on the target, the backstep has no direction", measured: false);
				}

				var landing = playerPos + (away / length * action.Setting.BackstepDistance);
				return DataCenter.IsFixedDashSafe(playerPos, landing)
					|| Refused($"the backstep from {target.Name} lands in a danger zone");

			default:
				return true;
		}
	}

	/// <summary>Whether this action grants one of the single-target HoTs in <see cref="StatusHelper.SingleHots"/>.</summary>
	private readonly bool GrantsSingleHot()
	{
		var provides = action.Setting.TargetStatusProvide;
		if (provides == null)
		{
			return false;
		}

		foreach (var status in provides)
		{
			if (Array.IndexOf(StatusHelper.SingleHots, status) >= 0)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether the player stands at <paramref name="target"/>: 0 yalms hitbox to hitbox, the distance
	/// the game shows. A gap closer onto such a target does not move the player.
	/// </summary>
	internal static bool StandsAtTarget(IBattleChara target) => target.DistanceToPlayer() <= 0;

	/// <summary>
	/// Records why the movement safety check withheld this action, for the diagnostics window, and
	/// answers <c>false</c>.
	/// </summary>
	private readonly bool Refused(string why, bool measured = true)
	{
		// Kept apart: a refusal without a target can repeat every frame (FindTargetAreaMove, TODO) and
		// would otherwise hide the refusal of a dash that was actually measured.
		var refusal = new DataCenter.MoveSafetyRefusal(action.Name, why, DateTime.Now);
		if (measured)
		{
			DataCenter.LastMoveSafetyRefusal = refusal;
		}
		else
		{
			DataCenter.LastMoveSafetyUnmeasured = refusal;
		}

		return false;
	}

	/// <summary>
	/// Finds the target area for friendly actions based on the specified range and strategy.
	/// </summary>
	/// <param name="range">The range within which to find the target area.</param>
	/// <param name="canAffects">The potential characters that can be affected.</param>
	/// <param name="player">The player character.</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="TargetResult"/> containing the target area and affected characters, or <c>null</c> if no target area is found.
	/// </returns>
	private readonly TargetResult? FindTargetAreaFriend(float range, IEnumerable<IBattleChara> canAffects, IPlayerCharacter player, TargetType targetOverride)
	{
		if (canAffects == null)
		{
			return null;
		}

		// If range is zero, always target self
		if (range == 0 || Service.Config.UseGroundBeneficialAbilityOnlySelf)
		{
			return new TargetResult(player, [.. GetAffectsVector(player.Position, canAffects)], player.Position);
		}

		if (Service.Config.UseTargetTankForGroundHeal && DataCenter.PartyMembers != null)
		{
			IBattleChara? tankWithStance = null;
			IBattleChara? tankWithoutStance = null;
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member == null || member.GameObjectId == player.GameObjectId)
				{
					continue;
				}

				if (member.IsJobCategory(JobRole.Tank))
				{
					if (member.HasStatus(false, StatusHelper.TankStanceStatus))
					{
						tankWithStance = member;
						break; // Prefer tank with stance
					}
					else
					{
						tankWithoutStance ??= member;
					}
				}
			}
			var tankTarget = tankWithStance ?? tankWithoutStance;
			if (tankTarget != null)
			{
				List<IBattleChara> affectsList = [.. GetAffectsVector(tankTarget.Position, canAffects)];
				return new TargetResult(tankTarget, [.. affectsList], tankTarget.Position);
			}
		}

		// --- Try OnLocations logic first ---
		_ = OtherConfiguration.BeneficialPositions.TryGetValue(Svc.ClientState.TerritoryType, out var pts);
		pts ??= [];

		// Use fallback points if no beneficial positions are found
		if (pts.Length == 0)
		{
			var isTrial = DataCenter.Territory?.ContentType == TerritoryContentType.Trials;
			var isRaid = DataCenter.Territory?.ContentType == TerritoryContentType.Raids;
			var partyCount = 0;
			if (DataCenter.PartyMembers != null)
			{
				foreach (var p in DataCenter.PartyMembers)
				{
					if (p is IPlayerCharacter)
					{
						partyCount++;
					}
				}
			}
			if (isTrial || (isRaid && partyCount >= 8))
			{
				Vector3[] fallbackPoints = [Vector3.Zero, new Vector3(100, 0, 100)];
				var closestFallback = fallbackPoints[0];
				var minDistance = Vector3.Distance(player.Position, fallbackPoints[0]);

				for (var i = 1; i < fallbackPoints.Length; i++)
				{
					var distance = Vector3.Distance(player.Position, fallbackPoints[i]);
					if (distance < minDistance)
					{
						closestFallback = fallbackPoints[i];
						minDistance = distance;
					}
				}

				pts = [closestFallback];
			}
		}

		// Find the closest point and apply a small random offset, then clamp within cast range
		if (pts.Length > 0)
		{
			var closest = pts[0];
			var minDistance = Vector3.Distance(player.Position, pts[0]);

			for (var i = 1; i < pts.Length; i++)
			{
				var distance = Vector3.Distance(player.Position, pts[i]);
				if (distance < minDistance)
				{
					closest = pts[i];
					minDistance = distance;
				}
			}

			// Small random jitter to avoid stacking perfectly; keep it within a reasonable local radius
			var rotation = Random.Shared.NextDouble() * Math.Tau;
			var jitterRadius = (float)(Random.Shared.NextDouble() * MathF.Min(EffectRange * 0.5f, 2f));
			closest.X += (float)(Math.Sin(rotation) * jitterRadius);
			closest.Z += (float)(Math.Cos(rotation) * jitterRadius);

			// Clamp desired placement within cast range from player
			var toDesired = closest - player.Position;
			var len = toDesired.Length();
			if (len > range)
			{
				closest = player.Position + toDesired / len * range;
			}

			// Only accept if within cast range; then compute affected list
			if (Vector3.Distance(player.Position, closest) <= range + player.HitboxRadius)
			{
				List<IBattleChara> affectsList = [.. GetAffectsVector(closest, canAffects)];
				if (affectsList.Count > 0)
				{
					return new TargetResult(player, [.. affectsList], closest);
				}
			}
		}

		// --- OnCalculated logic (fallback) ---
		// Try to place at/near a boss position (known valid terrain)
		// Exclude "wall bosses" — very large omnidirectional bosses positioned off the arena.
		IBattleChara? bossTarget = null;
		if (Svc.Targets.Target is IBattleChara b && b.DistanceToPlayer() <= range && b.IsBossFromIcon()
			&& (b.HasPositional() || b.HitboxRadius <= 8))
		{
			bossTarget = b;
		}

		// If the current target isn't a boss in range, search hostile targets for one
		if (bossTarget == null && DataCenter.AllHostileTargets != null)
		{
			foreach (var hostile in DataCenter.AllHostileTargets)
			{
				if (hostile != null && hostile.IsBossFromIcon() && hostile.DistanceToPlayer() <= range
					&& (hostile.HasPositional() || hostile.HitboxRadius <= 8))
				{
					bossTarget = hostile;
					break;
				}
			}
		}

		if (bossTarget != null)
		{
			var bossPos = bossTarget.Position;
			var distToBoss = Vector3.Distance(player.Position, bossPos);

			Vector3 targetPos;
			if (distToBoss <= range)
			{
				targetPos = bossPos;
			}
			else if (distToBoss > 0.001f)
			{
				// Adjust position to be within cast range along the line to boss
				var toBoss = bossPos - player.Position;
				targetPos = player.Position + toBoss / distToBoss * range;
			}
			else
			{
				targetPos = player.Position;
			}

			List<IBattleChara> affectsList = [.. GetAffectsVector(targetPos, canAffects)];
			return new TargetResult(player, [.. affectsList], targetPos);
		}
		else
		{
			var effectRange = EffectRange;
			List<IBattleChara> partyMembersInRadius = [];
			if (DataCenter.PartyMembers != null)
			{
				foreach (var member in DataCenter.PartyMembers)
				{
					var dist = Vector3.Distance(member.Position, player.Position);
					if (dist <= range + effectRange)
					{
						partyMembersInRadius.Add(member);
					}
				}
			}
			var attackT = FindTargetByType(partyMembersInRadius,
				TargetType.BeAttacked, action.Config.AutoHealRatio, action.Setting.SpecialType, targetOverride, true, InstantHeal);

			if (attackT == null)
			{
				List<IBattleChara> affectsList = [.. GetAffectsVector(player.Position, canAffects)];
				return new TargetResult(player, [.. affectsList], player.Position);
			}
			else
			{
				var disToTankRound = Vector3.Distance(player.Position, attackT.Position) + attackT.HitboxRadius;

				Vector3 finalPos;
				if (disToTankRound < effectRange
					|| disToTankRound > (2 * effectRange) - player.HitboxRadius)
				{
					finalPos = player.Position;
				}
				else
				{
					var directionToTank = attackT.Position - player.Position;
					var dirLength = directionToTank.Length();
					if (dirLength < 0.001f)
					{
						finalPos = player.Position;
					}
					else
					{
						var moveDirection = directionToTank / dirLength * Math.Max(0, disToTankRound - effectRange);
						finalPos = player.Position + moveDirection;
					}
				}

				// Clamp finalPos within cast range
				var toFinal = finalPos - player.Position;
				var toFinalLen = toFinal.Length();
				if (toFinalLen > range)
				{
					if (toFinalLen > 0.001f)
					{
						finalPos = player.Position + toFinal / toFinalLen * range;
					}
					else
					{
						finalPos = player.Position;
					}
				}

				// Safety: fall back to player position if computation produced invalid values
				if (float.IsNaN(finalPos.X) || float.IsNaN(finalPos.Y) || float.IsNaN(finalPos.Z) ||
					float.IsInfinity(finalPos.X) || float.IsInfinity(finalPos.Y) || float.IsInfinity(finalPos.Z))
				{
					finalPos = player.Position;
				}

				List<IBattleChara> affectsList = [.. GetAffectsVector(finalPos, canAffects)];
				return new TargetResult(player, [.. affectsList], finalPos);
			}
		}
	}

	/// <summary>
	/// Gets the characters that are affected within the specified range from a given point.
	/// </summary>
	/// <param name="point">The point from which to measure the effect range.</param>
	/// <param name="canAffects">The potential characters that can be affected.</param>
	/// <returns>
	/// An <see cref="IEnumerable{IBattleChara}"/> containing the characters that are within the effect range.
	/// </returns>
	private readonly IEnumerable<IBattleChara> GetAffectsVector(Vector3? point, IEnumerable<IBattleChara> canAffects)
	{
		if (canAffects == null)
		{
			yield break;
		}

		if (point == null)
		{
			yield break;
		}

		foreach (var t in canAffects)
		{
			if (Vector3.Distance(point.Value, t.Position) - t.HitboxRadius <= EffectRange)
			{
				yield return t;
			}
		}
	}

	/// <summary>
	/// Filters a list of candidates to only those actually inside the player's facing cone/line for
	/// self-cast directional AOEs (Cone and StraightLine cast types), and for Donut cast types
	/// excludes enemies inside the hollow centre (inner radius = 8y), matching the geometry used in
	/// <see cref="GetCanTarget"/>. Circle AOEs are omnidirectional with no inner exclusion and also pass through unfiltered.
	/// </summary>
	private readonly List<IBattleChara> FilterAffectsByFacing(List<IBattleChara> candidates)
	{
		if (Player.Object == null || candidates.Count == 0)
		{
			return candidates;
		}

		var castType = (CastType)action.Action.CastType;
		if (castType != CastType.Cone && castType != CastType.StraightLine && castType != CastType.Donut)
		{
			return candidates; // Circle is omnidirectional with no inner exclusion — pass through unfiltered.
		}

		var playerPos = Player.Object.Position;
		var faceVec = Vector3.Normalize(Player.Object.GetFaceVector());
		faceVec.Y = 0;

		List<IBattleChara> result = [];
		foreach (var t in candidates)
		{
			var toTarget = t.Position - playerPos;
			toTarget.Y = 0;
			var dist = toTarget.Length();
			if (dist <= 0.001f)
			{
				result.Add(t); // On top of player — include
				continue;
			}

			if (castType == CastType.Cone)
			{
				// Use the same cone half-angle (_alpha = π/3) as GetCanTarget
				var cos = Vector3.Dot(faceVec, toTarget / dist);
				if (cos >= (float)Math.Cos(_alpha))
				{
					result.Add(t);
				}
			}
			else if (castType == CastType.StraightLine)
			{
				// Only include enemies in front of the player (positive dot product)
				// and within lateral beam width (matches GetCanTarget line logic)
				if (Vector3.Dot(faceVec, toTarget) < 0)
				{
					continue;
				}

				// Perpendicular distance in yalms: Cross(unit_faceVec_xz, toTarget_xz).Length()
				// = |toTarget_xz| * sin(angle). Do NOT divide by dist — that would give
				// dimensionless sin(angle) which is always < halfWidth and passes every target.
				var faceXZ = Vector3.Normalize(new Vector3(faceVec.X, 0, faceVec.Z));
				Vector3 toTargetXZ = new(toTarget.X, 0, toTarget.Z);
				var perp = Vector3.Cross(faceXZ, toTargetXZ).Length();
				var halfWidth = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier / 2f : 2f;
				var width = halfWidth + t.HitboxRadius;
				if (perp <= width)
				{
					result.Add(t);
				}
			}
			else if (castType == CastType.Donut) // omnidirectional ring; exclude enemies inside the hollow centre
			{
				// Mirror the GetCanTarget Donut check: distance minus hitbox radius must be >= inner radius
				var innerRadius = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier : 8f;
				var d = dist - t.HitboxRadius;
				if (d >= innerRadius)
				{
					result.Add(t);
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Gets the characters that are affected by the specified target.
	/// </summary>
	/// <param name="tar">The target character.</param>
	/// <param name="canAffects">The potential characters that can be affected.</param>
	/// <returns>
	/// An <see cref="IEnumerable{IBattleChara}"/> containing the characters that are affected by the target.
	/// </returns>
	private readonly IEnumerable<IBattleChara> GetAffectsTarget(IBattleChara tar, IEnumerable<IBattleChara> canAffects)
	{
		if (tar == null)
		{
			yield break;
		}

		if (canAffects == null)
		{
			yield break;
		}

		foreach (var t in canAffects)
		{
			if (GetCanTarget(tar, t))
			{
				yield return t;
			}
		}
	}

	#endregion

	#region Get Most Target

	/// <summary>
	/// Gets the most targetable objects based on the specified criteria.
	/// </summary>
	/// <param name="canTargets">The potential targets that can be affected.</param>
	/// <param name="canAffects">The potential characters that can be affected.</param>
	/// <param name="aoeCount">The number of targets to consider for AoE.</param>
	/// <returns>
	/// An <see cref="IEnumerable{IBattleChara}"/> containing the most targetable objects based on the specified criteria.
	/// </returns>
	private readonly IEnumerable<IBattleChara> GetMostCanTargetObjects(IEnumerable<IBattleChara> canTargets, IEnumerable<IBattleChara> canAffects, int aoeCount)
	{
		if (canTargets == null || canAffects == null)
		{
			yield break;
		}

		// Single target or no AoE radius: just pass candidates through
		if (IsSingleTarget || EffectRange <= 0)
		{
			foreach (var target in canTargets)
			{
				yield return target;
			}

			yield break;
		}

		// For hostile actions: if AoE is disabled, block all non-single-target actions here
		if (!action.Setting.IsFriendly && Service.Config.AoEType == AoEType.Off)
		{
			yield break;
		}

		// Cleave mode. Attacks only, as Off already is: the setting is about AoE attacks, and a heal or
		// a party mitigation that wants several members in reach is not one (owner's reading, A147).
		// Without the IsFriendly test a healer on Cleave never cast Medica, Helios or Succor.
		if (!action.Setting.IsFriendly && aoeCount > 1
			&& (Service.Config.AoEType == AoEType.Cleave || (DataCenter.IsInM9S && Service.Config.M9SCleaveOnly)))
		{
			yield break;
		}

		// Materialize once to avoid multiple enumerations
		var targets = canTargets as List<IBattleChara> ?? [.. canTargets];

		List<IBattleChara> best = [];
		var bestCount = Math.Max(0, aoeCount);

		foreach (var t in targets)
		{
			var count = GetCanTargetCount(t, canAffects);
			if (count < bestCount)
			{
				continue;
			}

			if (count > bestCount)
			{
				bestCount = count;
				best.Clear();
			}
			best.Add(t);
		}

		for (var i = 0; i < best.Count; i++)
		{
			yield return best[i];
		}
	}

	/// <summary>
	/// Counts the number of objects that can be targeted based on the specified criteria.
	/// </summary>
	/// <param name="target">The target object.</param>
	/// <param name="canAffects">The potential objects that can be affected.</param>
	/// <returns>The count of objects that can be targeted.</returns>
	private readonly int GetCanTargetCount(IBattleChara target, IEnumerable<IBattleChara> canAffects)
	{
		if (target == null || canAffects == null)
		{
			return 0;
		}

		var count = 0;
		foreach (var t in canAffects)
		{
			if (target != t && !GetCanTarget(target, t))
			{
				continue;
			}

			if (Service.Config.NoNewHostiles && t.TargetObject == null)
			{
				return 0;
			}
			count++;
		}

		return count;
	}

	private const double _alpha = Math.PI / 3;
	/// <summary>
	/// Determines if the sub-target can be targeted based on the specified criteria.
	/// </summary>
	/// <param name="target">The main target object.</param>
	/// <param name="subTarget">The sub-target object.</param>
	/// <returns>True if the sub-target can be targeted; otherwise, false.</returns>
	private readonly bool GetCanTarget(IBattleChara target, IBattleChara subTarget)
	{
		if (target == null || subTarget == null || Player.Object == null)
		{
			return false;
		}

		var pPos = Player.Object.Position;
		var dir = target.Position - pPos;   // Aim direction (player -> main target)
		var tdir = subTarget.Position - pPos; // Vector to sub target

		var dirLen = dir.Length();
		var tdirLen = tdir.Length();


		// If the main target is essentially on top of the player, avoid divide-by-zero and
		// approximate using radial checks so we don't drop AoE entirely in degenerate cases.
		if (dirLen <= 0.001f)
		{
			switch ((CastType)action.Action.CastType)
			{
				case CastType.Circle: // Circle (centered at target)
					return Vector3.Distance(target.Position, subTarget.Position) - subTarget.HitboxRadius <= EffectRange;

				case CastType.Cone: // Cone (cast from player)
					return (tdirLen - subTarget.HitboxRadius) <= EffectRange;

				case CastType.StraightLine: // Line (beam from player in facing direction — degenerate: target on top of player)
					{
						if (tdirLen - subTarget.HitboxRadius > EffectRange)
						{
							return false;
						}

						var lineDir2 = Player.Object != null
							? Vector3.Normalize(new Vector3(Player.Object.GetFaceVector().X, 0, Player.Object.GetFaceVector().Z))
							: Vector3.UnitZ;
						Vector3 tdirXZ2 = new(tdir.X, 0, tdir.Z);
						var perp2 = Vector3.Cross(lineDir2, tdirXZ2).Length();
						var halfWidth2 = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier / 2f : 2f;
						return perp2 <= halfWidth2 + subTarget.HitboxRadius && Vector3.Dot(lineDir2, tdirXZ2) >= 0;
					}

				case CastType.Donut: // Donut (centered at target)
					{
						var d = Vector3.Distance(target.Position, subTarget.Position) - subTarget.HitboxRadius;
						var innerRadius = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier : 8f;
						return d <= EffectRange && d >= innerRadius;
					}

				default:
					if (Service.Config.InDebug)
					{
						//PluginLog.Debug($"{action.Action.Name.ExtractText()}'s CastType is not valid! The value is {action.Action.CastType}");
					}
					return false;
			}
		}

		switch ((CastType)action.Action.CastType)
		{
			case CastType.Circle: // Circle (centered at target)
				return Vector3.Distance(target.Position, subTarget.Position) - subTarget.HitboxRadius <= EffectRange;

			case CastType.Cone: // Cone (cast from player)
				{
					// Quick radial gate with hitbox
					if (tdirLen - subTarget.HitboxRadius > EffectRange)
					{
						return false;
					}

					// Normalize once
					var dirN = dir / dirLen;

					// Widen by target hitbox along aim direction to be more permissive near the apex
					var tdirAdj = tdir + dirN * (target.HitboxRadius / (float)Math.Sin(_alpha));

					var tlen = tdirAdj.Length();
					if (tlen <= 0.001f)
					{
						return true; // on top of player after adjustment: treat as inside
					}

					var cos = Vector3.Dot(dir, tdirAdj) / (dirLen * tlen);
					return cos >= Math.Cos(_alpha);
				}

			case CastType.StraightLine: // Line (beam from player in the player's facing direction)
				{
					// Distance gate: sub-target must be within beam length (EffectRange) of player
					if (tdirLen - subTarget.HitboxRadius > EffectRange)
					{
						return false;
					}

					// StraightLine fires in the direction the player is currently facing,
					// not necessarily toward the selected target.
					var lineDir = Player.Object != null
						? Vector3.Normalize(new Vector3(Player.Object.GetFaceVector().X, 0, Player.Object.GetFaceVector().Z))
						: dir / dirLen;

					// Perpendicular distance in yalms: project both vectors to XZ so Y height
					// differences don't inflate the cross product length.
					Vector3 tdirXZ = new(tdir.X, 0, tdir.Z);
					var perp = Vector3.Cross(lineDir, tdirXZ).Length();

					// Beam half-width from XAxisModifier + sub-target hitbox radius
					var halfWidth = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier / 2f : 2f;
					var width = halfWidth + subTarget.HitboxRadius;

					return perp <= width && Vector3.Dot(lineDir, tdirXZ) >= 0;
				}

			case CastType.Donut: // Donut (centered at target)
				{
					var dis = Vector3.Distance(target.Position, subTarget.Position) - subTarget.HitboxRadius;
					var innerRadius = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier : 8f;
					return dis <= EffectRange && dis >= innerRadius;
				}

			default:
				if (Service.Config.InDebug)
				{
					//PluginLog.Debug($"{action.Action.Name.ExtractText()}'s CastType is not valid! The value is {action.Action.CastType}");
				}
				return false;
		}
	}

	#endregion

	#region TargetFind

	/// <summary>
	/// Finds the target based on the specified type and criteria.
	/// </summary>IBattleChara battleChara
	/// <param name="battleChara"></param>
	/// <param name="type"></param>
	/// <param name="healRatio"></param>
	/// <param name="actionType"></param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <param name="isFriendly">Indicates whether the target is friendly.</param>
	/// <returns></returns>
	public static IBattleChara? FindTargetByType(IEnumerable<IBattleChara> battleChara, TargetType type, float healRatio, SpecialActionType actionType, TargetType targetOverride, bool isFriendly)
		=> FindTargetByType(battleChara, type, healRatio, actionType, targetOverride, isFriendly, false);

	/// <summary>
	/// <see cref="FindTargetByType(IEnumerable{IBattleChara}, TargetType, float, SpecialActionType, TargetType, bool)"/>
	/// for a heal of the given kind: <paramref name="instantHeal"/> is true for an off-GCD action, whose
	/// heal-ahead reading looks no further than the animation lock (A213).
	/// </summary>
	public static IBattleChara? FindTargetByType(IEnumerable<IBattleChara> battleChara, TargetType type, float healRatio, SpecialActionType actionType, TargetType targetOverride, bool isFriendly, bool instantHeal)
	{
		return FindTargetByType(battleChara, type, healRatio, actionType, targetOverride, isFriendly, instantHeal, null, null);
	}

	// hostileOrder: an order of its own for the hostile choice, ahead of the targeting type; null for none.
	// Only DotSpreadOrder gives one.
	private static IBattleChara? FindTargetByType(IEnumerable<IBattleChara> battleChara, TargetType type, float healRatio, SpecialActionType actionType, TargetType targetOverride, bool isFriendly, bool instantHeal, TargetingType? commandTargeting, Comparison<IBattleChara>? hostileOrder)
	{
		if (battleChara == null)
		{
			return null;
		}

		if (Player.Object == null)
		{
			return null;
		}

		if (type == TargetType.Self)
		{
			return Player.Object;
		}

		if (targetOverride == TargetType.Self)
		{
			return Player.Object;
		}

		switch (actionType)
		{
			case SpecialActionType.MeleeRangedAttack:
				{
					var filtered = new List<IBattleChara>();
					foreach (var t in battleChara)
					{
						if (t.DistanceToPlayer() >= 3f + (Service.Config?.MeleeRangedOffset ?? 0f))
						{
							filtered.Add(t);
						}
					}
					battleChara = filtered;
				}
				break;

			// Pure movement toward a hostile target — respects MoveForward/Intercept status
			// and prunes to targets within the configured movement range.
			case SpecialActionType.HostileMovingForward:
				if (Service.Config != null)
				{
					if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveForward) || Service.CountDownTime > 0)
					{
						if (Svc.Targets.Target is IBattleChara IsHostiletarg && IsHostiletarg.IsHostile())
						{
							return IsHostiletarg;
						}
					}
					if (DataCenter.MergedStatus.HasFlag(AutoStatus.Intercepting))
					{
						if (Svc.Targets.Target is IBattleChara IsHostiletarg && IsHostiletarg.IsHostile())
						{
							return IsHostiletarg;
						}
					}
					else
					{
						// Filter by distance AND hostile status
						var filtered = new List<IBattleChara>();
						foreach (var t in battleChara)
						{
							if (t.IsHostile() && t.DistanceToPlayer() < Service.Config.DistanceForMoving2)
							{
								filtered.Add(t);
							}
						}
						battleChara = filtered;
					}
				}
				break;

			// Pure movement toward a friendly/party target — prefers focus then hard target.
			case SpecialActionType.FriendlyMovingForward:
				if (Svc.Targets.FocusTarget != null)
				{
					if (Svc.Targets.FocusTarget is IBattleChara focus && focus.IsParty())
					{
						return focus;
					}
				}
				else if (Svc.Targets.FocusTarget == null && Svc.Targets.Target != null)
				{
					if (Svc.Targets.Target is IBattleChara IsPartytarg && IsPartytarg.IsParty())
					{
						return IsPartytarg;
					}
				}
				break;

			// Pure movement toward either a friendly or hostile target — checks focus first,
			// then hard target as party member, then hard target as hostile.
			case SpecialActionType.HostileFriendlyMovingForward:
				if (Svc.Targets.FocusTarget != null)
				{
					if (Svc.Targets.FocusTarget is IBattleChara focus && focus.IsParty())
					{
						return focus;
					}
				}
				else if (Svc.Targets.FocusTarget == null && Svc.Targets.Target != null)
				{
					if (Svc.Targets.Target is IBattleChara IsPartytarg && IsPartytarg.IsParty())
					{
						return IsPartytarg;
					}
					if (Svc.Targets.Target is IBattleChara IsHostiletarg && IsHostiletarg.IsHostile())
					{
						return IsHostiletarg;
					}
				}
				break;

			// Movement-attack: the character moves as part of dealing damage to a hostile target.
			// Use the full standard hostile targeting pipeline — all normal filters apply
			// (stop marks, priority, TTK, resistance, CanTarget predicate).
			// Fall through intentionally so the regular switch(type) path handles target selection.
			case SpecialActionType.HostileMovingAttack:
			case SpecialActionType.HostileAttackBackstep:
				// Filter alive hostiles; then let the standard hostile-target logic below run.
				{
					var filtered = new List<IBattleChara>();
					foreach (var t in battleChara)
					{
						if (ObjectHelper.IsAlive(t))
						{
							filtered.Add(t);
						}
					}
					battleChara = filtered;
				}
				break;

			// Fixed-distance self-move: no target needed; always use self so the action fires.
			case SpecialActionType.FixedDistanceMoveForward:
				return Player.Object;

			// Backward movement: character moves away from its current position/target.
			// Self-targeting is sufficient to fire the action; destination safety is validated separately.
			case SpecialActionType.FixedDistanceMoveBackward:
				return Player.Object;
		}

		// Command targeting, then targetOverride, then the action's own type.
		var effectiveType = commandTargeting.HasValue ? ToTargetType(commandTargeting.Value)
			: targetOverride == default ? type : targetOverride;

		{
			switch (effectiveType)
			{
				case TargetType.Death:
					if (DataCenter.DeathTarget != null)
					{
						return DataCenter.DeathTarget;
					}

					// Forced raises still skip anyone already raised or excluded by the raise filters.
					battleChara = battleChara.GetDeath();
					break;

				case TargetType.Move:
					break;

				case TargetType.FriendMove:
					{
						if (Svc.Targets.FocusTarget != null)
						{
							if (Svc.Targets.FocusTarget is IBattleChara focus && focus.IsParty())
							{
								return focus;
							}
						}
						else if (Svc.Targets.Target != null)
						{
							if (Svc.Targets.Target is IBattleChara targ && targ.IsParty())
							{
								return targ;
							}
						}
					}
					break;

				default:
					{
						var filtered = new List<IBattleChara>();
						foreach (var t in battleChara)
						{
							if (ObjectHelper.IsAlive(t))
							{
								filtered.Add(t);
							}
						}
						battleChara = filtered;
					}
					break;
			}

			return effectiveType switch
			{
				TargetType.BeAttacked => FindBeAttackedTarget(),
				TargetType.Provoke => FindProvokeTarget(),
				TargetType.TankSwap => FindTankSwapTarget(),
				TargetType.Dispel => FindDispelTarget(),
				TargetType.Move => FindTargetForMoving(),
				TargetType.Heal => FindHealTarget(healRatio),
				TargetType.Interrupt => FindInterruptTarget(),
				TargetType.Tank => FindTankTarget(),
				TargetType.Tankbuster => FindTankbusterTarget(),
				TargetType.Melee => battleChara != null ? RandomMeleeTarget(battleChara) : null,
				TargetType.Range => battleChara != null ? RandomRangeTarget(battleChara) : null,
				TargetType.Magical => battleChara != null ? RandomMagicalTarget(battleChara) : null,
				TargetType.Physical => battleChara != null ? RandomPhysicalTarget(battleChara) : null,
				TargetType.DarkCannon => FindDarkCannonTarget(),
				TargetType.ShockCannon => FindShockCannonTarget(),
				TargetType.PhantomBell => FindPhantomBell(),
				TargetType.PhantomRespite => FindPhantomRespite(),
				TargetType.DancePartner => FindDancePartner(),
				TargetType.MimicryTarget => FindMimicryTarget(),
				TargetType.TheSpear => FindTheSpear(),
				TargetType.TheBalance => FindTheBalance(),
				TargetType.Kardia => FindKardia(),
				TargetType.Deployment => FindDeploymentTacticsTarget(),
				//TargetType.Capture => FindCaptureTarget(),
				_ => isFriendly ? FindFriendly() : FindHostile(),
			};
		}

		IBattleChara? FindFriendly()
		{
			return battleChara == null ? null : PickByTargeting(battleChara, effectiveType);
		}

		IBattleChara? FindDarkCannonTarget()
		{
			if (battleChara != null)
			{
				if (PhantomRotation.CannoneerLevel < 4)
				{
					return FindHostile();
				}
				else
				{
					foreach (var hostile in battleChara)
					{
						if (!hostile.IsOCBlindImmuneTarget() && hostile.InCombat())
						{
							return hostile;
						}
					}
				}
			}
			return null;
		}

		IBattleChara? FindShockCannonTarget()
		{
			if (battleChara != null)
			{
				foreach (var hostile in battleChara)
				{
					if (!hostile.IsOCParalysisImmuneTarget() && hostile.InCombat())
					{
						return hostile;
					}
				}
			}
			return null;
		}

		IBattleChara? FindPhantomBell()
		{
			var partyMembers = DataCenter.PartyMembers;
			var allMembers = new List<IBattleChara>();
			if (DataCenter.AllTargets != null)
			{
				foreach (var x in DataCenter.AllTargets)
				{
					if (x != null && !x.IsEnemy() && !x.HasStatus(false, StatusID.BattleBell))
					{
						allMembers.Add(x);
					}
				}
			}
			if (partyMembers != null && partyMembers.Count > 0)
			{
				var bePartyAttackedTarget = FindTargetByType(partyMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);
				var beAllAttackedTarget = FindTargetByType(allMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);
				// Compare by reference or GameObjectId (safer for IBattleChara)
				if (bePartyAttackedTarget != null && beAllAttackedTarget != null &&
					bePartyAttackedTarget.GameObjectId == beAllAttackedTarget.GameObjectId)
				{
					return bePartyAttackedTarget;
				}
			}

			// Fallback: only return self if self does NOT have BattleBell
			if (!StatusHelper.PlayerHasStatus(false, StatusID.BattleBell))
			{
				return Player.Object;
			}
			return null;
		}

		IBattleChara? FindPhantomRespite()
		{
			List<IBattleChara> partyMembers = [];
			if (DataCenter.PartyMembers != null)
			{
				foreach (var x in DataCenter.PartyMembers)
				{
					if (x != null && !x.HasStatus(false, StatusID.RingingRespite))
					{
						partyMembers.Add(x);
					}
				}
			}

			var allMembers = new List<IBattleChara>();
			if (DataCenter.AllTargets != null)
			{
				foreach (var x in DataCenter.AllTargets)
				{
					if (x != null && !x.IsEnemy() && !x.HasStatus(false, StatusID.RingingRespite))
					{
						allMembers.Add(x);
					}
				}
			}

			if (partyMembers != null && partyMembers.Count > 0)
			{
				// Use FindTargetByType to get the BeAttacked target from party members
				var bePartyAttackedTarget = FindTargetByType(partyMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);
				var beAllAttackedTarget = FindTargetByType(allMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);
				// Compare by reference or GameObjectId (safer for IBattleChara)
				if (bePartyAttackedTarget != null && beAllAttackedTarget != null &&
					bePartyAttackedTarget.GameObjectId == beAllAttackedTarget.GameObjectId)
				{
					return bePartyAttackedTarget;
				}
			}

			// Fallback: return self
			return Player.Object;
		}

		IBattleChara? FindDancePartner()
		{
			if (!DataCenter.PlayerAvailable() || !TargetFilter.PlayerIsJobs(Job.DNC)
				|| StatusHelper.PlayerHasStatus(true, StatusID.ClosedPosition) || DancerRotation.IsDancing
				|| DataCenter.PartyMembers.Count < 2)
			{
				return null;
			}

			// Only members among the candidates, which are the ones in Closed Position's range: nothing
			// checks the range after this choice, so a partner out of reach was sent to the game, refused,
			// and chosen again on the next frame - at the head of the Dancer's off-GCD order (A223).
			HashSet<ulong> inReach = [];
			foreach (var candidate in battleChara)
			{
				if (candidate != null)
				{
					inReach.Add(candidate.GameObjectId);
				}
			}

			var priority = OtherConfiguration.DancePartnerPriority;
			var playerId = Player.Object!.GameObjectId;
			var partner = FirstByJobPriority(priority, m => m.GameObjectId != playerId && inReach.Contains(m.GameObjectId) && !m.HasStatus(false, PoorOrPartneredStatuses))
				?? FirstByJobPriority(priority, m => m.GameObjectId != playerId && inReach.Contains(m.GameObjectId) && !m.HasStatus(false, PoorTargetStatuses));
			if (partner != null)
			{
				return partner;
			}

			return RandomMeleeTarget(battleChara) ?? RandomRangeTarget(battleChara)
				?? RandomMagicalTarget(battleChara) ?? RandomPhysicalTarget(battleChara);
		}

		IBattleChara? FindTheSpear()
		{
			return FindCardTarget(OtherConfiguration.TheSpearPriority, StatusID.TheSpear_3889, true);
		}

		IBattleChara? FindTheBalance()
		{
			return FindCardTarget(OtherConfiguration.TheBalancePriority, StatusID.TheBalance_3887, false);
		}

		IBattleChara? FindKardia()
		{
			if (!TargetFilter.PlayerIsJobs(Job.SGE))
			{
				return null;
			}

			if (DataCenter.PartyMembers.Count == 1)
			{
				return Player.Object;
			}

			// Stanced tanks without Kardion first, then stanced tanks, then any tank.
			var priority = OtherConfiguration.KardiaTankPriority;
			var tank = FirstByJobPriority(priority, static m => m.IsJobCategory(JobRole.Tank) && m.HasStatus(false, StatusHelper.TankStanceStatus) && !m.HasStatus(false, StatusID.Kardion))
				?? FirstByJobPriority(priority, static m => m.IsJobCategory(JobRole.Tank) && m.HasStatus(false, StatusHelper.TankStanceStatus))
				?? FirstByJobPriority(priority, static m => m.IsJobCategory(JobRole.Tank));
			if (tank != null)
			{
				return tank;
			}

			var party = DataCenter.PartyMembers;
			return FindTankTarget() ?? RandomMeleeTarget(party) ?? RandomPhysicalTarget(party)
				?? RandomRangeTarget(party) ?? RandomMagicalTarget(party);
		}
		IBattleChara? FindDeploymentTacticsTarget()
		{
			if (!TargetFilter.PlayerIsJobs(Job.SCH))
			{
				return null;
			}

			if (DataCenter.PartyMembers == null)
			{
				return null;
			}

			IBattleChara? bestCatalyze = null;
			uint bestCatalyzeShield = 0;

			IBattleChara? bestGalvanize = null;
			uint bestGalvanizeShield = 0;

			//const float spreadRadius = 30f; // Deployment Tactics spreads within 30y of the target

			foreach (var battleChara in DataCenter.PartyMembers)
			{
				if (battleChara == null || battleChara.IsDead)
				{
					continue;
				}

				var shield = ObjectHelper.GetObjectShield(battleChara);
				if (shield == 0)
				{
					// No effective shield to spread
					continue;
				}

				// Require at least one other party member in spread radius
				//int neighbors = 0;
				//foreach (IBattleChara member in DataCenter.PartyMembers)
				//{
				//    if (member == null || member.IsDead || member.GameObjectId == battleChara.GameObjectId)
				//    {
				//        continue;
				//    }
				//    if (Vector3.Distance(member.Position, battleChara.Position) <= spreadRadius)
				//    {
				//        neighbors++;
				//        if (neighbors >= 1) break;
				//    }
				//}
				//if (neighbors == 0)
				//{
				//    // Nothing to spread to
				//    continue;
				//}

				if (battleChara.HasStatus(true, StatusID.Catalyze))
				{
					if (bestCatalyze == null || shield > bestCatalyzeShield)
					{
						bestCatalyze = battleChara;
						bestCatalyzeShield = shield;
					}
				}
				else if (battleChara.HasStatus(true, StatusID.Galvanize))
				{
					if (bestGalvanize == null || shield > bestGalvanizeShield)
					{
						bestGalvanize = battleChara;
						bestGalvanizeShield = shield;
					}
				}
			}

			if (bestCatalyze != null)
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"FindDeploymentTacticsTarget: {bestCatalyze.Name} is a valid target with Catalyze, largest shield, and nearby allies.");
				}
				return bestCatalyze;
			}

			if (bestGalvanize != null && bestCatalyze == null)
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"FindDeploymentTacticsTarget: {bestGalvanize.Name} is a valid target with Galvanize, largest shield, and nearby allies.");
				}
				return bestGalvanize;
			}

			return null;
		}

		IBattleChara? FindProvokeTarget()
		{
			if (battleChara != null && DataCenter.ProvokeTarget != null)
			{
				foreach (var o in battleChara)
				{
					if (o.GameObjectId == DataCenter.ProvokeTarget.GameObjectId)
					{
						return DataCenter.ProvokeTarget;
					}
				}
			}
			return null;
		}

		// A tank swap (TankSwapWatch): for Shirk the tank to hand the tankbuster's source to, for
		// Provoke that source when it is taken back - in either case only if it is among this action's
		// candidates, in range and targetable for it.
		IBattleChara? FindTankSwapTarget()
		{
			var swap = isFriendly ? TankSwapWatch.Target : TankSwapWatch.ReclaimSource;
			if (battleChara != null && swap != null)
			{
				foreach (var o in battleChara)
				{
					if (o.GameObjectId == swap.GameObjectId)
					{
						return swap;
					}
				}
			}
			return null;
		}

		IBattleChara? FindTargetForMoving()
		{
			if (battleChara == null || Player.Object == null)
			{
				return null;
			}

			var pPosition = Player.Object.Position;
			var towardsScreenCenter = Service.Config.MoveTowardsScreenCenter;
			var playerScrPos = Vector2.Zero;
			if (towardsScreenCenter && !Svc.GameGui.WorldToScreen(pPosition, out playerScrPos))
			{
				return null;
			}

			var faceVec = Player.Object.GetFaceVector();
			var halfAngle = Math.PI * Service.Config.MoveTargetAngle / 360;
			IBattleChara? farthest = null;
			var farthestDistance = float.MinValue;
			foreach (var t in battleChara)
			{
				var distance = t.DistanceToPlayer();
				if (distance > Service.Config.DistanceForMoving2 || distance <= farthestDistance)
				{
					continue;
				}

				bool inDirection;
				if (towardsScreenCenter)
				{
					if (!Svc.GameGui.WorldToScreen(t.Position, out var scrPos))
					{
						continue;
					}

					var dir = scrPos - playerScrPos;
					inDirection = dir.Y <= 0 && Math.Abs(dir.X / dir.Y) <= Math.Tan(halfAngle);
				}
				else
				{
					inDirection = Vector3.Dot(faceVec, Vector3.Normalize(t.Position - pPosition)) >= Math.Cos(halfAngle);
				}

				if (inDirection)
				{
					farthest = t;
					farthestDistance = distance;
				}
			}

			return farthest;
		}

		IBattleChara? FindHealTarget(float healRatio)
		{
			if (battleChara == null)
			{
				return null;
			}

			List<IBattleChara> filteredGameObjects = [];
			foreach (var o in battleChara)
			{
				// The death-trigger hold has to bite at the entrance, not further down. Everything
				// below draws from this set - the ranked candidate list, the healer and tank passes,
				// the worst-hurt pick and both fallbacks - and the tank-stance fallback would
				// otherwise hand back the very dark knight being held, since it wears Grit. The hold
				// then failed exactly where it was needed: whenever the member who raised the heal
				// flag is out of this action's range, the bearer is the only candidate left.
				//
				// The self short-cut below bypasses this set and carries the same check of its own.
				if (o.IsHeldForDeathTrigger())
				{
					continue;
				}

				// Forecast health here too, and this is the gate the rest depends on: everything below
				// draws from this set, so a member kept out here is invisible to the ranked list, to
				// the critical rank and to every short-cut. AutoHealRatio defaults to 0.8, which is
				// exactly where the look-ahead earns its keep - a tank at 90% who reaches zero in six
				// seconds is at a forecast 34%, and judged by his current health he would be dropped
				// before anything downstream ever saw him.
				//
				// This is not overhealing: the cut exists to keep a cast from being spent on somebody
				// who does not need it, and somebody who will be at 34% when the cast lands does.
				if (!IBaseAction.AutoHealCheck || o.GetForecastHealthRatio(instantHeal) < healRatio)
				{
					filteredGameObjects.Add(o);
				}
			}

			List<IBattleChara> partyMembers = [];
			foreach (var o in filteredGameObjects)
			{
				if (ObjectHelper.IsParty(o))
				{
					partyMembers.Add(o);
				}
			}

			var result = GeneralHealTarget(partyMembers, instantHeal);
			if (result != null)
			{
				return result;
			}

			result = GeneralHealTarget(filteredGameObjects, instantHeal);
			if (result != null)
			{
				return result;
			}

			foreach (var t in partyMembers)
			{
				if (t.HasStatus(false, StatusHelper.TankStanceStatus))
				{
					return t;
				}
			}

			if (partyMembers.Count > 0)
			{
				return partyMembers[0];
			}

			foreach (var t in filteredGameObjects)
			{
				if (t.HasStatus(false, StatusHelper.TankStanceStatus))
				{
					return t;
				}
			}

			if (filteredGameObjects.Count > 0)
			{
				return filteredGameObjects[0];
			}

			return null;

			static IBattleChara? GeneralHealTarget(List<IBattleChara> objs, bool instant)
			{
				// Everyone healable is a candidate. Only a status that nullifies healing outright
				// takes a target out; a protective status merely moves it back in the queue.
				//
				// This used to read `if (!o.NoNeedHealingInvuln())`, which collected the exact opposite set:
				// the function (CanBeHealed since 7.5.7.1) returns true when NO protective status is up
				// (an absent status has trivially "ended"), so the list held only the invulnerable
				// and was empty in the ordinary case. Everything below it - the healer pass, the
				// tank pass, the worst-hurt pick - then ran over nothing and the caller fell back
				// to the tank-stance holder, so the worst-hurt member could never be chosen.
				//
				// Demoting rather than excluding is what the protection actually warrants: the
				// invulnerability window is the safest moment to heal, and when it ends the target
				// stands wherever it left them - Superbolide leaves them at 1 HP on purpose.
				// Both sort keys are read once per member rather than inside the comparator. Sorting
				// n members costs O(n log n) comparisons, and each key walks a status list or the
				// party/doom checks in GetHealthRatio - this runs in the combat path. Reading them
				// up front also fixes the keys for the duration of the sort: CanBeHealed
				// compares a live RemainingTime against a GCD window, so a status sitting on that
				// boundary could otherwise answer differently between two comparisons, and an
				// inconsistent comparer makes List.Sort throw.
				List<(IBattleChara Obj, bool Unprotected, float Health)> ranked = [];
				foreach (var o in objs)
				{
					// A corpse takes no healing, so it is not a healing candidate - the same reason
					// HealingIneffectiveStatus keeps its bearers out, taken to its extreme.
					//
					// Nothing upstream of here excludes the dead: GetCanTargets only drops targets at
					// full health, and GetHealthRatio returns 0 for a corpse rather than something
					// out of range. The worst-hurt pick therefore always rated a dead member as the
					// most urgent, and only the role short-cuts ahead of it kept that from showing.
					// The critical rank below would have made it unconditional, because a corpse
					// holds the fewest effective points there can be. Raising is a separate path
					// with its own target type, so nothing here is taken away from it.
					if (o.IsDead || o.HasStatus(false, StatusHelper.HealingIneffectiveStatus))
					{
						continue;
					}

					// Forecast health rather than current, so the ordering below answers "who will be
					// worst off when a heal lands" instead of "who is worst off now". With the
					// setting off the two are the same number.
					ranked.Add((o, o.IsUnprotectedForHealing(), o.GetForecastHealthRatio(instant)));
				}

				// Unprotected before protected, then lowest health first inside each group.
				ranked.Sort((a, b) =>
				{
					var byProtection = b.Unprotected.CompareTo(a.Unprotected);
					return byProtection != 0 ? byProtection : a.Health.CompareTo(b.Health);
				});

				// Anyone about to die comes before every role short-cut below.
				//
				// The three short-cuts return outright once their own threshold is met and look at
				// nobody else, so a damage dealer at 10% was passed over the moment the tank stood
				// at 44%, or the player themself at 39%. That is the case the user names - "auch ein
				// Damagedealer ohne Aggro mit 10% Leben kann bei einem AoE sterben" - and the tank
				// behind mitigations and a large pool is not in more danger there, only earlier in
				// the order. His ranking of healer before tank before damage dealer applies at
				// *equal* danger, which 10% against 44% is not.
				//
				// The threshold is the tree's own statement of "this one is about to fall": the
				// value at which the tank rotations fire their invulnerability. Effective health,
				// so a barrier counts - a tank at 10% behind The Blackest Night does not die to the
				// next hit - and read the same way CanProvoke reads it.
				//
				// Ordered by absolute effective points rather than by percentage, because whoever
				// stands here dies to the next hit and a hit is an absolute number: 10% of a small
				// pool is fewer points than 14% of a large one. Role decides only a tie, which is
				// where the instruction puts it.
				//
				// Protected members stay out. Without that this rank would invert into its own
				// opposite - a gunbreaker under Superbolide sits at 1 HP on purpose, would hold the
				// fewest points of anyone and would outrank every genuine emergency.
				IBattleChara? critical = null;
				var criticalHp = uint.MaxValue;
				var criticalRole = int.MaxValue;
				foreach (var r in ranked)
				{
					if (!r.Obj.IsInCriticalClass(r.Unprotected))
					{
						continue;
					}

					var hp = r.Obj.GetForecastEffectiveHp(instant);
					var role = TieBreakRank(r.Obj);
					if (critical == null || hp < criticalHp || (hp == criticalHp && role < criticalRole))
					{
						critical = r.Obj;
						criticalHp = hp;
						criticalRole = role;
					}
				}

				if (critical != null)
				{
					return critical;
				}

				List<IBattleChara> healingNeededObjs = [];
				foreach (var r in ranked)
				{
					healingNeededObjs.Add(r.Obj);
				}

				// The role short-cuts below return their candidate outright once it drops under its
				// ratio, so they run ahead of the ordering above. Feeding them protected members
				// would undo the demotion at exactly the case it exists for: a Superbolide tank
				// sits at 1 HP, clears HealthTankRatio immediately, and would be picked over an
				// unprotected member at 20%. So the role passes see unprotected members only;
				// anyone protected is left to the final worst-hurt pick, which still reaches them
				// when nobody else needs healing.
				List<IBattleChara> healerTars = [];
				List<IBattleChara> tankTars = [];
				foreach (var r in ranked)
				{
					if (!r.Unprotected)
					{
						continue;
					}

					var enumHealer = TargetFilter.GetJobCategory([r.Obj], JobRole.Healer).GetEnumerator();
					var isHealer = enumHealer.MoveNext();
					enumHealer.Dispose();
					if (isHealer)
					{
						healerTars.Add(r.Obj);
						continue;
					}

					var enumTank = TargetFilter.GetJobCategory([r.Obj], JobRole.Tank).GetEnumerator();
					var isTank = enumTank.MoveNext();
					enumTank.Dispose();
					if (isTank)
					{
						tankTars.Add(r.Obj);
					}
				}

				// The self short-cut bypasses the ranking below, so it repeats the two checks that
				// keep a target out of it: a heal on a target that nullifies HP recovery is a wasted
				// cast, and a dark knight inside its Living Dead window is being held on purpose.
				// Without the second check, any other member raising the heal flag would let the
				// bearer heal itself out of the trigger through this path. It also asks whether the
				// action can reach the player at all (upstream, c3fac720b): an action that only targets
				// others, Nascent Flash for one, returned the player here and so was never cast.
				var player = Player.Object;
				if (player != null
					&& !player.HasStatus(false, StatusHelper.HealingIneffectiveStatus)
					&& !ObjectHelper.PlayerIsHeldForDeathTrigger()
					&& ObjectHelper.GetForecastPlayerHealthRatio(instant) <= Service.Config.HealthSelfRatio)
				{
					foreach (var o in objs)
					{
						if (o.GameObjectId == player.GameObjectId)
						{
							return player;
						}
					}
				}

				if (Service.Config.HealTargetByDanger)
				{
					return DangerClassTarget(ranked, instant);
				}

				var healerTar = healerTars.Count > 0 ? healerTars[0] : null;
				if (healerTar != null && healerTar.GetForecastHealthRatio(instant) <= Service.Config.HealthHealerRatio)
				{
					return healerTar;
				}

				var tankTar = tankTars.Count > 0 ? tankTars[0] : null;
				if (tankTar != null && tankTar.GetForecastHealthRatio(instant) <= Service.Config.HealthTankRatio)
				{
					return tankTar;
				}

				// The last pick keeps the plain ratio: it asks "is anyone hurt at all", and a member
				// at full health with a falling trend is not hurt yet. Forecasting here would hand
				// the heal to somebody the caller never asked about.
				var tar = healingNeededObjs.Count > 0 ? healingNeededObjs[0] : null;
				return tar != null && tar.GetHealthRatio() < 1 ? tar : null;
			}
		}

		// Classes 2 and 3 of the owner's triage (concept 07), below class 1 and the self short-cut.
		//
		// Class 2: a healer or tank under their role threshold who is also being attacked. The
		// threshold says "low enough", the aggro "and actually taking damage" - without it a tank would
		// sit in this class for good, since he always wears his stance. Lowest health first, a healer
		// before a tank at equal health.
		//
		// Class 3: everyone else unprotected. While area damage is announced, fewest effective hit
		// points first, because a raidwide takes the same number from everybody and a small pool dies
		// to it first; otherwise lowest health first. Role decides a tie. Announced is asked without the
		// mitigation verdict (DataCenter.AreaHitAnnounced): a cast too small to spend a cooldown on is
		// still area damage, and the large interruptible cast and the BossMod raidwide count as well.
		//
		// Protected members come last, as everywhere in this method; the last pick keeps the plain
		// "is anyone hurt at all" check.
		static IBattleChara? DangerClassTarget(List<(IBattleChara Obj, bool Unprotected, float Health)> ranked, bool instant)
		{
			IBattleChara? pressed = null;
			var pressedHealth = float.MaxValue;
			var pressedRole = int.MaxValue;
			foreach (var r in ranked)
			{
				if (!r.Unprotected || !DataCenter.TargetedPartyMembers.Contains(r.Obj.GameObjectId))
				{
					continue;
				}

				var isHealer = r.Obj.IsJobCategory(JobRole.Healer);
				if (!isHealer && !r.Obj.IsJobCategory(JobRole.Tank))
				{
					continue;
				}

				if (r.Health > (isHealer ? Service.Config.HealthHealerRatio : Service.Config.HealthTankRatio))
				{
					continue;
				}

				var role = isHealer ? 0 : 1;
				if (pressed == null || r.Health < pressedHealth || (r.Health == pressedHealth && role < pressedRole))
				{
					pressed = r.Obj;
					pressedHealth = r.Health;
					pressedRole = role;
				}
			}

			if (pressed != null)
			{
				// The role thresholds differ (tank 0.45, healer 0.40 by default), and they stand in for the damage
				// rate: the tank takes the hits, the healer stands still. In this class both are being attacked, so
				// that reason does not hold, and a healer at 42 % lost to a tank at 44 % - against the owner's
				// triage, the one most likely to die first and a healer before a tank at equal health (concept 07).
				// A threshold says when a role is low enough to come first, not that a lower member of the other
				// role must wait: whoever is being attacked and stands as low as the pick or lower joins it (A283).
				foreach (var r in ranked)
				{
					if (!r.Unprotected || r.Health > pressedHealth || !DataCenter.TargetedPartyMembers.Contains(r.Obj.GameObjectId))
					{
						continue;
					}

					var isHealer = r.Obj.IsJobCategory(JobRole.Healer);
					if (!isHealer && !r.Obj.IsJobCategory(JobRole.Tank))
					{
						continue;
					}

					var role = isHealer ? 0 : 1;
					if (r.Health < pressedHealth || (r.Health == pressedHealth && role < pressedRole))
					{
						pressed = r.Obj;
						pressedHealth = r.Health;
						pressedRole = role;
					}
				}

				return pressed;
			}

			var byPoints = DataCenter.AreaHitAnnounced;
			IBattleChara? rest = null;
			var restKey = float.MaxValue;
			var restRole = int.MaxValue;
			foreach (var r in ranked)
			{
				// Only the hurt: ranked by points, a damage dealer at full health has the smallest pool,
				// won the class, and the check below then healed nobody while the tank was at half.
				if (!r.Unprotected || r.Obj.GetHealthRatio() >= 1)
				{
					continue;
				}

				var key = byPoints ? r.Obj.GetForecastEffectiveHp(instant) : r.Health;
				var role = TieBreakRank(r.Obj);
				if (rest == null || key < restKey || (key == restKey && role < restRole))
				{
					rest = r.Obj;
					restKey = key;
					restRole = role;
				}
			}

			rest ??= ranked.Count > 0 ? ranked[0].Obj : null;
			return rest != null && rest.GetHealthRatio() < 1 ? rest : null;
		}

		// Among equal points, a healer first, then a tank, then everyone else (concept 07).
		static int TieBreakRank(IBattleChara member)
		{
			return member.IsJobCategory(JobRole.Healer) ? 0
				: member.IsJobCategory(JobRole.Tank) ? 1
				: 2;
		}

		IBattleChara? FindInterruptTarget()
		{
			if (battleChara == null || DataCenter.InterruptTarget == null)
			{
				return null;
			}

			foreach (var o in battleChara)
			{
				if (o.GameObjectId == DataCenter.InterruptTarget.GameObjectId)
				{
					return DataCenter.InterruptTarget;
				}
			}
			return null;
		}

		IBattleChara? FindHostile()
		{
			if (battleChara == null)
			{
				return null;
			}

			// Manual Any() check
			var hasAny = false;
			foreach (var _ in battleChara)
			{
				hasAny = true;
				break;
			}
			if (!hasAny)
			{
				return null;
			}

			// Filter out characters marked with stop markers
			if (Service.Config.FilterStopMark2 && !DataCenter.IsPvP)
			{
				var filteredCharacters = MarkingHelper.FilterStopCharacters(battleChara);
				// Manual Any() check
				var filteredHasAny = false;
				if (filteredCharacters != null)
				{
					foreach (var _ in filteredCharacters)
					{
						filteredHasAny = true;
						break;
					}
				}
				if (filteredCharacters != null && filteredHasAny)
				{
					battleChara = filteredCharacters;
				}
			}

			// Filter high priority hostiles. The targeting pick ignores list order, so numbered
			// treasure hunt mobs are narrowed to the lowest number present instead of sorted.
			var highPriorityHostiles = new List<IBattleChara>();
			var bestIconPriority = int.MaxValue;
			foreach (var b in battleChara)
			{
				if (!ObjectHelper.IsTopPriorityHostile(b) && !ObjectHelper.IsTopPriorityNamedHostile(b))
				{
					continue;
				}

				var iconPriority = ObjectHelper.GetNamePlateIconPriority(b.GetNamePlateIcon());
				if (iconPriority > bestIconPriority)
				{
					continue;
				}

				if (iconPriority < bestIconPriority)
				{
					bestIconPriority = iconPriority;
					highPriorityHostiles.Clear();
				}

				highPriorityHostiles.Add(b);
			}
			if (highPriorityHostiles.Count > 0)
			{
				battleChara = highPriorityHostiles;
			}

			return FindHostileRaw();
		}

		IBattleChara? FindHostileRaw()
		{
			if (battleChara == null)
			{
				return null;
			}

			// A damage over time being spread orders the candidates itself (DotSpreadOrder), after the stop
			// markers and the top-priority hostiles above have had their say; an explicit command still wins.
			if (commandTargeting == null && hostileOrder != null)
			{
				return MinBy(battleChara, hostileOrder);
			}

			return PickByTargeting(battleChara, ToTargetType(commandTargeting ?? DataCenter.TargetingType));
		}

		IBattleChara? FindBeAttackedTarget()
		{
			if (battleChara == null)
			{
				return null;
			}

			var hasAny = false;
			foreach (var _ in battleChara)
			{
				hasAny = true;
				break;
			}
			if (!hasAny)
			{
				return null;
			}

			// A tankbuster aimed at one of them names whom the help is for (IsTankbusterTargetToProtect): a healer's
			// Aquaveil or Exaltation in the single-target defence went to whoever their own target attacks - the main
			// tank - while the marker or the cast named the off-tank (A328). Without one among them, as before.
			List<IBattleChara> attachedT = [];
			foreach (var t in battleChara)
			{
				if (IsTankbusterTargetToProtect(t))
				{
					attachedT.Add(t);
				}
			}

			// Several aimed at, the help goes to the one standing with the least protection (LeastProtected).
			attachedT = LeastProtected(attachedT);

			if (attachedT.Count == 0)
			{
				foreach (var t in battleChara)
				{
					if (ObjectHelper.IsTargetOnSelf(t) && !TankbusterNeedsNothingFromYou(t))
					{
						attachedT.Add(t);
					}
				}
			}

			if (!DataCenter.AutoStatus.HasFlag(AutoStatus.DefenseSingle))
			{
				// If attachedT is empty, try tanks with TankStanceStatus
				if (attachedT.Count == 0)
				{
					foreach (var tank in battleChara)
					{
						if (tank.HasStatus(false, StatusHelper.TankStanceStatus))
						{
							attachedT.Add(tank);
						}
					}
				}

				// If still empty, try tanks by job category
				if (attachedT.Count == 0)
				{
					foreach (var tank in battleChara.GetJobCategory(JobRole.Tank))
					{
						attachedT.Add(tank);
					}
				}

				// If still empty, fallback to all
				if (attachedT.Count == 0)
				{
					foreach (var t in battleChara)
					{
						attachedT.Add(t);
					}
				}
			}

			// Select target based on Priolowtank config
			if (attachedT.Count == 0)
			{
				return null;
			}

			if (Service.Config.Priolowtank)
			{
				IBattleChara? lowest = null;
				var minHealth = float.MaxValue;
				foreach (var t in attachedT)
				{
					var health = ObjectHelper.GetHealthRatio(t);
					if (health < minHealth)
					{
						minHealth = health;
						lowest = t;
					}
				}
				return lowest;
			}
			else
			{
				IBattleChara? highest = null;
				var maxHealth = float.MinValue;
				foreach (var t in attachedT)
				{
					var health = ObjectHelper.GetHealthRatio(t);
					if (health > maxHealth)
					{
						maxHealth = health;
						highest = t;
					}
				}
				return highest;
			}
		}

		IBattleChara? FindDispelTarget()
		{
			if (battleChara != null && DataCenter.DispelTarget != null)
			{
				foreach (var o in battleChara)
				{
					if (o.GameObjectId == DataCenter.DispelTarget.GameObjectId)
					{
						return DataCenter.DispelTarget;
					}
				}
			}
			return null;
		}

		IBattleChara? FindTankTarget()
		{
			if (DataCenter.PartyMembers == null)
			{
				return null;
			}

			// Never the player: the one tank action that asks for "a tank" is Shirk, which cannot
			// target its user, and a tank in stance is first in the party list - so the manual Shirk
			// command picked the main tank himself and went nowhere. A healer is never a tank, so the
			// heal-over-time callers are unchanged.
			var self = Player.Object?.GameObjectId;
			foreach (var m in DataCenter.PartyMembers)
			{
				if (m.IsJobCategory(JobRole.Tank) && !m.IsDead && m.GameObjectId != self)
				{
					// 1. Tanks with tank stance
					if (m.HasStatus(false, StatusHelper.TankStanceStatus))
					{
						if (m.IsConditionCannotTarget())
						{
							continue;
						}

						if (Service.Config.InDebug)
						{
							PluginLog.Debug($"FindTankTarget 1: {m.Name} is a tank with TankStanceStatus.");
						}
						return m;
					}
				}
			}

			foreach (var m in DataCenter.PartyMembers)
			{
				if (m.IsJobCategory(JobRole.Tank) && !m.IsDead && m.GameObjectId != self)
				{
					if (m.IsConditionCannotTarget())
					{
						continue;
					}

					if (Service.Config.InDebug)
					{
						PluginLog.Debug($"FindTankTarget 2: {m.Name} is a tank fallback.");
					}
					return m;
				}
			}

			if (battleChara == null)
			{
				return null;
			}

			List<IBattleChara> others = [];
			foreach (var b in battleChara)
			{
				if (b.GameObjectId != self)
				{
					others.Add(b);
				}
			}

			return RandomPickByJobs(others, JobRole.Tank);
		}

		// A member a tankbuster is aimed at (DataCenter.IsTankbusterAimedAt), unless, under "hold mitigation under
		// invulnerability", every such tankbuster lands while he stands impervious - his proposal of 04.10.2026: a
		// tank the hit cannot touch gets nothing from a mitigation on him (concept 09).
		static bool IsTankbusterTargetToProtect(IBattleChara t)
		{
			return DataCenter.IsTankbusterAimedAt(t)
				&& !(Service.Config.HoldMitigationUnderInvulnerability
					&& (DataCenter.ImperviousPastAimedTankbuster(t) || MeasuredHitCallsForNoHelp(t)));
		}

		// The member the tankbuster filter above passed over, in the single defence the player has not commanded: not
		// taken back as the one his target attacks. A BossModReborn prediction keeps the healer's single defence open
		// beside such a tankbuster (StateUpdater, review of A260), and through this fallback the help went to the tank
		// the boss attacks all the same - Aquaveil on a tank under Hallowed Ground (A373). Heal paths and the area
		// defence keep their target: neither is help for this hit.
		static bool TankbusterNeedsNothingFromYou(IBattleChara t)
		{
			return !IBaseAction.HealPathRunning && !IBaseAction.AreaDefenceRunning && !IBaseAction.CommandedDefenceRunning
				&& DataCenter.IsTankbusterAimedAt(t) && !IsTankbusterTargetToProtect(t);
		}

		// Help from you for another member's measured tankbuster goes where the hit calls for it, by the size rule of
		// the area defence (DataCenter.AreaShareCallsForMitigation): a hit at or above what a large shield absorbs, or
		// one that leaves him below the health at which this rotation heals him by itself - mitigation where healing
		// would otherwise be needed. Below both, a White Mage spent Divine Benison and Aquaveil on a hit of 20 % of a
		// tank's HP (his trace of 25.10.2026, A364). Not for the player, whose own plan decides for him, not in the
		// single defence he commands, and not for a heal (Aurora, Heart of Corundum in the heal path) (A373).
		static bool MeasuredHitCallsForNoHelp(IBattleChara t)
		{
			if (IBaseAction.HealPathRunning || IBaseAction.AreaDefenceRunning || IBaseAction.CommandedDefenceRunning
				|| Player.Object is not { } self || t.GameObjectId == self.GameObjectId
				|| !DataCenter.AimedTankbusterLeaves(t, out var actionId, out var share, out var left))
			{
				return false;
			}

			var line = Service.Config.HealthSingleSpell;
			if (share >= DefensiveValues.LargestStatedBarrierShare || left < line)
			{
				return false;
			}

			DataCenter.NoteTankbusterHelpHeld(t, actionId, share, left, line);
			return true;
		}

		IBattleChara? FindTankbusterTarget()
		{
			// Whom a tankbuster is aimed at - marker, targeted cast, BossModReborn's mask for the player
			// (DataCenter.IsTankbusterAimedAt) - among the members this action can take: in range, its own filters
			// passed. The player first, as "prioritizing self" says. Until A328 this read only the marker list, which
			// for a tank held only a marker on himself, so a cast at him or any tankbuster on the co-tank found no one,
			// and it returned a list entry the action could not take when none was usable.
			if (battleChara == null || Player.Object == null)
			{
				return null;
			}

			List<IBattleChara> others = [];
			foreach (var t in battleChara)
			{
				if (!ObjectHelper.IsAlive(t) || t.IsConditionCannotTarget() || !IsTankbusterTargetToProtect(t))
				{
					continue;
				}

				if (t.GameObjectId == Player.Object.GameObjectId)
				{
					return t;
				}

				others.Add(t);
			}

			// Several others aimed at, the help goes to the one standing with the least protection (LeastProtected).
			others = LeastProtected(others);
			return others.Count > 0 ? others[0] : null;
		}

		// Of several members a tankbuster is aimed at, those standing with the fewest damage-reducing effects - a rated
		// mitigation (DefensiveValues.MitigationByStatusId) or a barrier (StatusHelper.ShieldStatus), from anyone.
		// Without it every help went to the same one of them, by health: with two tanks under one double tankbuster
		// a White Mage put Divine Benison and Aquaveil on one tank, whose hit the barrier took whole, while the other
		// took 74 % of his HP under a vulnerability (his trace of 25.10.2026, A364). One member, or all equal: as given.
		static List<IBattleChara> LeastProtected(List<IBattleChara> aimedAt)
		{
			if (aimedAt.Count <= 1)
			{
				return aimedAt;
			}

			var fewest = int.MaxValue;
			List<IBattleChara> least = [];
			foreach (var t in aimedAt)
			{
				var count = ProtectionsOn(t);
				if (count < fewest)
				{
					fewest = count;
					least.Clear();
				}

				if (count == fewest)
				{
					least.Add(t);
				}
			}

			return least;
		}

		static int ProtectionsOn(IBattleChara member)
		{
			var count = 0;
			var statuses = member.StatusList;
			if (statuses == null)
			{
				return 0;
			}

			foreach (var status in statuses)
			{
				if (status != null && status.StatusId != 0
					&& (DefensiveValues.MitigationByStatusId.ContainsKey(status.StatusId)
						|| Array.IndexOf(StatusHelper.ShieldStatus, (StatusID)status.StatusId) >= 0))
				{
					count++;
				}
			}

			return count;
		}

		//TODO: Capture
		//IBattleChara? FindCaptureTarget()
		//{
		//	if (battleChara == null || Player.Object == null)
		//	{
		//		return null;
		//	}

		//	IBattleChara? closest = null;
		//	var closestDistance = float.MaxValue;

		//	foreach (var t in battleChara)
		//	{
		//		var petId = DataCenter.GetPetIdFromModel(t);
		//		if (petId == 0 || DataCenter.PetUnlocked(petId))
		//		{
		//			continue;
		//		}

		//		var distance = Player.DistanceTo(t.Position);
		//		if (distance < closestDistance)
		//		{
		//			closestDistance = distance;
		//			closest = t;
		//		}
		//	}

		//	return closest;
		//}
	}

	private static IBattleChara? FindMimicryTarget()
	{
		if (DataCenter.AllTargets == null)
		{
			return null;
		}

		IBattleChara? bestTarget = null;
		var bestDistance = float.MaxValue;

		foreach (var target in DataCenter.AllTargets)
		{
			if (target != null && IsNeededRole(target))
			{
				var distance = Player.Object != null ? Player.DistanceTo(target.Position) : float.MaxValue;
				if (distance < bestDistance)
				{
					bestDistance = distance;
					bestTarget = target;
				}
			}
		}

		return bestTarget;
	}

	// One pass instead of sorting the whole list for its first element.
	private static IBattleChara? PickByTargeting(IEnumerable<IBattleChara> candidates, TargetType type)
	{
		var role = type switch
		{
			TargetType.PvPHealers => JobRole.Healer,
			TargetType.PvPTanks => JobRole.Tank,
			TargetType.PvPDPS => JobRole.AllDPS,
			_ => JobRole.None,
		};

		if (role != JobRole.None)
		{
			var jobs = role.ToJobs();
			return MinBy(candidates, NearestFirst, c => c.IsJobs(jobs)) ?? MinBy(candidates, NearestFirst);
		}

		return MinBy(candidates, GetTargetingComparison(type));
	}

	private static IBattleChara? MinBy(IEnumerable<IBattleChara> candidates, Comparison<IBattleChara> comparison, Func<IBattleChara, bool>? filter = null)
	{
		IBattleChara? best = null;
		foreach (var candidate in candidates)
		{
			if (candidate != null && (filter == null || filter(candidate)) && (best == null || comparison(candidate, best) < 0))
			{
				best = candidate;
			}
		}

		return best;
	}

	private static readonly Comparison<IBattleChara> NearestFirst = static (a, b) => a.DistanceToPlayer().CompareTo(b.DistanceToPlayer());

	private static Comparison<IBattleChara> GetTargetingComparison(TargetType type)
	{
		return type switch
		{
			TargetType.Small => static (a, b) => CompareHitboxThenHp(a, b, ascendingHitbox: true, Service.Config.SmallHp),
			TargetType.HighHP => static (a, b) => b.CurrentHp.CompareTo(a.CurrentHp),
			TargetType.LowHP => static (a, b) => a.CurrentHp.CompareTo(b.CurrentHp),
			TargetType.HighHPPercent => static (a, b) => HpPercent(b).CompareTo(HpPercent(a)),
			TargetType.LowHPPercent => static (a, b) => HpPercent(a).CompareTo(HpPercent(b)),
			TargetType.HighMaxHP => static (a, b) => b.MaxHp.CompareTo(a.MaxHp),
			TargetType.LowMaxHP => static (a, b) => a.MaxHp.CompareTo(b.MaxHp),
			TargetType.Nearest => NearestFirst,
			TargetType.Farthest => static (a, b) => b.DistanceToPlayer().CompareTo(a.DistanceToPlayer()),
			// The Big case reads its own setting; it read SmallHp, and BigHp had no reader (A225).
			_ => static (a, b) => CompareHitboxThenHp(a, b, ascendingHitbox: false, Service.Config.BigHp),
		};

		static float HpPercent(IBattleChara c)
		{
			return c.MaxHp == 0 ? 0 : (float)c.CurrentHp / c.MaxHp;
		}

		// Ties on hitbox size go to the lower HP target when the case's own setting ("Small HP", "Big HP") is on,
		// otherwise to the higher.
		static int CompareHitboxThenHp(IBattleChara a, IBattleChara b, bool ascendingHitbox, bool lowerHpFirst)
		{
			var cmp = ascendingHitbox ? a.HitboxRadius.CompareTo(b.HitboxRadius) : b.HitboxRadius.CompareTo(a.HitboxRadius);
			if (cmp != 0)
			{
				return cmp;
			}

			return lowerHpFirst ? a.CurrentHp.CompareTo(b.CurrentHp) : b.CurrentHp.CompareTo(a.CurrentHp);
		}
	}

	private static TargetType ToTargetType(TargetingType targetingType) => targetingType switch
	{
		TargetingType.Small => TargetType.Small,
		TargetingType.HighHP => TargetType.HighHP,
		TargetingType.LowHP => TargetType.LowHP,
		TargetingType.HighHPPercent => TargetType.HighHPPercent,
		TargetingType.LowHPPercent => TargetType.LowHPPercent,
		TargetingType.HighMaxHP => TargetType.HighMaxHP,
		TargetingType.LowMaxHP => TargetType.LowMaxHP,
		TargetingType.Nearest => TargetType.Nearest,
		TargetingType.Farthest => TargetType.Farthest,
		TargetingType.PvPHealers => TargetType.PvPHealers,
		TargetingType.PvPTanks => TargetType.PvPTanks,
		TargetingType.PvPDPS => TargetType.PvPDPS,
		_ => TargetType.Big,
	};

	private static bool IsNeededRole(IBattleChara character)
	{
		if (Player.Object == null)
		{
			return false;
		}

		if (character.GameObjectId == Player.Object.GameObjectId)
		{
			return false;
		}

		if (character is not IPlayerCharacter player || player.ClassJob.Value.IsLimitedJob)
		{
			return false;
		}

		var neededRole = DataCenter.BluRole;
		return neededRole != null && neededRole != CombatRole.NonCombat && (int)neededRole == (int)player.GetRole();
	}

	private static readonly StatusID[] PoorTargetStatuses = [StatusID.DamageDown_2911, StatusID.DamageDown, StatusID.Weakness, StatusID.BrinkOfDeath];
	private static readonly StatusID[] PoorOrPartneredStatuses = [.. PoorTargetStatuses, StatusID.DancePartner, StatusID.ClosedPosition];

	// The first living, targetable party member matching the job priority list, in list order.
	private static IBattleChara? FirstByJobPriority(List<Job> priority, Func<IBattleChara, bool> predicate)
	{
		foreach (var job in priority)
		{
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member.IsJobs(job) && !member.IsDead && !member.IsConditionCannotTarget() && predicate(member))
				{
					return member;
				}
			}
		}

		return null;
	}

	// AST cards: highest priority job first, ties to more max HP, then more current HP.
	private static IBattleChara? FindCardTarget(List<Job> priority, StatusID cardStatus, bool preferRanged)
	{
		var player = Player.Object;
		if (!TargetFilter.PlayerIsJobs(Job.AST) || player == null)
		{
			return null;
		}

		var party = DataCenter.PartyMembers;
		var playerId = player.GameObjectId;

		// Alone, or only with the player's chocobo.
		if (party.Count == 1 || (party.Count == 2 && party.Exists(m => m.GameObjectId == playerId) && party.Exists(ObjectHelper.IsPlayerCharacterChocobo)))
		{
			return player;
		}

		List<IBattleChara> candidates = [];
		foreach (var m in party)
		{
			if (m == null || m.GameObjectId == playerId || m.IsPlayerCharacterChocobo() || m.IsDead || m.IsConditionCannotTarget()
				|| m.HasStatus(false, PoorTargetStatuses) || m.HasStatus(false, cardStatus))
			{
				continue;
			}

			candidates.Add(m);
		}

		IBattleChara? best = null;
		var bestIndex = int.MaxValue;
		foreach (var member in candidates)
		{
			var index = priority.FindIndex(job => member.IsJobs(job));
			if (index < 0)
			{
				continue;
			}

			if (best == null || index < bestIndex
				|| (index == bestIndex && (member.MaxHp > best.MaxHp || (member.MaxHp == best.MaxHp && member.CurrentHp > best.CurrentHp))))
			{
				best = member;
				bestIndex = index;
			}
		}

		if (best != null)
		{
			return best;
		}

		return preferRanged
			? RandomRangeTarget(candidates) ?? RandomMeleeTarget(candidates) ?? RandomMagicalTarget(candidates) ?? RandomPhysicalTarget(candidates)
			: RandomMeleeTarget(candidates) ?? RandomRangeTarget(candidates) ?? RandomMagicalTarget(candidates) ?? RandomPhysicalTarget(candidates);
	}
	internal static IBattleChara? RandomPhysicalTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, Job.VPR, Job.WAR, Job.GNB, Job.MNK, Job.SAM, Job.DRG, Job.MCH, Job.DNC)
			?? RandomPickByJobs(tars, Job.PLD, Job.DRK, Job.NIN, Job.BRD, Job.RDM)
			?? RandomObject(tars);
	}

	internal static IBattleChara? RandomMagicalTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, Job.PCT, Job.SCH, Job.AST, Job.SGE, Job.BLM, Job.SMN)
			?? RandomPickByJobs(tars, Job.PLD, Job.DRK, Job.NIN, Job.BRD, Job.RDM)
			?? RandomObject(tars);
	}

	internal static IBattleChara? RandomRangeTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, JobRole.RangedMagical, JobRole.RangedPhysical, JobRole.Melee)
			?? RandomPickByJobs(tars, JobRole.Tank, JobRole.Healer)
			?? RandomObject(tars);
	}

	internal static IBattleChara? RandomMeleeTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, JobRole.Melee, JobRole.RangedMagical, JobRole.RangedPhysical)
			?? RandomPickByJobs(tars, JobRole.Tank, JobRole.Healer)
			?? RandomObject(tars);
	}

	private static IBattleChara? RandomPickByJobs(IEnumerable<IBattleChara> tars, params JobRole[] roles)
	{
		foreach (var role in roles)
		{
			var tar = RandomPickByJobs(tars, role.ToJobs());
			if (tar != null)
			{
				if (tar.IsConditionCannotTarget())
				{
					continue;
				}

				return tar;
			}
		}
		return null;
	}

	private static IBattleChara? RandomPickByJobs(IEnumerable<IBattleChara> tars, params Job[] jobs)
	{
		List<IBattleChara> targets = [];
		foreach (var t in tars)
		{
			if (t.IsConditionCannotTarget())
			{
				continue;
			}

			if (t.IsJobs(jobs))
			{
				targets.Add(t);
			}
		}
		return targets.Count > 0 ? RandomObject(targets) : null;
	}

	private static IBattleChara? RandomObject(IEnumerable<IBattleChara> objs)
	{
		List<IBattleChara> list = [];
		foreach (var o in objs)
		{
			if (o == null)
			{
				continue;
			}

			if (o.IsConditionCannotTarget())
			{
				continue;
			}

			list.Add(o);
		}

		var count = list.Count;
		if (count == 0)
		{
			return null;
		}

		var index = Random.Shared.Next(count);
		return list[index];
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
	TankSwap
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// The target result
/// </summary>
/// <param name="Target">the target.</param>
/// <param name="AffectedTargets">the targets that be affected by this action.</param>
/// <param name="Position">the position to use this action.</param>
public readonly record struct TargetResult(IBattleChara Target, IBattleChara[] AffectedTargets, Vector3? Position);