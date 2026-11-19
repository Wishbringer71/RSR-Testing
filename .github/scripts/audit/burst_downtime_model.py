#!/usr/bin/env python3
"""Concept 03, "F · Über den Augenblick hinaus", K1 (A901): a damage buff of the job (d s, recast r s) over a whole fight
with one downtime, used on its recast or held past the downtime, alone or with the party's raid buffs (P, the party's
summed damage share while its buffs lie on you) used on their recast or held as well.

What it shows: the value of the buff is its share of the party damage in its window; 0 in the downtime. A local rule -
hold when the window share f before the downtime times (1 + P) is below 1 - is wrong in a group whose buffs go on their
recast: holding then shifts every later window off theirs. The decision needs the rest of the fight and the party's
observed timing.

Assumptions (named, not measured): the party's raid buffs share one recast with yours and start at the pull; damage is
even outside the downtime; the fight ends at L. Usage: burst_downtime_model.py [--self-test]
"""
import sys


def windows(fight, downtime, duration, recast, policy):
    start, end = downtime
    result, t = [], 0.0
    while t < fight:
        if policy == "hold" and t < end and t + duration > start:
            t = end
        result.append((t, t + duration))
        t += recast
    return result


def value(fight, downtime, party_share, party, mine, duration=20.0, recast=120.0, step=0.5):
    start, end = downtime
    party_windows = windows(fight, downtime, duration, recast, party) if party_share > 0 else []
    own = windows(fight, downtime, duration, recast, mine)
    total, t = 0.0, 0.0
    while t < fight:
        if not start <= t < end and any(a <= t < b for a, b in own):
            total += (1 + (party_share if any(a <= t < b for a, b in party_windows) else 0)) * step
        t += step
    return total


def better(fight, downtime, party_share, party):
    held = value(fight, downtime, party_share, party, "hold")
    used = value(fight, downtime, party_share, party, "recast")
    return "hold" if held > used + 1e-6 else "recast" if used > held + 1e-6 else "equal"


def report():
    print("downtime 30 s starting s seconds into the window at 120 s, fight 600 s, buff 20 s / 120 s")
    print(" s   alone   party on recast (P 0.25)   party holds (P 0.25)   local rule f(1+P)<1")
    failures = 0
    for offset in range(0, 21, 2):
        down = (120.0 + offset, 150.0 + offset)
        alone = better(600, down, 0.0, "recast")
        on_recast = better(600, down, 0.25, "recast")
        holds = better(600, down, 0.25, "hold")
        local = "hold" if offset / 20 * 1.25 < 1 else "recast"
        print(f"{offset:2d}   {alone:6s}  {on_recast:25s}  {holds:21s}  {local}")
    return failures


def self_test():
    # Alone, any share lost to the downtime is worth holding for (no use lost in 600 s).
    assert better(600, (130, 160), 0.0, "recast") == "hold"
    # With a party on its recast, a downtime half way through the window: using now is better - the local rule says hold.
    assert better(600, (130, 160), 0.25, "recast") == "recast"
    assert 10 / 20 * 1.25 < 1, "the local rule would hold here"
    # The downtime right at the window's start: hold, with or without the party.
    assert better(600, (120, 150), 0.25, "recast") == "hold"
    # A party that holds as well: hold.
    assert better(600, (130, 160), 0.25, "hold") == "hold"
    print("self-test ok: alone hold, party on recast mid-window recast (against the local rule), at the start hold, "
          "party holding hold")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
