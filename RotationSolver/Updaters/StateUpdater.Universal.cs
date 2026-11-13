using ECommons.GameFunctions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Updaters;

internal static partial class StateUpdater
{
	/// <summary>
	/// Whether every member a tankbuster is now being cast at stands under Hallowed Ground or
	/// Superbolide past the hit. False when no target is known, and whenever a tankbuster marker
	/// stands: a marker states no landing time, so whether the invulnerability outlasts it is unknown.
	/// </summary>
	private static bool TankbusterTargetsAllImpervious()
	{
		if (DataCenter.IsCastingTankVfx())
		{
			return false;
		}

		// Over the same casters as the tankbuster flag, helpers included: a helper's tankbuster at a member who is not
		// covered must keep the mitigation (concept 09, A750).
		var any = false;
		foreach (var hostile in DataCenter.AreaCasters)
		{
			if (hostile == null || !hostile.IsCasting || !DataCenter.IsCastingTankbuster(hostile))
			{
				continue;
			}

			// One GCD on top of the cast for the hit to arrive, as everywhere a cast's hit is timed.
			var hitIn = hostile.TotalCastTime - hostile.CurrentCastTime + DataCenter.DefaultGCDTotal;
			if (Svc.Objects.SearchById(hostile.CastTargetObjectId) is not IBattleChara target
				|| !target.ImperviousThrough(hitIn))
			{
				return false;
			}

			any = true;
		}

		return any;
	}
}
