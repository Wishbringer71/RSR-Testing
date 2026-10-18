using System.Collections.Concurrent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What a Sleep or Repose aimed at an enemy's area cast did, as the fight shows it: whether it landed, whether
/// the cast stopped or went on, and how long a sleep takes from the press to its effect. The experiment the owner
/// asked for on 18.10.2026 (concept 08, "Schlaf gegen ein Flächenwirken"): can Sleep stop enemy area attacks, and
/// which?
/// </summary>
/// <remarks>
/// <para>No game text says that Sleep interrupts a cast. Its status (row 3) locks actions and movement as Stun
/// (row 2) does, and a stun stops the cast of an enemy that is not immune (the tree's Low Blow rule); community
/// sources say the same of Sleep (Final Fantasy Wiki: "Sleep can potentially be used to interrupt casting if the
/// target is vulnerable to the status"). Whether it holds, and for which casts, this watch measures.</para>
///
/// <para>It learns, for the session: an enemy kind that resisted (by its base id) is not tried again; an action
/// that went on although the sleep had landed is not tried again; the time from the press to the effect sets how
/// much of the enemy's cast a sleep needs. Each outcome is a line in the defence trace.</para>
///
/// <para>Recognition and its own measure only; the rule that presses the sleep decides
/// (<c>CustomRotation.SleepToStopAreaCast</c>).</para>
/// </remarks>
public static class SleepInterruptWatch
{
	/// <summary>One target of the player's Sleep or Repose.</summary>
	/// <param name="Target">The target's object id.</param>
	/// <param name="Applied">A status was put on it.</param>
	/// <param name="Resisted">The game reported it as fully resisted or the target as invulnerable.</param>
	internal readonly record struct Observation(ulong Target, bool Applied, bool Resisted);

	private sealed record Aim(ulong EnemyId, string EnemyName, uint BaseId, uint ActionId, string ActionName, DateTime CastEnd);

	private sealed class Watch
	{
		public required Aim Aim;
		public DateTime AppliedAt;
		public DateTime ExpectedEnd;
		public bool CastGone;
		public DateTime CastGoneAt;
	}

	// Enemy kinds that resisted a sleep, and actions a landed sleep did not stop.
	private static readonly ConcurrentDictionary<uint, byte> _immuneKinds = new();
	private static readonly ConcurrentDictionary<uint, byte> _notStopped = new();

	private static Aim? _aim;
	private static Watch? _watch;
	private static uint _pressedAction;
	private static DateTime _pressedAt;
	private static float _pressedCastTime;

	/// <summary>
	/// Seconds from the end of the sleep's own cast to its effect on the server's answer, last measured; negative
	/// before the first measurement.
	/// </summary>
	public static float Lag { get; private set; } = -1f;

	/// <summary>Whether <paramref name="actionId"/> is the casters' Sleep or the healers' Repose.</summary>
	/// <param name="actionId">The action.</param>
	/// <returns>True for either.</returns>
	public static bool IsSleepAction(uint actionId)
		=> actionId is (uint)ActionID.SleepPvE or (uint)ActionID.ReposePvE;

	/// <summary>
	/// Whether a sleep could still land on <paramref name="enemy"/>: not a boss (BNpcBase rank), no Sleep Resistance
	/// (row 1354, "Immune to sleep effects"), and its kind has not resisted one this session.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True while a sleep can still land.</returns>
	public static bool CanStillBeSlept(IBattleChara enemy)
	{
		return enemy != null && !enemy.IsBossFromIcon() && !enemy.HasStatus(false, StatusID.SleepResistance)
			&& !_immuneKinds.ContainsKey(enemy.BaseId);
	}

	/// <summary>
	/// Whether <paramref name="enemy"/> is casting an area attack that reaches the party - the player or two living
	/// members, by the shape of the action - and that a landed sleep has not failed to stop this session.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True for such a cast.</returns>
	public static bool IsAreaCastToStop(IBattleChara enemy)
	{
		if (enemy == null || !enemy.IsCasting || enemy.CastActionId == 0 || _notStopped.ContainsKey(enemy.CastActionId))
		{
			return false;
		}

		var act = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRow(enemy.CastActionId);
		if (act.RowId == 0)
		{
			return false;
		}

		var shape = (CastType)act.CastType;
		if (act.EffectRange == 0 && shape is CastType.Targeted or CastType.Unknown)
		{
			return false;
		}

		return DataCenter.AreaCastReachesParty(enemy, act);
	}

	/// <summary>
	/// The rule chose a sleep for this enemy's cast. Written to the trace once per cast; the effect of the sleep is
	/// matched against it.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <param name="sleep">The sleep chosen.</param>
	/// <param name="remaining">Seconds left on its cast.</param>
	/// <param name="needed">Seconds the sleep needs to land, as the rule reckoned them.</param>
	internal static void AimAt(IBattleChara enemy, IBaseAction sleep, float remaining, float needed)
	{
		var actionId = enemy.CastActionId;
		if (_aim is { } aim && aim.EnemyId == enemy.GameObjectId && aim.ActionId == actionId)
		{
			return;
		}

		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRow(actionId).Name.ExtractText();
		_aim = new Aim(enemy.GameObjectId, enemy.Name.TextValue, enemy.BaseId, actionId, name, DateTime.Now.AddSeconds(remaining));
		DefenseTrace.Line($"sleep to stop a cast -> {sleep.Name} #{sleep.ID} on {enemy.Name.TextValue} (kind {enemy.BaseId}), casting {name} #{actionId}"
			+ $", {remaining:F1} s left, interruptible {enemy.IsCastInterruptible}, needs {needed:F1} s");
	}

	/// <summary>The player pressed a sleep: the start of the time to its effect. Called from <c>BaseAction.Use</c>.</summary>
	/// <param name="actionId">The action pressed.</param>
	/// <param name="castTime">Its cast time at the press, zero when instant.</param>
	internal static void RecordPress(uint actionId, float castTime)
	{
		if (!IsSleepAction(actionId))
		{
			return;
		}

		_pressedAction = actionId;
		_pressedAt = DateTime.Now;
		_pressedCastTime = castTime;
	}

	/// <summary>
	/// The effect of the player's Sleep or Repose arrived: measures its lag, and for the enemy it was aimed at
	/// learns immunity or starts watching the cast. Called by the effect handler on the game thread.
	/// </summary>
	/// <param name="actionId">The sleep action.</param>
	/// <param name="targets">Its targets.</param>
	internal static void ObserveOwnSleep(uint actionId, IReadOnlyList<Observation> targets)
	{
		// One GCD past the cast for its effect to arrive, as everywhere a cast's effect is timed.
		var now = DateTime.Now;
		if (_pressedAction == actionId && (now - _pressedAt).TotalSeconds <= _pressedCastTime + DataCenter.DefaultGCDTotal)
		{
			Lag = Math.Max(0f, (float)(now - _pressedAt).TotalSeconds - _pressedCastTime);
			_pressedAction = 0;
		}

		if (_aim is not { } aim)
		{
			return;
		}

		_aim = null;
		Observation? found = null;
		for (var i = 0; i < targets.Count; i++)
		{
			if (targets[i].Target == aim.EnemyId)
			{
				found = targets[i];
				break;
			}
		}

		if (found is not { } hit || (!hit.Applied && !hit.Resisted))
		{
			DefenseTrace.Line($"sleep reached no effect on {aim.EnemyName} (casting {aim.ActionName} #{aim.ActionId})");
			return;
		}

		if (!hit.Applied)
		{
			_immuneKinds[aim.BaseId] = 0;
			DefenseTrace.Line($"sleep resisted by {aim.EnemyName} (kind {aim.BaseId}): immune, its kind is not tried again");
			return;
		}

		var enemy = ECommons.DalamudServices.Svc.Objects.SearchById(aim.EnemyId) as IBattleChara;
		if (enemy == null || !enemy.IsCasting || enemy.CastActionId != aim.ActionId)
		{
			DefenseTrace.Line($"sleep landed on {aim.EnemyName} after {aim.ActionName} #{aim.ActionId} had ended - gone off, interrupted by someone else or the enemy fell"
				+ $" (lag {Lag:F2} s)");
			return;
		}

		var left = enemy.TotalCastTime - enemy.CurrentCastTime;
		_watch = new Watch { Aim = aim, AppliedAt = now, ExpectedEnd = now.AddSeconds(left) };
		DefenseTrace.Line($"sleep landed on {aim.EnemyName} while it cast {aim.ActionName} #{aim.ActionId}, {left:F1} s left (lag {Lag:F2} s)");
	}

	/// <summary>
	/// An enemy's action resolved: if it is the watched cast, the sleep did not stop it. Called by the effect
	/// handler for every set from the enemy side, on the game thread.
	/// </summary>
	/// <param name="sourceId">The enemy.</param>
	/// <param name="actionId">Its action.</param>
	internal static void ObserveEnemyAction(ulong sourceId, uint actionId)
	{
		if (_watch is not { } watch || watch.Aim.EnemyId != sourceId || watch.Aim.ActionId != actionId)
		{
			return;
		}

		_watch = null;
		_notStopped[actionId] = 0;
		DefenseTrace.Line($"sleep did not stop {watch.Aim.ActionName} #{actionId} of {watch.Aim.EnemyName}: it went off"
			+ $" {(DateTime.Now - watch.AppliedAt).TotalSeconds:F1} s after the sleep landed; not tried on it again");
	}

	/// <summary>Settles the watched cast. Called once per framework cycle.</summary>
	internal static void Update()
	{
		if (_watch is not { } watch)
		{
			// An aim no sleep answered: past the cast's end and one GCD for an effect to arrive, or past the cast of
			// a sleep pressed after it.
			if (_aim is { } aim)
			{
				var until = aim.CastEnd;
				if (_pressedAction != 0 && _pressedAt.AddSeconds(_pressedCastTime) > until)
				{
					until = _pressedAt.AddSeconds(_pressedCastTime);
				}

				if (DateTime.Now > until.AddSeconds(DataCenter.DefaultGCDTotal))
				{
					_aim = null;
				}
			}

			return;
		}

		var now = DateTime.Now;
		var enemy = ECommons.DalamudServices.Svc.Objects.SearchById(watch.Aim.EnemyId) as IBattleChara;
		if (enemy == null || enemy.IsDead || enemy.CurrentHp == 0)
		{
			_watch = null;
			DefenseTrace.Line($"sleep on {watch.Aim.EnemyName}: it fell before {watch.Aim.ActionName} #{watch.Aim.ActionId} could resolve - no answer");
			return;
		}

		if (!watch.CastGone)
		{
			if (enemy.IsCasting && enemy.CastActionId == watch.Aim.ActionId)
			{
				return;
			}

			watch.CastGone = true;
			watch.CastGoneAt = now;

			// Ended before its time by more than the measured round trip and the last frame - what a natural end can
			// be off by on the client: the cast was cut off. A stun that landed at the same time could be the cause as
			// well; then the answer stays open.
			var early = Math.Max(0f, TankbusterForecast.RoundTrip) + ECommons.DalamudServices.Svc.Framework.UpdateDelta.TotalSeconds;
			if ((watch.ExpectedEnd - now).TotalSeconds > early)
			{
				_watch = null;
				if (StunWatch.StunLeft(enemy) > 0f)
				{
					DefenseTrace.Line($"{watch.Aim.ActionName} #{watch.Aim.ActionId} of {watch.Aim.EnemyName} stopped "
						+ $"{(watch.ExpectedEnd - now).TotalSeconds:F1} s early, but a stun landed on it too - not counted");
					return;
				}

				DefenseTrace.Line($"sleep stopped {watch.Aim.ActionName} #{watch.Aim.ActionId} of {watch.Aim.EnemyName}: "
					+ $"the cast ended {(watch.ExpectedEnd - now).TotalSeconds:F1} s early");
				return;
			}
		}

		// Ended at its time: whether it went off shows in its effect, which arrives after the cast bar.
		if ((now - watch.CastGoneAt).TotalSeconds > DataCenter.DefaultGCDTotal)
		{
			_watch = null;
			DefenseTrace.Line($"sleep on {watch.Aim.EnemyName}: {watch.Aim.ActionName} #{watch.Aim.ActionId} ended at its time with no effect seen - no answer");
		}
	}
}
