#!/usr/bin/env python3
"""An emergency heal ability between two hard casts (concept 07, "Die Notfallheilung zwischen zwei Zaubern"; A479).

Not a code scanner - a model calculator, in a closed loop. Two locks keep abilities out of the last half second before
the GCD (CustomRotation.Ability and RSCommands.DoAction, 0.5 s). After a hard-cast spell the time left to the next GCD
is its recast minus its cast, both shortened alike by spell speed and Presence of Mind. The model computes that window
for the healers' hard-cast heals, and replays the tank of 01.11.2026 (Great Gubal Library, his upload) with Benediction
pressed when the GCD came free instead of never - after checking that this moment was the absolute emergency: the
critical class held, Benediction's own line held, the tank would have died before a Cure II begun then landed, and the
next moment an ability could go found him below the dying line.

From the game sheets (xivapi v2, Action, 02.11.2026): cast and recast in seconds.
Measured in his ACT log of 01.11.2026: an instant heal changes the target's HP 0.533 to 0.537 s after its line 21 (26
heals); the tank's HP samples (lines 37 and 39) from 17:27:18.4 to 17:27:23.1, in seconds after 17:27:00; the White
Mage's second Cure II began at 21.277 (line 20), the moment the GCD came free.
ASSUMPTION, not from a source: an instant action's line 21 follows its press by 0.0 to 0.3 s (latency); the replay
reports the range.
"""
import sys

GATE = 0.5                # the lock in Ability and DoAction, seconds before the GCD (fixed_values: A388)
HEALS = {                 # name: (cast, recast) from the Action sheet
    "Cure II": (2.0, 2.5), "Medica II": (2.0, 2.5), "Cure III": (2.0, 2.5),
    "Adloquium": (2.0, 2.5), "Succor": (2.0, 2.5), "Prognosis": (2.0, 2.5),
    "Benefic II": (1.5, 2.5), "Helios": (1.5, 2.5), "Helios Conjunction": (1.5, 2.5), "Diagnosis": (1.5, 2.5),
    "Glare III": (1.5, 2.5), "Holy III": (1.5, 2.5),
}
SPEEDS = {"base": 1.0, "GCD 2.40": 0.96, "GCD 2.40 + Presence of Mind": 0.96 * 0.8}

TANK_MAX = 14752
SAMPLES = [(18.467, 11240), (18.512, 11387), (18.691, 10814), (18.691, 10358), (18.780, 9944), (18.914, 9442),
           (19.315, 8573), (19.672, 7713), (19.761, 7385), (19.806, 6533), (19.806, 5896), (19.895, 5583),
           (20.030, 6264), (20.030, 5938), (20.475, 5623), (20.609, 5294), (20.654, 4821), (20.654, 4510),
           (20.743, 4206), (20.788, 3905), (20.921, 3603), (21.501, 3750), (21.724, 3226), (21.814, 2795),
           (21.947, 1968), (21.947, 1491), (22.170, 712), (22.837, 43)]
GCD_FREE = 21.277
EFFECT_DELAY = 0.535
LATENCIES = (0.0, 0.1, 0.2, 0.3)
CAST = 1.534                    # his Cure II under Presence of Mind that evening (lines 20 to 21)
GCDS = (1.87, 1.96)             # his GCD that evening
DYING_LINE = 0.15               # HealthForDyingTanks, default
BENEDICTION_LINE = 0.30         # "Minimum health threshold ... to use Benediction", default
RATE_WINDOWS = (1.0, 2.0)       # ASSUMPTION: the code's corrected rate lies between these two windows' readings


def window(cast, recast, factor):
    """Seconds an ability may go between two of these casts: the gap after the cast less the lock."""
    return (recast - cast) * factor - GATE


def replay(benediction_at=None):
    """The tank's lowest HP and his HP at the last sample, Benediction restoring him fully at the given moment.

    Damage after the full heal is every HP drop of the samples after it, a rise (a tick) kept as measured."""
    hp, lowest, last_t = None, None, None
    for t, sample in SAMPLES:
        if benediction_at is not None and last_t is not None and last_t < benediction_at <= t:
            hp = TANK_MAX
        if hp is None or benediction_at is None or t <= benediction_at:
            hp = sample
        else:
            hp = max(0, min(TANK_MAX, hp + (sample - prev)))
        prev = sample
        lowest = hp if lowest is None else min(lowest, hp)
        last_t = t
    return lowest, hp


def hp_at(t):
    """The last sample at or before t."""
    return [hp for at, hp in SAMPLES if at <= t][-1]


def rate_at(t, span):
    """HP lost per second over the span before t, from the samples."""
    return (hp_at(t - span) - hp_at(t)) / span


def decision_at_free_gcd():
    """What the two conditions read when the GCD came free: the critical class (health carried forward to the landing
    of a GCD heal begun now, against the dying line) and Benediction's own line (instant: no lock or cast left, so the
    health now)."""
    share = hp_at(GCD_FREE) / TANK_MAX
    ttks = [hp_at(GCD_FREE) / rate_at(GCD_FREE, w) for w in RATE_WINDOWS]
    critical = all(max(0.0, 1 - min(GCDS) / ttk) * share <= DYING_LINE for ttk in ttks)
    return share, ttks, critical, share < BENEDICTION_LINE


def report():
    print("Window for an ability between two hard casts, seconds (negative: none):")
    for name, (cast, recast) in HEALS.items():
        cells = "   ".join(f"{label} {window(cast, recast, f):+.2f}" for label, f in SPEEDS.items())
        print(f"  {name:18s} cast {cast:.1f}   {cells}")
    share, ttks, critical, benediction = decision_at_free_gcd()
    print(f"\nWhen the GCD came free ({GCD_FREE:.2f} s): tank at {share:.1%}, dead in {min(ttks):.2f} to {max(ttks):.2f} s "
          f"at the measured rate, a GCD heal begun then lands after {min(GCDS):.2f} s or more -> critical class: "
          f"{critical}; under Benediction's default line: {benediction}")
    after_cast = GCD_FREE + CAST
    print(f"  The next moment an ability could go had the Cure II begun: its cast end {after_cast:.2f} s, tank at "
          f"{hp_at(after_cast)} HP, and the lock holds it until the GCD at {GCD_FREE + GCDS[0]:.2f} to "
          f"{GCD_FREE + GCDS[1]:.2f} s")
    lowest, end = replay()
    print(f"\nThe tank as it was: lowest {lowest} HP ({lowest / TANK_MAX:.1%}), at 22.84 s {end} HP")
    for latency in LATENCIES:
        at = GCD_FREE + latency + EFFECT_DELAY
        lowest, end = replay(at)
        print(f"  Benediction pressed at {GCD_FREE:.2f} s, latency {latency:.1f} s, heals at {at:.2f} s: lowest "
              f"{lowest} HP ({lowest / TANK_MAX:.0%}), at 22.84 s {end} HP ({end / TANK_MAX:.0%})")


def self_test():
    # Every 2.0-s heal leaves no window at any speed, every 1.5-s one does at base speed.
    for name, (cast, recast) in HEALS.items():
        for f in SPEEDS.values():
            if cast == 2.0:
                assert window(cast, recast, f) <= 0, (name, f)
        if cast == 1.5:
            assert window(cast, recast, 1.0) > 0, name
    # The free GCD was the absolute emergency: the critical class held, Benediction's own line too, and the next
    # moment after a cast found the tank below the dying line.
    share, ttks, critical, benediction = decision_at_free_gcd()
    assert critical and benediction and max(ttks) < CAST + EFFECT_DELAY, (share, ttks)
    assert hp_at(GCD_FREE + CAST) / TANK_MAX < DYING_LINE
    # The replay without Benediction reproduces the log.
    lowest, end = replay()
    assert lowest == 43 and end == 43, (lowest, end)
    # With Benediction at the GCD the tank never falls to the critical line again.
    for latency in LATENCIES:
        lowest, end = replay(GCD_FREE + latency + EFFECT_DELAY)
        assert end > 0.75 * TANK_MAX, (latency, end)
    # A constructed defect is caught: a lock of 0.3 s would open a window after Cure II at base speed.
    global GATE
    saved, GATE = GATE, 0.3
    try:
        assert window(2.0, 2.5, 1.0) > 0
    finally:
        GATE = saved
    print("self-test ok: no window after any 2.0-s heal at any speed, one after 1.5-s heals at base speed, the free GCD "
          "was the last moment for Benediction, the replay reproduces 43 HP, and Benediction then keeps the tank above "
          "75 %")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
