using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RotationSolver.Helpers;

/// <summary>
/// Concept 21, K15 (A695): writes each window of the Crucible - its name begins with "XBM" - with its values to
/// <c>DefenseTrace.log</c> the first time it is set up and refreshed in a run, the score at the end of a board among them.
/// No source gives the layout of that window or the points the player's remaining HP bring (concept 21, "Am Ende des
/// Endkampfs"); the line holds both. It only listens.
/// </summary>
internal static class CrucibleWindowTrace
{
	private const string Prefix = "XBM";
	private static readonly HashSet<string> _seen = [];
	private static int _run = -1;

	public static void Enable()
	{
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, OnWindow);
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, OnWindow);
	}

	// K7 (A799): the board's result window - per familiar of the team its rank before and after the run, not synced, read in
	// his trace of 14.11.2026: value 72 the count, from 73 the icons (242000 plus the Bestiary number), from 121 the ranks
	// before and from 137 after, in blocks of 16.
	private const string ResultWindow = "XBMResult";
	private const int ResultCount = 72;
	private const int ResultFirstIcon = 73;
	private const int ResultBlock = 16;
	private const int ResultRankBeforeBlock = 3;
	private const int ResultRankAfterBlock = 4;
	private const int FamiliarIconBase = 242000;

	// Every value is checked - icon of a familiar, ranks within the highest, after not below before -; one that fails drops
	// the whole reading, and the trace says so.
	private static unsafe void ReadResultRanks(AtkValue* values, uint count)
	{
		if (!BestiaryTrace.TryReadValue(values, count, ResultCount, out var countValue) || countValue.Type != AtkValueType.UInt
			|| countValue.UInt == 0 || countValue.UInt > ResultBlock)
		{
			return;
		}

		var maxRank = CrucibleBoard.MaxRank;
		var ranks = new List<(BeastmasterPet, int, int)>((int)countValue.UInt);
		for (var i = 0; i < (int)countValue.UInt; i++)
		{
			if (!BestiaryTrace.TryReadValue(values, count, ResultFirstIcon + i, out var icon)
				|| !BestiaryTrace.TryReadValue(values, count, ResultFirstIcon + (ResultRankBeforeBlock * ResultBlock) + i, out var before)
				|| !BestiaryTrace.TryReadValue(values, count, ResultFirstIcon + (ResultRankAfterBlock * ResultBlock) + i, out var after))
			{
				DefenseTrace.Line($"crucible result: ranks not read - value {i} of the familiars unreadable");
				return;
			}

			var pet = icon.Int - FamiliarIconBase;
			if (icon.Type != AtkValueType.Int || pet <= 0 || !Enum.IsDefined((BeastmasterPet)(byte)pet) || pet > byte.MaxValue
				|| before.Type != AtkValueType.UInt || after.Type != AtkValueType.UInt
				|| before.UInt == 0 || after.UInt < before.UInt || (maxRank > 0 && after.UInt > maxRank))
			{
				DefenseTrace.Line($"crucible result: ranks not read - familiar {i} does not read as one (icon {icon.Int}, ranks {before.UInt} to {after.UInt})");
				return;
			}

			ranks.Add(((BeastmasterPet)(byte)pet, (int)before.UInt, (int)after.UInt));
		}

		CrucibleBoard.RecordFamiliarRanks(ranks);
	}

	public static void Disable()
	{
		Svc.AddonLifecycle.UnregisterListener(OnWindow);
	}

	private static unsafe void OnWindow(AddonEvent type, AddonArgs args)
	{
		// A796: also the team screen before a board - since 7.58 it shows the true rank beside the synced one, which no
		// structure of the client names yet (concept 21, K7 "Leveln").
		if (!args.AddonName.StartsWith(Prefix, StringComparison.Ordinal)
			|| !(DataCenter.IsInCrucible || BeastmasterRotation.CrucibleTeamScreenOpen(out _)))
		{
			return;
		}

		var (count, values) = args switch
		{
			AddonSetupArgs setup => (setup.AtkValueCount, setup.AtkValues),
			AddonRefreshArgs refresh => (refresh.AtkValueCount, refresh.AtkValues),
			_ => (0u, nint.Zero),
		};
		if (CrucibleBoard.Run != _run)
		{
			_run = CrucibleBoard.Run;
			_seen.Clear();
		}

		// A796: a shop and the team screen before a board at every opening - each shop offers other wares, and the gear held
		// changes between them; the other windows once a run.
		var everyOpening = type == AddonEvent.PostSetup
			&& (args.AddonName == "XBMContentsItemShop" || (args.AddonName == "XBMPetParty" && !DataCenter.IsInCrucible));
		if (args.AddonName == ResultWindow && type == AddonEvent.PostRefresh)
		{
			ReadResultRanks((AtkValue*)values, count);
		}

		if (everyOpening || _seen.Add($"{args.AddonName} {type}"))
		{
			DefenseTrace.Line($"crucible window {args.AddonName} {type}: {BestiaryTrace.Values((AtkValue*)values, count)}");
		}
	}
}
