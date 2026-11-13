using ECommons.DalamudServices;
using ECommons.ExcelServices;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// The owner's proposal of 04.10.2026 (concept 09, "Tankwechsel nach einem Tankbuster"): after a
	/// tankbuster that leaves the player in danger of dying to the next one, and with his
	/// invulnerability not available, Shirk goes to the tank <see cref="TankSwapWatch"/> names - the
	/// one that will then hold the enemy.
	/// </summary>
	/// <remarks>
	/// The invulnerability counts as available only when RSR would use it for any next hit
	/// (<see cref="InvulnerabilityUsable"/>: at its threshold) and it is off cooldown. The tankbuster plan
	/// draws it ahead of a measured cast as well, but not for every next hit, so it does not count here.
	/// </remarks>
	private bool TankSwapAbility(out IAction? act)
	{
		act = null;
		var danger = TankSwapWatch.Danger;
		if (danger.Length == 0)
		{
			return false;
		}

		if (InvulnerabilityUsable(out var invulnerability) && invulnerability!.Cooldown.HasOneCharge)
		{
			TankSwapWatch.TraceHeld($"{invulnerability.Name} is ready");
			return false;
		}

		var receiver = TankSwapWatch.Target;
		if (receiver == null)
		{
			TankSwapWatch.TraceHeld(TankSwapWatch.Detail);
			return false;
		}

		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = TargetType.TankSwap;
		try
		{
			if (!ShirkPvE.CanUse(out act))
			{
				TankSwapWatch.TraceHeld(ShirkPvE.Cooldown.HasOneCharge
					? $"Shirk cannot reach {receiver.Name.TextValue}"
					: "Shirk is on cooldown");
				act = null;
				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		TankSwapWatch.PlanSwapShirk(TankSwapWatch.DangerSource, receiver, TankSwapWatch.Ratio);
		DefenseTrace.Decision($"tank swap ({danger}; {TankSwapWatch.Detail})", act);
		return true;
	}

	/// <summary>
	/// Concept 09, "Ein gemeldeter Treffer, der tötet" (A617): the damage the game has reported on the tank and not yet
	/// taken (<see cref="ReportedDamage"/>) reaches his HP and barrier - his invulnerability goes now, in the moment between
	/// the report and the damage, ahead of every other emergency action. Heals on their way are not counted: a heal that
	/// lands after the hit saves nobody.
	/// </summary>
	private bool InvulnerabilityAgainstReportedDeath(out IAction? act)
	{
		act = null;
		if (!Service.Config.InvulnerabilityAgainstReportedDeath || DataCenter.Role != JobRole.Tank || Invulnerability is not { } invulnerability
			|| Player is not { } player || player.IsDead)
		{
			return false;
		}

		var reported = ReportedDamage.Pending;
		var barrier = player.GetObjectShield();
		if (reported == 0 || reported < player.CurrentHp + barrier || !invulnerability.CanUse(out act))
		{
			return false;
		}

		DefenseTrace.Decision($"reported damage {reported} reaches your {player.CurrentHp} HP and {barrier} barrier before it lands", act);
		return true;
	}

	/// <summary>
	/// Whether RSR would use the tank's invulnerability for the next hit whatever it is: it exists for the
	/// job, is learned at the current level, enabled, and its threshold (HealthForDyingTanks) is above zero.
	/// The tankbuster plan does not count here: it draws the invulnerability only for a measured cast that
	/// nothing less survives, not for a marker, a BossModReborn prediction or an unmeasured action
	/// (review of A261).
	/// </summary>
	private bool InvulnerabilityUsable(out IBaseAction? invulnerability)
	{
		invulnerability = Invulnerability;
		return invulnerability != null && invulnerability.Config.IsEnabled && invulnerability.EnoughLevel
			&& HealthForDyingTanks > 0f;
	}

	/// <summary>
	/// The other half of the swap (the owner's rule of 04.10.2026): the enemy the swap handed to the
	/// co-tank is provoked back only after the co-tank has taken its next tankbuster (his question,
	/// accepted), once the debuff and the critical state are over - and, his precision, once the
	/// invulnerability is ready again. Where RSR would not use an invulnerability at
	/// all (not learned at this level, disabled, threshold zero) there is nothing to wait for.
	/// </summary>
	private bool TankSwapBackAbility(out IAction? act)
	{
		act = null;
		var source = TankSwapWatch.ReclaimSource;
		if (source == null)
		{
			return false;
		}

		if (InvulnerabilityUsable(out var invulnerability) && !invulnerability!.Cooldown.HasOneCharge)
		{
			TankSwapWatch.TraceReclaimHeld($"{invulnerability.Name} is not ready yet");
			return false;
		}

		var previous = IBaseAction.TargetOverride;
		IBaseAction.TargetOverride = TargetType.TankSwap;
		try
		{
			if (!ProvokePvE.CanUse(out act))
			{
				TankSwapWatch.TraceReclaimHeld(ProvokePvE.Cooldown.HasOneCharge
					? $"Provoke cannot reach {source.Name.TextValue}"
					: "Provoke is on cooldown");
				act = null;
				return false;
			}
		}
		finally
		{
			IBaseAction.TargetOverride = previous;
		}

		DefenseTrace.Decision($"tank swap back (the co-tank took the next tankbuster, your debuff and danger are over) on {source.Name.TextValue}", act);
		return true;
	}
}
