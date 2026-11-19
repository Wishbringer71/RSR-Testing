#!/usr/bin/env python3
"""Which of the changed files are inputs of the CI build (concept 24, "Kosten der CI", A893).

The Build job of build.yaml compiles the plugin and the game-data generator. A push that changes none of their inputs
builds the same code as the run before, and the build is skipped. Not inputs: Markdown anywhere, the concepts under
docs/, the audit scripts and their data under .github/scripts/, the rules and hooks under .claude/ and .githooks/.
Everything else counts as an input - an unknown file builds, never the other way round.

    git diff --name-only A B | python3 .github/scripts/audit/build_inputs.py    # prints the build inputs among them
    python3 .github/scripts/audit/build_inputs.py --self-test
"""
import sys

NOT_INPUT_PREFIXES = ('docs/', '.github/scripts/', '.claude/', '.githooks/')


def is_input(path):
    path = path.strip()
    if not path:
        return False
    if path.lower().endswith('.md'):
        return False
    return not path.startswith(NOT_INPUT_PREFIXES)


def self_test():
    # Not inputs: what the compiler never reads.
    for path in ('AUDIT_LOG.md', 'TODO.md', 'docs/rotation-flow/21-crucible.md', 'docs/action-matrix/AST.csv',
                 '.github/scripts/audit/check_fork_placement.py', '.github/scripts/audit/predictions/2026-11-18.json',
                 '.claude/rules/code.md', '.githooks/pre-commit', 'RotationSolver/README.md', ''):
        assert not is_input(path), f'{path!r} counted as a build input'
    # Inputs: code, projects, the workflow itself, anything unknown - the dangerous twin of each exclusion.
    for path in ('RotationSolver/Rotations/Limited/Beastmaster/BST_Reborn.cs', 'RotationSolver/RotationSolver.csproj',
                 'Directory.Build.props', 'BannedSymbols.txt', '.github/workflows/build.yaml', 'RebornMaterial',
                 'RotationSolver/Localization/ActionId.resx', 'docsy/file.cs', 'unknown.file',
                 'RotationSolver.GameData/Program.cs', '.github/scripts.cs'):
        assert is_input(path), f'{path!r} not counted as a build input'
    print('self-test ok: Markdown, docs/, the audit scripts, rules and hooks are no build input; code, projects, '
          'the workflow, a look-alike path and an unknown file are')


def main(argv):
    if '--self-test' in argv:
        self_test()
        return 0
    for line in sys.stdin:
        if is_input(line):
            print(line.strip())
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
