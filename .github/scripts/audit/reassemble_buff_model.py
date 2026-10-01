#!/usr/bin/env python3
"""What one Reassemble is worth inside a party buff window, against using it outside (concept 17).

Not a code scanner - a model calculator. It answers two questions the owner asked before deciding
whether the Machinist should hold a Reassemble charge for party buffs: which buff gives the
largest gain to a Reassembled hit, and what a held charge gains against what holding costs.

Game data (xivapi, 01.10.2026, effect texts): Divination +6 %, Searing Light, Embolden (party),
Brotherhood, Starry Muse, Technical Finish (four steps) +5 %, Dokumori +5 % damage taken,
Radiant Finale +2/4/6 % by coda, Arcane Circle +3 %, Battle Litany and Chain Stratagem +10 %
critical hit rate, Battle Voice +20 % direct hit rate; all 20 s, recast 120 s (Radiant Finale
110 s). Reassemble: guaranteed critical direct hit, recast 55 s, two charges, "increases damage
dealt when under an effect that raises critical hit rate or direct hit rate". Drill, Air Anchor,
Chain Saw, Excavator: potency 660.

Rule from community sources, not game data (consolegameswiki, Allagan Studies): on a guaranteed
critical hit, a critical hit rate buff adds its rate to the critical damage multiplier. The
matching rule for direct hit is NOT documented in either source; the model leaves Battle Voice's
effect on the Reassembled hit out and says so in its output.

The player's own critical multiplier and rates are ASSUMPTIONS (stats differ per player and
gear); they are parameters, and the result is reported relative to one Reassembled hit so the
order of the buffs - the question - does not hinge on them. The self-test checks the arithmetic
on hand-computed cases and that the order of the buffs holds across the plausible stat range.

Usage: python3 .github/scripts/audit/reassemble_buff_model.py [crit_mult crit_rate dh_rate]
"""
import sys

POTENCY = 660
DH_MULT = 1.25  # direct hit multiplier (Allagan Studies, base CDH 1.25 * crit)

DAMAGE_BUFFS = {  # damage dealt / taken, from the effect texts
    'Divination': 0.06, 'Radiant Finale (3 coda)': 0.06, 'Searing Light': 0.05, 'Embolden': 0.05,
    'Brotherhood': 0.05, 'Starry Muse': 0.05, 'Technical Finish': 0.05, 'Dokumori': 0.05,
    'Arcane Circle': 0.03,
}
CRIT_BUFFS = {'Battle Litany': 0.10, 'Chain Stratagem': 0.10}


def normal_hit(crit_mult, crit_rate, dh_rate, dmg=0.0, crit_buff=0.0, dh_buff=0.0):
    """Expected damage of an unreassembled 660 hit under the given buffs."""
    pc = min(1.0, crit_rate + crit_buff)
    pd = min(1.0, dh_rate + dh_buff)
    return POTENCY * (1 + pc * (crit_mult - 1)) * (1 + pd * (DH_MULT - 1)) * (1 + dmg)


def reassembled_hit(crit_mult, dmg=0.0, crit_buff=0.0):
    """A guaranteed critical direct hit; a crit rate buff adds to the critical multiplier."""
    return POTENCY * (crit_mult + crit_buff) * DH_MULT * (1 + dmg)


def gain_of_moving(crit_mult, crit_rate, dh_rate, dmg=0.0, crit_buff=0.0):
    """Gain of placing the Reassemble inside the buff instead of outside, per moved charge.

    Inside, the Reassembled hit replaces a normal hit that would also have been buffed; outside,
    the Reassembled hit replaced an unbuffed normal hit. The difference of the two differences.
    """
    inside = reassembled_hit(crit_mult, dmg, crit_buff) - normal_hit(crit_mult, crit_rate, dh_rate, dmg, crit_buff)
    outside = reassembled_hit(crit_mult) - normal_hit(crit_mult, crit_rate, dh_rate)
    return inside - outside


def ranking(crit_mult, crit_rate, dh_rate):
    rows = [(name, gain_of_moving(crit_mult, crit_rate, dh_rate, dmg=v)) for name, v in DAMAGE_BUFFS.items()]
    rows += [(name, gain_of_moving(crit_mult, crit_rate, dh_rate, crit_buff=v)) for name, v in CRIT_BUFFS.items()]
    return sorted(rows, key=lambda r: -r[1])


# --- Timeline: where do Reassembles land, with and without holding? -------------------------------
#
# A deliberately plain model of the Machinist's tool GCDs, to answer the antithesis "the tools align
# on the two-minute grid by themselves (40 s and 60 s divide 120 s), so RSR already hits the window".
# GCD 2.5 s, tools used as soon as ready - priority Excavator, Chain Saw, Air Anchor, Drill (an
# ASSUMPTION about RSR's order) - Overheat and other GCDs that delay tools left out (ASSUMPTION:
# drift is ignored, which favours the null variant). Reassemble: two charges, 55 s, one used in the
# countdown on the opening Air Anchor. Game data: Air Anchor 40 s, Chain Saw 60 s (grants Excavator
# Ready), Drill 20 s with two charges.

def simulate(policy, windows, fight=600.0, gcd=2.5):
    """Return (reassembles, reassembles inside a window). policy: 'O0', 'O5' or 'O1'."""
    ready = {'AA': 0.0, 'CS': 0.0}
    drill_charges, drill_progress = 2, 0.0
    excavator = False
    charges, progress = 1, 4.75          # one Reassemble spent in the countdown
    used = inside = 0
    in_window = lambda t: any(a <= t < a + 20 for a in windows)
    next_window = lambda t: min((a for a in windows if a > t), default=None)
    t = 0.0
    while t < fight:
        # recharge Reassemble and Drill up to now
        tool = None
        if excavator:
            tool = 'EXC'
        elif ready['CS'] <= t:
            tool = 'CS'
        elif ready['AA'] <= t:
            tool = 'AA'
        elif drill_charges > 0:
            tool = 'DRILL'
        eligible = tool in ('AA', 'CS', 'EXC') or (tool == 'DRILL' and policy in ('O5', 'O1') and in_window(t))
        if t == 0.0 and tool == 'CS':
            pass
        if eligible and charges > 0:
            hold = False
            if policy == 'O1' and not in_window(t) and charges == 1:
                nw = next_window(t)
                if nw is not None and t + (55.0 - progress) > nw:
                    hold = True          # the window comes before the second charge is full
            if not hold:
                charges -= 1
                used += 1
                inside += in_window(t)
        if tool == 'EXC':
            excavator = False
        elif tool == 'CS':
            ready['CS'] = t + 60.0
            excavator = True
        elif tool == 'AA':
            ready['AA'] = t + 40.0
        elif tool == 'DRILL':
            drill_charges -= 1
        # advance one GCD and recharge
        t += gcd
        if charges < 2:
            progress += gcd
            while progress >= 55.0 and charges < 2:
                charges += 1
                progress -= 55.0
            if charges == 2:
                progress = 0.0
        if drill_charges < 2:
            drill_progress += gcd
            while drill_progress >= 20.0 and drill_charges < 2:
                drill_charges += 1
                drill_progress -= 20.0
            if drill_charges == 2:
                drill_progress = 0.0
    return used, inside


def timeline_report():
    print('\nTimeline, 10 minutes, Reassembles used / inside a 20 s window:')
    for label, offset in (('windows on the two-minute grid from the pull', 0.0),
                          ('windows 30 s off the grid', 30.0), ('windows 70 s off the grid', 70.0)):
        windows = [offset + 120.0 * k for k in range(6)]
        row = '  '.join(f'{pol} {u}/{i}' for pol, (u, i) in ((p, simulate(p, windows)) for p in ('O0', 'O5', 'O1')))
        print(f'  {label:<46} {row}')


def self_test():
    # Hand-computed: crit mult 1.6, crit 25 %, dh 40 %.
    n = normal_hit(1.6, 0.25, 0.40)
    assert abs(n - 660 * 1.15 * 1.10) < 1e-6, n
    r = reassembled_hit(1.6)
    assert abs(r - 660 * 1.6 * 1.25) < 1e-6, r
    # A pure damage buff scales the Reassemble's own value: gain = (r - n) * buff.
    g = gain_of_moving(1.6, 0.25, 0.40, dmg=0.06)
    assert abs(g - (r - n) * 0.06) < 1e-6, g
    # No buff, no gain.
    assert abs(gain_of_moving(1.6, 0.25, 0.40)) < 1e-9
    # The order of the top of the list must not depend on the assumed stats inside a plausible range:
    # the crit rate buffs and Divination ahead of the 5 % buffs, Arcane Circle last.
    for cm in (1.5, 1.6, 1.7):
        for cr in (0.20, 0.25, 0.30):
            for dr in (0.30, 0.40, 0.50):
                names = [x[0] for x in ranking(cm, cr, dr)]
                assert names[-1] == 'Arcane Circle', names
                top = set(names[:4])
                assert {'Battle Litany', 'Chain Stratagem', 'Divination'} <= top, names
    # Holding never costs a Reassemble over the fight: the totals stay within one of each other (the
    # one a hold may carry past the end), and holding never puts fewer into the windows.
    for offset in (0.0, 30.0, 70.0):
        windows = [offset + 120.0 * k for k in range(6)]
        u0, i0 = simulate('O0', windows)
        u1, i1 = simulate('O1', windows)
        assert u0 - u1 <= 1 and i1 >= i0, (offset, u0, i0, u1, i1)
    print('self-test ok: hit formulas, pure damage buff, no-buff case, order stable across stats, holding loses no charge\n')


if __name__ == '__main__':
    self_test()
    cm, cr, dr = (float(x) for x in sys.argv[1:4]) if len(sys.argv) > 3 else (1.6, 0.25, 0.40)
    r = reassembled_hit(cm)
    n = normal_hit(cm, cr, dr)
    print(f'assumed: crit multiplier {cm}, crit rate {cr:.0%}, direct hit rate {dr:.0%}')
    print(f'Reassembled 660 hit {r:.0f}, normal 660 hit {n:.0f}, value of one Reassemble {r - n:.0f}'
          f' ({(r - n) / POTENCY:.0%} of the potency)\n')
    print('gain of placing one Reassemble inside the buff, per moved charge:')
    for name, g in ranking(cm, cr, dr):
        print(f'  {name:<24} {g:6.1f}   ({g / r:.1%} of a Reassembled hit)')
    print('  Battle Voice             not modelled - the direct hit rule for guaranteed direct hits is undocumented')
    timeline_report()
