#!/usr/bin/env python3
"""When Capture marks a falling enemy (concept 01, "Was gilt beim Anfreunden", A2; A523, A525).

Not a code scanner - a model calculator. A pact can only come from an enemy that dies while it carries Interest
Captured, and the mark arrives after Capture's effect: 0.62 s later in both of the owner's recordings (02.11.2026
Killermantis, 03.11.2026 Flechten-Goobbue), 0.69 s from the press on 04.11.2026 (Zorniger Ichor). The chance is higher
the lower the enemy's HP when Capture goes off ("Success rate is inversely proportional to the target's HP at the time
of execution"). The rules for the weaponskill that marks, read at the moments a GCD becomes ready:
  OLD   the time to kill as RSR filed it once a second (up to a second old) is at most one GCD
  NEW   the time to kill counted from now: the enemy outlives the mark of this GCD, not that of the next one -
        landing < remaining <= landing + GCD (A523)
  FIRST one GCD of reserve on NEW, with no lower bound: remaining <= landing + 2 GCD
  FALL  his precept of 04.11.2026 ("anfreunden hat oberste priorität ... so sicher sein, dass immer genug zeit bleibt"):
        Capture now when waiting for the next GCD could let the enemy fall before that mark lands - by the largest fall
        its own health history shows over that span (sampled at 1 Hz, the span rounded up to whole samples), or by
        NEW's forecast - with no lower bound, so a mark that may come too late still goes (A525)
The enemy's health is simulated in steps of 0.1 s: its last 30 s from full to zero, in three courses - even; in bursts
(the damage of each half second drawn from an exponential distribution, as hits, a familiar's action and weaving
come); and speeding up by a factor over its last seconds (a burst the history has not seen yet - the danger for every
rule that reads the past). Measured over the enemy's death at every phase against the GCD and the sampling: the share
of kills on which the mark lands before death, and the HP share left when Capture goes off.
ASSUMPTIONS, named where used: the time from pressing Capture to its effect (the connection) is NET; RSR's forecast is
the falling rate fitted over the history (TimeToKillTracker.FitRate), without the bias correction.
"""
import math
import random
import sys

GCD = 2.5
SAMPLE = 1.0     # the health history and the forecast are sampled once a second (TargetUpdater, TimeToKillTracker)
EFFECT_TO_MARK = 0.62
NET = 0.1        # ASSUMPTION: from pressing Capture to its effect
LANDING = NET + EFFECT_TO_MARK
FALL = 30.0      # ASSUMPTION: the enemy's last 30 s fall from full to zero
STEP = 0.1
RULES = ("OLD", "NEW", "FIRST", "FALL")


def course(rng, kind, speed_up=1.0, last=5.0):
    """The enemy's health share at each step over its last FALL seconds, ending at 0."""
    steps = int(round(FALL / STEP))
    if kind == "burst":
        half = int(round(0.5 / STEP))
        weights = []
        for _ in range(0, steps, half):
            w = rng.expovariate(1.0)
            weights.extend([w / half] * half)
        weights = weights[:steps]
    else:
        weights = [1.0] * steps
    if speed_up != 1.0:
        tail = int(round(last / STEP))
        for i in range(steps - tail, steps):
            weights[i] *= speed_up
    total = sum(weights)
    hp = [1.0]
    for w in weights:
        hp.append(hp[-1] - w / total)
    hp[-1] = 0.0
    return hp


def hp_at(hp, t):
    """Health at time t (seconds before death negative, 0 = death); full before the last FALL seconds."""
    i = int(math.floor((t + FALL) / STEP))
    if i <= 0:
        return 1.0
    if i >= len(hp) - 1:
        return 0.0
    return hp[i]


def capture(rule, hp, gcd_phase, sample_phase, landing=LANDING):
    """The first moment Capture goes off under the rule, relative to death at 0; None if never."""
    samples = []
    k = 0
    start = -FALL - GCD + gcd_phase
    next_sample = -FALL + sample_phase
    while True:
        ready = start + k * GCD
        if ready >= 0.0:
            return None
        while next_sample <= ready:
            samples.append((next_sample, hp_at(hp, next_sample)))
            next_sample += SAMPLE
        k += 1
        if len(samples) < 3:
            continue
        window = [s for s in samples if s[0] > samples[-1][0] - FALL]
        mt = sum(s[0] for s in window) / len(window)
        mr = sum(s[1] for s in window) / len(window)
        var = sum((s[0] - mt) ** 2 for s in window)
        cov = sum((s[0] - mt) * (s[1] - mr) for s in window)
        rate = -cov / var if var > 0 else 0.0
        if rate <= 0:
            continue
        filed_at, filed_ratio = samples[-1]
        filed = filed_ratio / rate
        remaining = max(0.0, filed - (ready - filed_at))
        now_hp = hp_at(hp, ready)
        if rule == "OLD":
            hit = filed <= GCD
        elif rule == "NEW":
            hit = landing < remaining <= landing + GCD
        elif rule == "FIRST":
            hit = remaining <= landing + 2 * GCD
        else:
            span = math.ceil((GCD + landing) / SAMPLE) * SAMPLE
            fall = 0.0
            for i, (ti, ri) in enumerate(samples):
                for tj, rj in samples[i + 1:]:
                    if tj - ti > span + 1e-9:
                        break
                    fall = max(fall, ri - rj)
            seen = samples[-1][0] - samples[0][0]
            if seen < span and seen > 0:
                fall = max(fall, (samples[0][1] - samples[-1][1]) * span / seen)
            hit = now_hp <= fall or remaining <= landing + GCD
        if hit:
            return ready


def run(rule, kind="even", speed_up=1.0, cases=4000, seed=7, landing=LANDING):
    rng = random.Random(seed)
    landed = 0
    hp_marks = []
    for _ in range(cases):
        hp = course(rng, kind, speed_up)
        used = capture(rule, hp, rng.uniform(0.0, GCD), rng.uniform(0.0, SAMPLE), landing)
        if used is not None and used + LANDING < 0.0:
            landed += 1
            hp_marks.append(hp_at(hp, used))
    return landed / cases, (sum(hp_marks) / len(hp_marks) if hp_marks else float("nan"))


COURSES = [("even", 1.0), ("burst", 1.0), ("even", 1.5), ("burst", 1.5), ("even", 2.0), ("burst", 2.0)]


def report():
    print("Share of kills marked in time, and the HP share at the mark (of the last 30 s):")
    for kind, speed in COURSES:
        cells = []
        for rule in RULES:
            share, hp = run(rule, kind, speed, cases=2000)
            cells.append(f"{rule} {share:6.1%} at {hp:5.1%}")
        label = f"{kind}" + (f", last 5 s x{speed}" if speed != 1.0 else "")
        print(f"  {label:20s} " + "   ".join(cells))


def self_test():
    old, _ = run("OLD", cases=1500)
    new, hp_new = run("NEW", cases=1500)
    fall, hp_fall = run("FALL", cases=1500)
    # The forecast as filed is up to a second long, and the mark needs its landing: the old rule misses kills on an
    # even course, the new one marks them.
    assert old < 0.9 and new > 0.97, (old, new)
    # His precept: the rule on the largest fall marks every kill on an even course and in bursts.
    assert fall > 0.99, fall
    burst_new, _ = run("NEW", "burst", cases=1500)
    burst_fall, _ = run("FALL", "burst", cases=1500)
    assert burst_fall > 0.99 and burst_fall >= burst_new, (burst_fall, burst_new)
    # Its price: an earlier mark, at more HP than the new rule.
    assert hp_fall > hp_new, (hp_fall, hp_new)
    # A fall the history has not seen yet still costs the new rule more than the safe one.
    sped_new, _ = run("NEW", "even", 2.0, cases=1500)
    sped_fall, _ = run("FALL", "even", 2.0, cases=1500)
    assert sped_fall > sped_new, (sped_fall, sped_new)
    # Constructed defect: the safe rule without the landing (span of one GCD only) is caught on bursts.
    short, _ = run("FALL", "burst", landing=-GCD, cases=1500)
    assert short < burst_fall, (short, burst_fall)
    print("self-test ok: the stale forecast costs the old rule kills; the new rule marks an even course; the rule on the "
          "largest fall marks every kill even in bursts and stays ahead when the end speeds up, at more HP; a span "
          "without the landing is caught")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
