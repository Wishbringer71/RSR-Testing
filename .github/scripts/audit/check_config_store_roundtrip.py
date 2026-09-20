#!/usr/bin/env python3
"""Guard against a stored list that is written but never read back.

OtherConfiguration keeps every persistent list as a static field, loads it at login and writes the
whole in-memory table back to its file on every save. That pairing is the entire contract, and it is
held together by nothing but two hand-written lists that have to agree.

They did not. `HostileCastingAreaPotential` - the learned damage share of each area action - was
entered into `Init` alone, while `InitAsync` is the entry point the plugin actually calls. The table
therefore started every session empty, and the first save of the session wrote that empty table over
the file. Nothing threw, nothing was logged, and in play the only symptom was every area action
reading as *unrated* again: mitigation behaved exactly as it had before the measurement existed, so
the loss looked like the feature simply not doing much. What it cost was the reason the store exists
- a fight progged over several evenings can only ever be rated across sessions.

The check is the round trip, in the direction that loses data: a field written by `Save(x, nameof(x))`
must also be loaded by `InitOne(ref x, nameof(x)` **in `LoadSteps`**. The other direction is harmless
- a list that is loaded and never saved simply stays as it was shipped.

The qualifier is the whole check. The first version of this script asked only whether the file held a
load line for the field anywhere, and run against the broken tree it reported it clean: the load line
existed, in the `Init` nobody calls. A load path that is never taken is not a load path, so the
reachable one - the single list both entry points now iterate - is what gets measured, and a stray
`InitOne` back inside `Init` or `InitAsync` is reported as the return of the two-lists construction
that caused this.
"""

import re
import sys
from pathlib import Path

TARGET = Path('RotationSolver.Basic/Configuration/OtherConfiguration.cs')

# Save(Field, nameof(Field)) - the only form used, and the one that writes the whole table.
SAVED = re.compile(r'\bSave\(\s*(\w+)\s*,\s*nameof\(\s*\1\s*\)\s*\)')
# InitOne(ref Field, nameof(Field) - trailing arguments (download flags) vary and do not matter here.
LOADED = re.compile(r'\bInitOne\(\s*ref\s+(\w+)\s*,\s*nameof\(\s*\1\s*\)')
# The one list both entry points iterate, from its signature to the closing bracket of the array.
STEPS = re.compile(r'private\s+static\s+Action\[\]\s+LoadSteps\(\)\s*=>\s*\[(?P<body>.*?)\n\t\];',
                   re.DOTALL)
# Init and InitAsync must carry no load lines of their own; the Reset* methods legitimately do.
ENTRY = re.compile(r'public\s+static\s+(?:async\s+Task|void)\s+Init(?:Async)?\s*\([^)]*\)\s*\{'
                   r'(?P<body>.*?)\n\t\}', re.DOTALL)


def check(text):
    """Returns a list of complaints; empty means every saved store is loaded on the path taken."""
    problems = []

    steps = STEPS.search(text)
    if not steps:
        return ['LoadSteps() not found - the single load list is what this check measures, so its '
                'absence is the defect, not a reason to pass']

    saved = {m.group(1) for m in SAVED.finditer(text)}
    loaded = {m.group(1) for m in LOADED.finditer(steps.group('body'))}

    for name in sorted(saved - loaded):
        problems.append('%s is written to disk but is not in LoadSteps - the first save of a session '
                        'overwrites the stored file with the empty table' % name)

    for m in ENTRY.finditer(text):
        for stray in LOADED.finditer(m.group('body')):
            problems.append('%s is loaded inside an entry point instead of LoadSteps - that is the '
                            'two-lists construction this check exists to prevent' % stray.group(1))

    return problems


def tree(steps, saves='', entry=''):
    """A miniature OtherConfiguration, tabbed the way the real file is."""
    return ('\tprivate static Action[] LoadSteps() =>\n\t[\n%s\n\t];\n\n'
            '\tpublic static void Init()\n\t{\n%s\n\t}\n\n%s' % (steps, entry, saves))


def self_test():
    """Constructed defects, because a silent pass is otherwise indistinguishable from a clean tree."""
    both = ('\t\t() => InitOne(ref HostileCastingArea, nameof(HostileCastingArea)),\n'
            '\t\t() => InitOne(ref HostileCastingAreaPotential, '
            'nameof(HostileCastingAreaPotential), false),')
    saves = ('\tpublic static Task SaveHostileCastingArea()\n'
             '\t\t=> Task.Run(() => Save(HostileCastingArea, nameof(HostileCastingArea)));\n'
             '\tpublic static Task SaveHostileCastingAreaPotential()\n'
             '\t\t=> Task.Run(() => Save(HostileCastingAreaPotential, '
             'nameof(HostileCastingAreaPotential)));')

    paired = tree(both, saves)
    if check(paired):
        raise AssertionError('a correctly paired store was rejected: %s' % check(paired))

    # The actual defect, in the shape it had: the potentials are saved, and the only load line for
    # them sits in an entry point rather than in the list both entry points run.
    only = '\t\t() => InitOne(ref HostileCastingArea, nameof(HostileCastingArea)),'
    stray = '\t\t_ = Task.Run(() => InitOne(ref HostileCastingAreaPotential, ' \
            'nameof(HostileCastingAreaPotential), false));'
    found = check(tree(only, saves, stray))
    if not any('is written to disk' in p and 'HostileCastingAreaPotential' in p for p in found):
        raise AssertionError('a store saved without a reachable load path went unnoticed: %s' % found)
    if not any('inside an entry point' in p for p in found):
        raise AssertionError('a load line back inside an entry point went unnoticed: %s' % found)

    # Saved with nothing loading it at all - the same loss, without the misleading load line.
    found = check(tree(only, saves))
    if not any('HostileCastingAreaPotential' in p for p in found):
        raise AssertionError('a store that is saved but never loaded went unnoticed: %s' % found)

    # Loaded but not saved is legitimate - a curated list the user never edits - and must not fail.
    if check(tree('\t\t() => InitOne(ref DangerousStatus, nameof(DangerousStatus)),')):
        raise AssertionError('a load without a save was reported as a defect')

    # No single list at all is the construction this check requires, so its absence is a complaint.
    if not check(saves):
        raise AssertionError('a file without LoadSteps passed')

    print('self-test ok: a paired store is accepted; a saved store with no reachable load path is\n'
          '  caught, whether its load line is missing or stranded in an entry point; a loaded store\n'
          '  with no save path is not reported; and a missing LoadSteps is itself a complaint')


def main():
    self_test()

    if not TARGET.exists():
        print('%s not found - run from the repository root' % TARGET)
        return 1

    text = TARGET.read_text(encoding='utf-8')
    problems = check(text)

    saved = len({m.group(1) for m in SAVED.finditer(text)})
    if problems:
        print('%s: %d of %d stored lists have no load path' % (TARGET, len(problems), saved))
        for p in problems:
            print('  %s' % p)
        return 1

    print('%s: all %d stored lists are loaded as well as saved' % (TARGET, saved))
    return 0


if __name__ == '__main__':
    sys.exit(main())
