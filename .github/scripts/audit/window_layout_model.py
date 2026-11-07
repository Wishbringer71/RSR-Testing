#!/usr/bin/env python3
"""Layout model of RSR's recommendation windows, run frame by frame in a real Dear ImGui (concept 03, "Hilfsfenster",
A654).

The build compiles a window; it does not lay one out. The recommendation windows of A633 compiled; in Dear ImGui the
team window opened 116 px wide, its title cut off, pet names one letter per line, and could not be resized:
ImGuiWindowFlags.AlwaysAutoResize sized the window to its content while the cards wrapped their text at the window's
edge, so the content never asked for more width than it had. This
model runs the frame of RecommendationWindow - the sizing, the measuring helpers, the card - in Dear ImGui itself
(imgui-bundle) and checks what the player sees:

- the window opens at its start width: every text of up to 75 characters on one line, longer ones broken at 65;
- it never gets narrower than 50 characters of its text, nor than a title, chip or button, so nothing is cut off;
- its height is its content's, down to the bottom of the screen, follows a change of content or width, and stays where
  the player drags it shorter until the player drags it back;
- a width saved by the old auto-size mode (116 px) is replaced by the start width on the first frame;
- at twice the text size, and on a small screen, the same holds;
- a tooltip (ImguiTooltips: auto-size, 150 to 1200 px) breaks a long description at the same line length and shows all
  of it, where the old one ran it on in one line past its 1200 px.

It also runs the old construction and checks that it shows the defect (self-test: a check that passes the old window is
no check). Three constructed defects - no minimum width, no height that follows, no replacement of the old width -
must each fail a check.

WHAT THIS MEASURES, STATED HONESTLY: the logic in Dear ImGui 1.93, with Dalamud's order of calls (PreDraw, size and
constraints, Begin, Draw, End) and RebornMaterial's card reduced to its layout (indent, wrap at the card's right inset,
the closing dummy). Not Dalamud, not RebornMaterial's painting, not the game font: the text is set in a sans font of the
system at 16 px for the body and 19 px for titles, the sizes Axis has under Dalamud's defaults.

Usage: python3 window_layout_model.py [--png DIR]    (needs imgui-bundle; --png also needs numpy and pillow)
"""

import argparse
import sys
from pathlib import Path

try:
    from imgui_bundle import imgui
except ImportError:
    print("window_layout_model: imgui-bundle is not installed (pip install imgui-bundle)")
    sys.exit(1)

# Line length in characters of the text itself (concept 03, "Hilfsfenster"): 50-75 (Ruder, Baymard), 65 (Windows UX
# Guide), at most 80 (WCAG 2.2, 1.4.8).
SHORTEST_LINE = 50
LINE_MEASURE = 65
LONGEST_SINGLE_LINE = 75

# RebornMaterial in the tight density (M3Style.Push, M3Card, M3Widgets at 0bd2517), scale 1.
WINDOW_PADDING = (6.0, 6.0)
FRAME_PADDING = (6.0, 3.0)
ITEM_SPACING = (5.0, 4.0)
SCROLLBAR = 10.0
CARD_INSET = 10.0
CARD_RIGHT = 10.0
CARD_TOP = 8.0
CARD_BOTTOM = 8.0
CARD_MARGIN = 4.0
CARD_MIN_WIDTH = 64.0
CARD_MIN_HEIGHT = 40.0
BUTTON_HEIGHT = 40.0
BUTTON_SIDE = 24.0
BUTTON_MIN_WIDTH = 64.0
# Colors for the pictures only (RebornMaterial's scheme for its default seed, 0bd2517).
CARD_FILL = 0xFF1E1A1A
CHIP_FILL = 0xFF2E5A1F
CHIP_TEXT = 0xFFB9F3A8
MUTED = 0xFFC9C4CC
BODY_PX = 16.0
TITLE_PX = 19.0
ICON = 16.0

FONT_CANDIDATES = [
    "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
    "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
    "/usr/share/fonts/truetype/noto/NotoSans-Regular.ttf",
]


class Context:
    """One Dear ImGui context without a renderer: textures are acknowledged, nothing is drawn to a screen."""

    def __init__(self, width=1920.0, height=1080.0, text_scale=1.0):
        self.ctx = imgui.create_context()
        imgui.set_current_context(self.ctx)
        io = imgui.get_io()
        io.display_size = imgui.ImVec2(width, height)
        io.delta_time = 1.0 / 60.0
        io.backend_flags |= imgui.BackendFlags_.renderer_has_textures.value
        io.backend_flags |= imgui.BackendFlags_.has_mouse_cursors.value
        io.set_ini_filename("")
        font_path = next((p for p in FONT_CANDIDATES if Path(p).exists()), None)
        self.font = io.fonts.add_font_from_file_ttf(font_path, BODY_PX) if font_path else io.fonts.add_font_default()
        self.text_scale = text_scale
        self.mouse = (0.0, 0.0)
        self.down = False
        self.last_draw_data = None

    def frame(self, windows):
        io = imgui.get_io()
        io.add_mouse_pos_event(*self.mouse)
        io.add_mouse_button_event(0, self.down)
        imgui.new_frame()
        imgui.push_font(self.font, BODY_PX * self.text_scale)
        for window in windows:
            window.run(self)
        imgui.pop_font()
        imgui.render()
        data = imgui.get_draw_data()
        for tex in data.textures:
            if tex.status != imgui.ImTextureStatus.ok:
                if tex.status == imgui.ImTextureStatus.want_create:
                    tex.set_tex_id(1)
                tex.set_status(imgui.ImTextureStatus.ok)
        self.last_draw_data = data

    def frames(self, windows, count):
        for _ in range(count):
            self.frame(windows)

    def drag(self, windows, start, end, steps=8):
        """Press at start, move to end in steps, release - the player dragging an edge."""
        self.mouse, self.down = start, False
        self.frames(windows, 2)
        self.down = True
        self.frames(windows, 2)
        for i in range(1, steps + 1):
            self.mouse = (start[0] + (end[0] - start[0]) * i / steps, start[1] + (end[1] - start[1]) * i / steps)
            self.frame(windows)
        self.down = False
        self.frames(windows, 3)
        self.mouse = (0.0, 0.0)
        self.frames(windows, 3)


class Recommendation:
    """RecommendationWindow: the frame (sizing and measuring) the six windows share."""

    def __init__(self, name, content, legacy=False, no_minimum=False, no_follow=False, no_replace=False):
        self.name = name
        self.content = content
        self.legacy = legacy
        self.no_minimum = no_minimum
        self.no_follow = no_follow
        self.no_replace = no_replace
        # measured while drawing (window coordinates, px)
        self.min_width = 0.0
        self.start_width = 0.0
        self.height = 0.0
        self.measured = False
        # applied before the next frame
        self.sized = False
        self.glued = True
        self.size = (0.0, 0.0)
        self.top = 0.0
        self.screen_width = 0.0
        self.screen_bottom = 0.0
        self.fit = 0.0
        # card state
        self.in_card = False
        self.card_has_item = False
        self.last_was_line = False
        # what the checks read
        self.scale = 1.0
        self.extents = []  # right edges (window coordinates) of everything that must stay visible
        self.lines = []  # every text line as Dear ImGui wraps it
        self.cut = []  # what lies past the visible edge
        self.pos = (0.0, 0.0)
        self.scroll = 0.0

    # ---- PreDraw: the theme, then the size from the last frame's measure (Dalamud applies Size and constraints next)
    def pre_draw(self):
        imgui.push_style_var(imgui.StyleVar_.window_padding, imgui.ImVec2(*WINDOW_PADDING))
        imgui.push_style_var(imgui.StyleVar_.frame_padding, imgui.ImVec2(*FRAME_PADDING))
        imgui.push_style_var(imgui.StyleVar_.item_spacing, imgui.ImVec2(*ITEM_SPACING))
        imgui.push_style_var(imgui.StyleVar_.scrollbar_size, SCROLLBAR)
        if self.legacy or not self.measured:
            return
        min_width = min(self.min_width, self.screen_width)
        start_width = max(min_width, min(self.start_width, self.screen_width))
        self.fit = max(min(self.height, self.screen_bottom - self.top), imgui.get_style().window_min_size.y)
        imgui.set_next_window_size_constraints(
            imgui.ImVec2(0.0 if self.no_minimum else min_width, 0.0),
            imgui.ImVec2(sys.float_info.max, self.fit))
        if not self.sized:
            self.sized = True
            if self.size[0] < min_width and not self.no_replace:
                imgui.set_next_window_size(imgui.ImVec2(start_width, self.fit), imgui.Cond_.always.value)
                return
        if self.glued and abs(self.size[1] - self.fit) >= 1.0 and not self.no_follow:
            imgui.set_next_window_size(imgui.ImVec2(self.size[0], self.fit), imgui.Cond_.always.value)

    def run(self, ctx):
        self.scale = ctx.text_scale
        self.pre_draw()
        flags = imgui.WindowFlags_.no_collapse.value
        if self.legacy:
            flags |= imgui.WindowFlags_.always_auto_resize.value
        imgui.begin(self.name, None, flags)
        self.draw()
        imgui.end()
        imgui.pop_style_var(4)

    # ---- Draw: the content, then the height it needs at this width
    def draw(self):
        self.min_width = 0.0
        self.start_width = 0.0
        self.in_card = False
        self.extents = []
        self.lines = []
        self.cut = []
        self.content(self)
        style = imgui.get_style()
        self.height = float(int(imgui.get_cursor_pos_y() - style.item_spacing.y + style.window_padding.y + 0.999))
        size = imgui.get_window_size()
        if self.measured:
            self.glued = size.y >= self.fit - 1.0
        self.size = (size.x, size.y)
        self.top = imgui.get_window_pos().y
        viewport = imgui.get_window_viewport()
        self.screen_width = viewport.work_size.x
        self.screen_bottom = viewport.work_pos.y + viewport.work_size.y
        self.pos = (imgui.get_window_pos().x, imgui.get_window_pos().y)
        self.scroll = imgui.get_scroll_max_y()
        edge = size.x - style.window_padding.x - (style.scrollbar_size if self.scroll > 0.0 else 0.0) + 0.5
        self.cut += [e for e in self.extents if e > edge]
        self.measured = True

    # ---- the helpers of RecommendationWindow
    def inset_right(self):
        style = imgui.get_style()
        right = style.window_padding.x + (CARD_RIGHT if self.in_card else 0.0)
        if imgui.get_scroll_max_y() > 0.0:
            right += style.scrollbar_size
        return right

    def extent(self):
        right = imgui.get_item_rect_max().x - imgui.get_window_pos().x
        self.extents.append(right)
        if self.legacy:
            return
        self.min_width = max(self.min_width, right + self.inset_right())
        self.start_width = max(self.start_width, right + self.inset_right())

    def text(self, text):
        start = imgui.get_cursor_pos_x()
        if not self.legacy:
            shortest = readable_column(text, SHORTEST_LINE, SHORTEST_LINE)
            opening = readable_column(text)
            self.min_width = max(self.min_width, start + shortest + self.inset_right())
            self.start_width = max(self.start_width, start + opening + self.inset_right())
        wraps = self.in_card or not self.legacy
        wrap = imgui.get_content_region_avail().x - (CARD_RIGHT if self.in_card else 0.0) if wraps else 1e9
        self.record_lines(text, max(1.0, wrap))
        if wraps and not self.in_card:
            imgui.push_text_wrap_pos(0.0)
        imgui.text_unformatted(text)
        if wraps and not self.in_card:
            imgui.pop_text_wrap_pos()

    def record_lines(self, text, wrap):
        font = imgui.get_font()
        size = imgui.get_font_size()
        rest = text
        while rest:
            cut = font.calc_word_wrap_position_python(size, rest, wrap)
            piece = rest[:cut].rstrip()
            if not piece:
                piece, cut = rest[:1], 1
            self.lines.append(piece)
            if imgui.calc_text_size(piece).x > wrap + 0.5:
                self.cut.append(piece)
            rest = rest[cut:].lstrip()

    def gap(self):
        if self.legacy:
            return
        imgui.set_cursor_pos_y(imgui.get_cursor_pos_y()
                               + max(0.0, imgui.get_text_line_height() - imgui.get_style().item_spacing.y))

    def heading(self, title, icon=True):
        if self.legacy:
            at = imgui.get_cursor_screen_pos()
            imgui.get_window_draw_list().add_text(imgui.ImVec2(at.x + ICON + 10.0, at.y), 0xFFFFFFFF, title)
            imgui.set_cursor_screen_pos(imgui.ImVec2(at.x, at.y + imgui.get_text_line_height() + 6.0))
            return
        imgui.push_font(None, TITLE_PX * self.scale)
        if icon:
            imgui.dummy(imgui.ImVec2(ICON * self.scale, ICON * self.scale))
            imgui.same_line()
        imgui.text_unformatted(title)
        self.extent()
        imgui.pop_font()

    def section(self, title, subtitle=None):
        card = Card(self)
        self.in_card = True
        self.heading(title)
        if subtitle:
            imgui.push_style_color(imgui.Col_.text, imgui.color_convert_u32_to_float4(MUTED))
            self.text(subtitle)
            imgui.pop_style_color()
        self.card_has_item = False
        return card

    def chip(self, label):
        pad = imgui.get_style().frame_padding
        if not self.legacy:
            imgui.align_text_to_frame_padding()
        size = imgui.calc_text_size(label)
        imgui.dummy(imgui.ImVec2(size.x + pad.x + pad.x, size.y + pad.y + pad.y))
        draw = imgui.get_window_draw_list()
        draw.add_rect_filled(imgui.get_item_rect_min(), imgui.get_item_rect_max(), CHIP_FILL, 999.0)
        at = imgui.get_item_rect_min()
        draw.add_text(imgui.ImVec2(at.x + pad.x, at.y + pad.y), CHIP_TEXT, label)
        self.extent()
        imgui.same_line()

    def line(self, text, chip=None):
        if self.in_card and self.card_has_item:
            self.gap()
        if chip:
            self.chip(chip)
        self.text(text)
        self.card_has_item = self.in_card
        self.last_was_line = True

    def note(self, text):
        if not text:
            return
        if self.in_card and self.card_has_item and not self.last_was_line:
            self.gap()
        imgui.push_style_color(imgui.Col_.text, imgui.color_convert_u32_to_float4(MUTED))
        self.text(text)
        imgui.pop_style_color()
        self.card_has_item = self.in_card
        self.last_was_line = False

    def action(self, label):
        width = max((BUTTON_SIDE * 2 + ICON) * self.scale + imgui.calc_text_size(label).x, BUTTON_MIN_WIDTH * self.scale)
        imgui.button(label, imgui.ImVec2(width, max(BUTTON_HEIGHT * self.scale, imgui.get_frame_height())))
        self.extent()
        self.last_was_line = False


class Card:
    """M3Card.Begin/Scope.Dispose reduced to their layout; the card has no title of its own (the window draws it)."""

    heights = {}

    def __init__(self, window):
        self.window = window
        self.min = imgui.get_cursor_screen_pos()
        self.width = max(CARD_MIN_WIDTH, imgui.get_content_region_avail().x)
        self.key = (window.name, round(self.min.y - imgui.get_window_pos().y + imgui.get_scroll_y()))
        previous = Card.heights.get(self.key, 0.0)
        if previous > 0.0:
            imgui.get_window_draw_list().add_rect_filled(
                self.min, imgui.ImVec2(self.min.x + self.width, self.min.y + previous), CARD_FILL, 12.0)
        imgui.indent(CARD_INSET)
        right = self.min.x + self.width - CARD_RIGHT - imgui.get_window_pos().x + imgui.get_scroll_x()
        imgui.push_text_wrap_pos(right)
        imgui.set_cursor_screen_pos(imgui.ImVec2(self.min.x + CARD_INSET, self.min.y + CARD_TOP))

    def __enter__(self):
        return self

    def __exit__(self, *exc):
        bottom_content = imgui.get_cursor_screen_pos().y
        imgui.pop_text_wrap_pos()
        imgui.unindent(CARD_INSET)
        bottom = max(bottom_content + CARD_BOTTOM, self.min.y + CARD_MIN_HEIGHT)
        Card.heights[self.key] = bottom - self.min.y
        imgui.set_cursor_screen_pos(imgui.ImVec2(self.min.x, bottom + CARD_MARGIN))
        imgui.dummy(imgui.ImVec2(self.width, 0.0))
        self.window.in_card = False
        self.window.card_has_item = False
        return False


# ---- the content: the Crucible team window with texts of the length RSR writes (BeastmasterCrucibleTeam.cs)
PICKS = [
    ("Lynx", "in team", "heals or protects; hits Fire (2), Ice (1); levels: rank 3 in place of Bomb (rank 1)"),
    ("Raptor", "joins", "hits Wind (3) - the board's most common weakness"),
    ("Morbol", "joins", "heals or protects; no weakness of this board"),
]


def team_content(picks, status="Team set: 3 familiars. Confirm it on the screen."):
    """The status line is empty until the player presses Apply - the state in which the window first appears."""
    def content(w):
        with w.section("Team for this board", f"{len(picks)} familiars"):
            for pet, chip, why in picks:
                w.line(pet, chip)
                w.note(why)
            w.line("Bomb", "leaves")
        w.action("Apply")
        w.note(status)
    return content


# ---- ReadableText and the tooltip (ImguiTooltips.ShowTooltip)
def readable_column(text, measure=LINE_MEASURE, single=LONGEST_SINGLE_LINE):
    """ReadableText.Column: per paragraph its whole width up to `single` characters, else `measure` of them."""
    widest = 0.0
    for paragraph in text.split("\n"):
        if paragraph:
            width = imgui.calc_text_size(paragraph).x
            widest = max(widest, width if len(paragraph) <= single else width * measure / len(paragraph))
    return widest


DESCRIPTION = " ".join(["As a tank: against a tankbuster the table has measured, RSR uses the mitigation that covers it"] * 23)


class Tooltip:
    """ImguiTooltips.ShowTooltip with ReadableText.Draw (new) or ImGui.Text (old)."""

    def __init__(self, text, old=False, bullet=False):
        self.text, self.old, self.bullet = text, old, bullet
        self.size = (0.0, 0.0)
        self.lines, self.cut = [], []

    def run(self, ctx):
        imgui.set_next_window_size_constraints(imgui.ImVec2(150, 0), imgui.ImVec2(1200, 1500))
        imgui.set_next_window_pos(imgui.ImVec2(10, 10))
        flags = (imgui.WindowFlags_.no_decoration.value | imgui.WindowFlags_.always_auto_resize.value
                 | imgui.WindowFlags_.no_saved_settings.value | imgui.WindowFlags_.no_inputs.value)
        imgui.begin("tooltip", None, flags)
        if self.bullet:
            imgui.bullet()
            imgui.same_line()
        start = imgui.get_cursor_pos_x()
        if self.old:
            imgui.text_unformatted(self.text)
            wrap = 1e9
        else:
            wrap = readable_column(self.text)
            imgui.push_text_wrap_pos(start + wrap)
            imgui.text_unformatted(self.text)
            imgui.pop_text_wrap_pos()
        size = imgui.get_window_size()
        self.size = (size.x, size.y)
        visible = size.x - imgui.get_style().window_padding.x - start
        self.lines, self.cut = [], []
        font, px = imgui.get_font(), imgui.get_font_size()
        for paragraph in self.text.split("\n"):
            rest = paragraph
            while rest:
                cut = font.calc_word_wrap_position_python(px, rest, wrap)
                piece = rest[:cut].rstrip() or rest[:1]
                self.lines.append(piece)
                if imgui.calc_text_size(piece).x > visible + 0.5:
                    self.cut.append(piece)
                rest = rest[max(cut, 1):].lstrip()
        imgui.end()


def scenario_tooltip(failures):
    print("tooltip with a long option description")
    for label, old, expect_ok in (("old (ImGui.Text, BulletText)", True, False), ("new (ReadableText.Draw)", False, True)):
        for bullet in (False, True):
            ctx = Context()
            tip = Tooltip(DESCRIPTION + "\nRight-click for the matching chat command.", old=old, bullet=bullet)
            ctx.frames([tip], 4)
            longest = max(len(x) for x in tip.lines)
            print(f"  {label}{' with bullet' if bullet else ''}: {tip.size[0]:.0f} x {tip.size[1]:.0f} px, longest line"
                  f" {longest} characters, {len(tip.cut)} line(s) cut off")
            if expect_ok:
                check("tooltip shows all of the description", not tip.cut, "", failures)
                check("tooltip lines at most 75 characters", longest <= LONGEST_SINGLE_LINE, "", failures)
                check("tooltip lines reach the measure", longest >= SHORTEST_LINE, "", failures)
            else:
                check("the old tooltip cuts the description off (self-test of the checks)", bool(tip.cut), "", failures)


def check(name, ok, detail, failures):
    print(("  ok   " if ok else "  FAIL ") + name + (f" - {detail}" if detail else ""))
    if not ok:
        failures.append(name)


def measure(window, ctx):
    """What the player sees: size, the longest text line, anything cut off, a scrollbar."""
    ctx.frame([window])
    longest = max((len(x) for x in window.lines), default=0)
    return {"width": window.size[0], "height": window.size[1], "need": window.height, "longest": longest,
            "lines": list(window.lines), "cut": list(window.cut), "scroll": window.scroll}


def scenario_old(failures):
    print("old construction (AlwaysAutoResize, A633): must show the defect")
    ctx = Context()
    window = Recommendation("old", team_content(PICKS, status=""), legacy=True)
    ctx.frames([window], 6)
    m = measure(window, ctx)
    check("the old window is narrow (self-test of the checks)", m["width"] < 150, f"{m['width']:.0f} x {m['height']:.0f} px", failures)
    check("its lines are far below 50 characters", m["longest"] < SHORTEST_LINE, f"longest line {m['longest']} characters", failures)
    return m


def checks_open(window, ctx, failures, label):
    m = measure(window, ctx)
    print(f"  {label}: {m['width']:.0f} x {m['height']:.0f} px, content {m['need']:.0f} px, longest line {m['longest']} characters")
    check("nothing cut off", not m["cut"], f"{len(m['cut'])} item(s) past the edge", failures)
    check("no line over 75 characters", m["longest"] <= LONGEST_SINGLE_LINE, "", failures)
    check("long texts reach the measure", m["longest"] >= SHORTEST_LINE, "", failures)
    return m


def scenario_fresh(failures, **defect):
    print("first opening, no saved size")
    ctx = Context()
    window = Recommendation("team", team_content(PICKS), **defect)
    ctx.frames([window], 8)
    m = checks_open(window, ctx, failures, "opens at")
    check("height is the content's", abs(m["height"] - m["need"]) <= 1.0, "", failures)
    check("no scrollbar", m["scroll"] <= 0.0, "", failures)
    return ctx, window, m


def scenario_saved_old_width(failures, **defect):
    print("saved size from the old auto-size mode, then the update")
    fresh = Context()
    first = Recommendation("team", team_content(PICKS))
    fresh.frames([first], 8)
    opening = first.size[0]
    ctx = Context()
    old = Recommendation("team", team_content(PICKS, status=""), legacy=True)
    ctx.frames([old], 6)
    print(f"  saved by the old mode: {old.size[0]:.0f} x {old.size[1]:.0f} px")
    window = Recommendation("team", team_content(PICKS), **defect)
    ctx.frames([window], 8)
    m = checks_open(window, ctx, failures, "after the update")
    check("the old width is replaced by the start width", abs(m["width"] - opening) <= 1.0,
          f"{m['width']:.0f} vs {opening:.0f} px", failures)
    check("height is the content's", abs(m["height"] - m["need"]) <= 1.0, "", failures)


def scenario_resize(failures, **defect):
    print("the player resizes")
    ctx, window, m0 = scenario_fresh([], **defect)
    x0, y0 = window.pos
    w0, h0 = window.size
    # drag the bottom edge up by a third
    ctx.drag([window], (x0 + w0 / 2, y0 + h0 - 1), (x0 + w0 / 2, y0 + h0 * 2 / 3))
    m1 = measure(window, ctx)
    check("height can be made shorter", m1["height"] < h0 - 10, f"{h0:.0f} -> {m1['height']:.0f} px", failures)
    check("shorter window scrolls instead of cutting", m1["scroll"] > 0.0, "", failures)
    # content grows while shortened: the window keeps the player's height
    window.content = team_content(PICKS + [("Gnath", "joins", "hits Earth (2); levels: rank 2 in place of Lynx (rank 1)")])
    ctx.frames([window], 4)
    m2 = measure(window, ctx)
    check("a shortened window keeps its height when content grows", abs(m2["height"] - m1["height"]) <= 1.0, "", failures)
    # drag the bottom edge far down: stops at the content
    ctx.drag([window], (x0 + w0 / 2, y0 + m2["height"] - 1), (x0 + w0 / 2, y0 + m2["height"] + 400))
    m3 = measure(window, ctx)
    check("height cannot exceed the content", m3["height"] <= m3["need"] + 1.0, f"{m3['height']:.0f} vs {m3['need']:.0f}", failures)
    check("dragged back to the content, it follows again", abs(m3["height"] - m3["need"]) <= 1.0, "", failures)
    window.content = team_content(PICKS)
    ctx.frames([window], 4)
    m4 = measure(window, ctx)
    check("content shrinks: height follows", abs(m4["height"] - m4["need"]) <= 1.0, f"{m4['height']:.0f} vs {m4['need']:.0f}", failures)
    window.content = team_content(PICKS + [("Gnath", "joins", "hits Earth (2)")])
    ctx.frames([window], 4)
    m5 = measure(window, ctx)
    check("content grows: height follows", abs(m5["height"] - m5["need"]) <= 1.0 and m5["height"] > m4["height"],
          f"{m4['height']:.0f} -> {m5['height']:.0f} px", failures)
    # drag the right edge wider, then far narrower
    w = window.size[0]
    ctx.drag([window], (x0 + w - 1, y0 + 60), (x0 + w + 200, y0 + 60))
    m6 = measure(window, ctx)
    check("width can be made wider", m6["width"] > w + 100, f"{w:.0f} -> {m6['width']:.0f} px", failures)
    check("wider: height follows the reflowed text", abs(m6["height"] - m6["need"]) <= 1.0, "", failures)
    ctx.drag([window], (x0 + m6["width"] - 1, y0 + 60), (x0 + 40, y0 + 60))
    m7 = checks_open(window, ctx, failures, "dragged narrow")
    check("narrowing stops at 50 characters", m7["longest"] >= SHORTEST_LINE - 6, f"longest line {m7['longest']}", failures)
    check("narrow: height follows", abs(m7["height"] - m7["need"]) <= 1.0, "", failures)


def scenario_scale_and_screen(failures):
    print("twice the text size")
    ctx = Context(text_scale=2.0)
    window = Recommendation("team", team_content(PICKS))
    ctx.frames([window], 8)
    checks_open(window, ctx, failures, "at 200 %")
    print("small screen (800 x 400)")
    ctx = Context(width=800.0, height=400.0, text_scale=2.0)
    window = Recommendation("team", team_content(PICKS * 3))
    ctx.frames([window], 8)
    m = measure(window, ctx)
    print(f"  {m['width']:.0f} x {m['height']:.0f} px")
    check("width within the screen", m["width"] <= 800.0 + 0.5, "", failures)
    check("bottom within the screen", window.pos[1] + m["height"] <= 400.0 + 1.0, "", failures)
    check("content that does not fit scrolls", m["scroll"] > 0.0, "", failures)


def self_test():
    print("self-test: constructed defects must each fail a check")
    ok = True
    for defect, scenario in (("no_minimum", scenario_resize), ("no_follow", scenario_resize),
                             ("no_replace", scenario_saved_old_width)):
        failures = []
        print(f"- {defect}")
        scenario(failures, **{defect: True})
        caught = bool(failures)
        print(f"  {'caught' if caught else 'NOT CAUGHT'}: {defect}")
        ok = ok and caught
    return ok


def render_png(ctx, path):
    """Rasterize the last frame's draw data (triangles, font atlas) to a PNG - a picture for a person to look at."""
    import numpy as np
    from PIL import Image
    data = ctx.last_draw_data
    w, h = int(data.display_size.x), int(data.display_size.y)
    canvas = np.zeros((h, w, 3), dtype=np.float32)
    canvas[:] = (0.07, 0.07, 0.08)
    tex = data.textures[0]
    pixels = np.array(tex.get_pixels_array(), dtype=np.float32).reshape(tex.height, tex.width, -1) / 255.0
    alpha = pixels[..., -1]
    for dl in data.cmd_lists:
        vtx = [(v.pos.x, v.pos.y, v.uv.x, v.uv.y, v.col) for v in dl.vtx_buffer]
        idx = list(dl.idx_buffer)
        for cmd in dl.cmd_buffer:
            cx0, cy0, cx1, cy1 = (int(cmd.clip_rect.x), int(cmd.clip_rect.y), int(cmd.clip_rect.z), int(cmd.clip_rect.w))
            for t in range(cmd.idx_offset, cmd.idx_offset + cmd.elem_count, 3):
                a, b, c = (vtx[cmd.vtx_offset + idx[t + k]] for k in range(3))
                x0 = max(int(min(a[0], b[0], c[0])), cx0, 0)
                x1 = min(int(max(a[0], b[0], c[0])) + 1, cx1, w)
                y0 = max(int(min(a[1], b[1], c[1])), cy0, 0)
                y1 = min(int(max(a[1], b[1], c[1])) + 1, cy1, h)
                if x0 >= x1 or y0 >= y1:
                    continue
                ys, xs = np.mgrid[y0:y1, x0:x1].astype(np.float32) + 0.5
                det = (b[1] - c[1]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[1] - c[1])
                if abs(det) < 1e-9:
                    continue
                l1 = ((b[1] - c[1]) * (xs - c[0]) + (c[0] - b[0]) * (ys - c[1])) / det
                l2 = ((c[1] - a[1]) * (xs - c[0]) + (a[0] - c[0]) * (ys - c[1])) / det
                l3 = 1.0 - l1 - l2
                inside = (l1 >= -1e-4) & (l2 >= -1e-4) & (l3 >= -1e-4)
                if not inside.any():
                    continue
                u = l1 * a[2] + l2 * b[2] + l3 * c[2]
                v = l1 * a[3] + l2 * b[3] + l3 * c[3]
                tu = np.clip((u * tex.width).astype(int), 0, tex.width - 1)
                tv = np.clip((v * tex.height).astype(int), 0, tex.height - 1)
                col = np.array([((a[4] >> s) & 0xFF) / 255.0 for s in (0, 8, 16, 24)], dtype=np.float32)
                cov = alpha[tv, tu] * col[3] * inside
                region = canvas[y0:y1, x0:x1]
                region[:] = region * (1 - cov[..., None]) + col[:3] * cov[..., None]
    Image.fromarray((np.clip(canvas, 0, 1) * 255).astype(np.uint8)).save(path)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--png", help="directory for pictures of the old and the new window")
    args = parser.parse_args()
    failures = []
    scenario_old(failures)
    ctx_old = Context(width=900, height=700)
    old = Recommendation("RSR - Crucible team (old)", team_content(PICKS, status=""), legacy=True)
    ctx_old.frames([old], 6)
    ctx, _, _ = scenario_fresh(failures)
    scenario_saved_old_width(failures)
    scenario_resize(failures)
    scenario_scale_and_screen(failures)
    scenario_tooltip(failures)
    caught = self_test()
    if args.png:
        out = Path(args.png)
        out.mkdir(parents=True, exist_ok=True)
        render_png(ctx_old, out / "old.png")
        ctx_new = Context(width=900, height=700)
        new = Recommendation("RSR - Crucible team", team_content(PICKS))
        ctx_new.frames([new], 8)
        render_png(ctx_new, out / "new.png")
        print(f"pictures: {out / 'old.png'}, {out / 'new.png'}")
    if failures or not caught:
        print(f"window_layout_model: FAILED ({', '.join(failures) or 'self-test'})")
        return 1
    print("window_layout_model: all checks hold")
    return 0


if __name__ == "__main__":
    sys.exit(main())
