using ECommons.DalamudServices;

namespace RotationSolver.Basic.Helpers;

internal static partial class TimeToKillTracker
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
	private static long _lastRecord = -1;

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
