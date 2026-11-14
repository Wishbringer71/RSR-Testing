using System.ComponentModel;

namespace RotationSolver.RebornRotations.Melee;

public sealed partial class SAM_Reborn : SamuraiRotation
{
	[RotationDesc(ActionID.HissatsuYatenPvE)]
	protected override bool MoveBackAbility(IAction nextGCD, out IAction? act)
	{
		if (HissatsuYatenPvE.CanUse(out act))
		{
			return true;
		}

		return base.MoveBackAbility(nextGCD, out act);
	}

	[RotationDesc]
	protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (SecondWindPvE.CanUse(out act))
		{
			return true;
		}

		if (BloodbathPvE.CanUse(out act))
		{
			return true;
		}

		return base.HealSingleAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.FeintPvE, ActionID.TengentsuPvE, ActionID.ThirdEyePvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		// EnoughWeaveTime is a clip-safety check, not a preference gate, so it is kept here.
		if (EnoughWeaveTime
			&& ShouldSustainMitigationDebuff(StatusID.Feint)
			&& FeintPvE.CanUse(out act, skipStatusProvideCheck: true))
		{
			return true;
		}

		// A tankbuster cast at us is the one hit Third Eye / Tengentsu exist for. Zanshin's weave yields as above, here
		// when the samurai or a tank is in danger.
		if (!HoldSingleDefense(HasZanshinReady, ZanshinHoldRule))
		{
			if (FeintPvE.CanUse(out act))
			{
				return true;
			}
			if (TengentsuPvE.CanUse(out act))
			{
				return true;
			}
			if (ThirdEyePvE.CanUse(out act))
			{
				return true;
			}
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	// A pause in the fight (no hostile within 25 yalms) is a GCD with nothing to strike. Meditate fills
	// it: "Gradually increases your Kenki Gauge", and only in battle. It ends on moving, and its base
	// setting already refuses it while moving, so it is asked only when the samurai stands.
	private bool UseMeditateInPause(out IAction? act)
	{
		act = null;
		return InCombatPause && MeditatePvE.CanUse(out act);
	}

	// Whether the pack calls for the AoE path: as many enemies within the AoE combo's radius as its AoE count asks, the set
	// and threshold its own targeting uses - one setting moves both (A391).
	private bool AoeComboReaches
	{
		get
		{
			var combo = FugaMasteryTrait.EnoughLevel ? FukoPvE : FugaPvE;
			return NumberOfHostilesInRangeOf(combo.Info.EffectRange) >= combo.Config.AoeCount;
		}
	}
}
