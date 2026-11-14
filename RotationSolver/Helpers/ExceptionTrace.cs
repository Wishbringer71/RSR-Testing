using System.Diagnostics;
using System.Reflection;
using ECommons.Logging;

namespace RotationSolver.Helpers;

/// <summary>
/// Concept 24, "Ursachen, nicht nur Symptome" (A798; his rule of 14.11.2026: "die ausnahme verhindert ja nur das ergebnis
/// crash. es ist also nur symptombehandlung. ursachenforschung muss dennoch sein"): every .NET exception that arises in
/// RSR's code - caught later or not - is reported the first time per place and session, in the defence trace he uploads
/// and with its stack as an error in the Dalamud log, which shows at every log level. A catch keeps the game alive; this
/// keeps the cause visible.
/// </summary>
internal static class ExceptionTrace
{
	private static readonly Assembly _own = typeof(ExceptionTrace).Assembly;
	private static readonly object _lock = new();
	private static readonly HashSet<string> _reported = [];
	private static bool _enabled;

	[ThreadStatic]
	private static bool _inside;

	internal static void Enable()
	{
		if (!_enabled)
		{
			_enabled = true;
			AppDomain.CurrentDomain.FirstChanceException += OnFirstChance;
		}
	}

	internal static void Disable()
	{
		if (_enabled)
		{
			_enabled = false;
			AppDomain.CurrentDomain.FirstChanceException -= OnFirstChance;
		}
	}

	// Runs on the throwing thread before the stack unwinds, for every exception of the process (all plugins share it): it
	// must never throw, and an exception inside it must not report itself.
	private static void OnFirstChance(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args)
	{
		if (_inside)
		{
			return;
		}

		_inside = true;
		try
		{
			var stack = new StackTrace(1, false);
			MethodBase? place = null;
			for (var i = 0; i < stack.FrameCount && place == null; i++)
			{
				var method = stack.GetFrame(i)?.GetMethod();
				if (method?.DeclaringType is { } type && type.Assembly == _own && type != typeof(ExceptionTrace))
				{
					place = method;
				}
			}

			if (place == null)
			{
				return;
			}

			var ex = args.Exception;
			var where = $"{place.DeclaringType?.FullName}.{place.Name}";
			lock (_lock)
			{
				if (!_reported.Add($"{ex.GetType().FullName}|{where}"))
				{
					return;
				}
			}

			var thrownIn = ex.TargetSite is { } site && site != place ? $" (thrown in {site.DeclaringType?.Name}.{site.Name})" : string.Empty;
			DefenseTrace.Line($"exception: {ex.GetType().Name} in {where}{thrownIn}: {ex.Message} (first of this place in the session)");
			PluginLog.Error($"[ExceptionTrace] {ex.GetType().FullName} in {where}{thrownIn}: {ex.Message}\n{stack}");
		}
		catch
		{
			// A report that fails must not become the fault it reports.
		}
		finally
		{
			_inside = false;
		}
	}
}
