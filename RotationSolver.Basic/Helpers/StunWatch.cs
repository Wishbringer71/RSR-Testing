using System.Collections.Concurrent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What a stun on each enemy still does, as the fight has shown it: how often a stun landed on it, the
/// last one that has not reached its status list yet, and whether the last attempt found it immune.
/// </summary>
/// <remarks>
/// <para>The game halves a stun with each application until the target is temporarily immune (The Balance,
/// White Mage Basic Guide, 02.10.2026: "halving its duration with each application until they become
/// temporarily immune to stun"). It shows neither the count nor that immunity as a status: Stun Resistance
/// (39, 1349, "Immune to stun effects") is the permanent immunity of certain enemies (xivapi). WrathCombo
/// counts the applications itself for the same reason - a tool, not a game source. Reading a status that is
/// not there cost the White Mage his Holy: an enemy that could no longer be stunned counted as fresh, and
/// the next Holy was held for a stun that never came, in 134 of 180 decisions after a Holy (trace of
/// 04-10 October, A272).</para>
///
/// <para>What the fight shows instead, from every action set of any source: a stun applied to an enemy, an
/// attempt the game reports as fully resisted, and an action that stunned before hitting an enemy without
/// stunning it. The last one is immunity as well; the game does not report every immune target.</para>
///
/// <para>The count corrects itself. The halving needs the count since the last immunity ended, which no
/// clock gives; when the stun shows up in the status list, its full length says which application it was,
/// and that replaces the count. An immunity ends the same way - the next stun that lands.</para>
///
/// <para>Recognition only: no option and no threshold. The readers decide what a stun is worth.</para>
/// </remarks>
public static class StunWatch
{
	private sealed class Record
	{
		public int Applications;
		public DateTime LastApplied;
		public float LastBase;
		public float LastExpected;
		public bool Immune;
		public bool LengthSeen;
	}

	/// <summary>One target of an action set, as far as stuns are concerned.</summary>
	/// <param name="Target">The target's object id.</param>
	/// <param name="Applied">A stun was put on it.</param>
	/// <param name="Resisted">The game reported a stun on it as fully resisted.</param>
	/// <param name="Damaged">The action damaged it.</param>
	internal readonly record struct Observation(ulong Target, bool Applied, bool Resisted, bool Damaged);

	private static readonly ConcurrentDictionary<ulong, Record> _records = new();

	// Actions seen to stun in this session. Only these say anything by hitting without a stun.
	private static readonly ConcurrentDictionary<uint, byte> _stunningActions = new();

	/// <summary>
	/// Records what one action set did to stuns. Called by the effect handler for every set.
	/// </summary>
	/// <param name="actionId">The action of the set.</param>
	/// <param name="targets">Its targets.</param>
	internal static void Observe(uint actionId, IReadOnlyList<Observation> targets)
	{
		if (!DataCenter.InCombat || targets.Count == 0)
		{
			return;
		}

		var anyApplied = false;
		for (var i = 0; i < targets.Count; i++)
		{
			anyApplied |= targets[i].Applied;
		}

		if (anyApplied)
		{
			_stunningActions[actionId] = 0;
		}
		else if (!_stunningActions.ContainsKey(actionId))
		{
			var anyResisted = false;
			for (var i = 0; i < targets.Count; i++)
			{
				anyResisted |= targets[i].Resisted;
			}

			if (!anyResisted)
			{
				return;
			}
		}

		var now = DateTime.Now;
		var baseLength = DefensiveValues.DurationOf(actionId);
		for (var i = 0; i < targets.Count; i++)
		{
			var t = targets[i];
			if (t.Applied)
			{
				var record = _records.GetOrAdd(t.Target, _ => new Record());
				record.Applications = record.Immune ? 1 : record.Applications + 1;
				record.Immune = false;
				record.LastApplied = now;
				record.LastBase = baseLength;
				// Halved with each application (The Balance): the n-th lasts base * 2^-(n-1).
				record.LastExpected = baseLength > 0f ? MathF.ScaleB(baseLength, 1 - record.Applications) : 0f;
				record.LengthSeen = false;
			}
			else if (t.Resisted || t.Damaged)
			{
				var record = _records.GetOrAdd(t.Target, _ => new Record());
				record.Immune = true;
				record.LastExpected = 0f;
			}
		}
	}

	/// <summary>Forgets everything once the fight is over. Called once per framework cycle.</summary>
	internal static void Update()
	{
		if (!DataCenter.InCombat && !_records.IsEmpty)
		{
			_records.Clear();
		}
	}

	/// <summary>
	/// How long a stun on <paramref name="enemy"/> still runs: the one in its status list, or one that landed
	/// and is not listed yet, at the length the halving gives it. Zero when nothing runs.
	/// </summary>
	/// <remarks>
	/// A stun without a timer is left out: it does not end, so nothing about it can be stretched or
	/// overwritten.
	/// </remarks>
	/// <param name="enemy">The enemy.</param>
	/// <returns>Seconds.</returns>
	public static float StunLeft(IBattleChara enemy)
	{
		if (enemy == null)
		{
			return 0f;
		}

		var listed = ListedStunLeft(enemy);
		_records.TryGetValue(enemy.GameObjectId, out var record);
		var now = DateTime.Now;

		if (listed > 0f)
		{
			// The first sight of the stun tells its full length, and with it which application it was.
			if (record != null && !record.LengthSeen && record.LastBase > 0f)
			{
				var full = listed + (float)(now - record.LastApplied).TotalSeconds;
				if (full > 0f)
				{
					record.Applications = Math.Max(1, (int)MathF.Round(MathF.Log2(record.LastBase / full)) + 1);
					record.LastExpected = full;
				}

				record.LengthSeen = true;
			}

			return listed;
		}

		if (record == null || record.Immune || record.LastExpected <= 0f)
		{
			return 0f;
		}

		return Math.Max(0f, record.LastExpected - (float)(now - record.LastApplied).TotalSeconds);
	}

	/// <summary>
	/// Whether a stun could still land on <paramref name="enemy"/>: it is neither permanently immune nor
	/// found immune by the last attempt.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True while a stun can still land.</returns>
	public static bool CanStillBeStunned(IBattleChara enemy)
	{
		if (enemy == null || enemy.HasStatus(false, StatusHelper.StunResistanceStatus))
		{
			return false;
		}

		return !_records.TryGetValue(enemy.GameObjectId, out var record) || !record.Immune;
	}

	/// <summary>How many stuns have landed on <paramref name="enemy"/> since it was last immune.</summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>The count, zero when none is known.</returns>
	public static int Applications(IBattleChara enemy)
		=> enemy != null && _records.TryGetValue(enemy.GameObjectId, out var record) && !record.Immune
			? record.Applications
			: 0;

	private static float ListedStunLeft(IBattleChara enemy)
	{
		var longest = 0f;
		try
		{
			var statuses = enemy.StatusList;
			if (statuses == null)
			{
				return 0f;
			}

			for (var i = 0; i < statuses.Length; i++)
			{
				var status = statuses[i];
				if (status == null || status.StatusId == 0 || status.RemainingTime <= 0f)
				{
					continue;
				}

				foreach (var stun in StatusHelper.StunStatus)
				{
					if ((uint)stun == status.StatusId)
					{
						longest = Math.Max(longest, status.RemainingTime);
						break;
					}
				}
			}
		}
		catch
		{
			return 0f;
		}

		return longest;
	}
}
