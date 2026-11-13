#!/usr/bin/env python3
"""The spells each Masked Carnivale stage guide of Blue Academy names as required or recommended, as RSR's set proposal
per stage (concept 22, "Satz je Stufe aus den Stufenführern", A627).

Blue Academy (RoarkGit/Blue-Academy, the source of mage.blue) writes one guide per stage with the lists "Required Spells"
and "Recommended Spells"; each list point names one or more spells by key. The repository carries no licence, so only
these facts are taken - which spells a point names, in what order, whether it is required, whether it only gives examples
("such as") and whether it means a filler - never the guides' text. Spell keys become spellbook numbers through
RoarkGit/Limited-Job-Data (spell/spell.yaml), the data the guides are built from.

  generate <Blue-Academy checkout> <Limited-Job-Data checkout>
                writes RotationSolver/Basic/Resources/CarnivaleSets.g.cs.
  --check       the file in the tree: stages ascending, every pick with a spell, and every number a Number of the
                AozActionTransient sheet (xivapi v2; skipped when xivapi cannot be reached).
  --self-test   the guide reading and the check against constructed cases.

Usage: python3 .github/scripts/audit/carnivale_sets.py generate <academy> <data> | --check | --self-test
"""
import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'RotationSolver/Basic/Resources/CarnivaleSets.g.cs'
API = 'https://v2.xivapi.com/api'

SECTION = re.compile(r'^###\s+(Required|Recommended) Spells\s*$')
HEADING = re.compile(r'^#{2,3}\s')
SPELL = re.compile(r'\{\{<\s*spell\s+([A-Za-z0-9_-]+)[^>]*>\}\}')
SENTENCE = re.compile(r'(?<=[.!?])\s+')
NOT_INTERRUPTIBLE = re.compile(r'cannot be interrupted', re.I)
ONLY_NOT_VIBE = re.compile(r'^[^,]*cannot be Vibe Checked\.?$')
SPELL_ID = re.compile(r'^- id:\s*(\S+)\s*$')
SPELL_NUMBER = re.compile(r'^\s+number:\s*(\d+)\s*$')
PICK = re.compile(r'new\((true|false), (true|false), (true|false), \[([0-9, ]*)\]\)')


def spell_numbers(yaml_text):
    """{spell key: spellbook number} from Limited-Job-Data's spell.yaml."""
    numbers = {}
    key = None
    for line in yaml_text.splitlines():
        match = SPELL_ID.match(line)
        if match:
            key = match.group(1)
            continue
        match = SPELL_NUMBER.match(line)
        if match and key is not None and key not in numbers:
            numbers[key] = int(match.group(1))
    return numbers


def read_guide(text, numbers):
    """[(required, example, filler, [numbers])] for one stage guide; unknown keys are reported, not taken."""
    picks, unknown = [], []
    section, point = None, None

    def close():
        if section is None or point is None:
            return
        keys = []
        for key in SPELL.findall(point):
            if key not in keys:
                keys.append(key)
        found = [numbers[k] for k in keys if k in numbers]
        unknown.extend(k for k in keys if k not in numbers)
        if found:
            lower = point.lower()
            picks.append((section == 'Required', 'such as' in lower, 'filler' in lower, found))

    for line in text.splitlines():
        match = SECTION.match(line)
        if match:
            close()
            section, point = match.group(1), None
            continue
        if HEADING.match(line):
            close()
            section, point = None, None
            continue
        if section is None:
            continue
        if line.startswith('- '):
            close()
            point = line[2:]
        elif line.strip() == '':
            close()
            point = None
        elif point is not None and line.startswith(' '):
            point += ' ' + line.strip()
    close()
    listed = {n for pick in picks for n in pick[3]}
    for key in mechanic_keys(text):
        if key not in numbers:
            unknown.append(key)
        elif numbers[key] not in listed:
            listed.add(numbers[key])
            picks.append((False, False, False, [numbers[key]]))
    return picks, unknown


def mechanic_keys(text):
    """The spells the text of a guide's acts names for a mechanic of the stage, in order (concept 22, "Zauber für die
    Mechanik aus dem Fließtext der Führer", A762): Flying Sardine where a sentence interrupts a cast with it (not where it
    "cannot be interrupted"), and the Ram's Voice and Ultravibration where enemies "can be Vibe Checked" (the basics page
    names the Vibe Check as these two spells). Lists and achievement notes are no act text; every other spell named in
    it is a choice, a negation or an action outside the set."""
    keys, section, paragraph = [], None, []

    def flush():
        sentences = SENTENCE.split(' '.join(' '.join(paragraph).split()))
        for sentence in sentences:
            spells = SPELL.findall(sentence)
            if ('flying_sardine' in spells and 'interrupt' in sentence.lower()
                    and not NOT_INTERRUPTIBLE.search(sentence)):
                found = ['flying_sardine']
            elif 'Vibe Checked' in sentence and not ONLY_NOT_VIBE.match(sentence):
                found = ['the_rams_voice', 'ultravibration']
            else:
                continue
            keys.extend(k for k in found if k not in keys)
        paragraph.clear()

    for line in text.splitlines():
        if HEADING.match(line):
            flush()
            section = line.lstrip('#').strip()
            continue
        if section is not None and section.startswith('Act'):
            paragraph.append(line)
    flush()
    return keys


def collect(academy, data):
    numbers = spell_numbers((data / 'spell/spell.yaml').read_text(encoding='utf-8'))
    stages, unknown = {}, []
    for guide in sorted((academy / 'content/masked-carnivale/stages').glob('[0-9]*.md')):
        picks, missing = read_guide(guide.read_text(encoding='utf-8'), numbers)
        unknown.extend(f'{guide.name}: {k}' for k in missing)
        if picks:
            stages[int(guide.stem)] = picks
    return stages, unknown


def render(stages, source):
    lines = [
        '// <auto-generated>',
        f'// Written by .github/scripts/audit/carnivale_sets.py from {source}; do not edit, generate it again.',
        '// </auto-generated>',
        'namespace RotationSolver.Basic.Resources;',
        '',
        '/// <summary>',
        '/// The spells each Masked Carnivale stage guide of Blue Academy names (concept 22, "Satz je Stufe aus den',
        '/// Stufenführern", A627): per stage the points of its required and recommended lists, each with the spellbook numbers',
        '/// it names in the guide\'s order. Facts only; the guides are at https://mage.blue/masked-carnivale/stages/.',
        '/// </summary>',
        'internal static class CarnivaleSets',
        '{',
        '\t/// <summary>One point of a guide\'s list: required or recommended, only examples ("such as"), a filler, its spells.</summary>',
        '\tinternal readonly record struct Pick(bool Required, bool Example, bool Filler, uint[] Spells);',
        '',
        '\tinternal static readonly Dictionary<int, Pick[]> Stages = new()',
        '\t{',
    ]
    for stage in sorted(stages):
        lines.append(f'\t\t[{stage}] =')
        lines.append('\t\t[')
        for required, example, filler, spells in stages[stage]:
            flags = ', '.join('true' if f else 'false' for f in (required, example, filler))
            lines.append(f'\t\t\tnew({flags}, [{", ".join(str(n) for n in spells)}]),')
        lines.append('\t\t],')
    lines.append('\t};')
    lines.append('}')
    return '\n'.join(lines) + '\n'


def parse_generated(text):
    stages = {}
    for match in re.finditer(r'\[(\d+)\] =\s*\[(.*?)\n\t\t\],', text, re.S):
        stages[int(match.group(1))] = [
            (r == 'true', e == 'true', f == 'true', [int(n) for n in re.findall(r'\d+', spells)])
            for r, e, f, spells in PICK.findall(match.group(2))]
    return stages


def fetch_json(url):
    """The parsed answer, or None when xivapi cannot be reached (curl: the proxy setup is configured for it)."""
    try:
        out = subprocess.run(['curl', '-sS', '--max-time', '30', url], capture_output=True, text=True)
        return json.loads(out.stdout) if out.returncode == 0 and out.stdout else None
    except (OSError, ValueError):
        return None


def sheet_numbers(fetch):
    """Every Number of the AozActionTransient sheet, or None when xivapi cannot be reached."""
    answer = fetch(f'{API}/sheet/AozActionTransient?limit=500&fields=Number')
    if answer is None:
        return None
    return {row['fields'].get('Number') for row in answer.get('rows', [])} - {0}


def problems(stages, known):
    found = []
    if list(stages) != sorted(stages):
        found.append('stages not ascending')
    if not stages:
        found.append('no stage read')
    for stage, picks in stages.items():
        for i, (_, _, _, spells) in enumerate(picks):
            if not spells:
                found.append(f'stage {stage}, point {i + 1}: no spell')
            if known is not None:
                found.extend(f'stage {stage}, point {i + 1}: {n} is no spell number of AozActionTransient'
                             for n in spells if n not in known)
    return found


def check(fetch=fetch_json):
    stages = parse_generated(OUT.read_text(encoding='utf-8'))
    known = sheet_numbers(fetch)
    found = problems(stages, known)
    for p in found:
        print(p)
    if known is None:
        print('xivapi is not reachable: the numbers were not held against AozActionTransient this run.')
    if not found:
        print(f'{OUT.relative_to(ROOT)}: {len(stages)} stage(s), {sum(len(p) for p in stages.values())} point(s)'
              + ('' if known is None else ', every number a spell of AozActionTransient') + '.')
    return 1 if found else 0


def self_test():
    numbers = spell_numbers('- id: water_cannon\n  name: Water Cannon\n  number: 1\n'
                            '- id: 1000_needles\n  number: 36\n- id: doom\n  number: 42\n- id: exuviation\n  number: 50\n')
    assert numbers == {'water_cannon': 1, '1000_needles': 36, 'doom': 42, 'exuviation': 50}, numbers
    guide = ('---\ntitle: X\n---\n\n### Recommended Spells\n\n- Water filler spell, such as {{< spell water_cannon >}}.\n'
             '- {{< spell unknown_one >}}\n\n### Required Spells\n\n- {{< spell 1000_needles >}}, but other death spells like\n'
             '  {{< spell doom >}} work well, too.\n- {{< spell exuviation strategy >}} and {{< spell exuviation >}}\n\n'
             '### Act 1\n\n- {{< spell doom >}} in the walkthrough is no pick.\n')
    picks, unknown = read_guide(guide, numbers)
    assert picks == [(False, True, True, [1]), (True, False, False, [36, 42]), (True, False, False, [50])], picks
    assert unknown == ['unknown_one'], unknown
    numbers.update({'flying_sardine': 24, 'the_rams_voice': 33, 'ultravibration': 92})
    acts = ('---\ntitle: Y\n---\n\n### Required Spells\n\n- {{< spell the_rams_voice >}}\n\n### Act 1\n\n'
            'Burst cannot be interrupted with {{< spell flying_sardine >}}. The Catoblepas cannot be Vibe Checked.\n\n'
            '### Act 2\n\nIt casts Doom, which we can\ninterrupt with {{< spell flying_sardine >}}. The Imps can be Vibe\n'
            'Checked.\n\n### Achievement Info\n\nInterrupt it with {{< spell flying_sardine >}} and use {{< spell doom >}}.\n')
    assert mechanic_keys(acts) == ['flying_sardine', 'the_rams_voice', 'ultravibration'], mechanic_keys(acts)
    picks2, _ = read_guide(acts, numbers)
    assert picks2 == [(True, False, False, [33]), (False, False, False, [24]), (False, False, False, [92])], picks2
    none = '---\nt: Z\n---\n\n### Act 1\n\nBurst cannot be interrupted with {{< spell flying_sardine >}}.\nThe Catoblepas cannot be Vibe Checked.\n'
    assert mechanic_keys(none) == [], mechanic_keys(none)
    six = '---\nt: Z\n---\n\n### Act 2\n\nThe Catoblepas cannot be Vibe Checked, but the other enemies can.\n'
    assert mechanic_keys(six) == ['the_rams_voice', 'ultravibration'], mechanic_keys(six)
    text = render({2: picks, 5: [(True, False, False, [36])]}, 'test')
    assert parse_generated(text) == {2: picks, 5: [(True, False, False, [36])]}, parse_generated(text)
    assert problems({5: [(True, False, False, [36])], 2: picks}, None) == ['stages not ascending']
    assert problems({2: [(True, False, False, [])]}, None) == ['stage 2, point 1: no spell']
    assert problems({2: [(True, False, False, [99])]}, {1, 36}) == ['stage 2, point 1: 99 is no spell number of AozActionTransient']
    print('self-test ok: spell numbers are read; list points of both sections become picks with their spells in order, '
          'required, example and filler flags, continuation lines joined, repeated and unknown keys handled, walkthrough '
          'text left out; act text yields Flying Sardine for an interrupt and the Vibe Check spells, not for a negation, '
          'an achievement note or a listed spell; the file round-trips; disorder, empty points and unknown numbers are reported')
    return 0


def main(argv):
    if argv[:1] == ['--self-test']:
        return self_test()
    if argv[:1] == ['--check']:
        return check()
    if len(argv) == 3 and argv[0] == 'generate':
        academy, data = Path(argv[1]), Path(argv[2])
        commits = [subprocess.run(['git', '-C', str(p), 'log', '-1', '--format=%h %cs'], capture_output=True, text=True).stdout.strip()
                   for p in (academy, data)]
        stages, unknown = collect(academy, data)
        for u in unknown:
            print(f'unknown spell key, left out: {u}')
        OUT.write_text(render(stages, f'RoarkGit/Blue-Academy {commits[0]} and RoarkGit/Limited-Job-Data {commits[1]}'),
                       encoding='utf-8')
        print(f'{OUT.relative_to(ROOT)}: {len(stages)} stage(s), {sum(len(p) for p in stages.values())} point(s)')
        return 0
    print(__doc__)
    return 2


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
