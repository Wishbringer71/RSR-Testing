using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

internal partial class HpPotionItem : BaseItem
{
	private readonly float _percent;
	private readonly uint _maxHp;

	protected override bool CanUseThis => Service.Config.UseHpPotions;

	/// <summary>
	/// The potions of special content that regenerate (Rehabilitation) instead of healing at once, each usable only in its
	/// own content: Sustaining (Palace of the Dead), Eurekan (Eureka), Empyrean (Heaven-on-High), Orthos (Eureka Orthos),
	/// Pilgrim's (Pilgrim's Traverse) - the items with the action 8667 in cooldown group 60 (xivapi, 06.11.2026; concept
	/// 03, "Regenerationstränke der Sonderinhalte", A613).
	/// </summary>
	private static readonly uint[] ContentRegenerationIds = [20309, 22306, 23163, 38944, 47102];

	public HpPotionItem(Item item) : base(item)
	{
		IsContentRegeneration = IsContentRegenerationId(item.RowId);
		var data = _item.ItemAction.Value!.DataHQ;
		_percent = data[0] / 100f;
		_maxHp = data[1];
		var dataNq = _item.ItemAction.Value!.Data;
		_percentNq = dataNq[0] / 100f;
		_maxHpNq = dataNq[1];
	}

	public override bool CanUse(out IAction item, bool clippingCheck)
	{
		item = this;

		if (Player.Object == null)
		{
			return false;
		}

		return Player.Available && ThresholdHealthRatio() <= Service.Config.UseHpPotionsPercent && Player.Object.MaxHp - Player.Object.CurrentHp >= MaxHp && base.CanUse(out item);
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
