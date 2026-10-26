#!/usr/bin/env python3
"""The Samurai against The Balance: Higanbana, Meikyo Shisui and Sen in SAM_Reborn's single-target loop (concept 14,
"Vergleich mit The Balance", A381).

Not a code scanner - a model calculator. The question: The Balance (Samurai Basic Guide, "Patch Applicable: 7.4", read
26.10.2026) uses Meikyo Shisui in every 60-s window to put Higanbana up "right on time" - Iaijutsu does not use up a
Meikyo stack -, and its "15 second rule" presses Meikyo when Higanbana runs out while Sen are still being built.
SAM_Reborn does the opposite: it keeps Meikyo back while Higanbana runs out within its refresh window and casts
Higanbana only without Meikyo stacks. Does that cost Higanbana time on the target, Meikyo charges or Sen - damage of
the Samurai's own, not only the alignment with other players' buffs, which the owner's rule does not count?

The GCD choice of SAM_Reborn is mirrored by hand from RotationSolver/RebornRotations/Melee/SAM_Reborn.cs (GeneralGCD
and the Meikyo Shisui branch of AttackAbility, as of 7e6a428a9); a change there needs the same change here (TODO,
technical debt). Figures: job guide, patch 7.5 (na.finalfantasyxiv.com/jobguide/samurai, read 26.10.2026) - Higanbana
200 and 50 every 3 s for 60 s, Meikyo Shisui 3 weaponskills in 20 s, recast 55 s, 2 charges, Tendo 30 s, Fugetsu and
Fuka 40 s, Fuka 13 % faster GCD, Ikishoten 120 s with Ogi Namikiri Ready 30 s, Tsubame-gaeshi Ready 30 s.

Simplifications: one target that lives through the fight (a boss), every positional hits, no movement, no downtime,
Kenki and its abilities left out (they do not change the GCD choice here), Higanbana's settings as shipped (refresh
6 GCDs, no multi-target block).

The self-test checks the rules of the model against hand-built cases and that a constructed defect - Higanbana never
refreshed - shows as lost uptime.

Usage: python3 .github/scripts/audit/samurai_balance_model.py [-t] [-b]   (-t trace, -b trace The Balance's use)
"""
import sys

GCD = round(2.5 * 0.87, 2)       # under Fuka
WEAVE = 0.7
HIGANBANA = 60.0
HIGANBANA_TICK = 3.0
REFRESH_GCDS = 6                  # HiganbanaPvE StatusRefreshGcdCount
BUFF = 40.0
BUFF_ENDING = 5.0                 # WillFugetsuEnd / WillFukaEnd
MEIKYO_RECAST, MEIKYO_STACKS, MEIKYO_TIME, TENDO_TIME = 55.0, 3, 20.0, 30.0
IKISHOTEN_RECAST, OGI_TIME, TSUBAME_TIME = 120.0, 30.0, 30.0


# Job guide, patch 7.5: combo potencies (every positional hits), Iaijutsu, Ogi Namikiri; Shoha for three Meditation.
POTENCY = {"gyofu": 240, "jinpu": 300, "shifu": 300, "gekko": 420, "kasha": 420, "yukikaze": 340, "higanbana": 200,
           "midare": 680, "kaeshi": 680, "tendo": 1100, "tendo_kaeshi": 1100, "ogi": 1000, "kaeshi_namikiri": 1000}
HIGANBANA_DOT, SHOHA = 50, 640
MEDITATION = {"higanbana", "midare", "tendo", "ogi"}


class S:
    pass


def new_state():
    s = S()
    s.t = 0.0
    s.sen = set()
    s.combo = None            # last combo weaponskill: gyofu, jinpu, shifu
    s.meikyo_stacks = 0
    s.meikyo_until = -1.0
    s.meikyo_charges = 2.0    # fractional: recharge accrues continuously
    s.meikyo_wasted = 0.0     # seconds of recharge lost at the cap
    s.tendo_until = -1.0
    s.tsubame = None          # "kaeshi" or "tendo"
    s.tsubame_until = -1.0
    s.higan_until = -1.0
    s.higan_casts = 0
    s.higan_clipped = 0.0
    s.higan_down = 0.0
    s.fugetsu_until = -1.0
    s.fuka_until = -1.0
    s.ikishoten_ready = 0.0
    s.ogi_until = -1.0
    s.kaeshi_namikiri = False
    s.last = None
    s.dup_sen = 0
    s.log = []
    s.counts = {}
    return s


def tick(s, t):
    """Advance the Meikyo recharge to t."""
    dt = t - s.t
    if dt <= 0:
        return
    if s.meikyo_charges >= 2:
        s.meikyo_wasted += dt
    else:
        s.meikyo_charges = min(2.0, s.meikyo_charges + dt / MEIKYO_RECAST)
    if s.higan_until < t:
        s.higan_down += t - max(s.higan_until, s.t)
    s.t = t


def buffs(s, t):
    return s.fugetsu_until > t and s.fuka_until > t


def meikyo(s, t):
    return s.meikyo_stacks > 0 and s.meikyo_until > t


def higan_due(s, t):
    return s.higan_until - t <= REFRESH_GCDS * GCD


def choose_gcd(s, t, no_refresh=False, policy="rsr"):
    """SAM_Reborn.GeneralGCD for one boss target; policy 'balance' lets Higanbana go under Meikyo Shisui, as The
    Balance does ("Iaijutsu does not count as one of the three Weaponskills on Meikyo")."""
    # UseOgiAndHiganbana
    if s.ogi_until > t:
        if (s.higan_until > t and buffs(s, t)) or s.ogi_until - t <= GCD:
            return "ogi"
    if (buffs(s, t) and s.fugetsu_until - t > BUFF_ENDING and s.fuka_until - t > BUFF_ENDING
            and (policy == "balance" or not meikyo(s, t)) and len(s.sen) == 1 and higan_due(s, t)
            and not no_refresh):
        return "higanbana"
    # UseAoeFinishers: Kaeshi Namikiri
    if s.kaeshi_namikiri:
        return "kaeshi_namikiri"
    # UseSetsugekka
    if len(s.sen) == 3 and s.tendo_until > t:
        return "tendo"
    if len(s.sen) == 3:
        return "midare"
    if s.tsubame == "kaeshi" and s.tsubame_until > t:
        return "kaeshi"
    if s.tsubame == "tendo" and s.tsubame_until > t:
        return "tendo_kaeshi"
    # UseSingleTargetFinishers
    mk = meikyo(s, t)
    if s.combo == "jinpu" or (mk and "getsu" not in s.sen):
        return "gekko"
    if s.combo == "shifu" or (mk and "ka" not in s.sen):
        return "kasha"
    if "setsu" not in s.sen and buffs(s, t) and (s.combo == "gyofu" or mk):
        return "yukikaze"
    # UseSingleTargetBuffs
    if s.combo == "gyofu":
        if buffs(s, t):
            return "jinpu" if s.fugetsu_until <= s.fuka_until else "shifu"
        if s.fugetsu_until <= t:
            return "jinpu"
        if s.fuka_until <= t:
            return "shifu"
    # UseComboStarters
    if not mk and not (s.tsubame and s.tsubame_until > t):
        return "gyofu"
    if mk and "setsu" not in s.sen:
        return "yukikaze"
    return "gyofu"


def do_gcd(s, t, g):
    s.counts[g] = s.counts.get(g, 0) + 1
    combo_ws = {"gyofu", "jinpu", "shifu", "gekko", "kasha", "yukikaze"}
    finishers = {"gekko": "getsu", "kasha": "ka", "yukikaze": "setsu"}
    if g in finishers:
        if finishers[g] in s.sen:
            s.dup_sen += 1
        s.sen.add(finishers[g])
        if g == "gekko":
            s.fugetsu_until = t + BUFF
        if g == "kasha":
            s.fuka_until = t + BUFF
        s.combo = None
    elif g in ("gyofu", "jinpu", "shifu"):
        s.combo = g
    if g in combo_ws and meikyo(s, t):
        s.meikyo_stacks -= 1
        if g in ("gyofu", "jinpu", "shifu"):
            s.combo = None
    if g == "jinpu":
        s.fugetsu_until = t + BUFF
    if g == "shifu":
        s.fuka_until = t + BUFF
    if g == "higanbana":
        if s.higan_until > t:
            s.higan_clipped += s.higan_until - t
        s.higan_until = t + HIGANBANA
        s.higan_casts += 1
        s.sen.clear()
    if g in ("midare", "tendo"):
        s.sen.clear()
        s.tsubame = "tendo" if g == "tendo" else "kaeshi"
        s.tsubame_until = t + TSUBAME_TIME
        if g == "tendo":
            s.tendo_until = -1.0
    if g in ("kaeshi", "tendo_kaeshi"):
        s.tsubame = None
    if g == "ogi":
        s.ogi_until = -1.0
        s.kaeshi_namikiri = True
    if g == "kaeshi_namikiri":
        s.kaeshi_namikiri = False
    s.last = g


def choose_meikyo(s, t, policy):
    """The Meikyo Shisui branch of SAM_Reborn.AttackAbility (policy 'rsr'), or The Balance's use ('balance')."""
    if meikyo(s, t):
        return False
    tsubame_ready = s.tsubame is not None and s.tsubame_until > t
    if policy == "rsr":
        near_cap = s.meikyo_charges + GCD / MEIKYO_RECAST >= 2
        if not (tsubame_ready or near_cap):
            if s.meikyo_charges < 2:
                return False
        if s.meikyo_charges < 1:
            return False
        outer = (not buffs(s, t)) or (s.higan_until > t and not higan_due(s, t))
        inner = (not buffs(s, t)) or tsubame_ready or s.last == "tendo_kaeshi"
        return outer and inner
    # The Balance's use, as SAM_Reborn could take it over: the same branch without the hold while Higanbana is due, and
    # one more trigger - the "15 second rule": Higanbana runs out while Sen are being built, the combo is finished, so
    # Meikyo brings the Sen; that trigger may spend the last charge.
    due = higan_due(s, t)
    fifteen = due and len(s.sen) != 1 and s.combo is None
    near_cap = s.meikyo_charges + GCD / MEIKYO_RECAST >= 2
    if not (near_cap or tsubame_ready or fifteen) and s.meikyo_charges < 2:
        return False
    if s.meikyo_charges < 1:
        return False
    outer = (not buffs(s, t)) or s.higan_until > t
    inner = (not buffs(s, t)) or tsubame_ready or s.last == "tendo_kaeshi" or fifteen
    return outer and inner



def choose_ogcd(s, t, combat, policy="rsr"):
    if combat >= 2 * GCD and s.ikishoten_ready <= t:
        s.ikishoten_ready = t + IKISHOTEN_RECAST
        s.ogi_until = t + OGI_TIME
        return "ikishoten"
    if choose_meikyo(s, t, policy):
        s.meikyo_charges -= 1
        s.meikyo_stacks = MEIKYO_STACKS
        s.meikyo_until = t + MEIKYO_TIME
        s.tendo_until = t + TENDO_TIME
        s.combo = None
        return "meikyo"
    return None


def run(fight=600.0, trace=False, no_refresh=False, policy="rsr"):
    s = new_state()
    # Countdown: Meikyo Shisui at -14 s (The Balance and RSR's default), one charge spent.
    s.meikyo_charges = 1.0 + 14.0 / MEIKYO_RECAST
    s.meikyo_stacks, s.meikyo_until, s.tendo_until = MEIKYO_STACKS, MEIKYO_TIME - 14.0, TENDO_TIME - 14.0
    t = 0.0
    while t < fight:
        tick(s, t)
        g = choose_gcd(s, t, no_refresh, policy)
        do_gcd(s, t, g)
        if trace:
            s.log.append((round(t, 2), g, "".join(sorted(x[0] for x in s.sen)), round(s.higan_until - t, 1),
                          round(s.meikyo_charges, 2), s.meikyo_stacks))
        w = t + WEAVE
        tick(s, w)
        o = choose_ogcd(s, w, w, policy)
        if trace and o:
            s.log.append((round(w, 2), o))
        t += GCD
    tick(s, fight)
    return s


def summary(s, fight=600.0):
    uptime = 1 - s.higan_down / fight
    meditation = sum(n for g, n in s.counts.items() if g in MEDITATION)
    potency = (sum(POTENCY[g] * n for g, n in s.counts.items()) + (fight - s.higan_down) / HIGANBANA_TICK * HIGANBANA_DOT
               + (meditation // 3) * SHOHA)
    return {
        "potency": round(potency),
        "higanbana casts": s.higan_casts,
        "higanbana uptime": round(uptime, 4),
        "higanbana clipped s": round(s.higan_clipped, 1),
        "meikyo recharge lost at cap s": round(s.meikyo_wasted, 1),
        "tendo": s.counts.get("tendo", 0),
        "midare": s.counts.get("midare", 0),
        "duplicate sen": s.dup_sen,
    }


def self_test():
    # Hand-built: with three Sen and Tendo the next GCD is Tendo Setsugekka, without Tendo Midare.
    s = new_state()
    s.sen = {"getsu", "ka", "setsu"}
    s.fugetsu_until = s.fuka_until = 30.0
    s.tendo_until = 10.0
    assert choose_gcd(s, 0.0) == "tendo"
    s.tendo_until = -1.0
    assert choose_gcd(s, 0.0) == "midare"
    # One Sen, Higanbana running out within its refresh window, no Meikyo: Higanbana.
    s = new_state()
    s.sen = {"getsu"}
    s.fugetsu_until = s.fuka_until = 30.0
    s.higan_until = 5.0
    s.higan_casts = 1
    assert choose_gcd(s, 0.0) == "higanbana"
    # The same under Meikyo: SAM_Reborn keeps Higanbana back, The Balance casts it (Iaijutsu uses no Meikyo stack).
    s.meikyo_stacks, s.meikyo_until = 2, 15.0
    assert choose_gcd(s, 0.0) != "higanbana"
    assert choose_gcd(s, 0.0, policy="balance") == "higanbana"
    # Higanbana due, no Sen and no combo begun: SAM_Reborn holds Meikyo, The Balance presses it (15 second rule).
    s = new_state()
    s.fugetsu_until = s.fuka_until = 30.0
    s.higan_until, s.higan_casts, s.meikyo_charges = 8.0, 1, 1.0
    assert not choose_meikyo(s, 0.0, "rsr") and choose_meikyo(s, 0.0, "balance")
    # Constructed defect: Higanbana never refreshed must show as lost uptime - a silent full uptime would not be.
    broken = run(no_refresh=True)
    assert summary(broken)["higanbana uptime"] < 0.5, summary(broken)
    print("self-test ok: Iaijutsu choice, Higanbana under Meikyo and the 15 second rule per policy, the never-refreshed"
          " Higanbana shows as lost uptime")


def grid():
    """The Balance's use against SAM_Reborn's across GCD lengths, Higanbana refresh windows and fight lengths: the gain
    in potency, in percent, for each combination."""
    global GCD, REFRESH_GCDS
    keep = GCD, REFRESH_GCDS
    gains = []
    try:
        for GCD in (2.10, 2.14, 2.17, 2.20):
            for REFRESH_GCDS in (2, 4, 6):
                for fight in (180.0, 300.0, 420.0, 600.0, 900.0):
                    r = summary(run(fight=fight), fight)["potency"]
                    b = summary(run(fight=fight, policy="balance"), fight)["potency"]
                    gains.append(100 * (b - r) / r)
    finally:
        GCD, REFRESH_GCDS = keep
    return gains


if __name__ == "__main__":
    self_test()
    policy = "balance" if "-b" in sys.argv else "rsr"
    s = run(trace="-t" in sys.argv, policy=policy)
    print("policy:", policy, "- 600 s, GCD", GCD, "- refresh", REFRESH_GCDS, "GCDs")
    for k, v in summary(s).items():
        print(f"   {k}: {v}")
    if policy == "rsr":
        b = summary(run(policy="balance"))
        print("policy: balance")
        for k, v in b.items():
            print(f"   {k}: {v}")
        g = grid()
        print(f"The Balance's Meikyo use against SAM_Reborn's over {len(g)} combinations of GCD, refresh window and fight"
              f" length: mean {sum(g) / len(g):+.2f} %, from {min(g):+.2f} % to {max(g):+.2f} %, lower in"
              f" {sum(1 for x in g if x < 0)}")
    if "-t" in sys.argv:
        for e in s.log:
            print(e)
