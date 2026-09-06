#!/usr/bin/env python3
"""Phase 3 scans: defect classes typical for rotation files."""
import os, re, collections

files = []
for r in ['RotationSolver/RebornRotations', 'RotationSolver/ExtraRotations', 'RotationSolver.Basic/Rotations']:
    for dp, _, ns in os.walk(r):
        for n in ns:
            if n.endswith('.cs'):
                files.append(os.path.join(dp, n))

def strip(s):
    s = re.sub(r'/\*.*?\*/', '', s, flags=re.S)
    s = re.sub(r'//[^\n]*', '', s)
    return re.sub(r'"(?:\\.|[^"\\])*"', '""', s)

def block_after(lines, i):
    """Return the lines of the block opened at or after line i."""
    j = i
    while j < len(lines) and '{' not in lines[j]:
        if lines[j].strip() and j > i:
            return [lines[j]]          # brace-less single statement
        j += 1
    if j >= len(lines):
        return []
    d = 0
    out = []
    while j < len(lines):
        d += lines[j].count('{') - lines[j].count('}')
        out.append(lines[j])
        if d <= 0 and len(out) > 1:
            break
        j += 1
    return out

R = collections.defaultdict(list)

for path in files:
    src = strip(open(path, encoding='utf-8').read())
    lines = src.split('\n')

    for i, line in enumerate(lines):
        s = line.strip()

        # (a) an action is tested but its block never returns - the CanUse result is discarded
        m = re.match(r'if\s*\((?:[^()]|\([^()]*\))*?(\w+PvE|\w+PvP)\.CanUse\(out (?:act|action)\b[^)]*\)\s*\)\s*$', s)
        if m:
            blk = block_after(lines, i)
            txt = ' '.join(blk)
            if blk and 'return' not in txt and 'act =' not in txt and 'action =' not in txt:
                R['a_canuse_block_never_returns'].append(f'{path}:{i+1}: {m.group(1)} | {txt.strip()[:110]}')

        # (b) two consecutive if-blocks with an identical body (copy-paste without adjusting)
        m2 = re.match(r'if\s*\((.+)\)\s*$', s)
        if m2:
            b1 = block_after(lines, i)
            if len(b1) >= 2:
                k = i + 1 + len(b1)
                while k < len(lines) and not lines[k].strip():
                    k += 1
                if k < len(lines) and re.match(r'if\s*\((.+)\)\s*$', lines[k].strip()):
                    b2 = block_after(lines, k)
                    n1 = ' '.join(x.strip() for x in b1[1:-1] if x.strip())
                    n2 = ' '.join(x.strip() for x in b2[1:-1] if x.strip())
                    c1 = re.sub(r'\s+', '', m2.group(1))
                    c2 = re.sub(r'\s+', '', re.match(r'if\s*\((.+)\)\s*$', lines[k].strip()).group(1))
                    if n1 and n1 == n2 and c1 != c2 and len(n1) > 15:
                        R['b_identical_bodies'].append(f'{path}:{i+1}/{k+1}: {n1[:70]} | {c1[:45]} vs {c2[:45]}')

    # (c) a lower-level action gated behind the higher-level one being available
    for i, line in enumerate(lines):
        m = re.search(r'(\w+PvE)\.EnoughLevel\s*&&\s*(\w+PvE)\.CanUse', line)
        if m and m.group(1) != m.group(2):
            R['c_level_gate_other_action'].append(f'{path}:{i+1}: {m.group(1)}.EnoughLevel gates {m.group(2)} | {line.strip()[:100]}')

for k in sorted(R):
    print(f'\n== {k} ({len(R[k])})')
    for x in R[k][:45]:
        print('  ' + x)
    if len(R[k]) > 45:
        print(f'  ... {len(R[k]) - 45} more')
print(f'\nscanned {len(files)} rotation files')
