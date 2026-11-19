#!/usr/bin/env python3
"""Pre-push gate: the CI's checks and build in the session before a push, not on GitHub (concept 24, "Kosten der CI",
A895).

His question of 19.11.2026: whether the checks could run in Claude Code's own environment instead of on GitHub, whose
CI quota was nearly spent. They already ran here before every commit (run_ci_checks.py, the Linux build); the CI on
each push repeated them. This gate makes the local run binding, and build.yaml runs on GitHub only when a pull request
is opened, reopened or marked ready, and on request (before a release).

What the CI had that a local run lacks is a clean checkout of exactly the pushed commit. The gate therefore refuses
unless every pushed commit is the checked-out HEAD and the working tree is clean - tracked, staged and untracked alike -
so the tree it checks is the commit it lets through. Then: every check (run_ci_checks.py; on main without
check_release_note, whose text lives in the working branch until it merges), the plugin build with the CI's compiler
switches (cloud_build.sh), latest.zip, and the game-data generator when the push changes it.

Only in a tree that has run_ci_checks.py: check_commit_identity.py pushes through the same hook from constructed
repositories, where nothing here applies.

    git push ... (from .githooks/pre-push, with git's lines on stdin)
    python3 .github/scripts/audit/pre_push_gate.py --self-test
"""
import pathlib
import subprocess
import sys

ZERO = '0' * 40


def pushes(lines):
    """(local ref, local sha, remote ref, remote sha) for each pushed ref, deletions left out."""
    found = []
    for line in lines:
        parts = line.split()
        if len(parts) == 4 and parts[1] != ZERO:
            found.append(tuple(parts))
    return found


def refusal(found, head, dirty):
    """Why the push is refused before any check runs, or None."""
    for _, local_sha, remote_ref, _ in found:
        if local_sha != head:
            return (f'{remote_ref} would get {local_sha[:9]}, but HEAD is {head[:9]}: the gate checks the working tree, so '
                    'push the checked-out commit')
    if dirty:
        return f'the working tree is not clean ({dirty[0]} and {len(dirty) - 1} more) - commit or stash first'
    return None


def touches_generator(found, run):
    """Whether a pushed range changes RotationSolver.GameData (a new branch counts as changing it)."""
    for _, local_sha, _, remote_sha in found:
        if remote_sha == ZERO:
            return True
        out = run(['git', 'diff', '--name-only', remote_sha, local_sha, '--', 'RotationSolver.GameData'])
        if out.returncode != 0 or out.stdout.strip():
            return True
    return False


def git(args):
    return subprocess.run(args, capture_output=True, text=True)


def self_test():
    head = 'a' * 40
    lines = [f'refs/heads/b {head} refs/heads/b {"b" * 40}', f'(delete) {ZERO} refs/heads/old {"c" * 40}', 'garbage']
    found = pushes(lines)
    assert found == [('refs/heads/b', head, 'refs/heads/b', 'b' * 40)], found
    assert refusal(found, head, []) is None, 'HEAD and a clean tree pass'
    assert 'HEAD is' in refusal(found, 'd' * 40, []), 'another commit than HEAD is refused'
    assert 'not clean' in refusal(found, head, ['?? new.cs']), 'an untracked file is refused'
    assert 'not clean' in refusal(found, head, [' M a.cs', 'M  b.cs']), 'a modified file is refused'
    assert refusal([], head, ['?? x']) is not None, 'even a push of deletions alone needs a clean tree'

    class Result:
        def __init__(self, out, code=0):
            self.stdout, self.returncode = out, code

    assert touches_generator([('r', head, 'r', ZERO)], lambda a: Result('')), 'a new branch builds the generator'
    assert touches_generator(found, lambda a: Result('RotationSolver.GameData/Program.cs\n'))
    assert not touches_generator(found, lambda a: Result('')), 'no change, no generator build'
    assert touches_generator(found, lambda a: Result('', 128)), 'an unknown range builds it'
    print('self-test ok: a push of HEAD from a clean tree passes; another commit, a modified, a staged or an '
          'untracked file are refused; the generator builds when its files change, on a new branch or an unknown range')


def main(argv):
    if '--self-test' in argv:
        self_test()
        return 0

    top = git(['git', 'rev-parse', '--show-toplevel'])
    root = pathlib.Path(top.stdout.strip()) if top.returncode == 0 else None
    if root is None or not (root / '.github/scripts/audit/run_ci_checks.py').exists():
        return 0

    found = pushes(sys.stdin.read().splitlines())
    head = git(['git', 'rev-parse', 'HEAD']).stdout.strip()
    dirty = [line for line in git(['git', 'status', '--porcelain']).stdout.splitlines() if line]
    why = refusal(found, head, dirty)
    if why:
        print(f'pre-push gate: refusing - {why}', file=sys.stderr)
        return 1
    if not found:
        return 0

    steps = [[sys.executable, '.github/scripts/audit/run_ci_checks.py']]
    if any(remote_ref == 'refs/heads/main' for _, _, remote_ref, _ in found):
        steps[0] += ['--skip', 'check_release_note']
    steps.append(['bash', '.github/scripts/cloud_build.sh'])
    steps.append([sys.executable, '.github/scripts/audit/check_plugin_zip.py', 'bin/Release/RotationSolver/latest.zip'])
    if touches_generator(found, git):
        steps.append(['bash', '.github/scripts/cloud_build.sh', '--generator'])
    for step in steps:
        print(f'pre-push gate: {" ".join(step[1:] if step[0] == sys.executable else step)}', file=sys.stderr, flush=True)
        if subprocess.run(step, cwd=root).returncode != 0:
            print(f'pre-push gate: refusing - {" ".join(step)} failed', file=sys.stderr)
            return 1
    print('pre-push gate: checks and build passed', file=sys.stderr)
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
