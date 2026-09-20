using ECommons.GameFunctions;
using ECommons.GameHelpers;

namespace RotationSolver.Updaters;

internal static class StateUpdater
{
	private static bool CanUseHealAction =>
		// PvP
		DataCenter.IsPvP
		// Job. AutoHealTimeToKill is a child option of UseHealWhenNotAHealer and only gates non-healers:
		// a healer's heal flags must not switch off because the average time-to-kill of a trash pack
		// dipped under eight seconds while the tank is still taking the hits.
		|| (Service.Config.AutoHeal
			&& (DataCenter.Role == JobRole.Healer
				|| (Service.Config.UseHealWhenNotAHealer
					&& (!DataCenter.InCombat || CustomRotation.IsLongerThan(Service.Config.AutoHealTimeToKill))))
			&& (DataCenter.InCombat || Service.Config.HealOutOfCombat));

	public static void UpdateState()
	{
		DataCenter.CommandStatus = StatusFromCmdOrCondition();
		DataCenter.AutoStatus = StatusFromAutomatic();
		if (!DataCenter.InCombat && DataCenter.AttackedTargets.Count > 0)
		{
			DataCenter.ResetAllRecords();
		}
	}

	private static AutoStatus StatusFromAutomatic()
	{
		var status = AutoStatus.None;

		if (ShouldAddNoCasting())
		{
			status |= AutoStatus.NoCasting;
		}

		if (ShouldAddDispel())
		{
			status |= AutoStatus.Dispel;
		}

		if (ShouldAddInterrupt())
		{
			status |= AutoStatus.Interrupt;
		}

		if (ShouldAddAntiKnockback())
		{
			status |= AutoStatus.AntiKnockback;
		}

		if (ShouldAddPositional())
		{
			status |= AutoStatus.Positional;
		}

		status |= StatusFromHealing();

		if (ShouldAddDefenseArea())
		{
			status |= AutoStatus.DefenseArea;
		}

		if (ShouldAddDefenseSingle())
		{
			status |= AutoStatus.DefenseSingle;
		}

		if (ShouldAddRaise())
		{
			status |= AutoStatus.Raise;
		}

		if (ShouldAddProvoke())
		{
			status |= AutoStatus.Provoke;
		}

		if (ShouldAddTankStance())
		{
			status |= AutoStatus.TankStance;
		}

		if (ShouldAddSpeed())
		{
			status |= AutoStatus.Speed;
		}

		return status;
	}

	// Condition methods for each AutoStatus flag

	private static bool ShouldAddNoCasting()
	{
		return DataCenter.IsHostileCastingStop;
	}

	private static bool ShouldAddDispel()
	{
		if (DataCenter.DispelTarget != null)
		{
			return true;
		}
		return false;
	}

	private static bool ShouldAddRaise()
	{
		return DataCenter.DeathTarget != null;
	}

	private static bool ShouldAddPositional()
	{
		if (DataCenter.Role == JobRole.Melee && ActionUpdater.NextGCDAction != null && Service.Config.AutoUseTrueNorth)
		{
			var id = ActionUpdater.NextGCDAction.ID;
			var target = ActionUpdater.NextGCDAction.Target.Target;
			if (target == null)
			{
				return false;
			}

			if (target.IsDead)
			{
				return false;
			}

			// Validate liveness before the native reads inside HasPositional()/
			// FindEnemyPositional() below; a try/catch can't cover an
			// AccessViolationException from a freed native object.
			if (!target.IsValid())
			{
				return false;
			}

			if (target.Address == nint.Zero)
			{
				return false;
			}

			try
			{
				if (ConfigurationHelper.ActionPositional.TryGetValue((ActionID)id, out var positional)
					&& target.HasPositional() && positional != target.FindEnemyPositional())
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
		return false;
	}

	private static bool ShouldAddDefenseArea()
	{
		if (DataCenter.InCombat && Service.Config.UseAoeDefense && DataCenter.IsHostileCastingAOE && !DataCenter.IsTyrantCastingSpecialIndicator())
		{
			return true;
		}

		// The same question asked of a cast the filter above drops for being interruptible. It only
		// answers for an action whose measured share is at or above what the largest barrier absorbs,
		// so it cannot reopen the enemy-count fallback A9 removed. See IsHostileCastingLargeArea.
		if (DataCenter.InCombat && Service.Config.UseAoeDefense && DataCenter.IsHostileCastingLargeArea
			&& !DataCenter.IsTyrantCastingSpecialIndicator())
		{
			return true;
		}

		if (DataCenter.InCombat && Service.Config.UseBmrTimeline
			&& DataCenter.BMRNextRaidwideIn > 0.6f
			&& DataCenter.BMRNextRaidwideIn <= Service.Config.BMRRaidwideMitWindow)
		{
			return true;
		}

		return false;
	}

	private static bool ShouldAddDefenseSingle()
	{
		if (!DataCenter.InCombat || !Service.Config.UseStDefense || DataCenter.IsTyrantCastingSpecialIndicator())
		{
			return false;
		}

		if (DataCenter.Role == JobRole.Healer)
		{
			if (DataCenter.IsHostileCastingToTank)
			{
				foreach (var member in DataCenter.PartyMembers)
				{
					var attackingCount = 0;
					foreach (var hostile in DataCenter.AllHostileTargets)
					{
						if (hostile.TargetObjectId == member.GameObjectId)
						{
							attackingCount++;
						}
					}

					if (attackingCount == 1)
					{
						return true;
					}
				}
			}

			// above only covers protecting the tank, not a buster landing on us
			if (DataCenter.IsHostileCastingTankBusterAtMe)
			{
				return true;
			}

			if (DataCenter.BMRTankbusterImminent)
			{
				return true;
			}
		}

		if (DataCenter.Role == JobRole.Tank)
		{
			var movingHere = false;
			if (DataCenter.NumberOfHostilesInMaxRange != 0)
			{
				movingHere = (float)DataCenter.NumberOfHostilesInRange / DataCenter.NumberOfHostilesInMaxRange > 0.3f;
			}

			var tarOnMeCount = 0;
			var attacked = false;
			var playerId = Player.Object?.GameObjectId ?? 0;
			if (playerId != 0)
			{
				foreach (var hostile in DataCenter.AllHostileTargets)
				{
					if (hostile.TargetObjectId == playerId && hostile.DistanceToPlayer() <= 3)
					{
						tarOnMeCount++;
						if (!attacked && ObjectHelper.IsAttacked(hostile))
						{
							attacked = true;
						}
					}
				}
			}

			if (tarOnMeCount >= Service.Config.AutoDefenseNumber
				&& ObjectHelper.GetPlayerHealthRatio() <= Service.Config.HealthForAutoDefense
				&& movingHere && attacked)
			{
				return true;

			}

			if (DataCenter.IsHostileCastingToTank)
			{
				return true;
			}

			if (DataCenter.BMRTankbusterImminent)
			{
				return true;
			}
		}

		if (DataCenter.Role is JobRole.Melee or JobRole.RangedPhysical or JobRole.RangedMagical)
		{
			// A cast actually landing on us covers the "buster went to the wrong person" case with a
			// real target, regardless of whether a tank is alive - leave this branch unrestricted.
			if (DataCenter.IsHostileCastingTankBusterAtMe)
			{
				return true;
			}

			// BMR predicts timing, not who gets hit, so for this role it is only a reasonable proxy when
			// no tank is alive to eat it. Otherwise the cast-verified branch above is the only trigger.
			if (DataCenter.BMRTankbusterImminent && DataCenter.PartyTank == null)
			{
				return true;
			}
		}

		return false;
	}

	// Helper: Returns true if there are any healers in the party with HP > 0
	private static bool AnyLivingHealerInParty()
	{
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member.IsJobCategory(JobRole.Healer) && !member.IsDead)
			{
				return true;
			}
		}
		return false;
	}

	private static bool NonHealerHealLogic()
	{
		if (Service.Config.OnlyHealAsNonHealIfNoHealers && DataCenter.Role != JobRole.Healer && AnyLivingHealerInParty())
		{
			return false;
		}
		return true;
	}

	// Owner's rule, second stage: "die aktuelle hp liegt unter dem schadenswert. dann wäre aber eine
	// heilung sinnvoll bis max maxhp."
	//
	// Every threshold below reads the health a member HAS. None of them reads the health he will
	// have once the cast already on screen lands, so a party at 60% in front of a 45% raidwide is
	// above every threshold and dies to it. The size of that cast is measured (concept 13) and was
	// so far only used to decide whether to MITIGATE; this is the other half of the same figure.
	//
	// Deliberately placed at the same threshold the flag itself uses, not at a stricter one: the
	// question is "would this hit put anyone where we would heal anyway", asked one cast earlier.
	// Where nothing is announced or its size has not been measured, the answer is false and the
	// flags behave exactly as before.
	private static bool ShouldHealAheadOfAnnouncedHit(float threshold)
	{
		if (!Service.Config.HealAheadOfAnnouncedHit)
		{
			return false;
		}

		// Asked here rather than read from whatever ran before: walking the casting enemies is what
		// establishes the size in the first place. Reading the recorded share on its own would make
		// this rule depend on the defensive branch having run earlier in the same frame - and that
		// branch is itself behind UseAoeDefense, so with area defence switched off the size would
		// never be established and this rule would silently never fire.
		//
		// The same call also decides the mitigation question, and its verdicts agree with this one
		// by construction: a cast it rates too small to mitigate is one that leaves everybody above
		// the healing level, which is the same sentence read from the other end.
		if (!DataCenter.IsHostileCastingAOE)
		{
			return false;
		}

		return DataCenter.AnnouncedHitDropsAnyoneBelow(threshold);
	}

	private static readonly StatusID[] HellInACellStatuses =
	[
		StatusID.HellInACell,
		StatusID.HellInACell_4732,
		StatusID.HellInACell_4733,
		StatusID.HellInACell_4734,
		StatusID.HellInACell_4735,
		StatusID.HellInACell_4736,
		StatusID.HellInACell_4737,
		StatusID.HellInACell_4738,
	];

	private static AutoStatus StatusFromHealing()
	{
		// Preconditions shared by every heal flag, evaluated once per update instead of once per flag.
		if (!DataCenter.HPNotFull || !CanUseHealAction || DataCenter.IsTyrantCastingSpecialIndicator())
		{
			return AutoStatus.None;
		}

		// Only allow non-healers to heal if there are no living healers in the party
		if (!NonHealerHealLogic())
		{
			return AutoStatus.None;
		}

		var status = AutoStatus.None;

		var doomNeedHealingCount = 0;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member.DoomNeedHealing())
			{
				doomNeedHealingCount++;
			}
		}

		// Heal spells are held while the player is inside a Hell in a Cell in M9S.
		var canUseHealSpell = !DataCenter.IsInM9S || !StatusHelper.PlayerHasStatus(false, HellInACellStatuses);

		var singleAbilityCount = ShouldHealSingle(StatusHelper.SingleHots,
			Service.Config.HealthSingleAbility,
			Service.Config.HealthSingleAbilityHot);

		var singleSpellCount = canUseHealSpell
			? ShouldHealSingle(StatusHelper.SingleHots,
				Service.Config.HealthSingleSpell,
				Service.Config.HealthSingleSpellHot)
			: 0;

		var partyCount = DataCenter.PartyMembers.Count;
		var areaHotRatio = partyCount > 2 ? SelfHealingOfTimeRatio(StatusHelper.AreaHots) : 0f;

		// Prioritize area healing if multiple members have DoomNeedHealing
		if (doomNeedHealingCount > 1 || singleAbilityCount > 2
			|| ShouldHealAheadOfAnnouncedHit(Service.Config.HealthAreaAbility)
			|| ShouldHealArea(partyCount, Service.Config.HealthAreaAbility, Service.Config.HealthAreaAbilityHot, areaHotRatio))
		{
			status |= AutoStatus.HealAreaAbility;
		}

		if (canUseHealSpell && (doomNeedHealingCount > 1 || singleSpellCount > 2
			|| ShouldHealAheadOfAnnouncedHit(Service.Config.HealthAreaSpell)
			|| ShouldHealArea(partyCount, Service.Config.HealthAreaSpell, Service.Config.HealthAreaSpellHot, areaHotRatio)))
		{
			status |= AutoStatus.HealAreaSpell;
		}

		var onlyHealSelf = Service.Config.OnlyHealSelfWhenNoHealer
			&& DataCenter.Role != JobRole.Healer;

		if (onlyHealSelf)
		{
			// Prioritize healing self if DoomNeedHealing is true
			var selfDoomed = StatusHelper.PlayerDoomNeedHealing();

			if (selfDoomed || ShouldHealSelf(StatusHelper.SingleHots, Service.Config.HealthSingleAbility, Service.Config.HealthSingleAbilityHot))
			{
				status |= AutoStatus.HealSingleAbility;
			}

			if (canUseHealSpell && (selfDoomed || ShouldHealSelf(StatusHelper.SingleHots, Service.Config.HealthSingleSpell, Service.Config.HealthSingleSpellHot)))
			{
				status |= AutoStatus.HealSingleSpell;
			}
		}
		else
		{
			// Prioritize healing any party member with DoomNeedHealing
			if (doomNeedHealingCount > 0 || singleAbilityCount > 0)
			{
				status |= AutoStatus.HealSingleAbility;
			}

			if (canUseHealSpell && (doomNeedHealingCount > 0 || singleSpellCount > 0))
			{
				status |= AutoStatus.HealSingleSpell;
			}
		}

		return status;
	}

	private static bool ShouldHealArea(int partyCount, float healArea, float healAreaHot, float ratio)
	{
		if (partyCount <= 2)
		{
			return false;
		}

		// If party is larger than 4 people, we select the 4 lowest HP players
		// in the party, and then calculate the thresholds on them instead.
		return partyCount > 4
			? DataCenter.LowestPartyMembersDifferHP < Service.Config.HealthDifference
				&& DataCenter.LowestPartyMembersAverHP < Lerp(healArea, healAreaHot, ratio)
			: DataCenter.PartyMembersDifferHP < Service.Config.HealthDifference
				&& DataCenter.PartyMembersAverHP < Lerp(healArea, healAreaHot, ratio);
	}

	private static bool ShouldAddAntiKnockback()
	{
		if (DataCenter.InCombat && DataCenter.IsInWindurst && StatusHelper.PlayerHasStatus(false, StatusID.WesterlyWinds) && StatusHelper.PlayerWillStatusEndGCD(2, 0, false, StatusID.WesterlyWinds))
		{
			return true;
		}

		if (DataCenter.InCombat && DataCenter.IsInWindurst && StatusHelper.PlayerHasStatus(false, StatusID.EasterlyWinds) && StatusHelper.PlayerWillStatusEndGCD(2, 0, false, StatusID.EasterlyWinds))
		{
			return true;
		}

		if (DataCenter.InCombat && Service.Config.UseKnockback && DataCenter.AreHostilesCastingKnockback)
		{
			return true;
		}

		// Proactive knockback prevention via BossModReborn timeline
		if (DataCenter.InCombat && Service.Config.UseBmrTimeline
			&& DataCenter.BMRNextKnockbackIn > 0.6f
			&& DataCenter.BMRNextKnockbackIn <= Service.Config.BMRKnockbackWindow)
		{
			return true;
		}

		return false;
	}

	private static bool ShouldAddProvoke()
	{
		// Cheapest checks first; the alliance tank count walks every alliance member.
		return DataCenter.ProvokeTarget != null
			&& (DataCenter.InCombat || Service.Config.ProvokeAnything)
			&& (DataCenter.Role == JobRole.Tank || StatusHelper.PlayerHasStatus(true, StatusID.VariantUltimatumSet))
			&& (Service.Config.AutoProvokeForTank || CountAllianceTanks() < 2);
	}

	private static bool ShouldAddInterrupt()
	{
		return DataCenter.InCombat && DataCenter.InterruptTarget != null && Service.Config.InterruptibleMoreCheck;
	}

	private static bool ShouldAddTankStance()
	{
		return Service.Config.AutoTankStance && DataCenter.Role == JobRole.Tank && !CustomRotation.HasTankStance && !AnyAllianceTankWithStance();
	}

	private static bool ShouldAddSpeed()
	{
		if (DataCenter.IsMoving && DataCenter.NotInCombatDelay && DataCenter.IsInDuty && Service.Config.AutoSpeedOutOfCombat)
		{
			return true;
		}

		if (DataCenter.IsMoving && DataCenter.NotInCombatDelay && !DataCenter.IsInDuty && Service.Config.AutoSpeedOutOfCombatNoDuty)
		{
			return true;
		}

		if (Service.Config.AutoSprintWithTank && DataCenter.IsMoving
			&& DataCenter.Role != JobRole.Tank
			&& !StatusHelper.PlayerHasStatus(false, StatusID.Sprint)
			&& AnyPartyTankSprinting())
		{
			return true;
		}

		return false;
	}

	// Helper methods used in condition methods

	private static float SelfHealingOfTimeRatio(params StatusID[] statusIds)
	{
		if (Player.Object == null)
		{
			return 0;
		}
		const float buffWholeTime = 15;

		var buffTime = StatusHelper.PlayerStatusTime(false, statusIds);

		return Math.Min(1, buffTime / buffWholeTime);
	}

	private static float GetHealingOfTimeRatio(IBattleChara target, params StatusID[] statusIds)
	{
		const float buffWholeTime = 15;

		var buffTime = target.StatusTime(false, statusIds);

		return Math.Min(1, buffTime / buffWholeTime);
	}

	private static int ShouldHealSingle(StatusID[] hotStatus, float healSingle, float healSingleHot)
	{
		var count = 0;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (ShouldHealSingle(member, hotStatus, healSingle, healSingleHot))
			{
				count++;
			}
		}
		return count;
	}

	private static bool ShouldHealSelf(StatusID[] hotStatus, float healSingle, float healSingleHot)
	{
		if (Player.Object == null)
		{
			return false;
		}

		if (Player.Object.StatusList == null)
		{
			return false;
		}

		if (DataCenter.IsPvP && StatusHelper.PlayerHasStatus(false, StatusID.Mounted))
		{
			return false;
		}

		if (DataCenter.IsInWindurst && StatusHelper.PlayerHasStatus(false, StatusID.HpRecoveryDown))
		{
			return false;
		}

		var doomed = StatusHelper.PlayerDoomNeedHealing();

		// Calculate the ratio of remaining healing-over-time effects on the target. If they have a "Doom" status, treat dot healing as non-existent.
		var ratio = doomed ? 0f : GetHealingOfTimeRatio(Player.Object, hotStatus);

		// Determine the target's health ratio. If they have a "Doom" status, treat their health as critically low (0.2).
		// Outside the Doom case this is the health the player is heading for by the time a heal
		// begun now would land - identical to the current ratio while HealAheadOfDamage is off, or
		// while the trend is not downward.
		var h = doomed ? 0.2f : ObjectHelper.GetForecastPlayerHealthRatio();

		// "Zero" here means a corpse, and it has to be asked of the real health. The forecast
		// reaches zero for somebody who is alive and about to die - the very case this exists for -
		// and reading it here would suppress the heal exactly then.
		if (ObjectHelper.GetPlayerHealthRatio() == 0
			|| StatusHelper.PlayerHasStatus(false, StatusHelper.HealingIneffectiveStatus))
		{
			return false;
		}

		// See ShouldHealSingle. Relevant here for a dark knight running a rotation that heals itself.
		if (ObjectHelper.PlayerIsHeldForDeathTrigger())
		{
			return false;
		}

		// Same construction as ShouldHealSingle: a protective status lowers the threshold rather
		// than suppressing the flag entirely. This path matters for a tank healing itself while
		// riding its own invulnerability.
		var normal = Lerp(healSingle, healSingleHot, ratio);
		var threshold = StatusHelper.PlayerNoNeedHealingInvuln()
			? normal
			: Math.Min(normal, Service.Config.HealthProtectedRatio);

		return h < threshold;
	}

	private static bool ShouldHealSingle(IBattleChara target, StatusID[] hotStatus, float healSingle, float healSingleHot)
	{
		if (target == null)
		{
			return false;
		}

		if (target.StatusList == null)
		{
			return false;
		}

		if (DataCenter.IsInWindurst && StatusHelper.HasStatus(target, false, StatusID.HpRecoveryDown))
		{
			return false;
		}

		if (DataCenter.IsPvP && StatusHelper.HasStatus(target, false, StatusID.Mounted))
		{
			return false;
		}

		// Calculate the ratio of remaining healing-over-time effects on the target. If they have a "Doom" status, treat dot healing as non-existent.
		var ratio = target.DoomNeedHealing() ? 0f : GetHealingOfTimeRatio(target, hotStatus);

		// Determine the target's health ratio. GetHealthRatio already treats "Doom" status targets as critically low (1%).
		// Forecast rather than current: a level threshold crossed at a steep rate leaves less time
		// than the heal it triggers needs to arrive. Identical to the current ratio while
		// HealAheadOfDamage is off, or while the member's trend is not downward.
		var h = target.GetForecastHealthRatio();

		// Healing that lands for nothing is still excluded outright - NoNeedHealingStatus mixes
		// that case in with genuine invulnerabilities, and only the latter get the softer
		// treatment below.
		// "Zero" here means a corpse, and it has to be asked of the real health. The forecast
		// reaches zero for somebody who is alive and about to die - the very case this exists for -
		// and reading it here would suppress the heal exactly then.
		var actual = target.GetHealthRatio();
		if (actual == 0 || target.HasStatus(false, StatusHelper.HealingIneffectiveStatus))
		{
			return false;
		}

		// Living Dead is waiting for its bearer to die, and a heal above zero takes that away - not
		// a wasted cast but the loss of the trigger. Off by default: RSR also fires Living Dead as
		// a last-ditch save, and under that usage the death is not wanted at all. The hold releases
		// itself with enough lead time for a heal to land, after which the normal threshold applies.
		if (target.IsHeldForDeathTrigger())
		{
			return false;
		}

		// A protective status lowers the threshold instead of removing it. "Cannot die right now"
		// is not "does not need healing": Superbolide puts the gunbreaker at 1 HP on purpose, and
		// when the window closes the target stands exactly where it left them. Suppressing the flag
		// outright meant that a protected tank who was the only wounded member got no healing at
		// all, and the two-GCD release before expiry is not enough time to bring one back up from
		// a few percent.
		//
		// Math.Min keeps the invariant that a protected target is never healed more readily than an
		// unprotected one, whatever the two settings are configured to.
		var normal = Lerp(healSingle, healSingleHot, ratio);
		var threshold = target.NoNeedHealingInvuln()
			? normal
			: Math.Min(normal, Service.Config.HealthProtectedRatio);

		return h < threshold;
	}

	private static float Lerp(float a, float b, float ratio)
	{
		return a + ((b - a) * ratio);
	}

	private static AutoStatus StatusFromCmdOrCondition()
	{
		var status = DataCenter.SpecialType switch
		{
			SpecialCommandType.NoCasting => AutoStatus.NoCasting,
			SpecialCommandType.HealArea => AutoStatus.HealAreaSpell
								| AutoStatus.HealAreaAbility,
			SpecialCommandType.HealSingle => AutoStatus.HealSingleSpell
								| AutoStatus.HealSingleAbility,
			SpecialCommandType.DefenseArea => AutoStatus.DefenseArea,
			SpecialCommandType.DefenseSingle => AutoStatus.DefenseSingle,
			SpecialCommandType.DispelStancePositional => AutoStatus.Dispel
								| AutoStatus.TankStance
								| AutoStatus.Positional,
			SpecialCommandType.RaiseShirk => AutoStatus.Raise
								| AutoStatus.Shirk,
			SpecialCommandType.MoveForward => AutoStatus.MoveForward,
			SpecialCommandType.MoveBack => AutoStatus.MoveBack,
			SpecialCommandType.AntiKnockback => AutoStatus.AntiKnockback,
			SpecialCommandType.Burst => AutoStatus.Burst,
			SpecialCommandType.Speed => AutoStatus.Speed,
			SpecialCommandType.Intercepting => AutoStatus.Intercepting,
			_ => AutoStatus.None,
		};


		if (!status.HasFlag(AutoStatus.Burst) && Service.Config.AutoBurst)
		{
			status |= AutoStatus.Burst;
		}

		return status;
	}

	private static int CountAllianceTanks()
	{
		var count = 0;
		foreach (var member in DataCenter.AllianceMembers)
		{
			if (member.IsJobCategory(JobRole.Tank))
			{
				count++;
			}
		}
		return count;
	}

	private static bool AnyAllianceTankWithStance()
	{
		foreach (var member in DataCenter.AllianceMembers)
		{
			if (member.IsJobCategory(JobRole.Tank) && member.CurrentHp != 0 && member.HasStatus(false, StatusHelper.TankStanceStatus))
			{
				return true;
			}
		}
		return false;
	}

	private static bool AnyPartyTankSprinting()
	{
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member.IsJobCategory(JobRole.Tank) && member.HasStatus(false, StatusID.Sprint))
			{
				return true;
			}
		}
		return false;
	}
}