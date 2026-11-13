using ECommons.DalamudServices;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// Whether "Tincture usage" allows a tincture in this content. A tincture is for a hard fight, to come through it and
	/// end it sooner; it costs money (his rule, A501). Under "high-end only" that is the game's own high-end flag - not a
	/// training dummy, where nothing is survived or ended.
	/// </summary>
	internal static bool TinctureAllowedHere => DataCenter.CurrentTinctureUseType switch
	{
		TinctureUseType.Anywhere => true,
		TinctureUseType.InHighEndDuty => DataCenter.Territory?.IsHighEndDuty ?? false,
		_ => false,
	};

	/// <summary>Set once this rotation has asked for a tincture in a fight; a rotation that never does holds no potion.</summary>
	private bool _asksForTinctureInCombat;

	/// <summary>
	/// HP and MP potions wait for the tincture under "Hold HP and MP potions for the tincture" (A502).
	/// </summary>
	/// <remarks>
	/// Every medicine shares one recast (cooldown group 59), so potion and tincture are only off recast together once the
	/// last tincture is a full recast back - and in a two-minute burst cycle the next burst is then at most a minute away,
	/// shorter than any potion's own recast. A potion drunk while the tincture is ready therefore keeps the tincture out
	/// of its burst; holding exactly then needs no burst forecast. Held only where the tincture would come: the rotation
	/// asks for one in a fight, the content allows it, and one of the job's type is enabled and in the inventory.
	/// </remarks>
	private bool PotionsWaitForTincture()
	{
		if (!Service.Config.HoldPotionsForTincture || !_asksForTinctureInCombat || !TinctureAllowedHere)
		{
			return false;
		}

		foreach (var medicine in Medicines)
		{
			if (medicine.Type == MedicineType && medicine.IsEnabled && medicine.HasIt)
			{
				return true;
			}
		}

		return false;
	}

	private static HashSet<uint>? _rehabilitationIds;
}
