namespace RotationSolver.Basic.Actions;



public partial struct ActionTargetInfo
{
	/// <summary>
	/// "Don't pull new enemies" (concept 03, A826; concept 23, A834): whether this attack's area, centred on
	/// <paramref name="centre"/> - around the player with <paramref name="aroundPlayer"/> -, would reach an enemy that is not
	/// fighting yet. Read over every enemy figure, not the list of targets: an engage setting that skips enemies not fighting
	/// yet leaves them out of that list, while the area hits them all the same (A834).
	/// </summary>
	private readonly bool PullsAround(IBattleChara centre, bool aroundPlayer)
	{
		if (centre == null || action.Setting.IsFriendly || !DataCenter.GuardsAgainstPull(ActionTooltip.HealsToo(action.ID)))
		{
			return false;
		}

		List<IBattleChara>? caught = null;
		var all = DataCenter.AllTargets;
		foreach (var t in aroundPlayer ? all.GetObjectInRadius(EffectRange) : all)
		{
			if (t == null || t.GameObjectId == centre.GameObjectId || t.IsDead || !t.IsEnemy() || !DataCenter.NotFightingYet(t))
			{
				continue;
			}

			if (!aroundPlayer)
			{
				if (GetCanTarget(centre, t))
				{
					return true;
				}

				continue;
			}

			(caught ??= []).Add(t);
		}

		return caught != null && FilterAffectsByFacing(caught).Count > 0;
	}
}
