using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations;

/// <summary>
/// Represents a custom rotation with various item-related methods.
/// </summary>
public partial class CustomRotation
{
	internal static PhoenixDownItem[] PhoenixDowns { get; } = [new PhoenixDownItem(Service.GetSheet<Item>().GetRow(4570))];

	// Every consumable list comes from one pass over the Item sheet, newest item first.
	private static class Consumables
	{
		internal static readonly MedicineItem[] Medicines;
		internal static readonly MpPotionItem[] MpPotions;
		internal static readonly HpPotionItem[] HpPotions;

		static Consumables()
		{
			List<MedicineItem> medicines = [];
			List<MpPotionItem> mpPotions = [];
			List<HpPotionItem> hpPotions = [];
			foreach (var i in Service.GetSheet<Item>())
			{
				var isMedicineCategory = i.ItemSearchCategory.RowId == 43;
				if (isMedicineCategory && i.FilterGroup == 6)
				{
					var medicine = new MedicineItem(i);
					if (medicine.Type != MedicineType.None)
					{
						medicines.Add(medicine);
					}
				}
				else if (isMedicineCategory && i.FilterGroup == 9)
				{
					mpPotions.Add(new MpPotionItem(i));
				}

				if ((isMedicineCategory && i.FilterGroup == 8) || i.RowId is 22306 or 47102 or 20309)
				{
					hpPotions.Add(new HpPotionItem(i));
				}
			}

			medicines.Reverse();
			mpPotions.Reverse();
			hpPotions.Reverse();
			Medicines = [.. medicines];
			MpPotions = [.. mpPotions];
			HpPotions = [.. hpPotions];
		}
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
	internal static MedicineItem[] Medicines => Consumables.Medicines;

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
	internal static MpPotionItem[] MpPotions => Consumables.MpPotions;

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
	internal static HpPotionItem[] HpPotions => Consumables.HpPotions;

	/// <summary>
	/// Uses an HP potion.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if an HP potion was used; otherwise, false.</returns>
	private static bool UseHpPotion(IAction nextGCD, out IAction? act)
	{
		HpPotionItem? best = null;
		var bestIsEmergency = false;
		foreach (var a in HpPotions)
		{
			// More healing wins; at EQUAL healing the lower grade wins, and both halves are stated
			// rather than left to the order of HpPotions.
			//
			// MaxHp is what this potion restores HERE AND NOW: the smaller of its own percentage of
			// Player.MaxHp and its own cap, and Player.MaxHp is already the synced pool. So the
			// comparison needs no knowledge about item levels or sync rules - it reads the effect.
			//
			// The tie is the case the owner named: a level 100 character in a level 50 instance may
			// still drink a level 100 potion, but the percentage is what binds there, and if two
			// grades state the same percentage they restore the same amount. Spending the expensive
			// one then buys nothing. Where the grades differ in percentage they differ in MaxHp too,
			// and the first half of the comparison picks the stronger one on its merits.
			//
			// The grade is the item level from the Item sheet, not the id: the ids happen to run in
			// grade order today, but nothing in the game ties them to it. Datamining table, checked
			// 25.09.2026: Potion 10, Hi- 25, Mega- 45, X- 70, Max- 150, Super- 290, Hyper- 560,
			// Ultra-Potion 690 - and the last three all restore 25 % in HQ (20 % in NQ), so they tie
			// wherever the percentage binds. Not relying on the list running strongest-first either: that order
			// is a property of GetHpPotions and would silently invert this rule if it ever changed.
			// The content-specific branches below deliberately keep ">=", so the potion meant for
			// that duty wins a tie against an ordinary one.
			if (a.ID != 47102 && a.ID != 22306 && a.ID != 20309 && a.CanUse(out _, true))
			{
				if (best == null || a.MaxHp > best.MaxHp || (a.MaxHp == best.MaxHp && a.ItemLevel < best.ItemLevel))
				{
					best = a;
					bestIsEmergency = false;
				}
			}

			// A confirmed tankbuster on the player, or one BMR predicts within Config.BMRTankbusterMitWindow
			// that reaches him, can exceed current HP even above the normal reactive threshold - any role, not
			// just tanks, can be caught under-margin. CanUseEmergency drops the HP% gate but keeps the same
			// missing-HP guard against wasting the potion via overheal. The BMR half used to ask only whether a
			// tankbuster comes, so a healer at 60 % drank for one on the tank (A284).
			if ((DataCenter.IsHostileCastingTankBusterAtMe || DataCenter.BMRTankbusterReachesPlayer) && a.ID != 47102 && a.ID != 22306 && a.ID != 20309 && a.CanUseEmergency(out _))
			{
				if (best == null || a.MaxHp > best.MaxHp || (a.MaxHp == best.MaxHp && a.ItemLevel < best.ItemLevel))
				{
					best = a;
					bestIsEmergency = !a.CanUse(out _, true);
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
		if (best != null)
		{
			DefenseTrace.Potion(best, bestIsEmergency);
		}

		return act != null;
	}
	#endregion
}