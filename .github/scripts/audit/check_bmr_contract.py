#!/usr/bin/env python3
"""Do RSR's copies of BossModReborn's enums still carry BossModReborn's ordinals?

`BossModUpdater` casts the integers BossModReborn sends over IPC straight into `SpecialMode` and `PredictedDamageType`
(`RotationSolver/Basic/Data/BossModEnums.cs`): `(PredictedDamageType)damageType`, `(SpecialMode)...`. Nothing in the
compiler ties those numbers to BossModReborn's own enums (`BossMod/BossModule/AIHints.cs`) - should BossModReborn insert
or reorder a member, a raidwide arrives here as a tankbuster, or a no-movement window as Pyretic, and every rule reading
them decides on the wrong event (CLAUDE.md, "Verträge"). This reads BossModReborn's file from its repository and
compares member by member: name and ordinal, in both directions. Before A362 nothing checked it.

The file comes from raw.githubusercontent.com (main branch), tried three times. Unreachable, the check fails with that
reason rather than passing: a silent pass would look like an unchanged contract.

Usage: python3 .github/scripts/audit/check_bmr_contract.py [path-to-AIHints.cs]
Exit code 1 when the ordinals differ, a member is missing on either side, or the file cannot be read.
"""
import re
import sys
import time
import urllib.request
from pathlib import Path

RSR = Path('RotationSolver/Basic/Data/BossModEnums.cs')
BMR_URL = 'https://raw.githubusercontent.com/FFXIV-CombatReborn/BossmodReborn/main/BossMod/BossModule/AIHints.cs'
ENUMS = ('SpecialMode', 'PredictedDamageType')

COMMENT = re.compile(r'//[^\n]*|/\*.*?\*/', re.DOTALL)
DOC_ATTR = re.compile(r'\[[^\]]*\]')


def members(source, name):
    """The members of enum `name` in C# source, name -> ordinal, implicit ordinals counted as the compiler does; None
    when the enum is not there."""
    match = re.search(r'\benum\s+' + name + r'\b[^{]*\{(?P<body>[^}]*)\}', COMMENT.sub('', source))
    if not match:
        return None
    result = {}
    next_value = 0
    for part in DOC_ATTR.sub('', match.group('body')).split(','):
        part = part.strip()
        if not part:
            continue
        member, _, value = part.partition('=')
        member = member.strip()
        if value.strip():
            next_value = int(value.strip(), 0)
        result[member] = next_value
        next_value += 1
    return result


def compare(rsr_source, bmr_source):
    """Every difference, as lines; empty when both sides agree."""
    problems = []
    for name in ENUMS:
        ours, theirs = members(rsr_source, name), members(bmr_source, name)
        if ours is None or theirs is None:
            problems.append(f'{name}: not found in {"RSR" if ours is None else "BossModReborn"}')
            continue
        for member, value in theirs.items():
            if member not in ours:
                problems.append(f'{name}.{member} = {value} in BossModReborn has no member in RSR')
            elif ours[member] != value:
                problems.append(f'{name}.{member} is {value} in BossModReborn, {ours[member]} in RSR')
        for member, value in ours.items():
            if member not in theirs:
                problems.append(f'{name}.{member} = {value} in RSR is gone from BossModReborn')
    return problems


def fetch():
    last = None
    for attempt in range(3):
        try:
            with urllib.request.urlopen(BMR_URL, timeout=20) as response:
                return response.read().decode('utf-8-sig')
        except OSError as error:  # URLError and timeouts are OSErrors
            last = error
            time.sleep(2 * (attempt + 1))
    raise SystemExit(f"BossModReborn's AIHints.cs could not be read ({last}) - the contract is unchecked, not unchanged")


def self_test():
    ours = 'public enum SpecialMode { Normal = 0, Pyretic = 1 }\npublic enum PredictedDamageType { None = 0, Tankbuster = 1 }'
    same = 'public enum SpecialMode\n{\n    Normal, // free\n    Pyretic,\n}\npublic enum PredictedDamageType { None, Tankbuster }'
    assert compare(ours, same) == [], 'implicit ordinals and comments read like explicit ones'
    inserted = 'public enum SpecialMode { Normal, Frozen, Pyretic }\npublic enum PredictedDamageType { None, Tankbuster }'
    found = compare(ours, inserted)
    assert any('Pyretic is 2' in p for p in found) and any('Frozen' in p for p in found), \
        'a member inserted before ours shifts the ordinal and is reported, and the new one too'
    gone = 'public enum SpecialMode { Normal }\npublic enum PredictedDamageType { None, Tankbuster }'
    assert any('Pyretic = 1 in RSR is gone' in p for p in compare(ours, gone)), 'a member BossModReborn dropped is reported'
    assert any('not found in BossModReborn' in p for p in compare(ours, 'enum Other { A }')), 'a missing enum is reported'
    explicit = 'public enum SpecialMode { Normal = 0, Pyretic = 0x1 }\npublic enum PredictedDamageType { None, Tankbuster }'
    assert compare(ours, explicit) == [], 'hex values are read'
    print('self-test ok: implicit and explicit ordinals, an inserted member, a dropped member and a missing enum')


def main():
    self_test()
    rsr_source = RSR.read_text(encoding='utf-8-sig')
    bmr_source = Path(sys.argv[1]).read_text(encoding='utf-8-sig') if len(sys.argv) > 1 else fetch()
    problems = compare(rsr_source, bmr_source)
    if problems:
        print("RSR's copies of BossModReborn's enums no longer match - every IPC cast reads the wrong member:")
        for problem in problems:
            print(f'  {problem}')
        sys.exit(1)
    counts = ', '.join(f'{name} {len(members(rsr_source, name))}' for name in ENUMS)
    print(f'BossModReborn contract holds: {counts} members, same names and ordinals')


if __name__ == '__main__':
    main()
