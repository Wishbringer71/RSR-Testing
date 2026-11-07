using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K10 "Weg" of the Crucible (concept 21, A632): between fights on a board and with a fork ahead, each way with where its next
/// fight leaves the player and the final fight's healers, and the way RSR recommends. Walking stays the player's.
/// </summary>
internal class CrucibleRouteWindow : RecommendationWindow
{
	public CrucibleRouteWindow() : base("RSR - Crucible route")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation
			&& BeastmasterRotation.CrucibleRouteHint.Count > 0;
	}

	protected override void DrawContent()
	{
		using (Section("crucible_route", "Next fork", FontAwesomeIcon.Route))
		{
			foreach (var option in BeastmasterRotation.CrucibleRouteHint)
			{
				Line(option.Way, option.Recommended ? "take" : option.Safe ? "safe" : null,
					option.Recommended ? M3Severity.Success : M3Severity.Info);
				Note(option.Outcome);
			}

			Note(BeastmasterRotation.CrucibleRouteWhy);
			Note(BeastmasterRotation.CrucibleRouteBasis);
		}
	}
}
