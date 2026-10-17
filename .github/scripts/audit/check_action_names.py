#!/usr/bin/env python3
"""Validate the German-to-English name dictionary against the generated resources and the game.

The user plays a German client, so every name he states is a German game name while
the tree speaks English identifiers. Looking each one up again costs a research pass
per mention and has produced wrong assignments more than once, so the pairs live in
action_names_de.json and this script keeps them honest:

  * every entry resolves to an identifier that actually exists in the generated
    resources, so a typo or a renamed action fails the build instead of quietly
    pointing nowhere;
  * every entry names an admissible source - the user, the job guide, or the game's own
    sheets - never a translation or a search summary, and an entry that does not fails;
  * every entry is held against the game's German sheets (xivapi v2, language=de): the
    action, item or job its identifier names has to carry exactly that German name. A
    contradiction fails; an unreachable xivapi skips this part, so an outage is not a defect;
  * duplicate German names are reported, and duplicate English ones are allowed but listed.

Run without arguments to validate and print the table.
"""

import json
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
DICT_PATH = os.path.join(HERE, 'action_names_de.json')
RESX = os.path.join(ROOT, 'RotationSolver.SourceGenerators', 'Properties', 'ActionId.resx')

VALID_KINDS = ('action', 'status', 'item', 'job')


def load_identifiers(text):
    """Every enum member the generated resource declares, name to row id."""
    return {name: int(row) for name, row in re.findall(r'\n([A-Za-z][A-Za-z0-9_]*) = (\d+),', text)}


def load_dictionary(path=DICT_PATH):
    with open(path, encoding='utf-8') as handle:
        return json.load(handle)


# The naming rule admits the user's own statement, the Square Enix job guide and the game's own
# sheets in their German version - a name is never formed, translated or taken from a search
# summary. The rule exists because guessing produced "Armlänge" for Arm's Length, which the German
# client calls Abtausch. Checking only that *a* source is named would pass an entry that names an
# inadmissible one.
ADMISSIBLE_SOURCES = ('user', 'job guide', 'jobguide', 'game data', 'xivapi')


def source_is_admissible(source):
    lowered = (source or '').lower()
    return any(token in lowered for token in ADMISSIBLE_SOURCES)


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

    if source_is_admissible('C30, via web search'):
        raise AssertionError('a web search was accepted as an admissible source')
    if not source_is_admissible('user, 2026-09-12'):
        raise AssertionError('the user\'s own statement was rejected as a source')
    if not source_is_admissible('Square Enix job guide'):
        raise AssertionError('the job guide was rejected as a source')

    # The game check, against a constructed answer: a matching name passes, a different one is a
    # problem, an unreachable xivapi skips (A286, G2).
    some = sorted(identifiers)[0]
    entries = {'entries': [{'de': 'Eins', 'en': 'One', 'identifier': some, 'kind': 'action', 'source': 'user'},
                           {'de': 'Feder', 'en': 'Down', 'identifier': '4570', 'kind': 'item', 'source': 'user'},
                           {'de': 'Paladin', 'en': 'Paladin', 'identifier': 'PLD', 'kind': 'job', 'source': 'user'}]}

    def answer(name_of_action, name_of_item):
        def fake(url):
            if '/Action?' in url:
                return {'rows': [{'row_id': identifiers[some], 'fields': {'Name': name_of_action}}]}
            if '/Item?' in url:
                return {'rows': [{'row_id': 4570, 'fields': {'Name': name_of_item}}]}
            if 'language=en' in url:
                return {'rows': [{'row_id': 19, 'fields': {'Abbreviation': 'PLD'}}]}
            return {'rows': [{'row_id': 19, 'fields': {'Name': 'Paladin'}}]}
        return fake

    if check_against_game(entries, identifiers, answer('Eins', 'Feder')):
        raise AssertionError('names the game confirms were reported')
    if len(check_against_game(entries, identifiers, answer('Zwei', 'Feder')) or []) != 1:
        raise AssertionError('a name the game contradicts was not reported')
    if check_against_game(entries, identifiers, lambda url: None) is not None:
        raise AssertionError('an unreachable xivapi was not told apart from a clean result')

    print('self-test ok: sound entry accepted; unknown identifier, missing source, '
          'an inadmissible source and '
          'a clashing German name each caught; a game name confirmed, contradicted and unreachable '
          'told apart')
    print()


API = 'https://v2.xivapi.com/api'


def fetch_json(url):
    """The parsed answer, or None when xivapi cannot be reached. curl, not urllib: the proxy and
    certificate setup of the environments this runs in is configured for curl."""
    try:
        out = subprocess.run(['curl', '-sS', '--max-time', '30', url], capture_output=True, text=True)
        return json.loads(out.stdout) if out.returncode == 0 and out.stdout else None
    except (OSError, ValueError):
        return None


def game_names(entries, identifiers, fetch=fetch_json):
    """German game name per entry (by position), or None when the game data cannot be read.

    Actions by the row id of their identifier, items by their id, jobs by the English
    abbreviation. A missing row is an empty name, which then contradicts the entry.
    """
    wanted = {'action': {}, 'item': {}}
    for i, entry in enumerate(entries):
        if entry.get('kind') == 'action' and entry.get('identifier') in identifiers:
            wanted['action'][i] = identifiers[entry['identifier']]
        elif entry.get('kind') == 'item' and str(entry.get('identifier', '')).isdigit():
            wanted['item'][i] = int(entry['identifier'])

    names = {}
    for kind, sheet in (('action', 'Action'), ('item', 'Item')):
        rows = sorted(set(wanted[kind].values()))
        if not rows:
            continue
        answer = fetch(f'{API}/sheet/{sheet}?rows={",".join(map(str, rows))}&fields=Name&language=de')
        if answer is None:
            return None
        by_row = {r['row_id']: r['fields'].get('Name', '') for r in answer.get('rows', [])}
        for i, row in wanted[kind].items():
            names[i] = by_row.get(row, '')

    if any(e.get('kind') == 'job' for e in entries):
        english = fetch(f'{API}/sheet/ClassJob?fields=Abbreviation&limit=100&language=en')
        german = fetch(f'{API}/sheet/ClassJob?fields=Name&limit=100&language=de')
        if english is None or german is None:
            return None
        row_of = {r['fields'].get('Abbreviation'): r['row_id'] for r in english.get('rows', [])}
        name_of = {r['row_id']: r['fields'].get('Name', '') for r in german.get('rows', [])}
        for i, entry in enumerate(entries):
            if entry.get('kind') == 'job':
                names[i] = name_of.get(row_of.get(entry.get('identifier')), '')
    return names


def check_against_game(data, identifiers, fetch=fetch_json):
    """Problems where an entry's German name is not the game's; None when xivapi is unreachable."""
    entries = data.get('entries', [])
    names = game_names(entries, identifiers, fetch)
    if names is None:
        return None
    return ['%s is recorded for %s, but the German game data names it %r'
            % (entries[i]['de'], entries[i].get('identifier'), name)
            for i, name in sorted(names.items()) if name != entries[i]['de']]


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

    # An entry whose source the naming rule does not admit fails: the job guide and the game's
    # German sheets are both reachable (since 27.09.2026), so such a name can always be replaced.
    inadmissible = [e for e in entries if not source_is_admissible(e.get('source'))]
    if inadmissible:
        print()
        print('%d entr%s on a source the naming rule does not admit:'
              % (len(inadmissible), 'y rests' if len(inadmissible) == 1 else 'ies rest'))
        for entry in inadmissible:
            print('  %s = %s (source: %s)' % (entry['de'], entry['en'], entry.get('source')))
        return 1

    contradicted = check_against_game(data, identifiers)
    print()
    if contradicted is None:
        print('xivapi is not reachable: the German names were not held against the game this run.')
    elif contradicted:
        print('%d entr%s the German game data contradicts:'
              % (len(contradicted), 'y' if len(contradicted) == 1 else 'ies'))
        for problem in contradicted:
            print('  ' + problem)
        return 1
    else:
        print('every German name matches the game data (xivapi v2, language=de).')

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
    print('%d hand-kept entries, all resolvable.' % len(entries))
    return 0


if __name__ == '__main__':
    sys.exit(main())
