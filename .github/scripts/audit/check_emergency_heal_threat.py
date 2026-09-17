#!/usr/bin/env python3
"""Guard the decisions that hang on measured danger rather than on a set number.

Two rules in WHM_Reborn ask that question, and both are one word away from deciding on a figure
again: the emergency full heal, and the bound on the Sanctus hold.


The user reported Benediction going out on a player the moment he was raised. A resurrected player
holds a few percent, carries no aggro and is taking no damage, so a health threshold reads him as
the most urgent member in the party while nothing is happening to him - and the once-per-90s full
heal is gone when the tank next needs it.

His requirement is about danger rather than about raising: the emergency heal is right when danger
is coming, and where there is no aggro, no announced area cast and no damage arriving, a HoT and
the smaller heals are enough. WHM_Reborn asks that through ObjectHelper.IsUnderThreat.

Nothing fails when the condition is lost. The branch compiles perfectly well without it, an upstream
merge brings the unguarded version back, and the only symptom is a wasted cooldown that nobody
notices until a tank dies two minutes later. So it is checked here, at the source.

The self-test carries constructed defects, because a silent pass is otherwise indistinguishable
from a clean tree.
"""

import re
import sys
from pathlib import Path

WHM = Path('RotationSolver/RebornRotations/Healer/WHM_Reborn.cs')
HELPER = Path('RotationSolver.Basic/Helpers/ObjectHelper.cs')
UPDATER = Path('RotationSolver/Updaters/TargetUpdater.cs')

BENEDICTION_BRANCH = re.compile(
    r'BenedictionPvE\.CanUse\(out act\).*?\)\s*\{', re.DOTALL)
THREAT_CALL = re.compile(r'BenedictionPvE\.Target\.Target\.IsUnderThreat\(\)')
THREAT_TOGGLE = re.compile(r'!BenedictionNeedsThreat\s*\|\|')
TOGGLE_DECL = re.compile(r'bool\s+BenedictionNeedsThreat\s*\{\s*get;\s*set;\s*\}\s*=\s*(\w+)')

# The three arms of the danger question. Losing any one of them narrows the rule silently: without
# aggro a tank being beaten on reads as safe, without the area cast the announced hit is missed, and
# without the trend anything that neither casts nor retargets is missed.
THREAT_METHOD = re.compile(r'bool\s+IsUnderThreat\s*\(')
THREAT_ARMS = (
    ('who an enemy is aiming at', re.compile(r'DataCenter\.TargetedPartyMembers\.Contains')),
    ('the announced area cast', re.compile(r'DataCenter\.IsHostileCastingAOE')),
    ('the health trend', re.compile(r'IsNaN\(\s*battleChara\.GetCorrectedTTK\(\)\s*\)')),
)

# The set has to be filled, or its arm answers "nobody is being aimed at" forever - a silent null
# result that looks exactly like a safe party. Both sources are required: the attack target is
# aggro, the cast target is what is about to land, and a boss beating on the tank while casting at
# a caster is the case that separates them.
SET_FILL = re.compile(r'DataCenter\.TargetedPartyMembers\s*=')
SET_SOURCES = (
    ('the attack target', re.compile(r'hostile\.TargetObjectId')),
    ('the cast target', re.compile(r'hostile\.CastTargetObjectId')),
)
# And it has to be cleared where the rest of the frame state is, or it names whoever was last under
# fire for as long as the party stays out of combat.
SET_CLEAR = re.compile(r'DataCenter\.TargetedPartyMembers\.Clear\(\)')

# The other rule in this file that used to decide on an invented number. The user's bound on the
# Sanctus hold is that the incoming damage must stay manageable; that was a made-up enemy-output
# figure and is now the measured question "does anyone actually fall inside the GCD this costs".
# Losing the call puts the invented number back in charge, and the optional ceiling must stay off by
# default or it decides again on a figure nobody measured.
HOLD_METHOD = re.compile(r'bool\s+ShouldHoldHolyWhilePackSlowed\s*\(')
HOLD_MEASURED = re.compile(r'ObjectHelper\.AnyPartyMemberFallingWithinHealWindow\(\)')
OUTPUT_CAP_DECL = re.compile(r'int\s+HoldHolyMaxHostileOutput\s*\{\s*get;\s*set;\s*\}\s*=\s*(\d+)')


def body_of(text, pattern):
    """The source of one method, so a match elsewhere in the file cannot stand in."""
    start = pattern.search(text)
    if start is None:
        return None
    i = text.index('{', start.end())
    depth = 0
    for j in range(i, len(text)):
        if text[j] == '{':
            depth += 1
        elif text[j] == '}':
            depth -= 1
            if depth == 0:
                return text[i:j + 1]
    return None


def check_branch(text):
    """The Benediction branch must ask the danger question, behind its toggle."""
    branch = BENEDICTION_BRANCH.search(text)
    if branch is None:
        return ['the Benediction branch was not found - renamed or removed']

    condition = branch.group(0)
    problems = []
    if THREAT_CALL.search(condition) is None:
        problems.append('Benediction no longer asks whether the target is in danger: a player who '
                        'was just raised takes the once-per-cooldown full heal')
    elif THREAT_TOGGLE.search(condition) is None:
        problems.append('the danger condition is no longer behind BenedictionNeedsThreat - it '
                        'cannot be turned off any more')

    decl = TOGGLE_DECL.search(text)
    if decl is None:
        problems.append('BenedictionNeedsThreat is not declared')
    elif decl.group(1) != 'true':
        problems.append('BenedictionNeedsThreat no longer defaults to true, so the reported '
                        'behaviour is back for anyone who does not change a setting')
    return problems


def check_hold_bound(text):
    """The Sanctus hold's bound has to be the measured one, not the invented number."""
    body = body_of(text, HOLD_METHOD)
    if body is None:
        return ['ShouldHoldHolyWhilePackSlowed not found - renamed or removed']

    problems = []
    if HOLD_MEASURED.search(body) is None:
        problems.append('the hold no longer asks whether anyone is actually falling: its bound is '
                        'back to a set number of enemies, which a pull the healing keeps up with '
                        'trips for no reason')

    decl = OUTPUT_CAP_DECL.search(text)
    if decl is None:
        problems.append('HoldHolyMaxHostileOutput is not declared')
    elif decl.group(1) != '0':
        problems.append('HoldHolyMaxHostileOutput no longer defaults to 0, so an unmeasured output '
                        'figure decides again')
    return problems


def check_threat(text):
    """All three arms of the danger question have to be there."""
    body = body_of(text, THREAT_METHOD)
    if body is None:
        return ['IsUnderThreat not found - renamed or removed']

    problems = []
    for name, pattern in THREAT_ARMS:
        if pattern.search(body) is None:
            problems.append('IsUnderThreat no longer reads %s, so that danger goes unseen' % name)
    return problems


def check_targets(text):
    """The set has to be filled from both target sources, and cleared with the rest of the state."""
    problems = []
    if SET_FILL.search(text) is None:
        problems.append('nothing fills DataCenter.TargetedPartyMembers: the first arm answers '
                        '"nobody is being aimed at" for the whole fight')
        return problems

    for name, pattern in SET_SOURCES:
        if pattern.search(text) is None:
            problems.append('the set no longer reads %s, so a member threatened only that way '
                            'reads as safe' % name)

    if SET_CLEAR.search(text) is None:
        problems.append('the set is never cleared on the no-targets path: it keeps naming whoever '
                        'was last under fire after the fight ends')
    return problems


def self_test():
    good_branch = '''
        public bool BenedictionNeedsThreat { get; set; } = true;

        if (BenedictionPvE.CanUse(out act) &&
            BenedictionPvE.Target.Target.GetHealthRatio() < BenedictionHeal &&
            (!BenedictionNeedsThreat || BenedictionPvE.Target.Target.IsUnderThreat()))
        {
            return true;
        }
    '''
    if check_branch(good_branch):
        raise AssertionError('the guarded branch was rejected: %s' % check_branch(good_branch))

    unguarded = good_branch.replace(
        ' &&\n            (!BenedictionNeedsThreat || BenedictionPvE.Target.Target.IsUnderThreat())',
        '')
    if not any('no longer asks whether the target is in danger' in p
               for p in check_branch(unguarded)):
        raise AssertionError('an unguarded Benediction branch went unnoticed')

    not_toggleable = good_branch.replace(
        '(!BenedictionNeedsThreat || BenedictionPvE.Target.Target.IsUnderThreat())',
        'BenedictionPvE.Target.Target.IsUnderThreat()')
    if not any('no longer behind BenedictionNeedsThreat' in p
               for p in check_branch(not_toggleable)):
        raise AssertionError('a danger condition without its toggle went unnoticed')

    off_by_default = good_branch.replace(
        'BenedictionNeedsThreat { get; set; } = true;',
        'BenedictionNeedsThreat { get; set; } = false;')
    if not any('no longer defaults to true' in p for p in check_branch(off_by_default)):
        raise AssertionError('a toggle flipped to off by default went unnoticed')

    good_hold = '''
        public int HoldHolyMaxHostileOutput { get; set; } = 0;

        private bool ShouldHoldHolyWhilePackSlowed()
        {
            if (ObjectHelper.AnyPartyMemberFallingWithinHealWindow()) { return false; }
            if (HoldHolyMaxHostileOutput > 0) { }
            return true;
        }
    '''
    if check_hold_bound(good_hold):
        raise AssertionError('the measured bound was rejected: %s' % check_hold_bound(good_hold))
    if not any('no longer asks whether anyone is actually falling' in p for p in check_hold_bound(
            good_hold.replace(
                'if (ObjectHelper.AnyPartyMemberFallingWithinHealWindow()) { return false; }', ''))):
        raise AssertionError('a hold back on the invented number went unnoticed')
    if not any('no longer defaults to 0' in p for p in check_hold_bound(
            good_hold.replace('HoldHolyMaxHostileOutput { get; set; } = 0;',
                              'HoldHolyMaxHostileOutput { get; set; } = 600;'))):
        raise AssertionError('an output cap switched back on by default went unnoticed')

    good_threat = '''
        internal static bool IsUnderThreat(this IBattleChara battleChara)
        {
            if (DataCenter.TargetedPartyMembers.Contains(battleChara.GameObjectId)) { return true; }
            if (DataCenter.IsHostileCastingAOE) { return true; }
            return !float.IsNaN(battleChara.GetCorrectedTTK());
        }
    '''
    if check_threat(good_threat):
        raise AssertionError('the intact danger question was rejected: %s'
                             % check_threat(good_threat))

    for name, snippet in (
            ('who an enemy is aiming at',
             'if (DataCenter.TargetedPartyMembers.Contains(battleChara.GameObjectId)) { return true; }\n'),
            ('the announced area cast', 'if (DataCenter.IsHostileCastingAOE) { return true; }\n'),
            ('the health trend', 'return !float.IsNaN(battleChara.GetCorrectedTTK());\n')):
        missing = good_threat.replace(snippet, '')
        if not any(name in p for p in check_threat(missing)):
            raise AssertionError('a danger question missing %s went unnoticed' % name)

    good_targets = '''
        DataCenter.TargetedPartyMembers.Clear();
        if (partyIds.Contains(hostile.TargetObjectId)) { }
        if (partyIds.Contains(hostile.CastTargetObjectId)) { }
        DataCenter.TargetedPartyMembers = targeted;
    '''
    if check_targets(good_targets):
        raise AssertionError('the intact target set was rejected: %s' % check_targets(good_targets))

    if not any('nothing fills' in p for p in check_targets(
            good_targets.replace('DataCenter.TargetedPartyMembers = targeted;', ''))):
        raise AssertionError('a target set that is never filled went unnoticed')

    if not any('the cast target' in p for p in check_targets(
            good_targets.replace('if (partyIds.Contains(hostile.CastTargetObjectId)) { }', ''))):
        raise AssertionError('a target set that ignores the cast target went unnoticed')

    if not any('the attack target' in p for p in check_targets(
            good_targets.replace('if (partyIds.Contains(hostile.TargetObjectId)) { }', ''))):
        raise AssertionError('a target set that ignores the attack target went unnoticed')

    if not any('never cleared' in p for p in check_targets(
            good_targets.replace('DataCenter.TargetedPartyMembers.Clear();', ''))):
        raise AssertionError('a target set that is never cleared went unnoticed')

    print('self-test ok: the guarded branch is accepted; an unguarded branch, a condition without '
          'its toggle\n  and a toggle flipped off are each caught; every one of the three danger '
          'arms is caught when\n  removed; a target set that is unfilled, missing either source, or '
          'never cleared is caught;\n  and the Sanctus hold falling back on the invented output '
          'number is caught')


def main():
    self_test()

    problems = []
    for path, checker in ((WHM, check_branch), (WHM, check_hold_bound), (HELPER, check_threat),
                          (UPDATER, check_targets)):
        if not path.exists():
            print('%s not found - run from the repository root' % path)
            return 1
        problems += checker(path.read_text(encoding='utf-8'))

    if problems:
        print('the emergency heal is no longer tied to actual danger:')
        for p in problems:
            print('  - %s' % p)
        return 1

    print('Benediction asks whether the target is in danger; the question reads who an enemy is '
          'aiming at,\n  announced area casts and the health trend; and the target set is filled '
          'from both sources\n  once per frame and cleared with the rest of the state.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
