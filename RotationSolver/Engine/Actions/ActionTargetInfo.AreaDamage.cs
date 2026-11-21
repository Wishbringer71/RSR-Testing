namespace RotationSolver.Engine.Actions;



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
		var healsToo = ActionTooltip.HealsToo(action.ID);
		if (centre == null || action.Setting.IsFriendly || !DataCenter.AreaMaySparePull(healsToo))
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
				|| !DataCenter.NotFightingYet(t) || !t.IsEnemy() || !DataCenter.PullsIfHit(t, healsToo))
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

	/// <summary>
	/// Concept 03, "F · Über den Augenblick hinaus", K5 (A917): whether an enemy in an area attack's reach still lives when
	/// the hit lands - its estimated time to kill against the cast time plus, for a GCD, the rest of the running GCD. An
	/// enemy that falls before counts toward no area threshold. Without an estimate, for a single target and for friendly
	/// actions it counts as before.
	/// </summary>
	private readonly bool LivesUntilItLands(IBattleChara enemy)
	{
		if (action.Setting.IsFriendly || IsSingleTarget || EffectRange <= 0)
		{
			return true;
		}

		var time = enemy.GetTTK();
		return float.IsNaN(time) || time >= action.Info.NeededStatusMargin;
	}
}
