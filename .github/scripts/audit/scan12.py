#!/usr/bin/env python3
"""Phase 12: concept documents that narrate their own revision history.

CLAUDE.md requires concept documents in `docs/` to state the standing position first
and justify it afterwards - BLUF and the Pyramid Principle, not a Gutachten that walks
through assumptions, revised assumptions and further corrections before arriving
anywhere. What a document ruled out belongs in the result with its reason; the chronicle
of its own versions does not, because it already lives in `AUDIT_LOG.md`.

The failure this guards against is not untidiness. A document whose beginning is only
correct in the light of its ending has a failure path: a reader who stops halfway holds
a withdrawn position for the current one. `09-tank-selfprotection.md` carried seven such
sections and a postscript that opened by stating "several statements above are
superseded by this" - and three of its statements were in fact overtaken by work that
had already shipped. `08-mitigation-synergy.md` opened by calling itself "a concept
without code" while both of its steps were built.

Two heuristics, both line-based:

  * **Chronicle headings** - a section whose title announces a round of the process
    rather than a subject ("Drittes Audit", "Nachtrag", "Verbesserung nach dem zweiten
    Audit", "NEU BEWERTET").
  * **Version-reference phrases** anywhere in the body ("die erste Fassung", "eine
    frühere Fassung", "in allen bisherigen Fassungen").

Neither is proof. A document may legitimately discuss the history of its *subject* -
`06-fork-audit.md` exists precisely to say where the fork's changes turned out wrong -
and prose may name two variants of a code snippet as "die erste" and "die zweite". The
scan prints a worklist; the reader decides whether a hit is the subject's history or the
document's own.

Usage: python3 .github/scripts/audit/scan12.py
"""
import re
import subprocess
import sys

ROOTS = ('docs/',)

HEADING = re.compile(r'^(?P<hashes>#{1,6})\s+(?P<title>.+?)\s*$')

# A heading that announces a round of the process rather than a subject.
CHRONICLE_HEADING = re.compile(
    r'(?:^|\W)(?:'
    r'(?:erstes|zweites|drittes|viertes|fünftes|sechstes|siebtes|weiteres)\s+audit'
    r'|audit\s+des\b|audit-verlauf'
    r'|nachtrag'
    r'|(?:erster|zweiter|dritter|vierter)\s+durchgang'
    r'|verbesserung\s+nach'
    r'|neu\s+bewertet'
    r'|stand\s+nach\b'
    r'|revision\b'
    r')',
    re.IGNORECASE)

# Prose that positions a statement against an earlier version of the same document.
VERSION_PHRASE = re.compile(
    r'\b(?:erste|zweite|dritte|vierte|frühere|alte|bisherige|vorige|vorherige|letzte)n?\s+'
    r'fassung(?:en)?\b',
    re.IGNORECASE)

# ... unless the sentence is plainly about code variants rather than document versions.
CODE_CONTEXT = re.compile(
    r'\b(?:schreibweise|variante|zeile|ausdruck|gegatet|ordnend)\w*\b', re.IGNORECASE)


def tracked_docs():
    out = subprocess.run(['git', 'ls-files', '*.md'],
                         capture_output=True, text=True, check=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(ROOTS)]


def scan_source(text):
    """Return (chronicle headings, version phrases) as (line no, line) pairs."""
    headings, phrases = [], []
    for no, raw in enumerate(text.splitlines(), 1):
        line = raw.rstrip()
        m = HEADING.match(line)
        if m and CHRONICLE_HEADING.search(m.group('title')):
            headings.append((no, m.group('title')))
            continue
        if VERSION_PHRASE.search(line) and not CODE_CONTEXT.search(line):
            phrases.append((no, line.strip()[:120]))
    return headings, phrases


def self_test():
    doc = '\n'.join([
        '# 09 · Tank-Selbstschutz',
        '## Ergebnis',
        'Der belastbare Kern ist die Zielwahl.',
        '## Drittes Audit — gegen die Artefaktprüfung',
        'Die erste Fassung führte Catharsis als auslöserbehaftet.',
        '## Nachtrag — Umsetzungsstand',
        '## Was ausgeschlossen wurde und warum',
        'Die ordnende Schreibweise versucht zusätzlich die Vorgängeraktion,',
        'die gegatete nicht - beides sind Fassungen desselben Blocks.',
        '## Verbesserung nach dem zweiten Audit',
    ])
    headings, phrases = scan_source(doc)
    got_h = [t for _, t in headings]
    assert got_h == ['Drittes Audit — gegen die Artefaktprüfung',
                     'Nachtrag — Umsetzungsstand',
                     'Verbesserung nach dem zweiten Audit'], got_h
    # The subject-matter headings must not be reported.
    assert 'Ergebnis' not in got_h and 'Was ausgeschlossen wurde und warum' not in got_h
    got_p = [t for _, t in phrases]
    assert len(got_p) == 1 and got_p[0].startswith('Die erste Fassung führte'), got_p
    # A sentence about two code variants is not a version reference.
    assert not any('gegatete' in t for t in got_p), got_p

    # A clean document reports nothing.
    clean = '# 03 · Universell\n## Ergebnis\nDie Slot-Kette ist für jeden Job gleich.\n'
    assert scan_source(clean) == ([], []), scan_source(clean)

    print('self-test ok: chronicle headings matched, subject headings and code variants '
          'excluded\n')


def main():
    self_test()
    docs = tracked_docs()
    if not docs:
        print('no tracked markdown under docs/')
        return 1

    total = 0
    for path in docs:
        try:
            with open(path, encoding='utf-8') as fh:
                headings, phrases = scan_source(fh.read())
        except OSError:
            continue
        if not headings and not phrases:
            continue
        total += len(headings) + len(phrases)
        print(f'=== {path}')
        for no, title in headings:
            print(f'  heading  {path}:{no}  {title}')
        for no, line in phrases:
            print(f'  phrase   {path}:{no}  {line}')
        print()

    print(f'{len(docs)} concept document(s) scanned, {total} hit(s).')
    if not total:
        print('No document narrates its own revision history.')
        return 0
    print('\nEach hit is a question, not a verdict: is this the history of the subject\n'
          '(keep it) or the history of this document (move it to AUDIT_LOG.md)?')
    return 0


if __name__ == '__main__':
    sys.exit(main())
