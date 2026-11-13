using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

public sealed partial class GNB_Reborn : GunbreakerRotation
{
	protected override bool HasOwnArmsLengthPullRule => true;

	// The enemies around the gunbreaker, where Fated Circle and the Demon Slice combo strike: counted by Demon Slice,
	// which needs no cartridge and is learned at level 10, so Sonic Break is weighed with an empty gauge too.
	private int EnemiesAroundMe()
	{
		return DemonSlicePvE.CanUse(out _, skipComboCheck: true, skipAoeCheck: true) ? DemonSlicePvE.Target.AffectedTargets.Length : 0;
	}

	// A cartridge on Fated Circle where its own count of enemies is met (2 by default, the reference's too), otherwise on
	// Burst Strike: the same spend, one hit on everybody around instead of a bigger one on the target.
	private bool CartridgeSpender(out IAction? act)
	{
		return FatedCirclePvE.CanUse(out act) || BurstStrikePvE.CanUse(out act);
	}

	// Sonic Break by the reference's counts: up to 5 enemies; at 4 and 5 only where the target it was just chosen for lives
	// its whole stated duration by its measured time to kill, at 3 half of it; while that time is unknown, as before. Its
	// damage over time sits on one enemy, so in a large pack the GCD gives the others nothing. Asked after CanUse, which
	// picks that target.
	private bool SonicBreakWorthIn(int pack)
	{
		if (pack < 3)
		{
			return true;
		}

		if (pack > 5)
		{
			return false;
		}

		var ttk = SonicBreakPvE.Target.Target?.GetCorrectedTTK() ?? float.NaN;
		if (float.IsNaN(ttk) || !ActionTooltip.TryStatedDuration(SonicBreakPvE.ID, out var duration))
		{
			return true;
		}

		return ttk >= (pack == 3 ? duration / 2f : duration);
	}
}
