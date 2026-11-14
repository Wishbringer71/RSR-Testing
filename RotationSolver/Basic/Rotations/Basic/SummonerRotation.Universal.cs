using Dalamud.Interface.Colors;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class SummonerRotation
{
	/// <summary>
	/// Is a Searing Light on the player at all, no matter who cast it?
	///
	/// Searing Light does not stack, it overwrites, and it raises damage by the same 5% whoever it
	/// came from. <see cref="HasSearingLight"/> counts the player's own buff alone, which is the
	/// right question for "may I cast it" and the wrong one for "am I standing in a buff window".
	/// </summary>
	public static bool HasAnySearingLight => StatusHelper.PlayerHasStatus(false, StatusID.SearingLight);

	/// <summary>
	/// Which burst phase the Summoner is standing in, if any. Kinds, not positions: Solar occupies
	/// two of the four demi slots in a cycle, and whoever holds it holds both.
	/// </summary>
	protected enum SearingPhase
	{
		/// <summary>Outside every demi window.</summary>
		None,

		/// <summary>Solar Bahamut.</summary>
		Solar,

		/// <summary>Demi-Bahamut.</summary>
		Bahamut,

		/// <summary>Demi-Phoenix.</summary>
		Phoenix,
	}

	/// <inheritdoc cref="SearingPhase"/>
	protected static SearingPhase CurrentSearingPhase =>
		InPhoenix ? SearingPhase.Phoenix
		: InBahamut ? SearingPhase.Bahamut
		: InSolarBahamut ? SearingPhase.Solar
		: SearingPhase.None;

	private bool _searingPhaseBooked;

	private bool _inBurstWindow;

	private bool _burstHeldThisWindow;

	private bool _burstHeldLastWindow;
}
