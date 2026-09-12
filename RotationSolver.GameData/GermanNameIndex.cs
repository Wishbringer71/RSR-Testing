using Lumina.Data;
using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.GameData;

/// <summary>
/// Writes the German-to-English name index that the audit scripts and the assistant read.
/// </summary>
/// <remarks>
/// The user plays a German client and states German names; the tree speaks English identifiers.
/// Every lookup in between used to be a research pass, and it produced a wrong assignment
/// (Abtausch to Shirk, from a web search), an invented name ("Armlänge"), and a "not in the tree"
/// report for a name that was in the tree eight times.
/// <para>
/// No admissible outside source is reachable from the build environment: the job guide and XIVAPI
/// are both refused at the proxy (measured, 403 on CONNECT). A datamining mirror would not settle
/// it either - the naming rule admits the job guide, the user's own statement and the game files,
/// and a third-hand copy is none of the three. The game files are the one primary source left, and
/// this program already reads them - it is what generates the English resources in
/// SourceGenerators/Properties. Reading the same sheets a second time under
/// <see cref="Language.German"/> costs one more pass over data that is already on disk.
/// </para>
/// <para>
/// The output is data, not code: <c>.github/scripts/audit/action_names_game.json</c>, checked by
/// check_action_names.py. Its hand-kept counterpart action_names_de.json stays, because it records
/// which names the user actually used and where a statement of his conflicts with the game data -
/// that is a fact about the conversation, and this file cannot carry it.
/// </para>
/// <para>
/// The language is an argument of GetExcelSheet, so one GameData answers both: LuminaOptions
/// documents DefaultExcelLanguage as overridable "on a case-by-case basis". A second instance for
/// German would need the sqpack path a second time, and that path is a per-machine fact - it is a
/// hard-coded constant in Program that holds for no one but its author.
/// </para>
/// <para>
/// One consequence of that API has to be handled here: where a sheet has no row in the requested
/// language, Lumina returns the language-neutral sheet rather than nothing. A pair would then read
/// as German while carrying the English name, which is why an entry is only written when the two
/// differ - not as a shortcut, but because equality is exactly the signature of that fallback.
/// </para>
/// </remarks>
internal static class GermanNameIndex
{
	private sealed record Pair(string De, string En, uint Id, string Kind, string Jobs);

	/// <summary>
	/// Reads every named action, status and item in both languages and writes the pairs as JSON.
	/// </summary>
	/// <param name="gameData">The game data the caller already opened; read twice per sheet here.</param>
	/// <param name="outputPath">Full path of the JSON file to write.</param>
	public static void Write(Lumina.GameData gameData, string outputPath)
	{
		var pairs = new List<Pair>();

		var actionsEn = gameData.GetExcelSheet<Action>(Language.English);
		var actionsDe = gameData.GetExcelSheet<Action>(Language.German);
		if (actionsEn != null && actionsDe != null)
		{
			foreach (var row in actionsEn)
			{
				var en = row.Name.ToString();
				if (string.IsNullOrWhiteSpace(en))
				{
					continue;
				}

				var de = GermanName(() => actionsDe.GetRow(row.RowId).Name.ToString());
				if (de == null || de == en)
				{
					continue;
				}

				// The job category is what makes the file usable as a lookup: the user names an
				// action, and the answer has to say which job it belongs to before anything can be
				// searched for.
				var jobs = row.ClassJobCategory.IsValid
					? row.ClassJobCategory.Value.Name.ToString()
					: string.Empty;
				pairs.Add(new Pair(de, en, row.RowId, "action", jobs));
			}
		}

		var statusEn = gameData.GetExcelSheet<Status>(Language.English);
		var statusDe = gameData.GetExcelSheet<Status>(Language.German);
		if (statusEn != null && statusDe != null)
		{
			foreach (var row in statusEn)
			{
				var en = row.Name.ToString();
				if (string.IsNullOrWhiteSpace(en))
				{
					continue;
				}

				var de = GermanName(() => statusDe.GetRow(row.RowId).Name.ToString());
				if (de == null || de == en)
				{
					continue;
				}

				pairs.Add(new Pair(de, en, row.RowId, "status", string.Empty));
			}
		}

		var itemsEn = gameData.GetExcelSheet<Item>(Language.English);
		var itemsDe = gameData.GetExcelSheet<Item>(Language.German);
		if (itemsEn != null && itemsDe != null)
		{
			foreach (var row in itemsEn)
			{
				var en = row.Name.ToString();
				if (string.IsNullOrWhiteSpace(en))
				{
					continue;
				}

				var de = GermanName(() => itemsDe.GetRow(row.RowId).Name.ToString());
				if (de == null || de == en)
				{
					continue;
				}

				pairs.Add(new Pair(de, en, row.RowId, "item", string.Empty));
			}
		}

		Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
		using var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8);
		writer.WriteLine("{");
		writer.WriteLine("  \"_comment\": [");
		writer.WriteLine("    \"Generated by RotationSolver.GameData - do not edit by hand.\",");
		writer.WriteLine("    \"German and English names straight from the game files, the only primary\",");
		writer.WriteLine("    \"source the build environment can reach. Regenerate after a patch.\"");
		writer.WriteLine("  ],");
		writer.WriteLine("  \"entries\": [");

		for (var i = 0; i < pairs.Count; i++)
		{
			var pair = pairs[i];
			var comma = i == pairs.Count - 1 ? string.Empty : ",";
			writer.WriteLine(
				$"    {{\"de\": {Json(pair.De)}, \"en\": {Json(pair.En)}, \"id\": {pair.Id}, " +
				$"\"kind\": \"{pair.Kind}\", \"jobs\": {Json(pair.Jobs)}}}{comma}");
		}

		writer.WriteLine("  ]");
		writer.WriteLine("}");

		Console.WriteLine($"Wrote {pairs.Count} name pairs to {outputPath}");
	}

	/// <summary>
	/// The German row for an id, or null when it is absent or empty. A sheet can be shorter in one
	/// language than in another, and a row that only exists in English is not a finding here.
	/// </summary>
	private static string? GermanName(Func<string> read)
	{
		try
		{
			var name = read();
			return string.IsNullOrWhiteSpace(name) ? null : name;
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>Minimal JSON string escaping; the names carry quotes and backslashes in a few cases.</summary>
	private static string Json(string value)
	{
		var escaped = value
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("\n", " ")
			.Replace("\r", " ");
		return $"\"{escaped}\"";
	}
}
