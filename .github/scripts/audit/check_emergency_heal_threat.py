#!/usr/bin/env python3
"""Guard the danger condition on the emergency full heal.

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
    ('aggro', re.compile(r'DataCenter\.AggroedMembers\.Contains')),
    ('the announced area cast', re.compile(r'DataCenter\.IsHostileCastingAOE')),
    ('the health trend', re.compile(r'IsNaN\(\s*battleChara\.GetCorrectedTTK\(\)\s*\)')),
)

# The aggro set has to be filled, or its arm answers "nobody is being attacked" forever - a silent
# null result that looks exactly like a safe party.
AGGRO_FILL = re.compile(r'DataCenter\.AggroedMembers\s*=')
AGGRO_SOURCE = re.compile(r'TargetObjectId')


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


def check_aggro(text):
    """The aggro set has to be filled from the hostiles' own targets."""
    fill = AGGRO_FILL.search(text)
    if fill is None:
        return ['nothing fills DataCenter.AggroedMembers: the aggro arm answers "nobody is being '
                'attacked" for the whole fight']
    if AGGRO_SOURCE.search(text) is None:
        return ['the aggro set is filled without reading TargetObjectId - check what it now holds']
    return []


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

    good_threat = '''
        internal static bool IsUnderThreat(this IBattleChara battleChara)
        {
            if (DataCenter.AggroedMembers.Contains(battleChara.GameObjectId)) { return true; }
            if (DataCenter.IsHostileCastingAOE) { return true; }
            return !float.IsNaN(battleChara.GetCorrectedTTK());
        }
    '''
    if check_threat(good_threat):
        raise AssertionError('the intact danger question was rejected: %s'
                             % check_threat(good_threat))

    for name, snippet in (
            ('aggro', 'if (DataCenter.AggroedMembers.Contains(battleChara.GameObjectId)) { return true; }\n'),
            ('the announced area cast', 'if (DataCenter.IsHostileCastingAOE) { return true; }\n'),
            ('the health trend', 'return !float.IsNaN(battleChara.GetCorrectedTTK());\n')):
        missing = good_threat.replace(snippet, '')
        if not any(name in p for p in check_threat(missing)):
            raise AssertionError('a danger question missing %s went unnoticed' % name)

    good_aggro = '''
        var targetId = hostileTargets[i]?.TargetObjectId ?? 0;
        DataCenter.AggroedMembers = aggroed;
    '''
    if check_aggro(good_aggro):
        raise AssertionError('the intact aggro fill was rejected: %s' % check_aggro(good_aggro))
    if not check_aggro(good_aggro.replace('DataCenter.AggroedMembers = aggroed;', '')):
        raise AssertionError('an aggro set that is never filled went unnoticed')

    print('self-test ok: the guarded branch is accepted; an unguarded branch, a condition without '
          'its toggle\n  and a toggle flipped off are each caught; every one of the three danger '
          'arms is caught when\n  removed; and an aggro set that nothing fills is caught')


def main():
    self_test()

    problems = []
    for path, checker in ((WHM, check_branch), (HELPER, check_threat), (UPDATER, check_aggro)):
        if not path.exists():
            print('%s not found - run from the repository root' % path)
            return 1
        problems += checker(path.read_text(encoding='utf-8'))

    if problems:
        print('the emergency heal is no longer tied to actual danger:')
        for p in problems:
            print('  - %s' % p)
        return 1

    print('Benediction asks whether the target is in danger, the question reads aggro, announced '
          'area casts\n  and the health trend, and the aggro set is filled once per frame.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
