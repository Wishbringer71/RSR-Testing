using ECommons.ExcelServices;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace RotationSolver.Engine.Actions;

public readonly partial struct ActionBasicInfo
{
	/// <summary>
	/// Whether this action leaves the MP a raise needs (concept 11, "Das Mana für die Wiederbelebung", A469): for a job that
	/// raises, in combat and in a party, a hostile action that costs MP goes only while the raise's cost - or "Don't raise
	/// below this MP" where that is higher - is left after it. Heals, the raise itself, actions without MP and a cast Thin
	/// Air makes free are untouched. The dark knight's "Keep at least 3000 MP" (A406), carried over to the raise. A cleanse
	/// counts as a heal only where it saves a life - its target bears a status that kills when it runs out -, else it keeps
	/// the reserve like damage (concept 07, "Reinigung oder Heilung", A565).
	/// </summary>
	private bool LeavesMpForRaise()
	{
		if (!Service.Config.KeepMpForRaise || (_action.Setting.IsFriendly && !CleanseSavesNoLife()) || !DataCenter.InCombat
			|| DataCenter.PartyMembers.Count <= 1)
		{
			return true;
		}

		var cost = MPNeed;
		if (cost == 0 || DataCenter.CurrentRotation is not CustomRotation rotation || rotation.RaiseAction is not { } raise
			|| !raise.EnoughLevel || raise.ID == ID)
		{
			return true;
		}

		if (Player.Job == Job.WHM
			&& (StatusHelper.PlayerHasStatus(true, StatusID.ThinAir) || CustomRotation.IsLastAction(ActionID.ThinAirPvE)))
		{
			return true;
		}

		// Where only the duty's own raises bring anyone back, the job's raise lifts nobody, and its mana is kept for
		// nothing (concept 11, "Tote, die nur die Wiederbelebung des Inhalts aufhebt", A616); those raises cost no mana.
		if (ObjectHelper.DutyRestrictsResurrection)
		{
			return true;
		}

		var reserve = Math.Max(raise.Info.MPNeed, (uint)Math.Max(0, Service.Config.LessMPNoRaise));
		return DataCenter.CurrentMp >= cost + reserve;
	}

	// A cleanse whose target - the one the dispel choice holds - bears no status that kills when it runs out.
	private bool CleanseSavesNoLife()
	{
		if (_action.Setting.TargetType != TargetType.Dispel || DataCenter.DispelTarget is not { } target)
		{
			return false;
		}

		var minRemaining = 1 + DataCenter.DefaultGCDRemain;
		foreach (ref readonly var status in target.GetStatuses())
		{
			if (StatusHelper.CanDispel(status, minRemaining) && StatusHelper.EndsInDeath((StatusID)status.StatusId))
			{
				return false;
			}
		}

		return true;
	}
}
