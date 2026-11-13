using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

public sealed partial class SGE_Reborn : SageRotation
{
	/// <summary>A raise is pending with Swiftcast up for it, so no GCD method spends the cast - only while the raise takes
	/// the running Swiftcast; otherwise the next spell spends it, and it does not run out unused (A504).</summary>
	private bool SwiftRaisePending =>
		(HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise)
		&& RunningSwiftGoesToRaise;

	// Set while the aim is the pull upkeep: the barrier then goes on the tank rather than on whoever the
	// action's own targeting picks.
	private bool _eukrasiaDiagnosisForTank;

	// Set while the aim is Eukrasian Dosis before it is due because no spell with a cast time can start (concept 03, "C6"):
	// the dosis then skips its refresh window.
	private bool _eukrasiaDotWhileCastBlocked;

	// The Eukrasian Dosis and Dosis of the current level, and whether putting the dosis on again now pays (A603).
	private IBaseAction EukrasianDosisAtLevel => EukrasianDosisIiiPvE.EnoughLevel ? EukrasianDosisIiiPvE
		: EukrasianDosisIiPvE.EnoughLevel ? EukrasianDosisIiPvE : EukrasianDosisPvE;

	private bool DosisWhileCastBlocked()
	{
		var dosis = EukrasianDosisAtLevel;
		var filler = DosisIiiPvE.EnoughLevel ? DosisIiiPvE : DosisIiPvE.EnoughLevel ? DosisIiPvE : DosisPvE;
		return CastStartBlocked && dosis.EnoughLevel && dosis.IsEnabled
			&& dosis.CanUse(out _, skipStatusProvideCheck: true) && DotWorthWhileCastBlocked(dosis, filler, dosis.Target.Target);
	}

	// Out of combat ClearEukrasia takes Eukrasia off again while no enemy is in reach of the sage, so the
	// pre-pull barrier waits until one is - otherwise Eukrasia would be pressed and removed in turn.
	private bool PullBarrierDue() =>
		UsePreEukrasianDiagnosis && (InCombat || HasHostilesInMaxRange)
		&& TryPullUpkeepOnTank(EukrasianDiagnosisPvE, PreEukrasianDiagnosisMinHostiles, PreEukrasianDiagnosisMinWallToWallHostiles,
			0f, EgeiroPvE, TankBarrierHeld, out _);

	public override bool CanHealSingleSpell => base.CanHealSingleSpell && GcdHealsAllowed(GCDHeal);

	public override bool CanHealAreaSpell => base.CanHealAreaSpell && GcdHealsAllowed(GCDHeal);
}
