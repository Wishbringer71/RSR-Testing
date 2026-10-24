#!/usr/bin/env python3
"""Which user settings does nothing read?

A setting with a label, a default and a range promises the user that moving it changes something.
When nothing reads it, the promise is silent and permanent: no warning, no log line, no failing
build. The user turns the knob, nothing happens, and the only way to find out is to read the whole
tree.

Two of these turned up inside a single code path while tracking down why raises were never cast:
`SwiftcastBuffer` (removed since, A339) and `IBaseAction.IgnoreClipping`, which names the mechanism
that was missing. Neither had a reader. That is a defect class, not two accidents.

An accepted entry goes stale twice over: when its setting is read again, and when the setting is
gone. Both are reported, because a stale reason reads like a current one - the entries for the
interrupt and provoke delays still said "blocked on Configs.Migrate" after both were wired, and the
one for LastSeenChangelog called its changelog prompt gone after upstream brought it back (7e539f3a5,
A339).

Scope, and why it is drawn narrowly: only properties **declared** in Configs.cs as
`public <type> <Name> { get; ... }` are checked. Settings that exist as private `[UI]` backing
fields are generated into properties by the source generator under a name this script cannot derive
reliably - deriving it produced five names that do not exist in the file at all, which would have
been reported as findings. A check that invents findings is worse than one with a known blind spot,
so the blind spot is declared here instead: private-field settings are not covered.

Usage: python3 .github/scripts/audit/scan18.py [--list]
"""
import pathlib
import re
import sys

CONFIG = 'RotationSolver.Basic/Configuration/Configs.cs'
ROOTS = ('RotationSolver.Basic', 'RotationSolver', 'RotationSolver.SourceGenerators')
SKIP_DIRS = ('/obj/', '/bin/')

PROP_RE = re.compile(
    r'^\s*public\s+(?:readonly\s+)?[A-Za-z0-9_<>?\[\],\s]+?\s+'
    r'([A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get\b', re.M)

# Settings with no reader that are known and accepted, with the reason they are not a finding.
# Anything not listed here and not read is reported.
ACCEPTED = {
    'RotationLibs':
        'stored state without a label: the paths of external rotation libraries, whose loader and '
        'input fields upstream replaced with its reworked rotation loading (d566eda86, 2025)',
    'ActionSequencerIndex':
        'internal persisted index, no UI and no label - stored state, not a promise to the user',
    'UiPaddingScale':
        'read outside this tree: Configs implements RebornMaterial.IM3Settings, and the package '
        '(7.5.6.2, upstream 27f6acb41) reads it through that interface (name found in its DLL)',
}


def sources():
    for root in ROOTS:
        base = pathlib.Path(root)
        if not base.exists():
            continue
        for path in base.rglob('*.cs'):
            text = str(path)
            if any(skip in text for skip in SKIP_DIRS):
                continue
            yield path


def declared(config_text):
    return sorted(set(PROP_RE.findall(config_text)))


def unread(names, blob):
    out = []
    for name in names:
        word = re.compile(r'\b' + re.escape(name) + r'\b')
        if not any(word.search(text) for path, text in blob if path.name != 'Configs.cs'):
            out.append(name)
    return out


def stale(accepted, names, orphans):
    """Accepted entries whose reason no longer holds: the setting is read now, or no longer declared."""
    return sorted(n for n in accepted if n not in names or n not in orphans)


def self_test():
    cfg = ('public float Wired { get; set; } = 1f;\n'
           'public float Orphan { get; set; } = 2f;\n'
           'private static readonly bool _derived = false;\n')
    names = declared(cfg)
    assert names == ['Orphan', 'Wired'], names
    assert '_derived' not in names and 'Derived' not in names, \
        'private backing fields are out of scope on purpose - deriving their names invents findings'

    fake = [(pathlib.Path('Other.cs'), 'var x = Service.Config.Wired;')]
    assert unread(names, fake) == ['Orphan'], 'a read setting must not be reported'

    none_read = [(pathlib.Path('Other.cs'), 'nothing at all')]
    assert unread(names, none_read) == ['Orphan', 'Wired'], 'both must be caught when nothing reads'

    # A reader inside Configs.cs itself does not count - that is the declaration file.
    only_self = [(pathlib.Path('Configs.cs'), 'Wired Orphan')]
    assert unread(names, only_self) == ['Orphan', 'Wired'], \
        'self-references in the declaration file must not count as readers'
    accepted = {'Orphan': 'reason', 'Wired': 'reason', 'Gone': 'reason'}
    assert stale(accepted, names, ['Orphan']) == ['Gone', 'Wired'], \
        'an accepted entry for a setting read again or no longer declared must be reported'
    print('self-test ok: orphan caught, wired accepted, private fields excluded, '
          'self-reference ignored, stale acceptances caught\n')


def main():
    self_test()

    cfg_path = pathlib.Path(CONFIG)
    if not cfg_path.exists():
        print(f'missing: {CONFIG}')
        return 1

    blob = []
    for path in sources():
        try:
            blob.append((path, path.read_text(encoding='utf-8-sig')))
        except OSError:
            continue

    names = declared(cfg_path.read_text(encoding='utf-8-sig'))
    orphans = unread(names, blob)

    print(f'{len(names)} settings declared as public properties in {CONFIG}.')
    print(f'{len(orphans)} without a reader outside that file.')

    if '--list' in sys.argv:
        for name in orphans:
            note = ACCEPTED.get(name)
            print(f'  {name}' + (f'  [accepted] {note}' if note else ''))

    new = [n for n in orphans if n not in ACCEPTED]
    if new:
        print('\nNo reader anywhere, and not recorded as accepted:')
        for name in new:
            print(f'  {name}')
        print('\nA setting with a label and a default promises the user that it does something.\n'
              'Either wire it up, remove it with a migration path for stored configuration, or\n'
              'add it to ACCEPTED in this script with the reason it stays.')
        return 1

    old = stale(ACCEPTED, names, orphans)
    if old:
        print('\nAccepted as unread, but read again or no longer declared - remove the entry:')
        for name in old:
            print(f'  {name}')
        return 1

    print('Every declared setting is read, or recorded as knowingly accepted.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
