#!/usr/bin/env python3
"""Do the concepts still state the defaults the code actually has?

A concept that says "default off" about a setting the code ships as on is worse than one that says
nothing: the next pass reads it, recommends switching the setting on, and the owner has had it on
all along. That has happened here (`StretchHolyStun`, see CLAUDE.md), and nothing fails when it
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
# ageing with a shorter fuse than a concept's, so it is measured with the concepts.
EXTRA_DOCS = (Path('docs/fork-changes-in-play.md'),)

SOURCE_DIRS = (Path('RotationSolver.Basic'), Path('RotationSolver'))

# `public bool Name { get; set; } = true;` - the form both the configuration class and every
# rotation file use for a switchable setting.
DEFINITION = re.compile(r'public\s+bool\s+([A-Za-z0-9_]+)\s*\{\s*get;\s*set;\s*\}\s*=\s*(true|false)\s*;')

# The wordings the concepts actually use, German and English, in the sentence around the name.
ON = r'(?:Standard\s+an|voreingestellt\s+an|Voreinstellung\s+an|on\s+by\s+default|Standard\s+ein)'
OFF = r'(?:Standard\s+aus|voreingestellt\s+aus|Voreinstellung\s+aus|off\s+by\s+default)'
CLAIM = re.compile(r'`([A-Z][A-Za-z0-9_]{3,})`(?P<between>[^`]{0,200}?)(?P<value>' + ON + '|' + OFF + ')',
                   re.IGNORECASE | re.DOTALL)

# A sentence may name a second setting between the first name and the default - then the default
# belongs to the nearer name, not the first. Bailing out is the honest answer: report it as
# unmatched rather than guess which one it meant.
ANOTHER_NAME = re.compile(r'`[A-Z][A-Za-z0-9_]{3,}`')

# Concepts name methods in the same backticks as settings, and a method mentioned anywhere near the
# word "default" is not a claim about a default. The first run reported four of them and nothing
# else - a report made entirely of noise teaches its reader to skip it.
METHOD = re.compile(r'\b(?:bool|void|int|float|string|Task|IAction)\s+([A-Za-z0-9_]+)\s*\(')


def code_defaults(dirs=SOURCE_DIRS):
    """Returns ({setting: 'true'|'false'}, {method names}) over the configuration class and the rotations."""
    found, methods = {}, set()
    for directory in dirs:
        if not directory.is_dir():
            continue
        for path in directory.rglob('*.cs'):
            text = path.read_text(encoding='utf-8', errors='replace')
            for name, value in DEFINITION.findall(text):
                found.setdefault(name, value)
            methods.update(METHOD.findall(text))
    return found, methods


def claims(text):
    """Returns [(setting, 'on'|'off')] stated in `text`, skipping ambiguous sentences."""
    out = []
    for m in CLAIM.finditer(text):
        if ANOTHER_NAME.search(m.group('between')):
            continue
        stated = 'off' if re.search(OFF, m.group('value'), re.IGNORECASE) else 'on'
        out.append((m.group(1), stated))
    return out


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

    print('self-test ok: a matching claim passes; a contradicting one is caught in both wordings; '
          'a claim\n  about an unknown setting is reported; a sentence naming two settings, and one '
          'naming a method,\n  are both left alone.')


def main():
    selftest()
    defaults, methods = code_defaults()
    if not defaults:
        print('no bool settings found in the source tree - the check cannot say anything')
        return 1

    wrong_all, unknown_all, counted = [], [], 0
    for path in sorted(CONCEPT_DIR.glob('*.md')) + [d for d in EXTRA_DOCS if d.exists()]:
        text = path.read_text(encoding='utf-8')
        counted += len(claims(text))
        wrong, unknown = check(text, defaults, methods)
        wrong_all += [(str(path), *w) for w in wrong]
        unknown_all += [(str(path), *u) for u in unknown]

    print('%d bool settings in the tree, %d statements about defaults in the concepts and the '
          'release description.' % (len(defaults), counted))

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
