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
}
