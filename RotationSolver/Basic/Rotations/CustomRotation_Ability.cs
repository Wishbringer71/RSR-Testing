using ECommons.DalamudServices;
using ECommons.ExcelServices;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	private const float TrueNorthLastWeaveRemain = 1.35f;

	/// <param name="nextGCD">The GCD that follows.</param>
	/// <param name="act">The ability, if one goes.</param>
	/// <param name="emergencyHealOnly">Asked by Invoke while the GCD is due: only a heal ability for a member in the
	/// critical class, past the clipping lock, behind every other gate (concept 07, "Die Notfallheilung zwischen zwei
	/// Zaubern").</param>
	private bool Ability(IAction nextGCD, out IAction? act, bool emergencyHealOnly = false)
	{
		// A command or an item waits for an ordinary window; only the emergency heal goes ahead of the GCD.
		act = emergencyHealOnly ? null : DataCenter.CommandNextAction;

		// GCD() may have returned from inside its heal path.
		IBaseAction.HealPathRunning = false;

		// ... or from inside any path that set the target override - heal, dispel, provoke, raise, defence. The
		// command, the emergency abilities, the tankbuster plan and the swap below run before the first path here
		// sets its own, and searched their targets by the GCD's: after a heal GCD a damage ability went to the
		// enemy with the least HP, and the paladin's emergency Intervention, which skips the stance check, to the
		// member with the least (A305). Each of them searches by its own type now.
		IBaseAction.TargetOverride = null;

		if (Player == null)
		{
			return false;
		}

		if (DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting))
		{
			return false;
		}

		// The last half second before the GCD stays free so an ability does not clip it - unless Invoke found no GCD to
		// cast and asks with IgnoreClipping: then there is nothing to clip (the flag's purpose since 4ec1a3a1c, A388).
		// The emergency heal passes it: the GCD is due, and the ability is pressed the moment it comes free.
		if (!emergencyHealOnly && !IBaseAction.IgnoreClipping && WeaponRemain <= ActionHelper.AbilityClipWindow && WeaponRemain > 0f)
		{
			return false;
		}

		if (DataCenter.Orbonne && IsLastAction(ActionID.HeavenlyShieldPvE) && DataCenter.IsAgriasCastingSpecialIndicator())
		{
			return false;
		}

		if (Service.Config.PldlockCasting && DataCenter.Job == Job.PLD && !DataCenter.IsMoving && IsLastAction(ActionID.PassageOfArmsPvE) && StatusHelper.PlayerHasStatus(true, StatusID.PassageOfArms) && DataCenter.AreaHitPending)
		{
			return false;
		}

		if (Service.Config.AstlockCasting && DataCenter.Job == Job.AST && !DataCenter.IsMoving && IsLastAction(ActionID.CollectiveUnconsciousPvE) && StatusHelper.PlayerHasStatus(true, StatusID.CollectiveUnconscious_848) && DataCenter.AreaHitPending)
		{
			return false;
		}

		if (Service.Config.BlulockCasting && DataCenter.Job == Job.BLU && !DataCenter.IsMoving && IsLastAction(ActionID.PhantomFlurryPvE) && StatusHelper.PlayerHasStatus(true, StatusID.PhantomFlurry) && !StatusHelper.PlayerWillStatusEnd(1, true, StatusID.PhantomFlurry))
		{
			return false;
		}

		using (new ForceEnableScope())
		{
			if (act is IBaseAction a && !a.Info.IsRealGCD && a.CanUse(out _, usedUp: true, skipAoeCheck: true, skipStatusProvideCheck: true))
			{
				return true;
			}
		}

		if (DataCenter.Job == Job.NIN && StatusHelper.PlayerHasStatus(true, StatusID.Mudra))
		{
			return false;
		}

		if (DataCenter.IsPvP && Service.Config.PvpGuardControl && HasPVPGuard)
		{
			return false;
		}

		if (act is IBaseItem i && i.CanUse(out _, true))
		{
			return true;
		}

		if (!Service.Config.UseAbility || Player.TotalCastTime > 0 || (StatusHelper.PlayerHasStatus(false, StatusID.ShackledAbilities) && DataCenter.NumberOfPartyMembersInRangeOf(8) > 1))
		{
			act = null;
			return false;
		}

		if (emergencyHealOnly)
		{
			return EmergencyHealAheadOfGcd(nextGCD, out act);
		}

		if (InvulnerabilityAgainstReportedDeath(out act))
		{
			return true;
		}

		if (!DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting) && ActiveDutyRotation?.EmergencyAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (!DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting) && EmergencyAbility(nextGCD, out act))
		{
			return true;
		}

		var role = DataCenter.Role;

		// The plan for a measured tankbuster - the mitigation it needs, or the invulnerability when nothing
		// less survives it - before the swap, which holds while the invulnerability is ready (concept 09).
		if (role == JobRole.Tank && TankbusterPlanAbility(out act))
		{
			return true;
		}

		// After the emergency abilities, so an invulnerability that fires at its HP threshold goes first.
		if (role == JobRole.Tank && Service.Config.ShirkToSwapAfterTankbuster
			&& (TankSwapAbility(out act) || TankSwapBackAbility(out act)))
		{
			return true;
		}

		IBaseAction.TargetOverride = TargetType.Interrupt;
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.Interrupt) && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra))
		{
			if (ActiveDutyRotation?.InterruptAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (MyInterruptAbility(role, nextGCD, out act))
			{
				return true;
			}
		}

		IBaseAction.TargetOverride = TargetType.Dispel;
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.Dispel))
		{
			if (ActiveDutyRotation?.DispelAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (DispelAbility(nextGCD, out act))
			{
				return true;
			}
		}

		IBaseAction.TargetOverride = TargetType.Tank;
		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Shirk))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.Shirk) && ShirkPvE.CanUse(out act))
		{
			return true;
		}
		IBaseAction.ShouldEndSpecial = false;

		IBaseAction.TargetOverride = null;
		if (DataCenter.CommandStatus.HasFlag(AutoStatus.TankStance))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.TankStance) && (TankStance?.CanUse(out act) ?? false))
		{
			return true;
		}
		IBaseAction.ShouldEndSpecial = false;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.AntiKnockback))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.AntiKnockback) && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra))
		{
			if (ActiveDutyRotation?.AntiKnockbackAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (AntiKnockback(role, nextGCD, out act))
			{
				return true;
			}
		}
		IBaseAction.ShouldEndSpecial = false;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Positional))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		var trueNorthWindow = DataCenter.CommandStatus.HasFlag(AutoStatus.Positional) || WeaponRemain <= TrueNorthLastWeaveRemain;
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.Positional) && trueNorthWindow && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra) && !StatusHelper.PlayerHasStatus(true, StatusID.TrueNorth) && TrueNorthPvE.Cooldown.CurrentCharges > 0 && !IsLastAbility(false, TrueNorthPvE) && TrueNorthPvE.CanUse(out act, skipComboCheck: true, usedUp: true))
		{
			return true;
		}
		IBaseAction.ShouldEndSpecial = false;

		IBaseAction.TargetOverride = TargetType.Heal;
		IBaseAction.HealPathRunning = true;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealAreaAbility))
		{
			IBaseAction.AllEmpty = true;
			IBaseAction.ShouldEndSpecial = true;
			if (ActiveDutyRotation?.HealAreaAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (CanCastHeals)
			{
				if (HealAreaAbility(nextGCD, out act))
				{
					return true;
				}
			}
			IBaseAction.AllEmpty = false;
			IBaseAction.ShouldEndSpecial = false;
		}

		if (AutoHealAreaAbility(nextGCD, out act))
		{
			return true;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleAbility))
		{
			IBaseAction.AllEmpty = true;
			IBaseAction.ShouldEndSpecial = true;
			if (ActiveDutyRotation?.HealSingleAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (CanCastHeals)
			{
				if (HealSingleAbility(nextGCD, out act))
				{
					return true;
				}
			}
			IBaseAction.AllEmpty = false;
			IBaseAction.ShouldEndSpecial = false;
		}

		if (AutoHealSingleAbility(nextGCD, out act))
		{
			return true;
		}

		IBaseAction.TargetOverride = null;
		IBaseAction.HealPathRunning = false;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Speed))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Speed))
		{
			if (ActiveDutyRotation?.SpeedAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (SpeedAbility(nextGCD, out act))
			{
				return true;
			}
		}
		IBaseAction.ShouldEndSpecial = false;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Provoke))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.Provoke))
		{
			if (!HasTankStance && (TankStance?.CanUse(out act) ?? false))
			{
				return true;
			}

			IBaseAction.TargetOverride = TargetType.Provoke;
			if (ProvokePvE.CanUse(out act))
			{
				// A take-over from a co-tank in danger (TankSwapWatch) goes into the trace with its reason, and its
				// outcome is settled by the enemy's next tankbuster.
				if (TankSwapWatch.TakeOverSourceId != 0 && ProvokePvE.Target.Target?.GameObjectId == TankSwapWatch.TakeOverSourceId)
				{
					TankSwapWatch.TakeOverChosen();
					DefenseTrace.Decision($"take over {ProvokePvE.Target.Target?.Name.TextValue} ({TankSwapWatch.TakeOverReason})", act);
				}

				return true;
			}

			if (ProvokeAbility(nextGCD, out act))
			{
				return true;
			}
		}
		IBaseAction.ShouldEndSpecial = false;

		IBaseAction.TargetOverride = TargetType.BeAttacked;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseArea))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseArea))
		{
			// A command from the player is not second-guessed; the automatic flag asks for the party,
			// so the actions that protect only the player ask whether the hit reaches him (A233).
			IBaseAction.SelfProtectionHitsMe = DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseArea)
				|| DataCenter.AreaHitReachesPlayer;
			IBaseAction.AreaDefenceRunning = true;
			DefenseTrace.BeginPath();
			if (ActiveDutyRotation?.DefenseAreaAbility(nextGCD, out act) == true)
			{
				IBaseAction.SelfProtectionHitsMe = null;
				IBaseAction.AreaDefenceRunning = false;
				DefenseTrace.Decision("area defence (duty)", act);
				return true;
			}
			// A damage dealer's single-target defence protects only himself, so under the area flag -
			// which asks whether the party is hit - it runs only when the hit reaches him (A218).
			if (DefenseAreaAbility(nextGCD, out act) || (role is JobRole.Melee or JobRole.RangedPhysical or JobRole.RangedMagical
				&& DataCenter.AreaHitReachesPlayer && DefenseSingleAbility(nextGCD, out act)))
			{
				IBaseAction.SelfProtectionHitsMe = null;
				IBaseAction.AreaDefenceRunning = false;
				DefenseTrace.Decision("area defence", act);
				return true;
			}
			DefenseTrace.NothingChosen("area defence");
			IBaseAction.SelfProtectionHitsMe = null;
			IBaseAction.AreaDefenceRunning = false;
		}
		IBaseAction.ShouldEndSpecial = false;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseSingle))
		{
			// The single-target flag also carries help for the other tank, so it stays; the actions that
			// protect only the player ask whether the single hit reaches him (A233).
			IBaseAction.SelfProtectionHitsMe = DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle)
				|| DataCenter.SingleHitReachesPlayer;
			IBaseAction.CommandedDefenceRunning = DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle);
			DefenseTrace.BeginPath();
			try
			{
				if (ActiveDutyRotation?.DefenseSingleAbility(nextGCD, out act) == true)
				{
					IBaseAction.SelfProtectionHitsMe = null;
					DefenseTrace.Decision("single defence (duty)", act);
					return true;
				}
				if (DefenseSingleAbility(nextGCD, out act)
					// Arm's Length as the last resort only for its Slow on a pack of ordinary enemies: it does not
					// touch the hit that strikes it, and on a boss's tankbuster - the owner saw it cast there - it
					// does nothing and is then missing for a knockback. A rotation that runs the pull rule itself
					// has already asked, with its own option and holds, and declined (A236).
					|| (!HasOwnArmsLengthPullRule && ArmsLengthSlowsPull(true, Service.Config.AutoDefenseNumber) && !StatusHelper.PlayerHasStatus(true, StatusID.Vengeance) && !StatusHelper.PlayerHasStatus(true, StatusID.Damnation) && ArmsLengthPvE.CanUse(out act)))
				{
					IBaseAction.SelfProtectionHitsMe = null;
					DefenseTrace.Decision("single defence", act);
					return true;
				}
			}
			finally
			{
				IBaseAction.CommandedDefenceRunning = false;
			}
			DefenseTrace.NothingChosen("single defence");
			IBaseAction.SelfProtectionHitsMe = null;
		}
		IBaseAction.ShouldEndSpecial = false;

		IBaseAction.TargetOverride = null;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.MoveForward))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		IBaseAction.AllEmpty = true;
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveForward) && Player != null && !StatusHelper.PlayerHasStatus(true, StatusID.Bind))
		{
			if (ActiveDutyRotation?.MoveForwardAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (MoveForwardAbility(nextGCD, out act))
			{
				return true;
			}
		}
		IBaseAction.ShouldEndSpecial = false;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.MoveBack))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveBack))
		{
			if (ActiveDutyRotation?.MoveBackAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (MoveBackAbility(nextGCD, out act))
			{
				return true;
			}
		}
		IBaseAction.ShouldEndSpecial = false;
		IBaseAction.AllEmpty = false;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleAbility))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		// A potion carries its own decision and does not borrow the heal flag's.
		//
		// It has three switches of its own - the global setting, a per-item enable, and its own HP
		// percentage - and each of them says outright when a potion should go out. AutoStatus
		// .HealSingleAbility answers a different question, about healing ACTIONS: whether this job
		// should be casting Physick or Vercure right now. Gating the potion on it inherited every
		// condition behind that answer - AutoHeal, UseHealWhenNotAHealer, the time-to-kill cut-off,
		// HPNotFull, and OnlyHealAsNonHealIfNoHealers - and each of them can be false while the
		// player sits at 10 % with a potion in the bag.
		//
		// Owner's report: a Summoner reduced to 1 HP by a mechanic had no way to reach a potion at
		// all, and it came back once the potion stopped reading the flag. Which of the five held him
		// is not measurable from here; OnlyHealAsNonHealIfNoHealers is off by default, and with it
		// on a non-healer in a party with a living healer never gets the flag. His own argument:
		// "das flag ist im lowlevel für physick interessant oder für einen redmage mit seinem heal.
		// aber für potions?"
		//
		// InCombat stays, and it is this rule's own condition rather than a borrowed one: a potion
		// is an emergency consumable, and out of combat health returns on its own.
		if (DataCenter.InCombat && UseHpPotion(nextGCD, PotionsWaitForTincture(), out act))
		{
			return true;
		}
		IBaseAction.ShouldEndSpecial = false;

		// The last resort for a corpse nobody can raise properly. PhoenixDownItem.CanUseThis carries
		// the condition - a raise target exists and, under the healer logic, no living raiser is
		// left in the set the raise settings draw from - and BaseItem.CanUse asks the game through
		// GetActionStatus, so a duty that forbids items refuses it here without a check of our own.
		//
		// It sits behind healing on purpose: keeping someone alive beats picking someone up, and a
		// feather only costs a weave window. It was written long ago and never called at all.
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.Raise) && UsePhoenixDown(nextGCD, out act))
		{
			return true;
		}

		if (HasHostilesInRange && ActiveDutyRotation?.AttackAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (HasHostilesInRange && AttackAbility(nextGCD, out act))
		{
			return true;
		}

		if (ActiveDutyRotation?.GeneralAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (GeneralAbility(nextGCD, out act))
		{
			return true;
		}

		return UseMpPotion(nextGCD, PotionsWaitForTincture(), out act) || GeneralUsingAbility(role, nextGCD, out act) || (DataCenter.AutoStatus.HasFlag(AutoStatus.Speed) && SpeedAbility(nextGCD, out act));
	}

	private bool MyInterruptAbility(JobRole role, IAction nextGCD, out IAction? act)
	{
		// Job override first: its combo-safety gate must precede the ungated role default below.
		if (InterruptAbility(nextGCD, out act))
		{
			return true;
		}

		switch (role)
		{
			case JobRole.Tank:
				if (InterjectPvE.CanUse(out act))
				{
					return true;
				}

				break;

			case JobRole.Melee:
				// A job that gates this same action itself already declined it above, possibly because
				// its gate is active - retrying it ungated here would defeat that gate.
				if (!HasOwnInterruptGate && LegSweepPvE.CanUse(out act) && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra))
				{
					return true;
				}

				break;

			case JobRole.RangedPhysical:
				if (HeadGrazePvE.CanUse(out act))
				{
					return true;
				}

				break;

			default:
				break;
		}
		return false;
	}

	private static readonly List<IBattleChara> _criticalMembers = [];

	/// <summary>
	/// One trace line per heal and target, or per member left without one, and GCD: the emergency heal reported ahead
	/// of the GCD, or a member in the critical class for whom no heal ability could go.
	/// </summary>
	/// <returns>Always true, so the line can be written where the heal is accepted.</returns>
	private static bool TraceEmergencyHeal(IBaseAction? heal, IBattleChara critical, IAction nextGCD, string? potions = null)
	{
		var target = heal?.Target.Target;
		var key = (heal?.ID ?? 0u, heal != null ? target?.GameObjectId ?? 0ul : critical.GameObjectId);
		var now = DateTime.Now;
		var gcd = TimeSpan.FromSeconds(DataCenter.DefaultGCDTotal);
		if (_emergencyHealTraced.TryGetValue(key, out var last) && now - last < gcd)
		{
			return true;
		}

		// Keys of members and heals long gone go, so the table stays the size of one emergency.
		_staleTraceKeys.Clear();
		foreach (var entry in _emergencyHealTraced)
		{
			if (now - entry.Value >= gcd)
			{
				_staleTraceKeys.Add(entry.Key);
			}
		}
		foreach (var stale in _staleTraceKeys)
		{
			_ = _emergencyHealTraced.Remove(stale);
		}

		_emergencyHealTraced[key] = now;
		var forecast = critical.GetForecastEffectiveHpPercent();
		DefenseTrace.Line(heal != null
			? $"emergency heal ahead of {nextGCD.Name} -> {heal.Name} #{heal.ID}"
				+ (target != null ? $" on {target.Name.TextValue} at {target.GetHealthRatio():P0}" : string.Empty)
				+ $", critical {critical.Name.TextValue} at {forecast}% forecast"
			: $"critical class: {critical.Name.TextValue} at {forecast}% forecast, no heal ability for it ahead of {nextGCD.Name}"
				+ (potions != null ? $" | {potions}" : string.Empty));
		return true;
	}

	/// <summary>
	/// Determines if an interrupt ability can be used.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the interrupt ability can be used; otherwise, false.</returns>
	protected virtual bool InterruptAbility(IAction nextGCD, out IAction? act)
	{
		act = null;
		return false;
	}

	/// <summary>
	/// Determines if an interrupt ability can be used.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the interrupt ability can be used; otherwise, false.</returns>
	protected virtual bool DispelAbility(IAction nextGCD, out IAction? act)
	{
		act = null;
		return false;
	}

	private bool AntiKnockback(JobRole role, IAction nextGCD, out IAction? act)
	{
		// Job override first: its combo-safety gate must precede the ungated role default below.
		if (AntiKnockbackAbility(nextGCD, out act))
		{
			return true;
		}

		switch (role)
		{
			case JobRole.Tank:
				if (ArmsLengthPvE.CanUse(out act) && !StatusHelper.PlayerHasStatus(true, StatusID.InnerStrength))
				{
					return true;
				}

				break;
			case JobRole.Melee:
				// Same reasoning as MyInterruptAbility's Melee case: a job with its own gated
				// AntiKnockbackAbility override (RPR/VPR) already tried ArmsLengthPvE above and
				// declined, possibly due to its combo-safety gate rather than unavailability -
				// retrying it ungated here would defeat that gate.
				if (!HasOwnAntiKnockbackGate && ArmsLengthPvE.CanUse(out act) && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra))
				{
					return true;
				}

				break;
			case JobRole.Healer:
			case JobRole.RangedMagical:
				if (SurecastPvE.CanUse(out act))
				{
					return true;
				}

				break;
			case JobRole.RangedPhysical:
				if (ArmsLengthPvE.CanUse(out act))
				{
					return true;
				}

				break;
			default:
				break;
		}

		return false;
	}

	/// <summary>
	/// Determines if an anti-knockback ability can be used.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the anti-knockback ability can be used; otherwise, false.</returns>
	protected virtual bool AntiKnockbackAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	/// <summary>
	/// Determines if a provoke ability can be used.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the provoke ability can be used; otherwise, false.</returns>
	protected virtual bool ProvokeAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	private bool GeneralUsingAbility(JobRole role, IAction nextGCD, out IAction? act)
	{
		act = null;
		switch (role)
		{
			case JobRole.Tank:
				if (LowBlowPvE.CanUse(out act))
				{
					return true;
				}

				break;

			case JobRole.Melee:
				if (SecondWindPvE.CanUse(out act) && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra))
				{
					return true;
				}

				if (BloodbathPvE.CanUse(out act) && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra))
				{
					return true;
				}

				break;

			case JobRole.Healer:
			case JobRole.RangedMagical:
				if (Job == Job.BLM)
				{
					break;
				}

				if (LucidDreamingPvE.CanUse(out act))
				{
					return true;
				}

				break;

			case JobRole.RangedPhysical:
				if (SecondWindPvE.CanUse(out act))
				{
					return true;
				}

				break;

			default:
				break;
		}
		return false;
	}

	/// <summary>
	/// Determines if an emergency ability can be used.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the emergency ability can be used; otherwise, false.</returns>
	protected virtual bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		if (TryPvPEmergency(out act))
		{
			return true;
		}

		// The job's own Limit Break in PvP (concept 25, "PvP: Limit Break", A756).
		if (PvPLimitBreak(out act))
		{
			return true;
		}

		// A long cast takes Swiftcast - the job's own raise only where "Raise with Swiftcast or Dualcast" allows it, whose
		// text promises that off, RSR never presses Swiftcast for a raise; Resurrection and Verraise are long casts (A402). Any
		// other long cast is damage - Verthunder III and Veraero III on Red Mage - and takes Swiftcast only where it is
		// not held for the raise (A503).
		var nextIsOwnRaise = Raise != null && nextGCD.IsTheSameTo(true, Raise);
		if (nextGCD is BaseAction action && (nextIsOwnRaise ? Service.Config.RaisePlayerBySwift : SwiftcastForDamage))
		{
			if (Role is JobRole.RangedMagical && action.Info.CastTime >= 5 && IActionHelper.IsLastActionGCD() && SwiftcastPvE.CanUse(out act))
			{
				if (!nextGCD.IsTheSameTo(false, ActionID.MegaflarePvE, ActionID.ThunderstormPvE, ActionID.JudgmentBoltPvE, ActionID.HellfirePvE, ActionID.EarthenWallPvE))
				{
					return true;
				}
			}
		}

		if (Role is JobRole.RangedMagical && SwiftcastForDamage && !HasSwift && IActionHelper.IsLastActionGCD() && nextGCD.IsTheSameTo(true, ActionID.OccultCometPvE))
		{
			if (SwiftcastPvE.CanUse(out act))
			{
				return true;
			}
		}

		// Swiftcast for a raise, spent in the weave window where the execution gate lets an ability
		// through at all.
		//
		// The first condition is the original one: the raise is already the next GCD. It only ever
		// holds once Swiftcast is up, because RaiseSpell reports the raise from its HasSwift stage -
		// so on its own it can never be what *starts* the sequence.
		//
		// The second closes that circle. RaiseSpell's own Swiftcast attempt sits under
		// WeaponRemain <= 0.5f, and RSCommands_Actions.DoAction refuses every ability while
		// 0 < DefaultGCDRemain <= 0.5f - the same clock, mutually exclusive. The only state where
		// both agree is a GCD sitting at exactly 0, which in automatic mode is immediately spent on
		// an attack. That is why raising works at once in manual mode with the corpse hard-targeted
		// (no hostile target, so the GCD stays free) and takes an arbitrary time otherwise.
		//
		// Asking about the pending raise here instead of about nextGCD is deliberate. Naming the
		// raise as the next GCD was tried and reverted (AUDIT_LOG C37): it ends the GCD dispatcher
		// ahead of healing and damage, and it rewrites nextGCD for every branch that reads it -
		// several hundred in this tree, among them Radiant Aegis on Summoner, which stopped coming
		// out. This condition leaves the GCD path untouched and only adds one weave.
		//
		// The raise is the job's own Raise, not a list of four ids: the list had no Verraise and no
		// Angel Whisper, the same ageing as the hand-kept raise list behind the original defect.
		//
		// A693: in the late weave window, not the first. The raise comes with the next GCD all the same; the first window stays
		// for another ability (the White Mage's Thin Air, which the raise then takes), and the span in which another healer
		// raises the same corpse first shrinks to the late window.
		if (Service.Config.RaisePlayerBySwift && DataCenter.CanRaise() && CanLateWeave
			&& ((Raise != null && nextGCD.IsTheSameTo(true, Raise))
				|| RaisePendingAndCastable()))
		{
			if (SwiftcastPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (Role is JobRole.Melee or JobRole.RangedPhysical && StatusHelper.PlayerHasStatus(false, StatusHelper.DoomHealStatus)
			&& SecondWindPvE.CanUse(out act))
		{
			return true;
		}

		act = null;
		return false;
	}

	private bool TryPvPEmergency(out IAction? act)
	{
		act = null;
		if (!DataCenter.IsPvP)
		{
			return false;
		}

		if (DataCenter.Job != Job.BRD && DataCenter.Job != Job.WHM && PurifyPvP.CanUse(out act)
			&& ((Service.Config.PvpPurifyStun && StatusHelper.PlayerHasStatus(false, StatusID.Stun_1343))
				|| (Service.Config.PvpPurifyHeavy && StatusHelper.PlayerHasStatus(false, StatusID.Heavy_1344))
				|| (Service.Config.PvpPurifyBind && StatusHelper.PlayerHasStatus(false, StatusID.Bind_1345))
				|| (Service.Config.PvpPurifySilence && StatusHelper.PlayerHasStatus(false, StatusID.Silence_1347))
				|| (Service.Config.PvpPurifyDeepFreeze && StatusHelper.PlayerHasStatus(false, StatusID.DeepFreeze_3219))
				|| (Service.Config.PvpPurifyMiracleOfNature && StatusHelper.PlayerHasStatus(false, StatusID.MiracleOfNature))))
		{
			return true;
		}

		// Health forecast to the landing of an action begun now, as for the heal targets; no Guard where it is wasted or
		// cannot go (concept 25, "PvP: Gemeinsame Aktionen", A756).
		if (GuardPvP.CanUse(out act) && ObjectHelper.GetForecastPlayerHealthRatio() <= Service.Config.HealthForGuard
			&& !StatusHelper.PlayerHasStatus(false, StatusHelper.GuardWastedStatuses))
		{
			return true;
		}

		return RecuperatePvP.CanUse(out act) || StandardissueElixirPvP.CanUse(out act);
	}

	/// <summary>
	/// The ability that makes the character move forward.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	[RotationDesc(DescType.MoveForwardAbility)]
	protected virtual bool MoveForwardAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	/// <summary>
	/// The ability that makes the character move back.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	[RotationDesc(DescType.MoveBackAbility)]
	protected virtual bool MoveBackAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	/// <summary>
	/// The ability that heals a single character.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	[RotationDesc(DescType.HealSingleAbility)]
	protected virtual bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	/// <summary>
	/// The ability that heals an area.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	[RotationDesc(DescType.HealAreaAbility)]
	protected virtual bool HealAreaAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	/// <summary>
	/// The ability that defends a single character.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	[RotationDesc(DescType.DefenseSingleAbility)]
	protected virtual bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	/// <summary>
	/// The ability that defends an area.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	[RotationDesc(DescType.DefenseAreaAbility)]
	protected virtual bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{

		act = null;
		return false;
	}

	/// <summary>
	/// The ability that speeds up the character.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	[RotationDesc(DescType.SpeedAbility)]
	[RotationDesc(ActionID.SprintPvE)]
	protected virtual bool SpeedAbility(IAction nextGCD, out IAction? act)
	{
		if (PelotonPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (SprintPvE.CanUse(out act))
		{
			return true;
		}

		if (DataCenter.IsPvP && (!DataCenter.InCombat || (Service.Config.PvpAllowSprintWithoutTarget && Svc.Targets.Target == null)) && SprintPvP.CanUse(out act))
		{
			return true;
		}

		act = null;
		return false;
	}

	/// <summary>
	/// The ability that can be used anywhere.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	protected virtual bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		act = null;
		return false;
	}

	/// <summary>
	/// The ability that attacks an enemy.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the ability can be used; otherwise, false.</returns>
	protected virtual bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		act = null;
		return false;
	}

}
