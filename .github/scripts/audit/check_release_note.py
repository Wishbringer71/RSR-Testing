#!/usr/bin/env python3
"""Does the release description describe one release, and is it still the next one?

Owner's rule: **a release description carries the differences, not the standing picture.** So
`docs/fork-changes-since-last-release.md` is the release text, and it names the release it starts
from in its heading - "Changes since 7.5.6.10+wsh1". `docs/fork-changes-in-play.md` keeps the whole
distance from upstream and takes each release's text over once that release has shipped.

Two things age here, and neither fails on its own.

**The base tag.** Once a release ships, its description is a record of the past, and the file has
to start over from the new tag. Nothing forces that: the file still parses, the workflow still
publishes it, and the next release would carry the previous one's text plus whatever was appended
- which is exactly the cumulative document the rule replaced. So the heading's tag is measured
against the newest fork tag on `origin`: they must be the same.

**The size.** The old form of this check guarded 11000 characters, because the text was pasted
into the release form by hand and that field silently truncated an 18611-character version. The
description now goes to the API as `body_path`, so that limit is gone and the one that applies is
the API's own 125000. Keeping the old budget would cost content for a constraint that no longer
exists - it already did once, on the 7.5.6.10+wsh1 text.

What this does NOT check is whether the content belongs in a release at all. Accounts of the
fork's own mistakes, of what has no effect in the game, and of what is still open are work
records; they belong in AUDIT_LOG.md and TODO.md, and no measurement can tell them apart from a
description of what changes in a fight. That distinction stays with the author.

Usage: python3 .github/scripts/audit/check_release_note.py [--remote origin]
Exit code 1 when the description has aged out of its purpose.
"""
import re
import subprocess
import sys
from pathlib import Path

RELEASE_NOTE = 'docs/fork-changes-since-last-release.md'
STANDING = 'docs/fork-changes-in-play.md'

# GitHub's release body limit. Far above anything this file should reach - it is here so an
# accident (a whole document pasted in) is caught rather than published truncated.
LIMIT = 125000

HEADING = re.compile(r'^#\s+Changes since\s+(\S+)\s*$', re.MULTILINE)
FORK_TAG = re.compile(r'^\d+(?:\.\d+)*\+wsh\d+$')


def run(*args):
    out = subprocess.run(args, capture_output=True, text=True)
    return out.stdout.strip() if out.returncode == 0 else ''


def tag_key(tag):
    numeric, _, fork = tag.partition('+wsh')
    return tuple(int(p) for p in numeric.split('.')) + (int(fork),)


def newest_fork_tag(remote):
    """The highest <upstream>+wsh<n> tag on the remote, or '' when none is reachable."""
    names = set()
    for line in run('git', 'ls-remote', '--tags', remote).split('\n'):
        if not line or line.endswith('^{}'):
            continue
        parts = line.split('refs/tags/')
        if len(parts) == 2 and FORK_TAG.match(parts[1]):
            names.add(parts[1])
    return max(names, key=tag_key) if names else ''


def check(text, newest):
    """Returns the problems found; empty means the description is the next release's.

    `newest` is the newest published fork tag, or '' when the tags could not be read - in which
    case the base tag is not compared, only its presence.
    """
    problems = []

    size = len(text)
    if size > LIMIT:
        problems.append('%s characters, %s over the API limit of %s - the release body would be '
                        'rejected or cut' % (size, size - LIMIT, LIMIT))

    heading = HEADING.search(text)
    if not heading:
        problems.append('no "# Changes since <tag>" heading - without it nothing states which '
                        'release this text is the difference to')
        return problems

    base = heading.group(1)
    if not FORK_TAG.match(base):
        problems.append('"%s" is not a fork tag of the form <upstream>+wsh<n>' % base)
    elif newest and base != newest:
        problems.append('starts from %s, but %s has shipped since - carry this text over into %s '
                        'and start again from %s' % (base, newest, STANDING, newest))

    return problems


def self_test():
    """Constructed defects, because a silent pass is otherwise indistinguishable from a clean tree."""
    good = '# Changes since 7.5.6.10+wsh1\n\nSomething changed.\n'
    if check(good, '7.5.6.10+wsh1'):
        raise AssertionError('a current description was rejected: %s'
                             % check(good, '7.5.6.10+wsh1'))

    # The defect this exists for: a release shipped and the file still starts from the one before.
    stale = check(good, '7.5.6.11+wsh1')
    if not any('has shipped since' in p for p in stale):
        raise AssertionError('a description left over from the previous release went unnoticed: %s'
                             % stale)

    if not any('no "# Changes since' in p for p in check('# Release notes\n\nStuff.\n', '')):
        raise AssertionError('a description without a base tag went unnoticed')

    if not any('not a fork tag' in p for p in check('# Changes since v2\n', '')):
        raise AssertionError('a base that is not a fork tag went unnoticed')

    # Without reachable tags the base cannot be compared, and that must not fail the tree.
    if check(good, ''):
        raise AssertionError('an unreachable remote was treated as a defect')

    if not any('API limit' in p for p in check(good + 'x' * LIMIT, '7.5.6.10+wsh1')):
        raise AssertionError('a description past the API limit went unnoticed')

    # Ordering has to put +wsh10 above +wsh9, which a string comparison would not.
    if tag_key('7.5.6.10+wsh10') <= tag_key('7.5.6.10+wsh9'):
        raise AssertionError('fork tags are ordered as strings')

    print('self-test ok: a current description is accepted; a leftover from the previous release,\n'
          '  a missing or malformed base tag and an oversized body are each caught; an unreachable\n'
          '  remote is not treated as a defect; and +wsh10 sorts above +wsh9')


def main():
    self_test()

    remote = 'origin'
    if '--remote' in sys.argv:
        remote = sys.argv[sys.argv.index('--remote') + 1]

    path = Path(RELEASE_NOTE)
    if not path.exists():
        print('%s is missing - it is the release description and cannot simply be gone'
              % RELEASE_NOTE)
        return 1

    text = path.read_text(encoding='utf-8')
    if not text.strip():
        print('%s is empty - the release would ship without a description' % RELEASE_NOTE)
        return 1

    newest = newest_fork_tag(remote)
    problems = check(text, newest)
    if problems:
        print('%s is no longer the next release\'s description:' % RELEASE_NOTE)
        for problem in problems:
            print('  %s' % problem)
        return 1

    if newest:
        print('%s: %s characters, describing what changed since %s (the newest tag on %s)'
              % (RELEASE_NOTE, len(text), newest, remote))
    else:
        print('%s: %s characters; no fork tag reachable on %s, so the base was not compared'
              % (RELEASE_NOTE, len(text), remote))
    return 0


if __name__ == '__main__':
    sys.exit(main())
