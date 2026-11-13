using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

public static partial class ObjectHelper
{
	/// <summary>
	/// Concept 11, "Tote, die nur die Wiederbelebung des Inhalts aufhebt" (A616): whether only the duty's own raises bring
	/// this corpse back - it carries a status saying so, or the player does: the restriction is the duty's, every player in
	/// it carries it, and whether a corpse still shows its permanent statuses is not known. A job's raise and the feather
	/// leave such a corpse alone.
	/// </summary>
	internal static bool OnlyDutyRaisesReach(this IBattleChara corpse)
	{
		return CarriesResurrectionRestriction(corpse) || DutyRestrictsResurrection;
	}

	/// <summary>
	/// Whether the player carries a status by which only the duty's own raises bring anyone back here (A616).
	/// </summary>
	internal static bool DutyRestrictsResurrection => Player.Object is { } player && CarriesResurrectionRestriction(player);

	private static bool CarriesResurrectionRestriction(IBattleChara battleChara)
	{
		var statuses = battleChara.StatusList;
		if (statuses == null)
		{
			return false;
		}

		foreach (var status in statuses)
		{
			if (status != null && status.StatusId != 0 && StatusHelper.RestrictsResurrection(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}
}
