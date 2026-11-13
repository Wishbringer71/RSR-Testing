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
	/// Which attacks this status turns back on whoever makes them, by its own text in the English status sheet: spikes,
	/// counters and reflections (concept 03, "Gegner mit Stacheln", A571, A588). "physical" in the text narrows it to
	/// physical attacks, "magic" or "-aspected" to magic ones, "ranged" to attacks from range; otherwise every attack. None
	/// for any other status, and when the sheet cannot be read.
	/// </summary>
	internal static AttackKinds CounteredAttacks(uint statusId)
	{
		return _counteredAttacks.GetOrAdd(statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				if (sheet == null || !sheet.TryGetRow(key, out var row))
				{
					return AttackKinds.None;
				}

				var text = row.Description.ExtractText();
				if (!HitsAttackers.IsMatch(text) && !(Reflects.IsMatch(text) && ReflectsAttacks.IsMatch(text)))
				{
					return AttackKinds.None;
				}

				var kinds = AttackKinds.None;
				if (text.Contains("physical", StringComparison.OrdinalIgnoreCase))
				{
					kinds |= AttackKinds.Physical;
				}

				if (text.Contains("magic", StringComparison.OrdinalIgnoreCase) || text.Contains("-aspected", StringComparison.OrdinalIgnoreCase))
				{
					kinds |= AttackKinds.Magic;
				}

				if (text.Contains("ranged", StringComparison.OrdinalIgnoreCase))
				{
					kinds |= AttackKinds.Ranged;
				}

				return kinds == AttackKinds.None ? AttackKinds.Any : kinds;
			}
			catch (Exception)
			{
				return AttackKinds.None;
			}
		});
	}

	/// <summary>
	/// Whether the next damage kills the bearer of this status, by its own text in the English status sheet: "Turned into
	/// stone. Taking damage results in instant death." (Stone Curse 437, 1417, 4537, 5058; xivapi, 05.11.2026). Concept 03,
	/// "Tötungsregeln" (A588). A sheet that cannot be read answers no.
	/// </summary>
	internal static bool DiesToAnyHit(uint statusId)
	{
		return _diesToAnyHit.GetOrAdd(statusId, static key =>
		{
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
				return sheet != null && sheet.TryGetRow(key, out var row)
					&& row.Description.ExtractText().Contains("Taking damage results in instant death", StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		});
	}
}
