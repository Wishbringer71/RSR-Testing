using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.DutyState;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using RebornMaterial;
using RotationSolver.Integrations.ActionTimeline;
using RotationSolver.Commands;
using RotationSolver.Integrations.IPC;
using RotationSolver.UI;
using RotationSolver.UI.Windows;
using RotationSolver.UI.TeachingMode;
using RotationSolver.UI.TeachingMode.ElementSpecial;
using RotationSolver.Updaters;
using Player = ECommons.GameHelpers.Player;

namespace RotationSolver;

public sealed partial class RotationSolverPlugin : IAsyncDalamudPlugin
{
	private static DiagnosticsWindow? _diagnosticsWindow;

	private static CrucibleTeamWindow? _crucibleTeamWindow;

	private static CrucibleShopWindow? _crucibleShopWindow;

	private static CrucibleCampWindow? _crucibleCampWindow;

	private static CrucibleRouteWindow? _crucibleRouteWindow;

	private static CarnivaleSetWindow? _carnivaleSetWindow;

	private static CrucibleTreasureWindow? _crucibleTreasureWindow;

	private static CrucibleSpoilsWindow? _crucibleSpoilsWindow;
}
