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

		// Over the list itself, not an iterator: this runs for every area evaluated in a frame. The cheap reads first; the
		// enemy test reads the game's object and only for a figure in reach that is not fighting.
		List<IBattleChara>? caught = null;
		foreach (var t in DataCenter.AllTargets)
		{
			if (t == null || t.GameObjectId == centre.GameObjectId || t.IsDead
				|| (aroundPlayer && t.DistanceToPlayer() > EffectRange)
				|| !DataCenter.NotFightingYet(t) || !t.IsEnemy())
			{
				continue;
			}

			if (!aroundPlayer)
			{
				if (GetCanTarget(centre, t))
				{
					DeepDungeonCrowd.TraceWait(action.ID, t);
					return true;
				}

				continue;
			}

			(caught ??= []).Add(t);
		}

		if (caught == null)
		{
			return false;
		}

		var reached = FilterAffectsByFacing(caught);
		if (reached.Count == 0)
		{
			return false;
		}

		DeepDungeonCrowd.TraceWait(action.ID, reached[0]);
		return true;
	}
}
