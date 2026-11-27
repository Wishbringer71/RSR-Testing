using Dalamud.Configuration;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using RebornMaterial;
using System.Collections.Concurrent;
using static RotationSolver.Configuration.ConfigTypes;

namespace RotationSolver.Configuration;

internal partial class Configs
{

	[ConditionBool, UI("Check intercepted movement actions with BossModReborn (Experimental)",
	Description = "Before RSR uses an intercepted movement action, or an action that moves you, ask BossModReborn whether the destination is safe.",
	Parent = nameof(InterceptAction3), Order = 1)]
	private static readonly bool _bmrSafetyCheckIntercept = false;

	[ConditionBool, UI("Intercept your action presses",
	Description = "In a fight: a job action you press goes at the next fitting moment of the rotation, in PvE and "
		+ "PvP.\nIn PvP, Guard, Purify, Recuperate, the Elixir and Sprint go at once.\nOff: pressed actions go as "
		+ "the game takes them.",
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

	[ConditionBool, UI("Don't pull new enemies with AoE", Description = "Never use an AoE action that could hit enemies that aren't already hostile targets. In deep dungeons their own options decide instead (Deep dungeon section).",
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

	[ConditionBool, UI("Turn back on when you are raised in combat",
		Description = "In a fight: when \"Turn off when you die\" turned RSR off, it comes back on as it was once you "
			+ "are raised.\nNot after you switched it yourself, changed zones or the fight ended.\nOff: RSR stays "
			+ "off until you turn it on.",
		Parent = nameof(AutoOffWhenDead))]
	private static readonly bool _autoOnWhenRaised = true;

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

	[ConditionBool, UI("Draw enemy areas without a ground marker through Splatoon",
	Description = "In a fight: circles and lines an enemy casts without the game's ground marker are drawn where they land. Needs Splatoon.\nLeft to BossMod's world projection or Splatoon's projection where they draw; their blacklist is respected.\nWhile Splatoon's projection is on, a ring it draws against the game's marker goes into its blacklist.",
	Filter = UiInformation)]
	private static readonly bool _drawAreasWithoutMarker = true;

	[ConditionBool, UI("Lock movement while casting",
	Description = "Stops your character from moving while casting, until the spell can no longer be interrupted, and while using the actions ticked below. It never holds you in danger: BossMod's AI and you can still move out. A ticked party mitigation holds against BossMod's AI only while it covers a party member in danger before an announced hit and no other party mitigation is ready.",
	Filter = Extra)]
	private static readonly bool _poslockCasting = false;

	// The channels that end upon moving (concept 03, "Bewegungssperre beim Wirken und die KI von BossMod", A651, A652): on, as
	// who lets a channel run means to stand there and the lock yields to danger; Meditate holds only where the pause is
	// known to outlast the walk.
	[UI("", Action = ActionID.PassageOfArmsPvE,
		Description = "Holds you still while Passage of Arms runs, so a step does not end it - another action still does. It lets go when danger reaches your spot before the channel ends - BossMod forbids the spot, or RSR sees a hit only a "
		+ "step avoids. BossMod's AI walking only for position does not end it. While BossMod's AI walks you out of danger, it still holds as long as the channel "
		+ "covers a party member in danger of dying before an announced party hit, you are not in danger of dying yourself, and neither Divine Veil nor Reprisal is ready - with one of them you leave, and RSR uses it for the hit.\nIn a fight: the party behind you keeps the damage reduction up to the announced hit instead of losing it at BossMod's first positioning step.",
		Parent = nameof(PoslockCasting))]
	public bool PosPassageOfArms { get; set; } = true;

	[UI("", Action = ActionID.FlamethrowerPvE,
		Description = "Holds you still while Flamethrower runs, so a step does not end it - another action still does. It lets go when danger reaches your spot before the channel ends - BossMod forbids the spot, or RSR sees a hit only a "
		+ "step avoids. BossMod's AI walking only for position does not end it.\nIn a fight: the flames keep burning the enemies before you instead of breaking at BossMod's first positioning step.",
		Parent = nameof(PoslockCasting))]
	public bool PosFlameThrower { get; set; } = true;

	[UI("", Action = ActionID.ImprovisationPvE,
		Description = "Holds you still while Improvisation runs, so a step does not end it - another action still does. It lets go when danger reaches your spot before the channel ends - BossMod forbids the spot, or RSR sees a hit only a "
		+ "step avoids. BossMod's AI walking only for position does not end it.\nIn a fight: the dance keeps healing you and the party nearby and gathers Rising Rhythm until the next action ends it, instead of breaking at BossMod's first positioning step.",
		Parent = nameof(PoslockCasting))]
	public bool PosImprovisation { get; set; } = true;

	[UI("", Action = ActionID.CollectiveUnconsciousPvE,
		Description = "In a fight: you stand still while Collective Unconscious runs, so its ring keeps renewing the "
			+ "regen on the party in it.\nDanger on your spot lets you go.",
		Parent = nameof(PoslockCasting))]
	public bool PosCollectiveUnconscious { get; set; } = true;

	[UI("", Action = ActionID.MeditatePvE,
		Description = "In a fight: you stand still for Meditate against BossMod's AI walking while the pause "
			+ "outlasts the walk, so the Kenki Gauge keeps filling.\nYour own movement always ends it; danger "
			+ "lets you go.",
		Parent = nameof(PoslockCasting))]
	public bool PosMeditate { get; set; } = true;

	[UI("", Action = ActionID.PhantomFlurryPvE,
		Description = "In a fight: you stand still while Phantom Flurry runs, so it keeps hitting up to its "
			+ "finishing blow.\nDanger on your spot lets you go; another action still ends it.",
		Parent = nameof(PoslockCasting))]
	public bool PosPhantomFlurry { get; set; } = true;

	[UI("", Action = ActionID.ApokalypsisPvE,
		Description = "In a fight: you stand still while Apokalypsis runs, so a positioning step does not end its "
			+ "beam early.\nDanger on your spot lets you go; another action still ends it.",
		Parent = nameof(PoslockCasting))]
	public bool PosApokalypsis { get; set; } = true;

	[UI("", Action = ActionID.ChelonianGatePvE,
		Description = "In a fight: you stand still while Chelonian Gate runs, so it keeps lowering your damage taken "
			+ "and can turn into Auspicious Trance.\nDanger on your spot lets you go.",
		Parent = nameof(PoslockCasting))]
	public bool PosChelonianGate { get; set; } = true;

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

	[JobConfig, UI("Tincture usage",
	Description = "When to use gemdraughts and tinctures. Only rotations that support automatic tincture usage follow this.\n"
		+ "In a fight: the tincture goes in each burst window where it is ready and in your inventory. Tinctures, HP and MP "
		+ "potions and Phoenix Down share one recast: a tincture blocks HP and MP potions for its 5 minutes.",
	Filter = AutoItemUsage, Section = 0, PvPFilter = JobFilterType.NoJob)]
	private readonly TinctureUseType _TinctureType = TinctureUseType.InHighEndDuty;

	// Damage against healing and mana is each player's own choice; off is the owner's rule, safety first (A502).
	[JobConfig, UI("Hold HP and MP potions for the tincture",
	Description = "In a fight: where your job drinks tinctures and holds one, HP and MP potions wait, so their "
		+ "recast does not keep the tincture from its burst.\nAn HP potion against a tankbuster on you still "
		+ "goes.\nOff: potions go at their threshold.",
	Filter = AutoItemUsage, Section = 0, PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _holdPotionsForTincture = false;

	[ConditionBool, UI("Use anti-knockback actions",
	Description = "Use Arm's Length and Surecast when an enemy casts an action on the Knockback list.",
	Filter = AutoActionUsage, Section = 2, Order = 1)]
	private static readonly bool _useKnockback = true;

	[ConditionBool, UI("Use HP potions",
	Description = "In a fight: HP potions, tinctures, MP potions and Phoenix Down share one recast. An HP potion blocks "
		+ "the next tincture for its recast; a tincture drunk in a burst blocks HP potions for 5 minutes.\n"
		+ "Enable individual potions in Actions > Item.",
	Filter = AutoItemUsage, Section = 1, PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _useHpPotions = false;

	[UI("Use HP potions below this HP", Parent = nameof(UseHpPotions), Order = 0)]
	[Range(0, 1, ConfigUnitType.Percent, 0.002f)]
	public float UseHpPotionsPercent { get; set; } = 0.5f;

	[ConditionBool, UI("Use HP potions out of combat", Parent = nameof(UseHpPotions), Order = 1)]
	private static readonly bool _useHpPotionsOutOfCombat = false;

	[UI("Stop using HP potions when enemies will die within",
	Description = "In combat, stop using HP potions when the enemies' average time to kill is below this. Set to 0 to disable.",
	Parent = nameof(UseHpPotions), Order = 2)]
	[Range(0, 30, ConfigUnitType.Seconds, 0.02f)]
	public float UseHpPotionsTimeToKill { get; set; } = 0f;

	[ConditionBool, UI("Only when no healer is present or alive", Parent = nameof(UseHpPotions), Order = 3)]
	private static readonly bool _useHpPotionsOnlyWhenNoHealer = false;

	[ConditionBool, UI("Use MP potions",
	Description = "In a fight: MP potions, tinctures, HP potions and Phoenix Down share one recast. An MP potion blocks "
		+ "the next tincture for its recast; a tincture drunk in a burst blocks MP potions for 5 minutes.\n"
		+ "Enable individual potions in Actions > Item.",
	Filter = AutoItemUsage, Section = 2, PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _useMpPotions = false;

	[UI("Use MP potions below this MP", Parent = nameof(UseMpPotions), Order = 0)]
	[Range(0, 1, ConfigUnitType.Percent, 0.002f)]
	public float UseMpPotionsPercent { get; set; } = 0.5f;

	[ConditionBool, UI("Use MP potions out of combat", Parent = nameof(UseMpPotions), Order = 1)]
	private static readonly bool _useMpPotionsOutOfCombat = false;

	[ConditionBool, UI("Use Phoenix Down (Experimental)",
	Filter = AutoItemUsage, Section = 3, PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _usePhoenixDown = false;

	[ConditionBool, UI("Only when no one who can raise is alive",
	Description = "A raiser is a living healer, Summoner or Red Mage of a high enough level to have their raise. Which of them count follows the raise target setting: the party alone under Party only and Party healers only, party and alliance under the alliance modes and All.\n"
		+ "On: the item is kept for the case it exists for - nobody left who can cast a raise. A wipe recovery where the healer is up costs no Phoenix Down.\n"
		+ "Off: a Phoenix Down can go out while a healer is standing next to the corpse, spending a consumable on a raise that was free.",
	Parent = nameof(UsePhoenixDown))]
	private static readonly bool _usePhoenixDownHealerLogic = true;

	[ConditionBool, UI("Use the deep dungeon's pomanders against bosses and in danger when alone",
	Description = "In a fight: alone in a deep dungeon, Steel, Strength and Haste go against a boss, and Raising, "
		+ "then Witching, when you are about to die.\nNever in a party, where the inventory is shared.",
	Filter = AutoItemUsage, Section = 4)]
	private static readonly bool _useDeepDungeonPomanders = true;

	[ConditionBool, UI("Use the deep dungeon's magicite, demiclones and incense against bosses and in danger when alone",
	Description = "In a fight: alone in a deep dungeon, a magicite or incense that clears the floor saves you from "
		+ "dying; Elder Magicite, demiclones and Barkbalm go against bosses.\nNever in a party.",
	Filter = AutoItemUsage, Section = 4)]
	private static readonly bool _useDeepDungeonStones = true;

	[ConditionBool, UI("Use the Limit Break when no other player is in the party",
	Description = "In a fight: with only NPC companions, you use your full Limit Break: a damage dealer when safe, a "
		+ "tank against big hits, a healer when several die.\nWith another player the bar stays theirs too.",
	Filter = AutoActionUsage)]
	private static readonly bool _useLimitBreakWhenAlone = true;

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

	[ConditionBool, UI("Use healing abilities as a non-healer",
		Description = "Lets tanks and DPS heal with abilities such as Vercure and Bloodbath.",
		Filter = HealingActionCondition, Section = 4)]
	private static readonly bool _useHealWhenNotAHealer = true;

	[ConditionBool, UI("Heal and protect yourself while the raise protection lasts",
		Description = "In a fight: in the 5 s of protection after a raise, RSR heals and protects you when the next "
			+ "hit lands only after them.\nNo damage, no buffs, no raise in that time.\nOff: you act as usual "
			+ "right away.",
		Filter = HealingActionCondition, Section = 4)]
	private static readonly bool _spendRaiseProtection = true;

	[JobConfig, UI("Interrupt enemy casts",
		Filter = AutoActionUsage, Section = 3,
		PvEFilter = JobFilterType.Interrupt,
		PvPFilter = JobFilterType.NoJob)]
	private static readonly bool _interruptibleMoreCheck = true;

	[ConditionBool, UI("Stop an enemy's cast with the job's stun or a sleep (experimental)",
		Description = "In a fight: as a healer or caster, RSR stops a non-boss enemy's area or interruptible cast "
			+ "with a stun or sleep when it lands in time.\nResisting kinds and attacks are learned and not tried "
			+ "again.\nOff: such casts go off.",
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
		Description = "In a fight: a small window shows why RSR does what it does - the rotation's status lines, the "
			+ "area damage table, the HP potion.\nNothing RSR does changes.",
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

	[UI("Accent color",
		Description = "The whole color scheme is built from this one color. Pick a preset, or the last swatch for your own color.",
		Filter = UiCustomize)]
	public Vector4 UiAccentColor { get; set; } = new(0.690f, 0.125f, 0.122f, 1f);

	[UI("Text size",
		Description = "Scales the text, on top of Dalamud's own font size.",
		Filter = UiCustomize)]
	[Range(0.75f, 2f, ConfigUnitType.Multiplier, Step = 0.05f)]
	public float UiTextScale { get; set; } = 1f;

	[UI("Control size",
		Description = "Scales buttons, switches, fields and the other controls.",
		Filter = UiCustomize)]
	[Range(0.75f, 2f, ConfigUnitType.Multiplier, Step = 0.05f)]
	public float UiElementScale { get; set; } = 1f;

	[UI("Spacing",
		Description = "Scales the space around and between things, without resizing them.",
		Filter = UiCustomize)]
	[Range(0.5f, 2f, ConfigUnitType.Multiplier, Step = 0.05f)]
	public float UiPaddingScale { get; set; } = 1f;

	[UI("Animation length",
		Description = "How long animations take, such as folding a window or opening a card. 0% turns them off.",
		Filter = UiCustomize)]
	[Range(0f, 2f, ConfigUnitType.Percent, Step = 0.05f)]
	public float UiMotionScale { get; set; } = 1f;

	[UI("Compact layout",
		Description = "Tighter padding and shorter rows, so more fits on screen.",
		Filter = UiCustomize, Order = 1)]
	public bool UiCompactLayout { get; set; } = false;

	[ConditionBool, UI("Inline descriptions",
		Description = "Show each setting's description under its name. Turn it off to show descriptions on hover instead.",
		Filter = UiCustomize, Order = 2)]
	private static readonly bool _uiInlineDescriptions = true;

	[UI("Icons-only sidebar",
		Description = "Turn the sidebar into a narrow strip of icons. Hover an icon to see its name.",
		Filter = UiCustomize, Order = 3)]
	public bool UiIconSidebar { get; set; } = false;

	[ConditionBool, UI("Show a toast when a command queues an action",
		Description = "Confirms each action that a /rotation DoAction command adds to the queue.",
		Filter = BasicCommands, Section = 2)]
	private static readonly bool _showToastsAboutDoAction = false;

	[ConditionBool, UI("Turn away from gaze attacks",
		Description = "In a fight: RSR turns you away from a gaze that reaches you just before it lands, and keeps "
			+ "actions that would face it waiting.\nAfter a gaze still hits you twice, RSR leaves that gaze to "
			+ "you.\nOff: such actions wait for the whole cast; you turn yourself.",
		Filter = AutoActionUsage, Section = 3)]
	private static readonly bool _turnAwayFromGazes = true;

	[ConditionBool, UI("Stop acting while an enemy casts a Gaze/Stop action (Experimental)",
		Description = "Uses the actions on the Gaze/Stop list.",
		Filter = AutoActionUsage, Section = 3)]
	private static readonly bool _castingStop = false;

	[UI("Stop this long before the cast finishes", Parent = nameof(CastingStop))]
	[Range(0, 15, ConfigUnitType.Seconds)]
	public float CastingStopTime { get; set; } = 2.5f;

	[ConditionBool, UI("Stop for the whole cast", Parent = nameof(CastingStop))]
	private static readonly bool _castingStopCalculate = false;

	[ConditionBool, UI("Automatic healing thresholds", Filter = HealingActionCondition)]
	private static readonly bool _autoHeal = true;

	[ConditionBool, UI("Stop single-target GCD heals once the target is above the threshold (Experimental)", Filter = HealingActionCondition, Description = "If another healer's healing puts the target above the healing threshold while you're casting, the cast is interrupted so you don't waste MP.")]
	private static readonly bool _stopHealingAfterThresholdExperimental2 = false;

	[ConditionBool, UI("Use oGCD abilities", Filter = AutoActionUsage)]
	private static readonly bool _useAbility = true;

	[ConditionBool, UI("Use duty actions", Filter = AutoActionUsage,
		Description = "Allow the duty rotation to use duty actions, such as Bozja Lost Actions, Occult Crescent Phantom actions and Variant actions. Manually used or intercepted duty actions are unaffected.")]
	private static readonly bool _useDutyActions = true;

	[ConditionBool, UI("Burst automatically",
		Description = "RSR keeps the Burst state on, so rotations use their burst abilities as soon as they're ready. Turn this off to time burst yourself with the Burst special state.",
		Filter = AutoActionUsage)]
	private static readonly bool _autoBurst = true;

	// The owner's note, 25.10.2026: a melee belongs in reach of its target. The Balance's Viper opener dashes at -0.5 s;
	// upstream's Monk did so whatever the distance. Off for a pull that has him wait away from the boss (A347).
	[ConditionBool, UI("Dash to the target at the end of the countdown when out of melee range",
		Description = "In a fight: Monk, Viper and Dragoon dash to a target beyond melee range as the countdown "
			+ "ends, so the first weaponskill lands at the pull.\nOff: you walk in yourself.",
		Filter = AutoActionUsage, PvEFilter = JobFilterType.Melee)]
	private static readonly bool _pullGapCloser = true;

	// The owner's note, 25.10.2026: damage areas only once the movement after a wall-to-wall pull stops - and, his
	// hints, enemies move inside an area without leaving it, swap at its edge in a big pull and die, and a tank stops to
	// pick up the next group and waits for his area attack before he moves on. A GCD in the area as the sign of a stop
	// is a derivation, not shown in play yet, hence the switch (concept 04, A350-A353).
	[ConditionBool, UI("Lay damage areas only where the enemies stay",
		Description = "In a fight: Salted Earth, Doton and Slipstream go down once the pack has stood in their "
			+ "radius for a GCD.\nNot onto a group the tank is still pulling.\nOff: they go out as soon as you "
			+ "stand.",
		Filter = AutoActionUsage)]
	private static readonly bool _damageAreasWhereEnemiesStay = true;

	// Concept 05, "Arenagrenzen ohne BossMod-Modul" (A934): on by default - safety before damage; the saved name stays.
	[ConditionBool, UI("Check where movement actions land",
		Description = "In a fight: before RSR moves you with an action, it asks BossModReborn's danger zones and "
			+ "vnavmesh's floor.\nA jump into a zone or over an abyss stays out.\nOff: it jumps unchecked.",
		Filter = AutoActionUsage)]
	private static readonly bool _bmrSafetyCheckAuto = true;

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
		Description = "Counted once an enemy has hit you in this fight: a training dummy holds you as its target and never hits.",
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

	// The enemy count says a pull has come, not whether it needs the big cooldowns; the measured fall says that,
	// net of every heal and mitigation (concept 09, "Die Abwehr im Pull"). In the closed-loop model
	// (tank_pull_defense_model.py) holding them until the fall would cross 30 % within their duration, without the old
	// 60 s / 30 s stagger, had the fewest deaths and the least time under 30 % in every case, the opening of a big pull
	// no worse than before (A359). On by default: the owner's rule for new rules (29.09.2026).
	[ConditionBool, UI("Hold the big mitigation and Rampart in a pull until your health needs them",
		Description = "In a fight: in a pull, the big mitigation and Rampart wait until your falling HP needs them; "
			+ "short mitigation and Reprisal go at once.\nNot while you or a tank is in danger.\nOff: they go at "
			+ "the start of every big pull, 60 and 30 s apart.",
		Parent = nameof(UseDefenseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _holdBigTankMitigationInPull = true;

	[UI("Health the big mitigation waits for", Parent = nameof(HoldBigTankMitigationInPull),
		PvEFilter = JobFilterType.Tank)]
	[Range(0, 1, ConfigUnitType.Percent, 0.01f)]
	public float HoldBigTankMitigationBelow { get; set; } = 0.3f;

	// With no healer alive nothing restores what the player loses until the fight ends - a dead player returns to the
	// start only once nobody is in combat -, and the time he stays alive is the time left for a Phoenix Down or a raise
	// (the owner's question and note of 25.10.2026). In the model (tank_pull_defense_model.py) the tank lived through 9 of
	// 400 big pulls he died in before and none the other way, 28 % longer against a boss with tankbusters and 37 % longer
	// against one without (A361). Every role, as the rule holds for whoever is attacked. On by default: the owner's rule
	// for new rules (29.09.2026).
	[ConditionBool, UI("Without a living healer, spend your defence to stay alive longer",
		Description = "In a fight: with no living healer, your defence opens whenever an enemy attacks you, and each "
			+ "mitigation goes once it will be back before the next tankbuster.\nOff: your defence keeps waiting "
			+ "for tankbusters and pulls.",
		Parent = nameof(UseDefenseAbility))]
	private static readonly bool _spendDefenceWithoutHealer = true;

	[ConditionBool, UI("Use tank stance", Parent = nameof(UseAbility),
		PvEFilter = JobFilterType.Tank)]
	private static readonly bool _autoTankStance = true;

	[ConditionBool, UI("Provoke when there is another tank in the party", Description = "Provoke an enemy that is attacking a non-tank party member, while there is more than one tank in the party.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _autoProvokeForTank = true;

	[ConditionBool, UI("Take the enemy from a co-tank in danger",
		Description = "In a fight: you provoke an enemy off the co-tank when he is low, would take its announced "
			+ "tankbuster unsafe, or shirks to you.\nNot while you are in danger yourself.\nOff: he is left to his "
			+ "healers and cooldowns.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _takeEnemyFromCoTankInDanger = true;

	[ConditionBool, UI("Shirk the co-tank after a tankbuster that leaves you in danger",
		Description = "In a fight: when a tankbuster leaves you in danger without your invulnerability, Shirk hands "
			+ "the enemy to the co-tank, and you provoke it back once safe.\nOff: you keep the enemy unless your "
			+ "co-tank provokes it.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _shirkToSwapAfterTankbuster = true;

	[ConditionBool, UI("Use the invulnerability before a tankbuster nothing less survives",
		Description = "In a fight: before a measured tankbuster that none of your ready mitigations would let you "
			+ "survive, your invulnerability goes, timed so it lasts through the hit.\nAn unmeasured cast or a "
			+ "first-seen marker gets nothing.\nOff: the invulnerability waits for the HP threshold.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _invulnerabilityBeforeLethalTankbuster = true;

	[ConditionBool, UI("Use the invulnerability when damage already reported would kill you",
		Description = "In a fight: when damage the game has already reported on you would kill you, your "
			+ "invulnerability goes at once, whatever your HP threshold.\nHeals on their way are not counted, so "
			+ "it can be spent where a heal would have saved you.\nOff: only the HP threshold fires it.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _invulnerabilityAgainstReportedDeath = true;

	[ConditionBool, UI("Spread Reprisal and the party mitigation over a run of area attacks",
		Description = "In a fight: after Reprisal or your party mitigation, the other waits while the first still "
			+ "works, so two area attacks in a row each get one.\nA big hit or a member in danger gets both.\nOff: "
			+ "both can go on the first.",
		Parent = nameof(UseAbility), PvEFilter = JobFilterType.Tank)]
	private static readonly bool _spreadTankAreaDefense = true;

	[ConditionBool, UI("Spend only the mitigation a measured tankbuster needs",
		Description = "In a fight: against a measured tankbuster, RSR spends the cheapest set of your mitigations "
			+ "that lets you survive it and keeps the rest for the next.\nHealers keep single-target mitigation "
			+ "off a tank who takes the hit safely.\nOff: mitigation is not chosen by the size of the hit.",
		Parent = nameof(UseAbility))]
	private static readonly bool _holdMitigationUnderInvulnerability = true;

	[ConditionBool, UI("Use True North",
		Parent = nameof(UseAbility),
		PvEFilter = JobFilterType.Melee)]
	private static readonly bool _autoUseTrueNorth = true;

	[ConditionBool, UI("Move into positionals through BossMod",
		Description = "In a fight: when your next weaponskill needs rear or flank and True North does not cover it, "
			+ "BossMod's \"Automatic movement\" walks you inside that side.\nNeeds BossMod; not while its AI "
			+ "moves you or another BossMod preset is active.\nAdds the BossMod preset \"RSR Positionals\".",
		Parent = nameof(UseAbility),
		PvEFilter = JobFilterType.Melee)]
	private static readonly bool _moveIntoPositionals = true;

	[ConditionBool, UI("Use movement speed abilities out of combat in duties", Parent = nameof(UseAbility))]
	private static readonly bool _autoSpeedOutOfCombat = true;

	[ConditionBool, UI("Use movement speed abilities out of combat outside duties", Parent = nameof(UseAbility))]
	private static readonly bool _autoSpeedOutOfCombatNoDuty = false;

	[ConditionBool, UI("Sprint when a party tank sprints (non-tanks only)", Parent = nameof(UseAbility))]
	private static readonly bool _autoSprintWithTank = false;

	[ConditionBool, UI("Use beneficial ground-targeted actions", Description = "Places ground-targeted heals like Asylum, Sacred Soil and Earthly Star. RSR tries these spots in order:\r\n1. The party tank, when that option is on.\r\n2. The closest saved spot in this map's Beneficial AoE locations, when you are near it. Trials and full-party raids with no saved spot use the arena center.\r\n3. The boss.\r\n4. A point between you and the party member being targetted by the boss.\r\n5. Your own position.\r\nEvery spot is kept inside the action's range and your line of sight.", Filter = HealingActionCondition, Section = 5)]
	private static readonly bool _useGroundBeneficialAbility = true;

	[JobConfig, UI("Use them while moving", Parent = nameof(UseGroundBeneficialAbility))]
	private static readonly bool _useGroundBeneficialAbilityWhenMoving = false;

	[JobConfig, UI("Always place them on yourself", Description = "Skips RSR's usual placement logic.", Parent = nameof(UseGroundBeneficialAbility))]
	private static readonly bool _useGroundBeneficialAbilityOnlySelf = false;

	[JobConfig, UI("Place them on the party tank when there is one", Description = "Skips RSR's usual placement logic.", Parent = nameof(UseGroundBeneficialAbility))]
	private static readonly bool _useTargetTankForGroundHeal = false;

	// The usual placement takes the arena centre, the boss or a point by the attacked member and asks only that one
	// member stands in it; with the party stacked away from that point the area covered almost nobody (A398).
	[JobConfig, UI("Place them where they cover the most party members",
		Description = "In a fight: Sacred Soil, Asylum, Earthly Star and similar areas land where they cover the "
			+ "most party members, not on an empty usual spot.\nOff: the usual placement alone.",
		Parent = nameof(UseGroundBeneficialAbility))]
	private static readonly bool _groundBeneficialCoverMost = true;

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
		Description = "In a fight: a dark knight under Living Dead is left at 1 HP, so the killing blow turns it "
			+ "into Walking Dead.\nThen healing worth his maximum HP must land within ten seconds.\nOff: he is "
			+ "healed normally, as when RSR fires Living Dead to save a dying tank.",
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
		Description = "In a fight: heal thresholds read the HP a member will have when a heal started now lands, so "
			+ "a falling tank is healed a GCD earlier.\nWhile HP holds steady, nothing changes.\nOff: thresholds "
			+ "read the HP shown now.",
		Filter = HealingActionCondition, Section = 1)]
	public bool HealAheadOfDamage { get; set; } = true;

	// The owner's proposal of 27.10.2026, as the loop left it (concept 08, "Schaden über Zeit: HoT gegen DoT", A432). On
	// by default: his rule that a default is the value that plays better, so a new rule is tested (29.09.2026); its gain
	// rests on the model, not on a fight measured with it.
	[UI("Heal over time against an enemy's damage over time",
		Description = "In a fight: as White Mage or Astrologian, a heal over time goes on whoever an enemy's damage "
			+ "over time would take below the heal threshold.\nThe strength of each damage over time is learned "
			+ "per area.\nOff: heals over time follow the thresholds alone.",
		Filter = HealingActionCondition, Section = 1)]
	public bool HotAgainstDot { get; set; } = true;

	// The owner's triage (concept 07): whoever is closest to dying first, and at equal danger healer
	// before tank before damage dealer. Class 1 - about to fall - is built and always on. This adds
	// classes 2 and 3 in place of the role short-cuts, which pick a role outright once it is under its
	// ratio and look at nobody else. On by default - the owner's rule for defaults (29.09.2026), and
	// his own order of who is healed first; that it keeps more people alive is not shown in play.
	[UI("Choose the heal target by danger",
		Description = "In a fight: a healer or tank under their threshold and being attacked is healed first, the "
			+ "lowest HP first.\nDuring an announced area cast, whoever has the fewest HP left comes next.\nOff: "
			+ "the role thresholds decide alone.",
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
		Description = "In a fight: a measured area cast that leaves everybody above the healing line raises no party "
			+ "mitigation, so Reprisal and Addle wait for a real hit.\nAn unmeasured cast is mitigated as "
			+ "before.\nOff: every listed cast is mitigated.",
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
		Description = "In a fight: when an announced area cast would leave a member about to fall, area healing goes "
			+ "out before it lands.\nIt uses the cast's measured size and counts barriers; an unmeasured cast "
			+ "changes nothing.\nOff: the heal comes after the hit.",
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
		Description = "In a fight: an area cast that missed you last time does not open your area defence; once it "
			+ "reaches you, it counts again.\nOff: every listed cast that can reach you opens it.",
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
		Description = "In a fight: an interruptible area cast also gets area mitigation when its measured damage is "
			+ "at least the largest barrier and it lands within a GCD.\nOff: interruptible casts get none, as "
			+ "someone is expected to interrupt them.",
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
		Description = "In a fight: a mitigation from a BossMod prediction waits while a small measured area cast "
			+ "runs, so it covers the heavier one behind it.\nOff: the prediction is followed at once.",
		Filter = HealingActionCondition, Section = 1)]
	public bool HoldProactiveMitigationForSmallCast { get; set; } = true;

	[UI("Dispel delay range",
		Description = "RSR waits a random time in this range before dispelling.\n"
			+ "In a fight: never past the point where it could come too late - a dangerous status that runs out within a "
			+ "GCD is cleansed at once.",
		Filter = AutoActionUsage, Section = 4, Order = 1,
		PvEFilter = JobFilterType.Dispel, PvPFilter = JobFilterType.NoJob)]
	[Range(0, 10, ConfigUnitType.Seconds, 0.002f)]
	public Vector2 EsunaDelay { get; set; } = new(0f, 0f);

	#region Raise
	[JobConfig, UI("Raise before healing or mitigating",
		Filter = RaiseActionCondition)]
	private static readonly bool _raisePlayerFirst = false;

	[JobConfig, UI("Raise with Swiftcast or Dualcast", Description = "If this is off, RSR never presses Swiftcast for a raise, "
		+ "and a Swiftcast or Dualcast already running goes to your next spells. Only when it would otherwise run out unused "
		+ "does it go into a pending raise.",
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

	[UI("Raise delay range",
		Description = "RSR waits a random time in this range before raising.",
		Filter = RaiseActionCondition, Section = 1)]
	[Range(0, 10, ConfigUnitType.Seconds, 0.002f)]
	// Default none, as for InterruptDelay: the wait exists to look less mechanical, and a raise that waits leaves the
	// member dead that much longer. The member accepts the raise when he chooses, and the race with another raiser is
	// handled without it (concept 11, A358).
	public Vector2 RaiseDelay2 { get; set; } = new(0, 0);

	[Range(0, 10000, ConfigUnitType.None, 100)]
	[UI("Don't raise below this MP",
		Filter = RaiseActionCondition, Section = 1)]
	public int LessMPNoRaise { get; set; } = 2400;

	[UI("Keep the MP a raise needs",
		Description = "In a fight: a job that can raise spends MP on damage only while a raise's cost stays left "
			+ "over.\nHeals, the raise itself and free casts are not held.\nOff: damage spends MP below a raise's "
			+ "cost.",
		Filter = RaiseActionCondition, Section = 1)]
	public bool KeepMpForRaise { get; set; } = true;
	#endregion

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

	// Damage against the party's safety is each player's own choice; on is the owner's rule, safety first (A516).
	[ConditionBool, UI("Help heal figures only healing defeats as a non-healer",
		Description = "In a fight: as Red Mage, Paladin or Summoner you help heal figures only healing defeats, like "
			+ "Shinryu's Reiryu, when the healing coming is not enough in time.\nOff: you keep attacking.",
		Filter = HealingActionCondition, Section = 3)]
	private static readonly bool _healToDefeatAsNonHealer = true;

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

	[UI("Special state duration",
		Description = "How long a special state, such as Heal Area or Burst, lasts when a /rotation Special command turns it on.",
		Filter = BasicCommands, Section = 1)]
	[Range(1, 20, ConfigUnitType.Seconds, 1f)]
	public float SpecialDuration { get; set; } = 3;

	[UI("Stop casting delay range", Parent = nameof(UseStopCasting))]
	[Range(0, 3, ConfigUnitType.Seconds, 0.002f)]
	public Vector2 StopCastingDelay { get; set; } = new(0.5f, 1);

	[UI("Interrupt delay range", Parent = nameof(InterruptibleMoreCheck),
		Description = "In a fight: the interrupt never waits past the point where it could come too late - with one GCD of "
			+ "the cast left, it goes at once.",
		PvEFilter = JobFilterType.Interrupt, PvPFilter = JobFilterType.NoJob)]
	[Range(0, 3, ConfigUnitType.Seconds, 0.002f)]
	// Default none: an interrupt that waits can miss its cast, and the setting exists to look less mechanical, not to
	// play better (concept 11, A338). Upstream's old default of 0.5 to 1 s is cut to none on load (A500).
	public Vector2 InterruptDelay { get; set; } = new(0, 0);

	[UI("Provoke delay range", Parent = nameof(AutoProvokeForTank),
		Description = "In a fight: the provoke never waits past the point where it could come too late - not at all when "
			+ "taking the enemy from a co-tank, and no longer than until the enemy's first hit on the party member it "
			+ "turned to.")]
	[Range(0, 10, ConfigUnitType.Seconds, 0.05f)]
	// Default none, as for InterruptDelay: a provoke that waits leaves the enemy on whoever took it longer. Upstream's
	// old default of 0.5 to 1 s is cut to none on load (A500).
	public Vector2 ProvokeDelay { get; set; } = new(0, 0);

	[UI("Downtime healing delay range", Parent = nameof(HealWhenNothingTodo))]
	[Range(0, 5, ConfigUnitType.Seconds, 0.05f)]
	public Vector2 HealWhenNothingTodoDelay { get; set; } = new(0.5f, 1);

	[UI("Countdown lead time",
		Description = "How long before the countdown ends RSR starts casting or attacking.",
		Filter = BasicTimer, Section = 1, PvPFilter = JobFilterType.NoJob)]
	[Range(0, 0.7f, ConfigUnitType.Seconds, 0.002f)]
	public float CountDownAhead { get; set; } = 0.4f;

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
	private readonly float _minUpdatingTime = 0.00f;

	[JobConfig, Range(0.05f, 0.4f, ConfigUnitType.Percent)]
	[UI("Action ahead", Filter = BasicTimer,
	Description = "How much of the GCD is left when RSR starts queuing the next GCD. Whether an oGCD still fits before it RSR measures: the action's animation lock plus the delay to the server's answer, as it is in this zone.")]
	private readonly float _action6head = 0.25f;

	// Henchman's "No Fall Damage" hack (A276). Off by default: it hooks a game function, and nobody should run
	// that without having chosen it.
	[ConditionBool, UI("No fall damage",
	Description = "In a fight: a fall from a height costs no HP.\nIf this game version lacks the function, the "
		+ "setting switches itself off.\nOff: falls hurt as usual.",
	Filter = Extra)]
	private static readonly bool _noFallDamage = false;

	// Concept 03, "Weben, Animationssperre und Latenz" (A753): measured, no set value. On by default, his requirement being
	// that RSR replaces XivAlexander, NoClippy and BossMod's action tweaks.
	[ConditionBool, UI("Remove lag-induced animation lock delay from instant casts",
	Description = "In a fight: RSR takes the network round trip off each instant action's animation lock, so two "
		+ "oGCDs fit between GCDs more often.\nHeld while XivAlexander, or NoClippy not taken over, runs.\nOff: "
		+ "the full lock stays.",
	Filter = Extra, Section = 1)]
	private static readonly bool _removeAnimationLockDelay = true;

	[ConditionBool, UI("Take over BossMod's and NoClippy's animation lock functions",
	Description = "In a fight: only RSR shortens the animation lock; BossModReborn's and NoClippy's lock functions "
		+ "are switched off for the session and back on later.\nXivAlexander keeps the lock where it runs.",
	Parent = nameof(RemoveAnimationLockDelay))]
	private static readonly bool _takeOverOtherLockTools = true;

	[ConditionBool, UI("Remove framerate-induced cooldown delay",
	Description = "The game fires a queued action only on a frame, a little after its cooldown or lock ended. RSR credits that rest back to the next cooldown and lock, up to one mean frame of the zone. Nothing to set.",
	Filter = Extra, Section = 1)]
	private static readonly bool _removeCooldownDelay = true;

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
	Description = "In a fight: damage over time goes first on an enemy without it, the one with the most HP, then on "
		+ "the one whose runs out first.\nOff: the targeting type decides, as for every other attack.",
	Filter = TargetConfig)]
	private static readonly bool _dotTargetByHpThenExpiry = true;

	// Concept 03, "Gegner mit Stacheln" (A571): an enemy whose spikes damage its attackers is no target of an attack while
	// they stand; an action that removes them may still take it. On by default: the reflection lands with every hit, the
	// wait costs damage, not HP (the owner's rule: safety before damage).
	[ConditionBool, UI("Do not attack enemies whose spikes damage their attackers",
	Description = "In a fight: RSR does not attack an enemy whose spikes, counter or reflection would turn your kind "
		+ "of attack back on you.\nAn action that removes that status may still target it.\nOff: RSR attacks it "
		+ "and takes the damage it returns.",
	Filter = TargetConfig, Section = 1)]
	private static readonly bool _avoidSpikedEnemies = true;

	// Concept 03, "Tötungsregeln", rules 2 and 3 (A588), with the fights in concept 21, K12: enemies that must fall together,
	// after another, only in a place or only at a time. On by default: a tablitaur that falls too early enrages the other (Daedalus, in the
	// field), a morpho's death confuses you.
	[ConditionBool, UI("Kill rules of known fights",
	Description = "In a fight: enemies of known fights fall in the order they must - pairs together, one whose death "
		+ "harms you only when that is safe.\nRSR notes each hold and release in DefenseTrace.log.\nOff: RSR "
		+ "attacks them like any other.",
	Filter = TargetConfig, Section = 1)]
	private static readonly bool _killRules = true;

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

	// Every attack that carries the player onto its target reads it - the dashes through the targeting filter, the leaps
	// through CustomRotation.GapCloserAllowed (A358).
	[UI("Only use damaging gap closers within this distance",
		Description = "Gap closers that deal damage are only used on targets within this distance, the distance itself "
			+ "included: the dashes such as Onslaught and the leaps onto the target - Intervene, Primal Rend, Crimson Cyclone, "
			+ "Stardiver, Dragonfire Dive. A distance a rotation sets for one of them applies on top. Not limited: a "
			+ "move-forward command, your own press of the action, the countdown, and a rotation switch you turn on to leap "
			+ "regardless of distance. The Beastmaster's Shield Charge goes onto enemies up to melee range (3 yalms) whatever "
			+ "this distance, and never farther.\n"
			+ "In a fight: RSR does not carry you from a spot you stepped to onto the enemy; standing at the enemy (0 yalms), "
			+ "a gap closer is only damage and goes even with this set to 0.",
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
}
