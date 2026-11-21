namespace RotationSolver.Engine.Actions;

public partial struct ActionTargetInfo
{
	/// <summary>
	/// Concept 05, "Arenagrenzen ohne BossMod-Modul" (A934): a landing point RSR computes itself that vnavmesh says has no
	/// floor under it - an abyss or a hole in the arena - refuses the move, with BossModReborn's module or without it. Floor
	/// lower than the move is long counts as none. Null when there is floor or nothing can say.
	/// </summary>
	private static (bool Safe, string Reason, bool Measured)? WithoutFloor(Vector3 landing, float reach)
		=> DataCenter.FloorAt(landing, reach) == false ? (false, "it lands where vnavmesh has no floor", true) : null;

	// The reason last written per action: each new refusal once in the trace, not every frame - also while several
	// movement actions are refused in the same frame (code review, A934).
	private static readonly Dictionary<string, string> _tracedRefusals = [];

	private static void TraceRefusal(string action, string why)
	{
		if (_tracedRefusals.TryGetValue(action, out var last) && last == why)
		{
			return;
		}

		_tracedRefusals[action] = why;
		DefenseTrace.Line($"movement check: {action} held - {why}");
	}

	/// <summary>Another area: a refusal there is new again.</summary>
	internal static void AreaChanged() => _tracedRefusals.Clear();
}
