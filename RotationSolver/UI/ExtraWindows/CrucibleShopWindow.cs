using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K10 of the Crucible (concept 21, Laden, A569; feed for the final fight, K14, A631): while a shop on a board is open and the
/// beastmaster rotation runs, what RSR recommends buying, why, what it weighs the protective gear against, a button that buys
/// the list, and the feed for the final fight. Nothing is bought without the click (decision C, A533).
/// </summary>
internal class CrucibleShopWindow : RecommendationWindow
{
	public CrucibleShopWindow() : base("RSR - Crucible shop")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation && BeastmasterRotation.CrucibleShopOpen;
	}

	public override void Draw()
	{
		var picks = BeastmasterRotation.CrucibleShopRecommendation;
		var (tokens, left) = BeastmasterRotation.CrucibleShopTokens;
		using (Section("crucible_shop", "Buy", FontAwesomeIcon.ShoppingCart, $"{tokens} tokens, {left} left after these"))
		{
			if (picks.Count == 0)
			{
				Note("Nothing here adds safety within your tokens and slots.");
			}

			foreach (var pick in picks)
			{
				Line(pick.Name, $"{pick.Price}");
				Note(pick.Why);
			}

			if (BeastmasterRotation.CrucibleShopBasis.Length > 0)
			{
				Note($"Gear weighed against {BeastmasterRotation.CrucibleShopBasis}.");
			}
		}

		if (Action("##crucible_shop_buy", "Buy", FontAwesomeIcon.ShoppingCart, picks.Count > 0))
		{
			BeastmasterRotation.RequestCrucibleShop();
		}

		Note(BeastmasterRotation.CrucibleShopStatus);

		// K14 (A631): feed for the final fight, one per familiar of its plan; bought and fed by hand.
		var feed = BeastmasterRotation.CrucibleFeedRecommendation;
		if (feed.Count > 0 || BeastmasterRotation.CrucibleFeedNote.Length > 0)
		{
			using (Section("crucible_feed", "Feed for the final fight", FontAwesomeIcon.Utensils, "buy and feed by hand"))
			{
				foreach (var pick in feed)
				{
					Line($"{pick.Feed} -> {pick.Pet}", $"{pick.Price}", pick.Affordable ? M3Severity.Success : M3Severity.Warning);
					Note(pick.Affordable ? pick.Why : $"{pick.Why}; beyond the tokens the list leaves");
				}

				Note(BeastmasterRotation.CrucibleFeedNote);
			}
		}
	}
}
