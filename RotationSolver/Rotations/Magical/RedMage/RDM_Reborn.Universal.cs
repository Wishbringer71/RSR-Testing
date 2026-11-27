namespace RotationSolver.Rotations.Magical.RedMage;

public sealed partial class RDM_Reborn : RedMageRotation
{
	/// <inheritdoc/>
	protected override bool SwiftcastForDamage => SwiftcastDamage || base.SwiftcastForDamage;

	[RotationDesc(ActionID.AddlePvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (UseAddle(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	public override bool CanHealSingleSpell => base.CanHealSingleSpell && GcdHealsAllowed(GCDHeal);
}
