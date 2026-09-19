#!/usr/bin/env python3
"""Do the concept documents still know about each other?

The concepts under `docs/rotation-flow/` answer neighbouring questions: how large an
incoming hit is, which answer it deserves, who gets healed, when a tank may hold a
cooldown back. A rule written into one of them is invisible to the others, and after a
context compaction the next pass reads whichever file it opens first - so a concept nobody
points at is a concept that silently ages out of use. Nothing fails when that happens,
which is exactly why it needs a check.

Two questions, and only two, because both can be answered from the files themselves:

1. **Does every reference point at a file that exists?** A renamed or deleted concept
   leaves the pointer behind, and the pointer is what the next pass follows.
2. **Is every concept reachable from another one?** A concept with no inbound reference is
   reported, not failed: some are entry points by design (the numbered series starts
   somewhere) and some are archives of a finished pass. The report names them so the
   decision is made, rather than made silently by nobody looking.

What this does NOT check is whether two concepts say contradictory things. No script can;
that stays with the author, and the cross-check belongs in the loop for every pass.

Usage: python3 .github/scripts/audit/check_concept_links.py
Exit code 1 only for a reference that points nowhere.
"""
import re
import sys
from pathlib import Path

CONCEPT_DIR = Path('docs/rotation-flow')

# The path matters, and leaving it out produced a false alarm on the first run: CLAUDE.md points at
# `docs/method/01-loop-evaluation-methods.md`, which exists - but matching the bare file name
# measured it against the concept series and called it broken. A folder other than the concept one
# is somebody else's numbering.
REFERENCE = re.compile(r'(?:docs/([a-z0-9-]+)/)?(\d\d-[a-z0-9-]+\.md)')


def concept_refs(text):
    """Concept file names referenced in `text` - references into another docs folder excluded."""
    return {name for folder, name in REFERENCE.findall(text)
            if not folder or folder == CONCEPT_DIR.name}

# The working documents point into the concepts too, and those pointers rot the same way. TODO.md
# named `07-codebase-audit.md` as the place a planned document would live - a file that never
# existed and could not, because 07 has been heal-target-priority for as long as the series has had
# numbers. Checking only inside the concept folder never saw it.
OUTSIDE = ('TODO.md', 'AUDIT_LOG.md', 'README.md', 'CLAUDE.md')

# A pointer to a document that is deliberately still to be written is not a broken reference. It has
# to say so in the same line, so the intent is readable rather than assumed.
PLANNED = re.compile(r'noch nicht angelegt|not created yet|anzulegen')


# The index names every concept by design, so counting it as an inbound link would make the orphan
# report say "all reachable" and mean nothing. It is navigation, not a concept, and stays out of
# both sides of the measurement.
INDEX = 'README.md'
NUMBERED = re.compile(r'^\d\d-')


def collect(directory):
    """Returns {concept file: set of concept files it references}, excluding self-references.

    Only the numbered series counts as a concept. The index is skipped on purpose: it points at all
    of them, and a graph in which everything is reachable from one navigation page answers nothing.
    """
    links = {}
    for path in sorted(directory.glob('*.md')):
        if path.name == INDEX or not NUMBERED.match(path.name):
            continue
        text = path.read_text(encoding='utf-8')
        links[path.name] = concept_refs(text) - {path.name}
    return links


def check(links):
    """Returns (broken, orphans). Broken references fail the run, orphans are reported.

    Self-references are dropped here rather than trusted to have been dropped by the caller:
    a document pointing at itself is not an inbound link from anywhere, and a check that
    depends on its caller having cleaned the input is a check with a second place to fail.
    """
    known = set(links)
    outbound = {name: refs - {name} for name, refs in links.items()}

    broken = []
    for name, refs in outbound.items():
        for ref in sorted(refs - known):
            broken.append((name, ref))

    referenced = set()
    for refs in outbound.values():
        referenced |= refs
    orphans = sorted(known - referenced)
    return broken, orphans


def check_outside(known, paths=OUTSIDE):
    """References from the working documents into the concept series, line by line.

    Returns (broken, planned): a reference whose target is missing fails unless its own line marks
    it as a document still to be written.
    """
    broken, planned = [], []
    for name in paths:
        path = Path(name)
        if not path.exists():
            continue
        for number, line in enumerate(path.read_text(encoding='utf-8').split('\n'), 1):
            for ref in concept_refs(line):
                if ref in known:
                    continue
                if PLANNED.search(line):
                    planned.append((name, number, ref))
                else:
                    broken.append((name, number, ref))
    return broken, planned


def todo_by_concept(path=Path('TODO.md')):
    """Which open points name which concept.

    The binding lives in TODO.md and only there - a copy of the titles inside each concept would be
    a second place to age. Entries carry a `**Konzept:**` line under their heading; this reads it
    back so the assignment can be seen without opening both files.
    """
    if not path.exists():
        return {}, []
    assigned, unassigned = {}, []
    heading = None
    for line in path.read_text(encoding='utf-8').split('\n'):
        if line.startswith('### '):
            heading = line[4:].strip()
            unassigned.append(heading)
        elif heading and line.startswith('**Konzept:**'):
            for ref in concept_refs(line):
                assigned.setdefault(ref, []).append(heading)
            if unassigned and unassigned[-1] == heading:
                unassigned.pop()
            heading = None
    return assigned, unassigned


def selftest():
    """Constructed cases, because a silent pass is otherwise indistinguishable from a clean tree."""
    clean = {'01-a.md': {'02-b.md'}, '02-b.md': {'01-a.md'}}
    broken, orphans = check(clean)
    if broken or orphans:
        raise AssertionError('a mutually linked pair was reported: %s %s' % (broken, orphans))

    broken, _ = check({'01-a.md': {'99-gone.md'}, '02-b.md': {'01-a.md'}})
    if not any(ref == '99-gone.md' for _, ref in broken):
        raise AssertionError('a reference to a missing concept went unnoticed')

    _, orphans = check({'01-a.md': {'02-b.md'}, '02-b.md': set()})
    if orphans != ['01-a.md']:
        raise AssertionError('a concept nobody points at went unnoticed: %s' % orphans)

    # A self-reference must not keep a document out of the orphan report.
    _, orphans = check({'01-a.md': {'01-a.md'}, '02-b.md': set()})
    if '01-a.md' not in orphans:
        raise AssertionError('a self-reference was counted as an inbound link')

    if NUMBERED.match(INDEX):
        raise AssertionError('the index would be counted as a concept')

    known = {'01-a.md', '02-b.md'}
    probe = Path('_selftest_outside.md')
    probe.write_text('siehe `docs/rotation-flow/99-missing.md`\n'
                     'geplant: `docs/rotation-flow/98-later.md` - noch nicht angelegt\n'
                     'gut: `docs/rotation-flow/01-a.md`\n'
                     'fremder Ordner: `docs/method/01-loop-evaluation-methods.md`\n', encoding='utf-8')
    try:
        broken, planned = check_outside(known, paths=(str(probe),))
    finally:
        probe.unlink()
    if not any(ref == '99-missing.md' for _, _, ref in broken):
        raise AssertionError('a dangling reference from a working document went unnoticed')
    if not any(ref == '98-later.md' for _, _, ref in planned):
        raise AssertionError('a reference marked as still to be written was treated as broken')
    if any(ref == '01-a.md' for _, _, ref in broken):
        raise AssertionError('a reference to an existing concept was reported')
    if any('loop-evaluation' in ref for _, _, ref in broken + planned):
        raise AssertionError('a reference into another docs folder was measured against the concepts')

    print('self-test ok: a linked pair passes; a dangling reference and a concept without an '
          'inbound\n  link are each caught, and a self-reference does not count as one; a dangling '
          'reference\n  from a working document is caught, one marked as still to be written is not.')


def main():
    selftest()
    if not CONCEPT_DIR.is_dir():
        print('%s is missing - nothing to check' % CONCEPT_DIR)
        return 1

    links = collect(CONCEPT_DIR)
    broken, orphans = check(links)

    print('%d concept document(s) checked.' % len(links))

    if orphans:
        print('\nNo other concept points at these (report, not a failure):')
        for name in orphans:
            print('  %s' % name)

    outside_broken, planned = check_outside(set(links))

    if planned:
        print('\nPointing at a concept that is deliberately still to be written:')
        for name, number, ref in planned:
            print('  %s:%d -> %s' % (name, number, ref))

    if broken or outside_broken:
        print('\nReferences that point nowhere:')
        for name, ref in broken:
            print('  %s -> %s' % (name, ref))
        for name, number, ref in outside_broken:
            print('  %s:%d -> %s' % (name, number, ref))
        return 1

    assigned, unassigned = todo_by_concept()
    if assigned or unassigned:
        print('\nOpen points per concept (from TODO.md):')
        for name in sorted(links):
            points = assigned.get(name, [])
            print('  %-38s %d' % (name, len(points)))
        print('  %-38s %d' % ('(no concept named)', len(unassigned)))

    print('\nEvery concept reference points at a file that exists, from the concepts and from '
          '%s.' % ', '.join(OUTSIDE))
    return 0


if __name__ == '__main__':
    try:
        sys.exit(main())
    except BrokenPipeError:
        # Reading the report through `head` closes the pipe early. That is a normal way to use a
        # report and must not look like a failure, so the exit code stays the one a full run would
        # have given: the checks above have already decided it.
        sys.stderr.close()
        sys.exit(0)
