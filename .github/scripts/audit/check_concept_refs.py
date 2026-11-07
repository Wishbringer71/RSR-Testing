#!/usr/bin/env python3
"""Does every code comment that names a concept section point at a section that is there?

The code names its concept as `concept 01, "Satz je Stufe aus den Stufenführern"`: the number of the file under
docs/rotation-flow/ and the title of the section. When a section moves to another concept (concept 20, "Ordnung der
Konzepte": the Crucible from 01 to 21) or is renamed, the comment keeps the old pair, reads well, and sends the next
reader to a file where the section is not. Nothing fails from that - which is why this check exists (A648).

A title counts as found in file NN when a heading (`## Title`), a bold paragraph or list-item title (`**Title**`,
`- **Title**`) or an italic list-item title (`- *Title*`) there starts with it, quotation marks left out; a comment may
shorten a long title. Only the quoted form is
checked: `concept 01` alone or `concept 01, K6` names no title. Comments whose number names no concept file are
reported too.

Usage:
  check_concept_refs.py              report every reference whose section is not in its file; exit 1 if any
  check_concept_refs.py --self-test  constructed files and comments, one right and three wrong
"""
import re
import sys
import tempfile
from pathlib import Path

CONCEPT_DIR = Path('docs/rotation-flow')
CODE_DIRS = (Path('RotationSolver'), Path('RotationSolver.SourceGenerators'))
REF = re.compile(r'concept (\d{2}), "([^"]+)"')
TITLE = re.compile(r'^(?:#{1,6} +(?P<h>.+?)\s*$|\s*(?:- )?\*\*(?P<b>[^*]+)\*\*|\s*- \*(?P<i>[^*]+)\*)')
QUOTES = str.maketrans('', '', '„“”"‚‘’')


def norm(text):
    """A title as compared: quotation marks dropped - a heading may quote a phrase the comment writes plain."""
    return ' '.join(text.translate(QUOTES).split())


def titles(path):
    """Every heading, bold paragraph title and italic list-item title of a concept, as written."""
    out = []
    for line in path.read_text(encoding='utf-8').splitlines():
        m = TITLE.match(line)
        if m:
            out.append(norm(m.group('h') or m.group('b') or m.group('i')))
    return out


def concept_files(directory):
    """Concept number -> its titles."""
    files = {}
    for path in sorted(directory.glob('[0-9][0-9]-*.md')):
        files[path.name[:2]] = titles(path)
    return files


def findings(code_dirs, files):
    """[(file, line number, number, title, reason)] for every reference whose section is not where it says."""
    out = []
    for directory in code_dirs:
        if not directory.is_dir():
            continue
        for path in sorted(directory.rglob('*.cs')):
            if '/obj/' in path.as_posix() or '/bin/' in path.as_posix():
                continue
            for number, line in enumerate(path.read_text(encoding='utf-8', errors='replace').splitlines(), 1):
                for m in REF.finditer(line):
                    concept, title = m.group(1), norm(m.group(2))
                    if concept not in files:
                        out.append((path, number, concept, title, 'no concept file has this number'))
                    elif not any(t.startswith(title) for t in files[concept]):
                        elsewhere = [c for c, ts in files.items() if any(t.startswith(title) for t in ts)]
                        where = f'; it is in concept {", ".join(elsewhere)}' if elsewhere else ''
                        out.append((path, number, concept, title, f'no section of concept {concept} starts with it{where}'))
    return out


def self_test():
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        docs = root / 'docs'
        docs.mkdir()
        (docs / '01-jobs.md').write_text('# 01\n\n## BST\n\n**Hörner nach Lage** (A1)\n', encoding='utf-8')
        (docs / '21-crucible.md').write_text('# 21\n\n- *K10 – Zwischen den Kämpfen* (A2)\n', encoding='utf-8')
        code = root / 'code'
        code.mkdir()
        (code / 'A.cs').write_text('// concept 01, "Hörner nach Lage"\n'
                                   '// concept 01, "K10 – Zwischen den Kämpfen"\n'
                                   '// concept 99, "Nirgends"\n'
                                   '// concept 21, "K11"\n', encoding='utf-8')
        got = findings((code,), concept_files(docs))
        reasons = [f[4] for f in got]
        ok = (len(got) == 3 and 'it is in concept 21' in reasons[0] and 'no concept file' in reasons[1]
              and 'starts with it' in reasons[2])
        if not ok:
            print(f'self-test FAILED: {got}')
            return False
    print('self-test ok: a moved section, an unknown concept and a missing title are reported; a right reference passes')
    return True


def main():
    if not self_test():
        return 1
    if '--self-test' in sys.argv:
        return 0
    files = concept_files(CONCEPT_DIR)
    if not files:
        print('no concept files found - run from the repository root')
        return 1
    found = findings(CODE_DIRS, files)
    for path, number, concept, title, reason in found:
        print(f'{path}:{number}: concept {concept}, "{title}" - {reason}')
    total = sum(1 for d in CODE_DIRS if d.is_dir() for p in d.rglob('*.cs') for _ in REF.finditer(
        p.read_text(encoding='utf-8', errors='replace')))
    print(f'{total} reference(s) to a concept section in the code, {len(found)} not where they say.')
    return 1 if found else 0


if __name__ == '__main__':
    sys.exit(main())
