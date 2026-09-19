#!/usr/bin/env python3
"""Guard against walking a set that exists to be looked up.

The stored action and status lists are HashSet<uint>, and every one of them was searched with a
foreach that compared each id in turn. That is O(n) out of a structure built for O(1). The area list
ships with 850 entries and grows in play, so the cost of one yes/no question grew with the list, and
the status ones are asked per status per target.

Nothing fails when the loop comes back. It compiles, it answers correctly, and the only symptom is
frame time - exactly the kind of regression an upstream merge reintroduces unnoticed, since most of
these sites were upstream code to begin with.

The survey behind the first version of this check stopped at DataCenter and the four casting lists.
It missed a site in Watcher and three in StatusHelper, so both the file list and the set list below
are wider than the defect that prompted them - the narrower version would have passed a tree that
still held six of the ten.

The check is deliberately narrow in the other direction: it looks for a foreach over one of these
sets whose body tests an id, not for set iteration in general. Iterating a set is legitimate -
saving it, drawing it in the UI, counting it. What is not legitimate is iterating it to answer
membership.
"""

import re
import sys
from pathlib import Path

# Both files that ask these lists whether they hold an id. The first survey stopped at DataCenter and
# missed the site in Watcher, which is why this is a list rather than a single path.
TARGETS = [
    Path('RotationSolver.Basic/DataCenter.cs'),
    Path('RotationSolver.Basic/Helpers/StatusHelper.cs'),
    Path('RotationSolver/Watcher.cs'),
]

SETS = (
    'HostileCastingArea',
    'HostileCastingTank',
    'HostileCastingKnockback',
    'HostileCastingStop',
    'InvincibleStatus',
    'PriorityStatus',
    'DangerousStatus',
    'NoCastingStatus',
)

# foreach over one of the sets whose body compares against a RowId - the membership test written out
# by hand. The body is bounded by the loop's own closing brace at the same nesting, which a plain
# regex cannot see, so the window is capped instead: a membership loop is short.
WALK = re.compile(
    r'foreach\s*\(\s*var\s+\w+\s+in\s+OtherConfiguration\.(%s)\s*\)(?P<body>.{0,400}?)\}' % '|'.join(SETS),
    re.DOTALL)
COMPARE = re.compile(r'==\s*\w+\.(RowId|StatusId)|\.(RowId|StatusId)\s*==')


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
        member = 'act.RowId' if name.startswith('HostileCasting') else 'status.StatusId'
        walked = '''
            foreach (var id in OtherConfiguration.%s)
            {
                if (id == %s)
                {
                    return true;
                }
            }
        ''' % (name, member)
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

    if check(good + '\n return OtherConfiguration.DangerousStatus.Contains(status.StatusId);'):
        raise AssertionError('the status lookup form was rejected')

    print('self-test ok: the lookup form is accepted, a membership walk over each of the %d sets '
          'is caught,\n  and iterating a set for another purpose is not reported' % len(SETS))


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
