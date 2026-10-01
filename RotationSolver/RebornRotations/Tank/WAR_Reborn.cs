using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.56")]
[SourceCode(Path = "main/RebornRotations/Tank/WAR_Reborn.cs")]

public sealed class WAR_Reborn : WarriorRotation
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
		Tooltip = "Nascent Flash on someone else puts Bloodwhetting on its cooldown too. With this on, it "
			+ "is not given away while you need Bloodwhetting yourself: a tankbuster on you comes before "
			+ "the cooldown is back and Bloodwhetting would be used for it, or you are about to die.\n"
			+ "In a fight: the member still gets it when they are about to die and win the triage - a "
			+ "healer always, another tank unless you are about to die too; a damage dealer does not. "
			+ "You heal yourself the same either way: Nascent Flash heals you with every weaponskill as "
			+ "Bloodwhetting does. What you keep is its damage reduction and barrier.")]
	public bool HoldNascentFlashForOwnNeed { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Arm's Length on a pull for its Slow",
		Tooltip = "Arm's Length is used on a group pull for its Slow, not only as knockback "
			+ "protection.\n"
			+ "In a fight: the Slow +20% lands on every enemy that strikes you and delays "
			+ "auto-attacks as well as casts, so in a standing pack it throttles the whole incoming "
			+ "stream for fifteen seconds. It costs nothing but its own cooldown. A pull is as many "
			+ "enemies in reach as the global \"Number of hostiles\" for defensive abilities, bosses not "
			+ "counted - Arm's Length does not soften the hit that strikes you, so a boss's tankbuster "
			+ "gains nothing from it; a pack "
			+ "that is already slowed is left alone.\n"
			+ "Not while BossModReborn announces a knockback that lands after the barrier has run out "
			+ "and before Arm's Length is ready again - the action is kept for it.")]
	public bool UseArmsLengthOnPull { get; set; } = true;

	protected override bool HasOwnArmsLengthPullRule => true;

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
		Tooltip = "Thrill of Battle goes out when a tankbuster on you lands within its duration, "
			+ "not only as a heal at low health.\n"
			+ "In a fight: your maximum HP rise by 20% and are filled, so the hit takes a smaller share "
			+ "of your health, and healing on you is 20% stronger while it lasts - the healers' answer "
			+ "to the hit included. It stacks with Bloodwhetting, Rampart or Damnation. The price is its "
			+ "90-second cooldown: it is not there as an emergency heal until then.\n"
			+ "The tankbuster is the one BossModReborn announces for you, or a listed tankbuster cast or "
			+ "tankbuster marker on you; not while an invulnerability covers you.")]
	public bool UseThrillForTankbuster { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Thrill of Battle when your health will fall below its threshold within its duration",
		Tooltip = "Thrill of Battle goes out as soon as your health, falling at the rate measured over the "
			+ "last seconds, would pass \"Thrill Of Battle Heal Threshold\" within the ten seconds Thrill of "
			+ "Battle lasts - not only once it has passed it.\n"
			+ "In a fight: on a heavy pull the 20% extra health and the 20% stronger healing are there "
			+ "while the pack is at full strength, and they carry Bloodwhetting's and Equilibrium's heals "
			+ "with them. Where the healers hold your health steady it does not fire. The price is the "
			+ "90-second cooldown: it may be spent on a pull that would have stayed above the threshold.")]
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

		if (OrogenyPvE.CanUse(out act))
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
		var _partyCount = 0;
		foreach (var _ in PartyMembers)
		{
			_partyCount++;
		}

		if ((InCombat && Player?.GetForecastHealthRatio(true) < HealIntuition && NumberOfHostilesInRange > 0) || (InCombat && _partyCount == 1 && NumberOfHostilesInRange > 0))
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
		if (ShakeItOffPvE.CanUse(out act, skipAoeCheck: true))
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

		// No stop while Bloodwhetting runs: it held Damnation, Rampart and Reprisal back for its eight
		// seconds, at the start of every pull and on every tankbuster it went out for (A238). Rampart and
		// Damnation still stagger against each other through their status check.
		// Predicted tankbuster takes priority over the elapsed-time stagger below.
		if (BMRShouldRefreshBefore(BMRTankbusterIn, 15f, true, null, DamnationPvE.EnoughLevel ? StatusID.Damnation : StatusID.Vengeance)
			&& (DamnationPvE.EnoughLevel ? DamnationPvE.CanUse(out act, skipStatusProvideCheck: true) : VengeancePvE.CanUse(out act, skipStatusProvideCheck: true)))
		{
			return true;
		}

		if (BMRShouldRefreshBefore(BMRTankbusterIn, 20f, true, null, StatusID.Rampart) && RampartPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && DamnationPvE.CanUse(out act) && DamnationPvE.EnoughLevel)
		{
			return true;
		}

		if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && VengeancePvE.CanUse(out act) && !DamnationPvE.EnoughLevel)
		{
			return true;
		}

		if (!VengeancePvE.EnoughLevel)
		{
			if (RampartPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (VengeancePvE.EnoughLevel && !DamnationPvE.EnoughLevel)
		{
			if (VengeancePvE.Cooldown.IsCoolingDown && VengeancePvE.Cooldown.ElapsedAfter(30) && RampartPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (DamnationPvE.EnoughLevel)
		{
			if (DamnationPvE.Cooldown.IsCoolingDown && DamnationPvE.Cooldown.ElapsedAfter(30) && RampartPvE.CanUse(out act))
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

	[RotationDesc(ActionID.ShakeItOffPvE, ActionID.ReprisalPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (ShakeItOffPvE.CanUse(out act, skipAoeCheck: true))
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
			if (ChaoticCyclonePvE.CanUse(out act))
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
			if (DecimatePvE.CanUse(out act, skipStatusProvideCheck: true))
			{
				return true;
			}
			if (!DecimatePvE.Info.EnoughLevelAndQuest() && SteelCyclonePvE.CanUse(out act, skipStatusProvideCheck: true))
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
				if (PrimalRendPvE.Target.Target != null && PrimalRendPvE.Target.Target.DistanceToPlayer() <= PrimalRendDistance2)
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

		// AOE
		if (!StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.SurgingTempest) || !StormsEyePvE.EnoughLevel)
		{
			if (DecimatePvE.CanUse(out act, skipStatusProvideCheck: true))
			{
				return true;
			}
			if (!DecimatePvE.Info.EnoughLevelAndQuest() && SteelCyclonePvE.CanUse(out act))
			{
				return true;
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

	// Whether the single-target defence casts Bloodwhetting (Raw Intuition before level 82): the one
	// condition, read by that path and by the Nascent Flash hold, so the two cannot disagree (A226).
	private bool BloodwhettingForDefense => SoloIntuition || NumberOfHostilesInRange > 2;

	/// <summary>
	/// Whether the warrior needs Bloodwhetting himself, which Nascent Flash on someone else would put on
	/// its shared cooldown (concept 09, "Krieger: Nascent Flash für einen anderen oder Bloodwhetting für
	/// sich"): he is about to die, or a tankbuster on him lands before the cooldown is back and the
	/// single-target defence would cast Bloodwhetting for it.
	/// </summary>
	/// <remarks>
	/// The healing is not what he keeps - Nascent Flash heals him with every weaponskill as Bloodwhetting
	/// does (effect text) - but the damage reduction and the barrier. How hard the tankbuster hits is not
	/// known (BossModReborn gives no amount, nothing measures it), so an announced one on him counts as
	/// possibly lethal, unless an invulnerability covers him while it is cast. Beyond the window in which
	/// BossModReborn names who is hit, it is taken to be his while his target has him targeted (an
	/// inference: some hit the second in enmity).
	/// </remarks>
	private bool NeedsBloodwhettingHimself(out bool aboutToDie)
	{
		aboutToDie = false;
		var player = Player;
		if (player == null)
		{
			return false;
		}

		aboutToDie = player.IsInCriticalClass();
		if (aboutToDie)
		{
			return true;
		}

		// Raw Intuition's own check: Bloodwhetting goes out only while his target has him targeted. An
		// off-tank marked for a tankbuster does not get it from the defence path, so holding for it
		// would keep Nascent Flash from the member for nothing.
		if (!BloodwhettingForDefense || !ObjectHelper.PlayerIsTargetOnSelf())
		{
			return false;
		}

		if (DataCenter.IsHostileCastingTankBusterAtMe)
		{
			return player.NoNeedHealingInvuln();
		}

		if (!BMRTankbusterWithin(NascentFlashPvE.Cooldown.RecastTimeOneChargeRaw))
		{
			return false;
		}

		// BossModReborn names who is hit only inside its mitigation window; further ahead, the one who has
		// the enemy's attention is taken to get it.
		return DataCenter.BMRTankbusterHitsPlayer ?? true;
	}

	/// <summary>
	/// The triage when the warrior and a member both need it (concept 09): the member must be about to
	/// die, and then a healer wins, another tank wins unless the warrior is about to die too, and a
	/// damage dealer - the one most easily given up - does not.
	/// </summary>
	private static bool WinsTriageOverWarrior(IBattleChara member, bool warriorAboutToDie)
	{
		if (!member.IsInCriticalClass())
		{
			return false;
		}

		if (member.IsJobCategory(JobRole.Healer))
		{
			return true;
		}

		return member.IsJobCategory(JobRole.Tank) && !warriorAboutToDie;
	}

	// Picks a party member under the Nascent Flash threshold: by the lowest HP%, optionally healers only,
	// or by the heal target order (concept 07). The target filter is swapped in for this call only and
	// restored afterwards.
	// Wicked bodge but it works for now
	// TODO: make a more generic "target filter swap" method in ActionSetting
	private bool NascentFlashCanUse(out IAction? act, bool healersOnly, bool byDanger = false)
	{
		var setting = NascentFlashPvE.Setting;
		var canTarget = setting.CanTarget;
		var healRatio = Math.Min(FlashHeal, NascentFlashPvE.Config.AutoHealRatio);

		var held = false;
		var warriorAboutToDie = false;
		if (HoldNascentFlashForOwnNeed)
		{
			held = NeedsBloodwhettingHimself(out warriorAboutToDie);
		}

		// NoNeedHealingInvuln is true while NO invulnerability is up: the candidate is unprotected. It
		// read negated here, which let only the invulnerable through - Nascent Flash went to a tank under
		// Holmgang, Superbolide, Hallowed Ground or Living Dead, and to nobody else (A226).
		setting.CanTarget = t => canTarget(t)
			&& t.GetForecastHealthRatio(true) < healRatio
			&& t.NoNeedHealingInvuln()
			&& !t.HasStatus(false, StatusHelper.HealingIneffectiveStatus)
			&& (!healersOnly || t.IsJobCategory(JobRole.Healer))
			&& (!held || WinsTriageOverWarrior(t, warriorAboutToDie));

		try
		{
			return NascentFlashPvE.CanUse(out act, targetOverride: byDanger ? TargetType.Heal : TargetType.LowHPPercent);
		}
		finally
		{
			setting.CanTarget = canTarget;
		}
	}
	#endregion
}