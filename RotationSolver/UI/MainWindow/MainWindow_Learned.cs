using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using RotationSolver.Basic.Configuration;
using GAction = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;
using TerritoryType = Lumina.Excel.Sheets.TerritoryType;

namespace RotationSolver.UI;

/// <summary>
/// The fork's learned tables in the list window, one tab per store: the value the readers see and its state under the
/// game version (concept 13, "Stufe 5", rule 10). Read only - his rule A173: a table is reset by deleting its file with
/// the game closed.
/// </summary>
public partial class MainWindow
{
	// One tab: its column headers and its rows, built off the draw path.
	private sealed record LearnedTab(string Name, string[] Columns, List<string[]> Rows);

	private static List<LearnedTab>? _learnedTabs;
	private static DateTime _learnedBuilt = DateTime.MinValue;
	private static string _learnedSearching = string.Empty;
	private static readonly Dictionary<uint, string> _learnedActionNames = [];
	private static readonly Dictionary<uint, string> _learnedStatusNames = [];
	private static readonly Dictionary<uint, string> _learnedAreaNames = [];

	private static void DrawListLearned()
	{
		// Rebuilt from copies of the stores at most once a second, and only while this section is drawn.
		var now = DateTime.Now;
		if (_learnedTabs == null || (now - _learnedBuilt).TotalSeconds >= 1)
		{
			_learnedTabs = BuildLearnedTabs();
			_learnedBuilt = now;
		}

		var game = OtherConfiguration.GameVersion;
		ImGui.TextColored(game == null ? ImGuiColors.DalamudYellow : ImGuiColors.DalamudGrey, game == null
			? "Game version not readable: no figure counts as confirmed, and every lock taken over from an earlier version holds."
			: $"Game version {game}. As the rules read them; nothing here can be changed - a table is reset by deleting its file with the game closed.");

		ImGui.SetNextItemWidth(ImGui.GetWindowWidth());
		_ = ImGui.InputTextWithHint("##Searching the learned tables", "Name or ID", ref _learnedSearching, 50);

		using var bar = ImRaii.TabBar("Rotation Solver Learned Tables");
		if (!bar)
		{
			return;
		}

		foreach (var tab in _learnedTabs)
		{
			using var item = ImRaii.TabItem($"{tab.Name} ({tab.Rows.Count})###{tab.Name}");
			if (!item)
			{
				continue;
			}

			var rows = FilterLearnedRows(tab.Rows);
			if (ImGui.Button("Copy##learned"))
			{
				ImGui.SetClipboardText(LearnedRowsAsText(tab.Columns, rows));
			}

			ImguiTooltips.HoveredTooltip("Copies the rows shown, tab-separated, header first.");
			DrawLearnedTable(tab, rows);
		}
	}

	private static List<string[]> FilterLearnedRows(List<string[]> rows)
	{
		if (string.IsNullOrWhiteSpace(_learnedSearching))
		{
			return rows;
		}

		var filtered = new List<string[]>();
		foreach (var row in rows)
		{
			if (row.Length > 0 && row[0].Contains(_learnedSearching, StringComparison.OrdinalIgnoreCase))
			{
				filtered.Add(row);
			}
		}

		return filtered;
	}

	private static string LearnedRowsAsText(string[] columns, List<string[]> rows)
	{
		var text = new System.Text.StringBuilder();
		_ = text.AppendJoin('\t', columns).Append('\n');
		foreach (var row in rows)
		{
			_ = text.AppendJoin('\t', row).Append('\n');
		}

		return text.ToString();
	}

	// Only the rows in view are drawn (ImGuiListClipper, as BossModReborn's replay lists use it).
	private static void DrawLearnedTable(LearnedTab tab, List<string[]> rows)
	{
		using var table = ImRaii.Table($"##learned {tab.Name}", tab.Columns.Length,
			ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY,
			new Vector2(0f, ImGui.GetContentRegionAvail().Y));
		if (!table)
		{
			return;
		}

		ImGui.TableSetupScrollFreeze(0, 1);
		foreach (var column in tab.Columns)
		{
			ImGui.TableSetupColumn(column);
		}

		ImGui.TableHeadersRow();
		var clipper = new ImGuiListClipper();
		clipper.Begin(rows.Count, ImGui.GetTextLineHeightWithSpacing());
		while (clipper.Step())
		{
			for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
			{
				ImGui.TableNextRow();
				foreach (var cell in rows[i])
				{
					_ = ImGui.TableNextColumn();
					ImGui.TextUnformatted(cell);
				}
			}
		}

		clipper.End();
	}

	private static List<LearnedTab> BuildLearnedTabs()
	{
		var game = OtherConfiguration.GameVersion;
		return
		[
			AreaTab(game),
			TankbusterTab(game),
			AimedHitTab(game),
			SimpleActionTab("Area series", "Series span", CopyAreaSeriesSpan(), span => $"{span:F1} s"),
			SimpleActionTab("Hit series", "Hits in a row", CopyHitSeries(), hits => hits.ToString()),
			SleepOutcomeTab(game),
			SleepKindTab(game),
			SleepLimitTab(game),
			DotTab(),
			PenaltyTab(game),
			MarkerTab(game),
		];
	}

	private static LearnedTab AreaTab(string? game)
	{
		var (entries, _) = OtherConfiguration.AreaRevisionSnapshot();
		var rows = new List<string[]>();
		foreach (var (id, share) in new Dictionary<uint, float>(OtherConfiguration.HostileCastingAreaPotential))
		{
			entries.TryGetValue(id, out var entry);
			rows.Add([ActionLabel(id), $"{share * 100f:F0}%", RevisionState(entry, OtherConfiguration.AreaShareConfirmed(id), game,
				v => $"{v * 100f:F0}%"), RunningReadings(entry, game)]);
		}

		Sort(rows);
		return new LearnedTab("Area hits", ["Action", "Share of max HP", "State", "Readings in this version"], rows);
	}

	private static LearnedTab TankbusterTab(string? game)
	{
		var (entries, _) = OtherConfiguration.TankbusterRevisionSnapshot();
		var readings = new Dictionary<uint, TankbusterReading>();
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			foreach (var (id, reading) in OtherConfiguration.TankbusterPotential)
			{
				readings[id] = reading;
			}
		}

		var rows = new List<string[]>();
		foreach (var (id, reading) in readings)
		{
			entries.TryGetValue(id, out var entry);
			var state = RevisionState(entry, OtherConfiguration.TankbusterConfirmed(id), game, v => $"{v.Unmitigated * 100f:F0}%");
			if (OtherConfiguration.HostileCastingArea.Contains(id))
			{
				state += "; in the AoE list";
			}

			rows.Add([ActionLabel(id), $"{reading.Unmitigated * 100f:F0}%", $"{reading.UnderVulnerability * 100f:F0}%",
				reading.Span > 0f ? $"{reading.Span:F1} s" : "-", reading.AimedOnly ? "yes" : "no", state, RunningReadings(entry, game)]);
		}

		Sort(rows);
		return new LearnedTab("Tankbusters",
			["Action", "Unmitigated", "Under vulnerability", "Series span", "Aimed only", "State", "Readings in this version"], rows);
	}

	private static LearnedTab AimedHitTab(string? game)
	{
		var (entries, _) = OtherConfiguration.AimedHitRevisionSnapshot();
		var readings = new Dictionary<uint, TankbusterReading>();
		lock (OtherConfiguration.AimedHitPotentialGate)
		{
			foreach (var (id, reading) in OtherConfiguration.AimedHitPotential)
			{
				readings[id] = reading;
			}
		}

		var rows = new List<string[]>();
		foreach (var (id, reading) in readings)
		{
			entries.TryGetValue(id, out var entry);
			rows.Add([ActionLabel(id), $"{reading.Unmitigated * 100f:F0}%", $"{reading.UnderVulnerability * 100f:F0}%",
				reading.Span > 0f ? $"{reading.Span:F1} s" : "-",
				RevisionState(entry, OtherConfiguration.AimedHitConfirmed(id), game, v => $"{v.Unmitigated * 100f:F0}%"),
				RunningReadings(entry, game)]);
		}

		Sort(rows);
		return new LearnedTab("Aimed hits on non-tanks",
			["Action", "Unmitigated", "Under vulnerability", "Series span", "State", "Readings in this version"], rows);
	}

	// What a revised entry is in the running game version (concept 13, rules 4 and 7, stage 3).
	private static string RevisionState<T>(RevisionEntry<T>? entry, bool confirmed, string? game, Func<T, string> show)
	{
		if (entry == null)
		{
			return "no revision yet";
		}

		var state = confirmed ? "confirmed in this game version"
			: entry.Current is { } current && game != null && current.Game == game ? "measured in this version, not cleanly yet"
			: entry.Inherited is { } inherited ? inherited.OldFile ? "taken over from the file before the revisions"
				: $"taken over from game version {(inherited.Game.Length > 0 ? inherited.Game : "unknown")}"
			: "taken over";
		if (entry.Refuted is { Value: { } refuted })
		{
			state += $"; refuted: was {show(refuted)}";
		}

		if (entry.Quarantined != null)
		{
			state += $"; quarantined: {entry.Quarantined.Value}";
		}

		return state;
	}

	private static string RunningReadings<T>(RevisionEntry<T>? entry, string? game)
	{
		return entry?.Current is { } current && game != null && current.Game == game
			? $"{current.Clean} clean of {current.Count}" + (current.Clean > 1 ? $", {current.Low * 100f:F0}-{current.High * 100f:F0}%" : string.Empty)
			: "-";
	}

	private static LearnedTab SimpleActionTab<T>(string name, string column, Dictionary<uint, T> store, Func<T, string> show)
	{
		var rows = new List<string[]>();
		foreach (var (id, value) in store)
		{
			rows.Add([ActionLabel(id), show(value)]);
		}

		Sort(rows);
		return new LearnedTab(name, ["Action", column], rows);
	}

	private static Dictionary<uint, float> CopyAreaSeriesSpan() => new(OtherConfiguration.AreaSeriesSpan);

	private static Dictionary<uint, int> CopyHitSeries()
	{
		lock (OtherConfiguration.HitSeriesGate)
		{
			return new Dictionary<uint, int>(OtherConfiguration.HitSeriesOnPlayer);
		}
	}

	private static LearnedTab SleepOutcomeTab(string? game)
	{
		var rows = new List<string[]>();
		Dictionary<uint, SleepReading> outcomes;
		SleepLockMarks marks;
		lock (OtherConfiguration.SleepStoreGate)
		{
			outcomes = new Dictionary<uint, SleepReading>(OtherConfiguration.SleepOutcomes);
			marks = OtherConfiguration.SleepLocks.Copy();
		}

		foreach (var (id, reading) in outcomes)
		{
			var unstoppable = SleepInterruptWatch.IsUnstoppable(id);
			var lockState = unstoppable
				? LockText(SleepLockRules.ActionState(true, marks, id, game), marks.Actions.GetValueOrDefault(id), game)
				: reading.Stopped > 0 ? "stoppable" : "-";
			rows.Add([ActionLabel(id), reading.Stopped > 0 ? $"{reading.Stopped} (within {reading.LongestStop:F1} s)" : "0",
				reading.WentOff > 0 ? $"{reading.WentOff} ({reading.LatestWentOff:F1} s left at most)" : "0",
				unstoppable ? $"a sleep does not stop it: {lockState}" : lockState]);
		}

		Sort(rows);
		return new LearnedTab("Sleep outcomes", ["Action", "Stopped", "Went off", "Lock"], rows);
	}

	private static LearnedTab SleepKindTab(string? game)
	{
		var rows = new List<string[]>();
		Dictionary<uint, int> resistant;
		SleepLockMarks marks;
		lock (OtherConfiguration.SleepStoreGate)
		{
			resistant = new Dictionary<uint, int>(OtherConfiguration.SleepResistantKinds);
			marks = OtherConfiguration.SleepLocks.Copy();
		}

		foreach (var (kind, count) in resistant)
		{
			var state = SleepLockRules.KindState(resistant, marks, kind, game);
			rows.Add([KindLabel(kind), count.ToString(), state == LockState.None ? "refuted: a sleep landed since"
				: LockText(state, marks.Kinds.GetValueOrDefault(kind), game)]);
		}

		Sort(rows);
		return new LearnedTab("Sleep kinds", ["Enemy kind", "Resisted", "Lock"], rows);
	}

	private static LearnedTab SleepLimitTab(string? game)
	{
		var rows = new List<string[]>();
		Dictionary<uint, int> diminishing;
		SleepLockMarks marks;
		lock (OtherConfiguration.SleepStoreGate)
		{
			diminishing = new Dictionary<uint, int>(OtherConfiguration.SleepDiminishing);
			marks = OtherConfiguration.SleepLocks.Copy();
		}

		var kinds = new HashSet<uint>(diminishing.Keys);
		kinds.UnionWith(marks.Limits.Keys);
		foreach (var kind in kinds)
		{
			var mark = marks.Limits.GetValueOrDefault(kind);
			var limit = SleepLockRules.LimitFor(diminishing, marks, kind, game, out var state);
			var lockText = mark is { Refuted: true }
				? $"refuted: above {mark.Value}" + (limit is { } other ? $"; another kind's {other} applies" : string.Empty)
				: LockText(state, mark, game);
			rows.Add([KindLabel(kind), diminishing.TryGetValue(kind, out var file) ? file.ToString() : "-",
				limit?.ToString() ?? "-", lockText]);
		}

		Sort(rows);
		return new LearnedTab("Sleep limits", ["Enemy kind", "In the file", "Applies", "Lock"], rows);
	}

	// A sleep lock's state in words (concept 13, "Stufe 4", 4b).
	private static string LockText(LockState state, SleepLockMark? mark, string? game)
	{
		return state switch
		{
			LockState.None => "none",
			LockState.TakenOver => "taken over from an earlier version, one try left",
			_ when game == null => "holds: game version not readable",
			_ when mark is { Refuted: false } && mark.Game == game => "holds in this game version",
			_ => "taken over, its try in this version spent",
		};
	}

	private static LearnedTab DotTab()
	{
		var rows = new List<string[]>();
		lock (OtherConfiguration.DotStoreGate)
		{
			foreach (var (area, readings) in OtherConfiguration.DotOnParty)
			{
				foreach (var (status, reading) in readings.Statuses)
				{
					rows.Add([$"{AreaName(area)} ({area})", StatusLabel(status), reading.Ticks.ToString(), $"{reading.TickAmount:F0}",
						reading.Ends.ToString(), $"{reading.Lifetime:F1} s"]);
				}
			}
		}

		Sort(rows);
		return new LearnedTab("Damage over time", ["Area", "Status", "Ticks", "Per tick", "Ends", "Lifetime"], rows);
	}

	private static LearnedTab PenaltyTab(string? game)
	{
		var rows = new List<string[]>();
		lock (OtherConfiguration.ActionPenaltyGate)
		{
			foreach (var (id, status) in OtherConfiguration.ActionPenaltyCasts)
			{
				var sighting = OtherConfiguration.ActionPenaltySightings.GetValueOrDefault(id);
				var here = sighting != null && game != null && sighting.Game == game;
				var before = sighting == null ? 1 : sighting.Before + (here ? 0 : sighting.With);
				rows.Add([ActionLabel(id), StatusLabel(status), sighting is { Refuted: true } ? "refuted: holds no spells" : "holds spells",
					here ? $"{sighting!.With} with, {sighting.Without} without" : "-", before.ToString()]);
			}
		}

		Sort(rows);
		return new LearnedTab("Penalty casts",
			["Action", "Status laid", "State", "Landings in this version", "With the status before"], rows);
	}

	private static LearnedTab MarkerTab(string? game)
	{
		var rows = new List<string[]>();
		var keys = new HashSet<string>(OtherConfiguration.TankbusterMarkerWithoutHit);
		keys.UnionWith(OtherConfiguration.TankbusterMarkerHits.Keys);
		foreach (var key in keys)
		{
			var split = key.IndexOf(':');
			var area = split > 0 && uint.TryParse(key[..split], out var territory) ? $"{AreaName(territory)} ({territory})" : "any (entry from before A340, passed over)";
			var path = split > 0 ? key[(split + 1)..] : key;
			var locked = !OtherConfiguration.TankbusterMarkerWithoutHit.Contains(key) ? "-"
				: DataCenter.MarkerLockHolds(key, trace: false) ? "no tankbuster: holds in this game version"
				: game == null ? "taken over: game version not readable, counts as a tankbuster"
				: "taken over from an earlier version: counts as a tankbuster";
			var hit = OtherConfiguration.TankbusterMarkerHits.TryGetValue(key, out var after)
				? $"{ActionLabel(after.ActionId)} after {after.Delay:F1} s" : "-";
			rows.Add([$"{area} {path}", locked, hit]);
		}

		Sort(rows);
		return new LearnedTab("Tankbuster markers", ["Area and marker", "Lock", "Hit after it"], rows);
	}

	private static void Sort(List<string[]> rows) => rows.Sort((a, b) => string.CompareOrdinal(a[0], b[0]));

	private static string ActionLabel(uint id)
	{
		if (!_learnedActionNames.TryGetValue(id, out var name))
		{
			name = Service.GetSheet<GAction>().GetRowOrDefault(id)?.Name.ExtractText() ?? string.Empty;
			_learnedActionNames[id] = name;
		}

		return name.Length > 0 ? $"{name} ({id})" : $"#{id}";
	}

	private static string StatusLabel(uint id)
	{
		if (!_learnedStatusNames.TryGetValue(id, out var name))
		{
			name = Service.GetSheet<Status>().GetRowOrDefault(id)?.Name.ExtractText() ?? string.Empty;
			_learnedStatusNames[id] = name;
		}

		return name.Length > 0 ? $"{name} ({id})" : $"#{id}";
	}

	private static string AreaName(uint id)
	{
		if (!_learnedAreaNames.TryGetValue(id, out var name))
		{
			name = Service.GetSheet<TerritoryType>().GetRowOrDefault(id)?.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty;
			_learnedAreaNames[id] = name;
		}

		return name.Length > 0 ? name : "?";
	}

	// The game names no enemy kind; a fighter of that kind in view lends its name.
	private static string KindLabel(uint kind)
	{
		foreach (var obj in ECommons.DalamudServices.Svc.Objects)
		{
			if (obj is IBattleChara chara && chara.BaseId == kind)
			{
				return $"{chara.Name.TextValue} ({kind})";
			}
		}

		return $"kind {kind}";
	}
}
