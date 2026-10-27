#!/usr/bin/env python3
"""The rule files against CLAUDE.md's reading table ("Was wann gilt").

CLAUDE.md holds what applies in every situation and loads whole into every session; the rest lies once in
.claude/rules (loaded when a matching file is touched) and .claude/skills (loaded when invoked), each named in the
table with its occasion (the owner's rule of 27.10.2026: in the repository, editable, no copies). What can go wrong
is silent: a rule file the table does not name is read by chance only; a path rule whose globs match nothing never
loads; a rule file without `paths:` loads every session and saves nothing; a rule standing in two files ages in one
of them; a skill without its name or description is never offered. This fails on each, and on a hook of any event
that points at a script that does not exist.

  check_rule_files.py              the repository
  check_rule_files.py --self-test  constructed defects, each must be found
"""

import json
import re
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
SKILL_LISTING_LIMIT = 1536  # Claude Code truncates description plus when_to_use at this length in the listing
MIN_DUPLICATE = 40  # shorter lines are table rows and headings that may repeat by nature


def glob_regex(pattern):
    out, i = "", 0
    while i < len(pattern):
        if pattern.startswith("**/", i):
            out, i = out + "(?:.*/)?", i + 3
        elif pattern.startswith("**", i):
            out, i = out + ".*", i + 2
        elif pattern[i] == "*":
            out, i = out + "[^/]*", i + 1
        elif pattern[i] == "?":
            out, i = out + "[^/]", i + 1
        else:
            out, i = out + re.escape(pattern[i]), i + 1
    return re.compile(out + r"\Z")


def frontmatter(text):
    if not text.startswith("---\n"):
        return None
    end = text.find("\n---", 4)
    return text[4:end] if end > 0 else None


def paths_of(fm):
    found, inside = [], False
    for line in fm.split("\n"):
        if line.startswith("paths:"):
            inside = True
            rest = line[len("paths:"):].strip()
            if rest:
                found += [p.strip().strip('"\'') for p in rest.split(",") if p.strip()]
            continue
        if inside and line.startswith("  - "):
            found.append(line[4:].strip().strip('"\''))
        elif inside and line and not line.startswith(" "):
            inside = False
    return found


def field(fm, name):
    m = re.search(rf"^{name}:\s*(.*)$", fm, re.M)
    return m.group(1).strip() if m else ""


def table_section(claude):
    m = re.search(r"^# Was wann gilt\n(.*?)(?=^# )", claude, re.M | re.S)
    return m.group(1) if m else ""


def bullets(text):
    return [l.strip() for l in text.split("\n") if l.lstrip().startswith("- ") and len(l.strip()) >= MIN_DUPLICATE]


def check(root, tracked):
    errors = []
    claude_path = root / "CLAUDE.md"
    claude = claude_path.read_text(encoding="utf-8")
    table = table_section(claude)
    if not table:
        return ["CLAUDE.md: no section '# Was wann gilt' - the reading table is the only way the rule files are found"]
    named = set(re.findall(r"`(\.claude/[^`]+)`", table))
    rule_files = sorted(str(p.relative_to(root)) for p in (root / ".claude" / "rules").rglob("*.md")) \
        if (root / ".claude" / "rules").exists() else []
    skill_files = sorted(str(p.relative_to(root)) for p in (root / ".claude" / "skills").glob("*/SKILL.md")) \
        if (root / ".claude" / "skills").exists() else []
    for rel in sorted(named):
        if not (root / rel).exists():
            errors.append(f"CLAUDE.md names {rel}, which does not exist")
    for rel in rule_files + skill_files:
        if rel not in named:
            errors.append(f"{rel} is not named in CLAUDE.md's reading table")
    for rel in rule_files:
        fm = frontmatter((root / rel).read_text(encoding="utf-8"))
        globs = paths_of(fm) if fm is not None else []
        if not globs:
            errors.append(f"{rel}: no 'paths:' - it loads every session like CLAUDE.md and belongs there")
        for g in globs:
            if not any(glob_regex(g).match(f) for f in tracked):
                errors.append(f"{rel}: path '{g}' matches no file in the repository, so the rule never loads")
    for rel in skill_files:
        fm = frontmatter((root / rel).read_text(encoding="utf-8"))
        name, desc = (field(fm, "name"), field(fm, "description")) if fm is not None else ("", "")
        if name != Path(rel).parent.name:
            errors.append(f"{rel}: name '{name}' is not its directory '{Path(rel).parent.name}'")
        if not desc:
            errors.append(f"{rel}: no description - the skill is never offered")
        elif len(desc) + len(field(fm, "when_to_use")) > SKILL_LISTING_LIMIT:
            errors.append(f"{rel}: description longer than the listing shows ({SKILL_LISTING_LIMIT} characters)")
    seen = {}
    for rel in ["CLAUDE.md"] + rule_files + skill_files:
        for b in bullets((root / rel).read_text(encoding="utf-8")):
            if b in seen and seen[b] != rel:
                errors.append(f"the same rule stands in {seen[b]} and {rel}: {b[:80]}")
            seen.setdefault(b, rel)
    settings = root / ".claude" / "settings.json"
    if settings.exists():
        try:
            hooks = json.loads(settings.read_text(encoding="utf-8")).get("hooks", {})
        except json.JSONDecodeError as ex:
            return errors + [f".claude/settings.json is not valid JSON: {ex}"]
        for event, groups in hooks.items():
            for group in groups:
                for hook in group.get("hooks", []):
                    for script in re.findall(r"([\w./-]+\.py)", hook.get("command", "")):
                        rel = script.split("}/", 1)[-1].lstrip("/")
                        if not (root / rel).exists():
                            errors.append(f".claude/settings.json: the {event} hook runs {rel}, which does not exist")
    return errors


def tracked_files(root):
    out = subprocess.run(["git", "ls-files"], cwd=root, capture_output=True, text=True, check=True).stdout.split("\n")
    untracked = subprocess.run(["git", "ls-files", "--others", "--exclude-standard"], cwd=root, capture_output=True,
                               text=True, check=True).stdout.split("\n")
    return [f for f in out + untracked if f]


def self_test():
    good_claude = "# REGEL\n\n# Was wann gilt\n\n| `.claude/rules/a.md` | `.claude/skills/s/SKILL.md` |\n\n# Prüfpunkte\n\n- Eine Regel, die nur hier steht, lang genug für den Vergleich.\n"
    good_rule = "---\npaths:\n  - \"src/**\"\n---\n\n# A\n\n- Eine andere Regel, die nur in der Regeldatei steht, lang genug.\n"
    good_skill = "---\nname: s\ndescription: Ein Ablauf.\n---\n\n# S\n"
    files = ["src/x/y.cs", "CLAUDE.md"]

    def tree(tmp, claude=good_claude, rule=good_rule, skill=good_skill, extra=None, settings=None):
        root = Path(tmp)
        (root / ".claude" / "rules").mkdir(parents=True)
        (root / ".claude" / "skills" / "s").mkdir(parents=True)
        (root / "CLAUDE.md").write_text(claude, encoding="utf-8")
        (root / ".claude" / "rules" / "a.md").write_text(rule, encoding="utf-8")
        (root / ".claude" / "skills" / "s" / "SKILL.md").write_text(skill, encoding="utf-8")
        for rel, text in (extra or {}).items():
            (root / rel).write_text(text, encoding="utf-8")
        if settings is not None:
            (root / ".claude" / "settings.json").write_text(settings, encoding="utf-8")
        return root

    cases = {
        "clean tree passes": (dict(), 0),
        "rule file not in the table": (dict(extra={".claude/rules/b.md": good_rule.replace("andere", "dritte")}), 1),
        "table names a missing file": (dict(claude=good_claude.replace("a.md", "z.md")), 2),
        "rule file without paths": (dict(rule="# A\n\n- Eine andere Regel, die nur in der Regeldatei steht, lang genug.\n"), 1),
        "path that matches nothing": (dict(rule=good_rule.replace("src/**", "lib/**")), 1),
        "rule standing in two files": (dict(rule=good_rule.replace("Eine andere Regel, die nur in der Regeldatei steht, lang genug.",
                                                                   "Eine Regel, die nur hier steht, lang genug für den Vergleich.")), 1),
        "skill without description": (dict(skill="---\nname: s\n---\n"), 1),
        "skill named unlike its directory": (dict(skill=good_skill.replace("name: s", "name: t")), 1),
        "hook runs a missing script": (dict(settings='{"hooks": {"SessionStart": [{"hooks": [{"type": "command", "command": "python3 x/missing.py"}]}]}}'), 1),
        "stop hook runs a missing script": (dict(settings='{"hooks": {"Stop": [{"hooks": [{"type": "command", "command": "python3 x/gone.py"}]}]}}'), 1),
        "no reading table": (dict(claude="# REGEL\n"), 1),
    }
    failed = []
    for name, (kwargs, expected) in cases.items():
        with tempfile.TemporaryDirectory() as tmp:
            found = check(tree(tmp, **kwargs), files)
            if (len(found) > 0) != (expected > 0) or (expected and len(found) < expected):
                failed.append(f"{name}: expected {expected}, found {found}")
    if failed:
        print("self-test FAILED:\n  " + "\n  ".join(failed))
        return 1
    print(f"self-test ok: {len(cases) - 1} constructed defects found, the clean tree passes")
    return 0


def main(argv):
    if "--self-test" in argv:
        return self_test()
    if self_test() != 0:
        return 1
    errors = check(ROOT, tracked_files(ROOT))
    claude_lines = len((ROOT / "CLAUDE.md").read_text(encoding="utf-8").split("\n"))
    rule_count = len(list((ROOT / ".claude" / "rules").rglob("*.md")))
    skill_count = len(list((ROOT / ".claude" / "skills").glob("*/SKILL.md")))
    print(f"CLAUDE.md {claude_lines} lines; {rule_count} rule file(s), {skill_count} skill(s) in the reading table")
    for e in errors:
        print("  " + e)
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
