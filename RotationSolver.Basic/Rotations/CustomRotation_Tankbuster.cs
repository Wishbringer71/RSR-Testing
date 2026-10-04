namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// What the plan spends on the soonest measured tankbuster cast at the player.
	/// </summary>
	/// <param name="Cast">The tankbuster.</param>
	/// <param name="Mitigations">The own mitigations it spends; all of them when nothing survives.</param>
	/// <param name="Invulnerability">Nothing less survives the hit, and the invulnerability goes out.</param>
	/// <param name="Survivable">The plan survives the hit by the figures; false when even everything does not.</param>
	/// <param name="After">Share of maximum HP the hit takes under the plan.</param>
	private sealed record TankbusterPlan(TankbusterForecast.Cast Cast, IBaseAction[] Mitigations, bool Invulnerability,
		bool Survivable, float After);

	private readonly record struct PlanCandidate(IBaseAction Action, float Factor, float Barrier, float Cost);

	private static TankbusterPlan? _tankbusterPlan;
	private static readonly HashSet<uint> _plannedIds = [];
	private static (ulong Source, uint ActionId, float Total, string Summary) _planTraced;

	/// <summary>
	/// The owner's rule of 04.10.2026 (concept 09, "Das geringste Mittel gegen einen gemessenen
	/// Tankbuster"): against a tankbuster the table has measured, the least means that survives it without
	/// risk. The cheapest set of the own mitigations available by the hit - one, or several stacked when one
	/// is not enough; nothing when the hit is survivable as it stands. The invulnerability only when no set
	/// survives - "mit kanonen auf spatzen schießen" otherwise. When even that is not ready, everything.
	/// Recomputed every cycle, so a heal or a co-tank's Reprisal that lands before the hit shrinks the plan.
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
		if (cast == null)
		{
			return;
		}

		var gcd = DataCenter.DefaultGCDTotal;
		var candidates = PlanCandidates(cast, gcd);

		// Every subset, the cheapest that survives; a tank's rated actions are few, so this stays small.
		var bestMask = -1;
		var bestCost = float.MaxValue;
		var bestAfter = 0f;
		for (var mask = 0; mask < 1 << candidates.Count; mask++)
		{
			var after = After(cast, candidates, mask, out var cost, out var count);
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

		TankbusterPlan plan;
		if (bestMask >= 0)
		{
			plan = new TankbusterPlan(cast, Pick(candidates, bestMask), false, true, bestAfter);
		}
		else if (Service.Config.InvulnerabilityBeforeLethalTankbuster && InvulnerabilityReadyBy(cast.Remaining - gcd))
		{
			plan = new TankbusterPlan(cast, [], true, true, 0f);
		}
		else
		{
			var all = (1 << candidates.Count) - 1;
			plan = new TankbusterPlan(cast, Pick(candidates, all), false, false, After(cast, candidates, all, out _, out _));
		}

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
	/// button as the game casts it.
	/// </summary>
	private List<PlanCandidate> PlanCandidates(TankbusterForecast.Cast cast, float gcd)
	{
		var player = Player;
		Dictionary<uint, PlanCandidate> byButton = [];
		foreach (var action in AllBaseActions)
		{
			if (action == null || action.Info.IsRealGCD || !action.EnoughLevel || !action.Config.IsEnabled
				|| !action.Cooldown.WillHaveOneCharge(Math.Max(0f, cast.Remaining - gcd)))
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

			if (player == null || StandsAtImpact(action.ID, player, cast)
				|| !action.Info.BasicCheck(true, false, true, true))
			{
				continue;
			}

			var candidate = new PlanCandidate(action, (1f - self) * (1f - enemy), barrier,
				action.Cooldown.RecastTimeOneChargeRaw);
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

	private static float After(TankbusterForecast.Cast cast, List<PlanCandidate> candidates, int mask, out float cost, out int count)
	{
		var share = cast.Predicted;
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

	private static void TracePlan(TankbusterPlan plan)
	{
		var summary = plan.Invulnerability
			? "nothing less survives it: the invulnerability"
			: !plan.Survivable
				? $"nothing survives it, everything goes ({Names(plan.Mitigations)}), leaving {plan.After:P0} against {plan.Cast.Budget:P0}"
				: plan.Mitigations.Length == 0
					? "survivable as it stands, nothing spent"
					: $"{Names(plan.Mitigations)}, leaving {plan.After:P0} against {plan.Cast.Budget:P0}";
		var key = (plan.Cast.Source.GameObjectId, plan.Cast.ActionId, plan.Cast.Total, summary);
		if (_planTraced == key)
		{
			return;
		}

		_planTraced = key;
		DefenseTrace.Line($"tankbuster plan for #{plan.Cast.ActionId} from {plan.Cast.Source.Name.TextValue}"
			+ $" in {plan.Cast.Remaining:F1} s: {summary}");
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
	/// A refusal is not final - <c>CanUse</c> also refuses during an animation lock - so it is tried again
	/// every cycle; if nothing has gone out by the last GCD, the hold opens (<see cref="HoldDefenceForTankbuster"/>).
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

			return PressForTankbuster(invulnerability, plan, gcd, "invulnerability before a tankbuster nothing less survives", out act);
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

		// The action's own target type: a GCD path that returned early can leave its override standing
		// when the abilities run (TODO.md, "Zielüberschreibung"). The status check is skipped because the
		// plan has already asked whether this mitigation stands at impact; the game's own blocking groups
		// (one big mitigation at a time) are the stagger the plan replaces for a measured hit.
		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = null;
		try
		{
			if (!action.CanUse(out act, skipStatusProvideCheck: true, skipAoeCheck: true, usedUp: true))
			{
				act = null;
				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		DefenseTrace.Decision($"{why} for #{plan.Cast.ActionId} from {plan.Cast.Source.Name.TextValue}"
			+ $" in {plan.Cast.Remaining:F1} s", act);
		return true;
	}

	// Pressed within the last GCD, its status perhaps not on the player yet.
	private static bool InvulnerabilityJustUsed(IBaseAction invulnerability, float gcd)
	{
		return invulnerability.Cooldown.IsCoolingDown && !invulnerability.Cooldown.ElapsedAfter(gcd);
	}

	/// <summary>
	/// Whether the rated defence aimed at the player is held this cycle, and what of it stays free. Held
	/// entirely while Hallowed Ground or Superbolide keeps the coming hit off him, or while the
	/// invulnerability is planned for the hit - standing through it, just pressed, or still to be drawn
	/// with more than a GCD to go. Held except the planned mitigations while a survivable plan runs; in the
	/// last GCD before the hit, a planned mitigation still missing opens it. Not held when nothing survives
	/// the hit and the invulnerability is not ready: then everything goes. Read once per cycle into
	/// <see cref="IBaseAction.HoldDefenceOnSelf"/>. A command from the player is not second-guessed.
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
