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
    """(file, stripped line) for every C# line the fork added since `base`."""
    diff = run("git", "diff", "-U0", f"{base}...HEAD", "--", "*.cs")
    if diff is None:
        return None
    result = []
    current = None
    for line in diff.splitlines():
        if line.startswith("+++ "):
            path = line[6:].strip() if line.startswith("+++ b/") else None
            current = path if path and not path.endswith(".g.cs") else None
            continue
        if current and line.startswith("+") and not line.startswith("+++"):
            result.append((current, line[1:].strip()))
    return result


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
