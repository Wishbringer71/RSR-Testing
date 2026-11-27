#!/usr/bin/env python3
"""Action ids for a search in his recordings, taken from the action matrix, never from memory (A940, A949).

Usage:
  python3 .github/scripts/audit/action_ids.py <JOB> [name ...]   ids, hex and identifier of the named actions of a job
  python3 .github/scripts/audit/action_ids.py <JOB>              every action of the job
  python3 .github/scripts/audit/action_ids.py --self-test

The matrix (docs/action-matrix/<JOB>.md, generate_action_matrix.py) is generated from the source generator's action
sheet, so its ids are the game's. A name matches case-insensitively as a whole or as the identifier; a name that matches
nothing is reported and the exit code is 1, so a search never runs with a missing id. The ACT log writes action ids in
upper-case hex without leading zeros (line 21/22, field 5).
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
MATRIX = ROOT / "docs" / "action-matrix"
ROW = re.compile(r"^\| [^|]+ \| (?P<name>.+?) \(`(?P<ident>\w+)`\) \| (?P<id>\d+) \| (?P<kind>\w+) \|")


def rows(text):
    """(name, identifier, id, kind) of every action row of a matrix page."""
    found = []
    for line in text.splitlines():
        m = ROW.match(line)
        if m:
            found.append((m["name"], m["ident"], int(m["id"]), m["kind"]))
    return found


def lookup(table, names):
    """The rows each name matches, and the names that match none."""
    hits, missing = [], []
    for name in names:
        key = name.lower()
        matched = [r for r in table if r[0].lower() == key or r[1].lower() == key]
        if matched:
            hits.extend(matched)
        else:
            missing.append(name)
    return hits, missing


def self_test():
    page = ("| Stufe | Aktion | Id | Art | Nutzung |\n|---|---|---|---|---|\n"
            "| Job | Goka Mekkyaku (`GokaMekkyakuPvE`) | 16491 | Ability | direkt |\n"
            "| Job | Ten (`TenPvE`) | 2259 | Ability | direkt |\n"
            "| Job | Ten (`TenPvE_18805`) | 18805 | Ability | direkt |\n"
            "| Goka Mekkyaku | braucht Kassatsu | Kassatsu |\n")
    table = rows(page)
    assert len(table) == 3, f"three action rows, the edge row left out: {table}"
    hits, missing = lookup(table, ["goka mekkyaku", "TenPvE_18805", "Hyosho Ranryu"])
    assert [h[2] for h in hits] == [16491, 18805], f"name and identifier match: {hits}"
    assert missing == ["Hyosho Ranryu"], "a name the matrix lacks is reported, not guessed"
    hits, _ = lookup(table, ["Ten"])
    assert [h[2] for h in hits] == [2259, 18805], "every action of the same name is listed"
    assert format(16491, "X") == "406B", "hex as the ACT log writes it"
    real = MATRIX / "NIN.md"
    if real.exists():
        hits, missing = lookup(rows(real.read_text(encoding="utf-8")), ["Goka Mekkyaku"])
        assert not missing and hits[0][2] == 16491, f"the shipped matrix reads: {hits}"
    print("self-test ok: rows read, edge rows left out, names and identifiers matched, a missing name reported")


def main(argv):
    if argv[1:2] == ["--self-test"]:
        self_test()
        return 0
    if len(argv) < 2:
        print(__doc__)
        return 2
    job = argv[1].upper()
    page = MATRIX / f"{job}.md"
    if not page.exists():
        print(f"no action matrix for {job}: {page}")
        return 1
    table = rows(page.read_text(encoding="utf-8"))
    hits, missing = lookup(table, argv[2:]) if len(argv) > 2 else (table, [])
    for name, ident, aid, kind in hits:
        print(f"{aid}\t{aid:X}\t{ident}\t{name}\t{kind}")
    for name in missing:
        print(f"NOT IN THE MATRIX: {name}")
    return 1 if missing else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
