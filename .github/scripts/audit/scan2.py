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
# A scale conversion on the same statement: the comparison is between like units after all.
SCALED = re.compile(r'\*\s*100')
# An integer declaration, used to build the set of names that cannot hold a fractional value.
INT_DECL = re.compile(r'\b(?:uint|int|byte|sbyte|short|ushort|long|ulong)\??\s+(\w+)\b')


def top_level_or_branches(condition):
    """The condition split at `||` outside parentheses.

    `(X.EnoughLevel && A) || (!X.EnoughLevel && B)` is the ordinary level fallback - one branch for
    the action being learned, one for it not being - and reading the line as a whole made all twelve
    of those read as a contradiction. A contradiction is both polarities in the *same* branch.
    """
    # Split at the shallowest depth any `||` occurs at, not at depth zero: the text handed in is a
    # whole source line, so an `if (` already puts the operator one level in.
    depths, depth = [], 0
    for i, char in enumerate(condition):
        if char == '(':
            depth += 1
        elif char == ')':
            depth -= 1
        elif condition.startswith('||', i):
            depths.append(depth)
    if not depths:
        return [condition]

    cut = min(depths)
    parts, depth, current, i = [], 0, '', 0
    while i < len(condition):
        char = condition[i]
        if char == '(':
            depth += 1
        elif char == ')':
            depth -= 1
        if depth == cut and condition.startswith('||', i):
            parts.append(current)
            current = ''
            i += 2
            continue
        current += char
        i += 1
    parts.append(current)
    return parts


def scan_source(path, src, R, ints=frozenset()):
    """Run every check against one already-stripped source. Appends to R in place.

    `ints` holds the names the tree declares as an integer type; check (c) needs it to tell an
    exact comparison on a whole number from one on a float.
    """
    lines = src.split('\n')

    for i, line in enumerate(lines, 1):
        # (a) percent scale compared against something that looks like a 0..1 config
        for m in PCT.finditer(line):
            # A `* 100` on the same statement converts the 0..1 setting to the 0..100 scale
            # GetEffectiveHpPercent returns, so the units do match. All five hits in this tree did
            # exactly that, and the check was reporting the conversion it should have been looking
            # for.
            if SCALED.search(line):
                continue
            R['a_percent_vs_ratio'].append(f'{path}:{i}: {m.group(0)}')
        # (b) ratio compared against an integer literal > 1 (would be a percent value)
        for m in RATIO.finditer(line):
            if float(m.group(2)) > 1:
                R['b_ratio_vs_percent'].append(f'{path}:{i}: {m.group(0)}')
        # (c) float equality
        for m in re.finditer(r'\b(\w*(?:Ratio|Time|Remain|Range|Distance|Hp|Mp|Gauge)\w*)\s*(==|!=)\s*(-?\d+(?:\.\d+)?f?)\b', line):
            # Exact equality is only a hazard on a fractional type. `CurrentMp` is a uint capped by
            # Math.Min(10000, ...), so `== 10000` tests the cap exactly and is right; the name
            # matched only because it ends in "Mp". Deciding it by the declared type rather than by
            # a list of exempt names is what keeps this from ageing.
            if m.group(1) in ints:
                continue
            if m.group(3).rstrip('f') not in ('0', '1'):
                R['c_float_equality'].append(f'{path}:{i}: {m.group(0)}')
        # (d) status source flag: hostile debuff queried as if it were from anyone
        for m in re.finditer(r'HostileTarget\??\.HasStatus\(\s*true\s*,', line):
            R['d_hostile_status_fromself'].append(f'{path}:{i}: {line.strip()[:110]}')

    for name, body, start in methods(src):
        blines = body.split('\n')

        # (e) usedUp: true with no condition on its own statement. Counted, not reported.
        #
        # `usedUp: true` means "spend the last charge", and whether that should hang on a burst
        # condition is a judgement about rotation design, not a property of the code: 156 sites in
        # this tree do it, across every job and both foreign rotation families. A rule cannot
        # decide which of them is wrong, and a list nobody can act on costs the scan its standing.
        for k, bl in enumerate(blines):
            if 'usedUp: true' not in bl:
                continue
            # the statement's own condition: everything before the CanUse call on this line
            stmt = bl.strip()
            head = stmt.split('.CanUse(')[0]
            bare = re.match(r'^(?:if\s*\()?\s*!?[A-Za-z_]\w*(?:PvE|PvP)$', head.strip())
            if bare and 'return' not in stmt:
                R['_counted_usedup_unconditional'].append(f'{path}:{start + k}: [{name}] {stmt[:110]}')

        # (f) skipStatusProvideCheck: true without a refresh gate nearby. Counted, not reported.
        #
        # Same kind as (e): the flag says "cast it even though the status is already up", which is
        # right for a reapplication and wrong for a waste, and which of the two it is depends on
        # the action. 105 sites, no decidable rule.
        for k, bl in enumerate(blines):
            if 'skipStatusProvideCheck: true' not in bl:
                continue
            ctx = ' '.join(blines[max(0, k - 3):k + 1])
            if not re.search(r'ShouldSustain|ShouldRefresh|BMRShouldRefreshBefore|WillStatusEnd|CommandNextAction|ShouldEndSpecial|IsLastAction|skipStatusProvideCheck: true.*?\bcommand', ctx):
                R['_counted_skipprovide_ungated'].append(f'{path}:{start + k}: [{name}] {bl.strip()[:110]}')

        # (g) contradictory level predicates on one action in a single condition
        for k, bl in enumerate(blines):
            for branch in top_level_or_branches(bl):
                pos = set(re.findall(r'(?<![!\w])(\w+PvE)\.EnoughLevel', branch))
                neg = set(re.findall(r'!(\w+PvE)\.EnoughLevel', branch))
                both = pos & neg
                if both:
                    R['g_contradictory_level'].append(f'{path}:{start + k}: {sorted(both)} | {bl.strip()[:100]}')
                    break

        # (h) identical condition twice in one method. Counted, not reported - two reasons.
        #
        # It measures the stripped source, where every string literal has become "". Seven
        # `ImGui.CollapsingHeader("...")` calls in one UI method therefore read as one condition
        # repeated seven times. And even on distinct text the class is not a defect: the same
        # question asked in two role branches is correct, which is what
        # StateUpdater.ShouldAddDefenseSingle does with IsHostileCastingTankBusterAtMe (A6).
        conds = collections.Counter()
        for bl in blines:
            m = re.match(r'\s*if\s*\((.+)\)\s*$', bl)
            if m:
                c = re.sub(r'\s+', '', m.group(1))
                if len(c) > 25 and 'out act' not in c and 'out var' not in c:
                    conds[c] += 1
        for c, n in conds.items():
            if n > 1:
                R['_counted_repeated_condition'].append(f'{path}:{start}: [{name}] x{n}: {c[:100]}')

        # (i) division by a value that is not obviously guarded
        for k, bl in enumerate(blines):
            for m in re.finditer(r'/\s*(\w+(?:\.\w+)*)\s*(?:[;,)\]]|$)', bl):
                v = m.group(1)
                if v.replace('.', '').isdigit() or v.endswith('f'):
                    continue
                # The guard is looked for in the whole enclosing method, not in four lines of
                # context: DataCenter's party-HP maths returns early on `hpCount == 0` thirty lines
                # above the division, and a window that small reported it as unguarded.
                ctx = '\n'.join(blines[:k + 1])
                if re.search(re.escape(v) + r'\s*(?:!=|>|==|<=)\s*0|Math\.Max\(', ctx):
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
    assert not _run('if (t.GetEffectiveHpPercent() <= HealRatio * 100f) { }')['a_percent_vs_ratio'], 'a: scaled'

    assert _run('if (t.GetHealthRatio() < 40) { }')['b_ratio_vs_percent'], 'b'
    assert not _run('if (t.GetHealthRatio() < 0.4f) { }')['b_ratio_vs_percent'], 'b: proper ratio'

    assert _run('if (WeaponRemain == 2.5f) { }')['c_float_equality'], 'c'
    assert not _run('if (WeaponRemain == 0) { }')['c_float_equality'], 'c: zero is exact'
    # An exact comparison on an integer is not a float hazard; CurrentMp is a capped uint.
    R = collections.defaultdict(list)
    scan_source('t.cs', strip('if (CurrentMp == 10000) { }'), R, frozenset(['CurrentMp']))
    assert not R['c_float_equality'], 'c: integer-typed name'

    assert _run('if (HostileTarget?.HasStatus(true, StatusID.Addle)) { }')['d_hostile_status_fromself'], 'd'
    assert not _run('if (HostileTarget?.HasStatus(false, StatusID.Addle)) { }')['d_hostile_status_fromself'], 'd'

    assert _run(_in_method('if (ReprisalPvE.CanUse(out act, usedUp: true))'))['_counted_usedup_unconditional'], 'e'
    assert not _run(_in_method('if (InBurst && ReprisalPvE.CanUse(out act, usedUp: true))'))['_counted_usedup_unconditional'], 'e: gated'

    assert _run(_in_method('if (AddlePvE.CanUse(out act, skipStatusProvideCheck: true))'))['_counted_skipprovide_ungated'], 'f'
    assert not _run(_in_method('if (BMRShouldRefreshBefore(x) && AddlePvE.CanUse(out act, skipStatusProvideCheck: true))'))['_counted_skipprovide_ungated'], 'f: gated'

    assert _run(_in_method('if (!ImpactPvE.EnoughLevel && ImpactPvE.EnoughLevel) { }'))['g_contradictory_level'], 'g'
    assert not _run(_in_method('if (!ImpactPvE.EnoughLevel && JoltPvE.EnoughLevel) { }'))['g_contradictory_level'], 'g: different actions'
    # Both polarities of one action, but in the two arms of a `||` - the level fallback, not a
    # contradiction. All twelve hits in this tree were this shape.
    assert not _run(_in_method('if ((ImpactPvE.EnoughLevel && HasBuff) || (!ImpactPvE.EnoughLevel && Other)) { }'))['g_contradictory_level'], 'g: level fallback'

    twice = _in_method('if (SomethingRatherLongAndDistinctive && AnotherPart)\n    if (SomethingRatherLongAndDistinctive && AnotherPart)')
    assert _run(twice)['_counted_repeated_condition'], 'h'
    assert not _run(_in_method('if (SomethingRatherLongAndDistinctive && AnotherPart)'))['_counted_repeated_condition'], 'h: once only'

    assert _run(_in_method('var avg = total / PartyMembers.Count;'))['i_unguarded_division'], 'i'
    assert not _run(_in_method('if (PartyMembers.Count > 0)\n    var avg = total / PartyMembers.Count;'))['i_unguarded_division'], 'i: guarded'
    # An early return far above the division is a guard too; four lines of context missed it.
    assert not _run(_in_method('if (PartyMembers.Count == 0) return;' + '\n    var x = 1;' * 20 +
                               '\n    var avg = total / PartyMembers.Count;'))['i_unguarded_division'], 'i: early return'

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

    sources = {path: strip(open(path, encoding='utf-8').read()) for path in files}
    # Built once over the whole tree: a name declared as an integer anywhere is an integer
    # everywhere here, and check (c) must not call an exact comparison on one a float hazard.
    ints = frozenset(n for src in sources.values() for n in INT_DECL.findall(src))

    R = collections.defaultdict(list)
    for path, src in sources.items():
        scan_source(path, src, R, ints)

    for k in sorted(k for k in R if not k.startswith('_')):
        print(f'\n== {k} ({len(R[k])})')
        for x in R[k][:40]:
            print('  ' + x)
        if len(R[k]) > 40:
            print(f'  ... {len(R[k]) - 40} more')

    print(f'\nscanned {len(files)} files, {len(ints)} integer-typed names indexed')
    # Surveyed, not reported: the class is real but nothing in it is decidable as a defect by a
    # static rule. See the comment at each check.
    for k in sorted(k for k in R if k.startswith('_')):
        print(f'{len(R[k])} {k.lstrip("_")}(s) surveyed and not reported')
    return 0


if __name__ == '__main__':
    sys.exit(main())
