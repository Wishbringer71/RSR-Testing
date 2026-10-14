#!/usr/bin/env python3
"""Does <Version> in Directory.Build.props still name the upstream release the fork sits on?

Upstream carries no version at all in its source tree: its `Directory.Build.props` has no
`<Version>`, and `publish.yaml` derives every version property from the git tag that triggered the
run. The three version lines in this fork's copy are therefore a fork addition, added so a build
without a tag still has an identity and so the package can carry the `-wsh1` fork marker.

That has a consequence worth a check: **nothing updates the number**. Merging upstream cannot bring
it along, because upstream has nothing to bring. It is a hand-maintained figure mirroring a fact
that lives somewhere else - the newest upstream tag in this branch's history - which is the same
aging shape as a hand-maintained status list, and it went stale the same way.

The check compares `<Version>` against the highest upstream tag that is an ancestor of HEAD. Tags
upstream published but this branch has not merged are ignored on purpose: the number claims which
upstream release the fork is *based on*, not which one exists.

Usage: python3 .github/scripts/audit/check_fork_version.py [--remote upstream] [--require-tags]
Exit code 1 when the number is behind. The CI runs it with --require-tags after fetching the
upstream tags, so a run that finds no tag to compare against fails instead of passing unchecked.
"""
import re
import subprocess
import sys

PROPS = 'Directory.Build.props'
VERSION = re.compile(r'<Version>([^<]+)</Version>')
NUMERIC_TAG = re.compile(r'^\d+(?:\.\d+)*$')


def run(*args):
    out = subprocess.run(args, capture_output=True, text=True)
    return out.stdout.strip() if out.returncode == 0 else ''


def key(tag):
    return tuple(int(p) for p in tag.split('.'))


def upstream_tags(remote):
    """Numeric tags of `remote` that are ancestors of HEAD, newest first."""
    listing = run('git', 'ls-remote', '--tags', remote)
    names = set()
    for line in listing.split('\n'):
        if not line or line.endswith('^{}'):
            continue
        parts = line.split('refs/tags/')
        if len(parts) == 2 and NUMERIC_TAG.match(parts[1]):
            names.add(parts[1])

    reachable = []
    for tag in names:
        sha = run('git', 'rev-list', '-n', '1', tag)
        if not sha:
            continue
        if subprocess.run(['git', 'merge-base', '--is-ancestor', sha, 'HEAD'],
                          capture_output=True).returncode == 0:
            reachable.append(tag)
    return sorted(reachable, key=key, reverse=True)


def declared():
    with open(PROPS, encoding='utf-8') as fh:
        m = VERSION.search(fh.read())
    return m.group(1) if m else None


def verdict(have, tags, require_tags, remote='upstream'):
    """(exit code, message) for a declared version against the upstream tags in this history.

    Every decision of the script is made here, so the self-test can hold each one (A286, C1).
    """
    if not tags:
        message = (f'No numeric tag of "{remote}" is an ancestor of HEAD - is the remote fetched? '
                   f'(git fetch --tags {remote})')
        # Without a tag this check has nothing to compare against. Saying so and exiting 0 is right
        # for a working copy that simply has no upstream remote, and wrong wherever the check is
        # relied on: there a silent pass is indistinguishable from a clean tree. The fork's own
        # remote carries only its own +wsh tags, none of upstream's, so in CI this branch is the
        # normal outcome unless the workflow fetches upstream itself, and it must fail there rather
        # than wave the run through.
        if require_tags:
            return 1, (message + '\n\nRun with --require-tags, so this is a failure: the check could not '
                       'be performed.\nFetch the upstream tags before it (git fetch --tags upstream) or '
                       'drop the flag.')
        return 0, message

    newest = tags[0]
    head = f'{PROPS} says {have}; newest {remote} tag in this history is {newest}.\n'
    if key(have) == key(newest):
        return 0, head + 'The fork version names the upstream release it sits on.'
    if key(have) < key(newest):
        return 1, (head + f'\nBehind by at least one release. Set <Version>, <InformationalVersion> and '
                   f'<PackageVersion>\nto {newest} (keeping the +wsh / -wsh markers) - nothing does '
                   f'this automatically, because\nupstream carries no version in its source tree at all.')
    return 0, (head + '\nAhead of every upstream tag in this history. That is deliberate only if the fork '
               'has\nmoved past a release upstream has not tagged yet; otherwise the number is wrong.')


def self_test():
    assert key('7.5.6.1') > key('7.5.6.0'), 'version ordering has to be numeric, not lexical'
    assert key('7.5.10.0') > key('7.5.9.0'), 'ten must sort above nine'
    assert NUMERIC_TAG.match('7.5.6.1') and not NUMERIC_TAG.match('7.5.5.41+wsh1'), \
        'fork tags carry a marker and must not be mistaken for upstream releases'
    assert VERSION.search('<Version>1.2.3</Version>').group(1) == '1.2.3'
    # Every verdict, the gate's own failure case among them: without a tag and with the flag the
    # run has to fail, or the CI passes having checked nothing.
    assert verdict('7.5.7.2', [], True)[0] == 1, 'no tag under --require-tags must fail'
    assert verdict('7.5.7.2', [], False)[0] == 0, 'no tag without the flag stays lenient'
    assert verdict('7.5.7.1', ['7.5.7.2'], True)[0] == 1, 'a version behind the newest tag must fail'
    assert verdict('7.5.7.2', ['7.5.7.2', '7.5.7.1'], True)[0] == 0, 'the matching version must pass'
    assert verdict('7.5.7.3', ['7.5.7.2'], True)[0] == 0, 'a version ahead is reported, not failed'
    print('self-test ok: numeric ordering, fork tags excluded, property parsed, and each verdict - '
          'no tag with and without the flag, behind, equal, ahead\n')


def main(argv):
    remote = argv[argv.index('--remote') + 1] if '--remote' in argv else 'upstream'
    self_test()

    have = declared()
    if have is None:
        print(f'{PROPS} declares no <Version>. Nothing to check.')
        return 0

    code, message = verdict(have, upstream_tags(remote), '--require-tags' in argv, remote)
    print(message)
    return code


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
