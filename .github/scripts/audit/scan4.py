#!/usr/bin/env python3
"""Phase 4: config declarations - default outside declared range. Self-test included."""
import re, sys

rng = re.compile(r'Range\(\s*([-\d.eEfF]+)\s*,\s*([-\d.eEfF]+)\s*,\s*ConfigUnitType\.(\w+)')
decl = re.compile(r'^\s*(?:public|private|internal|protected)\s+(?:readonly\s+)?[\w<>?\[\]]+\s+(\w+)\s*(?:\{[^{}]*\})?\s*=\s*([^;]+);')

def num(s):
    s = s.strip().rstrip('fFdDmM')
    try: return float(s)
    except ValueError: return None

def scan(lines, path):
    out, pairs = [], 0
    cur = None          # (linenumber, lo, hi, unit) from the most recent attribute block
    inattr = False
    for i, line in enumerate(lines):
        s = line.strip()
        m = rng.search(line)
        if m:
            cur = (i, num(m.group(1)), num(m.group(2)), m.group(3))
        d = decl.match(line)
        if d:
            if cur is not None:
                pairs += 1
                lo, hi, unit = cur[1], cur[2], cur[3]
                val = num(d.group(2))
                if val is not None and lo is not None and hi is not None and (val < lo or val > hi):
                    out.append(f'{path}:{i+1}: {d.group(1)} default {d.group(2).strip()} outside [{lo},{hi}] ({unit})')
            cur = None
        elif s.endswith(']') and not s.startswith('['):
            pass        # continuation of a multi-line attribute: keep cur
        elif s and not s.startswith('[') and not s.startswith('//') and cur is not None and not s.endswith(','):
            cur = None
    return out, pairs

# --- self-test with constructed defects ---
probe = [
 '[Range(0, 1, ConfigUnitType.Percent, 0.02f)]',
 'public float A { get; set; } = 0.15f;',            # ok
 '[Range(0, 1, ConfigUnitType.Percent)]',
 'public float B { get; set; } = 15f;',              # DEFECT
 '[JobConfig, Range(1, 60, ConfigUnitType.Seconds)]',
 '[UI("text",',
 '    Parent = nameof(X))]',
 'private readonly float _c = 0.5f;',                # DEFECT (below 1)
]
res, pr = scan(probe, 'probe')
assert pr == 3, f'pairs={pr}'
assert len(res) == 2, res
print('self-test ok: %d pairs, %d defects detected\n' % (pr, len(res)))

files = ['RotationSolver.Basic/Configuration/Configs.cs']
total_pairs = 0
for path in files:
    lines = open(path, encoding='utf-8').read().split('\n')
    res, pr = scan(lines, path)
    total_pairs += pr
    for x in res: print('  ' + x)
print(f'\n{total_pairs} range/default pairs checked')
