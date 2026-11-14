using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class BlueMageRotation
{
	/// <summary>
	/// Whether a spell of fixed damage - 1000 Needles - deals more than the filler in the time it takes: the fixed damage its
	/// text states, at most the health of the enemies it reaches since they share it, against the measured filler damage
	/// over its cast. False while nothing is measured or the text states no figure (concept 01, "Wert statt Reihenfolge").
	/// </summary>
	/// <param name="action">The spell, asked after its <c>CanUse</c> chose its targets.</param>
	protected bool FixedDamageBeatsFiller(IBaseAction action)
	{
		if (!ActionTooltip.TryFixedDamage(action.ID, out var fixedDamage))
		{
			return false;
		}

		var filler = FillerDamageOver(SpellTime(action)) * FillerFactorOn(action);
		return !float.IsNaN(filler) && Math.Min(fixedDamage, HealthReached(action, false)) > filler;
	}

	/// <summary>
	/// Whether a spell dealing a share of each enemy's health - Launcher, Dimensional Shift - deals more than the filler in
	/// the time it takes: the share its text states (the mean, where it names several at random) of the health of the
	/// enemies it reaches, against the measured filler damage over its cast. An enemy of higher level or of a kind such a
	/// spell has been seen to do nothing to counts with nothing.
	/// </summary>
	/// <param name="action">The spell, asked after its <c>CanUse</c> chose its targets.</param>
	protected bool HealthShareBeatsFiller(IBaseAction action)
	{
		if (!ActionTooltip.TryHealthShare(action.ID, out var share))
		{
			return false;
		}

		var filler = FillerDamageOver(SpellTime(action)) * FillerFactorOn(action);
		return !float.IsNaN(filler) && share * HealthReached(action, true) > filler;
	}

	// The current health of the enemies an action reaches as its CanUse chose them. With onlyAffected, an enemy the fixed-or-
	// lethal class does nothing to - of higher level, or of a kind seen unaffected - adds nothing.
	private static float HealthReached(IBaseAction action, bool onlyAffected)
	{
		var sum = 0f;
		var targets = action.Target.AffectedTargets;
		if (targets == null)
		{
			return sum;
		}

		foreach (var enemy in targets)
		{
			if (enemy == null || !enemy.IsEnemy()
				|| (onlyAffected && (enemy.Level > DataCenter.PlayerSyncedLevel() || OwnHitWatch.FlatOrLethalDoesNothing(enemy))))
			{
				continue;
			}

			sum += enemy.CurrentHp;
		}

		return sum;
	}

	/// <summary>
	/// Whether <paramref name="action"/>, a spell without a recast of its own, should go before Sonic Boom: it brings more
	/// potency per time over the enemies it reaches, and on a single target only without a longer cast - a longer cast costs
	/// the weaving room the Blue Mage's many abilities need, which is why Sonic Boom (1 s) is the filler at Blue Academy,
	/// WrathCombo and BossMod. True without Sonic Boom in the set or a figure in either text, so the old order stays
	/// (concept 01, "Füllreihenfolge").
	/// </summary>
	/// <param name="action">The spell, asked after its <c>CanUse</c> chose its targets.</param>
	protected bool OverSonicBoom(IBaseAction action)
	{
		if (!SonicBoomPvE.Info.IsOnSlot || !SonicBoomPvE.EnoughLevel)
		{
			return true;
		}

		var targets = Math.Max(1, action.Target.AffectedTargets?.Length ?? 1);
		if (!ActionTooltip.TryPotencyOn(action.AdjustedID, targets, out var potency)
			|| !ActionTooltip.TryPotencyOn(SonicBoomPvE.ID, 1, out var boom))
		{
			return true;
		}

		// In the Masked Carnivale each counts with what its target does to it (A647): nothing against an immune enemy, the
		// weakness factor against a weak one. An area spell's potency over its targets counts with its main target's factor.
		var target = action.Target.Target;
		var factor = CarnivaleFactor(action, target);
		var boomFactor = CarnivaleFactor(SonicBoomPvE, target);
		if (potency * factor / SpellTime(action) <= boom * boomFactor / SpellTime(SonicBoomPvE))
		{
			return false;
		}

		// A target weaker to it than to Sonic Boom: the doubled hit outweighs the weaving room a longer cast costs.
		return targets > 1 || factor > boomFactor || action.Info.CastTime <= SonicBoomPvE.Info.CastTime;
	}

	/// <summary>
	/// Whether a Moon Flute window begun now gains more than its Waning Nocturne costs (concept 01, "Mondflöte", "Wert").
	/// </summary>
	/// <remarks>
	/// Gained: the damage increase on what falls into Waxing - every damaging spell in the set with a recast of its own
	/// longer than the GCD that is ready within it, at the first potency its text states, once per charge at hand, and the
	/// filler for the GCDs left. Lost: the flute's own GCD and the GCDs of Waning at the filler's potency. Where the target is
	/// the last enemy in range and its time to kill is known, both count only up to it, so a flute just before the end costs
	/// its own GCD alone. No window while BossModReborn predicts downtime within Waxing. Multiple hits count with their first
	/// potency, so the window errs towards going too rarely.
	/// </remarks>
	/// <param name="why">The figures, for the trace.</param>
	/// <param name="net">What the window gains over what it costs, in potency.</param>
	/// <returns>True when the window gains more than it costs.</returns>
	protected bool MoonFluteWindowWorth(out string why, out float net)
	{
		net = 0f;
		var gcd = WeaponTotal;
		if (!TryMoonFluteText(out var damageUp, out var waxing, out var waning) || !TryFiller(out var filler, out _) || gcd <= 0f)
		{
			why = "Moon Flute's or the filler's effect text states no figure";
			return false;
		}

		if (BMRDowntimeWithin(waxing))
		{
			why = $"BossMod predicts downtime in {BMRDowntimeIn:F0} s";
			return false;
		}

		var left = float.PositiveInfinity;
		if (NumberOfHostilesInRange <= 1 && CurrentTarget is { } target && target.GetCorrectedTTK() is var ttk && !float.IsNaN(ttk) && ttk > 0f)
		{
			left = ttk;
		}

		var gainSpan = Math.Min(waxing, left);
		var lossSpan = Math.Min(waning, Math.Max(0f, left - waxing));

		// One entry per recast group: J Kick and Quasar, Feather Rain and Eruption, Nightbloom and Both Ends share theirs, and
		// only the stronger of a pair goes in the window.
		_windowGroups.Clear();
		foreach (var action in AllBaseActions)
		{
			if (action == null || action.ID == MoonFlutePvE.ID || action.Setting.IsFriendly || !action.Info.IsOnSlot
				|| !action.EnoughLevel || action.Cooldown.RecastTimeOneChargeRaw <= gcd)
			{
				continue;
			}

			if (!action.Cooldown.HasOneCharge && action.Cooldown.RecastTimeRemainOneChargeRaw > gainSpan)
			{
				continue;
			}

			if (!ActionTooltip.TryPotencyOn(action.ID, 1, out var potency))
			{
				continue;
			}

			var charges = Math.Max((int)action.Cooldown.CurrentCharges, 1);
			var group = action.Cooldown.CoolDownGroup;
			if (!_windowGroups.TryGetValue(group, out var known) || known.Potency < potency * charges)
			{
				_windowGroups[group] = (potency * charges, action.Info.IsRealGCD ? charges : 0);
			}
		}

		var burst = 0f;
		var burstGcds = 0;
		foreach (var (potency, gcds) in _windowGroups.Values)
		{
			burst += potency;
			burstGcds += gcds;
		}

		var fillers = Math.Max(0f, (gainSpan / gcd) - burstGcds);
		var gain = damageUp / 100f * (burst + (filler * fillers));
		var cost = filler * (1f + (lossSpan / gcd));
		why = $"window {burst + (filler * fillers):F0} potency in {gainSpan:F0} s: +{gain:F0} against {cost:F0} lost";
		net = gain - cost;
		return net > 0f;
	}

	/// <summary>
	/// The potency per second that holding the burst abilities ready now costs over a long fight: each of them comes back that
	/// much later, a share of a use per second of its recast. Holding them for the Moon Flute pays while the time held, times
	/// this, stays below what the window gains (concept 01, "Der Burst wartet").
	/// </summary>
	protected float BurstHoldCostPerSecond()
	{
		_windowGroups.Clear();
		foreach (var action in AllBaseActions)
		{
			if (action == null || action.Info.IsRealGCD || action.Setting.IsFriendly || !action.Info.IsOnSlot
				|| !action.Cooldown.HasOneCharge || action.Cooldown.RecastTimeOneChargeRaw <= WeaponTotal
				|| !ActionTooltip.TryPotencyOn(action.ID, 1, out var potency))
			{
				continue;
			}

			var perSecondOfIt = potency / action.Cooldown.RecastTimeOneChargeRaw;
			var group = action.Cooldown.CoolDownGroup;
			if (!_windowGroups.TryGetValue(group, out var known) || known.Potency < perSecondOfIt)
			{
				_windowGroups[group] = (perSecondOfIt, 0);
			}
		}

		var perSecond = 0f;
		foreach (var (potency, _) in _windowGroups.Values)
		{
			perSecond += potency;
		}

		return perSecond;
	}

	/// <summary>
	/// Whether the 15 s of Waning Nocturne put anyone at risk (concept 01, "Mondflöte", "Sicherheit"): tank or healer mimicry,
	/// a member in the critical class or an announced area cast that would put one there, the player's own health carried
	/// forward over the window into the critical class, a raidwide BossModReborn predicts within the window, or a tankbuster
	/// on the player within it. Where the chance is measured: the health trend, the announcement, BossModReborn's timeline;
	/// without a module only the first two.
	/// </summary>
	/// <param name="why">The reason it is not safe, or empty.</param>
	/// <returns>True when nothing of that holds.</returns>
	protected bool MoonFluteSafe(out string why)
	{
		if (IsTank || IsHealer)
		{
			why = IsTank ? "tank mimicry: Waning Nocturne leaves no mitigation" : "healer mimicry: Waning Nocturne leaves no heals";
			return false;
		}

		if (AreaDefenseDanger(out var danger))
		{
			why = danger;
			return false;
		}

		var window = TryMoonFluteText(out _, out var waxing, out var waning) ? waxing + waning : 0f;
		if (PlayerFallsWithin(window))
		{
			why = "the player's health, carried forward over the window, falls into the critical class";
			return false;
		}

		if (BMRRaidwideWithin(window))
		{
			why = $"BossMod predicts a raidwide in {BMRRaidwideIn:F0} s";
			return false;
		}

		if (TankbusterOnMeWithin(window))
		{
			why = "a tankbuster on the player is announced within the window";
			return false;
		}

		why = string.Empty;
		return true;
	}

	private static uint _setWrittenId;

	private static uint _setWrittenIn;

	private static uint[]? _setSeen;

	private static bool _setEditedByPlayer;

	/// <summary>
	/// Writes one improvement of the active set per call, out of combat (concept 01, "Verbessern statt ersetzen"): the
	/// filler where it is missing, the partners of the chains one part of which is in the set, then damage spells with a
	/// recast from recast groups the set lacks. It takes only free slots or slots whose spell RSR does not cast at this
	/// level - a plain-potency spell that is neither the best single-target nor the best area filler, or a fixed-damage
	/// spell that does not beat the filler. Cooldowns in the set stay, as do heals, defence, movement,
	/// mimicry, raises, interrupts and every spell with an additional effect stay. A slot is written only while its spell
	/// does not recast. After a write nothing more is written until the slot shows the new spell, so a write the game
	/// refuses - a duty that forbids changing the set - stops the improvement in that area.
	/// </summary>
	protected unsafe void ImproveActiveSet()
	{
		var player = Player;
		var manager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		// Not between areas: the slots are read only in a valid frame, and a write during the load would land on a set the
		// game is still replacing (A576 calls this every frame, RSR on or off).
		if (DataCenter.InCombat || player == null || player.IsDead || player.IsCasting || player.TargetObject != null
			|| manager == null || manager->ActionQueued || WeaponTotal <= 0f
			|| ECommons.DalamudServices.Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas]
			|| ECommons.DalamudServices.Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas51])
		{
			return;
		}

		// While the window writes a set for a Masked Carnivale stage, the improver waits (A627).
		if (CarnivaleApplying)
		{
			return;
		}

		var slots = DataCenter.BluSlots;

		// A set the player changed themselves - a saved set loaded, a spell for a Masked Carnivale stage put in - is his
		// decision: from then on RSR writes nothing into it in this session. Inside the Carnivale never.
		if (_setSeen != null && !_setEditedByPlayer)
		{
			for (var i = 0; i < slots.Length && i < _setSeen.Length; i++)
			{
				if (slots[i] != _setSeen[i] && !(i == _setWrittenSlot && slots[i] == _setWrittenId))
				{
					_setEditedByPlayer = true;
					DefenseTrace.Line($"blue mage set: changed by the player in slot {i + 1} - RSR leaves the set alone for this session");
					break;
				}
			}
		}

		_setSeen = (uint[])slots.Clone();
		if (_setEditedByPlayer || DataCenter.IsInMaskedCarnivale)
		{
			return;
		}

		if (_setWrittenSlot >= 0)
		{
			if (DataCenter.TerritoryID != _setWrittenIn)
			{
				_setWrittenSlot = -1;
			}
			else if (_setWrittenSlot < slots.Length && slots[_setWrittenSlot] == _setWrittenId)
			{
				DefenseTrace.Line($"blue mage set: slot {_setWrittenSlot + 1} now holds #{_setWrittenId}");
				_setWrittenSlot = -1;
			}

			return;
		}

		if (!TryPlanSetChange(slots, out var slot, out var add, out var why))
		{
			return;
		}

		var old = slots[slot];
		manager->AssignBlueMageActionToSlot(slot, add.ID);
		_setWrittenSlot = slot;
		_setWrittenId = add.ID;
		_setWrittenIn = DataCenter.TerritoryID;
		DefenseTrace.Line($"blue mage set: slot {slot + 1} #{old} -> {add.Name} #{add.ID} ({why})");
	}

	private bool TryPlanSetChange(uint[] slots, out int slot, out IBaseAction add, out string why)
	{
		slot = -1;
		add = null!;
		why = string.Empty;

		var inSet = new Dictionary<uint, IBaseAction>();
		var learned = new List<IBaseAction>();
		foreach (var action in AllBaseActions)
		{
			if (action == null || action.Action.ClassJob.RowId != (uint)ECommons.ExcelServices.Job.BLU || !action.EnoughLevel
				|| !action.Info.IsQuestUnlocked())
			{
				continue;
			}

			if (Array.IndexOf(slots, action.ID) >= 0)
			{
				inSet[action.ID] = action;
			}
			else
			{
				learned.Add(action);
			}
		}

		// The best single-target and the best area filler in the set stay; every other plain filler can make room.
		IBaseAction? bestSingle = null, bestArea = null;
		float single = 0f, area = 0f;
		foreach (var action in inSet.Values)
		{
			if (!IsPlainFiller(action))
			{
				continue;
			}

			var time = SpellTime(action);
			if (ActionTooltip.TryPotencyOn(action.ID, 1, out var one) && one / time > single)
			{
				single = one / time;
				bestSingle = action;
			}

			if (action.Action.EffectRange > 0 && ActionTooltip.TryPotencyOn(action.ID, 1 + 1, out var two) && two / time > area)
			{
				area = two / time;
				bestArea = action;
			}
		}

		bool Removable(IBaseAction action)
		{
			if (IsPlainFiller(action))
			{
				return action != bestSingle && action != bestArea;
			}

			if (!ActionTooltip.TryFixedDamage(action.ID, out var fixedDamage) || action.Cooldown.RecastTimeOneChargeRaw > WeaponTotal)
			{
				return false;
			}

			var filler = FillerDamageOver(SpellTime(action));
			return !float.IsNaN(filler) && fixedDamage <= filler;
		}

		// What comes in, first to last: the filler, the partners of chains in the set, cooldowns of missing recast groups.
		var wanted = new List<(IBaseAction Spell, float Value, string Why)>();
		if (learned.Contains(SonicBoomPvE))
		{
			wanted.Add((SonicBoomPvE, 0f, "the filler"));
		}

		void Pair(IBaseAction part, IBaseAction partner, string chain)
		{
			if (inSet.ContainsKey(part.ID) && learned.Contains(partner) && !wanted.Exists(w => w.Spell == partner))
			{
				wanted.Add((partner, 0f, chain));
			}
		}

		Pair(TripleTridentPvE, WhistlePvE, "Whistle before Triple Trident");
		Pair(TripleTridentPvE, TinglePvE, "Tingle before Triple Trident");
		Pair(WingedReprobationPvE, ConvictionMarcatoPvE, "Conviction Marcato after Winged Redemption");
		Pair(TheRamsVoicePvE, UltravibrationPvE, "Ultravibration after the Ram's Voice");
		Pair(BlackKnightsTourPvE, WhiteKnightsTourPvE, "the Knight's Tour cycle");
		Pair(WhiteKnightsTourPvE, BlackKnightsTourPvE, "the Knight's Tour cycle");
		Pair(PeripheralSynthesisPvE, MustardBombPvE, "Mustard Bomb on Lightheaded");
		Pair(MatraMagicPvE, BristlePvE, "Bristle before Matra Magic");
		Pair(SongOfTormentPvE, BristlePvE, "Bristle before Song of Torment");

		var groupsInSet = new HashSet<byte>();
		foreach (var action in inSet.Values)
		{
			groupsInSet.Add(action.Cooldown.CoolDownGroup);
		}

		var cooldowns = new List<(IBaseAction Spell, float Value, string Why)>();
		foreach (var action in learned)
		{
			var value = CooldownValue(action);
			if (value <= 0f || groupsInSet.Contains(action.Cooldown.CoolDownGroup))
			{
				continue;
			}

			var index = cooldowns.FindIndex(c => c.Spell.Cooldown.CoolDownGroup == action.Cooldown.CoolDownGroup);
			if (index < 0)
			{
				cooldowns.Add((action, value, "a damage cooldown the set lacks"));
			}
			else if (cooldowns[index].Value < value)
			{
				cooldowns[index] = (action, value, "a damage cooldown the set lacks");
			}
		}

		cooldowns.Sort((a, b) => b.Value.CompareTo(a.Value));
		wanted.AddRange(cooldowns);

		foreach (var (spell, value, reason) in wanted)
		{
			// A free slot first, then a filler RSR does not cast. A cooldown in the set stays: its first potency undervalues
			// Triple Trident's three hits and Winged Reprobation's four casts. A slot whose spell recasts is left alone.
			var best = -1;
			var bestHeld = float.MaxValue;
			for (var i = 0; i < slots.Length; i++)
			{
				float held;
				if (slots[i] == 0)
				{
					held = -1f;
				}
				else if (!inSet.TryGetValue(slots[i], out var heldAction) || heldAction.Cooldown.IsCoolingDown)
				{
					continue;
				}
				else if (Removable(heldAction))
				{
					held = 0f;
				}
				// The best single-target filler gives way to Sonic Boom where that brings more potency a second.
				else if (heldAction == bestSingle && spell == SonicBoomPvE
					&& ActionTooltip.TryPotencyOn(spell.ID, 1, out var boom) && boom / SpellTime(spell) > single)
				{
					held = 0f;
				}
				else
				{
					continue;
				}

				if (held < bestHeld)
				{
					bestHeld = held;
					best = i;
				}
			}

			if (best >= 0)
			{
				slot = best;
				add = spell;
				why = reason;
				return true;
			}
		}

		return false;
	}

	// A spell RSR uses only as a filler: no recast of its own, one plain potency in its text and no additional effect,
	// interrupt or enmity - so taking it out costs nothing a better filler does not give.
	private static bool IsPlainFiller(IBaseAction action)
	{
		var text = ActionTooltip.Text(action.ID);
		return !action.Setting.IsFriendly && action.Cooldown.RecastTimeOneChargeRaw <= WeaponTotal
			&& ActionTooltip.TryPlainPotency(action.ID, out _)
			&& !text.Contains("Additional Effect", StringComparison.Ordinal) && !text.Contains("nterrupt", StringComparison.Ordinal)
			&& !text.Contains("enmity", StringComparison.Ordinal);
	}

	// The potency a second a damaging spell with a recast of its own brings over a long fight: its first stated potency,
	// over its recast. Zero for anything else.
	private static float CooldownValue(IBaseAction action)
	{
		var recast = action.Cooldown.RecastTimeOneChargeRaw;
		return action.Setting.IsFriendly || recast <= WeaponTotal || !ActionTooltip.TryPotencyOn(action.ID, 1, out var potency)
			? 0f
			: potency / recast;
	}

	/// <summary>
	/// Whether the player's health, carried forward over what is left of Waxing Nocturne and the whole of Waning Nocturne,
	/// falls into the critical class: the one case for Diamondback at the end of Waxing, which turns it into Waning at once
	/// and roots the player for its duration (concept 01, "Diamantrücken").
	/// </summary>
	protected bool PlayerFallsInWaning
		=> TryMoonFluteText(out _, out _, out var waning)
			&& PlayerFallsWithin(StatusHelper.PlayerStatusTime(true, StatusID.WaxingNocturne) + waning);
}
