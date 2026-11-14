using RotationSolver.Basic.Tweaks;

namespace RotationSolver.Basic.Helpers;

public sealed partial class ActionManagerEx
{
	// A793: a fault of the action timing in UpdateDetour is written once per session, not every frame.
	private static bool _frameFaultLogged;

	/// <summary>
	/// The lock tweak of the running instance, or null while none runs - without creating one, as <see cref="Instance"/>
	/// would while the plugin unloads (concept 03, "Weben, Animationssperre und Latenz", A753).
	/// </summary>
	internal static AnimationLockTweak? CurrentLockTweak => _instance?.AnimationLockTweak;
}
