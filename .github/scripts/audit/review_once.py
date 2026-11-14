#!/usr/bin/env python3
"""PreToolUse hook for Agent (.claude/settings.json): one independent review round per concept and requirements.

His question (14.11.2026, A815): "Ist sichergestellt, dass du nicht wieder in endlosloops bei prüfrunden endest?" The
rule in CLAUDE.md ("Prüfgrad": one round per concept, a further one only when its requirements change, never to check
one's own fixes) stood in the text, and the text rule before it was misread once already (A812, C201): nine LLM review
rounds on the gaze watch. So the round is counted here, where it starts.

A call of the Agent tool whose prompt asks for a review (review, reviewer, Prüfer, Prüfung, Prüfrunde) must name the
concept it reviews (docs/rotation-flow/<name>.md). Each named concept is counted with a fingerprint of its written
requirements: the table rows below the heading "*Anforderungen ...". A concept with a round recorded for the same
fingerprint is refused; fixes after a review leave the requirements as they were, so checking one's own fixes is
refused. A concept without written requirements is refused too - the round is made against them. The rounds stand in
review_rounds.json next to this script, versioned, so they hold across sessions.

  review_once.py                      the hook: reads the hook input on stdin, exit 2 with the reason refuses the call
  review_once.py --record <concept>   records a round made before this hook (or by hand), with the current fingerprint
  review_once.py --self-test          constructed calls against a constructed repository
"""

import datetime
import hashlib
import json
import re
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from hook_io import read_hook_input  # noqa: E402

ROOT = Path(__file__).resolve().parents[3]
LEDGER = "review_rounds.json"
REVIEW = re.compile(r"\b(review\w*|prüf(er|ung|runde)\w*)\b", re.IGNORECASE)
CONCEPT = re.compile(r"docs/rotation-flow/[\w.-]+\.md")
HEADING = re.compile(r"\*Anforderungen\b")


def requirements(text):
    """The fingerprint of a concept's written requirements: every table row (indented or not) below a heading
    "*Anforderungen ...", up to the blank line after the table; None when the concept has none."""
    rows = []
    collecting = False
    for line in text.splitlines():
        if HEADING.search(line) and not line.lstrip().startswith("|"):
            collecting = True
            continue
        if collecting:
            if not line.strip():
                if rows and rows[-1] != "":
                    rows.append("")
                    collecting = False
                continue
            if line.lstrip().startswith("|"):
                rows.append(line.strip())
    rows = [row for row in rows if row]
    if not rows:
        return None
    return hashlib.sha256("\n".join(rows).encode("utf-8")).hexdigest()[:16]


def load(root):
    path = root / ".github" / "scripts" / "audit" / LEDGER
    return json.loads(path.read_text(encoding="utf-8")) if path.exists() else {}


def save(root, ledger):
    path = root / ".github" / "scripts" / "audit" / LEDGER
    path.write_text(json.dumps(ledger, indent=2, ensure_ascii=False, sort_keys=True) + "\n", encoding="utf-8")


def record(root, concept, ledger):
    fingerprint = requirements((root / concept).read_text(encoding="utf-8"))
    ledger.setdefault(concept, []).append({"requirements": fingerprint, "date": datetime.date.today().isoformat()})
    return fingerprint


def decide(data, root):
    """None to let the call through (and count the round), else the reason to refuse it."""
    if not data or data.get("tool_name") != "Agent":
        return None
    tool_input = data.get("tool_input") or {}
    prompt = f"{tool_input.get('description', '')}\n{tool_input.get('prompt', '')}"
    if not REVIEW.search(prompt):
        return None
    concepts = sorted(set(CONCEPT.findall(prompt)))
    if not concepts:
        return ("Review rounds are counted per concept (CLAUDE.md, 'Prüfgrad'; A815): name the concept under review "
                "as docs/rotation-flow/<name>.md in the prompt.")
    ledger = load(root)
    for concept in concepts:
        path = root / concept
        if not path.exists():
            return f"The concept {concept} named for the review does not exist."
        fingerprint = requirements(path.read_text(encoding="utf-8"))
        if fingerprint is None:
            return (f"{concept} has no written requirements (a table below a heading '*Anforderungen ...'); a review "
                    "round is made against them (CLAUDE.md, 'Prüfgrad').")
        if any(entry.get("requirements") == fingerprint for entry in ledger.get(concept, [])):
            return (f"{concept} already had its review round for these requirements (CLAUDE.md, 'Prüfgrad': one round "
                    "per concept; another only when its requirements change, never to check one's own fixes; A815). "
                    "Work in what the round found, list the rest with its price, or go back to the requirement.")
    for concept in concepts:
        record(root, concept, ledger)
    save(root, ledger)
    return None


def self_test():
    failed = []
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        (root / ".github" / "scripts" / "audit").mkdir(parents=True)
        (root / "docs" / "rotation-flow").mkdir(parents=True)
        concept = root / "docs" / "rotation-flow" / "13-x.md"
        concept.write_text("# X\n\nSeine Anforderungen im Text.\n\n| a | b |\n\n*Anforderungen:*\n\n  | # | Vorgabe |\n"
                           "  |---|---|\n  | B1 | eins |\n\nText.\n",
                           encoding="utf-8")
        (root / "docs" / "rotation-flow" / "20-y.md").write_text("# Y\n\nkein Abschnitt\n", encoding="utf-8")

        def call(prompt, tool="Agent"):
            return decide({"tool_name": tool, "tool_input": {"description": "d", "prompt": prompt}}, root)

        if call("Summarise the file docs/rotation-flow/13-x.md") is not None:
            failed.append("an agent call that is no review goes through")
        if call("Review the gaze watch") is None:
            failed.append("a review without a named concept is refused")
        if call("Independent reviewer of docs/rotation-flow/20-y.md") is None:
            failed.append("a review of a concept without written requirements is refused")
        if call("Independent reviewer of docs/rotation-flow/13-x.md") is not None:
            failed.append("the first round of a concept goes through")
        if call("Unabhängige Prüfung von docs/rotation-flow/13-x.md, siebte Runde") is None:
            failed.append("a second round with unchanged requirements is refused")
        concept.write_text(concept.read_text(encoding="utf-8") + "\nA fix in the text below the table.\n",
                           encoding="utf-8")
        if call("Review docs/rotation-flow/13-x.md after the fixes") is None:
            failed.append("a fix outside the requirements does not open another round")
        concept.write_text(concept.read_text(encoding="utf-8").replace("| B1 | eins |", "| B1 | eins |\n  | B2 | zwei |"),
                           encoding="utf-8")
        if call("Review docs/rotation-flow/13-x.md for the new requirement") is not None:
            failed.append("changed requirements open one more round")
        if call("Review docs/rotation-flow/13-x.md once more") is None:
            failed.append("that round is counted too")
        prose = concept.read_text(encoding="utf-8").replace("| a | b |", "| a | c |")
        concept.write_text(prose, encoding="utf-8")
        if call("Review docs/rotation-flow/13-x.md, a table outside the requirements changed") is None:
            failed.append("a table under prose that names requirements is no requirement")
        if call("Review docs/rotation-flow/13-x.md", tool="Bash") is not None:
            failed.append("another tool is not refused")
        if len(load(root).get("docs/rotation-flow/13-x.md", [])) != 2:
            failed.append("the ledger holds the two rounds made")
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print("self-test ok: a review without a named concept, of a concept without requirements and a second round for "
          "the same requirements refused; other agent calls, the first round and a round after changed requirements "
          "let through and counted")
    return 0


def main():
    if "--self-test" in sys.argv:
        return self_test()
    if "--record" in sys.argv:
        ledger = load(ROOT)
        for concept in sys.argv[sys.argv.index("--record") + 1:]:
            print(f"{concept}: {record(ROOT, concept, ledger)}")
        save(ROOT, ledger)
        return 0
    reason = decide(read_hook_input(), ROOT)
    if reason is None:
        return 0
    print(reason, file=sys.stderr)
    return 2


if __name__ == "__main__":
    sys.exit(main())
