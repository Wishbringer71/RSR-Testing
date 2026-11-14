using ECommons.GameFunctions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Updaters;

internal static partial class StateUpdater
{
	// Owner's rule, second stage: "die aktuelle hp liegt unter dem schadenswert. dann wäre aber eine
	// heilung sinnvoll bis max maxhp."
	//
	// Every threshold below reads the health a member HAS. None of them reads the health he will
	// have once the cast already on screen lands, so the hit counts for nothing until it has landed:
	// a member at 55% in front of a 45% raidwide is left at ten percent before any threshold sees it.
	// The size of that cast is measured (concept 13) and decides whether to MITIGATE; this is the
	// other half of the same figure.
	//
	// Asked where the hit would leave somebody in danger (DataCenter.AnnouncedHitCallsForHealFirst),
	// not where it would cross a heal threshold: that question fired on a full party before every
	// raidwide big enough, and a heal landing on full health heals nobody (A292). Where nothing is
	// announced or its size has not been measured, the answer is false and the flags behave exactly
	// as before.
	private static bool AnnouncedAreaCastIsUpForHealing()
	{
		if (!Service.Config.HealAheadOfAnnouncedHit)
		{
			return false;
		}

		// Asked here rather than read from whatever ran before: walking the casting enemies is what
		// establishes the size in the first place. Reading the recorded share on its own would make
		// this rule depend on the defensive branch having run earlier in the same frame - and that
		// branch is itself behind UseAoeDefense, so with area defence switched off the size would
		// never be established and this rule would silently never fire.
		//
		// Asked of the UNRATED recognitions, not of IsHostileCastingAOE. That one has the mitigation
		// verdict built into it - a cast leaving everybody above HealthAreaSpell is dropped - and
		// this rule makes its own judgement of the hit; it must not inherit the mitigation's
		// threshold and the setting behind it (CLAUDE.md, "Erkennung und Entscheidung trennen").
		//
		// Both are asked because they see different casts: the first drops anything interruptible,
		// the second takes those back up when their measured share is large. Healing comes before
		// mitigation, so it must not see less than the defence does.
		return DataCenter.IsHostileCastingAreaUnrated || DataCenter.IsHostileCastingLargeArea;
	}

	// The announced action the last "heal ahead" trace line was written for; 0 once the rule stops firing.
	private static uint _healAheadTraced;
}
