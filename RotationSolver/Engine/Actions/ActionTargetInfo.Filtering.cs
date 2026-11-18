using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using static RotationSolver.Configuration.ConfigTypes;
using AttackType = RotationSolver.Engine.Data.AttackType;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Logging;
using RotationSolver.Rotations.Ranged.Dancer;
using Player = ECommons.GameHelpers.Player;

namespace RotationSolver.Engine.Actions;

public partial struct ActionTargetInfo
{
	/// <summary>Whether this action grants its target a heal over time (<see cref="StatusHelper.IsAnyHealOverTime"/>).</summary>
	private readonly bool GrantsSingleHot()
	{
		var provides = action.Setting.TargetStatusProvide;
		if (provides == null)
		{
			return false;
		}

		foreach (var status in provides)
		{
			if (StatusHelper.IsAnyHealOverTime(status))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether the player stands at <paramref name="target"/>: 0 yalms hitbox to hitbox, the distance
	/// the game shows. A gap closer onto such a target does not move the player.
	/// </summary>
	internal static bool StandsAtTarget(IBattleChara target) => target.DistanceToPlayer() <= 0;

	/// <summary>
	/// Records why the movement safety check withheld this action, for the diagnostics window, and
	/// answers <c>false</c>.
	/// </summary>
	private readonly bool Refused(string why, bool measured = true)
	{
		// Kept apart: a refusal without a target can repeat every frame (FindTargetAreaMove, TODO) and
		// would otherwise hide the refusal of a dash that was actually measured.
		var refusal = new DataCenter.MoveSafetyRefusal(action.Name, why, DateTime.Now);
		if (measured)
		{
			DataCenter.LastMoveSafetyRefusal = refusal;
		}
		else
		{
			DataCenter.LastMoveSafetyUnmeasured = refusal;
		}

		return false;
	}

	/// <summary>
	/// Whether a hostile stands within <paramref name="range"/> of the player and in the cone before him, as the targeting
	/// of a cone cast from the player counts it - for a running channel that still reaches someone (A380).
	/// </summary>
	internal static bool HostileInFacingCone(float range)
	{
		if (Player.Object is not { } player)
		{
			return false;
		}

		var faceVec = Vector3.Normalize(player.GetFaceVector());
		faceVec.Y = 0;
		var hostiles = DataCenter.AllHostileTargets;
		for (var i = 0; i < hostiles.Count; i++)
		{
			if (hostiles[i] is not { } h || h.DistanceToPlayer() > range)
			{
				continue;
			}

			var toTarget = h.Position - player.Position;
			toTarget.Y = 0;
			var dist = toTarget.Length();
			if (dist == 0 || InFacingCone(faceVec, toTarget / dist))
			{
				return true;
			}
		}

		return false;
	}

	// hostileOrder: an order of its own for the hostile choice, ahead of the targeting type; null for none.
	// Only DotSpreadOrder gives one.
	private static IBattleChara? FindTargetByType(IEnumerable<IBattleChara> battleChara, TargetType type, float healRatio, SpecialActionType actionType, TargetType targetOverride, bool isFriendly, bool instantHeal, TargetingType? commandTargeting, Comparison<IBattleChara>? hostileOrder)
	{
		if (battleChara == null)
		{
			return null;
		}

		if (Player.Object == null)
		{
			return null;
		}

		if (type == TargetType.Self)
		{
			return Player.Object;
		}

		if (targetOverride == TargetType.Self)
		{
			return Player.Object;
		}

		switch (actionType)
		{
			case SpecialActionType.MeleeRangedAttack:
				{
					var filtered = new List<IBattleChara>();
					foreach (var t in battleChara)
					{
						if (t.DistanceToPlayer() >= 3f + (Service.Config?.MeleeRangedOffset ?? 0f))
						{
							filtered.Add(t);
						}
					}
					battleChara = filtered;
				}
				break;

			case SpecialActionType.HostileMovingForward:
				if (Service.Config != null)
				{
					if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveForward) || Service.CountDownTime > 0)
					{
						if (Svc.Targets.Target is IBattleChara IsHostiletarg && IsHostiletarg.IsHostile())
						{
							return IsHostiletarg;
						}
					}
					if (DataCenter.MergedStatus.HasFlag(AutoStatus.Intercepting))
					{
						if (Svc.Targets.Target is IBattleChara IsHostiletarg && IsHostiletarg.IsHostile())
						{
							return IsHostiletarg;
						}
					}
					else
					{
						var filtered = new List<IBattleChara>();
						foreach (var t in battleChara)
						{
							// Within the distance, the bound included, as GapCloserAllowed (A528).
							if (t.IsHostile() && t.DistanceToPlayer() <= Service.Config.DistanceForMoving2)
							{
								filtered.Add(t);
							}
						}
						battleChara = filtered;
					}
				}
				break;

			case SpecialActionType.FriendlyMovingForward:
				if (Svc.Targets.FocusTarget != null)
				{
					if (Svc.Targets.FocusTarget is IBattleChara focus && focus.IsParty())
					{
						return focus;
					}
				}
				else if (Svc.Targets.FocusTarget == null && Svc.Targets.Target != null)
				{
					if (Svc.Targets.Target is IBattleChara IsPartytarg && IsPartytarg.IsParty())
					{
						return IsPartytarg;
					}
				}
				break;

			case SpecialActionType.HostileFriendlyMovingForward:
				if (Svc.Targets.FocusTarget != null)
				{
					if (Svc.Targets.FocusTarget is IBattleChara focus && focus.IsParty())
					{
						return focus;
					}
				}
				else if (Svc.Targets.FocusTarget == null && Svc.Targets.Target != null)
				{
					if (Svc.Targets.Target is IBattleChara IsPartytarg && IsPartytarg.IsParty())
					{
						return IsPartytarg;
					}
					if (Svc.Targets.Target is IBattleChara IsHostiletarg && IsHostiletarg.IsHostile())
					{
						return IsHostiletarg;
					}
				}
				break;

			case SpecialActionType.HostileMovingAttack:
			case SpecialActionType.HostileAttackBackstep:
				{
					var filtered = new List<IBattleChara>();
					foreach (var t in battleChara)
					{
						if (ObjectHelper.IsAlive(t))
						{
							filtered.Add(t);
						}
					}
					battleChara = filtered;
				}
				break;

			case SpecialActionType.FixedDistanceMoveForward:
				return Player.Object;

			case SpecialActionType.FixedDistanceMoveBackward:
				return Player.Object;
		}

		var effectiveType = commandTargeting.HasValue ? ToTargetType(commandTargeting.Value)
			: targetOverride == default ? type : targetOverride;

		{
			switch (effectiveType)
			{
				case TargetType.Death:
					if (DataCenter.DeathTarget != null)
					{
						return DataCenter.DeathTarget;
					}

					battleChara = battleChara.GetDeath();
					break;

				case TargetType.Move:
					break;

				case TargetType.FriendMove:
					{
						if (Svc.Targets.FocusTarget != null)
						{
							if (Svc.Targets.FocusTarget is IBattleChara focus && focus.IsParty())
							{
								return focus;
							}
						}
						else if (Svc.Targets.Target != null)
						{
							if (Svc.Targets.Target is IBattleChara targ && targ.IsParty())
							{
								return targ;
							}
						}
					}
					break;

				default:
					{
						var filtered = new List<IBattleChara>();
						foreach (var t in battleChara)
						{
							if (ObjectHelper.IsAlive(t))
							{
								filtered.Add(t);
							}
						}
						battleChara = filtered;
					}
					break;
			}

			return effectiveType switch
			{
				TargetType.BeAttacked => FindBeAttackedTarget(),
				TargetType.Provoke => FindProvokeTarget(),
				TargetType.TankSwap => FindTankSwapTarget(),
				TargetType.Dispel => FindDispelTarget(),
				TargetType.HotAgainstDot => FindHotAgainstDotTarget(),
				TargetType.Move => FindTargetForMoving(),
				TargetType.Heal => FindHealTarget(healRatio),
				TargetType.Interrupt => FindInterruptTarget(),
				TargetType.Tank => FindTankTarget(),
				TargetType.Tankbuster => FindTankbusterTarget(),
				TargetType.Melee => battleChara != null ? RandomMeleeTarget(battleChara) : null,
				TargetType.Range => battleChara != null ? RandomRangeTarget(battleChara) : null,
				TargetType.Magical => battleChara != null ? RandomMagicalTarget(battleChara) : null,
				TargetType.Physical => battleChara != null ? RandomPhysicalTarget(battleChara) : null,
				TargetType.DarkCannon => FindDarkCannonTarget(),
				TargetType.ShockCannon => FindShockCannonTarget(),
				TargetType.PhantomBell => FindPhantomBell(),
				TargetType.PhantomRespite => FindPhantomRespite(),
				TargetType.DancePartner => FindDancePartner(),
				TargetType.MimicryTarget => FindMimicryTarget(),
				TargetType.TheSpear => FindTheSpear(),
				TargetType.TheBalance => FindTheBalance(),
				TargetType.Kardia => FindKardia(),
				TargetType.Deployment => FindDeploymentTacticsTarget(),
				_ => isFriendly ? FindFriendly() : FindHostile(),
			};
		}

		IBattleChara? FindFriendly()
		{
			return battleChara == null ? null : PickByTargeting(battleChara, effectiveType);
		}

		IBattleChara? FindDarkCannonTarget()
		{
			if (battleChara != null)
			{
				if (PhantomRotation.CannoneerLevel < 4)
				{
					return FindHostile();
				}
				else
				{
					foreach (var hostile in battleChara)
					{
						if (!hostile.IsOCBlindImmuneTarget() && hostile.InCombat())
						{
							return hostile;
						}
					}
				}
			}
			return null;
		}

		IBattleChara? FindShockCannonTarget()
		{
			if (battleChara != null)
			{
				foreach (var hostile in battleChara)
				{
					if (!hostile.IsOCParalysisImmuneTarget() && hostile.InCombat())
					{
						return hostile;
					}
				}
			}
			return null;
		}

		IBattleChara? FindPhantomBell()
		{
			var partyMembers = DataCenter.PartyMembers;
			var allMembers = new List<IBattleChara>();
			if (DataCenter.AllTargets != null)
			{
				foreach (var x in DataCenter.AllTargets)
				{
					if (x != null && !x.IsEnemy() && !x.HasStatus(false, StatusID.BattleBell))
					{
						allMembers.Add(x);
					}
				}
			}
			if (partyMembers != null && partyMembers.Count > 0)
			{
				var bePartyAttackedTarget = FindTargetByType(partyMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);
				var beAllAttackedTarget = FindTargetByType(allMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);

				if (bePartyAttackedTarget != null && beAllAttackedTarget != null &&
					bePartyAttackedTarget.GameObjectId == beAllAttackedTarget.GameObjectId)
				{
					return bePartyAttackedTarget;
				}
			}

			if (!StatusHelper.PlayerHasStatus(false, StatusID.BattleBell))
			{
				return Player.Object;
			}
			return null;
		}

		IBattleChara? FindPhantomRespite()
		{
			List<IBattleChara> partyMembers = [];
			if (DataCenter.PartyMembers != null)
			{
				foreach (var x in DataCenter.PartyMembers)
				{
					if (x != null && !x.HasStatus(false, StatusID.RingingRespite))
					{
						partyMembers.Add(x);
					}
				}
			}

			var allMembers = new List<IBattleChara>();
			if (DataCenter.AllTargets != null)
			{
				foreach (var x in DataCenter.AllTargets)
				{
					if (x != null && !x.IsEnemy() && !x.HasStatus(false, StatusID.RingingRespite))
					{
						allMembers.Add(x);
					}
				}
			}

			if (partyMembers != null && partyMembers.Count > 0)
			{
				var bePartyAttackedTarget = FindTargetByType(partyMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);
				var beAllAttackedTarget = FindTargetByType(allMembers, TargetType.BeAttacked, 0, SpecialActionType.None, targetOverride, isFriendly);

				if (bePartyAttackedTarget != null && beAllAttackedTarget != null &&
					bePartyAttackedTarget.GameObjectId == beAllAttackedTarget.GameObjectId)
				{
					return bePartyAttackedTarget;
				}
			}

			return Player.Object;
		}

		IBattleChara? FindDancePartner()
		{
			if (!DataCenter.PlayerAvailable() || !TargetFilter.PlayerIsJobs(Job.DNC)
				|| StatusHelper.PlayerHasStatus(true, StatusID.ClosedPosition) || DancerRotation.IsDancing
				|| DataCenter.PartyMembers.Count < 2)
			{
				return null;
			}

			// Only members among the candidates, which are the ones in Closed Position's range: nothing
			// checks the range after this choice, so a partner out of reach was sent to the game, refused,
			// and chosen again on the next frame - at the head of the Dancer's off-GCD order (A223).
			HashSet<ulong> inReach = [];
			foreach (var candidate in battleChara)
			{
				if (candidate != null)
				{
					inReach.Add(candidate.GameObjectId);
				}
			}

			var priority = OtherConfiguration.DancePartnerPriority;
			var playerId = Player.Object!.GameObjectId;
			var partner = FirstByJobPriority(priority, m => m.GameObjectId != playerId && inReach.Contains(m.GameObjectId) && !m.HasStatus(false, PoorOrPartneredStatuses))
				?? FirstByJobPriority(priority, m => m.GameObjectId != playerId && inReach.Contains(m.GameObjectId) && !m.HasStatus(false, PoorTargetStatuses));
			if (partner != null)
			{
				return partner;
			}

			return RandomMeleeTarget(battleChara) ?? RandomRangeTarget(battleChara)
				?? RandomMagicalTarget(battleChara) ?? RandomPhysicalTarget(battleChara);
		}

		IBattleChara? FindTheSpear()
		{
			return FindCardTarget(OtherConfiguration.TheSpearPriority, StatusID.TheSpear_3889, true);
		}

		IBattleChara? FindTheBalance()
		{
			return FindCardTarget(OtherConfiguration.TheBalancePriority, StatusID.TheBalance_3887, false);
		}

		IBattleChara? FindKardia()
		{
			if (!TargetFilter.PlayerIsJobs(Job.SGE))
			{
				return null;
			}

			if (DataCenter.PartyMembers.Count == 1)
			{
				return Player.Object;
			}

			// The tank holding the aggro first (A567); the job order of the list only where none does or at a tie.
			if (ObjectHelper.TankHoldingAggro() is { } holding)
			{
				return holding;
			}

			var priority = OtherConfiguration.KardiaTankPriority;
			var tank = FirstByJobPriority(priority, static m => m.IsJobCategory(JobRole.Tank) && m.HasStatus(false, StatusHelper.TankStanceStatus) && !m.HasStatus(false, StatusID.Kardion))
				?? FirstByJobPriority(priority, static m => m.IsJobCategory(JobRole.Tank) && m.HasStatus(false, StatusHelper.TankStanceStatus))
				?? FirstByJobPriority(priority, static m => m.IsJobCategory(JobRole.Tank));
			if (tank != null)
			{
				return tank;
			}

			var party = DataCenter.PartyMembers;
			return FindTankTarget() ?? RandomMeleeTarget(party) ?? RandomPhysicalTarget(party)
				?? RandomRangeTarget(party) ?? RandomMagicalTarget(party);
		}
		IBattleChara? FindDeploymentTacticsTarget()
		{
			if (!TargetFilter.PlayerIsJobs(Job.SCH))
			{
				return null;
			}

			if (DataCenter.PartyMembers == null)
			{
				return null;
			}

			// The spread radius from the game's own action row; without it the check below is skipped.
			var spreadRadius = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault((uint)ActionID.DeploymentTacticsPvE) is { } deployment
				? (float)deployment.EffectRange
				: 0f;

			IBattleChara? bestCatalyze = null;
			uint bestCatalyzeShield = 0;

			IBattleChara? bestGalvanize = null;
			uint bestGalvanizeShield = 0;

			foreach (var battleChara in DataCenter.PartyMembers)
			{
				if (battleChara == null || battleChara.IsDead)
				{
					continue;
				}

				var shield = ObjectHelper.GetObjectShield(battleChara);
				if (shield == 0)
				{
					continue;
				}

				// The action spreads this member's Galvanize to the others around them; with nobody there it
				// spends its recast on nothing and is missing at the next raidwide (A597). Members who already
				// carry a Galvanize count: no source says whether a larger one replaces it.
				if (spreadRadius > 0f && !HasOtherMemberWithin(battleChara, spreadRadius))
				{
					continue;
				}

				if (battleChara.HasStatus(true, StatusID.Catalyze))
				{
					if (bestCatalyze == null || shield > bestCatalyzeShield)
					{
						bestCatalyze = battleChara;
						bestCatalyzeShield = shield;
					}
				}
				else if (battleChara.HasStatus(true, StatusID.Galvanize))
				{
					if (bestGalvanize == null || shield > bestGalvanizeShield)
					{
						bestGalvanize = battleChara;
						bestGalvanizeShield = shield;
					}
				}
			}

			if (bestCatalyze != null)
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"FindDeploymentTacticsTarget: {bestCatalyze.Name} is a valid target with Catalyze, largest shield, and nearby allies.");
				}
				return bestCatalyze;
			}

			if (bestGalvanize != null && bestCatalyze == null)
			{
				if (Service.Config.InDebug)
				{
					PluginLog.Debug($"FindDeploymentTacticsTarget: {bestGalvanize.Name} is a valid target with Galvanize, largest shield, and nearby allies.");
				}
				return bestGalvanize;
			}

			return null;
		}

		static bool HasOtherMemberWithin(IBattleChara center, float radius)
		{
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member == null || member.IsDead || member.GameObjectId == center.GameObjectId)
				{
					continue;
				}

				if (Vector3.Distance(center.Position, member.Position) - member.HitboxRadius <= radius)
				{
					return true;
				}
			}

			return false;
		}

		IBattleChara? FindProvokeTarget()
		{
			return FirstCandidateInReach(DataCenter.ProvokeCandidates);
		}

		// The first of the candidates RSR wants acted on that is among this action's own targets - in its range and
		// attackable by the engage setting -, so one it cannot reach does not hold it back from one it can (concept 03,
		// "Unterbrechen, Betäuben, Provozieren: welcher Gegner", A612).
		IBattleChara? FirstCandidateInReach(List<IBattleChara> candidates)
		{
			if (battleChara == null)
			{
				return null;
			}

			foreach (var candidate in candidates)
			{
				foreach (var o in battleChara)
				{
					if (o.GameObjectId == candidate.GameObjectId)
					{
						return o;
					}
				}
			}

			return null;
		}

		// A tank swap (TankSwapWatch): for Shirk the tank to hand the tankbuster's source to, for
		// Provoke that source when it is taken back - in either case only if it is among this action's
		// candidates, in range and targetable for it.
		IBattleChara? FindTankSwapTarget()
		{
			var swap = isFriendly ? TankSwapWatch.Target : TankSwapWatch.ReclaimSource;
			if (battleChara != null && swap != null)
			{
				foreach (var o in battleChara)
				{
					if (o.GameObjectId == swap.GameObjectId)
					{
						return swap;
					}
				}
			}
			return null;
		}

		IBattleChara? FindTargetForMoving()
		{
			if (battleChara == null || Player.Object == null)
			{
				return null;
			}

			var pPosition = Player.Object.Position;
			var towardsScreenCenter = Service.Config.MoveTowardsScreenCenter;
			var playerScrPos = Vector2.Zero;
			if (towardsScreenCenter && !Svc.GameGui.WorldToScreen(pPosition, out playerScrPos))
			{
				return null;
			}

			var faceVec = Player.Object.GetFaceVector();
			var halfAngle = Math.PI * Service.Config.MoveTargetAngle / 360;
			IBattleChara? farthest = null;
			var farthestDistance = float.MinValue;
			// The candidates lie within the action's range already (FindTargetAreaMove). "Only use damaging gap closers within
			// this distance" does not limit a move-forward command, its text says; it limited this path until A520, when no
			// action reached it yet.
			foreach (var t in battleChara)
			{
				var distance = t.DistanceToPlayer();
				if (distance <= farthestDistance)
				{
					continue;
				}

				bool inDirection;
				if (towardsScreenCenter)
				{
					if (!Svc.GameGui.WorldToScreen(t.Position, out var scrPos))
					{
						continue;
					}

					var dir = scrPos - playerScrPos;
					inDirection = dir.Y <= 0 && Math.Abs(dir.X / dir.Y) <= Math.Tan(halfAngle);
				}
				else
				{
					inDirection = Vector3.Dot(faceVec, Vector3.Normalize(t.Position - pPosition)) >= Math.Cos(halfAngle);
				}

				if (inDirection)
				{
					farthest = t;
					farthestDistance = distance;
				}
			}

			return farthest;
		}

		IBattleChara? FindHealTarget(float healRatio)
		{
			if (battleChara == null)
			{
				return null;
			}

			List<IBattleChara> filteredGameObjects = [];
			List<IBattleChara>? figures = null;
			foreach (var o in battleChara)
			{
				// A figure only healing defeats needs every point until it is full - at 94 % the last heal is as urgent as the
				// first, or its cast runs out - so the heal ratio does not cut it, and it is ranked apart below (A516).
				if (HealToDefeat.IsFigure(o))
				{
					(figures ??= []).Add(o);
					continue;
				}

				// The death-trigger hold has to bite at the entrance, not further down. Everything
				// below draws from this set - the ranked candidate list, the healer and tank passes,
				// the worst-hurt pick and both fallbacks - and the tank-stance fallback would
				// otherwise hand back the very dark knight being held, since it wears Grit. The hold
				// then failed exactly where it was needed: whenever the member who raised the heal
				// flag is out of this action's range, the bearer is the only candidate left.
				//
				// The self short-cut below bypasses this set and carries the same check of its own.
				if (o.IsHeldForDeathTrigger())
				{
					continue;
				}

				// Forecast health here too, and this is the gate the rest depends on: everything below
				// draws from this set, so a member kept out here is invisible to the ranked list, to
				// the critical rank and to every short-cut. AutoHealRatio defaults to 0.8, which is
				// exactly where the look-ahead earns its keep - a tank at 90% who reaches zero in six
				// seconds is at a forecast 34%, and judged by his current health he would be dropped
				// before anything downstream ever saw him.
				//
				// This is not overhealing: the cut exists to keep a cast from being spent on somebody
				// who does not need it, and somebody who will be at 34% when the cast lands does.
				if (!IBaseAction.AutoHealCheck || o.GetForecastHealthRatio(instantHeal) < healRatio)
				{
					filteredGameObjects.Add(o);
				}
			}

			List<IBattleChara> partyMembers = [];
			foreach (var o in filteredGameObjects)
			{
				if (ObjectHelper.IsParty(o))
				{
					partyMembers.Add(o);
				}
			}

			// A member about to die first, then the figures only healing defeats, then everyone else (concept 07, A516):
			// failing them hits all eight, and a member they curse is helped by nothing but their death.
			if (figures != null)
			{
				return CriticalMember(partyMembers, instantHeal) ?? PickFigure(figures, instantHeal);
			}

			var result = GeneralHealTarget(partyMembers, instantHeal);
			if (result != null)
			{
				return result;
			}

			result = GeneralHealTarget(filteredGameObjects, instantHeal);
			if (result != null)
			{
				return result;
			}

			foreach (var t in partyMembers)
			{
				if (t.HasStatus(false, StatusHelper.TankStanceStatus))
				{
					return t;
				}
			}

			if (partyMembers.Count > 0)
			{
				return partyMembers[0];
			}

			foreach (var t in filteredGameObjects)
			{
				if (t.HasStatus(false, StatusHelper.TankStanceStatus))
				{
					return t;
				}
			}

			if (filteredGameObjects.Count > 0)
			{
				return filteredGameObjects[0];
			}

			return null;

			static IBattleChara? GeneralHealTarget(List<IBattleChara> objs, bool instant)
			{
				// Everyone healable is a candidate. Only a status that nullifies healing outright
				// takes a target out; a protective status merely moves it back in the queue.
				//
				// This used to read `if (!o.NoNeedHealingInvuln())`, which collected the exact opposite set:
				// the function (CanBeHealed since 7.5.7.1) returns true when NO protective status is up
				// (an absent status has trivially "ended"), so the list held only the invulnerable
				// and was empty in the ordinary case. Everything below it - the healer pass, the
				// tank pass, the worst-hurt pick - then ran over nothing and the caller fell back
				// to the tank-stance holder, so the worst-hurt member could never be chosen.
				//
				// Demoting rather than excluding is what the protection actually warrants: the
				// invulnerability window is the safest moment to heal, and when it ends the target
				// stands wherever it left them - Superbolide leaves them at 1 HP on purpose.
				// Both sort keys are read once per member rather than inside the comparator. Sorting
				// n members costs O(n log n) comparisons, and each key walks a status list or the
				// party/doom checks in GetHealthRatio - this runs in the combat path. Reading them
				// up front also fixes the keys for the duration of the sort: CanBeHealed
				// compares a live RemainingTime against a GCD window, so a status sitting on that
				// boundary could otherwise answer differently between two comparisons, and an
				// inconsistent comparer makes List.Sort throw.
				List<(IBattleChara Obj, bool Unprotected, float Health)> ranked = [];
				foreach (var o in objs)
				{
					// A corpse takes no healing, so it is not a healing candidate - the same reason
					// HealingIneffectiveStatus keeps its bearers out, taken to its extreme.
					//
					// Nothing upstream of here excludes the dead: GetCanTargets only drops targets at
					// full health, and GetHealthRatio returns 0 for a corpse rather than something
					// out of range. The worst-hurt pick therefore always rated a dead member as the
					// most urgent, and only the role short-cuts ahead of it kept that from showing.
					// The critical rank below would have made it unconditional, because a corpse
					// holds the fewest effective points there can be. Raising is a separate path
					// with its own target type, so nothing here is taken away from it.
					if (o.IsDead || o.HasStatus(false, StatusHelper.HealingIneffectiveStatus))
					{
						continue;
					}

					// Forecast health rather than current, so the ordering below answers "who will be
					// worst off when a heal lands" instead of "who is worst off now". With the
					// setting off the two are the same number.
					ranked.Add((o, o.IsUnprotectedForHealing(), o.GetForecastHealthRatio(instant)));
				}

				// Unprotected before protected, then lowest health first inside each group.
				ranked.Sort((a, b) =>
				{
					var byProtection = b.Unprotected.CompareTo(a.Unprotected);
					return byProtection != 0 ? byProtection : a.Health.CompareTo(b.Health);
				});

				// Anyone about to die comes before every role short-cut below.
				//
				// The three short-cuts return outright once their own threshold is met and look at
				// nobody else, so a damage dealer at 10% was passed over the moment the tank stood
				// at 44%, or the player themself at 39%. That is the case the user names - "auch ein
				// Damagedealer ohne Aggro mit 10% Leben kann bei einem AoE sterben" - and the tank
				// behind mitigations and a large pool is not in more danger there, only earlier in
				// the order. His ranking of healer before tank before damage dealer applies at
				// *equal* danger, which 10% against 44% is not.
				//
				// The threshold is the tree's own statement of "this one is about to fall": the
				// value at which the tank rotations fire their invulnerability. Effective health,
				// so a barrier counts - a tank at 10% behind The Blackest Night does not die to the
				// next hit - and read the same way CanProvoke reads it.
				//
				// Ordered by absolute effective points rather than by percentage, because whoever
				// stands here dies to the next hit and a hit is an absolute number: 10% of a small
				// pool is fewer points than 14% of a large one. Role decides only a tie, which is
				// where the instruction puts it.
				//
				// Protected members stay out. Without that this rank would invert into its own
				// opposite - a gunbreaker under Superbolide sits at 1 HP on purpose, would hold the
				// fewest points of anyone and would outrank every genuine emergency.
				var critical = CriticalMember(objs, instant);
				if (critical != null)
				{
					return critical;
				}

				List<IBattleChara> healingNeededObjs = [];
				foreach (var r in ranked)
				{
					healingNeededObjs.Add(r.Obj);
				}

				// The role short-cuts below return their candidate outright once it drops under its
				// ratio, so they run ahead of the ordering above. Feeding them protected members
				// would undo the demotion at exactly the case it exists for: a Superbolide tank
				// sits at 1 HP, clears HealthTankRatio immediately, and would be picked over an
				// unprotected member at 20%. So the role passes see unprotected members only;
				// anyone protected is left to the final worst-hurt pick, which still reaches them
				// when nobody else needs healing.
				List<IBattleChara> healerTars = [];
				List<IBattleChara> tankTars = [];
				foreach (var r in ranked)
				{
					if (!r.Unprotected)
					{
						continue;
					}

					var enumHealer = TargetFilter.GetJobCategory([r.Obj], JobRole.Healer).GetEnumerator();
					var isHealer = enumHealer.MoveNext();
					enumHealer.Dispose();
					if (isHealer)
					{
						healerTars.Add(r.Obj);
						continue;
					}

					var enumTank = TargetFilter.GetJobCategory([r.Obj], JobRole.Tank).GetEnumerator();
					var isTank = enumTank.MoveNext();
					enumTank.Dispose();
					if (isTank)
					{
						tankTars.Add(r.Obj);
					}
				}

				// The self short-cut bypasses the ranking below, so it repeats the two checks that
				// keep a target out of it: a heal on a target that nullifies HP recovery is a wasted
				// cast, and a dark knight inside its Living Dead window is being held on purpose.
				// Without the second check, any other member raising the heal flag would let the
				// bearer heal itself out of the trigger through this path. It also asks whether the
				// action can reach the player at all (upstream, c3fac720b): an action that only targets
				// others, Nascent Flash for one, returned the player here and so was never cast.
				var player = Player.Object;
				if (player != null
					&& !player.HasStatus(false, StatusHelper.HealingIneffectiveStatus)
					&& !ObjectHelper.PlayerIsHeldForDeathTrigger()
					&& ObjectHelper.GetForecastPlayerHealthRatio(instant) <= Service.Config.HealthSelfRatio)
				{
					foreach (var o in objs)
					{
						if (o.GameObjectId == player.GameObjectId)
						{
							return player;
						}
					}
				}

				if (Service.Config.HealTargetByDanger)
				{
					return DangerClassTarget(ranked, instant);
				}

				var healerTar = healerTars.Count > 0 ? healerTars[0] : null;
				if (healerTar != null && healerTar.GetForecastHealthRatio(instant) <= Service.Config.HealthHealerRatio)
				{
					return healerTar;
				}

				var tankTar = tankTars.Count > 0 ? tankTars[0] : null;
				if (tankTar != null && tankTar.GetForecastHealthRatio(instant) <= Service.Config.HealthTankRatio)
				{
					return tankTar;
				}

				// The last pick keeps the plain ratio: it asks "is anyone hurt at all", and a member
				// at full health with a falling trend is not hurt yet. Forecasting here would hand
				// the heal to somebody the caller never asked about.
				var tar = healingNeededObjs.Count > 0 ? healingNeededObjs[0] : null;
				return tar != null && tar.GetHealthRatio() < 1 ? tar : null;
			}
		}

		// Classes 2 and 3 of the owner's triage (concept 07), below class 1 and the self short-cut.
		//
		// Class 2: a healer or tank under their role threshold who is also being attacked. The
		// threshold says "low enough", the aggro "and actually taking damage" - without it a tank would
		// sit in this class for good, since he always wears his stance. Lowest health first, a healer
		// before a tank at equal health.
		//
		// Class 3: everyone else unprotected. While area damage is announced, fewest effective hit
		// points first, because a raidwide takes the same number from everybody and a small pool dies
		// to it first; otherwise lowest health first. Role decides a tie. Announced is asked without the
		// mitigation verdict (DataCenter.AreaHitAnnounced): a cast too small to spend a cooldown on is
		// still area damage, and the large interruptible cast and the BossMod raidwide count as well.
		//
		// Protected members come last, as everywhere in this method; the last pick keeps the plain
		// "is anyone hurt at all" check.
		static IBattleChara? DangerClassTarget(List<(IBattleChara Obj, bool Unprotected, float Health)> ranked, bool instant)
		{
			IBattleChara? pressed = null;
			var pressedHealth = float.MaxValue;
			var pressedRole = int.MaxValue;
			foreach (var r in ranked)
			{
				if (!r.Unprotected || !DataCenter.TargetedPartyMembers.Contains(r.Obj.GameObjectId))
				{
					continue;
				}

				var isHealer = r.Obj.IsJobCategory(JobRole.Healer);
				if (!isHealer && !r.Obj.IsJobCategory(JobRole.Tank))
				{
					continue;
				}

				if (r.Health > (isHealer ? Service.Config.HealthHealerRatio : Service.Config.HealthTankRatio))
				{
					continue;
				}

				var role = isHealer ? 0 : 1;
				if (pressed == null || r.Health < pressedHealth || (r.Health == pressedHealth && role < pressedRole))
				{
					pressed = r.Obj;
					pressedHealth = r.Health;
					pressedRole = role;
				}
			}

			if (pressed != null)
			{
				// The role thresholds differ (tank 0.45, healer 0.40 by default), and they stand in for the damage
				// rate: the tank takes the hits, the healer stands still. In this class both are being attacked, so
				// that reason does not hold, and a healer at 42 % lost to a tank at 44 % - against the owner's
				// triage, the one most likely to die first and a healer before a tank at equal health (concept 07).
				// A threshold says when a role is low enough to come first, not that a lower member of the other
				// role must wait: whoever is being attacked and stands as low as the pick or lower joins it (A283).
				foreach (var r in ranked)
				{
					if (!r.Unprotected || r.Health > pressedHealth || !DataCenter.TargetedPartyMembers.Contains(r.Obj.GameObjectId))
					{
						continue;
					}

					var isHealer = r.Obj.IsJobCategory(JobRole.Healer);
					if (!isHealer && !r.Obj.IsJobCategory(JobRole.Tank))
					{
						continue;
					}

					var role = isHealer ? 0 : 1;
					if (r.Health < pressedHealth || (r.Health == pressedHealth && role < pressedRole))
					{
						pressed = r.Obj;
						pressedHealth = r.Health;
						pressedRole = role;
					}
				}

				return pressed;
			}

			var byPoints = DataCenter.AreaHitAnnounced;
			IBattleChara? rest = null;
			var restKey = float.MaxValue;
			var restRole = int.MaxValue;
			foreach (var r in ranked)
			{
				// Only the hurt: ranked by points, a damage dealer at full health has the smallest pool,
				// won the class, and the check below then healed nobody while the tank was at half.
				if (!r.Unprotected || r.Obj.GetHealthRatio() >= 1)
				{
					continue;
				}

				var key = byPoints ? r.Obj.GetForecastEffectiveHp(instant) : r.Health;
				var role = TieBreakRank(r.Obj);
				if (rest == null || key < restKey || (key == restKey && role < restRole))
				{
					rest = r.Obj;
					restKey = key;
					restRole = role;
				}
			}

			rest ??= ranked.Count > 0 ? ranked[0].Obj : null;
			return rest != null && rest.GetHealthRatio() < 1 ? rest : null;
		}

		// Among equal points, a healer first, then a tank, then everyone else (concept 07).
		// The member about to die, if any: in the critical class, not protected, healing not nullified, fewest effective
		// points first and role on a tie. The first rank of GeneralHealTarget, and ahead of the figures only healing defeats.
		static IBattleChara? CriticalMember(List<IBattleChara> objs, bool instant)
		{
			IBattleChara? critical = null;
			var criticalHp = uint.MaxValue;
			var criticalRole = int.MaxValue;
			foreach (var o in objs)
			{
				if (o.IsDead || o.HasStatus(false, StatusHelper.HealingIneffectiveStatus))
				{
					continue;
				}

				var unprotected = o.IsUnprotectedForHealing();
				if (!o.IsInCriticalClass(unprotected))
				{
					continue;
				}

				var hp = o.GetForecastEffectiveHp(instant);
				var role = TieBreakRank(o);
				if (critical == null || hp < criticalHp || (hp == criticalHp && role < criticalRole))
				{
					critical = o;
					criticalHp = hp;
					criticalRole = role;
				}
			}

			return critical;
		}

		// Which figure only healing defeats is healed first (concept 07, A516): the one tethering a member whom healing no
		// longer reaches and who is about to die - nothing but the figure's death helps him -, the fewest effective points
		// first; otherwise the one nearest full, so that one after the other is finished and each ends or prevents a curse.
		static IBattleChara PickFigure(List<IBattleChara> figures, bool instant)
		{
			IBattleChara? rescue = null;
			var rescueHp = uint.MaxValue;
			var nearest = figures[0];
			var nearestRatio = nearest.GetHealthRatio();
			foreach (var figure in figures)
			{
				var ratio = figure.GetHealthRatio();
				if (ratio > nearestRatio)
				{
					nearest = figure;
					nearestRatio = ratio;
				}

				if (HealToDefeat.TetheredMember(figure) is { } member
					&& member.HasStatus(false, StatusHelper.HealingIneffectiveStatus)
					&& member.IsInCriticalClass(member.IsUnprotectedForHealing())
					&& member.GetForecastEffectiveHp(instant) is var hp && hp < rescueHp)
				{
					rescue = figure;
					rescueHp = hp;
				}
			}

			return rescue ?? nearest;
		}

		static int TieBreakRank(IBattleChara member)
		{
			return member.IsJobCategory(JobRole.Healer) ? 0
				: member.IsJobCategory(JobRole.Tank) ? 1
				: 2;
		}

		IBattleChara? FindInterruptTarget()
		{
			return FirstCandidateInReach(DataCenter.InterruptCandidates);
		}

		IBattleChara? FindHostile()
		{
			if (battleChara == null)
			{
				return null;
			}

			// Manual Any() check
			var hasAny = false;
			foreach (var _ in battleChara)
			{
				hasAny = true;
				break;
			}
			if (!hasAny)
			{
				return null;
			}

			if (Service.Config.FilterStopMark2)
			{
				var filteredCharacters = MarkingHelper.FilterStopCharacters(battleChara);
				var filteredHasAny = false;
				if (filteredCharacters != null)
				{
					foreach (var _ in filteredCharacters)
					{
						filteredHasAny = true;
						break;
					}
				}
				if (filteredCharacters != null && filteredHasAny)
				{
					battleChara = filteredCharacters;
				}
			}

			var highPriorityHostiles = new List<IBattleChara>();
			var bestIconPriority = int.MaxValue;
			foreach (var b in battleChara)
			{
				if (!ObjectHelper.IsTopPriorityHostile(b) && !ObjectHelper.IsTopPriorityNamedHostile(b))
				{
					continue;
				}

				var iconPriority = ObjectHelper.GetNamePlateIconPriority(b.GetNamePlateIcon());
				if (iconPriority > bestIconPriority)
				{
					continue;
				}

				if (iconPriority < bestIconPriority)
				{
					bestIconPriority = iconPriority;
					highPriorityHostiles.Clear();
				}

				highPriorityHostiles.Add(b);
			}
			if (highPriorityHostiles.Count > 0)
			{
				battleChara = highPriorityHostiles;
			}

			return FindHostileRaw();
		}

		IBattleChara? FindHostileRaw()
		{
			if (battleChara == null)
			{
				return null;
			}

			// A damage over time being spread orders the candidates itself (DotSpreadOrder), after the stop
			// markers and the top-priority hostiles above have had their say; an explicit command still wins.
			if (commandTargeting == null && hostileOrder != null)
			{
				return MinBy(battleChara, hostileOrder);
			}

			// Alone without tank and healer as a beastmaster or blue mage: the most dangerous per HP first (A541).
			if (!commandTargeting.HasValue && !IsSortType(effectiveType) && DataCenter.DangerFirstAlone)
			{
				var lead = MinBy(battleChara, EnemyDanger.Compare);
				EnemyDanger.NoteLead(lead);
				return lead;
			}

			var sort = commandTargeting.HasValue || IsSortType(effectiveType) ? effectiveType : ToTargetType(DataCenter.TargetingType);
			return PickByTargeting(battleChara, sort);
		}

		IBattleChara? FindBeAttackedTarget()
		{
			if (battleChara == null)
			{
				return null;
			}

			var hasAny = false;
			foreach (var _ in battleChara)
			{
				hasAny = true;
				break;
			}
			if (!hasAny)
			{
				return null;
			}

			// A tankbuster aimed at one of them names whom the help is for (TankbusterHelp.IsTargetToProtect): a healer's
			// Aquaveil or Exaltation in the single-target defence went to whoever their own target attacks - the main
			// tank - while the marker or the cast named the off-tank (A328). Without one among them, as before.
			List<IBattleChara> attachedT = [];
			foreach (var t in battleChara)
			{
				// So does a measured cast aimed at a member who is no tank, when it calls for a defence (A540).
				if (TankbusterHelp.IsTargetToProtect(t) || TankbusterHelp.AimedHitCallsForDefence(t, out _))
				{
					attachedT.Add(t);
				}
			}

			// Several aimed at, the help goes to the one standing with the least protection (LeastProtected).
			attachedT = LeastProtected(attachedT);

			if (attachedT.Count == 0)
			{
				foreach (var t in battleChara)
				{
					if (ObjectHelper.IsTargetOnSelf(t) && !TankbusterHelp.NeedsNothingFromYou(t))
					{
						attachedT.Add(t);
					}
				}
			}

			if (!DataCenter.AutoStatus.HasFlag(AutoStatus.DefenseSingle))
			{
				if (attachedT.Count == 0)
				{
					foreach (var tank in battleChara)
					{
						if (tank.HasStatus(false, StatusHelper.TankStanceStatus))
						{
							attachedT.Add(tank);
						}
					}
				}

				if (attachedT.Count == 0)
				{
					foreach (var tank in battleChara.GetJobCategory(JobRole.Tank))
					{
						attachedT.Add(tank);
					}
				}

				if (attachedT.Count == 0)
				{
					foreach (var t in battleChara)
					{
						attachedT.Add(t);
					}
				}
			}

			if (attachedT.Count == 0)
			{
				return null;
			}

			if (Service.Config.Priolowtank)
			{
				IBattleChara? lowest = null;
				var minHealth = float.MaxValue;
				foreach (var t in attachedT)
				{
					var health = ObjectHelper.GetHealthRatio(t);
					if (health < minHealth)
					{
						minHealth = health;
						lowest = t;
					}
				}
				return lowest;
			}
			else
			{
				IBattleChara? highest = null;
				var maxHealth = float.MinValue;
				foreach (var t in attachedT)
				{
					var health = ObjectHelper.GetHealthRatio(t);
					if (health > maxHealth)
					{
						maxHealth = health;
						highest = t;
					}
				}
				return highest;
			}
		}

		// The first of the members an enemy's damage over time takes below the threshold, lowest first, that this action
		// can still be put on - one who carries it already is not among its targets (A432).
		IBattleChara? FindHotAgainstDotTarget()
		{
			if (battleChara == null)
			{
				return null;
			}

			foreach (var member in DataCenter.HotAgainstDotMembers)
			{
				foreach (var o in battleChara)
				{
					if (o.GameObjectId == member.GameObjectId)
					{
						return o;
					}
				}
			}

			return null;
		}

		IBattleChara? FindDispelTarget()
		{
			if (battleChara != null && DataCenter.DispelTarget != null)
			{
				foreach (var o in battleChara)
				{
					if (o.GameObjectId == DataCenter.DispelTarget.GameObjectId)
					{
						return DataCenter.DispelTarget;
					}
				}
			}
			return null;
		}

		IBattleChara? FindTankTarget()
		{
			if (DataCenter.PartyMembers == null)
			{
				return null;
			}

			// Never the player: the one tank action that asks for "a tank" is Shirk, which cannot
			// target its user, and a tank in stance is first in the party list - so the manual Shirk
			// command picked the main tank himself and went nowhere. A healer is never a tank, so the
			// heal-over-time callers are unchanged.
			var self = Player.Object?.GameObjectId;
			foreach (var m in DataCenter.PartyMembers)
			{
				if (m.IsJobCategory(JobRole.Tank) && !m.IsDead && m.GameObjectId != self)
				{
					if (m.HasStatus(false, StatusHelper.TankStanceStatus))
					{
						if (m.IsConditionCannotTarget())
						{
							continue;
						}

						if (Service.Config.InDebug)
						{
							PluginLog.Debug($"FindTankTarget 1: {m.Name} is a tank with TankStanceStatus.");
						}
						return m;
					}
				}
			}

			foreach (var m in DataCenter.PartyMembers)
			{
				if (m.IsJobCategory(JobRole.Tank) && !m.IsDead && m.GameObjectId != self)
				{
					if (m.IsConditionCannotTarget())
					{
						continue;
					}

					if (Service.Config.InDebug)
					{
						PluginLog.Debug($"FindTankTarget 2: {m.Name} is a tank fallback.");
					}
					return m;
				}
			}

			if (battleChara == null)
			{
				return null;
			}

			List<IBattleChara> others = [];
			foreach (var b in battleChara)
			{
				if (b.GameObjectId != self)
				{
					others.Add(b);
				}
			}

			return RandomPickByJobs(others, JobRole.Tank);
		}

		IBattleChara? FindTankbusterTarget()
		{
			// Whom a tankbuster is aimed at - marker, targeted cast, BossModReborn's mask for the player
			// (DataCenter.IsTankbusterAimedAt) - among the members this action can take: in range, its own filters
			// passed. The player first, as "prioritizing self" says. Until A328 this read only the marker list, which
			// for a tank held only a marker on himself, so a cast at him or any tankbuster on the co-tank found no one,
			// and it returned a list entry the action could not take when none was usable.
			if (battleChara == null || Player.Object == null)
			{
				return null;
			}

			List<IBattleChara> others = [];
			foreach (var t in battleChara)
			{
				if (!ObjectHelper.IsAlive(t) || t.IsConditionCannotTarget() || !TankbusterHelp.IsTargetToProtect(t))
				{
					continue;
				}

				if (t.GameObjectId == Player.Object.GameObjectId)
				{
					return t;
				}

				others.Add(t);
			}

			// Several others aimed at, the help goes to the one standing with the least protection (LeastProtected).
			others = LeastProtected(others);
			return others.Count > 0 ? others[0] : null;
		}

		// Of several members a tankbuster is aimed at, those standing with the fewest damage-reducing effects - a rated
		// mitigation (DefensiveValues.MitigationByStatusId) or a barrier (StatusHelper.ShieldStatus), from anyone.
		// Without it every help went to the same one of them, by health: with two tanks under one double tankbuster
		// a White Mage put Divine Benison and Aquaveil on one tank, whose hit the barrier took whole, while the other
		// took 74 % of his HP under a vulnerability (his trace of 25.10.2026, A364). One member, or all equal: as given.
		static List<IBattleChara> LeastProtected(List<IBattleChara> aimedAt)
		{
			if (aimedAt.Count <= 1)
			{
				return aimedAt;
			}

			var fewest = int.MaxValue;
			List<IBattleChara> least = [];
			foreach (var t in aimedAt)
			{
				var count = ProtectionsOn(t);
				if (count < fewest)
				{
					fewest = count;
					least.Clear();
				}

				if (count == fewest)
				{
					least.Add(t);
				}
			}

			return least;
		}

		static int ProtectionsOn(IBattleChara member)
		{
			var count = 0;
			var statuses = member.StatusList;
			if (statuses == null)
			{
				return 0;
			}

			foreach (var status in statuses)
			{
				if (status != null && status.StatusId != 0
					&& (DefensiveValues.MitigationByStatusId.ContainsKey(status.StatusId)
						|| Array.IndexOf(StatusHelper.ShieldStatus, (StatusID)status.StatusId) >= 0))
				{
					count++;
				}
			}

			return count;
		}
	}

	#region TargetFind

	/// <summary>
	/// Finds the target based on the specified type and criteria.
	/// </summary>IBattleChara battleChara
	/// <param name="battleChara"></param>
	/// <param name="type"></param>
	/// <param name="healRatio"></param>
	/// <param name="actionType"></param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <param name="isFriendly">Indicates whether the target is friendly.</param>
	/// <returns></returns>
	public static IBattleChara? FindTargetByType(IEnumerable<IBattleChara> battleChara, TargetType type, float healRatio, SpecialActionType actionType, TargetType targetOverride, bool isFriendly)
		=> FindTargetByType(battleChara, type, healRatio, actionType, targetOverride, isFriendly, false);

	/// <summary>
	/// <see cref="FindTargetByType(IEnumerable{IBattleChara}, TargetType, float, SpecialActionType, TargetType, bool)"/>
	/// for a heal of the given kind: <paramref name="instantHeal"/> is true for an off-GCD action, whose
	/// heal-ahead reading looks no further than the animation lock (A213).
	/// </summary>
	public static IBattleChara? FindTargetByType(IEnumerable<IBattleChara> battleChara, TargetType type, float healRatio, SpecialActionType actionType, TargetType targetOverride, bool isFriendly, bool instantHeal)
	{
		return FindTargetByType(battleChara, type, healRatio, actionType, targetOverride, isFriendly, instantHeal, null, null);
	}

	private static IBattleChara? FindMimicryTarget()
	{
		if (DataCenter.AllTargets == null)
		{
			return null;
		}

		IBattleChara? bestTarget = null;
		var bestDistance = float.MaxValue;

		foreach (var target in DataCenter.AllTargets)
		{
			if (target != null && IsNeededRole(target))
			{
				var distance = Player.Object != null ? Player.DistanceTo(target.Position) : float.MaxValue;
				if (distance < bestDistance)
				{
					bestDistance = distance;
					bestTarget = target;
				}
			}
		}

		return bestTarget;
	}
	private static IBattleChara? PickByTargeting(IEnumerable<IBattleChara> candidates, TargetType type)
	{
		var role = type switch
		{
			TargetType.PvPHealers => JobRole.Healer,
			TargetType.PvPTanks => JobRole.Tank,
			TargetType.PvPDPS => JobRole.AllDPS,
			_ => JobRole.None,
		};

		if (role != JobRole.None)
		{
			var jobs = role.ToJobs();
			return MinBy(candidates, NearestFirst, c => c.IsJobs(jobs)) ?? MinBy(candidates, NearestFirst);
		}

		return MinBy(candidates, GetTargetingComparison(type));
	}

	private static IBattleChara? MinBy(IEnumerable<IBattleChara> candidates, Comparison<IBattleChara> comparison, Func<IBattleChara, bool>? filter = null)
	{
		IBattleChara? best = null;
		foreach (var candidate in candidates)
		{
			if (candidate != null && (filter == null || filter(candidate)) && (best == null || comparison(candidate, best) < 0))
			{
				best = candidate;
			}
		}

		return best;
	}

	private static readonly Comparison<IBattleChara> NearestFirst = static (a, b) => a.DistanceToPlayer().CompareTo(b.DistanceToPlayer());

	private static Comparison<IBattleChara> GetTargetingComparison(TargetType type)
	{
		return type switch
		{
			TargetType.Small => static (a, b) => CompareHitboxThenHp(a, b, ascendingHitbox: true, Service.Config.SmallHp),
			TargetType.HighHP => static (a, b) => b.CurrentHp.CompareTo(a.CurrentHp),
			TargetType.LowHP => static (a, b) => a.CurrentHp.CompareTo(b.CurrentHp),
			TargetType.HighHPPercent => static (a, b) => HpPercent(b).CompareTo(HpPercent(a)),
			TargetType.LowHPPercent => static (a, b) => HpPercent(a).CompareTo(HpPercent(b)),
			TargetType.HighMaxHP => static (a, b) => b.MaxHp.CompareTo(a.MaxHp),
			TargetType.LowMaxHP => static (a, b) => a.MaxHp.CompareTo(b.MaxHp),
			TargetType.Nearest => NearestFirst,
			TargetType.Farthest => static (a, b) => b.DistanceToPlayer().CompareTo(a.DistanceToPlayer()),
			// The Big case reads its own setting; it read SmallHp, and BigHp had no reader (A225).
			_ => static (a, b) => CompareHitboxThenHp(a, b, ascendingHitbox: false, Service.Config.BigHp),
		};

		static float HpPercent(IBattleChara c)
		{
			return c.MaxHp == 0 ? 0 : (float)c.CurrentHp / c.MaxHp;
		}

		// Ties on hitbox size go to the lower HP target when the case's own setting ("Small HP", "Big HP") is on,
		// otherwise to the higher.
		static int CompareHitboxThenHp(IBattleChara a, IBattleChara b, bool ascendingHitbox, bool lowerHpFirst)
		{
			var cmp = ascendingHitbox ? a.HitboxRadius.CompareTo(b.HitboxRadius) : b.HitboxRadius.CompareTo(a.HitboxRadius);
			if (cmp != 0)
			{
				return cmp;
			}

			return lowerHpFirst ? a.CurrentHp.CompareTo(b.CurrentHp) : b.CurrentHp.CompareTo(a.CurrentHp);
		}
	}

	private static bool IsSortType(TargetType type) => type is TargetType.Small or TargetType.HighHP or TargetType.LowHP
		or TargetType.HighHPPercent or TargetType.LowHPPercent or TargetType.HighMaxHP or TargetType.LowMaxHP
		or TargetType.Nearest or TargetType.Farthest or TargetType.PvPHealers or TargetType.PvPTanks or TargetType.PvPDPS;

	private static TargetType ToTargetType(TargetingType targetingType) => targetingType switch
	{
		TargetingType.Small => TargetType.Small,
		TargetingType.HighHP => TargetType.HighHP,
		TargetingType.LowHP => TargetType.LowHP,
		TargetingType.HighHPPercent => TargetType.HighHPPercent,
		TargetingType.LowHPPercent => TargetType.LowHPPercent,
		TargetingType.HighMaxHP => TargetType.HighMaxHP,
		TargetingType.LowMaxHP => TargetType.LowMaxHP,
		TargetingType.Nearest => TargetType.Nearest,
		TargetingType.Farthest => TargetType.Farthest,
		TargetingType.PvPHealers => TargetType.PvPHealers,
		TargetingType.PvPTanks => TargetType.PvPTanks,
		TargetingType.PvPDPS => TargetType.PvPDPS,
		_ => TargetType.Big,
	};

	private static bool IsNeededRole(IBattleChara character)
	{
		if (Player.Object == null)
		{
			return false;
		}

		if (character.GameObjectId == Player.Object.GameObjectId)
		{
			return false;
		}

		if (character is not IPlayerCharacter player || player.ClassJob.Value.IsLimitedJob)
		{
			return false;
		}

		var neededRole = DataCenter.BluRole;
		return neededRole != null && neededRole != CombatRole.NonCombat && (int)neededRole == (int)player.GetRole();
	}

	private static readonly StatusID[] PoorTargetStatuses = [StatusID.DamageDown_2911, StatusID.DamageDown, StatusID.Weakness, StatusID.BrinkOfDeath];
	private static readonly StatusID[] PoorOrPartneredStatuses = [.. PoorTargetStatuses, StatusID.DancePartner, StatusID.ClosedPosition];

	private static IBattleChara? FirstByJobPriority(List<Job> priority, Func<IBattleChara, bool> predicate)
	{
		foreach (var job in priority)
		{
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member.IsJobs(job) && !member.IsDead && !member.IsConditionCannotTarget() && predicate(member))
				{
					return member;
				}
			}
		}

		return null;
	}

	private static IBattleChara? FindCardTarget(List<Job> priority, StatusID cardStatus, bool preferRanged)
	{
		var player = Player.Object;
		if (!TargetFilter.PlayerIsJobs(Job.AST) || player == null)
		{
			return null;
		}

		var party = DataCenter.PartyMembers;
		var playerId = player.GameObjectId;

		if (party.Count == 1 || (party.Count == 2 && party.Exists(m => m.GameObjectId == playerId) && party.Exists(ObjectHelper.IsPlayerCharacterChocobo)))
		{
			return player;
		}

		List<IBattleChara> candidates = [];
		foreach (var m in party)
		{
			if (m == null || m.GameObjectId == playerId || m.IsPlayerCharacterChocobo() || m.IsDead || m.IsConditionCannotTarget()
				|| m.HasStatus(false, PoorTargetStatuses) || m.HasStatus(false, cardStatus))
			{
				continue;
			}

			candidates.Add(m);
		}

		IBattleChara? best = null;
		var bestIndex = int.MaxValue;
		foreach (var member in candidates)
		{
			var index = priority.FindIndex(job => member.IsJobs(job));
			if (index < 0)
			{
				continue;
			}

			if (best == null || index < bestIndex
				|| (index == bestIndex && (member.MaxHp > best.MaxHp || (member.MaxHp == best.MaxHp && member.CurrentHp > best.CurrentHp))))
			{
				best = member;
				bestIndex = index;
			}
		}

		if (best != null)
		{
			return best;
		}

		return preferRanged
			? RandomRangeTarget(candidates) ?? RandomMeleeTarget(candidates) ?? RandomMagicalTarget(candidates) ?? RandomPhysicalTarget(candidates)
			: RandomMeleeTarget(candidates) ?? RandomRangeTarget(candidates) ?? RandomMagicalTarget(candidates) ?? RandomPhysicalTarget(candidates);
	}
	internal static IBattleChara? RandomPhysicalTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, Job.VPR, Job.WAR, Job.GNB, Job.MNK, Job.SAM, Job.DRG, Job.MCH, Job.DNC)
			?? RandomPickByJobs(tars, Job.PLD, Job.DRK, Job.NIN, Job.BRD, Job.RDM)
			?? RandomObject(tars);
	}

	internal static IBattleChara? RandomMagicalTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, Job.PCT, Job.SCH, Job.AST, Job.SGE, Job.BLM, Job.SMN)
			?? RandomPickByJobs(tars, Job.PLD, Job.DRK, Job.NIN, Job.BRD, Job.RDM)
			?? RandomObject(tars);
	}

	internal static IBattleChara? RandomRangeTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, JobRole.RangedMagical, JobRole.RangedPhysical, JobRole.Melee)
			?? RandomPickByJobs(tars, JobRole.Tank, JobRole.Healer)
			?? RandomObject(tars);
	}

	internal static IBattleChara? RandomMeleeTarget(IEnumerable<IBattleChara> tars)
	{
		return RandomPickByJobs(tars, JobRole.Melee, JobRole.RangedMagical, JobRole.RangedPhysical)
			?? RandomPickByJobs(tars, JobRole.Tank, JobRole.Healer)
			?? RandomObject(tars);
	}

	private static IBattleChara? RandomPickByJobs(IEnumerable<IBattleChara> tars, params JobRole[] roles)
	{
		foreach (var role in roles)
		{
			var tar = RandomPickByJobs(tars, role.ToJobs());
			if (tar != null)
			{
				if (tar.IsConditionCannotTarget())
				{
					continue;
				}

				return tar;
			}
		}
		return null;
	}

	private static IBattleChara? RandomPickByJobs(IEnumerable<IBattleChara> tars, params Job[] jobs)
	{
		List<IBattleChara> targets = [];
		foreach (var t in tars)
		{
			if (t.IsConditionCannotTarget())
			{
				continue;
			}

			if (t.IsJobs(jobs))
			{
				targets.Add(t);
			}
		}
		return targets.Count > 0 ? RandomObject(targets) : null;
	}

	private static IBattleChara? RandomObject(IEnumerable<IBattleChara> objs)
	{
		List<IBattleChara> list = [];
		foreach (var o in objs)
		{
			if (o == null)
			{
				continue;
			}

			if (o.IsConditionCannotTarget())
			{
				continue;
			}

			list.Add(o);
		}

		var count = list.Count;
		if (count == 0)
		{
			return null;
		}

		var index = Random.Shared.Next(count);
		return list[index];
	}

	#endregion
}
