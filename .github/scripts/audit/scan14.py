#!/usr/bin/env python3
"""Phase 14: status lists in StatusHelper that are missing siblings of an id they already carry.

The game gives most effects several status ids under one display name - one per version of the
ability, plus PvP forms and the versions a trait upgrades into. A hand-maintained list that names
one id and omits its siblings answers *false* for a bearer of the omitted one, which is not lost
precision but an inverted answer. `scan13.py` solves that for `ShieldStatus`; this scan asks the
same question of every list in the file.

The scan reports, it does not decide, and the reason is visible in the output: sharing a display
name does not make two ids the same effect. Three cases from the first run:

  * `Rampart_1978` ("damage taken is reduced **while HP recovered via healing actions is
    increased**", scoped PLD WAR DRK GNB) is the form a tank carries from level 94 - the same
    mitigation, upgraded by a trait. It belongs in `RampartStatus`, and its absence made the list
    miss the most common mitigation in the game.
  * `Nebula_3051` ("inflicting a portion of sustained damage back to its source") and
    `Bloodwhetting_3030` ("weaponskills generate HP equal to the amount of damage dealt") share
    their names with mitigations but are the reflect and lifesteal halves. They do not belong.
  * `Holmgang` 88 and 1305 sit on the *target* of the invulnerability, not on its bearer
    (AUDIT_LOG C15).

To make that judgement cheap, each missing sibling is printed with its own effect text and a marker
saying whether that text starts like the text of the id already listed. A shared prefix is the
signature of a trait upgrade; a different opening is the signature of a different effect.

Usage: python3 .github/scripts/audit/scan14.py
"""
import re
import sys
from collections import defaultdict

RESX = 'RotationSolver.SourceGenerators/Properties/Status.resx'
HELPER = 'RotationSolver.Basic/Helpers/StatusHelper.cs'

LIST = re.compile(r'public static StatusID\[\] (\w+)\s*\{\s*get;\s*\}\s*=\s*\[(.*?)\];', re.S)
PREFIX_WORDS = 4


def parse_status_enum(path):
    """{identifier: (id, display name, scope, effect text)} - same shape as scan13."""
    see = re.compile(r'&lt;strong&gt;(?P<disp>[^&]+)&lt;/strong&gt;&lt;/see&gt;\s*(?P<pol>[↑↓])'
                     r'\s*\((?P<scope>[^)]*)\)')
    para = re.compile(r'&lt;para&gt;(?P<text>.*?)&lt;/para&gt;')
    member = re.compile(r'^(?P<name>[A-Za-z_]\w*)\s*=\s*(?P<id>\d+),')
    members, pending, text = {}, [], ''
    with open(path, encoding='utf-8') as fh:
        for raw in fh:
            m = see.search(raw)
            if m:
                pending.append((m.group('disp'), m.group('scope')))
            p = para.search(raw)
            if p:
                text = p.group('text')
            m = member.match(raw.strip())
            if m:
                if pending:
                    members[m.group('name')] = (int(m.group('id')), pending[0][0],
                                                pending[0][1], text)
                pending, text = [], ''
    return members


def opening(text):
    """The first few words, lowercased - enough to tell a trait upgrade from another effect."""
    return ' '.join(re.sub(r'[^a-z ]', ' ', (text or '').lower()).split()[:PREFIX_WORDS])


def find_lists(path):
    """[(list name, {identifiers})] in file order."""
    src = open(path, encoding='utf-8').read()
    return [(name, set(re.findall(r'StatusID\.(\w+)', body))) for name, body in LIST.findall(src)]


def self_test():
    assert opening('Damage taken is reduced.') == opening(
        'Damage taken is reduced while HP recovered via healing actions is increased.'), \
        'a trait upgrade has to read as the same opening'
    assert opening('Damage taken is reduced.') != opening(
        'Inflicting a portion of sustained damage back to its source.'), \
        'a different effect has to read as a different opening'
    assert opening('') == '', 'a missing text must not crash the comparison'

    import os
    import tempfile
    doc = '\n'.join([
        '/// &lt;see href="x/71"&gt;&lt;strong&gt;Rampart&lt;/strong&gt;&lt;/see&gt; ↑ (All Classes)',
        '/// &lt;para&gt;Damage taken is reduced.&lt;/para&gt;',
        'Rampart = 71,',
        '/// &lt;see href="x/1978"&gt;&lt;strong&gt;Rampart&lt;/strong&gt;&lt;/see&gt; ↑ (PLD WAR DRK GNB)',
        '/// &lt;para&gt;Damage taken is reduced while HP recovered is increased.&lt;/para&gt;',
        'Rampart_1978 = 1978,',
    ])
    fd, tmp = tempfile.mkstemp(suffix='.resx')
    with os.fdopen(fd, 'w', encoding='utf-8') as fh:
        fh.write(doc)
    try:
        members = parse_status_enum(tmp)
    finally:
        os.unlink(tmp)
    assert set(members) == {'Rampart', 'Rampart_1978'}, members
    assert members['Rampart_1978'][:3] == (1978, 'Rampart', 'PLD WAR DRK GNB')

    # The constructed defect: a list carrying only the base id must report its sibling.
    by_name = defaultdict(list)
    for name, value in members.items():
        by_name[value[1].strip().lower()].append(name)
    missing = [s for s in by_name['rampart'] if s not in {'Rampart'}]
    assert missing == ['Rampart_1978'], missing

    print('self-test ok: trait upgrades told from different effects, siblings found\n')


def main():
    self_test()
    members = parse_status_enum(RESX)
    lists = find_lists(HELPER)
    if not members or not lists:
        print('could not parse the status enum or the status lists')
        return 1

    by_name = defaultdict(list)
    for name, value in members.items():
        by_name[value[1].strip().lower()].append(name)

    print(f'{len(lists)} status list(s) in {HELPER}, {len(members)} ids in the enum.\n')

    total_lists, total_ids = 0, 0
    for list_name, listed in lists:
        rows = []
        for name in sorted(listed, key=lambda n: members.get(n, (0,))[0]):
            if name not in members:
                continue
            sid, disp, _, text = members[name]
            for sib in by_name[disp.strip().lower()]:
                if sib in listed or any(sib == r[1] for r in rows):
                    continue
                s_sid, _, s_scope, s_text = members[sib]
                same = opening(s_text) == opening(text)
                rows.append((s_sid, sib, s_scope, s_text, same, name, sid))

        if not rows:
            continue
        total_lists += 1
        total_ids += len(rows)
        print(f'=== {list_name}: {len(listed)} listed, {len(rows)} sibling(s) absent')
        for s_sid, sib, s_scope, s_text, same, src, src_sid in sorted(rows):
            mark = 'same opening ' if same else 'differs     '
            print(f'    {sib} ({s_sid}) ({s_scope}) [{mark}] - {s_text[:78]}')
            print(f'        listed sibling: {src} ({src_sid})')
        print()

    if not total_ids:
        print('Every list carries all siblings of the ids it names.')
        return 0

    print(f'{total_ids} absent sibling(s) across {total_lists} list(s). Read the effect text before '
          'adding any:\n"same opening" is the signature of a trait upgrade and usually belongs; '
          '"differs" is\nanother effect under a shared name and usually does not. A list that is '
          'deliberately a\nsubset - PvP purification, dispellable buffs - will report hits that are '
          'not defects.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
