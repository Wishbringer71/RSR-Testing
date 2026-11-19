using RotationSolver.Rotations.Limited.Beastmaster;
using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
namespace RotationSolver.UI.Windows;

/// <summary>
/// K17 of the Crucible (concept 21, A892): while the spoils of a fight are open and the beastmaster rotation runs, which pieces
/// to take and why. Taking stays the player's click in the game's window.
/// </summary>
internal sealed class CrucibleSpoilsWindow : RecommendationWindow
{
	public CrucibleSpoilsWindow() : base("RSR - Crucible spoils")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation && BeastmasterRotation.CrucibleSpoilsOpen;
	}

	protected override void DrawContent()
	{
		var offers = BeastmasterRotation.CrucibleSpoilsOffers;
		using (Section("crucible_spoils", BeastmasterRotation.CrucibleSpoilsTakeAll ? "Take all" : "Take", FontAwesomeIcon.Gift,
			"each piece, what it brings you over the rest of the board"))
		{
			if (offers.Count == 0)
			{
				Note("Nothing left to take.");
			}

			using var list = List("##crucible_spoils_pieces", offers.Count);
			var index = 0;
			foreach (var offer in offers)
			{
				list.Item(index++);
				Line(offer.Name, offer.Recommended ? "take" : "leave", offer.Recommended ? M3Severity.Success : M3Severity.Neutral);
				Note(offer.Verdict);
			}
		}

		Footer();
		Note("Take them in the game's window.");
	}
}
