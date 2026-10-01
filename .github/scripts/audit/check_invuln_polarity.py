#!/usr/bin/env python3
"""Check that NoNeedHealingInvuln is not read negated where "unprotected" is meant.

StatusHelper.NoNeedHealingInvuln() (and PlayerNoNeedHealingInvuln()) return *true* while NO
invulnerability is up: they ask whether the protective status will have ended two GCDs from now, and an
absent status has trivially ended. The name reads the other way round. Three callers have read it
backwards so far, each found by hand and each silent in play: the generic heal target once collected
only the protected (concept 09), the Scholar's Excogitation filter did the same, and the Warrior's
Nascent Flash filter let only the invulnerable through, so it went to a tank under Holmgang, Superbolide,
Hallowed Ground or Living Dead and to nobody else (A226). scan8.py lists such reads for review, but a
list nobody fails on did not stop the third.

A negated read ("!x.NoNeedHealingInvuln()") means "this one is protected". That is sometimes exactly what
a caller wants - to skip the protected - so a negated read is accepted when it is listed below with its
reason. Any other negated read fails.

Fails on a finding. The self-test runs first.
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
SCAN = [ROOT / "RotationSolver", ROOT / "RotationSolver.Basic"]

NEGATED = re.compile(r"!\s*[\w?.()\[\]]*?\bP?(?:layer)?NoNeedHealingInvuln\(\)|!\s*StatusHelper\.PlayerNoNeedHealingInvuln\(\)")

# Negated reads that mean "protected" on purpose: file, a fragment of the line, the reason.
ACCEPTED = [
    ("RotationSolver.Basic/DataCenter.cs", "unprotectedOnly && !member.NoNeedHealingInvuln()",
     "skips the protected when only the unprotected are asked for"),
    ("RotationSolver.Basic/Rotations/CustomRotation_OtherInfo.cs", "player == null || !player.NoNeedHealingInvuln()",
     "TankbusterOnMeWithin: no own cooldown for a tankbuster while an invulnerability covers the player"),
]


def findings_in(relative, text):
    """Line numbers and lines with a negated read that is not accepted."""
    found = []
    for number, line in enumerate(text.splitlines(), 1):
        code = line.split("//", 1)[0]
        if not NEGATED.search(code):
            continue
        if any(relative == path and fragment in code for path, fragment, _ in ACCEPTED):
            continue
        found.append((number, line.strip()))
    return found


def self_test():
    war = "\t\t\t&& !t.NoNeedHealingInvuln()\n"
    player = "\t\tif (!StatusHelper.PlayerNoNeedHealingInvuln()) return;\n"
    plain = "\t\t\t&& t.NoNeedHealingInvuln()\n"
    comment = "\t\t// This used to read `if (!o.NoNeedHealingInvuln())`, which collected the opposite set\n"
    accepted = "\t\t\t\t|| (unprotectedOnly && !member.NoNeedHealingInvuln()))\n"
    assert findings_in("RotationSolver/RebornRotations/Tank/WAR_Reborn.cs", war), \
        "the Warrior's inverted filter must be caught"
    assert findings_in("RotationSolver/Updaters/StateUpdater.cs", player), \
        "a negated read of the player variant must be caught"
    assert not findings_in("RotationSolver/RebornRotations/Tank/WAR_Reborn.cs", plain), \
        "the plain read means unprotected and is accepted"
    assert not findings_in("RotationSolver.Basic/Actions/ActionTargetInfo.cs", comment), \
        "a comment is not code"
    assert not findings_in("RotationSolver.Basic/DataCenter.cs", accepted), \
        "a listed negated read is accepted"
    assert findings_in("RotationSolver.Basic/Helpers/ObjectHelper.cs", accepted), \
        "a listed line is accepted only in its own file"
    print("self-test ok: negated reads are caught, the player variant too; the plain read, a comment "
          "and a listed read are left alone, and a listing holds only for its own file")


def main():
    self_test()
    total = 0
    for base in SCAN:
        for path in sorted(base.rglob("*.cs")):
            if "obj" in path.relative_to(ROOT).parts:
                continue
            relative = path.relative_to(ROOT).as_posix()
            for number, line in findings_in(relative, path.read_text(encoding="utf-8-sig")):
                print(f"{relative}:{number}: {line}")
                total += 1
    if total:
        print(f"\n{total} negated read(s) of NoNeedHealingInvuln. It is true while NO invulnerability is "
              "up, so the negation means 'protected'. Read it plain for 'unprotected', or list the line "
              "here with its reason if 'protected' is meant.")
        return 1
    print("Every read of NoNeedHealingInvuln means what its caller intends, or is listed with its reason.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
