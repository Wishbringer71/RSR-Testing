using RotationSolver.Basic.Tweaks;

namespace RotationSolver.Basic.Helpers;

public sealed partial class ActionManagerEx
{
	/// <summary>
	/// The lock tweak of the running instance, or null while none runs - without creating one, as <see cref="Instance"/>
	/// would while the plugin unloads (concept 03, "Weben, Animationssperre und Latenz", A753).
	/// </summary>
	internal static AnimationLockTweak? CurrentLockTweak => _instance?.AnimationLockTweak;
}
