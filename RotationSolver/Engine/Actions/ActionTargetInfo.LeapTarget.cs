namespace RotationSolver.Engine.Actions;

public partial struct ActionTargetInfo
{
	/// <summary>
	/// The candidates a leap onto the target picks from: those <see cref="CustomRotation.GapCloserAllowed"/> lets it reach,
	/// whenever at least one of them stands there; otherwise all of them. Concept 18, "Welches Ziel ein Sprung nimmt"
	/// (A961). The bound itself lifts for a move-forward command, the countdown and the player's own press of
	/// <paramref name="pressed"/>, and then nothing narrows. A list only when it narrows.
	/// </summary>
	/// <param name="candidates">The targets in range.</param>
	/// <param name="pressed">The leap asking, so the player's own press of it lifts the bound.</param>
	/// <returns>The candidates within the bound, or all of them.</returns>
	private static IEnumerable<IBattleChara> WithinGapCloserBound(IEnumerable<IBattleChara> candidates, ActionID pressed)
	{
		var all = 0;
		var within = 0;
		foreach (var candidate in candidates)
		{
			all++;
			if (CustomRotation.GapCloserAllowed(candidate, pressed: pressed))
			{
				within++;
			}
		}

		if (within == 0 || within == all)
		{
			return candidates;
		}

		var narrowed = new List<IBattleChara>(within);
		foreach (var candidate in candidates)
		{
			if (CustomRotation.GapCloserAllowed(candidate, pressed: pressed))
			{
				narrowed.Add(candidate);
			}
		}

		return narrowed;
	}
}
