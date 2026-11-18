using ECommons.ExcelServices;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 03, "Der Burst des Jobs als Uhr" (A885): when the rotation's own burst begins, read from its asks for the
/// burst medicine - every rotation asks for the tincture in its burst, so the asks are the one job-independent signal of
/// it. For a buff of <c>gap</c> seconds a burst begins with an ask after a gap longer than that: what lasts that long no
/// longer covers the asks before. The cadence is the median gap between two beginnings in the same fight, per job over
/// the session.
/// </summary>
/// <remarks>
/// The asks are kept once, as windows of asks no more than a GCD apart, per fight of this job's session; each clock
/// reads its beginnings from them when an ask opened a new window, so a clock made late misses nothing.
/// </remarks>
internal sealed class BurstClock
{
	// This fight's windows in combat seconds, the earlier fights of this job, and a count that moves with every new
	// window, fight and job (a clock recounts only then).
	private static readonly List<(float Start, float End)> _windows = [];
	private static readonly List<(float Start, float End)[]> _earlierFights = [];
	private static int _version;
	private static float _combatSeen = float.MaxValue;
	private static float _lastPullSeen = float.NaN;
	private static Job _job;

	private readonly Func<float> _gap;
	private int _countedVersion = -1;
	private readonly List<float> _cadences = [];
	private float _lastStart = float.NaN;
	private float _laterAt = float.NaN;
	private int _starts;

	/// <summary>The clock for a buff lasting <paramref name="gap"/> seconds, at least a GCD; read when it recounts.</summary>
	internal BurstClock(Func<float> gap)
	{
		_gap = gap;
	}

	/// <summary>The rotation asks for its burst medicine now, in combat (<c>UseBurstMedicine</c>).</summary>
	internal static void NoteAsk()
	{
		if (!DataCenter.InCombat)
		{
			return;
		}

		Sync();
		var now = DataCenter.CombatTimeRaw;
		if (_windows.Count > 0 && now - _windows[^1].End <= DataCenter.DefaultGCDTotal)
		{
			_windows[^1] = (_windows[^1].Start, now);
			return;
		}

		_windows.Add((now, now));
		_version++;
	}

	/// <summary>How many bursts began in this fight.</summary>
	internal int StartsThisFight
	{
		get
		{
			Count();
			return _starts;
		}
	}

	/// <summary>
	/// The combat second of this fight's first burst after a gap counted from the pull - the pull counts as an ask, so a
	/// first ask later than the gap is one (the tincture's later burst, A780); NaN without one.
	/// </summary>
	internal float LaterBurstAt
	{
		get
		{
			Count();
			return _laterAt;
		}
	}

	/// <summary>Whether the rotation asked within the last GCD: its burst runs now.</summary>
	internal static bool InBurstNow
	{
		get
		{
			Sync();
			return _windows.Count > 0 && DataCenter.CombatTimeRaw - _windows[^1].End <= DataCenter.DefaultGCDTotal;
		}
	}

	/// <summary>The measured cadence of this job's bursts; NaN before two began in one fight this session.</summary>
	internal float Cadence
	{
		get
		{
			Count();
			return _cadences.Count > 0 ? _cadences[_cadences.Count / 2] : float.NaN;
		}
	}

	/// <summary>
	/// Seconds until the next burst is due: the last beginning in this fight plus the cadence. NaN without either, and
	/// once the burst is more than a GCD late - it did not come, and nothing should wait for it.
	/// </summary>
	internal float SecondsToNext
	{
		get
		{
			var cadence = Cadence;
			if (float.IsNaN(_lastStart) || float.IsNaN(cadence))
			{
				return float.NaN;
			}

			var due = _lastStart + cadence - DataCenter.CombatTimeRaw;
			return due < -DataCenter.DefaultGCDTotal ? float.NaN : Math.Max(0f, due);
		}
	}

	// A new fight starts from the pull: out of combat, a lower combat time, or another last pull (a short pull can leave
	// the combat time higher than the one seen last, code review A883). Another job starts its history anew.
	private static void Sync()
	{
		if (_job != DataCenter.Job)
		{
			_job = DataCenter.Job;
			_windows.Clear();
			_earlierFights.Clear();
			_version++;
		}

		var now = DataCenter.InCombat ? DataCenter.CombatTimeRaw : 0f;
		if (now < _combatSeen || DataCenter.LastPullSeconds != _lastPullSeen)
		{
			_lastPullSeen = DataCenter.LastPullSeconds;
			if (_windows.Count > 1)
			{
				_earlierFights.Add([.. _windows]);
			}

			_windows.Clear();
			_version++;
		}

		_combatSeen = now;
	}

	// Recounts the beginnings for this clock's gap when a window, fight or job changed since the last count.
	private void Count()
	{
		Sync();
		if (_countedVersion == _version)
		{
			return;
		}

		_countedVersion = _version;
		var gap = Math.Max(_gap(), DataCenter.DefaultGCDTotal);
		_cadences.Clear();
		foreach (var fight in _earlierFights)
		{
			CountFight(fight, gap, out _, out _, out _);
		}

		CountFight(_windows, gap, out _starts, out _lastStart, out _laterAt);
		_cadences.Sort();
	}

	private void CountFight(IReadOnlyList<(float Start, float End)> windows, float gap, out int starts, out float lastStart, out float later)
	{
		starts = 0;
		lastStart = float.NaN;
		later = float.NaN;
		var previousEnd = 0f;
		for (var i = 0; i < windows.Count; i++)
		{
			var (start, end) = windows[i];
			var afterGap = start - previousEnd > gap;
			if (afterGap && float.IsNaN(later))
			{
				later = start;
			}

			if (afterGap || starts == 0)
			{
				if (!float.IsNaN(lastStart))
				{
					_cadences.Add(start - lastStart);
				}

				lastStart = start;
				starts++;
			}

			previousEnd = end;
		}
	}
}
