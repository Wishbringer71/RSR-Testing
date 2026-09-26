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
# The critical class is one definition shared by every reader (A159). GeneralHealTarget may call it
# instead of spelling the threshold out, and then the definition is where the look-ahead is checked.
HELPER = Path('RotationSolver.Basic/Helpers/ObjectHelper.cs')
HELPER_METHOD = re.compile(r'static bool IsInCriticalClass\s*\(')
CLASS_CALL = re.compile(r'IsInCriticalClass\(\)')

# The marks, in the order they have to appear inside GeneralHealTarget. The short-cut patterns
# tolerate either spelling of the health getter, because what they establish here is a position -
# that the critical rank runs first - and a rename must not make that check silently vanish. That
# the forecast spelling is the one in use is a separate question, asked by check_forecast below.
CRITICAL = re.compile(r'HealthForDyingTanks|IsInCriticalClass\(\)')
SELF_CUT = re.compile(r'Get(?:Forecast)?Player(?:Forecast)?HealthRatio\(\)\s*<=\s*'
                      r'Service\.Config\.HealthSelfRatio')
HEALER_CUT = re.compile(r'Get(?:Forecast)?HealthRatio\(\)\s*<=\s*Service\.Config\.HealthHealerRatio')
TANK_CUT = re.compile(r'Get(?:Forecast)?HealthRatio\(\)\s*<=\s*Service\.Config\.HealthTankRatio')
DEAD_FILTER = re.compile(r'o\.IsDead\s*\|\|')

# The forward-looking reads. Each of these decides who gets the heal, and each has a plain
# counterpart that compiles just as well and silently reverts the behaviour to "who is worst off
# now" - which is the whole of what this change is not. HealAheadOfDamage being off makes the two
# identical at runtime; it does not make them identical in the source, which is where this looks.
FORECAST_READS = (
    ('the candidate ordering', re.compile(r'GetForecastHealthRatio\(o\)')),
    ('the critical rank threshold', re.compile(r'GetForecastEffectiveHpPercent\(\)')),
    ('the critical rank ordering', re.compile(r'GetForecastEffectiveHp\(\)')),
    ('the self short-cut', re.compile(r'GetForecastPlayerHealthRatio\(\)')),
)

# The gate ahead of all of them, in the enclosing method rather than in GeneralHealTarget: nothing
# reaches the rank or the short-cuts that this filter has already dropped. AutoHealRatio defaults to
# 0.8, so on the plain getter a tank at 90% heading for 34% never becomes a candidate at all, and
# every check above would still pass while the behaviour is gone.
PREFILTER_METHOD = re.compile(r'IBattleChara\?\s+FindHealTarget\s*\(')
PREFILTER_READ = re.compile(r'o\.GetForecastHealthRatio\(\)\s*<\s*healRatio')
PREFILTER_PLAIN = re.compile(r'o\.GetHealthRatio\(\)\s*<\s*healRatio')
METHOD = re.compile(r'static IBattleChara\?\s+GeneralHealTarget\s*\(')
TTK_METHOD = re.compile(r'bool\s+CheckTimeToKill\s*\(')
TTK_FRIENDLY = re.compile(r'action\.Setting\.IsFriendly')
TTK_CALL = re.compile(r'GetTTK\(\)')


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


def check_time_to_kill(text):
    """CheckTimeToKill must let friendly targets through before it asks GetTTK.

    It asks an attack question - will this target live long enough to be worth the cast - and on a
    friendly target that inverts: the member closest to dying is the one dropped from the candidate
    list, which is the one a heal exists for. Party members are in RecordedHP now, so GetTTK answers
    for them with a real number; before that the NaN branch covered this by accident.
    """
    body = body_of(text, TTK_METHOD)
    if body is None:
        return ['CheckTimeToKill not found - renamed or removed']

    friendly = TTK_FRIENDLY.search(body)
    call = TTK_CALL.search(body)
    if friendly is None:
        return ['CheckTimeToKill no longer exempts friendly targets: a dying party member would be '
                'dropped from the heal candidates']
    if call is not None and call.start() < friendly.start():
        return ['CheckTimeToKill asks GetTTK before exempting friendly targets']
    return []


def check_forecast(text):
    """Every decision in GeneralHealTarget has to read the forecast health, not the current one.

    The two spellings differ by one word and compile alike, so a merge that brings the upstream
    method back, or an edit made without the concept in hand, reverts the look-ahead without
    anything failing. Off by default is not a reason to check less: the setting decides whether the
    forecast differs from the current value, and this decides whether it is asked for at all.
    """
    body = body_of(text, METHOD)
    if body is None:
        return ['GeneralHealTarget not found - the method was renamed or removed']

    problems = []
    for name, pattern in FORECAST_READS:
        # The shared definition stands in for the spelled-out threshold; check_helper asks whether
        # it reads the forecast.
        if name == 'the critical rank threshold' and CLASS_CALL.search(body):
            continue
        if pattern.search(body) is None:
            problems.append('%s no longer reads the forecast health: whoever is falling fastest is '
                            'judged by the health they still have' % name)

    outer = body_of(text, PREFILTER_METHOD)
    if outer is None:
        problems.append('FindHealTarget not found - renamed or removed')
    elif PREFILTER_READ.search(outer) is None:
        problems.append('the AutoHealRatio prefilter no longer reads the forecast health: a member '
                        'above the ratio now but heading below it never becomes a candidate, and '
                        'every check below this one still passes')
    elif PREFILTER_PLAIN.search(outer) is not None:
        problems.append('the AutoHealRatio prefilter has a second, plain-health comparison beside '
                        'the forecast one - one of them decides, and which is not evident here')
    return problems


def check_helper(text):
    """The shared critical-class definition has to read the forecast effective health, the
    threshold and the invulnerability test, for the same reason every read in GeneralHealTarget
    does."""
    body = body_of(text, HELPER_METHOD)
    if body is None:
        return ['IsInCriticalClass not found - renamed or removed']
    problems = []
    if re.search(r'GetForecastEffectiveHpPercent\(\)', body) is None:
        problems.append('IsInCriticalClass no longer reads the forecast health: whoever is falling '
                        'fastest is judged by the health they still have')
    if re.search(r'HealthForDyingTanks', body) is None:
        problems.append('IsInCriticalClass no longer reads HealthForDyingTanks')
    if re.search(r'NoNeedHealingInvuln\(\)', body) is None:
        problems.append('IsInCriticalClass no longer leaves invulnerable members out')
    return problems


def check(text):
    """Returns a list of complaints; empty means the order is intact."""
    body = body_of(text, METHOD)
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
    # Nested the way the tree nests it: GeneralHealTarget is a local function inside FindHealTarget,
    # and the prefilter sits in the outer one, ahead of everything the inner one decides.
    good = '''
        IBattleChara? FindHealTarget(float healRatio)
        {
            foreach (var o in battleChara)
            {
                if (!IBaseAction.AutoHealCheck || o.GetForecastHealthRatio() < healRatio)
                {
                    filteredGameObjects.Add(o);
                }
            }

            static IBattleChara? GeneralHealTarget(List<IBattleChara> objs)
            {
                foreach (var o in objs) { if (o.IsDead || o.HasStatus()) { continue; } }
                ranked.Add((o, o.NoNeedHealingInvuln(), ObjectHelper.GetForecastHealthRatio(o)));
                if (x.GetForecastEffectiveHpPercent() > Service.Config.HealthForDyingTanks * 100f) { }
                var hp = r.Obj.GetForecastEffectiveHp();
                if (ObjectHelper.GetForecastPlayerHealthRatio() <= Service.Config.HealthSelfRatio) { }
                if (healerTar.GetForecastHealthRatio() <= Service.Config.HealthHealerRatio) { }
                if (tankTar.GetForecastHealthRatio() <= Service.Config.HealthTankRatio) { }
            }
        }
    '''
    if check(good):
        raise AssertionError('the intact order was rejected: %s' % check(good))

    critical_line = ('if (x.GetForecastEffectiveHpPercent() > '
                     'Service.Config.HealthForDyingTanks * 100f) { }')
    tank_line = 'if (tankTar.GetForecastHealthRatio() <= Service.Config.HealthTankRatio) { }'

    swapped = good.replace(critical_line + '\n', '')
    swapped = swapped.replace(tank_line, tank_line + '\n                ' + critical_line)
    found = check(swapped)
    if not any('BEFORE the critical rank' in p for p in found):
        raise AssertionError('a short-cut moved ahead of the critical rank went unnoticed: %s'
                             % found)

    no_dead = good.replace('o.IsDead || ', '')
    if not any('drops the dead' in p for p in check(no_dead)):
        raise AssertionError('a candidate list keeping the dead went unnoticed')

    # A match outside the method must not stand in for one inside it.
    outside = good.replace(critical_line + '\n', '')
    outside += '\n void Elsewhere() { var a = Service.Config.HealthForDyingTanks; }'
    if not any('critical rank is gone' in p for p in check(outside)):
        raise AssertionError('a match outside the method was accepted as one inside it')

    # The look-ahead. Each read reverts to its plain counterpart with one word removed, and the
    # plain form compiles, so every one of them needs its own constructed defect.
    if check_forecast(good):
        raise AssertionError('the forward-looking reads were rejected: %s' % check_forecast(good))

    reverts = (
        ('the candidate ordering',
         'ObjectHelper.GetForecastHealthRatio(o)', 'ObjectHelper.GetHealthRatio(o)'),
        ('the critical rank threshold',
         'x.GetForecastEffectiveHpPercent()', 'x.GetEffectiveHpPercent()'),
        ('the critical rank ordering',
         'r.Obj.GetForecastEffectiveHp()', 'r.Obj.GetEffectiveHp()'),
        ('the self short-cut',
         'ObjectHelper.GetForecastPlayerHealthRatio()', 'ObjectHelper.GetPlayerHealthRatio()'),
    )
    for name, forecast, plain in reverts:
        reverted = good.replace(forecast, plain)
        if not any(name in p for p in check_forecast(reverted)):
            raise AssertionError('%s reverted to the current health went unnoticed' % name)
        # Reverting a look-ahead must not disturb the position check, or the two would mask
        # each other and a single edit would report the wrong fault.
        if check(reverted):
            raise AssertionError('reverting %s broke the order check as well: %s'
                                 % (name, check(reverted)))

    # The gate. Reverting it leaves every other forward-looking read in place, so it is the one
    # defect that a check over GeneralHealTarget alone cannot see.
    gate_reverted = good.replace('o.GetForecastHealthRatio() < healRatio',
                                 'o.GetHealthRatio() < healRatio')
    if not any('prefilter no longer reads the forecast' in p
               for p in check_forecast(gate_reverted)):
        raise AssertionError('the AutoHealRatio prefilter reverted to the current health went '
                             'unnoticed')
    if check(gate_reverted):
        raise AssertionError('reverting the prefilter broke the order check as well: %s'
                             % check(gate_reverted))

    both_forms = good.replace(
        'o.GetForecastHealthRatio() < healRatio',
        'o.GetForecastHealthRatio() < healRatio || o.GetHealthRatio() < healRatio')
    if not any('second, plain-health comparison' in p for p in check_forecast(both_forms)):
        raise AssertionError('a plain comparison left beside the forecast one went unnoticed')

    good_ttk = '''
        bool CheckTimeToKill(IBattleChara battleChara)
        {
            if (action.Setting.IsFriendly) { return true; }
            var time = b.GetTTK();
        }
    '''
    if check_time_to_kill(good_ttk):
        raise AssertionError('the guarded CheckTimeToKill was rejected: %s'
                             % check_time_to_kill(good_ttk))
    if not check_time_to_kill(good_ttk.replace('if (action.Setting.IsFriendly) { return true; }', '')):
        raise AssertionError('a CheckTimeToKill without the friendly exemption went unnoticed')
    swapped_ttk = good_ttk.replace(
        'if (action.Setting.IsFriendly) { return true; }\n            var time = b.GetTTK();',
        'var time = b.GetTTK();\n            if (action.Setting.IsFriendly) { return true; }')
    if not check_time_to_kill(swapped_ttk):
        raise AssertionError('GetTTK asked before the friendly exemption went unnoticed')

    # The shared definition in place of the spelled-out threshold.
    shared = good.replace(critical_line, 'if (!r.Obj.IsInCriticalClass()) { }')
    if check(shared) or check_forecast(shared):
        raise AssertionError('the shared critical class was rejected: %s'
                             % (check(shared) + check_forecast(shared)))
    helper_good = '''
        internal static bool IsInCriticalClass(this IBattleChara b)
        {
            return b.NoNeedHealingInvuln()
                && b.GetForecastEffectiveHpPercent() <= Service.Config.HealthForDyingTanks * 100f;
        }
    '''
    if check_helper(helper_good):
        raise AssertionError('the intact critical class was rejected: %s' % check_helper(helper_good))
    for broken, expected in (
        (helper_good.replace('GetForecastEffectiveHpPercent', 'GetEffectiveHpPercent'), 'forecast'),
        (helper_good.replace('Service.Config.HealthForDyingTanks', 'Threshold'), 'HealthForDyingTanks'),
        (helper_good.replace('b.NoNeedHealingInvuln()', 'true'), 'invulnerable'),
    ):
        if not any(expected in p for p in check_helper(broken)):
            raise AssertionError('a critical class without %s went unnoticed' % expected)

    print('self-test ok: the intact order is accepted, a short-cut moved ahead of the critical rank '
          'is caught,\n  a candidate list keeping the dead is caught, a match outside the method '
          'does not count,\n  each of the four forward-looking reads is caught when reverted to the '
          'current health,\n  the AutoHealRatio gate is caught both reverted and left doubled, '
          '\n  CheckTimeToKill is checked for its friendly exemption, and the shared critical class'
          '\n  is checked for the forecast, the threshold and the invulnerable')


def main():
    self_test()
    if not TARGET.exists():
        print('%s not found - run from the repository root' % TARGET)
        return 1
    text = TARGET.read_text(encoding='utf-8')
    problems = check(text) + check_forecast(text) + check_time_to_kill(text)
    if CLASS_CALL.search(body_of(text, METHOD) or ''):
        problems += check_helper(HELPER.read_text(encoding='utf-8'))
    if problems:
        print('heal target order is broken:')
        for p in problems:
            print('  - %s' % p)
        return 1
    print('%s: the critical rank runs ahead of all three short-cuts, the dead stay out, and every\n'
          '  decision reads the health each member is heading for.' % TARGET)
    return 0


if __name__ == '__main__':
    sys.exit(main())
