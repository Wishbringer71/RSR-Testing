#!/usr/bin/env python3
"""When an area series ends (concept 13, "Eine Serie zählt als ein Angriff"; A480).

Not a code scanner - a model calculator, in a closed loop. Watcher.SeriesShare chains landings of one listed action while
each follows the previous sooner than the action's cast time (Action sheet), whoever cast it, and stores the highest sum a
member took. Two rules are compared:
  TODAY  - the gap rule alone;
  ROUND  - the gap rule, and a targetable caster that already landed in the series and lands again at least a cast time
           after its own previous landing has cast anew: the landing starts a new series. Untargetable casters (mechanic
           objects, helpers) are left to the gap rule.

From his ACT logs of 25.10. to 01.11.2026 (every measured area action replayed, landings of one caster within 0.05 s
merged): the series with more than one caster, as (seconds after the first landing, caster index, targetable). Targetable
is read in the log as "hit by a player" - a stand-in for IsTargetable, which the log does not carry.
ASSUMPTION for the run-on study: a pack whose casters recast every interval +-20 % at a random phase, every member hit by
every landing at the same share; healing does not enter, because the table sums what the attack takes, not what is left.
"""
import random
import sys

LOGGED = {  # name: (cast seconds, [(t, caster, targetable)])
    "Augen auf mich #3358": (3.7, [(0.0, 0, True), (3.56, 1, True)]),
    "Schockwelle #3416 (a)": (3.7, [(0.0, 0, True), (0.18, 1, True), (2.85, 2, True)]),
    "Schockwelle #3416 (b)": (3.7, [(0.0, 0, True), (2.4, 1, True)]),
    "Schlangenpeitsche #5150": (2.7, [(0.0, 0, True), (2.01, 1, True)]),
    "Ersticken des Fleisches #48882": (5.7, [(0.0, 0, False), (3.02, 1, False), (6.03, 2, False), (9.0, 3, False)]),
    "Chemiebombe #44303": (6.7, [(0.0, 0, False), (3.03, 1, False), (6.01, 2, False), (9.04, 3, False)]),
    "Explosion #42286": (8.7, [(0.0, 0, False), (2.54, 1, False), (5.0, 2, False)]),
    "Verfluchter Blick #47152": (4.7, [(t, i, False) for i, t in
                                       enumerate((0.0, 3.03, 6.01, 8.99, 12.02, 15.0, 18.02, 21.0))]),
    "Grosser Meteorit #50186": (5.7, [(0.0, 0, False), (0.0, 1, False)]),
}
SERIES_IN_LOGS = 160  # every series of a measured area action in the six logs, single-caster ones included
SAME_CASTER_AGAIN = 0  # landings of a caster already in its series, in those 160


def series(landings, cast, rule, untargetable_too=False):
    """Split landings (t, caster, targetable) into series; returns the series as lists of landings."""
    out, last, own = [], None, {}
    for t, caster, targetable in landings:
        new = last is None or t - last > cast
        if not new and rule == "ROUND" and caster in own and (targetable or untargetable_too) and t - own[caster] >= cast:
            new = True
        if new:
            out.append([])
            own = {}
        out[-1].append((t, caster, targetable))
        own[caster] = t
        last = t
    return out


def pack_pull(rng, casters, interval, cast, duration):
    landings = []
    for c in range(casters):
        t = rng.uniform(0, interval) + cast
        while t < duration:
            landings.append((t, c, True))
            t += interval * rng.uniform(0.8, 1.2)
    return sorted(landings)


def stored(rng, casters, interval, rule, pulls=200, cast=3.0, duration=45.0, share=0.08):
    """The highest series sum over the pulls - what the table keeps."""
    best = 0.0
    for _ in range(pulls):
        for s in series(pack_pull(rng, casters, interval, cast, duration), cast, rule):
            best = max(best, min(1.0, share * len(s)))
    return best


def report():
    print("His logs, series with more than one caster: series per rule")
    for name, (cast, landings) in LOGGED.items():
        today, rnd = series(landings, cast, "TODAY"), series(landings, cast, "ROUND")
        span = landings[-1][0] - landings[0][0]
        print(f"  {name:32s} {len(landings)} landings over {span:4.1f} s: TODAY {len(today)}, ROUND {len(rnd)}")
    print(f"  all {SERIES_IN_LOGS} series: a caster landing again within its series {SAME_CASTER_AGAIN} times")
    print("\nA pack recasting one area action of 8 % (cast 3 s), 200 pulls of 45 s, highest stored sum:")
    for interval in (8.0, 12.0):
        for casters in (2, 3, 4, 5, 6):
            a = stored(random.Random(7), casters, interval, "TODAY")
            b = stored(random.Random(7), casters, interval, "ROUND")
            print(f"  {casters} casters every {interval:.0f} s: TODAY {a:.0%}, ROUND {b:.0%}")


def self_test():
    # In his logs the round rule changes no series.
    for name, (cast, landings) in LOGGED.items():
        assert len(series(landings, cast, "TODAY")) == len(series(landings, cast, "ROUND")) == 1, name
    # A run-on pack is bounded to one round: the sum stays at casters x share, while today it runs past it.
    for casters in (3, 4, 5, 6):
        assert stored(random.Random(7), casters, 8.0, "ROUND") <= casters * 0.08 + 1e-9, casters
        assert stored(random.Random(7), casters, 8.0, "TODAY") > casters * 0.08 + 1e-9, casters
    # Several hits of one cast (own gap under the cast time) stay one series.
    assert len(series([(0.0, 0, True), (1.0, 0, True), (2.0, 0, True)], 3.0, "ROUND")) == 1
    # Constructed defect: the rule applied to mechanic objects splits a tower sequence whose helper casts twice.
    towers = [(0.0, 0, False), (3.0, 1, False), (6.0, 0, False), (9.0, 2, False)]
    assert len(series(towers, 5.7, "ROUND")) == 1
    assert len(series(towers, 5.7, "ROUND", untargetable_too=True)) == 2
    print("self-test ok: no series in his logs changes, a recasting pack stops at one round, one cast's hits stay "
          "together, and helpers casting twice in a mechanic are not split")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
