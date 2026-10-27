#!/usr/bin/env python3
"""Heal over time against damage over time: today's threshold rule against a rule by effective healing (concept 08,
"Schaden über Zeit: HoT gegen DoT", the owner's question of 27.10.2026).

Not a code scanner - a model calculator. One member, health in percent of his maximum, steps of 3 s: every damage over
time and every heal over time ticks once per step on the member's own clock (DotTickTracker; cactbot LogGuide, line 24,
one aggregated tick per target). Per step: the DoT ticks, the HoT ticks, then the healer may spend one heal GCD (a GCD
of 2.5 s fits about once into a step).

  TODAY   the White Mage at its defaults: a single heal when the health a heal would land on is below HealthSingleSpell
          (65 %), lowered towards HealthSingleSpellHot (55 %) by the HoT's remaining time over 15 s; Regen first while
          the member stands above RegenHeal (30 %) and carries none, else Cure II
  EFFECT  the proposal: heal only if, without a heal, the member would fall below the same 65 % within Regen's time;
          then the means that heals the most missing health per GCD - Regen when the health it would land on (the
          backlog, the DoT and other damage over its six ticks) is at least what Cure II would land on; Cure II below
          30 % as today; a Cure II beside a running Regen only when all of it lands on missing health

Measured per scenario and rule: heal GCDs, MP, the share of healing that met full health (overheal), the lowest health.
Potencies from the job guide (27.10.2026): Regen 250 per tick for 18 s, 400 MP; Cure II 800, 1000 MP; Medica III 250
and 175 per tick for 15 s, 1000 MP; Cure III 600 within 10 y, 1500 MP; Medica 400, 900 MP. ASSUMPTIONS, not from a
source: a Regen tick heals 3.4 % of the member's health (the median HoT tick on players in his log of 26.10.2026); the
DoT and other damage below; one heal GCD per step.

The second part answers his follow-up for damage over time on several members: which means heals the most missing
health per GCD and per MP, by how many members carry the DoT and how strong it is against one Regen tick.

Usage: python3 .github/scripts/audit/dot_hot_model.py [--self-test]
"""
import sys

REGEN_TICK = 3.4                      # % of max HP per Regen tick (assumption, see above)
POT_PER_PCT = 250 / REGEN_TICK        # potency per percent of max HP
CURE2 = 800 / POT_PER_PCT
REGEN_TICKS = 6                        # 18 s / 3 s
MP = {"Regen": 400, "Cure II": 1000, "Medica III": 1000, "Cure III": 1500, "Medica": 900}
SINGLE, SINGLE_HOT, REGEN_FLOOR, HOT_WHOLE = 65.0, 55.0, 30.0, 15.0


def lerp(a, b, t):
    return a + (b - a) * t


class Member:
    def __init__(self, hp=100.0):
        self.hp, self.regen, self.healed, self.over, self.low = hp, 0, 0.0, 0.0, hp

    def heal(self, amount):
        landed = min(amount, 100.0 - self.hp)
        self.hp += landed
        self.healed += landed
        self.over += amount - landed


def project(hp, regen, dots, others, steps, regen_tick=REGEN_TICK):
    """The member's health over the next steps without a new heal, and the healing a new Regen would land."""
    low, landed, with_regen = hp, 0.0, hp
    for k in range(steps):
        loss = (dots[k] if k < len(dots) else 0.0) + (others[k] if k < len(others) else 0.0)
        hp = min(100.0, hp - loss + (regen_tick if k < regen else 0.0))
        low = min(low, hp)
        with_regen -= loss
        gain = min(regen_tick, 100.0 - with_regen)
        with_regen += gain
        landed += gain
    return low, landed


def rule_today(m, dot_next, other_next):
    forecast = m.hp - dot_next - other_next + (REGEN_TICK if m.regen else 0.0)
    threshold = lerp(SINGLE, SINGLE_HOT, min(1.0, m.regen * 3 / HOT_WHOLE))
    if forecast >= threshold:
        return None
    return "Regen" if forecast > REGEN_FLOOR and m.regen == 0 else "Cure II"


def rule_effect(m, dots_ahead, others_ahead):
    low, regen_lands = project(m.hp, m.regen, dots_ahead, others_ahead, REGEN_TICKS)
    landing = m.hp - (dots_ahead[0] if dots_ahead else 0.0) - (others_ahead[0] if others_ahead else 0.0)
    if landing <= REGEN_FLOOR:
        return "Cure II"
    if low >= SINGLE:
        return None
    cure_lands = min(CURE2, 100.0 - landing)
    if m.regen == 0 and regen_lands >= cure_lands:
        return "Regen"
    return "Cure II" if cure_lands >= CURE2 or low <= REGEN_FLOOR else None


def run(rule, dots, others, steps, hp=100.0):
    m, gcds, mp = Member(hp), 0, 0
    for k in range(steps):
        loss = (dots[k] if k < len(dots) else 0.0) + (others[k] if k < len(others) else 0.0)
        m.hp = max(0.0, m.hp - loss)
        if m.regen:
            m.heal(REGEN_TICK)
            m.regen -= 1
        m.low = min(m.low, m.hp)
        if rule == "today":
            choice = rule_today(m, dots[k + 1] if k + 1 < len(dots) else 0.0, others[k + 1] if k + 1 < len(others) else 0.0)
        else:
            choice = rule_effect(m, dots[k + 1:], others[k + 1:])
        if choice == "Regen":
            m.regen, gcds, mp = REGEN_TICKS, gcds + 1, mp + MP["Regen"]
        elif choice == "Cure II":
            m.heal(CURE2)
            gcds, mp = gcds + 1, mp + MP["Cure II"]
    total = m.healed + m.over
    return {"gcds": gcds, "mp": mp, "over": m.over / total if total else 0.0, "low": m.low, "end": m.hp}


SCENARIOS = [
    # name, start health, DoT per step and steps, other damage per step and steps, steps watched
    ("weak DoT, 0.6 % per tick for 5 min (his log)", 100, (0.6, 100), (0, 0), 100),
    ("DoT as strong as Regen for 18 s", 100, (3.4, 6), (0, 0), 30),
    ("the same with 20 % missing", 80, (3.4, 6), (0, 0), 30),
    ("strong DoT, 6 % per tick for 30 s", 100, (6.0, 10), (0, 0), 30),
    ("DoT 3 % for 30 s on a tank under 2 % autos", 100, (3.0, 10), (2.0, 20), 30),
]


def single_table():
    rows = []
    for name, hp, (d, dn), (o, on), steps in SCENARIOS:
        dots, others = [d] * dn, [o] * on
        rows.append((name, run("today", dots, others, steps, hp), run("effect", dots, others, steps, hp)))
    return rows


def member_landing(means, backlog, dot, ticks_left):
    """Healing that lands on missing health on one member, in potency, for one cast of the means."""
    pct = {"Regen": (0.0, 250, 6), "Medica III": (250, 175, 5), "Cure II": (800, 0, 0), "Cure III": (600, 0, 0),
           "Medica": (400, 0, 0)}[means]
    first, tick, n = pct[0] / POT_PER_PCT, pct[1] / POT_PER_PCT, pct[2]
    missing = backlog
    landed = min(first, missing)
    missing -= landed
    for k in range(n):
        missing += dot if k < ticks_left else 0.0
        gain = min(tick, missing)
        missing -= gain
        landed += gain
    return landed * POT_PER_PCT


def group_table(dot_share, ticks_left=6, backlog=0.0):
    """For n members with a DoT of dot_share x a Regen tick: what each means lands in total, per GCD and per MP."""
    dot = dot_share * REGEN_TICK
    out = []
    for n in range(1, 9):
        per = {k: member_landing(k, backlog, dot, ticks_left) for k in ("Regen", "Cure II", "Medica III", "Medica")}
        landed = {"Regen": per["Regen"], "Cure II": per["Cure II"], "Medica III": per["Medica III"] * n,
                  "Medica": per["Medica"] * n}
        best = max(landed, key=lambda k: (round(landed[k]), -MP[k]))
        best_mp = max(landed, key=lambda k: (landed[k] / MP[k], landed[k]))
        out.append((n, landed, best, best_mp))
    return out


def report():
    print("One member (health in % of max; Regen tick 3.4 %, Cure II 10.9 %):")
    print(f"{'scenario':48} {'rule':7} {'GCDs':>4} {'MP':>6} {'overheal':>8} {'lowest':>7} {'end':>5}")
    for name, today, effect in single_table():
        for label, r in (("today", today), ("effect", effect)):
            print(f"{name:48} {label:7} {r['gcds']:>4} {r['mp']:>6} {r['over']:>8.0%} {r['low']:>6.1f}% {r['end']:>4.0f}%")
    print()
    print("Damage over time on n members, no backlog, six ticks left - healing landed on missing health (potency):")
    for share in (0.25, 0.5, 1.0, 2.0):
        print(f"  DoT tick = {share:g} x Regen tick")
        for n, landed, best, best_mp in group_table(share):
            cells = "  ".join(f"{k} {v:5.0f}" for k, v in landed.items())
            print(f"    n={n}: {cells}  -> per GCD {best}, per MP {best_mp}")


def self_test():
    rows = {name: (t, e) for name, t, e in single_table()}
    weak_t, weak_e = rows[SCENARIOS[0][0]]
    neutral_t, neutral_e = rows[SCENARIOS[1][0]]
    strong_t, strong_e = rows[SCENARIOS[3][0]]
    tank_t, tank_e = rows[SCENARIOS[4][0]]
    checks = {
        "the rule never lets the member fall lower than today by more than one Regen tick":
            all(e["low"] >= t["low"] - REGEN_TICK for _, t, e in single_table()),
        "a weak DoT costs the rule no more heals than today": weak_e["gcds"] <= weak_t["gcds"],
        "a weak DoT is healed with less overheal than today": weak_e["over"] <= weak_t["over"],
        "a DoT as strong as Regen is answered by Regen with nothing spilled": neutral_e["over"] <= 0.01 or neutral_e["gcds"] == 0,
        "a strong DoT gets at least one heal": strong_e["gcds"] >= 1 and strong_t["gcds"] >= 1,
        "the tank under autos and a DoT never falls under the Regen floor": tank_e["low"] > REGEN_FLOOR,
        # constructed defects the tests must catch
        "a member at full health with nothing coming draws no heal": run("effect", [], [], 10)["gcds"] == 0,
        "Medica III beats one Regen once enough members carry a strong DoT":
            group_table(1.0)[-1][2] == "Medica III" and group_table(1.0)[0][2] != "Medica III",
        "per MP one member with a DoT is Regen's": group_table(1.0)[0][3] == "Regen",
        "with a weak DoT on one member a single cast heals no more than it needs":
            member_landing("Regen", 0.0, 0.25 * REGEN_TICK, 6) < 1500,
    }
    failed = [name for name, ok in checks.items() if not ok]
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print(f"self-test ok: {len(checks)} invariants")
    return 0


if __name__ == "__main__":
    if "--self-test" in sys.argv:
        sys.exit(self_test())
    report()
    sys.exit(self_test())
