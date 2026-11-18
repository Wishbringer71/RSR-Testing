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
  Signaturen in `RotationSolver.Engine`, `RotationSolver.Configuration` und den Basisklassen der Rotationen (bis
  Upstream c170c9cd5 `RotationSolver.Basic`) sind Vertrag mit Autoren abgeleiteter Rotationen; jede Fork-Zeile in einer Datei
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
Hintergrundthread jede Methode des Kerns (alles außer Oberfläche und Rotationen; eine Rotation ist, was
`ICustomRotation` umsetzt, nicht ein Namensraum: Seit Upstreams Umbau c170c9cd5 stehen in den Namensräumen der Jobs auch
Hilfstypen wie `FamiliarProfile` und `CarnivalePick`, die sonst erst im ersten Crucible- oder Carnivale-Kampf übersetzt
würden, A878) und beim Wechsel der Rotation deren
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
Die Bedingung „die Hälfte oder mehr“ ist erfüllt.
*Loop danach (A767): nicht gebaut.* Die Prämisse „in diesen Frames geht keine Aktion verloren, wenn nicht entschieden
wird“ hält nicht. In gesperrten Frames lesen andere Stellen die frisch entschiedene Aktion:
- Das Abfangen eines eigenen Tastendrucks (`ActionQueueManager`) vergleicht den gedrückten Knopf mit
  `ActionUpdater.NextAction`. Gedrückt wird gerade während eines Zaubers, für den nächsten GCD. Mit der Aktion von
  vor dem Zauber fiele der Vergleich anders aus: Ein Druck, der zur neuen Entscheidung passt, würde abgefangen.
- Der Zielwechsel nach der nächsten Aktion (`UpdateTargetFromNextAction`, Modus „Target only“ und Lehrmodus).
- Das Fenster „Next Action“ und die Markierung im Lehrmodus. Er sieht sie.
- True North für Nahkämpfer (`StateUpdater.ShouldAddPositional`) liest den nächsten GCD.
*Optionen danach:* (a) überspringen und diese Leser in Kauf nehmen: verworfen, ändert das Abfangen im Kampf. (b) Nur auf
Anfrage entscheiden, wenn ein Leser fragt: Das Fenster fragt jedes Bild, und die 29 plus 58 Zeitstempel im
Entscheidungspfad müssten trotzdem erst heraus. (c) In gesperrten Frames seltener entscheiden: Ein Leser sieht eine
Aktion, die ein Bild älter ist, und die Beobachtungen werden gröber; es spart die Hälfte von 0,1 bis 0,2 ms und
braucht eine neue feste Rate. *Bilanz:* Jede Variante zahlt mit Verhalten oder mit einem Umbau an 87 Stellen. Der
Gewinn ist 1 % der Framezeit, im Kampf nicht zu sehen. Option 0 bleibt. Die Messzeile „rotation step while the gate
would not press“ bleibt: Steigt der Anteil oder der Rotationsschritt deutlich, ist das neu zu prüfen.
*Falsifikation des Vorschlags:* Kein Gewinn? Möglich – die Zeile zeigt es. Falsche Option? Überspringen ohne die
Trennung verschöbe Fristen; deshalb nicht gebaut. Ausgeliefert, und nichts ändert sich? Mit dem Messmittel allein so
gewollt.

### Eine Suche, austauschbare Bedingungen

`CrucibleRoute` durchlief das Brett in sechs Fassungen derselben Breitensuche, je mit eigener Fund- und
Abbruchbedingung. Jetzt eine Suche (`Walk`) mit einem Besucher, der je Feld sagt: weiter, hier nicht weiter oder fertig.
Im Kampf ändert sich nichts; eine Korrektur an der Suche gilt für alle sechs Fragen. `RaiseProtection` lief zweimal
über die wirkenden Gegner mit fast gleicher Auswahl; jetzt eine Schleife mit der Auswahl als Parameter – eine
statische Bedingung, die ihre Werte als Zustand bekommt, damit der allgemeine Weg je Frage nichts anlegt.
Status über die ganze Gruppe und Unverwundbarkeit fragt der Fork über die gemeinsamen Funktionen von `StatusHelper`, die
Upstream 7.5.8.4 eingeführt hat (`AnyPartyMemberHasStatus`, `HasInvincibleStatus`), statt eigener Schleifen über
`StatusList`: der Tank-Limit-Break auf der Gruppe (Konzept 09) und die Unverwundbarkeit eines getroffenen Ziels (Konzept 08).
Im Kampf gleich, ohne ein Objekt je Status. Ebenso lesen die Schleifen, die je Frame über die Status der Gruppe laufen,
die Status über `StatusHelper.GetStatuses` statt über Dalamuds `StatusList` (Code-Review des Merges, 17.11.2026): DoTs
auf der Gruppe (`DotOnParty`), gemessene Heilminderung (`HealReduction`), Heilung anderer (`HealFromOthers`), die Wahl
des Reinigungsziels (`TargetUpdater.HealTarget`) und die Statusbedingung jeder Aktion (`ActionBasicInfo.IsStatusNeeded`,
wie Upstream ohne die zusätzliche Prüfung der Liste). Jede `StatusList` legte ein Objekt für die Liste und eines je Status
an. Dasselbe gilt für jede andere Fork-Stelle, die Status liest: Die meisten laufen ebenfalls je Frame und Figur – Zielwahl
(Stacheln, Parade, Tod beim nächsten Treffer, Schutz auf Gruppenmitgliedern), Betäubungsdauer, Verwundbarkeit und
Schadensbonus, Tankbuster-Vorhersage, Blicke, Reinigungsfrist, Schlaf in den Tiefen Gewölben, Sperre der eigenen
Fähigkeiten –, die übrigen bei einem Ereignis, und keine braucht etwas, das nur Dalamuds Objekt liefert: Name und Flags
des Status kommen aus dem Statusblatt (`TryGetStatusRow`, Clientsprache wie `GameData`), die Quelle aus
`SourceObject.ObjectId`. Alle lesen deshalb `GetStatuses`; wo nur die Bedingung je Status abweicht, über die eine Schleife
`StatusHelper.Carries` mit der Bedingung als Parameter. Nebenbei entfällt ein Absturzweg: Dalamuds Getter ist nie `null`, er
legt die Liste um `GetStatusManager()` an und liest bei einer Figur ohne Statusverwaltung über einen Nullzeiger (goatcorp/
Dalamud, `BattleChara.cs`, 17.11.2026) – die Prüfungen auf `null` an diesen Stellen schützten nie. `GetStatuses` liest das
Feld der Figur selbst und ruft keine virtuelle Funktion; die Zielwahl lässt Figuren ohne Statusverwaltung ohnehin aus
(`HasNativeState`). Dalamuds Weg bleibt nur in Upstreams
Zeilen – der Phantomjob in Kreszentia (`DutyRotation`, gedrosselt) und das Diagnosefenster –, weil jede Fork-Zeile dort
Merge-Logistik wäre und keine je Frame im Kampf läuft.
*Prüfmittel:* `check_status_list.py` weist jede Stelle mit `.StatusList` außerhalb dieser beiden Dateien und der fremden
Rotationen (die der Fork nicht bearbeitet, `layout.is_third_party`) ab.
Anforderung: Ein Upstream-Merge oder eine neue Regel bringt die Schleife sonst unbemerkt zurück – sie kompiliert, antwortet
richtig und kostet nur Framezeit; diese Klasse fand das Code-Review des Merges an fünf Stellen, und 25 weitere standen
noch (17.11.2026). Aufwand ein Textvergleich je Datei.
*Nicht angewandt:* das Fensterverhalten der Infofenster – zwei der drei Fenster gehören Upstream, und eine gemeinsame
Hilfe nur im Fork-Fenster spart nichts.

### Fork-Code in eigenen Teildateien

*Befund.* Rund 5.850 Zeilen ganzer Fork-Member stehen in fünf Dateien von Upstream. Jede Änderung von Upstream neben
ihnen ist beim Sync ein möglicher Konflikt, und wer eine Regel des Forks sucht, sucht in 5.600 Zeilen.
*Gewählt:* Die Klassen werden `partial`; ganze Fork-Member ziehen in Teildateien je Thema, benannt nach dem Konzept, das
sie tragen (etwa `DataCenter.AreaHits.cs` für Konzept 13). Keine Signatur ändert sich, kein Aufrufer. In der Datei von
Upstream bleibt ein Wort Fork-Änderung je Klasse (`partial`) neben den Änderungen in Upstreams eigenen Membern.
*Grenze:* Ein statischer Startwert, der ein anderes statisches Mitglied derselben Klasse liest, steht in derselben Datei
hinter ihm. Startwerte laufen in der Reihenfolge des Texts, über Teildateien in der Reihenfolge, in der der Compiler die
Dateien liest – die C#-Spezifikation legt sie nicht fest („Static field initialization"), sie folgt dem Dateinamen. Ein
Startwert, der dabei `null` liest, wirft im Typinitialisierer, und die Klasse fällt beim ersten Zugriff ganz aus; bei
`DataCenter` oder `StatusHelper` ist das RSR als Ganzes, gleich nach dem Laden. Ein Lambda oder eine übergebene
Methodengruppe liest erst bei ihrem Aufruf und hängt an keiner Reihenfolge. „In ihrer Datei bleiben" genügte nicht:
`DataCenter._channelSince` und `_channelSeen` blieben in `DataCenter.cs`, während `KeptChannels`, dessen Länge sie lesen,
nach `DataCenter.Channels.cs` zog; das ging nur gut, weil diese Datei vor `DataCenter.cs` übersetzt wurde, und brach,
als Upstreams Aufteilung sie nach `DataCenter.BMR.cs` brachte (A878).
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

*Upstreams eigene Aufteilung (c170c9cd5, A878):* Upstream teilte sechs Klassen selbst in Teildateien (`DataCenter` in
`.BMR`, `.Combat`, `.JobAndGcd`, `.Pets`, `.Territory`; `ObjectHelper`, `ActionTargetInfo`, `StatusHelper`, `Configs`,
`CustomRotation_OtherInfo`) und verschob den Namensraum `RotationSolver.Basic`. Jeder Fork-Member ging in die Teildatei
von Upstream, in der sein Nachbar landete, in der Reihenfolge des Forks; die Teildateien des Forks blieben. Ein Werkzeugfehler
setzte dabei 31 Member an den Anfang ihrer Klasse; sie stehen wieder hinter ihrem Vorgänger (A878). Die Reihenfolge der
Einstellungen im Fenster ist je Filtergruppe unverändert (316 von 316, gemessen 18.11.2026); zwei Gruppen reichen über
`Configs.CombatAndTarget.cs` und `Configs.cs`, deren Übersetzungsreihenfolge unter Windows und Linux dieselbe ist –
`Configs.DutiesAndPvP.cs` steht unter Windows vor, unter Linux hinter `Configs.cs`, trägt aber keine geteilte Gruppe.

### Speicher

`_tankbusterHelpHeld` (Fork) behält je Gegner und Aktion einen Zeitpunkt, gelesen nur innerhalb eines GCD; ältere
Einträge räumt jetzt jeder neue. `_lastHp` (Upstream) wächst um ein Mitglied je gesehenem Mitglied, einige KB über
eine lange Sitzung: unverändert, kein Nutzen im Kampf gegen eine Zeile Merge-Logistik. Die Sammlungen der wirkenden
Gegner (`TargetUpdater.UpdateAreaCasters`: `AreaCasters`, `AreaCastHelperIds`, `CastingEnemies`) werden je Bild geleert
und neu gefüllt, nicht neu angelegt (A750). Ihre Leser lesen sie nur im selben Bild.

### Fehlertoleranz: Code, den das Spiel aufruft

*Anforderung (A794).* Kein Fehler in RSR beendet das Spiel. Geschützt wird der Spieler und, in einem Inhalt mit
Gruppe, die Gruppe: Ein Absturz trennt ihn mitten im Kampf, die Gruppe verliert einen Spieler bis zur Rückkehr, im
Crucible ist der Lauf verloren. Sein Auftrag (14.11.2026): „alles mit den änderungen crucible seit dem letzten
erfolgreichen crucible ingame lauf kritisch prüfen. es darf nicht sein, dass der part nun jedes mal crashed und du
immer nur einen kleinen teil anpasst". Seit dem letzten Lauf ohne Absturz (11.11.2026, Build 03af7190f) endete jeder
Versuch im Crucible mit einem Absturz, zweimal beim Lesen von Fensterwerten (A764, A787), einmal durch eine Ausnahme
aus einem Hook (A793). Jede Behebung schloss die eine Stelle, nicht die Klasse.

*Die Klassen und was sie trifft* (ISO/IEC 25010, Zuverlässigkeit: Fehlertoleranz):
1. *Ausnahme in einem Hook.* Ein Hook ersetzt eine Funktion des Spiels; sein Ersatz (Detour) läuft in nativem Code.
   Eine .NET-Ausnahme, die ihn verlässt, beendet das Spiel (A793: Überlauf in `GazeWatch.AddMargins`, Absturz bei der
   ersten Aktion jedes Kampfes). Fängt Dalamud nicht ab: Der Detour wird direkt als Funktionszeiger aufgerufen.
2. *Lesen über einen ungültigen Zeiger* (AccessViolation): von .NET nicht abfangbar; ein try hilft nicht, nur ein
   geprüfter Zeiger oder ein Lesen über `SafeMemory` (A787).
3. *Stapelüberlauf, Endlosschleife*: ebenfalls nicht abfangbar.
4. *Ausnahme in einem Ereignis* (`Framework.Update`, `AddonLifecycle`, ECommons' `ActionEffect`, IPC): Die Quelle fängt
   ab und schreibt ins Protokoll; das Spiel läuft weiter, RSRs Teil fällt für dieses Ereignis aus.

*Gewählt für Klasse 1:* In jedem Detour läuft außerhalb eines try, dessen catch jede Ausnahme nimmt, nur der Aufruf der
Originalfunktion; Locals, Parameter, Literale und Feldlesungen über einen Zeiger der Spielstruktur (`self->Feld`)
dürfen stehen, weil sie keine .NET-Ausnahme werfen. Jeder Aufruf, jeder Konstruktor, jedes gelesene Member außer
`.Original` steht in einem solchen try. Ein `finally` schützt nicht: Was es ausführt, bekommt sein eigenes try. Der
Originalaufruf bleibt außerhalb: In einem try würde eine Ausnahme aus dem Detour eines anderen Plugins weiter unten in
der Kette abgefangen und die Funktion des Spiels danach ein zweites Mal aufgerufen. Ein Fehler wird je Stelle einmal
je Sitzung geschrieben, ins Dalamud-Protokoll mit Stapel und ins `DefenseTrace.log` als „hook fault: <Stelle> …"
(`HookFault.Report`); danach läuft das Spiel ohne RSRs Teil an dieser Stelle weiter.
*Gilt für jeden Detour, auch die von Upstream:* Die Gefahr kommt über die Aufgerufenen. Upstreams Bild-Hook
(`ActionManagerEx.UpdateDetour`) rief vor seinem try `CooldownDelayTweak.StartAdjustment` auf, dessen Grenze der Fork
seit A753 aus `AnimationLockTweak.MeanFrameTime` nimmt; eine Ausnahme dort hätte jedes Bild mit einer gedrückten
Aktion das Spiel beendet. Preis: einige Zeilen Merge-Logistik in vier Dateien von Upstream.
*Optionen:* (0) Nullvariante, jede Stelle bei ihrem Absturz absichern – dreimal gescheitert, jede neue Fork-Zeile in
einem Aufgerufenen öffnet die Klasse wieder. (1) Gewählt: ganzer Detour außer dem Originalaufruf im try, lexikalisch
in der CI geprüft. (2) Die Aufgerufenen transitiv auf Fork-Änderungen prüfen – ein Aufrufgraph über Eigenschaften,
Schnittstellen und Delegaten ist ohne Compiler unvollständig, und eine Lücke bliebe still. (3) Auch den Originalaufruf
in das try – doppelter Aufruf der Spielfunktion (oben). (4) Rückbau: die Hooks des Forks entfernen – nimmt die
Blickwache, die Zeitmessung der Aktionen und den Ausgleich der Bildrate.
*Prüfmittel:* `check_hook_guards.py` (CI) findet die Detours an ihrer Anmeldung (`HookFrom…`, `[EzHook(…, nameof(…))]`),
nicht an ihrem Namen; ein angemeldeter Detour ohne gefundenen Rumpf schlägt an. Selbsttest gegen konstruierte Fehler.
*Falsifikation.* Kein Defekt? Widerlegt: A793 und der ungeschützte Aufruf im Bild-Hook. Option falsch? Ein
abgefangener Fehler lässt einen halben Zustand zurück – geprüft je Detour: die Blickrichtung stellt ein eigenes,
geschütztes `finally` zurück, der Bildratenausgleich wird im `finally` beendet, die übrigen schreiben nur eigene
Messungen. Ausgeliefert, und nichts ändert sich? Möglich, wenn der nächste Absturz aus Klasse 2 oder 3 kommt; dafür die
Prüfung der Zeiger und Schleifen unten. *Vorhersage* (`predictions/2026-11-14.json`, „hook-no-fault"): In keiner
Sitzung mit Kampf steht „hook fault:"; steht eine, nennt sie Stelle und Ausnahme, und das Spiel lief weiter.

*Ursachen, nicht nur Symptome (A798, seine Vorgabe vom 14.11.2026):* „die ausnahme verhindert ja nur das ergebnis crash.
es ist also nur symptombehandlung. ursachenforschung muss dennoch sein, damit ursachen beseitigt werden können." Ein
`catch` hält das Spiel am Leben; die Ursache bleibt. Von 154 Fangstellen in RSR schrieben 60 nur eine Warnung ins
Dalamud-Protokoll, und sein Protokoll vom 14.11.2026 führt keine einzige Warnung (Protokollstufe): Was RSR dort abfing,
war für ihn und mich unsichtbar. *Gewählt:* `ExceptionTrace` meldet jede .NET-Ausnahme, die in RSRs Code entsteht – auch
eine, die später abgefangen wird –, beim ersten Mal je Stelle und Sitzung: im `DefenseTrace.log` („exception: <Typ> in
<Methode> …"), das er hochlädt, und mit Stapel als Fehler im Dalamud-Protokoll, das jede Stufe zeigt. Mittel ist .NETs
`AppDomain.FirstChanceException`; ob die Ausnahme RSR betrifft, sagt der Stapel des werfenden Threads (der erste Rahmen
aus RSRs Assembly). *Optionen:* (0) Nullvariante – Ursachen bleiben unsichtbar. (1) Jede der 154 Fangstellen umbauen –
Merge-Logistik in Upstreams Dateien, und eine neue Fangstelle fällt wieder heraus. (2) Gewählt: eine Stelle für alle,
auch künftige. *Kosten:* ein Stapel je geworfener Ausnahme im Prozess (alle Plugins teilen ihn); Ausnahmen sind selten,
eine je Bild wäre selbst ein Befund. *Sicherheit:* Der Melder fängt alles selbst ab und ruft sich nicht erneut auf (eine
Ausnahme im Melder würde sonst wieder gemeldet). *Vorhersage* („exception-none", `predictions/2026-11-14.json`): In einer
Sitzung mit Kampf steht keine Zeile „exception:"; steht eine, ist sie eine Ursache zum Beseitigen.

*Klassen 2 bis 4 über alle Änderungen seit 03af7190f* (unabhängige Prüfung am Code, A795; Fremdquellen: ECommons
3.2.1.26, Dalamud 15.0.3.9, FFXIVClientStructs 80cb4a7b, Werkzeugquellen):
- *Ereignisquellen fangen ab:* ECommons' `ProcessActionEffectDetour` je Abonnent, Dalamuds `AddonLifecycle` je Listener
  (`InvokeListenersSafely`), `Framework.Update` je Delegat (`ProfileAndInvoke`), `UiBuilder.Draw`, die IPC über
  `EzIPC` mit `SafeWrapper.AnyException`. Eine Ausnahme dort kostet RSRs Teil für dieses Ereignis, nicht das Spiel.
- *Figuren über Bilder gehalten – ein Absturzweg (A827, widerlegt A795):* Dalamuds `IsValid()` prüft nur, ob der
  eigene Charakter geladen ist, nicht ob die Figur noch besteht, und fast jede Eigenschaft einer Figur ruft über die
  virtuelle Tabelle der Spielstruktur: `GameObjectId`, `Name`, `IsDead`, `IsTargetable`, `StatusList` und jede
  Wirk-Eigenschaft über `GetCastInfo` (Platz 80; Dalamud 43b3411, ClientStructs 80cb4a7). A795 hielt das für harmlos,
  weil der Speicherblock der Kampffiguren bestehen bleibt. Sein Absturz vom 15.11.2026, 18:54 (Build 77af8f6d6, Crucible,
  Ahriman-Ebenbilder) widerlegt es: `GazeWatch.AddLandings` fragte eine gehaltene Figur nach `IsCasting`, zwei Sekunden
  nach ihrem Blick; `IsDead` lief noch, der Aufruf über Platz 80 nicht – der Platz gehörte inzwischen keiner Kampffigur
  mehr, und eine Zugriffsverletzung fängt .NET nicht ab. Innerhalb einer Sitzung bleibt der Speicher lesbar, die
  Funktionstabelle nicht; beim Ausloggen gibt das Spiel auch den Speicher frei, und das Lesen eines Felds der Figur
  (`ObjectIndex` in `IsCurrent`) beendete am 16.11.2026 das Spiel (A845). Die Prüfung liest deshalb nur die Tabelle.
  *Regel, ohne Ausnahme:* Was über ein Bild hinaus lebt, hält die Id (beim Anlegen aus der frischen Figur gelesen) und
  löst die Figur dort, wo es sie liest, über die Objekttabelle auf (`SearchById`); eine gehaltene Figur wird nach keiner
  Eigenschaft gefragt, auch nicht nach ihrer Id. Konzept 03, „Dritte Invariante", führt die Regel; die Blickwache hält
  sie jetzt typsicher ein (der Blick trägt nur noch die Id).
- *Rekursion im Brett* (`CrucibleRoute.WorstNeedAhead`): Der Platz wurde erst nach dem Abstieg gemerkt; ein Rückweg in
  den Spieldaten (`XBMContentStageEventMap`) liefe bis zum Stapelüberlauf, den .NET nicht abfängt. Die Bretter führen
  heute nur vorwärts – darauf darf das Spiel nicht hängen. Behoben wie in den drei Suchen daneben: Der Platz zählt
  sich selbst, bevor die Wege dahinter gesucht werden; ein Rückweg endet dort.
- *Fremde Plugins im Hook des Spiels:* Die Übernahme der Sperrzeit-Werkzeuge (BossModReborn per IPC, NoClippy per
  Befehl, die Modulliste des Prozesses auf XivAlexander) lief beim Gebietswechsel aus dem Bild-Hook des Aktionsmanagers.
  Ihre Ausnahmen fängt das `try` dort, nicht aber was sie an eigenen Hooks auf dieselbe Spielfunktion umschalten,
  während deren Aufruf läuft. Verlegt in `Framework.Update` (`RunOnTick`); Preis: ein Bild lang gilt nach dem
  Gebietswechsel die Feststellung des vorigen Gebiets, ob ein anderes Werkzeug die Sperre selbst kürzt.
- *Bleibt, mit Grund:* `AgentXBMPetParty.GetSyncedRank` ruft beim Betreten des Bretts native Funktionen ohne Prüfung,
  ob der Agent offen ist – der Agent besteht, solange das Spiel läuft, die Aufrufe standen schon im Lauf vom 11.11.
  ohne Absturz; was die Funktion intern liest, ist unbelegt. Fensterwerte ohne Obergrenze von Anzahl und Textlänge
  (`BestiaryTrace.Values`): Die Anzahl ist die des Spiels, das sie selbst durchläuft, und jede Seite liest
  `SafeMemory`; ein Text ohne Nullbyte über viele Seiten ist unwahrscheinlich (Schluss, nicht gemessen). Die Lücke
  unbeantworteter Anfragen (`AnimationLockTweak.NoteAnswer`) ist so groß wie die gesendeten Anfragen ohne Antwort.
- *Threads, Hooks beim Entladen, Arithmetik mit Startwerten:* ohne Befund (WarmUp-Thread in `try`, Abbruch über Token;
  jeder Hook wird beim Entladen freigegeben; `AddMargins` war die einzige ungeprüfte Rechnung mit einem Startwert).

### Analyzer auf Fork-Zeilen

- *Behoben:* `MainWindow_Debug` las ein Aktionsblatt mit `GetRow`, das für eine unbekannte Id eine Ausnahme wirft;
  die Prüfung auf `null` danach war tot (CA1508) – jetzt `GetRowOrDefault`. Versiegelt: zehn Fork-Klassen (CA1852);
  `DRK_Reborn.GroupStunRunning` statisch (CA1822); `Regex.Count` statt einer Trefferliste (CA1875); elf Vorbelegungen
  mit dem Standardwert entfernt (CA1805). Im Kampf ohne messbare Wirkung, ohne Preis.
- *Bleibt:* CA1802 in den `Configs`-Dateien – die Felder sind das Muster, aus dem der Source Generator die Einstellungen baut;
  CA1822 an `RunningSwiftGoesToRaise` – geschützte Eigenschaft, die abgeleitete Rotationen lesen;
  CA1819 in `StatusHelper` – die Felder sind öffentliche Schnittstelle für abgeleitete Rotationen; die übrigen
  CA1508 auf Fork-Zeilen sind falsch positiv (`Player.Object` ist beim Login `null`, obwohl die Signatur es nicht sagt).
- *Nicht angefasst:* 579 Befunde auf Upstream-Zeilen und die Klone in Upstreams PvP-Rotationen – keine Wirkung im
  Kampf, jede Zeile Merge-Logistik. PvP gehört seit dem 13.11.2026 zu seinem Profil (CLAUDE.md, „Nutzungsprofil“); ein
  Klon ändert dort nichts am Kampf, Fehler darin behandelt Konzept 25. CA1851 in `ActionTargetInfo`: Die Folge ist an
  diesen Stellen schon eine Liste, der zweite Lauf kostet keine neue Auswahl.
- *Keine Prüfung in der CI:* Ein zweiter Build mit allen Regeln kostete jeden Lauf rund 40 s für Befunde ohne Wirkung im
  Kampf; die Zahlen hier sind datiert und werden beim nächsten Audit neu erhoben.

### Spieldaten lesen: gemeinsame Bibliothek oder direkt

*Sein Hinweis (17.11.2026):* „entweder passt der rsr entwickler sein plugin an den gamepatch bei größeren umbauten im
spiel an, oder die entwickler von dalamud oder ecommons oder structs. da mehrere plugins auf den gemeinsamen libraries
basieren, ist eher der robuste und allgemeine teil zu bevorzugen. prüfe im loop mit antithese". Anlass: Upstream 7.5.8.4
liest die Status einer Figur direkt aus FFXIVClientStructs (`StatusHelper.GetStatuses`) statt über Dalamuds
`StatusList`.

*Grundlagen (belegt):*
- Dalamuds `StatusList` liest dieselbe Struktur über dieselbe Bibliothek (`StatusManager`, `NumValidStatuses`; Dalamud
  43b3411, `StatusList.cs`) und legt je Zugriff ein Objekt für die Liste und eines je Status an (`new Status(address)`).
- Speicherabstände stehen in keinem der beiden Wege in RSR; der JIT liest sie aus der geladenen ClientStructs. Die
  Verschiebung von `NumValidStatuses` mit 7.2 (0x2E8 → 0x3D8, ClientStructs 9cdd35dd, 25.03.2025) traf beide Wege gleich
  und keinen davon.
- Was nur den direkten Weg trifft, sind umbenannte oder entfernte Member: `Status.SourceId` wurde am 25.07.2025 zum Fehler
  erklärt und am 10.08.2025 entfernt (ClientStructs 76f44d77, 4a5ebdf0), nach Monaten als Warnung; Dalamuds
  `IStatus.SourceId` blieb. Ein Plugin, das das Feld direkt las, fiel nach dem Dalamud-Update bis zum Neubau aus.
- RSR hängt schon heute direkt an ClientStructs: 97 Dateien, 63 davon vom Fork (gezählt 17.11.2026).

*These (sein Hinweis):* Der allgemeine Teil wird von den Bibliotheken für alle Plugins nachgezogen, also ist er der
robustere. Für Umbenennungen zwischen zwei RSR-Builds trifft das zu (Beleg `SourceId`).
*Antithese 1 – Kosten je Frame:* Dalamuds Weg legt bei jeder Statusprüfung Objekte an; RSR prüft Status hunderte Male je
Frame (A325). Das ist Speicherbereinigung im Spielthread (oben, „Struktur und Kosten“). Wie viel davon die Statusprüfung
trägt, ist nicht gemessen (unbelegt; Upstream nennt „drastically improved performance … on larger pulls and weaker
hardware“ ohne Zahl).
*Antithese 2 – das Risiko ist angekündigt und abgedeckt:* ClientStructs entfernt einen Member erst nach einer Zeit als
Warnung. Er baut den Branch vor dem Spielen; ein entfernter Member wird dann ein Fehler beim Bauen, nicht im Kampf. Der
Abgleich mit Upstream prüft die Referenzen gegen die laufende Dalamud-Fassung (Skill `upstream-sync`, `RefCheck`).
*Antithese 3 – Logistik:* Zurück zu Dalamuds Weg hieße, Upstreams Kerndatei dauerhaft abweichend zu führen; jeder Sync
brächte Konflikte an derselben Stelle.
*Ergebnis:* Upstreams direkter Weg bleibt, eingeschränkt auf die zentrale Stelle: Status liest der Fork nur über
`StatusHelper` (dort `GetStatuses`), nicht mit eigenen ClientStructs-Zugriffen in Regeln, damit eine Umbenennung an einer
Stelle zu beheben ist. Seine These gilt, wo der allgemeine Weg nichts kostet: Für jeden neuen Zugriff auf Spieldaten nimmt
der Fork zuerst Dalamud oder ECommons, ClientStructs nur, wo es dort keinen Weg gibt oder er je Frame anlegt.
*Preis:* Für Nutzer eines fertigen Releases fällt die Statusprüfung aus, wenn Dalamud einen genutzten Member entfernt,
bevor RSR neu erscheint; dasselbe gilt heute schon für jeden der übrigen ClientStructs-Zugriffe.

## Wie es gemessen bleibt

- Die Arbeitsregeln daraus stehen in `.claude/rules/code.md`, „Struktur und Kosten", und greifen schon im Konzept: Die
  Definition of Ready verlangt für Code seinen Ort und, läuft er je Frame, seine Kosten (CLAUDE.md, A742).
- `check_fork_placement.py` (CI) lehnt einen neuen Fork-Member in einer Datei von Upstream ab. Fork-Member erkennt
  es an ihrer Signatur: Weder die Merge-Basis noch `upstream/main` führen sie in dieser Datei, und Upstream kennt keinen
  Member dieses Namens (eine geänderte Signatur bleibt Upstreams Member; Preis: eine neue Überladung einer Methode von
  Upstream geht unbemerkt durch). Die 563 Member, die am 13.11.2026 in 75 Dateien von Upstream standen, führt
  `fork_placement.json`; die Liste wird nur kürzer, weil ein Member beim nächsten Eingriff in seine Teildatei zieht.
  Ausgenommen: fremde Rotationen und die `Configs`-Dateien (`Configs.cs`, `.CombatAndTarget.cs`, `.DutiesAndPvP.cs` seit
  Upstreams Aufteilung c170c9cd5; alle Einstellungen in einer Klasse, das Muster von Upstream, das der Source Generator
  liest).

- Der Plugin-Build der CI behandelt CS8600 bis CS8604 als Fehler (`build.yaml`, mit Gegenprobe; A878). Wogegen: einen
  Startwert, der ein noch nicht belegtes statisches Mitglied liest (Grenze unter „Fork-Code in eigenen Teildateien");
  für jeden Nutzer nach einem Sync oder einer Umverteilung; ohne die Sperre lädt RSR, und die erste Aktion wirft im
  Typinitialisierer. Der Compiler prüft die Startwerte in der Reihenfolge, in der er sie ausgibt, und meldet ein solches
  Lesen als CS8601 (Zuweisung), CS8602 (Zugriff) oder CS8604 (Argument) – so fielen beim Sync c170c9cd5
  `StatusHelper.NoNeedHealingStatusBesidesDeathTrigger` (las die Liste, die Upstreams Aufteilung hinter sie gestellt
  hatte) und die beiden Kanalfelder auf; beide hätten RSR beim Laden abgeworfen, und der Build lief mit Warnung durch.
  In der CI und nicht in den Projekten, wie die Sperren für veraltete Aufrufe: Eine Dalamud-Fassung mit geänderten
  Annotationen soll seinen eigenen Build nicht anhalten. Aufwand: eine Zeile und die Gegenprobe; eine künftige
  Nullable-Warnung dieser Codes, auch aus Upstream, ist vor dem Push zu beheben. Grenze: Werttypen (sie lesen 0, ohne
  Absturz, gemessen an der Gegenprobe) und ein Lesen über einen Methodenaufruf sieht der Compiler nicht; eine
  Syntaxmessung über alle 1.959 statischen Mitglieder, aufgerufenen Methoden und Ausdruckseigenschaften folgend, fand am
  18.11.2026 keine weitere Stelle (A878).
- Nach jedem Kampf die Kostenzeile mit Übersetzungszeit; nach dem Laden und jedem Rotationswechsel die Zeile
  „warm-up". Ein langsamster Durchlauf über einem Frame mit wenig Übersetzungszeit ist der nächste Befund.
- Analyzer und Klonsuche wie oben (Befehle im Archiv, A739), bei jedem Audit neu.

### Analyzer, Klone und Teildateien: Stand und Entscheidungen

- *Analyzer* (wie oben, gemessen am 15.11.2026 auf efa5580c5, A825): 639 Befunde, 50 auf Fork-Zeilen (Zeilen, die `upstream/main...HEAD`
  hinzufügt). Alle sind die oben geführten Ausnahmen: CA1802 in `Configs` und den Teildateien von Einstellungen,
  CA1819 an öffentlichen Statuslisten von `StatusHelper` (neu: `PvPGuardStatuses`, `GuardWastedStatuses`, gleiche
  Begründung), CA1822 an `RunningSwiftGoesToRaise`, CA1508 an `Player.Object` und an einer Abwehr gegen einen
  ungenannten Wert von `HornOrder`, CA1852 an zwei öffentlichen Klassen, die abgeleitete Rotationen erweitern dürfen.
  Kein neuer Befund mit Wirkung.
- *Doppelter Code* (jscpd 4.0.5, wie oben): 1.269 von 94.882 Zeilen, 1,34 %, 58 Klone. Die Zunahme seit A739 sind die
  Köpfe der Teildateien (gleiche `using`-Zeilen in Zeile 1, kein Code). Mit Fork-Zeilen ein echter Klon:
  `SurveyStuns`, `SurveyHostileOutput` und `SurveyHostileStatus` (`CustomRotation_OtherInfo.Universal.cs`) gehen
  dieselbe Gegnerliste im selben Umkreis durch und unterscheiden sich nur in dem, was sie je Gegner zählen.
  - *Entscheidung:* eine Schleife `SurveyHostiles` mit einem Zähler als Parameter (eine Struktur je Frage, über eine
    Schnittstelle als generische Einschränkung, damit kein Abschluss und keine Anlage je Aufruf entsteht). Die drei
    geschützten Methoden bleiben als Schnittstelle für abgeleitete Rotationen, mit gleichem Verhalten. Grund: Eine
    Korrektur am Umkreis oder an der Liste (Helfer, Tote) gälte sonst nur für eine Kopie.
  - *Nicht angefasst:* die Klone in Upstreams PvP-Rotationen, Konfigurationsklassen und Jobrotationen (wie oben) und
    `DiagnosticsWindow` gegen `NextActionWindow` (Fensterrahmen, keine Wirkung im Kampf).
- *Fork-Member in Dateien von Upstream:* 35 weitere ziehen in Teildateien je Thema, jede Codezeile genau einmal
  (gemessen als Zeilenmenge vor und nach dem Umzug): `DataCenter` (11 nach Universal, TankProtection, HitResponse,
  AreaDamage), `ActionBasicInfo` (10, neu `.Universal` für die Sperren des Zauberbeginns und `.Raise` für die
  MP-Reserve der Wiederbelebung), `BaseAction` (4) und `IBaseAction` (10 statische Pfadflaggen). `fork_placement.json`:
  311 → 276. Upstreams Dateien ändern sich dabei nur in der Typdeklaration (`partial`) und um die herausgelösten
  Member.
  - *Bleibt:* Einstellungen (ihre Reihenfolge ist die des Fensters), Felder mit Startwert, `OtherConfiguration`
    (statische Felder mit Startwert) und die Instanzmember von `ActionTargetInfo`: Ein Struct mit Primärkonstruktor –
    sein Parameter `action` gilt nur im Teil mit der Parameterliste, und diese Member lesen ihn alle. Statische Member
    ohne ihn können in eine Teildatei (`ActionTargetInfo.AreaDamage.cs`, A826).
