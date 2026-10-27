---
paths:
  - "docs/**"
---

# Konzepte und Quellen

Lädt mit dem ersten Zugriff auf eine Datei unter `docs/`. Gilt für jede Aussage, Recherche und Änderung an einem Konzept.

## Quellen

- Quellen ausschöpfen, bevor ich eine Grenze behaupte; Zugang pfadweise messen, nicht pauschal.
- Eine Fundstelle, die ich nicht finde, ist nicht unerreichbar — weitere Pfade versuchen.
- Ein Werkzeug sage ich erst zu, wenn eine Probe es belegt. Zur Vorlage gehört der Preis des Wegs: Was er mit einem Handgriff erledigt, ist keine Aufgabe für diese Umgebung.
- Einen fremden Schutzmechanismus führe ich erst als Begründung an, wenn ich geprüft habe, was er abdeckt.
- Der Netzzugang ist seit 27.09.2026 unbegrenzt (seine Einstellung). Offizielle Quelle für Wirktexte samt der Werte, die `ActionId.resx` ausblendet (Dauern, Potenzen, Statusnamen): der Job-Guide (`na.`/`de.finalfantasyxiv.com/jobguide/`). Referenz für Rotationen: The Balance (`thebalanceffxiv.com`). Spieldaten: `garlandtools.org`, `v2.xivapi.com`. gamerescape und icy-veins weisen Skripte per Cloudflare-Prüfung ab — das ist die Seite, nicht die Umgebung.
- Öffentliche Git-Repositories sind über den Git-Proxy klonbar. `PunishXIV/WrathCombo` (Klon unter `/home/user/punishxiv/wrathcombo`) belegt, wie ein anderes Rotationswerkzeug eine Mechanik behandelt und was die Spiel-API liefert — keine Spielquelle, Status so benennen; Code nur lesen, nicht übernehmen. Was eine API des Clients tut, belegt ihr Quelltext: `goatcorp/Dalamud`, `aers/FFXIVClientStructs` (durch Reverse Engineering gewonnen), `NightmareXIV/ECommons`, `FFXIV-CombatReborn/BossmodReborn`; Spaltennamen der Blätter `xivdev/EXDSchema`. Status jeweils Werkzeugquelle. Vor jeder Aussage aus einem Klon bringe ich ihn auf den Stand seines Remotes und nenne Commit und Datum (nach seiner Frage, 27.10.2026); ein Klon von voriger Woche belegt den Stand von voriger Woche.
- Language-Server-Plugins (csharp-lsp) startet Claude Code nur in Terminal-Sitzungen, nicht in dieser Umgebung; `lsp_query.py` startet csharp-ls je Frage selbst (Debug-Build des Source Generators nötig, sonst scheitert jede Referenzsuche).
- `xivanalysis/xivanalysis` (über den Git-Proxy klonbar, MIT) bewertet FFLogs-Reports je Job nach Rotationsregeln — Community-Auswertung auf Schaden hin, keine Spielquelle; Regelkatalog für den Abgleich, gefiltert durch seine Vorgaben.

## Konzepte

- Mit der Erkenntnis fortschreiben, im selben Zug: Vorgabe, Messergebnis, widerlegte Annahme. Grund: Nach einer Kontextkomprimierung sind Chat und Commit-Nachricht weg, und ein Konzept mit altem Stand widerlegt die Erkenntnis.
- Ein Gegenstand gehört in das bestehende Konzept, das ihn trägt; ein neues Dokument nur, wenn keines ihn trägt (seine Vorgabe, 01.10.2026). Länge eines Konzepts und das Muster früherer Dokumente sind kein Grund — ein zweites Dokument verteilt den Gegenstand auf zwei Stellen, und eine davon veraltet.
- Urteilsstil: geltender Sachstand zuerst, Begründung danach, Ausgeschlossenes mit Grund. Keine Chronik der eigenen Fassungen, keine Nachträge; Änderungen einarbeiten.
- Prüfkriterium: Wer einen Abschnitt allein liest, erhält keinen überholten Stand.
- Die Geschichte des Gegenstands bleibt Inhalt; die des Dokuments steht im Archiv. Vor dem Entfernen prüfen, dass sie dort steht.
- Die Loop-Stufen sind Arbeitsverfahren, keine Gliederung eines Konzepts.
