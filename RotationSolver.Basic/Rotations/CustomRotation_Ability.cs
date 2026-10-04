using ECommons.DalamudServices;
using ECommons.ExcelServices;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// Determines if an ability can be used based on various conditions.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if an ability can be used; otherwise, false.</returns>
	private bool Ability(IAction nextGCD, out IAction? act)
	{
		act = DataCenter.CommandNextAction;

		if (Player == null)
		{
			return false;
		}

		if (DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting))
		{
			return false;
		}

		if (WeaponRemain <= 0.5f && WeaponRemain > 0f)
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

		IBaseAction.ForceEnable = true;
		if (act is IBaseAction a && a != null && !a.Info.IsRealGCD && a.CanUse(out _, usedUp: true, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}
		IBaseAction.ForceEnable = false;

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

		if (!DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting) && DataCenter.CurrentDutyRotation?.EmergencyAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (!DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting) && EmergencyAbility(nextGCD, out act))
		{
			return true;
		}

		var role = DataCenter.Role;

		// The invulnerability ahead of a tankbuster that the best case says kills - before the swap, which
		// holds while the invulnerability is ready (concept 09).
		if (role == JobRole.Tank && Service.Config.InvulnerabilityBeforeLethalTankbuster
			&& TankInvulnerabilityAbility(out act))
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
			if (DataCenter.CurrentDutyRotation?.InterruptAbility(nextGCD, out act) == true)
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
			if (DataCenter.CurrentDutyRotation?.DispelAbility(nextGCD, out act) == true)
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
			if (DataCenter.CurrentDutyRotation?.AntiKnockbackAbility(nextGCD, out act) == true)
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
		if (DataCenter.MergedStatus.HasFlag(AutoStatus.Positional) && !StatusHelper.PlayerHasStatus(true, StatusID.Mudra) && !StatusHelper.PlayerHasStatus(true, StatusID.TrueNorth) && TrueNorthPvE.Cooldown.CurrentCharges > 0 && !IsLastAbility(false, TrueNorthPvE) && TrueNorthPvE.CanUse(out act, skipComboCheck: true, usedUp: true))
		{
			return true;
		}
		IBaseAction.ShouldEndSpecial = false;

		IBaseAction.TargetOverride = TargetType.Heal;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealAreaAbility))
		{
			IBaseAction.AllEmpty = true;
			IBaseAction.ShouldEndSpecial = true;
			if (DataCenter.CurrentDutyRotation?.HealAreaAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (!StatusHelper.PlayerHealingPunished())
			{
				if (HealAreaAbility(nextGCD, out act))
				{
					return true;
				}
			}
			IBaseAction.AllEmpty = false;
			IBaseAction.ShouldEndSpecial = false;
		}

		if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealAreaAbility) && (CanHealAreaAbility || DataCenter.IsInOccultCrescentOp))
		{
			IBaseAction.AutoHealCheck = true;
			if (DataCenter.CurrentDutyRotation?.HealAreaAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (!StatusHelper.PlayerHealingPunished())
			{
				if (HealAreaAbility(nextGCD, out act))
				{
					return true;
				}
			}
			IBaseAction.AutoHealCheck = false;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleAbility))
		{
			IBaseAction.AllEmpty = true;
			IBaseAction.ShouldEndSpecial = true;
			if (DataCenter.CurrentDutyRotation?.HealSingleAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (!StatusHelper.PlayerHealingPunished())
			{
				if (HealSingleAbility(nextGCD, out act))
				{
					return true;
				}
			}
			IBaseAction.AllEmpty = false;
			IBaseAction.ShouldEndSpecial = false;
		}

		if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealSingleAbility) && (CanHealSingleAbility || DataCenter.IsInOccultCrescentOp))
		{
			IBaseAction.AutoHealCheck = true;
			if (DataCenter.CurrentDutyRotation?.HealSingleAbility(nextGCD, out act) == true)
			{
				return true;
			}
			if (!StatusHelper.PlayerHealingPunished())
			{
				if (HealSingleAbility(nextGCD, out act))
				{
					return true;
				}
			}
			IBaseAction.AutoHealCheck = false;
		}

		IBaseAction.TargetOverride = null;

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Speed))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Speed))
		{
			if (DataCenter.CurrentDutyRotation?.SpeedAbility(nextGCD, out act) == true)
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
			if (ProvokePvE.CanUse(out act) || ProvokeAbility(nextGCD, out act))
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
			if (DataCenter.CurrentDutyRotation?.DefenseAreaAbility(nextGCD, out act) == true)
			{
				IBaseAction.SelfProtectionHitsMe = null;
				DefenseTrace.Decision("area defence (duty)", act);
				return true;
			}
			// A damage dealer's single-target defence protects only himself, so under the area flag -
			// which asks whether the party is hit - it runs only when the hit reaches him (A218).
			if (DefenseAreaAbility(nextGCD, out act) || (role is JobRole.Melee or JobRole.RangedPhysical or JobRole.RangedMagical
				&& DataCenter.AreaHitReachesPlayer && DefenseSingleAbility(nextGCD, out act)))
			{
				IBaseAction.SelfProtectionHitsMe = null;
				DefenseTrace.Decision("area defence", act);
				return true;
			}
			IBaseAction.SelfProtectionHitsMe = null;
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
			IBaseAction.HoldDefenceOnSelf = HoldDefenceForInvulnerability();
			if (DataCenter.CurrentDutyRotation?.DefenseSingleAbility(nextGCD, out act) == true)
			{
				IBaseAction.SelfProtectionHitsMe = null;
				IBaseAction.HoldDefenceOnSelf = false;
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
				IBaseAction.HoldDefenceOnSelf = false;
				DefenseTrace.Decision("single defence", act);
				return true;
			}
			IBaseAction.SelfProtectionHitsMe = null;
			IBaseAction.HoldDefenceOnSelf = false;
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
			if (DataCenter.CurrentDutyRotation?.MoveForwardAbility(nextGCD, out act) == true)
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
			if (DataCenter.CurrentDutyRotation?.MoveBackAbility(nextGCD, out act) == true)
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
		if (DataCenter.InCombat && UseHpPotion(nextGCD, out act))
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

		if (HasHostilesInRange && DataCenter.CurrentDutyRotation?.AttackAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (HasHostilesInRange && AttackAbility(nextGCD, out act))
		{
			return true;
		}

		if (DataCenter.CurrentDutyRotation?.GeneralAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (GeneralAbility(nextGCD, out act))
		{
			return true;
		}

		return UseMpPotion(nextGCD, out act) || GeneralUsingAbility(role, nextGCD, out act) || (DataCenter.AutoStatus.HasFlag(AutoStatus.Speed) && SpeedAbility(nextGCD, out act));
	}

	/// <summary>
	/// Determines if an interrupt ability can be used based on the job role.
	/// </summary>
	/// <param name="role">The job role of the player.</param>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if the interrupt ability can be used; otherwise, false.</returns>
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
				// Handle unexpected job roles if necessary
				break;
		}
		return false;
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
	/// Whether this job's <see cref="InterruptAbility"/> override already gates the same shared
	/// role-fallback interrupt action (e.g. LegSweep) with its own combo-safety condition. When true,
	/// <see cref="MyInterruptAbility"/> skips its generic per-role fallback for that action instead of
	/// retrying it ungated after the job's own gate declined it.
	/// </summary>
	protected virtual bool HasOwnInterruptGate => false;

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

	/// <summary>
	/// Determines if an anti-knockback ability can be used based on the job role.
	/// </summary>
	/// <param name="role">The job role of the player.</param>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if an anti-knockback ability can be used; otherwise, false.</returns>
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
				// Handle unexpected job roles if necessary
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
	/// Whether this job's <see cref="AntiKnockbackAbility"/> override already gates the same shared
	/// role-fallback anti-knockback action (e.g. Arm's Length) with its own combo-safety condition.
	/// When true, <see cref="AntiKnockback"/> skips its generic per-role fallback for that action
	/// instead of retrying it ungated after the job's own gate declined it.
	/// </summary>
	protected virtual bool HasOwnAntiKnockbackGate => false;

	/// <summary>
	/// Whether this rotation's <see cref="DefenseSingleAbility"/> already runs
	/// <see cref="ArmsLengthSlowsPull"/> with its own option and holds. When true, the single defence
	/// does not retry Arm's Length as its last resort, which would cast it past a declined option or a
	/// hold such as the Dark Knight's barrier.
	/// </summary>
	protected virtual bool HasOwnArmsLengthPullRule => false;

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

	/// <summary>
	/// Determines if a general ability can be used based on the job role.
	/// </summary>
	/// <param name="role">The job role of the player.</param>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The resulting action.</param>
	/// <returns>True if a general ability can be used; otherwise, false.</returns>
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
				// Handle unexpected job roles if necessary
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
		#region PvP
		if (DataCenter.IsPvP)
		{
			if (DataCenter.Job != Job.BRD && DataCenter.Job != Job.WHM && PurifyPvP.CanUse(out act))
			{
				if (Service.Config.PvpPurifyStun && StatusHelper.PlayerHasStatus(false, StatusID.Stun_1343))
				{
					return true;
				}

				if (Service.Config.PvpPurifyHeavy && StatusHelper.PlayerHasStatus(false, StatusID.Heavy_1344))
				{
					return true;
				}

				if (Service.Config.PvpPurifyBind && StatusHelper.PlayerHasStatus(false, StatusID.Bind_1345))
				{
					return true;
				}

				if (Service.Config.PvpPurifySilence && StatusHelper.PlayerHasStatus(false, StatusID.Silence_1347))
				{
					return true;
				}

				if (Service.Config.PvpPurifyDeepFreeze && StatusHelper.PlayerHasStatus(false, StatusID.DeepFreeze_3219))
				{
					return true;
				}

				if (Service.Config.PvpPurifyMiracleOfNature && StatusHelper.PlayerHasStatus(false, StatusID.MiracleOfNature))
				{
					return true;
				}
			}

			if (GuardPvP.CanUse(out act) && Player?.GetHealthRatio() <= Service.Config.HealthForGuard && !StatusHelper.PlayerHasStatus(true, StatusID.UndeadRedemption) && !StatusHelper.PlayerHasStatus(true, StatusID.InnerRelease_1303))
			{
				return true;
			}

			if (RecuperatePvP.CanUse(out act))
			{
				return true;
			}

			if (StandardissueElixirPvP.CanUse(out act))
			{
				return true;
			}
		}
		#endregion

		if (nextGCD is BaseAction action)
		{
			if (Role is JobRole.RangedMagical && action.Info.CastTime >= 5 && IActionHelper.IsLastActionGCD() && SwiftcastPvE.CanUse(out act))
			{
				if (!nextGCD.IsTheSameTo(false, ActionID.MegaflarePvE, ActionID.ThunderstormPvE, ActionID.JudgmentBoltPvE, ActionID.HellfirePvE, ActionID.EarthenWallPvE))
				{
					return true;
				}
			}
		}

		if (Role is JobRole.RangedMagical && !HasSwift && IActionHelper.IsLastActionGCD() && nextGCD.IsTheSameTo(true, ActionID.OccultCometPvE))
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
		// 447 occurrences in this tree, among them Radiant Aegis on Summoner, which stopped coming
		// out. This condition leaves the GCD path untouched and only adds one weave.
		//
		// The raise is the job's own Raise, not a list of four ids: the list had no Verraise and no
		// Angel Whisper, the same ageing as the hand-kept raise list behind the original defect.
		if (Service.Config.RaisePlayerBySwift && DataCenter.CanRaise() && IActionHelper.IsLastActionGCD()
			&& ((Raise != null && nextGCD.IsTheSameTo(true, Raise))
				|| RaisePendingAndCastable()))
		{
			if (SwiftcastPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (Role is JobRole.Melee && StatusHelper.PlayerHasStatus(false, StatusHelper.DoomHealStatus))
		{
			if (SecondWindPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (Role is JobRole.RangedPhysical && StatusHelper.PlayerHasStatus(false, StatusHelper.DoomHealStatus))
		{
			if (SecondWindPvE.CanUse(out act))
			{
				return true;
			}
		}

		act = null;
		return false;
	}

	/// <summary>
	/// Is a raise waiting, and would it actually go out once the cast time is gone?
	///
	/// Both halves matter. Without the pending check Swiftcast would be spent whenever it happens
	/// to be ready; without the castable check it would be spent on a raise that then fails on MP,
	/// range or level, and the charge is gone for nothing.
	///
	/// The target override is not decoration. Raise actions declare no target type of their own -
	/// they are merely IsFriendly - and their target comes from TargetType.Death, which only the
	/// GCD path sets. Asking CanUse here without it would search the wrong set, and CanUse assigns
	/// Target as a side effect, so a stray call would also leave the action pointing somewhere
	/// else. The previous value is restored rather than cleared, because this runs inside the
	/// ability dispatcher, which sets overrides of its own around its branches.
	/// </summary>
	private bool RaisePendingAndCastable()
	{
		if (Raise == null || !DataCenter.MergedStatus.HasFlag(AutoStatus.Raise) || HasSwift)
		{
			return false;
		}

		var previousOverride = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = TargetType.Death;
		try
		{
			return Raise.CanUse(out _);
		}
		finally
		{
			IBaseAction.TargetOverride = previousOverride;
		}
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

	/// <summary>
	/// The tank's own invulnerability - Hallowed Ground, Holmgang, Living Dead, Superbolide - for the
	/// rules that need to know whether it can still save him. Null for every other job.
	/// </summary>
	protected virtual IBaseAction? Invulnerability => null;

	/// <inheritdoc cref="ICustomRotation.TankInvulnerability"/>
	public IBaseAction? TankInvulnerability => Invulnerability;

	/// <summary>
	/// The owner's proposal of 04.10.2026 (concept 09): whether the single defence holds everything aimed
	/// at the player, because his invulnerability is committed to the coming tankbuster or Hallowed
	/// Ground or Superbolide keeps it off him (<see cref="LethalTankbusterWatch"/>). A command from the
	/// player is not second-guessed.
	/// </summary>
	internal static bool HoldDefenceForInvulnerability()
	{
		return DataCenter.Role == JobRole.Tank && Service.Config.HoldMitigationUnderInvulnerability
			&& !DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle)
			&& (LethalTankbusterWatch.HoldMitigation || LethalTankbusterWatch.PlayerImpervious);
	}

	/// <summary>
	/// The owner's proposal of 04.10.2026 (concept 09, "Unverwundbarkeit vor einem tödlichen Tankbuster"):
	/// the invulnerability goes out for a tankbuster being cast at the player that the learned table rates
	/// lethal even in the best case (<see cref="LethalTankbusterWatch"/>), once the hit is no further away
	/// than the invulnerability's own duration less one GCD - all four last ten seconds (effect texts), so
	/// drawn earlier it would run out first, and the GCD is the time the hit needs to arrive after the
	/// cast ends.
	/// </summary>
	private bool TankInvulnerabilityAbility(out IAction? act)
	{
		act = null;
		if (!LethalTankbusterWatch.InvulnerabilityCommitted || LethalTankbusterWatch.Source == null
			|| LethalTankbusterWatch.PlayerUnderInvulnerability)
		{
			return false;
		}

		var invulnerability = Invulnerability;
		if (invulnerability == null)
		{
			return false;
		}

		var lasts = DefensiveValues.DurationOf(invulnerability.ID);
		if (lasts > 0f && LethalTankbusterWatch.Remaining > lasts - DataCenter.DefaultGCDTotal)
		{
			return false;
		}

		// The action's own target type: a GCD path that returned early can leave its override standing
		// when the abilities run (TODO.md, "Zielüberschreibung").
		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = null;
		try
		{
			if (!invulnerability.CanUse(out act))
			{
				act = null;
				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		DefenseTrace.Decision($"invulnerability before a lethal tankbuster from {LethalTankbusterWatch.Source?.Name.TextValue}"
			+ $" in {LethalTankbusterWatch.Remaining:F1} s", act);
		return true;
	}

	/// <summary>
	/// The owner's proposal of 04.10.2026 (concept 09, "Tankwechsel nach einem Tankbuster"): after a
	/// tankbuster that leaves the player in danger of dying to the next one, and with his
	/// invulnerability not available, Shirk goes to the tank <see cref="TankSwapWatch"/> names - the
	/// one that will then hold the enemy.
	/// </summary>
	/// <remarks>
	/// The invulnerability counts as available only when RSR would use it: enabled, learned, off
	/// cooldown, and with a threshold above zero (HealthForDyingTanks; at zero it never fires). It
	/// fires at that threshold, not ahead of a hit - see concept 09 for the gap that leaves.
	/// </remarks>
	private bool TankSwapAbility(out IAction? act)
	{
		act = null;
		var danger = TankSwapWatch.Danger;
		if (danger.Length == 0)
		{
			return false;
		}

		if (InvulnerabilityUsable(out var invulnerability) && invulnerability!.Cooldown.HasOneCharge)
		{
			TankSwapWatch.TraceHeld($"{invulnerability.Name} is ready");
			return false;
		}

		var receiver = TankSwapWatch.Target;
		if (receiver == null)
		{
			TankSwapWatch.TraceHeld(TankSwapWatch.Detail);
			return false;
		}

		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = TargetType.TankSwap;
		try
		{
			if (!ShirkPvE.CanUse(out act))
			{
				TankSwapWatch.TraceHeld(ShirkPvE.Cooldown.HasOneCharge
					? $"Shirk cannot reach {receiver.Name.TextValue}"
					: "Shirk is on cooldown");
				act = null;
				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		TankSwapWatch.PlanSwapShirk(TankSwapWatch.DangerSource, receiver, TankSwapWatch.Ratio);
		DefenseTrace.Decision($"tank swap ({danger}; {TankSwapWatch.Detail})", act);
		return true;
	}

	/// <summary>
	/// Whether RSR would use the tank's invulnerability at all: it exists for the job, is learned at the
	/// current level, enabled, and its threshold (HealthForDyingTanks) is above zero.
	/// </summary>
	private bool InvulnerabilityUsable(out IBaseAction? invulnerability)
	{
		invulnerability = Invulnerability;
		return invulnerability != null && invulnerability.Config.IsEnabled && invulnerability.EnoughLevel
			&& HealthForDyingTanks > 0f;
	}

	/// <summary>
	/// The other half of the swap (the owner's rule of 04.10.2026): the enemy the swap handed to the
	/// co-tank is provoked back only after the co-tank has taken its next tankbuster (his question,
	/// accepted), once the debuff and the critical state are over - and, his precision, once the
	/// invulnerability is ready again. Where RSR would not use an invulnerability at
	/// all (not learned at this level, disabled, threshold zero) there is nothing to wait for.
	/// </summary>
	private bool TankSwapBackAbility(out IAction? act)
	{
		act = null;
		var source = TankSwapWatch.ReclaimSource;
		if (source == null)
		{
			return false;
		}

		if (InvulnerabilityUsable(out var invulnerability) && !invulnerability!.Cooldown.HasOneCharge)
		{
			TankSwapWatch.TraceReclaimHeld($"{invulnerability.Name} is not ready yet");
			return false;
		}

		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = TargetType.TankSwap;
		try
		{
			if (!ProvokePvE.CanUse(out act))
			{
				TankSwapWatch.TraceReclaimHeld(ProvokePvE.Cooldown.HasOneCharge
					? $"Provoke cannot reach {source.Name.TextValue}"
					: "Provoke is on cooldown");
				act = null;
				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		DefenseTrace.Decision($"tank swap back (the co-tank took the next tankbuster, your debuff and danger are over) on {source.Name.TextValue}", act);
		return true;
	}
}
