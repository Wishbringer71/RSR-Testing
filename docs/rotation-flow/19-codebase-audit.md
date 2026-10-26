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
- **Rotationen, begonnen (26.10.2026):** Weißmagier vollständig – Thin Air wirkte Raise hart, gleich was die
  Einstellung sagte (behoben A401, Konzept 11) –, Maschinist vollständig ohne Befund; Klassensuche Wiederbelebung:
  Swiftcast auf die Wiederbelebung ging an „Raise with Swiftcast or Dualcast" vorbei (behoben A402); Beschwörer bis
  zur Burst-Logik ohne Befund, die Burst- und Beschwörungslogik führt Konzept 12 gegen den Code; die Leser der breiten Tankbuster-Lesart (`IsHostileCastingToTank`) sind nach A395 nur noch die
  Öffnung der Einzelabwehr für Tanks und Heiler, wie Konzept 09 sie führt. Gelehrter vollständig ohne Befund;
  Astrologe vollständig – die Strategie für Essential Dignity hielt die GCD-Heilungen auch, wenn die letzte Ladung auf
  Gefahr wartete (behoben A403, Konzept 07), „Prioritize Microcosmos over all other healing" hielt Oracle und den
  Trank mit (behoben A404, Konzept 08); Weiser vollständig – Taurochole, für Gefahr zurückgehalten, hielt Druochole
  mit (behoben A405, Konzept 07). Paladin und Krieger vollständig ohne Befund; Dunkelritter vollständig – „Keep at
  least 3000 MP" galt nicht, wenn Darkside auslief (behoben A406, Konzept 10); Revolverklinge vollständig ohne Befund.
- **Übrige Damage Dealer, Klassensuchen (26.10.2026):** jede Rotationsoption hat einen Leser (291, auch ohne
  `nameof`); jede einschränkende Option („Only", „Prevent", „Keep", „Limit" …) wird an allen Aufrufstellen ihrer Aktion
  gefragt; Bewegungsaktionen im Schadenspfad laufen über die zentrale Sprungprüfung (A358); Abwehr- und Heilpfade von
  Barde, Tänzer, Rotmagier, Schwarzmagier, Pictomancer und Beschwörer gelesen, Barde und Tänzer bis zur
  Schadenslogik. Ohne Befund.
- **Oberfläche (26.10.2026):** `SearchableConfigs` gelesen – „Reset to Default Value." schaltete jeden `bool`-Schalter
  ab, statt die Voreinstellung zu setzen (behoben A407, Konzept 18). Ohne Befund: jede Zahleneinstellung trägt einen
  Wertebereich; ausgeblendete Unteroptionen der Rotationen wirken nur, solange ihr Schalter an ist; die
  Aktionseinstellungen (`MainWindow_Actions`), die Statuslisten (`MainWindow_List`, Hinzufügen, Entfernen, Speichern) und
  die AoE-Umschaltung der Steuerfenster („Auto" setzt „Full", „Manual" den gemerkten Typ – so gebaut, ohne
  widersprechenden Text). Nur anzeigend und ohne Wirkung im Kampf, nicht zeilenweise gelesen: Diagnose-, Zeitleisten-,
  Lehr-, Tutorial-, Änderungs- und Osterei-Fenster.
- **Schadenslogik der Damage Dealer (26.10.2026), begonnen:** Dragoon ohne Befund; Samurai – Feint fiel unter seiner
  eigenen Gesundheitsschwelle ohne Treffer in Sicht (behoben A408, Konzept 08), sonst vollständig ohne Befund; Schnitter vollständig –
  Einschubsperren von Schnitter, Viper und Samurai auf Sicherheitspfaden wichen der Schranke nicht (behoben A409,
  Konzept 08); Viper und Rotmagier in Abwehr, Heilung, Wiederbelebung und den Hauptpfaden der GCDs ohne Befund; Ninja:
  die Mudra-Sperre sitzt zentral im Fähigkeitspfad (`CustomRotation_Ability`), Shade Shift kann die Ninjutsu nicht
  brechen; Schwarzmagier: die Ley-Lines-Sperre bei Flächencasts sitzt in `ModifyLeyLinesPvE` und gilt für beide
  Rotationen.
- **Offen:** die übrige Schadenslogik Zeile für Zeile – Ninja (Mudra-Folgen), Pictomancer, Schwarzmagier,
  Beschwörer außerhalb von Konzept 12, Barde (Lieder), Tänzer, Monk; ihre Prioritäten sind gegen The Balance abgeglichen
  (A375–A387, Konzept 14)
  (`RebornRotations`; `ExtraRotations` nur, wo eine zentrale Änderung sie berührt, Churin ausgenommen).

## Methode

- Je Bereich zuerst, was ein Prüfskript mit Selbsttest fassen kann – eine Fehlerklasse, die sich wiederholen kann,
  gehört in die CI. Dann Lesen gegen die Konzepte: was eine Stelle ausdrücken soll, ob sie es tut, was es im Kampf
  ändert.
- Ein Befund bekommt seinen eigenen Loop und seinen Archiveintrag; dieses Konzept nennt den Stand je Bereich, nicht die
  Geschichte der Befunde.
- Reihenfolge nach Wirkung in seinem Profil: zuerst, was jeder Job durchläuft (`DataCenter`, `BaseAction`,
  `ActionTargetInfo`), dann die Rotationen seiner häufigen Jobs, zuletzt die Oberfläche.
