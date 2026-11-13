using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

public sealed partial class SCH_Reborn : ScholarRotation
{
	protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
	{
		// Into a learned series also outside the area-defence flag: most series are instant attacks without a cast
		// (concept 08, A468). The alternation with Summon Seraph stays as in the area-defence path.
		if (MultiHitRestrict && SeraphismOnSeries && HitSeriesHitsLeft > 0
			&& (SummonSeraphPvE.Cooldown.IsCoolingDown || CurrentMp <= EmergencyHealingMPThreshold) && SeraphismPvE.CanUse(out act))
		{
			return true;
		}

		return base.GeneralAbility(nextGCD, out act);
	}

	// The ballpark estimate stands in only while no time to kill is measured for the target; a measured one the targeting
	// already weighs against the action's own time to kill (A387).
	private static bool BallparkAllows(IBattleChara? target, float expectedHp)
	{
		return target != null && (IsTimeToKillMeasured(target) || target.CurrentHp >= expectedHp);
	}

	public override bool CanHealSingleSpell => base.CanHealSingleSpell && GcdHealsAllowed(GCDHeal);

	public override bool CanHealAreaSpell => base.CanHealAreaSpell && GcdHealsAllowed(GCDHeal);

	/// <summary>A raise is pending with Swiftcast up for it, so no GCD method spends the cast - only while the raise takes
	/// the running Swiftcast; otherwise the next spell spends it, and it does not run out unused (A504).</summary>
	private bool SwiftRaisePending =>
		(HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise)
		&& RunningSwiftGoesToRaise;
}
