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
}
