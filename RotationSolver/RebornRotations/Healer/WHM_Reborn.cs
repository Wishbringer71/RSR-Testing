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

	[RotationConfig(CombatType.PvE, Name = "Regen on Tank as they close in on enemies (dungeons only, not Trials/Raids), and keep it up while it's otherwise idle GCD time (Regen is instant-cast, safe to keep up while moving).")]
	public bool UsePreRegen { get; set; } = true;

	[Range(1, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Minimum number of enemies near the tank before combat for the pre-pull Regen above to be worth casting", Parent = nameof(UsePreRegen))]
	public int PreRegenMinHostiles { get; set; } = 2;

	[Range(1, 12, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Minimum number of enemies still around the tank during a wall-to-wall pull for the Regen above to keep being force-refreshed, instead of falling back to normal reactive healing", Parent = nameof(UsePreRegen))]
	public int PreRegenMinWallToWallHostiles { get; set; } = 3;

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

	[RotationConfig(CombatType.PvE, Name = "Spend Thin Air on an expensive spell only while MP is low and Lucid Dreaming cannot cover it")]
	public bool ThinAirOnMpPressureOnly { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Skip Holy for one GCD while every enemy it would hit is already stunned")]
	public bool StretchHolyStun { get; set; } = false;

	[Range(2, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Minimum enemies in Holy's radius before the stun stretch applies", Parent = nameof(StretchHolyStun))]
	public int StretchHolyMinHostiles { get; set; } = 3;

	[RotationConfig(CombatType.PvE, Name = "Hold Holy while a tank carries The Blackest Night, so its barrier is spent")]
	public bool HoldHolyForBlackestNight { get; set; } = false;

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

		if (HolyPvE.EnoughLevel && !ShouldStretchHolyStun() && !ShouldHoldHolyForBarrier())
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