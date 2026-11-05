using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K7 of the Crucible (concept 01, A563): while the team screen of a board is open and the beastmaster rotation runs, the
/// team RSR recommends, why, and a button that makes the team that. Nothing changes without the click (decision C, A533).
/// </summary>
internal class CrucibleTeamWindow : Window
{
	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

	public CrucibleTeamWindow() : base("RSR - Crucible team", WindowFlags)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation
			&& BeastmasterRotation.CrucibleTeamRecommendation.Count > 0;
	}

	public override void Draw()
	{
		var picks = BeastmasterRotation.CrucibleTeamRecommendation;
		var team = BeastmasterRotation.CrucibleTeamNow();
		ImGui.TextUnformatted($"Recommended for this board ({picks.Count}):");
		foreach (var pick in picks)
		{
			ImGui.TextUnformatted($"{(pick.InTeam ? "  " : "+ ")}{pick.Pet}: {pick.Why}");
		}

		var wanted = new List<BeastmasterPet>();
		foreach (var pick in picks)
		{
			wanted.Add(pick.Pet);
		}

		foreach (var member in team)
		{
			if (!wanted.Contains(member))
			{
				ImGui.TextUnformatted($"- {member}: leaves the team");
			}
		}

		if (ImGui.Button("Apply") && picks.Count > 0)
		{
			BeastmasterRotation.RequestCrucibleTeam(wanted);
		}

		if (BeastmasterRotation.CrucibleTeamStatus.Length > 0)
		{
			ImGui.TextUnformatted(BeastmasterRotation.CrucibleTeamStatus);
		}
	}
}
