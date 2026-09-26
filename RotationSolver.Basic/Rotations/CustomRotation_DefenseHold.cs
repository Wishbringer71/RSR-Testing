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
		return Decide(wanted, rule, AreaDefenseDanger(out var why), why);
	}

	/// <summary>
	/// Whether a strategic hold of a single-target defense stands; it yields when a member is in
	/// danger or a tankbuster is announced (<see cref="SingleDefenseDanger"/>).
	/// </summary>
	/// <param name="wanted">The job rule's own verdict that the defense should wait.</param>
	/// <param name="rule">What the hold is, in a few words, for the diagnostics window.</param>
	/// <returns>True while the hold stands.</returns>
	protected static bool HoldSingleDefense(bool wanted, string rule)
	{
		return Decide(wanted, rule, SingleDefenseDanger(out var why), why);
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

	// A trigger is in effect from its use until the duration its own effect text states. Read off the
	// cooldown as the job rules read it before: out of charges, and no charge back within the recast
	// less that duration. A trigger that still holds a charge does not stretch, as before; one whose
	// text states no duration never does - a figure the game did not give is not invented here.
	private static bool AnyTriggerInEffect(IBaseAction[] triggers)
	{
		foreach (var trigger in triggers)
		{
			var duration = DefensiveValues.DurationOf(trigger.ID);
			if (duration <= 0f || !trigger.EnoughLevel)
			{
				continue;
			}

			if (trigger.Cooldown.IsCoolingDown
				&& !trigger.Cooldown.WillHaveOneCharge(trigger.Cooldown.RecastTimeOneChargeRaw - duration))
			{
				return true;
			}
		}

		return false;
	}

	private static bool Decide(bool wanted, string rule, bool danger, string why)
	{
		if (!wanted)
		{
			return false;
		}

		DataCenter.LastDefenseHold = new(rule, !danger, danger ? why : string.Empty, DateTime.Now);
		return !danger;
	}

	/// <summary>
	/// Whether an area defense must not wait: a living, unprotected member already stands in the
	/// heal chain's critical class (effective health at or below <see cref="HealthForDyingTanks"/>,
	/// concept 07), or the announced area cast, as measured, would put one there.
	/// </summary>
	/// <remarks>
	/// The measured share is the hardest the cast has been seen to land, after whatever mitigation
	/// stood then, and only ever raised. The mitigation standing now is therefore not taken off it
	/// again - that would count it twice. The estimate errs towards "dangerous", so the hold yields
	/// rather too often than too late. A cast never measured answers only through the first test.
	/// </remarks>
	/// <param name="why">Who is in danger, or empty.</param>
	/// <returns>True when a hold of an area defense must yield.</returns>
	internal static bool AreaDefenseDanger(out string why)
	{
		if (MemberInCriticalClass(out why))
		{
			return true;
		}

		if (DataCenter.AnnouncedHitDropsAnyoneBelow(HealthForDyingTanks))
		{
			why = $"the announced cast ({DataCenter.AnnouncedAreaShare:P0} of max HP as measured) would put a member into the critical class";
			return true;
		}

		why = string.Empty;
		return false;
	}

	/// <summary>
	/// Whether a single-target defense must not wait: a member in the critical class, or a
	/// tankbuster announced - by its cast or by BossModReborn.
	/// </summary>
	/// <param name="why">What the danger is, or empty.</param>
	/// <returns>True when a hold of a single-target defense must yield.</returns>
	internal static bool SingleDefenseDanger(out string why)
	{
		if (MemberInCriticalClass(out why))
		{
			return true;
		}

		if (DataCenter.IsHostileCastingToTank || DataCenter.BMRTankbusterImminent)
		{
			why = "a tankbuster is announced";
			return true;
		}

		why = string.Empty;
		return false;
	}

	private static bool MemberInCriticalClass(out string why)
	{
		foreach (var member in PartyMembers)
		{
			if (member == null || member.IsDead || member.MaxHp == 0)
			{
				continue;
			}

			if (member.NoNeedHealingInvuln()
				&& member.GetForecastEffectiveHpPercent() <= HealthForDyingTanks * 100f)
			{
				why = $"{member.Name} is in the critical class";
				return true;
			}
		}

		why = string.Empty;
		return false;
	}
}
