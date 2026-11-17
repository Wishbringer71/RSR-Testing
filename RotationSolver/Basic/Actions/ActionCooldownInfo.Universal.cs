namespace RotationSolver.Basic.Actions;

public readonly partial struct ActionCooldownInfo
{
	/// <summary>
	/// The part of <see cref="CooldownCheck"/> that refused last, as a fixed string <c>Cooldown:&lt;part&gt;</c>, for the reason
	/// <c>CanUse</c> reports (concept 03, "Bereit, aber nicht genutzt", A855): the animation lock after every action, no
	/// charge, charges kept for later and the recast itself all read "Cooldown" before, and the trace line of a cooldown
	/// that stood ready could not tell a lock of a frame from a hold of a minute. Game thread, like the check.
	/// </summary>
	internal static string CooldownRefusal { get; private set; } = "Cooldown";

	// Set at the start of CooldownCheck and by each named refusal.

	private static bool RefuseCooldown(string part)
	{
		CooldownRefusal = part;
		return false;
	}
}
