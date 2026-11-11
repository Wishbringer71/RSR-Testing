using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K7 of the Crucible (concept 21, A563): while the team screen of a board is open and the beastmaster rotation runs, the
/// team RSR recommends, why, and a button that makes the team that. Nothing changes without the click (decision C, A533).
/// </summary>
internal class CrucibleTeamWindow : RecommendationWindow
{
	public CrucibleTeamWindow() : base("RSR - Crucible team")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation
			&& BeastmasterRotation.CrucibleTeamRecommendation.Count > 0;
	}

	protected override void DrawContent()
	{
		var picks = BeastmasterRotation.CrucibleTeamRecommendation;
		var team = BeastmasterRotation.CrucibleTeamNow();
		var wanted = new List<BeastmasterPet>();
		using (Section("crucible_team", "Team for this board", FontAwesomeIcon.Users, $"{picks.Count} familiars"))
		{
			foreach (var pick in picks)
			{
				wanted.Add(pick.Pet);
				Line($"{pick.Pet}", pick.InTeam ? "in team" : "joins", pick.InTeam ? M3Severity.Info : M3Severity.Success);
				Note(pick.Why);
			}

			foreach (var member in team)
			{
				if (!wanted.Contains(member))
				{
					Line($"{member}", "leaves", M3Severity.Warning);
				}
			}

			if (BeastmasterRotation.CrucibleTeamSizeNote.Length > 0)
			{
				Note(BeastmasterRotation.CrucibleTeamSizeNote);
			}
		}

		if (Action("##crucible_team_apply", "Apply", FontAwesomeIcon.Check, picks.Count > 0))
		{
			BeastmasterRotation.RequestCrucibleTeam(wanted);
		}

		Note(BeastmasterRotation.CrucibleTeamStatus);
	}
}
