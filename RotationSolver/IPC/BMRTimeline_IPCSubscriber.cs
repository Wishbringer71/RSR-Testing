using ECommons.EzIpcManager;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace RotationSolver.IPC;

internal static class BMRTimeline_IPCSubscriber
{
	private static readonly EzIPCDisposalToken[] _disposalTokens =
		EzIPC.Init(typeof(BMRTimeline_IPCSubscriber), "BossMod", SafeWrapper.AnyException);

	internal static bool IsEnabled => IPCSubscriber_Common.IsReady("BossModReborn");

	[EzIPC("HasActiveModule", true)]
	internal static readonly Func<bool>? HasActiveModule;

	[EzIPC("ActiveModuleName", true)]
	internal static readonly Func<string?>? ActiveModuleName;

	[EzIPC("Timeline.NextRaidwideIn", true)]
	internal static readonly Func<float>? NextRaidwideIn;

	[EzIPC("Timeline.NextTankbusterIn", true)]
	internal static readonly Func<float>? NextTankbusterIn;

	[EzIPC("Timeline.NextKnockbackIn", true)]
	internal static readonly Func<float>? NextKnockbackIn;

	[EzIPC("Timeline.NextDowntimeIn", true)]
	internal static readonly Func<float>? NextDowntimeIn;

	[EzIPC("Timeline.NextDowntimeEndIn", true)]
	internal static readonly Func<float>? NextDowntimeEndIn;

	[EzIPC("Timeline.NextVulnerableIn", true)]
	internal static readonly Func<float>? NextVulnerableIn;

	[EzIPC("Timeline.NextVulnerableEndIn", true)]
	internal static readonly Func<float>? NextVulnerableEndIn;

	[EzIPC("Hints.NextDamageIn", true)]
	internal static readonly Func<float>? NextDamageIn;

	[EzIPC("Hints.NextDamageType", true)]
	internal static readonly Func<int>? NextDamageType;

	// The bitmask of who the next predicted damage event hits. Belongs with the two above and only
	// with them: all three read PredictedDamage[0], the entry BossMod sorts to the front by
	// activation time. The type-specific endpoints below search for the first entry OF THEIR TYPE,
	// which can be a different one, so this mask must not be paired with those.
	//
	// Bit 0 is the player: BossMod's PartyState declares PlayerSlot = 0, so the position is fixed
	// and does not move with the party list. Bits 1..7 are the rest of the party, 8..23 the
	// alliance, 24..63 other allies - a mapping this fork does not need and therefore does not make.
	[EzIPC("Hints.PredictedDamagePlayers", true)]
	internal static readonly Func<ulong>? PredictedDamagePlayers;

	[EzIPC("Hints.NextRaidwideDamageIn", true)]
	internal static readonly Func<float>? NextRaidwideDamageIn;

	[EzIPC("Hints.NextTankbusterDamageIn", true)]
	internal static readonly Func<float>? NextTankbusterDamageIn;

	[EzIPC("Debug.TimelineWalk", true)]
	internal static readonly Func<string?>? DebugTimelineWalk;

	[EzIPC("Hints.SpecialModeIn", true)]
	internal static readonly Func<float>? SpecialModeIn;

	[EzIPC("Hints.SpecialModeType", true)]
	internal static readonly Func<int>? SpecialModeType;

	/// <summary>
	/// Returns true if the destination position (XZ) is safe to move to.
	/// </summary>
	[EzIPC("Hints.IsPositionSafe", true)]
	internal static readonly Func<Vector3, bool>? IsPositionSafe;

	/// <summary>
	/// Returns true if the dash from <c>from</c> to <c>to</c> is safe.
	/// </summary>
	[EzIPC("Hints.IsDashSafe", true)]
	internal static readonly Func<Vector3, Vector3, bool>? IsDashSafe;

	/// <summary>
	/// Returns true if a fixed-distance dash (destination determined by the game) from <c>from</c> to <c>to</c> is safe.
	/// Unlike <see cref="IsDashSafe"/>, this is used for dashes whose endpoint is not freely chosen by the player.
	/// </summary>
	[EzIPC("Hints.IsFixedDashSafe", true)]
	internal static readonly Func<Vector3, Vector3, bool>? IsFixedDashSafe;

	internal static void Dispose() => IPCSubscriber_Common.DisposeAll(_disposalTokens);
}