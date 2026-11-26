using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using ECommons.DalamudServices;
using ECommons.Logging;
using RotationSolver.Integrations.IPC;

namespace RotationSolver.Engine.State;

internal static partial class DataCenter
{

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

	/// <summary>The first error the enemy effect handler raised since loading, for the windows.</summary>
	public static string EffectHandlerFirstError { get; internal set; } = string.Empty;

	/// <summary>
	/// Every strategic hold of a defense that was asked (CustomRotation_DefenseHold), by rule: whether
	/// it last held or yielded to a member in danger, why, and when. One entry per rule, so a rule
	/// asked after another in the same frame does not hide it. Shown in the diagnostics window,
	/// because a defense that waits cannot otherwise be told apart from one that was never asked.
	/// </summary>
	internal static readonly ConcurrentDictionary<string, DefenseHoldDecision> DefenseHolds = new();

	/// <summary>
	/// Whether the last landing of each listed area cast reached the player, for this session. Written
	/// by the effect handler, read by <see cref="IsAnyHostileCastingAreaReachingMe"/> under
	/// "Skip area defence for casts that missed you" (A205).
	/// </summary>
	internal static readonly ConcurrentDictionary<uint, bool> AreaCastReachedPlayer = new();

	#region BossModReborn Timeline Integration
	private static readonly FrameCached<bool> _bmrEnabled = new(() => IsPluginLoaded("BossModReborn", "BossModReborn"), 1000);
	private static readonly FrameCached<bool> _autoPvpSeriesGrindEnabled = new(() => IsPluginLoaded("Auto PVP Series Grind", "AutoPvpSeriesGrind"), 1000);

	public static bool BMREnabled => _bmrEnabled.Value;

	public static bool AutoPvpSeriesGrindEnabled => _autoPvpSeriesGrindEnabled.Value;

	internal static bool IsPluginLoaded(string name, string internalName)
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
	/// Delegate wired up by BossModUpdater to BossModReborn's <c>Hints.IsFixedDashSafe</c>: is a dash of the given
	/// range along the player's facing (backwards when the flag says so) safe? BossModReborn computes the landing
	/// point from its own copy of the player's position and facing and asks its dash check about that path (A313,
	/// A971). When null, BossModReborn is not available and all fixed dashes are considered safe.
	/// </summary>
	public static Func<float, bool, bool>? BMRIsFixedDashSafe { get; set; }

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
	/// Returns true if a dash of <paramref name="range"/> yalms along the player's facing - backwards when
	/// <paramref name="backwards"/> is set - is safe, or if BossModReborn IPC is unavailable.
	/// </summary>
	public static bool IsFixedDashSafe(float range, bool backwards)
	{
		if (BMRIsFixedDashSafe == null)
		{
			return true;
		}

		try
		{ return BMRIsFixedDashSafe(range, backwards); }
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

	#region BossModReborn Crucible Kinships
	private const long BmrCrucibleOpponentsRefreshMs = 10000;
	private const long BmrActiveModuleTypeRefreshMs = 1000;

	private static List<BMRCrucibleOpponent> _bmrCrucibleOpponents = [];
	private static long _bmrCrucibleOpponentsTick = -BmrCrucibleOpponentsRefreshMs;
	private static string? _bmrActiveModuleType;
	private static long _bmrActiveModuleTypeTick = -BmrActiveModuleTypeRefreshMs;

	/// <summary>
	///
	/// </summary>
	public static IReadOnlyList<BMRCrucibleOpponent> BMRCrucibleOpponents
	{
		get
		{
			var now = Environment.TickCount64;
			if (now - _bmrCrucibleOpponentsTick >= BmrCrucibleOpponentsRefreshMs)
			{
				_bmrCrucibleOpponentsTick = now;
				_bmrCrucibleOpponents = BMRCrucible_IPCSubscriber.IsEnabled ? BMRCrucible_IPCSubscriber.GetCrucibleOpponents() : [];
			}

			return _bmrCrucibleOpponents;
		}
	}

	/// <summary>
	///
	/// </summary>
	public static string? BMRActiveModuleType
	{
		get
		{
			var now = Environment.TickCount64;
			if (now - _bmrActiveModuleTypeTick >= BmrActiveModuleTypeRefreshMs)
			{
				_bmrActiveModuleTypeTick = now;
				try
				{
					_bmrActiveModuleType = BMRCrucible_IPCSubscriber.IsEnabled ? BMRCrucible_IPCSubscriber.ActiveModuleType?.Invoke() : null;
				}
				catch
				{
					_bmrActiveModuleType = null;
				}
			}

			return _bmrActiveModuleType;
		}
	}
	#endregion
}
