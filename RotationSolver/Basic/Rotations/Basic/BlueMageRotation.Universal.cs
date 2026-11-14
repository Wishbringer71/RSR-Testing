using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class BlueMageRotation
{
	/// <summary>
	/// The filler the value rules weigh against: Sonic Boom, else Water Cannon, whichever is in the active set; null with
	/// neither.
	/// </summary>
	protected IBaseAction? ValueFiller
		=> SonicBoomPvE.Info.IsOnSlot ? SonicBoomPvE : WaterCannonPvE.Info.IsOnSlot ? WaterCannonPvE : null;

	/// <summary>
	/// The time a spell takes of the player's: its cast, or the GCD where that is longer.
	/// </summary>
	protected static float SpellTime(IBaseAction action)
		=> Math.Max(action.Info.CastTime, WeaponTotal);

	/// <summary>
	/// The filler's potency as its effect text states it, and the time it takes; false without a filler in the set or a
	/// figure in its text.
	/// </summary>
	protected bool TryFiller(out float potency, out float time)
	{
		time = 0f;
		potency = 0f;
		var filler = ValueFiller;
		if (filler == null || !ActionTooltip.TryPotencyOn(filler.ID, 1, out potency))
		{
			return false;
		}

		time = SpellTime(filler);
		return potency > 0f && time > 0f;
	}

	/// <summary>
	/// The damage the filler would deal in <paramref name="seconds"/>, by the damage per potency measured on the player's own
	/// hits (<see cref="OwnHitWatch.DamagePerPotency"/>); NaN while nothing is measured.
	/// </summary>
	protected float FillerDamageOver(float seconds)
	{
		var perPotency = OwnHitWatch.DamagePerPotency;
		return float.IsNaN(perPotency) || !TryFiller(out var potency, out var time)
			? float.NaN
			: perPotency * potency / time * seconds;
	}
}
