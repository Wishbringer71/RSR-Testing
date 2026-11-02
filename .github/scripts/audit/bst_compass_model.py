#!/usr/bin/env python3
"""Which axe the beastmaster starts with when the familiar's Trick is to close the combo (concept 01, "Kompass: Dressur
und Axt im Uhrzeigersinn", K1; A487).

Not a code scanner - an exhaustive model. Branch 2 of BST_Reborn.AttackAbility lays an axe and lets the familiar's Trick
close the combo. Two instinctual skills within 7 s close a combo; one that follows the other clockwise on the Inner
Compass (Volant -> Rampant -> Durant -> Eldritch -> Volant) is an intentional combo, "dealing greater additional damage"
(the axes' effect texts, the job guide). How much greater no text says, so the model counts intentional links and does
not weigh them.

Compared over every heart standing before the axe (none or one of four) and every affinity of the Trick (four), and over
the levels at which the axes come (Avalanche 4, Mistral 8, Spinning 14, Gale 16):
  FIXED  the order BST_Reborn used: the first learned of Gale, Spinning, Mistral, Avalanche, the heart not asked
  K1     a heart standing: the axe that follows it clockwise; none: the axe before the Trick's affinity, so the Trick
         closes clockwise; that axe not learned: FIXED
Measured: intentional links in the two steps (heart -> axe, axe -> Trick).
"""
import sys

ORDER = ["Volant", "Rampant", "Durant", "Eldritch"]  # clockwise
AXE = {"Avalanche": "Rampant", "Mistral": "Durant", "Spinning": "Eldritch", "Gale": "Volant"}
LEARNED_AT = {"Avalanche": 4, "Mistral": 8, "Spinning": 14, "Gale": 16}
FIXED_ORDER = ["Gale", "Spinning", "Mistral", "Avalanche"]


def follows(first, second):
    """Whether `second` follows `first` clockwise - an intentional combo."""
    return first is not None and ORDER[(ORDER.index(first) + 1) % 4] == second


def axe_of(affinity):
    return next(name for name, aff in AXE.items() if aff == affinity)


def fixed(heart, trick, learned):
    return next((a for a in FIXED_ORDER if a in learned), None)


def k1(heart, trick, learned):
    if heart is not None:
        wanted = axe_of(ORDER[(ORDER.index(heart) + 1) % 4])
    else:
        wanted = axe_of(ORDER[(ORDER.index(trick) - 1) % 4])
    return wanted if wanted in learned else fixed(heart, trick, learned)


def links(heart, axe, trick):
    """Intentional links in heart -> axe -> Trick."""
    if axe is None:
        return 0
    return int(follows(heart, AXE[axe])) + int(follows(AXE[axe], trick))


def cases(level=90):
    learned = {a for a, lv in LEARNED_AT.items() if lv <= level}
    for heart in [None] + ORDER:
        for trick in ORDER:
            yield heart, trick, learned


def compare(rule, level=90):
    better = worse = 0
    for heart, trick, learned in cases(level):
        old = links(heart, fixed(heart, trick, learned), trick)
        new = links(heart, rule(heart, trick, learned), trick)
        better += new > old
        worse += new < old
    return better, worse


def branch1_expectation(heart):
    """Branch 1 with a heart standing, averaged over the Trick's affinity: (Trick first, then the axe that follows the
    Trick) against (the axe that follows the heart, then the Trick)."""
    trick_first = axe_first = 0
    for trick in ORDER:
        follow_trick = axe_of(ORDER[(ORDER.index(trick) + 1) % 4])
        trick_first += int(follows(heart, trick)) + int(follows(trick, AXE[follow_trick]))
        follow_heart = axe_of(ORDER[(ORDER.index(heart) + 1) % 4])
        axe_first += int(follows(heart, AXE[follow_heart])) + int(follows(AXE[follow_heart], trick))
    return trick_first / 4, axe_first / 4


def self_test():
    for level in (4, 8, 14, 16, 90):
        better, worse = compare(k1, level)
        assert worse == 0, (level, worse)
    better, worse = compare(k1)
    assert better == 12 and worse == 0, (better, worse)
    # Every case K1 reaches the most any learned axe could bring.
    for heart, trick, learned in cases():
        best = max(links(heart, a, trick) for a in learned)
        assert links(heart, k1(heart, trick, learned), trick) == best, (heart, trick)
    # Constructed defect: starting with the axe after the Trick's affinity instead of before it loses.
    def after(heart, trick, learned):
        return axe_of(ORDER[(ORDER.index(trick) + 1) % 4]) if heart is None else k1(heart, trick, learned)
    assert compare(after)[1] > 0
    # Branch 1 turned round brings the same expectation (one link sure, one at a quarter): rejected as no gain.
    for heart in ORDER:
        assert branch1_expectation(heart) == (1.25, 1.25), branch1_expectation(heart)
    print("self-test ok: K1 never brings fewer intentional links than the fixed order at any level, more in 12 of 20 "
          "cases, always the most a learned axe can bring, the axe after the Trick's affinity is caught, and branch 1 "
          "turned round brings the same expectation")


def report():
    for level in (4, 8, 14, 16, 90):
        better, worse = compare(k1, level)
        print(f"  axes learned at level {level:2d}: K1 better in {better:2d} of 20 cases, worse in {worse}")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
