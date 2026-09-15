#!/usr/bin/env python3
"""Check the fork's Resources/*.json against the copies the plugin actually loads.

The curated lists - which casts are tankbusters, which are area attacks, which statuses are
invincibilities - are not read from the tree. `OtherConfiguration.InitOne` loads the user's local
file if it exists and otherwise downloads

    https://raw.githubusercontent.com/{Service.USERNAME}/{Service.REPO}/main/Resources/<name>.json

and both constants name **upstream**: FFXIV-CombatReborn/RotationSolverReborn. The copies under
Resources/ in this fork are therefore decoration. Editing one to add a tankbuster id would change
nothing, and nothing would fail - the plugin would keep loading upstream's list, and the author
would have no way to tell.

That is the class this guards: a change that is never loaded. A difference is reported as a
finding, because it means either the edit needs to go upstream instead, or USERNAME/REPO were
repointed and the tree has to follow.

On a network failure the check is **skipped**, not failed: it would otherwise turn an outage into
a red run with no defect behind it, and a check that cries wolf gets switched off.
"""

import json
import os
import sys
import urllib.error
import urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
RESOURCES = os.path.join(ROOT, 'Resources')
SERVICE = os.path.join(ROOT, 'RotationSolver.Basic', 'Service.cs')
TIMEOUT = 30


def download_source():
    """(username, repo) as Service.cs declares them - the constants InitOne builds its URL from."""
    username = repo = None
    with open(SERVICE, encoding='utf-8') as handle:
        for line in handle:
            for name, const in (('USERNAME', 'username'), ('REPO', 'repo')):
                if 'const string %s' % name in line and '"' in line:
                    value = line.split('"')[1]
                    if const == 'username':
                        username = value
                    else:
                        repo = value
    return username, repo


def comparable(data):
    """The list as a set of strings, so ordering and int-vs-string spelling do not matter.

    These files are sets of ids or names; only membership decides behaviour. Comparing the raw
    text would report every reformatting as a difference, which is the surrogate, not the property.
    """
    if isinstance(data, dict):
        return {'%s=%s' % (k, data[k]) for k in data}
    if isinstance(data, list):
        return {json.dumps(x, sort_keys=True) for x in data}
    return {json.dumps(data, sort_keys=True)}


def fetch(url):
    with urllib.request.urlopen(url, timeout=TIMEOUT) as response:
        return json.loads(response.read().decode('utf-8'))


def self_test():
    if comparable([1, 2, 3]) != comparable([3, 2, 1]):
        raise AssertionError('ordering must not count as a difference')
    if comparable([1, 2]) == comparable([1, 2, 3]):
        raise AssertionError('a missing member must count as a difference')
    if comparable({'a': 1}) == comparable({'a': 2}):
        raise AssertionError('a changed value must count as a difference')

    username, repo = download_source()
    if not username or not repo:
        raise AssertionError('could not read USERNAME/REPO from Service.cs - the URL this check '
                             'compares against is built from them, so a silent miss here would '
                             'make the whole run meaningless')

    print('self-test ok: membership decides, ordering does not, and the download source reads as '
          '%s/%s' % (username, repo))
    print()


def main():
    self_test()

    username, repo = download_source()
    names = sorted(n[:-5] for n in os.listdir(RESOURCES) if n.endswith('.json'))
    print('%d list(s) in Resources/, compared against %s/%s' % (len(names), username, repo))
    print()

    differing, skipped = [], []
    for name in names:
        path = os.path.join(RESOURCES, name + '.json')
        url = ('https://raw.githubusercontent.com/%s/%s/main/Resources/%s.json'
               % (username, repo, name))
        try:
            remote = fetch(url)
        except urllib.error.HTTPError as error:
            if error.code == 404:
                differing.append('%s is not in %s/%s at all - the plugin cannot load it, so the '
                                 'copy here is unreachable' % (name, username, repo))
            else:
                skipped.append('%s: HTTP %d' % (name, error.code))
            continue
        except Exception as error:                      # network, TLS, malformed JSON
            skipped.append('%s: %s' % (name, error))
            continue

        with open(path, encoding='utf-8') as handle:
            local = json.load(handle)

        here, there = comparable(local), comparable(remote)
        if here != there:
            differing.append('%s differs: %d entr(ies) only here, %d only upstream'
                             % (name, len(here - there), len(there - here)))

    if skipped:
        print('%d list(s) could not be fetched and are not judged:' % len(skipped))
        for item in skipped:
            print('  ' + item)
        print()

    if differing:
        print('%d list(s) the plugin would not load as written here:' % len(differing))
        for item in differing:
            print('  ' + item)
        print()
        print('Resources/ is not read at runtime. Either the change belongs upstream, or')
        print('Service.USERNAME/REPO were repointed and these copies are now the source.')
        return 1

    if len(skipped) == len(names):
        print('Nothing could be fetched - treated as skipped, not as a pass.')
        return 0

    print('Every fetched list matches what the plugin loads. A change made here alone would')
    print('have no effect, which is what this check exists to notice.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
