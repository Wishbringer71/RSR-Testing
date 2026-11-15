namespace RotationSolver.Basic.Actions;



public partial struct ActionTargetInfo
{
	private static bool PullsNewHostile(List<IBattleChara> affected)
	{
		if (!Service.Config.NoNewHostiles)
		{
			return false;
		}

		for (var i = 0; i < affected.Count; i++)
		{
			if (DataCenter.PullsIfHit(affected[i]))
			{
				return true;
			}
		}

		return false;
	}
}
