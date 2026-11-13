using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;
using RotationSolver.Helpers;
using RotationSolver.Updaters;

namespace RotationSolver.Commands
{
	public static partial class RSCommands
	{
		private static DateTime _lastClickTime = DateTime.MinValue;
		private static TimeSpan _clickDelay = TimeSpan.Zero;
		private static bool _lastState;
		private static bool started = false;
		internal static DateTime _lastUsedTime = DateTime.MinValue;
		internal static uint _lastActionID;
		private static float _lastCountdownTime = 0;
		private static Job _previousJob = Job.ADV;
		private static readonly Random random = Random.Shared;
		private static readonly List<MiscUpdater.ActionBarSlotRef> _pulseSlots = [];

		internal static IBaseAction? CurrentAction { get; set; } = null;

		internal static bool CanDoAnAction(bool isGCD)
		{
			var currentState = DataCenter.State;

			if (!_lastState || !currentState)
			{
				_lastState = currentState;
				return false;
			}
			_lastState = currentState;

			if (DateTime.Now - _lastClickTime < _clickDelay)
			{
				return false;
			}

			_lastClickTime = DateTime.Now;
			_clickDelay = RandomSeconds(Service.Config.ClickingDelay.X, Service.Config.ClickingDelay.Y);

			if (!isGCD && DataCenter.DefaultGCDRemain <= ActionHelper.AbilityClipWindow && DataCenter.DefaultGCDRemain > 0f)
			{
				return false;
			}

			return isGCD || ActionUpdater.NextAction is not IBaseAction nextAction || !nextAction.Info.IsRealGCD;
		}

		public static void DoAction()
		{
			if (Player.Object != null && Player.Object.StatusList == null)
			{
				return;
			}

			var nextAction = ActionUpdater.NextAction;
			if (nextAction == null)
			{
				return;
			}

			if (DataCenter.AnimationLock > 0f)
			{
				return;
			}

			if (nextAction is BaseAction baseAct)
			{
				// Against this action's own lock where the server has given it (A753).
				if (baseAct.Info.IsAbility && !baseAct.Setting.IsMudra && DataCenter.DefaultGCDRemain <= ActionHelper.AbilityWindowFor(baseAct.AdjustedID) && DataCenter.DefaultGCDRemain > 0f)
				{
					return;
				}
			}

			var player = Player.Object;
			if (player != null && !DataCenter.IsPvP
				&& NoCastingStatusHelper.PlayerHasNoCastingStatus(out var minStatusTime, out var damagesEveryAction))
			{
				// Pyretic, Causality: every action costs health for as long as the status lasts, so none goes out (A338).
				if (damagesEveryAction)
				{
					return;
				}

				var remainingCastTime = player.TotalCastTime - player.CurrentCastTime;
				if (minStatusTime > remainingCastTime && minStatusTime < NoCastingStatusHelper.ResolveWindow)
				{
					return;
				}
			}

			// Nor when the action would still be under way as such a status lands: a GCD is chosen ahead of its press, and the
			// landing has come closer since (A439).
			if (player != null && !DataCenter.IsPvP
				&& ActionPenaltyWatch.WouldMeetLanding(nextAction is IBaseAction pressed ? pressed.Info.CastTime : 0f))
			{
				ActionPenaltyWatch.NoteHold($"no press of {nextAction.Name}");
				return;
			}

			if (DataCenter.BMRSpecialModeType == SpecialMode.Pyretic)
			{
				PluginLog.Verbose("Player has Pyretic special mode active, skipping action use to avoid potential issues.");
				return;
			}

			if (StatusHelper.PlayerHasStatus(false, StatusID.MotionTracker))
			{
				PluginLog.Verbose("Player has Motion Tracker status, skipping action use to avoid potential issues.");
				return;
			}

			// Any action begun ends the raise protection; it waits unless a heal or a defence is worth more now (A539).
			if (StatusHelper.PlayerHasStatus(false, StatusID.Transcendent) && !MaySpendRaiseProtection(nextAction))
			{
				return;
			}

			if (StatusHelper.PlayerHasStatus(false, StatusID.WaningNocturne))
			{
				return;
			}

#if DEBUG
			// if (nextAction is BaseAction debugAct)
			//     PluginLog.Debug($"Will Do {debugAct}");
#endif

			SwitchTargetForAction(nextAction);

			CurrentAction = nextAction as IBaseAction;

			var pulse = CapturePulseSlots(nextAction);

			if (nextAction.Use())
			{

				_lastActionID = nextAction.AdjustedID;
				_lastUsedTime = DateTime.Now;

				try
				{
					if (DataCenter.CurrentInterceptedAction != null && DataCenter.CurrentInterceptedAction.AdjustedID == nextAction.AdjustedID)
					{
						DataCenter.CurrentInterceptedAction = null;
						DoSpecialCommandType(SpecialCommandType.EndSpecial, false);
					}
				}
				catch (Exception ex)
				{
					PluginLog.Warning($"Failed to clear CurrentInterceptedAction after execution: {ex}");
				}

				if (nextAction is BaseAction finalAct)
				{
					if (Service.Config.EnableClickingCount)
					{
						OtherConfiguration.RotationSolverRecord.ClickingCount++;
					}

					if (pulse)
					{
						PulseSimulation();
					}

					if (finalAct.Setting.EndSpecial)
					{
						ResetSpecial();
					}
				}
			}
			else if (Service.Config.InDebug)
			{
				PluginLog.Verbose($"Failed to use the action {nextAction} ({nextAction.AdjustedID})");
			}
		}

		private static TimeSpan RandomSeconds(float min, float max)
		{
			return TimeSpan.FromSeconds(Math.Min(min, max) + (random.NextDouble() * Math.Abs(max - min)));
		}

		private static bool CapturePulseSlots(IAction action)
		{
			if (!Service.Config.KeyboardNoise || started || action is not BaseAction)
			{
				return false;
			}

			try
			{
				MiscUpdater.FindActionBarSlots(action.AdjustedID, _pulseSlots);
				return _pulseSlots.Count > 0;
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Pulse Failed!: {ex.Message}");
				return false;
			}
		}

		private static void PulseSimulation()
		{
			started = true;
			PulseAction(random.Next(2, 4));
		}

		private static void PulseAction(int remainingPulses)
		{
			if (remainingPulses <= 0)
			{
				started = false;
				return;
			}

			try
			{
				MiscUpdater.PulseActionBarSlots(_pulseSlots);
				_ = Svc.Framework.RunOnTick(() => PulseAction(remainingPulses - 1),
					RandomSeconds(Service.Config.ClickingDelay.X, Service.Config.ClickingDelay.Y));
			}
			catch (Exception ex)
			{
				started = false;
				PluginLog.Warning($"Pulse Failed!: {ex.Message}");
				BasicWarningHelper.AddSystemWarning($"Action bar failed to pulse because: {ex.Message}");
			}
		}

		internal static void ResetSpecial()
		{
			DoSpecialCommandType(SpecialCommandType.EndSpecial, false);
		}

		internal static void CancelState()
		{
			DataCenter.ResetAllRecords();
			DataCenter.IpcOverrides = null;
			if (DataCenter.State)
			{
				DoStateCommandType(StateCommandType.Off);
			}
		}

		internal static void SetTargetWithDelay(IBattleChara? candidate)
		{
			if (candidate == null)
			{
				return;
			}

			var min = Service.Config.TargetDelay.X;
			var max = Service.Config.TargetDelay.Y;
			var delay = Math.Max(0, min + (random.NextDouble() * Math.Max(0, max - min)));
			if (delay <= 0)
			{
				Svc.Targets.Target = candidate;
				return;
			}

			var initialTargetId = Svc.Targets.Target?.GameObjectId ?? 0;
			var candidateId = candidate.GameObjectId;

			_ = Svc.Framework.RunOnTick(() =>
			{
				try
				{
					var current = Svc.Targets.Target;
					var currentId = current?.GameObjectId ?? 0;

					if (currentId == initialTargetId)
					{
						IBattleChara? cand = null;
						foreach (var obj in Svc.Objects)
						{
							if (obj != null && obj.GameObjectId == candidateId)
							{
								cand = obj as IBattleChara;
								break;
							}
						}

						if (cand != null && cand.IsTargetable)
						{
							Svc.Targets.Target = cand;
						}
					}
				}
				catch
				{
					// Intentionally swallow; candidate may have despawned
				}
			}, TimeSpan.FromSeconds(delay));
		}

		public static void UpdateTargetFromNextAction()
		{
			if (Player.Object == null)
			{
				return;
			}

			SwitchTargetForAction(ActionUpdater.NextAction);
		}

		private static void SwitchTargetForAction(IAction? action)
		{
			if (action is not BaseAction baseAct || baseAct.Target.Target is not IBattleChara target)
			{
				return;
			}

			var isEnemy = target.IsEnemy();
			if (target.GameObjectId == Player.Object?.GameObjectId || !(Service.Config.SwitchTargetFriendly2 || isEnemy))
			{
				return;
			}

			if (isEnemy)
			{
				DataCenter.HostileTarget = target;
			}

			if (!DataCenter.IsManual &&
				(Service.Config.SwitchTargetFriendly2
				|| (Svc.Targets.Target?.IsEnemy() ?? true)
				|| Svc.Targets.Target?.GetObjectKind() == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Treasure))
			{
				Svc.Targets.Target = target;
			}
		}

		// The raise protection (Transcendent) ends with the first action begun - a cast at its start, an instant, an item
		// (his logs, 25.10. to 03.11.2026). It is spent on a heal, a defence or an HP potion that a heal or defence path
		// chose, when an announced hit on the player lands only after it ends or a member is about to die, and no
		// announced hit, marker or prediction can land while it lasts (concept 11, "Nach der eigenen Wiederbelebung",
		// A539). One trace line per window and verdict.
		private static bool MaySpendRaiseProtection(IAction nextAction)
		{
			if (!Service.Config.SpendRaiseProtection || !DataCenter.InCombat
				|| (nextAction is not HpPotionItem
					&& (nextAction is not IBaseAction action || !IBaseAction.ProtectiveChoices.Contains(action.ID))))
			{
				return false;
			}

			var remaining = RaiseProtection.Remaining;
			if (remaining > _raiseTraceRemaining)
			{
				_raiseKeptTraced = _raiseSpentTraced = false;
			}
			_raiseTraceRemaining = remaining;

			if (RaiseProtection.FirstWithin(remaining) is { } within)
			{
				if (!_raiseKeptTraced)
				{
					_raiseKeptTraced = true;
					DefenseTrace.Line($"raise protection kept for {nextAction.Name}: {within.Name} can land within it"
						+ $" ({within.LandsIn:F1} s, {remaining:F1} s left)");
				}
				return false;
			}

			string? reason = null;
			if (RaiseProtection.FirstAfter(remaining, Service.Config.BMRRaidwideMitWindow, Service.Config.BMRTankbusterMitWindow)
				is { } after)
			{
				reason = $"{after.Name} lands {after.LandsIn - remaining:F1} s after it";
			}
			else if (CriticalMemberName() is { } critical)
			{
				reason = $"{critical} is about to die";
			}

			if (reason == null)
			{
				return false;
			}

			if (!_raiseSpentTraced)
			{
				_raiseSpentTraced = true;
				DefenseTrace.Line($"raise protection given up for {nextAction.Name}: {reason}, {remaining:F1} s left");
			}
			return true;
		}

		private static float _raiseTraceRemaining;
		private static bool _raiseKeptTraced;
		private static bool _raiseSpentTraced;

		// The player or a party member in the heal chain's critical class (concept 07).
		private static string? CriticalMemberName()
		{
			if (Player.Object is { } player && player.IsInCriticalClass())
			{
				return "you";
			}

			var party = DataCenter.PartyMembers;
			for (var i = 0; i < party.Count; i++)
			{
				var member = party[i];
				if (member != null && !member.IsDead && member.IsInCriticalClass())
				{
					return member.Name.TextValue;
				}
			}

			return null;
		}

		// What "Turn off when you die" switched off in a fight, so that a raise in the same fight switches it back on
		// ("Turn back on when you are raised in combat", concept 11, A539). Null when nothing is pending.
		private static StateCommandType? _offByDeath;
		private static uint _offByDeathTerritory;

		internal static void UpdateRotationState()
		{
			try
			{
				if (Player.Object == null)
				{
					return;
				}

				if (ActionUpdater.AutoCancelTime != DateTime.MinValue &&
					(!DataCenter.State || DataCenter.InCombat))
				{
					ActionUpdater.AutoCancelTime = DateTime.MinValue;
				}

				var currentJob = Player.Job;
				var jobChanged = currentJob != _previousJob;
				_previousJob = currentJob;

				var diesInPvE = Service.Config.AutoOffWhenDead && DataCenter.Territory != null && !DataCenter.Territory.IsPvP
					&& Player.Object.CurrentHp == 0;

				// Turned off by death in a fight, by RSR's own state - not by AutoDuty, the henched mode or another plugin's
				// IPC state, which keep theirs.
				if (diesInPvE && DataCenter.State && DataCenter.InCombat && !DataCenter.IsAutoDuty && !DataCenter.IsHenched
					&& DataCenter.IpcOverrides == null)
				{
					_offByDeath = DataCenter.IsTargetOnly ? StateCommandType.TargetOnly
						: DataCenter.IsManual ? StateCommandType.Manual
						: StateCommandType.Auto;
					_offByDeathTerritory = DataCenter.TerritoryID;
					DefenseTrace.Line($"RSR off: you died in combat (Turn off when you die), {_offByDeath} again if you are raised in this fight"
						+ (Service.Config.AutoOnWhenRaised ? string.Empty : " - Turn back on when you are raised in combat is off"));
				}

				if (Svc.Condition[ConditionFlag.LoggingOut] ||
					diesInPvE ||
					(Service.Config.AutoOffWhenDeadPvP && DataCenter.Territory != null && DataCenter.Territory.IsPvP && Player.Object.CurrentHp == 0) ||
					(Service.Config.AutoOffPvPMatchEnd && Svc.Condition[ConditionFlag.PvPDisplayActive]) ||
					(Service.Config.AutoOffCutScene && !DataCenter.IsAutoDuty && Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]) ||
					(Service.Config.AutoOffSwitchClass && jobChanged) ||
					(Service.Config.AutoOffBetweenArea && !DataCenter.IsAutoDuty && (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])) ||
					(Service.Config.CancelStateOnCombatBeforeCountdown && Service.CountDownTime > 0.2f && DataCenter.InCombat) ||
					(ActionUpdater.AutoCancelTime != DateTime.MinValue && DateTime.Now > ActionUpdater.AutoCancelTime))
				{
					if (DataCenter.State)
					{
						CancelState();
					}

					ActionUpdater.AutoCancelTime = DateTime.MinValue;
					return;
				}

				// Back on after a raise in the same fight: the reason for turning off - dead, nothing to do - is gone. Not
				// once he switched RSR himself, changed zones or the fight ended (wipe, return to the start).
				if (_offByDeath is { } wasOn)
				{
					if (DataCenter.State || !DataCenter.InCombat || DataCenter.TerritoryID != _offByDeathTerritory)
					{
						_offByDeath = null;
					}
					else if (Player.Object.CurrentHp > 0)
					{
						_offByDeath = null;
						if (Service.Config.AutoOnWhenRaised)
						{
							DefenseTrace.Line($"RSR on again ({wasOn}): raised in combat");
							DoStateCommandType(wasOn);
							return;
						}
					}
				}

				if (Service.Config.AutoOnPvPMatchStart &&
					Svc.Condition[ConditionFlag.BetweenAreas] &&
					Svc.Condition[ConditionFlag.BoundByDuty] &&
					!DataCenter.State &&
					(DataCenter.Territory?.IsPvP ?? false))
				{
					DoStateCommandType(StateCommandType.Auto);
					return;
				}

				if (Service.Config.AutoOnYes && !DataCenter.State)
				{
					HashSet<ulong>? targetedIds = null;

					if (Service.Config.StartOnPartyIsInCombat2 && DataCenter.PartyMembers.Count > 1)
					{
						targetedIds ??= CollectHostileTargetObjectIds();
						if (AnyInCombatOrTargeted(DataCenter.PartyMembers, targetedIds))
						{
							DoStateCommandType(StateCommandType.Auto);
							return;
						}
					}

					var inFieldOp = DataCenter.IsInBozjanFieldOp || DataCenter.IsInBozjanFieldOpCE || DataCenter.IsInOccultCrescentOp;

					if (Service.Config.StartOnAllianceIsInCombat2 && DataCenter.AllianceMembers.Count > 1 && !inFieldOp)
					{
						targetedIds ??= CollectHostileTargetObjectIds();
						if (AnyInCombatOrTargeted(DataCenter.AllianceMembers, targetedIds))
						{
							DoStateCommandType(StateCommandType.Auto);
							return;
						}
					}

					if (Service.Config.StartOnFieldOpInCombat2 && inFieldOp)
					{
						targetedIds ??= CollectHostileTargetObjectIds();

						var targets = TargetHelper.GetTargetsByRange(30f);
						for (var i = 0; i < targets.Count; i++)
						{
							var t = targets[i];
							// GetTargetsByRange returns every object, and this branch reacts to someone
							// else being in combat, so hostiles are skipped on purpose. A striking
							// dummy is a hostile as well, so exempting it from that check left it as
							// the one hostile that still triggers the automatic start - and every
							// field op zone has one standing at its camp.
							//
							// Three reasons to skip, and the merge needs all three: upstream added the
							// null guard and the id set (DataCenter.AllHostileTargetIds, a lookup instead
							// of a walk over the hostile list), this fork added the dummy. Upstream's form
							// skipped a hostile only while it was NOT a dummy, which is the exemption that
							// put the dummy back in as a trigger.
							if (t == null || DataCenter.AllHostileTargetIds.Contains(t.GameObjectId) || ObjectHelper.IsDummy(t))
							{
								continue;
							}

							if (t.InCombat() || targetedIds.Contains(t.GameObjectId))
							{
								DoStateCommandType(StateCommandType.Auto);
								return;
							}
						}
					}

					if (Service.Config.StartOnAttackedBySomeone2)
					{
						IBattleChara? target = null;
						var playerId = Player.Object.GameObjectId;
						for (var i = 0; i < DataCenter.AllHostileTargets.Count; i++)
						{
							var battleChara = DataCenter.AllHostileTargets[i];

							if (battleChara == null || !battleChara.IsValid() || battleChara.Address == nint.Zero)
							{
								continue;
							}

							if (battleChara.TargetObjectId == playerId)
							{
								target = battleChara;
								break;
							}
						}

						if (target != null && !ObjectHelper.IsDummy(target))
						{
							DoStateCommandType(StateCommandType.Manual);
						}
					}
				}

				if (Service.Config.StartOnCountdown && !DataCenter.IsInDutyReplay())
				{
					if (Service.CountDownTime > 0)
					{
						_lastCountdownTime = Service.CountDownTime;
						if (!DataCenter.State)
						{
							DoStateCommandType(Service.Config.CountdownStartsManualMode
								? StateCommandType.Manual
								: StateCommandType.Auto);
						}
						return;
					}
					else if (Service.CountDownTime == 0 && _lastCountdownTime > 0.2f)
					{
						_lastCountdownTime = 0;
						CancelState();
						return;
					}
				}
			}
			catch (Exception ex)
			{
				PluginLog.Error($"Exception in UpdateRotationState: {ex.Message}");
			}
		}

		private static readonly HashSet<ulong> _targetedIds = [];

		private static HashSet<ulong> CollectHostileTargetObjectIds()
		{
			_targetedIds.Clear();
			foreach (var hostile in DataCenter.AllHostileTargets)
			{

				if (hostile == null || !hostile.IsValid() || hostile.Address == nint.Zero)
				{
					continue;
				}

				if (hostile.TargetObjectId != 0)
				{
					_ = _targetedIds.Add(hostile.TargetObjectId);
				}
			}
			return _targetedIds;
		}

		private static bool AnyInCombatOrTargeted(List<IBattleChara> members, HashSet<ulong> targetedIds)
		{
			foreach (var member in members)
			{
				if (member != null && (member.InCombat() || targetedIds.Contains(member.GameObjectId)))
				{
					return true;
				}
			}
			return false;
		}
	}
}