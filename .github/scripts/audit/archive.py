#!/usr/bin/env python3
"""Targeted access to AUDIT_LOG.md, which is read by entry and never whole (CLAUDE.md, "Was wann gilt").

The archive holds over four hundred entries in one file of more than a megabyte; scripts point into it by
entry number, so it stays one file. What a task needs is the few entries of its area:

  archive.py recent [N]    the headings of the newest N entries of part A and the newest N rows of part C
  archive.py find TERM...  the entries of part A whose heading or text holds every term (case-insensitive),
                           newest first, and the rows of part C that do; an entry a row of part C names is marked
  archive.py show ID...    the full text of an entry (A123) or a row (C45), then where it is named later: the
                           newer entries, the rows of part C and the concepts under docs/

Many entries carry a statement that a row of part C revokes, and most are named by a newer entry, which may
correct or replace them. An entry read without what names it later passes off a revoked
statement as valid; so show prints those mentions with the entry (the backlinks of a note vault such as Obsidian).
  archive.py --self-test   the parser against a constructed archive
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
ARCHIVE = ROOT / "AUDIT_LOG.md"
A_HEAD = re.compile(r"^### (A\d+)\s+·\s+(.*)$")
ANY_A_HEAD = re.compile(r"^### A\d+\b", re.M)
C_ROW = re.compile(r"^\| (C\d+) \|")
ID_REF = re.compile(r"\b([AC]\d{1,4})\b")
FIND_LIMIT = 40


def parse(text):
    """The entries of part A as (id, title, first line, lines) and the rows of part C as (id, line number, row)."""
    lines = text.split("\n")
    entries, rows = [], []
    part = None
    current = None
    for number, line in enumerate(lines, 1):
        if line.startswith("## A ·"):
            part = "A"
            continue
        if line.startswith("## B ·"):
            part = "B"
        elif line.startswith("## C ·"):
            part = "C"
        if part != "A" and current is not None:
            entries.append(current)
            current = None
        if part == "A":
            head = A_HEAD.match(line)
            if head:
                if current is not None:
                    entries.append(current)
                current = (head.group(1), head.group(2), number, [line])
            elif current is not None:
                current[3].append(line)
        elif part == "C":
            row = C_ROW.match(line)
            if row:
                rows.append((row.group(1), number, line))
    if current is not None:
        entries.append(current)
    # The divider before part B belongs to no entry. Ordered by number, not by place: a row put in the wrong
    # place would otherwise pass for the newest.
    entries = sorted(((i, t, n, _trim(ls)) for i, t, n, ls in entries), key=lambda e: int(e[0][1:]))
    rows.sort(key=lambda r: int(r[0][1:]))
    return entries, rows


def table_breaks(rows):
    """Rows of part C that do not follow the row before them directly: a blank or other line between two rows ends the
    table, and every row after it renders as plain text when he reads the archive."""
    return [i for (_, n0, _), (i, n1, _) in zip(rows, rows[1:]) if n1 != n0 + 1]


def _trim(lines):
    while lines and lines[-1].strip() in ("", "---"):
        lines = lines[:-1]
    return lines


def mentions(entries, rows, docs=None):
    """For each id, who names it later, as (kind, by): a newer entry ("newer"), a row of part C whose first cell,
    the former statement, names it ("revoked"), a row that names it only as the refutation ("refutes"), a concept."""
    named = {}
    for i, _, _, ls in entries:
        for ref in set(ID_REF.findall("\n".join(ls))):
            if ref != i and ref.startswith("A") and int(ref[1:]) < int(i[1:]):
                named.setdefault(ref, []).append(("newer", i))
    for i, _, row in rows:
        cells = row.split(" | ")
        statement = set(ID_REF.findall(cells[1])) if len(cells) > 1 else set()
        for ref in set(ID_REF.findall(row)) - {i}:
            named.setdefault(ref, []).append(("revoked" if ref in statement else "refutes", i))
    for rel, text in (docs or {}).items():
        for ref in set(ID_REF.findall(text)):
            named.setdefault(ref, []).append(("concept", rel))
    return named


def _by(named, wanted, kind):
    return [by for k, by in named.get(wanted, []) if k == kind]


def concept_texts():
    docs = ROOT / "docs"
    return {str(p.relative_to(ROOT)): p.read_text(encoding="utf-8") for p in sorted(docs.rglob("*.md"))} \
        if docs.exists() else {}


def recent(entries, rows, count):
    out = [f"{i} (Zeile {n}): {t}" for i, t, n, _ in entries[-count:]]
    out += [f"{i} (Zeile {n}): {row[:160]}" for i, n, row in rows[-count:]]
    return out


def find(entries, rows, terms, named=None):
    terms = [t.lower() for t in terms]
    named = named or {}
    hits = []
    for i, t, n, ls in reversed(entries):
        body = "\n".join(ls).lower()
        if all(term in body for term in terms):
            revoked = _by(named, i, "revoked")
            hits.append(f"{i} (Zeile {n}): {t}" + (f"  [Widerruf: {', '.join(revoked)}]" if revoked else ""))
    for i, n, row in reversed(rows):
        if all(term in row.lower() for term in terms):
            hits.append(f"{i} (Zeile {n}): {row[:160]}")
    if len(hits) > FIND_LIMIT:
        return hits[:FIND_LIMIT] + [f"... {len(hits) - FIND_LIMIT} weitere; die Suche enger fassen"]
    return hits


def show(entries, rows, ids, named=None):
    named = named or {}
    out = []
    for wanted in ids:
        wanted = wanted.upper()
        found = [ls for i, _, _, ls in entries if i == wanted] or [[row] for i, _, row in rows if i == wanted]
        if not found:
            out.append(f"{wanted}: nicht im Archiv")
            continue
        out += found[0]
        revoked, refutes = _by(named, wanted, "revoked"), _by(named, wanted, "refutes")
        newer, concepts = _by(named, wanted, "newer"), _by(named, wanted, "concept")
        out.append(f"-- {wanted} später genannt:")
        out.append("   Widerrufe: " + (", ".join(revoked) + " – vor Verwendung lesen" if revoked else "keine"))
        if refutes:
            out.append("   als Widerlegung in: " + ", ".join(refutes))
        out.append("   neuere Einträge: " + (", ".join(newer) if newer else "keine"))
        out.append("   Konzepte: " + (", ".join(concepts) if concepts else "keine"))
        out.append("")
    return out


def self_test():
    sample = "\n".join([
        "# Audit-Log", "", "## A · Vorgänge", "",
        "### A1  · Erster Vorgang (01.10.2026)", "", "- Text über den Weißmagier.", "",
        "### A2 · Zweiter Vorgang (02.10.2026)", "", "- Glockenspiel und Tankbuster, korrigiert A1.", "- Zweite Zeile.", "",
        "---", "## B · Commit-Register", "", "| Commit | Inhalt |", "",
        "## C · Widerrufene Aussagen", "", "| C2 | A2: „Anderes\" | Tankbuster zu stark (A1). | Widerrufen |",
        "| C1 | „Zitat\" | Falsch. | Widerrufen |",
    ])
    entries, rows = parse(sample)
    named = mentions(entries, rows, {"docs/x.md": "siehe A2", "docs/y.md": "nichts"})
    shown = show(entries, rows, ["A2"], named)
    checks = {
        "two entries in part A, one heading with a doubled space": [e[0] for e in entries] == ["A1", "A2"]
        and entries[0][1] == "Erster Vorgang (01.10.2026)",
        "an entry ends before the divider of part B": entries[1][3][-1] == "- Zweite Zeile.",
        "two rows in part C, ordered by number though out of place": [r[0] for r in rows] == ["C1", "C2"],
        "find matches heading and text, all terms": [h.split()[0] for h in find(entries, rows, ["tankbuster"])] == ["A2", "C2"]
        and find(entries, rows, ["glockenspiel", "weißmagier"]) == [],
        "find is case-insensitive": [h.split()[0] for h in find(entries, rows, ["WEISSMAGIER".replace("SS", "ß")])] == ["A1"],
        "show returns the whole entry": show(entries, rows, ["a2"])[0].startswith("### A2") and "- Zweite Zeile." in show(entries, rows, ["A2"]),
        "show names a missing id": show(entries, rows, ["A9"]) == ["A9: nicht im Archiv"],
        "recent takes the newest": recent(entries, rows, 1)[0].startswith("A2") and recent(entries, rows, 1)[1].startswith("C2"),
        "show names the revoking row, newer entries and concepts": "   Widerrufe: C2 – vor Verwendung lesen" in shown
        and "   Konzepte: docs/x.md" in shown and "   neuere Einträge: A2" in show(entries, rows, ["A1"], named),
        "an entry names no older one as later": "   neuere Einträge: keine" in shown,
        "a row naming an entry as the refutation does not revoke it": "   Widerrufe: keine" in show(entries, rows, ["A1"], named)
        and "   als Widerlegung in: C2" in show(entries, rows, ["A1"], named),
        "a line between two rows breaks the table of part C": table_breaks(sorted(rows, key=lambda r: r[1])) == []
        and table_breaks(sorted(parse(sample.replace("| C1 |", "\n| C1 |"))[1], key=lambda r: r[1])) == ["C1"],
        "find marks a revoked entry": find(entries, rows, ["glockenspiel"], named)[0].endswith("[Widerruf: C2]"),
    }
    failed = [name for name, ok in checks.items() if not ok]
    if failed:
        print("self-test FAILED: " + "; ".join(failed))
        return 1
    print("self-test ok: entries and rows parsed, find by every term in heading and text, show with later mentions, recent")
    return 0


def main(argv):
    if not argv or argv[0] in ("-h", "--help"):
        print(__doc__)
        return 0
    if argv[0] == "--self-test":
        return self_test()
    text = ARCHIVE.read_text(encoding="utf-8")
    entries, rows = parse(text)
    if not entries or not rows:
        # A parser that finds nothing would answer every question with silence; that must not pass as "no hit".
        print(f"{ARCHIVE.name}: no entries parsed - the archive's layout changed, fix the parser")
        return 1
    breaks = table_breaks(sorted(rows, key=lambda r: r[1]))
    if breaks:
        print(f"{ARCHIVE.name}: the table of part C breaks before {', '.join(breaks)} - remove what stands between the rows")
        return 1
    headings = len(ANY_A_HEAD.findall(text))
    if headings != len(entries):
        # A heading the parser misses hides its entry from show and find and hangs its text on the entry before.
        print(f"{ARCHIVE.name}: {headings} entry headings, {len(entries)} parsed - a heading is not '### A<n> · title'")
        return 1
    command, args = argv[0], argv[1:]
    if command == "recent":
        out = recent(entries, rows, int(args[0]) if args else 3)
    elif command == "find" and args:
        out = find(entries, rows, args, mentions(entries, rows)) or [f"kein Eintrag mit: {' '.join(args)}"]
    elif command == "show" and args:
        out = show(entries, rows, args, mentions(entries, rows, concept_texts()))
    else:
        print(__doc__)
        return 2
    print("\n".join(out))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
