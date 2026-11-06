using ECommons.DalamudServices;
using RotationSolver.Basic.Rotations.Duties;

namespace RotationSolver.RebornRotations.Duty;

[Rotation("Deep Dungeon", CombatType.PvE)]

internal class DeepDungeonDefault : DeepDungeonRotation
{
	private static HashSet<uint>? _transfigurationIds;
	private bool _wasTransformed;

	// Concept 03, "Tiefe Gewölbe: Verwandlung" (A614): transformed, the job's actions are locked, so the attack of the form
	// goes first. Which form the player has is not read: the game allows only the actions of the form he has.
	public override bool EmergencyGCD(IAction? nextGCD, out IAction? act)
	{
		TraceTransformation();

		// Dreadnaught: the area attack where enough enemies stand for one, else the single one.
		if (WreckingBallPvE.CanUse(out act))
		{
			return true;
		}

		if (RotosmashPvE.CanUse(out act))
		{
			return true;
		}

		if (PummelPvE.CanUse(out act))
		{
			return true;
		}

		// The forms whose only attack is an area attack use it on a single enemy as well.
		if (VoidFireIiPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (HeavenlyJudgePvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		if (BigBurstPvE.CanUse(out act, skipAoeCheck: true))
		{
			return true;
		}

		return base.EmergencyGCD(nextGCD, out act);
	}

	// Rocky Roll dashes 25 yalms forward: only on a move-forward command, as every damaging dash from afar.
	public override bool MoveForwardGCD(out IAction? act)
	{
		if (RockyRollPvE.CanUse(out act))
		{
			return true;
		}

		return base.MoveForwardGCD(out act);
	}

	// One line per transformation in DefenseTrace.log: which attacks of the forms the game allows at its start, so an
	// upload shows whether the chain holds. The form is recognised by the status name on the English sheet.
	private void TraceTransformation()
	{
		var transformed = PlayerIsTransformed();
		if (transformed && !_wasTransformed)
		{
			var allowed = string.Empty;
			foreach (var action in (IBaseAction[])[PummelPvE, VoidFireIiPvE, HeavenlyJudgePvE, RotosmashPvE, WreckingBallPvE, BigBurstPvE, RockyRollPvE])
			{
				if (action.Info.ActionManagerStatusValid())
				{
					allowed += (allowed.Length == 0 ? string.Empty : ", ") + action.Name;
				}
			}

			DefenseTrace.Line($"transformed in a deep dungeon: the game allows {(allowed.Length == 0 ? "none of the form attacks" : allowed)}");
		}

		_wasTransformed = transformed;
	}

	private static bool PlayerIsTransformed()
	{
		if (_transfigurationIds == null)
		{
			var ids = new HashSet<uint>();
			var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
			if (sheet != null)
			{
				foreach (var row in sheet)
				{
					if (row.Name.ExtractText() == "Transfiguration")
					{
						_ = ids.Add(row.RowId);
					}
				}
			}

			_transfigurationIds = ids;
		}

		var statuses = ECommons.GameHelpers.Player.Object?.StatusList;
		if (statuses == null)
		{
			return false;
		}

		foreach (var status in statuses)
		{
			if (status != null && _transfigurationIds.Contains(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}
}
