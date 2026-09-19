#!/usr/bin/env python3
"""Do the identifiers the concepts name still exist in the tree?

A concept that names `DataCenter.AggroedMembers` as the place where aggro is collected describes a
tree that does not exist - the field is called `TargetedPartyMembers`, and no identifier of the
first name was ever there. Nothing fails from that: the sentence reads well, the next pass looks
for the wrong name, finds nothing, and concludes the block is missing. That is exactly the shape of
the error this check exists for, and it is the one that produced it.

Reported, never failed. Three kinds of name legitimately have no counterpart in the tree:

- what a concept says was **removed** or **rejected** - the archive of the first fork audit is full
  of it, and deleting those sentences would delete the evidence;
- what a concept **proposes** - a planned measurement carries the name it would get;
- ordinary German words that happen to sit in backticks.

The first two say so in their own line, so the check reads the line before reporting. The third is
why this stays a report: no word list can separate a proposal from a typo, and a run that fails on
prose would be turned off within a week.

Usage: python3 .github/scripts/audit/check_concept_identifiers.py
Exit code 0 always; the finding is the report.
"""
import re
import sys
from pathlib import Path

CONCEPT_DIR = Path('docs/rotation-flow')
# The generator project counts too: a concept naming `StaticCodeGenerator.GenerateRotations` was
# reported only because this list did not include the folder the class lives in.
SOURCE_DIRS = ('RotationSolver.Basic', 'RotationSolver', 'RotationSolver.GameData',
               'RotationSolver.SourceGenerators', '.github')
SOURCE_PATTERNS = ('*.cs', '*.py', '*.resx', '*.yaml', '*.props', '*.csproj')

# `Name` or `Type.Member` - the member is what gets looked up. Short names produce noise without
# adding findings, so five characters is the floor.
CANDIDATE = re.compile(r'`([A-Z][A-Za-z0-9]*(?:\.[A-Za-z][A-Za-z0-9]*)*)`')
MIN_LENGTH = 5

# A line that says the thing is gone, was rejected, or is still to be built explains its own
# dangling name. Checked per line, because that is where the explanation stands.
EXPLAINED = re.compile(
    r'entfernt|gelöscht|zurückgebaut|zurueckgebaut|verworfen|ausgeschlossen|abgelehnt|'
    r'nicht umgesetzt|nicht gebaut|geplant|künftig|kuenftig|würde|wuerde|hieße|hiesse|'
    r'toter Code|dead code|früher|frueher|ehemals|einst|'
    r'removed|rejected|planned|would be', re.IGNORECASE)


def tree_text(dirs=SOURCE_DIRS, patterns=SOURCE_PATTERNS):
    """Everything the identifiers could live in, as one blob - this is a membership test, not a parse."""
    parts = []
    for directory in dirs:
        base = Path(directory)
        if not base.is_dir():
            continue
        for pattern in patterns:
            for path in base.rglob(pattern):
                try:
                    parts.append(path.read_text(encoding='utf-8', errors='replace'))
                except OSError:
                    continue
    return '\n'.join(parts)


def missing_in(text, blob, min_length=MIN_LENGTH):
    """Returns [(identifier, line number)] for names with no counterpart and no explanation."""
    out = []
    for number, line in enumerate(text.split('\n'), 1):
        explained = EXPLAINED.search(line)
        for match in CANDIDATE.finditer(line):
            member = match.group(1).split('.')[-1]
            if len(member) < min_length or member in blob:
                continue
            if explained:
                continue
            out.append((match.group(1), number))
    return out


def selftest():
    """Constructed cases, because a silent pass is otherwise indistinguishable from a clean tree."""
    blob = 'class Present { public int TargetedPartyMembers; }'

    if missing_in('Die Groesse `TargetedPartyMembers` steht bereit.', blob):
        raise AssertionError('an identifier that exists was reported')

    found = missing_in('Gefuellt wird `DataCenter.AggroedMembers` je Bild.', blob)
    if not any(name.endswith('AggroedMembers') for name, _ in found):
        raise AssertionError('an identifier that does not exist went unnoticed')

    if missing_in('`CalculateDamageFactor` wurde entfernt.', blob):
        raise AssertionError('a name the line marks as removed was reported')

    if missing_in('Ein `StunCoverage` waere zu messen - geplant.', blob):
        raise AssertionError('a name the line marks as planned was reported')

    if missing_in('Der `Pull` bleibt offen.', blob):
        raise AssertionError('a short word was reported despite the length floor')

    print('self-test ok: an existing identifier passes; a missing one is caught; names explained as '
          'removed\n  or planned are left alone, and names below the length floor are not counted.')


def main():
    selftest()
    blob = tree_text()
    if not blob:
        print('no source files found - the check cannot say anything')
        return 0

    total = 0
    for path in sorted(CONCEPT_DIR.glob('[0-9][0-9]-*.md')):
        found = missing_in(path.read_text(encoding='utf-8'), blob)
        if not found:
            continue
        print('%s:' % path.name)
        for name, number in found:
            print('  line %-5d `%s`' % (number, name))
        total += len(found)

    if total:
        print('\n%d identifier(s) named without a counterpart in the tree and without a line saying '
              'why.\nEach is either a name that changed, or a sentence that needs the word for what '
              'it is.' % total)
    else:
        print('Every identifier named in the concepts exists in the tree, or its line says why not.')
    return 0


if __name__ == '__main__':
    try:
        sys.exit(main())
    except BrokenPipeError:
        sys.stderr.close()
        sys.exit(0)
