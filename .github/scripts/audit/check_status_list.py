#!/usr/bin/env python3
"""Guard against reading statuses through Dalamud's StatusList.

Every access to IBattleChara.StatusList builds a StatusList object and, while it is walked, one
Status object per entry. Most status readers run every frame for every party member or enemy -
target choice, stun length, tank protection, gaze, cleanse deadlines - so that is garbage on the
game thread for an answer the client's own array already holds. Upstream 7.5.8.4 moved its readers to
StatusHelper.GetStatuses, a span over that array which also answers a figure without a status
manager with an empty sequence instead of null. The code review of that merge found five fork loops
still on the old path, and a sweep found 25 more (concept 24, "Spieldaten lesen", A853/A854).

Nothing fails when the loop comes back: it compiles, answers correctly and costs only frame time,
which is exactly what a merge or a new rule reintroduces unnoticed. Hence this check.

Left alone, by design:
- upstream's own two lines (the phantom job levels in DutyRotation, throttled; the debug window),
  because a fork line there is merge logistics and neither runs every frame in combat;
- ExtraRotations, the foreign rotations the fork does not edit (.claude/rules/code.md,
  "Fremde Rotationen").
"""

import re
import sys
from pathlib import Path

ROOTS = [Path('RotationSolver'), Path('RotationSolver.Basic')]

ALLOWED_FILES = {
    Path('RotationSolver/Basic/Rotations/Duties/DutyRotation.cs'),
    Path('RotationSolver/UI/MainWindow/MainWindow_Debug.cs'),
}

ALLOWED_DIRS = (Path('RotationSolver/ExtraRotations'),)

ACCESS = re.compile(r'\.StatusList\b')
LINE_COMMENT = re.compile(r'//.*$')


def strip_comments(text):
    """Drops line comments and block comments, keeping line numbers."""
    text = re.sub(r'/\*.*?\*/', lambda m: '\n' * m.group(0).count('\n'), text, flags=re.DOTALL)
    return '\n'.join(LINE_COMMENT.sub('', line) for line in text.split('\n'))


def check(text):
    """Returns the line numbers that read StatusList; empty means none does."""
    lines = []
    for number, line in enumerate(strip_comments(text).split('\n'), start=1):
        if ACCESS.search(line):
            lines.append(number)
    return lines


def allowed(path):
    return path in ALLOWED_FILES or any(parent in path.parents for parent in ALLOWED_DIRS)


def self_test():
    """Constructed cases, because a silent pass is otherwise indistinguishable from a clean tree."""
    defects = {
        'foreach': 'foreach (var status in member.StatusList)\n{\n}',
        'local': 'var statuses = battleChara.StatusList;',
        'null check': 'if (chara.StatusList == null)',
        'pattern': 'if (player?.StatusList is not { } list)',
        'indexed': 'var s = enemy.StatusList[i];',
    }
    for name, text in defects.items():
        if not check(text):
            raise AssertionError('a StatusList read went unnoticed (%s): %r' % (name, text))

    if check('x;\nvar s = enemy.StatusList;') != [2]:
        raise AssertionError('the line number of a finding is wrong')

    clean = '''
        foreach (ref readonly var status in member.GetStatuses())
        {
            if (status.StatusId != 0) { }
        }
        // Dalamud's member.StatusList builds an object per status.
        /* the old loop: foreach (var s in chara.StatusList) */
        var statusList = new List<uint>();
        var x = StatusListCount;
    '''
    if check(clean):
        raise AssertionError('the span form, a comment or a similar name was reported: %s' % check(clean))

    if not allowed(Path('RotationSolver/ExtraRotations/Healer/BeirutaWHM.cs')):
        raise AssertionError('a foreign rotation was not left alone')
    if allowed(Path('RotationSolver/Basic/Helpers/StunWatch.cs')):
        raise AssertionError('a fork file was let through')
    if allowed(Path('RotationSolver/Basic/Rotations/Duties/DutyRotation.Extra.cs')):
        raise AssertionError('an allowed file name let a sibling through')

    print('self-test ok: five forms of a StatusList read are caught, the span form, comments and similar\n'
          '  names are not, and only the two upstream files and the foreign rotations are left alone')


def main():
    self_test()

    found = []
    seen = 0
    for root in ROOTS:
        if not root.exists():
            continue
        for path in sorted(root.rglob('*.cs')):
            if 'obj' in path.parts or 'bin' in path.parts or allowed(path):
                continue
            seen += 1
            for line in check(path.read_text(encoding='utf-8-sig')):
                found.append('%s:%d' % (path, line))

    if seen == 0:
        print('no source file found - run from the repository root')
        return 1

    if found:
        print('statuses read through Dalamud\'s StatusList (an object per status, per call) - '
              'use StatusHelper.GetStatuses:')
        for f in found:
            print('  - %s' % f)
        return 1

    print('%d source files read statuses through GetStatuses only.' % seen)
    return 0


if __name__ == '__main__':
    sys.exit(main())
