#!/usr/bin/env python3
"""Fail if an override calls a *different* base method than the one it overrides.

`return base.DefenseAreaGCD(out act);` inside `override bool DefenseSingleGCD`
compiles, runs, and is invisible in a diff - it just silently continues the
dispatch chain in the wrong place. Nine such cases were found by hand in this
repository (see AUDIT_LOG.md); this check makes the tenth impossible.

Usage:  check_base_calls.py [root ...]
Exit:   0 = clean, 1 = mismatch found, 2 = nothing scanned (parser broken)

Intentional exceptions go in ALLOWLIST below, with a reason. An empty
allowlist is the desired state.
"""
import os
import re
import sys

# (file suffix, overriding method, called base method): reason
ALLOWLIST: dict[tuple[str, str, str], str] = {}

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


def main(roots: list[str]) -> int:
    findings: list[str] = []
    methods = 0
    base_calls = 0

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
                        findings.append(
                            f'{path}:{line}: override {method} calls base.{called}')

    # A parser that silently matches nothing would report a clean run forever.
    if methods == 0 or base_calls == 0:
        print(f'check_base_calls: parsed {methods} overrides and {base_calls} '
              f'base calls - the parser is broken, not the code.', file=sys.stderr)
        return 2

    if findings:
        print(f'check_base_calls: {len(findings)} mismatch(es):', file=sys.stderr)
        for finding in findings:
            print(f'  {finding}', file=sys.stderr)
        print('\nAn override must continue the dispatch chain through its own '
              'base method. If a cross-call is deliberate, add it to ALLOWLIST '
              'with a reason.', file=sys.stderr)
        return 1

    print(f'check_base_calls: clean - {methods} overrides, {base_calls} base calls.')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:] or ['RotationSolver', 'RotationSolver.Basic']))
