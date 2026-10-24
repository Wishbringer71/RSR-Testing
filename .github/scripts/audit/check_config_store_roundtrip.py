#!/usr/bin/env python3
"""Guard against a stored list that is written but never read back.

OtherConfiguration keeps every persistent list as a static field, loads it at login and writes the
whole in-memory table back to its file on every save. That pairing is the entire contract.

It once rested on two hand-written lists that had to agree, and they did not.
`HostileCastingAreaPotential` - the learned damage share of each area action - was entered into `Init`
alone, while `InitAsync` is the entry point the plugin actually calls. The table therefore started every
session empty, and the first save of the session wrote that empty table over the file. Nothing threw,
nothing was logged, and in play the only symptom was every area action reading as *unrated* again.

Since upstream 7.5.7.5 (merged in A331) one array, `Files`, carries every store: `InitAsync` loads exactly
its entries, `Save()` and `SaveList(name)` write exactly its entries, and `SaveList` looks a name up in it
(`GetFile`, which throws for a name it does not hold). A store can therefore not reach a save path
without a load path - provided nothing goes around the array. That proviso is what this check measures:

- every name a save asks for - `SaveList(nameof(X))`, a direct `SaveTracked(..., nameof(X)` - is in `Files`;
- every list the list window draws and saves by name (`nameof(OtherConfiguration.X)` in MainWindow_List)
  is in `Files`, since `SaveList` throws for any other;
- `InitAsync` and `Save()` both walk `Files`, and `InitAsync` carries no load line of its own - a stray
  `InitOne` there is the return of the two-lists construction.

A load path that is never taken is not a load path: the first version of this check asked only whether a
load line existed anywhere, and run against the broken tree it reported it clean.
"""

import re
import sys
from pathlib import Path

TARGET = Path('RotationSolver.Basic/Configuration/OtherConfiguration.cs')
LIST_WINDOW = Path('RotationSolver/UI/MainWindow/MainWindow_List.cs')

# The one array, from its declaration to the closing bracket.
FILES = re.compile(r'private\s+static\s+readonly\s+ListFile\[\]\s+Files\s*=\s*\[(?P<body>.*?)\n\t\];', re.DOTALL)
# An entry of it: new SetList(nameof(X), ..., new LocalFile<T>(nameof(X), ..., new LearnedFile<T>(nameof(X), ... - the
# built-in lists since upstream 3c6ce9ee2 (SetList, MapList<T>, OrderList), the user's own (LocalFile<T>), the learned
# tables (LearnedFile<T>); ListFile<T> is the form before it.
ENTRY = re.compile(r'new\s+(?:ListFile|LearnedFile|LocalFile|SetList|MapList|OrderList)(?:<[^(]*?>)?\(\s*nameof\(\s*(\w+)\s*\)')
# A save by name. SaveList(nameof(X)) is the ordinary path; a SaveTracked with a nameof would go around it.
SAVED = re.compile(r'\b(?:SaveList|SaveTracked)\(\s*(?:\w+\s*,\s*)?nameof\(\s*(\w+)\s*\)')
# A load line of its own.
LOAD_LINE = re.compile(r'\b(?:InitOne|LoadLearned)\s*\(')
# The two entry points and their bodies.
INIT_ASYNC = re.compile(r'public\s+static\s+async\s+Task\s+InitAsync\s*\([^)]*\)\s*\{(?P<body>.*?)\n\t\}', re.DOTALL)
SAVE_ALL = re.compile(r'public\s+static\s+Task\s+Save\s*\(\s*\)\s*\{(?P<body>.*?)\n\t\}', re.DOTALL)
# Lists the list window draws by name; each of those draws saves through SaveList(name).
WINDOW_NAME = re.compile(r'nameof\(\s*OtherConfiguration\.(\w+)\s*\)')


def check(text, window_text=''):
    """Returns a list of complaints; empty means every saved store is loaded on the path taken."""
    problems = []

    files = FILES.search(text)
    if not files:
        return ['the Files array was not found - the single list of stores is what this check measures, so its '
                'absence is the defect, not a reason to pass']

    listed = {m.group(1) for m in ENTRY.finditer(files.group('body'))}

    for name in sorted({m.group(1) for m in SAVED.finditer(text)} - listed):
        problems.append('%s is saved by name but is not in Files - it has no load path, and SaveList throws '
                        'for it' % name)

    for name in sorted({m.group(1) for m in WINDOW_NAME.finditer(window_text)} - listed):
        problems.append('the list window saves %s by name, but it is not in Files - SaveList throws for it' % name)

    init = INIT_ASYNC.search(text)
    if not init:
        problems.append('InitAsync not found - the entry point the plugin calls')
    else:
        if 'Files' not in init.group('body'):
            problems.append('InitAsync does not walk Files - the stores in it are not loaded')
        if LOAD_LINE.search(init.group('body')):
            problems.append('InitAsync carries a load line of its own - that is the two-lists construction this '
                            'check exists to prevent')

    save = SAVE_ALL.search(text)
    if not save:
        problems.append('Save() not found')
    elif 'Files' not in save.group('body'):
        problems.append('Save() does not walk Files - a store would be loaded and never written, or written '
                        'by a path of its own')

    return problems


def tree(entries, saves='', init_body='\t\tforeach (var file in Files) { file.Load(false); }',
         save_body='\t\tforeach (var file in Files) { file.PrepareWrite(); }\n\t\treturn Task.CompletedTask;'):
    """A miniature OtherConfiguration, tabbed the way the real file is."""
    return ('\tprivate static readonly ListFile[] Files =\n\t[\n%s\n\t];\n\n'
            '\tpublic static async Task InitAsync(CancellationToken cancellationToken = default)\n\t{\n%s\n\t}\n\n'
            '\tpublic static Task Save()\n\t{\n%s\n\t}\n\n%s' % (entries, init_body, save_body, saves))


def self_test():
    """Constructed defects, because a silent pass is otherwise indistinguishable from a clean tree."""
    both = ('\t\tnew SetList(nameof(HostileCastingArea), ActionLists.HostileCastingArea, () => HostileCastingArea, v => HostileCastingArea = v, 41147),\n'
            '\t\tnew LearnedFile<float>(nameof(HostileCastingAreaPotential),\n'
            '\t\t\t() => new Dictionary<uint, float>(HostileCastingAreaPotential), v => HostileCastingAreaPotential = v,\n'
            '\t\t\tMath.Max, state => AreaPotentialStoreState = state, "rated action(s)"),\n'
            '\t\tnew LocalFile<Dictionary<string, TankbusterMarkerHit>>(nameof(TankbusterMarkerHits), () => TankbusterMarkerHits, v => TankbusterMarkerHits = v),\n'
            '\t\tnew MapList<string>(nameof(NoProvokeNames), TerritoryLists.NoProvokeNames, () => NoProvokeNames, v => NoProvokeNames = v),\n'
            '\t\tnew OrderList(nameof(KardiaTankPriority), JobPriorityLists.KardiaTankPriority, () => KardiaTankPriority, v => KardiaTankPriority = v),\n'
            '\t\tnew ListFile<HashSet<uint>>(nameof(HostileCastingStop), () => HostileCastingStop, v => HostileCastingStop = v),')
    saves = ('\tpublic static Task SaveHostileCastingArea() => SaveList(nameof(HostileCastingArea));\n'
             '\tpublic static Task SaveHostileCastingAreaPotential() => SaveList(nameof(HostileCastingAreaPotential));\n'
             '\tpublic static Task SaveTankbusterMarkerHits() => SaveList(nameof(TankbusterMarkerHits));\n'
             '\tpublic static Task SaveNoProvokeNames() => SaveList(nameof(NoProvokeNames));\n'
             '\tpublic static Task SaveKardiaTankPriority() => SaveList(nameof(KardiaTankPriority));\n'
             '\tpublic static Task SaveHostileCastingStop() => SaveList(nameof(HostileCastingStop));\n')

    paired = tree(both, saves)
    if check(paired, 'DrawActionsList(nameof(OtherConfiguration.HostileCastingArea), x);'):
        raise AssertionError('a correctly paired store was rejected: %s' % check(paired))

    # The original defect in the new shape: the potentials are saved, but nothing loads them.
    only = both.split('\t\tnew LearnedFile')[0] + '\t\tnew LocalFile' + both.split('\t\tnew LocalFile')[1]
    found = check(tree(only, saves))
    if not any('HostileCastingAreaPotential' in p and 'not in Files' in p for p in found):
        raise AssertionError('a store saved without a load path went unnoticed: %s' % found)

    # A save that goes around the array.
    around = saves + '\tpublic static void X() => SaveTracked(snapshot, nameof(TankbusterPotential), Higher);\n'
    if not any('TankbusterPotential' in p for p in check(tree(both, around))):
        raise AssertionError('a direct tracked save of a store outside Files went unnoticed')

    # A load line back in the entry point, and an entry point that does not walk the array.
    stray = '\t\tvalue = InitOne(new T(), nameof(HostileCastingAreaPotential), false, false);'
    found = check(tree(both, saves, init_body=stray))
    if not any('load line of its own' in p for p in found) or not any('does not walk Files' in p for p in found):
        raise AssertionError('a load line in InitAsync, or InitAsync not walking Files, went unnoticed: %s' % found)

    # Save() writing by a path of its own.
    found = check(tree(both, saves, save_body='\t\treturn SaveHostileCastingArea();'))
    if not any('Save() does not walk Files' in p for p in found):
        raise AssertionError('a Save() that does not walk Files went unnoticed: %s' % found)

    # The list window saving a list the array does not hold.
    found = check(paired, 'DrawStatusList(nameof(OtherConfiguration.NoCastingStatus), x);')
    if not any('list window saves NoCastingStatus' in p for p in found):
        raise AssertionError('a list window name outside Files went unnoticed: %s' % found)

    # A store in Files that nobody saves by name is legitimate - Save() still writes it.
    if check(tree(both)):
        raise AssertionError('a listed store without a named save was reported as a defect')

    # No array at all is the construction this check requires, so its absence is a complaint.
    if not check(saves):
        raise AssertionError('a file without Files passed')

    print('self-test ok: a listed store is accepted; a store saved by name outside Files is caught, through\n'
          '  SaveList, a direct tracked save or the list window; a load line in InitAsync, and an InitAsync or\n'
          '  Save() that does not walk Files, are caught; and a missing Files array is itself a complaint')


def main():
    self_test()

    if not TARGET.exists():
        print('%s not found - run from the repository root' % TARGET)
        return 1

    text = TARGET.read_text(encoding='utf-8')
    window_text = LIST_WINDOW.read_text(encoding='utf-8') if LIST_WINDOW.exists() else ''
    problems = check(text, window_text)

    files = FILES.search(text)
    listed = len({m.group(1) for m in ENTRY.finditer(files.group('body'))}) if files else 0
    if problems:
        print('%s: %d problem(s) with %d listed store(s)' % (TARGET, len(problems), listed))
        for p in problems:
            print('  %s' % p)
        return 1

    print('%s: all %d stores are in Files, which both entry points walk, and every save by name finds its store'
          % (TARGET, listed))
    return 0


if __name__ == '__main__':
    sys.exit(main())
