using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class BlueMageRotation
{
	/// <summary>
	/// What a target does to an action's damage in the Masked Carnivale, from the briefing sheet: 0 when it is immune to
	/// the action's element or attack type, the measured <see cref="OwnHitWatch.WeaknessFactor"/> when it is weak to it,
	/// else 1; 1 outside the Carnivale (concept 22, "Schwäche und Immunität der Stufe", A647).
	/// </summary>
	protected static float CarnivaleFactor(IBaseAction action, IBattleChara? target)
	{
		if (!DataCenter.IsInMaskedCarnivale)
		{
			return 1f;
		}

		return MaskedCarnivaleHelper.ResistanceTo(target, action) switch
		{
			MobResistance.Immune => 0f,
			MobResistance.Weak => OwnHitWatch.WeaknessFactor,
			_ => 1f,
		};
	}

	// The filler's factor on the target a fixed or percentage spell would hit: Sonic Boom's or Water Cannon's; against an
	// enemy immune to it another filler goes at the plain rate (A647).
	private float FillerFactorOn(IBaseAction action)
	{
		var filler = ValueFiller;
		var factor = filler == null ? 1f : CarnivaleFactor(filler, action.Target.Target);
		return factor > 0f ? factor : 1f;
	}
}
