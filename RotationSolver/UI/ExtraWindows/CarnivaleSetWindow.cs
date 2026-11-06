using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// A627 (concept 01, "Satz je Stufe aus den Stufenführern"): while the Masked Carnivale briefing is open and a Blue Mage
/// rotation runs, the spells Blue Academy's guide names for the chosen stage, and a button that puts them into the active
/// set. Nothing changes without the click.
/// </summary>
internal class CarnivaleSetWindow : Window
{
	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

	public CarnivaleSetWindow() : base("RSR - Masked Carnivale set", WindowFlags)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BlueMageRotation && BlueMageRotation.CarnivaleStage > 0;
	}

	public override void Draw()
	{
		if (ImGui.Button("<"))
		{
			BlueMageRotation.ShiftCarnivaleStage(-1);
		}

		ImGui.SameLine();
		ImGui.TextUnformatted($"Stage {BlueMageRotation.CarnivaleStage}: {BlueMageRotation.CarnivaleStageName}");
		ImGui.SameLine();
		if (ImGui.Button(">"))
		{
			BlueMageRotation.ShiftCarnivaleStage(1);
		}

		var picks = BlueMageRotation.CarnivaleProposal;
		if (picks.Count == 0)
		{
			ImGui.TextUnformatted("Blue Academy's guide names no spell for this stage.");
			return;
		}

		foreach (var pick in picks)
		{
			var mark = pick.Spell == 0 ? "?" : pick.InSet ? " " : "+";
			ImGui.TextUnformatted($"{mark} {(pick.Required ? "Required" : "Recommended")}: {pick.Text}");
		}

		ImGui.TextUnformatted("+ goes into the set, ? is not learned. Source: mage.blue stage guides.");
		if (ImGui.Button("Apply") && !BlueMageRotation.CarnivaleApplying)
		{
			BlueMageRotation.RequestCarnivaleSet();
		}

		if (BlueMageRotation.CarnivaleStatus.Length > 0)
		{
			ImGui.TextUnformatted(BlueMageRotation.CarnivaleStatus);
		}
	}
}
