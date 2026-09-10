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

Usage: python3 .github/scripts/audit/stun_coverage.py [gcd_seconds]
"""
import sys

# Sanctus/Holy applies 4 s, halved on the second application, quartered on the third.
DURATIONS = [4.0, 2.0, 1.0]
HORIZON = 30.0


def simulate(gcd, should_insert, durations=None, horizon=HORIZON):
    """Step through the GCD grid, applying the rule, and return coverage and casts.

    `should_insert(remaining, applied)` decides whether this GCD goes to something other
    than the stun cast. `remaining` is the running stun's remaining time at that moment,
    `applied` how many stun applications were already spent.
    """
    durations = durations or DURATIONS
    intervals, end, applied, inserted, t = [], 0.0, 0, 0, 0.0

    while t < horizon and applied < len(durations):
        remaining = max(0.0, end - t)
        if should_insert(remaining, applied):
            inserted += 1
            t += gcd
            continue
        stop = max(t + durations[applied], end) if t <= end else t + durations[applied]
        intervals.append((t, stop))
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
    print('self-test ok: budget, insertion count, rejected rule, short GCD\n')


if __name__ == '__main__':
    self_test()
    gcd = float(sys.argv[1]) if len(sys.argv) > 1 else 2.5
    RULES['insert if remaining > 1 GCD (rejected)'] = lambda r, a, g=gcd: r > g

    print(f'GCD {gcd:.2f} s, stun durations {DURATIONS}, budget {sum(DURATIONS):.1f} s\n')
    print(f'{"coverage":>9}  {"GCDs spent":>10}  rule')
    for name, rule in RULES.items():
        total, iv, inserted = simulate(gcd, rule)
        print(f'{total:>8.1f}s  {inserted:>10}  {name}')
        print(f'{"":>21}  {" · ".join(f"{a:.1f}-{b:.1f}" for a, b in iv)}')
