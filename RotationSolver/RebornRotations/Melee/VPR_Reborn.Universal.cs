namespace RotationSolver.RebornRotations.Melee;


public sealed partial class VPR_Reborn : ViperRotation
{
	// The Balance's opener dashes with Slither at -0.5 s; only from beyond melee range (A347).
	protected override IAction? CountDownAction(float remainTime)
	{
		return PullGapCloser(remainTime, SlitherPvE, SteelFangsPvE, out var act) ? act : base.CountDownAction(remainTime);
	}

	[RotationDesc]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		// EnoughWeaveTime is a clip-risk check; the Serpent's Ire slot-guard is a burst hold on the
		// universal layer and gives way when the player or a tank is in the critical class.
		if (EnoughWeaveTime
			&& !HoldSingleDefense(IsBurst && SerpentsIrePvE.CanUse(out _), "Viper: Serpent's Ire slot in burst")
			&& ShouldSustainMitigationDebuff(StatusID.Feint)
			&& FeintPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		if (!HoldSingleDefense(!NoAbilityReady, FollowUpHoldRule) && FeintPvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	protected override bool HasOwnAntiKnockbackGate => true;

	protected override bool HasOwnInterruptGate => true;
}
