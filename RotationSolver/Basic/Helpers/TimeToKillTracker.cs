namespace RotationSolver.Basic.Helpers;

internal static class TimeToKillTracker
{
	private const long WindowMs = 30_000;
	private const int MinSamples = 3;
	private const float HpIncreaseTolerance = 0.01f;

	private sealed class History
	{
		public readonly Queue<(long Tick, float Ratio)> Samples = new();
		public long LastTick;
		public float LastRatio;
		public float Rate = float.NaN;
	}

	private static readonly Dictionary<ulong, History> _histories = [];

	internal static void Record(List<IBattleChara> hostiles, long now)
	{
		for (var i = 0; i < hostiles.Count; i++)
		{
			var target = hostiles[i];
			if (target == null || target.CurrentHp == 0)
			{
				continue;
			}

			var id = target.GameObjectId;
			var ratio = target.GetHealthRatio();
			if (ratio >= 1f)
			{
				_ = _histories.Remove(id);
				continue;
			}

			if (!_histories.TryGetValue(id, out var history))
			{
				history = new History();
				_histories[id] = history;
			}
			else if (ratio > history.LastRatio + HpIncreaseTolerance)
			{
				history.Samples.Clear();
			}

			history.Samples.Enqueue((now, ratio));
			history.LastTick = now;
			history.LastRatio = ratio;
			while (now - history.Samples.Peek().Tick > WindowMs)
			{
				_ = history.Samples.Dequeue();
			}

			history.Rate = FitRate(history.Samples);
		}

		foreach ((var id, var history) in _histories)
		{
			if (now - history.LastTick > WindowMs)
			{
				_ = _histories.Remove(id);
			}
		}
	}

	internal static float GetRate(ulong id)
	{
		return _histories.TryGetValue(id, out var history) ? history.Rate : float.NaN;
	}

	/// <summary>
	/// The largest share of its health the enemy has lost within <paramref name="span"/> seconds over its recorded history:
	/// between any two samples at most the span plus the widest gap between neighbouring samples apart, so a fall in any
	/// window of the span lies within one pair. A history shorter than the span gives its whole fall, scaled up to the
	/// span. NaN without two samples (concept 01, A2; A525).
	/// </summary>
	/// <param name="id">The enemy's object id.</param>
	/// <param name="span">The span in seconds.</param>
	/// <returns>The largest fall as a share of its health, or NaN.</returns>
	internal static float LargestFall(ulong id, float span)
	{
		if (!_histories.TryGetValue(id, out var history) || history.Samples.Count <= 1 || span <= 0f)
		{
			return float.NaN;
		}

		var samples = history.Samples.ToArray();
		var first = samples[0];
		var last = samples[^1];
		long gap = 0;
		for (var i = 1; i < samples.Length; i++)
		{
			gap = Math.Max(gap, samples[i].Tick - samples[i - 1].Tick);
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
		for (var i = 0; i < samples.Length; i++)
		{
			for (var j = i + 1; j < samples.Length && samples[j].Tick - samples[i].Tick <= reach; j++)
			{
				largest = Math.Max(largest, samples[i].Ratio - samples[j].Ratio);
			}
		}

		return largest;
	}

	internal static void Clear()
	{
		_histories.Clear();
	}

	private static float FitRate(Queue<(long Tick, float Ratio)> samples)
	{
		var count = samples.Count;
		if (count < MinSamples)
		{
			return float.NaN;
		}

		var origin = samples.Peek().Tick;
		double meanT = 0, meanR = 0;
		foreach ((var tick, var ratio) in samples)
		{
			meanT += (tick - origin) / 1000.0;
			meanR += ratio;
		}

		meanT /= count;
		meanR /= count;

		double covariance = 0, variance = 0;
		foreach ((var tick, var ratio) in samples)
		{
			var dt = ((tick - origin) / 1000.0) - meanT;
			covariance += dt * (ratio - meanR);
			variance += dt * dt;
		}

		var rate = variance > 0 ? -covariance / variance : 0;
		return rate > 0 ? (float)rate : float.NaN;
	}
}
