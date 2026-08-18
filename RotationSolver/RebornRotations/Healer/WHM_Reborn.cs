using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.55")]
[SourceCode(Path = "main/RebornRotations/Healer/WHM_Reborn.cs")]

public sealed class WHM_Reborn : WhiteMageRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Use the balance Opener in High-End Duties")]
	public bool UseOpenerHighEnd { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Limit Liturgy Of The Bell to multihit party stacks")]
	public bool MultiHitRestrict { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Tincture/Gemdraught when about to use Presence of Mind")]
	public bool UseMedicine { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Enable Swiftcast Restriction Logic to attempt to prevent actions other than Raise when you have swiftcast")]
	public bool SwiftLogic { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use GCDs to heal. (Ignored if you are the only healer in party)")]
	public bool GCDHeal { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use DOT while moving even if it does not need refresh (disabling is a damage down)")]
	public bool DOTUpkeep { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Lily at max stacks/about to overcap.")]
	public bool UseLilyWhenFull { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Lily if about to overcap and no valid target nearby.")]
	public bool UseLilyDowntime { get; set; } = true;

	[Range(1, 13, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Number of GCDs before you cap on blue lillies that overcap protection will consider 'near full'.")]
	public int LilyOvercapTime { get; set; } = 3;

	[RotationConfig(CombatType.PvE, Name = "Regen on Tank as they close in on enemies (dungeons only, not Trials/Raids), and keep it up while it's otherwise idle GCD time (Regen is instant-cast, safe to keep up while moving).")]
	public bool UsePreRegen { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Divine Caress as soon as its available")]
	public bool UseDivine { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Asylum as soon as a single player heal (i.e. tankbusters) while moving, in addition to normal logic")]
	public bool AsylumSingle { get; set; } = false;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum health threshold party member needs to be to use Benediction")]
	public float BenedictionHeal { get; set; } = 0.3f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "If a party member's health drops below this percentage, the Regen healing ability will not be used on them")]
	public float RegenHeal { get; set; } = 0.3f;

	[Range(0, 10000, ConfigUnitType.None, 100)]
	[RotationConfig(CombatType.PvE, Name = "Casting cost requirement for Thin Air to be used")]

	public float ThinAirNeed { get; set; } = 1000;

	[RotationConfig(CombatType.PvE, Name = "How to manage the last thin air charge")]
	public ThinAirUsageStrategy ThinAirLastChargeUsage { get; set; } = ThinAirUsageStrategy.ReserveLastChargeForRaise;

	public enum ThinAirUsageStrategy : byte
	{
		[Description("Use all thin air charges on expensive spells")]
		UseAllCharges,

		[Description("Reserve the last charge for raise")]
		ReserveLastChargeForRaise,

		[Description("Reserve the last charge for manual use")]
		ReserveLastCharge,
	}
	#endregion

	#region Countdown Logic
	protected override IAction? CountDownAction(float remainTime)
	{
		if (remainTime < StonePvE.Info.CastTime + CountDownAhead
			&& StonePvE.CanUse(out var act))
		{
			return act;
		}

		if (remainTime < 3 && UseBurstMedicine(out act))
		{
			return act;
		}

		// Countdown-timed pre-pull cast removed: dungeons (the actual wall-to-wall-pull use case)
		// never have an active countdown, so this never fired there - and where it DID fire
		// (trials/raids with a real countdown), it's explicitly out of scope for this mechanic.
		// See the TankApproachingMobGroup-gated GeneralGCD filler below instead.
		return base.CountDownAction(remainTime);
	}
	#endregion

	#region oGCD Logic
	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		var useLastThinAirCharge = ThinAirLastChargeUsage == ThinAirUsageStrategy.UseAllCharges || (ThinAirLastChargeUsage == ThinAirUsageStrategy.ReserveLastChargeForRaise && nextGCD == RaisePvE);
		if (((nextGCD is IBaseAction action && action.Info.MPNeed >= ThinAirNeed && IsLastAction() == IsLastGCD()) || ((MergedStatus.HasFlag(AutoStatus.Raise) || (nextGCD == RaisePvE)) && IsLastAction() == IsLastGCD())) &&
			ThinAirPvE.CanUse(out act, usedUp: useLastThinAirCharge))
		{
			return true;
		}

		if (StatusHelper.PlayerWillStatusEndGCD(2, 0, true, StatusID.DivineGrace) && DivineCaressPvE.CanUse(out act))
		{
			return true;
		}

		if (UseMedicine && !PresenceOfMindPvE.Cooldown.IsCoolingDown && UseBurstMedicine(out act))
		{
			return true;
		}

		if (nextGCD.IsTheSameTo(true, AfflatusRapturePvE, MedicaPvE, MedicaIiPvE, CureIiiPvE)
			&& (MergedStatus.HasFlag(AutoStatus.HealAreaSpell) || MergedStatus.HasFlag(AutoStatus.HealSingleSpell)))
		{
			if (PlenaryIndulgencePvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.EmergencyAbility(nextGCD, out act);
	}

	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		if (UseDivine && DivineCaressPvE.CanUse(out act))
		{
			return true;
		}
		return base.GeneralAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.TemperancePvE, ActionID.LiturgyOfTheBellPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		if ((TemperancePvE.Cooldown.IsCoolingDown && !TemperancePvE.Cooldown.WillHaveOneCharge(100))
			|| (LiturgyOfTheBellPvE.Cooldown.IsCoolingDown && !LiturgyOfTheBellPvE.Cooldown.WillHaveOneCharge(160)))
		{
			return base.DefenseAreaAbility(nextGCD, out act);
		}

		if (MultiHitRestrict && IsCastingMultiHit)
		{
			if (LiturgyOfTheBellPvE.CanUse(out act, skipAoeCheck: true))
			{
				return true;
			}
		}

		if (PlenaryIndulgencePvE.CanUse(out act))
		{
			return true;
		}

		if (TemperancePvE.CanUse(out act))
		{
			return true;
		}

		if (DivineCaressPvE.CanUse(out act))
		{
			return true;
		}

		if ((MultiHitRestrict && IsCastingMultiHit) || !MultiHitRestrict)
		{
			if (LiturgyOfTheBellPvE.CanUse(out act, skipAoeCheck: true))
			{
				return true;
			}
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.DivineBenisonPvE, ActionID.AquaveilPvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if ((DivineBenisonPvE.Cooldown.IsCoolingDown && !DivineBenisonPvE.Cooldown.WillHaveOneCharge(15))
			|| (AquaveilPvE.Cooldown.IsCoolingDown && !AquaveilPvE.Cooldown.WillHaveOneCharge(52)))
		{
			return base.DefenseSingleAbility(nextGCD, out act);
		}

		if (DivineBenisonPvE.CanUse(out act))
		{
			return true;
		}

		if (AquaveilPvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.AsylumPvE)]
	protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (AsylumPvE.CanUse(out act))
		{
			return true;
		}
		return base.HealAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.BenedictionPvE, ActionID.AsylumPvE, ActionID.DivineBenisonPvE, ActionID.TetragrammatonPvE)]
	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (BenedictionPvE.CanUse(out act) &&
			BenedictionPvE.Target.Target.GetHealthRatio() < BenedictionHeal)
		{
			return true;
		}

		if (IsLastAction(ActionID.BenedictionPvE))
		{
			return base.HealSingleAbility(nextGCD, out act);
		}

		if (AsylumSingle && !IsMoving && AsylumPvE.CanUse(out act))
		{
			return true;
		}

		if (DivineBenisonPvE.CanUse(out act))
		{
			return true;
		}

		if (TetragrammatonPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		return base.HealSingleAbility(nextGCD, out act);
	}

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		if (InCombat)
		{
			if (!IsInHighEndDuty || !UseOpenerHighEnd || (IsInHighEndDuty && UseOpenerHighEnd && !CombatElapsedLessGCD(3)))
			{
				if (PresenceOfMindPvE.CanUse(out act, skipTTKCheck: IsInHighEndDuty))
				{
					return true;
				}
			}


			if (!IsInHighEndDuty || !UseOpenerHighEnd || (IsInHighEndDuty && UseOpenerHighEnd && !CombatElapsedLessGCD(4)))
			{
				if (AssizePvE.CanUse(out act, skipAoeCheck: true))
				{
					return true;
				}
			}
		}

		return base.AttackAbility(nextGCD, out act);
	}
	#endregion

	#region GCD Logic
	[RotationDesc(ActionID.AfflatusRapturePvE, ActionID.MedicaIiPvE, ActionID.CureIiiPvE, ActionID.MedicaPvE)]
	protected override bool HealAreaGCD(out IAction? act)
	{
		if ((HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise))
		{
			return base.HealAreaGCD(out act);
		}

		if (AfflatusRapturePvE.CanUse(out act))
		{
			return true;
		}

		var hasMedica2 = 0;
		foreach (var n in PartyMembers)
		{
			if (n.HasStatus(true, StatusID.MedicaIi))
			{
				hasMedica2++;
			}
		}

		var partyCount = 0;
		foreach (var _ in PartyMembers)
		{
			partyCount++;
		}
		if (MedicaIiPvE.EnoughLevel)
		{
			if (MedicaIiiPvE.EnoughLevel && MedicaIiiPvE.CanUse(out act) && hasMedica2 < partyCount / 2 && !IsLastAction(true, MedicaIiPvE))
			{
				return true;
			}

			if (!MedicaIiiPvE.EnoughLevel && MedicaIiPvE.CanUse(out act) && hasMedica2 < partyCount / 2 && !IsLastAction(true, MedicaIiPvE))
			{
				return true;
			}
		}

		if (CureIiiPvE.CanUse(out act))
		{
			return true;
		}

		if (MedicaPvE.CanUse(out act))
		{
			return true;
		}

		// Proactive wall-to-wall sustain: HealAreaGCD is checked before GeneralGCD in the outer
		// dispatch (same as HealSingleGCD below), so a raised AoE heal-need flag can starve
		// TankApproachingMobGroup's check in GeneralGCD for the whole pull even on GCDs where
		// nothing above in this method actually resolves to a cast. Placed last, right before the
		// base call, so it never displaces any of the reactive AoE heals above it - same pattern
		// and RegenHeal safety threshold as the HealSingleGCD fix.
		if (UsePreRegen && TankApproachingMobGroup && RegenPvE.CanUse(out act, targetOverride: TargetType.Tank)
			&& RegenPvE.Target.Target != null && RegenPvE.Target.Target.GetHealthRatio() > RegenHeal
			&& (RegenPvE.Target.Target.WillStatusEndGCD(RegenPvE.Config.StatusRefreshGcdCount, 0, RegenPvE.Setting.StatusFromSelf, RegenPvE.Setting.TargetStatusProvide ?? [])))
		{
			return true;
		}

		return base.HealAreaGCD(out act);
	}

	[RotationDesc(ActionID.AfflatusSolacePvE, ActionID.RegenPvE, ActionID.CureIiPvE, ActionID.CurePvE)]
	protected override bool HealSingleGCD(out IAction? act)
	{
		if ((HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise))
		{
			return base.HealSingleGCD(out act);
		}

		if (AfflatusSolacePvE.CanUse(out act))
		{
			return true;
		}

		if (RegenPvE.CanUse(out act) && (RegenPvE.Target.Target.GetHealthRatio() > RegenHeal))
		{
			return true;
		}

		// Proactive wall-to-wall sustain: when the tank takes continuous, concentrated damage, this
		// method (not GeneralGCD) claims almost every GCD via CureII/Cure below, so
		// TankApproachingMobGroup's check in GeneralGCD never gets reached and Regen is never
		// refreshed for the rest of the pull despite being due. Reuses the same RegenHeal safety
		// threshold as the reactive branch above it, so this never fires below the HP level the
		// player already trusts Regen at instead of a direct heal - a genuine emergency below that
		// threshold still falls through to CureII/Cure untouched.
		if (UsePreRegen && TankApproachingMobGroup && RegenPvE.CanUse(out act, targetOverride: TargetType.Tank)
			&& RegenPvE.Target.Target != null && RegenPvE.Target.Target.GetHealthRatio() > RegenHeal
			&& (RegenPvE.Target.Target.WillStatusEndGCD(RegenPvE.Config.StatusRefreshGcdCount, 0, RegenPvE.Setting.StatusFromSelf, RegenPvE.Setting.TargetStatusProvide ?? [])))
		{
			return true;
		}

		if (CureIiPvE.CanUse(out act))
		{
			return true;
		}

		if (CurePvE.CanUse(out act))
		{
			return true;
		}

		return base.HealSingleGCD(out act);
	}

	[RotationDesc(ActionID.RaisePvE)]
	protected override bool RaiseGCD(out IAction? act)
	{
		if (RaisePvE.CanUse(out act))
		{
			return true;
		}

		return base.RaiseGCD(out act);
	}

	protected override bool GeneralGCD(out IAction? act)
	{
		if (HasThinAir && MergedStatus.HasFlag(AutoStatus.Raise))
		{
			return RaiseGCD(out act);
		}

		if ((HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise))
		{
			return base.GeneralGCD(out act);
		}

		// Regen is instant-cast (no cast time), so keeping it up on the tank costs no movement/uptime
		// unlike a hard-cast spell. Checked early (ahead of DoT upkeep/nukes/Lily burst below) rather
		// than as bottom-of-list filler: placed last, it never got a turn once real combat DPS/DoT
		// priorities existed for a fight against 4+ mobs, so it only ever fired for the very first
		// pull and was never refreshed for any pull after that - exactly the "not maintained" bug
		// report. Gated on TankApproachingMobGroup so it still only fires as the tank actually commits
		// to a pull, not while standing still at the start of the instance or between pulls.
		// targetOverride bypasses the normal candidate-list status check (FindTankTarget doesn't call
		// CheckStatus), so without this explicit check it would recast Regen on the tank every free GCD
		// regardless of remaining duration - check it here instead.
		if (UsePreRegen && TankApproachingMobGroup && RegenPvE.CanUse(out act, targetOverride: TargetType.Tank)
			&& (RegenPvE.Target.Target?.WillStatusEndGCD(RegenPvE.Config.StatusRefreshGcdCount, 0, RegenPvE.Setting.StatusFromSelf, RegenPvE.Setting.TargetStatusProvide ?? []) ?? true))
		{
			return true;
		}

		//if (NotInCombatDelay && RegenDefense.CanUse(out act)) return true;

		var liliesNearlyFull = Lily == 2 && LilyAfterGCD((uint)LilyOvercapTime);
		var liliesFullNoBlood = Lily == 3;

		if (!IsInHighEndDuty || !UseOpenerHighEnd || (IsInHighEndDuty && UseOpenerHighEnd && (HasBuffs || HasPresenceOfMind || ((liliesNearlyFull || liliesFullNoBlood) && !CombatElapsedLessGCD(3)))))
		{
			if (AfflatusMiseryPvE.CanUse(out act, skipAoeCheck: true))
			{
				return true;
			}
		}

		if (AfflatusMiseryPvE.EnoughLevel && UseLilyWhenFull && (!IsInHighEndDuty || !UseOpenerHighEnd || (IsInHighEndDuty && UseOpenerHighEnd && !CombatElapsedLessGCD(13))) && (liliesNearlyFull || liliesFullNoBlood) && AfflatusMiseryPvE.EnoughLevel && BloodLily < 3)
		{
			if (AfflatusRapturePvE.CanUse(out act, skipAoeCheck: true))
			{
				return true;
			}

			if (AfflatusSolacePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (GlareIvPvE.CanUse(out act))
		{
			return true;
		}

		if (StatusHelper.PlayerHasStatus(true, StatusID.Confession) && StatusHelper.PlayerWillStatusEndGCD(1, 0, true, StatusID.Confession))
		{
			if (AfflatusRapturePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (HolyPvE.EnoughLevel)
		{
			if (HolyIiiPvE.EnoughLevel && HolyIiiPvE.CanUse(out act))
			{
				return true;
			}
			if (HolyPvE.EnoughLevel && !HolyIiiPvE.EnoughLevel && HolyPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (AeroPvE.EnoughLevel)
		{
			if (DiaPvE.EnoughLevel && DiaPvE.CanUse(out act))
			{
				return true;
			}
			if (AeroIiPvE.EnoughLevel && !DiaPvE.EnoughLevel && AeroIiPvE.CanUse(out act))
			{
				return true;
			}
			if (AeroPvE.EnoughLevel && !AeroIiPvE.EnoughLevel && AeroPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (GlareIiiPvE.EnoughLevel && GlareIiiPvE.CanUse(out act))
		{
			return true;
		}
		if (GlarePvE.EnoughLevel && !GlareIiiPvE.EnoughLevel && GlarePvE.CanUse(out act))
		{
			return true;
		}
		if (StoneIvPvE.EnoughLevel && !GlarePvE.EnoughLevel && StoneIvPvE.CanUse(out act))
		{
			return true;
		}
		if (StoneIiiPvE.EnoughLevel && !StoneIvPvE.EnoughLevel && StoneIiiPvE.CanUse(out act))
		{
			return true;
		}
		if (StoneIiPvE.EnoughLevel && !StoneIiiPvE.Info.EnoughLevelAndQuest() && StoneIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!StoneIiPvE.EnoughLevel && StonePvE.CanUse(out act))
		{
			return true;
		}

		if (AfflatusMiseryPvE.EnoughLevel && UseLilyDowntime && (liliesNearlyFull || liliesFullNoBlood))
		{
			if (AfflatusRapturePvE.CanUse(out act, skipAoeCheck: true))
			{
				return true;
			}

			if (AfflatusSolacePvE.CanUse(out act))
			{
				return true;
			}
		}

		// A genuine refresh need was already handled by the unconditional Dia/Aero check earlier in
		// this method, so everything below only fires when the DoT is still fresh - this is always
		// optional filler damage, not upkeep. That's a real DPS gain while moving (what DOTUpkeep is
		// for), but recasting it specifically on a target that's already attacking the healer adds
		// avoidable enmity for zero upkeep benefit, risking a squishy healer pulling aggro during a
		// wall-to-wall pull. If the default target is unsafe, redirect to another one (TargetType.
		// SafeDotTarget) instead of just skipping the filler cast outright - only while DOTUpkeep is
		// actually active, matching why this branch exists in the first place.
		if (AeroPvE.EnoughLevel)
		{
			if (DiaPvE.EnoughLevel)
			{
				if (DiaPvE.Target.Target?.TargetObject != Player && DiaPvE.CanUse(out act, skipStatusProvideCheck: DOTUpkeep))
				{
					return true;
				}
				if (DOTUpkeep && DiaPvE.CanUse(out act, targetOverride: TargetType.SafeDotTarget, skipStatusProvideCheck: true))
				{
					return true;
				}
			}
			if (AeroIiPvE.EnoughLevel && !DiaPvE.EnoughLevel)
			{
				if (AeroIiPvE.Target.Target?.TargetObject != Player && AeroIiPvE.CanUse(out act, skipStatusProvideCheck: DOTUpkeep))
				{
					return true;
				}
				if (DOTUpkeep && AeroIiPvE.CanUse(out act, targetOverride: TargetType.SafeDotTarget, skipStatusProvideCheck: true))
				{
					return true;
				}
			}
			if (AeroPvE.EnoughLevel && !AeroIiPvE.EnoughLevel)
			{
				if (AeroPvE.Target.Target?.TargetObject != Player && AeroPvE.CanUse(out act, skipStatusProvideCheck: DOTUpkeep))
				{
					return true;
				}
				if (DOTUpkeep && AeroPvE.CanUse(out act, targetOverride: TargetType.SafeDotTarget, skipStatusProvideCheck: true))
				{
					return true;
				}
			}
		}

		return base.GeneralGCD(out act);
	}
	#endregion

	#region Extra Methods
	public override bool CanHealSingleSpell
	{
		get
		{
			var aliveHealerCount = 0;
			var healers = PartyMembers.GetJobCategory(JobRole.Healer);
			foreach (var h in healers)
			{
				if (!h.IsDead)
				{
					aliveHealerCount++;
				}
			}

			return base.CanHealSingleSpell && (GCDHeal || aliveHealerCount == 1);
		}
	}
	public override bool CanHealAreaSpell
	{
		get
		{
			var aliveHealerCount = 0;
			var healers = PartyMembers.GetJobCategory(JobRole.Healer);
			foreach (var h in healers)
			{
				if (!h.IsDead)
				{
					aliveHealerCount++;
				}
			}

			return base.CanHealAreaSpell && (GCDHeal || aliveHealerCount == 1);
		}
	}
	#endregion
}