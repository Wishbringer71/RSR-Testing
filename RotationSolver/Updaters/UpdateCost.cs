using System.Diagnostics;
using Dalamud.Plugin.Services;

namespace RotationSolver.Updaters;

/// <summary>
/// What one RSR update cycle costs, measured from the start of the framework handler to its end on the same
/// frame, and written to <c>DefenseTrace.log</c> once a fight ends.
/// </summary>
/// <remarks>
/// The open question was whether the getters that walk the party or the hostiles on every read cost
/// enough to be felt (TODO, "Zustandsabfragen, die bei jedem Lesen neu über Gruppe oder Gegner laufen").
/// It was recorded as waiting for an observation in play; the code can measure it itself, so it does
/// (A286, H2). Only fights are counted, because that is where the hot paths run. The share is of the
/// frame time of the frames RSR ran on, so a throttle (<c>MinUpdatingTime</c>) does not dilute it.
/// </remarks>
internal static class UpdateCost
{
	private static long _start;
	private static int _cycles;
	private static double _totalMs;
	private static double _maxMs;
	private static double _frameMs;
	private static bool _wasInCombat;

	// The largest party seen in the fight. Read at the first cycle out of combat, the party was empty after any fight in
	// which an enemy was attacked: the state update before it clears the party with the fight's records
	// (DataCenter.ResetAllRecords) - 210 of 216 fights in the traces of 24.-25.10.2026 showed "party of 0" (A365).
	private static int _party;

	// Which steps of the cycle carry the time (TODO, "erst erheben, welche Teile des Schritts die Zeit tragen", A310):
	// the time since the last step began goes to that step. Per fight the sum and the longest per step, and the
	// steps of the slowest cycle with the garbage collections that ran during it - a collection stops every managed
	// thread, so a pause caused by another plugin's allocations shows up in whichever step was running.
	private static string? _phase;
	private static long _phaseStart;
	private static int _collectionsAtBegin;
	private static readonly Dictionary<string, double> _cycle = [];
	private static readonly Dictionary<string, (double Total, double Max)> _fight = [];
	private static string _slowest = string.Empty;

	/// <summary>Called when a cycle that runs begins.</summary>
	public static void Begin()
	{
		_start = Stopwatch.GetTimestamp();
		_phase = null;
		_cycle.Clear();
		_collectionsAtBegin = GC.CollectionCount(0);
	}

	/// <summary>
	/// A step of the cycle begins; the time since the previous one began is booked to that one. Called by the
	/// handlers and between the larger updaters inside them.
	/// </summary>
	public static void Step(string name)
	{
		if (_start == 0)
		{
			return;
		}

		var now = Stopwatch.GetTimestamp();
		CloseStep(now);
		_phase = name;
		_phaseStart = now;
	}

	private static void CloseStep(long now)
	{
		if (_phase == null)
		{
			return;
		}

		var ms = Stopwatch.GetElapsedTime(_phaseStart, now).TotalMilliseconds;
		_cycle[_phase] = _cycle.TryGetValue(_phase, out var sum) ? sum + ms : ms;
		_phase = null;
	}

	/// <summary>Called when the same cycle ends.</summary>
	public static void End(IFramework framework)
	{
		if (_start == 0)
		{
			return;
		}

		var end = Stopwatch.GetTimestamp();
		CloseStep(end);
		var elapsed = Stopwatch.GetElapsedTime(_start, end).TotalMilliseconds;
		_start = 0;

		var inCombat = DataCenter.InCombat;
		if (inCombat)
		{
			_cycles++;
			_totalMs += elapsed;
			_frameMs += framework.UpdateDelta.TotalMilliseconds;
			_party = Math.Max(_party, DataCenter.PartyMembers.Count);
			foreach (var (name, ms) in _cycle)
			{
				var known = _fight.TryGetValue(name, out var f) ? f : default;
				_fight[name] = (known.Total + ms, Math.Max(known.Max, ms));
			}

			if (elapsed > _maxMs)
			{
				var collections = GC.CollectionCount(0) - _collectionsAtBegin;
				_slowest = $"slowest cycle {elapsed:F1} ms: {Steps(_cycle, s => s, 1)}"
					+ (collections > 0 ? $", {collections} garbage collection(s) during it" : ", no garbage collection during it");
			}

			_maxMs = Math.Max(_maxMs, elapsed);
		}
		else if (_wasInCombat && _cycles > 0)
		{
			// The party size beside it: the cost grows with the figures around, and the AoE list takes new
			// entries only from a party of four or more, which a trace of a fight alone could not show (A299).
			var share = _frameMs > 0 ? _totalMs / _frameMs : 0;
			DefenseTrace.Line($"update cost over the fight: {_cycles} cycles, mean {_totalMs / _cycles:F2} ms, "
				+ $"max {_maxMs:F2} ms, {share:P1} of the frame time, party of {_party}");
			DefenseTrace.Line($"update cost by step, mean/max ms: {Steps(_fight, s => s.Total, _cycles, s => s.Max)}; {_slowest}");
			_cycles = 0;
			_totalMs = _maxMs = _frameMs = 0;
			_party = 0;
			_fight.Clear();
			_slowest = string.Empty;
		}

		_wasInCombat = inCombat;
	}

	// The steps, the most expensive first: the value divided by the count, and the second value beside it when given.
	private static string Steps<T>(Dictionary<string, T> steps, Func<T, double> value, int count, Func<T, double>? second = null)
	{
		List<(string Name, double Value, double Second)> rows = [];
		foreach (var (name, data) in steps)
		{
			rows.Add((name, value(data) / Math.Max(1, count), second?.Invoke(data) ?? 0));
		}

		rows.Sort((a, b) => b.Value.CompareTo(a.Value));
		List<string> parts = [];
		foreach (var row in rows)
		{
			parts.Add(second == null ? $"{row.Name} {row.Value:F1}" : $"{row.Name} {row.Value:F2}/{row.Second:F1}");
		}

		return string.Join(", ", parts);
	}
}
