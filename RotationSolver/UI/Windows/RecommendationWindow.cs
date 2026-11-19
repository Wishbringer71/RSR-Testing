using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using RebornMaterial;

namespace RotationSolver.UI.Windows;

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
/// A816 (the owner's rule of 14.11.2026, his reproach of 15.11.2026 with a picture of the team window): the window is never
/// taller than the screen and moves up where it would reach past its bottom; what follows <see cref="Footer"/> - the action
/// and its status - stands below the scrolled content, always in sight; a <see cref="List"/> spreads over as many columns
/// as keep the window no taller than wide while every column keeps 50 characters and all fit the screen.
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
	private float _left;
	private float _top;
	private float _screenWidth;
	private float _screenHeight;
	private float _screenBottom;
	private float _fit;
	private bool _sized;
	private bool _glued = true;

	// A816: the scrolled body and the footer below it, in window coordinates.
	private Vector2 _origin;
	private float _bodyTop;
	private float _bodyNeed;
	private float _footerHeight;
	private float _footerStart;

	// A816: the columns of the lists, chosen before the frame from the last frame's measure.
	private int _columns = 1;
	private int _drawnColumns = 1;
	private float _listLeft;
	private float _listRoom;
	private float _listSpan;
	private float _listTotal;
	private float _columnNeed;
	private float _columnMin;
	private float _otherStart;
	private float _otherMin;
	private bool _inList;
	private int _listCount;
	private int _listPerColumn;
	private float _cellLeft;
	private float _cellTop;
	private float _cellTallest;

	// The card being drawn: a reason sits right under its line, the next line a line-height below.
	private bool _inCard;
	private bool _cardHasItem;
	private bool _lastWasLine;

	protected RecommendationWindow(string name) : base(name, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		IsOpen = true;
		RespectCloseHotkey = false;
	}

	// A896 (concept 03, "Geschlossen bis zum nächsten Anlass"): whether the window's condition held in the last frame.
	private bool _wanted;

	/// <summary>
	/// Every frame, open or closed (Dalamud asks DrawConditions only of an open window): a window the player closed comes
	/// back when its condition holds anew - the next team screen, shop or spoils - and stays closed for the one it was closed on.
	/// </summary>
	public sealed override void PreOpenCheck()
	{
		var wanted = DrawConditions();
		if (wanted && !_wanted)
		{
			IsOpen = true;
		}

		_wanted = wanted;
	}

	/// <summary>A896: a close by the player goes into the trace, so an upload tells it from a window that never showed.</summary>
	public override void OnClose()
	{
		if (_wanted)
		{
			DefenseTrace.Line($"recommendation window closed by the player: {WindowName}");
		}
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
		_otherStart = 0f;
		_otherMin = 0f;
		_listSpan = 0f;
		_listTotal = 0f;
		_columnNeed = 0f;
		_columnMin = 0f;
		_inCard = false;
		_lastWasLine = false;
		_drawnColumns = _columns;
		_origin = ImGui.GetWindowPos();
		_footerStart = -1f;
		var style = ImGui.GetStyle();
		var size = ImGui.GetWindowSize();

		// A816: the content scrolls in a body as high as the window leaves above the footer measured last frame.
		_bodyTop = ImGui.GetCursorPosY();
		// A scrollbar only where the content does not fit by more than a pixel: one shown for a rounding remainder narrows the
		// text, which then needs more height, the window grows, the scrollbar goes - and the window flickers between two heights.
		var bodyHeight = MathF.Max(1f, ImGui.GetContentRegionAvail().Y - _footerHeight);
		var bodyFlags = _bodyNeed <= bodyHeight + 1f ? ImGuiWindowFlags.NoScrollbar : ImGuiWindowFlags.None;
		_bodyNeed = 0f;
		_ = ImGui.BeginChild("##recommendation_body", new Vector2(0f, bodyHeight), false, bodyFlags);
		DrawContent();
		EndBody();

		var end = ImGui.GetCursorPosY();
		_footerHeight = _footerStart < 0f ? 0f : end - _footerStart;
		_height = MathF.Ceiling(_bodyTop + _bodyNeed + (_footerHeight > 0f ? _footerHeight : 0f) + style.WindowPadding.Y);
		if (_measured)
		{
			_glued = size.Y >= _fit - 1f;
		}

		// The screen the window stands on, which with Dalamud's multi-monitor windows need not be the main one.
		var viewport = ImGui.GetWindowViewport();
		_size = size;
		_left = _origin.X;
		_top = _origin.Y;
		_screenWidth = viewport.WorkSize.X;
		_screenHeight = viewport.WorkSize.Y;
		_screenBottom = viewport.WorkPos.Y + viewport.WorkSize.Y;
		_measured = true;
	}

	/// <summary>
	/// A816: what is drawn after this - the window's action and its status - stands below the scrolled content, always in
	/// sight; without it the whole content scrolls.
	/// </summary>
	protected void Footer()
	{
		EndBody();
		_footerStart = ImGui.GetCursorPosY();
	}

	// Closes the scrolled body once, with the height its content needs.
	private void EndBody()
	{
		if (_footerStart >= 0f)
		{
			return;
		}

		// Measured whether or not ImGui reports the body visible: the content is laid out either way.
		_bodyNeed = MathF.Max(0f, ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);
		ImGui.EndChild();
		_footerStart = ImGui.GetCursorPosY();
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

		// A816: columns are chosen from a frame drawn at least as wide as its columns need - the very first frame measures a
		// 32 px window, and a narrower one wraps more and reads the lists as taller than they are at their measure.
		var padding = _drawnColumns > 1 ? CellSides() : 0f;
		var columns = _sized && _listRoom >= _drawnColumns * (_columnNeed + padding) - 1f ? Columns() : _columns;
		var changed = columns != _columns;
		_columns = columns;
		var minWidth = MathF.Min(_listTotal > 0f ? MathF.Max(_otherMin, ListWidth(_columns, _columnMin)) : _minWidth, _screenWidth);
		var startWidth = Math.Clamp(_listTotal > 0f ? ProjectedWidth(_columns) : _startWidth, minWidth, _screenWidth);

		// A816: never taller than the screen; where the window would reach past its bottom, it moves up.
		_fit = MathF.Max(MathF.Min(_height, _screenHeight), ImGui.GetStyle().WindowMinSize.Y);
		Position = _top + MathF.Min(_size.Y, _fit) > _screenBottom ? new Vector2(_left, _screenBottom - MathF.Min(_size.Y, _fit)) : null;
		PositionCondition = ImGuiCond.Always;

		SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(minWidth / scale, 0f),
			MaximumSize = new Vector2(float.MaxValue, _fit / scale),
		};
		SizeCondition = ImGuiCond.Always;

		if (!_sized || changed)
		{
			_sized = true;
			if (_size.X < minWidth || changed)
			{
				// A816: a new column count lays the window out anew at its start width, as on its first frame.
				Size = new Vector2(startWidth, _glued || !changed ? _fit : MathF.Min(_size.Y, _fit)) / scale;
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

	/// <summary>
	/// One of a few choices as Material segmented buttons, the chosen one marked (A804); the index picked this frame, else
	/// -1. As wide as its labels need, at most the room left; the window keeps room for all of it.
	/// </summary>
	protected int Choice(string id, M3Segment[] segments, int current)
	{
		var width = MathF.Min(M3Widgets.SegmentedWidth(segments), MathF.Max(1f, ImGui.GetContentRegionAvail().X));
		var picked = M3Widgets.SegmentedButtons(id, segments, current, width);
		Extent();
		_lastWasLine = false;
		return picked;
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
		var start = ImGui.GetCursorScreenPos().X - _origin.X;
		if (_inList)
		{
			// A816: a text in a list counts for its column; the window's width follows from the columns at the list's end.
			_columnMin = MathF.Max(_columnMin, start - _cellLeft + ReadableText.Shortest(text));
			_columnNeed = MathF.Max(_columnNeed, start - _cellLeft + ReadableText.Column(text));
		}
		else
		{
			var right = InsetRight();
			_minWidth = MathF.Max(_minWidth, start + ReadableText.Shortest(text) + right);
			_startWidth = MathF.Max(_startWidth, start + ReadableText.Column(text) + right);
			_otherStart = MathF.Max(_otherStart, start + ReadableText.Column(text) + right);
			_otherMin = MathF.Max(_otherMin, start + ReadableText.Shortest(text) + right);
		}

		// A816: in a window widened by columns every text keeps its readable line length, in a column as outside it.
		var capped = _drawnColumns > 1;
		if (capped)
		{
			var room = ImGui.GetContentRegionAvail().X - (_inCard && !_inList ? M3Card.RightInset : 0f);
			ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + MathF.Max(1f, MathF.Min(room, ReadableText.Column(text))));
		}
		else if (!_inCard)
		{
			ImGui.PushTextWrapPos(0f);
		}

		M3Text.Draw(text, style);
		if (capped || !_inCard)
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
		var edge = ImGui.GetItemRectMax().X - _origin.X;
		if (_inList)
		{
			_columnMin = MathF.Max(_columnMin, edge - _cellLeft);
			_columnNeed = MathF.Max(_columnNeed, edge - _cellLeft);
			return;
		}

		var right = edge + InsetRight();
		_minWidth = MathF.Max(_minWidth, right);
		_startWidth = MathF.Max(_startWidth, right);
		_otherStart = MathF.Max(_otherStart, right);
		_otherMin = MathF.Max(_otherMin, right);
	}

	/// <summary>
	/// A816: a list of <paramref name="count"/> items, spread top to bottom over the columns chosen for the window; call
	/// <see cref="ListScope.Item"/> before each item and dispose the scope after the last.
	/// </summary>
	protected ListScope List(string id, int count)
	{
		_inList = true;
		_listCount = count;
		_listLeft = ImGui.GetCursorScreenPos().X - _origin.X;
		_listRoom = ImGui.GetContentRegionAvail().X - (_inCard ? M3Card.RightInset : 0f);
		_listPerColumn = Math.Max(1, (count + _drawnColumns - 1) / _drawnColumns);
		_cellTallest = 0f;
		var tabled = _drawnColumns > 1 && ImGui.BeginTable(id, _drawnColumns, ImGuiTableFlags.SizingStretchSame,
			new Vector2(MathF.Max(1f, ImGui.GetContentRegionAvail().X - (_inCard ? M3Card.RightInset : 0f)), 0f));
		if (tabled)
		{
			NextCell();
		}
		else
		{
			_cellLeft = _listLeft;
			_cellTop = ImGui.GetCursorPosY();
		}

		return new ListScope(this, tabled);
	}

	// The next column of a list: its text wraps at the column's edge.
	private void NextCell()
	{
		ImGui.TableNextColumn();
		ImGui.PushTextWrapPos(0f);
		_cellLeft = ImGui.GetCursorScreenPos().X - _origin.X;
		_cellTop = ImGui.GetCursorPosY();
	}

	// Closes a column, noting its height.
	private void EndCell(bool tabled)
	{
		var height = ImGui.GetCursorPosY() - _cellTop;
		_cellTallest = MathF.Max(_cellTallest, height);
		_listTotal += height;
		if (tabled)
		{
			ImGui.PopTextWrapPos();
		}
	}

	// The list's end: its columns' width becomes the window's, its tallest column its height.
	private void EndList(bool tabled)
	{
		EndCell(tabled);
		if (tabled)
		{
			ImGui.EndTable();
		}

		_inList = false;
		_listSpan += _cellTallest;
		var right = InsetRight();
		var padding = _drawnColumns > 1 ? CellSides() : 0f;
		_minWidth = MathF.Max(_minWidth, _listLeft + _drawnColumns * (_columnMin + padding) + right);
		_startWidth = MathF.Max(_startWidth, _listLeft + _drawnColumns * (_columnNeed + padding) + right);
	}

	// The width the window opens at with this many columns.
	private float ProjectedWidth(int columns) => MathF.Max(_otherStart, ListWidth(columns, _columnNeed));

	// The lists' width with this many columns of this width each.
	private float ListWidth(int columns, float column)
	{
		var padding = columns > 1 ? CellSides() : 0f;
		return _listLeft + columns * (column + padding) + InsetRightOuter();
	}

	/// <summary>
	/// A816: the fewest columns that keep the window no taller than wide, as long as every column keeps its 50 characters
	/// and all fit the screen; where none does, the most that fit. Projected from the last frame's measure: the content's
	/// height with the lists' height spread over the columns.
	/// </summary>
	private int Columns()
	{
		if (_listTotal <= 0f || _columnNeed <= 0f)
		{
			return 1;
		}

		var rest = _height - _listSpan;
		var padding = CellSides();
		var most = Math.Max(1, Math.Min(_listCount, (int)((_screenWidth - _listLeft - InsetRightOuter()) / MathF.Max(1f, _columnMin + padding))));
		for (var columns = 1; columns <= most; columns++)
		{
			if (MathF.Min(rest + _listTotal / columns, _screenHeight) <= ProjectedWidth(columns))
			{
				return columns;
			}
		}

		return most;
	}

	// A column's padding on its left and its right.
	private static float CellSides() => ImGui.GetStyle().CellPadding.X + ImGui.GetStyle().CellPadding.X;

	// What lies right of the content in the outer window: the card's inset and the window padding.
	private static float InsetRightOuter() => ImGui.GetStyle().WindowPadding.X + M3Card.RightInset;

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

	/// <summary>A list of the window (A816): <see cref="Item"/> before each item moves to the next column where due.</summary>
	protected readonly struct ListScope(RecommendationWindow window, bool tabled) : IDisposable
	{
		public void Item(int index)
		{
			if (tabled && index > 0 && index % window._listPerColumn == 0)
			{
				window.EndCell(tabled);
				window.NextCell();
				window._cardHasItem = false;
			}
		}

		public void Dispose() => window.EndList(tabled);
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
