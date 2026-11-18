using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 03, "Den Kampf eröffnen: Bereitschaftsanfrage und Countdown" (A879): RSR opens no fight while a ready check
/// waits for an answer and no countdown runs, whatever the engage setting or a priority rule allows; and when RSR itself
/// opens a fight out of combat, the trace says so, with what let it.
/// </summary>
internal static class PullGuard
{
	// Read once per frame: every enemy of every target choice asks while the party is out of combat.
	private static readonly FrameCached<int> _awaiting = new(CountAwaiting);

	private static bool _heldWritten;
	private static bool _openedWritten;

	/// <summary>
	/// How many members the open ready check still waits for (status <c>AwaitingResponse</c>); 0 with none open, and
	/// when the game's agent cannot be read - then no hold, as before this rule.
	/// </summary>
	internal static int Awaiting => _awaiting.Value;

	/// <summary>
	/// Whether attacking <paramref name="enemy"/> would open a fight while the party still answers a ready check: out of
	/// combat, outside PvP, no countdown running, and the enemy not fighting yet (<see cref="DataCenter.NotFightingYet"/>).
	/// </summary>
	internal static bool HoldsPull(IBattleChara enemy)
	{
		if (DataCenter.InCombat || DataCenter.IsPvP || Service.CountDownTime > 0 || !DataCenter.NotFightingYet(enemy))
		{
			return false;
		}

		var awaiting = Awaiting;
		if (awaiting == 0)
		{
			_heldWritten = false;
			return false;
		}

		if (!_heldWritten)
		{
			_heldWritten = true;
			DefenseTrace.Line($"pull: held - ready check open, {awaiting} not answered yet, no countdown; {enemy.Name} is not fighting");
		}

		return true;
	}

	/// <summary>
	/// RSR's own action <paramref name="action"/> just went out. Out of combat on an enemy not fighting yet it opened the
	/// fight: one line until the next fight starts, with the engage setting, the countdown, the ready check and whether a
	/// priority rule chose the enemy - the line that tells an opening by RSR from one by the player's own press.
	/// </summary>
	internal static void NoteOwnAction(IAction action)
	{
		if (DataCenter.InCombat)
		{
			_openedWritten = false;
			return;
		}

		if (_openedWritten || action is not IBaseAction act || OpenedOn(act.Target) is not { } target)
		{
			return;
		}

		_openedWritten = true;
		var engage = DataCenter.ActiveIpcOverrides?.HostileType is { } overridden
			? $"{overridden} (set over IPC)"
			: Service.Config.HostileType.ToString();
		var awaiting = Awaiting;
		DefenseTrace.Line($"pull: RSR opened the fight with {act.Name} #{act.Info.ID} on {target.Name}, not fighting yet | engage {engage}"
			+ $" | countdown {Service.CountDownTime:F1} s | ready check {(awaiting > 0 ? $"{awaiting} not answered" : "none")}"
			+ $" | party {DataCenter.PartyMembers.Count} | priority rule {target.IsTopPriorityHostile()}");
	}

	// The enemy not fighting yet that the action reaches: its target, or one in its area (an area around the player has
	// the player as target).
	private static IBattleChara? OpenedOn(TargetResult result)
	{
		if (result.Target is IBattleChara target && target.IsEnemy() && DataCenter.NotFightingYet(target))
		{
			return target;
		}

		if (result.AffectedTargets != null)
		{
			foreach (var affected in result.AffectedTargets)
			{
				if (affected != null && affected.IsEnemy() && DataCenter.NotFightingYet(affected))
				{
					return affected;
				}
			}
		}

		return null;
	}

	private static unsafe int CountAwaiting()
	{
		try
		{
			var agent = AgentReadyCheck.Instance();
			if (agent == null)
			{
				return 0;
			}

			var count = 0;
			foreach (ref readonly var entry in agent->ReadyCheckEntries)
			{
				if (entry.Status == ReadyCheckStatus.AwaitingResponse)
				{
					count++;
				}
			}

			return count;
		}
		catch (Exception)
		{
			// Reported once by ExceptionTrace; without a readable agent the rule does not hold (the behaviour before it).
			return 0;
		}
	}
}
