#!/usr/bin/env python3
"""Melee gap closers: the dash at the end of a countdown, and the dash into a gathered pack (concept 04, "Countdown und
Notfallslot", A347).

Not a code scanner - a model calculator. Two questions the owner set (25.10.2026: "Monk ist nahkämpfer, daher sollte
er im angriffsrange stehen ... Gerade wenn man Gegner explizit bei Wall-to-wall so anspringt, dass man mittig für aoe
Angriffe rauskommt, kann es Sinn machen"):

A. Countdown: how much later the first weaponskill lands when a melee stands d yalms from the boss at the pull, walking
   in, against a target dash pressed 0.5 s before the pull (The Balance, Viper opener: Slither at -0.5 s; upstream's
   Thunderclap at <= 0.5 s), and what the dash costs when he already stands in range.
B. Wall-to-wall: of a gathered pack, how many enemies a melee area attack (5 y around the player, to the target's
   hitbox - job guide, Arm of the Destroyer, Steel Maw: "all nearby enemies", 5y) reaches when he walks up and stops at
   the first enemy in reach, against a dash to the enemy that has the most others around it, and against a dash to a
   random enemy (RSR's commanded dash goes to the hard target).

Game values: job guide (patch 7.5): melee weaponskills 3 y, target dashes 20 y (Thunderclap, Slither, Winged Glide:
"Rush to a targeted enemy's location"), area attacks 5 y. ASSUMPTIONS, not documented in a source found (24.10.2026):
run speed (no figure published; varied 5-7 y/s), a dash's animation lock (varied 0.5-0.9 s), pack radius and hitboxes.
Results are reported across those ranges.

The self-test checks the arithmetic on hand-computed cases and that the pack geometry counts a constructed layout
right; a dash that always lands in reach must never cover fewer enemies than the walk to the first one.

Usage: python3 .github/scripts/audit/melee_gapcloser_model.py
"""
import math
import random

MELEE = 3.0      # weaponskill range, yalms (job guide)
DASH = 20.0      # target dash range (job guide)
AREA = 5.0       # area attack radius around the player (job guide)
PRESS = 0.5      # dash pressed this long before the pull (The Balance, Viper opener; upstream Thunderclap)


def first_gcd_walk(distance, speed):
    """Seconds after the pull until the first weaponskill when he walks in from `distance` (hitbox to hitbox)."""
    return max(0.0, (distance - MELEE) / speed)


def first_gcd_dash(distance, lock, speed, only_out_of_range):
    """Seconds after the pull until the first weaponskill with a dash pressed PRESS before it.

    Upstream presses it whatever the distance; the proposal only out of melee range. Beyond the dash's range he walks.
    """
    if distance > DASH or (only_out_of_range and distance <= MELEE):
        return first_gcd_walk(distance, speed)
    return max(0.0, lock - PRESS)


def countdown_table():
    rows = []
    for distance in (0, 2, 3, 5, 8, 12, 20):
        for speed in (5.0, 6.0, 7.0):
            for lock in (0.5, 0.7, 0.9):
                rows.append((distance, speed, lock, first_gcd_walk(distance, speed),
                             first_gcd_dash(distance, lock, speed, False), first_gcd_dash(distance, lock, speed, True)))
    return rows


def reached(player, mobs):
    """Enemies an area attack around `player` reaches: centre distance minus hitbox within AREA."""
    return sum(1 for (x, y, h) in mobs if math.hypot(x - player[0], y - player[1]) - h <= AREA)


def landing(mob, origin):
    """Where a target dash ends: at the target's hitbox, on the side he came from."""
    x, y, h = mob
    dx, dy = origin[0] - x, origin[1] - y
    d = math.hypot(dx, dy) or 1.0
    return (x + dx / d * h, y + dy / d * h)


def walk_stop(mobs, origin):
    """Walking straight at the pack centre, he stops at the first enemy within melee range."""
    cx = sum(m[0] for m in mobs) / len(mobs)
    cy = sum(m[1] for m in mobs) / len(mobs)
    dx, dy = cx - origin[0], cy - origin[1]
    d = math.hypot(dx, dy)
    ux, uy = dx / d, dy / d
    step = 0.1
    t = 0.0
    while t < d:
        p = (origin[0] + ux * t, origin[1] + uy * t)
        if any(math.hypot(m[0] - p[0], m[1] - p[1]) - m[2] <= MELEE for m in mobs):
            return p
        t += step
    return (cx, cy)


def pack_trial(rng, count, radius, approach):
    mobs = []
    for _ in range(count):
        r = radius * math.sqrt(rng.random())
        a = rng.random() * 2 * math.pi
        mobs.append((r * math.cos(a), r * math.sin(a), rng.uniform(0.5, 1.5)))
    a = rng.random() * 2 * math.pi
    origin = (approach * math.cos(a), approach * math.sin(a))
    walked = reached(walk_stop(mobs, origin), mobs)
    central = max(mobs, key=lambda m: reached((m[0], m[1]), mobs))
    dashed = reached(landing(central, origin), mobs)
    random_dash = reached(landing(rng.choice(mobs), origin), mobs)
    return walked, dashed, random_dash


def pack_table(trials=4000, seed=7):
    rng = random.Random(seed)
    rows = []
    for count in (3, 5, 8):
        for radius in (2.0, 4.0, 6.0):
            totals = [0, 0, 0]
            for _ in range(trials):
                for i, v in enumerate(pack_trial(rng, count, radius, 15.0)):
                    totals[i] += v
            rows.append((count, radius, *(t / trials for t in totals)))
    return rows


def self_test():
    # Hand-computed: 8 y away at 6 y/s walks (8 - 3) / 6 = 0.833 s; a dash with 0.7 s lock pressed 0.5 s early costs 0.2 s.
    assert abs(first_gcd_walk(8, 6) - 5 / 6) < 1e-9
    assert abs(first_gcd_dash(8, 0.7, 6, True) - 0.2) < 1e-9
    # In range: the proposal does not dash (0 s), upstream dashes and pays the lock beyond the press.
    assert first_gcd_dash(2, 0.9, 6, True) == 0.0 and abs(first_gcd_dash(2, 0.9, 6, False) - 0.4) < 1e-9
    # Beyond the dash's range both walk.
    assert first_gcd_dash(25, 0.7, 6, True) == first_gcd_walk(25, 6)
    # Geometry: a constructed line of enemies 4 y apart; from the middle one, the two neighbours are reached.
    line = [(-4.0, 0.0, 0.5), (0.0, 0.0, 0.5), (4.0, 0.0, 0.5), (12.0, 0.0, 0.5)]
    assert reached((0.0, 0.0), line) == 3, reached((0.0, 0.0), line)
    # A dash to the most central enemy never covers fewer than walking to the first one (constructed defect check:
    # a landing outside the pack would break it).
    rng = random.Random(1)
    for _ in range(500):
        walked, dashed, _ = pack_trial(rng, 6, 4.0, 15.0)
        assert dashed >= walked - 1, (walked, dashed)
    print("self-test ok: walk and dash timings, the in-range case and the pack geometry are computed right")


if __name__ == "__main__":
    self_test()
    print("A. first weaponskill after the pull, seconds: distance, run speed, dash lock -> walk | upstream dash | dash only out of range")
    for row in countdown_table():
        if row[1] == 6.0:
            print("  %4.0f y  %.0f y/s  lock %.1f s -> %.2f | %.2f | %.2f" % row)
    print("B. enemies reached by a 5 y area attack (approach from 15 y): count, pack radius -> walk to first | dash to central | dash to random")
    for row in pack_table():
        print("  %d enemies, radius %.0f y -> %.2f | %.2f | %.2f" % row)
