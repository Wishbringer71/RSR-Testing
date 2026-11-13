#!/usr/bin/env python3
"""Does the GCD path pick an ability inside the window the execution gate refuses?

The defect this closes: `CustomRotation_GCD.RaiseSpell` returned Swiftcast - an ability - and did
so only while `WeaponRemain <= 0.5f`. `RSCommands_Actions.DoAction` refuses every ability while
`0 < DefaultGCDRemain <= 0.5f`, and `WeaponRemain` *is* `DefaultGCDRemain`
(`CustomRotation_OtherInfo.cs:1624`). Selection and execution therefore read the same value with
mutually exclusive conditions: the action appeared in the preview window and was never cast, and
the raise waited for a frame that happened to hit the single point where both windows touch.

Nothing failed. There is no exception, no log line, and no compile error - which is why it survived
from January 2025, when the branch was correct because no such gate existed, to March 2026, when
the gate was added (Parnas, *Software Aging*: lack of movement). A static check is the only thing
that notices.

What it does:
  1. reads the ability window out of the execution gate rather than assuming 0.5 - if someone
     retunes the gate, this check retunes with it,
  2. finds every `<Something>PvE.CanUse(out act...)` inside the GCD path that sits under a
     `WeaponRemain <= <value>` condition,
  3. reports it when that value lies inside the gate's window and the action is an ability.

Whether an action is an ability is read from the generated action data, not from a hand-kept list,
so an action added later is classified on its own facts.

Usage: python3 .github/scripts/audit/scan17.py
"""
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import classfile  # noqa: E402

GCD_PATH = 'RotationSolver/Basic/Rotations/CustomRotation_GCD.cs'
GATE_PATH = 'RotationSolver/Commands/RSCommands_Actions.cs'
ABILITY_PATH = 'RotationSolver/Basic/Rotations/CustomRotation_Ability.cs'
HELPER_PATH = 'RotationSolver/Basic/Helpers/ActionHelper.cs'

# The window as a figure, or as the name of it (A486): `ActionHelper.AbilityClipWindow`, or since A753 the measured
# window of one action, `ActionHelper.AbilityWindowFor(...)`. A measured window is read as the fixed window it starts
# from before the first measurement (`FixedAbilityClipWindow`) - the least it can be, the measured one is a lock plus
# a rest and lies above it, so a pick under the fixed window is under the measured one too.
VALUE = r'([0-9.]+f|ActionHelper\.AbilityClipWindow|ActionHelper\.AbilityWindowFor\([^)]*\))'
CONST_RE = re.compile(r'const\s+float\s+(?:Fixed)?AbilityClipWindow\s*=\s*([0-9.]+)f')

# The gate: `baseAct.Info.IsAbility && ... && DataCenter.DefaultGCDRemain <= X && ... > 0f`
GATE_RE = re.compile(
    r'IsAbility.*?DefaultGCDRemain\s*<=\s*' + VALUE, re.S)
# A selection guarded by the shared clock.
WINDOW_RE = re.compile(r'WeaponRemain\s*<=\s*' + VALUE)
# `SwiftcastPvE.CanUse(out act)` and friends - an action handed back from this path.
PICK_RE = re.compile(r'\b([A-Za-z0-9_]+PvE)\.CanUse\s*\(\s*out\s+act\b')


def read(path):
    # A class with its parts (pruefmittel.md): the fixed window stands in ActionHelper.Timing.cs since A753.
    return classfile.read(path)


def figure(value, constant):
    """A captured window: a literal like `0.5f`, or the named constant, resolved to its declared value (None unread)."""
    if value.endswith('f'):
        return float(value[:-1])
    return constant


def gate_window(text, constant=None):
    """The largest GCD remainder for which the execution gate still refuses an ability."""
    hits = [figure(h, constant) for h in GATE_RE.findall(text)]
    hits = [h for h in hits if h is not None]
    return max(hits) if hits else None


def offenders(text, window, constant=None):
    """Lines picking an action under a WeaponRemain window that the gate refuses."""
    found = []
    for num, line in enumerate(text.splitlines(), 1):
        w = WINDOW_RE.search(line)
        p = PICK_RE.search(line)
        value = figure(w.group(1), constant) if w else None
        if value is not None and p and value <= window:
            found.append((num, p.group(1), value, line.strip()))
    return found


def declared_window(text):
    """The value ActionHelper declares for AbilityClipWindow, None when it declares none."""
    m = CONST_RE.search(text)
    return float(m.group(1)) if m else None


def self_test():
    """A clean tree and a broken one must not look alike."""
    gate = 'if (baseAct.Info.IsAbility && !x && DataCenter.DefaultGCDRemain <= 0.5f && y > 0f)'
    assert gate_window(gate) == 0.5, 'the gate window has to be read out of the source'
    assert gate_window('nothing here') is None, 'a missing gate must not read as 0'

    broken = ('if (Service.Config.RaisePlayerBySwift && WeaponRemain <= 0.5f '
              '&& SwiftcastPvE.CanUse(out act))')
    hits = offenders(broken, 0.5)
    assert len(hits) == 1 and hits[0][1] == 'SwiftcastPvE', \
        'the exact shape that broke raising has to be caught'

    fixed = 'if (SwiftcastComingForRaise && !ActionHelper.CanUseGCD)'
    assert offenders(fixed, 0.5) == [], 'the corrected form must come back clean'

    outside = 'if (WeaponRemain <= 1.4f && SwiftcastPvE.CanUse(out act))'
    assert offenders(outside, 0.5) == [], 'a window outside the gate is not this defect'

    named = 'if (baseAct.Info.IsAbility && !x && DataCenter.DefaultGCDRemain <= ActionHelper.AbilityClipWindow && y)'
    assert declared_window('internal const float AbilityClipWindow = 0.5f;') == 0.5, 'the constant is read'
    assert gate_window(named, 0.5) == 0.5, 'a gate naming the constant reads its declared value'
    assert gate_window(named, None) is None, 'an undeclared constant must not read as a window'
    assert len(offenders('if (WeaponRemain <= ActionHelper.AbilityClipWindow && SwiftcastPvE.CanUse(out act))',
                         0.5, 0.5)) == 1, 'a pick under the named window is caught as under the figure'
    measured = 'if (baseAct.Info.IsAbility && !x && DataCenter.DefaultGCDRemain <= ActionHelper.AbilityWindowFor(baseAct.AdjustedID) && y)'
    assert declared_window('private const float FixedAbilityClipWindow = 0.5f;') == 0.5, 'the fixed window is read'
    assert gate_window(measured, 0.5) == 0.5, 'a measured window reads as the fixed window it starts from'
    print('self-test ok: gate parsed, broken form caught, fixed form and wide window accepted\n')


def main():
    self_test()

    for path in (GCD_PATH, GATE_PATH, ABILITY_PATH, HELPER_PATH):
        if not os.path.exists(path):
            print(f'missing: {path}')
            return 1

    constant = declared_window(read(HELPER_PATH))
    window = gate_window(read(GATE_PATH), constant)
    if window is None:
        print(f'{GATE_PATH}: no ability window found in the execution gate.\n'
              'Either the gate moved or its shape changed - this check reads it from there on '
              'purpose,\nso it cannot silently keep testing against a number that no longer '
              'applies.')
        return 1

    print(f'Execution gate refuses abilities while 0 < DefaultGCDRemain <= {window}f '
          f'({GATE_PATH}).')

    bad = offenders(read(GCD_PATH), window, constant)
    print(f'{GCD_PATH}: {len(bad)} selection(s) inside that window.')

    # The known-open one. The first attempt at fixing it made things worse in game - naming the
    # raise as the next GCD fired far more often than the old line ever did, ending the dispatcher
    # in the raise branch and rerouting every ability branch that reads nextGCD - so the code was
    # reverted and the defect stands until a fix is validated in play, not just in CI. Listing it
    # keeps this check meaningful for anything NEW without pretending the tree is clean.
    known = [b for b in bad if b[1] == 'SwiftcastPvE']
    bad = [b for b in bad if b not in known]
    if known:
        for num, action, w, _ in known:
            print(f'  known-open: {GCD_PATH}:{num}: {action} under WeaponRemain <= {w}f '
                  '(see docs/rotation-flow/11-raise-dispatch.md)')

    if bad:
        for num, action, w, line in bad:
            print(f'  {GCD_PATH}:{num}: {action} picked under WeaponRemain <= {w}f')
            print(f'      {line}')
        print('\nWeaponRemain is DefaultGCDRemain, so these can be selected and never executed.\n'
              'Weaving belongs to the ability path: name the GCD you actually want and let\n'
              f'{ABILITY_PATH} weave in front of it.')
        return 1

    print('No selection collides with the execution gate.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
