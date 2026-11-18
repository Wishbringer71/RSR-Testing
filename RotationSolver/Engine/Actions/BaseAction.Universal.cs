using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.Engine.Actions;

public partial class BaseAction : IBaseAction
{
	private ActionConfig? _defaultConfig;

	/// <summary>
	/// The rotation's own defaults for this action, built once. <see cref="Config"/> compares against
	/// them on every read to carry default changes over into stored user configs, and it is read
	/// several times per <see cref="CanUse"/> for every action the rotation evaluates each frame -
	/// rebuilding them per read allocated an <see cref="ActionConfig"/> that was thrown away again.
	/// Safe to keep: every <c>CreateConfig</c> in this repository returns literal values, and
	/// <see cref="Setting"/> is assigned once during construction.
	/// </summary>
	private ActionConfig DefaultConfig => _defaultConfig ??= Setting.CreateConfig?.Invoke() ?? new ActionConfig();
}
