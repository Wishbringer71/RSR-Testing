#!/usr/bin/env python3
"""The Monk against The Balance: Perfect Balance, Masterful Blitz and the Replies in MNK_Reborn's single-target loop
(concept 14, "Vergleich mit The Balance", A412).

Not a code scanner - a model calculator. The question: The Balance (Monk Basic Guide and Openers page, "Patch
Applicable: 7.4", read 26.10.2026) builds every burst window from five rules - Riddle of Fire and Brotherhood on
cooldown, Riddle of Fire late-woven, Perfect Balance only after an opo-opo weaponskill, Formless Fist spent on opo-opo,
one Perfect Balance in odd and two in even windows. The even window starts with Perfect Balance "after an opo gcd in
the 3 to 1 gcd range before RoF (... about ~7.4ish to ~3ish seconds before RoF comes off cd)". MNK_Reborn presses it
there only while Brotherhood comes back within 3 s; it weaves Riddle of Fire in the first free slot, and builds the
Solar sequence raptor, coeurl, opo-opo regardless of potency. Does that cost a Masterful Blitz, a Reply or opo-opo
weaponskills under Riddle of Fire and Brotherhood - the Monk's own damage?

The choices of MNK_Reborn are mirrored by hand from RotationSolver/RebornRotations/Melee/MNK_Reborn.cs
(EmergencyAbility, AttackAbility, GeneralGCD) and the action checks of MonkRotation: RSR as the rotation stands since
A412, RSR_BEFORE as of c6ed70be4. A change there needs the same change here (TODO, technical debt). What the model found
(A412): MNK_Reborn pressed the even window's Perfect Balance only while Brotherhood came back within 3 s, and pressed
Brotherhood itself up to 3 s ahead of Riddle of Fire - when the opo-opo weaponskill fell after that, the window went
without its first Perfect Balance, the second came too late for Brotherhood, and a charge ran over. Figures: job guide, patch 7.5 (na.finalfantasyxiv.com/jobguide/
monk, read 26.10.2026) - weaponskill recast 2 s, Dragon Kick 320, Leaping Opo 260 / 460 with Opo-opo's Fury and a
guaranteed critical hit with the form bonus, Twin Snakes 420, Rising Raptor 340 / 540, Demolish 420 from the rear,
Pouncing Coeurl 370 / 520 from the flank, Elixir Burst and Rising Phoenix 900, Phantom Rush 1500, Celestial Revolution
600, Fire's Reply 1400, Wind's Reply 1040; Perfect Balance 40 s, 2 charges, 3 stacks for 20 s; Riddle of Fire 60 s,
+15 % for 20 s; Brotherhood 120 s, +5 % for 20 s; Riddle of Wind 90 s, Wind's Rumination 15 s; Formless Fist 30 s.

Simplifications: one target that lives through the fight, every positional hits, no movement, no downtime, the oGCD
attacks (The Forbidden Chakra, auto-attacks) left out - their timing does not depend on the GCD order modelled here.
Not from a source: the critical-hit factor of Leaping Opo against an average weaponskill (CRIT_LO; 1.0 and 1.39, the
latter for 25 % critical rate and +60 % critical damage) and the weave slots (WEAVE).

The self-test checks the opener against The Balance's image "Solar Lunar - DK Opener (5s Buffs)", the even window
against the Openers page's steps, and that a constructed defect - Perfect Balance never pressed - shows as charges lost.

Usage: python3 .github/scripts/audit/monk_balance_model.py [-t rsr|balance]   (-t traces the first 3 minutes)
"""
import math
import sys

WEAVE = (0.65, 1.3)               # the two weave slots after a weaponskill
ROF_RECAST, ROF_TIME, ROF_UP = 60.0, 20.0, 1.15
BH_RECAST, BH_TIME, BH_UP = 120.0, 20.0, 1.05
ROW_RECAST, WINDS_TIME = 90.0, 15.0
PB_RECAST, PB_CHARGES, PB_STACKS, PB_TIME = 40.0, 2, 3, 20.0
FORMLESS_TIME = 30.0
REPLY_ENDING = 5.0                # MNK_Reborn: PlayerWillStatusEnd(5, ...)
BLITZ_HOLD = 42.0                 # MNK_Reborn: RiddleOfFirePvE.Cooldown.JustUsedAfter(42)
ODD_PB_GAP = 20.0                 # MNK_Reborn: PerfectBalancePvE.Cooldown.JustUsedAfter(20)
EVEN_PB_ROF_GCDS, EVEN_PB_BH = 3, 3.0   # MNK_Reborn: WillHaveOneChargeGCD(3), WillHaveOneCharge(3)
BH_AHEAD = 3.0                    # MNK_Reborn: Brotherhood once RiddleOfFirePvE.Cooldown.WillHaveOneCharge(3)

POTENCY = {"dk": 320, "lo": 260, "lo_fury": 460, "ts": 420, "rr": 340, "rr_fury": 540, "dem": 420, "pc": 370,
           "pc_fury": 520, "eb": 900, "rp": 900, "pr": 1500, "cr": 600, "fr": 1400, "wr": 1040}
OPO, RAPTOR, COEURL = {"dk", "lo"}, {"ts", "rr"}, {"dem", "pc"}
BLITZ = {"eb", "rp", "pr", "cr"}

# MNK_Reborn as of c6ed70be4: the even window's Perfect Balance while Brotherhood comes back within 3 s.
RSR_BEFORE = {"name": "rsr_before", "pb_pre": "rsr", "late_rof": False, "solar_order": "rsr", "odd_solar_pre": False,
              "gcd_order": "rsr"}
# MNK_Reborn since A412: The Balance's identifier for the even window, the rest as before.
RSR = dict(RSR_BEFORE, name="rsr", pb_pre="balance")
BALANCE = {"name": "balance", "pb_pre": "balance", "late_rof": True, "solar_order": "balance",
           "odd_solar_pre": True, "gcd_order": "balance"}


class S:
    pass


def new_state():
    s = S()
    s.t = 0.0
    s.form = None
    s.formless_until = FORMLESS_TIME      # Form Shift before the pull
    s.opo_fury = s.raptor_fury = s.coeurl_fury = 0
    s.beast = []
    s.solar = s.lunar = False
    s.pb_charges = float(PB_CHARGES)
    s.pb_stacks = 0
    s.pb_until = -1.0
    s.pb_used = -1e9
    s.pb_wasted = 0.0
    s.rof_ready = s.bh_ready = s.row_ready = 0.0
    s.rof_used = s.bh_used = -1e9
    s.bh_uses = []
    s.rof_until = s.bh_until = s.fires_until = s.winds_until = -1.0
    s.last_gcd = None
    s.last_form_gcd = None
    s.last_ability = None
    s.window = 0                  # Riddle of Fire uses so far
    s.pb_in_window = {}           # window index -> Perfect Balance uses meant for it
    s.potency = 0.0
    s.log = []
    s.blitz = []                  # (t, kind, under RoF, under Brotherhood)
    s.replies = []                # (t, kind, under RoF)
    s.counts = {}
    s.opo_in_rof = 0
    return s


def tick(s, t):
    dt = t - s.t
    if dt <= 0:
        return
    if s.pb_charges >= PB_CHARGES:
        s.pb_wasted += dt
    else:
        grown = s.pb_charges + dt / PB_RECAST
        if grown > PB_CHARGES:
            s.pb_wasted += (grown - PB_CHARGES) * PB_RECAST
        s.pb_charges = min(float(PB_CHARGES), grown)
    s.t = t


def rof(s, t):
    return s.rof_until > t


def bh(s, t):
    return s.bh_until > t


def pb(s, t):
    return s.pb_stacks > 0 and s.pb_until > t


def formless(s, t):
    return s.formless_until > t


def fires(s, t):
    return s.fires_until > t


def winds(s, t):
    return s.winds_until > t


def opo_ws(s, t):
    return "lo" if s.opo_fury > 0 else "dk"


def raptor_ws(s):
    return "rr" if s.raptor_fury > 0 else "ts"


def coeurl_ws(s):
    return "pc" if s.coeurl_fury > 0 else "dem"


def value(s, g, crit_lo):
    if g == "lo":
        return POTENCY["lo_fury" if s.opo_fury > 0 else "lo"] * crit_lo
    if g == "rr":
        return POTENCY["rr_fury" if s.raptor_fury > 0 else "rr"]
    if g == "pc":
        return POTENCY["pc_fury" if s.coeurl_fury > 0 else "pc"]
    return POTENCY[g]


def blitz_kind(s):
    if s.solar and s.lunar:
        return "pr"
    if len(set(s.beast)) == 1:
        return "eb"
    if len(set(s.beast)) == 3:
        return "rp"
    return "cr"


def next_blitz_is_solar(s):
    return not s.solar


def solar_step(s, t, policy, crit_lo):
    """The next weaponskill of a Solar sequence under Perfect Balance."""
    options = []
    if "raptor" not in s.beast:
        options.append(raptor_ws(s))
    if "coeurl" not in s.beast:
        options.append(coeurl_ws(s))
    if "opo" not in s.beast:
        options.append(opo_ws(s, t))
    if policy["solar_order"] == "balance":
        # "order the three GCDs from weakest to strongest"
        options.sort(key=lambda g: value(s, g, crit_lo))
    return options[0]


def choose_gcd(s, t, policy, crit_lo):
    """MNK_Reborn.GeneralGCD for one boss target (policy 'rsr'), or The Balance's use ('balance')."""
    if len(s.beast) == PB_STACKS and (policy["gcd_order"] == "balance" or rof(s, t) or t - s.rof_used < BLITZ_HOLD):
        return blitz_kind(s)
    fr_ok = fires(s, t) and ((not pb(s, t) and not formless(s, t)
                              and (s.last_gcd if policy["gcd_order"] == "rsr" else s.last_form_gcd) in OPO)
                             or s.fires_until - t <= REPLY_ENDING)
    wr_ok = winds(s, t) and (not pb(s, t) or s.winds_until - t <= REPLY_ENDING)
    if policy["gcd_order"] == "rsr":
        if fr_ok:
            return "fr"
        if wr_ok:
            return "wr"
        if formless(s, t):
            return opo_ws(s, t)
    else:
        # The opener image: the Formless Fist of a Blitz on opo-opo first, then the Replies.
        if formless(s, t):
            return opo_ws(s, t)
        if wr_ok:
            return "wr"
        if fr_ok:
            return "fr"
    if pb(s, t) and not s.solar:
        return solar_step(s, t, policy, crit_lo)
    if pb(s, t):
        return opo_ws(s, t)
    if s.form == "coeurl":
        return coeurl_ws(s)
    if s.form == "raptor":
        return raptor_ws(s)
    return opo_ws(s, t)


def do_gcd(s, t, g, crit_lo):
    up = (ROF_UP if rof(s, t) else 1.0) * (BH_UP if bh(s, t) else 1.0)
    s.potency += value(s, g, crit_lo) * up
    s.counts[g] = s.counts.get(g, 0) + 1
    s.log.append((t, g, rof(s, t), bh(s, t)))
    if g in OPO | RAPTOR | COEURL:
        bonus = pb(s, t) or formless(s, t) or s.form == ("opo" if g in OPO else "raptor" if g in RAPTOR
                                                          else "coeurl")
        if pb(s, t):
            s.pb_stacks -= 1
            s.beast.append("opo" if g in OPO else "raptor" if g in RAPTOR else "coeurl")
        elif formless(s, t):
            s.formless_until = -1.0
        if g == "dk" and bonus:
            s.opo_fury = 1
        if g == "lo":
            s.opo_fury = 0
        if g == "ts":
            s.raptor_fury = 1
        if g == "rr":
            s.raptor_fury = 0
        if g == "dem":
            s.coeurl_fury = 2
        if g == "pc":
            s.coeurl_fury = max(0, s.coeurl_fury - 1)
        s.form = "raptor" if g in OPO else "coeurl" if g in RAPTOR else "opo"
        if g in OPO and rof(s, t):
            s.opo_in_rof += 1
        s.last_form_gcd = g
    if g in BLITZ:
        s.blitz.append((t, g, rof(s, t), bh(s, t)))
        if g == "pr":
            s.solar = s.lunar = False
        elif g == "eb":
            s.lunar = True
        elif g == "rp":
            s.solar = True
        elif s.lunar:
            s.solar = True
        else:
            s.lunar = True
        s.beast = []
        s.formless_until = t + FORMLESS_TIME
    if g == "fr":
        s.fires_until = -1.0
        s.formless_until = t + FORMLESS_TIME
        s.replies.append((t, g, rof(s, t)))
    if g == "wr":
        s.winds_until = -1.0
        s.replies.append((t, g, rof(s, t)))
    s.last_gcd = g


def will_have(ready, w, t, gcd, remain):
    """ActionCooldownInfo.WillHaveOneCharge: ready now, or the recast left after the next weaponskill begins
    (RecastTimeRemainOneCharge subtracts DefaultGCDRemain) is at most remain."""
    return ready <= w or ready - (t + gcd) <= remain


def pb_can_use(s, w):
    return s.pb_charges >= 1 and not s.beast and not pb(s, w)


def gcds_before_rof(s, t, gcd):
    """How many weaponskills after the one at t Riddle of Fire is late-woven: 0 when it comes off cooldown in this
    weaponskill's own weave window - the Openers page's "3 to 1 gcd range" are 3, 2 and 1."""
    if s.rof_ready <= t + s.weave[1]:
        return 0
    return math.ceil((s.rof_ready - t - s.weave[1]) / gcd - 1e-9)


def even_window_coming(s, w):
    """Brotherhood comes back with this Riddle of Fire, or is already pressed for it."""
    return s.bh_ready <= s.rof_ready + 1e-6 or bh(s, w)


def choose_ability(s, w, t, gcd, slot, policy):
    """MNK_Reborn.EmergencyAbility and AttackAbility (policy 'rsr'), or The Balance's use ('balance')."""
    last_opo = s.last_gcd in OPO
    if pb_can_use(s, w):
        if w < 3.0:                                            # CombatElapsedLess(3): the opener
            return "pb"
        if (rof(s, w) and bh(s, w) and last_opo and not formless(s, w) and not fires(s, w)
                and not winds(s, w)):                          # "opener 2nd burst" = the even window's second
            return "pb"
        if policy["pb_pre"] == "balance" or policy["odd_solar_pre"]:
            # "Press PB after an opo gcd in the 3 to 1 gcd range before RoF"; Basic Guide: "When you press an
            # Opo-opo GCD 2-4 GCDs before RoF and BH come off cooldown, follow it with a Perfect Balance" - the
            # Solar odd window the same way.
            even = even_window_coming(s, w)
            wanted = ((policy["pb_pre"] == "balance" and even)
                      or (policy["odd_solar_pre"] and not even and next_blitz_is_solar(s)))
            if wanted and not rof(s, w) and last_opo and 1 <= gcds_before_rof(s, t, gcd) <= 3:
                return "pb"
        if rof(s, w) and not (t + gcd - s.pb_used < ODD_PB_GAP) and last_opo:   # "odd min burst"
            if not (policy["odd_solar_pre"] and s.pb_in_window.get(s.window, 0) >= 1 and not bh(s, w)):
                return "pb"
        if (policy["pb_pre"] == "rsr" and not rof(s, w) and will_have(s.rof_ready, w, t, gcd, EVEN_PB_ROF_GCDS * gcd)
                and will_have(s.bh_ready, w, t, gcd, EVEN_PB_BH) and last_opo):  # "even min burst"
            return "pb"
    rof_ok = s.rof_ready <= w and not rof(s, w) and not fires(s, w)
    if policy["late_rof"] and slot == 0:
        rof_ok = False
    if rof_ok and s.last_ability == "bh":
        return "rof"
    if w >= 2 * gcd and will_have(s.rof_ready, w, t, gcd, BH_AHEAD) and s.bh_ready <= w and not bh(s, w):
        return "bh"
    if w >= 3 * gcd and rof_ok:
        return "rof"
    if w >= 3 * gcd and s.row_ready <= w and not winds(s, w):
        return "row"
    return None


def do_ability(s, w, a):
    s.log.append((w, a.upper(), rof(s, w), bh(s, w)))
    if a == "pb":
        s.pb_charges -= 1
        s.pb_stacks = PB_STACKS
        s.pb_until = w + PB_TIME
        s.pb_used = w
        # a Perfect Balance pressed before Riddle of Fire belongs to the coming window
        key = s.window if rof(s, w) else s.window + 1
        s.pb_in_window[key] = s.pb_in_window.get(key, 0) + 1
    if a == "rof":
        s.rof_used = w
        s.rof_ready = w + ROF_RECAST
        s.rof_until = w + ROF_TIME
        s.fires_until = w + ROF_TIME
        s.window += 1
    if a == "bh":
        s.bh_used = w
        s.bh_uses.append(w)
        s.bh_ready = w + BH_RECAST
        s.bh_until = w + BH_TIME
    if a == "row":
        s.row_ready = w + ROW_RECAST
        s.winds_until = w + WINDS_TIME
    s.last_ability = a


def run(minutes, gcd, policy, crit_lo=1.0, no_pb=False, late=None):
    """late: the second weave slot after a weaponskill; default WEAVE[1], The Balance's late weave is gcd - 0.55."""
    s = new_state()
    s.weave = (WEAVE[0], WEAVE[1] if late is None else late)
    t = 0.0
    end = minutes * 60.0
    while t < end:
        tick(s, t)
        g = choose_gcd(s, t, policy, crit_lo)
        do_gcd(s, t, g, crit_lo)
        for slot, off in enumerate(s.weave):
            w = t + off
            tick(s, w)
            a = choose_ability(s, w, t, gcd, slot, policy)
            if a == "pb" and no_pb:
                a = None
            if a:
                do_ability(s, w, a)
        t += gcd
    tick(s, end)
    return s


def summary(s):
    """Potency and what fell outside the buffs. Wind's Reply is left out: Riddle of Wind runs on its own 90 s, and
    The Balance uses it "whenever Riddle of Wind aligns" - outside Riddle of Fire the other times."""
    in_even = [(t, g, r, b) for (t, g, r, b) in s.blitz if any(u <= t < u + ROF_TIME + 2 * 2.5 for u in s.bh_uses)]
    return {"potency": round(s.potency), "blitz": len(s.blitz), "blitz_out_rof": sum(1 for b in s.blitz if not b[2]),
            "even_blitz_out_bh": sum(1 for b in in_even if not b[3]),
            "fr_out_rof": sum(1 for _, g, r in s.replies if g == "fr" and not r),
            "pb_wasted": round(s.pb_wasted, 1), "opo_in_rof": s.opo_in_rof}


def sequence(s, until):
    return [g for (t, g, _, _) in s.log if t < until and g.islower()]


def self_test():
    gcd = 1.94
    # The Balance, Openers page, "Solar Lunar - DK Opener (5s Buffs)", read as an image: 14 weaponskills.
    image = ["dk", "ts", "dem", "lo", "rp", "dk", "wr", "fr", "lo", "dk", "lo", "dk", "eb", "lo"]
    b = run(1, gcd, BALANCE)
    assert sequence(b, 14 * gcd - 0.1) == image, sequence(b, 14 * gcd - 0.1)
    r = run(1, gcd, RSR)
    rsr_opener = sequence(r, 14 * gcd - 0.1)
    # MNK_Reborn presses Wind's Reply before the Formless Fist of Rising Phoenix; the weaponskills are the same.
    assert sorted(rsr_opener) == sorted(image) and rsr_opener[5:7] == ["wr", "dk"], rsr_opener
    abilities = [(int(w // gcd), a) for (w, a, _, _) in r.log if a.isupper() and w < 14 * gcd]
    assert abilities[:5] == [(0, "PB"), (2, "BH"), (2, "ROF"), (3, "ROW"), (8, "PB")], abilities
    # The even window of the Openers page: PB 3 to 1 GCD before RoF, first window, Formless on opo, Replies,
    # Formless on opo, second window, Formless on opo - every Blitz and Reply of minute 2 under both buffs.
    b = run(3, gcd, BALANCE)
    window = [(t, g) for (t, g, rf, bb) in b.log if 118 <= t < 145 and g.islower()]
    kinds = [g for _, g in window]
    assert sum(1 for g in kinds if g in BLITZ) == 2, kinds
    assert all(rf and bb for (t, g, rf, bb) in b.log if 118 <= t < 145 and g in BLITZ | {"fr"}), window
    # The defect of c6ed70be4: at 1.97 s Brotherhood goes out before the opo-opo weaponskill of minute 4, the even
    # window gets one Perfect Balance, and three quarters of a charge run over - over ten minutes one Blitz; the
    # identifier since A412 keeps it.
    before, now = run(10, 1.97, RSR_BEFORE), run(10, 1.97, RSR)
    assert before.pb_wasted > PB_RECAST / 2 > now.pb_wasted and len(now.blitz) == len(before.blitz) + 1, \
        (before.pb_wasted, now.pb_wasted, len(before.blitz), len(now.blitz))
    # Perfect Balance never pressed: the charges run over, the Blitzes are gone.
    d = run(5, gcd, RSR, no_pb=True)
    assert d.pb_wasted > 5 * 60 - 1 and not d.blitz, (d.pb_wasted, d.blitz)
    print("self-test ok: opener as The Balance's image, even window, the charge lost before A412, Perfect Balance "
          "never pressed")


def trace(policy):
    """The first three minutes of one policy: time, action, Riddle of Fire, Brotherhood."""
    s = run(3, 1.94, policy)
    for (t, a, r, b) in s.log:
        print(f"{t:7.2f}  {a:4s}  {'RoF' if r else '   '} {'BH' if b else '  '}")


def compare(variants, label):
    """Mean, range and count of lower results of each variant against MNK_Reborn as it stands, over fight length, GCD,
    the Leaping Opo factor and the weave slot."""
    results = {key: [] for key in variants}
    lost = {key: 0 for key in variants}
    lost["rsr"] = 0
    runs = 0
    for minutes in range(3, 16):
        for gcd in (1.94, 1.97, 2.0):
            for crit in (1.0, 1.39):
                for late in (WEAVE[1], gcd - 0.55):
                    runs += 1
                    base = run(minutes, gcd, RSR, crit, late=late)
                    lost["rsr"] += base.pb_wasted > PB_RECAST / 2
                    for key, policy in variants.items():
                        other = run(minutes, gcd, policy, crit, late=late)
                        results[key].append(other.potency / base.potency - 1)
                        lost[key] += other.pb_wasted > PB_RECAST / 2
    print(f"{runs} combinations (3-15 min, GCD 1.94/1.97/2.00 s, Leaping Opo factor 1.0/1.39, two weave slots), "
          f"{label}:")
    for key, values in results.items():
        mean = 100 * sum(values) / len(values)
        print(f"  {key:14s} mean {mean:+.2f} %, {100 * min(values):+.2f} % to {100 * max(values):+.2f} %, "
              f"{sum(1 for v in values if v < -1e-9)} lower; half a charge or more of Perfect Balance lost in "
              f"{lost[key]} (MNK_Reborn: {lost['rsr']})")


def main():
    if "-t" in sys.argv:
        trace({"balance": BALANCE, "before": RSR_BEFORE}.get(sys.argv[-1], RSR))
        return
    self_test()
    print()
    lt = 1.94 - 0.55
    print("ten minutes, GCD 1.94 s, second weave slot 0.55 s before the next weaponskill, Leaping Opo factor 1.39:")
    for policy in (RSR_BEFORE, RSR, BALANCE):
        print(f"  {policy['name']:10s} {summary(run(10, 1.94, policy, 1.39, late=lt))}")
    print()
    compare({"rsr_before": RSR_BEFORE}, "MNK_Reborn before A412 against it since")
    print()
    parts = {"balance": BALANCE}
    for key in ("late_rof", "solar_order", "odd_solar_pre", "gcd_order"):
        parts[key] = dict(RSR, name=key, **{key: BALANCE[key]})
    compare(parts, "The Balance's rule against MNK_Reborn since A412 - all of it, then each remaining part alone")


if __name__ == "__main__":
    main()
