using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

public sealed partial class DRK_Reborn : DarkKnightRotation
{
	// "Use ... on lowest HP party member" with its health threshold: the lowest by points among the members under the
	// threshold. The pick used to come first and the threshold after it, so when the member with the fewest points stood
	// above it - a damage dealer with a small pool, barely hurt - the barrier went to nobody, although another member
	// was under the threshold (A283). The points measure stays: against one hit, a hit is an absolute number (concept 07).
	// The target filter is swapped in for this call only and restored afterwards, as Nascent Flash does for the Warrior.
	private static bool CanUseOnLowestBelow(IBaseAction action, float ratio, bool usedUp, out IAction? act)
	{
		var setting = action.Setting;
		var canTarget = setting.CanTarget;
		setting.CanTarget = t => canTarget(t)
			&& !t.HasStatus(false, StatusID.Transcendent)
			&& t.GetHealthRatio() <= ratio;
		try
		{
			return action.CanUse(out act, usedUp: usedUp, targetOverride: TargetType.LowHP);
		}
		finally
		{
			setting.CanTarget = canTarget;
		}
	}
}
