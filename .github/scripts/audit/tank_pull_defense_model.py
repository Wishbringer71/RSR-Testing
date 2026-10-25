#!/usr/bin/env python3
"""A tank's own defence in pulls: by the number of enemies on him, or by his measured fall (concept 09, "Die Abwehr im
Pull"; concept 18, "Abwehr"; A359).

Not a code scanner - a model calculator, in a closed loop: one healer heals by the forecast, as in party_ttk_model.py
(GCD heal of 18 % when the health at its landing is below 65 %, an instant one of 25 % every 20 s below 70 %), and the
tank answers by one of the rules below. The forecast is the fork's own (ANCHOR, corrected like ScoreTtkForecast),
taken from party_ttk_model.Forecaster; "falls below x within d" is GetHealthRatioIn(d) < x.

Before A359 the plugin did (StateUpdater: "Use tank mitigation when this many enemies target you", 2 by default, the
enemies within 3 yalms attacking him): the single-target defence opens and the tank presses, one per weave slot,
Arm's Length on a pack, his short mitigation, the big one when Rampart is ready or 60 s past, Rampart when the big one
is 30 s past (the two exclude each other), then Reprisal.

Rules compared:
  COUNT    that rule
  COUNTNS  COUNT without the 60 s / 30 s stagger - only the mutual exclusion
  ADD      COUNT, and the defence also opens with fewer enemies when his health would fall below 65 % within the short
           mitigation's duration
  HOLD     COUNT, but the big mitigation and Rampart only once his health would fall below the hold line within their
           own duration - the count still opens the short one, Arm's Length and Reprisal at once
  HOLD6    HOLD, but with at least six enemies on him the big one and Rampart go at once as before
  HOLDNS   HOLD without the stagger - the measured fall alone decides when the second big tool goes; built (A359)

Without a healer (the owner's question and note of 25.10.2026, A361): the party's healer has died in this fight, and
nobody heals until it ends - a dead player can return to the duty's start only once nobody is in combat (Return), so
each fight is its own case, a pull as much as a boss. What counts then is how long the tank stays alive: the time
left for a Phoenix Down (in combat since 7.3 in 4-player duties, not in 8-player trials and raids) or a raise from
whoever still can. Until A361 the plugin kept its rules: the pull hold waited for the measured fall, the tankbuster
plan held everything beyond its cheapest set for the next tankbuster, and a single enemy opened the defence only
for a tankbuster. Two fights, the healer dying at a random moment of it:
  a pull - one of the dungeon's big packs; the healer dies in the first 60 % of it; the tank alive at its end or not
  a boss - one enemy, auto-attacks of 1.0-2.0 % a second through the stance, a tankbuster of 35-60 % every 30-45 s
           from 20 s on, cast for 5 s; the healer dies 40-120 s in (the tankbusters before are known), or before the
           first one, or the boss has none; seconds the tank lives after the healer, up to 600
Rules there:
  TODAY    as before A361; at a tankbuster the plan presses the cheapest set of the ready tools that leaves him above
           15 % (the invulnerability line) and holds the rest while the cast runs
  PLAN     the plan's hold lifted: the rest goes on the tankbuster too, by the stagger
  OPEN     PLAN, and the defence opens while the enemy attacks him, by the stagger
  BUILT    PLAN, and the defence opens while the enemy attacks him; no hold and no stagger, but a tool goes only once
           its recast is over before the next tankbuster is due - the last one plus the shortest gap seen in this
           fight, the fight's start to the first included; none seen yet, none due (built, A361)
  KNOW     BUILT with the true time of the next tankbuster (what BossModReborn's prediction gives when a module runs)
In the pull, BUILT is FREE: no tankbuster is due, so neither the hold nor the stagger applies; STAG lifts the hold
and keeps the stagger.
The tank's own heals carry him then, modelled for this case only: 15 % every 25 s below 75 % (Bloodwhetting, Heart of
Corundum, Equilibrium and the like, lumped); the invulnerability at 15 %, 10 s every 300 s.

Measured over random dungeon runs (twenty pulls with walks between them; small packs of 2-3, wall-to-wall pulls of
6-12, single strong enemies): deaths, seconds below 30 %, seconds below 30 % in the first 5 s of a big pull, big
mitigations and Ramparts spent, and the healer's GCD heals (each a damage GCD he does not cast). Four cases: the base,
25 % more damage, a weaker healer (GCD heal 14 %), and big pulls whose first 4 s hit twice as hard. And a sweep of the
hold line for HOLDNS.

ASSUMPTIONS, not from a source - the conclusions are only as good as these: an enemy auto-attacks for 0.6-1.2 % of
the tank's health per second through his stance, a single strong one for 2-4 %; a pack dies between half and all of
12 + 2.5 s per enemy; walks of 8-20 s; one weave slot each 1.25 s. Mitigation values from the effect texts (concept
09): short 15 % for 8 s every 25 s, big 40 % for 15 s every 120 s, Rampart 20 % for 20 s every 90 s, Reprisal 10 % for
15 s every 60 s, Arm's Length's Slow 20 % for 15 s every 120 s, all on whatever is on him at the press. Tank self-heals,
barriers and the mid cooldowns (60-90 s, such as Camouflage, Dark Mind, Bulwark) are not modelled.

Usage: python3 .github/scripts/audit/tank_pull_defense_model.py [runs]
"""
import itertools
import math
import random
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from party_ttk_model import Forecaster  # noqa: E402

DT = 0.1
GCD = 2.5
CAST = 1.5
T_SPELL = 0.65
T_ABILITY = 0.70
HEAL_GCD = 0.18
HEAL_OGCD = 0.25
OGCD_RECAST = 20.0
ANIMATION_LOCK = 0.6
WEAVE = 1.25
AUTO_DEFENSE_NUMBER = 2
HOLD_LINE = 0.30  # the shipped default of "Health the big mitigation waits for" (A359)

TOOLS = {
    # name: (reduction, duration, recast)
    "short": (0.15, 8.0, 25.0),
    "big": (0.40, 15.0, 120.0),
    "rampart": (0.20, 20.0, 90.0),
    "reprisal": (0.10, 15.0, 60.0),
    "arms": (0.20, 15.0, 120.0),
}
RULES = ("COUNT", "COUNTNS", "ADD", "HOLD", "HOLD6", "HOLDNS")
SELF_HEAL = (0.15, 25.0, 0.75)  # amount, recast, below this health - assumption, without a healer only
INVULNERABILITY = (10.0, 300.0, 0.15)  # duration, recast, drawn at this health ("HealthForDyingTanks")
BUSTER_CAST = 5.0
BOSS_RULES = ("TODAY", "PLAN", "OPEN", "BUILT", "KNOW")


def make_run(seed, pulls=20, scale=1.0, spike=False):
    """A list of pulls: (start, [(dps, death time)...], kind), and the end time."""
    rng = random.Random(seed)
    t = 5.0
    out = []
    for _ in range(pulls):
        roll = rng.random()
        if roll < 0.45:
            kind, size, low, high = "big", rng.randint(6, 12), 0.006, 0.012
        elif roll < 0.85:
            kind, size, low, high = "small", rng.randint(2, 3), 0.006, 0.012
        else:
            kind, size, low, high = "single", 1, 0.020, 0.040
        length = 12.0 + 2.5 * size
        enemies = [(scale * rng.uniform(low, high), t + rng.uniform(0.5, 1.0) * length) for _ in range(size)]
        if spike and kind == "big":
            enemies += [(dps, t + 4.0) for dps, _ in enemies]
        out.append((t, enemies, kind))
        t = max(d for _, d in enemies) + rng.uniform(8.0, 20.0)
    return out, t


class Tank:
    def __init__(self):
        self.ready = {k: 0.0 for k in TOOLS}
        self.until = {k: -1.0 for k in TOOLS}
        self.used_at = {k: -1e9 for k in TOOLS}
        self.spent = {k: 0 for k in TOOLS}
        self.next_weave = 0.0
        self.slowed = set()
        self.reprised = set()

    def active(self, k, t):
        return t < self.until[k]

    def press(self, k, t, on):
        self.ready[k] = t + TOOLS[k][2]
        self.until[k] = t + TOOLS[k][1]
        self.used_at[k] = t
        self.spent[k] += 1
        self.next_weave = t + WEAVE
        if k == "arms":
            self.slowed = set(on)
        if k == "reprisal":
            self.reprised = set(on)

    def factor(self, t, enemy):
        f = 1.0
        for k in ("short", "big", "rampart"):
            if self.active(k, t):
                f *= 1 - TOOLS[k][0]
        if self.active("reprisal", t) and enemy in self.reprised:
            f *= 1 - TOOLS["reprisal"][0]
        if self.active("arms", t) and enemy in self.slowed:
            f *= 1 - TOOLS["arms"][0]
        return f


def falls_within(fc, r, seconds, line):
    """GetHealthRatioIn(seconds) below line: the health at the measured, corrected rate; no rate, no fall."""
    if not (fc.ttk == fc.ttk) or fc.ttk <= 0:
        return False
    return r * max(0.0, 1.0 - seconds / fc.ttk) < line


def decide(rule, tank, fc, r, t, alive_ids, count, line=HOLD_LINE):
    """One weave slot of the single-target defence; returns the tool pressed or None."""
    if t < tank.next_weave:
        return None
    by_count = count >= AUTO_DEFENSE_NUMBER
    add = rule == "ADD" and falls_within(fc, r, TOOLS["short"][1], T_SPELL)
    if not (by_count or add):
        return None
    hold = rule.startswith("HOLD") and not (rule == "HOLD6" and count >= 6)
    stagger = not (rule.endswith("NS") or rule == "FREE")

    def ready(k):
        return t >= tank.ready[k] and not tank.active(k, t)

    def needed(k):
        return not hold or falls_within(fc, r, TOOLS[k][1], line)

    if count >= AUTO_DEFENSE_NUMBER and ready("arms"):
        return "arms"
    if ready("short"):
        return "short"
    if ready("big") and not tank.active("rampart", t) and needed("big") \
            and (not stagger or t >= tank.ready["rampart"] or t - tank.used_at["rampart"] > 60):
        return "big"
    if ready("rampart") and not tank.active("big", t) and needed("rampart") \
            and (not stagger or t - tank.used_at["big"] > 30):
        return "rampart"
    if alive_ids and ready("reprisal"):
        return "reprisal"
    return None


def simulate(rule, seed, scale=1.0, heal_gcd=HEAL_GCD, spike=False, line=HOLD_LINE):
    pulls, end = make_run(seed, scale=scale, spike=spike)
    fc = Forecaster("ANCHOR")
    tank = Tank()
    r = 1.0
    next_gcd = 0.0
    ogcd_ready = 0.0
    pending = []
    deaths = heals = 0
    below = below_open = 0.0
    starts_big = [s for s, _, k in pulls if k == "big"]
    current = 0
    for step in range(int(end / DT)):
        t = step * DT
        while current + 1 < len(pulls) and pulls[current + 1][0] <= t:
            current += 1
        start, group, _ = pulls[current]
        alive = [((current, j), dps) for j, (dps, d) in enumerate(group) if start <= t < d]
        for eid, dps in alive:
            r -= dps * tank.factor(t, eid) * DT
        for land, amount in [p for p in pending if p[0] <= t]:
            r = min(1.0, r + amount)
            pending.remove((land, amount))
        if r <= 0:
            deaths += 1
            r = 0.5
        if step % round(1 / DT) == 0:
            fc.sample(round(t), r)
        choice = decide(rule, tank, fc, r, t, [e for e, _ in alive], len(alive), line)
        if choice:
            tank.press(choice, t, [e for e, _ in alive])
        if t >= ogcd_ready and fc.forecast(r, ANIMATION_LOCK) < T_ABILITY:
            r = min(1.0, r + HEAL_OGCD)
            ogcd_ready = t + OGCD_RECAST
        if t >= next_gcd:
            if fc.forecast(r, GCD) < T_SPELL:
                pending.append((t + CAST, heal_gcd))
                heals += 1
            next_gcd = t + GCD
        if r < 0.3:
            below += DT
            if any(0 <= t - s < 5 for s in starts_big):
                below_open += DT
    return deaths, below, below_open, tank.spent["big"] + tank.spent["rampart"], heals


def single_pull(rule, seed, scale=1.0):
    """One big pack; the healer heals by the forecast until he dies in its first 60 %. Returns (the tank alive at the
    end, seconds he lived after the healer)."""
    rng = random.Random(seed)
    size = rng.randint(6, 12)
    length = 12.0 + 2.5 * size
    enemies = [(scale * rng.uniform(0.006, 0.012), rng.uniform(0.5, 1.0) * length) for _ in range(size)]
    end = max(d for _, d in enemies)
    died = rng.uniform(0.0, 0.6) * end
    fc = Forecaster("ANCHOR")
    tank = Tank()
    r = 1.0
    next_gcd = ogcd_ready = self_heal_ready = 0.0
    pending = []
    for step in range(int(end / DT) + 1):
        t = step * DT
        alive = [((0, j), dps) for j, (dps, d) in enumerate(enemies) if t < d]
        for eid, dps in alive:
            r -= dps * tank.factor(t, eid) * DT
        for land, amount in [p for p in pending if p[0] <= t]:
            if land <= died:
                r = min(1.0, r + amount)
            pending.remove((land, amount))
        if r <= 0:
            return False, t - died
        if step % round(1 / DT) == 0:
            fc.sample(round(t), r)
        healer = t < died
        after = {"TODAY": "HOLDNS", "STAG": "COUNT"}.get(rule, "FREE")
        choice = decide("HOLDNS" if healer else after, tank, fc, r, t, [e for e, _ in alive], len(alive))
        if choice:
            tank.press(choice, t, [e for e, _ in alive])
        if healer:
            if t >= ogcd_ready and fc.forecast(r, ANIMATION_LOCK) < T_ABILITY:
                r = min(1.0, r + HEAL_OGCD)
                ogcd_ready = t + OGCD_RECAST
            if t >= next_gcd:
                if fc.forecast(r, GCD) < T_SPELL:
                    pending.append((t + CAST, HEAL_GCD))
                next_gcd = t + GCD
        else:
            amount, recast, below = SELF_HEAL
            if alive and t >= self_heal_ready and r < below:
                r = min(1.0, r + amount)
                self_heal_ready = t + recast
    return True, end - died


def cheapest_set(tank, t, r, size, ready_tools):
    """The tankbuster plan: the ready tools with the least recast in sum that leave him above the invulnerability line;
    None when no set does."""
    best = None
    for n in range(len(ready_tools) + 1):
        for combo in itertools.combinations(ready_tools, n):
            f = 1.0
            for k in ("short", "big", "rampart", "reprisal"):
                if tank.active(k, t) or k in combo:
                    f *= 1 - TOOLS[k][0]
            if r - size * f > INVULNERABILITY[2]:
                cost = sum(TOOLS[k][2] for k in combo)
                if best is None or cost < best[0]:
                    best = (cost, list(combo))
    return None if best is None else best[1]


def boss_fight(rule, seed, scale=1.0, case="mid"):
    """Seconds the tank lives after the healer's death against one boss, up to 600. case: "mid" - the healer dies
    40-120 s into the fight, its tankbusters before known; "pull" - before the first one; "none" - no tankbusters."""
    rng = random.Random(seed)
    dps = scale * rng.uniform(0.010, 0.020)
    died = rng.uniform(40.0, 120.0) if case == "mid" else 0.0
    hits = []
    t = 20.0 - died
    while case != "none" and t < 600.0:
        hits.append((t, scale * rng.uniform(0.35, 0.60)))
        t += rng.uniform(30.0, 45.0)
    tank = Tank()
    r = 1.0
    self_heal_ready = invulnerable_ready = 0.0
    invulnerable_until = -1.0
    plan_for, plan = None, None
    order = ("short", "big", "rampart", "reprisal")
    for step in range(int(600.0 / DT)):
        t = step * DT
        f = 0.0 if t < invulnerable_until else tank.factor(t, "boss")
        r -= dps * f * DT
        for hit, size in hits:
            if hit <= t < hit + DT:
                r -= size * f
        if r <= 0:
            return t
        if r <= INVULNERABILITY[2] and t >= invulnerable_ready:
            invulnerable_until, invulnerable_ready = t + INVULNERABILITY[0], t + INVULNERABILITY[1]
        amount, recast, below = SELF_HEAL
        if t >= self_heal_ready and r < below:
            r = min(1.0, r + amount)
            self_heal_ready = t + recast
        if t < tank.next_weave or t < invulnerable_until:
            continue

        def ready(k):
            return t >= tank.ready[k] and not tank.active(k, t)

        def excluded(k):
            return (k == "big" and tank.active("rampart", t)) or (k == "rampart" and tank.active("big", t))

        def staggered(k):
            if k == "big":
                return t >= tank.ready["rampart"] or t - tank.used_at["rampart"] > 60
            return k != "rampart" or t - tank.used_at["big"] > 30

        stagger = rule in ("TODAY", "PLAN", "OPEN")
        choice = None
        cast = next(((hit, size) for hit, size in hits if 0.0 <= hit - t <= BUSTER_CAST), None)
        if cast is not None:
            hit, size = cast
            if plan_for != hit:
                plan_for, plan = hit, cheapest_set(tank, t, r, size, [k for k in order if ready(k)])
            for k in plan if plan is not None else order:
                if ready(k) and not excluded(k) and hit - t <= TOOLS[k][1] - GCD:
                    choice = k
                    break
            if choice is None and not (rule == "TODAY" and plan is not None):
                choice = next((k for k in order if ready(k) and not excluded(k) and (not stagger or staggered(k))), None)
        elif rule in ("OPEN", "BUILT", "KNOW"):
            if rule == "KNOW":
                due = next((hit for hit, _ in hits if hit > t), float("inf"))
            else:
                seen = [hit for hit, _ in hits if hit <= t]
                gaps = [b - a for a, b in zip([-died] + seen[:-1], seen)]
                due = seen[-1] + min(gaps) if seen else float("inf")
                due = float("inf") if seen and t - seen[-1] > 2 * min(gaps) else due
            for k in order:
                back = rule == "OPEN" or t + TOOLS[k][2] <= due - (TOOLS[k][1] - GCD)
                if ready(k) and not excluded(k) and back and (not stagger or staggered(k)):
                    choice = k
                    break
        if choice:
            tank.press(choice, t, ["boss"])
    return 600.0


def total(rule, runs, **case):
    out = [0, 0.0, 0.0, 0, 0]
    for seed in range(runs):
        for i, v in enumerate(simulate(rule, seed, **case)):
            out[i] += v
    return out


CASES = [
    ("base", {}),
    ("25 % more damage", {"scale": 1.25}),
    ("weaker healer (GCD heal 14 %)", {"heal_gcd": 0.14}),
    ("big pulls hit twice as hard for 4 s", {"spike": True}),
]

BOSS_CASES = [
    ("healer dies 40-120 s in", {"case": "mid"}),
    ("healer dies 40-120 s in, 25 % more damage", {"case": "mid", "scale": 1.25}),
    ("healer dies before the first tankbuster", {"case": "pull"}),
    ("a boss without tankbusters", {"case": "none"}),
]


def self_test():
    def pull(rule, ratios, count):
        tank = Tank()
        fc = Forecaster("ANCHOR")
        for s, ratio in enumerate(ratios):
            fc.sample(s, ratio)
        pressed = []
        for step in range(40):
            t = len(ratios) + step * WEAVE
            c = decide(rule, tank, fc, ratios[-1], t, list(range(count)), count)
            if c:
                tank.press(c, t, list(range(count)))
                pressed.append(c)
        return pressed
    steady = [1.0, 1.0, 1.0, 1.0, 1.0]
    falling = [1.0, 0.92, 0.84, 0.76, 0.68]
    assert {"big", "rampart"} & set(pull("COUNT", steady, 2)), "the old rule spends a big mitigation on two enemies"
    assert not {"big", "rampart"} & set(pull("HOLDNS", steady, 2)), "the hold keeps them while the health holds"
    assert "big" in pull("HOLDNS", falling, 2), "the hold releases the big one once the fall would cross the line"
    assert pull("COUNT", steady, 1) == [], "one enemy does not open the old rule"
    assert {"big", "rampart"} <= set(pull("HOLDNS", falling, 8)) or "big" in pull("HOLDNS", falling, 8), \
        "a falling tank in a big pull gets the big one"
    assert pull("ADD", falling, 1), "a measured fall opens the defence for one enemy under ADD"
    assert {"big", "rampart"} <= set(pull("FREE", steady, 2)), "without a healer a steady pull of two gets both at once"
    assert "arms" not in pull("FREE", steady, 1), "Arm's Length only on a pack"
    tank = Tank()
    assert cheapest_set(tank, 0.0, 1.0, 0.50, ["short", "big", "rampart", "reprisal"]) == [], \
        "a tankbuster survivable as it stands gets nothing from the plan"
    assert cheapest_set(tank, 0.0, 0.5, 0.50, ["short", "big", "rampart", "reprisal"]) == ["short", "rampart"], \
        "at half health a 50 % tankbuster takes the short one and Rampart (115 s of recast), not the big one (120 s)"
    assert cheapest_set(tank, 0.0, 0.2, 0.60, ["short", "rampart"]) is None, "no set survives: the invulnerability's case"
    assert boss_fight("BUILT", 1, case="none") > boss_fight("TODAY", 1, case="none"), \
        "without tankbusters the defence against the auto-attacks lengthens his life"
    print("self-test ok: the old rule spends a big mitigation on a steady pull of two and the hold none; a fall that "
          "would cross the line releases it; one enemy opens only ADD; without a healer FREE spends both, the plan takes "
          "the cheapest set, and against a boss without tankbusters BUILT outlives TODAY")


def main():
    self_test()
    runs = int(sys.argv[1]) if len(sys.argv) > 1 else 100
    header = f"  {'rule':8s} {'deaths':>6s} {'s<30%':>8s} {'open':>6s} {'big+rampart':>12s} {'GCD heals':>10s}"
    for name, case in CASES:
        print(f"\n{name}, {runs} runs of 20 pulls (sums)\n{header}")
        for rule in RULES:
            d, below, opening, spent, heals = total(rule, runs, **case)
            print(f"  {rule:8s} {d:6d} {below:8.1f} {opening:6.1f} {spent:12d} {heals:10d}")
    print(f"\nWithout a healer: one big pull, the healer dying in its first 60 %, {4 * runs} pulls")
    today = [single_pull("TODAY", seed) for seed in range(4 * runs)]
    built = [single_pull("BUILT", seed) for seed in range(4 * runs)]
    staggered = [single_pull("STAG", seed) for seed in range(4 * runs)]
    saved = sum(1 for a, b in zip(today, built) if not a[0] and b[0])
    lost = sum(1 for a, b in zip(today, built) if a[0] and not b[0])
    print(f"  tank dies: TODAY {sum(1 for a in today if not a[0])}, BUILT {sum(1 for b in built if not b[0])},"
          f" STAG (no hold, the stagger kept) {sum(1 for c in staggered if not c[0])}"
          f" - BUILT saves him in {saved} pulls TODAY loses, and loses him in {lost} TODAY saves")
    for name, case in BOSS_CASES:
        base = [boss_fight("TODAY", seed, **case) for seed in range(2 * runs)]
        print(f"\nWithout a healer, a boss - {name}, {2 * runs} fights: seconds he lives after the healer\n"
              f"  TODAY    {sum(base) / len(base):6.1f}")
        for rule in BOSS_RULES[1:]:
            lives = [boss_fight(rule, seed, **case) for seed in range(2 * runs)]
            diff = [a - b for a, b in zip(lives, base)]
            mean = sum(diff) / len(diff)
            se = math.sqrt(sum((x - mean) ** 2 for x in diff) / (len(diff) - 1) / len(diff))
            print(f"  {rule:8s} {sum(lives) / len(lives):6.1f}  {mean:+6.1f} against TODAY (standard error {se:.1f})")
    print(f"\nHOLDNS by the hold line, base, {runs} runs\n{header}")
    for line in (0.15, 0.30, 0.45, 0.65):
        d, below, opening, spent, heals = total("HOLDNS", runs, line=line)
        print(f"  {line:<8.2f} {d:6d} {below:8.1f} {opening:6.1f} {spent:12d} {heals:10d}")


if __name__ == "__main__":
    main()
