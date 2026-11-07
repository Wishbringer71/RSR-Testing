using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.DalamudServices;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Enemies that run to another enemy and when each arrives (concept 21, K3, "Gegner, die zu einem Verbündeten laufen",
/// A660; the owner's hint of 07.11.2026 on the Ogre's wisps, which fly from four sides to the Ogre and hurt the player when
/// they reach him). The goal is the enemy with the most HP, also while it cannot be targeted - the Ogre stands
/// untargetable while its wisps march. An enemy runs to it while its distance to it has shrunk by more than its own size
/// since it was first seen; it arrives at its distance now less the distance at which one of its kind vanished, over the
/// rate it has closed in at. That distance is learned at the first one of the kind to vanish on its way; before, the
/// edges of the two touch. Updated by the target update, read there by the target order and by the rotation.
/// </summary>
internal static class Approach
{
	private sealed class Track
	{
		public uint NameId;
		public DateTime Since;
		public float From;
		public DateTime At;
		public float Now;
		public float Reach;
		public float Size;
	}

	private static readonly Dictionary<ulong, Track> _tracks = [];
	private static readonly Dictionary<uint, float> _arrival = [];
	private static readonly List<ulong> _gone = [];
	private static uint _territory;

	/// <summary>Takes this update's enemies and the goal they may run to.</summary>
	internal static void Update(List<IBattleChara> hostiles)
	{
		if (_territory != DataCenter.TerritoryID)
		{
			_territory = DataCenter.TerritoryID;
			_arrival.Clear();
			_tracks.Clear();
		}

		if (!DataCenter.InCombat)
		{
			_tracks.Clear();
			return;
		}

		var goal = Goal();
		if (goal == null)
		{
			_tracks.Clear();
			return;
		}

		var now = DateTime.Now;
		var present = new HashSet<ulong>();
		foreach (var enemy in hostiles)
		{
			if (enemy == null || enemy.GameObjectId == goal.GameObjectId || enemy.IsDead)
			{
				continue;
			}

			_ = present.Add(enemy.GameObjectId);
			var distance = Vector3.Distance(enemy.Position, goal.Position);
			if (!_tracks.TryGetValue(enemy.GameObjectId, out var track))
			{
				_tracks[enemy.GameObjectId] = new Track
				{
					NameId = enemy.NameId,
					Since = now,
					From = distance,
					At = now,
					Now = distance,
					Reach = goal.HitboxRadius + enemy.HitboxRadius,
					Size = enemy.HitboxRadius,
				};
				continue;
			}

			track.At = now;
			track.Now = distance;
		}

		// One that left the list on its way without being seen dead: it arrived, and its kind arrives at that distance.
		_gone.Clear();
		foreach (var (id, track) in _tracks)
		{
			if (!present.Contains(id))
			{
				_gone.Add(id);
				if (Running(track) && Svc.Objects.SearchById(id) is not IBattleChara { IsDead: true }
					&& (!_arrival.TryGetValue(track.NameId, out var known) || track.Now < known))
				{
					_arrival[track.NameId] = track.Now;
					DefenseTrace.Line($"approach: kind {track.NameId} vanished {track.Now:F1} y from the enemy it ran to, "
						+ $"{(track.At - track.Since).TotalSeconds:F1} s after it appeared - its kind arrives there");
				}
			}
		}

		foreach (var id in _gone)
		{
			_ = _tracks.Remove(id);
		}
	}

	/// <summary>Seconds until <paramref name="enemy"/> reaches the enemy it runs to; null while it runs to none.</summary>
	internal static float? Eta(IBattleChara enemy)
	{
		return _tracks.TryGetValue(enemy.GameObjectId, out var track) ? Eta(track) : null;
	}

	/// <summary>Seconds until the first enemy running to another arrives; null while none runs.</summary>
	internal static float? SoonestArrival
	{
		get
		{
			float? soonest = null;
			foreach (var track in _tracks.Values)
			{
				if (Eta(track) is { } eta && (soonest == null || eta < soonest))
				{
					soonest = eta;
				}
			}

			return soonest;
		}
	}

	private static float? Eta(Track track)
	{
		var seconds = (float)(track.At - track.Since).TotalSeconds;
		if (!Running(track) || seconds <= 0f)
		{
			return null;
		}

		var speed = (track.From - track.Now) / seconds;
		var arrival = _arrival.TryGetValue(track.NameId, out var learned) ? learned : track.Reach;
		return Math.Max(0f, track.Now - arrival) / speed;
	}

	// It has closed in on the goal by more than its own size: running to it, not milling about.
	private static bool Running(Track track)
	{
		return track.From - track.Now > track.Size;
	}

	// The enemy with the most HP, targetable or not.
	private static IBattleChara? Goal()
	{
		IBattleChara? goal = null;
		foreach (var obj in Svc.Objects)
		{
			if (obj is IBattleChara npc && obj is IBattleNpc && !npc.IsDead && npc.IsEnemy()
				&& npc.GetBattleNPCSubKind() is BattleNpcSubKind.Combatant && npc.MaxHp > (goal?.MaxHp ?? 0u))
			{
				goal = npc;
			}
		}

		return goal;
	}
}
