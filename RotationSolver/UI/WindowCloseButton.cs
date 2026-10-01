using Dalamud.Interface.Windowing;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.UI;

/// <summary>
/// Lets the close button in a window's title bar close it for good.
/// </summary>
/// <remarks>
/// The plugin sets IsOpen from the window's show setting on every framework update
/// (RotationSolverPlugin.UpdateDisplayWindow), so the X closed the window for one frame and the
/// next update opened it again: it could only be closed from the settings. Dalamud sets IsOpen to
/// false for the X, Escape and the gamepad back button while it draws the window, and calls
/// PostDraw after that in the same frame (WindowHost.Draw). The plugin closes windows from the
/// framework update instead, and a closed window is not drawn, so PostDraw never sees that case.
/// A window found closed in its own PostDraw was therefore closed by the player, and the setting
/// follows.
/// </remarks>
internal static class WindowCloseButton
{
	public static void TurnOffIfClosedByPlayer(Window window, ConditionBoolean showSetting)
	{
		if (window.IsOpen || !showSetting.Value)
		{
			return;
		}

		showSetting.Value = false;
		Service.Config.Save();
	}
}
