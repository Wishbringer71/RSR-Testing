using ECommons.EzIpcManager;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace RotationSolver.IPC;

internal static class BMRInfo_IPCSubscriber
{
	private static readonly EzIPCDisposalToken[] _disposalTokens =
		EzIPC.Init(typeof(BMRInfo_IPCSubscriber), "BossMod", SafeWrapper.AnyException);

	internal static bool IsEnabled => IPCSubscriber_Common.IsReady("BossModReborn");

	[EzIPC("Hints.ForceCancelCastMechanic", true)]
	internal static readonly Func<bool>? ForceCancelCast;

	[EzIPC("Hints.ForceCancelCastMechanicAI", true)]
	internal static readonly Func<bool>? ForceCancelCastAI;

	[EzIPC("AI.IsNavigating", true)]
	internal static readonly Func<bool>? IsNavigating;

	[EzIPC("Movement.IsMoving", true)]
	internal static readonly Func<bool>? IsMoving;

	[EzIPC("Hints.MaxCastTime", true)]
	internal static readonly Func<float>? MaxCastTime;

	// Whether the player holds a movement key; BossMod then sets MaxCastTime to zero (AIHintsBuilder.Update).
	[EzIPC("Movement.IsMoveRequested", true)]
	internal static readonly Func<bool>? IsMoveRequested;

	// Seconds until the earliest forbidden zone anywhere turns active; float.MaxValue without one.
	[EzIPC("Hints.ForbiddenZonesNextActivation", true)]
	internal static readonly Func<float>? ForbiddenZonesNextActivation;

	// Where BossMod's AI steers the player; null when it does not.
	[EzIPC("AI.NaviTargetPos", true)]
	internal static readonly Func<Vector3?>? NaviTargetPos;

	// The player's movement speed as BossMod's AI reads it, in yalms per second.
	[EzIPC("AI.PlayerSpeed", true)]
	internal static readonly Func<float>? PlayerSpeed;

	internal static void Dispose() => IPCSubscriber_Common.DisposeAll(_disposalTokens);
}
