using ECommons.GameHelpers;
using System.Text;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Cooldowns that stood ready in a fight and did not come (concept 03, "Bereit, aber nicht genutzt", A820). Every action of
/// the running rotation with a recast group of its own beside the GCD - abilities and GCDs like Technical Step - counts
/// while a charge of it is ready, the player is in combat and a hostile is in range; defence and movement do not. Each
/// stretch longer than a GCD is added up with the reasons <c>CanUse</c> refused it for (<see cref="Refused"/>, fed by
/// <see cref="ActionTracer.Reject"/>), and one trace line per fight names them. It decides nothing: it shows where a
/// rotation waits on purpose - charges pooled, a window awaited - and where a check holds it.
/// </summary>
internal static class BurstWatch
{
	private sealed class Watched(IBaseAction action)
	{
		public readonly IBaseAction Action = action;
		public readonly Dictionary<string, int> Reasons = [];
		public float Current;
		public float Held;
		public int Stretches;
		public int Refusals;
	}

	private static ICustomRotation? _rotation;
	private static readonly List<Watched> _watched = [];
	private static readonly Dictionary<uint, Watched> _byId = [];
	private static DateTime _lastFrame = DateTime.MinValue;

	/// <summary>
	/// <c>CanUse</c> refused <paramref name="action"/> for <paramref name="reason"/> (a fixed string): counted while it is
	/// ready and waits.
	/// </summary>
	internal static void Refused(IBaseAction action, string reason)
	{
		if (!_byId.TryGetValue(action.ID, out var watched) || watched.Current <= 0f)
		{
			return;
		}

		watched.Refusals++;
		watched.Reasons[reason] = watched.Reasons.TryGetValue(reason, out var count) ? count + 1 : 1;
	}

	/// <summary>Once per frame: how long each watched action has stood ready without coming.</summary>
	internal static void Frame()
	{
		var now = DateTime.Now;
		var delta = _lastFrame == DateTime.MinValue ? 0f : (float)(now - _lastFrame).TotalSeconds;
		_lastFrame = now;

		var rotation = DataCenter.CurrentRotation;
		if (!ReferenceEquals(rotation, _rotation))
		{
			Rebuild(rotation);
		}

		// Only while RSR plays: a cooldown the player himself leaves is not the rotation's.
		if (!DataCenter.InCombat || !DataCenter.IsActivated() || Player.Object is not { IsDead: false } || delta <= 0f)
		{
			return;
		}

		var hostileNear = DataCenter.HasHostilesInRange;
		var gcd = DataCenter.DefaultGCDTotal;
		for (var i = 0; i < _watched.Count; i++)
		{
			var watched = _watched[i];
			var action = watched.Action;
			if (hostileNear && action.EnoughLevel && action.IsEnabled && action.Cooldown.HasOneCharge)
			{
				watched.Current += delta;
				continue;
			}

			Close(watched, gcd);
		}
	}

	/// <summary>The fight ended: one line with every action that stood ready longer than a GCD, longest first.</summary>
	internal static void EndFight()
	{
		var gcd = DataCenter.DefaultGCDTotal;
		for (var i = 0; i < _watched.Count; i++)
		{
			Close(_watched[i], gcd);
		}

		_watched.Sort(static (a, b) => b.Held.CompareTo(a.Held));
		var line = new StringBuilder();
		for (var i = 0; i < _watched.Count; i++)
		{
			var watched = _watched[i];
			if (watched.Stretches == 0)
			{
				continue;
			}

			_ = line.Append(line.Length == 0 ? string.Empty : "; ")
				.Append($"{watched.Action.Name} {watched.Held:F1} s in {watched.Stretches}");
			if (watched.Refusals == 0)
			{
				_ = line.Append(" (not asked)");
			}
			else
			{
				_ = line.Append(" (refused");
				foreach (var (reason, count) in watched.Reasons)
				{
					_ = line.Append($" {reason} {count}");
				}

				_ = line.Append(')');
			}
		}

		if (line.Length > 0)
		{
			DefenseTrace.Line($"ready but unused over the fight: {line}");
		}

		for (var i = 0; i < _watched.Count; i++)
		{
			var watched = _watched[i];
			watched.Current = watched.Held = 0f;
			watched.Stretches = watched.Refusals = 0;
			watched.Reasons.Clear();
		}

		_lastFrame = DateTime.MinValue;
	}

	// A stretch ends and counts when it outlasted a GCD; the refusals count over all the fight's ready time.
	private static void Close(Watched watched, float gcd)
	{
		if (watched.Current <= 0f)
		{
			return;
		}

		if (watched.Current > gcd)
		{
			watched.Held += watched.Current;
			watched.Stretches++;
		}

		watched.Current = 0f;
	}

	private static void Rebuild(ICustomRotation? rotation)
	{
		_rotation = rotation;
		_watched.Clear();
		_byId.Clear();
		if (rotation == null)
		{
			return;
		}

		foreach (var action in rotation.AllBaseActions)
		{
			var row = action.Action;
			if (row.CooldownGroup == ActionHelper.GCDCooldownGroup
				|| DefensiveValues.For(action.ID) != default
				|| action.Setting.SpecialType is not (SpecialActionType.None or SpecialActionType.MeleeRangedAttack)
				|| _byId.ContainsKey(action.ID))
			{
				continue;
			}

			var watched = new Watched(action);
			_watched.Add(watched);
			_byId[action.ID] = watched;
		}
	}
}
