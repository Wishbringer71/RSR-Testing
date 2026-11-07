#!/usr/bin/env python3
"""No window sizes itself to content that wraps at its own edge (concept 03, "Hilfsfenster", A654).

ImGuiWindowFlags.AlwaysAutoResize makes a window as wide as its content; text that wraps at the window's or a card's edge
is as wide as the window. Together the content never asks for more width than it has: the recommendation windows of
A633 opened 116 px wide in Dear ImGui, their titles cut off, and could not be resized. The build cannot see this - it compiles.

What this checks: every C# file under RotationSolver/ that names AlwaysAutoResize must not also wrap text relative to
the window - ImGui.TextWrapped, PushTextWrapPos() or PushTextWrapPos(0), an M3Card (which wraps at its right inset),
M3Text with wrap: true, or a rotation's DisplayRotationStatus (TextWrapped in its base). Wrapping at a computed
position - a measure of characters from the cursor - is not relative to the window and passes. A file that combines both
on purpose is listed in ALLOWED with the reason it still lays out. A tooltip is such a window too (ImguiTooltips sizes
it to its content), so the content handed to ImguiTooltips.ShowTooltip(() => ...) is held to the same rule wherever it
is written; ReadableText.Draw breaks at the text's own line length and passes.

WHAT THIS CHECKS, STATED HONESTLY: the pattern in one file, not the layout. A window whose wrapping text sits in another
file escapes it; `window_layout_model.py` measures the layout of the recommendation windows in Dear ImGui.

Usage: python3 .github/scripts/audit/check_window_layout.py
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]

AUTO = re.compile(r"\bAlwaysAutoResize\b")
WRAPS = [
    (re.compile(r"\bImGui\.TextWrapped\s*\("), "ImGui.TextWrapped"),
    (re.compile(r"\bPushTextWrapPos\s*\(\s*(?:0(?:\.0)?f?)?\s*\)"), "PushTextWrapPos at the window edge"),
    (re.compile(r"\bM3Card\.Begin\s*\("), "M3Card (wraps at its right inset)"),
    (re.compile(r"\bM3Text\.(?:Draw|Bullet)\s*\([^;]*\bwrap\s*:\s*true"), "M3Text with wrap: true"),
    (re.compile(r"\bDisplayRotationStatus\s*\("), "a rotation's status (TextWrapped in its base)"),
]

# File -> why it lays out although it combines both.
ALLOWED = {
    "RotationSolver/UI/DiagnosticsWindow.cs":
        "its other lines are drawn unwrapped and always present (the AoE table's lines); they set the width, and the "
        "rotation's wrapped status takes the width they give instead of setting it",
    "RotationSolver/UI/MainWindow/MainWindow.cs":
        "the reset popup sets a minimum width of 380 px x scale before it opens (SetNextWindowSizeConstraints), so its "
        "wrapped text never gets narrower than that; the other TextWrapped in the file is in the resizable main window",
}


def strip_comments(code):
    code = re.sub(r"/\*.*?\*/", "", code, flags=re.S)
    return "\n".join(line.split("//")[0] for line in code.splitlines())


TOOLTIP = re.compile(r"\bImguiTooltips\.ShowTooltip\s*\(\s*\(\)\s*=>")


def tooltip_bodies(code):
    """The lambdas handed to ImguiTooltips.ShowTooltip, up to their closing parenthesis."""
    for match in TOOLTIP.finditer(code):
        depth, i = 1, match.end()
        while i < len(code) and depth:
            depth += {"(": 1, ")": -1}.get(code[i], 0)
            i += 1
        yield code[match.end():i]


def findings(files):
    """(path, what wraps) for every file that sizes itself, or a tooltip, to content wrapping at its own edge."""
    found = []
    for path, text in files:
        code = strip_comments(text)
        scopes = ([code] if AUTO.search(code) else []) + [("tooltip", body) for body in tooltip_bodies(code)]
        for scope in scopes:
            kind, body = scope if isinstance(scope, tuple) else ("window", scope)
            hit = next((what for pattern, what in WRAPS if pattern.search(body)), None)
            if hit:
                found.append((path, hit if kind == "window" else f"{hit} in a tooltip"))
                break
    return found


def self_test():
    cases = [
        ("auto + card", "flags = ImGuiWindowFlags.AlwaysAutoResize; using (M3Card.Begin(\"x\")) {}", True),
        ("auto + wrapped", "ImGuiWindowFlags.AlwaysAutoResize | x; ImGui.TextWrapped(t);", True),
        ("auto + wrap at edge", "ImGuiWindowFlags.AlwaysAutoResize; ImGui.PushTextWrapPos(0f);", True),
        ("auto + wrap no arg", "ImGuiWindowFlags.AlwaysAutoResize; ImGui.PushTextWrapPos();", True),
        ("auto + M3Text wrap", "ImGuiWindowFlags.AlwaysAutoResize; M3Text.Draw(t, M3TextStyle.Body, wrap: true);", True),
        ("auto + status", "ImGuiWindowFlags.AlwaysAutoResize; rotation.DisplayRotationStatus();", True),
        ("auto + measured wrap", "ImGuiWindowFlags.AlwaysAutoResize; ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + measure);", False),
        ("resizable + card", "ImGuiWindowFlags.NoCollapse; using (M3Card.Begin(\"x\")) {}", False),
        ("auto in a comment", "// AlwaysAutoResize\nusing (M3Card.Begin(\"x\")) {}", False),
        ("tooltip wraps at its edge", "ImguiTooltips.ShowTooltip(() =>\n{\n ImGui.Text(a);\n ImGui.TextWrapped(b);\n});", True),
        ("tooltip at the readable length", "ImguiTooltips.ShowTooltip(() =>\n{\n ReadableText.Draw(b);\n});", False),
        ("wrapped text outside a tooltip", "ImguiTooltips.ShowTooltip(() => ImGui.Text(a));\nImGui.TextWrapped(b);", False),
    ]
    ok = True
    for name, code, expected in cases:
        got = bool(findings([("case.cs", code)]))
        if got != expected:
            print(f"self-test FAILED: {name}: expected {'a finding' if expected else 'none'}")
            ok = False
    return ok


def main():
    if not self_test():
        return 1
    print("self-test ok: auto-size and tooltips with edge wrapping are found, measured wrapping and resizable windows pass.")
    files = [(p.relative_to(ROOT).as_posix(), p.read_text(encoding="utf-8", errors="replace"))
             for p in sorted((ROOT / "RotationSolver").rglob("*.cs"))
             if "/obj/" not in p.as_posix() and "/bin/" not in p.as_posix()]
    bad = []
    found = findings(files)
    for path, what in found:
        if path in ALLOWED:
            print(f"allowed   {path}: {what} - {ALLOWED[path]}")
        else:
            bad.append((path, what))
    stale = [path for path in ALLOWED if path not in {f[0] for f in found}]
    for path in stale:
        print(f"STALE     {path}: listed in ALLOWED but no longer combines auto-size and edge wrapping")
    for path, what in bad:
        print(f"AUTO-SIZE {path}: AlwaysAutoResize with {what} - the window never gets wider than its narrowest item")
    if bad or stale:
        print("A window that sizes itself to text wrapping at its own edge opens too narrow and cannot be resized "
              "(concept 03, 'Hilfsfenster'): let the player resize it and size it from the text, as RecommendationWindow does.")
        return 1
    print(f"{len(files)} C# files: no window sizes itself to text that wraps at its own edge.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
