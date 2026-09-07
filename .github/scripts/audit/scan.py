#!/usr/bin/env python3
"""Whole-tree structural scans for defect classes this repo has actually had.

The eight checks below live in scan_source() so the self-test can drive them against constructed
defects. Before that split they ran inline in the file loop and could not be exercised at all - a
silent zero from a broken pattern was indistinguishable from a clean tree, which is exactly the
failure mode scan3.py through scan8.py already carry self-tests for.
"""
import os, re, sys, collections

ROOTS = ['RotationSolver', 'RotationSolver.Basic']

def strip(src):
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    src = re.sub(r'//[^\n]*', '', src)
    return re.sub(r'"(?:\\.|[^"\\])*"', '""', src)

SIG = re.compile(r'\[RotationDesc\(([^)]*)\)\]\s*(?:public|protected|private|internal)[\w\s]*\boverride\s+(?:bool|IAction\?)\s+(\w+)\s*\(')

def method_body(src, start):
    i = src.find('{', start)
    d = 0
    j = i
    while j < len(src):
        if src[j] == '{': d += 1
        elif src[j] == '}':
            d -= 1
            if d == 0: return src[i:j]
        j += 1
    return src[i:]

def line_of(src, pos): return src[:pos].count('\n') + 1


def scan_source(path, src, R):
    """Run every check against one already-stripped source. Appends to R in place."""
    # (a) RotationDesc actions not used in the method body
    for m in SIG.finditer(src):
        listed = re.findall(r'ActionID\.(\w+)', m.group(1))
        body = method_body(src, m.end())
        for a in listed:
            if not re.search(r'\b' + a + r'\b', body):
                R['a_desc_not_in_body'].append(f'{path}:{line_of(src, m.start())}: {m.group(2)} lists {a}, body never uses it')

    # (b) if (X.CanUse(out _)) { return true; }  -> returns without setting act
    for m in re.finditer(r'if\s*\(([^{;]*?\.CanUse\(out _[^{;]*?)\)\s*\{\s*return true;\s*\}', src):
        if 'out act' not in m.group(1):
            R['b_return_true_out_discard'].append(f'{path}:{line_of(src, m.start())}: {m.group(1).strip()[:90]}')

    # (c) [Range(a, b ...)] ... = default; default outside [a, b]
    for m in re.finditer(r'\[Range\(\s*([-\d.]+)f?\s*,\s*([-\d.]+)f?[^\]]*\]\s*(?:\[[^\]]*\]\s*)*public\s+(?:int|float|byte|uint)\s+(\w+)\s*\{[^}]*\}\s*=\s*([-\d.]+)f?;', src):
        lo, hi, name, dv = float(m.group(1)), float(m.group(2)), m.group(3), float(m.group(4))
        if not (lo <= dv <= hi):
            R['c_default_outside_range'].append(f'{path}:{line_of(src, m.start())}: {name} = {dv} not in [{lo}, {hi}]')

    # (d) [RotationConfig] property never read anywhere in this file
    for m in re.finditer(r'\[RotationConfig\([^\]]*\)\]\s*(?:\[[^\]]*\]\s*)*public\s+[\w<>?]+\s+(\w+)\s*\{', src):
        name = m.group(1)
        uses = len(re.findall(r'\b' + name + r'\b', src)) - 1
        if uses == 0:
            R['d_unused_rotation_config'].append(f'{path}:{line_of(src, m.start())}: {name}')

    # (e) X.Target.Target.member without ?. and without a CanUse on the same statement
    for i, line in enumerate(src.split('\n'), 1):
        if re.search(r'\.Target\.Target\.\w', line) and '.CanUse(' not in line and 'Target.Target?' not in line:
            R['e_target_deref_without_canuse'].append(f'{path}:{i}: {line.strip()[:110]}')

    # (f) identical consecutive `if (...)` conditions (second is dead if first returns)
    lines = src.split('\n')
    prev_cond, prev_i = None, 0
    for i, line in enumerate(lines, 1):
        m = re.match(r'\s*if\s*\((.*)\)\s*$', line)
        if m:
            cond = re.sub(r'\s+', '', m.group(1))
            # "out act" has to be looked for in the raw condition: cond has had all whitespace
            # removed, so the spaced form could never be found in it and this check never fired.
            if cond == prev_cond and i - prev_i <= 6 and 'out act' in m.group(1):
                R['f_duplicate_consecutive_if'].append(f'{path}:{i}: {m.group(1).strip()[:100]}')
            prev_cond, prev_i = cond, i

    # (g) HasStatus(true, <enemy debuff>) — StatusFromSelf mismatch on enemy debuffs
    for m in re.finditer(r'HasStatus\(\s*true\s*,\s*StatusID\.(Addle|Feint|Reprisal|Dismantle|Reprisal_1193)\b', src):
        R['g_self_flag_on_enemy_debuff'].append(f'{path}:{line_of(src, m.start())}: {m.group(0)}')

    # (h) override that only calls base (pure passthrough) — noise, not a defect
    for m in re.finditer(r'(?:public|protected)\s+override\s+bool\s+(\w+)\([^)]*\)\s*\{\s*return base\.\1\([^)]*\);\s*\}', src):
        R['h_passthrough_override'].append(f'{path}:{line_of(src, m.start())}: {m.group(1)}')


def _run(text):
    """Strip and scan one constructed source, returning the result keys that fired."""
    R = collections.defaultdict(list)
    scan_source('t.cs', strip(text), R)
    return R


def self_test():
    # Every check gets one source that must fire and one that must stay silent. A pattern that
    # stopped matching would otherwise report a clean tree.
    hit = _run('''
    [RotationDesc(ActionID.SwiftcastPvE)]
    protected override bool EmergencyAbility(IAction n, out IAction? act) { act = null; return false; }
    ''')
    assert hit['a_desc_not_in_body'], 'a: listed action absent from body not caught'
    quiet = _run('''
    [RotationDesc(ActionID.SwiftcastPvE)]
    protected override bool EmergencyAbility(IAction n, out IAction? act) { return SwiftcastPvE.CanUse(out act); }
    ''')
    assert not quiet['a_desc_not_in_body'], 'a: false positive when the body does use it'

    assert _run('if (ThinAirPvE.CanUse(out _)) { return true; }')['b_return_true_out_discard'], 'b'
    assert not _run('if (ThinAirPvE.CanUse(out act)) { return true; }')['b_return_true_out_discard'], 'b: out act is fine'

    assert _run('[Range(0, 1)] public float Heal { get; set; } = 5;')['c_default_outside_range'], 'c'
    assert not _run('[Range(0, 1)] public float Heal { get; set; } = 0.3f;')['c_default_outside_range'], 'c: in range'

    assert _run('[RotationConfig(CombatType.PvE)] public bool Unused { get; set; }')['d_unused_rotation_config'], 'd'
    assert not _run('[RotationConfig(CombatType.PvE)] public bool Used { get; set; }\nif (Used) { }')['d_unused_rotation_config'], 'd: read once'

    assert _run('var x = Player.Target.Target.CurrentHp;')['e_target_deref_without_canuse'], 'e'
    assert not _run('var x = Player.Target.Target?.CurrentHp;')['e_target_deref_without_canuse'], 'e: null-conditional'

    dup = _run('        if (FooPvE.CanUse(out act))\n        if (FooPvE.CanUse(out act))\n')
    assert dup['f_duplicate_consecutive_if'], 'f'
    assert not _run('        if (FooPvE.CanUse(out act))\n        if (BarPvE.CanUse(out act))\n')['f_duplicate_consecutive_if'], 'f: different conditions'

    assert _run('if (HasStatus(true, StatusID.Addle)) { }')['g_self_flag_on_enemy_debuff'], 'g'
    assert not _run('if (HasStatus(false, StatusID.Addle)) { }')['g_self_flag_on_enemy_debuff'], 'g: false flag is correct'

    assert _run('protected override bool AttackAbility(IAction n, out IAction? act) { return base.AttackAbility(n, out act); }')['h_passthrough_override'], 'h'
    assert not _run('protected override bool AttackAbility(IAction n, out IAction? act) { return base.GeneralAbility(n, out act); }')['h_passthrough_override'], 'h: different base method'

    # A comment must not be read as code - strip() runs before every check.
    assert not _run('// if (ThinAirPvE.CanUse(out _)) { return true; }')['b_return_true_out_discard'], 'strip: comment leaked'

    print('self-test ok: all eight checks fire on a constructed defect and stay silent otherwise\n')


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
        for x in R[k][:80]:
            print('  ' + x)
        if len(R[k]) > 80:
            print(f'  ... {len(R[k]) - 80} more')
    print(f'\nscanned {len(files)} files')
    return 0


if __name__ == '__main__':
    sys.exit(main())
