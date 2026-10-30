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

	private static readonly List<IBattleChara> _raiseCandidates = [];
	private static readonly List<IBattleChara> _dispelCandidates = [];

	private static List<IBattleChara> _outOfDutyPlayers = [];

	private static long _lastUpdateTimeToKill;
	private const long TimeToKillUpdateIntervalMs = 1000;

	internal static void UpdateTargets()
	{
		DataCenter.TargetsByRange.Clear();
		DataCenter.AllTargets = GetAllTargets();

		if (DataCenter.AllTargets == null || DataCenter.AllTargets.Count == 0)
		{
			DataCenter.PartyMembers.Clear();
			DataCenter.AllianceMembers.Clear();
			DataCenter.AllHostileTargets.Clear();
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
			// Through the delays, so their timers start again with the next target instead of carrying this one over.
			DataCenter.ProvokeTarget = _provokeDelay.Delay<IBattleChara>(null);
			DataCenter.InterruptTarget = _interruptDelay.Delay<IBattleChara>(null);
			UpdateTimeToKill();
			return;
		}

		UpdateLists();
		UpdateAreaCasters();
		// Whom a tankbuster marker names, after the party list it is matched against (A328).
		DataCenter.UpdateTankbusterTargets();
		UpdateCastsByOthers();
		DataCenter.DeathTarget = GetDeathTarget();
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
		DataCenter.ProvokeTarget = _provokeDelay.Delay((DataCenter.Role == JobRole.Tank || StatusHelper.PlayerHasStatus(true, StatusID.VariantUltimatumSet))
			? GetFirstHostileTarget(target => target.CanProvoke(AutoProvokeAllowed))
			: null); // Calculating this per frame rather than on-demand is actually a fair amount worse
		// Tanks, Melee, RDM, and various phantom and duty actions can interrupt so just deal with it. The delay never outlasts
		// the cast: with one GCD of it left the interrupt goes now, since one that waits past the cast's end is no interrupt.
		var interruptCandidate = GetFirstHostileTarget(ObjectHelper.CanInterrupt);
		DataCenter.InterruptTarget = _interruptDelay.Delay(interruptCandidate)
			?? (interruptCandidate is { } caster && caster.TotalCastTime - caster.CurrentCastTime <= DataCenter.DefaultGCDTotal
				? interruptCandidate : null);

		if (DataCenter.IsInOccultCrescentOp && Service.Config.ElementalWeaknessTracking)
		{
			UpdateOccultWeaknesses();
		}

		UpdateTimeToKill();
	}

	/// <summary>
	/// The enemies whose casts the cast recognition reads: the hostile list, and every untargetable enemy casting an
	/// action on the area list (A277), on the tankbuster or knockback list, or a tankbuster the table has measured
	/// (A303, <c>DataCenter.IsListedHostileCast</c>).
	/// </summary>
	/// <remarks>
	/// Many raidwides are cast by invisible helpers - Shinryu's Meteor Impact by five light spheres, 5 s cast, each
	/// hitting the whole party (trace of 13.10.2026) - and a boss off the field casts its own. The hostile list holds
	/// targetable enemies only (<see cref="GetAllTargets"/>), so none of these opened a defence. The list is the
	/// evidence of hostility here: an area id enters it only after it damaged every party member, a knockback only
	/// after it knocked someone, a tankbuster by the shipped list or a measured hit, and an untargetable caster
	/// carries no other. Tankbusters and knockbacks of helpers went unread until A303. Range and line of sight are left to the reach check of each cast,
	/// because a helper stands where its area is, not where an enemy can be attacked. A helper is any untargetable
	/// figure of the enemy side, of each of its kinds - combatant, part of an enemy, helper, or none
	/// (ObjectHelper.IsEnemySideFigure); read as combatants only, a dungeon boss's parts cast and hit unread (A327), and
	/// without the helper kind the helpers of trials and raids (A334).
	/// </remarks>
	private static void UpdateAreaCasters()
	{
		var hostiles = DataCenter.AllHostileTargets;
		List<IBattleChara> casters = new(capacity: hostiles.Count);
		casters.AddRange(hostiles);
		HashSet<ulong> helperIds = [];
		var longestHelperCast = 0f;

		foreach (var obj in Svc.Objects)
		{
			if (obj is not IBattleChara caster || caster.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc
				|| caster.IsTargetable || !caster.HasNativeState() || !caster.IsCasting
				|| !caster.IsEnemySideFigure())
			{
				continue;
			}

			// Every helper's cast, listed or not: the tankbuster marker watch waits for the casts that run while a
			// marker stands, and a helper casting the hit after it is the common case, its action not yet listed.
			longestHelperCast = Math.Max(longestHelperCast, caster.TotalCastTime - caster.CurrentCastTime);

			if (!DataCenter.IsListedHostileCast(caster.CastActionId))
			{
				continue;
			}

			if (helperIds.Add(caster.GameObjectId))
			{
				casters.Add(caster);
			}
		}

		DataCenter.AreaCastHelperIds = helperIds;
		DataCenter.AreaCasters = casters;
		DataCenter.LongestHelperCastRemaining = longestHelperCast;
	}

	/// <summary>
	/// Whom other players are casting a raise or an Esuna on right now (A293).
	/// </summary>
	/// <remarks>
	/// A raise shows on its target as <c>StatusID.Raise</c> only once it lands. During another healer's
	/// eight-second hard cast the corpse was still a candidate, and RSR without Swiftcast started a raise of its
	/// own on it: two casts for one corpse, and whichever finished second was lost (his report of 15.10.2026).
	/// The other player's cast and its target are on screen the whole time; BossModReborn reads them the same way
	/// to leave a member to another healer's Esuna (<c>PartyHealth.Update</c>). A raise is recognised by game data,
	/// as the player's own (<c>ActionBasicInfo.RaiseAction</c>: <c>DeadTargetBehaviour</c> 1), so every job's and
	/// every duty's raise counts. A cast that is broken off is gone at the next update and its target is a
	/// candidate again; a raise that lands is then excluded by its status. Kept per body is the raise that
	/// lands first, as a point in time, so a reader in a later frame does not read an aged remainder.
	/// </remarks>
	private static void UpdateCastsByOthers()
	{
		Dictionary<ulong, (string Caster, DateTime LandsAt)> raised = [];
		var now = DateTime.Now;
		Dictionary<ulong, int> esunas = [];
		var playerId = Player.Object?.GameObjectId ?? 0;
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.Action>();

		foreach (var chara in DataCenter.AllTargets)
		{
			if (chara == null || chara.GameObjectId == playerId || chara.IsDead || !chara.IsCasting
				|| chara.CastActionType != (byte)FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action)
			{
				continue;
			}

			var targetId = chara.CastTargetObjectId;
			if (targetId == 0 || targetId == chara.GameObjectId)
			{
				continue;
			}

			if (chara.CastActionId == (uint)ActionID.EsunaPvE)
			{
				esunas[targetId] = esunas.GetValueOrDefault(targetId) + 1;
			}
			else if (sheet.GetRowOrDefault(chara.CastActionId) is { DeadTargetBehaviour: 1 })
			{
				var left = Math.Max(0f, chara.TotalCastTime - chara.CurrentCastTime);
				var landsAt = now.AddSeconds(left);
				var seen = raised.TryGetValue(targetId, out var known);
				if (seen && known.LandsAt <= landsAt)
				{
					continue;
				}

				raised[targetId] = (chara.Name.TextValue, landsAt);
				if (!seen && !DataCenter.RaisedByOthers.ContainsKey(targetId))
				{
					DefenseTrace.Line($"raise left to {chara.Name.TextValue}: casting it on "
						+ $"{Svc.Objects.SearchById(targetId)?.Name.TextValue ?? "?"}, {left:F1} s left");
				}
			}
		}

		DataCenter.RaisedByOthers = raised;
		DataCenter.EsunasByOthers = esunas;
	}

	private static unsafe void UpdateLists()
	{
		var allTargets = DataCenter.AllTargets;

		List<IBattleChara> partyMembers = new(capacity: allTargets.Count);
		List<IBattleChara> allianceMembers = new(capacity: allTargets.Count);
		List<IBattleChara> hostileTargets = new(capacity: allTargets.Count);
		List<IBattleChara> outOfDutyPlayers = [];

		var raisetype = Service.Config.RaiseType;
		var ignoreInvincibility = DataCenter.IsPvP && Service.Config.IgnorePvPInvincibility;

		var playerEye = Player.Object?.Position;
		if (playerEye != null)
		{
			playerEye = new Vector3(playerEye.Value.X, playerEye.Value.Y + 2.0f, playerEye.Value.Z);
		}

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

		DataCenter.PartyMembers = partyMembers;
		DataCenter.AllianceMembers = allianceMembers;
		DataCenter.AllHostileTargets = hostileTargets;
		// Which of them moved since the last update, and which came into combat, for the damage areas (A350, A353).
		HostileMovement.Update(hostileTargets);

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

	private static IBattleChara? GetDispelTarget()
	{
		if (Player.Job is Job.WHM or Job.SCH or Job.AST or Job.SGE or Job.BRD or Job.CNJ)
		{
			var weakenPeople = _dispelCandidates;
			weakenPeople.Clear();
			AddDispelTargets(DataCenter.PartyMembers, weakenPeople);

			_dispelPartyTargets.Delay(weakenPeople);

			const AutoStatus busyStatus = AutoStatus.HealAreaAbility | AutoStatus.HealAreaSpell
				| AutoStatus.HealSingleAbility | AutoStatus.HealSingleSpell
				| AutoStatus.DefenseArea | AutoStatus.DefenseSingle;
			var canDispelNonDangerous = (DataCenter.MergedStatus & busyStatus) == 0;

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

		// The party's own history (A91, A341): upstream's tracker above keeps hostiles only.
		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null || hostiles.Count == 0)
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
		for (var i = 0; i < hostiles.Count; i++)
		{
			hostiles[i]?.ScoreTtkForecast();
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
}