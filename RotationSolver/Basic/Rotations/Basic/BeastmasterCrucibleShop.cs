using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Dalamud.Game;
using ECommons.DalamudServices;
using ECommons.UIHelpers.AtkReaderImplementations;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K10 of the Crucible (concept 21, "K10 – Zwischen den Kämpfen", Laden, A569): while a shop on a board is open, what to
/// buy - safety first, as far as tokens and slots go - and on request the purchases one row at a time. Nothing is bought
/// without the click.
/// </summary>
public partial class BeastmasterRotation
{
	/// <summary>One recommended purchase: the shop row, the item, its price, and why.</summary>
	public readonly record struct CrucibleShopPick(int Row, string Name, int Price, string Why);

	// What a piece of beast gear does, read once from its English effect text (XBMItem): the shares it changes - maximum HP,
	// damage taken, physical and magic vulnerability, evasion, movement speed, slow, HP recovery, HP at the end of a
	// battle - whether it revives you, whether it inflicts a status on you, and the ailments it makes you immune to.
	private readonly record struct CrucibleGear(string Name, float MaxHp, float DamageTaken, float PhysicalVulnerability,
		float MagicVulnerability, float Evasion, float Movement, float Slow, float Recovery, float Curing, bool Life,
		bool SelfHarm, List<string> Resists, string RestItem = "", float EscapeChance = 0f, int TreasureItems = 0,
		string KillStack = "", float KillStackTaken = 0f, int KillStackMax = 0);

	// A677: gear that grants a stack on each kill whose effect lowers the damage you take (Demonic Armor: "Grants one stack
	// of High Wire upon defeating an enemy. High Wire Effect: ... reduces damage taken by 5% ... Maximum Stacks: 4").
	private static readonly Regex GearKillStack = new(
		@"Grants one stack of (?<status>[A-Z][A-Za-z' ]*?) upon defeating an enemy\.[\s\S]*?\k<status> Effect: [^\n]*?reduces damage taken by (?<share>\d+)%[\s\S]*?Maximum Stacks: (?<max>\d+)",
		RegexOptions.CultureInvariant);

	/// <summary>
	/// K8 "Auslöser" (A677): the stacking status the worn gear grants on a kill, the share of damage taken each stack takes
	/// off, and the most stacks; null without such gear.
	/// </summary>
	internal static (string Status, float Taken, int Max)? WornKillStack()
	{
		if (_gearSeen is not { } seen || seen.Run != CrucibleBoard.Run)
		{
			return null;
		}

		var gear = CrucibleGearTable;
		foreach (var id in seen.Gear)
		{
			if (gear.TryGetValue(id, out var piece) && piece.KillStack.Length > 0 && piece.KillStackMax > 0)
			{
				return (piece.KillStack, piece.KillStackTaken, piece.KillStackMax);
			}
		}

		return null;
	}

	// A671: gear that offers escape from enemy spaces (Coward's Knife) and that grants more items on treasure spaces (Thief's
	// Knife: "Grants two items on treasure spaces").
	private static readonly Regex GearEscape = new(@"(?<chance>\d+)% chance to offer escape from an enemy space", RegexOptions.CultureInvariant);
	private static readonly Regex GearTreasureItems = new(@"Grants (?<count>[a-z]+|\d+) items on treasure spaces", RegexOptions.CultureInvariant);

	// A671: what the gear the run wears does on the way - the chance an enemy space offers escape, and the items a treasure
	// space grants (1 without a piece that grants more).
	internal static (float EscapeChance, int TreasureItems) WornWayEffects()
	{
		var chance = 0f;
		var treasure = 1;
		if (_gearSeen is { } seen && seen.Run == CrucibleBoard.Run)
		{
			var gear = CrucibleGearTable;
			foreach (var id in seen.Gear)
			{
				if (gear.TryGetValue(id, out var piece))
				{
					chance = Math.Max(chance, piece.EscapeChance);
					treasure = Math.Max(treasure, piece.TreasureItems);
				}
			}
		}

		return (chance, treasure);
	}

	// A669: gear that brings an item at each rest - "Procures one random beast potion upon resting at campsites".
	private static readonly Regex GearRestItem = new(@"Procures one random (?<item>[a-z][a-z' -]*?) upon resting at campsites",
		RegexOptions.CultureInvariant);

	/// <summary>
	/// K13 (A669): the least an item of the kind <paramref name="kind"/> heals ("beast potion": G1 to G4 Beast Potion, the
	/// G1's 10 %) - which one a rest brings is chance; 0 where no healing item of that kind is known.
	/// </summary>
	private static float LeastHealOf(string kind)
	{
		var least = 0f;
		foreach (var item in CrucibleItems.Values)
		{
			if (item.Kind == CrucibleItemKind.Heal && item.Name.EndsWith(kind, StringComparison.OrdinalIgnoreCase)
				&& (least <= 0f || item.Value < least))
			{
				least = item.Value;
			}
		}

		return least;
	}

	private static readonly Regex GearStat = new(
		@"(?<stat>Maximum HP|Damage Taken|Physical Vulnerability|Magic Vulnerability|Evasion|Movement Speed|Slow) (?<sign>[+-])(?<share>\d+)%",
		RegexOptions.CultureInvariant);
	private static readonly Regex GearRecovery = new(@"Increases HP recovery via actions and crucible items by (?<share>\d+)%",
		RegexOptions.CultureInvariant);
	private static readonly Regex GearCuring = new(@"Recovers (?<share>\d+)% of maximum HP at the end of battles",
		RegexOptions.CultureInvariant);
	private static readonly Regex GearResists = new(@"(?<ailment>[A-Z][a-z]+) Resistance \+\d+%", RegexOptions.CultureInvariant);
	// "to inflict Stun on self" (Thunder Axe) as well as "Keen Edge Effect: Inflicts damage over time on self" (Wind Mask).
	private static readonly Regex GearSelfHarm = new(@"\binflicts? [^.]*? on self", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

	// Buying row r is the shop's callback [2, r] (Daedalus 74659a6, recorded 22.10.2026; BeastHelper be40a1e, after AutoDuty's
	// Screens.ItemShop.Buy): the game's command of XBMContentsItemShop, checked after each click at the row's sold flag.
	private const int ShopBuyCallback = 2;

	private static Dictionary<ushort, CrucibleGear>? _crucibleGear;

	private static List<int>? _shopWanted;
	private static (int Row, DateTime At)? _shopLastBuy;
	private static (int Coins, int Bought, int Gear, int Items, uint Board, double Fights, float Stretch, int Feed)? _shopKey;
	private static string _shopTraced = string.Empty;

	// K13 (A578): the gear the last shop of this run showed held, with the slots - the treasure window does not list it.
	// A662: by run (CrucibleBoard.Run), not by area - a new run on the same board is the same area and starts without gear.
	private static (int Run, List<ushort> Gear, int Slots)? _gearSeen;

	/// <summary>
	/// K10 Campsite (A661): what the gear worn in this run makes of the player's defences - his maximum HP (1 + its raises),
	/// the damage he takes, and his physical and magic vulnerability with evasion, each as a factor, multiplied over the
	/// pieces as the shop weighs them; all 1 before the run shows any gear.
	/// </summary>
	internal static (float Hp, float Taken, float Physical, float Magic) WornFactors()
	{
		float hp = 1f, taken = 1f, physical = 1f, magic = 1f;
		if (_gearSeen is not { } seen || seen.Run != CrucibleBoard.Run)
		{
			return (hp, taken, physical, magic);
		}

		var gear = CrucibleGearTable;
		foreach (var id in seen.Gear)
		{
			if (gear.TryGetValue(id, out var piece))
			{
				hp *= 1f + piece.MaxHp;
				taken *= 1f - piece.DamageTaken;
				physical *= (1f - piece.PhysicalVulnerability) * (1f - piece.Evasion);
				magic *= 1f - piece.MagicVulnerability;
			}
		}

		return (hp, taken, physical, magic);
	}

	/// <summary>
	/// A666: whether the gear this run wears holds a piece of the English name <paramref name="name"/> ("crimson ribbon"), as an
	/// item's text names it after "When carrying".
	/// </summary>
	private static bool WearsGearNamed(string name)
	{
		if (name.Length == 0 || _gearSeen is not { } seen || seen.Run != CrucibleBoard.Run)
		{
			return false;
		}

		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>(ClientLanguage.English);
		foreach (var id in seen.Gear)
		{
			if (sheet?.GetRowOrDefault(id) is { } row && string.Equals(row.Unknown2.ExtractText(), name, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	// A673: whether the gear this run wears makes the player immune to the status named ("Petrification Resistance +100%").
	internal static bool WornResists(string status)
	{
		if (_gearSeen is not { } seen || seen.Run != CrucibleBoard.Run)
		{
			return false;
		}

		var gear = CrucibleGearTable;
		foreach (var id in seen.Gear)
		{
			if (gear.TryGetValue(id, out var piece) && GearResistsAilment(piece, status))
			{
				return true;
			}
		}

		return false;
	}

	// A668: whether gear holds a piece of the English name an item's text names after "When carrying" ("crimson ribbon").
	private static bool Carries(List<CrucibleGear> gear, string name)
	{
		if (name.Length == 0)
		{
			return false;
		}

		foreach (var piece in gear)
		{
			if (string.Equals(piece.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	// A668: whether an item held grants the status <paramref name="status"/> (Spirit of the Breathtaker).
	private static bool GrantsHeld(List<ushort> held, string status)
	{
		if (status.Length == 0)
		{
			return false;
		}

		foreach (var id in held)
		{
			if (CrucibleItems.TryGetValue(id, out var item) && item.Kind == CrucibleItemKind.Buff
				&& string.Equals(item.Status, status, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// K13 (A668): what a protective item catches in the heaviest fight, as a share of your HP - the mitigation its share of the
	/// damage in the best window of its duration, the reflection the magic damage in it, Blink the physical hits in a row, as
	/// many as its stacks -, its duration as the gear gives it and its stacks with the items held; and that duration, those
	/// stacks and the share of the fight's damage of its kind caught.
	/// </summary>
	private static (float Value, double Seconds, int Stacks, float Share) ProtectionValue(CrucibleItem item, List<CrucibleGear> gear,
		List<ushort> held, (float Heavy, float HeavyPhysical, float HeavyMagic) load)
	{
		var seconds = item.LongerDuration > 0f && Carries(gear, item.LongerWith) ? item.LongerDuration
			: item.Duration > 0f ? item.Duration : double.MaxValue;
		var stacks = item.Stacks + (item.MoreStacks > 0 && GrantsHeld(held, item.GainsFrom) ? item.MoreStacks : 0);
		var (heavy, kind) = item.Kind switch
		{
			CrucibleItemKind.Mitigate => (item.Value * load.Heavy, (CrucibleBoard.HitKind?)null),
			CrucibleItemKind.NullifyPhysical => (load.HeavyPhysical, CrucibleBoard.HitKind.Physical),
			CrucibleItemKind.ReflectMagic => (load.HeavyMagic, CrucibleBoard.HitKind.Magic),
			_ => (0f, null),
		};

		if (heavy <= 0f)
		{
			return (0f, seconds, stacks, 0f);
		}

		var share = CrucibleBoard.CaughtShare(seconds, item.Kind == CrucibleItemKind.NullifyPhysical ? stacks : 0, kind);
		return (heavy * share, seconds, stacks, share);
	}

	/// <summary>
	/// K13 (A670): what an Auto-potion item (Beast Potion Kit) is worth, as a share of your HP - of what you can still heal,
	/// <paramref name="healable"/>, the part that would take you below its line, of that the share of the heaviest fight in
	/// its duration from the start (as the gear gives it), and of that the expected restores: the first sure, each further one
	/// with the chance the effect did not expire (less under the status it gains from, as held), each at most what is left;
	/// and that duration and chance.
	/// </summary>
	private static (float Value, double Seconds, float Expires) AutoPotionValue(CrucibleItem item, List<CrucibleGear> gear,
		List<ushort> held, float healable, float recovery)
	{
		var seconds = item.LongerDuration > 0f && Carries(gear, item.LongerWith) ? item.LongerDuration
			: item.Duration > 0f ? item.Duration : double.MaxValue;
		var expires = item.ExpiresWith > 0f && GrantsHeld(held, item.GainsFrom) ? item.ExpiresWith : item.Expires;
		var restores = item.Value * (1f + recovery);
		var below = Math.Max(0f, healable - (1f - item.AutoBelow)) * CrucibleBoard.ShareFromStart(seconds);
		var value = 0f;
		var chance = 1f;
		while (restores > 0f && below > 0f && chance > 0f)
		{
			value += chance * Math.Min(restores, below);
			below -= restores;
			chance *= 1f - expires;
		}

		return (value, seconds, expires);
	}

	/// <summary>Whether a shop is open and the recommendation runs, for the window.</summary>
	public static bool CrucibleShopOpen { get; private set; }

	/// <summary>What the protective gear is weighed against: the damage measured here, or the board's enemies in the sheet.</summary>
	public static string CrucibleShopBasis { get; private set; } = string.Empty;

	/// <summary>The purchases RSR recommends while a shop is open, computed in the update; empty while none is open.</summary>
	public static IReadOnlyList<CrucibleShopPick> CrucibleShopRecommendation { get; private set; } = [];

	/// <summary>The tokens the open shop shows, and what the recommendation leaves of them.</summary>
	public static (int Tokens, int Left) CrucibleShopTokens { get; private set; }

	/// <summary>What the purchases did last, for the window.</summary>
	public static string CrucibleShopStatus { get; private set; } = string.Empty;

	/// <summary>K10: buy the recommended rows, one at a time, each checked before the next.</summary>
	public static void RequestCrucibleShop()
	{
		var rows = new List<int>();
		foreach (var pick in CrucibleShopRecommendation)
		{
			rows.Add(pick.Row);
		}

		_shopWanted = rows;
		_shopLastBuy = null;
		CrucibleShopStatus = rows.Count == 0 ? "nothing to buy" : $"buying {rows.Count}...";
	}

	/// <summary>
	/// K10: every frame with the shop open - the recommendation (when <paramref name="recommend"/>), and the purchases asked
	/// for with the window's button.
	/// </summary>
	internal static unsafe void FollowCrucibleShop(bool recommend)
	{
		var shop = (AtkUnitBase*)Svc.GameGui.GetAddonByName("XBMContentsItemShop").Address;
		if (!recommend || !DataCenter.IsInCrucible || InCombat || shop == null || !shop->IsVisible)
		{
			CrucibleShopOpen = false;
			CrucibleShopRecommendation = [];
			_shopWanted = null;
			_shopLastBuy = null;
			_shopKey = null;
			_shopTraced = string.Empty;
			CrucibleShopStatus = string.Empty;
			return;
		}

		CrucibleShopOpen = true;
		var reader = new ReaderXBMContentsItemShop(shop);
		var stock = reader.StockEntries;
		var prompt = (AddonSelectYesno*)Svc.GameGui.GetAddonByName("SelectYesno").Address;
		if (_shopWanted is { } wanted)
		{
			if (prompt != null && prompt->IsVisible)
			{
				// The game asks: his answer decides. The wait for the row starts once the prompt has closed.
				if (_shopLastBuy is { } last)
				{
					_shopLastBuy = (last.Row, DateTime.Now);
				}

				CrucibleShopStatus = "the game asks - Yes buys on, No stops";
			}
			else
			{
				BuyNext(shop, stock, wanted);
			}
		}

		RecommendPurchases(reader, stock);
	}

	private static unsafe void BuyNext(AtkUnitBase* shop, List<ReaderXBMContentsItemShop.StockEntry> stock, List<int> wanted)
	{
		if (_shopLastBuy is { } last)
		{
			var bought = last.Row < stock.Count && stock[last.Row].Bought;
			if (!bought && (DateTime.Now - last.At).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}

			_shopLastBuy = null;
			if (!bought)
			{
				_shopWanted = null;
				CrucibleShopStatus = $"row {last.Row} was not taken - stopped";
				DefenseTrace.Line($"crucible shop: row {last.Row} not bought after the click - stopped");
				return;
			}

			_ = wanted.Remove(last.Row);
			DefenseTrace.Line($"crucible shop: bought row {last.Row}");
		}

		if (wanted.Count == 0)
		{
			_shopWanted = null;
			CrucibleShopStatus = "bought";
			return;
		}

		var row = wanted[0];
		if (row >= stock.Count || stock[row].Bought || !stock[row].Listed)
		{
			_ = wanted.Remove(row);
			return;
		}

		ReadOnlySpan<int> sent = [ShopBuyCallback, row];
		var values = stackalloc AtkValue[sent.Length];
		for (var i = 0; i < sent.Length; i++)
		{
			values[i] = default;
			values[i].Type = AtkValueType.Int;
			values[i].Int = sent[i];
		}

		shop->FireCallback((uint)sent.Length, values, true);
		_shopLastBuy = (row, DateTime.Now);
	}

	/// <summary>
	/// K10 (A569, A570): the purchases, safety first, while tokens and slots last: a second life while none is held; cover
	/// against each status of the board's fights that ends in death; protective and healing gear by the HP the player comes
	/// out with from a stretch of fights that ends with the heaviest; cover against the other statuses of the board's fights
	/// that cost safety; healing items, the strongest first; protective items. Never feed, never gear that inflicts a status
	/// on its bearer, nothing for damage.
	/// </summary>
	private static unsafe void RecommendPurchases(ReaderXBMContentsItemShop reader, List<ReaderXBMContentsItemShop.StockEntry> stock)
	{
		var budget = CountIn(reader.CoinsString);
		var gear = CrucibleGearTable;

		// What is held: the items in the director's ten slots, the gear the shop lists as owned.
		var heldItems = new List<ushort>();
		var itemSlots = 0;
		var director = EventFramework.Instance() is var framework && framework != null ? framework->GetInstanceContentDirector() : null;
		if (director != null && director->InstanceContentType == InstanceContentType.CrucibleOfTheUnbroken)
		{
			var crucible = (InstanceContentCrucibleOfTheUnbroken*)director;
			itemSlots = crucible->Inventory.Length;
			for (var i = 0; i < itemSlots; i++)
			{
				if (crucible->Inventory[i].ItemId != 0)
				{
					heldItems.Add(crucible->Inventory[i].ItemId);
				}
			}
		}

		var owned = new List<CrucibleGear>();
		var gearSlots = 0;
		var seen = new List<ushort>();
		foreach (var piece in reader.OwnedEntries)
		{
			gearSlots++;
			if (piece.Owned && piece.Id <= ushort.MaxValue)
			{
				seen.Add((ushort)piece.Id);
				if (gear.TryGetValue((ushort)piece.Id, out var held))
				{
					owned.Add(held);
				}
			}
		}

		// What RSR took from a treasure in this run counts as worn, should the shop's list not show it yet (A657, A662).
		if (_gearSeen is { } before && before.Run == CrucibleBoard.Run)
		{
			foreach (var id in before.Gear)
			{
				if (!seen.Contains(id))
				{
					seen.Add(id);
					if (gear.TryGetValue(id, out var taken))
					{
						owned.Add(taken);
					}
				}
			}
		}

		// Weighed again only when something it weighs has changed: tokens, purchases, what is held, the board, the fights.
		var board = AgentXBMStageDetailList.Instance() is var stages && stages != null ? stages->ContentId : 0;
		var loads = CrucibleBoard.Loads;
		var stretch = CrucibleBoard.FightsPerStretch;
		var boughtRows = 0;
		foreach (var entry in stock)
		{
			boughtRows += entry.Bought ? 1 : 0;
		}

		var key = (budget, boughtRows, owned.Count, heldItems.Count, board,
			loads is { } measured ? measured.Mean.Damage + measured.Heaviest.Damage + measured.Mean.Healed : 0d, stretch, _feedSlotsSeen);
		if (_shopKey == key)
		{
			return;
		}

		_shopKey = key;

		// Remembered for the treasure (K13, A578): the gear held in this run, and how many slots there are.
		_gearSeen = (CrucibleBoard.Run, seen, gearSlots);

		var offers = new List<(int Row, ushort Item, int Price)>();
		for (var r = 0; r < stock.Count; r++)
		{
			if (stock[r].Listed && !stock[r].Bought && stock[r].Item is > 0 and <= ushort.MaxValue)
			{
				offers.Add((r, (ushort)stock[r].Item, CountIn(stock[r].PriceString.TextValue)));
			}
		}

		var picks = RankOffers(offers, budget, int.MaxValue, owned, gearSlots, heldItems, itemSlots, board, out var spent);
		CrucibleShopRecommendation = picks;
		CrucibleShopTokens = (budget, budget - spent);
		RecommendFeed(offers, budget - spent);
		// A683: what the shop offered beside what RSR picked - on 08.11.2026 nothing against the Doom of board 2 was
		// recommended, and the trace could not tell whether the shop held any.
		var offered = new List<string>(offers.Count);
		foreach (var (_, item, price) in offers)
		{
			offered.Add($"{OfferName(item)} {price}");
		}

		var traced = string.Join(", ", picks.ConvertAll(static pick => pick.Name)) + " | " + string.Join(", ", offered);
		if (traced != _shopTraced)
		{
			_shopTraced = traced;
			DefenseTrace.Line($"crucible shop: {budget} tokens, weighed against {CrucibleShopBasis} - recommends "
				+ (picks.Count > 0 ? string.Join(", ", picks.ConvertAll(static pick => pick.Name)) + $", {budget - spent} tokens left" : "nothing")
				+ $"; offered: {string.Join(", ", offered)}");
		}
	}

	/// <summary>
	/// K10 and K13 (A569, A570, A578): the offers in the order safety asks - see <see cref="RecommendPurchases"/> - as far as
	/// <paramref name="budget"/>, the slots and <paramref name="maxPicks"/> go. <paramref name="owned"/> and
	/// <paramref name="heldItems"/> grow with each pick. <paramref name="spentAll"/> is what the picks cost together.
	/// </summary>
	private static unsafe List<CrucibleShopPick> RankOffers(List<(int Row, ushort Item, int Price)> offers, int budget, int maxPicks,
		List<CrucibleGear> owned, int gearSlots, List<ushort> heldItems, int itemSlots, uint board, out int spentAll, bool oneScale = false)
	{
		var picks = new List<CrucibleShopPick>();
		var gear = CrucibleGearTable;
		var items = CrucibleItems;
		var loads = CrucibleBoard.Loads;
		var stretch = CrucibleBoard.FightsPerStretch;
		offers = Unworn(offers, owned);

		// The board's fights: the statuses they inflict that cost safety, and their enemies' sheet strength against
		// intelligence.
		var fights = board != 0 ? CrucibleBoard.FightsOfBoard(board) : [];
		if (fights.Count == 0)
		{
			fights = CrucibleBoard.AllFights();
		}

		float strength = 0f, intelligence = 0f;
		var harms = new HashSet<uint>();
		foreach (var fight in fights)
		{
			foreach (var enemy in CrucibleBoard.EnemiesOf(fight))
			{
				strength += enemy.Strength;
				intelligence += enemy.Intelligence;
			}

			harms.UnionWith(CrucibleBoard.HarmsOf(fight));
		}

		var load = LoadOf(owned, loads, strength, intelligence, stretch, board != 0);

		var bought = new HashSet<int>();
		var itemsHeld = heldItems.Count;
		var gearHeld = owned.Count;
		var spent = 0;

		// 1. A second life while none is held.
		var life = false;
		foreach (var piece in owned)
		{
			life |= piece.Life;
		}

		foreach (var id in heldItems)
		{
			life |= items.TryGetValue(id, out var item) && item.Kind == CrucibleItemKind.Reraise;
		}

		foreach (var (row, id, price) in offers)
		{
			if (life)
			{
				break;
			}

			if (gear.TryGetValue(id, out var piece) && piece.Life && !piece.SelfHarm && Take(row, price, true))
			{
				picks.Add(new CrucibleShopPick(row, ClientItemName(id, piece.Name), price, "a second life: revives you by sacrificing your familiar"));
				owned.Add(piece);
				life = true;
			}
			else if (items.TryGetValue(id, out var item) && item.Kind == CrucibleItemKind.Reraise && Take(row, price, false))
			{
				picks.Add(new CrucibleShopPick(row, ClientItemName(id, item.Name), price, $"a second life: Reraise, {item.Value:P0} chance"));
				life = true;
			}
		}

		// 2. Against each status of the board's fights that ends in death: neither HP nor mitigation help against it.
		CoverStatuses(true);

		// 4 (A659: before the HP scale). Against each other status of the board's fights that costs safety - control, actions
		// prevented, damage over time: what HP and mitigation do not stop; the scale weighs only HP.
		CoverStatuses(false);

		// A single pick (a treasure, A656): gear and items on one scale - the HP each brings over the rest of the board - once a
		// fight is measured; the order below stays for the shop and before the first measurement.
		if (oneScale)
		{
			_offerScale = [];
		}

		if (oneScale && loads != null)
		{
			var top = -1;
			var scale = OfferScale(offers, owned, load, heldItems, 0f);
			_offerScale = scale;
			for (var i = 0; i < scale.Count && picks.Count < maxPicks; i++)
			{
				if (!bought.Contains(scale[i].Row) && scale[i].Value > 0f && (top < 0 || scale[i].Value > scale[top].Value))
				{
					top = i;
				}
			}

			if (top >= 0)
			{
				var (row, id, _, why) = scale[top];
				foreach (var offer in offers)
				{
					if (offer.Row == row && Take(row, offer.Price, gear.ContainsKey(id)))
					{
						picks.Add(new CrucibleShopPick(row, ClientItemName(id, EnglishItemName(id)), offer.Price, why));
						spentAll = spent;
						return picks;
					}
				}
			}
		}

		// 3, 5, 6 on one scale once a fight is measured (A659): gear, healing and protective items by the HP each brings over
		// the fights to the boss, piece by piece the most per token, from every start; of all choices the most, at a tie the
		// cheaper.
		if (!oneScale && loads != null)
		{
			ChooseOnScale();
			spentAll = spent;
			return picks;
		}

		// 3. Protective and healing gear: the choice that leaves the most HP after a stretch of fights ending with the
		// heaviest, then the most effective HP - every offer once first, then piece by piece the most gain per token; of all
		// these the best, at a tie the cheaper.
		var protective = new List<(int Row, CrucibleGear Piece, int Price, ushort Id)>();
		foreach (var (row, id, price) in offers)
		{
			if (!bought.Contains(row) && gear.TryGetValue(id, out var piece) && !piece.SelfHarm)
			{
				protective.Add((row, piece, price, id));
			}
		}

		var bestChoice = new List<int>();
		var best = ShopValue(owned, load);
		var bestCost = 0;
		for (var seed = 0; seed < protective.Count; seed++)
		{
			var (choice, value, cost) = ChooseFrom(seed);
			if (choice.Count > 0 && (Better(value, best) || (value == best && cost < bestCost)))
			{
				bestChoice = choice;
				best = value;
				bestCost = cost;
			}
		}

		var movement = 1f;
		foreach (var piece in owned)
		{
			movement *= 1f + piece.Movement;
		}

		foreach (var index in bestChoice)
		{
			movement *= 1f + protective[index].Piece.Movement;
		}

		foreach (var index in bestChoice)
		{
			var (row, piece, price, pieceId) = protective[index];
			var before = ShopValue(owned, load);
			_ = Take(row, price, true);
			owned.Add(piece);
			var after = ShopValue(owned, load);
			picks.Add(new CrucibleShopPick(row, ClientItemName(pieceId, piece.Name), price,
				(after.Left > before.Left
					? $"{after.Left - before.Left:+0%;-0%} of your HP left after a stretch of {load.Stretch:0.#} fights"
					: $"+{(after.Effective / before.Effective) - 1f:P0} effective HP against your heaviest fight")
				+ (piece.Movement < 0f ? $", movement {piece.Movement:+0%;-0%} (with all your gear {movement - 1f:+0%;-0%})" : string.Empty)
				+ (piece.Slow > 0f ? $", Slow {piece.Slow:+0%;-0%}: your actions come slower" : string.Empty)));
		}

		// 5. Healing items, the strongest first - a soul serum among them while you carry a crimson ribbon (A668).
		foreach (var (row, id, price) in ByValue(offers, o => items.TryGetValue(o.Item, out var item)
			&& HealShareOf(item, owned) is var share && share > 0f ? share + 1f : 0f))
		{
			var item = items[id];
			if (Take(row, price, false))
			{
				picks.Add(new CrucibleShopPick(row, ClientItemName(id, item.Name), price, $"heals {HealShareOf(item, owned):P0}"
					+ (item.Kind == CrucibleItemKind.Heal && item.Allies ? " to you and allies" : string.Empty)
					+ (item.Kind != CrucibleItemKind.Heal ? $" while you carry a {item.HealCarrying}" : string.Empty)));
			}
		}

		// 6. Protective items by their share.
		foreach (var (row, id, price) in ByValue(offers, o => items.TryGetValue(o.Item, out var item)
			&& item.Kind is CrucibleItemKind.Mitigate or CrucibleItemKind.NullifyPhysical or CrucibleItemKind.ReflectMagic ? item.Value + 1f : 0f))
		{
			var item = items[id];
			if (Take(row, price, false))
			{
				picks.Add(new CrucibleShopPick(row, ClientItemName(id, item.Name), price, item.Kind switch
				{
					CrucibleItemKind.Mitigate => $"{item.Value:P0} less damage taken",
					CrucibleItemKind.NullifyPhysical => "nullifies physical attacks",
					_ => "reflects magic attacks",
				}));
			}
		}

		spentAll = spent;
		return picks;

		// Steps 2 and 4: a resistance or a serum against each status of the board's fights - those ending in death, or the
		// others - that nothing held covers.
		void CoverStatuses(bool death)
		{
			foreach (var ailment in harms)
			{
				var name = StatusName(ailment);
				if (StatusHelper.EndsInDeath((StatusID)ailment) != death || name.Length == 0 || Covered(name))
				{
					continue;
				}

				foreach (var (row, id, price) in offers)
				{
					if (gear.TryGetValue(id, out var piece) && !piece.SelfHarm && GearResistsAilment(piece, name) && Take(row, price, true))
					{
						owned.Add(piece);
						picks.Add(new CrucibleShopPick(row, ClientItemName(id, piece.Name), price, $"immune to {name}, which this board inflicts"));
						break;
					}

					if (items.TryGetValue(id, out var item) && item.Kind == CrucibleItemKind.ResistAilment && item.Status.Length > 0
						&& name.Contains(item.Status, StringComparison.OrdinalIgnoreCase) && Take(row, price, false))
					{
						heldItems.Add(id);
						picks.Add(new CrucibleShopPick(row, ClientItemName(id, item.Name), price, $"against {name}, which this board inflicts"));
						break;
					}
				}
			}
		}

		// A659: the choice on the scale - from every start (-1: none set first), piece by piece the offer with the most HP
		// per token, the scale weighed anew on what is chosen; then taken, each with the reason it had when chosen.
		void ChooseOnScale()
		{
			var bestPicks = new List<(int Row, ushort Item, int Price, float Value, string Why)>();
			var bestValue = 0f;
			var bestCost = 0;
			for (var seed = -1; seed < offers.Count; seed++)
			{
				var (chosen, value, cost) = ScaleFrom(seed);
				if (chosen.Count > 0 && (value > bestValue || (value == bestValue && cost < bestCost)))
				{
					bestPicks = chosen;
					bestValue = value;
					bestCost = cost;
				}
			}

			foreach (var (row, id, price, _, why) in bestPicks)
			{
				var isGear = gear.TryGetValue(id, out var piece);
				if (Take(row, price, isGear))
				{
					if (isGear)
					{
						owned.Add(piece);
					}
					else
					{
						heldItems.Add(id);
					}

					picks.Add(new CrucibleShopPick(row, ClientItemName(id, EnglishItemName(id)), price, why
						+ (isGear && piece.Movement < 0f ? $", movement {piece.Movement:+0%;-0%}" : string.Empty)
						+ (isGear && piece.Slow > 0f ? $", Slow {piece.Slow:+0%;-0%}: your actions come slower" : string.Empty)));
				}
			}
		}

		(List<(int Row, ushort Item, int Price, float Value, string Why)> Chosen, float Value, int Cost) ScaleFrom(int seed)
		{
			var chosen = new List<(int Row, ushort Item, int Price, float Value, string Why)>();
			var set = new List<CrucibleGear>(owned);
			var held = new List<ushort>(heldItems);
			var tokens = budget - spent;
			var gearLeft = gearSlots - gearHeld;
			var itemsLeft = itemSlots - itemsHeld;
			var picksLeft = maxPicks - bought.Count;
			float total = 0f, healed = 0f;
			var cost = 0;
			var first = true;
			while (picksLeft > 0)
			{
				var open = new List<(int Row, ushort Item, int Price)>();
				foreach (var offer in offers)
				{
					// A668: every item, the scale weighs what it brings - a serum heals once a crimson ribbon is chosen, a buff
					// counts what it adds to an item held.
					if (!bought.Contains(offer.Row) && !chosen.Exists(c => c.Row == offer.Row) && offer.Price <= tokens
						&& (gear.TryGetValue(offer.Item, out var piece) ? !piece.SelfHarm && gearLeft > 0
							: items.ContainsKey(offer.Item) && itemsLeft > 0))
					{
						open.Add(offer);
					}
				}

				var scale = OfferScale(open, set, load, held, healed);
				var next = -1;
				var bestPerToken = 0f;
				for (var i = 0; i < scale.Count; i++)
				{
					if (scale[i].Value <= 0f)
					{
						continue;
					}

					var price = 0;
					foreach (var offer in open)
					{
						if (offer.Row == scale[i].Row)
						{
							price = offer.Price;
						}
					}

					var perToken = scale[i].Value / Math.Max(price, 1);
					if (first && seed >= 0 ? scale[i].Row == offers[seed].Row : perToken > bestPerToken)
					{
						next = i;
						bestPerToken = perToken;
					}
				}

				first = false;
				if (next < 0)
				{
					break;
				}

				var (row, id, value, why) = scale[next];
				var cost1 = 0;
				foreach (var offer in open)
				{
					if (offer.Row == row)
					{
						cost1 = offer.Price;
					}
				}

				chosen.Add((row, id, cost1, value, why));
				total += value;
				cost += cost1;
				tokens -= cost1;
				picksLeft--;
				if (gear.TryGetValue(id, out var taken))
				{
					set.Add(taken);
					gearLeft--;
				}
				else
				{
					held.Add(id);
					itemsLeft--;
					// A670: what an Auto-potion restores counts against the healing still needed, as a potion's does.
					if (items.TryGetValue(id, out var item) && (HealShareOf(item, set) > 0f || item.Kind == CrucibleItemKind.AutoPotion))
					{
						healed += value;
					}
				}
			}

			return (chosen, total, cost);
		}

		// One choice of protective gear, starting with offer <paramref name="seed"/>: its pieces, value and cost.
		(List<int> Choice, (float Left, float Effective) Value, int Cost) ChooseFrom(int seed)
		{
			var tokensLeft = budget - spent;
			var slotsLeft = Math.Min(gearSlots - gearHeld, maxPicks - bought.Count);
			var start = ShopValue(owned, load);
			var first = protective[seed];
			if (first.Price > tokensLeft || slotsLeft <= 0 || !Better(ShopValue([.. owned, first.Piece], load), start))
			{
				return ([], start, 0);
			}

			var choice = new List<int>();
			var set = new List<CrucibleGear>(owned);
			var cost = 0;
			var value = start;
			var next = seed;
			while (next >= 0)
			{
				set.Add(protective[next].Piece);
				choice.Add(next);
				cost += protective[next].Price;
				value = ShopValue(set, load);

				// The next piece: the most HP left per token while any piece adds to it, else the most effective HP per token.
				next = -1;
				var bestLeft = 0f;
				var bestEffective = 0f;
				for (var i = 0; i < protective.Count; i++)
				{
					if (choice.Contains(i) || protective[i].Price > tokensLeft - cost || choice.Count >= slotsLeft)
					{
						continue;
					}

					set.Add(protective[i].Piece);
					var with = ShopValue(set, load);
					set.RemoveAt(set.Count - 1);
					var price = Math.Max(protective[i].Price, 1);
					var left = (with.Left - value.Left) / price;
					var effective = ((with.Effective / value.Effective) - 1f) / price;
					if (left > bestLeft || (bestLeft <= 0f && left <= 0f && with.Left >= value.Left && effective > bestEffective))
					{
						next = i;
						bestLeft = Math.Max(left, bestLeft);
						bestEffective = left > 0f ? bestEffective : effective;
					}
				}
			}

			return (choice, value, cost);
		}

		bool Take(int row, int price, bool isGear)
		{
			if (bought.Contains(row) || bought.Count >= maxPicks || price > budget - spent
				|| (isGear ? gearHeld >= gearSlots : itemsHeld >= itemSlots))
			{
				return false;
			}

			_ = bought.Add(row);
			spent += price;
			if (isGear)
			{
				gearHeld++;
			}
			else
			{
				itemsHeld++;
			}

			return true;
		}

		bool Covered(string ailment)
		{
			foreach (var piece in owned)
			{
				if (GearResistsAilment(piece, ailment))
				{
					return true;
				}
			}

			foreach (var id in heldItems)
			{
				if (items.TryGetValue(id, out var item) && item.Kind == CrucibleItemKind.ResistAilment && item.Status.Length > 0
					&& ailment.Contains(item.Status, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		List<(int Row, ushort Item, int Price)> ByValue(List<(int Row, ushort Item, int Price)> all,
			Func<(int Row, ushort Item, int Price), float> value)
		{
			var chosen = new List<(int Row, ushort Item, int Price)>();
			foreach (var offer in all)
			{
				if (!bought.Contains(offer.Row) && value(offer) > 0f)
				{
					chosen.Add(offer);
				}
			}

			chosen.Sort((a, b) => value(b).CompareTo(value(a)) is var byValue && byValue != 0 ? byValue : a.Price.CompareTo(b.Price));
			return chosen;
		}
	}

	// Step 3 compares two values: more HP left after the stretch, at a tie more effective HP.
	private static bool Better((float Left, float Effective) value, (float Left, float Effective) than)
	{
		return value.Left > than.Left || (value.Left == than.Left && value.Effective > than.Effective);
	}

	private static bool GearResistsAilment(CrucibleGear piece, string ailment)
	{
		foreach (var resisted in piece.Resists)
		{
			if (ailment.Contains(resisted, StringComparison.OrdinalIgnoreCase)
				|| resisted.Contains(ailment, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	// What step 3 weighs the gear against, in shares of the player's maximum HP without gear: the damage of a mean and of the
	// heaviest fight by how it meets his defences, his own healing a fight, and the fights between two campsites.
	private readonly record struct ShopLoad(float Physical, float Magic, float Other, float HeavyPhysical, float HeavyMagic,
		float HeavyOther, float Healed, float Stretch);

	/// <summary>
	/// K10 step 3 (A570): the measured fights taken back to the player without his gear; before the first, a fight counts as
	/// a full HP bar split by the board's enemies' strength against intelligence, and without a measured stretch one fight.
	/// </summary>
	private static ShopLoad LoadOf(List<CrucibleGear> owned, (CrucibleBoard.FightLoad Mean, CrucibleBoard.FightLoad Heaviest, bool HereOnly)? loads,
		float strength, float intelligence, float stretch, bool boardKnown)
	{
		var fights = Math.Max(stretch, 1f);
		var stretchText = stretch > 0f ? $"{stretch:0.#} fights between campsites" : "no stretch between campsites measured yet, one fight counts";
		var player = Player;
		if (loads is not { } measured || player == null || player.MaxHp == 0)
		{
			var all = strength + intelligence;
			var physical = all > 0f ? strength / all : 0f;
			var magic = all > 0f ? intelligence / all : 0f;
			var other = all > 0f ? 0f : 1f;
			CrucibleShopBasis = $"no fight measured yet - a fight counted as a full HP bar, {physical:P0} physical by "
				+ (boardKnown ? "this board's" : "all Crucible") + $" enemies' strength; {stretchText}";
			return new ShopLoad(physical, magic, other, physical, magic, other, 0f, fights);
		}

		float hp = 1f, taken = 1f, physicalHeld = 1f, magicHeld = 1f, recovery = 0f;
		foreach (var piece in owned)
		{
			hp *= 1f + piece.MaxHp;
			taken *= 1f - piece.DamageTaken;
			physicalHeld *= (1f - piece.PhysicalVulnerability) * (1f - piece.Evasion);
			magicHeld *= 1f - piece.MagicVulnerability;
			recovery += piece.Recovery;
		}

		var maxHp = (float)player.MaxHp;
		var bare = maxHp / hp;
		var (mean, heaviest, hereOnly) = measured;
		CrucibleShopBasis = $"{(hereOnly ? "the fights here" : "the fights on every board since RSR loaded")}: a fight cost you "
			+ $"{mean.Damage / maxHp:P0} of your HP, the heaviest {heaviest.Damage / maxHp:P0}, you healed yourself "
			+ $"{mean.Healed / maxHp:P0} a fight; {stretchText}";
		return new ShopLoad(Share(mean.Physical, physicalHeld), Share(mean.Magic, magicHeld), Share(mean.Other, 1f),
			Share(heaviest.Physical, physicalHeld), Share(heaviest.Magic, magicHeld), Share(heaviest.Other, 1f),
			(float)(mean.Healed / (1f + recovery) / bare), fights);

		float Share(double amount, float held)
		{
			return (float)(amount / Math.Max(held * taken, float.Epsilon) / bare);
		}
	}

	/// <summary>
	/// K10 step 3 (A570), with <paramref name="gear"/> held: the share of his maximum HP the player comes out with from a
	/// stretch of fights that ends with the heaviest - each fight before it costs its mean damage less his own healing and
	/// the HP back at its end, the heaviest its damage less his healing -, and his effective HP against the heaviest fight's
	/// mix. Pieces multiply (concept 21, K10 Laden: an assumption, as is evasion as a chance to escape a physical hit).
	/// </summary>
	/// <summary>
	/// K13 (A656): every offer on one scale - the share of your HP it brings over the rest of the board. Gear: the damage it
	/// prevents in the fights left on the shortest way to the final fight (mean load for a fight, heaviest for elite and boss),
	/// its extra healing on the healing items held, its recovery after each fight up to what a fight costs. A healing item: its
	/// heal with your gear's extra healing. A protective item: its share of your heaviest fight. What the scale does not
	/// measure is named, not left out.
	/// </summary>
	private static List<(int Row, ushort Item, float Value, string Why)> OfferScale(List<(int Row, ushort Item, int Price)> offers,
		List<CrucibleGear> owned, ShopLoad load, List<ushort> heldItems, float healedChosen)
	{
		var result = new List<(int, ushort, float, string)>();
		var gear = CrucibleGearTable;
		var items = CrucibleItems;
		var (normal, heavy) = CrucibleRoute.FightsAhead() ?? (1, 1);
		var fights = normal + heavy;
		var held = HealingHeldShare();
		var now = RawLoad(owned, load);

		// A659, A661: what a healing item can still heal - the fights on the heaviest way to the boss, each with the heaviest
		// damage it ever dealt you (one never measured as heavy as your heaviest here), and what you miss now, less the
		// healing you hold and the choice has taken already.
		var holding = 0f;
		foreach (var id in heldItems)
		{
			if (items.TryGetValue(id, out var heal))
			{
				holding += HealShareOf(heal, owned);
			}
		}

		var ahead = CrucibleRoute.WorstAhead(false, battle => LoadNow(battle) ?? now.Heavy) ?? (fights * now.Heavy);

		// A671: the enemy fights on the shortest way - the ones a flight spares -, heaviest first, and the smoke bombs held,
		// which the way window spends on the heaviest of them.
		var enemyLoads = new List<float>();
		if (CrucibleRoute.EnemyBattlesAhead() is { } enemies)
		{
			foreach (var battle in enemies)
			{
				enemyLoads.Add(LoadNow(battle) ?? now.Mean);
			}
		}

		enemyLoads.Sort(static (a, b) => b.CompareTo(a));
		var bombsHeld = 0;
		foreach (var id in heldItems)
		{
			if (items.TryGetValue(id, out var bomb) && bomb.Kind == CrucibleItemKind.Escape)
			{
				bombsHeld++;
			}
		}

		var unspared = 0f;
		for (var i = bombsHeld; i < enemyLoads.Count; i++)
		{
			unspared += enemyLoads[i];
		}
		var worst = ahead + (1f - (Player is { MaxHp: > 0u } player ? player.GetHealthRatio() : 1f));
		var healable = Math.Max(0f, worst - (holding * (1f + now.Recovery)) - healedChosen);
		foreach (var (row, id, _) in offers)
		{
			if (gear.TryGetValue(id, out var piece))
			{
				if (piece.SelfHarm)
				{
					result.Add((row, id, 0f, "harms you: never taken"));
					continue;
				}

				List<CrucibleGear> withPiece = [.. owned, piece];
				var with = RawLoad(withPiece, load);
				var prevented = Math.Max(0f, (normal * (now.Mean - with.Mean)) + (heavy * (now.Heavy - with.Heavy)));
				var healing = piece.Recovery * held;
				var curing = piece.Curing > 0f
					? (normal * Math.Min(piece.Curing, with.Mean)) + (heavy * Math.Min(piece.Curing, with.Heavy))
					: 0f;

				// A668: what the piece lets the items held do - heal (a soul serum with a crimson ribbon), up to what you can
				// still heal, or stand longer (a tannin with an ice shield), on the heaviest fight with the piece.
				var enabled = 0f;
				var extended = 0f;
				var longerNames = new List<string>();
				foreach (var heldId in heldItems)
				{
					if (!items.TryGetValue(heldId, out var heldItem))
					{
						continue;
					}

					if (HealShareOf(heldItem, owned) <= 0f)
					{
						enabled += HealShareOf(heldItem, withPiece);
					}

					if (string.Equals(piece.Name, heldItem.LongerWith, StringComparison.OrdinalIgnoreCase))
					{
						var withLoad = (with.Heavy, with.HeavyPhysical, with.HeavyMagic);
						var gain = HeldValue(heldItem, withPiece, heldItems, withLoad, with.Recovery)
							- HeldValue(heldItem, owned, heldItems, withLoad, with.Recovery);
						if (gain > 0f)
						{
							extended += gain;
							longerNames.Add($"{ClientItemName(heldId, heldItem.Name)} {heldItem.LongerDuration:0} s instead of {heldItem.Duration:0} s");
						}
					}
				}

				var healingEnabled = Math.Min(enabled * (1f + with.Recovery), healable);

				// A669: an item at each rest (Chemist's Satchel) - the least it can be, at each campsite on every way to the
				// final fight, up to what you can still heal besides.
				var restHealing = 0f;
				var rests = 0;
				if (piece.RestItem.Length > 0 && CrucibleRoute.CampsitesAhead() is { } campsites && campsites > 0)
				{
					rests = campsites;
					restHealing = Math.Min(campsites * LeastHealOf(piece.RestItem) * (1f + with.Recovery),
						Math.Max(0f, healable - healingEnabled));
				}

				// A671: an offered escape the way window tells you to take spares that fight - its chance on every enemy fight on
				// the way no smoke bomb covers.
				var escaped = piece.EscapeChance * unspared;

				var parts = new List<string>();
				if (escaped > 0f)
				{
					parts.Add($"offers escape from {piece.EscapeChance:P0} of the enemy spaces: ~{escaped:P0} of your HP spared on the way");
				}

				if (prevented > 0f)
				{
					parts.Add($"prevents ~{prevented:P0} of your HP in the {fights} fights left");
				}

				if (healing > 0f)
				{
					parts.Add($"+{piece.Recovery:P0} on the {held:P0} of your healing items: ~{healing:P0}");
				}

				if (curing > 0f)
				{
					parts.Add($"recovers ~{curing:P0} after the fights left");
				}

				if (healingEnabled > 0f)
				{
					parts.Add($"lets items you hold heal: ~{healingEnabled:P0}");
				}

				if (extended > 0f)
				{
					parts.Add($"{string.Join(", ", longerNames)}: ~{extended:P0}");
				}

				if (restHealing > 0f)
				{
					parts.Add($"a {piece.RestItem} at each of the {rests} campsite(s) on every way, the weakest counted: ~{restHealing:P0}");
				}

				var value = prevented + healing + curing + healingEnabled + extended + restHealing + escaped;
				if (value <= 0f)
				{
					parts.Add(now.Mean <= 0f && now.Heavy <= 0f
						? "nothing to prevent: your fights cost you nothing measured"
						: "adds nothing against the damage your fights dealt");
				}

				var uncounted = UncountedEffects(id);
				if (uncounted.Length > 0)
				{
					parts.Add($"not counted: {uncounted}");
				}

				result.Add((row, id, value, $"lasts the whole board, ~{value:P0} in all: {string.Join("; ", parts)}"));
			}
			else if (items.TryGetValue(id, out var item))
			{
				// A668: a soul serum heals while the gear weighed carries a crimson ribbon; a protection catches what falls in its
				// duration; a buff counts what its status adds to an item held (three more Blink).
				var heals = HealShareOf(item, owned);
				var full = Math.Min(1f, heals * (1f + now.Recovery));
				var nowLoad = (now.Heavy, now.HeavyPhysical, now.HeavyMagic);
				var (protects, seconds, stacks, caught) = ProtectionValue(item, owned, heldItems, nowLoad);
				var (gives, givesTo) = item.Kind == CrucibleItemKind.Buff ? GainOfHeld(item, id) : (0f, string.Empty);
				var (restores, autoSeconds, expires) = item.Kind == CrucibleItemKind.AutoPotion
					? AutoPotionValue(item, owned, heldItems, healable, now.Recovery)
					: (0f, double.MaxValue, 0f);
				var spares = item.Kind == CrucibleItemKind.Escape && bombsHeld < enemyLoads.Count ? enemyLoads[bombsHeld] : 0f;
				var value = heals > 0f ? Math.Min(full, healable) : protects > 0f ? protects : restores > 0f ? restores
					: spares > 0f ? spares : gives;
				var lasting = seconds < double.MaxValue ? $" for {seconds:0} s" : string.Empty;
				result.Add((row, id, value, heals > 0f
					? (value < full
						? $"used once, heals ~{full:P0}, of which ~{value:P0} can still be needed: the fights left and what you miss come to at most ~{worst:P0}, you hold ~{holding * (1f + now.Recovery):P0} in healing"
						: $"used once, heals ~{value:P0}")
						+ (item.Kind == CrucibleItemKind.Heal && item.Allies ? " to you and the allies near you" : string.Empty)
						+ (item.Kind != CrucibleItemKind.Heal ? $" while you carry a {item.HealCarrying}" : string.Empty)
					: item.Kind switch
					{
						CrucibleItemKind.Mitigate => $"used once, {item.Value:P0} less damage{lasting}: ~{value:P0}, the {caught:P0} of your heaviest fight's damage that fell within it",
						CrucibleItemKind.NullifyPhysical => $"used once, nullifies {stacks} physical hits{lasting}: ~{value:P0}, {caught:P0} of your heaviest fight's physical damage",
						CrucibleItemKind.ReflectMagic => $"used once, reflects magic attacks{lasting}: ~{value:P0}, {caught:P0} of your heaviest fight's magic damage",
						CrucibleItemKind.Reraise => "a second life (weighed before this scale)",
						CrucibleItemKind.AutoPotion => restores > 0f
							? $"used against a boss, restores {item.Value:P0} below {item.AutoBelow:P0} of your HP for {autoSeconds:0} s, expiring with {expires:P0} each time: ~{value:P0} of the ~{healable:P0} your healing leaves"
							: $"restores only below {item.AutoBelow:P0} of your HP, and your healing keeps you above it (~{healable:P0} left to heal)",
						CrucibleItemKind.Buff when gives > 0f => $"used before {givesTo}, adds to it: ~{value:P0}",
						CrucibleItemKind.Escape => spares > 0f
							? $"flee the heaviest enemy fight on the way that no smoke bomb you hold covers (the way window names it): ~{value:P0}"
							: "no enemy fight left on the way that the smoke bombs you hold do not cover",
						_ => $"no HP on this scale ({item.Kind.ToString().ToLowerInvariant()})",
					}));
			}
			else
			{
				result.Add((row, id, 0f, "not read by RSR"));
			}
		}

		return result;

		// A668: what a buff's status adds to the protective items held that gain from it (Spirit of the Breathtaker: three
		// more Blink for the Tome of the Impervious), and their names.
		(float Gain, string To) GainOfHeld(CrucibleItem buff, ushort buffId)
		{
			if (buff.Status.Length == 0)
			{
				return (0f, string.Empty);
			}

			List<ushort> withBuff = [.. heldItems, buffId];
			var nowLoad = (now.Heavy, now.HeavyPhysical, now.HeavyMagic);
			var gain = 0f;
			var names = new List<string>();
			foreach (var heldId in heldItems)
			{
				if (items.TryGetValue(heldId, out var heldItem)
					&& string.Equals(heldItem.GainsFrom, buff.Status, StringComparison.OrdinalIgnoreCase))
				{
					var more = HeldValue(heldItem, owned, withBuff, nowLoad, now.Recovery) - HeldValue(heldItem, owned, heldItems, nowLoad, now.Recovery);
					if (more > 0f)
					{
						gain += more;
						names.Add(ClientItemName(heldId, heldItem.Name));
					}
				}
			}

			return (gain, string.Join(", ", names));
		}

		// A668, A670: what an item held is worth with this gear and these items - a protection by what it catches, an
		// Auto-potion by what it restores below its line.
		float HeldValue(CrucibleItem heldItem, List<CrucibleGear> gear, List<ushort> held,
			(float Heavy, float HeavyPhysical, float HeavyMagic) heavyLoad, float recovery)
		{
			return heldItem.Kind == CrucibleItemKind.AutoPotion
				? AutoPotionValue(heldItem, gear, held, healable, recovery).Value
				: ProtectionValue(heldItem, gear, held, heavyLoad).Value;
		}
	}

	// The load of a fight with this gear, as a share of your HP: the mean, the heaviest and its physical and magic parts, and
	// the extra healing of the gear.
	private static (float Mean, float Heavy, float HeavyPhysical, float HeavyMagic, float Recovery) RawLoad(List<CrucibleGear> gear,
		ShopLoad load)
	{
		float hp = 1f, taken = 1f, physical = 1f, magic = 1f, recovery = 0f;
		foreach (var piece in gear)
		{
			hp *= 1f + piece.MaxHp;
			taken *= 1f - piece.DamageTaken;
			physical *= (1f - piece.PhysicalVulnerability) * (1f - piece.Evasion);
			magic *= 1f - piece.MagicVulnerability;
			recovery += piece.Recovery;
		}

		var heavyPhysical = taken * load.HeavyPhysical * physical / hp;
		var heavyMagic = taken * load.HeavyMagic * magic / hp;
		return (taken * ((load.Physical * physical) + (load.Magic * magic) + load.Other) / hp,
			heavyPhysical + heavyMagic + (taken * load.HeavyOther / hp), heavyPhysical, heavyMagic, recovery);
	}

	// What a piece of gear does that the scale does not measure, from its English effect text: a stat of its own name
	// ("Haste +5%") other than those the scale reads, a faster pace, and an element it absorbs.
	private static string UncountedEffects(ushort id)
	{
		var text = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>(ClientLanguage.English)?.GetRowOrDefault(id)?.Unknown3.ExtractText();
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}

		var names = new List<string>();
		foreach (Match stat in AnyStat.Matches(text))
		{
			var name = stat.Groups["stat"].Value.Trim();
			if (name is not ("Maximum HP" or "Damage Taken" or "Physical Vulnerability" or "Magic Vulnerability" or "Evasion" or "Slow")
				&& !names.Contains(name))
			{
				names.Add(name);
			}
		}

		if (text.Contains("absorb", StringComparison.OrdinalIgnoreCase))
		{
			names.Add("absorbing damage as HP");
		}

		return string.Join(", ", names);
	}

	private static readonly Regex AnyStat = new(@"(?<stat>[A-Z][A-Za-z' ]*?) \+\d+%", RegexOptions.CultureInvariant);

	/// <summary>
	/// The offers without gear already worn (A657): a shop lists a piece you wear, and the game refuses to sell it - "You
	/// already own the Mythril Armor" - while the list weighed it as a second piece on the first.
	/// </summary>
	private static List<(int Row, ushort Item, int Price)> Unworn(List<(int Row, ushort Item, int Price)> offers, List<CrucibleGear> owned)
	{
		var gear = CrucibleGearTable;
		var left = new List<(int Row, ushort Item, int Price)>(offers.Count);
		foreach (var offer in offers)
		{
			if (!gear.TryGetValue(offer.Item, out var piece) || !owned.Contains(piece))
			{
				left.Add(offer);
			}
		}

		return left;
	}

	/// <summary>
	/// A count as the shop writes it, in the grouping of the client's language - "1.421" in German, "1,421" in English: the
	/// digits of the first number, whatever stands between its groups of three (A657). ECommons' FirstNumber knows only the
	/// comma and read "1.421" as 1. 0 where the text holds no number.
	/// </summary>
	private static int CountIn(string text)
	{
		var match = GroupedCount.Match(text);
		if (!match.Success)
		{
			return 0;
		}

		var digits = new StringBuilder(match.Length);
		foreach (var c in match.Value)
		{
			if (char.IsAsciiDigit(c))
			{
				_ = digits.Append(c);
			}
		}

		return int.TryParse(digits.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var count) ? count : 0;
	}

	private static readonly Regex GroupedCount = new(@"\d{1,3}(?:[.,'\s\u00A0\u202F]\d{3}(?!\d))+|\d+", RegexOptions.CultureInvariant);

	// K13 (A656): the last single pick's scale, offer by offer, for the treasure window.
	private static List<(int Row, ushort Item, float Value, string Why)> _offerScale = [];

	/// <summary>An item's English name from RSR's tables, or its row.</summary>
	private static string EnglishItemName(ushort id)
	{
		return CrucibleGearTable.TryGetValue(id, out var piece) ? piece.Name
			: CrucibleItems.TryGetValue(id, out var item) ? item.Name
			: CrucibleFeedTable.TryGetValue(id, out var feed) ? feed.Name
			: $"item {id}";
	}

	/// <summary>An item's name as the game shows it, in the client's language (A656); the English name where the sheet has none.</summary>
	private static string ClientItemName(ushort id, string english)
	{
		var name = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>()?.GetRowOrDefault(id)?.Unknown2.ExtractText();
		return string.IsNullOrEmpty(name) ? english : name;
	}

	// A683: the name of an offer of the shop, gear or item, as the client shows it.
	private static string OfferName(ushort id)
	{
		return CrucibleGearTable.TryGetValue(id, out var piece) ? ClientItemName(id, piece.Name)
			: CrucibleItems.TryGetValue(id, out var item) ? ClientItemName(id, item.Name)
			: ClientItemName(id, $"#{id}");
	}

	private static (float Left, float Effective) ShopValue(List<CrucibleGear> gear, ShopLoad load)
	{
		float hp = 1f, taken = 1f, physical = 1f, magic = 1f, recovery = 0f, curing = 0f;
		foreach (var piece in gear)
		{
			hp *= 1f + piece.MaxHp;
			taken *= 1f - piece.DamageTaken;
			physical *= (1f - piece.PhysicalVulnerability) * (1f - piece.Evasion);
			magic *= 1f - piece.MagicVulnerability;
			recovery += piece.Recovery;
			curing += piece.Curing;
		}

		var mean = taken * ((load.Physical * physical) + (load.Magic * magic) + load.Other) / hp;
		var heavyBare = load.HeavyPhysical + load.HeavyMagic + load.HeavyOther;
		var heavy = taken * ((load.HeavyPhysical * physical) + (load.HeavyMagic * magic) + load.HeavyOther) / hp;
		var healed = load.Healed * (1f + recovery) / hp;
		var left = 1f - ((load.Stretch - 1f) * Math.Max(0f, mean - healed - curing)) - Math.Max(0f, heavy - healed);
		var effective = heavyBare > 0f ? heavyBare / Math.Max(heavy, float.Epsilon) : hp / Math.Max(taken, float.Epsilon);
		return (left, effective);
	}

	/// <summary>K14 (A631): one feed for one familiar of the final fight's plan, its price and whether the tokens the list leaves
	/// pay for it.</summary>
	public readonly record struct CrucibleFeedPick(int Row, string Feed, BeastmasterPet Pet, int Price, bool Affordable, string Why);

	/// <summary>K14: the feed for the final fight while a shop is open, healers and protectors first; empty without a plan.</summary>
	public static IReadOnlyList<CrucibleFeedPick> CrucibleFeedRecommendation { get; private set; } = [];

	/// <summary>K14: whether a campsite can still come before the final fight, and what that means for the feed.</summary>
	public static string CrucibleFeedNote { get; private set; } = string.Empty;

	// What a feed does, read once from its English effect text (XBMItem, kind "Feed"): the shares it changes, the share of HP
	// it restores when eaten, the damage it adds, whether it harms its eater, whether it keeps feed over a rest, and the kins
	// it suits ("[Suitable for beastkin, ... and ashkin.]").
	private readonly record struct CrucibleFeed(string Name, float MaxHp, float DamageTaken, float PhysicalVulnerability,
		float MagicVulnerability, float Evasion, float Restore, float Damage, bool SelfHarm, bool Preserves, HashSet<string> Kins,
		bool FillsAtRest = false);

	// A669: a feed after which a rest at a campsite fills its eater's HP instead of the shared share (Lassi Simular).
	private const string FeedFillsAtRestText = "Completely restores HP at campsites";

	/// <summary>
	/// K10 Campsite (A669): what the feed <paramref name="ids"/> means for a rest - whether it outlasts the rest (Lily Simular),
	/// whether the rest fills the eater's HP (Lassi Simular), and the factors of its maximum HP and of the damage it takes.
	/// </summary>
	private static (bool Keeps, bool Fills, float Hp, float Taken) FeedAtRest(ReadOnlySpan<ushort> ids)
	{
		var keeps = false;
		var fills = false;
		float hp = 1f, taken = 1f;
		var feeds = CrucibleFeedTable;
		foreach (var id in ids)
		{
			if (feeds.TryGetValue(id, out var feed))
			{
				keeps |= feed.Preserves;
				fills |= feed.FillsAtRest;
				hp *= 1f + feed.MaxHp;
				taken *= 1f - feed.DamageTaken;
			}
		}

		return (keeps, fills, hp, taken);
	}

	// A669: whether a rest fills this familiar's HP, by the feed last seen on it in this run.
	private static bool RestFills(BeastmasterPet pet)
	{
		var (_, _, known, slots) = SlotsOf(pet);
		return known && FeedAtRest(slots.Items).Fills;
	}

	private static readonly Regex FeedRestore = new(@"Restores (?<share>\d+)% of HP upon consumption", RegexOptions.CultureInvariant);
	private static readonly Regex FeedDamage = new(@"Damage Dealt \+(?<share>\d+)%", RegexOptions.CultureInvariant);
	private static readonly Regex FeedKins = new(@"Suitable for (?<kins>[^\]]+)", RegexOptions.CultureInvariant);
	private const string FeedPreservesText = "Preserves the effects of feed";

	private static Dictionary<ushort, CrucibleFeed>? _crucibleFeed;
	private static string _feedTraced = string.Empty;
	private static Dictionary<ushort, CrucibleFeed> CrucibleFeedTable => _crucibleFeed ??= ReadCrucibleFeed();

	// K14 (A657, A662): each familiar's feed slots as the familiar window last showed them in this run - how many it holds, how
	// many it can hold, which - and its name in the client's language; counted up at each change for the shop's key.
	private readonly record struct FeedSlots(int Fed, int Max, ushort[] Items, string Name);

	private static readonly Dictionary<BeastmasterPet, FeedSlots> _feedSlots = [];
	private static int _feedSlotsRun;
	private static int _feedSlotsSeen;

	/// <summary>
	/// K14 (A657): while the familiar window shows - at a horn screen, a campsite, a feeding - each familiar's feed slots,
	/// read only where the window's rows are the team's (same count, same HP at each position, as at the campsite).
	/// </summary>
	internal static unsafe void NoteFeedSlots(AgentXBMPetParty* party, AtkUnitBase* screen)
	{
		if (party == null || screen == null || !screen->IsVisible || !DataCenter.IsInCrucible)
		{
			return;
		}

		if (_feedSlotsRun != CrucibleBoard.Run)
		{
			_feedSlotsRun = CrucibleBoard.Run;
			_feedSlots.Clear();
			_feedSlotsSeen++;
		}

		var pets = party->SelectedPets.AsSpan();
		var current = party->PetCurrentHealth;
		var max = party->PetMaxHealth;
		var seen = new List<(BeastmasterPet Pet, FeedSlots Slots)>(pets.Length);
		try
		{
			var rows = new ReaderXBMPetParty(screen).TeamEntries;
			if (rows.Count != pets.Length)
			{
				return;
			}

			for (var i = 0; i < rows.Count; i++)
			{
				if (i >= current.Length || i >= max.Length || rows[i].MaxHP != max[i] || rows[i].HP != current[i])
				{
					return;
				}

				var items = new List<ushort>();
				foreach (var entry in rows[i].FedItems)
				{
					if (entry.ItemId is > 0 and <= ushort.MaxValue)
					{
						items.Add((ushort)entry.ItemId);
					}
				}

				seen.Add(((BeastmasterPet)pets[i].PetId, new FeedSlots((int)rows[i].FedCurrent, (int)rows[i].FedMax, [.. items],
					rows[i].Name.TextValue)));
			}
		}
		catch (Exception ex) when (ex is InvalidCastException or ArgumentOutOfRangeException)
		{
			return;
		}

		var changed = new List<string>();
		foreach (var (pet, slots) in seen)
		{
			if (!_feedSlots.TryGetValue(pet, out var known) || known.Fed != slots.Fed || known.Max != slots.Max
				|| known.Name != slots.Name || !known.Items.AsSpan().SequenceEqual(slots.Items))
			{
				_feedSlots[pet] = slots;
				changed.Add($"{pet} {slots.Fed}/{slots.Max}");
			}
		}

		if (changed.Count > 0)
		{
			_feedSlotsSeen++;
			DefenseTrace.Line($"crucible feed slots: {string.Join(", ", changed)}");
		}
	}

	/// <summary>A familiar's name as the familiar window shows it, in the client's language; the English name before that.</summary>
	internal static string FamiliarName(BeastmasterPet pet)
	{
		return _feedSlots.TryGetValue(pet, out var slots) && slots.Name.Length > 0 ? slots.Name : pet.ToString();
	}

	// K14: a familiar's free feed slots and the feed it holds, as last seen in this run; one free slot and no feed where
	// the window was not seen here or read no maximum (Known false).
	private static (int Free, List<CrucibleFeed> Eaten, bool Known, FeedSlots Slots) SlotsOf(BeastmasterPet pet)
	{
		var eaten = new List<CrucibleFeed>();
		if (_feedSlotsRun != CrucibleBoard.Run || !_feedSlots.TryGetValue(pet, out var slots) || slots.Max <= 0
			|| slots.Fed > slots.Max)
		{
			return (1, eaten, false, default);
		}

		var feeds = CrucibleFeedTable;
		foreach (var item in slots.Items)
		{
			if (feeds.TryGetValue(item, out var feed))
			{
				eaten.Add(feed);
			}
		}

		return (slots.Max - slots.Fed, eaten, true, slots);
	}

	// K14: the familiars of the final fight's plan, healers and protectors first.
	private static List<BeastmasterPet> FeedOrder()
	{
		var order = new List<BeastmasterPet>(FinalTeam.Count);
		foreach (var pet in FinalTeam)
		{
			if (FinalHealers.Contains(pet))
			{
				order.Add(pet);
			}
		}

		foreach (var pet in FinalTeam)
		{
			if (!FinalHealers.Contains(pet))
			{
				order.Add(pet);
			}
		}

		return order;
	}

	// K14: what the final fight deals - the share of strength and intelligence among its enemies (the sheet), or all of it as
	// other damage without them - and each familiar's share of its HP now.
	private readonly record struct FeedContext(float Physical, float Magic, float Other, Dictionary<BeastmasterPet, float> Shares,
		Dictionary<BeastmasterPet, string> Health);

	private static unsafe FeedContext ReadFeedContext()
	{
		float strength = 0f, intelligence = 0f;
		foreach (var enemy in CrucibleBoard.EnemiesOf(FinalBattle))
		{
			strength += enemy.Strength;
			intelligence += enemy.Intelligence;
		}

		var all = strength + intelligence;
		var shares = new Dictionary<BeastmasterPet, float>();
		var health = new Dictionary<BeastmasterPet, string>();
		var party = AgentXBMPetParty.Instance();
		if (party != null)
		{
			var pets = party->SelectedPets.AsSpan();
			for (var i = 0; i < pets.Length && i < party->PetCurrentHealth.Length && i < party->PetMaxHealth.Length; i++)
			{
				if (party->PetMaxHealth[i] > 0)
				{
					shares[(BeastmasterPet)pets[i].PetId] = party->PetCurrentHealth[i] / (float)party->PetMaxHealth[i];
					health[(BeastmasterPet)pets[i].PetId] = $"{party->PetCurrentHealth[i]}/{party->PetMaxHealth[i]}";
				}
			}
		}

		return new FeedContext(all > 0f ? strength / all : 0f, all > 0f ? intelligence / all : 0f, all > 0f ? 0f : 1f, shares, health);
	}

	// K14 (A662): a familiar as the familiar window shows it - its name and HP ("Kolibri 374/487") - so that it is not taken
	// for one of a like name.
	private static string Shown(BeastmasterPet pet, FeedContext context)
	{
		return context.Health.TryGetValue(pet, out var hp) ? $"{FamiliarName(pet)} {hp}" : FamiliarName(pet);
	}

	/// <summary>
	/// K6, K14 (A662): what the feed <paramref name="pet"/> carries in this run makes of it - its damage as a factor
	/// (1 + each Damage Dealt) and the damage it takes as one (1 - each Damage Taken), multiplied over its feed; 1 and 1
	/// without feed or outside a board.
	/// </summary>
	internal static (float Dealt, float Taken) FeedFactors(BeastmasterPet pet)
	{
		float dealt = 1f, taken = 1f;
		if (!DataCenter.IsInCrucible)
		{
			return (dealt, taken);
		}

		foreach (var feed in SlotsOf(pet).Eaten)
		{
			dealt *= 1f + feed.Damage;
			taken *= 1f - feed.DamageTaken;
		}

		return (dealt, taken);
	}

	/// <summary>K14 (A662): counted up at each change of the feed slots read, for whoever weighs the feed.</summary>
	internal static int FeedSlotsSeen => _feedSlotsSeen;

	// K14 (A662): of the familiars of the final fight's plan with a free slot - its healers and protectors first, the others
	// only where none of them gains - the one the best of the offers not yet taken suits most: the most effective HP, or with
	// <paramref name="damageOnly"/> the most damage. Null where none gains.
	private static (BeastmasterPet Pet, (int Row, ushort Item, int Price, float Gain, float Bare) Fit)? NextFeed(
		List<(int Row, ushort Item, int Price)> offers, HashSet<int> taken,
		Dictionary<BeastmasterPet, (int Free, List<CrucibleFeed> Eaten, bool Known, FeedSlots Slots)> slots, FeedContext context,
		bool damageOnly)
	{
		var feeds = CrucibleFeedTable;
		foreach (var healers in (bool[])[true, false])
		{
			(BeastmasterPet Pet, (int Row, ushort Item, int Price, float Gain, float Bare) Fit)? best = null;
			foreach (var (pet, held) in slots)
			{
				if (FinalHealers.Contains(pet) != healers || held.Free <= 0
					|| BestFeed(pet, offers, taken, held.Eaten, context, damageOnly) is not { } fit)
				{
					continue;
				}

				if (best is not { } b || (damageOnly ? feeds[fit.Item].Damage > feeds[b.Fit.Item].Damage : fit.Gain > b.Fit.Gain))
				{
					best = (pet, fit);
				}
			}

			if (best != null)
			{
				return best;
			}
		}

		return null;
	}

	// K14: how long a familiar holds in the final fight - its effective HP: the share it has now plus what eating
	// <paramref name="added"/> restores, at most the new maximum, over the damage it takes after the cuts of all its feed
	// (multiplied like gear; unbelegt). The maximum HP of the feed already eaten is in the share now.
	private static float Survival(CrucibleFeed added, List<CrucibleFeed> eaten, float now, FeedContext context)
	{
		var max = 1f + added.MaxHp;
		var hp = Math.Min(max, now + (added.Restore * max));
		var taken = 1f - added.DamageTaken;
		var physicalCut = (1f - added.PhysicalVulnerability) * (1f - added.Evasion);
		var magicCut = 1f - added.MagicVulnerability;
		foreach (var feed in eaten)
		{
			taken *= 1f - feed.DamageTaken;
			physicalCut *= (1f - feed.PhysicalVulnerability) * (1f - feed.Evasion);
			magicCut *= 1f - feed.MagicVulnerability;
		}

		var hit = taken * ((context.Physical * physicalCut) + (context.Magic * magicCut) + context.Other);
		return hp / Math.Max(hit, float.Epsilon);
	}

	// K14: of the offers not yet taken that suit <paramref name="pet"/> and do not harm it, the one that keeps it longest on
	// top of its feed - or, with <paramref name="damageOnly"/>, one that adds no HP but counted damage; at a tie more damage,
	// then the cheaper. Null where none adds anything.
	private static (int Row, ushort Item, int Price, float Gain, float Bare)? BestFeed(BeastmasterPet pet,
		List<(int Row, ushort Item, int Price)> offers, HashSet<int> taken, List<CrucibleFeed> eaten, FeedContext context, bool damageOnly)
	{
		var feeds = CrucibleFeedTable;
		var kin = KinOf(pet).ToString().ToLowerInvariant();
		var share = context.Shares.TryGetValue(pet, out var known) ? known : 1f;
		var bare = Survival(default, eaten, share, context);
		(int Row, ushort Item, int Price, float Gain, float Bare)? best = null;
		var bestDamage = 0f;
		foreach (var (row, item, price) in offers)
		{
			// A679: never a feed it already ate - the game refuses a repeat (LazyCrucible's recorded pickers: every refusal was
			// one) -, nor one the plan already gives it.
			if (taken.Contains(row) || !feeds.TryGetValue(item, out var feed) || feed.SelfHarm || !feed.Kins.Contains(kin)
				|| AteAlready(eaten, feed.Name))
			{
				continue;
			}

			var gain = Survival(feed, eaten, share, context) - bare;
			if (damageOnly ? gain > 0f || feed.Damage <= 0f : gain <= 0f)
			{
				continue;
			}

			if (best is not { } b || gain > b.Gain || (gain == b.Gain && (feed.Damage > bestDamage
				|| (feed.Damage == bestDamage && price < b.Price))))
			{
				best = (row, item, price, gain, bare);
				bestDamage = feed.Damage;
			}
		}

		return best;
	}

	private static bool AteAlready(List<CrucibleFeed> eaten, string name)
	{
		foreach (var feed in eaten)
		{
			if (string.Equals(feed.Name, name, StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}

	// K14 (A631, A657): for the free feed slots of the final fight's plan, healers and protectors first, the feed on offer
	// that keeps each familiar longest in the final fight on top of what it holds - a slot each in turn - then what adds only
	// damage. Never a feed that adds neither, never one that harms its eater.
	private static void RecommendFeed(List<(int Row, ushort Item, int Price)> offers, int left)
	{
		var feeds = CrucibleFeedTable;
		var picks = new List<CrucibleFeedPick>();
		if (FinalTeam.Count == 0)
		{
			CrucibleFeedRecommendation = picks;
			CrucibleFeedNote = "No final-fight plan yet: it is made at a horn screen before another fight.";
			return;
		}

		var context = ReadFeedContext();
		var order = FeedOrder();
		var slots = new Dictionary<BeastmasterPet, (int Free, List<CrucibleFeed> Eaten, bool Known, FeedSlots Slots)>();
		var unseen = new List<string>();
		var open = 0;
		foreach (var pet in order)
		{
			var held = SlotsOf(pet);
			slots[pet] = held;
			open += held.Free;
			if (!held.Known)
			{
				unseen.Add(FamiliarName(pet));
			}
		}

		// A662: piece by piece, the healers and protectors of the plan first, each piece where it gains most - the rule the
		// feeding names its eater by, so that following this list in its order gives the same names there.
		var taken = new HashSet<int>();
		foreach (var damageOnly in (bool[])[false, true])
		{
			while (NextFeed(offers, taken, slots, context, damageOnly) is { } next)
			{
				var (pet, chosen) = next;
				var held = slots[pet];
				_ = taken.Add(chosen.Row);
				var eaten = feeds[chosen.Item];
				held.Eaten.Add(eaten);
				slots[pet] = held with { Free = held.Free - 1 };
				var affordable = chosen.Price <= left;
				left -= affordable ? chosen.Price : 0;
				var hp = context.Health.TryGetValue(pet, out var shown) ? shown : "unread";
				var effect = chosen.Gain > 0f ? $"effective HP +{chosen.Gain / chosen.Bare:P0} at {hp} HP" : "no HP";
				var slot = held.Known ? $"feed slot {held.Slots.Max - held.Free + 1} of {held.Slots.Max}" : "its feed slots not seen yet";
				picks.Add(new CrucibleFeedPick(chosen.Row, ClientItemName(chosen.Item, eaten.Name), pet, chosen.Price, affordable,
					$"{(FinalHealers.Contains(pet) ? "heals or protects" : "fights")} in the final: {effect}"
					+ (eaten.Damage > 0f ? $", damage +{eaten.Damage:P0}" : string.Empty) + $"; {slot}"));
			}
		}

		CrucibleFeedRecommendation = picks;
		var feedTrace = string.Join(", ", picks.ConvertAll(static pick => $"{pick.Feed} -> {pick.Pet}"));
		if (feedTrace != _feedTraced)
		{
			_feedTraced = feedTrace;
			DefenseTrace.Line($"crucible shop: feed for the final fight ({string.Join(", ", FinalTeam)}): "
				+ (picks.Count > 0 ? feedTrace : open == 0 ? "no free feed slot" : "none on offer adds HP or damage")
				+ $"; campsite after this shop: {CrucibleRoute.CampsiteAfterShop()?.ToString() ?? "unknown"}");
		}

		var why = picks.Count > 0 ? string.Empty
			: open == 0 ? "Every familiar of the final fight's plan has its feed slots filled. "
			: "Nothing on offer adds HP or damage to a familiar of the final fight's plan with a free feed slot. ";
		if (unseen.Count > 0)
		{
			why += $"Feed slots of {string.Join(", ", unseen)} not seen in this run yet: one each assumed. ";
		}

		CrucibleFeedNote = why + CrucibleRoute.CampsiteAfterShop() switch
		{
			false => "No campsite can come before the final fight: the feed lasts into it.",
			true => "A campsite can still come: a fed familiar that rests there loses its feed (RSR does not rest a fed one; Lily Simular keeps it).",
			_ => "Where this shop stands on the board is not known: a fed familiar that rests at a later campsite loses its feed.",
		};
	}

	/// <summary>
	/// K14 (A631, A657): for the feed being given, the familiar of the final fight's plan with a free feed slot - as the
	/// window shows them now - it keeps longest, else the one it adds most damage to; or why none.
	/// </summary>
	internal static string FeedTargetText(ushort item)
	{
		if (!CrucibleFeedTable.TryGetValue(item, out var feed))
		{
			return $"Feeding: item {item} is not a feed RSR reads.";
		}

		var name = ClientItemName(item, feed.Name);
		if (FinalTeam.Count == 0)
		{
			return $"Feed {name}: no final-fight plan yet.";
		}

		var context = ReadFeedContext();
		var offer = new List<(int Row, ushort Item, int Price)> { (0, item, 0) };
		var none = new HashSet<int>();
		var slots = new Dictionary<BeastmasterPet, (int Free, List<CrucibleFeed> Eaten, bool Known, FeedSlots Slots)>();
		var open = 0;
		foreach (var pet in FeedOrder())
		{
			slots[pet] = SlotsOf(pet);
			open += slots[pet].Free;
		}

		// A662: the rule of the shop's list - its healers and protectors first, where it gains most.
		foreach (var damageOnly in (bool[])[false, true])
		{
			if (NextFeed(offer, none, slots, context, damageOnly) is { } next)
			{
				var (pet, fit) = next;
				var held = slots[pet];
				var hp = context.Health.TryGetValue(pet, out var shown) ? shown : "unread";
				var effect = fit.Gain > 0f ? $"effective HP +{fit.Gain / fit.Bare:P0} at {hp} HP" : $"damage +{feed.Damage:P0}, no HP";
				var slot = held.Known ? $"slot {held.Slots.Max - held.Free + 1} of {held.Slots.Max}" : "its feed slots not seen yet";
				return $"Feed {name}: give it to {Shown(pet, context)} ({(FinalHealers.Contains(pet) ? "heals or protects" : "fights")} in the final; {effect}; {slot}).";
			}
		}

		return open == 0
			? $"Feed {name}: every familiar of the final fight's plan has its feed slots filled."
			: $"Feed {name}: it adds no HP or counted damage to a familiar of the final fight's plan with a free feed slot.";
	}

	private static Dictionary<ushort, CrucibleFeed> ReadCrucibleFeed()
	{
		var feeds = new Dictionary<ushort, CrucibleFeed>();
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>(ClientLanguage.English);
		var kinds = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItemType>(ClientLanguage.English);
		if (sheet == null || kinds == null)
		{
			return feeds;
		}

		var feedKind = uint.MaxValue;
		foreach (var kind in kinds)
		{
			if (kind.Unknown0.ExtractText() == "Feed")
			{
				feedKind = kind.RowId;
			}
		}

		foreach (var row in sheet)
		{
			if (row.Unknown13 != feedKind)
			{
				continue;
			}

			var text = row.Unknown3.ExtractText();
			float maxHp = 0f, taken = 0f, physical = 0f, magic = 0f, evasion = 0f, damage = 0f;
			foreach (Match stat in GearStat.Matches(text))
			{
				var share = Percent(stat.Groups["share"].Value) * (stat.Groups["sign"].Value == "-" ? -1f : 1f);
				switch (stat.Groups["stat"].Value)
				{
					case "Maximum HP": maxHp += share; break;
					case "Damage Taken": taken -= share; break;
					case "Physical Vulnerability": physical -= share; break;
					case "Magic Vulnerability": magic -= share; break;
					case "Evasion": evasion += share; break;
					default: break;
				}
			}

			foreach (Match dealt in FeedDamage.Matches(text))
			{
				damage += Percent(dealt.Groups["share"].Value);
			}

			var kins = new HashSet<string>();
			if (FeedKins.Match(text) is { Success: true } suits)
			{
				foreach (var part in suits.Groups["kins"].Value.Replace(" and ", ",", StringComparison.Ordinal).Split(','))
				{
					var kin = part.Trim().TrimEnd('.').Trim();
					if (kin.Length > 0)
					{
						_ = kins.Add(kin.ToLowerInvariant());
					}
				}
			}

			feeds[(ushort)row.RowId] = new CrucibleFeed(row.Unknown2.ExtractText(), maxHp, taken, physical, magic, evasion,
				FeedRestore.Match(text) is { Success: true } restore ? Percent(restore.Groups["share"].Value) : 0f, damage,
				GearSelfHarm.IsMatch(text) || text.Contains("on self", StringComparison.Ordinal),
				text.Contains(FeedPreservesText, StringComparison.Ordinal), kins,
				text.Contains(FeedFillsAtRestText, StringComparison.Ordinal));
		}

		return feeds;

		static float Percent(string digits)
		{
			_ = float.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent);
			return percent / 100f;
		}
	}

	private static Dictionary<ushort, CrucibleGear> CrucibleGearTable => _crucibleGear ??= ReadCrucibleGear();

	private static Dictionary<ushort, CrucibleGear> ReadCrucibleGear()
	{
		var gear = new Dictionary<ushort, CrucibleGear>();
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>(ClientLanguage.English);
		var kinds = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItemType>(ClientLanguage.English);
		if (sheet == null || kinds == null)
		{
			return gear;
		}

		// The kind of an item is its XBMItemType (Lumina's Unknown13): beast gear by the English name of that row.
		var gearKind = uint.MaxValue;
		foreach (var kind in kinds)
		{
			if (kind.Unknown0.ExtractText() == "Beast Gear")
			{
				gearKind = kind.RowId;
			}
		}

		foreach (var row in sheet)
		{
			if (row.Unknown13 != gearKind)
			{
				continue;
			}

			var text = row.Unknown3.ExtractText();
			float maxHp = 0f, taken = 0f, physical = 0f, magic = 0f, evasion = 0f, movement = 0f, slow = 0f;
			foreach (Match stat in GearStat.Matches(text))
			{
				var share = Percent(stat.Groups["share"].Value) * (stat.Groups["sign"].Value == "-" ? -1f : 1f);
				switch (stat.Groups["stat"].Value)
				{
					case "Maximum HP": maxHp += share; break;
					case "Damage Taken": taken -= share; break;
					case "Physical Vulnerability": physical -= share; break;
					case "Magic Vulnerability": magic -= share; break;
					case "Evasion": evasion += share; break;
					case "Movement Speed": movement += share; break;
					default: slow += share; break;
				}
			}

			var resists = new List<string>();
			foreach (Match resist in GearResists.Matches(text))
			{
				resists.Add(resist.Groups["ailment"].Value);
			}

			var kill = GearKillStack.Match(text);
			gear[(ushort)row.RowId] = new CrucibleGear(row.Unknown2.ExtractText(), maxHp, taken, physical, magic, evasion, movement, slow,
				GearRecovery.Match(text) is { Success: true } recovery ? Percent(recovery.Groups["share"].Value) : 0f,
				GearCuring.Match(text) is { Success: true } curing ? Percent(curing.Groups["share"].Value) : 0f,
				text.StartsWith("Revives beastmaster upon KO", StringComparison.Ordinal), GearSelfHarm.IsMatch(text), resists,
				GearRestItem.Match(text) is { Success: true } restItem ? restItem.Groups["item"].Value : string.Empty,
				GearEscape.Match(text) is { Success: true } escape ? Percent(escape.Groups["chance"].Value) : 0f,
				GearTreasureItems.Match(text) is { Success: true } more ? CountOf(more.Groups["count"].Value) : 0,
				kill.Success ? kill.Groups["status"].Value : string.Empty,
				kill.Success ? Percent(kill.Groups["share"].Value) : 0f,
				kill.Success ? CountOf(kill.Groups["max"].Value) : 0);
		}

		return gear;

		static float Percent(string digits)
		{
			_ = float.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent);
			return percent / 100f;
		}

		// A671: a count as the text writes it ("two"), by the words the item texts use (CountWords).
		static int CountOf(string count)
		{
			return int.TryParse(count, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number
				: Math.Max(Array.IndexOf(CountWords, count), 0);
		}
	}
}
