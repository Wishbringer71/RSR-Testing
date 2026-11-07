using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// A627 (concept 22, "Satz je Stufe aus den Stufenführern"): while the Masked Carnivale briefing is open and a Blue Mage
/// rotation runs, the spells Blue Academy's guide names for the chosen stage, and a button that puts them into the active
/// set. Nothing changes without the click.
/// </summary>
internal class CarnivaleSetWindow : RecommendationWindow
{
	public CarnivaleSetWindow() : base("RSR - Masked Carnivale set")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BlueMageRotation && BlueMageRotation.CarnivaleStage > 0;
	}

	protected override void DrawContent()
	{
		if (Action("##carnivale_previous", "", FontAwesomeIcon.ChevronLeft))
		{
			BlueMageRotation.ShiftCarnivaleStage(-1);
		}

		ImGui.SameLine();
		Title($"Stage {BlueMageRotation.CarnivaleStage}: {BlueMageRotation.CarnivaleStageName}");
		ImGui.SameLine();
		if (Action("##carnivale_next", "", FontAwesomeIcon.ChevronRight))
		{
			BlueMageRotation.ShiftCarnivaleStage(1);
		}

		var picks = BlueMageRotation.CarnivaleProposal;
		using (Section("carnivale_set", "Spells for this stage", FontAwesomeIcon.BookOpen, "mage.blue stage guides"))
		{
			if (picks.Count == 0)
			{
				Note("Blue Academy's guide names no spell for this stage.");
			}

			foreach (var pick in picks)
			{
				Line(pick.Text, pick.Spell == 0 ? "not learned" : pick.InSet ? "in set" : "adds",
					pick.Spell == 0 ? M3Severity.Warning : pick.InSet ? M3Severity.Info : M3Severity.Success);
				Note(pick.Required ? "required" : "recommended");
			}
		}

		if (Action("##carnivale_apply", "Apply", FontAwesomeIcon.Check, picks.Count > 0 && !BlueMageRotation.CarnivaleApplying))
		{
			BlueMageRotation.RequestCarnivaleSet();
		}

		Note(BlueMageRotation.CarnivaleStatus);
	}
}
