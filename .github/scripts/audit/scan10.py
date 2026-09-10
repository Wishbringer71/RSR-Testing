#!/usr/bin/env python3
"""Phase 10: status identifiers whose display name is shared by a status of opposite polarity.

Square Enix gives several statuses the same in-game name and distinguishes them only by id, and the
generated StatusID enum mirrors that: the first one keeps the bare name, the rest get an `_<id>`
suffix. Where such a group mixes a buff and a debuff, picking the identifier by name gets the wrong
effect, and nothing fails - the check simply never fires, or fires on the wrong character.

The anchor case: `StatusID.Holmgang` is id 88, "Unable to move until effect fades" - the movement
debuff Holmgang used to leave on the warrior's *target*. The protection on the warrior himself is id
409, `Holmgang_409`, "Most attacks cannot reduce your HP to less than 1". Five heal-lockout checks in
the Beiruta rotations asked for 88, so they never once fired for a warrior in Holmgang. The central
lists in StatusHelper had 409 all along, which is what made the mismatch visible at all.

The same trap caught this project's own audit once from the other side: a null result was reported
over the identifier `HallowedGround` without reading what the neighbouring ids do (AUDIT_LOG C15).

The scan reports every use of an identifier from a mixed-polarity name group, with the group's other
members, so the choice can be resolved against the effect text rather than the name. A use is not by
itself wrong - most of these are correct - but each one is a decision that the identifier alone does
not document.

Usage: python3 .github/scripts/audit/scan10.py
"""
import re
import subprocess
import sys
from collections import defaultdict

ROOTS = ('RotationSolver.Basic/', 'RotationSolver/')
RESX = 'RotationSolver.SourceGenerators/Properties/Status.resx'

# The generated doc block is HTML-escaped inside the resx value.
SEE = re.compile(r'&lt;strong&gt;(?P<disp>[^&]+)&lt;/strong&gt;&lt;/see&gt;\s*(?P<pol>[↑↓])')
PARA = re.compile(r'&lt;para&gt;(?P<text>.*?)&lt;/para&gt;')
MEMBER = re.compile(r'^(?P<name>[A-Za-z_]\w*)\s*=\s*(?P<id>\d+),')
USE = re.compile(r'\bStatusID\.(?P<name>[A-Za-z_]\w*)')


def strip_comment(raw):
    """Drop a whole-line comment. This file's own findings are described in code comments that name
    the very identifiers being scanned for."""
    return '' if raw.lstrip().startswith('//') else raw


def parse_status_enum(path):
    """{identifier: (id, [(display name, polarity)], effect text)} for every generated member."""
    members = {}
    pending, para = [], ''
    with open(path, encoding='utf-8') as fh:
        for raw in fh:
            m = SEE.search(raw)
            if m:
                pending.append((m.group('disp'), m.group('pol')))
            p = PARA.search(raw)
            if p:
                para = p.group('text')
            m = MEMBER.match(raw.strip())
            if m:
                if pending:
                    members[m.group('name')] = (int(m.group('id')), tuple(pending), para)
                pending, para = [], ''
    return members


def mixed_polarity_groups(members):
    """Display names carried by both a buff and a debuff, as {identifier: display name}."""
    groups = defaultdict(list)
    for name, (sid, pairs, _) in members.items():
        for disp, pol in pairs:
            groups[disp].append((name, sid, pol))
    flagged = {}
    for disp, entries in groups.items():
        if len({pol for _, _, pol in entries}) > 1:
            for name, _, _ in entries:
                flagged[name] = disp
    return flagged, groups


def tracked_files():
    out = subprocess.run(['git', 'ls-files', '*.cs'],
                         capture_output=True, text=True, check=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(ROOTS)]


def uses(files, flagged):
    hits = defaultdict(list)
    for path in files:
        try:
            with open(path, encoding='utf-8-sig') as fh:
                lines = fh.readlines()
        except OSError:
            continue
        for no, raw in enumerate(lines, 1):
            line = strip_comment(raw)
            if not line:
                continue
            for m in USE.finditer(line):
                name = m.group('name')
                if name in flagged:
                    hits[name].append(f'{path}:{no}  {line.strip()[:110]}')
    return hits


def self_test():
    sample = [
        '/// &lt;see href="x/88"&gt;&lt;strong&gt;Holmgang&lt;/strong&gt;&lt;/see&gt; ↓ (All Classes)',
        '/// &lt;para&gt;Unable to move until effect fades.&lt;/para&gt;',
        'Holmgang = 88,',
        '/// &lt;see href="x/409"&gt;&lt;strong&gt;Holmgang&lt;/strong&gt;&lt;/see&gt; ↑ (All Classes)',
        '/// &lt;para&gt;Most attacks cannot reduce your HP to less than 1.&lt;/para&gt;',
        'Holmgang_409 = 409,',
        '/// &lt;see href="x/810"&gt;&lt;strong&gt;Living Dead&lt;/strong&gt;&lt;/see&gt; ↑ (All Classes)',
        'LivingDead = 810,',
    ]
    members, pending, para = {}, [], ''
    for raw in sample:
        m = SEE.search(raw)
        if m:
            pending.append((m.group('disp'), m.group('pol')))
        p = PARA.search(raw)
        if p:
            para = p.group('text')
        m = MEMBER.match(raw.strip())
        if m and pending:
            members[m.group('name')] = (int(m.group('id')), tuple(pending), para)
            pending, para = [], ''
    assert set(members) == {'Holmgang', 'Holmgang_409', 'LivingDead'}, members
    assert members['Holmgang'][0] == 88 and 'Unable to move' in members['Holmgang'][2]
    assert members['Holmgang_409'][2].startswith('Most attacks')

    flagged, _ = mixed_polarity_groups(members)
    # Holmgang mixes a debuff and a buff; Living Dead is alone in its group.
    assert set(flagged) == {'Holmgang', 'Holmgang_409'}, flagged

    assert USE.search('target.HasStatus(false, StatusID.Holmgang) ||').group('name') == 'Holmgang'
    assert USE.search('StatusID.Holmgang_409,').group('name') == 'Holmgang_409'
    # A comment naming the identifier must not count as a use - the fix for the anchor case leaves
    # exactly such comments behind.
    assert strip_comment('\t\t\t// StatusID.Holmgang is id 88, the movement debuff') == ''

    print('self-test ok: mixed-polarity group detected, effect text kept, comments excluded\n')


def main():
    self_test()
    members = parse_status_enum(RESX)
    if not members:
        print(f'no status members parsed from {RESX}')
        return 1
    flagged, groups = mixed_polarity_groups(members)
    files = tracked_files()
    hits = uses(files, flagged)

    print(f'{len(members)} status members, {len(flagged)} of them in a name group that mixes a buff '
          f'and a debuff.\n')
    if not hits:
        print('No such identifier is used in code.')
        return 0

    print(f'{len(hits)} identifier(s) from a mixed group used in {sum(len(v) for v in hits.values())}'
          f' place(s) - resolve each against the effect text:\n')
    for name in sorted(hits):
        sid, _, para = members[name]
        print(f'=== StatusID.{name} (id {sid}) - "{para}"')
        for other, oid, pol in sorted(groups[flagged[name]], key=lambda e: e[1]):
            if other != name:
                print(f'    also named "{flagged[name]}": {other} (id {oid}) {pol} '
                      f'- "{members[other][2]}"')
        for site in hits[name]:
            print(f'      {site}')
        print()
    return 0


if __name__ == '__main__':
    sys.exit(main())
