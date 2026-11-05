using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// How dangerous each enemy is to the party right now, per second, and which one to attack first by it: the enemy whose
/// danger per second is largest against its HP (Smith's rule: the order that keeps the sum of danger times lifetime
/// smallest). For a beastmaster or a blue mage with no tank and no healer alive beside him (concept 03, "Zielart ohne
/// Tank und Heiler", A537, A538, A541).
/// </summary>
/// <remarks>
/// <para>Danger per second: the damage the enemy dealt to the player and the party members in this fight, over the time
/// since its first hit; a hit it announces by a cast at the player or reaching him, at once, with the size the learned
/// tables give it, over the cast time left; and for an enemy that heals other enemies, the damage those deal in the time
/// the heal gains them.</para>
///
/// <para>The rate is recorded on the game thread by the effect handler and read there by the target selection.</para>
/// </remarks>
internal static class EnemyDanger
{
	private sealed class Record
	{
		public DateTime First;
		public double Damage;
		public int Hits;
		public double Healed;
		public double HealGain;
	}

	private static readonly Dictionary<ulong, Record> _bySource = [];

	/// <summary>An enemy's hit on the player or a party member, for <paramref name="amount"/> HP. Effect handler.</summary>
	internal static void RecordHit(ulong source, uint amount)
	{
		if (amount == 0 || !DataCenter.InCombat)
		{
			return;
		}

		if (!_bySource.TryGetValue(source, out var record))
		{
			record = new Record { First = DateTime.Now };
			_bySource[source] = record;
		}

		record.Damage += amount;
		record.Hits++;
	}

	/// <summary>
	/// An enemy healed another enemy for <paramref name="amount"/> HP. What the heal gains is the damage the healed deals
	/// in the time it now lives longer: the HP over the rate at which it falls (its corrected time to kill against its
	/// HP), times its own danger per second. Effect handler.
	/// </summary>
	internal static void RecordHeal(ulong source, IBattleChara healed, uint amount)
	{
		if (amount == 0 || !DataCenter.InCombat || healed.GameObjectId == source || healed.CurrentHp == 0)
		{
			return;
		}

		if (!_bySource.TryGetValue(source, out var record))
		{
			record = new Record { First = DateTime.Now };
			_bySource[source] = record;
		}

		record.Healed += amount;
		var ttk = healed.GetCorrectedTTK();
		if (!float.IsNaN(ttk) && ttk > 0f)
		{
			record.HealGain += amount * (ttk / healed.CurrentHp) * OwnRate(healed.GameObjectId);
		}
	}

	/// <summary>Forgets every record: out of combat, or when the lists are cleared.</summary>
	internal static void Clear()
	{
		_bySource.Clear();
		_lead = 0;
	}

	/// <summary>
	/// The enemy's danger to the party per second (HP points): its measured rate, the heal gain over the same time, and a
	/// hit it announces now. <paramref name="hits"/> is the number of its measured hits; <paramref name="announced"/>
	/// whether an announced hit counts in it.
	/// </summary>
	internal static float Rate(IBattleChara enemy, out int hits, out bool announced)
	{
		hits = 0;
		var rate = 0f;
		if (_bySource.TryGetValue(enemy.GameObjectId, out var record))
		{
			hits = record.Hits;
			var seconds = Elapsed(record);
			rate = (float)((record.Damage + record.HealGain) / seconds);
		}

		var cast = AnnouncedRate(enemy);
		announced = cast > 0f;
		return rate + cast;
	}

	// Damage per second from the enemy's own hits alone, for the gain of a heal on it.
	private static float OwnRate(ulong enemy)
	{
		return _bySource.TryGetValue(enemy, out var record) ? (float)(record.Damage / Elapsed(record)) : 0f;
	}

	// From the first hit to now, at least a GCD: one hit right now is no rate of its own.
	private static double Elapsed(Record record)
	{
		return Math.Max((DateTime.Now - record.First).TotalSeconds, DataCenter.DefaultGCDTotal);
	}

	// A hit the enemy announces by its cast now: at the player, or reaching him, with the share the learned tables give
	// it, over the cast time left (at least a GCD). Zero when it casts nothing measured at him.
	private static float AnnouncedRate(IBattleChara enemy)
	{
		var player = Player.Object;
		if (player == null || !enemy.IsCasting || player.MaxHp == 0)
		{
			return 0f;
		}

		var aimed = enemy.CastTargetObjectId == player.GameObjectId;
		if (!aimed && !DataCenter.CastReachesPlayer(enemy))
		{
			return 0f;
		}

		var id = enemy.CastActionId;
		var share = 0f;
		if (aimed)
		{
			share = player.IsJobCategory(JobRole.Tank)
				? TankbusterTable.PredictedShare(id, player, enemy)
				: TankbusterTable.PredictedAimedShare(id, player, enemy);
		}

		if (share <= 0f)
		{
			share = DataCenter.LearnedAreaShare(id);
		}

		if (share <= 0f)
		{
			return 0f;
		}

		var left = Math.Max(enemy.TotalCastTime - enemy.CurrentCastTime, DataCenter.DefaultGCDTotal);
		return share * player.MaxHp / left;
	}

	// Danger per second over HP, and its relative uncertainty: a count of n hits scatters by 1/sqrt(n); an announced hit
	// is known; with neither there is nothing to rank by.
	private static float Priority(IBattleChara enemy, out float uncertainty)
	{
		// Asked twice per candidate for every action that picks a target: once per frame is enough.
		var cache = _priorities.Value;
		if (cache.TryGetValue(enemy.GameObjectId, out var known))
		{
			uncertainty = known.Uncertainty;
			return known.Priority;
		}

		var rate = Rate(enemy, out var hits, out var announced);
		uncertainty = announced || hits == 0 ? 0f : 1f / MathF.Sqrt(hits);
		var priority = rate <= 0f ? 0f : rate / Math.Max(1u, enemy.CurrentHp);
		cache[enemy.GameObjectId] = (priority, uncertainty);
		return priority;
	}

	private static readonly FrameCached<Dictionary<ulong, (float Priority, float Uncertainty)>> _priorities = new(() => []);

	/// <summary>
	/// The order of the target selection: the larger priority first, the fewer HP first while neither has one. The enemy
	/// the player attacks now keeps its place until another is surely more urgent - its priority less its uncertainty above
	/// this one's plus its own -, so the target does not jump on measurement noise.
	/// </summary>
	internal static int Compare(IBattleChara a, IBattleChara b)
	{
		var pa = Priority(a, out var ua);
		var pb = Priority(b, out var ub);
		if (pa <= 0f && pb <= 0f)
		{
			return a.CurrentHp.CompareTo(b.CurrentHp);
		}

		var current = Svc.Targets.Target?.GameObjectId ?? 0;
		if (a.GameObjectId == current)
		{
			return pb * (1f - ub) > pa * (1f + ua) ? 1 : -1;
		}

		if (b.GameObjectId == current)
		{
			return pa * (1f - ua) > pb * (1f + ub) ? -1 : 1;
		}

		var byPriority = pb.CompareTo(pa);
		return byPriority != 0 ? byPriority : a.CurrentHp.CompareTo(b.CurrentHp);
	}

	private static ulong _lead;
	private static DateTime _leadTraced = DateTime.MinValue;

	/// <summary>A line when the enemy the order puts first changes, at most once a GCD.</summary>
	internal static void NoteLead(IBattleChara? lead)
	{
		if (lead == null || lead.GameObjectId == _lead
			|| (DateTime.Now - _leadTraced).TotalSeconds < DataCenter.DefaultGCDTotal)
		{
			return;
		}

		_lead = lead.GameObjectId;
		_leadTraced = DateTime.Now;
		var rate = Rate(lead, out var hits, out var announced);
		DefenseTrace.Line($"targeting by danger: {lead.Name.TextValue} first, {rate:F0} HP per second over {lead.CurrentHp} HP"
			+ $" ({hits} hit(s) measured{(announced ? ", an announced hit counted" : string.Empty)})");
	}
}
