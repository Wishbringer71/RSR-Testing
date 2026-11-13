using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

public partial class BaseItem : IBaseItem
{
	/// <summary>
	/// Whether this item's switch holds for every job at once (A635). An HP or MP potion and a Phoenix Down are the same item
	/// for every job, and one global setting ("Use HP potions", "Use MP potions", "Use Phoenix Down (Experimental)") says when they go. Their
	/// switch kept per job and off by default left that setting without effect on every job where the item had not been
	/// switched on by hand (a Beastmaster died twice in Sägerschrei with "Use HP potions" on). For these the switch only
	/// excludes, for every job, and nothing is excluded by default. Tinctures follow the same rule (A645); where a job
	/// drinks them is its "Tincture usage".
	/// </summary>
	public virtual bool SwitchForAllJobs => false;

	private unsafe void TracePress()
	{
		var hq = InventoryManager.Instance()->GetInventoryItemCount(ID, true) > 0;
		DefenseTrace.Line($"item pressed: {Name} #{ID} ({(hq ? "high quality" : "normal")}) in area {DataCenter.TerritoryID}");
	}

	// A683 (concept 03, "Was das Spiel hier ablehnt"): a use the game refused, with its answer for both qualities, once per
	// item and area - a reason CanUse does not read yet shows here.
	private static readonly HashSet<(uint Item, uint Area)> _refusalsTraced = [];

	private unsafe void TraceRefusal()
	{
		if (!_refusalsTraced.Add((ID, DataCenter.TerritoryID)))
		{
			return;
		}

		var manager = ActionManager.Instance();
		DefenseTrace.Line($"item refused by the game: {Name} #{ID} in area {DataCenter.TerritoryID}, status {manager->GetActionStatus(ActionType.Item, ID)} (HQ {manager->GetActionStatus(ActionType.Item, ID + 1000000)})");
	}
}
