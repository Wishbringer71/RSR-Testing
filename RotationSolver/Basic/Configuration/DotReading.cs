namespace RotationSolver.Basic.Configuration;

/// <summary>
/// What one enemy damage over time on a party member was seen to do in one area - learned in play over every session
/// (concept 08, "Schaden über Zeit: HoT gegen DoT", A432).
/// </summary>
/// <remarks>
/// The strength is an amount of HP per tick, not a share of the member's maximum: a tank loses the same amount and a
/// smaller share, so the share is taken per member when it is read. Both figures keep the highest seen, like the other
/// learned tables: a tick a barrier swallowed in part, a tank's mitigation or a status cut short by its mechanic would
/// pull a mean below what the damage over time can do, and the reading is the one that decides before the first tick
/// has been measured - the owner's rule, the party's safety first (A432).
/// </remarks>
internal sealed class DotReading
{
	/// <summary>Ticks measured while one instance of this status was the only enemy damage over time on the member.</summary>
	public int Ticks { get; set; }

	/// <summary>The highest of them, in HP.</summary>
	public float TickAmount { get; set; }

	/// <summary>Times the status was seen to end, from its start, where it states no end of its own.</summary>
	public int Ends { get; set; }

	/// <summary>The longest of them, in seconds.</summary>
	public float Lifetime { get; set; }

	/// <summary>One more tick.</summary>
	public void AddTick(float amount)
	{
		Ticks++;
		TickAmount = Math.Max(TickAmount, amount);
	}

	/// <summary>One more end.</summary>
	public void AddEnd(float seconds)
	{
		Ends++;
		Lifetime = Math.Max(Lifetime, seconds);
	}

	/// <summary>A copy, for a save that writes off the game thread.</summary>
	public DotReading Copy()
	{
		return new DotReading { Ticks = Ticks, TickAmount = TickAmount, Ends = Ends, Lifetime = Lifetime };
	}

	/// <summary>Every figure at its higher value - the merge a save applies with the file.</summary>
	public static DotReading Higher(DotReading first, DotReading second)
	{
		return new DotReading
		{
			Ticks = Math.Max(first.Ticks, second.Ticks),
			TickAmount = Math.Max(first.TickAmount, second.TickAmount),
			Ends = Math.Max(first.Ends, second.Ends),
			Lifetime = Math.Max(first.Lifetime, second.Lifetime),
		};
	}
}

/// <summary>The enemy damage over time learned in one area, by status id.</summary>
internal sealed class DotArea
{
	/// <summary>The readings, by status id.</summary>
	public Dictionary<uint, DotReading> Statuses { get; set; } = [];

	/// <summary>A deep copy, for a save that writes off the game thread.</summary>
	public DotArea Copy()
	{
		var copy = new DotArea();
		foreach (var (id, reading) in Statuses)
		{
			copy.Statuses[id] = reading.Copy();
		}

		return copy;
	}

	/// <summary>Every status of both, each by <see cref="DotReading.Higher"/> - the merge a save applies with the file.</summary>
	public static DotArea Merge(DotArea first, DotArea second)
	{
		var merged = first.Copy();
		foreach (var (id, reading) in second.Statuses)
		{
			merged.Statuses[id] = merged.Statuses.TryGetValue(id, out var known)
				? DotReading.Higher(known, reading)
				: reading.Copy();
		}

		return merged;
	}
}
