using ECommons.DalamudServices;

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

		// Concept 01, A2 (A527): the last sample at full health before this fall began - the first hit falls between it
		// and the first sample here -, the enemy's health in points, and whether its death was read.
		public long FullTick = -1;
		public uint MaxHp;
		public bool Ended;
	}

	private static readonly Dictionary<ulong, History> _histories = [];

	// A527: when each enemy was last sampled at full health; the start of the fall its history does not hold.
	private static readonly Dictionary<ulong, long> _lastFull = [];
	private static readonly HashSet<ulong> _seen = [];

	// A527: the largest fall in points any enemy of this area took between two samples, by their distance in sampling
	// intervals (the index), the killing blow included. The area's own measure of how fast its enemies can fall.
	private static readonly List<float> _areaFall = [];
	private static uint _areaTerritory;
	private static long _lastRecord = -1;
	private static long _sampleGap;

	internal static void Record(List<IBattleChara> hostiles, long now)
	{
		var territory = (uint)Svc.ClientState.TerritoryType;
		if (territory != _areaTerritory)
		{
			_areaTerritory = territory;
			_areaFall.Clear();
		}

		if (_lastRecord >= 0)
		{
			_sampleGap = now - _lastRecord;
		}

		_lastRecord = now;
		_seen.Clear();
		for (var i = 0; i < hostiles.Count; i++)
		{
			var target = hostiles[i];
			if (target == null)
			{
				continue;
			}

			var id = target.GameObjectId;
			_ = _seen.Add(id);
			if (target.CurrentHp == 0)
			{
				LearnDeath(id, now);
				continue;
			}

			var ratio = target.GetHealthRatio();
			if (ratio >= 1f)
			{
				_ = _histories.Remove(id);
				_lastFull[id] = now;
				continue;
			}

			if (!_histories.TryGetValue(id, out var history))
			{
				history = new History { FullTick = _lastFull.TryGetValue(id, out var full) ? full : -1 };
				_histories[id] = history;
			}
			else if (ratio > history.LastRatio + HpIncreaseTolerance)
			{
				history.Samples.Clear();
				history.FullTick = -1;
			}

			history.MaxHp = target.MaxHp;
			if (history.FullTick >= 0 && now - history.FullTick > WindowMs)
			{
				history.FullTick = -1;
			}

			LearnFall(history, now, ratio);
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
			// Gone from the list since the last sample: its death, if its body still lies at 0.
			if (!_seen.Contains(id) && !history.Ended && Svc.Objects.SearchById(id) is IBattleChara { CurrentHp: 0 })
			{
				LearnDeath(id, now);
			}

			if (now - history.LastTick > WindowMs)
			{
				_ = _histories.Remove(id);
			}
		}

		foreach ((var id, var tick) in _lastFull)
		{
			if (now - tick > WindowMs)
			{
				_ = _lastFull.Remove(id);
			}
		}
	}

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

	/// <summary>
	/// The largest fall in health points any enemy of this area took within <paramref name="span"/> seconds - between two
	/// samples at most the span plus the sampling interval apart, its killing blow and the hit from full health included
	/// (concept 01, A2; A527). 0 while none fell here yet; a change of area starts it again.
	/// </summary>
	/// <param name="span">The span in seconds.</param>
	/// <returns>The fall in health points.</returns>
	internal static float AreaFall(float span)
	{
		if (_sampleGap <= 0)
		{
			return 0f;
		}

		var reach = ((long)TimeSpan.FromSeconds(span).TotalMilliseconds + _sampleGap) / _sampleGap;
		var largest = 0f;
		for (var i = 0; i < _areaFall.Count && i <= reach; i++)
		{
			largest = Math.Max(largest, _areaFall[i]);
		}

		return largest;
	}

	internal static float GetRate(ulong id)
	{
		return _histories.TryGetValue(id, out var history) ? history.Rate : float.NaN;
	}

	private static readonly List<(long Tick, float Ratio)> _points = [];

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

	internal static void Clear()
	{
		// A fight ends here: the deaths not yet read are learned first, the area's falls stay (A527).
		var now = Environment.TickCount64;
		foreach ((var id, var history) in _histories)
		{
			if (!history.Ended && Svc.Objects.SearchById(id) is IBattleChara { CurrentHp: 0 })
			{
				LearnDeath(id, now);
			}
		}

		_histories.Clear();
		_lastFull.Clear();
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
