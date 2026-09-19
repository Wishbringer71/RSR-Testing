using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations;

/// <summary>
/// Represents a custom rotation with various item-related methods.
/// </summary>
public partial class CustomRotation
{
	private static readonly BaseItem PhoenixDownItemNumber = new(4570);

	internal static PhoenixDownItem[] PhoenixDowns { get; } = GetPhoenixDownItem();

	private static PhoenixDownItem[] GetPhoenixDownItem()
	{
		var items = Service.GetSheet<Item>();
		var list = new List<PhoenixDownItem>();
		foreach (var i in items)
		{
			if (i.RowId == PhoenixDownItemNumber.ID)
			{
				list.Add(new PhoenixDownItem(i));
			}
		}
		return [.. list];
	}

	/// <summary>
	/// Uses a Phoenix Down.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if a Phoenix Down can be used; otherwise, false.</returns>
	public unsafe static bool UsePhoenixDown(IAction nextGCD, out IAction? act)
	{
		act = null;

		if (DataCenter.DeathTarget == null)
		{
			return false;
		}

		foreach (var phoenixdown in PhoenixDowns)
		{
			// Report it, do not cast it. This used to call Use() here and set act as well, so
			// hooking it up the way every other item is hooked up would have fired UseAction twice
			// for one corpse in the same frame - once here, once in RSCommands.DoAction, which calls
			// Use() on whatever is reported. That second call is not discarded: UseAction queues an
			// action that arrives during a lock, which its own documentation says outright. And even
			// where the game refuses it, DoAction books the result of that second call as the
			// outcome, so CurrentAction, _lastActionID and _lastUsedTime all follow a call that did
			// not do the work. The reason for casting here is gone anyway: BaseItem.Use gives item
			// 4570 its own branch targeting DataCenter.DeathTarget, HQ and NQ included.
			//
			// No target override around this either. Items carry no target of their own - BaseItem
			// reads DataCenter.DeathTarget directly in both CanUse and Use - so setting one here
			// changed nothing and suggested a target selection that never happened.
			if (phoenixdown.CanUse(out act, true))
			{
				return true;
			}
		}

		act = null;
		return false;
	}

	#region Burst Medicine

	/// <summary>
	/// Gets the type of medicine.
	/// </summary>
	public abstract MedicineType MedicineType { get; }

	/// <summary>
	/// Gets the collection of available medicines.
	/// </summary>
	internal static MedicineItem[] Medicines { get; } = GetMedicines();

	private static MedicineItem[] GetMedicines()
	{
		var items = Service.GetSheet<Item>();
		var list = new List<MedicineItem>();
		foreach (var i in items)
		{
			if (i.FilterGroup == 6 && i.ItemSearchCategory.RowId == 43)
			{
				var med = new MedicineItem(i);
				if (med.Type != MedicineType.None)
				{
					list.Add(med);
				}
			}
		}
		// Reverse the list
		var n = list.Count;
		var arr = new MedicineItem[n];
		for (var i = 0; i < n; i++)
		{
			arr[i] = list[n - i - 1];
		}
		return arr;
	}

	/// <summary>
	/// Uses the burst medicines.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <param name="clippingCheck">Indicates whether to perform a clipping check.</param>
	/// <returns>True if a burst medicine was used; otherwise, false.</returns>
	public bool UseBurstMedicine(out IAction? act, bool clippingCheck = true)
	{
		act = null;

		var isHostileTargetDummy = HostileTarget?.IsDummy() ?? false;
		var isInHighEndDuty = DataCenter.Territory?.IsHighEndDuty ?? false;

		if (!isHostileTargetDummy && !isInHighEndDuty && DataCenter.CurrentTinctureUseType == TinctureUseType.InHighEndDuty)
		{
			return false;
		}

		if (DataCenter.CurrentTinctureUseType == TinctureUseType.Nowhere)
		{
			return false;
		}

		foreach (var medicine in Medicines)
		{
			if (medicine.Type != MedicineType)
			{
				continue;
			}

			if (medicine.CanUse(out act, clippingCheck))
			{
				return true;
			}
		}

		return false;
	}
	#endregion

	#region MP Potions

	/// <summary>
	/// Gets the collection of available MP potions.
	/// </summary>
	internal static MpPotionItem[] MpPotions { get; } = GetMpPotions();

	private static MpPotionItem[] GetMpPotions()
	{
		var items = Service.GetSheet<Item>();
		var list = new List<MpPotionItem>();
		foreach (var i in items)
		{
			if (i.FilterGroup == 9 && i.ItemSearchCategory.RowId == 43)
			{
				list.Add(new MpPotionItem(i));
			}
		}
		// Reverse the list
		var n = list.Count;
		var arr = new MpPotionItem[n];
		for (var i = 0; i < n; i++)
		{
			arr[i] = list[n - i - 1];
		}
		return arr;
	}

	/// <summary>
	/// Uses an MP potion.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if an MP potion was used; otherwise, false.</returns>
	private static bool UseMpPotion(IAction nextGCD, out IAction? act)
	{
		MpPotionItem? best = null;
		foreach (var a in MpPotions)
		{
			if (a.CanUse(out _, true))
			{
				if (best == null || a.MaxMp >= best.MaxMp)
				{
					best = a;
				}
			}
		}
		act = best;
		return act != null;
	}
	#endregion

	#region HP Potions

	/// <summary>
	/// Gets the collection of available HP potions.
	/// </summary>
	internal static HpPotionItem[] HpPotions { get; } = GetHpPotions();

	private static HpPotionItem[] GetHpPotions()
	{
		var items = Service.GetSheet<Item>();
		var list = new List<HpPotionItem>();
		foreach (var i in items)
		{
			if ((i.FilterGroup == 8 && i.ItemSearchCategory.RowId == 43) || i.RowId == 22306 || i.RowId == 47102 || i.RowId == 20309)
			{
				list.Add(new HpPotionItem(i));
			}
		}
		// Reverse the list
		var n = list.Count;
		var arr = new HpPotionItem[n];
		for (var i = 0; i < n; i++)
		{
			arr[i] = list[n - i - 1];
		}
		return arr;
	}

	/// <summary>
	/// Uses an HP potion.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if an HP potion was used; otherwise, false.</returns>
	private static bool UseHpPotion(IAction nextGCD, out IAction? act)
	{
		HpPotionItem? best = null;
		foreach (var a in HpPotions)
		{
			if (a.ID != 47102 && a.ID != 22306 && a.ID != 20309 && a.CanUse(out _, true))
			{
				if (best == null || a.MaxHp >= best.MaxHp)
				{
					best = a;
				}
			}

			// A confirmed tankbuster, or one BMR predicts within Config.BMRTankbusterMitWindow, can
			// exceed current HP even above the normal reactive threshold - any role, not just tanks,
			// can be caught under-margin. CanUseEmergency drops the HP% gate but keeps the same
			// missing-HP guard against wasting the potion via overheal.
			if ((DataCenter.IsHostileCastingTankBusterAtMe || DataCenter.BMRTankbusterImminent) && a.ID != 47102 && a.ID != 22306 && a.ID != 20309 && a.CanUseEmergency(out _))
			{
				if (best == null || a.MaxHp >= best.MaxHp)
				{
					best = a;
				}
			}

			if ((DataCenter.IsInPilgrimsTraverse || DataCenter.IsInTheFinalVerse) && a.ID == 47102 && a.CanUse(out _, true) && !StatusHelper.PlayerHasStatus(false, StatusID.Rehabilitation_4191))
			{
				if (best == null || a.MaxHp >= best.MaxHp)
				{
					best = a;
				}
			}

			if (DataCenter.IsInEurekaFieldOp && a.ID == 22306 && !StatusHelper.PlayerHasStatus(false, StatusID.Rehabilitation_648) && a.CanUse(out _, true))
			{
				if (best == null || a.MaxHp >= best.MaxHp)
				{
					best = a;
				}
			}

			if (DataCenter.IsInPalaceOfTheDead && a.ID == 20309 && !StatusHelper.PlayerHasStatus(false, StatusID.Rehabilitation_648) && a.CanUse(out _, true))
			{
				if (best == null || a.MaxHp >= best.MaxHp)
				{
					best = a;
				}
			}
		}
		act = best;
		return act != null;
	}
	#endregion
}