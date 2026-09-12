#!/usr/bin/env python3
"""Three defect classes typical for rotation files, each measured at the property itself.

The first version of this script reported by resemblance and was therefore ignored: it
listed 41 pairs of consecutive if-blocks that share a body and 38 level gates, and every
one of them was correct code. A check whose output a reader has to sort by hand is worse
than no check, because the one real finding drowns in it.

What is looked for now:

  (a) an action is tested and its block never uses the result. `CanUse` assigns the action
      and returns whether it may be cast; a block that neither returns nor stores it has
      thrown that answer away, and nothing fails at runtime - the action simply never
      happens.

  (b) a second branch that cannot be reached. Two consecutive if-blocks with the same body
      are idiomatic - they are `if (c1 || c2)` written out - so the body alone proves
      nothing. It becomes a defect when the first block exits and the second condition
      implies the first: the second is then dead. Implication is decided syntactically, by
      splitting both conditions at top-level `&&`: if every conjunct of the first also
      appears in the second, the second is the stricter one and can never be true where the
      first was false.

  (c) a lower-level action gated behind a *higher*-level action being available. The
      negated form `!HolyIii.EnoughLevel && Holy.CanUse` is the ordinary downgrade fallback
      and correct; the un-negated form casts the old action only once the new one is
      already learned, which is the opposite of what a fallback means.
"""

import collections
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))

ROTATION_ROOTS = [
    'RotationSolver/RebornRotations',
    'RotationSolver/ExtraRotations',
    'RotationSolver.Basic/Rotations',
]

# `if (SomeActionPvE.CanUse(out act))` and nothing else on the line.
CANUSE_ONLY = re.compile(
    r'if\s*\((?:[^()]|\([^()]*\))*?(\w+PvE|\w+PvP)\.CanUse\(out (?:act|action)\b[^)]*\)\s*\)\s*$')
IF_LINE = re.compile(r'if\s*\((.+)\)\s*$')
LEVEL_GATE = re.compile(r'(!\s*)?(\w+PvE)\.EnoughLevel\s*&&\s*(\w+PvE)\.CanUse')
EXIT_STATEMENT = re.compile(r'^\s*(?:return|break|continue|throw|goto)\b')


def strip_noise(source):
    """Remove comments and string contents, which otherwise match every pattern here."""
    source = re.sub(r'/\*.*?\*/', '', source, flags=re.S)
    source = re.sub(r'//[^\n]*', '', source)
    return re.sub(r'"(?:\\.|[^"\\])*"', '""', source)


def block_at(lines, i):
    """The lines of the block opened at or after line i, and the index just past it."""
    j = i
    while j < len(lines) and '{' not in lines[j]:
        if lines[j].strip() and j > i:
            return [lines[j]], j + 1        # brace-less single statement
        j += 1
    if j >= len(lines):
        return [], len(lines)

    depth, out = 0, []
    while j < len(lines):
        depth += lines[j].count('{') - lines[j].count('}')
        out.append(lines[j])
        if depth <= 0:
            break
        j += 1
    return out, j + 1


def conjuncts(condition):
    """The condition split at top-level `&&`, as opaque strings.

    Nothing inside a conjunct is interpreted - a `||` stays one indivisible piece - so the
    subset test below stays sound whatever the parts contain.
    """
    parts, depth, current, i = [], 0, '', 0
    while i < len(condition):
        char = condition[i]
        if char == '(':
            depth += 1
        elif char == ')':
            depth -= 1
        if depth == 0 and condition.startswith('&&', i):
            parts.append(current)
            current = ''
            i += 2
            continue
        current += char
        i += 1
    parts.append(current)
    return frozenset(re.sub(r'\s+', '', p) for p in parts if p.strip())


def exits(block):
    """Whether the block's last statement leaves the method, so the next if is only
    reached when the condition was false."""
    for text in reversed(block[:-1]):
        if text.strip():
            return bool(EXIT_STATEMENT.match(text))
    return False


def body_of(block):
    return ' '.join(x.strip() for x in block[1:-1] if x.strip())


def scan_source(source, path, findings, stats):
    lines = strip_noise(source).split('\n')

    for i, line in enumerate(lines):
        statement = line.strip()

        # (a) the CanUse result is discarded
        match = CANUSE_ONLY.match(statement)
        if match:
            block, _ = block_at(lines, i)
            text = ' '.join(block)
            if block and 'return' not in text and 'act =' not in text and 'action =' not in text:
                findings['a_canuse_block_never_returns'].append(
                    '%s:%d: %s | %s' % (path, i + 1, match.group(1), text.strip()[:110]))

        # (b) a second branch the first one already covers
        first = IF_LINE.match(statement)
        if first:
            block1, after = block_at(lines, i)
            if len(block1) >= 2:
                k = after
                while k < len(lines) and not lines[k].strip():
                    k += 1
                second = IF_LINE.match(lines[k].strip()) if k < len(lines) else None
                if second:
                    block2, _ = block_at(lines, k)
                    body1, body2 = body_of(block1), body_of(block2)
                    c1, c2 = conjuncts(first.group(1)), conjuncts(second.group(1))
                    if body1 and body1 == body2 and c1 != c2 and len(body1) > 15:
                        stats['shared_bodies'] += 1
                        if c1 < c2 and exits(block1):
                            findings['b_second_branch_unreachable'].append(
                                '%s:%d/%d: %s | %s is implied by %s'
                                % (path, i + 1, k + 1, body1[:60],
                                   ' && '.join(sorted(c2))[:60], ' && '.join(sorted(c1))[:60]))

        # (c) an action gated behind a higher-level one being available
        for gate in LEVEL_GATE.finditer(line):
            stats['level_gates'] += 1
            if gate.group(1) or gate.group(2) == gate.group(3):
                continue                     # the negated form is the ordinary fallback
            findings['c_level_gate_other_action'].append(
                '%s:%d: %s.EnoughLevel gates %s | %s'
                % (path, i + 1, gate.group(2), gate.group(3), line.strip()[:100]))


def rotation_files():
    for entry in ROTATION_ROOTS:
        for base, _, names in os.walk(os.path.join(ROOT, entry)):
            for name in sorted(names):
                if name.endswith('.cs'):
                    yield os.path.join(base, name)


def self_test():
    """Constructed defects against constructed correct code, for all three patterns.

    Without this a silent "nothing found" cannot be told from a broken pattern, and two of
    the three patterns here were broken in exactly that way: they matched the correct form.
    """
    defective = '\n'.join([
        'if (FesterPvE.CanUse(out act))',                     # (a) result discarded
        '{',
        '    _somethingElse = true;',
        '}',
        'if (HasBuff)',                                       # (b) second is stricter
        '{',
        '    return base.GeneralGCD(out act);',
        '}',
        'if (HasBuff && InBurst)',
        '{',
        '    return base.GeneralGCD(out act);',
        '}',
        'if (HolyIiiPvE.EnoughLevel && HolyPvE.CanUse(out act))',   # (c) un-negated gate
        '{',
        '    return true;',
        '}',
    ])
    correct = '\n'.join([
        'if (FesterPvE.CanUse(out act))',                     # result is used
        '{',
        '    return true;',
        '}',
        'if (HasBuff && InBurst)',                            # stricter one comes first
        '{',
        '    return base.GeneralGCD(out act);',
        '}',
        'if (HasBuff)',
        '{',
        '    return base.GeneralGCD(out act);',
        '}',
        'if (InBurst)',                                       # first falls through
        '{',
        '    act = SomeStoredAction;',
        '}',
        'if (InBurst && HasBuff)',
        '{',
        '    act = SomeStoredAction;',
        '}',
        'if (!HolyIiiPvE.EnoughLevel && HolyPvE.CanUse(out act))',  # downgrade fallback
        '{',
        '    return true;',
        '}',
        '// if (HolyIiiPvE.EnoughLevel && HolyPvE.CanUse(out act)) in a comment',
    ])

    found, stats = collections.defaultdict(list), collections.Counter()
    scan_source(defective, 'defect', found, stats)
    for key in ('a_canuse_block_never_returns', 'b_second_branch_unreachable',
                'c_level_gate_other_action'):
        if len(found[key]) != 1:
            raise AssertionError('%s did not catch its constructed defect: %r'
                                 % (key, found[key]))

    found, stats = collections.defaultdict(list), collections.Counter()
    scan_source(correct, 'correct', found, stats)
    if any(found.values()):
        raise AssertionError('correct code was reported: %r' % dict(found))
    if stats['shared_bodies'] != 2:
        raise AssertionError('the idiomatic shared bodies were not counted: %d'
                             % stats['shared_bodies'])

    if conjuncts('A && (B || C) && D') != frozenset(['A', '(B||C)', 'D']):
        raise AssertionError('top-level split is wrong')
    if conjuncts('A || B') != frozenset(['A||B']):
        raise AssertionError('a disjunction must stay one indivisible conjunct')

    print('self-test ok: three constructed defects found, the correct forms of all three '
          'left alone')
    print()


def main():
    self_test()

    findings, stats = collections.defaultdict(list), collections.Counter()
    paths = list(rotation_files())
    for path in paths:
        with open(path, encoding='utf-8', errors='replace') as handle:
            scan_source(handle.read(), os.path.relpath(path, ROOT), findings, stats)

    for key in sorted(findings):
        print('== %s (%d)' % (key, len(findings[key])))
        for item in findings[key]:
            print('  ' + item)
        print()

    print('%d rotation file(s) scanned.' % len(paths))
    print('%d level gate(s) examined, %d pair(s) of consecutive if-blocks share a body '
          'without one implying the other - both forms are idiomatic and not reported.'
          % (stats['level_gates'], stats['shared_bodies']))

    if findings:
        return 1

    print('No discarded CanUse result, no unreachable duplicate branch, no inverted level '
          'gate.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
