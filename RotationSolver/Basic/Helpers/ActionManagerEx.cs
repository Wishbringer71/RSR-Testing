using Dalamud.Hooking;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using RotationSolver.Basic.Tweaks;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Applies the animation lock and cooldown delay tweaks to every action the client sends, whether RSR or the player used it.
/// </summary>
public sealed unsafe partial class ActionManagerEx : IDisposable
{
	private static ActionManagerEx? _instance;

	/// <summary>
	/// Gets the singleton instance of <see cref="ActionManagerEx"/>.
	/// </summary>
	public static ActionManagerEx Instance => _instance ??= new ActionManagerEx();

	/// <summary>
	/// Gets the <see cref="AnimationLockTweak"/> instance used for animation lock timing adjustments.
	/// </summary>
	public AnimationLockTweak AnimationLockTweak { get; } = new();

	/// <summary>
	/// Gets the <see cref="CooldownDelayTweak"/> instance used for cooldown delay adjustments.
	/// </summary>
	public CooldownDelayTweak CooldownDelayTweak { get; } = new();

	private delegate void UpdateDelegate(ActionManager* self);
	private delegate bool UseActionLocationDelegate(ActionManager* self, ActionType actionType, uint actionId, ulong targetId, Vector3* location, uint extraParam, byte comboRouteId);
	private delegate void ReceiveActionEffectDelegate(uint casterEntityId, Character* casterPtr, Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targetEntityIds);

	private readonly Hook<UpdateDelegate>? _updateHook;
	private readonly Hook<UseActionLocationDelegate>? _useActionLocationHook;
	private readonly Hook<ReceiveActionEffectDelegate>? _receiveActionEffectHook;

	private ActionManagerEx()
	{
		try
		{
			_updateHook = Svc.Hook.HookFromAddress<UpdateDelegate>(ActionManager.Addresses.Update.Value, UpdateDetour);
			_useActionLocationHook = Svc.Hook.HookFromAddress<UseActionLocationDelegate>(ActionManager.Addresses.UseActionLocation.Value, UseActionLocationDetour);
			_receiveActionEffectHook = Svc.Hook.HookFromAddress<ReceiveActionEffectDelegate>(ActionEffectHandler.Addresses.Receive.Value, ReceiveActionEffectDetour);
			_updateHook.Enable();
			_useActionLocationHook.Enable();
			_receiveActionEffectHook.Enable();
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[ActionManagerEx] Failed to hook the action manager: {ex}");
		}
	}

	internal static void Enable()
	{
		_ = Instance;
	}

	internal static void DisposeInstance()
	{
		_instance?.Dispose();
		_instance = null;
	}

	private void UpdateDetour(ActionManager* self)
	{
		if (Service.Config.RemoveCooldownDelay && self->ActionQueued)
		{
			CooldownDelayTweak.StartAdjustment(self->AnimationLock, GetRemainingCooldown(self, self->QueuedActionType, self->QueuedActionId),
				Framework.Instance()->FrameDeltaTime);
		}

		try
		{
			_updateHook!.Original(self);
			// Clipping, GCD time with nothing queued and the fight's line (concept 03, A753).
			AnimationLockTweak.Frame(self, Framework.Instance()->FrameDeltaTime);
		}
		finally
		{
			CooldownDelayTweak.StopAdjustment();
		}
	}

	private bool UseActionLocationDetour(ActionManager* self, ActionType actionType, uint actionId, ulong targetId, Vector3* location, uint extraParam, byte comboRouteId)
	{
		var prevSequence = self->LastUsedActionSequence;

		// A786 (concept 13, "Richtig stehen und weiter treffen"): every action the game runs passes here - RSR's press, the
		// game's queue and the player's own press - so a gaze due gets a safe facing instead of the game's turn at the target.
		var facing = actionType == ActionType.Action && DataCenter.IsActivated() ? GazeWatch.PrepareFacingFor(actionId, targetId, location) : null;
		bool result;
		try
		{
			result = _useActionLocationHook!.Original(self, actionType, actionId, targetId, location, extraParam, comboRouteId);
		}
		finally
		{
			GazeWatch.RestoreFacing(facing);
		}

		if (self->LastUsedActionSequence == prevSequence)
		{
			return result;
		}

		// The moment a request without a cast leaves, for the tankbuster plan's round trip (concept 09, A340).
		if (self->CastSpellId == 0)
		{
			TankbusterForecast.RecordRequest(self->LastUsedActionSequence);
		}

		try
		{
			var adjustment = CooldownDelayTweak.Adjustment;
			if (adjustment > 0)
			{
				var group = self->GetRecastGroup((int)actionType, actionId);
				var additionalGroup = self->GetAdditionalRecastGroup(actionType, actionId);
				AdjustRecast(self, group, adjustment);
				if (additionalGroup != group)
				{
					AdjustRecast(self, additionalGroup, adjustment);
				}

				self->AnimationLock = Math.Max(0, self->AnimationLock - adjustment);
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[ActionManagerEx] Used {actionId} {adjustment:f3}s earlier to make up for the frame rate.");
				}
			}

			// Recorded with the setting off too: the weave window counts with the delay measured on it (A753).
			AnimationLockTweak.RecordRequest(self->LastUsedActionSequence, self->AnimationLock);
			AnimationLockTweak.NoteRequest(self, actionType, actionId);
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"[ActionManagerEx] Failed to apply the action tweaks: {ex.Message}");
		}

		return result;
	}

	private void ReceiveActionEffectDetour(uint casterEntityId, Character* casterPtr, Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targetEntityIds)
	{
		var actionManager = ActionManager.Instance();
		var packetAnimLock = header->AnimationLock;
		var prevAnimLock = actionManager == null ? 0 : actionManager->AnimationLock;

		_receiveActionEffectHook!.Original(casterEntityId, casterPtr, targetPos, header, effects, targetEntityIds);

		if (header->SourceSequence != 0 && casterEntityId == Player.Object?.EntityId)
		{
			TankbusterForecast.RecordRequestAnswered(header->SourceSequence);
		}

		// Measured with the setting off too; only the reduction asks it (A753).
		if (actionManager == null || header->SourceSequence == 0 || casterEntityId != Player.Object?.EntityId)
		{
			return;
		}

		try
		{
			AnimationLockTweak.NoteAnswer(header->SourceSequence);
			var reduction = AnimationLockTweak.Apply(header->SourceSequence, prevAnimLock, actionManager->AnimationLock,
				packetAnimLock, header->AnimationLock, out var delay);
			if (reduction > 0)
			{
				actionManager->AnimationLock = Math.Max(0, actionManager->AnimationLock - reduction);
				AnimationLockTweak.NoteReduction(reduction);
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"[ActionManagerEx] Reduced animation lock by {reduction:f3}s (delay: {delay:f3}s)");
				}
			}

			if (AnimationLockTweak.LastMatched)
			{
				AnimationLockTweak.Observe(header->ActionId, packetAnimLock, delay, actionManager->AnimationLock);
			}
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"[ActionManagerEx] Failed to apply the animation lock tweak: {ex.Message}");
		}
	}

	private static float GetRemainingCooldown(ActionManager* actionManager, ActionType actionType, uint actionId)
	{
		var group = actionManager->GetRecastGroup((int)actionType, actionId);
		if (group < 0)
		{
			return 0;
		}

		var detail = actionManager->GetRecastGroupDetail(group);
		return detail == null || !detail->IsActive ? 0 : Math.Max(0, detail->Total - detail->Elapsed);
	}

	private static void AdjustRecast(ActionManager* actionManager, int group, float adjustment)
	{
		if (group < 0)
		{
			return;
		}

		var detail = actionManager->GetRecastGroupDetail(group);
		if (detail != null && detail->IsActive)
		{
			detail->Elapsed = Math.Min(detail->Total, detail->Elapsed + adjustment);
		}
	}

	/// <summary>
	/// Get animation lock delay estimate for rotation timing
	/// </summary>
	public float GetAnimationLockDelayEstimate() => AnimationLockTweak.DelayEstimate;

	/// <summary>
	/// Get current cooldown adjustment
	/// </summary>
	public float GetCooldownAdjustment() => CooldownDelayTweak.Adjustment;

	/// <summary>
	/// Releases all resources used by the <see cref="ActionManagerEx"/> instance.
	/// </summary>
	public void Dispose()
	{
		_updateHook?.Dispose();
		_useActionLocationHook?.Dispose();
		_receiveActionEffectHook?.Dispose();
		CooldownDelayTweak.StopAdjustment();
		// BossMod's and NoClippy's lock functions come back as RSR's go (A753).
		AnimationLockTweak.GiveBackOtherTools();
	}
}
