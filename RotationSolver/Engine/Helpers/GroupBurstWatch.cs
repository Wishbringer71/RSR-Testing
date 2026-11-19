using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 03, "F · Über den Augenblick hinaus", K3 (A904): when the group's raid buffs come, read from the other members'
/// actions - an action that lays a raid buff's status (<see cref="StatusHelper.RaisesDamageDealt"/>) on more members than
/// its caster and one other, or one that raises the damage an enemy takes (<see cref="StatusHelper.RaisesDamageTaken"/>).
/// It comes back after its recast from the action sheet; an action that is itself a GCD (Technical Finish, whose recast
/// Technical Step carries) after the measured gap between the caster's last two uses of the same status. A first step: it
/// observes and traces; nothing waits on it yet (Gegenthese 6).
/// </summary>
internal static class GroupBurstWatch
{
	private sealed class Use
	{
		internal ulong Caster;
		internal uint Status;
		internal uint Action;
		internal float UsedAt;
		internal float Recast;
	}

	private static readonly List<Use> _uses = [];
	private static float _lastPullSeen = float.NaN;
	private static float _combatSeen = float.MaxValue;

	/// <summary>
	/// Seconds until the earliest raid buff of the group observed in this fight comes back; NaN while none is observed or
	/// none has a known recast.
	/// </summary>
	internal static float NextIn
	{
		get
		{
			Sync();
			var now = DataCenter.CombatTimeRaw;
			var next = float.NaN;
			foreach (var use in _uses)
			{
				if (use.Recast > 0f)
				{
					var due = Math.Max(0f, use.UsedAt + use.Recast - now);
					next = float.IsNaN(next) ? due : Math.Min(next, due);
				}
			}

			return next;
		}
	}

	/// <summary>Every action effect: a raid buff of another member of the party, in combat.</summary>
	internal static void Observe(ActionEffectSet set, ulong playerId)
	{
		if (!DataCenter.InCombat || set.Action is not { } action || set.Source is not IBattleChara source
			|| source.GameObjectId == playerId || !IsMember(source.GameObjectId))
		{
			return;
		}

		uint status = 0;
		var members = 0;
		var onEnemy = false;
		foreach (var effect in set.TargetEffects)
		{
			var member = IsMember(effect.TargetID);
			var found = 0u;
			effect.ForEach(entry =>
			{
				if (found == 0 && entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0
					&& (member ? StatusHelper.RaisesDamageDealt((StatusID)entry.value) : StatusHelper.RaisesDamageTaken((StatusID)entry.value)))
				{
					found = entry.value;
				}
			});

			if (found != 0)
			{
				status = found;
				members += member ? 1 : 0;
				onEnemy |= !member;
			}
		}

		// More than the caster and one other: Standard Finish reaches the dancer and the partner, a card one member.
		if (status == 0 || (!onEnemy && members <= 2))
		{
			return;
		}

		Sync();
		var now = DataCenter.CombatTimeRaw;
		var recast = action.Recast100ms / 10f;
		Use? known = null;
		foreach (var use in _uses)
		{
			if (use.Caster == source.GameObjectId && use.Status == status)
			{
				known = use;
			}
		}

		if (known == null)
		{
			known = new Use { Caster = source.GameObjectId, Status = status, Action = action.RowId, UsedAt = float.NaN };
			_uses.Add(known);
		}

		// The action's own recast where it is more than a GCD; else the measured gap since the last use.
		var measured = !float.IsNaN(known.UsedAt) ? now - known.UsedAt : 0f;
		known.Recast = recast > DataCenter.DefaultGCDTotal ? recast : measured;
		known.UsedAt = now;
		known.Action = action.RowId;

		var next = NextIn;
		DefenseTrace.Line($"group burst: {source.ClassJob.Value.Abbreviation.ExtractText()} used {action.Name.ExtractText()}, status #{status} "
			+ (onEnemy ? "on the enemy" : $"on {members} members")
			+ $", recast {(known.Recast > 0f ? $"{known.Recast:F0} s" : "unknown")} - next group buff in "
			+ (float.IsNaN(next) ? "unknown" : $"{next:F0} s"));
	}

	private static bool IsMember(ulong id)
	{
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member.GameObjectId == id)
			{
				return true;
			}
		}

		return false;
	}

	// A new fight starts the list anew: a lower combat time or another last pull (as BurstClock).
	private static void Sync()
	{
		var now = DataCenter.InCombat ? DataCenter.CombatTimeRaw : 0f;
		if (now < _combatSeen || DataCenter.LastPullSeconds != _lastPullSeen)
		{
			_lastPullSeen = DataCenter.LastPullSeconds;
			_uses.Clear();
		}

		_combatSeen = now;
	}
}
