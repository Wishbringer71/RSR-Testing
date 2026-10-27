#!/usr/bin/env python3
"""PreToolUse hook for Bash (.claude/settings.json). The rule files under .claude/rules load when a matching file is
opened with Read, Edit or Write; a shell command that reads or changes the same file loads nothing (measured
27.10.2026, A423: `head` on a file under RotationSolver.Basic left code.md out, Read on it brought it in). A change made
by a script would so never see the rule of its occasion. This hook gives a rule file to the context next to the result
of the first Bash command in a session that names a path its `paths:` match, once per session. Read, Edit and Write
load a rule file themselves; the hook runs on them too and only records what they loaded, so a rule already in the
context does not come a second time. A compaction drops what was loaded, so session_brief.py clears the record then.

  rule_inject.py              the hook: reads the hook input on stdin, prints its answer
  rule_inject.py --self-test  constructed commands against a constructed rule tree
"""

import json
import re
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_rule_files import frontmatter, glob_regex, paths_of  # noqa: E402

ROOT = Path(__file__).resolve().parents[3]
BUDGET = 9900  # the hook's text is capped at 10,000 characters; past that it lands in a file Claude is not asked to read
HEADER_ROOM = 300  # the sentence before a rule's text, with the path the command named
PATH_TOKEN = re.compile(r"[\w.\-/*]+")


def marker_path(session_id, scratch=None):
    base = Path(scratch) if scratch else Path(tempfile.gettempdir())
    safe = re.sub(r"[^\w-]", "_", session_id or "none")
    return base / f"rule_inject_{safe}.json"


def rules(root):
    out = []
    for path in sorted((root / ".claude" / "rules").glob("*.md")):
        text = path.read_text(encoding="utf-8")
        fm = frontmatter(text)
        if fm is None:
            continue
        body = text[text.find("\n---", 4) + 4:].lstrip("\n")
        out.append((str(path.relative_to(root)), [glob_regex(g) for g in paths_of(fm)], body))
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


def answer(command, root, done):
    """The text for the context and the rule files it carries whole; None when nothing new matches."""
    tokens = named_paths(command, root)
    matched = []
    for rel, globs, body in rules(root):
        if rel in done:
            continue
        hit = next((t for t in tokens if any(g.match(t) for g in globs)), None)
        if hit:
            matched.append((rel, hit.rstrip("/"), body))
    if not matched:
        return None, []
    parts, carried, size = [], [], 0
    for rel, hit, body in matched:
        whole = (f"Regeldatei {rel} gilt für diesen Befehl: Er nennt `{hit}`, und die Shell lädt Regeldateien nicht. "
                 f"Ihr Inhalt:\n\n{body}")
        if size + len(whole) <= BUDGET:
            parts.append(whole)
            carried.append(rel)
            size += len(whole)
        else:
            parts.append(f"Regeldatei {rel} gilt ebenfalls (der Befehl nennt `{hit}`); sie passt nicht mehr in diesen "
                         f"Hinweis und steht erst nach dem Öffnen mit Read im Kontext.")
    return "\n\n".join(parts), carried


FILE_TOOLS = ("Read", "Edit", "Write", "MultiEdit", "NotebookEdit")


def hook(data, root):
    tool = data.get("tool_name")
    if tool != "Bash" and tool not in FILE_TOOLS:
        return None
    marker = marker_path(data.get("session_id"), data.get("scratchpad_dir"))
    try:
        done = set(json.loads(marker.read_text(encoding="utf-8")))
    except (OSError, ValueError):
        done = set()
    if tool in FILE_TOOLS:
        target = data.get("tool_input", {}).get("file_path") or data.get("tool_input", {}).get("notebook_path") or ""
        loaded = {rel for rel, globs, _ in rules(root) for t in named_paths(target, root) if any(g.match(t) for g in globs)}
        if loaded - done:
            marker.write_text(json.dumps(sorted(done | loaded)), encoding="utf-8")
        return None
    text, carried = answer(data.get("tool_input", {}).get("command", ""), root, done)
    if not text:
        return None
    if carried:
        marker.write_text(json.dumps(sorted(done | set(carried))), encoding="utf-8")
    return {"hookSpecificOutput": {"hookEventName": "PreToolUse", "additionalContext": text}}


def self_test():
    failed = []
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        (root / ".claude" / "rules").mkdir(parents=True)
        (root / ".claude" / "rules" / "code.md").write_text(
            '---\npaths:\n  - "Src*/**"\n---\n\n# Code\n\n- Regel für Code.\n', encoding="utf-8")
        (root / ".claude" / "rules" / "log.md").write_text(
            '---\npaths:\n  - "LOG.md"\n---\n\n# Log\n\n- Regel für das Log.\n', encoding="utf-8")
        (root / ".claude" / "rules" / "big.md").write_text(
            '---\npaths:\n  - "big/**"\n---\n\n' + "x" * (BUDGET - 150) + "\n", encoding="utf-8")
        (root / ".claude" / "rules" / "huge.md").write_text(
            '---\npaths:\n  - "huge/**"\n---\n\n' + "y" * (BUDGET + 10) + "\n", encoding="utf-8")
        scratch = root / "scratch"
        scratch.mkdir()

        def run(command, session="s1"):
            return hook({"tool_name": "Bash", "session_id": session, "scratchpad_dir": str(scratch),
                         "tool_input": {"command": command}}, root)

        first = run("head -3 SrcBasic/Actions/A.cs")
        checks = {
            "a command naming a code file carries the code rule, without its frontmatter":
                first and "- Regel für Code." in first["hookSpecificOutput"]["additionalContext"]
                and "paths:" not in first["hookSpecificOutput"]["additionalContext"],
            "the same rule is not carried twice in one session": run("sed -n 1p SrcBasic/B.cs") is None,
            "another session gets it again": run("cat SrcBasic/B.cs", session="s2") is not None,
            "a path inside a script is found": run("python3 - <<'EOF'\np = Path('LOG.md')\nEOF") is not None,
            "an absolute path and a bare directory are found":
                run(f"ls {root}/SrcOther", session="s3") is not None and run("ls SrcX", session="s4") is not None,
            "a command naming nothing carries nothing": run("git status --short", session="s5") is None,
            "another tool is left alone": hook({"tool_name": "Grep", "session_id": "s6", "tool_input": {}}, root) is None,
            "a rule Read loaded does not come again with the shell": hook(
                {"tool_name": "Read", "session_id": "s9", "scratchpad_dir": str(scratch),
                 "tool_input": {"file_path": f"{root}/SrcBasic/A.cs"}}, root) is None
                and run("cat SrcBasic/A.cs", session="s9") is None
                and run("cat LOG.md", session="s9") is not None,
        }
        three = run("wc -l big/a LOG.md SrcBasic/c", session="s7")["hookSpecificOutput"]["additionalContext"]
        checks["rules past the budget are named, not cut, and come whole with the next command"] = (
            "x" * 50 in three and "passt nicht mehr" in three and "- Regel für Code." not in three
            and "- Regel für Code." in run("cat SrcBasic/c", session="s7")["hookSpecificOutput"]["additionalContext"])
        checks["a rule larger than the budget alone is only named"] = all(
            "passt nicht mehr" in r["hookSpecificOutput"]["additionalContext"] and "y" * 50 not in r["hookSpecificOutput"]["additionalContext"]
            for r in (run("cat huge/a", session="s8"), run("cat huge/b", session="s8")))
        marker_path("s1", scratch).unlink()
        checks["a cleared record carries the rule again (after a compaction)"] = run("cat SrcBasic/B.cs") is not None
        failed = [name for name, ok in checks.items() if not ok]
    # The repository's own rule files: one that no longer fits the hook alone would reach a shell command only by name.
    for rel, _, body in rules(ROOT):
        if len(body) + HEADER_ROOM > BUDGET:
            failed.append(f"{rel} is larger than the hook carries ({BUDGET} characters) - split it by occasion")
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print(f"self-test ok: {len(checks)} cases - rule carried once per session, paths in scripts, budget, reset")
    return 0


def main(argv):
    if "--self-test" in argv:
        return self_test()
    try:
        out = hook(json.load(sys.stdin), ROOT)
    except Exception as ex:  # a broken hook must not stop the command; exit 1 shows the error without blocking
        print(f"rule_inject.py: {type(ex).__name__}: {ex}", file=sys.stderr)
        return 1
    if out:
        print(json.dumps(out, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
