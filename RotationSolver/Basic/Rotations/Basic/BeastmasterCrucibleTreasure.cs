using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K13 of the Crucible (concept 21, "K13 – Schatz", A578): while a treasure on a board is open, which offer to take - the
/// shop's order with no price and one pick - and on request that pick. Nothing is taken without the click.
/// </summary>
public partial class BeastmasterRotation
{
	// The treasure window XBMContentsTreasure: up to four offers, each an XBMItem row at the window's values 6 + 5·i; taking
	// offer i is the window's callback [2, i] (Daedalus 74659a6, CrucibleAutoPilot.TakeTreasure and CrucibleAutoPilotRules,
	// recorded 25.10.2026). The game's layout of the window, not a tuned value.
	private const int TreasureFirstOffer = 6;
	private const int TreasureOfferStride = 5;
	private const int TreasureOffers = 4;
	private const int TreasureTakeCallback = 2;

	private static readonly HashSet<int> _treasureTaken = [];
	private static int? _treasureWanted;
	private static string _treasureKey = string.Empty;

	/// <summary>Whether a treasure is open and the recommendation runs, for the window.</summary>
	public static bool CrucibleTreasureOpen { get; private set; }

	/// <summary>The offer RSR recommends while a treasure is open; null when none is open or none is worth it.</summary>
	public static CrucibleShopPick? CrucibleTreasureRecommendation { get; private set; }

	/// <summary>What the take did last, for the window.</summary>
	public static string CrucibleTreasureStatus { get; private set; } = string.Empty;

	/// <summary>K13 (A656): one offer of the open treasure, its name in the client's language, and what it is worth.</summary>
	public readonly record struct CrucibleOfferVerdict(int Row, string Name, string Verdict, bool Recommended);

	/// <summary>K13 (A656): every offer of the open treasure with its verdict, the recommended one marked; empty while none is open.</summary>
	public static IReadOnlyList<CrucibleOfferVerdict> CrucibleTreasureOffers { get; private set; } = [];

	/// <summary>K13: take the recommended offer.</summary>
	public static void RequestCrucibleTreasure()
	{
		_treasureWanted = CrucibleTreasureRecommendation?.Row;
	}

	/// <summary>
	/// K13: every frame with the treasure open - the recommendation (when <paramref name="recommend"/>), and the pick asked for
	/// with the window's button. With a Thief's Knife the window stays open for a second pick; the offer RSR took is left out.
	/// </summary>
	internal static unsafe void FollowCrucibleTreasure(bool recommend)
	{
		var treasure = (AtkUnitBase*)Svc.GameGui.GetAddonByName("XBMContentsTreasure").Address;
		if (!recommend || !DataCenter.IsInCrucible || InCombat || treasure == null || !treasure->IsVisible)
		{
			CrucibleTreasureOpen = false;
			CrucibleTreasureRecommendation = null;
			CrucibleTreasureOffers = [];
			_treasureTaken.Clear();
			_treasureWanted = null;
			_treasureKey = string.Empty;
			CrucibleTreasureStatus = string.Empty;
			return;
		}

		CrucibleTreasureOpen = true;
		var offers = new List<(int Row, ushort Item, int Price)>();
		for (var i = 0; i < TreasureOffers; i++)
		{
			var index = TreasureFirstOffer + (TreasureOfferStride * i);
			if (index >= treasure->AtkValuesCount || _treasureTaken.Contains(i))
			{
				continue;
			}

			var value = treasure->AtkValues[index];
			var id = value.Type == AtkValueType.UInt ? value.UInt : value.Type == AtkValueType.Int && value.Int > 0 ? (uint)value.Int : 0u;
			if (id is > 0 and <= ushort.MaxValue)
			{
				offers.Add((i, (ushort)id, 0));
			}
		}

		if (_treasureWanted is { } wanted)
		{
			_treasureWanted = null;
			var known = false;
			foreach (var offer in offers)
			{
				known |= offer.Row == wanted;
			}

			if (known)
			{
				Take(treasure, wanted, offers);
			}
		}

		Recommend(offers);
	}

	private static unsafe void Take(AtkUnitBase* treasure, int row, List<(int Row, ushort Item, int Price)> offers)
	{
		ReadOnlySpan<int> sent = [TreasureTakeCallback, row];
		var values = stackalloc AtkValue[sent.Length];
		for (var i = 0; i < sent.Length; i++)
		{
			values[i] = default;
			values[i].Type = AtkValueType.Int;
			values[i].Int = sent[i];
		}

		treasure->FireCallback((uint)sent.Length, values, true);
		_ = _treasureTaken.Add(row);
		foreach (var offer in offers)
		{
			if (offer.Row == row && CrucibleGearTable.ContainsKey(offer.Item))
			{
				// The treasure window does not list the gear held; what RSR took joins what the last shop showed.
				if (_gearSeen is not { } held || held.Run != CrucibleBoard.Run)
				{
					held = (CrucibleBoard.Run, [], 0);
				}

				held.Gear.Add(offer.Item);
				_gearSeen = held;
			}
		}

		CrucibleTreasureStatus = $"took offer {row + 1}";
		DefenseTrace.Line($"crucible treasure: took offer {row + 1}");
	}

	private static unsafe void Recommend(List<(int Row, ushort Item, int Price)> offers)
	{
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

		var gear = CrucibleGearTable;
		var owned = new List<CrucibleGear>();
		var gearSlots = 0;
		if (_gearSeen is { } seen && seen.Run == CrucibleBoard.Run)
		{
			gearSlots = seen.Slots;
			foreach (var id in seen.Gear)
			{
				if (gear.TryGetValue(id, out var piece))
				{
					owned.Add(piece);
				}
			}
		}

		// Before the first shop on the board the slots are unknown: one free is assumed (concept 21, K13, Gegenthese 1).
		if (gearSlots <= owned.Count)
		{
			gearSlots = owned.Count + 1;
		}

		var board = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentXBMStageDetailList.Instance() is var stages && stages != null
			? stages->ContentId : 0;
		var key = $"{string.Join(",", offers)}|{heldItems.Count}|{owned.Count}|{board}|{CrucibleBoard.FightsPerStretch}";
		if (key == _treasureKey)
		{
			return;
		}

		_treasureKey = key;

		// K15 (A690): while no beast gear has come in this run and the player carries the way to the final fight, an offer
		// without gear keeps the bonus Mit leeren Händen (750) - when there is one.
		if (owned.Count == 0 && CarriesToFinal(true, out var carried, out var heaviestWay))
		{
			var bare = offers.FindAll(offer => !gear.ContainsKey(offer.Item));
			if (bare.Count > 0 && bare.Count < offers.Count)
			{
				offers = bare;
				DefenseTrace.Line($"crucible score: treasure without gear for the bonus Mit leeren Händen - you carry the heaviest way "
					+ $"to the final fight ({carried:P0} against {heaviestWay:P0})");
			}
		}

		var picks = RankOffers(offers, int.MaxValue, 1, owned, gearSlots, heldItems, itemSlots, board, out _, true);
		CrucibleShopPick? pick = picks.Count > 0 ? picks[0] : null;

		// Nothing adds safety: a damage item or a buff (K8 spends them in the final fight), else gear that does not harm you.
		if (pick == null)
		{
			var items = CrucibleItems;
			foreach (var (row, id, _) in offers)
			{
				if (items.TryGetValue(id, out var item) && item.Kind is CrucibleItemKind.Damage or CrucibleItemKind.Buff)
				{
					pick = new CrucibleShopPick(row, ClientItemName(id, item.Name), 0, "nothing here adds safety; for damage in the final fight");
					break;
				}
			}

			foreach (var (row, id, _) in offers)
			{
				if (pick == null && gear.TryGetValue(id, out var piece) && !piece.SelfHarm && !owned.Contains(piece))
				{
					pick = new CrucibleShopPick(row, ClientItemName(id, piece.Name), 0, "nothing here adds safety; it does not harm you");
				}
			}
		}

		CrucibleTreasureRecommendation = pick;

		// Every offer with its verdict (A656): what the scale gives it, else why it stands behind the pick.
		var verdicts = new List<CrucibleOfferVerdict>(offers.Count);
		foreach (var (row, id, _) in offers)
		{
			var chosenRow = pick is { } p && p.Row == row;
			var verdict = chosenRow ? pick!.Value.Why : string.Empty;
			if (!chosenRow)
			{
				foreach (var weighed in _offerScale)
				{
					if (weighed.Row == row)
					{
						verdict = weighed.Why;
					}
				}
			}

			if (verdict.Length == 0)
			{
				verdict = gear.TryGetValue(id, out var worn) && owned.Contains(worn) ? "you already wear it"
					: CrucibleBoard.Loads == null ? "behind the pick in the order before a fight is measured: a second life, deadly statuses, protective gear, other statuses, healing, protection"
					: "behind the pick";
			}

			verdicts.Add(new CrucibleOfferVerdict(row, ClientItemName(id, EnglishItemName(id)), verdict, chosenRow));
		}

		CrucibleTreasureOffers = verdicts;
		DefenseTrace.Line($"crucible treasure: {offers.Count} offers, weighed against {CrucibleShopBasis} - recommends "
			+ (pick is { } chosen ? $"{chosen.Name} (offer {chosen.Row + 1}): {chosen.Why}" : "nothing"));
	}
}
