#!/usr/bin/env python3
"""Whether the sources RSR's concepts were checked against have moved on since they were last evaluated.

The owner's rule (07.11.2026): "Und immer auch ob Aktualisierungen da sind, welche Nutzen bringen" - with every research
and at the end of every task, the known sources are measured against their remote, and each update is read for what it
brings RSR, as an upstream sync is. source_versions.json holds each source at the commit last evaluated; this script asks
each remote for its default branch's head (git ls-remote, no clone) and names the ones that moved. After an update has
been evaluated, --record moves its entry to the new commit and today's date.

What this checks, stated honestly: that a source has new commits, not that they matter. Reading them is the work.

Usage: python3 .github/scripts/audit/check_source_updates.py [--self-test] [--record <owner/repo> <commit>]
"""
import datetime
import json
import os
import subprocess
import sys
import tempfile
from pathlib import Path

LIST = Path(__file__).with_name("source_versions.json")
TIMEOUT = 30


def remote_head(url):
    """The head commit of the remote's default branch, or None when it cannot be reached."""
    try:
        result = subprocess.run(["git", "ls-remote", url, "HEAD"], capture_output=True, text=True, timeout=TIMEOUT)
    except (subprocess.TimeoutExpired, OSError):
        return None
    if result.returncode != 0 or not result.stdout.strip():
        return None
    return result.stdout.split()[0]


def url_of(entry):
    return entry.get("url") or f"https://github.com/{entry['repo']}.git"


def check(entries):
    """(moved, current, unreachable, broken) as lists of (entry, head)."""
    moved, current, unreachable, broken = [], [], [], []
    for entry in entries:
        commit = entry.get("commit", "")
        if not entry.get("repo") or len(commit) < 7:
            broken.append((entry, None))
            continue
        head = remote_head(url_of(entry))
        if head is None:
            unreachable.append((entry, None))
        elif head.startswith(commit) or commit.startswith(head):
            current.append((entry, head))
        else:
            moved.append((entry, head))
    return moved, current, unreachable, broken


def load(path=LIST):
    return json.loads(path.read_text(encoding="utf-8"))


def record(repo, commit, path=LIST):
    data = load(path)
    for entry in data["sources"]:
        if entry["repo"] == repo:
            entry["commit"] = commit
            entry["date"] = datetime.date.today().isoformat()
            path.write_text(json.dumps(data, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
            return True
    return False


def report(entries):
    moved, current, unreachable, broken = check(entries)
    for entry, head in moved:
        print(f"MOVED       {entry['repo']}: evaluated {entry['commit'][:9]} ({entry['date']}), now {head[:9]} - {entry.get('use', '')}")
    for entry, _ in unreachable:
        print(f"UNREACHABLE {entry['repo']} (evaluated {entry['commit'][:9]}, {entry['date']})")
    for entry, _ in broken:
        print(f"BROKEN      entry {entry}")
    print(f"{len(entries)} source(s): {len(moved)} moved, {len(current)} current, {len(unreachable)} unreachable, {len(broken)} broken.")
    return 1 if broken else 0


def self_test():
    def git(*args, cwd):
        subprocess.run(["git", *args], cwd=cwd, check=True, capture_output=True,
                       env={**os.environ, "GIT_AUTHOR_NAME": "t", "GIT_AUTHOR_EMAIL": "t@t", "GIT_COMMITTER_NAME": "t",
                            "GIT_COMMITTER_EMAIL": "t@t", "GIT_CONFIG_GLOBAL": os.devnull, "GIT_CONFIG_SYSTEM": os.devnull})

    with tempfile.TemporaryDirectory() as tmp:
        repo = Path(tmp) / "source"
        repo.mkdir()
        git("init", "-q", "-b", "main", cwd=repo)
        (repo / "a").write_text("1")
        git("add", "a", cwd=repo)
        git("commit", "-q", "-m", "one", cwd=repo)
        first = subprocess.run(["git", "rev-parse", "HEAD"], cwd=repo, capture_output=True, text=True).stdout.strip()
        (repo / "a").write_text("2")
        git("commit", "-q", "-am", "two", cwd=repo)
        second = subprocess.run(["git", "rev-parse", "HEAD"], cwd=repo, capture_output=True, text=True).stdout.strip()
        url = repo.as_uri()
        entries = [
            {"repo": "t/moved", "url": url, "commit": first[:9], "date": "2026-01-01"},
            {"repo": "t/current", "url": url, "commit": second[:9], "date": "2026-01-01"},
            {"repo": "t/gone", "url": (Path(tmp) / "missing").as_uri(), "commit": first, "date": "2026-01-01"},
            {"repo": "t/broken", "url": url, "commit": "", "date": "2026-01-01"},
        ]
        moved, current, unreachable, broken = check(entries)
        assert [e["repo"] for e, _ in moved] == ["t/moved"], moved
        assert [e["repo"] for e, _ in current] == ["t/current"], current
        assert [e["repo"] for e, _ in unreachable] == ["t/gone"], unreachable
        assert [e["repo"] for e, _ in broken] == ["t/broken"], broken
        listing = Path(tmp) / "list.json"
        listing.write_text(json.dumps({"sources": entries[:1]}), encoding="utf-8")
        assert record("t/moved", second, listing) and load(listing)["sources"][0]["commit"] == second
        assert not record("t/none", second, listing)

    data = load()
    for entry in data["sources"]:
        assert entry.get("repo") and len(entry.get("commit", "")) >= 7 and entry.get("date") and entry.get("use"), entry
    print(f"self-test ok: moved, current, unreachable and broken entries are told apart, --record moves an entry; "
          f"{len(data['sources'])} listed source(s) are well formed.")


if __name__ == "__main__":
    if "--self-test" in sys.argv:
        self_test()
    elif "--record" in sys.argv:
        i = sys.argv.index("--record")
        if not record(sys.argv[i + 1], sys.argv[i + 2]):
            sys.exit(f"{sys.argv[i + 1]} is not listed in {LIST.name}")
    else:
        sys.exit(report(load()["sources"]))
