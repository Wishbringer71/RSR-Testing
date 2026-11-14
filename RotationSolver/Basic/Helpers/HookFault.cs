using ECommons.Logging;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Writes a fault caught in a hook detour, once per place and session (concept 24, "Fehlertoleranz: Code, den das Spiel
/// aufruft", A794). A .NET exception that left a detour would close the game; caught, the game goes on without RSR's
/// part at that place, and this says where and why - in the Dalamud log with the stack, and in the defence trace.
/// </summary>
internal static class HookFault
{
	private static readonly object _lock = new();
	private static readonly HashSet<string> _reported = [];

	/// <summary>Reports <paramref name="ex"/> caught at <paramref name="where"/>; later faults there stay quiet. Never throws.</summary>
	internal static void Report(string where, Exception ex)
	{
		try
		{
			lock (_lock)
			{
				if (!_reported.Add(where))
				{
					return;
				}
			}

			PluginLog.Error($"[{where}] caught in a hook, the game went on without this part (once per session): {ex}");
			DefenseTrace.Line($"hook fault: {where} {ex.GetType().Name}: {ex.Message} (once per session)");
		}
		catch
		{
			// A report that fails must not become the fault it reports.
		}
	}
}
