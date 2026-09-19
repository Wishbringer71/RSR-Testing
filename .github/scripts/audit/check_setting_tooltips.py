#!/usr/bin/env python3
"""Every setting this fork added has to say what it does in a fight.

A setting is a promise to the user that something changes. The name alone carries that promise
badly, and for a number it cannot carry it at all: "Minimum slowed enemies in Holy's radius before
the slow hold applies" says what is counted and nothing about what a higher or lower value does to
the fight. The user asked for exactly this - the explanation is the effect on the game and the
connection between a value and the game, not a restatement of the label.

Two mechanisms carry it, and they render differently:

  * Configs.cs uses [UI(..., Description = "...")]. The text appears as a tooltip while the setting
    is hovered, and it is gated on the "Show tooltips" setting (default on).
  * Rotation settings use [RotationConfig(..., Tooltip = "...")]. A non-empty tooltip is what makes
    RotationConfigWindow draw the "(?)" marker next to the setting at all - with no tooltip there is
    no marker and nothing to hover. That marker is bypassed by the tooltip toggle and always shows.

Nothing fails when an explanation is missing. The setting works, the window renders, the build is
green, and the only symptom is a user guessing what a slider does to their fight. So it is checked
here.

WHAT THIS CHECKS, STATED HONESTLY: that the question was asked, not that the answer is right. It
requires an explanation to exist and to contain a statement about the fight - either an explicit
effect ("In a fight: ...") or, for a value, what moving it does ("Lower: ... Higher: ..."). A
sentence can satisfy that and still be wrong; only reading the code establishes correctness. The
check catches the failure that actually recurs: a tooltip that restates its own label.

SCOPE, by the project's priority rule: settings outside the user's profile - PvP, Blue Mage, Bozja
and comparable limited content - are reported as recorded rather than demanded. A survey leading
there is its purpose, not a work order.

The self-test carries constructed defects, because a silent pass is otherwise indistinguishable
from a clean tree.
"""

import re
import subprocess
import sys
from pathlib import Path

CONFIGS = 'RotationSolver.Basic/Configuration/Configs.cs'

# The member may be public, private, internal or protected. Requiring `public` is not a harmless
# narrowing: DRK_Reborn carries RotationConfig on a private float, and a pattern that skips it
# attaches that attribute to the NEXT public member, reporting one setting's label under another
# name. That is how the first survey of this class mislabelled BlackestNightUsage.
ACCESS = r'(?:public|private|internal|protected)'

ROTATION_BLOCK = re.compile(
    r'\[RotationConfig\((?P<args>.*?)\)\]\s*'
    r'(?P<member>' + ACCESS + r'[^;{\]]*?)\s*(?:\{|;|=)', re.DOTALL)

UI_BLOCK = re.compile(
    r'\[(?P<attrs>[^\]]*?UI\("(?P<label>(?:[^"\\]|\\.)*)"(?P<args>[^\]]*?))\]\s*'
    r'(?P<member>' + ACCESS + r'[^;{]*?)\s*(?:\{|;|=)', re.DOTALL)

NAME_ARG = re.compile(r'Name\s*=\s*"((?:[^"\\]|\\.)*)"')
# The text may be a concatenation of several string literals, so the whole argument tail is taken
# and the literals joined - matching only the first one would judge a long tooltip by its opening.
TOOLTIP_ARG = re.compile(r'Tooltip\s*=\s*(?P<text>"(?:[^"\\]|\\.)*"(?:\s*\+\s*"(?:[^"\\]|\\.)*")*)')
DESC_ARG = re.compile(r'Description\s*=\s*(?P<text>"(?:[^"\\]|\\.)*"(?:\s*\+\s*"(?:[^"\\]|\\.)*")*)')
LITERAL = re.compile(r'"((?:[^"\\]|\\.)*)"')

# The two forms the user asked for: what changes in the fight, and what a value does to it. The
# second form depends on what kind of value it is - for a number that is "Lower:/Higher:", and for
# a switch it is "On:/Off:", which is the same statement about the same thing. Both are accepted;
# requiring only the first would have rejected a switch whose two states were spelled out properly.
EFFECT = re.compile(r'In a fight\s*:', re.IGNORECASE)
VALUE_EFFECT = re.compile(r'(?:^|\n)\s*(?:Lower|Higher|Raising|Lowering|On|Off)\s*:', re.IGNORECASE)

# Outside the user's profile: recorded, not demanded.
OUT_OF_PROFILE = re.compile(r'(?:PVPRotations|\.PVP\.cs$|BluemageRotation|BlueMage|Bozja)',
                            re.IGNORECASE)


def joined(match):
    """The concatenated text of a string-literal chain as the user reads it, or None.

    The escapes are resolved, because a C# source file carries a line break as the two characters
    backslash-n. Checking the raw literal instead would look for a newline that is never there and
    reject a properly formatted tooltip.
    """
    if match is None:
        return None
    text = ''.join(LITERAL.findall(match.group('text')))
    return text.replace('\\n', '\n').replace('\\"', '"').replace('\\\\', '\\')


def git(*args):
    return subprocess.run(['git', *args], capture_output=True, text=True)


def upstream_base():
    """The upstream state this fork sits on.

    Not the newest tag: the newest tag is a fork release and already contains the fork's own
    settings, so every setting added before it would read as upstream. Falls back to the highest
    upstream release tag - fork tags carry a +wsh suffix and are excluded - and fails loudly when
    neither exists, because a survey that cannot tell fork from upstream must not pass silently.
    """
    if git('rev-parse', '--verify', '--quiet', 'upstream/main').returncode == 0:
        return 'upstream/main'
    tags = [t for t in git('tag', '--sort=-v:refname').stdout.split() if '+' not in t]
    return tags[0] if tags else None


def files_at(ref):
    out = git('ls-tree', '-r', '--name-only', ref).stdout.split('\n')
    return [f for f in out if f.endswith('.cs')]


def read_at(path, ref):
    r = git('show', '%s:%s' % (ref, path))
    return r.stdout if r.returncode == 0 else ''


def rotation_settings(text):
    """{property: (label, explanation)} for one file's rotation settings."""
    found = {}
    for m in ROTATION_BLOCK.finditer(text):
        prop = re.search(r'(\w+)\s*$', m.group('member').replace('{', '').strip())
        if prop is None:
            continue
        args = m.group('args')
        name = NAME_ARG.search(args)
        found[prop.group(1)] = (name.group(1) if name else '', joined(TOOLTIP_ARG.search(args)))
    return found


def ui_settings(text):
    """{property: (label, explanation)} for the global settings in Configs.cs."""
    found = {}
    for m in UI_BLOCK.finditer(text):
        # The captured member stops before `{ get; set; }`, so the name is simply its last word -
        # looking for `<name> { get` finds nothing, which is how an earlier version of this check
        # silently reported no global settings at all.
        tail = re.search(r'(\w+)\s*$', m.group('member').strip())
        if tail is None:
            continue
        name = tail.group(1)
        if name.startswith('_'):
            # ConditionBool backing fields: _showTooltips generates ShowTooltips.
            name = name[1].upper() + name[2:]
        found[name] = (m.group('label'), joined(DESC_ARG.search(m.group('attrs'))))
    return found


def collect(ref=None):
    """All settings, keyed by (file, property)."""
    out = {}
    if ref:
        paths = files_at(ref)
    else:
        paths = [f for f in git('ls-files').stdout.split('\n') if f.endswith('.cs')]

    for path in paths:
        text = read_at(path, ref) if ref else Path(path).read_text(encoding='utf-8')
        if path == CONFIGS:
            for prop, value in ui_settings(text).items():
                out[(path, prop)] = value
        if '[RotationConfig(' in text:
            for prop, value in rotation_settings(text).items():
                out[(path, prop)] = value
    return out


def judge(label, explanation):
    """None when the setting is explained, otherwise why it is not."""
    if not explanation:
        return 'no explanation at all: the user is left with the label'

    effect = EFFECT.search(explanation) or VALUE_EFFECT.search(explanation)
    if effect is None:
        return ('the explanation never says what changes in the fight - no "In a fight:" and no '
                'statement of what a higher or lower value does')

    # The effect statement is the part that was asked for, so it is the part that has to carry new
    # information. Judging the whole text instead would pass "<the label>. Lower: <the label>.",
    # because the marker word alone puts a word outside the label into the set. Compared on words
    # rather than characters, so punctuation and wrapping cannot hide a restatement.
    tail = re.findall(r'\w+', explanation[effect.end():].lower())
    label_words = set(re.findall(r'\w+', label.lower()))
    if label_words and set(tail) <= label_words:
        return 'the explanation only restates the label'
    return None


def self_test():
    good_rot = '''
        [Range(2, 8, ConfigUnitType.None, 1)]
        [RotationConfig(CombatType.PvE, Name = "Slowed enemies before holding",
            Tooltip = "How many enemies have to carry the Slow before the hold applies.\\n"
                + "Lower: the hold triggers while the rest of the pack swings at full speed. "
                + "Higher: Holy keeps being cast into a well-slowed pack.")]
        private int HoldHolyMinSlowedHostiles { get; set; } = 3;
    '''
    got = rotation_settings(good_rot)
    if 'HoldHolyMinSlowedHostiles' not in got:
        raise AssertionError('a private rotation setting was not found')
    label, expl = got['HoldHolyMinSlowedHostiles']
    if judge(label, expl) is not None:
        raise AssertionError('an explained setting was rejected: %s' % judge(label, expl))
    if expl is None or 'well-slowed pack' not in expl:
        raise AssertionError('the concatenated tooltip was truncated to its first literal')

    # The mislabelling defect that the first survey of this class actually had.
    two_members = '''
        [RotationConfig(CombatType.PvE, Name = "Threshold for the option above")]
        private float OblationLanternRatio { get; set; } = 0.5f;

        [RotationConfig(CombatType.PvE, Name = "When to use The Blackest Night")]
        public BlackestNightStrategy BlackestNightUsage { get; set; } = BlackestNightStrategy.Open;
    '''
    pair = rotation_settings(two_members)
    if pair.get('BlackestNightUsage', ('',))[0] != 'When to use The Blackest Night':
        raise AssertionError('a label crossed from a private member to the next public one')

    no_tooltip = rotation_settings(
        good_rot[:good_rot.index(',\n            Tooltip')] + ')]\n        private int '
        'HoldHolyMinSlowedHostiles { get; set; } = 3;')
    label, expl = no_tooltip['HoldHolyMinSlowedHostiles']
    if 'no explanation at all' not in (judge(label, expl) or ''):
        raise AssertionError('a setting with no explanation went unnoticed')

    if 'never says what changes in the fight' not in (
            judge('Hold Holy while the pack is slowed',
                  'Holy is held back while the pack carries a Slow from Arm\'s Length.') or ''):
        raise AssertionError('an explanation without any effect statement went unnoticed')

    # Reaches the restatement test rather than the form test: the marker sits at the start of its
    # own line, so the form is satisfied and only the substance is missing.
    if 'only restates the label' not in (
            judge('Prefer Titan while moving',
                  'Prefer Titan while moving.\nLower: prefer Titan.') or ''):
        raise AssertionError('a tooltip that only restates its label went unnoticed')

    good_ui = '''
        [UI("Skip mitigation for small area casts",
            Description = "A learned area cast whose measured damage leaves everybody above the "
                + "healing level no longer raises the party mitigation.\\n"
                + "In a fight: Reprisal stays ready for the next real hit.",
            Filter = HealingActionCondition, Section = 1)]
        public bool SkipMitigationForSmallAreaCasts { get; set; } = true;
    '''
    got_ui = ui_settings(good_ui)
    if 'SkipMitigationForSmallAreaCasts' not in got_ui:
        raise AssertionError('a global setting was not found')
    if judge(*got_ui['SkipMitigationForSmallAreaCasts']) is not None:
        raise AssertionError('an explained global setting was rejected')

    cond_bool = '''
        [ConditionBool, UI("Show tooltips", Filter = UiInformation)]
        private static readonly bool _showTooltips = true;
    '''
    if 'ShowTooltips' not in ui_settings(cond_bool):
        raise AssertionError('a ConditionBool backing field did not resolve to its property')

    print('self-test ok: an explained setting is accepted; a missing explanation, one with no '
          'effect\n  statement and one that only restates its label are each caught; a private '
          'member is found\n  and its label does not cross to the next; a concatenated tooltip is '
          'read whole; and a\n  ConditionBool field resolves to its generated property')


def main():
    self_test()

    base = upstream_base()
    if base is None:
        print('no upstream reference: add the upstream remote and fetch it, or fetch its release '
              'tags.\nWithout it fork settings cannot be told from upstream ones, and passing '
              'here would be\na silent null result rather than a check.')
        return 1

    current = collect()
    upstream = collect(base)

    # Fork-owned, plus upstream settings this fork relabelled - the relabelling is fork work and
    # carries the same promise. An upstream setting left untouched is upstream's business.
    fork = {}
    for key, value in current.items():
        if key not in upstream:
            fork[key] = value
        elif value[0] != upstream[key][0]:
            fork[key] = value

    problems = []
    recorded = []
    for (path, prop), (label, explanation) in sorted(fork.items()):
        verdict = judge(label, explanation)
        if verdict is None:
            continue
        if OUT_OF_PROFILE.search(path):
            recorded.append((path, prop, verdict))
        else:
            problems.append((path, prop, verdict))

    if recorded:
        print('outside the user profile - recorded, not required:')
        for path, prop, verdict in recorded:
            print('  %s (%s): %s' % (prop, path, verdict))
        print()

    if problems:
        print('fork settings that do not say what they do in a fight (base %s):' % base)
        for path, prop, verdict in problems:
            print('  %s' % prop)
            print('    %s' % path)
            print('    %s' % verdict)
        return 1

    print('%d fork setting(s) measured against %s; every one explains its effect in the fight, '
          'and\n  for a value what moving it does. This checks that the question was asked, not '
          'that the\n  answer is right - only the code establishes that.' % (len(fork), base))
    return 0


if __name__ == '__main__':
    sys.exit(main())
