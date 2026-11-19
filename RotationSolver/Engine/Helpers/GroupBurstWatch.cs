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

	// The scan of one target's effect entries, without a closure per call: Observe runs on the game's thread only.
	private static bool _onMember;
	private static uint _found;
	private static readonly Action<EffectEntry> FindRaidStatus = static entry =>
	{
		if (_found == 0 && entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0
			&& (_onMember ? StatusHelper.RaisesDamageDealt((StatusID)entry.value) : StatusHelper.RaisesDamageTaken((StatusID)entry.value)))
		{
			_found = entry.value;
		}
	};
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
				// More than a GCD overdue: it did not come (a member died, or held it), and nothing should wait for it - as
				// BurstClock.SecondsToNext.
				var due = use.UsedAt + use.Recast - now;
				if (use.Recast > 0f && due >= -DataCenter.DefaultGCDTotal)
				{
					next = float.IsNaN(next) ? Math.Max(0f, due) : Math.Min(next, Math.Max(0f, due));
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
			_onMember = IsMember(effect.TargetID);
			_found = 0;
			effect.ForEach(FindRaidStatus);
			if (_found != 0)
			{
				// A status laid on another than the caster names the buff, so its own status (Embolden's 1239) does not.
				if (status == 0 || effect.TargetID != source.GameObjectId)
				{
					status = _found;
				}

				members += _onMember ? 1 : 0;
				onEnemy |= !_onMember;
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
		// A use is the caster's action where the sheet gives its recast; an action that is itself a GCD (the four Technical
		// Finishes) is the caster's status, so its uses meet across the finishes for the measured gap.
		var byAction = recast > DataCenter.DefaultGCDTotal;
		Use? known = null;
		foreach (var use in _uses)
		{
			if (use.Caster == source.GameObjectId && (byAction ? use.Action == action.RowId : use.Status == status))
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
		known.Recast = byAction ? recast : measured;
		known.UsedAt = now;
		known.Action = action.RowId;
		known.Status = status;

		var next = NextIn;
		var job = source.ClassJob.Value.Abbreviation.ExtractText();
		DefenseTrace.Line($"group burst: {(job.Length > 0 ? job : "NPC")} used {action.Name.ExtractText()}, status #{status} "
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
