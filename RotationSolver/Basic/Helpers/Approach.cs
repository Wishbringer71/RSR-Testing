using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Enemies that run to another enemy, when each arrives and whether its arrival reaches the player's side (concept 21,
/// K3, "Gegner, die zu einem Verbündeten laufen", A660, A724; the owner's hint of 07.11.2026 on the Ogre's wisps, which
/// fly from four sides to the Ogre and hurt the player when they reach him).
/// </summary>
/// <remarks>
/// <para>The goal is the enemy with the most HP that could be fought in this combat; it stays the goal while it lives,
/// also while it cannot be targeted - the Ogre stands untargetable while its wisps march, and only they can be
/// targeted then.</para>
/// <para>An enemy runs to the goal when it has a target of its enmity, has looked at none of the player's side since it
/// was first seen, has closed in on the goal by more than its own size and has been watched for longer than a GCD. The
/// gaze tells it from an attacker running at a player who stands at the goal: an enemy that attacks someone looks at
/// him from its first step, one that follows a script looks at nobody (his recording of 11.11.2026: none of 24 wisps
/// ever took a gaze target, every attacking kind did within one reading of its first step; approach_model.py).</para>
/// <para>It arrives at its distance now less the distance at which one of its kind arrived, over the rate it has closed
/// in at during the last one to two GCDs - a wisp stands for a moment before it walks, and the rate since it was first
/// seen put its arrival late. Where its kind arrives and whether that reaches one of the player's side shows the first
/// action of one running; until then the distance is not known. Learned per kind for the stay in the territory. Updated
/// by the target update and by the watcher's enemy actions, read by the rotation.</para>
/// </remarks>
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

		// The distance a GCD ago and the one before it, for the closing rate of the last one to two GCDs.
		public DateTime MarkAt;
		public float Mark;
		public DateTime BeforeAt;
		public float Before;
		public bool HasBefore;

		public bool Engaged;
		public bool Looked;
		public bool Acted;
	}

	private static readonly Dictionary<ulong, Track> _tracks = [];
	private static readonly Dictionary<uint, float> _arrival = [];
	private static readonly HashSet<uint> _reaches = [];
	private static readonly HashSet<uint> _spares = [];
	private static readonly HashSet<ulong> _present = [];
	private static readonly List<ulong> _gone = [];
	private static uint _territory;
	private static ulong _goal;

	/// <summary>Takes this update's enemies and the goal they may run to.</summary>
	internal static void Update(List<IBattleChara> hostiles)
	{
		if (_territory != DataCenter.TerritoryID)
		{
			_territory = DataCenter.TerritoryID;
			_arrival.Clear();
			_reaches.Clear();
			_spares.Clear();
			_tracks.Clear();
			_goal = 0;
		}

		var goal = DataCenter.InCombat ? Goal(hostiles) : null;
		if (goal == null)
		{
			_tracks.Clear();
			_goal = 0;
			return;
		}

		if (goal.GameObjectId != _goal)
		{
			// Every distance so far was one to another enemy.
			_tracks.Clear();
			_goal = goal.GameObjectId;
		}

		var now = DateTime.Now;
		_present.Clear();
		foreach (var enemy in hostiles)
		{
			if (enemy == null || enemy.GameObjectId == goal.GameObjectId || enemy.IsDead)
			{
				continue;
			}

			_ = _present.Add(enemy.GameObjectId);
			var distance = Vector3.Distance(enemy.Position, goal.Position);
			if (!_tracks.TryGetValue(enemy.GameObjectId, out var track))
			{
				track = new Track
				{
					NameId = enemy.NameId,
					Since = now,
					From = distance,
					MarkAt = now,
					Mark = distance,
					Reach = goal.HitboxRadius + enemy.HitboxRadius,
					Size = enemy.HitboxRadius,
				};
				_tracks[enemy.GameObjectId] = track;
			}

			Follow(track, enemy, distance, now);
		}

		// One that left the list but stands, lives and cannot be targeted is still on its way: a wisp turns untargetable
		// a moment before it blows up. Any other is gone - fallen, or taken away with the end of the fight.
		_gone.Clear();
		foreach (var (id, track) in _tracks)
		{
			if (_present.Contains(id))
			{
				continue;
			}

			if (Svc.Objects.SearchById(id) is IBattleChara { IsDead: false, IsTargetable: false } hidden)
			{
				Follow(track, hidden, Vector3.Distance(hidden.Position, goal.Position), now);
			}
			else
			{
				_gone.Add(id);
			}
		}

		foreach (var id in _gone)
		{
			_ = _tracks.Remove(id);
		}
	}

	/// <summary>
	/// An enemy's action landed. The first one of an enemy that runs to another is its arrival: its kind arrives at that
	/// distance, and <paramref name="reachedYours"/> tells whether the arrival reaches the player's side. A kind seen to
	/// reach it stays so.
	/// </summary>
	internal static void ObserveAction(IBattleChara source, bool reachedYours)
	{
		if (!_tracks.TryGetValue(source.GameObjectId, out var track) || !Running(track))
		{
			return;
		}

		track.Acted = true;
		var kind = track.NameId;
		var news = !_arrival.TryGetValue(kind, out var known) || track.Now > known;
		if (news)
		{
			_arrival[kind] = track.Now;
		}

		if (reachedYours)
		{
			news |= _reaches.Add(kind);
			_ = _spares.Remove(kind);
		}
		else if (!_reaches.Contains(kind))
		{
			news |= _spares.Add(kind);
		}

		if (news)
		{
			DefenseTrace.Line($"approach: kind {kind} acted {track.Now:F1} y from the enemy it ran to, "
				+ $"{(track.At - track.Since).TotalSeconds:F1} s after it appeared, and "
				+ (_reaches.Contains(kind) ? "reached your side" : "reached none of your side") + " - its kind arrives there");
		}
	}

	/// <summary>
	/// Whether an enemy runs to another and its kind has not been seen to arrive without reaching the player's side;
	/// <paramref name="seconds"/> until the first such arrival, to the edges touching where
	/// <paramref name="learned"/> is false - one of them is of a kind whose arrival distance is not known yet.
	/// </summary>
	internal static bool TryNextArrival(out float seconds, out bool learned)
	{
		seconds = float.MaxValue;
		learned = true;
		var any = false;
		foreach (var track in _tracks.Values)
		{
			if (!Running(track) || _spares.Contains(track.NameId))
			{
				continue;
			}

			var known = _arrival.TryGetValue(track.NameId, out var arrival);
			var eta = Eta(track, known ? arrival : track.Reach);
			if (!known)
			{
				learned = false;
			}
			else if (eta == null)
			{
				continue;
			}

			any = true;
			if (eta is { } left && left < seconds)
			{
				seconds = left;
			}
		}

		return any;
	}

	// Seconds until the track is within the distance; null while it does not close in.
	private static float? Eta(Track track, float arrival)
	{
		var since = track.HasBefore ? track.BeforeAt : track.Since;
		var from = track.HasBefore ? track.Before : track.From;
		var seconds = (float)(track.At - since).TotalSeconds;
		var closed = from - track.Now;
		return seconds > 0f && closed > 0f ? Math.Max(0f, track.Now - arrival) * seconds / closed : null;
	}

	private static void Follow(Track track, IBattleChara enemy, float distance, DateTime now)
	{
		track.At = now;
		track.Now = distance;

		// One that has looked at the player's side attacks it, for the rest of the combat.
		if (!track.Looked)
		{
			track.Looked = LooksAtYourSide(enemy);
			track.Engaged = enemy.TargetObject != null;
		}

		if ((now - track.MarkAt).TotalSeconds >= DataCenter.DefaultGCDTotal)
		{
			track.BeforeAt = track.MarkAt;
			track.Before = track.Mark;
			track.HasBefore = true;
			track.MarkAt = now;
			track.Mark = distance;
		}
	}

	// In combat with the player's side, looking at none of it, closed in on the goal by more than its own size, and
	// watched for longer than a GCD: an attacker may be without a gaze target for a moment, a march takes many GCDs.
	private static bool Running(Track track)
	{
		return !track.Acted && track.Engaged && !track.Looked && track.From - track.Now > track.Size
			&& (track.At - track.Since).TotalSeconds > DataCenter.DefaultGCDTotal;
	}

	// The gaze target (Character.LookAt, the torso's; what a recording shows as PCTargetID) is a figure that is not on
	// the enemy's side: the player, a party member, one of their objects - or a figure of a kind enemies have that the
	// game does not let the player attack though it can be targeted, an ally of the duty.
	private static unsafe bool LooksAtYourSide(IBattleChara enemy)
	{
		var chara = (Character*)enemy.Address;
		if (chara == null)
		{
			return false;
		}

		var gaze = chara->LookAt.Controller.Params[0].TargetParam.TargetId;
		return Svc.Objects.SearchById(gaze) is IBattleChara seen
			&& (!seen.IsEnemySideFigure() || (seen.IsTargetable && !seen.IsEnemy()));
	}

	// The enemy with the most HP in the list; the goal so far stays it while it lives and is in the list or cannot be
	// targeted, unless one with more HP has come.
	private static IBattleChara? Goal(List<IBattleChara> hostiles)
	{
		IBattleChara? goal = null;
		var listed = false;
		foreach (var enemy in hostiles)
		{
			if (enemy == null || enemy.IsDead)
			{
				continue;
			}

			listed |= enemy.GameObjectId == _goal;
			if (enemy.MaxHp > (goal?.MaxHp ?? 0u))
			{
				goal = enemy;
			}
		}

		return _goal != 0 && goal?.GameObjectId != _goal
			&& Svc.Objects.SearchById(_goal) is IBattleChara { IsDead: false } held
			&& (listed || !held.IsTargetable) && held.MaxHp >= (goal?.MaxHp ?? 0u)
			? held
			: goal;
	}
}
