using ECommons.DalamudServices;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations;

/// <summary>
/// Represents a custom rotation with various item-related methods.
/// </summary>
public partial class CustomRotation
{
	internal static PhoenixDownItem[] PhoenixDowns { get; } = [new PhoenixDownItem(Service.GetSheet<Item>().GetRow(4570))];

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

				if ((isMedicineCategory && i.FilterGroup == 8) || HpPotionItem.IsContentRegenerationId(i.RowId))
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
	public static bool UsePhoenixDown(IAction nextGCD, out IAction? act)
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

		// The rotation asks for a tincture in a fight: from here on a potion can cost it one (PotionsWaitForTincture).
		if (DataCenter.InCombat)
		{
			_asksForTinctureInCombat = true;
		}

		// Out of combat only under a countdown: a burst tincture drunk while waiting for a pull without one runs out
		// before the fight and keeps every medicine on its recast. Upstream closed this in three rotations (White Mage,
		// Viper, Dancer); here it holds for every caller, the countdown openers included (A433).
		if (!DataCenter.InCombat && Service.CountDownTime <= 0)
		{
			return false;
		}

		if (!TinctureAllowedHere)
		{
			return false;
		}

		foreach (var medicine in Medicines)
		{
			if (medicine.Type != MedicineType)
			{
				continue;
			}

			// A780 (concept 03, "In der Lernphase"): no tincture in the first burst of a pull while learning the fight.
			if (TinctureWaitsWhileLearning(medicine.EffectSeconds))
			{
				return false;
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

	internal static MpPotionItem[] MpPotions => Consumables.MpPotions;

	private static bool UseMpPotion(IAction nextGCD, bool waitForTincture, out IAction? act)
	{
		act = null;

		// An MP potion blocks the next tincture for its whole recast (A502).
		if (waitForTincture)
		{
			return false;
		}

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

	internal static HpPotionItem[] HpPotions => Consumables.HpPotions;

	private static bool UseHpPotion(IAction nextGCD, bool waitForTincture, out IAction? act)
	{
		HpPotionItem? best = null;
		var bestIsEmergency = false;

		// The feather and every medicine share one recast (cooldown group 59, Item sheet): a potion now keeps a raise
		// that is due off for its 75 s, the feather keeps potions off for 360 s. While the player lives through the
		// feather's cast, the raise goes first; a potion with a tankbuster on the way to the player still goes (A372).
		var featherFirst = DataCenter.MergedStatus.HasFlag(AutoStatus.Raise) && PhoenixDowns.Length > 0
			&& PhoenixDowns[0].RaisePending && PhoenixDowns[0].PlayerOutlivesTheCast;
		var bestIsOrdinary = false;

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
			// The content potions that regenerate are no rival here; they follow below (A613).
			if (!a.IsContentRegeneration && a.CanUse(out _, true))
			{
				if (best == null || a.MaxHp > best.MaxHp || (a.MaxHp == best.MaxHp && a.ItemLevel < best.ItemLevel))
				{
					best = a;
					bestIsEmergency = false;
					bestIsOrdinary = true;
				}
			}

			// A confirmed tankbuster on the player, or one BMR predicts within Config.BMRTankbusterMitWindow
			// that reaches him, can exceed current HP even above the normal reactive threshold - any role, not
			// just tanks, can be caught under-margin. CanUseEmergency drops the HP% gate but keeps the same
			// missing-HP guard against wasting the potion via overheal. The BMR half used to ask only whether a
			// tankbuster comes, so a healer at 60 % drank for one on the tank (A284).
			if ((DataCenter.IsHostileCastingTankBusterAtMe || DataCenter.BMRTankbusterReachesPlayer) && !a.IsContentRegeneration && a.CanUseEmergency(out _))
			{
				if (best == null || a.MaxHp > best.MaxHp || (a.MaxHp == best.MaxHp && a.ItemLevel < best.ItemLevel))
				{
					best = a;
					bestIsEmergency = !a.CanUse(out _, true);
					bestIsOrdinary = false;
				}
			}
		}

		// An ordinary potion waits for the feather; with a tankbuster on the way it goes, as the emergency branch would
		// have taken the same potion for it.
		if (featherFirst && bestIsOrdinary && !DataCenter.IsHostileCastingTankBusterAtMe && !DataCenter.BMRTankbusterReachesPlayer)
		{
			best = null;
		}

		// The same for the tincture under "Hold HP and MP potions for the tincture" (A502). The potion against a
		// tankbuster still goes: a death costs more damage than the tincture brings. The potions of Eureka and the deep
		// dungeons are never ordinary here and keep their own recast (cooldown group 60), so they are not held.
		if (waitForTincture && bestIsOrdinary && !DataCenter.IsHostileCastingTankBusterAtMe && !DataCenter.BMRTankbusterReachesPlayer)
		{
			best = null;
		}

		// Concept 03, "Regenerationstränke der Sonderinhalte" (A613): a content potion that regenerates shares no recast with
		// the others, so it is its own decision under the same threshold, after an ordinary potion chosen now, and only
		// while no Rehabilitation runs. The game refuses it outside its content, which CanUse asks.
		if (best == null && !PlayerRegenerates())
		{
			foreach (var a in HpPotions)
			{
				if (a.IsContentRegeneration && a.CanUse(out _, true))
				{
					best = a;
					bestIsEmergency = false;
					break;
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

	/// <summary>
	/// Whether a Rehabilitation runs on the player - the regeneration of a content potion, any of the status rows of that
	/// name, read from the English sheet so the client language does not matter (A613).
	/// </summary>
	private static bool PlayerRegenerates()
	{
		if (_rehabilitationIds == null)
		{
			var ids = new HashSet<uint>();
			var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
			if (sheet != null)
			{
				foreach (var row in sheet)
				{
					if (row.Name.ExtractText() == "Rehabilitation")
					{
						_ = ids.Add(row.RowId);
					}
				}
			}

			_rehabilitationIds = ids;
		}

		var statuses = ECommons.GameHelpers.Player.Object?.StatusList;
		if (statuses == null)
		{
			return false;
		}

		foreach (var status in statuses)
		{
			if (status != null && _rehabilitationIds.Contains(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// A530/A531: why each HP potion in the bag does not go out now, for the trace line of a player in the critical
	/// class - the first stopping condition of each, as the diagnostics window shows it. The global setting, the
	/// per-item switch and the threshold are the player's configuration, which the upload does not carry.
	/// </summary>
	/// <returns>One phrase, "no HP potion in the bag" when there is none.</returns>
	private string DescribeHpPotionsHeld()
	{
		var text = PotionsWaitForTincture() ? "held for the tincture (Hold HP and MP potions for the tincture); " : string.Empty;
		var held = false;
		foreach (var potion in HpPotions)
		{
			if (!potion.HasIt)
			{
				continue;
			}

			held = true;
			text += $"{potion.Name} {potion.DescribeBlock()}; ";
		}

		return "HP potions: " + (held ? text.TrimEnd(' ', ';') : text + "none in the bag");
	}
	#endregion
}