using Dalamud.Game;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// A625 (concept 03, "Tiefe Gewölbe: Pomander"): alone in a deep dungeon, the pomanders that guard against a hit or stave
/// off a death, for every job. Against a boss Steel, Strength and Haste - before the pull once it stands in sight, so the
/// lock of each use costs no GCD; in combat the critical class takes Raising, then Witching on an attacker in its radius. Never in a party: the dungeon's inventory is shared, and what a party member
/// means to do with it is an assumption.
/// </summary>
/// <remarks>
/// A pomander is recognised by the English tooltip of its <c>DeepDungeonItem</c> row, like a status, so a protomander of
/// the same effect counts the same. Which item a slot holds comes from the <c>DeepDungeon</c> sheet's PomanderSlot column
/// for the dungeon the director names, as BossModReborn reads it; count and the active bit come from the dungeon itself.
/// <c>UsePomander</c> is hooked by BossModReborn, which queues the use and runs it - the two never use the same pomander
/// twice, since its own automation uses pomanders only against overflow.
/// </remarks>
internal static class DeepDungeonPomanders
{
	private enum Moment : byte
	{
		AgainstBoss,
		BeforeDeath,
		AgainstAttacker,
	}

	// In the order they are used: protection before damage, Raising before Witching.
	private static readonly (string Tooltip, Moment When)[] Effects =
	[
		("Decreases damage received", Moment.AgainstBoss),
		("Increases damage dealt", Moment.AgainstBoss),
		("swiftly execute", Moment.AgainstBoss),
		("raises the first party member", Moment.BeforeDeath),
		("Transforms all targets within range", Moment.AgainstAttacker),
	];

	private static readonly Dictionary<uint, (int Effect, float Radius)> _byItem = [];
	private static readonly HashSet<int> _refused = [];
	private static bool _fought;
	private static byte _floor;
	private static (int Slot, byte Count, DateTime At, string Name) _pending = (-1, 0, DateTime.MinValue, string.Empty);

	/// <summary>Called every frame RSR runs, from the rotation's update. Uses at most one pomander.</summary>
	internal static unsafe void Use()
	{
		var framework = EventFramework.Instance();
		var dungeon = framework != null && DataCenter.IsInDeepDungeons ? framework->GetInstanceContentDeepDungeon() : null;
		var player = Player.Object;
		if (dungeon == null || player == null || player.IsDead || !Service.Config.UseDeepDungeonPomanders)
		{
			_refused.Clear();
			_pending = (-1, 0, DateTime.MinValue, string.Empty);
			return;
		}

		// A refused slot is tried again after the fight, or on the next floor.
		if ((_fought && !DataCenter.InCombat) || dungeon->Floor != _floor)
		{
			_refused.Clear();
		}

		_fought = DataCenter.InCombat;
		_floor = dungeon->Floor;

		// A use is pending until its count falls or it shows active, at most one GCD; a slot the game did not take is not
		// tried again in this fight.
		if (_pending.Slot >= 0)
		{
			var held = dungeon->Items[_pending.Slot];
			if (held.Count < _pending.Count || held.IsActive)
			{
				_pending = (-1, 0, DateTime.MinValue, string.Empty);
			}
			else if ((DateTime.Now - _pending.At).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}
			else
			{
				_ = _refused.Add(_pending.Slot);
				DefenseTrace.Line($"deep dungeon: {_pending.Name} (slot {_pending.Slot}) not taken by the game "
					+ $"(usable bit {held.IsUsable}) - not tried again in this fight");
				_pending = (-1, 0, DateTime.MinValue, string.Empty);
			}
		}

		if (!Alone(dungeon))
		{
			return;
		}

		var actions = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		if (actions == null || actions->AnimationLock > 0f || player.IsCasting)
		{
			return;
		}

		var definition = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.DeepDungeon>()?.GetRowOrDefault(dungeon->DeepDungeonId);
		if (definition == null)
		{
			return;
		}

		var slots = definition.Value.PomanderSlot;
		var boss = DataCenter.InCombat ? DataCenter.BossInCombat : BossInSight();
		var critical = DataCenter.InCombat && player.IsInCriticalClass();
		for (var effect = 0; effect < Effects.Length; effect++)
		{
			var when = Effects[effect].When;
			if ((when == Moment.AgainstBoss && !boss) || (when != Moment.AgainstBoss && !critical))
			{
				continue;
			}

			for (var slot = 0; slot < slots.Count && slot < dungeon->Items.Length; slot++)
			{
				var item = Describe(slots[slot].RowId);
				if (item.Effect != effect || _refused.Contains(slot))
				{
					continue;
				}

				var held = dungeon->Items[slot];
				if (held.Count == 0 || held.IsActive)
				{
					continue;
				}

				if (when == Moment.AgainstAttacker && !AttackedWithin(player, item.Radius))
				{
					continue;
				}

				var name = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.DeepDungeonItem>(ClientLanguage.English)?
					.GetRowOrDefault(slots[slot].RowId)?.Name.ExtractText() ?? slots[slot].RowId.ToString();
				dungeon->UsePomander((uint)slot);
				_pending = (slot, held.Count, DateTime.Now, name);
				DefenseTrace.Line($"deep dungeon: {name} (slot {slot}, {held.Count} held) - "
					+ (when == Moment.AgainstBoss ? (DataCenter.InCombat ? "a boss is fighting" : "a boss stands in sight before the pull")
						: when == Moment.BeforeDeath
						? "you stand in the critical class" : "you stand in the critical class and are attacked in its radius"));
				return;
			}
		}
	}

	// Which of the effects an item has, and the radius of its action; -1 for none of them. Read once per item.
	private static (int Effect, float Radius) Describe(uint itemId)
	{
		if (_byItem.TryGetValue(itemId, out var known))
		{
			return known;
		}

		var described = (Effect: -1, Radius: 0f);
		if (Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.DeepDungeonItem>(ClientLanguage.English)?.GetRowOrDefault(itemId)
			is { } row)
		{
			var tooltip = row.Tooltip.ExtractText();
			for (var effect = 0; effect < Effects.Length; effect++)
			{
				if (tooltip.Contains(Effects[effect].Tooltip, StringComparison.OrdinalIgnoreCase))
				{
					described = (effect, row.Action.ValueNullable?.EffectRange ?? 0);
					break;
				}
			}
		}

		_byItem[itemId] = described;
		return described;
	}

	// The dungeon's own party record and RSR's party list must both say alone.
	private static unsafe bool Alone(InstanceContentDeepDungeon* dungeon)
	{
		if (DataCenter.PartyMembers.Count > 1)
		{
			return false;
		}

		var members = 0;
		foreach (var member in dungeon->Party)
		{
			if (member.EntityId is not 0 and not 0xE0000000)
			{
				members++;
			}
		}

		return members <= 1;
	}

	// Out of combat: an enemy with a boss icon in the list of enemies (in sight, within its range), the pull still to come.
	private static bool BossInSight()
	{
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.IsBossFromIcon())
			{
				return true;
			}
		}

		return false;
	}

	// An enemy that is no boss, fighting, aimed at you and within the radius.
	private static bool AttackedWithin(IBattleChara player, float radius)
	{
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.InCombat() && !hostile.IsBossFromIcon()
				&& hostile.TargetObjectId == player.GameObjectId && hostile.DistanceToPlayer() <= radius)
			{
				return true;
			}
		}

		return false;
	}
}
