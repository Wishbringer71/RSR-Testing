#!/usr/bin/env python3
"""The downtime heal: whom "Use GCD heals when there's nothing else to do" heals (concept 07, "Die Füllheilung"; A477).

Not a code scanner - a model of the decision, in a closed loop. The downtime heal runs when no other GCD goes and the
lowest forecast health of the party is under its line ("When the lowest party HP is below"). The plugin set the heal
target choice but not its need filter, so the choice took anybody not at full health: in the Praetorium (his upload of
01.11.2026) Regen went on a black mage at 95 % while the tank who opened the heal at 75 % already carried Regen. The
model plays random downtime moments and counts, per rule, what the heal does.

ASSUMPTIONS, not from a source: a party of 4 or 8; each member's forecast health drawn at random, a third of them full;
a quarter of the members carry a HoT; one member in eight out of the heal's range; the job is a White Mage below level
52 (single heal: Regen on the first candidate without Regen and above 30 %, else Cure II on the first candidate; area
heal: Medica II around the caster); the danger order is the share of health among the candidates (class 3 without
aggro or an announced hit - the order of a quiet downtime); his line is 70, 80, 90 or 100 % with the action's heal ratio
at its default 80 %. Measured instead of assumed: nothing - the model checks the decision table, not amounts.

Rules (the downtime heal opens the same way under each; they differ in what it heals):
  TODAY     no need filter: every member not at full health is a candidate; the area heal opens when more than two
            members are not full and the spread is small, and goes out when anybody stands in its reach
  RATIO     the need filter at the action's heal ratio (80 %)
  LINE      the need filter at the downtime line; the area heal opens when more than two members are under the line
  TRIGGER   only the member who opened it (the lowest)

Counted per downtime moment that opened: a heal on a member at or over the line (over the line); the opener under the
line, in range and without a heal although nothing kept it from him (opener missed); an area heal that reached nobody
under the line (area for nobody).
"""
import random
import sys

TRIALS = 20000
LINES = (0.70, 0.80, 0.90, 1.00)
ACTION_RATIO = 0.80
REGEN_FLOOR = 0.30        # WHM_Reborn RegenHeal default: below it the emergency belongs to Cure II
SPREAD_LIMIT = 0.25       # "AoE heal HP spread" default
RULES = ("TODAY", "RATIO", "LINE", "TRIGGER")


def party(rng):
    size = rng.choice((4, 8))
    members = []
    for i in range(size):
        health = 1.0 if rng.random() < 1 / 3 else rng.uniform(0.2, 0.999)
        members.append({"id": i, "health": health, "hot": rng.random() < 0.25, "in_range": rng.random() >= 1 / 8})
    return members


def spread(values):
    if len(values) < 2:
        return 0.0
    mean = sum(values) / len(values)
    return (sum((v - mean) ** 2 for v in values) / len(values)) ** 0.5


def candidates(members, rule, line, opener):
    in_reach = [m for m in members if m["in_range"]]
    if rule == "TODAY":
        return [m for m in in_reach if m["health"] < 1.0]
    if rule == "RATIO":
        return [m for m in in_reach if m["health"] < ACTION_RATIO]
    if rule == "LINE":
        return [m for m in in_reach if m["health"] < line]
    return [opener] if opener["in_range"] else []


def downtime_heal(members, rule, line):
    """What the downtime heal casts: ("area", reached) or ("single", target) or None."""
    healths = [m["health"] for m in members]
    if min(healths) >= line:
        return None
    opener = min(members, key=lambda m: m["health"])
    open_count = sum(1 for h in healths if (h < line if rule == "LINE" else h < 1.0))
    if open_count > 2 and spread(healths) < SPREAD_LIMIT:
        reach = [m for m in members if m["in_range"]]
        need = candidates(members, rule, line, opener)
        if reach and (need or rule == "TODAY"):
            return ("area", reach)
    pool = sorted(candidates(members, rule, line, opener), key=lambda m: m["health"])
    for m in pool:
        if not m["hot"] and m["health"] > REGEN_FLOOR:
            return ("single", m)
    return ("single", pool[0]) if pool else None


def run(rule, line, seed=1):
    rng = random.Random(seed)
    opened = over = missed = area_nobody = 0
    for _ in range(TRIALS):
        members = party(rng)
        if min(m["health"] for m in members) >= line:
            continue
        opened += 1
        opener = min(members, key=lambda m: m["health"])
        cast = downtime_heal(members, rule, line)
        if cast is None:
            if opener["in_range"]:
                missed += 1
            continue
        kind, what = cast
        if kind == "area":
            if not any(m["health"] < line for m in what):
                area_nobody += 1
        else:
            if what["health"] >= line:
                over += 1
    return opened, over, missed, area_nobody


def report():
    print(f"Downtime moments that opened the heal, {TRIALS} drawn per line; counted per opened moment.")
    for line in LINES:
        print(f"\n  line {line:.0%} (action heal ratio {ACTION_RATIO:.0%})")
        for rule in RULES:
            opened, over, missed, area_nobody = run(rule, line)
            print(f"    {rule:8s} over the line {over / opened:6.1%}   opener missed {missed / opened:6.1%}   "
                  f"area for nobody {area_nobody / opened:6.1%}")


def self_test():
    # The defect the rule is built against: without the filter a member over the line gets the heal.
    tank = {"id": 0, "health": 0.75, "hot": True, "in_range": True}
    caster = {"id": 1, "health": 0.95, "hot": False, "in_range": True}
    full = [{"id": i, "health": 1.0, "hot": False, "in_range": True} for i in (2, 3)]
    praetorium = [tank, caster] + full
    assert downtime_heal(praetorium, "TODAY", 0.80) == ("single", caster), "TODAY must reproduce the Praetorium"
    assert downtime_heal(praetorium, "LINE", 0.80) == ("single", tank), "LINE heals the opener (Cure II)"
    # The action ratio filters the opener out when the line is set higher.
    high = [{"id": 0, "health": 0.85, "hot": False, "in_range": True}] + full
    assert downtime_heal(high, "RATIO", 0.90) is None and downtime_heal(high, "LINE", 0.90) is not None
    # An area heal opened by members just under full health while the one in need stands out of range.
    far = {"id": 0, "health": 0.70, "hot": False, "in_range": False}
    near = [{"id": i, "health": 0.95, "hot": False, "in_range": True} for i in (1, 2, 3)]
    assert downtime_heal([far] + near, "TODAY", 0.80)[0] == "area"
    assert downtime_heal([far] + near, "LINE", 0.80) is None
    # The model's own counters: LINE never heals over the line and never misses an opener in range.
    for line in LINES:
        opened, over, missed, area_nobody = run("LINE", line, seed=7)
        assert opened > 0 and over == 0 and missed == 0 and area_nobody == 0, (line, over, missed, area_nobody)
    # A constructed defect is caught: TODAY heals over the line at the default line.
    assert run("TODAY", 0.80, seed=7)[1] > 0
    print("self-test ok: TODAY reproduces the Praetorium, LINE heals the opener, RATIO loses him under a higher line, "
          "an area heal for nobody is found, and LINE never heals over the line nor misses an opener in range")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
