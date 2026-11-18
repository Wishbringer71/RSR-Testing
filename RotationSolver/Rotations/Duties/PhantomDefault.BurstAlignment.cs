namespace RotationSolver.Rotations.Duties;

/// <summary>
/// Concept 03, "Der Burst des Jobs als Uhr, und ein Verstärker, der ihm folgt: Phantom Aim" (A885): Phantom Aim waits
/// for the job's next burst when waiting costs no use over the rest of the fight.
/// </summary>
public sealed partial class PhantomDefault
{
	[RotationConfig(CombatType.PvE, Name = "Hold Phantom Aim for your burst while that costs no use in the fight")]
	public bool AimWaitsForBurst { get; set; } = true;

	// The bursts as a buff of Phantom Aim's length sees them: a new one after a gap longer than Aim lasts.
	private static readonly BurstClock AimClock = new(() => DefensiveValues.DurationOf((uint)ActionID.PhantomAimPvE));

	// The hold written in this fight: the pull it began in, its combat second and the wait it expected.
	private bool _aimHoldWritten;
	private float _aimHoldPull;
	private float _aimHoldAt;
	private float _aimHoldWait;

	/// <summary>
	/// Whether a ready Phantom Aim waits for the burst: not in a burst now, the next one due in w s by the measured
	/// cadence, and pressing now gives no more uses over the time the target has left than waiting -
	/// ⌊R/r⌋ = ⌊(R − w)/r⌋ with r the recast. Then every use lands in a burst and none is lost.
	/// </summary>
	private bool HoldsAimForBurst()
	{
		if (_aimHoldWritten && DataCenter.LastPullSeconds != _aimHoldPull)
		{
			_aimHoldWritten = false;
			DefenseTrace.Line($"phantom aim: hold ended with the fight, {DataCenter.LastPullSeconds - _aimHoldAt:F0} s after it began,"
				+ $" with the burst due {_aimHoldWait:F1} s after it");
		}

		if (!AimWaitsForBurst || BurstClock.InBurstNow)
		{
			_aimHoldWritten = false;
			return false;
		}

		var wait = AimClock.SecondsToNext;
		var remaining = HostileTarget?.GetCorrectedTTK() ?? float.NaN;
		var recast = PhantomAimPvE.Cooldown.RecastTimeOneChargeRaw;
		if (float.IsNaN(wait) || wait <= 0f || float.IsNaN(remaining) || recast <= 0f || wait > remaining
			|| MathF.Floor((remaining - wait) / recast) < MathF.Floor(remaining / recast))
		{
			// Released (or never held): Aim goes now, and a later hold in this fight writes its own line.
			_aimHoldWritten = false;
			return false;
		}

		if (!_aimHoldWritten)
		{
			_aimHoldWritten = true;
			_aimHoldPull = DataCenter.LastPullSeconds;
			_aimHoldAt = DataCenter.CombatTimeRaw;
			_aimHoldWait = wait;
			DefenseTrace.Line($"phantom aim: held for the burst in {wait:F1} s | fight {remaining:F0} s left | cadence {AimClock.Cadence:F0} s"
				+ $" | {DataCenter.Job}");
		}

		return true;
	}
}
