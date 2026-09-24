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
		var c = new HttpClient();
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
	/// observed to take, keyed by action id. Learned in play; not shipped with the plugin.
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
	/// list reset leaves it alone, why discarding it has its own button, and why it is written
	/// through a temporary file rather than in place.
	/// </remarks>
	public static Dictionary<uint, float> HostileCastingAreaPotential = [];

	public static RotationSolverRecord RotationSolverRecord = new();

	/// <summary>
	/// Every stored list this plugin loads, in one place.
	/// </summary>
	/// <remarks>
	/// <see cref="Init"/> and <see cref="InitAsync"/> used to carry a copy of this list each, and they
	/// drifted: <see cref="HostileCastingAreaPotential"/> was entered into Init only, while InitAsync
	/// is the one the plugin actually calls. The learned damage potentials were therefore never read
	/// back at login, and the first save of the session wrote the empty table over the file - so a
	/// measurement bought with a clear survived until the next logout and no further. Nothing failed,
	/// nothing was logged, and the only symptom in play was mitigation falling back to "unrated".
	///
	/// One list instead of two removes the failure mode rather than the instance of it: a store added
	/// here cannot reach a save path without a load path.
	/// </remarks>
	private static Action[] LoadSteps() =>
	[
		() => InitOne(ref DangerousStatus, nameof(DangerousStatus)),
		() => InitOne(ref PriorityStatus, nameof(PriorityStatus)),
		() => InitOne(ref InvincibleStatus, nameof(InvincibleStatus)),
		() => InitOne(ref DancePartnerPriority, nameof(DancePartnerPriority)),
		() => InitOne(ref TheSpearPriority, nameof(TheSpearPriority)),
		() => InitOne(ref TheBalancePriority, nameof(TheBalancePriority)),
		() => InitOne(ref KardiaTankPriority, nameof(KardiaTankPriority)),
		() => InitOne(ref NoHostileNames, nameof(NoHostileNames)),
		() => InitOne(ref NoProvokeNames, nameof(NoProvokeNames)),
		() => InitOne(ref HostileCastingArea, nameof(HostileCastingArea)),
		// No download: this one is learned in play and has no shipped counterpart to fetch.
		// The outcome is recorded for the list window: "loaded 0" and "file unreadable" and "no file
		// yet" all leave an empty table behind, and only the first of them is harmless.
		() =>
		{
			var path = GetFilePath(nameof(HostileCastingAreaPotential));
			var existed = File.Exists(path);
			InitOne(ref HostileCastingAreaPotential, nameof(HostileCastingAreaPotential), false);
			AreaPotentialStoreState = !existed
				? $"loaded {DateTime.Now:HH:mm:ss}: no file yet, started empty"
				: !File.Exists(path)
					? $"LOAD FAILED {DateTime.Now:HH:mm:ss}: file unreadable, set aside as .corrupt, started empty"
					: $"loaded {DateTime.Now:HH:mm:ss}: {HostileCastingAreaPotential.Count} rated action(s) from file";
		},
		() => InitOne(ref HostileCastingTank, nameof(HostileCastingTank)),
		() => InitOne(ref BeneficialPositions, nameof(BeneficialPositions)),
		() => InitOne(ref RotationSolverRecord, nameof(RotationSolverRecord), false),
		() => InitOne(ref NoCastingStatus, nameof(NoCastingStatus)),
		() => InitOne(ref HostileCastingKnockback, nameof(HostileCastingKnockback)),
		() => InitOne(ref HostileCastingStop, nameof(HostileCastingStop)),
		() => InitOne(ref NorthHornWeaknessRecords, nameof(NorthHornWeaknessRecords), false),
		() => InitOne(ref SouthHornWeaknessRecords, nameof(SouthHornWeaknessRecords), false),
	];

	private static void EnsureConfigDirectory()
	{
		if (!Directory.Exists(Svc.PluginInterface.ConfigDirectory.FullName))
		{
			_ = Directory.CreateDirectory(Svc.PluginInterface.ConfigDirectory.FullName);
		}
	}

	public static void Init()
	{
		EnsureConfigDirectory();

		foreach (var step in LoadSteps())
		{
			_ = Task.Run(step);
		}
	}

	public static async Task InitAsync(CancellationToken cancellationToken = default)
	{
		EnsureConfigDirectory();

		var steps = LoadSteps();
		var running = new Task[steps.Length];
		for (var i = 0; i < steps.Length; i++)
		{
			running[i] = Task.Run(steps[i], cancellationToken);
		}

		await Task.WhenAll(running);
	}

	public static Task Save()
	{
		return Task.Run(async () =>
		{
			await SavePriorityStatus();
			await SaveDangerousStatus();
			await SaveInvincibleStatus();
			await SaveDancePartnerPriority();
			await SaveTheSpearPriority();
			await SaveTheBalancePriority();
			await SaveKardiaTankPriority();
			await SaveNoHostileNames();
			await SaveHostileCastingArea();
			await SaveHostileCastingAreaPotential();
			await SaveHostileCastingTank();
			await SaveBeneficialPositions();
			await SaveRotationSolverRecord();
			await SaveNoProvokeNames();
			await SaveNoCastingStatus();
			await SaveHostileCastingKnockback();
			await SaveHostileCastingStop();
			await SaveNorthHornWeaknessRecords();
			await SaveSouthHornWeaknessRecords();
		});
	}
	#region Action Tab
	public static void ResetHostileCastingArea()
	{
		InitOne(ref HostileCastingArea, nameof(HostileCastingArea), true, true);
		SaveHostileCastingArea().Wait();
	}

	/// <summary>
	/// Discards everything learned about how hard the listed area actions hit.
	/// </summary>
	/// <remarks>
	/// Deliberately separate from <see cref="ResetHostileCastingArea"/>, which is the button users
	/// are told to press after every patch. Reloading the curated list is cheap - it is a download.
	/// The measurements are not: they cost runs in the game, and throwing them away with the list
	/// would mean starting from nothing every patch for the sake of the few actions that actually
	/// changed.
	///
	/// A rating left behind for an id the list no longer holds costs nothing, because every route
	/// that reads a rating goes through the list first.
	///
	/// What this button is for is the one case the highest-value rule cannot fix by itself. That rule
	/// only ever raises: an action rated too low corrects itself, since the mitigation is skipped and
	/// the next hit arrives unmitigated. An action that was *nerfed* keeps its old, too-high rating
	/// for good, and the only cost of that is mitigation spent where it is no longer needed - safe,
	/// but wrong. Clearing is the way out, and it is the user's call rather than an automatic decay:
	/// decay would undo the very property that makes one unmitigated observation worth keeping.
	/// </remarks>
	public static void ResetHostileCastingAreaPotential()
	{
		HostileCastingAreaPotential.Clear();
		SaveHostileCastingAreaPotential().Wait();
	}

	public static void ResetHostileCastingTank()
	{
		InitOne(ref HostileCastingTank, nameof(HostileCastingTank), true, true);
		SaveHostileCastingTank().Wait();
	}

	public static void ResetHostileCastingKnockback()
	{
		InitOne(ref HostileCastingKnockback, nameof(HostileCastingKnockback), true, true);
		SaveHostileCastingKnockback().Wait();
	}

	public static void ResetHostileCastingStop()
	{
		InitOne(ref HostileCastingStop, nameof(HostileCastingStop), true, true);
		SaveHostileCastingStop().Wait();
	}

	public static Task SaveHostileCastingArea()
	{
		return Task.Run(() => Save(HostileCastingArea, nameof(HostileCastingArea)));
	}

	public static Task SaveHostileCastingAreaPotential()
	{
		// The snapshot is taken HERE, on the caller's thread, and only the copy goes to the pool.
		//
		// The caller is the effect handler on the game thread, which is also the only writer of
		// this table. Handing the live dictionary to Task.Run serialised it on a pool thread while
		// the game thread could be adding the next reading: a Dictionary does not survive being
		// enumerated during a write, the serializer throws "Collection was modified", SavePath's
		// general catch logs a warning and returns - and that save is gone without a retry. The
		// reading stayed in memory, so it looked recorded, and reached the file only if a later
		// save came along. The last reading of a session had no later save.
		var snapshot = new Dictionary<uint, float>(HostileCastingAreaPotential);
		return Task.Run(() => SaveTracked(snapshot, nameof(HostileCastingAreaPotential)));
	}

	/// <summary>
	/// What the last save and the last load of the learned damage table did, in words. Read by the
	/// list window, so a store that silently fails can be told apart from one that simply has not
	/// measured anything yet.
	/// </summary>
	public static string AreaPotentialStoreState { get; private set; } = "not loaded yet";

	private static readonly object _areaPotentialSaveLock = new();

	private static void SaveTracked(Dictionary<uint, float> snapshot, string name)
	{
		// One writer at a time. Every save of a store uses the same "<name>.json.tmp", and two pool
		// tasks writing it at once made the second one fail on the locked file - retried twice, and
		// on the third failure dropped. Serialising them costs nothing here: a save is a few
		// hundred entries, and the order they land in is the order the readings were taken.
		lock (_areaPotentialSaveLock)
		{
			var ok = SavePath(snapshot, GetFilePath(name));

			// Read back what is on disk rather than trusting the call. A save that "succeeded" but
			// left a file the loader cannot read, or one with fewer entries than were written, is
			// exactly the failure this store cannot afford, and only the file itself can say so.
			var onDisk = CountEntriesOnDisk(name);
			AreaPotentialStoreState = ok && onDisk == snapshot.Count
				? $"saved {DateTime.Now:HH:mm:ss}: {snapshot.Count} rated action(s) written and read back"
				: !ok
					? $"SAVE FAILED {DateTime.Now:HH:mm:ss}: {snapshot.Count} in memory, file unchanged - see the log"
					: $"SAVE MISMATCH {DateTime.Now:HH:mm:ss}: {snapshot.Count} written, {onDisk} read back";
		}
	}

	private static int CountEntriesOnDisk(string name)
	{
		try
		{
			var path = GetFilePath(name);
			if (!File.Exists(path))
			{
				return -1;
			}

			var read = JsonConvert.DeserializeObject<Dictionary<uint, float>>(File.ReadAllText(path));
			return read?.Count ?? -1;
		}
		catch
		{
			return -1;
		}
	}

	public static Task SaveHostileCastingTank()
	{
		return Task.Run(() => Save(HostileCastingTank, nameof(HostileCastingTank)));
	}

	private static Task SaveHostileCastingKnockback()
	{
		return Task.Run(() => Save(HostileCastingKnockback, nameof(HostileCastingKnockback)));
	}

	private static Task SaveHostileCastingStop()
	{
		return Task.Run(() => Save(HostileCastingStop, nameof(HostileCastingStop)));
	}
	#endregion

	#region Status Tab

	public static void ResetPriorityStatus()
	{
		InitOne(ref PriorityStatus, nameof(PriorityStatus), true, true);
		SavePriorityStatus().Wait();
	}

	public static void ResetInvincibleStatus()
	{
		InitOne(ref InvincibleStatus, nameof(InvincibleStatus), true, true);
		SaveInvincibleStatus().Wait();
	}

	public static void ResetDangerousStatus()
	{
		InitOne(ref DangerousStatus, nameof(DangerousStatus), true, true);
		SaveDangerousStatus().Wait();
	}

	public static void ResetNoCastingStatus()
	{
		InitOne(ref NoCastingStatus, nameof(NoCastingStatus), true, true);
		SaveNoCastingStatus().Wait();
	}

	public static Task SavePriorityStatus()
	{
		return Task.Run(() => Save(PriorityStatus, nameof(PriorityStatus)));
	}

	public static Task SaveInvincibleStatus()
	{
		return Task.Run(() => Save(InvincibleStatus, nameof(InvincibleStatus)));
	}

	public static Task SaveDangerousStatus()
	{
		return Task.Run(() => Save(DangerousStatus, nameof(DangerousStatus)));
	}

	public static Task SaveNoCastingStatus()
	{
		return Task.Run(() => Save(NoCastingStatus, nameof(NoCastingStatus)));
	}

	#endregion
	public static Task SaveRotationSolverRecord()
	{
		return Task.Run(() => Save(RotationSolverRecord, nameof(RotationSolverRecord)));
	}

	public static Task SaveNoProvokeNames()
	{
		return Task.Run(() => Save(NoProvokeNames, nameof(NoProvokeNames)));
	}

	public static Task SaveBeneficialPositions()
	{
		return Task.Run(() => Save(BeneficialPositions, nameof(BeneficialPositions)));
	}

	public static void ResetDancePartnerPriority()
	{
		InitOne(ref DancePartnerPriority, nameof(DancePartnerPriority), true, true);
		SaveDancePartnerPriority().Wait();
	}

	public static void ResetTheSpearPriority()
	{
		InitOne(ref TheSpearPriority, nameof(TheSpearPriority), true, true);
		SaveTheSpearPriority().Wait();
	}

	public static void ResetTheBalancePriority()
	{
		InitOne(ref TheBalancePriority, nameof(TheBalancePriority), true, true);
		SaveTheBalancePriority().Wait();
	}

	public static void ResetKardiaTankPriority()
	{
		InitOne(ref KardiaTankPriority, nameof(KardiaTankPriority), true, true);
		SaveKardiaTankPriority().Wait();
	}

	public static Task SaveDancePartnerPriority()
	{
		return Task.Run(() => Save(DancePartnerPriority, nameof(DancePartnerPriority)));
	}

	public static Task SaveTheSpearPriority()
	{
		return Task.Run(() => Save(TheSpearPriority, nameof(TheSpearPriority)));
	}

	public static Task SaveTheBalancePriority()
	{
		return Task.Run(() => Save(TheBalancePriority, nameof(TheBalancePriority)));
	}

	public static Task SaveKardiaTankPriority()
	{
		return Task.Run(() => Save(KardiaTankPriority, nameof(KardiaTankPriority)));
	}

	public static Task SaveNoHostileNames()
	{
		return Task.Run(() => Save(NoHostileNames, nameof(NoHostileNames)));
	}

	#region Occult Crescent Weakness Tracking

	public static Task SaveNorthHornWeaknessRecords()
	{
		return Task.Run(() => Save(NorthHornWeaknessRecords, nameof(NorthHornWeaknessRecords)));
	}

	public static Task SaveSouthHornWeaknessRecords()
	{
		return Task.Run(() => Save(SouthHornWeaknessRecords, nameof(SouthHornWeaknessRecords)));
	}

	public static void ResetOccultWeaknessRecords()
	{
		NorthHornWeaknessRecords.Clear();
		SouthHornWeaknessRecords.Clear();
		SaveNorthHornWeaknessRecords().Wait();
		SaveSouthHornWeaknessRecords().Wait();
	}

	/// <summary>
	///
	/// </summary>
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
		var directory = Svc.PluginInterface.ConfigDirectory.FullName;

		return directory + $"\\{name}.json";
	}

	private static void Save<T>(T value, string name)
	{
		_ = SavePath(value, GetFilePath(name));
	}

	private static bool SavePath<T>(T value, string path)
	{
		var retryCount = 3;
		var delay = 1000; // 1 second delay

		for (var i = 0; i < retryCount; i++)
		{
			try
			{
				// Written to a temporary file and then moved into place, so an interruption cannot
				// leave a half-written file behind. WriteAllText truncates first and fills after: a
				// crash in between leaves JSON that no longer parses, and InitOne answers an
				// unreadable file by silently starting from empty - it does not re-download, because
				// the file exists.
				//
				// For the curated lists that costs a button press. For HostileCastingAreaPotential it
				// costs everything that was learned in play, and that store is written during combat,
				// on every new highest reading - which is exactly when a crash is most likely.
				var temp = path + ".tmp";
				File.WriteAllText(temp,
				JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.None,
				}));
				File.Move(temp, path, true);
				return true; // Exit the method if successful
			}
			catch (IOException ex) when (i < retryCount - 1)
			{
				PluginLog.Warning($"Failed to save the file to {path}. Retrying in {delay}ms...: {ex.Message}");
				Thread.Sleep(delay); // Wait before retrying
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Failed to save the file to {path}: {ex.Message}");
				return false; // Exit the method if an unexpected exception occurs
			}
		}

		return false;
	}

	private static void InitOne<T>(ref T value, string name, bool download = true, bool forceDownload = false) where T : new()
	{
		var path = GetFilePath(name);
		PluginLog.Information($"Initializing {name} from {path}");

		if (File.Exists(path) && !forceDownload)
		{
			try
			{
				value = JsonConvert.DeserializeObject<T>(File.ReadAllText(path), new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.None,
					Converters = [new StringEnumConverter()] // Add this line
				})! ?? throw new Exception("Deserialized value is null.");
				PluginLog.Information($"Loaded {name} from local file.");
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Failed to load {name} from local file. Reinitializing to default: {ex.Message}");
				value = new T(); // Reinitialize to default

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
		}
		else if (download || forceDownload)
		{
			try
			{
				var url = $"https://raw.githubusercontent.com/{Service.USERNAME}/{Service.REPO}/main/Resources/{name}.json";
				var str = Http.GetStringAsync(url).Result;

				File.WriteAllText(path, str);
				value = JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.None,
					Converters = [new StringEnumConverter()] // Add this line
				})! ?? throw new Exception("Deserialized value is null.");

				PluginLog.Information($"Downloaded and loaded {name} from GitHub.");
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Failed to download {name} from GitHub. Reinitializing to default. Exception: {ex.Message}");
				_ = BasicWarningHelper.AddSystemWarning($"Github download failed.");
				value = new T(); // Reinitialize to default
				_ = SavePath(value, path); // Save the default value
			}
		}
		else
		{
			value = new T(); // Reinitialize to default
			_ = SavePath(value, path); // Save the default value
		}
	}
}
