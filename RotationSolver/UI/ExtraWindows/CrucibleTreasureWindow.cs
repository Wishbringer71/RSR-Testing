using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K13 of the Crucible (concept 21, A578): while a treasure on a board is open and the beastmaster rotation runs, the offer RSR
/// recommends, why, and a button that takes it. Nothing is taken without the click.
/// </summary>
internal class CrucibleTreasureWindow : RecommendationWindow
{
	public CrucibleTreasureWindow() : base("RSR - Crucible treasure")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation && BeastmasterRotation.CrucibleTreasureOpen;
	}

	protected override void DrawContent()
	{
		var pick = BeastmasterRotation.CrucibleTreasureRecommendation;
		using (Section("crucible_treasure", "Take", FontAwesomeIcon.Gift))
		{
			if (pick is { } chosen)
			{
				Line(chosen.Name, $"offer {chosen.Row + 1}");
				Note(chosen.Why);
			}
			else
			{
				Note("Nothing recommended: no offer adds safety or damage, and the gear here would harm you.");
			}
		}

		if (Action("##crucible_treasure_take", "Take", FontAwesomeIcon.HandHolding, pick != null))
		{
			BeastmasterRotation.RequestCrucibleTreasure();
		}

		Note(BeastmasterRotation.CrucibleTreasureStatus);
	}
}
