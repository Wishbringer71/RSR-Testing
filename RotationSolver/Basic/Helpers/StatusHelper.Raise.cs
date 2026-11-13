using Dalamud.Game.ClientState.Statuses;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

public static partial class StatusHelper
{
	/// <summary>
	/// Concept 11, "Tote, die nur die Wiederbelebung des Inhalts aufhebt" (A616): whether this status lets only the duty's
	/// own means raise its bearer, by its text in the English status sheet - "making resurrection by certain means
	/// impossible" (1755), "Resurrection by certain means is impossible." (2449, 3380), "Resurrection can only be attempted
	/// by certain means." (4262); xivapi, 06.11.2026. A sheet that cannot be read answers no.
	/// </summary>
	internal static bool RestrictsResurrection(uint statusId)
	{
		return _restrictsResurrection.GetOrAdd(statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				if (sheet == null || !sheet.TryGetRow(key, out var row))
				{
					return false;
				}

				var text = row.Description.ExtractText();
				return text.Contains("resurrection", StringComparison.OrdinalIgnoreCase)
					&& text.Contains("by certain means", StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		});
	}
}
