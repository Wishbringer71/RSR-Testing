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
der Rotation). Seit A739 gebaut (Abschnitt „Der erste Pull"), im Spiel bestätigt am 13.11.2026 (A765).

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
Tank zieht und der erste GCD zählt. (2) scheidet aus, geprüft am 13.11.2026 (A748): Dalamud lädt jedes Plugin in einen
entladbaren AssemblyLoadContext und aus dem Speicher (`LocalPlugin.SetupLoaderConfig`: `IsUnloadable = true`,
`LoadInMemory = true`, `ManagedLoadContext.LoadAssemblyFromFilePath` dann über `LoadFromStream`; goatcorp/Dalamud
43b34118, 10.11.2026), und in einem entladbaren Kontext „ReadyToRun generated code will be ignored" (Microsoft Learn,
„How to use and debug assembly unloadability in .NET"). Damit fällt auch statisches PGO weg, das seine Profile über
ReadyToRun mitliefert. NativeAOT kennt keine nachgeladene verwaltete Bibliothek. Eine Ablage des Maschinencodes über
Starts hinweg, wie Python ihren Bytecode als `.pyc` ablegt, gibt es in .NET Core nicht – das Gegenstück zu `.pyc` ist die
IL der DLL, die der Build schon erzeugt; den Maschinencode legte nur `ngen.exe` des alten .NET Framework ab
(Baltes-Götz und Frischknecht, „Einführung in das Programmieren mit C# 14", Abschnitt 2.5). Die Optimierung zur
Laufzeit nach gemessenem Verhalten – das, was Visual Studio für C++ als profilgesteuerte Optimierung anbietet – macht
.NET 10 von selbst: Dynamisches PGO ist seit .NET 8 voreingestellt, und Dalamuds Laufzeitkonfiguration
(`Dalamud.runtimeconfig.json`) schaltet weder die gestufte Übersetzung noch PGO ab; es misst und übersetzt in jeder
Sitzung neu, im Hintergrund. (3) übersetzt
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
*Im Spiel bestätigt* (13.11.2026, A765): Auf seinem Rechner übersetzte das Vorübersetzen 10.561 und 10.584 Methoden des
Kerns in 13 bis 15 s im Hintergrund, die Rotation in 0,05 bis 0,5 s, keine abgelehnt. Im ersten Kampf danach kostete
der teuerste Frame 8,8 und 6,8 ms (25 ms Übersetzung über 800 und 3.000 Durchläufe verteilt). Am 12.11.2026, Build
ohne Vorübersetzen, waren es 27,2 ms. Kein Frame des ersten Kampfes überschreitet mehr die Hälfte eines Bilds bei 60
Bildern. Die 13 bis 15 s statt der 0,5 s im Container sind seine Maschine unter dem laufenden Spiel; der Kampf
begann jeweils Minuten danach.

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

### Entscheiden erst, wenn gedrückt werden kann

*Sein Vorschlag (13.11.2026, „nur ein gedankenspiel"):* Wenn bekannt ist, wann der nächste GCD und die nächste Fähigkeit
gehen, genügt es, kurz davor zu entscheiden – unter Beachtung der Rechen- und Entscheidungszeit; jeden Frame zu rechnen,
was verworfen wird, weil noch nichts gedrückt werden kann, ist Arbeit ohne Ergebnis.
*Am Code:* `ActionUpdater.UpdateNextAction` ruft jeden Frame die ganze Entscheidung (`TryInvoke`, Schritt „next action
(rotation)", im Mittel 0,25 ms der 0,35 ms eines Durchlaufs). Erst danach prüft `CanDoAction`, ob gedrückt werden darf:
nicht während eines Zaubers, der länger läuft als der Vorlauf (`CalculatedActionAhead`), und nicht in der
Animationssperre. In diesen Frames wird die Entscheidung gerechnet und verworfen. Fähigkeiten gehen in beiden Lagen
nicht, das Spiel lässt sie nicht zu – es geht also keine Aktion verloren, wenn dort nicht entschieden wird.
*Was gegen das Überspringen spricht:* (1) Die Anzeige der nächsten Aktion (Leiste, Fenster) stünde in diesen Frames still –
im Kampf ohne Wirkung. (2) Regeln, die beim Entscheiden Zustand schreiben (Protokollzeilen einer Wahl, der Grund eines
verfallenen Schlafs, Phasenbücher), sähen weniger Frames; welche das sind, ist vor einem Umbau Member für Member zu
erheben. (3) Der Gewinn ist der Anteil dieser Frames am Rotationsschritt: bei Zauberern mit 1,5 bis 2,5 s Wirkzeit je
2,5 s GCD vermutlich mehr als die Hälfte, bei Nahkämpfern die Animationssperre von etwa 0,6 s je Aktion – geschätzt
0,1 bis 0,15 ms je Frame, 1 bis 2 % der Framezeit (Schluss aus Wirkzeiten, nicht gemessen). Die Ruckler am ersten Pull
(Übersetzung) berührt es nicht.
*Entscheidungszeit:* Der Klickverzug zählt ab dem letzten Klick, nicht ab der Entscheidung
(`RSCommands.CanDoAnAction`), und der Rotationsschritt läuft im selben Durchlauf vor dem Schritt, der drückt. Wer erst
im Frame entscheidet, in dem das Tor aufgeht, drückt also im selben Frame wie heute; einen Vorlauf für die Rechenzeit
braucht es nicht, solange ein Durchlauf unter einem Frame bleibt (Median 3,8 %).
*Die eigentliche Bedingung – Beobachten und Entscheiden getrennt:* Im Entscheidungspfad stehen 29 Zeitstempel in 16
Dateien der Rotationen und 58 in Hilfsklassen, dazu 186 Protokollaufrufe (Zählung 13.11.2026). Ein Teil davon merkt sich,
seit wann eine Lage besteht, etwa das Zurückhalten einer Abwehr (`CustomRotation_DefenseHold`). Würde während eines
Zaubers nicht entschieden, begänne eine solche Frist erst nach ihm, und die Regel ließe im Kampf später los – das
Überspringen wäre keine reine Optimierung. Voraussetzung ist deshalb, dass jede Beobachtung im Schritt läuft, der jeden
Frame rechnet (Ziele, Zustand), und die Entscheidung nur liest – die Regel „Erkennung und Entscheidung trennen"
(`code.md`) verlangt das ohnehin. Erst danach ist Überspringen ohne Wirkung auf den Kampf.
*Gebaut zuerst: das Messmittel* (A744). Die Kostenzeile nennt, wie viele Durchläufe und welchen Anteil der Zeit des
Rotationsschritts das Tor ohnehin sperrte. *Entscheidung danach:* Trägt dieser Anteil die Hälfte des Rotationsschritts
oder mehr, folgt der Loop: zuerst die Beobachtungen aus dem Entscheidungspfad lösen, Member für Member, dann
überspringen; bleibt er klein, lohnt der Umbau nicht, und die Trennung bleibt eine Frage der Entwurfsregel.
*Gemessen (13.11.2026, A765; Weißmagier und Beschwörer, Builds f16ff5976 und dca6fb8e8, 40 Kämpfe):* In 50 bis 55 % der
Durchläufe sperrte das Tor ohnehin, und sie tragen 50 bis 55 % der Zeit des Rotationsschritts. Der Schritt kostet im
Mittel 0,21 bis 0,39 ms je Frame; der Gewinn läge bei 0,1 bis 0,2 ms je Frame, rund 1 % der Framezeit bei 60 Bildern.
Die Bedingung „die Hälfte oder mehr“ ist erfüllt. Der Loop folgt im TODO als offene Arbeit: zuerst die Beobachtungen
Member für Member aus dem Entscheidungspfad lösen.
*Falsifikation des Vorschlags:* Kein Gewinn? Möglich – die Zeile zeigt es. Falsche Option? Überspringen ohne die
Trennung verschöbe Fristen; deshalb nicht gebaut. Ausgeliefert, und nichts ändert sich? Mit dem Messmittel allein so
gewollt.

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
einmal einen Konflikt bringen.
*Umgesetzt (A740):* `DataCenter` in `DataCenter.AreaDamage.cs` (13), `.BlackestNight.cs` (10), `.Channels.cs` (14),
`.HitResponse.cs` (08), `.TankProtection.cs` (09), `.Universal.cs` (03 und ohne Verweis); `ObjectHelper` in `.HealTarget`,
`.HitResponse`, `.Jobs`, `.Raise`, `.Universal`; `StatusHelper` in `.HealTarget`, `.Raise`, `.TankProtection`, `.Universal`;
`CustomRotation_OtherInfo` in `.Channels`, `.HitResponse`, `.Roles`, `.Universal`. Das Thema eines Members ist das Konzept,
das sein Kommentar am häufigsten nennt; ohne Nennung das des Members davor. Fork-Zeilen in den vier Dateien von
Upstream: vorher 7.088, danach 1.431 – der Rest sind Änderungen in Upstreams eigenen Membern und die stehen gebliebenen
statischen Felder mit Startwert. Nachgewiesen: jede Codezeile steht danach genau einmal da (Vergleich der Zeilenmengen
vor und nach), Compile ohne Warnung, alle Prüfungen. Prüfskripte, die eine Klasse lesen, lesen sie samt Teildateien
(`check_learned_readers`, `check_emergency_heal_threat`); `check_invuln_polarity` und `fixed_values.json` nennen die
Teildatei. `ActionTargetInfo` bleibt: Dort sind nur 164 Zeilen ganze Fork-Member, 956 Fork-Zeilen stehen in Upstreams
eigenen Membern.
*Zweite Runde (A746):* `Watcher`, `TargetUpdater`, `CustomRotation_GCD`, `CustomRotation_Ability`, `SummonerRotation`,
`BlueMageRotation` und die Rotationen `WHM_Reborn`, `SMN_Reborn`, `DRK_Reborn`, `WAR_Reborn`, `SGE_Reborn`, `SAM_Reborn`,
3.708 Zeilen in 30 Teildateien. Dazu zwei Grenzen mehr: Kein Startwert wandert, auch kein Instanzfeld (ihre Reihenfolge
über Teildateien ist ebenso offen), und keine Einstellung (`RotationConfig`, `UI`), weil das Einstellungsfenster sie in
der Reihenfolge ihrer Deklaration zeigt. Die Aktionsliste einer Rotation erzeugt der Source Generator aus den
Basisklassen; eine gewanderte Eigenschaft ändert sie nicht. In `fork_placement.json` stehen danach 390 Member.
*Prüfmittel:* Der Matrix-Generator las nur die Hauptdatei und hätte beim Beschwörer Addle als ungenutzt geführt;
`check_marker_lists`, `check_set_lookups`, `check_heal_target_order` und `scan13` hätten still weniger geprüft. Alle
lesen eine Klasse jetzt samt Teildateien (`classfile.read`, Selbsttest), und die Regel steht in `pruefmittel.md`.
*Dritte Runde (A749):* 15 weitere Klassen (darunter `StateUpdater`, `TimeToKillTracker`, `CancelCastUpdater`,
`HpPotionItem`, `BaseItem`, `RotationSolverPlugin` und die Basisklassen von Maschinist, Weißmagier und Tänzer), 857
Zeilen; `fork_placement.json` 390 → 314. Eine Datei, die mit einer Bytefolge (BOM) vor `namespace` beginnt, gab dem
Werkzeug keinen Namensraum mit – die Teildatei lag im globalen Namensraum, und der Build schlug an; das Werkzeug liest die
Zeile jetzt auch hinter einer BOM, und jede Teildatei trägt ihren Namensraum.

### Speicher

`_tankbusterHelpHeld` (Fork) behält je Gegner und Aktion einen Zeitpunkt, gelesen nur innerhalb eines GCD; ältere
Einträge räumt jetzt jeder neue. `_lastHp` (Upstream) wächst um ein Mitglied je gesehenem Mitglied, einige KB über
eine lange Sitzung: unverändert, kein Nutzen im Kampf gegen eine Zeile Merge-Logistik. Die Sammlungen der wirkenden
Gegner (`TargetUpdater.UpdateAreaCasters`: `AreaCasters`, `AreaCastHelperIds`, `CastingEnemies`) werden je Bild geleert
und neu gefüllt, nicht neu angelegt (A750). Ihre Leser lesen sie nur im selben Bild.

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

- Die Arbeitsregeln daraus stehen in `.claude/rules/code.md`, „Struktur und Kosten", und greifen schon im Konzept: Die
  Definition of Ready verlangt für Code seinen Ort und, läuft er je Frame, seine Kosten (CLAUDE.md, A742).
- `check_fork_placement.py` (CI) lehnt einen neuen Fork-Member in einer Datei von Upstream ab. Fork-Member erkennt
  es an ihrer Signatur: Weder die Merge-Basis noch `upstream/main` führen sie in dieser Datei, und Upstream kennt keinen
  Member dieses Namens (eine geänderte Signatur bleibt Upstreams Member; Preis: eine neue Überladung einer Methode von
  Upstream geht unbemerkt durch). Die 563 Member, die am 13.11.2026 in 75 Dateien von Upstream standen, führt
  `fork_placement.json`; die Liste wird nur kürzer, weil ein Member beim nächsten Eingriff in seine Teildatei zieht.
  Ausgenommen: fremde Rotationen und `Configs.cs` (alle Einstellungen in einer Klasse, das Muster von Upstream, das der
  Source Generator liest).

- Nach jedem Kampf die Kostenzeile mit Übersetzungszeit; nach dem Laden und jedem Rotationswechsel die Zeile
  „warm-up". Ein langsamster Durchlauf über einem Frame mit wenig Übersetzungszeit ist der nächste Befund.
- Analyzer und Klonsuche wie oben (Befehle im Archiv, A739), bei jedem Audit neu.
