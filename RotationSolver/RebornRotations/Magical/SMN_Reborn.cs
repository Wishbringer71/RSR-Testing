using System.ComponentModel;

namespace RotationSolver.RebornRotations.Magical;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.55")]
[SourceCode(Path = "main/RebornRotations/Magical/SMN_Reborn.cs")]

public sealed class SMN_Reborn : SummonerRotation
{
	#region Config Options

	public enum SummonOrderType : byte
	{
		[Description("Topaz-Emerald-Ruby")] TopazEmeraldRuby,

		[Description("Topaz-Ruby-Emerald")] TopazRubyEmerald,

		[Description("Emerald-Topaz-Ruby")] EmeraldTopazRuby,

		[Description("Ruby-Emerald-Topaz")] RubyEmeraldTopaz,
	}

	[RotationConfig(CombatType.PvE, Name = "Use GCDs to heal. (Ignored if there are no healers alive in party)")]
	public bool GCDHeal { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Crimson Cyclone at any range, regardless of saftey use with caution (Enabling this ignores the below distance setting).")]
	public bool AddCrimsonCyclone { get; set; } = true;

	[Range(1, 20, ConfigUnitType.Yalms)]
	[RotationConfig(CombatType.PvE, Name = "Max distance you can be from the target for Crimson Cyclone use")]
	public float CrimsonCycloneDistance { get; set; } = 3.0f;

	[RotationConfig(CombatType.PvE, Name = "Use Crimson Cyclone when moving")]
	public bool AddCrimsonCycloneMoving { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Swiftcast on ressurection")]
	public bool AddSwiftcastOnRaise { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Raise while in solar bahamut")]
	public bool SBRaise { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Swiftcast on Ruby Ruin when not enough level for Ruby Rite")]
	public bool AddSwiftcastOnLowST { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Swiftcast on Ruby Outburst when not enough level for Ruby Rite")]
	public bool AddSwiftcastOnLowAOE { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Swiftcast on Garuda")]
	public bool AddSwiftcastOnGaruda { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Swiftcast on Ruby Rite if you are not high enough level for Garuda")]
	public bool AddSwiftcastOnRuby { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Order")]
	public SummonOrderType SummonOrder { get; set; } = SummonOrderType.TopazEmeraldRuby;

	[RotationConfig(CombatType.PvE, Name = "Prefer Titan while moving",
		Tooltip = "While you are moving, Titan is summoned ahead of the configured order.\n"
			+ "In a fight: Topaz Rite and its follow-ups are instant-cast, so a Titan phase runs at full "
			+ "output while you are dodging. Garuda and Ifrit need you standing still - taken during "
			+ "movement, their casts are interrupted or simply do not go out, and the phase loses GCDs.\n"
			+ "Titan is only brought forward: whenever Titan is not available at that moment, your "
			+ "configured summon order applies unchanged. Off by default, because it departs from the "
			+ "order you set.")]
	public bool PreferTitanWhileMoving { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Physick above level 30")]
	public bool Healbot { get; set; } = false;

	#endregion

	#region Tracking Properties
	// What the Searing Light rules decide from, and what they cost, for the owner to read during a
	// fight - not for anybody to evaluate afterwards. The held time is the drift: every second the
	// big summon waits moves it, and every demi after it, out of the party's two-minute window.
	public override void DisplayRotationStatus()
	{
		ImGui.Text($"EnergyDrainPvE: Is Cooling Down: {EnergyDrainPvE.Cooldown.IsCoolingDown}");
		ImGui.Text($"Next big summon opens the burst: {NextBigSummonIsBurst}");
		ImGui.Text($"Lux Solaris: range reported {LuxSolarisPvE.TargetInfo.Range:F1} y, radius {LuxSolarisPvE.TargetInfo.EffectRange:F1} y (range 0: on the heal path the heal is anchored on you and the need read in the radius)");
		if (StatusHelper.PlayerHasStatus(true, StatusID.RefulgentLux))
		{
			var hitIn = DataCenter.AnnouncedAreaHitIn;
			ImGui.Text(LuxHeldForAreaHit()
				? $"Lux Solaris expiry held: an area hit lands in {hitIn:F1} s, before Refulgent Lux ends - cast after it"
				: "Lux Solaris expiry: nothing held");
		}
		ImGui.Text($"Another Summoner in party: {AnotherSummonerInParty}");
		ImGui.Text(HostileTarget == null
			? "Fallback block: no hostile target - Titan only"
			: $"Fallback block: {HostileTarget.DistanceToPlayer():F1} y to the target - Ifrit only at 0 y, else Titan");
		var pair = SummonSolarBahamutPvE.EnoughLevel ? " (Bahamut and Phoenix booked as one)" : string.Empty;
		ImGui.Text($"Searing Light phases taken by others: Solar {PhaseBookText(SearingPhase.Solar)}"
			+ $" / Bahamut {PhaseBookText(SearingPhase.Bahamut)} / Phoenix {PhaseBookText(SearingPhase.Phoenix)}{pair}"
			+ $" - all held: {AllSearingPhasesHeld}");
		ImGui.Text("Searing Light vs big summon: " + SearingLightAgainstSummon());
		ImGui.Text($"Big summon held for: {(_summonHeldFor.Length == 0 ? "nothing" : _summonHeldFor)}"
			+ $" - {SummonHeldSecondsNow:F1} s this fight");
	}

	// The expiry clauses fire Lux Solaris so that Refulgent Lux is not lost unspent - with no
	// question of health, since a heal that lands on nobody hurt costs only the weave slot. That
	// changes when an area hit is announced and lands while Refulgent Lux still runs: cast before it,
	// the heal lands on a party about to be hit and is mostly overheal; cast after it, the same heal
	// meets the damage. Owner's observation: a full party, an announced area attack, Lux Solaris
	// before the hit - "zu früh und kontraproduktiv" unless it was the last moment it could go.
	//
	// So the clause waits while the hit lands with at least one GCD of Refulgent Lux left after it,
	// which leaves weave slots for the cast. Every figure is read, none set: the hit's cast bar or
	// BossModReborn's raidwide prediction, the status time, the GCD. When the hit would land too
	// late for a slot after it, nothing is held and the clause fires as before.
	private static bool LuxHeldForAreaHit()
	{
		var landsIn = DataCenter.AnnouncedAreaHitIn;
		var luxLeft = StatusHelper.PlayerStatusTime(true, StatusID.RefulgentLux);
		return landsIn < luxLeft - DataCenter.DefaultGCDTotal;
	}

	private string PhaseBookText(SearingPhase phase) =>
		SearingPhaseHeld(phase) ? "held" : SearingPhaseSeen(phase) ? "seen once" : "free";

	// Where the last Searing Light landed against the last big summon, from the actions the server
	// confirmed. "Before the first demi GCD" is what the rule is for; after it, the first Umbral
	// Impulse went out unbuffed - a buff too far behind for the summon to wait for.
	private static string SearingLightAgainstSummon()
	{
		// Newest first, so the first match of each is the latest.
		ActionRec? summon = null;
		ActionRec? searing = null;
		var records = DataCenter.RecordActions;
		foreach (var rec in records)
		{
			var id = (ActionID)rec.Action.RowId;
			if (summon == null
				&& id is ActionID.SummonSolarBahamutPvE or ActionID.SummonBahamutPvE or ActionID.SummonPhoenixPvE)
			{
				summon = rec;
			}
			else if (searing == null && id == ActionID.SearingLightPvE)
			{
				searing = rec;
			}
		}

		if (summon == null || searing == null)
		{
			return "not seen yet";
		}

		var offset = (searing.UsedTime - summon.UsedTime).TotalSeconds;
		if (offset < 0)
		{
			return $"{-offset:F1} s before the summon";
		}

		var gcdBetween = false;
		foreach (var rec in records)
		{
			if (rec.UsedTime > summon.UsedTime && rec.UsedTime < searing.UsedTime
				&& rec.Action.GetActionCate() is ActionCate.Spell or ActionCate.Weaponskill)
			{
				gcdBetween = true;
				break;
			}
		}

		return $"{offset:F1} s after the summon, "
			+ (gcdBetween ? "AFTER the first demi GCD" : "before the first demi GCD");
	}

	private string _summonHeldFor = string.Empty;
	private double _summonHeldSeconds;
	private DateTime _summonHeldSince = DateTime.MinValue;

	// Counted only while the summon is actually due, so a hold reason that stands between demis does
	// not add up. Counted per hold, from its first moment to its last, so no step between two calls
	// has to be judged - a pause in the GCD evaluation neither adds nor loses time.
	private void NoteSummonHold(string reason)
	{
		var now = DateTime.Now;
		var summonDue = SummonBahamutPvE.EnoughLevel
			&& !InBahamut && !InPhoenix && !InSolarBahamut
			&& SummonTime <= WeaponRemain
			&& SummonBahamutPvE.Cooldown.WillHaveOneCharge(WeaponRemain);
		_summonHeldFor = summonDue ? reason : string.Empty;

		if (!InCombat)
		{
			_summonHeldSeconds = 0;
			_summonHeldSince = DateTime.MinValue;
			return;
		}

		if (_summonHeldFor.Length > 0 && _summonHeldSince == DateTime.MinValue)
		{
			_summonHeldSince = now;
		}
		else if (_summonHeldFor.Length == 0 && _summonHeldSince != DateTime.MinValue)
		{
			_summonHeldSeconds += (now - _summonHeldSince).TotalSeconds;
			_summonHeldSince = DateTime.MinValue;
		}
	}

	// The time held this fight, including a hold that is still running.
	private double SummonHeldSecondsNow => _summonHeldSeconds
		+ (_summonHeldSince == DateTime.MinValue ? 0 : (DateTime.Now - _summonHeldSince).TotalSeconds);
	#endregion

	#region Countdown Logic
	protected override IAction? CountDownAction(float remainTime)
	{
		if (SummonCarbunclePvE.CanUse(out var act))
		{
			return act;
		}
		if (HasSummon && remainTime <= RuinPvE.Info.CastTime + CountDownAhead
			&& RuinPvE.CanUse(out act))
		{
			return act;
		}

		return base.CountDownAction(remainTime);
	}
	#endregion

	#region Additional oGCD Logic
	[RotationDesc(ActionID.LuxSolarisPvE)]
	protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (LuxSolarisPvE.CanUse(out act))
		{
			return true;
		}
		return base.HealAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.RekindlePvE)]
	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (TryRekindle(out act))
		{
			return true;
		}
		return base.HealSingleAbility(nextGCD, out act);
	}

	// Owner's rule: lowest percentage first, and the caster himself when there is nobody else.
	//
	// It used to be TargetType.LowHP, which sorts by CURRENT health points, not by share. Those are
	// different questions and the difference decides who gets the heal: a caster at full health can
	// hold fewer points than a tank at half, because the pools differ by that much. The sort then
	// hands back somebody who needs nothing while the tank keeps falling - and the cast is spent.
	//
	// The game itself measures this action in shares, which settles which of the two is meant: the
	// Rekindle effect text arms its heal-over-time "when HP falls below 75%". A target picked by
	// points can therefore be one the follow-up effect will never trigger on.
	//
	// The self fallback is not a formality. Rekindle only exists while Firebird Trance runs, so a
	// cast that finds no target is lost with the phase, and 400 potency on oneself beats nothing.
	private bool TryRekindle(out IAction? act)
	{
		if (RekindlePvE.CanUse(out act, targetOverride: TargetType.LowHPPercent))
		{
			return true;
		}

		return RekindlePvE.CanUse(out act, targetOverride: TargetType.Self);
	}

	[RotationDesc(ActionID.RadiantAegisPvE, ActionID.AddlePvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (!IsLastAction(false, RadiantAegisPvE) && RadiantAegisPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		if (TryAddleBeforeDamage(out act) || AddlePvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.RadiantAegisPvE, ActionID.AddlePvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (!IsLastAction(false, RadiantAegisPvE) && RadiantAegisPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		if (TryAddleBeforeDamage(out act) || AddlePvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	// Addle mitigates any damage the enemy deals, so the generic BMRDamageIn is the right signal.
	private bool TryAddleBeforeDamage(out IAction? act)
	{
		if (ShouldSustainMitigationDebuff(StatusID.Addle))
		{
			return AddlePvE.CanUse(out act, skipStatusProvideCheck: true);
		}
		act = null;
		return false;
	}
	#endregion

	#region oGCD Logic
	[RotationDesc(ActionID.LuxSolarisPvE)]
	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		if (StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.RefulgentLux) && !LuxHeldForAreaHit())
		{
			if (LuxSolarisPvE.CanUse(out act))
			{
				return true;
			}
		}

		// One branch, not two. There were two: three GCDs before Firebird Trance ends with a target
		// choice, and two GCDs before it without one - but the shorter window is contained in the
		// longer and stood FIRST, so from two GCDs onwards the unaimed call always answered first.
		// The intent was plainly the other way round, an aimed cast with a last-resort behind it, and
		// that is what TryRekindle does: lowest share, else the caster himself.
		//
		// The phase is read from the job gauge, not from Firebird Trance (3229). That status makes
		// Fountain of Fire and Brand of Purgatory castable - PvP actions - and in the basic rotations
		// only ModifyBrandOfPurgatoryPvP reads it (ChurinSMN reads it the way this branch did). Whether
		// PvE sets it is not established: Summon Phoenix's text says "Enters Firebird Trance". A status
		// that is missing counts as "ending now", so if PvE does not set it this branch fired in the
		// first free weave slot of the Phoenix phase instead of near its end. InPhoenix and the summon
		// timer answer the same question in either case.
		if (InPhoenix && SummonTimeEndAfterGCD(3))
		{
			if (TryRekindle(out act))
			{
				return true;
			}
		}

		if (HasSearingLight && InCombat && UseBurstMedicine(out act))
		{
			return true;
		}

		if (RadiantAegisAheadOfRaidwide
			&& RadiantAegisPvE.CanUse(out act, usedUp: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		return base.GeneralAbility(nextGCD, out act);
	}

	// BMRRaidwideIn is already the earliest of BMR's timeline/hints/generic raidwide predictions,
	// so unlike the raw BMRDamageIn/BMRDamageType pair this can't fire on a tankbuster meant for someone else.
	// The horizon is the shield's own duration, as its effect text states it: a shield cast further
	// ahead than that is gone before the hit.
	private bool RadiantAegisAheadOfRaidwide =>
		InCombat && !IsLastAction(false, RadiantAegisPvE)
		&& BMRShouldRefreshBefore(BMRRaidwideIn, DefensiveValues.DurationOf((uint)ActionID.RadiantAegisPvE),
			true, null, StatusID.RadiantAegis);

	// Radiant Aegis "can only be executed while Carbuncle is summoned" (effect text), and a demi
	// replaces Carbuncle for its 15 s. A shield that is due and not out before the summon is gone for
	// the whole phase, so it cannot be pushed into the burst behind Searing Light the way a weave
	// usually can. Pointed out by the owner as something the slot analysis had missed; the effect text
	// is the evidence.
	//
	// Due means the two ways the rotation asks for it anyway, read the way those branches read them:
	// a raidwide BMR announces (GeneralAbility, above), or the defence flag standing (DefenseArea- and
	// DefenseSingleAbility, which the dispatch reaches under the area flag) with no shield of ours up.
	// Castable means level, the option, Carbuncle and a charge - so a disabled, spent or impossible
	// shield never holds a summon back. Without a BMR module the first way is silent, and only a cast
	// already on screen raises the flag; that is the limit of the reactive path, not covered here.
	private bool RadiantAegisDueBeforeDemi =>
		RadiantAegisPvE.EnoughLevel
		&& RadiantAegisPvE.IsEnabled
		&& DataCenter.HasPet()
		&& RadiantAegisPvE.Cooldown.CurrentCharges > 0
		&& (RadiantAegisAheadOfRaidwide
			|| (MergedStatus.HasFlag(AutoStatus.DefenseArea)
				&& !IsLastAction(false, RadiantAegisPvE)
				&& !StatusHelper.PlayerHasStatus(true, StatusID.RadiantAegis)));

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		var inBigInvocation = !SummonBahamutPvE.EnoughLevel || InBahamut || InPhoenix || InSolarBahamut;
		var inSolarUnique = SummonSolarBahamutPvE.EnoughLevel ? !InBahamut && !InPhoenix && InSolarBahamut : InBahamut && !InPhoenix;
		var burstInSolar = (SummonSolarBahamutPvE.EnoughLevel && InSolarBahamut) || (!SummonSolarBahamutPvE.EnoughLevel && InBahamut) || !SummonBahamutPvE.EnoughLevel;

		// Searing Light overwrites, it does not stack, and it comes back exactly as often as the
		// Solar Bahamut window it is tied to. With one Summoner that tie is right. With a second one
		// every window collides and all but one charge is wasted, so with another Summoner in the
		// party the firing window widens - to any big summon, not just Solar, because the minor
		// windows carry 78% of a Solar window and are the natural place for a second caster.
		//
		// Outside a summon the charge goes out only when every burst phase is held by somebody who
		// keeps coming back, and then into a primal block: one carries at best 632 potency per GCD
		// against 947 to 1217 inside a demi, so leaving a burst phase costs more than firing early
		// gains. Which primal block is the question below. Skipping a chance costs nothing by comparison - the charge stays up,
		// its recast only starts when it is spent, and the next burst phase is at most one minor
		// window away.
		//
		// The condition this replaces was `!HasAnySearingLight` - fire as soon as the buff is gone,
		// with no books at all. Measured, that is the blind version of the same move: it wins where
		// every phase happens to be taken and loses where they are not, including below today's
		// narrow rule with two Summoners on fully drifted rotations. The book decides the same thing
		// from the situation instead of from the buff timer.
		//
		// Which block to fall back into, and the answer is not fixed: it depends on where the player
		// is standing. Ifrit is the strongest on paper - 632 potency per GCD against Titan's 464 -
		// but that figure includes Crimson Cyclone, which is a gap closer into melee range. Run into
		// a burst phase for it and the block is bought with a position risk the owner does not take.
		// Without the gap closer the ranking inverts inside the buff: Titan three attacks for 1300
		// potency, Ifrit one to two for 800 to 1420, and Titan's are instant while Ifrit's second
		// slot waits on Ruby Rite's cast time (concept 12).
		//
		// So Ifrit takes precedence only where its premise already holds - the player stands at the
		// target anyway, so there is nothing to run into and the full block is free. Standing at the
		// target means 0 yalms, hitbox to hitbox, the distance the game shows - the owner's own limit:
		// "wenn der beschwörer bereits beim boss steht (0 yalm), dann wäre der gapcloser nur noch
		// damage und kein risiko". Crimson Strike's reach (3 yalms) was used here before, and Crimson
		// Cyclone pulls the player across exactly that distance. The movement safety check reads the
		// same measure (ActionTargetInfo.StandsAtTarget), so both places answer "does the gap closer
		// move him" alike. CrimsonCycloneDistance stays the player's limit for the gap closer itself.
		// Otherwise Titan, the only block whose value depends on neither position nor an open cast.
		//
		// Waiting for Titan rather than firing into a distant Ifrit costs nothing: the charge stays
		// up and its recast only starts when it is spent.
		//
		// Outside a demi only onto an expired buff, not into the last seconds of a running one. The
		// guard lets a charge refresh a buff that is about to end, which pays inside a burst phase and
		// wastes the rest of somebody else's buff outside one - the reason concept 12 gives for V7's
		// second condition. Measured with the plugin's own book: without it the fallback gives away
		// 1.4 points of damage under a buff at three Summoners (searing_light_coverage.py, "guard").
		var standingAtTheTarget = HostileTarget != null && ActionTargetInfo.StandsAtTarget(HostileTarget);
		var fallbackBlockIsWorthIt = TitanActive || (IfritActive && standingAtTheTarget);

		// The phase is entered with the buff already up, not a weave slot later. `burstInSolar` only
		// turns true once the demi stands, so the earliest slot it can offer is the one AFTER the
		// summon GCD - and if that slot is taken, the charge falls somewhere inside the phase instead
		// of at its start. Reported from play twice. Searing Light runs 20s against a 15s demi, so
		// firing it in the slot BEFORE the summon covers the whole phase and keeps the overhang the
		// rotation already plans for; the summon itself is not delayed, because its condition accepts
		// a running buff as readiness.
		//
		// Read from the summon's own readiness, NOT from nextGCD. The summon waits for the buff (see
		// UseSummonsAndTrances), so asking "is the summon the next GCD" would be the chicken-and-egg
		// that kept raising broken for a year: the buff waits to be announced, the announcement waits
		// for the buff, and neither happens. Concept 11 has that case written out.
		//
		// No probe and no later analysis either: cooldown and burst flag are both readable here and
		// now, so the decision stays in the code where it falls.
		//
		// "Ready by the next GCD", not "ready now". Asking for a finished cooldown opened the window
		// only once the summon was already available - and its cooldown runs out ON the GCD grid,
		// because the previous summon was itself a GCD and 60 s is a whole number of GCDs. The weave
		// slot ahead of that GCD had passed by then, so the summon waited one GCD for the buff, then
		// fired late, and its next cooldown - and the buff's - started late with it. One GCD per
		// cycle, every cycle: reported from play as Searing Light slipping further back the longer
		// the fight ran, with no second Summoner in the party. Opening the window in the slot before
		// the summon's cooldown ends lets the buff go first and the summon land on time.
		//
		// If the buff cannot fire there - still cooling down itself, or no weave room because the GCD
		// before the summon is a cast - the summon may wait for it (searingSettled in
		// UseSummonsAndTrances), and this branch fires it in the first weave slot of the GCD spent
		// waiting. Further out, or with a second Summoner in the party, the summon goes without it and
		// the charge takes the next window that opens.
		var bigSummonReady = SummonSolarBahamutPvE.EnoughLevel
			? SummonSolarBahamutPvE.Cooldown.WillHaveOneCharge(WeaponRemain)
			: SummonBahamutPvE.Cooldown.WillHaveOneCharge(WeaponRemain);
		// Only ahead of the summon that opens the burst - Solar Bahamut, or Demi-Bahamut below its
		// level - unless another Summoner widens the window to every big summon. Without that, a lone
		// Summoner whose charge had come loose from Solar fired it ahead of Bahamut or Phoenix, and the
		// summon waited for it there: concept 12 ties him to Solar.
		var burstAboutToStart = IsBurst && bigSummonReady
			&& (NextBigSummonIsBurst || AnotherSummonerInParty);

		var mayFireSearingLight = burstInSolar
			|| burstAboutToStart
			|| (AnotherSummonerInParty
				&& (inBigInvocation || (AllSearingPhasesHeld && fallbackBlockIsWorthIt && !HasAnySearingLight)));

		// The shield first, in the slot ahead of the summon. The flag-driven shield already comes before
		// this branch in the dispatch; the one BMR announces sits in GeneralAbility, after it, so without
		// this Searing Light took the only slot before the demi and the shield was locked out for the
		// phase (RadiantAegisDueBeforeDemi). Any demi, not only the burst one, and with burst switched
		// off as well: every demi replaces Carbuncle.
		var demiAboutToStart = !inBigInvocation
			&& (SummonBahamutPvE.Cooldown.WillHaveOneCharge(WeaponRemain)
				|| (SummonSolarBahamutPvE.EnoughLevel
					&& SummonSolarBahamutPvE.Cooldown.WillHaveOneCharge(WeaponRemain)));
		if (demiAboutToStart && RadiantAegisAheadOfRaidwide
			&& RadiantAegisPvE.CanUse(out act, usedUp: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (mayFireSearingLight)
		{
			if (SearingLightPvE.CanUse(out act))
			{
				return true;
			}
		}

		// Lux Solaris is asked here, after Searing Light and ahead of the Aetherflow spenders, because
		// the two branches that could otherwise carry it both fail in this phase:
		//
		// - HealAreaAbility is only reached while AutoStatus.HealAreaAbility stands, and that flag
		//   wants the party's spread below HealthDifference AND its average below HealthAreaAbility.
		//   One member taking a mechanic raises the spread, so the flag stays down exactly when a
		//   single player is the one who is hurt.
		// - GeneralAbility carries an expiry clause already, but the dispatch asks AttackAbility
		//   first, and in a demi phase that branch always has something - Energy Siphon, Energy Drain,
		//   Enkindle. The clause therefore does not get a slot while the phase runs.
		//
		// Neither is a defect of those branches: the flag is built for a healer's expensive area cast,
		// where healing one hurt player with it is the wrong trade. Lux Solaris is not that. It costs
		// no MP and no GCD, its only cost is the weave slot, and it expires unspent with Refulgent Lux.
		// The question is therefore not "is area healing worth it" but "is this cast wasted".
		//
		// Owner's rule, and it is the answer to that question: fire when the missing health is just
		// large enough for the heal to land in full. That needs the heal in points, which cannot be
		// derived from the 500 potency in the effect text - healing power and gear decide it. So it is
		// measured instead: the effect handler sees what every one of our heals actually restored, and
		// GetObservedHealPerCast hands back the smoothed figure. Nothing is read later and nothing is
		// asked of the player; the rule corrects itself on every cast.
		//
		// Until the first landing has been seen the figure is 0, which means unknown. Then this branch
		// stays out of the way and the old behaviour applies - the heal flag decides, plus the expiry
		// clause below.
		//
		// The expiry clause pays for itself here. Refulgent Lux runs 30s against a 15s demi, so its
		// last GCDs fall AFTER the phase, where the attack branch is thin - the slot it takes there is
		// not a burst slot.
		var healPerCast = DataCenter.GetObservedHealPerCast((uint)ActionID.LuxSolarisPvE);
		var largestMissing = DataCenter.LargestMissingHp;
		var luxLandsInFull = healPerCast > 0 && largestMissing >= healPerCast;
		var luxAboutToExpire = largestMissing > 0
			&& StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.RefulgentLux)
			&& !LuxHeldForAreaHit();

		if (luxLandsInFull || luxAboutToExpire)
		{
			if (LuxSolarisPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (inBigInvocation)
		{
			if (EnergySiphonPvE.CanUse(out act))
			{
				if ((EnergySiphonPvE.Target.Target.IsBossFromTTK() || EnergySiphonPvE.Target.Target.IsBossFromIcon()) && EnergySiphonPvE.Target.Target.IsDying())
				{
					return true;
				}
				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}

			if (EnergyDrainPvE.CanUse(out act))
			{
				if ((EnergyDrainPvE.Target.Target.IsBossFromTTK() || EnergyDrainPvE.Target.Target.IsBossFromIcon()) && EnergyDrainPvE.Target.Target.IsDying())
				{
					return true;
				}
				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}

			if (EnkindleBahamutPvE.CanUse(out act))
			{
				if ((EnkindleBahamutPvE.Target.Target.IsBossFromTTK() || EnkindleBahamutPvE.Target.Target.IsBossFromIcon()) && EnkindleBahamutPvE.Target.Target.IsDying())
				{
					return true;
				}
				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}

			if (EnkindleSolarBahamutPvE.CanUse(out act))
			{
				if ((EnkindleSolarBahamutPvE.Target.Target.IsBossFromTTK() || EnkindleSolarBahamutPvE.Target.Target.IsBossFromIcon()) && EnkindleSolarBahamutPvE.Target.Target.IsDying())
				{
					return true;
				}
				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}

			if (EnkindlePhoenixPvE.CanUse(out act))
			{
				if ((EnkindlePhoenixPvE.Target.Target.IsBossFromTTK() || EnkindlePhoenixPvE.Target.Target.IsBossFromIcon()) && EnkindlePhoenixPvE.Target.Target.IsDying())
				{
					return true;
				}
				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}

			if (DeathflarePvE.CanUse(out act))
			{
				if ((DeathflarePvE.Target.Target.IsBossFromTTK() || DeathflarePvE.Target.Target.IsBossFromIcon()) && DeathflarePvE.Target.Target.IsDying())
				{
					return true;
				}
				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}

			if (SunflarePvE.CanUse(out act))
			{
				if ((SunflarePvE.Target.Target.IsBossFromTTK() || SunflarePvE.Target.Target.IsBossFromIcon()) && SunflarePvE.Target.Target.IsDying())
				{
					return true;
				}
				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}

			if (SearingFlashPvE.CanUse(out act))
			{
				if ((SearingFlashPvE.Target.Target.IsBossFromTTK() || SearingFlashPvE.Target.Target.IsBossFromIcon()) && SearingFlashPvE.Target.Target.IsDying())
				{
					return true;
				}

				if (SummonTime > 0f || !SummonBahamutPvE.EnoughLevel)
				{
					return true;
				}
			}
		}

		if (MountainBusterPvE.CanUse(out act))
		{
			return true;
		}

		if (PainflarePvE.CanUse(out act))
		{
			// HasAnySearingLight, not HasSearingLight: the question here is whether a buff window is
			// running, and a second Summoner's Searing Light raises this player's damage by the same
			// 5%. Asking only about the own buff held the Aetherflow spenders back while standing in
			// someone else's window.
			if ((inSolarUnique && HasAnySearingLight) || !SearingLightPvE.EnoughLevel)
			{
				return true;
			}
			if ((PainflarePvE.Target.Target.IsBossFromTTK() || PainflarePvE.Target.Target.IsBossFromIcon()) && PainflarePvE.Target.Target.IsDying())
			{
				return true;
			}
		}

		if (NecrotizePvE.CanUse(out act))
		{
			if ((inSolarUnique && HasAnySearingLight) || !SearingLightPvE.EnoughLevel)
			{
				return true;
			}
			if ((NecrotizePvE.Target.Target.IsBossFromTTK() || NecrotizePvE.Target.Target.IsBossFromIcon()) && NecrotizePvE.Target.Target.IsDying())
			{
				return true;
			}
			if (EnergyDrainPvE.Cooldown.WillHaveOneChargeGCD(2))
			{
				return true;
			}
		}

		if (FesterPvE.CanUse(out act))
		{
			if ((inSolarUnique && HasAnySearingLight) || !SearingLightPvE.EnoughLevel)
			{
				return true;
			}
			if ((FesterPvE.Target.Target.IsBossFromTTK() || FesterPvE.Target.Target.IsBossFromIcon()) && FesterPvE.Target.Target.IsDying())
			{
				return true;
			}
			if (EnergyDrainPvE.Cooldown.WillHaveOneChargeGCD(2))
			{
				return true;
			}
		}

		if (SearingFlashPvE.CanUse(out act))
		{
			if ((SearingFlashPvE.Target.Target.IsBossFromTTK() || SearingFlashPvE.Target.Target.IsBossFromIcon()) && SearingFlashPvE.Target.Target.IsDying())
			{
				return true;
			}
		}
		return base.AttackAbility(nextGCD, out act);
	}

	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		if (SwiftcastPvE.CanUse(out act))
		{
			if (AddSwiftcastOnRaise && nextGCD.IsTheSameTo(false, ResurrectionPvE))
			{
				return true;
			}
			if (AddSwiftcastOnLowST && !RubyRitePvE.EnoughLevel && nextGCD.IsTheSameTo(false, RubyRuinPvE, RubyRuinIiPvE, RubyRuinIiiPvE))
			{
				return true;
			}
			if (AddSwiftcastOnLowAOE && !RubyRitePvE.EnoughLevel && nextGCD.IsTheSameTo(false, RubyOutburstPvE))
			{
				return true;
			}
			if (AddSwiftcastOnRuby && nextGCD.IsTheSameTo(false, RubyRitePvE) && !ElementalMasteryTrait.EnoughLevel)
			{
				return true;
			}
			if (AddSwiftcastOnGaruda && nextGCD.IsTheSameTo(false, SlipstreamPvE) && ElementalMasteryTrait.EnoughLevel && !InBahamut && !InPhoenix && !InSolarBahamut)
			{
				return true;
			}
		}

		return base.EmergencyAbility(nextGCD, out act);
	}

	#endregion

	#region GCD Logic
	[RotationDesc(ActionID.CrimsonCyclonePvE)]
	protected override bool MoveForwardGCD(out IAction? act)
	{
		if (CrimsonCyclonePvE.CanUse(out act))
		{
			return true;
		}
		return base.MoveForwardGCD(out act);
	}

	[RotationDesc(ActionID.PhysickPvE)]
	protected override bool HealSingleGCD(out IAction? act)
	{
		if ((Healbot || DataCenter.PlayerSyncedLevel() <= 30) && PhysickPvE.CanUse(out act))
		{
			return true;
		}
		return base.HealSingleGCD(out act);
	}

	[RotationDesc(ActionID.ResurrectionPvE)]
	protected override bool RaiseGCD(out IAction? act)
	{
		if ((InSolarBahamut && SBRaise) || !InSolarBahamut)
		{
			if (ResurrectionPvE.CanUse(out act))
			{
				return true;
			}
		}
		return base.RaiseGCD(out act);
	}

	protected override bool GeneralGCD(out IAction? act)
	{
		return UseSummonsAndTrances(out act)
			|| UsePrimalFollowUps(out act)
			|| SummonPrimals(out act)
			|| UseFillers(out act)
			|| base.GeneralGCD(out act);
	}

	private bool UseSummonsAndTrances(out IAction? act)
	{
		if (SummonCarbunclePvE.CanUse(out act))
		{
			return true;
		}

		// The big summon waits for Searing Light, because the buff has to be up BEFORE the burst deals
		// its first damage. Read from the effect texts (ActionId.resx): the summon itself states no
		// potency at all - it enters Lightwyrm Trance and Solar Bahamut then "executes Luxwave
		// automatically on the targets attacked by you". So the first damage of the phase is the first
		// GCD after the summon, Umbral Impulse at 640 plus its automatic Luxwave at 160.
		//
		// That is why a short wait is the cheaper error. For a lone Summoner the wait moves Solar and
		// the buff together - he sets his own burst, so nothing is lost but the schedule of the later
		// demis, which only shows at the end of a fight. Going without the wait is dearer than it
		// looks: the weave slots behind the summon also lie before the first damage, but if both are
		// taken (Lux Solaris, Addle, a potion) the buff lands behind the first Umbral Impulse - and
		// since both cooldowns run from use, it then stays behind in every later Solar phase, the
		// first Umbral Impulse unbuffed each time. That is the drift the owner reported ("cooldown von
		// searing light ist später nicht fertig, wenn burst phase läuft"). The wait pulls it back each
		// cycle. The trance runs 15s inside a 20s buff, so the phase still fits whole.
		//
		// Three arms, and the last two are what keep the wait from costing the phase itself: a charge
		// that is already spent is not coming back inside this window, and below level 66 there is no
		// Searing Light at all. Waiting in either case would trade a 5% buff for the whole burst.
		//
		// This also settles a defect recorded in TODO.md: the same summon was asked twice, once with
		// no condition and once with this one, so the conditional call could never be reached and the
		// coupling it expressed never applied. One call, one condition.
		//
		// Waiting is also the smaller risk here, and naming the branches of THIS job rather than the
		// generic categories is what shows it. Ahead of the summon the Summoner's own heal branches
		// cannot fire at all: Lux Solaris requires the Refulgent Lux status and Rekindle checks
		// InPhoenix, and both of those come FROM a demi phase that has not started yet. What is left
		// ahead of the summon is Radiant Aegis and Addle, and only while a defence flag stands.
		//
		// After the summon it reverses, and that is the more likely reason the buff kept landing inside
		// the phase rather than at its head: the summon's own effect text grants Refulgent Lux, so the
		// moment it resolves Lux Solaris becomes castable, and HealAreaAbility - which the dispatch asks
		// ahead of AttackAbility - can take the very weave slot Searing Light needed, as soon as the
		// heal-area flag stands. The summon creates its own competitor for the slot behind it; the slot
		// ahead of it has no such competitor. (Inference from the dispatch order and the effect text,
		// not observed in play.)
		//
		// The residual risk of waiting is therefore a defence flag standing while the charge is up.
		// Guarding against it with a CanUse probe would be the "CanUse as a question, with targeting as
		// a side effect" pattern recorded as a defect class in TODO.md, so it is not done.
		// Settled means: the summon has nothing left to wait for. Four ways to get there.
		//
		// The buff is up. Or it does not exist at this level. Or the player has switched it off -
		// without that arm a disabled Searing Light never goes on cooldown, never counts as settled,
		// and the summon waits for ever.
		//
		// Or it will not be back in time for a single waiting GCD to carry it. A buff a second or two
		// short of ready used to count as settled: the summon went without it, the buff followed inside
		// the phase, and its next cooldown ended later still.
		//
		// The bound follows a suggestion of the owner, put forward for checking, not as a rule: "es
		// geht einfach um ein bis zwei sekunden am anfang ... den demi so zu verschieben, dass er erst
		// startet, wenn searing light verfügbar ist ... die primal rota muss nicht beendet werden." The
		// summon waits only for a buff that can still be woven into the GCD spent waiting - back
		// before that GCD's weave window closes, which is one GCD from now less the action-ahead
		// margin that ends every weave window. A buff back any later would miss that window too and
		// cost a second waiting GCD. What the waiting GCD is, is not chosen here: the primal branches
		// decide, and a cast without weave room behind it (Ruby Rite) cannot carry the buff, so the
		// summon then waits one more GCD. Every GCD waited shifts every later demi with it.
		//
		// With another Summoner in the party - the second half of the same suggestion - the charge is
		// not tied to this summon: the firing window above widens to every big summon and, when all of
		// them are held, to the strongest primal block. So the summon does not wait for a cooling buff
		// there at all; the charge goes out in the first window that rule opens once it is back.
		//
		// Two arms close waits that never end, and both are the same question: will the buff this
		// summon waits for actually be cast ahead of it?
		// - Any Searing Light, not only our own. The buff does not stack and ours cannot be cast over a
		//   running one (StatusProvide with StatusFromSelf = false). With a second Summoner's buff up,
		//   ours was ready, could not go out, and the summon waited for his buff to run out - every
		//   demi of the fight, the delay the owner named.
		// - Burst switched off. The slot ahead of the summon is opened by burstAboutToStart, which reads
		//   IsBurst; with burst off the buff never goes there, and a ready buff held the summon for good.
		// - A summon that is not the burst one, for a lone Summoner. burstAboutToStart does not open the
		//   slot ahead of it, so the buff would never come.
		var searingBackSoon = AnotherSummonerInParty
			? SearingLightPvE.Cooldown.WillHaveOneCharge(WeaponRemain)
			: SearingLightPvE.Cooldown.WillHaveOneCharge(
				WeaponRemain + WeaponTotal - DataCenter.CalculatedActionAhead);
		var searingSettled = !SearingLightPvE.EnoughLevel
			|| !SearingLightPvE.IsEnabled
			|| !IsBurst
			|| (!NextBigSummonIsBurst && !AnotherSummonerInParty)
			|| HasAnySearingLight
			|| !searingBackSoon;

		// A due shield holds every demi, whatever Searing Light does: once the demi stands, Radiant
		// Aegis cannot be cast for 15 s (RadiantAegisDueBeforeDemi). This is the one wait that is not
		// about damage, and it ends as soon as the shield is out or no longer due. Dreadwyrm Trance
		// below keeps Carbuncle out and is not held.
		var aegisFirst = RadiantAegisDueBeforeDemi;
		NoteSummonHold(aegisFirst ? "Radiant Aegis" : !searingSettled ? "Searing Light" : string.Empty);

		if (!aegisFirst && searingSettled && SummonBahamutPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonBahamutPvE.Info.EnoughLevelAndQuest() && DreadwyrmTrancePvE.CanUse(out act))
		{
			return true;
		}

		if (!aegisFirst && IsBurst && searingSettled && SummonSolarBahamutPvE.CanUse(out act))
		{
			return true;
		}

		act = null;
		return false;
	}

	private bool UsePrimalFollowUps(out IAction? act)
	{
		if (SlipstreamPvE.CanUse(out act, skipCastingCheck: AddSwiftcastOnGaruda && ((!SwiftcastPvE.Cooldown.IsCoolingDown && IsMoving) || HasSwift)))
		{
			return true;
		}

		if ((!IsMoving || AddCrimsonCycloneMoving) && CrimsonCyclonePvE.CanUse(out act) && (AddCrimsonCyclone || CrimsonCyclonePvE.Target.Target.DistanceToPlayer() <= CrimsonCycloneDistance))
		{
			return true;
		}

		if (CrimsonStrikePvE.CanUse(out act))
		{
			return true;
		}

		if (PreciousBrillianceTime(out act))
		{
			return true;
		}

		if (GemshineTime(out act))
		{
			return true;
		}

		if (!DreadwyrmTrancePvE.Info.EnoughLevelAndQuest() && HasHostilesInRange && AetherchargePvE.CanUse(out act))
		{
			return true;
		}

		act = null;
		return false;
	}

	private bool SummonPrimals(out IAction? act)
	{
		if (!InBahamut && !InPhoenix && !InSolarBahamut)
		{
			// Topaz GCDs are instant-cast; Garuda/Ifrit's follow-ups need a stationary summoner.
			// Falls through to the configured order below if Titan isn't available right now.
			if (PreferTitanWhileMoving && IsMoving && TitanTime(out act))
			{
				return true;
			}

			switch (SummonOrder)
			{
				case SummonOrderType.TopazEmeraldRuby:
				default:
					if (TitanTime(out act))
					{
						return true;
					}

					if (GarudaTime(out act))
					{
						return true;
					}

					if (IfritTime(out act))
					{
						return true;
					}

					break;

				case SummonOrderType.TopazRubyEmerald:
					if (TitanTime(out act))
					{
						return true;
					}

					if (IfritTime(out act))
					{
						return true;
					}

					if (GarudaTime(out act))
					{
						return true;
					}

					break;

				case SummonOrderType.EmeraldTopazRuby:
					if (GarudaTime(out act))
					{
						return true;
					}

					if (TitanTime(out act))
					{
						return true;
					}

					if (IfritTime(out act))
					{
						return true;
					}

					break;

				case SummonOrderType.RubyEmeraldTopaz:
					if (IfritTime(out act))
					{
						return true;
					}

					if (GarudaTime(out act))
					{
						return true;
					}

					if (TitanTime(out act))
					{
						return true;
					}

					break;
			}
		}

		act = null;
		return false;
	}

	private bool UseFillers(out IAction? act)
	{
		if (SummonTimeEndAfterGCD() && AttunmentTimeEndAfterGCD() && !InBahamut && !InPhoenix && !InSolarBahamut &&
			RuinIvPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (BrandOfPurgatoryPvE.CanUse(out act))
		{
			return true;
		}
		if (UmbralFlarePvE.CanUse(out act))
		{
			return true;
		}
		if (AstralFlarePvE.CanUse(out act))
		{
			return true;
		}
		if (OutburstPvE.CanUse(out act))
		{
			return true;
		}

		if (FountainOfFirePvE.CanUse(out act))
		{
			return true;
		}
		if (UmbralImpulsePvE.CanUse(out act))
		{
			return true;
		}
		if (AstralImpulsePvE.CanUse(out act))
		{
			return true;
		}
		if (RuinIiiPvE.EnoughLevel && RuinIiiPvE.CanUse(out act))
		{
			return true;
		}
		if (!RuinIiiPvE.Info.EnoughLevelAndQuest() && RuinIiPvE.EnoughLevel && RuinIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!RuinIiPvE.Info.EnoughLevelAndQuest() && RuinPvE.CanUse(out act))
		{
			return true;
		}
		act = null;
		return false;
	}
	#endregion

	#region Extra Methods
	private bool TitanTime(out IAction? act)
	{
		if (SummonTitanIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonTitanIiPvE.EnoughLevel && SummonTitanPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonTitanPvE.Info.EnoughLevelAndQuest() && SummonTopazPvE.CanUse(out act))
		{
			return true;
		}
		return false;
	}

	private bool GarudaTime(out IAction? act)
	{
		if (SummonGarudaIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonGarudaIiPvE.EnoughLevel && SummonGarudaPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonGarudaPvE.Info.EnoughLevelAndQuest() && SummonEmeraldPvE.CanUse(out act))
		{
			return true;
		}
		return false;
	}

	private bool IfritTime(out IAction? act)
	{
		if (SummonIfritIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonIfritIiPvE.EnoughLevel && SummonIfritPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonIfritPvE.Info.EnoughLevelAndQuest() && SummonRubyPvE.CanUse(out act))
		{
			return true;
		}
		return false;
	}

	private bool GemshineTime(out IAction? act)
	{
		if (RubyRitePvE.CanUse(out act))
		{
			return true;
		}
		if (EmeraldRitePvE.CanUse(out act))
		{
			return true;
		}
		if (TopazRitePvE.CanUse(out act))
		{
			return true;
		}

		if (RubyRuinIiiPvE.CanUse(out act))
		{
			return true;
		}
		if (EmeraldRuinIiiPvE.CanUse(out act))
		{
			return true;
		}
		if (TopazRuinIiiPvE.CanUse(out act))
		{
			return true;
		}

		if (RubyRuinIiPvE.CanUse(out act))
		{
			return true;
		}
		if (EmeraldRuinIiPvE.CanUse(out act))
		{
			return true;
		}
		if (TopazRuinIiPvE.CanUse(out act))
		{
			return true;
		}

		if (!SummonIfritPvE.Info.EnoughLevelAndQuest() && RubyRuinPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonGarudaPvE.Info.EnoughLevelAndQuest() && EmeraldRuinPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonTitanPvE.Info.EnoughLevelAndQuest() && TopazRuinPvE.CanUse(out act))
		{
			return true;
		}
		return false;
	}

	private bool PreciousBrillianceTime(out IAction? act)
	{
		if (RubyCatastrophePvE.CanUse(out act))
		{
			return true;
		}
		if (EmeraldCatastrophePvE.CanUse(out act))
		{
			return true;
		}
		if (TopazCatastrophePvE.CanUse(out act))
		{
			return true;
		}

		if (RubyDisasterPvE.CanUse(out act))
		{
			return true;
		}
		if (EmeraldDisasterPvE.CanUse(out act))
		{
			return true;
		}
		if (TopazDisasterPvE.CanUse(out act))
		{
			return true;
		}

		if (RubyOutburstPvE.CanUse(out act))
		{
			return true;
		}
		if (EmeraldOutburstPvE.CanUse(out act))
		{
			return true;
		}
		if (TopazOutburstPvE.CanUse(out act))
		{
			return true;
		}
		return false;
	}

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

			return base.CanHealSingleSpell && (GCDHeal || aliveHealerCount == 0);
		}
	}
	#endregion
}
