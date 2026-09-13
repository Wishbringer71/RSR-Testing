using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

internal class PhoenixDownItem : BaseItem
{
	public PhoenixDownItem(Item item) : base(item)
	{
		ItemCheck = () =>
		{
			var target = DataCenter.DeathTarget;
			return target != null && TargetAcceptsTheFeather(target);
		};
	}

	/// <summary>
	/// Would the game let this feather go out on this corpse, right now?
	///
	/// This used to ask <see cref="ObjectHelper.CanBeRaised"/>, which runs CanUseActionOnTarget
	/// against the Raise spell. That is the wrong question for an item, and it is wrongest for the
	/// only people a feather is for: a tank or a damage job has no Raise. The client's action status
	/// answers for level and learning - BadStatus carries 573, "Not learned or not high enough
	/// level", for exactly that reason - so a check phrased as a spell can fail on the carrier
	/// rather than on the corpse.
	///
	/// Asking about item 4570 with the corpse as the target moves the whole question to the game:
	/// range, line of sight, whether the target is a raisable corpse at all, whether this duty
	/// forbids items, and whether a raise is already pending on them. The last one matters most in
	/// play - between the feather going out and the Raise status coming back, the corpse still
	/// reads as dead on our side, and only the game knows the difference.
	///
	/// A status of exactly 0 means usable with no qualification. The looser BadStatus list is right
	/// for the untargeted check <see cref="BaseItem.CanUse"/> makes and wrong here: a target-related
	/// refusal is not in that list, so it would read as permission.
	/// </summary>
	private unsafe bool TargetAcceptsTheFeather(IBattleChara target)
	{
		var manager = ActionManager.Instance();
		if (manager == null)
		{
			return false;
		}

		// Use() spends the HQ stack first when there is one, so ask about the same one it would use.
		var inventory = InventoryManager.Instance();
		var hq = inventory != null && inventory->GetInventoryItemCount(ID, true) > 0;

		return manager->GetActionStatus(ActionType.Item, hq ? ID + 1000000 : ID, target.GameObjectId) == 0;
	}

	protected override bool CanUseThis => Service.Config.UsePhoenixDown && ((Service.Config.UsePhoenixDownHealerLogic && !DataCenter.AnyLivingRaiser(excludeSelf: false)) || !Service.Config.UsePhoenixDownHealerLogic) && DataCenter.DeathTarget != null;
}
