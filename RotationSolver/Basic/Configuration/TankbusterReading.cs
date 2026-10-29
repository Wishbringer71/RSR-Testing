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

	/// <summary>
	/// Learned only from the landings of an enemy's cast at its own target, a tank - no list entry, no marker (A452). Such a
	/// cast is no known tankbuster for the readers that open the defence or call for help: in a pull it is any trash cast at
	/// the tank. Only the plan for a cast at the player reads it. False in files written before, whose readings all came
	/// from list or marker; one landing under list or marker makes it false for good.
	/// </summary>
	public bool AimedOnly { get; set; }

	/// <summary>Both figures at their higher value, aimed only while both are - the merge a save applies with the file.</summary>
	public static TankbusterReading Higher(TankbusterReading first, TankbusterReading second)
	{
		return new TankbusterReading
		{
			Unmitigated = Math.Max(first.Unmitigated, second.Unmitigated),
			UnderVulnerability = Math.Max(first.UnderVulnerability, second.UnderVulnerability),
			AimedOnly = first.AimedOnly && second.AimedOnly,
		};
	}
}
