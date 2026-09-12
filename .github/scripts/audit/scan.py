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

# Any method declaration in the same file, by name, so a body's callees can be followed.
DECL = re.compile(r'\b(\w+)\s*\([^;{)]*\)\s*(?:\{|=>)')
# A call in a body: a name followed by an open parenthesis, keywords excluded below.
CALL = re.compile(r'\b([A-Za-z_]\w*)\s*\(')
NOT_CALLS = frozenset(['if', 'while', 'for', 'foreach', 'switch', 'return', 'catch', 'lock',
                       'using', 'nameof', 'typeof', 'sizeof', 'new', 'fixed'])
# How far a callee is followed. One hop is what the tree needs - a rotation puts the action in a
# helper the dispatch method calls - and two guards against a helper that only forwards.
CALLEE_DEPTH = 2
# The start of the method a line sits in: a declaration at class-member indentation. Used to
# read only the enclosing method rather than the whole file.
METHOD_START = re.compile(r'\n\t(?:public|protected|private|internal)[^\n(]*\(')


def reachable_body(src, start, depth=CALLEE_DEPTH):
    """The method body at start plus the bodies of the methods it calls, up to depth hops.

    Measuring the immediate body alone measures a surrogate: a rotation that moves the action into
    a helper still casts it, and the check reported nine such methods as if the action were never
    used - SCH's Sacred Soil, PLD's Sheltron, SGE's Eukrasian Prognosis II among them. What the
    RotationDesc claims is that the method *leads to* the action, so that is what has to be read.
    """
    body = method_body(src, start)
    seen, frontier = set(), [body]
    for _ in range(depth):
        names = set()
        for text in frontier:
            names.update(n for n in CALL.findall(text) if n not in NOT_CALLS)
        frontier = []
        for name in sorted(names - seen):
            seen.add(name)
            for decl in DECL.finditer(src):
                if decl.group(1) == name:
                    extra = method_body(src, decl.end() - 1)
                    body += '\n' + extra
                    frontier.append(extra)
    return body


def scan_source(path, src, R):
    """Run every check against one already-stripped source. Appends to R in place."""
    # (a) RotationDesc actions the method does not lead to, directly or through a helper
    for m in SIG.finditer(src):
        listed = re.findall(r'ActionID\.(\w+)', m.group(1))
        body = reachable_body(src, m.end())
        for a in listed:
            if not re.search(r'\b' + a + r'\b', body):
                R['a_desc_not_in_body'].append(f'{path}:{line_of(src, m.start())}: {m.group(2)} lists {a}, neither its body nor a helper it calls uses it')

    # (b) if (X.CanUse(out _)) { return true; } -> returns true with no action to return.
    #
    # The discarded result alone proves nothing: `CanUse(out _)` as a pure test beside an action
    # that some earlier line already chose is correct and common - GNB tests DemonSlice inside a
    # BowShock block, and CustomRotation_Ability re-validates an action it was *given*. It is a
    # defect only where nothing in the enclosing method ever assigns act, which is what makes the
    # `return true` a promise of an action that is not there.
    for m in re.finditer(r'if\s*\(([^{;]*?\.CanUse\(out _[^{;]*?)\)\s*\{\s*return true;\s*\}', src):
        if 'out act' in m.group(1):
            continue
        before = src[:m.start()]
        starts = [d.start() for d in METHOD_START.finditer(before)]
        method = before[starts[-1]:] if starts else before
        if re.search(r'\bout act\b|\bact\s*=', method):
            continue
        R['b_return_true_out_discard'].append(f'{path}:{line_of(src, m.start())}: {m.group(1).strip()[:90]}')

    # (c) [Range(a, b ...)] ... = default; default outside [a, b]
    for m in re.finditer(r'\[Range\(\s*([-\d.]+)f?\s*,\s*([-\d.]+)f?[^\]]*\]\s*(?:\[[^\]]*\]\s*)*public\s+(?:int|float|byte|uint)\s+(\w+)\s*\{[^}]*\}\s*=\s*([-\d.]+)f?;', src):
        lo, hi, name, dv = float(m.group(1)), float(m.group(2)), m.group(3), float(m.group(4))
        if not (lo <= dv <= hi):
            R['c_default_outside_range'].append(f'{path}:{line_of(src, m.start())}: {name} = {dv} not in [{lo}, {hi}]')

    # (d) [RotationConfig] property never read anywhere in this file
    for m in re.finditer(r'\[RotationConfig\(([^\]]*)\)\]\s*(?:\[[^\]]*\]\s*)*public\s+[\w<>?]+\s+(\w+)\s*\{', src):
        args, name = m.group(1), m.group(2)
        # A label spread over several lines is the setting itself: the rotation notes the Beiruta
        # files render in the settings window carry their whole content in Name, concatenated line
        # by line, and the property is only the peg they hang on. Nothing is meant to read it, so
        # an unread one is not a broken promise. (String contents are blanked by strip() before
        # this runs, but the line breaks between the concatenated pieces survive.)
        if '\n' in args:
            continue
        uses = len(re.findall(r'\b' + name + r'\b', src)) - 1
        if uses == 0:
            R['d_unused_rotation_config'].append(f'{path}:{line_of(src, m.start())}: {name}')

    # (e) X.Target.Target.member dereferenced with nothing in the method establishing it is there.
    #
    # `BaseAction.Target` is a non-nullable TargetResult, so only the IBattleChara inside it can be
    # null. The guard for it is routinely a *separate line* of the same condition -
    # `TheEwerPvE.CanUse(out act) && TheEwerPvE.Target.Target != null && ...Target.Target.Get...` -
    # so reading one line at a time measured formatting, not safety, and called all 60 of those
    # correct sites defects. The guard is looked for in the enclosing method, keyed to the same
    # action, which is the scope that actually decides it.
    #
    # Two limits of that, both deliberate. An enclosing `if (X.CanUse(out act))` counts as a guard
    # even when the dereference is not short-circuited by it: 58 of the 87 sites in this tree are
    # guarded by nesting rather than by `&&`, and CanUse does establish the target - it returns
    # false on a null PreviewTarget and assigns Target otherwise. And it counts even in the one
    # case where CanUse returns true *without* assigning Target: the action-preview pass, where
    # `BaseAction.cs:264` skips the assignment on purpose. The dereference then reads a stale or
    # default TargetResult. That is a real defect class, but it belongs to the preview path rather
    # than to any single line, and a line-based check cannot separate it - see TODO.md.
    offset = 0
    for i, line in enumerate(src.split('\n'), 1):
        hit = re.search(r'(\w+Pv[EP])\.Target\.Target\.\w', line)
        if hit and 'Target.Target?' not in line:
            action = hit.group(1)
            before = src[:offset]
            starts = [d.start() for d in METHOD_START.finditer(before)]
            method = src[starts[-1]:offset + len(line)] if starts else src[:offset + len(line)]
            guarded = re.search(
                re.escape(action) + r'\.(?:CanUse\(|Target\.Target\s*(?:!=\s*null|is not null)|Target\.Target\?)',
                method)
            if not guarded:
                R['e_target_deref_unguarded'].append(f'{path}:{i}: {line.strip()[:110]}')
        offset += len(line) + 1

    # (f) identical consecutive `if (...)` conditions (second is dead if first returns)
    lines = src.split('\n')
    prev_cond, prev_i = None, 0
    for i, line in enumerate(lines, 1):
        m = re.match(r'\s*if\s*\((.*)\)\s*$', line)
        if m:
            cond = re.sub(r'\s+', '', m.group(1))
            # "out act" has to be looked for in the raw condition: cond has had all whitespace
            # removed, so the spaced form could never be found in it and this check never fired.
            # Nothing may divert control between the two, or the second is simply the same
            # question asked on a different path: a `break` puts them in two switch arms, an
            # `else` in two arms of one branch. Both were reported as duplicates in BeirutaSCH,
            # where they are correct.
            between = '\n'.join(lines[prev_i:i - 1])
            diverted = bool(re.search(r'\b(?:else|break|case|default|continue|goto)\b', between))
            if cond == prev_cond and i - prev_i <= 6 and 'out act' in m.group(1) and not diverted:
                R['f_duplicate_consecutive_if'].append(f'{path}:{i}: {m.group(1).strip()[:100]}')
            prev_cond, prev_i = cond, i

    # (g) HasStatus(true, <enemy debuff>) — StatusFromSelf mismatch on enemy debuffs
    for m in re.finditer(r'HasStatus\(\s*true\s*,\s*StatusID\.(Addle|Feint|Reprisal|Dismantle|Reprisal_1193)\b', src):
        R['g_self_flag_on_enemy_debuff'].append(f'{path}:{line_of(src, m.start())}: {m.group(0)}')

    # (h) override that only calls base. Counted, not reported: it is behaviour-identical to no
    # override at all, so there is nothing to fix, and 17 such lines in the output crowd out the
    # findings that do need a reader. check_base_calls.py is what guards the case that does matter
    # here - a passthrough naming the *wrong* base method.
    for m in re.finditer(r'(?:public|protected)\s+override\s+bool\s+(\w+)\([^)]*\)\s*\{\s*return base\.\1\([^)]*\);\s*\}', src):
        R['_counted_passthrough_override'].append(f'{path}:{line_of(src, m.start())}: {m.group(1)}')


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
    # A pure test beside an action an earlier line already chose is correct: the `return true`
    # has an action to return. Reported as a defect five times before this condition existed.
    assert not _run('\n\tprotected bool M(out IAction? act)\n\t{\n\t\tif (BowShockPvE.CanUse(out act))\n\t\t{\n\t\t\tif (DemonSlicePvE.CanUse(out _)) { return true; }\n\t\t}\n\t\treturn false;\n\t}')['b_return_true_out_discard'], 'b: act already chosen'

    assert _run('[Range(0, 1)] public float Heal { get; set; } = 5;')['c_default_outside_range'], 'c'
    assert not _run('[Range(0, 1)] public float Heal { get; set; } = 0.3f;')['c_default_outside_range'], 'c: in range'

    assert _run('[RotationConfig(CombatType.PvE)] public bool Unused { get; set; }')['d_unused_rotation_config'], 'd'
    assert not _run('[RotationConfig(CombatType.PvE)] public bool Used { get; set; }\nif (Used) { }')['d_unused_rotation_config'], 'd: read once'
    assert not _run('[RotationConfig(CombatType.PvE, Name =\n    "Note:\\n" +\n    "• a line")]\n'
                    'public bool Notes { get; set; }')['d_unused_rotation_config'], 'd: display label'

    assert _run('\n\tprivate bool M()\n\t{\n\t\tvar x = FesterPvE.Target.Target.CurrentHp;\n\t}'
                )['e_target_deref_unguarded'], 'e'
    assert not _run('\n\tprivate bool M()\n\t{\n\t\tvar x = FesterPvE.Target.Target?.CurrentHp;\n\t}'
                    )['e_target_deref_unguarded'], 'e: null-conditional'
    # The guard on its own line of the same condition is the house style and is what made this
    # check report 60 correct sites.
    assert not _run('\n\tprivate bool M(out IAction? act)\n\t{\n\t\tif (FesterPvE.CanUse(out act) &&\n'
                    '\t\t\tFesterPvE.Target.Target != null &&\n'
                    '\t\t\tFesterPvE.Target.Target.GetHealthRatio() < 0.8f)\n\t\t{\n\t\t\treturn true;\n\t\t}\n\t}'
                    )['e_target_deref_unguarded'], 'e: guard on an earlier line'

    dup = _run('        if (FooPvE.CanUse(out act))\n        if (FooPvE.CanUse(out act))\n')
    assert dup['f_duplicate_consecutive_if'], 'f'
    assert not _run('        if (FooPvE.CanUse(out act))\n        if (BarPvE.CanUse(out act))\n')['f_duplicate_consecutive_if'], 'f: different conditions'
    # The same question in two switch arms or two branch arms is not a duplicate: control never
    # passes from one to the other.
    assert not _run('            if (FooPvE.CanUse(out act))\n                return act;\n'
                    '            break;\n        case Other:\n            if (FooPvE.CanUse(out act))\n'
                    )['f_duplicate_consecutive_if'], 'f: separate switch arms'
    assert not _run('            if (FooPvE.CanUse(out act))\n                return true;\n'
                    '        }\n        else\n        {\n            if (FooPvE.CanUse(out act))\n'
                    )['f_duplicate_consecutive_if'], 'f: separate branch arms'

    assert _run('if (HasStatus(true, StatusID.Addle)) { }')['g_self_flag_on_enemy_debuff'], 'g'
    assert not _run('if (HasStatus(false, StatusID.Addle)) { }')['g_self_flag_on_enemy_debuff'], 'g: false flag is correct'

    assert _run('protected override bool AttackAbility(IAction n, out IAction? act) { return base.AttackAbility(n, out act); }')['_counted_passthrough_override'], 'h'
    assert not _run('protected override bool AttackAbility(IAction n, out IAction? act) { return base.GeneralAbility(n, out act); }')['_counted_passthrough_override'], 'h: different base method'

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

    # A key starting with _ is surveyed, not reported: the class is real but every member of it
    # is correct code, and printing it would bury the findings that need a reader.
    for k in sorted(k for k in R if not k.startswith('_')):
        print(f'\n== {k} ({len(R[k])})')
        for x in R[k][:80]:
            print('  ' + x)
        if len(R[k]) > 80:
            print(f'  ... {len(R[k]) - 80} more')

    print(f'\nscanned {len(files)} files')
    for k in sorted(k for k in R if k.startswith('_')):
        print(f'{len(R[k])} {k.lstrip("_")}(s) surveyed and not reported - see the comment at the check')
    return 0


if __name__ == '__main__':
    sys.exit(main())
