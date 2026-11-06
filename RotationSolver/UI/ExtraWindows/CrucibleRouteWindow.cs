using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K10 "Weg" of the Crucible (concept 01, A632): between fights on a board and with a fork ahead, each way with where its next
/// fight leaves the player and the final fight's healers, and the way RSR recommends. Walking stays the player's.
/// </summary>
internal class CrucibleRouteWindow : Window
{
	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

	public CrucibleRouteWindow() : base("RSR - Crucible route", WindowFlags)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation
			&& BeastmasterRotation.CrucibleRouteHint.Count > 0;
	}

	public override void Draw()
	{
		ImGui.TextUnformatted("Next fork:");
		foreach (var option in BeastmasterRotation.CrucibleRouteHint)
		{
			ImGui.TextUnformatted($"{(option.Recommended ? "> " : "  ")}{option.Way}: {option.Outcome}{(option.Safe ? " - safe" : string.Empty)}");
		}

		ImGui.TextUnformatted(BeastmasterRotation.CrucibleRouteBasis);
	}
}
