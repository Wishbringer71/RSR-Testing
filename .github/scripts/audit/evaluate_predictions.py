#!/usr/bin/env python3
"""Judges an uploaded DefenseTrace.log against the predictions in .github/scripts/audit/predictions/*.json
(archiv-todo.md, "Vorhersage"; A792). The procedure follows the owner's other repositories (KSP-MyContracts,
KerbalMissionDirector, Retainer-Repricer); the code is this repository's own.

Why: a prediction read by hand after the log is in reads every result as a confirmation. Here the expectation is fixed
before the build is played and judged by rule.

A prediction: {"id", "archive", "kind", "match", "situation"?}
  present  a line matches "match"; with "situation", judged only once a line matches it (else OPEN).
  absent   once a line matches "situation" (required), no line matches "match".
  each     every line that matches "situation" (required) also matches "match" (one regex on the same line).
A session counts for a prediction only if the prediction stood with exactly this content in the commit the session's
header names ("version ... | commit <sha> | ..."), as that is the build the owner played; else UNBOUND. Bound by content
and ancestry, not by time: a prediction rewritten after the build does not count.
Verdict per prediction over all sessions: FAIL if a bound session fails, else PASS if one passes, else OPEN (the
situation did not occur), else UNBOUND.

Usage: evaluate_predictions.py <DefenseTrace.log> [...] [--predictions DIR]
       evaluate_predictions.py --self-test
"""
import glob
import json
import os
import re
import subprocess
import sys
from cli_args import option_value  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
DEFAULT_DIR = os.path.join(ROOT, ".github", "scripts", "audit", "predictions")
STARTED = re.compile(r"^==== .* trace started")
COMMIT = re.compile(r"\| commit ([0-9a-f]{7,40}) \|")
KINDS = ("present", "absent", "each")
# A decimal written with a dot only: the trace writes numbers in the client's culture, "2,35 s" in his German one (A877).
# A pattern that accepts only the dot never matches there - a "present" fails and an "each" or "absent" stays OPEN.
DOT_ONLY_DECIMAL = re.compile(r"(?:\\d|\[0-9\]|\])[+*?]?\\\.(?:\\d|\[0-9\])|\[(?:0-9|\\d)\.\]|\[\.(?:0-9|\\d)\]")


def load(directory):
    found = []
    for path in sorted(glob.glob(os.path.join(directory, "*.json"))):
        with open(path, encoding="utf-8") as handle:
            for entry in json.load(handle):
                found.append((os.path.relpath(path, ROOT).replace(os.sep, "/"), entry))
    return found


def malformed(entry):
    """Why a prediction cannot be judged, or None."""
    for key in ("id", "archive", "kind", "match"):
        if not entry.get(key):
            return f"{entry.get('id', '?')}: no {key}"
    if entry["kind"] not in KINDS:
        return f"{entry['id']}: unknown kind {entry['kind']}"
    if entry["kind"] in ("absent", "each") and not entry.get("situation"):
        return f"{entry['id']}: kind {entry['kind']} needs a situation"
    for key in ("match", "situation"):
        if entry.get(key):
            try:
                re.compile(entry[key])
            except re.error as error:
                return f"{entry['id']}: {key} is no regex ({error})"
            if DOT_ONLY_DECIMAL.search(entry[key]):
                return f"{entry['id']}: {key} reads a decimal with a dot only; the trace writes the client's culture ([.,])"
    return None


def sessions(lines):
    """[(commit or None, lines)] split at the "trace started" headers."""
    found = []
    for line in lines:
        if STARTED.match(line):
            found.append([None, []])
            continue
        if not found:
            if not line.strip():
                continue
            found.append([None, []])
        if found[-1][0] is None and COMMIT.search(line):
            found[-1][0] = COMMIT.search(line).group(1)
        found[-1][1].append(line)
    return [(commit, body) for commit, body in found]


def judge(entry, lines):
    """PASS, FAIL or OPEN for one session's lines."""
    match = re.compile(entry["match"])
    situation = re.compile(entry["situation"]) if entry.get("situation") else None
    seen = [line for line in lines if situation is None or situation.search(line)]
    if entry["kind"] == "present":
        if situation is not None and not seen:
            return "OPEN"
        return "PASS" if any(match.search(line) for line in lines) else "FAIL"
    if not seen:
        return "OPEN"
    if entry["kind"] == "absent":
        return "FAIL" if any(match.search(line) for line in lines) else "PASS"
    return "PASS" if all(match.search(line) for line in seen) else "FAIL"


def git_content(commit, path):
    """The predictions file as it stood in <commit>, or None."""
    out = subprocess.run(["git", "show", f"{commit}:{path}"], cwd=ROOT, capture_output=True, text=True, encoding="utf-8")
    return out.stdout if out.returncode == 0 else None


def bound(entry, path, commit, content_at):
    """Whether this exact prediction stood in the build the session ran."""
    if commit is None:
        return False
    text = content_at(commit, path)
    if text is None:
        return False
    try:
        return any(other == entry for other in json.loads(text))
    except ValueError:
        return False


def evaluate(predictions, log_lines, content_at=git_content):
    """{id: (verdict, detail)} over all sessions of the log."""
    result = {}
    parts = sessions(log_lines)
    for path, entry in predictions:
        problem = malformed(entry)
        if problem:
            result[entry.get("id", "?")] = ("MALFORMED", problem)
            continue
        verdicts = []
        for commit, lines in parts:
            if not bound(entry, path, commit, content_at):
                verdicts.append(("UNBOUND", commit))
                continue
            verdicts.append((judge(entry, lines), commit))
        kinds = [v for v, _ in verdicts]
        verdict = "FAIL" if "FAIL" in kinds else "PASS" if "PASS" in kinds else "OPEN" if "OPEN" in kinds else "UNBOUND"
        result[entry["id"]] = (verdict, ", ".join(f"{c or 'no commit'} {v}" for v, c in verdicts))
    return result


def self_test():
    stood = {"p": [{"id": "x", "archive": "A1", "kind": "absent", "situation": "gaze: turned", "match": "hit despite"}]}
    def content_at(commit, path):
        return json.dumps(stood["p"]) if commit == "abc1234" else None

    entry = stood["p"][0]
    preds = [("p.json", entry)]
    log = ["==== 2026-11-14 10:00:00 trace started", "version 1 | commit abc1234 | area defence True",
           "10:00:01 gaze: turned 40° away from #3499", "10:00:02 gaze: avoided #3499"]
    assert evaluate(preds, log, content_at)["x"][0] == "PASS", evaluate(preds, log, content_at)
    failing = log + ["10:00:03 gaze: hit despite RSR's facing"]
    assert evaluate(preds, failing, content_at)["x"][0] == "FAIL"
    quiet = log[:2] + ["10:00:01 combat started"]
    assert evaluate(preds, quiet, content_at)["x"][0] == "OPEN", "no situation, no verdict"
    other_build = ["==== 2026-11-14 10:00:00 trace started", "version 1 | commit def5678 | x"] + failing[2:]
    assert evaluate(preds, other_build, content_at)["x"][0] == "UNBOUND", "a build without the prediction does not count"
    rewritten = [("p.json", dict(entry, match="something else"))]
    assert evaluate(rewritten, failing, content_at)["x"][0] == "UNBOUND", "a prediction changed after the build does not count"
    each = [("p.json", {"id": "e", "archive": "A1", "kind": "each", "situation": "landed #3499", "match": r"\(0 looking"})]
    stood["p"] = [each[0][1]]
    lines = log[:2] + ["landed #3499: hit 2 (0 looking the safe way)", "landed #3499: hit 1 (1 looking the safe way)"]
    assert evaluate(each, lines, content_at)["e"][0] == "FAIL"
    assert evaluate(each, lines[:3], content_at)["e"][0] == "PASS"
    present = [("p.json", {"id": "s", "archive": "A1", "kind": "present", "situation": "turned.*#3499", "match": "avoided #3499"})]
    stood["p"] = [present[0][1]]
    assert evaluate(present, log, content_at)["s"][0] == "PASS"
    assert evaluate(present, quiet, content_at)["s"][0] == "OPEN"
    two = log + ["==== 2026-11-14 11:00:00 trace started", "version 1 | commit def5678 | x", "gaze: turned"]
    assert evaluate(present, two, content_at)["s"][0] == "PASS", "a later unbound session does not undo a bound verdict"
    assert malformed({"id": "m", "archive": "A1", "kind": "absent", "match": "x"}) is not None, "absent needs a situation"
    assert malformed({"id": "m", "archive": "A1", "kind": "present", "match": "("}) is not None, "a broken regex is reported"
    for dot_only in (r"GCD (\d\.\d\d) s", r"GCD [0-9.]+ s", r"GCD [\d.]+ s", r"GCD [0-9]+\.[0-9] s", r"[.0-9]+"):
        assert malformed({"id": "m", "archive": "A1", "kind": "present", "match": dot_only}) is not None, dot_only
    for either in (r"GCD (\d[.,]\d\d) s", r"GCD [0-9.,]+ s", r"GCD [0-9,.]+ s", r"tank_lockon\.avfx", r"[0-9]+ s", r"\(spell\)"):
        assert malformed({"id": "m", "archive": "A1", "kind": "present", "match": either}) is None, either
    two_cultures = [("p.json", {"id": "c", "archive": "A1", "kind": "present", "match": r"GCD [0-9.,]+ -> [0-9.,]+ s"})]
    stood["p"] = [two_cultures[0][1]]
    assert evaluate(two_cultures, log[:2] + ["GCD 2,36 -> 2,38 s"], content_at)["c"][0] == "PASS", "a German trace"
    assert evaluate(two_cultures, log[:2] + ["GCD 2.36 -> 2.38 s"], content_at)["c"][0] == "PASS", "an English trace"
    for path, entry in load(DEFAULT_DIR):
        problem = malformed(entry)
        assert problem is None, f"{path}: {problem}"
    print("self-test ok: present, absent and each judged; no situation stays open; a build without the prediction or a "
          "prediction changed after it is unbound; sessions combine; a decimal read with a dot only is refused; the repository's "
          "predictions are well formed")
    return 0


def main(argv):
    if argv[:1] == ["--self-test"]:
        return self_test()
    directory = DEFAULT_DIR
    if "--predictions" in argv:
        directory = option_value(argv, "--predictions")
        index = argv.index("--predictions")
        argv = argv[:index] + argv[index + 2:]
    if not argv:
        print(__doc__)
        return 2
    lines = []
    for path in argv:
        with open(path, encoding="utf-8", errors="replace") as handle:
            lines += handle.read().splitlines()
    predictions = load(directory)
    for key, (verdict, detail) in evaluate(predictions, lines).items():
        archive = next((e.get("archive", "") for _, e in predictions if e.get("id") == key), "")
        print(f"{verdict:9s} {key} ({archive}): {detail}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
