using System.ComponentModel;
using RotationSolver.Basic.Configuration;

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
			+ "Trick heals you is not sent off while the danger lasts, and Scaleskin goes up at once. With no tank beside you, "
			+ "a familiar whose Tempered Release stops a whole pack fighting you comes first. In the Crucible, a familiar that "
			+ "hits only the weakness of enemies still to come in this fight waits for them while another horn is ready, and "
			+ "comes first once they appear. Only what you have at your level counts.\n"
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
			+ "the Bestiary is open.\n"
			+ "On a Crucible board the game asks for the horns before each battle: on that screen RSR picks from your team "
			+ "the same way for the battle about to start - the final fight counts as a boss, the pack is the battle's enemies, "
			+ "a familiar weak spot counts the measured weakness bonus, and among equals the one with more HP left - and "
			+ "clicks them in, one at a time. Picked by hand there, your horns stay; you start the battle.\n"
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
			+ "weaponskill per attempt, and none once you have that familiar. Gauge goes out on such an enemy in combat, in a "
			+ "weave nothing else wants, once at first sight and once after your mark, to learn whether its answer counts "
			+ "the HP; DefenseTrace.log notes both answers.\n"
			+ "Off: Capture and Gauge are never used.")]
	private bool CaptureFamiliars { get; set; } = true;

	// K8 of the Crucible (concept 21, A551 to A555): the board's items that keep you and your familiar alive, and its damage
	// items.
	[RotationConfig(CombatType.PvE, Name = "Use the board's items that keep you and your familiar alive (Crucible only)",
		Tooltip = "On a Crucible board RSR uses, one at a time: when your HP fall below 'Use HP potions below this HP' in combat, "
			+ "the Beast Potion or Crucible Ash that lifts you back to that line with the least, or the largest when none does "
			+ "(the board locks normal potions); an antidote or eye drops against poison or blindness on you; a Beastmaster "
			+ "Reraiser while no Reraise stands, the stronger first - only against a boss once Reraise was seen to go without a "
			+ "fall; Temporal Sand out of combat while no Sandstill stands; a Beast Potion Kit against a boss while no "
			+ "Auto-potion stands; against an announced hit on you the Tome of the Impervious (physical), the Tome of "
			+ "Reflection (magic) or Crucible Tannin - in the final fight also while an enemy attacks you; for your familiar, when "
			+ "it holds no more HP than the largest hit it took this fight (before the first, below 'Use HP potions below this "
			+ "HP'), the Beast Potion that lifts it above that with the least - never your last healing item, which stays for "
			+ "you - and then the same protection when the hit could fell it; against the dangerous statuses the fight's enemies inflict (they take "
			+ "control from you or end in death) the soul serum that names one, else a Beast Remedy Kit; against a hit on you "
			+ "whose cast carries an element, the weakener of that element; a Crucible Needle for your petrified familiar; "
			+ "against a boss a Potion of Tempered Constitution.\n"
			+ "Off: RSR uses none of these.")]
	private bool UseCrucibleSafetyItems { get; set; } = true;

	[RotationConfig(CombatType.PvE, Name = "Use the board's damage items and buffs: damage on a pack once a fight, all of them in the final fight (Crucible only)",
		Tooltip = "On a Crucible board RSR throws a fang or Celestial Sand on your target: in the final fight - the board's items "
			+ "are lost after the run - every one, the most worth first; in any other fight one, once it hits more enemies than "
			+ "your target. Worth is its potency times the enemies it hits, an enemy weak to its element counting the measured "
			+ "weakness bonus more; at equal worth the one without knockback. One whose element only hits enemies still to come "
			+ "waits for them. In the final fight it first uses on you every potion or item that grants you a status (strength, "
			+ "magic, intensity, evasion, haste, TP, Life Siphon) while that status does not stand - a magic damage increase "
			+ "also raises the fangs; never one that inflicts a status on you.\n"
			+ "Off: RSR uses none of these.")]
	private bool UseCrucibleDamageItems { get; set; } = true;

	// K10 of the Crucible (concept 21, Campsite, A564): who rests with you; resting stays your click.
	[RotationConfig(CombatType.PvE, Name = "Pick who rests with you at a Crucible campsite (Crucible only)",
		Tooltip = "With the campsite window open RSR selects who rests with you; resting or leaving stays your click. A rest "
			+ "shares its healing (alone you recover 90% of your HP, with one familiar each 45%, with two each 30%), so "
			+ "familiars rest with you only when you still come out at full HP - never worse than resting alone. Then as many "
			+ "as bring them the most HP, up to what the campsite allows: the familiars not fallen, not at full HP and not fed "
			+ "(resting ends a feed) with the smallest share of their HP. RSR reads the share from the game's prompt once it "
			+ "is open and counts with it from then on. A selection you change by hand stays for that campsite.\n"
			+ "Off: RSR selects nobody at a campsite.")]
	private bool PickCampsiteRest { get; set; } = true;

	// K10 of the Crucible (concept 21, Laden, A569): what to buy in a shop; buying stays your click.
	[RotationConfig(CombatType.PvE, Name = "Recommend what to buy in a Crucible shop (Crucible only)",
		Tooltip = "While a shop on a board is open RSR shows a window with what to buy and why, safety first, as far as your "
			+ "tokens and slots go: a second life while you hold none; cover against each status of the board's fights that ends "
			+ "in death; protective and healing gear, the choice that leaves you the most HP after a stretch of fights between "
			+ "campsites ending with the heaviest - measured from your fights: the damage you took by kind, your own healing, the "
			+ "fights between campsites; before your first fight one fight counts as a full HP bar -, then the most effective HP; "
			+ "cover against the board's other statuses that cost safety (control, actions prevented, damage over time); healing "
			+ "items; protective items. Never feed, never gear that inflicts a status on you, nothing for damage. A piece that "
			+ "slows you says so. \"Buy\" in that window buys the list row by row; when the game asks, your answer decides. "
			+ "Without the click RSR buys nothing.\n"
			+ "Off: no window, RSR buys nothing.")]
	private bool RecommendCrucibleShop { get; set; } = true;

	// K13 (concept 21, "K13 – Schatz", A578): his report of 05.11.2026, no recommendation at a treasure.
	[RotationConfig(CombatType.PvE, Name = "Recommend what to take from a Crucible treasure",
		Tooltip = "While a treasure on a Crucible board is open, a window names the offer to take and why, in the order the "
			+ "shop recommendation uses with no price: a second life while none is held; cover against a status of the board's "
			+ "fights that ends in death; the gear that leaves you the most HP after a stretch of fights, then the most effective "
			+ "HP; cover against the board's other statuses that cost safety; healing items; protective items. When none adds "
			+ "safety, a damage item or a buff (spent in the final fight), else gear that does not harm you; never gear that "
			+ "inflicts a status on you. The gear you hold is what the last shop on the board showed, and what RSR took. \"Take\" "
			+ "in that window takes it; with a Thief's Knife the window stays open for the second pick.\n"
			+ "In a fight: nothing - a treasure is opened between fights.\n"
			+ "Off: no window, RSR takes nothing.")]
	private bool RecommendCrucibleTreasure { get; set; } = true;

	// K10 "Weg" (concept 21, A632): his rule of 06.11.2026 - route hints for the next step, from the run's progress.
	[RotationConfig(CombatType.PvE, Name = "Recommend which way to take on a Crucible board",
		Tooltip = "Between fights on a Crucible board, at the next fork a window shows each way: where its next fight leaves "
			+ "you and the healers of the final fight's plan, by the measured load of a fight, your own healing, your healing "
			+ "items and what a campsite restores; whether that is safe (one more heaviest fight in reserve, no final healer "
			+ "short); and recommends one - among safe ways the one with items, else the one that leaves you the most HP. "
			+ "Computed again on every space, one stretch ahead.\n"
			+ "In a fight: nothing.\n"
			+ "Off: no window.")]
	private bool RecommendCrucibleRoute { get; set; } = true;

	// K7 (concept 21, "Leveln", A579): his rule of 05.11.2026, the familiars that lag behind first, where they replace others adequately.
	[RotationConfig(CombatType.PvE, Name = "Prefer familiars that lag behind in rank for the Crucible team",
		Tooltip = "The team recommendation (team screen window) gives a familiar's place to the one with the lowest rank that "
			+ "replaces it adequately - it hits every weakness of the board's fights only that familiar hits in the team, heals or "
			+ "protects where that is still needed, and stands at least at the rank the game recommends for the board - so the "
			+ "familiars that lag behind earn experience. On the horn screen a familiar below that rank fights only when fewer "
			+ "others live than horns.\n"
			+ "In a fight: the three on the horns are chosen as before; a familiar that only levels sits out unless it fits best.\n"
			+ "Off: the strongest team for the board's fights, at equal worth those above the board's rank first.")]
	private bool PreferLaggingFamiliars { get; set; } = true;

	// K9 of the Crucible (concept 21, A550): on by default. Without a tank the enemies attack only you, and your fall ends the
	// run; the familiar's HP are a second store, and a fallen familiar costs that familiar, not the run.
	[RotationConfig(CombatType.PvE, Name = "Use Snarl and Challenge based on your HP and your familiar's HP (Crucible only)",
		Tooltip = "Off: Snarl is never used.\nOn: Snarl gives your damage to the familiar and Challenge takes it back, based on the settings below, "
			+ "and also as soon as the familiar holds no more HP than the largest hit it has taken in this fight - the next one "
			+ "could fell it. If one of you has to die, it will be the familiar.")]
	private bool FamiliarTankSwap { get; set; } = true;

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

		// B2: in danger, a familiar that neither heals nor protects you leaves for a ready one that does; with no healer alive
		// also while without healing you would fall before the enemies, for one whose Trick heals (A642) - the danger in
		// its course, not only once your forecast health is under the line.
		// Not while Snarl has the familiar taking your damage (Crucible): then it is the one sparing you.
		HornOrder? swapTo = null;
		var why = "you are in danger";
		if (HornBySituation && InCombat && ActiveBattlehorn != 0 && !HornIsSafe((HornOrder)ActiveBattlehorn)
			&& !StatusHelper.PlayerHasStatus(false, StatusID.Covered_2413))
		{
			if (InDanger && FirstSafeReadyHorn() is { } ready)
			{
				swapTo = ready;
			}
			// Not in danger yet, so the familiar out first spends its One with Nature - unless its Release is the retreat.
			else if ((!WaitsForNature(EffectiveNature((HornOrder)ActiveBattlehorn))
					|| (ReleaseRetreatsFamiliar && EffectiveNature((HornOrder)ActiveBattlehorn) == OneWithNatureOrder.Tempered))
				&& HealingHornWhileFalling(out var fallsIn) is { } healing)
			{
				swapTo = healing;
				why = $"without healing you fall in {fallsIn:F1} s, before the enemies";
			}
		}

		if (swapTo is { } safe)
		{
			// A624: a familiar that retreats with its own Release (Final Sting) leaves with it - the same retreat, twice the
			// potency of Parting Blow, and Lingering Vantage for the next one.
			if (ReleaseRetreatsFamiliar && HasOneWithNature
				&& EffectiveNature((HornOrder)ActiveBattlehorn) == OneWithNatureOrder.Tempered && TemperedReleaseNow(out act))
			{
				TraceHorn($"{(HornOrder)ActiveBattlehorn} ({CurrentPet}) leaves by its Release: {why}, {safe}"
					+ $" ({PetOnHorn(safe)}) heals, protects or stops your attackers");
				return true;
			}

			if (PartingBlowPvE.CanUse(out act))
			{
				TraceHorn($"{(HornOrder)ActiveBattlehorn} ({CurrentPet}) leaves by Parting Blow: {why}, {safe}"
					+ $" ({PetOnHorn(safe)}) heals, protects or stops your attackers");
				return true;
			}
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

	// K9 (A550): the largest drop of the familiar's HP from one frame to the next in this fight - a hit, or several landing
	// together, which errs on the safe side; and the familiar and its HP the frame before. A582: a drop to 0 is no hit - the
	// game shows a retreating familiar at 0 HP for a frame (Parting Blow, a Release it leaves after), and a killing hit
	// shows only what was left; counted, every familiar up to that HP drew Challenge at full HP.
	private static uint _familiarLargestHit;
	private static ulong _familiarWatched;
	private static uint _familiarLastHp;

	// A610: per fight, the HP the familiar out and the player lost while they held the enemies, and for how long - the
	// familiar's anew with each familiar. Read with the window Snarl and Challenge share (FamiliarTankSwapAbility).
	private static uint _familiarHeldLoss;
	private static float _familiarHeldSeconds;
	private static uint _playerHeldLoss;
	private static float _playerHeldSeconds;
	private static uint _playerLastHp;
	private static DateTime _heldLastRead = DateTime.MinValue;

	// A582: when the familiar out now appeared, for whether it has been sent off since (FamiliarLeaving).
	private static DateTime _familiarSince = DateTime.MinValue;

	// K11 (concept 21, A585): the use of the familiar out - which one, its HP when it appeared, its last HP above 0, whether
	// it fell -; per familiar the HP it last left a fight with and what the team agent showed for it then (the agent takes
	// a fight's losses over only with the next fight, a campsite at once); the largest loss of one use on this board.
	private static BeastmasterPet _usePet = BeastmasterPet.None;
	private static uint _useStartHp;
	private static uint _useLastHp;
	private static bool _useFell;
	private static readonly Dictionary<BeastmasterPet, (uint Hp, uint AgentHp)> _petSeenHp = [];
	private static uint _largestLossPerUse;
	private static uint _petSeenArea;

	// A637: `leaving` - the familiar out has been sent off (FamiliarLeaving); its object reads as dead until it is gone, and
	// that is no fall.
	private static void FollowFamiliarHits(bool leaving)
	{
		if (_petSeenArea != DataCenter.TerritoryID)
		{
			_petSeenArea = DataCenter.TerritoryID;
			_petSeenHp.Clear();
			_largestLossPerUse = 0u;
			FinalHealers.Clear();
			FinalTeam.Clear();
			FinalBattle = 0u;
			FinalLossPerUse = 0u;
			_usePet = BeastmasterPet.None;
		}

		var familiar = Familiar;
		if (!InCombat || familiar == null)
		{
			if (!InCombat)
			{
				_familiarLargestHit = 0u;
				_familiarHeldLoss = 0u;
				_familiarHeldSeconds = 0f;
				_playerHeldLoss = 0u;
				_playerHeldSeconds = 0f;
			}

			CloseFamiliarUse();
			_familiarWatched = 0;
			return;
		}

		var hp = familiar.CurrentHp;
		if (_familiarWatched == familiar.GameObjectId && hp < _familiarLastHp && hp > 0)
		{
			_familiarLargestHit = Math.Max(_familiarLargestHit, _familiarLastHp - hp);
		}

		// A610: the holder's loss and time; a pause longer than a GCD between reads is no holding time.
		var now = DateTime.Now;
		var since = (float)(now - _heldLastRead).TotalSeconds;
		_heldLastRead = now;
		var playerHp = Player?.CurrentHp ?? 0u;
		if (_familiarWatched != familiar.GameObjectId)
		{
			_familiarHeldLoss = 0u;
			_familiarHeldSeconds = 0f;
		}
		else if (since > 0f && since <= DataCenter.DefaultGCDTotal)
		{
			if (IsFamiliarTanking(familiar))
			{
				_familiarHeldSeconds += since;
				if (hp < _familiarLastHp && hp > 0)
				{
					_familiarHeldLoss += _familiarLastHp - hp;
				}
			}
			else if (PlayerHeldByEnemies())
			{
				_playerHeldSeconds += since;
				if (playerHp < _playerLastHp)
				{
					_playerHeldLoss += _playerLastHp - playerHp;
				}
			}
		}

		_playerLastHp = playerHp;

		if (_familiarWatched != familiar.GameObjectId)
		{
			CloseFamiliarUse();
			_familiarSince = DateTime.Now;
			_usePet = IsInCrucible ? CurrentPet : BeastmasterPet.None;
			_useStartHp = hp;
			_useLastHp = hp;
			_useFell = false;
		}

		if (hp > 0)
		{
			_useLastHp = hp;
		}

		_useFell |= familiar.IsDead && !leaving;
		_familiarWatched = familiar.GameObjectId;
		_familiarLastHp = hp;
	}

	// K11 (A585): the use of the familiar that just left - its loss into the board's measure, the HP it left with (0 when it
	// fell) beside what the team agent shows for it now.
	private static unsafe void CloseFamiliarUse()
	{
		if (_usePet == BeastmasterPet.None)
		{
			return;
		}

		var left = _useFell ? 0u : _useLastHp;
		if (_useStartHp > left)
		{
			_largestLossPerUse = Math.Max(_largestLossPerUse, _useStartHp - left);
		}

		if (AgentHealth(_usePet) is { } agent)
		{
			_petSeenHp[_usePet] = (left, agent);
		}

		_usePet = BeastmasterPet.None;
	}

	// The HP the team agent (AgentXBMPetParty) shows for `pet`, null when it is not in the team or the agent is not there.
	private static unsafe uint? AgentHealth(BeastmasterPet pet)
	{
		var party = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentXBMPetParty.Instance();
		if (party == null)
		{
			return null;
		}

		var team = party->SelectedPets.AsSpan();
		var current = party->PetCurrentHealth;
		for (var i = 0; i < team.Length && i < current.Length; i++)
		{
			if ((BeastmasterPet)team[i].PetId == pet)
			{
				return current[i];
			}
		}

		return null;
	}

	// K11 (A585): a familiar's HP for the choice between fights - what it last left a fight with while the agent has not
	// taken that fight over (it still shows what it showed then), else the agent's.
	private static uint EffectiveHealth(BeastmasterPet pet, uint agentHp)
	{
		return _petSeenHp.TryGetValue(pet, out var seen) && seen.AgentHp == agentHp ? seen.Hp : agentHp;
	}

	// A582: the familiar out has been sent off since it appeared - Parting Blow, or a Tempered Release it leaves after - and
	// covers you only until it is gone. Snarl on it would hold the 15 s recast from the one that follows.
	private bool FamiliarLeaving
	{
		get
		{
			if (_familiarWatched == 0)
			{
				return false;
			}

			var sent = DataCenter.LastUseOf(PartingBlowPvE.ID, PartingBlowPvE.AdjustedID) is { } blow && blow >= _familiarSince;
			if (!sent && ReleaseRetreatsFamiliar)
			{
				sent = (DataCenter.LastUseOf(TemperedReleasePvE.ID, TemperedReleasePvE.AdjustedID) is { } release && release >= _familiarSince)
					|| (DataCenter.LastUseOf(TemperedReleasePvE_47092.ID, TemperedReleasePvE_47092.AdjustedID) is { } other
						&& other >= _familiarSince);
			}

			return sent;
		}
	}

	private static (uint Fight, BeastmasterPet Pet) _sparedForFinal;

	// A610: whether an enemy in combat attacks the player.
	private static bool PlayerHeldByEnemies()
	{
		if (Player is not { } player)
		{
			return false;
		}

		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile.TargetObjectId == player.GameObjectId)
			{
				return true;
			}
		}

		return false;
	}

	// A610: the HP a holder loses per second - measured while it held for at least a GCD, else the enemies' measured stream
	// on the party (EnemyDanger, A541).
	private static float HoldingRate(uint loss, float seconds)
	{
		if (seconds >= DataCenter.DefaultGCDTotal)
		{
			return loss / seconds;
		}

		var stream = 0f;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			stream += EnemyDanger.Rate(hostile, out _, out _);
		}

		return stream;
	}

	// K8 item 7 (concept 21, A642): what the familiar out can lose before the next weave window - its largest hit in this
	// fight (K9), and while enemies attack it its holding rate over a GCD's lead, the larger. A hit counts once; a stream of
	// small ones from several enemies shows only in the rate.
	private static uint FamiliarLossAhead(IBattleChara? familiar)
	{
		var loss = (float)_familiarLargestHit;
		if (familiar != null && IsFamiliarTanking(familiar))
		{
			loss = Math.Max(loss, HoldingRate(_familiarHeldLoss, _familiarHeldSeconds) * ObjectHelper.GetHealLeadTime());
		}

		return (uint)loss;
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

		// K9 (A550, A610): Snarl and Challenge share one recast, so whoever holds after a swap holds that whole window. The
		// familiar falls in it when its HP do not cover the larger of its holding rate over the window and its largest hit in
		// this fight; you hold it while your HP stay over the floor. Unless your HP are at the floor, where the familiar
		// takes everything: Challenge when the familiar would fall and you would not, no Snarl onto a familiar that would
		// fall, Snarl when you would not hold the window and it would; when neither would, the familiar keeps it.
		var window = SnarlPvE.Cooldown.RecastTimeOneChargeRaw;
		var familiarLoss = Math.Max(HoldingRate(_familiarHeldLoss, _familiarHeldSeconds) * window, _familiarLargestHit);
		var playerLoss = HoldingRate(_playerHeldLoss, _playerHeldSeconds) * window;
		var nextHitFells = !playerCritical && familiarLoss > 0f && familiar!.CurrentHp <= familiarLoss;
		var playerHoldsWindow = Player.CurrentHp - playerLoss > PlayerHpFloor * Player.MaxHp;
		if (nextHitFells)
		{
			shouldSnarl = false;
			shouldChallenge = playerHoldsWindow;
		}
		else if (!playerCritical && !playerHoldsWindow)
		{
			shouldSnarl = true;
			shouldChallenge = false;
		}

		// K11 stage 2 (A590): before the final fight, not on a familiar the final fight needs to heal or protect while its HP
		// would not keep a loss for it - under Snarl it takes all that was yours, and only items and the campsite give it
		// back; your HP its Tricks heal again there. Unless your HP are at the floor.
		if (shouldSnarl && !playerCritical && IsInCrucible && CrucibleBoard.Fight != 0
			&& !CrucibleBoard.IsFinalBattle(CrucibleBoard.Fight) && ShortForFinal(CurrentPet, familiar!.CurrentHp))
		{
			shouldSnarl = false;
			if (_sparedForFinal != (CrucibleBoard.Fight, CurrentPet))
			{
				_sparedForFinal = (CrucibleBoard.Fight, CurrentPet);
				DefenseTrace.Line($"beastmaster: no Snarl on {CurrentPet} at {familiar.CurrentHp} HP - the final fight needs it to "
					+ $"heal or protect, and a use costs up to {FinalLossPerUse} HP on this board");
			}
		}

		// A582: not on a familiar that is leaving - it covers only until it is gone, and the recast would hold the next one's.
		if (shouldSnarl && !FamiliarLeaving)
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
				if (nextHitFells)
				{
					TraceHorn($"Challenge: {CurrentPet} holds {familiar!.CurrentHp} HP, it would lose {familiarLoss:F0} in the "
						+ $"{window:F0} s Snarl and Challenge share (largest hit in this fight {_familiarLargestHit}), you {playerLoss:F0}");
				}

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
		// A624: in a board's final fight it goes before the pack falls when no other familiar could follow before then -
		// held, it was lost, and after the final fight no horn is needed.
		var releaseBeforeFall = ReleaseRetreatsFamiliar && InCombat && IsInCrucible && CrucibleBoard.IsFinalFight
			&& DataCenter.IsPackTimeToKillKnown && !ReplacementHornReadyWithin(DataCenter.PackTimeToKill);
		if (nature == OneWithNatureOrder.Tempered
			&& (!ReleaseRetreatsFamiliar || ((ReplacementHornReady || releaseBeforeFall) && !IsCoveredByFamiliar))
			&& !(HornBySituation && InDanger && ReleaseHurtsPartyFamiliar) && TemperedReleaseNow(out act))
		{
			if (releaseBeforeFall && !ReplacementHornReady)
			{
				TraceHorn($"{(HornOrder)ActiveBattlehorn} ({CurrentPet}) releases before the pack falls in "
					+ $"{DataCenter.PackTimeToKill:F1} s: no other horn is back before then, and the board ends with this fight");
			}

			return true;
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
		var waitsForNature = WaitsForNature(nature);
		var keepForDanger = HornBySituation && ActiveBattlehorn != 0
			&& ((InDanger && TrickHealsOnHorn((HornOrder)ActiveBattlehorn)) || HealerFamiliarHolds());

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

		// Concept 01, "Ob Analysieren die Lebenspunkte einrechnet" (A546): Gauge last, in a weave nothing else wanted, on a
		// beast you lack - at first sight and after your mark -, to learn whether its stage counts the HP.
		if (CaptureFamiliars && GaugePvE.CanUse(out act))
		{
			if (GaugePvE.Target.Target is { } asked)
			{
				GaugeAsked(asked);
			}

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
		// A582: not on a familiar that is leaving.
		if (InCombat && !FamiliarLeaving)
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

	// The familiar's Tempered Release, by whichever of its two actions the game offers (R1, B2; A624).
	// B0: whether the familiar out still waits for the use its horn spends One with Nature on - none on "Neither", none for
	// an action the player has not got (`nature` is EffectiveNature of its horn).
	private static bool WaitsForNature(OneWithNatureOrder nature)
	{
		return HasOneWithNature && nature switch
		{
			OneWithNatureOrder.Tempered => true,
			// Borrow is refused while this horn's own Kinship runs, and that Kinship stands still while the familiar is out.
			OneWithNatureOrder.Borrow => KinshipBattlehorn != ActiveBattlehorn,
			_ => false,
		};
	}

	private bool TemperedReleaseNow(out IAction? act)
	{
		if (TemperedReleasePvE.IsEnabled && TemperedReleasePvE_47092.CanUse(out act))
		{
			return true;
		}

		return TemperedReleasePvE.CanUse(out act);
	}

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

	private bool _healerHoldTraced;

	// B2 (concept 01, A636): with no healer alive in the party, a familiar whose Trick heals stays out of the cycle while,
	// without its healing, you would fall before relief comes - the enemies in combat falling, or another horn with a
	// familiar whose Trick heals being ready. Without its healing: your HP and barrier over the damage per second the
	// enemies in combat dealt and announce (EnemyDanger, measured before any heal). Its own horn is no relief: its recast
	// starts only when it leaves.
	private bool HealerFamiliarHolds()
	{
		var active = (HornOrder)ActiveBattlehorn;
		var fallsIn = SecondsToFallUnhealed(out var rate);
		if (ActiveBattlehorn == 0 || !TrickHealsOnHorn(active) || float.IsPositiveInfinity(fallsIn))
		{
			_healerHoldTraced = false;
			return false;
		}

		var relief = DataCenter.IsPackTimeToKillKnown ? DataCenter.PackTimeToKill : float.PositiveInfinity;
		foreach (var horn in ThreeHorns)
		{
			if (horn != active && PetOnHorn(horn) != BeastmasterPet.None && TrickHealsOnHorn(horn))
			{
				relief = Math.Min(relief, HornReady(horn) ? 0f : HornRecastRemaining(horn));
			}
		}

		var holds = fallsIn < relief;
		if (holds && !_healerHoldTraced)
		{
			TraceHorn($"{active} ({CurrentPet}) stays: without its healing you fall in {fallsIn:F1} s at {rate:F0} HP per "
				+ $"second, relief in {(float.IsInfinity(relief) ? "no measured time" : $"{relief:F1} s")}");
		}

		_healerHoldTraced = holds;
		return holds;
	}

	// B2 (A636): in combat with no healer alive in the party, the seconds until you fall without healing - your HP and
	// barrier over the damage per second the enemies in combat dealt and announce (EnemyDanger, measured before any heal).
	// Infinity with a healer alive, out of combat, or before any enemy has hit or announced.
	private static float SecondsToFallUnhealed(out float rate)
	{
		rate = 0f;
		if (!InCombat || DataCenter.AnyLivingHealer(false) || Player is not { IsDead: false } player)
		{
			return float.PositiveInfinity;
		}

		foreach (var enemy in DataCenter.AllHostileTargets)
		{
			if (enemy != null && enemy.CurrentHp > 0 && enemy.InCombat())
			{
				rate += EnemyDanger.Rate(enemy, out _, out _);
			}
		}

		return rate > 0f ? player.GetEffectiveHp() / rate : float.PositiveInfinity;
	}

	// B2 (concept 01, "Danach wieder einer, der heilt", A639): while without healing you would fall before the enemies in
	// combat do, the first ready horn, in the configured order, whose familiar's Trick heals - so one healing familiar
	// follows another that left for it, instead of a short stint of another familiar in between.
	private HornOrder? HealingHornWhileFalling(out float fallsIn)
	{
		fallsIn = SecondsToFallUnhealed(out _);
		var enemiesFall = DataCenter.IsPackTimeToKillKnown ? DataCenter.PackTimeToKill : float.PositiveInfinity;
		if (!(fallsIn < enemiesFall))
		{
			return null;
		}

		foreach (var horn in ConfiguredHornOrder())
		{
			if (HornReady(horn) && PetOnHorn(horn) != BeastmasterPet.None && TrickHealsOnHorn(horn))
			{
				return horn;
			}
		}

		return null;
	}

	// Whether a horn with a familiar that heals or protects you comes back within `seconds` (B2, concept 01, A636).
	private bool SafeHornReadyWithin(float seconds)
	{
		foreach (var horn in ThreeHorns)
		{
			if (!HornReady(horn) && PetOnHorn(horn) != BeastmasterPet.None && HornRecastRemaining(horn) <= seconds
				&& HornIsSafe(horn))
			{
				return true;
			}
		}

		return false;
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
		// K5 stage 2 (A644): one that lowers their accuracy by a stated figure, where it lands, too.
		return SafeBy(familiar, EffectiveNature(horn), StopsAttackers(familiar, CurrentTarget) || LowersAttackersAccuracy(familiar),
			BattlehornMasteryIiTrait.EnoughLevel || !TemperedReleasePvE.Cooldown.IsCoolingDown,
			BattlehornMasteryIiiTrait.EnoughLevel || !BorrowPvE.Cooldown.IsCoolingDown);
	}

	// Whether `familiar` heals or protects you on a horn that spends One with Nature on `nature`: its Trick heals, once the
	// player has Trick, or the action the horn spends it on heals, protects, or holds the attackers off (`stops`: stops
	// them, or lowers their accuracy by a stated figure - K5 stage 2, A644), as far as that action comes with the call. One
	// judgement for B2 in a fight and B5 before it.
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
	// K2 (concept 21, A548): the first ready horn, in the configured order, whose familiar hits the weakness of an enemy that
	// came after the fight began.
	private HornOrder? FirstHornForEnemiesThatCame()
	{
		foreach (var horn in ConfiguredHornOrder())
		{
			if (HornReady(horn) && FamiliarOf(PetOnHorn(horn)) is { } familiar && HitsEnemyThatCame(familiar))
			{
				return horn;
			}
		}

		return null;
	}

	private bool SituationalHorn(out IAction? act)
	{
		act = null;
		HornOrder? chosen = null;
		var why = string.Empty;
		// B2 (A636): the danger counts before the pull too - on a board your HP does not return between fights.
		if (InDanger && FirstSafeReadyHorn() is { } safe)
		{
			chosen = safe;
			why = DangerReason;
		}
		else if (InDanger && SafeHornReadyWithin(DataCenter.DefaultGCDTotal))
		{
			// A636: one that heals or protects is back within a GCD - another called now would be sent off for it at once,
			// its horn spent for a second.
			return false;
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
		else if (InCombat && HealingHornWhileFalling(out var fallsIn) is { } healing)
		{
			// B2 (A639): one healing familiar follows another - B3 below would put it last at equal value, after a short
			// stint of another familiar without healing.
			chosen = healing;
			why = $"without healing you fall in {fallsIn:F1} s, before the enemies; its Trick heals";
		}
		else if (InCombat && FirstHornForEnemiesThatCame() is { } came)
		{
			// K2 (concept 21, A548): enemies that came later are here, and a ready familiar hits their weakness.
			chosen = came;
			why = "enemies that came later are weak to it";
		}
		else
		{
			var bestValue = float.MaxValue;
			var bestSafe = true;
			var ready = 0;
			var hasTrick = TrickPvE.Info.EnoughLevelAndQuest();

			// K2 (A548): a familiar that hits only enemies still to come waits for them, while another horn is ready.
			var anotherReady = false;
			foreach (var horn in ConfiguredHornOrder())
			{
				if (HornReady(horn) && FamiliarOf(PetOnHorn(horn)) is { } candidate && !HitsOnlyEnemiesToCome(candidate))
				{
					anotherReady = true;
					break;
				}
			}

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

				if (anotherReady && HitsOnlyEnemiesToCome(familiar))
				{
					continue;
				}

				ready++;
				// Before the player has Trick no familiar brings one: all alike, the configured order decides.
				var value = hasTrick ? TrickValue(familiar, CurrentTarget, EffectiveNature(horn) == OneWithNatureOrder.Tempered) : 0f;
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

		// A2 (A523): the mark of Capture, its landing and the pact, followed while Capture is on.
		if (CaptureFamiliars)
		{
			WatchCapture();
		}

		FollowFamiliarHits(FamiliarLeaving);
		if ((UseCrucibleSafetyItems || UseCrucibleDamageItems) && DataCenter.State)
		{
			UseCrucibleItems(Service.Config.UseHpPotionsPercent, FamiliarLossAhead(Familiar), UseCrucibleSafetyItems,
				UseCrucibleDamageItems);
		}
		CountFight();
	}

	// A576: what happens between fights, RSR on or off - "Turn off after combat ends" turned RSR off 30 s after each fight,
	// and the Crucible's horn screen, team screen, campsite and shop came later; none of them ran (his report, 05.11.2026).
	protected override void UpdateServices()
	{
		base.UpdateServices();

		// B5 (A522): the judgement of a sent assignment every frame, in combat or not.
		_ = BattlehornAssignmentPending;
		// Stage 1 of the Crucible (A548) and what the fights here showed (B5): in a fight they read the hostile list, which
		// is built only while RSR is on or an automatic start is set; out of a fight they follow the board and the area.
		if (!InCombat || DataCenter.HostileListLive)
		{
			FollowCrucible();
			RecordFight();
		}

		// A change by hand, then, out of combat, the horns for the next fight; B5 reads the next enemy from the hostile list.
		WatchHand();
		if (HornsBetweenFights)
		{
			if (DataCenter.HostileListLive)
			{
				AssignBetweenFights();
			}

			SetHornsOnHornScreen();
		}

		FollowTeamRequest(PreferLaggingFamiliars);
		FollowCampsite(PickCampsiteRest);
		FollowCrucibleShop(RecommendCrucibleShop);
		FollowCrucibleTreasure(RecommendCrucibleTreasure);
		FollowCrucibleRoute(RecommendCrucibleRoute);
	}

	#region Battlehorns between fights
	// B5 (concept 01, "Belegung der Hörner in der Rotation", A522): his proposal of 04.11.2026 - in a dungeon the area
	// familiars for the trash, before the boss a combination for one enemy, protection or healing where the fights showed
	// a need for it, set while out of combat.

	// BossSafeFamiliars (B5, A522): in BeastmasterRotation, read by B5, K6 and K7.

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
	private bool _dangerRecorded;
	private bool _dangerKnownChecked;
	private bool _heldByHand;
	private byte[] _slotsSeen = [];
	private HornPlanKey? _planKey;
	private string _planTraced = string.Empty;
	private readonly List<BeastmasterPet> _candidates = [];

	// What the choice reads; a new plan only when one of these changes.
	private readonly record struct HornPlanKey(uint Area, int Level, bool Boss, int Required, int Pack,
		byte First, byte Second, byte Third, int Out, int Candidates, OneWithNatureOrder NatureFirst,
		OneWithNatureOrder NatureSecond, OneWithNatureOrder NatureThird);

	// A familiar on a horn, as the choice weighs it; on a Crucible board with the share of its HP left (1 elsewhere).
	// A637: `Lasts` - its HP stand above the largest loss of one use measured on this board (always outside the Crucible).
	private readonly record struct HornOption(BeastmasterPet Pet, bool Safe, bool Fitting, float Damage, bool TrickHeals,
		float Health = 1f, int Rank = 0, bool Interrupts = false, bool Lasts = true);

	// The order of the choice (concept 01, B5): the required familiars that heal or protect; among them the kind that
	// covers this fight best; the most damage; after a danger in this area more that heal or protect; his familiars on
	// their horns; more that heal or protect; more whose Trick heals. A637: before the damage, more that outlast a use.
	private readonly record struct HornPlanScore(int Required, int Fitting, float Damage, int SafeAfterDanger, int Kept,
		int Safe, int TrickHeals, float Health, int Rank = 0, int Interrupt = 0, int Lasting = 0) : IComparable<HornPlanScore>
	{
		public int CompareTo(HornPlanScore other)
		{
			var order = Required.CompareTo(other.Required);
			order = order != 0 ? order : Fitting.CompareTo(other.Fitting);
			// K4 (A587): before a fight with an enemy kind seen casting interruptibly, a Soul Crush ready to stop it.
			order = order != 0 ? order : Interrupt.CompareTo(other.Interrupt);
			// A637 (concept 21, K6 "Übersteht den Einsatz"): a familiar that could fall in this fight only after those that last.
			order = order != 0 ? order : Lasting.CompareTo(other.Lasting);
			order = order != 0 ? order : Damage.CompareTo(other.Damage);
			order = order != 0 ? order : SafeAfterDanger.CompareTo(other.SafeAfterDanger);
			order = order != 0 ? order : Kept.CompareTo(other.Kept);
			order = order != 0 ? order : Safe.CompareTo(other.Safe);
			order = order != 0 ? order : TrickHeals.CompareTo(other.TrickHeals);
			order = order != 0 ? order : Health.CompareTo(other.Health);
			// A579: on a board, at an otherwise equal choice the stronger by synced rank; elsewhere always equal.
			return order != 0 ? order : Rank.CompareTo(other.Rank);
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
			_dangerRecorded = false;
			_dangerKnownChecked = false;
			_heldByHand = false;
			_planKey = null;
		}

		// A638 (concept 01, B5): a danger seen in this area before, at this level or above, counts from the first pull. Read
		// once the player is there - at the change of area itself the player may still be loading.
		if (!_dangerKnownChecked && Player is { } entering)
		{
			_dangerKnownChecked = true;
			int known;
			lock (OtherConfiguration.DangerousAreasGate)
			{
				known = OtherConfiguration.DangerousAreas.GetValueOrDefault(area);
			}

			if (!_dangerInArea && known > 0 && entering.Level <= known)
			{
				_dangerInArea = true;
				DefenseTrace.Line($"beastmaster: danger seen in this area before, up to level {known} - one more familiar that "
					+ "heals or protects goes on the horns from the first fight here");
			}
		}

		if ((InCombat && InDanger) || Player is { IsDead: true })
		{
			if (!_dangerInArea)
			{
				_dangerInArea = true;
				DefenseTrace.Line("beastmaster: danger seen in this area - one more familiar that heals or protects goes on the "
					+ "horns before each fight here");
			}

			// A638: remembered for this area over every session, with the highest level it came at.
			if (!_dangerRecorded && Player is { } player)
			{
				_dangerRecorded = true;
				var changed = false;
				lock (OtherConfiguration.DangerousAreasGate)
				{
					if (OtherConfiguration.DangerousAreas.GetValueOrDefault(area) < player.Level)
					{
						OtherConfiguration.DangerousAreas[area] = player.Level;
						changed = true;
					}
				}

				if (changed)
				{
					_ = OtherConfiguration.SaveDangerousAreas();
				}
			}
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
		// On a board the game clears the horns after each battle and K6 sets them on the horn screen, which keeps its own
		// watch for a change by hand; B5 does not run there (A580).
		if (IsInCrucible)
		{
			return;
		}

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

	// K5 (A589): a stopping Release protects before a battle only when no enemy of it resists the stop by the sheet.
	private static bool OpenToStop(FamiliarProfile familiar, CrucibleBoard.Enemy[]? battle) => OpenTo(battle, StopOf(familiar));

	// K5 (A589, A644): whether no enemy of the battle resists `ailment` by the sheet; true without a battle.
	private static bool OpenTo(CrucibleBoard.Enemy[]? battle, CrucibleBoard.Ailment ailment)
	{
		if (battle == null)
		{
			return true;
		}

		foreach (var enemy in battle)
		{
			if (CrucibleBoard.Resists(enemy, ailment) == true)
			{
				return false;
			}
		}

		return true;
	}

	// What `pet` brings on `horn` before the next fight: whether it heals or protects you there (a stopping Release only
	// before a pack, C1); whether it is the kind that covers that fight best - its Trick heals before a boss, its Tempered
	// Release stops a pack; the damage of its Trick and, on a horn that spends One with Nature on it, its Tempered Release,
	// each its potency times the enemies its area reaches.
	private HornOption Option(BeastmasterPet pet, HornOrder horn, bool boss, int pack, CrucibleBoard.Enemy[]? battle = null)
	{
		if (FamiliarOf(pet) is not { } familiar)
		{
			return new HornOption(pet, false, false, 0f, false);
		}

		var nature = EffectiveNature(horn);
		var stops = !boss && familiar.ReleaseStops && familiar.ReleaseRadius > 0f && OpenToStop(familiar, battle);
		// K5 stage 2 (A644): a Release that lowers the enemies' accuracy by a stated figure protects where none resists Blind.
		var blinds = familiar.ReleaseLowersAccuracy && OpenTo(battle, CrucibleBoard.Ailment.Blind);
		var safe = SafeBy(familiar, nature, stops || blinds, releaseComes: true, borrowComes: true);
		var trickHeals = familiar.TrickHeals && TrickPvE.Info.EnoughLevelAndQuest();
		var fitting = safe && (boss ? trickHeals : stops && nature == OneWithNatureOrder.Tempered);
		var damage = TrickPvE.Info.EnoughLevelAndQuest()
			? (familiar.TrickPotency > 0f ? familiar.TrickPotency : 1f) * Hits(familiar.TrickRadius, boss, pack)
			: 0f;
		if (nature == OneWithNatureOrder.Tempered)
		{
			damage += familiar.ReleasePotency * Hits(familiar.ReleaseRadius, boss, pack);
		}

		// K6: on a board, the weakness bonus on the share of the familiar's damage whose source strikes each enemy's weakness,
		// over the battle's enemies (K1 over every source, A586).
		if (battle is { Length: > 0 })
		{
			var weak = 0f;
			foreach (var enemy in battle)
			{
				weak += WeakShare(familiar, enemy.Weakness, nature == OneWithNatureOrder.Tempered);
			}

			damage *= 1f + (CrucibleBoard.WeaknessBonus * weak / battle.Length);
		}

		// K4 (A587): a Soulkin familiar on a horn that spends One with Nature on Borrow brings Soul Crush, which interrupts -
		// worth it before a battle whose sheet names a kind seen casting interruptibly.
		var interrupts = battle is { Length: > 0 } && nature == OneWithNatureOrder.Borrow
			&& KinOf(pet) == BeastmasterKinType.Soulkin && CrucibleBoard.CastsInterruptibly(battle);
		return new HornOption(pet, safe, fitting, damage, trickHeals, Interrupts: interrupts);
	}

	// K6 (concept 21, "Stufe 4: K6, Hörner im Hornfenster", A562): on a Crucible board the horn screen before each battle
	// (AgentXBMPetParty active in mode 2). The team in screen order with its HP (SelectedPets, PetCurrentHealth), the chosen
	// positions in horn order (SelectedPetIds), the battle about to start (the stage list's battle entry); a click on a
	// team member sends [1, position] to the screen and toggles it (Daedalus 74659a6, recorded 22.10.2026).
	private bool _hornScreenHeldByHand;
	private int _hornScreenClicks;
	private DateTime _hornScreenClickAt;
	private string _hornScreenSelection = string.Empty;
	private string _hornScreenTraced = string.Empty;
	private bool _hornScreenOpen;
	private bool _hornScreenReady;

	// AgentXBMStageDetailList.AddonInitState: 2 = entries built (FFXIVClientStructs aad0de6) - the game's state, not a tuned value.
	private const uint StageEntriesBuilt = 2;

	private unsafe void SetHornsOnHornScreen()
	{
		var party = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentXBMPetParty.Instance();
		if (!IsInCrucible || InCombat || party == null
			|| !((FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentInterface*)party)->IsAgentActive() || party->Mode != CrucibleBoard.HornScreenMode)
		{
			// A576, A580: a screen that closed without ever being ready says so once.
			if (_hornScreenOpen && !_hornScreenReady)
			{
				TraceHorn("Crucible horn screen: closed before the battle about to start and the team could be read - horns left as they were");
			}

			_hornScreenOpen = false;
			_hornScreenReady = false;
			_hornScreenHeldByHand = false;
			_hornScreenClicks = 0;
			_hornScreenSelection = string.Empty;
			return;
		}

		_hornScreenOpen = true;
		var team = party->SelectedPets.AsSpan();
		var current = party->PetCurrentHealth;
		var max = party->PetMaxHealth;
		var selected = new List<int>();
		foreach (var chosen in party->SelectedPetIds.AsSpan())
		{
			selected.Add((int)chosen.PetId);
		}

		// A580: the screen is ready once the field agent has built its entries, the battle about to start reads and the
		// window shows. Before, it still holds the last battle's horn positions (on 05.11.2026 four of five screens opened
		// with them, the stage list not yet built, and cleared them 55 ms later) - what is seen then is not the player's.
		var stages = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentXBMStageDetailList.Instance();
		var battle = CrucibleBoard.UpcomingBattle();
		var addon = (FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)ECommons.DalamudServices.Svc.GameGui.GetAddonByName("XBMPetParty").Address;
		if (stages == null || stages->AddonInitState < StageEntriesBuilt || battle == 0 || team.Length == 0
			|| addon == null || !addon->IsVisible)
		{
			_hornScreenSelection = string.Empty;
			return;
		}

		_hornScreenReady = true;
		var selection = string.Join(",", selected);
		var now = DateTime.Now;
		var clickPending = _hornScreenClicks > 0 && (now - _hornScreenClickAt).TotalSeconds < DataCenter.DefaultGCDTotal;
		// Emptied before RSR's first click is still the game clearing the last battle's horns, not a choice.
		var gameCleared = selection.Length == 0 && _hornScreenClicks == 0;
		if (_hornScreenSelection.Length > 0 && selection != _hornScreenSelection && !clickPending && !gameCleared && !_hornScreenHeldByHand)
		{
			_hornScreenHeldByHand = true;
			TraceHorn($"Crucible horn screen: changed by hand to positions {selection} - kept for this battle");
		}

		if (_hornScreenHeldByHand || (clickPending && selection == _hornScreenSelection))
		{
			return;
		}

		_hornScreenSelection = selection;

		var enemies = CrucibleBoard.EnemiesOf(battle);
		var boss = CrucibleBoard.IsFinalBattle(battle);
		var required = (boss ? BossSafeFamiliars : 0) + (_dangerInArea ? 1 : 0);
		var pack = boss ? 1 : Math.Max(1, enemies.Length);
		// Each horn spends One with Nature by its own setting, so each weighs the team on its own, as B5 does.
		var none = new HornOption(BeastmasterPet.None, false, false, 0f, false, 0f, Lasts: false);
		var options = new List<HornOption>[Horns.Length];
		var position = new Dictionary<BeastmasterPet, int>();
		for (var h = 0; h < Horns.Length; h++)
		{
			options[h] = [];
		}

		// A579: a familiar below the rank the game recommends for this board fights only when fewer others live than horns.
		// K11 (A585): with the HP it last left a fight with, while the agent has not taken that fight over.
		var recommended = CrucibleBoard.RecommendedRank(party->ContentId);
		var adequateLiving = 0;
		for (var i = 0; i < team.Length; i++)
		{
			var pet = (BeastmasterPet)team[i].PetId;
			if (i < current.Length && EffectiveHealth(pet, current[i]) > 0
				&& (party->IsPetOverSyncedRank((uint)pet) || party->GetSyncedRank((uint)pet) >= recommended))
			{
				adequateLiving++;
			}
		}

		var candidates = new List<(BeastmasterPet Pet, int Position, uint Hp, uint Full, int Rank)>();
		for (var i = 0; i < team.Length; i++)
		{
			var pet = (BeastmasterPet)team[i].PetId;
			var hp = i < current.Length ? EffectiveHealth(pet, current[i]) : 0u;
			var full = i < max.Length ? max[i] : 0u;
			if (hp == 0 || full == 0 || FamiliarOf(pet) is null || position.ContainsKey(pet))
			{
				continue;
			}

			var rank = (int)party->GetSyncedRank((uint)pet);
			if (adequateLiving >= Horns.Length && !party->IsPetOverSyncedRank((uint)pet) && rank < recommended)
			{
				continue;
			}

			position[pet] = i;
			candidates.Add((pet, i, hp, full, rank));
		}

		var held = boss ? [] : HeldForFinal(party->ContentId, candidates, boss, pack, enemies, required, none);
		foreach (var (pet, _, hp, full, rank) in candidates)
		{
			if (held.Contains(pet))
			{
				continue;
			}

			for (var h = 0; h < Horns.Length; h++)
			{
				options[h].Add(Option(pet, Horns[h], boss, pack, enemies) with
				{
					Health = hp / (float)full,
					Rank = rank,
					Lasts = _largestLossPerUse == 0u || hp > _largestLossPerUse,
				});
			}
		}

		for (var h = 0; h < Horns.Length; h++)
		{
			// A team smaller than the horns leaves a horn empty.
			if (options[h].Count <= h)
			{
				options[h].Add(none);
			}
		}

		var currentPets = new BeastmasterPet[Horns.Length];
		for (var h = 0; h < Horns.Length; h++)
		{
			currentPets[h] = h < selected.Count && selected[h] < team.Length ? (BeastmasterPet)team[selected[h]].PetId : BeastmasterPet.None;
		}

		var best = BestPlan(options, currentPets, required, none);

		var wanted = new List<int>();
		foreach (var option in best)
		{
			if (option.Pet != BeastmasterPet.None)
			{
				wanted.Add(position[option.Pet]);
			}
		}

		var prefix = selected.Count <= wanted.Count;
		for (var i = 0; prefix && i < selected.Count; i++)
		{
			prefix = selected[i] == wanted[i];
		}

		var plan = $"{battle}:{string.Join(",", wanted)}";
		if (prefix && selected.Count == wanted.Count)
		{
			if (plan != _hornScreenTraced)
			{
				_hornScreenTraced = plan;
				TraceHorn($"Crucible horn screen: horns set for battle {battle} ({(boss ? "final fight" : $"{enemies.Length} enemies")}, "
					+ $"{required} that heal or protect): {best[0].Pet}, {best[1].Pet}, {best[2].Pet}"
					+ (held.Count > 0 ? $"; held for the final fight, a use costing up to {_largestLossPerUse} HP: {string.Join(", ", held)}" : string.Empty)
					+ (InterrupterHeld.Overlaps(held) ? $" (for a later battle's interrupt: {string.Join(", ", InterrupterHeld)})" : string.Empty));
			}

			return;
		}

		if (_hornScreenClicks >= team.Length * Horns.Length)
		{
			if (plan != _hornScreenTraced)
			{
				_hornScreenTraced = plan;
				TraceHorn($"Crucible horn screen: gave up after {_hornScreenClicks} clicks - set the horns by hand");
			}

			return;
		}

		// A wrong order is cleared from the back and picked again: horn order is the order of selection.
		var click = prefix ? wanted[selected.Count] : selected[^1];
		ReadOnlySpan<int> sent = [1, click];
		var values = stackalloc FFXIVClientStructs.FFXIV.Component.GUI.AtkValue[sent.Length];
		for (var i = 0; i < sent.Length; i++)
		{
			values[i] = default;
			values[i].Type = FFXIVClientStructs.FFXIV.Component.GUI.AtkValueType.Int;
			values[i].Int = sent[i];
		}

		addon->FireCallback((uint)sent.Length, values, true);
		_hornScreenClicks++;
		_hornScreenClickAt = now;
	}

	// The plan of three horns the score ranks first, from each horn's options; a familiar on one horn only.
	private HornOption[] BestPlan(List<HornOption>[] options, BeastmasterPet[] currentPets, int required, HornOption none)
	{
		HornPlanScore? bestScore = null;
		var best = new[] { none, none, none };
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

					var score = Score(a, b, c, currentPets, required);
					if (bestScore is not { } known || score.CompareTo(known) > 0)
					{
						bestScore = score;
						best = [a, b, c];
					}
				}
			}
		}

		return best;
	}

	// K11 (concept 21, "K11 – Begleiter über das Brett planen", stage 1, A585): before a fight that is not the board's final, the familiars
	// that heal or protect in the plan the final fight would get now - its reserve -, held off this fight when one more use
	// as large as the largest of the board would leave less than another such use for the final; as long as without them
	// the horns fill and as many heal or protect as this fight requires. Else the one with the most HP comes back first.
	// None before a use has been measured.
	// K11 stage 3 (A643): the Soulkin familiars the reserve keeps for a later battle's interrupt, for the trace.
	private static readonly HashSet<BeastmasterPet> InterrupterHeld = [];

	// K11 stage 3 (A643): the only Soulkin familiar of the living team and its HP, when a horn spends One with Nature on
	// Borrow, the battle about to start has no enemy kind seen casting interruptibly and a battle still reachable on the board
	// has one (all the board's battles while its ways are unread); else null.
	private (BeastmasterPet Pet, uint Hp)? SoleInterrupterAhead(uint board,
		List<(BeastmasterPet Pet, int Position, uint Hp, uint Full, int Rank)> candidates, CrucibleBoard.Enemy[] enemies)
	{
		if (CrucibleBoard.CastsInterruptibly(enemies))
		{
			return null;
		}

		var borrowHorn = false;
		foreach (var horn in Horns)
		{
			borrowHorn |= EffectiveNature(horn) == OneWithNatureOrder.Borrow;
		}

		(BeastmasterPet Pet, uint Hp)? soul = null;
		foreach (var candidate in candidates)
		{
			if (KinOf(candidate.Pet) == BeastmasterKinType.Soulkin)
			{
				if (soul != null)
				{
					return null;
				}

				soul = (candidate.Pet, candidate.Hp);
			}
		}

		if (soul == null || !borrowHorn)
		{
			return null;
		}

		IEnumerable<uint> ahead = CrucibleRoute.BattlesAhead() is { } reachable ? reachable : CrucibleBoard.FightsOfBoard(board);
		foreach (var battle in ahead)
		{
			if (CrucibleBoard.CastsInterruptibly(CrucibleBoard.EnemiesOf(battle)))
			{
				return soul;
			}
		}

		return null;
	}

	private HashSet<BeastmasterPet> HeldForFinal(uint board, List<(BeastmasterPet Pet, int Position, uint Hp, uint Full, int Rank)> candidates,
		bool boss, int pack, CrucibleBoard.Enemy[] enemies, int required, HornOption none)
	{
		var held = new HashSet<BeastmasterPet>();
		InterrupterHeld.Clear();
		var loss = _largestLossPerUse;
		var fights = CrucibleBoard.FightsOfBoard(board);
		if (fights.Count == 0 || !CrucibleBoard.IsFinalBattle(fights[0]))
		{
			return held;
		}

		var final = CrucibleBoard.EnemiesOf(fights[0]);
		var finalOptions = new List<HornOption>[Horns.Length];
		for (var h = 0; h < Horns.Length; h++)
		{
			finalOptions[h] = [];
			foreach (var (pet, _, hp, full, rank) in candidates)
			{
				finalOptions[h].Add(Option(pet, Horns[h], true, 1, final) with { Health = hp / (float)full, Rank = rank });
			}

			if (finalOptions[h].Count <= h)
			{
				finalOptions[h].Add(none);
			}
		}

		var nobody = new BeastmasterPet[Horns.Length];
		var finalRequired = BossSafeFamiliars + (_dangerInArea ? 1 : 0);
		var reserve = new List<(BeastmasterPet Pet, uint Hp)>();
		FinalHealers.Clear();
		FinalTeam.Clear();
		FinalLossPerUse = loss;
		FinalBattle = fights[0];
		foreach (var option in BestPlan(finalOptions, nobody, finalRequired, none))
		{
			if (option.Pet != BeastmasterPet.None)
			{
				// K14 (A631): the final fight's plan as a whole - the campsite and the feed before it read it.
				FinalTeam.Add(option.Pet);
				if (option.Safe)
				{
					_ = FinalHealers.Add(option.Pet);
				}
			}
		}

		FinalTeam.Sort((a, b) => FinalHealers.Contains(b).CompareTo(FinalHealers.Contains(a)));

		// The reserve waits for a measured use (A585); the plan above does not.
		if (loss == 0u)
		{
			return held;
		}

		foreach (var candidate in candidates)
		{
			if (ShortForFinal(candidate.Pet, candidate.Hp))
			{
				reserve.Add((candidate.Pet, candidate.Hp));
			}
		}

		// K11 stage 3 (concept 21, A643): the team's only Soulkin familiar - Soul Crush over Borrow, the board's only
		// interrupt - when a battle still reachable has an enemy kind seen casting interruptibly and this one has none; by
		// the same measure as the final fight's healers.
		if (SoleInterrupterAhead(board, candidates, enemies) is { } soul && (soul.Hp < loss || soul.Hp - loss < loss)
			&& !reserve.Exists(r => r.Pet == soul.Pet))
		{
			reserve.Add(soul);
			_ = InterrupterHeld.Add(soul.Pet);
		}

		// The one with the least HP is held longest: released from the most HP down.
		reserve.Sort((a, b) => a.Hp.CompareTo(b.Hp));
		foreach (var (pet, _) in reserve)
		{
			_ = held.Add(pet);
		}

		while (held.Count > 0 && !Enough())
		{
			_ = held.Remove(reserve[^1].Pet);
			reserve.RemoveAt(reserve.Count - 1);
		}

		return held;

		bool Enough()
		{
			var left = 0;
			var safe = 0;
			foreach (var (pet, _, _, _, _) in candidates)
			{
				if (held.Contains(pet))
				{
					continue;
				}

				left++;
				for (var h = 0; h < Horns.Length; h++)
				{
					if (Option(pet, Horns[h], boss, pack, enemies).Safe)
					{
						safe++;
						break;
					}
				}
			}

			return left >= Math.Min(Horns.Length, candidates.Count) && safe >= required;
		}
	}

	// The enemies an area reaches in the next fight: one for a single target or a boss, else the pack - every enemy of
	// it within the area, as the model assumes (a tank gathers them).
	private static int Hits(float radius, bool boss, int pack)
	{
		return radius <= 0f || boss ? 1 : pack;
	}

	private HornPlanScore Score(HornOption a, HornOption b, HornOption c, BeastmasterPet[] current, int required)
	{
		// A637: one that could fall in this fight heals or protects only until it falls - it counts as neither.
		var safe = (a.Safe && a.Lasts ? 1 : 0) + (b.Safe && b.Lasts ? 1 : 0) + (c.Safe && c.Lasts ? 1 : 0);
		var lasting = (a.Lasts ? 1 : 0) + (b.Lasts ? 1 : 0) + (c.Lasts ? 1 : 0);
		var fitting = (a.Fitting ? 1 : 0) + (b.Fitting ? 1 : 0) + (c.Fitting ? 1 : 0);
		var kept = (a.Pet == current[I] ? 1 : 0) + (b.Pet == current[II] ? 1 : 0) + (c.Pet == current[III] ? 1 : 0);
		var healers = (a.TrickHeals ? 1 : 0) + (b.TrickHeals ? 1 : 0) + (c.TrickHeals ? 1 : 0);
		var interrupts = a.Interrupts || b.Interrupts || c.Interrupts ? 1 : 0;
		return new HornPlanScore(Math.Min(safe, required), required > 0 ? Math.Min(fitting, 1) : 0,
			a.Damage + b.Damage + c.Damage, _dangerInArea ? safe : 0, kept, safe, healers, a.Health + b.Health + c.Health,
			a.Rank + b.Rank + c.Rank, interrupts, lasting);
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

		// A558: on a board, before the axes when the target's water weakness makes it worth more than the combo's mean step.
		if (QuellingWavePvE.CanUse(out act, skipTargetStatusNeedCheck: true) && WaveBeatsCombo(QuellingWavePvE.Target.Target))
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

		// A558: as damage when no axe reaches the target - Quelling Wave reaches 30 yalms, the axes are melee; in melee it
		// would cost about 70 potency against the combo's mean step.
		if (QuellingWavePvE.CanUse(out act, skipTargetStatusNeedCheck: true))
		{
			return true;
		}
		return base.GeneralGCD(out act);
	}
	#endregion
}