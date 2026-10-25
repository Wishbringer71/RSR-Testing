namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Which enemies are moving: an enemy whose position changed within the player's last GCD, or that has not been seen
/// long enough to tell. The game gives no such flag for an enemy - only for the player (AgentMap) - so it is read from
/// the positions the client shows (concept 04, "Schadensflächen erst, wenn die Gegner stehen", A350).
/// </summary>
/// <remarks>
/// A figure that stands keeps its position to the last digit, so any change is movement, with no distance to choose.
/// Still means still for a GCD: a tank who pauses between two packs of a wall-to-wall pull has not stopped, and the
/// next slot an area could take comes a GCD later anyway. It also makes the reading independent of how often the client
/// moves a running figure's drawn position. An enemy seen for the first time counts as moving. Updated by the target
/// update on the game thread, read there by the rotation.
/// </remarks>
internal static class HostileMovement
{
	private static Dictionary<ulong, (Vector3 At, DateTime Moved)> _seen = [];
	private static Dictionary<ulong, (Vector3 At, DateTime Moved)> _next = [];

	/// <summary>Takes the positions of this update's enemies.</summary>
	internal static void Update(List<IBattleChara> hostiles)
	{
		var now = DateTime.Now;
		_next.Clear();
		foreach (var enemy in hostiles)
		{
			if (enemy == null)
			{
				continue;
			}

			var id = enemy.GameObjectId;
			var at = enemy.Position;
			_next[id] = _seen.TryGetValue(id, out var was) && was.At == at ? was : (at, now);
		}

		(_seen, _next) = (_next, _seen);
	}

	/// <summary>Forgets every enemy, when there are none.</summary>
	internal static void Clear()
	{
		_seen.Clear();
		_next.Clear();
	}

	/// <summary>Whether <paramref name="enemy"/> moved within the player's last GCD, or has not been seen that long.</summary>
	internal static bool IsMoving(IBattleChara enemy)
	{
		return !_seen.TryGetValue(enemy.GameObjectId, out var seen)
			|| (DateTime.Now - seen.Moved).TotalSeconds < DataCenter.DefaultGCDTotal;
	}
}
