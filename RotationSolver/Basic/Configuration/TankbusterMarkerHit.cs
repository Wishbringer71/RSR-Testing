namespace RotationSolver.Basic.Configuration;

/// <summary>
/// What followed a tankbuster marker in <see cref="OtherConfiguration.TankbusterMarkerHits"/>: the action whose hit
/// confirmed the marker and the time from the marker to that hit (concept 09, A340).
/// </summary>
internal sealed class TankbusterMarkerHit
{
	/// <summary>The action that hit the marked member first, not counting party-wide actions.</summary>
	public uint ActionId { get; set; }

	/// <summary>
	/// Seconds from the marker's appearance to that hit, the shortest seen for this action: a defence timed by it
	/// comes early rather than late.
	/// </summary>
	public float Delay { get; set; }
}
