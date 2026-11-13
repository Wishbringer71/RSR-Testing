namespace RotationSolver.Basic.Rotations.Basic;

public partial class WhiteMageRotation
{
	/// <summary>
	/// Regen against an enemy's damage over time on one member; Medica III, below its level Medica II, on more than one
	/// (concept 08, "Schaden über Zeit: HoT gegen DoT", A432).
	/// </summary>
	protected override bool HotAgainstDotByJob(out IAction? act)
	{
		return TryHotAgainstDot(RegenPvE, MedicaIiiPvE.EnoughLevel ? MedicaIiiPvE : MedicaIiPvE, out act);
	}

	/// <summary>
	/// Holy or Holy III stops an enemy's cast by stunning it - and every other enemy in its radius that can still be
	/// stunned. The owner's rule of 23.10.2026: "ausserdem steht sleep nicht in konkurenz zu holy beim whm. wenn holy
	/// stunnen kann, ist das vorzuziehen. speziell bei gruppen." (concept 08, "Ein gegnerisches Wirken stoppen").
	/// </summary>
	/// <remarks>
	/// It goes where the enemy stands in the radius, can still be stunned (StunWatch: no Stun Resistance, not worn
	/// down by repeated stuns) and enough of its cast is left: Holy's own cast, the GCD still running, the time to its
	/// effect and the margin a status needs before the cast ends - learned from sleeps, the same kind of status
	/// (inference). Holy's area count does not apply here, the stop is the reason; neither do the White Mage
	/// rotation's holds for the stun stretch, the Dark Knight's barrier or a slowed pack, which weigh later or
	/// resource reasons against an attack that is coming now (his rule: the party's safety first).
	/// </remarks>
	protected override bool StopEnemyCastByJob(IBattleChara enemy, float left, float lag, out IAction? act)
	{
		act = null;
		var holy = HolyIiiPvE.EnoughLevel ? HolyIiiPvE : HolyPvE;
		if (!holy.EnoughLevel || !StunWatch.CanStillBeStunned(enemy) || enemy.DistanceToPlayer() > holy.Info.EffectRange
			|| Player is not { } player)
		{
			return false;
		}

		var needed = holy.Info.NeededStatusMargin + lag + SleepInterruptWatch.LandingMargin;
		if (left <= needed || AreaReachesNewMob(player, holy.Info.EffectRange)
			|| !holy.CanUse(out act, skipAoeCheck: true, skipTTKCheck: true))
		{
			return false;
		}

		SleepInterruptWatch.JobStop(enemy, holy, left, needed);
		return true;
	}

	// The second press: the button the game shows while the bell stands. It releases every stack left at once
	// (effect text: 200 potency per stack), so it is only ever there while the bell's own status is - and only once the
	// bell has healed at least once: pressed before, it would trade the hit's 400 per stack for 200 ahead of the hit the
	// bell was placed for (the owner's sequence, 24.10.2026: the hit, the bell's own heal, then the rest).
	static partial void ModifyLiturgyOfTheBellPvE_28509(ref ActionSetting setting)
	{
		setting.StatusNeed = [StatusID.LiturgyOfTheBell];
		setting.IsFriendly = true;
		setting.ActionCheck = () => Player != null && LiturgyMaxStacks > 0
			&& Player.StatusStack(true, StatusID.LiturgyOfTheBell) < LiturgyMaxStacks;
	}

	// How many stacks the bell starts with, from the game's status sheet; zero when the sheet does not say.
	private static byte LiturgyMaxStacks => _liturgyMaxStacks ??=
		Service.GetSheet<Lumina.Excel.Sheets.Status>().TryGetRow((uint)StatusID.LiturgyOfTheBell, out var row) ? row.MaxStacks : (byte)0;

	private static byte? _liturgyMaxStacks;

	/// <summary>
	/// The triggers a Liturgy of the Bell laid now would still collect from an enemy attack that is hitting the player
	/// and another party member in a series: what that action has been seen to reach in a row - one trigger a second,
	/// within the bell's duration - less what has landed so far. 0 when no such series runs or none is learned yet
	/// (concept 08, "Glockenspiel", A349).
	/// </summary>
	protected static int LiturgyTriggersLeft => HitSeries.TriggersLeft();

	/// <summary>Medica II, later Medica III (concept 07, "Reinigung oder Heilung", A565).</summary>
	internal override bool HasGroupHealOverTime => MedicaIiPvE.EnoughLevel || MedicaIiiPvE.EnoughLevel;
}
