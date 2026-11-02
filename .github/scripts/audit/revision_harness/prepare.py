#!/usr/bin/env python3
"""Copies the revision store's sources into a build directory, with the game version read from the harness.

The store's own file and the locks of stage 4 (LearnedLocks.cs) are taken as they are, except ReadGameVersion: it reads the game's memory, which outside the game is
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
    ('if (entry != null && TryKnown(entry, out var known) && Same(higher(known, value), known))', 'if (false)'),
    ('file = ReadFile<RevisionFile<TValue>>(path + ".bak");', 'file = null;'),
    ('&& current.Game == _game && current.Method == method && current.Clean > 0;', '&& current.Count > 0;'),
    # Stage 3: the old file brings a refuted value back; a save is refused once a value was refuted; the file's
    # taken-over layer returns over the refuted place.
    ('if (entry != null && TryKnown(entry, out var known) && Same(higher(known, value), known))',
     'if (entry != null && TryShow(entry, out var known) && Same(higher(known, value), known))'),
    ('if (TryKnown(before, out var was) && (!TryKnown(after, out var now) || !Same(higher(now, was), now)))',
     'if (TryShow(before, out var was) && (!TryShow(after, out var now) || !Same(higher(now, was), now)))'),
    ('&& (entry.Inherited == null || !SameHistory(entry.Inherited, refutedHere)))', '&& false)'),
    # Stage 3: the old file is written from what the readers see only, so a refuted value is lost with the revision file.
    ('if (entry.Refuted != null && TryKnown(entry, out var known))', 'if (false && TryKnown(entry, out var known))'),
    # Stage 4 (LearnedLocks.cs): a taken-over lock tried again and again; tried while the version is unreadable; a refuted
    # kind still locked; a resistance from elsewhere after a refutation unseen; the limit of a version not the lowest; a
    # fallback limit at a count the kind took; an action laying the status refuted although it did in this version, or
    # with the earlier sightings forgotten, or after fewer landings without it than it had with it.
    ('return seenHere || game == null || mark?.Tried == game ? LockState.Holds : LockState.TakenOver;',
     'return seenHere || game == null ? LockState.Holds : LockState.TakenOver;'),
    ('return seenHere || game == null || mark?.Tried == game ? LockState.Holds : LockState.TakenOver;',
     'return seenHere || mark?.Tried == game ? LockState.Holds : LockState.TakenOver;'),
    ('if (mark is { Refuted: true } && count <= mark.Value)', 'if (false)'),
    ('if (mark is { Refuted: true } && count <= mark.Value)', 'if (mark is { Refuted: true })'),
    ('mark.Value = here ? Math.Min(mark.Value, slept) : slept;', 'mark.Value = slept;'),
    ('|| found.Limit <= floor)', ')'),
    ('if (sighting.With == 0 && sighting.Without >= RefuteLandings', 'if (sighting.Without >= RefuteLandings'),
    ('sighting.Before += sighting.With;', ';'),
    ('&& sighting.Without > sighting.Before)', ')'),
]


def main():
    target = Path(sys.argv[1])
    mutant = int(sys.argv[sys.argv.index('--mutant') + 1]) if '--mutant' in sys.argv else None
    shutil.copy(SOURCE / 'LearnedRevision.cs', target / 'LearnedRevision.cs')
    shutil.copy(SOURCE / 'TankbusterReading.cs', target / 'TankbusterReading.cs')
    locks = (SOURCE / 'LearnedLocks.cs').read_text(encoding='utf-8-sig')
    text = (SOURCE / 'OtherConfiguration.Revision.cs').read_text(encoding='utf-8-sig')
    text = text.replace('using FFXIVClientStructs.FFXIV.Client.System.Framework;\n', '')
    start = text.index('\tprivate static unsafe string? ReadGameVersion()')
    end = text.index('\tprivate static bool IsVersionText')
    text = text[:start] + '\tprivate static string? ReadGameVersion() => Harness.Game;\n\n' + text[end:]
    if mutant is not None:
        before, after = MUTANTS[mutant]
        if before in text:
            text = text.replace(before, after, 1)
        else:
            assert before in locks, f'mutant {mutant}: "{before}" is no longer in the store or the locks - update MUTANTS'
            locks = locks.replace(before, after, 1)
    (target / 'Revision.cs').write_text(text, encoding='utf-8')
    (target / 'LearnedLocks.cs').write_text(locks, encoding='utf-8')
    return 0


if __name__ == '__main__':
    sys.exit(main())
