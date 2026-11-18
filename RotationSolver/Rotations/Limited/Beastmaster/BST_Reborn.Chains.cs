namespace RotationSolver.Rotations.Limited.Beastmaster;

/// <summary>
/// Concept 01, "Ketten: Rally und Rallying Cheer tragen die Kette weiter" (A886): in a chain, Rally or Rallying Cheer
/// refills the side whose instinctual skill then completes the next combo; outside a chain they wait while their stacks
/// can still grow.
/// </summary>
public sealed partial class BST_Reborn
{
	[RotationConfig(CombatType.PvE, Name = "Keep Rally and Rallying Cheer to carry a combo chain on",
		Tooltip = "In a fight: outside a chain, Rally and Rallying Cheer wait while their stacks can still grow. In a chain, when neither "
			+ "Trick nor an axe could follow in time, the one whose skill can then follow refills, so that skill completes "
			+ "the next combo, which deals more the later it comes in the chain. Off: both refill whenever their TP fit.")]
	public bool KeepRefillsForChains { get; set; } = true;

	/// <summary>
	/// Whether Rally or Rallying Cheer waits outside a chain: the setting is on and a further combo could still add a
	/// stack (<paramref name="stacks"/> below full). With full stacks the next combo's stack would be lost, so it fills now.
	/// </summary>
	private bool RefillWaitsForChain(int stacks)
	{
		return KeepRefillsForChains && stacks < FullInstinctStacks && ChainTimeLeft <= DataCenter.DefaultGCDTotal;
	}

	// The most stacks of Mastered or Natural Instinct the gauge holds (the gauge's three notches each).
	private const int FullInstinctStacks = 3;

	/// <summary>
	/// In a chain that stands longer than a GCD, when neither Trick nor an axe can follow before it ends: Rallying Cheer
	/// when its TP lift the familiar to what Trick needs and Trick is ready in time, else Rally when its TP lift yours to
	/// what an axe needs and the axes are ready in time.
	/// </summary>
	private bool CarryChain(out IAction? act)
	{
		act = null;
		var chain = ChainTimeLeft;
		if (!KeepRefillsForChains || chain <= DataCenter.DefaultGCDTotal)
		{
			return false;
		}

		var before = chain - DataCenter.DefaultGCDTotal;
		var trickInTime = TrickPvE.EnoughLevel && TrickPvE.Cooldown.RecastTimeRemainOneCharge < before;
		var axeInTime = AvalancheAxePvE.Cooldown.RecastTimeRemainOneCharge < before;
		if ((trickInTime && PetTPCount >= TrickTPNeed) || (axeInTime && TPCount >= AxeTPNeed))
		{
			return false;
		}

		if (trickInTime && PetTPCount + RallyingCheerTPRestore >= TrickTPNeed && RallyingCheerPvE.CanUse(out act))
		{
			TraceHorn($"bst chain: Rallying Cheer carries chain {ChainCount} on, {chain:F1} s left | familiar TP {PetTPCount} + {RallyingCheerTPRestore}");
			return true;
		}

		if (axeInTime && TPCount + RallyTPRestore >= AxeTPNeed && RallyPvE.CanUse(out act))
		{
			TraceHorn($"bst chain: Rally carries chain {ChainCount} on, {chain:F1} s left | TP {TPCount} + {RallyTPRestore}");
			return true;
		}

		return false;
	}
}
