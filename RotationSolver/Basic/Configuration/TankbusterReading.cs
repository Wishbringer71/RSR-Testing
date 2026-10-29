namespace RotationSolver.Basic.Configuration;

/// <summary>
/// One tankbuster's learned figures in <see cref="OtherConfiguration.TankbusterPotential"/>, each a share
/// of the maximum HP of the one it hit, scaled back to what it would have done unmitigated.
/// </summary>
internal sealed class TankbusterReading
{
	/// <summary>The highest unmitigated share seen on a target without a vulnerability debuff.</summary>
	public float Unmitigated { get; set; }

	/// <summary>
	/// The highest unmitigated share seen on a target carrying a vulnerability debuff - kept apart, so a
	/// swap mechanic's second hit does not rate the action itself as that hard.
	/// </summary>
	public float UnderVulnerability { get; set; }

	/// <summary>Both figures at their higher value - the merge a save applies with the file.</summary>
	public static TankbusterReading Higher(TankbusterReading first, TankbusterReading second)
	{
		return new TankbusterReading
		{
			Unmitigated = Math.Max(first.Unmitigated, second.Unmitigated),
			UnderVulnerability = Math.Max(first.UnderVulnerability, second.UnderVulnerability),
		};
	}
}
