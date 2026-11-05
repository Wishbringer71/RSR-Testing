namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The kinds of attack a status turns back on its attackers, and the kinds a job attacks with (concept 03, "Gegner mit
/// Stacheln", A588).
/// </summary>
[Flags]
internal enum AttackKinds : byte
{
	None = 0,

	/// <summary>Physical attacks: the disciples of war, the beastmaster among them.</summary>
	Physical = 1,

	/// <summary>Magic attacks: the disciples of magic; "-aspected" reflections count here.</summary>
	Magic = Physical << 1,

	/// <summary>Attacks from range: the ranged and healer roles.</summary>
	Ranged = Magic << 1,

	/// <summary>Every attack.</summary>
	Any = Physical | Magic | Ranged,
}
