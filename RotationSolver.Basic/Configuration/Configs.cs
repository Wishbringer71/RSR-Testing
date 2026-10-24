using Dalamud.Configuration;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using RebornMaterial;
using System.Collections.Concurrent;
using static RotationSolver.Basic.Configuration.ConfigTypes;

namespace RotationSolver.Basic.Configuration;

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

	public string LastSeenChangelog { get; set; } = "0.0.0.0";
	public bool TutorialDone { get; set; } = false;

	public AoEType StateWindowManualAoEType { get; set; } = AoEType.Cleave;

	public bool ControlWindowLinkAutoFull { get; set; } = false;
	public bool ControlWindowLinkManualCleave { get; set; } = false;

	public bool IsMainWindowLock { get; set; } = false;

	public List<ActionEventInfo> Events { get; private set; } = [];
	public SortedSet<Job> DisabledJobs { get; private set; } = [];

	public string[] RotationLibs { get; set; } = [];
	public List<TargetingType> TargetingTypes { get; set; } = [];

	public MacroInfo DutyStart { get; set; } = new MacroInfo();
	public MacroInfo DutyEnd { get; set; } = new MacroInfo();

	#region Duty Specific
	[ConditionBool, UI("Don't use flat damage or death actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockflatdamagedeathimmuneBLU = false;

	[ConditionBool, UI("Don't use Slow actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockslowimmuneBLU = false;

	[ConditionBool, UI("Don't use Petrification actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockpetrificationimmuneBLU = false;

	[ConditionBool, UI("Don't use Paralysis actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockparalysisimmuneBLU = false;

	[ConditionBool, UI("Don't use Interrupt actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockinterruptimmuneBLU = false;

	[ConditionBool, UI("Don't use Blind actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockblindimmuneBLU = false;

	[ConditionBool, UI("Don't use Stun actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockstunimmuneBLU = false;

	[ConditionBool, UI("Don't use Sleep actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blocksleepimmuneBLU = false;

	[ConditionBool, UI("Don't use Bind actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockbindimmuneBLU = false;

	[ConditionBool, UI("Don't use Heavy actions on immune enemies", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockheavyimmuneBLU = false;

	[ConditionBool, UI("Don't use aspected actions on enemies immune to that aspect", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blockimmuneBLU = true;

	[ConditionBool, UI("Only use actions of an aspect the enemy is weak to", Filter = DutySpecificTheMaskedCarnivale)]
	private static readonly bool _blocknonweakBLU = false;

	[ConditionBool, UI("Still allow unaspected actions", Filter = DutySpecificTheMaskedCarnivale, Parent = nameof(BlocknonweakBlu))]
	private static readonly bool _allowunaspectedBLU = true;

	[ConditionBool, UI("O12S - Packet Filter logic",
	Description = "Treat OmegaM/OmegaF as immune if you have their corresponding Packet Filter status (also applies to Normal).",
	Filter = DutySpecificSavage)]
	private static readonly bool _o12sOmegaMF = true;

	[ConditionBool, UI("M8S - Wolf Pack/Stone Pack logic",
	Description = "Treat Wolf of Wind/Wolf of Stone as immune if you don't have the corresponding status for it.",
	Filter = DutySpecificSavage)]
	private static readonly bool _m8sWindStone = true;

	[ConditionBool, UI("M9S - Only use cleave logic",
	Description = "This should clear up any targeting issues when a nail, the boss, and a flail are near each other at the same time.",
	Filter = DutySpecificSavage)]
	private static readonly bool _m9sCleaveOnly = true;

	[ConditionBool, UI("M9S - Cell targeting logic",
	Description = "Treat Cells as immune if you don't have the corresponding status for it.",
	Filter = DutySpecificSavage)]
	private static readonly bool _m9sCellTargeting = true;

	[ConditionBool, UI("M9S - Ads targeting logic",
	Description = "Prioritize Doornail or Flail based on role and distance to target.",
	Filter = DutySpecificSavage)]
	private static readonly bool _m9sAdsTargeting = true;

	[ConditionBool, UI("M10S - Firesnaking/Watersnaking targeting logic",
	Description = "Prioritize Red Hot if you have the firesnaking buff, and Deep Blue if you have the watersnaking buff (also applies to Normal).",
	Filter = DutySpecificSavage)]
	private static readonly bool _m10sBroTargeting = true;

	[ConditionBool, UI("Limitless Blue Extreme - Whaleback logic",
	Description = "Treat Bismarck Shell/Bismarck Corona as immune if you don't have the Whaleback status.",
	Filter = DutySpecificExtreme)]
	private static readonly bool _limitlessBlueTargeting = true;

	[ConditionBool, UI("Cinder Drift Extreme - Pall targeting logic",
	Description = "Treat Pall of Rage/Pall of Grief as immune if you don't have the corresponding status for it.",
	Filter = DutySpecificExtreme)]
	private static readonly bool _cinderDriftPallTargeting = true;

	[ConditionBool, UI("The Unmaking Extreme - Shadow logic",
	Description = "Treat Shadow as immune if you don't have the corresponding status.",
	Filter = DutySpecificExtreme)]
	private static readonly bool _theUnmakingShadow = true;

	[ConditionBool, UI("The Epic of Alexander (Ultimate) - Jagd Doll logic",
	Description = "Treat Jagd Doll ads as immune when HP is less than 25%.",
	Filter = DutySpecificUltimate)]
	private static readonly bool _teaJagdDoll = true;

	[ConditionBool, UI("The Epic of Alexander (Ultimate) - True Heart logic",
	Description = "Treat True Heart ad as immune.",
	Filter = DutySpecificUltimate)]
	private static readonly bool _teaTrueHeart = true;

	[ConditionBool, UI("The Omega Protocol (Ultimate) - Packet Filter logic",
	Description = "Treat OmegaM/OmegaF as immune if you have their corresponding Packet Filter status.",
	Filter = DutySpecificUltimate)]
	private static readonly bool _topOmegaMF = true;

	[ConditionBool, UI("Futures Rewritten (Ultimate) - Crystal of Darkness logic",
	Description = "Treat Crystal of Darkness ad as immune.",
	Filter = DutySpecificUltimate)]
	private static readonly bool _fruCrystalOfDarkness = true;

	[ConditionBool, UI("Dancing Mad (Ultimate) - Hero/Villain logic",
	Description = "Treat bosses as immune if you don't have the corresponding status for it.",
	Filter = DutySpecificUltimate)]
	private static readonly bool _dmuBossImmune = true;

	[ConditionBool, UI("The Ghimlyt Dark - Colossus Rubricatus logic",
	Description = "Treat Colossus Rubricatus as immune while it's casting the scripted action that leads to its death.",
	Filter = DutySpecificDungeon)]
	private static readonly bool _colossusRubricatusImmune = true;

	[ConditionBool, UI("Dohn Mheg - Liar's Lyre logic",
	Description = "Treat Liar's Lyre as immune if you don't have the Unfooled status.",
	Filter = DutySpecificDungeon)]
	private static readonly bool _dohnMhegLyre = true;

	[ConditionBool, UI("The Meso Terminal - Jailer logic",
	Description = "Treat Jailers in the second boss fight as immune if you don't have the corresponding buff.",
	Filter = DutySpecificDungeon)]
	private static readonly bool _jailerImmune = true;

	[ConditionBool, UI("Forked Tower: Blood - Dead Stars logic",
	Description = "Treat Triton/Nereid/Phobos as immune if you don't have the corresponding status for it.",
	Filter = DutySpecificFieldOps)]
	private static readonly bool _forkedtowerDeadStar = true;

	[ConditionBool, UI("Forked Tower: Magic - First boss Hero/Villain logic",
	Description = "Treat the first boss of Forked Tower as immune if you don't have the corresponding status for it.",
	Filter = DutySpecificFieldOps)]
	private static readonly bool _forkedtowerFirstBossVillianHero = true;

	[ConditionBool, UI("North Horn CE - Tiny Mage targeting logic",
	Description = "Treat Tiny Mage CE mobs as immune if you're more than 5 yalms away and they are not the farthest along their cast.",
	Filter = DutySpecificFieldOps)]
	private static readonly bool _northHornTinyMage = true;

	[ConditionBool, UI("Occult Crescent - Track elemental weaknesses",
	Description = "Tracks enemies' elemental weaknesses so you can report them to the developers. See Debug > Occult Crescent Weaknesses.",
	Filter = DutySpecificFieldOps)]
	private static readonly bool _elementalWeaknessTracking = false;

	[ConditionBool, UI("Pilgrim's Traverse - Eminent Grief logic",
	Description = "Treat Eminent Grief as immune if you don't have the Light Vengeance buff, and treat Devoured Eater as immune if you don't have the Dark Vengeance buff.",
	Filter = DutySpecificDeepDungeon)]
	private static readonly bool _eminent = true;

	[ConditionBool, UI("The Labyrinth of the Ancients - Thanatos logic",
	Description = "Treat Thanatos as immune if you don't have the Astral Realignment buff.",
	Filter = DutySpecificAlliance)]
	private static readonly bool _thanatosImmune = true;

	[ConditionBool, UI("The Void Ark - Irminsul and Sawtooth logic",
	Description = "Treat Irminsul and Sawtooth as immune if you don't have the corresponding buff.",
	Filter = DutySpecificAlliance)]
	private static readonly bool _irminsulSawtoothImmune = true;

	[ConditionBool, UI("The Puppets' Bunker - Superior Flight Unit logic",
	Description = "Treat each Superior Flight Unit as immune if you don't have the corresponding buff.",
	Filter = DutySpecificAlliance)]
	private static readonly bool _superiorFlightUnitImmune = true;

	[ConditionBool, UI("The Tower at Paradigm's Breach - Hansel and Gretel logic",
	Description = "Treat each Hansel/Gretel as immune if you are at an angle that would cause you to take rebound damage from the shield mechanic.",
	Filter = DutySpecificAlliance)]
	private static readonly bool _hanselorGretelShieldedImmune = true;

	[ConditionBool, UI("Jeuno: The First Walk - The Ark Angels logic",
	Description = "Treat each Ark Angel as immune if you don't have the corresponding buff.",
	Filter = DutySpecificAlliance)]
	private static readonly bool _jeunoBossImmune = true;

	[ConditionBool, UI("Windurst: The Third Walk - Alexander Resurrected logic",
	Description = "Treat Alexander and/or Gordius Systems as immune if they have Perfect Defense.",
	Filter = DutySpecificAlliance)]
	private static readonly bool _alexanderImmune = true;

	[ConditionBool, UI("Cloud of Darkness - Ads phase logic",
	Description = "Treat Cloud of Darkness/Stygian as immune if you don't have the corresponding buff.",
	Filter = DutySpecificChaoticAlliance)]
	private static readonly bool _codImmune = true;

	[ConditionBool, UI("The Sil'dihn Subterrane - Drake ads logic",
	Description = "Treat certain drakes as immune so they die in the order needed for Variant path 12.",
	Filter = DutySpecificVariantDungeon)]
	private static readonly bool _drakeImmune = true;

	[ConditionBool, UI("Treasure dungeons - Timed mob logic",
	Description = "Prioritizes limited-time mobs in treasure dungeons to resolve the extra loot mechanic.",
	Filter = DutySpecificTreasureDungeon)]
	private static readonly bool _treasuredungeontimed = true;

	[ConditionBool, UI("Treasure dungeons - Numbered mob logic",
	Description = "Sets mobs with number plates as top priority, from lowest to highest, to resolve the extra loot mechanic.",
	Filter = DutySpecificTreasureDungeon)]
	private static readonly bool _treasuredungeonnumbered = true;

	[ConditionBool, UI("Crucible of the Unbroken - Ymir logic",
	Description = "Prioritize the Sahagin Piece until it has Paralyzing Spikes, then the Ymir Shell, then the Ymir Piece once its shell is gone. Ymir is treated as immune while it has Vulnerability Down, and the spiked Sahagin while Ymir is up.",
	Filter = DutySpecificCrucibleOfTheUnbroken)]
	private static readonly bool _crucibleYmir = true;

	[ConditionBool, UI("Crucible of the Unbroken - Siren logic",
	Description = "Prioritize Crawling Pieces, which bind you in place.",
	Filter = DutySpecificCrucibleOfTheUnbroken)]
	private static readonly bool _crucibleSiren = true;
	#endregion

	#region PvP
	[JobConfig, UI("Use Guard below this HP",
		Filter = DutySpecificPvP)]
	[Range(0, 1, ConfigUnitType.Percent, 0.02f)]
	public float HealthForGuard { get; set; } = 0.15f;

	[ConditionBool, UI("Ignore time to kill in PvP", Filter = DutySpecificPvP)]
	private static readonly bool _ignorePvPTTK = true;

	[ConditionBool, UI("Prioritize A-tier tomeliths in Shatter", Filter = DutySpecificPvP, Order = 1)]
	private static readonly bool _prioAtomelith = false;

	[ConditionBool, UI("Prioritize B-tier tomeliths in Shatter", Filter = DutySpecificPvP, Order = 1)]
	private static readonly bool _prioBtomelith = false;

	[JobConfig, UI("Ignore invincibility in PvP", Filter = DutySpecificPvP)]
	private static readonly bool _ignorePvPInvincibility = false;

	[ConditionBool, UI("Turn off when you die in PvP",
		 Filter = DutySpecificPvP, Section = 1)]
	private static readonly bool _autoOffWhenDeadPvP = true;

	[ConditionBool, UI("Turn off when a PvP match ends",
		 Filter = DutySpecificPvP, Section = 1)]
	private static readonly bool _autoOffPvPMatchEnd = true;

	[ConditionBool, UI("Turn on when a PvP match starts",
		 Filter = DutySpecificPvP, Section = 1)]
	private static readonly bool _autoOnPvPMatchStart = true;

	[ConditionBool, UI("Use the PvP state when turning RSR on in PvP",
		 Filter = DutySpecificPvP, Section = 1)]
	private static readonly bool _pvpStateControl = false;

	[ConditionBool, UI("Don't use actions while Guard is up",
		 Filter = DutySpecificPvP)]
	private static readonly bool _pvpGuardControl = true;

	[ConditionBool, UI("Cancel casts when the target gains Guard",
		Description = "Actions that ignore Guard keep casting.",
		Filter = DutySpecificPvP)]
	private static readonly bool _pvpGuardCancel = false;

	[ConditionBool, UI("Use Purify on Stun",
		 Filter = DutySpecificPvP, Section = 2)]
	private static readonly bool _pvpPurifyStun = true;

	[ConditionBool, UI("Use Purify on Silence",
		 Filter = DutySpecificPvP, Section = 2)]
	private static readonly bool _pvpPurifySilence = true;

	[ConditionBool, UI("Use Purify on Deep Freeze",
		 Filter = DutySpecificPvP, Section = 2)]
	private static readonly bool _pvpPurifyDeepFreeze = true;

	[ConditionBool, UI("Use Purify on Miracle of Nature",
		 Filter = DutySpecificPvP, Section = 2)]
	private static readonly bool _pvpPurifyMiracleOfNature = true;

	[ConditionBool, UI("Use Purify on Heavy",
		 Filter = DutySpecificPvP, Section = 2)]
	private static readonly bool _pvpPurifyHeavy = false;

	[ConditionBool, UI("Use Purify on Bind",
		 Filter = DutySpecificPvP, Section = 2)]
	private static readonly bool _pvpPurifyBind = false;

	[ConditionBool, UI("Hold GCDs for heals below 50% HP (Experimental)",
		Description = "Stops the GCD rotation while you're below 50% HP and have over 2000 MP, so the MP goes to heals.",
		Filter = DutySpecificPvP)]
	private static readonly bool _pvpGCDLockControl = true;

	[ConditionBool, UI("Sprint with no target, even near enemies (Experimental)",
		 Filter = DutySpecificPvP)]
	private static readonly bool _pvpAllowSprintWithoutTarget = true;

	#endregion

	[ConditionBool, UI("Check intercepted movement actions with BossModReborn (Experimental)",
	Description = "Before RSR uses an intercepted movement action, or an action that moves you, ask BossModReborn whether the destination is safe.",
	Parent = nameof(InterceptAction3), Order = 1)]
	private static readonly bool _bmrSafetyCheckIntercept = false;

	[ConditionBool, UI("Intercept your action presses (PvE only)",
	Description = "Actions you press are queued, and RSR uses them at the right moment in its rotation.",
	Filter = AutoIntercept)]
	private static readonly bool _interceptAction3 = true;

	[ConditionBool, UI("Intercept spells",
	Parent = nameof(InterceptAction3))]
	private static readonly bool _interceptSpell3 = true;

	[ConditionBool, UI("Intercept weaponskills",
	Parent = nameof(InterceptAction3))]
	private static readonly bool _interceptWeaponskill3 = true;

	[ConditionBool, UI("Intercept abilities",
	Parent = nameof(InterceptAction3))]
	private static readonly bool _interceptAbility3 = true;

	[ConditionBool, UI("Intercept actions in macros",
	Parent = nameof(InterceptAction3))]
	private static readonly bool _interceptMacro = false;

	[ConditionBool, UI("Intercept actions that are on cooldown",
	Parent = nameof(InterceptAction3))]
	private static readonly bool _interceptCooldown = false;

	[ConditionBool, UI("Also pass your press to the game",
	Description = "The game handles your press as well as RSR. This causes clipping.",
	Parent = nameof(InterceptAction3))]
	private static readonly bool _interceptPassing = true;

	[UI("How long RSR keeps trying an intercepted action",
	Parent = nameof(InterceptAction3), Order = 1)]
	[Range(1, 10, ConfigUnitType.Seconds)]
	public float InterceptActionTime { get; set; } = 5;

	[ConditionBool, UI("Let BossModReborn Cooldown Planner actions through (Experimental)",
		Description = "When BossModReborn is loaded, actions planned in its Cooldown Planner pass through the intercept system while their window is active.",
		Parent = nameof(InterceptAction3), Order = 1)]
	private static readonly bool _useBMRPlan = false;

	[UI("Cooldown Planner look-ahead", Parent = nameof(UseBmrPlan),
		Description = "How far ahead to look for upcoming Cooldown Planner actions.")]
	[Range(1, 60, ConfigUnitType.Seconds, 1f)]
	public float BMRPlanLookAheadSeconds { get; set; } = 5f;

	/// <markdown file="Auto" name="AoE usage" section="Action Usage and Control">
	/// - Full: Use all available AoE actions.
	/// - Cleave: Use only single-target AoE actions.
	/// - Off: Do not use any AoE actions.
	///
	/// Attacks only: heals and other actions on the party are not affected.
	/// </markdown>
	[UI("AoE usage",
	Description = "Full: Use all available AoE actions.\nCleave: Use only single-target AoE actions.\nOff: Do not use any AoE actions.\nAttacks only: heals and other actions on the party are not affected.",
	Filter = AutoActionUsage, Section = 1)]
	public AoEType AoEType { get; set; } = AoEType.Full;

	[ConditionBool, UI("Don't apply statuses to status-capped enemies",
	Filter = AutoActionUsage, Section = 1)]
	private static readonly bool _statuscap2 = true;

	[ConditionBool, UI("Time DoT refreshes from DoT ticks and time to kill (Experimental)",
		Description = "On: DoTs with potency data are refreshed on the last GCD that loses no tick, and skipped when the target will die before they out-damage your filler.\nOff: DoTs are refreshed a set number of GCDs before they run out.",
		Filter = AutoActionUsage, Section = 1)]
	// On by default, unlike upstream: the owner's rule for defaults (29.09.2026) - a new rule is to be tested, and the
	// sensible value in a fight is a refresh that loses no tick and no DoT on a target dying before it pays (A341).
	private static readonly bool _useDotTickTiming = true;

	[ConditionBool, UI("Don't pull new enemies with AoE", Description = "Never use an AoE action that could hit enemies that aren't already hostile targets.",
		Filter = AutoActionUsage, Section = 1)]
	private static readonly bool _noNewHostiles = false;

	[ConditionBool, UI("Turn off during area transitions",
	Filter = BasicAutoSwitch)]
	private static readonly bool _autoOffBetweenArea = true;

	[ConditionBool, UI("Turn off during cutscenes",
	Filter = BasicAutoSwitch)]
	private static readonly bool _autoOffCutScene = true;

	[ConditionBool, UI("Turn off when you change jobs",
	Filter = BasicAutoSwitch)]
	private static readonly bool _autoOffSwitchClass = true;

	[ConditionBool, UI("Turn off when you die",
	Description = "PvE only. The PvP setting is under Duty > PvP.",
	Filter = BasicAutoSwitch)]
	private static readonly bool _autoOffWhenDead = true;

	[ConditionBool, UI("Turn off when a duty is completed",
	Filter = BasicAutoSwitch)]
	private static readonly bool _autoOffWhenDutyCompleted = true;

	[ConditionBool, UI("Show the changelog after an update",
	Filter = UiInformation)]
	private static readonly bool _changelogPopup = true;

	[ConditionBool, UI("Show RSR's state in the server info bar",
	Filter = UiInformation)]
	private static readonly bool _showInfoOnDtr = true;

	[UI("Server info bar style", Parent = nameof(ShowInfoOnDtr))]
	public DTRType DTRType { get; set; } = DTRType.DTRNormal;

	[ConditionBool, UI("Show a toast when RSR's state changes",
	Filter = UiInformation)]
	private static readonly bool _showInfoOnToast = false;

	[ConditionBool, UI("Lock movement while casting",
	Description = "Stops your character from moving while casting, and while using the actions ticked below.",
	Filter = Extra)]
	private static readonly bool _poslockCasting = false;

	[UI("", Action = ActionID.PassageOfArmsPvE, Parent = nameof(PoslockCasting))]
	public bool PosPassageOfArms { get; set; } = false;

	[UI("", Action = ActionID.FlamethrowerPvE, Parent = nameof(PoslockCasting))]
	public bool PosFlameThrower { get; set; } = false;

	[UI("", Action = ActionID.ImprovisationPvE, Parent = nameof(PoslockCasting))]
	public bool PosImprovisation { get; set; } = false;

	[UI("Unlock movement while holding both mouse buttons", Parent = nameof(PoslockCasting))]
	public bool PosLockMouse { get; set; } = false;

	[ConditionBool, UI("Don't break Passage of Arms",
	Description = "While Passage of Arms is up for AoE mitigation, RSR won't use actions that would end it.",
	Filter = Extra)]
	private static readonly bool _pldlockCasting = true;

	[ConditionBool, UI("Don't break Collective Unconscious",
	Description = "While Collective Unconscious is up for AoE mitigation, RSR won't use actions that would end it.",
	Filter = Extra)]
	private static readonly bool _astlockCasting = false;

	[ConditionBool, UI("Don't break Phantom Flurry",
	Description = "While Phantom Flurry is channeling, RSR won't use actions that would end it.",
	Filter = Extra)]
	private static readonly bool _blulockCasting = true;

	/// <markdown file="Auto" name="Tincture usage" section="Action Usage and Control">
	/// Sets whether to use damage-boosting potions and in which duty. You also need to enable the specific
	/// potion in the `Actions` tab, under `Items`.
	/// </markdown>
	[JobConfig, UI("Tincture usage",
	Description = "When to use gemdraughts and tinctures. Only rotations that support automatic tincture usage follow this.",
	Filter = AutoActionUsage, Section = 5, PvPFilter = JobFilterType.NoJob)]
	private readonly TinctureUseType _TinctureType = TinctureUseType.InHighEndDuty;

	[ConditionBool, UI("Use anti-knockback actions",
	Description = "Use Arm's Length and Surecast when an enemy casts an action on the Knockback list.",
	Filter = AutoActionUsage, Section = 2, Order = 1)]
	private static readonly bool _useKnockback = true;

	[ConditionBool, UI("Use HP potions",
	Filter = AutoActionUsage, Section = 5)]
	private static readonly bool _useHpPotions = false;

	[UI("Use HP potions below this HP", Parent = nameof(UseHpPotions))]
	[Range(0, 1, ConfigUnitType.Percent, 0.002f)]
	public float UseHpPotionsPercent { get; set; } = 0.5f;

	[ConditionBool, UI("Use MP potions",
	Filter = AutoActionUsage, Section = 5)]
	private static readonly bool _useMpPotions = false;

	[UI("Use MP potions below this MP", Parent = nameof(UseMpPotions))]
	[Range(0, 1, ConfigUnitType.Percent, 0.002f)]
	public float UseMpPotionsPercent { get; set; } = 0.5f;

	[ConditionBool, UI("Use Phoenix Down (Experimental)",
	Filter = AutoActionUsage, Section = 5)]
	private static readonly bool _usePhoenixDown = false;

	[ConditionBool, UI("Only when no one who can raise is alive",
	Description = "A raiser is a living healer, Summoner or Red Mage of a high enough level to have their raise. Which of them count follows the raise target setting: the party alone under Party only and Party healers only, party and alliance under the alliance modes and All.\n"
		+ "On: the item is kept for the case it exists for - nobody left who can cast a raise. A wipe recovery where the healer is up costs no Phoenix Down.\n"
		+ "Off: a Phoenix Down can go out while a healer is standing next to the corpse, spending a consumable on a raise that was free.",
	Parent = nameof(UsePhoenixDown))]
	private static readonly bool _usePhoenixDownHealerLogic = true;

	[ConditionBool, UI("Use AoE on marked targets",
	Description = "AoE actions may hit targets with attack markers.",
	Parent = nameof(ChooseAttackMark))]
	private static readonly bool _canAttackMarkAOE = true;

	[ConditionBool, UI("Teaching mode",
		Description = "Highlights the action RSR would use next on your hotbars.",
		Filter = UiHotbars)]
	private static readonly bool _teachingMode = false;

	[ConditionBool, UI("Target what the rotation suggests in combat",
		Description = "Switches your target to the suggested action's target. Useful for tanks and healers, whose best target can differ from the one you have selected.",
		Parent = nameof(TeachingMode))]
	private static readonly bool _teachingModeAutoTarget = false;

	[ConditionBool, UI("Show target hint in Next Action window",
		Description = "Shows the suggested target's name below the Next Action icon: orange if you don't have it selected, green if you do.",
		Parent = nameof(TeachingMode))]
	private static readonly bool _teachingModeShowTargetHint = false;

	[ConditionBool, UI("Show button presses on hotbars",
		Description = "Plays the pressed effect on the hotbar slot when RSR uses an action.",
		Filter = UiHotbars)]
	private static readonly bool _keyboardNoise = true;

	[ConditionBool, UI("Turn on Auto mode when a countdown starts",
		Filter = BasicAutoSwitch, Section = 1)]
	private static readonly bool _startOnCountdown = true;

	[ConditionBool, UI("Use Manual mode instead",
			   Parent = nameof(StartOnCountdown))]
	private static readonly bool _countdownStartsManualMode = false;

	[ConditionBool, UI("Turn off if combat starts before the countdown ends",
		Filter = BasicAutoSwitch, Section = 1)]
	private static readonly bool _cancelStateOnCombatBeforeCountdown = false;

	[ConditionBool, UI("Let RSR turn itself on",
		Description = "Needed for the settings below. They turn RSR's autorotation on without any input from you.",
		Filter = BasicAutoSwitch, Section = 2)]
	private static readonly bool _AutoOnYes = false;

	[ConditionBool, UI("Turn on Manual mode when you're attacked",
			   Parent = nameof(AutoOnYes))]
	private static readonly bool _startOnAttackedBySomeone2 = false;

	[ConditionBool, UI("Turn on Auto mode when your party is in combat",
			   Parent = nameof(AutoOnYes))]
	private static readonly bool _startOnPartyIsInCombat2 = false;

	[ConditionBool, UI("Turn on Auto mode when your alliance is in combat",
			   Parent = nameof(AutoOnYes))]
	private static readonly bool _startOnAllianceIsInCombat2 = false;

	[ConditionBool, UI("Turn on Auto mode in combat in field operations",
			   Description = "Bozja, Eureka, and Occult Crescent FATEs and critical engagements.",
			   Parent = nameof(AutoOnYes))]
	private static readonly bool _startOnFieldOpInCombat2 = false;

	/// <markdown file="Auto" name="Use healing abilities as a non-healer" section="Healing Usage and Control">
	/// Allow usage of healing abilities when not playing as a healer (such as Vercure, Bloodbath, etc.)
	/// </markdown>
	[ConditionBool, UI("Use healing abilities as a non-healer",
		Description = "Lets tanks and DPS heal with abilities such as Vercure and Bloodbath.",
		Filter = HealingActionCondition, Section = 4)]
	private static readonly bool _useHealWhenNotAHealer = true;

	[JobConfig, UI("Interrupt enemy casts",
		Filter = AutoActionUsage, Section = 3,
		PvEFilter = JobFilterType.Interrupt,
		PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _interruptibleMoreCheck = true;

	[ConditionBool, UI("Stop an enemy's cast with the job's stun or a sleep (experimental)",
		Description = "As a healer or a caster, who have no interrupt: when an enemy that is not a boss casts an area attack that reaches you or two party members, or a cast the game marks interruptible - what a melee would stop with Leg Sweep -, and enough of its cast is left to land in time - the own cast time, the GCD still running, the measured time to the effect and what a cast has to have left when the status lands -, RSR stops it. A White Mage uses Holy when the enemy stands in its radius and can still be stunned, which stuns every enemy around; otherwise Repose or Sleep goes on the enemy. Swiftcast is not spent on it, Dualcast and Triplecast are. A healer does not use it while a party member is about to die. With \"Don't pull new enemies with AoE\" on, no area stop reaches an enemy that is not fighting yet. What sleeps show is kept over every session: enemy kinds that resist a sleep, how many sleeps a kind takes in a fight before it resists, and per enemy action whether a sleep stops it and how early it has to land - so an enemy or an attack it does not work on is not tried again. Every attempt and what came of it is written to DefenseTrace.log.\nIn a fight: an enemy's attack can stop before it lands, for one GCD of yours; the enemy wakes at the next hit. Bosses and many elite enemies resist.",
		Filter = AutoActionUsage, Section = 3,
		PvEFilter = JobFilterType.Sleep,
		PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _sleepToStopAreaCasts = true;

	[ConditionBool, UI("Provoke any enemy not on the Don't provoke list",
		Description = "Skips RSR's usual checks for whether an enemy is worth provoking.",
		Filter = AutoActionUsage, Section = 3,
		PvEFilter = JobFilterType.Tank)]
	private static readonly bool _provokeAnything = false;

	[ConditionBool, UI("Stop casting when the target dies",
		Filter = AutoActionUsage, Section = 3)]
	private static readonly bool _useStopCasting = false;

	/// <markdown file="Auto" name="Dispel every dispellable debuff" section="Action Usage and Control">
	/// Enabling this setting will force the usage of Esuna on all target that are affected by a
	/// cleansable debuff.
	/// </markdown>
	[ConditionBool, UI("Dispel every dispellable debuff",
		Description = "Otherwise, while enemies are nearby, RSR focuses on the debuffs in the Dispellable debuffs list.",
		Filter = AutoActionUsage, Section = 4,
		PvEFilter = JobFilterType.Dispel, PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _dispelAll = false;

	[ConditionBool, UI("Debug mode", Filter = Debug)]
	private static readonly bool _inDebug = false;

	[ConditionBool, UI("Enable the action tracer (only at a developer's request)", Filter = Debug)]
	private static readonly bool _enableActionTracer = false;

	[ConditionBool, UI("Mirror tracer output to the Dalamud log",
		Parent = nameof(EnableActionTracer))]
	private static readonly bool _traceMirrorToPluginLog = false;

	[ConditionBool, UI("Make /rotation Manual a toggle",
		Filter = BasicCommands)]
	private static readonly bool _toggleManual = false;

	[ConditionBool, UI("Make /rotation Auto a toggle",
		Description = "When off, using /rotation Auto again cycles to your next targeting type.",
		Filter = BasicCommands)]
	private static readonly bool _toggleAuto = false;

	[ConditionBool, UI("Show State window",
		Description = "A small Auto / Manual / Off switch for the autorotation state. /rotation Control shows and hides it too.",
		Filter = UiWindows)]
	private static readonly bool _showStateControlWindow = false;

	[ConditionBool, UI("Only show RSR windows in duties or near enemies",
		Description = "Applies to the Control, Next Action, Intercepted Action and Action Timeline windows, and to teaching mode. Enemies count as near within 25 yalms.",
		Filter = UiWindows, Section = 1)]
	private static readonly bool _onlyShowWithHostileOrInDuty = false;

	[ConditionBool, UI("Show Control window",
		Filter = UiWindows)]
	private static readonly bool _showControlWindow = false;

	[ConditionBool, UI("Lock Control window",
		Parent = nameof(ShowControlWindow))]
	private static readonly bool _isControlWindowLock = false;

	[ConditionBool, UI("Show special buttons",
		Description = "The Heal, Defense, Movement and Rotation buttons on the Control window. The button at the top right of the window shows and hides them too.",
		Parent = nameof(ShowControlWindow))]
	private static readonly bool _showControlWindowSpecials = true;

	[ConditionBool, UI("Show Next Action window", Filter = UiWindows)]
	private static readonly bool _showNextActionWindow = false;

	[ConditionBool, UI("Show Intercepted Action window",
		Description = "Shows the action you pressed that RSR is waiting to use.",
		Filter = UiWindows)]
	private static readonly bool _showInterceptedActionWindow = false;

	[ConditionBool, UI("Show Diagnostics Window",
		Description = "A small window that stays open in combat and shows why RSR does what it does: the current rotation's status lines, the AoE damage table's store and last rated hit, and why an enabled HP potion is or is not used.\nIn a fight: nothing RSR does changes. You can see during the pull whether a rule fired or what held it back, instead of opening the settings afterwards.",
		Filter = UiWindows)]
	private static readonly bool _showDiagnosticsWindow = false;

	[ConditionBool, UI("Make info windows click-through",
		Description = "Applies to the Next Action and Intercepted Action windows.",
		Filter = UiWindows, Section = 1)]
	private static readonly bool _isInfoWindowNoInputs = false;

	[ConditionBool, UI("Lock info windows in place",
		Description = "Applies to the Next Action and Intercepted Action windows.",
		Filter = UiWindows, Section = 1)]
	private static readonly bool _isInfoWindowNoMove = false;

	[ConditionBool, UI("Show keybind (Experimental)",
		Description = "Show the keybind of the hotbar slot that holds the next action. Only visible hotbars are checked.",
		Parent = nameof(ShowNextActionWindow))]
	private static readonly bool _showNextActionKeybind = false;

	[UI("Window size",
		Description = "Scales the whole Next Action window: the action icon, the GCD bar, the keybind and the target hint.",
		Parent = nameof(ShowNextActionWindow))]
	[Range(0.5f, 3f, ConfigUnitType.Percent, 0.01f)]
	public float NextActionWindowScale { get; set; } = 1f;

	[ConditionBool, UI("Use the game's cooldown overlay",
		Description = "Draws cooldowns on the Control window's action icons the way the game's hotbars do.",
		Parent = nameof(ShowControlWindow))]
	private static readonly bool _useOriginalCooldown = false;

	[ConditionBool, UI("Always show cooldowns on action icons",
		Description = "Applies to the Control and Next Action windows.",
		Filter = UiWindows, Section = 1)]
	private static readonly bool _showCooldownsAlways = false;

	[ConditionBool, UI("Show tooltips",
		Filter = UiInformation)]
	private static readonly bool _showTooltips = true;

	[ConditionBool, UI("Add Enable/Disable to the action right-click menu",
		Description = "Adds an entry to the game's action context menu that enables or disables the action in RSR.",
		Filter = UiHotbars)]
	private static readonly bool _showContext = true;

	[ConditionBool, UI("Show tips at the top of the settings window",
		Description = "A different tip shows every 7 seconds.",
		Filter = UiInformation)]
	private static readonly bool _showHints = true;

	[ConditionBool, UI("Tint disabled actions on hotbars",
		Description = "Actions you have disabled in RSR are tinted on the game's hotbars.",
		Filter = UiHotbars)]
	private static readonly bool _reddenDisabledHotbarActions = false;

	[UI("Tint color", Parent = nameof(ReddenDisabledHotbarActions))]
	public Vector4 HotbarDisabledTintColor { get; set; } = new(1f, 0f, 0f, 0.40f);

	[ConditionBool, UI("Show setting descriptions inline",
		Description = "Prints each setting's description underneath its name instead of only in the hover tooltip. Turn this off for a denser list.",
		Filter = UiCustomize)]
	private static readonly bool _uiInlineDescriptions = true;

	[UI("Interface accent colour",
		Description = "This acts as the primary color that all other colors are derived from.",
		Filter = UiCustomize)]
	public Vector4 UiAccentColor { get; set; } = new(0.690f, 0.125f, 0.122f, 1f);

	[UI("Text size",
		Description = "Scales the text in every RSR window, on top of Dalamud's own font settings.",
		Filter = UiCustomize)]
	[Range(0.75f, 1.75f, ConfigUnitType.Percent, 0.01f)]
	public float UiTextScale { get; set; } = 1f;

	[UI("Element size",
		Description = "Scales the padding, spacing and controls in every RSR window. Turn it down for a more compact layout; text keeps to the Text size setting.",
		Filter = UiCustomize)]
	[Range(0.75f, 1.75f, ConfigUnitType.Percent, 0.01f)]
	public float UiElementScale { get; set; } = 1f;

	[UI("Padding",
		Description = "Scales the space around and between things in every RSR window: window edges, card and row padding, and the gaps between items. Buttons, icons and text keep their size.",
		Filter = UiCustomize)]
	[Range(0.25f, 2f, ConfigUnitType.Percent, 0.01f)]
	public float UiPaddingScale { get; set; } = 1.5f;

	[ConditionBool, UI("Show a toast when a command queues an action",
		Description = "Confirms each action that a /rotation DoAction command adds to the queue.",
		Filter = BasicCommands, Section = 2)]
	private static readonly bool _showToastsAboutDoAction = false;

	/// <markdown file="Auto" name="Stop acting while an enemy casts a Gaze/Stop action" section="Action Usage and Control">
	/// This setting is linked with the <see cref="RotationSolver.Basic.Configuration.OtherConfiguration.HostileCastingStop">Gaze/Stop list.</see>
	/// </markdown>
	[ConditionBool, UI("Stop acting while an enemy casts a Gaze/Stop action (Experimental)",
		Description = "Uses the actions on the Gaze/Stop list.",
		Filter = AutoActionUsage, Section = 3)]
	private static readonly bool _castingStop = false;

	[UI("Stop this long before the cast finishes", Parent = nameof(CastingStop))]
	[Range(0, 15, ConfigUnitType.Seconds)]
	public float CastingStopTime { get; set; } = 2.5f;

	[ConditionBool, UI("Stop for the whole cast", Parent = nameof(CastingStop))]
	private static readonly bool _castingStopCalculate = false;

	/// <markdown file="Auto" name="Automatic healing thresholds" section="Healing Usage and Control" isSubsection="1">
	/// When enabled, you can customize the healing thresholds for when healing will be cast occur on target(s).
	/// </markdown>
	[ConditionBool, UI("Automatic healing thresholds", Filter = HealingActionCondition)]
	private static readonly bool _autoHeal = true;

	/// <markdown file="Auto" name="Stop single-target GCD heals once the target is above the threshold" section="Healing Usage and Control" isSubsection="1">
	/// When enabled, you can customize the healing thresholds for when healing will be cast occur on target(s).
	/// </markdown>
	[ConditionBool, UI("Stop single-target GCD heals once the target is above the threshold (Experimental)", Filter = HealingActionCondition, Description = "If another healer's healing puts the target above the healing threshold while you're casting, the cast is interrupted so you don't waste MP.")]
	private static readonly bool _stopHealingAfterThresholdExperimental2 = false;

	/// <markdown file="Auto" name="Use oGCD abilities" section="Action Usage and Control" isSubsection="1">
	/// Whether to use oGCD abilities or not at all.
	/// </markdown>
	[ConditionBool, UI("Use oGCD abilities", Filter = AutoActionUsage)]
	private static readonly bool _useAbility = true;

	[ConditionBool, UI("Use duty actions", Filter = AutoActionUsage,
		Description = "Allow the duty rotation to use duty actions, such as Bozja Lost Actions, Occult Crescent Phantom actions and Variant actions. Manually used or intercepted duty actions are unaffected.")]
	private static readonly bool _useDutyActions = true;

	[ConditionBool, UI("Burst automatically",
		Description = "RSR keeps the Burst state on, so rotations use their burst abilities as soon as they're ready. Turn this off to time burst yourself with the Burst special state.",
		Filter = AutoActionUsage)]
	private static readonly bool _autoBurst = true;

	[ConditionBool, UI("Check movement actions with BossModReborn (Experimental)",
		Description = "Before RSR uses a movement action, or an action that moves you, on its own, ask BossModReborn whether the destination is safe.",
		Filter = AutoActionUsage)]
	private static readonly bool _bmrSafetyCheckAuto = false;

	[ConditionBool, UI("Use defensive actions", Filter = AutoActionUsage, Section = 2, Description = "Turn this off if you'd rather plan defensive actions yourself, for example in raids.")]
	private static readonly bool _useDefenseAbility = true;

	[ConditionBool, UI("Use single-target defensive actions", Parent = nameof(UseDefenseAbility))]
	private static readonly bool _useSTDefense = true;

	[ConditionBool, UI("Use AoE defensive actions", Parent = nameof(UseDefenseAbility))]
	private static readonly bool _useAOEDefense = true;

	[ConditionBool, UI("Use BossModReborn's timeline for proactive mitigation",
		Description = "When BossModReborn is loaded, RSR uses its timeline to use defensive actions before raidwides and tankbusters hit.",
		Parent = nameof(UseDefenseAbility), Order = 2)]
	private static readonly bool _useBMRTimeline = false;

	// Gates when AutoStatus.DefenseArea gets set at all - job DefenseAreaAbility() methods aren't even
	// called before that, so this also caps how early their own BMRShouldRefreshBefore calls (which use
	// each status's own, often longer, duration as their upper bound) can actually trigger via the
	// BMR-only path. A real detected cast (IsHostileCastingAOE) is a separate, unaffected trigger.
	[UI("Area mitigation lead time", Description = "How long before a raidwide to use area mitigation.", Parent = nameof(UseBmrTimeline))]
	[Range(1, 15, ConfigUnitType.Seconds, 0.5f)]
	public float BMRRaidwideMitWindow { get; set; } = 5f;

	// Same caveat as BMRRaidwideMitWindow above, for AutoStatus.DefenseSingle/tankbuster mitigation. Not limited
	// to tanks as upstream has it: the healers' single-target defence and the emergency potion read it too (A341).
	[UI("Tankbuster mitigation lead time", Description = "How long before a tankbuster to use single-target mitigation.", Parent = nameof(UseBmrTimeline))]
	[Range(1, 10, ConfigUnitType.Seconds, 0.5f)]
	public float BMRTankbusterMitWindow { get; set; } = 3f;

	[UI("Anti-knockback lead time", Description = "How long before a knockback to use anti-knockback actions.", Parent = nameof(UseBmrTimeline))]
	[Range(1, 10, ConfigUnitType.Seconds, 0.5f)]
	public float BMRKnockbackWindow { get; set; } = 3f;

	[UI("Use tank mitigation when this many enemies target you", Parent = nameof(UseDefenseAbility), Order = 1,
		PvEFilter = JobFilterType.Tank)]
	[Range(1, 8, ConfigUnitType.None, 0.05f)]
	public int AutoDefenseNumber { get; set; } = 2;

	[UI("Use tank mitigation when this many enemies target you for enemy mitigation sustain-refresh (Addle/Feint/Reprisal)", Parent = nameof(UseDefenseAbility))]
	[Range(1, 8, ConfigUnitType.None, 1)]
	public int MitigationSustainHostileCount { get; set; } = 4;

	[JobConfig, Range(0, 1, ConfigUnitType.Percent, 0.02f)]
	[UI("Use tank mitigation below this HP", Parent = nameof(UseDefenseAbility),
		PvEFilter = JobFilterType.Tank)]
	private readonly float _healthForAutoDefense = 1;

	[ConditionBool, UI("Use tank stance", Parent = nameof(UseAbility),
		PvEFilter = JobFilterType.Tank)]
	private static readonly bool _autoTankStance = true;

	[ConditionBool, UI("Provoke when there is another tank in the party", Description = "Provoke an enemy that is attacking a non-tank party member, while there is more than one tank in the party.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _autoProvokeForTank = true;

	[ConditionBool, UI("Take the enemy from a co-tank in danger",
		Description = "With another tank in the party, RSR provokes an enemy attacking the co-tank when\n- his HP with shield are at or below the HP you set for a tank to use invulnerability;\n- BossModReborn announces its next tankbuster and he would take it still vulnerable, or would not survive it by his HP trend - at once, because a cast already aimed at him or a marker on him can no longer be turned away;\n- he shirks to you: he does not want the next hit.\nNot while you are in danger or vulnerable yourself, not while he is impervious, not when the announced tankbuster hits you anyway, and on an announcement not again for an enemy whose tankbuster still hit him after a take-over. Your tank stance goes on first if it is off.\nIn a fight: the next tankbuster lands on you instead of the endangered co-tank. Off: he is left to his healers and his own cooldowns.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _takeEnemyFromCoTankInDanger = true;

	[ConditionBool, UI("Shirk the co-tank after a tankbuster that leaves you in danger",
		Description = "With another tank in the party: after a tankbuster on you, if it left you with a vulnerability debuff or so low that a repeat of the hardest one measured would kill you, and your invulnerability is not ready, Shirk goes to the other tank - but only when the party's enmity on the enemy shows it will then attack that tank, because below that Shirk moves nothing, and never to a tank who is in danger himself. With several, the one fewest enemies are attacking, then the one with most HP. Once the co-tank has taken the enemy's next tankbuster, your debuff has run out, you could survive the buster again and your invulnerability is ready, RSR provokes the enemy back.\nIn a fight: the next hit goes to the co-tank instead of killing you, and you take the enemy back when you are safe. Off: you keep the enemy unless your co-tank provokes it.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _shirkToSwapAfterTankbuster = true;

	[ConditionBool, UI("Use the invulnerability before a tankbuster nothing less survives",
		Description = "When a tankbuster is cast at you that the learned tankbuster table has measured, and no set of your own mitigations that is ready by the hit would let you survive it - at your HP now, with only what still stands when it lands, a co-tank's Reprisal or a healer's barrier included -, your invulnerability goes out before it lands, no earlier than its own duration less one GCD before the hit. With \"Spend only the mitigation a measured tankbuster needs\" off, nothing presses such a set for you, so only what already stands or has gone out counts. It needs the invulnerability enabled, learned and off cooldown, not the dying-tank threshold. A tankbuster marker with no cast counts once the same marker in the same area has been followed by a hit the table measured: that hit is taken as coming, timed from the marker. A BossModReborn prediction, an action not yet measured and a marker seen for the first time get nothing: none of them names a measured action. Living Dead included: after the hit, the healers have to restore your full HP within Walking Dead, or you fall.\nIn a fight: you survive a hit that no mitigation of yours would have let you survive, instead of the invulnerability firing only once your HP is already under the threshold - and it is not spent where mitigation does the job.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _invulnerabilityBeforeLethalTankbuster = true;

	[ConditionBool, UI("Spread Reprisal and the party mitigation over a run of area attacks",
		Description = "As a tank: after Reprisal or your party mitigation - Divine Veil, Passage of Arms, Shake It Off, Dark Missionary, Heart of Light -, the other waits while the first still works, a barrier until it is used up, so the next area attack gets one as well. Both go together when a party member is in danger, or the announced area attack, as measured, would put an unprotected member there.\nIn a fight: two area attacks in a row each get a tool instead of the first getting both and the second none; a big one still gets both.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _spreadTankAreaDefense = true;

	[ConditionBool, UI("Spend only the mitigation a measured tankbuster needs",
		Description = "As a tank: against a tankbuster the table has measured, RSR spends the cheapest set of your own mitigations - by their cooldown - that lets you survive it at your HP now, with only what still stands when it lands; several at once if one is not enough, none if the hit is survivable as it stands. While that tankbuster is the only one known to be coming at you, every other mitigation, barrier or Reprisal aimed at yourself is held for the next one, from any part of the rotation; help for another member, heals and area defence that reaches beyond you stay. Party tools such as Shake It Off or Divine Veil stay with the area defence. Only when nothing survives it and the invulnerability is not there to use - on cooldown, switched off, or refused with no retry left before the last GCD before the hit - does everything go, never on top of the invulnerability. If a planned mitigation has not gone out by the last GCD before the hit, the hold opens. Against a tankbuster marker with no cast, met with the hit measured after the same marker before, nothing else is held except under a planned invulnerability: the marker may announce another hit this time. Under Hallowed Ground or Superbolide past the hit nothing aimed at you is spent. As a healer: no single-target mitigation for a tankbuster cast at a tank who stands under Hallowed Ground or Superbolide past the hit; a BossModReborn prediction still opens it.\nIn a fight: a tankbuster that needs two mitigations gets both, one that needs one gets one, and Rampart, Vengeance and the like stay ready for the next.",
		Parent = nameof(UseAbility))]
	private static readonly bool _holdMitigationUnderInvulnerability = true;

	/// <markdown file="Auto" name="Use True North" section="Action Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._useAbility">
	/// Whether to cast True North when playing as a melee DPS when you do not have the right
	/// positional on the enemy.
	/// </markdown>
	[ConditionBool, UI("Use True North",
		Parent = nameof(UseAbility),
		PvEFilter = JobFilterType.Melee)]
	private static readonly bool _autoUseTrueNorth = true;

	[ConditionBool, UI("Use movement speed abilities out of combat in duties", Parent = nameof(UseAbility))]
	private static readonly bool _autoSpeedOutOfCombat = true;

	[ConditionBool, UI("Use movement speed abilities out of combat outside duties", Parent = nameof(UseAbility))]
	private static readonly bool _autoSpeedOutOfCombatNoDuty = false;

	[ConditionBool, UI("Sprint when a party tank sprints (non-tanks only)", Parent = nameof(UseAbility))]
	private static readonly bool _autoSprintWithTank = false;

	[ConditionBool, UI("Use beneficial ground-targeted actions", Description = "1.    Self-Target Fallback:\r\nIf range is zero, always targets the player and returns all affectable targets at the player's position.\r\n2.    Preferred Positions (OnLocations):\r\n•    Tries to get predefined beneficial positions for the current territory.\r\n•    If none are found and the content is a trial or raid, uses fallback points (e.g., 0,0 or 100,100 point as those are the center of arenas most of the time).\r\n•    Picks the closest point to the player, applies a small random offset, and checks if it’s within effect range.\r\n•    If so, returns that as the target area.\r\n3.    Boss Positional Fallback:\r\n•    If the current target is a boss with positional requirements and within range, uses the boss’s position (or a point within range) as the target area.\r\n4.    Party Member Fallback:\r\n•    Gathers party members within range + effect range.\r\n•    Attempts to find a party member who is being attacked (tank or focus target).\r\n•    If found, calculates whether to stay at the player’s position or move closer to the tank, based on distances and effect range.\r\n•    If not found or not needed, defaults to the player’s position.", Filter = HealingActionCondition, Section = 5)]
	private static readonly bool _useGroundBeneficialAbility = true;

	/// <markdown file="Auto" name="Use them while moving" section="Healing Usage and Control">
	/// Enable to allow the usage of ground AoE actions while moving, such as Earthly Star (AST), Sacred Soil (SCH), etc.
	/// </markdown>
	[JobConfig, UI("Use them while moving", Parent = nameof(UseGroundBeneficialAbility))]
	private static readonly bool _useGroundBeneficialAbilityWhenMoving = false;

	[JobConfig, UI("Always place them on yourself", Description = "Skips RSR's usual placement logic.", Parent = nameof(UseGroundBeneficialAbility))]
	private static readonly bool _useGroundBeneficialAbilityOnlySelf = false;

	[JobConfig, UI("Place them on the party tank when there is one", Description = "Skips RSR's usual placement logic.", Parent = nameof(UseGroundBeneficialAbility))]
	private static readonly bool _useTargetTankForGroundHeal = false;

	[ConditionBool, UI("Show Action Timeline window",
		Description = "A scrolling record of the actions you use: GCDs on the top lane with their cast and recast, oGCDs beneath them with their animation lock.",
		Filter = UiWindows)]
	private static readonly bool _showActionTimelineWindow = false;

	[ConditionBool, UI("Lock timeline",
		Description = "Stops the timeline from being moved or resized. Hover over it to unlock it again.",
		Parent = nameof(ShowActionTimelineWindow))]
	private static readonly bool _isActionTimelineLock = false;

	[UI("Timeline size",
		Description = "Scales the whole timeline: the icons, the lanes and how far a second stretches.",
		Parent = nameof(ShowActionTimelineWindow))]
	[Range(0.5f, 3f, ConfigUnitType.Percent, 0.01f)]
	public float ActionTimelineWindowScale { get; set; } = 1f;

	[ConditionBool, UI("Only show timeline in combat", Parent = nameof(ShowActionTimelineWindow))]
	private static readonly bool _actionTimelineOnlyInCombat = true;

	[ConditionBool, UI("Only show timeline when RSR is active", Parent = nameof(ShowActionTimelineWindow))]
	private static readonly bool _actionTimelineOnlyWhenActive = true;

	[ConditionBool, UI("Show oGCD actions in timeline", Parent = nameof(ShowActionTimelineWindow))]
	private static readonly bool _actionTimelineShowOGCD = true;

	[ConditionBool, UI("Show auto-attacks in timeline", Parent = nameof(ShowActionTimelineWindow))]
	private static readonly bool _actionTimelineShowAutoAttack = false;

	[ConditionBool, UI("Save timeline to JSON file after combat", Parent = nameof(ShowActionTimelineWindow))]
	private static readonly bool _actionTimelineSaveToFile = false;

	[ConditionBool, UI("Record enemy AoE actions", Filter = ListCastingArea)]
	private static readonly bool _recordCastingArea = true;

	[ConditionBool, UI("Turn off after combat ends",
		Filter = BasicAutoSwitch)]
	private static readonly bool _autoOffAfterCombat = true;

	[ConditionBool, UI("Count RSR's action clicks",
		Description = "Shows how many times RSR has clicked actions for you on the About page.",
		Filter = UiInformation)]
	private static readonly bool _enableClickingCount = true;

	[ConditionBool, UI("Hide all warnings",
		Filter = UiInformation)]
	private static readonly bool _hideWarning = false;

	/// <markdown file="Auto" name="Only heal yourself as a non-healer" section="Healing Usage and Control">
	/// When enabled and not a healer, skills that can target someone and heal will only target you.
	/// </markdown>
	[ConditionBool, UI("Only heal yourself as a non-healer",
		Filter = HealingActionCondition, Section = 4)]
	private static readonly bool _onlyHealSelfWhenNoHealer = false;

	[ConditionBool, UI("Only heal as a non-healer when no healer is alive",
	Description = "Tanks and DPS only use healing abilities when the party has no healer, or every healer is dead.",
	Filter = HealingActionCondition, Section = 4)]
	private static readonly bool _onlyHealAsNonHealIfNoHealers = false;

	[ConditionBool, UI("Show changed settings in chat",
		Description = "Prints the setting and its new value when a /rotation Settings command changes it.",
		Filter = BasicCommands, Section = 2)]
	private static readonly bool _ShowToggledSettingInChat = false;

	[ConditionBool, UI("Record enemy knockback actions", Filter = ListKnockback)]
	private static readonly bool _recordKnockbackies = false;

	#region Float
	[UI("Turn off after this long out of combat",
		Parent = nameof(AutoOffAfterCombat))]
	[Range(0, 600, ConfigUnitType.Seconds)]
	public float AutoOffAfterCombatTime { get; set; } = 30;

	[UI("Vision cone angle", Parent = nameof(OnlyAttackInVisionCone))]
	[Range(0, 90, ConfigUnitType.Degree, 0.02f)]
	public float AngleOfVisionCone { get; set; } = 45;

	/// <markdown file="Auto" name="Melee ranged action offset" section="Action Usage and Control">
	/// Additional buffer in yalms where ranged attacks will be used for melee classes. For example,
	/// if you are playing as Samurai and this setting is set to `1`, Enpi will only be used starting
	/// at 4 yalms.
	///
	/// This is because the default "overflow" for the melee range is 3 yalms, which means you can cast
	/// melee attacks 3 yalms outside the enemy's hitbox. This setting (the offset) takes max melee range and
	/// adds the value you set.
	///
	/// This setting exists to leave you time to approach and enter in melee range of the enemy without wasting
	/// a GCD.
	/// </markdown>
	[UI("Melee ranged action offset",
		Description = "Melee jobs use their ranged attack only when the target is this much farther than melee range.",
		Filter = AutoActionUsage, Order = 1,
		PvEFilter = JobFilterType.Melee, PvPFilter = JobFilterType.Melee)]
	[Range(0, 5, ConfigUnitType.Yalms, 0.02f)]
	public float MeleeRangedOffset { get; set; } = 1;

	[UI("When the lowest party HP is below", Parent = nameof(HealWhenNothingTodo))]
	[Range(0, 1, ConfigUnitType.Percent, 0.002f)]
	public float HealWhenNothingTodoBelow { get; set; } = 0.8f;

	[UI("Heal tanks first below this HP",
		Filter = HealingActionCondition, Section = 1)]
	[Range(0, 1, ConfigUnitType.Percent, 0.02f)]
	public float HealthTankRatio { get; set; } = 0.45f;

	[UI("Heal healers first below this HP",
		Filter = HealingActionCondition, Section = 1)]
	[Range(0, 1, ConfigUnitType.Percent, 0.02f)]
	public float HealthHealerRatio { get; set; } = 0.4f;

	// A target riding an invulnerability cannot die right now, but keeps whatever health it has when
	// the window closes - Superbolide puts the gunbreaker at 1 HP on purpose. So the threshold drops
	// to this value instead of healing being suppressed for the whole window. It is clamped against
	// the normal threshold, so a protected target is never healed more readily than an unprotected
	// one, and it governs the automatic decision only: a manual heal command and the Doom
	// short-circuit still reach a protected target above this value, which is the point of both.
	//
	// Living Dead is the exception and this setting does not apply to it in either position of
	// WithholdHealingForLivingDead. On, the bearer is held in full rather than merely demoted,
	// because there the death is the trigger and not the disaster; that hold releases on its own
	// clock, measured on Living Dead alone (StatusHelper.InDeathTriggerWindow). Off, he is healed
	// like an unprotected member (ObjectHelper.IsUnprotectedForHealing): his fall to zero is not a
	// spared hit but Walking Dead, and that setting's text promises he is healed normally (A306).
	[UI("Heal a target under an invulnerability only below this HP.",
		Description = "Living Dead follows \"Withhold healing under Living Dead\" instead.",
		Filter = HealingActionCondition, Section = 1)]
	[Range(0, 1, ConfigUnitType.Percent, 0.02f)]
	public float HealthProtectedRatio { get; set; } = 0.15f;

	// Living Dead is the one invulnerability whose trigger is the bearer's own death: dying converts
	// it into Walking Dead. Healing the dark knight above zero while it is up removes that trigger.
	//
	// What phase two demands is quoted from the action's own effect text (ActionId.resx, action
	// 3638), not from memory: Living Dead lasts 10s, Walking Dead another 10s, "most attacks will
	// not lower your HP below 1", and "if before the Walking Dead timer runs out an amount of HP
	// totaling your maximum HP is restored the effect will change to Undead Rebirth. If this amount
	// is not restored you will be KO'd." The bearer contributes to that total himself - "Restores HP
	// with each weaponskill successfully delivered or spell cast, Cure Potency: 1500" - so the
	// healer carries the remainder, not the whole of it. Which is also why the hold is not a
	// gamble on one heal: a white mage's Benediction restores the target fully on its own.
	//
	// Off by default, and deliberately so - RSR fires Living Dead itself as a last-ditch save at
	// HealthForDyingTanks (DarkKnightRotation.EmergencyAbility), and under that usage the death is
	// not wanted at all. Holding the heal is right where the death was the plan. Whoever knows that
	// for their group turns this on.
	[UI("Withhold healing under Living Dead",
		Description = "On: a dark knight under Living Dead is left at 1 HP instead of being healed up, "
			+ "so the killing blow lands and converts the effect into Walking Dead. From that moment "
			+ "healing totalling his maximum HP has to go in within ten seconds or he is KO'd for real - "
			+ "he contributes to that himself with every spell and weaponskill, and a Benediction covers "
			+ "the rest on its own.\n"
			+ "Off: he is healed normally, the death never happens, and Living Dead runs out without "
			+ "turning into Walking Dead.\n"
			+ "Only switch this on if your group plays it that way. RSR also fires Living Dead by itself "
			+ "as a last-ditch save on a dying tank, and in that use the death is not wanted at all.",
		Filter = HealingActionCondition, Section = 1)]
	public bool WithholdHealingForLivingDead { get; set; } = false;

	[UI("Heal yourself first below this HP",
		Filter = HealingActionCondition, Section = 1)]
	[Range(0, 1, ConfigUnitType.Percent, 0.02f)]
	public float HealthSelfRatio { get; set; } = 0.4f;

	// Every health threshold above is a level, and what kills a tank in a wall-to-wall pull is a
	// rate. A member falling fast crosses its threshold with less time left than the heal that
	// threshold triggers needs to arrive - the rest of the GCD, then the cast - so the cast goes
	// out after the death. With this on, the thresholds and the heal target selection read the
	// health each member is headed for by the time a heal begun now would land, taken from the
	// observed trend rather than from any table: ObjectHelper.GetForecastSurvivingShare.
	//
	// It changes nothing while a party is held steady. The trend is measured net of every
	// mitigation, barrier and foreign heal, so health that is not falling on balance yields no
	// look-ahead at all; it appears when the net trend turns downward and grows as it steepens.
	//
	// On by default: the owner's rule is that a default is the value that plays better, so new rules
	// get tested (29.09.2026). The effect is not proven in play; with the health held steady it is
	// exactly the old behaviour.
	[UI("Heal ahead of incoming damage",
		Description = "Every healing threshold and the heal target choice read the health a member is "
			+ "heading for by the time a heal started now would land, instead of the health shown right "
			+ "now.\n"
			+ "In a fight: a tank dropping fast gets his heal about one GCD earlier, and someone falling "
			+ "quickly is picked ahead of someone sitting lower but steady - the case where a cast used "
			+ "to go out after the death.\n"
			+ "While a party is held steady it changes nothing. The trend is measured net of every "
			+ "mitigation, barrier and outside heal, so health that is not falling on balance produces "
			+ "no look-ahead at all; it appears when the net trend turns downward and grows as it "
			+ "steepens.",
		Filter = HealingActionCondition, Section = 1)]
	public bool HealAheadOfDamage { get; set; } = true;

	// The owner's triage (concept 07): whoever is closest to dying first, and at equal danger healer
	// before tank before damage dealer. Class 1 - about to fall - is built and always on. This adds
	// classes 2 and 3 in place of the role short-cuts, which pick a role outright once it is under its
	// ratio and look at nobody else. On by default - the owner's rule for defaults (29.09.2026), and
	// his own order of who is healed first; that it keeps more people alive is not shown in play.
	[UI("Choose the heal target by danger",
		Description = "Below a member who is about to fall, heal first a healer or tank who is under their "
			+ "role threshold AND being attacked, together with any healer or tank being attacked who stands as "
			+ "low or lower - lowest health first, a healer before a tank at equal health. Everyone else after that: while an area cast is announced, the one with the fewest "
			+ "hit points left first, because the hit takes the same number from everybody; otherwise the "
			+ "lowest percentage.\n"
			+ "In a fight: when the healer holds aggro and the tank does not, the healer comes first; a tank "
			+ "under his threshold but not being hit no longer jumps ahead of a damage dealer who is lower.\n"
			+ "Off: the role thresholds decide outright, as before.",
		Filter = HealingActionCondition, Section = 1)]
	public bool HealTargetByDanger { get; set; } = true;

	// The learned area list holds everything that once hit the whole party, from a raidwide taking
	// sixty percent to a trash tick taking two, and every entry raised the same party mitigation.
	// What that spends is the cooldown: a Reprisal laid on a trivial tick is missing at the next
	// real hit. With this on, an action whose measured damage would leave everybody above the level
	// at which the tree heals anyway does not raise it.
	//
	// It reads what Watcher records, so an action nothing has been measured on behaves exactly as
	// before - which is how the 850 shipped entries keep their mitigation. Ratings arrive with play:
	// in content that is repeated, one clear of the fight.
	//
	// On by default: it implements a shortcoming the user reported, and its fallback in every
	// unknown case is the old behaviour. Whoever wants mitigation on everything turns it off.
	[UI("Skip mitigation for small area casts",
		Description = "A learned area cast whose measured damage would leave everybody above the level "
			+ "at which the tree heals anyway no longer raises the party mitigation.\n"
			+ "In a fight: Reprisal, Addle and the rest stay off cooldown and are ready for the next "
			+ "real hit, instead of being spent on a two-percent tick. A hit that would push anyone to "
			+ "where healing is called for is still mitigated in full.\n"
			+ "An action nothing has been measured on behaves exactly as before, which is what keeps the "
			+ "850 shipped entries mitigated. Ratings arrive with play - in content you repeat, one "
			+ "clear of the fight. The AOE list in Lists shows the measured share per entry and which "
			+ "of them have already saved a cooldown.",
		Filter = HealingActionCondition, Section = 1)]
	public bool SkipMitigationForSmallAreaCasts { get; set; } = true;

	// The setting above uses the measured size of an area cast to decide whether to MITIGATE. This
	// one uses the same figure for the other half of the answer: whether to HEAL first.
	//
	// Every healing threshold reads the health a member has. None of them reads the health he will
	// have when the cast already on screen lands, so the hit counts for nothing until it has landed:
	// a member at 55% in front of a 45% raidwide is left at ten percent before any threshold sees it.
	// "Heal ahead of incoming damage" above does not cover this: it extrapolates the observed trend,
	// and a cast that has not landed yet leaves no trend.
	//
	// The owner's rule, and the reason this comes before mitigation rather than after: "die aktuelle
	// hp liegt unter dem schadenswert. dann wäre aber eine heilung sinnvoll bis max maxhp. wenn dann
	// die hp unter dem schadenswert liegt, sollte zusätzlich geschildet werden."
	//
	// Asked where the hit would leave an unprotected member at or below HealthForDyingTanks (the
	// critical class, concept 07) while he still has health to restore - the owner's "unter dem
	// schadenswert", on the safe side as the holds' barrier reads it (A283). Asked
	// at the area heal thresholds before, it fired on a full party before every raidwide big enough
	// to cross them, and the heals landed on full health (A292). Area casts only: a hit that reaches
	// everybody is answered by area healing.
	//
	// On by default - the owner's rule for defaults (29.09.2026). Nothing changes for an action whose
	// size has never been measured.
	[UI("Heal ahead of an announced area cast",
		Description = "Raises the area healing flags when the area cast currently being announced would "
			+ "leave somebody about to fall - the members \"Choose the heal target by danger\" heals "
			+ "first - and that member is missing health a heal can restore.\n"
			+ "In a fight: a member at fifty-five percent in front of a forty-five percent hit is healed "
			+ "BEFORE the raidwide lands instead of being left at ten percent by it. A hit that leaves "
			+ "everybody above that line is healed after it lands; this setting sends no heal ahead of it.\n"
			+ "Uses the measured size of that action and counts an existing barrier against it, since "
			+ "the barrier absorbs this particular hit. An action whose size has never been measured "
			+ "changes nothing - ratings arrive with play, one clear of the fight.",
		Filter = HealingActionCondition, Section = 1)]
	public bool HealAheadOfAnnouncedHit { get; set; } = true;

	// The AoE list learns an action once it hit every party member, and cannot tell a raidwide from an
	// area the party could have dodged, or from a hit centred on someone else that happened to catch
	// everyone once. The resolution the TODO named was an observation in play; the fight can measure
	// it instead (A205): when a listed cast lands and leaves no damage entry on the living player, it
	// did not reach him, and the next cast of it opens no area defence for him - until one does reach
	// him again. On by default - the owner's rule for defaults (29.09.2026); the risk is a cast dodged
	// last time and not dodged now, met without the area defence.
	[UI("Skip area defence for casts that missed you",
		Description = "An area cast from the AoE list that missed you last time - you dodged it, or it was "
			+ "centred on someone else - does not open your area defence the next time. As soon as it "
			+ "reaches you once, it counts again; a hit you blocked, parried, absorbed or took under "
			+ "invulnerability has reached you.\n"
			+ "In a fight: party mitigation and your own area defensives are kept for the casts that "
			+ "actually reach you. The cost: the first cast of each action in a session still opens "
			+ "them, and a cast you dodged last time and fail to dodge now is met without them.\n"
			+ "Off: every listed cast that can reach you opens the area defence, as before.",
		Filter = HealingActionCondition, Section = 1)]
	public bool SkipAreaCastsThatMissedMe { get; set; } = true;

	// Reported from a four-player dungeon: Addle and Radiant Aegis go out against some area casts and
	// not others. Traced to the pre-filter every area question runs through - it drops any cast that
	// is INTERRUPTIBLE, on the reasoning that an interruptible cast gets interrupted and mitigating it
	// would waste a cooldown. That reasoning holds only while somebody interrupts. A Summoner has no
	// interrupt; in a four-player party with a tank who does not use Interject, the cast lands and
	// nothing answered it.
	//
	// Not a return to the enemy-count fallback removed in A9. That one raised the defence from the
	// number of enemies - no evidence of danger, and it fired with nothing happening. This requires a
	// figure measured from an actual landing, at or above what the largest barrier in the game
	// absorbs, and only within a GCD of the hit. A cast nobody has been hit by opens nothing.
	//
	// It was not buildable then: the per-action damage share did not exist, so the pre-filter had to
	// carry the coarseness alone.
	//
	// On by default - the owner's rule for defaults (29.09.2026). Whether the extra cover is worth the
	// cooldown is not established from the code.
	[UI("Mitigate a big area cast even when it is interruptible",
		Description = "Raises the area defence for an interruptible cast too, if its measured damage is "
			+ "at or above what the largest barrier in the game absorbs and it lands within about one "
			+ "GCD.\n"
			+ "In a fight: in a dungeon where nobody interrupts, the big pull-wide gets Addle and "
			+ "Radiant Aegis instead of passing unanswered. Small casts, unmeasured casts and casts "
			+ "further out than a GCD are unaffected, so it cannot fire on trash that is doing nothing.\n"
			+ "Leaves the healer's threat detection alone: it is a separate path, so the White Mage's "
			+ "Benediction guard does not widen with it.",
		Filter = HealingActionCondition, Section = 1)]
	public bool MitigateBigAreaCastsEvenIfInterruptible { get; set; } = true;

	// BossModReborn answers WHEN the next damage lands, never HOW HARD. Every proactive mitigation in
	// the tree reads that timing and nothing else, so two hits in a row are answered on the first
	// one regardless of which is worse.
	//
	// Reported from play, Eternal Queen's opening: a small area cast, then a big one. The prediction
	// fires on the first, Radiant Aegis or Tactician is spent on it, and the big one arrives with
	// the barrier eaten or the debuff expired. A barrier is the plain case - it absorbs points, so a
	// small hit consumes it outright.
	//
	// The size of a cast already on screen IS known: it is measured per action. With this on, a
	// proactive refresh stands down while a rated SMALL area cast is running, and takes the next
	// event instead. Where nothing is running, or the running cast has never been measured, nothing
	// changes.
	//
	// This closes the hole the size rating left: the reactive path has asked "is this hit worth a
	// cooldown" since the large-barrier threshold went in, the proactive path never did.
	//
	// On by default - the owner's rule for defaults (29.09.2026) - and honestly a heuristic: nothing
	// here proves the prediction refers to the cast that happens to be running.
	[UI("Hold a predicted mitigation while a small cast is running",
		Description = "Stops a mitigation that fires from a BossMod prediction from being spent on a "
			+ "small area cast when a bigger one is coming right behind it.\n"
			+ "In a fight: in an opening with a small area cast followed by a heavy one, Radiant "
			+ "Aegis, Tactician, Troubadour or Shield Samba wait for the second instead of being "
			+ "eaten by the first. Only applies while a cast is running whose damage has actually "
			+ "been measured and came out small; otherwise the prediction is followed as before.\n"
			+ "A judgement call, not a certainty: BossMod gives the timing of the next event, not "
			+ "which cast it means.",
		Filter = HealingActionCondition, Section = 1)]
	public bool HoldProactiveMitigationForSmallCast { get; set; } = true;

	[UI("Dispel delay range",
		Description = "RSR waits a random time in this range before dispelling.",
		Filter = AutoActionUsage, Section = 4, Order = 1,
		PvEFilter = JobFilterType.Dispel, PvPFilter = JobFilterType.NoJob)]
	[Range(0, 10, ConfigUnitType.Seconds, 0.002f)]
	public Vector2 EsunaDelay { get; set; } = new(0f, 0f);

	#region Raise
	[JobConfig, UI("Raise before healing or mitigating",
		Filter = RaiseActionCondition)]
	private static readonly bool _raisePlayerFirst = false;

	[JobConfig, UI("Raise with Swiftcast or Dualcast", Description = "If this is off, RSR never uses Swiftcast or Dualcast to raise.",
		Filter = RaiseActionCondition)]
	private static readonly bool _raisePlayerBySwift = true;

	[JobConfig, UI("Hard-cast raise",
		Filter = RaiseActionCondition)]
	private readonly HardCastRaiseType _HardCastRaiseType = HardCastRaiseType.HardCastNormal;

	[JobConfig, UI("Who to raise",
		Filter = RaiseActionCondition)]
	private readonly RaiseType _RaiseType = RaiseType.PartyOnly;

	[JobConfig, UI("Raise players with Brink of Death",
		Filter = RaiseActionCondition)]
	private static readonly bool _raiseBrinkOfDeath = true;

	[JobConfig, UI("Raise non-healers from the bottom of the party list up",
		Description = "For the second healer in a light party.",
		Filter = RaiseActionCondition)]
	private static readonly bool _h2 = false;

	[JobConfig, UI("Raise Red Mages and Summoners first when no tanks or healers are dead",
		Filter = RaiseActionCondition)]
	private static readonly bool _offRaiserRaise = false;

	/// <markdown file="Auto" name="Raise delay range" section="Raise Usage and Control">
	/// In order to not make is so obvious that you use RSR, casting a raise action will be delayed by a random amount of seconds
	/// between the two values.
	/// </markdown>
	[UI("Raise delay range",
		Description = "RSR waits a random time in this range before raising.",
		Filter = RaiseActionCondition, Section = 1)]
	[Range(0, 10, ConfigUnitType.Seconds, 0.002f)]
	public Vector2 RaiseDelay2 { get; set; } = new(3f, 3f);

	[Range(0, 10000, ConfigUnitType.None, 100)]
	[UI("Don't raise below this MP",
		Filter = RaiseActionCondition, Section = 1)]
	public int LessMPNoRaise { get; set; } = 2400;
	#endregion

	/// <markdown file="Auto" name="AoE heal HP spread" section="Healing Usage and Control">
	/// Controls how much party members' HP must differ before using AoE healing instead
	/// of single-target heals. Lower values require party members to have more similar HP
	/// for AoE healing to trigger (more selective). Higher values allow AoE healing even
	/// when HP differences are larger (less selective). Adjust only if you want to fine-tune
	/// AoE heal behavior.
	/// </markdown>
	[UI("AoE heal HP spread", Description = "The largest standard deviation of party HP at which RSR still uses AoE heals instead of single-target heals. Lower values need the party's HP to be closer together. Adjust only if you want to fine-tune AoE heal behavior.",
		Filter = HealingActionCondition, Section = 1)]
	[Range(0, 0.5f, ConfigUnitType.Percent, 0.02f)]
	public float HealthDifference { get; set; } = 0.25f;

	[ConditionBool, UI("Heal party members out of combat",
		Filter = HealingActionCondition, Section = 2)]
	private static readonly bool _healOutOfCombat = false;

	[ConditionBool, UI("Heal solo instance NPCs (Experimental)", Description = "Only enable as needed.",
		Filter = HealingActionCondition, Section = 3, Order = 1)]
	private static readonly bool _friendlyBattleNPCHeal = false;

	[ConditionBool, UI("Heal and raise party NPCs",
		Filter = HealingActionCondition, Section = 3)]
	private static readonly bool _friendlyPartyNPCHealRaise3 = true;

	[ConditionBool, UI("Treat your chocobo as a party member",
		Filter = HealingActionCondition, Section = 3, Order = 1)]
	private static readonly bool _chocoboPartyMember = false;

	[ConditionBool, UI("Treat your focus target as a party member in alliance raids (Experimental)", Description = "Includes chaotic alliance raids.",
		Filter = HealingActionCondition, Section = 3, Order = 1)]
	private static readonly bool _focusTargetIsParty = false;

	[ConditionBool, UI("Use GCD heals when there's nothing else to do",
		Filter = HealingActionCondition, Section = 2)]
	private static readonly bool _healWhenNothingTodo = true;

	[ConditionBool, UI("Give tankbuster mitigation to the lower-HP tank",
		Filter = HealingActionCondition, Section = 1)]
	private static readonly bool _priolowtank = false;

	/// <markdown file="Basic" name="Special state duration">
	/// The duration of special windows opened by /rotation commands by default.
	/// (Found in Main => Macros)
	/// </markdown>
	[UI("Special state duration",
		Description = "How long a special state, such as Heal Area or Burst, lasts when a /rotation Special command turns it on.",
		Filter = BasicCommands, Section = 1)]
	[Range(1, 20, ConfigUnitType.Seconds, 1f)]
	public float SpecialDuration { get; set; } = 3;

	[UI("Stop casting delay range", Parent = nameof(UseStopCasting))]
	[Range(0, 3, ConfigUnitType.Seconds, 0.002f)]
	public Vector2 StopCastingDelay { get; set; } = new(0.5f, 1);

	[UI("Interrupt delay range", Parent = nameof(InterruptibleMoreCheck),
		PvEFilter = JobFilterType.Interrupt, PvPFilter = JobFilterType.NoJob)]
	[Range(0, 3, ConfigUnitType.Seconds, 0.002f)]
	// Default none: an interrupt that waits can miss its cast, and the setting exists to look less mechanical, not to
	// play better (concept 11, A338).
	public Vector2 InterruptDelay { get; set; } = new(0, 0);

	[UI("Provoke delay range", Parent = nameof(AutoProvokeForTank))]
	[Range(0, 10, ConfigUnitType.Seconds, 0.05f)]
	// Default none, as for InterruptDelay: a provoke that waits leaves the enemy on whoever took it longer.
	public Vector2 ProvokeDelay { get; set; } = new(0, 0);

	[UI("Downtime healing delay range", Parent = nameof(HealWhenNothingTodo))]
	[Range(0, 5, ConfigUnitType.Seconds, 0.05f)]
	public Vector2 HealWhenNothingTodoDelay { get; set; } = new(0.5f, 1);

	/// <markdown file="Basic" name="Countdown lead time">
	/// How soon before countdown is finished to start casting or attacking.
	/// </markdown>
	[UI("Countdown lead time",
		Description = "How long before the countdown ends RSR starts casting or attacking.",
		Filter = BasicTimer, Section = 1, PvPFilter = JobFilterType.NoJob)]
	[Range(0, 0.7f, ConfigUnitType.Seconds, 0.002f)]
	public float CountDownAhead { get; set; } = 0.4f;

	/// <markdown file="Basic" name="Delay between clicks">
	/// Delay between showing the clicking/pressing effect on the actions in hotbars.
	/// </markdown>
	[UI("Delay between clicks",
		Description = "RSR waits a random time in this range between using actions. It also spaces out the simulated hotbar presses.",
		Filter = BasicTimer, Section = 1)]
	[Range(0.00f, 0.25f, ConfigUnitType.Seconds, 0.002f)]
	public Vector2 ClickingDelay { get; set; } = new(0.1f, 0.2f);

	[UI("Out of combat delay range",
		Description = "How long after combat ends RSR waits before using out-of-combat actions, such as speed buffs.",
		Filter = BasicTimer, Section = 1)]
	[Range(0, 10, ConfigUnitType.Seconds, 0.002f)]
	public Vector2 NotInCombatDelay { get; set; } = new(2, 3);

	[UI("Next action size",
		Description = "Scales the Next action icons in the Control window, and the icon in the Intercepted Action window. At 100% the GCD icon is 40 pixels and the oGCD icon is 30 pixels.",
		Parent = nameof(ShowControlWindow), Order = 1)]
	[Range(0, 10, ConfigUnitType.Percent, 0.02f)]
	public float ControlWindowNextSizeRatio { get; set; } = 1.5f;

	[UI("Special button size",
		Description = "Scales the Control window's special buttons (Heal AoE, Forward, Dispel and the rest): their icons, labels and padding together. At 100% the GCD icon is 40 pixels and the oGCD icon is 30 pixels.",
		Parent = nameof(ShowControlWindow), Order = 1)]
	[Range(0.5f, 3f, ConfigUnitType.Percent, 0.01f)]
	public float ControlWindowSpecialsScale { get; set; } = 1f;

	[UI("GCD bar height", Parent = nameof(ShowNextActionWindow))]
	[Range(2, 30, ConfigUnitType.Pixels)]
	public float ControlProgressHeight { get; set; } = 8;

	[UI("Stop healing when enemies will die within",
		Description = "In combat, automatic healing stops when the enemies' average time to kill is below this.",
		Filter = HealingActionCondition, Section = 1)]
	[Range(0, 30, ConfigUnitType.Seconds, 0.02f)]
	public float AutoHealTimeToKill { get; set; } = 8f;

	[UI("Minimum update interval",
		Description = "The minimum time between RSR's updates. Raising it can help your framerate but can hurt rotation performance.",
		Filter = BasicTimer, Order = 1)]
	[JobConfig, Range(0, 0.3f, ConfigUnitType.Seconds, 0.002f)]
	public float MinUpdatingTime { get; set; } = 0.00f;

	/// <markdown file="Basic" name="Action ahead">
	/// Percent of your GCD time remaining on a GCD cycle before RSR will try to queue the next GCD.
	///
	/// This setting controls how many oGCDs RSR will try to fit in a single GCD window.
	/// Lower numbers mean more oGCDs, but potentially more GCD clipping.
	/// </markdown>
	[JobConfig, Range(0.05f, 0.4f, ConfigUnitType.Percent)]
	[UI("Action ahead", Filter = BasicTimer,
	Description = "How much of the GCD is left when RSR starts queuing the next GCD. This controls how many oGCDs RSR tries to fit in a GCD window: lower values fit more oGCDs, but can clip the GCD.")]
	private readonly float _action6head = 0.25f;

	// Henchman's "No Fall Damage" hack (A276). Off by default: it hooks a game function, and nobody should run
	// that without having chosen it.
	[ConditionBool, UI("No fall damage",
	Description = "The game's fall damage function returns zero, a hook taken from Henchman's hacks.\nIn a fight: a fall from a height costs no HP.\nIf this game version does not have that function, the setting switches itself off.",
	Filter = Extra)]
	private static readonly bool _noFallDamage = false;

	/// <summary>
	/// Remove extra lag-induced animation lock delay from instant casts (read tooltip!)
	/// Do NOT use with XivAlexander or NoClippy - this should automatically disable itself if they are detected, but double check first!
	/// </summary>
	[ConditionBool, UI("Remove lag-induced animation lock delay from instant casts (read tooltip!)",
	Description = "Do NOT use with XivAlexander, BMR tweaks enabled, or NoClippy - this should automatically disable itself if they are detected, but double check first!",
	Filter = Extra, Section = 1)]
	private static readonly bool _removeAnimationLockDelay = false;

	/// <summary>
	/// Animation lock max. simulated delay in milliseconds
	/// Configures the maximum simulated delay in milliseconds when using animation lock removal - this is required and cannot be reduced to zero.
	/// Setting this to 20ms will enable triple-weaving when using autorotation. The minimum setting to remove triple-weaving is 26ms.
	/// The minimum of 20ms has been accepted by FFLogs and should not cause issues with your logs.
	/// </summary>
	[UI("Max simulated animation lock delay (ms) (read tooltip!)",
	Description = "Configures the maximum simulated delay in milliseconds when using animation lock removal - this is required and cannot be reduced to zero. Setting this to 20ms will enable triple-weaving when using autorotation. The minimum setting to remove triple-weaving is 26ms. The minimum of 20ms has been accepted by FFLogs and should not cause issues with your logs.",
	Parent = nameof(RemoveAnimationLockDelay))]
	[Range(20, 50, ConfigUnitType.None, 1f)]
	public int AnimationLockDelayMax2 { get; set; } = 26;

	[UI("Delay smoothing factor", Parent = nameof(RemoveAnimationLockDelay))]
	[Range(0.3f, 0.95f, ConfigUnitType.None, 0.01f)]
	public float AnimLockDelaySmoothing { get; set; } = 0.3f;

	/// <summary>
	/// Remove extra framerate-induced cooldown delay
	/// Dynamically adjusts cooldown and animation locks to ensure queued actions resolve immediately regardless of framerate limitations
	/// </summary>
	[ConditionBool, UI("Remove framerate-induced cooldown delay",
	Description = "Dynamically adjusts cooldown and animation locks to ensure queued actions resolve immediately regardless of framerate limitations.",
	Filter = Extra, Section = 1)]
	private static readonly bool _removeCooldownDelay = false;

	[UI("Max cooldown adjustment (ms)", Parent = nameof(RemoveCooldownDelay))]
	[Range(10, 150, ConfigUnitType.None, 1f)]
	public int CooldownAdjustMaxMs { get; set; } = 100;

	[UI("Highlight color", Parent = nameof(TeachingMode))]
	public Vector4 TeachingModeColor { get; set; } = new(0f, 1f, 0f, 1f);

	[UI("Background when locked", Parent = nameof(ShowControlWindow), Order = 1)]
	public Vector4 ControlWindowLockBg { get; set; } = new(0, 0, 0, 0.55f);

	[UI("Background when unlocked", Parent = nameof(ShowControlWindow), Order = 1)]
	public Vector4 ControlWindowUnlockBg { get; set; } = new(0, 0, 0, 0.75f);

	[UI("Info window background",
		Description = "Applies to the Next Action, Intercepted Action and Action Timeline windows.",
		Filter = UiWindows, Section = 1, Order = 1)]
	public Vector4 InfoWindowBg { get; set; } = new(0, 0, 0, 0.4f);
	#endregion

	#region Target
	[JobConfig, UI("Engage settings", Description = "Which enemies RSR attacks.", Filter = TargetConfig, PvPFilter = JobFilterType.NoJob)]
	private readonly TargetHostileType _hostileType = TargetHostileType.AllTargetsWhenSoloInDuty;

	[ConditionBool, UI("Target the nearest enemy when you have no target",
		Description = "In combat, when nothing is targeted and there's no valid action target nearby. Works in Manual mode too.",
		Filter = TargetConfig, Order = 1)]
	private static readonly bool _targetFreely = false;

	[ConditionBool, UI("Delay auto-targeting",
		Filter = TargetConfig, Order = 1)]
	private static readonly bool _targetDelayEnable = false;

	[UI("Auto-target delay range", Description = "How long RSR waits before locking onto a newly aggroed or new target. Don't set it too low, or you can pull fresh dungeon mobs off the tank.",
		Parent = nameof(TargetDelayEnable))]
	[Range(0, 3, ConfigUnitType.Seconds)]
	public Vector2 TargetDelay { get; set; } = new(0f, 0f);

	[ConditionBool, UI("Hard target for all actions", Description = "When off, RSR only uses the game's soft targeting on allies for heals, shields and the like.",
		Filter = TargetConfig, Order = 1)]
	private static readonly bool _switchTargetFriendly2 = false;

	[ConditionBool, UI("Prioritize targets with attack markers",
	Filter = TargetConfig, Section = 1)]
	private static readonly bool _chooseAttackMark = true;

	// The owner's rule (16.10.2026): "bei der dot-verteilung sollte immer geschaut werden, welcher gegner die
	// aktuell höchsten hp hat, so dass der maximale effekt beim schaden erzielt werden kann. danach immer der
	// reihe nach wie die dots auslaufen." Concept 03, "Wahl des DoT-Ziels" (A295). On by default - the owner's
	// rule for defaults (29.09.2026).
	[ConditionBool, UI("Spread damage over time by HP, then by expiry",
	Description = "When a damage-over-time action can go on several enemies, it goes on one without it first, the one with the most HP left, so the damage over time ticks the longest. Once each carries it, it goes on the one whose damage over time runs out first.\nOff: the targeting type decides, as for every other attack.",
	Filter = TargetConfig)]
	private static readonly bool _dotTargetByHpThenExpiry = true;

	[ConditionBool, UI("Never attack targets with stop markers",
	Filter = TargetConfig, Section = 1)]
	private static readonly bool _filterStopMark2 = false;

	[ConditionBool, UI("Prioritize enemy parts",
	Description = "Such as Titan's Heart.",
	Filter = TargetConfig, Section = 1)]
	private static readonly bool _prioEnemyParts = true;

	[ConditionBool, UI("Prioritize FATE targets",
		Filter = TargetConfig, Section = 1)]
	private static readonly bool _targetFatePriority = true;

	[ConditionBool, UI("Only attack FATE enemies in FATEs",
	Description = "In a FATE, only its enemies are targeted. Outside a FATE, FATE enemies are ignored.",
	Filter = TargetConfig, Section = 1)]
	private static readonly bool _ignoreNonFateInFate = true;

	[ConditionBool, UI("Focus Forlorns in FATEs",
		Description = "Ignores every other FATE target while a Forlorn is up.",
		Filter = TargetConfig, Section = 1)]
	private static readonly bool forlornPriority = true;

	[ConditionBool, UI("Prioritize hunt, relic and leve targets",
		Description = "Relic targeting is known to be buggy.",
		Filter = TargetConfig, Section = 1)]
	private static readonly bool _targetHuntingRelicLevePriority = true;

	[ConditionBool, UI("Prioritize quest targets",
		Description = "Overrides the engage settings.",
		Filter = TargetConfig, Section = 1)]
	private static readonly bool _targetQuestPriority = true;

	[ConditionBool, UI("Don't target other players' quest enemies",
		Filter = TargetConfig, Section = 1)]
	private static readonly bool targetQuestThings3 = true;

	[ConditionBool, UI("Prefer lower-HP targets with the Small targeting type",
		Description = "When several Small targets are present, pick the one with the lowest HP instead of the highest.",
		Filter = TargetConfig, Section = 1)]
	private static readonly bool _smallHP = false;

	[ConditionBool, UI("Prefer lower-HP targets with the Big targeting type",
		Description = "When several Big targets are present, pick the one with the lowest HP instead of the highest.",
		Filter = TargetConfig, Section = 1)]
	private static readonly bool _bigHP = false;

	[ConditionBool, UI("Only attack targets on screen",
		Filter = TargetConfig, Section = 2)]
	private static readonly bool _onlyAttackInView = false;

	[ConditionBool, UI("Only attack targets in your vision cone",
		Filter = TargetConfig, Section = 2)]
	private static readonly bool _onlyAttackInVisionCone = false;

	[ConditionBool, UI("Ignore target dummies",
		Filter = TargetConfig, Section = 2)]
	private static readonly bool _disableTargetDummys = false;

	[ConditionBool, UI("Treat 1 HP targets as invincible",
	Description = "For the rare targets that are invincible without a status saying so.",
	Filter = TargetConfig, Section = 2)]
	private static readonly bool _filterOneHPInvincible = true;

	[UI("Boss time to kill",
		Description = "Targets with a longer time to kill than this count as bosses.",
		Filter = TargetConfig, Section = 3)]
	[Range(10, 1800, ConfigUnitType.Seconds, 0.02f)]
	public float BossTimeToKill { get; set; } = 90;

	[UI("Dying time to kill",
		Description = "Targets with a shorter time to kill than this count as dying.",
		Filter = TargetConfig, Section = 3)]
	[Range(0, 60, ConfigUnitType.Seconds, 0.02f)]
	public float DyingTimeToKill { get; set; } = 10;

	[UI("Dying HP",
		Description = "Targets below this HP count as dying.",
		Filter = TargetConfig, Section = 3)]
	[Range(0, 0.1f, ConfigUnitType.Percent, 0.01f)]
	public float IsDyingConfig { get; set; } = 0.02f;

	[ConditionBool, UI("Treat target dummies as bosses",
		Filter = TargetConfig, Section = 3)]
	private static readonly bool _dummyBoss = true;

	[UI("Movement target angle",
		Description = "How wide the cone is that movement actions pick their target from: in front of your character, or upward from your character on screen when aiming at the screen center.",
		Filter = TargetConfig, Section = 4)]
	[Range(0, 90, ConfigUnitType.Degree, 0.02f)]
	public float MoveTargetAngle { get; set; } = 24;

	[UI("Only use damaging gap closers within this distance",
		Description = "Gap closers that deal damage are only used on targets closer than this.",
		Filter = TargetConfig, Section = 4)]
	[Range(0, 30, ConfigUnitType.Yalms, 1f)]
	public float DistanceForMoving2 { get; set; } = 3f;

	[ConditionBool, UI("Aim movement actions at the screen center",
	Description = "When off, movement actions aim at what your character is facing.",
	Filter = TargetConfig, Section = 4)]
	private static readonly bool _moveTowardsScreenCenter = false;

	[ConditionBool, UI("Move as far as possible with ground-targeted movement actions",
		Filter = TargetConfig, Section = 4)]
	private static readonly bool _moveAreaActionFarthest = false;

	[UI("/rotation Cycle behavior", Filter = BasicCommands, Order = 1)]
	public CycleType CycleType { get; set; } = CycleType.CycleNormal;
	#endregion

	#region Integer

	public int ActionSequencerIndex { get; set; }

	[UI("Modifier key that unlocks movement", Description = "RB is for gamepad players.", Parent = nameof(PoslockCasting))]
	public ConsoleModifiers PoslockModifier { get; set; }

	[Range(0, 5, ConfigUnitType.None, 1)]
	[UI("Presses per action", Description = "RSR plays a random number of presses in this range.", Parent = nameof(KeyboardNoise))]
	public Vector2Int KeyboardNoisePresses { get; set; } = new(2, 3);

	[Range(0, 10, ConfigUnitType.None)]
	public int TargetingIndex { get; set; }

	#endregion

	#region Jobs

	[JobConfig, UI("Use Lucid Dreaming below this MP", Filter = AutoActionUsage)]
	[Range(0, 10000, ConfigUnitType.None)]
	private readonly int _lucidDreamingMpThreshold = 6000;

	/// <markdown file="Auto" name="HP threshold for AoE healing oGCDs (Heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **oGCD** AoE healing abilities **when they have a heal-over-time already applied to them, by you or a teammate**.
	/// 
	/// This is calculated with the <see cref="RotationSolver.Basic.Configuration.Configs.HealthDifference">standard AoE healing deviation</see>.
	///
	/// Use the button this setting to calculate your preferred values.
	/// </markdown>
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaAbilityHot = 0.55f;

	/// <markdown file="Auto" name="HP threshold for AoE healing GCDs (Heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **GCD** AoE healing abilities **when they have a heal-over-time already applied to them, by you or a teammate**.
	/// 
	/// This is calculated with the <see cref="RotationSolver.Basic.Configuration.Configs.HealthDifference">standard AoE healing deviation</see>.
	///
	/// Use the button this setting to calculate your preferred values.
	/// </markdown>
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaSpellHot = 0.55f;

	/// <markdown file="Auto" name="HP threshold for AoE healing GCDs (No heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **oGCD** AoE healing abilities.
	/// 
	/// This is calculated with the <see cref="RotationSolver.Basic.Configuration.Configs.HealthDifference">standard AoE healing deviation</see>.
	///
	/// Use the button this setting to calculate your preferred values.
	/// </markdown> 
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaAbility = 0.75f;

	/// <markdown file="Auto" name="HP threshold for AoE healing GCDs (No heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **GCD** AoE healing abilities.
	/// 
	/// This is calculated with the <see cref="RotationSolver.Basic.Configuration.Configs.HealthDifference">standard AoE healing deviation</see>.
	///
	/// Use the button this setting to calculate your preferred values.
	/// </markdown> 
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthAreaSpell = 0.65f;

	/// <markdown file="Auto" name="HP threshold for single-target healing oGCDs (Heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **oGCD** single-target healing abilities **when they have a heal-over-time already applied to them, by you or a teammate**.
	/// </markdown>
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleAbilityHot = 0.65f;

	/// <markdown file="Auto" name="HP threshold for single-target healing GCDs (Heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **GCD** single-target healing abilities **when they have a heal-over-time already applied to them, by you or a teammate**.
	/// </markdown>
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleSpellHot = 0.55f;

	/// <markdown file="Auto" name="HP threshold for single-target healing oGCDs (No Heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **oGCD** AoE healing abilities.
	/// </markdown>
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleAbility = 0.7f;

	/// <markdown file="Auto" name="HP threshold for single-target healing GCDs (No Heal-over-time)" section="Healing Usage and Control" subsection="RotationSolver.Basic.Configuration.Configs._autoHeal">
	/// Relates to **GCD** single-target healing abilities.
	/// </markdown>
	[JobConfig, Range(0, 1, ConfigUnitType.Percent)]
	private readonly float _healthSingleSpell = 0.65f;

	/// <markdown file="Auto" name="Use invulnerability below this HP" section="Action Usage and Control">
	/// The threshold to automatically use your tank invulnerability action when falling below the HP percentage set.
	/// </markdown>
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
		File.WriteAllText(Svc.PluginInterface.ConfigFile.FullName,
			JsonConvert.SerializeObject(this, Formatting.Indented));
	}

	public static Configs Migrate(Configs oldConfigs)
	{
		// Implement migration logic if needed
		if (oldConfigs.Version != CurrentVersion)
		{
			// Reset to default if versions do not match
			return new Configs();
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

		if (restoredConfigs.Version != CurrentVersion)
		{
			Svc.Toasts.ShowNormal("Backed up configs are not compatible with the current version.");
			return;
		}

		Service.Config = restoredConfigs;
		Save();
		Svc.Toasts.ShowNormal("Configs restored. Closing to set.");
		DataCenter.HoldingRestore = true;
	}
}
