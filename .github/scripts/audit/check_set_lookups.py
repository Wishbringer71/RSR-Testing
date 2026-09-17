#!/usr/bin/env python3
"""Guard against walking a set that exists to be looked up.

The four stored action lists - HostileCastingArea, HostileCastingTank, HostileCastingKnockback and
HostileCastingStop - are HashSet<uint>, and all four were searched with a foreach that compared
every id in turn. That is O(n) out of a structure built for O(1), and the area list ships with 850
entries and grows in play, so the cost of one yes/no question grew with the list.

Nothing fails when the loop comes back. It compiles, it answers correctly, and the only symptom is
frame time - which is exactly the kind of regression an upstream merge reintroduces unnoticed, since
four of the five sites were upstream code to begin with.

The check is deliberately narrow: it looks for a foreach over one of these sets, not for set
iteration in general. Iterating a set is legitimate - saving it, drawing it in the UI, counting it.
What is not legitimate is iterating it to answer membership.
"""

import re
import sys
from pathlib import Path

TARGETS = [Path('RotationSolver.Basic/DataCenter.cs')]

SETS = (
    'HostileCastingArea',
    'HostileCastingTank',
    'HostileCastingKnockback',
    'HostileCastingStop',
)

# foreach over one of the sets whose body compares against a RowId - the membership test written out
# by hand. The body is bounded by the loop's own closing brace at the same nesting, which a plain
# regex cannot see, so the window is capped instead: a membership loop is short.
WALK = re.compile(
    r'foreach\s*\(\s*var\s+\w+\s+in\s+OtherConfiguration\.(%s)\s*\)(?P<body>.{0,400}?)\}' % '|'.join(SETS),
    re.DOTALL)
COMPARE = re.compile(r'==\s*\w+\.RowId|\.RowId\s*==')


def check(text):
    """Returns a list of complaints; empty means every membership test is a lookup."""
    problems = []
    for m in WALK.finditer(text):
        if COMPARE.search(m.group('body')):
            line = text.count('\n', 0, m.start()) + 1
            problems.append('line %d: OtherConfiguration.%s is walked to test membership - use '
                            'Contains, the set is built for it' % (line, m.group(1)))
    return problems


def self_test():
    """Constructed defects, because a silent pass is otherwise indistinguishable from a clean tree."""
    good = '''
        return OtherConfiguration.HostileCastingArea.Contains(act.RowId)
            && AreaCastCanReachPlayer(h, act);
        return OtherConfiguration.HostileCastingTank.Contains(act.RowId);
    '''
    if check(good):
        raise AssertionError('the lookup form was rejected: %s' % check(good))

    for name in SETS:
        walked = '''
            foreach (var id in OtherConfiguration.%s)
            {
                if (id == act.RowId)
                {
                    return true;
                }
            }
        ''' % name
        found = check(walked)
        if not any(name in p for p in found):
            raise AssertionError('a membership walk over %s went unnoticed: %s' % (name, found))

    # Iterating a set for something other than membership is fine and must not be reported.
    legitimate = '''
        foreach (var id in OtherConfiguration.HostileCastingArea)
        {
            writer.Write(id);
        }
    '''
    if check(legitimate):
        raise AssertionError('iterating a set to save it was reported as a defect: %s'
                             % check(legitimate))

    print('self-test ok: the lookup form is accepted, a membership walk over each of the four sets '
          'is caught,\n  and iterating a set for another purpose is not reported')


def main():
    self_test()

    problems = []
    for path in TARGETS:
        if not path.exists():
            print('%s not found - run from the repository root' % path)
            return 1
        problems += ['%s %s' % (path, p) for p in check(path.read_text(encoding='utf-8'))]

    if problems:
        print('a stored action list is searched instead of looked up:')
        for p in problems:
            print('  - %s' % p)
        return 1

    print('every membership test against the stored action lists is a lookup.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
