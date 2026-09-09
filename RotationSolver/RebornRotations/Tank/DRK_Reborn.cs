using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.55")]
[SourceCode(Path = "main/RebornRotations/Tank/DRK_Reborn.cs")]

public sealed class DRK_Reborn : DarkKnightRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Use provoke in opening if tank stance is on")]
	public bool UseProvokeInOpening { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Keep at least 3000 MP")]
	public bool TheBlackestNight { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use The Blackest Night on lowest HP party member during AOE scenarios")]
	public bool BlackLantern { get; set; } = false;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Target health threshold needed to use Blackest Night with above option", Parent = nameof(BlackLantern))]
	private float BlackLanternRatio { get; set; } = 0.5f;

	[RotationConfig(CombatType.PvE, Name = "Use Oblation on lowest HP party member during AOE scenarios")]
	public bool OblationLantern { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Oblation last stack of Oblation for party members", Parent = nameof(OblationLantern))]
	public bool OblationLanternStack { get; set; } = false;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Target health threshold needed to use Oblation with above option", Parent = nameof(OblationLantern))]
	private float OblationLanternRatio { get; set; } = 0.5f;

	[RotationConfig(CombatType.PvE, Name = "When to use The Blackest Night on yourself")]
	public BlackestNightStrategy BlackestNightUsage { get; set; } = BlackestNightStrategy.WheneverDefensesOpen;

	public enum BlackestNightStrategy : byte
	{
		[Description("Whenever single-target defences open")]
		WheneverDefensesOpen,

		[Description("Tankbuster, or a big pull with no other mitigation running")]
		TankbusterOrHeavyPull,

		[Description("As above, plus below the health threshold")]
		TankbusterHeavyPullOrLowHealth,
	}

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold for the Blackest Night option above")]
	private float BlackestNightHealthRatio { get; set; } = 0.6f;

	[RotationConfig(CombatType.PvE, Name = "Opener action")]
	public OpenerActionStrategy OpenerActionUsage { get; set; } = OpenerActionStrategy.Unmend;

	public enum OpenerActionStrategy : byte
	{
		[Description("Use Unmend")]
		Unmend,

		[Description("Use Shadowstride")]
		Shadowstride,
	}
	#endregion

	#region Countdown Logic
	// Countdown logic to prepare for combat.
	// Includes logic for using Provoke, tank stances, and burst medicines.
	protected override IAction? CountDownAction(float remainTime)
	{
		//Provoke when has Shield.
		if (UseProvokeInOpening && remainTime <= CountDownAhead)
		{
			if (HasTankStance)
			{
				if (ProvokePvE.CanUse(out _))
				{
					return ProvokePvE;
				}
			}
		}

		if (remainTime < 1f && UseBurstMedicine(out var act))
		{
			return act;
		}

		if (remainTime <= 3f && TheBlackestNightPvE.CanUse(out act))
		{
			return act;
		}

		if (OpenerActionUsage == OpenerActionStrategy.Unmend)
		{
			if (remainTime <= 1f && UnmendPvE.CanUse(out act))
			{
				return act;
			}
		}

		if (OpenerActionUsage == OpenerActionStrategy.Shadowstride)
		{
			if (remainTime <= 1f && ShadowstridePvE.CanUse(out act))
			{
				return act;
			}
		}

		return base.CountDownAction(remainTime);
	}
	#endregion

	#region oGCD Logic
	// Decision-making for emergency abilities, focusing on Blood Weapon usage.
	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		if (CombatElapsedLessGCD(3) && (IsLastAction(false, UnmendPvE) || IsLastAction(false, HardSlashPvE)))
		{
			if (EdgeOfShadowPvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.EmergencyAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.ShadowstridePvE)]
	protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
	{
		if (ShadowstridePvE.CanUse(out act))
		{
			return true;
		}
		return base.MoveForwardAbility(nextGCD, out act);
	}

	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{

		return base.HealSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.DarkMissionaryPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		// BlackLantern was declared and named as the threshold's parent but never read, so this
		// branch ran regardless of the switch - and the UI hides the threshold while the switch is
		// off, leaving the only adjustment invisible. The Oblation line below checks its own option,
		// and ChurinDRK checks this one on the same branch.
		if (BlackLantern && !InTwoMIsBurst && TheBlackestNightPvE.CanUse(out act, targetOverride: TargetType.LowHP) && !TheBlackestNightPvE.Target.Target.HasStatus(false, StatusID.Transcendent) && TheBlackestNightPvE.Target.Target.GetHealthRatio() <= BlackLanternRatio)
		{
			return true;
		}

		if (!IsLastAbility(false, OblationPvE))
		{
			if (!InTwoMIsBurst && OblationLantern && OblationPvE.CanUse(out act, usedUp: OblationLanternStack, targetOverride: TargetType.LowHP) && !OblationPvE.Target.Target.HasStatus(false, StatusID.Transcendent) && OblationPvE.Target.Target.GetHealthRatio() <= OblationLanternRatio)
			{
				return true;
			}
		}

		if (!InTwoMIsBurst && DarkMissionaryPvE.CanUse(out act))
		{
			return true;
		}

		if (!InTwoMIsBurst
			&& ShouldSustainMitigationDebuff(StatusHelper.ReprisalStatus)
			&& ReprisalPvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!InTwoMIsBurst && ReprisalPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (!IsLastAbility(false, OblationPvE))
		{
			if (!InTwoMIsBurst && OblationPvE.CanUse(out act, skipStatusProvideCheck: false, targetOverride: TargetType.Self))
			{
				return true;
			}
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	/// <summary>
	/// Whether the self-cast of The Blackest Night is wanted in the current situation. See
	/// docs/rotation-flow/10-drk-blackest-night.md: the barrier repays its 3000 MP as Dark Arts only
	/// when it is absorbed in full, which takes 25% of maximum HP in 7 seconds - roughly 3.6% per
	/// second, and more than that once another mitigation has cut the incoming damage.
	/// <para>
	/// Two situations reach that rate, and they want opposite handling. Against a tankbuster the
	/// hit clears the threshold even under Shadow Wall, so mitigating alongside costs nothing and
	/// stacking is right. In a wall-to-wall pull the damage is a stream, and stacking wastes
	/// coverage that should be staggered - so there the barrier goes up only while no big
	/// mitigation is running, the same stagger Shadow Wall and Shadowed Vigil already keep against
	/// each other through StatusProvide.
	/// </para>
	/// </summary>
	private bool ShouldUseBlackestNightOnSelf()
	{
		var staggeredHeavyPull = InHeavyPull && !HasMajorMitigation;

		return BlackestNightUsage switch
		{
			BlackestNightStrategy.TankbusterOrHeavyPull => TankbusterOnMe || staggeredHeavyPull,
			// Low health is an emergency, so it deliberately skips the stagger condition.
			BlackestNightStrategy.TankbusterHeavyPullOrLowHealth =>
				TankbusterOnMe || staggeredHeavyPull
				|| Player?.GetHealthRatio() <= BlackestNightHealthRatio,
			_ => true,
		};
	}

	[RotationDesc(ActionID.OblationPvE, ActionID.TheBlackestNightPvE, ActionID.DarkMindPvE, ActionID.ShadowWallPvE, ActionID.ShadowedVigilPvE, ActionID.RampartPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		//10
		if (!IsLastAbility(false, OblationPvE))
		{
			if (OblationPvE.CanUse(out act, usedUp: true, skipStatusProvideCheck: false, targetOverride: TargetType.Self))
			{
				return true;
			}
		}

		// The trigger that opens this path says "defending is warranted", not "damage worth 25% of
		// maximum HP is coming" - it fires on two enemies in melee range, or on any uninterruptible
		// cast aimed at its own target. Rampart and Reprisal cost nothing but their cooldown; this
		// costs 3000 MP and only repays it as Dark Arts when the barrier is absorbed in full.
		if (ShouldUseBlackestNightOnSelf() && TheBlackestNightPvE.CanUse(out act, targetOverride: TargetType.Self))
		{
			return true;
		}
		//20
		if (DarkMindPvE.CanUse(out act))
		{
			return true;
		}

		// Predicted tankbuster takes priority over the elapsed-time stagger below.
		if (BMRShouldRefreshBefore(BMRTankbusterIn, 15f, true, null, ShadowedVigilPvE.EnoughLevel ? StatusID.ShadowedVigil : StatusID.ShadowWall)
			&& (ShadowedVigilPvE.EnoughLevel ? ShadowedVigilPvE.CanUse(out act, skipStatusProvideCheck: true) : ShadowWallPvE.CanUse(out act, skipStatusProvideCheck: true)))
		{
			return true;
		}

		if (BMRShouldRefreshBefore(BMRTankbusterIn, 20f, true, null, StatusID.Rampart) && RampartPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		//30-40
		if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && ShadowedVigilPvE.CanUse(out act) && ShadowedVigilPvE.EnoughLevel)
		{
			return true;
		}

		if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && ShadowWallPvE.CanUse(out act) && !ShadowedVigilPvE.EnoughLevel)
		{
			return true;
		}

		//20
		if (!ShadowWallPvE.EnoughLevel)
		{
			if (RampartPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (ShadowWallPvE.EnoughLevel && !ShadowedVigilPvE.EnoughLevel)
		{
			if (ShadowWallPvE.Cooldown.IsCoolingDown && ShadowWallPvE.Cooldown.ElapsedAfter(30) && RampartPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (ShadowedVigilPvE.EnoughLevel)
		{
			if (ShadowedVigilPvE.Cooldown.IsCoolingDown && ShadowedVigilPvE.Cooldown.ElapsedAfter(30) && RampartPvE.CanUse(out act))
			{
				return true;
			}
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

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		if (IsBurst)
		{
			if (InCombat && (IsLastGCD(false, SouleaterPvE) || !CombatElapsedLessGCD(4)))
			{
				if (DeliriumPvE.CanUse(out act))
				{
					return true;
				}
			}

			if (!DeliriumPvE.EnoughLevel)
			{
				if (BloodWeaponPvE.CanUse(out act))
				{
					return true;
				}
			}
			if (InCombat && (IsLastGCD(false, HardSlashPvE) || !CombatElapsedLessGCD(3)))
			{
				if (LivingShadowPvE.CanUse(out act, skipAoeCheck: true))
				{
					return true;
				}
			}
		}

		if (CombatElapsedLessGCD(4))
		{
			return base.AttackAbility(nextGCD, out act);
		}

		if (CheckDarkSide)
		{
			if (FloodOfDarknessPvE.CanUse(out act))
			{
				return true;
			}

			if (EdgeOfDarknessPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (!IsMoving && SaltedEarthPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (ShadowbringerPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (NumberOfHostilesInRange >= 3 && AbyssalDrainPvE.CanUse(out act))
		{
			return true;
		}

		if (CarveAndSpitPvE.CanUse(out act))
		{
			return true;
		}

		if (InTwoMIsBurst)
		{
			if (ShadowbringerPvE.CanUse(out act, usedUp: true, skipAoeCheck: true))
			{
				return true;
			}
		}

		if (SaltAndDarknessPvE.CanUse(out act))
		{
			return true;
		}

		return base.AttackAbility(nextGCD, out act);
	}
	#endregion

	#region GCD Logic
	protected override bool GeneralGCD(out IAction? act)
	{
		if (CombatElapsedLessGCD(4) && !IsLastGCD(false, SouleaterPvE) && NumberOfHostilesInRange < 2)
		{
			if (!HasDelirium && SouleaterPvE.CanUse(out act))
			{
				return true;
			}

			if (!HasDelirium && SyphonStrikePvE.CanUse(out act))
			{
				return true;
			}

			if (!HasDelirium && HardSlashPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (DisesteemPvE.CanUse(out act, skipComboCheck: true, skipAoeCheck: true))
		{
			return true;
		}

		//AOE Delirium
		if (ImpalementPvE.CanUse(out act))
		{
			return true;
		}

		if (QuietusPvE.CanUse(out act))
		{
			return true;
		}

		// Single Target Delirium
		if (TorcleaverPvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		if (ComeuppancePvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		if (ScarletDeliriumPvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		if (BloodspillerPvE.CanUse(out act, skipComboCheck: true))
		{
			return true;
		}

		//AOE
		if (StalwartSoulPvE.CanUse(out act, skipAoeCheck: true) && NumberOfHostilesInRange > 0)
		{
			return true;
		}

		if (UnleashPvE.CanUse(out act))
		{
			return true;
		}

		//Single Target
		if (!HasDelirium && SouleaterPvE.CanUse(out act))
		{
			return true;
		}

		if (!HasDelirium && SyphonStrikePvE.CanUse(out act))
		{
			return true;
		}

		if (!HasDelirium && HardSlashPvE.CanUse(out act))
		{
			return true;
		}

		if (UnmendPvE.CanUse(out act))
		{
			return true;
		}

		return base.GeneralGCD(out act);
	}
	#endregion

	#region Extra Methods
	// Indicates whether the Dark Knight can heal using a single ability.
	public override bool CanHealSingleAbility => false;

	// Logic to determine when to use blood-based abilities.
	private bool UseBlood
	{
		get
		{
			// Conditions based on player statuses and ability cooldowns.
			if (!DeliriumPvE.EnoughLevel || !LivingShadowPvE.EnoughLevel)
			{
				return true;
			}

			if (StatusHelper.PlayerHasStatus(true, StatusID.Delirium_3836))
			{
				return true;
			}

			if ((StatusHelper.PlayerHasStatus(true, StatusID.Delirium_1972) || StatusHelper.PlayerHasStatus(true, StatusID.Delirium_3836)) && LivingShadowPvE.Cooldown.IsCoolingDown)
			{
				return true;
			}

			return (DeliriumPvE.Cooldown.WillHaveOneChargeGCD(1) && !LivingShadowPvE.Cooldown.WillHaveOneChargeGCD(3)) || (Blood >= 90 && !LivingShadowPvE.Cooldown.WillHaveOneChargeGCD(1));
		}
	}
	// Determines if currently in a burst phase based on cooldowns of key abilities.
	private bool InTwoMIsBurst => BloodWeaponPvE.Cooldown.IsCoolingDown && DeliriumPvE.Cooldown.IsCoolingDown && ((LivingShadowPvE.Cooldown.IsCoolingDown && !LivingShadowPvE.Cooldown.ElapsedAfter(15)) || !LivingShadowPvE.EnoughLevel);

	// Manages DarkSide ability based on several conditions.
	private bool CheckDarkSide
	{
		get
		{
			if (DarkSideEndAfterGCD(3))
			{
				return true;
			}

			if ((InTwoMIsBurst && HasDarkArts) || (HasDarkArts && StatusHelper.PlayerHasStatus(true, StatusID.BlackestNight)) || (HasDarkArts && DarkSideEndAfterGCD(3)))
			{
				return true;
			}

			if (InTwoMIsBurst && BloodWeaponPvE.Cooldown.IsCoolingDown && LivingShadowPvE.Cooldown.IsCoolingDown && SaltedEarthPvE.Cooldown.IsCoolingDown && ShadowbringerPvE.Cooldown.CurrentCharges == 0 && CarveAndSpitPvE.Cooldown.IsCoolingDown)
			{
				return true;
			}

			return (!TheBlackestNight || CurrentMp >= 6000) && CurrentMp >= 8500;
		}
	}
	#endregion
}