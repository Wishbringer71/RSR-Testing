using Dalamud.Interface.Colors;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class DancerRotation
{
	// The statuses that make a member a poor partner - the same set the partner choice skips
	// (ActionTargetInfo.FindDancePartner): Weakness and Brink of Death after a raise, and Damage Down.
	private static bool IsHamperedPartner(IBattleChara member)
		=> member.HasStatus(false, StatusID.DamageDown_2911, StatusID.DamageDown, StatusID.Weakness, StatusID.BrinkOfDeath);

	// Also not another dancer, nor another dancer's partner - the first choice skips both
	// (ActionTargetInfo.FindDancePartner). Counted as available, with a second dancer in the party a
	// partner of the other ranked above ours ended our partnership every 30 s, and Closed Position then
	// picked the same partner again (review of A186). And not out of Closed Position's range, for the
	// same reason: the choice skips them too (A223).
	private static bool IsAvailablePartner(IBattleChara member, float range)
		=> member != Player && !member.IsDead && !IsHamperedPartner(member) && !member.IsConditionCannotTarget()
			&& member.DistanceToPlayer() <= range
			&& !member.HasStatus(false, StatusID.DancePartner, StatusID.ClosedPosition);

	// A member's place in the dance partner priority: the index of the first job of theirs in the list.
	private static int DancePartnerRank(IBattleChara member)
	{
		var priority = OtherConfiguration.DancePartnerPriority;
		for (var i = 0; i < priority.Count; i++)
		{
			if (member.IsJobs(priority[i]))
			{
				return i;
			}
		}

		return int.MaxValue;
	}

	// The partner is looked up among everyone targetable, not only the party list: a Duty Support
	// partner is not in PartyMembers unless NPCs are counted as party members, and reading it from
	// there took a living partner for a lost one - Ending, Closed Position on the same NPC, and again
	// once the recast allowed (review of A186).
	private static IBattleChara? OwnDancePartner()
	{
		foreach (var member in DataCenter.AllTargets)
		{
			if (member != null && member.HasStatus(true, StatusID.DancePartner))
			{
				return member;
			}
		}

		return null;
	}

	/// <summary>
	/// Whether the current dance partnership should be ended so Closed Position can pick again. The
	/// owner's rule (concept 15, V5): while the partner is dead the partnership is lifted; while they
	/// carry the weakness of a raise, whoever would deal the most damage is chosen again; and once it
	/// has run out, the choice is made again.
	/// </summary>
	/// <param name="namedPartner">The partner named in the rotation's settings. A partner by that name is
	/// only replaced for death or weakness, never for a higher place in the priority list.</param>
	/// <remarks>
	/// "Who deals the most damage" is the dance partner priority list, the same order the first choice
	/// reads. Ending for a replacement waits until Closed Position is ready again (30 s recast): ended
	/// earlier, the buffs would go to nobody until then. Death ends it at once - the buffs are lost on a
	/// dead partner anyway. Never while dancing, so the finish still reaches the partner.
	/// </remarks>
	protected bool DancePartnerNeedsChange(string namedPartner)
	{
		if (!HasClosedPosition || IsDancing || !EndingPvE.EnoughLevel)
		{
			return false;
		}

		var partner = OwnDancePartner();
		if (partner == null || partner.IsDead)
		{
			return true;
		}

		if (ClosedPositionPvE.Cooldown.IsCoolingDown)
		{
			return false;
		}

		var hampered = IsHamperedPartner(partner);
		var isNamed = namedPartner.Length > 0 && partner.Name.ToString() == namedPartner;
		var partnerRank = DancePartnerRank(partner);
		foreach (var member in PartyMembers)
		{
			if (member == partner || !IsAvailablePartner(member, ClosedPositionPvE.TargetInfo.Range))
			{
				continue;
			}

			if (hampered || (!isNamed && DancePartnerRank(member) < partnerRank))
			{
				return true;
			}
		}

		return false;
	}
}
