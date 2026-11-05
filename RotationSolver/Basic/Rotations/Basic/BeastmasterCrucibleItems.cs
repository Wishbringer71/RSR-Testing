using System.Globalization;
using System.Text.RegularExpressions;
using Dalamud.Game;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K8 of the Crucible, first part (concept 01, "Stufe 3, erster Teil: K8, Heilgegenstände", A551): the board's healing
/// items when the player's HP fall below his HP potion line.
/// </summary>
public partial class BeastmasterRotation
{
	// The healing items of the board, by XBMItem row: the share of HP their text states, whether they heal the allies
	// nearby as well, their name. Read once from the English sheet: "Restores N% of HP to self or familiar" or "... to self
	// and nearby allies".
	private static Dictionary<ushort, (float Heal, bool Allies, string Name)>? _crucibleHealItems;

	// The last use: its slot, its item, when; and the slots the game did not take in this fight.
	private static (int Slot, ushort Item, DateTime At) _crucibleItemUsed = (-1, 0, DateTime.MinValue);
	private static readonly HashSet<int> _crucibleSlotsRefused = [];

	private static readonly Regex RestoresShare = new(@"^Restores (?<share>\d+)% of HP to self (?<whom>or familiar|and nearby allies)",
		RegexOptions.CultureInvariant);

	private static Dictionary<ushort, (float Heal, bool Allies, string Name)> CrucibleHealItems
		=> _crucibleHealItems ??= ReadCrucibleHealItems();

	private static Dictionary<ushort, (float Heal, bool Allies, string Name)> ReadCrucibleHealItems()
	{
		var items = new Dictionary<ushort, (float Heal, bool Allies, string Name)>();
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>(ClientLanguage.English);
		if (sheet == null)
		{
			return items;
		}

		foreach (var row in sheet)
		{
			var match = RestoresShare.Match(row.Unknown3.ExtractText());
			if (match.Success && float.TryParse(match.Groups["share"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent))
			{
				items[(ushort)row.RowId] = (percent / 100f, match.Groups["whom"].Value.StartsWith("and", StringComparison.Ordinal),
					row.Unknown2.ExtractText());
			}
		}

		return items;
	}

	/// <summary>
	/// K8 (A551): in combat on a board, below <paramref name="line"/> of the player's HP, the healing item that lifts him
	/// back to it with the least, else the largest; used on him through his soft target. Called every frame from the update.
	/// </summary>
	internal static unsafe void UseCrucibleHealItem(float line)
	{
		var player = Player;
		if (!DataCenter.IsInCrucible || !InCombat || player == null || player.IsDead || player.MaxHp == 0)
		{
			_crucibleSlotsRefused.Clear();
			return;
		}

		var director = EventFramework.Instance() is var framework && framework != null
			? framework->GetInstanceContentDirector()
			: null;
		if (director == null || director->InstanceContentType != InstanceContentType.CrucibleOfTheUnbroken)
		{
			return;
		}

		var crucible = (InstanceContentCrucibleOfTheUnbroken*)director;

		// The last use is pending until its slot shows another item, at most one GCD; a slot the game did not take is not
		// tried again in this fight.
		if (_crucibleItemUsed.Slot >= 0)
		{
			if (crucible->Inventory[_crucibleItemUsed.Slot].ItemId != _crucibleItemUsed.Item)
			{
				_crucibleItemUsed = (-1, 0, DateTime.MinValue);
			}
			else if ((DateTime.Now - _crucibleItemUsed.At).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}
			else
			{
				_ = _crucibleSlotsRefused.Add(_crucibleItemUsed.Slot);
				DefenseTrace.Line($"crucible item: slot {_crucibleItemUsed.Slot} ({CrucibleHealItems[_crucibleItemUsed.Item].Name}) not taken "
					+ "by the game - not tried again in this fight");
				_crucibleItemUsed = (-1, 0, DateTime.MinValue);
			}
		}

		var hp = player.GetHealthRatio();
		var actions = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		if (hp >= line || actions == null || actions->AnimationLock > 0f || player.IsCasting)
		{
			return;
		}

		var items = CrucibleHealItems;
		var best = -1;
		var bestHeal = 0f;
		var bestAllies = false;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			var id = crucible->Inventory[slot].ItemId;
			if (id == 0 || _crucibleSlotsRefused.Contains(slot) || !items.TryGetValue(id, out var item))
			{
				continue;
			}

			// The least that lifts him to the line, a potion before ash at the same heal; while none does, the largest.
			var lifts = hp + item.Heal >= line;
			var bestLifts = best >= 0 && hp + bestHeal >= line;
			var better = best < 0
				|| (lifts && !bestLifts)
				|| (lifts && bestLifts && (item.Heal < bestHeal || (item.Heal == bestHeal && bestAllies && !item.Allies)))
				|| (!lifts && !bestLifts && item.Heal > bestHeal);
			if (better)
			{
				best = slot;
				bestHeal = item.Heal;
				bestAllies = item.Allies;
			}
		}

		if (best < 0)
		{
			return;
		}

		var targets = TargetSystem.Instance();
		if (targets == null)
		{
			return;
		}

		var used = crucible->Inventory[best].ItemId;
		var previous = targets->SoftTarget;
		try
		{
			targets->SoftTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)player.Address;
			crucible->UseItem((uint)best, 0);
		}
		finally
		{
			targets->SoftTarget = previous;
		}

		_crucibleItemUsed = (best, used, DateTime.Now);
		DefenseTrace.Line($"crucible item: {items[used].Name} at {hp:P0} HP (line {line:P0}), restores {items[used].Heal:P0}");
	}
}
