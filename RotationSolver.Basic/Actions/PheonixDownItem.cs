using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

internal class PhoenixDownItem : BaseItem
{
	public PhoenixDownItem(Item item) : base(item)
	{
		// Only allow when the current DeathTarget is actually raisable (range/LoS/flags)
		ItemCheck = () =>
		{
			var t = DataCenter.DeathTarget;
			return t != null && ObjectHelper.CanBeRaised(t);
		};
	}


	protected override bool CanUseThis => Service.Config.UsePhoenixDown && ((Service.Config.UsePhoenixDownHealerLogic && !DataCenter.AnyLivingRaiser(excludeSelf: false)) || !Service.Config.UsePhoenixDownHealerLogic) && DataCenter.DeathTarget != null;
}
