using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Updaters;

internal static partial class TargetUpdater
{
	private static readonly ObjectListDelay<IBattleChara>
		_raisePartyTargets = new(() => Service.Config.RaiseDelay2),
		_raiseAllTargets = new(() => Service.Config.RaiseDelay2),
		_dispelPartyTargets = new(() => Service.Config.EsunaDelay);


	// Scratch lists reused every update; ObjectListDelay copies what it needs.
	private static readonly List<IBattleChara> _raiseCandidates = [];
	private static readonly List<IBattleChara> _dispelCandidates = [];

	private static DateTime _lastUpdateTimeToKill = DateTime.MinValue;
	private static readonly TimeSpan TimeToKillUpdateInterval = TimeSpan.FromSeconds(1);

	internal static void UpdateTargets()
	{
		DataCenter.TargetsByRange.Clear();
		DataCenter.AllTargets = GetAllTargets();

		// Early-out: avoid downstream work and stale data when there are no potential targets
		if (DataCenter.AllTargets == null || DataCenter.AllTargets.Count == 0)
		{
			DataCenter.PartyMembers.Clear();
			DataCenter.AllianceMembers.Clear();
			DataCenter.AllHostileTargets.Clear();
			// Cleared with the rest, or it keeps naming whoever was last under fire. Nothing fails
			// when it is left behind: the set simply reports that member as threatened for as long
			// as the party stays out of combat, and the emergency heal treats them accordingly the
			// next time they drop.
			DataCenter.TargetedPartyMembers.Clear();
			DataCenter.DeathTarget = null;
			DataCenter.DispelTarget = null;
			DataCenter.ProvokeTarget = null;
			DataCenter.InterruptTarget = null;
			UpdateTimeToKill();
			return;
		}

		UpdateLists();
		DataCenter.DeathTarget = GetDeathTarget();
		DataCenter.DispelTarget = GetDispelTarget();
		DataCenter.ProvokeTarget = (DataCenter.Role == JobRole.Tank || StatusHelper.PlayerHasStatus(true, StatusID.VariantUltimatumSet))
			? GetFirstHostileTarget(ObjectHelper.CanProvoke)
			: null; // Calculating this per frame rather than on-demand is actually a fair amount worse
		DataCenter.InterruptTarget = GetFirstHostileTarget(ObjectHelper.CanInterrupt); // Tanks, Melee, RDM, and various phantom and duty actions can interrupt so just deal with it

		if (DataCenter.IsInOccultCrescentOp && Service.Config.ElementalWeaknessTracking)
		{
			UpdateOccultWeaknesses();
		}

		UpdateTimeToKill();
	}

	private static unsafe void UpdateLists()
	{
		var allTargets = DataCenter.AllTargets;
		if (allTargets == null || allTargets.Count == 0)
		{
			// Defensive: ensure lists are empty if no targets
			DataCenter.PartyMembers.Clear();
			DataCenter.AllianceMembers.Clear();
			DataCenter.AllHostileTargets.Clear();
			return;
		}

		// Pre-size lists to reduce reallocations
		List<IBattleChara> partyMembers = new(capacity: allTargets.Count);
		List<IBattleChara> allianceMembers = new(capacity: allTargets.Count);
		List<IBattleChara> hostileTargets = new(capacity: allTargets.Count);

		var raisetype = Service.Config.RaiseType;
		var ignoreInvincibility = DataCenter.IsPvP && Service.Config.IgnorePvPInvincibility;

		// Compute player eye position once
		var playerEye = Player.Object?.Position;
		if (playerEye != null)
		{
			playerEye = new Vector3(playerEye.Value.X, playerEye.Value.Y + 2.0f, playerEye.Value.Z);
		}

		// GetAllTargets already filtered out untargetable objects and pets.
		foreach (var member in allTargets)
		{
			try
			{
				if (member.IsEnemy())
				{
					// Only proceed if playerEye is available
					if (playerEye == null)
					{
						continue;
					}

					if (member.DistanceToPlayer() < 48 && member.CanSeeFrom(playerEye.Value))
					{
						// Valid hostile target
						if (!ignoreInvincibility)
						{
							var hasInvincible = false;
							var statusList = member.StatusList;

							if (statusList != null)
							{
								var statusCount = statusList.Length;
								for (var i = 0; i < statusCount; i++)
								{
									var status = statusList[i];
									if (status != null && status.StatusId != 0 && status.IsInvincible())
									{
										hasInvincible = true;
										break;
									}
								}
							}

							if (hasInvincible)
							{
								continue; // Invincible enemy doesn't get added to any lists
							}
						}
						hostileTargets.Add(member);
					}
				}
				else if (member.IsParty())
				{
					// Party members are only added to the party list
					if (member.Character() != null)
					{
						partyMembers.Add(member);
					}
				}
				else // Not a party member or hostile, so check alliance status
				{
					// Alliance members are based on the raise settings
					if (raisetype == RaiseType.PartyOnly)
					{
						// No alliance member checks
					}
					else if (raisetype == RaiseType.AllOutOfDuty)
					{
						if (member.IsOtherPlayerOutOfDuty() && member.Character() != null)
						{
							allianceMembers.Add(member);
						}
					}
					else if (member.IsAllianceMember())
					{
						var character = member.Character();
						if (character != null)
						{
							allianceMembers.Add(member);
						}
					}
				}
			}
			catch (Exception ex)
			{
				PluginLog.Error($"Error in Updating Member Lists: {ex.Message}");
			}
		}

		DataCenter.PartyMembers = partyMembers;
		DataCenter.AllianceMembers = allianceMembers;
		DataCenter.AllHostileTargets = hostileTargets;

		// Which party members an enemy is pointing at, collected once here rather than asked per
		// member later. The hostile list is built in this loop anyway and each enemy names its
		// target outright, so the cost is one pass over the enemies and the answer becomes a lookup.
		// Asking it the other way round - "is any enemy aiming at this member" - would walk every
		// enemy for every member.
		//
		// Two sources, because they are not the same question. TargetObjectId is who the enemy is
		// attacking, which is aggro. CastTargetObjectId is who the cast in progress will land on,
		// and the two part company exactly where it matters: a boss that keeps hitting the tank
		// while casting something at a caster who holds no aggro and, until it lands, no damage
		// either. Reading only the first would call that caster safe.
		//
		// Filtered against the party, so an entry means what a reader will take it to mean. Enemies
		// point at pets, at other enemies and at nothing at all, and an unfiltered set is therefore
		// never empty - a later reader asking "is anyone under fire" would always get yes.
		HashSet<ulong> targeted = new(capacity: partyIds.Count);
		for (var i = 0; i < hostileTargets.Count; i++)
		{
			var hostile = hostileTargets[i];
			if (hostile == null)
			{
				continue;
			}

			if (partyIds.Contains(hostile.TargetObjectId))
			{
				_ = targeted.Add(hostile.TargetObjectId);
			}

			if (partyIds.Contains(hostile.CastTargetObjectId))
			{
				_ = targeted.Add(hostile.CastTargetObjectId);
			}
		}
		DataCenter.TargetedPartyMembers = targeted;
	}

	private static List<IBattleChara> GetAllTargets()
	{
		List<IBattleChara> allTargets = [];
		var skipDummyCheck = !Service.Config.DisableTargetDummys;

		var objects = Svc.Objects;
		if (objects != null)
		{
			var count = objects.Length;
			for (var i = 0; i < count; i++)
			{
				var obj = objects[i];
				if (obj is IBattleChara battleChara)
				{
					if ((skipDummyCheck || !battleChara.IsDummy())
						&& battleChara.IsTargetable
						&& battleChara.StatusList != null
						&& !battleChara.IsPet())
					{
						allTargets.Add(battleChara);
					}
				}
			}
		}

		return allTargets;
	}

	private static IBattleChara? GetFirstHostileTarget(Func<IBattleChara, bool> predicate)
	{
		var hostileTargets = DataCenter.AllHostileTargets;
		if (hostileTargets == null)
		{
			return null;
		}

		foreach (var target in hostileTargets)
		{
			try
			{
				if (predicate(target))
				{
					return target;
				}
			}
			catch (Exception ex)
			{
				PluginLog.Error($"Error in GetFirstHostileTarget: {ex.Message}");
			}
		}
		return null;
	}

	private static IBattleChara? GetDeathTarget()
	{
		if (!DataCenter.CanRaise())
		{
			return null;
		}

		try
		{
			var raisetype = Service.Config.RaiseType;

			var validRaiseTargets = _raiseCandidates;
			validRaiseTargets.Clear();
			if (DataCenter.PartyMembers != null)
			{
				foreach (var target in DataCenter.PartyMembers.GetDeath())
				{
					if (raisetype != RaiseType.PartyHealersOnly || target.IsJobCategory(JobRole.Healer))
					{
						validRaiseTargets.Add(target);
					}
				}
			}

			// Alliance members never include party members (UpdateLists keeps them disjoint).
			if (DataCenter.AllianceMembers != null)
			{
				if (raisetype == RaiseType.PartyAndAllianceSupports || raisetype == RaiseType.PartyAndAllianceHealers)
				{
					foreach (var member in DataCenter.AllianceMembers.GetDeath())
					{
						if (raisetype == RaiseType.PartyAndAllianceHealers)
						{
							if (member.IsJobCategory(JobRole.Healer))
							{
								validRaiseTargets.Add(member);
							}
						}
						else // PartyAndAllianceSupports
						{
							if (member.IsJobCategory(JobRole.Healer) || member.IsJobCategory(JobRole.Tank))
							{
								validRaiseTargets.Add(member);
							}
						}
					}
				}
				else if (raisetype == RaiseType.All || raisetype == RaiseType.AllOutOfDuty)
				{
					foreach (var target in DataCenter.AllianceMembers.GetDeath())
					{
						validRaiseTargets.Add(target);
					}
				}
			}

			// Apply raise delay without allocating a new list per frame
			if (raisetype == RaiseType.PartyOnly)
			{
				_raisePartyTargets.Delay(validRaiseTargets);
				validRaiseTargets.Clear();
				foreach (var p in _raisePartyTargets)
				{
					validRaiseTargets.Add(p);
				}
			}
			else
			{
				_raiseAllTargets.Delay(validRaiseTargets);
				validRaiseTargets.Clear();
				foreach (var p in _raiseAllTargets)
				{
					validRaiseTargets.Add(p);
				}
			}

			return GetPriorityDeathTarget(validRaiseTargets, raisetype);
		}
		catch (Exception ex)
		{
			PluginLog.Error($"Error in GetDeathTarget: {ex.Message}");
			return null;
		}
	}

	private static IBattleChara? GetPriorityDeathTarget(List<IBattleChara> validRaiseTargets, RaiseType raiseType = RaiseType.PartyOnly)
	{
		if (validRaiseTargets.Count == 0)
		{
			return null;
		}

		List<IBattleChara> deathTanks = [];
		List<IBattleChara> deathHealers = [];
		List<IBattleChara> deathOffHealers = [];
		List<IBattleChara> deathOthers = [];

		foreach (var chara in validRaiseTargets)
		{
			if (chara.IsJobCategory(JobRole.Tank))
			{
				deathTanks.Add(chara);
			}
			else if (chara.IsJobCategory(JobRole.Healer))
			{
				deathHealers.Add(chara);
			}
			else if (Service.Config.OffRaiserRaise && chara.IsJobs(Job.SMN))
			{
				deathOffHealers.Add(chara);
			}
			else if (Service.Config.OffRaiserRaise && chara.IsJobs(Job.RDM))
			{
				deathOffHealers.Add(chara);
			}
			else
			{
				deathOthers.Add(chara);
			}
		}

		if (raiseType == RaiseType.PartyAndAllianceHealers && deathHealers.Count > 0)
		{
			return deathHealers[0];
		}

		if (Service.Config.H2)
		{
			deathTanks.Reverse();
			deathHealers.Reverse();
			deathOffHealers.Reverse();
			deathOthers.Reverse();
		}

		if (deathTanks.Count > 1)
		{
			return deathTanks[0];
		}

		return deathHealers.Count > 0
			? deathHealers[0]
			: deathTanks.Count > 0
			? deathTanks[0]
			: Service.Config.OffRaiserRaise && deathOffHealers.Count > 0
			? deathOffHealers[0]
			: deathOthers.Count > 0 ? deathOthers[0] : null;
	}

	private static IBattleChara? GetDispelTarget()
	{
		if (Player.Job is Job.WHM or Job.SCH or Job.AST or Job.SGE or Job.BRD or Job.CNJ)
		{
			var weakenPeople = _dispelCandidates;
			weakenPeople.Clear();
			AddDispelTargets(DataCenter.PartyMembers, weakenPeople);

			// Apply dispel delay to the candidate list
			_dispelPartyTargets.Delay(weakenPeople);

			const AutoStatus busyStatus = AutoStatus.HealAreaAbility | AutoStatus.HealAreaSpell
				| AutoStatus.HealSingleAbility | AutoStatus.HealSingleSpell
				| AutoStatus.DefenseArea | AutoStatus.DefenseSingle;
			var canDispelNonDangerous = (DataCenter.MergedStatus & busyStatus) == 0;

			// Single-pass selection over the delayed set to avoid extra list allocations
			IBattleChara? closestDangerous = null;
			var closestDangerousDist = float.MaxValue;
			IBattleChara? closestNonDangerous = null;
			var closestNonDangerousDist = float.MaxValue;

			foreach (var person in _dispelPartyTargets)
			{
				var hasDangerous = false;
				var statusList = person.StatusList;
				if (statusList != null)
				{
					for (int i = 0, n = statusList.Length; i < n; i++)
					{
						var status = statusList[i];
						if (status != null && status.IsDangerous())
						{
							hasDangerous = true;
							break;
						}
					}
				}

				var dist = ObjectHelper.DistanceToPlayer(person);
				if (hasDangerous)
				{
					if (dist < closestDangerousDist)
					{
						closestDangerousDist = dist;
						closestDangerous = person;
					}
				}
				else if (dist < closestNonDangerousDist)
				{
					closestNonDangerousDist = dist;
					closestNonDangerous = person;
				}
			}

			var allowNonDangerous = canDispelNonDangerous
									 || !DataCenter.HasHostilesInRange
									 || Service.Config.DispelAll
									 || DataCenter.IsPvP;

			if (!allowNonDangerous)
			{
				return closestDangerous;
			}

			return closestDangerous ?? closestNonDangerous;
		}
		return null;
	}

	private static void AddDispelTargets(List<IBattleChara>? members, List<IBattleChara> targetList)
	{
		if (members == null)
		{
			return;
		}

		foreach (var member in members)
		{
			try
			{
				if (member.StatusList != null)
				{
					for (var i = 0; i < member.StatusList.Length; i++)
					{
						var status = member.StatusList[i];
						if (status != null && status.CanDispel())
						{
							targetList.Add(member);
							break; // Add only once per member if any status can be dispelled
						}
					}
				}
			}
			catch (NullReferenceException ex)
			{
				PluginLog.Error($"NullReferenceException in AddDispelTargets for member {member?.ToString()}: {ex.Message}");
			}
		}
	}

	// Recording new entries at 1/second and dequeuing old values to keep only the last DataCenter.HP_RECORD_TIME worth of combat time
	// Has performance implications for keeping too much data for too many targets as they're also all evaluated multiple times a frame for expected TTK
	private static void UpdateTimeToKill()
	{
		var now = DateTime.Now;
		if (now - _lastUpdateTimeToKill < TimeToKillUpdateInterval)
		{
			return;
		}

		_lastUpdateTimeToKill = now;

		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null || hostiles.Count == 0)
		{
			return;
		}

		if (DataCenter.RecordedHP.Count >= DataCenter.HP_RECORD_TIME)
		{
			_ = DataCenter.RecordedHP.Dequeue();
		}

		// Party members are recorded alongside the hostiles, which is what lets GetTTK answer for
		// them at all. The method never cared who it was asked about - it looks an id up in this
		// history and reads the trend - so the only reason it returned NaN for a healer or a tank was
		// that nobody ever put them in here.
		//
		// What that buys is a damage rate per member that needs no list of any kind: the observed
		// fall is already net of every reduction, mitigation, barrier and foreign heal, including the
		// ones this plugin has no table for. A rising ratio yields NaN from GetTTK, which reads as
		// "no death in sight" - so the trend answers "is the healing keeping up" directly rather
		// than by inference.
		//
		// Cost is one dictionary entry per member at 1 Hz. The read side needed one guard first:
		// ActionTargetInfo.CheckTimeToKill asked this question of friendly targets too and was only
		// ever right because the answer was NaN.
		var party = DataCenter.PartyMembers;

		Dictionary<ulong, float> currentHPs = new(hostiles.Count + party.Count);
		for (var i = 0; i < hostiles.Count; i++)
		{
			var target = hostiles[i];
			if (target != null && target.CurrentHp != 0)
			{
				currentHPs[target.GameObjectId] = target.GetHealthRatio();
			}
		}

		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member != null && member.CurrentHp != 0)
			{
				currentHPs[member.GameObjectId] = member.GetHealthRatio();
			}
		}

		DataCenter.RecordedHP.Enqueue((now, currentHPs));

		// Scoring the estimate against what actually happened. A second ago GetTTK said "this one
		// reaches zero in N seconds"; the sample just enqueued says what the health really did since.
		// Comparing the two needs no outside observer and no play session to report back - it is
		// arithmetic on two numbers the plugin already holds, and it never looks away.
		//
		// What it is for: GetTTK averages the fall over the whole time the member has been damaged,
		// so a tank who has been chipped for a minute and then takes a pack of autos is still
		// reported with the slow old trend. That is the "should fall in 8s, falls in 3s" case, and it
		// is the wrong direction to be wrong in - a rule reading the uncorrected number steps in too
		// late. The bias measures exactly that gap, and GetCorrectedTTK divides it out.
		//
		// Order matters: this runs after the enqueue, so the forecast filed for the next round is the
		// one the freshest history supports.
		for (var i = 0; i < party.Count; i++)
		{
			party[i]?.ScoreTtkForecast();
		}

		ObjectHelper.ForgetStaleTtkForecasts();
	}

	private static void UpdateOccultWeaknesses()
	{
		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null || hostiles.Count == 0)
		{
			return;
		}

		var foundNew = false;

		for (var i = 0; i < hostiles.Count; i++)
		{
			var hostile = hostiles[i];
			if (hostile == null || hostile.NameId == 0)
			{
				continue;
			}

			// Skip mobs whose weakness is already manually curated in StatusHelper.
			if (StatusHelper.HasKnownOccultWeakness(hostile.NameId))
			{
				continue;
			}

			foreach (var weakness in StatusHelper.OccultWeaknessStatuses)
			{
				if (hostile.HasStatus(false, weakness))
				{
					if (OtherConfiguration.RecordOccultWeakness(hostile.NameId, weakness))
					{
						foundNew = true;
					}
				}
			}
		}

		if (foundNew)
		{
			if (DataCenter.IsInNorthHorn)
			{
				_ = OtherConfiguration.SaveNorthHornWeaknessRecords();
			}
			else if (DataCenter.IsInSouthHorn)
			{
				_ = OtherConfiguration.SaveSouthHornWeaknessRecords();
			}
		}
	}
}