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
REFERENCE = re.compile(r'(\d\d-[a-z0-9-]+\.md)')


def collect(directory):
    """Returns {file name: set of concept files it references}, excluding self-references."""
    links = {}
    for path in sorted(directory.glob('*.md')):
        text = path.read_text(encoding='utf-8')
        links[path.name] = set(REFERENCE.findall(text)) - {path.name}
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

    print('self-test ok: a linked pair passes; a dangling reference and a concept without an '
          'inbound\n  link are each caught, and a self-reference does not count as one.')


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

    if broken:
        print('\nReferences that point nowhere:')
        for name, ref in broken:
            print('  %s -> %s' % (name, ref))
        return 1

    print('\nEvery concept reference points at a file that exists.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
