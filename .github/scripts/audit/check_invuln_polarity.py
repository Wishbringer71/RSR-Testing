#!/usr/bin/env python3
"""Check that CanBeHealed is not read negated where "unprotected" is meant.

StatusHelper.CanBeHealed() (and PlayerCanBeHealed()) return *true* while NO invulnerability is up: they
ask whether the protective status will have ended two GCDs from now, and an absent status has trivially
ended. Until upstream 7.5.7.1 they were named NoNeedHealingInvuln and PlayerNoNeedHealingInvuln, and that
name read the other way round. Three callers here read it backwards, each found by hand and each silent in play: the generic heal target once collected
only the protected (concept 09), the Scholar's Excogitation filter did the same, and the Warrior's
Nascent Flash filter let only the invulnerable through, so it went to a tank under Holmgang, Superbolide,
Hallowed Ground or Living Dead and to nobody else (A226). scan8.py lists such reads for review, but a
list nobody fails on did not stop the third; upstream found the same in its own Warrior and Scholar code
and renamed the function. Both names are checked, so an old caller carried over by a merge is caught too.
ObjectHelper.IsUnprotectedForHealing() and PlayerIsUnprotectedForHealing(), the heal chain's reading of the
same question (Living Dead not counted while its hold is off, A306), share the polarity and are checked alike.

A negated read ("!x.CanBeHealed()") means "this one is protected". That is sometimes exactly what
a caller wants - to skip the protected - so a negated read is accepted when it is listed below with its
reason. Any other negated read fails.

Fails on a finding. The self-test runs first.
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
SCAN = [ROOT / "RotationSolver", ROOT / "RotationSolver.Basic"]

NEGATED = re.compile(r"!\s*[\w?.()\[\]]*?\b(?:Player)?(?:CanBeHealed|NoNeedHealingInvuln|IsUnprotectedForHealing)\(\)")

# Negated reads that mean "protected" on purpose: file, a fragment of the line, the reason.
ACCEPTED = [
    ("RotationSolver.Basic/DataCenter.cs", "unprotectedOnly && !member.IsUnprotectedForHealing()",
     "skips the protected when only the unprotected are asked for"),
    ("RotationSolver.Basic/DataCenter.cs", "member.MaxHp == 0 || !member.IsUnprotectedForHealing()",
     "AnnouncedHitKills: an invulnerable member does not die of the raidwide, so it is not counted; Living Dead "
     "with its hold off counts, since its fall to zero is Walking Dead (A306)"),
    ("RotationSolver.Basic/Rotations/CustomRotation_OtherInfo.cs", "player == null || !player.CanBeHealed()",
     "TankbusterOnMeWithin: no own cooldown for a tankbuster while an invulnerability covers the player"),
    ("RotationSolver.Basic/Helpers/TankSwapWatch.cs", "asked ? !holder.CanBeHealed()",
     "TakeOverHeld: the enemy is not taken from a co-tank an invulnerability protects (A321)"),
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
    war = "\t\t\t&& !t.CanBeHealed()\n"
    player = "\t\tif (!StatusHelper.PlayerCanBeHealed()) return;\n"
    old_name = "\t\t\t&& !t.NoNeedHealingInvuln()\n"
    plain = "\t\t\t&& t.CanBeHealed()\n"
    comment = "\t\t// This used to read `if (!o.NoNeedHealingInvuln())`, which collected the opposite set\n"
    heal_chain = "\t\t\t&& !t.IsUnprotectedForHealing()\n"
    player_heal_chain = "\t\tif (!ObjectHelper.PlayerIsUnprotectedForHealing()) return;\n"
    accepted = "\t\t\t\t|| (unprotectedOnly && !member.IsUnprotectedForHealing()))\n"
    assert findings_in("RotationSolver/RebornRotations/Tank/WAR_Reborn.cs", war), \
        "the Warrior's inverted filter must be caught"
    assert findings_in("RotationSolver/Updaters/StateUpdater.cs", player), \
        "a negated read of the player variant must be caught"
    assert findings_in("RotationSolver/RebornRotations/Tank/WAR_Reborn.cs", old_name), \
        "a negated read under the old name must be caught"
    assert findings_in("RotationSolver/RebornRotations/Tank/WAR_Reborn.cs", heal_chain), \
        "a negated read of the heal chain's predicate must be caught"
    assert findings_in("RotationSolver/Updaters/StateUpdater.cs", player_heal_chain), \
        "a negated read of the heal chain's player predicate must be caught"
    assert not findings_in("RotationSolver/RebornRotations/Tank/WAR_Reborn.cs", plain), \
        "the plain read means unprotected and is accepted"
    assert not findings_in("RotationSolver.Basic/Actions/ActionTargetInfo.cs", comment), \
        "a comment is not code"
    assert not findings_in("RotationSolver.Basic/DataCenter.cs", accepted), \
        "a listed negated read is accepted"
    assert findings_in("RotationSolver.Basic/Helpers/ObjectHelper.cs", accepted), \
        "a listed line is accepted only in its own file"
    print("self-test ok: negated reads are caught, the player variant, the old name and the heal chain's predicate too; "
          "the plain read, a comment "
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
        print(f"\n{total} negated read(s) of CanBeHealed. It is true while NO invulnerability is "
              "up, so the negation means 'protected'. Read it plain for 'unprotected', or list the line "
              "here with its reason if 'protected' is meant.")
        return 1
    print("Every read of CanBeHealed means what its caller intends, or is listed with its reason.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
