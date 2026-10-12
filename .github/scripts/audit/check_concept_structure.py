#!/usr/bin/env python3
"""Is each concept built the way CLAUDE.md asks, or does it carry the history of its own writing?

The rules (CLAUDE.md, "Artefakte - Konzepte"): the standing state first, its reasons after, what is
excluded with its reason; no chronicle of the document's own versions, no addenda; the loop stages are
a working procedure, not an outline. A concept that breaks them ages in place: a section headed
"Befunde" or "Antithesen" or "(A238)" records what was found at one time, and the next change to the
code leaves it standing beside the new state - a reader of that section alone gets the old one.

Checked per heading, in every concept under docs/rotation-flow/ (the README is an index, 06 is the
closed archive of the first audit and exempt):
  S1  a loop stage as a heading (Research, Optionen, Abwägung, Abgleich, Review, Falsifikation,
      Antithesen, Umsetzung, Nachweis, Wirksamkeit, Modellprüfung) - the procedure used as outline;
  S2  the document's history in a heading: an archive id (A123), a date, "Nachtrag", "Befund(e)",
      "behoben", "Runde", "Review";
  S3  the same heading twice under the same parent - one subject in two places, one of which ages.
      ("Grenzen" under two different sections is two subjects, each with its limits.)

Reported, and failed when --strict is given. The check reads headings only: whether a section's body
is current is a question of content, which only reading against the code answers.

Usage: python3 .github/scripts/audit/check_concept_structure.py [--strict]
"""
import re
import sys
from pathlib import Path

CONCEPT_DIR = Path('docs/rotation-flow')
EXEMPT = {'README.md', '06-fork-audit.md'}

# A heading that *is* a stage - it starts with the stage's name. "Wo die Umsetzung von seinem Wortlaut
# abweicht" names a subject that happens to contain the word, and is not caught.
LOOP_STAGES = re.compile(
    r'^(Research|Optionen|Abwägung|Abgleich|Review|Falsifikation|Antithesen?|Umsetzung|Nachweis(barkeit)?|'
    r'Wirksamkeit|Dokumentation|Modellprüfung)\b', re.IGNORECASE)
HISTORY = re.compile(
    r'\(A\d{2,3}[^)]*\)|\bA\d{2,3}\b|\d{1,2}\.\d{1,2}\.\d{4}|\bNachtrag|\bBefunde?\b|\bbehoben\b|\bRunde\b',
    re.IGNORECASE)
HEADING = re.compile(r'^(#{1,6})\s+(.*\S)\s*$')


def findings_in(name, text):
    found = []
    seen = {}
    stack = []
    in_code = False
    for number, line in enumerate(text.splitlines(), 1):
        if line.lstrip().startswith('```'):
            in_code = not in_code
            continue
        match = None if in_code else HEADING.match(line)
        if not match:
            continue
        title = match.group(2)
        level = len(match.group(1))
        stack = [entry for entry in stack if entry[0] < level]
        parent = tuple(t for _, t in stack)
        stack.append((level, title))
        if LOOP_STAGES.search(title):
            found.append((name, number, 'S1', title))
        if HISTORY.search(title):
            found.append((name, number, 'S2', title))
        key = (parent, re.sub(r'\s+', ' ', title.strip().lower()))
        if key in seen:
            found.append((name, number, 'S3', f'{title} (also line {seen[key]})'))
        else:
            seen[key] = number
    return found


def self_test():
    sample = (
        '# Thema\n## Sachstand\n### Antithesen\n### Befunde\n### Umsetzung (A226)\n'
        '### Mechanik (Belege, abgerufen 04.10.2026)\n### Regeln\n### Mechanik (Belege, abgerufen 04.10.2026)\n'
        '```\n### Antithesen in a code block\n```\n### Ausgeschlossen\n'
        '## Zweites Thema\n### Regeln\n### Wo die Umsetzung von seinem Wortlaut abweicht\n'
    )
    got = {(f[2], f[1]) for f in findings_in('x', sample)}
    expected = {('S1', 3), ('S2', 4), ('S1', 5), ('S2', 5), ('S2', 6), ('S2', 8), ('S3', 8)}
    assert got == expected, sorted(got ^ expected)
    print('self-test ok: loop stages, history marks and repeated headings under one parent are caught; '
          'plain headings, a stage word inside a subject, the same heading under another parent and code blocks '
          'are left alone')


def main():
    self_test()
    strict = '--strict' in sys.argv[1:]
    found = []
    for path in sorted(CONCEPT_DIR.glob('*.md')):
        if path.name in EXEMPT:
            continue
        found += findings_in(path.name, path.read_text(encoding='utf-8'))
    by_doc = {}
    for name, number, kind, title in found:
        by_doc.setdefault(name, []).append((number, kind, title))
    for name, items in by_doc.items():
        print(f'{name}:')
        for number, kind, title in items:
            print(f'  line {number:<5} {kind}  {title}')
    print(f'{len(found)} heading(s) against the concept rules in {len(by_doc)} concept(s).')
    return 1 if strict and found else 0


if __name__ == '__main__':
    sys.exit(main())
