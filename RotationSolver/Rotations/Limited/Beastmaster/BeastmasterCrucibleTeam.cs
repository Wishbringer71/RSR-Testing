using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace RotationSolver.Rotations.Limited.Beastmaster;

/// <summary>
/// K7, "Ziel des Laufs" (concept 21, A804; his proposal of 14.11.2026): what a Crucible run plays for - the team RSR
/// recommends follows it (A806: the goals differ in the team alone - on the board every goal plays safe, then for the
/// highest score). Stored by name in the rotation's settings.
/// </summary>
public enum CrucibleGoal : byte
{
	/// <summary>Every place below the level limit - the board's cap, or the next board's recommended rank where higher (A822) -, the lowest first; above it only while endangered; offered only with a familiar below the limit, else played as <see cref="SafeWin"/> (A821).</summary>
	[Description("Level")]
	Level,

	/// <summary>The full team, the lowest as long as passing is safe; the strongest in place of the weakest while endangered or unmeasured.</summary>
	[Description("Safe win")]
	SafeWin,

	/// <summary>The strongest, as few as are not endangered; a team of one kin where it scores more.</summary>
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

	/// <summary>K7 (A821): which goals hold on the board of the open team screen - Level, Safe win, Score.</summary>
	public static (bool Level, bool SafeWin, bool Score) CrucibleGoalsOffered { get; private set; } = (true, true, true);

	/// <summary>K7 (A821): the goal the recommendation plays on this board - the chosen one, or Safe win (Score) in its stead.</summary>
	public static CrucibleGoal CrucibleGoalPlayed { get; private set; } = CrucibleGoal.Level;

	/// <summary>K7 (A821): why the chosen goal is not offered on this board; empty when it is.</summary>
	public static string CrucibleGoalWhyNot { get; private set; } = string.Empty;

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

	// K7 (A703, A807): the HP the team screen shows for each familiar of the team as it stands, and the HP they show per
	// point of vitality at the rank they fight with on this board - a familiar outside the team counts its vitality there
	// times that (concept 21, "Ziel des Laufs"); 0 per point while the screen shows none with a known rank.
	private static unsafe TeamHealth ShownHealth(List<TeamCandidate> all, int sync)
	{
		var shown = new Dictionary<BeastmasterPet, uint>();
		var party = AgentXBMPetParty.Instance();
		if (party != null)
		{
			var team = party->SelectedPets.AsSpan();
			for (var i = 0; i < team.Length && i < party->PetMaxHealth.Length; i++)
			{
				if ((BeastmasterPet)team[i].PetId != BeastmasterPet.None && party->PetMaxHealth[i] > 0)
				{
					shown[(BeastmasterPet)team[i].PetId] = party->PetMaxHealth[i];
				}
			}
		}

		// A807: the ratio the game shows, not a figure in the code - floor(VIT x 6.7) in all 57 pairs of his uploads. The
		// smallest pair's: a stale rank too low raises a pair's ratio and would give every other familiar too many HP.
		var perVitality = 0d;
		foreach (var candidate in all)
		{
			if (shown.TryGetValue(candidate.Pet, out var hp)
				&& CrucibleBoard.FamiliarVitality(candidate.Pet, CrucibleBoard.FightingRank(candidate.Rank, candidate.OverRank, sync)) is > 0 and var vit
				&& (perVitality <= 0d || (double)hp / vit < perVitality))
			{
				perVitality = (double)hp / vit;
			}
		}

		// Each candidate's HP once for this pass: as shown, else its vitality at its fighting rank times the shown ratio,
		// rounded down (at most a point or two below the game's floor(VIT x 6.7) - the safe side); 0 where neither is known.
		var health = new Dictionary<BeastmasterPet, uint>(all.Count);
		foreach (var candidate in all)
		{
			health[candidate.Pet] = shown.TryGetValue(candidate.Pet, out var hp) ? hp
				: (uint)Math.Floor(perVitality * CrucibleBoard.FamiliarVitality(candidate.Pet,
					CrucibleBoard.FightingRank(candidate.Rank, candidate.OverRank, sync)));
		}

		return new TeamHealth(health, perVitality);
	}

	// K7 (A807): each candidate's HP on this board, and the HP per point of vitality the team screen shows.
	private readonly record struct TeamHealth(Dictionary<BeastmasterPet, uint> Health, double PerVitality)
	{
		public uint Of(TeamCandidate candidate) => Health.GetValueOrDefault(candidate.Pet);
	}

	// K7 (A703, A729): whether the first `n` of `picks` carry every use of a familiar measured on this board - with a team as
	// strong as they are, or weaker - with the one of fewest HP in reserve; null while no run on the board measured them.
	private static bool? CarriesLosses(uint board, List<TeamCandidate> picks, int n, TeamHealth health, out List<uint> uses)
	{
		var sync = CrucibleBoard.SyncRank(board);
		long ranks = 0;
		for (var i = 0; i < n; i++)
		{
			ranks += CrucibleBoard.FightingRank(picks[i].Rank, picks[i].OverRank, sync);
		}

		uses = CrucibleBoard.FamiliarUseLossesOf(board, CrucibleBoard.RankStep(ranks, n));
		if (uses.Count == 0 || health.PerVitality <= 0d || n < 1)
		{
			return null;
		}

		var left = new List<uint>();
		for (var i = 0; i < n; i++)
		{
			left.Add(health.Of(picks[i]));
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
	/// K7 "Leveln" (his rules of 14.11.2026, A806, summing up A579, A796, A805): every place goes to a familiar below the
	/// board's cap (its sync rank) - first those that heal or protect as B5 asks before the boss and the interrupter, then
	/// the lowest in rank. With fewer of them than places, the lowest at or above the cap fill one by one while the team is
	/// endangered: it does not carry the board's measured losses at its rank step with one in reserve, counted on the
	/// familiars the horn choice sends in - at least the recommended rank (K6) - and unmeasured counts as endangered. A full
	/// team below the cap that is measured endangered takes, for its weakest place, the lowest outside that fights
	/// stronger - below the cap first, a role only for the same role (Strengthen). Empty with no familiar behind in rank (<paramref name="laggers"/> 0).
	/// <paramref name="learning"/> receives each familiar that lags behind: true when it fights only when too few others live.
	/// </summary>
	/// <summary>
	/// K7 (A822, his rule of 15.11.2026): the rank Level levels up to on <paramref name="board"/> - its cap, and where a next
	/// board follows, that board's recommended rank when it lies higher: below it a familiar fights there only when too few
	/// others live (K6).
	/// </summary>
	private static int LevelLimit(uint board)
	{
		var sync = CrucibleBoard.SyncRank(board);
		var next = board + 1;
		return CrucibleBoard.SyncRank(next) > 0 ? Math.Max(sync, CrucibleBoard.RecommendedRank(next)) : sync;
	}

	// K7 (A822): the familiars of known rank below the level limit.
	private static HashSet<BeastmasterPet> BelowLimit(List<TeamCandidate> all, int sync, int limit)
	{
		var below = new HashSet<BeastmasterPet>();
		foreach (var candidate in all)
		{
			if (TrueRank(candidate, sync, out var known) is var rank && known && rank < limit)
			{
				_ = below.Add(candidate.Pet);
			}
		}

		return below;
	}

	private static List<TeamCandidate> LevelTeam(uint board, int size, List<TeamCandidate> all, Dictionary<uint, int> weight,
		bool interrupting, List<BeastmasterPet> team, TeamHealth health, Dictionary<BeastmasterPet, bool> learning, out string note,
		out int laggers)
	{
		var maxRank = CrucibleBoard.MaxRank;
		var sync = CrucibleBoard.SyncRank(board);
		var recommended = CrucibleBoard.RecommendedRank(board);
		laggers = 0;
		foreach (var candidate in all)
		{
			var rank = TrueRank(candidate, sync, out var known);
			laggers += rank > 0 && (known ? rank < maxRank : sync < maxRank) ? 1 : 0;
		}

		if (laggers == 0)
		{
			note = "No familiar lags behind in rank: the team for the board's fights and the score.";
			return [];
		}

		// In order: below the level limit - the roles, then the lowest -, then at or above it, the lowest first (A822: the
		// limit is the cap, or the next board's recommended rank where that lies higher).
		var limit = LevelLimit(board);
		var belowLimit = BelowLimit(all, sync, limit);
		var order = ChooseTeam(new List<TeamCandidate>(all), all.Count, weight, interrupting, LastRunTeam(team), out _, sync, recommended,
			belowLimit);
		var chosen = new List<TeamCandidate>(size);
		var above = new List<TeamCandidate>();
		var roles = RoleHolders(order, interrupting);
		foreach (var pick in order)
		{
			if (chosen.Count < size && (BelowCap(pick) || roles.ContainsKey(pick.Pet)))
			{
				chosen.Add(pick);
			}
			else if (!BelowCap(pick))
			{
				above.Add(pick);
			}
		}

		var filled = 0;
		var swapped = 0;
		var next = 0;
		while (chosen.Count < size && next < above.Count && TeamEndangered(board, chosen, recommended, health) != false)
		{
			chosen.Add(above[next++]);
			filled++;
		}

		if (chosen.Count == size)
		{
			// The rest of the order (below the cap, then at or above it, the lowest first) - the lowest that fights stronger.
			var outside = new List<TeamCandidate>();
			foreach (var pick in order)
			{
				if (!chosen.Contains(pick))
				{
					outside.Add(pick);
				}
			}

			swapped = Strengthen(board, chosen, outside, interrupting, sync, recommended, health, false);
		}

		var below = 0;
		foreach (var pick in chosen)
		{
			below += BelowCap(pick) ? 1 : 0;
		}

		var offHorns = 0;
		foreach (var pick in chosen)
		{
			var rank = TrueRank(pick, sync, out var known);
			if (rank > 0 && (known ? rank < maxRank : sync < maxRank))
			{
				var learns = !pick.OverRank && pick.Rank < recommended;
				learning[pick.Pet] = learns;
				offHorns += learns ? 1 : 0;
			}
		}

		var squad = SquadPoints(board, chosen.Count);
		note = $"{below} below {(limit > sync ? $"the next board's recommended rank ({limit})" : $"the board's cap ({sync})")} level"
			+ (filled > 0 ? $"; {filled} at or above it fill places" : string.Empty)
			+ (swapped > 0 ? $"; {swapped} stronger in place of weaker ones" : string.Empty)
			+ (filled + swapped > 0 ? " - against danger: the team below the limit does not carry the losses measured on this board, or none are measured yet" : string.Empty)
			+ $"; {offHorns} below the recommended rank ({recommended}) fight only when too few others live"
			+ (chosen.Count < size && squad > 0 ? $"; {chosen.Count} familiars bring {squad} points for a small team." : ".");
		return chosen;

		bool BelowCap(TeamCandidate candidate) => belowLimit.Contains(candidate.Pet);
	}

	// K7 (A806): the one danger check of every goal - whether the familiars the horn choice sends in (over the sync or at
	// least the recommended rank, K6) do not carry the board's measured losses at their rank step with one in reserve; null
	// while unmeasured.
	private static bool? TeamEndangered(uint board, List<TeamCandidate> picks, int recommended, TeamHealth health)
	{
		var fighters = new List<TeamCandidate>(picks.Count);
		foreach (var pick in picks)
		{
			if (pick.OverRank || pick.Rank >= recommended)
			{
				fighters.Add(pick);
			}
		}

		// With none that fights: endangered where losses are measured, unknown where not.
		var carries = fighters.Count > 0 ? CarriesLosses(board, fighters, fighters.Count, health, out _)
			: CarriesLosses(board, picks, picks.Count, health, out _) == null ? null : false;
		return carries == null ? null : !carries.Value;
	}

	// K7 (A806, his rule of 14.11.2026 for the best bonus): the strongest of `pool` first - the healers and the interrupter
	// before them -, as few as carry the board's measured losses with one in reserve (no smaller one is endangered); the
	// full size while unmeasured (the safe side).
	private static List<TeamCandidate> FewestStrong(uint board, int size, List<TeamCandidate> pool, Dictionary<uint, int> weight,
		bool interrupting, List<BeastmasterPet> team, TeamHealth health, out int keep)
	{
		var sync = CrucibleBoard.SyncRank(board);
		var recommended = CrucibleBoard.RecommendedRank(board);
		var order = ChooseTeam(new List<TeamCandidate>(pool), pool.Count, weight, interrupting, LastRunTeam(team), out var roles, -Math.Max(sync, 1),
			recommended);
		keep = Math.Min(size, order.Count);
		for (var n = Math.Max(Math.Min(roles, keep), 1); n < keep; n++)
		{
			if (TeamEndangered(board, order.GetRange(0, n), recommended, health) == false)
			{
				keep = n;
				break;
			}
		}

		return order.GetRange(0, keep);
	}

	// K7 (A806, A807; his rule "die niedrigsten vorrang, ausser ... gefährdet"): while the team is endangered (with
	// `unknownEndangers`, also while unmeasured), the first of `outside` - in its order, the lowest first - that is
	// stronger takes the team's weakest place, one at a time: the fewest HP at the rank it fights with, at equal the later
	// in the team's order (the one it wanted last); a place that holds a role only goes to one that holds the same. The
	// number of such swaps.
	private static int Strengthen(uint board, List<TeamCandidate> team, List<TeamCandidate> outside, bool interrupting, int sync,
		int recommended, TeamHealth health, bool unknownEndangers)
	{
		var swaps = 0;
		while (TeamEndangered(board, team, recommended, health) is var danger && (danger == true || (unknownEndangers && danger == null)))
		{
			var roles = RoleHolders(team, interrupting);
			var place = -1;
			var stronger = -1;
			var weakest = long.MaxValue;

			// From the end: at equal strength the later place is found first and kept.
			for (var i = team.Count - 1; i >= 0; i--)
			{
				var strength = Strength(team[i]);
				if (strength >= weakest)
				{
					continue;
				}

				var role = roles.GetValueOrDefault(team[i].Pet);
				for (var j = 0; j < outside.Count; j++)
				{
					if (Strength(outside[j]) > strength && Holds(outside[j], role, interrupting))
					{
						place = i;
						stronger = j;
						weakest = strength;
						break;
					}
				}
			}

			if (place < 0)
			{
				break;
			}

			team[place] = outside[stronger];
			outside.RemoveAt(stronger);
			swaps++;
		}

		return swaps;

		// What a familiar adds on the horns (A807): its HP at the rank it fights with - what the danger turns on; at one rank
		// vitality spans 130 to 197 among familiars - or that rank while the screen shows no HP per vitality; none below the
		// recommended rank (K6).
		long Strength(TeamCandidate candidate) => !(candidate.OverRank || candidate.Rank >= recommended) ? 0
			: health.PerVitality > 0d ? health.Of(candidate)
			: CrucibleBoard.FightingRank(candidate.Rank, candidate.OverRank, sync);
	}

	// K7 (A807): the roles a familiar of the team holds - healing or protecting before the boss (B5), interrupting (K4).
	private readonly record struct TeamRole(bool Safe, bool Interrupter);

	// K7 (A807): whether `candidate` can take a place holding `role`.
	private static bool Holds(TeamCandidate candidate, TeamRole role, bool interrupting)
		=> (!role.Safe || candidate.Safe)
		&& (!role.Interrupter || (interrupting && KinOf(candidate.Pet) == BeastmasterKinType.Soulkin));

	// K7 (A806, A807): the roles in a team, counted as the order counts them (ChooseTeam) - the first two that heal or
	// protect, and the first Soulkin where enemies cast interruptibly; a Soulkin that heals holds both, and a Soulkin among
	// the first two that heal is the interrupter. A third healer, or a Soulkin without interruptible enemies, holds none.
	private static Dictionary<BeastmasterPet, TeamRole> RoleHolders(List<TeamCandidate> team, bool interrupting)
	{
		var roles = new Dictionary<BeastmasterPet, TeamRole>();
		var safe = 0;
		var soul = false;
		foreach (var pick in team)
		{
			var kin = KinOf(pick.Pet) == BeastmasterKinType.Soulkin;
			var role = new TeamRole(pick.Safe && safe < BossSafeFamiliars, interrupting && kin && !soul);
			safe += pick.Safe ? 1 : 0;
			soul |= kin;
			if (role.Safe || role.Interrupter)
			{
				roles[pick.Pet] = role;
			}
		}

		return roles;
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

	// K7: rank and role of each familiar of the last recommendation, for the trace line of a requested team.
	private static readonly Dictionary<BeastmasterPet, string> _teamRanks = [];

	// K7 (A806): the team the screen showed when it opened, null while it is closed.
	private static List<BeastmasterPet>? _teamAtOpen;

	// K7 (A806): the last run's team for the turns among equals - the one the screen showed when it opened, else the one now.
	private static List<BeastmasterPet> LastRunTeam(List<BeastmasterPet> now) => _teamAtOpen ?? now;

	// K7 (A803): where each candidate's rank came from - the game, the last result window, or neither.
	private static readonly Dictionary<BeastmasterPet, string> _rankSources = [];

	private static List<BeastmasterPet>? _teamWanted;

	/// <summary>K7 (A821): whether a requested team is still being set, one toggle at a time.</summary>
	public static bool CrucibleTeamRequestRunning => _teamWanted != null;
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

		// A804: the window says when no rank is known at all - then leveling counts by the board's sync alone.
		var anyRank = false;
		foreach (var candidate in candidates)
		{
			anyRank |= candidate.Rank > 0;
		}

		// A806 (his rules of 14.11.2026): the goals differ in the team alone. Level - every place below the board's cap, above
		// it only against danger; Safe win - the lowest as long as passing is safe; Score - the best, as few as pass safely.
		var learning = new Dictionary<BeastmasterPet, bool>();
		var leveled = false;
		List<TeamCandidate> chosen = [];
		var recommended = CrucibleBoard.RecommendedRank(board);
		var sync = CrucibleBoard.SyncRank(board);
		var health = ShownHealth(all, sync);

		// A821 (his rules of 15.11.2026): only the goals that hold on this board are offered. Level while a familiar of known
		// rank stands below the level limit (A822: the cap, or the next board's recommended rank) - else it has no place to fill, and Safe win takes more familiars along (all on
		// the highest rank: Score). Score where one has reached the cap; with none there, only while its team is measured not
		// endangered. A goal chosen and not offered plays as Safe win here.
		var belowCap = false;
		var reachedCap = false;
		var allAtTop = candidates.Count > 0;
		var levelLimit = LevelLimit(board);
		foreach (var candidate in candidates)
		{
			var rank = TrueRank(candidate, sync, out var known);
			belowCap |= known && rank < levelLimit;
			reachedCap |= rank >= sync && (known || candidate.OverRank);
			allAtTop &= known && rank >= CrucibleBoard.MaxRank;
		}

		var scoreOffered = reachedCap
			|| TeamEndangered(board, FewestStrong(board, size, all, weight, interrupting, team, health, out _), recommended, health) == false;
		CrucibleGoalsOffered = (belowCap, true, scoreOffered);
		var chosenGoal = goal;
		goal = goal switch
		{
			CrucibleGoal.Level when !belowCap => allAtTop && scoreOffered ? CrucibleGoal.Score : CrucibleGoal.SafeWin,
			CrucibleGoal.Score when !scoreOffered => CrucibleGoal.SafeWin,
			_ => goal,
		};
		CrucibleGoalPlayed = goal;
		CrucibleGoalWhyNot = chosenGoal == goal ? string.Empty
			: chosenGoal == CrucibleGoal.Level
				? $"Level is not offered: no familiar of known rank stands below {levelLimit}, this board's cap or the next board's recommended rank; plays as {goal}."
				: "Score is not offered: no familiar has reached this board's cap, and its team is not measured to pass safely; plays as Safe win.";

		if (goal == CrucibleGoal.Level && party != null)
		{
			chosen = LevelTeam(board, size, all, weight, interrupting, team, health, learning, out var levelNote, out var laggers);
			leveled = laggers > 0;
			CrucibleTeamSizeNote = levelNote;
		}

		if (goal == CrucibleGoal.SafeWin)
		{
			// The lowest first; while the team is endangered - measured not to carry the board's losses, or unmeasured - the
			// lowest outside that fights stronger takes its weakest place, a role only for the same role (Strengthen).
			var order = ChooseTeam(new List<TeamCandidate>(all), all.Count, weight, interrupting, LastRunTeam(team), out _, int.MaxValue,
				recommended);
			chosen = order.Count > size ? order.GetRange(0, size) : order;
			var outside = order.Count > size ? order.GetRange(size, order.Count - size) : [];
			var swapped = Strengthen(board, chosen, outside, interrupting, sync, recommended, health, true);

			CrucibleTeamSizeNote = "Safe win: the full team, the lowest in rank as long as passing is safe"
				+ (swapped > 0 ? $"; {swapped} stronger in place of low ones - the low team does not carry the losses measured on this board, "
					+ "or none are measured yet." : ".");
		}
		else if (!leveled)
		{
			// Score, and Level once none lags behind: the strongest first, as few as carry the board's measured losses with one
			// in reserve; unmeasured, the full team (the safe side).
			chosen = FewestStrong(board, size, all, weight, interrupting, team, health, out var keep);
			var squad = SquadPoints(board, chosen.Count);
			CrucibleTeamSizeNote = (leveled || goal == CrucibleGoal.Level ? CrucibleTeamSizeNote + " " : string.Empty)
				+ $"Score: the {chosen.Count} strongest"
				+ (chosen.Count < size ? $" carry the losses measured on this board with one in reserve{(squad > 0 ? $" - {squad} points for a small team" : string.Empty)}."
					: " - no smaller team is measured to carry this board.");

			// A699: a team of one kin where one holds what the mixed one holds - its bonus on top.
			if (GambitTeam(all, size, weight, interrupting, team, board, chosen, keep, health) is { } gambit)
			{
				chosen = gambit.Team;
				CrucibleTeamSizeNote = gambit.Note;
			}
		}

		if (!anyRank && candidates.Count > 0)
		{
			CrucibleTeamSizeNote = (CrucibleTeamSizeNote.Length > 0 ? CrucibleTeamSizeNote + " " : string.Empty)
				+ "The game gave no familiar's rank, and no result window showed one: ranks count by the board's sync only.";
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
				+ (learning.ContainsKey(pick.Pet) ? " levels" : string.Empty)
				// A828: why one above the level limit came - a role holder heals or protects.
				+ (pick.Safe ? " protects" : string.Empty);
		}

		return picks;
	}

	// K7: the greedy choice over `candidates`, which loses each pick - first as many that heal or protect as B5 wants before
	// a boss, then, where enemies cast interruptibly, a Soulkin, then one by one the familiar that hits the weakness of the
	// most enemies not yet covered (each enemy of a weakness counts one, shared by the familiars already chosen that hit it);
	// at equal worth one above the board's rank first, then below it the higher rank, then one in the team already. `core`
	// counts up to the last pick that covers a weakness anew or is needed as a healer or interrupter (A698).
	private static List<TeamCandidate> ChooseTeam(List<TeamCandidate> candidates, int size, Dictionary<uint, int> weight,
		bool interrupting, List<BeastmasterPet> team, out int core, int lowFirstFrom = 0, int fightsFrom = 0,
		HashSet<BeastmasterPet>? levelFirst = null)
	{
		var lowFirst = lowFirstFrom > 0;

		// A806: a negative value orders the strongest first, by the rank they fight with on a board of that sync (Score).
		var highFirst = lowFirstFrom < 0;
		var strongSync = -lowFirstFrom;
		var chosen = new List<TeamCandidate>();
		var covered = new Dictionary<uint, int>();
		var safeTaken = 0;
		var soulTaken = false;
		core = 0;
		while (chosen.Count < size && candidates.Count > 0)
		{
			// A805 review: only while one that heals or protects is left - with fewer than the boss asks for, the phase would
			// otherwise last the whole team, and no weakness, interrupter or core would count.
			var needSafe = safeTaken < BossSafeFamiliars && HasSafe();
			var needSoul = !needSafe && interrupting && !soulTaken && HasSoul();
			var best = -1;
			var bestWorth = -1f;
			var bestCovers = false;
			for (var i = 0; i < candidates.Count; i++)
			{
				var candidate = candidates[i];
				if ((needSafe && !candidate.Safe) || (needSoul && KinOf(candidate.Pet) != BeastmasterKinType.Soulkin))
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

				// A806 (his rules of 14.11.2026): for leveling, one below the cap (`lowFirstFrom`) first, then the lowest in
				// rank; a weakness covered anew only at equal rank (not asked while a healer or the interrupter is wanted - any
				// of those is needed), then worth, then one not in the last run's team (in turns). Whether the team carries the
				// board, LevelTeam measures.
				covers &= !needSafe && !needSoul;
				var adequate = Adequate(candidate);

				// A806 (review, high): a role goes first to one the horn choice sends in - over the sync or at least the
				// recommended rank (K6); one below only where none at it carries the role.
				if ((lowFirst || highFirst) && (needSafe || needSoul) && best >= 0 && Fights(candidate) != Fights(candidates[best]))
				{
					if (Fights(candidate))
					{
						best = i;
						bestWorth = worth;
						bestCovers = covers;
					}

					continue;
				}

				var better = highFirst
					? best < 0 || (Strength(candidate) != Strength(candidates[best]) ? Strength(candidate) > Strength(candidates[best])
						: covers != bestCovers ? covers
						: worth != bestWorth ? worth > bestWorth
						: !team.Contains(candidate.Pet) && team.Contains(candidates[best].Pet))
					: !lowFirst
					? best < 0 || worth > bestWorth || (worth == bestWorth && Before(candidate, candidates[best]))
					: best < 0 || (adequate != Adequate(candidates[best]) ? adequate
						: LevelRank(candidate) != LevelRank(candidates[best]) ? LevelRank(candidate) < LevelRank(candidates[best])
						: covers != bestCovers ? covers
						: worth != bestWorth ? worth > bestWorth
						: !team.Contains(candidate.Pet) && team.Contains(candidates[best].Pet));
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
		bool Fights(TeamCandidate candidate) => candidate.OverRank || candidate.Rank >= fightsFrom;

		int Strength(TeamCandidate candidate) => CrucibleBoard.FightingRank(candidate.Rank, candidate.OverRank, strongSync);

		// A805: a rank to level by - unknown counts as the highest.
		static int LevelRank(TeamCandidate candidate) => candidate.Rank > 0 ? candidate.Rank : int.MaxValue;

		// A806: preferred for leveling - a known rank below the cap.
		// A822: Level names them - below its level limit, which may lie above the cap.
		bool Adequate(TeamCandidate candidate) => levelFirst?.Contains(candidate.Pet)
			?? (!candidate.OverRank && candidate.Rank > 0 && candidate.Rank < lowFirstFrom);

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

	// K7 (A698, A807): the points `board` grants for the "In Unterzahl" step a team of `count` familiars falls in; 0 for none.
	private static int SquadPoints(uint board, int count)
	{
		foreach (var (min, max, bonus, _) in Squads())
		{
			if (count >= min && count <= max)
			{
				return CrucibleBoard.BonusPoints(board, (uint)bonus);
			}
		}

		return 0;
	}

	private readonly record struct GambitPick(List<TeamCandidate> Team, BeastmasterKinType Kin, string Note);

	// K7 (A699): the team of one kin that hits every weakness of the board the mixed team hits, holds as many that heal or
	// protect as the mixed one needs before a boss and its interrupter, under the same size rule - the one with the most
	// points beyond the mixed team's; null where none holds that.
	private static GambitPick? GambitTeam(List<TeamCandidate> all, int size, Dictionary<uint, int> weight, bool interrupting,
		List<BeastmasterPet> team, uint board, List<TeamCandidate> mixed, int mixedKeep, TeamHealth health)
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
		var bestPoints = mixedKeep < size ? SquadPoints(board, mixedKeep) : 0;
		GambitPick? best = null;
		foreach (var (kin, row, name) in Gambits())
		{
			var points = CrucibleBoard.BonusPoints(board, row);
			// Its interrupter only where the board asks for one - a Soulkin in the mixed team by chance bars no other kin.
			if (points <= 0 || (soul && interrupting && kin != BeastmasterKinType.Soulkin))
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

			var picks = FewestStrong(board, size, pool, weight, interrupting, team, health, out var keep);

			// A806 (review): smaller than the board's size only where measured not endangered - unmeasured, the full size.
			if (picks.Count < size && TeamEndangered(board, picks, CrucibleBoard.RecommendedRank(board), health) != false)
			{
				continue;
			}

			var sizeNote = keep < size ? $"{keep} familiars carry the losses measured on this board with one in reserve." : string.Empty;

			var hits = new HashSet<uint>();
			var kinSafe = 0;
			foreach (var pick in picks)
			{
				hits.UnionWith(pick.Hits);
				kinSafe += pick.Safe && (pick.OverRank || pick.Rank >= CrucibleBoard.RecommendedRank(board)) ? 1 : 0;
			}

			var total = points + (keep < size ? SquadPoints(board, keep) : 0);
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
	private static bool _teamScreenTraced;

	internal static void FollowTeamRequest(CrucibleGoal goal)
	{
		// A806: the team the screen showed when it opened - the last run's - for the turns among equals; after an Apply the
		// screen shows the requested team, and comparing with it flipped the recommendation on every click (his trace of
		// 14.11.2026, 17:24: Uragnite and Sabotender, then Salamander and Worm, then back).
		var open = CrucibleTeamScreenOpen(out var board);
		if (!open)
		{
			_teamAtOpen = null;
		}
		else if (_teamAtOpen == null)
		{
			_teamAtOpen = CrucibleTeamNow();
		}

		var opened = open && _teamAtOpen != null && !_teamScreenTraced;
		CrucibleTeamRecommendation = open ? RecommendCrucibleTeam(board, goal) : [];
		// A896: what the screen offered when it opened - an empty recommendation hides the window, so the trace tells it from
		// a window the player closed.
		_teamScreenTraced = open;
		if (opened)
		{
			DefenseTrace.Line($"crucible team screen open: board {board}, team size {CrucibleBoard.TeamSize(board)}, "
				+ $"{CrucibleTeamRecommendation.Count} picks, goal {goal}");
		}

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
