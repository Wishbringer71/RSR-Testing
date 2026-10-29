#!/usr/bin/env python3
"""No code outside the store writes the area and tankbuster tables the readers see (concept 13, "Stufe 1").

Since stage 1 of the revisions, `HostileCastingAreaPotential` and `TankbusterPotential` are set from the revisions
alone: every writer reports its reading (`OtherConfiguration.MeasureAreaShare`, `MeasureTankbuster`), and the store sets
the table to the higher of the running and the taken-over layer. A writer that still sets the table itself puts a value
there that no layer holds - the next load rebuilds the table from the layers, and the value is gone, or it stands for the
session beside layers that say otherwise. Nothing throws; in a fight the table and the revisions disagree, and stage 2
reads the revisions.

What counts as a write, in every C# file of the repository outside .github/, bin/ and obj/ except the two files of the
store (OtherConfiguration.cs, whose load sets the field, and OtherConfiguration.Revision.cs, the store): an indexer
assignment, an assignment to the field, and Add, TryAdd, Remove, Clear or TryRemove on it. Reading it - TryGetValue,
ContainsKey, Count, a foreach - is left alone. NOT SEEN: a write through a local alias of the table (`var t =
OtherConfiguration.HostileCastingAreaPotential; t[id] = x;`) - the one alias there is, in the list window, only reads; and
a change inside a TankbusterReading, which its init-only figures rule out at compile time.

Usage: python3 .github/scripts/audit/check_learned_writes.py
"""

import re
import sys
from pathlib import Path

SKIPPED = ('.github', 'bin', 'obj')
TABLES = ('HostileCastingAreaPotential', 'TankbusterPotential')
ALLOWED = {
    Path('RotationSolver/Basic/Configuration/OtherConfiguration.cs'),
    Path('RotationSolver/Basic/Configuration/OtherConfiguration.Revision.cs'),
}
NAMES = '|'.join(TABLES)
# The table name as a whole word - TankbusterPotentialGate is the lock, not the table.
WRITES = [
    re.compile(rf'\b(?:{NAMES})\b\s*\[[^\]]*\]\s*(?:=(?!=)|\+=|-=|\?\?=)'),
    re.compile(rf'(?<![\w.])(?:OtherConfiguration\.)?(?:{NAMES})\b\s*(?:=(?![=>])|\?\?=)'),
    re.compile(rf'\b(?:{NAMES})\b\s*\.\s*(?:Add|TryAdd|Remove|Clear|TryRemove)\s*\('),
]
COMMENT = re.compile(r'^\s*//')


def writes_in(text):
    found = []
    for number, line in enumerate(text.splitlines(), 1):
        if COMMENT.match(line):
            continue
        code = line.split('//', 1)[0]
        if any(pattern.search(code) for pattern in WRITES):
            found.append((number, line.strip()))
    return found


def self_test():
    caught = [
        'OtherConfiguration.HostileCastingAreaPotential[id] = highestShare;',
        'OtherConfiguration.TankbusterPotential[series.Key] = new TankbusterReading',
        'OtherConfiguration.HostileCastingAreaPotential.Remove(id);',
        'OtherConfiguration.TankbusterPotential.TryAdd(id, reading);',
        'OtherConfiguration.HostileCastingAreaPotential = [];',
    ]
    passed = [
        'if (!OtherConfiguration.HostileCastingAreaPotential.TryGetValue(id, out var known))',
        'lock (OtherConfiguration.TankbusterPotentialGate)',
        'var rated = OtherConfiguration.HostileCastingAreaPotential;',
        'OtherConfiguration.HostileCastingAreaPotential.ContainsKey(id) == false',
        '// OtherConfiguration.HostileCastingAreaPotential[id] = 0f;',
        'foreach (var (id, reading) in OtherConfiguration.TankbusterPotential)',
    ]
    for line in caught:
        assert writes_in(line), f'a write passed: {line}'
    for line in passed:
        assert not writes_in(line), f'a read was taken for a write: {line}'
    print(f'self-test ok: {len(caught)} writes caught, {len(passed)} reads, the lock and a comment left alone')


def main():
    self_test()
    problems = []
    for path in sorted(Path('.').rglob('*.cs')):
        if path in ALLOWED or path.name.endswith('.g.cs') or any(part in SKIPPED for part in path.parts):
            continue
        for number, line in writes_in(path.read_text(encoding='utf-8-sig')):
            problems.append(f'{path}:{number}: {line}')
    if problems:
        print('The tables the readers see are written outside the store - report the reading to it instead '
              '(OtherConfiguration.MeasureAreaShare, MeasureTankbuster):')
        for problem in problems:
            print('  ' + problem)
        return 1
    print(f'{" and ".join(TABLES)}: written only by the store')
    return 0


if __name__ == '__main__':
    sys.exit(main())
