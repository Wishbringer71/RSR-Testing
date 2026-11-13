using ECommons.DalamudServices;

namespace RotationSolver.Basic.Helpers;

internal static partial class TimeToKillTracker
{
	private static uint _areaTerritory;

	private static long _sampleGap;

	// A527: every fall this sample shows against the earlier ones of the same enemy, and against its last full health.
	private static void LearnFall(History history, long now, float ratio)
	{
		if (history.MaxHp == 0)
		{
			return;
		}

		if (history.FullTick >= 0)
		{
			LearnAreaFall(now - history.FullTick, (1f - ratio) * history.MaxHp);
		}

		foreach ((var tick, var earlier) in history.Samples)
		{
			LearnAreaFall(now - tick, (earlier - ratio) * history.MaxHp);
		}
	}

	private static void LearnDeath(ulong id, long now)
	{
		if (_histories.TryGetValue(id, out var history) && !history.Ended)
		{
			history.Ended = true;
			LearnFall(history, now, 0f);
		}
	}

	private static void LearnAreaFall(long distance, float fall)
	{
		if (fall <= 0f || distance <= 0 || _sampleGap <= 0)
		{
			return;
		}

		// Counted in intervals, not seconds: the samples come a little more than a second apart, and four intervals
		// read in whole seconds would land beside the span of four.
		var index = (int)Math.Round((double)distance / _sampleGap);
		while (_areaFall.Count <= index)
		{
			_areaFall.Add(0f);
		}

		_areaFall[index] = Math.Max(_areaFall[index], fall);
	}

	private static readonly List<(long Tick, float Ratio)> _points = [];
}
