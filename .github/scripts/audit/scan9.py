#!/usr/bin/env python3
"""Phase 9: bool params-predicates called with no arguments.

A method declared `bool Name(params T[] xs)` is callable as `Name()`. C# then passes an empty array,
and a predicate that answers "is any of xs the case" answers `false` - always, for every game state.
The compiler is content, no test fails, and the call site reads as if it asked a real question.

The anchor case, CustomRotation_OtherInfo.HasWeaved:

    return IsLastAction() == IsLastAbility();     // false == false  =>  unconditionally true

Both are `params ActionID[]` overloads. The line was meant to ask "was the last action an ability,
i.e. has an oGCD already been weaved" - the helper for exactly that, IActionHelper.IsLastActionAbility,
was added in the very same commit (0246bea5) and was not used. HasWeaved was therefore constant true,
and its only consumer

    CanEarlyWeave => (!HasWeaved() || WeaponRemain > LateWeaveWindow) && CanWeave

collapsed to its second half: the "nothing weaved yet" condition never contributed.

The rule: a call with an empty argument list binds to the params overload only when no genuine
zero-argument overload of the same name exists. Where it does bind there, the predicate is constant
and the call site is dead weight at best and inverted logic at worst.

Names are matched across the tree without resolving receivers, so an unrelated type declaring the
same name can pull in a false positive. That is acceptable: the list is short and each entry is a
constant-valued predicate either way.

Usage: python3 .github/scripts/audit/scan9.py
"""
import re
import subprocess
import sys

ROOTS = ('RotationSolver.Basic/', 'RotationSolver/')

MODIFIERS = (r'(?:\s+(?:static|virtual|abstract|override|sealed|partial|new|unsafe|extern|async))*')

# bool Name(<params>) - the parameter list is captured so the caller can tell the overloads apart.
BOOL_DECL = re.compile(
    r'^\s*(?:public|protected|internal|private)' + MODIFIERS +
    r'\s+bool\s+(?P<name>\w+)\s*\((?P<args>[^)]*)\)'
)

# A params array is only ever the last parameter, so testing the whole list is enough.
PARAMS_ARG = re.compile(r'\bparams\s+[\w\.\<\>\?]+\[\]\s*\w+')


def call_re(name):
    """`Name()` with an empty argument list, optionally through a receiver."""
    return re.compile(r'(?P<bang>!\s*)?(?:[\w\.\?]+\.)?\b' + re.escape(name) + r'\s*\(\s*\)')


def tracked_files():
    out = subprocess.run(['git', 'ls-files', '*.cs'],
                         capture_output=True, text=True, check=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(ROOTS)]


def strip_comment(raw):
    """Drop a whole-line comment. Trailing comments are left alone - a "//" inside a string literal
    would otherwise truncate real code."""
    return '' if raw.lstrip().startswith('//') else raw


def classify_declarations(files):
    """Return (params_only, zero_arg): names declared with a params array, and names that have a
    genuine zero-argument overload. A name in both binds to the zero-argument overload and is not a
    finding."""
    params_only = {}
    zero_arg = set()
    for path in files:
        try:
            with open(path, encoding='utf-8-sig') as fh:
                for no, raw in enumerate(fh, 1):
                    line = strip_comment(raw)
                    m = BOOL_DECL.match(line)
                    if not m:
                        continue
                    name, args = m.group('name'), m.group('args').strip()
                    if not args:
                        zero_arg.add(name)
                    elif PARAMS_ARG.search(args):
                        params_only.setdefault(name, f'{path}:{no}')
        except OSError:
            continue
    return params_only, zero_arg


def empty_calls(files, names):
    """Every `Name()` call site, excluding the declarations themselves."""
    patterns = {n: call_re(n) for n in names}
    hits = []
    for path in files:
        try:
            with open(path, encoding='utf-8-sig') as fh:
                lines = fh.readlines()
        except OSError:
            continue
        for no, raw in enumerate(lines, 1):
            line = strip_comment(raw)
            if not line or BOOL_DECL.match(line):
                continue
            for name, pat in patterns.items():
                if pat.search(line):
                    hits.append((name, f'{path}:{no}', line.strip()[:110]))
    return hits


def self_test():
    decls = [
        ('	internal static bool IsLastGCD(params ActionID[] ids)', 'IsLastGCD', True, False),
        ('	public static bool IsLastActionGCD()', 'IsLastActionGCD', False, True),
        ('	internal static bool IsLastGCD(bool isAdjust, params IAction[] actions)',
         'IsLastGCD', True, False),
        ('	public static bool HasStatus(this IGameObject obj, bool isFromSelf)',
         'HasStatus', False, False),
    ]
    for line, name, is_params, is_zero in decls:
        m = BOOL_DECL.match(line)
        assert m and m.group('name') == name, line
        args = m.group('args').strip()
        assert bool(PARAMS_ARG.search(args)) == is_params, line
        assert (args == '') == is_zero, line
    # A non-bool return type is out of scope - the empty-array answer is only decidable for predicates.
    assert not BOOL_DECL.match('	public static int Count(params int[] xs)')

    pat = call_re('IsLastAbility')
    assert pat.search('		return IsLastAction() == IsLastAbility();')
    assert pat.search('		if (!x.IsLastAbility()) return false;').group('bang')
    # A call that actually passes arguments is not a finding.
    assert not pat.search('		return IsLastAbility(ActionID.FellCleavePvE);')
    # Nor is a name that merely ends with the same characters.
    assert not call_re('LastAbility').search('		return IsLastAbility();')

    assert strip_comment('	// return IsLastAction() == IsLastAbility();') == ''

    # End to end against a constructed defect: the params overload must be reported, the name that
    # also has a zero-argument overload must not.
    src = ['	internal static bool IsLastAbility(params ActionID[] ids)',
           '	public static bool IsLastActionGCD()',
           '	internal static bool IsLastActionGCD(params ActionID[] ids)']
    names, zeros = {}, set()
    for line in src:
        m = BOOL_DECL.match(line)
        args = m.group('args').strip()
        if not args:
            zeros.add(m.group('name'))
        elif PARAMS_ARG.search(args):
            names.setdefault(m.group('name'), 'x:1')
    reportable = {n for n in names if n not in zeros}
    assert reportable == {'IsLastAbility'}, reportable

    print('self-test ok: params overloads told apart from zero-argument ones, empty calls matched\n')


def main():
    self_test()
    files = tracked_files()
    if not files:
        print('no tracked C# files found')
        return 1

    params_only, zero_arg = classify_declarations(files)
    reportable = {n: loc for n, loc in params_only.items() if n not in zero_arg}
    print(f'{len(params_only)} bool member(s) declared with a params array, '
          f'{len(params_only) - len(reportable)} of them shadowed by a zero-argument overload.\n')
    if not reportable:
        print('No params-only bool predicate to check.')
        return 0

    hits = empty_calls(files, reportable)
    if not hits:
        print(f'None of the {len(reportable)} params-only predicates is called with an empty '
              f'argument list.')
        return 0

    print(f'{len(hits)} call site(s) pass an empty array to a params predicate - constant false:\n')
    for name, loc, text in hits:
        print(f'=== {name}   declared at {reportable[name]}')
        print(f'      {loc}  {text}')
    print('\nA constant-valued predicate is not what the call site reads as. Resolve each against\n'
          'the intent stated in the surrounding comment and name, not against the compiler.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
