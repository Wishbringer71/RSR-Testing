using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.Basic.Actions;

public partial class BaseAction : IBaseAction
{
	// Whether this action, pressed now, is ready again before the next tankbuster on the player needs it: a charge left
	// after the press, or one charge's recast over before the hit less what of the action's own duration must stand
	// before it (pressed later than that, it would run out before the hit). Positive infinity: no tankbuster is due.
	private bool BackBefore(float due)
	{
		if (float.IsPositiveInfinity(due) || Cooldown.CurrentCharges > 1)
		{
			return true;
		}

		var lasts = DefensiveValues.DurationOf(ID);
		var window = lasts > 0f ? Math.Max(0f, lasts - DataCenter.DefaultGCDTotal) : 0f;
		return Cooldown.RecastTimeOneChargeRaw <= due - window;
	}
}
