using ECommons.DalamudServices;
using Lumina.Excel;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// K14 of the Crucible (concept 01, "Endkampf vorbereiten", A631): a board's spaces and the ways between them, from the sheets,
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

	// The raw columns as Daedalus c4a788f reads them (CrucibleSheetReader): a space's move, kind and index; a drawn cell's kind,
	// its space and the space a way leads to.
	private const int MoveColumn = 0, KindColumn = 1, IndexColumn = 2;
	private const int CellKindColumn = 2, CellNodeColumn = 3, CellNextColumn = 4;

	/// <summary>A space: its move, kind, battle (XBMBattleDetail row), what a random space can become, and the rest limits of
	/// the campsites it is or can be.</summary>
	internal readonly record struct Node(int Move, Space Kind, uint Battle, bool MayBeCampsite, bool MayBeBattle, List<int> Camps,
		List<(Space Kind, uint Battle)> Options);

	// Camps: how many familiars rest with the player at each campsite of the board (XBMContentCamp, a subrow per campsite
	// index - the index of a campsite space or of a random space's campsite option).
	private sealed record Board(Node[] Nodes, List<int>[]? Next, List<int> Camps);

	private static Dictionary<uint, Board>? _boards;
	private static (uint Board, int Node) _lastBattle = (0, -1);
	private static uint _lastNoted;
	private static int _current = -1;

	/// <summary>The space RSR follows the run on: the last battle, or a campsite, shop or treasure opened after it; -1 unknown.</summary>
	internal static int Current => _current;

	/// <summary>The board and space of the last battle the horn screen named in this run; board 0 before the first.</summary>
	internal static (uint Board, int Node) LastBattle => _lastBattle;

	/// <summary>Forgets the run: on entering or leaving a board.</summary>
	internal static void Reset()
	{
		_lastBattle = (0, -1);
		_lastNoted = 0;
		_current = -1;
	}

	/// <summary>
	/// A window of <paramref name="kind"/> opened (campsite, shop, treasure): the run stands on the next space of that kind - or a
	/// random space that can be it - after the space it stood on. Nothing changes when none is next.
	/// </summary>
	internal static void NoteSpace(Space kind)
	{
		if (_lastBattle.Board == 0 || _current < 0 || !Boards.TryGetValue(_lastBattle.Board, out var route) || route.Next == null
			|| route.Nodes[_current].Kind == kind)
		{
			return;
		}

		foreach (var next in route.Next[_current])
		{
			var node = route.Nodes[next];
			if (node.Kind == kind || (node.Kind == Space.Random && node.Options.Exists(o => o.Kind == kind)))
			{
				_current = next;
				DefenseTrace.Line($"crucible route: on the {kind} at move {node.Move}");
				return;
			}
		}
	}

	/// <summary>The spaces of the board the run is on, and the ways between them; null while none is known.</summary>
	internal static (Node[] Nodes, List<int>[] Next, List<int> Camps)? Map
		=> _lastBattle.Board != 0 && Boards.TryGetValue(_lastBattle.Board, out var route) && route.Next != null
			? (route.Nodes, route.Next, route.Camps)
			: null;

	/// <summary>The horn screen named <paramref name="battleDetail"/> (an XBMBattleDetail row): the space it stands on.</summary>
	internal static void NoteBattle(uint battleDetail)
	{
		if (battleDetail == _lastNoted)
		{
			return;
		}

		_lastNoted = battleDetail;
		foreach (var (board, route) in Boards)
		{
			for (var i = 0; i < route.Nodes.Length; i++)
			{
				if (route.Nodes[i].Battle == battleDetail && (_lastBattle.Board != board || _lastBattle.Node != i))
				{
					_lastBattle = (board, i);
					_current = i;
					DefenseTrace.Line($"crucible route: battle {battleDetail} is board {board}, move {route.Nodes[i].Move} "
						+ $"({route.Nodes[i].Kind}) - {(route.Next == null ? "the board's ways are unread" : "ways read")}");
					return;
				}
			}
		}
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

		route = new Board([], null, []);
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
			foreach (var board in excel.GetSubrowSheet<RawSubrow>(name: "XBMContentStageEventMap"))
			{
				var list = new List<(int, int)>();
				foreach (var row in board)
				{
					// Kind 1 is the space itself, 0 padding; above 1 a way drawn from Node to Next.
					if (row.ReadUInt8Column(CellKindColumn) > (byte)Space.Start)
					{
						list.Add((row.ReadUInt8Column(CellNodeColumn), row.ReadUInt8Column(CellNextColumn)));
					}
				}

				ways[board.RowId] = list;
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
					boards[board.RowId] = new Board([.. nodes], next, restCounts.TryGetValue(board.RowId, out var rest) ? rest : []);
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
