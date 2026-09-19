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
		if (RekindlePvE.CanUse(out act, targetOverride: TargetType.LowHP))
		{
			return true;
		}
		return base.HealSingleAbility(nextGCD, out act);
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

		if (StatusHelper.PlayerWillStatusEndGCD(2, 0, true, StatusID.FirebirdTrance))
		{
			if (RekindlePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.FirebirdTrance))
		{
			if (RekindlePvE.CanUse(out act, targetOverride: TargetType.LowHP))
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
		var bigSummonReady = SummonSolarBahamutPvE.EnoughLevel
			? !SummonSolarBahamutPvE.Cooldown.IsCoolingDown
			: !SummonBahamutPvE.Cooldown.IsCoolingDown;
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
		// its first damage - a buff that lands one weave slot into the phase leaves the strongest GCDs
		// of the cycle unbuffed, and the demi GCDs carry 947 to 1217 potency against 632 outside.
		//
		// Three arms, and the last two are what keep the wait from costing the phase itself: a charge
		// that is already spent is not coming back inside this window, and below level 66 there is no
		// Searing Light at all. Waiting in either case would trade a 5% buff for the whole burst.
		//
		// This also settles a defect recorded in TODO.md: the same summon was asked twice, once with
		// no condition and once with this one, so the conditional call could never be reached and the
		// coupling it expressed never applied. One call, one condition.
		// The risk of waiting, stated rather than hidden: while the charge is up but the weave slot
		// keeps going to emergency, interrupt, healing or defence, the phase is held back with it.
		// Searing Light is a self-buff whose only action check is being in combat, so it is normally
		// castable in the very next slot - but under sustained healing pressure the burst can start
		// late. Guarding that with a CanUse probe here would be the "CanUse as a question, with
		// targeting as a side effect" pattern recorded as a defect class in TODO.md, so it is not
		// done; the trade is a rare late burst against a buff that regularly missed its own phase.
		var searingSettled = !SearingLightPvE.EnoughLevel
			|| HasSearingLight
			|| SearingLightPvE.Cooldown.IsCoolingDown;

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
