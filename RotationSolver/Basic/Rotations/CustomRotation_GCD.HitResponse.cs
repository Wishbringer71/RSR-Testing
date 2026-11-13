using ECommons.ExcelServices;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// The job's heals over time against an enemy's damage over time, through <see cref="TryHotAgainstDot"/>. Among the
	/// healers only the White Mage and the Astrologian have them as a GCD; the Scholar's and the Sage's are abilities,
	/// cost no damage and stay with the heal thresholds (concept 08).
	/// </summary>
	/// <param name="act">The heal over time to use.</param>
	/// <returns>True when the job lays one.</returns>
	protected virtual bool HotAgainstDotByJob(out IAction? act)
	{
		act = null;
		return false;
	}

	/// <summary>
	/// Stops the cast of an enemy that is not a boss - an area attack reaching the party, any cast the game marks
	/// interruptible, or a cast on one target a sleep has stopped before (SleepInterruptWatch.IsCastWorthStopping) - while
	/// enough of it is left: first with the job's own stopper (<see cref="StopEnemyCastByJob"/>),
	/// otherwise with Sleep or Repose. The owner's experiment of 18.10.2026, "kann sleep der magiewirkenden klassen
	/// unterbrechen?", and his rules of 23.10.2026: the stop is the melee interrupt's counterpart, and "wenn holy stunnen
	/// kann, ist das vorzuziehen" (concept 08, "Ein gegnerisches Wirken stoppen"). Healers and casters together: the
	/// roles without an interrupt. What came of each sleep, and what it learns, is SleepInterruptWatch's.
	/// </summary>
	private bool StopEnemyCast(out IAction? act)
	{
		act = null;
		var role = DataCenter.Role;
		if (!Service.Config.SleepToStopAreaCasts || DataCenter.IsPvP || !DataCenter.InCombat
			|| role is not (JobRole.Healer or JobRole.RangedMagical))
		{
			return false;
		}

		// His rule: Swiftcast stays for raises, and a stop under it would spend it. Dualcast and Triplecast are
		// spent on whatever spell comes next, so a stop may take them.
		if (StatusHelper.PlayerHasStatus(true, StatusID.Swiftcast))
		{
			SleepInterruptWatch.NoteLapse(null, "Swiftcast stands, kept for a raise");
			return false;
		}

		// The time from the end of an own cast to its effect, as last measured for a sleep - before the first, the
		// measured round trip of a press.
		var lag = SleepInterruptWatch.Lag >= 0f ? SleepInterruptWatch.Lag : Math.Max(0f, TankbusterForecast.RoundTrip);

		// The casts worth stopping, the one that ends soonest first: the others leave time for the next GCD.
		List<(IBattleChara Enemy, float Left)> casts = [];
		foreach (var enemy in DataCenter.AllHostileTargets)
		{
			if (enemy != null && enemy.IsCasting && !enemy.IsBossFromIcon() && SleepInterruptWatch.IsCastWorthStopping(enemy))
			{
				casts.Add((enemy, enemy.TotalCastTime - enemy.CurrentCastTime));
			}
		}

		if (casts.Count == 0)
		{
			SleepInterruptWatch.NoteLapse(null, "the cast ended or was stopped");
			return false;
		}

		casts.Sort((a, b) => a.Left.CompareTo(b.Left));

		// A healer's GCD belongs to the member about to die.
		if (role == JobRole.Healer)
		{
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member != null && !member.IsDead && member.IsInCriticalClass())
				{
					SleepInterruptWatch.NoteLapse(null, "a party member in the critical class takes the GCD");
					return false;
				}
			}
		}

		var sleep = role == JobRole.Healer ? ReposePvE : SleepPvE;

		// What a sleep needs of the enemy's cast: its own cast and the GCD still running, the time to its effect, and
		// what has to be left of the cast when it lands, learned from sleeps that came too late.
		var needed = sleep.Info.NeededStatusMargin + lag + SleepInterruptWatch.LandingMargin;
		foreach (var (enemy, left) in casts)
		{
			if (StopEnemyCastByJob(enemy, left, lag, out act))
			{
				SleepInterruptWatch.NoteLapse(null, $"the job's own stopper went instead, at {enemy.Name.TextValue} ({act?.Name})");
				return true;
			}

			if (!sleep.EnoughLevel || left <= needed || !SleepInterruptWatch.IsOpenToSleep(enemy) || AreaReachesNewMob(enemy, sleep.Info.EffectRange))
			{
				SleepInterruptWatch.NoteLapse(enemy, left <= needed
					? $"too little time: {left:F1} s left, {needed:F1} s needed"
					: "the cast or the enemy is no longer one to sleep");
				continue;
			}

			// The stored locks - its kind resisted, the fight's sleeps reached the limit, a sleep did not stop the action -
			// hold in the game version that saw them. One from an earlier version is a hypothesis with one try in this one,
			// and only on a GCD no heal needs (concept 13, "Stufe 4", 4b).
			var locks = SleepInterruptWatch.LocksOn(enemy, _sleepTries);
			var busy = string.Empty;
			if (locks == LockState.Holds || (locks == LockState.TakenOver && !SleepTryAllowed(out busy)))
			{
				SleepInterruptWatch.NoteLapse(enemy, locks == LockState.Holds
					? "a stored lock holds: its kind resisted, the sleeps in this fight reached its limit, or a sleep did not stop the action"
					: $"a lock from an earlier game version is tried only on a GCD no heal needs, and {busy}");
				continue;
			}

			// The target filter is swapped in for this call only and restored afterwards, as for Nascent Flash.
			var setting = sleep.Setting;
			var canTarget = setting.CanTarget;
			var chosenId = enemy.GameObjectId;
			setting.CanTarget = t => canTarget(t) && t.GameObjectId == chosenId;
			try
			{
				if (!sleep.CanUse(out act, skipAoeCheck: true, skipTTKCheck: true))
				{
					SleepInterruptWatch.NoteLapse(enemy, DataCenter.IsMoving ? $"{sleep.Name} not castable: moving" : $"{sleep.Name} not castable now");
					continue;
				}
			}
			finally
			{
				setting.CanTarget = canTarget;
			}

			SleepInterruptWatch.AimAt(enemy, sleep, left, needed, _sleepTries);
			return true;
		}

		act = null;
		return false;
	}

	/// <summary>
	/// The job's own way to stop <paramref name="enemy"/>'s cast, tried before a sleep: a stun that reaches it in time
	/// (concept 08, "Ein gegnerisches Wirken stoppen"). Among the healers and casters only the White Mage has one.
	/// </summary>
	/// <param name="enemy">The casting enemy, not a boss.</param>
	/// <param name="left">Seconds left on its cast.</param>
	/// <param name="lag">Seconds from the end of an own cast to its effect, as measured.</param>
	/// <param name="act">The action that stops it.</param>
	/// <returns>True when the job stops the cast itself.</returns>
	protected virtual bool StopEnemyCastByJob(IBattleChara enemy, float left, float lag, out IAction? act)
	{
		act = null;
		return false;
	}

	/// <summary>
	/// Whether an area action of <paramref name="radius"/> around <paramref name="center"/> would reach an enemy that is
	/// not fighting yet while "Don't pull new enemies with AoE" is on - the setting's own test, a hostile without a
	/// target. A stop is an area action like any other, and the setting says "never".
	/// </summary>
	/// <param name="center">The centre of the area.</param>
	/// <param name="radius">Its radius, zero for a single target.</param>
	/// <returns>True when the setting forbids the action.</returns>
	protected static bool AreaReachesNewMob(IBattleChara center, float radius)
	{
		if (!Service.Config.NoNewHostiles || radius <= 0f || center == null)
		{
			return false;
		}

		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && hostile.TargetObject == null
				&& Vector3.Distance(hostile.Position, center.Position) - hostile.HitboxRadius <= radius)
			{
				return true;
			}
		}

		return false;
	}
}
