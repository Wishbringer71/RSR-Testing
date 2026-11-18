namespace RotationSolver.Engine.Actions;

public partial struct ActionTargetInfo
{
	/// <summary>
	/// A figure a rule aims one use of an action at, outside the lists of targets: set around a single <c>CanUse</c> call in
	/// try/finally, 0 otherwise; game thread. The figure is checked like a commanded target (range, sight, whether the game
	/// allows the action on it), but neither taken from the list of targets nor refused for an untouchable in its area -
	/// the rule answers for what its area may reach. Concept 23, "Tiefe Gewölbe: Gegnerkontrolle" (A837): a sleep renewed
	/// on a sleeper, which the lists hold back from every attack.
	/// </summary>
	internal static ulong RuleTargetId { get; set; }
}
