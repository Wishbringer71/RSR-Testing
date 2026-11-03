using CombatRole = RotationSolver.Basic.Data.CombatRole;
namespace RotationSolver.Basic.Rotations.Basic;

public partial class BlueMageRotation
{
	#region Status Tracking
	/// <summary>
	/// 
	/// </summary>
	public static bool WaxingNocturneWillEnd => HasWaxingNocturne && StatusHelper.PlayerWillStatusEndGCD(2, 0, false, StatusID.WaxingNocturne);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasWaxingNocturne => StatusHelper.PlayerHasStatus(true, StatusID.WaxingNocturne);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasWaningNocturne => StatusHelper.PlayerHasStatus(true, StatusID.WaningNocturne);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasBasicInstinct => StatusHelper.PlayerHasStatus(true, StatusID.BasicInstinct);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasSurpanakhasFury => StatusHelper.PlayerHasStatus(true, StatusID.SurpanakhasFury);

	/// <summary>
	/// 
	/// </summary>
	public static bool HasHarmonizedBoost => StatusHelper.PlayerHasStatus(true, StatusID.Boost_1716) || StatusHelper.PlayerHasStatus(true, StatusID.Harmonized);

	/// <summary>
	/// 
	/// </summary>
	public static bool IsTank => StatusHelper.PlayerHasStatus(true, StatusID.AethericMimicryTank);

	/// <summary>
	/// 
	/// </summary>
	public static bool IsDPS => StatusHelper.PlayerHasStatus(true, StatusID.AethericMimicryDps);

	/// <summary>
	/// 
	/// </summary>
	public static bool IsHealer => StatusHelper.PlayerHasStatus(true, StatusID.AethericMimicryHealer);

	/// <summary>
	/// 
	/// </summary>
	public static bool NoMimicry => !IsTank && !IsDPS && !IsHealer;
	#endregion

	#region Value against the filler and the Moon Flute window (concept 01, "Blaumagier", A507)

	/// <summary>
	/// The filler the value rules weigh against: Sonic Boom, else Water Cannon, whichever is in the active set; null with
	/// neither.
	/// </summary>
	protected IBaseAction? ValueFiller
		=> SonicBoomPvE.Info.IsOnSlot ? SonicBoomPvE : WaterCannonPvE.Info.IsOnSlot ? WaterCannonPvE : null;

	/// <summary>
	/// The time a spell takes of the player's: its cast, or the GCD where that is longer.
	/// </summary>
	protected static float SpellTime(IBaseAction action)
		=> Math.Max(action.Info.CastTime, WeaponTotal);

	/// <summary>
	/// The filler's potency as its effect text states it, and the time it takes; false without a filler in the set or a
	/// figure in its text.
	/// </summary>
	protected bool TryFiller(out float potency, out float time)
	{
		time = 0f;
		potency = 0f;
		var filler = ValueFiller;
		if (filler == null || !ActionTooltip.TryPotencyOn(filler.ID, 1, out potency))
		{
			return false;
		}

		time = SpellTime(filler);
		return potency > 0f && time > 0f;
	}

	/// <summary>
	/// The damage the filler would deal in <paramref name="seconds"/>, by the damage per potency measured on the player's own
	/// hits (<see cref="OwnHitWatch.DamagePerPotency"/>); NaN while nothing is measured.
	/// </summary>
	protected float FillerDamageOver(float seconds)
	{
		var perPotency = OwnHitWatch.DamagePerPotency;
		return float.IsNaN(perPotency) || !TryFiller(out var potency, out var time)
			? float.NaN
			: perPotency * potency / time * seconds;
	}

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

		var filler = FillerDamageOver(SpellTime(action));
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

		var filler = FillerDamageOver(SpellTime(action));
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

		if (potency / SpellTime(action) <= boom / SpellTime(SonicBoomPvE))
		{
			return false;
		}

		return targets > 1 || action.Info.CastTime <= SonicBoomPvE.Info.CastTime;
	}

	/// <summary>
	/// Whether the player's own health, carried forward by its measured trend over <paramref name="seconds"/>, falls into
	/// the heal chain's critical class (concept 07). False while health is not falling on balance.
	/// </summary>
	protected static bool PlayerFallsWithin(float seconds)
	{
		var player = Player;
		if (player == null)
		{
			return false;
		}

		var ttk = player.GetCorrectedTTK();
		if (float.IsNaN(ttk) || ttk <= 0f)
		{
			return false;
		}

		var left = player.GetEffectiveHpPercent() * Math.Max(0f, 1f - (seconds / ttk));
		return left <= HealthForDyingTanks * 100f;
	}

	/// <summary>
	/// Moon Flute's Waxing and Waning Nocturne and the damage it adds, from its effect text; false when the text states them
	/// not.
	/// </summary>
	protected bool TryMoonFluteText(out float damageUp, out float waxing, out float waning)
	{
		waxing = 0f;
		waning = 0f;
		return ActionTooltip.TryStatedDamageUp(MoonFlutePvE.ID, out damageUp)
			& ActionTooltip.TryStatedDuration(MoonFlutePvE.ID, out waxing)
			& ActionTooltip.TrySecondStatedDuration(MoonFlutePvE.ID, out waning);
	}

	private readonly Dictionary<byte, (float Potency, int Gcds)> _windowGroups = [];

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

	#region Improving the active set from the learned spells (concept 01, "Verbessern statt ersetzen", A509)

	private static int _setWrittenSlot = -1;
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
		if (DataCenter.InCombat || player == null || player.IsDead || player.IsCasting || player.TargetObject != null
			|| manager == null || manager->ActionQueued || WeaponTotal <= 0f)
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

	#endregion

	/// <summary>
	/// Whether the player's health, carried forward over what is left of Waxing Nocturne and the whole of Waning Nocturne,
	/// falls into the critical class: the one case for Diamondback at the end of Waxing, which turns it into Waning at once
	/// and roots the player for its duration (concept 01, "Diamantrücken").
	/// </summary>
	protected bool PlayerFallsInWaning
		=> TryMoonFluteText(out _, out _, out var waning)
			&& PlayerFallsWithin(StatusHelper.PlayerStatusTime(true, StatusID.WaxingNocturne) + waning);

	#endregion


	/// <summary>
	/// 
	/// </summary>
	public override MedicineType MedicineType => MedicineType.Intelligence;

	private protected sealed override IBaseAction Raise => AngelWhisperPvE;
	private protected sealed override IBaseAction TankStance => MightyGuardPvE;

	/// <summary>
	/// 
	/// </summary>
	public static CombatRole BlueId => IsTank ? CombatRole.Tank : IsHealer ? CombatRole.Healer : CombatRole.DPS;

	/// <summary>
	/// Gets the job role based on the current Aetheric Mimicry status.
	/// </summary>
	public new static JobRole Role => IsTank ? JobRole.Tank : IsHealer ? JobRole.Healer : JobRole.RangedMagical;

	static partial void ModifyWaterCannonPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
	}

	static partial void ModifyFlameThrowerPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Fire;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyAquaBreathPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
		setting.TargetStatusProvide = [StatusID.Dropsy_1736];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyFlyingFrenzyPvE(ref ActionSetting setting)
	{
		// "Delivers a jumping physical attack to target" (effect text): a leap onto the target, so its path is checked and
		// it stays within the reach the player allows damaging gap closers (concept 05, A519).
		setting.SpecialType = SpecialActionType.HostileMovingAttack;
		setting.CanTarget = static t => GapCloserAllowed(t, pressed: ActionID.FlyingFrenzyPvE);
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyDrillCannonsPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
		setting.IsFriendly = false;
		setting.TargetStatusNeed = [StatusID.Petrification, StatusID.Petrification_1511, StatusID.Petrification_3007, StatusID.Petrification_4445, StatusID.Petrification];
	}

	static partial void ModifyHighVoltagePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.IsParalysisSpell = true;
		setting.TargetStatusProvide = [StatusID.Paralysis];
		setting.TargetStatusNeed = [StatusID.Dropsy_1736];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyLoomPvE(ref ActionSetting setting)
	{
		//setting.SpecialType = SpecialActionType.MovingForward;
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.IsFriendly = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyFinalStingPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
		setting.ActionCheck = () => !StatusHelper.PlayerHasStatus(true, StatusID.BrushWithDeath);
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySongOfTormentPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.Bleeding_1714];
	}

	static partial void ModifyGlowerPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.IsParalysisSpell = true;
		setting.TargetStatusProvide = [StatusID.Paralysis];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyPlaincrackerPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Earth;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBristlePvE(ref ActionSetting setting)
	{
		setting.IsFriendly = true;
		setting.StatusProvide = [StatusID.Boost_1716, StatusID.Harmonized];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyWhiteWindPvE(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyLevel5PetrifyPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsPetrificationSpell = true;
		setting.TargetStatusProvide = [StatusID.Petrification];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySharpenedKnifePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Slashing;
		setting.TargetStatusNeed = [StatusID.Stun, StatusID.Stun_1343, StatusID.Stun_142, StatusID.Stun_149, StatusID.Stun_1513, StatusID.Stun_1521, StatusID.Stun_1522, StatusID.Stun_201, StatusID.Stun_2656, StatusID.Stun_2953, StatusID.Stun_3408, StatusID.Stun_4163, StatusID.Stun_4374, StatusID.Stun_4378, StatusID.Stun_4433, StatusID.Stun_5043];
	}

	static partial void ModifyIceSpikesPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Ice;
		setting.StatusProvide = [StatusID.IceSpikes_1720, StatusID.VeilOfTheWhorl_1724, StatusID.Schiltron];
		setting.IsFriendly = true;
		setting.TargetType = TargetType.Self;
	}

	static partial void ModifyBloodDrainPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
	}

	static partial void ModifyAcornBombPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsSleepSpell = true;
		setting.TargetStatusProvide = [StatusID.Sleep];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBombTossPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Fire;
		setting.IsStunSpell = true;
		setting.TargetStatusProvide = [StatusID.Stun];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyOffguardPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.Offguard];
	}

	static partial void ModifySelfdestructPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Fire;
		setting.ActionCheck = () => !StatusHelper.PlayerHasStatus(true, StatusID.BrushWithDeath);
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyTransfusionPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.ActionCheck = () => !StatusHelper.PlayerHasStatus(true, StatusID.BrushWithDeath);
		setting.IsFriendly = true;
	}

	static partial void ModifyFazePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsStunSpell = true;
		setting.TargetStatusProvide = [StatusID.Stun];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyFlyingSardinePvE(ref ActionSetting setting)
	{
		//setting.TargetType = TargetType.Interrupt;
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
		setting.IsInterruptSpell = true;
	}

	static partial void ModifySnortPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Wind;
		setting.IsFriendly = false;
		setting.IsInterruptSpell = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
		setting.ActionCheck = () =>
		{
			if (Player == null)
			{
				return false;
			}

			var playerFace = Player.GetFaceVector();
			var playerPos = Player.Position;
			const float snortRange = 6f;
			const double coneHalfAngle = Math.PI / 4; // 45 degrees each side = 90 degree cone
			foreach (var target in DataCenter.AllHostileTargets)
			{
				if (!target.CanInterrupt())
				{
					continue;
				}

				if (target.DistanceToPlayer() > snortRange)
				{
					continue;
				}

				var dir = Vector3.Normalize(target.Position - playerPos);
				var angle = playerFace.AngleTo(dir);
				if (angle <= coneHalfAngle)
				{
					return true;
				}
			}
			return false;
		};
	}

	static partial void Modify_4TonzeWeightPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.IsHeavySpell = true;
		setting.TargetStatusProvide = [StatusID.Heavy];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyTheLookPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		//setting.TargetType = TargetType.Provoke;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBadBreathPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.TargetStatusProvide = [StatusID.Slow, StatusID.Heavy, StatusID.Blind, StatusID.Paralysis, StatusID.Poison, StatusID.Malodorous];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyDiamondbackPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.Diamondback];
		setting.IsFriendly = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMightyGuardPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.MightyGuard];
	}

	static partial void ModifyStickyTonguePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.IsStunSpell = true;
		setting.TargetStatusProvide = [StatusID.Stun];
	}

	static partial void ModifyToadOilPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.ToadOil];
		setting.IsFriendly = true;
	}

	static partial void ModifyTheRamsVoicePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Ice;
		setting.TargetStatusProvide = [StatusID.DeepFreeze_1731];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyTheDragonsVoicePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.IsParalysisSpell = true;
		setting.TargetStatusProvide = [StatusID.Paralysis];
		setting.TargetStatusNeed = [StatusID.DeepFreeze_1731];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMissilePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetType = TargetType.HighHPPercent;
		setting.IsFlatDamageDeath = true;
	}

	static partial void Modify_1000NeedlesPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
		setting.TargetType = TargetType.LowHP;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyInkJetPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsBlindSpell = true;
		setting.TargetStatusProvide = [StatusID.Blind];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyFireAngonPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
		setting.AdditionalAspects = [Aspect.Fire];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMoonFlutePvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.WaxingNocturne];
		setting.IsFriendly = true;
	}

	static partial void ModifyTailScrewPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFlatDamageDeath = true;
	}

	static partial void ModifyMindBlastPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsParalysisSpell = true;
		setting.TargetStatusProvide = [StatusID.Paralysis];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 2,
		};
	}

	static partial void ModifyDoomPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.Doom_1738];
		// "Has no effect on enemies whose level is higher than your own" - the fixed-or-lethal class (A507).
		setting.IsFlatDamageDeath = true;
	}

	static partial void ModifyPeculiarLightPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.PeculiarLight];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyFeatherRainPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Wind;
		setting.TargetStatusProvide = [StatusID.Windburn_1723];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyEruptionPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Fire;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMountainBusterPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.AdditionalAspects = [Aspect.Earth];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyShockStrikePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyGlassDancePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Ice;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyVeilOfTheWhorlPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
		setting.StatusProvide = [StatusID.IceSpikes_1720, StatusID.VeilOfTheWhorl_1724, StatusID.Schiltron];
		setting.TargetType = TargetType.Self;
		setting.IsFriendly = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyAlpineDraftPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Wind;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyProteanWavePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyNortherliesPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Ice;
		setting.TargetStatusNeed = [StatusID.Dropsy_1736];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyElectrogenesisPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyKaltstrahlPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Slashing;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyAbyssalTransfixionPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
		setting.IsParalysisSpell = true;
		setting.TargetStatusProvide = [StatusID.Paralysis];
	}

	static partial void ModifyChirpPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsSleepSpell = true;
		setting.TargetStatusProvide = [StatusID.Sleep];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyEerieSoundwavePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusNeed = [StatusID.VulnerabilityDown, StatusID.CriticalStrikes_1797];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyPomCurePvE(ref ActionSetting setting)
	{
		setting.IsFriendly = true;
	}

	static partial void ModifyGobskinPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.StatusProvide = [StatusID.Gobskin];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMagicHammerPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		// StatusProvide = [Conked] used to sit here. It never fired - Conked is the debuff on the
		// target and StatusProvide is read against the player - and firing is not what one would
		// want either: Magic Hammer is a 250-potency area attack that also restores 1000 MP, used
		// from BLU_Reborn.UseFiller. Conked is its side effect, not its purpose, so a lockout while
		// Conked is up would cost damage and MP to avoid refreshing a debuff nobody waits on.

		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyAvailPvE(ref ActionSetting setting)
	{
		//setting.StatusProvide = [StatusID.Avail];
	}

	static partial void ModifyFrogLegsPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetType = TargetType.Provoke;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySonicBoomPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Wind;
	}

	static partial void ModifyWhistlePvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.Boost_1716, StatusID.Harmonized];
		setting.IsFriendly = true;
	}

	static partial void ModifyWhiteKnightsTourPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsSlowSpell = true;
		setting.TargetStatusProvide = [StatusID.Slow];
		// Effect text: "Potency is increased to 400 when enemies are bound. The Bind effect is also removed." (A507)
		setting.TargetStatusNeed = [StatusID.Bind];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBlackKnightsTourPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		// Effect text: "Additional Effect: Bind" (A507); it was read as Blind.
		setting.IsBindSpell = true;
		setting.TargetStatusProvide = [StatusID.Bind];
		setting.TargetStatusNeed = [StatusID.Slow];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyLevel5DeathPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.IsFlatDamageDeath = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyLauncherPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.IsFlatDamageDeath = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyPerpetualRayPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsStunSpell = true;
		setting.TargetStatusProvide = [StatusID.Stun];
	}

	static partial void ModifyCactguardPvE(ref ActionSetting setting)
	{
		setting.TargetStatusProvide = [StatusID.Cactguard];
		setting.TargetType = TargetType.BeAttacked;
	}

	static partial void ModifyRevengeBlastPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.ActionCheck = () => Player?.GetHealthRatio() > 0.2f;
	}

	static partial void ModifyAngelWhisperPvE(ref ActionSetting setting)
	{
		setting.TargetType = TargetType.Death;
	}

	static partial void ModifyExuviationPvE(ref ActionSetting setting)
	{
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyRefluxPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.IsHeavySpell = true;
		setting.TargetStatusProvide = [StatusID.Heavy];
	}

	static partial void ModifyDevourPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.StatusProvide = [StatusID.HpBoost_2120];
	}

	static partial void ModifyCondensedLibraPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.PhysicalAttenuation, StatusID.AstralAttenuation, StatusID.UmbralAttenuation];
	}

	static partial void ModifyAethericMimicryPvE(ref ActionSetting setting)
	{
		setting.IsFriendly = true;
		setting.TargetType = TargetType.MimicryTarget;
		setting.StatusProvide =
			[StatusID.AethericMimicryDps, StatusID.AethericMimicryHealer, StatusID.AethericMimicryTank];
	}

	static partial void ModifySurpanakhaPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Earth;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyQuasarPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyJKickPvE(ref ActionSetting setting)
	{
		// A leap onto the target like Flying Frenzy (effect text), up to 25 yalms (concept 05, A519).
		setting.SpecialType = SpecialActionType.HostileMovingAttack;
		setting.CanTarget = static t => GapCloserAllowed(t, pressed: ActionID.JKickPvE);
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.ActionCheck = () => !StatusHelper.PlayerHasStatus(false, StatusID.Bind);
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyTripleTridentPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
	}

	static partial void ModifyTinglePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Lightning;
		setting.StatusProvide = [StatusID.Tingling];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyTatamigaeshiPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		// Effect text: "Additional Effect: Stun, Duration: 3s" (A507); the setting named Tingle's own status.
		setting.IsStunSpell = true;
		setting.TargetStatusProvide = [StatusID.Stun];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyColdFogPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.ColdFog];
		setting.TargetType = TargetType.Self;
		setting.IsFriendly = true;
	}

	static partial void ModifyWhiteDeathPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Ice;
		setting.StatusNeed = [StatusID.TouchOfFrost];
		setting.RequiredBluSlotActionId = 23267; // ColdFogPvE
		setting.TargetStatusProvide = [StatusID.DeepFreeze_1731];
	}

	static partial void ModifyStotramPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.ActionCheck = () => !IsHealer;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
	}

	static partial void ModifyStotramPvE_23416(ref ActionSetting setting)
	{
		setting.IsFriendly = true;
		setting.ActionCheck = () => IsHealer;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySaintlyBeamPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyFeculentFloodPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Earth;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyAngelsSnackPvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.AngelsSnack];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyChelonianGatePvE(ref ActionSetting setting)
	{
		setting.IsFriendly = true;
		setting.TargetType = TargetType.Self;
		setting.StatusProvide = [StatusID.ChelonianGate];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyDivineCataractPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
		setting.StatusNeed = [StatusID.AuspiciousTrance];
		setting.RequiredBluSlotActionId = 23273; // ChelonianGatePvE
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyTheRoseOfDestructionPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBasicInstinctPvE(ref ActionSetting setting)
	{
		setting.IsFriendly = true;
		setting.StatusProvide = [StatusID.BasicInstinct];
		setting.ActionCheck = () => IsInDuty && AliveOtherPartyMemberCount == 0 && DataCenter.Territory?.ContentType != TerritoryContentType.TheMaskedCarnivale;
	}

	static partial void ModifyUltravibrationPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusNeed = [StatusID.DeepFreeze_1731, StatusID.Petrification];
		setting.IsFriendly = false;
		setting.IsFlatDamageDeath = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBlazePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Ice;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMustardBombPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Fire;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyDragonForcePvE(ref ActionSetting setting)
	{
		setting.StatusProvide = [StatusID.DragonForce];
		setting.TargetType = TargetType.Self;
		setting.IsFriendly = true;
	}

	static partial void ModifyAetherialSparkPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.Bleeding_1714];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyHydroPullPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMaledictionOfWaterPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
		setting.IsFriendly = false;
		setting.ActionCheck = () => InCombat;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyChocoMeteorPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMatraMagicPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
	}

	static partial void ModifyPeripheralSynthesisPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		// StatusProvide and StatusNeed both held Lightheaded_2501 here, and both are read against
		// the player, who never carries it - it is the debuff this spell puts on its target. The
		// StatusNeed half therefore blocked the action outright: IsStatusNeeded reports true
		// whenever the status is absent, so PeripheralSynthesisPvE.CanUse could not succeed without
		// skipStatusNeed. BLU_Reborn passed a different skip flag at each of its two call sites,
		// and the one in SpendCooldowns picked skipStatusProvideCheck - that line could never fire.
		//
		// Neither belongs on the target side either. Lightheaded raises this spell's potency from
		// 220 to 400 while it is up, so a lockout would suppress the action exactly when it is
		// strongest, and a requirement would prevent the first cast that applies it.

		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBothEndsPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyPhantomFlurryPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.IsFriendly = false;
		setting.ActionCheck = () => !IsMoving;
		setting.StatusProvide = [StatusID.PhantomFlurry];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyPhantomFlurryPvE_23289(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.IsFriendly = false;
		setting.ActionCheck = () => !IsMoving;
		setting.StatusNeed = [StatusID.PhantomFlurry];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyNightbloomPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.TargetStatusProvide = [StatusID.Bleeding_1714];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyGoblinPunchPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.StatusNeed = [StatusID.MightyGuard];
	}

	static partial void ModifyRightRoundPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.ActionCheck = () => InCombat;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySchiltronPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.StatusProvide = [StatusID.IceSpikes_1720, StatusID.VeilOfTheWhorl_1724, StatusID.Schiltron];
		setting.IsFriendly = true;
		setting.TargetType = TargetType.Self;
	}

	static partial void ModifyRehydrationPvE(ref ActionSetting setting)
	{
		setting.IsFriendly = true;
		setting.TargetType = TargetType.Self;
	}

	static partial void ModifyBreathOfMagicPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.BreathOfMagic];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyWildRagePvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.ActionCheck = () => Player?.GetEffectiveHpPercent() > 50f;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyPeatPeltPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Earth;
		setting.TargetStatusProvide = [StatusID.Begrimed];
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyDeepCleanPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Blunt;
		setting.TargetStatusNeed = [StatusID.Begrimed];
		setting.ActionCheck = () => StatusHelper.PlayerStatusStack(true, StatusID.Spickandspan) < 6;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyRubyDynamicsPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Slashing;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyDivinationRunePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 3,
		};
		setting.IsFriendly = false;
	}

	static partial void ModifyDimensionalShiftPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		// "Has no effect on enemies whose level is higher than your own" - the fixed-or-lethal class (A507).
		setting.IsFlatDamageDeath = true;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyConvictionMarcatoPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyForceFieldPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = true;
		setting.TargetType = TargetType.Self;
	}

	static partial void ModifyWingedReprobationPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Physical;
		setting.AspectOverride = Aspect.Piercing;
		setting.StatusProvide = [StatusID.WingedReprobation, StatusID.WingedRedemption];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyLaserEyePvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyCandyCanePvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.TargetStatusProvide = [StatusID.CandyCane];
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyMortalFlamePvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Fire;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifySeaShantyPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Water;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyApokalypsisPvE(ref ActionSetting setting)
	{
		//Need data
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	static partial void ModifyBeingMortalPvE(ref ActionSetting setting)
	{
		setting.AttackTypeOverride = AttackType.Magic;
		setting.AspectOverride = Aspect.Unaspected;
		setting.IsFriendly = false;
		setting.CreateConfig = () => new ActionConfig()
		{
			AoeCount = 1,
		};
	}

	/// <summary>
	///
	/// </summary>
	public override void DisplayBaseStatus()
	{
		ImGui.TextWrapped($"Aetheric Mimicry Role: {BlueId}");
	}
}
