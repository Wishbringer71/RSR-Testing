#!/usr/bin/env python3
"""How an entry of the old file may be lowered (concept 13, rule 5, "Für die Einträge aus der alten Datei"; A475, A482).

Not a code scanner - a model calculator. An entry taken over from the file before the revisions holds the highest of an
unknown number of readings, imported as one clean reading without spread: rule 5 then needs only two new clean readings
below it by more than a damage roll, and an action that varies - the member it hits hardest takes 4 % one time and 40 %
the next (Donnerfunke, his logs of 01.11.2026) - is lowered without any change in the game. Rules compared for such an
entry, applied to the clean readings of the running game version:
  RULE5   rule 5 as drafted, with the old layer as one reading without spread: at least two new clean readings, spread at
          least the old one's (none), the new highest below the old by more than a roll -> the new highest
  CLEAR   the clear drop of rule 5b for a single old reading: the last two new clean readings within a roll of each
          other, both below the old by more than a roll -> the higher of them
  STEADY  every clean reading of the running version within one roll of the others, at least three of them, all below
          the old by more than a roll -> their highest; a reading outside the roll shows a varying action and stops it
          for good in this version (the decision, A482); STEADY2 the same with two
  NEVER   the old value stands for good
Measured per scenario: the share of casts after the change where a saving decision skips protection for a hit that then
lands at or above the threshold (wrong skip), and, where the game weakened the hit, the share of casts that still spend
protection on a hit below it (kept cover).

Readings carry the game's damage roll, rand[95,105] on every hit taken (Allagan Studies, "How to be a Math Wizard",
Shadowbringers edition; a community source). ASSUMPTIONS, not from a source, named per scenario: how often a varying
action brings its hard variant, how many readings the old file had seen.
"""
import random
import sys

ROLL = (0.95, 1.05)
THRESHOLD = 0.25        # LargeShieldShare, The Blackest Night's 25 % from its effect text
TRIALS = 20000
CASTS = 12
OLD_READINGS = (3, 10, 30)  # ASSUMPTION: readings behind the old maximum
STEADY_READINGS = 3         # the decision of A482: the fewest at which no unchanged case here falls clearly behind NEVER


def fixed(share):
    return lambda rng: share * rng.uniform(*ROLL)


def varying(hard, hard_rate, soft_low, soft_high):
    """The hardest-hit member's share: the hard variant at hard_rate, else a soft one (ASSUMPTION per scenario)."""
    def draw(rng):
        base = hard if rng.random() < hard_rate else rng.uniform(soft_low, soft_high)
        return base * rng.uniform(*ROLL)
    return draw


def stack(rng):
    """A stack of 96 % shared by 8, 6, 4 or 2 (ASSUMPTION: how often each)."""
    return 0.96 / rng.choices((8, 6, 4, 2), (0.5, 0.3, 0.15, 0.05))[0] * rng.uniform(*ROLL)


SCENARIOS = {  # name: (before the change, after it)
    "unchanged fixed 22 %": (fixed(0.22), fixed(0.22)),
    "unchanged fixed 30 %": (fixed(0.30), fixed(0.30)),
    "unchanged varying, hard 56 % one in three": (varying(0.56, 1 / 3, 0.04, 0.40), varying(0.56, 1 / 3, 0.04, 0.40)),
    "unchanged varying, hard 37 % one in four": (varying(0.37, 0.25, 0.10, 0.20), varying(0.37, 0.25, 0.10, 0.20)),
    "unchanged stack of 96 %, shared by 8, 6, 4 or 2": (lambda rng: stack(rng), lambda rng: stack(rng)),
    "patch weakens fixed 40 -> 15 %": (fixed(0.40), fixed(0.15)),
    "patch weakens varying, hard 56 -> 20 %": (varying(0.56, 1 / 3, 0.04, 0.40), varying(0.20, 1 / 3, 0.04, 0.18)),
}


def lowered(old, readings, rule):
    """The value after the clean readings of the running version, under the rule."""
    low = 1 / ROLL[1] * ROLL[0]  # within one roll: the lower at least this share of the higher
    if rule == "RULE5" and len(readings) >= 2 and max(readings) < old * low:
        return max(readings)
    steady = {"STEADY": STEADY_READINGS, "STEADY2": 2}.get(rule)
    if steady and len(readings) >= steady and min(readings) >= max(readings) * low and max(readings) < old * low:
        return max(readings)
    if rule == "CLEAR" and len(readings) >= 2:
        a, b = readings[-2], readings[-1]
        if min(a, b) >= max(a, b) * low and max(a, b) < old * low:
            return max(a, b)
    return old


def play(scenario, rule, old_count, rng):
    """One entry over the casts after the change. A saving decision needs one clean reading of the running version
    first (stage 2); it then reads the higher of the old value and the new readings, or the new readings alone once
    the rule has lowered the old value."""
    before, after = SCENARIOS[scenario]
    old = max(before(rng) for _ in range(old_count))
    readings, lowered_old, wrong, kept, cover_due = [], False, 0, 0, 0
    for _ in range(CASTS):
        hit = after(rng)
        skips = False
        if readings:
            value = max(readings) if lowered_old else max(old, max(readings))
            skips = value < THRESHOLD
        if skips and hit >= THRESHOLD:
            wrong += 1
        if hit < THRESHOLD:
            cover_due += 1
            if not skips:
                kept += 1
        readings.append(hit)
        if not lowered_old and rule != "NEVER" and lowered(old, readings, rule) != old:
            lowered_old = True
    return wrong, kept, cover_due


def rates(scenario, rule, old_count, trials=TRIALS, seed=11):
    rng = random.Random(seed)
    wrong = kept = due = 0
    for _ in range(trials):
        w, k, d = play(scenario, rule, old_count, rng)
        wrong += w
        kept += k
        due += d
    return wrong / (trials * CASTS), (kept / due if due else 0.0)


def report():
    print(f"Wrong skips per cast (hit >= {THRESHOLD:.0%} after a skip) | kept cover on hits below it, {CASTS} casts")
    for scenario in SCENARIOS:
        print(f"  {scenario}")
        for old_count in OLD_READINGS:
            cells = []
            for rule in ("RULE5", "CLEAR", "STEADY2", "STEADY", "NEVER"):
                w, k = rates(scenario, rule, old_count, trials=4000)
                cells.append(f"{rule} {w:5.1%} | {k:5.1%}")
            print(f"    old file of {old_count:2d} readings: " + "   ".join(cells))


def excess():
    """Per rule, the largest number of points it skips wrongly more often than NEVER on an unchanged case."""
    worst = {}
    for name in SCENARIOS:
        if not name.startswith("unchanged"):
            continue
        for old_count in OLD_READINGS:
            never, _ = rates(name, "NEVER", old_count, trials=4000)
            for rule in ("RULE5", "CLEAR", "STEADY2", "STEADY"):
                w, _ = rates(name, rule, old_count, trials=4000)
                worst[rule] = max(worst.get(rule, 0.0), w - never)
    return worst


def self_test():
    unchanged = [name for name in SCENARIOS if name.startswith("unchanged")]
    for name in unchanged:
        never, _ = rates(name, "NEVER", 10, trials=3000)
        steady, _ = rates(name, "STEADY", 10, trials=3000)
        # The decision's measure: STEADY skips wrongly at most half a point more often than never lowering at all.
        assert steady <= never + 0.005, (name, steady, never)
    # Rule 5 as drafted lowers an unchanged varying entry of the old file clearly more often.
    w5, _ = rates("unchanged varying, hard 37 % one in four", "RULE5", 10, trials=3000)
    wn, _ = rates("unchanged varying, hard 37 % one in four", "NEVER", 10, trials=3000)
    assert w5 > wn + 0.02, (w5, wn)
    # A fixed hit the patch weakened: STEADY lets the cover go after three landings, NEVER keeps it for good.
    _, ks = rates("patch weakens fixed 40 -> 15 %", "STEADY", 10, trials=3000)
    _, kn = rates("patch weakens fixed 40 -> 15 %", "NEVER", 10, trials=3000)
    assert ks <= 3 / CASTS + 1e-9 and kn > 0.99, (ks, kn)
    # Constructed defect: STEADY over the last three readings only, not all of the version, lowers the stack more often.
    original = lowered

    def last_three(old, readings, rule):
        if rule == "STEADY":
            return original(old, readings[-STEADY_READINGS:], rule)
        return original(old, readings, rule)

    globals()["lowered"] = last_three
    try:
        wl, _ = rates("unchanged stack of 96 %, shared by 8, 6, 4 or 2", "STEADY", 10, trials=3000)
    finally:
        globals()["lowered"] = original
    ws, _ = rates("unchanged stack of 96 %, shared by 8, 6, 4 or 2", "STEADY", 10, trials=3000)
    assert wl > ws, (wl, ws)
    print("self-test ok: STEADY stays within half a point of never lowering on every unchanged case, rule 5 as drafted "
          "does not, a weakened fixed hit's cover goes after three landings, and reading only the last three is caught")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
        print("Most points of wrong skips above never lowering, over the unchanged cases: "
              + ", ".join(f"{rule} {points * 100:.1f}" for rule, points in excess().items()))
