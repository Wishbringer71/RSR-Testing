#!/usr/bin/env python3
"""Copies the revision store's sources into a build directory, with the game version read from the harness.

The store's own file and the locks of stage 4 (LearnedLocks.cs) are taken as they are, except ReadGameVersion: it reads the game's memory, which outside the game is
no memory at all. Every store declared in OtherConfiguration.cs is written to Stores.cs with its table and gate, so a
store added there needs no copy in Stubs.cs (A878). With --mutant N, one of the defects the store is built against is put in (MUTANTS), for the self-test.

Usage: prepare.py <build directory> [--mutant N]
"""
import re
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
SOURCE = ROOT / 'RotationSolver/Configuration'
STORE = re.compile(r'^\tprivate static readonly RevisedStore<(\w+)> (\w+) = new\(.*?\);\n', re.S | re.M)

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


def stores(config):
    """Each store declaration of OtherConfiguration with the table and the gate it reads: (declaration, value type,
    table, gate or None)."""
    found = []
    for match in STORE.finditer(config):
        decl = match.group(0)
        table = re.search(r'\(\) => (\w+)', decl).group(1)
        gate = re.search(r'\b(\w+Gate)\b', decl)
        found.append((decl, match.group(1), table, gate.group(1) if gate else None))
    return found


def stores_file(config):
    found = stores(config)
    assert len(found) >= 3, f'{len(found)} stores found in OtherConfiguration.cs - the declaration pattern changed'
    lines = ['// Written by prepare.py from OtherConfiguration.cs.', 'namespace RotationSolver.Configuration', '{',
             '\tinternal partial class OtherConfiguration', '\t{']
    for decl, value, table, gate in found:
        lines.append(f'\t\tinternal static Dictionary<uint, {value}> {table} = [];')
        if gate:
            lines.append(f'\t\tinternal static readonly object {gate} = new();')
        lines.append('\t' + decl.replace('\n\t', '\n\t\t').rstrip('\n'))
        lines.append('')
    lines += ['\t}', '}', '']
    return '\n'.join(lines)


def self_test():
    config = ('\tprivate static readonly RevisedStore<float> AreaRevision = new(nameof(A), 1, Math.Max,\n'
              '\t\tIsValid, s => s, () => A, null, "area table");\n\n'
              '\tprivate static readonly RevisedStore<R> BRevision = new(nameof(B), 1,\n'
              '\t\tR.Higher, R.IsValid, r => r.U,\n\t\t() => B, BGate, "b table");\n')
    assert [(v, t, g) for _, v, t, g in stores(config)] == [('float', 'A', None), ('R', 'B', 'BGate')]
    try:
        stores_file(config)
    except AssertionError:
        pass
    else:
        raise AssertionError('two stores must not pass for the three OtherConfiguration holds')
    text = stores_file((SOURCE / 'OtherConfiguration.cs').read_text(encoding='utf-8-sig'))
    assert 'AimedHitPotentialGate' in text and 'TankbusterRevision' in text, 'the stores of the plugin are found'
    print('self-test ok: stores, tables and gates are taken from OtherConfiguration.cs')
    return 0


def main():
    if sys.argv[1:] == ['--self-test']:
        return self_test()
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
    config = (SOURCE / 'OtherConfiguration.cs').read_text(encoding='utf-8-sig')
    (target / 'Stores.cs').write_text(stores_file(config), encoding='utf-8')
    (target / 'LearnedLocks.cs').write_text(locks, encoding='utf-8')
    return 0


if __name__ == '__main__':
    sys.exit(main())
