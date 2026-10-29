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
	/// <param name="Covered">The invulnerability already stands over the hit (Z1), rather than being planned (Z3).</param>
	/// <param name="ReprisalKept">Reprisal was left out for the announced raidwide although it would have been cheaper
	/// (K6).</param>
	/// <param name="PressesLeft">The presses that fit before the deadline, when a set that survives did not fit into
	/// them; -1 otherwise.</param>
	private sealed record TankbusterPlan(TankbusterForecast.Cast Cast, IBaseAction[] Mitigations, bool Invulnerability,
		bool Survivable, float After, bool Covered = false, bool ReprisalKept = false, int PressesLeft = -1);

	// Reserved: a member another rule keeps for something else - Reprisal for the raidwide announced after its effect
	// and before its recast (K6). It goes only when no bundle without it survives.
	private readonly record struct PlanCandidate(IBaseAction Action, float Factor, float Barrier, float Cost, bool Refused,
		bool Reserved = false);

	private static readonly Dictionary<uint, StatusID[]> _statusesOfAction = [];

	private static TankbusterPlan? _tankbusterPlan;
	private static readonly HashSet<uint> _allowedNow = [];
	// The states of concept 09, "Die Zustände je Wirken", that a plan can be in.
	private enum PlanState
	{
		Covered,
		Bundle,
		Invulnerability,
		Everything,
	}

	private static (ulong Source, uint ActionId, float Total, PlanState State, int Members) _planTraced;

	// What the game or the rotation refused for a cast while neither an animation lock nor a cast of the
	// player's own explains it - out of range, no MP, a stun, a check of the rotation's own: left out of the
	// plan for one GCD, then tried again. The entry stays until the cast ends, so the refusal is written once.
	private static readonly Dictionary<(int Cast, uint ActionId), long> _refused = [];

	// What the plan cannot do without this cycle (FindNeeded).
	private static readonly HashSet<IBaseAction> _needed = [];

	// Whether, for the bundle now planned, the invulnerability could take over before the last GCD - option on,
	// learned, ready by then, a retry left. A help that takes a needed member then costs no one's life.
	private static bool _invulnerabilityCanTakeOver;

	private static readonly List<(int, uint)> _endedKeys = [];

	// The cast for which the line "blocked until after the hit" was written.
	private static int _blockedTraced;

	// The cast for which the line "no healer alive, nothing held" was written.
	private static int _unhealedTraced;


	/// <summary>
	/// The owner's rule of 04.10.2026 (concept 09, "Gemessener Tankbuster: das geringste
	/// Mittel"): against a tankbuster the table has measured, the least means that survives it without
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
		_needed.Clear();
		_invulnerabilityCanTakeOver = false;

		// Refusals belong to casts still running.
		_endedKeys.Clear();
		foreach (var key in _refused.Keys)
		{
			if (!TankbusterForecast.IsRunning(key.Cast))
			{
				_endedKeys.Add(key);
			}
		}

		foreach (var key in _endedKeys)
		{
			_ = _refused.Remove(key);
		}


		if (DataCenter.Role != JobRole.Tank
			|| (!Service.Config.InvulnerabilityBeforeLethalTankbuster && !Service.Config.HoldMitigationUnderInvulnerability))
		{
			return;
		}

		var measured = TankbusterForecast.SoonestMeasured;
		if (measured == null)
		{
			_planTraced = default;
			return;
		}

		// The forecast read the cast at the end of the last frame. Its caster is taken from the object table now, by
		// id, not from the reference it kept: Dalamud keeps one object per table slot and points it at whatever
		// occupies the slot when anyone next walks the table, so a reference held across a frame can show another
		// enemy or one that has gone (A309). Gone, it casts nothing.
		if (ECommons.DalamudServices.Svc.Objects.SearchById(measured.SourceId) is not IBattleChara source)
		{
			_planTraced = default;
			return;
		}

		// On the clock of now: what the plan reads itself - statuses, cooldowns, own records - is read now, the
		// cast's remaining time at the forecast's last reading.
		var lag = TankbusterForecast.SinceReading;
		var cast = measured with
		{
			Source = source,
			Remaining = Math.Max(0f, measured.Remaining - lag),
			Horizon = Math.Max(0f, measured.Horizon - lag),
		};

		var gcd = DataCenter.DefaultGCDTotal;
		var invulnerability = Invulnerability;

		// Z1 (concept 09, "Die Zustände je Wirken"): an invulnerability over the hit - its status, or the
		// own press of it - and for this cast no mitigation goes out on top of it (invariant 2).
		if (InvulnerabilityCovers(cast, true))
		{
			SetPlan(new TankbusterPlan(cast, [], true, true, 0f, Covered: true));
			return;
		}

		// What holds every press back now: the animation lock, a cast of the player's own, or a block on every
		// ability (stun, amnesia) for the rest its status names - unknown counts as ending now. One lasting past the
		// end of the cast leaves nothing to do for it, and nothing held for it (concept 09, "Spielerweite Sperre und
		// Frist", rule 6).
		var free = ObjectHelper.GetInstantHealLeadTime();
		if (StatusHelper.PlayerAbilitiesLocked(out var lockedFor))
		{
			free = Math.Max(free, lockedFor);
		}

		if (free >= cast.Remaining)
		{
			if (_blockedTraced != cast.Serial)
			{
				_blockedTraced = cast.Serial;
				DefenseTrace.Line($"tankbuster #{cast.ActionId} from {cast.Source.Name.TextValue} in {cast.Remaining:F1} s:"
					+ $" every ability is blocked for {free:F1} s, nothing to do for it");
			}

			_planTraced = default;
			return;
		}

		// The presses of a mitigation that still reach the server before the snapshot (rule 3 and 5): a bundle is
		// chosen only if its members still to press fit into them.
		var slots = PressSlots(MitigationDeadline(cast), free);

		var candidates = PlanCandidates(cast, gcd, out var pending, out var standing);

		// Without the plan option nothing presses a bundle for the player; hoping the rotation will is not
		// "ohne Risiko" (his rule), so only what already stands or went out counts.
		if (!Service.Config.HoldMitigationUnderInvulnerability)
		{
			candidates.Clear();
		}

		// What went out on the player and lasts until the hit, its status not on yet, counts as standing.
		var predicted = cast.Predicted;
		foreach (var done in pending)
		{
			predicted = Math.Max(0f, (predicted * done.Factor) - done.Barrier);
		}

		// Every subset, the cheapest that survives; a tank's rated actions are few, so this stays small. A bundle
		// without a refused member is preferred; one with a member refused a moment ago still counts - the
		// candidates keep a refused member only while its refusal runs out before the last GCD, so a retry is
		// left before the fallback -, so a passing refusal never draws the invulnerability (invariant 1).
		// Before that, a bundle without a reserved member: Reprisal kept for the announced raidwide goes on the
		// tankbuster only when nothing without it survives - the single-target defence's rule (A244), which the
		// plan, pressing on its own path, did not know (K6, A302).
		// Survived means left above the line at which the tank's own invulnerability is drawn ("The HP% for tank
		// to use invulnerability", read by every tank's EmergencyAbility): a bundle that leaves him at or below it
		// draws the invulnerability right after the hit anyway, so both are spent - and in the meantime he stands
		// in front of the next auto-attack with almost nothing (A304). At the line 0 this is the old reckoning.
		var budget = cast.Budget - Math.Max(0f, HealthForDyingTanks);
		var reprisalKept = false;
		var bestMask = BestBundle(candidates, predicted, budget, false, false, slots, out var bestAfter);
		if (bestMask >= 0)
		{
			reprisalKept = BestBundle(candidates, predicted, budget, false, true, slots, out _) is var cheapest
				&& cheapest >= 0 && cheapest != bestMask;
		}
		else
		{
			bestMask = BestBundle(candidates, predicted, budget, false, true, slots, out bestAfter);
		}

		if (bestMask < 0)
		{
			bestMask = BestBundle(candidates, predicted, budget, true, true, slots, out bestAfter);
		}

		// A set that survives but does not fit before the deadline: the plan names the presses left.
		var pressesLeft = bestMask < 0 && slots < candidates.Count
			&& BestBundle(candidates, predicted, budget, true, true, int.MaxValue, out _) >= 0
			? slots
			: -1;

		// A refusal of the invulnerability never makes "everything" (the owner's note, 12.10.2026): what refuses it
		// in the game - a stun, amnesia - refuses every other ability alike, so nothing else would go out either.
		// What RSR knows beforehand is availability (InvulnerabilityReadyBy), not a refusal.
		var invulnerabilityAvailable = Service.Config.InvulnerabilityBeforeLethalTankbuster && invulnerability != null
			&& InvulnerabilityReadyBy(cast.Remaining - gcd);
		if (bestMask >= 0)
		{
			SetPlan(new TankbusterPlan(cast, Pick(candidates, bestMask), false, true, bestAfter, ReprisalKept: reprisalKept));
			FindNeeded(candidates, bestMask, pending, standing, predicted, budget, slots);
			_invulnerabilityCanTakeOver = invulnerabilityAvailable;
		}
		else if (invulnerabilityAvailable)
		{
			SetPlan(new TankbusterPlan(cast, [], true, true, 0f, PressesLeft: pressesLeft));
		}
		else
		{
			// Everything, the most valuable member first: when not all of them fit before the deadline, what goes
			// out takes the most off the hit (rule 5).
			candidates.Sort((a, b) => Value(b, predicted).CompareTo(Value(a, predicted)));
			var all = (1 << candidates.Count) - 1;
			SetPlan(new TankbusterPlan(cast, Pick(candidates, all), false, false, After(predicted, candidates, all, out _, out _),
				PressesLeft: pressesLeft));

			// Everything goes, in the hope of someone else's help (his precision): every member and every
			// own mitigation that stands is needed.
			foreach (var c in candidates)
			{
				_needed.Add(c.Action);
			}

			foreach (var c in pending)
			{
				_needed.Add(c.Action);
			}

			foreach (var c in standing)
			{
				_needed.Add(c.Action);
			}
		}
	}

	// What a member takes off the hit by itself: its share of the predicted hit, and its barrier.
	private static float Value(PlanCandidate candidate, float predicted)
	{
		return (predicted * (1f - candidate.Factor)) + candidate.Barrier;
	}

	/// <summary>
	/// Seconds from now until the last moment a mitigation pressed for <paramref name="cast"/> still reaches the
	/// server before the snapshot at the end of the cast: the cast's rest less the measured round trip of a press
	/// (concept 09, "Spielerweite Sperre und Frist", rule 3). Before the first measurement, the cast's rest. Not for
	/// the invulnerability: Holmgang's floor counts when the damage is dealt, after the snapshot, and for the others
	/// it is not known; pressed until the cast ends, it may still hold, and a death it does not prevent costs nothing
	/// more.
	/// </summary>
	private static float MitigationDeadline(TankbusterForecast.Cast cast)
	{
		return cast.Remaining - Math.Max(0f, TankbusterForecast.RoundTrip);
	}

	// How many presses fit before the deadline: the first once what holds them back now ends, each next one after the
	// spacing a press keeps (measured). Before the first measurement the plan does not count them (int.MaxValue).
	private static int PressSlots(float deadline, float free)
	{
		var spacing = TankbusterForecast.PressSpacing;
		if (spacing <= 0f)
		{
			return int.MaxValue;
		}

		return deadline <= free ? 0 : (int)Math.Ceiling((deadline - free) / spacing);
	}

	// The cheapest bundle that leaves less than the budget and whose members fit into the presses left, ties to the
	// smaller one; -1 when none does.
	private static int BestBundle(List<PlanCandidate> candidates, float predicted, float budget, bool withRefused,
		bool withReserved, int slots, out float bestAfter)
	{
		var bestMask = -1;
		var bestCost = float.MaxValue;
		bestAfter = 0f;
		for (var mask = 0; mask < 1 << candidates.Count; mask++)
		{
			if ((!withRefused && HasRefused(candidates, mask)) || (!withReserved && HasReserved(candidates, mask))
				|| BitCount(mask) > slots)
			{
				continue;
			}

			var after = After(predicted, candidates, mask, out var cost, out var count);
			if (after >= budget)
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

		return bestMask;
	}

	/// <summary>
	/// The plan's members and the own mitigation standing or on its way that the hit cannot be survived
	/// without - no bundle without it survives, the invulnerability not counted as a replacement (concept 09,
	/// "Andere Pfade, die dem Plan ein Glied nehmen", K1-K4). What takes one of them away - Shake It Off dispelling it, help
	/// for another member on the same button, recast group or gauge - waits for it. A replacement counts only if it
	/// fits into the presses left before the deadline (<paramref name="slots"/>).
	/// </summary>
	private static void FindNeeded(List<PlanCandidate> candidates, int bestMask, List<PlanCandidate> pending,
		List<PlanCandidate> standing, float predicted, float budget, int slots)
	{
		for (var i = 0; i < candidates.Count; i++)
		{
			if ((bestMask & (1 << i)) != 0 && BestBundle(Without(candidates, i), predicted, budget, true, true, slots, out _) < 0)
			{
				_needed.Add(candidates[i].Action);
			}
		}

		// What already stands or went out: the hit without it - its factor taken back out of the figure, its
		// barrier off the budget (a pending one was taken off the figure).
		foreach (var c in pending)
		{
			if (c.Factor > 0f && BestBundle(candidates, (predicted + c.Barrier) / c.Factor, budget, true, true, slots, out _) < 0)
			{
				_needed.Add(c.Action);
			}
		}

		foreach (var c in standing)
		{
			if (c.Factor > 0f && BestBundle(candidates, predicted / c.Factor, budget - c.Barrier, true, true, slots, out _) < 0)
			{
				_needed.Add(c.Action);
			}
		}
	}

	private static List<PlanCandidate> Without(List<PlanCandidate> candidates, int index)
	{
		List<PlanCandidate> rest = [];
		for (var i = 0; i < candidates.Count; i++)
		{
			if (i != index)
			{
				rest.Add(candidates[i]);
			}
		}

		return rest;
	}

	/// <summary>
	/// Whether the plan for the measured tankbuster coming at you cannot do without <paramref name="action"/>:
	/// a member no other surviving bundle replaces, or own mitigation standing or on its way that the hit
	/// would not be survived without (concept 09, K1-K4). False with no plan, under an invulnerability, and
	/// for an action the plan does not count.
	/// </summary>
	protected static bool TankbusterPlanNeeds(IBaseAction? action)
	{
		if (action == null)
		{
			return false;
		}

		foreach (var needed in _needed)
		{
			if (needed.ID == action.ID || needed.AdjustedID == action.AdjustedID)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The triage of his rule for Nascent Flash, for every help that takes a member a measured tankbuster
	/// needs (concept 09, K2-K4): a healer in the critical class wins ("fällt der Heiler, fällt die Gruppe").
	/// Another tank in the critical class wins only while your invulnerability can take over and you are not
	/// critical yourself - then both live. Without it he does not: your death without the member is measured
	/// and certain, his critical state an uncertain one, and trading the certain for the uncertain is not
	/// necessarily good for the group (his answer to D3). A damage dealer does not. When the help goes, the
	/// plan reckons without the member and draws the invulnerability if nothing less survives.
	/// </summary>
	internal static bool HelpWinsTriage(IBattleChara member)
	{
		if (!member.IsInCriticalClass())
		{
			return false;
		}

		if (member.IsJobCategory(JobRole.Healer))
		{
			return true;
		}

		return member.IsJobCategory(JobRole.Tank) && _invulnerabilityCanTakeOver && !(Player?.IsInCriticalClass() ?? false);
	}

	/// <summary>
	/// <paramref name="help"/> on another party member a tankbuster is aimed at - marker or targeted cast
	/// (DataCenter.IsTankbusterAimedAt) -, never on the player, whom his own defence and its options cover. The level
	/// "Tanks" of concept 09, "Hilfe für das Ziel eines Tankbusters": each tank names its help and the option it
	/// stands under; the triage against his own measured need runs in <c>BaseAction.CanUse</c> (K2-K4).
	/// </summary>
	/// <param name="help">The action that protects another member.</param>
	/// <param name="act">The action, when it can be used.</param>
	/// <param name="tanksOnly">Only another tank, where the setting text says co-tank.</param>
	/// <param name="alsoTakes">A further condition of the rotation on the member, such as its own triage.</param>
	/// <param name="skipTargetStatusNeedCheck">Skip the action's own status condition on the target.</param>
	protected static bool HelpTankbusterTarget(IBaseAction help, out IAction? act, bool tanksOnly = false,
		Func<IBattleChara, bool>? alsoTakes = null, bool skipTargetStatusNeedCheck = false)
	{
		// The target filter is swapped in for this call only and restored afterwards, as for Nascent Flash.
		var setting = help.Setting;
		var canTarget = setting.CanTarget;
		var playerId = Player?.GameObjectId ?? 0;
		setting.CanTarget = t => canTarget(t) && t.GameObjectId != playerId
			&& (!tanksOnly || t.IsJobCategory(JobRole.Tank)) && (alsoTakes == null || alsoTakes(t));
		try
		{
			return help.CanUse(out act, skipTargetStatusNeedCheck: skipTargetStatusNeedCheck, targetOverride: TargetType.Tankbuster);
		}
		finally
		{
			setting.CanTarget = canTarget;
		}
	}

	/// <summary>Whether a plan stands for a measured tankbuster cast at you (any state).</summary>
	protected static bool TankbusterPlanActive => _tankbusterPlan != null;

	/// <summary>
	/// Whether the measured area cast now coming costs the group more, by the figures, than the one tank a
	/// tankbuster kills without a needed member: it kills a healer ("fällt der Heiler, fällt die Gruppe"), or
	/// more than one member. The owner's measure for a needed member against the party: the best result for
	/// the group (concept 09, K1).
	/// </summary>
	protected static bool AnnouncedAreaHitCostsMoreThanATank
	{
		get
		{
			var killed = DataCenter.AnnouncedHitKills(out var healer);
			return healer || killed > 1;
		}
	}

	/// <summary>
	/// Whether <paramref name="help"/>, aimed at another member, takes away something the plan needs: the
	/// same button with no second charge (The Blackest Night, Heart of Corundum, Oblation on its last
	/// charge), the same recast group (Nascent Flash and Bloodwhetting), or the same gauge (Intervention,
	/// Cover and Holy Sheltron spend the Oath Gauge) - read from the cooldown and the action sheet's cost
	/// type, not from a list (concept 09, K2-K4).
	/// </summary>
	internal static bool HelpTakesNeededMember(IBaseAction help)
	{
		foreach (var needed in _needed)
		{
			if (needed.ID == help.ID || needed.AdjustedID == help.AdjustedID)
			{
				if (help.Cooldown.CurrentCharges <= 1)
				{
					return true;
				}

				continue;
			}

			if (needed.Cooldown.CoolDownGroup == help.Cooldown.CoolDownGroup
				|| (help.Action.PrimaryCostType != 0 && needed.Action.PrimaryCostType == help.Action.PrimaryCostType))
			{
				return true;
			}
		}

		return false;
	}

	private static bool HasReserved(List<PlanCandidate> candidates, int mask)
	{
		for (var i = 0; i < candidates.Count; i++)
		{
			if ((mask & (1 << i)) != 0 && candidates[i].Reserved)
			{
				return true;
			}
		}

		return false;
	}

	private static bool HasRefused(List<PlanCandidate> candidates, int mask)
	{
		for (var i = 0; i < candidates.Count; i++)
		{
			if ((mask & (1 << i)) != 0 && candidates[i].Refused)
			{
				return true;
			}
		}

		return false;
	}

	private static void SetPlan(TankbusterPlan plan)
	{
		_tankbusterPlan = plan;
		TracePlan(plan);
	}

	/// <summary>
	/// The own mitigations that can still act on the hit: learned, enabled, usable by their own checks
	/// (resources included), off cooldown by one GCD before the hit, lasting more than two GCD (their window
	/// has to open before the last GCD), rated by their effect text against this hit's damage type, and not
	/// already standing at impact. One per button as the game casts it. Party tools - a barrier or mitigation
	/// spread over the party, Shake It Off, Divine Veil, Dark Missionary, Heart of Light - stay out: they belong
	/// to the area defence, and Shake It Off dispels the tank's own Damnation and Bloodwhetting. Reprisal, a
	/// debuff on the enemy, stays in. <paramref name="pending"/>: mitigations of the player pressed or gone out
	/// on him whose status the forecast has not seen yet and that stand at the hit (<see cref="PendingStands"/>)
	/// - asked before the status list, which can already show what the forecast's last reading did not count.
	/// A cooldown would say neither who pressed nor on whom, and Vengeance and Damnation share one. Reprisal
	/// states its duration only through its trait, from level 98 (A266); below that it is neither a candidate
	/// nor bridged and counts once its debuff is on the caster. From 98 it is a candidate, and the cheapest by
	/// recast, so it is reserved while the single-target defence would keep it for an announced raidwide (K6).
	/// </summary>
	private List<PlanCandidate> PlanCandidates(TankbusterForecast.Cast cast, float gcd, out List<PlanCandidate> pending,
		out List<PlanCandidate> standing)
	{
		standing = [];
		var player = Player;
		Dictionary<uint, PlanCandidate> byButton = [];
		Dictionary<uint, PlanCandidate> pendingByButton = [];
		foreach (var action in AllBaseActions)
		{
			if (action == null || player == null || action.Info.IsRealGCD)
			{
				continue;
			}

			var value = DefensiveValues.For(action.ID);
			var canSelf = action.Action.CanTargetSelf;

			// Own mitigation for this hit's damage type (Dark Mind halves differ by type), and what enlarges
			// the pool the hit meets: a barrier, or a raise of the maximum HP (Thrill of Battle, Great Nebula)
			// - the hit stays as large, so the raise works like a barrier of that share (concept 09, A266).
			// Against a learned hit after a marker the hitter is not known - a debuff on the target may not reach it -,
			// so a debuff is no member of the plan (A340).
			var self = canSelf ? value.SelfAgainst(cast.AttackType) : 0f;
			var enemy = cast.Learned
				? 0f
				: cast.AttackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot
					? value.EnemyPhysical
					: cast.AttackType == (uint)AttackType.Magic
						? value.EnemyMagical
						: Math.Min(value.EnemyPhysical, value.EnemyMagical);
			var barrier = canSelf ? value.Barrier + value.MaxHp : 0f;
			if (self <= 0f && enemy <= 0f && barrier <= 0f)
			{
				continue;
			}

			// A party tool: it reaches beyond the tank and protects the party, not against the enemy.
			if (action.Info.EffectRange > 0 && value.ProtectsBearer)
			{
				continue;
			}

			var refusal = RefusalLeft(cast, action.ID, gcd);
			var candidate = new PlanCandidate(action, (1f - self) * (1f - enemy), barrier,
				action.Cooldown.RecastTimeOneChargeRaw, refusal > 0f,
				action.ID == ReprisalPvE.ID && RaidwideAfterEffectBeforeRecast(ReprisalPvE, MitigationDebuffDuration));

			// One entry per button as the game casts it. What went out on the player is a fact whatever
			// would make it a candidate - switched off, refused, pressed by command (invariant 6). Asked before
			// the status list: the forecast reads once a frame, after the rotation, so a status that has just
			// shown is in the list but not yet in its figures, and the record still counts it.
			var key = action.AdjustedID;
			if (pendingByButton.ContainsKey(key))
			{
				continue;
			}

			if (PendingStands(key, action.Info.EffectRange > 0, cast))
			{
				pendingByButton[key] = candidate;
				_ = byButton.Remove(key);
				continue;
			}

			if (StandsAtImpact(action, player, cast))
			{
				standing.Add(candidate);
				continue;
			}

			// Invariant 5: its window (the hit no further away than its duration less a GCD and the series' span) has to
			// open before the last GCD, in which the hold ends for a missing member, so its duration has to be known and
			// longer than two GCD and the span. Below their trait level Reprisal and Sheltron state none (TODO.md) and stay
			// out. A refused member stays only while its refusal runs out before the last GCD (invariant 1).
			if (!action.EnoughLevel || !action.Config.IsEnabled
				|| WindowOpensAt(DefensiveValues.DurationOf(action.ID), cast, gcd, false) <= gcd
				|| (refusal > 0f && cast.Remaining - refusal <= gcd)
				|| !action.Cooldown.WillHaveOneCharge(Math.Max(0f, cast.Remaining - gcd))
				|| !action.Info.BasicCheck(true, false, true, true))
			{
				continue;
			}

			if (!byButton.TryGetValue(key, out var known) || candidate.Factor < known.Factor
				|| (candidate.Factor == known.Factor && candidate.Barrier > known.Barrier))
			{
				byButton[key] = candidate;
			}
		}

		pending = [.. pendingByButton.Values];
		return [.. byButton.Values];
	}

	// Whether the action's effect already stands over the horizon - on the player or, for a debuff, on the
	// caster -, from whoever put it there; read from the status list itself (no predicted status).
	private static bool StandsAtImpact(IBaseAction action, IBattleChara player, TankbusterForecast.Cast cast)
	{
		var key = DefensiveValues.MitigatingStatusesByActionId.ContainsKey(action.AdjustedID) ? action.AdjustedID : action.ID;
		if (!_statusesOfAction.TryGetValue(key, out var statuses))
		{
			if (!DefensiveValues.MitigatingStatusesByActionId.TryGetValue(key, out var tied))
			{
				return false;
			}

			statuses = new StatusID[tied.Length];
			for (var i = 0; i < tied.Length; i++)
			{
				statuses[i] = (StatusID)tied[i];
			}

			_statusesOfAction[key] = statuses;
		}

		return TankbusterForecast.LongestCopy(player, statuses, false) >= cast.Horizon
			|| (!cast.Learned && TankbusterForecast.LongestCopy(cast.Source, statuses, false) >= cast.Horizon);
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

	/// <summary>
	/// Whether the invulnerability is available within <paramref name="seconds"/>: everything RSR's own check
	/// would refuse it for that is known beforehand - learned and unlocked, enabled, on a slot, the action's
	/// time-to-kill threshold - and off cooldown by then. A refusal at the press can then come only from the game
	/// blocking every ability.
	/// </summary>
	private bool InvulnerabilityReadyBy(float seconds)
	{
		var invulnerability = Invulnerability;
		return invulnerability != null && invulnerability.Config.IsEnabled && invulnerability.Info.EnoughLevelAndQuest()
			&& invulnerability.Info.IsOnSlot && DataCenter.AverageTTK >= invulnerability.Config.TimeToKill
			&& invulnerability.Cooldown.WillHaveOneCharge(Math.Max(0f, seconds));
	}

	/// <summary>
	/// Whether an invulnerability on the player covers <paramref name="cast"/> - lasts over <see cref="InvulnerabilityHorizon"/>,
	/// from now -: its status, or - between the own press and the status showing - its record (<see cref="PendingStands"/>).
	/// Holmgang protects its user whatever it was aimed at.
	/// </summary>
	private bool InvulnerabilityCovers(TankbusterForecast.Cast cast, bool windowCounts)
	{
		return InvulnerabilityCovers(cast, InvulnerabilityHorizon(cast, DataCenter.DefaultGCDTotal), windowCounts);
	}

	private bool InvulnerabilityCovers(TankbusterForecast.Cast cast, float horizon, bool windowCounts)
	{
		var invulnerability = Invulnerability;
		var covered = cast with { Horizon = horizon, Span = 0f };
		return TankbusterForecast.PlayerInvulnerableThrough(horizon)
			|| (invulnerability != null && PendingStands(invulnerability.AdjustedID, true, covered, windowCounts));
	}

	/// <summary>
	/// H_I (concept 09, A454): how long an invulnerability has to last, from now, to cover <paramref name="cast"/> - until
	/// the series' last hit, or where none can reach that far, as far as one pressed at the last GCD reaches. Measured
	/// against the first hit, one pressed by hand before a series and ending before its last hit held every other defence
	/// back; measured against the last hit alone, one that cannot reach it never covered the series, and the plan pressed a
	/// bundle on top of its own for hits it already kept off.
	/// </summary>
	private float InvulnerabilityHorizon(TankbusterForecast.Cast cast, float gcd)
	{
		var lasts = Invulnerability is { } invulnerability ? DefensiveValues.DurationOf(invulnerability.ID) : 0f;
		return lasts > 0f ? Math.Min(cast.Horizon, cast.Remaining + lasts - gcd) : cast.Horizon;
	}

	/// <summary>
	/// The rest of the cast at which the window of an action of the plan opens: from then it lasts until the series' last
	/// hit - its duration <paramref name="lasts"/> less a GCD for the first hit to arrive and less the series' span (concept
	/// 09, invariant 5, "H_S", A454). The invulnerability's likewise, but at the latest at the last GCD, where Z3 stops
	/// holding the rest: it is measured against the first hit and takes what of the series falls into its duration.
	/// </summary>
	private static float WindowOpensAt(float lasts, TankbusterForecast.Cast cast, float gcd, bool invulnerability)
	{
		var opens = lasts - gcd - cast.Span;
		return invulnerability ? Math.Min(lasts - gcd, Math.Max(opens, gcd)) : opens;
	}

	/// <summary>
	/// Whether an own action whose status has not shown yet stands at the hit (concept 09, "Wann eine eigene
	/// Aktion als stehend zählt"): gone out inside its window for this cast - from the plan, another path or by
	/// hand, as the forecast noted at the press -, or lasting over the horizon by its duration from the press. Not when the own copy that ran at the press already lasts over the horizon - a
	/// renewal: that copy is in the forecast's figures already. <paramref name="windowCounts"/>: false when the
	/// horizon reaches past the cast the press was planned for, which its window does not cover.
	/// </summary>
	private static bool PendingStands(uint actionId, bool anyTarget, TankbusterForecast.Cast cast, bool windowCounts = true)
	{
		var pending = TankbusterForecast.OwnPending(actionId, anyTarget);
		if (pending.Cover <= 0f || pending.PriorLeft >= cast.Horizon)
		{
			return false;
		}

		return pending.Cover >= cast.Horizon || (windowCounts && pending.CoveredSerial == cast.Serial);
	}

	private static bool Refused(TankbusterForecast.Cast cast, uint actionId, float gcd)
	{
		return RefusalLeft(cast, actionId, gcd) > 0f;
	}

	// Seconds until a refusal of the action for this cast runs out; zero when none runs.
	private static float RefusalLeft(TankbusterForecast.Cast cast, uint actionId, float gcd)
	{
		return _refused.TryGetValue((cast.Serial, actionId), out var tick)
			? Math.Max(0f, gcd - TankbusterForecast.SecondsSince(tick, TankbusterForecast.Now))
			: 0f;
	}

	// One line per change of what the plan spends; the figures ride along but do not make a new line. The
	// line is built only on a change.
	private static void TracePlan(TankbusterPlan plan)
	{
		var state = plan.Covered ? PlanState.Covered
			: plan.Invulnerability ? PlanState.Invulnerability
			: !plan.Survivable ? PlanState.Everything
			: PlanState.Bundle;
		var members = 0;
		foreach (var action in plan.Mitigations)
		{
			members = HashCode.Combine(members, action.AdjustedID);
		}

		var key = (plan.Cast.Source.GameObjectId, plan.Cast.ActionId, plan.Cast.Total, state, members);
		if (_planTraced == key)
		{
			return;
		}

		_planTraced = key;
		var summary = plan.Covered
			? "the invulnerability already covers it, nothing else"
			: plan.Invulnerability
				? "nothing less survives it: the invulnerability"
				: !plan.Survivable
					? $"nothing survives it and no invulnerability, everything goes ({Names(plan.Mitigations)})"
					: plan.Mitigations.Length == 0
						? "survivable as it stands, nothing spent"
						: Names(plan.Mitigations) + (plan.ReprisalKept ? " (Reprisal kept for the announced raidwide)" : string.Empty);
		if (plan.PressesLeft >= 0)
		{
			summary += $" (a set that survives needs more than the {plan.PressesLeft} press(es) left before the snapshot)";
		}
		DefenseTrace.Line($"tankbuster plan for #{plan.Cast.ActionId} from {plan.Cast.Source.Name.TextValue}"
			+ $" in {plan.Cast.Remaining:F1} s: {summary}"
			+ (plan.Invulnerability ? string.Empty : $", leaving {plan.After:P0} against {plan.Cast.Budget:P0}"
				+ $" with the invulnerability line at {HealthForDyingTanks:P0}"));
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
			if (invulnerability == null || InvulnerabilityCovers(plan.Cast, true))
			{
				return false;
			}

			// Retried every cycle: a refusal here is a block on every ability (stun, amnesia), no answer about it.
			return PressForTankbuster(invulnerability, plan, gcd, "invulnerability, nothing less survives", out act, notesRefusal: false,
				isInvulnerability: true);
		}

		if (!Service.Config.HoldMitigationUnderInvulnerability)
		{
			return false;
		}

		// After the deadline a mitigation reaches the server only after the snapshot and changes nothing about the
		// hit (his note: "alles nach schadenseingang bringt auch nix").
		if (MitigationDeadline(plan.Cast) <= 0f)
		{
			return false;
		}

		foreach (var action in plan.Mitigations)
		{
			if (!Refused(plan.Cast, action.ID, gcd)
				&& PressForTankbuster(action, plan, gcd, plan.Survivable ? "tankbuster plan" : "tankbuster plan, everything", out act))
			{
				return true;
			}
		}

		return false;
	}

	private static bool PressForTankbuster(IBaseAction action, TankbusterPlan plan, float gcd, string why, out IAction? act,
		bool notesRefusal = true, bool isInvulnerability = false)
	{
		act = null;
		var lasts = DefensiveValues.DurationOf(action.ID);
		if (lasts > 0f && plan.Cast.Remaining > WindowOpensAt(lasts, plan.Cast, gcd, isInvulnerability))
		{
			return false;
		}

		// Aimed at the player: the plan counted it on him (The Blackest Night would otherwise pick its own
		// target), and a GCD path that returned early can leave its override standing (TODO.md,
		// "Zielüberschreibung"). The status check is skipped because the plan has already asked whether
		// this mitigation stands at impact; the game's blocking group of big mitigations is the stagger the
		// plan replaces for a measured hit. A refusal while an animation lock runs, the player casts or a block on
		// every ability stands (stun, amnesia) is not an answer about the action: retried in the next cycle. One
		// outside them leaves a mitigation out of the plan for one GCD (MP, range); the invulnerability is retried
		// every cycle instead (notesRefusal false).
		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = null;
		try
		{
			if (!action.CanUse(out act, skipStatusProvideCheck: true, skipAoeCheck: true, usedUp: true,
				targetOverride: TargetType.Self))
			{
				act = null;
				if (notesRefusal && ECommons.GameHelpers.Player.AnimationLock <= 0f && Player is { IsCasting: false }
					&& action.Cooldown.HasOneCharge && !StatusHelper.PlayerAbilitiesLocked(out _))
				{
					var key = (plan.Cast.Serial, action.ID);
					if (!_refused.ContainsKey(key))
					{
						DefenseTrace.Line($"{action.Name} refused for #{plan.Cast.ActionId}; planning without it for a GCD at a time");
					}

					_refused[key] = TankbusterForecast.Now;
				}

				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		DefenseTrace.Decision($"{why} for #{plan.Cast.ActionId} from {plan.Cast.Source.Name.TextValue}"
			+ $" in {plan.Cast.Remaining:F1} s"
			+ (TankbusterForecast.RoundTrip > 0f ? $", round trip {TankbusterForecast.RoundTrip:F2} s" : string.Empty), act);
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

		// BossModReborn names a target only for its soonest entry, so any prediction before the cast counts,
		// however far ahead - not only inside the window its own mitigation uses.
		var bmr = DataCenter.BMRNextTankbusterIn;
		return !Service.Config.UseBmrTimeline || bmr <= 0f || bmr == float.MaxValue
			|| bmr >= plan.Cast.Remaining - DataCenter.DefaultGCDTotal;
	}

	// An invulnerability that outlasts every cast now coming at the player covers them all. The casts' times
	// are taken to now, like the plan's.
	private bool InvulnerabilityCoversEveryCast(TankbusterPlan plan)
	{
		var lag = TankbusterForecast.SinceReading;
		var gcd = DataCenter.DefaultGCDTotal;
		var own = InvulnerabilityHorizon(plan.Cast, gcd);
		var latest = own;
		foreach (var cast in TankbusterForecast.Casts)
		{
			latest = Math.Max(latest, InvulnerabilityHorizon(cast with
			{
				Remaining = Math.Max(0f, cast.Remaining - lag),
				Horizon = Math.Max(0f, cast.Horizon - lag),
			}, gcd));
		}

		return InvulnerabilityCovers(plan.Cast, latest, latest <= own);
	}

	/// <summary>
	/// Whether the rated defence aimed at the player is held this cycle, and what of it stays free. Held
	/// entirely while Hallowed Ground or Superbolide keeps the coming hit off him, while an invulnerability
	/// lasts over every cast coming at him, or while it is planned for the only one and still to be drawn
	/// with more than a GCD to go. Held except the planned mitigations while a survivable plan runs for the
	/// only tankbuster known to come; in the last GCD before the hit, a planned mitigation still missing
	/// opens it. Not held when nothing survives the hit and the invulnerability is not there to use, nor
	/// while another tankbuster the plan does not know is coming. Read once per cycle into
	/// <see cref="IBaseAction.HoldDefenceOnSelf"/>. A command from the player frees what he commanded - the
	/// commanded action, and the single defence while he commands it -, not the other paths.
	/// </summary>
	private bool HoldDefenceForTankbuster()
	{
		IBaseAction.AllowedDefenceOnSelf = null;
		if (DataCenter.Role != JobRole.Tank || !Service.Config.HoldMitigationUnderInvulnerability)
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
		if (plan.Invulnerability && InvulnerabilityCoversEveryCast(plan))
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

			// Still to be drawn - held already while it is still coming off cooldown, as long as it will be ready
			// by its window, so nothing goes out on top of it.
			return plan.Cast.Remaining > gcd && InvulnerabilityReadyBy(plan.Cast.Remaining - gcd);
		}

		// A learned hit after a marker holds nothing but under the invulnerability, which keeps any hit off: the marker
		// may have announced another hit this time, and a bundle sized for the learned one would then leave the tank
		// with less than the marker's own defence gives him (A340). Nor does a cast neither list nor marker names a
		// tankbuster, known only from the table - an enemy's cast at its own target, learned from its landing (A452):
		// in a pull it comes from one of many enemies, and the rest of the defence is for their blows. Its plan still
		// spends what it needs, the invulnerability included.
		if (!plan.Survivable || plan.Cast.Learned || !plan.Cast.Announced)
		{
			return false;
		}

		// No healer alive: what the hit takes beyond the cheapest set is lost for good, and the next tankbuster meets
		// the tank with that much less - the reserve kept for it buys nothing then. The rest goes on this one too, where
		// it also covers the auto-attacks after it; the plan's own set still goes first. In the model he lives 28 %
		// longer after the healer's death against a boss with tankbusters (A361, tank_pull_defense_model.py).
		if (Service.Config.SpendDefenceWithoutHealer && !DataCenter.AnyLivingHealer(false))
		{
			if (_unhealedTraced != plan.Cast.Serial)
			{
				_unhealedTraced = plan.Cast.Serial;
				DefenseTrace.Line($"tankbuster #{plan.Cast.ActionId}: no healer alive, nothing held beside the plan");
			}

			return false;
		}

		// After the deadline nothing more goes out for this cast, and nothing is held for it (rule 4): what the
		// other paths press now, for the hits after it, they press for their own reasons.
		if (MitigationDeadline(plan.Cast) <= 0f)
		{
			return false;
		}

		if (plan.Cast.Remaining <= gcd && plan.Mitigations.Length > 0)
		{
			// Planned but not pressed in time: the fallback is everything.
			return false;
		}

		// A planned member goes out from any path only once its window is open (invariant 4); before, it
		// would run out ahead of the hit.
		_allowedNow.Clear();
		foreach (var action in plan.Mitigations)
		{
			if (plan.Cast.Remaining <= WindowOpensAt(DefensiveValues.DurationOf(action.ID), plan.Cast, gcd, false))
			{
				_ = _allowedNow.Add(action.ID);
				_ = _allowedNow.Add(action.AdjustedID);
			}
		}

		IBaseAction.AllowedDefenceOnSelf = _allowedNow;
		return true;
	}
}
