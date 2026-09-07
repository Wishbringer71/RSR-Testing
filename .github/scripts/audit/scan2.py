#!/usr/bin/env python3
"""Phase 2 scans: defect classes derived from the findings this repo has actually produced.

The nine checks live in scan_source() so the self-test can drive them against constructed defects.
Before that split they ran inline in the file loop and could not be exercised at all - a pattern that
stopped matching would have reported a clean tree, which is the failure mode scan3.py through
scan8.py already guard against.
"""
import os, re, sys, collections

ROOTS = ['RotationSolver', 'RotationSolver.Basic']

def strip(src):
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    src = re.sub(r'//[^\n]*', '', src)
    return re.sub(r'"(?:\\.|[^"\\])*"', '""', src)

def methods(src):
    """Yield (name, body, start_line) for every method with a block body."""
    sig = re.compile(r'(?:public|protected|private|internal)[\w\s]*?\b(\w+)\s*\([^;{}]*\)\s*\{')
    for m in sig.finditer(src):
        i = src.find('{', m.end() - 1)
        d, j = 0, i
        while j < len(src):
            if src[j] == '{': d += 1
            elif src[j] == '}':
                d -= 1
                if d == 0:
                    break
            j += 1
        yield m.group(1), src[i:j], src[:m.start()].count('\n') + 1

# ratio (0..1) vs percent (0..100) mix-ups
PCT = re.compile(r'GetEffectiveHpPercent\(\)\s*(<|>|<=|>=)\s*([A-Za-z_]\w*)')
RATIO = re.compile(r'GetHealthRatio\(\)\s*(<|>|<=|>=)\s*(\d+(?:\.\d+)?)\s*(?![f\d.])')


def scan_source(path, src, R):
    """Run every check against one already-stripped source. Appends to R in place."""
    lines = src.split('\n')

    for i, line in enumerate(lines, 1):
        # (a) percent scale compared against something that looks like a 0..1 config
        for m in PCT.finditer(line):
            R['a_percent_vs_ratio'].append(f'{path}:{i}: {m.group(0)}')
        # (b) ratio compared against an integer literal > 1 (would be a percent value)
        for m in RATIO.finditer(line):
            if float(m.group(2)) > 1:
                R['b_ratio_vs_percent'].append(f'{path}:{i}: {m.group(0)}')
        # (c) float equality
        for m in re.finditer(r'\b(\w*(?:Ratio|Time|Remain|Range|Distance|Hp|Mp|Gauge)\w*)\s*(==|!=)\s*(-?\d+(?:\.\d+)?f?)\b', line):
            if m.group(3).rstrip('f') not in ('0', '1'):
                R['c_float_equality'].append(f'{path}:{i}: {m.group(0)}')
        # (d) status source flag: hostile debuff queried as if it were from anyone
        for m in re.finditer(r'HostileTarget\??\.HasStatus\(\s*true\s*,', line):
            R['d_hostile_status_fromself'].append(f'{path}:{i}: {line.strip()[:110]}')

    for name, body, start in methods(src):
        blines = body.split('\n')

        # (e) usedUp: true with no condition anywhere in the enclosing if-chain of that line
        for k, bl in enumerate(blines):
            if 'usedUp: true' not in bl:
                continue
            # the statement's own condition: everything before the CanUse call on this line
            stmt = bl.strip()
            head = stmt.split('.CanUse(')[0]
            bare = re.match(r'^(?:if\s*\()?\s*!?[A-Za-z_]\w*(?:PvE|PvP)$', head.strip())
            if bare and 'return' not in stmt:
                R['e_usedup_unconditional'].append(f'{path}:{start + k}: [{name}] {stmt[:110]}')

        # (f) skipStatusProvideCheck: true without a refresh/sustain gate on the same statement
        for k, bl in enumerate(blines):
            if 'skipStatusProvideCheck: true' not in bl:
                continue
            ctx = ' '.join(blines[max(0, k - 3):k + 1])
            if not re.search(r'ShouldSustain|ShouldRefresh|BMRShouldRefreshBefore|WillStatusEnd|CommandNextAction|ShouldEndSpecial|IsLastAction|skipStatusProvideCheck: true.*?\bcommand', ctx):
                R['f_skipprovide_ungated'].append(f'{path}:{start + k}: [{name}] {bl.strip()[:110]}')

        # (g) contradictory level predicates on one action in a single condition
        for k, bl in enumerate(blines):
            pos = set(re.findall(r'(?<![!\w])(\w+PvE)\.EnoughLevel', bl))
            neg = set(re.findall(r'!(\w+PvE)\.EnoughLevel', bl))
            both = pos & neg
            if both:
                R['g_contradictory_level'].append(f'{path}:{start + k}: {sorted(both)} | {bl.strip()[:100]}')

        # (h) identical condition twice in the same method (dead second branch)
        conds = collections.Counter()
        for bl in blines:
            m = re.match(r'\s*if\s*\((.+)\)\s*$', bl)
            if m:
                c = re.sub(r'\s+', '', m.group(1))
                if len(c) > 25 and 'out act' not in c and 'out var' not in c:
                    conds[c] += 1
        for c, n in conds.items():
            if n > 1:
                R['h_repeated_condition'].append(f'{path}:{start}: [{name}] x{n}: {c[:100]}')

        # (i) division by a value that is not obviously guarded
        for k, bl in enumerate(blines):
            for m in re.finditer(r'/\s*(\w+(?:\.\w+)*)\s*(?:[;,)\]]|$)', bl):
                v = m.group(1)
                if v.replace('.', '').isdigit() or v.endswith('f'):
                    continue
                ctx = ' '.join(blines[max(0, k - 4):k + 1])
                if re.search(re.escape(v) + r'\s*(?:!=|>)\s*0|Math\.Max\(', ctx):
                    continue
                if re.search(r'Count|Length|Total|Max\b', v):
                    R['i_unguarded_division'].append(f'{path}:{start + k}: {bl.strip()[:110]}')

def _run(text):
    """Strip and scan one constructed source, returning the keys that fired."""
    R = collections.defaultdict(list)
    scan_source('t.cs', strip(text), R)
    return R


def _in_method(stmt):
    """Wrap a statement in a method so the method-scoped checks (e-i) see it."""
    return 'private bool Probe(out IAction? act)\n{\n    ' + stmt + '\n    return false;\n}\n'


def self_test():
    # Each check gets a source that must fire and one that must stay silent.
    assert _run('if (t.GetEffectiveHpPercent() < HealRatio) { }')['a_percent_vs_ratio'], 'a'
    assert not _run('if (t.GetEffectiveHpPercent() < 40) { }')['a_percent_vs_ratio'], 'a: literal is not a config'

    assert _run('if (t.GetHealthRatio() < 40) { }')['b_ratio_vs_percent'], 'b'
    assert not _run('if (t.GetHealthRatio() < 0.4f) { }')['b_ratio_vs_percent'], 'b: proper ratio'

    assert _run('if (WeaponRemain == 2.5f) { }')['c_float_equality'], 'c'
    assert not _run('if (WeaponRemain == 0) { }')['c_float_equality'], 'c: zero is exact'

    assert _run('if (HostileTarget?.HasStatus(true, StatusID.Addle)) { }')['d_hostile_status_fromself'], 'd'
    assert not _run('if (HostileTarget?.HasStatus(false, StatusID.Addle)) { }')['d_hostile_status_fromself'], 'd'

    assert _run(_in_method('if (ReprisalPvE.CanUse(out act, usedUp: true))'))['e_usedup_unconditional'], 'e'
    assert not _run(_in_method('if (InBurst && ReprisalPvE.CanUse(out act, usedUp: true))'))['e_usedup_unconditional'], 'e: gated'

    assert _run(_in_method('if (AddlePvE.CanUse(out act, skipStatusProvideCheck: true))'))['f_skipprovide_ungated'], 'f'
    assert not _run(_in_method('if (BMRShouldRefreshBefore(x) && AddlePvE.CanUse(out act, skipStatusProvideCheck: true))'))['f_skipprovide_ungated'], 'f: gated'

    assert _run(_in_method('if (!ImpactPvE.EnoughLevel && ImpactPvE.EnoughLevel) { }'))['g_contradictory_level'], 'g'
    assert not _run(_in_method('if (!ImpactPvE.EnoughLevel && JoltPvE.EnoughLevel) { }'))['g_contradictory_level'], 'g: different actions'

    twice = _in_method('if (SomethingRatherLongAndDistinctive && AnotherPart)\n    if (SomethingRatherLongAndDistinctive && AnotherPart)')
    assert _run(twice)['h_repeated_condition'], 'h'
    assert not _run(_in_method('if (SomethingRatherLongAndDistinctive && AnotherPart)'))['h_repeated_condition'], 'h: once only'

    assert _run(_in_method('var avg = total / PartyMembers.Count;'))['i_unguarded_division'], 'i'
    assert not _run(_in_method('if (PartyMembers.Count > 0)\n    var avg = total / PartyMembers.Count;'))['i_unguarded_division'], 'i: guarded'

    # strip() must run first: a commented-out line is not code.
    assert not _run('// if (t.GetHealthRatio() < 40) { }')['b_ratio_vs_percent'], 'strip: comment leaked'

    print('self-test ok: all nine checks fire on a constructed defect and stay silent otherwise\n')


def main():
    self_test()
    files = []
    for r in ROOTS:
        for dp, _, ns in os.walk(r):
            for n in ns:
                if n.endswith('.cs'):
                    files.append(os.path.join(dp, n))
    if not files:
        print('no C# files found - run from the repository root')
        return 1

    R = collections.defaultdict(list)
    for path in files:
        scan_source(path, strip(open(path, encoding='utf-8').read()), R)

    for k in sorted(R):
        print(f'\n== {k} ({len(R[k])})')
        for x in R[k][:40]:
            print('  ' + x)
        if len(R[k]) > 40:
            print(f'  ... {len(R[k]) - 40} more')
    print(f'\nscanned {len(files)} files')
    return 0


if __name__ == '__main__':
    sys.exit(main())
