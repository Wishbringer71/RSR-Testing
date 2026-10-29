namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Where the enemies have been over the player's last GCD, so a damage area can be laid where they stay: at least one
/// has been inside it throughout that GCD, and no fewer are inside now than, of those still alive, were inside at its
/// start; and the tank has gathered the pack - no enemy out of combat in the area or, in a duty, within a tank's area
/// attack reach, none come into combat there within that GCD (concept 04, "Schadensflächen erst, wo die Gegner
/// bleiben", A350-A354).
/// </summary>
/// <remarks>
/// <para>The owner's hints (25.10.2026): enemies move about inside an area without leaving it; and in a big pull not all
/// fit into it - one pushing in pushes another out and takes its place - while enemies die over time, so "es kann nicht
/// einfach nur ein zähler der gegner innerhalb der schadensfläche sein". What wastes an area is the pack moving on, not
/// an enemy shifting inside it, a swap at its edge or a death. The count compares the same enemies: a swap leaves it, a
/// death leaves both sides, a pack moving on takes it down. A rule that let nobody leave was open only 14-51 % of the
/// time a big pull stayed in the model; this one 61-75 %, and it closes on a pack moving on within the GCD
/// (damage_area_model.py).</para>
/// <para>His further hint: a tank stops on the way to pick up the next group with an area attack that draws enmity and,
/// since his rotation may be elsewhere just then, waits until it has gone out before he moves on. That group stood idle
/// where it was - by its track it has stayed - and the pack he brought settles around him while he waits; in the model
/// the area went out at every such stop. An enemy out of combat in the area or within the tank's reach is one he is
/// about to pick up, and a group just come into combat there is one he moves on with; laid on an idle group, the area
/// would also pull it. Out of combat is the enemy's own combat flag with no target - WrathCombo reads the same flag for
/// "Only Attack Targets Already In Combat" (not a game source). One first seen already in combat counts as just come
/// into combat: a group pulled out of the player's sight shows up that way. Only where it stands counts - in the area
/// or at a tank -, so an add wave across the room holds nothing up. An idle enemy at a tank counts only in a duty that
/// is not a field operation: there every enemy a tank stands at is about to be pulled, in the open world a passive one
/// may stand there for good. A striking dummy never moves and is left out.</para>
/// <para>The game gives no movement state for an enemy - only for the player (AgentMap) - so the track is the position
/// the client shows at each target update, a new point whenever it changes. A GCD in the area is what tells a stop from
/// a pause: a pack following its tank at run speed crosses an area of 5 y in under a second. An enemy seen for the first
/// time within that GCD has not stayed anywhere yet; one arriving does not hold the area up. Updated by the target
/// update, read there by the rotation.</para>
/// </remarks>
internal static class HostileMovement
{
	private static readonly Dictionary<ulong, List<(DateTime At, Vector3 Pos)>> _tracks = [];
	private static readonly HashSet<ulong> _present = [];
	private static readonly List<ulong> _gone = [];
	private static readonly HashSet<ulong> _outOfCombat = [];
	private static readonly Dictionary<ulong, DateTime> _cameIntoCombat = [];
	private static readonly object _gradeGate = new();
	private static LaidArea? _laid;
	private static float _tankReach;

	private static TimeSpan Window => TimeSpan.FromSeconds(DataCenter.DefaultGCDTotal);

	/// <summary>
	/// How far a tank's area attack reaches around him: the effect range of the four tanks' first area attack, from the
	/// action sheet (5 y each in 7.5). Kept once read; a sheet not ready yet is asked again.
	/// </summary>
	private static float TankReach => _tankReach > 0f ? _tankReach : _tankReach = LongestEffectRange(
		ActionID.TotalEclipsePvE, ActionID.OverpowerPvE, ActionID.UnleashPvE, ActionID.DemonSlicePvE);

	// Where a tank stands at an enemy he is about to pull with his area attack: in a duty, not in the open world or a
	// field operation, where passive enemies stand about.
	private static bool PullsEveryEnemy => DataCenter.IsInDuty && !DataCenter.IsInFieldOperations;

	/// <summary>Takes the positions of this update's enemies, and when each came into combat.</summary>
	internal static void Update(List<IBattleChara> hostiles)
	{
		var now = DateTime.Now;
		var window = Window;
		var start = now - window;
		_present.Clear();
		foreach (var enemy in hostiles)
		{
			if (enemy == null)
			{
				continue;
			}

			var id = enemy.GameObjectId;
			_ = _present.Add(id);
			var seen = _tracks.TryGetValue(id, out var track);
			if (!seen)
			{
				track = [];
				_tracks[id] = track;
			}

			var at = enemy.Position;
			if (track!.Count == 0 || track[^1].Pos != at)
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

			if (enemy.IsDummy())
			{
				continue;
			}

			if (!Engaged(enemy))
			{
				_ = _outOfCombat.Add(id);
				_ = _cameIntoCombat.Remove(id);
			}
			else if (_outOfCombat.Remove(id) || !seen)
			{
				_cameIntoCombat[id] = now;
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

		foreach (var (id, since) in _cameIntoCombat)
		{
			if (now - since >= window)
			{
				_gone.Add(id);
			}
		}

		foreach (var id in _gone)
		{
			if (!_present.Contains(id))
			{
				_ = _tracks.Remove(id);
				_ = _outOfCombat.Remove(id);
			}

			_ = _cameIntoCombat.Remove(id);
		}

		Grade(hostiles, now);
	}

	/// <summary>Forgets every enemy, when there are none; an area still being graded is written as it stands.</summary>
	internal static void Clear()
	{
		_tracks.Clear();
		_outOfCombat.Clear();
		_cameIntoCombat.Clear();
		lock (_gradeGate)
		{
			Finish();
		}
	}

	/// <summary>
	/// Whether an area of <paramref name="radius"/> around <paramref name="centre"/> holds its enemies: at least one of
	/// <paramref name="hostiles"/> - the living ones - has been inside it throughout the last GCD, and no fewer are inside
	/// it now than were inside at the start of that GCD; no enemy out of combat is inside it or, in a duty, within a party
	/// tank's area attack reach, and none inside it or at a tank came into combat within that GCD. Inside is the enemy's
	/// hitbox within the radius, as the game's area check has it.
	/// </summary>
	internal static bool AreaHolds(Vector3 centre, float radius, List<IBattleChara> hostiles)
	{
		var now = DateTime.Now;
		var start = now - Window;
		var pullsEveryEnemy = PullsEveryEnemy;
		var stayed = false;
		var atStart = 0;
		var insideNow = 0;
		foreach (var enemy in hostiles)
		{
			if (enemy == null || enemy.IsDead)
			{
				continue;
			}

			var id = enemy.GameObjectId;
			var hitbox = enemy.HitboxRadius;
			var inNow = Inside(enemy.Position, centre, radius, hitbox);
			if (_outOfCombat.Contains(id) ? inNow || (pullsEveryEnemy && NearTank(enemy.Position, hitbox))
				: _cameIntoCombat.ContainsKey(id) && (inNow || NearTank(enemy.Position, hitbox)))
			{
				return false;
			}

			if (!_tracks.TryGetValue(id, out var track) || track.Count == 0)
			{
				continue;
			}

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

			if (inNow)
			{
				insideNow++;
			}

			stayed |= alwaysInside && inNow;
		}

		return stayed && insideNow >= atStart;
	}

	/// <summary>
	/// An area the player laid: <paramref name="name"/>, <paramref name="radius"/> around <paramref name="centre"/> for
	/// <paramref name="duration"/> seconds. Graded over its duration and written to the defence trace: how long enemies
	/// were in it, and how long the enemies it was laid on were alive outside it - a pack that moved on. Called by the
	/// effect handler.
	/// </summary>
	/// <remarks>
	/// The measure for a rule whose answer falls in play. It does not steer the rule: learning the wait from areas left
	/// behind would need a bound on how long a stay still counts as a pause - an area that worked for 10 s is hardly
	/// wasted, but 10 s of waiting at every stop would cost much - and that bound has no source (A353).
	/// </remarks>
	internal static void AreaLaid(string name, Vector3 centre, float radius, float duration, List<IBattleChara> hostiles)
	{
		lock (_gradeGate)
		{
			Finish();
			if (duration <= 0f)
			{
				return;
			}

			var now = DateTime.Now;
			var laid = new LaidArea(name, centre, radius, duration, now);
			foreach (var enemy in hostiles)
			{
				if (enemy != null && !enemy.IsDead && Inside(enemy.Position, centre, radius, enemy.HitboxRadius))
				{
					_ = laid.LaidOn.Add(enemy.GameObjectId);
				}
			}

			_laid = laid;
		}
	}

	private static void Grade(List<IBattleChara> hostiles, DateTime now)
	{
		lock (_gradeGate)
		{
			if (_laid is not { } laid)
			{
				return;
			}

			var until = now < laid.Ends ? now : laid.Ends;
			var step = until - laid.Seen;

			// Not watched in between (RSR off): one look cannot stand for that stretch, so the area goes ungraded.
			if (step > Window)
			{
				_laid = null;
				return;
			}

			if (step > TimeSpan.Zero)
			{
				var anyInside = false;
				var laidOnAlive = false;
				foreach (var enemy in hostiles)
				{
					if (enemy == null || enemy.IsDead)
					{
						continue;
					}

					anyInside |= Inside(enemy.Position, laid.Centre, laid.Radius, enemy.HitboxRadius);
					laidOnAlive |= laid.LaidOn.Contains(enemy.GameObjectId);
				}

				if (anyInside)
				{
					laid.Held += step.TotalSeconds;
				}
				else if (laidOnAlive)
				{
					laid.Left += step.TotalSeconds;
				}

				laid.Seen = until;
			}

			if (now >= laid.Ends)
			{
				Finish();
			}
		}
	}

	// Under _gradeGate.
	private static void Finish()
	{
		if (_laid is not { } laid)
		{
			return;
		}

		_laid = null;
		DefenseTrace.Line($"damage area {laid.Name} ({laid.Duration:F0} s, on {laid.LaidOn.Count}): enemies in it "
			+ $"{laid.Held:F1} s, the ones it was laid on alive outside it {laid.Left:F1} s"
			+ (Service.Config.DamageAreasWhereEnemiesStay ? string.Empty : " (rule off)"));
	}

	// In combat by its own flag, or attacking someone: a target is the other sign of an enemy that has been pulled.
	private static bool Engaged(IBattleChara enemy)
	{
		return enemy.InCombat() || enemy.TargetObjectId is not 0 and not 0xE0000000;
	}

	private static bool NearTank(Vector3 pos, float hitbox)
	{
		var reach = TankReach;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member != null && !member.IsDead && member.IsJobCategory(JobRole.Tank)
				&& Inside(pos, member.Position, reach, hitbox))
			{
				return true;
			}
		}

		return false;
	}

	private static float LongestEffectRange(params ActionID[] actions)
	{
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.Action>();
		float longest = 0;
		foreach (var action in actions)
		{
			if (sheet.TryGetRow((uint)action, out var row))
			{
				longest = Math.Max(longest, row.EffectRange);
			}
		}

		return longest;
	}

	private static bool Inside(Vector3 pos, Vector3 centre, float radius, float hitbox)
		=> Vector3.Distance(centre, pos) - hitbox <= radius;

	private sealed class LaidArea(string name, Vector3 centre, float radius, float duration, DateTime at)
	{
		public string Name { get; } = name;
		public Vector3 Centre { get; } = centre;
		public float Radius { get; } = radius;
		public float Duration { get; } = duration;
		public DateTime Ends { get; } = at.AddSeconds(duration);
		public HashSet<ulong> LaidOn { get; } = [];
		public DateTime Seen { get; set; } = at;
		public double Held { get; set; }
		public double Left { get; set; }
	}
}
