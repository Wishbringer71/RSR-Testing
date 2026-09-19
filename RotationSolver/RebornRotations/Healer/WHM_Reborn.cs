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

	/// <summary>A raise is pending with Swiftcast up for it, so no GCD method spends the cast.</summary>
	private bool SwiftRaisePending =>
		(HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise);

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

	[RotationConfig(CombatType.PvE, Name = "Keep Regen on the tank through a pull",
		Tooltip = "Regen goes on the tank at five seconds on the pull countdown, again as they close in "
			+ "on a group, and is then refreshed for as long as the pull lasts, using GCDs that would "
			+ "otherwise go to damage.\n"
			+ "In a fight: the tank takes the first hits with a heal already ticking, so the reactive "
			+ "heals start from a higher point instead of chasing a tank who is already low. Regen is "
			+ "instant-cast, so nothing is lost while running.\n"
			+ "Never fires at or below the Regen health threshold below - a real emergency is left to "
			+ "Cure II. Dungeons only: in Trials and Raids the rule does not apply at all, because "
			+ "there the damage is scripted rather than a stream.")]
	public bool UsePreRegen { get; set; } = true;

	[Range(1, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies near the tank before the pull", Parent = nameof(UsePreRegen),
		Tooltip = "How many enemies have to stand within gap-closer range of the tank, before combat "
			+ "starts, for the Regen above to go out.\n"
			+ "Lower: Regen also goes up for a single stray enemy, which costs a GCD you would rather "
			+ "spend on damage. Higher: the tank pulls a small group without it and the first hits land "
			+ "on a tank with nothing ticking.")]
	public int PreRegenMinHostiles { get; set; } = 2;

	[Range(1, 12, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies still around the tank during the pull", Parent = nameof(UsePreRegen),
		Tooltip = "The same count, but during combat: how many enemies have to remain around the tank "
			+ "for Regen to keep being refreshed. Once the pull thins out below this number, healing "
			+ "falls back to reacting to health thresholds.\n"
			+ "Lower: the upkeep runs to the end of the pull, so GCDs keep going to Regen while the "
			+ "last two enemies are dying and the damage no longer warrants it.\n"
			+ "Higher: the upkeep stops early and the tail of the pull is healed reactively, which "
			+ "frees those GCDs for damage but leaves the tank without a regen if the pull is "
			+ "re-engaged.")]
	public int PreRegenMinWallToWallHostiles { get; set; } = 3;

	[RotationConfig(CombatType.PvE, Name = "Use Divine Caress as soon as its available")]
	public bool UseDivine { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Asylum as soon as a single player heal (i.e. tankbusters) while moving, in addition to normal logic")]
	public bool AsylumSingle { get; set; } = false;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum health threshold party member needs to be to use Benediction")]
	public float BenedictionHeal { get; set; } = 0.3f;

	// The user reported Benediction going out on a player the moment he was raised. A resurrected
	// player holds a few percent, carries no aggro and is taking no damage, so the health threshold
	// above reads him as the most urgent member in the party while nothing is happening to him - and
	// the once-per-90s full heal is gone when the tank next needs it.
	//
	// His requirement names the condition rather than the case: the emergency heal is right "falls
	// Gefahr bevorsteht, z.B. grosser heftiger AoE", and where there is no aggro, no announced area
	// cast and no damage arriving, "wuerde doch HoT oder kleinere Heals bzw. beides reichen". That
	// is what IsUnderThreat asks, and it holds for anyone - a damage dealer who just ran out of an
	// area effect is the same situation.
	//
	// Nothing else is needed to get the smaller heals: with Benediction held, this same method falls
	// through to Asylum, Divine Benison and Tetragrammaton, and the GCD path still has Regen and
	// Cure II. The target is not being passed over, only the most expensive answer to it.
	//
	// On by default: this implements a reported malfunction against the user's own requirement.
	// Whoever wants the upstream behaviour back turns it off.
	[RotationConfig(CombatType.PvE, Name = "Benediction only on a target in danger",
		Tooltip = "Benediction needs a reason beyond low health: the target is being attacked or cast "
			+ "at, an area cast is announced, or their health is measurably falling.\n"
			+ "In a fight: a player who was just raised holds a few percent, carries no aggro and is "
			+ "taking no damage - the health threshold reads him as the most urgent member in the party "
			+ "while nothing is happening to him, and the full heal is then gone for the ninety seconds "
			+ "in which the tank needs it. With this on he gets a HoT and the smaller heals instead, "
			+ "and Benediction stays available.\n"
			+ "Off: the old behaviour - health threshold alone decides.")]
	public bool BenedictionNeedsThreat { get; set; } = true;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "If a party member's health drops below this percentage, the Regen healing ability will not be used on them")]
	public float RegenHeal { get; set; } = 0.3f;

	[Range(0, 10000, ConfigUnitType.None, 100)]
	[RotationConfig(CombatType.PvE, Name = "Casting cost requirement for Thin Air to be used")]

	public float ThinAirNeed { get; set; } = 1000;

	[RotationConfig(CombatType.PvE, Name = "How to manage the last thin air charge")]
	public ThinAirUsageStrategy ThinAirLastChargeUsage { get; set; } = ThinAirUsageStrategy.ReserveLastChargeForRaise;

	[RotationConfig(CombatType.PvE, Name = "Thin Air only under MP pressure",
		Tooltip = "Thin Air is only spent on an expensive spell while MP has actually fallen to where "
			+ "Lucid Dreaming would be cast and Lucid cannot answer it - on cooldown, not learned, or "
			+ "switched off. A raise still takes a charge regardless of MP.\n"
			+ "In a fight: without this, a charge goes to the next expensive spell whenever one comes "
			+ "up, so both charges can be gone before MP is anywhere near a problem, and the free cast "
			+ "is missing at the point where MP actually runs out during heavy healing.\n"
			+ "Off by default, which is the old behaviour. It uses Lucid Dreaming's own MP threshold "
			+ "rather than a second number, so both decisions read the same value.")]
	public bool ThinAirOnMpPressureOnly { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Stretch the Holy stun",
		Tooltip = "Holy (Sanctus) is skipped for one GCD while every enemy it would hit is already "
			+ "stunned by it.\n"
			+ "In a fight: Holy's stun does not stack on top of itself - casting into a stun that is "
			+ "still running overwrites it and the pack starts swinging again sooner. Waiting one GCD "
			+ "lets the running stun finish first, so the same number of casts holds the pack still for "
			+ "longer and the damage stream to the tank stays thinner.\n"
			+ "Costs one GCD of Holy damage each time it triggers, and only where a damage GCD is "
			+ "guaranteed to replace it. Off by default because the stretch has not been confirmed in "
			+ "play.")]
	public bool StretchHolyStun { get; set; } = false;

	[Range(2, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies in Holy's radius before holding", Parent = nameof(StretchHolyStun),
		Tooltip = "How many enemies have to stand in the radius of Holy (Sanctus) before it may be "
			+ "held back at all.\n"
			+ "This number governs two rules, not one: the stun stretch above and the Blackest Night "
			+ "hold below. Raising it switches both off for smaller pulls.\n"
			+ "In a fight: below this count the pack is thin enough that the tank is not in danger from "
			+ "the stream, so giving up Holy damage buys nothing. Above it, a held stun is worth more "
			+ "than one cast of damage.")]
	public int StretchHolyMinHostiles { get; set; } = 3;

	[RotationConfig(CombatType.PvE, Name = "Hold Holy while a tank carries The Blackest Night",
		Tooltip = "On a group pull, Holy (Sanctus) is held back while a tank carries the barrier from "
			+ "The "
			+ "Blackest Night.\n"
			+ "In a fight: that barrier only pays the dark knight back with mana if it is fully "
			+ "absorbed. Stunning the pack stops the very hits that would spend it, so the barrier "
			+ "expires unused and the tank loses the return. Holding Holy lets the hits land into the "
			+ "barrier, which is what it is for.\n"
			+ "Group pulls only - in a boss fight the damage arrives as scripted single hits that break "
			+ "the barrier whatever the stun does. Only while the barrier sits on a tank, and only "
			+ "while there is stun headroom left to give up; it also needs the enemy count above.")]
	public bool HoldHolyForBlackestNight { get; set; } = true;

	// On by default, unlike the two above: this one is not a proposal but the third timing of the
	// rule in concept 08, and the user asked for it directly after seeing Holy cast into a slow that
	// had just landed. The cost stays bounded by the same replacement guarantee the stun branch uses.
	[RotationConfig(CombatType.PvE, Name = "Hold Holy while the pack is slowed",
		Tooltip = "Holy (Sanctus) is held back while the pack is already slowed, typically by the "
			+ "tank's Arm's Length.\n"
			+ "In a fight: Slow and Holy's stun both throttle the same incoming stream, and a slowed "
			+ "enemy is already swinging less often. Spending the stun on top of it buys little, while "
			+ "the same stun is worth much more once the Slow has run out and the pack is back at full "
			+ "speed. Holding it keeps that option for the harder moment.\n"
			+ "The hold ends as soon as anyone would actually die inside the GCD it costs - that is "
			+ "measured from the health trend, not set as a number. On by default, because it is the "
			+ "case you reported after seeing Holy cast into a Slow that had just landed.")]
	public bool HoldHolyWhilePackSlowed { get; set; } = true;

	[Range(2, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Slowed enemies before holding", Parent = nameof(HoldHolyWhilePackSlowed),
		Tooltip = "How many enemies in Holy's radius have to carry the Slow before the hold applies. "
			+ "More than half of those in radius must be slowed as well, so this is a floor and not the "
			+ "whole condition.\n"
			+ "Lower: the hold triggers on a couple of slowed enemies while the rest of the pack swings "
			+ "at full speed, and the stun is given up for a throttle that covers only part of the "
			+ "stream. Higher: Holy keeps being cast into a well-slowed pack, which is the waste this "
			+ "rule exists to stop.")]
	public int HoldHolyMinSlowedHostiles { get; set; } = 3;

	// Holding the stun only pays while the incoming stream is still manageable - the user's bound on
	// this rule. That bound is now measured rather than set: ObjectHelper.AnyPartyMemberFalling-
	// WithinHealWindow asks whether anyone actually goes down inside the GCD the hold costs, which
	// is what "manageable" means. A head count of enemies never was: a nine-enemy pull the healing
	// keeps up with is where stretching the throttle pays most, and three enemies killing the tank
	// is where it pays least.
	//
	// This sum stays as an optional ceiling on top, and defaults to 0 = off. Its old default was a
	// figure I invented, sitting between two numbers the user had named; removing the setting
	// outright would discard a value already stored in user configuration.
	[Range(0, 1200, ConfigUnitType.None, 100)]
	[RotationConfig(CombatType.PvE, Name = "Extra cap on total enemy output", Parent = nameof(HoldHolyWhilePackSlowed),
		Tooltip = "An optional second condition on the hold above: it only applies while the summed "
			+ "output of the enemies in Holy's radius stays at or below this figure. The scale is "
			+ "percent per enemy after their own throttles, so 100 is one enemy at full strength and "
			+ "300 is three.\n"
			+ "In a fight: with a figure set, Holy goes out into a slowed pack again as soon as the "
			+ "pack is large enough, so the stun is spent on a stream that the Slow has already "
			+ "thinned - and is no longer available when the Slow expires.\n"
			+ "0 means no cap, and that is the default - the hold is already bounded by the measured "
			+ "question of whether anyone actually goes down inside the GCD it costs, which is what "
			+ "matters. This cap is a head count in disguise: a nine-enemy pull your healing keeps up "
			+ "with is where stretching the throttle pays most, and three enemies killing the tank is "
			+ "where it pays least.")]
	public int HoldHolyMaxHostileOutput { get; set; } = 0;

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

		if (UsePreRegen && remainTime <= 5 && remainTime > 3)
		{
			if (RegenPvE.CanUse(out act, targetOverride: TargetType.Tank))
			{
				return act;
			}

			if (DivineBenisonPvE.CanUse(out act))
			{
				return act;
			}
		}

		return base.CountDownAction(remainTime);
	}
	#endregion

	#region oGCD Logic
	[RotationDesc(ActionID.AetherialShiftPvE)]
	protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
	{
		if (AetherialShiftPvE.CanUse(out act))
		{
			return true;
		}

		return base.MoveForwardAbility(nextGCD, out act);
	}

	/// <summary>
	/// MP has fallen to where Lucid Dreaming would be cast, and Lucid cannot answer it: it is on
	/// cooldown, not yet learned, or switched off. Thin Air is then the only MP relief left, which is
	/// what ThinAirOnMpPressureOnly reserves it for. The threshold is Lucid's own configured one
	/// rather than a second value, so both sides of the decision read the same number.
	/// </summary>
	private bool UnderMpPressure =>
		CurrentMp < Service.Config.LucidDreamingMpThreshold
		&& (!LucidDreamingPvE.EnoughLevel || !LucidDreamingPvE.IsEnabled
			|| LucidDreamingPvE.Cooldown.IsCoolingDown);

	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		var useLastThinAirCharge = ThinAirLastChargeUsage == ThinAirUsageStrategy.UseAllCharges || (ThinAirLastChargeUsage == ThinAirUsageStrategy.ReserveLastChargeForRaise && nextGCD == RaisePvE);
		if (((nextGCD is IBaseAction action && action.Info.MPNeed >= ThinAirNeed && (!ThinAirOnMpPressureOnly || UnderMpPressure) && IActionHelper.IsLastActionGCD()) || ((MergedStatus.HasFlag(AutoStatus.Raise) || (nextGCD == RaisePvE)) && IActionHelper.IsLastActionGCD())) &&
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
			BenedictionPvE.Target.Target.GetHealthRatio() < BenedictionHeal &&
			(!BenedictionNeedsThreat || BenedictionPvE.Target.Target.IsUnderThreat()))
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
	/// <summary>
	/// Proactive wall-to-wall sustain: keep Regen up on the tank as they commit to a pull. Called from
	/// GeneralGCD, HealSingleGCD and HealAreaGCD alike - the outer dispatch reaches the two heal methods
	/// first, so a raised heal-need flag would otherwise starve the check in GeneralGCD for a whole
	/// pull. Never fires at or below <see cref="RegenHeal"/>, leaving a genuine emergency to Cure II /
	/// Cure. targetOverride bypasses the candidate status check (FindTankTarget doesn't call
	/// CheckStatus), so the remaining duration is verified explicitly here.
	/// </summary>
	private bool TrySustainRegenOnTank(out IAction? act)
	{
		act = null;

		if (!UsePreRegen || !TankApproachingMobGroup(PreRegenMinHostiles, PreRegenMinWallToWallHostiles))
		{
			return false;
		}

		if (!RegenPvE.CanUse(out act, targetOverride: TargetType.Tank))
		{
			act = null;
			return false;
		}

		var tank = RegenPvE.Target.Target;
		if (tank != null && tank.GetHealthRatio() > RegenHeal
			&& tank.WillStatusEndGCD(RegenPvE.Config.StatusRefreshGcdCount, 0, RegenPvE.Setting.StatusFromSelf, RegenPvE.Setting.TargetStatusProvide ?? []))
		{
			return true;
		}

		act = null;
		return false;
	}

	[RotationDesc(ActionID.AfflatusRapturePvE, ActionID.MedicaIiPvE, ActionID.CureIiiPvE, ActionID.MedicaPvE)]
	protected override bool HealAreaGCD(out IAction? act)
	{
		if (SwiftRaisePending)
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

		// Last, so it never displaces one of the reactive AoE heals above it.
		if (TrySustainRegenOnTank(out act))
		{
			return true;
		}

		return base.HealAreaGCD(out act);
	}

	[RotationDesc(ActionID.AfflatusSolacePvE, ActionID.RegenPvE, ActionID.CureIiPvE, ActionID.CurePvE)]
	protected override bool HealSingleGCD(out IAction? act)
	{
		if (SwiftRaisePending)
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

		// Ahead of Cure II / Cure, which would otherwise claim every GCD under continuous damage.
		if (TrySustainRegenOnTank(out act))
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

	/// <summary>
	/// Whether this GCD should go to something other than Holy so the stun is not overwritten while
	/// it still runs.
	/// </summary>
	/// <remarks>
	/// A stun lasts 4s, then 2s, then 1s, after which the target is immune - seven seconds to place,
	/// once per pull. Recasting on cooldown lands the second application inside the first and wastes
	/// part of it: about 5.5s of coverage instead of 7s. Yielding a single GCD while the stun runs
	/// recovers the difference, and only one is needed, because the shorter follow-ups are over
	/// before the next cast comes round. Modelled in .github/scripts/audit/stun_coverage.py.
	///
	/// The damage lost is not weighed against this. Keeping the party alive ranks above dealing
	/// damage, so more coverage decides; only the absence of a worthwhile replacement stops it.
	/// The two emergency checks the design first carried were dropped after inspection: the
	/// dispatcher already runs every heal and defense branch ahead of GeneralGCD, so a critical
	/// state never reaches this code, and a predicted raidwide comes from a boss the stun does not
	/// touch.
	///
	/// Radius rather than job range: Holy covers eight yalms while a caster reaches twenty-five,
	/// and the wider set would count enemies the cast never hits.
	/// </remarks>
	private bool ShouldStretchHolyStun()
	{
		if (!StretchHolyStun)
		{
			return false;
		}

		// Only worth it where an area cast is the filler at all, and where a stun still does something.
		//
		// "No headroom" generalises "everyone is stunned" to "nobody left this cast could stun", which
		// covers a pack of two stunned enemies and one already immune. It must not be read as a reason
		// on its own: once every enemy in radius is immune and none is still stunned, there is no stun
		// to protect, and yielding the GCD would trade an area cast for a single-target dot for the
		// rest of the pull. Hence the explicit requirement that a stun is actually running.
		var radius = HolyIiiPvE.EnoughLevel ? HolyIiiPvE.Info.EffectRange : HolyPvE.Info.EffectRange;
		var inRange = SurveyStuns(radius, out var stunned, out var allStunned, out var headroom);
		if (inRange < StretchHolyMinHostiles || stunned == 0 || (!allStunned && headroom))
		{
			return false;
		}

		// Replacement guarantee: yield the GCD only when something with value of its own can take
		// it. Without this the rotation would fall through to Glare, which is a plain loss.
		return DiaPvE.CanUse(out _) || AeroIiPvE.CanUse(out _) || AeroPvE.CanUse(out _);
	}

	/// <summary>
	/// Whether Holy has to wait because a stronger mitigation is already carrying the pull, so the
	/// stun is worth less now than later.
	/// </summary>
	/// <remarks>
	/// The third of the three timings in concept 08: a non-Holy GCD is inserted when it has value of
	/// its own and the stun is not lost by it - because it still runs, because it no longer works,
	/// or because a stronger mitigation is carrying right now. The first two were implemented, this
	/// one was not, while the same document listed the hold as done.
	///
	/// Arm's Length applies Slow +20% to every physical attacker for 15s, and the slow raises
	/// auto-attack delay, which is where trash damage comes from - the same order as Rampart. While
	/// that runs, the stream is already thinned, and spending one of the pull's three stun
	/// applications on it burns a budget that is gone for good: 4s, then 2s, then 1s, then immunity.
	///
	/// The measure is a head count over Holy's own radius, and both halves of it are the user's:
	/// **more than half** the enemies in radius carry the slow, and at least
	/// <see cref="HoldHolyMinSlowedHostiles"/> of them do. The share says the stream as a whole is
	/// thinned rather than one straggler being clipped; the floor keeps a two-enemy remnant from
	/// satisfying the share by arithmetic alone.
	///
	/// The measure this replaced was total enemy output against AoeCount * 100 - the area rule
	/// restated in the unit mitigations are expressed in. It was mine rather than his, and in play
	/// it did nothing: its own documentation noted that it "only ever bites at exactly AoeCount
	/// enemies", because one body beyond that carries at least 80 on its own and clears the
	/// threshold however many are slowed. A wall-to-wall pull always holds more enemies than the
	/// cast needs, so the hold never fired there - which is exactly what the user observed, Holy
	/// going out into a slow that had caught almost every enemy. Five enemies with four slowed came
	/// to 432 against a threshold of 300.
	///
	/// What that shows is not a wrong number but a wrong question. Output asks "is this pull still
	/// worth an area cast", and the answer is yes almost always; the rule has to ask "is the stream
	/// already being handled", and that is a share, not a sum.
	///
	/// Its earlier forms measured the wrong set rather than the wrong quantity: the first borrowed
	/// DRK_Reborn.PackSlowed's share rule, which weighs the stream reaching the tank over job range
	/// instead of what this cast would hit; the next asked for a bare majority with no floor; the
	/// next counted the enemies the slow had not reached.
	///
	/// The replacement guarantee is the stun branch's and bounds the cost the same way: Holy is this
	/// job's only area spell, so a held GCD falls through to single-target damage. Without a DoT
	/// worth placing, Holy goes out no matter how slowed the pack is - which is also why a 15s slow
	/// does not translate into 15s without Holy.
	/// </remarks>
	private bool ShouldHoldHolyWhilePackSlowed()
	{
		if (!HoldHolyWhilePackSlowed)
		{
			return false;
		}

		var holy = HolyIiiPvE.EnoughLevel ? HolyIiiPvE : HolyPvE;
		var radius = holy.Info.EffectRange;

		// No enemy left that this cast could stun, no reason to hold it back. The point of holding
		// is to keep the stun budget for a moment when the stream is not already thinned; once every
		// enemy in radius is stunned or resistant, there is no budget left to keep, and yielding the
		// GCD trades an area cast for a single-target dot without buying anything at all.
		//
		// The user states it as the condition on every hold: only suspend Holy while the enemies can
		// still be stunned by it, otherwise it is pointless. ShouldStretchHolyStun carried this from
		// the start (A50); the two later holds did not, which is one cause in two places.
		SurveyStuns(radius, out _, out var headroom);
		if (!headroom)
		{
			return false;
		}

		var inRange = SurveyHostileStatus(radius, StatusHelper.SlowStatus, out var slowed);

		// Nothing in radius says nothing at all - not "no slow".
		if (inRange == 0 || slowed < HoldHolyMinSlowedHostiles)
		{
			return false;
		}

		// Strictly more than half, so a slow on exactly half the pack is not enough: 3 of 5 holds,
		// 3 of 6 does not, 4 of 6 does.
		if (slowed * 2 <= inRange)
		{
			return false;
		}

		// And only while what is left is still manageable. The share says the stream is being
		// throttled; it does not say the remainder can be healed through. Holding the stun back
		// while somebody is going down stretches a throttle the party does not survive long enough
		// to benefit from.
		//
		// Manageable is measured, not set. It used to be a number I invented - 600, sitting between
		// two figures the user had called manageable and hopeless - and a head count of enemies was
		// never the question anyway: a nine-enemy pull that the healing is keeping up with is
		// exactly where stretching the throttle pays, and a three-enemy pull that is killing the
		// tank is exactly where it does not. What decides is whether anyone actually falls inside
		// the GCD this hold costs, which the health trend answers per member.
		//
		// Nobody falling reads as NaN, and NaN is no death in sight rather than a short time, so a
		// party being held steady never trips this however large the pack.
		if (ObjectHelper.AnyPartyMemberFallingWithinHealWindow())
		{
			return false;
		}

		// The old sum is kept as an optional ceiling for whoever wants one, and defaults to off.
		// Removing the setting outright would discard a value already stored in user configuration;
		// leaving it at a figure I made up would keep deciding on it.
		if (HoldHolyMaxHostileOutput > 0)
		{
			_ = SurveyHostileOutput(radius, out var output);
			if (output > HoldHolyMaxHostileOutput)
			{
				return false;
			}
		}

		return DiaPvE.CanUse(out _) || AeroIiPvE.CanUse(out _) || AeroPvE.CanUse(out _);
	}

	/// <summary>
	/// Whether Holy has to wait because its stun would strand a barrier that pays off only when it
	/// is spent in full.
	/// </summary>
	/// <remarks>
	/// The Blackest Night grants Dark Arts only when its barrier - 25% of maximum HP over 7s - is
	/// absorbed completely (action 7393), and nothing at all when it is not. Stopping the damage
	/// stream for four of those seven seconds is therefore not a saving but a double loss: the
	/// barrier expires unspent, and the stun budget - about seven seconds per pull before the
	/// enemies turn immune - is gone with it. What the stun prevents does not buy that back, because
	/// in a wall-to-wall pull the damage it stops is the damage the barrier was absorbing anyway, on
	/// the same target: the enemies are on the tank, which is who holds the barrier.
	///
	/// This does not wait on the dark knight's rule, which holds the barrier back while a group stun
	/// runs. Both states expire on their own - the stun after four seconds, the barrier after seven
	/// - so neither side can hold the other indefinitely. They yield to whichever landed first, and
	/// when neither is up both simply go.
	///
	/// Two limits keep the cost bounded, and the cost is real: Holy is the only area spell this job
	/// has, so a held GCD falls through to single-target damage. Once every enemy in radius is
	/// immune the cast can no longer interrupt anything and goes out normally, which ends the hold
	/// for the rest of the pull; and a barrier that is over before this cast lands is not worth
	/// waiting for.
	///
	/// A third limit is the tank role. The barrier can sit on anyone - the action reads "self or
	/// target party member", and the dark knight's party branch aims it at the lowest HP - but the
	/// case this rule exists for is the one where the barrier and the stun protect the same person:
	/// the pull is on the tank, so the damage is too. On a damage dealer carrying the barrier, the
	/// stun is likelier to be what keeps them alive, and holding it back would trade a life for a
	/// resource.
	/// </remarks>
	private bool ShouldHoldHolyForBarrier()
	{
		if (!HoldHolyForBlackestNight)
		{
			return false;
		}

		// Group pulls only, by the user's qualification: in a boss fight the rule does not apply.
		// There the damage arrives as scripted single hits that break the barrier on their own
		// whatever the stun does, so holding Holy would give up its damage for a reward that is not
		// at risk. The same hostile count the stun stretch uses, rather than a second number.
		if (NumberOfHostilesInRange < StretchHolyMinHostiles)
		{
			return false;
		}

		var radius = HolyIiiPvE.EnoughLevel ? HolyIiiPvE.Info.EffectRange : HolyPvE.Info.EffectRange;
		_ = SurveyStuns(radius, out _, out _, out var headroom);
		if (!headroom)
		{
			return false;
		}

		var party = PartyMembers;
		if (party == null)
		{
			return false;
		}

		var cast = HolyIiiPvE.EnoughLevel ? HolyIiiPvE.Info.CastTime : HolyPvE.Info.CastTime;
		foreach (var member in party)
		{
			if (member == null || !member.IsJobCategory(JobRole.Tank))
			{
				continue;
			}

			// isFromSelf false: the barrier belongs to the dark knight, not to this healer.
			if (member.HasStatus(false, StatusHelper.FullAbsorbRewardStatus)
				&& !member.WillStatusEnd(cast, false, StatusHelper.FullAbsorbRewardStatus))
			{
				return true;
			}
		}

		return false;
	}

	protected override bool GeneralGCD(out IAction? act)
	{
		if (HasThinAir && MergedStatus.HasFlag(AutoStatus.Raise))
		{
			return RaiseGCD(out act);
		}

		if (SwiftRaisePending)
		{
			return base.GeneralGCD(out act);
		}

		// Ahead of the damage filler: as bottom-of-list it only ever fired on the first pull.
		if (TrySustainRegenOnTank(out act))
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

		if (HolyPvE.EnoughLevel && !ShouldStretchHolyStun() && !ShouldHoldHolyForBarrier()
			&& !ShouldHoldHolyWhilePackSlowed())
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

		if (AeroPvE.EnoughLevel)
		{
			if (DiaPvE.EnoughLevel && DiaPvE.CanUse(out act, skipStatusProvideCheck: DOTUpkeep))
			{
				return true;
			}
			if (AeroIiPvE.EnoughLevel && !DiaPvE.EnoughLevel && AeroIiPvE.CanUse(out act, skipStatusProvideCheck: DOTUpkeep))
			{
				return true;
			}
			if (AeroPvE.EnoughLevel && !AeroIiPvE.EnoughLevel && AeroPvE.CanUse(out act, skipStatusProvideCheck: DOTUpkeep))
			{
				return true;
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