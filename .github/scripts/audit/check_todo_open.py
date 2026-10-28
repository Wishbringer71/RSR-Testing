#!/usr/bin/env python3
"""TODO.md holds open work only. A finished point leaves the file, and its history stands in the archive and the
concept (`.claude/rules/archiv-todo.md`). The order paragraph had grown into a chronicle of finished orders, one
sentence per task - "Seine Aufträge vom 18.10.2026 sind erledigt" down to A415 - and an entry kept the dates of its
measurements (owner's question, 28.10.2026: "warum sind in der todo bereits erledigte dinge aufgelistet?", A436).
This fails on the wording that carries them: finished, built, solved, a history marker "seit A<n>". The preamble above
the first section states the rule itself and is left alone.

  check_todo_open.py              the repository's TODO.md
  check_todo_open.py --self-test  constructed defects, each must be found
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
FINISHED = re.compile(
    r"\b(?:erledigt|abgeschlossen|abgearbeitet|umgesetzt|behoben|aufgelöst|(?:ist|sind|wurde|wurden)\s+gebaut"
    r"|seit\s+A\d+)\b",
    re.IGNORECASE)
FENCE = re.compile(r"^\s*(```|~~~)")


def defects(text):
    """[(line number, wording)] below the first section heading."""
    out, started, fenced = [], False, False
    for number, line in enumerate(text.split("\n"), 1):
        if line.startswith("## "):
            started = True
        if FENCE.match(line):
            fenced = not fenced
            continue
        if not started or fenced:
            continue
        out.extend((number, m.group(0)) for m in FINISHED.finditer(line))
    return out


def self_test():
    head = "# TODO\n\nEin abgeschlossener Punkt verschwindet aus dieser Datei.\n\n"
    good = head + "## Reihenfolge\n\n1. Pyretisch – tötete zweimal.\n\n## Defekte\n\n### X\n*Auflösung:* eine Quelle.\n"
    cases = {
        "open work and the preamble pass": (good, 0),
        "a finished order is found": (good + "Sein Auftrag vom 18.10. ist erledigt (A322).\n", 1),
        "a built point is found": (good + "Flamethrower und Six-sided Star sind gebaut.\n", 1),
        "a solved defect is found": (good + "Der Defekt daraus behoben (A356).\n", 1),
        "a settled debt is found": (good + "Drei Schuldpunkte aufgelöst.\n", 1),
        "a history marker is found": (good + "Der Fall ist seit A327 gelesen.\n", 1),
        "a code block is left alone": (good + "```\nerledigt\n```\n", 0),
    }
    failed = [f"{name}: {defects(text)}" for name, (text, n) in cases.items() if len(defects(text)) != n]
    if failed:
        print("self-test FAILED:\n  " + "\n  ".join(failed))
        return 1
    print(f"self-test ok: {len(cases)} cases - finished, built, solved, settled and history found; preamble and code"
          " left alone")
    return 0


def main(argv):
    if "--self-test" in argv:
        return self_test()
    if self_test() != 0:
        return 1
    found = defects((ROOT / "TODO.md").read_text(encoding="utf-8"))
    for number, wording in found:
        print(f"TODO.md:{number}: \"{wording}\" - a finished point or its history; the archive and the concept carry it")
    if found:
        print(f"{len(found)} finding(s): TODO.md holds open work only.")
        return 1
    print("TODO.md holds open work only.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
