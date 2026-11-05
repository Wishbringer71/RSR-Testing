using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K13 of the Crucible (concept 01, "K13 – Schatz", A578): while a treasure on a board is open and the beastmaster rotation
/// runs, the offer RSR recommends taking, why, and a button that takes it. Nothing is taken without the click (decision C,
/// A533).
/// </summary>
internal class CrucibleTreasureWindow : Window
{
	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

	public CrucibleTreasureWindow() : base("RSR - Crucible treasure", WindowFlags)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation && BeastmasterRotation.CrucibleTreasureOpen;
	}

	public override void Draw()
	{
		if (BeastmasterRotation.CrucibleTreasureRecommendation is { } pick)
		{
			ImGui.TextUnformatted($"Take {pick.Name} (offer {pick.Row + 1}): {pick.Why}");
			if (ImGui.Button("Take"))
			{
				BeastmasterRotation.RequestCrucibleTreasure();
			}
		}
		else
		{
			ImGui.TextUnformatted("Nothing recommended: no offer adds safety or damage, and the gear here would harm you.");
		}

		if (BeastmasterRotation.CrucibleTreasureStatus.Length > 0)
		{
			ImGui.TextUnformatted(BeastmasterRotation.CrucibleTreasureStatus);
		}
	}
}
