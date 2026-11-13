using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RotationSolver.Helpers;

namespace RotationSolver.Updaters;

internal static partial class CancelCastUpdater
{
	// The raise last broken off because another lands first, so the trace names it once.
	private static (uint Action, ulong Target, float Total) _raiseStopTraced;

	// The single-target heal being cast: whether its target needed it, the target's HP and the time when it began, and
	// whether the trace has named it.
	private static (uint Action, ulong Target, float Elapsed, bool NeededAtStart, uint HpAtStart, DateTime Start, bool Traced,
		bool Kept) _singleHealCast;

	private static (uint Action, DateTime At) _cancelTraced = (0, DateTime.MinValue);

	/// <summary>
	/// Whether the player is casting a raise on a body another player's raise lands on first.
	/// </summary>
	/// <remarks>
	/// Equal times keep the cast: the other caster's cast reaches this client later than it started, so
	/// it reads as landing later than it does, and two clients that each wait for the other would leave the
	/// body lying. Should the other cast be broken off after ours, the body is a raise candidate again at the
	/// next target update (TargetUpdater.UpdateCastsByOthers).
	/// </remarks>
	private static bool RaiseLandsFirstElsewhere(IPlayerCharacter player, IBattleChara? castTarget)
	{
		if (castTarget == null || !castTarget.IsDead
			|| !DataCenter.RaisedByOthers.TryGetValue(castTarget.GameObjectId, out var other)
			|| player.CastActionType != (byte)FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action
			|| Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(player.CastActionId) is not { DeadTargetBehaviour: 1 })
		{
			return false;
		}

		var ownLandsAt = DateTime.Now.AddSeconds(Math.Max(0f, player.TotalCastTime - player.CurrentCastTime));
		if (other.LandsAt >= ownLandsAt)
		{
			return false;
		}

		var cast = (player.CastActionId, castTarget.GameObjectId, player.TotalCastTime);
		if (cast != _raiseStopTraced)
		{
			_raiseStopTraced = cast;
			DefenseTrace.Line($"raise stopped: {other.Caster} lands it on {castTarget.Name.TextValue} "
				+ $"{(ownLandsAt - other.LandsAt).TotalSeconds:F1} s before yours");
		}

		return true;
	}
}
