using System.Globalization;
using System.Text.RegularExpressions;
using Dalamud.Game;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K8 of the Crucible (concept 01, "Stufe 3: K8, Gegenstände für die Sicherheit", A551, A552): the board's items that keep
/// the player alive - healing below his HP potion line, a cure for poison or blindness, Reraise, Sandstill before a fight,
/// the Beast Potion Kit against a boss.
/// </summary>
public partial class BeastmasterRotation
{
	private enum CrucibleItemKind : byte
	{
		Heal,
		Reraise,
		Sandstill,
		AutoPotion,
		CurePoison,
		CureBlindness,
	}

	// What an item of the board does, read once from its English effect text (XBMItem): its kind, the share it heals or the
	// chance it revives with, whether it heals the allies nearby as well, its name.
	private readonly record struct CrucibleItem(CrucibleItemKind Kind, float Value, bool Allies, string Name);

	private static Dictionary<ushort, CrucibleItem>? _crucibleItems;

	// The last use: its slot, its item, when; the slots the game did not take in this fight; whether Reraise has gone
	// without a fall, and whether it stood the frame before.
	private static (int Slot, ushort Item, DateTime At) _crucibleItemUsed = (-1, 0, DateTime.MinValue);
	private static readonly HashSet<int> _crucibleSlotsRefused = [];
	private static bool _reraiseLostWithoutFall;
	private static bool _reraiseStood;
	private static bool _fellUnderReraise;

	private static readonly Regex HealsShare = new(@"^Restores (?<share>\d+)% of HP to self (?<whom>or familiar|and nearby allies)",
		RegexOptions.CultureInvariant);
	private static readonly Regex GrantsReraise = new(@"^Grants Reraise to self\..*?(?<share>\d+)% chance of automatic revival",
		RegexOptions.CultureInvariant | RegexOptions.Singleline);
	private static readonly Regex Cures = new(@"^Instantly cures most instances of (?<ailment>poison|blindness) for self",
		RegexOptions.CultureInvariant);

	private static Dictionary<ushort, CrucibleItem> CrucibleItems => _crucibleItems ??= ReadCrucibleItems();

	private static Dictionary<ushort, CrucibleItem> ReadCrucibleItems()
	{
		var items = new Dictionary<ushort, CrucibleItem>();
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>(ClientLanguage.English);
		if (sheet == null)
		{
			return items;
		}

		foreach (var row in sheet)
		{
			var text = row.Unknown3.ExtractText();
			var name = row.Unknown2.ExtractText();
			var id = (ushort)row.RowId;
			if (HealsShare.Match(text) is { Success: true } heal && Share(heal, out var healed))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Heal, healed,
					heal.Groups["whom"].Value.StartsWith("and", StringComparison.Ordinal), name);
			}
			else if (GrantsReraise.Match(text) is { Success: true } reraise && Share(reraise, out var chance))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Reraise, chance, false, name);
			}
			else if (text.StartsWith("Grants Sandstill to self", StringComparison.Ordinal))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Sandstill, 0f, false, name);
			}
			else if (text.StartsWith("Grants Auto-potion to self", StringComparison.Ordinal))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.AutoPotion, 0f, false, name);
			}
			else if (Cures.Match(text) is { Success: true } cure)
			{
				items[id] = new CrucibleItem(cure.Groups["ailment"].Value == "poison" ? CrucibleItemKind.CurePoison
					: CrucibleItemKind.CureBlindness, 0f, false, name);
			}
		}

		return items;

		static bool Share(Match match, out float share)
		{
			var parsed = float.TryParse(match.Groups["share"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent);
			share = percent / 100f;
			return parsed;
		}
	}

	// The English names of the statuses, for the ones the items answer: Reraise, Sandstill, Auto-potion, poison, blindness.
	private static readonly Dictionary<uint, string> _statusNames = [];

	private static bool PlayerHasStatusNamed(IBattleChara player, string fragment)
	{
		if (player.StatusList == null)
		{
			return false;
		}

		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English);
		foreach (var status in player.StatusList)
		{
			if (status == null || status.StatusId == 0)
			{
				continue;
			}

			if (!_statusNames.TryGetValue(status.StatusId, out var name))
			{
				name = sheet?.GetRowOrDefault(status.StatusId)?.Name.ExtractText() ?? string.Empty;
				_statusNames[status.StatusId] = name;
			}

			if (name.Contains(fragment, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// K8 (A551, A552): on a board, the item that keeps the player alive now, one at a time, used on him through his soft
	/// target - in combat below <paramref name="line"/> of his HP the healing item that lifts him back to it with the least,
	/// else the largest; in combat a cure for the poison or blindness he carries; Reraise while none stands (once Reraise was
	/// seen to go without a fall, only against a boss); out of combat Sandstill while none stands; against a boss the Beast
	/// Potion Kit while no Auto-potion stands. Called every frame from the update.
	/// </summary>
	internal static unsafe void UseCrucibleItems(float line)
	{
		var player = Player;
		if (!DataCenter.IsInCrucible || player == null || player.MaxHp == 0)
		{
			_crucibleSlotsRefused.Clear();
			return;
		}

		FollowReraise(player);
		if (player.IsDead)
		{
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
		if (!InCombat)
		{
			_crucibleSlotsRefused.Clear();
		}

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
				DefenseTrace.Line($"crucible item: slot {_crucibleItemUsed.Slot} ({CrucibleItems[_crucibleItemUsed.Item].Name}) not taken "
					+ "by the game - not tried again in this fight");
				_crucibleItemUsed = (-1, 0, DateTime.MinValue);
			}
		}

		var actions = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		if (actions == null || actions->AnimationLock > 0f || player.IsCasting)
		{
			return;
		}

		var hp = player.GetHealthRatio();
		var bossFighting = BossFighting();
		var slot = -1;
		var why = string.Empty;
		if (InCombat && hp < line)
		{
			slot = BestHeal(crucible, hp, line);
			why = $"at {hp:P0} HP (line {line:P0})";
		}

		if (slot < 0 && InCombat && PlayerHasStatusNamed(player, "Poison"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.CurePoison);
			why = "against poison";
		}

		if (slot < 0 && InCombat && PlayerHasStatusNamed(player, "Blind"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.CureBlindness);
			why = "against blindness";
		}

		if (slot < 0 && !_reraiseStood && (!_reraiseLostWithoutFall || bossFighting))
		{
			slot = MostOf(crucible, CrucibleItemKind.Reraise);
			why = _reraiseLostWithoutFall ? "against a boss, no Reraise stands" : "no Reraise stands";
		}

		if (slot < 0 && !InCombat && !PlayerHasStatusNamed(player, "Sandstill"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.Sandstill);
			why = "before the next fight, no Sandstill stands";
		}

		if (slot < 0 && InCombat && bossFighting && !PlayerHasStatusNamed(player, "Auto-potion"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.AutoPotion);
			why = "against a boss, no Auto-potion stands";
		}

		var targets = TargetSystem.Instance();
		if (slot < 0 || targets == null)
		{
			return;
		}

		var used = crucible->Inventory[slot].ItemId;
		var previous = targets->SoftTarget;
		try
		{
			targets->SoftTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)player.Address;
			crucible->UseItem((uint)slot, 0);
		}
		finally
		{
			targets->SoftTarget = previous;
		}

		_crucibleItemUsed = (slot, used, DateTime.Now);
		DefenseTrace.Line($"crucible item: {CrucibleItems[used].Name} {why}");
	}

	// The healing item that lifts the player to the line with the least, a potion before ash at the same heal; while none
	// does, the largest.
	private static unsafe int BestHeal(InstanceContentCrucibleOfTheUnbroken* crucible, float hp, float line)
	{
		var best = -1;
		CrucibleItem bestItem = default;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (!TryItem(crucible, slot, CrucibleItemKind.Heal, out var item))
			{
				continue;
			}

			var lifts = hp + item.Value >= line;
			var bestLifts = best >= 0 && hp + bestItem.Value >= line;
			if (best < 0
				|| (lifts && !bestLifts)
				|| (lifts && bestLifts && (item.Value < bestItem.Value || (item.Value == bestItem.Value && bestItem.Allies && !item.Allies)))
				|| (!lifts && !bestLifts && item.Value > bestItem.Value))
			{
				best = slot;
				bestItem = item;
			}
		}

		return best;
	}

	private static unsafe int FirstOf(InstanceContentCrucibleOfTheUnbroken* crucible, CrucibleItemKind kind)
	{
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, kind, out _))
			{
				return slot;
			}
		}

		return -1;
	}

	private static unsafe int MostOf(InstanceContentCrucibleOfTheUnbroken* crucible, CrucibleItemKind kind)
	{
		var best = -1;
		var value = 0f;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, kind, out var item) && (best < 0 || item.Value > value))
			{
				best = slot;
				value = item.Value;
			}
		}

		return best;
	}

	private static unsafe bool TryItem(InstanceContentCrucibleOfTheUnbroken* crucible, int slot, CrucibleItemKind kind, out CrucibleItem item)
	{
		item = default;
		var id = crucible->Inventory[slot].ItemId;
		return id != 0 && !_crucibleSlotsRefused.Contains(slot) && CrucibleItems.TryGetValue(id, out item) && item.Kind == kind;
	}

	private static bool BossFighting()
	{
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.InCombat() && hostile.IsBossFromIcon())
			{
				return true;
			}
		}

		return false;
	}

	// Reraise is assumed to stand until a fall, since its text names no duration. Seen to go while the player lives, it is
	// kept for the fights against a boss from then on.
	private static void FollowReraise(IBattleChara player)
	{
		var stands = PlayerHasStatusNamed(player, "Reraise");
		_fellUnderReraise |= _reraiseStood && player.IsDead;
		if (_reraiseStood && !stands && !player.IsDead)
		{
			if (!_fellUnderReraise && !_reraiseLostWithoutFall)
			{
				_reraiseLostWithoutFall = true;
				DefenseTrace.Line("crucible item: Reraise went without a fall - from now on a Reraiser only against a boss");
			}

			_fellUnderReraise = false;
		}

		_reraiseStood = stands || (_reraiseStood && player.IsDead);
	}
}
