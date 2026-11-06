using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using RebornMaterial;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// The one frame of RSR's recommendation windows (Crucible team, shop, treasure, campsite, route; Masked Carnivale set): the
/// scheme of RebornMaterial pushed for the whole window as the other extra windows do, sections as cards, a recommended line
/// with a badge, reasons and notes in the muted role, actions as Material buttons (concept 03, "Hilfsfenster", A633; the
/// owner's proposal of 06.11.2026).
/// </summary>
internal abstract class RecommendationWindow : Window
{
	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;
	private M3Style.Scope _theme;

	protected RecommendationWindow(string name) : base(name, WindowFlags)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	public override void PreDraw()
	{
		_theme = M3Style.Push(M3Density.Tight);
		base.PreDraw();
	}

	public override void PostDraw()
	{
		base.PostDraw();
		_theme.Dispose();
		_theme = default;
	}

	/// <summary>A titled card for one part of the window; dispose it to close the card.</summary>
	protected static M3Card.Scope Section(string id, string title, FontAwesomeIcon icon, string? subtitle = null)
	{
		return M3Card.Begin(id, title, icon, subtitle: subtitle);
	}

	/// <summary>One line of the recommendation: an optional badge (success, warning or info), then the text.</summary>
	protected static void Line(string text, string? badge = null, M3Severity severity = M3Severity.Success)
	{
		if (badge != null)
		{
			M3Badge.Draw(badge, M3.Severity(severity));
			ImGui.SameLine();
		}

		M3Text.Draw(text);
	}

	/// <summary>A reason, a basis or a status, in the muted role.</summary>
	protected static void Note(string text)
	{
		if (text.Length > 0)
		{
			M3Text.Draw(text, M3TextStyle.Muted);
		}
	}

	/// <summary>The window's action: a filled Material button.</summary>
	protected static bool Action(string id, string label, FontAwesomeIcon icon, bool enabled = true)
	{
		return M3Widgets.Button(id, label, M3ButtonStyle.Filled, icon, enabled: enabled);
	}
}
