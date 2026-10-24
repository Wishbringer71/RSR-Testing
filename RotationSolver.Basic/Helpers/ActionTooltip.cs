using Dalamud.Game;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// An action's effect text as its tooltip shows it at the player's synced level: the client's effect text
/// (ActionTransient, English sheet) evaluated by Dalamud's SeString evaluator. The evaluation fills in the figures a
/// level or trait decides, which the raw text - and ActionId.resx made from it - leaves blank ("a potency of .",
/// "Duration: s"). English whatever the client language, so a German client reads the same figures (A316, A342).
/// </summary>
/// <remarks>
/// What the game gives at runtime weighs more than a table fetched beforehand (the owner's rule, 12.10.2026): it shows
/// the patch that is running. Whether the evaluation follows a level sync is not documented; a reader whose answer
/// depends on it says how it guards against a figure of the unsynced level.
/// </remarks>
internal static class ActionTooltip
{
	private static readonly Dictionary<(uint ActionId, int Level), string> _texts = [];
	private static readonly object _gate = new();

	private static readonly Regex StatedPotency = new(@"potency of (?<value>\d[\d,]*)", RegexOptions.Compiled);
	private static readonly Regex StatedFalloff = new(@"(?<value>\d{1,2})% less for all remaining enemies", RegexOptions.Compiled);

	/// <summary>
	/// The evaluated effect text of <paramref name="actionId"/>, empty when the sheet has no row for it or the
	/// evaluation fails.
	/// </summary>
	public static string Text(uint actionId)
	{
		var level = DataCenter.PlayerSyncedLevel();
		lock (_gate)
		{
			if (_texts.TryGetValue((actionId, level), out var known))
			{
				return known;
			}

			var text = string.Empty;
			try
			{
				var sheet = Svc.Data.GetExcelSheet<ActionTransient>(ClientLanguage.English);
				if (sheet != null && sheet.TryGetRow(actionId, out var row))
				{
					text = Svc.SeStringEvaluator.Evaluate(row.Description, default, ClientLanguage.English).ExtractText();
				}
			}
			catch (Exception)
			{
				// A sheet or an evaluation that fails answers nothing; each reader keeps its own fallback.
				text = string.Empty;
			}

			_texts[(actionId, level)] = text;
			return text;
		}
	}

	/// <summary>
	/// The figure the first match of <paramref name="pattern"/> captures as "value" in the text of
	/// <paramref name="actionId"/>; false when the text states none.
	/// </summary>
	public static bool TryStated(uint actionId, Regex pattern, out float value)
	{
		value = 0f;
		var match = pattern.Match(Text(actionId));
		return match.Success && float.TryParse(match.Groups["value"].Value.Replace(",", string.Empty),
			NumberStyles.Float, CultureInfo.InvariantCulture, out value);
	}

	/// <summary>
	/// The potency one use of <paramref name="actionId"/> deals over <paramref name="targets"/> enemies, as its tooltip
	/// states it: the first enemy's potency, every further one less by the stated falloff ("30% less for all remaining
	/// enemies"), every one alike where the text states none. False when the text states no potency.
	/// </summary>
	/// <remarks>
	/// Only the first stated potency counts: a bonus under a status ("Meisui Bonus: Potency is increased to 550") and a
	/// damage over time are left out, and a reader whose comparison hinges on them weighs them itself.
	/// </remarks>
	public static bool TryPotencyOn(uint actionId, int targets, out float total)
	{
		total = 0f;
		if (targets < 1 || !TryStated(actionId, StatedPotency, out var first))
		{
			return false;
		}

		var rest = TryStated(actionId, StatedFalloff, out var falloff) ? first * (1f - (falloff / 100f)) : first;
		total = first + (rest * (targets - 1));
		return true;
	}

	/// <summary>
	/// Whether <paramref name="area"/> on <paramref name="targets"/> enemies deals more than <paramref name="single"/>
	/// on one, by the potencies their tooltips state (<see cref="TryPotencyOn"/>). True when either cannot be read, so
	/// a caller falls back to what it did without the comparison.
	/// </summary>
	public static bool AreaOutdamagesSingle(uint area, int targets, uint single)
	{
		return !TryPotencyOn(area, targets, out var areaTotal) || !TryPotencyOn(single, 1, out var singleTotal)
			|| areaTotal > singleTotal;
	}
}
