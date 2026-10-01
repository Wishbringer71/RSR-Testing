#!/usr/bin/env python3
"""Stun coverage under a given insertion rule, for the concept in docs/rotation-flow/08.

Not a code scanner - a model calculator that simulates the rule GCD by GCD. It answers
whether a rule spends its stun budget well: FFXIV halves the stun duration on the second
application and quarters it on the third, after which the target is immune, so the three
applications are worth 7 s if they never overlap and less if they do.

It lives in the repository because it already found a defect. The concept's first draft
gated the insertion on "remaining > one GCD"; simulating that rule shows it never fires,
because the remainder after the first cast is 1.5 s - less than a GCD - and the whole
gain is lost. Re-run it whenever the assumed GCD length or the stun durations change.

Holy has a cast time, and its stun starts when the cast lands, not when it begins. With the
cast time modelled the question the rule has to ask becomes visible: not "is a stun running",
but "would a Holy started now land while it still runs". A rule that only asks the first
question also yields during the first stun's last 1.5 s and after the 2 s application, where
the next cast lands after the stun anyway, and spends two GCDs for nothing.

Usage: python3 .github/scripts/audit/stun_coverage.py [gcd_seconds] [cast_seconds]
"""
import sys

# Sanctus/Holy applies 4 s, halved on the second application, quartered on the third.
DURATIONS = [4.0, 2.0, 1.0]
HORIZON = 30.0


def simulate(gcd, should_insert, durations=None, horizon=HORIZON, cast=0.0):
    """Step through the GCD grid, applying the rule, and return coverage and casts.

    `should_insert(remaining, applied)` decides whether this GCD goes to something other
    than the stun cast. `remaining` is the stun's remaining time at that moment - including a
    stun whose cast has finished but which has not started yet - and `applied` how many stun
    applications were already spent. A cast started at t lands, and stuns, at t + cast.
    """
    durations = durations or DURATIONS
    intervals, end, applied, inserted, t = [], 0.0, 0, 0, 0.0

    while t < horizon and applied < len(durations):
        remaining = max(0.0, end - t)
        if should_insert(remaining, applied):
            inserted += 1
            t += gcd
            continue
        start = t + cast
        stop = max(start + durations[applied], end) if start <= end else start + durations[applied]
        intervals.append((start, stop))
        end = stop
        applied += 1
        t += gcd

    merged = []
    for start, stop in sorted(intervals):
        if merged and start <= merged[-1][1]:
            merged[-1] = (merged[-1][0], max(merged[-1][1], stop))
        else:
            merged.append((start, stop))
    return sum(b - a for a, b in merged), merged, inserted


# The rules worth comparing.
RULES = {
    'never insert (today)': lambda remaining, applied: False,
    'insert while running (concept)': lambda remaining, applied: remaining > 0.0,
    'insert while the next cast would land inside it (fork)': None,  # needs the cast time
    'insert if remaining > 1 GCD (rejected)': None,  # filled in per gcd below
}


def self_test():
    # Without insertions the second application lands inside the first and loses time.
    total, _, _ = simulate(2.5, lambda r, a: False)
    assert abs(total - 5.5) < 1e-9, total

    # Inserting while a stun runs reaches the full budget of 7 s.
    total, _, inserted = simulate(2.5, lambda r, a: r > 0.0)
    assert abs(total - 7.0) < 1e-9, total
    assert inserted == 1, inserted  # and it costs exactly one GCD, not more

    # The rejected rule never fires at a 2.5 s GCD and falls back to the naive result.
    total, _, inserted = simulate(2.5, lambda r, a: r > 2.5)
    assert inserted == 0 and abs(total - 5.5) < 1e-9, (inserted, total)

    # A shorter GCD does not break the rule: still the full budget, still one insertion.
    total, _, inserted = simulate(2.0, lambda r, a: r > 0.0)
    assert abs(total - 7.0) < 1e-9 and inserted == 1, (total, inserted)

    # A follow-up landing inside a longer running stun must not shorten it.
    total, _, _ = simulate(0.5, lambda r, a: False, durations=[4.0, 2.0])
    assert abs(total - 4.0) < 1e-9, total
    # With Holy's cast time the stun starts a cast later and the question changes. Asking only
    # "is a stun running" also yields while the next cast would land after the stun anyway -
    # once more while the first stun's last 1.5 s run, once after the 2 s application: same
    # coverage, two GCDs more. Asking whether the next cast would land inside it yields once.
    total, _, inserted = simulate(2.5, lambda r, a: False, cast=2.5)
    assert abs(total - 5.5) < 1e-9, total
    total, _, inserted = simulate(2.5, lambda r, a: r > 0.0, cast=2.5)
    assert abs(total - 7.0) < 1e-9 and inserted == 3, (total, inserted)
    total, _, inserted = simulate(2.5, lambda r, a: r > 2.5, cast=2.5)
    assert abs(total - 7.0) < 1e-9 and inserted == 1, (total, inserted)
    # Presence of Mind shortens cast and recast alike; the same rule still spends one GCD.
    total, _, inserted = simulate(2.0, lambda r, a: r > 2.0, cast=2.0)
    assert abs(total - 7.0) < 1e-9 and inserted == 1, (total, inserted)
    print('self-test ok: budget, insertion count, rejected rule, short GCD, cast time\n')


if __name__ == '__main__':
    self_test()
    gcd = float(sys.argv[1]) if len(sys.argv) > 1 else 2.5
    cast = float(sys.argv[2]) if len(sys.argv) > 2 else 0.0
    RULES['insert if remaining > 1 GCD (rejected)'] = lambda r, a, g=gcd: r > g
    RULES['insert while the next cast would land inside it (fork)'] = lambda r, a, c=cast: r > c

    print(f'GCD {gcd:.2f} s, cast {cast:.2f} s, stun durations {DURATIONS}, budget {sum(DURATIONS):.1f} s\n')
    print(f'{"coverage":>9}  {"GCDs spent":>10}  rule')
    for name, rule in RULES.items():
        total, iv, inserted = simulate(gcd, rule, cast=cast)
        print(f'{total:>8.1f}s  {inserted:>10}  {name}')
        print(f'{"":>21}  {" · ".join(f"{a:.1f}-{b:.1f}" for a, b in iv)}')
