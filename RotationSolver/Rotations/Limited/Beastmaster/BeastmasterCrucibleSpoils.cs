using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RotationSolver.Rotations.Limited.Beastmaster;

/// <summary>
/// K17 of the Crucible (concept 21, "K17 – Beute nach dem Kampf", A892): while the spoils of a fight are open, which pieces to
/// take - all, where everything left fits and nothing costs more than it brings - with the reason for each. Taking stays the
/// player's click in the game's window.
/// </summary>
public partial class BeastmasterRotation
{
	// The spoils window XBMContentsBooty: up to four pieces, piece k present at 6 + 5·k, its XBMItem row at 9 + 5·k, taken at
	// 129 + k (his trace of 15.11.2026; LazyCrucible cfc5ac5, Screens.ReadBooty; ECommons 7d50941, ReaderXBMContentsBooty).
	// The game's layout of the window, not a tuned value.
	private const int SpoilsFirstPresent = 6;
	private const int SpoilsFirstRow = 9;
	private const int SpoilsStride = 5;
	private const int SpoilsPieces = 4;
	private const int SpoilsFirstTaken = 129;

	private static string _spoilsKey = string.Empty;

	/// <summary>Whether the spoils of a fight are open and the recommendation runs, for the window.</summary>
	public static bool CrucibleSpoilsOpen { get; private set; }

	/// <summary>Whether every piece left is worth taking and fits: the game's "take all".</summary>
	public static bool CrucibleSpoilsTakeAll { get; private set; }

	/// <summary>K17: every piece left in the open spoils with its verdict, those to take marked; empty while none is open.</summary>
	public static IReadOnlyList<CrucibleOfferVerdict> CrucibleSpoilsOffers { get; private set; } = [];

	/// <summary>K17: every frame with the spoils open, the recommendation (when <paramref name="recommend"/>).</summary>
	internal static unsafe void FollowCrucibleSpoils(bool recommend)
	{
		var spoils = (AtkUnitBase*)Svc.GameGui.GetAddonByName("XBMContentsBooty").Address;
		if (!recommend || !DataCenter.IsInCrucible || InCombat || spoils == null || !spoils->IsVisible)
		{
			CrucibleSpoilsOpen = false;
			CrucibleSpoilsTakeAll = false;
			CrucibleSpoilsOffers = [];
			_spoilsKey = string.Empty;
			return;
		}

		CrucibleSpoilsOpen = true;
		var offers = new List<(int Row, ushort Item, int Price)>();
		for (var k = 0; k < SpoilsPieces; k++)
		{
			var present = SpoilsFirstPresent + (SpoilsStride * k);
			var row = SpoilsFirstRow + (SpoilsStride * k);
			var taken = SpoilsFirstTaken + k;
			if (row >= spoils->AtkValuesCount || !IsTrue(spoils->AtkValues[present]) || (taken < spoils->AtkValuesCount && IsTrue(spoils->AtkValues[taken])))
			{
				continue;
			}

			var value = spoils->AtkValues[row];
			var id = value.Type == AtkValueType.UInt ? value.UInt : value.Type == AtkValueType.Int && value.Int > 0 ? (uint)value.Int : 0u;
			if (id is > 0 and <= ushort.MaxValue)
			{
				offers.Add((k, (ushort)id, 0));
			}
		}

		RecommendSpoils(offers);

		static bool IsTrue(AtkValue value) => value.Type == AtkValueType.Bool && value.Byte != 0;
	}

	private static void RecommendSpoils(List<(int Row, ushort Item, int Price)> offers)
	{
		ReadHoldings(out var heldItems, out var itemSlots, out var owned, out var gearSlots, out var board);
		var key = $"{string.Join(",", offers)}|{string.Join(",", heldItems)}|{owned.Count}|{gearSlots}|{board}|{CrucibleBoard.FightsPerStretch}";
		if (key == _spoilsKey)
		{
			return;
		}

		_spoilsKey = key;
		var gear = CrucibleGearTable;
		var verdicts = new Dictionary<int, (bool Take, string Why)>();

		// Step 1: worn gear is not given twice.
		var open = new List<(int Row, ushort Item, int Price)>();
		foreach (var offer in offers)
		{
			if (gear.TryGetValue(offer.Item, out var worn) && owned.Contains(worn))
			{
				verdicts[offer.Row] = (false, "you already wear it");
			}
			else
			{
				open.Add(offer);
			}
		}

		// Step 2 (K15, A690, as at the treasure): while no beast gear has come in this run and the player carries the way to the
		// final fight, gear stays - it would cost the bonus Mit leeren Händen.
		if (owned.Count == 0 && CarriesToFinal(true, out var carried, out var heaviestWay))
		{
			for (var i = open.Count - 1; i >= 0; i--)
			{
				if (gear.ContainsKey(open[i].Item))
				{
					verdicts[open[i].Row] = (false, $"keeps the bonus Mit leeren Händen: you carry the heaviest way to the final fight without it ({carried:P0} against {heaviestWay:P0})");
					open.RemoveAt(i);
				}
			}
		}

		// Step 3: the shop's order, price nought - what adds safety first.
		var itemsFree = Math.Max(0, itemSlots - heldItems.Count);
		var gearFree = Math.Max(0, gearSlots - owned.Count);
		var picks = RankOffers(open, int.MaxValue, open.Count, owned, gearSlots, new List<ushort>(heldItems), itemSlots, board, out _, true);
		foreach (var pick in picks)
		{
			foreach (var offer in open)
			{
				if (offer.Row == pick.Row && !verdicts.ContainsKey(offer.Row) && TakeSlot(offer.Item))
				{
					verdicts[offer.Row] = (true, pick.Why);
				}
			}
		}

		// Step 4: every other item while a slot is free; every other piece of gear that does not harm you while a gear slot is.
		var items = CrucibleItems;
		foreach (var offer in open)
		{
			if (verdicts.ContainsKey(offer.Row))
			{
				continue;
			}

			if (gear.TryGetValue(offer.Item, out var piece) && piece.SelfHarm)
			{
				verdicts[offer.Row] = (false, "it harms you");
			}
			else if (TakeSlot(offer.Item))
			{
				verdicts[offer.Row] = (true, items.TryGetValue(offer.Item, out var item) && item.Kind is CrucibleItemKind.Damage or CrucibleItemKind.Buff
					? "for damage in the final fight" : "costs nothing but a free slot");
			}
			else
			{
				verdicts[offer.Row] = (false, gear.ContainsKey(offer.Item) ? "no free gear slot" : "no free item slot; the pieces above rank first");
			}
		}

		var list = new List<CrucibleOfferVerdict>(offers.Count);
		var takeAll = offers.Count > 0;
		foreach (var take in new[] { true, false })
		{
			foreach (var (row, id, _) in offers)
			{
				if (verdicts.TryGetValue(row, out var verdict) && verdict.Take == take)
				{
					list.Add(new CrucibleOfferVerdict(row, ClientItemName(id, EnglishItemName(id)), verdict.Why, take));
					takeAll &= take;
				}
			}
		}

		CrucibleSpoilsOffers = list;
		CrucibleSpoilsTakeAll = takeAll;
		var taken = new List<string>();
		var left = new List<string>();
		foreach (var verdict in list)
		{
			(verdict.Recommended ? taken : left).Add($"{verdict.Name} ({verdict.Verdict})");
		}

		// A973: a window read before the game filled it lists no piece - "take nothing; leave " said nothing about the spoils.
		DefenseTrace.Line($"crucible spoils: {offers.Count} pieces, {itemsFree} item and {gearFree} gear slots free - "
			+ (list.Count == 0 ? "nothing offered"
				: takeAll ? "take all" : $"take {(taken.Count > 0 ? string.Join(", ", taken) : "nothing")}; leave {string.Join(", ", left)}"));

		// A slot of the piece's kind is free: then it is counted as taken.
		bool TakeSlot(ushort id)
		{
			if (gear.ContainsKey(id))
			{
				if (gearFree <= 0)
				{
					return false;
				}

				gearFree--;
				return true;
			}

			if (itemsFree <= 0)
			{
				return false;
			}

			itemsFree--;
			return true;
		}
	}
}
