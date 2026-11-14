using ECommons.GameFunctions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Updaters;

internal static partial class StateUpdater
{
	// Figures only healing defeats (concept 07, A516): a reason to heal of their own, apart from the party's health and
	// from the settings that let a non-healer heal the party - a healer whenever one stands, anyone else when the option
	// and the deadline call for it (HealToDefeat.CallsForPlayer). "Auto heal" switches it off as it does every heal.
	private static AutoStatus StatusFromHealToDefeat()
	{
		return Service.Config.AutoHeal && HealToDefeat.CallsForPlayer
			? AutoStatus.HealSingleAbility | AutoStatus.HealSingleSpell
			: AutoStatus.None;
	}

	private static string? _antiKnockbackWhy;

	/// <summary>
	/// Whether <paramref name="member"/> calls for a single-target heal spell, by the decision that sets
	/// <see cref="AutoStatus.HealSingleSpell"/> for him - for a reader that weighs one cast against its target.
	/// </summary>
	internal static bool NeedsSingleHealSpell(IBattleChara member)
	{
		return ShouldHealSingle(member, Service.Config.HealthSingleSpell,
			Service.Config.HealthSingleSpellHot, false);
	}
}
