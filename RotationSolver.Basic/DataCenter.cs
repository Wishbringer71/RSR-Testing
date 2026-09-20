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
using Lumina.Excel;
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

internal static class DataCenter
{
	public static List<IBattleChara> PartyMembers { get; set; } = [];

	/// <summary>
	/// The party's first living tank, or null if there is none.
	/// </summary>
	public static IBattleChara? PartyTank
	{
		get
		{
			foreach (var member in PartyMembers)
			{
				if (member.IsJobCategory(JobRole.Tank) && !member.IsDead)
				{
					return member;
				}
			}
			return null;
		}
	}

	public static List<IBattleChara> AllianceMembers { get; set; } = [];

	public static List<IBattleChara> AllHostileTargets { get; set; } = [];

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

	public static IBattleChara? InterruptTarget { get; set; }

	public static IBattleChara? ProvokeTarget { get; set; }

	public static IBattleChara? DeathTarget { get; set; }

	public static IBattleChara? DispelTarget { get; set; }

	public static List<IBattleChara> AllTargets { get; set; } = [];
	public static Dictionary<float, List<IBattleChara>> TargetsByRange { get; set; } = [];

	/// <summary>
	/// The action most recently queued via interception (current).
	/// Set by the interception logic when an action is queued for RSR to attempt.
	/// </summary>
	public static IAction? CurrentInterceptedAction { get; set; }

	public static bool IsInDutyReplay()
	{
		if (!PlayerAvailable())
		{
			return false;
		}

		return Svc.Condition[ConditionFlag.DutyRecorderPlayback];
	}

	/// <summary>
	///
	/// </summary>
	public unsafe static Buddy.BuddyMember? ActivePet => *UIState.Instance()->Buddy.PetInfo.Pet;

	// XBMPet is static game data; indexed once on first use instead of scanning the whole Excel sheet on every access.
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

	/// <summary>
	///
	/// </summary>
	public static bool BMPet
	{
		get
		{
			var dataId = ActivePet?.DataId;
			return dataId > 0 && XbmPetByDataId.ContainsKey(dataId.Value);
		}
	}

	/// <summary>
	///
	/// </summary>
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

	/// <summary>
	/// Affinity of every Beastmaster pet, indexed by <see cref="BeastmasterPet"/> (which matches the XBMPet row id).
	/// A pet's Trick grants the Heart status matching its affinity, and that Heart decides which Instinctual axe
	/// follows it: Volant -> Avalanche Axe -> Rampant -> Mistral Axe -> Durant -> Spinning Axe -> Eldritch -> Gale Axe -> Volant.
	/// This kinda sucks as a way to determine a pet's affinity, but it's the only way to do it right now. TODO: Future me figure out a better way that isnt hardcoded.
	/// </summary>
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

	/// <summary>
	/// The affinity of a specific Beastmaster pet, or <see cref="BeastmasterAffinity.None"/> when the pet is unknown.
	/// </summary>
	public static BeastmasterAffinity AffinityOf(BeastmasterPet pet)
	{
		var index = (int)pet;
		return index > 0 && index < PetAffinities.Length ? PetAffinities[index] : BeastmasterAffinity.None;
	}

	/// <summary>
	/// The affinity of the currently summoned Beastmaster pet.
	/// </summary>
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

	// Tracking fields for Tyrant special sequence (Scythe/Axe -> Charybdistopia)
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
		// 0 is standard, 1 is legacy
		return Svc.GameConfig.UiControl.GetUInt(UiControlOption.MoveMode.ToString());
	}

	internal static IBattleChara? HostileTarget
	{
		get => Svc.Objects.SearchById(_hostileTargetId) as IBattleChara;
		set => _hostileTargetId = value?.GameObjectId ?? 0;
	}

	internal static List<uint> PrioritizedNameIds { get; set; } = [];
	internal static List<uint> BlacklistedNameIds { get; set; } = [];

	/// <summary>
	/// List of hostile NameIds that should be excluded as valid targets when an action opts-in via IsRestrictedDOT.
	/// </summary>
	internal static List<uint> RestrictedDotNameIds { get; set; } =
	[
		9214,
	];

	/// <summary>
	/// 
	/// </summary>
	internal static List<uint> RestrictedActionNameIds { get; set; } =
	[
		14301, 14499,
	];

	internal static ConcurrentQueue<VfxNewData> VfxDataQueue { get; } = new();

	/// <summary>
	/// Players currently targeted by tankbuster VFX markers (populated from VFX queue).
	/// </summary>
	internal static List<IBattleChara> TankbusterTargets { get; } = [];

	private static readonly Lock _tankbusterLock = new();

	/// <summary>
	/// Only recorded 15s hps.
	/// </summary>
	public const int HP_RECORD_TIME = 240;

	internal static Queue<(DateTime time, Dictionary<ulong, float> hpRatios)> RecordedHP { get; } =
		new(HP_RECORD_TIME + 1);

	public static ICustomRotation? CurrentRotation { get; internal set; }
	public static DutyRotation? CurrentDutyRotation { get; internal set; }

	public static Dictionary<string, DateTime> SystemWarnings { get; set; } = [];
	public static bool HoldingRestore = false;

	internal static bool NoPoslock => Svc.Condition[ConditionFlag.OccupiedInEvent]
									  || !Service.Config.PoslockCasting
									  //Key cancel.
									  || Svc.KeyState[Service.Config.PoslockModifier.ToVirtual()]
									  //Gamepad cancel.
									  || Svc.GamepadState.Raw(Dalamud.Game.ClientState.GamePad.GamepadButtons.R1) >=
									  0.5f
									  //Mouse cancel: holding left and right mouse buttons at the same time.
									  || (Service.Config.PosLockMouse && InputManager.IsLeftMouseDown() && InputManager.IsRightMouseDown());

	internal static DateTime EffectTime { private get; set; } = DateTime.Now;
	internal static DateTime EffectEndTime { private get; set; } = DateTime.Now;

	internal static int AttackedTargetsCount { get; set; } = 48;
	internal static Queue<(ulong id, DateTime time)> AttackedTargets { get; } = new(AttackedTargetsCount);

	internal static Queue<MacroItem> Macros { get; } = new Queue<MacroItem>();

	internal static bool InEffectTime => DateTime.Now >= EffectTime && DateTime.Now <= EffectEndTime;
	internal static Dictionary<ulong, uint> HealHP { get; set; } = [];

	// How much health one of our own healing actions actually restored, in points, per action id.
	// Healing is an absolute figure just like damage, so it is stored as points and divided by the
	// member's own maximum where it is read - the same member carries a different share of it.
	//
	// This exists because the potency in an effect text cannot be converted into points from here:
	// the result depends on healing power, on the job gauge and on buffs, and it changes with every
	// piece of gear. Asking the fight instead costs nothing - the effect handler already sees every
	// heal we land, with the real number.
	//
	// Smoothed rather than overwritten, because a critical heal restores markedly more than an
	// ordinary one and a single one of those must not move the estimate to where the next decision
	// is wrong. The weight is even: the most recent landing counts as much as everything before it,
	// so a gear change is followed within a few casts instead of being averaged away.
	private static readonly Dictionary<uint, float> _observedHealPerCast = [];

	internal static void RecordHealEffect(uint actionId, IEnumerable<uint> healedAmounts)
	{
		float sum = 0;
		var count = 0;
		foreach (var amount in healedAmounts)
		{
			// A heal that landed on a full target reports the overheal as 0 in the effect packet,
			// which would drag the estimate towards zero and never recover. Only landings that
			// actually restored something say what the action is worth.
			if (amount == 0)
			{
				continue;
			}
			sum += amount;
			count++;
		}

		if (count == 0)
		{
			return;
		}

		var perTarget = sum / count;
		_observedHealPerCast[actionId] = _observedHealPerCast.TryGetValue(actionId, out var known) && known > 0
			? (known + perTarget) / 2f
			: perTarget;
	}

	/// <summary>
	/// The healing one cast of this action was last seen to restore, in health points, or 0 when it
	/// has not been observed yet. 0 means "unknown", never "heals nothing" - a caller that cannot
	/// act on an unknown value keeps its previous behaviour instead of assuming one.
	/// </summary>
	public static float GetObservedHealPerCast(uint actionId)
	{
		return _observedHealPerCast.TryGetValue(actionId, out var known) ? known : 0f;
	}

	/// <summary>
	/// The largest amount of health missing from any living party member, in points. This is the
	/// figure a heal has to reach for none of it to be wasted on that member.
	/// </summary>
	public static float LargestMissingHp
	{
		get
		{
			float largest = 0;
			foreach (var member in PartyMembers)
			{
				if (member.IsDead || member.MaxHp == 0 || member.CurrentHp >= member.MaxHp)
				{
					continue;
				}

				var missing = (float)(member.MaxHp - member.CurrentHp);
				if (missing > largest)
				{
					largest = missing;
				}
			}
			return largest;
		}
	}

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

	internal static void AddCommandAction(IAction act, double time)
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

		NextAct newItem = new(act, DateTime.Now.AddSeconds(time));
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
	public static TargetHostileType CurrentTargetToHostileType => Service.Config.HostileType;

	public static TargetingType? TargetingTypeOverride { get; set; }

	public static TargetingType TargetingType
	{
		get
		{
			if (TargetingTypeOverride.HasValue)
			{
				return TargetingTypeOverride.Value;
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

	// Combo.Timer <= 0 means the game itself considers the combo expired/inactive.
	// In that state Combo.Action can hold a stale/unreliable value (e.g. left over from
	// unrelated action usage such as mounting), so it must not be reported as the last combo action.
	// Additionally, only Weaponskill/Spell actions can actually be part of a combo chain; other
	// categories (e.g. general/system actions like Sprint/Return, action ID 4) can end up in
	// Combo.Action without representing a real combo step, so they must be filtered out as well.
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
			var action = Svc.Data.GetExcelSheet<Action>()?.GetRowOrDefault(id);
			var cate = action?.GetActionCate() ?? ActionCate.None;
			return cate is ActionCate.Weaponskill or ActionCate.Spell
				? (ActionID)id
				: ActionID.None;
		}
	}

	public static unsafe float ComboTime => ActionManager.Instance()->Combo.Timer;

	public static bool IsMoving => Player.IsMoving || BMRIsMoving;

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

	/// <summary>
	/// Raised whenever <see cref="SpecialType"/> changes so that UI layers (e.g. RSCommands) can keep their display strings in sync.
	/// </summary>
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

	/// <summary>
	/// Is out of combat.
	/// </summary>
	public static bool NotInCombatDelay => _notInCombatDelay.Delay(!InCombat);

	internal static float CombatTimeRaw { get; set; }

	private static DateTime _startRaidTime = DateTime.MinValue;

	internal static float RaidTimeRaw
	{
		get
		{
			// If the raid start time is not set, return 0.
			if (_startRaidTime == DateTime.MinValue)
			{
				return 0;
			}

			// Calculate and return the total seconds elapsed since the raid started.
			return (float)(DateTime.Now - _startRaidTime).TotalSeconds;
		}
		set
		{
			// If the provided value is negative, reset the raid start time.
			if (value < 0)
			{
				_startRaidTime = DateTime.MinValue;
			}
			else
			{
				// Set the raid start time to the current time minus the provided value in seconds.
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
	private static long _avgTTKCacheTick = long.MinValue;
	private const long AverageTtkTtlMs = 15;

	// GetTTK() walks the RecordedHP history per hostile target, so this is cached for a
	// single frame rather than recomputed on every CanUse()/condition check that reads it.
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
			_avgTTK = count > 0 ? total / count : float.PositiveInfinity;
			_avgTTKCacheTick = now;
			return _avgTTK;
		}
	}

	#region Territory Info Tracking

	public static Data.TerritoryInfo? Territory { get; set; }
	public static uint TerritoryID => (ushort)Svc.ClientState.TerritoryType;

	public static bool IsPvP => Territory?.IsPvP ?? false;

	/// <summary>
	/// When set to <c>true</c> by an external plugin via IPC, the TargetFreely behaviour is
	/// activated for the current session without modifying the user's <c>TargetFreely</c>
	/// config value.  Reset to <c>false</c> by calling the corresponding IPC method.
	/// </summary>
	public static bool TargetFreelyOverride { get; set; }

	public static bool IsInMaskedCarnivale => Territory?.ContentType == TerritoryContentType.TheMaskedCarnivale;

	public static bool IsInDuty => Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56];

	/// <summary>
	/// True when playing a Quest Battle, where the player's normal character/actions are replaced
	/// by an NPC with its own action set (e.g. Hardboiled). These duties set <see cref="ConditionFlag.RolePlaying"/>
	/// instead of <see cref="ConditionFlag.BoundByDuty"/>, and their duty actions replace the normal
	/// hotbars rather than occupying the dedicated duty action slots.
	/// </summary>
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
	/// <summary>
	/// 
	/// </summary>
	public static unsafe ushort PlayerFateId
	{
		get
		{
			try
			{
				if ((IntPtr)FateManager.Instance() != IntPtr.Zero
					&& (IntPtr)FateManager.Instance()->CurrentFate != IntPtr.Zero
					&& DataCenter.PlayerSyncedLevel() <= FateManager.Instance()->CurrentFate->MaxLevel)
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

	/// <summary>
	/// 
	/// </summary>
	public static bool IsInFate => PlayerFateId != 0 && !IsInBozja && !IsInOccultCrescentOp;

	#endregion

	#region Treasure Hunt
	/// <summary>
	/// 
	/// </summary>
	public static bool IsInTreasureHunt => Territory?.ContentType == TerritoryContentType.TreasureHunt;

	/// <summary>
	/// 
	/// </summary>
	public static bool IsInTheAquapolis => TerritoryID == 558;

	/// <summary>
	/// The Lost Canals of Uznair
	/// </summary>
	public static bool IsInTheLostCanalsofUznair => TerritoryID == 712;

	/// <summary>
	/// The Shifting Altars of Uznair
	/// </summary>
	public static bool IsInTheShiftingAltarsofUznair => TerritoryID == 794;

	/// <summary>
	/// The Hidden Canals of Uznair
	/// </summary>
	public static bool IsInTheHiddenCanalsofUznair => TerritoryID == 725;

	/// <summary>
	/// The Dungeons of Lyhe Ghiah
	/// </summary>
	public static bool IsInTheDungeonsofLyheGhiah => TerritoryID == 879;

	/// <summary>
	/// The Shifting Oubliettes of Lyhe Ghiah
	/// </summary>
	public static bool IsInTheShiftingOubliettesofLyheGhiah => TerritoryID == 924;

	/// <summary>
	/// The Excitatron 6000
	/// </summary>
	public static bool IsInTheExcitatron6000 => TerritoryID == 1000;

	/// <summary>
	/// The Shifting Gymnasion Agonon
	/// </summary>
	public static bool IsInTheShiftingGymnasionAgonon => TerritoryID == 1123;

	/// <summary>
	/// Cenote Ja Ja Gural
	/// </summary>
	public static bool IsInCenoteJaJaGural => TerritoryID == 1209;

	/// <summary>
	/// Vault Oneiron
	/// </summary>
	public static bool IsInVaultOneiron => TerritoryID == 1279;
	#endregion

	#region Bozja
	/// <summary>
	/// Determines if the current content is Bozjan Southern Front or Zadnor.
	/// </summary>
	public static bool IsInBozjanFieldOp => Content.ContentType == ECommons.GameHelpers.ContentType.FieldOperations
		&& Territory?.ContentType == TerritoryContentType.SaveTheQueen;

	/// <summary>
	/// Determines if the current content is Bozjan Southern Front CE or Zadnor CE.
	/// </summary>
	public static bool IsInBozjanFieldOpCE => IsInBozjanFieldOp
		&& StatusHelper.PlayerHasStatus(false, StatusID.DutiesAsAssigned);

	/// <summary>
	/// Determines if the current content is Delubrum Reginae.
	/// </summary>
	public static bool IsInDelubrumNormal => Content.ContentType == ECommons.GameHelpers.ContentType.FieldRaid
		&& Territory?.ContentType == TerritoryContentType.SaveTheQueen;

	/// <summary>
	/// Determines if the current content is Delubrum Reginae (Savage).
	/// </summary>
	public static bool IsInDelubrumSavage => Content.ContentType == ECommons.GameHelpers.ContentType.FieldRaid
		&& Content.ContentDifficulty == ContentDifficulty.FieldRaidsSavage
		&& Territory?.ContentType == TerritoryContentType.SaveTheQueen;

	/// <summary>
	/// Determines if the current territory is Bozja and is either a field operation or field raid.
	/// </summary>
	public static bool IsInBozja => IsInBozjanFieldOp || IsInDelubrumNormal || IsInDelubrumSavage;
	#endregion

	/// <summary>
	///
	/// </summary>
	public static bool IsInFieldOperations => Content.ContentType == ECommons.GameHelpers.ContentType.FieldOperations;

	/// <summary>
	///
	/// </summary>
	public static bool IsInFieldRaid => Content.ContentType == ECommons.GameHelpers.ContentType.FieldRaid;

	#region Occult Crescent
	/// <summary>
	/// Determines if the current content is Occult Crescent.
	/// </summary>
	public static bool IsInOccultCrescentOp => Territory?.ContentType == TerritoryContentType.OccultCrescent;

	/// <summary>
	///
	/// </summary>
	public static bool IsInNorthHorn => IsInOccultCrescentOp && TerritoryID == 1346;

	/// <summary>
	///
	/// </summary>
	public static bool IsInSouthHorn => IsInOccultCrescentOp && TerritoryID == 1252;

	/// <summary>
	/// Determines if the current content is Forked Tower.
	/// </summary>
	public static bool IsInForkedTowerBlood => IsInOccultCrescentOp && StatusHelper.PlayerHasStatus(false, StatusID.DutiesAsAssigned_4228);
	#endregion

	#region Variant Dungeon
	/// <summary>
	/// 
	/// </summary>
	public static bool TheMerchantsTaleAdvanced => IsInTerritory(1316);

	/// <summary>
	/// 
	/// </summary>
	public static bool TheMerchantsTale => IsInTerritory(1315);

	/// <summary>
	/// 
	/// </summary>
	public static bool SildihnSubterrane => IsInTerritory(1069);

	/// <summary>
	/// 
	/// </summary>
	public static bool MountRokkon => IsInTerritory(1137);

	/// <summary>
	/// 
	/// </summary>
	public static bool AloaloIsland => IsInTerritory(1176);

	/// <summary>
	/// 
	/// </summary>
	public static bool InVariantDungeon => TheMerchantsTaleAdvanced || TheMerchantsTale || AloaloIsland || MountRokkon || SildihnSubterrane;
	#endregion

	#region Misc Duty Info

	/// <summary>
	///
	/// </summary>
	public static bool IsInEurekaFieldOp => Territory?.ContentType == TerritoryContentType.Eureka;

	/// <summary>
	///
	/// </summary>
	public static bool IsInDeepDungeons => Territory?.ContentType == TerritoryContentType.DeepDungeons;

	/// <summary>
	/// 
	/// </summary>
	public static bool IsInPilgrimsTraverse => IsInTerritory(1281)
	|| IsInTerritory(1282) || IsInTerritory(1283)
	|| IsInTerritory(1284) || IsInTerritory(1285)
	|| IsInTerritory(1286) || IsInTerritory(1287)
	|| IsInTerritory(1288) || IsInTerritory(1289)
	|| IsInTerritory(1290);

	/// <summary>
	/// 
	/// </summary>
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

	/// <summary>
	/// 
	/// </summary>
	public static bool IsInTheFinalVerse => IsInTerritory(1311) || IsInTerritory(1333);

	/// <summary>
	/// Determines if the current content is a Monster Hunter duty
	/// </summary>
	public static bool IsInMonsterHunterDuty => RathalosNormal || RathalosEX || ArkveldNormal || ArkveldEX;

	/// <summary>
	/// 
	/// </summary>
	public static bool RathalosNormal => IsInTerritory(761);

	/// <summary>
	/// 
	/// </summary>
	public static bool RathalosEX => IsInTerritory(762);

	/// <summary>
	/// 
	/// </summary>
	public static bool ArkveldNormal => IsInTerritory(1300);

	/// <summary>
	/// 
	/// </summary>
	public static bool ArkveldEX => IsInTerritory(1306);

	/// <summary>
	/// 
	/// </summary>
	public static bool Orbonne => IsInTerritory(826);

	/// <summary>
	/// 
	/// </summary>
	public static bool Emanation => IsInTerritory(719);

	/// <summary>
	/// 
	/// </summary>
	public static bool EmanationEX => IsInTerritory(720);
	#endregion

	#region Job Info
	public static Job Job => Player.Job;

	private static readonly BaseItem PhoenixDownItem = new(4570);
	/// <summary>
	/// Is someone alive who could raise, in the set the current raise settings draw targets from?
	///
	/// Two callers ask this with opposite intent, which is why the player is a parameter rather
	/// than a fixed rule. A Phoenix Down asks "can nobody do this properly" and must count the
	/// player, because a living raiser uses their spell instead of an item. The only-healer hard
	/// cast modes ask "is there anyone else", and there the player is the one deciding.
	///
	/// The reference set follows <see cref="Configs.RaiseType"/>, because that is what decides
	/// where a corpse may come from. Under the alliance modes the other alliances are real parties
	/// with their own raisers: while one of them is alive the raise is theirs to take. AllOutOfDuty
	/// is deliberately excluded - strangers in the open world are not a raise reserve, and counting
	/// them would suppress raising almost always.
	/// </summary>
	/// <param name="excludeSelf">Skip the player, for callers asking whether anyone *else* can.</param>
	public static bool AnyLivingRaiser(bool excludeSelf)
	{
		return AnyLivingRaiser(excludeSelf, healersOnly: false);
	}

	/// <inheritdoc cref="AnyLivingRaiser(bool)"/>
	/// <param name="excludeSelf">Skip the player, for callers asking whether anyone *else* can.</param>
	/// <param name="healersOnly">
	/// Count only the healer jobs, leaving Summoner and Red Mage out.
	///
	/// The two "only healer" hard cast modes ask for this, because their option text does: "Raise
	/// while Swiftcast is on cooldown and other <i>healers</i> are dead". The wider question - can
	/// anybody raise - would hold the hard cast back on the strength of a living Summoner, that is
	/// on an assumption about what another player is going to do. The user's instruction is the
	/// text: those modes wait for healers and for nobody else.
	///
	/// The feather keeps the wide set, because his instruction there is the opposite one and says
	/// raiser: a Phoenix Down goes out when nobody left can raise at all.
	///
	/// The level check applies either way - a healer below <see cref="RaiseLevel"/> has no raise to
	/// wait for.
	/// </param>
	public static bool AnyLivingRaiser(bool excludeSelf, bool healersOnly)
	{
		if (HasLivingRaiser(PartyMembers, excludeSelf, healersOnly))
		{
			return true;
		}

		return Service.Config.RaiseType is RaiseType.PartyAndAllianceSupports
				or RaiseType.PartyAndAllianceHealers
				or RaiseType.All
			&& HasLivingRaiser(AllianceMembers, excludeSelf, healersOnly);
	}

	private static bool HasLivingRaiser(IEnumerable<IBattleChara>? members, bool excludeSelf, bool healersOnly)
	{
		if (members == null)
		{
			return false;
		}

		foreach (var member in members)
		{
			if (member == null || member.IsDead)
			{
				continue;
			}

			if (excludeSelf && member.IsPlayer())
			{
				continue;
			}

			if (member.IsJobCategory(JobRole.Healer)
				|| (!healersOnly && member.IsJobs(ECommons.ExcelServices.Job.SMN)))
			{
				if (member.Level >= RaiseLevel)
				{
					return true;
				}

				continue;
			}

			if (!healersOnly && member.IsJobs(ECommons.ExcelServices.Job.RDM) && member.Level >= VerraiseLevel)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The level at which a healer or a Summoner has their raise, and the one at which a Red Mage has
	/// Verraise.
	///
	/// <see cref="CanRaise"/> has always applied these to the player. <see cref="HasLivingRaiser"/> did
	/// not apply them to anyone else, so a Red Mage below 64 - every level-synced run through the older
	/// content - counted as a living raiser and held back the feather that was the only way anyone was
	/// getting up. Sharing the two numbers is what keeps the question from being answered differently
	/// for the player than for the party.
	///
	/// For another party member the level comes from ICharacter.Level, which is what the client shows
	/// for them; whether that reports the synced level or the true one inside a synced duty is not
	/// established here. Either way it is strictly better than the assumption it replaces, which was
	/// that every healer, Summoner and Red Mage alive can raise.
	/// </summary>
	private const byte RaiseLevel = 12;

	/// <inheritdoc cref="RaiseLevel"/>
	private const byte VerraiseLevel = 64;

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

	public static JobRole Role
	{
		get
		{
			if (CurrentRotation is BlueMageRotation)
			{
				return BlueMageRotation.Role;
			}

			var classJob = Service.GetSheet<ClassJob>().GetRow((uint)Job);
			return classJob.RowId != 0 ? classJob.GetJobRole() : JobRole.None;
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
					radius = 3;
					break;
			}

			return radius;
		}
	}

	/// <summary>
	/// This quest is needed to do the quests that give Job Stones.
	/// </summary>
	public static unsafe bool SylphManagementFinished()
	{
		if (UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(66049))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Returns true if the current class is a base class (pre-jobstone), otherwise false.
	/// </summary>
	public static bool BaseClass()
	{
		// FFXIV base classes: 1-7, 26, 29 (GLA, PGL, MRD, LNC, ARC, CNJ, THM, ACN, ROG)
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
	/// Returns the total duration of the default GCD (seconds). Clamped to non-negative.
	/// </summary>
	public static float DefaultGCDTotal => Math.Max(0f, ActionManagerHelper.GetDefaultRecastTime());

	/// <summary>
	/// Returns the remaining time for the default GCD by subtracting the elapsed time from the total recast time.
	/// Clamped to non-negative.
	/// </summary>
	public static float DefaultGCDRemain => Math.Max(0f, DefaultGCDTotal - DefaultGCDElapsed);

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

	/// <summary>
	/// The distance from <paramref name="battleChara"/> to the player's pet
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static float DistanceToPet(IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return float.MaxValue;
		}
		var pet = GetPet();
		if (pet == null)
		{
			return float.MaxValue;
		}

		return Vector3.Distance(pet.Position, battleChara.Position) - (battleChara.HitboxRadius);
	}
	#endregion

	#region HP

	private static Dictionary<ulong, float> _refinedHpCache = [];
	private static long _refinedHpCacheTick = long.MinValue;

	// Party HP is read many times per decision pass (once per potential heal target),
	// so the computed dictionary is cached for a single frame instead of being rebuilt on every access.
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

			Dictionary<ulong, float> refinedHP = [];
			foreach (var member in PartyMembers)
			{
				try
				{
					if (member == null || member.GameObjectId == 0)
					{
						continue; // Skip invalid or null members
					}

					refinedHP[member.GameObjectId] = GetPartyMemberHPRatio(member);
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Error($"AccessViolationException in RefinedHP: {ex.Message}");
					continue; // Skip problematic members
				}
			}

			_refinedHpCache = refinedHP;
			_refinedHpCacheTick = now;
			return refinedHP;
		}
	}

	// Tracks each member's last confirmed (non-predicted) HP, so a pending heal's predicted
	// amount can be dropped as soon as the server's real HP update reflects it.
	private static readonly Dictionary<ulong, uint> _lastHp = [];

	private static float GetPartyMemberHPRatio(IBattleChara member)
	{
		ArgumentNullException.ThrowIfNull(member);

		if (member.MaxHp == 0)
		{
			return 0f;
		}

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

			if (currentHp - lastHp >= healedHp)
			{
				_ = HealHP.Remove(id);
				_lastHp[id] = currentHp;
				return (float)currentHp / member.MaxHp;
			}

			return Math.Min(1, (healedHp + currentHp) / (float)member.MaxHp);
		}

		return (float)currentHp / member.MaxHp;
	}

	private static readonly float[] _hpBuffer = new float[8];
	private static long _partyHpStatsCacheTick = long.MinValue;
	private static float _minHpCache, _avgHpCache, _stdDevHpCache, _lowestAvgHpCache, _lowestStdDevHpCache;
	private const long PartyHpStatsTtlMs = 15;

	// Each of the PartyMembers*HP properties below wants a different field of this same
	// computation, and are often checked back-to-back in the same decision (e.g. CustomRotation_GCD),
	// so the result is cached for a single frame rather than recomputed per property access.
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
			if (member.GameObjectId != 0 && hpCount < _hpBuffer.Length)
			{
				try
				{
					var hp = GetPartyMemberHPRatio(member);
					if (hp > 0)
					{
						_hpBuffer[hpCount++] = hp;
					}
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Error($"AccessViolationException in Party HP computation: {ex.Message}");
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

		// If there are more than 4 players, we order the array
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
			var hpList = new List<float>();
			foreach (var member in PartyMembers)
			{
				try
				{
					if (member == null || member.GameObjectId == 0)
					{
						continue;
					}

					var hp = GetPartyMemberHPRatio(member);
					if (hp > 0)
					{
						hpList.Add(hp);
					}
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Error($"AccessViolationException in PartyMembersHP: {ex.Message}");
				}
			}

			foreach (var hp in hpList)
			{
				yield return hp;
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
				List<DamageRec> recs = [];
				foreach (var rec in _damages)
				{
					if (DateTime.Now - rec.ReceiveTime < TimeSpan.FromMilliseconds(5))
					{
						recs.Add(rec);
					}
				}

				if (recs.Count == 0)
				{
					return 0;
				}

				float damages = 0;
				for (var i = 0; i < recs.Count; i++)
				{
					damages += recs[i].Ratio;
				}
				var first = recs[0].ReceiveTime;
				var last = recs[^1].ReceiveTime;
				var time = last - first + TimeSpan.FromMilliseconds(2.5f);

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

		//Record
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
		_partyHpStatsCacheTick = long.MinValue;
		_timeLastActionUsed = DateTime.Now;
		_actions.Clear();

		AttackedTargets.Clear();
		while (VfxDataQueue.TryDequeue(out _))
		{ }
		AllHostileTargets.Clear();
		AllianceMembers.Clear();
		PartyMembers.Clear();
		AllTargets.Clear();
		TargetsByRange.Clear();
	}

	internal static void AddDamageRec(float damageRatio)
	{
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
	/// Determines whether any currently casting hostile action is classified as magical.
	/// </summary>
	/// <returns>
	/// True if at least one hostile target is casting an action whose <c>AttackType.RowId == 5</c> (interpreted as magical); otherwise false.
	/// </returns>
	/// <remarks>
	/// Scans all hostile entities with a non-zero <c>CastActionId</c>, looks up the action row, and inspects the attack type.
	/// Returns early on the first confirmed magical cast.
	/// If the action sheet cannot be loaded or no valid casts exist, returns false.
	/// </remarks>
	public static bool IsMagicalDamageIncoming()
	{
		var hostileEnum = AllHostileTargets;
		if (hostileEnum == null)
		{
			return false;
		}

		var actionSheet = Service.GetSheet<Action>();
		if (actionSheet == null)
		{
			return false;
		}

		for (int i = 0, n = hostileEnum.Count; i < n; i++)
		{
			var hostile = hostileEnum[i];
			if (hostile == null)
			{
				continue;
			}

			try
			{
				if (hostile.CastActionId == 0)
				{
					continue;
				}

				var action = actionSheet.GetRow(hostile.CastActionId);
				if (action.RowId == 0)
				{
					continue;
				}

				// AttackType row id 5 interpreted as magical.
				if (action.AttackType.RowId == 5)
				{
					return true;
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsMagicalDamageIncoming for obj {hostile?.GameObjectId}: {ex.Message}");
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether any currently casting hostile action is classified as physical.
	/// </summary>
	/// <returns>
	/// True if at least one hostile target is casting an action whose <c>AttackType.RowId == 7</c> (interpreted as physical); otherwise false.
	/// </returns>
	/// <remarks>
	/// Scans all hostile entities with a non-zero <c>CastActionId</c>, looks up the action row, and inspects the attack type.
	/// Returns early on the first confirmed magical cast.
	/// If the action sheet cannot be loaded or no valid casts exist, returns false.
	/// </remarks>
	public static bool IsPhysicalDamageIncoming()
	{
		var hostileEnum = AllHostileTargets;
		if (hostileEnum == null)
		{
			return false;
		}

		var actionSheet = Service.GetSheet<Action>();
		if (actionSheet == null)
		{
			return false;
		}

		for (int i = 0, n = hostileEnum.Count; i < n; i++)
		{
			var hostile = hostileEnum[i];
			if (hostile == null)
			{
				continue;
			}

			try
			{
				if (hostile.CastActionId == 0)
				{
					continue;
				}

				var action = actionSheet.GetRow(hostile.CastActionId);
				if (action.RowId == 0)
				{
					continue;
				}

				// AttackType row id 7 interpreted as physical.
				if (action.AttackType.RowId == 7)
				{
					return true;
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsPhysicalDamageIncoming for obj {hostile?.GameObjectId}: {ex.Message}");
			}
		}
		return false;
	}

	/// <summary>
	/// True if any hostile is currently casting action 46553 or 46554.
	/// </summary>
	public static bool IsExtremeCastingSpecialIndicator()
	{
		if (IsInM10S)
		{
			var hostileEnum = AllHostileTargets;
			if (hostileEnum == null)
			{
				return false;
			}

			for (int i = 0, n = hostileEnum.Count; i < n; i++)
			{
				var hostile = hostileEnum[i];
				if (hostile == null)
				{
					continue;
				}

				try
				{
					if (hostile.CastActionId == 46553 || hostile.CastActionId == 46554)
					{
						return true;
					}
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Warning($"AccessViolation in IsHostileCastingSpecialIndicator for obj {hostile?.GameObjectId}: {ex.Message}");
				}
			}
		}

		return false;
	}

	/// <summary>
	/// True if any hostile is currently casting action 46553 or 46554.
	/// </summary>
	public static bool IsTyrantCastingSpecialIndicator()
	{
		if (IsInM11S)
		{
			var hostileEnum = AllHostileTargets;
			if (hostileEnum == null)
			{
				return false;
			}

			for (int i = 0, n = hostileEnum.Count; i < n; i++)
			{
				var hostile = hostileEnum[i];
				if (hostile == null)
				{
					continue;
				}

				try
				{
					if (hostile.CastActionId == 46117)
					{
						return true;
					}
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Warning($"AccessViolation in IsTyrantCastingSpecialIndicator for obj {hostile?.GameObjectId}: {ex.Message}");
				}
			}
		}

		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool IsTyrantCastingSpecialIndicator2()
	{
		if (!IsInM11S)
		{
			// Not in the relevant duty, ensure flags are cleared
			_hasCastScythe = false;
			_hasCastAxe = false;
			_wasCastingCharyb = false;
			_tyrantShouldStopHealing = false;
			return false;
		}

		// If combat hasn't started, reset everything
		if (CombatTimeRaw == 0)
		{
			_hasCastScythe = false;
			_hasCastAxe = false;
			_wasCastingCharyb = false;
			_tyrantShouldStopHealing = false;
			return false;
		}

		var hostileEnum = AllHostileTargets;
		if (hostileEnum == null)
		{
			return false;
		}

		var anyCurrentlyCastingCharyb = false;

		for (int i = 0, n = hostileEnum.Count; i < n; i++)
		{
			var hostile = hostileEnum[i];
			if (hostile == null)
			{
				continue;
			}

			try
			{
				if (!hostile.IsCasting)
				{
					anyCurrentlyCastingCharyb = false;
					continue;
				}

				var castId = hostile.CastActionId;
				if (castId == 46115)
				{
					_hasCastScythe = true;
					continue;
				}
				else if (castId == 46114)
				{
					_hasCastAxe = true;
					continue;
				}
				else if (castId == 46117)
				{
					anyCurrentlyCastingCharyb = true;
					_wasCastingCharyb = true;
					continue;
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsTyrantCastingSpecialIndicator for obj {hostile?.GameObjectId}: {ex.Message}");
			}
		}

		// If we've observed both scythe and axe, flip the stop-healing flag
		if (_hasCastScythe && _hasCastAxe)
		{
			_tyrantShouldStopHealing = true;
		}

		// If Charybdistopia was casting and now finished, clear everything
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
	/// 
	/// </summary>
	public static bool IsAgriasCastingSpecialIndicator()
	{
		if (!Orbonne)
		{
			return false;
		}

		var hostileEnum = AllHostileTargets;
		if (hostileEnum == null)
		{
			return false;
		}

		for (int i = 0, n = hostileEnum.Count; i < n; i++)
		{
			var hostile = hostileEnum[i];
			if (hostile == null)
			{
				continue;
			}

			try
			{
				// Ensure the hostile is actually casting
				if (!hostile.IsCasting)
				{
					continue;
				}

				// We're only interested in this specific cast id
				if (hostile.CastActionId != 14423)
				{
					continue;
				}

				// Remaining cast time is exposed as CurrentCastTime (units consistent with other checks)
				var remaining = hostile.TotalCastTime - hostile.CurrentCastTime;

				// If the remaining cast time is less than or equal to the player's remaining GCD,
				// trigger as close to the last second as possible.
				if (remaining <= 2.5f)
				{
					if (Service.Config.InDebug)
					{
						PluginLog.Debug($"Agrias cast detected on obj {hostile.GameObjectId} - remaining: {remaining:F3}s, GCD remain: {DefaultGCDRemain:F3}s");
					}
					return true;
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsHostileCastingSpecialIndicator for obj {hostile?.GameObjectId}: {ex.Message}");
			}
		}

		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool IsLichCastingSpecialIndicator()
	{
		if (!IsInQuestBattle)
		{
			return false;
		}

		var hostileEnum = AllHostileTargets;
		if (hostileEnum == null)
		{
			return false;
		}

		for (int i = 0, n = hostileEnum.Count; i < n; i++)
		{
			var hostile = hostileEnum[i];
			if (hostile == null)
			{
				continue;
			}

			try
			{
				// Ensure the hostile is actually casting
				if (!hostile.IsCasting)
				{
					continue;
				}

				// Alexandrian Quake - 46419
				var castId = hostile.CastActionId;

				// Remaining cast time is exposed as CurrentCastTime (units consistent with other checks)
				var remaining = hostile.TotalCastTime - hostile.CurrentCastTime;

				if (castId == 46419 || castId == 46427)
				{
					if (remaining <= 3f)
					{
						if (Service.Config.InDebug)
						{
							PluginLog.Debug($"Lich Cast Detected");
						}
						return true;
					}
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsHostileCastingSpecialIndicator for obj {hostile?.GameObjectId}: {ex.Message}");
			}
		}

		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	public static bool IsLakshmiCastingSpecialIndicator()
	{
		if (!EmanationEX && !Emanation)
		{
			return false;
		}

		var hostileEnum = AllHostileTargets;
		if (hostileEnum == null)
		{
			return false;
		}

		for (int i = 0, n = hostileEnum.Count; i < n; i++)
		{
			var hostile = hostileEnum[i];
			if (hostile == null)
			{
				continue;
			}

			try
			{
				// Ensure the hostile is actually casting
				if (!hostile.IsCasting)
				{
					continue;
				}

				// We're only interested in a specific set of Lakshmi cast ids
				// Known special casts (from duty):
				// Stotram - 8519 (Raidwide DOT)
				// Divine Denial - 8521 (Raidwide knockback)
				// Divine Doubt - 8522 (Raidwide confuse)
				// Divine Desire - 8523 (Raidwide pull and bleed)
				// The Path of Light - 8539 (high damage when Chanchala buffed)
				// The Pull of Light - 8543 (high damage when Chanchala buffed)
				var castId = hostile.CastActionId;

				if (EmanationEX)
				{
					if (hostile.HasStatus(false, StatusID.Chanchala_1410))
					{
						if (castId != 8519 && castId != 8521 && castId != 8522 && castId != 8523 && castId != 8539 && castId != 8543)
						{
							continue;
						}
					}
					if (!hostile.HasStatus(false, StatusID.Chanchala_1410))
					{
						if (castId != 8519 && castId != 8521 && castId != 8522 && castId != 8523)
						{
							continue;
						}
					}
				}

				if (Emanation)
				{
					if (castId != 9349)
					{
						continue;
					}
				}

				// Remaining cast time is exposed as CurrentCastTime (units consistent with other checks)
				var remaining = hostile.TotalCastTime - hostile.CurrentCastTime;

				// If the remaining cast time is less than or equal to the player's remaining GCD,
				// trigger as close to the last second as possible.
				if (remaining <= 3f)
				{
					if (Service.Config.InDebug)
					{
						PluginLog.Debug($"Lakshmi cast detected on obj {hostile.GameObjectId} - remaining: {remaining:F3}s, GCD remain: {DefaultGCDRemain:F3}s");
					}
					return true;
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsHostileCastingSpecialIndicator for obj {hostile?.GameObjectId}: {ex.Message}");
			}
		}

		return false;
	}

	// Cached, case-insensitive path sets modeled after WrathCombo VFX.cs
	private static readonly FrozenSet<string> TankbusterPaths = FrozenSet.ToFrozenSet(
	[
		"vfx/lockon/eff/tank_lockon",
		"vfx/lockon/eff/tank_laser",
		"vfx/lockon/eff/sharelaser2tank5sec_c0k1",
		"vfx/lockon/eff/sharelaser2tank8sec_c0p",
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
		"vfx/lockon/eff/share_10s_6m_0w",
		"vfx/lockon/eff/share_12s_6m_t1",
		"vfx/lockon/eff/share_14s_6m_t1",
		"vfx/lockon/eff/com_trg01_0c",
		"vfx/lockon/eff/com_trg02_0c",
		"vfx/lockon/eff/com_trg01_0c",
		"vfx/lockon/eff/x6r9_loc01_t0a1",
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
		"vfx/lockon/eff/x6r9_loc01_t0a1",
		"vfx/lockon/eff/x6r9_loc02_t0a1",
		// Duty-specific AOE share markers
		"vfx/lockon/eff/x6fd_loc04m_5s1v",
		"vfx/lockon/eff/m0922tar_a0w"
	], StringComparer.OrdinalIgnoreCase);

	private static readonly StringComparison PathCmp = StringComparison.OrdinalIgnoreCase;

	public static bool IsHostileCastingAOE =>
		InCombat && (IsCastingAreaVfx() || (AllHostileTargets != null && IsAnyHostileCastingArea()));

	/// <summary>
	/// An enemy is casting an action whose measured area damage is large, that will reach the player,
	/// and that lands within about one GCD.
	/// </summary>
	/// <remarks>
	/// <para><b>Why this exists beside <see cref="IsHostileCastingAOE"/>.</b> That one routes through
	/// <see cref="IsHostileCastingBase"/>, which drops every INTERRUPTIBLE cast. The reasoning behind
	/// that is sound - an interruptible cast is meant to be interrupted, and mitigating it would spend
	/// a cooldown on something that never lands. It holds only while somebody actually interrupts.
	/// A Summoner has no interrupt; in a four-player dungeon with a tank who does not use Interject,
	/// the cast lands anyway, and nothing answered it. That is the reported picture: "sometimes Radiant
	/// Aegis and Addle go out on area casts, sometimes not".</para>
	///
	/// <para><b>Why it is not a return to what A9 removed.</b> A9 took out a fallback that raised the
	/// defence from the ENEMY COUNT - no evidence of danger at all, and the owner reported it firing
	/// with nothing happening. This asks the opposite: it requires a figure measured from an actual
	/// landing, at or above what the largest barrier in the game absorbs. A cast nobody has been hit
	/// by carries no figure and opens nothing.</para>
	///
	/// <para><b>What makes it possible now and not then.</b> The per-action damage share did not exist
	/// when that fallback was removed, so coarseness had to be handled in the pre-filter. With the
	/// size measured, the filter no longer has to carry that job alone.</para>
	///
	/// <para>Deliberately a separate property rather than a loosening of
	/// <see cref="IsHostileCastingAOE"/>: that one also feeds <c>ObjectHelper.IsUnderThreat</c> and
	/// through it the White Mage's Benediction guard. Widening it would move two paths at once.</para>
	///
	/// <para><b>This answers what is being cast, not what anyone wants done about it</b>, so the
	/// setting that governs the mitigation decision is checked by its reader and not here. Built the
	/// other way round first, and it was wrong twice over: the healing rule reads the same question
	/// and would have been switched off by a setting about mitigating, and the recorded share -
	/// which the healing rule needs - would never have been written while that setting was off. The
	/// same coupling had already been found and removed from AreaCastIsWorthMitigating in the same
	/// session; putting it straight back in a new property makes it a class, not a slip.</para>
	/// </remarks>
	public static bool IsHostileCastingLargeArea
	{
		get
		{
			if (!InCombat)
			{
				return false;
			}

			var targets = AllHostileTargets;
			if (targets == null)
			{
				return false;
			}

			var actionSheet = Service.GetSheet<Action>();
			if (actionSheet == null)
			{
				return false;
			}

			for (var i = 0; i < targets.Count; i++)
			{
				var h = targets[i];
				try
				{
					if (h == null || h.GameObjectId == 0 || !h.IsCasting || !h.IsEnemy())
					{
						continue;
					}

					// The same one-GCD window IsHostileCastingBase uses at its far end, and for the
					// same reason: answer shortly before the hit, not at the start of the cast. It
					// also leaves the interrupt its chance - by this point it has happened or it will
					// not happen at all.
					var remaining = h.TotalCastTime - h.CurrentCastTime;
					if (remaining <= 0f || remaining > GCDTime(1))
					{
						continue;
					}

					if (!OtherConfiguration.HostileCastingArea.Contains(h.CastActionId))
					{
						continue;
					}

					if (!OtherConfiguration.HostileCastingAreaPotential.TryGetValue(h.CastActionId, out var share)
						|| share < LargeShieldShare)
					{
						continue;
					}

					var action = actionSheet.GetRow(h.CastActionId);
					if (action.RowId == 0 || !AreaCastCanReachPlayer(h, action))
					{
						continue;
					}

					// Recorded here as well, and not only in AreaCastIsWorthMitigating, because this
					// path never passes through it. Without this the healing rule would be blind to
					// exactly the casts this property exists to catch: the defence would open and
					// the heal would not - and the standing order is healing before mitigation.
					_announcedAreaShare = share;
					_announcedAreaShareTime = DateTime.Now;
					_lastAnnouncedAreaAction = h.CastActionId;

					return true;
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Warning($"AccessViolation in IsHostileCastingLargeArea: {ex.Message}");
				}
			}

			return false;
		}
	}

	/// <summary>
	/// An area cast is announced that will reach the player - <b>without</b> asking whether it is
	/// worth mitigating. Records its measured share on the way.
	/// </summary>
	/// <remarks>
	/// <para><see cref="IsHostileCastingAOE"/> carries a verdict inside it: <c>AreaCastIsWorthMitigating</c>
	/// drops a cast that leaves everybody above <c>HealthAreaSpell</c>. That is the right question
	/// for spending a cooldown, and the wrong one for every other reader, because it is taken at the
	/// MITIGATION threshold.</para>
	///
	/// <para>The healing rule asks at its own, higher threshold (<c>HealthAreaAbility</c>, 0.75
	/// against 0.65 in the shipped defaults). Reading the mitigation verdict would therefore lose it
	/// exactly the band between the two: a hit that puts the party at 70% is "too small to mitigate"
	/// and would have been too small to heal ahead of as well - although the heal flag itself would
	/// have raised at that health. The rule would have been switched off by a decision that is not
	/// its own.</para>
	///
	/// <para>Same shape as the two couplings already removed in this session - the recorded share
	/// sitting behind <c>SkipMitigationForSmallAreaCasts</c>, and the mitigation setting sitting
	/// inside <see cref="IsHostileCastingLargeArea"/>. Three of them is a class: <b>a recognition
	/// must not contain a decision.</b></para>
	/// </remarks>
	public static bool IsHostileCastingAreaUnrated
	{
		get
		{
			var targets = AllHostileTargets;
			if (!InCombat || targets == null)
			{
				return false;
			}

			for (var i = 0; i < targets.Count; i++)
			{
				var h = targets[i];
				if (h == null)
				{
					continue;
				}

				if (IsHostileCastingBase(h, act =>
				{
					if (!OtherConfiguration.HostileCastingArea.Contains(act.RowId)
						|| !AreaCastCanReachPlayer(h, act))
					{
						return false;
					}

					RecordAnnouncedAreaShare(act.RowId);
					return true;
				}))
				{
					return true;
				}
			}

			return false;
		}
	}

	private static bool IsAnyHostileCastingArea()
	{
		if (AllHostileTargets == null)
		{
			return false;
		}

		for (var i = 0; i < AllHostileTargets.Count; i++)
		{
			if (IsHostileCastingArea(AllHostileTargets[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHostileCastingToTank =>
		InCombat && (IsCastingTankVfx() || (AllHostileTargets != null && IsAnyHostileCastingTank()));

	private static bool IsAnyHostileCastingTank()
	{
		if (AllHostileTargets == null)
		{
			return false;
		}

		for (var i = 0; i < AllHostileTargets.Count; i++)
		{
			if (IsHostileCastingTank(AllHostileTargets[i]))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// True if a hostile is casting a tankbuster at the player specifically, whatever their role.
	/// Counts only casts actually known to be tankbusters: an action on the curated
	/// <see cref="OtherConfiguration.HostileCastingTank"/> list, or one that placed a tankbuster
	/// lock-on VFX on the player. The "casting at its current target" fallback inside
	/// <see cref="IsHostileCastingTank"/> is deliberately not used here. That fallback holds for
	/// practically every uninterruptible cast a hostile aims at whoever it is attacking, which for
	/// a tank is a fair guess at a tankbuster but for anyone else is an ordinary trash-mob attack -
	/// and it made every such cast open the whole single-target defensive chain.
	/// </summary>
	public static bool IsHostileCastingTankBusterAtMe =>
		InCombat && Player.Object != null
		&& (IsTankbusterVfxOnPlayer() || (AllHostileTargets != null && IsAnyHostileCastingListedTankBusterAtMe()));

	private static bool IsAnyHostileCastingListedTankBusterAtMe()
	{
		if (AllHostileTargets == null || Player.Object == null)
		{
			return false;
		}

		var playerId = Player.Object.GameObjectId;
		for (var i = 0; i < AllHostileTargets.Count; i++)
		{
			var h = AllHostileTargets[i];
			if (h != null && h.CastTargetObjectId == playerId && IsHostileCastingListedTank(h))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Whether the hostile is casting an action from the curated tankbuster list, without the
	/// target-identity fallback that <see cref="IsHostileCastingTank"/> adds on top of it.
	/// </summary>
	public static bool IsHostileCastingListedTank(IBattleChara h)
	{
		return h != null && IsHostileCastingBase(h, (act) =>
		{
			return OtherConfiguration.HostileCastingTank.Contains(act.RowId);
		});
	}

	/// <summary>
	/// Whether a tankbuster lock-on VFX currently sits on the player. Read-only counterpart to
	/// <see cref="IsCastingTankVfx"/>, which also rebuilds <see cref="TankbusterTargets"/> and
	/// answers for the party rather than for the player alone.
	/// </summary>
	public static bool IsTankbusterVfxOnPlayer()
	{
		var player = Player.Object;
		if (player == null || VfxDataQueue == null || VfxDataQueue.IsEmpty)
		{
			return false;
		}

		var playerId = player.GameObjectId;
		foreach (var s in VfxDataQueue)
		{
			try
			{
				if (s.ObjectId != playerId || string.IsNullOrEmpty(s.Path))
				{
					continue;
				}

				foreach (var p in TankbusterPaths)
				{
					if (s.Path.StartsWith(p, PathCmp))
					{
						return true;
					}
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Warning($"AccessViolation in IsTankbusterVfxOnPlayer while scanning VFX: {ex.Message}");
			}
		}
		return false;
	}

	public static bool IsHostileCastingStop =>
		InCombat && Service.Config.CastingStop && AllHostileTargets != null && IsAnyHostileStop();

	private static bool IsAnyHostileStop()
	{
		if (AllHostileTargets == null)
		{
			return false;
		}

		for (var i = 0; i < AllHostileTargets.Count; i++)
		{
			if (IsHostileStop(AllHostileTargets[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHostileStop(IBattleChara h)
	{
		return IsHostileCastingStopBase(h,
			(act) =>
			{
				if (act.RowId == 0)
				{
					return false;
				}

				return OtherConfiguration.HostileCastingStop.Contains(act.RowId);
			});
	}

	public static bool IsHostileCastingStopBase(IBattleChara h, Func<Action, bool> check)
	{
		if (h == null || check == null)
		{
			return false;
		}

		try
		{
			if (h.GameObjectId == 0)
			{
				return false;
			}

			// Check if the hostile character is casting
			if (!h.IsCasting)
			{
				return false;
			}

			// Check if the cast is interruptible
			if (h.IsCastInterruptible)
			{
				return false;
			}

			// Validate the cast time
			if ((h.TotalCastTime - h.CurrentCastTime) > (Service.Config.CastingStopCalculate ? 100 : Service.Config.CastingStopTime))
			{
				return false;
			}

			// Get the action sheet
			var actionSheet = Service.GetSheet<Action>();
			if (actionSheet == null)
			{
				return false; // Check if actionSheet is null
			}

			// Get the action being cast
			var action = actionSheet.GetRow(h.CastActionId);
			if (action.RowId == 0)
			{
				return false; // Check if action is not initialized
			}

			// Invoke the check function on the action and return the result
			return check(action);
		}
		catch (AccessViolationException ex)
		{
			PluginLog.Warning($"AccessViolation in IsHostileCastingStopBase for obj {h?.GameObjectId}: {ex.Message}");
			return false;
		}
	}

	public static bool IsCastingVfx(ConcurrentQueue<VfxNewData> vfxData, Func<VfxNewData, bool> isVfx)
	{
		if (vfxData == null || vfxData.IsEmpty)
		{
			return false;
		}

		foreach (var data in vfxData)
		{
			if (isVfx(data))
			{
				return true;
			}
		}
		return false;
	}

	// Improved multi-hit detection using a cached set of known multi-hit share paths
	public static bool IsCastingMultiHit()
	{
		return IsCastingVfx(VfxDataQueue, s =>
		{
			if (!Player.Available || Player.Object == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(s.Path))
			{
				return false;
			}

			// Any path in multi-hit share list qualifies
			foreach (var p in MultiHitSharedPaths)
			{
				if (s.Path.StartsWith(p, PathCmp))
				{
					return true;
				}
			}

			return false;
		});
	}

	public static bool IsCastingTankVfx()
	{
		// Populate TankbusterTargets from the VFX queue and return whether any tankbuster VFX was found.
		lock (_tankbusterLock)
		{
			TankbusterTargets.Clear();
			if (!Player.Available || Player.Object == null)
			{
				return false;
			}

			if (VfxDataQueue == null || VfxDataQueue.IsEmpty)
			{
				return false;
			}

			var found = false;
			var isTank = TargetFilter.PlayerJobCategory(JobRole.Tank);

			foreach (var s in VfxDataQueue)
			{
				try
				{
					if (string.IsNullOrEmpty(s.Path))
					{
						continue;
					}

					foreach (var p in TankbusterPaths)
					{
						if (!s.Path.StartsWith(p, PathCmp))
						{
							continue;
						}

						var isPlayerTarget = s.ObjectId == Player.Object.GameObjectId;

						if (!isTank || isPlayerTarget)
						{
							if (Service.Config.InDebug)
							{
								PluginLog.Debug($"Tank lock-on VFX triggered: {s.Path}, ObjectId: {s.ObjectId}");
							}

							// Try to resolve the object id to a party/alliance member and add to the list
							if (Svc.Objects.SearchById(s.ObjectId) is IBattleChara obj && obj.IsParty() && !obj.IsDead && !TankbusterTargets.Contains(obj))
							{
								TankbusterTargets.Add(obj);
							}

							found = true;
						}
					}
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Warning($"AccessViolation in IsCastingTankVfx while scanning VFX: {ex.Message}");
				}
			}

			return found;
		}
	}

	// Improved shared AOE detection using cached sets, covers both regular and multi-hit stack markers
	public static bool IsCastingAreaVfx()
	{
		return IsCastingVfx(VfxDataQueue, s =>
		{
			if (!Player.Available || Player.Object == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(s.Path))
			{
				return false;
			}

			// Multi-hit markers (treated as area/stack mechanics)
			foreach (var p in MultiHitSharedPaths)
			{
				if (s.Path.StartsWith(p, PathCmp))
				{
					return true;
				}
			}

			// Regular shared markers
			foreach (var p in SharedDamagePaths)
			{
				if (s.Path.StartsWith(p, PathCmp))
				{
					return true;
				}
			}

			// Regular spread markers
			foreach (var p in SpreadDamagePaths)
			{
				if (s.Path.StartsWith(p, PathCmp))
				{
					return true;
				}
			}

			return false;
		});
	}

	public static bool IsHostileCastingTank(IBattleChara h)
	{
		return h != null && IsHostileCastingBase(h, (act) =>
		{
			return OtherConfiguration.HostileCastingTank.Contains(act.RowId)
				|| h.CastTargetObjectId == h.TargetObjectId;
		});
	}

	public static bool IsHostileCastingArea(IBattleChara h)
	{
		return IsHostileCastingBase(h, (act) =>
		{
			// Contains, not a walk over the set. All four of these lists were searched with a
			// foreach that compared every id in turn - O(n) out of a structure whose whole purpose
			// is O(1). The area list ships with 850 entries and grows in play, so the cost of the
			// question grew with the list while it could have been constant.
			//
			// Not a hot-path emergency: IsHostileCastingBase only reaches this predicate while an
			// enemy is casting something uninterruptible that is longer than a GCD and sits in a
			// one-GCD window before it lands, so a trash pull barely gets here. It is simply wrong
			// as built, and it is the reason the list cannot be allowed to grow freely - see
			// docs/rotation-flow/13-aoe-damage-classification.md.
			return OtherConfiguration.HostileCastingArea.Contains(act.RowId)
				&& AreaCastCanReachPlayer(h, act)
				&& AreaCastIsWorthMitigating(act.RowId);
		});
	}

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

	/// <summary>
	/// Every small area action a predicted mitigation was held back for, and when it last happened.
	/// Same purpose as the two above: a mitigation that waits looks exactly like one that never
	/// triggered, so without this the rule cannot be tested by switching it on.
	/// </summary>
	public static readonly ConcurrentDictionary<uint, DateTime> ProactiveMitigationHeld = new();

	// How large the announced area cast is, as a share of the weakest member's maximum health, and
	// when that was last established. This is the same figure AreaCastIsWorthMitigating already
	// looks up to decide WHETHER to answer; kept here it also answers WITH WHAT.
	//
	// Held with a timestamp rather than cleared from somewhere else: the predicate runs while an
	// enemy is casting and simply stops running when nothing is. A clearing point would have to be
	// found and kept correct in a second place; an age does not.
	private static float _announcedAreaShare;
	private static DateTime _announcedAreaShareTime = DateTime.MinValue;

	// Which action the recorded share belongs to, so a probe can name it rather than counting
	// anonymously. Carried with the share and subject to the same lifetime.
	private static uint _lastAnnouncedAreaAction;

	// One GCD's worth. The figure describes the cast that is landing now, so it must not outlive it
	// and steer the next decision; long enough for the defensive branch of the same window to read it.
	private static readonly TimeSpan AnnouncedAreaShareLifetime = TimeSpan.FromSeconds(2.5);

	/// <summary>
	/// The share of the weakest party member's maximum health carried by the area cast that is
	/// currently announced, or 0 when none is announced or its size has not been measured yet.
	/// 0 means "unknown", never "harmless".
	/// </summary>
	public static float AnnouncedAreaShare =>
		DateTime.Now - _announcedAreaShareTime <= AnnouncedAreaShareLifetime ? _announcedAreaShare : 0f;

	/// <summary>
	/// An area cast is running right now whose measured damage is small - small enough that the
	/// reactive rule would not spend a cooldown on it. False when nothing is running, when the
	/// running cast has never been measured, or when it is large.
	/// </summary>
	/// <remarks>
	/// <para>The counterpart to <see cref="AreaCastIsWorthMitigating"/> for the PROACTIVE side.
	/// BossModReborn answers when the next damage arrives, never how hard, and that is enough to
	/// spend a cooldown on the wrong one of two hits in a row: reported from Eternal Queen's
	/// opening, a small area cast followed by a big one, where the barrier is eaten by the first.</para>
	///
	/// <para>The size of a cast already on screen IS known, because it is measured per action. So a
	/// rated small cast running now is the most likely referent of a prediction pointing at the
	/// immediate future, and a proactive refresh can wait for the next one. A heuristic, and stated
	/// as one: nothing here proves the prediction means this cast.</para>
	///
	/// <para><b>The exact question - which action is the prediction pointing at, so its rating can be
	/// looked up - cannot be asked. Measured against BossmodReborn's own source on 20.09.2026, not
	/// assumed:</b></para>
	/// <list type="bullet">
	/// <item><c>Timeline.NextRaidwideIn</c> returns
	/// <c>module.StateMachine.NextTransitionWithFlag(StateHint.Raidwide)</c> - a transition in the
	/// module's state machine that its author flagged as "a raidwide happens here". There is no cast
	/// and no action id in it at all.</item>
	/// <item><c>Hints.NextRaidwideDamageIn</c> returns the activation time of the first
	/// <c>PredictedDamage</c> entry of that type, and that struct carries exactly three fields:
	/// <c>Players</c> (a bitmask), <c>Activation</c> and <c>Type</c>. No action.</item>
	/// <item>The prediction list is not exposed either - each endpoint returns the FIRST matching
	/// entry. Even with sizes available, "two are coming and the second is the big one" could not be
	/// read out, which is precisely the reported case.</item>
	/// </list>
	/// <para>So the rating cannot be looked up for a predicted event. What is running now is the only
	/// size information available at that moment, and this is what it is worth.</para>
	///
	/// <para>"Small" is the same question the reactive rule asks - would this hit push anyone to
	/// where the tree heals anyway - so the two sides cannot disagree about the same cast.</para>
	/// </remarks>
	public static bool AnnouncedHitIsSmall
	{
		get
		{
			var share = AnnouncedAreaShare;
			if (share <= 0f || share >= LargeShieldShare)
			{
				return false;
			}

			return !AnnouncedHitDropsAnyoneBelow(Service.Config.HealthAreaSpell);
		}
	}

	/// <summary>
	/// The action id the currently recorded area share belongs to, or 0 when none is recorded.
	/// </summary>
	public static uint AnnouncedAreaAction =>
		DateTime.Now - _announcedAreaShareTime <= AnnouncedAreaShareLifetime ? _lastAnnouncedAreaAction : 0u;

	/// <summary>
	/// Whether the announced area cast would put any living party member below the given share of
	/// their maximum health. False when nothing is announced or its size has not been measured -
	/// unknown is not "harmless", it is simply no reason to act.
	/// </summary>
	/// <remarks>
	/// <para>This is the owner's second stage, the one that needs a figure nothing else supplies:
	/// "die aktuelle hp liegt unter dem schadenswert. dann wäre aber eine heilung sinnvoll bis max
	/// maxhp." Every healing threshold in the tree reads the health a member HAS; none of them reads
	/// the health he will have when the cast that is already on screen lands. A member at 60% in
	/// front of a 45% raidwide is above every threshold and dies to it.</para>
	///
	/// <para>The barrier counts here, and that is not a contradiction of A85. A85 removed the
	/// barrier from the general healing threshold, because a barrier does not restore health - a
	/// tank at 40% behind a shield is still at 40% once it expires unspent. This question is a
	/// different one: does he survive THIS hit, the one being cast right now. Against that hit the
	/// barrier is spent and does absorb it, so leaving it out would call for healing that the shield
	/// has already paid for.</para>
	/// </remarks>
	public static bool AnnouncedHitDropsAnyoneBelow(float threshold)
	{
		var share = AnnouncedAreaShare;
		if (share <= 0f)
		{
			return false;
		}

		var party = PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || member.IsDead || member.MaxHp == 0)
			{
				continue;
			}

			var buffer = member.GetEffectiveHp() / (float)member.MaxHp;
			if (buffer - share < threshold)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether an incoming area cast is big enough that the party mitigation is worth its cooldown.
	/// </summary>
	/// <remarks>
	/// The user's requirement: an area action whose damage stays below what a small shield absorbs
	/// does not need to be treated as a big hit; one at or above the size of a large shield does.
	/// What is actually spent on a trivial hit is not the shield but the **cooldown** - a Reprisal
	/// laid on a two-percent tick is missing at the next real one.
	///
	/// The measure is not a threshold anybody had to invent. It is the same question the healing
	/// side already answers: **does this hit create a need to heal?** Buffer minus expected hit
	/// against the level at which the tree would heal by itself. Two percent on a full player does
	/// not; thirty does. The same action is therefore minor for a healthy tank and major for a
	/// wounded caster, which no stored category could express - and it fits the standing order that
	/// healing comes before mitigation, because mitigation goes where healing would otherwise be
	/// needed.
	///
	/// An earlier draft compared against HealthForDyingTanks (0.15). Working it through showed it
	/// would practically never mitigate: a thirty-percent hit leaves a full player at seventy. A rule
	/// that cannot fire in its own domain is no rule - the same shape C59 found on the Holy hold.
	///
	/// Unrated actions return true, which is the behaviour the tree has always had. That is what
	/// keeps the 850 shipped entries from losing their mitigation the moment this ships: an entry
	/// only enters the arithmetic once an actual hit has been measured, and in content that is
	/// repeated - an extreme trial, a savage fight in progression - that is one clear.
	/// </remarks>
	/// <summary>
	/// What a large shield absorbs, as a share of maximum HP: the point from which an area hit counts
	/// as a big one regardless of how healthy the party is.
	/// </summary>
	/// <remarks>
	/// Not an invented number, and no longer a written-down one either: it is read from the effect
	/// texts. The largest barrier in the tree that states its size as a share is The Blackest Night -
	/// "absorbs damage totaling 25% of target's maximum HP" - and DefensiveValues.g.cs carries that
	/// figure along with every other, generated from the same sheets and checked against them in CI.
	///
	/// It used to be a literal 0.25f with the source named in prose. That is the ageing form this
	/// file has been caught by twice already: an earlier version of this remark said "five barriers"
	/// and listed their row ids, and the generated table finds more than five. A patch that restates
	/// a barrier, or a new job action with a larger one, now moves this threshold with it instead of
	/// leaving a number behind that nothing reads as wrong.
	///
	/// The smaller barriers name 10%, 15% and 20%, which is the other end of the user's requirement:
	/// below a small shield the hit only matters to someone already low. That end needs no constant,
	/// because the buffer comparison below is exactly that question.
	///
	/// Concept 13 once dismissed the two-threshold form as "not implementable without invented
	/// numbers, and not needed, because the buffer comparison answers the same question". Both halves
	/// were wrong: the barriers state their share, and the buffer comparison answers a different
	/// question - whether healing would be needed, not whether the hit is large.
	/// </remarks>
	private static float LargeShieldShare => DefensiveValues.LargestStatedBarrierShare;

	// Kept separate from the verdict below, and called before it, for two reasons. The figure
	// describes the incoming cast, not this rule's opinion of it - and a second reader, the healing
	// flag, needs it whether or not the owner wants small casts mitigated. Recorded inside the
	// verdict it sat behind SkipMitigationForSmallAreaCasts, which would have left that reader blind
	// to every cast whenever the switch was off.
	private static void RecordAnnouncedAreaShare(uint actionId)
	{
		if (OtherConfiguration.HostileCastingAreaPotential.TryGetValue(actionId, out var share)
			&& share > 0f)
		{
			_announcedAreaShare = share;
			_announcedAreaShareTime = DateTime.Now;
			_lastAnnouncedAreaAction = actionId;
		}
	}

	private static bool AreaCastIsWorthMitigating(uint actionId)
	{
		RecordAnnouncedAreaShare(actionId);

		if (!Service.Config.SkipMitigationForSmallAreaCasts)
		{
			return true;
		}

		if (!OtherConfiguration.HostileCastingAreaPotential.TryGetValue(actionId, out var share)
			|| share <= 0f)
		{
			return true; // Unrated: behave exactly as before.
		}

		// Above the size of a large shield the hit is a big one, whatever the party's health - the
		// user's requirement says so in as many words, and the party's health does not change how
		// hard it lands. Without this arm the rule asks only whether healing would be needed, and a
		// healthy party answers no to almost every raidwide: at a buffer of 1.0 against the area
		// heal threshold of 0.65, mitigation would need a share above 0.35 to happen at all. That
		// is what took Addle and Radiant Aegis out of the fight on Summoner.
		if (share >= LargeShieldShare)
		{
			return true;
		}

		if (PartyMembers.Count == 0)
		{
			return true;
		}

		// Anyone the hit would push to where healing would be called for is reason enough. The same
		// question decides whether to heal ahead of the cast, so it is asked in one place - see
		// AnnouncedHitDropsAnyoneBelow, including why the barrier counts here and not in the general
		// healing threshold (A85).
		if (AnnouncedHitDropsAnyoneBelow(Service.Config.HealthAreaSpell))
		{
			return true;
		}

		AreaMitigationSkipped[actionId] = DateTime.Now;
		return false;
	}

	/// <summary>
	/// Whether an area cast could reach the player at all. Hostiles are collected out to 48 yalms and
	/// the area list only records that an action once hit a whole party, never whether the player is
	/// inside this instance of it - so without this check any listed cast anywhere in that radius
	/// raised <see cref="AutoStatus.DefenseArea"/>, which opens the job's entire area-defense chain.
	/// Two cases pass regardless of distance: an effect range of 0, which covers both "not filled in"
	/// and the party-wide hits that carry no radius of their own, and a cast aimed at the player,
	/// since a ground-placed effect follows its target rather than its caster.
	/// </summary>
	private static bool AreaCastCanReachPlayer(IBattleChara h, Action act)
	{
		if (act.EffectRange == 0)
		{
			return true;
		}

		var player = Player.Object;
		if (player != null && h.CastTargetObjectId == player.GameObjectId)
		{
			return true;
		}

		return h.DistanceToPlayer() <= act.EffectRange;
	}

	public static bool AreHostilesCastingKnockback
	{
		get
		{
			var targets = AllHostileTargets;
			for (int i = 0, n = targets.Count; i < n; i++)
			{
				var h = targets[i];
				try
				{
					if (IsHostileCastingKnockback(h))
					{
						return true;
					}
				}
				catch (AccessViolationException ex)
				{
					PluginLog.Warning($"AccessViolation when checking knockback for obj {h?.GameObjectId}: {ex.Message}");
				}
			}
			return false;
		}
	}

	public static bool IsHostileCastingKnockback(IBattleChara h)
	{
		return IsHostileCastingBase(h,
			(act) =>
			{
				if (act.RowId == 0)
				{
					return false;
				}

				return OtherConfiguration.HostileCastingKnockback.Contains(act.RowId);
			});
	}

	public static bool IsHostileCastingBase(IBattleChara h, Func<Action, bool> check)
	{
		if (h == null || check == null)
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

		try
		{
			if (h.GameObjectId == 0)
			{
				return false;
			}

			if (!h.IsEnemy())
			{
				return false;
			}

			if (!h.IsCasting)
			{
				return false;
			}

			// Check if the cast is interruptible
			if (h.IsCastInterruptible)
			{
				return false;
			}

			// Calculate the time since the cast started
			var last = h.TotalCastTime - h.CurrentCastTime;
			var t = last - DefaultGCDTotal;

			// Check if the total cast time is greater than the minimum cast time and if the calculated time is within a valid range
			if (!(h.TotalCastTime > DefaultGCDTotal && t > 0 && t < GCDTime(1)))
			{
				return false;
			}

			// Get the action sheet
			var actionSheet = Service.GetSheet<Action>();
			if (actionSheet == null)
			{
				PluginLog.Error("IsHostileCastingBase: Action sheet is null.");
				return false;
			}

			// Get the action being cast
			var action = actionSheet.GetRow(h.CastActionId);
			if (action.RowId == 0)
			{
				PluginLog.Error("IsHostileCastingBase: Action is not initialized.");
				return false;
			}

			// Invoke the check function on the action and return the result
			return check(action);
		}
		catch (AccessViolationException ex)
		{
			PluginLog.Warning($"AccessViolation in IsHostileCastingBase for obj {h?.GameObjectId}: {ex.Message}");
			return false;
		}
	}
	#endregion

	#region BossModReborn Timeline Integration

	private static bool _bmrEnabledCache;
	private static long _bmrEnabledCacheTick = long.MinValue;
	private const long BmrEnabledTtlMs = 1000;

	// Installed-plugin state changes only when the user (de)activates a plugin, so this is
	// re-checked at most once a second instead of walking InstalledPlugins on every access.
	public static bool BMREnabled
	{
		get
		{
			var now = Environment.TickCount64;
			if (_bmrEnabledCacheTick != long.MinValue && now - _bmrEnabledCacheTick < BmrEnabledTtlMs)
			{
				return _bmrEnabledCache;
			}

			var name = "BossModReborn";
			var installedPlugins = Svc.PluginInterface.InstalledPlugins;
			var enabled = false;
			foreach (var x in installedPlugins)
			{
				if ((x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || x.InternalName.Equals(name, StringComparison.OrdinalIgnoreCase)) && x.IsLoaded)
				{
					enabled = true;
					break;
				}
			}

			_bmrEnabledCache = enabled;
			_bmrEnabledCacheTick = now;
			return enabled;
		}
	}

	private static bool _autoPvpSeriesGrindCache;
	private static long _autoPvpSeriesGrindCacheTick = long.MinValue;
	private const long AutoPvpSeriesGrindTtlMs = 1000;

	public static bool AutoPvpSeriesGrindEnabled
	{
		get
		{
			var now = Environment.TickCount64;
			if (_autoPvpSeriesGrindCacheTick != long.MinValue && now - _autoPvpSeriesGrindCacheTick < AutoPvpSeriesGrindTtlMs)
			{
				return _autoPvpSeriesGrindCache;
			}

			var enabled = false;
			var installedPlugins = Svc.PluginInterface.InstalledPlugins;
			foreach (var x in installedPlugins)
			{
				if ((x.Name.Equals("Auto PVP Series Grind", StringComparison.OrdinalIgnoreCase)
					|| x.InternalName.Equals("AutoPvpSeriesGrind", StringComparison.OrdinalIgnoreCase)) && x.IsLoaded)
				{
					enabled = true;
					break;
				}
			}

			_autoPvpSeriesGrindCache = enabled;
			_autoPvpSeriesGrindCacheTick = now;
			return enabled;
		}
	}

	public static bool PvPAutomationBlocked => IsPvP && AutoPvpSeriesGrindEnabled;

	public static bool BMRHasActiveModule { get; set; }
	public static string? BMRActiveModuleName { get; set; }
	public static float BMRNextRaidwideIn { get; set; } = float.MaxValue;
	public static float BMRNextTankbusterIn { get; set; } = float.MaxValue;
	public static float BMRNextKnockbackIn { get; set; } = float.MaxValue;
	public static float BMRNextDowntimeIn { get; set; } = float.MaxValue;
	public static float BMRNextDowntimeEndIn { get; set; } = float.MaxValue;
	public static float BMRNextVulnerableIn { get; set; } = float.MaxValue;
	public static float BMRNextVulnerableEndIn { get; set; } = float.MaxValue;
	public static float BMRNextDamageIn { get; set; } = float.MaxValue;
	public static PredictedDamageType BMRNextDamageType { get; set; } = PredictedDamageType.None;

	/// <summary>
	/// Who the next predicted damage event hits, as BossModReborn's party bitmask. 0 when nothing is
	/// predicted.
	/// </summary>
	/// <remarks>
	/// Reads the same entry as <see cref="BMRNextDamageIn"/> and <see cref="BMRNextDamageType"/> -
	/// BossMod sorts its prediction list by activation time and all three endpoints take the first
	/// element, so the three describe one event. The type-specific endpoints behind
	/// <see cref="BMRNextRaidwideIn"/> and <see cref="BMRNextTankbusterIn"/> search for the first
	/// entry of THEIR type instead, which can be a different one - this mask does not belong to
	/// those and must not be read alongside them.
	/// </remarks>
	public static ulong BMRNextDamagePlayers { get; set; }

	/// <summary>
	/// Whether the next predicted damage event includes the player.
	/// </summary>
	/// <remarks>
	/// Bit 0, and that position is fixed rather than a guess: BossModReborn's <c>PartyState</c>
	/// declares <c>PlayerSlot = 0</c>, so the player is always the first slot of its party list and
	/// does not move with party order. The remaining bits - 1..7 party, 8..23 alliance, 24..63 other
	/// allies - would need a mapping from BossMod's slot order to this tree's party list, which is
	/// another unverified contract across the IPC boundary and is deliberately not made.
	/// </remarks>
	public static bool BMRNextDamageHitsPlayer { get; set; }

	/// <summary>
	/// BMR predicts a tankbuster inside the user's single-target mitigation window.
	/// </summary>
	public static bool BMRTankbusterImminent =>
		Service.Config.UseBmrTimeline && BMRNextTankbusterIn > 0.6f && BMRNextTankbusterIn <= Service.Config.BMRTankbusterMitWindow;
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
	/// True if BossMod is moving.
	/// </summary>
	public static bool BMRIsMoving { get; set; }

	// Debug diagnostics
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
	public static string? BMRDebugTimelineWalk { get; set; }

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
	/// Delegate wired up by BossModUpdater to the <c>Hints.IsFixedDashSafe</c> IPC endpoint.
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
				PluginLog.Information($"GetActivePlannedAction: Found active planned action {action.ActionId} with ActivationIn {action.ActivationIn} and WindowEndIn {action.WindowEndIn}");
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
		BMRNextTankbusterIn = float.MaxValue;
		BMRNextKnockbackIn = float.MaxValue;
		BMRNextDowntimeIn = float.MaxValue;
		BMRNextDowntimeEndIn = float.MaxValue;
		BMRNextVulnerableIn = float.MaxValue;
		BMRNextVulnerableEndIn = float.MaxValue;
		BMRNextDamageIn = float.MaxValue;
		BMRNextDamageType = PredictedDamageType.None;
		BMRSpecialModeIn = float.MaxValue;
		BMRSpecialModeType = SpecialMode.Normal;
		BMRForceCancelCast = false;
		BMRForceCancelCastAI = false;
		BMRIsMoving = false;
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
		BMRDebugTimelineWalk = null;
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