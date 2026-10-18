"""Run every Python step of the build workflow locally, in the order the workflow runs them.

The workflow is the list of record: a local run over check_*.py alone misses the scan*.py and
model scripts the workflow also runs (A264: scan18.py failed on main after a local run that
never called it). Reading the steps from build.yaml keeps this runner from ageing behind it.

Steps that need the upstream remote (fetched by a shell step in the workflow) expect it to be
present locally as `upstream`.

The models (the workflow's MODELS line) run as the workflow runs them: only when their own script
changed - here against the branch's upstream, so uncommitted and unpushed edits count - and all of
them with --models. They read no plugin code, so a run without a change to them says nothing new
(A323).
"""
import pathlib
import re
import shlex
import subprocess
import sys

WORKFLOW = pathlib.Path('.github/workflows/build.yaml')
STEP = re.compile(r'^\s*run:\s*(python3\s+\S+\.py(?:\s+[^#\n]*)?)\s*$')
MODELS = re.compile(r'^\s*MODELS="([^"]+)"\s*$', re.M)
AUDIT = '.github/scripts/audit/'


def steps(text):
    return [m.group(1).strip() for m in map(STEP.match, text.splitlines()) if m]


def models(text):
    match = MODELS.search(text)
    return match.group(1).split() if match else []


def changed_models(names):
    """The models whose script differs from the branch's upstream (or from HEAD without one)."""
    paths = [f'{AUDIT}{name}.py' for name in names]
    base = subprocess.run(['git', 'rev-parse', '--abbrev-ref', '@{upstream}'], capture_output=True, text=True)
    against = base.stdout.strip() if base.returncode == 0 else 'HEAD'
    diff = subprocess.run(['git', 'diff', '--name-only', against, '--', *paths], capture_output=True, text=True)
    touched = set(diff.stdout.split())
    return [name for name in names if f'{AUDIT}{name}.py' in touched]


def self_test():
    sample = (
        "      - name: A\n        run: python3 .github/scripts/audit/check_a.py\n"
        "      - name: B\n        run: python3 .github/scripts/audit/check_b.py --check\n"
        "      - name: C\n        run: |\n          python3 not_a_single_line_step.py\n"
        "      - name: D\n        run: dotnet build\n"
    )
    found = steps(sample)
    assert found == ['python3 .github/scripts/audit/check_a.py',
                     'python3 .github/scripts/audit/check_b.py --check'], found
    assert models('        run: |\n          MODELS="m_one m_two"\n') == ['m_one', 'm_two'], 'the models line is read'
    assert models("no such line") == [], "without a models line there are none"
    print('self-test ok: single-line Python steps are read with their arguments; '
          'block scripts and other tools are left out')


def main():
    self_test()
    if not WORKFLOW.exists():
        print(f'{WORKFLOW} not found - run from the repository root')
        return 2

    text = WORKFLOW.read_text(encoding='utf-8')
    found = steps(text)
    named = models(text)
    chosen = named if '--models' in sys.argv[1:] else changed_models(named)
    found += [f'python3 {AUDIT}{name}.py' for name in chosen]
    skipped = [name for name in named if name not in chosen]
    if skipped:
        print('models unchanged, not run (--models runs them):', ', '.join(skipped))
    missing = [s for s in found if not pathlib.Path(shlex.split(s)[1]).exists()]
    if not found or missing:
        print('workflow steps unreadable or pointing at missing scripts:', missing or 'none found')
        return 2

    failed = []
    for command in found:
        print(f'-- {command}', flush=True)
        result = subprocess.run(shlex.split(command), capture_output=True, text=True)
        if result.returncode != 0:
            failed.append(command)
            print(f'== FAILED: {command} (exit {result.returncode})')
            print((result.stdout + result.stderr).strip()[-2000:])

    print(f'{len(found)} workflow step(s) run, {len(failed)} failed.')
    return 1 if failed else 0


if __name__ == '__main__':
    sys.exit(main())
