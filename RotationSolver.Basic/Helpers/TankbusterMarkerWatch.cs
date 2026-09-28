using ECommons.DalamudServices;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Learns which tankbuster markers are not tankbusters. The owner's rule (concept 15, V2): "negativliste
/// ingame aufbauen, wenn ein vfx nachträglich als tankbuster falsifiziert wurde. (sicher speichern ...)".
/// </summary>
/// <remarks>
/// <para>Every marker that the tankbuster paths catch on a party member opens a watch. An enemy action
/// that damages the marked member - any amount, a hit swallowed by a barrier included, auto-attacks
/// excluded - confirms it. A watch that closes without such a hit falsifies the path, and it goes on
/// the list; the decision (DataCenter.IsCastingTankVfx, IsTankbusterVfxOnPlayer) then ignores it.</para>
///
/// <para>The watch keeps running for listed paths: a later hit after the same marker takes the path off
/// the list again. A wrong entry therefore costs one missed mitigation, on the next occurrence, and
/// then corrects itself. That is the reason a single observation is enough and no count is needed.</para>
///
/// <para>When a watch closes: once the marker has left the VFX queue - the queue decides how long a
/// marker counts as announcing a hit, and the decision reads the same queue - and any enemy cast that
/// was running when the marker appeared has ended, plus one GCD for the hit to arrive. Both errors lean
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
		public required DateTime CastEnds { get; init; }
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
				changed |= OtherConfiguration.TankbusterMarkerFalsified.Remove(watch.Path);
				_open.RemoveAt(i);
				continue;
			}

			if (watch.LeftQueue == null && !InQueue(queue, watch))
			{
				watch.LeftQueue = now;
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
				changed |= OtherConfiguration.TankbusterMarkerFalsified.Add(watch.Path);
			}
		}

		if (changed)
		{
			_ = OtherConfiguration.SaveTankbusterMarkerFalsified();
		}
	}

	/// <summary>
	/// An enemy action that is not an auto-attack damaged <paramref name="targetId"/>. Called from the
	/// effect handler, on the game thread.
	/// </summary>
	public static void RecordHit(ulong targetId)
	{
		foreach (var watch in _open)
		{
			if (watch.Target == targetId)
			{
				watch.Hit = true;
			}
		}
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
			if (hostile != null && hostile.IsCasting)
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
