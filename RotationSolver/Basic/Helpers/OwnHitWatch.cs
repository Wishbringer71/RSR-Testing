using System.Collections.Concurrent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What the player's own hits show: the damage one point of potency deals, and - for spells whose effect text says they do
/// nothing to an enemy of higher level, and for the states the player's spells put on - which enemy kinds they do nothing
/// to (concept 01, "Blaumagier", "Wert statt Reihenfolge", A507).
/// </summary>
/// <remarks>
/// <para>Damage per potency: a rule that weighs a fixed amount (1000 Needles' 1,000 points) or a share of an enemy's health
/// (Launcher) against a filler spell needs the filler's damage in points, and that depends on level, gear, level sync,
/// party buffs and Basic Instinct. Measured, it follows all of them. Counted are spells whose effect text states exactly one
/// potency and no condition, on their main target - the first enemy, which takes the full potency. The figure is the sum of
/// damage over the sum of potency since the level or the area last changed; critical and direct hits and the buffs a
/// filler would fall under are in it, which is what the comparison needs.</para>
///
/// <para>Immunity: an enemy kind (its base id) that a spell of the fixed-or-lethal class hit without damage, without a
/// status and without a miss does nothing to that class; a miss is the spell's low chance and says nothing. A state the game
/// reports as fully resisted (effect types 52 and 55) is resisted by the kind. Both are kept for the session: the learned
/// tables on disk carry revisions and quarantine (concept 13), and an error here costs one spell per kind and session.</para>
///
/// <para>Recognition only: no option and no threshold. The readers decide what a figure is worth.</para>
/// </remarks>
internal static class OwnHitWatch
{
	private static readonly object _gate = new();
	private static (int Level, uint Territory) _measuredAt;
	private static double _damage;
	private static double _potency;

	private static readonly ConcurrentDictionary<uint, byte> _flatImmuneKinds = new();
	private static readonly ConcurrentDictionary<(uint Kind, uint Status), byte> _resistedStates = new();
	private static readonly ConcurrentDictionary<(ulong Target, uint Action), bool> _lastLanded = new();
	private static readonly ConcurrentDictionary<uint, bool> _undeadKinds = new();

	/// <summary>
	/// The damage one point of potency has dealt on the main target, over the player's plain hits since the level or the area
	/// last changed; NaN when none has been measured there yet.
	/// </summary>
	public static float DamagePerPotency
	{
		get
		{
			lock (_gate)
			{
				return _potency > 0d && _measuredAt == (DataCenter.PlayerSyncedLevel(), DataCenter.TerritoryID)
					? (float)(_damage / _potency)
					: float.NaN;
			}
		}
	}

	/// <summary>
	/// Records one own hit on its main target. Only an action whose effect text states one potency without a condition
	/// counts (<see cref="ActionTooltip.TryPlainPotency"/>).
	/// </summary>
	/// <param name="actionId">The action that hit.</param>
	/// <param name="amount">The damage it dealt to its main target.</param>
	internal static void Hit(uint actionId, uint amount)
	{
		if (amount == 0 || !ActionTooltip.TryPlainPotency(actionId, out var potency) || potency <= 0f)
		{
			return;
		}

		var at = (DataCenter.PlayerSyncedLevel(), DataCenter.TerritoryID);
		bool first;
		lock (_gate)
		{
			if (_measuredAt != at)
			{
				_measuredAt = at;
				_damage = 0d;
				_potency = 0d;
			}

			first = _potency <= 0d;
			_damage += amount;
			_potency += potency;
		}

		if (first)
		{
			DefenseTrace.Line($"damage per potency: first measure at level {at.Item1} in area {at.Item2}: action {actionId}, {amount} damage at potency {potency:F0}");
		}
	}

	/// <summary>
	/// Records what one own spell did to one target, for the immunity of its kind.
	/// </summary>
	/// <param name="actionId">The spell.</param>
	/// <param name="target">The enemy it hit.</param>
	/// <param name="ignoresHigherLevelClass">Whether its effect text puts it in the fixed-or-lethal class
	/// (<see cref="ActionTooltip.IgnoresHigherLevel"/>).</param>
	/// <param name="damaged">It dealt damage.</param>
	/// <param name="statusApplied">It put a status on the target.</param>
	/// <param name="missed">The game reported a miss.</param>
	/// <param name="invulnerable">The target could not be affected right now - a phase, not an immunity of the kind.</param>
	/// <param name="resistedStatuses">The statuses the game reported as fully resisted.</param>
	/// <param name="promisedStatuses">The statuses the spell's text promises without a chance, for a spell that damaged the
	/// target and put no status on it: the game does not report every immune target (StunWatch), and such a state did not
	/// land.</param>
	internal static void Outcome(uint actionId, IBattleChara target, bool ignoresHigherLevelClass, bool damaged, bool statusApplied,
		bool missed, bool invulnerable, IReadOnlyList<uint> resistedStatuses, IReadOnlyList<uint> promisedStatuses)
	{
		if (target == null || target.BaseId == 0)
		{
			return;
		}

		foreach (var status in resistedStatuses)
		{
			if (_resistedStates.TryAdd((target.BaseId, status), 0))
			{
				DefenseTrace.Line($"state resisted: {target.Name.TextValue} (kind {target.BaseId}) resisted status {status} from action {actionId}"
					+ " - a chain that needs it no longer starts against this kind in this session");
			}
		}

		if (damaged && !statusApplied && !invulnerable && resistedStatuses.Count == 0)
		{
			foreach (var status in promisedStatuses)
			{
				if (_resistedStates.TryAdd((target.BaseId, status), 0))
				{
					DefenseTrace.Line($"state did not land: {target.Name.TextValue} (kind {target.BaseId}) took action {actionId} without status {status}"
						+ " - a chain that needs it no longer starts against this kind in this session");
				}
			}
		}

		if (ignoresHigherLevelClass && !damaged && !statusApplied && !missed && !invulnerable
			&& _flatImmuneKinds.TryAdd(target.BaseId, 0))
		{
			DefenseTrace.Line($"fixed or lethal spell without effect: action {actionId} on {target.Name.TextValue} (kind {target.BaseId})"
				+ " - no spell of that class goes against this kind in this session");
		}
	}

	/// <summary>
	/// Records whether an own spell put a status on one target when it last hit it, for a status that wears off with
	/// repeated use (Lightheaded): a hit without it says this enemy is immune for now, not its kind.
	/// </summary>
	internal static void Landed(ulong targetId, uint actionId, bool statusApplied)
	{
		if (!DataCenter.InCombat)
		{
			_lastLanded.Clear();
		}

		_lastLanded[(targetId, actionId)] = statusApplied;
	}

	/// <summary>
	/// Whether the spell put a status on this target when it last hit it; null when it has not hit it in this fight.
	/// </summary>
	public static bool? LastLanded(IBattleChara? target, uint actionId)
		=> target != null && _lastLanded.TryGetValue((target.GameObjectId, actionId), out var applied) ? applied : null;

	/// <summary>
	/// Records a hit of a spell whose text raises its potency against undead (Saintly Beam): the damage, against the measured
	/// damage per potency, says which of the two potencies it dealt - nearer the raised one, the kind is undead. Nothing
	/// is recorded while no damage per potency is measured.
	/// </summary>
	internal static void UndeadHit(uint actionId, IBattleChara target, uint amount, float basePotency, float undeadPotency)
	{
		var perPotency = DamagePerPotency;
		if (target == null || target.BaseId == 0 || amount == 0 || float.IsNaN(perPotency) || undeadPotency <= basePotency)
		{
			return;
		}

		var dealt = amount / perPotency;
		var undead = Math.Abs(dealt - undeadPotency) < Math.Abs(dealt - basePotency);
		if (_undeadKinds.TryAdd(target.BaseId, undead) && undead)
		{
			DefenseTrace.Line($"undead kind: {target.Name.TextValue} (kind {target.BaseId}) took {amount} from action {actionId}"
				+ $" - about {undeadPotency:F0} potency, not {basePotency:F0}");
		}
	}

	/// <summary>Whether this enemy's kind has been seen to take a spell's raised potency against undead.</summary>
	public static bool IsUndead(IBattleChara? target)
		=> target != null && _undeadKinds.TryGetValue(target.BaseId, out var undead) && undead;

	/// <summary>Whether a spell of the fixed-or-lethal class has been seen to do nothing to this enemy's kind.</summary>
	public static bool FlatOrLethalDoesNothing(IBattleChara? target)
		=> target != null && _flatImmuneKinds.ContainsKey(target.BaseId);

	/// <summary>Whether this enemy's kind has been seen to resist the status fully.</summary>
	public static bool Resists(IBattleChara? target, StatusID status)
		=> target != null && _resistedStates.ContainsKey((target.BaseId, (uint)status));
}
