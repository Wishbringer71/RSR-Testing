using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

public sealed partial class DRK_Reborn : DarkKnightRotation
{
	// The barrier breaks, and grants Dark Arts, only when it absorbs its share of maximum HP within its duration (effect
	// text: 25 % in 7 s). What the player took over the last such span answers whether this pull's stream does that,
	// after whatever mitigation already stood - which is what the barrier would meet. It replaces the assumption that
	// enough enemies reach that rate, which nothing checked (concept 10, A340). Without a figure from the game it does
	// not hold the barrier back.
	private bool StreamWouldBreakBarrier()
	{
		var share = DefensiveValues.For(TheBlackestNightPvE.ID).Barrier;
		var lasts = DefensiveValues.DurationOf(TheBlackestNightPvE.ID);
		return share <= 0f || lasts <= 0f || DataCenter.DamageTakenWithin(lasts) >= share;
	}

	private bool _barrierOnSelf;

	private bool _darkArtsBeforeBarrier;

	private float _streamBeforeBarrier;

	// Whether the player's own barrier broke: Dark Arts comes with the break, so it is read when the barrier goes. One
	// line per barrier in DefenseTrace.log, with the stream that came before it - the check on the rule above, without
	// a reading by the owner as part of it.
	protected override void UpdateInfo()
	{
		base.UpdateInfo();
		var on = Player != null && Player.HasStatus(true, StatusHelper.FullAbsorbRewardStatus);
		var lasts = DefensiveValues.DurationOf(TheBlackestNightPvE.ID);
		if (on && !_barrierOnSelf)
		{
			_darkArtsBeforeBarrier = HasDarkArts;
			_streamBeforeBarrier = DataCenter.DamageTakenWithin(lasts);
		}
		else if (!on && _barrierOnSelf && DataCenter.InCombat)
		{
			DefenseTrace.Line("The Blackest Night on you "
				+ (_darkArtsBeforeBarrier ? "ended, Dark Arts was already held" : HasDarkArts ? "broke (Dark Arts)" : "ran out unbroken")
				+ $"; {_streamBeforeBarrier:P0} of max HP taken in the {lasts:F0} s before it, {NumberOfHostilesInRange} enemies in range");
		}

		_barrierOnSelf = on;
	}

	private bool ShouldUseBlackestNightOnSelf()
	{
		// The free mitigations first. Both cost nothing but their cooldown and sit at or near the
		// end of this path, so while The Blackest Night is off cooldown every opportunity goes to
		// the 3000 MP action and the cheap ones only land once it happens to be unavailable. In a
		// pull that ordering is backwards; against a tankbuster it does not matter, which is why
		// this only gates the pull branch.
		var reprisalDone = !ReprisalPvE.EnoughLevel
			|| ReprisalPvE.Cooldown.IsCoolingDown
			|| (HostileTarget?.HasStatus(false, StatusHelper.ReprisalStatus) ?? false);

		// Arm's Length is the second one. "Done" means the same thing: either it cannot be cast, or
		// it already is - a cooling-down Arm's Length is one that has been spent on this pull.
		var armsLengthDone = !UseArmsLengthOnPull
			|| !ArmsLengthPvE.EnoughLevel
			|| ArmsLengthPvE.Cooldown.IsCoolingDown;

		// In a pull the barrier has to stand on its own: a big mitigation running alongside drops
		// the damage stream below the rate that spends it, a stun chain stops the stream outright,
		// and a slowed pack thins it for as long as the debuff lasts.
		var staggeredHeavyPull = NumberOfHostilesInRange >= BlackestNightMinHostiles
			&& !HasMajorMitigation
			&& !GroupStunRunning()
			&& !PackSlowed()
			&& reprisalDone
			&& armsLengthDone
			&& (!BlackestNightWaitsForStream || StreamWouldBreakBarrier());

		return BlackestNightUsage switch
		{
			BlackestNightStrategy.TankbusterOrHeavyPull => TankbusterOnMe || staggeredHeavyPull,
			// Low health is an emergency, so it deliberately skips the stagger condition.
			BlackestNightStrategy.TankbusterHeavyPullOrLowHealth =>
				TankbusterOnMe || staggeredHeavyPull
				|| Player?.GetHealthRatio() <= BlackestNightHealthRatio,
			_ => true,
		};
	}

	/// <summary>
	/// Whether Bloodspiller or Quietus may spend Blood now: inside the burst, or where the Blood
	/// would not fit into the next Delirium. Outside it the Blood is pooled for the burst.
	/// </summary>
	/// <remarks>
	/// The Balance: "Use Bloodspiller under raid buffs, or to prevent overcapping on blood", and
	/// Bloodspiller "can be delayed without loss, as long as ... Blood Gauge do[es] not overcap". The
	/// property stood here unread since the rotation rework, built around a Blood cost of Living
	/// Shadow the action no longer has; Bloodspiller went out at 50 Blood whenever it could (A197).
	/// Under Delirium the two actions cost nothing (their own check lets them through then).
	/// </remarks>
	private bool UseBlood
	{
		get
		{
			if (!DeliriumPvE.EnoughLevel || DeliriumStacks > 0 || LowDeliriumStacks > 0)
			{
				return true;
			}

			if (StatusHelper.PlayerHasStatus(true, StatusID.Delirium_1972, StatusID.Delirium_3836) || InTwoMIsBurst)
			{
				return true;
			}

			return Blood > BloodPoolLimit;
		}
	}

	// "Keep at least 3000 MP" keeps what The Blackest Night costs: Edge or Flood spends only what lies above it, both
	// costs read from the game. Darkside running out spent the reserve too, and the barrier was missing for the next
	// tankbuster (A406).
	private bool EdgeKeepsBlackestNightMp => !TheBlackestNight || HasDarkArts
		|| CurrentMp >= TheBlackestNightPvE.Info.MPNeed + EdgeOfDarknessPvE.Info.MPNeed;
}
