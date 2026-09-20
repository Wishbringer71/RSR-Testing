#!/usr/bin/env python3
"""Find defensive/mitigation actions used inside methods that carry no danger gate.

The dispatcher calls Defense*/Heal* only when the corresponding AutoStatus flag is set, so a
mitigation placed in one of the ungated methods below fires on cooldown rather than on danger. That
is the class behind the HasHostileCountAoeMitigation finding (AUDIT_LOG C9).

The scan lives in scan_file() so the self-test can drive it against constructed sources; before that
split it ran inline and a broken pattern would have reported a clean tree.
"""
import os, re, sys

# What counts as a mitigation is not listed here any more. It used to be 35 names typed out, which
# is the ageing form Parnas calls lack of movement: the list was right when it was written, and a
# job action added later was simply invisible to the scan without anything failing.
#
# The tree states it itself - every one of these actions names its own figure in its effect text -
# and generate_defensive_values.py reads those texts into DefensiveValues.g.cs. Asking the generated
# table is the capability check that replaces the enumeration.
def load_mitigations():
    table = os.path.join(os.path.dirname(__file__), '..', '..', '..',
                         'RotationSolver.Basic', 'Data', 'DefensiveValues.g.cs')
    names = []
    try:
        with open(table, encoding='utf-8') as handle:
            for line in handle:
                found = re.search(r'//\s*(\w+PvE)\s*$', line)
                if found and '] = new(' in line:
                    names.append(found.group(1))
    except OSError:
        return []
    return names


MIT = load_mitigations()
# methods that run without any AutoStatus danger gate
UNGATED = ['GeneralAbility','AttackAbility','EmergencyAbility','GeneralGCD','AttackGCD']

sig = re.compile(r'(?:public|protected|private)[\w\s]*\b(?:override\s+)?bool\s+(\w+)\s*\([^)]*\)\s*\{')

def strip(src):
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    src = re.sub(r'//[^\n]*', '', src)
    return src

def body(src, i):
    d=0; j=src.find('{', i)
    st=j
    while j < len(src):
        if src[j]=='{': d+=1
        elif src[j]=='}':
            d-=1
            if d==0: return src[st:j]
        j+=1
    return src[st:]


def scan_file(path, src):
    """Yield (path, line, method, action, context) for each mitigation inside an ungated method."""
    out = []
    for m in sig.finditer(src):
        name = m.group(1)
        if name not in UNGATED:
            continue
        b = body(src, m.end()-1)
        for a in MIT:
            for mm in re.finditer(r'\b'+a+r'\.CanUse\(', b):
                line = src[:m.end()].count('\n') + b[:mm.start()].count('\n') + 1
                ctx = b[max(0, mm.start()-260):mm.start()+60].replace('\n', ' ')
                ctx = re.sub(r'\s+', ' ', ctx)[-190:]
                out.append((path, line, name, a, ctx))
    return out


def self_test():
    ungated = '''
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (ReprisalPvE.CanUse(out act)) { return true; }
        return false;
    }
    '''
    hits = scan_file('t.cs', strip(ungated))
    assert len(hits) == 1, f'mitigation in an ungated method not caught: {hits}'
    assert hits[0][2] == 'AttackAbility' and hits[0][3] == 'ReprisalPvE', hits

    # The same action inside a gated method is the correct placement and must stay silent.
    gated = ungated.replace('AttackAbility', 'DefenseAreaAbility')
    assert not scan_file('t.cs', strip(gated)), 'false positive inside a gated method'

    # An action that is not a mitigation must not be reported.
    other = ungated.replace('ReprisalPvE', 'GlarePvE')
    assert not scan_file('t.cs', strip(other)), 'false positive on a non-mitigation action'

    # A commented-out call is not a call - strip() has to run first.
    commented = ungated.replace('if (ReprisalPvE.CanUse(out act)) { return true; }',
                                '// if (ReprisalPvE.CanUse(out act)) { return true; }')
    assert not scan_file('t.cs', strip(commented)), 'comment leaked past strip()'

    # Two mitigations in one ungated method must both be reported, not just the first.
    two = ungated.replace('if (ReprisalPvE.CanUse(out act)) { return true; }',
                          'if (ReprisalPvE.CanUse(out act)) { return true; }\n'
                          '        if (RampartPvE.CanUse(out act)) { return true; }')
    assert len(scan_file('t.cs', strip(two))) == 2, 'second mitigation in the same body missed'

    # The body must end at its closing brace: a mitigation in the *next* method is not this one's.
    spill = ungated + '''
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (RampartPvE.CanUse(out act)) { return true; }
        return false;
    }
    '''
    assert len(scan_file('t.cs', strip(spill))) == 1, 'body() ran past the closing brace'

    print('self-test ok: ungated hits found, gated and non-mitigation cases silent, bodies bounded\n')


def main():
    self_test()
    hits = []
    seen = 0
    for dp, _, ns in os.walk('RotationSolver'):
        for n in ns:
            if not n.endswith('.cs'):
                continue
            p = os.path.join(dp, n)
            seen += 1
            hits.extend(scan_file(p, strip(open(p, encoding='utf-8').read())))
    if not seen:
        print('no C# files found - run from the repository root')
        return 1

    for h in hits:
        print(f'{h[0]}:{h[1]}  [{h[2]}] {h[3]}\n      ...{h[4]}')
    print(f'\n{len(hits)} hits across {seen} files')
    return 0


if __name__ == '__main__':
    sys.exit(main())
