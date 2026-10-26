# 19 · Audit der gesamten Codebasis

**Leitfrage:** Wo weicht der Code – Upstream und Fork gleichermaßen – von seiner Absicht, vom Text seiner Einstellungen
oder von den Konzepten ab, und was ändert das im Kampf?

Sein Auftrag (05.09.2026): „Audit und Code-Review über die gesamte Codebasis" – der ganze Baum, nicht nur der Fork-Diff.
Abgearbeitet nach seiner Vorgabe vom 26.10.2026: „todo nach priorität und umsetzbarkeit in loops abarbeiten".

## Sachstand

- **Abgeschlossen:** mechanische Scans über alle Bäume (A8), Rotationsbäume, Konfiguration, Oberfläche, Kommandos, IPC
  und BossMod-Updater (A10), der zweite Durchgang mit allen Prüfskripten (A82), die Einzelprüfung der Fork-Commits
  (A286–A290). Was davon in der CI weiterläuft, steht in `run_ci_checks.py`.
- **`DataCenter`, mechanisch:** 486 Mitglieder, neun ohne Leser im Baum (26.10.2026). Sieben sind öffentliche Werte, die
  abgeleitete Rotationen lesen dürfen (`IsOnStaging`, `IsInM12S`, `IsInTheAquapolis`, `IsInDeepDungeons`,
  `SylphManagementFinished`, `PartyMembersDifferHP`, `LowestPartyMembersDifferHP`), `AreaMeasurementTally` ist die
  öffentliche Sicht neben der gelesenen Textform. `RaidTimeRaw` ist intern und ohne Leser – der Rest einer
  Zeitleisten-Funktion von Upstream (`57ad3f81d`, 27.02.2024); Adressat Upstream, nicht gelöscht, weil der Fork sonst
  einen Konflikt in fremdem Code trägt und nichts im Kampf gewinnt.
- **`DataCenter`, inhaltlich (26.10.2026):** gelesen sind Gebiete und Inhaltsarten, Rollen und Wiederbelebung, GCD,
  Begleiter, Gesundheit, Aktionsprotokoll und Gegnerzahlen in Reichweite. Ein Defekt mit Wirkung: Die Pull-Regel der
  Tanks verlangte zusätzlich, dass mehr als 30 % der Gegner in 25 Yalm bei ihm stehen, und blieb neben ungezogenen
  Rudeln zu (behoben A393, Konzept 09, „Die Abwehr im Pull"). Die Liste der Allianzraids deckt sich mit den
  Spielblättern (`TerritoryIntendedUse` 8, 18 Gebiete, 26.10.2026); sie altert mit dem nächsten Allianzraid, die
  Fähigkeitsprüfung über das Blatt wäre die Konstruktion ohne Pflege – heute ohne Wirkung, Adressat Upstream.
- **Offen:** `DataCenter` inhaltlich ab „Hostile Casting“ und der BossModReborn-Zeitleiste, dazu der Kopf der Klasse; die Rotationen Datei für Datei (`RebornRotations`;
  `ExtraRotations` nur, wo eine zentrale Änderung sie berührt, Churin ausgenommen); `RotationSolver/UI`.

## Methode

- Je Bereich zuerst, was ein Prüfskript mit Selbsttest fassen kann – eine Fehlerklasse, die sich wiederholen kann,
  gehört in die CI. Dann Lesen gegen die Konzepte: was eine Stelle ausdrücken soll, ob sie es tut, was es im Kampf
  ändert.
- Ein Befund bekommt seinen eigenen Loop und seinen Archiveintrag; dieses Konzept nennt den Stand je Bereich, nicht die
  Geschichte der Befunde.
- Reihenfolge nach Wirkung in seinem Profil: zuerst, was jeder Job durchläuft (`DataCenter`, `BaseAction`,
  `ActionTargetInfo`), dann die Rotationen seiner häufigen Jobs, zuletzt die Oberfläche.
