using ECommons.ExcelServices;
using ECommons.Logging;

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

	// Quest battle kits replace the whole hotbar, so they ignore the duty action toggle.
	private static Duties.DutyRotation? ActiveDutyRotation =>
		Service.Config.UseDutyActions || DataCenter.IsInQuestBattle ? DataCenter.CurrentDutyRotation : null;

	private IAction? GCD()
	{
		var act = DataCenter.CommandNextAction;

		using (new ForceEnableScope())
		{
			if (act is IBaseAction a && a.Info.IsRealGCD
				&& a.CanUse(out _, usedUp: true, skipAoeCheck: true, skipStatusProvideCheck: true))
			{
				return act;
			}
		}

		if (DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting))
		{
			return null;
		}

		if (DataCenter.Orbonne && IsLastAction(ActionID.HeavenlyShieldPvE) && DataCenter.IsAgriasCastingSpecialIndicator())
		{
			return null;
		}

		// The channel locks hold "during AOE mitigations" (their setting text), as the ability path
		// does: while the announced hit has not landed. Without that the GCD stayed held after the hit
		// until some ability broke the channel - up to its full 18 s.
		if (Service.Config.PldlockCasting && DataCenter.Job == Job.PLD && IsLastAction(ActionID.PassageOfArmsPvE) && StatusHelper.PlayerHasStatus(true, StatusID.PassageOfArms) && DataCenter.AreaHitPending)
		{
			return null;
		}

		if (Service.Config.AstlockCasting && DataCenter.Job == Job.AST && IsLastAction(ActionID.CollectiveUnconsciousPvE) && StatusHelper.PlayerHasStatus(true, StatusID.CollectiveUnconscious_848) && DataCenter.AreaHitPending)
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
			if (ActiveDutyRotation?.EmergencyGCD(act, out act) == true)
			{
				return act;
			}
			if (EmergencyGCD(act, out act))
			{
				return act;
			}

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.Interrupt))
			{
				if (ActiveDutyRotation?.MyInterruptGCD(out act) == true)
				{
					return act;
				}
				if (MyInterruptGCD(out var action))
				{
					return action;
				}
			}

			// The owner's experiment of 18.10.2026 and his rules of 23.10.2026: an enemy's cast stopped by the job's own
			// stun or a sleep (concept 08, "Ein gegnerisches Wirken stoppen").
			if (StopEnemyCast(out act))
			{
				return act;
			}

			IBaseAction.TargetOverride = TargetType.Dispel;
			if (DataCenter.MergedStatus.HasFlag(AutoStatus.Dispel))
			{
				if (ActiveDutyRotation?.DispelGCD(out act) == true)
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
				if (ActiveDutyRotation?.ProvokeGCD(out act) == true)
				{
					return act;
				}
				if (ProvokeGCD(out var action))
				{
					return action;
				}
			}

			IBaseAction.TargetOverride = TargetType.Death;
			if (Service.Config.RaisePlayerFirst && TryRaise(out act))
			{
				return act;
			}

			IBaseAction.TargetOverride = null;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveForward))
			{
				if (ActiveDutyRotation?.MoveForwardGCD(out act) == true)
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
			IBaseAction.HealPathRunning = true;

			if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealAreaSpell))
			{
				IBaseAction.AutoHealCheck = true;
				if (ActiveDutyRotation?.HealAreaGCD(out act) == true)
				{
					return act;
				}

				if (CanCastHeals && HealAreaGCD(out var action))
				{
					return action;
				}
				IBaseAction.AutoHealCheck = false;
			}
			if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealAreaSpell))
			{
				IBaseAction.AutoHealCheck = true;
				if (ActiveDutyRotation?.HealAreaGCD(out act) == true)
				{
					return act;
				}

				if (CanHealAreaSpell && CanCastHeals && HealAreaGCD(out var action))
				{
					return action;
				}

				IBaseAction.AutoHealCheck = false;
			}

			if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleSpell))
			{
				IBaseAction.AutoHealCheck = true;
				if (ActiveDutyRotation?.HealSingleGCD(out act) == true)
				{
					return act;
				}

				if (CanCastHeals && HealSingleGCD(out var action))
				{
					return action;
				}
				IBaseAction.AutoHealCheck = false;
			}
			if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealSingleSpell))
			{
				IBaseAction.AutoHealCheck = true;
				if (ActiveDutyRotation?.HealSingleGCD(out act) == true)
				{
					return act;
				}

				if (CanHealSingleSpell && CanCastHeals && HealSingleGCD(out var action))
				{
					return action;
				}

				IBaseAction.AutoHealCheck = false;
			}

			IBaseAction.TargetOverride = null;
			IBaseAction.HealPathRunning = false;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseArea))
			{
				IBaseAction.AreaDefenceRunning = true;
				try
				{
					if (ActiveDutyRotation?.DefenseAreaGCD(out act) == true)
					{
						return act;
					}

					if (DefenseAreaGCD(out var action))
					{
						return action;
					}
				}
				finally
				{
					IBaseAction.AreaDefenceRunning = false;
				}
			}

			IBaseAction.TargetOverride = TargetType.BeAttacked;

			if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseSingle))
			{
				IBaseAction.CommandedDefenceRunning = DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle);
				try
				{
					if (ActiveDutyRotation?.DefenseSingleGCD(out act) == true)
					{
						return act;
					}

					if (DefenseSingleGCD(out var action))
					{
						return action;
					}
				}
				finally
				{
					IBaseAction.CommandedDefenceRunning = false;
				}
			}

			IBaseAction.TargetOverride = TargetType.Death;
			if (!Service.Config.RaisePlayerFirst && TryRaise(out act))
			{
				return act;
			}

			IBaseAction.ShouldEndSpecial = false;
			IBaseAction.TargetOverride = null;

			if (!DataCenter.MergedStatus.HasFlag(AutoStatus.NoCasting))
			{
				if (ActiveDutyRotation?.GeneralGCD(out act) == true)
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

					// A healing threshold, so it reads the health heading for the moment the heal lands, as "Heal ahead of
					// incoming damage" states for every one - and so does the spread weighed with it, as in the area
					// thresholds (A182, A358). With that setting off both are the levels.
					DataCenter.ComputeForecastAreaStats(false, out _, out var forecastSpread, out _, out _);
					if (DataCenter.ForecastMinHp(false) < Service.Config.HealWhenNothingTodoBelow)
					{
						IBaseAction.TargetOverride = TargetType.Heal;
						IBaseAction.HealPathRunning = true;

						if (forecastSpread < Service.Config.HealthDifference)
						{
							var count = 0;
							foreach (var hp in DataCenter.PartyMembersHP)
							{
								if (hp < 1)
								{
									count++;
								}
							}
							if (count > 2 && ActiveDutyRotation?.HealAreaGCD(out act) == true)
							{
								return act;
							}
							if (count > 2 && HealAreaGCD(out act))
							{
								return act;
							}
						}
						if (ActiveDutyRotation?.HealSingleGCD(out act) == true)
						{
							return act;
						}
						if (HealSingleGCD(out act))
						{
							return act;
						}

						IBaseAction.TargetOverride = null;
						IBaseAction.HealPathRunning = false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Exception in GCD method: {ex.Message}");
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
	/// Healing is held under Scalebound, and under Shackled Healing unless the player is alone.
	/// </summary>
	private static bool CanCastHeals => !StatusHelper.PlayerHealingPunished();

	private bool TryRaise(out IAction? act)
	{
		act = null;
		if (!DataCenter.MergedStatus.HasFlag(AutoStatus.Raise) || !DataCenter.CanRaise())
		{
			return false;
		}

		if (RaiseSpell(out act, false))
		{
			return true;
		}

		// The two "only healer" modes are the two others plus "the other healers are dead"; the plain one inherits
		// the Swiftcast reservation of HardCastNormal - every mode's text starts "Raise while Swiftcast is on cooldown".
		var mayHardCast = Service.Config.HardCastRaiseType switch
		{
			HardCastRaiseType.HardCastNormal => !SwiftcastComingForRaise,
			HardCastRaiseType.HardCastSwiftCooldown => HardCastBeatsWaitingForSwiftcast,
			HardCastRaiseType.HardCastOnlyHealer => !SwiftcastComingForRaise && !AnyOtherLivingRaiser(),
			HardCastRaiseType.HardCastOnlyHealerSwiftCooldown => HardCastBeatsWaitingForSwiftcast && !AnyOtherLivingRaiser(),
			_ => false,
		};

		return mayHardCast && RaiseSpell(out act, true);
	}

	/// <summary>
	/// Is anyone besides the player still standing who could take this raise?
	///
	/// This is what the "only healer" hard cast modes mean to ask: hard casting costs eight seconds
	/// of GCD and is only worth it when nobody else can do it instead. The branches used to build
	/// two sets of party healers and compare their sizes, which counted healers rather than raisers,
	/// excluded the player from both - so a lone healer compared 0 == 0 behind a "&gt; 0" guard and
	/// never hard cast at all - and could not express "nobody else" in the first place.
	///
	/// The reference set lives in <see cref="DataCenter.AnyLivingRaiser(bool, bool)"/>, shared with
	/// the Phoenix Down decision, which asks the same question with the player counted.
	///
	/// Healers only, and that is the user's instruction rather than a narrowing of my own: every
	/// one of these modes says "other <i>healers</i> are dead" in its own option text, and the
	/// wider raiser set would hold the hard cast back because a Summoner is alive - an assumption
	/// about what a stranger is going to do with a corpse. In an eight-man party with one healer
	/// down that is the difference between the dead player getting up now and waiting on somebody
	/// else. The feather keeps the wide set; there his instruction says raiser.
	/// </summary>
	private static bool AnyOtherLivingRaiser()
	{
		return DataCenter.AnyLivingRaiser(excludeSelf: true, healersOnly: true);
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
	///
	/// <c>EnoughLevel</c> closes the same hole one level down. <c>IsCoolingDown</c> reads the recast
	/// timer (<see cref="ActionIdHelper.IsCoolingDown"/>), and an action the player cannot cast at
	/// all has no timer running - so a raiser below Swiftcast's level, or synced under it in older
	/// content, reported "Swiftcast is ready" forever. With the setting on that shut every hard cast
	/// branch while the Swiftcast path itself failed on CanUse, and nobody was raised by either.
	/// Raise comes well before Swiftcast, so the gap is a real span of levels rather than a corner
	/// case. At full level EnoughLevel is true and nothing changes.
	/// </summary>
	private bool SwiftcastComingForRaise =>
		Service.Config.RaisePlayerBySwift
		&& SwiftcastPvE.EnoughLevel
		&& !SwiftcastPvE.Cooldown.IsCoolingDown;

	/// <summary>
	/// For the two modes that promise "cooldown is higher than raise cast time": is starting the
	/// hard cast now better than waiting for Swiftcast?
	///
	/// It is when the rotation is not spending Swiftcast on raises at all, when Swiftcast is not
	/// available to this player in the first place (same reason as in
	/// <see cref="SwiftcastComingForRaise"/>), or when the wait is longer than the cast itself.
	/// This cannot simply read <c>!SwiftcastComingForRaise</c>: that is true for any running
	/// cooldown and would drop the very comparison these two modes exist for.
	///
	/// It lives here rather than four times in the dispatch because the four copies are the same
	/// decision - the previous gap was in all four of them at once.
	/// </summary>
	private bool HardCastBeatsWaitingForSwiftcast =>
		!Service.Config.RaisePlayerBySwift
		|| !SwiftcastPvE.EnoughLevel
		|| (SwiftcastPvE.Cooldown.IsCoolingDown && Raise != null
			&& Raise.Info.CastTime < SwiftcastPvE.Cooldown.RecastTimeRemainOneCharge);

	private bool RaiseSpell(out IAction? act, bool mustUse)
	{
		act = null;

		if (DataCenter.CanRaise())
		{
			if (DataCenter.CommandStatus.HasFlag(AutoStatus.Raise))
			{
				IBaseAction.ShouldEndSpecial = true;
				if (ActiveDutyRotation?.RaiseGCD(out act) == true)
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
				if (ActiveDutyRotation?.RaiseGCD(out act) == true)
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
		return false;
	}

	/// <summary>
	/// Stops the cast of an enemy that is not a boss - an area attack reaching the party, or a cast the game marks
	/// interruptible - while enough of it is left: first with the job's own stopper (<see cref="StopEnemyCastByJob"/>),
	/// otherwise with Sleep or Repose. The owner's experiment of 18.10.2026, "kann sleep der magiewirkenden klassen
	/// unterbrechen?", and his rules of 23.10.2026: the stop is the melee interrupt's counterpart, and "wenn holy stunnen
	/// kann, ist das vorzuziehen" (concept 08, "Ein gegnerisches Wirken stoppen"). Healers and casters together: the
	/// roles without an interrupt. What came of each sleep, and what it learns, is SleepInterruptWatch's.
	/// </summary>
	private bool StopEnemyCast(out IAction? act)
	{
		act = null;
		var role = DataCenter.Role;
		if (!Service.Config.SleepToStopAreaCasts || DataCenter.IsPvP || !DataCenter.InCombat
			|| role is not (JobRole.Healer or JobRole.RangedMagical))
		{
			return false;
		}

		// His rule: Swiftcast stays for raises, and a stop under it would spend it. Dualcast and Triplecast are
		// spent on whatever spell comes next, so a stop may take them.
		if (StatusHelper.PlayerHasStatus(true, StatusID.Swiftcast))
		{
			return false;
		}

		// The time from the end of an own cast to its effect, as last measured for a sleep - before the first, the
		// measured round trip of a press.
		var lag = SleepInterruptWatch.Lag >= 0f ? SleepInterruptWatch.Lag : Math.Max(0f, TankbusterForecast.RoundTrip);

		// The casts worth stopping, the one that ends soonest first: the others leave time for the next GCD.
		List<(IBattleChara Enemy, float Left)> casts = [];
		foreach (var enemy in DataCenter.AllHostileTargets)
		{
			if (enemy != null && enemy.IsCasting && !enemy.IsBossFromIcon() && SleepInterruptWatch.IsCastWorthStopping(enemy))
			{
				casts.Add((enemy, enemy.TotalCastTime - enemy.CurrentCastTime));
			}
		}

		if (casts.Count == 0)
		{
			return false;
		}

		casts.Sort((a, b) => a.Left.CompareTo(b.Left));

		// A healer's GCD belongs to the member about to die.
		if (role == JobRole.Healer)
		{
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member != null && !member.IsDead && member.IsInCriticalClass())
				{
					return false;
				}
			}
		}

		var sleep = role == JobRole.Healer ? ReposePvE : SleepPvE;

		// What a sleep needs of the enemy's cast: its own cast and the GCD still running, the time to its effect, and
		// what has to be left of the cast when it lands, learned from sleeps that came too late.
		var needed = sleep.Info.NeededStatusMargin + lag + SleepInterruptWatch.LandingMargin;
		foreach (var (enemy, left) in casts)
		{
			if (StopEnemyCastByJob(enemy, left, lag, out act))
			{
				return true;
			}

			if (!sleep.EnoughLevel || left <= needed || SleepInterruptWatch.IsUnstoppable(enemy.CastActionId)
				|| !SleepInterruptWatch.CanStillBeSlept(enemy) || AreaReachesNewMob(enemy, sleep.Info.EffectRange))
			{
				continue;
			}

			// The target filter is swapped in for this call only and restored afterwards, as for Nascent Flash.
			var setting = sleep.Setting;
			var canTarget = setting.CanTarget;
			var chosenId = enemy.GameObjectId;
			setting.CanTarget = t => canTarget(t) && t.GameObjectId == chosenId;
			try
			{
				if (!sleep.CanUse(out act, skipAoeCheck: true, skipTTKCheck: true))
				{
					continue;
				}
			}
			finally
			{
				setting.CanTarget = canTarget;
			}

			SleepInterruptWatch.AimAt(enemy, sleep, left, needed);
			return true;
		}

		act = null;
		return false;
	}

	/// <summary>
	/// The job's own way to stop <paramref name="enemy"/>'s cast, tried before a sleep: a stun that reaches it in time
	/// (concept 08, "Ein gegnerisches Wirken stoppen"). Among the healers and casters only the White Mage has one.
	/// </summary>
	/// <param name="enemy">The casting enemy, not a boss.</param>
	/// <param name="left">Seconds left on its cast.</param>
	/// <param name="lag">Seconds from the end of an own cast to its effect, as measured.</param>
	/// <param name="act">The action that stops it.</param>
	/// <returns>True when the job stops the cast itself.</returns>
	protected virtual bool StopEnemyCastByJob(IBattleChara enemy, float left, float lag, out IAction? act)
	{
		act = null;
		return false;
	}

	/// <summary>
	/// Whether an area action of <paramref name="radius"/> around <paramref name="center"/> would reach an enemy that is
	/// not fighting yet while "Don't pull new enemies with AoE" is on - the setting's own test, a hostile without a
	/// target. A stop is an area action like any other, and the setting says "never".
	/// </summary>
	/// <param name="center">The centre of the area.</param>
	/// <param name="radius">Its radius, zero for a single target.</param>
	/// <returns>True when the setting forbids the action.</returns>
	protected static bool AreaReachesNewMob(IBattleChara center, float radius)
	{
		if (!Service.Config.NoNewHostiles || radius <= 0f || center == null)
		{
			return false;
		}

		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && hostile.TargetObject == null
				&& Vector3.Distance(hostile.Position, center.Position) - hostile.HitboxRadius <= radius)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Attempts to use the Raise GCD action.
	/// </summary>
	/// <param name="act">The action to be performed.</param>
	/// <returns>True if the action can be used; otherwise, false.</returns>
	protected virtual bool RaiseGCD(out IAction? act)
	{
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
		return TryPvPEmergency(out act);
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
		return false;
	}

	private bool ShouldSkipAction()
	{
		return DataCenter.CommandStatus.HasFlag(AutoStatus.Raise) && Role is JobRole.Healer && (HasSwift || IsLastAction(ActionID.SwiftcastPvE));
	}
}
