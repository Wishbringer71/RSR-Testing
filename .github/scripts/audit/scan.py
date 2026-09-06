#!/usr/bin/env python3
"""Whole-tree structural scans for defect classes this repo has actually had."""
import os, re, sys, collections

ROOTS = ['RotationSolver', 'RotationSolver.Basic']
files = []
for r in ROOTS:
    for dp, _, ns in os.walk(r):
        for n in ns:
            if n.endswith('.cs'):
                files.append(os.path.join(dp, n))

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

R = collections.defaultdict(list)

for path in files:
    raw = open(path, encoding='utf-8').read()
    src = strip(raw)

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
            if cond == prev_cond and i - prev_i <= 6 and 'out act' in cond:
                R['f_duplicate_consecutive_if'].append(f'{path}:{i}: {m.group(1).strip()[:100]}')
            prev_cond, prev_i = cond, i

    # (g) HasStatus(true, <enemy debuff>) — StatusFromSelf mismatch on enemy debuffs
    for m in re.finditer(r'HasStatus\(\s*true\s*,\s*StatusID\.(Addle|Feint|Reprisal|Dismantle|Reprisal_1193)\b', src):
        R['g_self_flag_on_enemy_debuff'].append(f'{path}:{line_of(src, m.start())}: {m.group(0)}')

    # (h) override that only calls base (pure passthrough) — noise, not a defect
    for m in re.finditer(r'(?:public|protected)\s+override\s+bool\s+(\w+)\([^)]*\)\s*\{\s*return base\.\1\([^)]*\);\s*\}', src):
        R['h_passthrough_override'].append(f'{path}:{line_of(src, m.start())}: {m.group(1)}')

for k in sorted(R):
    print(f'\n== {k} ({len(R[k])})')
    for x in R[k][:80]:
        print('  ' + x)
    if len(R[k]) > 80:
        print(f'  ... {len(R[k]) - 80} more')
print(f'\nscanned {len(files)} files')
