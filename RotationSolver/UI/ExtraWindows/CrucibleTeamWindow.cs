using Dalamud.Interface;
using ECommons.DalamudServices;
using RebornMaterial;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// K7 of the Crucible (concept 21, A563): while the team screen of a board is open and the beastmaster rotation runs, the
/// team RSR recommends, why, and a button that makes the team that. Nothing changes without the click (decision C, A533).
/// </summary>
internal sealed class CrucibleTeamWindow : RecommendationWindow
{
	public CrucibleTeamWindow() : base("RSR - Crucible team")
	{
	}

	public override bool DrawConditions()
	{
		return Svc.ClientState.IsLoggedIn && DataCenter.CurrentRotation is BeastmasterRotation
			&& BeastmasterRotation.CrucibleTeamRecommendation.Count > 0;
	}

	// A804: the goals of a run, in the order of CrucibleGoal.
	private static readonly M3Segment[] Goals =
	[
		new("Level", Tooltip: "Every place below the board's cap, the lowest first; above it only while the team is endangered."),
		new("Safe win", Tooltip: "The full team, the lowest as long as passing is safe; stronger ones while it is endangered or unmeasured."),
		new("Score", Tooltip: "The strongest, as few as are not endangered; a team of one kin where it scores more."),
	];

	// A821: the segments of the goals offered on this board, rebuilt only when the offer changes.
	private (bool Level, bool SafeWin, bool Score) _offered;
	private readonly List<M3Segment> _shown = [];
	private readonly List<CrucibleGoal> _shownGoals = [];
	private M3Segment[] _shownArray = [];

	protected override void DrawContent()
	{
		using (Section("crucible_goal", "Goal of the runs", FontAwesomeIcon.Flag, "Kept until you change it; the team follows it, the board always plays safe, then for points"))
		{
			// A821: only the goals that hold on this board, the one played on it marked; the chosen one stays saved.
			var offered = BeastmasterRotation.CrucibleGoalsOffered;
			if (offered != _offered)
			{
				_offered = offered;
				_shown.Clear();
				_shownGoals.Clear();
				for (var i = 0; i < Goals.Length; i++)
				{
					if ((CrucibleGoal)i switch { CrucibleGoal.Level => offered.Level, CrucibleGoal.Score => offered.Score, _ => offered.SafeWin })
					{
						_shown.Add(Goals[i]);
						_shownGoals.Add((CrucibleGoal)i);
					}
				}

				_shownArray = [.. _shown];
			}

			var played = BeastmasterRotation.CrucibleGoalPlayed;
			var picked = Choice("##crucible_goal", _shownArray, _shownGoals.IndexOf(played));
			if (picked >= 0 && picked < _shownGoals.Count && _shownGoals[picked] != BeastmasterRotation.CrucibleGoalNow
				&& DataCenter.CurrentRotation is { } rotation && BeastmasterRotation.ChooseCrucibleGoal(rotation, _shownGoals[picked]))
			{
				Service.Config.Save();
			}

			Note(Goals[(int)played].Tooltip ?? string.Empty);
			if (BeastmasterRotation.CrucibleGoalWhyNot.Length > 0)
			{
				Note(BeastmasterRotation.CrucibleGoalWhyNot);
			}
		}

		var picks = BeastmasterRotation.CrucibleTeamRecommendation;
		var team = BeastmasterRotation.CrucibleTeamNow();
		var wanted = new List<BeastmasterPet>();
		using (Section("crucible_team", "Team for this board", FontAwesomeIcon.Users, $"{picks.Count} familiars"))
		{
			using (var list = List("##crucible_team_picks", picks.Count))
			{
				for (var i = 0; i < picks.Count; i++)
				{
					list.Item(i);
					var pick = picks[i];
					wanted.Add(pick.Pet);
					Line($"{pick.Pet}", pick.InTeam ? "in team" : "joins", pick.InTeam ? M3Severity.Info : M3Severity.Success);
					Note(pick.Why);
				}
			}

			// A816: who leaves has no reason of its own (the reason stands with who joins) - one line for all of them.
			var leaving = new List<string>();
			foreach (var member in team)
			{
				if (!wanted.Contains(member))
				{
					leaving.Add($"{member}");
				}
			}

			if (leaving.Count > 0)
			{
				Line(string.Join(", ", leaving), "leaves", M3Severity.Warning);
			}

			if (BeastmasterRotation.CrucibleTeamSizeNote.Length > 0)
			{
				Note(BeastmasterRotation.CrucibleTeamSizeNote);
			}
		}

		Footer();

		// A821 (his rule of 15.11.2026): with the team as recommended there is nothing to apply. Starting the board from here
		// waits until his trace shows the event the game's start button sends (concept 21, K7).
		var ready = picks.Count > 0 && !BeastmasterRotation.CrucibleTeamRequestRunning && team.Count == wanted.Count
			&& wanted.TrueForAll(team.Contains);
		if (ready)
		{
			Line("Team ready", "start the board in the game's window", M3Severity.Success);
		}
		else if (Action("##crucible_team_apply", "Apply", FontAwesomeIcon.Check, picks.Count > 0))
		{
			BeastmasterRotation.RequestCrucibleTeam(wanted);
		}

		Note(BeastmasterRotation.CrucibleTeamStatus);
	}
}
