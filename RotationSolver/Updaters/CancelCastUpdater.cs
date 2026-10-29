using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RotationSolver.Helpers;

namespace RotationSolver.Updaters;

internal static class CancelCastUpdater
{
#pragma warning disable IDE0044
	private static RandomDelay _tarStopCastDelay = new(() => Service.Config.StopCastingDelay);
#pragma warning restore IDE0044

	// The raise last broken off because another lands first, so the trace names it once.
	private static (uint Action, ulong Target, float Total) _raiseStopTraced;

	// The single-target heal being cast: whether its target needed it and the target's HP when it began.
	private static (uint Action, ulong Target, float Elapsed, bool NeededAtStart, uint HpAtStart, bool Traced) _singleHealCast;

	internal static unsafe void UpdateCancelCast()
	{
		var player = Player.Object;
		if (player == null || !player.IsCasting)
		{
			_singleHealCast = default;
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

		var stopForDeadTarget = _tarStopCastDelay.Delay(tarDead);

		var tarHasRaise = castTarget != null && castTarget.HasStatus(false, StatusHelper.RaiseStatus);

		// Cancel a raise another player lands first on the same body (A294). The status above comes only once
		// that raise has landed, so until then both casts ran to the end and ours spent its MP on a body
		// already up. Both casts and their target are on screen, the party list shows them side by side.
		var raiseLandsFirstElsewhere = RaiseLandsFirstElsewhere(player, castTarget);

		// Cancel if a NoCastingStatus is about to resolve - same window and PvP exemption as DoAction, which still starts casts outside of it -
		// or at once under one that damages every action, which DoAction does not act under at all (A338)
		var hasNoCastingStatus = !DataCenter.IsPvP
			&& NoCastingStatusHelper.PlayerHasNoCastingStatus(out var noCastingRemaining, out var damagesEveryAction)
			&& (damagesEveryAction || noCastingRemaining < NoCastingStatusHelper.ResolveWindow);

		// Cancel a cast that would still be under way when a status that damages every action lands on the player (A439).
		var castMeetsPenalty = !DataCenter.IsPvP && ActionPenaltyWatch.WouldMeetLanding(player.TotalCastTime - player.CurrentCastTime);
		if (castMeetsPenalty)
		{
			ActionPenaltyWatch.NoteHold("own cast cancelled");
		}

		var tarHasGuard = DataCenter.IsPvP && Service.Config.PvpGuardCancel
			&& castTarget != null
			&& castTarget.IsEnemy()
			&& castTarget.HasStatus(false, StatusID.Guard)
			&& ((ActionID)player.CastActionId).GetActionFromID(true, RotationUpdater.CurrentRotationActions)
				is IBaseAction { Setting.IgnoreGuard: false };

		var bmrForceCancelCast = Service.Config.UseBmrTimeline
			&& (DataCenter.BMRForceCancelCast || DataCenter.BMRForceCancelCastAI);

		if (stopForDeadTarget || hasNoCastingStatus || castMeetsPenalty || tarHasRaise || raiseLandsFirstElsewhere || tarHasGuard || bmrForceCancelCast || ShouldStopHealing(player, castTarget))
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

	/// <summary>
	/// "Stop single target GCD healing after reaching threshold": the target of the heal being cast needed it
	/// when the cast began, has been healed since, and no longer needs it.
	/// </summary>
	/// <remarks>
	/// It used to ask whether anybody still called for a heal spell. That flag reads the forecast as well, and the
	/// forecast drops out as soon as a falling trend levels off - with nobody healed, a heal begun for a predicted
	/// danger was broken off and begun again at the next hit. It also broke off casts that are no threshold heal at
	/// all: a barrier on a tank at full health, such as the Adloquium before Recitation runs out (A314). Both stayed
	/// hidden while the setting had no effect; it only reaches the casts since upstream 7.5.7.5 (A332).
	/// </remarks>
	private static bool ShouldStopHealing(IPlayerCharacter player, IBattleChara? castTarget)
	{
		if (!Service.Config.StopHealingAfterThresholdExperimental2 || castTarget == null
			|| ((ActionID)player.CastActionId).GetActionFromID(true, RotationUpdater.CurrentRotationActions)
				is not IBaseAction { Setting.GCDSingleHeal: true })
		{
			return false;
		}

		var needed = StateUpdater.NeedsSingleHealSpell(castTarget);

		// A cast is new when it is not the one recorded or its bar has started over.
		if (_singleHealCast.Action != player.CastActionId || _singleHealCast.Target != castTarget.GameObjectId
			|| player.CurrentCastTime < _singleHealCast.Elapsed)
		{
			_singleHealCast = (player.CastActionId, castTarget.GameObjectId, player.CurrentCastTime, needed, castTarget.CurrentHp, false);
			return false;
		}

		_singleHealCast.Elapsed = player.CurrentCastTime;

		var stop = DataCenter.InCombat
			&& !CustomRotation.HealingWhileDoingNothing
			&& DataCenter.CommandNextAction?.AdjustedID != player.CastActionId
			&& _singleHealCast.NeededAtStart && !needed
			&& castTarget.CurrentHp > _singleHealCast.HpAtStart;

		if (stop && !_singleHealCast.Traced)
		{
			_singleHealCast.Traced = true;
			DefenseTrace.Line($"heal stopped: {castTarget.Name.TextValue} healed from {_singleHealCast.HpAtStart} to "
				+ $"{castTarget.CurrentHp} HP during the cast, above the single heal threshold");
		}

		return stop;
	}
}
