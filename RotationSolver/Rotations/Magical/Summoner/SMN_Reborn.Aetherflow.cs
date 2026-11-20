namespace RotationSolver.Rotations.Magical.Summoner;

public sealed partial class SMN_Reborn
{
	private static long _burstSummonReadySince;

	/// <summary>
	/// Concept 14, "Die Necrotize der ungeraden Minute warten auf das Zwei-Minuten-Fenster", "Das Warten endet" (A952):
	/// whether the burst summon has stood ready for more than a GCD without being cast - then something else holds it
	/// (movement, a target out of range, another rule), and the Aetherflow gauge no longer waits for its window. Without
	/// this end the gauge waited for good, and Energy Drain, which needs it empty, waited with it.
	/// </summary>
	/// <param name="burstSummon">Summon Solar Bahamut, below its level Summon Bahamut.</param>
	private static bool BurstSummonHeldElsewhere(IBaseAction burstSummon)
	{
		if (!burstSummon.Cooldown.HasOneCharge)
		{
			_burstSummonReadySince = 0;
			return false;
		}

		var now = Environment.TickCount64;
		if (_burstSummonReadySince == 0)
		{
			_burstSummonReadySince = now;
			return false;
		}

		return TimeSpan.FromMilliseconds(now - _burstSummonReadySince).TotalSeconds > WeaponTotal;
	}
}
