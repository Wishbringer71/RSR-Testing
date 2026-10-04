using ECommons.DalamudServices;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Learns which tankbuster markers are not tankbusters. The owner's rule (concept 15, V2): "negativliste
/// ingame aufbauen, wenn ein vfx nachträglich als tankbuster falsifiziert wurde. (sicher speichern ...)".
/// </summary>
/// <remarks>
/// <para>Every marker that the tankbuster paths catch on a party member opens a watch. An enemy action
/// that reaches the marked member - damage of any amount, blocked, parried or swallowed by a barrier,
/// or a hit turned away by invulnerability or evasion; auto-attacks excluded - confirms it. A watch that closes without such a hit falsifies the path, and it goes on
/// the list; the decision (DataCenter.IsCastingTankVfx, IsTankbusterVfxOnPlayer) then ignores it.</para>
///
/// <para>The watch keeps running for listed paths: a later hit after the same marker takes the path off
/// the list again. A wrong entry therefore costs one missed mitigation, on the next occurrence, and
/// then corrects itself. That is the reason a single observation is enough and no count is needed.</para>
///
/// <para>When a watch closes: once the marker has left the VFX queue - the queue decides how long a
/// marker counts as announcing a hit, and the decision reads the same queue - and every enemy cast that
/// ran while the marker stood has ended, plus one GCD for the hit to arrive. Both errors lean
/// the safe way: a window that is too long lets other damage confirm a marker, so it stays a
/// tankbuster, as before.</para>
///
/// <para>Inconclusive, and dropped without a verdict: the marked member dead or gone when the window
/// closes, and the fight ending first.</para>
///
/// <para>Detection and decision are kept apart: this records what the fight showed; the readers of the
/// list decide what to do about it.</para>
/// </remarks>
internal static class TankbusterMarkerWatch
{
	private sealed class Watch
	{
		public required ulong Target { get; init; }
		public required string Path { get; init; }
		public required DateTime Seen { get; init; }
		public required DateTime CastEnds { get; set; }
		public DateTime? LeftQueue { get; set; }
		public bool Hit { get; set; }
	}

	private static readonly List<Watch> _open = [];

	/// <summary>
	/// Opens watches for new markers and settles the ones whose window has closed. Called once per
	/// framework cycle, on the game thread.
	/// </summary>
	public static void Update()
	{
		if (!DataCenter.InCombat)
		{
			_open.Clear();
			return;
		}

		var now = DateTime.Now;
		var queue = DataCenter.VfxDataQueue;

		foreach (var vfx in queue)
		{
			if (string.IsNullOrEmpty(vfx.Path) || !DataCenter.IsTankbusterMarkerPath(vfx.Path) || IsWatched(vfx))
			{
				continue;
			}

			if (Svc.Objects.SearchById(vfx.ObjectId) is not IBattleChara target || !target.IsParty())
			{
				continue;
			}

			_open.Add(new Watch
			{
				Target = vfx.ObjectId,
				Path = vfx.Path.ToLowerInvariant(),
				Seen = vfx.Time,
				CastEnds = now + TimeSpan.FromSeconds(LongestEnemyCastRemaining()),
			});
		}

		var gcd = TimeSpan.FromSeconds(DataCenter.DefaultGCDTotal);
		var changed = false;
		for (var i = _open.Count - 1; i >= 0; i--)
		{
			var watch = _open[i];

			if (watch.Hit)
			{
				changed |= OtherConfiguration.TankbusterMarkerWithoutHit.Remove(watch.Path);
				_open.RemoveAt(i);
				continue;
			}

			// A cast that begins while the marker still stands belongs to it as much as one already
			// running when it appeared - an eight-second lock-on is often followed by its cast, and a
			// window fixed at the marker's first frame closed before that hit (re-audit of A189).
			if (watch.LeftQueue == null)
			{
				var castEnds = now + TimeSpan.FromSeconds(LongestEnemyCastRemaining());
				if (castEnds > watch.CastEnds)
				{
					watch.CastEnds = castEnds;
				}

				if (!InQueue(queue, watch))
				{
					watch.LeftQueue = now;
				}
			}

			if (watch.LeftQueue is not { } left)
			{
				continue;
			}

			var closes = (left > watch.CastEnds ? left : watch.CastEnds) + gcd;
			if (now < closes)
			{
				continue;
			}

			_open.RemoveAt(i);
			if (Svc.Objects.SearchById(watch.Target) is IBattleChara target && !target.IsDead && target.IsParty())
			{
				changed |= OtherConfiguration.TankbusterMarkerWithoutHit.Add(watch.Path);
			}
		}

		if (changed)
		{
			_ = OtherConfiguration.SaveTankbusterMarkerWithoutHit();
		}
	}

	/// <summary>
	/// An enemy action that is not an auto-attack damaged <paramref name="targetId"/>. Called from the
	/// effect handler, on the game thread. Returns whether a marker on that member stood for the hit
	/// that is not on the learned list of markers without one - the hit was a tankbuster on them.
	/// </summary>
	public static bool RecordHit(ulong targetId)
	{
		var marked = false;
		foreach (var watch in _open)
		{
			if (watch.Target == targetId)
			{
				watch.Hit = true;
				marked |= !OtherConfiguration.TankbusterMarkerWithoutHit.Contains(watch.Path);
			}
		}

		return marked;
	}

	private static bool IsWatched(VfxNewData vfx)
	{
		foreach (var watch in _open)
		{
			if (watch.Target == vfx.ObjectId && watch.Seen == vfx.Time
				&& string.Equals(watch.Path, vfx.Path, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private static bool InQueue(IEnumerable<VfxNewData> queue, Watch watch)
	{
		foreach (var vfx in queue)
		{
			if (vfx.ObjectId == watch.Target && vfx.Time == watch.Seen
				&& string.Equals(vfx.Path, watch.Path, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private static float LongestEnemyCastRemaining()
	{
		var longest = 0f;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && hostile.IsValid() && hostile.IsCasting)
			{
				var remaining = hostile.TotalCastTime - hostile.CurrentCastTime;
				if (remaining > longest)
				{
					longest = remaining;
				}
			}
		}

		return longest;
	}
}
