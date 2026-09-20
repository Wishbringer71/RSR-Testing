#!/usr/bin/env python3
"""Check that a target override does not sort by points where the action itself measures in shares.

`TargetType.LowHP` sorts candidates by current health POINTS, `TargetType.LowHPPercent` by their
SHARE of maximum health. Both are legitimate - concept 07 says so: points answer "who does not
survive the next hit", which is the right question for a barrier placed against one incoming blow,
and shares answer "who is furthest from safe", which is the right question for healing.

The measure is wrong in one case that can be decided from the tree alone: when the action's own
effect text states a PERCENTAGE threshold for its follow-up effect, the game evaluates that action
in shares, and a target picked by points can be one the follow-up will never trigger on.

Found this way: Summoner's Rekindle arms its heal-over-time "when HP falls below 75%" and was cast
with targetOverride: TargetType.LowHP. A caster at full health can hold fewer points than a tank at
half, so the sort handed back somebody who needed nothing.

Reports rather than fails, except for the self-test.
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
RESX = ROOT / "RotationSolver.SourceGenerators" / "Properties" / "ActionId.resx"
ROTATION_DIRS = [
    ROOT / "RotationSolver" / "RebornRotations",
    ROOT / "RotationSolver.Basic" / "Rotations",
]

# "...when HP falls below 75%...", "...while HP is below 50%..." - a share the game itself applies.
SHARE_THRESHOLD = re.compile(r"\b(?:HP|health)\b[^.<]{0,40}?below\s+(\d{1,3})\s*%", re.IGNORECASE)
CALL = re.compile(r"(\w+PvE)\.CanUse\([^;]*?targetOverride:\s*TargetType\.LowHP\b(?!Percent)")


def share_threshold_actions(resx_text):
    """Map action identifier -> the percentage its own effect text names, for those that name one."""
    found = {}
    pending_text = None
    for line in resx_text.splitlines():
        if "&lt;para&gt;" in line:
            match = SHARE_THRESHOLD.search(line)
            pending_text = match.group(1) if match else None
            continue
        stripped = line.strip()
        assignment = re.match(r"(\w+PvE)\s*=\s*\d+,", stripped)
        if assignment:
            if pending_text is not None:
                found[assignment.group(1)] = pending_text
            pending_text = None
    return found


def scan(share_actions, files):
    hits = []
    for path in files:
        try:
            text = path.read_text(encoding="utf-8", errors="replace")
        except OSError:
            continue
        for number, line in enumerate(text.splitlines(), start=1):
            for match in CALL.finditer(line):
                action = match.group(1)
                if action in share_actions:
                    hits.append((path, number, action, share_actions[action]))
    return hits


def self_test(share_actions):
    """A constructed defect has to be caught, and the correct form has to pass."""
    import tempfile

    with tempfile.TemporaryDirectory() as directory:
        defect = Path(directory) / "Defect.cs"
        defect.write_text(
            "if (RekindlePvE.CanUse(out act, targetOverride: TargetType.LowHP)) { }\n",
            encoding="utf-8",
        )
        clean = Path(directory) / "Clean.cs"
        clean.write_text(
            "if (RekindlePvE.CanUse(out act, targetOverride: TargetType.LowHPPercent)) { }\n"
            "if (SomeBarrierPvE.CanUse(out act, targetOverride: TargetType.LowHP)) { }\n",
            encoding="utf-8",
        )

        if not scan(share_actions, [defect]):
            return "the constructed defect was not caught"
        if scan(share_actions, [clean]):
            return "a correct call was reported"
    return None


def main():
    if not RESX.exists():
        print(f"{RESX} is missing - cannot establish which actions measure in shares.")
        return 1

    share_actions = share_threshold_actions(RESX.read_text(encoding="utf-8", errors="replace"))
    if "RekindlePvE" not in share_actions:
        print(
            "self-test failed: Rekindle states 'when HP falls below 75%' in its effect text and has "
            "to be recognised as measuring in shares."
        )
        return 1

    failure = self_test(share_actions)
    if failure:
        print(f"self-test failed: {failure}")
        return 1

    print(
        "self-test ok: a points sort on an action that states a percentage threshold is caught, "
        "and the share sort as well as a barrier are left alone."
    )

    files = []
    for directory in ROTATION_DIRS:
        if directory.exists():
            files.extend(sorted(directory.rglob("*.cs")))

    hits = scan(share_actions, files)
    if hits:
        print("")
        for path, number, action, percent in hits:
            print(
                f"{path.relative_to(ROOT)}:{number}: {action} is picked by current health POINTS, "
                f"but its own effect text works from a {percent}% share."
            )
        print("")
        print(f"{len(hits)} call(s) sort by the measure the action itself does not use.")
        return 0

    print(
        f"{len(share_actions)} action(s) state a percentage threshold of their own; none of them is "
        "targeted by a points sort."
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
