using ECommons.ExcelServices;

namespace RotationSolver.Rotations;

public partial class CustomRotation
{
	private static bool _pullPreparationTraced;

	/// <summary>
	/// Concept 03, "Den Kampf eröffnen", pull without a countdown (A924): out of combat, without a countdown, and with a GCD chosen against an enemy -
	/// the pull is due -, the rotation's own countdown path is asked as if the countdown ended now, and what it prepares
	/// without damage and without moving the player goes before that GCD. Not for the ninja, whose countdown path sets its
	/// ninjutsu aim when asked and would start mudras for a Suiton that, as an attack, may not answer here.
	/// </summary>
	private IAction? PullPreparation(IAction? gcd)
	{
		if (DataCenter.InCombat)
		{
			_pullPreparationTraced = false;
			return null;
		}

		if (Service.CountDownTime > 0 || DataCenter.IsPvP || DataCenter.Job == Job.NIN
			|| gcd is not IBaseAction attack || attack.Target.Target is not { } target || !target.IsEnemy())
		{
			return null;
		}

		IAction? preparation;
		IBaseAction.PullPreparationOnly = true;
		try
		{
			preparation = CountDownAction(0f);
		}
		finally
		{
			IBaseAction.PullPreparationOnly = false;
		}

		if (preparation != null && !_pullPreparationTraced)
		{
			_pullPreparationTraced = true;
			DefenseTrace.Line($"pull prepared: {preparation.Name} before {attack.Name}, no countdown");
		}

		return preparation;
	}
}
