#!/usr/bin/env python3
"""Phase 7: public surface of RotationSolver.Basic removed or changed since a base revision.

RotationSolver.Basic ships as a NuGet package (GeneratePackageOnBuild), so authors of derived
rotations compile against its public and protected members. Removing one, renaming it or changing its
parameter list breaks their build; Semantic Versioning calls that a major change, and the project has
no deprecation path for it.

The relevant base is the last released fork tag, not upstream/main: the packages people compiled
against are the ones this fork published.

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
TYPE_DECL = re.compile(r'\b(?:class|struct|interface|enum|record)\b')
NOT_A_MEMBER = {'if', 'while', 'for', 'foreach', 'switch', 'catch', 'lock', 'return', 'using'}


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
        for line in text.split('\n'):
            m = MEMBER.match(line)
            if not m or m.group('name') in NOT_A_MEMBER or TYPE_DECL.search(line):
                continue
            key = f'{m.group("name")}'
            if m.group('tail') == '(':
                # Keep the parameter count so a changed signature is not read as an intact member.
                params = line.split('(', 1)[1]
                arity = 0 if params.strip().startswith(')') else params.count(',') + 1
                key = f'{key}/{arity}'
            found.setdefault(key, (path, line.strip()))
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
            key = m.group('name')
            if m.group('tail') == '(':
                params = line.split('(', 1)[1]
                key += '/' + str(0 if params.strip().startswith(')') else params.count(',') + 1)
            got[key] = line
    assert set(got) == {'Helper/2', 'Duration', 'NoArgs/0'}, sorted(got)
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
