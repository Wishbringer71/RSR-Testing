#!/usr/bin/env python3
"""Which familiars go on the battlehorns before the next fight (concept 01, B5; A522).

Not a code scanner - a model calculator. It mirrors the choice BST_Reborn makes out of combat and plays a dungeon run
with the familiar cycle of bst_horn_model (imported; run this model again when that one changes):
  FIXED  his horns as he set them, the whole run
  B5     before each fight the trio for what comes next - a pack of n, or a boss (the nearest enemy not yet fought
         carries a boss icon) - chosen by
           1. as many familiars that heal or protect as required, up to the requirement: two before a boss, one
              more once a danger was seen in this area (BOSS_SAFE; one before a boss is the rejected variant shown)
           2. among them the kind that covers that fight best: before a boss one whose Trick heals (B2 keeps it out
              for the whole danger; a protecting Release ends after 48 s), before a pack one whose Tempered Release
              stops it (C1, bst_control_model)
           3. the most damage against that fight: Trick and, on a horn that spends One with Nature on it, Tempered
              Release - each its potency times the enemies an area reaches, one for a single target
           4. after a danger in this area, more familiars that heal or protect
           5. as many of his familiars kept on their horns as possible
           6. more familiars that heal or protect, then more whose Trick heals
         and the familiar still out keeps its horn: what a reassignment does to it is not measured.
Measured: potency over the run, and at the boss the share of a danger covered (a healing Trick out or a protecting
Release running).

From the game sheets (xivapi v2, Action through Pet.Abilities, 02.11.2026): every Trick 500 here, area or single by
CastType; the Tempered Release figures below (a multi-hit one as its hits times the stated potency). Safe by: "trick"
(its Trick heals), "release" (its Tempered Release protects or heals), "stop" (its Tempered Release stops every enemy
it reaches - counts on a pack only, C1).
ASSUMPTIONS, not from a source, named where used: the pool of familiars he has (those caught up to level 22, his known
ones among them), the length of a pull and of the boss, the size of a pull, when the danger begins. The third horn spends
One with Nature on Borrow, which this model counts as neither protecting nor damaging.
"""
import itertools
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bst_horn_model as horn  # noqa: E402

# name: Trick (potency, area?), Tempered Release (kind, value, what, seconds, area?), safe by
POOL = {
    "Cu Sith": ((500, True), ("damage", 900, None, 0, False), None),
    "Squirrel": ((500, True), ("none", 0, None, 0, True), None),
    "Pugil": ((500, True), ("protect", 0.20, "magic", 48, True), "release"),
    "Opo-opo": ((500, True), ("damage", 750, None, 0, False), None),
    "Coblyn": ((500, False), ("protect", 0.20, "physical", 48, True), "release"),
    "Wespe": ((500, False), ("damage", 2000, None, 0, False), None),
    "Bat": ((500, False), ("damage", 450, None, 0, True), "trick"),
    "Ghost": ((500, True), ("damage", 300, None, 0, True), None),
    "Geshunpest": ((500, True), ("protect", 0.0, "heal", 21, True), "release"),
    "Buffalo": ((500, True), ("damage", 750, None, 0, True), "stop"),
    "Flying Trap": ((500, True), ("damage", 550, None, 0, True), None),
    "Raptor": ((500, True), ("damage", 1000, None, 0, True), None),
    "Slime": ((500, False), ("damage", 550, None, 0, True), "trick"),
}
NATURES = ("tempered", "tempered", "borrow")
HIS_HORNS = ["Geshunpest", "Bat", "Opo-opo"]  # his horns from 20:05 on 02.11.2026 (A489: Gespenst, Pet 60)
BOSS_SAFE = 2   # familiars that heal or protect before a boss - the choice of this model, against 1 in the report
PULL = 45.0     # ASSUMPTION: a wall-to-wall pull lasts 45 s
PULL_SIZE = 5   # ASSUMPTION: five enemies in it
PULLS = 3       # ASSUMPTION: three pulls before the boss
BOSS = 150.0    # ASSUMPTION: the boss lasts 150 s
DANGER = 30.0   # ASSUMPTION: at the boss a danger begins after 30 s and lasts to the end


def safe_on(name, horn_index, pack):
    by = POOL[name][2]
    if by == "trick":
        return True
    if NATURES[horn_index] != "tempered":
        return False
    return by == "release" or (by == "stop" and pack)


def damage_value(name, horn_index, pack, n):
    """Trick, and on a Tempered horn the Tempered Release: potency times the enemies each reaches."""
    potency, area = POOL[name][0]
    value = potency * (n if pack and area else 1)
    kind, release, _, _, release_area = POOL[name][1]
    if NATURES[horn_index] == "tempered" and kind == "damage":
        value += release * (n if pack and release_area else 1)
    return value


def score(trio, current, pack, n, danger, required):
    safe = sum(1 for h, name in enumerate(trio) if name and safe_on(name, h, pack))
    fitting = sum(1 for h, name in enumerate(trio) if name and safe_on(name, h, pack)
                  and POOL[name][2] == ("stop" if pack else "trick"))
    value = sum(damage_value(name, h, pack, n) for h, name in enumerate(trio) if name)
    kept = sum(1 for h, name in enumerate(trio) if name == current[h])
    healers = sum(1 for name in trio if name and POOL[name][2] == "trick")
    return (min(safe, required), min(fitting, 1) if required else 0, value, safe if danger else 0, kept, safe,
            healers)


def choose(pool, current, out, pack, n, danger, scorer=None):
    """The trio for the next fight; `out` is the horn whose familiar is still out (kept), or None."""
    scorer = scorer or score
    required = (0 if pack else BOSS_SAFE) + (1 if danger else 0)
    free = [h for h in range(3) if h != out]
    candidates = [name for name in pool if out is None or name != current[out]]
    best, best_score = list(current), None
    for combo in itertools.permutations(candidates, len(free)):
        trio = list(current)
        for h, name in zip(free, combo):
            trio[h] = name
        s = scorer(trio, current, pack, n, danger, required)
        if best_score is None or s > best_score:
            best, best_score = trio, s
    return best


def install(n):
    """Puts the pool into the horn model; a single-target Release enters as value / n, since play() multiplies every
    damage Release by the enemies (constant within each fight here)."""
    for name, ((potency, area), (kind, value, what, seconds, rarea), by) in POOL.items():
        if kind == "damage" and not rarea:
            value = value / n
        horn.FAMILIARS[name] = dict(trick=(potency, area, "x"), release=(kind, value, what, seconds),
                                    safe="trick" if by == "trick" else ("release" if by == "release" else None))


def fight(trio, seconds, n, danger_from=None):
    install(n)
    horn.FIGHT = seconds
    measure = {}
    total, _ = horn.play(trio, "RULES", lambda t: n, danger_from=danger_from, measure=measure)
    return total, measure


def run(assign, pool=None, current=None, scorer=None):
    """Potency over the pulls and at the boss, and the share of the boss's danger covered."""
    pool = pool or list(POOL)
    current = list(current or HIS_HORNS)
    out, trash, trios = None, 0.0, []
    for _ in range(PULLS):
        if assign:
            current = choose(pool, current, out, True, PULL_SIZE, False, scorer)
        trios.append(list(current))
        total, m = fight(current, PULL, PULL_SIZE)
        trash += total
        out = current.index(m["last"]) if m.get("last") in current else None
    if assign:
        current = choose(pool, current, out, False, 1, False, scorer)
    trios.append(list(current))
    boss, m = fight(current, BOSS, 1, danger_from=DANGER)
    return trash, boss, m["covered"], trios


HORN_SETS = (["Geshunpest", "Bat", "Opo-opo"], ["Wespe", "Coblyn", "Bat"], ["Buffalo", "Raptor", "Slime"],
             ["Cu Sith", "Squirrel", "Ghost"])


def sweep(boss_safe):
    """(danger start or None, boss potency %, change of the danger covered, pull potency %) per case, B5 against FIXED."""
    global PULL, PULL_SIZE, BOSS, DANGER, BOSS_SAFE
    saved = (PULL, PULL_SIZE, BOSS, DANGER, BOSS_SAFE)
    rows = []
    try:
        BOSS_SAFE = boss_safe
        for PULL in (15.0, 45.0, 90.0):
            for PULL_SIZE in (2, 5, 8):
                for BOSS in (60.0, 150.0, 300.0):
                    for start in (2.0, 30.0, 100.0, None):
                        DANGER = start if start is not None else float("inf")
                        for horns in HORN_SETS:
                            f = run(False, current=horns)
                            b = run(True, current=horns)
                            rows.append((start, (b[1] / f[1] - 1) * 100, b[2] - f[2], (b[0] / f[0] - 1) * 100))
    finally:
        PULL, PULL_SIZE, BOSS, DANGER, BOSS_SAFE = saved
    return rows


def report():
    f_trash, f_boss, f_cov, _ = run(False)
    b_trash, b_boss, b_cov, trios = run(True)
    print(f"Pool: {', '.join(POOL)}; his horns {', '.join(HIS_HORNS)}")
    print(f"{PULLS} pulls of {PULL_SIZE} for {PULL:.0f} s, a boss for {BOSS:.0f} s with a danger from {DANGER:.0f} s:")
    print(f"  FIXED  pulls {f_trash:9.0f}   boss {f_boss:9.0f}   danger covered {f_cov:6.1%}")
    print(f"  B5     pulls {b_trash:9.0f} ({(b_trash / f_trash - 1) * 100:+5.1f} %)   boss {b_boss:9.0f} "
          f"({(b_boss / f_boss - 1) * 100:+5.1f} %)   danger covered {b_cov:6.1%}")
    for i, trio in enumerate(trios):
        print(f"  {'pull ' + str(i + 1) if i < PULLS else 'boss  '}: {', '.join(trio)}")
    print("\nA danger seen in the area before the boss (three familiars that heal or protect required):")
    print(f"  {', '.join(choose(list(POOL), trios[-2], None, False, 1, True))}")
    print("\nOver pulls of 15, 45, 90 s and 2, 5, 8 enemies, a boss of 60, 150, 300 s, a danger at it from 2, 30, 100 s or none,"
          "\nand four horn sets of his - against FIXED:")
    for boss_safe in (1, 2):
        rows = sweep(boss_safe)
        quiet = [r for r in rows if r[0] is None]
        hit = [r for r in rows if r[0] is not None]
        print(f"  {boss_safe} before a boss: boss potency without a danger {min(r[1] for r in quiet):+5.1f} % worst, "
              f"{sum(r[1] for r in quiet) / len(quiet):+5.1f} % mean; with one {min(r[1] for r in hit):+5.1f} % worst; "
              f"danger covered worse in {sum(1 for r in hit if r[2] < -1e-9)} of {len(hit)}, "
              f"mean {sum(r[2] for r in hit) / len(hit):+.3f}; pulls {min(r[3] for r in rows):+5.1f} % worst")
    print("\nOnly single-target familiars besides one area one (pool Wespe, Coblyn, Bat, Slime, Ghost):")
    small = ["Wespe", "Coblyn", "Bat", "Slime", "Ghost"]
    s_f = run(False, small, ["Wespe", "Coblyn", "Bat"])
    s_b = run(True, small, ["Wespe", "Coblyn", "Bat"])
    print(f"  FIXED pulls {s_f[0]:9.0f}, B5 pulls {s_b[0]:9.0f} ({(s_b[0] / s_f[0] - 1) * 100:+5.1f} %), "
          f"boss covered {s_f[2]:5.1%} / {s_b[2]:5.1%}")


def self_test():
    pool = list(POOL)
    # A pack: three area Tricks when he has three.
    trio = choose(pool, HIS_HORNS, None, True, PULL_SIZE, False)
    assert all(POOL[name][0][1] for name in trio), trio
    # A boss: two familiars that heal or protect, one of them by its Trick.
    trio = choose(pool, ["Ghost", "Opo-opo", "Raptor"], None, False, 1, False)
    assert sum(1 for h, name in enumerate(trio) if safe_on(name, h, False)) == 2, trio
    assert any(POOL[name][2] == "trick" for name in trio), trio
    # A boss after a danger in this area: three, when he has three that are safe on these horns.
    trio = choose(pool, ["Ghost", "Opo-opo", "Raptor"], None, False, 1, True)
    assert sum(1 for h, name in enumerate(trio) if safe_on(name, h, False)) == 3, trio
    # Over the sweep two before a boss never cover a danger worse than his horns, one does - the reason for two.
    assert all(r[2] >= -1e-9 for r in sweep(2) if r[0] is not None)
    assert any(r[2] < -1e-9 for r in sweep(1) if r[0] is not None)
    # Nothing to gain: his horns stay - the choice made once is made again from it.
    best = choose(pool, HIS_HORNS, None, True, PULL_SIZE, False)
    assert choose(pool, best, None, True, PULL_SIZE, False) == best, best
    # The familiar out keeps its horn.
    trio = choose(pool, ["Wespe", "Coblyn", "Bat"], 0, True, PULL_SIZE, False)
    assert trio[0] == "Wespe", trio
    # The run: B5 brings more on the pulls and covers the boss's danger at least as well, at no loss on the boss.
    f_trash, f_boss, f_cov, _ = run(False)
    b_trash, b_boss, b_cov, _ = run(True)
    assert b_trash > f_trash and b_cov >= f_cov, (f_trash, b_trash, f_cov, b_cov)
    # Constructed defect 1: a choice that ignores the size of the pack gains nothing on the pulls.
    def blind(trio, current, pack, n, danger, required):
        return score(trio, current, pack, 1, danger, required)
    x_trash, _, _, _ = run(True, scorer=blind)
    assert x_trash < b_trash, (x_trash, b_trash)
    # Constructed defect 2: a choice by damage alone, then his horns, leaves the boss without a familiar that heals or
    # protects when his horns hold none.
    def careless(trio, current, pack, n, danger, required):
        full = score(trio, current, pack, n, danger, required)
        return (full[2], full[4])
    trio = choose(pool, ["Ghost", "Opo-opo", "Raptor"], None, False, 1, False, careless)
    assert not any(safe_on(name, h, False) for h, name in enumerate(trio)), trio
    print("self-test ok: area Tricks on a pack, two familiars that heal or protect before a boss and three after a danger, "
          "two never covering a boss's danger worse than his horns where one does, "
          "his horns kept when nothing is gained, the familiar out keeps its horn, the run gains on the pulls without "
          "losing the boss's cover, and a choice blind to the pack or to the requirement is caught")


if __name__ == "__main__":
    self_test()
    if "--self-test" not in sys.argv:
        report()
