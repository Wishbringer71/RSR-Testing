using System.ComponentModel;

namespace RotationSolver.RebornRotations.Melee;

[Rotation("Reborn", CombatType.PvE, GameVersion = "7.57")]
[SourceCode(Path = "main/RebornRotations/Limited Jobs/BST_Reborn.cs")]
public sealed class BST_Reborn : BeastmasterRotation
{
	[RotationConfig(CombatType.PvE, Name = "First Horn")]
	private HornOrder FirstHorn { get; set; } = HornOrder.FirstBattlehorn;

	[RotationConfig(CombatType.PvE, Name = "Second Horn")]
	private HornOrder SecondHorn { get; set; } = HornOrder.SecondBattlehorn;

	[RotationConfig(CombatType.PvE, Name = "Third Horn")]
	private HornOrder ThirdHorn { get; set; } = HornOrder.ThirdBattlehorn;

	[RotationConfig(CombatType.PvE, Name = "What to use One with Nature on for the First Horn")]
	private OneWithNatureOrder HornNatureFirst { get; set; } = OneWithNatureOrder.Tempered;

	[RotationConfig(CombatType.PvE, Name = "What to use One with Nature on for the Second Horn")]
	private OneWithNatureOrder HornNatureSecond { get; set; } = OneWithNatureOrder.Tempered;

	[RotationConfig(CombatType.PvE, Name = "What to use One with Nature on for the Third Horn")]
	private OneWithNatureOrder HornNatureThird { get; set; } = OneWithNatureOrder.Borrow;

	// Concept 01, "Begleiter nach Lage" (A485): which ready horn to call is the one lever over the familiars; what each
	// horn spends One with Nature on stays the setting above. On by default: a new rule is to be tested.
	[RotationConfig(CombatType.PvE, Name = "Choose the battlehorn by the situation",
		Tooltip = "Among the ready horns, the familiar called first is the one whose Trick brings least against the enemies "
			+ "around your target, so the one that does most - an area Trick on a pack - is called last and stays until a "
			+ "horn is back. At equal damage the one that heals or protects you stays. When your HP is falling below the line "
			+ "at which RSR heals you with abilities, a familiar that heals or protects you (its Trick, or what its horn "
			+ "spends One with Nature on) is called first, one that does neither leaves by Parting Blow for it, one whose "
			+ "Trick heals you is not sent off while the danger lasts, and Scaleskin goes up at once. Only what you have at "
			+ "your level counts.\n"
			+ "In a fight: on a pack the area familiar keeps the long stint, and when you are losing health the healing "
			+ "familiar comes at once instead of at its turn.\n"
			+ "Off: the horn order above.")]
	private bool HornBySituation { get; set; } = true;

	// Concept 01, B5 (A522): his proposal of 04.11.2026 - out of combat, the familiars for the next fight. On by default:
	// a new rule is to be tested.
	[RotationConfig(CombatType.PvE, Name = "Set the battlehorns between fights for the next one",
		Tooltip = "Out of combat - the only time the server takes it - RSR puts on your horns the familiars you have that suit "
			+ "the next fight. Before a pack: the most damage against it, a Trick or Tempered Release on an area counted "
			+ "by the enemies it reaches. Before a boss - the nearest enemy you have not fought yet carries a boss icon: "
			+ "two familiars that heal or protect you, one whose Trick heals among them, then the most damage. Once you "
			+ "were in danger in this area, one more that heals or protects, before every fight here. At equal value "
			+ "your familiars stay; the horn whose familiar is out stays as it is. What each horn spends One with Nature "
			+ "on stays your setting. Changed by hand, your horns stay until the next fight ends; nothing changes while "
			+ "the Bestiary is open or in the Crucible.\n"
			+ "In a fight: a pack meets the familiars that hit or stop all of it; at a boss the familiars that heal or "
			+ "protect you are on the horns when the danger comes.\n"
			+ "Off: RSR never changes your horns.")]
	private bool HornsBetweenFights { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Capture familiars you do not have yet",
		Tooltip = "When an enemy you can befriend as a familiar, one you do not have yet and not above your level, is about to "
			+ "fall, Capture comes first and marks it with the last weaponskill whose mark is sure to arrive before it falls: "
			+ "as soon as waiting one more weaponskill could let it fall first - by the largest fall it has taken within "
			+ "that time since its full health, by the largest fall any enemy in this area took within it, killing blows "
			+ "included, or by its time to kill. Trash that falls that fast is marked with the pull, on the enemy you "
			+ "attack anyway. No pact if it falls before the mark arrives; the lower its HP when marked, the likelier the "
			+ "pact, so the mark comes no earlier than that. Capture shares the weaponskill timer, so it costs one "
			+ "weaponskill per attempt, and none once you have that familiar.\n"
			+ "Off: Capture is never used.")]
	private bool CaptureFamiliars { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use Snarl and Challenge based on your HP and your familiar's HP (Crucible only)",
		Tooltip = "Off: Snarl is never used.\nOn: Snarl gives your damage to the familiar and Challenge takes it back, based on the settings below. If one of you has to die, it will be the familiar.")]
	private bool FamiliarTankSwap { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Only use Snarl and Challenge on Last Horn", Parent = nameof(FamiliarTankSwap))]
	private bool LasthornOnly { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "How to decide who takes damage", Parent = nameof(FamiliarTankSwap))]
	private TankSwapMode TankSwap { get; set; } = TankSwapMode.Thresholds;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Snarl when familiar HP is at or above", Parent = nameof(TankSwap), ParentValue = ThresholdsDescription)]
	private float SnarlFamiliarHp { get; set; } = 0.7f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Challenge when familiar HP is at or below", Parent = nameof(TankSwap), ParentValue = ThresholdsDescription)]
	private float ChallengeFamiliarHp { get; set; } = 0.3f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Challenge when your HP is this much higher than the familiar's", Parent = nameof(TankSwap), ParentValue = DynamicDescription,
		Tooltip = "The familiar takes damage while its HP is at or above yours. Once your HP is higher than the familiar's by this much, Challenge takes the damage back. Higher values mean fewer swaps.")]
	private float DynamicSwapMargin { get; set; } = 0.2f;

	[Range(0, 1, ConfigUnitType.Percent)]
	[RotationConfig(CombatType.PvE, Name = "Your HP floor: at or below this the familiar takes all damage, even if it dies", Parent = nameof(FamiliarTankSwap))]
	private float PlayerHpFloor { get; set; } = 0.3f;

	private const string ThresholdsDescription = "Familiar HP thresholds";
	private const string DynamicDescription = "Dynamic (compare your HP and the familiar's HP)";

	public enum TankSwapMode : byte
	{
		[Description(ThresholdsDescription)]
		Thresholds,

		[Description(DynamicDescription)]
		Dynamic,
	}

	#region Countdown logic
	// Defines logic for actions to take during the countdown before combat starts.
	protected override IAction? CountDownAction(float remainTime)
	{

		return base.CountDownAction(remainTime);
	}
	#endregion

	#region Emergency Logic
	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		// B2: in danger the barrier Borrow left you goes up at once, not only for an announced hit.
		if (HornBySituation && InCombat && InDanger && ScaleskinPvE.CanUse(out act))
		{
			TraceHorn("Scaleskin: you are in danger");
			return true;
		}

		// B2: in danger, a familiar that neither heals nor protects you leaves for a ready one that does.
		// Not while Snarl has the familiar taking your damage (Crucible): then it is the one sparing you.
		if (HornBySituation && InCombat && ActiveBattlehorn != 0 && InDanger && !HornIsSafe((HornOrder)ActiveBattlehorn)
			&& !StatusHelper.PlayerHasStatus(false, StatusID.Covered_2413)
			&& FirstSafeReadyHorn() is { } safe && PartingBlowPvE.CanUse(out act))
		{
			TraceHorn($"{(HornOrder)ActiveBattlehorn} ({CurrentPet}) leaves by Parting Blow: you are in danger, {safe}"
				+ $" ({PetOnHorn(safe)}) heals, protects or stops your attackers");
			return true;
		}

		if (InCombat && FamiliarTankSwap)
		{
			if (!LasthornOnly || (LasthornOnly && OnLastHorn))
			{
				if (FamiliarTankSwapAbility(out act))
				{
					return true;
				}
			}
		}

		return base.EmergencyAbility(nextGCD, out act);
	}

	private bool FamiliarTankSwapAbility(out IAction? act)
	{
		act = null;

		var familiar = Familiar;
		if (!FamiliarHasHp(familiar) || Player == null)
		{
			return false;
		}

		var playerHp = Player.GetHealthRatio();
		var familiarHp = familiar!.GetHealthRatio();

		// at or below the floor the player never takes damage back, so the familiar dies first.
		var playerCritical = playerHp <= PlayerHpFloor;

		bool shouldSnarl;
		bool shouldChallenge;
		if (TankSwap == TankSwapMode.Dynamic)
		{
			shouldSnarl = playerCritical || familiarHp >= playerHp;
			shouldChallenge = !playerCritical && playerHp > familiarHp + DynamicSwapMargin;
		}
		else
		{
			shouldSnarl = playerCritical || (familiarHp >= SnarlFamiliarHp && familiarHp > ChallengeFamiliarHp);
			shouldChallenge = !playerCritical && familiarHp <= ChallengeFamiliarHp;
		}

		if (shouldSnarl)
		{
			if (SnarlPvE.CanUse(out act, skipTargetStatusNeedCheck: true))
			{
				return true;
			}
		}

		if (shouldChallenge && IsFamiliarTanking(familiar))
		{
			if (ChallengePvE.CanUse(out act))
			{
				return true;
			}
		}

		return false;
	}
	#endregion

	#region oGCD Logic
	protected override bool AttackAbility(IAction nextGCD, out IAction? act)
	{
		var resultingTP = TPCount + RallyTPRestore;

		var shouldRally =
			(MasteredInstinct == 3 && InstinctualMasteryTrait.EnoughLevel && (HasMoonstalker || HasSunstrider)) ||
			(resultingTP >= 100 && resultingTP <= 250 && !InstinctualMasteryTrait.EnoughLevel);

		if (shouldRally)
		{
			if (RallyPvE.CanUse(out act))
			{
				return true;
			}
		}

		var resultingPetTP = PetTPCount + RallyingCheerTPRestore;

		var shouldRallyCheer = resultingPetTP >= 100 && resultingPetTP <= 250;

		if (shouldRallyCheer)
		{
			if (RallyingCheerPvE.CanUse(out act))
			{
				return true;
			}
		}

		if (Finisher(skipStatusNeed: false, out act))
		{
			return true;
		}

		// W5 (concept 01, A524, A525): Shield Charge on a pack before the actions that come back within seconds - 300 on
		// every enemy within 6 y of the target, on a 60 s recast; his precept of 04.11.2026: within 3 y, on the enemy in
		// the middle of the pack, so it reaches the most, every charge. On one enemy it stays last, as before.
		if (ShieldChargePvE.CanUse(out act, usedUp: true)
			&& ShieldChargePvE.Target.AffectedTargets is { Length: > 1 } hit)
		{
			TraceHorn($"Shield Charge on {ShieldChargePvE.Target.Target?.Name}: {hit.Length} enemies in its reach");
			return true;
		}

		WatchChargeHeld();

		if (NaturalInstinct == 3 || MasteredInstinct < 3)
		{
			if (TPCount >= 100 && !IsLastAction(true, PartingBlowPvE))
			{
				if (TrickPvE.CanUse(out act, skipStatusNeed: true))
				{
					return true;
				}
			}

			if (AvalancheAxePvE.CanUse(out act, usedUp: true))
			{
				return true;
			}
			if (MistralAxePvE.CanUse(out act, usedUp: true))
			{
				return true;
			}
			if (SpinningAxePvE.CanUse(out act, usedUp: true))
			{
				return true;
			}
			if (GaleAxePvE.CanUse(out act, usedUp: true))
			{
				return true;
			}
		}

		if (MasteredInstinct == 3 || NaturalInstinct < 3)
		{
			if (PetTPCount >= 100)
			{
				// K1 (concept 01, A487): the axe that keeps the compass clockwise, so the Trick closes intentionally.
				if (CompassAxe(out act))
				{
					return true;
				}

				if (GaleAxePvE.CanUse(out act, skipStatusNeed: true, usedUp: true))
				{
					return true;
				}

				if (SpinningAxePvE.CanUse(out act, skipStatusNeed: true, usedUp: true))
				{
					return true;
				}

				if (MistralAxePvE.CanUse(out act, skipStatusNeed: true, usedUp: true))
				{
					return true;
				}

				if (AvalancheAxePvE.CanUse(out act, skipStatusNeed: true, usedUp: true))
				{
					return true;
				}
			}

			if (!IsLastAction(true, PartingBlowPvE))
			{
				if (TrickPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		// B0: what the active horn spends One with Nature on, as far as the player has it.
		var nature = EffectiveNature((HornOrder)ActiveBattlehorn);
		// R1 (concept 01, A488): a Release after which the familiar leaves (Wespe's Final Sting) waits for a familiar to
		// take its place; the horn of the familiar out starts its 90 s only with the retreat.
		// C3 (A491): a Release that deals damage over time to the party (Hypodermic Hustle) waits while you are in danger.
		// A530: and one after which the familiar leaves waits while it covers you after Snarl, as Parting Blow does below.
		if (nature == OneWithNatureOrder.Tempered && (!ReleaseRetreatsFamiliar || (ReplacementHornReady && !IsCoveredByFamiliar))
			&& !(HornBySituation && InDanger && ReleaseHurtsPartyFamiliar))
		{
			if (TemperedReleasePvE.IsEnabled)
			{
				if (TemperedReleasePvE_47092.CanUse(out act))
				{
					return true;
				}
			}

			if (TemperedReleasePvE.CanUse(out act))
			{
				return true;
			}
		}

		if (nature == OneWithNatureOrder.Borrow)
		{
			if (BorrowPvE.CanUse(out act))
			{
				return true;
			}
		}

		// B0: Parting Blow waits only for the use the horn spends One with Nature on - none on "Neither", none for an
		// action the player has not got. B2: a familiar whose Trick heals you is not sent off while the danger lasts.
		var waitsForNature = HasOneWithNature && nature switch
		{
			OneWithNatureOrder.Tempered => true,
			// Borrow is refused while this horn's own Kinship runs, and that Kinship stands still while the familiar is out.
			OneWithNatureOrder.Borrow => KinshipBattlehorn != ActiveBattlehorn,
			_ => false,
		};
		var keepForDanger = HornBySituation && ActiveBattlehorn != 0 && InDanger
			&& TrickHealsOnHorn((HornOrder)ActiveBattlehorn);

		// A530: a familiar that covers you after Snarl (Crucible) is not sent off for the cycle - it is taking the damage
		// meant for you, and there the enemy attacks only you. The cover ends with its time, Challenge, or its fall.
		var keepForCover = ActiveBattlehorn != 0 && IsCoveredByFamiliar;
		if (!keepForCover)
		{
			_coverHoldTraced = false;
		}
		else if (!_coverHoldTraced && ReplacementHornReady && !waitsForNature && !keepForDanger)
		{
			_coverHoldTraced = true;
			TraceHorn($"{(HornOrder)ActiveBattlehorn} ({CurrentPet}) stays: it covers you after Snarl, Parting Blow waits");
		}

		if (!waitsForNature && !keepForDanger && !keepForCover)
		{
			// R1: only with another horn's familiar ready to follow - not the horn of the familiar out.
			if (ReplacementHornReady && ((!IsLastAction(true, TrickPvE) && TrickPvE.Cooldown.HasOneCharge) || !TrickPvE.EnoughLevel))
			{
				if (PartingBlowPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		if (Finisher(skipStatusNeed: true, out act))
		{
			return true;
		}

		if (ShieldChargePvE.CanUse(out act, usedUp: CurrentPet != BeastmasterPet.None))
		{
			return true;
		}

		if (SoulCrushPvE.CanUse(out act))
		{
			return true;
		}

		return base.AttackAbility(nextGCD, out act);
	}

	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		// The Beast Mode actions that protect go out as soon as Borrow grants them (concept 01, B2): Beastskin, and
		// Seedsower, which lowers the damage of the enemies it reaches. Seedsower stood last among the attacks and rarely
		// found a slot; B2 and B5 count a Seedkin familiar on a Borrow horn as protection (A524).
		if (InCombat)
		{
			if (BeastskinPvE.CanUse(out act))
			{
				return true;
			}

			if (SeedsowerPvE.CanUse(out act))
			{
				return true;
			}
		}

		// this unbound usage is for UnnamedStatus_2552, special mech in crucible board 1 fight 1
		if (InCombat)
		{
			if (SnarlPvE.CanUse(out act))
			{
				return true;
			}
		}

		// B5: while an assignment waits for the server, no horn is called, so a call never brings the familiar it replaces.
		var assigning = BattlehornAssignmentPending;
		if (ActiveBattlehorn == 0 && !assigning && HornBySituation && SituationalHorn(out act))
		{
			return true;
		}

		if (ActiveBattlehorn == 0 && !assigning)
		{
			act = null;

			if (FirstHorn switch
			{
				HornOrder.FirstBattlehorn => FirstBattlehornPvE.CanUse(out act),
				HornOrder.SecondBattlehorn => SecondBattlehornPvE.CanUse(out act),
				HornOrder.ThirdBattlehorn => ThirdBattlehornPvE.CanUse(out act),
				_ => false,
			})
			{
				return true;
			}

			if (SecondHorn switch
			{
				HornOrder.FirstBattlehorn => FirstBattlehornPvE.CanUse(out act),
				HornOrder.SecondBattlehorn => SecondBattlehornPvE.CanUse(out act),
				HornOrder.ThirdBattlehorn => ThirdBattlehornPvE.CanUse(out act),
				_ => false,
			})
			{
				return true;
			}

			if (ThirdHorn switch
			{
				HornOrder.FirstBattlehorn => FirstBattlehornPvE.CanUse(out act),
				HornOrder.SecondBattlehorn => SecondBattlehornPvE.CanUse(out act),
				HornOrder.ThirdBattlehorn => ThirdBattlehornPvE.CanUse(out act),
				_ => false,
			})
			{
				return true;
			}

			// Horn order fallback if the above fails for some reason
			if (FirstBattlehornPvE.CanUse(out act))
			{
				return true;
			}

			if (SecondBattlehornPvE.CanUse(out act))
			{
				return true;
			}

			if (ThirdBattlehornPvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.GeneralAbility(nextGCD, out act);
	}

	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (InCombat)
		{
			if (ScaleskinPvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		if (InCombat)
		{
			if (ScaleskinPvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	protected override bool DispelAbility(IAction nextGCD, out IAction? act)
	{
		if (ScouringAshPvE.CanUse(out act))
		{
			return true;
		}

		return base.DispelAbility(nextGCD, out act);
	}

	protected override bool InterruptAbility(IAction nextGCD, out IAction? act)
	{
		// W4 (concept 01, A486): Soul Crush deals 1,000 instead of 300 and grants 100 TP when it interrupts - on a caster
		// whose cast can be interrupted first; otherwise it goes as before.
		if (SoulCrushPvE.CanUse(out act))
		{
			return true;
		}

		return base.InterruptAbility(nextGCD, out act);
	}

	protected override bool AntiKnockbackAbility(IAction nextGCD, out IAction? act)
	{
		if (VileskinPvE.CanUse(out act))
		{
			return true;
		}

		return base.AntiKnockbackAbility(nextGCD, out act);
	}

	protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
	{

		return base.MoveForwardAbility(nextGCD, out act);
	}
	#endregion

	#region Compass
	// K1 (concept 01, A487): with a heart standing, the axe that follows it clockwise - its status need is that heart;
	// with none, the axe before the Trick's affinity, so the familiar's Trick closes an intentional combo after it. Any
	// other case, or that axe not learned, falls through to the order below. Same TP, same recast: only which axe.
	private bool CompassAxe(out IAction? act)
	{
		act = null;
		if (HasVolantHeart || HasRampantHeart || HasDurantHeart || HasEldritchHeart)
		{
			return AvalancheAxePvE.CanUse(out act, usedUp: true) || MistralAxePvE.CanUse(out act, usedUp: true)
				|| SpinningAxePvE.CanUse(out act, usedUp: true) || GaleAxePvE.CanUse(out act, usedUp: true);
		}

		var axe = (FamiliarOf(CurrentPet)?.TrickHeart ?? 0) switch
		{
			StatusID.RampantHeart => GaleAxePvE,
			StatusID.DurantHeart => AvalancheAxePvE,
			StatusID.EldritchHeart => MistralAxePvE,
			StatusID.VolantHeart => SpinningAxePvE,
			_ => null,
		};
		return axe != null && axe.CanUse(out act, skipStatusNeed: true, usedUp: true);
	}
	#endregion

	#region Finishers
	// W3 (concept 01, A486): the finisher that does not move you first. Risen Fall is Brutal Rage without the rush - the
	// same potency, affinity, circle and reach. Between Calamity's line and Hawkish Talons' circle the one that reaches
	// more enemies, a tie to Calamity. Sunstrider before Moonstalker as before.
	private bool Finisher(bool skipStatusNeed, out IAction? act)
	{
		if (RisenFallPvE.CanUse(out act, usedUp: true, skipStatusNeed: skipStatusNeed)
			|| BrutalRagePvE.CanUse(out act, usedUp: true, skipStatusNeed: skipStatusNeed))
		{
			return true;
		}

		var line = CalamityPvE.CanUse(out var calamity, usedUp: true, skipStatusNeed: skipStatusNeed);
		var rush = HawkishTalonsPvE.CanUse(out var talons, usedUp: true, skipStatusNeed: skipStatusNeed);
		if (line && (!rush || Reached(CalamityPvE) >= Reached(HawkishTalonsPvE)))
		{
			act = calamity;
			return true;
		}

		act = talons;
		return rush;
	}

	private static int Reached(IBaseAction action)
	{
		return Math.Max(1, action.Target.AffectedTargets?.Length ?? 0);
	}
	#endregion

	#region Battlehorn choice
	// The horns in the configured order (First/Second/Third Horn), each once, the rest after them.
	private List<HornOrder> ConfiguredHornOrder()
	{
		_hornOrder.Clear();
		AddHorn(FirstHorn);
		AddHorn(SecondHorn);
		AddHorn(ThirdHorn);
		foreach (var horn in AllHorns)
		{
			AddHorn(horn);
		}

		return _hornOrder;
	}

	private void AddHorn(HornOrder horn)
	{
		if (horn != HornOrder.None && !_hornOrder.Contains(horn))
		{
			_hornOrder.Add(horn);
		}
	}

	private readonly List<HornOrder> _hornOrder = [];
	private static readonly HornOrder[] AllHorns = Enum.GetValues<HornOrder>();

	// B0: what `horn` spends One with Nature on, as far as the player has the action and RSR may use it: its setting;
	// Borrow that cannot come is Tempered Release, the only other use; neither when Tempered Release cannot come either.
	private OneWithNatureOrder EffectiveNature(HornOrder horn)
	{
		var nature = horn switch
		{
			HornOrder.FirstBattlehorn => HornNatureFirst,
			HornOrder.SecondBattlehorn => HornNatureSecond,
			HornOrder.ThirdBattlehorn => HornNatureThird,
			_ => OneWithNatureOrder.Neither,
		};
		if (nature == OneWithNatureOrder.Borrow && !(BorrowPvE.IsEnabled && BorrowPvE.Info.EnoughLevelAndQuest()))
		{
			nature = OneWithNatureOrder.Tempered;
		}

		return nature == OneWithNatureOrder.Tempered
			&& !(TemperedReleasePvE.IsEnabled && TemperedReleasePvE.Info.EnoughLevelAndQuest())
			? OneWithNatureOrder.Neither
			: nature;
	}

	// Whether the familiar on `horn` heals you with its Trick - the one way of healing that lasts only while it is out.
	private bool TrickHealsOnHorn(HornOrder horn)
	{
		return FamiliarOf(PetOnHorn(horn)) is { TrickHeals: true } && TrickPvE.Info.EnoughLevelAndQuest();
	}

	// B2: you are in danger as the ability self-heal reads it (Second Wind: forecast to an instant heal under the line at
	// which RSR heals you with abilities), or in the critical class. The judgement of this rotation, not of the base class.
	private static bool InDanger
	{
		get
		{
			var player = Player;
			return player != null && !player.IsDead
				&& (player.GetForecastHealthRatio(true) < HealthSingleAbility || player.IsInCriticalClass());
		}
	}

	// Whether the familiar on `horn` heals or protects you, by what the player has: its Trick, or what the horn spends One
	// with Nature on - and that only when it comes with the call (the recast reset of Battlehorn Mastery II and III, or
	// the action not recasting), not up to 30 s later.
	private bool HornIsSafe(HornOrder horn)
	{
		if (FamiliarOf(PetOnHorn(horn)) is not { } familiar)
		{
			return false;
		}

		// C1 (concept 01, A491): a Release that stops every enemy of a pack attacking you protects like one that lowers
		// the damage you take.
		return SafeBy(familiar, EffectiveNature(horn), StopsAttackers(familiar, CurrentTarget),
			BattlehornMasteryIiTrait.EnoughLevel || !TemperedReleasePvE.Cooldown.IsCoolingDown,
			BattlehornMasteryIiiTrait.EnoughLevel || !BorrowPvE.Cooldown.IsCoolingDown);
	}

	// Whether `familiar` heals or protects you on a horn that spends One with Nature on `nature`: its Trick heals, once the
	// player has Trick, or the action the horn spends it on heals, protects, or stops the attackers (`stops`), as far as
	// that action comes with the call. One judgement for B2 in a fight and B5 before it.
	private bool SafeBy(FamiliarProfile familiar, OneWithNatureOrder nature, bool stops, bool releaseComes, bool borrowComes)
	{
		return (familiar.TrickHeals && TrickPvE.Info.EnoughLevelAndQuest())
			|| (nature == OneWithNatureOrder.Tempered && releaseComes
				&& (familiar.ReleaseHeals || familiar.ReleaseProtects || stops))
			|| (nature == OneWithNatureOrder.Borrow && borrowComes && familiar.BorrowProtects);
	}

	// The first ready horn whose familiar's Tempered Release stops the pack coming at you, on a horn that spends One with
	// Nature on it and with the Release coming with the call (C1 at the pull, concept 01, A542).
	private HornOrder? FirstPackStopperReadyHorn()
	{
		foreach (var horn in ConfiguredHornOrder())
		{
			if (HornReady(horn) && FamiliarOf(PetOnHorn(horn)) is { } familiar
				&& EffectiveNature(horn) == OneWithNatureOrder.Tempered
				&& (BattlehornMasteryIiTrait.EnoughLevel || !TemperedReleasePvE.Cooldown.IsCoolingDown)
				&& StopsPackComingAtYou(familiar, CurrentTarget))
			{
				return horn;
			}
		}

		return null;
	}

	private HornOrder? FirstSafeReadyHorn()
	{
		foreach (var horn in ConfiguredHornOrder())
		{
			if (HornReady(horn) && PetOnHorn(horn) != BeastmasterPet.None && HornIsSafe(horn))
			{
				return horn;
			}
		}

		return null;
	}

	// B2 and B3 of concept 01: in danger a familiar that heals or protects; otherwise the one whose Trick brings least
	// against the enemies around the target first, so the best stays last; at equal value one that heals or protects
	// stays, then the configured order. A horn without a familiar does not count (B0). Without a catalog, or with no
	// horn ready, the configured order below decides.
	private bool SituationalHorn(out IAction? act)
	{
		act = null;
		HornOrder? chosen = null;
		var why = string.Empty;
		if (InCombat && InDanger && FirstSafeReadyHorn() is { } safe)
		{
			chosen = safe;
			why = DangerReason;
		}
		else if (InCombat && FirstPackStopperReadyHorn() is { } stopper)
		{
			// C1 at the pull (A542): a pack comes at you and nobody holds it; the familiar whose Release stops it comes
			// first, before any danger, while its Release still finds the pack fresh and no other Release has taken the
			// shared recast. On 04.11.2026 the Raptor came first by the configured order, its Release went for damage, and
			// War Cry lay in the recast when the danger came 1.5 s later.
			chosen = stopper;
			why = "a pack comes at you and nobody holds it, its Release stops it";
		}
		else
		{
			var bestValue = float.MaxValue;
			var bestSafe = true;
			var ready = 0;
			var hasTrick = TrickPvE.Info.EnoughLevelAndQuest();
			foreach (var horn in ConfiguredHornOrder())
			{
				if (!HornReady(horn) || PetOnHorn(horn) == BeastmasterPet.None)
				{
					continue;
				}

				if (FamiliarOf(PetOnHorn(horn)) is not { } familiar)
				{
					return false;
				}

				ready++;
				// Before the player has Trick no familiar brings one: all alike, the configured order decides.
				var value = hasTrick ? TrickValue(familiar, CurrentTarget) : 0f;
				var isSafe = HornIsSafe(horn);
				if (value < bestValue || (value == bestValue && bestSafe && !isSafe))
				{
					bestValue = value;
					bestSafe = isSafe;
					chosen = horn;
				}
			}

			why = ready > 1 ? $"its Trick brings least here ({bestValue:F0}), the best stays last" : "the only horn ready";
		}

		if (chosen is not { } call)
		{
			return false;
		}

		// W1 keeps a horn's 1 s cast out of the next GCD; a horn called in danger may run into it (A486): a fraction of a
		// GCD against the healing or protecting familiar up to a GCD sooner.
		var clipping = IBaseAction.IgnoreClipping;
		IBaseAction.IgnoreClipping = clipping || why == DangerReason;
		bool pressed;
		try
		{
			pressed = call switch
			{
				HornOrder.FirstBattlehorn => FirstBattlehornPvE.CanUse(out act),
				HornOrder.SecondBattlehorn => SecondBattlehornPvE.CanUse(out act),
				HornOrder.ThirdBattlehorn => ThirdBattlehornPvE.CanUse(out act),
				_ => false,
			};
		}
		finally
		{
			IBaseAction.IgnoreClipping = clipping;
		}
		if (pressed)
		{
			TraceHorn($"{call} ({PetOnHorn(call)}) called: {why}");
		}

		return pressed;
	}

	private const string DangerReason = "you are in danger, it heals, protects or stops your attackers";

	// One line per decision: the choice is asked every frame until the familiar is out.
	private static void TraceHorn(string line)
	{
		var now = DateTime.Now;
		if (line == _lastHornTrace && (now - _lastHornTraceAt).TotalSeconds < DataCenter.DefaultGCDTotal)
		{
			return;
		}

		_lastHornTrace = line;
		_lastHornTraceAt = now;
		DefenseTrace.Line(line);
	}

	private static string _lastHornTrace = string.Empty;
	private static DateTime _lastHornTraceAt = DateTime.MinValue;

	// A530: whether the hold for this cover has been written once.
	private static bool _coverHoldTraced;

	// A530: the enemy and the time of the last Capture line.
	private static ulong _captureTracedId;
	private static DateTime _captureTracedAt = DateTime.MinValue;

	// What the horn choice and Capture read (concept 01, "Zustandszeile", A489).
	private readonly record struct BeastmasterState(BeastmasterPet First, BeastmasterPet Second, BeastmasterPet Third,
		HornOrder Out, bool BySituation, bool BetweenFights, bool Capture, bool CaptureEnabled, bool ListLoaded, string Catalog,
		bool LeastHpFirst);

	private static BeastmasterState? _lastState;

	// One line whenever that changes. On 02.11.2026 RSR called 28 horns in an hour with every rule built (b83dc3ce9) and
	// no rule wrote a line or built the catalog; the files could not tell empty slots from a switch that was off.
	private void TraceState()
	{
		var state = new BeastmasterState(PetOnHorn(HornOrder.FirstBattlehorn), PetOnHorn(HornOrder.SecondBattlehorn),
			PetOnHorn(HornOrder.ThirdBattlehorn), (HornOrder)ActiveBattlehorn, HornBySituation, HornsBetweenFights, CaptureFamiliars,
			CapturePvE.IsEnabled, FamiliarListLoaded, FamiliarCatalogState, DataCenter.NoTankNoHealerBeside);
		if (state == _lastState)
		{
			return;
		}

		_lastState = state;
		DefenseTrace.Line($"beastmaster state: horns {state.First}, {state.Second}, {state.Third} | out {state.Out}"
			+ $" | choose by the situation {state.BySituation} | set between fights {state.BetweenFights}"
			+ $" | capture {state.Capture}, action enabled {state.CaptureEnabled}"
			+ $" | familiar list loaded {state.ListLoaded} | catalog {state.Catalog}"
			+ $" | least HP first (no tank, no healer beside you) {state.LeastHpFirst}");
	}
	#endregion

	protected override void UpdateInfo()
	{
		base.UpdateInfo();

		// B5 (A522): the judgement of a sent assignment every frame, in combat or not; what the fights here showed; a change
		// by hand; then, out of combat, the horns for the next fight.
		_ = BattlehornAssignmentPending;
		// A2 (A523): the mark of Capture, its landing and the pact, followed while Capture is on.
		if (CaptureFamiliars)
		{
			WatchCapture();
		}

		RecordFight();
		CountFight();
		WatchHand();
		if (HornsBetweenFights)
		{
			AssignBetweenFights();
		}
	}

	#region Battlehorns between fights
	// B5 (concept 01, "Belegung der Hörner in der Rotation", A522): his proposal of 04.11.2026 - in a dungeon the area
	// familiars for the trash, before the boss a combination for one enemy, protection or healing where the fights showed
	// a need for it, set while out of combat.

	// Familiars that heal or protect you on the horns before a boss. With one, a danger that came after its stint found
	// none in 36 of 324 modelled fights; with two, no danger was covered worse than with his own horns, at 0.3 % of the
	// boss's potency on average where none came (bst_assign_model.py, 04.11.2026).
	private const int BossSafeFamiliars = 2;

	private static readonly HornOrder[] Horns = ThreeHorns;
	private static readonly int I = HornSlot(HornOrder.FirstBattlehorn);
	private static readonly int II = HornSlot(HornOrder.SecondBattlehorn);
	private static readonly int III = HornSlot(HornOrder.ThirdBattlehorn);
	private static readonly BeastmasterPet[] Pets = Enum.GetValues<BeastmasterPet>();

	private uint _area;
	private bool _fighting;
	private int _fightPeak;
	private int _lastPeak;
	private int _waitingPeak;
	private bool _dangerInArea;
	private bool _heldByHand;
	private byte[] _slotsSeen = [];
	private HornPlanKey? _planKey;
	private string _planTraced = string.Empty;
	private readonly List<BeastmasterPet> _candidates = [];

	// What the choice reads; a new plan only when one of these changes.
	private readonly record struct HornPlanKey(uint Area, int Level, bool Boss, int Required, int Pack,
		byte First, byte Second, byte Third, int Out, int Candidates, OneWithNatureOrder NatureFirst,
		OneWithNatureOrder NatureSecond, OneWithNatureOrder NatureThird);

	// A familiar on a horn, as the choice weighs it.
	private readonly record struct HornOption(BeastmasterPet Pet, bool Safe, bool Fitting, float Damage, bool TrickHeals);

	// The order of the choice (concept 01, B5): the required familiars that heal or protect; among them the kind that
	// covers this fight best; the most damage; after a danger in this area more that heal or protect; his familiars on
	// their horns; more that heal or protect; more whose Trick heals.
	private readonly record struct HornPlanScore(int Required, int Fitting, float Damage, int SafeAfterDanger, int Kept,
		int Safe, int TrickHeals) : IComparable<HornPlanScore>
	{
		public int CompareTo(HornPlanScore other)
		{
			var order = Required.CompareTo(other.Required);
			order = order != 0 ? order : Fitting.CompareTo(other.Fitting);
			order = order != 0 ? order : Damage.CompareTo(other.Damage);
			order = order != 0 ? order : SafeAfterDanger.CompareTo(other.SafeAfterDanger);
			order = order != 0 ? order : Kept.CompareTo(other.Kept);
			order = order != 0 ? order : Safe.CompareTo(other.Safe);
			return order != 0 ? order : TrickHeals.CompareTo(other.TrickHeals);
		}
	}

	// What the fights in this area showed: the most enemies fighting at once in the last one, and whether you were in
	// danger in any - B2's judgement, or dead. Both start anew in another area.
	private void RecordFight()
	{
		var area = DataCenter.TerritoryID;
		if (area != _area)
		{
			_area = area;
			_fighting = false;
			_fightPeak = 0;
			_lastPeak = 0;
			_waitingPeak = 0;
			_dangerInArea = false;
			_heldByHand = false;
			_planKey = null;
		}

		if (!_dangerInArea && ((InCombat && InDanger) || Player is { IsDead: true }))
		{
			_dangerInArea = true;
			DefenseTrace.Line("beastmaster: danger seen in this area - one more familiar that heals or protects goes on the "
				+ "horns before each fight here");
		}

		if (InCombat)
		{
			_fighting = true;
			_fightPeak = Math.Max(_fightPeak, EnemiesFighting());
		}
		else if (_fighting)
		{
			_fighting = false;
			_lastPeak = _fightPeak;
			_fightPeak = 0;
			_waitingPeak = 0;
			_heldByHand = false;
			_planKey = null;
		}
	}

	// A524: what the familiars' actions came to in a fight - Shield Charge, Borrow and the Beast Mode actions, by RSR or
	// by hand - and how long Shield Charge stood ready with more than one enemy in range. One line when the fight ends.
	private readonly Dictionary<ActionID, int> _fightUses = [];
	private DateTime _lastUseSeen;
	private DateTime _lastCount;
	private float _chargeIdle;
	private bool _counting;
	private string _chargeHeld = string.Empty;

	// W5 (A525, A528): Shield Charge stands ready with more than one enemy within reach and does not go - why, once per
	// fight and kind of reason, with the values at that moment. On 04.11.2026 it stood so for 23 s in a pull in the South
	// Shroud, and the files could not say why; the line then came 1,051 times in one afternoon, because the distance it
	// names changed every frame.
	private void WatchChargeHeld()
	{
		if (!InCombat || !ShieldChargePvE.EnoughLevel || !ShieldChargePvE.Cooldown.HasOneCharge || NumberOfHostilesInRange <= 1)
		{
			return;
		}

		string kind;
		string why;
		if (!ShieldChargePvE.IsEnabled)
		{
			kind = why = "the action is switched off in RSR";
		}
		else if (Service.Config.AoEType != RotationSolver.Basic.Configuration.ConfigTypes.AoEType.Full)
		{
			kind = "AoE setting";
			why = $"the AoE setting is {Service.Config.AoEType}";
		}
		else if (!ShieldChargePvE.CanUse(out _, usedUp: true))
		{
			var nearest = float.MaxValue;
			foreach (var hostile in DataCenter.AllHostileTargets)
			{
				if (hostile != null && !hostile.IsDead)
				{
					nearest = Math.Min(nearest, hostile.DistanceToPlayer());
				}
			}

			kind = "no target";
			why = $"no target it may take: nearest enemy {nearest:F1} y, melee range {DataCenter.JobRange:F0} y, no new "
				+ $"enemies {Service.Config.NoNewHostiles}, else BossModReborn's dash check or the action's own conditions";
		}
		else
		{
			kind = "reach";
			why = $"its target reaches {ShieldChargePvE.Target.AffectedTargets?.Length ?? 0} enemy";
		}

		if (kind != _chargeHeld)
		{
			_chargeHeld = kind;
			DefenseTrace.Line($"Shield Charge held with {NumberOfHostilesInRange} enemies in range: {why}");
		}
	}

	private static readonly ActionID[] CountedActions = [ActionID.ShieldChargePvE, ActionID.BorrowPvE, ActionID.BeastskinPvE,
		ActionID.VileskinPvE, ActionID.CloudSkimPvE, ActionID.SeedsowerPvE, ActionID.QuellingWavePvE, ActionID.ScaleskinPvE,
		ActionID.SoulCrushPvE, ActionID.ScouringAshPvE];

	private void CountFight()
	{
		var now = DateTime.Now;
		if (InCombat)
		{
			if (!_counting)
			{
				_counting = true;
				_fightUses.Clear();
				_chargeIdle = 0f;
				_chargeHeld = string.Empty;
				_lastCount = now;
				// The pull itself - an opening Shield Charge - goes out a moment before the combat flag: a GCD back.
				_lastUseSeen = now.AddSeconds(-DataCenter.DefaultGCDTotal);
			}

			// Every action that went out since the last look, newest first, each by the time it was recorded. Read from
			// the time since the last action instead, each frame gave a slightly different moment and counted the same
			// action again: "Shield Charge 73" in one fight on 04.11.2026.
			var newest = _lastUseSeen;
			foreach (var record in DataCenter.RecordActions)
			{
				if (record.UsedTime <= _lastUseSeen)
				{
					break;
				}

				if (record.UsedTime > newest)
				{
					newest = record.UsedTime;
				}

				// Borrow becomes one action per kin (BorrowPvE_47238 to _47245): each counts as Borrow.
				var used = (ActionID)record.Action.RowId;
				var counted = used.ToString().StartsWith(nameof(ActionID.BorrowPvE), StringComparison.Ordinal)
					? ActionID.BorrowPvE
					: used;
				if (Array.IndexOf(CountedActions, counted) >= 0)
				{
					_fightUses[counted] = _fightUses.GetValueOrDefault(counted) + 1;
				}
			}

			_lastUseSeen = newest;

			if (ShieldChargePvE.EnoughLevel && ShieldChargePvE.Cooldown.HasOneCharge && NumberOfHostilesInRange > 1)
			{
				_chargeIdle += (float)(now - _lastCount).TotalSeconds;
			}

			_lastCount = now;
			return;
		}

		if (!_counting)
		{
			return;
		}

		_counting = false;
		var uses = string.Empty;
		foreach (var counted in CountedActions)
		{
			if (counted != ActionID.ShieldChargePvE && _fightUses.TryGetValue(counted, out var n))
			{
				uses += $", {counted.ToString().Replace("PvE", string.Empty)} {n}";
			}
		}

		DefenseTrace.Line($"beastmaster fight (by RSR or by hand): Shield Charge {_fightUses.GetValueOrDefault(ActionID.ShieldChargePvE)}"
			+ (ShieldChargePvE.EnoughLevel ? $" (ready with more than one enemy in range for {_chargeIdle:F0} s)" : " (not learned)")
			+ $"{uses}{(uses.Length == 0 ? ", no Borrow and no Beast Mode action" : string.Empty)}");
	}

	// A change of the horns that RSR did not send is his: it stays until the next fight ends.
	private void WatchHand()
	{
		var slots = CurrentPets;
		if (slots.Length < Horns.Length)
		{
			return;
		}

		var changed = _slotsSeen.Length != Horns.Length;
		for (var i = 0; !changed && i < Horns.Length; i++)
		{
			changed = slots[i] != _slotsSeen[i];
		}

		if (!changed)
		{
			return;
		}

		var first = _slotsSeen.Length == 0;
		_slotsSeen = slots[..Horns.Length].ToArray();
		if (!first && !_heldByHand && !BattlehornAssignmentPending && !HornsAsAssigned)
		{
			_heldByHand = true;
			DefenseTrace.Line($"battlehorns changed by hand to {(BeastmasterPet)_slotsSeen[I]}, {(BeastmasterPet)_slotsSeen[II]}, "
				+ $"{(BeastmasterPet)_slotsSeen[III]} - kept until the next fight ends");
		}
	}

	private bool HornAvailable(HornOrder horn) => horn switch
	{
		HornOrder.FirstBattlehorn => FirstBattlehornPvE.Info.EnoughLevelAndQuest(),
		HornOrder.SecondBattlehorn => SecondBattlehornPvE.Info.EnoughLevelAndQuest(),
		HornOrder.ThirdBattlehorn => ThirdBattlehornPvE.Info.EnoughLevelAndQuest(),
		_ => false,
	};

	// B5: out of combat, the familiars for the next fight on the horns - the horn of the familiar out, and a horn the
	// player has not got yet, as they are.
	private void AssignBetweenFights()
	{
		if (InCombat || _heldByHand || BattlehornAssignmentPending || IsInCrucible || !FamiliarListLoaded || BestiaryOpen
			|| Player is not { IsDead: false } player || player.IsCasting || NextEnemy() is not { } next)
		{
			return;
		}

		var slots = CurrentPets;
		if (slots.Length < Horns.Length)
		{
			return;
		}

		var boss = next.IsBossFromIcon();
		var required = (boss ? BossSafeFamiliars : 0) + (_dangerInArea ? 1 : 0);
		_candidates.Clear();
		var reach = 0f;
		foreach (var pet in Pets)
		{
			if (pet != BeastmasterPet.None && FamiliarUnlocked(pet) == true && FamiliarOf(pet) is { } familiar)
			{
				_candidates.Add(pet);
				reach = Math.Max(reach, Math.Max(familiar.TrickRadius, familiar.ReleaseRadius));
			}
		}

		if (_candidates.Count == 0)
		{
			return;
		}

		// The most enemies seen waiting together since the last fight: it only grows until the next one. Read afresh each
		// frame it flipped with whichever enemy was nearest - on 04.11.2026 six assignments in 1.5 s between a pack of one
		// and of two (A523).
		_waitingPeak = Math.Max(_waitingPeak, EnemiesWaitingAround(next, reach));
		var pack = boss ? 1 : Math.Max(1, Math.Max(_lastPeak, _waitingPeak));
		var key = new HornPlanKey(_area, DataCenter.PlayerSyncedLevel(), boss, required, pack, slots[I],
			slots[II], slots[III], ActiveBattlehorn, _candidates.Count, EffectiveNature(HornOrder.FirstBattlehorn),
			EffectiveNature(HornOrder.SecondBattlehorn), EffectiveNature(HornOrder.ThirdBattlehorn));
		if (key == _planKey)
		{
			return;
		}

		_planKey = key;
		var current = new BeastmasterPet[Horns.Length];
		var options = new List<HornOption>[Horns.Length];
		for (var h = 0; h < Horns.Length; h++)
		{
			current[h] = PetOnHorn(Horns[h]);
			options[h] = [];
		}

		for (var h = 0; h < Horns.Length; h++)
		{
			var horn = Horns[h];
			if ((int)horn == ActiveBattlehorn || !HornAvailable(horn))
			{
				// The familiar out keeps its horn: what an assignment does to it is not measured.
				options[h].Add(Option(current[h], horn, boss, pack));
				continue;
			}

			// What the horn holds now stays a choice, an empty horn too: with fewer familiars than horns no trio would be
			// left otherwise.
			if (current[h] == BeastmasterPet.None || !_candidates.Contains(current[h]))
			{
				options[h].Add(Option(current[h], horn, boss, pack));
			}

			foreach (var pet in _candidates)
			{
				var fixedElsewhere = false;
				for (var other = 0; other < Horns.Length; other++)
				{
					fixedElsewhere |= other != h && current[other] == pet
						&& ((int)Horns[other] == ActiveBattlehorn || !HornAvailable(Horns[other]));
				}

				if (!fixedElsewhere)
				{
					options[h].Add(Option(pet, horn, boss, pack));
				}
			}
		}

		var best = (BeastmasterPet[])current.Clone();
		HornPlanScore? bestScore = null;
		foreach (var a in options[I])
		{
			foreach (var b in options[II])
			{
				if (b.Pet != BeastmasterPet.None && b.Pet == a.Pet)
				{
					continue;
				}

				foreach (var c in options[III])
				{
					if (c.Pet != BeastmasterPet.None && (c.Pet == a.Pet || c.Pet == b.Pet))
					{
						continue;
					}

					var score = Score(a, b, c, current, required);
					if (bestScore is not { } known || score.CompareTo(known) > 0)
					{
						bestScore = score;
						best[I] = a.Pet;
						best[II] = b.Pet;
						best[III] = c.Pet;
					}
				}
			}
		}

		var what = $"{(boss ? $"the boss {next.Name}" : $"a pack of {pack}")}, {required} that heal or protect"
			+ $"{(_dangerInArea ? " (danger seen in this area)" : string.Empty)}";
		var changes = false;
		for (var h = 0; h < Horns.Length; h++)
		{
			changes |= best[h] != current[h];
		}

		if (!changes)
		{
			var summary = $"{boss}|{required}|{current[I]},{current[II]},{current[III]}";
			if (summary != _planTraced)
			{
				_planTraced = summary;
				DefenseTrace.Line($"battlehorns for the next fight, {what}: kept {current[I]}, {current[II]}, {current[III]}");
			}

			return;
		}

		_planTraced = string.Empty;
		DefenseTrace.Line($"battlehorns for the next fight, {what}: I {current[I]} -> {best[I]}, II {current[II]} -> {best[II]}, "
			+ $"III {current[III]} -> {best[III]}");
		AssignBattlehorns(best[I], best[II], best[III]);
	}

	// What `pet` brings on `horn` before the next fight: whether it heals or protects you there (a stopping Release only
	// before a pack, C1); whether it is the kind that covers that fight best - its Trick heals before a boss, its Tempered
	// Release stops a pack; the damage of its Trick and, on a horn that spends One with Nature on it, its Tempered Release,
	// each its potency times the enemies its area reaches.
	private HornOption Option(BeastmasterPet pet, HornOrder horn, bool boss, int pack)
	{
		if (FamiliarOf(pet) is not { } familiar)
		{
			return new HornOption(pet, false, false, 0f, false);
		}

		var nature = EffectiveNature(horn);
		var stops = !boss && familiar.ReleaseStops && familiar.ReleaseRadius > 0f;
		var safe = SafeBy(familiar, nature, stops, releaseComes: true, borrowComes: true);
		var trickHeals = familiar.TrickHeals && TrickPvE.Info.EnoughLevelAndQuest();
		var fitting = safe && (boss ? trickHeals : stops && nature == OneWithNatureOrder.Tempered);
		var damage = TrickPvE.Info.EnoughLevelAndQuest()
			? (familiar.TrickPotency > 0f ? familiar.TrickPotency : 1f) * Hits(familiar.TrickRadius, boss, pack)
			: 0f;
		if (nature == OneWithNatureOrder.Tempered)
		{
			damage += familiar.ReleasePotency * Hits(familiar.ReleaseRadius, boss, pack);
		}

		return new HornOption(pet, safe, fitting, damage, trickHeals);
	}

	// The enemies an area reaches in the next fight: one for a single target or a boss, else the pack - every enemy of
	// it within the area, as the model assumes (a tank gathers them).
	private static int Hits(float radius, bool boss, int pack)
	{
		return radius <= 0f || boss ? 1 : pack;
	}

	private HornPlanScore Score(HornOption a, HornOption b, HornOption c, BeastmasterPet[] current, int required)
	{
		var safe = (a.Safe ? 1 : 0) + (b.Safe ? 1 : 0) + (c.Safe ? 1 : 0);
		var fitting = (a.Fitting ? 1 : 0) + (b.Fitting ? 1 : 0) + (c.Fitting ? 1 : 0);
		var kept = (a.Pet == current[I] ? 1 : 0) + (b.Pet == current[II] ? 1 : 0) + (c.Pet == current[III] ? 1 : 0);
		var healers = (a.TrickHeals ? 1 : 0) + (b.TrickHeals ? 1 : 0) + (c.TrickHeals ? 1 : 0);
		return new HornPlanScore(Math.Min(safe, required), required > 0 ? Math.Min(fitting, 1) : 0,
			a.Damage + b.Damage + c.Damage, _dangerInArea ? safe : 0, kept, safe, healers);
	}
	#endregion

	#region GCD Logic
	protected override bool MoveForwardGCD(out IAction? act)
	{

		return base.MoveForwardGCD(out act);
	}

	protected override bool GeneralGCD(out IAction? act)
	{
		TraceState();

		// A1, A2 (concept 01, A487, A525, A527): Capture first, on the last weaponskill whose mark is sure to arrive before
		// a familiar you lack falls. Out of combat only on the enemy RSR attacks anyway, so it pulls no other: trash that
		// falls within a weaponskill is marked with the pull.
		if (CaptureFamiliars && CapturePvE.CanUse(out act)
			&& (InCombat || (CurrentTarget is { } attacked && CapturePvE.Target.Target?.GameObjectId == attacked.GameObjectId)))
		{
			var why = string.Empty;
			if (CapturePvE.Target.Target is { } marked)
			{
				CaptureAimed(marked);
				_ = CaptureNow(marked, out why);
			}

			// A530: the choice repeats every frame until the weaponskill goes and its figures move with it; one line per
			// enemy and weaponskill, not one per frame.
			var chosen = CapturePvE.Target.Target?.GameObjectId ?? 0;
			var now = DateTime.Now;
			if (chosen != _captureTracedId || (now - _captureTracedAt).TotalSeconds >= DataCenter.DefaultGCDTotal)
			{
				_captureTracedId = chosen;
				_captureTracedAt = now;
				TraceHorn($"Capture on {CapturePvE.Target.Target?.Name} ({CapturableFamiliar(CapturePvE.Target.Target)}): "
					+ $"a familiar you do not have; waiting a weaponskill could let it fall before that mark arrives - {why} "
					+ $"(landing {CaptureLanding:F2} s)");
			}

			return true;
		}

		if (QuellingWavePvE.CanUse(out act))
		{
			return true;
		}

		if (ShieldsplitterPvE.CanUse(out act))
		{
			return true;
		}
		if (AxebladeBitePvE.CanUse(out act))
		{
			return true;
		}
		if (SmashAxePvE.CanUse(out act))
		{
			return true;
		}

		if (QuellingWavePvE.CanUse(out act, skipTargetStatusNeedCheck: true))
		{
			return true;
		}
		return base.GeneralGCD(out act);
	}
	#endregion
}