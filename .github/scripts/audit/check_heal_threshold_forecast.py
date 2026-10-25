#!/usr/bin/env python3
"""Check that healing thresholds in the default rotations read the forecast health.

"Heal ahead of incoming damage" says in its own text: "Every healing threshold and the heal target
choice read the health a member is heading for". The central gates did (A93, A182), the job
rotations' own thresholds did not - Regen, Essential Dignity, Taurochole, Excogitation, Clemency,
Second Wind and the rest compared the health shown now (A191). With the setting off the forecast is
exactly the plain value, so the two spellings behave the same for anyone who has not switched it
on, and compile the same - which is why nothing but a check keeps a new threshold from slipping
back.

What counts as a healing threshold: a comparison of `GetHealthRatio()` - or of the party's plain
statistics, `PartyMembersAverHP`, `PartyMembersMinHP`, `LowestPartyMembersAverHP`, `PartyMembersDifferHP` - with
a setting whose name says it is one - it contains "Heal" or names a heal (the list below), or is one of
the global single/area/self/healer/tank heal ratios. The party statistics were missed until A358: seven
thresholds on the party's average or minimum (Horoscope, Lady of Crowns, Holos, Pneuma, the Scholar's
emergency and Recitation, the downtime heal) read the level while their single-target neighbours read the
forecast; `PartyMembersForecastAverHP` and `DataCenter.ForecastMinHp` are the forecast spellings. Mitigation thresholds (dying tank, Intervention, Cover,
The Blackest Night) are not healing thresholds and are left alone, and so are Duty and PvP
rotations and the ones under ExtraRotations, which are not the fork's to edit.

Fails on a finding. The self-test runs first.
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
SCAN = [
    ROOT / "RotationSolver" / "RebornRotations",
    ROOT / "RotationSolver.Basic" / "Rotations",
]
SKIP_PARTS = {"PVPRotations", "Duty", "ExtraRotations"}

PARTY = r"\b(?:PartyMembersAverHP|PartyMembersMinHP|LowestPartyMembersAverHP|PartyMembersDifferHP)\b"
PLAIN = re.compile(r"\.GetHealthRatio\(\)\s*(?:<=?|>=?)|(?:<=?|>=?)\s*[\w?.]*\.GetHealthRatio\(\)"
                   r"|" + PARTY + r"\s*(?:<=?|>=?)|(?:<=?|>=?)\s*[\w.]*" + PARTY)
HEAL_NAME = re.compile(
    r"\b\w*(?:Heal(?!th)|HealthSingle|HealthArea|HealthSelf|HealthHealer|HealthTank|Excog|Aetherpact|Dignity|"
    r"Synastry|Krasis|Pneuma|Soteria|Benediction|Clemency|Intuition|Equilibrium|ThrillOfBattle|"
    r"EmergencyTactics|synastryHp|healRatio|floorHealthRatio|Indomitability|HealthDifference|Horoscope|Holos)\w*\b")


def findings_in(text):
    """Line numbers and lines where a plain health read meets a healing threshold."""
    found = []
    for number, line in enumerate(text.splitlines(), 1):
        code = line.split("//", 1)[0]
        if PLAIN.search(code) and HEAL_NAME.search(re.sub(PARTY, "", code.replace("GetHealthRatio", ""))):
            found.append((number, line.strip()))
    return found


def self_test():
    bad = "\t\tif (RegenPvE.Target.Target.GetHealthRatio() > RegenHeal)\n"
    good = "\t\tif (RegenPvE.Target.Target.GetForecastHealthRatio() > RegenHeal)\n"
    mitigation = "\t\t&& Player?.GetHealthRatio() <= Service.Config.HealthForDyingTanks)\n"
    comment = "\t\t// was: t.GetHealthRatio() < KrasisHeal\n"
    config = "\t\tsetting.ActionCheck = () => Player?.GetHealthRatio() < Service.Config.HealthSingleAbility;\n"
    party = "\t\tif (PartyMembersAverHP < HoroscopeHeal && HoroscopePvE.CanUse(out act))\n"
    recite = "\t\tif (!IndomitabilityPvE.Cooldown.IsCoolingDown && PartyMembersAverHP <= ReciteIndomitability)\n"
    spread = "\t\t\t\t\t\tif (DataCenter.PartyMembersDifferHP < Service.Config.HealthDifference)\n"
    party_good = "\t\tif (PartyMembersForecastAverHP(true) < HoroscopeHeal && HoroscopePvE.CanUse(out act))\n"
    assert findings_in(bad), "a plain read at a heal threshold must be caught"
    assert findings_in(party), "the party's plain average at a heal threshold must be caught"
    assert findings_in(recite), "Recitation with Indomitability is a healing threshold"
    assert findings_in(spread), "the plain spread weighed with a heal threshold must be caught"
    assert not findings_in(party_good), "the party's forecast average must be accepted"
    assert findings_in(config), "a global heal ratio must be caught"
    assert not findings_in(good), "the forecast read must be accepted"
    assert not findings_in(mitigation), "a mitigation threshold is not a healing threshold"
    assert not findings_in(comment), "a comment is not code"
    print("self-test ok: a plain read at a heal threshold is caught, the party's plain average, minimum and spread "
          "and a global heal ratio too; the forecast reads, a mitigation threshold and a comment are left alone")


def main():
    self_test()
    total = 0
    for base in SCAN:
        for path in sorted(base.rglob("*.cs")):
            if SKIP_PARTS & set(path.relative_to(ROOT).parts):
                continue
            for number, line in findings_in(path.read_text(encoding="utf-8-sig")):
                print(f"{path.relative_to(ROOT)}:{number}: {line}")
                total += 1
    if total:
        print(f"\n{total} healing threshold(s) read the health shown now, not the health a member is heading "
              "for. Use GetForecastHealthRatio(), for the party PartyMembersForecastAverHP() or DataCenter.ForecastMinHp() "
              "and ComputeForecastAreaStats(): with the setting off each is the same value.")
        return 1
    print("Every healing threshold in the default rotations reads the forecast health.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
