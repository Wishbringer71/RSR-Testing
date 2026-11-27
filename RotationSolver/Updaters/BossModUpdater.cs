using ECommons.Logging;
using RotationSolver.Integrations.IPC;

namespace RotationSolver.Updaters;

internal static class BossModUpdater
{
	private const long ErrorLogIntervalMs = 10000;
	private static readonly Dictionary<string, long> _lastErrorTicks = [];

	private static readonly Func<Vector3, bool> _safeIsPositionSafe = pos =>
	{
		var func = BMRTimeline_IPCSubscriber.IsPositionSafe;
		if (func == null)
		{
			return true;
		}

		try
		{
			return func(pos);
		}
		catch (Exception ex)
		{
			LogIpcError("Hints.IsPositionSafe", ex);
			return true;
		}
	};

	private static readonly Func<Vector3, Vector3, bool> _safeIsDashSafe = (from, to) =>
	{
		var func = BMRTimeline_IPCSubscriber.IsDashSafe;
		if (func == null)
		{
			return true;
		}

		try
		{
			return func(from, to);
		}
		catch (Exception ex)
		{
			LogIpcError("Hints.IsDashSafe", ex);
			return true;
		}
	};

	private static readonly Func<float, bool, bool> _safeIsFixedDashSafe = (distance, forward) =>
	{
		var func = BMRTimeline_IPCSubscriber.IsFixedDashSafe;
		if (func == null)
		{
			return true;
		}

		try
		{
			return func(distance, forward);
		}
		catch (Exception ex)
		{
			LogIpcError("Hints.IsFixedDashSafe", ex);
			return true;
		}
	};

	private static void LogIpcError(string endpoint, Exception ex)
	{
		var now = Environment.TickCount64;
		if (_lastErrorTicks.TryGetValue(endpoint, out var lastTick) && now - lastTick < ErrorLogIntervalMs)
		{
			return;
		}

		_lastErrorTicks[endpoint] = now;
		PluginLog.Debug($"[BossModUpdater] IPC '{endpoint}' failed: {ex.Message}");
	}

	private static T SafeCall<T>(Func<T>? func, T fallback, string endpoint)
	{
		if (func == null)
		{
			return fallback;
		}

		try
		{
			return func();
		}
		catch (Exception ex)
		{
			LogIpcError(endpoint, ex);
			return fallback;
		}
	}

	private static float NormalizeTimer(float value)
	{
		return float.IsFinite(value) && value > 0f ? value : float.MaxValue;
	}

	public static void Update()
	{
		// All BMR subscribers target the same plugin; the readiness check is cached in
		// IPCSubscriber_Common.IsReady, so polling it each update picks up BossMod being enabled or
		// disabled after RSR has started. This fork used to keep a five-second interval of its own
		// here for exactly that reason - with the cache now on the other side, that was a second
		// cache stacked on the first, and ResetAvailabilityCheck went with it.
		if (!BMRTimeline_IPCSubscriber.IsEnabled)
		{
			DataCenter.ResetBmrData();
			return;
		}

		try
		{
			DataCenter.BMRHasActiveModule = SafeCall(BMRTimeline_IPCSubscriber.HasActiveModule, false, "HasActiveModule");

			DataCenter.BMRForceCancelCast = SafeCall(BMRInfo_IPCSubscriber.ForceCancelCast, false, "ForceCancelCast");
			DataCenter.BMRForceCancelCastAI = SafeCall(BMRInfo_IPCSubscriber.ForceCancelCastAI, false, "ForceCancelCastAI");
			DataCenter.BMRIsMoving = SafeCall(BMRInfo_IPCSubscriber.IsMoving, false, "IsMoving");
			DataCenter.BMRMaxCastTime = SafeCall(BMRInfo_IPCSubscriber.MaxCastTime, float.MaxValue, "MaxCastTime");
			DataCenter.BMRMoveRequested = SafeCall(BMRInfo_IPCSubscriber.IsMoveRequested, false, "IsMoveRequested");
			DataCenter.BMRIsNavigating = SafeCall(BMRInfo_IPCSubscriber.IsNavigating, false, "IsNavigating");
			DataCenter.BMRNaviTargetPos = SafeCall<System.Numerics.Vector3?>(BMRInfo_IPCSubscriber.NaviTargetPos, null, "NaviTargetPos");
			DataCenter.BMRPlayerSpeed = SafeCall(BMRInfo_IPCSubscriber.PlayerSpeed, 0f, "PlayerSpeed");
			DataCenter.BMRForbiddenZonesNextActivation = SafeCall(BMRInfo_IPCSubscriber.ForbiddenZonesNextActivation, float.MaxValue, "ForbiddenZonesNextActivation");

			DataCenter.BMRActiveModuleName = SafeCall(BMRTimeline_IPCSubscriber.ActiveModuleName, (string?)null, "ActiveModuleName");

			DataCenter.BMRDebugTimelineRwFunc = BMRTimeline_IPCSubscriber.NextRaidwideIn != null;
			DataCenter.BMRDebugTimelineTbFunc = BMRTimeline_IPCSubscriber.NextTankbusterIn != null;
			DataCenter.BMRDebugHintsRwFunc = BMRTimeline_IPCSubscriber.NextRaidwideDamageIn != null;
			DataCenter.BMRDebugHintsTbFunc = BMRTimeline_IPCSubscriber.NextTankbusterDamageIn != null;

			var timelineRaidwide = SafeCall(BMRTimeline_IPCSubscriber.NextRaidwideIn, float.MaxValue, "Timeline.NextRaidwideIn");
			var timelineTankbuster = SafeCall(BMRTimeline_IPCSubscriber.NextTankbusterIn, float.MaxValue, "Timeline.NextTankbusterIn");
			DataCenter.BMRNextKnockbackIn = NormalizeTimer(SafeCall(BMRTimeline_IPCSubscriber.NextKnockbackIn, float.MaxValue, "Timeline.NextKnockbackIn"));
			DataCenter.BMRNextDowntimeIn = SafeCall(BMRTimeline_IPCSubscriber.NextDowntimeIn, float.MaxValue, "Timeline.NextDowntimeIn");
			DataCenter.BMRNextDowntimeEndIn = SafeCall(BMRTimeline_IPCSubscriber.NextDowntimeEndIn, float.MaxValue, "Timeline.NextDowntimeEndIn");
			DataCenter.BMRNextVulnerableIn = SafeCall(BMRTimeline_IPCSubscriber.NextVulnerableIn, float.MaxValue, "Timeline.NextVulnerableIn");
			DataCenter.BMRNextVulnerableEndIn = SafeCall(BMRTimeline_IPCSubscriber.NextVulnerableEndIn, float.MaxValue, "Timeline.NextVulnerableEndIn");
			// Downtime and vulnerability as BossModReborn gives them: their readers take 0 as "now" (StatusHelper.TankProtection);
			// upstream c79f584a5 reads them through NormalizeTimer, which would turn it into "never".
			// Debug values keep the raw reading; the merge below works on the normalised ones.
			DataCenter.BMRDebugTimelineRaidwide = timelineRaidwide;
			DataCenter.BMRDebugTimelineTankbuster = timelineTankbuster;
			timelineRaidwide = DiscardPastEvent(timelineRaidwide);
			timelineTankbuster = DiscardPastEvent(timelineTankbuster);

			var damageIn = NormalizeTimer(SafeCall(BMRTimeline_IPCSubscriber.NextDamageIn, float.MaxValue, "Hints.NextDamageIn"));
			var damageType = SafeCall(BMRTimeline_IPCSubscriber.NextDamageType, 0, "Hints.NextDamageType");
			DataCenter.BMRNextDamageIn = damageIn;
			DataCenter.BMRNextDamageType = Enum.IsDefined(typeof(PredictedDamageType), damageType) ? (PredictedDamageType)damageType : PredictedDamageType.None;

			// Same entry as the two above - PredictedDamage[0] - so the three describe one event.
			// Bit 0 is the player (BossMod PartyState.PlayerSlot = 0).
			var predictedPlayers = SafeCall(BMRTimeline_IPCSubscriber.PredictedDamagePlayers, 0ul, "Timeline.PredictedDamagePlayers");
			DataCenter.BMRNextDamagePlayers = predictedPlayers;
			DataCenter.BMRNextDamageHitsPlayer = (predictedPlayers & 1ul) != 0ul;
			DataCenter.BMRDebugGenericDamageIn = damageIn;
			DataCenter.BMRDebugGenericDamageType = damageType;

			var hintsRaidwide = NormalizeTimer(SafeCall(BMRTimeline_IPCSubscriber.NextRaidwideDamageIn, float.MaxValue, "Hints.NextRaidwideDamageIn"));
			var hintsTankbuster = NormalizeTimer(SafeCall(BMRTimeline_IPCSubscriber.NextTankbusterDamageIn, float.MaxValue, "Hints.NextTankbusterDamageIn"));
			DataCenter.BMRDebugHintsRaidwide = hintsRaidwide;
			DataCenter.BMRDebugHintsTankbuster = hintsTankbuster;

			hintsRaidwide = DiscardPastEvent(hintsRaidwide);
			hintsTankbuster = DiscardPastEvent(hintsTankbuster);

			var genericRaidwide = (damageType == 2 && damageIn > 0f) ? damageIn : float.MaxValue;
			var genericTankbuster = (damageType == 1 && damageIn > 0f) ? damageIn : float.MaxValue;

			// BossModReborn calls a prediction "Raidwide" when it is "expected to hit everyone and deal minor
			// damage; also used for spread components", and it is the type of every prediction a component
			// adds without naming one - towers on their soakers, baits on the baited, a charge on whoever
			// stands in its line (AIHints.AddPredictedDamage and its callers, BossModReborn be8ff77,
			// 13.10.2026). Each entry names whom it hits. That mask is readable for entry [0] only; when [0]
			// is a raidwide it is also the first raidwide, the one the raidwide endpoint returns, because the
			// list is sorted by activation. Slots 0..7 are the party, slot 0 the player (PartyState). One that
			// reaches neither the player nor more than one of the party is no area hit on this party -
			// concept 13's reading of a cast (A218), and the reading A220 gave the tankbuster (A273).
			var raidwideMaskKnown = damageType == (int)PredictedDamageType.Raidwide && damageIn > 0f;
			var raidwideHitsPlayer = (predictedPlayers & 1ul) != 0ul;
			var raidwideReachesParty = raidwideHitsPlayer || BitOperations.PopCount((uint)(byte)predictedPlayers) > 1;
			if (raidwideMaskKnown && !raidwideReachesParty)
			{
				hintsRaidwide = float.MaxValue;
				genericRaidwide = float.MaxValue;
			}

			DataCenter.BMRNextRaidwideIn = Math.Min(Math.Min(timelineRaidwide, hintsRaidwide), genericRaidwide);
			DataCenter.BMRNextRaidwideSource = DataCenter.BMRNextRaidwideIn == float.MaxValue ? string.Empty
				: DataCenter.BMRNextRaidwideIn == timelineRaidwide ? "timeline"
				: DataCenter.BMRNextRaidwideIn == hintsRaidwide ? "hints"
				: "generic";
			DataCenter.NoteBmrRaidwide(DataCenter.BMRNextRaidwideIn);

			// Known only when the raidwide in use is that entry; the timeline's raidwide names nobody.
			DataCenter.BMRRaidwideHitsPlayer = raidwideMaskKnown && hintsRaidwide < float.MaxValue
				&& DataCenter.BMRNextRaidwideIn == hintsRaidwide
				? raidwideHitsPlayer
				: null;
			DataCenter.BMRNextTankbusterIn = Math.Min(Math.Min(timelineTankbuster, hintsTankbuster), genericTankbuster);

			DataCenter.BMRSpecialModeIn = NormalizeTimer(SafeCall(BMRTimeline_IPCSubscriber.SpecialModeIn, float.MaxValue, "Hints.SpecialModeIn"));
			var specialModeType = SafeCall(BMRTimeline_IPCSubscriber.SpecialModeType, 0, "Hints.SpecialModeType");
			DataCenter.BMRSpecialModeType = Enum.IsDefined(typeof(SpecialMode), specialModeType) ? (SpecialMode)specialModeType : SpecialMode.Normal;

			DataCenter.BMRIsPositionSafe = BMRTimeline_IPCSubscriber.IsPositionSafe != null ? _safeIsPositionSafe : null;
			DataCenter.BMRIsDashSafe = BMRTimeline_IPCSubscriber.IsDashSafe != null ? _safeIsDashSafe : null;
			DataCenter.BMRIsFixedDashSafe = BMRTimeline_IPCSubscriber.IsFixedDashSafe != null ? _safeIsFixedDashSafe : null;
		}
		catch (Exception ex)
		{
			LogIpcError("Update", ex);
			DataCenter.ResetBmrData();
		}
	}

	/// <summary>
	/// Normalises a reading for an upcoming damage event to "no prediction".
	/// </summary>
	/// <remarks>
	/// BossModReborn computes every one of these endpoints as (activation - now) in seconds and only
	/// reports float.MaxValue when it has nothing to predict, so the value runs through zero into
	/// negative numbers while its state machine catches up with an event that has already landed.
	/// Every consumer here requires more than 0.6s, so such a reading carries no meaning of its own -
	/// but a raw one does win the Math.Min merge against a valid prediction from another source,
	/// silently dropping the mitigation for the event that follows.
	/// <para>
	/// This is only correct for damage events. Downtime and vulnerability windows keep their raw
	/// value: there the sign is the information, telling a window still ahead from one already
	/// running.
	/// </para>
	/// </remarks>
	private static float DiscardPastEvent(float seconds) => seconds <= 0f ? float.MaxValue : seconds;
}