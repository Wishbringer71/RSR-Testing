#!/usr/bin/env python3
"""Phase 5: fork behaviour changes, grouped by whether the dispatch path can be switched off.

The feature-toggle rule in CLAUDE.md says a behaviour change whose effect cannot be evidenced
belongs behind an option rather than in the default path. Whether a rotation change is reachable
without a switch is decided by which method it sits in, not by whether that method happens to read
Service.Config: the AutoStatus-driven methods are only dispatched when StateUpdater sets their flag,
and StateUpdater checks the user options there. The ungated methods run on every free slot.

Usage: python3 .github/scripts/audit/scan5.py [base-ref] [--detail]   (default: upstream/main)
"""
import re
import subprocess
import sys
import collections

BASE = 'upstream/main'

# Dispatched unconditionally on every decision pass - a change here is in the default path.
UNGATED = {
    'GeneralGCD', 'AttackAbility', 'GeneralAbility',
    'EmergencyGCD', 'EmergencyAbility',
}
# Dispatched only when StateUpdater raises the matching AutoStatus flag, which it does under the
# user's options - a change here inherits that switch.
FLAG_GATED = {
    'DefenseSingleAbility', 'DefenseAreaAbility', 'DefenseSingleGCD', 'DefenseAreaGCD',
    'HealSingleAbility', 'HealAreaAbility', 'HealSingleGCD', 'HealAreaGCD',
    'RaiseGCD', 'DispelGCD', 'MoveForwardAbility', 'MoveBackAbility',
    'AntiKnockbackAbility', 'MyInterruptAbility', 'ProvokeGCD', 'ProvokeAbility',
}

BEHAVIOUR = re.compile(
    r'\b(CanUse|return true|return act|AutoStatus|MergedStatus|'
    r'HasStatus|WillStatusEnd|DoStateCommandType)\b'
)
NOISE = re.compile(r'^\s*(//|/\*|\*|\{|\}|using |namespace |#region|#endregion|$)')
# A member declaration ends in "(" for a method, in "=>" for an expression-bodied property, and in
# "{ get" for a block one. Properties have to be recognised too: without them the backward walk runs
# past a property body into the method above it and reports a change under a name it never sat in.
SIGNATURE = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*(?:public|private|protected|internal)\s'
    r'(?:[\w<>?\[\],\s]*?\s)?(\w+)\s*(?:\(|=>|\{\s*get)'
)
TYPE_DECL = re.compile(r'\b(class|struct|interface|enum|record)\b')
NOT_A_MEMBER = {'if', 'while', 'for', 'foreach', 'switch', 'catch', 'lock', 'using', 'return'}


def changed_lines(base):
    """Return {path: set(line numbers in the working tree that the fork added)}."""
    out = subprocess.run(
        ['git', 'diff', f'{base}...HEAD', '-U0', '--', '*.cs'],
        capture_output=True, text=True, check=True).stdout
    per_file = collections.defaultdict(set)
    path = None
    for line in out.split('\n'):
        if line.startswith('+++ b/'):
            path = line[6:]
        elif line.startswith('@@') and path:
            m = re.search(r'\+(\d+)(?:,(\d+))?', line)
            if m:
                start = int(m.group(1))
                count = int(m.group(2) or 1)
                per_file[path].update(range(start, start + count))
    return per_file


def enclosing_method(lines, index):
    """Walk backwards from a line to the nearest method or property declaration."""
    for i in range(index, -1, -1):
        m = SIGNATURE.match(lines[i])
        if m and m.group(1) not in NOT_A_MEMBER and not TYPE_DECL.search(lines[i]):
            return m.group(1)
    return '(none)'


def collect(per_file):
    findings = collections.defaultdict(list)
    for path, numbers in per_file.items():
        try:
            lines = open(path, encoding='utf-8').read().split('\n')
        except OSError:
            continue  # deleted by the fork
        for n in sorted(numbers):
            if n - 1 >= len(lines):
                continue
            body = lines[n - 1]
            if NOISE.match(body) or not BEHAVIOUR.search(body):
                continue
            method = enclosing_method(lines, n - 1)
            bucket = 'ungated' if method in UNGATED else (
                'flag-gated' if method in FLAG_GATED else 'other')
            findings[bucket].append((path, method, n, body.strip()))
    return findings


def self_test():
    lines = [
        'public sealed class Fake : Rotation',
        '{',
        '    protected override bool GeneralGCD(out IAction? act)',
        '    {',
        '        if (FakePvE.CanUse(out act)) return true;',
        '    }',
        '    protected override bool DefenseAreaAbility(out IAction? act)',
        '    {',
        '        if (OtherPvE.CanUse(out act)) return true;',
        '    }',
        '    private static float Helper(float x)',
        '    {',
        '        return x;',
        '    }',
        '    private bool Contested =>',
        '        OtherPvE.CanUse(out _);',
        '    public int Threshold { get; set; } = 2000;',
    ]
    assert enclosing_method(lines, 4) == 'GeneralGCD', enclosing_method(lines, 4)
    assert enclosing_method(lines, 8) == 'DefenseAreaAbility', enclosing_method(lines, 8)
    assert enclosing_method(lines, 12) == 'Helper', enclosing_method(lines, 12)
    # An expression-bodied property is its own scope; before this was recognised, its body was
    # attributed to Helper above it, which put fork changes in methods they never sat in.
    assert enclosing_method(lines, 15) == 'Contested', enclosing_method(lines, 15)
    assert enclosing_method(lines, 16) == 'Threshold', enclosing_method(lines, 16)
    # The class declaration is not a member and must not swallow the walk.
    assert enclosing_method(lines, 1) == '(none)', enclosing_method(lines, 1)
    assert NOISE.match('   // CanUse in a comment')
    assert BEHAVIOUR.search('if (FakePvE.CanUse(out act))')
    print('self-test ok: methods, properties and type declarations distinguished\n')


if __name__ == '__main__':
    self_test()
    args = [a for a in sys.argv[1:] if a != '--detail']
    detail = '--detail' in sys.argv[1:]
    base = args[0] if args else BASE
    try:
        per_file = changed_lines(base)
    except subprocess.CalledProcessError:
        print(f'cannot diff against {base}; fetch the upstream remote first')
        sys.exit(1)

    findings = collect(per_file)
    for bucket, title in (
            ('ungated', 'in the always-dispatched methods - no switch of their own'),
            ('flag-gated', 'in AutoStatus-dispatched methods - inherit StateUpdater\'s options'),
            ('other', 'in helpers, updaters and infrastructure - judge individually')):
        rows = findings.get(bucket, [])
        print(f'== {bucket}: {len(rows)} changed behaviour lines {title}')
        if detail:
            for path, method, n, body in sorted(rows):
                print(f'  {path}:{n}  {method}: {body}')
        else:
            seen = collections.Counter()
            for path, method, n, body in rows:
                seen[(path, method)] += 1
            for (path, method), count in sorted(seen.items()):
                print(f'  {path}::{method}  ({count})')
        print()
