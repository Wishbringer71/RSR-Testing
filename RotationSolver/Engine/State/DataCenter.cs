using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Lumina.Excel.Sheets;
using System.Collections.Frozen;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Concurrent;
using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.Engine.State;

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

	public unsafe static int PlayerCurrentLevel
	{
		get
		{
			var playerState = PlayerState.Instance();
			return playerState == null ? 0 : playerState->CurrentLevel;
		}
	}

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
	internal static readonly Lock _nameIdLock = new();

	internal static void CopyBlacklistedNameIds(HashSet<uint> destination)
	{
		lock (_nameIdLock)
		{
			if (BlacklistedNameIds == null)
			{
				return;
			}

			for (var i = 0; i < BlacklistedNameIds.Count; i++)
			{
				_ = destination.Add(BlacklistedNameIds[i]);
			}
		}
	}

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
	internal static readonly Lock _attackedTargetsLock = new();
	private static readonly Lock _nextActsLock = new();

	internal static void CopyTankbusterTargetIds(List<ulong> destination)
	{
		destination.Clear();
		lock (_tankbusterLock)
		{
			destination.AddRange(TankbusterTargetIds);
		}
	}

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

	private static long _effectEndTick = long.MinValue;

	internal static void StartEffectTime(float seconds)
	{
		_effectEndTick = Environment.TickCount64 + (long)(seconds * 1000);
	}

	internal static int AttackedTargetsCount { get; set; } = 48;
	internal static Queue<(ulong id, DateTime time)> AttackedTargets { get; } = new(AttackedTargetsCount);

	internal static Queue<MacroItem> Macros { get; } = new Queue<MacroItem>();

	internal static bool InEffectTime => Environment.TickCount64 <= _effectEndTick;
	internal static Dictionary<ulong, uint> HealHP { get; set; } = [];

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
			lock (_nextActsLock)
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
	}

	internal static CommandTarget? GetCommandTarget(IAction act)
	{
		lock (_nextActsLock)
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
	}

	internal static void AddCommandAction(IAction act, double time, CommandTarget? target = null)
	{
		lock (_nextActsLock)
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

			var types = Service.Config.TargetingTypes;
			return types[((Service.Config.TargetingIndex % types.Count) + types.Count) % types.Count];
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
			if (manager == null || manager->Combo.Timer <= 0)
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

	public static unsafe float ComboTime
	{
		get
		{
			var manager = ActionManager.Instance();
			return manager == null ? 0f : manager->Combo.Timer;
		}
	}

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
}
