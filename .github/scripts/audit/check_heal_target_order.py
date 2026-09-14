#!/usr/bin/env python3
"""Guard the order of decisions in the heal target selection.

`ActionTargetInfo.GeneralHealTarget` is upstream code that the fork changes in one specific way: a
critical rank runs ahead of the three short-cuts (self, healer, tank), because each of those returns
outright and would otherwise pass over whoever is actually dying - a damage dealer at 10% loses the
heal to a tank at 44%.

That ordering is the whole of the fix, and nothing fails when it is lost. A merge that brings the
upstream version of this method back, or an edit that moves the block, restores the defect silently.
The candidate list dropping the dead is the same kind of thing: GetHealthRatio returns 0 for a
corpse, so without that filter the critical rank picks a corpse over every living member.

Both are checked here at the source, which is the only place they can be checked without the game.
"""

import re
import sys
from pathlib import Path

TARGET = Path('RotationSolver.Basic/Actions/ActionTargetInfo.cs')

# The marks, in the order they have to appear inside GeneralHealTarget.
CRITICAL = re.compile(r'HealthForDyingTanks')
SELF_CUT = re.compile(r'GetPlayerHealthRatio\(\)\s*<=\s*Service\.Config\.HealthSelfRatio')
HEALER_CUT = re.compile(r'GetHealthRatio\(\)\s*<=\s*Service\.Config\.HealthHealerRatio')
TANK_CUT = re.compile(r'GetHealthRatio\(\)\s*<=\s*Service\.Config\.HealthTankRatio')
DEAD_FILTER = re.compile(r'o\.IsDead\s*\|\|')
METHOD = re.compile(r'static IBattleChara\?\s+GeneralHealTarget\s*\(')


def body_of_general_heal_target(text):
    """The source of GeneralHealTarget alone, so a match elsewhere in the file cannot stand in."""
    start = METHOD.search(text)
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


def check(text):
    """Returns a list of complaints; empty means the order is intact."""
    body = body_of_general_heal_target(text)
    if body is None:
        return ['GeneralHealTarget not found - the method was renamed or removed']

    problems = []
    if DEAD_FILTER.search(body) is None:
        problems.append('the candidate list no longer drops the dead (o.IsDead)')

    critical = CRITICAL.search(body)
    if critical is None:
        problems.append('the critical rank is gone - nothing reads HealthForDyingTanks here')
        return problems

    for name, pattern in (('self', SELF_CUT), ('healer', HEALER_CUT), ('tank', TANK_CUT)):
        cut = pattern.search(body)
        if cut is None:
            problems.append('the %s short-cut is gone - check whether the rank still has a job'
                            % name)
        elif cut.start() < critical.start():
            problems.append('the %s short-cut now runs BEFORE the critical rank: '
                            'whoever is about to die is passed over again' % name)
    return problems


def self_test():
    """Constructed defects, because a silent pass is otherwise indistinguishable from a clean tree."""
    good = '''
        static IBattleChara? GeneralHealTarget(List<IBattleChara> objs)
        {
            foreach (var o in objs) { if (o.IsDead || o.HasStatus()) { continue; } }
            if (x.GetEffectiveHpPercent() > Service.Config.HealthForDyingTanks * 100f) { }
            if (ObjectHelper.GetPlayerHealthRatio() <= Service.Config.HealthSelfRatio) { }
            if (healerTar.GetHealthRatio() <= Service.Config.HealthHealerRatio) { }
            if (tankTar.GetHealthRatio() <= Service.Config.HealthTankRatio) { }
        }
    '''
    if check(good):
        raise AssertionError('the intact order was rejected: %s' % check(good))

    swapped = good.replace(
        'if (x.GetEffectiveHpPercent() > Service.Config.HealthForDyingTanks * 100f) { }\n', '')
    swapped = swapped.replace(
        'if (tankTar.GetHealthRatio() <= Service.Config.HealthTankRatio) { }',
        'if (tankTar.GetHealthRatio() <= Service.Config.HealthTankRatio) { }\n'
        '            if (x.GetEffectiveHpPercent() > Service.Config.HealthForDyingTanks * 100f) { }')
    found = check(swapped)
    if not any('BEFORE the critical rank' in p for p in found):
        raise AssertionError('a short-cut moved ahead of the critical rank went unnoticed: %s'
                             % found)

    no_dead = good.replace('o.IsDead || ', '')
    if not any('drops the dead' in p for p in check(no_dead)):
        raise AssertionError('a candidate list keeping the dead went unnoticed')

    # A match outside the method must not stand in for one inside it.
    outside = good.replace('if (x.GetEffectiveHpPercent() > Service.Config.HealthForDyingTanks * 100f) { }\n', '')
    outside += '\n void Elsewhere() { var a = Service.Config.HealthForDyingTanks; }'
    if not any('critical rank is gone' in p for p in check(outside)):
        raise AssertionError('a match outside the method was accepted as one inside it')

    print('self-test ok: the intact order is accepted, a short-cut moved ahead of the critical rank '
          'is caught,\n  a candidate list keeping the dead is caught, and a match outside the method '
          'does not count')


def main():
    self_test()
    if not TARGET.exists():
        print('%s not found - run from the repository root' % TARGET)
        return 1
    problems = check(TARGET.read_text(encoding='utf-8'))
    if problems:
        print('heal target order is broken:')
        for p in problems:
            print('  - %s' % p)
        return 1
    print('%s: the critical rank runs ahead of all three short-cuts, and the dead stay out.'
          % TARGET)
    return 0


if __name__ == '__main__':
    sys.exit(main())
