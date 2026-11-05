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

/// <summary>
/// How a hit of an <see cref="AttackType"/> row meets the target's defences: physical (slashing, piercing, blunt, shot) or
/// magic; any other row (breath, sound, limit break) meets neither. One place for the tankbuster plan, the learned
/// tankbusters, the incoming-damage checks and the Crucible.
/// </summary>
internal static class AttackTypeKind
{
	/// <summary>Slashing, piercing, blunt or shot: met by physical mitigation and vulnerability.</summary>
	internal static bool IsPhysical(uint attackType)
	{
		return attackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot;
	}

	/// <summary>Magic: met by magic mitigation and vulnerability.</summary>
	internal static bool IsMagic(uint attackType)
	{
		return attackType == (uint)AttackType.Magic;
	}
}
