#!/usr/bin/env python3
"""Two structural checks that the compiler cannot make, both drawn from bugs
this repository actually had (see AUDIT_LOG.md).

1. Base call target
   `return base.DefenseAreaGCD(out act);` inside `override bool
   DefenseSingleGCD` compiles, runs, and is invisible in a diff - it silently
   continues the dispatch chain in the wrong place. Nine such cases were found
   by hand here.

2. Contradictory level predicate
   Upgrade chains are written by hand as `!Higher.EnoughLevel && Lower.CanUse`.
   Naming the same action on both sides - `!X.EnoughLevel && X.CanUse` - is a
   condition that can never be true, so the branch is dead. That is exactly the
   RDM Impact bug. The chains themselves are too varied to replace mechanically
   (the gate action is often a different action than the one being cast), so
   the error class is closed off here instead of refactored away.

Usage:  check_base_calls.py [root ...]
Exit:   0 = clean, 1 = finding, 2 = nothing scanned (parser broken)

Intentional exceptions go in the ALLOWLIST constants below, with a reason. An
empty allowlist is the desired state.
"""
import os
import re
import sys

# (file suffix, overriding method, called base method): reason
ALLOWLIST: dict[tuple[str, str, str], str] = {}

# (file suffix, action name): reason
LEVEL_ALLOWLIST: dict[tuple[str, str], str] = {}

_LINE_COMMENT = re.compile(r'//[^\n]*')
_BLOCK_COMMENT = re.compile(r'/\*.*?\*/', re.S)
_STRING = re.compile(r'"(?:\\.|[^"\\])*"')
_SIGNATURE = re.compile(
    r'(?:public|protected|private|internal)[\w\s]*\boverride\s+(?:bool|IAction\?)\s+(\w+)\s*\(')
_BASE_CALL = re.compile(r'\bbase\.(\w+)\s*\(')


def strip_noise(src: str) -> str:
    """Remove comments and string literals so braces inside them cannot
    confuse the brace matching below."""
    src = _BLOCK_COMMENT.sub('', src)
    src = _LINE_COMMENT.sub('', src)
    return _STRING.sub('""', src)


def iter_methods(src: str):
    """Yield (name, body, line) for every overriding bool/IAction? method."""
    for match in _SIGNATURE.finditer(src):
        start = src.find('{', match.end())
        if start < 0:
            continue
        depth, end = 0, start
        while end < len(src):
            if src[end] == '{':
                depth += 1
            elif src[end] == '}':
                depth -= 1
                if depth == 0:
                    break
            end += 1
        yield match.group(1), src[start:end], src[:match.start()].count('\n') + 1


_CONDITION = re.compile(r'\bif\s*\(', re.M)
_LEVEL_TERM = re.compile(r'(!?)\s*(\w+PvE)\.(?:EnoughLevel\b|Info\.EnoughLevelAndQuest\(\))')


def iter_conditions(src: str):
    """Yield (text, line) for every if-condition, including multi-line ones."""
    for match in _CONDITION.finditer(src):
        start = match.end() - 1
        depth, end = 0, start
        while end < len(src):
            if src[end] == '(':
                depth += 1
            elif src[end] == ')':
                depth -= 1
                if depth == 0:
                    break
            end += 1
        yield src[start:end + 1], src[:match.start()].count('\n') + 1


def contradictory_levels(condition: str) -> set[str]:
    """Actions whose level is required to be both insufficient and sufficient.

    `!X.EnoughLevel && X.CanUse(...)` can never be true: CanUse fails on
    insufficient level itself, so the branch is unreachable.

    Conditions containing `||` are skipped. The common and *correct* idiom in
    this repository is an explicit level bracket -
    `(X.EnoughLevel && ...) || !X.EnoughLevel` - in which both terms appear but
    sit in different branches. Judging those would need a real expression
    parser; without one, every single occurrence would be a false alarm (16 of
    16 when this was first tried). A quiet correct check is worth more than a
    loud wrong one, because the loud one gets switched off.
    """
    if '||' in condition:
        return set()

    negated, plain = set(), set()
    for sign, action in _LEVEL_TERM.findall(condition):
        (negated if sign == '!' else plain).add(action)
    used_positively = {a for a in negated
                       if re.search(r'\b' + a + r'\.CanUse\b', condition)}
    return (negated & plain) | used_positively


def main(roots: list[str]) -> int:
    base_findings: list[str] = []
    level_findings: list[str] = []
    methods = 0
    base_calls = 0
    conditions = 0

    for root in roots:
        for dirpath, _, names in os.walk(root):
            for name in names:
                if not name.endswith('.cs'):
                    continue
                path = os.path.join(dirpath, name)
                with open(path, encoding='utf-8') as handle:
                    src = strip_noise(handle.read())

                for method, body, line in iter_methods(src):
                    methods += 1
                    for call in _BASE_CALL.finditer(body):
                        base_calls += 1
                        called = call.group(1)
                        if called == method:
                            continue
                        if any(path.endswith(suffix) and a == method and b == called
                               for (suffix, a, b) in ALLOWLIST):
                            continue
                        base_findings.append(
                            f'{path}:{line}: override {method} calls base.{called}')

                for condition, line in iter_conditions(src):
                    conditions += 1
                    for action in contradictory_levels(condition):
                        if any(path.endswith(suffix) and a == action
                               for (suffix, a) in LEVEL_ALLOWLIST):
                            continue
                        level_findings.append(
                            f'{path}:{line}: {action} required to be both '
                            f'below and at level in one condition')

    # A parser that silently matches nothing would report a clean run forever.
    if methods == 0 or base_calls == 0 or conditions == 0:
        print(f'check_base_calls: parsed {methods} overrides, {base_calls} base '
              f'calls, {conditions} conditions - the parser is broken, not the '
              f'code.', file=sys.stderr)
        return 2

    if base_findings:
        print(f'check_base_calls: {len(base_findings)} base call mismatch(es):',
              file=sys.stderr)
        for finding in base_findings:
            print(f'  {finding}', file=sys.stderr)
        print('\nAn override must continue the dispatch chain through its own '
              'base method. If a cross-call is deliberate, add it to ALLOWLIST '
              'with a reason.', file=sys.stderr)

    if level_findings:
        print(f'check_base_calls: {len(level_findings)} contradictory level '
              f'predicate(s):', file=sys.stderr)
        for finding in level_findings:
            print(f'  {finding}', file=sys.stderr)
        print('\nIn an upgrade chain the gate names the *higher* action and the '
              'call names the *lower* one. Naming the same action on both sides '
              'makes the branch unreachable.', file=sys.stderr)

    if base_findings or level_findings:
        return 1

    print(f'check_base_calls: clean - {methods} overrides, {base_calls} base '
          f'calls, {conditions} conditions.')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:] or ['RotationSolver', 'RotationSolver.Basic']))
