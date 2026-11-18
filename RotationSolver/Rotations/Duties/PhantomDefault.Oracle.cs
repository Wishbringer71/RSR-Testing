namespace RotationSolver.Rotations.Duties;

/// <summary>
/// Concept 07, "Sternenfall vor einer laufenden Phantom-Verjüngung" (A885): Starfall, which costs up to nine tenths of
/// your HP, waits for the last GCD before a running Phantom Rejuvenation heals, while its card outlasts that heal.
/// </summary>
public sealed partial class PhantomDefault
{
	private bool _starfallWaitWritten;

	private bool StarfallWaitsForRejuvenation()
	{
		var heal = StatusHelper.PlayerStatusTime(false, StatusID.PhantomRejuvenation);
		var card = StatusHelper.PlayerStatusTime(false, StatusID.PredictionOfStarfall);
		if (heal <= DataCenter.DefaultGCDTotal || card <= heal)
		{
			_starfallWaitWritten = false;
			return false;
		}

		if (!_starfallWaitWritten)
		{
			_starfallWaitWritten = true;
			DefenseTrace.Line($"starfall: waits for phantom rejuvenation, which heals in {heal:F1} s | card {card:F1} s left");
		}

		return true;
	}
}
