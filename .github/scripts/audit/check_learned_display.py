#!/usr/bin/env python3
"""Every learned store of the fork has a tab in the list window's learned tables (concept 13, "Stufe 5", rule 10).

The display exists so that the owner sees why RSR mitigates, leaves out, does not sleep, passes a marker over or holds a
spell: each entry with the value the rules read and its state under the game version. A store added later without a tab
is a store whose decisions he cannot see - nothing fails, it is just missing. So every store in OtherConfiguration.Files
that learns in play - each `LearnedFile`, and each `LocalFile` kept with a backup (the marker and lock stores) - must be
named in RotationSolver/UI/MainWindow/MainWindow_Learned.cs, or read there through the accessor INDIRECT names, or be
listed in EXEMPT with the reason it shows nothing of its own.

NOT SEEN: whether the tab shows the right columns; that is read in the concept's table and the code.

Usage: python3 .github/scripts/audit/check_learned_display.py [--self-test]
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
STORES = ROOT / 'RotationSolver/Basic/Configuration/OtherConfiguration.cs'
DISPLAY = ROOT / 'RotationSolver/UI/MainWindow/MainWindow_Learned.cs'

LEARNED = re.compile(r'new LearnedFile<[^>]*>\(nameof\((\w+)\)')
LOCAL_WITH_BACKUP = re.compile(r'new LocalFile<.*?>\(nameof\((\w+)\)(?:(?!new (?:LocalFile|LearnedFile|SetList|MapList)).)*?backup: true',
                               re.S)

# Read in the display through another name.
INDIRECT = {
    'TankbusterMarkerWithoutHitGame': 'DataCenter.MarkerLockHolds',
    'HostileCastingAreaPotential': 'OtherConfiguration.AreaRevisionSnapshot',
    'TankbusterPotential': 'OtherConfiguration.TankbusterRevisionSnapshot',
}

# Stores that show nothing of their own.
EXEMPT = {
    # How an area figure was measured (unmitigated since A441), not what the game does; the area tab shows the figure.
    'AreaPotentialUnmitigated',
}


def stores(text):
    return sorted(set(LEARNED.findall(text)) | set(LOCAL_WITH_BACKUP.findall(text)))


def findings(store_text, display_text):
    found = []
    for name in stores(store_text):
        if name in EXEMPT:
            continue
        if re.search(r'\b' + name + r'\b', display_text):
            continue
        if name in INDIRECT and INDIRECT[name] in display_text:
            continue
        found.append(name)
    return found


def self_test():
    store_text = STORES.read_text(encoding='utf-8-sig')
    display_text = DISPLAY.read_text(encoding='utf-8-sig')
    names = stores(store_text)
    for expected in ('HostileCastingAreaPotential', 'SleepLocks', 'ActionPenaltySightings', 'TankbusterMarkerHits'):
        if expected not in names:
            raise AssertionError(f'self-test: the store {expected} was not found in OtherConfiguration.Files')
    if 'RotationSolverRecord' in names:
        raise AssertionError('self-test: a local file without a backup was taken for a learned store')
    # A store without a tab must be found.
    added = store_text.replace('new LearnedFile<int>(nameof(HitSeriesOnPlayer)',
                               'new LearnedFile<int>(nameof(LearnedButHidden)), new LearnedFile<int>(nameof(HitSeriesOnPlayer)')
    if 'LearnedButHidden' not in findings(added, display_text):
        raise AssertionError('self-test: a learned store without a tab went unseen')
    # A tab removed must be found.
    if 'SleepDiminishing' not in findings(store_text, display_text.replace('SleepDiminishing', 'Removed')):
        raise AssertionError('self-test: a store whose tab was removed went unseen')
    print(f'self-test ok: {len(names)} learned stores found, a hidden store and a removed tab are caught')


def main():
    self_test()
    found = findings(STORES.read_text(encoding='utf-8-sig'), DISPLAY.read_text(encoding='utf-8-sig'))
    for name in found:
        print(f'{DISPLAY.relative_to(ROOT)}: the learned store {name} has no tab (concept 13, "Stufe 5")')
    if found:
        print(f'{len(found)} finding(s): give each store a tab, or list it in EXEMPT with its reason.')
        return 1
    print('every learned store has a tab in the list window')
    return 0


if __name__ == '__main__':
    sys.exit(main())
