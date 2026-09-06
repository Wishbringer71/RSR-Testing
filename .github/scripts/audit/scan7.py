#!/usr/bin/env python3
"""Phase 7: public surface of RotationSolver.Basic removed or changed since a base revision.

RotationSolver.Basic ships as a NuGet package (GeneratePackageOnBuild), so authors of derived
rotations compile against its public and protected members. Removing one, renaming it or changing its
parameter list breaks their build; Semantic Versioning calls that a major change, and the project has
no deprecation path for it.

The relevant base is the last released fork tag, not upstream/main: the packages people compiled
against are the ones this fork published.

Members are keyed by declaring type, and interface members are read as well - they carry no
visibility modifier of their own unless they are explicitly internal, which takes them back out of
the surface.

Usage: python3 .github/scripts/audit/scan7.py [base-ref]   (default: the newest fork tag)
"""
import re
import subprocess
import sys

PROJECT = 'RotationSolver.Basic/'

# Public surface: a member declared public or protected. "protected" counts because derived rotations
# inherit CustomRotation and call into exactly those members.
MEMBER = re.compile(
    r'^\s*(?:public|protected)(?:\s+(?:static|virtual|abstract|override|sealed|readonly|const|'
    r'partial|unsafe|new|async))*\s+'
    r'(?P<type>[\w<>,\.\s\[\]?]+?)\s+(?P<name>\w+)\s*(?P<tail>\(|\{|=>|=|;)'
)
# Inside an interface every member is public and carries no visibility modifier of its own, so the
# pattern above cannot see them. ICustomRotation is exactly such a case: dropping a member from it
# breaks any rotation that implements the interface instead of deriving from CustomRotation.
IFACE_MEMBER = re.compile(
    r'^\s*(?:(?:static|abstract|virtual|sealed|new|unsafe|public)\s+)*'
    r'(?P<type>[\w<>,\.\s\[\]?]+?)\s+(?P<name>\w+)\s*(?P<tail>\(|\{|=>|;)'
)
IFACE_DECL = re.compile(r'\binterface\s+\w+')
# C# 8 allows access modifiers on interface members. One declared internal is invisible outside the
# assembly, so it is not part of what a consumer compiles against - ICustomRotation holds such
# members and they must not be counted as package surface.
IFACE_NOT_PUBLIC = re.compile(r'^\s*(?:internal|private|protected)\b')
TYPE_NAME = re.compile(r'\b(?:class|struct|interface|record)\s+(\w+)')
TYPE_DECL = re.compile(r'\b(?:class|struct|interface|enum|record)\b')
NOT_A_MEMBER = {'if', 'while', 'for', 'foreach', 'switch', 'catch', 'lock', 'return', 'using'}


def _key(name, tail, line):
    if tail != '(':
        return name
    # Keep the parameter count so a changed signature is not read as an intact member.
    params = line.split('(', 1)[1]
    arity = 0 if params.strip().startswith(')') else params.count(',') + 1
    return f'{name}/{arity}'


def surface(revision=None):
    """Return {"file::name(params)": declaring line} for the project's public and protected members."""
    listing = subprocess.run(
        ['git', 'ls-files'] if revision is None
        else ['git', 'ls-tree', '-r', '--name-only', revision],
        capture_output=True, text=True, check=True).stdout.split()
    files = [f for f in listing if f.startswith(PROJECT) and f.endswith('.cs')]
    if not files:
        raise RuntimeError(f'no {PROJECT} sources found in {revision or "the working tree"}')

    found = {}
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
        in_interface = False
        depth = 0
        # The declaring type is part of the key: two same-named members in different types are two
        # contracts, and keying on the bare name would let the removal of one hide behind the other.
        # A partial type keeps one name across its files, which is what a consumer sees.
        owner = '?'
        for raw in text.split('\n'):
            # Strip line comments before looking for a type: a prose sentence mentioning "struct" or
            # "class" would otherwise be read as a declaration and mis-own every member below it,
            # and a comment added on one side of the comparison alone turns that into 55 phantom
            # removals - which is how this was found.
            line = raw.split('//', 1)[0] if raw.lstrip().startswith('//') else raw
            named = TYPE_NAME.search(line)
            if named:
                owner = named.group(1)
            if in_interface:
                depth += line.count('{') - line.count('}')
                if depth <= 0:
                    in_interface = False
                m = IFACE_MEMBER.match(line)
                if (m and m.group('name') not in NOT_A_MEMBER and not TYPE_DECL.search(line)
                        and not IFACE_NOT_PUBLIC.match(line)):
                    found.setdefault(f'{owner}.{_key(m.group("name"), m.group("tail"), line)}',
                                     (path, line.strip()))
                continue
            if IFACE_DECL.search(line):
                in_interface = True
                depth = line.count('{') - line.count('}')
                continue
            m = MEMBER.match(line)
            if not m or m.group('name') in NOT_A_MEMBER or TYPE_DECL.search(line):
                continue
            found.setdefault(f'{owner}.{_key(m.group("name"), m.group("tail"), line)}',
                             (path, line.strip()))
    return found


def newest_tag():
    out = subprocess.run(['git', 'tag', '--sort=-v:refname'],
                         capture_output=True, text=True, check=True).stdout.split()
    return out[0] if out else 'upstream/main'


def self_test():
    src = [
        'public sealed class Thing',
        '    public static bool Helper(float a, float b)',
        '    protected float Duration => 10f;',
        '    private int Hidden { get; set; }',
        '    public void NoArgs()',
    ]
    got = {}
    for line in src:
        m = MEMBER.match(line)
        if m and m.group('name') not in NOT_A_MEMBER and not TYPE_DECL.search(line):
            got[_key(m.group('name'), m.group('tail'), line)] = line
    assert set(got) == {'Helper/2', 'Duration', 'NoArgs/0'}, sorted(got)

    # An interface member carries no visibility modifier of its own, so MEMBER cannot see it, while
    # one marked internal is not package surface at all. ICustomRotation holds both kinds, and
    # counting the internal ones would have reported a break where a consumer sees nothing.
    iface = ['public interface ICustomRotation', '{', '    bool HasFlag { get; }',
             '    internal bool NotSurface { get; }',
             '    bool Ability(IAction next, out IAction? act);', '}']
    seen = {}
    inside, depth = False, 0
    for line in iface:
        if inside:
            depth += line.count('{') - line.count('}')
            if depth <= 0:
                inside = False
            m = IFACE_MEMBER.match(line)
            if (m and m.group('name') not in NOT_A_MEMBER and not TYPE_DECL.search(line)
                    and not IFACE_NOT_PUBLIC.match(line)):
                seen[_key(m.group('name'), m.group('tail'), line)] = line
            continue
        if IFACE_DECL.search(line):
            inside = True
            depth = line.count('{') - line.count('}')
    assert set(seen) == {'HasFlag', 'Ability/2'}, sorted(seen)
    # The class declaration must not be read as a member, and a private one must stay out.
    assert 'Thing' not in got and 'Hidden' not in got, sorted(got)

    try:
        live = surface()
    except (subprocess.CalledProcessError, RuntimeError):
        live = None
    if live is not None:
        assert len(live) > 200, f'working tree yielded only {len(live)} members'
    print('self-test ok: arity kept, private members and type declarations excluded\n')


if __name__ == '__main__':
    self_test()
    base_ref = sys.argv[1] if len(sys.argv) > 1 else newest_tag()
    try:
        before = surface(base_ref)
    except (subprocess.CalledProcessError, RuntimeError) as err:
        print(f'cannot read {base_ref}: {err}')
        sys.exit(1)

    now = surface()
    gone = sorted(set(before) - set(now))
    print(f'== base {base_ref}: {len(before)} members, working tree: {len(now)}')
    print(f'== removed or changed: {len(gone)}')
    for key in gone:
        path, line = before[key]
        print(f'  {path}  {line}')
