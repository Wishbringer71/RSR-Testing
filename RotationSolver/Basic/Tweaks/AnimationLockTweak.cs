using ECommons.Logging;

namespace RotationSolver.Basic.Tweaks;

/// <summary>
/// Effective animation lock reduction tweak (a-la xivalex/noclippy).
/// The game handles instants and casted actions differently:
/// * instants: on action request (e.g. on the frame the action button is pressed), animation lock is set to 0.5 (or 0.35 for some specific actions); it then ticks down every frame
///   some time later (ping + server latency, typically 50-100ms if ping is good), we receive action effect packet - the packet contains action's animation lock (typically 0.6)
///   the game then updates animation lock (now equal to 0.5 minus time since request) to the packet data
///   so the 'effective' animation lock between action request and animation lock end is equal to action's animation lock + delay between request and response
///   this tweak reduces effective animation lock by either removing extra delay completely or clamping it to specified min/max values
/// * casts: on action request animation lock is not set (remains equal to 0), remaining cast time is set to action's cast time; remaining cast time then ticks down every frame
///   some time later (cast time minus approximately 0.5s, aka slidecast window), we receive action effect packet - the packet contains action's animation lock (typically 0.1)
///   the game then updates animation lock (still 0) to the packet data - however, since animation lock isn't ticking down while cast is in progress, there is no extra delay
///   this tweak does nothing for casts, since they already work correctly
/// The tweak also allows predicting the delay based on history (using exponential average).
/// </summary>
public sealed unsafe partial class AnimationLockTweak
{
	private float _lastReqInitialAnimLock;
	private uint _lastReqSequence = uint.MaxValue;

	/// <summary>
	/// Gets the mean delay between client request and server response in the current zone (A753: measured, no set value).
	/// </summary>
	public float DelayAverage { get; private set; }

	/// <summary>
	/// Gets the rest an instant action is expected to keep after its lock now: what remains of the delay after the
	/// reduction, mean plus mean deviation; 0 before the first measurement (concept 03, "Weben, Animationssperre und
	/// Latenz", A753).
	/// </summary>
	public float DelayEstimate => KeptEstimate;

	/// <summary>
	/// Record initial animation lock after action request
	/// </summary>
	/// <param name="expectedSequence">Expected sequence number for the action</param>
	/// <param name="initialAnimLock">Initial animation lock value</param>
	public void RecordRequest(uint expectedSequence, float initialAnimLock)
	{
		_lastReqInitialAnimLock = initialAnimLock;
		_lastReqSequence = expectedSequence;
	}

	/// <summary>
	/// Apply the tweak: calculate animation lock delay and determine how much animation lock should be reduced
	/// </summary>
	/// <param name="sequence">Action sequence number</param>
	/// <param name="gamePrevAnimLock">Previous animation lock from game</param>
	/// <param name="gameCurrAnimLock">Current animation lock from game</param>
	/// <param name="packetPrevAnimLock">Previous animation lock from packet</param>
	/// <param name="packetCurrAnimLock">Current animation lock from packet</param>
	/// <param name="delay">Output: calculated delay</param>
	/// <returns>Animation lock reduction amount</returns>
	public float Apply(uint sequence, float gamePrevAnimLock, float gameCurrAnimLock, float packetPrevAnimLock, float packetCurrAnimLock, out float delay)
	{
		delay = Math.Max(0, _lastReqInitialAnimLock - gamePrevAnimLock);

		if (_lastReqSequence != sequence && gameCurrAnimLock != gamePrevAnimLock)
		{
			if (Service.Config.InDebug)
			{
				PluginLog.Debug($"[AnimLockTweak] Animation lock updated by action with unexpected sequence ID #{sequence}: {gamePrevAnimLock:f3} -> {gameCurrAnimLock:f3}");
			}
		}

		float reduction = 0;

		// Whether this answer belongs to the request recorded last: only then is the delay measured (A753).
		LastMatched = _lastReqSequence == sequence && _lastReqInitialAnimLock > 0;
		if (LastMatched)
		{
			SanityCheck(packetPrevAnimLock, packetCurrAnimLock, gameCurrAnimLock);
			// The controller's cut for this answer, from the zone's earlier answers, then this one learned (A753).
			var cut = Learn(delay);
			DelayAverage = _delayMean;

			reduction = ReductionAllowed ? Math.Clamp(Math.Min(cut, delay), 0, gameCurrAnimLock) : 0;
		}

		_lastReqInitialAnimLock = 0;
		_lastReqSequence = uint.MaxValue;
		return reduction;
	}

	private void SanityCheck(float packetOriginalAnimLock, float packetModifiedAnimLock, float gameCurrAnimLock)
	{
		if (!Service.Config.RemoveAnimationLockDelay || _heldInZone)
		{
			return;
		}

		// The game's lock after the packet must equal the packet's too, as in BossModReborn: a plugin that shortens the
		// lock between the two (BossMod's own tweak, NoClippy) left it unseen, and the lock was cut twice (A753).
		if (packetOriginalAnimLock == packetModifiedAnimLock && packetOriginalAnimLock == gameCurrAnimLock
			&& (packetOriginalAnimLock % 0.01f) is <= 0.0005f or >= 0.0095f)
		{
			return;
		}

		PluginLog.Warning($"[AnimLockTweak] Unexpected animation lock {packetOriginalAnimLock:f6} -> {packetModifiedAnimLock:f6} -> {gameCurrAnimLock:f6}, holding the animation lock reduction in this zone");
		DefenseTrace.Line($"animation lock reduction held in this zone: the lock was {packetOriginalAnimLock:F3} s in the packet,"
			+ $" {packetModifiedAnimLock:F3} s after it and {gameCurrAnimLock:F3} s in the game - another tool changes it");

		// Held until the next zone, the player's setting untouched: the cause may be gone then, as BossModReborn keeps it
		// unsaved for the same reason (A753).
		_heldInZone = true;
	}
}
