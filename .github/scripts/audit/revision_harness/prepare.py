#!/usr/bin/env python3
"""Copies the revision store's sources into a build directory, with the game version read from the harness.

The store's own file is taken as it is, except ReadGameVersion: it reads the game's memory, which outside the game is
no memory at all. With --mutant N, one of the defects the store is built against is put in (MUTANTS), for the self-test.

Usage: prepare.py <build directory> [--mutant N]
"""
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
SOURCE = ROOT / 'RotationSolver/Basic/Configuration'

# Each: (what the store says, what the defect says) - one per way the design could lose or double a reading.
MUTANTS = [
    ('merged.Count = Math.Max(mine.Count, theirs.Count);', 'merged.Count = mine.Count + theirs.Count;'),
    ('entry.Inherited = Combine(entry.Inherited, current, true);', 'entry.Inherited ??= null;'),
    ('if (entry != null && TryHighest(entry, out var known) && Same(higher(known, value), known))', 'if (false)'),
    ('file = ReadFile<RevisionFile<TValue>>(path + ".bak");', 'file = null;'),
]


def main():
    target = Path(sys.argv[1])
    mutant = int(sys.argv[sys.argv.index('--mutant') + 1]) if '--mutant' in sys.argv else None
    shutil.copy(SOURCE / 'LearnedRevision.cs', target / 'LearnedRevision.cs')
    shutil.copy(SOURCE / 'TankbusterReading.cs', target / 'TankbusterReading.cs')
    text = (SOURCE / 'OtherConfiguration.Revision.cs').read_text(encoding='utf-8-sig')
    text = text.replace('using FFXIVClientStructs.FFXIV.Client.System.Framework;\n', '')
    start = text.index('\tprivate static unsafe string? ReadGameVersion()')
    end = text.index('\tprivate static bool IsVersionText')
    text = text[:start] + '\tprivate static string? ReadGameVersion() => Harness.Game;\n\n' + text[end:]
    if mutant is not None:
        before, after = MUTANTS[mutant]
        assert before in text, f'mutant {mutant}: "{before}" is no longer in the store - update MUTANTS'
        text = text.replace(before, after, 1)
    (target / 'Revision.cs').write_text(text, encoding='utf-8')
    return 0


if __name__ == '__main__':
    sys.exit(main())
