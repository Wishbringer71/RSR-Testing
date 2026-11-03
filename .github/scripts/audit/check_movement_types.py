#!/usr/bin/env python3
"""Does every action that moves the player carry a movement type?

The movement safety check ("Use BMR integration to verify safety of movement actions/actions that cause movement for
automatic usage") runs only for an action whose `ActionSetting.SpecialType` is one of the moving types
(`ActionTargetInfo.IsMovingSpecialType`). Which actions move is the game's own answer: the Action sheet's
`AffectsPosition`. Until A498 En Avant, Smudge, Aetherial Shift and Elusive Jump moved without a type and went out on a
movement command unchecked; upstream added the type to three actions by hand in 7.5.7.14 (A498).

This reads every PvE player action with `AffectsPosition` from xivapi v2, maps it to its identifier through
`ActionId.resx`, and requires a moving `setting.SpecialType` in its `Modify<Name>PvE` under
`RotationSolver/Basic/Rotations/Basic`. Actions that move the player but cannot be measured are listed in ACCEPTED with
their reason; an accepted action that has a type by now is reported as stale. An unreachable xivapi skips the
comparison: an outage is not a defect.

Usage: python3 .github/scripts/audit/check_movement_types.py
Exit code 1 for a moving action without a type, a stale acceptance, or a failed self-test.
"""
import json
import pathlib
import re
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[3]
RESX = ROOT / 'RotationSolver.SourceGenerators' / 'Properties' / 'ActionId.resx'
BASIC = ROOT / 'RotationSolver' / 'Basic' / 'Rotations' / 'Basic'
API = 'https://v2.xivapi.com/api'

MOVING_TYPES = {
    'FixedDistanceMoveForward', 'FixedDistanceMoveBackward', 'HostileMovingForward', 'FriendlyMovingForward',
    'HostileFriendlyMovingForward', 'HostileMovingAttack', 'ObjectBasedMovement', 'HostileAttackBackstep',
}

ACCEPTED = {
    'ShukuchiPvE': 'ground target ("Move quickly to the specified location"): the landing point is the spot chosen, '
                   'which needs a targeting of its own; no fork rotation casts it',
    'CloudSkimPvE': 'ground target in a chosen direction ("Move quickly in the selected direction"); no fork rotation '
                    'casts it',
    'RescuePvE': 'moves the party member drawn, not the player ("Instantly draws target party member to your side")',
}


def fetch_json(url):
    """The parsed answer, or None when xivapi cannot be reached. curl, as check_action_names.py: the proxy setup of the
    environments this runs in is configured for curl."""
    try:
        out = subprocess.run(['curl', '-sS', '--max-time', '30', url], capture_output=True, text=True)
        return json.loads(out.stdout) if out.returncode == 0 and out.stdout else None
    except (OSError, ValueError):
        return None


def moving_rows(fetch=fetch_json):
    """Row id -> name of every PvE player action that moves someone and belongs to a job or role (level above 0),
    the Blue Mage left out (a limited job outside the owner's profile); None when xivapi is unreachable."""
    rows = {}
    cursor = None
    while True:
        url = (f'{API}/search?sheets=Action&limit=500&fields=Name,ClassJobLevel,ClassJob.Abbreviation'
               '&query=%2BAffectsPosition%3Dtrue%20%2BIsPlayerAction%3Dtrue%20%2BIsPvP%3Dfalse')
        if cursor:
            url += f'&cursor={cursor}'
        answer = fetch(url)
        if answer is None or 'results' not in answer:
            return None
        for result in answer['results']:
            fields = result.get('fields', {})
            job = ((fields.get('ClassJob') or {}).get('fields') or {}).get('Abbreviation', '')
            if fields.get('ClassJobLevel', 0) > 0 and job != 'BLU':
                rows[result['row_id']] = fields.get('Name', '')
        cursor = answer.get('next')
        if not cursor:
            return rows


def identifiers(resx_text):
    """Row id -> identifier of every PvE action the generated resource declares."""
    out = {}
    for name, row in re.findall(r'\n([A-Za-z][A-Za-z0-9_]*PvE) = (\d+),', resx_text):
        out.setdefault(int(row), name)
    return out


def special_types(sources):
    """Identifier -> the SpecialType its Modify method sets (None when it sets none); only identifiers that have a
    Modify method appear."""
    out = {}
    for text in sources:
        for match in re.finditer(r'static partial void Modify(\w+PvE)\(ref ActionSetting setting\)\s*\{(.*?)\n\t\}',
                                 text, re.S):
            found = re.findall(r'^\s*setting\.SpecialType\s*=\s*SpecialActionType\.(\w+)\s*;', match.group(2), re.M)
            out[match.group(1)] = found[-1] if found else None
    return out


def compare(rows, ids, types, accepted):
    """Problem lines: a moving action whose base sets no moving type, and an acceptance no longer needed."""
    problems = []
    for row, name in sorted(rows.items()):
        ident = ids.get(row)
        if ident is None:
            continue
        typed = types.get(ident) in MOVING_TYPES
        if ident in accepted:
            if typed:
                problems.append(f'STALE    {ident} ({row}, {name}) carries {types[ident]} now - remove it from ACCEPTED')
            continue
        if not typed:
            problems.append(f'UNTYPED  {ident} ({row}, {name}): AffectsPosition in the sheet, '
                            f'SpecialType {types.get(ident, "no Modify method")}')
    return problems


def self_test():
    rows = {1: 'Dash', 2: 'Untyped Dash', 3: 'Ground Move', 4: 'Typed Ground Move', 5: 'Unknown'}
    ids = {1: 'DashPvE', 2: 'UntypedDashPvE', 3: 'GroundMovePvE', 4: 'TypedGroundMovePvE'}
    source = ('\tstatic partial void ModifyDashPvE(ref ActionSetting setting)\n\t{\n'
              '\t\tsetting.SpecialType = SpecialActionType.FixedDistanceMoveForward;\n\t}\n'
              '\tstatic partial void ModifyUntypedDashPvE(ref ActionSetting setting)\n\t{\n'
              '\t\t//setting.SpecialType = SpecialActionType.HostileMovingForward;\n'
              '\t\tsetting.IsFriendly = true;\n\t}\n'
              '\tstatic partial void ModifyTypedGroundMovePvE(ref ActionSetting setting)\n\t{\n'
              '\t\tsetting.SpecialType = SpecialActionType.ObjectBasedMovement;\n\t}\n')
    types = special_types([source])
    accepted = {'GroundMovePvE': 'test', 'TypedGroundMovePvE': 'test'}
    problems = compare(rows, ids, types, accepted)
    assert types == {'DashPvE': 'FixedDistanceMoveForward', 'UntypedDashPvE': None,
                     'TypedGroundMovePvE': 'ObjectBasedMovement'}, types
    assert any(p.startswith('UNTYPED  UntypedDashPvE') for p in problems), problems
    assert any(p.startswith('STALE    TypedGroundMovePvE') for p in problems), problems
    assert not any('DashPvE (1' in p or 'GroundMovePvE (3' in p for p in problems), problems
    assert len(problems) == 2, problems
    assert moving_rows(fetch=lambda url: None) is None
    print('self-test ok: a typed move passes, a commented-out type is caught, an acceptance holds, a stale one is '
          'caught, an unreachable xivapi is told apart')


def main():
    self_test()
    rows = moving_rows()
    if rows is None:
        print('xivapi unreachable: the comparison against the Action sheet is skipped.')
        return 0
    ids = identifiers(RESX.read_text(encoding='utf-8-sig'))
    types = special_types(p.read_text(encoding='utf-8-sig') for p in sorted(BASIC.glob('*.cs')))
    problems = compare(rows, ids, types, ACCEPTED)
    print(f'{len(rows)} PvE player actions move someone (AffectsPosition); {len(ACCEPTED)} accepted without a type.')
    if problems:
        print('\n'.join(problems))
        print('An action that moves the player needs a moving SpecialType, or the movement safety check never runs '
              'for it (concept 05, A498).')
        return 1
    print('Every one of them carries a movement type, or is accepted with its reason.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
