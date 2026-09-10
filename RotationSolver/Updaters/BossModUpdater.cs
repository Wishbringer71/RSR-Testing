using RotationSolver.IPC;

namespace RotationSolver.Updaters;

internal static class BossModUpdater
{
	private static bool _isAvailable;
	private static DateTime _lastAvailabilityCheck = DateTime.MinValue;

	// BossModReborn can be enabled or disabled while RSR is already running, and Dalamud does not
	// notify us about it, so availability is re-polled on an interval instead of latched on the
	// first tick. The reflection lookup behind IsEnabled is too expensive to run every frame.
	private static readonly TimeSpan AvailabilityCheckInterval = TimeSpan.FromSeconds(5);

	public static void Update()
	{
		var now = DateTime.Now;
		if (now - _lastAvailabilityCheck >= AvailabilityCheckInterval)
		{
			_isAvailable = BMRTimeline_IPCSubscriber.IsEnabled || BMRInfo_IPCSubscriber.IsEnabled || BMRPlan_IPCSubscriber.IsEnabled;
			_lastAvailabilityCheck = now;
		}

		if (!_isAvailable)
		{
			DataCenter.ResetBmrData();
			return;
		}

		try
		{
			DataCenter.BMRHasActiveModule = BMRTimeline_IPCSubscriber.HasActiveModule?.Invoke() ?? false;

			DataCenter.BMRForceCancelCast = BMRInfo_IPCSubscriber.ForceCancelCast?.Invoke() ?? false;
			DataCenter.BMRForceCancelCastAI = BMRInfo_IPCSubscriber.ForceCancelCastAI?.Invoke() ?? false;
			DataCenter.BMRIsMoving = BMRInfo_IPCSubscriber.IsMoving?.Invoke() ?? false;

			DataCenter.BMRActiveModuleName = BMRTimeline_IPCSubscriber.ActiveModuleName?.Invoke();

			// Store whether IPC Funcs are bound (null = BMR doesn't have that endpoint)
			DataCenter.BMRDebugTimelineRwFunc = BMRTimeline_IPCSubscriber.NextRaidwideIn != null;
			DataCenter.BMRDebugTimelineTbFunc = BMRTimeline_IPCSubscriber.NextTankbusterIn != null;
			DataCenter.BMRDebugHintsRwFunc = BMRTimeline_IPCSubscriber.NextRaidwideDamageIn != null;
			DataCenter.BMRDebugHintsTbFunc = BMRTimeline_IPCSubscriber.NextTankbusterDamageIn != null;

			// Poll Timeline endpoints (state machine flags)
			var timelineRaidwide = BMRTimeline_IPCSubscriber.NextRaidwideIn?.Invoke() ?? float.MaxValue;
			var timelineTankbuster = BMRTimeline_IPCSubscriber.NextTankbusterIn?.Invoke() ?? float.MaxValue;
			DataCenter.BMRNextKnockbackIn = DiscardPastEvent(BMRTimeline_IPCSubscriber.NextKnockbackIn?.Invoke() ?? float.MaxValue);
			DataCenter.BMRNextDowntimeIn = BMRTimeline_IPCSubscriber.NextDowntimeIn?.Invoke() ?? float.MaxValue;
			DataCenter.BMRNextDowntimeEndIn = BMRTimeline_IPCSubscriber.NextDowntimeEndIn?.Invoke() ?? float.MaxValue;
			DataCenter.BMRNextVulnerableIn = BMRTimeline_IPCSubscriber.NextVulnerableIn?.Invoke() ?? float.MaxValue;
			DataCenter.BMRNextVulnerableEndIn = BMRTimeline_IPCSubscriber.NextVulnerableEndIn?.Invoke() ?? float.MaxValue;
			// Debug values keep the raw reading; the merge below works on the normalised ones.
			DataCenter.BMRDebugTimelineRaidwide = timelineRaidwide;
			DataCenter.BMRDebugTimelineTankbuster = timelineTankbuster;
			timelineRaidwide = DiscardPastEvent(timelineRaidwide);
			timelineTankbuster = DiscardPastEvent(timelineTankbuster);

			// Poll Hints endpoints (component-level damage predictions)
			var damageIn = BMRTimeline_IPCSubscriber.NextDamageIn?.Invoke() ?? float.MaxValue;
			var damageType = BMRTimeline_IPCSubscriber.NextDamageType?.Invoke() ?? 0;
			DataCenter.BMRNextDamageIn = damageIn;
			DataCenter.BMRNextDamageType = (PredictedDamageType)damageType;
			DataCenter.BMRDebugGenericDamageIn = damageIn;
			DataCenter.BMRDebugGenericDamageType = damageType;

			// Type-specific Hints endpoints
			var hintsRaidwide = BMRTimeline_IPCSubscriber.NextRaidwideDamageIn?.Invoke() ?? float.MaxValue;
			var hintsTankbuster = BMRTimeline_IPCSubscriber.NextTankbusterDamageIn?.Invoke() ?? float.MaxValue;
			DataCenter.BMRDebugHintsRaidwide = hintsRaidwide;
			DataCenter.BMRDebugHintsTankbuster = hintsTankbuster;

			hintsRaidwide = DiscardPastEvent(hintsRaidwide);
			hintsTankbuster = DiscardPastEvent(hintsTankbuster);

			// Final fallback: use generic damage prediction if type matches
			var genericRaidwide = (damageType == 2 && damageIn > 0f) ? damageIn : float.MaxValue;
			var genericTankbuster = (damageType == 1 && damageIn > 0f) ? damageIn : float.MaxValue;

			// Merge all sources: Timeline OR type-specific Hints OR generic damage prediction
			DataCenter.BMRNextRaidwideIn = Math.Min(Math.Min(timelineRaidwide, hintsRaidwide), genericRaidwide);
			DataCenter.BMRNextTankbusterIn = Math.Min(Math.Min(timelineTankbuster, hintsTankbuster), genericTankbuster);

			DataCenter.BMRSpecialModeIn = BMRTimeline_IPCSubscriber.SpecialModeIn?.Invoke() ?? float.MaxValue;
			DataCenter.BMRSpecialModeType = (SpecialMode)(BMRTimeline_IPCSubscriber.SpecialModeType?.Invoke() ?? 0);
			DataCenter.BMRDebugTimelineWalk = BMRTimeline_IPCSubscriber.DebugTimelineWalk?.Invoke();

			DataCenter.BMRIsPositionSafe = BMRTimeline_IPCSubscriber.IsPositionSafe != null
				? pos => BMRTimeline_IPCSubscriber.IsPositionSafe.Invoke(pos)
				: null;
			DataCenter.BMRIsDashSafe = BMRTimeline_IPCSubscriber.IsDashSafe != null
				? (from, to) => BMRTimeline_IPCSubscriber.IsDashSafe.Invoke(from, to)
				: null;
			DataCenter.BMRIsFixedDashSafe = BMRTimeline_IPCSubscriber.IsFixedDashSafe != null
				? (from, to) => BMRTimeline_IPCSubscriber.IsFixedDashSafe.Invoke(from, to)
				: null;
		}
		catch
		{
			DataCenter.ResetBmrData();
			ResetAvailabilityCheck();
		}
	}

	public static void ResetAvailabilityCheck()
	{
		_lastAvailabilityCheck = DateTime.MinValue;
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