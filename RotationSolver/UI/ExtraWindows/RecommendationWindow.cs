using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using RebornMaterial;

namespace RotationSolver.UI.ExtraWindows;

/// <summary>
/// The one frame of RSR's recommendation windows (Crucible team, shop, treasure, campsite, route; Masked Carnivale set):
/// the scheme of RebornMaterial pushed for the whole window, sections as cards, a recommended line with a chip, reasons
/// and notes in the muted role, actions as Material buttons (concept 03, "Hilfsfenster", A633; the owner's proposal of
/// 06.11.2026).
///
/// Size and type follow the general rules of legibility, not a library's defaults (A654; the owner's rules of 07.11.2026).
/// The window is resizable in both directions. It opens as wide as its texts need - one line up to 75 characters, longer
/// ones broken at 65 - and never gets narrower than 50 characters of a text or than a title, chip or button, so nothing is
/// cut off. Its height is its content's down to the bottom of the screen and follows a change of content or width,
/// until the player drags it shorter; dragged back to the content, it follows again. The widths are measured in
/// characters of the text itself while it is drawn, and applied before the next frame. `window_layout_model.py` runs
/// this frame in Dear ImGui.
///
/// It used to size itself with ImGuiWindowFlags.AlwaysAutoResize while the cards wrapped their text at the window's
/// edge, so the content never asked for more width than it had: in Dear ImGui the team window opened 116 px wide, its
/// title cut off, and could not be resized (A654).
/// </summary>
internal abstract class RecommendationWindow : Window
{
	private M3Style.Scope _theme;

	// Measured while drawing, in pixels and window coordinates; applied before the next frame.
	private float _minWidth;
	private float _startWidth;
	private float _height;
	private bool _measured;

	// The window as the last frame left it.
	private Vector2 _size;
	private float _top;
	private float _screenWidth;
	private float _screenBottom;
	private float _fit;
	private bool _sized;
	private bool _glued = true;

	// The card being drawn: a reason sits right under its line, the next line a line-height below.
	private bool _inCard;
	private bool _cardHasItem;
	private bool _lastWasLine;

	protected RecommendationWindow(string name) : base(name, ImGuiWindowFlags.NoCollapse)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	public override void PreDraw()
	{
		_theme = M3Style.Push(M3Density.Tight);
		base.PreDraw();
		Fit();
	}

	public override void PostDraw()
	{
		base.PostDraw();
		_theme.Dispose();
		_theme = default;
	}

	public sealed override void Draw()
	{
		_minWidth = 0f;
		_startWidth = 0f;
		_inCard = false;
		_lastWasLine = false;
		DrawContent();

		var style = ImGui.GetStyle();
		_height = MathF.Ceiling(ImGui.GetCursorPosY() - style.ItemSpacing.Y + style.WindowPadding.Y);
		var size = ImGui.GetWindowSize();
		if (_measured)
		{
			_glued = size.Y >= _fit - 1f;
		}

		// The screen the window stands on, which with Dalamud's multi-monitor windows need not be the main one.
		var viewport = ImGui.GetWindowViewport();
		_size = size;
		_top = ImGui.GetWindowPos().Y;
		_screenWidth = viewport.WorkSize.X;
		_screenBottom = viewport.WorkPos.Y + viewport.WorkSize.Y;
		_measured = true;
	}

	/// <summary>The window's content, drawn with <see cref="Section"/>, <see cref="Line"/>, <see cref="Note"/>,
	/// <see cref="Title"/> and <see cref="Action"/>.</summary>
	protected abstract void DrawContent();

	/// <summary>
	/// Size limits and, where due, the size for the coming frame (Dalamud applies both after PreDraw, scaled by the global
	/// scale). The height may not exceed the content or the screen; the width may not drop below what the content needs.
	/// On the first frame of a session a width below that minimum was not chosen under these rules - it is the one the old
	/// auto-size mode saved - and the window takes its start width; afterwards the height follows the content while the
	/// window stands at it.
	/// </summary>
	private void Fit()
	{
		if (!_measured)
		{
			Size = null;
			SizeConstraints = null;
			return;
		}

		var scale = ImGuiHelpers.GlobalScale;
		var minWidth = MathF.Min(_minWidth, _screenWidth);
		var startWidth = Math.Clamp(_startWidth, minWidth, _screenWidth);
		_fit = MathF.Max(MathF.Min(_height, _screenBottom - _top), ImGui.GetStyle().WindowMinSize.Y);

		SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(minWidth / scale, 0f),
			MaximumSize = new Vector2(float.MaxValue, _fit / scale),
		};
		SizeCondition = ImGuiCond.Always;

		if (!_sized)
		{
			_sized = true;
			if (_size.X < minWidth)
			{
				Size = new Vector2(startWidth, _fit) / scale;
				return;
			}
		}

		Size = _glued && MathF.Abs(_size.Y - _fit) >= 1f ? new Vector2(_size.X, _fit) / scale : null;
	}

	/// <summary>A titled card for one part of the window; dispose it to close the card.</summary>
	protected SectionScope Section(string id, string title, FontAwesomeIcon icon, string? subtitle = null)
	{
		var card = M3Card.Begin(id);
		_inCard = true;
		using (ImRaii.PushFont(TitleFont()))
		{
			var glyph = M3Draw.MeasureIcon(icon);
			ImGui.Dummy(glyph);
			M3Draw.Icon(ImGui.GetWindowDrawList(), icon, ImGui.GetItemRectMin(), M3.Scheme.Primary);
			ImGui.SameLine();
			ImGui.TextUnformatted(title);
			Extent();
		}

		if (!string.IsNullOrEmpty(subtitle))
		{
			Text(subtitle, M3TextStyle.Muted);
		}

		_cardHasItem = false;
		_lastWasLine = false;
		return new SectionScope(card, this);
	}

	/// <summary>The window's heading outside a card, in the title size.</summary>
	protected void Title(string text)
	{
		using var font = ImRaii.PushFont(TitleFont());
		ImGui.TextUnformatted(text);
		Extent();
	}

	/// <summary>One line of the recommendation: an optional chip (success, warning or info), then the text.</summary>
	protected void Line(string text, string? chip = null, M3Severity severity = M3Severity.Success)
	{
		if (_inCard && _cardHasItem)
		{
			Gap();
		}

		if (chip != null)
		{
			Chip(chip, severity);
		}

		Text(text, M3TextStyle.Body);
		_cardHasItem = _inCard;
		_lastWasLine = true;
	}

	/// <summary>A reason, a basis or a status, in the muted role; right under the line it explains.</summary>
	protected void Note(string text)
	{
		if (text.Length == 0)
		{
			return;
		}

		if (_inCard && _cardHasItem && !_lastWasLine)
		{
			Gap();
		}

		Text(text, M3TextStyle.Muted);
		_cardHasItem = _inCard;
		_lastWasLine = false;
	}

	/// <summary>The window's action: a filled Material button.</summary>
	protected bool Action(string id, string label, FontAwesomeIcon icon, bool enabled = true)
	{
		var pressed = M3Widgets.Button(id, label, M3ButtonStyle.Filled, icon, enabled: enabled);
		Extent();
		_lastWasLine = false;
		return pressed;
	}

	/// <summary>
	/// A text that wraps at the card's or window's edge. The window opens at the text's readable line length and never
	/// gets narrower than its shortest readable line (<see cref="ReadableText"/>).
	/// </summary>
	private void Text(string text, M3TextStyle style)
	{
		var start = ImGui.GetCursorPosX();
		var right = InsetRight();
		_minWidth = MathF.Max(_minWidth, start + ReadableText.Shortest(text) + right);
		_startWidth = MathF.Max(_startWidth, start + ReadableText.Column(text) + right);

		if (!_inCard)
		{
			ImGui.PushTextWrapPos(0f);
		}

		M3Text.Draw(text, style);
		if (!_inCard)
		{
			ImGui.PopTextWrapPos();
		}
	}

	/// <summary>
	/// The verdict of a line in the body's size on the scheme's container role. RebornMaterial's badge sets it in 11 px,
	/// below the smallest Axis size the game uses (9.6 pt); the word carries the meaning, the color only repeats it.
	/// </summary>
	private void Chip(string label, M3Severity severity)
	{
		var padding = ImGui.GetStyle().FramePadding;
		ImGui.AlignTextToFramePadding();
		ImGui.Dummy(ImGui.CalcTextSize(label) + padding + padding);
		var min = ImGui.GetItemRectMin();
		var drawList = ImGui.GetWindowDrawList();
		drawList.AddRectFilled(min, ImGui.GetItemRectMax(), M3.U32(M3.SeverityContainer(severity)), M3.ShapeFull);
		drawList.AddText(min + padding, M3.U32(M3.OnSeverityContainer(severity)), label);
		Extent();
		ImGui.SameLine();
	}

	/// <summary>A line-height between two recommendations - the paragraph space of 1 em; a reason stays close to its line.</summary>
	private static void Gap()
	{
		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + MathF.Max(0f, ImGui.GetTextLineHeight() - ImGui.GetStyle().ItemSpacing.Y));
	}

	/// <summary>The last item does not wrap: the window keeps room for all of it.</summary>
	private void Extent()
	{
		var right = ImGui.GetItemRectMax().X - ImGui.GetWindowPos().X + InsetRight();
		_minWidth = MathF.Max(_minWidth, right);
		_startWidth = MathF.Max(_startWidth, right);
	}

	/// <summary>What lies right of the content: the card's inset, the window padding, a scrollbar when one shows.</summary>
	private float InsetRight()
	{
		var style = ImGui.GetStyle();
		return style.WindowPadding.X + (_inCard ? M3Card.RightInset : 0f) + (ImGui.GetScrollMaxY() > 0f ? style.ScrollbarSize : 0f);
	}

	/// <summary>The smallest of RebornMaterial's title roles that is larger than the body text.</summary>
	private static ImFontPtr TitleFont()
	{
		var body = ImGui.GetTextLineHeight();
		using (ImRaii.PushFont(M3.TitleLarge))
		{
			if (ImGui.GetTextLineHeight() > body)
			{
				return M3.TitleLarge;
			}
		}

		return M3.HeadlineSmall;
	}

	/// <summary>A card of the window; closing it closes the card and ends its spacing.</summary>
	protected readonly struct SectionScope(M3Card.Scope card, RecommendationWindow window) : IDisposable
	{
		public void Dispose()
		{
			card.Dispose();
			window._inCard = false;
			window._cardHasItem = false;
		}
	}
}
