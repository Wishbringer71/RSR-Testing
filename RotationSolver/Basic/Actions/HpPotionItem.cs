using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

internal class HpPotionItem : BaseItem
{
	private readonly float _percent;
	private readonly uint _maxHp;
	private readonly float _percentNq;
	private readonly uint _maxHpNq;

	/// <summary>
	/// What this potion restores here and now, for the form that will actually be drunk: UseItem takes
	/// the HQ one whenever the bag holds one, else the NQ one, and the two state different figures
	/// (Super-Potion 25 % against 20 %). Reading the HQ figure for an NQ potion overrated the heal, so
	/// the missing-health guard held the potion back longer than it restores.
	/// </summary>
	public uint MaxHp
	{
		get
		{
			if (!Player.Available || Player.Object == null)
			{
				return 0;
			}

			var hq = HasHq;
			return Math.Min((uint)(Player.Object.MaxHp * (hq ? _percent : _percentNq)), hq ? _maxHp : _maxHpNq);
		}
	}

	/// <summary>Whether the bag holds the HQ form, which UseItem prefers.</summary>
	private unsafe bool HasHq => InventoryManager.Instance()->GetInventoryItemCount(ID, true) > 0;

	/// <summary>
	/// The potion's item level from the Item sheet - its grade. Decides between two potions that
	/// restore the same amount here: the lower grade goes first.
	/// </summary>
	public uint ItemLevel => _item.LevelItem.RowId;

	protected override bool CanUseThis => Service.Config.UseHpPotions;

	public HpPotionItem(Item item) : base(item)
	{
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

		return Player.Available && ObjectHelper.GetPlayerHealthRatio() <= Service.Config.UseHpPotionsPercent && Player.Object.MaxHp - Player.Object.CurrentHp >= MaxHp && base.CanUse(out item);
	}

	/// <summary>
	/// Names the first condition that currently stops this potion, or "ready" when none does.
	/// </summary>
	/// <remarks>
	/// <see cref="CanUse"/> answers one bit, and six independent conditions produce that bit: the
	/// global setting, this item's own enable flag, the health threshold, the missing-health guard,
	/// having one in the bag, and - outside this class - the heal flag at the call site. A player
	/// asking "why did no potion go out" cannot tell those apart from a False, and neither can a
	/// reading of the code from somewhere else: which of them holds depends on that player's
	/// configuration and inventory, which is not observable from the repository.
	///
	/// So the item states its own reason. The two conditions it cannot see are named by the caller.
	/// </remarks>
	public string DescribeBlock()
	{
		if (!Service.Config.UseHpPotions)
		{
			return "off: Use HP Potions is disabled";
		}

		if (!IsEnabled)
		{
			return "off: this item is not enabled (per-item switch, separate from the setting above)";
		}

		if (!HasIt)
		{
			return "none in the bag";
		}

		if (Player.Object == null || !Player.Available)
		{
			return "no player";
		}

		var ratio = ObjectHelper.GetPlayerHealthRatio();
		if (ratio > Service.Config.UseHpPotionsPercent)
		{
			return $"health {ratio:P0} is above the threshold {Service.Config.UseHpPotionsPercent:P0}";
		}

		var missing = Player.Object.MaxHp - Player.Object.CurrentHp;
		if (missing < MaxHp)
		{
			return $"missing {missing} HP is less than this potion heals ({MaxHp})";
		}

		return base.CanUse(out _) ? "ready" : "the game refuses it (cooldown, zone or level)";
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
