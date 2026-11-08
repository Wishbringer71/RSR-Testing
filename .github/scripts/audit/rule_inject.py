#!/usr/bin/env python3
"""PreToolUse hook for Bash, Read, Edit and Write (.claude/settings.json).

The rule files under .claude/rules load when Claude Code opens a matching file with Read, Edit or Write - a partial
Read (offset, limit) included - once per stretch between two compactions; a shell command that names the same file
loads nothing (measured 27.10.2026, A423). What Read, Edit, Write or a hook add to the context arrives with the tool's
result, after the call that needed it (A682, reported from KSP-MyContracts, confirmed in this repository's transcript).
A first write - a script, `sed -i`, a Write that creates a file, an Edit after a compaction - would so run without the
rule of its occasion. Until 08.11.2026 this hook carried a rule's text next to a shell command's result; Claude Code
then loaded the same file again on the next Read: in 210 of 300 file-stretches, about 16,000 characters a stretch on
average, more than the split by occasion saves (A682).

So the hook refuses the first Bash, Edit or Write call that touches a path a rule file's `paths:` match while that rule
is not loaded, and names a file of that occasion to open with Read (limit 1). Claude Code then loads the rule once,
before the call is sent again; a call sent again without it is refused again. Read is never refused; the hook records
what it loads, and a Read of a rule file itself counts as that rule. A compaction drops what was loaded, so
session_brief.py clears the record then.

  rule_inject.py              the hook: reads the hook input on stdin; exit 2 with the reason on stderr refuses the call
  rule_inject.py --self-test  constructed calls against a constructed rule tree, and every rule of this repository
"""

import json
import re
import subprocess
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_rule_files import frontmatter, glob_regex, paths_of  # noqa: E402
from hook_io import read_hook_input  # noqa: E402

ROOT = Path(__file__).resolve().parents[3]
PATH_TOKEN = re.compile(r"[\w.\-/*]+")
RULES_DIR = ".claude/rules/"


def marker_path(session_id, scratch=None):
    base = Path(scratch) if scratch else Path(tempfile.gettempdir())
    safe = re.sub(r"[^\w-]", "_", session_id or "none")
    return base / f"rule_inject_{safe}.json"


def rules(root):
    out = []
    for path in sorted((root / ".claude" / "rules").glob("*.md")):
        fm = frontmatter(path.read_text(encoding="utf-8"))
        if fm is not None:
            out.append((str(path.relative_to(root)), [glob_regex(g) for g in paths_of(fm)]))
    return out


def named_paths(command, root):
    """The path-like words of a command, relative to the repository; a bare word also as a directory."""
    prefix = str(root) + "/"
    found = []
    for token in PATH_TOKEN.findall(command):
        token = token[len(prefix):] if token.startswith(prefix) else token
        token = token[2:] if token.startswith("./") else token
        if token and not token.startswith("/"):
            found += [token, token.rstrip("/") + "/"]
    return found


def tracked_files(root):
    try:
        out = subprocess.run(["git", "-C", str(root), "ls-files"], capture_output=True, text=True, timeout=5)
        if out.returncode == 0 and out.stdout.strip():
            return out.stdout.split("\n")
    except (OSError, subprocess.TimeoutExpired):
        pass
    return [str(p.relative_to(root)) for p in root.rglob("*") if p.is_file() and ".git" not in p.parts]


def example_file(root, globs, hit, files=None):
    """A file of the rule's occasion to open with Read: the one the call names, else one under it, else any."""
    if (root / hit).is_file() and any(g.match(hit) for g in globs):
        return hit
    files = tracked_files(root) if files is None else files
    matching = [f for f in files if f and any(g.match(f) for g in globs)]
    under = [f for f in matching if f.startswith(hit.rstrip("/") + "/")]
    return (under or matching or [None])[0]


def answer(touched, root, done):
    """The reason to refuse the call, or None when every rule it touches is loaded."""
    tokens = named_paths(touched, root)
    lines, files = [], None
    for rel, globs in rules(root):
        if rel in done:
            continue
        hit = next((t for t in tokens if any(g.match(t) for g in globs)), None)
        if not hit:
            continue
        if files is None:
            files = tracked_files(root)
        example = example_file(root, globs, hit.rstrip("/"), files) or rel
        lines.append(f"- {rel} (der Aufruf nennt `{hit.rstrip('/')}`): `{example}` mit Read öffnen, limit 1")
    if not lines:
        return None
    return ("rule_inject.py: Dieser Aufruf lief nicht. Er berührt den Anlass einer Regeldatei, die noch nicht im "
            "Kontext steht; ein Hinweis von hier käme erst mit dem Ergebnis, nach dem Aufruf. Claude Code lädt die "
            "Regeldatei, sobald eine Datei ihres Anlasses mit Read geöffnet wird. Zuerst öffnen, dann den Aufruf nach "
            "der Regel prüfen und erneut senden:\n" + "\n".join(lines))


def hook(data, root):
    tool = data.get("tool_name")
    if tool not in ("Bash", "Read", "Edit", "Write", "MultiEdit", "NotebookEdit"):
        return None
    marker = marker_path(data.get("session_id"), data.get("scratchpad_dir"))
    try:
        done = set(json.loads(marker.read_text(encoding="utf-8")))
    except (OSError, ValueError):
        done = set()
    tool_input = data.get("tool_input", {})
    if tool == "Read":
        target = tool_input.get("file_path") or ""
        tokens = named_paths(target, root)
        loaded = {rel for rel, globs in rules(root) for t in tokens if any(g.match(t) for g in globs)}
        loaded |= {t for t in tokens if t.startswith(RULES_DIR) and t.endswith(".md")}
        if loaded - done:
            marker.write_text(json.dumps(sorted(done | loaded)), encoding="utf-8")
        return None
    touched = tool_input.get("command", "") if tool == "Bash" else (
        tool_input.get("file_path") or tool_input.get("notebook_path") or "")
    return answer(touched, root, done)


def self_test():
    failed = []
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        (root / ".claude" / "rules").mkdir(parents=True)
        (root / ".claude" / "rules" / "code.md").write_text(
            '---\npaths:\n  - "Src*/**"\n---\n\n# Code\n\n- Regel für Code.\n', encoding="utf-8")
        (root / ".claude" / "rules" / "log.md").write_text(
            '---\npaths:\n  - "LOG.md"\n---\n\n# Log\n\n- Regel für das Log.\n', encoding="utf-8")
        (root / ".claude" / "rules" / "gone.md").write_text(
            '---\npaths:\n  - "gone/**"\n---\n\n# Weg\n', encoding="utf-8")
        (root / "SrcBasic" / "Actions").mkdir(parents=True)
        (root / "SrcBasic" / "Actions" / "A.cs").write_text("a\n", encoding="utf-8")
        (root / "SrcBasic" / "B.cs").write_text("b\n", encoding="utf-8")
        (root / "LOG.md").write_text("log\n", encoding="utf-8")
        scratch = root / "scratch"
        scratch.mkdir()

        def run(command, session="s1"):
            return hook({"tool_name": "Bash", "session_id": session, "scratchpad_dir": str(scratch),
                         "tool_input": {"command": command}}, root)

        def tool(name, path, session):
            return hook({"tool_name": name, "session_id": session, "scratchpad_dir": str(scratch),
                         "tool_input": {"file_path": f"{root}/{path}"}}, root)

        first = run("head -3 SrcBasic/Actions/A.cs")
        checks = {
            "a first command naming a code file is refused and names that file to open, not the rule's text":
                first is not None and "`SrcBasic/Actions/A.cs` mit Read öffnen" in first and "code.md" in first
                and "- Regel für Code." not in first and "lief nicht" in first,
            "sent again without the Read, it is refused again": run("head -3 SrcBasic/Actions/A.cs") is not None,
            "after the Read it passes": tool("Read", "SrcBasic/Actions/A.cs", "s1") is None
                and run("head -3 SrcBasic/Actions/A.cs") is None and run("sed -n 1p SrcBasic/B.cs") is None,
            "another session is refused again": run("cat SrcBasic/B.cs", session="s2") is not None,
            "a path inside a script is found": run("python3 - <<'EOF'\np = Path('LOG.md')\nEOF", session="s3") is not None,
            "an absolute path is found": run(f"ls {root}/SrcBasic", session="s4") is not None,
            "a directory the call names gives a file under it": "`SrcBasic/" in (run("ls SrcBasic", session="s5") or ""),
            "a command naming nothing passes": run("git status --short", session="s6") is None,
            "another tool is left alone": hook({"tool_name": "Grep", "session_id": "s6", "tool_input": {}}, root) is None,
            "Read is never refused": tool("Read", "LOG.md", "s7") is None and tool("Read", "SrcBasic/B.cs", "s8") is None,
            "a rule a Read loaded refuses nothing, another rule still does":
                run("cat SrcBasic/A.cs", session="s8") is None and run("cat LOG.md", session="s8") is not None,
            "a Write creating a file is refused and names an existing file of its occasion":
                "`SrcBasic/" in (tool("Write", "SrcBasic/New.cs", "s9") or "")
                and "New.cs` mit Read" not in (tool("Write", "SrcBasic/New.cs", "s9") or ""),
            "an Edit after Read passes": tool("Read", "LOG.md", "s10") is None and tool("Edit", "LOG.md", "s10") is None,
            "a Read of the rule file itself counts as that rule":
                tool("Read", ".claude/rules/log.md", "s11") is None and run("cat LOG.md", session="s11") is None,
            "a rule whose occasion has no file names the rule file itself":
                "`.claude/rules/gone.md` mit Read" in (run("ls gone/x", session="s12") or ""),
        }
        marker_path("s10", scratch).unlink()
        checks["an Edit after a cleared record (a compaction) is refused"] = tool("Edit", "LOG.md", "s10") is not None
        failed = [name for name, ok in checks.items() if not ok]
    # The repository's own rule files: each must have a file of its occasion to name.
    files = tracked_files(ROOT)
    for rel, globs in rules(ROOT):
        if example_file(ROOT, globs, "", files) is None:
            failed.append(f"{rel} matches no tracked file - the hook could only name the rule file itself")
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print(f"self-test ok: {len(checks)} cases - first touch refused until a file of its occasion is read (Bash, Write, "
          "Edit after a reset), Read never, paths in scripts and directories; every rule of this repository has a file")
    return 0


def main(argv):
    if "--self-test" in argv:
        return self_test()
    try:
        data = read_hook_input()
        if data is None:  # run by hand without a hook call
            print(__doc__)
            return 0
        reason = hook(data, ROOT)
    except Exception as ex:  # a broken hook must not stop the call; exit 1 shows the error without blocking
        print(f"rule_inject.py: {type(ex).__name__}: {ex}", file=sys.stderr)
        return 1
    if reason:
        print(reason, file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
