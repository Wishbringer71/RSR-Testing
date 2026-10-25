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
Stale and orphaned branches are printed but do not fail the run; the script surfaces them and does not
act. A branch whose remote is gone and that holds no commit missing from every remote branch is a
remnant of the session's environment: CLAUDE.md ("Grenzen und Löschungen") has it deleted with
`git branch -d` and reported, without approval. One with a commit found only here needs approval -
before A368 the script said so of both, and two merged branches were held for an approval nobody needed.

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


def measure(do_fetch=True):
    """Measure the working copy in the current directory and say so; 1 when no change may be made.

    Every verdict this script gives is made here, so the self-test can hold each one against a
    constructed repository instead of testing git's own counting.
    """
    present = remotes()

    fetch_failed = []
    if do_fetch:
        for remote in ('origin', 'upstream'):
            if remote in present:
                print('fetching %s ...' % remote)
                result = subprocess.run(('git', 'fetch', '--prune', '--tags', remote),
                                        capture_output=True, text=True)
                # A fetch that fails leaves the refs of the last one, and every count below would
                # describe that older state while reading exactly like a current one - the error this
                # script exists to prevent. Say so and refuse the verdict.
                if result.returncode != 0:
                    fetch_failed.append(remote)
                    print('  FETCH FAILED for %s: %s' % (remote, (result.stderr.strip().splitlines() or [''])[-1]))
        print()

    head = git('rev-parse', '--abbrev-ref', 'HEAD')
    print('checked out: %s (%s)' % (head, git('rev-parse', '--short', 'HEAD')))

    failed = bool(fetch_failed)

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
            only_here = int(git('rev-list', '--count', name, '--not', '--remotes') or 0)
            if only_here:
                print('  %-34s remote is GONE, %d commit(s) only here - deletion needs approval' % (name, only_here))
            else:
                print('  %-34s remote is GONE, nothing only here - session remnant: git branch -d, then report it' % name)
        elif 'behind' in track:
            print('  %-34s %s - stale, do not measure against it' % (name, track))
        elif track:
            print('  %-34s %s' % (name, track))
        else:
            print('  %-34s up to date' % name)

    if fetch_failed:
        print('\nThe fetch of %s failed, so the figures above are those of the last fetch. No statement '
              'about sync state.' % ' and '.join(fetch_failed))
        return 1

    if failed:
        print('\nHEAD is behind upstream. Syncing is the precondition for changing code.')
        return 1

    print('\nHEAD is up to date with upstream/main.')
    return 0


def self_test():
    """Hold every verdict of measure() against a constructed repository.

    The earlier self-test asked git whether it counts and whether it knows a shallow clone, which
    git always does; a broken verdict in this script would have passed it (A286, C2).
    """
    import contextlib
    import io
    import os
    import tempfile

    def run(*args):
        subprocess.run(('git',) + args, capture_output=True, text=True, check=True)

    def verdict(do_fetch=True):
        buffer = io.StringIO()
        with contextlib.redirect_stdout(buffer):
            code = measure(do_fetch)
        return code, buffer.getvalue()

    original = os.getcwd()
    workspace = tempfile.mkdtemp()
    try:
        upstream = os.path.join(workspace, 'upstream')
        clone = os.path.join(workspace, 'clone')
        os.makedirs(upstream)
        os.chdir(upstream)
        run('init', '-q', '-b', 'main')
        run('config', 'user.email', 't@t')
        run('config', 'user.name', 't')
        open('f', 'w').write('1')
        run('add', 'f')
        run('commit', '-qm', 'one')
        os.chdir(workspace)
        run('clone', '-q', '-o', 'upstream', upstream, clone)
        os.chdir(clone)
        run('config', 'user.email', 't@t')
        run('config', 'user.name', 't')

        code, text = verdict()
        if code != 0 or '0 behind' not in text:
            raise AssertionError('a synced copy was not reported synced:\n' + text)

        # Upstream moves on: the copy is behind and may not change code.
        os.chdir(upstream)
        open('f', 'w').write('2')
        run('commit', '-qam', 'two')
        os.chdir(clone)
        code, text = verdict()
        if code != 1 or '1 behind' not in text:
            raise AssertionError('a copy one commit behind was not refused:\n' + text)

        # A local branch whose remote is gone is named as a deletion candidate.
        run('merge', '-q', '--ff-only', 'upstream/main')
        run('checkout', '-q', '-b', 'topic')
        run('push', '-q', 'upstream', 'topic')
        run('branch', '-q', '--set-upstream-to', 'upstream/topic')
        os.chdir(upstream)
        run('branch', '-q', '-D', 'topic')
        os.chdir(clone)
        code, text = verdict()
        if 'GONE, nothing only here' not in text:
            raise AssertionError('a merged branch whose remote is gone was not named a session remnant:\n' + text)

        # The same with a commit that no remote holds: that one needs approval.
        open('g', 'w').write('x')
        run('add', 'g')
        run('commit', '-qm', 'local only')
        code, text = verdict()
        if '1 commit(s) only here - deletion needs approval' not in text:
            raise AssertionError('a branch with a commit found only here was not held for approval:\n' + text)
        run('checkout', '-q', 'main')

        # The fetch fails: the old refs must not pass as current (C3).
        run('remote', 'set-url', 'upstream', os.path.join(workspace, 'nowhere'))
        code, text = verdict()
        if code != 1 or 'FETCH FAILED' not in text:
            raise AssertionError('a failed fetch was taken as a current measurement:\n' + text)

        # A shallow clone gives no verdict at all.
        shallow = os.path.join(workspace, 'shallow')
        os.chdir(workspace)
        run('clone', '-q', '--depth', '1', 'file://' + upstream, shallow)
        os.chdir(shallow)
        code, text = verdict(do_fetch=False)
        if code != 1 or 'SHALLOW' not in text:
            raise AssertionError('a shallow clone was measured:\n' + text)

        print('self-test ok: synced, behind, a gone remote with and without a commit only here, a failed '
              'fetch and a shallow clone each get their verdict\n')
    finally:
        os.chdir(original)
        subprocess.run(['rm', '-rf', workspace], capture_output=True)


def main():
    self_test()
    return measure('--no-fetch' not in sys.argv)


if __name__ == '__main__':
    sys.exit(main())
