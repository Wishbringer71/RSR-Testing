namespace RotationSolver.RebornRotations.Magical;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.57")]
[SourceCode(Path = "main/RebornRotations/Limited Jobs/BLU_Reborn.cs")]

public sealed class BLU_Reborn : BlueMageRotation
{
	[RotationConfig(CombatType.PvE, Name = "Use Basic Instinct")]
	public bool UseBasicInstinct { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Mighty Guard")]
	public bool UseMightyGuard { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Spam Gobskin, keeping its status active")]
	public bool GobskinSpam { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Transfusion to heal")]
	public bool UseTransfusion { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Snort to interrupt")]
	public bool UseSnort { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Bad Breath for AOE mitigation")]
	public bool UseBadBreath { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Low Chance abilities")]
	public bool LowChance { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Moon Flute only when it is safe: not as tank or healer, no member in danger, no raidwide or tankbuster on you announced within its window",
		Tooltip = "In a fight: Moon Flute goes when the burst it lifts gains more than Waning Nocturne costs - Waning allows no action at all, no heal, no mitigation, no raise. On: it also waits while you are under tank or healer mimicry, while a party member is in danger or an announced area cast would put one there, while your own health, carried forward over the window, would fall that low, and while BossModReborn announces a raidwide or a tankbuster on you within it. Without BossModReborn's timeline only what is cast or measured counts. Off: only the value decides.")]
	public bool MoonFluteOnlyWhenSafe { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Improve the Blue Mage set from your learned spells",
		Tooltip = "In a fight: RSR casts only spells in your active set, so with this on more of your learned spells come into play. Out of combat, without a target: RSR adds Sonic Boom where it is missing, the partners of the spell chains one part of which is in your set, and damage spells with a recast from recast groups your set lacks. It takes only free slots and fillers it does not cast at your level - damage cooldowns, heals, defence, movement, mimicry, raises, interrupts and every spell with an additional effect stay, and a slot is changed only while its spell is ready. Once you change your set yourself, or inside the Masked Carnivale, RSR leaves it alone for the rest of the session. Your saved sets and hotbars stay as they are. Off: your set stays as you made it.")]
	public bool ImproveBlueMageSet { get; set; } = true;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP percent allowed for the use of Devour as a heal")]
	public float DevourHealThreshold { get; set; } = 0.6f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Minimum HP percent allowed for the use of Missile")]
	public float TheMissileKnowsWhereItIs { get; set; } = 0.6f;

	[Range(0, 10000, ConfigUnitType.None, 100)]
	[RotationConfig(CombatType.PvE, Name = "MP needed to use Blood Drain/Divination Rune for MP gain")]

	public float MPGainNeed { get; set; } = 5000;

	/// <inheritdoc/>
	// A576: out of combat whether RSR is on or not - before, only in the 30 s after a fight until RSR turned itself off.
	protected override void UpdateServices()
	{
		base.UpdateServices();
		if (ImproveBlueMageSet)
		{
			ImproveActiveSet();
		}
	}

	#region Countdown logic

	protected override IAction? CountDownAction(float remainTime)
	{
		return base.CountDownAction(remainTime);
	}

	#endregion

	#region Emergency Logic

	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		if (IsLastAbility(false, SurpanakhaPvE) && SurpanakhaPvE.Cooldown.HasOneCharge && SurpanakhaPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		return base.EmergencyAbility(nextGCD, out act);
	}

	#endregion

	#region Move oGCD Logic

	protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
	{
		return base.MoveForwardAbility(nextGCD, out act);
	}

	protected override bool MoveBackAbility(IAction nextGCD, out IAction? act)
	{
		return base.MoveBackAbility(nextGCD, out act);
	}

	protected override bool SpeedAbility(IAction nextGCD, out IAction? act)
	{
		return base.SpeedAbility(nextGCD, out act);
	}

	#endregion

	#region Heal/Defense oGCD Logic

	protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
	{
		return base.HealAreaAbility(nextGCD, out act);
	}

	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{
		return base.HealSingleAbility(nextGCD, out act);
	}

	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		return base.DefenseAreaAbility(nextGCD, out act);
	}

	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		return base.DefenseSingleAbility(nextGCD, out act);
	}

	#endregion

	#region oGCD Logic

	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		if (IsLastAbility(true, SurpanakhaPvE) && SurpanakhaPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		// The burst waits for the Moon Flute coming within the next GCDs (concept 01, "Der Burst wartet", A507).
		if (BurstWaits)
		{
			return base.AttackAbility(nextGCD, out act);
		}

		if (SurpanakhaPvE.Cooldown.CurrentCharges == SurpanakhaPvE.Cooldown.MaxCharges && SurpanakhaPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		if (SeaShantyPvE.CanUse(out act))
		{
			return true;
		}

		if (PhantomFlurryPvE_23289.CanUse(out act) && StatusHelper.PlayerWillStatusEnd(1, true, StatusID.PhantomFlurry))
		{
			return true;
		}

		if (PhantomFlurryPvE.CanUse(out act))
		{
			return true;
		}

		if (FeatherRainPvE.CanUse(out act))
		{
			return true;
		}

		if (EruptionPvE.CanUse(out act))
		{
			return true;
		}

		if (MountainBusterPvE.CanUse(out act))
		{
			return true;
		}

		if (ShockStrikePvE.CanUse(out act))
		{
			return true;
		}

		if (GlassDancePvE.CanUse(out act))
		{
			return true;
		}

		if (VeilOfTheWhorlPvE.CanUse(out act))
		{
			return true;
		}

		if (QuasarPvE.CanUse(out act))
		{
			return true;
		}

		if (JKickPvE.CanUse(out act))
		{
			return true;
		}

		if (BothEndsPvE.CanUse(out act))
		{
			return true;
		}

		if (NightbloomPvE.CanUse(out act))
		{
			return true;
		}

		return base.AttackAbility(nextGCD, out act);
	}

	protected override bool InterruptAbility(IAction nextGCD, out IAction? act)
	{
		return base.InterruptAbility(nextGCD, out act);
	}

	protected override bool DispelAbility(IAction nextGCD, out IAction? act)
	{
		return base.DispelAbility(nextGCD, out act);
	}

	protected override bool AntiKnockbackAbility(IAction nextGCD, out IAction? act)
	{
		return base.AntiKnockbackAbility(nextGCD, out act);
	}

	protected override bool ProvokeAbility(IAction nextGCD, out IAction? act)
	{
		return base.ProvokeAbility(nextGCD, out act);
	}

	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{

		return base.GeneralAbility(nextGCD, out act);
	}

	#endregion

	#region GCD Logic

	protected override bool EmergencyGCD(IAction? nextGCD, out IAction? act)
	{
		if ((IsLastAbility(true, SurpanakhaPvE) || (SurpanakhaPvE.Cooldown.CurrentCharges == SurpanakhaPvE.Cooldown.MaxCharges && !BurstWaits))
			&& SurpanakhaPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		// Diamondback turns Waxing into Waning at once and roots the player for its duration (effect text): only when the
		// player's health, carried forward over Waning, would fall into the critical class (concept 01, "Diamantrücken", A507).
		if (WaxingNocturneWillEnd && PlayerFallsInWaning && DiamondbackPvE.CanUse(out act))
		{
			return true;
		}

		if (UseMightyGuard && (IsTank || HasBasicInstinct))
		{
			if (MightyGuardPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (UseBasicInstinct && BasicInstinctPvE.CanUse(out act))
		{
			return true;
		}

		if (EerieSoundwavePvE.CanUse(out act))
		{
			return true;
		}

		return base.EmergencyGCD(nextGCD, out act);
	}

	protected override bool ProvokeGCD(out IAction? act)
	{
		if (TheLookPvE.CanUse(out act, targetOverride: TargetType.Provoke, skipAoeCheck: true))
		{
			return true;
		}

		if (BadBreathPvE.CanUse(out act, targetOverride: TargetType.Provoke, skipAoeCheck: true))
		{
			return true;
		}

		if (StickyTonguePvE.CanUse(out act, targetOverride: TargetType.Provoke))
		{
			return true;
		}

		if (FrogLegsPvE.CanUse(out act))
		{
			return true;
		}

		return base.ProvokeGCD(out act);
	}

	protected override bool MyInterruptGCD(out IAction? act)
	{
		if (BombTossPvE.CanUse(out act, skipAoeCheck: true, targetOverride: TargetType.Interrupt))
		{
			return true;
		}

		if (FazePvE.CanUse(out act, skipAoeCheck: true, targetOverride: TargetType.Interrupt))
		{
			return true;
		}

		if (FlyingSardinePvE.CanUse(out act, targetOverride: TargetType.Interrupt))
		{
			return true;
		}

		if (UseSnort)
		{
			if (SnortPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (StickyTonguePvE.CanUse(out act, targetOverride: TargetType.Interrupt))
		{
			return true;
		}

		return base.MyInterruptGCD(out act);
	}

	protected override bool GeneralGCD(out IAction? act)
	{
		return UseUtilityAndDebuffs(out act)
			|| MaintainDoTs(out act)
			|| SpendCooldowns(out act)
			|| UseSituational(out act)
			|| UseFiller(out act)
			|| base.GeneralGCD(out act);
	}

	private bool UseUtilityAndDebuffs(out IAction? act)
	{
		if (IsTank && DivineCataractPvE.CanUse(out act))
		{
			return true;
		}

		if (GobskinSpam && GobskinPvE.CanUse(out act))
		{
			return true;
		}

		if (ChirpPvE.CanUse(out act))
		{
			return true;
		}

		if (LowChance)
		{
			if (DoomPvE.CanUse(out act))
			{
				return true;
			}

			if (Level5DeathPvE.CanUse(out act))
			{
				return true;
			}

			if (TailScrewPvE.CanUse(out act))
			{
				return true;
			}

			// Petrification opens Drill Cannons at 600 and Ultravibration; the effect text names a low chance and a level
			// that must be a multiple of five, which the target check reads (concept 01, Synergien, A508).
			if (Level5PetrifyPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (CondensedLibraPvE.CanUse(out act))
		{
			return true;
		}

		if (OffguardPvE.CanUse(out act))
		{
			return true;
		}

		if (PeculiarLightPvE.CanUse(out act))
		{
			return true;
		}

		// The Moon Flute window by value and safety; Whistle and Tingle go before it when Triple Trident falls into Waxing, so
		// they take no Waxing GCD (concept 01, "Mondflöte", A507).
		if (MoonFluteComing)
		{
			if (TridentInWaxing && WhistlePvE.CanUse(out act))
			{
				return true;
			}

			if (TridentInWaxing && TinglePvE.CanUse(out act))
			{
				return true;
			}

			if (MoonFlutePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (UltravibrationPvE.CanUse(out act))
		{
			return true;
		}

		act = null;
		return false;
	}

	private bool MaintainDoTs(out IAction? act)
	{
		if (SongOfTormentPvE.Info.IsOnSlot && SongOfTormentPvE.CanUse(out _, skipStatusProvideCheck: true) && SongOfTormentPvE.Target.Target != null && (SongOfTormentPvE.Target.Target?.WillStatusEndGCD(3, 0, true, SongOfTormentPvE.Setting.TargetStatusProvide ?? []) ?? false))
		{
			// Bristle lifts the next spell, Song of Torment's 550 over its duration by half; Whistle lifts only a physical one
			// and went to waste here. Not while the flute comes: its cast is the next spell (concept 01, "Verstärker", A507).
			if (!MoonFluteComing && BristlePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (SongOfTormentPvE.CanUse(out act))
		{
			return true;
		}

		if (AetherialSparkPvE.CanUse(out act))
		{
			return true;
		}

		if (BreathOfMagicPvE.CanUse(out act))
		{
			return true;
		}

		if (SongOfTormentPvE.CanUse(out act, skipStatusProvideCheck: true) && SongOfTormentPvE.Target.Target != null && (SongOfTormentPvE.Target.Target?.WillStatusEndGCD(3, 0, true, SongOfTormentPvE.Setting.TargetStatusProvide ?? []) ?? false))
		{
			if (BristlePvE.Info.IsOnSlot && StatusHelper.PlayerHasStatus(true, StatusID.Boost_1716))
			{
				return true;
			}
		}

		act = null;
		return false;
	}

	private bool SpendCooldowns(out IAction? act)
	{
		// Whistle, then Tingle, then Triple Trident: each enhancer as many GCDs ahead as enhancers are still missing. The old
		// condition asked for Tingle as the last GCD, which Tingle's own status never let happen (D8, A507).
		if (TripleTridentPvE.Info.IsOnSlot)
		{
			var tingleMissing = TinglePvE.Info.IsOnSlot && !StatusHelper.PlayerHasStatus(true, StatusID.Tingling);
			if (TripleTridentPvE.Cooldown.WillHaveOneChargeGCD((uint)((tingleMissing ? 1 : 0) + 1)) && WhistlePvE.CanUse(out act))
			{
				return true;
			}

			if (TripleTridentPvE.Cooldown.WillHaveOneChargeGCD(1) && TinglePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (TripleTridentPvE.CanUse(out act))
		{
			return true;
		}

		if (DivineCataractPvE.CanUse(out act))
		{
			return true;
		}

		// Bristle lifts Matra Magic's eight hits by half: under DPS mimicry 800 to 1,200 for one GCD of 210; without it 400
		// to 600, less than the GCD (concept 01, Synergien, A508).
		if (IsDPS && MatraMagicPvE.Info.IsOnSlot && MatraMagicPvE.Cooldown.WillHaveOneChargeGCD(1) && !MoonFluteComing
			&& BristlePvE.CanUse(out act))
		{
			return true;
		}

		if (MatraMagicPvE.CanUse(out act))
		{
			return true;
		}

		if (TheRoseOfDestructionPvE.CanUse(out act))
		{
			return true;
		}

		// The fourth Winged Reprobation grants Winged Redemption for 10 s, which lifts Conviction Marcato from 220 to 440;
		// RSR never cast Marcato (D11, A508).
		if (StatusHelper.PlayerHasStatus(true, StatusID.WingedRedemption) && ConvictionMarcatoPvE.CanUse(out act))
		{
			return true;
		}

		if (WingedReprobationPvE.CanUse(out act, skipStatusProvideCheck: true, usedUp: true))
		{
			return true;
		}

		if (BeingMortalPvE.CanUse(out act))
		{
			return true;
		}

		// Peripheral Synthesis deals 400 instead of 220 when it puts Lightheaded on its target, and Mustard Bomb burns a
		// Lightheaded target for 15 s. Repeated use wears Lightheaded off until the target is immune for a while: it goes only
		// on a target without Lightheaded on which its last hit landed it (concept 01, Synergien, A508). Unconditioned it
		// went before every filler, at 220 in a 2 s cast.
		if (PeripheralSynthesisPvE.CanUse(out act) && PeripheralSynthesisPvE.Target.Target is { } synthesised
			&& !synthesised.HasStatus(true, StatusID.Lightheaded_2501)
			&& OwnHitWatch.LastLanded(synthesised, PeripheralSynthesisPvE.ID) != false)
		{
			return true;
		}

		if (MustardBombPvE.CanUse(out act) && MustardBombPvE.Target.Target is { } burnt
			&& burnt.HasStatus(true, StatusID.Lightheaded_2501) && !IsLastGCD(true, MustardBombPvE))
		{
			return true;
		}

		if (DevourPvE.CanUse(out act, skipStatusProvideCheck: Player?.GetEffectiveHpPercent() <= DevourHealThreshold * 100f || IsTank))
		{
			return true;
		}

		act = null;
		return false;
	}

	private bool UseSituational(out IAction? act)
	{
		if (NumberOfHostilesInMaxRange > 0 && IceSpikesPvE.CanUse(out act))
		{
			return true;
		}

		if (NumberOfHostilesInMaxRange > 0 && SchiltronPvE.CanUse(out act))
		{
			return true;
		}

		// Missile's effect text says its chance is low: it goes under "Use Low Chance abilities" only (D1, A507).
		if (LowChance && MissilePvE.CanUse(out act))
		{
			if (MissilePvE.Target.Target != null && MissilePvE.Target.Target.GetEffectiveHpPercent() >= TheMissileKnowsWhereItIs * 100f)
			{
				return true;
			}
		}

		// Fixed and percentage damage only where it beats the filler in the same time, by the measured damage per potency
		// (concept 01, "Wert statt Reihenfolge", A507).
		if (_1000NeedlesPvE.CanUse(out act) && FixedDamageBeatsFiller(_1000NeedlesPvE))
		{
			return true;
		}

		if (LauncherPvE.CanUse(out act) && HealthShareBeatsFiller(LauncherPvE))
		{
			return true;
		}

		if (DimensionalShiftPvE.CanUse(out act) && HealthShareBeatsFiller(DimensionalShiftPvE))
		{
			return true;
		}

		if (DrillCannonsPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (Player?.GetEffectiveHpPercent() <= 20f)
		{
			if (RevengeBlastPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (WhiteDeathPvE.CanUse(out act))
		{
			return true;
		}

		// Against a kind seen to take its raised potency (undead, 500), Saintly Beam beats Sonic Boom on one target too.
		if (SaintlyBeamPvE.CanUse(out act) && (OwnHitWatch.IsUndead(SaintlyBeamPvE.Target.Target) || OverSonicBoom(SaintlyBeamPvE)))
		{
			return true;
		}

		if (BlackKnightsTourPvE.CanUse(out act))
		{
			return true;
		}

		if (WhiteKnightsTourPvE.CanUse(out act))
		{
			return true;
		}

		if (StotramPvE.CanUse(out act))
		{
			return true;
		}

		if (SharpenedKnifePvE.CanUse(out act))
		{
			return true;
		}

		// A stun opens Sharpened Knife at 450 (3 y): Faze stuns for 6 s, two Knives follow (900 against three fillers' 630);
		// Tatami-gaeshi (220) and Bomb Toss (200) stun for 3 s, one Knife follows. Only where the player already stands in
		// Knife's reach, the target can still be stunned and lives that long (concept 01, Synergien, A508).
		if (KnifeCanFollow(FazePvE, SharpenedKnifePvE) && FazePvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (KnifeCanFollow(TatamigaeshiPvE, SharpenedKnifePvE) && TatamigaeshiPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (KnifeCanFollow(BombTossPvE, SharpenedKnifePvE) && BombTossPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (GoblinPunchPvE.CanUse(out act))
		{
			if (GoblinPunchPvE.Target.Target != null && CanHitPositional(EnemyPositional.Front, GoblinPunchPvE.Target.Target))
			{
				return true;
			}
		}

		if (Player?.CurrentMp <= MPGainNeed)
		{
			if (DivinationRunePvE.CanUse(out act))
			{
				return true;
			}

			if (BloodDrainPvE.CanUse(out act))
			{
				return true;
			}
		}

		act = null;
		return false;
	}

	private bool UseFiller(out IAction? act)
	{
		if (DevourPvE.CanUse(out act))
		{
			return true;
		}

		if (MagicHammerPvE.CanUse(out act))
		{
			return true;
		}

		if (CandyCanePvE.CanUse(out act))
		{
			return true;
		}

		if (HasChocobo && ChocoMeteorPvE.CanUse(out act))
		{
			return true;
		}

		if (PeripheralSynthesisPvE.CanUse(out act) && OverSonicBoom(PeripheralSynthesisPvE))
		{
			return true;
		}

		if (HydroPullPvE.CanUse(out act) && OverSonicBoom(HydroPullPvE))
		{
			return true;
		}

		if (BlazePvE.CanUse(out act) && OverSonicBoom(BlazePvE))
		{
			return true;
		}

		if (MustardBombPvE.CanUse(out act) && OverSonicBoom(MustardBombPvE))
		{
			return true;
		}

		if (FlameThrowerPvE.CanUse(out act) && OverSonicBoom(FlameThrowerPvE))
		{
			return true;
		}

		if (GlowerPvE.CanUse(out act) && OverSonicBoom(GlowerPvE))
		{
			return true;
		}

		if (TheLookPvE.CanUse(out act) && OverSonicBoom(TheLookPvE))
		{
			return true;
		}

		if (PlaincrackerPvE.CanUse(out act) && OverSonicBoom(PlaincrackerPvE))
		{
			return true;
		}

		if (HighVoltagePvE.CanUse(out act) && OverSonicBoom(HighVoltagePvE))
		{
			return true;
		}

		if (FeculentFloodPvE.CanUse(out act) && OverSonicBoom(FeculentFloodPvE))
		{
			return true;
		}

		if (KaltstrahlPvE.CanUse(out act) && OverSonicBoom(KaltstrahlPvE))
		{
			return true;
		}

		if (ElectrogenesisPvE.CanUse(out act) && OverSonicBoom(ElectrogenesisPvE))
		{
			return true;
		}

		if (NortherliesPvE.CanUse(out act) && OverSonicBoom(NortherliesPvE))
		{
			return true;
		}

		if (ProteanWavePvE.CanUse(out act) && OverSonicBoom(ProteanWavePvE))
		{
			return true;
		}

		if (AlpineDraftPvE.CanUse(out act) && OverSonicBoom(AlpineDraftPvE))
		{
			return true;
		}

		if (TatamigaeshiPvE.CanUse(out act) && OverSonicBoom(TatamigaeshiPvE))
		{
			return true;
		}

		// Perpetual Ray (220 in a 3 s cast) is no filler: Sonic Boom brings more per second (concept 01, "Füllreihenfolge").
		if (RefluxPvE.CanUse(out act) && OverSonicBoom(RefluxPvE))
		{
			return true;
		}

		if (GoblinPunchPvE.CanUse(out act) && OverSonicBoom(GoblinPunchPvE))
		{
			return true;
		}

		// Ram's Voice freezes for Ultravibration only where Ultravibration can kill: not against a higher level, a kind it was
		// seen to do nothing to, or a kind Deep Freeze does not land on. Without that, the blocked Ultravibration stayed ready
		// and Ram's Voice went every GCD (concept 01, Synergien, A508).
		var ultravibrationKills = UltravibrationPvE.Info.IsOnSlot && UltravibrationPvE.Cooldown.HasOneCharge
			&& CurrentTarget is { } frozen && frozen.Level <= DataCenter.PlayerSyncedLevel()
			&& !OwnHitWatch.FlatOrLethalDoesNothing(frozen) && !OwnHitWatch.Resists(frozen, StatusID.DeepFreeze_1731);
		if (TheRamsVoicePvE.CanUse(out act, skipAoeCheck: ultravibrationKills))
		{
			if (ultravibrationKills)
			{
				return true;
			}

			if (!UltravibrationPvE.Info.IsOnSlot && OverSonicBoom(TheRamsVoicePvE))
			{
				return true;
			}
		}

		if (TheDragonsVoicePvE.CanUse(out act, skipTargetStatusNeedCheck: !TheRamsVoicePvE.Info.IsOnSlot)
			&& (TheRamsVoicePvE.Info.IsOnSlot || OverSonicBoom(TheDragonsVoicePvE)))
		{
			return true;
		}

		// The Knight's Tour cycle begins with one tour at 200 against Sonic Boom's 210 and pays 400 for every one after it, as
		// long as Bind and Slow land. It begins only against a kind not seen to resist them, and where the target lives for the
		// opener and one payoff (concept 01, "Rösselsprünge", A507). The opener skipped the player's own status need before,
		// not the target's, and never went (D10).
		if (KnightsTourCanBegin(BlackKnightsTourPvE, WhiteKnightsTourPvE)
			&& BlackKnightsTourPvE.CanUse(out act, skipTargetStatusNeedCheck: true))
		{
			return true;
		}

		if (KnightsTourCanBegin(WhiteKnightsTourPvE, BlackKnightsTourPvE)
			&& WhiteKnightsTourPvE.CanUse(out act, skipTargetStatusNeedCheck: true))
		{
			return true;
		}

		if (ChocoMeteorPvE.CanUse(out act) && OverSonicBoom(ChocoMeteorPvE))
		{
			return true;
		}

		if (MaledictionOfWaterPvE.CanUse(out act) && OverSonicBoom(MaledictionOfWaterPvE))
		{
			return true;
		}

		if (FireAngonPvE.CanUse(out act) && OverSonicBoom(FireAngonPvE))
		{
			return true;
		}

		if (_4TonzeWeightPvE.CanUse(out act) && OverSonicBoom(_4TonzeWeightPvE))
		{
			return true;
		}

		if (InkJetPvE.CanUse(out act) && OverSonicBoom(InkJetPvE))
		{
			return true;
		}

		if (BombTossPvE.CanUse(out act) && OverSonicBoom(BombTossPvE))
		{
			return true;
		}

		if (DrillCannonsPvE.CanUse(out act, skipTargetStatusNeedCheck: true) && OverSonicBoom(DrillCannonsPvE))
		{
			return true;
		}

		if (HighVoltagePvE.CanUse(out act, skipTargetStatusNeedCheck: true) && OverSonicBoom(HighVoltagePvE))
		{
			return true;
		}

		if (FlyingFrenzyPvE.CanUse(out act) && OverSonicBoom(FlyingFrenzyPvE))
		{
			return true;
		}

		if (AquaBreathPvE.CanUse(out act) && OverSonicBoom(AquaBreathPvE))
		{
			return true;
		}

		if (AbyssalTransfixionPvE.CanUse(out act) && OverSonicBoom(AbyssalTransfixionPvE))
		{
			return true;
		}

		if (SharpenedKnifePvE.CanUse(out act, skipTargetStatusNeedCheck: true) && OverSonicBoom(SharpenedKnifePvE))
		{
			return true;
		}

		if (SonicBoomPvE.CanUse(out act))
		{
			return true;
		}

		if (WaterCannonPvE.CanUse(out act))
		{
			return true;
		}

		if (GoblinPunchPvE.CanUse(out act, skipStatusNeed: true))
		{
			return true;
		}

		if (FlyingSardinePvE.CanUse(out act))
		{
			return true;
		}

		act = null;
		return false;
	}

	protected override bool RaiseGCD(out IAction? act)
	{
		if (AngelWhisperPvE.CanUse(out act))
		{
			return true;
		}

		return base.RaiseGCD(out act);
	}

	protected override bool DispelGCD(out IAction? act)
	{
		if (ExuviationPvE.CanUse(out act))
		{
			return true;
		}

		return base.DispelGCD(out act);
	}

	protected override bool MoveForwardGCD(out IAction? act)
	{
		if (LoomPvE.CanUse(out act))
		{
			return true;
		}

		return base.MoveForwardGCD(out act);
	}

	protected override bool HealSingleGCD(out IAction? act)
	{
		if (UseTransfusion)
		{
			if (TransfusionPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (RehydrationPvE.CanUse(out act))
		{
			return true;
		}

		if (PomCurePvE.CanUse(out act))
		{
			return true;
		}

		return base.HealSingleGCD(out act);
	}

	protected override bool HealAreaGCD(out IAction? act)
	{
		if (Player?.GetEffectiveHpPercent() > 50 && WhiteWindPvE.CanUse(out act))
		{
			return true;
		}

		if (StotramPvE_23416.CanUse(out act))
		{
			return true;
		}

		if (ExuviationPvE.CanUse(out act))
		{
			return true;
		}

		if (AngelsSnackPvE.CanUse(out act, skipStatusProvideCheck: !IsHealer))
		{
			return true;
		}

		return base.HealAreaGCD(out act);
	}

	protected override bool DefenseSingleGCD(out IAction? act)
	{
		if (DiamondbackPvE.CanUse(out act))
		{
			return true;
		}

		if (ToadOilPvE.CanUse(out act))
		{
			return true;
		}

		if (DragonForcePvE.CanUse(out act))
		{
			return true;
		}

		if (IsTank)
		{
			if (CactguardPvE.CanUse(out act, targetOverride: TargetType.Self))
			{
				return true;
			}
		}

		if (CactguardPvE.CanUse(out act))
		{
			return true;
		}

		if (ChelonianGatePvE.CanUse(out act, targetOverride: TargetType.Self))
		{
			return true;
		}

		return base.DefenseSingleGCD(out act);
	}

	protected override bool DefenseAreaGCD(out IAction? act)
	{
		if (UseBadBreath)
		{
			if (BadBreathPvE.CanUse(out act, targetOverride: TargetType.HighHP, skipAoeCheck: true))
			{
				return true;
			}
		}

		if (IsHealer)
		{
			if (AngelsSnackPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (GobskinPvE.CanUse(out act))
		{
			return true;
		}

		if (ColdFogPvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseAreaGCD(out act);
	}

	#endregion

	#region Extra Methods

	private FrameCached<bool>? _moonFluteComing;
	private string _moonFluteHeldFor = string.Empty;
	private bool _moonFluteWasComing;
	private DateTime _moonFluteComingSince = DateTime.MinValue;
	private float _moonFluteNet;

	/// <summary>
	/// Whether the burst abilities wait for the coming Moon Flute: it comes, and waiting has not begun to cost more than the
	/// window gains (<see cref="BurstWaitCostsUse"/>).
	/// </summary>
	private bool BurstWaits => MoonFluteComing && !BurstWaitCostsUse();

	/// <summary>
	/// Whether the Moon Flute goes within the next GCDs: in combat, neither Waxing nor Waning running, in the set, its window
	/// worth more than Waning costs, and - under <see cref="MoonFluteOnlyWhenSafe"/> - safe (concept 01, "Mondflöte", A507).
	/// Decided once per frame; the trace gets a line when it begins to come and when the reason it is held changes.
	/// </summary>
	private bool MoonFluteComing => (_moonFluteComing ??= new(DecideMoonFlute, DataCenter.FrameCacheMs)).Value;

	private bool DecideMoonFlute()
	{
		if (!InCombat)
		{
			_moonFluteHeldFor = string.Empty;
		}

		if (!InCombat || HasWaxingNocturne || HasWaningNocturne || !MoonFlutePvE.Info.IsOnSlot || !MoonFlutePvE.EnoughLevel
			|| !MoonFluteWindowWorth(out var worth, out _moonFluteNet))
		{
			_moonFluteWasComing = false;
			return false;
		}

		if (MoonFluteOnlyWhenSafe && !MoonFluteSafe(out var why))
		{
			if (why != _moonFluteHeldFor)
			{
				_moonFluteHeldFor = why;
				DefenseTrace.Line($"moon flute held: {why} ({worth})");
			}

			_moonFluteWasComing = false;
			return false;
		}

		_moonFluteHeldFor = string.Empty;
		if (!_moonFluteWasComing)
		{
			_moonFluteComingSince = DateTime.Now;
			DefenseTrace.Line($"moon flute comes: {worth}");
		}

		_moonFluteWasComing = true;
		return true;
	}

	/// <summary>
	/// Whether Triple Trident is ready within Waxing Nocturne, so Whistle and Tingle go before the flute.
	/// </summary>
	private bool TridentInWaxing
		=> TripleTridentPvE.Info.IsOnSlot && TryMoonFluteText(out _, out var waxing, out _)
			&& (TripleTridentPvE.Cooldown.HasOneCharge || TripleTridentPvE.Cooldown.WillHaveOneCharge(waxing));

	/// <summary>
	/// Whether waiting for the flute costs more than it brings: the time already waited - the flute cannot be cast while the
	/// player moves - times what holding the burst costs per second has reached what the window gains
	/// (<see cref="BlueMageRotation.BurstHoldCostPerSecond"/>), or the wait still ahead costs one of the abilities ready now a
	/// use before the target dies (<see cref="CustomRotation.HoldCostsUse"/>). The wait ahead reaches over the rest of this
	/// GCD, Whistle and Tingle where they still go, and the flute itself.
	/// </summary>
	private bool BurstWaitCostsUse()
	{
		if ((float)(DateTime.Now - _moonFluteComingSince).TotalSeconds * BurstHoldCostPerSecond() >= _moonFluteNet)
		{
			return true;
		}

		var gcds = (TridentInWaxing && WhistlePvE.Info.IsOnSlot ? 1 : 0) + (TridentInWaxing && TinglePvE.Info.IsOnSlot ? 1 : 0);
		var wait = WeaponRemain + (gcds * WeaponTotal) + SpellTime(MoonFlutePvE);
		foreach (var action in AllBaseActions)
		{
			if (action != null && !action.Info.IsRealGCD && !action.Setting.IsFriendly && action.Info.IsOnSlot
				&& action.Cooldown.HasOneCharge && action.Cooldown.RecastTimeOneChargeRaw > WeaponTotal
				&& HoldCostsUse(action, wait))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether a stun from <paramref name="stun"/> pays through <paramref name="knife"/>: Knife in the set, the target already
	/// within Knife's reach (RSR moves nobody for damage), not stunned now, still able to be stunned (<see cref="StunWatch"/>),
	/// and living through the stun and one Knife where its time to kill is known.
	/// </summary>
	private bool KnifeCanFollow(IBaseAction stun, IBaseAction knife)
	{
		if (!knife.Info.IsOnSlot || !stun.Info.IsOnSlot || CurrentTarget is not { } target
			|| target.DistanceToPlayer() > knife.Info.Range || !StunWatch.CanStillBeStunned(target)
			|| target.HasStatus(false, StatusHelper.StunStatus))
		{
			return false;
		}

		var ttk = target.GetCorrectedTTK();
		return float.IsNaN(ttk) || ttk > SpellTime(stun) + SpellTime(knife);
	}

	/// <summary>
	/// Whether a Knight's Tour cycle may begin with <paramref name="opener"/>: the other tour that pays for it is in the set,
	/// the target's kind has not been seen to resist Bind or Slow, and the target lives long enough for the opener and one
	/// payoff where its time to kill is known.
	/// </summary>
	private bool KnightsTourCanBegin(IBaseAction opener, IBaseAction payoff)
	{
		if (!payoff.Info.IsOnSlot || CurrentTarget is not { } target
			|| OwnHitWatch.Resists(target, StatusID.Bind) || OwnHitWatch.Resists(target, StatusID.Slow))
		{
			return false;
		}

		var ttk = target.GetCorrectedTTK();
		return float.IsNaN(ttk) || ttk > SpellTime(opener) + SpellTime(payoff);
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

			return base.CanHealSingleSpell && (IsHealer || aliveHealerCount == 0);
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

			return base.CanHealAreaSpell && (IsHealer || aliveHealerCount == 0);
		}
	}
	#endregion
}