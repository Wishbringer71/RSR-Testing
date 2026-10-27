---
paths:
  - "docs/fork-changes-*.md"
  - "CHANGELOG.md"
---

# Release-Texte

Lädt mit dem ersten Zugriff auf einen Release-Text oder `CHANGELOG.md`.

- Englisch; sie beschreiben, was der Fork seit dem letzten Release selbst geändert hat — nicht den Bestand und nicht die Änderungen von Upstream (seine Vorgabe, 23.10.2026: „hier sind nur die eigenen änderungen aufzuführen").
- `docs/fork-changes-since-last-release.md` mit dem Ausgangs-Release im Titel; nach Veröffentlichung übernimmt `docs/fork-changes-in-play.md` den Text (`check_release_note.py`).
- Keine Rechenschaft über eigene Fehler darin.
- Die Längengrenze ist die des tatsächlichen Wegs (Release-API über `body_path`).
