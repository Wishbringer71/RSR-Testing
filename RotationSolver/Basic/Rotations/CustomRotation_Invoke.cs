using ECommons.Logging;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <inheritdoc/>
	public bool TryInvoke(out IAction? newAction, out IAction? gcdAction)
	{
		newAction = gcdAction = null;
		if (!IsEnabled)
		{
			return false;
		}

		try
		{
			UpdateInfo();
			UpdatePreview();

			CountingOfLastUsing = CountingOfCombatTimeUsing = 0;
			// The defence paths set this and clear it on the way out; an exception in one of them would
			// leave it standing for every action of the next update.
			IBaseAction.SelfProtectionHitsMe = null;
			newAction = Invoke(out gcdAction);

			try
			{
				uint gcdId = 0;
				if (gcdAction is IBaseAction ib)
				{
					gcdId = ib.AdjustedID;
				}

				if (gcdId != _lastNextGCDId)
				{
					_lastNextGCDId = gcdId;
					_lastNextGCDChange = DateTime.Now;
				}
			}
			catch
			{

			}
			if (InCombat || CountOfTracking == 0)
			{
				AverageCountOfLastUsing =
					((AverageCountOfLastUsing * CountOfTracking) + CountingOfLastUsing)
					/ ++CountOfTracking;
				MaxCountOfLastUsing = Math.Max(MaxCountOfLastUsing, CountingOfLastUsing);

				AverageCountOfCombatTimeUsing =
					((AverageCountOfCombatTimeUsing * (CountOfTracking - 1)) + CountingOfCombatTimeUsing)
					/ CountOfTracking;
				MaxCountOfCombatTimeUsing = Math.Max(MaxCountOfCombatTimeUsing, CountingOfCombatTimeUsing);
			}

			if (!IsValid)
			{
				IsValid = true;
			}
		}
		catch (Exception? ex)
		{
			WhyNotValid = "Failed to invoke the next action, please contact support.";

			while (ex != null)
			{
				if (!string.IsNullOrEmpty(ex.Message))
				{
					WhyNotValid += "\n" + ex.Message;
				}

				if (!string.IsNullOrEmpty(ex.StackTrace))
				{
					WhyNotValid += "\n" + ex.StackTrace;
				}

				ex = ex.InnerException;
			}

			LogErrorOnce(WhyNotValid);

			IsValid = false;
		}
		finally
		{
			IBaseAction.ResetOverrides();
		}

		return newAction != null;
	}

	private string? _lastLoggedError;

	private void LogErrorOnce(string message)
	{
		if (message == _lastLoggedError)
		{
			return;
		}

		_lastLoggedError = message;
		PluginLog.Error(message);
	}

	private void UpdatePreview()
	{
		if (!DataCenter.DrawingActions)
		{
			return;
		}

		IBaseAction.ActionPreview = true;
		try
		{
			UpdateActions(Role);
		}
		catch (Exception ex)
		{
			LogErrorOnce($"Failed to preview the actions: {ex}");
		}
		finally
		{
			IBaseAction.ActionPreview = false;
			IBaseAction.ResetOverrides();
		}
	}

	private void UpdateActions(JobRole role)
	{
		ActionMoveForwardGCD = MoveForwardGCD(out var act) ? act : null;

		UpdateHealingActions(role, out _);
		UpdateDefenseActions(out _);
		UpdateDispelAndRaiseActions(role, out _);
		UpdatePositionalActions(role, out _);
		UpdateMovementActions(out _);
	}

	private void UpdateHealingActions(JobRole role, out IAction? act)
	{
		act = null;

		try
		{
			if (!DataCenter.HPNotFull && role == JobRole.Healer)
			{
				ActionHealAreaGCD = ActionHealAreaAbility = ActionHealSingleGCD = ActionHealSingleAbility = null;
			}
			else
			{
				ActionHealAreaGCD = HealAreaGCD(out act) ? act : null;
				ActionHealSingleGCD = HealSingleGCD(out act) ? act : null;

				ActionHealAreaAbility = HealAreaAbility(AddlePvE, out act) ? act : null;
				ActionHealSingleAbility = HealSingleAbility(AddlePvE, out act) ? act : null;
			}
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Exception in UpdateHealingActions method: {ex.Message}");
			ActionHealAreaGCD = ActionHealAreaAbility = ActionHealSingleGCD = ActionHealSingleAbility = null;
		}
	}

	private void UpdateDefenseActions(out IAction? act)
	{
		IBaseAction.TargetOverride = TargetType.BeAttacked;
		ActionDefenseAreaGCD = DefenseAreaGCD(out act) ? act : null;
		ActionDefenseSingleGCD = DefenseSingleGCD(out act) ? act : null;
		IBaseAction.TargetOverride = null;

		try
		{
			ActionDefenseAreaAbility = DefenseAreaAbility(AddlePvE, out act) ? act : null;
			ActionDefenseSingleAbility = DefenseSingleAbility(AddlePvE, out act) ? act : null;
		}
		catch (MissingMethodException ex)
		{
			_ = BasicWarningHelper.AddSystemWarning($"Exception in UpdateDefenseActions method: {ex.Message}");
			ActionDefenseAreaAbility = ActionDefenseSingleAbility = null;
		}
	}

	private void UpdateDispelAndRaiseActions(JobRole role, out IAction? act)
	{
		act = null;

		IBaseAction.TargetOverride = TargetType.Dispel;
		ActionDispelStancePositionalGCD = role switch
		{
			JobRole.Healer => DataCenter.DispelTarget != null && DispelGCD(out act) ? act : null,
			_ => null,
		};
		IBaseAction.TargetOverride = null;

		IBaseAction.TargetOverride = TargetType.Death;
		ActionRaiseShirkGCD = role switch
		{
			JobRole.Healer => DataCenter.DeathTarget != null && RaiseSpell(out act, true) ? act : null,
			_ => null,
		};
		IBaseAction.TargetOverride = null;
	}

	private void UpdatePositionalActions(JobRole role, out IAction? act)
	{
		act = null;

		ActionDispelStancePositionalAbility = role switch
		{
			JobRole.Melee => TrueNorthPvE.CanUse(out act) ? act : null,
			JobRole.Tank => TankStance?.CanUse(out act) ?? false ? act : null,
			_ => null,
		};

		ActionRaiseShirkAbility = role switch
		{
			JobRole.Tank => ShirkPvE.CanUse(out act) ? act : null,
			_ => null,
		};

		ActionAntiKnockbackAbility = AntiKnockback(role, AddlePvE, out act) ? act : null;
	}

	private void UpdateMovementActions(out IAction? act)
	{
		IBaseAction.TargetOverride = TargetType.Move;
		var movingTarget = MoveForwardAbility(AddlePvE, out act);
		IBaseAction.TargetOverride = null;
		ActionMoveForwardAbility = movingTarget ? act : null;

		ActionMoveBackAbility = MoveBackAbility(AddlePvE, out act) ? act : null;
		ActionSpeedAbility = SpeedAbility(AddlePvE, out act) ? act : null;
	}

	private IAction? Invoke(out IAction? gcdAction)
	{
		gcdAction = null;

		IBaseAction.ShouldEndSpecial = false;
		IBaseAction.IgnoreClipping = true;
		// The defence paths clear these on every exit they take; an exception thrown inside a rotation's
		// defence would leave them standing until the path ran again.
		IBaseAction.SelfProtectionHitsMe = null;
		IBaseAction.AreaDefenceRunning = false;
		IBaseAction.HealPathRunning = false;
		IBaseAction.CommandedDefenceRunning = false;

		try
		{
			var countDown = Service.CountDownTime;
			if (countDown > 0 && !DataCenter.InCombat)
			{
				return CountDownAction(countDown);
			}

			IBaseAction.TargetOverride = null;

			// For every path of this cycle, not only the single defence: a rotation spends the same
			// mitigation from its general abilities too (concept 09).
			UpdateTankbusterPlan();
			IBaseAction.HoldDefenceOnSelf = HoldDefenceForTankbuster();

			// No healer alive and no tankbuster opening the defence: a mitigation on the player waits only while it would
			// not be back before the next tankbuster on him is due (A361).
			IBaseAction.KeepDefenceForTankbusterIn = DataCenter.UnhealedUnderAttack && !DataCenter.TankbusterOpensDefence
				? TankbusterForecast.NextOnPlayerIn
				: null;

			gcdAction = GCD();
			IBaseAction.IgnoreClipping = false;

			IBaseAction.TargetOverride = null;
			IBaseAction.ShouldEndSpecial = false;

			IBaseAction.AutoHealCheck = false;

			if (gcdAction != null)
			{
				return ActionHelper.CanUseGCD ? gcdAction : Ability(gcdAction, out var ability) ? ability : gcdAction;
			}
			else
			{
				IBaseAction.IgnoreClipping = true;
				if (Ability(AddlePvE, out var ability))
				{
					return ability;
				}
				IBaseAction.IgnoreClipping = false;

				return null;
			}
		}
		catch (Exception ex)
		{
			LogErrorOnce($"Exception in Invoke method: {ex.Message}");
			return null;
		}
		finally
		{
			IBaseAction.ResetOverrides();
			IBaseAction.HoldDefenceOnSelf = false;
			IBaseAction.AllowedDefenceOnSelf = null;
			IBaseAction.KeepDefenceForTankbusterIn = null;
		}
	}

	/// <summary>
	/// The action in countdown.
	/// </summary>
	/// <param name="remainTime"></param>
	/// <returns></returns>
	protected virtual IAction? CountDownAction(float remainTime)
	{
		return null;
	}
}
