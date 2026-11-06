using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K10 and K14 of the Crucible (concept 01, A564, A631): while a campsite is open and the beastmaster rotation runs, whom RSR
/// selects to rest and why, and at the last campsite before the final fight whom to rest for it, with a button that selects
/// them; while a familiar is to be fed, whom the feed is meant for. Resting and feeding stay the player's clicks.
/// </summary>
internal class CrucibleCampWindow : Window
{
	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

	public CrucibleCampWindow() : base("RSR - Crucible campsite", WindowFlags)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
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
			ImGui.TextUnformatted(BeastmasterRotation.CrucibleFeedTarget);
			return;
		}

		if (BeastmasterRotation.CrucibleCampSummary.Length > 0)
		{
			ImGui.TextUnformatted(BeastmasterRotation.CrucibleCampSummary);
		}

		var picks = BeastmasterRotation.CrucibleCampRecommendation;
		if (picks.Count == 0)
		{
			return;
		}

		ImGui.TextUnformatted("For the final fight, rest with:");
		foreach (var pick in picks)
		{
			ImGui.TextUnformatted($"  {pick.Pet} at {pick.Share:P0}: {pick.Why}");
		}

		ImGui.TextUnformatted(BeastmasterRotation.CrucibleCampOutcome);
		if (ImGui.Button("Apply"))
		{
			BeastmasterRotation.RequestCrucibleCamp();
		}
	}
}
