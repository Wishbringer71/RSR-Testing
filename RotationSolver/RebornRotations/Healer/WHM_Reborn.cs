using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.57", RotationType = RotationType.Reborn)]
[SourceCode(Path = "main/RebornRotations/Healer/WHM_Reborn.cs")]

public sealed partial class WHM_Reborn : WhiteMageRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Use the balance Opener in High-End Duties")]
	public bool UseOpenerHighEnd { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Limit Liturgy Of The Bell to multihit party stacks")]
	public bool MultiHitRestrict { get; set; } = false;

	// The owner's decision of 25.10.2026 (concept 08, "Glockenspiel"): beside the stacks, the bell also goes before a hit
	// the measured size says would put a member in danger, and into an attack that hits in a series without a marker.
	[RotationConfig(CombatType.PvE, Name = "Also before a hit that would put a party member in danger", Parent = nameof(MultiHitRestrict),
		Tooltip = "With the limit to multihit party stacks on, the bell also goes before an announced area hit that, by its "
			+ "measured size, would leave an unprotected member in the critical class and that reaches you - after Plenary "
			+ "Indulgence, Temperance and Divine Caress, which lessen the hit itself.\n"
			+ "In a fight: the bell's first heal comes with that hit and the rest right after it, on a party the hit has "
			+ "nearly emptied; a multihit stack in the three minutes after goes without it.")]
	public bool LiturgyBeforeDanger { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Also into an enemy attack that hits in a series", Parent = nameof(MultiHitRestrict),
		Tooltip = "With the limit to multihit party stacks on, the bell also goes out at the first hit of an enemy attack "
			+ "that has been seen to hit you and the party several times in a row - one trigger a second, within the bell's "
			+ "duration -, or just before that hit when an enemy cast of it that reaches you lands within the next GCD. "
			+ "Learned per enemy action over every session; the first time an attack is met it teaches.\n"
			+ "In a fight: an attack of ten hits a second apart heals the party after each of the next five, instead of "
			+ "the bell staying unused; four cast towers in a row heal after all four instead of the last three.")]
	public bool LiturgyOnSeries { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Press Liturgy of the Bell a second time when the party needs an area heal",
		Tooltip = "While the bell still holds stacks, its second press heals with every stack left at once; otherwise "
			+ "they heal only when the bell runs out, often onto a party that is healed by then. It goes out on the "
			+ "area-heal path - once the party is below your area-heal thresholds -, before Asylum.\n"
			+ "In a fight: after a hit that drops the party, the bell's first heal comes with the hit and the rest right "
			+ "after it, instead of when the bell runs out.")]
	public bool LiturgySecondPress { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Enable Swiftcast Restriction Logic to attempt to prevent actions other than Raise when you have swiftcast")]
	public bool SwiftLogic { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use GCDs to heal. (Ignored if you are the only healer in party, or while a party member is about to fall)",
		Tooltip = "Off: while another healer is alive your GCD heals are left to them and you deal damage instead, with "
		+ "your abilities still healing - except while a party member is in the critical class, about to fall by the "
		+ "time a heal lands, when your GCD heals go anyway. On: GCD heals whenever the healing thresholds call for them.\n"
		+ "In a fight: off, a member about to fall still gets your GCD heal, whatever the other healer does.")]
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
	[RotationConfig(CombatType.PvE, Name = "If a party member's health drops below this percentage, the Regen healing ability will not be used on them while Cure II or Cure can be cast",
		Tooltip = "Below this health a member gets Cure II or Cure instead of Regen: they heal more at once.\n"
			+ "In a fight: while you move, or when the member is beyond their reach or you lack the MP for them, "
			+ "neither can be cast. Regen then goes out under this health too - the HoT heals, the damage spell "
			+ "that would come otherwise does not. Benediction is not affected.")]
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
			+ "It uses Lucid Dreaming's own MP threshold "
			+ "rather than a second number, so both decisions read the same value.")]
	public bool ThinAirOnMpPressureOnly { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Stretch the Holy stun",
		Tooltip = "Right after Holy (Sanctus), the next GCD goes to another spell while Holy's stun would "
			+ "still be running when a second Holy lands.\n"
			+ "In a fight: Holy's stun does not stack on top of itself - casting into a stun that is "
			+ "still running overwrites it and the pack starts swinging again sooner. One spell in "
			+ "between puts the second stun at the end of the first, so the same number of casts holds "
			+ "the pack still for longer and the damage stream to the tank stays thinner.\n"
			+ "Costs one GCD of Holy damage per pull: it goes to a DoT where one is due, otherwise to "
			+ "Glare.")]
	public bool StretchHolyStun { get; set; } = true;

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
			+ "while there is stun headroom left to give up; it also needs the enemy count below.")]
	public bool HoldHolyForBlackestNight { get; set; } = true;

	[Range(2, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies in Holy's radius before holding", Parent = nameof(HoldHolyForBlackestNight),
		Tooltip = "How many enemies have to be in range before Holy (Sanctus) may be held back for "
			+ "The Blackest Night.\n"
			+ "In a fight: below this count the pack is thin enough that the tank is not in danger from "
			+ "the stream, so giving up Holy damage buys nothing. Above it, a held stun is worth more "
			+ "than one cast of damage. The stun stretch does not read it: that one follows Holy's own "
			+ "area check.")]
	public int StretchHolyMinHostiles { get; set; } = 3;

	// The third timing of the rule in concept 08; the user asked for it directly after seeing Holy
	// cast into a slow that had just landed. Held for as long as the slow carries, with no
	// replacement guarantee: the held GCDs go to DoTs and then Glare (A178).
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

		// The tincture when Presence of Mind is about to go, whenever it is usable and at hand; where is "Tincture usage"
		// alone (his rule, A500) - the rotation's own switch asked it a second time.
		if (InCombat && !PresenceOfMindPvE.Cooldown.IsCoolingDown && UseBurstMedicine(out act))
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
		// The bell into a series already hitting, or just before its first hit when a cast announces it: every further
		// hit of it triggers the bell (A349, A365).
		if (MultiHitRestrict && LiturgyOnSeries && LiturgyTriggersLeft is var triggers and > 0
			&& LiturgyOfTheBellPvE.CanUse(out act, skipAoeCheck: true))
		{
			DefenseTrace.Decision($"bell into a hit series, {triggers} trigger(s) to come", act);
			return true;
		}

		if (UseDivine && DivineCaressPvE.CanUse(out act))
		{
			return true;
		}

		if (TryChargedBarrierAtCap(DivineBenisonPvE, out act))
		{
			return true;
		}
		return base.GeneralAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.TemperancePvE, ActionID.LiturgyOfTheBellPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		// White mage special rule on the universal stretch (concept 08, "Die Abwehrsperren"): after
		// Temperance or Liturgy of the Bell the rest waits until that effect runs out, unless the
		// party is in danger. Formerly the recast less a written-in 20 s; the duration now comes from
		// the effect texts.
		if (AreaDefenseStretched("White Mage: Temperance or Liturgy still in effect", TemperancePvE, LiturgyOfTheBellPvE))
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

		// Last of the four: the bell heals after the hit, the three before it lessen the hit itself.
		if (!MultiHitRestrict || IsCastingMultiHit
			|| (LiturgyBeforeDanger && AreaHitOnMe && AreaDefenseDanger(out _)))
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
		// The same stretch for the single-target defence: Divine Benison and Aquaveil, each held while the other's effect
		// stands on the target chosen, unless the player or a tank is in the critical class. The other tank of a
		// tankbuster on both carries neither and gets the second at once (A440).
		const string stretch = "White Mage: Divine Benison or Aquaveil still in effect";

		// The charge left once the cap rule has spent one (TryChargedBarrierAtCap) is the one kept for danger: without
		// usedUp the check passed only with every charge back, so after each cap spend Divine Benison stood refused for a
		// tankbuster for up to a whole recharge - three of four tankbusters against Zoraal Ja on 26.10.2026 (A419).
		if (DivineBenisonPvE.CanUse(out act, usedUp: true)
			&& !SingleDefenseStretched(DivineBenisonPvE, stretch, DivineBenisonPvE, AquaveilPvE))
		{
			return true;
		}

		if (AquaveilPvE.CanUse(out act) && !SingleDefenseStretched(AquaveilPvE, stretch, DivineBenisonPvE, AquaveilPvE))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.LiturgyOfTheBellPvE_28509, ActionID.AsylumPvE)]
	protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
	{
		// The owner's proposals (23. and 24.10.2026, concept 08): the rest of the bell's stacks pays only while the party
		// lacks that much - read here by the area-heal path opening at his thresholds -, and after a hit that drops the
		// party it heals at once instead of when the bell runs out. A free oGCD heal ahead of the ones that cost.
		if (LiturgySecondPress && LiturgyOfTheBellPvE_28509.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

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
			BenedictionPvE.Target.Target.GetForecastHealthRatio(true) < BenedictionHeal &&
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
			if (n.HasStatus(true, StatusID.MedicaIi, StatusID.MedicaIii))
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

		// Below RegenHeal the emergency belongs to Cure II and Benediction - except under Walking
		// Dead, where the dark knight heals himself by attacking and the owner's rule asks for a HoT
		// as the light support (concept 09), and where neither Cure II nor Cure can be cast (moving,
		// out of their reach, short of MP): a HoT heals, the damage GCD that comes otherwise does not
		// (concept 07, A694).
		if (RegenPvE.CanUse(out act)
			&& (RegenPvE.Target.Target.GetForecastHealthRatio() > RegenHeal
				|| RegenPvE.Target.Target.HasStatus(false, StatusID.WalkingDead)
				|| (!CureIiPvE.CanUse(out _) && !CurePvE.CanUse(out _))))
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

	private string _lastStretchTrace = string.Empty;
	private DateTime _lastStretchTraceAt = DateTime.MinValue;

	// The figures in a stretch line (the stun left, the landing time) change every frame.
	private static readonly System.Text.RegularExpressions.Regex StretchFigures = new(@"\d+(?:[.,]\d+)?");

	// What the slowed-pack hold last decided, for the diagnostics window. Holding and not holding
	// look alike from outside - the Holy that does not go out is invisible - so the reason is kept
	// where the decision falls.
	private string _holySlowHold = "no decision yet";

	protected override bool GeneralGCD(out IAction? act)
	{
		// Thin Air up for a raise is no reason of its own to hard cast it: whether to is "Hard-cast raise"'s question,
		// answered by the central raise path before this one. Returning RaiseGCD here hard cast past "Do not hard cast
		// Raise" and the healer modes, and chose no GCD at all while the raise could not be cast, moving (A401).
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
			// After an announced raidwide, not before it, unless a lily would be lost first (A292).
			if (!HealForResourceWaitsForAreaHit(liliesFullNoBlood && LilyAfterGCD(1)))
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
	#endregion
}