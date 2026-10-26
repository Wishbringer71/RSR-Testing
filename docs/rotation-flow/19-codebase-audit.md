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
- **`DataCenter`, inhaltlich (26.10.2026, abgeschlossen):** jede Region gelesen. Drei Defekte mit Wirkung im Kampf:
  Die Pull-Regel der Tanks verlangte zusätzlich, dass mehr als 30 % der Gegner in 25 Yalm bei ihm stehen, und blieb
  neben ungezogenen Rudeln zu (behoben A393, Konzept 09, „Die Abwehr im Pull"). Im M9 stand der Spread-Marker von
  Aetherletting auch unter den Stacks und öffnete den Selbstschutz für einen Spread auf einem anderen (behoben A394,
  Konzept 13, „Marker"; alle übrigen Markerpfade gegen Lockon-Blatt und BossMod geprüft). Die Pull-Rückhaltung der
  großen Minderung fiel bei jedem nicht unterbrechbaren Trash-Zauber auf das Ziel eines Gegners (behoben A395, Konzept
  09). Ohne Wirkung im Kampf, berichtigt (A396): eine Beschreibung an der falschen Eigenschaft, ein Kommentar auf dem
  Stand vor A303, die Personenmaske von BossModReborn blieb beim Zurücksetzen stehen. Die Liste der Allianzraids deckt
  sich mit den Spielblättern (`TerritoryIntendedUse` 8, 18 Gebiete, 26.10.2026); sie altert mit dem nächsten
  Allianzraid, die Fähigkeitsprüfung über das Blatt wäre die Konstruktion ohne Pflege – heute ohne Wirkung, Adressat
  Upstream. Begegnungsspezifische Sonderfälle von Upstream (M10S, M11S, Orbonne, Lakshmi, Lich) sind gelesen, nicht
  gegen die Kämpfe geprüft: Sie liegen außerhalb dessen, was sich hier belegen lässt, und gelten nur in ihrem Gebiet.
- **`BaseAction` mit `ActionBasicInfo` und `ActionCooldownInfo` (26.10.2026):** gelesen. Ein Defekt mit Wirkung:
  `JustUsedAfter` hielt eine bereite Aktion für eben benutzt – Uncoiled Fury des Vipers ging neben einem bereiten
  Serpent's Ire, Perfect Balance des Monks blieb mit zwei Ladungen gesperrt (behoben A397).
- **`ActionTargetInfo` bis zur Bodenplatzierung (26.10.2026):** gelesen. Freundliche Bodenflächen gingen an die
  Arenamitte, solange irgendwer dort stand; jetzt an die Stelle, die mehr Mitglieder deckt, wenn es sie gibt (A398,
  Konzept 07, Option ab Werk an).
- **`ActionTargetInfo`, AoE-Zählung (26.10.2026):** gelesen. Linien mit Ziel zählten nach der Blickrichtung des
  Spielers statt zum Ziel hin (behoben A399, Konzept 03, C3).
- **`ActionTargetInfo.FindTargetByType` (26.10.2026):** gelesen bis auf die Zielarten, die die Konzepte 07 und 09 gegen
  den Code führen; ohne Befund (A400). Klassensuche nach Abklingzeit-Fragen in den Rotationen: ohne Defekt, eine tote
  Bedingung beim Ninja (A400).
- **Rotationen, begonnen (26.10.2026):** Weißmagier bis zur GCD-Logik und Maschinist bis zur Wildfire-Logik gelesen,
  ohne Befund; die Leser der breiten Tankbuster-Lesart (`IsHostileCastingToTank`) sind nach A395 nur noch die
  Öffnung der Einzelabwehr für Tanks und Heiler, wie Konzept 09 sie führt.
- **Offen:** die Rotationen Datei für Datei, zuerst der Rest von Weißmagier und Maschinist, dann Beschwörer und die
  übrigen Jobs (`RebornRotations`; `ExtraRotations` nur, wo eine zentrale Änderung sie berührt, Churin ausgenommen);
  `RotationSolver/UI`.

## Methode

- Je Bereich zuerst, was ein Prüfskript mit Selbsttest fassen kann – eine Fehlerklasse, die sich wiederholen kann,
  gehört in die CI. Dann Lesen gegen die Konzepte: was eine Stelle ausdrücken soll, ob sie es tut, was es im Kampf
  ändert.
- Ein Befund bekommt seinen eigenen Loop und seinen Archiveintrag; dieses Konzept nennt den Stand je Bereich, nicht die
  Geschichte der Befunde.
- Reihenfolge nach Wirkung in seinem Profil: zuerst, was jeder Job durchläuft (`DataCenter`, `BaseAction`,
  `ActionTargetInfo`), dann die Rotationen seiner häufigen Jobs, zuletzt die Oberfläche.
