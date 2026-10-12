using Dalamud.Game;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Durations a trait sets where the action's effect text leaves the number blank ("Duration: s") -
/// Reprisal, Feint, Addle, Sheltron, Nascent Flash -, read in the game from the client's own trait
/// sheets: "Extends the duration of Reprisal to 15 seconds", learned at the trait's level.
/// </summary>
/// <remarks>
/// What the game gives at runtime weighs more than a table fetched beforehand (the owner's rule,
/// 12.10.2026): it shows the patch that is running. The fetched table
/// (<see cref="DefensiveValues.TraitDurationByActionId"/>) stays as the fallback where the game gives
/// nothing - weighting, not exclusion (his precision). The sentence is read in the English sheet,
/// whatever the client language, so a German client reads the same figure.
/// </remarks>
internal static class TraitDurations
{
	private static readonly Regex Extends = new(@"Extends the duration of (?<name>.+?) to (?<seconds>\d{1,3}) seconds", RegexOptions.Compiled);
	private static readonly object _gate = new();

	// English action name to the traits that set its duration, (seconds, level), lowest level first.
	private static Dictionary<string, List<(float Seconds, int Level)>>? _byName;
	private static readonly Dictionary<uint, List<(float Seconds, int Level)>?> _byAction = [];

	/// <summary>
	/// Whether the game states a trait duration for <paramref name="actionId"/>: the trait with the highest
	/// level the player's synced level has, or - when none is learned yet - the lowest one, so that the
	/// caller's level check still answers no. False when no trait names the action, or the sheets cannot
	/// be read.
	/// </summary>
	public static bool TryInGame(uint actionId, out (float Seconds, int Level) trait)
	{
		trait = default;
		var traits = Of(actionId);
		if (traits == null || traits.Count == 0)
		{
			return false;
		}

		var level = DataCenter.PlayerSyncedLevel();
		trait = traits[0];
		foreach (var candidate in traits)
		{
			if (candidate.Level <= level)
			{
				trait = candidate;
			}
		}

		return true;
	}

	private static List<(float Seconds, int Level)>? Of(uint actionId)
	{
		lock (_gate)
		{
			if (_byAction.TryGetValue(actionId, out var known))
			{
				return known;
			}

			List<(float Seconds, int Level)>? found = null;
			try
			{
				_byName ??= Read();
				var actions = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>(ClientLanguage.English);
				if (actions != null && actions.TryGetRow(actionId, out var row))
				{
					_ = _byName.TryGetValue(row.Name.ExtractText(), out found);
				}
			}
			catch (Exception)
			{
				// A sheet that cannot be read answers nothing; the fetched table is the fallback.
				found = null;
			}

			_byAction[actionId] = found;
			return found;
		}
	}

	private static Dictionary<string, List<(float Seconds, int Level)>> Read()
	{
		Dictionary<string, List<(float Seconds, int Level)>> byName = [];
		var texts = Svc.Data.GetExcelSheet<TraitTransient>(ClientLanguage.English);
		var traits = Svc.Data.GetExcelSheet<Trait>(ClientLanguage.English);
		if (texts == null || traits == null)
		{
			return byName;
		}

		foreach (var text in texts)
		{
			var match = Extends.Match(text.Description.ExtractText());
			if (!match.Success || !traits.TryGetRow(text.RowId, out var trait) || trait.Level == 0)
			{
				continue;
			}

			// "Nascent Flash and Nascent Glint" extends both; the action is the first.
			var name = match.Groups["name"].Value;
			var and = name.IndexOf(" and ", StringComparison.Ordinal);
			if (and > 0)
			{
				name = name[..and];
			}

			if (!byName.TryGetValue(name, out var list))
			{
				list = [];
				byName[name] = list;
			}

			list.Add((float.Parse(match.Groups["seconds"].Value, System.Globalization.CultureInfo.InvariantCulture), trait.Level));
			list.Sort((a, b) => a.Level.CompareTo(b.Level));
		}

		return byName;
	}
}
