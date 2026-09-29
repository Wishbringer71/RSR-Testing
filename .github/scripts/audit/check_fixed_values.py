#!/usr/bin/env python3
"""No fixed value enters the fork's C# without a loop that asked whether the game can supply it.

The owner's rule: "ich will generell keine festen werte im code haben. alles muss ingame ableitbar
sein. bevor eine ausnahme entsteht muss vorab ein vollständiger loop zum jeweiligen wert entstehen
mit recherce, ob man ihn nicht doch ingame ableiten kann."

A number written into the code is a claim about the game that nothing re-checks: a duration a patch
changes, a level a sync lowers, a GCD count that assumed one speed. The game states most of them
itself - action data, effect texts, the GCD, the animation lock - and where it does, the code is to
read them there.

What this checks: every numeric literal on a C# line the fork added against upstream - the diff from
the merge base with upstream/main to HEAD - must be listed in fixed_values.json, either as an
exception whose loop is recorded in AUDIT_LOG.md, or as an open loop. A new literal that is listed
nowhere fails the build; so does a listed one that no longer exists, because a list that keeps dead
entries stops saying which values are really there.

Not counted: 0 and 1, which are identities rather than claims about the game; literals inside
strings and comments; generated files (*.g.cs), which are derived from the game data by their
generators and checked against it in CI.

WHAT THIS CHECKS, STATED HONESTLY: that a number was looked at, not that the loop was right. The
loop's reasoning lives in AUDIT_LOG.md and has to be read there.

Usage: python3 .github/scripts/audit/check_fixed_values.py [--base <rev>] [--list]
"""

import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
LIST = Path(__file__).with_name("fixed_values.json")

NUMBER = re.compile(r"(?<![\w.])(\d+(?:\.\d+)?)[fFdDmMuUlL]?(?![\w.])")
# Hexadecimal literals: NUMBER stops at the "0" of "0x40", which is followed by a letter, so a bit
# mask passed unseen until it was checked for by hand.
HEX = re.compile(r"(?<![\w.])(0[xX][0-9a-fA-F]+)[uUlL]*(?![\w.])")
STRING = re.compile(r'\$?@?"(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)\'')
TRIVIAL = {"0", "1", "0.0", "1.0"}


def run(*args):
    out = subprocess.run(args, capture_output=True, text=True, cwd=ROOT)
    return out.stdout if out.returncode == 0 else None


def literals(code):
    """The numeric literals a line of C# states, strings and comments removed."""
    stripped = code.strip()
    if stripped.startswith("//") or stripped.startswith("*") or stripped.startswith("#"):
        return []
    code = STRING.sub('""', code)
    code = code.split("//")[0]
    return [n for n in NUMBER.findall(code) + HEX.findall(code) if n not in TRIVIAL]


def added_lines(base):
    """(file, stripped line) for every C# line the fork added since `base`.

    Measured against the working tree, not HEAD: run before a commit, a diff to HEAD sees none of
    the lines about to be committed, and the check reported a clean tree while a new number sat in
    the change. In CI the working tree is HEAD, so nothing changes there. A file not yet known to
    git counts with every line: git diff does not show it at all, and a new file with a new number
    passed here and failed in CI (A159)."""
    diff = run("git", "diff", "-U0", base, "--", "*.cs")
    if diff is None:
        return None
    untracked = run("git", "ls-files", "--others", "--exclude-standard", "--", "*.cs")
    if untracked is None:
        return None
    files = {}
    for path in untracked.splitlines():
        try:
            files[path] = (ROOT / path).read_text(encoding="utf-8").splitlines()
        except OSError:
            files[path] = []
    return collect(diff, files)


def collect(diff, untracked_files):
    """The added lines from a unified diff plus every line of the untracked files.

    A line the diff shows as removed and added again within one hunk is not the fork's: git aligns
    a hunk differently after an edit nearby, and an unchanged upstream line then appears as - and +.
    Counted as added, it failed the check for a number the fork never wrote (29.09.2026, the
    upstream casting-stop line in DataCenter.cs). Within a hunk, each removed line cancels one
    identical added line. Across hunks nothing cancels: an upstream line the fork moved into its own
    logic elsewhere stays counted, and so does its open loop.
    """
    result = []
    current = None
    hunk_added = []
    hunk_removed = {}

    def close_hunk():
        for key in hunk_added:
            if hunk_removed.get(key, 0) > 0:
                hunk_removed[key] -= 1
                continue
            result.append(key)
        hunk_added.clear()
        hunk_removed.clear()

    for line in diff.splitlines():
        if line.startswith("--- "):
            continue
        if line.startswith("+++ ") or line.startswith("@@") or line.startswith("diff "):
            close_hunk()
            if line.startswith("+++ "):
                path = line[6:].strip() if line.startswith("+++ b/") else None
                current = path if path and not path.endswith(".g.cs") else None
            continue
        if current and line.startswith("+"):
            hunk_added.append((current, line[1:].strip()))
        elif current and line.startswith("-"):
            key = (current, line[1:].strip())
            hunk_removed[key] = hunk_removed.get(key, 0) + 1
    close_hunk()
    for path, lines in untracked_files.items():
        if path.endswith(".g.cs"):
            continue
        result.extend((path, line.strip()) for line in lines)
    return result


def drop_upstream_lines(lines, listed_keys, upstream_lines):
    """Leaves out an added line that stands word for word in the upstream version of its file, unless
    it is listed already: an upstream line the fork moved into its own logic keeps its open loop."""
    return [(p, c) for p, c in lines if (p, c) in listed_keys or c not in upstream_lines(p)]


def findings(lines):
    found = {}
    for path, code in lines:
        values = literals(code)
        if values:
            found[(path, code)] = values
    return found


def self_test():
    cases = [
        ("var x = SummonTime <= WeaponRemain + 2.5f;", ["2.5"]),
        ("if (count > 1 && share < 0) return;", []),
        ('ImGui.Text($"held {seconds:F1} s at 30");', []),
        ("return value; // waits 30 s", []),
        ("/// Duration: 30s", []),
        ("private const int SearingPhaseHeldAfter = 2;", ["2"]),
        ("var id = SummonBahamutPvE2;", []),
        ("new int[Enum.GetValues<SearingPhase>().Length]", []),
        ("BMRShouldRefreshBefore(BMRRaidwideIn, 30f, true)", ["30"]),
        ("=> (entry.flags & 0x40) != 0 ? entry.Damage : entry.value;", ["0x40"]),
    ]
    for code, expected in cases:
        got = literals(code)
        if got != expected:
            return f"{code!r} read as {got}, expected {expected}"

    # A file git does not know yet has no diff; its lines must still be seen.
    lines = collect("+++ b/A.cs\n+var a = 2.5f;\n", {"B.cs": ["var b = 30f;"], "C.g.cs": ["var c = 7;"]})
    if ("B.cs", "var b = 30f;") not in lines or any(p == "C.g.cs" for p, _ in lines) \
            or ("A.cs", "var a = 2.5f;") not in lines:
        return f"untracked files were not collected as added lines: {lines}"
    # An unchanged line that git shows as removed and added again in one hunk is not an added line;
    # the same line moved to another hunk still is.
    lines = collect("+++ b/A.cs\n@@ -1 +1,2 @@\n-var a = 100;\n+var b = 2;\n+var a = 100;\n", {})
    if ("A.cs", "var a = 100;") in lines or ("A.cs", "var b = 2;") not in lines:
        return f"a realigned unchanged line was counted as added, or a new one was lost: {lines}"
    lines = collect("+++ b/A.cs\n@@ -1 +0,0 @@\n-var a = 100;\n@@ -9,0 +9 @@\n+var a = 100;\n", {})
    if ("A.cs", "var a = 100;") not in lines:
        return f"a line moved to another hunk was not counted as added: {lines}"
    # A line upstream already has is upstream's, wherever git puts it - unless it is listed.
    kept = drop_upstream_lines([("A.cs", "var a = 100;"), ("A.cs", "var b = 2;"), ("A.cs", "var c = 3;")],
                               {("A.cs", "var c = 3;")},
                               lambda path: {"var a = 100;", "var c = 3;"})
    if kept != [("A.cs", "var b = 2;"), ("A.cs", "var c = 3;")]:
        return f"upstream lines were not told apart from the fork's: {kept}"
    return None


def main(argv):
    failure = self_test()
    if failure:
        print(f"self-test failed: {failure}")
        return 1
    print("self-test ok: numbers in code are found, identities, strings, comments and identifiers "
          "are left out.")

    base = argv[argv.index("--base") + 1] if "--base" in argv else None
    if base is None:
        base = (run("git", "merge-base", "upstream/main", "HEAD") or "").strip()
    if not base:
        print("No merge base with upstream/main - fetch upstream main before this check "
              "(git fetch upstream main). Nothing was checked, and saying so is the result.")
        return 1

    lines = added_lines(base)
    if lines is None:
        print(f"git diff against {base} failed.")
        return 1

    # A line that stands word for word in the upstream version of the same file is upstream's, however
    # git aligns the diff after an edit nearby - unless it is already listed: an upstream line the fork
    # moved into its own logic keeps its open loop (29.09.2026, the casting-stop line in DataCenter.cs,
    # which a hunk realignment kept presenting as added).
    listed_keys = {(e["file"], e["code"])
                   for e in json.loads(LIST.read_text(encoding="utf-8"))["entries"]}
    base_lines = {}

    def upstream_lines(path):
        if path not in base_lines:
            text = run("git", "show", f"{base}:{path}")
            base_lines[path] = {l.strip() for l in text.splitlines()} if text is not None else set()
        return base_lines[path]

    lines = drop_upstream_lines(lines, listed_keys, upstream_lines)
    found = findings(lines)

    if "--list" in argv:
        for (path, code), values in sorted(found.items()):
            print(f"{path}: {', '.join(values)}  |  {code}")
        print(f"{len(found)} line(s)")
        return 0

    listed = json.loads(LIST.read_text(encoding="utf-8"))["entries"]
    keys = {(e["file"], e["code"]) for e in listed}
    missing = [(k, v) for k, v in found.items() if k not in keys]
    stale = [e for e in listed if (e["file"], e["code"]) not in found]
    bad = [e for e in listed
           if e.get("status") not in ("exception", "open")
           or (e.get("status") == "exception" and not e.get("loop"))]

    for (path, code), values in missing:
        print(f"NOT LISTED  {path}: {', '.join(values)}  |  {code}")
    for e in stale:
        print(f"STALE       {e['file']}  |  {e['code']}")
    for e in bad:
        print(f"INCOMPLETE  {e['file']}  |  {e['code']}  - an exception names its loop, "
              "anything else is 'open'")

    open_count = sum(1 for e in listed if e.get("status") == "open")
    exceptions = sum(1 for e in listed if e.get("status") == "exception")
    if missing or stale or bad:
        print("A fixed value needs a loop before it enters the code: derive it from the game, or "
              "record why that is impossible in AUDIT_LOG.md and list it as an exception.")
        return 1
    print(f"{len(found)} line(s) with fixed values: {exceptions} exception(s) with a recorded loop, "
          f"{open_count} still waiting for one.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
