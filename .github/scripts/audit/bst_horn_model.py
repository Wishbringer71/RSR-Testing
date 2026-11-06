#!/usr/bin/env python3
"""Which battlehorn the beastmaster calls (concept 01, "Begleiter nach Lage"; A485).

Not a code scanner - a model calculator. It plays the rotation's familiar cycle as BST_Reborn runs it: a call grants One
with Nature, the horn spends it on Tempered Release or Borrow, and while another horn is ready Parting Blow follows and the
next familiar comes; with none ready the familiar stays until the first horn is back, 90 s after its retreat. Three ways to
choose among the ready horns are compared:
  FIXED  the configured order (first, second, third horn)
  RULES  B2 danger first: a familiar that heals or protects is called; a running one that does neither leaves by
            Parting Blow for a ready one that does; one whose Trick heals stays out while the danger lasts - a Tempered
            Release's effect outlasts the familiar, so one that protects by it goes on as the cycle has it
         B3 the familiar whose Trick brings the least against the enemies around the target first - the last one stays;
            at equal damage the one that heals or protects keeps the long stint
  WEAKEN RULES, and a Tempered Release that weakens what another ready familiar's Trick deals, or strengthens you, first
         (the rejected B4: kept here to show why)
  KEEPALL RULES, but every familiar that heals or protects stays out while the danger lasts (the draft of B2, rejected)
  RESERVE RULES, but a familiar leaves only while, after the next call, another horn stays ready - always a successor
         in reserve (his proposal of 06.11.2026, A639)
  SAFE_RESERVE RULES, but a horn whose familiar heals or protects is called only in a danger or when no other is ready:
         the cycle runs on the others, and the successor in reserve is one that helps in a danger (A639, the variant)
  KEEP_HEALER RULES, but a familiar whose Trick heals is called after the others and then stays out to the end - its
         healing runs before any danger (A639, the counter-variant)
  EVEN   RULES, but each familiar stays half a recast, so the calls come evenly spaced instead of three at the pull (his
         proposal of 06.11.2026 as spacing, A640)
  SPREAD RULES at the pull; once no other horn is ready, the familiar out leaves halfway to the second horn's return
         instead of at the first's, so the next one is in reserve for half of each stretch (A640, the variant)
Measured: potency over the fight; in a danger the seconds until a familiar that heals or protects is out, and the share
of the danger covered - a familiar whose Trick heals out, or a protection of a Tempered Release running.

From the game sheets (xivapi v2, Action and ActionTransient, 02.11.2026): Trick potency 500 (single, circle around the
target, cone or line by CastType), Parting Blow 1,000 or 1,500 under Lingering Vantage, Tempered Release figures as below,
battlehorn 90 s from retreat (tool sources; on board 1 about 66 s after Parting Blow, his ACT log of 06.11.2026, A640),
Trick 3 s, Tempered Release and Borrow 30 s, Parting Blow 10 s.
ASSUMPTIONS, not from a source, named where used: a Trick every TRICK_EVERY seconds (familiar TP comes from auto-attacks),
the beastmaster's own potency per second, how long a short stint lasts, -20 % resistance read as +20 % damage taken, and
that every enemy in a pack stands within each area's radius of the target.
"""
import sys

FIGHT = 180.0
HORN_RECAST = 90.0
SHORT_STINT = 5.0       # ASSUMPTION: Tempered Release or Borrow, one Trick, then Parting Blow - two GCDs
TRICK_EVERY = 7.5       # ASSUMPTION: familiar TP for one Trick every three GCDs
OWN_PER_SECOND = 200.0  # ASSUMPTION: the beastmaster's own potency per second, physical
PARTING = 1500.0

# name: trick (potency, area?, element), release (kind, value, element/type, seconds), safe by: "trick" (its Trick heals,
# only while it is out), "release" (its Tempered Release protects, for the release's seconds), None
FAMILIARS = {
    "Wespe": dict(trick=(500, False, "pierce"), release=("damage", 2000, None, 0), safe=None),
    "Coblyn": dict(trick=(500, False, "lightning"), release=("protect", 0.20, "physical", 48), safe="release"),
    "Puk": dict(trick=(500, True, "fire"), release=("damage", 750, None, 0), safe=None),
    "Raptor": dict(trick=(500, True, "ice"), release=("weaken", 0.20, "fire", 60), safe=None),
    "Slime": dict(trick=(500, False, "drain"), release=("damage", 550, None, 0), safe="trick"),
    "Mantis": dict(trick=(500, False, "slash"), release=("weaken", 0.10, "physical", 30), safe=None),
    "Dullahan": dict(trick=(450, True, "slash"), release=("buff", 0.10, "physical", 30), safe=None),
}
# ASSUMPTION: a second familiar whose Trick heals, like the Slime - B5 sets two that heal or protect before a boss (A639).
FAMILIARS["Slime II"] = dict(FAMILIARS["Slime"])
PHYSICAL = {"pierce", "slash", "blunt"}


def trick_value(name, enemies):
    potency, area, _ = FAMILIARS[name]["trick"]
    return potency * (enemies if area else 1)


def benefits(provider, beneficiary):
    kind, _, what, _ = FAMILIARS[provider]["release"]
    element = FAMILIARS[beneficiary]["trick"][2]
    if kind == "weaken":
        return element == what or (what == "physical" and element in PHYSICAL)
    return kind == "buff"


def choose(ready, rule, enemies, danger, ttk):
    """The horn to call now among the ready ones (familiar names, in the configured order)."""
    if rule == "FIXED":
        return ready[0]
    # RULES, WEAKEN, RESERVE and SAFE_RESERVE
    if rule == "SAFE_RESERVE" and not danger and any(not FAMILIARS[name]["safe"] for name in ready):
        ready = [name for name in ready if not FAMILIARS[name]["safe"]]
    if rule == "KEEP_HEALER" and any(FAMILIARS[name]["safe"] != "trick" for name in ready):
        ready = [name for name in ready if FAMILIARS[name]["safe"] != "trick"]
    if danger:
        safe = [name for name in ready if FAMILIARS[name]["safe"]]
        if safe:
            return safe[0]
    order = sorted(ready, key=lambda name: (trick_value(name, enemies), bool(FAMILIARS[name]["safe"]), ready.index(name)))
    if rule == "WEAKEN" and ttk > SHORT_STINT * 2:
        for name in list(order):
            if any(other != name and benefits(name, other) for other in order):
                order.remove(name)
                order.insert(0, name)
                break
    return order[0]


def keeps_out(name, rule):
    """Whether the familiar stays out while the danger lasts, under the rule."""
    if rule == "KEEPALL":
        return bool(FAMILIARS[name]["safe"])
    return rule != "FIXED" and FAMILIARS[name]["safe"] == "trick"


def play(horns, rule, enemies_at, danger_from=None, measure=None, fight=FIGHT, recast=HORN_RECAST):
    """Potency over the fight and the seconds from the danger to a safe familiar; with measure a dict, it also gets
    the share of the danger covered."""
    ready_at = {name: 0.0 for name in horns}
    t, total, effects, heal_at = 0.0, 0.0, [], None
    current, leave_at, next_trick = None, 0.0, 0.0
    covered = danger_steps = calls = 0
    successor = out_steps = 0
    step = 0.5
    while t < fight:
        enemies = enemies_at(t)
        danger = danger_from is not None and t >= danger_from
        if danger and current and not FAMILIARS[current]["safe"] and rule != "FIXED" \
                and any(FAMILIARS[n]["safe"] and ready_at[n] <= t for n in horns):
            leave_at = t
        if current and t >= leave_at and danger and keeps_out(current, rule):
            leave_at = t + step
        if current and t >= leave_at:
            total += PARTING * enemies
            ready_at[current] = t + recast
            current = None
        if current is None:
            ready = [n for n in horns if ready_at[n] <= t]
            if ready:
                current = choose(ready, rule, enemies, danger, ttk=fight - t)
                calls += 1
                ready_at[current] = float("inf")
                if rule == "SPREAD":
                    others_due = sorted(ready_at[n] for n in horns if n != current)
                    if others_due and others_due[0] <= t:
                        leave_at = t + SHORT_STINT
                    elif len(others_due) >= 2:
                        leave_at = max(others_due[0], (t + others_due[1]) / 2)
                    else:
                        leave_at = others_due[0] if others_due else float("inf")
                elif rule == "EVEN":
                    # Half a recast out: the next horn is back when it leaves, the first again a half later.
                    due = min((ready_at[n] for n in horns if n != current), default=float("inf"))
                    leave_at = max(t + recast / 2, due)
                elif rule == "KEEP_HEALER" and FAMILIARS[current]["safe"] == "trick":
                    leave_at = float("inf")
                elif rule == "SAFE_RESERVE" and not FAMILIARS[current]["safe"]:
                    # Leave once another horn that is not the reserve is ready.
                    due = min((ready_at[n] for n in horns if n != current and not FAMILIARS[n]["safe"]),
                              default=float("inf"))
                    leave_at = max(t + SHORT_STINT, due)
                elif rule == "RESERVE":
                    # Leave once two other horns are ready: one follows, one stays in reserve.
                    others_due = sorted(ready_at[n] for n in horns if n != current)
                    due = others_due[1] if len(others_due) >= 2 else float("inf")
                    leave_at = max(t + SHORT_STINT, due)
                else:
                    others = any(ready_at[n] <= t for n in horns if n != current)
                    leave_at = t + SHORT_STINT if others else min(ready_at[n] for n in horns if n != current)
                kind, value, what, seconds = FAMILIARS[current]["release"]
                if kind == "damage":
                    total += value * enemies
                elif kind in ("weaken", "buff", "protect"):
                    effects.append((kind, value, what, t + seconds))
                next_trick = t
        effects = [e for e in effects if e[3] > t]
        if current:
            out_steps += 1
            successor += any(ready_at[n] <= t for n in horns if n != current)
        if danger and heal_at is None and current and FAMILIARS[current]["safe"]:
            heal_at = t - danger_from
        if danger:
            danger_steps += 1
            if (current and FAMILIARS[current]["safe"] == "trick") or any(e[0] == "protect" for e in effects):
                covered += 1

        def bonus(element):
            gain = 1.0
            for kind, value, what, _ in effects:
                if kind == "protect":
                    continue
                if what == element or (what == "physical" and element in PHYSICAL):
                    gain += value
            return gain

        total += OWN_PER_SECOND * step * bonus("pierce")
        if current and t >= next_trick:
            total += trick_value(current, enemies) * bonus(FAMILIARS[current]["trick"][2])
            next_trick = t + TRICK_EVERY
        t += step
    if measure is not None:
        measure["covered"] = covered / danger_steps if danger_steps else 0.0
        # Each call brings a Tempered Release or Borrow and, at the retreat, a Parting Blow.
        measure["calls"] = calls
        # The share of the time a familiar is out with another horn ready to follow it.
        measure["successor"] = successor / out_steps if out_steps else 0.0
        # The familiar still out when the fight ends - it keeps its horn into the next fight (B5, bst_assign_model).
        measure["last"] = current
    return total, heal_at


SCENARIOS = {
    "single target, 180 s": lambda t: 1,
    "pack of four for 40 s, then one": lambda t: 4 if t < 40 else 1,
    "pack of four throughout": lambda t: 4,
}
SETS = {
    "Wespe, Coblyn, Puk": ["Wespe", "Coblyn", "Puk"],
    "Puk, Wespe, Coblyn": ["Puk", "Wespe", "Coblyn"],
    "Puk, Wespe, Raptor": ["Puk", "Wespe", "Raptor"],
    "Wespe, Mantis, Slime": ["Wespe", "Mantis", "Slime"],
}


def report():
    print("Potency over the fight: FIXED, RULES (change), WEAKEN (change):")
    for set_name, horns in SETS.items():
        for scenario, enemies in SCENARIOS.items():
            fixed, _ = play(horns, "FIXED", enemies)
            rules, _ = play(horns, "RULES", enemies)
            weaken, _ = play(horns, "WEAKEN", enemies)
            print(f"  {set_name:22s} {scenario:32s} {fixed:9.0f}  {rules:9.0f} ({(rules / fixed - 1) * 100:+5.1f} %)"
                  f"  {weaken:9.0f} ({(weaken / fixed - 1) * 100:+5.1f} %)")
    print("\nSeconds from the danger until a familiar that heals or protects is out (horns Slime, Wespe, Mantis):")
    for scenario, enemies in SCENARIOS.items():
        for start in (2.0, 40.0, 100.0):
            cells = []
            for rule in ("FIXED", "RULES"):
                _, heal = play(["Slime", "Wespe", "Mantis"], rule, enemies, danger_from=start)
                cells.append(f"{rule} {heal if heal is not None else 'never'}")
            print(f"  {scenario:32s} danger from {start:5.1f} s: " + ", ".join(cells))
    print("\nShare of a danger to the end covered, and potency, one enemy:")
    for horns in (["Slime", "Wespe", "Coblyn"], ["Coblyn", "Slime", "Wespe"], ["Wespe", "Coblyn", "Slime"]):
        for start in (2.0, 40.0, 100.0):
            cells = []
            for rule in ("FIXED", "RULES", "KEEPALL"):
                m = {}
                total, _ = play(horns, rule, SCENARIOS["single target, 180 s"], danger_from=start, measure=m)
                cells.append(f"{rule} {m['covered']:5.1%} {total:8.0f}")
            print(f"  {', '.join(horns):22s} from {start:5.1f} s: " + "   ".join(cells))


RESERVE_RULES = ("RULES", "RESERVE", "SAFE_RESERVE", "KEEP_HEALER")
RESERVE_SETS = (["Wespe", "Mantis", "Slime"], ["Puk", "Wespe", "Slime"], ["Puk", "Wespe", "Coblyn"])


def report_reserve():
    print("\nA successor always ready (A639): calls and potency against RULES, no danger:")
    for fight, scenario in ((30.0, "pack of four throughout"), (60.0, "pack of four throughout"),
                            (180.0, "single target, 180 s"), (180.0, "pack of four for 40 s, then one")):
        for horns in RESERVE_SETS:
            cells, base = [], None
            for rule in RESERVE_RULES:
                m = {}
                total, _ = play(horns, rule, SCENARIOS[scenario], measure=m, fight=fight)
                base = base or total
                cells.append(f"{rule} {m['calls']} {(total / base - 1) * 100:+5.1f} %")
            print(f"  {', '.join(horns):20s} {scenario:24s} {fight:4.0f} s ({base:6.0f}): " + ", ".join(cells))
    print("\n  A danger that starts at any second of the fight (every 2.5 s) and lasts to its end: the share of starts with a "
          "familiar that heals or protects out within a GCD, and the share of the danger covered:")
    for horns in RESERVE_SETS + (["Wespe", "Slime", "Slime II"],):
        for fight, scenario in ((180.0, "single target, 180 s"), (180.0, "pack of four for 40 s, then one"),
                                (30.0, "pack of four throughout"), (60.0, "pack of four throughout")):
            cells = []
            for rule in RESERVE_RULES:
                fast, covered = danger_sweep(horns, rule, SCENARIOS[scenario], fight=fight)
                cells.append(f"{rule} {fast:5.1%} {covered:5.1%}")
            print(f"  {', '.join(horns):20s} {scenario:24s} {fight:4.0f} s " + "   ".join(cells))
    print("\n  Two familiars whose Trick heals (B5 before a boss), one enemy, 180 s, no danger:")
    for rule in RESERVE_RULES:
        m = {}
        total, _ = play(["Wespe", "Slime", "Slime II"], rule, SCENARIOS["single target, 180 s"], measure=m)
        print(f"  {rule:12s} calls {m['calls']}, potency {total:6.0f}")


def report_schedules():
    print("\nSchedules (A640): calls, potency against RULES and the share of time with a successor ready, by fight length;"
          " recast from the retreat, 90 s outside the Crucible, at most 66 s after Parting Blow on board 1 (06.11.2026):")
    for recast in (90.0, 66.0):
        for scenario in ("single target, 180 s", "pack of four throughout"):
            for fight in (30.0, 60.0, 120.0, 180.0, 300.0):
                cells, base = [], None
                for rule in ("RULES", "SPREAD", "EVEN", "RESERVE"):
                    m = {}
                    total, _ = play(["Puk", "Wespe", "Slime"], rule, SCENARIOS[scenario], measure=m, fight=fight,
                                    recast=recast)
                    base = base or total
                    cells.append(f"{rule} {m['calls']} calls {(total / base - 1) * 100:+5.1f} % successor {m['successor']:4.0%}")
                print(f"  recast {recast:3.0f} s  {scenario:24s} {fight:4.0f} s: " + ", ".join(cells))
    print("\n  Board 1 (recast 66 s), fights of 80 to 150 s as measured there, mean against RULES:")
    for scenario in ("single target, 180 s", "pack of four throughout"):
        for rule in ("SPREAD", "EVEN", "RESERVE"):
            gains, today, spread = [], [], []
            for fight in range(80, 151, 5):
                mr, mx = {}, {}
                r, _ = play(["Puk", "Wespe", "Slime"], "RULES", SCENARIOS[scenario], measure=mr, fight=float(fight),
                            recast=66.0)
                x, _ = play(["Puk", "Wespe", "Slime"], rule, SCENARIOS[scenario], measure=mx, fight=float(fight),
                            recast=66.0)
                gains.append(x / r - 1)
                today.append(mr["successor"])
                spread.append(mx["successor"])
            print(f"  {scenario:24s} {rule:8s} potency {sum(gains) / len(gains) * 100:+5.1f} %, a successor ready "
                  f"{sum(today) / len(today):4.0%} -> {sum(spread) / len(spread):4.0%}")


def danger_sweep(horns, rule, enemies_at, gcd=2.5, fight=FIGHT):
    """Over danger starts every GCD: the share with a familiar that heals or protects out within a GCD, and the mean
    share of the danger covered."""
    starts = [i * gcd for i in range(int(fight / gcd) - 1)]
    fast = covered = 0.0
    for start in starts:
        m = {}
        _, heal = play(horns, rule, enemies_at, danger_from=start, measure=m, fight=fight)
        fast += heal is not None and heal <= gcd
        covered += m["covered"]
    return fast / len(starts), covered / len(starts)


def self_test():
    # One enemy: the rules never lose potency against the configured order (every Trick alike; only which familiar keeps
    # the long stint, and so how often each Tempered Release comes, can change).
    for horns in SETS.values():
        f, _ = play(horns, "FIXED", SCENARIOS["single target, 180 s"])
        r, _ = play(horns, "RULES", SCENARIOS["single target, 180 s"])
        assert r >= f - 1e-6, (horns, f, r)
    # A pack: the area familiar keeps the long stint - more than the configured order with it first.
    f, _ = play(["Puk", "Wespe", "Coblyn"], "FIXED", SCENARIOS["pack of four throughout"])
    r, _ = play(["Puk", "Wespe", "Coblyn"], "RULES", SCENARIOS["pack of four throughout"])
    assert r > f * 1.05, (f, r)
    # Danger: the healing familiar comes at once under the rules, under the fixed order much later.
    _, hf = play(["Slime", "Wespe", "Mantis"], "FIXED", SCENARIOS["single target, 180 s"], danger_from=40.0)
    _, hr = play(["Slime", "Wespe", "Mantis"], "RULES", SCENARIOS["single target, 180 s"], danger_from=40.0)
    assert hr is not None and hr <= 1.0 and (hf is None or hf > hr + 10), (hf, hr)
    # A danger that lasts: the healing familiar stays out instead of leaving by Parting Blow when a horn comes back.
    _, hl = play(["Slime", "Wespe", "Mantis"], "RULES", SCENARIOS["single target, 180 s"], danger_from=60.0)
    assert hl == 0.0, hl
    # A danger that lasts with a familiar that protects by its Tempered Release first: it goes on, the healing one comes
    # and stays - the danger is covered longer than when the protecting one is kept out past its release.
    mr, mk = {}, {}
    play(["Slime", "Wespe", "Coblyn"], "RULES", SCENARIOS["single target, 180 s"], danger_from=40.0, measure=mr)
    play(["Slime", "Wespe", "Coblyn"], "KEEPALL", SCENARIOS["single target, 180 s"], danger_from=40.0, measure=mk)
    assert mr["covered"] > mk["covered"] + 0.2, (mr, mk)
    # The rejected B4 loses on one enemy where RULES does not.
    w, _ = play(["Puk", "Wespe", "Raptor"], "WEAKEN", SCENARIOS["single target, 180 s"])
    f, _ = play(["Puk", "Wespe", "Raptor"], "FIXED", SCENARIOS["single target, 180 s"])
    assert w < f, (w, f)
    # On one enemy the healing familiar keeps the long stint without costing potency.
    f, _ = play(["Slime", "Wespe", "Coblyn"], "FIXED", SCENARIOS["single target, 180 s"])
    r, _ = play(["Slime", "Wespe", "Coblyn"], "RULES", SCENARIOS["single target, 180 s"])
    assert r >= f - 1e-6, (f, r)
    # A639: a successor always in reserve costs calls and potency in every fight; left to the order of B3 it covers a
    # danger worse than today on a pack, while two familiars whose Trick heals already relieve each other.
    for scenario in SCENARIOS:
        mr, mv = {}, {}
        r, _ = play(["Puk", "Wespe", "Slime"], "RULES", SCENARIOS[scenario], measure=mr)
        v, _ = play(["Puk", "Wespe", "Slime"], "RESERVE", SCENARIOS[scenario], measure=mv)
        assert mv["calls"] < mr["calls"] and v < r, (scenario, mr, mv, r, v)
    _, today = danger_sweep(["Puk", "Wespe", "Slime"], "RULES", SCENARIOS["pack of four for 40 s, then one"])
    _, reserve = danger_sweep(["Puk", "Wespe", "Slime"], "RESERVE", SCENARIOS["pack of four for 40 s, then one"])
    assert reserve < today - 0.2, (today, reserve)
    fast, _ = danger_sweep(["Wespe", "Slime", "Slime II"], "RULES", SCENARIOS["single target, 180 s"])
    assert fast == 1.0, fast
    lone, _ = danger_sweep(["Puk", "Wespe", "Slime"], "RULES", SCENARIOS["pack of four throughout"], fight=60.0)
    held, _ = danger_sweep(["Puk", "Wespe", "Slime"], "SAFE_RESERVE", SCENARIOS["pack of four throughout"], fight=60.0)
    assert lone < 0.5 and held == 1.0, (lone, held)
    # A640: his spacing loses the pull - one call in a 30 s fight instead of three; the variant that splits only the
    # stretches after the pull keeps the pull's calls and brings a successor more often in a long fight.
    me, mr, ms = {}, {}, {}
    play(["Puk", "Wespe", "Slime"], "EVEN", SCENARIOS["pack of four throughout"], measure=me, fight=30.0)
    play(["Puk", "Wespe", "Slime"], "RULES", SCENARIOS["pack of four throughout"], measure=mr, fight=180.0)
    play(["Puk", "Wespe", "Slime"], "SPREAD", SCENARIOS["pack of four throughout"], measure=ms, fight=180.0)
    assert me["calls"] == 1 and ms["calls"] == mr["calls"] and ms["successor"] > mr["successor"] + 0.1, (me, mr, ms)
    # Constructed defect: keeping the best Trick first instead of last loses against the fixed order on a pack.
    global choose
    original = choose

    def best_first(ready, rule, enemies, danger, ttk):
        if rule == "RULES":
            return max(ready, key=lambda name: trick_value(name, enemies))
        return original(ready, rule, enemies, danger, ttk)

    choose = best_first
    try:
        bad, _ = play(["Wespe", "Coblyn", "Puk"], "RULES", SCENARIOS["pack of four throughout"])
        good_f, _ = play(["Wespe", "Coblyn", "Puk"], "FIXED", SCENARIOS["pack of four throughout"])
        assert bad < good_f, (bad, good_f)
    finally:
        choose = original
    print("self-test ok: one enemy loses nothing, a pack keeps the area familiar longest, danger brings the healing "
          "familiar at once and keeps it while the danger lasts, a protecting one is not kept past its release, the rejected "
          "B4 loses on one enemy, a successor in reserve costs calls and on a pack cover, two healing familiars relieve "
          "each other, his spacing loses the pull while splitting only later stretches keeps it, and calling the best Trick "
          "first is caught")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
        report_reserve()
        report_schedules()
