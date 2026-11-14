using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

internal partial class HpPotionItem : BaseItem
{
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

	/// <inheritdoc/>
	public override bool SwitchForAllJobs => true;

	/// <summary>Whether the item is one of <see cref="ContentRegenerationIds"/>.</summary>
	internal static bool IsContentRegenerationId(uint id)
	{
		return Array.IndexOf(ContentRegenerationIds, id) >= 0;
	}

	/// <summary>
	/// Whether this potion regenerates instead of healing at once (<see cref="ContentRegenerationIds"/>). Its item action
	/// states no percentage and cap, so <see cref="MaxHp"/> is 0, and it does not share the recast of the others: it is
	/// its own decision, not a rival of an ordinary potion (A613).
	/// </summary>
	public bool IsContentRegeneration { get; }

	/// <summary>
	/// The player's health that "Use HP potions below this HP" reads (concept 03, "Die Schwelle nach der Vorhersage, wo
	/// kein Heiler lebt", A641): now, and - with no healer alive in the party - the lower forecast for the next weave
	/// window, a GCD on, as the critical class reads it. Nobody else turns the measured fall around then, and a potion
	/// that misses this window goes a GCD later. The board's healing items read the same.
	/// </summary>
	internal static float ThresholdHealthRatio()
	{
		var now = ObjectHelper.GetPlayerHealthRatio();
		return DataCenter.AnyLivingHealer(false) ? now : Math.Min(now, ObjectHelper.GetForecastPlayerHealthRatio());
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
			return "off: switched off in the action list (for every job)";
		}

		if (!HasIt)
		{
			return "none in the bag";
		}

		if (Player.Object == null || !Player.Available)
		{
			return "no player";
		}

		var ratio = ThresholdHealthRatio();
		if (ratio > Service.Config.UseHpPotionsPercent)
		{
			return $"health {ratio:P0} (now or, with no healer alive, at the next weave) is above the threshold "
				+ $"{Service.Config.UseHpPotionsPercent:P0}";
		}

		var missing = Player.Object.MaxHp - Player.Object.CurrentHp;
		if (missing < MaxHp)
		{
			return $"missing {missing} HP is less than this potion heals ({MaxHp})";
		}

		return base.CanUse(out _) ? "ready" : "the game refuses it (cooldown, zone or level)";
	}
}
