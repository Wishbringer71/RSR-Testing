using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 03, "Dynamische Rotation: geht eine Aktion nicht, kommt eine Alternative" (A930), D1: an action the game refused
/// when pressed, with a reason it names for that action and target, is left out of the choice until the game takes it
/// again - asked of the game only for the actions it refused, so nothing is asked per frame otherwise. Without a reason (the
/// game takes it when asked again) nothing is held: a matter of timing, not of the action.
/// </summary>
internal static class DynamicRotation
{
	private static readonly Dictionary<(uint Action, ulong Target), uint> _refused = [];
	// Concept 03, D1 (A957): a refusal without a reason is a matter of timing; written once per action, target and fight.
	private static readonly HashSet<(uint Action, ulong Target)> _tracedWithoutReason = [];

	/// <summary>The game said no to <paramref name="action"/> pressed on its target.</summary>
	internal static unsafe void PressFailed(IBaseAction action)
	{
		var manager = ActionManager.Instance();
		if (manager == null || action.TargetInfo.IsTargetArea || Player.Object is not { } player)
		{
			return;
		}

		var key = (action.AdjustedID, action.Target.Target?.GameObjectId ?? player.GameObjectId);
		var status = manager->GetActionStatus(ActionType.Action, key.AdjustedID, key.Item2, false, false);
		if (status == 0)
		{
			if (_tracedWithoutReason.Add(key))
			{
				DefenseTrace.Line($"dynamic rotation: {action.Name} refused by the game without a reason - not left out");
			}

			return;
		}

		if (_refused.TryAdd(key, status))
		{
			DefenseTrace.Line($"dynamic rotation: {action.Name} refused by the game on {action.Target.Target?.Name.TextValue ?? "you"} "
				+ $"({(ActionStatus)status}) - left out until the game takes it");
		}
	}

	/// <summary>
	/// Whether <paramref name="action"/> on <paramref name="targetId"/> is still left out: the game refused it and names a
	/// reason still. Once it names none, the action is taken again.
	/// </summary>
	internal static unsafe bool StillRefused(IBaseAction action, ulong targetId)
	{
		if (_refused.Count == 0)
		{
			return false;
		}

		var key = (action.AdjustedID, targetId);
		if (!_refused.ContainsKey(key))
		{
			return false;
		}

		var manager = ActionManager.Instance();
		if (manager != null && manager->GetActionStatus(ActionType.Action, key.AdjustedID, targetId, false, false) != 0)
		{
			return true;
		}

		_ = _refused.Remove(key);
		DefenseTrace.Line($"dynamic rotation: {action.Name} taken again");
		return false;
	}

	/// <summary>Another area: what the game refused here says nothing there.</summary>
	internal static void AreaChanged()
	{
		_refused.Clear();
		_tracedWithoutReason.Clear();
	}

	/// <summary>A fight ended: the next one writes its refusals without a reason again.</summary>
	internal static void FightEnded()
	{
		_tracedWithoutReason.Clear();
	}
}
