using Dalamud.Game;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver.Helpers;

internal static class NoCastingStatusHelper
{
	internal const float ResolveWindow = 3f;

	internal static bool PlayerHasNoCastingStatus(out float minRemainingTime)
	{
		return PlayerHasNoCastingStatus(out minRemainingTime, out _);
	}

	/// <summary>
	/// As <see cref="PlayerHasNoCastingStatus(out float)"/>, and whether one of the matching statuses deals damage with
	/// every action the player takes (<see cref="DamagesEveryAction"/>).
	/// </summary>
	/// <param name="minRemainingTime">The shortest remaining time among matching statuses; permanent statuses count as <see cref="float.MaxValue"/>.</param>
	/// <param name="damagesEveryAction">True when a matching status punishes every action for as long as it lasts.</param>
	/// <returns>True if the player has at least one no-casting status.</returns>
	internal static bool PlayerHasNoCastingStatus(out float minRemainingTime, out bool damagesEveryAction)
	{
		minRemainingTime = float.MaxValue;
		damagesEveryAction = false;

		var noCastingStatus = OtherConfiguration.NoCastingStatus;
		if (noCastingStatus == null || noCastingStatus.Count == 0)
		{
			return false;
		}

		var statusList = Player.Object?.StatusList;
		if (statusList == null)
		{
			return false;
		}

		var found = false;
		foreach (var status in statusList)
		{
			if (status == null || status.StatusId == 0 || !noCastingStatus.Contains(status.StatusId))
			{
				continue;
			}

			found = true;
			damagesEveryAction |= DamagesEveryAction(status.StatusId);
			var remaining = status.RemainingTime == 0f ? float.MaxValue : status.RemainingTime;
			if (remaining < minRemainingTime)
			{
				minRemainingTime = remaining;
			}
		}

		return found;
	}

	private static readonly Dictionary<uint, bool> _damagesEveryAction = [];
	private static readonly object _gate = new();

	// The game's own wording for the two statuses in it that punish every action: Pyretic, "Fire-aspected damage is
	// taken with every action" (six rows), and Causality, "Damage is taken each time an action is used" (Status sheet,
	// English, 24.10.2026). No field of the sheet tells them from a penalty on expiry (A336).
	private static readonly Regex EveryActionWording = new(@"with every action|each time an action", RegexOptions.IgnoreCase);

	/// <summary>
	/// Whether the status deals damage with every action for as long as it lasts, by its own effect text in the client's
	/// English sheet, whatever language the client shows. The window before expiry (<see cref="ResolveWindow"/>) fits a
	/// status whose penalty falls when it ends (Acceleration Bomb); under this kind every action until then costs health,
	/// and the list's text says "Do not take action if you have one of these debuffs" (A338).
	/// </summary>
	/// <param name="statusId">The status.</param>
	/// <returns>True for such a status; false when the sheet cannot be read.</returns>
	internal static bool DamagesEveryAction(uint statusId)
	{
		lock (_gate)
		{
			if (_damagesEveryAction.TryGetValue(statusId, out var known))
			{
				return known;
			}

			var result = false;
			try
			{
				var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English);
				if (sheet != null && sheet.TryGetRow(statusId, out var row))
				{
					result = EveryActionWording.IsMatch(row.Description.ExtractText());
				}
			}
			catch (Exception)
			{
				// A sheet that cannot be read answers nothing; the status keeps the window before expiry, as before.
				result = false;
			}

			_damagesEveryAction[statusId] = result;
			return result;
		}
	}
}
