#!/usr/bin/env python3
"""The Machinist against The Balance: Automaton Queen timing and the Auto Crossbow breakpoint (concept 14,
"Vergleich mit The Balance", A342).

Not a code scanner - a model calculator. Two questions:

1. Queen: when does MCH_Reborn summon the Automaton Queen over ten minutes, single target, level 100, 2.50 GCD,
   and does it lose battery to the gauge's cap? Compared with The Balance's table (Machinist Basic Guide, "Patch
   Applicable: 7.3", last updated 5 Aug 2025, read 24.10.2026). The rotation's logic is mirrored by hand from
   RotationSolver/RebornRotations/Ranged/MCH_Reborn.cs (GeneralGCD, EmergencyAbility, UseQueen, ToolChargeSoon as
   of 262c292bd); a change there needs the same change here (TODO, technical debt).
2. Auto Crossbow: from how many enemies in its cone it out-damages Blazing Shot with the recast Blazing Shot takes
   off Double Check and Checkmate, for each number of enemies those two reach. The plugin computes the same
   comparison at runtime from the tooltips (MachinistRotation.AutoCrossbowOutdamagesBlazingShot); the figures here
   are the official job guide's (na.finalfantasyxiv.com/jobguide/machinist, patch 7.5, read 24.10.2026).
3. Flamethrower: from how many enemies in its cone it out-damages the filler it replaces, with and without the value of
   the Heat and Battery the filler brings - The Balance: "If gauge is not usable, beats filler on 2+" (A380). Job guide,
   patch 7.5 (read 26.10.2026): Flamethrower 120 potency to every enemy in its cone for 10 s, per second by the German
   text ("im Sekundentakt"); the Heated combo 220, 320, 420 with 5 Heat each and 10 Battery on the last; Scattergun 130
   to every enemy with 10 Heat. Heat is worth what Hypercharge makes of it: 50 Heat buy five Blazing Shots (or Auto
   Crossbow where it wins) in the time of three filler GCDs; Battery is worth 266 potency per 10 (The Balance).

Simplifications of the queen model: no movement, no downtime, no raid buffs, no potion; Reassemble spent in the
countdown; the Queen takes 16 s from summon to finish (The Balance: "roughly 16 seconds"), the effect text's 12 s
of action plus her start.

The self-test checks the arithmetic of both parts against hand-computed cases and The Balance's stated
breakpoints, and that a constructed defect - the summon that guards the cap switched off - shows as lost battery.

Usage: python3 .github/scripts/audit/machinist_balance_model.py [-t]
"""
import sys

GCD = 2.5
OH_GCD = 1.5
COMBO = 30.0
QUEEN_ACTIVE = 16.0  # The Balance: "roughly 16 seconds from start to finish"; effect text: 12 s of action


class S:
    pass


def new_state(excavator_policy="asap", queen_active=QUEEN_ACTIVE, cap_guard=True):
    s = S()
    s.cap_guard = cap_guard
    s.t = 0.0
    s.heat = 0
    s.battery = 0
    s.combo = 0            # 0 none, 1 after split, 2 after slug
    s.combo_until = -1.0
    s.drill_ch = 2
    s.drill_next = None    # time the next charge returns
    s.aa = 0.0
    s.cs = 0.0
    s.exc_until = -1.0
    s.fmm_until = -1.0     # Full Metal Machinist
    s.hyperd_until = -1.0  # Hypercharged
    s.oh_stacks = 0
    s.oh_until = -1.0
    s.hc_ready = 0.0
    s.wf_ready = 0.0
    s.wf_until = -1.0
    s.barrel = 0.0
    s.reas_ch = 1          # one spent in the countdown
    s.reas_next = 55.0 - 4.75
    s.reassembled = False
    s.cm_ch = 3
    s.cm_next = None
    s.dc_ch = 3
    s.dc_next = None
    s.queen_until = -1.0
    s.queen_ready = 0.0
    s.last_summon = 0
    s.step = 0
    s.was_active = False
    s.last_gcd = None
    s.last_action = None
    s.summons = []
    s.wasted = 0
    s.exc_policy = excavator_policy
    s.queen_active = queen_active
    return s


STEPS = [(0, 60), (60, 90), (90, 100), (100, 50), (50, 60), (60, 100), (100, 50), (50, 70), (70, 100), (100, 50),
         (50, 80), (80, 100), (100, 50), (50, 60)]


def tick_charges(s, t):
    for name, rec in (("drill", 20.0), ("cm", 30.0), ("dc", 30.0), ("reas", 55.0)):
        ch = getattr(s, name + "_ch")
        nxt = getattr(s, name + "_next")
        mx = 2 if name in ("drill", "reas") else 3
        while nxt is not None and nxt <= t + 1e-9:
            ch += 1
            nxt = nxt + rec if ch < mx else None
        setattr(s, name + "_ch", ch)
        setattr(s, name + "_next", nxt)


def spend(s, name, rec, t):
    ch = getattr(s, name + "_ch")
    mx = 2 if name in ("drill", "reas") else 3
    if ch == mx:
        setattr(s, name + "_next", t + rec)
    setattr(s, name + "_ch", ch - 1)


def overheated(s, t):
    return s.oh_stacks > 0 and s.oh_until > t


def add_battery(s, n):
    total = s.battery + n
    if total > 100:
        s.wasted += total - 100
    s.battery = min(100, total)


def choose_gcd(s, t):
    combo_left = s.combo_until - t if s.combo else 0.0
    oh = overheated(s, t)
    if s.combo == 2 and GCD <= combo_left <= 2 * GCD and not oh:
        return "clean"
    if s.combo == 1 and GCD <= combo_left <= 2 * GCD and not oh:
        return "slug"
    if oh:
        return "blazing"
    if s.aa <= t:
        return "aa"
    if s.drill_ch >= 2:
        return "drill"
    if s.exc_until > t and s.exc_policy == "asap":
        return "exc"
    if s.exc_until > t and s.exc_policy != "asap" and s.exc_policy(s, t):
        return "exc"
    if s.cs <= t:
        return "cs"
    if (not s.aa <= t and not s.cs <= t and not s.exc_until > t and s.last_gcd != "cs" and s.drill_ch < 2
            and s.fmm_until > t and (s.wf_ready <= t or s.last_action == "wf")):
        return "fmf"
    if s.drill_ch >= 1:
        return "drill"
    if s.fmm_until > t and s.fmm_until - t <= 3:
        return "fmf"
    if s.exc_until > t and s.exc_until - t <= 3:
        return "exc"
    if s.combo == 2 and combo_left > 0:
        return "clean"
    if s.combo == 1 and combo_left > 0:
        return "slug"
    return "split"


def do_gcd(s, t, g):
    if g == "clean":
        s.heat = min(100, s.heat + 5)
        add_battery(s, 10)
        s.combo = 0
    elif g == "slug":
        s.heat = min(100, s.heat + 5)
        s.combo = 2
        s.combo_until = t + COMBO
    elif g == "split":
        s.heat = min(100, s.heat + 5)
        s.combo = 1
        s.combo_until = t + COMBO
    elif g == "blazing":
        s.oh_stacks -= 1
        for n in ("cm", "dc"):
            nxt = getattr(s, n + "_next")
            if nxt is not None:
                setattr(s, n + "_next", nxt - 15.0)
        tick_charges(s, t)
    elif g == "aa":
        s.aa = t + 40.0
        add_battery(s, 20)
    elif g == "drill":
        spend(s, "drill", 20.0, t)
    elif g == "cs":
        s.cs = t + 60.0
        add_battery(s, 20)
        s.exc_until = t + 30.0
    elif g == "exc":
        s.exc_until = -1.0
        add_battery(s, 20)
    elif g == "fmf":
        s.fmm_until = -1.0
    if s.combo and s.combo_until <= t:
        s.combo = 0
    if s.wf_until > t and g:
        pass
    s.reassembled = False if g not in ("blazing",) or True else s.reassembled
    s.last_gcd = g
    s.last_action = g


def tool_charge_soon(s, t):
    rest = 8.0
    if s.aa <= t + rest:
        return True
    if s.drill_ch >= 2 or (s.drill_ch == 1 and s.drill_next is not None and s.drill_next <= t + rest):
        return True
    if s.cs <= t + rest:
        return True
    return False


def can_hypercharge(s, t):
    return s.hc_ready <= t and not overheated(s, t) and (s.heat >= 50 or s.hyperd_until > t)


def do_hypercharge(s, t):
    if s.hyperd_until > t:
        s.hyperd_until = -1.0
    else:
        s.heat -= 50
    s.hc_ready = t + 10.0
    s.oh_stacks = 5
    s.oh_until = t + 10.0
    s.last_action = "hc"


def summon(s, t):
    s.summons.append((round(t, 2), s.battery))
    s.last_summon = s.battery
    s.battery = 0
    s.queen_until = t + s.queen_active
    s.queen_ready = t + 6.0
    s.last_action = "queen"


def choose_ogcd(s, t, late, next_gcd, combat_time):
    # Robot step tracking (EmergencyAbility)
    active = s.queen_until > t
    if active and not s.was_active:
        s.step += 1
    s.was_active = active
    found = s.step < len(STEPS) and s.last_summon == STEPS[s.step][0] and s.battery == STEPS[s.step][1]
    # Emergency: Hypercharge after FMF under Wildfire
    if s.wf_until > t and s.last_action == "fmf" and can_hypercharge(s, t):
        do_hypercharge(s, t)
        return "hc"
    if s.fmm_until > t and s.last_action == "wf":
        return None
    # Reassemble
    if s.reas_ch > 0 and not s.reassembled and next_gcd in ("cs", "exc", "aa"):
        spend(s, "reas", 55.0, t)
        s.reassembled = True
        s.last_action = "reas"
        return "reas"
    if s.cm_ch == 3:
        spend(s, "cm", 30.0, t)
        s.last_action = "cm"
        return "cm"
    if s.dc_ch == 3:
        spend(s, "dc", 30.0, t)
        s.last_action = "dc"
        return "dc"
    if s.barrel <= t:
        s.barrel = t + 120.0
        s.hyperd_until = t + 30.0
        s.fmm_until = t + 30.0
        s.last_action = "barrel"
        return "barrel"
    if (s.heat >= 50 or s.hyperd_until > t) and late and next_gcd == "fmf" and s.wf_ready <= t:
        s.wf_ready = t + 120.0
        s.wf_until = t + 10.0
        s.last_action = "wf"
        return "wf"
    # UseQueen
    if not active and s.queen_ready <= t:
        if s.battery == 60 and s.last_gcd == "exc" and combat_time < 15:
            summon(s, t)
            return "queen"
        if found and s.battery >= 50:
            summon(s, t)
            return "queen"
        if s.cap_guard and ((next_gcd == "clean" and s.battery > 90) or (next_gcd in ("aa", "cs", "exc") and s.battery > 80)):
            summon(s, t)
            return "queen"
    # Hypercharge outside Wildfire
    combo_left = s.combo_until - t if s.combo else 0.0
    wf_in_30 = s.wf_ready <= t + 30.0
    if not s.reassembled and (not wf_in_30 or s.heat == 100) and not (0 < combo_left <= 9.0):
        if not tool_charge_soon(s, t) and can_hypercharge(s, t):
            do_hypercharge(s, t)
            return "hc"
    if next_gcd != "fmf":
        if s.cm_ch > 0:
            spend(s, "cm", 30.0, t)
            return "cm"
        if s.dc_ch > 0:
            spend(s, "dc", 30.0, t)
            return "dc"
    return None


def run(fight=600.0, excavator_policy="asap", queen_active=QUEEN_ACTIVE, trace=False, cap_guard=True):
    s = new_state(excavator_policy, queen_active, cap_guard)
    # countdown: Reassemble at -4.75 (one charge spent), Air Anchor at 0
    s.reassembled = True
    t = 0.0
    log = []
    while t < fight:
        tick_charges(s, t)
        g = choose_gcd(s, t)
        do_gcd(s, t, g)
        if g == "blazing":
            pass
        if s.wf_until > t:
            pass
        gl = OH_GCD if g == "blazing" else GCD
        nxt_t = t + gl
        slots = [t + 0.7] if gl < 2 else [t + 0.7, t + 1.4]
        for i, st in enumerate(slots):
            tick_charges(s, st)
            ng = choose_gcd(s, nxt_t)
            o = choose_ogcd(s, st, late=(i == len(slots) - 1), next_gcd=ng, combat_time=st)
            if trace:
                log.append((round(st, 2), "o", o, s.battery, s.heat))
        if trace:
            log.append((round(t, 2), "g", g, s.battery, s.heat))
        t = nxt_t
    return s, log


BALANCE = [(7.5, 60), (68.25, 90), (121.5, 100), (158.25, 50), (188.25, 60), (240.75, 100), (278.25, 50), (306.5, 70),
           (360.75, 100), (398.25, 50), (430.0, 80), (480.75, 100), (518.75, 50), (548.25, 60)]


# Auto Crossbow against Blazing Shot. Job guide, patch 7.5: Auto Crossbow 180 to every enemy in its cone; Blazing
# Shot 240, raised by 20 under Overheated ("single-target weaponskills"), takes 15 s off the recast of both Double
# Check and Checkmate; each of those hits 180, 30 % less for every further enemy around its target, recast 30 s.
CROSSBOW, SHOT, OVERHEAT, CUT = 180, 240, 20, 15
CUT_FIRST, CUT_FALLOFF, CUT_RECAST = 180, 0.30, 30.0


def blazing_value(reached):
    """Blazing Shot plus the share of a Double Check and a Checkmate its recast cut buys, both reaching `reached`."""
    per_cut = CUT_FIRST + CUT_FIRST * (1 - CUT_FALLOFF) * (max(1, reached) - 1)
    return SHOT + OVERHEAT + 2 * min(1.0, CUT / CUT_RECAST) * per_cut


def crossbow_wins(cone, reached):
    return CROSSBOW * cone > blazing_value(reached)


def breakpoint(outside):
    """The fewest enemies in the cone from which Auto Crossbow wins, with `outside` of them beyond the cut's reach."""
    for cone in range(1, 20):
        if crossbow_wins(cone, cone - outside):
            return cone
    return None


# Flamethrower against the filler (part 3). The Balance's filler: Heated combo on 1-2 enemies, Scattergun on 3+.
FLAME_TICK, FLAME_TICK_EVERY = 120, 1.0
COMBO_AVG, COMBO_HEAT, COMBO_BATTERY = (220 + 320 + 420) / 3, 5, 10 / 3
SCATTER, SCATTER_HEAT = 130, 10
BATTERY_VALUE = 266 / 10
HYPERCHARGE_HEAT, HYPERCHARGE_SHOTS, HYPERCHARGE_GCDS = 50, 5, 3


def filler_raw(n):
    return COMBO_AVG if n < 3 else SCATTER * n


def heat_value(n):
    """Potency one Heat adds: the Hypercharge window it buys over the filler GCDs that window displaces."""
    window = HYPERCHARGE_SHOTS * max(blazing_value(n), CROSSBOW * n)
    return (window - HYPERCHARGE_GCDS * filler_raw(n)) / HYPERCHARGE_HEAT


def filler_with_gauge(n):
    heat, battery = (COMBO_HEAT, COMBO_BATTERY) if n < 3 else (SCATTER_HEAT, 0)
    return filler_raw(n) + heat * heat_value(n) + battery * BATTERY_VALUE


def flamethrower(n):
    """Flamethrower's potency in the time of one GCD."""
    return FLAME_TICK * (GCD / FLAME_TICK_EVERY) * n


def flame_breakpoint(gauge_usable):
    for n in range(1, 20):
        if flamethrower(n) >= (filler_with_gauge(n) if gauge_usable else filler_raw(n)):
            return n
    return None


def self_test():
    # Hand-computed: three enemies, all reached: 540 against 260 + 2 * 0.5 * (180 + 2 * 126) = 692.
    assert abs(blazing_value(3) - 692) < 1e-9, blazing_value(3)
    assert not crossbow_wins(3, 3) and crossbow_wins(3, 1), "three enemies: Blazing Shot unless two stand outside"
    # The Balance: "Blazing Shot on 1-5, Auto Crossbow on 6+"; "If 1 target is out of range, Auto Crossbow is a gain
    # on 4+ targets".
    assert breakpoint(0) == 6, breakpoint(0)
    assert breakpoint(1) == 4, breakpoint(1)
    # Flamethrower: The Balance, "If gauge is not usable, beats filler on 2+"; on one enemy it loses either way. With the
    # gauge counted two enemies are even (600 against 596), three ahead.
    assert flame_breakpoint(gauge_usable=False) == 2, flame_breakpoint(False)
    assert flamethrower(1) < filler_raw(1) and flamethrower(1) < filler_with_gauge(1)
    assert abs(flamethrower(2) - 600) < 1e-9 and abs(filler_with_gauge(2) - 600) < 10, filler_with_gauge(2)
    assert flamethrower(3) > filler_with_gauge(3)
    # Queen: no battery lost with the cap guard, the opener and one-minute summons as The Balance has them.
    s, _ = run()
    assert s.wasted == 0, s.wasted
    assert [b for _, b in s.summons[:2]] == [60, 90], s.summons[:2]
    assert abs(s.summons[0][0] - BALANCE[0][0]) < GCD and abs(s.summons[1][0] - BALANCE[1][0]) < GCD, s.summons[:2]
    # Constructed defect: without the summon that guards the cap, battery is lost - a silent zero would not be.
    broken, _ = run(cap_guard=False)
    assert broken.wasted > 0, "the cap guard switched off must show as lost battery"
    # The Balance's even-minute Queens follow Air Anchor at 100 battery.
    assert all(b == 100 for t, b in BALANCE if 115 <= t % 120 or t % 120 <= 5), BALANCE
    print("self-test ok: breakpoints 6 and 4 as The Balance states them, Flamethrower from 2 without the gauge, no"
          " battery lost, the switched-off cap guard shows as lost battery")


if __name__ == "__main__":
    self_test()
    s, log = run(trace="-t" in sys.argv)
    print("RSR summons (time s, battery):")
    for x in s.summons:
        print("  ", x)
    print("battery lost to overcap:", s.wasted)
    print("The Balance:", BALANCE)
    print("Auto Crossbow from (enemies in the cone): all reached", breakpoint(0), "- one outside", breakpoint(1),
          "- two outside", breakpoint(2))
    print("Flamethrower per GCD against the filler (enemies: flamethrower / filler / filler with gauge):")
    for n in range(1, 7):
        print(f"   {n}: {flamethrower(n):.0f} / {filler_raw(n):.0f} / {filler_with_gauge(n):.0f}")
    if "-t" in sys.argv:
        for e in log:
            print(e)
