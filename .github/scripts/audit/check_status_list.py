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
- the foreign rotations the fork does not edit (.claude/rules/code.md, "Fremde Rotationen"), told apart by
  layout.is_third_party: since upstream c170c9cd5 they stand in the job folders beside the shipped ones, and the folder
  this script had left alone (RotationSolver/ExtraRotations) exists no more (A878).
"""

import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import layout  # noqa: E402

ROOTS = [Path('RotationSolver')]

# Upstream's own lines, counted: one more in the same file is a new reader and is reported (A864).
ALLOWED_COUNTS = {
    Path('RotationSolver/Rotations/Duties/DutyRotation.cs'): 1,
    Path('RotationSolver/UI/MainWindow/MainWindow_Debug.cs'): 2,
}

# A member access (.StatusList) or a property pattern ({ StatusList: ... }).
ACCESS = re.compile(r'\.StatusList\b|[{,]\s*StatusList\s*:')
# Strings first, so that a // or /* inside one does not hide the code after it; then comments. Newlines are kept.
TOKENS = re.compile(r'@"(?:[^"]|"")*"|\$?"(?:\\.|[^"\\\n])*"|\'(?:\\.|[^\'\\\n])\'|//[^\n]*|/\*.*?\*/', re.DOTALL)


def strip_comments(text):
    """Blanks string and character literals and drops comments, keeping line numbers."""
    def blank(m):
        token = m.group(0)
        return '\n' * token.count('\n') if token.startswith('/') else '""' + '\n' * token.count('\n')
    return TOKENS.sub(blank, text)


def check(text):
    """Returns the line numbers that read StatusList; empty means none does."""
    lines = []
    for number, line in enumerate(strip_comments(text).split('\n'), start=1):
        if ACCESS.search(line):
            lines.append(number)
    return lines


def allowed(path):
    """Whole files left alone: the foreign rotations."""
    return path.is_file() and layout.is_third_party(path)


def self_test():
    """Constructed cases, because a silent pass is otherwise indistinguishable from a clean tree."""
    defects = {
        'foreach': 'foreach (var status in member.StatusList)\n{\n}',
        'local': 'var statuses = battleChara.StatusList;',
        'null check': 'if (chara.StatusList == null)',
        'pattern': 'if (player?.StatusList is not { } list)',
        'indexed': 'var s = enemy.StatusList[i];',
        'property pattern': 'if (target is { StatusList: { } list })',
        'after a string with //': 'var url = "https://x"; foreach (var s in c.StatusList) { }',
        'after a string with /*': 'var p = "a/*b"; var l = chara.StatusList;',
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

    if not allowed(Path('RotationSolver/Rotations/Healer/WhiteMage/BeirutaWHM.cs')):
        raise AssertionError('a foreign rotation was not left alone')
    if allowed(Path('RotationSolver/Engine/Helpers/StunWatch.cs')):
        raise AssertionError('a fork file was let through')
    if allowed(Path('RotationSolver/Rotations/Duties/DutyRotation.cs')):
        raise AssertionError('an upstream file was let through whole instead of by its count')
    if over_count(Path('RotationSolver/Rotations/Duties/DutyRotation.cs'), [587]):
        raise AssertionError("upstream's own line was reported")
    if not over_count(Path('RotationSolver/Rotations/Duties/DutyRotation.cs'), [587, 900]):
        raise AssertionError('a second reader in an upstream file went unnoticed')
    if not over_count(Path('RotationSolver/Rotations/Duties/DutyRotation.Extra.cs'), [1]):
        raise AssertionError('a sibling of an allowed file was let through')
    if check('var s = "x.StatusList";\nvar t = \'.\';'):
        raise AssertionError('a StatusList inside a string was reported')

    print('self-test ok: eight forms of a StatusList read are caught (dotted, property pattern, after a string\n'
          '  with // or /*), the span form, comments, strings and similar names are not; upstream keeps exactly\n'
          '  its own lines, and only the foreign rotations are left alone whole')


def over_count(path, lines):
    """Whether these finding lines in this file exceed what upstream keeps there."""
    return len(lines) > ALLOWED_COUNTS.get(path, 0)


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
            lines = check(path.read_text(encoding='utf-8-sig'))
            if over_count(path, lines):
                found += ['%s:%d' % (path, line) for line in lines]

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
