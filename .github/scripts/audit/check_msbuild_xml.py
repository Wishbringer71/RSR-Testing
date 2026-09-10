#!/usr/bin/env python3
"""Are the MSBuild files well-formed XML?

A malformed `Directory.Build.props` fails every project in the tree at import time, before a single
line is compiled - and it costs a full Windows build to find out. The failure that prompted this
check was a comment containing `--`, which XML forbids: the file held a git command line in prose,
`git describe --tags --abbrev=0`, and the build died with MSB4024 on the SDK's own props import.

The check parses every MSBuild file with an XML parser. That is exactly what MSBuild does first, so
a file that parses here cannot fail there for this reason. It runs in the Linux job, in under a
second, where the equivalent finding otherwise costs a minute of Windows runner.

It does not validate MSBuild semantics - a well-formed file can still be wrong. The class it closes
is the one where nothing compiles at all.

Usage: python3 .github/scripts/audit/check_msbuild_xml.py
"""
import os
import sys
import xml.etree.ElementTree as ET

SUFFIXES = ('.props', '.targets', '.csproj', '.sln.props')
SKIP_DIRS = {'.git', 'obj', 'bin', 'node_modules', '__pycache__'}


def files():
    for root, dirs, names in os.walk('.'):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        for name in names:
            if name.endswith(SUFFIXES):
                yield os.path.join(root, name)[2:]


def self_test():
    import tempfile
    good = '<Project><PropertyGroup><Version>1.0</Version></PropertyGroup></Project>'
    # The exact shape that broke the build: a double hyphen inside a comment.
    bad = '<Project><!-- git describe --abbrev=0 --><PropertyGroup /></Project>'
    fd, path = tempfile.mkstemp(suffix='.props')
    try:
        with os.fdopen(fd, 'w', encoding='utf-8') as fh:
            fh.write(good)
        ET.parse(path)
        with open(path, 'w', encoding='utf-8') as fh:
            fh.write(bad)
        try:
            ET.parse(path)
        except ET.ParseError:
            pass
        else:
            raise AssertionError('a comment containing "--" has to fail the parser')
    finally:
        os.unlink(path)
    print('self-test ok: well-formed accepted, "--" inside a comment rejected\n')


def main():
    self_test()
    checked, bad = 0, []
    for path in sorted(files()):
        checked += 1
        try:
            ET.parse(path)
        except ET.ParseError as err:
            bad.append(f'  {path}: {err}')

    print(f'{checked} MSBuild file(s) parsed.')
    if bad:
        print('\n'.join(bad))
        print('\nMSBuild imports these before compiling anything, so a parse error here fails '
              'every\nproject in the tree. Note that XML forbids "--" inside a comment.')
        return 1
    print('All well-formed.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
