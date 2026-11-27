namespace RotationSolver.Rotations.Healer.WhiteMage;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.58", RotationType = RotationType.Reborn)]
[SourceCode(Path = "main/Rotations/Healer/WhiteMage/WHM_Reborn.cs")]

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
		Tooltip = "In a fight: the bell also goes before an announced area hit that would leave an unprotected "
			+ "member in danger, after the abilities that lessen the hit.\nThe next multihit stack in three "
			+ "minutes then goes without it.")]
	public bool LiturgyBeforeDanger { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Also into an enemy attack that hits in a series", Parent = nameof(MultiHitRestrict),
		Tooltip = "In a fight: the bell also goes at the first hit of an attack learned to hit the party several "
			+ "times in a row, healing after each hit.")]
	public bool LiturgyOnSeries { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Press Liturgy of the Bell a second time when the party needs an area heal",
		Tooltip = "In a fight: once the party is below your area-heal thresholds, Liturgy of the Bell is pressed "
			+ "again, healing with every stack left at once, before Asylum.\nOff: the stacks heal only when the "
			+ "bell runs out.")]
	public bool LiturgySecondPress { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Enable Swiftcast Restriction Logic to attempt to prevent actions other than Raise when you have swiftcast")]
	public bool SwiftLogic { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use GCDs to heal. (Ignored if you are the only healer in party, or while a party member is about to fall)",
		Tooltip = "In a fight: on, your GCD heals follow the healing thresholds. Off, they are left to a living "
			+ "second healer while you deal damage.\nA member about to fall still gets your GCD heal either way.")]
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
		Tooltip = "In a fight: in dungeons, Regen goes on the tank five seconds before a pull and stays up "
			+ "through it, so the first hits meet a ticking heal.\nNot at or below the Regen health threshold.")]
	public bool UsePreRegen { get; set; } = true;

	[Range(1, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies near the tank before the pull", Parent = nameof(UsePreRegen),
		Tooltip = "In a fight: Regen goes on the tank before the pull only when at least this many enemies stand "
			+ "within gap-closer range of him.\nLower: Regen for a single stray. Higher: small pulls start "
			+ "without it.")]
	public int PreRegenMinHostiles { get; set; } = 2;

	[Range(1, 12, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies still around the tank during the pull", Parent = nameof(UsePreRegen),
		Tooltip = "In a fight: Regen on the tank is refreshed during a pull only while at least this many "
			+ "enemies remain around him.\nLower: upkeep to the end of the pull. Higher: it stops early, freeing "
			+ "GCDs for damage.")]
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
		Tooltip = "In a fight: Benediction goes only to a member who is attacked, cast at, under an announced "
			+ "area cast or measurably falling, so the tank keeps it.\nOff: the health threshold alone decides.")]
	public bool BenedictionNeedsThreat { get; set; } = true;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "If a party member's health drops below this percentage, the Regen healing ability will not be used on them while Cure II or Cure can be cast",
		Tooltip = "In a fight: below this health a member gets Cure II or Cure instead of Regen, which heal more "
			+ "at once.\nWhile you move, or when they are out of reach or MP, Regen still goes below it.\nLower: "
			+ "Regen more often. Higher: Cure more often.")]
	public float RegenHeal { get; set; } = 0.3f;

	[Range(0, 10000, ConfigUnitType.None, 100)]
	[RotationConfig(CombatType.PvE, Name = "Casting cost requirement for Thin Air to be used")]

	public float ThinAirNeed { get; set; } = 1000;

	[RotationConfig(CombatType.PvE, Name = "How to manage the last thin air charge")]
	public ThinAirUsageStrategy ThinAirLastChargeUsage { get; set; } = ThinAirUsageStrategy.ReserveLastChargeForRaise;

	[RotationConfig(CombatType.PvE, Name = "Thin Air only under MP pressure",
		Tooltip = "In a fight: Thin Air is spent on an expensive spell only once MP is as low as Lucid "
			+ "Dreaming's threshold and Lucid cannot answer.\nA raise still takes a charge.\nOff: a charge goes to "
			+ "the next expensive spell.")]
	public bool ThinAirOnMpPressureOnly { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Stretch the Holy stun",
		Tooltip = "In a fight: right after Holy, the next GCD goes to another spell while Holy's stun is still "
			+ "running, so the second stun starts when the first ends.\nCosts one GCD of Holy damage per "
			+ "pull.\nOff: Holy follows Holy.")]
	public bool StretchHolyStun { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Hold Holy while a tank carries The Blackest Night",
		Tooltip = "In a fight: on a group pull, Holy waits while a tank carries The Blackest Night, so the hits "
			+ "break it and he gets his mana back.\nOnly while stun headroom is left.")]
	public bool HoldHolyForBlackestNight { get; set; } = true;

	[Range(2, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies in Holy's radius before holding", Parent = nameof(HoldHolyForBlackestNight),
		Tooltip = "In a fight: Holy waits for The Blackest Night only when at least this many enemies are in its "
			+ "radius.\nLower: it waits for thin packs too. Higher: only big packs make it wait.")]
	public int StretchHolyMinHostiles { get; set; } = 3;

	// The third timing of the rule in concept 08; the user asked for it directly after seeing Holy
	// cast into a slow that had just landed. Held for as long as the slow carries, with no
	// replacement guarantee: the held GCDs go to DoTs and then Glare (A178).
	[RotationConfig(CombatType.PvE, Name = "Hold Holy while the pack is slowed",
		Tooltip = "In a fight: Holy waits while the pack is already slowed, typically by the tank's Arm's "
			+ "Length, so its stun is there once the Slow runs out.\nIt goes as soon as anyone would die in the "
			+ "GCD it costs.\nOff: Holy goes into the slowed pack.")]
	public bool HoldHolyWhilePackSlowed { get; set; } = true;

	[Range(2, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Slowed enemies before holding", Parent = nameof(HoldHolyWhilePackSlowed),
		Tooltip = "In a fight: Holy waits for a slowed pack only when at least this many enemies in its radius, "
			+ "and more than half of them, carry the Slow.\nLower: it waits for a partly slowed pack. Higher: "
			+ "Holy goes into a well-slowed pack.")]
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
		Tooltip = "In a fight: the hold on Holy applies only while the enemies in its radius deal at most this "
			+ "much, 100 being one enemy at full strength.\nLower: Holy goes into slowed packs sooner. Higher: it "
			+ "waits for larger packs too.\n0: no cap (default).")]
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
			if (MedicaIiiPvE.EnoughLevel && MedicaIiiPvE.CanUse(out act) && hasMedica2 < (partyCount + 1) / 2 && !IsLastAction(true, MedicaIiPvE))
			{
				return true;
			}

			if (!MedicaIiiPvE.EnoughLevel && MedicaIiPvE.CanUse(out act) && hasMedica2 < (partyCount + 1) / 2 && !IsLastAction(true, MedicaIiPvE))
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