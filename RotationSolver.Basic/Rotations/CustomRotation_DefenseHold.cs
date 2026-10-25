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
	// owner's (Swiftcast kept for a raise, the Living Dead window, a heal ban) or one the game imposes
	// (no action possible at all) is not a strategic choice and does not come through here; nor is a
	// hold a setting's own text orders without exception (Prioritize Microcosmos, Prevent the use of
	// defense abilities during burst), because the setting text binds.
	//
	// Heals are held by the same layer where the hold is strategic. The single-target scope covers a
	// heal or defense on the player or a tank; HoldSelfHeal covers a heal only the player receives.

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
	/// Whether a strategic hold of a heal the player casts on itself stands; it yields when the
	/// player is in the critical class. The same universal layer as the defense holds, for the one
	/// member such a heal can reach.
	/// </summary>
	/// <param name="wanted">The job rule's own verdict that the heal should wait.</param>
	/// <param name="rule">What the hold is, in a few words, for the diagnostics window.</param>
	/// <returns>True while the hold stands.</returns>
	protected static bool HoldSelfHeal(bool wanted, string rule)
	{
		if (!wanted)
		{
			return false;
		}

		var danger = Player != null && !Player.IsDead && Player.IsInCriticalClass();
		return Record("self heal", rule, !danger, danger ? $"{Player!.Name} is in the critical class" : string.Empty);
	}

	/// <summary>
	/// Whether a strategic hold of a heal the player casts on the party stands. It yields when no
	/// healer in the party is alive, or when the healers do not keep up: a living member stands in the
	/// critical class. The owner's rule for a damage dealer's heal (14.10.2026): heal when it is needed
	/// and the healers are dead or cannot keep up, for instance without mana - own damage is worth
	/// nothing when others die and deal none. A healer short of mana shows here by its effect, a member
	/// falling into the critical class, not by reading the mana itself.
	/// </summary>
	/// <param name="wanted">The job rule's own verdict that the heal should wait.</param>
	/// <param name="rule">What the hold is, in a few words, for the diagnostics window.</param>
	/// <returns>True while the hold stands.</returns>
	protected static bool HoldPartyHeal(bool wanted, string rule)
	{
		if (!wanted)
		{
			return false;
		}

		if (!DataCenter.AnyLivingHealer(false))
		{
			return Record("party heal", rule, false, "no healer in the party is alive");
		}

		foreach (var member in PartyMembers)
		{
			if (member != null && !member.IsDead && member.MaxHp > 0 && member.IsInCriticalClass())
			{
				return Record("party heal", rule, false, $"{member.Name} is in the critical class");
			}
		}

		return Record("party heal", rule, true, string.Empty);
	}

	/// <summary>
	/// Whether a rotation's GCD heals may go while its "Use GCDs to heal" is off: when no other healer is alive - a
	/// healer leaves them to the co-healer, a tank or damage dealer to the healers - or when those healers do not keep
	/// up, a living member in the critical class (<see cref="HoldPartyHeal"/>). The owner's rule for a heal that waits
	/// on the healers (14.10.2026) and the universal layer every strategic hold yields to; until A360 the setting held
	/// them even for a member about to fall while another healer lived. One definition for the seven rotations that
	/// carried the check as their own copy.
	/// </summary>
	/// <param name="useGcdHeals">The rotation's "Use GCDs to heal".</param>
	/// <returns>True when its GCD heals may go.</returns>
	protected static bool GcdHealsAllowed(bool useGcdHeals)
	{
		if (useGcdHeals)
		{
			return true;
		}

		return !DataCenter.AnyLivingHealer(true) || !HoldPartyHeal(true, "GCD heals left to the healers");
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
		return HoldAreaDefense(AnyTriggerInEffect(triggers, true), rule);
	}

	/// <summary>
	/// The tanks' stage of the stretch: Reprisal and the job's party mitigation take turns over a run of area attacks
	/// instead of both falling on the first - The Balance stacks them for a big hit and spreads them over several, and
	/// the measured size decides which through the danger yield (concept 09, "Krieger: die übrige Abwehr", rule 9). Read
	/// only with "Spread Reprisal and the party mitigation over a run of area attacks" on.
	/// </summary>
	/// <param name="partyTools">The job's party mitigation: Divine Veil and Passage of Arms, Shake It Off, Dark
	/// Missionary, Heart of Light.</param>
	/// <returns>True while Reprisal and the party tools wait.</returns>
	protected bool TankAreaDefenseStretched(params IBaseAction[] partyTools)
	{
		if (!Service.Config.SpreadTankAreaDefense)
		{
			return false;
		}

		IBaseAction[] triggers = [ReprisalPvE, .. partyTools];
		return AreaDefenseStretched("Tank: Reprisal or the party mitigation still in effect", triggers);
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
		return HoldSingleDefense(AnyTriggerInEffect(triggers, false), rule);
	}

	// A trigger is in effect from its use until the duration its own effect text states, and only
	// while it has no charge back - a trigger that still holds a charge does not stretch, as the job
	// rules had it before. The use is taken from the record of the player's own actions, and the
	// charge from the action's recast group, never from the button: some buttons turn into another
	// action while the effect runs (Liturgy of the Bell's second press, Macrocosmos and Microcosmos),
	// and that other action's cooldown is not the trigger's. A text that states no duration never
	// stretches - a figure the game did not give is not invented.
	private static bool AnyTriggerInEffect(IBaseAction[] triggers, bool area)
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

			if (UsedWithin(trigger.ID, duration) && !(area && EffectGoneFromPlayer(trigger)))
			{
				return true;
			}
		}

		return false;
	}

	// An effect works until it is gone, not always for the time its text states: Shake It Off and Divine Veil give 30 s,
	// and the area attack they were pressed for takes the barrier with it; Liturgy of the Bell ends at its second press or
	// its last stack. Read on the player, who is in the effect's range: the game names such an effect after the action,
	// and once a beneficial status of that name - laid by the player - is gone from him, a GCD after the press so it had
	// time to show, the effect is over and nothing is left to stretch (A340). An action without such a status keeps its
	// stated time: Reprisal lies on the enemy, a detrimental status, and is left alone. Asked for the area stretch only,
	// whose effect reaches the player; a single-target one lies on its target (Divine Benison on the tank). This can only
	// end a stretch early.
	private static bool EffectGoneFromPlayer(IBaseAction trigger)
	{
		if (Player == null)
		{
			return false;
		}

		var statuses = OwnEffectStatusesOf(trigger.ID);
		return statuses.Length > 0 && !UsedWithin(trigger.ID, DataCenter.DefaultGCDTotal)
			&& !Player.HasStatus(true, statuses);
	}

	private static readonly Dictionary<uint, StatusID[]> _effectStatuses = [];

	// The beneficial statuses bearing the action's own name, read once from the English sheets.
	private static StatusID[] OwnEffectStatusesOf(uint actionId)
	{
		if (_effectStatuses.TryGetValue(actionId, out var known))
		{
			return known;
		}

		List<StatusID> found = [];
		var actions = ECommons.DalamudServices.Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>(Dalamud.Game.ClientLanguage.English);
		var statuses = ECommons.DalamudServices.Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
		if (actions.TryGetRow(actionId, out var action))
		{
			var name = action.Name.ExtractText();
			foreach (var row in statuses)
			{
				if (row.StatusCategory == (byte)StatusCategory.Beneficial && row.Name.ExtractText() == name)
				{
					found.Add((StatusID)row.RowId);
				}
			}
		}

		known = [.. found];
		_effectStatuses[actionId] = known;
		return known;
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
		(var danger, why) = _areaDanger.Value;
		return danger;
	}

	private static readonly FrameCached<(bool Danger, string Why)> _areaDanger =
		new(static () => (FindAreaDanger(out var why), why), DataCenter.FrameCacheMs);

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

	/// <summary>
	/// Whether a heal spent to keep a resource from overflowing, not because anyone lacks health, should
	/// wait: area damage on the party is announced and has not landed, and the resource is not lost within
	/// the next GCD (<paramref name="lostWithinGcd"/>).
	/// </summary>
	/// <remarks>
	/// Such a heal - the White Mage's lilies at the cap - goes out whatever the party's health. Spent just
	/// before a raidwide it lands on full health and is gone; spent after it, the same GCD heals what the
	/// raidwide took. His note of 15.10.2026: "bei direkt aoe-heals jedoch verschwendung" (A292). Read from
	/// the recognition (<see cref="DataCenter.AreaHitAnnounced"/>), not from the mitigation verdict: any area
	/// hit leaves health to restore. Not a strategic hold of the kind above, so it does not yield to danger;
	/// a member in danger is healed through the heal flags, where the same action is the first choice.
	/// The other healers have no such heal on the area: the Sage's Druochole spends its stack only one GCD
	/// before it is lost, and the Astrologian's third Essential Dignity asks for health.
	/// </remarks>
	protected static bool HealForResourceWaitsForAreaHit(bool lostWithinGcd)
	{
		return !lostWithinGcd && DataCenter.AreaHitAnnounced;
	}

	/// <summary>
	/// Whether the single-target defence stands open by the tank's pull rule alone: no tankbuster or listed cast aimed
	/// at a tank, no BossModReborn tankbuster, and no command of the player's.
	/// </summary>
	private static bool PullDefenseOnly => DataCenter.TankPullOnPlayer && !DataCenter.TankbusterOpensDefence;

	/// <summary>
	/// No healer alive, with "Without a living healer, spend your defence to stay alive longer" on: nothing restores what
	/// the player loses, so neither the pull hold below nor the fixed spacing of the big mitigation and Rampart keeps
	/// anything - a tool goes once it is back before the next tankbuster on him is due (BaseAction,
	/// "KeptForTheNextTankbuster"). In the model the tank lives through 9 of 400 big pulls he died in before, and none
	/// the other way (A361, tank_pull_defense_model.py).
	/// </summary>
	private static bool DefenceSpentWithoutHealer =>
		Service.Config.SpendDefenceWithoutHealer && !DataCenter.AnyLivingHealer(false);

	/// <summary>
	/// Whether the tank's big mitigation or Rampart waits in a pull: "Hold the big mitigation and Rampart in a pull until
	/// your health needs them" is on, the defence stands open by the pull alone, and the player's health, at the rate
	/// measured net of every heal and mitigation, stays at or above "Health the big mitigation waits for" through
	/// <paramref name="mitigation"/>'s own duration. Before a pull has a rate, it waits. A strategic hold, so it yields
	/// while the player or a tank is in danger (<see cref="HoldSingleDefense"/>). Modelled in
	/// tank_pull_defense_model.py (concept 09, "Die Abwehr im Pull", A359).
	/// </summary>
	/// <param name="mitigation">The big mitigation or Rampart.</param>
	/// <returns>True while it waits.</returns>
	protected static bool BigMitigationHeldInPull(IBaseAction mitigation)
	{
		if (!Service.Config.HoldBigTankMitigationInPull || !PullDefenseOnly || Player == null || DefenceSpentWithoutHealer)
		{
			return false;
		}

		var heading = Player.GetHealthRatioIn(DefensiveValues.DurationOf(mitigation.ID));
		return HoldSingleDefense(heading >= Service.Config.HoldBigTankMitigationBelow, "pull: big mitigation waits for the measured fall");
	}

	/// <summary>
	/// The tank's big mitigation - <paramref name="upgraded"/> once learned, else <paramref name="big"/> - and Rampart
	/// outside an announced hit, the part every tank rotation carried as its own copy. The two exclude each other through
	/// their statuses. In a pull under <see cref="BigMitigationHeldInPull"/> each goes once the measured fall would cross
	/// the line within its own duration; in the model a fixed spacing on top only cost (A359). With no healer alive
	/// neither holds (<see cref="DefenceSpentWithoutHealer"/>). Otherwise spaced as before: the big one while Rampart is
	/// ready or 60 s past, Rampart 30 s after the big one; below the big one's level Rampart alone.
	/// </summary>
	/// <param name="big">The job's big mitigation (Sentinel, Vengeance, Shadow Wall, Nebula).</param>
	/// <param name="upgraded">Its upgrade (Guardian, Damnation, Shadowed Vigil, Great Nebula).</param>
	/// <param name="act">The action to use.</param>
	/// <returns>True when one goes.</returns>
	protected bool StaggeredBigMitigation(IBaseAction big, IBaseAction upgraded, out IAction? act)
	{
		var current = upgraded.EnoughLevel ? upgraded : big;
		var measured = (Service.Config.HoldBigTankMitigationInPull && PullDefenseOnly) || DefenceSpentWithoutHealer;

		if (current.EnoughLevel
			&& (measured || !RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60))
			&& !BigMitigationHeldInPull(current) && current.CanUse(out act))
		{
			return true;
		}

		if ((!big.EnoughLevel || measured || (current.Cooldown.IsCoolingDown && current.Cooldown.ElapsedAfter(30)))
			&& !BigMitigationHeldInPull(RampartPvE) && RampartPvE.CanUse(out act))
		{
			return true;
		}

		act = null;
		return false;
	}
}
