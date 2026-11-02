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
				+ $" ({PetOnHorn(safe)}) heals or protects");
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

		if (BrutalRagePvE.CanUse(out act, usedUp: true))
		{
			return true;
		}
		if (HawkishTalonsPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}
		if (RisenFallPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}
		if (CalamityPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

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
		if (nature == OneWithNatureOrder.Tempered)
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
		if (!waitsForNature && !keepForDanger)
		{
			if (AnyBattlehornReady && ((!IsLastAction(true, TrickPvE) && TrickPvE.Cooldown.HasOneCharge) || !TrickPvE.EnoughLevel))
			{
				if (PartingBlowPvE.CanUse(out act))
				{
					return true;
				}
			}
		}

		if (BrutalRagePvE.CanUse(out act, usedUp: true, skipStatusNeed: true))
		{
			return true;
		}
		if (HawkishTalonsPvE.CanUse(out act, usedUp: true, skipStatusNeed: true))
		{
			return true;
		}
		if (RisenFallPvE.CanUse(out act, usedUp: true, skipStatusNeed: true))
		{
			return true;
		}
		if (CalamityPvE.CanUse(out act, usedUp: true, skipStatusNeed: true))
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

		if (SeedsowerPvE.CanUse(out act))
		{
			return true;
		}

		return base.AttackAbility(nextGCD, out act);
	}

	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		if (InCombat)
		{
			if (BeastskinPvE.CanUse(out act))
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

		if (ActiveBattlehorn == 0 && HornBySituation && SituationalHorn(out act))
		{
			return true;
		}

		if (ActiveBattlehorn == 0)
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

		var nature = EffectiveNature(horn);
		return TrickHealsOnHorn(horn)
			|| (nature == OneWithNatureOrder.Tempered && (familiar.ReleaseHeals || familiar.ReleaseProtects)
				&& (BattlehornMasteryIiTrait.EnoughLevel || !TemperedReleasePvE.Cooldown.IsCoolingDown))
			|| (nature == OneWithNatureOrder.Borrow && familiar.BorrowProtects
				&& (BattlehornMasteryIiiTrait.EnoughLevel || !BorrowPvE.Cooldown.IsCoolingDown));
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
			why = "you are in danger, it heals or protects";
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

		var pressed = call switch
		{
			HornOrder.FirstBattlehorn => FirstBattlehornPvE.CanUse(out act),
			HornOrder.SecondBattlehorn => SecondBattlehornPvE.CanUse(out act),
			HornOrder.ThirdBattlehorn => ThirdBattlehornPvE.CanUse(out act),
			_ => false,
		};
		if (pressed)
		{
			TraceHorn($"{call} ({PetOnHorn(call)}) called: {why}");
		}

		return pressed;
	}

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
	#endregion

	#region GCD Logic
	protected override bool MoveForwardGCD(out IAction? act)
	{

		return base.MoveForwardGCD(out act);
	}

	protected override bool GeneralGCD(out IAction? act)
	{
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