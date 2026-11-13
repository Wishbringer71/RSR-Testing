using ECommons.GameHelpers;

namespace RotationSolver.Updaters;

internal static partial class ActionUpdater
{
	/// <summary>
	/// Whether <see cref="CanDoAction"/> refuses on the cast or the animation lock, read with its own conditions (A744,
	/// concept 24): a cast longer than the lead, or the lock still running.
	/// </summary>
	internal static bool GateClosedByCastOrLock()
	{
		var player = Player.Object;
		return player != null && (player.TotalCastTime - DataCenter.CalculatedActionAhead > 0 || DataCenter.AnimationLock > 0f);
	}
}
