using ECommons.DalamudServices;
using ECommons.ExcelServices;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// The automatic area heal abilities, the duty's before the job's, with the heal target order and the need filter.
	/// The caller has set the heal path's target override.
	/// </summary>
	private bool AutoHealAreaAbility(IAction nextGCD, out IAction? act)
	{
		act = null;
		if (!DataCenter.AutoStatus.HasFlag(AutoStatus.HealAreaAbility) || !(CanHealAreaAbility || DataCenter.IsInOccultCrescentOp))
		{
			return false;
		}

		IBaseAction.AutoHealCheck = true;
		if (ActiveDutyRotation?.HealAreaAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (CanCastHeals && HealAreaAbility(nextGCD, out act))
		{
			return true;
		}
		IBaseAction.AutoHealCheck = false;
		return false;
	}

	/// <summary>
	/// The automatic single heal abilities, the duty's before the job's, with the heal target order and the need
	/// filter. The caller has set the heal path's target override.
	/// </summary>
	private bool AutoHealSingleAbility(IAction nextGCD, out IAction? act)
	{
		act = null;
		if (!DataCenter.AutoStatus.HasFlag(AutoStatus.HealSingleAbility) || !(CanHealSingleAbility || DataCenter.IsInOccultCrescentOp))
		{
			return false;
		}

		IBaseAction.AutoHealCheck = true;
		if (ActiveDutyRotation?.HealSingleAbility(nextGCD, out act) == true)
		{
			return true;
		}
		if (CanCastHeals && HealSingleAbility(nextGCD, out act))
		{
			return true;
		}
		IBaseAction.AutoHealCheck = false;
		return false;
	}
}
