using RotationSolver.Basic.Configuration;
using Player = ECommons.GameHelpers.Player;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Whether help from the player for another member's tankbuster is called for: the decision both the target selection
/// (<see cref="ActionTargetInfo"/>) and the check of an outward mitigation in the single defence
/// (<see cref="BaseAction"/>) read, so the two cannot disagree (concept 09, "Hilfe für das Ziel eines Tankbusters", A373).
/// </summary>
/// <remarks>
/// The facts come from <see cref="DataCenter"/> - whom a tankbuster is aimed at, whether he stands impervious past it,
/// what the measured hit leaves him -; the options and thresholds are read here.
/// </remarks>
internal static class TankbusterHelp
{
	/// <summary>
	/// A member a tankbuster is aimed at (<see cref="DataCenter.IsTankbusterAimedAt"/>) who needs help from you for it.
	/// Under "Spend only the mitigation a measured tankbuster needs" not one who takes every such hit under Hallowed
	/// Ground or Superbolide - his proposal of 04.10.2026: a tank the hit cannot touch gets nothing from a mitigation on
	/// him -, and not one whose measured hit calls for no help (<see cref="MeasuredHitCallsForNoHelp"/>).
	/// </summary>
	internal static bool IsTargetToProtect(IBattleChara t)
	{
		return DataCenter.IsTankbusterAimedAt(t)
			&& !(Service.Config.HoldMitigationUnderInvulnerability
				&& (DataCenter.ImperviousPastAimedTankbuster(t) || MeasuredHitCallsForNoHelp(t)));
	}

	/// <summary>
	/// A member a tankbuster is aimed at whom <see cref="IsTargetToProtect"/> passed over, in the single defence the player
	/// has not commanded. Not taken back by the attacked target's fallback: a BossModReborn prediction keeps the healer's
	/// single defence open beside such a tankbuster (StateUpdater, review of A260), and through that fallback the help went
	/// to the tank the boss attacks all the same - Aquaveil on a tank under Hallowed Ground (A373). Heal paths and the area
	/// defence are not help for this hit.
	/// </summary>
	internal static bool NeedsNothingFromYou(IBattleChara t)
	{
		return !IBaseAction.HealPathRunning && !IBaseAction.AreaDefenceRunning && !IBaseAction.CommandedDefenceRunning
			&& DataCenter.IsTankbusterAimedAt(t) && !IsTargetToProtect(t);
	}

	/// <summary>
	/// Whether the single defence is open for tankbusters that only reach members who need nothing from you: at least one
	/// tankbuster is aimed at a party member, every one of them at a member <see cref="NeedsNothingFromYou"/>, and no
	/// BossModReborn tankbuster is imminent - it names no target here and may be a hit on someone else. Then an outward
	/// mitigation such as Reprisal has no hit to cover (A374).
	/// </summary>
	internal static bool OnlyForMembersNeedingNothing()
	{
		if (DataCenter.BMRTankbusterImminent)
		{
			return false;
		}

		var any = false;
		var party = DataCenter.PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || !DataCenter.IsTankbusterAimedAt(member))
			{
				continue;
			}

			if (!NeedsNothingFromYou(member))
			{
				return false;
			}

			any = true;
		}

		return any;
	}

	/// <summary>
	/// Help from you for another member's measured tankbuster goes where the hit calls for it, by the size rule of the area
	/// defence (DataCenter.AreaShareCallsForMitigation): a hit at or above what a large shield absorbs, or one that leaves
	/// him below the health at which this rotation heals him by itself - mitigation where healing would otherwise be
	/// needed. Below both, a White Mage spent Divine Benison and Aquaveil on a hit of 20 % of a tank's HP (his trace of
	/// 25.10.2026, A364). Not for the player, whose own plan decides for him, not in the single defence he commands, and
	/// not for a heal (Aurora, Heart of Corundum in the heal path) (A373).
	/// </summary>
	private static bool MeasuredHitCallsForNoHelp(IBattleChara t)
	{
		if (IBaseAction.HealPathRunning || IBaseAction.AreaDefenceRunning || IBaseAction.CommandedDefenceRunning
			|| Player.Object is not { } self || t.GameObjectId == self.GameObjectId
			|| !DataCenter.AimedTankbusterLeaves(t, out var actionId, out var share, out var left))
		{
			return false;
		}

		var line = Service.Config.HealthSingleSpell;
		if (share >= DefensiveValues.LargestStatedBarrierShare || left < line)
		{
			return false;
		}

		DataCenter.NoteTankbusterHelpHeld(t, actionId, share, left, line);
		return true;
	}
}
