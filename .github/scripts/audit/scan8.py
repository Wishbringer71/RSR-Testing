#!/usr/bin/env python3
"""Phase 8: negated-name predicates read with both polarities.

The defect this guards against was found twice in the same tree, months apart, and both times by
hand. StatusHelper.NoNeedHealingInvuln() returns *true* when no protective status is present - the
implementation asks "will the status have ended two GCDs from now", and a status that is absent has
trivially ended. So the value means "healing is due again", while the name reads as "no healing
needed". Callers split along that gap:

    StateUpdater.cs:726   if (h == 0 || !target.NoNeedHealingInvuln()) return false;   correct
    ObjectHelper.cs:125   ... && targetObject.NoNeedHealingInvuln() && ...             correct
    ActionTargetInfo.cs   if (!o.NoNeedHealingInvuln()) healingNeededObjs.Add(o);      inverted
    SCH_Reborn.cs:830     ... && !member.NoNeedHealingInvuln()                         inverted

Nothing fails when a caller gets it backwards - the rotation simply heals the wrong target - so the
error is invisible to the compiler and to any test that does not model party state.

The heuristic: a predicate whose *name* already carries a negation ("No", "Not", "Never", "Cannot",
"Without") forces every reader to resolve a double negative at each call site. Where such a predicate
is read with both polarities in the tree, at least one group of callers holds a different belief
about its meaning than the other. That is not proof of a defect - a genuinely two-sided predicate
exists - but it is the exact shape of the one found here, and it is a short list to check by hand.

Plain predicates (IsDead, HasStatus) are deliberately not reported: reading them both ways is
ordinary and would bury the signal.

Usage: python3 .github/scripts/audit/scan8.py
"""
import re
import subprocess
import sys
from collections import defaultdict

ROOTS = ('RotationSolver.Basic/', 'RotationSolver/')

# A negation already spelled into the identifier. Anchored to a word boundary inside PascalCase so
# "Normal" and "Notice" do not match on their first letters.
NEGATED_NAME = re.compile(r'(?:^|[a-z])(No|Not|Never|Cannot|Without)(?=[A-Z])')

# Declaration of a bool-returning member: method, expression-bodied property or auto-property.
DECL = re.compile(
    r'^\s*(?:public|protected|internal|private)(?:\s+(?:static|virtual|abstract|override|sealed|'
    r'partial|new|unsafe|extern|async))*\s+bool\s+(?P<name>\w+)\s*(?:\(|=>|\{)'
)

# A call site, with whatever sits immediately in front of it. Captures the "!" when present, and the
# receiver so "this.Foo()" and "x.Foo()" both resolve to Foo.
def call_re(name):
    return re.compile(r'(?P<bang>!\s*)?(?:[\w\.\?]+\.)?\b' + re.escape(name) + r'\s*\(')


def tracked_files():
    out = subprocess.run(['git', 'ls-files', '*.cs'],
                         capture_output=True, text=True, check=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(ROOTS)]


def strip_comment(raw):
    """Drop a whole-line comment. Trailing comments are left alone - a "//" inside a string literal
    would otherwise truncate real code."""
    return '' if raw.lstrip().startswith('//') else raw


def declared_predicates(files):
    """Bool members whose name spells a negation."""
    found = {}
    for path in files:
        try:
            with open(path, encoding='utf-8-sig') as fh:
                for no, raw in enumerate(fh, 1):
                    line = strip_comment(raw)
                    m = DECL.match(line)
                    if m and NEGATED_NAME.search(m.group('name')):
                        found.setdefault(m.group('name'), f'{path}:{no}')
        except OSError:
            continue
    return found


def call_sites(files, names):
    """Every call, keyed by predicate name, split by polarity."""
    patterns = {n: call_re(n) for n in names}
    sites = defaultdict(lambda: {'plain': [], 'negated': []})
    for path in files:
        try:
            with open(path, encoding='utf-8-sig') as fh:
                lines = fh.readlines()
        except OSError:
            continue
        for no, raw in enumerate(lines, 1):
            line = strip_comment(raw)
            if not line:
                continue
            for name, pat in patterns.items():
                for m in pat.finditer(line):
                    # The declaration itself is not a call site.
                    if DECL.match(line):
                        continue
                    bucket = 'negated' if m.group('bang') else 'plain'
                    sites[name][bucket].append(f'{path}:{no}  {line.strip()[:110]}')
    return sites


def self_test():
    assert NEGATED_NAME.search('NoNeedHealingInvuln')
    assert NEGATED_NAME.search('PlayerNoNeedHealingInvuln')
    assert NEGATED_NAME.search('CannotUseAction')
    # Words that merely start with the same letters must not match.
    for benign in ('Normal', 'Notice', 'Nothing', 'IsDead', 'HasStatus', 'NotifyUser'):
        assert not NEGATED_NAME.search(benign), benign

    decls = ['    public static bool NoNeedHealingInvuln(this IBattleChara p)',
             '    public bool NotReady => true;',
             '    private bool IsDead { get; }',
             '    public static int NoCount(int a)']
    got = [DECL.match(d).group('name') for d in decls if DECL.match(d)]
    assert got == ['NoNeedHealingInvuln', 'NotReady', 'IsDead'], got
    assert [n for n in got if NEGATED_NAME.search(n)] == ['NoNeedHealingInvuln', 'NotReady'], got

    pat = call_re('NoNeedHealingInvuln')
    cases = [
        ('if (!o.NoNeedHealingInvuln())', 'negated'),
        ('if (h == 0 || !target.NoNeedHealingInvuln())', 'negated'),
        ('&& targetObject.NoNeedHealingInvuln()', 'plain'),
        ('return Invulnp.WillStatusEndGCD(2, 0, false, NoNeedHealingStatus);', None),
    ]
    for line, want in cases:
        m = pat.search(line)
        got = None if not m else ('negated' if m.group('bang') else 'plain')
        assert got == want, (line, got, want)

    # A comment line must not contribute a call site - otherwise the doc block above this very
    # script's anchor case would report itself.
    assert strip_comment('    // if (!o.NoNeedHealingInvuln())') == ''

    print('self-test ok: negation detected in names, benign names excluded, polarity read\n')


def main():
    self_test()
    files = tracked_files()
    if not files:
        print('no tracked C# files found')
        return 1
    preds = declared_predicates(files)
    if not preds:
        print('no bool members with a negated name - nothing to check')
        return 0

    sites = call_sites(files, preds)
    mixed = {n: s for n, s in sites.items() if s['plain'] and s['negated']}

    print(f'{len(preds)} bool member(s) with a negated name, '
          f'{sum(1 for n in preds if sites[n]["plain"] or sites[n]["negated"])} of them called.\n')

    if not mixed:
        print('No negated-name predicate is read with both polarities.')
        return 0

    print(f'{len(mixed)} predicate(s) read with BOTH polarities - check each by hand:\n')
    for name in sorted(mixed):
        print(f'=== {name}   declared at {preds[name]}')
        print(f'    read plain ({len(mixed[name]["plain"])}x) - "the negated thing holds":')
        for s in mixed[name]['plain']:
            print(f'      {s}')
        print(f'    read negated ({len(mixed[name]["negated"])}x) - double negative:')
        for s in mixed[name]['negated']:
            print(f'      {s}')
        print()
    print('A mixed reading is not proof of a defect, but one side is likely to hold the wrong\n'
          'belief about what the predicate returns. Resolve against the implementation, not the name.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
