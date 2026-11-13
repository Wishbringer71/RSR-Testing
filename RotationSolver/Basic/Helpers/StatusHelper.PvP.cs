namespace RotationSolver.Basic.Helpers;

public static partial class StatusHelper
{
	/// <summary>
	/// Guard in PvP: two rows of the status sheet, both named Guard with the same text ("Damage taken is reduced ..."), 3054
	/// and 3673. Until A756 only 3054 counted, and a target under 3673 took the full burst (concept 25, "PvP: Ziele unter
	/// Guard und Unverwundbarkeit").
	/// </summary>
	public static StatusID[] PvPGuardStatuses { get; } =
	[
		StatusID.Guard,
		StatusID.Guard_3673,
	];

	/// <summary>
	/// The player's own states under which Guard is wasted or cannot go: invulnerable (Hallowed Ground), held above 1 HP
	/// (Undead Redemption), out of reach (Sky High), already holding still under reduced damage (Hardened Scales), "Cannot
	/// execute Guard" (Inner Release, Unguarded), and on a machina ("beneficial effects conferred by actions are
	/// nullified"). Guard holds every action for its 4 s, so a Guard under one of these lost them for nothing (concept 25,
	/// "PvP: Gemeinsame Aktionen", A756).
	/// </summary>
	public static StatusID[] GuardWastedStatuses { get; } =
	[
		StatusID.HallowedGround_1302,
		StatusID.UndeadRedemption,
		StatusID.SkyHigh,
		StatusID.HardenedScales,
		StatusID.InnerRelease_1303,
		StatusID.Unguarded,
		StatusID.Mounted,
	];
}
