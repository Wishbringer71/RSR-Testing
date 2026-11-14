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
	// "Interrupt delay range" and "Provoke delay range": the delay their
	// texts promise, from the moment a target appears until it is chosen. Wired here until upstream's refactor of the
	// target update dropped it (527414d60, 2024) and left both settings without a reader (A338). Not readonly: the
	// struct keeps its timer.
	private static RandomDelay _interruptDelay = new(() => Service.Config.InterruptDelay);
	private static RandomDelay _provokeDelay = new(() => Service.Config.ProvokeDelay);

	// The enemies that qualify for an interrupt or a provoke in this frame, before the delays.
	private static readonly List<IBattleChara> _interruptQualified = [];
	private static readonly List<IBattleChara> _provokeQualified = [];

	private static readonly List<IBattleChara> _raiseCandidates = [];
	private static readonly List<IBattleChara> _dispelCandidates = [];

	private static List<IBattleChara> _outOfDutyPlayers = [];

	private static long _lastUpdateTimeToKill;
	private const long TimeToKillUpdateIntervalMs = 1000;

	internal static void UpdateTargets()
	{
		// Damage reported on the player and not yet taken, every frame, so its waits are measured in and out of combat (A617).
		ReportedDamage.Update();
		DataCenter.TargetsByRange.Clear();
		DataCenter.AllTargets = GetAllTargets();

		if (DataCenter.AllTargets == null || DataCenter.AllTargets.Count == 0)
		{
			DataCenter.PartyMembers.Clear();
			DataCenter.AllianceMembers.Clear();
			DataCenter.AllHostileTargets.Clear();
			HealToDefeat.Update();
			HostileMovement.Clear();
			UpdateAreaCasters();
			DataCenter.UpdateTankbusterTargets();
			DataCenter.RaisedByOthers = [];
			DataCenter.EsunasByOthers = [];
			// Cleared with the rest, or it keeps naming whoever was last under fire. Nothing fails
			// when it is left behind: the set simply reports that member as threatened for as long
			// as the party stays out of combat, and the emergency heal treats them accordingly the
			// next time they drop.
			DataCenter.TargetedPartyMembers.Clear();
			DataCenter.AllHostileTargetIds.Clear();
			_outOfDutyPlayers.Clear();
			DataCenter.DeathTarget = null;
			DataCenter.DispelTarget = null;
			// With no party list there is nothing to follow, and a status seen again afterwards is seen late (A432).
			DotOnParty.Clear();
			ActionPenaltyWatch.Clear();
			TankbusterTable.ClearCasts();
			EnemyDanger.Clear();
			DataCenter.LastStrikeByEnemy.Clear();
			// Through the delays, so their timers start again with the next target instead of carrying this one over.
			_ = _provokeDelay.Delay(false);
			_ = _interruptDelay.Delay(false);
			DataCenter.ProvokeCandidates.Clear();
			DataCenter.InterruptCandidates.Clear();
			_provokeTurns.Clear();
			DataCenter.ProvokeTarget = null;
			DataCenter.InterruptTarget = null;
			UpdateTimeToKill();
			return;
		}

		UpdateLists();
		// Figures only healing defeats, after the party list they are told apart from (A516).
		HealToDefeat.Update();
		UpdateAreaCasters();
		// Whom a tankbuster marker names, after the party list it is matched against (A328).
		DataCenter.UpdateTankbusterTargets();
		UpdateCastsByOthers();
		DataCenter.DeathTarget = GetDeathTarget();
		TraceDutyOnlyRaise();
		DataCenter.DispelTarget = GetDispelTarget();
		// Enemy damage over time on the party, after the party list and the Esuna target it leaves out (A432).
		DotOnParty.Update();
		// Casts that will lay a status damaging every action on the player (A439).
		ActionPenaltyWatch.Update();
		// Every enemy cast but a raidwide's, so the hits of a tankbuster can be told as its cast's landing (A452, A454).
		TankbusterTable.UpdateCasts();
		// "Provoke when there is another tank in the party" covers the cases its text names; the party
		// walk behind it runs only when one of them comes up.
		bool? autoProvokeAllowed = null;
		bool AutoProvokeAllowed() => autoProvokeAllowed ??= Service.Config.AutoProvokeForTank || StateUpdater.CountPartyTanks() < 2;
		_provokeQualified.Clear();
		if (DataCenter.Role == JobRole.Tank || StatusHelper.PlayerHasStatus(true, StatusID.VariantUltimatumSet))
		{
			// Calculating this per frame rather than on-demand is actually a fair amount worse
			CollectHostileTargets(_provokeQualified, target => target.CanProvoke(AutoProvokeAllowed));
		}

		// Every qualifying enemy, not the first: each action takes the first among its own candidates (concept 03,
		// "Unterbrechen, Betäuben, Provozieren: welcher Gegner", A612). The delay counts from the first one, as before, and
		// never outlasts the point where the provoke could come too late (his rule, A500): taking the enemy from a co-tank
		// - in danger, or ahead of a tankbuster - does not wait at all, and an enemy that turned to another member waits
		// only until its first hit on him.
		FillReady(DataCenter.ProvokeCandidates, _provokeQualified, _provokeDelay.Delay(_provokeQualified.Count > 0), ProvokeCannotWait);
		ForgetProvokeTurns();
		DataCenter.ProvokeTarget = DataCenter.ProvokeCandidates.Count > 0 ? DataCenter.ProvokeCandidates[0] : null;

		// Tanks, Melee, RDM, and various phantom and duty actions can interrupt so just deal with it. The delay never outlasts
		// the cast: with one GCD of it left the interrupt goes now, since one that waits past the cast's end is no interrupt.
		_interruptQualified.Clear();
		CollectHostileTargets(_interruptQualified, ObjectHelper.CanInterrupt);
		FillReady(DataCenter.InterruptCandidates, _interruptQualified, _interruptDelay.Delay(_interruptQualified.Count > 0),
			caster => caster.TotalCastTime - caster.CurrentCastTime <= DataCenter.DefaultGCDTotal);
		DataCenter.InterruptTarget = DataCenter.InterruptCandidates.Count > 0 ? DataCenter.InterruptCandidates[0] : null;

		if (DataCenter.IsInOccultCrescentOp && Service.Config.ElementalWeaknessTracking)
		{
			UpdateOccultWeaknesses();
		}

		UpdateTimeToKill();
	}

	private static unsafe void UpdateLists()
	{
		var allTargets = DataCenter.AllTargets;

		List<IBattleChara> partyMembers = new(capacity: allTargets.Count);
		List<IBattleChara> allianceMembers = new(capacity: allTargets.Count);
		List<IBattleChara> hostileTargets = new(capacity: allTargets.Count);
		List<IBattleChara> spikedHostiles = [];
		List<IBattleChara> parriedHostiles = [];
		List<IBattleChara> untouchableHostiles = [];
		var stopTargets = Service.Config.FilterStopMark2 ? MarkingHelper.GetStopTargets() : [];
		List<IBattleChara> outOfDutyPlayers = [];

		var raisetype = Service.Config.RaiseType;
		var ignoreInvincibility = DataCenter.IsPvP && Service.Config.IgnorePvPInvincibility;

		var playerEye = Player.Object?.Position;
		if (playerEye != null)
		{
			playerEye = new Vector3(playerEye.Value.X, playerEye.Value.Y + 2.0f, playerEye.Value.Z);
		}

		// Concept 03, "Tötungsregeln" (A588): which enemies a kill rule holds back, from every battle character - a held
		// one leaves the list below, so the rules cannot read it.
		KillOrder.Update(allTargets);

		foreach (var member in allTargets)
		{
			try
			{
				if (member.IsEnemy())
				{
					if (playerEye == null)
					{
						continue;
					}

					if (member.DistanceToPlayer() < 48 && member.CanSeeFrom(playerEye.Value))
					{
						// Concept 03, "Gegner, die keine Fläche treffen darf" (A572): what no attack may hit, area ones
						// included - whether or not it is a target.
						if (member.MustNotBeHit(stopTargets))
						{
							untouchableHostiles.Add(member);
						}

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
								continue;
							}
						}

						// Concept 03, "Gegner mit Stacheln" (A571): spikes that damage attackers keep an enemy out of the
						// list of targets; only an action that removes them reaches it.
						if (member.HasSpikes())
						{
							spikedHostiles.Add(member);
							TraceSpiked(member);
							continue;
						}

						if (member.IsGuardedZuEgg() || KillOrder.IsHeld(member))
						{
							continue;
						}

						// Concept 03, "Gegner, die deine Seite pariert" (A734): a hit from a side its Directional Parry guards
						// deals nothing; it waits while another enemy can be attacked.
						if (member.ParriesPlayerSide())
						{
							parriedHostiles.Add(member);
							continue;
						}

						hostileTargets.Add(member);
					}
				}
				else if (member.IsParty())
				{
					if (member.Character() != null)
					{
						partyMembers.Add(member);
					}
				}
				else if (member.Character() != null)
				{
					if (member.IsAllianceMember())
					{
						allianceMembers.Add(member);
					}
					else if (raisetype == RaiseType.AllOutOfDuty && member.IsOtherPlayerOutOfDuty())
					{
						outOfDutyPlayers.Add(member);
					}
				}
			}
			catch (Exception ex)
			{
				PluginLog.Error($"Error in Updating Member Lists: {ex.Message}");
			}
		}

		// A734: with no other enemy to attack, the one parrying your side stays a target - your familiar's or companion's
		// attacks reach it, and holding back gains nothing.
		if (hostileTargets.Count == 0)
		{
			hostileTargets.AddRange(parriedHostiles);
		}
		else
		{
			TraceParried(parriedHostiles);
		}

		DataCenter.PartyMembers = partyMembers;
		DataCenter.AllianceMembers = allianceMembers;
		DataCenter.AllHostileTargets = hostileTargets;
		DataCenter.SpikedHostiles = spikedHostiles;
		DataCenter.UntouchableHostiles = untouchableHostiles;
		if (spikedHostiles.Count == 0)
		{
			_spikedTraced.Clear();
		}
		// Which of them moved since the last update, and which came into combat, for the damage areas (A350, A353).
		HostileMovement.Update(hostileTargets);

		// Which of them run to another enemy, and when each arrives (concept 21, K3, A660).
		Approach.Update(hostileTargets);

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
		// Built here from the finished party list rather than filled inside the loop above. It used
		// to be filled there, next to the alliance checks that needed the same lookup - upstream has
		// since dropped those, the variable went with them, and this reader was left naming something
		// that no longer existed. One pass over at most eight members costs nothing and does not
		// depend on how that loop is written next time.
		HashSet<ulong> partyIds = new(capacity: partyMembers.Count);
		for (var i = 0; i < partyMembers.Count; i++)
		{
			_ = partyIds.Add(partyMembers[i].GameObjectId);
		}

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
		_outOfDutyPlayers = outOfDutyPlayers;

		DataCenter.AllHostileTargetIds.Clear();
		foreach (var hostile in hostileTargets)
		{
			_ = DataCenter.AllHostileTargetIds.Add(hostile.GameObjectId);
		}

		// A674: which enemies with a danger record still stand in the list, so a fallen one's rate ends with it.
		EnemyDanger.Seen(hostileTargets);
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
						// What "StatusList != null" stood here for; Dalamud's getter never returns null, it
						// calls the native object and throws when the object has no status manager.
						&& battleChara.HasNativeState()
						&& !battleChara.IsPet())
					{
						allTargets.Add(battleChara);
					}
				}
			}
		}

		return allTargets;
	}

	// Every hostile target that satisfies the predicate, in the order of the hostile list.
	private static void CollectHostileTargets(List<IBattleChara> into, Func<IBattleChara, bool> predicate)
	{
		var hostileTargets = DataCenter.AllHostileTargets;
		if (hostileTargets == null)
		{
			return;
		}

		foreach (var target in hostileTargets)
		{
			try
			{
				if (predicate(target))
				{
					into.Add(target);
				}
			}
			catch (Exception ex)
			{
				PluginLog.Error($"Error in CollectHostileTargets: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Concept 11, "Tote, die nur die Wiederbelebung des Inhalts aufhebt" (A616): one line per change in DefenseTrace.log -
	/// the corpse RSR picked is one only the duty's own raises bring back, and the mana kept for the job's raise is free here.
	/// </summary>
	private static void TraceDutyOnlyRaise()
	{
		var corpse = DataCenter.DeathTarget;
		var id = corpse != null && corpse.OnlyDutyRaisesReach() ? corpse.GameObjectId : 0;
		if (id != _dutyOnlyCorpse)
		{
			_dutyOnlyCorpse = id;
			if (corpse != null && id != 0)
			{
				DefenseTrace.Line($"raise: the dead {(Job)corpse.ClassJob.RowId} comes back only by the duty's own raises; the job's raise and the feather leave them");
			}
		}

		var restricted = ObjectHelper.DutyRestrictsResurrection;
		if (restricted != _dutyRestrictionSeen)
		{
			_dutyRestrictionSeen = restricted;
			DefenseTrace.Line(restricted
				? "raise: only the duty's own raises bring anyone back here; the mana kept for the job's raise is free"
				: "raise: the job's raise works here again");
		}
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
						else
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

			if (raisetype == RaiseType.AllOutOfDuty)
			{
				foreach (var target in _outOfDutyPlayers.GetDeath())
				{
					validRaiseTargets.Add(target);
				}
			}

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

		// H2 as its option text states it: "Raise non-Healers from bottom of party list to the top".
		// Healers keep the list order; the tank, off-healer and remaining lists turn. It used to
		// turn the healer list too, against the text - the owner's rule: the UI settings bind.
		if (Service.Config.H2)
		{
			deathTanks.Reverse();
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

	private static readonly List<IBattleChara> _urgentDispel = [];

	private static IBattleChara? GetDispelTarget()
	{
		if (Player.Job is Job.WHM or Job.SCH or Job.AST or Job.SGE or Job.BRD or Job.CNJ)
		{
			var weakenPeople = _dispelCandidates;
			weakenPeople.Clear();
			AddDispelTargets(DataCenter.PartyMembers, weakenPeople);

			_dispelPartyTargets.Delay(weakenPeople);

			// The delay never outlasts the point where the cleanse could come too late (his rule, A500): a dangerous status
			// that runs out within a GCD - Doom and its kind - is cleansed at once, as an interrupt goes with one GCD left.
			var dispelNow = _urgentDispel;
			dispelNow.Clear();
			foreach (var person in _dispelPartyTargets)
			{
				dispelNow.Add(person);
			}

			foreach (var person in weakenPeople)
			{
				if (!dispelNow.Contains(person) && DangerousStatusEndsWithin(person, DataCenter.DefaultGCDTotal))
				{
					dispelNow.Add(person);
				}
			}

			const AutoStatus busyStatus = AutoStatus.HealAreaAbility | AutoStatus.HealAreaSpell
				| AutoStatus.HealSingleAbility | AutoStatus.HealSingleSpell
				| AutoStatus.DefenseArea | AutoStatus.DefenseSingle;
			var canDispelNonDangerous = (DataCenter.MergedStatus & busyStatus) == 0;

			// A healer's cleanse is a GCD with MP against a heal (concept 07, "Reinigung oder Heilung", A565).
			if (DataCenter.Role == JobRole.Healer)
			{
				return HealerDispelTarget(dispelNow, canDispelNonDangerous);
			}

			IBattleChara? closestDangerous = null;
			var closestDangerousDist = float.MaxValue;
			IBattleChara? closestNonDangerous = null;
			var closestNonDangerousDist = float.MaxValue;

			foreach (var person in dispelNow)
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

	private static readonly Dictionary<uint, int> _dotBearers = [];

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
					// Each Esuna another player is already casting on him removes one of these; only what is
					// left after them is ours to take (A293).
					var leftToOthers = DataCenter.EsunasByOthers.GetValueOrDefault(member.GameObjectId);
					for (var i = 0; i < member.StatusList.Length; i++)
					{
						var status = member.StatusList[i];
						if (status != null && status.CanDispel() && leftToOthers-- <= 0)
						{
							targetList.Add(member);
							break;
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

	private static void UpdateTimeToKill()
	{
		var now = Environment.TickCount64;
		if (now - _lastUpdateTimeToKill < TimeToKillUpdateIntervalMs)
		{
			return;
		}

		_lastUpdateTimeToKill = now;
		TimeToKillTracker.Record(DataCenter.AllHostileTargets, now);

		// The party's own history (A91, A341): upstream's tracker above keeps hostiles only. Recorded while the fight runs
		// as well as while enemies stand in the list: with no enemy to target - a boss gone for a transition - the damage
		// goes on, and without samples every forecast was stale after three seconds (A727).
		var hostiles = DataCenter.AllHostileTargets;
		var enemies = hostiles?.Count ?? 0;
		if (enemies == 0 && !DataCenter.InCombat)
		{
			return;
		}

		if (DataCenter.RecordedHP.Count >= DataCenter.HP_RECORD_TIME)
		{
			_ = DataCenter.RecordedHP.Dequeue();
		}

		// Party members are recorded here, which is what lets GetTTK answer for them at all. The
		// method never cared who it was asked about - it looks an id up in this history and reads the
		// trend - so the only reason it returned NaN for a healer or a tank was that nobody ever put
		// them in here.
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

		Dictionary<ulong, float> currentHPs = new(party.Count);
		DataCenter.RecordedPartyIds.Clear();
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member != null && member.CurrentHp != 0)
			{
				currentHPs[member.GameObjectId] = member.GetHealthRatio();
				_ = DataCenter.RecordedPartyIds.Add(member.GameObjectId);
			}
		}

		DataCenter.RecordedHP.Enqueue((DateTime.Now, currentHPs));
		// Answers given earlier in this frame read the history without this sample.
		ObjectHelper.ForgetTimeToKill();

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

		// Every hostile as well: the rules that ask when an enemy dies read the same corrected forecast - the last
		// weaponskill on a target, a Scholar's Bio (A463). Their rate is upstream's tracker above, a 30 s fit that lags
		// behind a burst, which is the gap the bias measures. Scored for the party alone, no forecast was ever filed
		// for an enemy, and GetCorrectedTTK answered NaN for every one.
		for (var i = 0; i < enemies; i++)
		{
			hostiles![i]?.ScoreTtkForecast();
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

	private static readonly HashSet<ulong> _spikedTraced = [];

	// A571: one line per enemy left alone for its spikes, until no spiked enemy is in reach.
	// A734: one line when an enemy starts or stops being passed over for parrying your side.
	private static readonly HashSet<ulong> _parriedTraced = [];
	private static readonly HashSet<ulong> _parriedNow = [];

}