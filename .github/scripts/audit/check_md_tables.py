#!/usr/bin/env python3
"""Markdown tables in every tracked .md file. A table starts with its header row and its delimiter row; rows after
a blank line, or rows left behind when a table was rewritten, have neither, and GitHub shows them as plain text -
the archive's table of revocations rendered from C60 on as running text that way (A422). A delimiter row without
its leading pipe (`---|---|`) is the remnant of a table whose header was cut (found in KerbalMissionDirector,
27.10.2026). This fails on both.

  check_md_tables.py              the repository
  check_md_tables.py --self-test  constructed defects, each must be found
"""

import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
DELIMITER = re.compile(r"^\|?\s*:?-+:?\s*(\|\s*:?-+:?\s*)*\|?\s*$")
DELIMITER_WITHOUT_PIPE = re.compile(r"^:?-+:?\s*\|")
FENCE = re.compile(r"^\s*(```|~~~)")


def defects(text):
    out, block, fenced = [], [], False

    def close():
        if block and (len(block) < 2 or not DELIMITER.match(block[1][1])):
            out.append((block[0][0], "table rows without header and delimiter row - a blank or other line split the "
                                     "table, or a rewrite left them behind"))
        block.clear()

    for number, line in enumerate(text.split("\n"), 1):
        if FENCE.match(line):
            close()
            fenced = not fenced
            continue
        if fenced:
            continue
        stripped = line.strip()
        if DELIMITER_WITHOUT_PIPE.match(stripped):
            out.append((number, "delimiter row without its leading pipe - the remnant of a table whose header was cut"))
        if stripped.startswith("|"):
            block.append((number, stripped))
        else:
            close()
    close()
    return out


def tracked_markdown(root):
    out = subprocess.run(["git", "ls-files", "*.md"], cwd=root, capture_output=True, text=True, check=True).stdout
    return [f for f in out.split("\n") if f]


def self_test():
    good = "# T\n\n| A | B |\n|---|---|\n| 1 | 2 |\n| 3 | 4 |\n\nText.\n"
    cases = {
        "a whole table passes": (good, 0),
        "a blank line inside a table is found": (good.replace("| 1 | 2 |\n", "| 1 | 2 |\n\n"), 1),
        "rows without header are found": ("Text.\n\n| 1 | 2 |\n| 3 | 4 |\n", 1),
        "a delimiter row without its pipe is found": ("Text.\n---|---|\n| alt | Wert |\n", 2),
        "a table in a code block is left alone": ("```\n| 1 | 2 |\n```\n", 0),
        "a thematic break and front matter pass": ("---\npaths:\n---\n\nText.\n\n---\n", 0),
        "an aligned delimiter row passes": ("| A | B |\n|:--|--:|\n| 1 | 2 |\n", 0),
    }
    failed = [f"{name}: {defects(text)}" for name, (text, n) in cases.items() if len(defects(text)) != n]
    if failed:
        print("self-test FAILED:\n  " + "\n  ".join(failed))
        return 1
    print(f"self-test ok: {len(cases)} cases - split table, rows without header, cut header, code and breaks left alone")
    return 0


def main(argv):
    if "--self-test" in argv:
        return self_test()
    if self_test() != 0:
        return 1
    files = tracked_markdown(ROOT)
    found = [(rel, n, why) for rel in files for n, why in defects((ROOT / rel).read_text(encoding="utf-8"))]
    print(f"{len(files)} Markdown file(s), {len(found)} table defect(s)")
    for rel, n, why in found:
        print(f"  {rel}:{n}: {why}")
    return 1 if found else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
