using ECommons.DalamudServices;
using Lumina.Excel;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// K14 of the Crucible (concept 21, "K14 – Endkampf vorbereiten", A631): a board's spaces and the ways between them, from the sheets,
/// and the last battle the player went into - the game keeps no current space anywhere a plugin reads.
/// </summary>
/// <remarks>
/// The sheets carry no column names (EXDSchema has no XBM schemas); they are read raw as Daedalus c4a788f reads them, checked by
/// its author against the client (31.10.2026) and two board layouts of the wiki: XBMContentStageEvent per board a subrow per
/// space with column 0 the move, 1 the kind, 2 an index (a battle's XBMContentBattle subrow, a random space's
/// XBMContentRandomStageEvent subrow); XBMContentStageEventMap the drawn board, rows of kind above 1 a way from one space to
/// the next; XBMRandomStageEvent what a random space can turn out to be (kind, index).
/// </remarks>
internal static class CrucibleRoute
{
	internal enum Space : byte
	{
		None = 0,
		Start = 1,
		Enemy = 2,
		Elite = 3,
		Boss = 4,
		Shop = 5,
		Campsite = 6,
		Treasure = 7,
		Random = 8,
	}

	// The raw columns as Daedalus c4a788f reads them (CrucibleSheetReader): a space's move, kind and index; a drawn cell's grid
	// column (east) and row (south) - Daedalus 2644cbe -, its kind, its space and the space a way leads to.
	private const int MoveColumn = 0, KindColumn = 1, IndexColumn = 2;
	private const int CellXColumn = 0, CellYColumn = 1, CellKindColumn = 2, CellNodeColumn = 3, CellNextColumn = 4;

	// A drawn cell of kind 1 is the space itself; above it a way, 0 padding.
	private const byte SpaceCell = 1;

	/// <summary>A space: its move, kind, battle (XBMBattleDetail row), what a random space can become, and the rest limits of
	/// the campsites it is or can be.</summary>
	internal readonly record struct Node(int Move, Space Kind, uint Battle, bool MayBeCampsite, bool MayBeBattle, List<int> Camps,
		List<(Space Kind, uint Battle)> Options);

	// Camps: how many familiars rest with the player at each campsite of the board (XBMContentCamp, a subrow per campsite
	// index - the index of a campsite space or of a random space's campsite option). Cells: where each space is drawn on the
	// board's grid (column east, row south; -1 where the map draws none).
	private sealed record Board(Node[] Nodes, List<int>[]? Next, List<int> Camps, (int X, int Y)[] Cells);

	private static Dictionary<uint, Board>? _boards;
	private static (uint Board, int Node) _lastBattle = (0, -1);
	private static uint _lastNoted;
	private static int _current = -1;
	private static bool _done;
	private static bool _fighting;

	// A715: what the space the run stands on was noted as - a fight, or the kind of the window that opened on it; a random
	// space carries what it became.
	private static Space _standsAs = Space.None;

	/// <summary>The space RSR follows the run on: the last battle, or a campsite, shop or treasure opened after it; -1 unknown.</summary>
	internal static int Current => _current;

	/// <summary>
	/// Whether the space RSR follows the run on is done: a campsite, shop or treasure as soon as its window opened, a battle once
	/// a fight ran and ended after its horn screen named it (concept 21, K10 "Weg", "Wann"; his rule of 07.11.2026).
	/// </summary>
	internal static bool CurrentDone => _done;

	/// <summary>Every frame on a board: a fight that ends after the horn screen named the battle finishes its space.</summary>
	internal static void NoteCombat(bool inCombat)
	{
		if (inCombat)
		{
			_fighting = true;
		}
		else if (_fighting)
		{
			_fighting = false;
			_done = true;
		}
	}

	/// <summary>The board and space of the last battle the horn screen named in this run; board 0 before the first.</summary>
	internal static (uint Board, int Node) LastBattle => _lastBattle;

	/// <summary>Forgets the run: on entering or leaving a board.</summary>
	internal static void Reset()
	{
		_lastBattle = (0, -1);
		_lastNoted = 0;
		_current = -1;
		_done = false;
		_fighting = false;
		_standsAs = Space.None;
		_severalTraced = (-1, Space.None);
	}

	/// <summary>
	/// A window of <paramref name="kind"/> shows (campsite, shop, treasure; <paramref name="opened"/>: it was not there
	/// the frame before): the run stands on the space ahead that is of that kind or, a random space, can become it - the
	/// first such space on the ways on, over spaces whose window RSR did not see, never over a fixed fight, which the horn
	/// screen would have named. Standing on a space of that kind already, only a window that opened anew moves it, and only
	/// to the very next space. Where the ways on hold several such spaces - a fork whose ways both begin with that kind -
	/// the window does not tell which: the run stays where it stands, both ways ahead, until the next fight tells (A715).
	/// Nothing changes when there is no such space.
	/// </summary>
	internal static void NoteSpace(Space kind, bool opened)
	{
		if (_lastBattle.Board == 0 || _current < 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| _current >= route.Nodes.Length)
		{
			return;
		}

		List<int> found;
		if (_standsAs != kind)
		{
			found = FirstAhead(route, _current, node => CanBe(node, kind));
		}
		else if (opened)
		{
			found = [];
			foreach (var successor in route.Next[_current])
			{
				if (CanBe(route.Nodes[successor], kind) && !found.Contains(successor))
				{
					found.Add(successor);
				}
			}
		}
		else
		{
			return;
		}

		if (found.Count == 0)
		{
			return;
		}

		if (found.Count > 1)
		{
			// Not on a space of a known kind any more: the next window is looked for on every way on.
			_standsAs = Space.None;
			var fits = (_current, kind);
			if (_severalTraced != fits)
			{
				_severalTraced = fits;
				DefenseTrace.Line($"crucible route: the {kind} fits {found.Count} spaces after move {route.Nodes[_current].Move} - "
					+ "the run stays there, every way ahead counted, until the next fight tells");
			}

			return;
		}

		var next = found[0];
		_current = next;
		_standsAs = kind;
		_done = true;
		DefenseTrace.Line($"crucible route: on the {kind} at move {route.Nodes[next].Move}"
			+ (route.Nodes[next].Kind == Space.Random ? " (a random space)" : string.Empty));
	}

	private static (int Node, Space Kind) _severalTraced = (-1, Space.None);

	private static bool CanBe(Node node, Space kind) => node.Kind == kind || (node.Kind == Space.Random && node.Options.Exists(o => o.Kind == kind));

	// A715: whether a space holds this battle - its own, or one a random space can become (the sheets list those only as
	// the random space's option).
	private static bool HoldsBattle(Node node, uint battle)
		=> node.Battle == battle || (node.Kind == Space.Random && node.Options.Exists(o => o.Battle == battle));

	// A715: on every way on from `from`, the first space that `wanted` holds for; empty for none. A fixed fight ends a
	// way: the run cannot have passed it unnoticed.
	private static List<int> FirstAhead(Board route, int from, Func<Node, bool> wanted)
	{
		var found = new List<int>();
		var seen = new HashSet<int>();
		var queue = new Queue<int>(route.Next![from]);
		while (queue.Count > 0)
		{
			var index = queue.Dequeue();
			if (index < 0 || index >= route.Nodes.Length || !seen.Add(index))
			{
				continue;
			}

			var node = route.Nodes[index];
			if (wanted(node))
			{
				found.Add(index);
				continue;
			}

			if (node.Kind is Space.Enemy or Space.Elite or Space.Boss)
			{
				continue;
			}

			foreach (var next in route.Next[index])
			{
				queue.Enqueue(next);
			}
		}

		return found;
	}

	/// <summary>The spaces of the board the run is on, the ways between them and where each is drawn; null while none is known.</summary>
	internal static (Node[] Nodes, List<int>[] Next, List<int> Camps, (int X, int Y)[] Cells)? Map
		=> _lastBattle.Board != 0 && Boards.TryGetValue(_lastBattle.Board, out var route) && route.Next != null
			? (route.Nodes, route.Next, route.Camps, route.Cells)
			: null;

	/// <summary>
	/// The fights on the shortest way from where the run stands to the final fight, the final fight included (concept 21, K13,
	/// A656): normal ones (an enemy, a random space that can be a battle) and heavy ones (elite, boss). The space the run stands
	/// on counts while it is not done. Null while the board or its ways are unknown.
	/// </summary>
	internal static (int Normal, int Heavy)? FightsAhead()
	{
		if (_lastBattle.Board == 0 || _current < 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| _current >= route.Nodes.Length)
		{
			return null;
		}

		var memo = new Dictionary<int, (int Normal, int Heavy)>();
		if (!_done)
		{
			return From(_current);
		}

		(int Normal, int Heavy)? best = null;
		foreach (var next in route.Next[_current])
		{
			var ahead = From(next);
			if (best is not { } b || ahead.Normal + ahead.Heavy < b.Normal + b.Heavy)
			{
				best = ahead;
			}
		}

		return best ?? (0, 0);

		(int Normal, int Heavy) From(int index)
		{
			if (memo.TryGetValue(index, out var known))
			{
				return known;
			}

			var node = route.Nodes[index];
			(int Normal, int Heavy) own = node.Kind switch
			{
				Space.Elite or Space.Boss => (0, 1),
				Space.Enemy => (1, 0),
				_ => node.MayBeBattle ? (1, 0) : (0, 0),
			};

			memo[index] = own;
			if (node.Kind == Space.Boss || route.Next[index].Count == 0)
			{
				return own;
			}

			(int Normal, int Heavy)? rest = null;
			foreach (var next in route.Next[index])
			{
				var ahead = From(next);
				if (rest is not { } r || ahead.Normal + ahead.Heavy < r.Normal + r.Heavy)
				{
					rest = ahead;
				}
			}

			var total = rest is { } found ? (own.Normal + found.Normal, own.Heavy + found.Heavy) : own;
			memo[index] = total;
			return total;
		}
	}

	/// <summary>
	/// K13 (A671): the battles of the enemy spaces - the ones a player may flee - on the shortest way from where the run stands
	/// to the final fight, as <see cref="FightsAhead"/> takes it; the space it stands on counts while it is not done. A random
	/// space does not count: what it becomes is chance. Null while the board or its ways are unknown.
	/// </summary>
	internal static List<uint>? EnemyBattlesAhead()
	{
		if (_lastBattle.Board == 0 || _current < 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| _current >= route.Nodes.Length)
		{
			return null;
		}

		var memo = new Dictionary<int, (int Fights, List<uint> Enemies)>();
		if (!_done)
		{
			return From(_current).Enemies;
		}

		(int Fights, List<uint> Enemies)? best = null;
		foreach (var next in route.Next[_current])
		{
			var ahead = From(next);
			if (best is not { } b || ahead.Fights < b.Fights)
			{
				best = ahead;
			}
		}

		return best?.Enemies ?? [];

		// The fights on the shortest way on from a space, and its enemy battles.
		(int Fights, List<uint> Enemies) From(int index)
		{
			if (memo.TryGetValue(index, out var known))
			{
				return known;
			}

			var node = route.Nodes[index];
			var own = node.Kind is Space.Enemy or Space.Elite or Space.Boss || node.MayBeBattle ? 1 : 0;
			List<uint> enemies = node.Kind == Space.Enemy && node.Battle != 0 ? [node.Battle] : [];
			memo[index] = (own, enemies);
			if (node.Kind == Space.Boss || route.Next[index].Count == 0)
			{
				return (own, enemies);
			}

			(int Fights, List<uint> Enemies)? rest = null;
			foreach (var next in route.Next[index])
			{
				var ahead = From(next);
				if (rest is not { } r || ahead.Fights < r.Fights)
				{
					rest = ahead;
				}
			}

			var total = rest is { } found ? (own + found.Fights, (List<uint>)[.. enemies, .. found.Enemies]) : (own, enemies);
			memo[index] = total;
			return total;
		}
	}

	/// <summary>
	/// K13 (A669): the fewest campsites on any way from where the run stands to the final fight - the campsites every way
	/// passes; the space it stands on counts while it is not done, a random space that can become one does not. Null while
	/// the board or its ways are unknown.
	/// </summary>
	internal static int? CampsitesAhead()
	{
		if (_lastBattle.Board == 0 || _current < 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| _current >= route.Nodes.Length)
		{
			return null;
		}

		var memo = new Dictionary<int, int>();
		if (!_done)
		{
			return From(_current);
		}

		int? fewest = null;
		foreach (var next in route.Next[_current])
		{
			var ahead = From(next);
			if (fewest is not { } f || ahead < f)
			{
				fewest = ahead;
			}
		}

		return fewest ?? 0;

		int From(int index)
		{
			if (memo.TryGetValue(index, out var known))
			{
				return known;
			}

			var own = route.Nodes[index].Kind == Space.Campsite ? 1 : 0;
			memo[index] = own;
			if (route.Nodes[index].Kind == Space.Boss || route.Next[index].Count == 0)
			{
				return own;
			}

			int? rest = null;
			foreach (var next in route.Next[index])
			{
				var ahead = From(next);
				if (rest is not { } r || ahead < r)
				{
					rest = ahead;
				}
			}

			var total = own + (rest ?? 0);
			memo[index] = total;
			return total;
		}
	}

	/// <summary>
	/// K10 Campsite and shop (concept 21, A661): the load of the heaviest way from where the run stands - each battle with
	/// <paramref name="load"/> of its XBMBattleDetail row, a random space with its heaviest battle - to the next campsite
	/// (<paramref name="toCampsite"/>; a way without one runs to the final fight, included) or to the final fight. The space
	/// the run stands on counts while it is not done. Null while the board or its ways are unknown, or a battle on any way
	/// has no load.
	/// </summary>
	internal static float? WorstAhead(bool toCampsite, Func<uint, float?> load)
		=> WorstNeedAhead(toCampsite, AsNeed(load));

	/// <summary>
	/// As <see cref="WorstAhead(bool, Func{uint, float?})"/>, from space <paramref name="from"/> of the board the run is on,
	/// counting that space itself unless <paramref name="fromDone"/> - for the way window's look ahead.
	/// </summary>
	internal static float? WorstAhead(int from, bool fromDone, bool toCampsite, Func<uint, float?> load)
		=> WorstNeedAhead(from, fromDone, toCampsite, AsNeed(load));

	// A load that only adds up: what a battle takes for good is also the most it takes at any moment.
	private static Func<uint, (float Through, float Peak)?> AsNeed(Func<uint, float?> load)
		=> battle => load(battle) is { } all ? (all, all) : null;

	/// <summary>
	/// K15 (concept 21, A709): what the heaviest way from where the run stands demands at its worst moment - each battle
	/// with what it takes for good (<c>Through</c>) and the most it takes at any moment (<c>Peak</c>): a battle's peak, or
	/// what it takes for good plus the demand of the way after it, whichever is more. Ways, random spaces and unknowns as
	/// in <see cref="WorstAhead(bool, Func{uint, float?})"/>.
	/// </summary>
	internal static float? WorstNeedAhead(bool toCampsite, Func<uint, (float Through, float Peak)?> load)
	{
		if (_lastBattle.Board == 0 || _current < 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| _current >= route.Nodes.Length)
		{
			return null;
		}

		return WorstNeedAhead(_current, _done, toCampsite, load);
	}

	private static float? WorstNeedAhead(int from, bool fromDone, bool toCampsite, Func<uint, (float Through, float Peak)?> load)
	{
		if (_lastBattle.Board == 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| from < 0 || from >= route.Nodes.Length)
		{
			return null;
		}

		var memo = new Dictionary<int, float?>();
		if (!fromDone)
		{
			return From(from, true);
		}

		return Rest(from);

		// The heaviest of the ways on from a space, the space itself not counted; 0 at a dead end.
		float? Rest(int index)
		{
			float? worst = 0f;
			foreach (var next in route.Next[index])
			{
				if (From(next, false) is not { } ahead)
				{
					return null;
				}

				worst = Math.Max(worst.Value, ahead);
			}

			return worst;
		}

		float? From(int index, bool first)
		{
			if (memo.TryGetValue(index, out var known))
			{
				return known;
			}

			var node = route.Nodes[index];
			if (toCampsite && !first && node.Kind == Space.Campsite)
			{
				memo[index] = 0f;
				return 0f;
			}

			(float Through, float Peak)? own = (0f, 0f);
			switch (node.Kind)
			{
				case Space.Enemy or Space.Elite or Space.Boss:
					own = node.Battle != 0 ? load(node.Battle) : null;
					break;
				case Space.Random:
					foreach (var (_, battle) in node.Options)
					{
						if (battle != 0)
						{
							own = load(battle) is { } option && own is { } sofar
								? (Math.Max(sofar.Through, option.Through), Math.Max(sofar.Peak, option.Peak))
								: null;
						}
					}

					break;
			}

			var total = own is { } here && (node.Kind == Space.Boss || route.Next[index].Count == 0 ? 0f : Rest(index)) is { } after
				? Math.Max(here.Peak, here.Through + after)
				: (float?)null;
			memo[index] = total;
			return total;
		}
	}

	/// <summary>
	/// The horn screen named <paramref name="battleDetail"/> (an XBMBattleDetail row): the space it stands on - one with
	/// that battle, or a random space that can become it (A715); the first such space on the ways on from where the run
	/// stands, else the first on any board.
	/// </summary>
	internal static void NoteBattle(uint battleDetail)
	{
		if (battleDetail == _lastNoted)
		{
			return;
		}

		_lastNoted = battleDetail;
		if (_lastBattle.Board != 0 && _current >= 0 && Boards.TryGetValue(_lastBattle.Board, out var here) && here.Next != null
			&& _current < here.Nodes.Length && FirstAhead(here, _current, node => HoldsBattle(node, battleDetail)) is { Count: > 0 } ahead)
		{
			Stand(_lastBattle.Board, here, ahead[0]);
			return;
		}

		foreach (var (board, route) in Boards)
		{
			for (var i = 0; i < route.Nodes.Length; i++)
			{
				if (HoldsBattle(route.Nodes[i], battleDetail) && (_lastBattle.Board != board || _lastBattle.Node != i))
				{
					Stand(board, route, i);
					return;
				}
			}
		}

		void Stand(uint board, Board route, int node)
		{
			_lastBattle = (board, node);
			_current = node;
			_standsAs = Space.Enemy;
			_done = false;
			_fighting = false;
			DefenseTrace.Line($"crucible route: battle {battleDetail} is board {board}, move {route.Nodes[node].Move} "
				+ $"({route.Nodes[node].Kind}) - {(route.Next == null ? "the board's ways are unread" : "ways read")}");
		}
	}

	/// <summary>
	/// The battles (XBMBattleDetail rows) still reachable on the board's ways after the last battle, what a random space can
	/// become included, the boss's too (concept 21, K11 stage 3, A643). Null while no battle is known or the ways were not read.
	/// </summary>
	internal static HashSet<uint>? BattlesAhead()
	{
		if (!TryStart(out var route, out var start) || route.Next == null)
		{
			return null;
		}

		var battles = new HashSet<uint>();
		var seen = new HashSet<int>();
		var queue = new Queue<int>(route.Next[start]);
		while (queue.Count > 0)
		{
			var index = queue.Dequeue();
			if (index < 0 || index >= route.Nodes.Length || !seen.Add(index))
			{
				continue;
			}

			var node = route.Nodes[index];
			if (node.Battle != 0)
			{
				_ = battles.Add(node.Battle);
			}

			foreach (var (_, battle) in node.Options)
			{
				if (battle != 0)
				{
					_ = battles.Add(battle);
				}
			}

			foreach (var next in route.Next[index])
			{
				queue.Enqueue(next);
			}
		}

		return battles;
	}

	/// <summary>
	/// Whether a campsite - or a random space that can become one - can still come after another battle, from the last battle
	/// on. False: the campsite after this battle is the last before the boss. Null while no battle is known or the board's ways
	/// were not read.
	/// </summary>
	internal static bool? CampsiteAfterAnotherBattle()
	{
		if (!TryStart(out var route, out var start) || route.Next == null)
		{
			return null;
		}

		var seen = new HashSet<(int, bool)>();
		var queue = new Queue<(int Node, bool Passed)>();
		foreach (var next in route.Next[start])
		{
			queue.Enqueue((next, false));
		}

		while (queue.Count > 0)
		{
			var (index, passed) = queue.Dequeue();
			if (index < 0 || index >= route.Nodes.Length || !seen.Add((index, passed)))
			{
				continue;
			}

			var node = route.Nodes[index];
			if (passed && node.MayBeCampsite)
			{
				return true;
			}

			if (node.Kind == Space.Boss)
			{
				continue;
			}

			var after = passed || node.MayBeBattle;
			foreach (var next in route.Next[index])
			{
				queue.Enqueue((next, after));
			}
		}

		return false;
	}

	/// <summary>
	/// Whether a campsite - or a random space that can become one - can come after a shop of the stretch from the last battle
	/// on, before the next battle: what is fed at that shop may be lost resting there. Null while no battle is known or the
	/// board's ways were not read.
	/// </summary>
	internal static bool? CampsiteAfterShop()
	{
		if (!TryStart(out var route, out var start) || route.Next == null)
		{
			return null;
		}

		// The shops of this stretch: reached from the last battle without passing another.
		var shops = new List<int>();
		var seen = new HashSet<int>();
		var queue = new Queue<int>(route.Next[start]);
		while (queue.Count > 0)
		{
			var index = queue.Dequeue();
			if (index < 0 || index >= route.Nodes.Length || !seen.Add(index))
			{
				continue;
			}

			var node = route.Nodes[index];
			if (node.Kind == Space.Shop)
			{
				shops.Add(index);
			}

			if (node.MayBeBattle || node.Kind == Space.Boss)
			{
				continue;
			}

			foreach (var next in route.Next[index])
			{
				queue.Enqueue(next);
			}
		}

		foreach (var shop in shops)
		{
			if (CampsiteReachable(route, shop))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// How many familiars may rest at the campsite of this stretch: the fewest any campsite reached from the last battle without
	/// passing another allows (XBMContentCamp). Null while no battle is known or the board's ways were not read.
	/// </summary>
	internal static int? CampsiteLimit()
	{
		if (!TryStart(out var route, out var start) || route.Next == null)
		{
			return null;
		}

		int? fewest = null;
		var seen = new HashSet<int>();
		var queue = new Queue<int>(route.Next[start]);
		while (queue.Count > 0)
		{
			var index = queue.Dequeue();
			if (index < 0 || index >= route.Nodes.Length || !seen.Add(index))
			{
				continue;
			}

			var node = route.Nodes[index];
			foreach (var camp in node.Camps)
			{
				if (camp >= 0 && camp < route.Camps.Count)
				{
					fewest = Math.Min(fewest ?? int.MaxValue, route.Camps[camp]);
				}
			}

			if (node.MayBeBattle || node.Kind == Space.Boss)
			{
				continue;
			}

			foreach (var next in route.Next[index])
			{
				queue.Enqueue(next);
			}
		}

		return fewest;
	}

	/// <summary>
	/// K14 (A662): whether a campsite - or a random space that can become one - can still come before the final fight from
	/// where the run stands; the space it stands on counts while it is not done. True while the board or its ways are unknown,
	/// as nothing then says the HP stay as they are.
	/// </summary>
	internal static bool CampsiteAhead()
	{
		if (_lastBattle.Board == 0 || _current < 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| _current >= route.Nodes.Length)
		{
			return true;
		}

		return (!_done && route.Nodes[_current].MayBeCampsite) || CampsiteReachable(route, _current);
	}

	private static bool CampsiteReachable(Board route, int from)
	{
		var seen = new HashSet<int>();
		var queue = new Queue<int>(route.Next![from]);
		while (queue.Count > 0)
		{
			var index = queue.Dequeue();
			if (index < 0 || index >= route.Nodes.Length || !seen.Add(index))
			{
				continue;
			}

			if (route.Nodes[index].MayBeCampsite)
			{
				return true;
			}

			foreach (var next in route.Next[index])
			{
				queue.Enqueue(next);
			}
		}

		return false;
	}

	private static bool TryStart(out Board route, out int start)
	{
		start = _lastBattle.Node;
		if (_lastBattle.Board != 0 && Boards.TryGetValue(_lastBattle.Board, out var known) && start >= 0 && start < known.Nodes.Length)
		{
			route = known;
			return true;
		}

		route = new Board([], null, [], []);
		return false;
	}

	private static Dictionary<uint, Board> Boards => _boards ??= Read();

	private static Dictionary<uint, Board> Read()
	{
		var boards = new Dictionary<uint, Board>();
		try
		{
			var excel = Svc.Data.Excel;
			var battles = new Dictionary<uint, List<uint>>();
			foreach (var board in excel.GetSubrowSheet<RawSubrow>(name: "XBMContentBattle"))
			{
				var list = new List<uint>();
				foreach (var row in board)
				{
					list.Add(row.ReadUInt16Column(0));
				}

				battles[board.RowId] = list;
			}

			var options = new Dictionary<uint, List<(Space Kind, int Index)>>();
			foreach (var pool in excel.GetSubrowSheet<RawSubrow>(name: "XBMRandomStageEvent"))
			{
				var list = new List<(Space, int)>();
				foreach (var row in pool)
				{
					list.Add(((Space)row.ReadUInt8Column(0), row.ReadInt8Column(1)));
				}

				options[pool.RowId] = list;
			}

			var randoms = new Dictionary<uint, List<uint>>();
			foreach (var board in excel.GetSubrowSheet<RawSubrow>(name: "XBMContentRandomStageEvent"))
			{
				var list = new List<uint>();
				foreach (var row in board)
				{
					list.Add(row.ReadUInt16Column(0));
				}

				randoms[board.RowId] = list;
			}

			var ways = new Dictionary<uint, List<(int From, int To)>>();
			var drawnAt = new Dictionary<uint, Dictionary<int, (int X, int Y)>>();
			foreach (var board in excel.GetSubrowSheet<RawSubrow>(name: "XBMContentStageEventMap"))
			{
				var list = new List<(int, int)>();
				var cells = new Dictionary<int, (int X, int Y)>();
				foreach (var row in board)
				{
					var cellKind = row.ReadUInt8Column(CellKindColumn);
					if (cellKind > SpaceCell)
					{
						list.Add((row.ReadUInt8Column(CellNodeColumn), row.ReadUInt8Column(CellNextColumn)));
					}
					else if (cellKind == SpaceCell)
					{
						cells[row.ReadUInt8Column(CellNodeColumn)] = (row.ReadUInt8Column(CellXColumn), row.ReadUInt8Column(CellYColumn));
					}
				}

				ways[board.RowId] = list;
				drawnAt[board.RowId] = cells;
			}

			var restCounts = new Dictionary<uint, List<int>>();
			foreach (var board in excel.GetSubrowSheet<RawSubrow>(name: "XBMContentCamp"))
			{
				var list = new List<int>();
				foreach (var row in board)
				{
					list.Add(row.ReadUInt8Column(0));
				}

				restCounts[board.RowId] = list;
			}

			foreach (var board in excel.GetSubrowSheet<RawSubrow>(name: "XBMContentStageEvent"))
			{
				var ofBoard = battles.TryGetValue(board.RowId, out var b) ? b : [];
				var pools = randoms.TryGetValue(board.RowId, out var r) ? r : [];
				var nodes = new List<Node>();
				foreach (var row in board)
				{
					var kind = (Space)row.ReadUInt8Column(KindColumn);
					var index = row.ReadInt8Column(IndexColumn);
					var battle = kind is Space.Enemy or Space.Elite or Space.Boss && index >= 0 && index < ofBoard.Count ? ofBoard[index] : 0u;
					bool camp = kind == Space.Campsite, fight = kind is Space.Enemy or Space.Elite;
					var camps = new List<int>();
					var becomes = new List<(Space, uint)>();
					if (camp)
					{
						camps.Add(index);
					}

					if (kind == Space.Random && index >= 0 && index < pools.Count && options.TryGetValue(pools[index], out var can))
					{
						foreach (var (option, optionIndex) in can)
						{
							becomes.Add((option, option is Space.Enemy or Space.Elite && optionIndex >= 0 && optionIndex < ofBoard.Count
								? ofBoard[optionIndex] : 0u));
							if (option == Space.Campsite)
							{
								camp = true;
								camps.Add(optionIndex);
							}

							fight |= option is Space.Enemy or Space.Elite;
						}
					}

					nodes.Add(new Node(row.ReadInt8Column(MoveColumn), kind, battle, camp, fight, camps, becomes));
				}

				List<int>[]? next = null;
				if (ways.TryGetValue(board.RowId, out var drawn) && drawn.Count > 0)
				{
					next = new List<int>[nodes.Count];
					for (var i = 0; i < next.Length; i++)
					{
						next[i] = [];
					}

					foreach (var (from, to) in drawn)
					{
						if (from < next.Length && to < next.Length && !next[from].Contains(to))
						{
							next[from].Add(to);
						}
					}
				}

				if (nodes.Count > 0)
				{
					var cells = new (int X, int Y)[nodes.Count];
					var placed = drawnAt.TryGetValue(board.RowId, out var at) ? at : [];
					for (var i = 0; i < cells.Length; i++)
					{
						cells[i] = placed.TryGetValue(i, out var cell) ? cell : (-1, -1);
					}

					boards[board.RowId] = new Board([.. nodes], next, restCounts.TryGetValue(board.RowId, out var rest) ? rest : [], cells);
				}
			}
		}
		catch (Exception ex)
		{
			DefenseTrace.Line($"crucible route: the board sheets were not read - {ex.Message}");
		}

		return boards;
	}
}
