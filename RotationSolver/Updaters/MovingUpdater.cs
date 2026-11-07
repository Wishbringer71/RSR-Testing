using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using RotationSolver.Commands;

namespace RotationSolver.Updaters;

internal static class MovingUpdater
{
	/// <summary>
	/// Sets the game's movement lock for "Lock movement while casting": during a spell, during a ticked channel and before
	/// RSR presses either - each time unless <see cref="MovementLock"/> says it yields (concept 03, "Bewegungssperre beim
	/// Wirken und die KI von BossMod"). The option itself and the player's unlock keys act in <see cref="Service.CanMove"/>.
	/// </summary>
	internal static unsafe void UpdateCanMove(bool doNextAction)
	{
		if (Player.Object == null)
		{
			return;
		}

		if (Svc.Condition?[ConditionFlag.OccupiedInEvent] == true)
		{
			Service.CanMove = true;
			return;
		}

		string reason;
		bool yields;
		if (Svc.Condition?[ConditionFlag.Casting] == true)
		{
			var castId = Player.Object?.CastActionId ?? 0;
			if (ActionBasicInfo.ActionsNoNeedCasting.Contains(castId))
			{
				Service.CanMove = true;
				return;
			}

			yields = MovementLock.CastYields(out reason);
			Settle(yields, reason, $"spell #{castId}");
			return;
		}

		if (MovementLock.ActiveTickedChannel(out var channel, out var left))
		{
			yields = MovementLock.ChannelYields(channel, left, out reason);
			Settle(yields, reason, $"{channel.Action} ({left:F1} s left)");
			return;
		}

		ActionID action;
		if (DateTime.Now - RSCommands._lastUsedTime < TimeSpan.FromMilliseconds(100f))
		{
			action = (ActionID)RSCommands._lastActionID;
		}
		else if (doNextAction)
		{
			action = (ActionID)(ActionUpdater.NextAction?.AdjustedID ?? 0);
		}
		else
		{
			action = 0;
		}

		var castTime = action.GetCastTime();
		if (castTime > 0f)
		{
			yields = MovementLock.PreCastYields(castTime, out reason);
			Settle(yields, reason, $"the press of {action}");
			return;
		}

		if (MovementLock.IsTickedChannelPress(action))
		{
			Settle(false, string.Empty, $"the press of {action}");
			return;
		}

		MovementLock.Note(false, false, string.Empty, string.Empty);
		Service.CanMove = true;
	}

	// The lock holds unless it yields; the trace notes a release only while the option is on and not unlocked by hand.
	private static void Settle(bool yields, string reason, string what)
	{
		Service.CanMove = yields;
		var lockWanted = !DataCenter.NoPoslock;
		MovementLock.Note(lockWanted, held: lockWanted && !yields, reason, what);
	}
}
