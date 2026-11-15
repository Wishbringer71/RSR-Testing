namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Concept 03, "Dritte Invariante" (A827): at the start of each pass, every figure RSR still holds from an earlier frame
/// that is no longer the figure of its object table slot is dropped, before anything reads it. Within a pass no figure
/// leaves the table, so what remains is valid for the pass. Lists that the pass rebuilds lose nothing they would keep;
/// lists read without being rebuilt - RSR off, a skipped pass - no longer hand out a figure that is gone.
/// </summary>
internal static class StaleFigures
{
	internal static void Prune()
	{
		Prune(DataCenter.PartyMembers);
		Prune(DataCenter.AllianceMembers);
		Prune(DataCenter.HotAgainstDotMembers);
		Prune(DataCenter.HealToDefeatTargets);
		Prune(DataCenter.SpikedHostiles);
		Prune(DataCenter.UntouchableHostiles);
		Prune(DataCenter.AllHostileTargets);
		Prune(DataCenter.AreaCasters);
		Prune(DataCenter.InterruptCandidates);
		Prune(DataCenter.ProvokeCandidates);
		Prune(DataCenter.AllTargets);
		Prune(DataCenter.CastingEnemies);
		foreach (var list in DataCenter.TargetsByRange.Values)
		{
			Prune(list);
		}

		if (DataCenter.InterruptTarget is { } interrupt && !interrupt.IsCurrent())
		{
			DataCenter.InterruptTarget = null;
		}

		if (DataCenter.ProvokeTarget is { } provoke && !provoke.IsCurrent())
		{
			DataCenter.ProvokeTarget = null;
		}

		if (DataCenter.DeathTarget is { } death && !death.IsCurrent())
		{
			DataCenter.DeathTarget = null;
		}

		if (DataCenter.DispelTarget is { } dispel && !dispel.IsCurrent())
		{
			DataCenter.DispelTarget = null;
		}

		TankbusterForecast.PruneStale();
		CustomRotation.PruneStaleTankbusterPlan();
		TankSwapWatch.PruneStale();
	}

	private static void Prune(List<IBattleChara>? figures)
	{
		_ = figures?.RemoveAll(static figure => !figure.IsCurrent());
	}
}
