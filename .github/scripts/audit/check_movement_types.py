#!/usr/bin/env python3
"""Does every action that moves the player carry a movement type?

The movement safety check ("Use BMR integration to verify safety of movement actions/actions that cause movement for
automatic usage") runs only for an action whose `ActionSetting.SpecialType` is one of the moving types
(`ActionTargetInfo.IsMovingSpecialType`). Which actions move is the game's own answer: the Action sheet's
`AffectsPosition`. Until A498 En Avant, Smudge, Aetherial Shift and Elusive Jump moved without a type and went out on a
movement command unchecked; upstream added the type to three actions by hand in 7.5.7.14 (A498).

This reads every action with `AffectsPosition` from xivapi v2, PvE and PvP, maps it to its identifier through
`ActionId.resx`, and requires a moving `setting.SpecialType` in its `Modify<Name>PvE` or `Modify<Name>PvP` under
`RotationSolver/Basic/Rotations/Basic` or `/Duties`. Until A756 PvP was left out as outside the owner's profile, and
Onslaught, Plunge, Rough Divide and five more PvP jumps went out from 20 yalms unchecked (concept 25, "PvP: Bewegung"). Measured are every job action, the Blue Mage included, and every content action (Phantom jobs, Bozja, quest
battles) that a rotation casts - a `<Name>.CanUse(` outside a comment; one nobody casts never moves the player through
RSR. Until A519 the Blue Mage and all content actions were left out as outside the owner's profile, and J Kick, Flying
Frenzy, Phantom Kick and Occult Featherfoot went out unchecked. Actions that move the player but cannot be measured are
listed in ACCEPTED with their reason; an accepted action that has a type by now is reported as stale. An unreachable
xivapi skips the comparison: an outage is not a defect.

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
DUTIES = ROOT / 'RotationSolver' / 'Basic' / 'Rotations' / 'Duties'
CODE = ROOT / 'RotationSolver'
API = 'https://v2.xivapi.com/api'

MOVING_TYPES = {
    'FixedDistanceMoveForward', 'FixedDistanceMoveBackward', 'HostileMovingForward', 'FriendlyMovingForward',
    'HostileFriendlyMovingForward', 'HostileMovingAttack', 'ObjectBasedMovement', 'HostileAttackBackstep',
    'GroundTargetMove',
}

ACCEPTED = {
    'RescuePvE': 'moves the party member drawn, not the player ("Instantly draws target party member to your side")',
    'SteadfastStancePvE': 'RSR casts it on the player only (TargetType.Self); the rush comes only "when targeting a party '
                          'member"',
    'NightStallionPvE': 'quest battle slide ("a sliding attack ... in a straight line before you"): no text states how '
                        'far, and a fixed type without a distance is refused next to BossModReborn (A313)',
    'LostSeraphStrikePvE': 'Bozja, outside the owner\'s profile: none of his recordings from 25.10. to 03.11.2026 shows '
                           'Bozja or Zadnor; recorded, not worked on (A519)',
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
    """Row id -> (name, is_job) of every action, PvE and PvP, that moves someone: a job or role action (a player action with a
    level above 0, the Blue Mage included) or a content action (level 0: Phantom jobs, Bozja, quest battles); None when
    xivapi is unreachable."""
    rows = {}
    cursor = None
    while True:
        url = (f'{API}/search?sheets=Action&limit=500&fields=Name,ClassJobLevel,IsPlayerAction'
               '&query=%2BAffectsPosition%3Dtrue')
        if cursor:
            url += f'&cursor={cursor}'
        answer = fetch(url)
        if answer is None or 'results' not in answer:
            return None
        for result in answer['results']:
            fields = result.get('fields', {})
            if fields.get('ClassJobLevel', 0) > 0:
                if fields.get('IsPlayerAction'):
                    rows[result['row_id']] = (fields.get('Name', ''), True)
            else:
                rows[result['row_id']] = (fields.get('Name', ''), False)
        cursor = answer.get('next')
        if not cursor:
            return rows


def cast_identifiers(sources):
    """Identifiers some rotation casts: a `<Name>.CanUse(` outside a line comment."""
    out = set()
    for text in sources:
        for line in text.splitlines():
            code = line.split('//', 1)[0]
            out.update(re.findall(r'\b(\w+Pv[EP](?:_\d+)?)\.CanUse\(', code))
    return out


def measured_rows(rows, ids, cast):
    """Row id -> name of the actions to hold against a type: every job action, and a content action only when a
    rotation casts it."""
    out = {}
    for row, (name, is_job) in rows.items():
        if is_job or ids.get(row) in cast:
            out[row] = name
    return out


def identifiers(resx_text):
    """Row id -> identifier of every PvE and PvP action the generated resource declares."""
    out = {}
    for name, row in re.findall(r'\n([A-Za-z][A-Za-z0-9_]*Pv[EP]) = (\d+),', resx_text):
        out.setdefault(int(row), name)
    return out


def special_types(sources):
    """Identifier -> the SpecialType its Modify method sets (None when it sets none); only identifiers that have a
    Modify method appear."""
    out = {}
    for text in sources:
        for match in re.finditer(r'static partial void Modify(\w+Pv[EP])\(ref ActionSetting setting\)\s*\{(.*?)\n\t\}',
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
    rows = {1: 'Dash', 2: 'Untyped Dash', 3: 'Ground Move', 4: 'Typed Ground Move', 5: 'Unknown', 6: 'PvP Rush'}
    ids = {1: 'DashPvE', 2: 'UntypedDashPvE', 3: 'GroundMovePvE', 4: 'TypedGroundMovePvE', 6: 'RushPvP'}
    source = ('\tstatic partial void ModifyDashPvE(ref ActionSetting setting)\n\t{\n'
              '\t\tsetting.SpecialType = SpecialActionType.FixedDistanceMoveForward;\n\t}\n'
              '\tstatic partial void ModifyUntypedDashPvE(ref ActionSetting setting)\n\t{\n'
              '\t\t//setting.SpecialType = SpecialActionType.HostileMovingForward;\n'
              '\t\tsetting.IsFriendly = true;\n\t}\n'
              '\tstatic partial void ModifyTypedGroundMovePvE(ref ActionSetting setting)\n\t{\n'
              '\t\tsetting.SpecialType = SpecialActionType.ObjectBasedMovement;\n\t}\n'
              '\tstatic partial void ModifyRushPvP(ref ActionSetting setting)\n\t{\n'
              '\t\t//setting.SpecialType = SpecialActionType.MovingForward;\n\t}\n')
    types = special_types([source])
    accepted = {'GroundMovePvE': 'test', 'TypedGroundMovePvE': 'test'}
    problems = compare(rows, ids, types, accepted)
    assert types == {'DashPvE': 'FixedDistanceMoveForward', 'UntypedDashPvE': None,
                     'TypedGroundMovePvE': 'ObjectBasedMovement', 'RushPvP': None}, types
    assert any(p.startswith('UNTYPED  UntypedDashPvE') for p in problems), problems
    assert any(p.startswith('UNTYPED  RushPvP') for p in problems), problems
    assert any(p.startswith('STALE    TypedGroundMovePvE') for p in problems), problems
    assert not any('DashPvE (1' in p or 'GroundMovePvE (3' in p for p in problems), problems
    assert len(problems) == 3, problems
    assert moving_rows(fetch=lambda url: None) is None
    page = {'results': [
        {'row_id': 10, 'fields': {'Name': 'Job Leap', 'ClassJobLevel': 30, 'IsPlayerAction': True}},
        {'row_id': 11, 'fields': {'Name': 'Pet Leap', 'ClassJobLevel': 30, 'IsPlayerAction': False}},
        {'row_id': 12, 'fields': {'Name': 'Cast Duty Dash', 'ClassJobLevel': 0, 'IsPlayerAction': False}},
        {'row_id': 13, 'fields': {'Name': 'Idle Duty Dash', 'ClassJobLevel': 0, 'IsPlayerAction': False}}]}
    fetched = moving_rows(fetch=lambda url: page)
    assert fetched == {10: ('Job Leap', True), 12: ('Cast Duty Dash', False), 13: ('Idle Duty Dash', False)}, fetched
    cast = cast_identifiers(['\t\tif (CastDutyDashPvE.CanUse(out act))\n',
                             '\t\t//if (IdleDutyDashPvE.CanUse(out act))\n',
                             '\t\tif (RushPvP.CanUse(out act))\n'])
    assert cast == {'CastDutyDashPvE', 'RushPvP'}, cast
    measured = measured_rows(fetched, {10: 'JobLeapPvE', 12: 'CastDutyDashPvE', 13: 'IdleDutyDashPvE'}, cast)
    assert measured == {10: 'Job Leap', 12: 'Cast Duty Dash'}, measured
    print('self-test ok: a typed move passes, a commented-out type is caught, an acceptance holds, a stale one is '
          'caught, an unreachable xivapi is told apart; a content action counts once a rotation casts it, not when '
          'the call is commented out; PvP actions are measured as PvE ones')


def main():
    self_test()
    rows = moving_rows()
    if rows is None:
        print('xivapi unreachable: the comparison against the Action sheet is skipped.')
        return 0
    ids = identifiers(RESX.read_text(encoding='utf-8-sig'))
    bases = sorted(BASIC.glob('*.cs')) + sorted(DUTIES.glob('*.cs'))
    types = special_types(p.read_text(encoding='utf-8-sig') for p in bases)
    cast = cast_identifiers(p.read_text(encoding='utf-8-sig') for p in sorted(CODE.rglob('*.cs'))
                            if 'obj' not in p.parts and 'bin' not in p.parts)
    rows = measured_rows(rows, ids, cast)
    problems = compare(rows, ids, types, ACCEPTED)
    print(f'{len(rows)} PvE and PvP actions move someone (AffectsPosition) - job actions and the content actions a rotation '
          f'casts; {len(ACCEPTED)} accepted without a type.')
    if problems:
        print('\n'.join(problems))
        print('An action that moves the player needs a moving SpecialType, or the movement safety check never runs '
              'for it (concept 05, A498, A519).')
        return 1
    print('Every one of them carries a movement type, or is accepted with its reason.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
