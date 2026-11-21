#!/usr/bin/env python3
"""The Red Mage against The Balance: when the melee combo starts, against Embolden (concept 14, "Rotmagier: Mana vor
Embolden"; TODO "Rotmagier: Manaschwellen aus Kosten und Gewinn herleiten", A966).

Not a code scanner - a model calculator. The question: RedMageRotation.ManaNeededNoPooling starts the combo once both
gauges reach 60 (Embolden back in more than 80 s), 50 (40 to 80 s), 70 (15 to 40 s), 90 (the last 15 s), 50 under
Embolden - upstream's figures (5dd3dac18), read as the time remaining since A957. The Balance (Red Mage Basic Guide,
patch 7.4, read 20.11.2026): "Melee combo at 50|50+ mana"; "Always save enough mana for one melee combo under buffs";
the two-minute window "with 73|73 or above mana (starting 15 seconds before Embolden ...)"; "avoid overcapping mana".
Which rule puts more of the combo's potency under Embolden without losing mana to the cap?

Figures: effect texts (xivapi v2 ActionTransient and Action, read 21.11.2026): Jolt III 360, +2|+2; Verthunder III and
Veraero III 440, +6 to their colour, 50 % chance of Verfire or Verstone Ready; Verfire and Verstone 380, +5; Enchanted
Riposte 340 for 20|20, Enchanted Zwerchhau 380 (combo) for 15|15, Enchanted Redoublement 560 (combo) for 15|15;
Verflare and Verholy 650, +11 to their colour; Scorch 750, Resolution 850, each +4|+4; Embolden +10 % own magic damage
for 20 s, recast 120 s; Manafication 3 stacks of Magicked Swordplay (one combo without cost), recast 110 s.

Simplifications: one target that lives through the fight, no movement, no downtime; the 50 % procs as their expected
value (every second hardcast is Verfire or Verstone instead of Jolt III); colours alternate so the gauges stay within
the imbalance the game punishes; the oGCDs (Fleche, Contre Sixte, Vice of Thorns, Prefulgence) and Grand Impact left
out - their timing does not depend on when the combo starts; Manafication goes with Embolden (RDM_Reborn, within 5 s)
for every rule alike. The gauge cap is 100.

The self-test checks the mana arithmetic and the combo against hand-computed cases, and that a constructed defect - a
threshold above the cap, so the combo never starts - shows as mana lost to the cap.

Usage: python3 .github/scripts/audit/redmage_balance_model.py
"""
import sys

GCD_DEFAULT = 2.5
CAP = 100
COST = (20, 15, 15)
MELEE = (340, 380, 560)
FINISHER, SCORCH, RESOLUTION = 650, 750, 850
FINISH_GAIN, SCORCH_GAIN = 11, 4
JOLT, JOLT_GAIN = 360, 2
VER, VER_GAIN = 440, 6
PROC, PROC_GAIN = 380, 5
EMBOLDEN_TIME, EMBOLDEN_RECAST, EMBOLDEN_UP = 20.0, 120.0, 1.10
MANAFICATION_RECAST, MANAFICATION_AHEAD = 110.0, 5.0
SEQUENCE = 6                       # Riposte, Zwerchhau, Redoublement, Verflare/Verholy, Scorch, Resolution


def rsr(remaining, emboldened):
    if emboldened:
        return 50
    if remaining > 80:
        return 60
    if remaining > 40:
        return 50
    if remaining > 15:
        return 70
    return 90


def rsr_until_death(remaining, emboldened, left):
    """RDM_Reborn since A966: the ladder, but no hold for an Embolden that returns only after the target dies - then
    the combo goes at its cost (the time to death as CustomRotation.HoldCostsUse reads it)."""
    if not emboldened and remaining >= left:
        return sum(COST)
    return rsr(remaining, emboldened)


rsr_until_death.reads_left = True


def balance(remaining, emboldened):
    if emboldened:
        return sum(COST)
    return 73 if remaining <= 15 else sum(COST)


def derived(gcd):
    """The combo at its cost, except where it would end before Embolden starts and Embolden is near enough that
    waiting costs no mana: held while the gauges plus what the spells bring until Embolden stay within the cap."""
    def rule(remaining, emboldened, black=0, white=0):
        if emboldened:
            return sum(COST)
        gain = (remaining / gcd) * (JOLT_GAIN + VER_GAIN / 2 + PROC_GAIN / 4)
        if remaining <= SEQUENCE * gcd * 2 and max(black, white) + gain <= CAP:
            return CAP + 1
        return sum(COST)
    rule.reads_gauge = True
    return rule


def run(policy, fight=600.0, gcd=GCD_DEFAULT):
    t = 0.0
    black = white = 0
    lost = 0.0
    potency = 0.0
    emb_ready = 0.0
    emb_until = -1.0
    mana_ready = 0.0
    free = 0
    step = -1                     # -1: casting; 0..5: position in the melee sequence
    pair = 0                      # alternates Jolt/proc and the colour of the Verthunder/Veraero
    hardcast = True
    combos = combos_in = 0
    while t < fight:
        if emb_ready <= t:
            emb_until = t + EMBOLDEN_TIME
            emb_ready = t + EMBOLDEN_RECAST
        if mana_ready <= t and (emb_until > t or emb_ready - t <= MANAFICATION_AHEAD):
            free = len(COST)
            mana_ready = t + MANAFICATION_RECAST
        up = emb_until > t
        if step < 0 and hardcast:
            remaining = max(0.0, emb_ready - t)
            if getattr(policy, "reads_left", False):
                need = policy(remaining, up, fight - t)
            elif getattr(policy, "reads_gauge", False):
                need = policy(remaining, up, black, white)
            else:
                need = policy(remaining, up)
            if free == len(COST) or (black >= need and white >= need and need <= CAP):
                step = 0
        if step >= 0:
            if step < len(COST):
                if free > 0:
                    free -= 1
                else:
                    black -= COST[step]
                    white -= COST[step]
                value = MELEE[step]
                if step == 0:
                    combos += 1
                    combos_in += up
            elif step == 3:
                value = FINISHER
                if black <= white:
                    black, over = min(CAP, black + FINISH_GAIN), max(0, black + FINISH_GAIN - CAP)
                else:
                    white, over = min(CAP, white + FINISH_GAIN), max(0, white + FINISH_GAIN - CAP)
                lost += over
            else:
                value = SCORCH if step == 4 else RESOLUTION
                lost += max(0, black + SCORCH_GAIN - CAP) + max(0, white + SCORCH_GAIN - CAP)
                black, white = min(CAP, black + SCORCH_GAIN), min(CAP, white + SCORCH_GAIN)
            step = step + 1 if step < SEQUENCE - 1 else -1
            hardcast = True
        elif hardcast:
            if pair % 2:
                value = PROC
                gain_b = gain_w = 0
                if (pair // 2) % 2:
                    gain_b = PROC_GAIN
                else:
                    gain_w = PROC_GAIN
            else:
                value, gain_b, gain_w = JOLT, JOLT_GAIN, JOLT_GAIN
            lost += max(0, black + gain_b - CAP) + max(0, white + gain_w - CAP)
            black, white = min(CAP, black + gain_b), min(CAP, white + gain_w)
            hardcast = False
        else:
            value = VER
            if pair % 2:
                gain_b, gain_w = VER_GAIN, 0
            else:
                gain_b, gain_w = 0, VER_GAIN
            lost += max(0, black + gain_b - CAP) + max(0, white + gain_w - CAP)
            black, white = min(CAP, black + gain_b), min(CAP, white + gain_w)
            pair += 1
            hardcast = True
        potency += value * (EMBOLDEN_UP if up else 1.0)
        t += gcd
    return {"potency": round(potency), "combos": combos, "combos under Embolden": combos_in, "mana lost": round(lost)}


def self_test():
    # Mana arithmetic: one pair of Jolt III and Verthunder III brings 2|2 and 6 black.
    assert JOLT_GAIN * 2 + VER_GAIN == 10
    # The combo costs 50|50, as RSR's threshold below Embolden's level says.
    assert sum(COST) == 50 and balance(100.0, False) == 50 and rsr(60.0, False) == 50
    # The Balance holds 73 in the last 15 s; RSR 90.
    assert balance(10.0, False) == 73 and rsr(10.0, False) == 90
    s = run(rsr)
    assert s["combos"] > 0 and s["combos under Embolden"] > 0, s
    # Constructed defect: a threshold above the cap never starts a paid combo - mana must show as lost.
    broken = run(lambda remaining, up: CAP + 1)
    assert broken["mana lost"] > 0, broken
    print("self-test ok: mana per pair, the combo's cost, both threshold rules at their edges, a threshold above the cap"
          " shows as mana lost")


def grid():
    rows = {"rsr": [], "rsr_until_death": [], "balance": [], "derived": []}
    for gcd in (2.40, 2.45, 2.50):
        for minutes in range(3, 16):
            fight = minutes * 60.0
            base = run(rsr, fight, gcd)
            for name, policy in (("rsr", rsr), ("rsr_until_death", rsr_until_death), ("balance", balance),
                                 ("derived", derived(gcd))):
                r = run(policy, fight, gcd)
                rows[name].append((r["potency"] / base["potency"] - 1, r["mana lost"], r["combos under Embolden"]))
    return rows


if __name__ == "__main__":
    self_test()
    for name, policy in (("rsr", rsr), ("rsr_until_death", rsr_until_death), ("balance", balance),
                         ("derived", derived(GCD_DEFAULT))):
        print(f"  {name:16s} 10 min, GCD 2.50: {run(policy)}")
    for name, values in grid().items():
        gains = [v[0] for v in values]
        print(f"{name:16s} against rsr over {len(gains)} cases: mean {100 * sum(gains) / len(gains):+.3f} %, "
              f"{100 * min(gains):+.3f} % to {100 * max(gains):+.3f} %, lower in {sum(1 for g in gains if g < -1e-9)};"
              f" mana lost {min(v[1] for v in values)} to {max(v[1] for v in values)}")
    sys.exit(0)
