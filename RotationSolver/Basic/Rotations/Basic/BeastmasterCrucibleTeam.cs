using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K7 of the Crucible (concept 21, "Stufe 4, zweiter Teil: K7, Kader auf Wunsch", A563): with the team screen open, the
/// team RSR recommends for the board, and on request the steps that make the team that - one TogglePet at a time.
/// </summary>
public partial class BeastmasterRotation
{
	// Familiars that heal or protect you on the horns before a boss. With one, a danger that came after its stint found
	// none in 36 of 324 modelled fights; with two, no danger was covered worse than with his own horns, at 0.3 % of the
	// boss's potency on average where none came (bst_assign_model.py, 04.11.2026). B5 and K6 put that many on the horns
	// before a boss; K7 takes that many into the team.
	internal const int BossSafeFamiliars = 2;

	/// <summary>One recommended familiar and why: what it brings to the board, and whether it is in the team already.</summary>
	public readonly record struct CrucibleTeamPick(BeastmasterPet Pet, string Why, bool InTeam);

	/// <summary>K7 team size (A698): why the recommendation holds as many familiars as it does - the score's bonus for a smaller team, or why not.</summary>
	public static string CrucibleTeamSizeNote { get; private set; } = string.Empty;

	// K7 team size (A698): the team sizes of "In Unterzahl" I to III, read from their conditions ("Clear the board with a team
	// of 6 or 7 familiars."), once.
	private static List<(int Min, int Max, CrucibleBoard.ScoreBonus Bonus, string Name)>? _squads;
	private static readonly System.Text.RegularExpressions.Regex SquadSizes =
		new(@"team of (?<min>\d+) (?:or|to) (?<max>\d+) familiars", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

	private static List<(int Min, int Max, CrucibleBoard.ScoreBonus Bonus, string Name)> Squads()
	{
		if (_squads != null)
		{
			return _squads;
		}

		var squads = new List<(int Min, int Max, CrucibleBoard.ScoreBonus Bonus, string Name)>();
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMScoreBonus>(Dalamud.Game.ClientLanguage.English);
		foreach (var bonus in new[] { CrucibleBoard.ScoreBonus.SquadI, CrucibleBoard.ScoreBonus.SquadII, CrucibleBoard.ScoreBonus.SquadIII })
		{
			if (sheet?.GetRowOrDefault((uint)bonus) is { } row
				&& SquadSizes.Match(row.Unknown1.ExtractText()) is { Success: true } size)
			{
				squads.Add((int.Parse(size.Groups["min"].Value, System.Globalization.CultureInfo.InvariantCulture),
					int.Parse(size.Groups["max"].Value, System.Globalization.CultureInfo.InvariantCulture), bonus, row.Unknown0.ExtractText()));
			}
		}

		squads.Sort((a, b) => a.Max.CompareTo(b.Max));
		return _squads = squads;
	}

	// K7 team size (A698): the smallest size of an "In Unterzahl" step - its upper bound - that holds the core of the
	// recommendation and whose HP without one familiar hold the most the familiars lost in one run on this board; the full
	// size while no run there was measured. The mean HP of the familiars on the team screen stand for each (a conclusion:
	// no sheet gives the HP of the others).
	private static unsafe int TeamSizeForScore(uint board, int size, int core, out string note)
	{
		note = string.Empty;
		var party = AgentXBMPetParty.Instance();
		long hp = 0;
		var counted = 0;
		if (party != null)
		{
			var team = party->SelectedPets.AsSpan();
			for (var i = 0; i < team.Length && i < party->PetMaxHealth.Length; i++)
			{
				if ((BeastmasterPet)team[i].PetId != BeastmasterPet.None && party->PetMaxHealth[i] > 0)
				{
					hp += party->PetMaxHealth[i];
					counted++;
				}
			}
		}

		var loss = CrucibleBoard.FamiliarLossOf(board);
		var mean = counted > 0 ? hp / (double)counted : 0d;
		foreach (var (_, max, bonus, name) in Squads())
		{
			if (max >= size || max < core)
			{
				continue;
			}

			var points = CrucibleBoard.BonusPoints(bonus);
			if (loss is not { } need || mean <= 0d)
			{
				note = $"Full team: a team of {max} would bring \"{name}\" ({points} points), but no run on this board has measured "
					+ "what the familiars lose here yet.";
				continue;
			}

			var holds = (max - 1) * mean;
			if (holds >= need)
			{
				note = $"{max} familiars: \"{name}\", {points} points. The familiars lost at most {need} HP in a run here; "
					+ $"{max - 1} of them hold about {holds:F0}, one more stands in reserve.";
				return max;
			}

			note = $"Full team: {max - 1} familiars hold about {holds:F0} HP, less than the {need} the familiars lost in a run here.";
		}

		return size;
	}

	private static List<BeastmasterPet>? _teamWanted;
	private static (BeastmasterPet Pet, bool Add, DateTime At)? _teamLastToggle;
	private static readonly HashSet<BeastmasterPet> _teamRefused = [];

	/// <summary>What the team change did last, for the window.</summary>
	public static string CrucibleTeamStatus { get; private set; } = string.Empty;

	/// <summary>The recommendation while the team screen is open, computed in the update (it asks the game for ranks);
	/// the window only reads it. Empty while the screen is closed.</summary>
	public static IReadOnlyList<CrucibleTeamPick> CrucibleTeamRecommendation { get; private set; } = [];

	/// <summary>
	/// Whether the Crucible's team screen is open: the pet-party agent active, not in the horn screen, with a board in
	/// ContentId (Daedalus 74659a6, CrucibleRosterLoader), and not on a board - the team is fixed for the run, and there the
	/// same agent stands at the campsite, while feeding and between fights (his report of 06.11.2026, A634).
	/// <paramref name="board"/> is that XBMContent row.
	/// </summary>
	public static unsafe bool CrucibleTeamScreenOpen(out uint board)
	{
		board = 0;
		var party = AgentXBMPetParty.Instance();
		if (DataCenter.IsInCrucible || party == null || !((AgentInterface*)party)->IsAgentActive()
			|| party->Mode == CrucibleBoard.HornScreenMode || party->ContentId == 0)
		{
			return false;
		}

		board = party->ContentId;
		return true;
	}

	/// <summary>The team as the screen lists it now.</summary>
	public static unsafe List<BeastmasterPet> CrucibleTeamNow()
	{
		var team = new List<BeastmasterPet>();
		var party = AgentXBMPetParty.Instance();
		if (party == null)
		{
			return team;
		}

		foreach (var member in party->SelectedPets.AsSpan())
		{
			if (member.PetId != 0 && member.PetId <= byte.MaxValue)
			{
				team.Add((BeastmasterPet)member.PetId);
			}
		}

		return team;
	}

	/// <summary>
	/// K7: the team RSR recommends for <paramref name="board"/> - as many as it holds; first as many that heal or protect as
	/// B5 wants before a boss, then one by one the familiar that hits the weakness of the most enemies of the board's fights
	/// not yet covered (each enemy of a weakness counts one, shared by the familiars already chosen that hit it); at equal
	/// worth one above the board's rank first, then one in the team already. With <paramref name="levelLagging"/> (his rule
	/// of 05.11.2026, A579) each pick, from the highest rank down, gives way to the lowest-ranked familiar outside the team
	/// that replaces it adequately: it hits every weakness only that pick hits in the team, heals or protects where that is
	/// still needed, stands at least at the board's recommended rank, and ranks below the pick and below the highest rank.
	/// </summary>
	internal static unsafe List<CrucibleTeamPick> RecommendCrucibleTeam(uint board, bool levelLagging)
	{
		var picks = new List<CrucibleTeamPick>();
		var size = CrucibleBoard.TeamSize(board);
		if (size <= 0)
		{
			return picks;
		}

		var weight = new Dictionary<uint, int>();
		var interrupting = false;
		foreach (var fight in CrucibleBoard.FightsOfBoard(board))
		{
			// K4 (A587): a fight with an enemy kind seen casting interruptibly asks for a Soulkin familiar (Soul Crush).
			interrupting |= CrucibleBoard.CastsInterruptibly(CrucibleBoard.EnemiesOf(fight));
			foreach (var enemy in CrucibleBoard.EnemiesOf(fight))
			{
				if (enemy.Weakness != 0)
				{
					weight[enemy.Weakness] = weight.GetValueOrDefault(enemy.Weakness) + 1;
				}
			}
		}

		var team = CrucibleTeamNow();
		var party = AgentXBMPetParty.Instance();
		var candidates = new List<TeamCandidate>();
		foreach (var pet in Enum.GetValues<BeastmasterPet>())
		{
			if (pet == BeastmasterPet.None || _teamRefused.Contains(pet) || FamiliarUnlocked(pet) != true || FamiliarOf(pet) is not { } profile)
			{
				continue;
			}

			var safe = profile.TrickHeals || profile.ReleaseHeals || profile.ReleaseProtects || profile.BorrowProtects;
			var overRank = party != null && party->IsPetOverSyncedRank((uint)pet);
			var rank = party != null ? (int)party->GetSyncedRank((uint)pet) : 0;
			candidates.Add(new TeamCandidate(pet, ElementsOf(profile), safe, overRank, rank));
		}

		var chosen = new List<TeamCandidate>();
		var covered = new Dictionary<uint, int>();
		var safeTaken = 0;
		var soulTaken = false;
		var core = 0;
		while (chosen.Count < size && candidates.Count > 0)
		{
			var needSafe = safeTaken < BossSafeFamiliars;
			var needSoul = !needSafe && interrupting && !soulTaken && HasSoul();
			var best = -1;
			var bestWorth = -1f;
			for (var i = 0; i < candidates.Count; i++)
			{
				var candidate = candidates[i];
				if ((needSafe && !candidate.Safe && HasSafe()) || (needSoul && KinOf(candidate.Pet) != BeastmasterKinType.Soulkin))
				{
					continue;
				}

				var worth = 0f;
				foreach (var element in candidate.Hits)
				{
					worth += weight.GetValueOrDefault(element) / (1f + covered.GetValueOrDefault(element));
				}

				if (best < 0 || worth > bestWorth || (worth == bestWorth && Before(candidate, candidates[best])))
				{
					best = i;
					bestWorth = worth;
				}
			}

			var pick = candidates[best];
			candidates.RemoveAt(best);
			chosen.Add(pick);
			foreach (var element in pick.Hits)
			{
				covered[element] = covered.GetValueOrDefault(element) + 1;
			}

			safeTaken += pick.Safe ? 1 : 0;
			soulTaken |= KinOf(pick.Pet) == BeastmasterKinType.Soulkin;

			// A698: the core - up to the last pick that covers a weakness anew or is needed as a healer or interrupter.
			if (bestWorth > 0f || needSafe || needSoul)
			{
				core = chosen.Count;
			}
		}

		// A698: as few as the score's bonus for a smaller team allows with the core and the familiars' measured loss.
		var keep = TeamSizeForScore(board, size, core, out var sizeNote);
		CrucibleTeamSizeNote = sizeNote;
		for (var i = chosen.Count - 1; i >= keep; i--)
		{
			var dropped = chosen[i];
			chosen.RemoveAt(i);
			candidates.Add(dropped);
			foreach (var element in dropped.Hits)
			{
				covered[element] = covered.GetValueOrDefault(element) - 1;
			}
		}

		// A579: leveling - a pick gives way to the lowest-ranked familiar that replaces it adequately.
		var replaced = new Dictionary<BeastmasterPet, TeamCandidate>();
		if (levelLagging && party != null)
		{
			var recommended = CrucibleBoard.RecommendedRank(board);
			var maxRank = CrucibleBoard.MaxRank;
			var byRank = new List<TeamCandidate>(chosen);
			byRank.Sort((a, b) => a.OverRank != b.OverRank ? (a.OverRank ? -1 : 1) : b.Rank.CompareTo(a.Rank));
			foreach (var pick in byRank)
			{
				var unique = new List<uint>();
				foreach (var element in pick.Hits)
				{
					if (weight.ContainsKey(element) && covered.GetValueOrDefault(element) == 1)
					{
						unique.Add(element);
					}
				}

				var safeLeft = 0;
				foreach (var member in chosen)
				{
					safeLeft += member.Safe && member.Pet != pick.Pet ? 1 : 0;
				}

				var needsSafe = pick.Safe && safeLeft < BossSafeFamiliars;
				var substitute = -1;
				for (var i = 0; i < candidates.Count; i++)
				{
					var other = candidates[i];
					if (other.OverRank || other.Rank < recommended || other.Rank >= maxRank
						|| (!pick.OverRank && other.Rank >= pick.Rank) || (needsSafe && !other.Safe) || !other.Hits.IsSupersetOf(unique))
					{
						continue;
					}

					if (substitute < 0 || other.Rank < candidates[substitute].Rank
						|| (other.Rank == candidates[substitute].Rank && team.Contains(other.Pet) && !team.Contains(candidates[substitute].Pet)))
					{
						substitute = i;
					}
				}

				if (substitute < 0)
				{
					continue;
				}

				var joiner = candidates[substitute];
				candidates.RemoveAt(substitute);
				candidates.Add(pick);
				_ = chosen.Remove(pick);
				chosen.Add(joiner);
				replaced[joiner.Pet] = pick;
				foreach (var element in pick.Hits)
				{
					covered[element] = covered.GetValueOrDefault(element) - 1;
				}

				foreach (var element in joiner.Hits)
				{
					covered[element] = covered.GetValueOrDefault(element) + 1;
				}
			}
		}

		foreach (var pick in chosen)
		{
			var hit = new List<string>();
			foreach (var element in pick.Hits)
			{
				if (weight.TryGetValue(element, out var enemies))
				{
					hit.Add($"{CrucibleBoard.ElementName(element)} ({enemies})");
				}
			}

			var why = (pick.Safe ? "heals or protects; " : string.Empty)
				+ (hit.Count > 0 ? $"hits {string.Join(", ", hit)}" : "no weakness of this board")
				+ (pick.OverRank ? string.Empty : "; below the board's rank")
				+ (replaced.TryGetValue(pick.Pet, out var gave) ? $"; levels: rank {pick.Rank} in place of {gave.Pet} ({(gave.OverRank ? "above the board's rank" : $"rank {gave.Rank}")})" : string.Empty);
			picks.Add(new CrucibleTeamPick(pick.Pet, why, team.Contains(pick.Pet)));
		}

		return picks;

		bool HasSafe()
		{
			foreach (var candidate in candidates)
			{
				if (candidate.Safe)
				{
					return true;
				}
			}

			return false;
		}

		bool HasSoul()
		{
			foreach (var candidate in candidates)
			{
				if (KinOf(candidate.Pet) == BeastmasterKinType.Soulkin)
				{
					return true;
				}
			}

			return false;
		}

		// At equal worth: above the board's rank first, then one in the team already.
		bool Before(TeamCandidate a, TeamCandidate b)
			=> a.OverRank != b.OverRank ? a.OverRank : team.Contains(a.Pet) && !team.Contains(b.Pet);
	}

	// K7: a familiar the team recommendation weighs - what it hits, whether it heals or protects, its rank on this board.
	private readonly record struct TeamCandidate(BeastmasterPet Pet, HashSet<uint> Hits, bool Safe, bool OverRank, int Rank);

	/// <summary>K7: make the team <paramref name="wanted"/> - leavers first, then joiners, one TogglePet at a time.</summary>
	public static void RequestCrucibleTeam(List<BeastmasterPet> wanted)
	{
		_teamWanted = wanted;
		_teamLastToggle = null;
		CrucibleTeamStatus = "Setting the team...";
		DefenseTrace.Line($"crucible team: requested {string.Join(", ", wanted)}");
	}

	// K7: one step of a requested change each time the team has taken the last one, at most one GCD after it; a joiner the
	// game does not take leaves the recommendation for this session, a leaver it does not take ends the change.
	internal static void FollowTeamRequest(bool levelLagging)
	{
		CrucibleTeamRecommendation = CrucibleTeamScreenOpen(out var board) ? RecommendCrucibleTeam(board, levelLagging) : [];
		if (_teamWanted == null)
		{
			return;
		}

		if (!CrucibleTeamScreenOpen(out _))
		{
			_teamWanted = null;
			CrucibleTeamStatus = "Stopped: the team screen was closed.";
			DefenseTrace.Line("crucible team: " + CrucibleTeamStatus);
			return;
		}

		var team = CrucibleTeamNow();
		if (_teamLastToggle is { } last)
		{
			var took = team.Contains(last.Pet) == last.Add;
			if (!took && (DateTime.Now - last.At).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}

			_teamLastToggle = null;
			if (!took && last.Add)
			{
				_ = _teamRefused.Add(last.Pet);
				_ = _teamWanted.Remove(last.Pet);
				DefenseTrace.Line($"crucible team: the game did not add {last.Pet} - left out for this session");
			}
			else if (!took)
			{
				_teamWanted = null;
				CrucibleTeamStatus = $"The game did not remove {last.Pet} - finish by hand.";
				DefenseTrace.Line("crucible team: " + CrucibleTeamStatus);
				return;
			}
		}

		BeastmasterPet? leave = null;
		foreach (var member in team)
		{
			if (!_teamWanted.Contains(member))
			{
				leave = member;
				break;
			}
		}

		BeastmasterPet? join = null;
		if (leave == null)
		{
			foreach (var wantedPet in _teamWanted)
			{
				if (!team.Contains(wantedPet))
				{
					join = wantedPet;
					break;
				}
			}
		}

		if (leave == null && join == null)
		{
			_teamWanted = null;
			CrucibleTeamStatus = $"Team set: {team.Count} familiars. Confirm it on the screen.";
			DefenseTrace.Line("crucible team: " + CrucibleTeamStatus);
			return;
		}

		var pet = leave ?? join!.Value;
		unsafe
		{
			var party = AgentXBMPetParty.Instance();
			if (party == null)
			{
				return;
			}

			party->TogglePet((uint)pet);
		}

		_teamLastToggle = (pet, leave == null, DateTime.Now);
		CrucibleTeamStatus = $"{(leave == null ? "Adding" : "Removing")} {pet}...";
		DefenseTrace.Line($"crucible team: {(leave == null ? "adding" : "removing")} {pet}");
	}
}
