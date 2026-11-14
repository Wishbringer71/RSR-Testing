#!/usr/bin/env python3
"""Where a tincture pays most, by job: each job's own damage in its burst windows, measured on FFLogs (concept 03,
"Opener oder später", A781).

A tincture raises the main stat for 30 s, so it belongs in the 30-s windows with the most own damage. For the best pulls
of every job on the given encounters, this sums the player's and their pets' damage per second and takes the best 30-s
window around each burst (opener, 2, 6 and 8 minutes). The tincture's own effect is taken out twice, at 0 % and at an
upper bound (11 %, from +10 % main stat up to 461); only what holds at both bounds is a finding.

  fetch <encounter> <pulls per job>   needs FFLogsClientID and FFLogsClientSecret; writes the data into <workdir>.
  analyse <encounter>...              opener/2 min (one tincture each) and (0+6)/(2+8) (two each), per job.
  --self-test                         the window and schedule arithmetic against constructed series.

Player names stay in the work directory, which is outside the repository; nothing names a player in the output.
Usage: python3 .github/scripts/audit/tincture_windows.py fetch 101 4 | analyse 101 103 104 | --self-test
       (WORKDIR environment variable: where the fetched data goes; default ./tincture-data)
"""
import json
import os
import subprocess
import sys
import time

API = 'https://www.fflogs.com/api/v2/client'
JOBS = ['Paladin', 'Warrior', 'DarkKnight', 'Gunbreaker', 'WhiteMage', 'Scholar', 'Astrologian', 'Sage', 'Monk',
        'Dragoon', 'Ninja', 'Samurai', 'Reaper', 'Viper', 'Bard', 'Machinist', 'Dancer', 'BlackMage', 'Summoner',
        'RedMage', 'Pictomancer']
MEDICATED = 1000049  # FFLogs' id of status 49, Medicated
EFFECT = 30          # seconds of Medicated (ItemAction.Data of the gemdraughts)
UPPER_GAIN = 0.11    # upper bound of a tincture's damage gain (concept 03)
WINDOWS = {'0': (0, 30), '2': (95, 155), '6': (335, 395), '8': (455, 515)}  # where each burst's 30 s may start


def workdir():
    path = os.environ.get('WORKDIR', 'tincture-data')
    os.makedirs(path, exist_ok=True)
    return path


def token():
    out = subprocess.run(['curl', '-sS', '-u', f"{os.environ['FFLogsClientID']}:{os.environ['FFLogsClientSecret']}",
                          '-d', 'grant_type=client_credentials', 'https://www.fflogs.com/oauth/token'],
                         capture_output=True, text=True)
    return json.loads(out.stdout)['access_token']


def gql(tok, query):
    for _ in range(4):
        out = subprocess.run(['curl', '-sS', '-H', f'Authorization: Bearer {tok}', '-H', 'Content-Type: application/json',
                              '-d', json.dumps({'query': query}), API], capture_output=True, text=True)
        try:
            data = json.loads(out.stdout)
            if data.get('data'):
                return data['data']
        except ValueError:
            pass
        time.sleep(5)
    return {}


def fetch(encounter, per_job):
    tok = token()
    pulls = {}
    for job in JOBS:
        q = (f'{{ worldData {{ encounter(id:{encounter}) {{ characterRankings(className:"{job}", specName:"{job}", '
             f'metric:dps, difficulty:101, page:1) }} }} }}')
        ranks = (gql(tok, q).get('worldData') or {}).get('encounter', {}).get('characterRankings', {}).get('rankings', [])
        for rank in ranks[:per_job]:
            code, fight = rank['report']['code'], rank['report']['fightID']
            q = (f'{{ reportData {{ report(code:"{code}") {{ fights(fightIDs:[{fight}]) {{ startTime endTime }} '
                 f'masterData {{ actors {{ id name type petOwner }} }} }} }} }}')
            report = (gql(tok, q).get('reportData') or {}).get('report')
            if not report:
                continue
            start, end = report['fights'][0]['startTime'], report['fights'][0]['endTime']
            player = [a['id'] for a in report['masterData']['actors'] if a['name'] == rank['name'] and a['type'] == 'Player']
            if not player:
                continue
            sources = player + [a['id'] for a in report['masterData']['actors'] if a.get('petOwner') == player[0]]
            seconds = [0.0] * (int((end - start) / 1000) + 2)
            potted = []
            for source in sources:
                at = start
                while at is not None:
                    q = (f'{{ reportData {{ report(code:"{code}") {{ events(fightIDs:[{fight}], dataType:DamageDone, '
                         f'startTime:{at}, endTime:{end}, limit:10000, sourceID:{source}) {{ data nextPageTimestamp }} }} }} }}')
                    events = ((gql(tok, q).get('reportData') or {}).get('report') or {}).get('events')
                    if not events:
                        break
                    for e in events['data']:
                        if e.get('type') == 'damage' and 'amount' in e and 0 <= int((e['timestamp'] - start) / 1000) < len(seconds):
                            seconds[int((e['timestamp'] - start) / 1000)] += e['amount']
                    at = events.get('nextPageTimestamp')
            q = (f'{{ reportData {{ report(code:"{code}") {{ events(fightIDs:[{fight}], dataType:Buffs, '
                 f'abilityID:{MEDICATED}, limit:1000) {{ data }} }} }} }}')
            buffs = ((gql(tok, q).get('reportData') or {}).get('report') or {}).get('events') or {'data': []}
            potted = sorted({round((e['timestamp'] - start) / 1000, 1) for e in buffs['data']
                             if e.get('type') == 'applybuff' and e.get('targetID') in player})
            pulls.setdefault(job, []).append({'sec': seconds, 'pots': potted})
    with open(os.path.join(workdir(), f'dmg_{encounter}.json'), 'w', encoding='utf-8') as f:
        json.dump(pulls, f)
    print(encounter, {j: len(v) for j, v in pulls.items()})


def best_window(seconds, lo, hi):
    best = None
    for start in range(lo, hi + 1):
        if start + EFFECT > len(seconds):
            break
        total = sum(seconds[start:start + EFFECT])
        best = total if best is None or total > best else best
    return best


def windows(pull, gain):
    seconds, n = pull['sec'], len(pull['sec'])
    potted = [False] * n
    for t in pull['pots']:
        for i in range(int(t), min(n, int(t) + EFFECT)):
            potted[i] = True
    plain = [d / (1 + gain) if potted[i] else d for i, d in enumerate(seconds)]
    return {k: best_window(plain, lo, hi) for k, (lo, hi) in WINDOWS.items()}


def ratios(pulls):
    """{gain: (opener/2 min values, (0+6)/(2+8) values)} over the pulls."""
    found = {}
    for gain in (0.0, UPPER_GAIN):
        one, two = [], []
        for pull in pulls:
            w = windows(pull, gain)
            if w['0'] and w['2']:
                one.append(w['0'] / w['2'])
            if all(w[k] for k in '0268'):
                two.append((w['0'] + w['6']) / (w['2'] + w['8']))
        found[gain] = (one, two)
    return found


def verdict(found):
    """'2 min' when both cases are below 1 at both gains, 'opener' when above, else 'open'."""
    means = []
    for gain in found:
        for values in found[gain]:
            if not values:
                return 'open'
            means.append(sum(values) / len(values))
    return '2 min' if max(means) < 1 else 'opener' if min(means) > 1 else 'open'


def analyse(encounters):
    data = {}
    for encounter in encounters:
        with open(os.path.join(workdir(), f'dmg_{encounter}.json'), encoding='utf-8') as f:
            for job, pulls in json.load(f).items():
                data.setdefault(job, []).extend(pulls)
    for job, pulls in data.items():
        found = ratios(pulls)
        cells = []
        for case in (0, 1):
            low, high = found[UPPER_GAIN][case], found[0.0][case]
            cells.append('n/a' if not high else f'{sum(low) / len(low):.3f}..{sum(high) / len(high):.3f} (n={len(high)})')
        print(f'{job:12s} | opener/2min {cells[0]:24s} | (0+6)/(2+8) {cells[1]:24s} | {verdict(found)}')


def self_test():
    flat = {'sec': [10.0] * 600, 'pots': []}
    w = windows(flat, 0.0)
    assert w == {'0': 300.0, '2': 300.0, '6': 300.0, '8': 300.0}, w
    burst = {'sec': [10.0] * 600, 'pots': [0.0]}
    for i in range(0, 30):
        burst['sec'][i] = 11.0  # the opener carries a tincture worth +10 %
    for i in range(120, 150):
        burst['sec'][i] = 12.0  # the 2-minute burst is stronger by itself
    w0, w11 = windows(burst, 0.0), windows(burst, 0.10)
    assert w0['0'] == 330.0 and abs(w11['0'] - 300.0) < 1e-6 and w0['2'] == 360.0, (w0, w11)
    assert verdict(ratios([burst])) == '2 min', ratios([burst])
    strong = {'sec': [10.0] * 600, 'pots': []}
    for i in list(range(0, 30)) + list(range(360, 390)):
        strong['sec'][i] = 14.0
    assert verdict(ratios([strong])) == 'opener', ratios([strong])
    short = {'sec': [10.0] * 200, 'pots': []}
    assert ratios([short])[0.0][1] == [], 'a pull too short for the 8-minute window gives no two-tincture ratio'
    print('self-test ok: best 30-s windows, the tincture taken out at both bounds, both verdicts, short pulls left out')
    return 0


def main(argv):
    if argv[:1] == ['--self-test']:
        return self_test()
    if len(argv) == 3 and argv[0] == 'fetch':
        fetch(int(argv[1]), int(argv[2]))
        return 0
    if len(argv) >= 2 and argv[0] == 'analyse':
        analyse(argv[1:])
        return 0
    print(__doc__)
    return 2


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
