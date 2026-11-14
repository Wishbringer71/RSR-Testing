using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K7, "Ziel des Laufs" (concept 21, A804; his proposal of 14.11.2026): what a Crucible run plays for - the team RSR
/// recommends and the score bonuses it weighs on the board follow it. Stored by name in the rotation's settings.
/// </summary>
public enum CrucibleGoal : byte
{
	/// <summary>Familiars that lag behind in rank fill the team; without any, as <see cref="Score"/>. Bonuses weighed.</summary>
	[Description("Level")]
	Level,

	/// <summary>
	/// The full team, the stronger at equal worth; every decision on the board as if there were no score bonus - items,
	/// purchases, treasures, rests, feeds, the way - but what the final fight's last GCD buys only points.
	/// </summary>
	[Description("Safe win")]
	SafeWin,

	/// <summary>A smaller team and a team of one kin where the measured losses allow it; bonuses weighed.</summary>
	[Description("Score")]
	Score,
}

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

	/// <summary>The name of the rotation setting that holds the goal of a Crucible run (A804).</summary>
	public const string CrucibleGoalSetting = "CrucibleGoal";

	/// <summary>K7, "Ziel des Laufs" (A804): the goal of the run, as the rotation's setting holds it; set every frame.</summary>
	public static CrucibleGoal CrucibleGoalNow { get; private protected set; } = CrucibleGoal.Level;

	/// <summary>
	/// K15 under the goal (A804): whether the score's bonuses are weighed against the safety of an action - not under
	/// <see cref="CrucibleGoal.SafeWin"/>, where every action goes as its safety rule asks.
	/// </summary>
	private protected static bool WeighsScore => CrucibleGoalNow != CrucibleGoal.SafeWin;

	/// <summary>
	/// K7 (A804): sets the goal of a Crucible run in <paramref name="rotation"/>'s setting, as the settings window would;
	/// false when the rotation has no such setting.
	/// </summary>
	public static bool ChooseCrucibleGoal(ICustomRotation rotation, CrucibleGoal goal)
	{
		foreach (var config in rotation.Configs)
		{
			if (config.Name == CrucibleGoalSetting)
			{
				config.Value = typeof(CrucibleGoal).GetField(goal.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description
					?? goal.ToString();
				if (CrucibleGoalNow != goal)
				{
					DefenseTrace.Line($"crucible team: goal {goal} (was {CrucibleGoalNow})");
				}

				CrucibleGoalNow = goal;
				return true;
			}
		}

		return false;
	}

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

	// K7 team size (A698, A703): the smallest size of an "In Unterzahl" step - its upper bound - that holds the core of the
	// recommendation and on whose familiars but one every use measured on this board fits: the uses, largest first, each
	// on the familiar with the most HP left that still stands after it. The HP are those the team agent shows for the
	// picks, the mean of the team for one outside it (a conclusion: no sheet gives them). The full size while no run there
	// was measured.
	private static int TeamSizeForScore(uint board, int size, int core, List<TeamCandidate> picks, out string note)
	{
		note = string.Empty;
		var shown = ShownHealth(out var mean);
		foreach (var (_, max, bonus, name) in Squads())
		{
			if (max >= size || max < core || max > picks.Count)
			{
				continue;
			}

			var points = CrucibleBoard.BonusPoints(bonus);
			var fits = CarriesLosses(board, picks, max, shown, mean, out var uses);
			if (fits == null)
			{
				note = $"Full team: a team of {max} would bring \"{name}\" ({points} points), but no run on this board has measured "
					+ "what each use of a familiar costs here yet.";
				continue;
			}

			if (fits == true)
			{
				note = $"{max} familiars: \"{name}\", {points} points. The {uses.Count} uses measured on this board, up to {uses[0]} HP "
					+ $"each, fit on {max - 1} of them without one falling; one more stands in reserve.";
				return max;
			}

			note = $"Full team: the {uses.Count} uses measured on this board, up to {uses[0]} HP each, do not fit on {max - 1} familiars "
				+ "without one falling.";
		}

		return size;
	}

	// K7 (A703): the most HP the team screen shows for each familiar of the team as it stands, and their mean - the HP of a
	// familiar outside the team no sheet gives (concept 21, "Teamgröße nach Punkten").
	private static unsafe Dictionary<BeastmasterPet, uint> ShownHealth(out uint mean)
	{
		var shown = new Dictionary<BeastmasterPet, uint>();
		long hp = 0;
		var party = AgentXBMPetParty.Instance();
		if (party != null)
		{
			var team = party->SelectedPets.AsSpan();
			for (var i = 0; i < team.Length && i < party->PetMaxHealth.Length; i++)
			{
				if ((BeastmasterPet)team[i].PetId != BeastmasterPet.None && party->PetMaxHealth[i] > 0)
				{
					shown[(BeastmasterPet)team[i].PetId] = party->PetMaxHealth[i];
					hp += party->PetMaxHealth[i];
				}
			}
		}

		mean = shown.Count > 0 ? (uint)(hp / shown.Count) : 0u;
		return shown;
	}

	// K7 (A703, A729): whether the first `n` of `picks` carry every use of a familiar measured on this board - with a team as
	// strong as they are, or weaker - with the one of fewest HP in reserve; null while no run on the board measured them.
	private static bool? CarriesLosses(uint board, List<TeamCandidate> picks, int n, Dictionary<BeastmasterPet, uint> shown,
		uint mean, out List<uint> uses)
	{
		var sync = CrucibleBoard.SyncRank(board);
		long ranks = 0;
		for (var i = 0; i < n; i++)
		{
			ranks += CrucibleBoard.FightingRank(picks[i].Rank, picks[i].OverRank, sync);
		}

		uses = CrucibleBoard.FamiliarUseLossesOf(board, CrucibleBoard.RankStep(ranks, n));
		if (uses.Count == 0 || mean == 0 || n < 1)
		{
			return null;
		}

		var left = new List<uint>();
		for (var i = 0; i < n; i++)
		{
			left.Add(shown.TryGetValue(picks[i].Pet, out var known) ? known : mean);
		}

		left.Sort();
		left.RemoveAt(0);
		foreach (var use in uses)
		{
			var best = -1;
			for (var i = 0; i < left.Count; i++)
			{
				if (left[i] > use && (best < 0 || left[i] > left[best]))
				{
					best = i;
				}
			}

			if (best < 0)
			{
				return false;
			}

			left[best] -= use;
		}

		return true;
	}

	/// <summary>
	/// K7 "Leveln" (his rule of 05.11.2026, A579; his hints of 14.11.2026, A796): every familiar in the team earns
	/// experience, on a horn or not. The fighters are the core of <paramref name="chosen"/> - every pick that covers a
	/// weakness anew, heals or protects for the boss, interrupts - and the reserve the measured losses of the board ask for
	/// (the full team while none is measured); a reserve place takes a familiar that lags behind and replaces adequately
	/// first - below the highest rank, or above the board's sync where its true rank is unknown, and at least the rank the
	/// game recommends - lowest rank first. The places left up to <paramref name="size"/> go to the other familiars that lag behind, lowest rank first.
	/// Without any familiar that lags behind the team stays as chosen. <paramref name="learning"/> receives each familiar that
	/// lags behind outside the core: true when it learns off the horns, false when it fights in reserve;
	/// <paramref name="laggers"/> counts every one that lags behind, the core's included (A804).
	/// </summary>
	private static List<TeamCandidate> LevelingTeam(uint board, int size, List<TeamCandidate> chosen, List<TeamCandidate> all,
		Dictionary<uint, int> weight, bool interrupting, List<BeastmasterPet> team,
		Dictionary<BeastmasterPet, bool> learning, out string note, out int laggers)
	{
		note = string.Empty;
		laggers = 0;
		var maxRank = CrucibleBoard.MaxRank;
		var recommended = CrucibleBoard.RecommendedRank(board);
		// A803: the game gives the true rank, above the board's sync too; the rank the last result window showed counts where
		// it is higher (A799). Without either a familiar above the sync stands at the sync, unknown, and lags behind (his order
		// of 14.11.2026: all up to the highest rank first; concept 21, K7 "Leveln", Gegenthese 3).
		var sync = CrucibleBoard.SyncRank(board);
		var lagging = new List<TeamCandidate>();
		var known = new Dictionary<BeastmasterPet, bool>();
		foreach (var candidate in all)
		{
			var rank = TrueRank(candidate, sync, out var isKnown);
			known[candidate.Pet] = isKnown;
			var behind = isKnown ? rank < maxRank : sync < maxRank;
			if (rank > 0 && behind)
			{
				lagging.Add(candidate with { Rank = rank });
			}
		}

		laggers = lagging.Count;
		if (lagging.Count == 0)
		{
			note = "No familiar lags behind in rank: the team for the board's fights and the score.";
			return chosen;
		}

		// Lowest rank first, a known rank before an unknown one; at equal rank one not in the team now - the team screen
		// shows the last run's team, so equals take turns (his order of 14.11.2026: "reihum").
		lagging.Sort((a, b) => a.Rank != b.Rank ? a.Rank.CompareTo(b.Rank)
			: known[a.Pet] != known[b.Pet] ? known[b.Pet].CompareTo(known[a.Pet])
			: team.Contains(a.Pet).CompareTo(team.Contains(b.Pet)));
		var core = CoreOf(chosen, weight, interrupting);

		// The reserve in the order it fills: familiars that lag behind and replace adequately, then the greedy picks.
		var reserve = new List<TeamCandidate>();
		foreach (var candidate in lagging)
		{
			if (candidate.Rank >= recommended && !ContainsPet(chosen, core, candidate.Pet))
			{
				reserve.Add(candidate);
			}
		}

		for (var i = core; i < chosen.Count; i++)
		{
			if (!ContainsPet(reserve, reserve.Count, chosen[i].Pet))
			{
				reserve.Add(chosen[i]);
			}
		}

		// Fighters: the smallest count from the core up to the chosen team's that carries the measured losses.
		var shown = ShownHealth(out var mean);
		var fighters = new List<TeamCandidate>(size);
		for (var i = 0; i < core; i++)
		{
			fighters.Add(chosen[i]);
		}

		var fighting = chosen.Count;
		for (var n = Math.Max(core, 1); n <= chosen.Count; n++)
		{
			while (fighters.Count < n && fighters.Count - core < reserve.Count)
			{
				fighters.Add(reserve[fighters.Count - core]);
			}

			var carries = CarriesLosses(board, fighters, fighters.Count, shown, mean, out _);
			if (carries == null)
			{
				break;
			}

			if (carries == true)
			{
				fighting = fighters.Count;
				break;
			}
		}

		while (fighters.Count < fighting && fighters.Count - core < reserve.Count)
		{
			fighters.Add(reserve[fighters.Count - core]);
		}

		if (fighters.Count > fighting)
		{
			fighters.RemoveRange(fighting, fighters.Count - fighting);
		}

		foreach (var fighter in fighters)
		{
			if (ContainsPet(lagging, lagging.Count, fighter.Pet) && !ContainsPet(chosen, core, fighter.Pet))
			{
				learning[fighter.Pet] = false;
			}
		}

		// Learners: the other familiars that lag behind, up to the board's size.
		var learners = 0;
		foreach (var candidate in lagging)
		{
			if (fighters.Count >= size)
			{
				break;
			}

			if (!ContainsPet(fighters, fighters.Count, candidate.Pet))
			{
				fighters.Add(candidate);
				learning[candidate.Pet] = true;
				learners++;
			}
		}

		var squad = SquadPoints(fighters.Count);
		note = $"{fighting} fight{(fighting < chosen.Count ? " (they carry the measured losses, one in reserve)" : string.Empty)}; "
			+ $"{lagging.Count} lag behind in rank and level with them, {learners} of them off the horns unless too few others live"
			+ (fighters.Count < size && squad > 0 ? $"; {fighters.Count} familiars bring {squad} points for a small team." : ".");
		return fighters;
	}

	// K7 (A803, A799): a familiar's true rank - the higher of the game's and the last result window's; `known` false when
	// neither gives one, or one that contradicts the game's flag above the sync - then a familiar above the sync stands at
	// the sync, one below it at 0 (no place without a rank).
	private static int TrueRank(TeamCandidate candidate, int sync, out bool known)
	{
		var rank = Math.Max(candidate.Rank, CrucibleBoard.KnownRank(candidate.Pet));
		if (candidate.OverRank && rank <= sync)
		{
			known = false;
			return Math.Max(sync, 0);
		}

		known = rank > 0;
		return rank;
	}

	// K7: the core of a team in its greedy order - up to the last pick that covers a weakness anew, or heals or protects while
	// fewer than the boss asks are in, or interrupts where the board asks for it (as ChooseTeam counts it).
	private static int CoreOf(List<TeamCandidate> picks, Dictionary<uint, int> weight, bool interrupting)
	{
		var covered = new HashSet<uint>();
		var safe = 0;
		var soul = false;
		var core = 0;
		for (var i = 0; i < picks.Count; i++)
		{
			var anew = false;
			foreach (var element in picks[i].Hits)
			{
				anew |= weight.ContainsKey(element) && covered.Add(element);
			}

			var needed = (picks[i].Safe && safe < BossSafeFamiliars)
				|| (interrupting && !soul && KinOf(picks[i].Pet) == BeastmasterKinType.Soulkin);
			safe += picks[i].Safe ? 1 : 0;
			soul |= KinOf(picks[i].Pet) == BeastmasterKinType.Soulkin;
			if (anew || needed)
			{
				core = i + 1;
			}
		}

		return core;
	}

	private static bool ContainsPet(List<TeamCandidate> list, int count, BeastmasterPet pet)
	{
		for (var i = 0; i < count && i < list.Count; i++)
		{
			if (list[i].Pet == pet)
			{
				return true;
			}
		}

		return false;
	}

	// K7: rank and role of each familiar of the last recommendation, for the trace line of a requested team.
	private static readonly Dictionary<BeastmasterPet, string> _teamRanks = [];

	// K7 (A803): where each candidate's rank came from - the game, the last result window, or neither.
	private static readonly Dictionary<BeastmasterPet, string> _rankSources = [];

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
	/// worth one above the board's rank first, then the higher true rank, then one in the team already. The
	/// <paramref name="goal"/> (A804) decides the rest: under <see cref="CrucibleGoal.Level"/> familiars that lag behind
	/// fill the team (A796), under <see cref="CrucibleGoal.Score"/> it shrinks for the score where the measured losses allow
	/// it (A698, A699), under <see cref="CrucibleGoal.SafeWin"/> it stays at the board's size.
	/// </summary>
	internal static unsafe List<CrucibleTeamPick> RecommendCrucibleTeam(uint board, CrucibleGoal goal)
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
		_teamRanks.Clear();
		_rankSources.Clear();
		foreach (var pet in Enum.GetValues<BeastmasterPet>())
		{
			if (pet == BeastmasterPet.None || _teamRefused.Contains(pet) || FamiliarUnlocked(pet) != true || FamiliarOf(pet) is not { } profile)
			{
				continue;
			}

			var safe = profile.TrickHeals || profile.ReleaseHeals || profile.ReleaseProtects || profile.BorrowProtects;
			var overRank = party != null && party->IsPetOverSyncedRank((uint)pet);
			// A803: the higher of the game's rank and the last result window's (A799); the trace names which.
			var game = CrucibleBoard.PetRank(party, pet);
			var learned = CrucibleBoard.KnownRank(pet);
			_rankSources[pet] = game > 0 && game >= learned ? "game" : learned > 0 ? "learned" : "unknown";
			candidates.Add(new TeamCandidate(pet, ElementsOf(profile), safe, overRank, Math.Max(game, learned)));
		}

		var all = new List<TeamCandidate>(candidates);

		// A804: the window says when no rank is known at all - then leveling counts by the board's sync alone, and Safe win
		// cannot tell the weak.
		var anyRank = false;
		foreach (var candidate in candidates)
		{
			anyRank |= candidate.Rank > 0;
		}

		// A804: under Safe win a familiar below the board's recommended rank - or of no known rank - takes a place only where the
		// others do not fill the size, the highest of them first: one there goes on a horn only when fewer others live than
		// horns (K6), so its place is no reserve.
		var safeWin = goal == CrucibleGoal.SafeWin;
		if (safeWin && anyRank)
		{
			var recommended = CrucibleBoard.RecommendedRank(board);
			while (candidates.Count > size)
			{
				var weakest = -1;
				for (var i = 0; i < candidates.Count; i++)
				{
					if (!candidates[i].OverRank && candidates[i].Rank < recommended
						&& (weakest < 0 || candidates[i].Rank < candidates[weakest].Rank))
					{
						weakest = i;
					}
				}

				if (weakest < 0)
				{
					break;
				}

				candidates.RemoveAt(weakest);
			}
		}

		// A805: under Level, while a familiar lags behind, the core too comes from the lowest ranks at least at the board's
		// recommended rank; those below it join as learners (LevelingTeam), not as fighters.
		var lowFirst = false;
		if (goal == CrucibleGoal.Level)
		{
			var maxRank = CrucibleBoard.MaxRank;
			foreach (var candidate in candidates)
			{
				lowFirst |= candidate.Rank is > 0 and var known && known < maxRank;
			}

			if (lowFirst)
			{
				var recommended = CrucibleBoard.RecommendedRank(board);
				var adequate = new List<TeamCandidate>(candidates.Count);
				foreach (var candidate in candidates)
				{
					if (candidate.OverRank || candidate.Rank >= recommended)
					{
						adequate.Add(candidate);
					}
				}

				candidates = adequate;
			}
		}

		var chosen = ChooseTeam(candidates, size, weight, interrupting, team, out var core, lowFirst);

		// A698: as few as the score's bonus for a smaller team allows with the core and the familiars' measured loss - not
		// under the goal Safe win (A804).
		var sizeNote = "Safe win: the full team for the board's fights, the stronger at equal worth; on the board every choice "
			+ "as if there were no score bonus - items, purchases, treasures, rests, feeds, the way - but what the final "
			+ "fight's last GCD buys only points.";
		var keep = safeWin ? size : TeamSizeForScore(board, size, core, chosen, out sizeNote);
		CrucibleTeamSizeNote = sizeNote;
		if (chosen.Count > keep)
		{
			chosen.RemoveRange(keep, chosen.Count - keep);
		}

		// A579, A796 (concept 21, K7 "Leveln"): every familiar in the team earns experience, on a horn or not. The fighters - the
		// core and the reserve the measured losses ask for - take familiars that lag behind first where they replace adequately;
		// the places left up to the board's size go to the other familiars that lag behind, lowest rank first. A804: of every
		// kin - a team of one kin waits until none lags behind (his order of 14.11.2026: all in turns).
		var learning = new Dictionary<BeastmasterPet, bool>();
		var leveled = false;
		if (goal == CrucibleGoal.Level && party != null)
		{
			var leveling = LevelingTeam(board, size, chosen, all, weight, interrupting, team, learning, out var levelNote,
				out var laggers);
			leveled = laggers > 0;
			if (leveled)
			{
				chosen = leveling;
				CrucibleTeamSizeNote = levelNote;
			}
			else
			{
				CrucibleTeamSizeNote = (CrucibleTeamSizeNote.Length > 0 ? CrucibleTeamSizeNote + " " : string.Empty) + levelNote;
			}
		}

		// A699: a team of one kin where one holds what the mixed one holds - its bonus on top; not under Safe win, and under
		// Level only once none lags behind.
		if (!safeWin && !leveled && GambitTeam(all, size, weight, interrupting, team, board, chosen, keep) is { } gambit)
		{
			chosen = gambit.Team;
			CrucibleTeamSizeNote = gambit.Note;
		}

		if (!anyRank && candidates.Count > 0)
		{
			CrucibleTeamSizeNote = (CrucibleTeamSizeNote.Length > 0 ? CrucibleTeamSizeNote + " " : string.Empty)
				+ "The game gave no familiar's rank, and no result window showed one: ranks count by the board's sync only.";
		}

		if (goal == CrucibleGoal.Score)
		{
			// A802: the state shows in the window, so a team without leveling is not read as a fault of the rule.
			CrucibleTeamSizeNote = (CrucibleTeamSizeNote.Length > 0 ? CrucibleTeamSizeNote + " " : string.Empty)
				+ "Score: no familiar joins only to level.";
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
				+ (learning.TryGetValue(pick.Pet, out var learns)
					? learns ? $"; levels, rank {pick.Rank}" : $"; levels in reserve, rank {pick.Rank}"
					: string.Empty);
			picks.Add(new CrucibleTeamPick(pick.Pet, why, team.Contains(pick.Pet)));
			_teamRanks[pick.Pet] = $"{pick.Rank}{(pick.OverRank ? "+" : string.Empty)} {_rankSources.GetValueOrDefault(pick.Pet, "unknown")}"
				+ (learning.ContainsKey(pick.Pet) ? " levels" : string.Empty);
		}

		return picks;
	}

	// K7: the greedy choice over `candidates`, which loses each pick - first as many that heal or protect as B5 wants before
	// a boss, then, where enemies cast interruptibly, a Soulkin, then one by one the familiar that hits the weakness of the
	// most enemies not yet covered (each enemy of a weakness counts one, shared by the familiars already chosen that hit it);
	// at equal worth one above the board's rank first, then below it the higher rank, then one in the team already. `core`
	// counts up to the last pick that covers a weakness anew or is needed as a healer or interrupter (A698).
	private static List<TeamCandidate> ChooseTeam(List<TeamCandidate> candidates, int size, Dictionary<uint, int> weight,
		bool interrupting, List<BeastmasterPet> team, out int core, bool lowFirst = false)
	{
		var chosen = new List<TeamCandidate>();
		var covered = new Dictionary<uint, int>();
		var safeTaken = 0;
		var soulTaken = false;
		core = 0;
		while (chosen.Count < size && candidates.Count > 0)
		{
			var needSafe = safeTaken < BossSafeFamiliars;
			var needSoul = !needSafe && interrupting && !soulTaken && HasSoul();
			var best = -1;
			var bestWorth = -1f;
			var bestCovers = false;
			for (var i = 0; i < candidates.Count; i++)
			{
				var candidate = candidates[i];
				if ((needSafe && !candidate.Safe && HasSafe()) || (needSoul && KinOf(candidate.Pet) != BeastmasterKinType.Soulkin))
				{
					continue;
				}

				var worth = 0f;
				var covers = false;
				foreach (var element in candidate.Hits)
				{
					worth += weight.GetValueOrDefault(element) / (1f + covered.GetValueOrDefault(element));
					covers |= weight.ContainsKey(element) && covered.GetValueOrDefault(element) == 0;
				}

				// A805 (his rule of 14.11.2026): while familiars lag behind, of those that cover a weakness anew the lowest in
				// rank, then the higher worth - one above the sync only where none below carries the weakness, and to fill.
				var better = lowFirst
					? best < 0 || covers != bestCovers ? best < 0 || covers
						: LevelRank(candidate) != LevelRank(candidates[best]) ? LevelRank(candidate) < LevelRank(candidates[best])
						: worth > bestWorth || (worth == bestWorth && Before(candidate, candidates[best]))
					: best < 0 || worth > bestWorth || (worth == bestWorth && Before(candidate, candidates[best]));
				if (better)
				{
					best = i;
					bestWorth = worth;
					bestCovers = covers;
				}
			}

			var pick = candidates[best];
			candidates.RemoveAt(best);
			chosen.Add(pick);
			var anew = false;
			foreach (var element in pick.Hits)
			{
				anew |= weight.ContainsKey(element) && covered.GetValueOrDefault(element) == 0;
				covered[element] = covered.GetValueOrDefault(element) + 1;
			}

			safeTaken += pick.Safe ? 1 : 0;
			soulTaken |= KinOf(pick.Pet) == BeastmasterKinType.Soulkin;

			// A796: the core ends with the last pick that covers a weakness anew (A698). Counting every pick that hits one at
			// all made the core the whole team: no smaller team for points, and no room for familiars that lag behind.
			if (anew || needSafe || needSoul)
			{
				core = chosen.Count;
			}
		}

		return chosen;

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

		// At equal worth: above the board's rank first, then below it the higher rank (A804), then one in the team already.
		// A805: a rank to level by - unknown counts as the highest.
		static int LevelRank(TeamCandidate candidate) => candidate.Rank > 0 ? candidate.Rank : int.MaxValue;

		// Two above the sync fight at the sync - equally strong.
		bool Before(TeamCandidate a, TeamCandidate b)
			=> a.OverRank != b.OverRank ? a.OverRank
				: !a.OverRank && a.Rank != b.Rank ? a.Rank > b.Rank
				: team.Contains(a.Pet) && !team.Contains(b.Pet);
	}

	// K7 (A699): the kin of each "team of only ..." bonus and its XBMScoreBonus row and name, read from their conditions once.
	private static List<(BeastmasterKinType Kin, uint Row, string Name)>? _gambits;
	private static readonly System.Text.RegularExpressions.Regex OnlyKin =
		new(@"team of only (?<kin>\w+)\.", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

	private static List<(BeastmasterKinType Kin, uint Row, string Name)> Gambits()
	{
		if (_gambits != null)
		{
			return _gambits;
		}

		var gambits = new List<(BeastmasterKinType Kin, uint Row, string Name)>();
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMScoreBonus>(Dalamud.Game.ClientLanguage.English);
		if (sheet != null)
		{
			foreach (var row in sheet)
			{
				if (OnlyKin.Match(row.Unknown1.ExtractText()) is { Success: true } only
					&& Enum.TryParse<BeastmasterKinType>(only.Groups["kin"].Value, true, out var kin) && kin != BeastmasterKinType.None)
				{
					gambits.Add((kin, row.RowId, row.Unknown0.ExtractText()));
				}
			}
		}

		return _gambits = gambits;
	}

	// K7 (A698): the points of the "In Unterzahl" step a team of `count` familiars falls in; 0 for none.
	private static int SquadPoints(int count)
	{
		foreach (var (min, max, bonus, _) in Squads())
		{
			if (count >= min && count <= max)
			{
				return CrucibleBoard.BonusPoints(bonus);
			}
		}

		return 0;
	}

	private readonly record struct GambitPick(List<TeamCandidate> Team, BeastmasterKinType Kin, string Note);

	// K7 (A699): the team of one kin that hits every weakness of the board the mixed team hits, holds as many that heal or
	// protect as the mixed one needs before a boss and its interrupter, under the same size rule - the one with the most
	// points beyond the mixed team's; null where none holds that.
	private static GambitPick? GambitTeam(List<TeamCandidate> all, int size, Dictionary<uint, int> weight, bool interrupting,
		List<BeastmasterPet> team, uint board, List<TeamCandidate> mixed, int mixedKeep)
	{
		var needed = new HashSet<uint>();
		var safe = 0;
		var soul = false;
		foreach (var pick in mixed)
		{
			foreach (var element in pick.Hits)
			{
				if (weight.ContainsKey(element))
				{
					_ = needed.Add(element);
				}
			}

			safe += pick.Safe ? 1 : 0;
			soul |= KinOf(pick.Pet) == BeastmasterKinType.Soulkin;
		}

		var safeNeeded = Math.Min(safe, BossSafeFamiliars);
		var bestPoints = mixedKeep < size ? SquadPoints(mixedKeep) : 0;
		GambitPick? best = null;
		foreach (var (kin, row, name) in Gambits())
		{
			var points = CrucibleBoard.BonusPoints(row);
			if (points <= 0 || (soul && kin != BeastmasterKinType.Soulkin))
			{
				continue;
			}

			var pool = new List<TeamCandidate>();
			foreach (var candidate in all)
			{
				if (KinOf(candidate.Pet) == kin)
				{
					pool.Add(candidate);
				}
			}

			var picks = ChooseTeam(pool, size, weight, interrupting, team, out var core);
			var keep = TeamSizeForScore(board, size, core, picks, out var sizeNote);
			if (picks.Count > keep)
			{
				picks.RemoveRange(keep, picks.Count - keep);
			}

			var hits = new HashSet<uint>();
			var kinSafe = 0;
			foreach (var pick in picks)
			{
				hits.UnionWith(pick.Hits);
				kinSafe += pick.Safe ? 1 : 0;
			}

			var total = points + (keep < size ? SquadPoints(keep) : 0);
			if (picks.Count == 0 || !hits.IsSupersetOf(needed) || kinSafe < safeNeeded || total <= bestPoints)
			{
				continue;
			}

			bestPoints = total;
			best = new GambitPick(picks, kin, $"\"{name}\": a team of only {kin}, {points} points - it hits every weakness the mixed "
				+ $"team hits, heals or protects as much and interrupts where that one does. {sizeNote}");
		}

		return best;
	}

	// K7: a familiar the team recommendation weighs - what it hits, whether it heals or protects, its rank on this board.
	private readonly record struct TeamCandidate(BeastmasterPet Pet, HashSet<uint> Hits, bool Safe, bool OverRank, int Rank);

	/// <summary>K7: make the team <paramref name="wanted"/> - leavers first, then joiners, one TogglePet at a time.</summary>
	public static void RequestCrucibleTeam(List<BeastmasterPet> wanted)
	{
		_teamWanted = wanted;
		_teamLastToggle = null;
		CrucibleTeamStatus = "Setting the team...";
		var named = new List<string>(wanted.Count);
		foreach (var pet in wanted)
		{
			named.Add(_teamRanks.TryGetValue(pet, out var rank) ? $"{pet} {rank}" : pet.ToString());
		}

		DefenseTrace.Line($"crucible team: goal {CrucibleGoalNow} - requested {string.Join(", ", named)}");
	}

	// K7: one step of a requested change each time the team has taken the last one, at most one GCD after it; a joiner the
	// game does not take leaves the recommendation for this session, a leaver it does not take ends the change.
	internal static void FollowTeamRequest(CrucibleGoal goal)
	{
		CrucibleTeamRecommendation = CrucibleTeamScreenOpen(out var board) ? RecommendCrucibleTeam(board, goal) : [];
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
