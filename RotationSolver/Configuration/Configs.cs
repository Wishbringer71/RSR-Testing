using Dalamud.Configuration;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using RebornMaterial;
using System.Collections.Concurrent;
using static RotationSolver.Configuration.ConfigTypes;

namespace RotationSolver.Configuration;

internal partial class Configs : IPluginConfiguration, IM3Settings
{
	[JsonIgnore]
	public const string
		BasicTimer = "BasicTimer",
		BasicAutoSwitch = "BasicAutoSwitch",
		DutySpecificCrucibleOfTheUnbroken = "DutySpecificCrucibleOfTheUnbroken",
		DutySpecificTheMaskedCarnivale = "DutySpecificTheMaskedCarnivale",
		DutySpecificPvP = "DutySpecificPvP",
		DutySpecificFieldOps = "DutySpecificFieldOps",
		DutySpecificAlliance = "DutySpecificAlliance",
		DutySpecificDeepDungeon = "DutySpecificDeepDungeon",
		DutySpecificVariantDungeon = "DutySpecificVariantDungeon",
		DutySpecificTreasureDungeon = "DutySpecificTreasureDungeon",
		DutySpecificChaoticAlliance = "DutySpecificChaoticAlliance",
		DutySpecificDungeon = "DutySpecificDungeon",
		DutySpecificUltimate = "DutySpecificUltimate",
		DutySpecificExtreme = "DutySpecificExtreme",
		DutySpecificSavage = "DutySpecificSavage",
		BasicCommands = "BasicCommands",
		UiInformation = "UiInformation",
		UiCustomize = "UiCustomize",
		UiWindows = "UiWindows",
		UiHotbars = "UiHotbars",
		AutoActionUsage = "AutoActionUsage",
		AutoIntercept = "AutoIntercept",
		HealingActionCondition = "HealingActionCondition",
		RaiseActionCondition = "RaiseActionCondition",
		TargetConfig = "TargetConfig",
		Extra = "Extra",
		ListCastingArea = "ListCastingArea",
		ListKnockback = "ListKnockback",
		Debug = "Debug";

	public const int CurrentVersion = 12;
	public int Version { get; set; } = CurrentVersion;

	/// <summary>
	/// The fork's own migration count (<see cref="Migrate"/>); a file without it starts at 0, and a fresh configuration
	/// runs the steps once against its defaults, which they leave as they are.
	/// </summary>
	public int ForkVersion { get; set; }

	public string LastSeenChangelog { get; set; } = "0.0.0.0";
	public bool TutorialDone { get; set; } = false;

	public AoEType StateWindowManualAoEType { get; set; } = AoEType.Cleave;

	public bool ControlWindowLinkAutoFull { get; set; } = false;
	public bool ControlWindowLinkManualCleave { get; set; } = false;

	public bool IsMainWindowLock { get; set; } = false;

	public List<ActionEventInfo> Events { get; private set; } = [];
	public SortedSet<Job> DisabledJobs { get; private set; } = [];

	/// <summary>The HP and MP potions, Phoenix Downs and tinctures switched off in the action list - for every job (A635, A645).</summary>
	public SortedSet<uint> ExcludedConsumables { get; private set; } = [];

	public string[] RotationLibs { get; set; } = [];
	public List<TargetingType> TargetingTypes { get; set; } = [];

	public MacroInfo DutyStart { get; set; } = new MacroInfo();
	public MacroInfo DutyEnd { get; set; } = new MacroInfo();

	#region Integer

	public int ActionSequencerIndex { get; set; }

	[UI("Modifier key that unlocks movement", Description = "RB is for gamepad players.", Parent = nameof(PoslockCasting))]
	public ConsoleModifiers PoslockModifier { get; set; }

	[Range(0, 10, ConfigUnitType.None)]
	public int TargetingIndex { get; set; }

	#endregion

	#region Jobs

	[JobConfig, UI("Use Lucid Dreaming below this MP", Filter = AutoActionUsage)]
	[Range(0, 10000, ConfigUnitType.None)]
	private readonly int _lucidDreamingMpThreshold = 6000;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaAbilityHot = 0.55f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaSpellHot = 0.55f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaAbility = 0.75f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaSpell = 0.65f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleAbilityHot = 0.65f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleSpellHot = 0.55f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleAbility = 0.7f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleSpell = 0.65f;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent, 0.02f)]
	[UI("Use invulnerability below this HP",
		Filter = AutoActionUsage, Section = 2,
		PvEFilter = JobFilterType.Tank, PvPFilter = JobFilterType.NoJob)]
	private readonly float _healthForDyingTanks = 0.15f;

	[JobConfig]
	private readonly string _PvPRotationChoice = string.Empty;

	[JobConfig]
	private readonly string _rotationChoice = string.Empty;
	#endregion

	[JobConfig]
	private readonly ConcurrentDictionary<uint, ActionConfig> _rotationActionConfig = new();

	[JobConfig]
	private readonly ConcurrentDictionary<uint, ItemConfig> _rotationItemConfig = new();

	[JobChoiceConfig]
	private readonly ConcurrentDictionary<string, string> _rotationConfigurations = new();

	public ConcurrentDictionary<uint, string> DutyRotationChoice = new();

	public void Save()
	{
#if DEBUG
		PluginLog.Information("Saved configurations.");
#endif
		var path = Svc.PluginInterface.ConfigFile.FullName;
		var tempPath = path + ".tmp";
		File.WriteAllText(tempPath, JsonConvert.SerializeObject(this, Formatting.Indented));
		File.Move(tempPath, path, true);
	}

	/// <summary>
	/// Carries stored settings across a change of <see cref="CurrentVersion"/> field by field: the stored values were read
	/// into today's type by name, so a setting that still exists keeps its value, a new one takes its default and a
	/// removed one is dropped. Upstream reset the whole configuration instead (A389).
	/// </summary>
	/// <param name="oldConfigs">The configuration as read from the file.</param>
	/// <returns>The same configuration at the current version.</returns>
	public static Configs Migrate(Configs oldConfigs)
	{
		oldConfigs.Version = CurrentVersion;

		// The fork's own steps, counted apart from upstream's Version, so an upstream bump neither skips nor repeats them.
		if (oldConfigs.ForkVersion < 1)
		{
			// The interrupt and provoke delays still at upstream's old default of 0.5 to 1 s, unread from 2024 until A338,
			// are cut to the fork's default of none (his rule, A500): the delay is optional, and one never set is none.
			var oldDefault = new Vector2(0.5f, 1f);
			if (oldConfigs.InterruptDelay == oldDefault)
			{
				oldConfigs.InterruptDelay = Vector2.Zero;
			}

			if (oldConfigs.ProvokeDelay == oldDefault)
			{
				oldConfigs.ProvokeDelay = Vector2.Zero;
			}

			oldConfigs.ForkVersion = 1;
		}

		return oldConfigs;
	}

	public void Backup()
	{
		Save();
		File.Copy(Svc.PluginInterface.ConfigFile.FullName, Svc.PluginInterface.ConfigFile.Directory + "\\RotationSolver_Backup.json", true);
		Svc.Toasts.ShowNormal("Configs backed up.");
	}

	public void Restore()
	{
		File.Copy(Svc.PluginInterface.ConfigFile.FullName, Svc.PluginInterface.ConfigFile.Directory + "\\RotationSolver_SafetySave.json", true);
		File.Copy(Svc.PluginInterface.ConfigFile.Directory + "\\RotationSolver_Backup.json", Svc.PluginInterface.ConfigFile.FullName, true);

		var restoredConfigs = JsonConvert.DeserializeObject<Configs>(
									  File.ReadAllText(Svc.PluginInterface.ConfigFile.FullName))
								  ?? new Configs();

		Service.Config = Migrate(restoredConfigs);
		Save();
		Svc.Toasts.ShowNormal("Configs restored. Closing to set.");
		DataCenter.HoldingRestore = true;
	}
}
