namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Where the enemies have been over the player's last GCD, so a damage area can be laid where they stay: at least one
/// has been inside it throughout that GCD, and none that was inside it in that time has left (concept 04,
/// "Schadensflächen erst, wo die Gegner bleiben", A350).
/// </summary>
/// <remarks>
/// <para>The owner's correction (25.10.2026): enemies move about inside an area without leaving it. What wastes an area
/// is an enemy leaving it - a pack moving on behind its tank, or passing through - not one shifting its place in it; the
/// first version, which asked that none had moved at all, would never have laid an area under a pack that keeps
/// shifting (damage_area_model.py).</para>
/// <para>The game gives no movement state for an enemy - only for the player (AgentMap) - so the track is the position
/// the client shows at each target update, a new point whenever it changes. A GCD in the area is what tells a stop from
/// a pause: a pack following its tank at run speed crosses an area of 5 y in under a second. An enemy seen for the first
/// time within that GCD has not stayed anywhere yet; one arriving does not hold the area up. Updated by the target
/// update on the game thread, read there by the rotation.</para>
/// </remarks>
internal static class HostileMovement
{
	private static readonly Dictionary<ulong, List<(DateTime At, Vector3 Pos)>> _tracks = [];
	private static readonly HashSet<ulong> _present = [];
	private static readonly List<ulong> _gone = [];

	private static TimeSpan Window => TimeSpan.FromSeconds(DataCenter.DefaultGCDTotal);

	/// <summary>Takes the positions of this update's enemies.</summary>
	internal static void Update(List<IBattleChara> hostiles)
	{
		var now = DateTime.Now;
		var start = now - Window;
		_present.Clear();
		foreach (var enemy in hostiles)
		{
			if (enemy == null)
			{
				continue;
			}

			var id = enemy.GameObjectId;
			_ = _present.Add(id);
			if (!_tracks.TryGetValue(id, out var track))
			{
				track = [];
				_tracks[id] = track;
			}

			var at = enemy.Position;
			if (track.Count == 0 || track[^1].Pos != at)
			{
				track.Add((now, at));
			}

			// The last position held before the window began stays: it is where the enemy stood at its start.
			var keepFrom = 0;
			while (keepFrom + 1 < track.Count && track[keepFrom + 1].At <= start)
			{
				keepFrom++;
			}

			if (keepFrom > 0)
			{
				track.RemoveRange(0, keepFrom);
			}
		}

		_gone.Clear();
		foreach (var id in _tracks.Keys)
		{
			if (!_present.Contains(id))
			{
				_gone.Add(id);
			}
		}

		foreach (var id in _gone)
		{
			_ = _tracks.Remove(id);
		}
	}

	/// <summary>Forgets every enemy, when there are none.</summary>
	internal static void Clear()
	{
		_tracks.Clear();
	}

	/// <summary>
	/// Whether an area of <paramref name="radius"/> around <paramref name="centre"/> holds its enemies: at least one of
	/// <paramref name="hostiles"/> has been inside it throughout the last GCD, and none that was inside it during that
	/// GCD is outside now. Inside is the enemy's hitbox within the radius, as the game's area check has it.
	/// </summary>
	internal static bool AreaHolds(Vector3 centre, float radius, List<IBattleChara> hostiles)
	{
		var start = DateTime.Now - Window;
		var stayed = false;
		foreach (var enemy in hostiles)
		{
			if (enemy == null || enemy.IsDead || !_tracks.TryGetValue(enemy.GameObjectId, out var track) || track.Count == 0)
			{
				continue;
			}

			var hitbox = enemy.HitboxRadius;
			var insideNow = Inside(enemy.Position, centre, radius, hitbox);
			var wasInside = false;
			var alwaysInside = track[0].At <= start;
			foreach (var (_, pos) in track)
			{
				var inside = Inside(pos, centre, radius, hitbox);
				wasInside |= inside;
				alwaysInside &= inside;
			}

			// One that has left within the GCD: the pack is moving on, or passed through.
			if (wasInside && !insideNow)
			{
				return false;
			}

			stayed |= alwaysInside && insideNow;
		}

		return stayed;
	}

	private static bool Inside(Vector3 pos, Vector3 centre, float radius, float hitbox)
		=> Vector3.Distance(centre, pos) - hitbox <= radius;
}
