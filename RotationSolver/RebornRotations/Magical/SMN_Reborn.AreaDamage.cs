using System.ComponentModel;

namespace RotationSolver.RebornRotations.Magical;

public sealed partial class SMN_Reborn : SummonerRotation
{
	[RotationDesc(ActionID.RadiantAegisPvE, ActionID.AddlePvE)]
	protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
	{
		// Radiant Aegis shields the summoner alone. Whether the hit that opened this path reaches him is
		// asked centrally for every such action (BaseAction.CanUse, A233), and not under the player's own
		// Defense command (concept 13).
		if (!IsLastAction(false, RadiantAegisPvE) && RadiantAegisPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		if (TryAddleBeforeDamage(out act) || AddlePvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseAreaAbility(nextGCD, out act);
	}

	[RotationDesc(ActionID.RadiantAegisPvE, ActionID.AddlePvE)]
	protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
	{
		if (!IsLastAction(false, RadiantAegisPvE) && RadiantAegisPvE.CanUse(out act, usedUp: true))
		{
			return true;
		}

		if (TryAddleBeforeDamage(out act) || AddlePvE.CanUse(out act))
		{
			return true;
		}

		return base.DefenseSingleAbility(nextGCD, out act);
	}

	// Addle mitigates any damage the enemy deals, so the generic BMRDamageIn is the right signal.
	private bool TryAddleBeforeDamage(out IAction? act)
	{
		if (ShouldSustainMitigationDebuff(StatusID.Addle))
		{
			return AddlePvE.CanUse(out act, skipStatusProvideCheck: true);
		}
		act = null;
		return false;
	}

	// BMRRaidwideIn is already the earliest of BMR's timeline/hints/generic raidwide predictions,
	// so unlike the raw BMRDamageIn/BMRDamageType pair this can't fire on a tankbuster meant for someone else.
	// BossModReborn's "raidwide" also covers towers, baits and spreads on other players; the shield
	// protects only the Summoner, so a prediction that names whom it hits and leaves him out is passed
	// over (A273). The horizon is the shield's own duration, as its effect text states it: a shield cast
	// further ahead than that is gone before the hit.
	private bool RadiantAegisAheadOfRaidwide =>
		InCombat && !IsLastAction(false, RadiantAegisPvE)
		&& (DataCenter.BMRRaidwideHitsPlayer ?? true)
		&& BMRShouldRefreshBefore(BMRRaidwideIn, DefensiveValues.DurationOf((uint)ActionID.RadiantAegisPvE),
			true, null, StatusID.RadiantAegis);

	// Radiant Aegis "can only be executed while Carbuncle is summoned" (effect text), and a demi
	// replaces Carbuncle for its 15 s. A shield that is due and not out before the summon is gone for
	// the whole phase, so it cannot be pushed into the burst behind Searing Light the way a weave
	// usually can. Pointed out by the owner as something the slot analysis had missed; the effect text
	// is the evidence.
	//
	// Due means the two ways the rotation asks for it anyway, read the way those branches read them:
	// a raidwide BMR announces (GeneralAbility, above), or the defence flag standing (DefenseArea- and
	// DefenseSingleAbility, which the dispatch reaches under the area flag) with no shield of ours up.
	// Castable means level, the option, Carbuncle and a charge - so a disabled, spent or impossible
	// shield never holds a summon back. Without a BMR module the first way is silent, and only a cast
	// already on screen raises the flag; that is the limit of the reactive path, not covered here.
	private bool RadiantAegisDueBeforeDemi =>
		RadiantAegisPvE.EnoughLevel
		&& RadiantAegisPvE.IsEnabled
		&& DataCenter.HasPet()
		&& RadiantAegisPvE.Cooldown.CurrentCharges > 0
		&& (RadiantAegisAheadOfRaidwide
			|| (MergedStatus.HasFlag(AutoStatus.DefenseArea) && AreaHitOnMe
				&& !IsLastAction(false, RadiantAegisPvE)
				&& !StatusHelper.PlayerHasStatus(true, StatusID.RadiantAegis)));

	public override bool CanHealSingleSpell => base.CanHealSingleSpell && GcdHealsAllowed(GCDHeal);
}
