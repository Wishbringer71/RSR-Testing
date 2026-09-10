#!/usr/bin/env python3
"""Phase 11: status settings whose polarity does not match the character they are checked against.

ActionSetting has four status fields, and they are read against two different characters:

    StatusProvide, StatusNeed              -> Player.Object   (ActionBasicInfo.IsStatusProvided /
                                                               IsStatusNeeded)
    TargetStatusProvide, TargetStatusNeed  -> the target       (ActionTargetInfo.CheckStatus)

Put a status the player can never carry into a player-side field and nothing fails. StatusProvide
degrades to a lockout that never locks; StatusNeed degrades to a condition that never holds, which
blocks the action outright for the life of the build.

The anchor cases, all from the same shape of copy:

    RedMageRotation ModifyEnchantedZwerchhauPvP    StatusProvide = [EnchantedZwerchhau_3238]
    RedMageRotation ModifyEnchantedRedoublementPvP StatusProvide = [EnchantedRedoublement_3239]

3237-3239 are the damage-over-time debuffs on the target; the barriers on the player are 3234-3236,
and the neighbouring ModifyEnchantedRipostePvP picks from that half correctly.

The heuristic is the ↑/↓ marker the status generator writes into each member's doc block: a member
that only ever appears as ↓ is a debuff. A debuff in a player-side field is worth looking at, but it
is NOT by itself a defect - three legitimate shapes exist in this tree and the scan reports them too:

  * a debuff the action genuinely puts on the player (DarkKnightRotation's WalkingDead, Bozja's
    Heavy from Lost Manawall),
  * StatusProvide used as a deliberate lockout rather than as "already applied" (SprintPenalty on
    Sprint, Bind on Hell's Ingress - jumping while bound is pointless),
  * a status whose polarity differs between the two ids sharing its name, which scan10.py covers.

The target-side rule has a legitimate shape of its own: a dispel names the buffs it strips in
TargetStatusNeed on a hostile action (BlueMageRotation's Eerie Soundwave).

So this prints a worklist, not a verdict. Resolve each against what the action does - and against
what the action is for. Reviewing this list the first time, the obvious repair (move the setting to
the matching side) turned out to be wrong for both Blue Mage entries: Magic Hammer is a 250-potency
filler that also restores 1000 MP, and Peripheral Synthesis gains potency while its debuff is up, so
a lockout on either would cost more than the refresh it saves. Both settings were removed instead.

Usage: python3 .github/scripts/audit/scan11.py
"""
import re
import subprocess
import sys

ROOTS = ('RotationSolver.Basic/', 'RotationSolver/')
RESX = 'RotationSolver.SourceGenerators/Properties/Status.resx'

SEE = re.compile(r'&lt;strong&gt;[^&]+&lt;/strong&gt;&lt;/see&gt;\s*(?P<pol>[↑↓])')
PARA = re.compile(r'&lt;para&gt;(?P<text>.*?)&lt;/para&gt;')
MEMBER = re.compile(r'^(?P<name>[A-Za-z_]\w*)\s*=\s*(?P<id>\d+),')

BLOCK = re.compile(r'^\s*static\s+partial\s+void\s+Modify(?P<action>\w+)\s*\(\s*ref\s+ActionSetting')
ASSIGN = re.compile(r'setting\.(?P<field>StatusProvide|StatusNeed|TargetStatusProvide|TargetStatusNeed)\s*=')
FRIENDLY = re.compile(r'setting\.IsFriendly\s*=\s*(?P<value>true|false)')
IDENT = re.compile(r'StatusID\.(?P<name>[A-Za-z_]\w*)')

PLAYER_SIDE = ('StatusProvide', 'StatusNeed')


def strip_comment(raw):
    return '' if raw.lstrip().startswith('//') else raw


def parse_status_enum(path):
    """{identifier: (id, set of polarities, effect text)}."""
    members, pols, para = {}, [], ''
    with open(path, encoding='utf-8') as fh:
        for raw in fh:
            m = SEE.search(raw)
            if m:
                pols.append(m.group('pol'))
            p = PARA.search(raw)
            if p:
                para = p.group('text')
            m = MEMBER.match(raw.strip())
            if m:
                if pols:
                    members[m.group('name')] = (int(m.group('id')), set(pols), para)
                pols, para = [], ''
    return members


def tracked_files():
    out = subprocess.run(['git', 'ls-files', '*.cs'],
                         capture_output=True, text=True, check=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(ROOTS)]


def parse_blocks(lines):
    """Yield (action name, start line, [(field, line no, [identifiers])], friendly) per Modify block.

    Blocks are delimited by the next Modify declaration rather than by brace matching: every one of
    them sits at the same nesting level in these generated partial files, and an assignment can span
    lines inside a collection initialiser."""
    starts = [(no, m.group('action')) for no, raw in enumerate(lines, 1)
              for m in [BLOCK.match(raw)] if m]
    for i, (start, action) in enumerate(starts):
        end = starts[i + 1][0] - 1 if i + 1 < len(starts) else len(lines)
        body = lines[start:end]
        friendly = None
        fields = []
        for offset, raw in enumerate(body, start + 1):
            line = strip_comment(raw)
            if not line:
                continue
            f = FRIENDLY.search(line)
            if f:
                friendly = f.group('value') == 'true'
            a = ASSIGN.search(line)
            if a:
                names = [m.group('name') for m in IDENT.finditer(line)]
                fields.append((a.group('field'), offset, names))
        if fields:
            yield action, start, fields, friendly


def findings(files, members):
    out = []
    for path in files:
        try:
            with open(path, encoding='utf-8-sig') as fh:
                lines = fh.readlines()
        except OSError:
            continue
        for action, _, fields, friendly in parse_blocks(lines):
            for field, no, names in fields:
                for name in names:
                    if name not in members:
                        continue
                    sid, pols, desc = members[name]
                    if field in PLAYER_SIDE and pols == {'↓'}:
                        out.append((path, no, action, field, name, sid, desc,
                                    'debuff in a player-side field'))
                    elif field not in PLAYER_SIDE and pols == {'↑'} and friendly is False:
                        out.append((path, no, action, field, name, sid, desc,
                                    'buff in a target-side field of a hostile action'))
    return out


def self_test():
    sample = [
        '/// &lt;see href="x/3238"&gt;&lt;strong&gt;Enchanted Zwerchhau&lt;/strong&gt;&lt;/see&gt; ↓ (RDM)',
        '/// &lt;para&gt;Suffering damage over time.&lt;/para&gt;',
        'EnchantedZwerchhau_3238 = 3238,',
        '/// &lt;see href="x/3235"&gt;&lt;strong&gt;Enchanted Zwerchhau&lt;/strong&gt;&lt;/see&gt; ↑ (RDM)',
        '/// &lt;para&gt;A magicked barrier is nullifying damage.&lt;/para&gt;',
        'EnchantedZwerchhau = 3235,',
    ]
    members, pols, para = {}, [], ''
    for raw in sample:
        m = SEE.search(raw)
        if m:
            pols.append(m.group('pol'))
        p = PARA.search(raw)
        if p:
            para = p.group('text')
        m = MEMBER.match(raw.strip())
        if m and pols:
            members[m.group('name')] = (int(m.group('id')), set(pols), para)
            pols, para = [], ''
    assert members['EnchantedZwerchhau_3238'][1] == {'↓'}
    assert members['EnchantedZwerchhau'][1] == {'↑'}

    code = [
        '\tstatic partial void ModifyEnchantedZwerchhauPvP(ref ActionSetting setting)\n',
        '\t{\n',
        '\t\tsetting.StatusProvide = [StatusID.EnchantedZwerchhau_3238];\n',
        '\t}\n',
        '\tstatic partial void ModifyGoodOnePvP(ref ActionSetting setting)\n',
        '\t{\n',
        '\t\tsetting.StatusProvide = [StatusID.EnchantedZwerchhau];\n',
        '\t}\n',
        '\tstatic partial void ModifyCommentedOutPvP(ref ActionSetting setting)\n',
        '\t{\n',
        '\t\t// setting.StatusProvide = [StatusID.EnchantedZwerchhau_3238];\n',
        '\t}\n',
    ]
    blocks = {a: (f, fr) for a, _, f, fr in parse_blocks(code)}
    assert set(blocks) == {'EnchantedZwerchhauPvP', 'GoodOnePvP'}, blocks
    assert blocks['EnchantedZwerchhauPvP'][0][0][2] == ['EnchantedZwerchhau_3238']

    # findings() reads files, so exercise the rule directly on the parsed blocks.
    reported = [name for _, _, names in blocks['EnchantedZwerchhauPvP'][0] for name in names
                if members[name][1] == {'↓'}]
    assert reported == ['EnchantedZwerchhau_3238'], reported
    ok = [name for _, _, names in blocks['GoodOnePvP'][0] for name in names
          if members[name][1] == {'↓'}]
    assert ok == [], ok

    print('self-test ok: blocks split, comments dropped, debuff told from buff\n')


def main():
    self_test()
    members = parse_status_enum(RESX)
    if not members:
        print(f'no status members parsed from {RESX}')
        return 1
    hits = findings(tracked_files(), members)
    print(f'{len(members)} status members parsed.\n')
    if not hits:
        print('No status setting sits on the wrong side.')
        return 0

    print(f'{len(hits)} status setting(s) to resolve by hand:\n')
    for path, no, action, field, name, sid, desc, why in hits:
        print(f'{path}:{no}  Modify{action}')
        print(f'    {field} = StatusID.{name} ({sid}) - "{desc}"')
        print(f'    {why}\n')
    print('A debuff in a player-side field is legitimate when the action really puts it on the\n'
          'player, or when StatusProvide is used as a deliberate lockout. Read the action first.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
