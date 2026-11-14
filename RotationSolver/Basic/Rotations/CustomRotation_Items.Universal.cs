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

	/// <summary>
	/// Whether the player has cleared the duty they are in, as the game keeps it (A779): a cleared duty is farmed, one not
	/// yet cleared is learned with wipes. A rotation may weigh a tincture in the opener by it; outside an instanced duty false.
	/// </summary>
	public static unsafe bool InClearedDuty
	{
		get
		{
			var finder = DataCenter.Territory?.ContentFinderConditionId ?? 0;
			return finder != 0 && Service.GetSheet<ContentFinderCondition>().GetRowOrDefault(finder) is { ContentLinkType: 1 } row
				&& FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.IsInstanceContentCompleted(row.Content.RowId);
		}
	}

	/// <summary>
	/// Seconds the last combat in this territory lasted; 0 when the last combat was elsewhere or there was none (A779). A
	/// pull that wiped early tells a rotation that the opener of the next one may wipe early too.
	/// </summary>
	public static float LastPullTime => DataCenter.LastPullTerritoryId != 0 && DataCenter.LastPullTerritoryId == DataCenter.Territory?.Id
		? DataCenter.LastPullSeconds
		: 0f;

	// A780: per territory, the combat time of the second burst - the first ask for a tincture after a gap longer than
	// the tincture lasts - and this pull's state of it.
	private static readonly Dictionary<uint, float> _secondBurstAt = [];
	private static float _lastAskAt;
	private static float _lastCombatSeen;
	private static bool _laterBurst;

	/// <summary>
	/// While learning a fight, the tincture of the pull's first burst waits (concept 03, "In der Lernphase", A780): a wipe
	/// before the second burst wastes it, and every pull that reaches the second burst drinks one there anyway. Not in a
	/// cleared duty, and not once the last pull here reached the second burst; a burst counts as later when the rotation
	/// asks again after a gap longer than the tincture lasts.
	/// </summary>
	internal static bool TinctureWaitsWhileLearning(float effectSeconds)
	{
		var combat = DataCenter.InCombat ? DataCenter.CombatTimeRaw : 0f;
		if (combat < _lastCombatSeen)
		{
			_lastAskAt = 0f;
			_laterBurst = false;
		}

		_lastCombatSeen = combat;
		var territory = DataCenter.Territory?.Id ?? 0;
		if (DataCenter.InCombat)
		{
			if (!_laterBurst && combat - _lastAskAt > effectSeconds)
			{
				_laterBurst = true;
				if (territory != 0 && (!_secondBurstAt.TryGetValue(territory, out var known) || combat < known))
				{
					_secondBurstAt[territory] = combat;
				}
			}

			_lastAskAt = combat;
		}

		if (_laterBurst || InClearedDuty)
		{
			return false;
		}

		return !(_secondBurstAt.TryGetValue(territory, out var second) && LastPullTime >= second);
	}

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
