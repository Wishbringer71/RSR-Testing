#!/usr/bin/env python3
"""How much Searing Light uptime does a party of n Summoners actually get?

The concept in docs/rotation-flow/12-searing-light-stacking.md carried percentages that were
arithmetic done in the head, and one of them was wrong in a way the head does not catch: it said
coverage stays at 33% with the widened cast window, while the section right above it explained that
drift between rotations scatters the windows - which raises coverage. Both cannot be true. This
model settles it by running the rules instead of estimating their outcome.

What it models, all of it taken from the code or from the sources the concept cites:

- Searing Light: 20s effect, 120s recast, does not stack - a second cast replaces the first.
- Demi summons: 15s standing, one every 60s, in the order Solar, Bahamut, Solar, Phoenix. Solar
  therefore comes around every 120s.
- Cast window: today only while Solar stands (SMN_Reborn.cs:203); widened, any demi.
- The guard: a cast is refused while a Searing Light is running and more than `guard_lead` seconds
  remain on it - ActionBasicInfo.IsStatusProvided with StatusRefreshGcdCount = 2, about 5s at a
  2.5s GCD.
- Drift: each Summoner's cycle is offset by a fixed amount, spread over `drift` seconds. Drift 0 is
  a clean synchronised pull; drift 60 is a fight where deaths, movement and stuns have pulled the
  rotations fully apart.

What it does not model: that a buff landing outside the two-minute burst window is worth less than
one inside it. Coverage is not damage. The concept says so; this script only counts seconds.

Usage: python3 .github/scripts/audit/searing_light_coverage.py [--csv]
"""
import sys

BUFF = 20.0
RECAST = 120.0
DEMI_STAND = 15.0
DEMI_EVERY = 60.0
GUARD_LEAD = 5.0
FIGHT = 600.0
STEP = 0.1
# Comparisons need a tolerance in the order of the time grid: one step is STEP/FIGHT of the result,
# and a boundary condition can land on either side of a step. Anything tighter tests the grid, not
# the rules - the first version used 1e-9 and failed on a 0.02 percentage point difference.
TOL = 0.002


def in_window(t, offset, mode):
    """Is this Summoner allowed to cast at time t?

    mode 'solar'  - only while Solar Bahamut stands, which is what SMN_Reborn does today
    mode 'demi'   - while any demi stands (V2)
    mode 'anytime' - whenever a target is up, i.e. the demi tie is dropped entirely (V4)
    mode 'informed' - any demi, plus outside one when no other known Summoner can cover the gap (V5)
    """
    if mode == 'anytime':
        return t >= offset
    if mode == 'informed':
        return t >= offset  # the caller decides; window handling for V5 lives in simulate()
    local = t - offset
    if local < 0:
        return False
    # Which demi are we in, if any?
    since = local % DEMI_EVERY
    if since >= DEMI_STAND:
        return False
    index = int(local // DEMI_EVERY)  # 0 Solar, 1 Bahamut, 2 Solar, 3 Phoenix, ...
    if mode == 'demi':
        return True
    return index % 2 == 0  # Solar sits on every second demi


def simulate(n, mode, drift):
    """Return the fraction of the fight covered by a Searing Light.

    For 'informed' (V5) each Summoner also keeps what every client can actually observe: the status
    carries its source, so once another Summoner has cast, his earliest possible return is known -
    that cast plus the recast. The rule is to hold to the demi windows as in V2, and to cast outside
    one only when nobody else can possibly cover the coming gap.
    """
    offsets = [0.0] * n if n == 1 or drift == 0 else [drift * i / (n - 1) for i in range(n)]
    ready = [0.0] * n          # earliest time each Summoner may cast again
    buff_until = -1.0          # when the running buff expires
    covered = 0.0

    # What the party has observed: earliest possible return per Summoner, or None if never seen.
    # Before a Summoner's first cast nobody knows he exists as a caster - that is the honest state.
    seen_ready = [None] * n

    t = 0.0
    while t < FIGHT:
        if buff_until > t:
            covered += STEP

        remaining = buff_until - t
        blocked = remaining > GUARD_LEAD

        if not blocked:
            for i in range(n):
                if ready[i] > t:
                    continue

                allowed = False
                if mode == 'informed':
                    if in_window(t, offsets[i], 'demi'):
                        allowed = True
                    else:
                        # Outside a demi, two conditions, and the second was learned from the model
                        # rather than assumed. First: no other observed Summoner can possibly take
                        # this gap. Second: the buff must be fully expired, not merely inside the
                        # guard's five-second grace. Casting into those last seconds is what the
                        # guard permits for a refresh, but out here it burns a whole charge for a
                        # few seconds of gain - the first version of this rule did exactly that and
                        # came out *below* V2 at two Summoners.
                        others = [seen_ready[j] for j in range(n) if j != i and seen_ready[j] is not None]
                        nobody_else = all(r > t + GUARD_LEAD for r in others) if others else False
                        allowed = nobody_else and buff_until <= t
                elif in_window(t, offsets[i], mode):
                    allowed = True

                if allowed:
                    buff_until = t + BUFF   # overwrite, never stack
                    ready[i] = t + RECAST
                    seen_ready[i] = t + RECAST   # every client sees the source of the status
                    break

        t += STEP

    return covered / FIGHT


def self_test():
    # One Summoner, no drift: one cast per recast, 20s of buff each - 20/120.
    lone = simulate(1, 'solar', drift=0)
    if not 0.15 < lone < 0.19:
        raise AssertionError('a single Summoner should land near 20/120, got %.3f' % lone)

    # Each widening step must never lower coverage, at any party size or drift.
    for n in range(1, 9):
        for drift in (0, 30, 60):
            narrow = simulate(n, 'solar', drift=drift)
            wide = simulate(n, 'demi', drift=drift)
            if wide < narrow - TOL:
                raise AssertionError('widening lowered coverage at n=%d drift=%d' % (n, drift))

    # V5 must never do worse than V2: it is V2 plus an extra permission.
    for n in range(1, 9):
        for drift in (0, 30, 60):
            if simulate(n, 'informed', drift) < simulate(n, 'demi', drift) - TOL:
                raise AssertionError('informed fell below demi at n=%d drift=%d' % (n, drift))

    # Dropping the demi tie entirely can never do worse than only casting in Solar.
    for n in range(1, 9):
        for drift in (0, 30, 60):
            if simulate(n, 'anytime', drift) < simulate(n, 'solar', drift) - TOL:
                raise AssertionError('anytime fell below solar at n=%d drift=%d' % (n, drift))

    # Deliberately NOT asserted: that more Summoners mean more coverage. They do not, and an earlier
    # version of this test claimed they did. At half drift the widened window gives 84% with seven
    # and 83% with eight, and a weaker "large parties never fall below small ones" fails as well.
    # The cause is greedy allocation and it is real, not a modelling artefact: whoever sits in a
    # window first casts, and no client can see another's cooldowns in order to defer to them. So an
    # extra caster can take a window moments before someone whose own cooldown would have covered a
    # later gap. Any monotonicity test here would have to be loosened until it tested nothing.

    # The ceiling: coverage can never exceed what the charges allow, n * 20s per 120s.
    for n in range(1, 7):
        ceiling = min(1.0, n * BUFF / RECAST)
        for drift in (0, 30, 60):
            for mode in ('solar', 'demi', 'anytime', 'informed'):
                got = simulate(n, mode, drift)
                if got > ceiling + 0.02:
                    raise AssertionError('n=%d exceeded its charge ceiling: %.3f > %.3f'
                                         % (n, got, ceiling))

    print('self-test ok: single Summoner near 20/120, each widening step never lowers coverage, '
          'charge ceiling respected\n')


def main():
    self_test()

    as_csv = '--csv' in sys.argv
    if as_csv:
        print('summoners,drift,solar,demi,anytime,informed')

    for drift, label in ((0, 'synchronised (clean pull)'),
                         (30, 'half drifted'),
                         (60, 'fully drifted (deaths, movement, stuns)')):
        if not as_csv:
            print('%s' % label)
            print('  %-11s %-20s %-15s %-15s %s'
                  % ('Summoners', 'Solar only', 'any demi (V2)', 'anytime (V4)', 'informed (V5)'))
        for n in range(1, 9):
            narrow = simulate(n, 'solar', drift=drift)
            wide = simulate(n, 'demi', drift=drift)
            free = simulate(n, 'anytime', drift=drift)
            informed = simulate(n, 'informed', drift=drift)
            if as_csv:
                print('%d,%d,%.3f,%.3f,%.3f,%.3f' % (n, drift, narrow, wide, free, informed))
            else:
                print('  %-11d %-20s %-15s %-15s %s'
                      % (n, '%.0f%%' % (narrow * 100), '%.0f%%' % (wide * 100),
                         '%.0f%%' % (free * 100), '%.0f%%' % (informed * 100)))
        if not as_csv:
            print()

    if not as_csv:
        print('Charge ceiling: n x 20s per 120s, so 100%% needs six Summoners casting on time.')
        print('Coverage is not damage - a buff outside the two-minute window buffs less of it.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
