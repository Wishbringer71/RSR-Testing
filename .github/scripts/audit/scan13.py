#!/usr/bin/env python3
"""Phase 13: barrier statuses missing from StatusHelper.ShieldStatus.

`ShieldStatus` is what `HasSurvivingShield` and, through `GetEffectiveHpPercent`, the
healing decision read to credit a shield toward a target's effective health. A barrier
missing from that list makes its bearer look more hurt than they are, so the healer
spends a cast that was not needed.

The list is hand-maintained, and the game gives most barriers **several** status ids
under one display name - one per version of the ability, plus PvP forms. Naming one and
omitting the siblings is the same shape of ageing as any other enumeration: correct when
written, quietly incomplete after the next expansion. `Galvanize` is listed while
`Galvanize_3087` is not; `BlackestNight` while `BlackestNight_1308` is not.

Two questions, both answered here:

  * **Missing siblings** - a display name that is represented in the list, but not by all
    of its ids.
  * **Missing groups** - a barrier whose display name appears in the list not at all.

Membership is decided on the effect text, not the name: a status counts as a barrier when
its text says a barrier/shield is nullifying or preventing damage. That deliberately
excludes the neighbours that merely reduce damage (`Catalyze_3088`, "Damage taken is
reduced") and the ones that only *set up* a barrier for later (`DivineVeil` 726, "Upon HP
recovery ... a barrier is created"). Both would otherwise be swept in by their names.

The scan reports; it does not decide. A hit is a candidate, and the effect text printed
with it is what the reader judges - a PvP-only form is harmless to add but pointless, and
a stack counter that restores a barrier is not itself one.

Telling the PvE form from the PvP one is what the status text alone cannot do: both carry
the same display name and the same "a magicked barrier is nullifying damage". The action
resx can. Each candidate is therefore matched against the action of the same name, and the
label says what that action does in PvE:

  * **PvE barrier** - a `<name>PvE` action exists and its description creates a barrier.
  * **PvE action grants no barrier** - a `<name>PvE` action exists but only mitigates, so
    the barrier id belongs to the PvP form. `Aquaveil` and `Holy Sheltron` are the cases
    that motivated this: in PvE both reduce damage taken, and only their PvP versions put
    up a barrier. Both had been listed as PvE candidates before this check existed.
  * **PvP only** / **no action of this name** - a PvP-exclusive action, or a duty, item or
    removed-job effect with no player action behind it.

Usage: python3 .github/scripts/audit/scan13.py
"""
import re
import sys
from collections import defaultdict

RESX = 'RotationSolver.SourceGenerators/Properties/Status.resx'
ACTION_RESX = 'RotationSolver.SourceGenerators/Properties/ActionId.resx'
HELPER = 'RotationSolver.Basic/Helpers/StatusHelper.cs'

SEE = re.compile(r'&lt;strong&gt;(?P<disp>[^&]+)&lt;/strong&gt;&lt;/see&gt;\s*(?P<pol>[↑↓])'
                 r'\s*\((?P<scope>[^)]*)\)')
PARA = re.compile(r'&lt;para&gt;(?P<text>.*?)&lt;/para&gt;')
MEMBER = re.compile(r'^(?P<name>[A-Za-z_]\w*)\s*=\s*(?P<id>\d+),')

# A barrier absorbs damage. "Damage taken is reduced" on its own is mitigation, and a
# status that creates a barrier on some later event is not yet one.
IS_BARRIER = re.compile(
    r'\b(?:barrier|shield|shadows|darkness)\b[^.]*\b(?:nullif|prevent|absorb)\w*\s+damage'
    r'|\bnullifying damage\b',
    re.IGNORECASE)
SETS_UP_LATER = re.compile(r'\bupon\b[^.]*\bis created\b', re.IGNORECASE)


def parse_status_enum(path):
    """{identifier: (id, display name, scope, effect text)}."""
    members, pending, para = {}, [], ''
    with open(path, encoding='utf-8') as fh:
        for raw in fh:
            m = SEE.search(raw)
            if m:
                pending.append((m.group('disp'), m.group('scope')))
            p = PARA.search(raw)
            if p:
                para = p.group('text')
            m = MEMBER.match(raw.strip())
            if m:
                if pending:
                    members[m.group('name')] = (int(m.group('id')), pending[0][0],
                                                pending[0][1], para)
                pending, para = [], ''
    return members


def listed_shields(path):
    src = open(path, encoding='utf-8').read()
    m = re.search(r'ShieldStatus\s*\{\s*get;\s*\}\s*=\s*\[(.*?)\];', src, re.S)
    if not m:
        return set()
    return set(re.findall(r'StatusID\.(\w+)', m.group(1)))


def is_barrier(text):
    return bool(IS_BARRIER.search(text)) and not SETS_UP_LATER.search(text)


# An action doc block: display name, PvE/PvP marker, description paragraph, then the member.
ACTION = re.compile(
    r'&lt;i&gt;(?P<kind>PvE|PvP)&lt;/i&gt;'
    r'(?:(?!^\s*\w+ = \d+,).)*?'
    r'(?:&lt;para&gt;(?P<desc>.*?)&lt;/para&gt;(?:(?!^\s*\w+ = \d+,).)*?)?'
    r'^\s*(?P<name>\w+) = \d+,',
    re.S | re.M)
# A barrier in an action description: it creates, erects or absorbs, not merely reduces.
ACTION_BARRIER = re.compile(r'\bbarrier\b|\bnullif\w+ damage\b|\babsorbs?\b', re.IGNORECASE)


def parse_action_enum(path):
    """{action identifier: (PvE|PvP, description text)}."""
    raw = open(path, encoding='utf-8').read()
    out = {}
    for m in ACTION.finditer(raw):
        desc = re.sub(r'&lt;.*?&gt;|<.*?>', ' ', m.group('desc') or '')
        out[m.group('name')] = (m.group('kind'), ' '.join(desc.split()))
    return out


def classify(ident, actions):
    """What the action of the same name does in PvE - see the module docstring."""
    base = re.sub(r'_\d+$', '', ident)
    pve = actions.get(base + 'PvE')
    if pve:
        return ('PvE barrier' if ACTION_BARRIER.search(pve[1])
                else 'PvE action grants no barrier')
    return 'PvP only' if base + 'PvP' in actions else 'no action of this name'


def self_test():
    cases = [
        ('A magicked barrier is nullifying damage.', True),
        ('An all-encompassing darkness is nullifying damage.', True),
        ('An aetherial barrier is preventing damage.', True),
        ('Shadows are nullifying damage.', True),
        ('A highly effective defensive maneuver is nullifying damage.', True),
        ('Damage taken is reduced and a magicked barrier is nullifying damage.', True),
        # Mitigation, not a barrier.
        ('Damage taken is reduced.', False),
        # Creates one later; not one yet.
        ('Upon HP recovery via healing magic a damage-reducing barrier is created.', False),
        # A counter that refills a barrier is not the barrier.
        ('Stacks are consumed to restore the Haima barrier each time it is absorbed.', False),
        ('Most attacks cannot reduce your HP to less than 1.', False),
    ]
    for text, want in cases:
        assert is_barrier(text) == want, (text, is_barrier(text), want)

    doc = '\n'.join([
        '/// &lt;see href="x/1178"&gt;&lt;strong&gt;Blackest Night&lt;/strong&gt;&lt;/see&gt; ↑ (DRK)',
        '/// &lt;para&gt;An all-encompassing darkness is nullifying damage.&lt;/para&gt;',
        'BlackestNight = 1178,',
        '/// &lt;see href="x/1308"&gt;&lt;strong&gt;Blackest Night&lt;/strong&gt;&lt;/see&gt; ↑ (DRK)',
        '/// &lt;para&gt;An all-encompassing darkness is nullifying damage.&lt;/para&gt;',
        'BlackestNight_1308 = 1308,',
    ])
    import tempfile, os
    fd, tmp = tempfile.mkstemp(suffix='.resx')
    with os.fdopen(fd, 'w', encoding='utf-8') as fh:
        fh.write(doc)
    try:
        members = parse_status_enum(tmp)
    finally:
        os.unlink(tmp)
    assert set(members) == {'BlackestNight', 'BlackestNight_1308'}, members
    assert members['BlackestNight_1308'][:3] == (1308, 'Blackest Night', 'DRK')

    # The PvE/PvP split, against constructed actions of both shapes.
    adoc = '\n'.join([
        '/// &lt;see href="x/1"&gt;&lt;strong&gt;Divine Caress&lt;/strong&gt;&lt;/see&gt; &lt;i&gt;PvE&lt;/i&gt; (WHM)',
        '/// &lt;para&gt;Creates a barrier around self and all party members near you.&lt;/para&gt;',
        'DivineCaressPvE = 1,',
        '/// &lt;see href="x/2"&gt;&lt;strong&gt;Aquaveil&lt;/strong&gt;&lt;/see&gt; &lt;i&gt;PvE&lt;/i&gt; (WHM)',
        '/// &lt;para&gt;Reduces damage taken by a party member or self by 15%.&lt;/para&gt;',
        'AquaveilPvE = 2,',
        '/// &lt;see href="x/3"&gt;&lt;strong&gt;Epicycle&lt;/strong&gt;&lt;/see&gt; &lt;i&gt;PvP&lt;/i&gt; (AST)',
        '/// &lt;para&gt;Creates a barrier around self.&lt;/para&gt;',
        'EpicyclePvP = 3,',
    ])
    fd, tmp = tempfile.mkstemp(suffix='.resx')
    with os.fdopen(fd, 'w', encoding='utf-8') as fh:
        fh.write(adoc)
    try:
        actions = parse_action_enum(tmp)
    finally:
        os.unlink(tmp)
    assert set(actions) == {'DivineCaressPvE', 'AquaveilPvE', 'EpicyclePvP'}, actions
    assert classify('DivineCaress', actions) == 'PvE barrier'
    # The case this check exists for: the PvE action mitigates, the barrier id is the PvP form.
    assert classify('Aquaveil_3086', actions) == 'PvE action grants no barrier'
    assert classify('Epicycle', actions) == 'PvP only'
    assert classify('GuardiansWill', actions) == 'no action of this name'

    print('self-test ok: barriers told from mitigation, from set-up statuses '
          'and from PvP-only forms\n')


def main():
    self_test()
    members = parse_status_enum(RESX)
    listed = listed_shields(HELPER)
    actions = parse_action_enum(ACTION_RESX)
    if not members or not listed or not actions:
        print('could not parse the status enum, ShieldStatus or the action enum')
        return 1

    by_name = defaultdict(list)
    for name, (sid, disp, scope, text) in members.items():
        by_name[disp].append((name, sid, scope, text))

    listed_names = {members[n][1] for n in listed if n in members}

    siblings, groups = [], []
    for disp, entries in sorted(by_name.items()):
        barriers = [e for e in entries if is_barrier(e[3])]
        if not barriers:
            continue
        missing = [e for e in barriers if e[0] not in listed]
        if not missing:
            continue
        (siblings if disp in listed_names else groups).append((disp, entries, missing))

    print(f'{len(listed)} ids in ShieldStatus, {len(by_name)} display names in the enum.\n')

    def show(title, rows):
        print(f'=== {title}: {len(rows)} group(s), '
              f'{sum(len(m) for _, _, m in rows)} id(s)\n')
        for disp, entries, missing in rows:
            have = [n for n, _, _, _ in entries if n in listed]
            print(f'  "{disp}"' + (f'   listed: {have}' if have else '   (nothing listed)'))
            for name, sid, scope, text in sorted(missing, key=lambda e: e[1]):
                print(f'      {name} ({sid}) ({scope}) [{classify(name, actions)}]'
                      f' - {text[:90]}')
            print()

    show('Missing siblings of a listed barrier', siblings)
    show('Barrier groups absent from ShieldStatus entirely', groups)

    total = sum(len(m) for _, _, m in siblings) + sum(len(m) for _, _, m in groups)
    if not total:
        print('ShieldStatus covers every barrier id in the enum.')
        return 0
    print(f'{total} candidate id(s). Read the effect text before adding any: a PvP-only\n'
          'form is harmless but pointless, and the scope in brackets says which job or\n'
          'content it belongs to. The label after the scope says what the action of the\n'
          'same name does in PvE - "PvE action grants no barrier" means the id is the PvP\n'
          'form of an ability that only mitigates in PvE.')
    pve = sorted((sid, name, scope) for _, _, missing in siblings + groups
                 for name, sid, scope, _ in missing
                 if classify(name, actions) == 'PvE barrier')
    print(f'\nOf those, {len(pve)} sit behind a PvE action that creates a barrier:')
    for sid, name, scope in pve:
        print(f'  {name} ({sid}) ({scope})')
    return 0


if __name__ == '__main__':
    sys.exit(main())
