#!/usr/bin/env python3
"""Hold the movement lock's channel list against the game data.

`MovementLock.Channels` (RotationSolver/Basic/Helpers/MovementLock.cs) names the channels the
movement lock knows: the player actions whose effect text says "Effect ends upon using another
action or moving" (concept 03, "Bewegungssperre beim Wirken und die KI von BossMod", A651). A
channel the list lacks is walked off by BossMod's AI with nothing to hold it - before A651 that
was Collective Unconscious and Meditate. A patch can add one; this reads the game's ActionTransient
sheet (xivapi v2) for the sentence, keeps the actions of a job at a level (PvE, ClassJobLevel > 0:
Eureka's and Bozja's actions carry level 0) and fails for every one the list lacks or names
without the game saying so. An unreachable xivapi skips the comparison, so an outage is not a
defect.

    python3 .github/scripts/audit/check_channel_locks.py [--self-test]
"""
import json
import re
import subprocess
import sys
import urllib.parse
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
LOCK = ROOT / 'RotationSolver/Basic/Helpers/MovementLock.cs'
ACTION_IDS = ROOT / 'RotationSolver.SourceGenerators/Properties/ActionId.resx'
API = 'https://v2.xivapi.com/api'
SENTENCE = 'or moving'


def fetch_json(url):
    """The parsed answer, or None when xivapi cannot be reached (curl, as check_action_names.py)."""
    try:
        out = subprocess.run(['curl', '-sS', '--max-time', '30', url], capture_output=True, text=True)
        return json.loads(out.stdout) if out.returncode == 0 and out.stdout else None
    except (OSError, ValueError):
        return None


def listed_identifiers(source):
    """The ActionID names of the channel table, in order."""
    start = source.find('internal static readonly Channel[] Channels')
    end = source.find('];', start)
    if start < 0 or end < 0:
        return []
    return re.findall(r'new\(ActionID\.(\w+)', source[start:end])


def resolve(identifiers, resx_text):
    """Row id per identifier from the generated action ids; a name without a row stays out."""
    ids = {}
    for name in identifiers:
        m = re.search(r'^' + re.escape(name) + r' = (\d+),', resx_text, re.M)
        if m:
            ids[name] = int(m.group(1))
    return ids


def game_channels(fetch=fetch_json):
    """Row id -> name of the player actions at a job level whose effect text ends them upon moving; None if unreachable."""
    query = urllib.parse.quote(f'Description~"{SENTENCE}"')
    found = fetch(f'{API}/search?sheets=ActionTransient&query={query}&fields=Description&limit=500')
    if found is None or 'results' not in found:
        return None
    rows = sorted({r['row_id'] for r in found['results']})
    if not rows:
        return {}
    detail = fetch(f"{API}/sheet/Action?rows={','.join(map(str, rows))}&fields=Name,ClassJobLevel,IsPvP")
    if detail is None or 'rows' not in detail:
        return None
    return {r['row_id']: r['fields'].get('Name', '') for r in detail['rows']
            if r['fields'].get('ClassJobLevel', 0) > 0 and not r['fields'].get('IsPvP', False)}


def problems(listed, game):
    """Findings: channels the game has and the list lacks, and listed ones the game does not end on moving.

    A row named like a listed one is the same channel - Phantom Flurry's second press (23289) is the finisher that
    ends the channel the first one (23288) starts, with the same effect text."""
    out = []
    by_id = {v: k for k, v in listed.items()}
    listed_names = {game[row] for row in listed.values() if row in game}
    for row in sorted(set(game) - set(listed.values())):
        if game[row] not in listed_names:
            out.append(f'Action {row} ({game[row]}) ends upon moving (ActionTransient) but MovementLock.Channels lacks it')
    for row in sorted(set(listed.values()) - set(game)):
        out.append(f'MovementLock.Channels lists {by_id[row]} ({row}), whose effect text does not end it upon moving')
    return out


def self_test():
    listed = {'PassageOfArmsPvE': 7385, 'FlamethrowerPvE': 7418}
    both = {7385: 'Passage of Arms', 7418: 'Flamethrower'}
    assert problems(listed, both) == [], 'a matching list was reported'
    assert len(problems(listed, {**both, 3613: 'Collective Unconscious'})) == 1, 'a channel the list lacks was missed'
    assert problems(listed, {**both, 9999: 'Flamethrower'}) == [], 'a second press of a listed channel was reported'
    assert len(problems(listed, {7385: 'Passage of Arms'})) == 1, 'a listed action the game does not end on moving was missed'
    table = 'internal static readonly Channel[] Channels =\n[\n\tnew(ActionID.PassageOfArmsPvE, x),\n\tnew(ActionID.MeditatePvE, y),\n];'
    assert listed_identifiers(table) == ['PassageOfArmsPvE', 'MeditatePvE'], 'the table was misread'
    assert resolve(['MeditatePvE', 'Missing'], 'MeditatePvE = 7497,\n') == {'MeditatePvE': 7497}, 'ids misresolved'
    assert game_channels(fetch=lambda url: None) is None, 'an unreachable xivapi was not told apart'
    sheet = {'results': [{'row_id': 7385}, {'row_id': 21611}]}
    detail = {'rows': [{'row_id': 7385, 'fields': {'ClassJobLevel': 70, 'IsPvP': False}},
                       {'row_id': 21611, 'fields': {'ClassJobLevel': 0, 'IsPvP': False}}]}
    assert game_channels(fetch=lambda url: sheet if 'search' in url else detail) == {7385: ''}, 'level 0 kept'
    print('self-test ok: a lacking and a surplus channel are caught, a matching list and a second press of a listed '
          'channel pass, level-0 special actions are left out, an unreachable xivapi is told apart')


def main():
    if '--self-test' in sys.argv:
        self_test()
        return 0
    self_test()
    identifiers = listed_identifiers(LOCK.read_text(encoding='utf-8'))
    listed = resolve(identifiers, ACTION_IDS.read_text(encoding='utf-8'))
    unresolved = [n for n in identifiers if n not in listed]
    if unresolved:
        print('MovementLock.Channels names actions without an id: ' + ', '.join(unresolved))
        return 1
    game = game_channels()
    if game is None:
        print('xivapi is not reachable: the channel list was not held against the game this run.')
        return 0
    found = problems(listed, game)
    for line in found:
        print(line)
    if found:
        return 1
    print(f'{len(listed)} channel(s) in MovementLock.Channels, as the game data has them.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
