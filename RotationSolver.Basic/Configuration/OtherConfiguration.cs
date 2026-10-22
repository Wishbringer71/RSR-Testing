using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using Newtonsoft.Json.Converters;

namespace RotationSolver.Basic.Configuration;

internal class OtherConfiguration
{
	private static readonly HttpClient Http = CreateHttpClient();
	private static HttpClient CreateHttpClient()
	{
		var c = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
		try
		{
			c.DefaultRequestHeaders.UserAgent.ParseAdd("RotationSolver");
			c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
		}
		catch { /* best-effort headers */ }
		return c;
	}
	/// <markdown file="List" name="AoE" section="Actions">
	/// **`It is recommended to click on the reset button after every patch.`**
	///
	/// RSR will use group mitigation if any enemy in the enmity list is casting
	/// one of the listed actions. Usually those actions are raid-wides.
	/// </markdown>
	public static HashSet<uint> HostileCastingArea = [];

	/// <markdown file="List" name="Tank Buster" section="Actions">
	/// **`It is recommended to click on the reset button after every patch.`**
	///
	/// RSR will use mitigation on target (heal) or self (tank) if the target is currently
	/// being targeted by one of the listed actions.
	/// </markdown>
	public static HashSet<uint> HostileCastingTank = [];

	/// <markdown file="List" name="Knockback" section="Actions">
	/// **`It is recommended to click on the reset button after every patch.`**
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

	/// <markdown file="List" name="Gaze/Stop" section="Actions">
	/// **`It is recommended to click on the reset button after every patch.`**
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
	/// **`It is recommended to click on the reset button after every patch.`**
	///
	/// Listed statuses will be dispelled (Esuna) first before any
	/// other dispellable statuses.
	/// </markdown>
	public static HashSet<uint> DangerousStatus = [];

	/// <markdown file="List" name="Priority" section="Statuses">
	/// **`It is recommended to click on the reset button after every patch.`**
	///
	/// If running in auto mode, if any enemy in your enmity list has this status,
	/// it will target them as priority.
	/// </markdown>
	public static HashSet<uint> PriorityStatus = [];

	/// <markdown file="List" name="Invulnerability" section="Statuses">
	/// **`It is recommended to click on the reset button after every patch.`**
	///
	/// Ignores target if they have one of the statuses listed.
	/// </markdown>
	public static HashSet<uint> InvincibleStatus = [];

	/// <markdown file="List" name="No-Casting Debuffs" section="Statuses">
	/// **`It is recommended to click on the reset button after every patch.`**
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

	public static RotationSolverRecord RotationSolverRecord = new();

	private abstract class ListFile(string name, bool download)
	{
		public string Name { get; } = name;
		public bool Download { get; } = download;

		public abstract void Load(bool forceDownload);

		/// <summary>
		/// Takes what is to be written on the caller's thread and returns the write for a pool thread.
		/// </summary>
		/// <remarks>
		/// The game thread changes several of these stores during play. Copied on a pool thread, a store
		/// could be read while the game thread adds to it - "Collection was modified" thrown out of the
		/// copy, and the save lost without a retry (A224).
		/// </remarks>
		public abstract Action PrepareWrite();
	}

	private sealed class ListFile<T>(string name, Func<T> get, Action<T> set, bool download = true) : ListFile(name, download) where T : new()
	{
		public override void Load(bool forceDownload)
		{
			set(InitOne(get(), Name, Download, forceDownload));
		}

		public override Action PrepareWrite()
		{
			if (!WasLoaded(Name))
			{
				return static () => { };
			}

			var path = GetFilePath(Name);
			var json = JsonConvert.SerializeObject(get(), Formatting.Indented, SerializerSettings);
			return () => WriteFile(path, json);
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
		: ListFile(name, false)
	{
		// One writer at a time. Every save of a store uses the same "<name>.json.tmp", and two pool
		// tasks writing it at once made the second one fail on the locked file - retried twice, and
		// on the third failure dropped.
		private readonly object _gate = new();

		public override void Load(bool forceDownload)
		{
			// "loaded 0", "file unreadable" and "no file yet" all leave an empty table behind, and only
			// the first of them is harmless - so the outcome is recorded for the list window.
			var path = GetFilePath(Name);
			var existed = File.Exists(path);
			var value = InitOne(new Dictionary<uint, TValue>(), Name, false, false);
			var pruned = prune?.Invoke(value) ?? 0;
			set(value);
			report(!existed
				? $"loaded {DateTime.Now:HH:mm:ss}: no file yet, started empty"
				: !File.Exists(path)
					? $"LOAD FAILED {DateTime.Now:HH:mm:ss}: file unreadable, set aside as .corrupt, started empty"
					: $"loaded {DateTime.Now:HH:mm:ss}: {value.Count} {noun} from file"
						+ (pruned > 0 ? $", {pruned} above a whole max HP left out" : string.Empty));
		}

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
		new ListFile<HashSet<uint>>(nameof(DangerousStatus), () => DangerousStatus, v => DangerousStatus = v),
		new ListFile<HashSet<uint>>(nameof(PriorityStatus), () => PriorityStatus, v => PriorityStatus = v),
		new ListFile<HashSet<uint>>(nameof(InvincibleStatus), () => InvincibleStatus, v => InvincibleStatus = v),
		new ListFile<List<Job>>(nameof(DancePartnerPriority), () => DancePartnerPriority, v => DancePartnerPriority = v),
		new ListFile<List<Job>>(nameof(TheSpearPriority), () => TheSpearPriority, v => TheSpearPriority = v),
		new ListFile<List<Job>>(nameof(TheBalancePriority), () => TheBalancePriority, v => TheBalancePriority = v),
		new ListFile<List<Job>>(nameof(KardiaTankPriority), () => KardiaTankPriority, v => KardiaTankPriority = v),
		new ListFile<Dictionary<uint, string[]>>(nameof(NoHostileNames), () => NoHostileNames, v => NoHostileNames = v),
		new ListFile<Dictionary<uint, string[]>>(nameof(NoProvokeNames), () => NoProvokeNames, v => NoProvokeNames = v),
		new ListFile<HashSet<uint>>(nameof(HostileCastingArea), () => HostileCastingArea, v => HostileCastingArea = v),
		// The game thread is the only writer of this table, and the copy is taken there.
		new LearnedFile<float>(nameof(HostileCastingAreaPotential),
			() => new Dictionary<uint, float>(HostileCastingAreaPotential), v => HostileCastingAreaPotential = v,
			Math.Max, state => AreaPotentialStoreState = state, "rated action(s)", IsWithinWholeHealth, DropBeyondWholeHealth),
		new LearnedFile<TankbusterReading>(nameof(TankbusterPotential), CopyTankbusterPotential,
			v => { lock (TankbusterPotentialGate) { TankbusterPotential = v; } },
			TankbusterReading.Higher, state => TankbusterStoreState = state, "rated tankbuster(s)"),
		new ListFile<HashSet<uint>>(nameof(HostileCastingTank), () => HostileCastingTank, v => HostileCastingTank = v),
		new ListFile<Dictionary<uint, Vector3[]>>(nameof(BeneficialPositions), () => BeneficialPositions, v => BeneficialPositions = v),
		new ListFile<RotationSolverRecord>(nameof(RotationSolverRecord), () => RotationSolverRecord, v => RotationSolverRecord = v, false),
		new ListFile<HashSet<uint>>(nameof(NoCastingStatus), () => NoCastingStatus, v => NoCastingStatus = v),
		new ListFile<HashSet<uint>>(nameof(HostileCastingKnockback), () => HostileCastingKnockback, v => HostileCastingKnockback = v),
		// No download: learned in play, like the damage tables.
		new ListFile<HashSet<string>>(nameof(TankbusterMarkerWithoutHit), () => TankbusterMarkerWithoutHit, v => TankbusterMarkerWithoutHit = v, false),
		new ListFile<HashSet<uint>>(nameof(HostileCastingStop), () => HostileCastingStop, v => HostileCastingStop = v),
		new ListFile<Dictionary<uint, List<string>>>(nameof(NorthHornWeaknessRecords), () => NorthHornWeaknessRecords, v => NorthHornWeaknessRecords = v, false),
		new ListFile<Dictionary<uint, List<string>>>(nameof(SouthHornWeaknessRecords), () => SouthHornWeaknessRecords, v => SouthHornWeaknessRecords = v, false),
	];

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
		if (!Directory.Exists(Svc.PluginInterface.ConfigDirectory.FullName))
		{
			_ = Directory.CreateDirectory(Svc.PluginInterface.ConfigDirectory.FullName);
		}

		_loadToken = cancellationToken;

		var tasks = new Task[Files.Length];
		for (var i = 0; i < Files.Length; i++)
		{
			var file = Files[i];
			tasks[i] = Task.Run(() => file.Load(false), cancellationToken);
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

	// Downloads in the background so the game thread isn't blocked by the request.
	private static void ResetFile(string name)
	{
		var file = GetFile(name);
		_ = Task.Run(() => file.Load(true));
	}

	#region Action Tab
	public static void ResetHostileCastingArea() => ResetFile(nameof(HostileCastingArea));

	public static void ResetHostileCastingTank() => ResetFile(nameof(HostileCastingTank));

	public static void ResetHostileCastingKnockback() => ResetFile(nameof(HostileCastingKnockback));

	public static void ResetHostileCastingStop() => ResetFile(nameof(HostileCastingStop));

	public static Task SaveHostileCastingArea() => SaveList(nameof(HostileCastingArea));

	public static Task SaveHostileCastingAreaPotential() => SaveList(nameof(HostileCastingAreaPotential));

	public static Task SaveTankbusterPotential() => SaveList(nameof(TankbusterPotential));

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

		// The load did not finish - cancelled by the plugin's load timeout, a download that failed, or
		// it threw - and the unload writes every store. Without this every list and the learned table
		// were emptied on disk by a load that merely failed.
		PluginLog.Warning($"Not saving {name}: it was not loaded in this session, and its file is left as it is.");
		return false;
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

	/// <summary>
	/// The plugin's load timeout, handed in by <see cref="InitAsync"/>. A download at load is bounded
	/// by it as well as by the client's own timeout, so a load that has given up does not wait on it.
	/// </summary>
	private static CancellationToken _loadToken;

	private static T InitOne<T>(T current, string name, bool download, bool forceDownload) where T : new()
	{
		var path = GetFilePath(name);
		PluginLog.Information($"Initializing {name} from {path}");

		if (File.Exists(path) && !forceDownload)
		{
			try
			{
				var value = JsonConvert.DeserializeObject<T>(File.ReadAllText(path), DeserializerSettings)
					?? throw new Exception("Deserialized value is null.");
				PluginLog.Information($"Loaded {name} from local file.");
				_loadedStores[name] = true;
				return value;
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Failed to load {name} from local file. Reinitializing to default: {ex.Message}");

				// Keep the unreadable file instead of letting the next save overwrite it, and say so.
				// Losing a curated list this way is recoverable with the reset button; losing the
				// learned damage potentials is not, so the loss must not be silent.
				try
				{
					var kept = path + ".corrupt";
					File.Move(path, kept, true);
					PluginLog.Warning($"Kept the unreadable {name} as {kept}.");
					_ = BasicWarningHelper.AddSystemWarning($"{name} could not be read and was set aside.");
				}
				catch (Exception moveEx)
				{
					PluginLog.Warning($"Could not set aside the unreadable {name}: {moveEx.Message}");
				}
			}

			// An unreadable curated list is fetched again below, the same as a missing one (A196). It
			// used to start empty and stay so: the empty default was saved, and a file that exists is
			// never downloaded.
		}

		if (!download && !forceDownload)
		{
			// Nothing to fetch: a store learned or recorded in play starts empty. No default is written,
			// so a store set aside above reads as "file unreadable" in the list window, not as "loaded 0".
			_loadedStores[name] = true;
			return new T();
		}

		try
		{
			var url = $"https://raw.githubusercontent.com/{Service.USERNAME}/{Service.REPO}/main/Resources/{name}.json";
			var str = Http.GetStringAsync(url, forceDownload ? CancellationToken.None : _loadToken).GetAwaiter().GetResult();
			var value = JsonConvert.DeserializeObject<T>(str, DeserializerSettings)
				?? throw new Exception("Deserialized value is null.");

			// Only a list that parsed is written, so a bad download never replaces the local copy.
			_ = WriteFile(path, str);
			PluginLog.Information($"Downloaded and loaded {name} from GitHub.");
			_loadedStores[name] = true;
			return value;
		}
		catch (Exception ex)
		{
			_ = BasicWarningHelper.AddSystemWarning($"Github download failed.");

			// A reset asked to replace a working list with a fresh one. When the fresh one cannot be had,
			// the working one stays, and with it whether it counts as loaded: emptying it here wrote an
			// empty list over the file, which switched off every rule reading it - and the damage
			// measurement with it, since only listed actions are measured.
			if (forceDownload)
			{
				PluginLog.Warning($"Failed to download {name} from GitHub. Keeping the current list. Exception: {ex.Message}");
				return current;
			}

			// No list could be had. Nothing is written and the store does not count as loaded, so no
			// save writes the empty default either: an empty file would be read as the list on every
			// later start and never be fetched again - for the AoE list that meant no party mitigation
			// by list and no damage measurement until a reset happened to succeed (A196). The next start
			// tries again. The cost: changes made to this list in this session are not saved; a warning
			// in the log says so at every save.
			PluginLog.Warning($"Failed to download {name} from GitHub. Starting empty for this session, and trying again at the next start. Exception: {ex.Message}");
			return new T();
		}
	}
}
