#!/usr/bin/env python3
"""Which Tempered Release protects the beastmaster in a danger (concept 01, "Begleiter nach Lage", C1 and C2; A491).

Not a code scanner - an exhaustive model. In a danger B2 calls first a familiar that heals or protects you. The effect
texts (xivapi v2, ActionTransient, 02.11.2026) hold two kinds of Tempered Release that B1 read wrongly:
  - Releases that stop the enemies they reach - Stun (War Cry, 15 s), Petrification (Petribreath 21 s, Stone Gaze 42 s),
    Deep Freeze (The Ram's Voice, 20 s); the Status sheet gives each LockActions ("preventing the execution of actions").
    B1 counted none of them.
  - Releases that lower only one element's damage - Strut (earth 15 %), Formic Pheromones (water, earth, ice 15 %).
    B1 counted them as protection, because their text says "damage taken".
Compared, as damage kept off the beastmaster over a danger of a given length, with the protections B1 counts: a 20 %
lower vulnerability for 48 s (Water Wall, Vulcanize, Harden Shell) and Beastskin (25 % physical, 30 s).

Rules compared:
  C1     a Release that stops counts as protection only against a pack - two or more enemies attacking you - when every
         one of them is within its reach and can be stopped: no boss, and for a stun none applied since the last
         immunity (the next stun is halved, The Balance). One attacker alone is the elite or boss case (his deaths of
         02.11.2026, A489), where an immunity the game does not show would leave the danger without any protection.
  ANY    counts as soon as one attacker can be stopped (the defect C1 avoids)
  C2     a reduction that names an element does not count (Strut, Formic Pheromones)
ASSUMPTIONS, not from a source, named where used: every add deals the same damage per second, a boss deals BOSS_WEIGHT
times that, and the damage of enemies is neither earth, water nor ice (most of it is physical or unaspected; no sheet
gives an enemy's element).
"""
import sys

STOPS = {"stun": 15.0, "deep freeze": 20.0, "petrify": 21.0}
MITIGATION = (0.20, 48.0)
BEASTSKIN = (0.25, 30.0)
ELEMENT_ONLY = (0.15, 60.0)  # Strut: earth damage taken -15 % for 60 s
BOSS_WEIGHT = 4.0  # ASSUMPTION: a boss hits four times as hard as an add
WINDOWS = (10.0, 15.0, 20.0, 30.0, 48.0, 60.0)


ELEMENTS = ("fire", "ice", "wind", "earth", "lightning", "water")

# (text, protects) - the parts of the effect texts the rule reads
TEXTS = [
    ("reducing earth damage taken by 15% for nearby party members, and occasionally nullifying it entirely.", False),
    ("afflicting all nearby enemies with Slow. Duration: 30s Additional Effect: Reduces water, earth, and ice damage taken "
     "by 15% for nearby party members, and occasionally nullifies it entirely", False),
    ("reducing nearby party members' magic vulnerability by 20% and fire damage taken by 15%, occasionally nullifying it "
     "entirely.", True),
    ("reducing nearby party members' magic vulnerability by 20%.", True),
    ("reducing nearby party members' physical vulnerability by 20%.", True),
    ("Reduces physical damage taken by 25% and increases critical hit rate by 5%.", True),
    ("Creates a barrier around self that absorbs magic damage totaling 8% of maximum HP.", True),
    ("Afflicts nearby enemies with Seeds Sown. Seeds Sown Effect: Reduces damage dealt by 10%", True),
]


def protects_old(text, on_party):
    if not text or (on_party and on_party not in text):
        return False
    return (("vulnerability by" in text and "reducing" in text) or "damage taken" in text or "absorbs" in text
            or "Reduces damage dealt" in text)


def protects_c2(text, on_party):
    """As before, but a "damage taken" whose clause names an element does not count."""
    if not text or (on_party and on_party not in text):
        return False
    general_taken = False
    at = text.find("damage taken")
    while at >= 0:
        start = max(text.rfind("educ", 0, at), 0)
        if not any(element in text[start:at].lower() for element in ELEMENTS):
            general_taken = True
        at = text.find("damage taken", at + 1)
    return (("vulnerability by" in text and "reducing" in text) or general_taken or "absorbs" in text
            or "Reduces damage dealt" in text)


def incoming(adds, bosses):
    return adds + bosses * BOSS_WEIGHT


def prevented_by_stop(seconds, adds, bosses, window, halved=False):
    """Damage kept off: the adds stopped for the stop's length within the window; bosses are immune."""
    length = seconds / 2.0 if halved else seconds
    return adds * min(length, window)


def prevented_by_reduction(share, seconds, adds, bosses, window):
    return share * incoming(adds, bosses) * min(seconds, window)


PACK = 2  # C1 counts a stop only against two or more attackers


def counts_c1(adds, bosses, fresh_stun=True, kind="stun"):
    return adds >= PACK and bosses == 0 and (kind != "stun" or fresh_stun)


def counts_any(adds, bosses, fresh_stun=True, kind="stun"):
    return adds > 0


def cases():
    for adds in range(0, 5):
        for bosses in range(0, 2):
            for kind, seconds in STOPS.items():
                for fresh in (True, False):
                    for window in WINDOWS:
                        yield adds, bosses, kind, seconds, fresh, window


def worse_than_mitigation(rule):
    """Cases where the rule counts the stopping Release as protection, but a 20 % mitigation would have kept more off."""
    bad = []
    for adds, bosses, kind, seconds, fresh, window in cases():
        if adds + bosses == 0 or not rule(adds, bosses, fresh, kind):
            continue
        stop = prevented_by_stop(seconds, adds, bosses, window, halved=(kind == "stun" and not fresh))
        mitigation = prevented_by_reduction(*MITIGATION, adds, bosses, window)
        if stop < mitigation:
            bad.append((adds, bosses, kind, fresh, window, stop, mitigation))
    return bad


def missed_by_old(rule):
    """Cases where B1 counted no stopping Release, though under the rule it keeps more off than Beastskin would."""
    gained = 0
    for adds, bosses, kind, seconds, fresh, window in cases():
        if adds + bosses == 0 or not rule(adds, bosses, fresh, kind):
            continue
        stop = prevented_by_stop(seconds, adds, bosses, window, halved=(kind == "stun" and not fresh))
        if stop > prevented_by_reduction(*BEASTSKIN, adds, bosses, window):
            gained += 1
    return gained


def self_test():
    # C1 never counts a stop where a 20 % mitigation would have kept more off.
    assert not worse_than_mitigation(counts_c1), worse_than_mitigation(counts_c1)[:3]
    # Constructed defect: counting as soon as one attacker can be stopped loses against a boss.
    assert any(case[1] > 0 for case in worse_than_mitigation(counts_any))
    # A halved stun loses against the mitigation over a long danger; C1 therefore wants a fresh one.
    def no_fresh_check(adds, bosses, fresh_stun=True, kind="stun"):
        return adds >= PACK and bosses == 0
    assert any(not case[3] for case in worse_than_mitigation(no_fresh_check))
    # C1 counts stops that keep more off than Beastskin in many cases B1 did not count at all.
    assert missed_by_old(counts_c1) > 0
    # One attacker alone is never counted: an immune elite would leave the danger unprotected.
    assert not any(counts_c1(1, 0, True, kind) for kind in STOPS)
    # C2, mirrored from BeastmasterFamiliars.Protects (a mirror: it tests this rule, not the C# code), on the effect
    # texts of the Releases and Beast Mode actions that say "damage taken" or "vulnerability" (xivapi v2, 02.11.2026).
    for text, expected in TEXTS:
        assert protects_c2(text, "nearby party members" if "party" in text else "") == expected, text
    assert protects_old(TEXTS[0][0], "nearby party members"), "the old rule counted Strut"
    print("self-test ok: C1 never counts a stop that keeps less off than a 20 % mitigation, counting with a boss among "
          "the attackers or a halved stun is caught, C1 gains cases B1 missed, and C2 counts every general reduction "
          "but no element-only one (Strut, Formic Pheromones), which the old rule counted")


def report():
    print(f"stops counted by C1 that keep more off than Beastskin: {missed_by_old(counts_c1)}")
    for adds in (1, 2, 3):
        for window in (15.0, 30.0, 60.0):
            stun = prevented_by_stop(STOPS["stun"], adds, 0, window)
            mitigation = prevented_by_reduction(*MITIGATION, adds, 0, window)
            print(f"  {adds} add(s), danger {window:4.0f} s: stun keeps off {stun:5.1f}, 20 % mitigation {mitigation:5.1f}"
                  " (add-seconds)")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
