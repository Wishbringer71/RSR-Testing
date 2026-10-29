using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace RotationSolver.Basic.Configuration;

internal partial class OtherConfiguration
{
	/// <summary>The newest form of a revision file this RSR reads (<see cref="RevisionFile{TValue}.Format"/>).</summary>
	private const int RevisionFormat = 1;

	/// <summary>What the area table's revisions hold now, for the trace's head line.</summary>
	internal static string AreaRevisionState => AreaRevision.Summary();

	/// <summary>What the tankbuster table's revisions hold now, for the trace's head line.</summary>
	internal static string TankbusterRevisionState => TankbusterRevision.Summary();

	/// <summary>
	/// One area reading - the share a landing measured, or the rating 0 of an action without a value - into the table's
	/// revisions; the table the readers see takes the highest valid reading, as before (concept 13, "Stufe 1").
	/// </summary>
	/// <param name="clean">No damage up on the caster and no vulnerability on a member measured.</param>
	internal static void MeasureAreaShare(uint actionId, float share, bool clean) => AreaRevision.Measure(actionId, share, clean);

	/// <summary>One stored tankbuster series into the table's revisions; the entry is set under <see cref="TankbusterPotentialGate"/>.</summary>
	/// <param name="clean">Not taken under a vulnerability.</param>
	internal static void MeasureTankbuster(uint actionId, TankbusterReading reading, bool clean)
		=> TankbusterRevision.Measure(actionId, reading, clean);

	/// <summary>Whether an area share is one the table can hold: finite, not negative, at most a whole maximum HP (A298).</summary>
	internal static bool IsValidAreaShare(float share) => float.IsFinite(share) && share >= 0f && IsWithinWholeHealth(share);

	/// <summary>
	/// The game version this client runs, with its expansions' versions; null while it cannot be read. A main version
	/// not made of digits and dots is no version - a field read after a shift in memory would otherwise call every patch
	/// none. An expansion slot that holds no version is passed over: the main version alone still tells a patch, and one
	/// unused slot left unset must not make the version unreadable for good.
	/// </summary>
	private static unsafe string? ReadGameVersion()
	{
		try
		{
			var framework = Framework.Instance();
			if (framework == null)
			{
				return null;
			}

			var game = framework->GameVersionString;
			if (!IsVersionText(game))
			{
				return null;
			}

			var parts = new List<string> { game };
			foreach (var expansion in framework->ExVersions)
			{
				var version = expansion.VersionString;
				if (IsVersionText(version))
				{
					parts.Add(version);
				}
			}

			return string.Join('/', parts);
		}
		catch (Exception ex)
		{
			// Asked again at every reading while it fails: said once, not at every landing of a fight.
			if (!_gameVersionFailureLogged)
			{
				_gameVersionFailureLogged = true;
				PluginLog.Warning($"Could not read the game version: {ex.Message}");
			}

			return null;
		}
	}

	private static bool _gameVersionFailureLogged;

	private static bool IsVersionText(string? text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}

		var digits = false;
		foreach (var character in text)
		{
			if (char.IsAsciiDigit(character))
			{
				digits = true;
			}
			else if (character != '.')
			{
				return false;
			}
		}

		return digits;
	}

	/// <summary>
	/// A learned table with revisions (concept 13, "Stufe 1: Speicher, Übernahme, Sicherung"): per entry a running layer -
	/// the readings of the game version and measuring method that run - and a taken-over one - the readings before, the
	/// old file's values, and those taken while the version could not be read. The table the readers see is set from it
	/// alone: per entry the higher of the two layers, the highest valid reading as before. Nothing in a fight changes
	/// with it; the later stages read the layers.
	/// </summary>
	/// <remarks>
	/// Saved in "&lt;name&gt;.rev.json" beside the old file, which <see cref="LearnedFile{TValue}"/> goes on writing as
	/// before - an older RSR finds everything in it, and it is the second store when the new file and its backup fail.
	/// Every way in which this could lose a reading - a version change, the merge with the file, the old file taken over
	/// again, the backup, an unreadable version - is played through in learned_store_model.py.
	/// </remarks>
	private sealed class RevisedStore<TValue>(string name, int method, Func<TValue, TValue, TValue> higher,
		Func<TValue, bool> valid, Func<TValue, float?> cleanFigure, Func<Dictionary<uint, TValue>> table, object? gate,
		string noun)
		where TValue : notnull
	{
		// The table's own lock where its readers take one, so the readers never see an entry half set.
		private readonly object _memory = gate ?? new object();

		private readonly object _file = new();

		private Dictionary<uint, RevisionEntry<TValue>> _entries = [];

		// Null while the game version cannot be read: then no layer moves, and every reading goes to the taken-over one.
		private string? _game;

		// False until a load finished, and for a file of a newer form: then nothing is written over it.
		private bool _writable;

		private const string NotLoaded = "not loaded yet";

		private string _loaded = NotLoaded;

		private string _saved = "not saved yet";

		private string FilePath => GetFilePath(name + ".rev");

		/// <summary>
		/// Builds the layers from the new file - or its backup, when the file is there but cannot be read - and takes over
		/// what the old file adds to them. Returns the table for the readers.
		/// </summary>
		public Dictionary<uint, TValue> Load(Dictionary<uint, TValue> old)
		{
			lock (_memory)
			{
				_game = ReadGameVersion();
				var path = FilePath;
				var existed = File.Exists(path);
				var file = ReadFile<RevisionFile<TValue>>(path);
				var source = !existed ? "no revision file yet" : "from the revision file";
				if (file == null && existed && !File.Exists(path))
				{
					// The file is there but unreadable, set aside as .corrupt. A missing file is not replaced by its backup:
					// he deleted it, and that stands.
					file = ReadFile<RevisionFile<TValue>>(path + ".bak");
					source = file != null ? "REVISION FILE UNREADABLE, set aside as .corrupt, read from its backup"
						: "REVISION FILE UNREADABLE, set aside as .corrupt, no readable backup";
				}

				_writable = true;
				if (file != null && file.Format > RevisionFormat)
				{
					// A newer RSR wrote it in a form this one may read wrongly: left as it is, never written over.
					_writable = false;
					source = $"REVISION FILE OF A NEWER FORM ({file.Format}), left as it is and not written";
					file = null;
				}

				var entries = file?.Entries ?? [];
				var unreadable = Sanitize(entries);
				var takenOver = 0;
				var setAside = 0;
				foreach (var (id, value) in old)
				{
					switch (TakeOver(entries, id, value))
					{
						case TakenOver.Value:
							takenOver++;
							break;
						case TakenOver.SetAside:
							setAside++;
							break;
					}
				}

				var moved = 0;
				foreach (var entry in entries.Values)
				{
					if (Transition(entry, _game))
					{
						moved++;
					}
				}

				_entries = entries;
				var shown = new Dictionary<uint, TValue>(entries.Count);
				foreach (var (id, entry) in entries)
				{
					if (TryShow(entry, out var value))
					{
						shown[id] = value;
					}
				}

				_loaded = $"loaded {DateTime.Now:HH:mm:ss} {source}"
					+ (takenOver > 0 ? $", {takenOver} taken over from the old file" : string.Empty)
					+ (moved > 0 ? $", {moved} moved to taken-over at a new game version or method" : string.Empty)
					+ (setAside > 0 ? $", {setAside} invalid value(s) of the old file set aside" : string.Empty)
					+ (unreadable > 0 ? $", {unreadable} unreadable entry or layer(s) set aside" : string.Empty)
					+ (_game == null ? ", GAME VERSION NOT READABLE - readings go to the taken-over layer" : string.Empty);
				return shown;
			}
		}

		/// <summary>One reading. Its validity, the layer it goes to and the table the readers see are settled here.</summary>
		public void Measure(uint id, TValue value, bool clean)
		{
			string? line = null;
			lock (_memory)
			{
				_game ??= ReadGameVersion();
				if (!_entries.TryGetValue(id, out var entry))
				{
					entry = new RevisionEntry<TValue>();
					_entries[id] = entry;
				}

				if (!valid(value))
				{
					Quarantine(entry, value);
					line = $"{noun} #{id}: reading {Text(value)} is not valid - set aside, not read";
				}
				else if (_game != null)
				{
					_ = Transition(entry, _game);
					var confirms = clean && cleanFigure(value) != null && (entry.Current == null || entry.Current.Clean == 0)
						&& entry.Inherited != null;
					entry.Current ??= new RevisionLayer<TValue> { Game = _game, Method = method, Value = value };
					Add(entry.Current, value, clean);
					if (confirms)
					{
						line = $"{noun} #{id}: first clean reading in this game version, {Text(value)}"
							+ $" (taken over: {Text(entry.Inherited!.Value!)}) - confirmed";
					}
				}
				else
				{
					entry.Inherited ??= new RevisionLayer<TValue> { Game = string.Empty, Method = method, Value = value };
					Add(entry.Inherited, value, clean);
				}

				if (TryShow(entry, out var shown))
				{
					table()[id] = shown;
				}
			}

			if (line != null)
			{
				DefenseTrace.Line(line);
			}

			_ = SaveList(name);
		}

		/// <summary>The layers as they are now, for a save off this thread; values are shared, never changed in place.</summary>
		public (Dictionary<uint, RevisionEntry<TValue>> Entries, string? Game) Snapshot()
		{
			lock (_memory)
			{
				var copy = new Dictionary<uint, RevisionEntry<TValue>>(_entries.Count);
				foreach (var (id, entry) in _entries)
				{
					copy[id] = entry.Copy();
				}

				return (copy, _game);
			}
		}

		/// <summary>
		/// Merged with the file, checked against it, written, read back, and only then copied as the backup. Nothing is
		/// written that would lose an entry, a layer or a reading the file holds.
		/// </summary>
		public void Save((Dictionary<uint, RevisionEntry<TValue>> Entries, string? Game) snapshot)
		{
			if (!_writable)
			{
				_saved = $"NOT SAVED {DateTime.Now:HH:mm:ss}: " + (_loaded == NotLoaded
					? "the table was never loaded this session, revision file left as it is"
					: "the revision file is of a newer form, left as it is");
				return;
			}

			lock (_file)
			{
				var (entries, game) = snapshot;
				foreach (var entry in entries.Values)
				{
					_ = Transition(entry, game);
				}

				if (!TryReadOnDisk(out var onDisk))
				{
					_saved = $"NOT SAVED {DateTime.Now:HH:mm:ss}: the revision file could not be read to merge with, left as it is";
					return;
				}

				foreach (var entry in onDisk.Values)
				{
					_ = Transition(entry, game);
				}

				var merged = Merge(entries, onDisk);
				if (Broken(merged, onDisk) is { } broken)
				{
					_saved = $"SAVE INVARIANT {DateTime.Now:HH:mm:ss}: {broken} - not written, kept in memory";
					PluginLog.Warning($"{name}: {_saved}");
					DefenseTrace.Line($"{noun}: {_saved}");
					return;
				}

				var path = FilePath;
				var ok = WriteFile(path, JsonConvert.SerializeObject(new RevisionFile<TValue> { Format = RevisionFormat, Entries = merged },
					Formatting.Indented, SerializerSettings));
				var readBack = TryReadOnDisk(out var written) ? written.Count : -1;
				if (ok && readBack == merged.Count)
				{
					try
					{
						File.Copy(path, path + ".bak", true);
					}
					catch (Exception ex)
					{
						PluginLog.Warning($"Could not copy {path} as its backup: {ex.Message}");
					}

					_saved = $"saved {DateTime.Now:HH:mm:ss}: {merged.Count} {noun} entries written and read back";
				}
				else
				{
					_saved = !ok
						? $"SAVE FAILED {DateTime.Now:HH:mm:ss}: {merged.Count} in memory, revision file unchanged - see the log"
						: $"SAVE MISMATCH {DateTime.Now:HH:mm:ss}: {merged.Count} written, {readBack} read back";
				}
			}
		}

		/// <summary>The game version, the entries per state, and how the last load and save went.</summary>
		public string Summary()
		{
			lock (_memory)
			{
				var running = 0;
				var earlier = 0;
				var quarantined = 0;
				foreach (var entry in _entries.Values)
				{
					if (entry.Current != null && entry.Current.Game == _game && entry.Current.Method == method)
					{
						running++;
					}
					else if (entry.Current != null || entry.Inherited != null)
					{
						earlier++;
					}

					if (entry.Quarantined != null)
					{
						quarantined++;
					}
				}

				return $"game {_game ?? "NOT READABLE"}, method {method}: {running} measured in it, {earlier} from earlier versions"
					+ $" only, {quarantined} set aside as invalid; {_loaded}; {_saved}";
			}
		}

		// A version or method other than the running layer's: its readings join the taken-over layer. Not while the
		// version cannot be read - the running layer may well belong to the version that runs.
		private bool Transition(RevisionEntry<TValue> entry, string? game)
		{
			if (game == null || entry.Current is not { } current || (current.Game == game && current.Method == method))
			{
				return false;
			}

			entry.Inherited = Combine(entry.Inherited, current, true);
			entry.Current = null;
			return true;
		}

		// A value of the old file that adds to what the layers hold - unknown to them, or above their highest - joins the
		// taken-over layer as one clean reading without spread. The table's own save adds nothing the layers do not hold,
		// so this happens once, and again only for what an older RSR learned in between.
		private TakenOver TakeOver(Dictionary<uint, RevisionEntry<TValue>> entries, uint id, TValue value)
		{
			entries.TryGetValue(id, out var entry);
			if (!valid(value))
			{
				if (entry?.Quarantined?.Value == Text(value))
				{
					return TakenOver.Nothing;
				}

				entry ??= entries[id] = new RevisionEntry<TValue>();
				Quarantine(entry, value);
				return TakenOver.SetAside;
			}

			if (entry != null && TryHighest(entry, out var known) && Same(higher(known, value), known))
			{
				return TakenOver.Nothing;
			}

			entry ??= entries[id] = new RevisionEntry<TValue>();
			var taken = new RevisionLayer<TValue> { Game = string.Empty, Method = 0, Value = value };
			Add(taken, value, true);
			entry.Inherited = Combine(entry.Inherited, taken, entry.Inherited == null);
			return TakenOver.Value;
		}

		private enum TakenOver
		{
			Nothing,
			Value,
			SetAside,
		}

		private void Add(RevisionLayer<TValue> layer, TValue value, bool clean)
		{
			layer.Value = layer.Count == 0 ? value : higher(layer.Value!, value);
			layer.Count++;
			if (clean && cleanFigure(value) is { } figure)
			{
				layer.Low = layer.Clean == 0 ? figure : Math.Min(layer.Low, figure);
				layer.High = layer.Clean == 0 ? figure : Math.Max(layer.High, figure);
				layer.Clean++;
			}
		}

		// Readings of another history: counted together. The label is the younger one's at a version change, and the
		// layer's own when the old file's values join it.
		private RevisionLayer<TValue> Combine(RevisionLayer<TValue>? into, RevisionLayer<TValue> other, bool labelFromOther)
		{
			if (into == null)
			{
				return other.Copy();
			}

			var combined = into.Copy();
			combined.Value = higher(into.Value!, other.Value!);
			combined.Count += other.Count;
			Spread(combined, other);
			combined.Clean += other.Clean;
			if (labelFromOther)
			{
				combined.Game = other.Game;
				combined.Method = other.Method;
			}

			return combined;
		}

		// The same history seen from memory and from the file: the higher of each count, since a sum would count twice.
		private RevisionLayer<TValue> MergeSame(RevisionLayer<TValue> mine, RevisionLayer<TValue> theirs)
		{
			var merged = mine.Copy();
			merged.Value = higher(mine.Value!, theirs.Value!);
			merged.Count = Math.Max(mine.Count, theirs.Count);
			Spread(merged, theirs);
			merged.Clean = Math.Max(mine.Clean, theirs.Clean);
			return merged;
		}

		// Called before the clean counts are joined: a side without a clean reading has no figures to give.
		private static void Spread(RevisionLayer<TValue> into, RevisionLayer<TValue> other)
		{
			if (other.Clean == 0)
			{
				return;
			}

			if (into.Clean == 0)
			{
				into.Low = other.Low;
				into.High = other.High;
				return;
			}

			into.Low = Math.Min(into.Low, other.Low);
			into.High = Math.Max(into.High, other.High);
		}

		private Dictionary<uint, RevisionEntry<TValue>> Merge(Dictionary<uint, RevisionEntry<TValue>> mine,
			Dictionary<uint, RevisionEntry<TValue>> onDisk)
		{
			foreach (var (id, theirs) in onDisk)
			{
				if (!mine.TryGetValue(id, out var entry))
				{
					mine[id] = theirs;
					continue;
				}

				if (theirs.Current is { } current)
				{
					if (entry.Current == null)
					{
						entry.Current = current;
					}
					else if (entry.Current.Game == current.Game && entry.Current.Method == current.Method)
					{
						entry.Current = MergeSame(entry.Current, current);
					}
					else
					{
						// Two running layers of different revisions cannot both be running: the file's joins the taken-over.
						entry.Inherited = Combine(entry.Inherited, current, false);
					}
				}

				if (theirs.Inherited is { } inherited)
				{
					entry.Inherited = entry.Inherited == null ? inherited
						: entry.Inherited.Game == inherited.Game && entry.Inherited.Method == inherited.Method
							? MergeSame(entry.Inherited, inherited)
							// Different histories: counted together. One count too many in the taken-over layer only holds
							// a lowering back (rule 5); one too few would loosen it.
							: Combine(entry.Inherited, inherited, false);
				}

				if (theirs.Quarantined is { } set)
				{
					entry.Quarantined ??= set;
					entry.Quarantined.Count = Math.Max(entry.Quarantined.Count, set.Count);
				}
			}

			return mine;
		}

		// What a save may not do against the file it replaces (concept 13, rule 7): lose an entry, a layer, a set-aside
		// value or a reading, or lower the highest value.
		private string? Broken(Dictionary<uint, RevisionEntry<TValue>> merged, Dictionary<uint, RevisionEntry<TValue>> onDisk)
		{
			foreach (var (id, before) in onDisk)
			{
				if (!merged.TryGetValue(id, out var after))
				{
					return $"#{id} would be lost";
				}

				if ((before.Current != null || before.Inherited != null) && after.Current == null && after.Inherited == null)
				{
					return $"#{id} would lose its readings";
				}

				if (before.Quarantined != null && after.Quarantined == null)
				{
					return $"#{id} would lose its set-aside value";
				}

				if (TryHighest(before, out var was) && (!TryHighest(after, out var now) || !Same(higher(now, was), now)))
				{
					return $"#{id} would fall from {Text(was)}";
				}

				if (Readings(after) < Readings(before))
				{
					return $"#{id} would count {Readings(after)} readings instead of {Readings(before)}";
				}
			}

			return null;
		}

		private static int Readings(RevisionEntry<TValue> entry)
			=> (entry.Current?.Count ?? 0) + (entry.Inherited?.Count ?? 0) + (entry.Quarantined?.Count ?? 0);

		private bool TryHighest(RevisionEntry<TValue> entry, out TValue value) => TryShow(entry, out value);

		// What the readers see: the higher of the two layers. The set-aside value never.
		private bool TryShow(RevisionEntry<TValue> entry, out TValue value)
		{
			if (entry.Current is { } current && entry.Inherited is { } inherited)
			{
				value = higher(current.Value!, inherited.Value!);
				return true;
			}

			var layer = entry.Current ?? entry.Inherited;
			value = layer != null ? layer.Value! : default!;
			return layer != null;
		}

		// A layer without a value or with one no reader may take - a file edited by hand - is set aside, on both sides of
		// a merge alike, so the invariants compare like with like.
		private int Sanitize(Dictionary<uint, RevisionEntry<TValue>> entries)
		{
			var empty = new List<uint>();
			foreach (var (id, entry) in entries)
			{
				if (entry == null)
				{
					empty.Add(id);
				}
			}

			foreach (var id in empty)
			{
				_ = entries.Remove(id);
			}

			var setAside = empty.Count;
			foreach (var entry in entries.Values)
			{

				if (entry.Current is { } current && (current.Value is null || !valid(current.Value)))
				{
					if (current.Value is not null)
					{
						Quarantine(entry, current.Value);
					}

					entry.Current = null;
					setAside++;
				}

				if (entry.Inherited is { } inherited && (inherited.Value is null || !valid(inherited.Value)))
				{
					if (inherited.Value is not null)
					{
						Quarantine(entry, inherited.Value);
					}

					entry.Inherited = null;
					setAside++;
				}
			}

			return setAside;
		}

		private static void Quarantine(RevisionEntry<TValue> entry, TValue value)
		{
			entry.Quarantined ??= new RevisionQuarantine();
			entry.Quarantined.Value = Text(value);
			entry.Quarantined.Count++;
		}

		/// <returns>False when the file is there but cannot be read, or is of a newer form; true with nothing when there is none.</returns>
		private bool TryReadOnDisk(out Dictionary<uint, RevisionEntry<TValue>> entries)
		{
			entries = [];
			var path = FilePath;
			try
			{
				if (!File.Exists(path))
				{
					return true;
				}

				var file = JsonConvert.DeserializeObject<RevisionFile<TValue>>(File.ReadAllText(path), DeserializerSettings);
				if (file == null || file.Format > RevisionFormat)
				{
					return false;
				}

				entries = file.Entries ?? [];
				_ = Sanitize(entries);
				return true;
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Could not read {path}: {ex.Message}");
				return false;
			}
		}

		private static string Text(TValue value) => JsonConvert.SerializeObject(value, SerializerSettings);

		private static bool Same(TValue first, TValue second) => Text(first) == Text(second);
	}
}
