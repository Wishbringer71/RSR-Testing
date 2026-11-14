using System.ComponentModel;

namespace RotationSolver.RebornRotations.Tank;

public sealed partial class WAR_Reborn : WarriorRotation
{
	/// <summary>
	/// Whether Shake It Off waits in the single-target heal: a party tool spent for one member below the
	/// heal threshold (concept 09, "Krieger: die übrige Abwehr", rule 6). It waits while it would take Damnation or
	/// Bloodwhetting off the warrior - up to 40 % mitigation for a 2 % larger barrier - and while BossModReborn
	/// announces a raidwide after its barrier would have run out and before it is back. The universal hold
	/// yields when any member is in the critical class (A245).
	/// </summary>
	private bool HoldShakeItOffInHeal()
		=> ShakeItOffTakesNeeded()
			|| HoldAreaDefense(
				StatusHelper.PlayerHasStatus(true, ShakeItOffStrips)
					|| RaidwideAfterEffectBeforeRecast(ShakeItOffPvE, DefensiveValues.DurationOf((uint)ActionID.ShakeItOffPvE)),
				"Warrior: Shake It Off kept from the single heal");

	/// <summary>
	/// Whether Shake It Off would dispel what the tankbuster plan cannot do without - Damnation, Vengeance,
	/// Bloodwhetting, Raw Intuition or Thrill of Battle, standing or pressed and still on its way (concept 09,
	/// K1). It then waits even while the party is in danger: without it the measured tankbuster kills the
	/// warrior by the figures. It goes when the measured raidwide costs the group more than that one tank -
	/// a healer, or more than one member, by the figures: the owner's measure, the best result for the group.
	/// </summary>
	private bool ShakeItOffTakesNeeded()
		=> (TankbusterPlanNeeds(DamnationPvE) || TankbusterPlanNeeds(VengeancePvE) || TankbusterPlanNeeds(BloodwhettingPvE)
				|| TankbusterPlanNeeds(RawIntuitionPvE) || TankbusterPlanNeeds(ThrillOfBattlePvE))
			&& !AnnouncedAreaHitCostsMoreThanATank;

	/// <summary>
	/// Whether Shake It Off waits at a raidwide: only while Damnation (Vengeance) runs for a tankbuster on the
	/// warrior that lands before it ends - dispelled now, the tankbuster would meet him without it. Otherwise
	/// the raidwide gets it, Damnation or not: the party gains the barrier, he loses the rest of his own
	/// mitigation. The universal hold yields when the announced, measured raidwide would put a member into the
	/// critical class (A245).
	/// </summary>
	private bool HoldShakeItOffForTankbuster()
	{
		if (ShakeItOffTakesNeeded())
		{
			return true;
		}

		var bigLeft = StatusHelper.PlayerStatusTime(true, StatusID.Damnation, StatusID.Vengeance);
		return HoldAreaDefense(bigLeft > 0f && TankbusterOnMeWithin(bigLeft),
			"Warrior: Shake It Off kept for the tankbuster under Damnation");
	}

	/// <summary>
	/// Whether the warrior needs Bloodwhetting himself, which Nascent Flash on someone else would put on
	/// its shared cooldown (concept 09, "Krieger: Urimpuls für sich oder Urflackern für
	/// andere"): he is about to die, or a tankbuster on him lands before the cooldown is back and the
	/// single-target defence would cast Bloodwhetting for it.
	/// </summary>
	/// <remarks>
	/// The healing is not what he keeps - Nascent Flash heals him with every weaponskill as Bloodwhetting
	/// does (effect text) - but the damage reduction and the barrier. How hard the tankbuster hits is not
	/// known (BossModReborn gives no amount, nothing measures it), so an announced one on him counts as
	/// possibly lethal, unless an invulnerability covers him while it is cast. Beyond the window in which
	/// BossModReborn names who is hit, it is taken to be his while his target has him targeted (an
	/// inference: some hit the second in enmity).
	/// </remarks>
	private bool NeedsBloodwhettingHimself(out bool aboutToDie)
	{
		aboutToDie = false;
		var player = Player;
		if (player == null)
		{
			return false;
		}

		aboutToDie = player.IsInCriticalClass();
		if (aboutToDie)
		{
			return true;
		}

		// A measured tankbuster answers his first question - is it lethal - with figures: the plan needs
		// Bloodwhetting when no surviving bundle does without it (concept 09, K2). The announcement rule
		// below stays for the tankbusters the table has not measured.
		if (TankbusterPlanActive)
		{
			return TankbusterPlanNeeds(BloodwhettingPvE) || TankbusterPlanNeeds(RawIntuitionPvE);
		}

		// Raw Intuition's own check: Bloodwhetting goes out only while his target has him targeted. An
		// off-tank marked for a tankbuster does not get it from the defence path, so holding for it
		// would keep Nascent Flash from the member for nothing.
		if (!BloodwhettingForDefense || !ObjectHelper.PlayerIsTargetOnSelf())
		{
			return false;
		}

		if (DataCenter.IsHostileCastingTankBusterAtMe)
		{
			return player.CanBeHealed();
		}

		if (!BMRTankbusterWithin(NascentFlashPvE.Cooldown.RecastTimeOneChargeRaw))
		{
			return false;
		}

		// BossModReborn names who is hit only inside its mitigation window; further ahead, the one who has
		// the enemy's attention is taken to get it.
		return DataCenter.BMRTankbusterHitsPlayer ?? true;
	}

	/// <summary>
	/// The triage when the warrior and a member both need it (concept 09): the member must be about to
	/// die, and then a healer wins, another tank wins unless the warrior is about to die too, and a
	/// damage dealer - the one most easily given up - does not.
	/// </summary>
	private static bool WinsTriageOverWarrior(IBattleChara member, bool warriorAboutToDie)
	{
		if (!member.IsInCriticalClass())
		{
			return false;
		}

		if (member.IsJobCategory(JobRole.Healer))
		{
			return true;
		}

		// Against a measured need the plan's triage decides: another tank wins only while the warrior's
		// invulnerability can take over (the owner's answer to D3, concept 09).
		if (TankbusterPlanActive)
		{
			return HelpWinsTriage(member);
		}

		return member.IsJobCategory(JobRole.Tank) && !warriorAboutToDie;
	}

	// Nascent Flash on another member a tankbuster is aimed at: while he needs Bloodwhetting himself, only on a
	// member who wins the triage, as for the heal path.
	private bool NascentFlashForTankbuster(out IAction? act)
	{
		var held = false;
		var warriorAboutToDie = false;
		if (HoldNascentFlashForOwnNeed)
		{
			held = NeedsBloodwhettingHimself(out warriorAboutToDie);
		}

		return HelpTankbusterTarget(NascentFlashPvE, out act,
			alsoTakes: t => !held || WinsTriageOverWarrior(t, warriorAboutToDie));
	}

	protected override bool HasOwnArmsLengthPullRule => true;

	// Whether the single-target defence casts Bloodwhetting (Raw Intuition before level 82): the one
	// condition, read by that path and by the Nascent Flash hold, so the two cannot disagree (A226).
	private bool BloodwhettingForDefense => SoloIntuition || NumberOfHostilesInRange > 2;
}
