#!/usr/bin/env python3
"""What every session start and every context compaction needs, printed by the SessionStart hook in
.claude/settings.json (CLAUDE.md, "Prüfpunkte"): the sync state against upstream, the open TODO items by title,
and the newest archive entries. Its output enters the context; the hook caps it at 10,000 characters, so it stays
far below that.

  session_brief.py            the brief, with the sync state (fetches origin and upstream)
  session_brief.py --no-sync  without the fetch; the CI runs this to see that the brief still parses its sources
"""

import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
AUDIT = ROOT / ".github" / "scripts" / "audit"
CAP = 4000  # well under the hook's 10,000, so a grown TODO cannot push the archive lines out
OPEN_SECTIONS = ("## Defekte", "## Offene Arbeit")


def todo_open(text):
    """The titles of the entries under the TODO's defect and open-work sections, or what those sections say."""
    out, section = [], None
    for line in text.split("\n"):
        if line.startswith("## "):
            section = line if line in OPEN_SECTIONS else None
            if section:
                out.append(section[3:] + ":")
            continue
        if section and line.startswith("### "):
            out.append("- " + line[4:])
        elif section and line.strip() and not line.startswith(("*", "-", " ")) and out[-1].endswith(":"):
            out.append("- " + line.strip())
    return out


def run(args, timeout):
    try:
        done = subprocess.run(args, cwd=ROOT, capture_output=True, text=True, timeout=timeout)
        return done.stdout.strip().split("\n")
    except (OSError, subprocess.TimeoutExpired) as ex:
        return [f"nicht gelaufen: {ex}"]


def main(argv):
    lines = ["Sitzungsbrief (SessionStart-Hook, .github/scripts/audit/session_brief.py)"]
    if "--no-sync" not in argv:
        lines.append("Abgleich (check_sync_state.py):")
        lines += [" " + l for l in run([sys.executable, str(AUDIT / "check_sync_state.py")], 120)[-6:]]
    todo = ROOT / "TODO.md"
    lines.append("TODO.md, offen:")
    lines += [" " + l for l in todo_open(todo.read_text(encoding="utf-8"))] if todo.exists() else [" TODO.md fehlt"]
    lines.append("Archiv, jüngste Einträge (archive.py recent 3; Einträge mit archive.py show lesen):")
    lines += [" " + l for l in run([sys.executable, str(AUDIT / "archive.py"), "recent", "3"], 30)]
    text = "\n".join(lines)
    if len(text) > CAP:
        text = text[:CAP] + "\n... gekürzt; TODO.md und archive.py recent selbst lesen"
    print(text)
    # The CI runs the brief without the fetch: a TODO whose sections it no longer finds must fail there, not go
    # silent at the next session start.
    if "--no-sync" in argv and not any(l.startswith((" Defekte:", " Offene Arbeit:")) for l in lines):
        print("session_brief: no open-work section found in TODO.md - its layout changed")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
