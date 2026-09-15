namespace RotationSolver.Basic.Data;

/// <summary>
/// Defines special combat modes that restrict or alter player actions and movement
/// in response to specific mechanic effects.
/// <para>
/// The numbers are not free to choose: BossModReborn hands its own AIHints.SpecialMode
/// across the IPC boundary as a plain int, which BossModUpdater casts straight into this
/// enum, so every member has to sit on the same ordinal as its counterpart there.
/// </para>
/// </summary>
public enum SpecialMode
{
	/// <summary>
	/// No special restrictions are active. The player may move and act freely.
	/// </summary>
	Normal = 0,

	/// <summary>
	/// Pyretic or acceleration-bomb type effect is active. At activation time,
	/// no movement, no actions, and no casting are allowed.
	/// </summary>
	Pyretic = 1,

	/// <summary>
	/// A no-movement effect is active. The player must remain stationary.
	/// </summary>
	NoMovement = 2,

	/// <summary>
	/// A freezing effect is active. The player is expected to be moving at activation time.
	/// </summary>
	Freezing = 3,

	/// <summary>
	/// A temporary misdirection effect is active, altering the player's movement direction.
	/// </summary>
	Misdirection = 4,
}

/// <summary>
/// Describes the type of incoming damage predicted by BossModReborn,
/// used to determine appropriate mitigation or response actions.
/// </summary>
/// <remarks>
/// <b>These ordinals are an interface contract, not an internal detail.</b> The value arrives as a
/// plain <c>int</c> over IPC - BossModReborn registers <c>Hints.NextDamageType</c> as
/// <c>(int)predicted[0].Type</c>, the ordinal of <i>its</i> enum - and
/// <c>BossModUpdater</c> casts it straight to this type. Inserting or reordering a member here
/// therefore reinterprets what the other plugin sends, and nothing fails: the wrong mitigation is
/// simply chosen. The values are written out so that is visible while editing, which is the
/// precaution <see cref="SpecialMode"/> already carries for the same reason.
/// <para>
/// What is <i>not</i> established here is that the other side numbers them the same way. That
/// needs its enum, which is not reachable from the build environment; the numbering below is
/// today's implicit one, written down unchanged. See TODO.md.
/// </para>
/// </remarks>
public enum PredictedDamageType
{
	/// <summary>
	/// No incoming damage is predicted.
	/// </summary>
	None = 0,

	/// <summary>
	/// A tankbuster attack is incoming, targeting one or more tanks.
	/// </summary>
	Tankbuster = 1,

	/// <summary>
	/// A raidwide attack is incoming, hitting all party members.
	/// </summary>
	Raidwide = 2,

	/// <summary>
	/// A shared damage attack is incoming, requiring multiple players to stack
	/// in order to split the damage.
	/// </summary>
	Shared = 3
}
