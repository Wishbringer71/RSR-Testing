namespace RotationSolver.Basic.Actions;



public partial struct ActionTargetInfo
{
	// "Don't pull new enemies with AoE": an enemy the area reaches that holds no target is not fighting yet (A826).
	private static bool PullsNewHostile(IBattleChara enemy) => Service.Config.NoNewHostiles && enemy.TargetObject == null;

	private static bool PullsNewHostile(List<IBattleChara> affected)
	{
		if (!Service.Config.NoNewHostiles)
		{
			return false;
		}

		for (var i = 0; i < affected.Count; i++)
		{
			if (PullsNewHostile(affected[i]))
			{
				return true;
			}
		}

		return false;
	}
}
