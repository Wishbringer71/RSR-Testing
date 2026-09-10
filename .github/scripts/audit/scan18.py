#!/usr/bin/env python3
"""Which user settings does nothing read?

A setting with a label, a default and a range promises the user that moving it changes something.
When nothing reads it, the promise is silent and permanent: no warning, no log line, no failing
build. The user turns the knob, nothing happens, and the only way to find out is to read the whole
tree.

Two of these turned up inside a single code path while tracking down why raises were never cast:
`SwiftcastBuffer`, whose own documentation describes exactly the timing that was broken, and
`IBaseAction.IgnoreClipping`, which names the mechanism that was missing. Neither had a reader.
That is a defect class, not two accidents.

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
    'SwiftcastBuffer':
        'recorded in TODO.md: its documented meaning puts Swiftcast inside the window the '
        'execution gate refuses, so wiring it would rebuild the defect it belongs to',
    'InterruptDelay':
        'recorded in TODO.md: wiring it would delay interrupts by 0.5-1s by default, which can '
        'lose the cast it exists to stop - needs a decision, not a patch',
    'ProvokeDelay':
        'recorded in TODO.md: same shape as InterruptDelay, decided together with it',
    'TargetColor':
        'recorded in TODO.md: also declares itself as its own UI parent, so the setting cannot '
        'be reached in the interface either - one finding, two causes',
    'RotationLibs':
        'recorded in TODO.md: external rotation library paths; LoadCustomRotationGroup takes its '
        'assemblies from elsewhere, so the feature this configured is gone rather than broken',
    'ActionSequencerIndex':
        'internal persisted index, no UI and no label - stored state, not a promise to the user',
    'LastSeenChangelog':
        'internal persisted marker for a changelog prompt that no longer exists - stored state, '
        'not a promise to the user',
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
    print('self-test ok: orphan caught, wired accepted, private fields excluded, '
          'self-reference ignored\n')


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

    print('Every declared setting is read, or recorded as knowingly accepted.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
