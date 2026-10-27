#!/usr/bin/env python3
"""Heals over time in an ACT network log: were they needed, and where did their ticks land (concept 08, "Schaden über
Zeit: HoT gegen DoT"; A430).

Not a code scanner - the measuring tool for the heal-over-time rule. It reads one ACT log (Network_*.log, fields split
by |; cactbot LogGuide) and reports, per zone:

  casts    every heal over time the player cast that costs a GCD (Regen, Medica II/III, Aspected Benefic, Aspected
           Helios, Helios Conjunction), and how many of them were needed after the fact: the target - for a group HoT
           at least two members - would have fallen below 65 % within the HoT's time without any heal, by the damage
           and the DoT ticks that actually followed. A HoT laid ahead of a pull is counted like any other.
  ticks    the overheal of the HoT ticks on party members (line 24), by which of the player's own HoTs ran on the
           member: GCD HoTs, HoTs free of a GCD (Asylum, Divine Aura, the oGCD HoTs of the other healers), none. The
           game reports one aggregated tick per member, so a tick under the player's HoT may hold a co-healer's share.
  dots     the DoT ticks on party members as a share of their maximum HP, and how many of them reported 0.

Actions and statuses by id, so the client language does not matter. Names of players are never printed.

Usage: python3 .github/scripts/audit/act_heal_need.py <Network_*.log> [--self-test]
"""
import sys
from collections import defaultdict
from datetime import datetime

THRESHOLD = 0.65
GCD_HOTS = {137: ("Regen", 18.0, False), 133: ("Medica II", 15.0, True), 37010: ("Medica III", 15.0, True),
            3595: ("Aspected Benefic", 15.0, False), 3601: ("Aspected Helios", 15.0, True),
            37030: ("Helios Conjunction", 15.0, True)}
GCD_HOT_STATUS = {158, 150, 3880, 3986, 835, 836, 3894}
FREE_HOT_STATUS = {1911, 3904, 1879, 315, 1874, 1944, 2620, 2617, 2938}


def stamp(text):
    return datetime.fromisoformat(text[:26] + text[-6:]).timestamp()


def effect_amount(value):
    """An amount from an action line (21/22): bytes ABCD, a third byte 0x40 carries a fourth (cactbot LogGuide)."""
    try:
        x = int(value, 16)
    except ValueError:
        return 0
    a, b, c, d = x >> 24 & 0xFF, x >> 16 & 0xFF, x >> 8 & 0xFF, x & 0xFF
    return (d << 16 | a << 8 | b) if c & 0x40 else (a << 8 | b)


def read(lines):
    me, party, zone = None, set(), ""
    events, casts = [], []
    own = defaultdict(set)
    last = {}
    ticks = defaultdict(lambda: [0, 0, 0])
    dots = defaultdict(list)
    for raw in lines:
        f = raw.rstrip("\n").split("|")
        kind = f[0]
        if kind == "02" and len(f) > 2:
            me = f[2]
        elif kind == "01" and len(f) > 3:
            zone = f[3]
        elif kind == "11" and len(f) > 3:
            party = set(f[3:3 + int(f[2])])
        elif kind in ("26", "30") and len(f) > 8 and f[5] == me and f[2]:
            status = int(f[2], 16)
            (own[f[7]].add if kind == "26" else own[f[7]].discard)(status)
        elif kind in ("21", "22") and len(f) > 25 and f[6] in party:
            t = stamp(f[1])
            if f[24] and f[25]:
                last[f[6]] = (int(f[24]), int(f[25]))
                events.append((t, "hp", f[6], last[f[6]]))
            for i in range(8, 24, 2):
                if f[i] and int(f[i], 16) & 0xFF == 3:
                    events.append((t, "loss", f[6], effect_amount(f[i + 1])))
            action = int(f[4], 16) if f[2] == me and f[4] else 0
            if action in GCD_HOTS and not (casts and casts[-1][0] == t and casts[-1][1] == action):
                casts.append((t, action, f[6], frozenset(party), zone))
        elif kind == "24" and len(f) > 18 and f[2] in party and f[7] and f[8]:
            t, amount, cur, top = stamp(f[1]), int(f[6], 16), int(f[7]), int(f[8])
            if f[4] == "DoT":
                events.append((t, "loss", f[2], amount))
                dots[zone].append(amount / top if top else 0.0)
            else:
                before = last.get(f[2], (cur, top))[0]
                landed = max(0, min(amount, top - before))
                hots = own[f[2]]
                group = ("GCD HoT" if hots & GCD_HOT_STATUS else "free HoT" if hots & FREE_HOT_STATUS
                         else "none of the player's")
                cell = ticks[(zone, group)]
                cell[0] += amount
                cell[1] += amount - landed
                cell[2] += 1
            last[f[2]] = (cur, top)
            events.append((t, "hp", f[2], (cur, top)))
        elif kind == "37" and len(f) > 6 and f[2] in party and f[5] and f[6]:
            last[f[2]] = (int(f[5]), int(f[6]))
            events.append((stamp(f[1]), "hp", f[2], last[f[2]]))
    events.sort(key=lambda e: e[0])
    return casts, events, ticks, dots


def needed(casts, events):
    """Per zone and action: casts, and casts needed after the fact."""
    out = defaultdict(lambda: [0, 0])
    hp, i = {}, 0
    for t, action, target, party, zone in casts:
        # the cast's own line carries the target's health before the heal
        while i < len(events) and events[i][0] <= t:
            if events[i][1] == "hp":
                hp[events[i][2]] = events[i][3]
            i += 1
        name, window, group = GCD_HOTS[action]
        loss = defaultdict(int)
        j = i
        while j < len(events) and events[j][0] <= t + window:
            if events[j][1] == "loss":
                loss[events[j][2]] += events[j][3]
            j += 1
        falls = [m for m in party if m in hp and hp[m][1] and (hp[m][0] - loss[m]) / hp[m][1] < THRESHOLD]
        cell = out[(zone, name)]
        cell[0] += 1
        cell[1] += (len(falls) >= 2) if group else (target in falls)
    return out


def report(path):
    with open(path, encoding="utf-8", errors="replace") as handle:
        casts, events, ticks, dots = read(handle)
    print("GCD heals over time, needed after the fact (falls below 65 % within the HoT's time without heals):")
    for (zone, name), (n, need) in sorted(needed(casts, events).items()):
        print(f"  {zone[:30]:30} {name:20} casts {n:4}  needed {need:4}  not needed {n - need:4}")
    print("HoT ticks on party members, overheal by the player's own HoTs on the member:")
    for (zone, group), (amount, over, n) in sorted(ticks.items()):
        if n:
            print(f"  {zone[:30]:30} {group:22} ticks {n:5}  overheal {over / amount if amount else 0:5.0%}")
    print("DoT ticks on party members (share of max HP):")
    for zone, shares in sorted(dots.items()):
        shares = sorted(shares)
        print(f"  {zone[:30]:30} ticks {len(shares):4}  median {shares[len(shares) // 2]:6.2%}  "
              f"max {shares[-1]:6.2%}  reported as 0: {sum(1 for s in shares if s == 0)}")


def self_test():
    t = "2026-10-27T20:00:{:02d}.0000000+01:00"
    me, tank = "10000001", "10000002"

    def action(sec, act, target, cur, top, flags="", value=""):
        pairs = [flags, value] + [""] * 14
        return f"21|{t.format(sec)}|{me}|x|{act:X}|x|{target}|x|" + "|".join(pairs) + f"|{cur}|{top}|||||||||"

    lines = [f"02|{t.format(0)}|{me}|x", f"01|{t.format(0)}|1|Testzone", f"11|{t.format(0)}|2|{me}|{tank}",
             # Regen on a tank at 90 % with nothing to follow: not needed
             action(1, 137, tank, 9000, 10000),
             # Regen on the tank at 70 %, then 20 % of damage within the window: needed
             action(20, 137, tank, 7000, 10000),
             action(22, 7, tank, 7000, 10000, "3", f"{0x07D00000:X}"),
             # one HoT tick under the player's Regen on full health, one on missing health
             f"26|{t.format(20)}|9E|Regen|18.00|{me}|x|{tank}|x|00|10000|10000",
             f"37|{t.format(23)}|{tank}|x|0|10000|10000",
             f"24|{t.format(23)}|{tank}|x|HoT|0|1F4|10000|10000|0|0|||0|0|0|0|{me}|x|0|0|0|0|0|||0|0|0|0",
             f"37|{t.format(26)}|{tank}|x|0|6000|10000",
             f"24|{t.format(26)}|{tank}|x|HoT|0|1F4|6500|10000|0|0|||0|0|0|0|{me}|x|0|0|0|0|0|||0|0|0|0",
             # two DoT ticks, one swallowed by a barrier
             f"24|{t.format(29)}|{tank}|x|DoT|0|3E8|5000|10000|0|0|||0|0|0|0|40000001|x|0|0|0|0|0|||0|0|0|0",
             f"24|{t.format(32)}|{tank}|x|DoT|0|0|5000|10000|0|0|||0|0|0|0|40000001|x|0|0|0|0|0|||0|0|0|0"]
    casts, events, ticks, dots = read(lines)
    need = needed(casts, events)
    checks = {
        "two Regen casts are read": need[("Testzone", "Regen")][0] == 2,
        "only the one followed by damage counts as needed": need[("Testzone", "Regen")][1] == 1,
        "the tick on full health is overheal, the one on missing health is not":
            ticks[("Testzone", "GCD HoT")][1] == 500 and ticks[("Testzone", "GCD HoT")][2] == 2,
        "a DoT tick swallowed by a barrier is read as 0": sorted(dots["Testzone"]) == [0.0, 0.1],
        "an action amount with the 0x40 byte carries the fourth byte": effect_amount("12344001") == 0x011234,
    }
    failed = [name for name, ok in checks.items() if not ok]
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print(f"self-test ok: {len(checks)} checks")
    return 0


if __name__ == "__main__":
    if "--self-test" in sys.argv:
        sys.exit(self_test())
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(2)
    report(sys.argv[1])
