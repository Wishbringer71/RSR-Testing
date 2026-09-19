#!/usr/bin/env python3
"""Does the release description still fit into the field it is pasted into?

`docs/fork-changes-in-play.md` is not a repository document that happens to be long. It is the
text that goes into the release description on GitHub, by copy and paste, and that field counts
characters and stops accepting them. Measured on the 18611-character version: the counter reached
"0 remaining" inside section 7, which started at character 12347 - so the field takes somewhere
between those two figures, and everything after it was silently dropped. Nothing failed, no build
broke, and the release simply carried a text that ended mid-sentence.

That is the aging shape this check guards: the document grows section by section while the limit
stays where it is, and the first sign of it is a truncated release. The exact limit of the field
cannot be measured from here - there is no access to the form - so the budget below sits well
under the lower of the two measured bounds rather than at it.

What this does NOT check is whether the content belongs in a release at all. Accounts of the
fork's own mistakes, of what has no effect in the game, and of what is still open are work
records; they belong in AUDIT_LOG.md and TODO.md, and no character count can tell them apart
from a description of what changes in a fight. That distinction stays with the author.

Usage: python3 .github/scripts/audit/check_release_note_size.py
Exit code 1 when the release description no longer fits.
"""
import sys
from pathlib import Path

RELEASE_NOTE = 'docs/fork-changes-in-play.md'

# The field took a text of at least 12347 characters and less than 18611. The budget keeps a
# margin below the lower bound, because that bound is an observation, not a documented limit.
BUDGET = 11000


def check(text):
    """Returns the problems found; empty means the text fits."""
    problems = []
    size = len(text)
    if size > BUDGET:
        problems.append(
            '%s characters, %s over the budget of %s - the release form stops accepting '
            'the paste and drops the rest silently' % (size, size - BUDGET, BUDGET))
    return problems


def selftest():
    """Constructed cases, because a silent pass is otherwise indistinguishable from a clean tree."""
    if check('x' * (BUDGET - 1)):
        raise AssertionError('a text below the budget was rejected')
    if check('x' * BUDGET):
        raise AssertionError('a text exactly at the budget was rejected')
    if not check('x' * (BUDGET + 1)):
        raise AssertionError('a text one character over the budget went unnoticed')
    if not check('x' * 18611):
        raise AssertionError('the version that actually got truncated went unnoticed')
    print('self-test ok: the budget accepts a text up to its limit and catches every text past '
          'it,\n  including the size that was measured as truncated.')


def main():
    selftest()
    path = Path(RELEASE_NOTE)
    if not path.exists():
        print('%s is missing - it is the release description and cannot simply be gone' % RELEASE_NOTE)
        return 1
    problems = check(path.read_text(encoding='utf-8'))
    if problems:
        print('%s does not fit the release form:' % RELEASE_NOTE)
        for problem in problems:
            print('  %s' % problem)
        print('\n  Shorten it, or move what is not a description of the fight into TODO.md, '
              'AUDIT_LOG.md or README.md.')
        return 1
    size = len(path.read_text(encoding='utf-8'))
    print('%s: %s characters, %s left in the budget of %s' % (RELEASE_NOTE, size, BUDGET - size, BUDGET))
    return 0


if __name__ == '__main__':
    sys.exit(main())
