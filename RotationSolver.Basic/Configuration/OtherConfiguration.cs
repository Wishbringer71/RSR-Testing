using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using Newtonsoft.Json.Converters;
using RotationSolver.Basic.Resources;

namespace RotationSolver.Basic.Configuration;

internal class OtherConfiguration
{
	/// <markdown file="List" name="AoE" section="Actions">
	///
	/// RSR will use group mitigation if any enemy in the enmity list is casting
	/// one of the listed actions. Usually those actions are raid-wides.
	/// </markdown>
	public static HashSet<uint> HostileCastingArea = [];

	/// <markdown file="List" name="Tank Buster" section="Actions">
	///
	/// RSR will use mitigation on target (heal) or self (tank) if the target is currently
	/// being targeted by one of the listed actions.
	/// </markdown>
	public static HashSet<uint> HostileCastingTank = [];

	/// <markdown file="List" name="Knockback" section="Actions">
	///
	/// **Click on "Record knockback actions" at your own peril. Some duties expect you take the
	/// knockback in order to reach a proper safe-spot, like in Sil'dihn Subterrane (Savage).**
	///
	/// RSR will use anti-knockback actions when you would be hit by one of the listed actions.
	/// </markdown>
	public static HashSet<uint> HostileCastingKnockback = [];

	/// <summary>
	/// Tankbuster marker paths the fight has shown not to be followed by a hit on the marked member
	/// (concept 15, V2). Learned in play, never downloaded; a later hit after the same marker takes the
	/// path out again. Paths are kept in lower case.
	/// </summary>
	public static HashSet<string> TankbusterMarkerWithoutHit = [];

	/// <summary>
	/// What hit after a tankbuster marker, per marker entry (area and path, <see cref="DataCenter.TankbusterMarkerKey"/>):
	/// lets the tank plan meet a marker that no cast accompanies with the hit measured after it before (concept 09,
	/// A340). Learned in play, never downloaded; a confirmation by another action replaces an entry, one by the same
	/// action keeps the shorter time.
	/// </summary>
	public static Dictionary<string, TankbusterMarkerHit> TankbusterMarkerHits = [];

	/// <markdown file="List" name="Gaze/Stop" section="Actions">
	///
	/// If the target is casting one of the listed actions, RSR will stop casting
	/// in the seconds before the action is resolved
	/// <see cref="RotationSolver.Basic.Configuration.Configs._castingStop">here</see>.
	/// </markdown>
	public static HashSet<uint> HostileCastingStop = [];

	public static Dictionary<uint, string[]> NoHostileNames = [];
	public static Dictionary<uint, string[]> NoProvokeNames = [];

	/// <summary>
	/// Auto-recorded elemental weaknesses observed while in North Horn, keyed by NameId.
	/// </summary>
	public static Dictionary<uint, List<string>> NorthHornWeaknessRecords = [];

	/// <summary>
	/// Auto-recorded elemental weaknesses observed while in South Horn, keyed by NameId.
	/// </summary>
	public static Dictionary<uint, List<string>> SouthHornWeaknessRecords = [];

	/// <markdown file="List" name="Beneficial Positions" section="Map-Specific Settings">
	/// Adds a preferred location used for ground **healing** AoE abilities (example: Earthly Star).
	///
	/// You can add multiple locations, in case a boss fight moves you to another platform, like M4S - Wicked Thunder.
	/// </markdown>
	public static Dictionary<uint, Vector3[]> BeneficialPositions = [];

	/// <markdown file="List" name="Dispellable Debuffs" section="Statuses">
	///
	/// Listed statuses will be dispelled (Esuna) first before any
	/// other dispellable statuses.
	/// </markdown>
	public static HashSet<uint> DangerousStatus = [];

	/// <markdown file="List" name="Priority" section="Statuses">
	///
	/// If running in auto mode, if any enemy in your enmity list has this status,
	/// it will target them as priority.
	/// </markdown>
	public static HashSet<uint> PriorityStatus = [];

	/// <markdown file="List" name="Invulnerability" section="Statuses">
	///
	/// Ignores target if they have one of the statuses listed.
	/// </markdown>
	public static HashSet<uint> InvincibleStatus = [];

	/// <markdown file="List" name="No-Casting Debuffs" section="Statuses">
	///
	/// If you have any of the statuses listed, RSR will stop taking any actions.
	/// </markdown>
	public static HashSet<uint> NoCastingStatus = [];
	public static List<Job> DancePartnerPriority = [];
	public static List<Job> TheSpearPriority = [];
	public static List<Job> TheBalancePriority = [];
	public static List<Job> KardiaTankPriority = [];

	/// <summary>
	/// The largest share of a party member's maximum HP that each listed area action has ever been
	/// observed to take from a member who could survive it, keyed by action id - at most his whole
	/// maximum HP (<see cref="IsWithinWholeHealth"/>). Learned in play; not shipped with the plugin.
	/// </summary>
	/// <remarks>
	/// Kept beside <see cref="HostileCastingArea"/> rather than folded into it, and that is the whole
	/// reason this costs nothing to introduce: the list keeps its type, its stored format, its
	/// surface and the one UI method that draws four such lists. An action missing here is *unrated*,
	/// which is not the same as *small* - it behaves exactly as it did before, which is what keeps
	/// the 850 shipped entries from losing their mitigation overnight.
	///
	/// A share rather than an amount, so it does not age with item level or content sync. The highest
	/// value ever seen rather than the last, so one unmitigated observation sets the truth and later
	/// well-mitigated ones cannot talk it back down.
	///
	/// Read by <c>DataCenter.AreaCastIsWorthMitigating</c>, which decides whether an incoming area
	/// cast is worth a mitigation cooldown. Measured in play, evaluated in play, applied in play -
	/// and this file is the only thing that carries a reading past the end of a session. Without it
	/// every login would start from nothing and "rated after one clear" would hold only until the
	/// player logs out, which for a fight progged over several evenings means never. That is why the
	/// list reset leaves it alone, why a save merges with the file instead of replacing it, and why
	/// it is written through a temporary file rather than in place. There is no button to discard
	/// it: after a patch that changes how hard these actions hit, the file is deleted by hand with
	/// the game closed (owner's decision - a function that is not needed is one more way to fail).
	/// </remarks>
	public static Dictionary<uint, float> HostileCastingAreaPotential = [];

	/// <summary>
	/// What each tankbuster does to the one it hits, scaled back to unmitigated, as a share of that
	/// member's maximum HP - keyed by action id. Learned in play from every tankbuster seen on any
	/// player, not only on the user (concept 13, "Die Tankbuster-Tabelle").
	/// </summary>
	/// <remarks>
	/// The tankbuster counterpart of <see cref="HostileCastingAreaPotential"/>, stored the same way and
	/// through the same guarded path (<see cref="SaveTracked{T}"/>): merged with the file on every
	/// save, never written when its load did not finish, read back after writing, written through a
	/// temporary file, set aside as .corrupt when unreadable. Changed on the game thread under
	/// <see cref="TankbusterPotentialGate"/>, and every reader takes the same lock.
	///
	/// Unlike the area table it is not a raw observation: the measured share is divided by the
	/// mitigation that stood on the member and on the attacker at the hit
	/// (<see cref="DefensiveValues.TryStatus"/>), so a well-mitigated hit does not rate the
	/// action low. A hit taken under a vulnerability debuff goes into its own figure. Both only ever
	/// rise. No button discards it: after a patch the file is deleted by hand with the game closed.
	/// </remarks>
	public static Dictionary<uint, TankbusterReading> TankbusterPotential = [];

	/// <summary>Guards <see cref="TankbusterPotential"/> between the effect handler and its readers.</summary>
	public static readonly object TankbusterPotentialGate = new();

	/// <summary>
	/// What sleeps did to each enemy action they were put on, keyed by action id - learned in play over every session
	/// (concept 08, "Ein gegnerisches Wirken stoppen").
	/// </summary>
	/// <remarks>
	/// Stored like <see cref="HostileCastingAreaPotential"/>, his model for it (23.10.2026): merged with the file on
	/// every save, every figure only rising, never written when its load did not finish, set aside as .corrupt when
	/// unreadable. Changed on the game thread under <see cref="SleepStoreGate"/>, read there by the rule. No button
	/// discards it: the file is deleted by hand with the game closed.
	/// </remarks>
	internal static Dictionary<uint, SleepReading> SleepOutcomes = [];

	/// <summary>
	/// Enemy kinds (base id) that resisted a sleep as a status, with how often - learned in play over every session.
	/// Stored like <see cref="SleepOutcomes"/>.
	/// </summary>
	internal static Dictionary<uint, int> SleepResistantKinds = [];

	/// <summary>
	/// Per enemy kind (base id), the fewest sleeps it had taken in a fight before it resisted the next one - the
	/// point where the halving of repeated sleeps ends in a temporary immunity. Learned in play over every session,
	/// stored like <see cref="SleepOutcomes"/>; merged with the file by the lower count.
	/// </summary>
	internal static Dictionary<uint, int> SleepDiminishing = [];

	/// <summary>Guards <see cref="SleepOutcomes"/>, <see cref="SleepResistantKinds"/> and <see cref="SleepDiminishing"/>.</summary>
	internal static readonly object SleepStoreGate = new();

	/// <summary>
	/// How many times each enemy action has been seen to hit the player in a row, counted the way Liturgy of the Bell
	/// counts its triggers (<see cref="HitSeries"/>), keyed by action id - learned in play over every session.
	/// </summary>
	/// <remarks>
	/// Stored like <see cref="SleepResistantKinds"/>: merged with the file on every save by the higher count, never
	/// written when its load did not finish, set aside as .corrupt when unreadable. Changed on the game thread under
	/// <see cref="HitSeriesGate"/>. No button discards it: the file is deleted by hand with the game closed.
	/// </remarks>
	internal static Dictionary<uint, int> HitSeriesOnPlayer = [];

	/// <summary>Guards <see cref="HitSeriesOnPlayer"/>.</summary>
	internal static readonly object HitSeriesGate = new();

	public static RotationSolverRecord RotationSolverRecord = new();

	private abstract class ListFile(string name)
	{
		public string Name { get; } = name;

		public abstract string FilePath { get; }

		public abstract void Load();

		public abstract void Reset();

		public abstract string Serialize();

		/// <summary>
		/// Takes what is to be written on the caller's thread and returns the write for a pool thread.
		/// </summary>
		/// <remarks>
		/// The game thread changes several of these stores during play. Copied on a pool thread, a store
		/// could be read while the game thread adds to it - "Collection was modified" thrown out of the
		/// copy, and the save lost without a retry (A224). A store whose load did not finish is not
		/// written: it holds its empty default, and for a built-in list that diffs to "every entry
		/// removed" - the next start would read every list as empty (A196, A341).
		/// </remarks>
		public virtual Action PrepareWrite()
		{
			if (!WasLoaded(Name))
			{
				return static () => { };
			}

			var path = FilePath;
			var json = Serialize();
			return () => WriteFile(path, json);
		}
	}

	// A list that only exists in the user's config folder.
	private sealed class LocalFile<T>(string name, Func<T> get, Action<T> set) : ListFile(name) where T : class, new()
	{
		public override string FilePath => GetFilePath(Name);

		public override void Load()
		{
			set(ReadFile<T>(FilePath) ?? new T());
			MarkLoaded(Name);
		}

		public override void Reset() => set(new T());

		public override string Serialize() => JsonConvert.SerializeObject(get(), Formatting.Indented, SerializerSettings);
	}

	private abstract class BuiltInList<T, TChanges>(string name, T builtInList, Func<T> get, Action<T> set) : ListFile(name)
		where T : class, new()
		where TChanges : class
	{
		private T BuiltIn { get; } = builtInList;

		public override string FilePath => GetChangesPath(Name);

		public override void Load()
		{
			var changes = ReadFile<TChanges>(FilePath) ?? MigrateLegacyFile();
			set(changes == null ? Copy(BuiltIn) : Apply(BuiltIn, changes));
			MarkLoaded(Name);
		}

		public override void Reset() => set(Copy(BuiltIn));

		public override string Serialize() => JsonConvert.SerializeObject(Diff(BuiltIn, get()), Formatting.Indented, SerializerSettings);

		protected abstract T Copy(T value);

		protected abstract T Apply(T builtIn, TChanges changes);

		protected abstract TChanges Diff(T builtIn, T current);

		protected virtual TChanges? Migrate(T builtIn, T legacy) => null;

		private TChanges? MigrateLegacyFile()
		{
			var legacyPath = GetFilePath(Name);
			if (!File.Exists(legacyPath))
			{
				return null;
			}

			var legacy = ReadFile<T>(legacyPath);
			var changes = legacy == null ? null : Migrate(BuiltIn, legacy);
			if (changes != null)
			{
				WriteFile(FilePath, JsonConvert.SerializeObject(changes, Formatting.Indented, SerializerSettings));
				if (!File.Exists(FilePath))
				{
					return changes;
				}
			}

			try
			{
				File.Delete(legacyPath);
				PluginLog.Information($"Moved {Name} to the built-in list{(changes == null ? string.Empty : " and kept your additions")}.");
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Failed to delete the old {legacyPath}: {ex.Message}");
			}

			return changes;
		}
	}

	private sealed class SetChanges
	{
		public HashSet<uint> Added { get; set; } = [];
		public HashSet<uint> Removed { get; set; } = [];
	}

	private sealed class SetList(string name, uint[] builtInIds, Func<HashSet<uint>> get, Action<HashSet<uint>> set, params uint[] retired)
		: BuiltInList<HashSet<uint>, SetChanges>(name, [.. builtInIds], get, set)
	{
		protected override HashSet<uint> Copy(HashSet<uint> value) => [.. value];

		protected override HashSet<uint> Apply(HashSet<uint> builtIn, SetChanges changes)
		{
			var result = new HashSet<uint>(builtIn);
			result.ExceptWith(changes.Removed);
			result.UnionWith(changes.Added);
			return result;
		}

		protected override SetChanges Diff(HashSet<uint> builtIn, HashSet<uint> current)
		{
			var changes = new SetChanges();
			foreach (var id in current)
			{
				if (!builtIn.Contains(id))
				{
					_ = changes.Added.Add(id);
				}
			}

			foreach (var id in builtIn)
			{
				if (!current.Contains(id))
				{
					_ = changes.Removed.Add(id);
				}
			}

			return changes;
		}

		// A missing id may have joined the built-in list after the old file was downloaded, so only additions carry over.
		// Retired ids were once built in and later dropped, so an old file holding them got them from a download.
		protected override SetChanges Migrate(HashSet<uint> builtIn, HashSet<uint> legacy)
		{
			var changes = Diff(builtIn, legacy);
			changes.Removed.Clear();
			changes.Added.ExceptWith(retired);
			return changes;
		}
	}

	private sealed class MapChanges<TItem>
	{
		public Dictionary<uint, TItem[]> Added { get; set; } = [];
		public Dictionary<uint, TItem[]> Removed { get; set; } = [];
	}

	// Per-territory lists.
	private sealed class MapList<TItem>(string name, Dictionary<uint, TItem[]> builtInMap, Func<Dictionary<uint, TItem[]>> get, Action<Dictionary<uint, TItem[]>> set)
		: BuiltInList<Dictionary<uint, TItem[]>, MapChanges<TItem>>(name, builtInMap, get, set)
	{
		protected override Dictionary<uint, TItem[]> Copy(Dictionary<uint, TItem[]> value)
		{
			var result = new Dictionary<uint, TItem[]>(value.Count);
			foreach (var (key, items) in value)
			{
				result[key] = [.. items];
			}

			return result;
		}

		protected override Dictionary<uint, TItem[]> Apply(Dictionary<uint, TItem[]> builtIn, MapChanges<TItem> changes)
		{
			var result = Copy(builtIn);
			foreach (var (key, removed) in changes.Removed)
			{
				if (result.TryGetValue(key, out var items))
				{
					result[key] = Except(items, removed, false);
				}
			}

			foreach (var (key, added) in changes.Added)
			{
				result[key] = result.TryGetValue(key, out var items) ? [.. items, .. Except(added, items, true)] : [.. added];
			}

			return result;
		}

		protected override MapChanges<TItem> Diff(Dictionary<uint, TItem[]> builtIn, Dictionary<uint, TItem[]> current)
		{
			var changes = new MapChanges<TItem>();
			foreach (var (key, items) in current)
			{
				var added = Except(items, builtIn.GetValueOrDefault(key), true);
				if (added.Length > 0)
				{
					changes.Added[key] = added;
				}
			}

			foreach (var (key, items) in builtIn)
			{
				var removed = Except(items, current.GetValueOrDefault(key), true);
				if (removed.Length > 0)
				{
					changes.Removed[key] = removed;
				}
			}

			return changes;
		}

		protected override MapChanges<TItem> Migrate(Dictionary<uint, TItem[]> builtIn, Dictionary<uint, TItem[]> legacy)
		{
			var changes = Diff(builtIn, legacy);
			changes.Removed.Clear();
			return changes;
		}

		private static TItem[] Except(TItem[] items, TItem[]? other, bool skipBlank)
		{
			var result = new List<TItem>(items.Length);
			foreach (var item in items)
			{
				if ((skipBlank && item is null or string { Length: 0 })
					|| (other != null && Array.IndexOf(other, item) >= 0)
					|| result.Contains(item))
				{
					continue;
				}

				result.Add(item);
			}

			return [.. result];
		}
	}

	private sealed class OrderList(string name, Job[] builtInOrder, Func<List<Job>> get, Action<List<Job>> set)
		: BuiltInList<List<Job>, List<Job>>(name, [.. builtInOrder], get, set)
	{
		protected override List<Job> Copy(List<Job> value) => [.. value];

		protected override List<Job> Apply(List<Job> builtIn, List<Job> changes)
		{
			var result = new List<Job>(builtIn.Count);
			foreach (var job in changes)
			{
				if (builtIn.Contains(job) && !result.Contains(job))
				{
					result.Add(job);
				}
			}

			foreach (var job in builtIn)
			{
				if (!result.Contains(job))
				{
					result.Add(job);
				}
			}

			return result;
		}

		protected override List<Job> Diff(List<Job> builtIn, List<Job> current)
		{
			if (current.Count == builtIn.Count)
			{
				var same = true;
				for (var i = 0; i < current.Count && same; i++)
				{
					same = current[i] == builtIn[i];
				}

				if (same)
				{
					return [];
				}
			}

			return [.. current];
		}
	}

	/// <summary>
	/// A table learned in play: nothing to download, merged with its file on every save
	/// (<see cref="SaveTracked{T}"/>), and its load and save reported in words for the list window.
	/// </summary>
	/// <remarks>
	/// <paramref name="prune"/> removes what the table's own measurement would not store, and returns how
	/// many it removed; it runs in the same step as the load, because the loads run in parallel.
	/// </remarks>
	private sealed class LearnedFile<TValue>(string name, Func<Dictionary<uint, TValue>> copy,
		Action<Dictionary<uint, TValue>> set, Func<TValue, TValue, TValue> higher, Action<string> report,
		string noun, Func<TValue, bool>? keep = null, Func<Dictionary<uint, TValue>, int>? prune = null)
		: ListFile(name)
	{
		// One writer at a time. Every save of a store uses the same "<name>.json.tmp", and two pool
		// tasks writing it at once made the second one fail on the locked file - retried twice, and
		// on the third failure dropped.
		private readonly object _gate = new();

		public override string FilePath => GetFilePath(Name);

		public override void Load()
		{
			// "loaded 0", "file unreadable" and "no file yet" all leave an empty table behind, and only
			// the first of them is harmless - so the outcome is recorded for the list window.
			var path = FilePath;
			var existed = File.Exists(path);
			var value = ReadFile<Dictionary<uint, TValue>>(path) ?? [];
			MarkLoaded(Name);
			var pruned = prune?.Invoke(value) ?? 0;
			set(value);
			report(!existed
				? $"loaded {DateTime.Now:HH:mm:ss}: no file yet, started empty"
				: !File.Exists(path)
					? $"LOAD FAILED {DateTime.Now:HH:mm:ss}: file unreadable, set aside as .corrupt, started empty"
					: $"loaded {DateTime.Now:HH:mm:ss}: {value.Count} {noun} from file"
						+ (pruned > 0 ? $", {pruned} above a whole max HP left out" : string.Empty));
		}

		// Learned in play, never reset: there is no built-in table to return to, and no button discards it.
		public override void Reset()
		{
		}

		public override string Serialize() => JsonConvert.SerializeObject(copy(), Formatting.Indented, SerializerSettings);

		public override Action PrepareWrite()
		{
			var snapshot = copy();
			return () => SaveTracked(snapshot, Name, higher, report, _gate, noun, keep);
		}
	}

	private static readonly JsonSerializerSettings SerializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None,
	};

	private static readonly JsonSerializerSettings DeserializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None,
		Converters = [new StringEnumConverter()],
	};

	/// <summary>
	/// Every stored list this plugin keeps, in one place: <see cref="InitAsync"/> loads exactly these,
	/// <see cref="Save()"/> and <see cref="SaveList"/> write exactly these.
	/// </summary>
	/// <remarks>
	/// Load and save once carried a list each, and they drifted: <see cref="HostileCastingAreaPotential"/>
	/// was saved but loaded only in an entry point the plugin never called. The learned damage potentials
	/// were therefore never read back at login, and the first save of the session wrote the empty table
	/// over the file - nothing failed, nothing was logged, and the only symptom in play was mitigation
	/// falling back to "unrated". One list for both removes the failure mode rather than the instance of
	/// it: a store cannot reach a save path without a load path.
	/// </remarks>
	private static readonly ListFile[] Files =
	[
		new SetList(nameof(DangerousStatus), StatusLists.DangerousStatus, () => DangerousStatus, v => DangerousStatus = v),
		new SetList(nameof(PriorityStatus), StatusLists.PriorityStatus, () => PriorityStatus, v => PriorityStatus = v, 861, 1323, 2654, 3202),
		new SetList(nameof(InvincibleStatus), StatusLists.InvincibleStatus, () => InvincibleStatus, v => InvincibleStatus = v, 151, 198, 350, 385, 945, 1303, 3054),
		new OrderList(nameof(DancePartnerPriority), JobPriorityLists.DancePartnerPriority, () => DancePartnerPriority, v => DancePartnerPriority = v),
		new OrderList(nameof(TheSpearPriority), JobPriorityLists.TheSpearPriority, () => TheSpearPriority, v => TheSpearPriority = v),
		new OrderList(nameof(TheBalancePriority), JobPriorityLists.TheBalancePriority, () => TheBalancePriority, v => TheBalancePriority = v),
		new OrderList(nameof(KardiaTankPriority), JobPriorityLists.KardiaTankPriority, () => KardiaTankPriority, v => KardiaTankPriority = v),
		new MapList<string>(nameof(NoHostileNames), TerritoryLists.NoHostileNames, () => NoHostileNames, v => NoHostileNames = v),
		new MapList<string>(nameof(NoProvokeNames), TerritoryLists.NoProvokeNames, () => NoProvokeNames, v => NoProvokeNames = v),
		new SetList(nameof(HostileCastingArea), ActionLists.HostileCastingArea, () => HostileCastingArea, v => HostileCastingArea = v, 41147, 41763),
		// The game thread is the only writer of this table, and the copy is taken there.
		new LearnedFile<float>(nameof(HostileCastingAreaPotential),
			() => new Dictionary<uint, float>(HostileCastingAreaPotential), v => HostileCastingAreaPotential = v,
			Math.Max, state => AreaPotentialStoreState = state, "rated action(s)", IsWithinWholeHealth, DropBeyondWholeHealth),
		new LearnedFile<TankbusterReading>(nameof(TankbusterPotential), CopyTankbusterPotential,
			v => { lock (TankbusterPotentialGate) { TankbusterPotential = v; } },
			TankbusterReading.Higher, state => TankbusterStoreState = state, "rated tankbuster(s)"),
		new LearnedFile<SleepReading>(nameof(SleepOutcomes), () => CopySleepStore(SleepOutcomes),
			v => { lock (SleepStoreGate) { SleepOutcomes = v; } },
			SleepReading.Higher, state => SleepStoreState = state, "slept action(s)"),
		new LearnedFile<int>(nameof(SleepResistantKinds), () => CopySleepStore(SleepResistantKinds),
			v => { lock (SleepStoreGate) { SleepResistantKinds = v; } },
			Math.Max, state => SleepKindsStoreState = state, "resistant enemy kind(s)"),
		new LearnedFile<int>(nameof(SleepDiminishing), () => CopySleepStore(SleepDiminishing),
			v => { lock (SleepStoreGate) { SleepDiminishing = v; } },
			Math.Min, state => SleepDiminishingStoreState = state, "enemy kind(s) with a sleep limit"),
		new LearnedFile<int>(nameof(HitSeriesOnPlayer), CopyHitSeries,
			v => { lock (HitSeriesGate) { HitSeriesOnPlayer = v; } },
			Math.Max, state => HitSeriesStoreState = state, "enemy action(s) hitting in series"),
		new SetList(nameof(HostileCastingTank), ActionLists.HostileCastingTank, () => HostileCastingTank, v => HostileCastingTank = v,
			13364, 13365, 13367, 13371, 13375, 13385, 13388, 13438, 13444, 13452, 13454, 13462, 13475),
		new MapList<Vector3>(nameof(BeneficialPositions), TerritoryLists.BeneficialPositions, () => BeneficialPositions, v => BeneficialPositions = v),
		new LocalFile<RotationSolverRecord>(nameof(RotationSolverRecord), () => RotationSolverRecord, v => RotationSolverRecord = v),
		new SetList(nameof(NoCastingStatus), StatusLists.NoCastingStatus, () => NoCastingStatus, v => NoCastingStatus = v),
		new SetList(nameof(HostileCastingKnockback), ActionLists.HostileCastingKnockback, () => HostileCastingKnockback, v => HostileCastingKnockback = v),
		// Learned in play, like the damage tables: no built-in list.
		new LocalFile<HashSet<string>>(nameof(TankbusterMarkerWithoutHit), () => TankbusterMarkerWithoutHit, v => TankbusterMarkerWithoutHit = v),
		new LocalFile<Dictionary<string, TankbusterMarkerHit>>(nameof(TankbusterMarkerHits), () => TankbusterMarkerHits, v => TankbusterMarkerHits = v),
		new SetList(nameof(HostileCastingStop), ActionLists.HostileCastingStop, () => HostileCastingStop, v => HostileCastingStop = v, 3715, 3719, 3720, 3721, 3722),
		new LocalFile<Dictionary<uint, List<string>>>(nameof(NorthHornWeaknessRecords), () => NorthHornWeaknessRecords, v => NorthHornWeaknessRecords = v),
		new LocalFile<Dictionary<uint, List<string>>>(nameof(SouthHornWeaknessRecords), () => SouthHornWeaknessRecords, v => SouthHornWeaknessRecords = v),
	];

	private static Dictionary<uint, T> CopySleepStore<T>(Dictionary<uint, T> store)
	{
		lock (SleepStoreGate)
		{
			return new Dictionary<uint, T>(store);
		}
	}

	private static Dictionary<uint, int> CopyHitSeries()
	{
		lock (HitSeriesGate)
		{
			return new Dictionary<uint, int>(HitSeriesOnPlayer);
		}
	}

	private static Dictionary<uint, TankbusterReading> CopyTankbusterPotential()
	{
		lock (TankbusterPotentialGate)
		{
			return new Dictionary<uint, TankbusterReading>(TankbusterPotential);
		}
	}

	private static ListFile GetFile(string name)
	{
		foreach (var file in Files)
		{
			if (file.Name == name)
			{
				return file;
			}
		}

		throw new ArgumentException($"Unknown list file {name}", nameof(name));
	}

	/// <summary>
	/// Whether a learned area share is one the measurement stores: at most the member's whole maximum HP.
	/// </summary>
	/// <remarks>
	/// The measurement leaves out a hit beyond a member's health and barrier - it kills from full health
	/// whatever is done, the hit of a kill mechanic on the one who missed it - and counts one he survived
	/// only behind his barrier as his whole health (Watcher.TryAreaShare, A298). Anything above that in the
	/// file is a reading from before, Megadeath at 941 % among them; since the table only ever rises, it
	/// would otherwise stand for good, and on every save the merge would write it back.
	/// </remarks>
	internal static bool IsWithinWholeHealth(float share) => share <= 1f;

	private static int DropBeyondWholeHealth(Dictionary<uint, float> table)
	{
		var beyond = new List<uint>();
		foreach (var (id, share) in table)
		{
			if (!IsWithinWholeHealth(share))
			{
				beyond.Add(id);
			}
		}

		foreach (var id in beyond)
		{
			_ = table.Remove(id);
		}

		return beyond.Count;
	}

	public static async Task InitAsync(CancellationToken cancellationToken = default)
	{
		_ = Directory.CreateDirectory(GetChangesPath(string.Empty));

		var tasks = new Task[Files.Length];
		for (var i = 0; i < Files.Length; i++)
		{
			var file = Files[i];
			tasks[i] = Task.Run(file.Load, cancellationToken);
		}

		await Task.WhenAll(tasks);
	}

	public static Task Save()
	{
		var writes = new Action[Files.Length];
		for (var i = 0; i < Files.Length; i++)
		{
			writes[i] = Files[i].PrepareWrite();
		}

		return Task.Run(() =>
		{
			foreach (var write in writes)
			{
				// One failed write must not cost the ones after it: callers discard this task, so a
				// throw here would end the sequence without a trace (A224).
				try
				{
					write();
				}
				catch (Exception ex)
				{
					PluginLog.Warning($"Failed to save a list: {ex.Message}");
				}
			}
		});
	}

	// Writes one list file, so editing a list doesn't rewrite all of them.
	internal static Task SaveList(string name)
	{
		var write = GetFile(name).PrepareWrite();
		return Task.Run(write);
	}

	private static void ResetFile(string name)
	{
		var file = GetFile(name);
		file.Reset();
		// A reset is a state of its own, the built-in list: written even where the load did not finish.
		MarkLoaded(file.Name);
		_ = SaveList(name);
	}

	#region Action Tab
	public static void ResetHostileCastingArea() => ResetFile(nameof(HostileCastingArea));

	public static void ResetHostileCastingTank() => ResetFile(nameof(HostileCastingTank));

	public static void ResetHostileCastingKnockback() => ResetFile(nameof(HostileCastingKnockback));

	public static void ResetHostileCastingStop() => ResetFile(nameof(HostileCastingStop));

	public static Task SaveHostileCastingArea() => SaveList(nameof(HostileCastingArea));

	public static Task SaveHostileCastingAreaPotential() => SaveList(nameof(HostileCastingAreaPotential));

	public static Task SaveTankbusterPotential() => SaveList(nameof(TankbusterPotential));

	internal static Task SaveSleepOutcomes() => SaveList(nameof(SleepOutcomes));

	internal static Task SaveSleepResistantKinds() => SaveList(nameof(SleepResistantKinds));

	internal static Task SaveSleepDiminishing() => SaveList(nameof(SleepDiminishing));

	internal static Task SaveHitSeriesOnPlayer() => SaveList(nameof(HitSeriesOnPlayer));

	/// <summary>What the last save and load of <see cref="HitSeriesOnPlayer"/> did, in words.</summary>
	internal static string HitSeriesStoreState { get; private set; } = "not loaded yet";

	/// <summary>What the last save and load of <see cref="SleepDiminishing"/> did, in words.</summary>
	internal static string SleepDiminishingStoreState { get; private set; } = "not loaded yet";

	/// <summary>What the last save and load of <see cref="SleepOutcomes"/> did, in words.</summary>
	internal static string SleepStoreState { get; private set; } = "not loaded yet";

	/// <summary>What the last save and load of <see cref="SleepResistantKinds"/> did, in words.</summary>
	internal static string SleepKindsStoreState { get; private set; } = "not loaded yet";

	/// <summary>What the last save and load of the tankbuster table did, in words.</summary>
	public static string TankbusterStoreState { get; private set; } = "not loaded yet";

	/// <summary>
	/// What the last save and the last load of the learned damage table did, in words. Read by the
	/// list window, so a store that silently fails can be told apart from one that simply has not
	/// measured anything yet.
	/// </summary>
	public static string AreaPotentialStoreState { get; private set; } = "not loaded yet";

	/// <summary>
	/// The guarded save of a table learned in play. <paramref name="higher"/> merges a value in memory
	/// with the one on disk - each store only ever grows, so the file is a lower bound and nothing it
	/// holds is dropped, except a value <paramref name="keep"/> rejects: one the table's own measurement
	/// would not store, which the load left out as well. <paramref name="report"/> receives what
	/// happened, in words.
	/// </summary>
	private static void SaveTracked<T>(Dictionary<uint, T> snapshot, string name, Func<T, T, T> higher,
		Action<string> report, object gate, string noun, Func<T, bool>? keep = null)
	{
		if (!WasLoaded(name))
		{
			report($"NOT SAVED {DateTime.Now:HH:mm:ss}: the table was never loaded this session, file left as it is");
			return;
		}

		lock (gate)
		{
			// The table only ever grows by itself, so what is on disk is never less than true: a save
			// takes the higher value of memory and file for every action and drops nothing the file
			// holds. Memory can hold less than the file - a load that failed, a table that started
			// empty for any reason - and before this a single save then replaced weeks of readings
			// with the few taken since. To start over after a patch, the file is deleted by hand
			// while the game is closed - there is deliberately no button for it (owner's decision).
			if (!TryReadEntriesOnDisk<T>(name, out var onDiskBefore))
			{
				// A file that is there but cannot be read now - locked by another program, or broken -
				// is not overwritten by a save that could not merge with it. The reading stays in
				// memory and goes out with the next save.
				report($"NOT SAVED {DateTime.Now:HH:mm:ss}: the file could not be read to merge with, left as it is - see the log");
				return;
			}

			if (onDiskBefore != null)
			{
				foreach (var (id, onDisk) in onDiskBefore)
				{
					// Taken back from the file, a value the load left out would raise the reading measured
					// since, and the next load would leave out both.
					if (keep != null && !keep(onDisk))
					{
						continue;
					}

					snapshot[id] = snapshot.TryGetValue(id, out var inMemory) ? higher(inMemory, onDisk) : onDisk;
				}
			}

			var ok = WriteFile(GetFilePath(name), JsonConvert.SerializeObject(snapshot, Formatting.Indented, SerializerSettings));

			// Read back what is on disk rather than trusting the call. A save that "succeeded" but
			// left a file the loader cannot read, or one with fewer entries than were written, is
			// exactly the failure this store cannot afford, and only the file itself can say so.
			var readBack = CountEntriesOnDisk<T>(name);
			report(ok && readBack == snapshot.Count
				? $"saved {DateTime.Now:HH:mm:ss}: {snapshot.Count} {noun} written and read back"
				: !ok
					? $"SAVE FAILED {DateTime.Now:HH:mm:ss}: {snapshot.Count} in memory, file unchanged - see the log"
					: $"SAVE MISMATCH {DateTime.Now:HH:mm:ss}: {snapshot.Count} written, {readBack} read back");
		}
	}

	/// <returns>False when the file exists but could not be read; true with null when there is none.</returns>
	private static bool TryReadEntriesOnDisk<T>(string name, out Dictionary<uint, T>? entries)
	{
		entries = null;
		try
		{
			var path = GetFilePath(name);
			if (!File.Exists(path))
			{
				return true;
			}

			entries = JsonConvert.DeserializeObject<Dictionary<uint, T>>(File.ReadAllText(path));
			return entries != null;
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"Could not read {name} before saving: {ex.Message}");
			return false;
		}
	}

	private static int CountEntriesOnDisk<T>(string name)
	{
		try
		{
			var path = GetFilePath(name);
			if (!File.Exists(path))
			{
				return -1;
			}

			var read = JsonConvert.DeserializeObject<Dictionary<uint, T>>(File.ReadAllText(path));
			return read?.Count ?? -1;
		}
		catch
		{
			return -1;
		}
	}

	public static Task SaveHostileCastingTank() => SaveList(nameof(HostileCastingTank));

	public static Task SaveHostileCastingKnockback() => SaveList(nameof(HostileCastingKnockback));

	public static Task SaveTankbusterMarkerWithoutHit() => SaveList(nameof(TankbusterMarkerWithoutHit));

	public static Task SaveTankbusterMarkerHits() => SaveList(nameof(TankbusterMarkerHits));
	#endregion

	#region Status Tab
	public static void ResetPriorityStatus() => ResetFile(nameof(PriorityStatus));

	public static void ResetInvincibleStatus() => ResetFile(nameof(InvincibleStatus));

	public static void ResetDangerousStatus() => ResetFile(nameof(DangerousStatus));

	public static void ResetNoCastingStatus() => ResetFile(nameof(NoCastingStatus));
	#endregion

	public static Task SaveRotationSolverRecord() => SaveList(nameof(RotationSolverRecord));

	public static Task SaveNoProvokeNames() => SaveList(nameof(NoProvokeNames));

	public static Task SaveBeneficialPositions() => SaveList(nameof(BeneficialPositions));

	public static void ResetDancePartnerPriority() => ResetFile(nameof(DancePartnerPriority));

	public static void ResetTheSpearPriority() => ResetFile(nameof(TheSpearPriority));

	public static void ResetTheBalancePriority() => ResetFile(nameof(TheBalancePriority));

	public static void ResetKardiaTankPriority() => ResetFile(nameof(KardiaTankPriority));

	public static Task SaveDancePartnerPriority() => SaveList(nameof(DancePartnerPriority));

	public static Task SaveTheSpearPriority() => SaveList(nameof(TheSpearPriority));

	public static Task SaveTheBalancePriority() => SaveList(nameof(TheBalancePriority));

	public static Task SaveKardiaTankPriority() => SaveList(nameof(KardiaTankPriority));

	public static Task SaveNoHostileNames() => SaveList(nameof(NoHostileNames));

	#region Occult Crescent Weakness Tracking

	public static Task SaveNorthHornWeaknessRecords() => SaveList(nameof(NorthHornWeaknessRecords));

	public static Task SaveSouthHornWeaknessRecords() => SaveList(nameof(SouthHornWeaknessRecords));

	public static void ResetOccultWeaknessRecords()
	{
		NorthHornWeaknessRecords = [];
		SouthHornWeaknessRecords = [];
		_ = SaveNorthHornWeaknessRecords();
		_ = SaveSouthHornWeaknessRecords();
	}

	public static bool RecordOccultWeakness(uint nameId, StatusID weakness)
	{
		if (nameId == 0)
		{
			return false;
		}

		// Skip checking/recording entirely if this NameId is already manually curated.
		if (StatusHelper.HasKnownOccultWeakness(nameId))
		{
			return false;
		}

		var records = DataCenter.IsInNorthHorn
			? NorthHornWeaknessRecords
			: DataCenter.IsInSouthHorn
				? SouthHornWeaknessRecords
				: null;

		if (records == null)
		{
			return false;
		}

		if (!records.TryGetValue(nameId, out var list))
		{
			list = [];
			records[nameId] = list;
		}

		var weaknessName = weakness.ToString();
		if (list.Contains(weaknessName))
		{
			return false;
		}

		list.Add(weaknessName);
		return true;
	}

	#endregion

	private static string GetFilePath(string name)
	{
		return Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, $"{name}.json");
	}

	private static string GetChangesPath(string name)
	{
		var directory = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "ListChanges");
		return name.Length == 0 ? directory : Path.Combine(directory, $"{name}.json");
	}

	private static T? ReadFile<T>(string path) where T : class
	{
		if (!File.Exists(path))
		{
			return null;
		}

		try
		{
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), DeserializerSettings);
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"Failed to load {path}: {ex.Message}");

			// Keep the unreadable file instead of letting the next save overwrite it, and say so. A curated list
			// returns to its built-in entries; the learned damage potentials could not be had back, so the loss
			// must not be silent.
			try
			{
				var kept = path + ".corrupt";
				File.Move(path, kept, true);
				PluginLog.Warning($"Kept the unreadable {path} as {kept}.");
				_ = BasicWarningHelper.AddSystemWarning($"{Path.GetFileNameWithoutExtension(path)} could not be read and was set aside.");
			}
			catch (Exception moveEx)
			{
				PluginLog.Warning($"Could not set aside the unreadable {path}: {moveEx.Message}");
			}

			return null;
		}
	}

	/// <summary>
	/// Stores whose load has run to an end in this session. A store that was never loaded holds its
	/// empty default, and writing that default would replace the file with nothing.
	/// </summary>
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> _loadedStores = new();

	private static bool WasLoaded(string name)
	{
		if (_loadedStores.ContainsKey(name))
		{
			return true;
		}

		// The load did not finish - cancelled by the plugin's load timeout, or it threw - and the unload
		// writes every store. Without this every list and the learned table were emptied on disk by a load
		// that merely failed.
		PluginLog.Warning($"Not saving {name}: it was not loaded in this session, and its file is left as it is.");
		return false;
	}

	private static void MarkLoaded(string name)
	{
		_loadedStores[name] = true;
	}

	// Written to a temp file first, so a crash mid-write can't leave a half-written list.
	private static bool WriteFile(string path, string json)
	{
		const int retryCount = 3;
		for (var i = 0; i < retryCount; i++)
		{
			try
			{
				var tempPath = path + ".tmp";
				File.WriteAllText(tempPath, json);
				File.Move(tempPath, path, true);
				return true;
			}
			catch (IOException ex) when (i < retryCount - 1)
			{
				PluginLog.Warning($"Failed to save the file to {path}. Retrying in 1s...: {ex.Message}");
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Failed to save the file to {path}: {ex.Message}");
				return false;
			}
		}

		return false;
	}
}
