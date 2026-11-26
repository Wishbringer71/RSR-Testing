namespace RotationSolver.Rotations.Tank.Warrior;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.58", RotationType = RotationType.Reborn)]
[SourceCode(Path = "main/Rotations/Tank/Warrior/WAR_Reborn.cs")]

public sealed partial class WAR_Reborn : WarriorRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Only use Nascent Flash if Tank Stance is off")]
	public bool NeverscentFlash { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Nascent Flash target priority")]
	public NascentFlashTargetStrategy NascentFlashTarget { get; set; } = NascentFlashTargetStrategy.ByDanger;

	public enum NascentFlashTargetStrategy : byte
	{
		[Description("Lowest HP party member")]
		LowestHP,

		[Description("Healers first, then lowest HP party member")]
		HealerFirst,

		[Description("Healers only")]
		HealerOnly,

		[Description("Most in danger of dying first, then healers, tanks, damage dealers (the heal target order)")]
		ByDanger,
	}

	[RotationConfig(CombatType.PvE, Name = "Keep Bloodwhetting for yourself when you need it",
		Tooltip = "In a fight: Nascent Flash is not given away while you need Bloodwhetting yourself, for a "
			+ "coming tankbuster or when you are about to die.\nA healer about to die still gets it.")]
	public bool HoldNascentFlashForOwnNeed { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Nascent Flash on another party member a tankbuster is aimed at",
		Tooltip = "In a fight: Nascent Flash goes on the member a tankbuster is aimed at, most often the other "
			+ "tank, after your own Bloodwhetting needs.\nOff: it stays for the usual heal rules.")]
	public bool NascentFlashOnTankbusterTarget { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Arm's Length on a pull for its Slow",
		Tooltip = "In a fight: on a pack attacking you, bosses not counted, Arm's Length goes for its Slow, "
			+ "delaying every attacker's swings and casts for fifteen seconds.\nNot when BossModReborn announces "
			+ "a knockback it must be kept for.")]
	public bool UseArmsLengthOnPull { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Bloodwhetting/Raw intuition on single enemies",
		Tooltip = "The single-target defence casts Bloodwhetting (Raw Intuition before level 82) also "
			+ "when fewer than three enemies are in reach - above all for a boss's tankbuster.\n"
			+ "In a fight: 10% less damage for 8 seconds, another 10% for the first 4, and a barrier, "
			+ "for the price of a 25-second cooldown it shares with Nascent Flash. Off: against a lone "
			+ "boss it goes out only after the hit, as a heal below the Bloodwhetting heal threshold.")]
	public bool SoloIntuition { get; set; } = true;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Bloodwhetting/Raw intuition heal threshold")]
	public float HealIntuition { get; set; } = 0.7f;

	[RotationConfig(CombatType.PvE, Name = "Use both stacks of Onslaught during burst while standing still")]
	public bool YEETBurst { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use a stack of Onslaught when its about to overcap while standing still")]
	public bool YEETCooldown { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Inner Release while moving")]
	public bool InnerReleaseMoving { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Primal Rend while moving (Dangerous)")]
	public bool YEET { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Primal Rend while standing still outside of configured melee range (Dangerous)")]
	public bool YEETStill { get; set; } = false;

	[Range(1, 20, ConfigUnitType.Yalms)]
	[RotationConfig(CombatType.PvE, Name = "Max distance you can be from the boss for Primal Rend use (Danger, setting too high will get you killed)")]
	public float PrimalRendDistance2 { get; set; } = 3.5f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Nascent Flash Heal Threshold")]
	public float FlashHeal { get; set; } = 0.6f;

	[RotationConfig(CombatType.PvE, Name = "Use Thrill of Battle before a tankbuster on you",
		Tooltip = "In a fight: Thrill of Battle goes before a tankbuster on you lands, raising your HP and the "
			+ "healing on you by 20%.\nNot under an invulnerability; it is then missing as an emergency heal for "
			+ "90 s.")]
	public bool UseThrillForTankbuster { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Thrill of Battle when your health will fall below its threshold within its duration",
		Tooltip = "In a fight: Thrill of Battle goes once your falling HP would pass its heal threshold within "
			+ "its ten seconds, not only after.\nIt may be spent on a pull that would have stayed above.")]
	public bool UseThrillAheadOfFallingHealth { get; set; } = true;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Thrill Of Battle Heal Threshold")]
	public float ThrillOfBattleHeal { get; set; } = 0.6f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Equilibrium Heal Threshold")]
	public float EquilibriumHeal { get; set; } = 0.6f;

	#endregion

	#region Countdown Logic
	protected override IAction? CountDownAction(float remainTime)
	{
		if (remainTime < 0.54f && TomahawkPvE.CanUse(out var act))
		{
			return act;
		}
		return base.CountDownAction(remainTime);
	}
	#endregion

	#region oGCD Logic
	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		if (InfuriatePvE.CanUse(out act, gcdCountForAbility: 3))
		{
			return true;
		}

		if (!InnerReleasePvE.EnoughLevel && StatusHelper.PlayerHasStatus(true, StatusID.Berserk) && InfuriatePvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		if (CombatElapsedLessGCD(1))
		{
			return false;
		}

		if (!StatusHelper.PlayerWillStatusEndGCD(2, 0, true, StatusID.SurgingTempest)
			|| !StormsEyePvE.EnoughLevel)
		{
			if ((InnerReleaseMoving || !IsMoving) && InnerReleasePvE.CanUse(out act))
			{
				return true;
			}
			if ((InnerReleaseMoving || !IsMoving) && !InnerReleasePvE.Info.EnoughLevelAndQuest() && BerserkPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (IsBurstStatus && (InnerReleaseStacks == 0 || InnerReleaseStacks == 3))
		{
			if (InfuriatePvE.CanUse(out act, usedUp: true))
			{
				return true;
			}
		}

		if (CombatElapsedLessGCD(4))
		{
			return false;
		}

		if (OrogenyPvE.CanUse(out act) && AreaWorthOver(OrogenyPvE, UpheavalPvE))
		{
			return true;
		}

		if (UpheavalPvE.CanUse(out act))
		{
			return true;
		}

		if (StatusHelper.PlayerHasStatus(false, StatusID.Wrathful) && PrimalWrathPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (YEETBurst && OnslaughtPvE.CanUse(out act, usedUp: IsBurstStatus) &&
		   !IsMoving &&
		   !IsLastAction(false, OnslaughtPvE) &&
		   !IsLastAction(false, UpheavalPvE) &&
			StatusHelper.PlayerHasStatus(true, StatusID.SurgingTempest))
		{
			return true;
		}

		if (YEETCooldown && OnslaughtPvE.CanUse(out act, usedUp: true) &&
		   !IsMoving &&
		   !IsLastAction(false, OnslaughtPvE) &&
		   OnslaughtPvE.Cooldown.WillHaveXChargesGCD(OnslaughtMax, 1) &&
			StatusHelper.PlayerHasStatus(true, StatusID.SurgingTempest))
		{
			return true;
		}

		if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act))
		{
			return true;
		}

		return base.AttackAbility(nextGCD, out act);
	}

	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		// A817: alone, on every cooldown only while an enemy attacks him and no healer lives (DataCenter.UnhealedUnderAttack) -
		// "alone with an enemy in range" pressed it at a training dummy that never attacks.
		if ((InCombat && Player?.GetForecastHealthRatio(true) < HealIntuition && NumberOfHostilesInRange > 0) || (InCombat && DataCenter.UnhealedUnderAttack))
		{
			if (BloodwhettingPvE.CanUse(out act))
			{
				return true;
			}
			if (!BloodwhettingPvE.Info.EnoughLevelAndQuest() && RawIntuitionPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (Player?.GetForecastHealthRatio(true) < ThrillOfBattleHeal
			|| (UseThrillAheadOfFallingHealth && InCombat
				&& Player?.GetHealthRatioIn(DefensiveValues.DurationOf((uint)ActionID.ThrillOfBattlePvE)) < ThrillOfBattleHeal))
		{
			if (ThrillOfBattlePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (!StatusHelper.PlayerHasStatus(true, StatusID.Holmgang_409))
		{
			if (Player?.GetForecastHealthRatio(true) < EquilibriumHeal)
			{
				if (EquilibriumPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		if (StatusHelper.PlayerHasStatus(true, StatusID.PrimalRendReady) && InCombat && UseBurstMedicine(out act))
		{
			return true;
		}
		return base.GeneralAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.ShakeItOffPvE, ActionID.NascentFlashPvE)]
	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (!HoldShakeItOffInHeal() && ShakeItOffPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (InCombat && (!NeverscentFlash || !StatusHelper.PlayerHasStatus(true, StatusID.Defiance)))
		{
			if (NascentFlashTarget == NascentFlashTargetStrategy.ByDanger)
			{
				if (NascentFlashCanUse(out act, healersOnly: false, byDanger: true))
				{
					return true;
				}
			}
			else
			{
				if (NascentFlashTarget != NascentFlashTargetStrategy.LowestHP && NascentFlashCanUse(out act, healersOnly: true))
				{
					return true;
				}

				if (NascentFlashTarget != NascentFlashTargetStrategy.HealerOnly && NascentFlashCanUse(out act, healersOnly: false))
				{
					return true;
				}
			}
		}

		return base.HealSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.RawIntuitionPvE, ActionID.VengeancePvE, ActionID.RampartPvE, ActionID.RawIntuitionPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		act = null;

		if (StatusHelper.PlayerHasStatus(true, StatusID.Holmgang_409) && Player?.GetHealthRatio() < 0.3f)
		{
			return false;
		}

		// A predicted tankbuster on the player gets its big mitigation first - or Rampart while that one is
		// spent - ahead of the short ones: the window opens a few seconds before the hit, and with the cheap ones
		// ahead the big one landed last (trace of 01.10.2026: Damnation 0.7 s before the hit, A241, A243).
		if (BMRShouldRefreshBefore(BMRTankbusterIn, DefensiveValues.DurationOf(DamnationPvE.EnoughLevel ? DamnationPvE.ID : VengeancePvE.ID), true, null, DamnationPvE.EnoughLevel ? StatusID.Damnation : StatusID.Vengeance)
			&& (DamnationPvE.EnoughLevel ? DamnationPvE.CanUse(out act, skipStatusProvideCheck: true) : VengeancePvE.CanUse(out act, skipStatusProvideCheck: true)))
		{
			return true;
		}

		if (BMRShouldRefreshBefore(BMRTankbusterIn, DefensiveValues.DurationOf((uint)ActionID.RampartPvE), true, null, StatusID.Rampart)
			&& RampartTakesPredictedTankbuster(DamnationPvE.EnoughLevel ? DamnationPvE : VengeancePvE, DamnationPvE.EnoughLevel ? StatusID.Damnation : StatusID.Vengeance)
			&& RampartPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		// Free of cost but its cooldown, so ahead of the paid mitigations (A194).
		if (ArmsLengthSlowsPull(UseArmsLengthOnPull, Service.Config.AutoDefenseNumber) && ArmsLengthPvE.CanUse(out act))
		{
			return true;
		}

		if (RawIntuitionPvE.CanUse(out act) && BloodwhettingForDefense)
		{
			return true;
		}

		// Thrill of Battle stacks with Bloodwhetting on a tankbuster rather than replacing it (A237).
		if (UseThrillForTankbuster && TankbusterOnMeWithin(DefensiveValues.DurationOf((uint)ActionID.ThrillOfBattlePvE))
			&& ThrillOfBattlePvE.CanUse(out act))
		{
			return true;
		}

		// The big mitigation and Rampart, spaced, or in a pull held until the measured fall needs them (A359).
		if (StaggeredBigMitigation(VengeancePvE, DamnationPvE, out act))
		{
			return true;
		}

		if (!HoldReprisalForRaidwide()
			&& ShouldSustainMitigationDebuff(StatusHelper.ReprisalStatus)
			&& ReprisalPvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!HoldReprisalForRaidwide() && ReprisalPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		// Nascent Flash on another member a tankbuster is aimed at, after his own defence and Bloodwhetting, with
		// which it shares the cooldown (concept 09, "Hilfe für das Ziel eines Tankbusters"). The Balance: "Most often
		// on the co-tank".
		if (NascentFlashOnTankbusterTarget && (!NeverscentFlash || !StatusHelper.PlayerHasStatus(true, StatusID.Defiance))
			&& NascentFlashForTankbuster(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.ShakeItOffPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		// Tanks' stage: Reprisal and the party tool take turns over a run of area attacks (concept 09, rule 9).
		if (TankAreaDefenseStretched(ShakeItOffPvE))
		{
			return base.DefenseAreaAbility(nextGCD, out act);
		}

		if (!HoldShakeItOffForTankbuster() && ShakeItOffPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (ShouldSustainMitigationDebuff(StatusHelper.ReprisalStatus)
			&& ReprisalPvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (ReprisalPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}
	#endregion

	#region GCD Logic
	protected override bool GeneralGCD(out IAction? act)
	{
		if (!StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.SurgingTempest))
		{
			// Each spender's area version only where it deals at least the single one on the enemies it reaches, by their
			// tooltips (concept 02, "Ausgaben in der Fläche erst, wo sie mindestens so viel bringen"; A473): Decimate and
			// Chaotic Cyclone from 4, Orogeny and Steel Cyclone from 3 at patch 7.5, each AoE count the lower bound.
			if (ChaoticCyclonePvE.CanUse(out act) && AreaWorthOver(ChaoticCyclonePvE, InnerChaosPvE))
			{
				return true;
			}

			if (InnerChaosPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (!StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.SurgingTempest) && !StatusHelper.PlayerHasStatus(true, StatusID.NascentChaos) && InnerReleaseStacks > 0)
		{
			if (DecimatePvE.CanUse(out act, skipStatusProvideCheck: true) && AreaWorthOver(DecimatePvE, FellCleavePvE))
			{
				return true;
			}
			if (!DecimatePvE.Info.EnoughLevelAndQuest() && SteelCyclonePvE.CanUse(out act, skipStatusProvideCheck: true)
				&& AreaWorthOver(SteelCyclonePvE, InnerBeastPvE))
			{
				return true;
			}

			if (FellCleavePvE.CanUse(out act, skipStatusProvideCheck: true))
			{
				return true;
			}
			if (!FellCleavePvE.Info.EnoughLevelAndQuest() && InnerBeastPvE.CanUse(out act, skipStatusProvideCheck: true))
			{
				return true;
			}
		}

		if (!StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.SurgingTempest) && InnerReleaseStacks == 0)
		{
			if (PrimalRendPvE.CanUse(out act, skipAoeCheck: true))
			{
				if (GapCloserAllowed(PrimalRendPvE.Target.Target, PrimalRendDistance2))
				{
					return true;
				}
				if (YEET || (YEETStill && !IsMoving))
				{
					return true;
				}
			}
			if (PrimalRuinationPvE.CanUse(out act))
			{
				return true;
			}
		}

		// AOE. Where the pack is too small for the area spender, the single one goes here, ahead of the area combo as the
		// area spender did, so the gauge is spent in a pack of two or three as before (A473).
		if (!StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.SurgingTempest) || !StormsEyePvE.EnoughLevel)
		{
			if (DecimatePvE.CanUse(out act, skipStatusProvideCheck: true))
			{
				if (AreaWorthOver(DecimatePvE, FellCleavePvE) || FellCleavePvE.CanUse(out act, skipStatusProvideCheck: true))
				{
					return true;
				}
			}
			if (!DecimatePvE.Info.EnoughLevelAndQuest() && SteelCyclonePvE.CanUse(out act))
			{
				if (AreaWorthOver(SteelCyclonePvE, InnerBeastPvE) || InnerBeastPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		if (MythrilTempestPvE.CanUse(out act))
		{
			return true;
		}

		if (OverpowerPvE.CanUse(out act))
		{
			return true;
		}

		// Single Target
		if (!StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.SurgingTempest) || !StormsEyePvE.EnoughLevel)
		{
			if (FellCleavePvE.CanUse(out act, skipStatusProvideCheck: true))
			{
				return true;
			}
			if (!FellCleavePvE.Info.EnoughLevelAndQuest() && InnerBeastPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (StormsEyePvE.CanUse(out act))
		{
			return true;
		}

		if (StormsPathPvE.CanUse(out act))
		{
			return true;
		}

		if (MaimPvE.CanUse(out act))
		{
			return true;
		}

		if (HeavySwingPvE.CanUse(out act))
		{
			return true;
		}

		// Ranged
		if (TomahawkPvE.CanUse(out act))
		{
			return true;
		}

		return base.GeneralGCD(out act);
	}
	#endregion

	#region Extra Methods
	private static bool IsBurstStatus => !StatusHelper.PlayerWillStatusEndGCD(0, 0, false, StatusID.InnerStrength);

	// Shake It Off dispels Thrill of Battle, Damnation and Bloodwhetting for 2 % more barrier each (job
	// guide; Vengeance below level 92, WrathCombo and The Balance). Thrill of Battle is not listed here: the
	// barrier is sized on the raised maximum HP, so dispelling it is a gain (The Balance).
	private static readonly StatusID[] ShakeItOffStrips = [StatusID.Damnation, StatusID.Vengeance, StatusID.Bloodwhetting];

	// Picks a party member under the Nascent Flash threshold: by the lowest HP%, optionally healers only,
	// or by the heal target order (concept 07). The conditions go in as this call's target filter, on top of
	// the action's own CanTarget, so nothing on the shared setting changes (A971).
	private bool NascentFlashCanUse(out IAction? act, bool healersOnly, bool byDanger = false)
	{
		var healRatio = Math.Min(FlashHeal, NascentFlashPvE.Config.AutoHealRatio);

		var held = false;
		var warriorAboutToDie = false;
		if (HoldNascentFlashForOwnNeed)
		{
			held = NeedsBloodwhettingHimself(out warriorAboutToDie);
		}

		// IsUnprotectedForHealing is CanBeHealed - true while NO invulnerability is up - with Living Dead not
		// counted while its hold is off (A306): the candidate is unprotected. CanBeHealed read negated here, which let only the invulnerable through - Nascent Flash went to a tank under
		// Holmgang, Superbolide, Hallowed Ground or Living Dead, and to nobody else (A226).
		return NascentFlashPvE.CanUse(out act, targetOverride: byDanger ? TargetType.Heal : TargetType.LowHPPercent,
			targetFilter: t => t.GetForecastHealthRatio(true) < healRatio
				&& t.IsUnprotectedForHealing()
				&& !t.HasStatus(false, StatusHelper.HealingIneffectiveStatus)
				&& (!healersOnly || t.IsJobCategory(JobRole.Healer))
				&& (!held || WinsTriageOverWarrior(t, warriorAboutToDie)));
	}
	#endregion
}