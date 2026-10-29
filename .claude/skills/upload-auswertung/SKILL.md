---
name: upload-auswertung
description: Auswertung seiner Uploads - DefenseTrace.log, gelernte Tabellen (JSON), ACT-Mitschnitte (Network_*.log). Erst seine Frage, dann alle übrigen Kämpfe, Zeilenarten und Dateien.
---

# Upload-Auswertung

Anlass: Er lädt Dateien hoch, mit oder ohne Frage dazu.

## Regeln

- Er zeichnet seine Kämpfe mit ACT auf (seine Angabe); FFLogs-Reports sind also möglich.
- Was er hochlädt – `DefenseTrace.log`, die gelernten Dateien, ACT-Mitschnitte –, werte ich jedes Mal auch allgemein aus,
  wenn er eine bestimmte Frage dazu stellt (seine Vorgabe, 25.10.2026): erst seine Frage, dann alle übrigen Kämpfe,
  Zeilenarten und Dateien. Grund: Eine Frage zu einem Boss lässt den Rest des Abends ungeprüft.
- Namen anderer Spieler aus seinen Dateien kommen in keine Datei des Repositorys (A364).

## Ablauf

1. Ablage: `/root/.claude/uploads/<Sitzung>/`. Archive in einen eigenen leeren Ordner im Notizordner entpacken; Python, das sie liest, mit `-I`. Große Dateien gezielt lesen (`grep`, `awk`, Ausschnitte), nie ganz.
2. `DefenseTrace.log`: Sitzungen an „==== … trace started" trennen; die Kopfzeile nennt den Build-Commit, gegen den jede Aussage gilt. Zeilenarten je Sitzung zählen (Zeit abschneiden, Zahlen entfernen, `sort | uniq -c`). Kopf ohne Zeilen und ohne „trace ended": Schreibfehler oder Absturz, unterscheidbar seit A418 an „trace reopened".
3. ACT-Mitschnitt (`Network_*.log`, Felder mit `|`, Zeit lokal mit Versatz; Werkzeugquelle: cactbot LogGuide): 01 Gebiet, 02 Spieler (Id, Name), 03 Kämpfer, 11 Gruppe, 12 Spielerwerte (Feld 3: Job-Id), 20 Wirkbeginn, 21/22 Aktion (22: mehrere Ziele; Feld 3/4 Quelle, 5/6 Aktion, 7/8 Ziel, 10 Wert, 25/26 HP des Ziels davor), 25 Tod (Ziel, dann Quelle), 26/30 Status an/ab (Feld 4 Name, 7 Quelle, 9 Ziel), 260 Kampfstatus, 251/253 Prozess und Plugin. Die Aktionen des Spielers über seine Id aus Zeile 02.
4. Gelernte Tabellen (`HostileCastingAreaPotential.json`, `HitSeriesOnPlayer.json`, `TankbusterPotential.json`, `Sleep*.json`, `NorthHornWeaknessRecords.json`): neue und geänderte Einträge gegen die vorige Fassung, Plausibilität am Mitschnitt. Seit A456 tragen Fläche und Tankbuster ihre Schichten in `<Name>.rev.json`: Spielversion der laufenden Schicht gegen die Kopfzeile des `DefenseTrace.log`, Zählungen, Quarantäne.
5. Jeden Befund als Wirkung im Kampf am Mitschnitt belegen; Defekte im Loop beheben, offene Punkte ins TODO, Auswertung ins Archiv (Muster: A419).
