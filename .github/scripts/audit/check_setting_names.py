#!/usr/bin/env python3
"""Do the release texts name each setting by the text the owner sees?

The description of 7.5.7.6+wsh1 named a setting "Hold mitigation under invulnerability"; no setting
has that text, the one meant reads "Spend only the mitigation a measured tankbuster needs" (A333). The
reader looks for the quoted name in the settings window, does not find it, and cannot act on the
sentence. `check_concept_defaults.py` reads only names that come with a stated default, so this one
passed.

A quoted text in `docs/fork-changes-*.md` counts as naming a setting when the word "setting", "option"
or "switch" stands in the same sentence, or when it nearly matches a setting text without matching it -
the slip this exists to catch. It must then match one of the texts the owner sees: a `UI("...")` label,
its `Description = "..."`, a rotation's `RotationConfig(Name = "...")`, or a window text
(`[Description("...")]` in `UiString`). Compared with whitespace collapsed, case ignored and without a
closing full stop; an ellipsis stands for any text. Quotation marks carry far more than setting names -
log lines, action names - and those are left alone. So is a name in a sentence that says "removed": the
reader is told not to look for it, and the settings window no longer holds its text (A339).

Usage: python3 .github/scripts/audit/check_setting_names.py
Exit code 1 when a release text names a setting by a text no setting has.
"""
import difflib
import re
import sys
from pathlib import Path

DOCS = (Path('docs/fork-changes-in-play.md'), Path('docs/fork-changes-since-last-release.md'))
SOURCE_DIRS = (Path('RotationSolver'),)

LABEL = re.compile(r'(?:\bUI\(\s*|\bName\s*=\s*|\bDescription\s*=\s*|\[Description\(\s*)"(?P<label>[^"]{4,})"')
# Backticks stay on one line; straight quotes may wrap, as the documents break at a fixed width.
QUOTED = re.compile(r'`(?P<b>[^`\n]{6,160})`|"(?P<q>[^"]{6,200})"')
SETTING_WORD = re.compile(r'\b(?:settings?|options?|switch(?:es|ed)?)\b', re.IGNORECASE)
REMOVED_WORD = re.compile(r'\bremoved\b', re.IGNORECASE)
SENTENCE_END = re.compile(r'[.!?](?:\s|$)|\n\s*\n|\n\s*[-*+]\s')
NEAR = 0.8


def key(text):
    """A text as it is compared: whitespace collapsed, case ignored, no closing full stop."""
    return ' '.join(text.split()).rstrip('.').lower()


def labels(dirs=SOURCE_DIRS):
    """Every text a setting or a window shows the owner, as compared keys."""
    out = set()
    for directory in dirs:
        if not directory.is_dir():
            continue
        for path in directory.rglob('*.cs'):
            if '/obj/' in path.as_posix() or '/bin/' in path.as_posix():
                continue
            text = path.read_text(encoding='utf-8', errors='replace')
            for m in LABEL.finditer(text):
                out.add(key(m.group('label')))
    return out


def matches(candidate, known):
    """Whether the quoted text is a known setting text. An ellipsis stands for any text, and a shortened name may
    stop before the end of its setting text: "Prioritize Low HP … Big Target" for "Prioritize Low HP targets instead
    of High HP targets when using Big Target and multiple Big targets present."""
    k = key(candidate)
    if k in known:
        return True
    if '…' in k or '...' in k:
        parts = [re.escape(p.strip()) for p in re.split(r'…|\.\.\.', k)]
        pattern = re.compile('^' + '.*'.join(parts))
        return any(pattern.match(label) for label in known)
    return False


def sentence_around(text, start, end):
    """The sentence a quoted span stands in, bounded by a full stop, a blank line or a list item."""
    left = 0
    for m in SENTENCE_END.finditer(text, 0, start):
        left = m.end()
    m = SENTENCE_END.search(text, end)
    right = m.start() if m else len(text)
    return text[left:start] + text[end:right]


def findings(text, known):
    """[(quoted text, reason)] for each quoted text that names a setting no setting has."""
    out = []
    for m in QUOTED.finditer(text):
        candidate = ' '.join((m.group('b') or m.group('q')).split())
        # A setting text starts with a capital and has words; identifiers, file names, log lines do not.
        if not candidate[:1].isupper() or ' ' not in candidate or matches(candidate, known):
            continue
        sentence = sentence_around(text, m.start(), m.end())
        if REMOVED_WORD.search(sentence):
            continue
        close = difflib.get_close_matches(key(candidate), known, n=1, cutoff=NEAR)
        if close:
            out.append((candidate, f'nearly the setting text "{close[0]}"'))
        elif SETTING_WORD.search(sentence):
            out.append((candidate, 'named as a setting, but no setting has this text'))
    return out


def self_test():
    known = {key('Skip mitigation for small area casts'), key('Do not take action if you have one of these debuffs')}
    cases = [
        ('The setting "Hold mitigation under invulnerability" holds it.', 1),
        ('With the setting `Skip mitigation for small area casts` on, it waits.', 0),
        ('The list says "Do not take action if you have one of these debuffs".', 0),
        ('DefenseTrace.log writes "open, nothing chosen" for it.', 0),
        ('It reads `Skip mitigation for small area cast` now.', 1),
        ('The option "Skip mitigation … area casts" is on.', 0),
        ('The option "Skip mitigation … small" is on.', 0),
        ('The option "Skip healing … small" is on.', 1),
        ('The trace line "Sleep resisted by an enemy" names it.\n\nThe setting is on.', 0),
        ('The setting "Target colour of the ring" was removed.', 0),
        ('The setting "Target colour of the ring" is on.', 1),
    ]
    for text, expected in cases:
        got = len(findings(text, known))
        if got != expected:
            print(f'self-test FAILED: {text!r} gave {got} finding(s), expected {expected}')
            return False
    print('self-test ok: a wrong and a nearly right setting name are caught; a right one, an abbreviated one,'
          ' a window text, a quoted log line and a removed setting are left alone')
    return True


def main():
    if not self_test():
        return 1
    known = labels()
    if not known:
        print('no setting texts found - run from the repository root')
        return 1
    bad = 0
    for doc in DOCS:
        if not doc.is_file():
            continue
        for candidate, reason in findings(doc.read_text(encoding='utf-8'), known):
            bad += 1
            print(f'{doc}: "{candidate}" - {reason}')
    if bad:
        print(f'{bad} setting name(s) in the release texts match no setting text.')
        return 1
    print(f'Every setting the release texts name matches a text in the settings window ({len(known)} texts read).')
    return 0


if __name__ == '__main__':
    sys.exit(main())
