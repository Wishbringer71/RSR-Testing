using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K10 and K14 of the Crucible (concept 21, A564, A631): while a campsite is open and the beastmaster rotation runs, whom RSR
/// selects to rest and why, and at the last campsite before the final fight whom to rest for it, with a button that selects
/// them; while a familiar is to be fed, whom the feed is meant for. Resting and feeding stay the player's clicks.
/// </summary>
internal class CrucibleCampWindow : RecommendationWindow
{
	public CrucibleCampWindow() : base("RSR - Crucible campsite")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation
			&& (BeastmasterRotation.CrucibleCampOpen || BeastmasterRotation.CrucibleFeedTarget.Length > 0);
	}

	public override void Draw()
	{
		if (BeastmasterRotation.CrucibleFeedTarget.Length > 0)
		{
			using (Section("crucible_feeding", "Feeding", FontAwesomeIcon.Utensils))
			{
				Line(BeastmasterRotation.CrucibleFeedTarget);
			}

			return;
		}

		using (Section("crucible_camp", "Campsite", FontAwesomeIcon.Campground))
		{
			Note(BeastmasterRotation.CrucibleCampSummary);
		}

		var picks = BeastmasterRotation.CrucibleCampRecommendation;
		if (picks.Count == 0)
		{
			return;
		}

		using (Section("crucible_camp_final", "Rest for the final fight", FontAwesomeIcon.Bed, "last campsite before it"))
		{
			foreach (var pick in picks)
			{
				Line($"{pick.Pet}", $"{pick.Share:P0}", M3Severity.Info);
				Note(pick.Why);
			}

			Note(BeastmasterRotation.CrucibleCampOutcome);
		}

		if (Action("##crucible_camp_apply", "Apply", FontAwesomeIcon.Check))
		{
			BeastmasterRotation.RequestCrucibleCamp();
		}
	}
}
