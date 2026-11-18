namespace RotationSolver.Rotations.Duties;

/// <summary>
/// Concept 03, "Der Burst des Jobs als Uhr, und ein Verstärker, der ihm folgt: Phantom Aim" (A885): Phantom Aim waits
/// for the job's next burst when waiting costs no use over the rest of the fight.
/// </summary>
public sealed partial class PhantomDefault
{
	[RotationConfig(CombatType.PvE, Name = "Hold Phantom Aim for your burst while that costs no use in the fight",
		Tooltip = "In a fight: Phantom Aim waits for your job's next burst when the time your target has left gives the same number of "
			+ "Aims either way, so each lands in a burst and none is lost. Your burst is where your rotation asks for the "
			+ "tincture; its cadence is learned in your fights. Off: Aim goes whenever it is ready.")]
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

	// A890 (concept 03, "Die Uhr für die Phantom-Angriffe"): the job rotations whose IsBursting is CustomRotation's own,
	// which always answers true - upstream's "Save Phantom Attacks" never held for them.
	private static readonly Dictionary<Type, bool> _noBurstWindow = [];
	private IBaseAction[]? _burstAttacks;
	private bool _attackHoldWritten;

	// The abilities of upstream's region "Burst abilities".
	private IBaseAction[] BurstAttacks => _burstAttacks ??= [DrainTouchPvE, FlameScrollPvE, FumaShurikenPvE, LancePvE,
		LightningScrollPvE, MesmerizePvE, OccultCounterPvE, OccultLibraPvE, PhantomKickPvE, PilferWeaponPvE];

	private static bool JobHasNoBurstWindow()
	{
		if (DataCenter.CurrentRotation is not { } rotation)
		{
			return false;
		}

		var type = rotation.GetType();
		if (!_noBurstWindow.TryGetValue(type, out var none))
		{
			none = type.GetMethod(nameof(CustomRotation.IsBursting), Type.EmptyTypes)?.DeclaringType == typeof(CustomRotation);
			_noBurstWindow[type] = none;
		}

		return none;
	}

	/// <summary>
	/// Under "Save Phantom Attacks", for a job without a burst window of its own: the ready phantom attacks wait for the
	/// burst the clock expects (a gap as long as a tincture lasts), when none of them loses a use over the time the
	/// target has left - ⌊R/r⌋ = ⌊(R − w)/r⌋ for each one's recast r.
	/// </summary>
	private bool PhantomAttacksWaitForBurst()
	{
		if (!SaveForBurstWindow || !InCombat || BurstClock.InBurstNow || !JobHasNoBurstWindow())
		{
			_attackHoldWritten = false;
			return false;
		}

		var wait = CustomRotation.TinctureClock.SecondsToNext;
		var remaining = HostileTarget?.GetCorrectedTTK() ?? float.NaN;
		if (float.IsNaN(wait) || wait <= 0f || float.IsNaN(remaining) || wait > remaining)
		{
			_attackHoldWritten = false;
			return false;
		}

		var ready = 0;
		foreach (var attack in BurstAttacks)
		{
			if (!attack.CanUse(out _))
			{
				continue;
			}

			var recast = attack.Cooldown.RecastTimeOneChargeRaw;
			if (recast <= 0f || MathF.Floor((remaining - wait) / recast) < MathF.Floor(remaining / recast))
			{
				_attackHoldWritten = false;
				return false;
			}

			ready++;
		}

		if (ready == 0)
		{
			return false;
		}

		if (!_attackHoldWritten)
		{
			_attackHoldWritten = true;
			DefenseTrace.Line($"phantom attacks: held for the burst in {wait:F1} s | {ready} ready | fight {remaining:F0} s left | {DataCenter.Job}");
		}

		return true;
	}
}
