using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Updaters;

internal static partial class TargetUpdater
{
	// Per enemy the provoke waits for: the member it turned to, and since when (A500). Per enemy since every action chooses
	// among all of them (A612); one shared record would restart with each enemy read and never see a strike.
	private static readonly Dictionary<ulong, (ulong Member, DateTime Since)> _provokeTurns = [];

	/// <summary>
	/// Whether the provoke on <paramref name="enemy"/> must go now, whatever its delay says: the enemy attacks a tank (the
	/// provoke takes it from a co-tank, where waiting risks him) or nobody, or it has already struck the member it turned
	/// to since it turned.
	/// </summary>
	private static bool ProvokeCannotWait(IBattleChara enemy)
	{
		if (Svc.Objects.SearchById(enemy.TargetObjectId) is not IBattleChara member || member.IsJobCategory(JobRole.Tank))
		{
			return true;
		}

		if (!_provokeTurns.TryGetValue(enemy.GameObjectId, out var turn) || turn.Member != member.GameObjectId)
		{
			turn = (member.GameObjectId, DateTime.Now);
			_provokeTurns[enemy.GameObjectId] = turn;
		}

		return DataCenter.LastStrikeByEnemy.TryGetValue(enemy.GameObjectId, out var strike)
			&& strike.Target == member.GameObjectId && strike.At >= turn.Since;
	}

	// Keeps the turn of each enemy that still qualifies for a provoke and drops the others, so an enemy that qualifies
	// again later starts a new turn.
	private static void ForgetProvokeTurns()
	{
		if (_provokeTurns.Count == 0)
		{
			return;
		}

		List<ulong>? gone = null;
		foreach (var id in _provokeTurns.Keys)
		{
			var stillQualifies = false;
			foreach (var enemy in _provokeQualified)
			{
				if (enemy.GameObjectId == id)
				{
					stillQualifies = true;
					break;
				}
			}

			if (!stillQualifies)
			{
				(gone ??= []).Add(id);
			}
		}

		if (gone != null)
		{
			foreach (var id in gone)
			{
				_ = _provokeTurns.Remove(id);
			}
		}
	}

	// The qualifying enemies that may be acted on now: all of them once the delay has run, otherwise those that cannot wait.
	private static void FillReady(List<IBattleChara> ready, List<IBattleChara> qualified, bool delayRunOut,
		Func<IBattleChara, bool> cannotWait)
	{
		ready.Clear();
		foreach (var enemy in qualified)
		{
			if (delayRunOut || cannotWait(enemy))
			{
				ready.Add(enemy);
			}
		}
	}
}
