namespace RotationSolver.Basic.Configuration;

/// <summary>
/// What sleeps did to one enemy action in <see cref="OtherConfiguration.SleepOutcomes"/>, over every session
/// (concept 08, "Ein gegnerisches Wirken stoppen").
/// </summary>
/// <remarks>
/// Raw observations only; what follows from them - how long a sleep takes to stop a cast, how much of a cast it has
/// to leave, which action it cannot stop - <see cref="Helpers.SleepInterruptWatch"/> works out at each decision, so
/// a later observation corrects an earlier conclusion instead of being shut out by it.
/// </remarks>
internal sealed class SleepReading
{
	/// <summary>Casts of this action a sleep cut short.</summary>
	public int Stopped { get; set; }

	/// <summary>The longest one of them ran on after the sleep landed, in seconds.</summary>
	public float LongestStop { get; set; }

	/// <summary>Casts of this action that went off although a sleep had landed.</summary>
	public int WentOff { get; set; }

	/// <summary>The most of its cast one of them had left when the sleep landed, in seconds.</summary>
	public float LatestWentOff { get; set; }

	/// <summary>Every figure at its higher value - the merge a save applies with the file.</summary>
	public static SleepReading Higher(SleepReading first, SleepReading second)
	{
		return new SleepReading
		{
			Stopped = Math.Max(first.Stopped, second.Stopped),
			LongestStop = Math.Max(first.LongestStop, second.LongestStop),
			WentOff = Math.Max(first.WentOff, second.WentOff),
			LatestWentOff = Math.Max(first.LatestWentOff, second.LatestWentOff),
		};
	}
}
