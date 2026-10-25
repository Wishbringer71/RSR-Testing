namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Where the enemies have been over the player's last GCD, so a damage area can be laid where they stay: at least one
/// has been inside it throughout that GCD, and no fewer are inside now than, of those still alive, were inside at its
/// start (concept 04, "Schadensflächen erst, wo die Gegner bleiben", A350-A352).
/// </summary>
/// <remarks>
/// <para>The owner's hints (25.10.2026): enemies move about inside an area without leaving it; and in a big pull not all
/// fit into it - one pushing in pushes another out and takes its place - while enemies die over time, so "es kann nicht
/// einfach nur ein zähler der gegner innerhalb der schadensfläche sein". What wastes an area is the pack moving on, not
/// an enemy shifting inside it, a swap at its edge or a death. The count compares the same enemies: a swap leaves it, a
/// death leaves both sides, a pack moving on takes it down. A rule that let nobody leave was open only 14-51 % of the
/// time a big pull stayed in the model; this one 61-75 %, and it closes on a pack moving on within the GCD
/// (damage_area_model.py).</para>
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
	/// <paramref name="hostiles"/> - the living ones - has been inside it throughout the last GCD, and no fewer are inside
	/// it now than were inside at the start of that GCD. Inside is the enemy's hitbox within the radius, as the game's
	/// area check has it.
	/// </summary>
	internal static bool AreaHolds(Vector3 centre, float radius, List<IBattleChara> hostiles)
	{
		var start = DateTime.Now - Window;
		var stayed = false;
		var atStart = 0;
		var insideNow = 0;
		foreach (var enemy in hostiles)
		{
			if (enemy == null || enemy.IsDead || !_tracks.TryGetValue(enemy.GameObjectId, out var track) || track.Count == 0)
			{
				continue;
			}

			var hitbox = enemy.HitboxRadius;
			var now = Inside(enemy.Position, centre, radius, hitbox);
			var known = track[0].At <= start;
			var alwaysInside = known;
			foreach (var (_, pos) in track)
			{
				alwaysInside &= Inside(pos, centre, radius, hitbox);
			}

			// The position held at the start of the GCD is the first of the track when it began before it.
			if (known && Inside(track[0].Pos, centre, radius, hitbox))
			{
				atStart++;
			}

			if (now)
			{
				insideNow++;
			}

			stayed |= alwaysInside && now;
		}

		return stayed && insideNow >= atStart;
	}

	private static bool Inside(Vector3 pos, Vector3 centre, float radius, float hitbox)
		=> Vector3.Distance(centre, pos) - hitbox <= radius;
}
