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
	public override void DisplayRotationStatus()
	{
		ImGui.Text($"EnergyDrainPvE: Is Cooling Down: {EnergyDrainPvE.Cooldown.IsCoolingDown}");
	}
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
		if (StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.RefulgentLux))
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
		if (StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.FirebirdTrance))
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

		// BMRRaidwideIn is already the earliest of BMR's timeline/hints/generic raidwide predictions,
		// so unlike the raw BMRDamageIn/BMRDamageType pair this can't fire on a tankbuster meant for someone else.
		if (InCombat && !IsLastAction(false, RadiantAegisPvE)
			&& BMRShouldRefreshBefore(BMRRaidwideIn, 30f, true, null, StatusID.RadiantAegis)
			&& RadiantAegisPvE.CanUse(out act, usedUp: true, skipStatusProvideCheck: true))
		{
			return true;
		}

		return base.GeneralAbility(nextGCD, out act);
	}

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		var inBigInvocation = !SummonBahamutPvE.EnoughLevel || InBahamut || InPhoenix || InSolarBahamut;
		var inSolarUnique = DataCenter.PlayerSyncedLevel() == 100 ? !InBahamut && !InPhoenix && InSolarBahamut : InBahamut && !InPhoenix;
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
		// target anyway, so there is nothing to run into and the full block is free. The distance is
		// the one the rotation already uses for exactly this question, the threshold below which
		// Crimson Cyclone needs no approach. Otherwise Titan, the only block whose value depends on
		// neither position nor an open cast.
		//
		// Waiting for Titan rather than firing into a distant Ifrit costs nothing: the charge stays
		// up and its recast only starts when it is spent.
		var standingAtTheTarget =
			CrimsonCyclonePvE.Target.Target?.DistanceToPlayer() <= CrimsonCycloneDistance;
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
		// If the buff cannot fire there - still cooling down itself - the summon does not wait:
		// searingSettled reads a cooling buff as settled. So the drift cannot build up through this
		// branch either way.
		var bigSummonReady = SummonSolarBahamutPvE.EnoughLevel
			? SummonSolarBahamutPvE.Cooldown.WillHaveOneCharge(WeaponRemain)
			: SummonBahamutPvE.Cooldown.WillHaveOneCharge(WeaponRemain);
		var burstAboutToStart = IsBurst && bigSummonReady;

		var mayFireSearingLight = burstInSolar
			|| burstAboutToStart
			|| (AnotherSummonerInParty
				&& (inBigInvocation || (AllSearingPhasesHeld && fallbackBlockIsWorthIt)));

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
			&& StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.RefulgentLux);

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
		// That is why waiting is the cheaper error. Missing the buff costs 5% of every GCD it misses,
		// 40 potency on the first one alone and again on each that follows, plus the same share for
		// every nearby party member. Waiting costs a summon one GCD later: the trance runs 15s inside
		// a 20s buff, so the phase still fits whole, and the GCD spent waiting is a filler rather than
		// a loss.
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
		// Or it will NOT be ready by the next GCD. This arm used to read "is cooling down", and that
		// was the drift the owner reported: "cooldown von searing light ist später nicht fertig, wenn
		// burst phase läuft. das ist die konsequenz." A buff a second or two short of ready counted as
		// settled, the summon went without it, the buff followed inside the phase - and its next
		// cooldown ended later still, so the gap grew every cycle. Now the summon waits when the buff
		// will be ready within one GCD, which is exactly the size the gap grows by per cycle; the wait
		// pulls buff and summon back into step instead of letting the buff fall behind. A buff further
		// out than that is not waited for, so the wait is never longer than a GCD.
		var searingSettled = !SearingLightPvE.EnoughLevel
			|| !SearingLightPvE.IsEnabled
			|| HasSearingLight
			|| !SearingLightPvE.Cooldown.WillHaveOneCharge(WeaponRemain);

		if (searingSettled && SummonBahamutPvE.CanUse(out act))
		{
			return true;
		}
		if (!SummonBahamutPvE.Info.EnoughLevelAndQuest() && DreadwyrmTrancePvE.CanUse(out act))
		{
			return true;
		}

		if (IsBurst && searingSettled && SummonSolarBahamutPvE.CanUse(out act))
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
