#!/usr/bin/env python3
"""PreToolUse hook for Edit, Write, MultiEdit, NotebookEdit and Bash (.claude/settings.json): the concept comes before the code.

His rule (29.10.2026, CLAUDE.md "Definition of Ready"): an existing concept is extended first and the code built on it;
code first and the concept adjusted afterwards is trial and error and forbidden. The rule stood in the text and was
broken twice all the same (his reproach of 06.11.2026, A635: the potion switch coded, its concept written after). So a
change to a file under RotationSolver*/ is refused while no change under docs/ is open in the working tree: the concept
change has to exist before the first code edit of a commit. It cannot tell whether the open concept change belongs to
this code change; it catches the order that went wrong, code without any concept written first.

A shell command counts as a code change when it names a path under RotationSolver*/ and carries a write form (sed -i,
perl -i, tee, a redirection, open(..., "w"), write_text, git apply, patch): on 19.11.2026 a Python script in Bash wrote
four fixes into code before their concept, and this hook, then only for Edit and Write, did not see it (A910).

Not refused: a merge in progress (an upstream merge resolves conflicts in code, the concept follows in the evaluation of
the sync), and paths outside RotationSolver*/ or under bin/ and obj/.

  concept_first.py              the hook: reads the hook input on stdin, exit 2 with the reason refuses the call
  concept_first.py --self-test  constructed calls against a constructed repository
"""

import re
import subprocess
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from hook_io import read_hook_input  # noqa: E402

ROOT = Path(__file__).resolve().parents[3]
TOOLS = ("Edit", "Write", "MultiEdit", "NotebookEdit")
PATH_TOKEN = re.compile(r"[\w.\-/]+")
WRITE_FORM = re.compile(r"\bsed\s+(-\w+\s+)*-i|\bperl\s+(-\w+\s+)*-\w*i|\btee\b|"
                        r"\bopen\([^)]*['\"][wa]\+?['\"]|write_text\(|write_bytes\(|\bgit\s+apply\b|\bpatch\s")
# A redirection writes only its own target; "=>" in a searched pattern or "> /tmp/x" after a reading command is none.
REDIRECT = re.compile(r"(?<![<>&\d=-])>>?(?!&)\s*(?P<target>[\w.\-/]+)")
REASON = ("Concept first (CLAUDE.md, 'Definition of Ready', his rule of 29.10.2026): no change under docs/ is open, "
          "so this code change would come before its concept. Write the concept part under docs/rotation-flow/ "
          "first - the loop, the decision, what changes in a fight - then the code.")


def code_path(file_path, root):
    """The path relative to the repository when it is code under RotationSolver*/, else None."""
    try:
        rel = Path(file_path).resolve().relative_to(root.resolve())
    except ValueError:
        return None
    parts = rel.parts
    if len(parts) < 2 or not parts[0].startswith("RotationSolver") or "bin" in parts or "obj" in parts:
        return None
    return rel


def concept_open(root):
    """Whether the working tree holds a change under docs/, staged, unstaged or untracked."""
    out = subprocess.run(["git", "-C", str(root), "status", "--porcelain", "--", "docs/"],
                         capture_output=True, text=True, check=False)
    return bool(out.stdout.strip())


def merging(root):
    """Whether a merge is in progress (MERGE_HEAD in the repository's git directory)."""
    out = subprocess.run(["git", "-C", str(root), "rev-parse", "--git-path", "MERGE_HEAD"], capture_output=True,
                         text=True, check=False).stdout.strip()
    if not out:
        return False
    path = Path(out)
    return (path if path.is_absolute() else Path(root) / path).exists()


def shell_writes_code(command, root):
    """Whether a shell command names a code path under RotationSolver*/ and carries a write form, or redirects into one."""
    prefix = str(root) + "/"

    def is_code(token):
        token = token[len(prefix):] if token.startswith(prefix) else token
        return not token.startswith("/") and code_path(Path(root) / token, root) is not None and "." in Path(token).name

    if any(is_code(m.group("target")) for m in REDIRECT.finditer(command)):
        return True
    if not WRITE_FORM.search(command):
        return False
    return any(is_code(token) for token in PATH_TOKEN.findall(command))


def decide(data, root):
    """None to let the call through, else the reason to refuse it."""
    if not data:
        return None
    tool_input = data.get("tool_input") or {}
    if data.get("tool_name") == "Bash":
        if not shell_writes_code(tool_input.get("command", ""), root):
            return None
    elif data.get("tool_name") not in TOOLS:
        return None
    else:
        file_path = tool_input.get("file_path") or tool_input.get("notebook_path")
        if not file_path or code_path(file_path, root) is None:
            return None
    if merging(root) or concept_open(root):
        return None
    return REASON


def self_test():
    failed = []
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)

        def git(*args):
            subprocess.run(["git", "-C", str(root), *args], capture_output=True, text=True, check=True)

        git("init", "-q")
        git("config", "user.email", "test@example.invalid")
        git("config", "user.name", "test")
        (root / "docs").mkdir()
        (root / "docs" / "c.md").write_text("concept\n")
        (root / "RotationSolver" / "Basic").mkdir(parents=True)
        code = root / "RotationSolver" / "Basic" / "A.cs"
        code.write_text("class A {}\n")
        git("add", "-A")
        git("commit", "-q", "-m", "base")

        def call(tool, path):
            return decide({"tool_name": tool, "tool_input": {"file_path": str(path)}}, root)

        if call("Edit", code) is None:
            failed.append("a code edit without an open concept change is refused")
        if call("Write", root / "RotationSolver" / "Basic" / "New.cs") is None:
            failed.append("a new code file without an open concept change is refused")
        if call("Edit", root / "RotationSolver" / "bin" / "x.cs") is not None:
            failed.append("build output is not refused")
        if call("Edit", root / "docs" / "c.md") is not None:
            failed.append("a concept edit is never refused")
        if call("Edit", root / "TODO.md") is not None:
            failed.append("a file outside the code is not refused")
        if call("Read", code) is not None:
            failed.append("reading is not refused")

        def shell(command):
            return decide({"tool_name": "Bash", "tool_input": {"command": command}}, root)

        rel = "RotationSolver/Basic/A.cs"
        for command in (f"python3 - <<'EOF'\np='{rel}'\nopen(p,'w').write(t)\nEOF", f"sed -i 's/a/b/' {rel}",
                        f"echo x > {rel}", f"cat x | tee {root}/{rel}", f"perl -pi -e 's/a/b/' {rel}"):
            if shell(command) is None:
                failed.append(f"a shell command writing code is refused: {command!r}")
        for command in (f"grep -n Foo {rel}", f"sed -n 1,20p {rel}", f"grep -n Foo {rel} 2>/dev/null | head",
                        f"git diff --stat -- {rel}", "echo x > /tmp/out.txt", "sed -i 's/a/b/' TODO.md",
                        f"grep -rn 'IsGeneralGCD =>\\|== 58' {rel} | head", f"git diff {rel} > /tmp/d.patch"):
            if shell(command) is not None:
                failed.append(f"a shell command that does not write code goes through: {command!r}")
        (root / "docs" / "c.md").write_text("concept, extended\n")
        if call("Edit", code) is not None:
            failed.append("a code edit after an open concept change goes through")
        git("checkout", "-q", "--", "docs/c.md")
        (root / "docs" / "new.md").write_text("a new concept\n")
        if call("Edit", code) is not None:
            failed.append("an untracked concept file counts as an open change")
        (root / "docs" / "new.md").unlink()
        (Path(tmp) / ".git" / "MERGE_HEAD").write_text("0" * 40 + "\n")
        if call("Edit", code) is not None:
            failed.append("a merge in progress is not refused")
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print("self-test ok: code without an open concept change refused, by Edit, Write and five shell write forms; "
          "concept, build output, other files, reading, reading shell commands (a searched \"=>\", a redirection "
          "elsewhere), an open or new concept change and a merge let through")
    return 0


def main():
    if "--self-test" in sys.argv:
        return self_test()
    reason = decide(read_hook_input(), ROOT)
    if reason is None:
        return 0
    print(reason, file=sys.stderr)
    return 2


if __name__ == "__main__":
    sys.exit(main())
