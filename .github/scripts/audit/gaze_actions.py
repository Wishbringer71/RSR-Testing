#!/usr/bin/env python3
"""The actions BossModReborn's fight modules treat as gazes, as RSR's list of gaze casts (concept 13, "Blicke ohne
Kennzeichen, aus BossModReborns Modulen", A772).

The sheet marks a gaze only sometimes (Action.VFX 25); Beatrice's Death Foreseen (#29821, #29828) carries fight-specific
cast effects instead. BossModReborn (FFXIV-CombatReborn/BossmodReborn, BSD-3-Clause) names gazes per fight with
`Components.CastGaze(module, (uint)AID.X ...)` and `Components.CastGazes(module, [(uint)AID.X, ...] ...)`; only these facts
are taken - which action ids, and whether the gaze is inverted (the player must look at it) - never its code.

Gazes a module builds itself (`GenericGaze`, A776) place the eye or decide the inversion at runtime: at a created figure,
by an icon, a status or an animation. RSR cannot read those, so their ids go to `Unsure` and count as no gaze, even with
the gaze mark or a gaze's name. A class that inverts nothing and puts the eye where its cast happens (`spell.LocXZ` in
`OnCastStarted`, without a module setting) gives its cast ids to `Gazes`: RSR places the eye at the cast's location.

  generate <BossmodReborn checkout>   writes RotationSolver/Basic/Resources/GazeActions.g.cs.
  --check       the file in the tree: ids ascending and unique, none in both lists, every id an enemy action of the
                Action sheet (xivapi v2; skipped when xivapi cannot be reached).
  --self-test   the module reading and the check against constructed cases.

Usage: python3 .github/scripts/audit/gaze_actions.py generate <bmr> | --check | --self-test
"""
import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'RotationSolver/Basic/Resources/GazeActions.g.cs'
API = 'https://v2.xivapi.com/api'

AID_ENUM = re.compile(r'enum\s+AID\b[^{]*\{(.*?)\}', re.S)
ENUM_ENTRY = re.compile(r'\b(\w+)\s*=\s*(0x[0-9A-Fa-f]+|\d+)\b')
CAST_GAZE = re.compile(r'Components\.CastGaze\(\s*module\s*,\s*\(uint\)AID\.(\w+)\s*(\)|,(.*?)\)\s*[;{])', re.S)
CAST_GAZES = re.compile(r'Components\.CastGazes\(\s*module\s*,\s*\[(.*?)\](.*?)\)\s*[;{]', re.S)
AID_REF = re.compile(r'\(uint\)AID\.(\w+)')
LIST = re.compile(r'internal static readonly HashSet<uint> (\w+) =\s*\[([0-9, \n\t]*)\];')
GENERIC = re.compile(r'class (\w+)\([^)]*\)\s*:\s*Components\.GenericGaze\b')
NEXT_CLASS = re.compile(r'\n(?:(?:sealed|public|internal|abstract|private)\s+)*class \w+')
LISTS = ('Gazes', 'Inverted', 'Unsure')


def aid_values(texts):
    """{name: id} from every `enum AID` in the texts (one module directory shares its enums)."""
    values = {}
    for text in texts:
        for block in AID_ENUM.findall(text):
            for name, value in ENUM_ENTRY.findall(block):
                values[name] = int(value, 0)
    return values


def inverted(rest):
    """Whether the arguments after the action make the gaze inverted: `inverted: true` or `true` as the next one."""
    if rest is None:
        return False
    rest = rest.strip()
    return bool(re.search(r'\binverted\s*:\s*true\b', rest)) or bool(re.match(r'true\b', rest))


def generic_classes(text):
    """(name, body) of every class in the text that derives from `Components.GenericGaze`."""
    found = []
    for m in GENERIC.finditer(text):
        rest = NEXT_CLASS.search(text, m.end())
        found.append((m.group(1), text[m.start():rest.start() if rest else len(text)]))
    return found


def method(body, name):
    """The body of the override `name` in a class body, '' when it has none."""
    m = re.search(r'override void ' + name + r'\b[^{]*\{', body)
    if not m:
        return ''
    depth, i = 1, m.end()
    while depth and i < len(body):
        depth += {'{': 1, '}': -1}.get(body[i], 0)
        i += 1
    return body[m.end():i]


def read_modules(texts):
    """(gazes, inverted gazes, unsure, unresolved names) for the texts of one module directory."""
    values = aid_values(texts)
    gazes, inverse, unsure, unknown = set(), set(), set(), []
    for text in texts:
        for name, _, rest in CAST_GAZE.findall(text):
            target = inverse if inverted(rest) else gazes
            if name in values:
                target.add(values[name])
            else:
                unknown.append(name)
        for names, rest in CAST_GAZES.findall(text):
            target = inverse if inverted(rest.lstrip(', ')) else gazes
            for name in AID_REF.findall(names):
                if name in values:
                    target.add(values[name])
                else:
                    unknown.append(name)
        for _, body in generic_classes(text):
            ids = {values[n] for n in AID_REF.findall(body) if n in values}
            started = method(body, 'OnCastStarted')
            at_cast = set()
            if not re.search('invert', body, re.I) and 'spell.LocXZ' in started and not re.search(r'config', started, re.I):
                at_cast = {values[n] for n in AID_REF.findall(started + method(body, 'OnCastFinished')) if n in values}
            gazes |= at_cast
            unsure |= ids - at_cast
    return gazes, inverse, unsure, unknown


def collect(bmr):
    gazes, inverse, unsure, unknown = set(), set(), set(), []
    for directory in sorted({p.parent for p in (bmr / 'BossMod/Modules').rglob('*.cs')}):
        texts = [p.read_text(encoding='utf-8', errors='replace') for p in sorted(directory.glob('*.cs'))]
        g, i, u, n = read_modules(texts)
        gazes |= g
        inverse |= i
        unsure |= u
        unknown.extend(f'{directory.relative_to(bmr)}: {x}' for x in n)
    return gazes - inverse, inverse, unsure - gazes - inverse, unknown


def render(gazes, inverse, unsure, source):
    def block(name, ids, doc):
        lines = [f'\t/// <summary>{doc}</summary>', f'\tinternal static readonly HashSet<uint> {name} =', '\t[']
        ordered = sorted(ids)
        for i in range(0, len(ordered), 10):
            lines.append('\t\t' + ', '.join(str(n) for n in ordered[i:i + 10]) + ',')
        lines.append('\t];')
        return lines

    lines = [
        '// <auto-generated>',
        f'// Written by .github/scripts/audit/gaze_actions.py from {source}; do not edit, generate it again.',
        '// </auto-generated>',
        'namespace RotationSolver.Basic.Resources;',
        '',
        '/// <summary>',
        '/// The actions BossModReborn\'s fight modules treat as gazes (concept 13, "Blicke ohne Kennzeichen, aus BossModReborns',
        '/// Modulen", A772, A776): ids only, from its CastGaze and CastGazes components and its own gaze classes.',
        '/// </summary>',
        'internal static class GazeActions',
        '{',
    ]
    lines += block('Gazes', gazes, 'Gazes that hit whoever looks at the caster.')
    lines.append('')
    lines += block('Inverted', inverse, 'Inverted gazes, which hit whoever looks away from the caster.')
    lines.append('')
    lines += block('Unsure', unsure, 'Ids the modules use only in gazes they build at runtime (eye elsewhere, inversion '
                   'by icon, status or animation): no gaze for RSR.')
    lines.append('}')
    return '\n'.join(lines) + '\n'


def parse_generated(text):
    return {name: [int(n) for n in re.findall(r'\d+', body)] for name, body in LIST.findall(text)}


def fetch_json(url):
    try:
        out = subprocess.run(['curl', '-sS', '--max-time', '30', url], capture_output=True, text=True)
        return json.loads(out.stdout) if out.returncode == 0 and out.stdout else None
    except (OSError, ValueError):
        return None


def problems(lists, rows):
    found = []
    for name in LISTS:
        ids = lists.get(name)
        if ids is None:
            found.append(f'{name}: list missing')
            continue
        if ids != sorted(set(ids)):
            found.append(f'{name}: not ascending and unique')
        if rows is not None and name != 'Unsure':
            for n in ids:
                row = rows.get(n)
                if row is None:
                    found.append(f'{name}: {n} is no row of the Action sheet')
                elif row.get('IsPlayerAction'):
                    found.append(f'{name}: {n} is a player action')
    for a, b in (('Gazes', 'Inverted'), ('Gazes', 'Unsure'), ('Inverted', 'Unsure')):
        both = set(lists.get(a, [])) & set(lists.get(b, []))
        if both:
            found.append(f'in {a} and {b}: {sorted(both)}')
    if not lists.get('Gazes'):
        found.append('no gaze read')
    return found




def sheet_rows(ids, fetch):
    rows = {}
    ordered = sorted(ids)
    for i in range(0, len(ordered), 100):
        chunk = ordered[i:i + 100]
        answer = fetch(f"{API}/sheet/Action?rows={','.join(str(n) for n in chunk)}&fields=IsPlayerAction")
        if answer is None:
            return None
        for row in answer.get('rows', []):
            rows[row['row_id']] = row.get('fields', {})
    return rows


def check(fetch=fetch_json):
    lists = parse_generated(OUT.read_text(encoding='utf-8'))
    ids = set(lists.get('Gazes', [])) | set(lists.get('Inverted', []))  # Unsure holds helper rows without a name too
    rows = sheet_rows(ids, fetch)
    found = problems(lists, rows)
    for p in found:
        print(p)
    if rows is None:
        print('xivapi is not reachable: the ids were not held against the Action sheet this run.')
    if not found:
        print(f'{OUT.relative_to(ROOT)}: {len(lists["Gazes"])} gaze(s), {len(lists["Inverted"])} inverted, '
              f'{len(lists["Unsure"])} unsure'
              + ('' if rows is None else ', every id an enemy action of the Action sheet') + '.')
    return 1 if found else 0


def self_test():
    module = ('public enum AID : uint\n{\n    DeathForeseen1 = 29821, // gaze\n    DeathForeseen2 = 29828,\n    Other = 0x10,\n'
              '    Look = 40000,\n    Pair1 = 41, Pair2 = 42,\n    Moon = 50, Glare = 51, GlareEnd = 52, Pop = 53, Eye = 54,\n}\n'
              'sealed class A(BossModule module) : Components.CastGaze(module, (uint)AID.DeathForeseen1);\n'
              'sealed class B(BossModule module) : Components.CastGaze(module, (uint)AID.DeathForeseen2, maxCasts: 2);\n'
              'sealed class C(BossModule module) : Components.CastGaze(module, (uint)AID.Look, true);\n'
              'sealed class D(BossModule module) : Components.CastGazes(module, [(uint)AID.Pair1, (uint)AID.Pair2], inverted: true);\n'
              'sealed class E(BossModule module) : Components.CastGaze(module, (uint)AID.Missing)\n{\n}\n'
              'sealed class F(BossModule module) : Components.GenericGaze(module, (uint)AID.Other)\n{\n'
              '    public override void OnActorCreated(Actor actor) { _eye = [new(actor.Position)]; }\n'
              '    public override void OnEventCast(Actor caster, ActorCastEvent spell) { if (spell.Action.ID == (uint)AID.Moon) _eye = []; }\n}\n'
              'sealed class G(BossModule module) : Components.GenericGaze(module)\n{\n'
              '    public override void OnCastStarted(Actor caster, ActorCastInfo spell)\n    {\n'
              '        if (spell.Action.ID is (uint)AID.Glare) { var loc = spell.LocXZ; Eyes.Add(new(loc)); }\n    }\n'
              '    public override void OnCastFinished(Actor caster, ActorCastInfo spell)\n    {\n'
              '        if (spell.Action.ID is (uint)AID.GlareEnd) { Eyes.Clear(); }\n    }\n}\n'
              'sealed class H(BossModule module) : Components.GenericGaze(module)\n{\n'
              '    public override void OnCastStarted(Actor caster, ActorCastInfo spell)\n    {\n'
              '        if (_config.Pop && spell.Action.ID == (uint)AID.Pop) { _gaze = new(spell.LocXZ); }\n    }\n}\n'
              'sealed class I(BossModule module) : Components.GenericGaze(module)\n{\n'
              '    public override void OnCastStarted(Actor caster, ActorCastInfo spell)\n    {\n'
              '        if (spell.Action.ID == (uint)AID.Eye) { Eyes.Add(new(spell.LocXZ, inverted: _flip)); }\n    }\n}\n')
    gazes, inverse, unsure, unknown = read_modules([module])
    assert gazes == {29821, 29828, 51, 52}, gazes
    assert inverse == {40000, 41, 42}, inverse
    assert unsure == {0x10, 50, 53, 54}, unsure
    assert unknown == ['Missing'], unknown
    enums = 'public enum AID : uint\n{\n    Split = 7,\n}\n'
    user = 'class G(BossModule module) : Components.CastGaze(module, (uint)AID.Split, range: 20f);\n'
    assert read_modules([enums, user])[0] == {7}
    text = render({29821, 29828}, {40000}, {50}, 'test')
    assert parse_generated(text) == {'Gazes': [29821, 29828], 'Inverted': [40000], 'Unsure': [50]}, parse_generated(text)
    assert problems({'Gazes': [2, 1], 'Inverted': [], 'Unsure': []}, None) == ['Gazes: not ascending and unique']
    assert problems({'Gazes': [1], 'Inverted': [1], 'Unsure': []}, None) == ['in Gazes and Inverted: [1]']
    assert problems({'Gazes': [1], 'Inverted': [], 'Unsure': [1]}, None) == ['in Gazes and Unsure: [1]']
    assert problems({'Gazes': [1], 'Inverted': []}, None) == ['Unsure: list missing']
    assert problems({'Gazes': [5], 'Inverted': [], 'Unsure': []}, {5: {'IsPlayerAction': True}}) == ['Gazes: 5 is a player action']
    assert problems({'Gazes': [6], 'Inverted': [], 'Unsure': []}, {}) == ['Gazes: 6 is no row of the Action sheet']
    print('self-test ok: AID enums are read across a module directory, hex and decimal; CastGaze and CastGazes give '
          'their ids, inverted by name or position; unknown names are reported; a module\'s own gaze class gives its '
          'ids as unsure, or as gazes when it inverts nothing and places the eye at the cast without a setting; the '
          'file round-trips; disorder, overlap of any two lists, a missing list, player actions and missing rows are '
          'reported')
    return 0


def main(argv):
    if argv[:1] == ['--self-test']:
        return self_test()
    if argv[:1] == ['--check']:
        return check()
    if len(argv) == 2 and argv[0] == 'generate':
        bmr = Path(argv[1])
        commit = subprocess.run(['git', '-C', str(bmr), 'log', '-1', '--format=%h %cs'], capture_output=True,
                                text=True).stdout.strip()
        gazes, inverse, unsure, unknown = collect(bmr)
        for u in unknown:
            print(f'unresolved action, left out: {u}')
        OUT.write_text(render(gazes, inverse, unsure, f'FFXIV-CombatReborn/BossmodReborn {commit}'), encoding='utf-8')
        print(f'{OUT.relative_to(ROOT)}: {len(gazes)} gaze(s), {len(inverse)} inverted, {len(unsure)} unsure')
        return 0
    print(__doc__)
    return 2


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
