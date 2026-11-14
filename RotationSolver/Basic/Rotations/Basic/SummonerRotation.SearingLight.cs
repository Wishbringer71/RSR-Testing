using Dalamud.Interface.Colors;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class SummonerRotation
{
	/// <summary>
	/// Did somebody else's Searing Light stand at any time in the own last burst window (Solar Bahamut,
	/// below its level Demi-Bahamut), or does it stand in the current one?
	///
	/// The owner's rule moves to Bahamut or Phoenix only when another Summoner got there first
	/// (concept 12, points 2 to 4); without that, a charge that comes free in a minor phase waits for the
	/// next Solar instead of drifting onto the minor phases for the rest of the fight. Observed over the
	/// whole window rather than on entering it: the phase book judges on entry, and a buff cast a moment
	/// later went unseen - in the model that cost up to ten points in a synchronised pull
	/// (searing_light_coverage.py, 'plugin_narrow').
	/// </summary>
	protected bool BurstPhaseHeldByAnother => _burstHeldLastWindow || _burstHeldThisWindow;

	/// <summary>
	/// Is every burst phase held by somebody who keeps coming back?
	///
	/// This is the only question the book answers, and the distinction matters: it does **not** say
	/// which phase to aim at. Holding back applies to the phase at hand, not as a matter of
	/// principle - so every free burst phase is fair game whenever no buff is running, and only
	/// when all three are spoken for is there a reason to look outside them at all.
	///
	/// Only the phases this level has are asked. Solar Bahamut does not exist below its level and
	/// Phoenix not below its own, so requiring them made the answer "no" in every level-synced duty,
	/// and the fallback into a primal block could never be reached there.
	/// </summary>
	protected bool AllSearingPhasesHeld
	{
		get
		{
			if (!SummonBahamutPvE.EnoughLevel)
			{
				return false;
			}

			if (!SearingPhaseHeld(SearingPhase.Bahamut))
			{
				return false;
			}

			if (SummonSolarBahamutPvE.EnoughLevel)
			{
				return SearingPhaseHeld(SearingPhase.Solar);
			}

			return !SummonPhoenixPvE.EnoughLevel || SearingPhaseHeld(SearingPhase.Phoenix);
		}
	}

	/// <summary>
	/// Was this phase found occupied by a foreign Searing Light on the last two entries?
	/// </summary>
	protected bool SearingPhaseHeld(SearingPhase phase) => _searingPhaseHeld[(int)BookSlot(phase)];

	/// <summary>
	/// Was this phase found occupied on the last entry - one sighting, not yet a pattern? For the
	/// rotation status, so the tester can see what the rule decides from.
	/// </summary>
	protected bool SearingPhaseSeen(SearingPhase phase) => _searingPhaseSeen[(int)BookSlot(phase)];

	/// <summary>
	/// Where a phase is booked. With Solar Bahamut in the cycle, Demi-Bahamut and Demi-Phoenix take
	/// the windows between two Solars in turn, and Searing Light's 120 s recast is two windows - so a
	/// Summoner seen in one of them returns in the other, and both are spoken for (concept 12,
	/// "Das Phasenbuch"). Booking them apart needed two sightings of each, every 240 s, before
	/// either counted as held. Below Solar's level the two alternate every window and a caster
	/// returns to the same one, so they are booked apart.
	/// </summary>
	private SearingPhase BookSlot(SearingPhase phase) =>
		phase == SearingPhase.Phoenix && SummonSolarBahamutPvE.EnoughLevel
			? SearingPhase.Bahamut
			: phase;

	/// <summary>
	/// Keeps the phase book, once per window entered rather than once per frame.
	///
	/// Entering a burst phase while somebody else's Searing Light is running marks it seen, and held
	/// if it was seen the time before as well; entering it and finding no foreign buff clears both. That is
	/// what makes the book self-healing without a clock: a Summoner who stops casting - died,
	/// left, switched job - stops being found there, and his phase comes back on the next pass. No
	/// grace period has to be guessed, and no reset point beyond leaving combat is needed.
	/// </summary>
	protected void UpdateSearingPhaseBook()
	{
		if (!DataCenter.InCombat)
		{
			Array.Clear(_searingPhaseSeen, 0, _searingPhaseSeen.Length);
			Array.Clear(_searingPhaseHeld, 0, _searingPhaseHeld.Length);
			_lastSearingPhase = SearingPhase.None;
			_lastBigSummon = SearingPhase.None;
			_demiNamesTaken = false;
			_searingPhaseBooked = false;
			_inBurstWindow = _burstHeldThisWindow = _burstHeldLastWindow = false;
			return;
		}

		var phase = CurrentSearingPhase;
		CheckDemiNames(phase);

		// The burst window, watched over its whole length (BurstPhaseHeldByAnother).
		var inBurst = phase == (SummonSolarBahamutPvE.EnoughLevel ? SearingPhase.Solar : SearingPhase.Bahamut);
		if (inBurst != _inBurstWindow)
		{
			if (_inBurstWindow)
			{
				_burstHeldLastWindow = _burstHeldThisWindow;
			}

			_burstHeldThisWindow = false;
			_inBurstWindow = inBurst;
		}

		if (inBurst && HasAnySearingLight && !HasSearingLight)
		{
			_burstHeldThisWindow = true;
		}

		if (phase != _lastSearingPhase)
		{
			_lastSearingPhase = phase;
			_searingPhaseBooked = false;
			if (phase != SearingPhase.None)
			{
				_lastBigSummon = phase;
			}
		}

		if (phase == SearingPhase.None || _searingPhaseBooked)
		{
			return;
		}

		// The player's own buff says nothing about whether anyone else holds this phase, so it
		// neither books nor clears - the window is simply left unjudged and asked again later in the
		// same phase. Without this a charge spent in Solar would clear the Bahamut entry on entry,
		// on the strength of a buff the player cast himself.
		if (HasSearingLight)
		{
			return;
		}

		_searingPhaseBooked = true;
		var slot = (int)BookSlot(phase);
		var occupied = HasAnySearingLight;
		_searingPhaseHeld[slot] = occupied && _searingPhaseSeen[slot];
		_searingPhaseSeen[slot] = occupied;
	}

	/// <inheritdoc/>
	protected override void UpdateInfo()
	{
		base.UpdateInfo();
		UpdateSearingPhaseBook();
	}

	/// <summary>
	/// Is there another Summoner in the party who could cast Searing Light?
	///
	/// Alliance members are not asked: Searing Light reaches nearby party members, so a Summoner in
	/// another alliance party never buffs this player. The dead are not asked either - they cast
	/// nothing. The level comes from the action's own data rather than a literal, and it is applied
	/// to the other player for the same reason it is applied here: a Summoner below it has no
	/// Searing Light to give, and counting him would hold this one back for a buff that cannot come.
	/// That is the mistake <see cref="DataCenter.AnyLivingRaiser(bool)"/> made with Red Mage.
	///
	/// What this cannot answer is whether the other Summoner runs this rotation, runs any plugin at
	/// all, or plays the job well. It answers "can a second Searing Light exist here", which is the
	/// question the firing window turns on.
	/// </summary>
	protected bool AnotherSummonerInParty
	{
		get
		{
			var members = DataCenter.PartyMembers;
			if (members == null)
			{
				return false;
			}

			foreach (var member in members)
			{
				if (member == null || member.IsDead || member.IsPlayer())
				{
					continue;
				}

				if (member.IsJobs(ECommons.ExcelServices.Job.SMN)
					&& member.Level >= SearingLightPvE.Level)
				{
					return true;
				}
			}

			return false;
		}
	}

	private bool _demiNamesTaken;

	private static bool _gaugeMisnamedDemi;

	private static bool _buttonMisnamedDemi;

	private SearingPhase BigSummonOnGauge =>
		SummonSolarBahamutPvE.EnoughLevel && IsSolarBahamutReady ? SearingPhase.Solar
		: JobGauge.IsPhoenixReady ? SearingPhase.Phoenix
		: SearingPhase.Bahamut;

	private static SearingPhase BigSummonOnButton => Service.GetAdjustedActionId(ActionID.SummonBahamutPvE) switch
	{
		ActionID.SummonSolarBahamutPvE => SearingPhase.Solar,
		ActionID.SummonPhoenixPvE => SearingPhase.Phoenix,
		ActionID.SummonBahamutPvE => SearingPhase.Bahamut,
		_ => SearingPhase.None,
	};

	/// <summary>
	/// Holds the names gauge and button gave in the slot ahead of the summon against the demi that
	/// came. Taken once, when the summon first comes within reach of the next GCD - the moment
	/// Searing Light is decided on - and compared on entering the demi.
	/// </summary>
	private void CheckDemiNames(SearingPhase phase)
	{
		if (!SummonPhoenixPvE.EnoughLevel)
		{
			return;
		}

		if (phase == SearingPhase.None)
		{
			var cooldown = SummonSolarBahamutPvE.EnoughLevel ? SummonSolarBahamutPvE.Cooldown : SummonBahamutPvE.Cooldown;
			if (!_demiNamesTaken && cooldown.WillHaveOneCharge(WeaponRemain))
			{
				_gaugeNamed = BigSummonOnGauge;
				_buttonNamed = BigSummonOnButton;
				_demiNamesTaken = true;
			}

			return;
		}

		if (phase != _lastSearingPhase && _demiNamesTaken)
		{
			_gaugeMisnamedDemi |= _gaugeNamed != phase;
			_buttonMisnamedDemi |= _buttonNamed != SearingPhase.None && _buttonNamed != phase;
			_demiNamesTaken = false;
		}
	}
}
