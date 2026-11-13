using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// Whether <paramref name="area"/>, after its <c>CanUse</c> chose where it lands, deals at least the potency of
	/// <paramref name="single"/> on one target over the enemies it reaches, by the tooltips of the running patch (their
	/// first stated potency, a stated falloff applied). A tie goes to the area: the same damage, spread over every enemy.
	/// True when a tooltip states no potency, so a caller keeps what it did without the comparison. The area's own AoE
	/// count stays the lower bound, since <c>CanUse</c> asked it first (concept 02, "Ausgaben in der Fläche erst, wo sie
	/// mindestens so viel bringen"; A473).
	/// </summary>
	/// <param name="area">The area version, asked after its <c>CanUse</c> returned true.</param>
	/// <param name="single">The single-target version it replaces.</param>
	/// <returns>True when the area deals at least as much.</returns>
	public static bool AreaWorthOver(IBaseAction area, IBaseAction single)
	{
		return area == null || single == null
			|| ActionTooltip.AreaAtLeastSingle(area.AdjustedID, area.Target.AffectedTargets?.Length ?? 1, single.AdjustedID);
	}

	/// <summary>
	/// Whether holding <paramref name="action"/> for <paramref name="holdSeconds"/> costs it a use before the current
	/// target dies: fewer of its recasts fit into the target's corrected time to kill after the hold than without it -
	/// The Balance's test for aligning a cooldown with a buff ("Align with Embolden only if you're sure you won't lose a
	/// use"). Asked only while the target is the last hostile in range, since otherwise the fight goes on after it; false
	/// whenever no time to kill is measured, so the hold stays as it was (A464).
	/// </summary>
	/// <param name="action">The cooldown that would wait.</param>
	/// <param name="holdSeconds">How long from now it would wait.</param>
	/// <returns>True when the hold loses a use before the target dies.</returns>
	public static bool HoldCostsUse(IBaseAction action, float holdSeconds)
	{
		if (action == null || holdSeconds <= 0f || NumberOfHostilesInRange > 1 || CurrentTarget is not { } target)
		{
			return false;
		}

		var ttk = target.GetCorrectedTTK();
		var recast = action.Cooldown.RecastTimeOneChargeRaw;
		if (float.IsNaN(ttk) || recast <= 0f)
		{
			return false;
		}

		var ready = action.Cooldown.HasOneCharge ? 0f : Math.Max(0f, action.Cooldown.RecastTimeRemainOneCharge);
		return UsesWithin(ttk - ready, recast) > UsesWithin(ttk - Math.Max(ready, holdSeconds), recast);

		static int UsesWithin(float span, float recast)
		{
			return span < 0f ? 0 : (int)(span / recast) + 1;
		}
	}
}
