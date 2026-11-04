#!/usr/bin/env python3
"""When Capture marks a falling enemy (concept 01, "Was gilt beim Anfreunden", A2; A523).

Not a code scanner - a model calculator. A pact can only come from an enemy that dies while it carries Interest
Captured, and the mark arrives after Capture's effect: 0.62 s later in both of the owner's recordings (02.11.2026
Killermantis, 03.11.2026 Flechten-Goobbue: action effect, then the status). The chance is higher the lower the enemy's
HP when Capture goes off ("Success rate is inversely proportional to the target's HP at the time of execution").
Two rules for the weaponskill that marks, both read at the moments a GCD becomes ready:
  OLD  the corrected time to kill is at most one GCD - read from the forecast RSR files once a second, as it was
       filed (up to a second old, so up to a second too long)
  NEW  the forecast less its age; Capture when the enemy outlives the mark's landing after this GCD starts, and would
       not after the next one: landing < remaining <= landing + GCD. The landing is what the rule measures itself,
       from pressing Capture to the mark (NET + EFFECT_TO_MARK here)
Measured over the enemy's death at every phase against the GCD and the 1 Hz sampling: the share of kills on which the
mark lands before death, and the HP share left when Capture goes off.
ASSUMPTIONS, named where used: the enemy's health falls evenly over its last seconds; the forecast is right when
filed, or off by the noise given; the time from pressing Capture to its effect (the connection) is NET.
"""
import random
import sys

GCD = 2.5
SAMPLE = 1.0     # the forecast is filed once per history sample, 1 Hz (ObjectHelper.ScoreTtkForecast)
EFFECT_TO_MARK = 0.62
NET = 0.1        # ASSUMPTION: from pressing Capture to its effect
FALL = 30.0      # ASSUMPTION: the enemy's last 30 s fall evenly from full to zero


def capture(rule, death, gcd_phase, sample_phase, noise, rng, landing):
    """The moment Capture goes off, or None."""
    k = 0
    while True:
        ready = gcd_phase + k * GCD
        if ready >= death:
            return None
        filed_at = sample_phase + SAMPLE * ((ready - sample_phase) // SAMPLE)
        forecast = (death - filed_at) * (1.0 + rng.uniform(-noise, noise))
        if rule == "OLD":
            due = forecast <= GCD
        else:
            remaining = max(0.0, forecast - (ready - filed_at))
            due = landing < remaining <= landing + GCD
        if due:
            return ready
        k += 1


def run(rule, noise=0.0, cases=20000, seed=7, landing=NET + EFFECT_TO_MARK):
    rng = random.Random(seed)
    marked = 0
    hp = []
    for _ in range(cases):
        death = 20.0 + rng.uniform(0.0, GCD * SAMPLE * 4)
        used = capture(rule, death, rng.uniform(0.0, GCD), rng.uniform(0.0, SAMPLE), noise, rng, landing)
        if used is not None and used + NET + EFFECT_TO_MARK < death:
            marked += 1
            hp.append((death - used) / FALL)
    return marked / cases, (sum(hp) / len(hp) if hp else float("nan"))


def report():
    print("Share of kills marked in time, and the HP share left when Capture went off:")
    for noise in (0.0, 0.1, 0.25):
        for rule in ("OLD", "NEW"):
            share, hp = run(rule, noise)
            print(f"  forecast noise {noise:4.0%}  {rule}: marked {share:6.1%}, HP at Capture {hp:6.2%}")


def self_test():
    old, _ = run("OLD")
    new, hp_new = run("NEW")
    # The forecast as filed is up to a second long, and the mark needs 0.62 s: the old rule misses a part of the kills
    # even with a perfect forecast; the new one marks every kill.
    assert old < 0.9 and new > 0.99, (old, new)
    # The new rule still marks late: within the last GCD and a landing of the enemy's life.
    assert hp_new < (GCD + EFFECT_TO_MARK + NET) / FALL, hp_new
    # With a noisy forecast the new rule stays ahead.
    for noise in (0.1, 0.25):
        assert run("NEW", noise)[0] > run("OLD", noise)[0], noise
    # Constructed defect: the new rule without the landing (Capture as late as the GCD allows) loses kills again.
    late, _ = run("NEW", landing=0.0)
    assert late < new - 0.1, (late, new)
    print("self-test ok: the stale forecast and the landing cost the old rule kills, the new one marks them all at "
          "a late GCD, stays ahead under noise, and a rule without the landing is caught")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
