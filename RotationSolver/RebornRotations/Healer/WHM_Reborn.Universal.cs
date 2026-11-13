using System.ComponentModel;

namespace RotationSolver.RebornRotations.Healer;

public sealed partial class WHM_Reborn : WhiteMageRotation
{
	/// <summary>A raise is pending with Swiftcast up for it, so no GCD method spends the cast - only while the raise takes
	/// the running Swiftcast; otherwise the next spell spends it, and it does not run out unused (A504).</summary>
	private bool SwiftRaisePending =>
		(HasSwift || IsLastAction(ActionID.SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise)
		&& RunningSwiftGoesToRaise;

	[RotationDesc(ActionID.AetherialShiftPvE)]
	protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
	{
		if (AetherialShiftPvE.CanUse(out act))
		{
			return true;
		}

		return base.MoveForwardAbility(nextGCD, out act);
	}

	/// <summary>
	/// MP has fallen to where Lucid Dreaming would be cast, and Lucid cannot answer it: it is on
	/// cooldown, not yet learned, or switched off. Thin Air is then the only MP relief left, which is
	/// what ThinAirOnMpPressureOnly reserves it for. The threshold is Lucid's own configured one
	/// rather than a second value, so both sides of the decision read the same number.
	/// </summary>
	private bool UnderMpPressure =>
		CurrentMp < Service.Config.LucidDreamingMpThreshold
		&& (!LucidDreamingPvE.EnoughLevel || !LucidDreamingPvE.IsEnabled
			|| LucidDreamingPvE.Cooldown.IsCoolingDown);

	/// <summary>
	/// Proactive wall-to-wall sustain: keep Regen up on the tank as they commit to a pull. Called from
	/// GeneralGCD, HealSingleGCD and HealAreaGCD alike - the outer dispatch reaches the two heal methods
	/// first, so a raised heal-need flag would otherwise starve the check in GeneralGCD for a whole
	/// pull. Never fires at or below <see cref="RegenHeal"/>, leaving a genuine emergency to Cure II /
	/// Cure. The rule itself is the healers' shared one, TryPullUpkeepOnTank.
	/// </summary>
	private bool TrySustainRegenOnTank(out IAction? act)
	{
		act = null;
		return UsePreRegen
			&& TryPullUpkeepOnTank(RegenPvE, PreRegenMinHostiles, PreRegenMinWallToWallHostiles,
				RegenHeal, RaisePvE, null, out act);
	}

	/// <summary>
	/// Whether this GCD should go to something other than Holy so the stun is not overwritten while
	/// it still runs.
	/// </summary>
	/// <remarks>
	/// The user's rule, without room for discretion: a stun from Holy lasts four seconds and its
	/// recast is two and a half, so a second Holy cast straight away lands inside the running stun
	/// and overwrites it instead of extending it. Another spell in between puts the second stun at
	/// the end of the first. A stun lasts 4s, then 2s, then 1s, after which the target is immune -
	/// seven seconds per pull, about 5.5s of them when recast on cooldown.
	///
	/// The question is whether a Holy started now would land while the last one's stun still runs.
	/// Holy has a cast time and stuns where the cast lands, so the stun is compared with the time to
	/// that landing, not with zero: after the 2s application the next cast lands after the stun
	/// anyway, and holding there would spend a GCD for nothing. That yields exactly one GCD per pull
	/// at any GCD length, Presence of Mind included (.github/scripts/audit/stun_coverage.py with a
	/// cast time).
	///
	/// The stun of a Holy that has just finished casting may not be in the enemies' status lists yet
	/// when the next GCD is chosen - cast and recast are equally long, so the next GCD is free the
	/// moment the cast lands. StunWatch knows it from the effect set, at the length the halving gives
	/// that application, and knows the enemies the cast found immune. Until A272 an enemy that was
	/// neither stunned nor carrying a resistance status counted as about to receive a full four
	/// seconds; the game shows no such status for the halving, so every pack that could no longer be
	/// stunned held the next Holy all the same (trace of 04-10 October: 134 of 180 decisions).
	///
	/// Nothing else is weighed. The held GCD goes to a DoT where one is due and to Glare otherwise;
	/// the damage lost is not set against the stun, because keeping the party alive ranks above
	/// dealing damage. Whether Holy is worth casting at all stays with Holy's own area check. The one
	/// condition above every hold is the user's too: once nothing in the radius can be stunned any
	/// more, there is nothing to stretch - and then no stun runs that a Holy could overwrite.
	/// </remarks>
	private bool ShouldStretchHolyStun()
	{
		if (!StretchHolyStun)
		{
			return false;
		}

		// "Between the first and the second Holy": only the GCD right after a Holy is held. After the
		// spell in between, the next Holy goes out whatever is running.
		if (!IsLastGCD(true, HolyPvE, HolyIiiPvE))
		{
			return false;
		}

		var holy = HolyIiiPvE.EnoughLevel ? HolyIiiPvE : HolyPvE;
		var radius = holy.Info.EffectRange;
		var stunLasts = LongestStunInRadius(radius, out var application);
		var lands = WeaponRemain + ((ActionID)holy.ID).GetCastTime();

		// Each outcome is written to DefenseTrace.log with its reason: the owner reported the second
		// Holy going out at once (29.09.2026), and which case let it through is not readable from the
		// code.
		if (stunLasts <= 0)
		{
			TraceStretch("not held: nothing in Holy's radius is stunned");
			return false;
		}

		if (stunLasts <= lands)
		{
			TraceStretch($"not held: the stun ends in {stunLasts:F1}s, before a Holy cast now lands in {lands:F1}s");
			return false;
		}

		TraceStretch($"held: the stun runs {stunLasts:F1}s (application {application}), a Holy cast now would land inside it in {lands:F1}s");
		return true;
	}

	/// <summary>
	/// The longest stun still running on a hostile inside the radius, from any source, including one
	/// that has landed and is not in its status list yet (<see cref="StunWatch.StunLeft"/>).
	/// </summary>
	/// <param name="radius">How far around the player hostiles are counted, in yalms.</param>
	/// <param name="application">Which application of a stun the longest one is, zero when unknown.</param>
	private static float LongestStunInRadius(float radius, out int application)
	{
		application = 0;
		var longest = 0f;
		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null)
		{
			return 0f;
		}

		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var hostile = hostiles[i];
			if (hostile == null || hostile.DistanceToPlayer() > radius)
			{
				continue;
			}

			var left = StunWatch.StunLeft(hostile);
			if (left > longest)
			{
				longest = left;
				application = StunWatch.Applications(hostile);
			}
		}

		return longest;
	}

	// The GCD path asks every frame; a line is written when the outcome changes or once a GCD has passed.
	// The outcome is compared without its figures: compared with them, every frame read as a change, and
	// the trace of 16.10.2026 holds about ten such lines a second, over half the file (A297).
	private void TraceStretch(string outcome)
	{
		var now = DateTime.Now;
		var kind = StretchFigures.Replace(outcome, "#");
		if (kind == _lastStretchTrace && (now - _lastStretchTraceAt).TotalSeconds < DataCenter.DefaultGCDTotal)
		{
			return;
		}

		_lastStretchTrace = kind;
		_lastStretchTraceAt = now;
		DefenseTrace.Line("White Mage, Holy stretch " + outcome);
	}
}
