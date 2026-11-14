using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

public sealed partial class WHM_Reborn : WhiteMageRotation
{
	/// <summary>
	/// Whether Holy has to wait because a stronger mitigation is already carrying the pull, so the
	/// stun is worth less now than later.
	/// </summary>
	/// <remarks>
	/// The third of the three timings in concept 08: a non-Holy GCD is inserted when it has value of
	/// its own and the stun is not lost by it - because it still runs, because it no longer works,
	/// or because a stronger mitigation is carrying right now. The first two were implemented, this
	/// one was not, while the same document listed the hold as done.
	///
	/// Arm's Length applies Slow +20% to every physical attacker for 15s, and the slow raises
	/// auto-attack delay, which is where trash damage comes from - the same order as Rampart. While
	/// that runs, the stream is already thinned, and spending one of the pull's three stun
	/// applications on it burns a budget that is gone for good: 4s, then 2s, then 1s, then immunity.
	///
	/// The measure is a head count over Holy's own radius, and both halves of it are the user's:
	/// **more than half** the enemies in radius carry the slow, and at least
	/// <see cref="HoldHolyMinSlowedHostiles"/> of them do. The share says the stream as a whole is
	/// thinned rather than one straggler being clipped; the floor keeps a two-enemy remnant from
	/// satisfying the share by arithmetic alone.
	///
	/// The measure this replaced was total enemy output against AoeCount * 100 - the area rule
	/// restated in the unit mitigations are expressed in. It was mine rather than his, and in play
	/// it did nothing: its own documentation noted that it "only ever bites at exactly AoeCount
	/// enemies", because one body beyond that carries at least 80 on its own and clears the
	/// threshold however many are slowed. A wall-to-wall pull always holds more enemies than the
	/// cast needs, so the hold never fired there - which is exactly what the user observed, Holy
	/// going out into a slow that had caught almost every enemy. Five enemies with four slowed came
	/// to 432 against a threshold of 300.
	///
	/// What that shows is not a wrong number but a wrong question. Output asks "is this pull still
	/// worth an area cast", and the answer is yes almost always; the rule has to ask "is the stream
	/// already being handled", and that is a share, not a sum.
	///
	/// Its earlier forms measured the wrong set rather than the wrong quantity: the first borrowed
	/// DRK_Reborn.PackSlowed's share rule, which weighs the stream reaching the tank over job range
	/// instead of what this cast would hit; the next asked for a bare majority with no floor; the
	/// next counted the enemies the slow had not reached.
	///
	/// Held for as long as the condition stands, and no longer - the user's rule. A held GCD falls
	/// through to the DoTs while any enemy lacks one and to Glare after that. This rule used to carry
	/// the stun-stretch rule's replacement guarantee as well, releasing Holy as soon as no DoT was
	/// left to place. That guarantee belongs to the stretch, which needs exactly one inserted GCD; a
	/// slow lasts many. Carried over, it ended every hold after one DoT per enemy while the pack was
	/// still slowed - observed by the user: held after Arm's Length, then Holy again (A178).
	/// </remarks>
	private bool ShouldHoldHolyWhilePackSlowed()
	{
		if (!HoldHolyWhilePackSlowed)
		{
			_holySlowHold = "off";
			return false;
		}

		var holy = HolyIiiPvE.EnoughLevel ? HolyIiiPvE : HolyPvE;
		var radius = holy.Info.EffectRange;

		// No enemy left that this cast could stun, no reason to hold it back. The point of holding
		// is to keep the stun budget for a moment when the stream is not already thinned; once every
		// enemy in radius is stunned or immune, there is no budget left to keep, and yielding the
		// GCD trades an area cast for a single-target dot without buying anything at all.
		//
		// The user states it as the condition on every hold: only suspend Holy while the enemies can
		// still be stunned by it, otherwise it is pointless. ShouldStretchHolyStun carried this from
		// the start (A50); the two later holds did not, which is one cause in two places.
		SurveyStuns(radius, out _, out var headroom);
		if (!headroom)
		{
			_holySlowHold = "released - nothing in radius can be stunned any more";
			return false;
		}

		var inRange = SurveyHostileStatus(radius, StatusHelper.SlowStatus, out var slowed);

		// Nothing in radius says nothing at all - not "no slow".
		if (inRange == 0 || slowed < HoldHolyMinSlowedHostiles)
		{
			_holySlowHold = "not held - too few slowed in radius";
			return false;
		}

		// Strictly more than half, so a slow on exactly half the pack is not enough: 3 of 5 holds,
		// 3 of 6 does not, 4 of 6 does.
		if (slowed * 2 <= inRange)
		{
			_holySlowHold = "not held - half or fewer slowed in radius";
			return false;
		}

		// And only while what is left is still manageable. The share says the stream is being
		// throttled; it does not say the remainder can be healed through. Holding the stun back
		// while somebody is going down stretches a throttle the party does not survive long enough
		// to benefit from.
		//
		// Manageable is measured, not set. It used to be a number I invented - 600, sitting between
		// two figures the user had called manageable and hopeless - and a head count of enemies was
		// never the question anyway: a nine-enemy pull that the healing is keeping up with is
		// exactly where stretching the throttle pays, and a three-enemy pull that is killing the
		// tank is exactly where it does not. What decides is whether anyone actually falls inside
		// the GCD this hold costs, which the health trend answers per member.
		//
		// Nobody falling reads as NaN, and NaN is no death in sight rather than a short time, so a
		// party being held steady never trips this however large the pack.
		if (ObjectHelper.AnyPartyMemberFallingWithinHealWindow())
		{
			_holySlowHold = "released - a party member is falling";
			return false;
		}

		// The old sum is kept as an optional ceiling for whoever wants one, and defaults to off.
		// Removing the setting outright would discard a value already stored in user configuration;
		// leaving it at a figure I made up would keep deciding on it.
		if (HoldHolyMaxHostileOutput > 0)
		{
			_ = SurveyHostileOutput(radius, out var output);
			if (output > HoldHolyMaxHostileOutput)
			{
				_holySlowHold = "released - enemy output above the set ceiling";
				return false;
			}
		}

		_holySlowHold = "held - pack slowed";
		return true;
	}

	/// <inheritdoc/>
	public override void DisplayRotationStatus()
	{
		ImGui.Text("Holy while the pack is slowed: " + _holySlowHold);
	}

	/// <summary>
	/// Whether Holy has to wait because its stun would strand a barrier that pays off only when it
	/// is spent in full.
	/// </summary>
	/// <remarks>
	/// The Blackest Night grants Dark Arts only when its barrier - 25% of maximum HP over 7s - is
	/// absorbed completely (action 7393), and nothing at all when it is not. Stopping the damage
	/// stream for four of those seven seconds is therefore not a saving but a double loss: the
	/// barrier expires unspent, and the stun budget - about seven seconds per pull before the
	/// enemies turn immune - is gone with it. What the stun prevents does not buy that back, because
	/// in a wall-to-wall pull the damage it stops is the damage the barrier was absorbing anyway, on
	/// the same target: the enemies are on the tank, which is who holds the barrier.
	///
	/// This does not wait on the dark knight's rule, which holds the barrier back while a group stun
	/// runs. Both states expire on their own - the stun after four seconds, the barrier after seven
	/// - so neither side can hold the other indefinitely. They yield to whichever landed first, and
	/// when neither is up both simply go.
	///
	/// Two limits keep the cost bounded, and the cost is real: Holy is the only area spell this job
	/// has, so a held GCD falls through to single-target damage. Once every enemy in radius is
	/// immune the cast can no longer interrupt anything and goes out normally, which ends the hold
	/// for the rest of the pull; and a barrier that is over before this cast lands is not worth
	/// waiting for.
	///
	/// A third limit is the tank role. The barrier can sit on anyone - the action reads "self or
	/// target party member", and the dark knight's party branch aims it at the lowest HP - but the
	/// case this rule exists for is the one where the barrier and the stun protect the same person:
	/// the pull is on the tank, so the damage is too. On a damage dealer carrying the barrier, the
	/// stun is likelier to be what keeps them alive, and holding it back would trade a life for a
	/// resource.
	/// </remarks>
	private bool ShouldHoldHolyForBarrier()
	{
		if (!HoldHolyForBlackestNight)
		{
			return false;
		}

		// Group pulls only, by the user's qualification: in a boss fight the rule does not apply.
		// There the damage arrives as scripted single hits that break the barrier on their own
		// whatever the stun does, so holding Holy would give up its damage for a reward that is not
		// at risk. The same hostile count the stun stretch uses, rather than a second number.
		if (NumberOfHostilesInRange < StretchHolyMinHostiles)
		{
			return false;
		}

		var radius = HolyIiiPvE.EnoughLevel ? HolyIiiPvE.Info.EffectRange : HolyPvE.Info.EffectRange;
		_ = SurveyStuns(radius, out _, out _, out var headroom);
		if (!headroom)
		{
			return false;
		}

		var party = PartyMembers;
		if (party == null)
		{
			return false;
		}

		var cast = HolyIiiPvE.EnoughLevel ? HolyIiiPvE.Info.CastTime : HolyPvE.Info.CastTime;
		foreach (var member in party)
		{
			if (member == null || !member.IsJobCategory(JobRole.Tank))
			{
				continue;
			}

			// isFromSelf false: the barrier belongs to the dark knight, not to this healer.
			if (member.HasStatus(false, StatusHelper.FullAbsorbRewardStatus)
				&& !member.WillStatusEnd(cast, false, StatusHelper.FullAbsorbRewardStatus))
			{
				return true;
			}
		}

		return false;
	}

	public override bool CanHealSingleSpell => base.CanHealSingleSpell && GcdHealsAllowed(GCDHeal);

	public override bool CanHealAreaSpell => base.CanHealAreaSpell && GcdHealsAllowed(GCDHeal);
}
