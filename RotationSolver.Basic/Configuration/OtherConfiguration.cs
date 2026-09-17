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
	/// Nothing reads this yet. It is filled first and used second, on purpose: the rating only helps
	/// once entries have left the unrated state, and that takes runs in the game rather than work
	/// here. In repeated content - an extreme trial, a savage tier being progged - that is one clear
	/// of the fight.
	/// </remarks>
	public static Dictionary<uint, float> HostileCastingAreaPotential = [];

	public static RotationSolverRecord RotationSolverRecord = new();

	public static void Init()
	{
		if (!Directory.Exists(Svc.PluginInterface.ConfigDirectory.FullName))
		{
			_ = Directory.CreateDirectory(Svc.PluginInterface.ConfigDirectory.FullName);
		}

		_ = Task.Run(() => InitOne(ref DangerousStatus, nameof(DangerousStatus)));
		_ = Task.Run(() => InitOne(ref PriorityStatus, nameof(PriorityStatus)));
		_ = Task.Run(() => InitOne(ref InvincibleStatus, nameof(InvincibleStatus)));
		_ = Task.Run(() => InitOne(ref DancePartnerPriority, nameof(DancePartnerPriority)));
		_ = Task.Run(() => InitOne(ref TheSpearPriority, nameof(TheSpearPriority)));
		_ = Task.Run(() => InitOne(ref TheBalancePriority, nameof(TheBalancePriority)));
		_ = Task.Run(() => InitOne(ref KardiaTankPriority, nameof(KardiaTankPriority)));
		_ = Task.Run(() => InitOne(ref NoHostileNames, nameof(NoHostileNames)));
		_ = Task.Run(() => InitOne(ref NoProvokeNames, nameof(NoProvokeNames)));
		_ = Task.Run(() => InitOne(ref HostileCastingArea, nameof(HostileCastingArea)));
		// No download: this one is learned in play and has no shipped counterpart to fetch.
		_ = Task.Run(() => InitOne(ref HostileCastingAreaPotential, nameof(HostileCastingAreaPotential), false));
		_ = Task.Run(() => InitOne(ref HostileCastingTank, nameof(HostileCastingTank)));
		_ = Task.Run(() => InitOne(ref BeneficialPositions, nameof(BeneficialPositions)));
		_ = Task.Run(() => InitOne(ref RotationSolverRecord, nameof(RotationSolverRecord), false));
		_ = Task.Run(() => InitOne(ref NoCastingStatus, nameof(NoCastingStatus)));
		_ = Task.Run(() => InitOne(ref HostileCastingKnockback, nameof(HostileCastingKnockback)));
		_ = Task.Run(() => InitOne(ref HostileCastingStop, nameof(HostileCastingStop)));
		_ = Task.Run(() => InitOne(ref NorthHornWeaknessRecords, nameof(NorthHornWeaknessRecords), false));
		_ = Task.Run(() => InitOne(ref SouthHornWeaknessRecords, nameof(SouthHornWeaknessRecords), false));
	}

	public static async Task InitAsync(CancellationToken cancellationToken = default)
	{
		if (!Directory.Exists(Svc.PluginInterface.ConfigDirectory.FullName))
		{
			_ = Directory.CreateDirectory(Svc.PluginInterface.ConfigDirectory.FullName);
		}

		await Task.WhenAll(
			Task.Run(() => InitOne(ref DangerousStatus, nameof(DangerousStatus)), cancellationToken),
			Task.Run(() => InitOne(ref PriorityStatus, nameof(PriorityStatus)), cancellationToken),
			Task.Run(() => InitOne(ref InvincibleStatus, nameof(InvincibleStatus)), cancellationToken),
			Task.Run(() => InitOne(ref DancePartnerPriority, nameof(DancePartnerPriority)), cancellationToken),
			Task.Run(() => InitOne(ref TheSpearPriority, nameof(TheSpearPriority)), cancellationToken),
			Task.Run(() => InitOne(ref TheBalancePriority, nameof(TheBalancePriority)), cancellationToken),
			Task.Run(() => InitOne(ref KardiaTankPriority, nameof(KardiaTankPriority)), cancellationToken),
			Task.Run(() => InitOne(ref NoHostileNames, nameof(NoHostileNames)), cancellationToken),
			Task.Run(() => InitOne(ref NoProvokeNames, nameof(NoProvokeNames)), cancellationToken),
			Task.Run(() => InitOne(ref HostileCastingArea, nameof(HostileCastingArea)), cancellationToken),
			Task.Run(() => InitOne(ref HostileCastingTank, nameof(HostileCastingTank)), cancellationToken),
			Task.Run(() => InitOne(ref BeneficialPositions, nameof(BeneficialPositions)), cancellationToken),
			Task.Run(() => InitOne(ref RotationSolverRecord, nameof(RotationSolverRecord), false), cancellationToken),
			Task.Run(() => InitOne(ref NoCastingStatus, nameof(NoCastingStatus)), cancellationToken),
			Task.Run(() => InitOne(ref HostileCastingKnockback, nameof(HostileCastingKnockback)), cancellationToken),
			Task.Run(() => InitOne(ref HostileCastingStop, nameof(HostileCastingStop)), cancellationToken),
			Task.Run(() => InitOne(ref NorthHornWeaknessRecords, nameof(NorthHornWeaknessRecords), false), cancellationToken),
			Task.Run(() => InitOne(ref SouthHornWeaknessRecords, nameof(SouthHornWeaknessRecords), false), cancellationToken)
		);
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
		return Task.Run(() => Save(HostileCastingAreaPotential, nameof(HostileCastingAreaPotential)));
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
		SavePath(value, GetFilePath(name));
	}

	private static void SavePath<T>(T value, string path)
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
				return; // Exit the method if successful
			}
			catch (IOException ex) when (i < retryCount - 1)
			{
				PluginLog.Warning($"Failed to save the file to {path}. Retrying in {delay}ms...: {ex.Message}");
				Thread.Sleep(delay); // Wait before retrying
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Failed to save the file to {path}: {ex.Message}");
				return; // Exit the method if an unexpected exception occurs
			}
		}
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
				SavePath(value, path); // Save the default value
			}
		}
		else
		{
			value = new T(); // Reinitialize to default
			SavePath(value, path); // Save the default value
		}
	}
}
