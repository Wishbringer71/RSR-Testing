using System.Diagnostics;
using Dalamud.Plugin.Services;

namespace RotationSolver.Updaters;

/// <summary>
/// What one RSR update cycle costs, measured from the first framework handler to the last on the same
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

	/// <summary>Called by the first handler of a cycle that runs.</summary>
	public static void Begin()
	{
		_start = Stopwatch.GetTimestamp();
	}

	/// <summary>Called by the last handler of the same cycle.</summary>
	public static void End(IFramework framework)
	{
		if (_start == 0)
		{
			return;
		}

		var elapsed = Stopwatch.GetElapsedTime(_start).TotalMilliseconds;
		_start = 0;

		var inCombat = DataCenter.InCombat;
		if (inCombat)
		{
			_cycles++;
			_totalMs += elapsed;
			_maxMs = Math.Max(_maxMs, elapsed);
			_frameMs += framework.UpdateDelta.TotalMilliseconds;
		}
		else if (_wasInCombat && _cycles > 0)
		{
			var share = _frameMs > 0 ? _totalMs / _frameMs : 0;
			DefenseTrace.Line($"update cost over the fight: {_cycles} cycles, mean {_totalMs / _cycles:F2} ms, "
				+ $"max {_maxMs:F2} ms, {share:P1} of the frame time");
			_cycles = 0;
			_totalMs = _maxMs = _frameMs = 0;
		}

		_wasInCombat = inCombat;
	}
}
