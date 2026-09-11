using ECommons.ExcelServices;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// Whether the player is currently doing nothing (and healing).
	/// </summary>
	public static bool HealingWhileDoingNothing =>
		_nextTimeToHeal + TimeSpan.FromSeconds(DataCenter.DefaultGCDTotal) > DateTime.Now;

	private static DateTime _nextTimeToHeal = DateTime.MinValue;
	private static readonly Random _random = new();

	private IAction? GCD()
	{
		var act = DataCenter.CommandNextAction;

		IBaseAction.ForceEnable = true;
		if (act is IBaseAction a && a.Info.IsRealGCD
			&& a.CanUse(out _, usedUp: true, skipAoeCheck: true, skipStatusProvideCheck: true))
		{
			return act;
		}

		IBaseAction.ForceEnable = false;

		if (DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting))
		{
			return null;
		}

		if (DataCenter.Orbonne && IsLastAction(ActionID.HeavenlyShieldPvE) && DataCenter.IsAgriasCastingSpecialIndicator())
		{
			return null;
		}

		if (Service.Config.PldlockCasting && DataCenter.Job == Job.PLD && IsLastAction(ActionID.PassageOfArmsPvE) && StatusHelper.PlayerHasStatus(true, StatusID.PassageOfArms))
		{
			return null;
		}

		if (Service.Config.AstlockCasting && DataCenter.Job == Job.AST && IsLastAction(ActionID.CollectiveUnconsciousPvE) && StatusHelper.PlayerHasStatus(true, StatusID.CollectiveUnconscious_848))
		{
			return null;
		}

		if (Service.Config.BlulockCasting && DataCenter.Job == Job.BLU && IsLastAction(ActionID.PhantomFlurryPvE) && StatusHelper.PlayerHasStatus(true, StatusID.PhantomFlurry))
		{
			return null;
		}

		if (DataCenter.Job == Job.NIN && StatusHelper.PlayerHasStatus(true, StatusID.Mudra) && DataCenter.DefaultGCDRemain > 0.6f)
		{
			return null;
		}

		if (DataCenter.IsPvP && Service.Config.PvpGuardControl && HasPVPGuard)
		{
			return null;
		}

		if (Player != null && DataCenter.IsPvP && Service.Config.PvpGcdLockControl && Player.CurrentMp >= 2000 && Player.GetEffectiveHpPercent() < 50f)
		{
			return null;
		}

		try
		{
			IBaseAction.ShouldEndSpecial = false;
			if (DataCenter.CurrentDutyRotation?.EmergencyGCD(act, out act) == true)
			{
				return act;
			}
			if (EmergencyGCD(act, out act))
			{
				return act;
			}

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.Interrupt))
			{
				if (DataCenter.CurrentDutyRotation?.MyInterruptGCD(out act) == true)
				{
					return act;
				}
				if (MyInterruptGCD(out var action))
				{
					return action;
				}
			}

			IBaseAction.TargetOverride = TargetType.Dispel;
			if (DataCenter.MergedStatus.HasFlag(AutoStatus.Dispel))
			{
				if (DataCenter.CurrentDutyRotation?.DispelGCD(out act) == true)
				{
					return act;
				}
				if (DispelGCD(out var action))
				{
					return action;
				}
			}

			IBaseAction.TargetOverride = TargetType.Provoke;
			if (DataCenter.MergedStatus.HasFlag(AutoStatus.Provoke))
			{
				if (DataCenter.CurrentDutyRotation?.ProvokeGCD(out act) == true)
				{
					return act;
				}
				if (ProvokeGCD(out var action))
				{
					return action;
				}
			}

			IBaseAction.TargetOverride = TargetType.Death;

			var hardcastraisetype = Service.Config.HardCastRaiseType;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.Raise) && DataCenter.CanRaise() && Service.Config.RaisePlayerFirst)
			{
				if (RaiseSpell(out act, false))
				{
					return act;
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastNormal && !SwiftcastComingForRaise)
				{
					if (RaiseSpell(out act, true))
					{
						return act;
					}
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastSwiftCooldown)
				{
					if (!Service.Config.RaisePlayerBySwift || (SwiftcastPvE.Cooldown.IsCoolingDown && Raise != null && Raise.Info.CastTime < SwiftcastPvE.Cooldown.RecastTimeRemainOneCharge))
					{
						if (RaiseSpell(out act, true))
						{
							return act;
						}
					}
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastOnlyHealer)
				{
					if (!AnyOtherLivingRaiser() && RaiseSpell(out act, true))
					{
						return act;
					}
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastOnlyHealerSwiftCooldown)
				{
					if (!Service.Config.RaisePlayerBySwift || (SwiftcastPvE.Cooldown.IsCoolingDown && Raise != null && Raise.Info.CastTime < SwiftcastPvE.Cooldown.RecastTimeRemainOneCharge))
					{
						if (!AnyOtherLivingRaiser() && RaiseSpell(out act, true))
						{
							return act;
						}
					}
				}
			}

			IBaseAction.TargetOverride = null;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveForward))
			{
				if (DataCenter.CurrentDutyRotation?.MoveForwardGCD(out act) == true)
				{
					if (act is IBaseAction b && ObjectHelper.DistanceToPlayer(b.Target.Target) > 5)
					{
						return act;
					}
				}
				if (MoveForwardGCD(out var action))
				{
					if (action is IBaseAction b && ObjectHelper.DistanceToPlayer(b.Target.Target) > 5)
					{
						return action;
					}
				}
			}

			IBaseAction.TargetOverride = TargetType.Heal;

			if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealAreaSpell))
			{
				IBaseAction.AutoHealCheck = true;
				if (DataCenter.CurrentDutyRotation?.HealAreaGCD(out act) == true)
				{
					return act;
				}

				if (!StatusHelper.PlayerHasStatus(false, StatusID.Scalebound) && (!StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) || StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) && DataCenter.NumberOfPartyMembersInRangeOf(21) == 1))
				{
					if (HealAreaGCD(out var action))
					{
						return action;
					}
				}
				IBaseAction.AutoHealCheck = false;
			}
			if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealAreaSpell))
			{
				IBaseAction.AutoHealCheck = true;
				// Unconditional like the other three heal branches. The condition that used to sit here
				// enumerated the duty heal sources that existed when it was written, and Bozja's area
				// cures arrived later without being added, which left them unreachable on this path.
				if (DataCenter.CurrentDutyRotation?.HealAreaGCD(out act) == true)
				{
					return act;
				}

				if (CanHealAreaSpell)
				{
					if (!StatusHelper.PlayerHasStatus(false, StatusID.Scalebound) && (!StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) || StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) && DataCenter.NumberOfPartyMembersInRangeOf(21) == 1))
					{
						if (HealAreaGCD(out var action))
						{
							return action;
						}
					}
				}

				IBaseAction.AutoHealCheck = false;
			}

			if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleSpell))
			{
				IBaseAction.AutoHealCheck = true;
				if (DataCenter.CurrentDutyRotation?.HealSingleGCD(out act) == true)
				{
					return act;
				}

				if (!StatusHelper.PlayerHasStatus(false, StatusID.Scalebound) && (!StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) || StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) && DataCenter.NumberOfPartyMembersInRangeOf(21) == 1))
				{
					if (HealSingleGCD(out var action))
					{
						return action;
					}
				}
				IBaseAction.AutoHealCheck = false;
			}
			if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealSingleSpell))
			{
				IBaseAction.AutoHealCheck = true;
				if (DataCenter.CurrentDutyRotation?.HealSingleGCD(out act) == true)
				{
					return act;
				}

				if (CanHealSingleSpell)
				{
					if (!StatusHelper.PlayerHasStatus(false, StatusID.Scalebound) && (!StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) || StatusHelper.PlayerHasStatus(false, StatusID.ShackledHealing) && DataCenter.NumberOfPartyMembersInRangeOf(21) == 1))
					{
						if (HealSingleGCD(out var action))
						{
							return action;
						}
					}
				}

				IBaseAction.AutoHealCheck = false;
			}

			IBaseAction.TargetOverride = null;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseArea))
			{
				if (DataCenter.CurrentDutyRotation?.DefenseAreaGCD(out act) == true)
				{
					return act;
				}

				if (DefenseAreaGCD(out var action))
				{
					return action;
				}
			}

			IBaseAction.TargetOverride = TargetType.BeAttacked;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseSingle))
			{
				if (DataCenter.CurrentDutyRotation?.DefenseSingleGCD(out act) == true)
				{
					return act;
				}

				if (DefenseSingleGCD(out var action))
				{
					return action;
				}
			}

			IBaseAction.TargetOverride = TargetType.Death;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.Raise) && DataCenter.CanRaise() && !Service.Config.RaisePlayerFirst)
			{
				if (RaiseSpell(out act, false))
				{
					return act;
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastNormal && !SwiftcastComingForRaise)
				{
					if (RaiseSpell(out act, true))
					{
						return act;
					}
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastSwiftCooldown)
				{
					if (!Service.Config.RaisePlayerBySwift || (SwiftcastPvE.Cooldown.IsCoolingDown && Raise != null && Raise.Info.CastTime < SwiftcastPvE.Cooldown.RecastTimeRemainOneCharge))
					{
						if (RaiseSpell(out act, true))
						{
							return act;
						}
					}
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastOnlyHealer)
				{
					if (!AnyOtherLivingRaiser() && RaiseSpell(out act, true))
					{
						return act;
					}
				}

				if (hardcastraisetype == HardCastRaiseType.HardCastOnlyHealerSwiftCooldown)
				{
					if (!Service.Config.RaisePlayerBySwift || (SwiftcastPvE.Cooldown.IsCoolingDown && Raise != null && Raise.Info.CastTime < SwiftcastPvE.Cooldown.RecastTimeRemainOneCharge))
					{
						if (!AnyOtherLivingRaiser() && RaiseSpell(out act, true))
						{
							return act;
						}
					}
				}
			}

			IBaseAction.TargetOverride = null;

			IBaseAction.ShouldEndSpecial = false;
			IBaseAction.TargetOverride = null;

			if (!DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting))
			{
				if (DataCenter.CurrentDutyRotation?.GeneralGCD(out act) == true)
				{
					return act;
				}

				if (GeneralGCD(out var action))
				{
					return action;
				}
			}

			if (Service.Config.HealWhenNothingTodo && InCombat)
			{
				// Please don't tell me someone's fps is less than 1!!
				if (DateTime.Now - _nextTimeToHeal > TimeSpan.FromSeconds(1))
				{
					var min = Service.Config.HealWhenNothingTodoDelay.X;
					var max = Service.Config.HealWhenNothingTodoDelay.Y;
					_nextTimeToHeal = DateTime.Now + TimeSpan.FromSeconds((_random.NextDouble() * (max - min)) + min);
				}
				else if (_nextTimeToHeal < DateTime.Now)
				{
					_nextTimeToHeal = DateTime.Now;

					if (PartyMembersMinHP < Service.Config.HealWhenNothingTodoBelow)
					{
						IBaseAction.TargetOverride = TargetType.Heal;

						if (DataCenter.PartyMembersDifferHP < Service.Config.HealthDifference)
						{
							var count = 0;
							foreach (var hp in DataCenter.PartyMembersHP)
							{
								if (hp < 1)
								{
									count++;
								}
							}
							if (count > 2 && DataCenter.CurrentDutyRotation?.HealAreaGCD(out act) == true)
							{
								return act;
							}
							if (count > 2 && HealAreaGCD(out act))
							{
								return act;
							}
						}
						if (DataCenter.CurrentDutyRotation?.HealSingleGCD(out act) == true)
						{
							return act;
						}
						if (HealSingleGCD(out act))
						{
							return act;
						}

						IBaseAction.TargetOverride = null;
					}
				}
			}
		}
		catch (Exception ex)
		{
			// Log the exception or handle it as needed
			Console.WriteLine($"Exception in GCD method: {ex.Message}");
		}
		finally
		{
			// Ensure these are reset
			IBaseAction.ShouldEndSpecial = false;
			IBaseAction.TargetOverride = null;
		}

		return null;
	}

	/// <summary>
	/// Is anyone besides the player still standing who could take this raise?
	///
	/// This is the question the "only healer" hard cast modes mean to ask: hard casting costs eight
	/// seconds of GCD, which is only worth it when nobody else can do it instead. The branches used
	/// to build two sets of party healers and compare their sizes, which answered a different
	/// question and failed in three ways:
	///
	/// - It counted healers, not raisers. A living Summoner or Red Mage raises just as well, and
	///   PheonixDownItem.AnyLivingRaiserInParty already knows that.
	/// - Both sets excluded the player, so a lone healer - every four-man party, and solo - ended up
	///   comparing 0 == 0 with a "&gt; 0" guard behind it, and never hard cast at all. That is the
	///   exact case where hard casting is the only way anyone gets up.
	/// - Being a count comparison, it could not express "nobody else", only "as many dead as there
	///   are".
	///
	/// The reference set follows RaiseType, because that is what decides where the corpse may come
	/// from. Under the alliance modes the other alliances are real parties with their own raisers:
	/// while one of them is alive the raise is theirs to take, and once none is, hard casting is
	/// the only way anyone there gets up. AllOutOfDuty is deliberately excluded - strangers in the
	/// open world are not a raise reserve, and counting them would block hard casting almost always.
	/// </summary>
	private static bool AnyOtherLivingRaiser()
	{
		if (HasLivingRaiser(DataCenter.PartyMembers))
		{
			return true;
		}

		return Service.Config.RaiseType is RaiseType.PartyAndAllianceSupports
				or RaiseType.PartyAndAllianceHealers
				or RaiseType.All
			&& HasLivingRaiser(DataCenter.AllianceMembers);
	}

	/// <summary>
	/// Does this set hold someone other than the player who is alive and able to raise?
	/// </summary>
	private static bool HasLivingRaiser(IEnumerable<IBattleChara>? members)
	{
		if (members == null)
		{
			return false;
		}

		foreach (var member in members)
		{
			if (member == null || member.IsDead || member.IsPlayer())
			{
				continue;
			}

			if (member.IsJobCategory(JobRole.Healer)
				|| member.IsJobs(Job.SMN)
				|| member.IsJobs(Job.RDM))
			{
				return true;
			}
		}

		return false;
	}

			if (member.IsJobCategory(JobRole.Healer)
				|| member.IsJobs(Job.SMN)
				|| member.IsJobs(Job.RDM))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Is the rotation still going to spend Swiftcast on the raise?
	///
	/// The hard cast branches used to ask <c>SwiftcastPvE.Cooldown.IsCoolingDown</c>, which answers
	/// a different question and leaves a state they cannot get out of. For a healer this rotation
	/// spends Swiftcast on the raise path alone - the two other triggers in CustomRotation_Ability
	/// are gated on JobRole.RangedMagical - so with RaisePlayerBySwift off it is never spent, never
	/// enters recovery, the hard cast branch never fires, and nobody is raised at all. The setting
	/// only promises not to spend Swiftcast on raises, not to stop raising.
	///
	/// With the setting on, `!SwiftcastComingForRaise` equals the old `IsCoolingDown` in every
	/// combination; it differs only for setting-off with Swiftcast ready, which is exactly the
	/// state that raised nobody.
	/// </summary>
	private bool SwiftcastComingForRaise =>
		Service.Config.RaisePlayerBySwift && !SwiftcastPvE.Cooldown.IsCoolingDown;

	private bool RaiseSpell(out IAction? act, bool mustUse)
	{
		act = null;

		if (DataCenter.CanRaise())
		{
			if (DataCenter.CommandStatus.HasFlag(AutoStatus.Raise))
			{
				IBaseAction.ShouldEndSpecial = true;
				if (DataCenter.CurrentDutyRotation?.RaiseGCD(out act) == true)
				{
					return true;
				}

				if (RaiseGCD(out act))
				{
					return true;
				}
			}
			IBaseAction.ShouldEndSpecial = false;

			if (DataCenter.AutoStatus.HasFlag(AutoStatus.Raise))
			{
				if (DataCenter.CurrentDutyRotation?.RaiseGCD(out act) == true)
				{
					return true;
				}

				if (RaiseGCD(out act))
				{
					if (HasSwift || IsLastAction(ActionID.SwiftcastPvE))
					{
						return true;
					}

					if (Service.Config.RaisePlayerBySwift && !SwiftcastPvE.Cooldown.IsCoolingDown && WeaponRemain <= 0.5f && SwiftcastPvE.CanUse(out act))
					{
						return true;
					}

					if (mustUse && !IsMoving)
					{
						return true;
					}
				}
			}

			return false;
		}
		return false;
	}

	/// <summary>
	/// Attempts to use the Interrupt GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	protected virtual bool MyInterruptGCD(out IAction? act)
	{
		act = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		act = null;
		return false;
	}

	/// <summary>
	/// Attempts to use the Raise GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	protected virtual bool RaiseGCD(out IAction? act)
	{
		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Raise))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		IBaseAction.ShouldEndSpecial = false;
		act = null;
		return false;
	}

	/// <summary>
	/// Attempts to use the Dispel GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	protected virtual bool DispelGCD(out IAction? act)
	{
		act = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Dispel))
		{
			IBaseAction.ShouldEndSpecial = true;
		}
		if (!HasSwift && EsunaPvE.CanUse(out act))
		{
			return true;
		}

		IBaseAction.ShouldEndSpecial = false;
		return false;
	}

	/// <summary>
	///
	/// </summary>
	protected virtual bool ProvokeGCD(out IAction? act)
	{
		if (DataCenter.CommandStatus.HasFlag(AutoStatus.Provoke))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		IBaseAction.ShouldEndSpecial = false;
		act = null;
		return false;
	}

	/// <summary>
	/// Attempts to use the Emergency GCD action.
	/// </summary>
	/// <param name="nextGCD">The next GCD action.</param>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	protected virtual bool EmergencyGCD(IAction? nextGCD, out IAction? act)
	{
		act = null;
		if (DataCenter.IsPvP)
		{
			if (DataCenter.Job != Job.BRD && DataCenter.Job != Job.WHM && PurifyPvP.CanUse(out act))
			{
				if (Service.Config.PvpPurifyStun && StatusHelper.PlayerHasStatus(false, StatusID.Stun_1343))
				{
					return true;
				}

				if (Service.Config.PvpPurifyHeavy && StatusHelper.PlayerHasStatus(false, StatusID.Heavy_1344))
				{
					return true;
				}

				if (Service.Config.PvpPurifyBind && StatusHelper.PlayerHasStatus(false, StatusID.Bind_1345))
				{
					return true;
				}

				if (Service.Config.PvpPurifySilence && StatusHelper.PlayerHasStatus(false, StatusID.Silence_1347))
				{
					return true;
				}

				if (Service.Config.PvpPurifyDeepFreeze && StatusHelper.PlayerHasStatus(false, StatusID.DeepFreeze_3219))
				{
					return true;
				}

				if (Service.Config.PvpPurifyMiracleOfNature && StatusHelper.PlayerHasStatus(false, StatusID.MiracleOfNature))
				{
					return true;
				}
			}

			if (GuardPvP.CanUse(out act) && Player?.GetHealthRatio() <= Service.Config.HealthForGuard && !StatusHelper.PlayerHasStatus(true, StatusID.UndeadRedemption) && !StatusHelper.PlayerHasStatus(true, StatusID.InnerRelease_1303))
			{
				return true;
			}

			if (RecuperatePvP.CanUse(out act))
			{
				return true;
			}

			if (StandardissueElixirPvP.CanUse(out act))
			{
				return true;
			}
		}

		if (ShouldSkipAction())
		{
			return false;
		}

		act = null!;
		return false;
	}

	/// <summary>
	/// Attempts to use the Move Forward GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	[RotationDesc(DescType.MoveForwardGCD)]
	protected virtual bool MoveForwardGCD(out IAction? act)
	{
		act = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.MoveForward))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		IBaseAction.ShouldEndSpecial = false;
		act = null;
		return false;
	}

	/// <summary>
	/// Attempts to use the Heal Single GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	[RotationDesc(DescType.HealSingleGCD)]
	protected virtual bool HealSingleGCD(out IAction? act)
	{
		act = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleSpell))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		IBaseAction.ShouldEndSpecial = false;
		act = null;
		return false;
	}

	/// <summary>
	/// Attempts to use the Heal Area GCD action.
	/// </summary>
	/// <param name="action">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	[RotationDesc(DescType.HealAreaGCD)]
	protected virtual bool HealAreaGCD(out IAction? action)
	{
		action = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealAreaSpell))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		IBaseAction.ShouldEndSpecial = false;
		action = null!;
		return false;
	}

	/// <summary>
	/// Attempts to use the Defense Single GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	[RotationDesc(DescType.DefenseSingleGCD)]
	protected virtual bool DefenseSingleGCD(out IAction? act)
	{
		act = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		IBaseAction.ShouldEndSpecial = false;
		act = null!;
		return false;
	}

	/// <summary>
	/// Attempts to use the Defense Area GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	[RotationDesc(DescType.DefenseAreaGCD)]
	protected virtual bool DefenseAreaGCD(out IAction? act)
	{
		act = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		if (DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseArea))
		{
			IBaseAction.ShouldEndSpecial = true;
		}

		IBaseAction.ShouldEndSpecial = false;
		act = null;
		return false;
	}

	/// <summary>
	/// Attempts to use the General GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	protected virtual bool GeneralGCD(out IAction? act)
	{
		act = null;
		if (ShouldSkipAction())
		{
			return false;
		}

		act = null;
		return false;
	}

	private bool ShouldSkipAction()
	{
		return DataCenter.CommandStatus.HasFlag(AutoStatus.Raise) && Role is JobRole.Healer && (HasSwift || IsLastAction(ActionID.SwiftcastPvE));
	}
}
