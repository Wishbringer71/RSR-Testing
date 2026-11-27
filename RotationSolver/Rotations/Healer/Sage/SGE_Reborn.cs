namespace RotationSolver.Rotations.Healer.Sage;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.58", RotationType = RotationType.Reborn)]
[SourceCode(Path = "main/Rotations/Healer/Sage/SGE_Reborn.cs")]

public sealed partial class SGE_Reborn : SageRotation
{
	#region Config Options
	[RotationConfig(CombatType.PvE, Name = "Use Eukrasia Action to heal")]
	public bool EukrasiaActionHeal { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Attempt to prevent bricking by allowing E.Prog at the end of GCD logic (experimental)")]
	public bool AntiBrick { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Eukrasia when out of combat")]
	public bool OOCEukrasia { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Rhizomata when out of combat")]
	public bool OOCRhizomata { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Limit Panhaima to multihit party stacks")]
	public bool MultiHitRestrict { get; set; } = false;

	// The bell's two cases for every healer with such a limit (concept 08, "Glockenspiel", A468).
	[RotationConfig(CombatType.PvE, Name = "Also before a hit that would put a party member in danger", Parent = nameof(MultiHitRestrict),
		Tooltip = "In a fight: Panhaima also goes before an announced area hit that would leave an unprotected "
			+ "member in danger; its barrier takes part of the hit.")]
	public bool PanhaimaBeforeDanger { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Also into an enemy attack that hits in a series", Parent = nameof(MultiHitRestrict),
		Tooltip = "In a fight: Panhaima also goes into an attack learned to hit the party several times in a "
			+ "row; its barrier forms anew after each hit.")]
	public bool PanhaimaOnSeries { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use GCDs to heal. (Ignored if you are the only healer in party, or while a party member is about to fall)",
		Tooltip = "In a fight: on, your GCD heals follow the healing thresholds. Off, they are left to a living "
			+ "second healer while you deal damage.\nA member about to fall still gets your GCD heal either way.")]
	public bool GCDHeal { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Enable Swiftcast Restriction Logic to attempt to prevent actions other than Raise when you have swiftcast")]
	public bool SwiftLogic { get; set; } = true;

	// The owner's rule, first built for Benediction (BenedictionNeedsThreat): an emergency measure only
	// where there is danger, otherwise a HoT and the smaller heals are enough. Every healer carries one,
	// so the rule reads the same shared check (ObjectHelper.IsUnderThreat) on each.
	[RotationConfig(CombatType.PvE, Name = "Taurochole as a heal only on a target in danger",
		Tooltip = "In a fight: Taurochole heals only a member who is attacked, cast at, under an announced area "
			+ "cast or measurably falling, so the tank keeps it.\nOff: the health threshold alone decides.")]
	public bool TaurocholeNeedsThreat { get; set; } = true;

	// The owner's rule for shield healers (concept 15, V1): the same pull upkeep as the white mage's
	// Regen, with a barrier, renewed when it runs out or is used up, instant so it can be cast while
	// running. On by default: the owner asked for exactly this (27.09.2026, "erneuerung bei ablauf/verfall
	// solange walltowall läuft", and MP-costing shields are fine as long as they are instant). The renewal
	// on every break is what 5755ad5b removed on 05.09.2026 as a barrier kept up without a request; his rule
	// is that request (A251).
	[RotationConfig(CombatType.PvE, Name = "Keep Eukrasian Diagnosis on the tank through a pull",
		Tooltip = "In a fight: in dungeons, Eukrasian Diagnosis goes on the tank as he closes in and is renewed "
			+ "whenever its barrier breaks, at the cost of Dosis GCDs.\nIt stops while your MP would not cover "
			+ "Egeiro.")]
	public bool UsePreEukrasianDiagnosis { get; set; } = true;

	[Range(1, 8, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies near the tank before the pull", Parent = nameof(UsePreEukrasianDiagnosis),
		Tooltip = "In a fight: Eukrasian Diagnosis goes on the tank as he closes in only when at least this many "
			+ "enemies stand within gap-closer range of him.\nLower: also for a single stray. Higher: small pulls "
			+ "start without it.")]
	public int PreEukrasianDiagnosisMinHostiles { get; set; } = 2;

	[Range(1, 12, ConfigUnitType.None, 1)]
	[RotationConfig(CombatType.PvE, Name = "Enemies still around the tank during the pull", Parent = nameof(UsePreEukrasianDiagnosis),
		Tooltip = "In a fight: Eukrasian Diagnosis on the tank is renewed during a pull only while at least this "
			+ "many enemies remain around him.\nLower: upkeep to the end of the pull. Higher: it stops early, "
			+ "freeing GCDs for damage.")]
	public int PreEukrasianDiagnosisMinWallToWallHostiles { get; set; } = 3;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold party member needs to be to use Taurochole")]
	public float TaurocholeHeal { get; set; } = 0.8f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold party member needs to be to use Soteria")]
	public float SoteriaHeal { get; set; } = 0.85f;

	[RotationConfig(CombatType.PvE, Name = "Use Kerachole as a heal when applicable")]
	public bool KeracholePvEHealOption { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Holos as a heal when applicable")]
	public bool HolosHealOption { get; set; } = true;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Average health threshold party members need to be to use Holos", Parent = nameof(HolosHealOption))]
	public float HolosHeal { get; set; } = 0.5f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold tank party member needs to use an OGCD Heal on Tanks while not holding addersgal stacks")]
	public float OGCDTankHeal { get; set; } = 0.65f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold party member needs to be to use Krasis")]
	public float KrasisHeal { get; set; } = 0.3f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold tank party member needs to use Krasis")]
	public float KrasisTankHeal { get; set; } = 0.7f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold party member needs to be to use Pneuma as a ST heal")]
	public float PneumaSTPartyHeal { get; set; } = 0.2f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold tank party member needs to use Pneuma as a ST heal")]
	public float PneumaSTTankHeal { get; set; } = 0.6f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Average health threshold party members need to be to use Pneuma as an AOE heal")]
	public float PneumaAOEPartyHeal { get; set; } = 0.65f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Health threshold tank party member needs to use Pneuma as an AOE heal")]
	public float PneumaAOETankHeal { get; set; } = 0.6f;

	[RotationConfig(CombatType.PvE, Name = "Which opener to use")]
	public OpenerStrategy OpenerSelection { get; set; } = OpenerStrategy.PneumaOpener;

	public enum OpenerStrategy : byte
	{
		[Description("Use Toxikon prepull opener")]
		ToxikonOpener,

		[Description("Use Pneuma prepull opener")]
		PneumaOpener,
	}

	#endregion

	#region Tracking Properties
	private IBaseAction? _lastEukrasiaActionAim = null;
	public override void DisplayRotationStatus()
	{
		ImGui.Text($"Last E.Action Aim Cleared From Queue: {_lastEukrasiaActionAim}");
		ImGui.Text($"Current E.Action Aim: {_EukrasiaActionAim}");
	}
	#endregion

	#region Countdown Logic
	protected override IAction? CountDownAction(float remainTime)
	{
		if (OpenerSelection == OpenerStrategy.PneumaOpener && remainTime < PneumaPvE.Info.CastTime + CountDownAhead
			&& PneumaPvE.CanUse(out var act))
		{
			return act;
		}

		if (remainTime <= 2.1f && UseBurstMedicine(out act))
		{
			return act;
		}

		if (OpenerSelection == OpenerStrategy.ToxikonOpener && remainTime < 1.5f + CountDownAhead
			&& ToxikonIiPvE.CanUse(out act))
		{
			return act;
		}

		if (remainTime <= 5 && EukrasiaPvE.CanUse(out act))
		{
			return act;
		}

		return base.CountDownAction(remainTime);
	}
	#endregion

	protected override void OnFrameUpdate()
	{
		if (IsInCountdown)
		{
			return;
		}

		if (IsLastGCD(false, EukrasianPrognosisIiPvE, EukrasianPrognosisPvE,
			EukrasianDiagnosisPvE, EukrasianDyskrasiaPvE, EukrasianDosisIiiPvE, EukrasianDosisIiPvE,
			EukrasianDosisPvE) || !InCombat)
		{
			ClearEukrasia();
		}

		ChoiceEukrasia();

		if (HasEukrasia && InCombat && HasHostilesInMaxRange && NextGCDAction == null && SecondsSinceLastNextGCDChange >= 2f)
		{
			_lastEukrasiaActionAim = _EukrasiaActionAim;
			_EukrasiaActionAim = null;
			_eukrasiaDiagnosisForTank = false;
			_eukrasiaDotWhileCastBlocked = false;
			StatusHelper.StatusOff(StatusID.Eukrasia);
		}
	}

	#region oGCD Logic
	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		if (ZoePvE.EnoughLevel && !ZoePvE.Cooldown.IsCoolingDown)
		{
			if (nextGCD.IsTheSameTo(false, PneumaPvE))
			{
				if (ZoePvE.CanUse(out act))
				{
					return true;
				}
			}

			if (nextGCD.IsTheSameTo(false, EukrasiaPvE))
			{
				if (_EukrasiaActionAim == EukrasianPrognosisPvE
					|| _EukrasiaActionAim == EukrasianPrognosisIiPvE
					|| _EukrasiaActionAim == EukrasianDiagnosisPvE)
				{
					if (ZoePvE.CanUse(out act))
					{
						return true;
					}
				}
			}
		}

		return base.EmergencyAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.PsychePvE)]
	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		if (PsychePvE.CanUse(out act))
		{
			return true;
		}
		return base.AttackAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.PanhaimaPvE, ActionID.KeracholePvE, ActionID.HolosPvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (!PanhaimaPvE.Cooldown.IsCoolingDown)
		{
			if (PhysisIiPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (Addersgall <= 1)
		{
			if (!MultiHitRestrict || MultiHitAhead(PanhaimaOnSeries, PanhaimaBeforeDanger))
			{
				if (PanhaimaPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		if (KeracholePvE.CanUse(out act))
		{
			return true;
		}

		if (HolosPvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.HaimaPvE, ActionID.TaurocholePvE, ActionID.PanhaimaPvE, ActionID.KeracholePvE, ActionID.HolosPvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (Addersgall <= 1)
		{
			if (HaimaPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (TaurocholePvE.CanUse(out act) && TaurocholePvE.Target.Target.GetForecastHealthRatio(true) < TaurocholeHeal)
		{
			return true;
		}

		if (Addersgall <= 1)
		{
			if ((!HaimaPvE.EnoughLevel || HaimaPvE.Cooldown.ElapsedAfter(20)) && PanhaimaPvE.CanUse(out act))
			{
				return true;
			}
		}

		if ((!TaurocholePvE.EnoughLevel || TaurocholePvE.Cooldown.ElapsedAfter(20)) && KeracholePvE.CanUse(out act))
		{
			return true;
		}

		if (HolosPvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.KeracholePvE, ActionID.PhysisPvE, ActionID.HolosPvE, ActionID.IxocholePvE)]
	protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (!MergedStatus.HasFlag(AutoStatus.DefenseArea) && !MergedStatus.HasFlag(AutoStatus.DefenseSingle) && PepsisPvE.CanUse(out act))
		{
			return true;
		}

		if (KeracholePvE.CanUse(out act) && EnhancedKeracholeTrait.EnoughLevel && KeracholePvEHealOption)
		{
			return true;
		}

		if (IxocholePvE.CanUse(out act))
		{
			return true;
		}

		if (PhysisIiPvE.CanUse(out act))
		{
			return true;
		}

		if (!PhysisIiPvE.EnoughLevel && PhysisPvE.CanUse(out act))
		{
			return true;
		}

		if (HolosPvE.CanUse(out act) && PartyMembersForecastAverHP(true) < HolosHeal && HolosHealOption)
		{
			return true;
		}

		return base.HealAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.TaurocholePvE, ActionID.DruocholePvE, ActionID.HolosPvE, ActionID.PhysisPvE, ActionID.PanhaimaPvE)]
	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{
		var tankEnum = PartyMembers.GetJobCategory(JobRole.Tank);
		List<IBattleChara> tank = [.. tankEnum];

		if (nextGCD.IsTheSameTo(false, PneumaPvE, EukrasianDiagnosisPvE, DiagnosisPvE, PrognosisPvE))
		{
			for (var i = 0; i < tank.Count; i++)
			{
				var t = tank[i];
				if (t.GetForecastHealthRatio() < KrasisTankHeal)
				{
					if (KrasisPvE.CanUse(out act))
					{
						return true;
					}
				}
			}

			foreach (var member in PartyMembers)
			{
				if (member.GetForecastHealthRatio() < KrasisHeal)
				{
					if (KrasisPvE.CanUse(out act))
					{
						return true;
					}
				}
			}
		}

		if (nextGCD.IsTheSameTo(false, PneumaPvE, EukrasianPrognosisPvE, EukrasianPrognosisIiPvE, DiagnosisPvE, PrognosisPvE))
		{
			if (PhilosophiaPvE.CanUse(out act))
			{
				return true;
			}
		}

		var taurocholeHeld = false;
		if (TaurocholePvE.CanUse(out act))
		{
			if (!TaurocholeNeedsThreat || TaurocholePvE.Target.Target.IsUnderThreat())
			{
				return true;
			}

			taurocholeHeld = true;
		}

		// Druochole waits for Taurochole while that can go. Held for want of danger, Taurochole kept Druochole back too, and
		// the target got no heal from the sage where "Taurochole as a heal only on a target in danger" gives him the
		// smaller heals (A405).
		if ((!TaurocholePvE.EnoughLevel || TaurocholePvE.Cooldown.IsCoolingDown || taurocholeHeld) && DruocholePvE.CanUse(out act))
		{
			return true;
		}

		foreach (var member in PartyMembers)
		{
			if (SoteriaPvE.CanUse(out act) && member.HasStatus(true, StatusID.Kardion) && member.GetForecastHealthRatio() < SoteriaHeal)
			{
				return true;
			}
		}

		for (var i = 0; i < tank.Count; i++)
		{
			var t = tank[i];
			if (Addersgall < 1 && t.GetForecastHealthRatio(true) < OGCDTankHeal)
			{
				if (HaimaPvE.CanUse(out act))
				{
					return true;
				}

				if (PhysisIiPvE.CanUse(out act))
				{
					return true;
				}

				if (!PhysisIiPvE.EnoughLevel && PhysisPvE.CanUse(out act))
				{
					return true;
				}

				if (HolosPvE.CanUse(out act) && HolosHealOption)
				{
					return true;
				}

				if ((!HaimaPvE.EnoughLevel || HaimaPvE.Cooldown.ElapsedAfter(20)) && PanhaimaPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		return base.HealSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.KardiaPvE, ActionID.RhizomataPvE, ActionID.SoteriaPvE)]
	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		// Into a learned series also outside the area-defence flag, where Kerachole is not asked either: most series are
		// instant attacks without a cast, and the barrier forms anew after each hit (concept 08, A468).
		if (MultiHitRestrict && PanhaimaOnSeries && HitSeriesHitsLeft > 0 && PanhaimaPvE.CanUse(out act))
		{
			return true;
		}

		// A fourth stack is lost at three. Spending one just before the next arrives keeps three for
		// the emergencies once it has, and Druochole returns 7% MP whether or not anyone needs the
		// heal - The Balance: "use them liberally even if not necessarily needed" (A203). On whoever
		// the heal finds, else on the sage.
		if (InCombat && Addersgall >= 3 && AddersgallEndAfterGCD(1)
			&& (DruocholePvE.CanUse(out act) || DruocholePvE.CanUse(out act, targetOverride: TargetType.Self)))
		{
			return true;
		}

		if (InCombat || (!InCombat && !HasKardia))
		{
			if (KardiaPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (OOCRhizomata && !InCombat && Addersgall <= 1 && RhizomataPvE.CanUse(out act))
		{
			return true;
		}

		if (InCombat && Addersgall <= 1 && RhizomataPvE.CanUse(out act))
		{
			return true;
		}

		var found = false;
		foreach (var b in PartyMembers)
		{
			if (b.HasStatus(true, StatusID.Kardion) && b.GetForecastHealthRatio() < HealthSingleAbility)
			{
				found = true;
				break;
			}
		}

		if (SoteriaPvE.CanUse(out act) && found)
		{
			return true;
		}

		if (HasBuffs && UseBurstMedicine(out act))
		{
			return true;
		}

		return base.GeneralAbility(nextGCD, out act);
	}
	#endregion

	#region Eukrasia Logic
	private IBaseAction? _EukrasiaActionAim = null;

	// Eukrasian Diagnosis's own list, widened by the barrier its effect text says it cannot stack with.
	private static readonly StatusID[] TankBarrierHeld = [StatusID.EukrasianDiagnosis, StatusID.Galvanize, StatusID.EukrasianPrognosis];

	// Sets the target Eukrasia action to be performed next.
	// If the action is null, it exits early.
	// If the current action aim is not null and the last action matches certain conditions, it exits early.
	// Finally, updates the current Eukrasia action aim if it's different from the incoming action.
	private void SetEukrasia(IBaseAction act)
	{
		if (act == null || (_EukrasiaActionAim != null && IsLastGCD(true, _EukrasiaActionAim)))
		{
			return;
		}

		_EukrasiaActionAim = act;
		_eukrasiaDotWhileCastBlocked = false;
	}

	// Clears the Eukrasia action aim, effectively resetting any planned Eukrasia action.
	private void ClearEukrasia()
	{
		if (_EukrasiaActionAim != null)
		{
			_lastEukrasiaActionAim = _EukrasiaActionAim;
			_EukrasiaActionAim = null;
			_eukrasiaDiagnosisForTank = false;
			_eukrasiaDotWhileCastBlocked = false;
			if (HasEukrasia && !InCombat && !HasHostilesInMaxRange)
			{
				StatusHelper.StatusOff(StatusID.Eukrasia);
			}
		}
	}

	private void ChoiceEukrasia()
	{
		// Only decide the aim; do not require Eukrasia to be currently usable here.
		// Attempts to set correct Eukrasia action based on availability and MergedStatus.
		if (EukrasianPrognosisIiPvE.EnoughLevel && EukrasianPrognosisIiPvE.IsEnabled && MergedStatus.HasFlag(AutoStatus.DefenseArea)
			&& EukrasianPrognosisIiPvE.CanUse(out _))
		{
			SetEukrasia(EukrasianPrognosisIiPvE);
		}
		else if (!EukrasianPrognosisIiPvE.EnoughLevel && EukrasianPrognosisPvE.EnoughLevel && EukrasianPrognosisPvE.IsEnabled && MergedStatus.HasFlag(AutoStatus.DefenseArea)
			&& EukrasianPrognosisPvE.CanUse(out _))
		{
			SetEukrasia(EukrasianPrognosisPvE);
		}
		else if (EukrasianDiagnosisPvE.EnoughLevel && EukrasianDiagnosisPvE.IsEnabled && MergedStatus.HasFlag(AutoStatus.DefenseSingle)
			&& EukrasianDiagnosisPvE.CanUse(out _))
		{
			SetEukrasia(EukrasianDiagnosisPvE);
			_eukrasiaDiagnosisForTank = false;
		}
		// After the defence shields, ahead of the damage-over-time: in a pull the tank's damage is certain.
		else if (EukrasianDiagnosisPvE.EnoughLevel && EukrasianDiagnosisPvE.IsEnabled && PullBarrierDue())
		{
			if (_EukrasiaActionAim != EukrasianDiagnosisPvE)
			{
				SetEukrasia(EukrasianDiagnosisPvE);
				_eukrasiaDiagnosisForTank = _EukrasiaActionAim == EukrasianDiagnosisPvE;
			}
		}
		else if (EukrasianDyskrasiaPvE.EnoughLevel && EukrasianDyskrasiaPvE.IsEnabled && (!MergedStatus.HasFlag(AutoStatus.DefenseSingle) && !MergedStatus.HasFlag(AutoStatus.DefenseArea))
			&& EukrasianDyskrasiaPvE.CanUse(out _))
		{
			SetEukrasia(EukrasianDyskrasiaPvE);
		}
		// No spell with a cast time can start: the dosis before it is due, where that pays (concept 03, "C6", A603).
		else if (!MergedStatus.HasFlag(AutoStatus.DefenseSingle) && !MergedStatus.HasFlag(AutoStatus.DefenseArea)
			&& DosisWhileCastBlocked())
		{
			SetEukrasia(EukrasianDosisAtLevel);
			_eukrasiaDotWhileCastBlocked = _EukrasiaActionAim == EukrasianDosisAtLevel;
		}
		else if ((!EukrasianDyskrasiaPvE.CanUse(out _) || !DyskrasiaPvE.CanUse(out _))
			&& EukrasianDosisIiiPvE.CanUse(out _) && EukrasianDosisIiiPvE.EnoughLevel && (!MergedStatus.HasFlag(AutoStatus.DefenseSingle) && !MergedStatus.HasFlag(AutoStatus.DefenseArea)) && EukrasianDosisIiiPvE.IsEnabled)
		{
			SetEukrasia(EukrasianDosisIiiPvE);
		}
		else if ((!EukrasianDyskrasiaPvE.CanUse(out _) || !DyskrasiaPvE.CanUse(out _))
			&& EukrasianDosisIiPvE.CanUse(out _) && !EukrasianDosisIiiPvE.EnoughLevel && EukrasianDosisIiPvE.EnoughLevel && (!MergedStatus.HasFlag(AutoStatus.DefenseSingle) && !MergedStatus.HasFlag(AutoStatus.DefenseArea)) && EukrasianDosisIiPvE.IsEnabled)
		{
			SetEukrasia(EukrasianDosisIiPvE);
		}
		else if ((!EukrasianDyskrasiaPvE.CanUse(out _) || !DyskrasiaPvE.CanUse(out _))
			&& EukrasianDosisPvE.CanUse(out _) && !EukrasianDosisIiPvE.EnoughLevel && EukrasianDosisPvE.EnoughLevel && (!MergedStatus.HasFlag(AutoStatus.DefenseSingle) && !MergedStatus.HasFlag(AutoStatus.DefenseArea)) && EukrasianDosisPvE.IsEnabled)
		{
			SetEukrasia(EukrasianDosisPvE);
		}
	}
	#endregion

	#region Eukrasia Execution
	// Attempts to perform a Eukrasia action, based on the current game state and conditions.
	private bool DoEukrasianPrognosisIi(out IAction? act)
	{
		act = null;

		if (_EukrasiaActionAim != null &&
			_EukrasiaActionAim == EukrasianPrognosisIiPvE)
		{
			// If we don't have Eukrasia, press it first to enable the Eukrasian spell.
			if (!HasEukrasia)
			{
				if (EukrasiaPvE.CanUse(out act))
				{
					return true;
				}
				return false;
			}

			if (EukrasianPrognosisIiPvE.CanUse(out act))
			{
				return true;
			}
		}
		return false;
	}

	// Attempts to perform a Eukrasia action, based on the current game state and conditions.
	private bool DoEukrasianPrognosis(out IAction? act)
	{
		act = null;

		if (_EukrasiaActionAim != null &&
			_EukrasiaActionAim == EukrasianPrognosisPvE)
		{
			// If we don't have Eukrasia, press it first to enable the Eukrasian spell.
			if (!HasEukrasia)
			{
				if (EukrasiaPvE.CanUse(out act))
				{
					return true;
				}
				return false;
			}

			if (EukrasianPrognosisPvE.CanUse(out act))
			{
				return true;
			}
		}
		return false;
	}

	// Attempts to perform a Eukrasia action, based on the current game state and conditions.
	private bool DoEukrasianDiagnosis(out IAction? act)
	{
		act = null;

		if (_EukrasiaActionAim != null && _EukrasiaActionAim == EukrasianDiagnosisPvE)
		{
			if (!HasEukrasia)
			{
				if (EukrasiaPvE.CanUse(out act))
				{
					return true;
				}
				return false;
			}

			if (EukrasianDiagnosisPvE.CanUse(out act, targetOverride: _eukrasiaDiagnosisForTank ? TargetType.Tank : default))
			{
				return true;
			}
		}
		return false;
	}

	// Attempts to perform a Eukrasia action, based on the current game state and conditions.
	private bool DoEukrasianDyskrasia(out IAction? act)
	{
		act = null;

		if (_EukrasiaActionAim != null && _EukrasiaActionAim == EukrasianDyskrasiaPvE)
		{
			if (!HasEukrasia)
			{
				if (EukrasiaPvE.CanUse(out act))
				{
					return true;
				}
				return false;
			}

			if (EukrasianDyskrasiaPvE.CanUse(out act))
			{
				return true;
			}
		}
		return false;
	}

	// Attempts to perform a Eukrasia action, based on the current game state and conditions.
	private bool DoEukrasianDosisIii(out IAction? act)
	{
		act = null;

		if (_EukrasiaActionAim != null &&
			_EukrasiaActionAim == EukrasianDosisIiiPvE)
		{
			if (!HasEukrasia)
			{
				if (EukrasiaPvE.CanUse(out act))
				{
					return true;
				}
				return false;
			}

			if (EukrasianDosisIiiPvE.CanUse(out act, skipStatusProvideCheck: _eukrasiaDotWhileCastBlocked))
			{
				return true;
			}
		}
		return false;
	}

	private bool DoEukrasianDosisIi(out IAction? act)
	{
		act = null;

		if (_EukrasiaActionAim != null &&
			_EukrasiaActionAim == EukrasianDosisIiPvE)
		{
			if (!HasEukrasia)
			{
				if (EukrasiaPvE.CanUse(out act))
				{
					return true;
				}
				return false;
			}

			if (EukrasianDosisIiPvE.CanUse(out act, skipStatusProvideCheck: _eukrasiaDotWhileCastBlocked))
			{
				return true;
			}
		}
		return false;
	}

	private bool DoEukrasianDosis(out IAction? act)
	{
		act = null;

		if (_EukrasiaActionAim != null &&
			_EukrasiaActionAim == EukrasianDosisPvE)
		{
			if (!HasEukrasia)
			{
				if (EukrasiaPvE.CanUse(out act))
				{
					return true;
				}
				return false;
			}

			if (EukrasianDosisPvE.CanUse(out act, skipStatusProvideCheck: _eukrasiaDotWhileCastBlocked))
			{
				return true;
			}
		}
		return false;
	}
	#endregion

	#region GCD Logic 
	[RotationDesc(ActionID.PneumaPvE, ActionID.PrognosisPvE, ActionID.EukrasianPrognosisPvE, ActionID.EukrasianPrognosisIiPvE)]
	protected override bool HealAreaGCD(out IAction? act)
	{
		if (SwiftRaisePending)
		{
			return base.HealAreaGCD(out act);
		}

		var tankBelowThreshold = false;
		var tanks = PartyMembers.GetJobCategory(JobRole.Tank);
		foreach (var t in tanks)
		{
			if (t.GetForecastHealthRatio() < PneumaAOETankHeal)
			{
				tankBelowThreshold = true;
				break;
			}
		}

		if (PartyMembersForecastAverHP() < PneumaAOEPartyHeal || (DyskrasiaPvE.CanUse(out _) && tankBelowThreshold))
		{
			if (PneumaPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (HasEukrasia && EukrasiaActionHeal && EukrasianPrognosisPvE.CanUse(out act))
		{
			return true;
		}

		if (EukrasiaPvE.EnoughLevel && !HasEukrasia && EukrasiaActionHeal && EukrasiaPvE.CanUse(out act))
		{
			return true;
		}

		if (_EukrasiaActionAim == null && PrognosisPvE.CanUse(out act))
		{
			return true;
		}

		return base.HealAreaGCD(out act);
	}

	[RotationDesc(ActionID.DiagnosisPvE, ActionID.EukrasianDiagnosisPvE)]
	protected override bool HealSingleGCD(out IAction? act)
	{
		if (SwiftRaisePending)
		{
			return base.HealSingleGCD(out act);
		}

		if (HasEukrasia && EukrasiaActionHeal && EukrasianDiagnosisPvE.CanUse(out act))
		{
			return true;
		}

		if (EukrasiaPvE.EnoughLevel && !HasEukrasia && EukrasiaActionHeal && EukrasiaPvE.CanUse(out act))
		{
			return true;
		}

		if (_EukrasiaActionAim == null && DiagnosisPvE.CanUse(out act))
		{
			return true;
		}

		return base.HealSingleGCD(out act);
	}

	[RotationDesc(ActionID.EgeiroPvE)]
	protected override bool RaiseGCD(out IAction? act)
	{
		if (EgeiroPvE.CanUse(out act))
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

		if (DoEukrasianPrognosisIi(out act))
		{
			return true;
		}

		if (DoEukrasianPrognosis(out act))
		{
			return true;
		}

		if (DoEukrasianDiagnosis(out act))
		{
			return true;
		}

		if (PhlegmaPvE.CanUse(out act, usedUp: IsMoving || PhlegmaPvE.Cooldown.WillHaveXChargesGCD(2, 1)))
		{
			return true;
		}

		foreach (var member in PartyMembers)
		{
			if (member.GetForecastHealthRatio() < PneumaSTPartyHeal && !member.IsDead)
			{
				if (PneumaPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		var tanks = PartyMembers.GetJobCategory(JobRole.Tank);
		foreach (var tank in tanks)
		{
			if (tank.GetForecastHealthRatio() < PneumaSTTankHeal && !tank.IsDead)
			{
				if (PneumaPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		if (IsMoving && ToxikonPvE.CanUse(out act))
		{
			return true;
		}

		if (DoEukrasianDyskrasia(out act))
		{
			return true;
		}

		if (DyskrasiaPvE.CanUse(out act))
		{
			return true;
		}

		if (DoEukrasianDosisIii(out act))
		{
			return true;
		}

		if (DoEukrasianDosisIi(out act))
		{
			return true;
		}

		if (DoEukrasianDosis(out act))
		{
			return true;
		}

		if (DosisPvE.CanUse(out act))
		{
			return true;
		}

		if (OOCEukrasia && !InCombat && !HasEukrasia && EukrasiaPvE.CanUse(out act))
		{
			return true;
		}

		if (InCombat && !HasHostilesInRange && EukrasiaPvE.CanUse(out act))
		{
			return true;
		}

		// fallback
		if (AntiBrick && InCombat && HasHostilesInRange && HasEukrasia)
		{
			if (EukrasianPrognosisPvE.CanUse(out act, skipStatusProvideCheck: true))
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