using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Rotations.Duties;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic;

internal static partial class DataCenter
{
	public static List<IBattleChara> PartyMembers { get; set; } = [];

	/// <summary>
	/// The members an enemy's damage over time alone takes below the single-heal threshold before a heal over time laid
	/// now would run out, lowest first - set by the heal-over-time path right before it asks its action (A432).
	/// </summary>
	internal static List<IBattleChara> HotAgainstDotMembers { get; set; } = [];

	public static List<IBattleChara> AllianceMembers { get; set; } = [];

	/// <summary>
	/// Friendly figures outside the party that only single-target healing defeats (<see cref="HealToDefeat"/>): read by
	/// the target choice of single heals, never by party values.
	/// </summary>
	public static List<IBattleChara> HealToDefeatTargets { get; set; } = [];

	/// <summary>
	/// Concept 03, "Gegner mit Stacheln" (A571): the enemies in reach left out of <see cref="AllHostileTargets"/> because
	/// their spikes damage their attackers - candidates only of an action that removes those spikes.
	/// </summary>
	public static List<IBattleChara> SpikedHostiles { get; set; } = [];

	/// <summary>
	/// Concept 03, "Gegner, die keine Fläche treffen darf" (A572): the enemies in reach that no attack may hit, not even an
	/// area one - spiked enemies, stop-marked ones while that setting is on, and the Zu's eggs while the Zu lives.
	/// </summary>
	public static List<IBattleChara> UntouchableHostiles { get; set; } = [];

	public static List<IBattleChara> AllHostileTargets { get; set; } = [];

	/// <summary>
	/// The enemies whose casts the cast recognition reads: <see cref="AllHostileTargets"/>, and the untargetable
	/// enemies - the helpers that resolve many raidwides, tankbusters and knockbacks, a boss off the field - while
	/// they cast an action on the area list (A277) or on the tankbuster or knockback list, or a tankbuster the
	/// table has measured (A303). Rebuilt with the hostile list in <c>TargetUpdater</c>. Every reader asks its own
	/// list of the action, so a helper here for one list is nothing to the others.
	/// </summary>
	public static List<IBattleChara> AreaCasters { get; set; } = [];

	/// <summary>The untargetable members of <see cref="AreaCasters"/>.</summary>
	public static HashSet<ulong> AreaCastHelperIds { get; set; } = [];

	/// <summary>
	/// The party members an enemy is currently aiming at - either attacking them, or casting
	/// something that will land on them. Rebuilt once per frame beside the hostile list itself in
	/// <c>TargetUpdater.UpdateLists</c>.
	/// </summary>
	/// <remarks>
	/// Both sources are read because they answer different questions: the attack target is aggro,
	/// the cast target is what is about to arrive, and they part company on a boss that beats on the
	/// tank while casting at somebody else. Only party members are recorded, so an empty set means
	/// what it says - nothing is aimed at the party - rather than "no enemy has any target at all".
	/// Empty out of combat.
	/// </remarks>
	public static HashSet<ulong> TargetedPartyMembers { get; set; } = [];
	internal static HashSet<ulong> AllHostileTargetIds { get; } = [];

	/// <summary>
	/// The first of <see cref="InterruptCandidates"/>, for the status flag and derived rotations; the actions choose
	/// among all of them (concept 03, "Unterbrechen, Betäuben, Provozieren: welcher Gegner", A612).
	/// </summary>
	public static IBattleChara? InterruptTarget { get; set; }

	/// <summary>
	/// The first of <see cref="ProvokeCandidates"/>, for the status flag and derived rotations (A612).
	/// </summary>
	public static IBattleChara? ProvokeTarget { get; set; }

	/// <summary>
	/// Every enemy whose cast an interrupt should stop now, past the interrupt delay or with a GCD of its cast left, in
	/// the order of the hostile list. Each action takes the first among its own candidates - in its range and attackable
	/// by the engage setting -, so an enemy it cannot reach does not hold it back from one it can (concept 03, A612).
	/// </summary>
	internal static List<IBattleChara> InterruptCandidates { get; } = [];

	/// <summary>
	/// Every enemy a tank should provoke now, past the provoke delay or unable to wait, in the order of the hostile list;
	/// chosen per action as <see cref="InterruptCandidates"/> (A612).
	/// </summary>
	internal static List<IBattleChara> ProvokeCandidates { get; } = [];

	public static IBattleChara? DeathTarget { get; set; }

	public static IBattleChara? DispelTarget { get; set; }

	/// <summary>
	/// How many Esunas other players are casting on each party member right now, by object id (A293).
	/// </summary>
	internal static Dictionary<ulong, int> EsunasByOthers { get; set; } = [];

	public static List<IBattleChara> AllTargets { get; set; } = [];
	public static Dictionary<float, List<IBattleChara>> TargetsByRange { get; set; } = [];

	public static IAction? CurrentInterceptedAction { get; set; }

	public static bool IsInDutyReplay()
	{
		if (!PlayerAvailable())
		{
			return false;
		}

		return Svc.Condition[ConditionFlag.DutyRecorderPlayback];
	}

	public unsafe static Buddy.BuddyMember? ActivePet
	{
		get
		{
			var uiState = UIState.Instance();
			if (uiState == null || uiState->Buddy.PetInfo.Pet == null)
			{
				return null;
			}

			return *uiState->Buddy.PetInfo.Pet;
		}
	}

	private static Dictionary<int, XBMPet>? _xbmPetByDataId;

	private static Dictionary<int, XBMPet> XbmPetByDataId
	{
		get
		{
			if (_xbmPetByDataId != null)
			{
				return _xbmPetByDataId;
			}

			Dictionary<int, XBMPet> map = [];
			foreach (var x in Svc.Data.GetExcelSheet<XBMPet>())
			{
				_ = map.TryAdd(x.Unknown4, x);
			}
			return _xbmPetByDataId = map;
		}
	}

	public static bool BMPet
	{
		get
		{
			var dataId = ActivePet?.DataId;
			return dataId > 0 && XbmPetByDataId.ContainsKey(dataId.Value);
		}
	}

	public static BeastmasterKinType BMPetKinType
	{
		get
		{
			var dataId = ActivePet?.DataId;
			if (dataId is null or <= 0 || !XbmPetByDataId.TryGetValue(dataId.Value, out var row))
			{
				return BeastmasterKinType.None;
			}

			return row.Unknown7 switch
			{
				1 => BeastmasterKinType.Beastkin,
				2 => BeastmasterKinType.Vilekin,
				3 => BeastmasterKinType.Cloudkin,
				4 => BeastmasterKinType.Seedkin,
				5 => BeastmasterKinType.Wavekin,
				6 => BeastmasterKinType.Scalekin,
				7 => BeastmasterKinType.Soulkin,
				8 => BeastmasterKinType.Ashkin,
				_ => BeastmasterKinType.None
			};
		}
	}

	private static readonly BeastmasterAffinity[] PetAffinities =
	[
		BeastmasterAffinity.None,     // 0  (no pet)
		BeastmasterAffinity.Rampant,  // 1  Cu Sith
		BeastmasterAffinity.Rampant,  // 2  Squirrel
		BeastmasterAffinity.Rampant,  // 3  Lamb
		BeastmasterAffinity.Durant,   // 4  Pugil
		BeastmasterAffinity.Rampant,  // 5  Opo-opo
		BeastmasterAffinity.Eldritch, // 6  Dodo
		BeastmasterAffinity.Eldritch, // 7  Coblyn
		BeastmasterAffinity.Rampant,  // 8  Diremite
		BeastmasterAffinity.Durant,   // 9  Megalocrab
		BeastmasterAffinity.Volant,   // 10 Wespe
		BeastmasterAffinity.Volant,   // 11 Vulture
		BeastmasterAffinity.Rampant,  // 12 Mandragora
		BeastmasterAffinity.Eldritch, // 13 Geshunpest
		BeastmasterAffinity.Rampant,  // 14 Puk
		BeastmasterAffinity.Durant,   // 15 Crab
		BeastmasterAffinity.Durant,   // 16 Mantis
		BeastmasterAffinity.Eldritch, // 17 Slime
		BeastmasterAffinity.Durant,   // 18 Dullahan
		BeastmasterAffinity.Volant,   // 19 Bat
		BeastmasterAffinity.Volant,   // 20 Flying Trap
		BeastmasterAffinity.Durant,   // 21 Ziz
		BeastmasterAffinity.Rampant,  // 22 Sabotender
		BeastmasterAffinity.Eldritch, // 23 Golem
		BeastmasterAffinity.Durant,   // 24 Apkallu
		BeastmasterAffinity.Eldritch, // 25 Adamantoise
		BeastmasterAffinity.Rampant,  // 26 Buffalo
		BeastmasterAffinity.Durant,   // 27 Uragnite
		BeastmasterAffinity.Eldritch, // 28 Worm
		BeastmasterAffinity.Rampant,  // 29 Spriggan
		BeastmasterAffinity.Rampant,  // 30 Goobbue
		BeastmasterAffinity.Eldritch, // 31 Gigantoad
		BeastmasterAffinity.Volant,   // 32 Colibri
		BeastmasterAffinity.Eldritch, // 33 Coeurl
		BeastmasterAffinity.Durant,   // 34 Raptor
		BeastmasterAffinity.Rampant,  // 35 Drake
		BeastmasterAffinity.Eldritch, // 36 Treant
		BeastmasterAffinity.Rampant,  // 37 Antling
		BeastmasterAffinity.Rampant,  // 38 Chimera
		BeastmasterAffinity.Rampant,  // 39 Morbol
		BeastmasterAffinity.Volant,   // 40 Ghost
		BeastmasterAffinity.Durant,   // 41 Salamander
		BeastmasterAffinity.Durant,   // 42 Cobra
		BeastmasterAffinity.Durant,   // 43 Hydra
		BeastmasterAffinity.Volant,   // 44 Damselfly
		BeastmasterAffinity.Eldritch, // 45 Rotting Goobbue
		BeastmasterAffinity.Volant,   // 46 Zu
		BeastmasterAffinity.Durant,   // 47 Ice Golem
		BeastmasterAffinity.Durant,   // 48 Karlabos
		BeastmasterAffinity.Eldritch, // 49 Rafflesia
		BeastmasterAffinity.Eldritch, // 50 Behemoth
	];

	public static BeastmasterAffinity AffinityOf(BeastmasterPet pet)
	{
		var index = (int)pet;
		return index > 0 && index < PetAffinities.Length ? PetAffinities[index] : BeastmasterAffinity.None;
	}

	public static BeastmasterAffinity BMPetAffinity
	{
		get
		{
			var dataId = ActivePet?.DataId;
			return dataId is null or <= 0 || !XbmPetByDataId.TryGetValue(dataId.Value, out var row)
				? BeastmasterAffinity.None
				: AffinityOf((BeastmasterPet)row.RowId);
		}
	}

	private static ulong _hostileTargetId = 0;
	private static bool _hasCastScythe = false;
	private static bool _hasCastAxe = false;
	private static bool _wasCastingCharyb = false;
	private static bool _tyrantShouldStopHealing = false;

	public static bool ResetActionConfigs { get; set; } = false;

	public static int PlayerSyncedLevel()
	{
		if (Player.IsLevelSynced)
		{
			return Player.SyncedLevel;
		}

		if (PlayerCurrentLevel < PlayerMaxLevel)
		{
			return PlayerCurrentLevel;
		}

		return PlayerMaxLevel;
	}

	public unsafe static int PlayerCurrentLevel => PlayerState.Instance()->CurrentLevel;

	public static int PlayerMaxLevel => Player.MaxLevel;

	public static bool IsActivated()
	{
		return Player.Available && (State || IsManual || Service.Config.TeachingMode) && !PvPAutomationBlocked;
	}

	public static bool IsActivatedIPC()
	{
		return Player.Available && (State || IsManual) && !PvPAutomationBlocked;
	}

	public static bool PlayerAvailable()
	{
		return Player.Available && Player.Object != null;
	}

	public static bool DalamudStagingEnabled = false;
	public static bool IsOnStaging()
	{
		try
		{
			var v = Svc.PluginInterface.GetDalamudVersion();
			if (v.BetaTrack != null && v.BetaTrack.Equals("release", StringComparison.CurrentCultureIgnoreCase))
			{
				DalamudStagingEnabled = false;
				return false;
			}
			else
			{
				DalamudStagingEnabled = true;
				return true;
			}
		}
		catch (Exception ex)
		{
			ex.Log("Probably CN or something");
			DalamudStagingEnabled = false;
			return false;
		}
	}

	public static bool AutoFaceTargetOnActionSetting()
	{
		return Svc.GameConfig.UiControl.GetBool(UiControlOption.AutoFaceTargetOnAction.ToString());
	}

	public static uint MoveModeSetting()
	{
		return Svc.GameConfig.UiControl.GetUInt(UiControlOption.MoveMode.ToString());
	}

	private static int _hostileTargetIndex = -1;

	internal static IBattleChara? HostileTarget
	{
		get
		{
			if (_hostileTargetId == 0)
			{
				return null;
			}

			if (_hostileTargetIndex >= 0 && Svc.Objects[_hostileTargetIndex] is IBattleChara inSlot && inSlot.GameObjectId == _hostileTargetId)
			{
				return inSlot;
			}

			var target = Svc.Objects.SearchById(_hostileTargetId) as IBattleChara;
			_hostileTargetIndex = target?.ObjectIndex ?? -1;
			return target;
		}
		set
		{
			_hostileTargetId = value?.GameObjectId ?? 0;
			_hostileTargetIndex = -1;
		}
	}

	internal static List<uint> PrioritizedNameIds { get; set; } = [];
	internal static List<uint> BlacklistedNameIds { get; set; } = [];

	internal static List<uint> RestrictedDotNameIds { get; set; } =
	[
		9214,
	];

	internal static List<uint> RestrictedActionNameIds { get; set; } =
	[
		14301, 14499,
	];

	internal static ConcurrentQueue<VfxNewData> VfxDataQueue { get; } = new();

	/// <summary>
	/// Ids of the living party members a tankbuster marker stands on, for every role (<see cref="UpdateTankbusterTargets"/>,
	/// once per frame). Kept as ids so a despawned object is never reused.
	/// </summary>
	internal static List<ulong> TankbusterTargetIds { get; } = [];

	private static readonly Lock _tankbusterLock = new();

	/// <summary>
	/// The members in the latest sample of <see cref="RecordedHP"/>: whose time to death GetTTK reads from it.
	/// </summary>
	internal static HashSet<ulong> RecordedPartyIds { get; } = [];

	public static ICustomRotation? CurrentRotation { get; internal set; }
	public static DutyRotation? CurrentDutyRotation { get; internal set; }

	public static Dictionary<string, DateTime> SystemWarnings { get; set; } = [];
	public static bool HoldingRestore = false;

	internal static bool NoPoslock => Svc.Condition[ConditionFlag.OccupiedInEvent]
									  || !Service.Config.PoslockCasting
									  || Svc.KeyState[Service.Config.PoslockModifier.ToVirtual()]
									  || Svc.GamepadState.Raw(Dalamud.Game.ClientState.GamePad.GamepadButtons.R1) >=
									  0.5f
									  || (Service.Config.PosLockMouse && InputManager.IsLeftMouseDown() && InputManager.IsRightMouseDown());

	internal static DateTime EffectTime { private get; set; } = DateTime.Now;
	internal static DateTime EffectEndTime { private get; set; } = DateTime.Now;

	internal static int AttackedTargetsCount { get; set; } = 48;
	internal static Queue<(ulong id, DateTime time)> AttackedTargets { get; } = new(AttackedTargetsCount);

	internal static Queue<MacroItem> Macros { get; } = new Queue<MacroItem>();

	internal static bool InEffectTime => DateTime.Now >= EffectTime && DateTime.Now <= EffectEndTime;
	internal static Dictionary<ulong, uint> HealHP { get; set; } = [];

	// How much health one of our own healing actions restores to one target, in points, per action id.
	// Healing is an absolute figure just like damage, so it is stored as points; a caller compares it
	// with points (a member's missing health), never with a share.
	//
	// This exists because the potency in an effect text cannot be converted into points from here:
	// the result depends on healing power, on the job gauge and on buffs, and it changes with every
	// piece of gear. Asking the fight instead costs nothing - the effect handler already sees every
	// heal we land, with the real number.
	//
	// The SMALLEST amount seen is kept, not a mean. Owner's rule: a heal can land as a critical hit
	// and be much larger, and a need is judged against the least the heal will surely restore, never
	// the most. Only amounts that measure the whole heal count: one that met more missing health than
	// it restored (nothing was lost to overheal), or any amount once the packet is seen to report
	// overheal at all - then every amount is the full heal. It is cleared on a territory change only
	// (ResetHealMeasurements): item level sync is set per duty, so a figure from outside would be
	// wrong inside; a gear change is followed from the next zone on.
	private static readonly Dictionary<uint, float> _observedHealPerCast = [];
	private static readonly Dictionary<uint, HealLanding> _lastHealLanding = [];

	internal static Dictionary<ulong, uint> ApplyStatus { get; set; } = [];
	internal static uint MPGain { get; set; }

	public static AutoStatus MergedStatus => AutoStatus | CommandStatus;
	public static AutoStatus AutoStatus { get; set; } = AutoStatus.None;
	public static AutoStatus CommandStatus { get; set; } = AutoStatus.None;

	private static readonly List<NextAct> NextActs = [];

	public static IAction? CommandNextAction
	{
		get
		{
			NextAct? next = null;
			if (NextActs.Count > 0)
			{
				next = NextActs[0];
			}

			while (next != null && NextActs.Count > 0 &&
				   (next.DeadTime < DateTime.Now || IActionHelper.IsLastAction(false, next.Act)))
			{
				NextActs.RemoveAt(0);
				next = NextActs.Count > 0 ? NextActs[0] : null;
			}

			return next?.Act;
		}
	}

	internal static CommandTarget? GetCommandTarget(IAction act)
	{
		for (var i = 0; i < NextActs.Count; i++)
		{
			var next = NextActs[i];
			if (next.Act.ID == act.ID && next.DeadTime >= DateTime.Now)
			{
				return next.Target;
			}
		}

		return null;
	}

	internal static void AddCommandAction(IAction act, double time, CommandTarget? target = null)
	{
		var index = -1;
		for (var i = 0; i < NextActs.Count; i++)
		{
			if (NextActs[i].Act.ID == act.ID)
			{
				index = i;
				break;
			}
		}

		NextAct newItem = new(act, DateTime.Now.AddSeconds(time), target);
		if (index < 0)
		{
			NextActs.Add(newItem);
		}
		else
		{
			NextActs[index] = newItem;
		}

		NextActs.Sort((a, b) => a.DeadTime.CompareTo(b.DeadTime));
	}

	public static IpcStateOverrides? IpcOverrides { get; set; }

	public static IpcStateOverrides? ActiveIpcOverrides => State ? IpcOverrides : null;

	public static TargetHostileType CurrentTargetToHostileType => ActiveIpcOverrides?.HostileType ?? Service.Config.HostileType;

	public static bool TargetFreelyEnabled => ActiveIpcOverrides?.TargetFreely ?? (Service.Config.TargetFreely || TargetFreelyOverride);

	public static bool AutoOffAfterCombatEnabled => ActiveIpcOverrides?.AutoOffAfterCombat ?? Service.Config.AutoOffAfterCombat;

	public static bool FriendlyPartyNpcHealRaiseEnabled => ActiveIpcOverrides?.FriendlyPartyNpcHealRaise ?? Service.Config.FriendlyPartyNpcHealRaise3;

	public static TargetingType? TargetingTypeOverride { get; set; }

	public static TargetingType TargetingType
	{
		get
		{
			if (TargetingTypeOverride.HasValue)
			{
				return TargetingTypeOverride.Value;
			}

			// The owner's precept of 04.11.2026 (concept 03, "Zielart ohne Tank und Heiler", A525, A526): a beastmaster,
			// and on his suggestion a blue mage, with no tank and no healer alive beside him attacks the enemy with the
			// least HP first, whatever the setting - fewer enemies hit him sooner.
			// Its order is EnemyDanger's in the target selection; read as a targeting type, it is the fewest HP first.
			if (DangerFirstAlone)
			{
				return TargetingType.LowHP;
			}

			if (Service.Config.TargetingTypes.Count == 0)
			{
				Service.Config.TargetingTypes.Add(TargetingType.LowHP);
				Service.Config.TargetingTypes.Add(TargetingType.HighHP);
				Service.Config.TargetingTypes.Add(TargetingType.Small);
				Service.Config.TargetingTypes.Add(TargetingType.Big);
				Service.Config.Save();
			}

			return Service.Config.TargetingTypes[Service.Config.TargetingIndex % Service.Config.TargetingTypes.Count];
		}
	}

	public static TinctureUseType CurrentTinctureUseType => Service.Config.TinctureType;

	private static uint _comboRawId = uint.MaxValue;
	private static ActionID _comboAction = ActionID.None;

	public static unsafe ActionID LastComboAction
	{
		get
		{
			var manager = ActionManager.Instance();
			if (manager->Combo.Timer <= 0)
			{
				return ActionID.None;
			}

			var id = manager->Combo.Action;
			if (id != _comboRawId)
			{
				var action = Svc.Data.GetExcelSheet<Action>()?.GetRowOrDefault(id);
				var cate = action?.GetActionCate() ?? ActionCate.None;
				_comboAction = cate is ActionCate.Weaponskill or ActionCate.Spell ? (ActionID)id : ActionID.None;
				_comboRawId = id;
			}

			return _comboAction;
		}
	}

	public static unsafe float ComboTime => ActionManager.Instance()->Combo.Timer;

	public static bool IsMoving => Player.IsMoving || BMRIsMoving;

	private static DateTime _closingLastRead = DateTime.MinValue;

	internal static float StopMovingRaw { get; set; }

	internal static float MovingRaw { get; set; }
	internal static float DeadTimeRaw { get; set; }
	internal static float AliveTimeRaw { get; set; }

	public static uint[] BluSlots { get; internal set; } = new uint[24];

	public static uint[] DutyActions { get; internal set; } = new uint[5];

	private static DateTime _specialStateStartTime = DateTime.MinValue;
	private static double SpecialTimeElapsed => (DateTime.Now - _specialStateStartTime).TotalSeconds;
	internal static double? SpecialDurationOverride { get; private set; } = null;
	public static double SpecialTimeLeft => (SpecialDurationOverride ?? Service.Config.SpecialDuration) - SpecialTimeElapsed;

	internal static Action<SpecialCommandType>? OnSpecialTypeChanged { get; set; }

	private static SpecialCommandType _specialType = SpecialCommandType.EndSpecial;

	internal static SpecialCommandType SpecialType
	{
		get => SpecialTimeLeft < 0 ? SpecialCommandType.EndSpecial : _specialType;
		set
		{
			_specialType = value;
			if (value == SpecialCommandType.EndSpecial)
			{
				SpecialDurationOverride = null;
				_specialStateStartTime = DateTime.MinValue;
			}
			else
			{
				SpecialDurationOverride = null;
				_specialStateStartTime = DateTime.Now;
			}
			OnSpecialTypeChanged?.Invoke(value);
		}
	}

	internal static void SetSpecialTypeWithDuration(SpecialCommandType value, double duration)
	{
		_specialType = value;
		if (value == SpecialCommandType.EndSpecial)
		{
			SpecialDurationOverride = null;
			_specialStateStartTime = DateTime.MinValue;
		}
		else
		{
			SpecialDurationOverride = duration;
			_specialStateStartTime = DateTime.Now;
		}
		OnSpecialTypeChanged?.Invoke(value);
	}

	public static bool State { get; set; } = false;

	public static bool IsManual { get; set; } = false;

	public static bool IsAutoDuty { get; set; } = false;

	public static bool IsHenched { get; set; } = false;

	public static bool IsPvPStateEnabled { get; set; } = false;

	public static bool IsTargetOnly { get; set; } = false;

	public static bool InCombat { get; set; } = false;

	public static bool DrawingActions { get; set; } = false;

	private static RandomDelay _notInCombatDelay = new(() => Service.Config.NotInCombatDelay);

	public static bool NotInCombatDelay => _notInCombatDelay.Delay(!InCombat);

	internal static float CombatTimeRaw { get; set; }

	private static DateTime _startRaidTime = DateTime.MinValue;

	internal static float RaidTimeRaw
	{
		get
		{
			if (_startRaidTime == DateTime.MinValue)
			{
				return 0;
			}

			return (float)(DateTime.Now - _startRaidTime).TotalSeconds;
		}
		set
		{
			if (value < 0)
			{
				_startRaidTime = DateTime.MinValue;
			}
			else
			{
				_startRaidTime = DateTime.Now - TimeSpan.FromSeconds(value);
			}
		}
	}

	private static float _cachedJobRange = -1f;
	private static int _cachedTargetCount = 0;
	private static int _lastTargetFrame = -1;

	public static bool MobsTime
	{
		get
		{
			var currentFrame = Environment.TickCount;
			if (_lastTargetFrame != currentFrame)
			{
				_cachedJobRange = JobRange;
				_cachedTargetCount = 0;
				var targets = AllHostileTargets;
				for (int i = 0, n = targets.Count; i < n; i++)
				{
					var o = targets[i];
					if (o.DistanceToPlayer() < _cachedJobRange && o.CanSee())
					{
						_cachedTargetCount++;
					}
				}
				_lastTargetFrame = currentFrame;
			}
			return _cachedTargetCount >= Service.Config.AutoDefenseNumber;
		}
	}

	private static float _avgTTK = float.PositiveInfinity;
	private static bool _avgTTKKnown;
	private static long _avgTTKCacheTick = long.MinValue;
	private const long AverageTtkTtlMs = 15;

	public static float AverageTTK
	{
		get
		{
			var now = Environment.TickCount64;
			if (_avgTTKCacheTick != long.MinValue && now - _avgTTKCacheTick < AverageTtkTtlMs)
			{
				return _avgTTK;
			}

			var total = 0f;
			var count = 0;
			var targets = AllHostileTargets;
			for (int i = 0, n = targets.Count; i < n; i++)
			{
				var ttk = targets[i].GetTTK();
				if (!float.IsNaN(ttk))
				{
					total += ttk;
					count++;
				}
			}
			// No target has an estimate yet for the first ~2.5s of a pull (GetTTK returns NaN until it
			// has hit-history). Every consumer reads a low value as "fight ending soon, skip it", so
			// "unknown" must be PositiveInfinity, not 0 - all of them compare via > or >=.
			_avgTTKKnown = count > 0;
			_avgTTK = _avgTTKKnown ? total / count : float.PositiveInfinity;
			_avgTTKCacheTick = now;
			return _avgTTK;
		}
	}

	private static float _packTTK = float.PositiveInfinity;
	private static long _packTTKCacheTick = long.MinValue;

	/// <summary>
	/// False while no hostile has a time to kill yet, e.g. early in a pull or during downtime. <see cref="AverageTTK"/> is
	/// PositiveInfinity then, so a consumer comparing it with > or >= lets the action through.
	/// </summary>
	public static bool IsAverageTTKKnown
	{
		get
		{
			_ = AverageTTK;
			return _avgTTKKnown;
		}
	}

	#region Territory Info Tracking

	public static Data.TerritoryInfo? Territory { get; set; }
	public static uint TerritoryID => (ushort)Svc.ClientState.TerritoryType;

	public static bool IsPvP => Territory?.IsPvP ?? false;

	public static bool TargetFreelyOverride { get; set; }

	public static bool IsInMaskedCarnivale => Territory?.ContentType == TerritoryContentType.TheMaskedCarnivale;

	public static bool IsInDuty => Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56];

	public static bool IsInQuestBattle => Territory?.ContentType == TerritoryContentType.QuestBattles;

	private static readonly ushort[] _allianceTerritoryIds =
	[
		151, 174, 372, 508, 556, 627, 734, 776, 826, 882, 917, 966, 1054, 1118, 1178, 1248, 1304, 1368
	];

	public static bool IsInAllianceRaid
	{
		get
		{
			var territoryId = TerritoryID;
			for (var i = 0; i < _allianceTerritoryIds.Length; i++)
			{
				if (_allianceTerritoryIds[i] == territoryId)
				{
					return true;
				}
			}

			return false;
		}
	}

	public static bool IsInTerritory(ushort territoryId)
	{
		return TerritoryID == territoryId;
	}

	#endregion

	#region Ultimate
	public static bool IsInUCoB => TerritoryID == 733;
	public static bool IsInUwU => TerritoryID == 777;
	public static bool IsInTEA => TerritoryID == 887;
	public static bool IsInDSR => TerritoryID == 968;
	public static bool IsInTOP => TerritoryID == 1122;
	public static bool IsInFRU => TerritoryID == 1238;
	public static bool IsInDMU => TerritoryID == 1363;
	#endregion

	#region Chaotic
	public static bool IsInCOD => TerritoryID == 1241;
	#endregion

	#region Savage
	public static bool IsInM9S => TerritoryID == 1321;
	public static bool IsInM10S => TerritoryID == 1323;
	public static bool IsInM11S => TerritoryID == 1327;
	public static bool IsInM12S => TerritoryID == 1325;
	#endregion

	#region Crucible
	public static bool IsInCrucible => IsinFirstBoard || IsinSecondBoard || IsinThirdBoard || IsinFirstMasterBoard || IsinSecondMasterBoard;
	public static bool IsinFirstBoard => TerritoryID == 1339;
	public static bool IsinSecondBoard => TerritoryID == 1340;
	public static bool IsinThirdBoard => TerritoryID == 1341;
	public static bool IsinFirstMasterBoard => TerritoryID == 1342;
	public static bool IsinSecondMasterBoard => TerritoryID == 1343;
	#endregion

	#region Extreme
	public static bool IsTheUnmaking => TerritoryID == 1362;
	#endregion

	#region Alliance Raid
	public static bool IsInWindurst => TerritoryID == 1368;
	#endregion

	#region FATE
	public static unsafe ushort PlayerFateId
	{
		get
		{
			try
			{
				if ((IntPtr)FateManager.Instance() != IntPtr.Zero
					&& (IntPtr)FateManager.Instance()->CurrentFate != IntPtr.Zero
					&& PlayerSyncedLevel() <= FateManager.Instance()->CurrentFate->MaxLevel)
				{
					return FateManager.Instance()->CurrentFate->FateId;
				}
			}
			catch (Exception ex)
			{
				PluginLog.Error(ex.StackTrace ?? ex.Message);
			}

			return 0;
		}
	}

	public static bool IsInFate => PlayerFateId != 0 && !IsInBozja && !IsInOccultCrescentOp;

	#endregion

	#region Treasure Hunt
	public static bool IsInTreasureHunt => Territory?.ContentType == TerritoryContentType.TreasureHunt;

	public static bool IsInTheAquapolis => TerritoryID == 558;

	public static bool IsInTheLostCanalsofUznair => TerritoryID == 712;

	public static bool IsInTheShiftingAltarsofUznair => TerritoryID == 794;

	public static bool IsInTheHiddenCanalsofUznair => TerritoryID == 725;

	public static bool IsInTheDungeonsofLyheGhiah => TerritoryID == 879;

	public static bool IsInTheShiftingOubliettesofLyheGhiah => TerritoryID == 924;

	public static bool IsInTheExcitatron6000 => TerritoryID == 1000;

	public static bool IsInTheShiftingGymnasionAgonon => TerritoryID == 1123;

	public static bool IsInCenoteJaJaGural => TerritoryID == 1209;

	public static bool IsInVaultOneiron => TerritoryID == 1279;
	#endregion

	#region Bozja
	public static bool IsInBozjanFieldOp => Content.ContentType == ECommons.GameHelpers.ContentType.FieldOperations
		&& Territory?.ContentType == TerritoryContentType.SaveTheQueen;

	public static bool IsInBozjanFieldOpCE => IsInBozjanFieldOp
		&& StatusHelper.PlayerHasStatus(false, StatusID.DutiesAsAssigned);

	public static bool IsInDelubrumNormal => Content.ContentType == ECommons.GameHelpers.ContentType.FieldRaid
		&& Territory?.ContentType == TerritoryContentType.SaveTheQueen;

	public static bool IsInDelubrumSavage => Content.ContentType == ECommons.GameHelpers.ContentType.FieldRaid
		&& Content.ContentDifficulty == ContentDifficulty.FieldRaidsSavage
		&& Territory?.ContentType == TerritoryContentType.SaveTheQueen;

	public static bool IsInBozja => IsInBozjanFieldOp || IsInDelubrumNormal || IsInDelubrumSavage;
	#endregion

	public static bool IsInFieldOperations => Content.ContentType == ECommons.GameHelpers.ContentType.FieldOperations;

	public static bool IsInFieldRaid => Content.ContentType == ECommons.GameHelpers.ContentType.FieldRaid;

	#region Occult Crescent
	public static bool IsInOccultCrescentOp => Territory?.ContentType == TerritoryContentType.OccultCrescent;

	public static bool IsInNorthHorn => IsInOccultCrescentOp && TerritoryID == 1346;

	public static bool IsInSouthHorn => IsInOccultCrescentOp && TerritoryID == 1252;

	public static bool IsInForkedTowerBlood => IsInOccultCrescentOp && StatusHelper.PlayerHasStatus(false, StatusID.DutiesAsAssigned_4228);
	#endregion

	#region Variant Dungeon
	public static bool TheMerchantsTaleAdvanced => IsInTerritory(1316);

	public static bool TheMerchantsTale => IsInTerritory(1315);

	public static bool SildihnSubterrane => IsInTerritory(1069);

	public static bool MountRokkon => IsInTerritory(1137);

	public static bool AloaloIsland => IsInTerritory(1176);

	public static bool InVariantDungeon => TheMerchantsTaleAdvanced || TheMerchantsTale || AloaloIsland || MountRokkon || SildihnSubterrane;
	#endregion

	#region Misc Duty Info

	public static bool IsInEurekaFieldOp => Territory?.ContentType == TerritoryContentType.Eureka;

	public static bool IsInDeepDungeons => Territory?.ContentType == TerritoryContentType.DeepDungeons;

	public static bool IsInPilgrimsTraverse => IsInTerritory(1281)
	|| IsInTerritory(1282) || IsInTerritory(1283)
	|| IsInTerritory(1284) || IsInTerritory(1285)
	|| IsInTerritory(1286) || IsInTerritory(1287)
	|| IsInTerritory(1288) || IsInTerritory(1289)
	|| IsInTerritory(1290);

	public static bool IsInPalaceOfTheDead => IsInTerritory(561)
	|| IsInTerritory(562) || IsInTerritory(563)
	|| IsInTerritory(564) || IsInTerritory(565)
	|| IsInTerritory(593) || IsInTerritory(594)
	|| IsInTerritory(595) || IsInTerritory(596)
	|| IsInTerritory(597) || IsInTerritory(598)
	|| IsInTerritory(599) || IsInTerritory(600)
	|| IsInTerritory(601) || IsInTerritory(602)
	|| IsInTerritory(603) || IsInTerritory(604)
	|| IsInTerritory(605) || IsInTerritory(606)
	|| IsInTerritory(607);

	public static bool IsInTheFinalVerse => IsInTerritory(1311) || IsInTerritory(1333);

	public static bool IsInMonsterHunterDuty => RathalosNormal || RathalosEX || ArkveldNormal || ArkveldEX;

	public static bool RathalosNormal => IsInTerritory(761);

	public static bool RathalosEX => IsInTerritory(762);

	public static bool ArkveldNormal => IsInTerritory(1300);

	public static bool ArkveldEX => IsInTerritory(1306);

	public static bool Orbonne => IsInTerritory(826);

	public static bool Emanation => IsInTerritory(719);

	public static bool EmanationEX => IsInTerritory(720);
	#endregion

	#region Job Info
	public static Job Job => Player.Job;

	public static RotationType PlayerRotationType => RotationType.Reborn;

	// TODO: add the two Evercold jobs once they exist in ecommons.
	private static readonly HashSet<Job> EvolvedOnlyJobs = [];

	public static bool IsRotationTypeAllowed(Job job, RotationType type)
	{
		if (job is Job.BLU or Job.BST)
		{
			return true;
		}

		return type == (EvolvedOnlyJobs.Contains(job) ? RotationType.Evolved : PlayerRotationType);
	}

	private static readonly BaseItem PhoenixDownItem = new(4570);

	/// <summary>
	/// Whether a healer in the party is alive, the player counted unless <paramref name="excludeSelf"/>. Without one
	/// nothing restores what a member loses but his own heals: the heals a tank or damage dealer holds back for the
	/// healers go (<c>CustomRotation.HoldPartyHeal</c>, <c>GcdHealsAllowed</c>), and so does the defence the player
	/// keeps (<see cref="UnhealedUnderAttack"/>, A361). A living raiser does not count here: whether he raises, and
	/// when, is another player's doing, and a Phoenix Down in someone's bag cannot be seen at all (the owner's note,
	/// 25.10.2026). One definition for the three callers that carried it as their own loop.
	/// </summary>
	/// <param name="excludeSelf">Skip the player, for a healer asking whether another healer is there.</param>
	/// <returns>True when a healer of the party is alive.</returns>
	public static bool AnyLivingHealer(bool excludeSelf)
	{
		return AnyLiving(JobRole.Healer, excludeSelf);
	}

	// A tankbuster marker on a tank of the party, or a cast at him from the tankbuster list or the measured table.
	private static readonly FrameCached<bool> _knownTankbusterAtATank = new(() =>
	{
		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member != null && !member.IsDead && member.IsJobCategory(JobRole.Tank) && IsTankbusterAimedAt(member))
			{
				return true;
			}
		}

		return false;
	});

	public static bool CanRaise()
	{
		if (IsPvP)
		{
			return false;
		}

		if (Service.Config.UsePhoenixDown && PhoenixDownItem.HasIt)
		{
			return true;
		}

		if ((Role == JobRole.Healer || Job == Job.SMN) && PlayerSyncedLevel() >= RaiseLevel)
		{
			return true;
		}

		if (Job == Job.RDM && PlayerSyncedLevel() >= VerraiseLevel)
		{
			return true;
		}

		if (DutyRotation.ChemistLevel >= 3)
		{
			return true;
		}

		if (DutyRotation.WhiteMageLevel >= 4)
		{
			return true;
		}

		if (StatusHelper.PlayerHasStatus(false, StatusID.VariantRaiseSet))
		{
			return true;
		}
		return false;
	}

	private static Job _roleJob = Job.ADV;
	private static JobRole _role = JobRole.None;

	public static JobRole Role
	{
		get
		{
			if (CurrentRotation is BlueMageRotation)
			{
				return BlueMageRotation.Role;
			}

			var job = Job;
			if (job != _roleJob || job == Job.ADV)
			{
				var classJob = Service.GetSheet<ClassJob>().GetRowOrDefault((uint)job);
				_role = classJob is { RowId: not 0 } row ? row.GetJobRole() : JobRole.None;
				_roleJob = job;
			}

			return _role;
		}
	}

	public static float JobRange
	{
		get
		{
			float radius = 25;
			if (!Player.Available)
			{
				return radius;
			}

			switch (Role)
			{
				case JobRole.Tank:
				case JobRole.Melee:
					radius = IsPvP ? 5 : 3;
					break;
			}

			return radius;
		}
	}

	public static unsafe bool SylphManagementFinished()
	{
		if (UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(66049))
		{
			return true;
		}

		return false;
	}

	public static bool BaseClass()
	{
		if (Svc.Objects.LocalPlayer == null)
		{
			return false;
		}

		var rowId = Svc.Objects.LocalPlayer.ClassJob.RowId;
		return (rowId >= 1 && rowId <= 7) || rowId == 26 || rowId == 29;
	}
	#endregion

	#region GCD
	/// <summary>
	/// Returns the current animation lock remaining time (seconds).
	/// </summary>
	public static float AnimationLock => Player.AnimationLock;

	/// <summary>
	/// Time until the next ability relative to the next GCD window.
	/// Non-negative (clamped to 0).
	/// </summary>
	public static float NextAbilityToNextGCD => Math.Max(0f, DefaultGCDRemain - AnimationLock);

	/// <summary>
	/// Returns the total duration of the default GCD (seconds): the length of one GCD, also while none is
	/// running.
	/// </summary>
	/// <remarks>
	/// The game reports the GCD group's total only while its timer runs; between GCDs - out of range, between
	/// pulls, before the first GCD of a fight - it reads 0. Every reader asks for the length of a GCD (a lead
	/// time, a window, a throttle, "the first n GCDs of combat"), and 0 shut each of them: the defence trace of
	/// 01.10.2026 holds the same decision two and three times within 50 ms, past a throttle of one GCD (A240).
	/// So the last length the game reported stands in while the timer is idle, and before the first GCD the
	/// adjusted recast of a weaponskill does - the game's own figure for the player's speed.
	/// </remarks>
	public static float DefaultGCDTotal
	{
		get
		{
			var running = ActionManagerHelper.GetDefaultRecastTime();
			if (running > 0f)
			{
				_lastGcdTotal = running;
				return running;
			}

			return _lastGcdTotal > 0f ? _lastGcdTotal : Math.Max(0f, ActionManagerHelper.GetDefaultAdjustedRecastTime());
		}
	}

	/// <summary>
	/// Returns the remaining time for the default GCD by subtracting the elapsed time from the total recast time.
	/// Clamped to non-negative; 0 while no GCD is running.
	/// </summary>
	public static float DefaultGCDRemain => Math.Max(0f, ActionManagerHelper.GetDefaultRecastTime() - DefaultGCDElapsed);

	/// <summary>
	/// Returns the elapsed time since the start of the default GCD. Clamped to non-negative.
	/// </summary>
	public static float DefaultGCDElapsed => Math.Max(0f, ActionManagerHelper.GetDefaultRecastTimeElapsed());

	/// <summary>
	/// Calculates the action ahead time based on the default GCD total and configured multiplier.
	/// Result is clamped to non-negative.
	/// </summary>
	public static float CalculatedActionAhead => Math.Max(0f, DefaultGCDTotal * Service.Config.Action6Head);

	/// <summary>
	/// Calculates the total GCD time for a given number of GCDs and an optional offset.
	/// </summary>
	/// <param name="gcdCount">The number of GCDs.</param>
	/// <param name="offset">The optional offset.</param>
	/// <returns>The total GCD time.</returns>
	public static float GCDTime(uint gcdCount = 0, float offset = 0)
	{
		return (DefaultGCDTotal * gcdCount) + offset;
	}
	#endregion

	#region Pet Tracking
	public static bool HasPet()
	{
		return Svc.Buddies.PetBuddy != null;
	}

	public static unsafe bool HasCompanion
	{
		get
		{
			var playerBattleChara = Player.BattleChara;
			if (playerBattleChara == null)
			{
				return false;
			}

			var characterManager = CharacterManager.Instance();
			if (characterManager == null)
			{
				return false;
			}

			var companion = characterManager->LookupBuddyByOwnerObject(playerBattleChara);
			return (IntPtr)companion != IntPtr.Zero;
		}
	}

	public static unsafe BattleChara* GetCompanion()
	{
		var playerBattleChara = Player.BattleChara;
		if (playerBattleChara == null)
		{
			return null;
		}

		var characterManager = CharacterManager.Instance();
		return characterManager == null ? (BattleChara*)null : characterManager->LookupBuddyByOwnerObject(playerBattleChara);
	}

	/// <summary>
	/// Gets the players pet
	/// </summary>
	/// <returns>IBattleChara? pet</returns>
	public static IBattleChara? GetPet()
	{
		return Svc.Buddies.PetBuddy?.GameObject as IBattleChara;
	}
	#endregion

	#region HP

	private static readonly Dictionary<ulong, float> _refinedHpCache = [];
	private static long _refinedHpCacheTick = long.MinValue;

	private const long RefinedHpTtlMs = 15;

	public static Dictionary<ulong, float> RefinedHP
	{
		get
		{
			var now = Environment.TickCount64;
			if (_refinedHpCacheTick != long.MinValue && now - _refinedHpCacheTick < RefinedHpTtlMs)
			{
				return _refinedHpCache;
			}

			_refinedHpCache.Clear();
			foreach (var member in PartyMembers)
			{
				if (member == null || !member.IsValid() || member.GameObjectId == 0)
				{
					continue;
				}

				_refinedHpCache[member.GameObjectId] = GetPartyMemberHPRatio(member);
			}

			_refinedHpCacheTick = now;
			return _refinedHpCache;
		}
	}

	private static readonly Dictionary<ulong, uint> _lastHp = [];

	private static float GetPartyMemberHPRatio(IBattleChara member)
	{
		ArgumentNullException.ThrowIfNull(member);

		if (member.MaxHp == 0)
		{
			return 0f;
		}

		ConfirmPendingHeal(member);
		HealFromOthers.Follow(member);

		var id = member.GameObjectId;

		if (!InEffectTime || !HealHP.TryGetValue(id, out var healedHp))
		{
			_lastHp[id] = member.CurrentHp;
			return (float)member.CurrentHp / member.MaxHp;
		}

		var currentHp = member.CurrentHp;
		if (currentHp > 0)
		{
			_ = _lastHp.TryGetValue(id, out var lastHp);

			if (HealPrediction.HasLanded(currentHp, ref lastHp, healedHp))
			{
				_ = HealHP.Remove(id);
				_lastHp[id] = currentHp;
				return (float)currentHp / member.MaxHp;
			}

			_lastHp[id] = lastHp;

			return Math.Min(1, (healedHp + currentHp) / (float)member.MaxHp);
		}

		return (float)currentHp / member.MaxHp;
	}

	private static readonly float[] _hpBuffer = new float[16];
	private static long _partyHpStatsCacheTick = long.MinValue;
	private static float _minHpCache, _avgHpCache, _stdDevHpCache, _lowestAvgHpCache, _lowestStdDevHpCache;
	private const long PartyHpStatsTtlMs = 15;

	private static void ComputePartyHpStats(out float minHp, out float avgHp, out float stdDevHp, out float lowestAvgHp, out float lowestStdDevHp)
	{
		var now = Environment.TickCount64;
		if (_partyHpStatsCacheTick != long.MinValue && now - _partyHpStatsCacheTick < PartyHpStatsTtlMs)
		{
			minHp = _minHpCache;
			avgHp = _avgHpCache;
			stdDevHp = _stdDevHpCache;
			lowestAvgHp = _lowestAvgHpCache;
			lowestStdDevHp = _lowestStdDevHpCache;
			return;
		}

		var hpCount = 0;
		foreach (var member in PartyMembers)
		{
			if (member.IsValid() && member.GameObjectId != 0 && hpCount < _hpBuffer.Length)
			{
				var hp = GetPartyMemberHPRatio(member);
				if (hp > 0)
				{
					// What healing can close on him, for a member under a status that lowers it (concept 07, A544).
					_hpBuffer[hpCount++] = HealReduction.Restorable(member, hp);
				}
			}
		}

		if (hpCount == 0)
		{
			minHp = avgHp = stdDevHp = lowestAvgHp = lowestStdDevHp = 0;
			_minHpCache = _avgHpCache = _stdDevHpCache = _lowestAvgHpCache = _lowestStdDevHpCache = 0;
			_partyHpStatsCacheTick = now;
			return;
		}

		if (hpCount > 4)
		{
			Array.Sort(_hpBuffer, 0, hpCount);
		}

		float sum = 0;
		float lowestHpMembersSum = 0;
		var min = float.MaxValue;
		for (var i = 0; i < hpCount; i++)
		{
			sum += _hpBuffer[i];
			if (i < 4)
			{
				lowestHpMembersSum += _hpBuffer[i];
			}

			if (_hpBuffer[i] < min)
			{
				min = _hpBuffer[i];
			}
		}

		var avg = sum / hpCount;
		var lowestHpMembersAvg = lowestHpMembersSum / (hpCount > 4 ? 4 : hpCount);
		float variance = 0;
		float lowestHpMembersVariance = 0;
		for (var i = 0; i < hpCount; i++)
		{
			var diff = _hpBuffer[i] - avg;
			variance += diff * diff;
			if (i < 4)
			{
				var lowestHpMembersDiff = _hpBuffer[i] - lowestHpMembersAvg;
				lowestHpMembersVariance += lowestHpMembersDiff * lowestHpMembersDiff;
			}
		}

		minHp = min;
		avgHp = avg;
		stdDevHp = (float)Math.Sqrt(variance / hpCount);
		lowestAvgHp = lowestHpMembersAvg;
		lowestStdDevHp = (float)Math.Sqrt(lowestHpMembersVariance / (hpCount > 4 ? 4 : hpCount));

		_minHpCache = minHp;
		_avgHpCache = avgHp;
		_stdDevHpCache = stdDevHp;
		_lowestAvgHpCache = lowestAvgHp;
		_lowestStdDevHpCache = lowestStdDevHp;
		_partyHpStatsCacheTick = now;
	}

	private static readonly float[] _forecastHpBuffer = new float[_hpBuffer.Length];

	// One cache per kind of heal - off-GCD and GCD look ahead by different leads (A213).
	private static readonly long[] _forecastStatsCacheTick = [long.MinValue, long.MinValue];
	private static readonly float[] _forecastAvgCache = new float[_forecastStatsCacheTick.Length];
	private static readonly float[] _forecastStdDevCache = new float[_forecastStatsCacheTick.Length];
	private static readonly float[] _forecastLowestAvgCache = new float[_forecastStatsCacheTick.Length];
	private static readonly float[] _forecastLowestStdDevCache = new float[_forecastStatsCacheTick.Length];
	private static readonly float[] _forecastMinCache = new float[_forecastStatsCacheTick.Length];

	public static float PartyMembersMinHP
	{
		get
		{
			ComputePartyHpStats(out var minHp, out _, out _, out _, out _);
			return minHp;
		}
	}

	public static float PartyMembersAverHP
	{
		get
		{
			ComputePartyHpStats(out _, out var avgHp, out _, out _, out _);
			return avgHp;
		}
	}

	public static float PartyMembersDifferHP
	{
		get
		{
			ComputePartyHpStats(out _, out _, out var stdDevHp, out _, out _);
			return stdDevHp;
		}
	}

	public static float LowestPartyMembersAverHP
	{
		get
		{
			ComputePartyHpStats(out _, out _, out _, out var lowestAvgHp, out _);
			return lowestAvgHp;
		}
	}

	public static float LowestPartyMembersDifferHP
	{
		get
		{
			ComputePartyHpStats(out _, out _, out _, out _, out var lowestStdDevHp);
			return lowestStdDevHp;
		}
	}

	public static IEnumerable<float> PartyMembersHP
	{
		get
		{
			foreach (var member in PartyMembers)
			{
				if (member == null || !member.IsValid() || member.GameObjectId == 0)
				{
					continue;
				}

				var hp = GetPartyMemberHPRatio(member);
				if (hp > 0)
				{
					yield return hp;
				}
			}
		}
	}

	public static bool HPNotFull => PartyMembersMinHP < 1;

	public static uint CurrentMp => Player.Object != null ? Math.Min(10000, Player.Object.CurrentMp + MPGain) : MPGain;
	#endregion

	#region Action Record
	private const int QUEUECAPACITY = 48;
	private static readonly Queue<ActionRec> _actions = new(QUEUECAPACITY);
	private static readonly Queue<DamageRec> _damages = new(QUEUECAPACITY);

	internal static CombatRole? BluRole => CurrentRotation is BlueMageRotation ? BlueMageRotation.BlueId : null;
	public static float DPSTaken
	{
		get
		{
			try
			{
				var now = DateTime.Now;
				float damages = 0;
				DateTime? first = null, last = null;
				foreach (var rec in _damages)
				{
					if (now - rec.ReceiveTime < TimeSpan.FromSeconds(5))
					{
						damages += rec.Ratio;
						first ??= rec.ReceiveTime;
						last = rec.ReceiveTime;
					}
				}

				if (first == null || last == null)
				{
					return 0;
				}

				var time = last.Value - first.Value + TimeSpan.FromSeconds(2.5);
				return damages / (float)time.TotalSeconds;
			}
			catch
			{
				return 0;
			}
		}
	}

	public static ActionRec[] RecordActions
	{
		get
		{
			var arr = new ActionRec[_actions.Count];
			var i = _actions.Count - 1;
			foreach (var rec in _actions)
			{
				arr[i--] = rec;
			}
			return arr;
		}
	}
	private static DateTime _timeLastActionUsed = DateTime.Now;
	public static TimeSpan TimeSinceLastAction => DateTime.Now - _timeLastActionUsed;

	public static ActionID LastAction { get; private set; } = 0;

	public static ActionID LastGCD { get; private set; } = 0;

	public static ActionID LastAbility { get; private set; } = 0;

	internal static unsafe void AddActionRec(Action act)
	{
		var id = (ActionID)act.RowId;

		switch (act.GetActionCate())
		{
			case ActionCate.Spell:
			case ActionCate.Weaponskill:
				LastAction = LastGCD = id;
				break;
			case ActionCate.Ability:
				LastAction = LastAbility = id;
				break;
			default:
				return;
		}

		if (_actions.Count >= QUEUECAPACITY)
		{
			_ = _actions.Dequeue();
		}

		_timeLastActionUsed = DateTime.Now;
		_actions.Enqueue(new ActionRec(_timeLastActionUsed, act));
	}

	internal static void ResetAllRecords()
	{
		LastAction = 0;
		LastGCD = 0;
		LastAbility = 0;
		// Unknown, not "fight over" - see AverageTTK.
		_avgTTK = float.PositiveInfinity;
		_avgTTKCacheTick = long.MinValue;
		TimeToKillTracker.Clear();
		DotTickTracker.Clear();
		DotOnParty.Clear();
		HealFromOthers.Clear();
		ActionPenaltyWatch.Clear();
		_partyHpStatsCacheTick = long.MinValue;
		_timeLastActionUsed = DateTime.Now;
		_actions.Clear();
		DefenseHolds.Clear();

		AttackedTargets.Clear();

		// The hold's record is per fight. Carrying it across would judge one boss by another's
		// pattern - a fight where the hold was always right would keep it holding through a fight
		// where it is always wrong, and vice versa.
		_holdExpectsHitUntil = DateTime.MinValue;
		_holdsVindicated = 0;
		_holdsWasted = 0;

		while (VfxDataQueue.TryDequeue(out _))
		{ }
		lock (_tankbusterLock)
		{
			TankbusterTargetIds.Clear();
		}
		AllHostileTargets.Clear();
		AllHostileTargetIds.Clear();
		AllianceMembers.Clear();
		PartyMembers.Clear();
		AllTargets.Clear();
		TargetsByRange.Clear();
	}

	internal static void AddDamageRec(float damageRatio)
	{
		// Only what did damage: the queue keeps a fixed number of records, and every enemy action that missed the
		// player filled it with zeros - in a large party a few seconds' worth, too short for DamageTakenWithin (A340).
		if (damageRatio <= 0f)
		{
			return;
		}

		if (_damages.Count >= QUEUECAPACITY)
		{
			_ = _damages.Dequeue();
		}

		_damages.Enqueue(new DamageRec(DateTime.Now, damageRatio));
	}

	internal static DateTime KnockbackFinished { get; set; } = DateTime.MinValue;
	internal static DateTime KnockbackStart { get; set; } = DateTime.MinValue;

	#endregion

	#region Hostile Range
	public static bool HasHostilesInRange => NumberOfHostilesInRange > 0;
	public static bool HasHostilesInMaxRange => NumberOfHostilesInMaxRange > 0;
	public static int NumberOfHostilesInRange
	{
		get
		{
			var jobRange = JobRange;
			var targets = AllHostileTargets;
			var count = 0;
			for (int i = 0, n = targets.Count; i < n; i++)
			{
				if (targets[i].DistanceToPlayer() < jobRange)
				{
					count++;
				}
			}
			return count;
		}
	}
	public static int NumberOfHostilesInMaxRange
	{
		get
		{
			var targets = AllHostileTargets;
			var count = 0;
			for (int i = 0, n = targets.Count; i < n; i++)
			{
				if (targets[i].DistanceToPlayer() < 25)
				{
					count++;
				}
			}
			return count;
		}
	}
	public static int NumberOfHostilesInRangeOf(float range)
	{
		var targets = AllHostileTargets;
		var count = 0;
		for (int i = 0, n = targets.Count; i < n; i++)
		{
			if (targets[i].DistanceToPlayer() < range)
			{
				count++;
			}
		}
		return count;
	}

	public static int NumberOfPartyMembersInRangeOf(float range)
	{
		var targets = PartyMembers;
		var count = 0;
		for (int i = 0, n = targets.Count; i < n; i++)
		{
			if (targets[i].DistanceToPlayer() < range)
			{
				count++;
			}
		}
		return count;
	}
	public static int NumberOfAllHostilesInRange => NumberOfHostilesInRange;
	public static int NumberOfAllHostilesInMaxRange => NumberOfHostilesInMaxRange;
	#endregion

	#region Hostile Casting

	/// <summary>
	/// Determines whether any hostile is casting an action whose attack type is magical (<c>AttackType.RowId == 5</c>).
	/// </summary>
	public static bool IsMagicalDamageIncoming()
	{
		return AnyHostileCasting(static action => AttackTypeKind.IsMagic(action.AttackType.RowId));
	}

	/// <summary>
	/// Determines whether any hostile is casting an action whose attack type is slashing, piercing, blunt or shot
	/// (<c>AttackType</c> rows 1 to 4).
	/// </summary>
	/// <remarks>
	/// The sheet's rows, read 27.09.2026 through v2.xivapi.com: 1 slashing, 2 piercing, 3 blunt, 4 shot,
	/// 5 magic, 6 breath, 7 sound, 8 limit break. This used to test row 7 as physical, which is sound.
	/// </remarks>
	public static bool IsPhysicalDamageIncoming()
	{
		return AnyHostileCasting(static action => AttackTypeKind.IsPhysical(action.AttackType.RowId));
	}

	private static bool AnyHostileCasting(Func<Action, bool> check)
	{
		return AnyHostile(h => h != null && h.CastActionId != 0 && TryGetCastAction(h, out var action) && check(action));
	}

	/// <summary>
	/// True if any hostile in M10S is currently casting action 46553 or 46554.
	/// </summary>
	public static bool IsExtremeCastingSpecialIndicator()
	{
		return IsInM10S && AnyHostile(static h => h?.CastActionId is 46553 or 46554);
	}

	/// <summary>
	/// True if any hostile in M11S is currently casting Charybdistopia (46117).
	/// </summary>
	public static bool IsTyrantCastingSpecialIndicator()
	{
		return IsInM11S && AnyHostile(static h => h?.CastActionId == 46117);
	}

	/// <summary>
	/// True from the moment the Tyrant has cast both Scythe (46115) and Axe (46114) until its next Charybdistopia (46117) finishes.
	/// </summary>
	public static bool IsTyrantCastingSpecialIndicator2()
	{
		if (!IsInM11S || CombatTimeRaw == 0)
		{
			_hasCastScythe = false;
			_hasCastAxe = false;
			_wasCastingCharyb = false;
			_tyrantShouldStopHealing = false;
			return false;
		}

		var anyCurrentlyCastingCharyb = false;
		foreach (var hostile in AllHostileTargets)
		{
			if (hostile == null || !hostile.IsValid() || !hostile.IsCasting)
			{
				continue;
			}

			switch (hostile.CastActionId)
			{
				case 46115:
					_hasCastScythe = true;
					break;
				case 46114:
					_hasCastAxe = true;
					break;
				case 46117:
					anyCurrentlyCastingCharyb = true;
					_wasCastingCharyb = true;
					break;
			}
		}

		if (_hasCastScythe && _hasCastAxe)
		{
			_tyrantShouldStopHealing = true;
		}

		if (_wasCastingCharyb && !anyCurrentlyCastingCharyb)
		{
			_hasCastScythe = false;
			_hasCastAxe = false;
			_wasCastingCharyb = false;
			_tyrantShouldStopHealing = false;
		}

		return _tyrantShouldStopHealing;
	}

	/// <summary>
	/// True while Agrias's Judgment Blade (14423) in Orbonne resolves within 2.5 seconds.
	/// </summary>
	public static bool IsAgriasCastingSpecialIndicator()
	{
		return Orbonne && AnyHostile(static h => IsCastResolving(h, 2.5f, "Agrias", 14423));
	}

	/// <summary>
	/// True while the Lich's Alexandrian Quake (46419, 46427) in the quest battle resolves within 3 seconds.
	/// </summary>
	public static bool IsLichCastingSpecialIndicator()
	{
		return IsInQuestBattle && AnyHostile(static h => IsCastResolving(h, 3f, "Lich", 46419, 46427));
	}

	/// <summary>
	/// True while one of Lakshmi's raid-wide casts resolves within 3 seconds. In the Extreme version, The Path of Light (8539)
	/// and The Pull of Light (8543) only count while she has Chanchala.
	/// </summary>
	public static bool IsLakshmiCastingSpecialIndicator()
	{
		if (Emanation)
		{
			return AnyHostile(static h => IsCastResolving(h, 3f, "Lakshmi", 9349));
		}

		return EmanationEX && AnyHostile(static h => IsCastResolving(h, 3f, "Lakshmi", 8519, 8521, 8522, 8523)
			|| (h != null && h.HasStatus(false, StatusID.Chanchala_1410) && IsCastResolving(h, 3f, "Lakshmi", 8539, 8543)));
	}

	private static bool IsCastResolving(IBattleChara? hostile, float seconds, string source, params ReadOnlySpan<uint> castIds)
	{
		if (hostile == null || !hostile.IsCasting || !castIds.Contains(hostile.CastActionId))
		{
			return false;
		}

		var remaining = hostile.TotalCastTime - hostile.CurrentCastTime;
		if (remaining > seconds)
		{
			return false;
		}

		if (Service.Config.InDebug)
		{
			PluginLog.Debug($"{source} cast detected on obj {hostile.GameObjectId} - remaining: {remaining:F3}s, GCD remain: {DefaultGCDRemain:F3}s");
		}

		return true;
	}

	private static readonly FrozenSet<string> TankbusterPaths = FrozenSet.ToFrozenSet(
	[
		// Prefixes, matched with StartsWith: every tank marker ("tank_lockon", "tank_laser", ...) and
		// every shared tank laser. A marker the prefix catches wrongly is learned away by
		// TankbusterMarkerWatch (concept 15, V2).
		"vfx/lockon/eff/tank",
		"vfx/lockon/eff/sharelaser2tank",
		"vfx/lockon/eff/x6fe_fan100_50_0t1",     // Necron Blue Shockwave - Cone Tankbuster
		//"vfx/common/eff/mon_eisyo03t",           // M10 Deep Impact AoE TB need different path for this, this is the generic target vfx part
		"vfx/lockon/eff/m0676trg_tw_d0t1p",      // M10 Hot Impact shared TB
		"vfx/lockon/eff/m0676trg_tw_s6_d0t1p",   // M11 Raw Steel
		"vfx/lockon/eff/z6r2b3_8sec_lockon_c0a1",// Kam'lanaut Princely Blow
		"vfx/lockon/eff/m0742trg_b1t1",          // M7 Abominable Blink
		"vfx/lockon/eff/x6r9_tank_lockonae",      // M9 Hardcore Large TB
		"vfx/lockon/eff/z6r2b3_8sec_lockon_c0a1",  // Tankbuster line cleave knockback
		"vfx/lockon/eff/m0926trg_t0a1"
	], StringComparer.OrdinalIgnoreCase);

	private static readonly FrozenSet<string> MultiHitSharedPaths = FrozenSet.ToFrozenSet(
	[
		"vfx/lockon/eff/com_share4a1",
		"vfx/lockon/eff/com_share5a1",
		"vfx/lockon/eff/com_share6m7s_1v",
		"vfx/lockon/eff/com_share8s_0v",
		"vfx/lockon/eff/share_laser_5s_c0w",     // Line
		"vfx/lockon/eff/share_laser_8s_c0g",     // Line
		"vfx/lockon/eff/m0922trg_t2w"
	], StringComparer.OrdinalIgnoreCase);

	private static readonly FrozenSet<string> SharedDamagePaths = FrozenSet.ToFrozenSet(
	[
		"vfx/lockon/eff/coshare",
		"vfx/lockon/eff/share_laser",
		"vfx/lockon/eff/com_share",
		"vfx/lockon/eff/share_1",
		"vfx/lockon/eff/d1084_share_24m_s6_0k2", // San d'Oria: The Second Walk
		"vfx/lockon/eff/share_10s_6m_0w",
		"vfx/lockon/eff/share_12s_6m_t1",
		"vfx/lockon/eff/share_14s_6m_t1",
		"vfx/lockon/eff/com_trg01_0c",
		"vfx/lockon/eff/com_trg02_0c",
		"vfx/lockon/eff/com_trg01_0c",
		// M9 Penetrating Pitch stack (Lockon 656). Its spread, Aetherletting (655, x6r9_loc01_t0a1), stood here too, so a
		// spread on another member counted as a hit on the player (A394, check_marker_lists.py).
		"vfx/lockon/eff/x6r9_loc02_t0a1",
		// Duty-specific AOE share markers
		"vfx/lockon/eff/m0982trg_g0c",
		"vfx/lockon/eff/m0906_share4_7s0k2",
		"vfx/lockon/eff/x6fd_share_4m_5s_c0v", //Zelenia EX party stack
		"vfx/lockon/eff/bahamut_kakyu_target_t01i", // UCOB, party stack.
		"vfx/monster/gimmick2/eff/z3o7_b1_g06c0t", // Puppet's Bunker, Superior Flight Unit.
		"vfx/monster/gimmick4/eff/z5r1_b4_g09c0c"  // Aglaia, Nald'thal
	], StringComparer.OrdinalIgnoreCase);

	private static readonly FrozenSet<string> SpreadDamagePaths = FrozenSet.ToFrozenSet(
	[
		"vfx/lockon/eff/x6r9_loc01_t0a1",        // M9 Aetherletting spread (Lockon 655)
		// Duty-specific AOE spread markers
		"vfx/lockon/eff/x6fd_loc04m_5s1v",
		"vfx/lockon/eff/m0922tar_a0w"
	], StringComparer.OrdinalIgnoreCase);

	private static readonly StringComparison PathCmp = StringComparison.OrdinalIgnoreCase;
	internal static bool IsTrackedVfxPath(string path)
	{
		return path.StartsWith("vfx/lockon/", PathCmp) || StartsWithAny(path, SharedDamagePaths) || StartsWithAny(path, MultiHitSharedPaths)
			|| StartsWithAny(path, SpreadDamagePaths) || StartsWithAny(path, TankbusterPaths);
	}

	private static bool StartsWithAny(string path, FrozenSet<string> prefixes)
	{
		foreach (var p in prefixes)
		{
			if (path.StartsWith(p, PathCmp))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>Whether a VFX path is one of the tankbuster markers, before the learned exclusions.</summary>
	internal static bool IsTankbusterMarkerPath(string path)
	{
		foreach (var p in TankbusterPaths)
		{
			if (path.StartsWith(p, PathCmp))
			{
				return true;
			}
		}

		return false;
	}

	private static readonly ConcurrentDictionary<string, byte> _markerLocksTakenOver = new();

	/// <summary>
	/// Whether the marker entry <paramref name="key"/> is locked as "no tankbuster" in the running game version (concept
	/// 13, "Stufe 4", 4a): a lock from an earlier version, or from before stage 4, is a hypothesis and does not hold - the
	/// marker counts as a tankbuster until it goes without a hit again. Nothing holds while the version cannot be read.
	/// </summary>
	internal static bool MarkerLockHolds(string key, bool trace = true)
	{
		if (!OtherConfiguration.TankbusterMarkerWithoutHit.Contains(key))
		{
			return false;
		}

		if (OtherConfiguration.GameVersion is { } game && OtherConfiguration.TankbusterMarkerWithoutHitGame.TryGetValue(key, out var seen)
			&& seen == game)
		{
			return true;
		}

		if (trace && _markerLocksTakenOver.TryAdd(key, 0))
		{
			DefenseTrace.Line($"tankbuster marker {key}: no hit seen in an earlier game version only - counted as a tankbuster"
				+ " until it goes without a hit in this one");
		}

		return false;
	}

	private static readonly FrameCached<bool> _isHostileCastingAoe = new(() => InCombat && (IsCastingAreaVfx() || IsAnyHostileCastingArea()));
	private static readonly FrameCached<bool> _isHostileCastingToTank = new(() => InCombat && (IsCastingTankVfx() || IsAnyHostileCastingTank()));
	private static readonly FrameCached<bool> _isHostileCastingStop = new(() => InCombat && Service.Config.CastingStop && AnyHostile(IsHostileStop));
	private static readonly FrameCached<bool> _areHostilesCastingKnockback = new(IsAnyHostileCastingKnockback);

	// Over the targetable enemies; a reader that must see the untargetable helpers too walks AreaCasters (A277, A303).
	private static bool AnyHostile(Func<IBattleChara, bool> predicate)
	{
		var targets = AllHostileTargets;
		for (var i = 0; i < targets.Count; i++)
		{
			var h = targets[i];
			if (h != null && h.IsValid() && predicate(h))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether the area hit that opens the area defence reaches the player himself - the question for
	/// the actions that protect only him (concept 13, A218). The flag asks for the party; Radiant Aegis
	/// spent on a tankbuster circle around the tank protected nobody it was hitting.
	/// </summary>
	/// <remarks>
	/// Sources, read without side effects: a stack marker (a stack involves everyone who stacks), a
	/// spread marker on the player, a listed area cast within its landing window that reaches him -
	/// rated worth mitigating, or unrated - interruptible ones included under "mitigate big area casts
	/// even if interruptible", and a BossModReborn raidwide inside the mitigation window, unless its own list of
	/// whom it hits leaves him out (A273).
	/// </remarks>
	public static bool AreaHitReachesPlayer
	{
		get
		{
			if (!InCombat || Player.Object == null)
			{
				return false;
			}

			if (IsAreaVfxReachingPlayer())
			{
				return true;
			}

			// Unless the prediction names whom it hits and leaves him out (A273).
			if (BMRRaidwideHitsPlayer ?? true)
			{
				if (Service.Config.UseBmrTimeline && BMRNextRaidwideIn <= Service.Config.BMRRaidwideMitWindow
					&& PressReachesBeforeSnapshot(BMRNextRaidwideIn))
				{
					return true;
				}
			}

			var targets = AreaCasters;
			if (targets == null)
			{
				return false;
			}

			var actionSheet = Service.GetSheet<Action>();
			for (var i = 0; i < targets.Count; i++)
			{
				var h = targets[i];
				if (h == null || !h.IsValid() || !h.IsCasting)
				{
					continue;
				}

				// The same recognition the flag uses, asked of the player instead of the party.
				if (IsHostileCastingBase(h, act => OtherConfiguration.HostileCastingArea.Contains(act.RowId)
					&& AreaCastCanReachPlayer(h, act)
					&& AreaCastIsWorthMitigating(act.RowId, h)))
				{
					return true;
				}

				// The large interruptible cast, as IsHostileCastingLargeArea recognises it, without its
				// recording.
				if (!h.IsCastInterruptible || !Service.Config.MitigateBigAreaCastsEvenIfInterruptible
					|| !OtherConfiguration.HostileCastingArea.Contains(h.CastActionId)
					|| !AreaShareNow(h.CastActionId, h, out var share)
					|| share < LargeShieldShare)
				{
					continue;
				}

				var remaining = h.TotalCastTime - h.CurrentCastTime;
				if (remaining <= 0f || remaining > GCDTime(1))
				{
					continue;
				}

				var action = actionSheet.GetRowOrDefault(h.CastActionId) ?? default;
				if (action.RowId != 0 && AreaCastCanReachPlayer(h, action))
				{
					return true;
				}
			}

			return false;
		}
	}

	public static bool IsHostileCastingStopBase(IBattleChara h, Func<Action, bool> check)
	{
		if (h == null || !h.IsValid() || check == null || h.GameObjectId == 0)
		{
			return false;
		}

		if (!h.IsCasting || h.IsCastInterruptible)
		{
			return false;
		}

		if ((h.TotalCastTime - h.CurrentCastTime) > (Service.Config.CastingStopCalculate ? 100 : Service.Config.CastingStopTime))
		{
			return false;
		}

		return TryGetCastAction(h, out var action) && check(action);
	}

	private static readonly FrameCached<bool> _keptChannelRunning = new(ReadKeptChannel);

	private static readonly DateTime[] _channelSince = new DateTime[KeptChannels.Length];
	private static readonly DateTime[] _channelSeen = new DateTime[KeptChannels.Length];

	/// <summary>
	/// Every area action for which <see cref="AreaCastIsWorthMitigating"/> has withheld the party
	/// mitigation, and when it last did so. This is the probe for that decision, and the domain
	/// requires one: whether a rule that fires in combat fires at all cannot be settled by reading it.
	/// </summary>
	/// <remarks>
	/// It records actions and not calls on purpose. The predicate is asked once per casting enemy per
	/// frame, so a counter would report the frame rate rather than the number of hits let through -
	/// a surrogate for the effect instead of the effect. Writing the id is idempotent, so the same
	/// cast seen sixty times a second leaves one entry, and the count answers the question that
	/// matters: for how many of the rated actions has this rule actually saved a cooldown.
	///
	/// Diagnostic only, so it is deliberately not persisted: after a restart the honest answer is
	/// "not yet seen this session". The size is bounded by the number of rated actions.
	/// </remarks>
	public static readonly ConcurrentDictionary<uint, DateTime> AreaMitigationSkipped = new();

	/// <summary>
	/// Every area action the pre-heal rule has raised the healing flag for, and when it last did.
	/// </summary>
	/// <remarks>
	/// The counterpart probe to <see cref="AreaMitigationSkipped"/>, and it exists for a stated
	/// reason: the owner tests changes by switching the new option ON, so a rule that cannot be told
	/// apart from its own absence is not testable. Healing that arrives before a raidwide looks in
	/// the fight exactly like healing that arrives for any other reason.
	///
	/// Actions, not calls - the question is asked every frame, so a counter would report the frame
	/// rate. Diagnostic only and not persisted.
	/// </remarks>
	public static readonly ConcurrentDictionary<uint, DateTime> HealedAheadOfAreaCast = new();

	/// <summary>
	/// Every interruptible area action the defence was raised for because its measured share was
	/// large, and when it last happened. Same purpose as the two above.
	/// </summary>
	public static readonly ConcurrentDictionary<uint, DateTime> MitigatedInterruptibleCast = new();

	// The hold's own prediction, and whether the fight bore it out. Holding back says: "the event
	// this prediction points at is not the small cast now running - something larger follows". That
	// is a claim about the next few seconds, and the next few seconds answer it.
	//
	// This is the difference between a counter and a probe that decides. A counter would have to be
	// read by somebody and reported back, which costs a fight, a reading and a round per figure -
	// the owner's standing objection. This one scores itself: every hold is checked against what
	// actually landed, and the rule stops holding once its own record says it is wrong.
	private static DateTime _holdExpectsHitUntil = DateTime.MinValue;

	/// <summary>
	/// Every small area action a predicted mitigation was held back for, and when it last happened.
	/// Same purpose as the two above: a mitigation that waits looks exactly like one that never
	/// triggered, so without this the rule cannot be tested by switching it on.
	/// </summary>
	public static readonly ConcurrentDictionary<uint, DateTime> ProactiveMitigationHeld = new();

	private static DateTime _announcedAreaShareTime = DateTime.MinValue;

	/// <summary>
	/// Why the last enemy cast that damaged the player was or was not rated for the learned damage
	/// table, with its time and action id. Written by the effect handler; shown in the AoE list.
	/// </summary>
	/// <remarks>
	/// Casts only. Instant hits - auto-attacks above all - are never rated, and while they were
	/// reported here the next swing replaced a raidwide's reason within a second, so the line could
	/// only ever show why an auto-attack was not measured.
	/// </remarks>
	public static string AreaMeasurementLastOutcome { get; private set; } = "no enemy cast has hit the player yet";

	private static readonly Dictionary<string, int> _areaMeasurementTally = [];

	/// <summary>The first error the enemy effect handler raised since loading, for the windows.</summary>
	public static string EffectHandlerFirstError { get; internal set; } = string.Empty;

	/// <summary>
	/// Every strategic hold of a defense that was asked (CustomRotation_DefenseHold), by rule: whether
	/// it last held or yielded to a member in danger, why, and when. One entry per rule, so a rule
	/// asked after another in the same frame does not hide it. Shown in the diagnostics window,
	/// because a defense that waits cannot otherwise be told apart from one that was never asked.
	/// </summary>
	internal static readonly ConcurrentDictionary<string, DefenseHoldDecision> DefenseHolds = new();

	private static bool HitDropsBelow(float share, float threshold, bool unprotectedOnly, bool needsRoom, out string who)
	{
		who = string.Empty;
		if (share <= 0f)
		{
			return false;
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || member.IsDead || member.MaxHp == 0
				|| (unprotectedOnly && !member.IsUnprotectedForHealing())
				|| (needsRoom && member.CurrentHp >= member.MaxHp))
			{
				continue;
			}

			// The unprotected reading is the critical class's own footing (IsInCriticalClass): the
			// forecast effective health, at or below the threshold. The other keeps its original form
			// for its original readers.
			if (unprotectedOnly
				? member.GetForecastEffectiveHp() / (float)member.MaxHp - share <= threshold
				: member.GetEffectiveHp() / (float)member.MaxHp - share < threshold)
			{
				who = member.Name.TextValue;
				return true;
			}
		}

		return false;
	}

	// Kept separate from the verdict below, and called before it, for two reasons. The figure
	// describes the incoming cast, not this rule's opinion of it - and a second reader, the healing
	// flag, needs it whether or not the owner wants small casts mitigated. Recorded inside the
	// verdict it sat behind SkipMitigationForSmallAreaCasts, which would have left that reader blind
	// to every cast whenever the switch was off.
	private static void RecordAnnouncedAreaShare(uint actionId, IBattleChara caster)
	{
		// A rating of 0 is a landing measured without damage (A366), and announces exactly that.
		if (AreaShareNow(actionId, caster, out var share))
		{
			_announcedAreaShare = share;
			_announcedAreaShareConfirmed = OtherConfiguration.AreaShareConfirmed(actionId);
			_announcedAreaShareTime = DateTime.Now;
			_lastAnnouncedAreaAction = actionId;
		}
	}

	/// <summary>
	/// Whether the last landing of each listed area cast reached the player, for this session. Written
	/// by the effect handler, read by <see cref="IsAnyHostileCastingAreaReachingMe"/> under
	/// "Skip area defence for casts that missed you" (A205).
	/// </summary>
	internal static readonly ConcurrentDictionary<uint, bool> AreaCastReachedPlayer = new();

	/// <summary>
	/// A731 (concept 13, "Wen die Form erreicht"): the angle of a cone and the inner radius of a ring as the action's omen
	/// names them - "gl_fan060_1bf" a cone of 60 degrees, "gl_sircle_2010bf" a ring with the figures 20 and 10, as
	/// BossModReborn's analysis reads them. The ring's figures hold as the inner radius itself or as its share of the sheet's
	/// range, unevenly even for one omen; the smaller is taken, so more count as hit. 0 for what the omen does not name.
	/// </summary>
	internal static (float Degrees, float Inner) OmenShape(Action act)
	{
		return _omenShapes.GetOrAdd(act.RowId, _ =>
		{
			var path = act.Omen.ValueNullable?.Path.ExtractText() ?? string.Empty;
			var degrees = DigitsAfter(path, "fan", out var angle) ? angle : 0;
			var inner = 0f;
			foreach (var key in RingKeys)
			{
				if (DigitsAfter(path, key, out var figures) && figures > 0)
				{
					var digits = figures.ToString(System.Globalization.CultureInfo.InvariantCulture);
					var half = digits.Length / 2;
					if (half > 0 && int.TryParse(digits.AsSpan(0, digits.Length - half), out var outer) && int.TryParse(digits.AsSpan(digits.Length - half), out var innerFigure))
					{
						inner = outer > 0 ? Math.Min(innerFigure, act.EffectRange * innerFigure / (float)outer) : innerFigure;
					}

					break;
				}
			}

			return (degrees, inner);
		});
	}

	// A731: the omen names in which a ring's figures follow, as BossModReborn's analysis reads them.
	private static readonly string[] RingKeys = ["sircle_", "sicle_", "circle"];

	// The number that follows <key> in <path>, digits only.
	private static bool DigitsAfter(string path, string key, out int number)
	{
		number = 0;
		var start = path.IndexOf(key, StringComparison.Ordinal);
		if (start < 0)
		{
			return false;
		}

		start += key.Length;
		var end = start;
		while (end < path.Length && char.IsAsciiDigit(path[end]))
		{
			end++;
		}

		return end > start && int.TryParse(path.AsSpan(start, end - start), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out number);
	}

	private static float HorizontalDistance(Vector3 a, Vector3 b)
	{
		var dx = a.X - b.X;
		var dz = a.Z - b.Z;
		return MathF.Sqrt((dx * dx) + (dz * dz));
	}

	/// <summary>
	/// Every source that can open the player's defence, as it stands now, for the defence trace.
	/// Reads the same predicates the flags are built from; decides nothing.
	/// </summary>
	internal static string DescribeDefenseSources()
	{
		var parts = new List<string> { $"flags {MergedStatus & (AutoStatus.DefenseArea | AutoStatus.DefenseSingle)}" };
		var player = Player.Object;
		try
		{
			foreach (var vfx in VfxDataQueue)
			{
				if (string.IsNullOrEmpty(vfx.Path))
				{
					continue;
				}

				var kind = IsTankbusterMarkerPath(vfx.Path)
					? IsFalsifiedTankbusterMarker(vfx.Path) ? "tankbuster marker (falsified)" : "tankbuster marker"
					: StartsWithAny(vfx.Path, MultiHitSharedPaths) || StartsWithAny(vfx.Path, SharedDamagePaths) ? "stack marker"
					: StartsWithAny(vfx.Path, SpreadDamagePaths) ? "spread marker"
					: null;
				if (kind == null)
				{
					continue;
				}

				var marked = Svc.Objects.SearchById(vfx.ObjectId) as IBattleChara;
				var onPlayer = player != null && vfx.ObjectId == player.GameObjectId;
				parts.Add($"{kind} {vfx.Path} on {(onPlayer ? "you" : marked?.Name.TextValue ?? "?")}"
					+ (onPlayer || marked == null || player == null ? string.Empty : $" {HorizontalDistance(player.Position, marked.Position):F1} y from you"));
			}

			var sheet = Service.GetSheet<Action>();
			var isTank = Role == JobRole.Tank;
			foreach (var h in AreaCasters)
			{
				if (h == null || !h.IsValid() || !h.IsCasting)
				{
					continue;
				}

				var id = h.CastActionId;
				var listedArea = OtherConfiguration.HostileCastingArea.Contains(id);
				var listedTank = OtherConfiguration.HostileCastingTank.Contains(id);

				// A tank also reads an unlisted cast an enemy aims at the one it is attacking as a
				// tankbuster; that source opened his single-target defence without showing here.
				if (!listedArea && !listedTank)
				{
					if (isTank && IsHostileCastingTank(h))
					{
						var aimed = Svc.Objects.SearchById(h.CastTargetObjectId);
						parts.Add($"cast {sheet?.GetRowOrDefault(id)?.Name.ExtractText() ?? "?"} #{id} by {h.Name.TextValue} at "
							+ $"{(player != null && h.CastTargetObjectId == player.GameObjectId ? "you" : aimed?.Name.TextValue ?? "?")}"
							+ $" (unlisted, cast at its target) {h.TotalCastTime - h.CurrentCastTime:F1} s left");
					}

					continue;
				}

				var act = sheet?.GetRowOrDefault(id);
				var target = Svc.Objects.SearchById(h.CastTargetObjectId);
				var text = $"cast {act?.Name.ExtractText() ?? "?"} #{id} by {h.Name.TextValue}"
					+ $"{(AreaCastHelperIds.Contains(h.GameObjectId) ? " (untargetable)" : string.Empty)} at {target?.Name.TextValue ?? "?"}"
					+ $" ({(listedArea ? "area list" : string.Empty)}{(listedArea && listedTank ? ", " : string.Empty)}{(listedTank ? "tankbuster list" : string.Empty)})"
					+ $" {h.TotalCastTime - h.CurrentCastTime:F1} s left";
				if (act is { } a)
				{
					text += $", type {a.CastType} range {a.Range} effect {a.EffectRange} width {a.XAxisModifier}"
						+ $", caster {h.DistanceToPlayer():F1} y from you";
					// Only side-effect-free reads here: the worth check and the large-cast check record
					// what they decide, and the trace must not make decisions the flags did not make.
					if (listedArea)
					{
						text += $", reaches you {AreaCastCanReachPlayer(h, a)}, reaches the party {AreaCastReachesParty(h, a)}, interruptible {h.IsCastInterruptible}"
							+ $", rated {(OtherConfiguration.HostileCastingAreaPotential.TryGetValue(id, out var share) ? share.ToString("P0") + " unmitigated" : "no")}"
							+ $", last landing reached you {(AreaCastReachedPlayer.TryGetValue(id, out var reachedLast) ? reachedLast.ToString() : "not seen")}";
					}
				}

				parts.Add(text);
			}
		}
		catch (Exception ex)
		{
			parts.Add($"scan failed: {ex.Message}");
		}

		parts.Add($"area hit reaches you {AreaHitReachesPlayer}");
		parts.Add($"single hit reaches you {SingleHitReachesPlayer}");

		if (TankPullOnPlayer)
		{
			parts.Add($"pull on you: at least {Service.Config.AutoDefenseNumber} enemies within reach attacking you");
		}

		if (UnhealedUnderAttack)
		{
			var due = TankbusterForecast.NextOnPlayerIn;
			parts.Add("no healer alive, an enemy attacks you: mitigation that is back before the next tankbuster"
				+ (float.IsPositiveInfinity(due) ? " (none due)" : $" (due in {due:F1} s)"));
		}

		if (BMRNextRaidwideIn is > 0f and < float.MaxValue)
		{
			parts.Add($"BMR raidwide in {BMRNextRaidwideIn:F1} s (window {Service.Config.BMRRaidwideMitWindow:F1},"
				+ $" {BMRActiveModuleName ?? "no module"} by {BMRNextRaidwideSource}, hits you {BMRRaidwideHitsPlayer?.ToString() ?? "unknown"})");
		}

		if (BMRNextTankbusterIn is > 0f and < float.MaxValue)
		{
			parts.Add($"BMR tankbuster in {BMRNextTankbusterIn:F1} s (window {Service.Config.BMRTankbusterMitWindow:F1}), party tank {PartyTank?.Name.TextValue ?? "none"}"
				+ $", hits you {BMRTankbusterHitsPlayer?.ToString() ?? "unknown"}");
		}

		if (IsTankbusterVfxOnPlayer())
		{
			parts.Add("tankbuster marker on you");
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			if (party[i] is { } member && TankbusterHelp.AimedHitCallsForDefence(member, out var aimed))
			{
				parts.Add(aimed);
			}
		}

		return string.Join(" | ", parts);
	}

	// Whether a marker sits on the player or a member of his own party, Duty Support companions
	// included whatever the NPC party setting says: they share the stack as a player would.
	private static bool IsOwnPartyMarker(ulong objectId)
	{
		var player = Player.Object;
		if (player != null && objectId == player.GameObjectId)
		{
			return true;
		}

		return Svc.Objects.SearchById(objectId) is IBattleChara marked
			&& (marked.IsParty() || marked.IsNpcPartyMember());
	}

	private static bool IsAnyHostileCastingKnockback()
	{
		// Over the helpers too (A303): a listed knockback resolved by an untargetable helper is the knockback
		// "Automatically use Anti-Knockback role actions" promises to answer from the list. Only one that reaches the
		// player (A701, concept 13): the knockback is an effect on whom the action hits; Ground and Pound in a line he
		// did not stand in took Surecast for 120 s.
		KnockbackReachingPlayer = null;
		var targets = AreaCasters;
		for (var i = 0; i < targets.Count; i++)
		{
			var h = targets[i];
			if (IsHostileCastingKnockback(h) && TryGetCastAction(h, out var act) && AreaCastCanReachPlayer(h, act))
			{
				KnockbackReachingPlayer = (h, act);
				return true;
			}
		}

		return false;
	}

	public static bool IsHostileCastingKnockback(IBattleChara h)
	{
		return IsHostileCastingBase(h, static act => OtherConfiguration.HostileCastingKnockback.Contains(act.RowId));
	}

	public static bool IsHostileCastingBase(IBattleChara h, Func<Action, bool> check)
	{
		if (h == null || check == null || h.GameObjectId == 0)
		{
			return false;
		}

		unsafe
		{
			if (h.Struct() == null)
			{
				return false;
			}
		}

		if (!IsAreaCastEnemy(h) || !h.IsCasting || h.IsCastInterruptible)
		{
			return false;
		}

		var t = h.TotalCastTime - h.CurrentCastTime - DefaultGCDTotal;
		if (!(h.TotalCastTime > DefaultGCDTotal && t > 0 && t < GCDTime(1)))
		{
			return false;
		}

		return TryGetCastAction(h, out var action) && check(action);
	}
	#endregion

	#region BossModReborn Timeline Integration
	private static readonly FrameCached<bool> _bmrEnabled = new(() => IsPluginLoaded("BossModReborn", "BossModReborn"), 1000);
	private static readonly FrameCached<bool> _autoPvpSeriesGrindEnabled = new(() => IsPluginLoaded("Auto PVP Series Grind", "AutoPvpSeriesGrind"), 1000);

	public static bool BMREnabled => _bmrEnabled.Value;

	public static bool AutoPvpSeriesGrindEnabled => _autoPvpSeriesGrindEnabled.Value;

	private static bool IsPluginLoaded(string name, string internalName)
	{
		foreach (var x in Svc.PluginInterface.InstalledPlugins)
		{
			if ((x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || x.InternalName.Equals(internalName, StringComparison.OrdinalIgnoreCase)) && x.IsLoaded)
			{
				return true;
			}
		}

		return false;
	}
	public static bool PvPAutomationBlocked => IsPvP && AutoPvpSeriesGrindEnabled;

	public static bool BMRHasActiveModule { get; set; }
	public static string? BMRActiveModuleName { get; set; }
	public static float BMRNextRaidwideIn { get; set; } = float.MaxValue;

	/// <summary>
	/// Which BossModReborn endpoint gave <see cref="BMRNextRaidwideIn"/>: "timeline", "hints" or "generic".
	/// For the trace: only "hints" carries a list of whom the raidwide hits.
	/// </summary>
	public static string BMRNextRaidwideSource { get; set; } = string.Empty;

	private static List<BmrRaidwideCast> _bmrRaidwideCasts = [];
	private static DateTime _bmrRaidwideLandsAt = DateTime.MinValue;
	private static DateTime _bmrRaidwideReadAt = DateTime.MinValue;
	private static TimeSpan _bmrRaidwideReadFrame = TimeSpan.Zero;

	private static DateTime _bmrRaidwideHeldTracedFor = DateTime.MinValue;

	public static float BMRNextTankbusterIn { get; set; } = float.MaxValue;
	public static float BMRNextKnockbackIn { get; set; } = float.MaxValue;
	public static float BMRNextDowntimeIn { get; set; } = float.MaxValue;
	public static float BMRNextDowntimeEndIn { get; set; } = float.MaxValue;
	public static float BMRNextVulnerableIn { get; set; } = float.MaxValue;
	public static float BMRNextVulnerableEndIn { get; set; } = float.MaxValue;
	public static float BMRNextDamageIn { get; set; } = float.MaxValue;
	public static PredictedDamageType BMRNextDamageType { get; set; } = PredictedDamageType.None;

	public static float BMRSpecialModeIn { get; set; } = float.MaxValue;
	public static SpecialMode BMRSpecialModeType { get; set; } = SpecialMode.Normal;

	/// <summary>
	/// True if BossMod's boss module AI hints are requesting the current cast be cancelled.
	/// </summary>
	public static bool BMRForceCancelCast { get; set; }

	/// <summary>
	/// True if BossMod's AI controller is requesting the current cast be cancelled.
	/// </summary>
	public static bool BMRForceCancelCastAI { get; set; }

	/// <summary>
	/// The longest cast BossMod expects to finish before its AI or a mechanic moves the player (<c>Hints.MaxCastTime</c>);
	/// float.MaxValue without BossMod. A longer cast is interrupted (A628).
	/// </summary>
	public static float BMRMaxCastTime { get; set; } = float.MaxValue;

	/// <summary>
	/// True while BossMod wants a movement of the player or holds casts (<c>Movement.IsMoving</c>:
	/// <c>hints.ForcedMovement != null || hints.HoldCasts</c>, BossModReborn 2087153c2) - also while its AI does not walk.
	/// Whether the AI steers the player is <see cref="BMRIsNavigating"/> (A725).
	/// </summary>
	public static bool BMRIsMoving { get; set; }

	/// <summary>
	/// Seconds until the earliest of BossMod's forbidden zones turns active, wherever it lies
	/// (<c>Hints.ForbiddenZonesNextActivation</c>); float.MaxValue without one.
	/// </summary>
	public static float BMRForbiddenZonesNextActivation { get; set; } = float.MaxValue;
	public static float BMRDebugTimelineRaidwide { get; set; } = float.MaxValue;
	public static float BMRDebugTimelineTankbuster { get; set; } = float.MaxValue;
	public static float BMRDebugHintsRaidwide { get; set; } = float.MaxValue;
	public static float BMRDebugHintsTankbuster { get; set; } = float.MaxValue;
	public static float BMRDebugGenericDamageIn { get; set; } = float.MaxValue;
	public static int BMRDebugGenericDamageType { get; set; }
	public static bool BMRDebugTimelineRwFunc { get; set; }
	public static bool BMRDebugTimelineTbFunc { get; set; }
	public static bool BMRDebugHintsRwFunc { get; set; }
	public static bool BMRDebugHintsTbFunc { get; set; }

	/// <summary>
	/// Delegate wired up by BossModUpdater to the <c>Hints.IsPositionSafe</c> IPC endpoint.
	/// When null, BossModReborn is not available and all positions are considered safe.
	/// </summary>
	public static Func<Vector3, bool>? BMRIsPositionSafe { get; set; }

	/// <summary>
	/// Delegate wired up by BossModUpdater to the <c>Hints.IsDashSafe</c> IPC endpoint.
	/// When null, BossModReborn is not available and all dashes are considered safe.
	/// </summary>
	public static Func<Vector3, Vector3, bool>? BMRIsDashSafe { get; set; }

	/// <summary>
	/// Delegate wired up by BossModUpdater for a dash whose landing point the game fixes - a fixed distance along the
	/// facing, or a backstep from the target - with the landing point RSR computes. It asks BossModReborn's
	/// <c>Hints.IsDashSafe</c>: its own <c>Hints.IsFixedDashSafe</c> takes range and direction, not two points (A313).
	/// When null, BossModReborn is not available and all fixed dashes are considered safe.
	/// </summary>
	public static Func<Vector3, Vector3, bool>? BMRIsFixedDashSafe { get; set; }

	/// <summary>
	/// The most recently polled set of upcoming planned actions from BossMod's Cooldown Planner,
	/// wired up by BMRPlanUpdater. Empty when no plan is active or BossModReborn is unavailable.
	/// </summary>
	public static List<BMRPlannedAction> BMRPlannedActions { get; set; } = [];

	/// <summary>
	/// Returns the currently active planned action (if any) matching the given adjusted action id,
	/// i.e. one whose activation window has started (ActivationIn &lt;= 0) and not yet ended
	/// (WindowEndIn &gt; 0). Only entries resolved to a concrete game action (ActionType == 1) are considered.
	/// </summary>
	public static BMRPlannedAction? GetActivePlannedAction(uint actionId)
	{
		var actions = BMRPlannedActions;
		for (var i = 0; i < actions.Count; i++)
		{
			var action = actions[i];
			if (action.ActionId == actionId && action.ActivationIn <= 0f && action.WindowEndIn > 0f)
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"GetActivePlannedAction: Found active planned action {action.ActionId} with ActivationIn {action.ActivationIn} and WindowEndIn {action.WindowEndIn}");
				}

				return action;
			}
		}

		return null;
	}

	/// <summary>
	/// Returns true if the destination is safe to move to, or if BossModReborn IPC is unavailable.
	/// </summary>
	public static bool IsMovementDestinationSafe(Vector3 destination)
	{
		if (BMRIsPositionSafe == null)
		{
			return true;
		}

		try
		{ return BMRIsPositionSafe(destination); }
		catch { return true; }
	}

	/// <summary>
	/// Returns true if the dash from <paramref name="from"/> to <paramref name="to"/> is safe,
	/// or if BossModReborn IPC is unavailable.
	/// </summary>
	public static bool IsDashSafe(Vector3 from, Vector3 to)
	{
		if (BMRIsDashSafe == null)
		{
			return true;
		}

		try
		{ return BMRIsDashSafe(from, to); }
		catch { return true; }
	}

	/// <summary>
	/// Returns true if a fixed-distance dash (game-determined destination) from <paramref name="from"/> to
	/// <paramref name="to"/> is safe, or if BossModReborn IPC is unavailable.
	/// For FixedDistanceMoveForward, FixedDistanceMoveBackward
	/// </summary>
	public static bool IsFixedDashSafe(Vector3 from, Vector3 to)
	{
		if (BMRIsFixedDashSafe == null)
		{
			return true;
		}

		try
		{ return BMRIsFixedDashSafe(from, to); }
		catch { return true; }
	}
	public static void ResetBmrData()
	{
		BMRHasActiveModule = false;
		BMRActiveModuleName = null;
		BMRNextRaidwideIn = float.MaxValue;
		BMRNextRaidwideSource = string.Empty;
		BMRRaidwideHitsPlayer = null;
		_bmrRaidwideCasts = [];
		_bmrRaidwideLandsAt = DateTime.MinValue;
		BMRNextTankbusterIn = float.MaxValue;
		BMRNextKnockbackIn = float.MaxValue;
		BMRNextDowntimeIn = float.MaxValue;
		BMRNextDowntimeEndIn = float.MaxValue;
		BMRNextVulnerableIn = float.MaxValue;
		BMRNextVulnerableEndIn = float.MaxValue;
		BMRNextDamageIn = float.MaxValue;
		BMRNextDamageType = PredictedDamageType.None;
		BMRNextDamagePlayers = 0;
		BMRNextDamageHitsPlayer = false;
		BMRSpecialModeIn = float.MaxValue;
		BMRSpecialModeType = SpecialMode.Normal;
		BMRForceCancelCast = false;
		BMRForceCancelCastAI = false;
		BMRMaxCastTime = float.MaxValue;
		BMRMoveRequested = false;
		BMRIsMoving = false;
		BMRIsNavigating = false;
		BMRNaviTargetPos = null;
		BMRPlayerSpeed = 0f;
		BMRForbiddenZonesNextActivation = float.MaxValue;
		BMRDebugTimelineRaidwide = float.MaxValue;
		BMRDebugTimelineTankbuster = float.MaxValue;
		BMRDebugHintsRaidwide = float.MaxValue;
		BMRDebugHintsTankbuster = float.MaxValue;
		BMRDebugGenericDamageIn = float.MaxValue;
		BMRDebugGenericDamageType = 0;
		BMRDebugTimelineRwFunc = false;
		BMRDebugTimelineTbFunc = false;
		BMRDebugHintsRwFunc = false;
		BMRDebugHintsTbFunc = false;
		BMRIsPositionSafe = null;
		BMRIsDashSafe = null;
		BMRIsFixedDashSafe = null;
	}

	/// <summary>
	/// Clears any cached Cooldown Planner data (called when the feature is disabled or BMR becomes unavailable).
	/// </summary>
	public static void ResetBmrPlanData()
	{
		BMRPlannedActions = [];
	}
	#endregion
}