#!/usr/bin/env python3
"""The learned tables across a game version: which rule lowers an old reading, and when may a reading save protection
(concept 13, "Gelernte Tabellen über Spielversionen", the owner's order of 28.10.2026 and his objection of 29.10.2026:
"was ist mit der vorgabe, dass das überleben zählt? was ist mit failsafe und hardening?").

Not a code scanner - a model calculator. One enemy action whose hit is learned as a share of a member's maximum HP,
stored as the highest reading. A decision that SAVES protection - "Skip mitigation for small area casts", the tankbuster
plan's "spend only what it needs" - skips when the reading lies below a threshold T; it is WRONG when the hit that then
lands is at or above T. A decision that ADDS protection reads the same value the other way and cannot go wrong by a
reading that is too high.

Readings carry the game's damage roll: every hit taken is multiplied by rand[95,105] before mitigation (Allagan
Studies, "How to be a Math Wizard", Shadowbringers edition, damage taken; a community source, not stated for Dawntrail).
ASSUMPTIONS, not from a source, and named where used: the share of hits taken under a barrier and how much of the hit
the barrier hides from the reading; the distribution of how many members share a stack.

Rules compared, for the readings after a version change:
  NEW       reference: the action as if never seen before the change, learned by today's rule - unrated mitigates,
            the first reading decides the next cast; the risk today accepts for every new action
  NEWC      NEW, but a saving decision needs one clean reading (hardening within a version)
  NEW2      NEW, but a saving decision needs two clean readings, the fewest that show a spread, and none while they
            vary beyond the roll (hardening within a version)
  TODAY     no revision: the highest reading over all versions, used for every decision
  FIRST     the first reading of the new version replaces the old value (the form concept 13 rejected)
  SYM       the old value stands until the new version has at least as many clean readings (no barrier) as it had;
            then the highest new reading replaces it if lower; any new reading at or above confirms it
  ASYM      SYM, and until the new version has one clean reading the old value may only add protection: a saving
            decision treats the action as unrated, which mitigates
  CLEAN     TODAY within one version, but a saving decision needs one clean reading of the action: a value learned
            only under barriers adds protection and saves none
  PROP      the proposal: ASYM and CLEAN, and an old value is lowered only when the new version has at least as many
            clean readings as the old one and at least two - the fewest that show a spread -, they vary at least as
            much as the old ones did, give or take one damage roll, and the new highest lies below the old by more
            than a roll: a lower reading set by the roll alone, or new readings that have not yet met the variants
            the old ones met (a stack shared by fewer), keep the old value

Measured per scenario and rule: the probability that a saving decision skips protection for a hit at or above T, over
the casts that follow the version change, and the casts until a lowered action saves protection again.

Usage: python3 .github/scripts/audit/learned_revision_model.py [--self-test]
"""
import random
import sys

ROLL = (0.95, 1.05)
THRESHOLD = 0.25          # LargeShieldShare: The Blackest Night's 25 % from its effect text (concept 13)
TRIALS = 20000
CASTS_AFTER = 12


def roll(rng):
    return rng.uniform(*ROLL)


class Action:
    """One enemy action: its true unmitigated share and how a reading of one landing comes about."""

    def __init__(self, share, barrier_rate=0.0, barrier_hides=0.0, stack=None):
        self.share = share
        self.barrier_rate = barrier_rate      # ASSUMPTION per scenario
        self.barrier_hides = barrier_hides    # ASSUMPTION per scenario
        self.stack = stack                    # None, or [(sharers, probability), ...] for a share split by sharers

    def hit(self, rng):
        """The share one landing takes from the member hit hardest."""
        share = self.share
        if self.stack:
            pick = rng.random()
            for sharers, p in self.stack:
                pick -= p
                if pick <= 0:
                    share = self.share * self.stack[0][0] / sharers
                    break
        return share * roll(rng)

    def reading(self, rng, hit):
        """What the table stores of that landing, and whether the reading was clean (no barrier on the member)."""
        if rng.random() < self.barrier_rate:
            return hit * (1.0 - self.barrier_hides), False
        return hit, True


def learn(action, rng, n):
    """The old version's state: highest reading, its clean readings, and their lowest and highest."""
    best, clean, low, high = 0.0, 0, float("inf"), 0.0
    for _ in range(n):
        value, is_clean = action.reading(rng, action.hit(rng))
        best = max(best, value)
        clean += is_clean
        if is_clean:
            low, high = min(low, value), max(high, value)
    return best, clean, low, high


ROLL_RATIO = ROLL[1] / ROLL[0]


def spread(low, high):
    """How far clean readings vary, highest over lowest; 1 when there are fewer than two."""
    return high / low if 0 < low < high else 1.0


# The fewest clean readings that show a spread; definitional, not a tunable (concept 13, A446).
SPREAD_READINGS = 2


def run(rule, before, after, n_old, rng):
    """Wrong skips over the casts after the change, and the cast from which a saving uses a lowered value."""
    if rule in ("NEW", "NEWC", "NEW2"):
        n_old = 0
    old, old_clean, old_low, old_high = learn(before, rng, n_old)
    new, new_clean, replaced = 0.0, 0, False
    clean_low, clean_high = float("inf"), 0.0
    wrong = 0
    lowered_at = None
    for cast in range(CASTS_AFTER):
        hit = after.hit(rng)
        if rule in ("TODAY", "CLEAN", "NEW", "NEWC", "NEW2"):
            value = max(old, new)
        elif rule == "FIRST":
            value = new if cast > 0 else old
        else:
            value = new if replaced else max(old, new)
        if rule == "ASYM":
            unconfirmed = new_clean == 0
        elif rule == "PROP":
            unconfirmed = new_clean == 0 or (not replaced and old_clean + new_clean == 0)
        elif rule in ("CLEAN", "NEWC"):
            unconfirmed = old_clean + new_clean == 0
        elif rule == "NEW2":
            unconfirmed = new_clean < SPREAD_READINGS or spread(clean_low, clean_high) > ROLL_RATIO
        else:
            unconfirmed = False
        # Nothing read yet is unrated, and unrated mitigates (concept 13).
        rated = n_old > 0 or cast > 0
        skips = rated and not unconfirmed and value < THRESHOLD
        if skips and hit >= THRESHOLD:
            wrong += 1
        if lowered_at is None and value < old and skips:
            lowered_at = cast
        reading, is_clean = after.reading(rng, hit)
        new = max(new, reading)
        new_clean += is_clean
        if is_clean:
            clean_low, clean_high = min(clean_low, reading), max(clean_high, reading)
        if rule in ("SYM", "ASYM", "PROP") and not replaced:
            if new >= old:
                replaced = True
            elif rule == "PROP":
                replaced = (new_clean >= max(old_clean, SPREAD_READINGS)
                            and spread(clean_low, clean_high) * ROLL_RATIO >= spread(old_low, old_high)
                            and new * ROLL_RATIO < old)
            elif new_clean >= max(old_clean, 1):
                replaced = True
    return wrong, lowered_at


def scenario(rule, before, after, n_old, seed=1):
    rng = random.Random(seed)
    wrong_casts, lowered = 0, []
    for _ in range(TRIALS):
        wrong, at = run(rule, before, after, n_old, rng)
        wrong_casts += wrong
        if at is not None:
            lowered.append(at)
    p_wrong = wrong_casts / (TRIALS * CASTS_AFTER)
    resumed = sum(lowered) / len(lowered) if lowered else None
    return p_wrong, resumed, len(lowered) / TRIALS


SCENARIOS = [
    ("unchanged, 22 % - near the line, damage roll only", Action(0.22), Action(0.22), 3),
    ("unchanged, 24 %, stacks shared by 8, sometimes 6 or 4 (ASSUMPTION)",
     Action(0.24, stack=[(8, 0.7), (6, 0.2), (4, 0.1)]), Action(0.24, stack=[(8, 0.7), (6, 0.2), (4, 0.1)]), 3),
    ("unchanged, 30 %, half the landings under a barrier hiding a third (ASSUMPTION)",
     Action(0.30, 0.5, 0.33), Action(0.30, 0.5, 0.33), 3),
    ("patch lowers 30 % to 18 %", Action(0.30), Action(0.18), 3),
    ("patch raises 18 % to 30 %", Action(0.18), Action(0.30), 3),
    ("patch raises 18 % to 30 %, old value from 10 readings", Action(0.18), Action(0.30), 10),
    ("patch lowers 30 % to 18 %, stacks as above (ASSUMPTION)",
     Action(0.30, stack=[(8, 0.7), (6, 0.2), (4, 0.1)]), Action(0.18, stack=[(8, 0.7), (6, 0.2), (4, 0.1)]), 3),
]

RULES = ["NEW", "NEWC", "NEW2", "TODAY", "CLEAN", "FIRST", "SYM", "ASYM", "PROP"]


def report():
    print("Wrong skips per cast after a version change (a saving decision skipped protection for a hit at or above "
          f"{THRESHOLD:.0%}), over {CASTS_AFTER} casts, {TRIALS} runs each; in brackets the mean cast from which a "
          "lowered value saves, and how often it does.")
    for name, before, after, n_old in SCENARIOS:
        print(f"\n  {name} (old value from {n_old} readings)")
        for rule in RULES:
            p_wrong, resumed, share = scenario(rule, before, after, n_old)
            extra = f"  [lowered value saves from cast {resumed:.1f}, in {share:.0%} of runs]" if resumed is not None else ""
            print(f"    {rule:6s} {p_wrong:7.2%}{extra}")


def self_test():
    """Constructed defects the model has to tell apart from the rules it reports."""
    raised = (Action(0.18), Action(0.30), 3)
    stacked = (Action(0.24, stack=[(8, 0.7), (6, 0.2), (4, 0.1)]), Action(0.24, stack=[(8, 0.7), (6, 0.2), (4, 0.1)]), 3)
    lowered = (Action(0.30), Action(0.18), 3)

    # A raised action: today and the symmetric rule skip on the old small value at the first cast; the proposal never.
    asym, _, _ = scenario("ASYM", *raised)
    today, _, _ = scenario("TODAY", *raised)
    if not asym < today:
        raise AssertionError("the proposal did not keep the first cast after a raise protected")
    if asym > 0.0:
        raise AssertionError("an unconfirmed reading saved protection under the proposal")

    # Stacks: replacing on the first reading is the riskiest rule; the symmetric rule is not.
    first, _, _ = scenario("FIRST", *stacked)
    sym, _, _ = scenario("SYM", *stacked)
    if not first > sym:
        raise AssertionError("replacing on the first reading was not caught as riskier than the symmetric rule")

    # A lowered action saves again under every rule that lowers; under the symmetric one only after the old evidence.
    _, first_at, _ = scenario("FIRST", *lowered)
    _, sym_at, _ = scenario("SYM", *lowered)
    if first_at is None or sym_at is None or not first_at < sym_at:
        raise AssertionError("the symmetric rule did not wait for as many readings as the old value had")
    _, today_at, _ = scenario("TODAY", *lowered)
    if today_at is not None:
        raise AssertionError("today's rule lowered a value - it only rises")

    # The proposal: where the hit did not weaken, no worse than today; where a patch weakened it, no worse than the risk
    # today accepts for every new action; and it still lowers a fixed hit a patch weakened.
    for name, before, after, n_old in SCENARIOS:
        prop, _, _ = scenario("PROP", before, after, n_old)
        today, _, _ = scenario("TODAY", before, after, n_old)
        fresh, _, _ = scenario("NEW", before, after, n_old)
        if prop > (today if after.share >= before.share else fresh):
            raise AssertionError(f"the proposal skipped protection more often than the accepted risk: {name}")
    _, prop_at, _ = scenario("PROP", *lowered)
    if prop_at is None:
        raise AssertionError("the proposal never lowered a hit a patch weakened")

    # A value learned only under barriers saves nothing under CLEAN.
    barrier = (Action(0.30, 0.5, 0.33), Action(0.30, 0.5, 0.33), 3)
    clean, _, _ = scenario("CLEAN", *barrier)
    today_barrier, _, _ = scenario("TODAY", *barrier)
    if not clean < today_barrier:
        raise AssertionError("requiring a clean reading did not remove the skips on barrier readings")

    # Determinism: the same seed gives the same figure.
    if scenario("SYM", *stacked, seed=7) != scenario("SYM", *stacked, seed=7):
        raise AssertionError("the model is not deterministic")

    print("self-test ok: the proposal keeps the first cast after a raise protected, replacing on the first reading is "
          "caught as the riskier rule, the symmetric rule waits for the old evidence, today's rule never lowers, the "
          "proposal skips protection in no scenario more often than today and still lowers a weakened fixed hit, a "
          "clean reading removes the skips on barrier readings, and runs are deterministic")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
