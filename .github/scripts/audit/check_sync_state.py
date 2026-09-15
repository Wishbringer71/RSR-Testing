#!/usr/bin/env python3
"""Am I actually synced, and is what I am measuring against still alive?

Run this before changing code, and again before making any statement about branch, tag or sync
state. It exists because that statement was made wrong: upstream was reported as twenty-one commits
ahead when it was two. The measurement had used the local `main`, which had been sitting at an
August commit while main itself moved forward through pull request merges on GitHub - a local branch
follows nothing on its own. Git had been saying so all along, in `git branch -vv`: "behind 382".

The working clone here is long-lived, not a fresh checkout per session, so every local ref is a
claim about the past rather than about the remote. This script refuses to take any of them at face
value:

- It fetches both remotes with --prune first, because a measurement against unfetched refs measures
  the last fetch, not the remote.
- It measures HEAD - what is actually checked out - against upstream/main, never a named local
  branch. That single substitution is what produced the wrong number.
- It reports every local branch that is behind its remote, and every one whose remote is gone.

Exit code 1 when HEAD is behind upstream/main, because that is the precondition for changing code.
Stale and orphaned branches are printed but do not fail the run: deleting them needs approval, so
the script's job is to surface them, not to act.

Not part of CI. In CI the clone is fresh and every ref is current, so there would be nothing to
find; the failure this guards against belongs to a persistent working copy.

Usage: python3 .github/scripts/audit/check_sync_state.py [--no-fetch]
"""
import subprocess
import sys


def git(*args, check=True):
    result = subprocess.run(('git',) + args, capture_output=True, text=True)
    if check and result.returncode != 0:
        raise RuntimeError('git %s failed: %s' % (' '.join(args), result.stderr.strip()))
    return result.stdout.strip()


def counts(left, right):
    """Commits in `left` not in `right`, and the other way round."""
    out = git('rev-list', '--left-right', '--count', '%s...%s' % (left, right))
    a, b = out.split()
    return int(a), int(b)


def remotes():
    return set(git('remote').split())


def self_test():
    """Construct the exact defect in a throwaway repository: a local branch left behind."""
    import os
    import tempfile

    original = os.getcwd()
    workspace = tempfile.mkdtemp()
    try:
        origin = os.path.join(workspace, 'origin')
        clone = os.path.join(workspace, 'clone')
        os.makedirs(origin)
        os.chdir(origin)
        git('init', '-q', '-b', 'main')
        git('config', 'user.email', 't@t')
        git('config', 'user.name', 't')
        open('f', 'w').write('1')
        git('add', 'f')
        git('commit', '-qm', 'one')
        os.chdir(workspace)
        git('clone', '-q', origin, clone)
        # origin moves on; the clone's local main does not.
        os.chdir(origin)
        open('f', 'w').write('2')
        git('commit', '-qam', 'two')
        os.chdir(clone)
        git('config', 'user.email', 't@t')
        git('config', 'user.name', 't')
        git('fetch', '-q', 'origin')

        behind_local, ahead_local = counts('origin/main', 'main')
        if behind_local != 1:
            raise AssertionError('the constructed lag was not seen: %d' % behind_local)
        if ahead_local != 0:
            raise AssertionError('local main should carry nothing of its own')

        # Second case: a shallow clone has to be recognised as one, because its counts are worthless.
        shallow = os.path.join(workspace, 'shallow')
        os.chdir(workspace)
        git('clone', '-q', '--depth', '1', 'file://' + origin, shallow)
        os.chdir(shallow)
        if git('rev-parse', '--is-shallow-repository') != 'true':
            raise AssertionError('a --depth 1 clone has to report as shallow')

        print('self-test ok: a local branch left behind by its remote is detected (behind 1), '
              'and a shallow clone is recognised\n')
    finally:
        os.chdir(original)
        subprocess.run(['rm', '-rf', workspace], capture_output=True)


def main():
    self_test()

    do_fetch = '--no-fetch' not in sys.argv
    present = remotes()

    if do_fetch:
        for remote in ('origin', 'upstream'):
            if remote in present:
                print('fetching %s ...' % remote)
                git('fetch', '--prune', '--tags', remote, check=False)
        print()

    head = git('rev-parse', '--abbrev-ref', 'HEAD')
    print('checked out: %s (%s)' % (head, git('rev-parse', '--short', 'HEAD')))

    failed = False

    # A shallow clone cannot answer this question at all: rev-list walks history that is not there,
    # and the count it returns is not wrong-looking, it is simply wrong. Found the hard way - a
    # freshly provisioned working copy reported "0 behind, 123 ahead" where the real figures were
    # "0 behind, 390 ahead". The first number happened to be right, which is the dangerous kind of
    # silent null result: it reads exactly like a clean measurement.
    if git('rev-parse', '--is-shallow-repository') == 'true':
        print('\nThis is a SHALLOW clone. Every count below is unreliable, because the history the\n'
              'comparison needs is not present. Run `git fetch --unshallow origin` (and fetch\n'
              'upstream afterwards) before making any statement about sync state.')
        return 1

    if 'upstream' in present:
        behind, ahead = counts('upstream/main', 'HEAD')
        print('HEAD vs upstream/main: %d behind, %d ahead' % (behind, ahead))
        if behind:
            print('  -> not synced. Merge upstream/main before changing code.')
            failed = True
    else:
        print('no upstream remote configured')

    print('\nlocal branches measured against their remote:')
    for line in git('for-each-ref', '--format=%(refname:short)|%(upstream:short)|%(upstream:track)',
                    'refs/heads').split('\n'):
        if not line:
            continue
        name, tracked, track = (line.split('|') + ['', ''])[:3]
        if not tracked:
            print('  %-34s no remote-tracking branch' % name)
        elif 'gone' in track:
            print('  %-34s remote is GONE - deletion candidate, needs approval' % name)
        elif 'behind' in track:
            print('  %-34s %s - stale, do not measure against it' % (name, track))
        elif track:
            print('  %-34s %s' % (name, track))
        else:
            print('  %-34s up to date' % name)

    if failed:
        print('\nHEAD is behind upstream. Syncing is the precondition for changing code.')
        return 1

    print('\nHEAD is up to date with upstream/main.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
