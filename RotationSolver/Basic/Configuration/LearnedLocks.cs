namespace RotationSolver.Basic.Configuration;

/// <summary>What a learned lock does in the running game version (concept 13, "Stufe 4").</summary>
internal enum LockState
{
	/// <summary>No lock stands.</summary>
	None,

	/// <summary>
	/// A lock from an earlier game version, or from before stage 4, not yet tried in the running one: a hypothesis with one
	/// try.
	/// </summary>
	TakenOver,

	/// <summary>The lock holds: seen in the running game version, its one try there spent, or the version unreadable.</summary>
	Holds,
}

/// <summary>Which of the three sleep locks a key names.</summary>
internal enum SleepLockType
{
	/// <summary>An enemy kind that resisted a first sleep (<see cref="OtherConfiguration.SleepResistantKinds"/>).</summary>
	Kind,

	/// <summary>The sleeps a kind takes in a fight before it resists (<see cref="OtherConfiguration.SleepDiminishing"/>).</summary>
	Limit,

	/// <summary>An enemy action a sleep did not stop (<see cref="Helpers.SleepInterruptWatch.IsUnstoppable"/>).</summary>
	Action,
}

/// <summary>One sleep lock: its type and the enemy kind (base id) or action it is about.</summary>
internal readonly record struct SleepLockKey(SleepLockType Type, uint Id);

/// <summary>
/// One sleep lock's latest decisive observation, kept beside the learned file, which only rises or falls (concept 13,
/// "Stufe 4", 4b).
/// </summary>
internal sealed class SleepLockMark
{
	/// <summary>The game version of the latest observation that decided the lock; empty when it could not be read.</summary>
	public string Game { get; set; } = string.Empty;

	/// <summary>That observation refuted the lock: a sleep landed on the kind, or after as many sleeps as the limit.</summary>
	public bool Refuted { get; set; }

	/// <summary>
	/// For a kind, the resistances the file counted when it was decided; for a limit, the limit learned, or when refuted the
	/// sleeps a sleep landed after.
	/// </summary>
	public int Value { get; set; }

	/// <summary>The game version in which the one try on the lock, taken over, was spent.</summary>
	public string Tried { get; set; } = string.Empty;
}

/// <summary>The marks of the three sleep locks, stored as <c>SleepLocks.json</c>.</summary>
internal sealed class SleepLockMarks
{
	/// <summary>Per enemy kind that resisted a first sleep.</summary>
	public Dictionary<uint, SleepLockMark> Kinds { get; set; } = [];

	/// <summary>Per enemy kind with a learned sleep limit.</summary>
	public Dictionary<uint, SleepLockMark> Limits { get; set; } = [];

	/// <summary>Per enemy action a sleep did not stop.</summary>
	public Dictionary<uint, SleepLockMark> Actions { get; set; } = [];

	/// <summary>A deep copy, for a save off the game thread.</summary>
	public SleepLockMarks Copy()
	{
		return new SleepLockMarks { Kinds = CopyOf(Kinds), Limits = CopyOf(Limits), Actions = CopyOf(Actions) };

		static Dictionary<uint, SleepLockMark> CopyOf(Dictionary<uint, SleepLockMark> marks)
		{
			var copy = new Dictionary<uint, SleepLockMark>(marks.Count);
			foreach (var (id, mark) in marks)
			{
				copy[id] = new SleepLockMark { Game = mark.Game, Refuted = mark.Refuted, Value = mark.Value, Tried = mark.Tried };
			}

			return copy;
		}
	}

	/// <summary>This, with what a broken or hand-edited file can leave null dropped or emptied, as read.</summary>
	public SleepLockMarks Whole()
	{
		Kinds = Clean(Kinds);
		Limits = Clean(Limits);
		Actions = Clean(Actions);
		return this;

		static Dictionary<uint, SleepLockMark> Clean(Dictionary<uint, SleepLockMark>? marks)
		{
			var clean = new Dictionary<uint, SleepLockMark>();
			foreach (var (id, mark) in marks ?? [])
			{
				if (mark != null)
				{
					mark.Game ??= string.Empty;
					mark.Tried ??= string.Empty;
					clean[id] = mark;
				}
			}

			return clean;
		}
	}

	/// <summary>The marks of one lock type.</summary>
	public Dictionary<uint, SleepLockMark> Of(SleepLockType type) => type switch
	{
		SleepLockType.Kind => Kinds,
		SleepLockType.Limit => Limits,
		_ => Actions,
	};
}

/// <summary>
/// The sleep locks under the game version (concept 13, "Stufe 4", 4b): which hold, which are a hypothesis with one try,
/// and what an observation confirms or refutes. Pure: the learned files and the marks are passed in, so the logic runs
/// outside the game (revision_harness).
/// </summary>
internal static class SleepLockRules
{
	/// <summary>Whether an enemy kind is locked as resistant.</summary>
	/// <param name="resistant">The learned resistances per kind.</param>
	/// <param name="marks">The lock marks.</param>
	/// <param name="kind">The kind.</param>
	/// <param name="game">The running game version, null when unreadable.</param>
	internal static LockState KindState(Dictionary<uint, int> resistant, SleepLockMarks marks, uint kind, string? game)
	{
		if (!resistant.TryGetValue(kind, out var count))
		{
			return LockState.None;
		}

		var mark = marks.Kinds.GetValueOrDefault(kind);

		// Refuted, unless the file counts a resistance written since by another client or an older RSR.
		if (mark is { Refuted: true } && count <= mark.Value)
		{
			return LockState.None;
		}

		return Standing(mark is { Refuted: false } && game != null && mark.Game == game, mark, game);
	}

	/// <summary>
	/// The sleeps an enemy of <paramref name="kind"/> takes in a fight before it resists, and how that limit holds: its own,
	/// otherwise the lowest of the other kinds above any limit refuted for it. Null when none is known.
	/// </summary>
	/// <param name="diminishing">The learned limits per kind.</param>
	/// <param name="marks">The lock marks.</param>
	/// <param name="kind">The kind.</param>
	/// <param name="game">The running game version, null when unreadable.</param>
	/// <param name="state">How the limit holds.</param>
	internal static int? LimitFor(Dictionary<uint, int> diminishing, SleepLockMarks marks, uint kind, string? game, out LockState state)
	{
		state = LockState.None;
		var mark = marks.Limits.GetValueOrDefault(kind);
		if (OwnLimit(diminishing, marks, kind, game, out var floor) is { } own)
		{
			state = Standing(own.Here, mark, game);
			return own.Limit;
		}

		// The halving is the game's, not the kind's: another kind's limit stands for this one, but not at or below a
		// count this kind has been seen to take.
		int? lowest = null;
		var here = false;
		foreach (var other in diminishing.Keys)
		{
			Lower(other);
		}

		foreach (var other in marks.Limits.Keys)
		{
			Lower(other);
		}

		if (lowest is not { } limit)
		{
			return null;
		}

		state = Standing(here, mark, game);
		return limit;

		void Lower(uint other)
		{
			if (other == kind || OwnLimit(diminishing, marks, other, game, out _) is not { } found || found.Limit <= floor)
			{
				return;
			}

			if (lowest == null || found.Limit < lowest)
			{
				lowest = found.Limit;
				here = found.Here;
			}
			else if (found.Limit == lowest)
			{
				here |= found.Here;
			}
		}
	}

	// A kind's own limit: the one its mark learned, otherwise the file's. A refuted mark leaves none, and the count it was
	// refuted at as the floor.
	private static (int Limit, bool Here)? OwnLimit(Dictionary<uint, int> diminishing, SleepLockMarks marks, uint kind, string? game,
		out int floor)
	{
		floor = 0;
		if (marks.Limits.TryGetValue(kind, out var mark))
		{
			if (mark.Refuted)
			{
				floor = mark.Value;
				return null;
			}

			if (mark.Value > 0)
			{
				return (mark.Value, game != null && mark.Game == game);
			}
		}

		return diminishing.TryGetValue(kind, out var limit) ? (limit, false) : null;
	}

	/// <summary>Whether an enemy action is locked as one a sleep does not stop.</summary>
	/// <param name="unstoppable">The stored outcomes say a sleep cannot stop it.</param>
	/// <param name="marks">The lock marks.</param>
	/// <param name="action">The action.</param>
	/// <param name="game">The running game version, null when unreadable.</param>
	internal static LockState ActionState(bool unstoppable, SleepLockMarks marks, uint action, string? game)
	{
		if (!unstoppable)
		{
			return LockState.None;
		}

		var mark = marks.Actions.GetValueOrDefault(action);
		return Standing(mark != null && game != null && mark.Game == game, mark, game);
	}

	// A lock seen in the running version holds. One that was not is a hypothesis with one try there - none while the version
	// is unreadable, where "one per version" cannot be kept.
	private static LockState Standing(bool seenHere, SleepLockMark? mark, string? game)
	{
		return seenHere || game == null || mark?.Tried == game ? LockState.Holds : LockState.TakenOver;
	}

	/// <summary>A sleep landed on an enemy of a kind locked as resistant: the lock is refuted.</summary>
	/// <returns>True when this refuted it.</returns>
	internal static bool RefuteKind(Dictionary<uint, int> resistant, SleepLockMarks marks, uint kind, string? game)
	{
		if (KindState(resistant, marks, kind, game) == LockState.None)
		{
			return false;
		}

		var mark = MarkOf(marks.Kinds, kind);
		mark.Game = game ?? string.Empty;
		mark.Refuted = true;
		mark.Value = resistant[kind];
		return true;
	}

	/// <summary>
	/// A sleep landed on an enemy of <paramref name="kind"/> after <paramref name="sleptBefore"/> sleeps in this fight: a limit
	/// at or below that count is refuted.
	/// </summary>
	/// <returns>True when this refuted it.</returns>
	internal static bool RefuteLimit(Dictionary<uint, int> diminishing, SleepLockMarks marks, uint kind, int sleptBefore, string? game)
	{
		if (sleptBefore <= 0 || LimitFor(diminishing, marks, kind, game, out _) is not { } limit || sleptBefore < limit)
		{
			return false;
		}

		var mark = MarkOf(marks.Limits, kind);
		mark.Game = game ?? string.Empty;
		mark.Refuted = true;
		mark.Value = sleptBefore;
		return true;
	}

	/// <summary>An enemy of <paramref name="kind"/> resisted a first sleep, counted in the file: the lock holds in this version.</summary>
	internal static void ConfirmKind(Dictionary<uint, int> resistant, SleepLockMarks marks, uint kind, string? game)
	{
		var mark = MarkOf(marks.Kinds, kind);
		mark.Game = game ?? string.Empty;
		mark.Refuted = false;
		mark.Value = resistant.GetValueOrDefault(kind);
	}

	/// <summary>
	/// An enemy of <paramref name="kind"/> resisted after <paramref name="slept"/> sleeps in this fight: the limit holds in
	/// this version, the lowest seen in it.
	/// </summary>
	internal static void ConfirmLimit(SleepLockMarks marks, uint kind, int slept, string? game)
	{
		var mark = MarkOf(marks.Limits, kind);
		var here = !mark.Refuted && mark.Value > 0 && mark.Game == (game ?? string.Empty);
		mark.Value = here ? Math.Min(mark.Value, slept) : slept;
		mark.Game = game ?? string.Empty;
		mark.Refuted = false;
	}

	/// <summary>A cast of <paramref name="action"/> went off although a sleep landed in time: the lock holds in this version.</summary>
	internal static void ConfirmAction(SleepLockMarks marks, uint action, string? game)
	{
		MarkOf(marks.Actions, action).Game = game ?? string.Empty;
	}

	/// <summary>A sleep was pressed on these taken-over locks: their one try in this version is spent.</summary>
	/// <returns>False when the version is unreadable and nothing was marked.</returns>
	internal static bool Try(SleepLockMarks marks, IReadOnlyList<SleepLockKey> keys, string? game)
	{
		if (game == null)
		{
			return false;
		}

		foreach (var key in keys)
		{
			MarkOf(marks.Of(key.Type), key.Id).Tried = game;
		}

		return keys.Count > 0;
	}

	private static SleepLockMark MarkOf(Dictionary<uint, SleepLockMark> marks, uint id)
	{
		if (!marks.TryGetValue(id, out var mark))
		{
			mark = new SleepLockMark();
			marks[id] = mark;
		}

		return mark;
	}
}

/// <summary>
/// What the landings of one enemy action laying a status that damages every action showed (concept 13, "Stufe 4", 4c),
/// kept beside <see cref="OtherConfiguration.ActionPenaltyCasts"/>, which only rises.
/// </summary>
internal sealed class PenaltySighting
{
	/// <summary>The game version the counts below are of.</summary>
	public string Game { get; set; } = string.Empty;

	/// <summary>Landings in that version that laid such a status on a party member.</summary>
	public int With { get; set; }

	/// <summary>Landings in that version that hit the cast's target, a party member, and laid it on nobody.</summary>
	public int Without { get; set; }

	/// <summary>Landings with the status in earlier versions; one for an action learned before stage 4, uncounted then.</summary>
	public int Before { get; set; }

	/// <summary>The running version refuted it: the action no longer holds spells until it lays the status again.</summary>
	public bool Refuted { get; set; }
}

/// <summary>
/// When a learned action laying a status that damages every action is refuted (concept 13, rule 6, "Pyretisch-Wirken"):
/// only after the running version saw it land on its target without the status at least twice, more often than it was
/// seen with the status before, and never with it. Pure, as <see cref="SleepLockRules"/>.
/// </summary>
internal static class PenaltySightingRules
{
	/// <summary>
	/// The fewest landings without the status that refute an action: one may be a case the effect does not show; two is the
	/// least that repeats (rule 6, "mindestens zweimal").
	/// </summary>
	internal const int RefuteLandings = 2;

	/// <summary>Whether a refuted action no longer holds spells.</summary>
	internal static bool IsRefuted(Dictionary<uint, PenaltySighting> sightings, uint action)
	{
		return sightings.TryGetValue(action, out var sighting) && sighting.Refuted;
	}

	/// <summary>A landing of <paramref name="action"/> laid the status on a party member.</summary>
	/// <param name="sightings">The sightings.</param>
	/// <param name="action">The action.</param>
	/// <param name="learned">The action was learned before this landing.</param>
	/// <param name="game">The running game version, null when unreadable.</param>
	/// <returns>True when this lifted a refutation.</returns>
	internal static bool SeenWith(Dictionary<uint, PenaltySighting> sightings, uint action, bool learned, string? game)
	{
		var lifted = IsRefuted(sightings, action);
		if (game == null)
		{
			if (lifted)
			{
				sightings[action].Refuted = false;
			}

			return lifted;
		}

		var sighting = Running(sightings, action, learned, game);
		sighting.With++;
		sighting.Refuted = false;
		return lifted;
	}

	/// <summary>A landing of a learned <paramref name="action"/> hit its target, a party member, and laid the status on nobody.</summary>
	/// <param name="sightings">The sightings.</param>
	/// <param name="action">The action.</param>
	/// <param name="game">The running game version, null when unreadable: then nothing is counted.</param>
	/// <returns>True when this refuted it.</returns>
	internal static bool SeenWithout(Dictionary<uint, PenaltySighting> sightings, uint action, string? game)
	{
		if (game == null || IsRefuted(sightings, action))
		{
			return false;
		}

		var sighting = Running(sightings, action, true, game);
		sighting.Without++;
		if (sighting.With == 0 && sighting.Without >= RefuteLandings && sighting.Without > sighting.Before)
		{
			sighting.Refuted = true;
			return true;
		}

		return false;
	}

	// The sighting of the running version: the counts of an earlier one move to Before.
	private static PenaltySighting Running(Dictionary<uint, PenaltySighting> sightings, uint action, bool learned, string game)
	{
		if (!sightings.TryGetValue(action, out var sighting))
		{
			sighting = new PenaltySighting { Game = game, Before = learned ? 1 : 0 };
			sightings[action] = sighting;
		}
		else if (sighting.Game != game)
		{
			sighting.Before += sighting.With;
			sighting.With = 0;
			sighting.Without = 0;
			sighting.Game = game;
		}

		return sighting;
	}

	/// <summary>The sightings as read, with what a broken or hand-edited file can leave null dropped or emptied.</summary>
	internal static Dictionary<uint, PenaltySighting> Whole(Dictionary<uint, PenaltySighting>? sightings)
	{
		var clean = new Dictionary<uint, PenaltySighting>();
		foreach (var (id, sighting) in sightings ?? [])
		{
			if (sighting != null)
			{
				sighting.Game ??= string.Empty;
				clean[id] = sighting;
			}
		}

		return clean;
	}

	/// <summary>A deep copy, for a save off the game thread.</summary>
	internal static Dictionary<uint, PenaltySighting> Copy(Dictionary<uint, PenaltySighting> sightings)
	{
		var copy = new Dictionary<uint, PenaltySighting>(sightings.Count);
		foreach (var (id, s) in sightings)
		{
			copy[id] = new PenaltySighting { Game = s.Game, With = s.With, Without = s.Without, Before = s.Before, Refuted = s.Refuted };
		}

		return copy;
	}
}
