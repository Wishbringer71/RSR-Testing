using Dalamud.Game;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Durations a trait sets where the action's effect text leaves the number blank ("Duration: s") -
/// Reprisal, Feint, Addle, Sheltron, Nascent Flash -, read in the game from the client's own trait
/// sheets: "Extends the duration of Reprisal to 15 seconds", learned at the trait's level. The same
/// for a damage reduction a trait raises and the effect text leaves blank ("by %"): "Improves
/// Troubadour's damage reduction effect to 15%" (A286, G6).
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
	private static readonly Regex Improves = new(@"Improves (?<name>.+?)'s damage reduction effect to (?<percent>\d{1,2})%", RegexOptions.Compiled);
	private static readonly object _gate = new();

	// English action name to the traits that set its duration, (seconds, level), lowest level first.
	private static Dictionary<string, List<(float Seconds, int Level)>>? _byName;
	private static readonly Dictionary<uint, List<(float Seconds, int Level)>?> _byAction = [];

	// English action name to the traits that raise its damage reduction, (share, level), lowest level first.
	private static Dictionary<string, List<(float Seconds, int Level)>>? _sharesByName;
	private static readonly Dictionary<uint, List<(float Seconds, int Level)>?> _sharesByAction = [];

	/// <summary>
	/// Whether the game states a trait duration for <paramref name="actionId"/>: the trait with the highest
	/// level the player's synced level has, or - when none is learned yet - the lowest one, so that the
	/// caller's level check still answers no. False when no trait names the action, or the sheets cannot
	/// be read.
	/// </summary>
	public static bool TryInGame(uint actionId, out (float Seconds, int Level) trait)
	{
		return Pick(Of(actionId, false), out trait);
	}

	/// <summary>
	/// Whether the game states a trait that raises the damage reduction of <paramref name="actionId"/>:
	/// the share (0.15 for 15 %) and level, chosen as in <see cref="TryInGame"/>.
	/// </summary>
	public static bool TryShareInGame(uint actionId, out (float Share, int Level) trait)
	{
		var found = Pick(Of(actionId, true), out var picked);
		trait = (picked.Seconds, picked.Level);
		return found;
	}

	private static bool Pick(List<(float Seconds, int Level)>? traits, out (float Seconds, int Level) trait)
	{
		trait = default;
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

	private static List<(float Seconds, int Level)>? Of(uint actionId, bool shares)
	{
		lock (_gate)
		{
			var cache = shares ? _sharesByAction : _byAction;
			if (cache.TryGetValue(actionId, out var known))
			{
				return known;
			}

			List<(float Seconds, int Level)>? found = null;
			try
			{
				if (_byName == null || _sharesByName == null)
				{
					(_byName, _sharesByName) = Read();
				}

				var actions = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>(ClientLanguage.English);
				if (actions != null && actions.TryGetRow(actionId, out var row))
				{
					_ = (shares ? _sharesByName : _byName).TryGetValue(row.Name.ExtractText(), out found);
				}
			}
			catch (Exception)
			{
				// A sheet that cannot be read answers nothing; the fetched table is the fallback.
				found = null;
			}

			cache[actionId] = found;
			return found;
		}
	}

	private static (Dictionary<string, List<(float Seconds, int Level)>> Durations,
		Dictionary<string, List<(float Seconds, int Level)>> Shares) Read()
	{
		Dictionary<string, List<(float Seconds, int Level)>> durations = [];
		Dictionary<string, List<(float Seconds, int Level)>> shares = [];
		var texts = Svc.Data.GetExcelSheet<TraitTransient>(ClientLanguage.English);
		var traits = Svc.Data.GetExcelSheet<Trait>(ClientLanguage.English);
		if (texts == null || traits == null)
		{
			return (durations, shares);
		}

		foreach (var text in texts)
		{
			if (!traits.TryGetRow(text.RowId, out var trait) || trait.Level == 0)
			{
				continue;
			}

			var description = text.Description.ExtractText();
			var match = Extends.Match(description);
			if (match.Success)
			{
				// "Nascent Flash and Nascent Glint" extends both; the action is the first.
				var name = match.Groups["name"].Value;
				var and = name.IndexOf(" and ", StringComparison.Ordinal);
				if (and > 0)
				{
					name = name[..and];
				}

				Add(durations, name, float.Parse(match.Groups["seconds"].Value, System.Globalization.CultureInfo.InvariantCulture), trait.Level);
			}

			var improves = Improves.Match(description);
			if (improves.Success)
			{
				Add(shares, improves.Groups["name"].Value,
					float.Parse(improves.Groups["percent"].Value, System.Globalization.CultureInfo.InvariantCulture) / 100f, trait.Level);
			}
		}

		return (durations, shares);
	}

	private static void Add(Dictionary<string, List<(float Seconds, int Level)>> byName, string name, float figure, int level)
	{
		if (!byName.TryGetValue(name, out var list))
		{
			list = [];
			byName[name] = list;
		}

		list.Add((figure, level));
		list.Sort((a, b) => a.Level.CompareTo(b.Level));
	}
}
