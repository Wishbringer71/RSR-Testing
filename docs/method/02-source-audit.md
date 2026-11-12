# 02 · Prüfung des eigenen Quellcodes

Sein Auftrag vom 12.11.2026: „den eigenen quellcodes des Plugins vollständig prüfen, inwieweit darin probleme
existieren … alles in kompletten loops". Dieses Dokument hält fest, wie geprüft wird, was geprüft ist und was noch
nicht – damit der Stand eine Kontextkomprimierung übersteht und „vollständig" messbar bleibt.

## Umfang

Der Quelltext unter `RotationSolver/`, `RotationSolver.GameData/` und `RotationSolver.SourceGenerators/`: 407 Dateien,
161.370 Zeilen, davon 53.526 Zeilen, die der Fork gegenüber `upstream/main` hinzugefügt hat (gemessen 12.11.2026,
`git diff --numstat upstream/main...HEAD`). Nicht im Umfang: ECommons, RebornMaterial, Dalamud, die erzeugten
`*.g.cs`.

## Vorige Prüfungen

- Tiefenprüfung vom 29.09.2026 (A220 bis A224: Abwehr, Heiler, Tanks, Schadensausteiler, Infrastruktur), Stand
  ad73a6a5c. Seither kamen 906 Commits; fast der ganze Fork-Code im Umfang ist jünger (Beastmaster und Crucible,
  Tankbuster-Tabelle, Tankwechsel, Schlaf, DoT, Bewegungssperre, Tötungsregeln).
- Je Vorhaben die Loops im Archiv; sie prüfen eine Regel, nicht die Datei.

## Verfahren

Zwei Arten von Durchgang, weil sie Verschiedenes finden:

1. **Mechanisch, über jede Datei.** Ein Werkzeug liest alles; die Deckung ist 100 %, die Tiefe gering. Es findet
   Formfehler: tote Bedingungen, verschluckte Ausnahmen, vergessene Freigaben, in der eigenen Aufzählung geänderte
   Sammlungen.
2. **Lesend, Datei für Datei, gegen Fehlerklassen.** Findet, was kein Werkzeug sieht: eine Regel, die im Kampf das
   Falsche tut. Reihenfolge nach Risiko – zuerst der Code des Forks, darin zuerst der jüngste und der, der im Kampf
   entscheidet. Der Code von Upstream wird gelesen, wo Fork-Code auf ihm ruht.

Jeder Befund wird am Artefakt belegt, kommt als eigener Eintrag ins `TODO.md` (oder wird im selben Zug im Loop
behoben) und ins Archiv. Ein Durchgang ohne Befund steht mit Datum und Commit in der Tabelle unten.

### Fehlerklassen für den lesenden Durchgang

Aus den Befunden der bisherigen Loops; jede mit dem Fall, an dem sie auffiel.

| # | Klasse | Frage an die Stelle | Fall |
|---|---|---|---|
| F1 | Messgröße hängt an dem, was die Regel steuert | Würde der gelernte Wert anders, wenn die Regel anders entschiede? | A709: Nettoverlust samt Heilgegenständen gelernt, die Regel spart sie ein |
| F2 | Messgröße hängt an Rolle oder Dauer statt am Gemessenen | Ist die Bezugsgröße (Zeit, Anzahl) für alle Vergleichsobjekte dieselbe? | A711: Schaden je Sekunde zwischen erstem und letztem Treffer |
| F3 | Regel hält ohne Ende | Welcher Zustand macht die Bedingung dauerhaft wahr – ein Wert 0, ein Fenster, das nie passt? | A708: Ruffenster ohne rollenden GCD, unter Tempo |
| F4 | Zustand wird nicht zurückgesetzt oder nicht nachgeführt | Was steht in dem Feld nach Gebietswechsel, neuem Lauf, Jobwechsel, frühem Rücksprung? | A696: Hörner im Crucible nicht gelesen |
| F5 | Einheiten gemischt | Anteil gegen LP, mit gegen ohne Ausrüstung, Sekunden gegen GCDs | A706: Senke des Bretts gegen Summe des Wegs |
| F6 | Erkennung ohne Reichweite oder Lage | Trifft das Erkannte den, für den gehandelt wird? | A701: Rückstoß in einer Linie, in der er nicht stand |
| F7 | Text und Code weichen ab | Tut der Code, was Einstellungstext, Kommentar oder Konzept sagen? | A712: Auffüllen gegen einen stehenden Bonus |
| F8 | Schlüssel und Kodierungen | Können zwei Bedeutungen denselben Schlüssel treffen; passt der Wertebereich? | A710: Kampfzeilen neben Brettzeilen in einer Datei |
| F9 | Gelernte Höchstwerte altern nicht | Was geschieht, wenn die Wirklichkeit leichter wird? | TODO „Crucible: Gelernte Lasten sinken nicht …" |
| F10 | Nebenläufigkeit | Wird eine Sammlung vom Spiel-Thread und von einem Pool-Thread berührt, ohne Sperre? | A224 |
| F11 | Reihenfolge der Zweige | Verdrängt ein früher Zweig einen späteren, der wichtiger ist; läuft ein Pfad nie? | A693: Swiftcast hinter einem anderen oGCD |
| F12 | Annahme über das Spiel ohne Beleg | Welche Spielmechanik setzt die Stelle voraus, und wo ist sie belegt? | A707/C183 |

## Stand

### Mechanische Durchgänge (alle Dateien)

| Durchgang | Werkzeug | Stand | Ergebnis |
|---|---|---|---|
| Compiler mit allen Analysen (`AnalysisMode=All`) | `dotnet build`, 12.11.2026, 3b49604d8 | durch | rund 1.500 Meldungen, fast alle Stil und Benennung. Geprüft: tote Bedingungen (CA1508, 21 Stellen: Schutzabfragen, die der Compiler für überflüssig hält; zwei auffällige am Code gelesen, ohne Wirkung), Freigaben (CA2000, 9: geteilte Texturen von Dalamud, kein Besitz), ungenutzte Felder (CA1823: `MarkingHelper._lock`, drei in Churin-Rotationen), kulturabhängiges Parsen (keines). Kein Defekt. |
| Verschluckte Ausnahmen | Musterdurchgang, 12.11.2026 | durch | 12 leere `catch`, zwei auf Fork-Zeilen (`TankbusterForecast`, `BestiaryTrace`), beide mit Grund im Kommentar. Kein Defekt. |
| Division durch `MaxHp` ohne Schutz | Musterdurchgang, 12.11.2026 | durch | 6 Stellen, drei auf Fork-Zeilen; alle mit Schutz weiter oben oder nur in Texten. Kein Defekt. |
| Sammlung in der eigenen Aufzählung geändert | Musterdurchgang, 12.11.2026 | durch | 1 Treffer, falsch positiv (Kopie). |
| Ereignis abonniert, nie abbestellt | Musterdurchgang, 12.11.2026 | durch | kein Treffer. |

Dazu laufen bei jedem Commit die 53 Prüfschritte der CI (`run_ci_checks.py`), darunter Struktur, Basisaufrufe,
gelernte Dateien (geschrieben, gelesen, angezeigt), Blattsprache, feste Werte, Konzeptverweise.

### Lesender Durchgang (Fork-Code nach Bereich)

Zeilen: gesamt / vom Fork hinzugefügt (12.11.2026). Stand: offen, in Arbeit, gelesen (Datum, Commit).

| Bereich | Dateien | Zeilen | Fork | Stand |
|---|---|---|---|---|
| `Basic/Helpers` | 51 | 22.094 | 15.663 | gelesen (12.11.2026): die Fork-Dateien ganz (A713 bis A726); in `StatusHelper`, `ObjectHelper`, `TargetFilter` die seit der Tiefenprüfung vom 29.09. neuen Abschnitte (ohne Befund; „Interrupt enemy casts" heißt im Code `InterruptibleMoreCheck` – aus bedeutet richtig „nicht unterbrechen") |
| `Basic/Rotations` | 53 | 39.201 | 14.541 | gelesen (12.11.2026): alle Fork-Zeilen – die Beastmaster- und Crucible-Dateien (A714 bis A721), die Rotationsbasis `CustomRotation_*` (Tankbuster, Abwehr-Zurückhalten, OtherInfo, Fähigkeit, GCD, Limit Break, Gegenstände, Aufruf, Grundinfos), die Job-Grundlagen (Beschwörer, Blaumagier samt Carnivale-Set, Maschinist, Ninja, Tänzer, Weißmagier, Weiser, Mönch, Schwarzmagier) und `DeepDungeonRotation`; ohne weiteren Befund (`HostileOutputPercent` nimmt bei Addle den physischen Anteil – gewollt, der Strom eines Packs sind Auto-Angriffe) |
| `Basic` (DataCenter u. a.) | 4 | 5.763 | 3.447 | `DataCenter.cs` gelesen: alle 1.902 Fork-Zeilen ohne Kommentare (12.11.2026, ohne Befund; `ClosingSpeed` ergäbe bei zwei Lesungen im selben Bild null, wird aber je Bild einmal gelesen; die Beschreibung von `BMRIsMoving` auf BossModReborns Bedeutung gebracht); offen `Service.cs` und die zwei übrigen Dateien |
| `Basic/Configuration` | 24 | 6.357 | 3.309 | offen |
| `RebornRotations/Limited Jobs` | 2 | 4.131 | 2.814 | `BST_Reborn.cs` gelesen (12.11.2026, 6ae686837; die Schleifen über `AllHostileTargets` ohne Nullprüfung sind kein Defekt, die Liste trägt konstruktionsbedingt keine leeren Einträge), `BLU_Reborn.cs` gelesen (12.11.2026, ohne Befund); dazu `Duty/DeepDungeonDefault`, `PhantomDefault`, `MonsterHunterDefault` |
| `Basic/Actions` | 18 | 6.939 | 1.945 | gelesen (12.11.2026, ohne Befund): die Fork-Zeilen von `ActionTargetInfo` (bis Zeile 430 der kommentarlosen Fassung von 683; Rest Heilzielwahl gesondert gelesen), `ActionBasicInfo`, `BaseAction`, `HpPotionItem`, `BaseItem`, `PhoenixDownItem`; `IBaseAction`, `ActionSetting` sind Deklarationen |
| `Updaters` | 16 | 5.186 | 1.801 | gelesen (12.11.2026): die Fork-Abschnitte von `TargetUpdater` (A727), `StateUpdater`, `CancelCastUpdater`, `BossModUpdater`, `MajorUpdater`, dazu `NoFallDamageUpdater` (A276, seine Entscheidung), `UpdateCost`; `BlueMageSetTrace` (Spur) nicht gelesen |
| Wurzel (`Watcher`, Plugin) | 3 | 2.212 | 1.468 | `Watcher.cs` gelesen (12.11.2026, 7dc3f8237); `RotationSolverPlugin.cs` offen |
| `Basic/Data` | 40 | 3.912 | 1.176 | offen |
| `RebornRotations/Healer` | 4 | 3.959 | 1.161 | gelesen (12.11.2026, ohne Befund) |
| `RebornRotations/Tank` | 4 | 2.880 | 1.111 | gelesen (12.11.2026, ohne Befund) |
| `RebornRotations/Magical` | 5 | 3.899 | 1.032 | gelesen (12.11.2026, ohne Befund) |
| `UI/MainWindow` | 20 | 6.448 | 1.024 | offen |
| `UI/ExtraWindows` | 18 | 5.894 | 663 | offen |
| `Helpers` | 4 | 544 | 447 | offen |
| `RebornRotations/Melee` | 6 | 4.560 | 447 | gelesen (12.11.2026, ohne Befund; dazu die Fork-Zeilen von `Ranged/MCH`, `BRD`, `DNC`) |
| übrige (je unter 300 Fork-Zeilen) | 138 | 37.391 | 1.477 | offen |

### Befunde

| Befund | Klasse | Wo | Stand |
|---|---|---|---|
| Hornwahl-Messung belohnt kurze Einsätze | F2 | `CrucibleBoard.LearnFamiliarDps` | behoben, A711 |
| Wegtest lernt mit den Mitteln, die er einspart | F1, F5 | `CrucibleBoard.LearnNetAndDip`, `CarriesToFinal` | behoben, A709 |
| Auffüllen gegen einen stehenden Gegenstandsbonus | F7 | `BeastmasterCrucibleItems.SpareHeal` | behoben, A712 |
| Reserve misst mit dem größten Verlust des Bretts | F2 | `BST_Reborn.HeldForFinal`, `ShortForFinal` | behoben, A710 |
| Gelernte Lasten sinken nicht | F9 | `CrucibleBoard.Learn*` | offen, TODO |
| Zufallsfeld wird Kampf: die Brettposition blieb auf dem vorigen Feld; an drei Gabelungen mit Schatz auf beiden Seiten stand sie auf dem falschen Abzweig | F4 | `CrucibleRoute.NoteBattle`, `NoteSpace` | behoben, A715 (Modell `crucible_route_model.py`) |
| Zwei Maße für dieselbe Frage „tragen die LP den Weg": K15 misst Nettoverlust und Senke (A709), die Wegempfehlung zählte den Bruttoschaden und nur den Stand nach dem Endkampf | F5 | `FollowCrucibleRoute.Apply`, `IsSafe` | behoben, A721; Rast, Laden und überzählige Heilung bleiben beim Brutto (dosieren Mittel, A709) |
| Modell zu A709 zog die eigene Heilung mit der Meldung ab: Senken der Kämpfe mit Heiltrank zu tief, Zahlen in Konzept, Archiv und Release-Text falsch (der Code maß richtig) | F1 | `netdip.py` (Notizordner) | berichtigt, A721, C186 |
| Heiler-Kriterium der Wegempfehlung und von „keine Rast nötig" unerfüllbar, sobald ein Einsatz mehr kostet, als der kleinste Endkampf-Heiler LP hat (Lauf 3, 15:31:20; 522 gegen Puk 521) | F3 | `FollowCrucibleRoute.IsSafe`, `NeedsNoRestOrFeed` | behoben, A716 |
| LP der Endkampf-Heiler aus dem Teamfenster gelesen, das einen Kampf nachhängt (A585) | F4 | `FinalHealerShare`, `ReadFeedContext` | behoben, A716 |
| Reraise „ging ohne Sturz", weil er das Brett verließ: danach ein Reraiser nur noch gegen einen Boss, die ganze Sitzung; derselbe Weg über „Turn off when you die" | F4 | `BeastmasterRotation.FollowReraise` | behoben, A714 |
| Am Ende des Endkampfs ersetzte das Bestien-Ausdauermittel die Heilung für den Begleiter und ging auf ihn | F11 | `UseCrucibleItems` | behoben, A717 |
| Elemente eines Begleiters einmal je Sitzung gemerkt | F4 | `BeastmasterRotation.ElementsOf(FamiliarProfile)` | behoben, A718 |
| Tötungsstapel einer Gegenstandstötung: Stapel im Bild des Falls gelesen, die Gewährung kann schon darin stehen | F12 | `CrucibleBoard.FollowKillStacks` | behoben, A719 |
| Gemeldete eigene Heilung blieb ohne Ablauf vorgemerkt und nahm später fremde Heilung mit | F4 | `CrucibleBoard.FollowPlayerHp` | behoben, A720 |
| Treffer der Begleiter über Sitzungen summiert, gegen die Erwartung von heute gehalten | F9 | `CrucibleBoard.LearnFamiliarHits` | offen, TODO „Crucible: Gelernte Lasten sinken nicht, wenn die Begleiter stärker werden" |
| Last je Kampf im Crucible zählte nur Treffer, nicht die Ticks von Schaden über Zeit (Elitekampf Brett 1: rund die Hälfte fehlte) | F2 | `Watcher.ActorControlDetour`, `CrucibleBoard.RecordTaken` | behoben, A722 |
| Gefahr je Sekunde eines Gegners ohne seinen Schaden über Zeit, entgegen Konzept 03 | F7 | `EnemyDanger`, `Watcher` | behoben, A723 |
| „Gegner läuft zu einem anderen Gegner" traf jeden, der auf den Spieler am Hauptgegner zulief; Ziel verlangte Anvisierbarkeit; Verschwinden am Kampfende als Ankunft gelernt (vier Fehlauslösungen von Zähnefletschen am 11.11.) | F6, F7, F12, F2 | `Approach`, `BST_Reborn` K9 | behoben, A724 (Modell `approach_model.py`) |
| „Die Sperre hält BossMods KI nicht" galt nach einer Messung für die ganze Sitzung, gemessen an `Movement.IsMoving`, das auch zurückgehaltene Zauber meldet; ein Rückstoß im Kanal zählte als Schritt der KI | F12, F4 | `MovementLock.Note` | behoben, A725 |
| „Im Rückstand" bei Figuren, die nur Heilung besiegt: die Rate ohne die schon Vollgeheilten – Nichtheiler sprangen ein, sobald die erste Figur fertig war | F7 | `HealToDefeat.UpdateDeadline` | behoben, A726 |
| Gelernte Trefferserien wachsen nur: nach einer kürzeren Serie gelten bis zum Ende der Glockendauer Auslösungen als ausstehend | F9 | `HitSeries` | offen, TODO „Gelernte Trefferserien sinken nicht" |
| Schadensrate der Gruppe nur aufgezeichnet, solange anvisierbare Gegner da sind: in einer Phase ohne sie sah die vorausschauende Heilung niemanden fallen | F6 | `TargetUpdater.UpdateTimeToKill` | behoben, A727 |
| Fehlgeschlagene Wirktext-Auswertung für die Sitzung gemerkt; Steinnamen je Frame aus den Blättern; verrutschte Methodenbeschreibung | F4, – | `ActionTooltip.Text`, `DeepDungeonStones`, `TankbusterHelp` | behoben, A726 |
