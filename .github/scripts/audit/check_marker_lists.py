#!/usr/bin/env python3
"""Does every marker path belong to one kind only - tankbuster, stack or spread?

`DataCenter` sorts the lock-on effects it reads into four lists (`TankbusterPaths`, `MultiHitSharedPaths`,
`SharedDamagePaths`, `SpreadDamagePaths`) and matches them by prefix. The kind decides whom a marker hits: a stack
involves everyone who stacks, a spread only the member it sits on, a tankbuster the tank. A path in two kinds is read
as the one asked first - M9's spread marker `x6r9_loc01_t0a1` sat in the stack list too, so a spread on another member
counted as a hit on the player, and every self-only protection went for it (A394). Upstream added it to both lists in
one commit; a sync can bring the next one.

The two stack lists are one kind. A path counts as in a kind when it starts with any entry of that kind, as the reader
matches it.

Usage: python3 .github/scripts/audit/check_marker_lists.py [path-to-DataCenter.cs]
Exit code 1 when a path matches two kinds, or a list is not found.
"""
import re
import sys
from pathlib import Path

SOURCE = Path('RotationSolver.Basic/DataCenter.cs')
KINDS = {
    'tankbuster': ('TankbusterPaths',),
    'stack': ('MultiHitSharedPaths', 'SharedDamagePaths'),
    'spread': ('SpreadDamagePaths',),
}

LINE_COMMENT = re.compile(r'//[^\n]*')


def entries(source, name):
    """The string entries of the FrozenSet field `name`, lower case, comments left out; None when it is not there."""
    match = re.search(r'\b' + name + r'\s*=\s*FrozenSet\.ToFrozenSet\(\s*\[(?P<body>.*?)\]', source, re.DOTALL)
    if not match:
        return None
    return [value.lower() for value in re.findall(r'"([^"]*)"', LINE_COMMENT.sub('', match.group('body')))]


def overlaps(source):
    """(path, kind it is listed under, other kind whose entry it starts with) for every path in two kinds; raises
    ValueError when a list is missing."""
    kinds = {}
    for kind, names in KINDS.items():
        paths = []
        for name in names:
            found = entries(source, name)
            if found is None:
                raise ValueError(f'list {name} not found')
            paths.extend(found)
        kinds[kind] = paths

    result = []
    for kind, paths in kinds.items():
        for other, prefixes in kinds.items():
            if other == kind:
                continue
            for path in paths:
                if any(path.startswith(prefix) for prefix in prefixes):
                    result.append((path, kind, other))
    return sorted(set(result))


def self_test():
    def lists(tank, multi, shared, spread):
        def field(name, values):
            body = ',\n'.join(f'"{v}" // note' for v in values)
            return f'private static readonly FrozenSet<string> {name} = FrozenSet.ToFrozenSet(\n[\n{body}\n], x);\n'
        return (field('TankbusterPaths', tank) + field('MultiHitSharedPaths', multi)
                + field('SharedDamagePaths', shared) + field('SpreadDamagePaths', spread))

    clean = lists(['vfx/lockon/eff/tank'], ['vfx/lockon/eff/com_share4a1'], ['vfx/lockon/eff/com_share'],
                  ['vfx/lockon/eff/x6r9_loc01_t0a1'])
    assert overlaps(clean) == [], 'two stack lists sharing a prefix are one kind'
    both = lists(['vfx/lockon/eff/tank'], [], ['vfx/lockon/eff/x6r9_loc01_t0a1'], ['vfx/lockon/eff/x6r9_loc01_t0a1'])
    assert ('vfx/lockon/eff/x6r9_loc01_t0a1', 'spread', 'stack') in overlaps(both), 'a path in two kinds is caught'
    prefix = lists(['vfx/lockon/eff/tank'], [], [], ['vfx/lockon/eff/tank_spread'])
    assert ('vfx/lockon/eff/tank_spread', 'spread', 'tankbuster') in overlaps(prefix), \
        'a path another kind matches by prefix is caught'
    case = lists(['vfx/lockon/eff/Tank'], [], ['vfx/lockon/eff/TANK_share'], [])
    assert overlaps(case), 'the match ignores case, as the reader does'
    commented = lists(['vfx/lockon/eff/tank'], [], [], ['vfx/lockon/eff/x6fd_loc04m_5s1v'])
    commented = commented.replace('// note', '// vfx/lockon/eff/tank')
    assert overlaps(commented) == [], 'a path in a comment is no entry'
    try:
        overlaps(clean.replace('SpreadDamagePaths', 'Renamed'))
    except ValueError:
        pass
    else:
        raise AssertionError('a missing list fails instead of passing')
    print('self-test ok: one path in two kinds, a prefix of another kind and a case difference are caught; '
          'the two stack lists count as one kind, comments are no entries, and a missing list fails')


def main():
    self_test()
    path = Path(sys.argv[1]) if len(sys.argv) > 1 else SOURCE
    try:
        found = overlaps(path.read_text(encoding='utf-8-sig'))
    except ValueError as error:
        print(f'{path}: {error}')
        return 1
    for marker, kind, other in found:
        print(f'{path}: "{marker}" is listed as {kind} and matched as {other}')
    if found:
        return 1
    print(f'Every marker path in {path} belongs to one kind.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
