#!/usr/bin/env python3
"""What every session start and every context compaction needs, printed by the SessionStart hook in
.claude/settings.json (CLAUDE.md, "Prüfpunkte"): the sync state against upstream, the open TODO items by title,
the open issues of the repository - where his other repositories pass on what they found (his rule of 27.10.2026) -
and the newest archive entries. Its output enters the context; the hook caps it at 10,000 characters, so it stays
far below that. After a compaction or a clear it also clears rule_inject.py's record of the rule files it gave this
session, since the context no longer holds them.

  session_brief.py            the brief, with the sync state (fetches origin and upstream) and the issues
  session_brief.py --no-sync  without the network; the CI runs this to see that the brief still parses its sources
"""

import json
import re
import subprocess
import sys
import urllib.request
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from hook_io import read_hook_input  # noqa: E402
from rule_inject import marker_path  # noqa: E402

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


def open_issues(timeout=15):
    """The open issues of origin's repository by number and title; pull requests left out."""
    url = run(["git", "remote", "get-url", "origin"], 10)[0]
    slug = "/".join(re.sub(r"\.git$", "", url).rstrip("/").split("/")[-2:])
    try:
        with urllib.request.urlopen(f"https://api.github.com/repos/{slug}/issues?state=open&per_page=30",
                                    timeout=timeout) as response:
            items = json.load(response)
    except Exception as ex:
        return [f"nicht abrufbar ({type(ex).__name__}); mit den GitHub-Werkzeugen lesen"]
    issues = [f"- #{i['number']} {i['title']}" for i in items if "pull_request" not in i]
    return issues or ["- keine"]


def git_guards():
    """The commit guard and the clone depth. .githooks/pre-commit refuses a commit under a foreign name or address,
    but git runs it only with core.hooksPath set; a session that commits without it relies on a flag per commit
    (found unset on 27.10.2026). Setting it is local and changes no commit. Taken from KerbalMissionDirector's
    tools/session_state.py, which reports both."""
    out = []
    hooks = run(["git", "config", "core.hooksPath"], 10)[0]
    if hooks == ".githooks" or not (ROOT / ".githooks").is_dir():
        out.append(f"core.hooksPath: {hooks or 'nicht gesetzt'}")
    else:
        done = subprocess.run(["git", "config", "core.hooksPath", ".githooks"], cwd=ROOT, capture_output=True)
        out.append(f"core.hooksPath war {hooks or 'nicht gesetzt'}"
                   + (", jetzt .githooks: Der Identitätsschutz läuft vor jedem Commit" if done.returncode == 0
                      else ", Setzen fehlgeschlagen: mit -c core.hooksPath=.githooks committen"))
    if run(["git", "rev-parse", "--is-shallow-repository"], 10)[0] == "true":
        out.append("flacher Klon: Aussagen über die Versionsgeschichte erst nach git fetch --unshallow")
    return out


def run(args, timeout):
    try:
        done = subprocess.run(args, cwd=ROOT, capture_output=True, text=True, timeout=timeout)
        return done.stdout.strip().split("\n")
    except (OSError, subprocess.TimeoutExpired) as ex:
        return [f"nicht gelaufen: {ex}"]


def main(argv):
    data = {} if "--no-sync" in argv else (read_hook_input() or {})
    if data.get("source") in ("compact", "clear"):
        marker_path(data.get("session_id"), data.get("scratchpad_dir")).unlink(missing_ok=True)
    lines = ["Sitzungsbrief (SessionStart-Hook, .github/scripts/audit/session_brief.py)"]
    if "--no-sync" not in argv:
        lines.append("Abgleich (check_sync_state.py):")
        lines += [" " + l for l in run([sys.executable, str(AUDIT / "check_sync_state.py")], 120)[-6:]]
        lines += [" " + l for l in git_guards()]
        lines.append("Offene Issues (Weitergaben aus seinen anderen Repositories kommen hier an):")
        lines += [" " + l for l in open_issues()]
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
