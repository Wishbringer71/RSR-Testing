#!/usr/bin/env python3
"""Do the concepts still state the defaults the code actually has?

A concept that says "default off" about a setting the code ships as on is worse than one that says
nothing: the next pass reads it, recommends switching the setting on, and the owner has had it on
all along. That has happened here (`StretchHolyStun`, see AUDIT_LOG.md), and nothing fails when it
does - the sentence stays readable and wrong.

Two sources, and the second one is why a naive check misses most of it: the global options live in
`Configs.cs`, but every rotation carries its own settings as plain properties in its own file
(`WHM_Reborn.cs`, `DRK_Reborn.cs`, …). A check that reads only the configuration class finds eight
options and calls everything else unknown.

What this compares is the **code default**, nothing else. What the owner has configured is not
readable from here, and a concept must not claim it either - that distinction is in CLAUDE.md and
this check cannot enforce it, only the one it can: the claim about the code.

Usage: python3 .github/scripts/audit/check_concept_defaults.py
Exit code 1 when a concept states a default the code contradicts.
"""
import re
import sys
from pathlib import Path

CONCEPT_DIR = Path('docs/rotation-flow')

# The release description states defaults to the user, who then acts on them - the same
# ageing with a shorter fuse than a concept's, so it is measured with the concepts. Both
# halves count: the standing picture and the text of the release being prepared.
EXTRA_DOCS = (Path('docs/fork-changes-in-play.md'),
              Path('docs/fork-changes-since-last-release.md'))

SOURCE_DIRS = (Path('RotationSolver.Basic'), Path('RotationSolver'))

# `public bool Name { get; set; } = true;` - the form both the configuration class and every
# rotation file use for a switchable setting.
DEFINITION = re.compile(
    r'(?:public|private)\s+(?:readonly\s+)?bool\s+_?([A-Za-z][A-Za-z0-9_]*)'
    r'(?:\s*\{\s*get;\s*set;\s*\})?\s*=\s*(true|false)\s*;')

# The wordings the concepts actually use, German and English, in the sentence around the name.
# "ab Werk an" and "**an** als Vorgabewert" were missing, and a concept said "aus als Vorgabewert" about
# a setting the code had switched on four days earlier - unnoticed, because the wording was not read
# (A250). The trailing guard keeps "an" from matching the start of a longer word.
_END = r'(?![A-Za-zÄÖÜäöüß])'
ON = (r'(?:Standard\s+an' + _END + r'|voreingestellt\s+an' + _END + r'|Voreinstellung\s+an' + _END
      + r'|on\s+by\s+default|Standard\s+ein' + _END + r'|ab\s+Werk\s+\**an' + _END
      + r'|Vorgabewert\s+\**an' + _END + r'|\**an\**\s+als\s+Vorgabewert)')
OFF = (r'(?:Standard\s+aus|voreingestellt\s+aus|Voreinstellung\s+aus|off\s+by\s+default'
       r'|ab\s+Werk\s+\**aus|Vorgabewert\s+\**aus|\**aus\**\s+als\s+Vorgabewert)')
CLAIM = re.compile(r'`([A-Z][A-Za-z0-9_]{3,})`(?P<between>[^`]{0,200}?)(?P<value>' + ON + '|' + OFF + ')',
                   re.IGNORECASE | re.DOTALL)

# Concepts mostly quote a setting by the text the owner sees, not by its property name - `Heal ahead of
# an announced area cast`, „Use Thrill of Battle before a tankbuster on you". Those claims were not read
# at all. A label is mapped to its property through the definition that follows it in the source
# (`UI("...")` in the configuration, `Name = "..."` in a rotation); a quoted text that is no label is
# left alone, because quotation marks carry far more than setting names.
LABEL_CLAIM = re.compile(r'(?:`|„|")(?P<label>[^`„“"\n]{6,160})(?:`|“|")(?P<between>[^`„]{0,200}?)'
                         r'(?P<value>' + ON + '|' + OFF + ')', re.IGNORECASE | re.DOTALL)
LABEL_DEFINITION = re.compile(r'(?:\bUI\(\s*|\bName\s*=\s*)"(?P<label>[^"]{6,})"')

# A sentence may name a second setting between the first name and the default - then the default
# belongs to the nearer name, not the first. Bailing out is the honest answer: report it as
# unmatched rather than guess which one it meant.
ANOTHER_NAME = re.compile(r'`[A-Z][A-Za-z0-9_]{3,}`')

# The same ageing with numbers, and there are more of them: thresholds like HealthAreaSpell 0,65 or
# BlackestNightMinHostiles 4 are quoted in the concepts and shipped in the code. German decimal
# commas are the written form here, so both separators have to be read.
# Three shapes carry a default, and missing the third made the check report a real setting as
# unknown: `public float X { get; set; } = 0.15f;`, `private float X { get; set; } = 0.6f;` in a
# rotation, and `private readonly float _x = 0.15f;` in the configuration, where a generator turns
# the field into the property the tree reads.
NUMERIC_DEFINITION = re.compile(
    r'(?:public|private)\s+(?:readonly\s+)?(?:float|int|double)\s+_?([A-Za-z][A-Za-z0-9_]*)'
    r'(?:\s*\{\s*get;\s*set;\s*\})?\s*=\s*([0-9]*\.?[0-9]+)f?\s*;')
# Deliberately narrow: only the form the concepts use to STATE a default - the name, then the value
# in brackets, with at most a qualifying word between them. A wider window read line numbers, action
# ids and a model's own constants as claims; of twenty findings, twenty were noise. A check that has
# to be filtered by its reader is not a check.
NUMERIC_CLAIM = re.compile(
    r'`([A-Z][A-Za-z0-9_]{3,})`(?P<between>[ ,]*)'
    r'\((?:Vorgabe|Standard|Voreinstellung|Default)?\s*(?P<value>[0-9]+(?:[.,][0-9]+)?)\s*%?\)')

# Concepts name methods in the same backticks as settings, and a method mentioned anywhere near the
# word "default" is not a claim about a default. The first run reported four of them and nothing
# else - a report made entirely of noise teaches its reader to skip it.
METHOD = re.compile(r'\b(?:bool|void|int|float|string|Task|IAction)\s+([A-Za-z0-9_]+)\s*\(')


def label_map(text):
    """Returns {label lower-cased: (property, default)} for the bool settings defined in `text`.

    The definition belongs to a label only if no other label stands between them, so a label whose
    setting is numeric does not borrow the next bool's default.
    """
    out = {}
    labels = list(LABEL_DEFINITION.finditer(text))
    for i, m in enumerate(labels):
        limit = labels[i + 1].start() if i + 1 < len(labels) else len(text)
        d = DEFINITION.search(text, m.end(), limit)
        if d is None or NUMERIC_DEFINITION.search(text, m.end(), d.start()):
            continue
        name = d.group(1)
        out.setdefault(m.group('label').strip().lower(), (name[0].upper() + name[1:], d.group(2)))
    return out


def code_defaults(dirs=SOURCE_DIRS, labels=None):
    """Returns ({bool setting: value}, {numeric setting: value}, {method names}) over the tree.

    With `labels` given, it is filled with {label: (property, default)} on the way.
    """
    # The same property name occurs in several rotations - `AddCrimsonCyclone` is off in SMN_Reborn and
    # on in the third-party ChurinSMN. Taking whichever file came first scored a correct concept as
    # wrong. The concepts describe the owner's rotations, so a definition outside ExtraRotations wins;
    # where the owner's own files still disagree, the name says nothing and is dropped.
    seen, seen_numbers = {}, {}
    methods = set()
    for directory in dirs:
        if not directory.is_dir():
            continue
        for path in directory.rglob('*.cs'):
            text = path.read_text(encoding='utf-8', errors='replace')
            third_party = 'ExtraRotations' in path.parts
            for name, value in DEFINITION.findall(text):
                seen.setdefault(name[0].upper() + name[1:], []).append((third_party, value))
            for name, value in NUMERIC_DEFINITION.findall(text):
                seen_numbers.setdefault(name[0].upper() + name[1:], []).append((third_party, float(value)))
            methods.update(METHOD.findall(text))
            if labels is not None:
                for label, entry in label_map(text).items():
                    if third_party:
                        labels.setdefault(label, entry)
                    else:
                        labels[label] = entry
    return resolve(seen), resolve(seen_numbers), methods


def resolve(seen):
    """One value per name: the owner's definitions first, and none where they disagree."""
    out = {}
    for name, entries in seen.items():
        own = {v for third, v in entries if not third}
        values = own or {v for _, v in entries}
        if len(values) == 1:
            out[name] = values.pop()
    return out


def check_labels(text, labels):
    """Returns [(label, stated, actual)] for claims made about a setting by its visible text."""
    wrong = []
    for m in LABEL_CLAIM.finditer(text):
        entry = labels.get(m.group('label').strip().lower())
        if entry is None or ANOTHER_NAME.search(m.group('between')):
            continue
        stated = 'off' if re.search(OFF, m.group('value'), re.IGNORECASE) else 'on'
        if (stated == 'on') != (entry[1] == 'true'):
            wrong.append((m.group('label').strip(), stated, entry[1]))
    return wrong


def claims(text):
    """Returns [(setting, 'on'|'off')] stated in `text`, skipping ambiguous sentences."""
    out = []
    for m in CLAIM.finditer(text):
        if ANOTHER_NAME.search(m.group('between')):
            continue
        stated = 'off' if re.search(OFF, m.group('value'), re.IGNORECASE) else 'on'
        out.append((m.group(1), stated))
    return out


def numeric_claims(text):
    """Returns [(setting, value)] for numbers stated next to a setting name."""
    out = []
    for m in NUMERIC_CLAIM.finditer(text):
        if ANOTHER_NAME.search(m.group('between')):
            continue
        out.append((m.group(1), float(m.group('value').replace(',', '.'))))
    return out


def check_numbers(concept_text, numbers, methods=frozenset()):
    """Returns (wrong, unknown) for the numeric claims of one document.

    A tolerance of one hundredth: the concepts write 0,65 for 0.65f, and a value the code carries as
    a percentage of a percentage is not worth a failing run over the last digit.
    """
    wrong, unknown = [], []
    for name, stated in numeric_claims(concept_text):
        actual = numbers.get(name)
        if actual is None:
            if name in methods:
                continue
            unknown.append((name, stated))
        else:
            # A concept writes "Vorgabe 60 %" for a code value of 0.6 - the same number in the
            # unit the reader thinks in. Comparing them literally would report every ratio.
            candidates = {stated, stated / 100.0}
            if not any(abs(actual - c) <= 0.01 for c in candidates):
                wrong.append((name, stated, actual))
    return wrong, unknown


def check(concept_text, defaults, methods=frozenset()):
    """Returns (wrong, unknown) for one document. Names that are methods are not claims."""
    wrong, unknown = [], []
    for name, stated in claims(concept_text):
        actual = defaults.get(name)
        if actual is None:
            if name in methods:
                continue
            unknown.append((name, stated))
        elif (stated == 'on') != (actual == 'true'):
            wrong.append((name, stated, actual))
    return wrong, unknown


def selftest():
    """Constructed cases, because a silent pass is otherwise indistinguishable from a clean tree."""
    defaults = {'AlphaSetting': 'false', 'BetaSetting': 'true'}

    wrong, unknown = check('Die Option `AlphaSetting` ist **Standard aus**.', defaults)
    if wrong or unknown:
        raise AssertionError('a correct claim was reported: %s %s' % (wrong, unknown))

    wrong, _ = check('Die Option `AlphaSetting` ist **Standard an**.', defaults)
    if not any(n == 'AlphaSetting' for n, _, _ in wrong):
        raise AssertionError('a claim contradicting the code went unnoticed')

    wrong, _ = check('`BetaSetting` (**off by default**)', defaults)
    if not any(n == 'BetaSetting' for n, _, _ in wrong):
        raise AssertionError('the English wording went unnoticed')

    _, unknown = check('`GoneSetting` ist **Standard aus**.', defaults)
    if not any(n == 'GoneSetting' for n, _ in unknown):
        raise AssertionError('a claim about a setting the code does not have went unnoticed')

    # Two names before the default: the sentence is ambiguous and must not be scored.
    wrong, unknown = check('`AlphaSetting` haengt an `BetaSetting`, **Standard an**.', defaults)
    if wrong or unknown:
        raise AssertionError('an ambiguous sentence was scored anyway: %s %s' % (wrong, unknown))

    # A method carries no default, however close the word stands.
    _, unknown = check('`SurveyStuns` liefert das, Voreinstellung aus.', defaults, {'SurveyStuns'})
    if unknown:
        raise AssertionError('a method name was reported as a setting: %s' % unknown)

    numbers = {'AlphaThreshold': 0.65, 'BetaCount': 4.0}
    if check_numbers('`AlphaThreshold` (0,65)', numbers) != ([], []):
        raise AssertionError('a correct numeric claim was reported')
    nwrong, _ = check_numbers('`AlphaThreshold` (0,40)', numbers)
    if not any(n == 'AlphaThreshold' for n, _, _ in nwrong):
        raise AssertionError('a numeric claim contradicting the code went unnoticed')
    if check_numbers('`GammaRatio` (60 %)', {'GammaRatio': 0.6}) != ([], []):
        raise AssertionError('a percentage stated for a ratio was reported as wrong')
    nwrong, _ = check_numbers('`BetaCount` (Vorgabe 4)', numbers)
    if nwrong:
        raise AssertionError('an integer claim in the German wording was rejected: %s' % nwrong)
    _, nunknown = check_numbers('`SurveyStuns` liefert 3', numbers, {'SurveyStuns'})
    if nunknown:
        raise AssertionError('a method name was reported as a numeric setting')

    wrong, _ = check('`AlphaSetting` steht **an** als Vorgabewert.', defaults)
    if not any(n == 'AlphaSetting' for n, _, _ in wrong):
        raise AssertionError('the wording "an als Vorgabewert" went unnoticed')
    wrong, _ = check('`BetaSetting`, ab Werk aus.', defaults)
    if not any(n == 'BetaSetting' for n, _, _ in wrong):
        raise AssertionError('the wording "ab Werk aus" went unnoticed')
    wrong, _ = check('`AlphaSetting` steht ab Werk andersherum.', defaults)
    if wrong:
        raise AssertionError('"an" inside a longer word was read as a claim')

    source = ('[UI("Heal ahead of a cast", Filter = X)]\n public bool HealAhead { get; set; } = true;\n'
              '[UI("Cast count for that", Filter = X)]\n public int CastCount { get; set; } = 3;\n'
              '[RotationConfig(CombatType.PvE, Name = "Use it on a pull")]\n public bool UseOnPull { get; set; } = false;')
    labels = label_map(source)
    if labels.get('heal ahead of a cast') != ('HealAhead', 'true') or 'cast count for that' in labels:
        raise AssertionError('labels were not mapped to their own setting: %s' % labels)
    if not check_labels('| `Heal ahead of a cast` | Wird geheilt? | **aus** als Vorgabewert |', labels):
        raise AssertionError('a wrong default stated by the setting text went unnoticed')
    if check_labels('Option „Use it on a pull", ab Werk aus.', labels):
        raise AssertionError('a correct default stated by the setting text was reported')
    if check_labels('„Some quoted words", Standard an.', labels):
        raise AssertionError('a quotation that is no setting text was scored')

    resolved = resolve({'Twice': [(True, 'true'), (False, 'false')], 'Split': [(False, 'true'), (False, 'false')],
                        'Foreign': [(True, 'true')]})
    if resolved != {'Twice': 'false', 'Foreign': 'true'}:
        raise AssertionError('a name defined in several rotations was not resolved: %s' % resolved)

    print('self-test ok: a matching claim passes; a contradicting one is caught in both wordings; '
          'a claim\n  about an unknown setting is reported; a sentence naming two settings, and one '
          'naming a method,\n  are both left alone; the same four cases hold for the numeric '
          'thresholds; claims by setting text\n  and in the wordings "ab Werk" and "als Vorgabewert" '
          'are read.')


def main():
    selftest()
    labels = {}
    defaults, numbers, methods = code_defaults(labels=labels)
    if not defaults:
        print('no bool settings found in the source tree - the check cannot say anything')
        return 1

    wrong_all, unknown_all, counted = [], [], 0
    for path in sorted(CONCEPT_DIR.glob('*.md')) + [d for d in EXTRA_DOCS if d.exists()]:
        text = path.read_text(encoding='utf-8')
        counted += len(claims(text))
        counted += len(numeric_claims(text))
        wrong, unknown = check(text, defaults, methods)
        wrong_all += [(str(path), *w) for w in wrong]
        unknown_all += [(str(path), *u) for u in unknown]
        lwrong = check_labels(text, labels)
        counted += len(lwrong)
        wrong_all += [(str(path), *w) for w in lwrong]
        nwrong, nunknown = check_numbers(text, numbers, methods)
        wrong_all += [(str(path), n, s, a) for n, s, a in nwrong]
        unknown_all += [(str(path), n, s) for n, s in nunknown]

    print('%d bool and %d numeric settings in the tree, %d statements about defaults in the '
          'concepts\nand the release description.' % (len(defaults), len(numbers), counted))

    if unknown_all:
        print('\nNamed with a default, but no such setting in the code (report):')
        for doc, name, stated in unknown_all:
            print('  %-42s `%s` stated %s' % (doc, name, stated))

    if wrong_all:
        print('\nThe code says otherwise:')
        for doc, name, stated, actual in wrong_all:
            print('  %-42s `%s` stated %s, code has %s' % (doc, name, stated, actual))
        return 1

    print('\nEvery stated default matches the code.')
    return 0


if __name__ == '__main__':
    try:
        sys.exit(main())
    except BrokenPipeError:
        sys.stderr.close()
        sys.exit(0)
