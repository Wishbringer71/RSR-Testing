namespace RotationSolver.Basic.Data;

/// <summary>
/// Represents the type of attack.
/// </summary>
public enum AttackType : byte
{
	/// <summary>
	/// Unknown attack type.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Slashing attack type.
	/// </summary>
	Slashing = 1,

	/// <summary>
	/// Piercing attack type.
	/// </summary>
	Piercing = 2,

	/// <summary>
	/// Blunt attack type.
	/// </summary>
	Blunt = 3,

	/// <summary>
	/// Shot attack type - physical, as the game's AttackType sheet lists it between Blunt and Magic.
	/// </summary>
	Shot = 4,

	/// <summary>
	/// Magic attack type.
	/// </summary>
	Magic = 5,

	/// <summary>
	/// Darkness attack type.
	/// </summary>
	Darkness = 6,

	/// <summary>
	/// Physical attack type.
	/// </summary>
	Physical = 7,

	/// <summary>
	/// Limit Break attack type.
	/// </summary>
	LimitBreak = 8,
}