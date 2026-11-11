using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RotationSolver.Helpers;

/// <summary>
/// Concept 21, K15 (A695): writes each window of the Crucible - its name begins with "XBM" - with its values to
/// <c>DefenseTrace.log</c> the first time it is set up and refreshed in a run, the score at the end of a board among them.
/// No source gives the layout of that window or the points the player's remaining HP bring (concept 21, "Am Ende des
/// Endkampfs"); the line holds both. It only listens.
/// </summary>
internal static class CrucibleWindowTrace
{
	private const string Prefix = "XBM";
	private static readonly HashSet<string> _seen = [];
	private static int _run = -1;

	public static void Enable()
	{
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, OnWindow);
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, OnWindow);
	}

	public static void Disable()
	{
		Svc.AddonLifecycle.UnregisterListener(OnWindow);
	}

	private static unsafe void OnWindow(AddonEvent type, AddonArgs args)
	{
		if (!DataCenter.IsInCrucible || !args.AddonName.StartsWith(Prefix, StringComparison.Ordinal))
		{
			return;
		}

		var (count, values) = args switch
		{
			AddonSetupArgs setup => (setup.AtkValueCount, setup.AtkValues),
			AddonRefreshArgs refresh => (refresh.AtkValueCount, refresh.AtkValues),
			_ => (0u, nint.Zero),
		};
		if (CrucibleBoard.Run != _run)
		{
			_run = CrucibleBoard.Run;
			_seen.Clear();
		}

		if (_seen.Add($"{args.AddonName} {type}"))
		{
			DefenseTrace.Line($"crucible window {args.AddonName} {type}: {BestiaryTrace.Values((AtkValue*)values, count)}");
		}
	}
}
