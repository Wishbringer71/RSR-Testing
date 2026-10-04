namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// What the plan spends on the soonest measured tankbuster cast at the player.
	/// </summary>
	/// <param name="Cast">The tankbuster.</param>
	/// <param name="Mitigations">The own mitigations it spends; all of them when nothing survives.</param>
	/// <param name="Invulnerability">The invulnerability covers the hit - planned because nothing less
	/// survives it, or already standing over it; nothing else is spent.</param>
	/// <param name="Survivable">The plan survives the hit by the figures; false when even everything does not.</param>
	/// <param name="After">Share of maximum HP the hit takes under the plan.</param>
	private sealed record TankbusterPlan(TankbusterForecast.Cast Cast, IBaseAction[] Mitigations, bool Invulnerability,
		bool Survivable, float After);

	private readonly record struct PlanCandidate(IBaseAction Action, float Factor, float Barrier, float Cost);

	private static TankbusterPlan? _tankbusterPlan;
	private static readonly HashSet<uint> _plannedIds = [];
	private static (ulong Source, uint ActionId, float Total, string Summary) _planTraced;

	// Per cast: what the game or the rotation refused outside an animation lock - out of range, no MP, a
	// check of the rotation's own -, left out of the plan for one GCD and then tried again; and what the
	// plan pressed itself, which counts as standing until its status is on.
	private static (ulong Source, uint ActionId, float Total) _planCast;
	private static readonly Dictionary<uint, DateTime> _refused = [];
	private static readonly Dictionary<uint, DateTime> _pressed = [];

	/// <summary>
	/// The owner's rule of 04.10.2026 (concept 09, "Das geringste Mittel gegen einen gemessenen
	/// Tankbuster"): against a tankbuster the table has measured, the least means that survives it without
	/// risk. The cheapest set of the own mitigations available by the hit - one, or several stacked when one
	/// is not enough; nothing when the hit is survivable as it stands. The invulnerability only when no set
	/// survives - "mit kanonen auf spatzen schießen" otherwise - and then nothing else. Everything only when
	/// the invulnerability is not there to use and even all own mitigation together does not reach, in the
	/// hope of someone else's debuff or barrier (his precision). Recomputed every cycle, so a heal or a
	/// co-tank's Reprisal that lands before the hit shrinks the plan.
	/// </summary>
	/// <remarks>
	/// Cheapest by the recast of what is spent: what costs the least cooldown leaves the most for the
	/// next tankbuster (my reading of "geringstes notwendiges Mittel", not his rule). Risk-free by the
	/// forecast's figures: the highest hit ever measured, the HP now, only what still stands at impact.
	/// </remarks>
	private void UpdateTankbusterPlan()
	{
		_tankbusterPlan = null;
		_plannedIds.Clear();
		if (DataCenter.Role != JobRole.Tank
			|| (!Service.Config.InvulnerabilityBeforeLethalTankbuster && !Service.Config.HoldMitigationUnderInvulnerability))
		{
			return;
		}

		var cast = TankbusterForecast.SoonestMeasured;
		var castKey = cast == null ? default : (cast.Source.GameObjectId, cast.ActionId, cast.Total);
		if (_planCast != castKey)
		{
			// A new cast, or none: nothing of the last one carries over, not even its trace line.
			_planCast = castKey;
			_refused.Clear();
			_pressed.Clear();
			_planTraced = default;
		}

		if (cast == null)
		{
			return;
		}

		var gcd = DataCenter.DefaultGCDTotal;
		var invulnerability = Invulnerability;

		// An invulnerability already over the hit - drawn by this plan, or at its HP threshold - or pressed
		// a moment ago with its status not yet on: the hit is covered, nothing else is spent on it.
		if (TankbusterForecast.PlayerInvulnerableThrough(cast.Horizon)
			|| (invulnerability != null && InvulnerabilityJustUsed(invulnerability, gcd)))
		{
			SetPlan(new TankbusterPlan(cast, [], true, true, 0f));
			return;
		}

		var candidates = PlanCandidates(cast, gcd, out var pending);

		// What was pressed a moment ago but does not stand yet counts as standing.
		var predicted = cast.Predicted;
		foreach (var done in pending)
		{
			predicted = Math.Max(0f, (predicted * done.Factor) - done.Barrier);
		}

		// Every subset, the cheapest that survives; a tank's rated actions are few, so this stays small.
		var bestMask = -1;
		var bestCost = float.MaxValue;
		var bestAfter = 0f;
		for (var mask = 0; mask < 1 << candidates.Count; mask++)
		{
			var after = After(predicted, candidates, mask, out var cost, out var count);
			if (after >= cast.Budget)
			{
				continue;
			}

			if (cost < bestCost || (cost == bestCost && count < BitCount(bestMask)))
			{
				bestMask = mask;
				bestCost = cost;
				bestAfter = after;
			}
		}

		if (bestMask >= 0)
		{
			SetPlan(new TankbusterPlan(cast, Pick(candidates, bestMask), false, true, bestAfter));
		}
		else if (Service.Config.InvulnerabilityBeforeLethalTankbuster && invulnerability != null
			&& !Refused(invulnerability.ID, gcd) && InvulnerabilityReadyBy(cast.Remaining - gcd))
		{
			SetPlan(new TankbusterPlan(cast, [], true, true, 0f));
		}
		else
		{
			var all = (1 << candidates.Count) - 1;
			SetPlan(new TankbusterPlan(cast, Pick(candidates, all), false, false, After(predicted, candidates, all, out _, out _)));
		}
	}

	private static void SetPlan(TankbusterPlan plan)
	{
		_tankbusterPlan = plan;
		foreach (var action in plan.Mitigations)
		{
			_ = _plannedIds.Add(action.ID);
			_ = _plannedIds.Add(Service.GetAdjustedActionId(action.ID));
		}

		TracePlan(plan);
	}

	/// <summary>
	/// The own mitigations that can still act on the hit: learned, enabled, usable by their own checks
	/// (resources included), off cooldown by one GCD before the hit, lasting at least that GCD, rated by
	/// their effect text against this hit's damage type, and not already standing at impact. One per
	/// button as the game casts it. Party tools - a barrier or mitigation spread over the party, Shake It
	/// Off, Divine Veil, Dark Missionary, Heart of Light - stay out: they belong to the area defence, and
	/// Shake It Off dispels the tank's own Damnation and Bloodwhetting. Reprisal, a debuff on the enemy,
	/// stays in. <paramref name="pending"/>: mitigations this plan pressed within the last GCD whose status
	/// is not on yet - only its own presses, which went on the player; a cooldown alone says neither who
	/// pressed it nor on whom, and a shared recast (Vengeance and Damnation) would count twice.
	/// </summary>
	private List<PlanCandidate> PlanCandidates(TankbusterForecast.Cast cast, float gcd, out List<PlanCandidate> pending)
	{
		var player = Player;
		pending = [];
		Dictionary<uint, PlanCandidate> byButton = [];
		foreach (var action in AllBaseActions)
		{
			if (action == null || player == null || action.Info.IsRealGCD || !action.EnoughLevel
				|| !action.Config.IsEnabled || Refused(action.ID, gcd))
			{
				continue;
			}

			var lasts = DefensiveValues.DurationOf(action.ID);
			if (lasts > 0f && lasts < gcd)
			{
				continue;
			}

			var value = DefensiveValues.For(action.ID);
			var canSelf = action.Action.CanTargetSelf;
			var self = canSelf ? value.Self : 0f;
			var enemy = cast.AttackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot
				? value.EnemyPhysical
				: cast.AttackType == (uint)AttackType.Magic
					? value.EnemyMagical
					: Math.Min(value.EnemyPhysical, value.EnemyMagical);
			var barrier = canSelf ? value.Barrier : 0f;
			if (self <= 0f && enemy <= 0f && barrier <= 0f)
			{
				continue;
			}

			// A party tool: it reaches beyond the tank and protects the party, not against the enemy.
			if (action.Info.EffectRange > 0 && (self > 0f || barrier > 0f))
			{
				continue;
			}

			if (StandsAtImpact(action.ID, player, cast))
			{
				continue;
			}

			var candidate = new PlanCandidate(action, (1f - self) * (1f - enemy), barrier,
				action.Cooldown.RecastTimeOneChargeRaw);
			if (_pressed.TryGetValue(action.ID, out var pressedAt) && (DateTime.Now - pressedAt).TotalSeconds < gcd)
			{
				pending.Add(candidate);
				continue;
			}

			if (!action.Cooldown.WillHaveOneCharge(Math.Max(0f, cast.Remaining - gcd))
				|| !action.Info.BasicCheck(true, false, true, true))
			{
				continue;
			}

			var key = Service.GetAdjustedActionId(action.ID);
			if (!byButton.TryGetValue(key, out var known) || candidate.Factor < known.Factor
				|| (candidate.Factor == known.Factor && candidate.Barrier > known.Barrier))
			{
				byButton[key] = candidate;
			}
		}

		return [.. byButton.Values];
	}

	private static bool StandsAtImpact(uint actionId, IBattleChara player, TankbusterForecast.Cast cast)
	{
		if (!DefensiveValues.MitigatingStatusesByActionId.TryGetValue(actionId, out var statuses))
		{
			return false;
		}

		foreach (var status in statuses)
		{
			var id = (StatusID)status;
			if ((player.HasStatus(false, id) && !player.WillStatusEnd(cast.Horizon, false, id))
				|| (cast.Source.HasStatus(false, id) && !cast.Source.WillStatusEnd(cast.Horizon, false, id)))
			{
				return true;
			}
		}

		return false;
	}

	private static float After(float predicted, List<PlanCandidate> candidates, int mask, out float cost, out int count)
	{
		var share = predicted;
		var barriers = 0f;
		cost = 0f;
		count = 0;
		for (var i = 0; i < candidates.Count; i++)
		{
			if ((mask & (1 << i)) == 0)
			{
				continue;
			}

			share *= candidates[i].Factor;
			barriers += candidates[i].Barrier;
			cost += candidates[i].Cost;
			count++;
		}

		return Math.Max(0f, share - barriers);
	}

	private static IBaseAction[] Pick(List<PlanCandidate> candidates, int mask)
	{
		List<IBaseAction> picked = [];
		for (var i = 0; i < candidates.Count; i++)
		{
			if ((mask & (1 << i)) != 0)
			{
				picked.Add(candidates[i].Action);
			}
		}

		return [.. picked];
	}

	private static int BitCount(int mask)
	{
		return mask < 0 ? int.MaxValue : System.Numerics.BitOperations.PopCount((uint)mask);
	}

	/// <summary>Whether the invulnerability is learned, enabled and off cooldown within <paramref name="seconds"/>.</summary>
	private bool InvulnerabilityReadyBy(float seconds)
	{
		var invulnerability = Invulnerability;
		return invulnerability != null && invulnerability.Config.IsEnabled && invulnerability.EnoughLevel
			&& invulnerability.Cooldown.WillHaveOneCharge(Math.Max(0f, seconds));
	}

	// Pressed within the last GCD - by the plan or at its HP threshold -, its status perhaps not on yet.
	// The invulnerability has no shared recast and one charge, so its cooldown says who: the tank himself.
	private static bool InvulnerabilityJustUsed(IBaseAction invulnerability, float gcd)
	{
		return invulnerability.Cooldown.IsCoolingDown && invulnerability.Cooldown.JustUsedAfter(gcd);
	}

	private static bool Refused(uint actionId, float gcd)
	{
		return _refused.TryGetValue(actionId, out var at) && (DateTime.Now - at).TotalSeconds < gcd;
	}

	// One line per change of what the plan spends; the figures ride along but do not make a new line.
	private static void TracePlan(TankbusterPlan plan)
	{
		var summary = plan.Invulnerability
			? "the invulnerability, nothing else"
			: !plan.Survivable
				? $"nothing survives it and no invulnerability, everything goes ({Names(plan.Mitigations)})"
				: plan.Mitigations.Length == 0
					? "survivable as it stands, nothing spent"
					: Names(plan.Mitigations);
		var key = (plan.Cast.Source.GameObjectId, plan.Cast.ActionId, plan.Cast.Total, summary);
		if (_planTraced == key)
		{
			return;
		}

		_planTraced = key;
		DefenseTrace.Line($"tankbuster plan for #{plan.Cast.ActionId} from {plan.Cast.Source.Name.TextValue}"
			+ $" in {plan.Cast.Remaining:F1} s: {summary}"
			+ (plan.Invulnerability ? string.Empty : $", leaving {plan.After:P0} against {plan.Cast.Budget:P0}"));
	}

	private static string Names(IBaseAction[] actions)
	{
		List<string> names = [];
		foreach (var action in actions)
		{
			names.Add(action.Name);
		}

		return names.Count == 0 ? "nothing left" : string.Join(" + ", names);
	}

	/// <summary>
	/// The plan carried out: each planned mitigation once the hit is no further away than its own duration
	/// less one GCD (drawn earlier it would run out first; the GCD is the time the hit needs to arrive), the
	/// invulnerability likewise. Planned mitigation needs the plan option, the invulnerability its own.
	/// </summary>
	private bool TankbusterPlanAbility(out IAction? act)
	{
		act = null;
		var plan = _tankbusterPlan;
		if (plan == null)
		{
			return false;
		}

		var gcd = DataCenter.DefaultGCDTotal;
		if (plan.Invulnerability)
		{
			var invulnerability = Invulnerability;
			if (invulnerability == null || TankbusterForecast.PlayerInvulnerableThrough(plan.Cast.Horizon)
				|| InvulnerabilityJustUsed(invulnerability, gcd))
			{
				return false;
			}

			return PressForTankbuster(invulnerability, plan, gcd, "invulnerability, nothing less survives", out act);
		}

		if (!Service.Config.HoldMitigationUnderInvulnerability)
		{
			return false;
		}

		foreach (var action in plan.Mitigations)
		{
			if (PressForTankbuster(action, plan, gcd, plan.Survivable ? "tankbuster plan" : "tankbuster plan, everything", out act))
			{
				return true;
			}
		}

		return false;
	}

	private static bool PressForTankbuster(IBaseAction action, TankbusterPlan plan, float gcd, string why, out IAction? act)
	{
		act = null;
		var lasts = DefensiveValues.DurationOf(action.ID);
		if (lasts > 0f && plan.Cast.Remaining > lasts - gcd)
		{
			return false;
		}

		// Aimed at the player: the plan counted it on him (The Blackest Night would otherwise pick its own
		// target), and a GCD path that returned early can leave its override standing (TODO.md,
		// "Zielüberschreibung"). The status check is skipped because the plan has already asked whether
		// this mitigation stands at impact; the game's blocking group of big mitigations is the stagger the
		// plan replaces for a measured hit. A refusal while an animation lock runs is not an answer; one
		// outside it is the game's or the rotation's answer for this cast, and the plan is made again
		// without the action.
		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = null;
		try
		{
			if (!action.CanUse(out act, skipStatusProvideCheck: true, skipAoeCheck: true, usedUp: true,
				targetOverride: TargetType.Self))
			{
				act = null;
				if (ECommons.GameHelpers.Player.AnimationLock <= 0f && action.Cooldown.HasOneCharge)
				{
					_refused[action.ID] = DateTime.Now;
					DefenseTrace.Line($"{action.Name} refused for #{plan.Cast.ActionId}; planning without it for a GCD");
				}

				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		_pressed[action.ID] = DateTime.Now;

		DefenseTrace.Decision($"{why} for #{plan.Cast.ActionId} from {plan.Cast.Source.Name.TextValue}"
			+ $" in {plan.Cast.Remaining:F1} s", act);
		return true;
	}

	/// <summary>
	/// Whether the plan's tankbuster is the only one known to be coming at the player: no other cast at him,
	/// measured or not, and no BossModReborn tankbuster predicted before it. Otherwise the hold would also
	/// keep mitigation from a hit the plan does not know. A marker beside the cast is taken as the cast's.
	/// </summary>
	private static bool PlanCoversEveryHit(TankbusterPlan plan)
	{
		if (TankbusterForecast.Casts.Count != 1)
		{
			return false;
		}

		// A prediction known to be on someone else is not a hit on the player.
		return !DataCenter.BMRTankbusterImminent || DataCenter.BMRTankbusterHitsPlayer == false
			|| DataCenter.BMRNextTankbusterIn >= plan.Cast.Remaining - DataCenter.DefaultGCDTotal;
	}

	// An invulnerability that outlasts every cast now coming at the player covers them all.
	private static bool InvulnerabilityCoversEveryCast()
	{
		var latest = 0f;
		foreach (var cast in TankbusterForecast.Casts)
		{
			latest = Math.Max(latest, cast.Horizon);
		}

		return TankbusterForecast.PlayerInvulnerableThrough(latest);
	}

	/// <summary>
	/// Whether the rated defence aimed at the player is held this cycle, and what of it stays free. Held
	/// entirely while Hallowed Ground or Superbolide keeps the coming hit off him, or while the
	/// invulnerability covers the hit - standing through it, just pressed, or planned and still to be drawn
	/// with more than a GCD to go. Held except the planned mitigations while a survivable plan runs; in the
	/// last GCD before the hit, a planned mitigation still missing opens it. Not held when nothing survives
	/// the hit and the invulnerability is not there to use, nor while another tankbuster the plan does not
	/// know is coming. Read once per cycle into <see cref="IBaseAction.HoldDefenceOnSelf"/>. A command from
	/// the player is not second-guessed.
	/// </summary>
	private bool HoldDefenceForTankbuster()
	{
		IBaseAction.AllowedDefenceOnSelf = null;
		if (DataCenter.Role != JobRole.Tank || !Service.Config.HoldMitigationUnderInvulnerability
			|| DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle))
		{
			return false;
		}

		if (TankbusterForecast.PlayerImpervious)
		{
			return true;
		}

		var plan = _tankbusterPlan;
		if (plan == null)
		{
			return false;
		}

		// Never mitigation on top of an invulnerability that holds over everything coming.
		if (plan.Invulnerability && InvulnerabilityCoversEveryCast())
		{
			return true;
		}

		if (!PlanCoversEveryHit(plan))
		{
			return false;
		}

		var gcd = DataCenter.DefaultGCDTotal;
		if (plan.Invulnerability)
		{
			var invulnerability = Invulnerability;
			if (invulnerability == null)
			{
				return false;
			}

			if (TankbusterForecast.PlayerInvulnerableThrough(plan.Cast.Horizon) || InvulnerabilityJustUsed(invulnerability, gcd))
			{
				return true;
			}

			return plan.Cast.Remaining > gcd && invulnerability.Cooldown.HasOneCharge;
		}

		if (!plan.Survivable)
		{
			return false;
		}

		if (plan.Cast.Remaining <= gcd && plan.Mitigations.Length > 0)
		{
			// Planned but not pressed in time: the fallback is everything.
			return false;
		}

		IBaseAction.AllowedDefenceOnSelf = _plannedIds;
		return true;
	}
}
