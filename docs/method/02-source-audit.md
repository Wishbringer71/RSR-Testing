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
| `Basic/Helpers` | 51 | 22.094 | 15.663 | in Arbeit: `CrucibleBoard.cs`, `CrucibleRoute.cs` gelesen (12.11.2026, 6ae686837) |
| `Basic/Rotations` | 53 | 39.201 | 14.541 | in Arbeit: gelesen `BeastmasterCrucible.cs`, `…Route.cs`, `…Score.cs`, `…Camp.cs`, `…Items.cs` (12.11.2026, 6ae686837) |
| `Basic` (DataCenter u. a.) | 4 | 5.763 | 3.447 | offen |
| `Basic/Configuration` | 24 | 6.357 | 3.309 | offen |
| `RebornRotations/Limited Jobs` | 2 | 4.131 | 2.814 | in Arbeit (wie oben) |
| `Basic/Actions` | 18 | 6.939 | 1.945 | offen |
| `Updaters` | 16 | 5.186 | 1.801 | offen |
| Wurzel (`Watcher`, Plugin) | 3 | 2.212 | 1.468 | offen |
| `Basic/Data` | 40 | 3.912 | 1.176 | offen |
| `RebornRotations/Healer` | 4 | 3.959 | 1.161 | offen |
| `RebornRotations/Tank` | 4 | 2.880 | 1.111 | offen |
| `RebornRotations/Magical` | 5 | 3.899 | 1.032 | offen |
| `UI/MainWindow` | 20 | 6.448 | 1.024 | offen |
| `UI/ExtraWindows` | 18 | 5.894 | 663 | offen |
| `Helpers` | 4 | 544 | 447 | offen |
| `RebornRotations/Melee` | 6 | 4.560 | 447 | offen |
| übrige (je unter 300 Fork-Zeilen) | 138 | 37.391 | 1.477 | offen |

### Befunde

| Befund | Klasse | Wo | Stand |
|---|---|---|---|
| Hornwahl-Messung belohnt kurze Einsätze | F2 | `CrucibleBoard.LearnFamiliarDps` | behoben, A711 |
| Wegtest lernt mit den Mitteln, die er einspart | F1, F5 | `CrucibleBoard.LearnNetAndDip`, `CarriesToFinal` | behoben, A709 |
| Auffüllen gegen einen stehenden Gegenstandsbonus | F7 | `BeastmasterCrucibleItems.SpareHeal` | behoben, A712 |
| Reserve misst mit dem größten Verlust des Bretts | F2 | `BST_Reborn.HeldForFinal`, `ShortForFinal` | behoben, A710 |
| Gelernte Lasten sinken nicht | F9 | `CrucibleBoard.Learn*` | offen, TODO |
| Zufallsfeld wird Kampf: die Brettposition bleibt auf dem vorigen Feld, bis ein fester Kampf folgt (der Kampf steht in den Blättern nur als Option des Zufallsfelds; Bretter 2 bis 5) | F4 | `CrucibleRoute.NoteBattle` | belegt an den Blättern, Loop offen |
| Zwei Maße für dieselbe Frage „tragen die LP den Weg": K15 misst Nettoverlust und Senke ohne eigene Heilung (A709), Wegempfehlung, Rastwahl, Laden und „Heilung, die kein Kampf braucht" den Bruttoschaden (A661) | F5 | `FollowCrucibleRoute.Apply`, `StretchNeed`, `SpareHeal`, Laden | belegt am Code, Loop offen |
| Heiler-Kriterium der Wegempfehlung und von „keine Rast nötig" unerfüllbar, sobald ein Einsatz mehr kostet, als der kleinste Endkampf-Heiler LP hat: „kein Weg ist sicher" bei 26 % eigenen LP (Lauf 3, 15:31:20; 522 gegen Puk 521) | F3, F2 | `FollowCrucibleRoute.IsSafe`, `NeedsNoRestOrFeed`, `FinalHealerShare` | belegt am Protokoll, Loop offen |
| LP der Endkampf-Heiler aus dem Teamfenster gelesen, das einen Kampf nachhängt (A585): die Wegempfehlung nach einem Kampf rechnet mit den LP von davor | F4 | `FinalHealerShare`, Laden (`BeastmasterCrucibleShop` 1629, 1756) | belegt am Protokoll, Loop offen |
| Reraise „ging ohne Sturz", weil er das Brett verließ: danach nimmt RSR einen Reraiser nur noch gegen einen Boss, die ganze Sitzung (15:09:22; am 11.11. bis zum Endkampf gehalten) | F4 | `BeastmasterRotation.FollowReraise` | belegt am Protokoll, Loop offen |
| Am Ende des Endkampfs ersetzt das Beast Tonic die Heilung für den Begleiter („Unversehrte Bestien") und geht auf ihn: sein Maximum steigt, er ist nicht voll, beide Boni weg | F11 | `UseCrucibleItems`, Schritt „Tonic vor der Heilung" | belegt am Code, Loop offen |
| Elemente eines Begleiters einmal je Sitzung gemerkt, die Elemente seiner Aktionen lernen aber aus Treffern nach | F4 | `BeastmasterRotation.ElementsOf(FamiliarProfile)` | belegt am Code, gering (das Blatt nennt sie für Begleiter), Loop offen |
| Tötungsstapel einer Gegenstandstötung: Stapel im Frame des Todes gelesen, die Gewährung kann schon darin stehen | F12 | `CrucibleBoard.FollowKillStacks` | Kandidat, zu prüfen |
| Gemeldete eigene Heilung bleibt ohne Ablauf vorgemerkt und nimmt später fremde Heilung mit | F4 | `CrucibleBoard.FollowPlayerHp` | Kandidat, gering |
| Treffer der Begleiter über Sitzungen summiert, gegen die Erwartung von heute gehalten | F9 | `CrucibleBoard.LearnFamiliarHits`, `FamiliarHitFactor` | Kandidat, zu prüfen |
