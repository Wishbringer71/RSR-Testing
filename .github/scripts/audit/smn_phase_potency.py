#!/usr/bin/env python3
"""Potency accounting for the Summoner phases, single target, level 100.

Answers two questions that the Searing Light concept left open:

1. How much damage does a Solar Bahamut / Bahamut / Phoenix window produce
   compared with an equally long window of the Ifrit / Titan / Garuda blocks,
   with no Searing Light applied?
2. Does pulling the strongest primal block forward, so that it falls inside a
   running Searing Light, pay for itself?

Every potency carries its source.  ``resx`` means the value was read from
``RotationSolver.SourceGenerators/Properties/ActionId.resx`` in this
repository, which mirrors the game's own tooltip text.  ``extern`` means the
tooltip in that file has the number blanked out -- the game writes a
placeholder wherever a trait rewrites the potency -- and the value comes from
a third party instead.  Those values are not verified against a primary
source and are marked as such in every report this script prints.

The phase composition is taken from RotationSolver/RebornRotations/Magical/
SMN_Reborn.cs: the dispatch order in GeneralGCD (UseSummonsAndTrances ->
UsePrimalFollowUps -> SummonPrimals -> UseFillers) and in AttackAbility.
"""

import sys

REPO_RESX = 'resx'
THIRD_PARTY = 'extern'

# --- potency table -------------------------------------------------------
# name -> (potency, source)
POTENCY = {
    # demi fillers and their pet answer
    'Umbral Impulse': (640, REPO_RESX),
    'Luxwave': (160, REPO_RESX),
    'Astral Impulse': (500, THIRD_PARTY),
    'Wyrmwave': (150, REPO_RESX),
    'Fountain of Fire': (580, THIRD_PARTY),
    'Scarlet Flame': (150, REPO_RESX),
    # demi finishers
    'Sunflare': (1000, REPO_RESX),
    'Exodus': (1500, REPO_RESX),
    'Deathflare': (500, REPO_RESX),
    'Akh Morn': (1300, REPO_RESX),
    'Revelation': (1300, REPO_RESX),
    'Rekindle': (0, REPO_RESX),          # cure potency only, no damage
    # primal blocks
    'Inferno': (800, THIRD_PARTY),
    'Earthen Fury': (800, THIRD_PARTY),
    'Aerial Blast': (800, THIRD_PARTY),
    'Ruby Rite': (620, THIRD_PARTY),
    'Topaz Rite': (340, THIRD_PARTY),
    'Emerald Rite': (280, THIRD_PARTY),
    'Crimson Cyclone': (560, THIRD_PARTY),
    'Crimson Strike': (560, THIRD_PARTY),
    'Slipstream': (520, THIRD_PARTY),
    'Mountain Buster': (160, THIRD_PARTY),
    # fillers outside every window
    'Ruin III': (400, THIRD_PARTY),
    'Ruin IV': (520, THIRD_PARTY),
    # abilities RotationSolver parks inside the Solar Bahamut window
    'Energy Drain': (100, REPO_RESX),
    'Necrotize': (500, REPO_RESX),
    'Searing Flash': (700, REPO_RESX),
}

# --- cast times ----------------------------------------------------------
# A buff is applied when the cast snapshots, so an action started inside the
# buff but finishing after it is not buffed.  Only two facts here come from
# this repository, both from SMN_Reborn.cs: Slipstream has a cast time (the
# rotation spends Swiftcast on it, AddSwiftcastOnGaruda) and the Topaz GCDs
# are instant while the Garuda and Ifrit ones are not (option text of
# PreferTitanWhileMoving).  The seconds themselves are third party.
CAST_UNKNOWN = None
CAST_TIME = {
    'Slipstream': (3.0, THIRD_PARTY),
    'Ruby Rite': (CAST_UNKNOWN, THIRD_PARTY),
    'Emerald Rite': (CAST_UNKNOWN, THIRD_PARTY),
    'Ruin III': (CAST_UNKNOWN, THIRD_PARTY),
}


def cast_time(name):
    """Cast time in seconds, or None when it is a cast of unknown length."""
    if name not in CAST_TIME:
        return 0.0
    return CAST_TIME[name][0]


# --- timing --------------------------------------------------------------
GCD = 2.5                # recast assumed for the report
DEMI_DURATION = 15.0     # resx: "Duration: 15s" on all three demi summons
SEARING_LIGHT = 20.0     # resx: "Duration: 20s"
CYCLE = 120.0            # Searing Light recast; two demi rounds
SEARING_LIGHT_BONUS = 0.05   # resx: "Increases damage dealt ... by 5%"


def pot(name):
    return POTENCY[name][0]


# --- phases --------------------------------------------------------------
# A demi window holds DEMI_DURATION / GCD casts.  Whether the summon itself
# eats one of those slots is the one open point in the model: the tooltip of
# all three demi summons says the action "does not share a recast timer with
# any other actions" and, unlike Slipstream, does not add the sentence that
# applies its recast to every other spell, which reads as off-GCD; but
# RotationSolver dispatches the summons from GeneralGCD, which reads as a GCD.
# Both readings are reported.

def demi_window(filler, pet, flow, enkindle, summon_costs_gcd):
    slots = int(round(DEMI_DURATION / GCD))
    casts = slots - 1 if summon_costs_gcd else slots
    total = casts * pot(filler) + casts * pot(pet) + pot(flow) + pot(enkindle)
    return {'gcds': slots, 'casts': casts, 'potency': total}


def demi_phases(summon_costs_gcd):
    return {
        'Solar Bahamut': demi_window('Umbral Impulse', 'Luxwave',
                                     'Sunflare', 'Exodus', summon_costs_gcd),
        'Bahamut': demi_window('Astral Impulse', 'Wyrmwave',
                               'Deathflare', 'Akh Morn', summon_costs_gcd),
        'Phoenix': demi_window('Fountain of Fire', 'Scarlet Flame',
                               'Rekindle', 'Revelation', summon_costs_gcd),
    }


# A primal block is a list of (name, costs_a_gcd).  The summon spell itself
# costs a GCD and makes the egi fire its signature attack; attunement stacks
# come from the summon tooltips in the resx (Ifrit 2, Titan 4, Garuda 4).
PRIMAL_BLOCKS = {
    'Ifrit': [('Inferno', True), ('Crimson Cyclone', True),
              ('Crimson Strike', True), ('Ruby Rite', True), ('Ruby Rite', True)],
    'Titan': [('Earthen Fury', True), ('Topaz Rite', True), ('Topaz Rite', True),
              ('Topaz Rite', True), ('Topaz Rite', True),
              ('Mountain Buster', False)],
    'Garuda': [('Aerial Blast', True), ('Slipstream', True),
               ('Emerald Rite', True), ('Emerald Rite', True),
               ('Emerald Rite', True), ('Emerald Rite', True)],
}

# What is left of the Ifrit block when the summoner stays at range.  Crimson
# Strike only exists because Crimson Cyclone granted it ("Grants Crimson
# Strike Ready", ActionId.resx), so declining the approach costs both, and the
# block shrinks from five GCDs to three.
PRIMAL_BLOCKS_AT_RANGE = dict(PRIMAL_BLOCKS)
PRIMAL_BLOCKS_AT_RANGE['Ifrit'] = [('Inferno', True), ('Ruby Rite', True),
                                   ('Ruby Rite', True)]

# The one-off action each primal grants on top of its two repeatable gemshine
# forms -- the action Astral Flow or the favor turns into.
PRIMAL_SIGNATURE = {
    'Ifrit': ['Crimson Cyclone', 'Crimson Strike'],
    'Titan': ['Mountain Buster'],
    'Garuda': ['Slipstream'],
}


def block_potency(block):
    return sum(pot(n) for n, _ in block)


def block_gcds(block):
    return sum(1 for _, g in block if g)


def primal_sequence(order):
    """Flatten the primal blocks into one action list in the given order."""
    seq = []
    for name in order:
        seq.extend(PRIMAL_BLOCKS[name])
    return seq


WEAVES_PER_GCD = 2


def tail_window(order, gcd=GCD, swiftcast=False, after_demi=True, blocks=None,
                unknown_cast='instant'):
    """Everything that still snapshots inside Searing Light after the demi.

    The swiftcast argument is a comparison, not a recommendation: this fork's
    user keeps Swiftcast for raises and does not spend it in the rotation, so
    the branch that makes a cast instant is there to show what the cast time
    costs, not to propose paying for it.

    The demi holds the GCD from the pull until its 15 s are up; the buff runs
    20 s from the same moment, which is the assumption this makes -- Searing
    Light is woven into the summon, not cast seconds later.  From there the
    primal blocks take over, one action per gcd, and an action counts only if
    it snapshots strictly before the buff ends.
    """
    demi_casts = 0
    if after_demi:
        while demi_casts * gcd < DEMI_DURATION:
            demi_casts += 1
    t = demi_casts * gcd

    hits = []
    unknown = []
    for block_name in order:
        block = (blocks or PRIMAL_BLOCKS)[block_name]
        weave_slots = 0
        for name, costs_gcd in block:
            if not costs_gcd:
                continue
            if t >= SEARING_LIGHT:
                break
            cast = cast_time(name)
            if cast is None:
                unknown.append(name)
                # An unproven cast time is reported either way; 'instant' is
                # the upper bound of what can land inside the buff, a full
                # recast the lower one.
                cast = 0.0 if unknown_cast == 'instant' else gcd
            elif swiftcast and cast > 0.0:
                cast = 0.0
            if t + cast < SEARING_LIGHT:
                hits.append((name, t + cast, pot(name)))
            weave_slots += WEAVES_PER_GCD
            t += gcd
        # off-GCD actions of this block ride along with its GCDs
        for name, costs_gcd in block:
            if costs_gcd or weave_slots <= 0:
                continue
            weave_slots -= 1
            hits.append((name, t - gcd, pot(name)))
        if t >= SEARING_LIGHT:
            break
    return {'demi_casts': demi_casts, 'start': demi_casts * gcd,
            'hits': hits, 'potency': sum(p for _, _, p in hits),
            'unknown_cast': unknown}


def cycle_potency(summon_costs_gcd):
    """Raw potency of one 120 s cycle: two demi rounds plus two primal rounds.

    The order of the primal blocks does not enter here -- the same actions are
    cast either way -- which is what makes the reordering question a pure
    question of buff coverage.
    """
    phases = demi_phases(summon_costs_gcd)
    per_round = phases['Solar Bahamut']['potency']
    second = phases['Bahamut']['potency']
    primal = sum(block_potency(b) for b in PRIMAL_BLOCKS.values())
    gcds_per_round = int(round(60.0 / GCD))
    demi_slots = int(round(DEMI_DURATION / GCD))
    primal_slots = sum(block_gcds(b) for b in PRIMAL_BLOCKS.values())
    spare = gcds_per_round - demi_slots - primal_slots
    filler = max(spare, 0) * pot('Ruin III')
    return per_round + second + 2 * primal + 2 * filler


# --- report --------------------------------------------------------------

def report():
    for summon_costs_gcd in (False, True):
        label = ('summon off the GCD (6 filler casts per demi)'
                 if not summon_costs_gcd
                 else 'summon on the GCD (5 filler casts per demi)')
        print('== %s ==' % label)
        phases = demi_phases(summon_costs_gcd)
        ref = phases['Solar Bahamut']['potency']

        blocks = {n: (block_potency(b), block_gcds(b))
                  for n, b in PRIMAL_BLOCKS.items()}
        primal_total = sum(p for p, _ in blocks.values())
        primal_gcds = sum(g for _, g in blocks.values())
        window_slots = int(round(DEMI_DURATION / GCD))
        primal_window = primal_total / primal_gcds * window_slots

        print('  phase                potency   per GCD   vs Solar   vs primal window')
        for name, d in phases.items():
            print('  %-18s %8d %9.0f %9.0f%% %14.2fx'
                  % (name, d['potency'], d['potency'] / d['gcds'],
                     100.0 * d['potency'] / ref,
                     d['potency'] / primal_window))
        print('  %-18s %8.0f %9.0f %9.0f%% %14.2fx'
              % ('primal (%d s avg)' % DEMI_DURATION, primal_window,
                 primal_total / primal_gcds,
                 100.0 * primal_window / ref, 1.0))
        print()
        print('  primal block         potency   GCDs   per GCD   signature action(s)')
        for name, (p, g) in blocks.items():
            sig = ', '.join('%s %d' % (s, pot(s)) for s in PRIMAL_SIGNATURE[name])
            print('  %-18s %8d %6d %9.0f   %s' % (name, p, g, p / g, sig))
        print()

        # The one-off action of each primal, measured three ways.  Raw potency
        # flatters the two-part Ifrit action; potency per GCD is the rate; the
        # gain over the filler that would otherwise stand in that slot is what
        # the action is actually worth, and an off-GCD action keeps all of it.
        print('  signature action     potency   GCDs   per GCD   gain over Ruin III')
        for name in PRIMAL_BLOCKS:
            actions = PRIMAL_SIGNATURE[name]
            raw = sum(pot(a) for a in actions)
            gcds = sum(1 for a in actions
                       if dict((x, g) for x, g in PRIMAL_BLOCKS[name])[a])
            gain = sum(pot(a) - (pot('Ruin III') if
                                 dict((x, g) for x, g in PRIMAL_BLOCKS[name])[a]
                                 else 0) for a in actions)
            rate = raw / gcds if gcds else float('inf')
            print('  %-18s %8d %6d %9s %10d'
                  % (name, raw, gcds,
                     '%.0f' % rate if gcds else 'no GCD', gain))
        print()

        # Searing Light window: what the order of the primal blocks is worth
        # once the demi has given the GCD back.
        buff_slots = int(round(SEARING_LIGHT / GCD))
        cycle = cycle_potency(summon_costs_gcd)

        # The same question when the buff does not sit on a demi at all --
        # the case V7 creates.  Then the whole buff is primal blocks.
        inside_free = {}
        for order in (('Ifrit', 'Titan', 'Garuda'), ('Titan', 'Garuda', 'Ifrit'),
                      ('Garuda', 'Titan', 'Ifrit')):
            inside_free[order] = tail_window(order, after_demi=False)['potency']
        gain_free = SEARING_LIGHT_BONUS * (max(inside_free.values())
                                           - min(inside_free.values()))
        print('  with the buff entirely outside a demi window (%d GCDs of primal):'
              % buff_slots)
        for order, value in inside_free.items():
            print('    %-24s %5d potency inside the buff' % (' -> '.join(order), value))
        print('  best minus worst is worth %.1f potency, %.3f %% of the cycle.'
              % (gain_free, 100.0 * gain_free / cycle))
        print()

    # How much still fits into the tail of Searing Light, counted on the
    # clock instead of in GCD slots, because a cast that starts inside the
    # buff and lands after it is not buffed.
    print('== what still fits into the tail of Searing Light ==')
    for gcd in (2.50, 2.45, 2.40):
        for swiftcast in (False, True):
            if swiftcast and gcd != 2.50:
                continue
            note = ' (Swiftcast on the cast)' if swiftcast else ''
            first = tail_window(('Ifrit', 'Titan', 'Garuda'), gcd, swiftcast)
            print('  GCD %.2f s%s: the demi takes %d casts and gives the GCD '
                  'back at %.2f s,' % (gcd, note, first['demi_casts'],
                                       first['start']))
            print('  leaving %.2f s of the %.0f s buff.'
                  % (SEARING_LIGHT - first['start'], SEARING_LIGHT))
            for lead in ('Ifrit', 'Titan', 'Garuda'):
                order = tuple([lead] + [x for x in ('Ifrit', 'Titan', 'Garuda')
                                        if x != lead])
                w = tail_window(order, gcd, swiftcast)
                names = ', '.join('%s %d' % (n, p) for n, _, p in w['hits'])
                print('    %-7s first: %d attacks, %5d potency  [%s]'
                      % (lead, len(w['hits']), w['potency'], names))
                if w['unknown_cast']:
                    print('      counted as instant although the cast time is '
                          'not established: %s' % ', '.join(w['unknown_cast']))
            print()

    # The same tail, but with the summoner staying at range.  Declining the
    # Crimson Cyclone approach costs Crimson Strike with it.
    print('== the same tail with the Crimson Cyclone approach declined ==')
    for lead in ('Ifrit', 'Titan', 'Garuda'):
        order = tuple([lead] + [x for x in ('Ifrit', 'Titan', 'Garuda')
                                if x != lead])
        high = tail_window(order, blocks=PRIMAL_BLOCKS_AT_RANGE)
        low = tail_window(order, blocks=PRIMAL_BLOCKS_AT_RANGE,
                          unknown_cast='recast')
        names = ', '.join('%s %d' % (n, p) for n, _, p in high['hits'])
        if high['potency'] == low['potency']:
            print('    %-7s first: %d attacks, %5d potency  [%s]'
                  % (lead, len(high['hits']), high['potency'], names))
        else:
            print('    %-7s first: %d to %d attacks, %d to %d potency  [%s]'
                  % (lead, len(low['hits']), len(high['hits']),
                     low['potency'], high['potency'], names))
            print('      the spread is the unproven cast time of %s: instant '
                  'it lands, a full recast long it does not'
                  % ', '.join(sorted(set(high['unknown_cast']))))
    safe = block_potency(PRIMAL_BLOCKS_AT_RANGE['Ifrit'])
    safe_gcds = block_gcds(PRIMAL_BLOCKS_AT_RANGE['Ifrit'])
    spare = block_gcds(PRIMAL_BLOCKS['Ifrit']) - safe_gcds
    print('  the Ifrit block itself falls from %d potency over %d GCDs to %d '
          'over %d,' % (block_potency(PRIMAL_BLOCKS['Ifrit']),
                        block_gcds(PRIMAL_BLOCKS['Ifrit']), safe, safe_gcds))
    print('  and the %d freed GCDs go back to the filler: %.0f potency per GCD '
          'instead of %.0f.'
          % (spare, (safe + spare * pot('Ruin III'))
             / block_gcds(PRIMAL_BLOCKS['Ifrit']),
             block_potency(PRIMAL_BLOCKS['Ifrit'])
             / block_gcds(PRIMAL_BLOCKS['Ifrit'])))
    print()

    # What RotationSolver additionally parks in the Solar Bahamut window.
    # SMN_Reborn.cs holds Necrotize and Fester behind "inSolarUnique &&
    # HasSearingLight", and Searing Flash only exists because Searing Light
    # granted Ruby's Glimmer, so none of this survives a comparison that
    # switches Searing Light off.
    parked = [('Energy Drain', 1), ('Necrotize', 2), ('Searing Flash', 1)]
    extra = sum(pot(n) * c for n, c in parked)
    solar = demi_phases(False)['Solar Bahamut']['potency']
    print('bound to Searing Light and therefore not part of the comparison above:')
    for name, count in parked:
        print('  %-16s %d x %4d' % (name, count, pot(name)))
    print('  total %d potency, which raises the Solar window from %d to %d, '
          '%.0f %% more' % (extra, solar, solar + extra, 100.0 * extra / solar))
    print()

    unverified = sorted(n for n, (_, s) in POTENCY.items() if s == THIRD_PARTY)
    print('potencies not backed by this repository (tooltip blanked by a trait),')
    print('taken from third-party summaries and unverified:')
    print('  ' + ', '.join(unverified))


# --- self test -----------------------------------------------------------

def self_test():
    # every action the model uses must carry a source
    for block in PRIMAL_BLOCKS.values():
        for name, _ in block:
            if name not in POTENCY:
                raise AssertionError('%s has no potency entry' % name)

    # the GCD budget has to add up: a 60 s round holds 24 GCDs, of which the
    # demi window takes 6 and the three primal blocks 16
    gcds_per_round = int(round(60.0 / GCD))
    demi_slots = int(round(DEMI_DURATION / GCD))
    primal_slots = sum(block_gcds(b) for b in PRIMAL_BLOCKS.values())
    if demi_slots + primal_slots > gcds_per_round:
        raise AssertionError('phase model overbooks the 60 s round: %d + %d > %d'
                             % (demi_slots, primal_slots, gcds_per_round))

    # reordering must not change the raw potency of a cycle -- if it did, the
    # buff-coverage argument below would be measuring the wrong thing
    a = sum(pot(n) for n, _ in primal_sequence(('Ifrit', 'Titan', 'Garuda')))
    b = sum(pot(n) for n, _ in primal_sequence(('Garuda', 'Ifrit', 'Titan')))
    if a != b:
        raise AssertionError('primal order changed the raw potency: %d vs %d' % (a, b))

    # the window accounting must actually read the table: zero out the filler
    # of the Solar window and it has to fall behind Bahamut
    saved = POTENCY['Umbral Impulse']
    try:
        POTENCY['Umbral Impulse'] = (0, REPO_RESX)
        phases = demi_phases(False)
        if phases['Solar Bahamut']['potency'] >= phases['Bahamut']['potency']:
            raise AssertionError('constructed defect not detected: Solar still '
                                 'ahead of Bahamut with a zeroed filler')
    finally:
        POTENCY['Umbral Impulse'] = saved

    # the tail after the demi must hold exactly the first two GCD actions of
    # the block that leads, and nothing more
    expect = pot('Inferno') + pot('Crimson Cyclone')
    if tail_window(('Ifrit', 'Titan', 'Garuda'))['potency'] != expect:
        raise AssertionError('the tail did not match the Ifrit opening')

    # the tail accounting must not drop an off-GCD action of the leading
    # block -- the first version of this function did exactly that, because
    # Mountain Buster is listed behind four Topaz Rites that no longer fit
    titan = tail_window(('Titan', 'Garuda', 'Ifrit'))
    if 'Mountain Buster' not in [n for n, _, _ in titan['hits']]:
        raise AssertionError('the off-GCD action of the leading block was lost')

    # a cast that starts inside the buff but lands after it must not count,
    # and Swiftcast must bring it back
    plain = tail_window(('Garuda', 'Titan', 'Ifrit'))
    swift = tail_window(('Garuda', 'Titan', 'Ifrit'), swiftcast=True)
    if 'Slipstream' in [n for n, _, _ in plain['hits']]:
        raise AssertionError('a cast landing past the buff was counted')
    if 'Slipstream' not in [n for n, _, _ in swift['hits']]:
        raise AssertionError('Swiftcast did not bring the cast back into the buff')
    if swift['potency'] <= plain['potency']:
        raise AssertionError('Swiftcast did not raise the buffed potency')

    # an action of unproven cast time must be named when it reaches the window,
    # so that a silent zero cannot pass for a measured one
    saved = CAST_TIME.get('Crimson Cyclone')
    try:
        CAST_TIME['Crimson Cyclone'] = (CAST_UNKNOWN, THIRD_PARTY)
        probe = tail_window(('Ifrit', 'Titan', 'Garuda'))
        if 'Crimson Cyclone' not in probe['unknown_cast']:
            raise AssertionError('an unproven cast time went unreported')
    finally:
        if saved is None:
            del CAST_TIME['Crimson Cyclone']
        else:
            CAST_TIME['Crimson Cyclone'] = saved

    # a cycle must be worth more than a single demi window, or the percentages
    # printed against it are meaningless
    if cycle_potency(False) <= demi_phases(False)['Solar Bahamut']['potency']:
        raise AssertionError('cycle total is not plausible')

    print('self-test ok: budget, source coverage, order conservation, window '
          'accounting and a constructed defect all check out')
    print()


def main():
    self_test()
    report()
    return 0


if __name__ == '__main__':
    sys.exit(main())
