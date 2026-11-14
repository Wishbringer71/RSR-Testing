namespace RotationSolver.Basic.Helpers;

internal static partial class ActionHelper
{
	/// <summary>
	/// The window before the GCD in which an ability does not start, before the first measurement: upstream's window
	/// (A388, A486).
	/// </summary>
	private const float FixedAbilityClipWindow = 0.5f;

	/// <summary>
	/// The window before the GCD in which no ability starts, so that its animation lock does not run into the GCD: the
	/// lock the server last gave plus the rest the player's actions keep after it, as measured in this zone (concept 03,
	/// "Weben, Animationssperre und Latenz", A753); upstream's fixed window before the first measurement.
	/// </summary>
	internal static float AbilityClipWindow => AbilityWindowFor(0);

	/// <summary>The same window for one action: its own lock where the server has given it.</summary>
	internal static float AbilityWindowFor(uint actionId)
		=> ActionManagerEx.CurrentLockTweak?.AbilityWindow(actionId) ?? FixedAbilityClipWindow;

	/// <summary>Whether the window is measured, not upstream's fixed one.</summary>
	internal static bool AbilityWindowMeasured => ActionManagerEx.CurrentLockTweak?.AbilityWindow(0) != null;

	/// <summary>
	/// Whether an ability still fits before the GCD after "Action ahead" reports the GCD: the measured window ends before
	/// the GCD is ready. Before the first measurement never, as before A753.
	/// </summary>
	internal static bool AbilityStillFits => AbilityWindowMeasured && DataCenter.DefaultGCDRemain > AbilityClipWindow;

	/// <summary>
	/// The least GCD left for an ability to weave: the measured window, or "Action ahead" before the first measurement.
	/// </summary>
	internal static float WeaveLimit => AbilityWindowMeasured ? AbilityClipWindow : DataCenter.CalculatedActionAhead;
}
