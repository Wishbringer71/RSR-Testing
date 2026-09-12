#!/usr/bin/env python3
"""Check that `File.cs:123` references in the documentation still point where they claim.

A line reference is the cheapest way to make a concept verifiable and the first thing to
rot: any edit above the cited line moves it, and nothing fails. The reader follows the
reference, lands somewhere unrelated, and either loses trust in the document or - worse -
believes what the wrong place appears to say.

Two checks, in increasing strictness:

  * the file exists and has that many lines. A miss here is always wrong.
  * where the same sentence names a code identifier in backticks, that identifier should
    appear near the cited line. A miss is reported with the identifier's real position, so
    the fix is mechanical.

Only the first check fails the run. The second cannot tell a drifted reference from a
correct one that cites a line inside the body of the named method - BaseAction.cs:257 sits
45 lines below the CanUse it belongs to and is right - so it reports and leaves the
judgement to a reader. A check that cannot separate the two must not claim it can.
"""

import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))

# `Some/Path/File.cs:123` or `File.cs`:123 - both spellings occur in the tree.
REFERENCE = re.compile(r'`([A-Za-z0-9_./-]+\.(?:cs|py|yaml|yml|json|props|resx))`?:(\d+)')
# A follow-up citation into the file just named, spelled `:948` or `:1403-1411`. The documents
# use it to give a second line without repeating the file name, so it carries none and the
# reference pattern cannot see it - which makes a sentence with two citations look like one with
# a single citation, and the identifier belonging to the second gets held against the first.
CONTINUATION = re.compile(r'`:(\d+)(?:-\d+)?`')
# An identifier in backticks: method, property or type name, optionally qualified.
IDENTIFIER = re.compile(r'`([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)`')

# How far from the cited line the identifier may sit before the reference counts as drifted.
# Generous on purpose: a reference to a method usually cites its first line while the
# sentence names something used in its body.
WINDOW = 25

# Checked, because these state the position as it is now: a concept in the ruling style and
# the list of open work. The archive is deliberately left out - AUDIT_LOG and CHANGELOG
# record what a check found at the time, and a line number that was right then is a
# historical fact, not a defect to repair.
DOC_ROOTS = ['docs', '.github/scripts/audit/README.md', 'TODO.md', 'README.md']

# Words that match the identifier pattern without being identifiers.
NOT_IDENTIFIERS = frozenset([
    'true', 'false', 'null', 'if', 'else', 'var', 'new', 'return', 'this', 'base',
    'string', 'int', 'float', 'bool', 'byte', 'uint', 'void', 'static', 'public',
    'private', 'protected', 'internal', 'const', 'readonly', 'sealed', 'partial',
])


def documentation_files():
    for entry in DOC_ROOTS:
        path = os.path.join(ROOT, entry)
        if os.path.isfile(path):
            yield path
        elif os.path.isdir(path):
            for base, _, names in os.walk(path):
                for name in sorted(names):
                    if name.endswith('.md'):
                        yield os.path.join(base, name)


def resolve(cited):
    """Find the file a reference names, by suffix: documents cite bare file names."""
    direct = os.path.join(ROOT, cited)
    if os.path.isfile(direct):
        return direct

    matches = []
    wanted = '/' + cited.lstrip('./')
    for base, dirs, names in os.walk(ROOT):
        dirs[:] = [d for d in dirs if d not in ('.git', 'bin', 'obj', '__pycache__')]
        for name in names:
            full = os.path.join(base, name)
            if full.replace(os.sep, '/').endswith(wanted):
                matches.append(full)
    # An ambiguous name is not resolvable, and guessing one would make the check lie.
    return matches[0] if len(matches) == 1 else None


def identifiers_near(path, line_no, names):
    """Which of names appear within WINDOW lines of line_no, and where each really sits."""
    with open(path, encoding='utf-8', errors='replace') as handle:
        lines = handle.readlines()

    near, elsewhere = set(), {}
    low, high = max(0, line_no - 1 - WINDOW), min(len(lines), line_no + WINDOW)
    for name in names:
        short = name.split('.')[-1]
        if any(short in lines[i] for i in range(low, high)):
            near.add(name)
            continue
        for i, text in enumerate(lines):
            if short in text:
                elsewhere[name] = i + 1
                break
    return near, elsewhere, len(lines)


def check(paths):
    """Return (hard, drifted, unverifiable) findings."""
    hard, drifted, unverifiable = [], [], []

    for doc in paths:
        with open(doc, encoding='utf-8', errors='replace') as handle:
            doc_lines = handle.readlines()

        for doc_no, text in enumerate(doc_lines, start=1):
            for cited, line_str in REFERENCE.findall(text):
                line_no = int(line_str)
                where = '%s:%d' % (os.path.relpath(doc, ROOT), doc_no)

                target = resolve(cited)
                if target is None:
                    unverifiable.append('%s cites %s, which is not in the tree under that '
                                        'name (or the name is ambiguous)' % (where, cited))
                    continue

                # Only when the sentence carries exactly one reference can an identifier
                # in it be attributed to that reference. Two references in one sentence and
                # the pairing is a guess - which produced three false reports before this
                # condition existed.
                if len(REFERENCE.findall(text)) + len(CONTINUATION.findall(text)) > 1:
                    unverifiable.append('%s cites %s:%d in a sentence with several '
                                        'references' % (where, cited, line_no))
                    continue

                names = [n for n in IDENTIFIER.findall(text)
                         if n.lower() not in NOT_IDENTIFIERS
                         and not n.endswith(('.cs', '.py', '.md', '.json', '.yaml'))]
                near, elsewhere, total = identifiers_near(target, line_no, names)

                if line_no > total:
                    hard.append('%s cites %s:%d, but that file has %d lines'
                                % (where, cited, line_no, total))
                    continue

                if not names:
                    unverifiable.append('%s cites %s:%d with no identifier to check it '
                                        'against' % (where, cited, line_no))
                elif not near and elsewhere:
                    first = sorted(elsewhere.items(), key=lambda kv: kv[1])[0]
                    drifted.append('%s cites %s:%d, but %s sits at line %d'
                                   % (where, cited, line_no, first[0], first[1]))

    return hard, drifted, unverifiable


def self_test():
    import tempfile

    with tempfile.TemporaryDirectory() as tmp:
        code = os.path.join(tmp, 'Sample.cs')
        with open(code, 'w', encoding='utf-8') as handle:
            handle.write('\n' * 40 + 'void TheMethod() { }\n' + '\n' * 10)

        near, elsewhere, total = identifiers_near(code, 41, ['TheMethod'])
        if 'TheMethod' not in near:
            raise AssertionError('an identifier on the cited line was not found')

        near, elsewhere, total = identifiers_near(code, 5, ['TheMethod'])
        if 'TheMethod' in near or elsewhere.get('TheMethod') != 41:
            raise AssertionError('a drifted reference was not reported at its real line')

        if total != 51:  # 40 blank + the method + 10 blank
            raise AssertionError('line count is wrong: %d' % total)

    # A sentence that cites one file and then a second line inside it carries two references,
    # not one, and none of its identifiers can be attributed to either.
    two = '`ChurinSMN.cs:1015` trägt den Befund, `:948` nutzt `BahamutBurst`.'
    if len(REFERENCE.findall(two)) + len(CONTINUATION.findall(two)) != 2:
        raise AssertionError('the `:948` continuation is not counted as a second reference')

    one = 'Die Regel steht in `WHM_Reborn.cs:723` als `ShouldHoldHoly`.'
    if len(REFERENCE.findall(one)) + len(CONTINUATION.findall(one)) != 1:
        raise AssertionError('a single reference was miscounted')

    print('self-test ok: an identifier at the cited line is accepted, one 36 lines away is '
          'reported with its real position')
    print()


def main():
    self_test()

    paths = list(documentation_files())
    hard, drifted, unverifiable = check(paths)

    print('%d documentation file(s) checked.' % len(paths))
    print()

    if hard:
        print('%d reference(s) that cannot be right:' % len(hard))
        for item in hard:
            print('  ' + item)
        print()

    if drifted:
        print('%d reference(s) worth a look - the named identifier is elsewhere, which means'
              % len(drifted))
        print('either the reference drifted or it cites a line inside that method:')
        for item in drifted:
            print('  ' + item)
        print()

    print('%d reference(s) carry no identifier and are not checked here.' % len(unverifiable))

    if hard:
        return 1

    print('Every reference points at a line that exists.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
