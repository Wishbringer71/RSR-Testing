"""Fetch the traits that extend an action's duration - the second source for generate_defensive_values.py.

ActionId.resx leaves the number blank where a trait changes it ("Duration: s"), so the duration
of Reprisal, Feint, Addle, Sheltron and Nascent Flash is missing from the effect texts (concept 09,
A266). The game states it in the trait that changes it: "Extends the duration of Reprisal to 15
seconds", learned at level 98. In the game the plugin reads that trait itself (TraitDurations) - what
the game gives at runtime weighs more (the owner's rule, 12.10.2026). This table, fetched from the
game data through xivapi with date and source, is the fallback where the game gives nothing and the
cross-check: weighting, not exclusion (his precision).

Run when the game data changes:  python3 .github/scripts/audit/fetch_trait_durations.py
"""
import datetime
import json
import pathlib
import re
import subprocess
import sys

OUT = pathlib.Path(__file__).with_name('trait_durations.json')
API = 'https://v2.xivapi.com/api'
EXTENDS = re.compile(r'Extends the duration of (.+?) to (\d{1,3}) seconds')


def fetch(url):
    # curl, not urllib: the environment's proxy and certificate setup is configured for curl
    return json.loads(subprocess.run(['curl', '-sS', url], capture_output=True, text=True, check=True).stdout)


def parse(text):
    """(action name, seconds) from a trait's text, or None."""
    match = EXTENDS.search(re.sub(r'<[^>]+>', '', text or ''))
    if not match:
        return None
    # "Nascent Flash and Nascent Glint" extends both; the action is the first
    name = match.group(1).split(' and ')[0].strip()
    return name, int(match.group(2))


def self_test():
    cases = [
        ('Extends the duration of Reprisal to 15 seconds.', ('Reprisal', 15)),
        ('Extends the duration of Nascent Flash and Nascent Glint to 8 seconds.', ('Nascent Flash', 8)),
        ('Extends the duration of Bio and Miasma applied by Bane to the full duration', None),
        ('Upgrades Sheltron to Holy Sheltron.', None),
    ]
    for text, expected in cases:
        got = parse(text)
        if got != expected:
            print(f'SELF-TEST FAILED: {text!r} -> {got}, expected {expected}')
            return False
    print('self-test ok: a trait naming one or two actions is read, other trait texts are left out')
    return True


def main():
    if not self_test():
        return 2
    found = fetch(f'{API}/search?sheets=TraitTransient&query=Description~%22Extends%20the%20duration%20of%22'
                  '&fields=Description&limit=500')
    rows = {}
    for result in found.get('results', []):
        parsed = parse(result['fields']['Description'])
        if parsed:
            rows[result['row_id']] = parsed
    levels = fetch(f'{API}/sheet/Trait?rows={",".join(map(str, rows))}&fields=Name,Level&limit=500')
    traits = []
    for row in levels.get('rows', []):
        name, seconds = rows[row['row_id']]
        level = row['fields']['Level']
        if level > 0:
            traits.append({'trait': row['fields']['Name'], 'trait_row': row['row_id'], 'level': level,
                           'action': name, 'seconds': seconds})
    traits.sort(key=lambda t: (t['action'], t['level']))
    OUT.write_text(json.dumps({
        'source': 'xivapi v2, sheets TraitTransient (Description) and Trait (Level)',
        'fetched': datetime.date.today().isoformat(),
        'traits': traits,
    }, indent=2) + '\n', encoding='utf-8')
    print(f'{len(traits)} trait(s) written to {OUT.name}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
