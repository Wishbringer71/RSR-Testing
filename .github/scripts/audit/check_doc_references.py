#!/usr/bin/env python3
"""Check that `File.cs:123` references in the documentation still point where they claim.

A line reference is the cheapest way to make a concept verifiable and the first thing to
rot: any edit above the cited line moves it, and nothing fails. The reader follows the
reference, lands somewhere unrelated, and either loses trust in the document or - worse -
believes what the wrong place appears to say.

Checks, in increasing strictness:

  * the file exists and has that many lines, and the cited line carries code - not a blank
    or a lone brace. A miss here is always wrong, and fails.
  * the sentence names a code identifier in backticks. A reference nothing vouches for drifts
    unseen (A270: eleven references in concepts and TODO had drifted onto braces and other
    code), so a reference without one fails too: name what the line holds, or cite the
    method instead of the line.
  * that identifier stands at the cited line (two lines either way), or the cited line lies in
    the body of a member whose declaration names it - BaseAction.cs:257 sits 45 lines below the
    CanUse it belongs to and is right. Anything else is a drifted reference and fails, reported
    with the identifier's real position so the fix is mechanical; and a sentence whose
    identifiers the cited file does not contain at all fails as unverifiable. Until A286 the
    test was "within 25 lines", which let a reference six lines off pass and could not tell
    the two cases apart, so it only reported (C4).
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

# How far from the cited line the identifier may stand when the line is not inside its body: the
# line itself and its immediate neighbours, for a sentence that names the condition on the line
# above the cited statement.
NEAR = 2

# A line that declares a member: modifiers or a type ahead of the name, then a parameter list, an
# expression body, a property block or an initialiser.
DECLARATION = r'^\s*(?:\[[^\]]*\]\s*)*(?:(?:public|private|protected|internal|static|override|virtual|' \
    r'abstract|sealed|async|readonly|partial|unsafe|extern|new|const)\s+)*[\w<>\[\],.?]+\s+%s\s*(?:\(|=>|\{|=|$)'


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


def body_end(lines, start):
    """The last line of the block that opens at or after `start`, by brace count; None without one."""
    depth, opened = 0, False
    for i in range(start, len(lines)):
        for char in lines[i]:
            if char == '{':
                depth += 1
                opened = True
            elif char == '}':
                depth -= 1
                if opened and depth == 0:
                    return i
        # An expression-bodied member ends with its statement.
        if not opened and lines[i].rstrip().endswith(';') and i >= start:
            return i
    return None


def identifiers_near(path, line_no, names):
    """Which of names vouch for line_no, and where each really sits.

    A name vouches when it stands within NEAR lines of the cited line, or when the cited line lies
    in the body of a member whose declaration names it.
    """
    with open(path, encoding='utf-8', errors='replace') as handle:
        lines = handle.readlines()

    near, elsewhere = set(), {}
    low, high = max(0, line_no - 1 - NEAR), min(len(lines), line_no + NEAR)
    for name in names:
        short = name.split('.')[-1]
        if any(re.search(r'\b%s\b' % re.escape(short), lines[i]) for i in range(low, high)):
            near.add(name)
            continue
        declaration = re.compile(DECLARATION % re.escape(short))
        inside = False
        for i, text in enumerate(lines):
            if declaration.search(text):
                end = body_end(lines, i)
                if end is not None and i + 1 <= line_no <= end + 1:
                    inside = True
                    break
        if inside:
            near.add(name)
            continue
        for i, text in enumerate(lines):
            if re.search(r'\b%s\b' % re.escape(short), text):
                elsewhere[name] = i + 1
                break
    return near, elsewhere, len(lines)


# A line worth citing has something on it. These do not: a blank, or a line that carries only
# block punctuation. The review that added this check found four references in TODO.md pointing at
# a closing brace and a blank line after an upstream merge - all four had been right when written,
# and all four sat in the blind spot below, because their sentence names no identifier. Citing a
# line means claiming something stands there, so this needs no identifier to be decidable.
EMPTY_LINE = re.compile(r'^[\s{}()\[\];,]*$')


def cited_line_is_empty(path, line_no):
    """Does the cited line carry nothing but whitespace or block punctuation?"""
    with open(path, encoding='utf-8', errors='replace') as handle:
        lines = handle.readlines()
    if not 1 <= line_no <= len(lines):
        return False, ''
    text = lines[line_no - 1].rstrip('\n')
    return bool(EMPTY_LINE.match(text)), text


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

                # A cited line has to carry something, whether or not the sentence names an
                # identifier: one named a few lines away let a reference to a closing brace pass
                # (04-concept.md cited BaseAction.cs:346 after the code below it had moved).
                empty, content = cited_line_is_empty(target, line_no)
                if empty:
                    hard.append('%s cites %s:%d, but that line is empty%s'
                                % (where, cited, line_no,
                                   '' if not content.strip()
                                   else ' but for %r' % content.strip()))
                    continue

                if not names:
                    unverifiable.append('%s cites %s:%d with no identifier to check it '
                                        'against' % (where, cited, line_no))
                elif not near and elsewhere:
                    first = sorted(elsewhere.items(), key=lambda kv: kv[1])[0]
                    drifted.append('%s cites %s:%d, but %s sits at line %d'
                                   % (where, cited, line_no, first[0], first[1]))
                elif not near:
                    # Nothing the sentence names is in that file: the reference vouches for
                    # nothing there (TODO.md cited publish.yaml:42 for a build line at 94, naming
                    # only csproj properties, and passed - A286, C4).
                    unverifiable.append('%s cites %s:%d, but none of %s occurs in that file'
                                        % (where, cited, line_no, ', '.join(names)))

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

    # A cited line inside the body of the member the sentence names is right however far below the
    # declaration it sits; six lines outside it is a drifted reference (A286, C4).
    with tempfile.TemporaryDirectory() as tmp:
        code = os.path.join(tmp, 'Body.cs')
        body = ['\tpublic bool CanUse(out IAction? act)', '\t{'] + ['\t\tvar x = %d;' % i for i in range(40)] + \
            ['\t\treturn true;', '\t}', '', '\tprivate void Other()', '\t{', '\t\tvar y = 1;', '\t}']
        with open(code, 'w', encoding='utf-8') as handle:
            handle.write('\n'.join(body) + '\n')
        near, _, _ = identifiers_near(code, 30, ['CanUse'])
        if 'CanUse' not in near:
            raise AssertionError('a line inside the named method was not accepted')
        near, elsewhere, _ = identifiers_near(code, 48, ['CanUse'])
        if 'CanUse' in near or elsewhere.get('CanUse') != 1:
            raise AssertionError('a line in the next method was taken as inside CanUse')

        doc = os.path.join(tmp, 'Doc.md')
        with open(doc, 'w', encoding='utf-8') as handle:
            handle.write('`Body.cs:30` liegt in `CanUse`.\n`Body.cs:48` nennt `CanUse`.\n'
                         '`Body.cs:30` nennt nur `Nirgends`.\n')
        global ROOT
        saved, ROOT = ROOT, tmp
        try:
            hard, drifted, unverifiable = check([doc])
        finally:
            ROOT = saved
        if hard or len(drifted) != 1 or len(unverifiable) != 1:
            raise AssertionError('body, drift and absent identifier not told apart: %r %r %r'
                                 % (hard, drifted, unverifiable))

    # A sentence that cites one file and then a second line inside it carries two references,
    # not one, and none of its identifiers can be attributed to either.
    two = '`ChurinSMN.cs:1015` trägt den Befund, `:948` nutzt `BahamutBurst`.'
    if len(REFERENCE.findall(two)) + len(CONTINUATION.findall(two)) != 2:
        raise AssertionError('the `:948` continuation is not counted as a second reference')

    one = 'Die Regel steht in `WHM_Reborn.cs:723` als `ShouldHoldHoly`.'
    if len(REFERENCE.findall(one)) + len(CONTINUATION.findall(one)) != 1:
        raise AssertionError('a single reference was miscounted')

    # The blind spot this check was extended to cover: a sentence naming no identifier used to
    # pass unexamined, and eleven references in the tree pointed at a closing brace or a blank
    # line. A cited line has to carry something.
    with tempfile.TemporaryDirectory() as tmp:
        code = os.path.join(tmp, 'Empty.cs')
        with open(code, 'w', encoding='utf-8') as handle:
            handle.write('void Real() { }\n' + '\n' + '\t}\n' + '\tvar x = 1;\n')

        for line_no, expect_empty in ((1, False), (2, True), (3, True), (4, False)):
            empty, _ = cited_line_is_empty(code, line_no)
            if empty != expect_empty:
                raise AssertionError('line %d judged %s' % (line_no, 'empty' if empty else 'full'))

        # Out of range is the other check's business, not this one's.
        if cited_line_is_empty(code, 99)[0]:
            raise AssertionError('a line past the end must not be reported as empty')

    print('self-test ok: an identifier at the cited line or around it in its body is accepted, one '
          'outside is a drift reported with its real position, a name the file lacks is '
          'unverifiable, and a citation of a blank or brace-only line is caught without one')
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
        print('%d reference(s) that drifted - the named identifier is neither at the cited line nor'
              % len(drifted))
        print('the member whose body holds it:')
        for item in drifted:
            print('  ' + item)
        print()

    if unverifiable:
        # A reference no identifier vouches for drifts unseen; name what the line holds.
        print('%d reference(s) carry no identifier to check them against:' % len(unverifiable))
        for item in unverifiable:
            print('  ' + item)
        print()

    if hard or drifted or unverifiable:
        return 1

    print('Every reference points at a line that exists.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
