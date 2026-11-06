using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	// Concept 03, "Limit Break allein" (A628): the animation lock of each Limit Break, measured on its own use this session.
	private static readonly Dictionary<uint, float> _limitBreakLock = [];
	private static (uint Action, ushort Units, DateTime At) _limitBreakUsed = (0, 0, DateTime.MinValue);

	/// <summary>
	/// A damage dealer with no other player in the party: the job's Limit Break of the full bar, while no announced hit on the
	/// player lands within its cast and animation lock, BossMod's AI does not move him and he is not in the critical class.
	/// </summary>
	private unsafe bool LimitBreakWhenAlone(out IAction? act)
	{
		act = null;
		var controller = &UIState.Instance()->LimitBreakController;
		MeasureLimitBreakLock(controller);
		var player = Player;
		if (!Service.Config.UseLimitBreakWhenAlone || !DataCenter.InCombat || DataCenter.IsPvP || player == null
			|| DataCenter.Role is not (JobRole.Melee or JobRole.RangedPhysical or JobRole.RangedMagical)
			|| controller->BarCount == 0 || controller->BarUnits == 0
			|| controller->CurrentUnits < controller->BarCount * controller->BarUnits || OtherPlayerInParty(player))
		{
			return false;
		}

		// The game names the Limit Break of the player's class for the level the full bar gives.
		var id = controller->GetActionId((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)player.Address,
			(byte)(controller->BarCount - 1));
		IBaseAction? limitBreak = null;
		foreach (var candidate in new[] { LimitBreak1, LimitBreak2, LimitBreak3 })
		{
			if (candidate != null && candidate.ID == id)
			{
				limitBreak = candidate;
			}
		}

		if (limitBreak == null)
		{
			return false;
		}

		var window = limitBreak.Info.CastTime
			+ (_limitBreakLock.TryGetValue(limitBreak.ID, out var measured) ? measured : LockBound(controller->BarCount));
		// BossMod interrupts a cast longer than its MaxCastTime, which it lowers before its AI or a mechanic moves the player.
		if (player.IsInCriticalClass() || DataCenter.BMRIsMoving || BossModWouldInterrupt(limitBreak.Info.CastTime)
			|| RaiseProtection.FirstWithin(window) != null || !IsLongerThan(window))
		{
			return false;
		}

		IBaseAction.LimitBreakPathRunning = true;
		try
		{
			if (!limitBreak.CanUse(out act, skipAoeCheck: true))
			{
				return false;
			}
		}
		finally
		{
			IBaseAction.LimitBreakPathRunning = false;
		}

		if (_limitBreakUsed.Action != limitBreak.ID || (DateTime.Now - _limitBreakUsed.At).TotalSeconds > window)
		{
			DefenseTrace.Line($"limit break: {limitBreak.Name} (level {controller->BarCount}) - full bar, no other player, "
				+ $"no announced hit within {window:F1} s");
		}

		_limitBreakUsed = (limitBreak.ID, controller->CurrentUnits, DateTime.Now);
		return true;
	}

	// Upper bound of the lock per level until the first own measurement: the shortest gap from a Limit Break's hit to the
	// same player's next action in the owner's ACT logs of 25.10.-05.11.2026, reaction included (A628).
	private static readonly float[] LockBounds = [4.4f, 5.8f, 8.4f];

	private static float LockBound(int level)
	{
		return LockBounds[Math.Clamp(level, 1, LockBounds.Length) - 1];
	}

	// Once the bar has fallen after RSR chose a Limit Break, the first animation lock after its cast is its lock.
	private static unsafe void MeasureLimitBreakLock(LimitBreakController* controller)
	{
		if (_limitBreakUsed.Action == 0 || _limitBreakLock.ContainsKey(_limitBreakUsed.Action))
		{
			return;
		}

		var player = Player;
		var actions = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		if (player == null || actions == null || controller->CurrentUnits >= _limitBreakUsed.Units || player.IsCasting
			|| actions->AnimationLock <= 0f)
		{
			if ((DateTime.Now - _limitBreakUsed.At).TotalSeconds > DataCenter.DefaultGCDTotal
				&& controller->CurrentUnits >= _limitBreakUsed.Units)
			{
				_limitBreakUsed = (0, 0, DateTime.MinValue);
			}

			return;
		}

		_limitBreakLock[_limitBreakUsed.Action] = actions->AnimationLock;
		DefenseTrace.Line($"limit break: lock of #{_limitBreakUsed.Action} measured at {actions->AnimationLock:F2} s");
		_limitBreakUsed = (0, 0, DateTime.MinValue);
	}

	private static bool BossModWouldInterrupt(float castTime)
	{
		return castTime > 0f && DataCenter.BMRMaxCastTime < castTime;
	}

	private static bool OtherPlayerInParty(IBattleChara player)
	{
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member is Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter && member.GameObjectId != player.GameObjectId)
			{
				return true;
			}
		}

		return false;
	}
}
