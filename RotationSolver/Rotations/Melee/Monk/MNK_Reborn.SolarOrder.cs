namespace RotationSolver.Rotations.Melee.Monk;

public sealed partial class MNK_Reborn
{
	/// <summary>
	/// The next weaponskill of a Solar sequence under Perfect Balance: of the forms the sequence still lacks, the one whose
	/// weaponskill deals the least potency, as its tooltip states it with its form's Fury (The Balance: "order the three
	/// GCDs from weakest to strongest"). The strongest falls last, deeper into Riddle of Fire and Brotherhood. Concept 14,
	/// "Monk: Perfect Balance und das Burstfenster" (A963). Leaping Opo's certain critical hit is left out; it hangs on
	/// the player's stats. Without a stated potency a form keeps the place the former order gave it: raptor, coeurl,
	/// opo-opo.
	/// </summary>
	private bool WeakestSolarStep(out IAction? act)
	{
		act = null;
		var least = float.MaxValue;
		if (!BeastChakrasContains(BeastChakra.Raptor) && RaptorForm(out var raptor))
		{
			TakeIfWeaker(raptor, RaptorFury > 0, ref act, ref least);
		}

		if (!BeastChakrasContains(BeastChakra.Coeurl) && CoerlForm(out var coeurl))
		{
			TakeIfWeaker(coeurl, CoeurlFury > 0, ref act, ref least);
		}

		if (!BeastChakrasContains(BeastChakra.OpoOpo) && OpoOpoForm(out var opo))
		{
			TakeIfWeaker(opo, OpoOpoFury > 0, ref act, ref least);
		}

		return act != null;
	}

	private static void TakeIfWeaker(IAction? candidate, bool fury, ref IAction? act, ref float least)
	{
		if (candidate == null)
		{
			return;
		}

		var potency = ActionTooltip.TryHitPotency(candidate.AdjustedID, fury, out var stated) ? stated : float.MaxValue;
		if (act == null || potency < least)
		{
			act = candidate;
			least = potency;
		}
	}
}
