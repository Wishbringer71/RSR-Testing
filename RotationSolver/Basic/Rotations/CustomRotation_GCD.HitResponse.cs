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
	/// Concept 23, "Gegnerkontrolle" (A834, A836): in a deep dungeon, a healer or caster whose party two or more awake enemies
	/// attack puts one that is not fought (the player's target, a party tank's) to sleep - not one about to die, not one taking damage over time, not one a sleep
	/// cannot reach - so the player fights one at a time. Not while a party member is about to die (a healer's GCD is
	/// theirs) or Swiftcast stands (kept for raises). The sleeper is spared by RSR's areas and attacked last; whether its
	/// kind sleeps is learned by SleepInterruptWatch from the game's answer.
	/// </summary>
	private bool SleepFurtherAttacker(out IAction? act)
	{
		act = null;
		var role = DataCenter.Role;
		if (!DeepDungeonCrowd.Enabled || !DataCenter.InCombat || role is not (JobRole.Healer or JobRole.RangedMagical)
			|| Player == null || StatusHelper.PlayerHasStatus(true, StatusID.Swiftcast))
		{
			return false;
		}

		if (role == JobRole.Healer && DeepDungeonCrowd.PartyInDanger())
		{
			return false;
		}

		var sleep = role == JobRole.Healer ? ReposePvE : SleepPvE;
		if (!sleep.EnoughLevel)
		{
			return false;
		}

		// G5: a sleeper whose sleep would run out before a sleep cast at the next GCD could land is renewed now - not
		// earlier, a renewal holds shorter than the sleep it replaces. Only while an awake enemy attacks the party; without
		// one the sleeper is the next target anyway.
		var lag = SleepInterruptWatch.Lag >= 0f ? SleepInterruptWatch.Lag : Math.Max(0f, TankbusterForecast.RoundTrip);
		var renewBy = sleep.Info.NeededStatusMargin + lag + DataCenter.DefaultGCDTotal;
		IBattleChara? renew = null;
		var renewLeft = float.MaxValue;
		if (DeepDungeonCrowd.Sleepers.Count > 0 && AnyAwakeAttacker())
		{
			foreach (var sleeper in DeepDungeonCrowd.Sleepers)
			{
				if (sleeper == null || sleeper.IsDead)
				{
					continue;
				}

				var left = sleeper.StatusTime(false, StatusID.Sleep);
				// On an equal rest the lower id, as for a first sleep below: one sleep of the caster's lands on several at once.
				if (left <= 0f || left > renewBy || left > renewLeft
					|| (left == renewLeft && renew != null && sleeper.GameObjectId > renew.GameObjectId) || !CanSleep(sleeper, sleep))
				{
					continue;
				}

				renew = sleeper;
				renewLeft = left;
			}
		}

		if (renew != null)
		{
			if (!SleepOn(renew, sleep, out act))
			{
				return false;
			}

			DeepDungeonCrowd.NoteChosen(renew, sleep, $"renewed, {renewLeft:F1} s of its sleep left");
			return true;
		}

		// The awake enemies attacking the party; of those not fought (the player's target, a tank's), the one with the most
		// HP left sleeps first. The tank takes the next sleeper once the other attackers are dead (his precision, A836).
		// A sleep with an area (the caster's, G6) goes where it puts the most attackers to sleep that stay asleep - the
		// middle of a pack -, an area that would shorten a running sleep counting against it; HP and id decide only then.
		var radius = sleep.Info.EffectRange;
		var awake = 0;
		IBattleChara? chosen = null;
		int chosenStays = 0, chosenShortened = 0;
		foreach (var enemy in DataCenter.AllHostileTargets)
		{
			if (enemy == null || enemy.IsDead || !DeepDungeonCrowd.AttacksParty(enemy) || enemy.HasStatus(false, StatusID.Sleep))
			{
				continue;
			}

			awake++;
			if (!StaysAsleep(enemy) || !CanSleep(enemy, sleep))
			{
				continue;
			}

			int stays = 0, shortened = 0;
			if (radius > 0f)
			{
				SleepArea(enemy, radius, out stays, out shortened);
			}

			// On equal HP the lower id: the list's order changes from frame to frame, and enemies of one kind arrive with
			// the same HP - without a fixed order the choice flipped every frame (upload of 16.11.2026).
			if (chosen == null || stays > chosenStays
				|| (stays == chosenStays && (shortened < chosenShortened
					|| (shortened == chosenShortened && (enemy.CurrentHp > chosen.CurrentHp
						|| (enemy.CurrentHp == chosen.CurrentHp && enemy.GameObjectId < chosen.GameObjectId))))))
			{
				chosen = enemy;
				chosenStays = stays;
				chosenShortened = shortened;
			}
		}

		// The chosen one and at least one more awake attacker - otherwise there is nobody to fight meanwhile.
		if (chosen == null || awake <= 1 || !SleepOn(chosen, sleep, out act))
		{
			return false;
		}

		DeepDungeonCrowd.NoteChosen(chosen, sleep, radius > 0f
			? $"{awake} awake enemies attack the party, {chosenStays - 1} more asleep in its area, {chosenShortened} sleep(s) shortened"
			: $"{awake} awake enemies attack the party");
		return true;
	}

	// Whether a sleep on this awake attacker holds: not fought (the next attack wakes it), not about to die, no damage over
	// time (its next tick wakes it), and its kind can still be slept.
	private static bool StaysAsleep(IBattleChara enemy)
	{
		return !DeepDungeonCrowd.IsFought(enemy) && !enemy.IsDying() && !DeepDungeonCrowd.TakesDamageOverTime(enemy)
			&& SleepInterruptWatch.CanStillBeSlept(enemy);
	}

	// What a sleep with an area on this centre does: how many awake attackers in it, the centre included, fall asleep and stay
	// so, and how many sleepers in it it would shorten. Measured as the area check of CanSleep measures, to the hitbox.
	private static void SleepArea(IBattleChara centre, float radius, out int stays, out int shortened)
	{
		stays = 0;
		shortened = 0;
		foreach (var other in DataCenter.AllHostileTargets)
		{
			if (other == null || other.IsDead || Vector3.Distance(other.Position, centre.Position) - other.HitboxRadius > radius)
			{
				continue;
			}

			if (other.HasStatus(false, StatusID.Sleep))
			{
				shortened++;
			}
			else if (DeepDungeonCrowd.AttacksParty(other) && StaysAsleep(other))
			{
				stays++;
			}
		}
	}

	// Whether an awake enemy attacks the party: the fight a sleeper is kept out of goes on.
	private static bool AnyAwakeAttacker()
	{
		foreach (var enemy in DataCenter.AllHostileTargets)
		{
			if (enemy != null && !enemy.IsDead && DeepDungeonCrowd.AttacksParty(enemy) && !enemy.HasStatus(false, StatusID.Sleep))
			{
				return true;
			}
		}

		return false;
	}

	// What bars a sleep on this enemy, first or renewed: a sleep of the player's on it not shown yet, its kind or the
	// sleeps it took this fight (SleepInterruptWatch), and in the casters' area an enemy not fighting yet or one no attack
	// may hit other than a sleeper - the rule target skips the action's own check of that area. The fought target in that
	// area sleeps along and wakes at the next attack (his hint, 16.11.2026, A836).
	private static bool CanSleep(IBattleChara enemy, IBaseAction sleep)
	{
		var radius = sleep.Info.EffectRange;
		if (DeepDungeonCrowd.IsPending(enemy) || !SleepInterruptWatch.CanStillBeSlept(enemy) || AreaReachesNewMob(enemy, radius))
		{
			return false;
		}

		if (radius <= 0f)
		{
			return true;
		}

		foreach (var other in DataCenter.UntouchableHostiles)
		{
			if (other != null && other.GameObjectId != enemy.GameObjectId && !DeepDungeonCrowd.IsHeldAsleep(other)
				&& Vector3.Distance(other.Position, enemy.Position) - other.HitboxRadius <= radius)
			{
				return false;
			}
		}

		return true;
	}

	// The sleep on this one enemy, aimed past the lists of targets: a sleeper is not in them and counts as untouchable
	// there, which holds for damage, not for a sleep (concept 23, "Tiefe Gewölbe: Gegnerkontrolle", A837). Its status is
	// no reason to refuse: a renewal lands on a sleeping enemy.
	private static bool SleepOn(IBattleChara enemy, IBaseAction sleep, out IAction? act)
	{
		ActionTargetInfo.RuleTargetId = enemy.GameObjectId;
		try
		{
			return sleep.CanUse(out act, skipStatusProvideCheck: true, skipAoeCheck: true, skipTTKCheck: true);
		}
		finally
		{
			ActionTargetInfo.RuleTargetId = 0;
		}
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
	/// not fighting yet while "Don't pull new enemies with AoE" is on, or in a deep dungeon "Deep dungeons: damage doesn't
	/// pull new enemies" (A834). A stop is an area action like any other, and the setting says "never".
	/// </summary>
	/// <param name="center">The centre of the area.</param>
	/// <param name="radius">Its radius, zero for a single target.</param>
	/// <returns>True when the setting forbids the action.</returns>
	protected static bool AreaReachesNewMob(IBattleChara center, float radius)
	{
		if (!DataCenter.GuardsAgainstPull(false) || radius <= 0f || center == null)
		{
			return false;
		}

		// A834: over every enemy figure - one not fighting yet may be missing from the list of targets.
		foreach (var hostile in DataCenter.AllTargets)
		{
			if (hostile != null && hostile.GameObjectId != center.GameObjectId && hostile.IsEnemy() && !hostile.IsDead
				&& DataCenter.PullsIfHit(hostile)
				&& Vector3.Distance(hostile.Position, center.Position) - hostile.HitboxRadius <= radius)
			{
				return true;
			}
		}

		return false;
	}
}
