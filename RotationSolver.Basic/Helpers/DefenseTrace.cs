using ECommons.DalamudServices;
using ECommons.Logging;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// A plain-text record of why the defensive chain fired, written to <c>DefenseTrace.log</c> in the
/// plugin's config folder.
/// </summary>
/// <remarks>
/// The owner's report of 29.09.2026: Radiant Aegis is still spent on tankbusters aimed at the tank
/// while he stands far away. Five sources can open a player's defence - an area marker, a listed area
/// cast, a large interruptible one, a BossModReborn raidwide or tankbuster, a tankbuster aimed at him -
/// and which of them fired in his fights cannot be read from the code. Each line names the action the
/// chain chose and every source standing at that moment; each enemy hit on the player is listed
/// beside it, so the file shows whether the hit the defence was spent on arrived.
///
/// The file is kept across loads and builds: the owner rebuilds the branch before playing and may
/// reload several times before uploading, and replacing the file at each load lost every session but
/// the last. Each session opens with its date, time and the commit the plugin was built from, so the
/// sections can be told apart and matched to their code. Deleting the file starts it over. Written
/// through one open writer under a lock: the dispatch writes from the framework thread, the effect
/// handler from the game thread.
/// </remarks>
internal static class DefenseTrace
{
	private static readonly object _lock = new();
	private static StreamWriter? _writer;
	private static uint _lastDecision;
	private static DateTime _lastDecisionWritten = DateTime.MinValue;

	/// <summary>The trace file's full path.</summary>
	public static string FilePath => Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "DefenseTrace.log");

	/// <summary>Starts this session's section of the trace, after whatever earlier sessions wrote.</summary>
	public static void Start(string header)
	{
		lock (_lock)
		{
			try
			{
				_writer?.Dispose();
				_writer = new StreamWriter(FilePath, true) { AutoFlush = true };
				_writer.WriteLine();
				_writer.WriteLine($"==== {DateTime.Now:yyyy-MM-dd HH:mm:ss} trace started");
				_writer.WriteLine(header);
			}
			catch (Exception ex)
			{
				_writer = null;
				PluginLog.Warning($"Could not start the defence trace: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// The defensive chain chose <paramref name="act"/> on the way named by <paramref name="path"/>.
	/// </summary>
	/// <remarks>
	/// The chain returns the same choice every frame until the action goes out, so a choice is
	/// written again only when it changes or once a GCD has passed - enough to follow it, not a line
	/// per frame.
	/// </remarks>
	public static void Decision(string path, IAction? act)
	{
		if (act == null || _writer == null)
		{
			return;
		}

		// Checked and set under the lock: the trace of 30.09.2026 holds the same choice twice, 21 ms apart.
		var now = DateTime.Now;
		lock (_lock)
		{
			if (act.ID == _lastDecision && (now - _lastDecisionWritten).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}

			_lastDecision = act.ID;
			_lastDecisionWritten = now;
		}

		Line($"{path} -> {act.Name} #{act.ID} | {DataCenter.DescribeDefenseSources()}");
	}

	/// <summary>Adds one line with the time of day in front.</summary>
	public static void Line(string text)
	{
		lock (_lock)
		{
			try
			{
				_writer?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {text}");
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Could not write the defence trace: {ex.Message}");
			}
		}
	}

	/// <summary>Writes a last line and closes the file.</summary>
	public static void Stop(string footer)
	{
		lock (_lock)
		{
			try
			{
				_writer?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {footer}");
				_writer?.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} trace ended");
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Could not finish the defence trace: {ex.Message}");
			}
			finally
			{
				_writer?.Dispose();
				_writer = null;
			}
		}
	}
}
