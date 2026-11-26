using ECommons.GameHelpers;
using System.Text;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 03, "Bereit, aber nicht genutzt", GCD-Leerlauf (A923): a stretch longer than a GCD in which the GCD stood free
/// in a fight, a hostile was in range, RSR played and the player was not casting, yet no GCD came. At its end one trace
/// line names how long, which GCD the rotation had chosen in it and which GCDs <c>CanUse</c> refused with which reason
/// (fed by <see cref="ActionTracer.Reject"/>). It decides nothing; it names the cause of a stall that otherwise only shows
/// as auto-attacks.
/// </summary>
internal static class GcdIdleWatch
{
	private static float _idle;
	private static DateTime _lastFrame = DateTime.MinValue;
	private static readonly List<string> _chosen = [];
	private static readonly Dictionary<string, int> _refused = [];

	/// <summary>Seconds the GCD has stood free in a fight without one coming; zero outside such a stretch.</summary>
	internal static float IdleSeconds => _idle;

	/// <summary><c>CanUse</c> refused <paramref name="action"/> for <paramref name="reason"/>: counted while a stall runs.</summary>
	internal static void Refused(IBaseAction action, string reason)
	{
		if (_idle <= 0f || !action.Info.IsGeneralGCD)
		{
			return;
		}

		var key = $"{action.Name} ({reason})";
		_refused[key] = _refused.TryGetValue(key, out var count) ? count + 1 : 1;
	}

	/// <summary>Once per frame after the rotation chose: <paramref name="nextGcd"/> is its GCD, if any.</summary>
	internal static void Frame(IBaseAction? nextGcd)
	{
		var now = DateTime.Now;
		var delta = _lastFrame == DateTime.MinValue ? 0f : (float)(now - _lastFrame).TotalSeconds;
		_lastFrame = now;

		// A973: more than a GCD between two calls is time RSR did not watch - it stood off, or no rotation ran between two
		// areas. The stretch ends there and the next one starts afresh; before, the first frame of the next fight added the
		// whole pause ("gcd idle: 1680.6 s", 25.11.2026), and D2 read it as a GCD long missed.
		if (delta > DataCenter.DefaultGCDTotal)
		{
			Close();
			delta = 0f;
		}

		var idle = DataCenter.InCombat && DataCenter.IsActivated() && DataCenter.HasHostilesInRange
			&& Player.Object is { IsDead: false, IsCasting: false } && DataCenter.DefaultGCDRemain <= 0f;
		if (!idle)
		{
			Close();
			return;
		}

		_idle += delta;
		if (nextGcd != null && !_chosen.Contains(nextGcd.Name))
		{
			_chosen.Add(nextGcd.Name);
		}
	}

	private static void Close()
	{
		if (_idle > DataCenter.DefaultGCDTotal && DataCenter.DefaultGCDTotal > 0f)
		{
			var line = new StringBuilder($"gcd idle: {_idle:F1} s without a GCD - chosen {(_chosen.Count == 0 ? "none" : string.Join(", ", _chosen))}");
			if (_refused.Count > 0)
			{
				_ = line.Append("; refused");
				foreach (var (reason, count) in _refused)
				{
					_ = line.Append($" {reason} {count}");
				}
			}

			DefenseTrace.Line(line.ToString());
		}

		_idle = 0f;
		_chosen.Clear();
		_refused.Clear();
	}
}
