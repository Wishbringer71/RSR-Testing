using Dalamud.Game;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// A626 (concept 23, "Tiefe Gewölbe: Magicite, Demiklone und Weihrauch"): alone in a deep dungeon, the dungeon's stones -
/// magicite, demiclones, incense - for every job, after the pomanders. In the critical class a stone that kills the
/// floor's enemies and makes the party invulnerable for 10 s; against a boss Elder Magicite or a demiclone in combat,
/// Barkbalm before the pull once the boss stands in sight. Never Mazeroot, and never in a party.
/// </summary>
/// <remarks>
/// The sheets carry only names and story text; the effects come from the wiki, so a stone is known by its English name.
/// A slot's value names its kind, counted from 1 in the dungeon's MagiciteSlot list, 0 empty; a used stone moves the others
/// up. The list names rows of the sheet DeepDungeonType selects (concept 23, "Welcher Stein auf welchem Platz", A737). A
/// line per floor shows the values and the kind read.
/// </remarks>
internal static class DeepDungeonStones
{
	private enum Purpose : byte
	{
		Never,
		Rescue,
		RescueOffBossFloor,
		KillBoss,
		Ally,
		BossFloor,
	}

	// English names as the sheets give them, with what they are for; the ally ranks in the order they are called.
	private static readonly (string Name, Purpose For)[] Kinds =
	[
		("inferno", Purpose.Rescue),
		("crag", Purpose.Rescue),
		("vortex", Purpose.Rescue),
		("poisonfruit", Purpose.RescueOffBossFloor),
		("elder", Purpose.KillBoss),
		("unei", Purpose.Ally),
		("onion knight", Purpose.Ally),
		("doga", Purpose.Ally),
		("barkbalm", Purpose.BossFloor),
		("mazeroot", Purpose.Never),
	];

	private static readonly HashSet<int> _refused = [];
	private static bool _fought;
	private static byte _floor;
	private static byte _tracedFloor;

	// One of each per moment: a rescue until you have left the critical class (the floor's enemies may die before its
	// invulnerability shows), an ally and the boss killer once a fight, Barkbalm once a floor.
	private static bool _rescueArmed = true;
	private static bool _allyCalled;
	private static bool _bossKillerUsed;
	private static bool _barkbalmUsed;
	private static (int Slot, byte Value, DateTime At, string Name) _pending = (-1, 0, DateTime.MinValue, string.Empty);

	/// <summary>Called every frame RSR runs, after the pomanders had none to use. Uses at most one stone.</summary>
	internal static unsafe void Use()
	{
		var framework = EventFramework.Instance();
		var dungeon = framework != null && DataCenter.IsInDeepDungeons ? framework->GetInstanceContentDeepDungeon() : null;
		var player = Player.Object;
		if (dungeon == null || player == null || player.IsDead || !Service.Config.UseDeepDungeonStones)
		{
			_refused.Clear();
			_pending = (-1, 0, DateTime.MinValue, string.Empty);
			_tracedFloor = 0;
			return;
		}

		if ((_fought && !DataCenter.InCombat) || dungeon->Floor != _floor)
		{
			_refused.Clear();
			_allyCalled = false;
			_bossKillerUsed = false;
		}

		_barkbalmUsed &= dungeon->Floor == _floor;

		_fought = DataCenter.InCombat;
		_floor = dungeon->Floor;

		var definition = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.DeepDungeon>()?.GetRowOrDefault(dungeon->DeepDungeonId);
		if (definition == null)
		{
			return;
		}

		Span<int> kinds = stackalloc int[dungeon->Magicite.Length];
		var names = new string[dungeon->Magicite.Length];
		var sources = new string[dungeon->Magicite.Length];
		for (var slot = 0; slot < dungeon->Magicite.Length; slot++)
		{
			names[slot] = Read(dungeon, definition.Value, slot, out sources[slot]);
			kinds[slot] = KindOf(names[slot]);
		}

		TraceFloor(dungeon, names, sources);

		// A use is pending until its slot shows another value, at most one GCD; a slot the game did not take is not tried
		// again in this fight.
		if (_pending.Slot >= 0)
		{
			if (dungeon->Magicite[_pending.Slot] != _pending.Value)
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
				DefenseTrace.Line($"deep dungeon: {_pending.Name} (stone slot {_pending.Slot}) not taken by the game "
					+ "- not tried again in this fight");
				_pending = (-1, 0, DateTime.MinValue, string.Empty);
			}
		}

		if (!DeepDungeonPomanders.Alone(dungeon) || !DeepDungeonPomanders.Free(player))
		{
			return;
		}

		var boss = DataCenter.InCombat ? DataCenter.BossInCombat : DeepDungeonPomanders.BossInSight();
		var critical = DataCenter.InCombat && player.IsInCriticalClass();
		var bossFighting = DataCenter.InCombat && DataCenter.BossInCombat;
		_rescueArmed |= !critical;

		// In the order of the kinds: rescue, then the boss killer, then the allies by rank, then Barkbalm.
		for (var kind = 0; kind < Kinds.Length; kind++)
		{
			var wanted = Kinds[kind].For switch
			{
				Purpose.Rescue => critical && _rescueArmed,
				Purpose.RescueOffBossFloor => critical && _rescueArmed && !boss,
				Purpose.KillBoss => bossFighting && !_bossKillerUsed,
				Purpose.Ally => bossFighting && !_allyCalled,
				Purpose.BossFloor => boss && !_barkbalmUsed,
				_ => false,
			};
			if (!wanted)
			{
				continue;
			}

			for (var slot = 0; slot < kinds.Length; slot++)
			{
				if (kinds[slot] != kind || _refused.Contains(slot))
				{
					continue;
				}

				var value = dungeon->Magicite[slot];
				dungeon->UseStone((uint)slot);
				switch (Kinds[kind].For)
				{
					case Purpose.Rescue or Purpose.RescueOffBossFloor:
						_rescueArmed = false;
						break;
					case Purpose.KillBoss:
						_bossKillerUsed = true;
						break;
					case Purpose.Ally:
						_allyCalled = true;
						break;
					default:
						_barkbalmUsed = true;
						break;
				}

				_pending = (slot, value, DateTime.Now, names[slot]);
				var why = Kinds[kind].For switch
				{
					Purpose.Rescue or Purpose.RescueOffBossFloor => "you stand in the critical class",
					Purpose.BossFloor when !DataCenter.InCombat => "a boss stands in sight before the pull",
					_ => "a boss is fighting",
				};
				DefenseTrace.Line($"deep dungeon: {names[slot]} (stone slot {slot}, read from {sources[slot]}) - {why}");
				return;
			}
		}
	}

	// A737: the stone sheets a MagiciteSlot list names.
	private enum StoneSheet : byte
	{
		None,
		MagicStone,
		Demiclone,
	}

	// A737: Pilgrim's Traverse mixes both sheets in its list (5, 4, 5), which EXDSchema leaves open; measured in the owner's
	// run of 12.11.2026: kind 1 Barkbalm (demiclone 5, Titania appeared with each use), kind 2 Mazeroot (demiclone 4), kind
	// 3 Poisonfruit (magicite 5).
	private static readonly StoneSheet[] PilgrimsTraverseSheets = [StoneSheet.Demiclone, StoneSheet.Demiclone, StoneSheet.MagicStone];

	// A737: the sheet of the list's entry at <index>, by DeepDungeonType as EXDSchema switches it - 1 magicite (Heaven-on-High),
	// 2 demiclones (Eureka Orthos) - and for Pilgrim's Traverse (3) as measured.
	private static StoneSheet SheetOf(Lumina.Excel.Sheets.DeepDungeon definition, int index) => definition.DeepDungeonType switch
	{
		1 => StoneSheet.MagicStone,
		2 => StoneSheet.Demiclone,
		3 when index < PilgrimsTraverseSheets.Length => PilgrimsTraverseSheets[index],
		_ => StoneSheet.None,
	};

	// The English name of the stone on a slot, or empty: its value counts from 1 in the dungeon's MagiciteSlot list.
	private static unsafe string Read(InstanceContentDeepDungeon* dungeon, Lumina.Excel.Sheets.DeepDungeon definition, int slot,
		out string source)
	{
		var value = dungeon->Magicite[slot];
		if (value == 0)
		{
			source = "empty slot";
			return string.Empty;
		}

		var index = value - 1;
		if (index >= definition.MagiciteSlot.Count)
		{
			source = "beyond the dungeon's list";
			return string.Empty;
		}

		var row = definition.MagiciteSlot[index].RowId;
		source = $"list entry {index}, row {row}";
		return SheetOf(definition, index) switch
		{
			StoneSheet.MagicStone => Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.DeepDungeonMagicStone>(ClientLanguage.English)?
				.GetRowOrDefault(row)?.Name.ExtractText() ?? string.Empty,
			StoneSheet.Demiclone => Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.DeepDungeonDemiclone>(ClientLanguage.English)?
				.GetRowOrDefault(row)?.TitleCase.ExtractText() ?? string.Empty,
			_ => string.Empty,
		};
	}

	private static int KindOf(string name)
	{
		for (var kind = 0; kind < Kinds.Length; kind++)
		{
			if (name.Contains(Kinds[kind].Name, StringComparison.OrdinalIgnoreCase))
			{
				return kind;
			}
		}

		return -1;
	}

	// Once per floor: the slot values, the agent's entries and the reading per slot - an upload settles the mapping.
	private static unsafe void TraceFloor(InstanceContentDeepDungeon* dungeon, string[] names, string[] sources)
	{
		if (dungeon->Floor == _tracedFloor)
		{
			return;
		}

		_tracedFloor = dungeon->Floor;
		var slots = new List<string>(names.Length);
		for (var slot = 0; slot < names.Length; slot++)
		{
			slots.Add($"{slot}: value {dungeon->Magicite[slot]} = {(names[slot].Length == 0 ? "-" : names[slot])} ({sources[slot]})");
		}

		DefenseTrace.Line($"deep dungeon stones, dungeon {dungeon->DeepDungeonId} floor {dungeon->Floor}: {string.Join("; ", slots)}");
	}
}
