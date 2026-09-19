using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.55")]
[SourceCode(Path = "main/RebornRotations/Healer/AST_Reborn.cs")]

public sealed class AST_Reborn : AstrologianRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Limit Macrocosmos to multihit party stacks")]
	public bool MultiHitRestrict { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Enable Swiftcast Restriction Logic to attempt to prevent actions other than Raise when you have swiftcast")]
	public bool SwiftLogic { get; set; } = true;

	/// <summary>A raise is pending with Swiftcast up for it, so no GCD method spends the cast.</summary>
	private bool SwiftRaisePending =>
		(HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise);

	[RotationConfig(CombatType.PvE, Name = "Use both stacks of Lightspeed while moving")]
	public bool LightspeedMove { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use GCDs to heal. (Ignored if you are the only healer in party)")]
	public bool GCDHeal { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Prioritize Microcosmos over all other healing when available")]
	public bool MicroPrio { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Simple Lord of Crowns logic (use under divinaiton)")]
	public bool SimpleLord { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Detonate Earlthy Star when you have Giant Dominance")]
	public bool StellarNow { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Earthly Star as an attack while moving")]
	public bool StarMove { get; set; } = true;

	[Range(4, 20, ConfigUnitType.Seconds)]
	[RotationConfig(CombatType.PvE, Name = "Use Earthly Star during countdown timer.")]
	public float UseEarthlyStarTime { get; set; } = 4;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP threshold party member needs to be to use Aspected Benefic")]
	public float AspectedBeneficHeal { get; set; } = 0.4f;

	[RotationConfig(CombatType.PvE, Name = "Keep Aspected Benefic on the tank through a pull",
		Tooltip = "Aspected Benefic goes on the tank as they close in on a group, and is refreshed for "
			+ "as long as the pull lasts, using GCDs that would otherwise go to damage.\n"
			+ "In a fight: the tank takes the first hits with a regen already ticking, so the reactive "
			+ "heals start from a higher point instead of chasing a tank who is already low. "
			+ "Aspected Benefic is instant-cast, so nothing is lost while running.\n"
			+ "Never fires at or below the Aspected Benefic health threshold above - a real emergency "
			+ "is left to Benefic II. Dungeons only: in Trials and Raids the rule does not apply, "
			+ "because there the damage is scripted rather than a stream. Unlike the white mage's "
			+ "Regen, it is not placed during the pull countdown, only once the tank moves in.")]
	public bool UsePreAspectedBenefic { get; set; } = true;

	[Range(1, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies near the tank before the pull", Parent = nameof(UsePreAspectedBenefic),
		Tooltip = "How many enemies have to stand within gap-closer range of the tank, before combat "
			+ "starts, for the regen above to go out.\n"
			+ "Lower: it also goes up for a single stray enemy, which costs a GCD you would rather "
			+ "spend on damage. Higher: the tank pulls a small group without it and the first hits land "
			+ "on a tank with nothing ticking.")]
	public int PreAspectedBeneficMinHostiles { get; set; } = 2;

	[Range(1, 12, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies still around the tank during the pull", Parent = nameof(UsePreAspectedBenefic),
		Tooltip = "The same count, but during combat: how many enemies have to remain around the tank "
			+ "for the regen to keep being refreshed. Once the pull thins out below this number, "
			+ "healing falls back to reacting to health thresholds.\n"
			+ "Lower: the upkeep runs to the end of the pull, so GCDs keep going to the regen while "
			+ "the last two enemies are dying and the damage no longer warrants it.\n"
			+ "Higher: the upkeep stops early and the tail of the pull is healed reactively, which "
			+ "frees those GCDs for damage but leaves the tank without a regen if the pull is "
			+ "re-engaged.")]
	public int PreAspectedBeneficMinWallToWallHostiles { get; set; } = 3;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP threshold party member needs to be to use Synastry")]
	public float SynastryHeal { get; set; } = 0.5f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP threshold among party member needed to use Horoscope")]
	public float HoroscopeHeal { get; set; } = 0.5f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum average HP threshold among party members needed to use Lady Of Crowns")]
	public float LadyOfHeals { get; set; } = 0.8f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP threshold party member needs to be to use Essential Dignity 3rd charge")]
	public float EssentialDignityThird { get; set; } = 0.8f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP threshold party member needs to be to use Essential Dignity 2nd charge")]
	public float EssentialDignitySecond { get; set; } = 0.7f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP threshold party member needs to be to use Essential Dignity last charge")]
	public float EssentialDignityLast { get; set; } = 0.6f;

	[RotationConfig(CombatType.PvE, Name = "Prioritize Essential Dignity over single target GCD heals when available")]
	public EssentialPrioStrategy EssentialPrio2 { get; set; } = EssentialPrioStrategy.UseGCDs;

	public enum EssentialPrioStrategy : byte
	{
		[Description("Ignore setting")]
		UseGCDs,

		[Description("When capped")]
		CappedCharges,

		[Description("Any charges")]
		AnyCharges,
	}
	#endregion

	private static bool InBurstStatus => HasDivination;

	private bool InBurstWindow => HasDivination
		|| !DivinationPvE.EnoughLevel
		|| DivinationPvE.Cooldown.HasOneCharge
		|| DivinationPvE.Cooldown.WillHaveOneCharge(5);

	#region Tracking Properties
	public override void DisplayRotationStatus()
	{
		ImGui.Text($"Suntouched 1: {StatusHelper.PlayerWillStatusEndGCD(1, 0, true, StatusID.Suntouched)}");
		ImGui.Text($"Suntouched 2: {StatusHelper.PlayerWillStatusEndGCD(2, 0, true, StatusID.Suntouched)}");
		ImGui.Text($"Suntouched 3: {StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.Suntouched)}");
		ImGui.Text($"Suntouched 4: {StatusHelper.PlayerWillStatusEndGCD(4, 0, true, StatusID.Suntouched)}");
		ImGui.Text($"Suntouched Time: {StatusHelper.PlayerStatusTime(true, StatusID.Suntouched)}");
	}
	#endregion

	#region Countdown Logic
	protected override IAction? CountDownAction(float remainTime)
	{
		if (remainTime < MaleficPvE.Info.CastTime + CountDownAhead && MaleficPvE.CanUse(out var act))
		{
			return act;
		}

		if (remainTime < 3 && UseBurstMedicine(out act))
		{
			return act;
		}

		if (remainTime < UseEarthlyStarTime && EarthlyStarPvE.CanUse(out act, skipTTKCheck: true))
		{
			return act;
		}

		if (remainTime < 30 && AstralDrawPvE.CanUse(out act))
		{
			return act;
		}

		return base.CountDownAction(remainTime);
	}
	#endregion

	#region oGCD Logic
	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		if (MicroPrio && HasMacrocosmos)
		{
			return base.EmergencyAbility(nextGCD, out act);
		}

		if (!InCombat)
		{
			return base.EmergencyAbility(nextGCD, out act);
		}

		if (OraclePvE.CanUse(out act))
		{
			return true;
		}

		if (nextGCD.IsTheSameTo(false, HeliosConjunctionPvE, AspectedHeliosPvE))
		{
			if (NeutralSectPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (nextGCD.IsTheSameTo(false, HeliosConjunctionPvE, HeliosPvE))
		{
			if (PartyMembersAverHP < HoroscopeHeal && HoroscopePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (SynastryPvE.CanUse(out act))
		{
			if (CanCastSynastry(AspectedBeneficPvE, SynastryPvE, SynastryHeal, nextGCD) ||
				CanCastSynastry(BeneficIiPvE, SynastryPvE, SynastryHeal, nextGCD) ||
				CanCastSynastry(BeneficPvE, SynastryPvE, SynastryHeal, nextGCD))
			{
				return true;
			}
		}

		if (DivinationPvE.CanUse(out _) && UseBurstMedicine(out act))
		{
			return true;
		}

		if (StellarNow && HasGiantDominance && StellarDetonationPvE.CanUse(out act))
		{
			return true;
		}

		return base.EmergencyAbility(nextGCD, out act);

		static bool CanCastSynastry(IBaseAction actionCheck, IBaseAction synastry, float synastryHp, IAction next)
			=> next.IsTheSameTo(false, actionCheck) &&
			   synastry.Target.Target == actionCheck.Target.Target &&
			   synastry.Target.Target.GetHealthRatio() < synastryHp;
	}

	[RotationDesc(ActionID.ExaltationPvE, ActionID.TheSpirePvE, ActionID.TheBolePvE, ActionID.CelestialIntersectionPvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (ExaltationPvE.CanUse(out act))
		{
			return true;
		}

		if (InCombat && TheBolePvE.CanUse(out act))
		{
			return true;
		}

		if (InCombat && TheSpirePvE.CanUse(out act))
		{
			return true;
		}

		if (CelestialIntersectionPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.CollectiveUnconsciousPvE, ActionID.SunSignPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (SunSignPvE.CanUse(out act))
		{
			return true;
		}

		if (EarthlyStarPvE.CanUse(out act))
		{
			return true;
		}

		if ((MacrocosmosPvE.Cooldown.IsCoolingDown && !MacrocosmosPvE.Cooldown.WillHaveOneCharge(150))
			|| (CollectiveUnconsciousPvE.Cooldown.IsCoolingDown && !CollectiveUnconsciousPvE.Cooldown.WillHaveOneCharge(40)))
		{
			return base.DefenseAreaAbility(nextGCD, out act);
		}

		if (CollectiveUnconsciousPvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.TheArrowPvE, ActionID.TheEwerPvE, ActionID.EssentialDignityPvE, ActionID.CelestialIntersectionPvE)]
	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (MicroPrio && HasMacrocosmos)
		{
			return base.HealSingleAbility(nextGCD, out act);
		}

		if (InCombat && TheArrowPvE.CanUse(out act))
		{
			return true;
		}

		if (InCombat && TheEwerPvE.CanUse(out act))
		{
			return true;
		}

		if (EssentialDignityPvE.Cooldown.CurrentCharges == 3 && EssentialDignityPvE.CanUse(out act, usedUp: true))
		{
			if (EssentialDignityPvE.Target.Target.GetHealthRatio() < EssentialDignityThird)
			{
				return true;
			}
		}

		if (EssentialDignityPvE.Cooldown.CurrentCharges == 2 && EssentialDignityPvE.CanUse(out act, usedUp: true))
		{
			if (EssentialDignityPvE.Target.Target.GetHealthRatio() < EssentialDignitySecond)
			{
				return true;
			}
		}

		if (EssentialDignityPvE.Cooldown.CurrentCharges == 1 && EssentialDignityPvE.CanUse(out act, usedUp: true))
		{
			if (EssentialDignityPvE.Target.Target.GetHealthRatio() < EssentialDignityLast)
			{
				return true;
			}
		}

		if (CelestialIntersectionPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		return base.HealSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.CelestialOppositionPvE, ActionID.StellarDetonationPvE, ActionID.HoroscopePvE, ActionID.HoroscopePvE_16558, ActionID.LadyOfCrownsPvE)]
	protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (HasGiantDominance && StellarDetonationPvE.CanUse(out act))
		{
			return true;
		}

		if (MicrocosmosPvE.CanUse(out act))
		{
			return true;
		}

		if (MicroPrio && HasMacrocosmos)
		{
			return base.HealAreaAbility(nextGCD, out act);
		}

		if (CelestialOppositionPvE.CanUse(out act))
		{
			return true;
		}

		if (StellarDetonationPvE.CanUse(out act))
		{
			return true;
		}

		if (PartyMembersAverHP < HoroscopeHeal && HoroscopePvE_16558.CanUse(out act))
		{
			return true;
		}

		if (PartyMembersAverHP < HoroscopeHeal && HoroscopePvE.CanUse(out act))
		{
			return true;
		}

		if (LadyOfCrownsPvE.CanUse(out act))
		{
			return true;
		}

		return base.HealAreaAbility(nextGCD, out act);
	}

	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		if (StatusHelper.PlayerHasStatus(true, StatusID.Suntouched) && StatusHelper.PlayerWillStatusEndGCD(3, 0, true, StatusID.Suntouched))
		{
			if (SunSignPvE.CanUse(out act, skipAoeCheck: true, skipTTKCheck: true))
			{
				return true;
			}
		}

		if (PartyMembersAverHP < LadyOfHeals && LadyOfCrownsPvE.CanUse(out act))
		{
			return true;
		}

		if (AstralDrawPvE.Cooldown.WillHaveOneCharge(3) && LadyOfCrownsPvE.CanUse(out act))
		{
			return true;
		}

		if (AstralDrawPvE.Cooldown.WillHaveOneCharge(3) && InCombat && TheEwerPvE.CanUse(out act))
		{
			return true;
		}

		if (AstralDrawPvE.Cooldown.WillHaveOneCharge(3) && InCombat && TheBolePvE.CanUse(out act))
		{
			return true;
		}

		if (UmbralDrawPvE.Cooldown.WillHaveOneCharge(3) && InCombat && TheArrowPvE.CanUse(out act))
		{
			return true;
		}

		if (UmbralDrawPvE.Cooldown.WillHaveOneCharge(3) && InCombat && TheSpirePvE.CanUse(out act))
		{
			return true;
		}

		if (AstralDrawPvE.CanUse(out act))
		{
			return true;
		}

		if (UmbralDrawPvE.CanUse(out act))
		{
			return true;
		}

		if ((HasDivination || !DivinationPvE.Cooldown.WillHaveOneCharge(66) || !DivinationPvE.EnoughLevel) && InCombat && TheBalancePvE.CanUse(out act))
		{
			return true;
		}

		if ((HasDivination || !DivinationPvE.Cooldown.WillHaveOneCharge(66) || !DivinationPvE.EnoughLevel) && InCombat && TheSpearPvE.CanUse(out act))
		{
			return true;
		}

		return base.GeneralAbility(nextGCD, out act);
	}

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		if (SimpleLord && InCombat && HasDivination && LordOfCrownsPvE.CanUse(out act))
		{
			return true;
		}

		if (IsBurst && !IsMoving && InCombat && DivinationPvE.CanUse(out act))
		{
			return true;
		}

		if (AstralDrawPvE.CanUse(out act, usedUp: IsBurst))
		{
			return true;
		}

		if (!HasLightspeed && InCombat &&
			(InBurstStatus
			|| DivinationPvE.Cooldown.ElapsedAfter(115)
			|| DivinationPvE.Cooldown.WillHaveOneCharge(5)
			|| HasDivination) && LightspeedPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		if (InCombat)
		{
			if (!HasLightspeed && IsMoving && LightspeedPvE.CanUse(out act, usedUp: LightspeedMove))
			{
				return true;
			}

			if (((!StarMove && !IsMoving) || StarMove) && !HasGiantDominance && !HasEarthlyDominance && EarthlyStarPvE.CanUse(out act))
			{
				return true;
			}

			if (!SimpleLord &&
				(HasDivination
				|| !DivinationPvE.Cooldown.WillHaveOneCharge(45)
				|| !DivinationPvE.EnoughLevel
				|| UmbralDrawPvE.Cooldown.WillHaveOneCharge(3)) && LordOfCrownsPvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.AttackAbility(nextGCD, out act);
	}
	#endregion

	#region GCD Logic
	/// <summary>
	/// Proactive wall-to-wall sustain: keep Aspected Benefic up on the tank as they commit to a pull.
	/// Called from GeneralGCD, HealSingleGCD and HealAreaGCD alike - the outer dispatch reaches the two
	/// heal methods first, so a raised heal-need flag would otherwise starve the check in GeneralGCD
	/// for a whole pull. Never fires at or below <see cref="AspectedBeneficHeal"/>, leaving a genuine
	/// emergency to Benefic II / Benefic. targetOverride bypasses the candidate status check
	/// (FindTankTarget doesn't call CheckStatus), so the remaining duration is verified explicitly here.
	/// </summary>
	private bool TrySustainAspectedBeneficOnTank(out IAction? act)
	{
		act = null;

		if (!UsePreAspectedBenefic || !TankApproachingMobGroup(PreAspectedBeneficMinHostiles, PreAspectedBeneficMinWallToWallHostiles))
		{
			return false;
		}

		if (!AspectedBeneficPvE.CanUse(out act, targetOverride: TargetType.Tank))
		{
			act = null;
			return false;
		}

		var tank = AspectedBeneficPvE.Target.Target;
		if (tank != null && tank.GetHealthRatio() > AspectedBeneficHeal
			&& tank.WillStatusEndGCD(AspectedBeneficPvE.Config.StatusRefreshGcdCount, 0, AspectedBeneficPvE.Setting.StatusFromSelf, AspectedBeneficPvE.Setting.TargetStatusProvide ?? []))
		{
			return true;
		}

		act = null;
		return false;
	}

	protected override bool DefenseSingleGCD(out IAction? act)
	{
		if ((MacrocosmosPvE.Cooldown.IsCoolingDown && !MacrocosmosPvE.Cooldown.WillHaveOneCharge(150))
			|| (CollectiveUnconsciousPvE.Cooldown.IsCoolingDown && !CollectiveUnconsciousPvE.Cooldown.WillHaveOneCharge(40)))
		{
			return base.DefenseSingleGCD(out act);
		}

		if ((NeutralSectPvE.CanUse(out _) || HasNeutralSect || IsLastAbility(false, NeutralSectPvE)) && AspectedBeneficPvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseSingleGCD(out act);
	}

	[RotationDesc(ActionID.MacrocosmosPvE)]
	protected override bool DefenseAreaGCD(out IAction? act)
	{
		if ((MacrocosmosPvE.Cooldown.IsCoolingDown && !MacrocosmosPvE.Cooldown.WillHaveOneCharge(150))
			|| (CollectiveUnconsciousPvE.Cooldown.IsCoolingDown && !CollectiveUnconsciousPvE.Cooldown.WillHaveOneCharge(40)))
		{
			return base.DefenseAreaGCD(out act);
		}

		if ((NeutralSectPvE.CanUse(out _) || HasNeutralSect || IsLastAbility(false, NeutralSectPvE)) && HeliosConjunctionPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		if ((MultiHitRestrict && IsCastingMultiHit) || !MultiHitRestrict)
		{
			if (MacrocosmosPvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.DefenseAreaGCD(out act);
	}

	[RotationDesc(ActionID.AspectedBeneficPvE, ActionID.BeneficIiPvE, ActionID.BeneficPvE)]
	protected override bool HealSingleGCD(out IAction? act)
	{
		if (SwiftRaisePending)
		{
			return base.HealSingleGCD(out act);
		}

		if (MicroPrio && HasMacrocosmos)
		{
			return base.HealSingleGCD(out act);
		}

		var shouldUseEssentialDignity =
			(EssentialPrio2 == EssentialPrioStrategy.AnyCharges && EssentialDignityPvE.EnoughLevel &&
			 EssentialDignityPvE.Cooldown.CurrentCharges > 0) ||
			(EssentialPrio2 == EssentialPrioStrategy.CappedCharges && EssentialDignityPvE.EnoughLevel &&
			 EssentialDignityPvE.Cooldown.CurrentCharges == EssentialDignityPvE.Cooldown.MaxCharges);

		if (shouldUseEssentialDignity)
		{
			return base.HealSingleGCD(out act);
		}

		if (AspectedBeneficPvE.CanUse(out act))
		{
			if (IsMoving || AspectedBeneficPvE.Target.Target.GetHealthRatio() < AspectedBeneficHeal)
			{
				return true;
			}
		}

		// Ahead of Benefic II / Benefic, which would otherwise claim every GCD under continuous damage.
		if (TrySustainAspectedBeneficOnTank(out act))
		{
			return true;
		}

		if (BeneficIiPvE.CanUse(out act))
		{
			return true;
		}

		if (BeneficPvE.CanUse(out act))
		{
			return true;
		}

		return base.HealSingleGCD(out act);
	}

	[RotationDesc(ActionID.AspectedHeliosPvE, ActionID.HeliosPvE, ActionID.HeliosConjunctionPvE)]
	protected override bool HealAreaGCD(out IAction? act)
	{
		if (SwiftRaisePending)
		{
			return base.HealAreaGCD(out act);
		}

		if (MicroPrio && HasMacrocosmos)
		{
			return base.HealAreaGCD(out act);
		}

		if (HeliosConjunctionPvE.EnoughLevel && HeliosConjunctionPvE.CanUse(out act))
		{
			return true;
		}

		if (!HeliosConjunctionPvE.EnoughLevel && AspectedHeliosPvE.CanUse(out act))
		{
			return true;
		}

		if (HeliosPvE.CanUse(out act))
		{
			return true;
		}

		// Last, so it never displaces one of the reactive AoE heals above it.
		if (TrySustainAspectedBeneficOnTank(out act))
		{
			return true;
		}

		return base.HealAreaGCD(out act);
	}

	[RotationDesc(ActionID.AscendPvE)]
	protected override bool RaiseGCD(out IAction? act)
	{
		if (AscendPvE.CanUse(out act))
		{
			return true;
		}

		return base.RaiseGCD(out act);
	}

	protected override bool GeneralGCD(out IAction? act)
	{
		if (SwiftRaisePending)
		{
			return base.GeneralGCD(out act);
		}

		// Ahead of the damage filler: as bottom-of-list it only ever fired on the first pull.
		if (TrySustainAspectedBeneficOnTank(out act))
		{
			return true;
		}

		if (GravityIiPvE.EnoughLevel && GravityIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!GravityIiPvE.EnoughLevel && GravityPvE.EnoughLevel && GravityPvE.CanUse(out act))
		{
			return true;
		}

		if (CombustIiiPvE.EnoughLevel && CombustIiiPvE.CanUse(out act))
		{
			return true;
		}
		if (!CombustIiiPvE.EnoughLevel && CombustIiPvE.EnoughLevel && CombustIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!CombustIiPvE.EnoughLevel && CombustPvE.EnoughLevel && CombustPvE.CanUse(out act))
		{
			return true;
		}

		if (FallMaleficPvE.EnoughLevel && FallMaleficPvE.CanUse(out act))
		{
			return true;
		}
		if (!FallMaleficPvE.EnoughLevel && MaleficIvPvE.EnoughLevel && MaleficIvPvE.CanUse(out act))
		{
			return true;
		}
		if (!MaleficIvPvE.EnoughLevel && MaleficIiiPvE.EnoughLevel && MaleficIiiPvE.CanUse(out act))
		{
			return true;
		}
		if (!MaleficIiiPvE.EnoughLevel && MaleficIiPvE.EnoughLevel && MaleficIiPvE.CanUse(out act))
		{
			return true;
		}
		if (!MaleficIiPvE.Info.EnoughLevelAndQuest() && MaleficPvE.CanUse(out act))
		{
			return true;
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
