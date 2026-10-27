---
paths:
  - "CLAUDE.md"
  - ".claude/**"
---

# Regeldateien

Lädt mit dem ersten Zugriff auf `CLAUDE.md` oder eine Datei unter `.claude/`. Gilt für jede Änderung an Regeln, Regeldateien, Skills und Hooks.

- Jede Regel steht genau einmal: in CLAUDE.md, wenn sie in jeder Lage gilt, sonst in der Regeldatei (`.claude/rules/`, mit `paths:`) oder dem Skill (`.claude/skills/`) ihres Anlasses. Alles im Repository, von ihm editierbar, keine Kopien und keine erzeugten Fassungen (seine Vorgabe, 27.10.2026).
- Eine Regeldatei ohne `paths:` lädt in jeder Sitzung wie CLAUDE.md und spart nichts; was in jeder Lage gilt, gehört in CLAUDE.md.
- Eine neue Regeldatei oder ein neuer Skill steht im selben Zug in der Lesetabelle (→ „Was wann gilt", CLAUDE.md); `check_rule_files.py` prüft das.
- Jede Regel hier ändert eine Entscheidung beim Arbeiten. Was das nicht tut — Zitate, Fehlergeschichten, Quellenangaben, Umgebungsdetails —, gehört ins Archiv (`AUDIT_LOG.md`), weil CLAUDE.md nach jeder Kontextkomprimierung ganz gelesen wird und Ballast die Regeln verdrängt.
- Form: eine Anweisung je Punkt, der Grund in einem Halbsatz, ein Beispiel nur, wo die Regel ohne es falsch angewandt würde.
- Umfang folgt dem Inhalt, keine harte Zeilenzahl: Eine Regel, die in jeder Lage gilt, bleibt in CLAUDE.md, auch wenn die Datei dadurch länger wird; eine, die nur in einer Lage gilt, wandert in deren Regeldatei, auch wenn CLAUDE.md kurz ist (seine Vorgabe, 27.10.2026). Richtwerte der Dokumentation – etwa 200 Zeilen – sind Hinweise auf die Befolgung, keine Grenze.
- Verweise nennen den **Titel** einer Regel, nie „oben" oder „darunter" — Positionen verschieben sich. Liegt der Titel in einer anderen Datei, steht ihr Name dabei.
- Neue Regeln einarbeiten, nicht anhängen: vorher prüfen, ob eine bestehende dasselbe sagt, ihr widerspricht oder nur genauer wird, und dann die bestehende umschreiben. Nach jeder Änderung alle Regeldateien prüfen: Zählungen, Verweise, Widersprüche (A134).
