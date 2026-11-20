using ECommons.ExcelServices;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using AttackType = RotationSolver.Engine.Data.AttackType;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using static RotationSolver.Configuration.ConfigTypes;
using Player = ECommons.GameHelpers.Player;

namespace RotationSolver.Engine.Actions;

public partial struct ActionTargetInfo
{
	#region Target Result

	internal readonly TargetResult? FindTarget(bool skipAoeCheck, bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck, TargetType targetOverride)
	{
		if (action == null || action.Setting == null || action.Config == null)
		{
			return null;
		}

		if (Player.Object == null)
		{
			return null;
		}

		if (Range == 0 && EffectRange == 0)
		{
			// A fixed dash has no target to choose and lands here; its landing point is checked on this path or not at
			// all - every other check sits behind it, and until A498 no fixed dash was ever checked in combat.
			if (action.Setting.SpecialType is SpecialActionType.FixedDistanceMoveForward or SpecialActionType.FixedDistanceMoveBackward
				&& ShouldCheckMoveSafety() && !CheckMovementSafety(Player.Object.Position))
			{
				return null;
			}

			return new TargetResult(Player.Object, [], Player.Object.Position);
		}

		if (action.Setting.SpecialType == SpecialActionType.ObjectBasedMovement)
		{
			if (ShouldCheckMoveSafety() && action.Setting.ObjectBasedMovementObjectOID != 0)
			{
				if (!ObjectHelper.TryGetOwnedObjectPosition(action.Setting.ObjectBasedMovementObjectOID, out var position))
				{
					return null;
				}

				if (!CheckMovementSafety(position))
				{
					return null;
				}

				return new TargetResult(Player.Object, [], position);
			}
		}

		var type = action.Setting.TargetType;

		if (Range == 0 && EffectRange > 0 && !IsSingleTarget && !IsTargetArea && !action.Setting.IsFriendly)
		{
			if (Service.Config.AoEType == AoEType.Off)
			{
				return null;
			}

			var required = skipAoeCheck ? 0 : action.Config.AoeCount;

			if (required > 1 && (Service.Config.AoEType == AoEType.Cleave || (DataCenter.IsInM9S && Service.Config.M9SCleaveOnly)))
			{
				return null;
			}

			var selfAffects = GetCanAffects(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);
			selfAffects = FilterAffectsByFacing(selfAffects);
			// "Don't pull new enemies with AoE" holds for an area around the player as for any other (concept 03, A826).
			if (selfAffects.Count < Math.Max(1, required) || HitsUntouchable(Player.Object) || PullsAround(Player.Object, true))
			{
				return null;
			}

			return new TargetResult(Player.Object, [.. selfAffects], Player.Object.Position);
		}

		// The friendly counterpart: an area heal centred on the caster (Medica, Helios, Succor, Lux
		// Solaris and the rest - Range 0, an effect radius, no ground target). The general path below
		// looks for a heal *target* within Range, and with Range 0 that is the caster plus whoever's
		// hitbox touches theirs; the candidate then has to be under the action's own heal ratio. So a
		// healer standing full and a few yalms from the party never cast it, however hurt the party
		// was - the need was measured on the anchor instead of on the people the heal lands on.
		//
		// Here the anchor is the caster, always, and the need is read where the heal lands: the
		// hurt members inside the effect radius (GetCanAffects drops the full ones for a heal, as it
		// does for the general path), at least AoeCount of them, and at least one of those under
		// the heal ratio when the auto-heal check is on - the same two tests the general path applies,
		// moved from the anchor to the affected. A member held for a death trigger, or carried through
		// Walking Dead by his own attacks, is healed if he stands in the radius - an area heal cannot
		// skip him - but he never counts as the reason.
		//
		// Only for the heal target type, and not when a caller names the caster outright
		// (targetOverride Self), which the general path answers with the caster unconditionally. A
		// friendly Range-0 action asked for anything else keeps the general path, because only the
		// heal question depends on who needs it. The global AoE type (Off, Cleave, Full) does not
		// apply: it is about attacks (see GetMostCanTargetObjects). The action's own AoeCount does.
		if (Range == 0 && EffectRange > 0 && !IsSingleTarget && !IsTargetArea && action.Setting.IsFriendly
			&& type == TargetType.Heal && targetOverride != TargetType.Self)
		{
			// The dead and those who cannot be healed stay out, as GeneralHealTarget keeps them out:
			// a corpse reads 0 health, so it would count towards AoeCount and pass the heal ratio on
			// its own, and the cast would go to whoever stands next to it.
			var inRadius = GetCanAffects(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);
			inRadius.RemoveAll(member => member.IsDead || member.HasStatus(false, StatusHelper.HealingIneffectiveStatus));
			var required = skipAoeCheck ? 1 : Math.Max(1, (int)action.Config.AoeCount);
			if (inRadius.Count < required)
			{
				DataCenter.LastSelfCentredHeal = new(action.Name, inRadius.Count, required, false, DateTime.Now);
				return null;
			}

			var anyInNeed = false;
			foreach (var member in inRadius)
			{
				if (member.IsHeldForDeathTrigger() || member.WalkingDeadCarriedBySelfHeal(out _))
				{
					continue;
				}

				// The deficit healing can close on him: a member under a status that lowers it is no reason for an area heal
				// it barely reaches (concept 07, A544).
				if (!IBaseAction.AutoHealCheck || HealReduction.Restorable(member, member.GetForecastHealthRatio(InstantHeal)) < HealRatio)
				{
					anyInNeed = true;
					break;
				}
			}

			DataCenter.LastSelfCentredHeal = new(action.Name, inRadius.Count, required, anyInNeed, DateTime.Now);
			return anyInNeed ? new TargetResult(Player.Object, [.. inRadius], Player.Object.Position) : null;
		}

		IEnumerable<IBattleChara> canTargets = GetCanTargets(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);
		var canAffects = GetCanAffects(skipStatusProvideCheck, skipTargetStatusNeedCheck, type, targetOverride);

		// A Walking Dead bearer who is carried by his own attacks is supported with a HoT only, the
		// owner's rule (concept 09): a heal action that grants no HoT does not take him as its target
		// until StatusHelper.WalkingDeadCarriedBySelfHeal releases him. Here rather than in
		// FindHealTarget, because only this layer knows which action is asking, and FindTargetByType
		// is public and has no action to ask.
		if (type == TargetType.Heal && canTargets is List<IBattleChara> healCandidates && !GrantsSingleHot())
		{
			_ = healCandidates.RemoveAll(member => member.WalkingDeadCarriedBySelfHeal(out _));
		}

		if (canTargets == null || canAffects == null)
		{
			return null;
		}

		if (!action.Setting.IsFriendly && Service.Config.AoEType == AoEType.Off && !IsSingleTarget)
		{
			return null;
		}

		if (RuleTargetId != 0)
		{
			return FindCommandObjectTarget(RuleTargetId, canAffects, skipStatusProvideCheck, skipTargetStatusNeedCheck);
		}

		var commandTarget = DataCenter.GetCommandTarget(action);
		if (commandTarget is { ObjectId: not 0 } objectTarget
			&& FindCommandObjectTarget(objectTarget.ObjectId, canAffects, skipStatusProvideCheck, skipTargetStatusNeedCheck) is { } commandResult)
		{
			return commandResult;
		}

		if (IsTargetArea)
		{
			return FindTargetArea(canTargets, canAffects, Range, Player.Object, targetOverride);
		}

		List<IBattleChara> targetsList = [.. GetMostCanTargetObjects(canTargets, canAffects, skipAoeCheck ? 0 : action.Config.AoeCount)];

		var commandTargeting = commandTarget?.TargetingType;
		var dotOrder = DotSpreadOrder(skipStatusProvideCheck, targetOverride, commandTargeting);
		var target = FindTargetByType(targetsList, type, HealRatio, action.Setting.SpecialType, targetOverride, action.Setting.IsFriendly, InstantHeal, commandTargeting, dotOrder);
		if (target == null)
		{
			return null;
		}

		// Concept 11, "Tote, die nur die Wiederbelebung des Inhalts aufhebt" (A616): a job's raise leaves a corpse only the
		// duty's own raises bring back; the duty's raises take it.
		if (!action.Info.IsDutyAction && target.IsDead && target.OnlyDutyRaisesReach())
		{
			return null;
		}

		if (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position, target))
		{
			return null;
		}

		return new TargetResult(target, [.. GetAffectsTarget(target, canAffects)], target.Position);
	}

	private readonly TargetResult? FindCommandObjectTarget(ulong objectId, IEnumerable<IBattleChara> canAffects, bool skipStatusProvideCheck, bool skipTargetStatusNeedCheck)
	{
		if (Player.Object == null || Svc.Objects.SearchById(objectId) is not IBattleChara target)
		{
			return null;
		}

		if (IsTargetArea)
		{
			if (Vector3.Distance(Player.Object.Position, target.Position) > Range
				|| (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position)))
			{
				return null;
			}

			return new TargetResult(target, [.. GetAffectsVector(target.Position, canAffects)], target.Position);
		}

		if (target.DistanceToPlayer() > Range
			|| !IsInPvPPlayerRange(target)
			|| !CanUseTo(target)
			|| !CheckStatus(target, skipStatusProvideCheck, skipTargetStatusNeedCheck)
			|| !action.Setting.CanTarget(target)
			|| (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position, target)))
		{
			return null;
		}

		return new TargetResult(target, [.. GetAffectsTarget(target, canAffects)], target.Position);
	}

	private readonly TargetResult? FindTargetArea(IEnumerable<IBattleChara> canTargets, IEnumerable<IBattleChara> canAffects,
	float range, IPlayerCharacter player, TargetType targetOverride)
	{
		if (player == null || canTargets == null || canAffects == null)
		{
			return null;
		}

		if (action.Setting.TargetType == TargetType.Move)
		{
			return FindTargetAreaMove(range, targetOverride);
		}
		else
		{
			if (action.Setting.IsFriendly)
			{
				if (!Service.Config.UseGroundBeneficialAbility)
				{
					return null;
				}
				else
				{
					if (!Service.Config.UseGroundBeneficialAbilityWhenMoving && DataCenter.IsMoving)
					{
						return null;
					}
					else
					{
						return FindTargetAreaFriend(range, canAffects, player, targetOverride);
					}
				}
			}
			else
			{
				return FindTargetAreaHostile(canTargets, canAffects, action.Config.AoeCount, targetOverride);
			}
		}
	}

	/// <summary>
	/// Finds the hostile target area based on the specified criteria.
	/// </summary>
	/// <param name="canTargets">The potential targets that can be affected.</param>
	/// <param name="canAffects">The potential characters that can be affected.</param>
	/// <param name="aoeCount">The number of targets to consider for AoE.</param>
	/// <param name="targetOverride">Overrides the default target type for the action.</param>
	/// <returns>
	/// A <see cref="TargetResult"/> containing the target and affected characters, or <c>null</c> if no target is found.
	/// </returns>
	private readonly TargetResult? FindTargetAreaHostile(IEnumerable<IBattleChara> canTargets, IEnumerable<IBattleChara> canAffects, int aoeCount, TargetType targetOverride)
	{
		if (canAffects == null || canTargets == null)
		{
			return null;
		}

		List<IBattleChara> tiedAnchors = [.. GetMostCanTargetObjects(canTargets, canAffects, aoeCount)];
		if (tiedAnchors.Count == 0)
		{
			return null;
		}

		// Anchors can tie on hit count. Resolve the tie the same way the target-based AoE path does,
		// so a ground-targeted AoE anchors on the same enemy a target-based one would pick. Falling
		// back to the first tied anchor keeps the old guarantee that a non-empty tie always yields one.
		var target = FindTargetByType(tiedAnchors, action.Setting.TargetType, action.Config.AutoHealRatio,
			action.Setting.SpecialType, targetOverride, false) ?? tiedAnchors[0];

		List<IBattleChara> affectedTargets = [];
		foreach (var t in canAffects)
		{
			if (Vector3.Distance(target.Position, t.Position) - t.HitboxRadius <= EffectRange)
			{
				affectedTargets.Add(t);
			}
		}

		var affectedTargetsArray = new IBattleChara[affectedTargets.Count];
		for (var i = 0; i < affectedTargets.Count; i++)
		{
			affectedTargetsArray[i] = affectedTargets[i];
		}
		return new TargetResult(target, affectedTargetsArray, target.Position);
	}

	private readonly TargetResult? FindTargetAreaMove(float range, TargetType targetOverride)
	{
		if (Player.Object == null)
		{
			return null;
		}

		if (Service.Config.MoveAreaActionFarthest)
		{
			var pPosition = Player.Object.Position;
			if (Service.Config.MoveTowardsScreenCenter)
			{
				unsafe
				{
					var cameraManager = CameraManager.Instance();
					if (cameraManager == null)
					{
						return null;
					}

					var camera = cameraManager->CurrentCamera;
					var tar = camera->LookAtVector - camera->Object.Position;
					tar.Y = 0;
					var length = ((Vector3)tar).Length();
					if (length == 0)
					{
						return null;
					}

					tar = tar / length * range;
					var dest1 = new Vector3(pPosition.X + tar.X, pPosition.Y, pPosition.Z + tar.Z);
					if (ShouldCheckMoveSafety() && !CheckMovementSafety(dest1))
					{
						return null;
					}

					return new TargetResult(Player.Object, [], dest1);
				}
			}
			else
			{
				var rotation = Player.Object.Rotation;
				var dest2 = new Vector3(pPosition.X + ((float)Math.Sin(rotation) * range), pPosition.Y, pPosition.Z + ((float)Math.Cos(rotation) * range));
				if (ShouldCheckMoveSafety() && !CheckMovementSafety(dest2))
				{
					return null;
				}

				return new TargetResult(Player.Object, [], dest2);
			}
		}
		else
		{
			List<IBattleChara> availableCharas = [];
			foreach (var availableTarget in DataCenter.AllTargets)
			{
				if (availableTarget.GameObjectId != Player.Object.GameObjectId)
				{
					availableCharas.Add(availableTarget);
				}
			}

			var targetList = TargetFilter.GetObjectInRadius(availableCharas, range);
			var target = FindTargetByType(targetList, TargetType.Move, HealRatio, action.Setting.SpecialType, targetOverride, true);
			if (target == null)
			{
				return null;
			}

			if (ShouldCheckMoveSafety() && !CheckMovementSafety(target.Position))
			{
				return null;
			}

			return new TargetResult(target, [], target.Position);
		}
	}

	// An off-GCD heal lands as soon as the animation lock lets it go out, so its heal-ahead reading
	// looks no further than that; a GCD heal looks ahead by the rest of the GCD and its cast (A213).
	// An off-GCD heal that lands with the next GCD looks ahead like the GCD (A221).
	private readonly bool InstantHeal => !action.Info.IsRealGCD && !action.Setting.HealsWithNextGcd;

	/// <summary>
	/// Determines whether the movement safety check should be performed for the current action.
	/// </summary>
	/// <returns><c>true</c> if the movement safety check should be performed; otherwise, <c>false</c>.</returns>
	public readonly bool ShouldCheckMoveSafety()
		=> (IsMovingSpecialType(action.Setting.SpecialType))
		   && !action.Config.SkipPositionSafetyCheck && Service.Config.BmrSafetyCheckAuto;

	/// <summary>
	/// Returns true for any <see cref="SpecialActionType"/> that physically moves the character,
	/// regardless of whether the movement is a pure repositioning action or an attack with built-in movement.
	/// The one list: the action settings window reads it too, so a new movement type cannot be checked
	/// here and lack its "skip safety check" switch there (it did, for the backstep attacks of A193).
	/// </summary>
	internal static bool IsMovingSpecialType(SpecialActionType type)
		=> type is SpecialActionType.FixedDistanceMoveForward
				or SpecialActionType.FixedDistanceMoveBackward
				or SpecialActionType.HostileMovingForward
				or SpecialActionType.FriendlyMovingForward
				or SpecialActionType.HostileFriendlyMovingForward
				or SpecialActionType.HostileMovingAttack
				or SpecialActionType.ObjectBasedMovement
				or SpecialActionType.HostileAttackBackstep
				or SpecialActionType.GroundTargetMove;

	/// <summary>
	/// The movement types that leave the player at his target or ahead of where he stood: a dash to a target, a dash
	/// forward. Not the moves backwards (Elusive Jump grants the very Piercing Talon a ranged follow-up uses), nor a
	/// move to a placed object.
	/// </summary>
	internal static bool IsForwardMovingSpecialType(SpecialActionType type)
		=> type is SpecialActionType.FixedDistanceMoveForward
				or SpecialActionType.HostileMovingForward
				or SpecialActionType.HostileFriendlyMovingForward
				or SpecialActionType.HostileMovingAttack;

	private readonly bool CheckMovementSafety(Vector3 destination, IBattleChara? target = null)
	{
		if (Player.Object == null)
		{
			return false;
		}

		var safety = MovementSafety(destination, target);
		return safety.Safe || Refused(safety.Reason, safety.Measured);
	}

	/// <summary>
	/// The movement safety check as the combat path makes it, without recording a refusal: whether the move is safe, why
	/// not, and whether a landing point was measured at all. The action window shows this (A465), so what it shows is what
	/// decides in combat.
	/// </summary>
	/// <param name="destination">Where the move ends, for a move to a placed object; otherwise the player's position.</param>
	/// <param name="target">The target a dash or a backstep is measured against.</param>
	/// <returns>Safe, the reason when it is not, and whether a landing point was measured.</returns>
	internal readonly (bool Safe, string Reason, bool Measured) MovementSafety(Vector3 destination, IBattleChara? target = null)
	{
		if (Player.Object == null)
		{
			return (false, "the player is not available", false);
		}

		var playerPos = Player.Object.Position;
		var specialType = action.Setting.SpecialType;

		switch (specialType)
		{
			case SpecialActionType.FixedDistanceMoveForward:
			case SpecialActionType.FixedDistanceMoveBackward:
			{
				// The game moves the character along its facing by the distance the effect text states ("Quickly
				// dash 15 yalms forward"). The target is the player himself, so the destination handed in is where he
				// stands; the landing point is computed here (A313). Without a known distance there is nothing to
				// measure: refused while BossModReborn could answer, as before, allowed without it.
				var distance = action.Setting.BackstepDistance;
				if (distance <= 0f)
				{
					return (DataCenter.BMRIsFixedDashSafe == null,
						"the action's distance is not known, so its landing point is not either", false);
				}

				var facing = Player.Object.Rotation;
				var along = specialType == SpecialActionType.FixedDistanceMoveBackward ? -distance : distance;
				var landingPoint = new Vector3(playerPos.X + (MathF.Sin(facing) * along), playerPos.Y,
					playerPos.Z + (MathF.Cos(facing) * along));
				return WithoutFloor(landingPoint, distance)
					?? (DataCenter.IsFixedDashSafe(playerPos, landingPoint), "the landing point is in a danger zone", true);
			}

			case SpecialActionType.HostileMovingForward:
			case SpecialActionType.FriendlyMovingForward:
			case SpecialActionType.HostileFriendlyMovingForward:
			case SpecialActionType.HostileMovingAttack:
				if (target == null)
				{
					return (false, "no target to measure the dash against", false);
				}

				// Standing at the target, the dash does not move the player, and there is nothing
				// for this check to decide. Owner's report, Summoner: "wenn der beschwörer bereits
				// beim boss steht (0 yalm), dann wäre der gapcloser nur noch damage und kein risiko".
				// Asking whether the line to the target is safe measured a path that is never taken,
				// and an area under the boss withheld the ability from a player already standing in
				// it. Where he stands is a different question, and refusing the action does not
				// answer it: he is already there.
				//
				// 0 yalms means hitbox to hitbox, the distance the game shows (StandsAtTarget). This
				// used to exempt only a player whose centre was inside the target's ring, and kept
				// measuring a path shorter than his own hitbox when the rings merely touched. The
				// Summoner's fallback block reads the same measure. As soon as any distance remains,
				// the path to the hitbox edge is measured; how far a player may run up at all is the
				// rotation's own setting (for the Summoner AddCrimsonCyclone and CrimsonCycloneDistance).
				if (StandsAtTarget(target))
				{
					return (true, string.Empty, true);
				}

				var toTarget = target.Position - playerPos;
				var direction = toTarget / toTarget.Length();
				var finalDestination = target.Position - direction * target.HitboxRadius;
				return DataCenter.IsDashSafe(playerPos, finalDestination)
					? (true, string.Empty, true)
					: (false, $"the dash to {target.Name} crosses a danger zone ({target.DistanceToPlayer():F1} y)", true);

			case SpecialActionType.ObjectBasedMovement:
				return (DataCenter.IsDashSafe(playerPos, destination),
					"the way to the placed object crosses a danger zone", true);

			case SpecialActionType.GroundTargetMove:
				// The spot the area-move logic placed (FindTargetAreaMove) is where the move ends.
				return WithoutFloor(destination, Vector3.Distance(playerPos, destination))
					?? (DataCenter.IsDashSafe(playerPos, destination), "the way to the chosen spot crosses a danger zone", true);

			case SpecialActionType.HostileAttackBackstep:
				// The attack itself does not move the player towards the target; the backstep carries
				// him away from it, and that landing point is what can put him into an area. The
				// target is the reference for the direction: facing it is how the attack is made.
				if (target == null)
				{
					return (false, "no target to measure the backstep against", false);
				}

				var away = playerPos - target.Position;
				away.Y = 0;
				var length = away.Length();
				if (length <= 0f)
				{
					return (false, "standing on the target, the backstep has no direction", false);
				}

				var landing = playerPos + (away / length * action.Setting.BackstepDistance);
				return WithoutFloor(landing, action.Setting.BackstepDistance)
					?? (DataCenter.IsFixedDashSafe(playerPos, landing)
						? (true, string.Empty, true)
						: (false, $"the backstep from {target.Name} lands in a danger zone", true));

			default:
				return (true, string.Empty, false);
		}
	}

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

		TraceRefusal(action.Name, why);

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

	private readonly TargetResult? FindTargetAreaFriend(float range, IEnumerable<IBattleChara> canAffects, IPlayerCharacter player, TargetType targetOverride)
	{
		if (canAffects == null)
		{
			return null;
		}

		if (range == 0 || Service.Config.UseGroundBeneficialAbilityOnlySelf)
		{
			return new TargetResult(player, [.. GetAffectsVector(player.Position, canAffects)], player.Position);
		}

		if (Service.Config.UseTargetTankForGroundHeal && DataCenter.PartyMembers != null)
		{
			IBattleChara? tankWithStance = null;
			IBattleChara? tankWithoutStance = null;
			foreach (var member in DataCenter.PartyMembers)
			{
				if (member == null || member.GameObjectId == player.GameObjectId)
				{
					continue;
				}

				if (member.IsJobCategory(JobRole.Tank))
				{
					if (member.HasStatus(false, StatusHelper.TankStanceStatus))
					{
						tankWithStance = member;
						break;
					}
					else
					{
						tankWithoutStance ??= member;
					}
				}
			}
			var tankTarget = tankWithStance ?? tankWithoutStance;
			if (tankTarget != null)
			{
				List<IBattleChara> affectsList = [.. GetAffectsVector(tankTarget.Position, canAffects)];
				return new TargetResult(tankTarget, [.. affectsList], tankTarget.Position);
			}
		}

		var placed = PlaceGroundAreaFriend(range, canAffects, player, targetOverride);
		if (!Service.Config.GroundBeneficialCoverMost)
		{
			return placed;
		}

		// The spot at a party member that covers the most of them, if it covers more than the usual placement does: that
		// one asks only that someone stands in the area, and an arena centre or a boss holds the tank and the melee while
		// the party stacks elsewhere (A398).
		var covering = GroundAreaCoveringMost(range, canAffects, player);
		return covering is { } better && better.AffectedTargets.Length > (placed?.AffectedTargets.Length ?? 0) ? better : placed;
	}

	/// <summary>
	/// The spot within <paramref name="range"/> of the player, at one of <paramref name="canAffects"/>, whose area covers
	/// the most of them; among equals the one they stand closest around. Null when nobody can be covered.
	/// </summary>
	private readonly TargetResult? GroundAreaCoveringMost(float range, IEnumerable<IBattleChara> canAffects, IPlayerCharacter player)
	{
		TargetResult? best = null;
		var bestSpread = float.MaxValue;
		foreach (var member in canAffects)
		{
			if (member == null || member.IsDead)
			{
				continue;
			}

			var spot = member.Position;
			var toSpot = spot - player.Position;
			var length = toSpot.Length();
			if (length > range)
			{
				spot = player.Position + (toSpot / length * range);
			}

			List<IBattleChara> covered = [.. GetAffectsVector(spot, canAffects)];
			var spread = 0f;
			foreach (var c in covered)
			{
				spread += Vector3.Distance(spot, c.Position);
			}

			var count = best?.AffectedTargets.Length ?? 0;
			if (covered.Count > count || (covered.Count == count && covered.Count > 0 && spread < bestSpread))
			{
				best = new TargetResult(player, [.. covered], spot);
				bestSpread = spread;
			}
		}

		return best;
	}

	// The usual placement after the player's own choices: a stored point or the arena centre, the boss, a point by the
	// attacked member, the player himself.
	private readonly TargetResult? PlaceGroundAreaFriend(float range, IEnumerable<IBattleChara> canAffects, IPlayerCharacter player, TargetType targetOverride)
	{
		_ = OtherConfiguration.BeneficialPositions.TryGetValue(Svc.ClientState.TerritoryType, out var pts);
		pts ??= [];

		if (pts.Length == 0)
		{
			var isTrial = DataCenter.Territory?.ContentType == TerritoryContentType.Trials;
			var isRaid = DataCenter.Territory?.ContentType == TerritoryContentType.Raids;
			var partyCount = 0;
			if (DataCenter.PartyMembers != null)
			{
				foreach (var p in DataCenter.PartyMembers)
				{
					if (p is IPlayerCharacter)
					{
						partyCount++;
					}
				}
			}
			if (isTrial || (isRaid && partyCount >= 8))
			{
				Vector3[] fallbackPoints = [Vector3.Zero, new Vector3(100, 0, 100)];
				var closestFallback = fallbackPoints[0];
				var minDistance = Vector3.Distance(player.Position, fallbackPoints[0]);

				for (var i = 1; i < fallbackPoints.Length; i++)
				{
					var distance = Vector3.Distance(player.Position, fallbackPoints[i]);
					if (distance < minDistance)
					{
						closestFallback = fallbackPoints[i];
						minDistance = distance;
					}
				}

				pts = [closestFallback];
			}
		}

		if (pts.Length > 0)
		{
			var closest = pts[0];
			var minDistance = Vector3.Distance(player.Position, pts[0]);

			for (var i = 1; i < pts.Length; i++)
			{
				var distance = Vector3.Distance(player.Position, pts[i]);
				if (distance < minDistance)
				{
					closest = pts[i];
					minDistance = distance;
				}
			}

			var rotation = Random.Shared.NextDouble() * Math.Tau;
			var jitterRadius = (float)(Random.Shared.NextDouble() * MathF.Min(EffectRange * 0.5f, 2f));
			closest.X += (float)(Math.Sin(rotation) * jitterRadius);
			closest.Z += (float)(Math.Cos(rotation) * jitterRadius);

			var toDesired = closest - player.Position;
			var len = toDesired.Length();
			if (len > range)
			{
				closest = player.Position + toDesired / len * range;
			}

			if (Vector3.Distance(player.Position, closest) <= range + player.HitboxRadius)
			{
				List<IBattleChara> affectsList = [.. GetAffectsVector(closest, canAffects)];
				if (affectsList.Count > 0)
				{
					return new TargetResult(player, [.. affectsList], closest);
				}
			}
		}

		IBattleChara? bossTarget = null;
		if (Svc.Targets.Target is IBattleChara b && b.DistanceToPlayer() <= range && b.IsBossFromIcon()
			&& (b.HasPositional() || b.HitboxRadius <= 8))
		{
			bossTarget = b;
		}

		if (bossTarget == null && DataCenter.AllHostileTargets != null)
		{
			foreach (var hostile in DataCenter.AllHostileTargets)
			{
				if (hostile != null && hostile.IsBossFromIcon() && hostile.DistanceToPlayer() <= range
					&& (hostile.HasPositional() || hostile.HitboxRadius <= 8))
				{
					bossTarget = hostile;
					break;
				}
			}
		}

		if (bossTarget != null)
		{
			var bossPos = bossTarget.Position;
			var distToBoss = Vector3.Distance(player.Position, bossPos);

			Vector3 targetPos;
			if (distToBoss <= range)
			{
				targetPos = bossPos;
			}
			else if (distToBoss > 0.001f)
			{
				var toBoss = bossPos - player.Position;
				targetPos = player.Position + toBoss / distToBoss * range;
			}
			else
			{
				targetPos = player.Position;
			}

			List<IBattleChara> affectsList = [.. GetAffectsVector(targetPos, canAffects)];
			return new TargetResult(player, [.. affectsList], targetPos);
		}
		else
		{
			var effectRange = EffectRange;
			List<IBattleChara> partyMembersInRadius = [];
			if (DataCenter.PartyMembers != null)
			{
				foreach (var member in DataCenter.PartyMembers)
				{
					var dist = Vector3.Distance(member.Position, player.Position);
					if (dist <= range + effectRange)
					{
						partyMembersInRadius.Add(member);
					}
				}
			}
			var attackT = FindTargetByType(partyMembersInRadius,
				TargetType.BeAttacked, HealRatio, action.Setting.SpecialType, targetOverride, true, InstantHeal);

			if (attackT == null)
			{
				List<IBattleChara> affectsList = [.. GetAffectsVector(player.Position, canAffects)];
				return new TargetResult(player, [.. affectsList], player.Position);
			}
			else
			{
				var disToTankRound = Vector3.Distance(player.Position, attackT.Position) + attackT.HitboxRadius;

				Vector3 finalPos;
				if (disToTankRound < effectRange
					|| disToTankRound > (2 * effectRange) - player.HitboxRadius)
				{
					finalPos = player.Position;
				}
				else
				{
					var directionToTank = attackT.Position - player.Position;
					var dirLength = directionToTank.Length();
					if (dirLength < 0.001f)
					{
						finalPos = player.Position;
					}
					else
					{
						var moveDirection = directionToTank / dirLength * Math.Max(0, disToTankRound - effectRange);
						finalPos = player.Position + moveDirection;
					}
				}

				var toFinal = finalPos - player.Position;
				var toFinalLen = toFinal.Length();
				if (toFinalLen > range)
				{
					if (toFinalLen > 0.001f)
					{
						finalPos = player.Position + toFinal / toFinalLen * range;
					}
					else
					{
						finalPos = player.Position;
					}
				}

				if (float.IsNaN(finalPos.X) || float.IsNaN(finalPos.Y) || float.IsNaN(finalPos.Z) ||
					float.IsInfinity(finalPos.X) || float.IsInfinity(finalPos.Y) || float.IsInfinity(finalPos.Z))
				{
					finalPos = player.Position;
				}

				List<IBattleChara> affectsList = [.. GetAffectsVector(finalPos, canAffects)];
				return new TargetResult(player, [.. affectsList], finalPos);
			}
		}
	}

	private readonly IEnumerable<IBattleChara> GetAffectsVector(Vector3? point, IEnumerable<IBattleChara> canAffects)
	{
		if (canAffects == null)
		{
			yield break;
		}

		if (point == null)
		{
			yield break;
		}

		foreach (var t in canAffects)
		{
			if (Vector3.Distance(point.Value, t.Position) - t.HitboxRadius <= EffectRange)
			{
				yield return t;
			}
		}
	}

	private readonly List<IBattleChara> FilterAffectsByFacing(List<IBattleChara> candidates)
	{
		if (Player.Object == null || candidates.Count == 0)
		{
			return candidates;
		}

		var castType = SelfAimedShape ?? (CastType)action.Action.CastType;
		if (castType != CastType.Cone && castType != CastType.StraightLine && castType != CastType.Donut)
		{
			return candidates;
		}

		var playerPos = Player.Object.Position;
		var faceVec = Vector3.Normalize(Player.Object.GetFaceVector());
		faceVec.Y = 0;

		List<IBattleChara> result = [];
		foreach (var t in candidates)
		{
			var toTarget = t.Position - playerPos;
			toTarget.Y = 0;
			var dist = toTarget.Length();
			if (dist <= 0.001f)
			{
				result.Add(t);
				continue;
			}

			if (castType == CastType.Cone)
			{
				if (InFacingCone(faceVec, toTarget / dist))
				{
					result.Add(t);
				}
			}
			else if (castType == CastType.StraightLine)
			{
				if (Vector3.Dot(faceVec, toTarget) < 0)
				{
					continue;
				}

				var faceXZ = Vector3.Normalize(new Vector3(faceVec.X, 0, faceVec.Z));
				Vector3 toTargetXZ = new(toTarget.X, 0, toTarget.Z);
				var perp = Vector3.Cross(faceXZ, toTargetXZ).Length();
				var halfWidth = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier / 2f : 2f;
				var width = halfWidth + t.HitboxRadius;
				if (perp <= width)
				{
					result.Add(t);
				}
			}
			else if (castType == CastType.Donut)
			{
				var innerRadius = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier : 8f;
				var d = dist - t.HitboxRadius;
				if (d >= innerRadius)
				{
					result.Add(t);
				}
			}
		}
		return result;
	}

	private readonly IEnumerable<IBattleChara> GetAffectsTarget(IBattleChara tar, IEnumerable<IBattleChara> canAffects)
	{
		if (tar == null)
		{
			yield break;
		}

		if (canAffects == null)
		{
			yield break;
		}

		foreach (var t in canAffects)
		{
			if (GetCanTarget(tar, t))
			{
				yield return t;
			}
		}
	}

	#endregion
}
