using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

public sealed partial class AST_Reborn : AstrologianRotation
{
	/// <summary>A raise is pending with Swiftcast up for it, so no GCD method spends the cast - only while the raise takes
	/// the running Swiftcast; otherwise the next spell spends it, and it does not run out unused (A504).</summary>
	private bool SwiftRaisePending =>
		(HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise)
		&& RunningSwiftGoesToRaise;

	/// <summary>
	/// Proactive wall-to-wall sustain: keep Aspected Benefic up on the tank as they commit to a pull.
	/// Called from GeneralGCD, HealSingleGCD and HealAreaGCD alike - the outer dispatch reaches the two
	/// heal methods first, so a raised heal-need flag would otherwise starve the check in GeneralGCD
	/// for a whole pull. Never fires at or below <see cref="AspectedBeneficHeal"/>, leaving a genuine
	/// emergency to Benefic II / Benefic. The rule itself is the healers' shared one, TryPullUpkeepOnTank.
	/// </summary>
	private bool TrySustainAspectedBeneficOnTank(out IAction? act)
	{
		act = null;
		return UsePreAspectedBenefic
			&& TryPullUpkeepOnTank(AspectedBeneficPvE, PreAspectedBeneficMinHostiles, PreAspectedBeneficMinWallToWallHostiles,
				AspectedBeneficHeal, AscendPvE, null, out act);
	}

	public override bool CanHealSingleSpell => base.CanHealSingleSpell && GcdHealsAllowed(GCDHeal);

	public override bool CanHealAreaSpell => base.CanHealAreaSpell && GcdHealsAllowed(GCDHeal);
}
