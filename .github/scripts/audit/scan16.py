#!/usr/bin/env python3
"""Phase 16: actions whose second effect the rotation never reads.

Concept 08 names the shape: an action with two effects is filed under one of them. Holy is a damage
spell that also stuns, and the stun was not part of any decision until this fork added one; Assize
is an attack oGCD that also heals; Arm's Length is filed as knockback protection and its Slow +20%
on every attacker is read nowhere at all.

The scan takes the effect text of every PvE action from `ActionId.resx`, extracts the control and
mitigation effects it inflicts on enemies - Slow, Stun, Heavy, Bind, Blind, Silence, Paralysis,
"damage dealt is reduced" - and asks whether the tree ever reads the matching status. Reading it
means one of two things:

  * the status identifier appears in `StatusHelper.cs`, i.e. it is part of a named list, or
  * it appears in a rotation or helper as `StatusID.<name>`.

An effect nobody reads is not automatically a defect. Plenty of them belong to jobs outside the
owner's profile, to PvP, or to duty actions, and plenty are simply not worth a decision. What the
list is for is the opposite question - which effects the plugin could act on but currently cannot
see, because nothing in the tree looks at them.

The output separates the roles that matter for this fork's owner (tank and healer PvE) from the
rest, because the project rule is that the survey is complete while the work is not: a finding
outside the profile is recorded, not acted on.

Usage: python3 .github/scripts/audit/scan16.py [--all]
       --all   also list the effects that are read, and the ones outside tank/healer
"""
import os
import re
import sys
from collections import defaultdict

ACTIONS = 'RotationSolver.SourceGenerators/Properties/ActionId.resx'
STATUSES = 'RotationSolver.SourceGenerators/Properties/Status.resx'
HELPER = 'RotationSolver.Basic/Helpers/StatusHelper.cs'
CODE_DIRS = ['RotationSolver.Basic', 'RotationSolver']

# Effects an action can put on an enemy that the plugin could act on. The key is the status name
# the game uses, the value the phrases that announce it in an action's description.
CONTROL = {
    'Slow': [r'\bslow\b'],
    'Stun': [r'\bstun\b'],
    'Heavy': [r'\bheavy\b'],
    'Bind': [r'\bbind\b'],
    'Blind': [r'\bblind\b'],
    'Silence': [r'\bsilence\b'],
    'Paralysis': [r'\bparaly'],
}
DAMAGE_DOWN = re.compile(r'reduces damage dealt by|damage dealt (?:by [^.]{0,40})?is reduced', re.I)

TANK_JOBS = {'PLD', 'WAR', 'DRK', 'GNB', 'GLA', 'MRD'}
HEALER_JOBS = {'WHM', 'SCH', 'AST', 'SGE', 'CNJ'}


def parse_actions(path):
    """[(identifier, id, scope, text)] for every PvE action."""
    see = re.compile(r'&lt;strong&gt;(?P<disp>[^&]+)&lt;/strong&gt;&lt;/see&gt;.*?'
                     r'\((?P<scope>[^)]*)\)')
    para = re.compile(r'&lt;para&gt;(?P<text>.*?)&lt;/para&gt;')
    member = re.compile(r'^(?P<name>[A-Za-z_]\w*)\s*=\s*(?P<id>\d+),')
    out, scope, text, disp = [], '', '', ''
    with open(path, encoding='utf-8') as fh:
        for raw in fh:
            m = see.search(raw)
            if m:
                disp, scope = m.group('disp'), m.group('scope')
            p = para.search(raw)
            if p:
                text = p.group('text')
            m = member.match(raw.strip())
            if m:
                if m.group('name').endswith('PvE'):
                    out.append((m.group('name'), int(m.group('id')), scope, text, disp))
                scope, text, disp = '', '', ''
    return out


def status_names(path):
    """{base name: [identifiers]} - Slow -> [Slow, Slow_10, ...]."""
    member = re.compile(r'^(?P<name>[A-Za-z_]\w*)\s*=\s*(?P<id>\d+),')
    groups = defaultdict(list)
    with open(path, encoding='utf-8') as fh:
        for raw in fh:
            m = member.match(raw.strip())
            if m:
                groups[re.sub(r'_\d+$', '', m.group('name'))].append(m.group('name'))
    return groups


def read_sites(idents):
    """{identifier: [files that mention it]} across the code, helper listed first."""
    wanted = {i: [] for i in idents}
    pattern = re.compile(r'StatusID\.(\w+)')
    for directory in CODE_DIRS:
        for root, dirs, names in os.walk(directory):
            dirs[:] = [d for d in dirs if d not in ('obj', 'bin')]
            for name in names:
                if not name.endswith('.cs'):
                    continue
                path = os.path.join(root, name)
                with open(path, encoding='utf-8', errors='replace') as fh:
                    body = fh.read()
                for hit in set(pattern.findall(body)):
                    if hit in wanted:
                        wanted[hit].append(path)
    return wanted


def roles_of(scope):
    jobs = set(scope.replace(',', ' ').split())
    role = []
    if jobs & TANK_JOBS:
        role.append('tank')
    if jobs & HEALER_JOBS:
        role.append('healer')
    return role


def effects_in(text):
    found = []
    for status, patterns in CONTROL.items():
        if any(re.search(p, text, re.I) for p in patterns):
            found.append(status)
    if DAMAGE_DOWN.search(text):
        found.append('(damage down)')
    return found


def self_test():
    assert effects_in('the striker will be afflicted with Slow +20%') == ['Slow']
    assert effects_in('Delivers an attack. Additional Effect: Stun') == ['Stun']
    assert 'Slow' not in effects_in('Restores own HP.'), 'a plain heal must not read as control'
    assert effects_in('Reduces damage dealt by target by 10%') == ['(damage down)']
    assert roles_of('GLA MRD PLD WAR DRK GNB') == ['tank']
    assert roles_of('CNJ WHM') == ['healer']
    assert roles_of('BLM SMN RDM') == []
    print('self-test ok: control effects recognised, plain effects rejected, roles mapped\n')


def main(argv):
    verbose = '--all' in argv
    self_test()

    actions = parse_actions(ACTIONS)
    groups = status_names(STATUSES)
    with open(HELPER, encoding='utf-8') as fh:
        helper_body = fh.read()

    interesting = set()
    for status in CONTROL:
        interesting.update(groups.get(status, []))
    sites = read_sites(interesting)

    listed, elsewhere, unread = [], [], []
    for name, aid, scope, text, disp in actions:
        found = effects_in(text)
        if not found:
            continue
        role = roles_of(scope)
        for status in found:
            if status == '(damage down)':
                state = 'measured' if 'GetCurrentMitigationPercent' in helper_body else 'n/a'
                row = (role, name, aid, disp, status, 'read by the mitigation survey')
                (listed if state == 'measured' else elsewhere).append(row)
                continue
            idents = groups.get(status, [])
            in_helper = [i for i in idents if re.search(rf'StatusID\.{i}\b', helper_body)]
            in_code = sorted({p for i in idents for p in sites.get(i, [])
                              if not p.endswith('StatusHelper.cs')})
            if in_helper:
                listed.append((role, name, aid, disp, status,
                               f'{len(in_helper)} id(s) in a StatusHelper list'))
            elif in_code:
                elsewhere.append((role, name, aid, disp, status,
                                  f'only read directly, in {len(in_code)} file(s)'))
            else:
                unread.append((role, name, aid, disp, status, 'not read anywhere'))

    def show(title, rows, only_profile):
        rows = [r for r in rows if (r[0] if only_profile else not r[0])]
        if not rows:
            return 0
        print(f'--- {title} ({len(rows)})')
        for role, name, aid, disp, status, note in sorted(rows, key=lambda r: (r[3], r[4])):
            where = '/'.join(role) if role else 'other roles'
            print(f'    {disp} [{where}] {status}: {note}')
            print(f'        {name} ({aid})')
        print()
        return len(rows)

    print(f'{len(actions)} PvE actions, {len(listed) + len(elsewhere) + len(unread)} carrying a '
          f'control or damage-down effect.\n')

    print('=== Tank and healer - the roles this fork is maintained for\n')
    gap = show('effect the tree never reads', unread, True)
    gap += show('effect read only as a single id, never as a group', elsewhere, True)
    if verbose:
        show('effect covered by a StatusHelper list', listed, True)

    print('=== Other roles - recorded, not worked on\n')
    other = show('effect the tree never reads', unread, False)
    if verbose:
        show('effect read only as a single id', elsewhere, False)
        show('effect covered by a StatusHelper list', listed, False)

    print(f'{gap} effect(s) inside the tank/healer profile the plugin cannot currently act on, '
          f'{other} outside it.\nAn unread effect is a candidate, not a defect: it is worth a '
          f'decision only where acting on it\nwould change what the rotation does.')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
