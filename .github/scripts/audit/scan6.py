#!/usr/bin/env python3
"""Phase 6: enum members whose ordinal value moved relative to the base revision.

Configs is an IPluginConfiguration and Dalamud serialises it without a StringEnumConverter, so every
enum-typed setting is stored in the user's configuration as a plain number. OtherConfiguration writes
the same way (Configuration/OtherConfiguration.cs:439-441; the read side adds a StringEnumConverter,
which accepts numbers too). Renumbering a member of such an enum therefore silently reinterprets
settings that are already on disk: the value stays, its meaning changes.

The scan compares every enum in the tree against the base revision and reports members whose ordinal
moved. Enums that are reachable from a persisted configuration are reported as contract breaks, the
rest as informational - a purely in-memory enum may be renumbered freely, which is why the SpecialMode
change in this fork stayed harmless.

Usage: python3 .github/scripts/audit/scan6.py [base-ref]   (default: upstream/main)
"""
import os
import re
import subprocess
import sys

BASE = 'upstream/main'

ENUM_HEAD = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)?\s*enum\s+(\w+)')
MEMBER = re.compile(r'^\s*(\w+)\s*(?:=\s*([^,]+?))?\s*,?\s*$')
SKIP = re.compile(r'^\s*(//|/\*|\*|\[|#|$)')
# Any member declaration, whatever its visibility. Private fields count: the [JobConfig] and
# [ConditionBool] attributes drive a source generator that turns "private readonly TargetHostileType
# _hostileType" into a public per-job dictionary, so the enum behind such a field is persisted even
# though the field itself is not. Over-collecting only costs a closer look; under-collecting would
# let a contract break pass as an in-memory rename.
CONFIG_TYPE = re.compile(
    r'\b(?:public|internal|private|protected)\s+(?:(?:static|readonly|const)\s+)*'
    r'([\w<>,\s\[\]?]+?)\s+\w+\s*(?:\{|=>|=|;)'
)
TYPE_BODY = re.compile(
    r'\b(?:class|struct|record)\s+(\w+)'
)

CONFIG_FILES = ('RotationSolver.Basic/Configuration/Configs.cs',
                'RotationSolver.Basic/Configuration/OtherConfiguration.cs')


def parse_enums(text):
    """Return {enum name: [(member, ordinal), ...]} for one C# source text."""
    enums = {}
    lines = text.split('\n')
    i = 0
    while i < len(lines):
        head = ENUM_HEAD.match(lines[i])
        if not head:
            i += 1
            continue
        name = head.group(1)
        # Advance to the opening brace, which may sit on the declaration line or the next one.
        j = i
        while j < len(lines) and '{' not in lines[j]:
            j += 1
        members, value, depth = [], 0, 0
        j += 1
        while j < len(lines):
            line = lines[j]
            if '}' in line and depth == 0:
                break
            if SKIP.match(line):
                j += 1
                continue
            m = MEMBER.match(line)
            if m:
                if m.group(2) is not None:
                    literal = m.group(2).strip()
                    try:
                        value = int(literal, 0)
                    except ValueError:
                        # Composite flag values (A | B) carry no ordinal of their own to compare.
                        value = None
                if value is not None:
                    members.append((m.group(1), value))
                    value += 1
            j += 1
        enums[name] = members
        i = j + 1
    return enums


def tree_enums(revision=None):
    """Parse every .cs file, either from the working tree or from a git revision."""
    # ls-tree does not take a glob pathspec the way ls-files does - passing "*.cs" there returns an
    # empty list, which reads exactly like a clean comparison. Filter in Python instead.
    listing = subprocess.run(
        ['git', 'ls-files'] if revision is None
        else ['git', 'ls-tree', '-r', '--name-only', revision],
        capture_output=True, text=True, check=True).stdout.split()
    files = [f for f in listing if f.endswith('.cs')]
    if not files:
        raise RuntimeError(f'no C# files found in {revision or "the working tree"}')
    out = {}
    for path in files:
        if revision is None:
            try:
                text = open(path, encoding='utf-8', errors='replace').read()
            except OSError:
                continue
        else:
            got = subprocess.run(['git', 'show', f'{revision}:{path}'],
                                 capture_output=True, text=True)
            if got.returncode:
                continue
            text = got.stdout
        for name, members in parse_enums(text).items():
            out.setdefault(name, (path, members))
    return out


def type_declarations():
    """Return {type name: source text} for every class, struct and record in the tree."""
    bodies = {}
    listing = subprocess.run(['git', 'ls-files'], capture_output=True, text=True,
                             check=True).stdout.split()
    for path in (f for f in listing if f.endswith('.cs')):
        try:
            text = open(path, encoding='utf-8', errors='replace').read()
        except OSError:
            continue
        for m in TYPE_BODY.finditer(text):
            bodies.setdefault(m.group(1), text)
    return bodies


def member_types(text):
    names = set()
    for m in CONFIG_TYPE.finditer(text):
        names.update(re.findall(r'\w+', m.group(1)))
    return names


def persisted_enum_names():
    """Type names reachable from a stored setting, following nested types to a fixed point.

    A setting whose type is a class of its own carries that class's members into the same file, so
    stopping at the two configuration files would miss any enum one level down.
    """
    names = set()
    for path in CONFIG_FILES:
        try:
            names |= member_types(open(path, encoding='utf-8', errors='replace').read())
        except OSError:
            continue
    bodies = type_declarations()
    pending = list(names)
    while pending:
        name = pending.pop()
        body = bodies.get(name)
        if body is None:
            continue
        for found in member_types(body) - names:
            names.add(found)
            pending.append(found)
    return names


def compare(base_enums, head_enums, persisted):
    breaks, informational = [], []
    for name, (path, members) in sorted(head_enums.items()):
        if name not in base_enums:
            continue
        before = dict(base_enums[name][1])
        for member, ordinal in members:
            if member in before and before[member] != ordinal:
                row = (name, member, before[member], ordinal, path)
                (breaks if name in persisted else informational).append(row)
    return breaks, informational


def self_test():
    src = '\n'.join([
        'public enum Colour',
        '{',
        '    Red,',
        '    // a comment',
        '    Green = 4,',
        '    Blue,',
        '}',
    ])
    assert parse_enums(src) == {'Colour': [('Red', 0), ('Green', 4), ('Blue', 5)]}, parse_enums(src)

    moved = src.replace('    Red,\n', '    Red,\n    Amber,\n')
    base = {'Colour': ('x.cs', parse_enums(src)['Colour'])}
    head = {'Colour': ('x.cs', parse_enums(moved)['Colour'])}
    # Amber is inserted before Green, but Green carries an explicit 4, so only Blue moves.
    breaks, info = compare(base, head, {'Colour'})
    assert [(r[1], r[2], r[3]) for r in breaks] == [], breaks
    assert parse_enums(moved)['Colour'] == [('Red', 0), ('Amber', 1), ('Green', 4), ('Blue', 5)]

    implicit = src.replace('    Green = 4,', '    Green,')
    shifted = implicit.replace('    Red,\n', '    Red,\n    Amber,\n')
    base = {'Colour': ('x.cs', parse_enums(implicit)['Colour'])}
    head = {'Colour': ('x.cs', parse_enums(shifted)['Colour'])}
    breaks, info = compare(base, head, {'Colour'})
    assert [(r[1], r[2], r[3]) for r in breaks] == [('Green', 1, 2), ('Blue', 2, 3)], breaks
    breaks, info = compare(base, head, set())
    assert not breaks and len(info) == 2, (breaks, info)

    # A base revision that yields nothing reads like a clean comparison, so both sides are checked
    # for plausibility instead. The first version of this scan reported zero findings for exactly
    # that reason: its ls-tree pathspec matched no file at all.
    for revision in (None, BASE):
        try:
            found = tree_enums(revision)
        except (subprocess.CalledProcessError, RuntimeError):
            continue  # the remote may not be fetched; the caller reports that separately
        assert len(found) > 20, f'{revision or "working tree"} yielded only {len(found)} enums'

    # TargetHostileType reaches the stored configuration only through the [JobConfig] source
    # generator, from a private readonly field. The first version of this scan looked at public
    # members alone and classified it as in-memory, which is the misclassification that would let a
    # renumbering of the engage setting pass unnoticed.
    if os.path.exists(CONFIG_FILES[0]):
        assert 'TargetHostileType' in persisted_enum_names(), \
            'generator-backed private config fields are no longer recognised'
    print('self-test ok: ordinals tracked, enums separated, both revisions non-empty\n')


if __name__ == '__main__':
    self_test()
    base_ref = sys.argv[1] if len(sys.argv) > 1 else BASE
    try:
        base_enums = tree_enums(base_ref)
    except subprocess.CalledProcessError:
        print(f'cannot read {base_ref}; fetch the upstream remote first')
        sys.exit(1)

    head_enums = tree_enums()
    persisted = persisted_enum_names()
    breaks, informational = compare(base_enums, head_enums, persisted)

    print(f'== contract breaks: {len(breaks)} members of a persisted enum changed ordinal')
    for name, member, was, now, path in breaks:
        print(f'  {path}  {name}.{member}: {was} -> {now}')
    print()
    print(f'== informational: {len(informational)} members of an in-memory enum changed ordinal')
    for name, member, was, now, path in informational:
        print(f'  {path}  {name}.{member}: {was} -> {now}')
