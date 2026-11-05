using System.Globalization;
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
/// K10 of the Crucible (concept 01, "K10 – Zwischen den Kämpfen", Laden, A569): while a shop on a board is open, what to
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
		bool SelfHarm, List<string> Resists);

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
	private static (int Coins, int Bought, int Gear, int Items, uint Board, double Fights, float Stretch)? _shopKey;
	private static string _shopTraced = string.Empty;

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
		var picks = new List<CrucibleShopPick>();
		var budget = reader.Coins;
		var gear = CrucibleGearTable;
		var items = CrucibleItems;

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
		foreach (var piece in reader.OwnedEntries)
		{
			gearSlots++;
			if (piece.Owned && piece.Id <= ushort.MaxValue && gear.TryGetValue((ushort)piece.Id, out var held))
			{
				owned.Add(held);
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
			loads is { } measured ? measured.Mean.Damage + measured.Heaviest.Damage + measured.Mean.Healed : 0d, stretch);
		if (_shopKey == key)
		{
			return;
		}

		_shopKey = key;

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

		var offers = new List<(int Row, ushort Item, int Price)>();
		for (var r = 0; r < stock.Count; r++)
		{
			if (stock[r].Listed && !stock[r].Bought && stock[r].Item is > 0 and <= ushort.MaxValue)
			{
				offers.Add((r, (ushort)stock[r].Item, stock[r].Price));
			}
		}

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
				picks.Add(new CrucibleShopPick(row, piece.Name, price, "a second life: revives you by sacrificing your familiar"));
				owned.Add(piece);
				life = true;
			}
			else if (items.TryGetValue(id, out var item) && item.Kind == CrucibleItemKind.Reraise && Take(row, price, false))
			{
				picks.Add(new CrucibleShopPick(row, item.Name, price, $"a second life: Reraise, {item.Value:P0} chance"));
				life = true;
			}
		}

		// 2. Against each status of the board's fights that ends in death: neither HP nor mitigation help against it.
		CoverStatuses(true);

		// 3. Protective and healing gear: the choice that leaves the most HP after a stretch of fights ending with the
		// heaviest, then the most effective HP - every offer once first, then piece by piece the most gain per token; of all
		// these the best, at a tie the cheaper.
		var protective = new List<(int Row, CrucibleGear Piece, int Price)>();
		foreach (var (row, id, price) in offers)
		{
			if (!bought.Contains(row) && gear.TryGetValue(id, out var piece) && !piece.SelfHarm)
			{
				protective.Add((row, piece, price));
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
			var (row, piece, price) = protective[index];
			var before = ShopValue(owned, load);
			_ = Take(row, price, true);
			owned.Add(piece);
			var after = ShopValue(owned, load);
			picks.Add(new CrucibleShopPick(row, piece.Name, price,
				(after.Left > before.Left
					? $"{after.Left - before.Left:+0%;-0%} of your HP left after a stretch of {load.Stretch:0.#} fights"
					: $"+{(after.Effective / before.Effective) - 1f:P0} effective HP against your heaviest fight")
				+ (piece.Movement < 0f ? $", movement {piece.Movement:+0%;-0%} (with all your gear {movement - 1f:+0%;-0%})" : string.Empty)
				+ (piece.Slow > 0f ? $", Slow {piece.Slow:+0%;-0%}: your actions come slower" : string.Empty)));
		}

		// 4. Against each other status of the board's fights that costs safety: control, actions prevented, damage over time.
		CoverStatuses(false);

		// 5. Healing items, the strongest first; 6. protective items by their share.
		foreach (var kinds in new[] { new[] { CrucibleItemKind.Heal }, new[] { CrucibleItemKind.Mitigate,
			CrucibleItemKind.NullifyPhysical, CrucibleItemKind.ReflectMagic } })
		{
			foreach (var (row, id, price) in ByValue(offers, o => items.TryGetValue(o.Item, out var item)
				&& Array.IndexOf(kinds, item.Kind) >= 0 ? item.Value + 1f : 0f))
			{
				var item = items[id];
				if (Take(row, price, false))
				{
					picks.Add(new CrucibleShopPick(row, item.Name, price, item.Kind switch
					{
						CrucibleItemKind.Heal => $"heals {item.Value:P0}{(item.Allies ? " to you and allies" : string.Empty)}",
						CrucibleItemKind.Mitigate => $"{item.Value:P0} less damage taken",
						CrucibleItemKind.NullifyPhysical => "nullifies physical attacks",
						_ => "reflects magic attacks",
					}));
				}
			}
		}

		CrucibleShopRecommendation = picks;
		CrucibleShopTokens = (budget, budget - spent);
		var traced = string.Join(", ", picks.ConvertAll(static pick => pick.Name));
		if (traced != _shopTraced)
		{
			_shopTraced = traced;
			DefenseTrace.Line($"crucible shop: {budget} tokens, weighed against {CrucibleShopBasis} - recommends "
				+ (picks.Count > 0 ? traced + $", {budget - spent} tokens left" : "nothing"));
		}

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
						picks.Add(new CrucibleShopPick(row, piece.Name, price, $"immune to {name}, which this board inflicts"));
						break;
					}

					if (items.TryGetValue(id, out var item) && item.Kind == CrucibleItemKind.ResistAilment && item.Status.Length > 0
						&& name.Contains(item.Status, StringComparison.OrdinalIgnoreCase) && Take(row, price, false))
					{
						heldItems.Add(id);
						picks.Add(new CrucibleShopPick(row, item.Name, price, $"against {name}, which this board inflicts"));
						break;
					}
				}
			}
		}

		// One choice of protective gear, starting with offer <paramref name="seed"/>: its pieces, value and cost.
		(List<int> Choice, (float Left, float Effective) Value, int Cost) ChooseFrom(int seed)
		{
			var tokensLeft = budget - spent;
			var slotsLeft = gearSlots - gearHeld;
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
			if (bought.Contains(row) || price > budget - spent || (isGear ? gearHeld >= gearSlots : itemsHeld >= itemSlots))
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
	/// mix. Pieces multiply (concept 01, K10 Laden: an assumption, as is evasion as a chance to escape a physical hit).
	/// </summary>
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

			gear[(ushort)row.RowId] = new CrucibleGear(row.Unknown2.ExtractText(), maxHp, taken, physical, magic, evasion, movement, slow,
				GearRecovery.Match(text) is { Success: true } recovery ? Percent(recovery.Groups["share"].Value) : 0f,
				GearCuring.Match(text) is { Success: true } curing ? Percent(curing.Groups["share"].Value) : 0f,
				text.StartsWith("Revives beastmaster upon KO", StringComparison.Ordinal), GearSelfHarm.IsMatch(text), resists);
		}

		return gear;

		static float Percent(string digits)
		{
			_ = float.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent);
			return percent / 100f;
		}
	}
}
