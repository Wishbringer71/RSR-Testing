using ECommons.GameHelpers;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

internal class HpPotionItem : BaseItem
{
	private readonly float _percent;
	private readonly uint _maxHp;

	public uint MaxHp => !Player.Available || Player.Object == null ? 0 : Math.Min((uint)(Player.Object.MaxHp * _percent), _maxHp);

	protected override bool CanUseThis => Service.Config.UseHpPotions;

	public HpPotionItem(Item item) : base(item)
	{
		var data = _item.ItemAction.Value!.DataHQ;
		_percent = data[0] / 100f;
		_maxHp = data[1];
	}

	public override bool CanUse(out IAction item, bool clippingCheck)
	{
		item = this;

		if (Player.Object == null)
		{
			return false;
		}

		return Player.Available && ObjectHelper.GetPlayerHealthRatio() <= Service.Config.UseHpPotionsPercent && Player.Object.MaxHp - Player.Object.CurrentHp >= MaxHp && base.CanUse(out item);
	}

	/// <summary>
	/// Emergency variant for a confirmed incoming tankbuster: the normal <see cref="CanUse"/> only
	/// reacts to already-low current HP, but a tankbuster can exceed current HP even when it's above
	/// the configured reactive threshold. Drops the HP% gate but keeps the same missing-HP guard (the
	/// potion's own known heal amount must fully fit in missing HP) so it still won't waste a potion
	/// via overheal near full HP.
	/// </summary>
	public bool CanUseEmergency(out IAction item)
	{
		item = this;

		if (Player.Object == null)
		{
			return false;
		}

		return Player.Available && Player.Object.MaxHp - Player.Object.CurrentHp >= MaxHp && base.CanUse(out item);
	}
}
