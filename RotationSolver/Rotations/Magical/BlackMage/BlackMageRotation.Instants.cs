namespace RotationSolver.Rotations.Magical.BlackMage;

public partial class BlackMageRotation
{
	/// <summary>
	/// Concept 14, "Schwarzmagier", Triplecast and Swiftcast (A951): Blizzard III into Umbral Ice is made instant when it is
	/// cast in the ice - after Transpose, which leaves a single Umbral Ice - so its cast does not run slow there (The Balance,
	/// Black Mage Basic Guide 7.4). Swiftcast first, then Triplecast, whose charges carry three instants for movement. Not
	/// while the next spell is instant anyway. The Black Mage has no raise to keep Swiftcast for.
	/// </summary>
	/// <param name="nextGCD">The GCD the rotation chose next.</param>
	/// <param name="act">Swiftcast or Triplecast.</param>
	protected bool InstantForIceEntry(IAction nextGCD, out IAction? act)
	{
		act = null;
		if (!InUmbralIce || NextGCDisInstant || !nextGCD.IsTheSameTo(true, BlizzardIiiPvE))
		{
			return false;
		}

		return (SwiftcastForDamage && SwiftcastPvE.CanUse(out act)) || TriplecastPvE.CanUse(out act, usedUp: true);
	}

	/// <summary>
	/// Concept 14 (A951): while the player moves and the next spell has a cast, an instant keeps the casting going - since
	/// 7.2 the main use of both (The Balance: "primarily used for movement/weaving"). Triplecast first, three instants a
	/// charge, then Swiftcast. Not before an instant spell, which would not use it up.
	/// </summary>
	/// <param name="nextGCD">The GCD the rotation chose next.</param>
	/// <param name="act">Triplecast or Swiftcast.</param>
	protected bool InstantWhileMoving(IAction nextGCD, out IAction? act)
	{
		act = null;
		if (!IsMoving || !InCombat || !HasHostilesInRange || NextGCDisInstant
			|| nextGCD is not IBaseAction next || next.Info.CastTime <= 0f)
		{
			return false;
		}

		return TriplecastPvE.CanUse(out act, usedUp: true) || (SwiftcastForDamage && SwiftcastPvE.CanUse(out act));
	}
}
