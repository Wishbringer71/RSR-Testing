namespace RotationSolver.RebornRotations.Magical;


public sealed partial class RDM_Reborn : RedMageRotation
{
	/// <inheritdoc/>
	protected override bool SwiftcastForDamage => SwiftcastDamage || base.SwiftcastForDamage;

	[RotationDesc(ActionID.AddlePvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (ShouldSustainMitigationDebuff(StatusID.Addle)
			&& AddlePvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (AddlePvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	public override bool CanHealSingleSpell => base.CanHealSingleSpell && GcdHealsAllowed(GCDHeal);
}
