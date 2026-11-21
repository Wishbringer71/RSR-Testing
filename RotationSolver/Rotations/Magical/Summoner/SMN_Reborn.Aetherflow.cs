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

	/// <summary>
	/// Concept 12, "Sol-Bahamut läuft dem Buff davon" (A957): the demi summons' recast shortens with spell speed, so below a
	/// 2.50 s GCD Solar Bahamut comes before Searing Light and the buff turns ready outside the phase. A lone Summoner with
	/// burst on then fires it on cooldown (The Balance: "Searing Light should be used on cooldown") instead of holding it up
	/// to a whole cycle for the next Solar. Not while the burst summon comes by the next GCD - that wait is the burst rule's
	/// own - and not with another Summoner in the party, where the phase book decides.
	/// </summary>
	/// <param name="bigSummonReady">Whether the burst summon is ready by the next GCD.</param>
	private bool SearingLightOnCooldownAlone(bool bigSummonReady)
	{
		// Out of combat its own check holds it (ModifySearingLightPvE: InCombat).
		return IsBurst && !bigSummonReady && !AnotherSummonerInParty && SearingLightPvE.EnoughLevel;
	}
}
