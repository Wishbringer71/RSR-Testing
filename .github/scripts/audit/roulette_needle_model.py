#!/usr/bin/env python3
"""When the beastmaster may break the King Ahriman piece's final hourglass (concept 21, K12 "Königs-Ahriman"; A675).

Not a code scanner - a model calculator. It plays the Roulette as GluttonyCombo recorded it (cb5bf32, 07.11.2026): the
needle points along the four axes and turns a quarter one step after each beat; breaking the hourglass makes the first
beat after the break fire Death along the needle, and a Death on the player kills. It runs the rule of KillOrder
(PressAims, WaitForSafePress) at every press time over a roulette and checks each allowed press against where Death
really goes:
  RULE      the rule as built: the break lands one round trip after the press at the earliest and one beat later at the
            latest; a beat is dangerous when the needle then points into the player's half
  NOW       a defect: the heading at the press decides (no beat, no lag)
  NOLAG     a defect: the rule without the round trip and without the beat of slack
  RANDOM    no rule: every press goes out
Measured: allowed presses that kill (must be 0 for RULE), the share of time a press may go out, and the longest wait.

Game values (xivapi v2, 07.11.2026): Death 49422 reaches 23 y. Recorded values (GluttonyCombo): beat 1.5605 s, the turn
0.49 s after its beat, hit to break 0.4 and 0.67 s. The cone's width is in no sheet; the model kills with half-angles of
45, 60 and 89 degrees, the rule assumes the half-plane.

Usage: python3 .github/scripts/audit/roulette_needle_model.py [--self-test]
"""
import math
import random
import sys

BEAT = 1.5605
TURN_AFTER_BEAT = 0.49
REACH = 23.0
QUARTERS = 4


def heading_vector(quarter):
    angle = quarter * (math.tau / QUARTERS)
    return math.sin(angle), math.cos(angle)


def in_half(quarter, dx, dz):
    hx, hz = heading_vector(quarter)
    return dx * dx + dz * dz <= REACH * REACH and hx * dx + hz * dz > 0


def in_cone(quarter, dx, dz, half_angle):
    hx, hz = heading_vector(quarter)
    dist = math.hypot(dx, dz)
    if dist > REACH or dist == 0:
        return False
    return (hx * dx + hz * dz) / dist >= math.cos(half_angle)


class Needle:
    """Turns at n * BEAT (n >= 1); beat n falls TURN_AFTER_BEAT before turn n and fires with the heading after turn n - 1."""

    def __init__(self, q0, sense, phase):
        self.q0, self.sense, self.phase = q0, sense, phase

    def turn_time(self, n):
        return self.phase + n * BEAT

    def last_turn(self, t):
        n = math.floor((t - self.phase) / BEAT)
        return n, self.turn_time(n), self.q0 + n * self.sense

    def death_heading(self, b):
        # first beat at or after the break b
        n = math.ceil((b - self.phase + TURN_AFTER_BEAT) / BEAT)
        return self.q0 + (n - 1) * self.sense


def press_aims(x, lag, since, quarter, sense, dx, dz, beat=BEAT, before=TURN_AFTER_BEAT, slack=None):
    earliest = x + lag
    latest = earliest + (beat if slack is None else slack)
    for k in range(1, QUARTERS * QUARTERS + 1):
        at = -since - before + k * beat
        if at - beat >= latest:
            break
        if at >= earliest and in_half(quarter + (k - 1) * sense, dx, dz):
            return True
    return False


def decide(variant, t, needle, rt, dx, dz):
    n, turned, quarter = needle.last_turn(t)
    since = t - turned
    if variant == "RULE":
        return not press_aims(0.0, rt, since, quarter, needle.sense, dx, dz)
    if variant == "NOLAG":
        return not press_aims(0.0, 0.0, since, quarter, needle.sense, dx, dz, slack=0.0)
    if variant == "NOW":
        return not in_half(quarter, dx, dz)
    return True


def run(variant, bearing_offset, half_angle, delta_range, seed=7, roulettes=200):
    rng = random.Random(seed)
    kills = allowed = total = 0
    longest_wait = 0.0
    for _ in range(roulettes):
        sense = rng.choice((1, -1))
        needle = Needle(rng.randrange(QUARTERS), sense, rng.uniform(0, BEAT))
        side = rng.randrange(QUARTERS) * (math.tau / QUARTERS) + bearing_offset
        dist = rng.uniform(2.0, 15.0)
        dx, dz = math.sin(side) * dist, math.cos(side) * dist
        rt = rng.uniform(0.05, 0.35)
        last_allowed = 2 * BEAT
        t = 2 * BEAT
        while t < 2 * BEAT + 25.0:
            total += 1
            if decide(variant, t, needle, rt, dx, dz):
                allowed += 1
                longest_wait = max(longest_wait, t - last_allowed)
                last_allowed = t
                b = t + rt + rng.uniform(*delta_range)
                if in_cone(needle.death_heading(b), dx, dz, half_angle):
                    kills += 1
            t += 0.01
    return kills, allowed / total, longest_wait


def report():
    print(f"{'variant':8} {'stand':9} {'cone':>5} {'delta':>11} {'kills':>6} {'free':>6} {'wait':>6}")
    for stand, offset in (("on axis", math.radians(3)), ("diagonal", math.radians(44))):
        for half in (45, 60, 89):
            for delta in ((0.4, 0.67), (0.0, BEAT)):
                for variant in ("RULE", "NOW", "NOLAG", "RANDOM"):
                    kills, free, wait = run(variant, offset, math.radians(half), delta)
                    print(f"{variant:8} {stand:9} {half:5} {delta[0]:4.2f}-{delta[1]:4.2f} {kills:6} {free:6.0%} {wait:5.1f}s")


def self_test():
    for offset in (math.radians(3), math.radians(44)):
        for half in (45, 60, 89):
            for delta in ((0.4, 0.67), (0.0, BEAT)):
                kills, free, wait = run("RULE", offset, math.radians(half), delta, roulettes=60)
                assert kills == 0, ("RULE killed", offset, half, delta, kills)
                assert free > 0.2, ("RULE leaves too little", offset, free)
                assert wait <= 3 * BEAT + 0.05, ("RULE waits too long", offset, wait)
    # Off an axis by any angle the half-plane takes two headings: a quarter of the time is free.
    near_axis = run("RULE", math.radians(3), math.radians(60), (0.4, 0.67), roulettes=60)[1]
    assert 0.2 < near_axis < 0.3, ("near an axis, a quarter of the time should be free", near_axis)
    # Constructed defects: the model must see them kill.
    assert run("NOW", math.radians(3), math.radians(60), (0.4, 0.67), roulettes=60)[0] > 0, "NOW should kill"
    assert run("NOLAG", math.radians(3), math.radians(60), (0.4, 0.67), roulettes=60)[0] > 0, "NOLAG should kill"
    assert run("RANDOM", math.radians(3), math.radians(60), (0.4, 0.67), roulettes=60)[0] > 0, "RANDOM should kill"
    print("self-test ok: the rule never lets a press kill, a quarter of the time is free, the wait stays within three "
          "beats; the heading at the press, the rule without lag and no rule all kill")


if __name__ == "__main__":
    if "--self-test" in sys.argv:
        self_test()
    else:
        report()
