using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RotationSolver.Helpers;

namespace RotationSolver.Updaters;

internal static class CancelCastUpdater
{
	private static readonly RandomDelay _tarStopCastDelay = new(() => Service.Config.StopCastingDelay);

	// The raise last broken off because another lands first, so the trace names it once.
	private static (uint Action, ulong Target, float Total) _raiseStopTraced;

	internal static unsafe void UpdateCancelCast()
	{
		var player = Player.Object;
		if (player == null || !player.IsCasting)
		{
			return;
		}

		if (!DataCenter.State)
		{
			return;
		}

		var castTarget = Svc.Objects.SearchById(player.CastTargetObjectId) as IBattleChara;

		var tarDead = Service.Config.UseStopCasting
			&& castTarget != null
			&& castTarget.IsEnemy()
			&& castTarget.CurrentHp == 0;

		// Evaluate the delay every frame so its timer tracks the target's state.
		var stopForDeadTarget = _tarStopCastDelay.Delay(tarDead);

		// Cancel raise cast if target already has Raise status
		var tarHasRaise = castTarget != null && castTarget.HasStatus(false, StatusHelper.RaiseStatus);

		// Cancel a raise another player lands first on the same body (A294). The status above comes only once
		// that raise has landed, so until then both casts ran to the end and ours spent its MP on a body
		// already up. Both casts and their target are on screen, the party list shows them side by side.
		var raiseLandsFirstElsewhere = RaiseLandsFirstElsewhere(player, castTarget);

		// Cancel if a NoCastingStatus is about to resolve - same window and PvP exemption as DoAction, which still starts casts outside of it
		var hasNoCastingStatus = !DataCenter.IsPvP
			&& NoCastingStatusHelper.PlayerHasNoCastingStatus(out var noCastingRemaining)
			&& noCastingRemaining < NoCastingStatusHelper.ResolveWindow;

		// Cancel cast in PvP if an enemy target gains Guard and the action does not ignore Guard
		var tarHasGuard = DataCenter.IsPvP && Service.Config.PvpGuardCancel
			&& castTarget != null
			&& castTarget.IsEnemy()
			&& castTarget.HasStatus(false, StatusID.Guard)
			&& ((ActionID)player.CastActionId).GetActionFromID(true, RotationUpdater.CurrentRotationActions)
				is IBaseAction { Setting.IgnoreGuard: false };

		// Cancel the cast if BossMod's own AI hints (module or AI controller) are requesting a cancel,
		// e.g. because the boss module determined the cast is no longer safe/useful.
		var bmrForceCancelCast = Service.Config.UseBmrTimeline
			&& (DataCenter.BMRForceCancelCast || DataCenter.BMRForceCancelCastAI);

		if (stopForDeadTarget || hasNoCastingStatus || tarHasRaise || raiseLandsFirstElsewhere || tarHasGuard || bmrForceCancelCast || ShouldStopHealing(player))
		{
			var uiState = UIState.Instance();
			if (uiState != null)
			{
				uiState->Hotbar.CancelCast();
			}
		}
	}

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

	private static bool ShouldStopHealing(IPlayerCharacter player)
	{
		return Service.Config.StopHealingAfterThresholdExperimental2
			&& DataCenter.InCombat
			&& !CustomRotation.HealingWhileDoingNothing
			&& DataCenter.CommandNextAction?.AdjustedID != player.CastActionId
			&& ((ActionID)player.CastActionId).GetActionFromID(true, RotationUpdater.CurrentRotationActions)
				is IBaseAction { Setting.GCDSingleHeal: true }
			&& (DataCenter.MergedStatus & (AutoStatus.HealAreaSpell | AutoStatus.HealSingleSpell)) == 0;
	}
}
