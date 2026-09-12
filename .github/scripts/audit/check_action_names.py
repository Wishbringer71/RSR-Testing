#!/usr/bin/env python3
"""Validate the German-to-English name dictionary against the generated resources.

The user plays a German client, so every name he states is a German game name while
the tree speaks English identifiers. Looking each one up again costs a research pass
per mention and has produced wrong assignments more than once, so the pairs live in
action_names_de.json and this script keeps them honest:

  * every entry resolves to an identifier that actually exists in the generated
    resources, so a typo or a renamed action fails the build instead of quietly
    pointing nowhere;
  * every entry names its source, because a German name may only come from the job
    guide or from the user - never from a translation or a search summary;
  * duplicate German names are reported, and duplicate English ones are allowed but
    listed, since that is exactly the Abtausch/Rückstoß case.

Run without arguments to validate and print the table.
"""

import json
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
DICT_PATH = os.path.join(HERE, 'action_names_de.json')
RESX = os.path.join(ROOT, 'RotationSolver.SourceGenerators', 'Properties', 'ActionId.resx')

VALID_KINDS = ('action', 'status', 'item')


def load_identifiers(text):
    """Every enum member name the generated resource declares."""
    return set(re.findall(r'\n([A-Za-z][A-Za-z0-9_]*) = \d+,', text))


def load_dictionary(path=DICT_PATH):
    with open(path, encoding='utf-8') as handle:
        return json.load(handle)


def validate(data, identifiers):
    """Return a list of problems; empty means the dictionary is sound."""
    problems = []
    seen_de = {}

    for entry in data.get('entries', []):
        de = entry.get('de')
        en = entry.get('en')
        ident = entry.get('identifier')
        kind = entry.get('kind')
        source = entry.get('source')

        where = de or en or '<unnamed entry>'

        if not de or not en:
            problems.append('%s: needs both a German and an English name' % where)
            continue
        if not source:
            problems.append('%s: no source - a German name without one is not admissible'
                            % where)
        if kind not in VALID_KINDS:
            problems.append('%s: kind %r is not one of %s' % (where, kind, ', '.join(VALID_KINDS)))
            continue

        if de in seen_de and seen_de[de] != en:
            problems.append('%s: given for both %s and %s' % (de, seen_de[de], en))
        seen_de[de] = en

        # An action has to exist in the generated enum. Items and statuses are
        # identified by number or live in a different resource, so only their shape
        # is checked here - claiming more would be a check that cannot fail.
        if kind == 'action':
            if not ident:
                problems.append('%s: an action entry needs its identifier' % where)
            elif ident not in identifiers:
                problems.append('%s: identifier %s is not in ActionId.resx' % (where, ident))
        elif not ident:
            problems.append('%s: needs an identifier (an id is fine)' % where)

    return problems


def self_test(identifiers):
    """Constructed defects, so a silent pass cannot be mistaken for a clean file."""
    good = {'entries': [{'de': 'Sanctus', 'en': 'Holy', 'identifier': 'HolyPvE',
                         'kind': 'action', 'source': 'user'}]}
    if validate(good, identifiers):
        raise AssertionError('a sound entry was rejected')

    typo = {'entries': [{'de': 'Sanctus', 'en': 'Holy', 'identifier': 'HollyPvE',
                         'kind': 'action', 'source': 'user'}]}
    if not validate(typo, identifiers):
        raise AssertionError('an identifier that does not exist was accepted')

    unsourced = {'entries': [{'de': 'Sanctus', 'en': 'Holy', 'identifier': 'HolyPvE',
                              'kind': 'action'}]}
    if not validate(unsourced, identifiers):
        raise AssertionError('an entry without a source was accepted')

    clash = {'entries': [
        {'de': 'Sanctus', 'en': 'Holy', 'identifier': 'HolyPvE', 'kind': 'action',
         'source': 'user'},
        {'de': 'Sanctus', 'en': 'Cure', 'identifier': 'CurePvE', 'kind': 'action',
         'source': 'user'},
    ]}
    if not validate(clash, identifiers):
        raise AssertionError('one German name for two actions was accepted')

    print('self-test ok: sound entry accepted; unknown identifier, missing source and '
          'a clashing German name each caught')
    print()


def main():
    with open(RESX, encoding='utf-8') as handle:
        identifiers = load_identifiers(handle.read())
    if not identifiers:
        print('could not read any identifier from %s' % RESX)
        return 1

    self_test(identifiers)

    data = load_dictionary()
    problems = validate(data, identifiers)
    if problems:
        print('%d problem(s) in action_names_de.json:' % len(problems))
        for problem in problems:
            print('  ' + problem)
        return 1

    entries = data['entries']
    width = max(len(e['de']) for e in entries)
    print('%-*s  %-22s %s' % (width, 'German', 'English', 'identifier'))
    for entry in entries:
        print('%-*s  %-22s %s' % (width, entry['de'], entry['en'], entry['identifier']))

    # The same action under two German names is not an error, but it is worth seeing.
    by_en = {}
    for entry in entries:
        by_en.setdefault(entry['en'], []).append(entry['de'])
    doubled = {en: names for en, names in by_en.items() if len(names) > 1}
    if doubled:
        print()
        for en, names in doubled.items():
            print('%s is recorded under two German names: %s - prefer the English '
                  'identifier until the user settles it' % (en, ' and '.join(names)))

    print()
    print('%d entries, all resolvable.' % len(entries))
    return 0


if __name__ == '__main__':
    sys.exit(main())
