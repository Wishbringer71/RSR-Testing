using ECommons.DalamudServices;

namespace RotationSolver.Basic.Helpers;

internal static partial class TimeToKillTracker
{
	/// <summary>
	/// The largest share of its health the enemy has lost within <paramref name="span"/> seconds: from its last sample at
	/// full health over its recorded history to <paramref name="held"/>, its health now - between any two of these at
	/// most the span plus the widest gap between neighbours apart, so a fall in any window of the span lies within one
	/// pair. Shorter than the span, its whole fall scaled up to the span. NaN without two points (concept 01, A2; A525,
	/// A527).
	/// </summary>
	/// <param name="id">The enemy's object id.</param>
	/// <param name="span">The span in seconds.</param>
	/// <param name="held">Its health share now.</param>
	/// <returns>The largest fall as a share of its health, or NaN.</returns>
	internal static float LargestFall(ulong id, float span, float held)
	{
		if (span <= 0f)
		{
			return float.NaN;
		}

		_points.Clear();
		if (_histories.TryGetValue(id, out var history))
		{
			if (history.FullTick >= 0)
			{
				_points.Add((history.FullTick, 1f));
			}

			foreach (var sample in history.Samples)
			{
				_points.Add(sample);
			}
		}
		else if (_lastFull.TryGetValue(id, out var full))
		{
			_points.Add((full, 1f));
		}

		_points.Add((Environment.TickCount64, held));
		if (_points.Count <= 1)
		{
			return float.NaN;
		}

		var first = _points[0];
		var last = _points[^1];
		long gap = 0;
		for (var i = 1; i < _points.Count; i++)
		{
			gap = Math.Max(gap, _points[i].Tick - _points[i - 1].Tick);
		}

		var seen = (float)TimeSpan.FromMilliseconds(last.Tick - first.Tick).TotalSeconds;
		if (seen <= 0f)
		{
			return float.NaN;
		}

		if (seen < span)
		{
			return Math.Max(0f, first.Ratio - last.Ratio) * span / seen;
		}

		var reach = (long)TimeSpan.FromSeconds(span).TotalMilliseconds + gap;
		var largest = 0f;
		for (var i = 0; i < _points.Count; i++)
		{
			for (var j = i + 1; j < _points.Count && _points[j].Tick - _points[i].Tick <= reach; j++)
			{
				largest = Math.Max(largest, _points[i].Ratio - _points[j].Ratio);
			}
		}

		return largest;
	}
}
