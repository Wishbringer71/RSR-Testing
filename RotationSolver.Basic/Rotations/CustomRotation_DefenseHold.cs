namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	// The universal layer under every rule that holds back a defensive action (concept 08, "Die
	// Abwehrsperren"). The holds themselves are special rules of single jobs - a white mage stretching
	// its party mitigation, a dark knight keeping its burst window for damage, a machinist not weaving
	// during Overheat - and each is right only while the party is safe. That condition is the same for
	// all of them, so it is asked here once: the owner's order puts the survival of the party above
	// anything a hold saves (concept 09, "Jede Rückhaltung wird erst geprüft, wenn Stufe 1 gesichert
	// ist"), and a rule that sits on one job would leave the same situation open on every other.
	//
	// Not every early return is a hold of this kind. A hold that protects another safety rule of the
	// owner's (Swiftcast kept for a raise) or one the game imposes (no action possible at all) is not
	// a strategic choice and does not come through here.

	/// <summary>
	/// Whether a strategic hold of an area defense stands. <paramref name="wanted"/> is the job's own
	/// reason to hold; the hold yields when the party is in danger (<see cref="AreaDefenseDanger"/>).
	/// </summary>
	/// <param name="wanted">The job rule's own verdict that the defense should wait.</param>
	/// <param name="rule">What the hold is, in a few words, for the diagnostics window.</param>
	/// <returns>True while the hold stands.</returns>
	protected static bool HoldAreaDefense(bool wanted, string rule)
	{
		if (!wanted)
		{
			return false;
		}

		return Record("area", rule, !AreaDefenseDanger(out var why), why);
	}

	/// <summary>
	/// Whether a strategic hold of a single-target defense stands; it yields when the player or a
	/// tank is in danger (<see cref="SingleDefenseDanger"/>).
	/// </summary>
	/// <param name="wanted">The job rule's own verdict that the defense should wait.</param>
	/// <param name="rule">What the hold is, in a few words, for the diagnostics window.</param>
	/// <returns>True while the hold stands.</returns>
	protected static bool HoldSingleDefense(bool wanted, string rule)
	{
		if (!wanted)
		{
			return false;
		}

		return Record("single", rule, !SingleDefenseDanger(out var why), why);
	}

	/// <summary>
	/// The stretching of a job's own defense: after one of <paramref name="triggers"/> the rest waits
	/// until that trigger's effect has run out, so two hits in a row both get something instead of
	/// the first getting everything. Area scope; yields like <see cref="HoldAreaDefense"/>.
	/// </summary>
	/// <param name="rule">What the hold is, in a few words, for the diagnostics window.</param>
	/// <param name="triggers">The actions after which the job's area defense waits.</param>
	/// <returns>True while the stretch holds.</returns>
	protected static bool AreaDefenseStretched(string rule, params IBaseAction[] triggers)
	{
		return HoldAreaDefense(AnyTriggerInEffect(triggers), rule);
	}

	/// <summary>
	/// <see cref="AreaDefenseStretched"/> for single-target defense; yields like
	/// <see cref="HoldSingleDefense"/>.
	/// </summary>
	/// <param name="rule">What the hold is, in a few words, for the diagnostics window.</param>
	/// <param name="triggers">The actions after which the job's single-target defense waits.</param>
	/// <returns>True while the stretch holds.</returns>
	protected static bool SingleDefenseStretched(string rule, params IBaseAction[] triggers)
	{
		return HoldSingleDefense(AnyTriggerInEffect(triggers), rule);
	}

	// A trigger is in effect from its use until the duration its own effect text states, and only
	// while it has no charge back - a trigger that still holds a charge does not stretch, as the job
	// rules had it before. The use is taken from the record of the player's own actions, and the
	// charge from the action's recast group, never from the button: some buttons turn into another
	// action while the effect runs (Liturgy of the Bell's second press, Macrocosmos and Microcosmos),
	// and that other action's cooldown is not the trigger's. A text that states no duration never
	// stretches - a figure the game did not give is not invented.
	private static bool AnyTriggerInEffect(IBaseAction[] triggers)
	{
		foreach (var trigger in triggers)
		{
			var duration = DefensiveValues.DurationOf(trigger.ID);
			if (duration <= 0f || !trigger.EnoughLevel)
			{
				continue;
			}

			var cooldown = trigger.Cooldown;
			if (!ActionIdHelper.IsCoolingDownGroup(cooldown.CoolDownGroup))
			{
				continue;
			}

			// The recast of a charged action runs over all its charges, and the elapsed time counts the
			// charges already back: an elapsed time of at least one charge means a charge is ready.
			var oneCharge = cooldown.RecastTime / cooldown.MaxCharges;
			if (oneCharge <= 0f || cooldown.RecastTimeElapsedRaw >= oneCharge)
			{
				continue;
			}

			if (UsedWithin(trigger.ID, duration))
			{
				return true;
			}
		}

		return false;
	}

	// The record is newest first and ordered by time, so the walk stops at the first entry older
	// than the window. It is cleared with the fight, and so is every stretch.
	private static bool UsedWithin(uint actionId, float seconds)
	{
		var now = DateTime.Now;
		foreach (var record in DataCenter.RecordActions)
		{
			if ((now - record.UsedTime).TotalSeconds >= seconds)
			{
				return false;
			}

			if (record.Action.RowId == actionId)
			{
				return true;
			}
		}

		return false;
	}

	// One entry per scope and rule: several jobs ask the same rule for area and single-target
	// defense in one frame, and the two can answer differently.
	private static bool Record(string scope, string rule, bool held, string why)
	{
		var name = $"{rule} ({scope})";
		DataCenter.DefenseHolds[name] = new(name, held, held ? string.Empty : why, DateTime.Now);
		return held;
	}

	/// <summary>
	/// Whether an area defense must not wait: the single-target danger below, any living member in
	/// the heal chain's critical class (<see cref="ObjectHelper.IsInCriticalClass(IBattleChara)"/>,
	/// concept 07), or the announced area cast, as measured, would put an unprotected living member
	/// there.
	/// </summary>
	/// <remarks>
	/// The measured share is the hardest the cast has been seen to land, after whatever mitigation
	/// stood then, and only ever raised. The mitigation standing now is therefore not taken off it
	/// again - that would count it twice. The estimate errs towards "dangerous", so the hold yields
	/// rather too often than too late. A cast never measured answers only through the class test.
	/// The answer is kept for one frame: several holds ask it back to back.
	/// </remarks>
	/// <param name="why">Who is in danger, or empty.</param>
	/// <returns>True when a hold of an area defense must yield.</returns>
	internal static bool AreaDefenseDanger(out string why)
	{
		var now = Environment.TickCount64;
		if (_areaDangerTick == long.MinValue || now - _areaDangerTick >= DataCenter.FrameCacheMs)
		{
			_areaDangerTick = now;
			_areaDanger = FindAreaDanger(out _areaDangerWhy);
		}

		why = _areaDangerWhy;
		return _areaDanger;
	}

	private static long _areaDangerTick = long.MinValue;
	private static bool _areaDanger;
	private static string _areaDangerWhy = string.Empty;

	private static bool FindAreaDanger(out string why)
	{
		foreach (var member in PartyMembers)
		{
			if (member != null && !member.IsDead && member.MaxHp > 0 && member.IsInCriticalClass())
			{
				why = $"{member.Name} is in the critical class";
				return true;
			}
		}

		if (DataCenter.AnnouncedHitDropsUnprotectedBelow(HealthForDyingTanks, out var who))
		{
			why = $"the announced cast ({DataCenter.AnnouncedAreaShare:P0} of max HP as measured) would put {who} into the critical class";
			return true;
		}

		why = string.Empty;
		return false;
	}

	/// <summary>
	/// Whether a single-target defense must not wait: the player or a tank stands in the critical
	/// class.
	/// </summary>
	/// <remarks>
	/// <para>Only the player and the tanks, because they are who a single-target defense reaches: the
	/// dispatch opens it for a tankbuster or for the player's own danger, and most of what it gates goes
	/// on the caster (Camouflage, Rampart, Heart of Corundum). A damage dealer in danger across the
	/// arena gains nothing from it; the heal chain answers for them.</para>
	/// <para>An announced tankbuster is deliberately not a reason by itself. It is the very signal that
	/// opens the single-target defense, so yielding to it would dissolve every hold at the moment it is
	/// asked - two busters in a row would both lose the stretch. A buster has no measured size to weigh
	/// as the area cast does; the test left is the one that needs none.</para>
	/// </remarks>
	/// <param name="why">Who is in danger, or empty.</param>
	/// <returns>True when a hold of a single-target defense must yield.</returns>
	internal static bool SingleDefenseDanger(out string why)
	{
		foreach (var member in PartyMembers)
		{
			if (member == null || member.IsDead || member.MaxHp == 0)
			{
				continue;
			}

			if ((member.GameObjectId == Player?.GameObjectId || member.IsJobCategory(JobRole.Tank))
				&& member.IsInCriticalClass())
			{
				why = $"{member.Name} is in the critical class";
				return true;
			}
		}

		why = string.Empty;
		return false;
	}
}
