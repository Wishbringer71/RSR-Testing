namespace RotationSolver.Rotations.Healer.Astrologian;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.58", RotationType = RotationType.Reborn)]
[SourceCode(Path = "main/Rotations/Healer/Astrologian/AST_Reborn.cs")]

public sealed partial class AST_Reborn : AstrologianRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Limit Macrocosmos to multihit party stacks")]
	public bool MultiHitRestrict { get; set; } = false;

	// The bell's two cases for every healer with such a limit (concept 08, "Glockenspiel", A468).
	[RotationConfig(CombatType.PvE, Name = "Also before a hit that would put a party member in danger", Parent = nameof(MultiHitRestrict),
		Tooltip = "In a fight: Macrocosmos also goes before an announced area hit that would leave an "
			+ "unprotected member in danger, and heals half of it back when it ends.")]
	public bool MacrocosmosBeforeDanger { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Also into an enemy attack that hits in a series", Parent = nameof(MultiHitRestrict),
		Tooltip = "In a fight: Macrocosmos also goes into an attack learned to hit the party several times in a "
			+ "row, and heals the series back when it ends.")]
	public bool MacrocosmosOnSeries { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Enable Swiftcast Restriction Logic to attempt to prevent actions other than Raise when you have swiftcast")]
	public bool SwiftLogic { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use both stacks of Lightspeed while moving")]
	public bool LightspeedMove { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use GCDs to heal. (Ignored if you are the only healer in party, or while a party member is about to fall)",
		Tooltip = "In a fight: on, your GCD heals follow the healing thresholds. Off, they are left to a living "
			+ "second healer while you deal damage.\nA member about to fall still gets your GCD heal either way.")]
	public bool GCDHeal { get; set; } = true;

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
		Tooltip = "In a fight: in dungeons, Aspected Benefic goes on the tank as he closes in and stays up "
			+ "through the pull, so the first hits meet a heal.\nNot at or below its health threshold.")]
	public bool UsePreAspectedBenefic { get; set; } = true;

	[Range(1, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies near the tank before the pull", Parent = nameof(UsePreAspectedBenefic),
		Tooltip = "In a fight: Aspected Benefic goes on the tank as he closes in only when at least this many "
			+ "enemies stand within gap-closer range of him.\nLower: also for a single stray. Higher: small pulls "
			+ "start without it.")]
	public int PreAspectedBeneficMinHostiles { get; set; } = 2;

	[Range(1, 12, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies still around the tank during the pull", Parent = nameof(UsePreAspectedBenefic),
		Tooltip = "In a fight: Aspected Benefic on the tank is refreshed during a pull only while at least this "
			+ "many enemies remain around him.\nLower: upkeep to the end of the pull. Higher: it stops early, "
			+ "freeing GCDs for damage.")]
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

	// The owner's rule, first built for Benediction (BenedictionNeedsThreat): an emergency measure only
	// where there is danger, otherwise a HoT and the smaller heals are enough. Every healer carries one,
	// so the rule reads the same shared check (ObjectHelper.IsUnderThreat) on each.
	[RotationConfig(CombatType.PvE, Name = "Last charge of Essential Dignity only on a target in danger",
		Tooltip = "In a fight: the last charge of Essential Dignity goes only to a member who is attacked, cast "
			+ "at, under an announced area cast or measurably falling.\nOff: the health threshold alone decides.")]
	public bool EssentialDignityNeedsThreat { get; set; } = true;

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
		if (!InCombat)
		{
			return base.EmergencyAbility(nextGCD, out act);
		}

		if (OraclePvE.CanUse(out act))
		{
			return true;
		}

		// "Prioritize Microcosmos over all other healing when available" holds the heals below while Macrocosmos runs.
		// Oracle above and the potion are no healing: held with them, Oracle slipped out of Divination and the potion lost
		// its window, which closes once Divination is pressed (A404).
		var healsHeld = MicroPrio && HasMacrocosmos;

		if (!healsHeld && nextGCD.IsTheSameTo(false, HeliosConjunctionPvE, AspectedHeliosPvE))
		{
			if (NeutralSectPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (!healsHeld && nextGCD.IsTheSameTo(false, HeliosConjunctionPvE, HeliosPvE))
		{
			if (PartyMembersForecastAverHP(true) < HoroscopeHeal && HoroscopePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (!healsHeld && SynastryPvE.CanUse(out act))
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

		if (!healsHeld && StellarNow && HasGiantDominance && StellarDetonationPvE.CanUse(out act))
		{
			return true;
		}

		return base.EmergencyAbility(nextGCD, out act);

		static bool CanCastSynastry(IBaseAction actionCheck, IBaseAction synastry, float synastryHp, IAction next)
			=> next.IsTheSameTo(false, actionCheck) &&
			   synastry.Target.Target == actionCheck.Target.Target &&
			   synastry.Target.Target.GetForecastHealthRatio() < synastryHp;
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

		// Astrologian special rule on the universal stretch (concept 08, "Die Abwehrsperren"): after
		// Macrocosmos or Collective Unconscious the rest waits until that effect runs out, unless the
		// party is in danger. Formerly the recast less a written-in 30 and 20 s, which matched no
		// effect; the durations now come from the effect texts (15 and 10 s).
		if (AreaDefenseStretched("Astrologian: Macrocosmos or Collective Unconscious still in effect", MacrocosmosPvE, CollectiveUnconsciousPvE))
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
			if (EssentialDignityPvE.Target.Target?.GetForecastHealthRatio(true) < EssentialDignityThird)
			{
				return true;
			}
		}

		if (EssentialDignityPvE.Cooldown.CurrentCharges == 2 && EssentialDignityPvE.CanUse(out act, usedUp: true))
		{
			if (EssentialDignityPvE.Target.Target?.GetForecastHealthRatio(true) < EssentialDignitySecond)
			{
				return true;
			}
		}

		// The first charges recharge behind each other, so spending one does not leave the tank without an
		// answer; the last one does, and only it waits for danger.
		if (EssentialDignityPvE.Cooldown.CurrentCharges == 1 && EssentialDignityPvE.CanUse(out act, usedUp: true))
		{
			if (EssentialDignityPvE.Target.Target?.GetForecastHealthRatio(true) < EssentialDignityLast
				&& (!EssentialDignityNeedsThreat || (EssentialDignityPvE.Target.Target?.IsUnderThreat() ?? false)))
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

		if (PartyMembersForecastAverHP(true) < HoroscopeHeal && HoroscopePvE_16558.CanUse(out act))
		{
			return true;
		}

		if (PartyMembersForecastAverHP(true) < HoroscopeHeal && HoroscopePvE.CanUse(out act))
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

		if (PartyMembersForecastAverHP(true) < LadyOfHeals && LadyOfCrownsPvE.CanUse(out act))
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

		if (TryChargedBarrierAtCap(CelestialIntersectionPvE, out act))
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

	protected override bool DefenseSingleGCD(out IAction? act)
	{
		// The same triggers hold the single-target barrier too (an Astrologian special rule), for a target that carries
		// their effect; it yields when the player or a tank is in the critical class.
		if ((NeutralSectPvE.CanUse(out _) || HasNeutralSect || IsLastAbility(false, NeutralSectPvE)) && AspectedBeneficPvE.CanUse(out act)
			&& !SingleDefenseStretched(AspectedBeneficPvE, "Astrologian: Macrocosmos or Collective Unconscious still in effect",
				MacrocosmosPvE, CollectiveUnconsciousPvE))
		{
			return true;
		}

		return base.DefenseSingleGCD(out act);
	}

	[RotationDesc(ActionID.MacrocosmosPvE)]
	protected override bool DefenseAreaGCD(out IAction? act)
	{
		// Astrologian special rule on the universal stretch (concept 08, "Die Abwehrsperren"): after
		// Macrocosmos or Collective Unconscious the rest waits until that effect runs out, unless the
		// party is in danger. Formerly the recast less a written-in 30 and 20 s, which matched no
		// effect; the durations now come from the effect texts (15 and 10 s).
		if (AreaDefenseStretched("Astrologian: Macrocosmos or Collective Unconscious still in effect", MacrocosmosPvE, CollectiveUnconsciousPvE))
		{
			return base.DefenseAreaGCD(out act);
		}

		if ((NeutralSectPvE.CanUse(out _) || HasNeutralSect || IsLastAbility(false, NeutralSectPvE)) && HeliosConjunctionPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!MultiHitRestrict || MultiHitAhead(MacrocosmosOnSeries, MacrocosmosBeforeDanger))
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

		// Essential Dignity is available only when it can go out now. Its last charge waits for a target in danger
		// (EssentialDignityNeedsThreat); held for it, the GCD heals left a low member out of danger with no heal at all,
		// where that option gives him the smaller heals (A403).
		var shouldUseEssentialDignity =
			(EssentialPrio2 == EssentialPrioStrategy.AnyCharges
				|| (EssentialPrio2 == EssentialPrioStrategy.CappedCharges
					&& EssentialDignityPvE.Cooldown.CurrentCharges == EssentialDignityPvE.Cooldown.MaxCharges))
			&& EssentialDignityPvE.CanUse(out _, usedUp: true)
			&& (EssentialDignityPvE.Cooldown.CurrentCharges > 1 || !EssentialDignityNeedsThreat
				|| (EssentialDignityPvE.Target.Target?.IsUnderThreat() ?? false));

		if (shouldUseEssentialDignity)
		{
			return base.HealSingleGCD(out act);
		}

		if (AspectedBeneficPvE.CanUse(out act))
		{
			if (IsMoving || AspectedBeneficPvE.Target.Target?.GetForecastHealthRatio() < AspectedBeneficHeal)
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

		// Into a learned series also outside the area-defence flag: most series are instant attacks without a cast, and
		// every further hit is compiled (concept 08, A468).
		if (MultiHitRestrict && MacrocosmosOnSeries && HitSeriesHitsLeft > 0 && MacrocosmosPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
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

		// No spell with a cast time can start: Combust again before it is due, where that pays per MP at least as well as
		// the filler (concept 03, "C6", A603).
		var combust = CombustIiiPvE.EnoughLevel ? CombustIiiPvE : CombustIiPvE.EnoughLevel ? CombustIiPvE : CombustPvE;
		var malefic = FallMaleficPvE.EnoughLevel ? FallMaleficPvE : MaleficIvPvE.EnoughLevel ? MaleficIvPvE
			: MaleficIiiPvE.EnoughLevel ? MaleficIiiPvE : MaleficIiPvE.EnoughLevel ? MaleficIiPvE : MaleficPvE;
		if (CastStartBlocked && combust.CanUse(out act, skipStatusProvideCheck: true)
			&& DotWorthWhileCastBlocked(combust, malefic, combust.Target.Target))
		{
			return true;
		}

		return base.GeneralGCD(out act);
	}
	#endregion

	#region Extra Methods
	#endregion
}
