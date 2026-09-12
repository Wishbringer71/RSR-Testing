#!/usr/bin/env python3
"""Phase 15: code references in the documents that no longer point at what they name.

The concept documents cite the tree by file and line - `WHM_Reborn.cs:566`, `StatusHelper.cs:781` -
and every commit that touches those files moves the target without touching the citation. The
citation still looks right, so a reader follows it and lands on unrelated code, or worse, on code
that plausibly reads like the subject. That is the same *Lack of Movement* aging the status lists
have, one layer up: correct when written, silently wrong after the next edit elsewhere.

The scan pairs each `<file>.cs:<line>` citation with the backticked identifiers standing near it in
the same paragraph or table row, and asks whether any of them is actually within a few lines of the
cited position. Three outcomes:

  * `ok`        - an identifier from the sentence sits within the window. The citation still holds.
  * `moved`     - the identifier is in the file, but elsewhere. The line number aged; the scan
                  reports where it went.
  * `gone`      - none of the identifiers is in the file at all. Either the code was removed or the
                  citation was wrong to begin with; both need a reader.
  * `no anchor` - the citation names no identifier the scan can check. Not a finding, reported
                  separately so the coverage of the run is visible.

A missing file is always a finding.

The identifier is the anchor rather than the line number itself, because a line number alone cannot
be verified - any line exists. What can be verified is that the thing the sentence talks about is
where the sentence says it is.

Usage: python3 .github/scripts/audit/scan15.py [--window N] [--all]
       --window  how many lines around the citation still count as "there" (default 12)
       --all     also list the `ok` and `no anchor` citations, not just the findings
"""
import os
import re
import sys

DOC_DIRS = ['docs', '.github/scripts/audit']
DOC_FILES = ['CLAUDE.md', 'AUDIT_LOG.md', 'TODO.md', 'CHANGELOG.md', 'README.md']
# The archive records what was true when a check was made. A line number that has since moved is
# not a defect there - it dates the finding. Scanned and counted, but reported apart from the
# documents that claim to describe the current state.
ARCHIVE = {'AUDIT_LOG.md'}

# `Something.cs:123` or `Something.cs:123-456`, with or without the backticks around it.
CITE = re.compile(r'(?P<file>[A-Za-z0-9_.]+\.(?:cs|py|yaml|yml|resx|props|json)):(?P<line>\d+)'
                  r'(?:-(?P<end>\d+))?')
# `:286` or `:1403-1411` - a second line in the file just cited, written without repeating the
# name. It carries no file, so CITE cannot see it, yet it bounds a segment exactly like a full
# citation: the identifier after it belongs to it, not to the citation before.
CONTINUATION = re.compile(r'`:(\d+)(?:-\d+)?`')
# Backticked identifiers: C# members, types, options. Dotted names are split on the dot.
IDENT = re.compile(r'`([^`]+)`')
# A backticked file name with no line number - `CustomRotation_Invoke.cs`. Not an identifier: the
# sentence names the file it lives in, and holding that name against the *other* file cited beside
# it reads as "the code is gone".
FILENAME = re.compile(r'^[A-Za-z0-9_./-]+\.(?:cs|py|yaml|yml|resx|props|json|md)$')
WORD = re.compile(r'[A-Za-z_][A-Za-z0-9_]{3,}')
DEFAULT_WINDOW = 12


def repo_files():
    """{basename: [paths]} for every file the tree can be cited by name."""
    index = {}
    for root, dirs, names in os.walk('.'):
        dirs[:] = [d for d in dirs if d not in ('.git', 'obj', 'bin', 'node_modules')]
        for name in names:
            index.setdefault(name, []).append(os.path.join(root, name)[2:])
    return index


def doc_paths():
    out = [p for p in DOC_FILES if os.path.exists(p)]
    for directory in DOC_DIRS:
        for root, dirs, names in os.walk(directory):
            dirs[:] = [d for d in dirs if d != '.git']
            out += [os.path.join(root, n) for n in names if n.endswith('.md')]
    return sorted(set(out))


ANCHOR_BEFORE = 220
ANCHOR_AFTER = 90


def anchors(chunk, at):
    """Identifiers named inside backticks near position `at`, longest first.

    Bounded to the sentence around the citation rather than the whole chunk: a table row or
    paragraph often carries two citations, and taking every identifier in it would check each name
    against both files - which reads as "the code is gone" when the name simply belongs to the
    other citation. The window is cut further at any neighbouring citation, and identifiers that
    are themselves file names are dropped for the same reason.

    Longest first because a specific name (`ShouldStretchHolyStun`) is a better anchor than the
    type it lives in (`WHM_Reborn`), and one hit is enough to accept the citation.
    """
    start, end = max(0, at - ANCHOR_BEFORE), min(len(chunk), at + ANCHOR_AFTER)
    bounds = list(CITE.finditer(chunk)) + list(CONTINUATION.finditer(chunk))
    for other in bounds:
        if other.end() <= at and other.end() > start:
            start = other.end()
        if other.start() > at and other.start() < end:
            end = other.start()

    # Cutting at a neighbouring citation lands inside its backticks, which would shift every pair
    # in the segment by one and read the prose as code. Trim to whole backtick groups.
    if chunk[:start].count('`') % 2:
        nxt = chunk.find('`', start)
        start = len(chunk) if nxt < 0 else nxt + 1
    segment = chunk[start:end]
    if segment.count('`') % 2:
        segment = segment[:segment.rfind('`')]

    # Left of the citation first. The convention in these documents is `Identifier` (`File.cs:12`),
    # so a name standing after the citation usually belongs to the next clause, not to this one.
    left, right = [], []
    for group in IDENT.finditer(segment):
        if CITE.search(group.group(1)) or FILENAME.match(group.group(1).strip()):
            continue
        side = left if group.end() <= at - start else right
        for word in WORD.findall(group.group(1)):
            if word not in side:
                side.append(word)
    ordered = sorted(left, key=len, reverse=True) + sorted(right, key=len, reverse=True)
    return list(dict.fromkeys(ordered))


# A markdown list item. Like a table row it is a unit of its own: a list of findings puts one
# citation per bullet, and reading the whole block as a paragraph hands each bullet's identifier
# to its neighbour's citation - which reads as "the code is gone".
LIST_ITEM = re.compile(r'^\s*(?:[-*+]\s|\d+[.)]\s)')


def chunk_of(lines, index):
    """(text, offset of this line inside it) for the paragraph, table row or list item."""
    if lines[index].lstrip().startswith('|') or LIST_ITEM.match(lines[index]):
        return lines[index], 0
    start = index
    while start > 0 and lines[start - 1].strip():
        start -= 1
    end = index
    while end + 1 < len(lines) and lines[end + 1].strip():
        end += 1
    text = '\n'.join(lines[start:end + 1])
    offset = sum(len(lines[n]) + 1 for n in range(start, index))
    return text, offset


def find_lines(source_lines, ident):
    return [n for n, text in enumerate(source_lines, 1) if ident in text]


def check(doc, index, window):
    """[(status, detail)] for every citation in one document."""
    with open(doc, encoding='utf-8') as fh:
        lines = fh.read().split('\n')

    results = []
    for i, line in enumerate(lines):
        for m in CITE.finditer(line):
            name, cited = m.group('file'), int(m.group('line'))
            paths = index.get(name, [])
            if not paths:
                results.append(('gone', f'{doc}:{i + 1}  {name}:{cited} - no such file in the tree'))
                continue
            path = paths[0]
            with open(path, encoding='utf-8', errors='replace') as fh:
                source = fh.read().split('\n')

            if cited > len(source):
                results.append(('moved', f'{doc}:{i + 1}  {name}:{cited} - file has only '
                                         f'{len(source)} lines'))
                continue

            chunk, offset = chunk_of(lines, i)
            names = anchors(chunk, offset + m.start())
            names = [n for n in names if n not in name]
            if not names:
                results.append(('no anchor', f'{doc}:{i + 1}  {name}:{cited}'))
                continue

            hit, elsewhere = None, None
            for ident in names:
                at = find_lines(source, ident)
                if not at:
                    continue
                near = [n for n in at if abs(n - cited) <= window]
                if near:
                    hit = (ident, near[0])
                    break
                if elsewhere is None:
                    elsewhere = (ident, at[0], len(at))

            if hit:
                results.append(('ok', f'{doc}:{i + 1}  {name}:{cited} ~ {hit[0]} at {hit[1]}'))
            elif elsewhere:
                ident, at, count = elsewhere
                results.append(('moved', f'{doc}:{i + 1}  {name}:{cited} - `{ident}` is at line '
                                         f'{at}{" (+%d more)" % (count - 1) if count > 1 else ""}, '
                                         f'not within {window} lines of {cited}'))
            else:
                results.append(('gone', f'{doc}:{i + 1}  {name}:{cited} - none of '
                                        f'{", ".join(names[:3])} appears in {path}'))
    return results


def self_test():
    import tempfile
    work = tempfile.mkdtemp()
    src = os.path.join(work, 'Sample.cs')
    with open(src, 'w', encoding='utf-8') as fh:
        fh.write('\n'.join(['// header'] * 40 + ['	private bool ShouldHoldHoly()'] + ['	// body'] * 5))
    doc_ok = 'Die Regel steht in `Sample.cs:41` als `ShouldHoldHoly`.'
    doc_moved = 'Die Regel steht in `Sample.cs:5` als `ShouldHoldHoly`.'
    doc_gone = 'Die Regel steht in `Sample.cs:41` als `ThisNeverExisted`.'

    global index
    index = {'Sample.cs': [src]}
    for text, want in ((doc_ok, 'ok'), (doc_moved, 'moved'), (doc_gone, 'gone')):
        path = os.path.join(work, 'doc.md')
        with open(path, 'w', encoding='utf-8') as fh:
            fh.write(text)
        got = check(path, index, DEFAULT_WINDOW)
        assert got and got[0][0] == want, f'expected {want} for {text!r}, got {got}'

    # A citation with no identifier beside it must not be counted as a finding.
    path = os.path.join(work, 'doc.md')
    with open(path, 'w', encoding='utf-8') as fh:
        fh.write('Siehe Sample.cs:41 ohne Bezeichner.')
    assert check(path, index, DEFAULT_WINDOW)[0][0] == 'no anchor'

    # Two citations in one table row: the identifier belonging to the second must not be held
    # against the first, which is what made the first run report phantom "gone" hits.
    other = os.path.join(work, 'Other.cs')
    with open(other, 'w', encoding='utf-8') as fh:
        fh.write('\n'.join(['// x'] * 9 + ['	void DrawTheThing()']))
    index['Other.cs'] = [other]
    with open(path, 'w', encoding='utf-8') as fh:
        fh.write('| `ShouldHoldHoly` in `Sample.cs:41` | gelesen von `DrawTheThing` '
                 'in `Other.cs:10` |')
    got = [status for status, _ in check(path, index, DEFAULT_WINDOW)]
    assert got == ['ok', 'ok'], got
    del index['Other.cs']
    os.unlink(other)

    # A bare file name in backticks is the name of a file, not an identifier to look for inside
    # the file cited next to it. Held against `Sample.cs` it read as "the code is gone".
    with open(path, 'w', encoding='utf-8') as fh:
        fh.write('`Sample.cs:41` definiert es, `Other.cs` liest es.')
    assert check(path, index, DEFAULT_WINDOW)[0][0] == 'no anchor', check(path, index,
                                                                         DEFAULT_WINDOW)

    for name in os.listdir(work):
        os.unlink(os.path.join(work, name))
    os.rmdir(work)
    print('self-test ok: intact, moved, removed and anchorless citations told apart\n')


def main(argv):
    window = DEFAULT_WINDOW
    if '--window' in argv:
        window = int(argv[argv.index('--window') + 1])
    verbose = '--all' in argv

    self_test()

    global index
    index = repo_files()
    docs = doc_paths()

    counts = {'ok': 0, 'moved': 0, 'gone': 0, 'no anchor': 0}
    findings, archived = [], []
    for doc in docs:
        for status, detail in check(doc, index, window):
            counts[status] += 1
            if status in ('moved', 'gone'):
                (archived if os.path.basename(doc) in ARCHIVE else findings).append(
                    f'  {status:<9} {detail}')
            elif verbose:
                print(f'  {status:<9} {detail}')

    total = sum(counts.values())
    print(f'{len(docs)} document(s), {total} code citation(s): {counts["ok"]} intact, '
          f'{counts["moved"]} moved, {counts["gone"]} gone, {counts["no anchor"]} without an '
          f'identifier to check against.\n')

    if findings:
        print('\n'.join(findings))
        print(f'\n{len(findings)} citation(s) in documents that describe the current state no '
              f'longer point at\nwhat they name. "moved" is an aged line number; "gone" needs a '
              f'reader, because either the\ncode or the claim is no longer there.')
    else:
        print('Every checkable citation in the current-state documents still points at the '
              'identifier\nits sentence names.')

    if archived:
        print(f'\n{len(archived)} more in {", ".join(sorted(ARCHIVE))}, listed for completeness '
              f'and not a finding:\nthe archive records what was true when the check was made.')
        print('\n'.join(archived))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
