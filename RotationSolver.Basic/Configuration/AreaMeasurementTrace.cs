using ECommons.DalamudServices;
using ECommons.Logging;

namespace RotationSolver.Basic.Configuration;

/// <summary>
/// A plain-text record of the learned damage table's path through one session, written to
/// <c>AreaMeasurementTrace.log</c> in the plugin's config folder.
/// </summary>
/// <remarks>
/// Asked for by the owner, after four duties with area damage left the table empty and the
/// windows could not say why ("schreib debug-kommentare in die ausgabedatei ... mit den
/// unterschiedlichen stellen, bis wo die routinen kommen"). Every line says how far one enemy
/// action got on the way from the game's effect message to the file: which check stopped it, or
/// that it was stored. The file is replaced at every load, so it holds exactly one session and
/// cannot grow past it.
///
/// Written through one open writer under a lock: the effect handler writes from the game thread,
/// the store from a pool thread.
/// </remarks>
internal static class AreaMeasurementTrace
{
	private static readonly object _lock = new();
	private static StreamWriter? _writer;

	/// <summary>The trace file's full path, for the windows.</summary>
	public static string FilePath => Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "AreaMeasurementTrace.log");

	/// <summary>Starts a new trace for this session, replacing the last one.</summary>
	public static void Start(string header)
	{
		lock (_lock)
		{
			try
			{
				_writer?.Dispose();
				_writer = new StreamWriter(FilePath, false) { AutoFlush = true };
				_writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} trace started");
				_writer.WriteLine(header);
			}
			catch (Exception ex)
			{
				_writer = null;
				PluginLog.Warning($"Could not start the area measurement trace: {ex.Message}");
			}
		}
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
				PluginLog.Warning($"Could not write the area measurement trace: {ex.Message}");
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
				PluginLog.Warning($"Could not finish the area measurement trace: {ex.Message}");
			}
			finally
			{
				_writer?.Dispose();
				_writer = null;
			}
		}
	}
}
