namespace RotationSolver.Basic.Configuration;

/// <summary>
/// The file of a learned table with revisions (concept 13, "Stufe 1: Speicher, Übernahme, Sicherung"): per entry a
/// running and a taken-over layer, and what was set aside as invalid. Written beside the table's old file, which goes on
/// holding the highest value alone.
/// </summary>
internal sealed class RevisionFile<TValue>
{
	/// <summary>Raised only when an older RSR would read a file with a new field wrongly; a new field alone reads empty.</summary>
	public int Format { get; set; } = 1;

	public Dictionary<uint, RevisionEntry<TValue>> Entries { get; set; } = [];
}

/// <summary>One learned entry: the readings of the running revision, those of the ones before, and the invalid ones.</summary>
internal sealed class RevisionEntry<TValue>
{
	/// <summary>The readings taken under the revision this layer names; moved into <see cref="Inherited"/> when another runs.</summary>
	public RevisionLayer<TValue>? Current { get; set; }

	/// <summary>
	/// The readings of earlier revisions, the old file's values and the readings taken while the game version could not
	/// be read: they protect, they confirm nothing.
	/// </summary>
	public RevisionLayer<TValue>? Inherited { get; set; }

	/// <summary>The last value no reader may take - not finite, negative, or beyond what the table can hold - kept, not read.</summary>
	public RevisionQuarantine? Quarantined { get; set; }

	public RevisionEntry<TValue> Copy()
	{
		return new RevisionEntry<TValue>
		{
			Current = Current?.Copy(),
			Inherited = Inherited?.Copy(),
			Quarantined = Quarantined == null ? null : new RevisionQuarantine { Value = Quarantined.Value, Count = Quarantined.Count },
		};
	}
}

/// <summary>The readings of one revision - or of several, in the taken-over layer - of one entry.</summary>
/// <remarks><see cref="Value"/> is never changed in place: a reading replaces it with a new value, so a copy may share it.</remarks>
internal sealed class RevisionLayer<TValue>
{
	/// <summary>The game version the readings were taken in; empty for the old file's values and an unreadable version.</summary>
	public string Game { get; set; } = string.Empty;

	/// <summary>The table's measuring method they were taken with; 0 for the old file's values.</summary>
	public int Method { get; set; }

	/// <summary>The highest reading, in the form of the old file.</summary>
	public TValue? Value { get; set; }

	public int Count { get; set; }

	/// <summary>Readings without a damage up on the enemy and without a vulnerability on the one measured.</summary>
	public int Clean { get; set; }

	/// <summary>The lowest clean reading; meaningful only with <see cref="Clean"/> above zero.</summary>
	public float Low { get; set; }

	/// <summary>The highest clean reading; meaningful only with <see cref="Clean"/> above zero.</summary>
	public float High { get; set; }

	public RevisionLayer<TValue> Copy()
	{
		return new RevisionLayer<TValue>
		{
			Game = Game,
			Method = Method,
			Value = Value,
			Count = Count,
			Clean = Clean,
			Low = Low,
			High = High,
		};
	}
}

/// <summary>An invalid value, as text: a value that is not finite does not survive every JSON reader as a number.</summary>
internal sealed class RevisionQuarantine
{
	public string Value { get; set; } = string.Empty;

	public int Count { get; set; }
}
