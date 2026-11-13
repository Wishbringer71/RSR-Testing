# 24 · Code: Struktur und Rechenkosten

**Leitfrage:** Wo kann der Code modularer, allgemeiner und sparsamer an Rechenzeit und Speicher werden, ohne dass sich
im Kampf etwas verschlechtert – und woran wird das gemessen?

Sein Auftrag (13.11.2026): „im vollen kritischen loop prüfen, wie man den code weiter optimieren kann, ohne seinen
funktionsumfang zu verschlechtern: opjectorientiert, modular, rednundanten code, allgemeine funktionen mit mehr
austauschbaren parametern, weniger ressourcenverbrauch (cpu, speicher), du hast u.a. die berechnung schon auf ein bild
reduziert. wo ist sowas variabel möglich? recherchiere nach programmierrichtlinien und standards zur codeoptimierung.
dann erstelle ein umfassendes konzept. audit, codereview, fehlerbeseitigung". Die Laufzeitregeln des Frames stehen in
Konzept 03, „Was ein Frame kostet"; dieses Konzept trägt Maßstab, Messung und die Entscheidungen zur Struktur.

## Maßstab

- **ISO/IEC 25010** (Produktqualität): *Leistungseffizienz* mit Zeitverhalten, Ressourcennutzung (CPU, Speicher) und
  Kapazität; *Wartbarkeit* mit Modularität, Wiederverwendbarkeit, Analysierbarkeit, Änderbarkeit und Testbarkeit. Der
  Auftrag nennt genau diese: „modular", „allgemeine Funktionen" (Wiederverwendbarkeit), „redundanter Code"
  (Änderbarkeit), „CPU, Speicher" (Ressourcennutzung). Quelle: Sekundärliteratur zur Norm (SonarSource, TMAP); der
  Normtext selbst liegt nicht vor.
- **Microsoft, .NET** (Learn, „Reduce memory allocations using new C# features", „Writing Large, Responsive .NET Framework
  Apps", „Code quality rules"): Leistungsarbeit nur auf *heißen Pfaden*, die oft laufen; vorher eine Basislinie
  messen, nach jeder Änderung erneut. Allokationen kosten Speicherbereinigung; LINQ, Boxing über Schnittstellen und
  Abschlüsse sind ihre häufigsten unsichtbaren Quellen. Die Analyzer der Kategorien Performance (CA18xx) und
  Maintainability (CA15xx) prüfen das mechanisch.
- **Framework Design Guidelines** (Microsoft, nach Cwalina/Abrams): Werttyp statt Referenztyp nur für kleine,
  unveränderliche Daten; die öffentliche Schnittstelle einer Bibliothek ist ein Vertrag.
- **Refactoring** (Fowler): die Gerüche *Duplicated Code*, *Large Class*, *Long Method*; die Behandlungen *Extract
  Function*, *Parameterize Function* (eine Funktion mit austauschbarem Parameter statt mehrerer Kopien), *Move Function*.
- **Spielschleife:** Dalamuds `Framework.Update` läuft im Spielthread, jeden Frame; bei 60 Bildern je Sekunde bleiben
  16,7 ms für Spiel und alle Plugins. Spiel-Engines empfehlen null Allokationen je Frame im Dauerbetrieb (Unity,
  „Garbage collection best practices"; Werkzeugquelle, nicht für Dalamud geschrieben).
- **Grenzen dieses Repositorys:** LINQ ist gesperrt (`BannedSymbols.txt`); keine festen Werte (`code.md`); öffentliche
  Signaturen in `RotationSolver.Basic` sind Vertrag mit Autoren abgeleiteter Rotationen; jede Fork-Zeile in einer Datei
  von Upstream ist Merge-Logistik (Skill `upstream-sync`); der Funktionsumfang bleibt (sein Auftrag). Knuths Satz von
  der verfrühten Optimierung gilt hier als Messgebot: Ohne Messung, welcher Teil trägt, wird nicht umgebaut (A310).

## Stand der Codebasis

Gemessen am 13.11.2026.

**Laufzeit**, aus den Kostenzeilen seiner Protokolle (`DefenseTrace.log`, 759 Kämpfe vom 25.10. bis 12.11.2026):

| Größe | Wert |
|---|---|
| Mittel je Durchlauf, über alle Kämpfe gewichtet | 0,35 ms |
| Anteil an der Framezeit, Median der Kämpfe | 3,8 % |
| Mittel je Schritt | Rotation 0,25 ms · Ziele 0,13 · Zustand 0,07 · übrige je unter 0,04 |
| Durchläufe über einem Frame (16,7 ms) | 10 verschiedene, 21 bis 64 ms |
| davon im ersten Kampf der Sitzung | 8; die anderen zwei im ersten Kampf auf einem neuen Pfad (vierter Kampf im Turm von Babil, 41. Kampf im Crucible) |
| davon mit Speicherbereinigung | keiner |
| langsamste Durchläufe mit Speicherbereinigung | 451 von 759, Median 3,9 ms, höchstens 15,1 ms |

Was das im Kampf heißt: Im Dauerbetrieb kostet RSR weniger als ein Zwanzigstel eines Frames, die
Speicherbereinigung läuft oft mit und bleibt unter einem Frame. Spürbar ist nur der erste Pull einer Sitzung: Ein
Durchlauf von bis zu 64 ms hält das Bild für bis zu vier Frames an (Thok ast Thok, 11.11.2026, 45 ms davon im Schritt
der Rotation). Das ist die technische Schuld „Der erste Kampf einer Sitzung hält einen Durchlauf bis 170 ms an".

**Statisch**, am Quelltext (Stand b0ab17a15):

- *Analyzer* (`AnalysisModePerformance` und `AnalysisModeMaintainability` auf `All`): 643 Befunde, 64 davon auf
  Fork-Zeilen. Inhaltlich zählen: CA1508 (Bedingung immer gleich, 3 Fork-Stellen), CA1822 (könnte statisch sein, 2),
  CA1875 (`Regex.Count`, 1), CA1852 (versiegeln, 10), CA1802 (Konstante statt `static readonly`, 27), CA1805 (unnötige
  Vorbelegung, 11), CA1819 (Eigenschaft gibt ein Feld zurück, 9). Auf Upstream-Zeilen 579, davon 75 in fremden Rotationen.
- *Doppelter Code* (jscpd 4.0.5, ab 70 Token, ohne `ExtraRotations`): 946 von 78.647 Zeilen, 1,2 %, in 45 Klonen; die
  meisten in Upstreams PvP-Rotationen und Konfigurationsklassen. Mit Fork-Zeilen: die Brettsuche in `CrucibleRoute`
  (sechs Fassungen derselben Breitensuche), zwei Schleifen in `RaiseProtection`, das Fensterverhalten von
  `DiagnosticsWindow` gegen zwei Fenster von Upstream, zwei Stellen in `PCT_Reborn`.
- *Fork-Zeilen in Dateien von Upstream:* `DataCenter.cs` 3.581 von 5.609 Zeilen, `ActionTargetInfo.cs` 1.142,
  `ObjectHelper.cs` 1.149, `CustomRotation_OtherInfo.cs` 1.191, `StatusHelper.cs` 1.156. Davon in ganzen Fork-Membern
  (Methode, Eigenschaft, Feld, die Upstream nicht kennt): 2.873, 164, 974, 1.070 und 931 Zeilen.
- *Speicher:* 40 statische Sammlungen mit einer Objekt-Id als Schlüssel; 36 räumen auf (Kampfende, Gebietswechsel oder
  Verfall). Ohne Räumung: `LastStrikeByEnemy` (geräumt in `TargetUpdater`), `_parriedTraced` (räumt über `RemoveWhere`),
  `_lastHp` (Upstream, je gesehenem Gruppenmitglied ein Eintrag) und `_tankbusterHelpHeld` (Fork, A373: je Gegner und
  Aktion ein Eintrag, nie entfernt).

## Die Regeln im Einzelnen

### Der erste Pull: Übersetzung beim ersten Aufruf

*Befund.* .NET übersetzt eine Methode beim ersten Aufruf (JIT). Der erste Kampf ruft hunderte Regeln zum ersten Mal,
im Spielthread, und das Bild wartet. Belegt ist das Muster (erster Kampf, erster Pfad, ohne Speicherbereinigung), nicht
die Ursache: Auch ein Blatt des Clients, das beim ersten Zugriff von der Platte geladen wird, kostet nur einmal.
*Gebaut zuerst: das Messmittel.* Die Kostenzeile nennt je Kampf die Übersetzungszeit und die Zahl übersetzter Methoden im
Spielthread, die Zeile des langsamsten Durchlaufs dasselbe für ihn (`System.Runtime.JitInfo`, je Thread). Damit
entscheidet sein nächster Upload, welcher Anteil eines Rucklers Übersetzung ist.

*Optionen:* (0) nichts; (1) Vorübersetzen im Hintergrund; (2) ReadyToRun, also vorübersetzte Bibliotheken; (3) die
gestufte Übersetzung abschalten. *Abwägung im Kampf:* (0) lässt den Ruckler am Pull, der genau dann fällt, wenn der
Tank zieht und der erste GCD zählt. (2) entfernt ihn ganz, aber Dalamud lädt die Plugin-Bibliothek selbst und baut sie
mit seinem Packager; ob ein ReadyToRun-Abbild dort angenommen wird, ist ungeprüft und hier nicht prüfbar. (3) übersetzt
jede Methode beim ersten Aufruf voll optimiert – teurer, der Ruckler würde länger. (1) übersetzt beim Laden auf einem
Hintergrundthread jede Methode des Kerns (alles außer Oberfläche und Rotationen) und beim Wechsel der Rotation deren
Klasse samt Basisklassen; der Spielthread findet sie fertig vor. *Gewählt:* (1). *Gemessen* an der gebauten Bibliothek
(Testprogramm, das dieselbe Auswahl übersetzt, 13.11.2026, Linux-Container): der Kern 6.959 Methoden in 0,5 s, keine
abgelehnt; die Rotation des Weißmagiers mit Basisklassen 1.083 Methoden in 70 ms – im Mittel 0,07 ms je Methode. Ein
erster Pull, der einige hundert Regeln zum ersten Mal ruft, kommt damit auf die gemessenen 20 bis 64 ms; das stützt die
Hypothese, belegt sie nicht. *Preis:* einmalig eine halbe Sekunde eines Kerns beim Laden und unter 0,1 s je
Rotationswechsel, im Hintergrund mit niedriger Priorität; Speicher für Maschinencode, den der Kampf sonst später anlegen
würde, dazu der für Methoden, die nie laufen (nicht gemessen; bei einigen hundert Byte je Methode wenige MB). Das
Protokoll nennt Zahl und Dauer („warm-up: …").
*Falsifikation.* Kein Defekt? Ein Bild, das 64 ms steht, sieht er am Pull; die Zeile belegt es achtmal. Option falsch?
Ist der Ruckler das Laden eines Blatts, nicht Übersetzung, zeigt die neue Kostenzeile wenig JIT-Zeit im langsamsten
Durchlauf; dann ist der nächste Schritt das Vorladen der Blätter. Ausgeliefert, und nichts ändert sich? Möglich, wenn
der Kampf vor dem Ende des Vorübersetzens beginnt oder der Ruckler in Bibliotheken liegt, die RSR nicht übersetzt
(ECommons, Dalamud); beides zeigen die Zeilen „warm-up" und die JIT-Zahl des langsamsten Durchlaufs.

### Ein Ergebnis gilt, bis sich seine Grundlage ändert

*Der Grundsatz* hinter „auf ein Bild reduziert": Eine Antwort wird so lange behalten, wie sich das nicht ändert, wovon
sie abhängt. Hängt sie am Zustand der Figuren, ist das der Frame (`ObjectHelper.ForgetTimeToKill`, Konzept 03). Hängt
sie an einem Ereignis – ein betretenes Feld, ein Kampf, ein geladener Plan –, ist es die Fassung dieses Zustands, und
die Antwort gilt über viele Frames. Variabel ist damit der *Schlüssel*, nicht der Mechanismus.

*Angewandt:* `CrucibleRoute.CampsiteAhead` wird außerhalb des Kampfes jeden Frame gefragt (im Schlüssel des
Endkampfplans, `BST_Reborn.RefreshFinalPlan`); vorher legte jede Frage zwei Sammlungen an und durchlief das Brett. Die
Antwort hängt am Brett, am Feld und daran, ob es erledigt ist – genau diese drei sind ihr Schlüssel, und sie gilt, bis
sich einer ändert.
*Nicht angewandt, mit Grund:* die Erkennungsabfragen von `DataCenter` und `HostileTarget` (Konzept 03, „Nicht
umgestellt"): Ohne Messung, welcher Teil trägt, bleibt A310. Die Kostenzeile mit Übersetzungszeit trennt jetzt den
einmaligen vom dauernden Anteil; ein dauernder Anteil über einem Frame wäre der Beleg.

### Eine Suche, austauschbare Bedingungen

`CrucibleRoute` durchlief das Brett in sechs Fassungen derselben Breitensuche, je mit eigener Fund- und
Abbruchbedingung. Jetzt eine Suche (`Walk`) mit einem Besucher, der je Feld sagt: weiter, hier nicht weiter oder fertig.
Im Kampf ändert sich nichts; eine Korrektur an der Suche gilt für alle sechs Fragen. `RaiseProtection` lief zweimal
über die wirkenden Gegner mit fast gleicher Auswahl; jetzt eine Schleife mit der Auswahl als Parameter – eine
statische Bedingung, die ihre Werte als Zustand bekommt, damit der allgemeine Weg je Frage nichts anlegt.
*Nicht angewandt:* das Fensterverhalten der Infofenster – zwei der drei Fenster gehören Upstream, und eine gemeinsame
Hilfe nur im Fork-Fenster spart nichts.

### Fork-Code in eigenen Teildateien

*Befund.* Rund 5.850 Zeilen ganzer Fork-Member stehen in fünf Dateien von Upstream. Jede Änderung von Upstream neben
ihnen ist beim Sync ein möglicher Konflikt, und wer eine Regel des Forks sucht, sucht in 5.600 Zeilen.
*Gewählt:* Die Klassen werden `partial`; ganze Fork-Member ziehen in Teildateien je Thema, benannt nach dem Konzept, das
sie tragen (etwa `DataCenter.AreaHits.cs` für Konzept 13). Keine Signatur ändert sich, kein Aufrufer. In der Datei von
Upstream bleibt ein Wort Fork-Änderung je Klasse (`partial`) neben den Änderungen in Upstreams eigenen Membern.
*Grenze:* Statische Felder, deren Startwert ein anderes statisches Feld liest, bleiben in ihrer Datei – die Reihenfolge
der Startwerte über Teildateien ist nicht festgelegt (C#-Spezifikation, „Static field initialization"); ein Feld, das
dabei `null` läse, wäre ein Absturz beim Laden.
*Preis:* ein großer, rein verschiebender Commit; ein Sync, der zeitgleich neben einem verschobenen Block ändert, kann
einmal einen Konflikt bringen. *Reihenfolge:* nach den kleinen Punkten, je Datei ein Commit, jeweils kompiliert und mit
allen Prüfungen.

### Speicher

`_tankbusterHelpHeld` (Fork) behält je Gegner und Aktion einen Zeitpunkt, gelesen nur innerhalb eines GCD; ältere
Einträge räumt jetzt jeder neue. `_lastHp` (Upstream) wächst um ein Mitglied je gesehenem Mitglied, einige KB über
eine lange Sitzung: unverändert, kein Nutzen im Kampf gegen eine Zeile Merge-Logistik.

### Analyzer auf Fork-Zeilen

- *Behoben:* `MainWindow_Debug` las ein Aktionsblatt mit `GetRow`, das für eine unbekannte Id eine Ausnahme wirft;
  die Prüfung auf `null` danach war tot (CA1508) – jetzt `GetRowOrDefault`. Versiegelt: zehn Fork-Klassen (CA1852);
  `DRK_Reborn.GroupStunRunning` statisch (CA1822); `Regex.Count` statt einer Trefferliste (CA1875); elf Vorbelegungen
  mit dem Standardwert entfernt (CA1805). Im Kampf ohne messbare Wirkung, ohne Preis.
- *Bleibt:* CA1802 in `Configs.cs` – die Felder sind das Muster, aus dem der Source Generator die Einstellungen baut;
  CA1822 an `RunningSwiftGoesToRaise` – geschützte Eigenschaft, die abgeleitete Rotationen lesen;
  CA1819 in `StatusHelper` – die Felder sind öffentliche Schnittstelle für abgeleitete Rotationen; die übrigen
  CA1508 auf Fork-Zeilen sind falsch positiv (`Player.Object` ist beim Login `null`, obwohl die Signatur es nicht sagt).
- *Nicht angefasst:* 579 Befunde auf Upstream-Zeilen und die Klone in Upstreams PvP-Rotationen – keine Wirkung im
  Kampf, jede Zeile Merge-Logistik; PvP liegt außerhalb seines Profils. CA1851 in `ActionTargetInfo`: Die Folge ist an
  diesen Stellen schon eine Liste, der zweite Lauf kostet keine neue Auswahl.
- *Keine Prüfung in der CI:* Ein zweiter Build mit allen Regeln kostete jeden Lauf rund 40 s für Befunde ohne Wirkung im
  Kampf; die Zahlen hier sind datiert und werden beim nächsten Audit neu erhoben.

## Wie es gemessen bleibt

- Nach jedem Kampf die Kostenzeile mit Übersetzungszeit; nach dem Laden und jedem Rotationswechsel die Zeile
  „warm-up". Ein langsamster Durchlauf über einem Frame mit wenig Übersetzungszeit ist der nächste Befund.
- Analyzer und Klonsuche wie oben (Befehle im Archiv, A739), bei jedem Audit neu.
