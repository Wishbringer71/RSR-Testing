namespace RotationSolver.Basic.Rotations.Basic;

public partial class SamuraiRotation
{
	static partial void ModifyZantetsukenPvP(ref ActionSetting setting)
	{
		// "If target is afflicted with Kuzushi, deals damage equal to 100% of their maximum HP": chosen among the enemies that
		// carry it, not checked on whichever target the targeting type gave (concept 25, "PvP: Limit Break", A761).
		setting.TargetStatusNeed = [StatusID.Kuzushi];
	}
}
