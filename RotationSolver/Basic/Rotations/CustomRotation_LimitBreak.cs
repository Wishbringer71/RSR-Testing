using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	// Concept 03, "Limit Break allein" (A628, A630): the animation lock of each Limit Break and the duration of a tank's, measured on
	// its own use this session.
	private static readonly Dictionary<uint, float> _limitBreakLock = [];
	private static readonly Dictionary<uint, float> _limitBreakDuration = [];
	private static (uint Action, ushort Units, DateTime At) _limitBreakUsed = (0, 0, DateTime.MinValue);
	private static (uint Action, string Name, float Window, DateTime At) _limitBreakStatusPending = (0, string.Empty, 0f, DateTime.MinValue);
	private static (IBaseAction? Action, int Level, float Window, string Reason) _limitBreakChosen;

	/// <summary>
	/// The job's Limit Break of the full bar with no other player in the party, when the player's role calls for it: a damage dealer
	/// while no announced hit on him lands within its cast and lock; a tank against an announced hit on the party or on him; a
	/// healer with more than one member about to die, or at level 3 with a member to raise. For every role BossMod must not need to
	/// move him within the cast and lock, and the enemies must outlive it.
	/// </summary>
	private unsafe bool LimitBreakWhenAlone(out IAction? act)
	{
		act = null;
		_limitBreakChosen = (null, 0, 0f, string.Empty);
		var controller = &UIState.Instance()->LimitBreakController;
		MeasureLimitBreakDuration();
		MeasureLimitBreakLock(controller);
		var player = Player;
		var role = DataCenter.Role;
		if (!Service.Config.UseLimitBreakWhenAlone || !DataCenter.InCombat || DataCenter.IsPvP || player == null || player.IsDead
			|| role is not (JobRole.Tank or JobRole.Healer or JobRole.Melee or JobRole.RangedPhysical or JobRole.RangedMagical)
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
		// BossMod's MaxCastTime is the time the player may stand before he has to move; the lock holds him as the cast does.
		if (DataCenter.BMRIsMoving || DataCenter.BMRMaxCastTime < window || !IsLongerThan(window))
		{
			return false;
		}

		var reason = role switch
		{
			JobRole.Tank => TankLimitBreakReason(player, limitBreak, window),
			JobRole.Healer => HealerLimitBreakReason(limitBreak, window),
			_ => player.IsInCriticalClass() || RaiseProtection.FirstWithin(window) != null
				? null
				: $"full bar, no announced hit within {window:F1} s",
		};
		if (reason == null)
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

		_limitBreakChosen = (limitBreak, controller->BarCount, window, reason);
		return true;
	}

	/// <summary>
	/// The Limit Break <see cref="LimitBreakWhenAlone"/> chose is the action reported: one trace line per use, and the lock and a
	/// tank's duration are measured from here.
	/// </summary>
	private static unsafe void NoteLimitBreakReported()
	{
		var (limitBreak, level, window, reason) = _limitBreakChosen;
		if (limitBreak == null)
		{
			return;
		}

		var controller = &UIState.Instance()->LimitBreakController;
		if (_limitBreakUsed.Action != limitBreak.ID || (DateTime.Now - _limitBreakUsed.At).TotalSeconds > window)
		{
			DefenseTrace.Line($"limit break: {limitBreak.Name} (level {level}, {DataCenter.Role}) - {reason}; no other player, "
				+ $"window {window:F1} s");
		}

		_limitBreakUsed = (limitBreak.ID, controller->CurrentUnits, DateTime.Now);
		if (DataCenter.Role == JobRole.Tank && !_limitBreakDuration.ContainsKey(limitBreak.ID))
		{
			_limitBreakStatusPending = (limitBreak.ID, limitBreak.Action.Name.ExtractText(), window, DateTime.Now);
		}
	}

	// A tank's Limit Break lowers the damage of everyone within its range, the tank too, and has no cast: it answers the defence's
	// own calls - an announced hit on the party, or one on him that his invulnerability does not cover - while its status still
	// stands when the hit lands, or while the damage already streams (the pull rule, no healer alive).
	private string? TankLimitBreakReason(IBattleChara player, IBaseAction limitBreak, float window)
	{
		// In danger his invulnerability and emergency defence come first; the lock would hold them back.
		if (player.IsInCriticalClass() || RaiseProtection.FirstDodgeableWithin(window) is not null)
		{
			return null;
		}

		var duration = TankLimitBreakDuration(limitBreak);
		var hit = RaiseProtection.FirstWithin(duration);
		var merged = DataCenter.MergedStatus;
		if (merged.HasFlag(AutoStatus.DefenseArea) && hit is { } partyHit)
		{
			return $"announced hit on the party: {partyHit.Name} in {partyHit.LandsIn:F1} s, status {duration:F0} s";
		}

		if (!merged.HasFlag(AutoStatus.DefenseSingle) || player.ImperviousThrough(duration))
		{
			return null;
		}

		if (DataCenter.TankPullOnPlayer || DataCenter.UnhealedUnderAttack)
		{
			return DataCenter.TankPullOnPlayer ? "the pull rule: enemies attack you below its HP line" : "no healer alive under attack";
		}

		return hit is { } ownHit ? $"announced hit on you: {ownHit.Name} in {ownHit.LandsIn:F1} s, status {duration:F0} s" : null;
	}

	// How long the status of a tank's Limit Break stands: measured on its first own use, else as its effect text states it, else the
	// shortest any level of the job states (the job texts of level 3 are empty).
	private float TankLimitBreakDuration(IBaseAction limitBreak)
	{
		if (_limitBreakDuration.TryGetValue(limitBreak.ID, out var measured))
		{
			return measured;
		}

		if (ActionTooltip.TryStatedDuration(limitBreak.ID, out var stated) && stated > 0f)
		{
			return stated;
		}

		var shortest = 0f;
		foreach (var level in new[] { LimitBreak1, LimitBreak2, LimitBreak3 })
		{
			if (level != null && ActionTooltip.TryStatedDuration(level.ID, out var levelStated) && levelStated > 0f
				&& (shortest == 0f || levelStated < shortest))
			{
				shortest = levelStated;
			}
		}

		return shortest;
	}

	// A healer's Limit Break heals everyone within its range after its cast, and at level 3 raises the dead: more than one member
	// about to die - a single heal saves one -, or a member to raise.
	private string? HealerLimitBreakReason(IBaseAction limitBreak, float window)
	{
		if (RaiseProtection.FirstDodgeableWithin(window) is not null)
		{
			return null;
		}

		var range = (float)limitBreak.Action.EffectRange;
		var critical = 0;
		var dead = 0;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member == null || member.DistanceToPlayer() > range)
			{
				continue;
			}

			if (member.IsDead)
			{
				if (!member.HasStatus(false, StatusHelper.RaiseStatus))
				{
					dead++;
				}

				continue;
			}

			if (member.MaxHp > 0 && member.IsInCriticalClass())
			{
				critical++;
			}
		}

		if (critical > 1)
		{
			return $"{critical} members about to die";
		}

		return dead > 0 && limitBreak == LimitBreak3 ? $"{dead} dead to raise" : null;
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

	// A tank's Limit Break puts a status of its own name on him; the first time it is seen from him after the use, its time left is
	// the duration. Not seen within the window: the use did not go out.
	private static void MeasureLimitBreakDuration()
	{
		var (action, name, window, at) = _limitBreakStatusPending;
		var player = Player;
		if (action == 0 || player == null)
		{
			return;
		}

		foreach (var status in player.StatusList)
		{
			if (status == null || status.SourceId != player.GameObjectId || status.RemainingTime <= 0f
				|| status.GameData.ValueNullable?.Name.ExtractText() != name)
			{
				continue;
			}

			_limitBreakDuration[action] = status.RemainingTime;
			DefenseTrace.Line($"limit break: status of #{action} measured at {status.RemainingTime:F1} s");
			_limitBreakStatusPending = (0, string.Empty, 0f, DateTime.MinValue);
			return;
		}

		if ((DateTime.Now - at).TotalSeconds > window)
		{
			_limitBreakStatusPending = (0, string.Empty, 0f, DateTime.MinValue);
		}
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
