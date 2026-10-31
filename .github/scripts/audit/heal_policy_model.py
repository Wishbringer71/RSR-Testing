#!/usr/bin/env python3
"""When a healer heals: fixed thresholds, or what the fight shows (concept 18, "Heilung"; concept 07; A470).

Not a code scanner - a model calculator, in a closed loop. The owner's question of 31.10.2026: can the healing
thresholds - when healing starts, how far apart the members may be for an area heal, when an ability and when a spell
heals - give way to what RSR measures in the fight, so that little is overhealed, nobody comes into real danger, and
everybody stays at or just under full health, where natural regeneration also works.

Measured in his logs, not assumed (ACT, 25. and 26.10.2026, White Mage, 8- and 24-player content):
  natural regeneration in combat  1.0 % of a member's maximum HP per 3-s tick (line 39, pairs 3 s apart with nothing
                                  else on the member between; median of 166, 29, 611 and 72 pairs; 10 % out of combat)
  overheal of his heals           Afflatus Solace and Tetragrammaton about 8 %, Cure II 18 %, Medica III 0-35 %,
                                  Medica II 48 %; Afflatus Rapture 62-67 % and Assize 84-97 % - Rapture spent at the
                                  lily cap, Assize a damage ability on its recast: both fall whatever the party needs

ASSUMPTIONS, not from a source: one GCD of 2.5 s, a heal spell lands 1.8 s after it starts; amounts in units of a
damage dealer's maximum HP - a spell heal 0.30 (Cure II; his heals of 800 potency, Afflatus Solace and Cure II, gave a
median of 31 % of the target's maximum HP on 25.10.2026, 49 heals), an area spell 0.17 to everyone, an instant heal 0.28 every
60 s and one of 0.18 on two charges of 30 s, an instant area heal 0.15 every 60 s, a lily every 20 s up to three,
spent on an instant spell of 0.30 or 0.15 to everyone; a tank holds 1.55 of that, a healer 0.95; no critical heals;
the amounts known from the first cast (in the plugin the smallest full heal is measured per area, from the first
landing on). Each fight's damage is drawn at random per seed (scenarios below); a member who dies stays dead. A
tankbuster comes at the size it has after the tank's own mitigation, which the model does not play; the healer's help
ahead of it (TankbusterHelp: a barrier or heal when the hit would leave the tank under the single spell threshold) is
modelled as an instant single heal, else a spell, for every rule alike.

Rules compared (each healer of the party follows the same one; the co-healer of a full party follows THRESH in the
second set, since other players' behaviour is never a carrying reason):
  THRESH   the plugin today at its defaults: the four flags at 70/65 % (single) and 75/65 % (area), each on the
           health forecast for the heal's landing (party_ttk_model.Forecaster, ANCHOR); the area flag on the lowest
           four when more than four members, average under the threshold and spread under 25 %, or three members under
           the single threshold; heal target the lowest forecast under the heal's own ratio of 80 %; the heal ahead of
           an announced area hit when it would leave a member who is not full at or under 15 %, with one share for the
           whole party as the plugin reads it; a lily at the cap goes on the area spell first, as the White Mage does
  THRESHHI THRESH with every threshold 10 points higher; THRESHHI2 20 points higher
  AREAFULL THRESH, but the spread for the area flag replaced by the measured amounts: the area heal where it heals more
           of what is missing than the single heal would on the member who lacks most
  CAPLILY  THRESH, but the lily at the cap goes where it heals most: the single spell on the member who lacks most
           when that heals more than the area spell over everyone in reach - a choice by measured amount and deficit
  CAPALL   CAPLILY, and every instant single heal whose charges are all ready goes on the member who lacks most where
           it lands fully - a free heal by deficit and measured amount, the threshold not asked
  NEARFREE CAPALL for any ready instant, and lilies wherever they land fully - the free means topping up, spells by
           THRESH
  NEAR     the owner's picture taken literally: a heal goes wherever it lands fully - the instant ones first, then the
           spells -, on top of THRESH
  LEARN    THRESH, but the single start learned in the fight: the largest hit each member has taken so far plus the
           critical line of 15 %, the defaults until his first hit
  DANGER   heals only where the forecast falls under 30 % or the announced hit would leave a member under it; the lily
           at the cap as THRESH

Measured per rule over the random fights of each scenario: deaths per fight, member-seconds under 30 %, heal spells
per fight (each one a damage GCD lost; a lily spell is not, Afflatus Misery pays it back), overheal as a share of all
healing, and natural regeneration used as a share of what full health had cost.

Result (31.10.2026, 300 fights per scenario for the close comparisons; concept 18, "Heilung"): no rule beats THRESH in
every scenario at the same price. NEAR is safer (light party with unannounced hits 0 instead of 2.7 deaths per 100
fights, raid 3.0 instead of 5.7) and behaves almost exactly like THRESHHI2 - same deaths and time under 30 %, slightly
fewer spells and 0-7 points less overheal. LEARN is safer in the raid and more dangerous in the light party; DANGER
loses members everywhere; NEARFREE lies between THRESH and THRESHHI; AREAFULL, CAPLILY and CAPALL stay within noise.

Usage: python3 .github/scripts/audit/heal_policy_model.py [--self-test]
"""
import random
import sys

DT = 0.5
GCD = 2.5
CAST = 1.8
ANIM = 0.7
REGEN = 0.01
REGEN_TICK = 3.0
HISTORY = 240
SINCE_START = 2.5
SINCE_WINDOW = 5

SPELL = 0.30
AREA_SPELL = 0.17
INSTANT_1 = (0.28, 60.0, 1)
INSTANT_2 = (0.18, 30.0, 2)
AREA_INSTANT = (0.15, 60.0, 1)
LILY_SINGLE = 0.30
LILY_AREA = 0.15
LILY_EVERY = 20.0
LILY_CAP = 3

T_SINGLE_ABILITY = 0.70
T_SINGLE_SPELL = 0.65
T_AREA_ABILITY = 0.75
T_AREA_SPELL = 0.65
SPREAD = 0.25
AUTO_HEAL_RATIO = 0.80
LIGHT_PARTY = 4
DANGER_LINE = 0.30
CRITICAL = 0.15

TANK_MAX = 1.55
HEALER_MAX = 0.95

RULES = ["THRESH", "THRESHHI", "THRESHHI2", "AREAFULL", "CAPLILY", "CAPALL", "NEARFREE", "NEAR", "LEARN", "DANGER"]


class Forecaster:
    """ComputeTTK's anchored fall (ANCHOR in party_ttk_model.py) and ScoreTtkForecast's correction, kept incrementally:
    within one fight of at most HISTORY seconds no sample leaves the window, so the anchor only moves forward."""

    def __init__(self):
        self.anchor = None
        self.recent = []
        self.bias = 1.0
        self.last = None
        self.ttk = float("nan")

    def sample(self, t, ratio):
        if ratio != 1.0:
            if self.anchor is None or ratio >= self.anchor[1]:
                self.anchor = (t, ratio)
            self.recent.append(ratio)
            self.recent = self.recent[-SINCE_WINDOW:]
        rate = float("nan")
        if self.anchor is not None and t - self.anchor[0] >= SINCE_START and self.recent:
            diff = self.anchor[1] - sum(self.recent) / len(self.recent)
            if diff > 0:
                rate = diff / (t - self.anchor[0])
        ttk = ratio / rate if rate == rate and rate > 0 else float("nan")
        if self.last is not None:
            l_ratio, l_ttk = self.last
            fall = l_ratio - ratio
            if l_ttk == l_ttk and l_ttk > 0 and fall > 0:
                expected = l_ratio / l_ttk
                self.bias = min(4.0, max(0.25, (self.bias * 3 + fall / expected) / 4))
        self.last = (ratio, ttk)
        self.ttk = ttk / self.bias if ttk == ttk else ttk

    def forecast(self, ratio, lead):
        if not (self.ttk == self.ttk) or self.ttk <= 0:
            return ratio
        return ratio * max(0.0, 1.0 - lead / self.ttk)


class Member:
    def __init__(self, role):
        self.role = role
        self.max = TANK_MAX if role == "T" else HEALER_MAX if role == "H" else 1.0
        self.hp = 1.0
        self.alive = True
        self.f = Forecaster()
        self.largest = 0.0

    def deficit(self):
        return (1.0 - self.hp) * self.max if self.alive else 0.0


class Stats:
    def __init__(self):
        self.deaths = 0
        self.below = 0.0
        self.spells = 0
        self.gross = 0.0
        self.over = 0.0
        self.regen_used = 0.0
        self.regen_possible = 0.0
        self.dead_roles = {}


def apply_heal(stats, member, amount):
    if not member.alive:
        return
    need = member.deficit()
    stats.gross += amount
    stats.over += max(0.0, amount - need)
    member.hp = min(1.0, member.hp + amount / member.max)


class Healer:
    def __init__(self, me, rule):
        self.me = me
        self.rule = rule
        self.next_gcd = 0.0
        self.ogcd_free = 0.0
        self.charges = {"i1": INSTANT_1[2], "i2": INSTANT_2[2], "ia": AREA_INSTANT[2]}
        self.recharge = {"i1": None, "i2": None, "ia": None}
        self.lily = 0
        self.lily_at = LILY_EVERY

    def tick_resources(self, t):
        for key, (_, recast, cap) in (("i1", INSTANT_1), ("i2", INSTANT_2), ("ia", AREA_INSTANT)):
            if self.recharge[key] is not None and t >= self.recharge[key]:
                self.charges[key] += 1
                self.recharge[key] = t + recast if self.charges[key] < cap else None
        if t >= self.lily_at:
            self.lily = min(LILY_CAP, self.lily + 1)
            self.lily_at += LILY_EVERY

    def spend(self, key, t):
        recast, cap = {"i1": INSTANT_1, "i2": INSTANT_2, "ia": AREA_INSTANT}[key][1:]
        if self.charges[key] == cap:
            self.recharge[key] = t + recast
        self.charges[key] -= 1


def flags(party, healer, t, announced):
    """The four flags of StateUpdater and the heal ahead of an announced hit, at the defaults (THRESHHI: each 10 points
    higher)."""
    up = {"THRESHHI": 0.10, "THRESHHI2": 0.20}.get(healer.rule, 0.0)
    lead_spell = max(0.0, healer.next_gcd - t) + CAST
    living = [m for m in party if m.alive]
    fi = [m.f.forecast(m.hp, ANIM) for m in living]
    fs = [m.f.forecast(m.hp, lead_spell) for m in living]
    if healer.rule == "LEARN":
        # The single start learned in the fight: the largest hit each member has taken so far, plus the critical line
        # of 15 %; the defaults until his first hit.
        def line(m, normal, extra):
            return normal if m.largest == 0 else min(0.95, m.largest / m.max + CRITICAL + extra)
        sa = sum(1 for m, x in zip(living, fi) if x < line(m, T_SINGLE_ABILITY, 0.05))
        ss = sum(1 for m, x in zip(living, fs) if x < line(m, T_SINGLE_SPELL, 0.0))
    else:
        sa = sum(1 for x in fi if x < T_SINGLE_ABILITY + up)
        ss = sum(1 for x in fs if x < T_SINGLE_SPELL + up)

    def area(values, threshold, amount, single):
        if len(party) <= 2 or not values:
            return False
        group = sorted(values)[:LIGHT_PARTY] if len(party) > LIGHT_PARTY else values
        mean = sum(group) / len(group)
        if healer.rule == "AREAFULL":
            # The spread replaced by what the fight shows: the area heal where it heals more of what is missing than
            # the single heal would on the member who lacks most.
            most = max((m.deficit() for m in living), default=0.0)
            return mean < threshold and area_effective(party, amount) > min(single, most)
        spread = (sum((x - mean) ** 2 for x in group) / len(group)) ** 0.5
        return spread < SPREAD and mean < threshold

    # DataCenter.AnnouncedHitCallsForHealFirst: the hit would leave a member who is not at full health at or under
    # HealthForDyingTanks (15 %), with one share for the whole party - a cast does not tell whom it reaches.
    ahead = False
    if announced is not None:
        ahead = any(m.hp < 1.0 and m.hp - announced / m.max <= CRITICAL for m in living)
    aa = (sa > 2 and healer.rule != "AREAFULL") or ahead or area(fi, T_AREA_ABILITY + up, AREA_INSTANT[0], INSTANT_1[0])
    as_ = (ss > 2 and healer.rule != "AREAFULL") or ahead or area(fs, T_AREA_SPELL + up, AREA_SPELL, SPELL)
    return sa > 0, ss > 0, aa, as_, ahead


def heal_target(party, lead, ratio=AUTO_HEAL_RATIO):
    best = None
    for m in party:
        if not m.alive:
            continue
        fc = m.f.forecast(m.hp, lead)
        if fc >= ratio:
            continue
        # The critical class first, then the lowest forecast; at the same health healer before tank before the rest.
        key = (fc > CRITICAL, round(fc, 2), m.role != "H", m.role != "T")
        if best is None or key < best[0]:
            best = (key, m)
    return best[1] if best else None


def most_lacking(party, amount=0.0):
    living = [m for m in party if m.alive and m.deficit() >= amount]
    return max(living, key=lambda m: m.deficit()) if living else None


def area_effective(party, amount):
    return sum(min(amount, m.deficit()) for m in party if m.alive)


def act(party, healer, t, announced, buster, pending, stats):
    rule = healer.rule
    if rule != "DANGER":
        sa, ss, aa, as_, ahead = flags(party, healer, t, announced)
    else:
        lead_spell = max(0.0, healer.next_gcd - t) + CAST
        living = [m for m in party if m.alive]
        sa = any(m.f.forecast(m.hp, ANIM) < DANGER_LINE for m in living)
        ss = any(m.f.forecast(m.hp, lead_spell) < DANGER_LINE for m in living)
        ahead = announced is not None and any(m.hp * m.max - announced < DANGER_LINE * m.max for m in living)
        under = sum(1 for m in living if m.f.forecast(m.hp, lead_spell) < DANGER_LINE)
        aa = as_ = ahead or under > 2
    # TankbusterHelp: an announced tankbuster that would leave the tank under the single spell threshold draws help
    # from the healer - a barrier or a heal on the tank, modelled as the instant single heal, else the spell. Every rule
    # alike, DANGER included: it is a defence layer of the plugin, not a healing threshold.
    help_tank = None
    if buster is not None:
        tank, amount = buster
        if tank.alive and tank.hp - amount / tank.max < T_SINGLE_SPELL:
            help_tank = tank
    # Instants: one per animation lock, never while a spell is being cast.
    if t >= healer.ogcd_free:
        done = False
        if help_tank is not None:
            for key, spec in (("i1", INSTANT_1), ("i2", INSTANT_2)):
                if healer.charges[key] > 0:
                    healer.spend(key, t)
                    apply_heal(stats, help_tank, spec[0])
                    done = True
                    help_tank = None
                    break
        if done:
            pass
        elif aa and healer.charges["ia"] > 0:
            healer.spend("ia", t)
            for m in party:
                apply_heal(stats, m, AREA_INSTANT[0])
            done = True
        elif sa:
            for key, spec in (("i1", INSTANT_1), ("i2", INSTANT_2)):
                if healer.charges[key] > 0:
                    target = heal_target(party, ANIM, DANGER_LINE if rule == "DANGER" else AUTO_HEAL_RATIO)
                    if target is not None:
                        healer.spend(key, t)
                        apply_heal(stats, target, spec[0])
                        done = True
                    break
        if not done and rule in ("CAPALL", "NEARFREE", "NEAR"):
            for key, spec in (("i1", INSTANT_1), ("i2", INSTANT_2)):
                full = healer.charges[key] == spec[2]
                if healer.charges[key] > 0 and (full or rule in ("NEARFREE", "NEAR")):
                    target = most_lacking(party, spec[0])
                    if target is not None:
                        healer.spend(key, t)
                        apply_heal(stats, target, spec[0])
                        done = True
                        break
            if not done and rule in ("NEARFREE", "NEAR") and healer.charges["ia"] > 0 \
                    and area_effective(party, AREA_INSTANT[0]) >= INSTANT_2[0]:
                healer.spend("ia", t)
                for m in party:
                    apply_heal(stats, m, AREA_INSTANT[0])
                done = True
        if done:
            healer.ogcd_free = t + ANIM
    if t < healer.next_gcd:
        return
    healer.next_gcd = t + GCD
    lily_capped = healer.lily == LILY_CAP or (healer.lily == LILY_CAP - 1 and healer.lily_at - t <= 3.0)
    lead_spell = CAST

    def lily(single_target=None, area=False):
        healer.lily -= 1
        if area:
            for m in party:
                apply_heal(stats, m, LILY_AREA)
        else:
            apply_heal(stats, single_target, LILY_SINGLE)

    def spell(target=None, area=False):
        stats.spells += 1
        healer.ogcd_free = t + CAST
        pending.append((t + CAST, None if area else target, AREA_SPELL if area else SPELL))

    if help_tank is not None and help_tank.hp < 1.0:
        if healer.lily > 0:
            lily(help_tank)
        else:
            spell(help_tank)
        return
    if as_:
        if healer.lily > 0:
            lily(area=True)
        else:
            spell(area=True)
        return
    if ss:
        target = heal_target(party, lead_spell, DANGER_LINE if rule == "DANGER" else AUTO_HEAL_RATIO)
        if target is not None:
            if healer.lily > 0:
                lily(target)
            else:
                spell(target)
            return
    if rule == "NEARFREE" and healer.lily > 0:
        target = most_lacking(party, LILY_SINGLE)
        if area_effective(party, LILY_AREA) >= LILY_SINGLE and area_effective(party, LILY_AREA) > (target.deficit() if target else 0):
            lily(area=True)
            return
        if target is not None:
            lily(target)
            return
    if rule == "NEAR":
        target = most_lacking(party, SPELL)
        if area_effective(party, AREA_SPELL) >= SPELL and area_effective(party, AREA_SPELL) > (target.deficit() if target else 0):
            if healer.lily > 0:
                lily(area=True)
            else:
                spell(area=True)
            return
        if target is not None:
            if healer.lily > 0:
                lily(target)
            else:
                spell(target)
            return
    if lily_capped and healer.lily > 0:
        if rule in ("CAPLILY", "CAPALL", "NEARFREE", "NEAR"):
            target = most_lacking(party)
            if target is not None and min(LILY_SINGLE, target.deficit()) > area_effective(party, LILY_AREA):
                lily(target)
                return
        lily(area=True)
        return
    # A damage GCD.


def scenario_hits(kind, rng, seconds, party):
    """Damage of one fight: a rate per member and time, and a list of (time, cast start or None when unannounced,
    targets, amount)."""
    tank = [i for i, m in enumerate(party) if m.role == "T"]
    others = [i for i, m in enumerate(party) if m.role != "T"]
    events = []
    auto = rng.uniform(0.02, 0.045) if kind == "dungeon" else rng.uniform(0.01, 0.02)
    if kind == "dungeon":
        # Pulls of 45-60 s with a short walk, enemies' cleaves and puddles on the others, unannounced.
        t = rng.uniform(5, 12)
        while t < seconds:
            events.append((t, None, [rng.choice(others)], rng.uniform(0.08, 0.20)))
            t += rng.uniform(8, 15)
        quiet = []
        t = 0.0
        while t < seconds:
            length = rng.uniform(45, 60)
            quiet.append((t + length, t + length + rng.uniform(8, 15)))
            t = quiet[-1][1]

        def rate(i, now):
            if i != tank[0]:
                return 0.0
            return 0.0 if any(a <= now < b for a, b in quiet) else auto
        return rate, events
    raid = kind == "raid"
    wide = (0.45, 0.70) if raid else (0.30, 0.55)
    t = rng.uniform(10, 20)
    while t < seconds:
        events.append((t, t - 4.0, list(range(len(party))), rng.uniform(*wide)))
        if kind not in ("boss", "partial"):
            for k in range(5):
                events.append((t + 3.0 * (k + 1), None, list(range(len(party))), 0.015))
        t += rng.uniform(20, 35) if raid else rng.uniform(25, 40)
    t = rng.uniform(20, 30)
    while t < seconds:
        events.append((t, t - 4.0, [tank[0]], rng.uniform(0.30, 0.45) * TANK_MAX))
        t += rng.uniform(30, 45)
    if kind == "partial":
        # Hits on part of the party - stacks, spreads, cleaves on a half -, announced like the raidwides.
        t = rng.uniform(12, 18)
        while t < seconds:
            size = rng.randint(2, len(party) - 2)
            events.append((t, t - 4.0, rng.sample(range(len(party)), size), rng.uniform(0.30, 0.60)))
            t += rng.uniform(10, 18)
    if kind not in ("boss", "partial"):
        t = rng.uniform(8, 15)
        while t < seconds:
            events.append((t, None, [rng.randrange(len(party))], rng.uniform(0.15, 0.40 if not raid else 0.35)))
            t += rng.uniform(12, 25) if not raid else rng.uniform(15, 30)

    def rate(i, now):
        return auto if i == tank[0] else 0.0
    return rate, events


SCENARIOS = [
    ("dungeon: pulls, cleaves and puddles without a cast bar, one healer", "dungeon", "TDHD"),
    ("boss, light party: announced raidwides and tankbusters only", "boss", "TDHD"),
    ("boss, light party: the same, unannounced hits and a bleed after each raidwide", "mixed", "TDHD"),
    ("raid, full party: harder raidwides, unannounced hits, bleed; two healers", "raid", "TTHHDDDD"),
    ("full party: raidwides, and hits on two to six members every 10-18 s; two healers", "partial", "TTHHDDDD"),
]


def fight(kind, roles, rule, seed, co_rule=None, seconds=180.0):
    rng = random.Random(seed)
    party = [Member(r) for r in roles]
    healers = []
    for i, m in enumerate(party):
        if m.role == "H":
            healers.append(Healer(i, rule if not healers or co_rule is None else co_rule))
    rate, events = scenario_hits(kind, rng, seconds, party)
    events.sort()
    stats = Stats()
    pending = []
    next_regen = REGEN_TICK
    for step in range(int(seconds / DT)):
        t = step * DT
        for i, m in enumerate(party):
            if m.alive:
                m.hp -= rate(i, t) * DT / m.max
        while events and events[0][0] <= t:
            _, _, targets, amount = events.pop(0)
            for i in targets:
                if party[i].alive:
                    party[i].hp -= amount / party[i].max
                    party[i].largest = max(party[i].largest, amount)
        for p in [p for p in pending if p[0] <= t]:
            pending.remove(p)
            if p[1] is None:
                for m in party:
                    apply_heal(stats, m, p[2])
            else:
                apply_heal(stats, p[1], p[2])
        for m in party:
            if m.alive and m.hp <= 0:
                m.alive = False
                m.hp = 0.0
                stats.deaths += 1
                stats.dead_roles[m.role] = stats.dead_roles.get(m.role, 0) + 1
        if t >= next_regen:
            next_regen += REGEN_TICK
            for m in party:
                if m.alive:
                    stats.regen_possible += REGEN * m.max
                    gain = min(REGEN, 1.0 - m.hp)
                    stats.regen_used += gain * m.max
                    m.hp += gain
        if step % round(1 / DT) == 0:
            for m in party:
                if m.alive:
                    m.f.sample(t, m.hp)
        announced = buster = None
        for e in events:
            if e[1] is not None and e[1] <= t < e[0]:
                if len(e[2]) > 1 and announced is None:
                    announced = e[3]
                elif len(e[2]) == 1 and party[e[2][0]].role == "T" and buster is None:
                    buster = (party[e[2][0]], e[3])
        for h in healers:
            if party[h.me].alive:
                h.tick_resources(t)
                act(party, h, t, announced, buster, pending, stats)
        for m in party:
            if m.alive and m.hp < 0.30:
                stats.below += DT
    return stats


def summary(kind, roles, runs, co_rule=None):
    out = {}
    for rule in RULES:
        total = [0, 0.0, 0, 0.0, 0.0, 0.0, 0.0]
        for seed in range(runs):
            s = fight(kind, roles, rule, seed, co_rule)
            total[0] += s.deaths
            total[1] += s.below
            total[2] += s.spells
            total[3] += s.gross
            total[4] += s.over
            total[5] += s.regen_used
            total[6] += s.regen_possible
        out[rule] = (total[0] / runs, total[1] / runs, total[2] / runs,
                     total[4] / total[3] if total[3] else 0.0, total[5] / total[6] if total[6] else 0.0)
    return out


def self_test():
    # The forecaster agrees with party_ttk_model's ANCHOR on falls, rises and hits, and stays blind on a rise.
    import party_ttk_model
    rng = random.Random(7)
    for _ in range(20):
        mine, theirs = Forecaster(), party_ttk_model.Forecaster("ANCHOR")
        r = 1.0
        for t in range(120):
            r = min(1.0, max(0.01, r - rng.uniform(-0.03, 0.05)))
            mine.sample(t, r)
            theirs.sample(t, r)
            assert (mine.ttk != mine.ttk and theirs.ttk != theirs.ttk) or abs(mine.ttk - theirs.ttk) < 1e-6, (t, mine.ttk, theirs.ttk)
    g = Forecaster()
    for t, r in enumerate([0.5, 0.6, 0.7, 0.8]):
        g.sample(t, r)
    assert g.ttk != g.ttk
    # Regeneration: an idle damaged member gains 1 % a tick and stops at full.
    party = [Member("D")]
    party[0].hp = 0.95
    stats = Stats()
    for _ in range(10):
        gain = min(REGEN, 1.0 - party[0].hp)
        party[0].hp += gain
        stats.regen_used += gain
    assert abs(party[0].hp - 1.0) < 1e-9 and abs(stats.regen_used - 0.05) < 1e-9
    # A constructed defect: a healer who never heals loses members where THRESH loses none.
    never = summary_one("boss", "TDHD", "NEVER", 10)
    thresh = summary_one("boss", "TDHD", "THRESH", 10)
    assert never[0] > thresh[0], (never, thresh)
    # Overheal: healing wherever a heal lands fully cannot overheal less than... it must not be zero-cost either:
    # NEAR spends more heal spells than THRESH in the mixed fights.
    near = summary_one("mixed", "TDHD", "NEAR", 10)
    thresh = summary_one("mixed", "TDHD", "THRESH", 10)
    assert near[2] > thresh[2], (near, thresh)
    print("self-test ok: forecaster as ANCHOR, regeneration 1 % a tick up to full, a healer who never heals loses "
          "members, healing wherever it lands fully costs more spells")


def summary_one(kind, roles, rule, runs):
    if rule == "NEVER":
        total = [0, 0.0, 0]
        for seed in range(runs):
            rng = random.Random(seed)
            party = [Member(r) for r in roles]
            rate, events = scenario_hits(kind, rng, 180.0, party)
            for t, _, targets, amount in sorted(events):
                for i in targets:
                    if party[i].alive:
                        party[i].hp -= amount / party[i].max
                        if party[i].hp <= 0:
                            party[i].alive = False
                            total[0] += 1
        return (total[0] / runs, 0.0, 0)
    total = [0, 0.0, 0]
    for seed in range(runs):
        s = fight(kind, roles, rule, seed)
        total[0] += s.deaths
        total[1] += s.below
        total[2] += s.spells
    return (total[0] / runs, total[1] / runs, total[2] / runs)


def main():
    self_test()
    if "--self-test" in sys.argv:
        return
    runs = 80
    for title, kind, roles in SCENARIOS:
        print(f"\n{title} ({runs} fights of 180 s)")
        print(f"  {'rule':8} {'deaths':>7} {'s<30%':>7} {'spells':>7} {'overheal':>9} {'regen used':>11}")
        for rule, (d, b, s, o, r) in summary(kind, roles, runs).items():
            print(f"  {rule:8} {d:7.2f} {b:7.1f} {s:7.1f} {o:9.1%} {r:11.1%}")
    print(f"\nraid, full party, the co-healer at THRESH whatever the rule ({runs} fights)")
    print(f"  {'rule':8} {'deaths':>7} {'s<30%':>7} {'spells':>7} {'overheal':>9} {'regen used':>11}")
    for rule, (d, b, s, o, r) in summary("raid", "TTHHDDDD", runs, co_rule="THRESH").items():
        print(f"  {rule:8} {d:7.2f} {b:7.1f} {s:7.1f} {o:9.1%} {r:11.1%}")


if __name__ == "__main__":
    main()
