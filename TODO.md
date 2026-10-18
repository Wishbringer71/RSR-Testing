# TODO — nur offene Arbeit

Je Eintrag: was im Kampf anders wäre, wo es sitzt, der nächste Schritt oder die Auflösungsbedingung, das Konzept.
Begründungen, Messungen und die Geschichte stehen im Konzept und im Archiv (`AUDIT_LOG.md`), nicht hier. Ein
abgeschlossener Punkt verschwindet aus dieser Datei; das Konzept nennt dann den geschlossenen Stand.
Betroffene: **N** Endnutzer · **R** Autoren abgeleiteter Rotationen (`RotationSolver.Basic`) · **U** Upstream-Pflege.

## Reihenfolge

Nach der Wirkung im Kampf: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit, in seinem Profil
(seine Vorgabe, 17.10.2026: „todo nach wichtigkeit abarbeiten. jeweils im loop bearbeiten"). Seine Aufträge vom
18.10.2026 gehen vor: TODO berichtigen (A322), dann die Prüfläufe, dann die Laufzeitkosten.

1. Prüfläufe: Notwendigkeit, Gliederung, Laufzeit (sein Auftrag)
2. Laufzeitkosten: Sonderabfragen statt geordneter Regeln (sein Auftrag)
3. Revolverklinge: „Only use on tankbuster targets prioritizing self" erreicht den Co-Tank nie
4. Tanks: Hilfe für den Co-Tank vor seinem Tankbuster
5. Tank-Unverwundbarkeit vor Tankbustern ohne Aktionsangabe oder Messung nur reaktiv
6. Tanks: Reflexion und Gruppenbarriere auf demselben Raidwide
7. DRK: Bruch von The Blackest Night zählen
8. Rückstoß und Stopp-Casts: Größe messen
9. Vorausschau vor dem ersten Treffer
10. Flächenheilung um den Wirkenden: Radius gegen Trefferfläche
11. Einstellungen ohne Leser, deren Text eine Wirkung verspricht
12. Feste Werte: jeder offene Wert braucht seinen Loop
13. Rotationen gegen The Balance abgleichen
14. Six-sided Star und Flamethrower
15. Vorlauf: Dragoon und Viper
16. Audit der gesamten Codebasis
17. `DPSTaken` zählt 5 ms statt 5 s

Danach technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

## Defekte

### Revolverklinge: „Only use on tankbuster targets prioritizing self" erreicht den Co-Tank nie · N, U

*Im Kampf:* Herz des Korunds und Aurora gehen in der ab Werk gewählten Einstellung nie auf einen anderen Tank mit
Tankbuster-Marker. *Ort:* `DataCenter.IsCastingTankVfx` füllt `TankbusterTargets` für einen Tank nur mit Markern auf
ihm selbst (`if (!isTank || isPlayerTarget)`, upstream `cbcc9d537`); die Bedingung gehört zur Frage „betrifft der
Marker den Spieler", nicht zur Liste. *Nächster Schritt:* Liste und Antwort trennen; zusammen mit „Tanks: Hilfe für
den Co-Tank vor seinem Tankbuster", die dieselbe Erkennung braucht. Text bindet. *Konzept:* 09.

### Einstellungen ohne Leser, deren Text eine Wirkung verspricht · N

*Im Kampf:* keine Wirkung – aber die Oberfläche verspricht eine. `InterruptDelay` und `ProvokeDelay` („random delay"),
`SwiftcastBuffer` („how early before next GCD should RSR use swiftcast for raise"), `TargetColor` („Target color",
dazu `Parent` auf sich selbst). Keine hat einen Leser (gemessen 18.10.2026). Nach seiner Vorgabe bindet der Text, also
ist der Code anzupassen. *Gegen das einfache Verdrahten steht je Stelle:* Die Unterbrechungsverzögerung kann das Fenster
des Zaubers verbrauchen; `SwiftcastBuffer` baute den in Konzept 11 behobenen Defekt neu; für `TargetColor` ist kein
Zeichenort bekannt. *Nächster Schritt:* je Einstellung ein Loop, Ergebnis Verdrahtung mit sinnvoller Voreinstellung
oder Entfernung mit Grund. *Konzept:* 11 (`SwiftcastBuffer`).

### `DPSTaken` zählt 5 ms statt 5 s · N, R, U

*Im Kampf:* keine – nur die Debug-Anzeige liest es; öffentlich und damit für abgeleitete Rotationen falsch. *Ort:*
`DataCenter.DPSTaken`, `TimeSpan.FromMilliseconds(5)` (upstream seit `3d1f2ccb2`). *Nächster Schritt:* Einheit richten,
Adressat auch Upstream.

## Offene Arbeit

### Prüfläufe: Notwendigkeit, Gliederung, Laufzeit (sein Auftrag) · U

*Sein Auftrag (18.10.2026):* warum die Prüfläufe immer umfangreicher werden, was davon notwendig ist, wie sie
geordnet laufen. Ein lokaler Lauf dauert rund 18 Minuten, fast nur das Tankbuster-Modell. *Nächster Schritt:* voller
Loop über alle Schritte (was jeder schützt, was er kostet, was davon ein Modell statt einer Prüfung ist).

### Laufzeitkosten: Sonderabfragen statt geordneter Regeln (sein Auftrag) · N, R

*Sein Verdacht (18.10.2026):* Die vielen eingebauten Sonderfälle laufen als eigene Abfragen statt als geordnete Regeln
und treiben den Rechenbedarf. *Gemessen:* rund ein Zehntel der Framezeit der Frames, in denen RSR läuft, Einzelschritte
bis 70 ms (seine Protokolle 16. und 17.10.2026, A299). Seit A310 schreibt das Protokoll die Kosten je Teilschritt.
Dazu gehört `DataCenter` mit Gettern, die bei jedem Lesen über Gruppe oder Gegner laufen, und die AoE-Schwelle an drei
Orten (Konzept 03, U5). *Nächster Schritt:* voller Loop – erheben, welche Abfragen je Frame laufen und wie oft, dann
zentral ordnen. *Konzept:* 03.

### Tanks: Hilfe für den Co-Tank vor seinem Tankbuster · N, U

*Im Kampf:* Kein Tank setzt seine Hilfe für andere (Nascent Flash, Intervention, The Blackest Night, Herz des
Korunds) vor dem Tankbuster auf den Co-Tank; The Balance tut es beim Krieger „most often on the co-tank". *Bausteine:*
Erkennung auf Stufe „alle" (Marker auf ihm, gezieltes Wirken auf ihn ab Wirkbeginn; BossModReborn nur mit Maske – belegt
ist nur Bit 0 = Spieler); Entscheidung auf Stufe „Tanks" mit der Frist vor dem Snapshot (A319) und der Triage gegen den
eigenen Bedarf (Krieger: `NeedsBloodwhettingHimself`). Bloodwhetting fällt upstream nur, solange das Ziel den Krieger
anvisiert (`PlayerIsTargetOnSelf`). *Konzept:* 09, „Krieger: Urimpuls für sich oder Urflackern für andere".

### Tank-Unverwundbarkeit vor Tankbustern ohne Aktionsangabe oder Messung nur reaktiv · N

*Im Kampf:* Vor einem Marker ohne Wirken, einem nur von BossModReborn angekündigten Tankbuster und dem ersten
Auftreten einer Aktion zündet die Unverwundbarkeit erst unter `HealthForDyingTanks` (vor einem gemessenen Wirken
vorher, A260, A308). *Nächster Schritt:* Das Protokoll zählt die Fälle („no cast taken for it"); für den Marker ohne
Wirken bräuchte es eine gelernte Zeit vom Marker zum Treffer – voller Loop vor dem Bau. *Konzept:* 09.

### Tanks: Reflexion und Gruppenbarriere auf demselben Raidwide · N

*Im Kampf:* Abschütteln und Reflexion fallen fast immer auf denselben Raidwide (seine Protokolle 01.10.2026). The
Balance: stapeln für große Treffer, verteilen über mehrere. *Nächster Schritt:* Fallarbeit für alle vier Tanks über die
gemessene Größe je Raidwide. *Konzept:* 09, „Krieger: die übrige Abwehr".

### DRK: Bruch von The Blackest Night zählen · N, U

*Im Kampf:* Ob die Barriere unter den engeren Stufen von `BlackestNightUsage` bricht (Annahmen: vier Gegner erreichen
3,6 % der maximalen LP je Sekunde; ein Tankbuster nimmt die angehobene Schwelle mit), sieht niemand. *Nächster
Schritt:* Die Regel zählt ihren Bruch selbst (Dark Arts danach) und steuert `BlackestNightMinHostiles` nach. *Konzept:* 10.

### Rückstoß und Stopp-Casts: Größe messen · N

*Im Kampf:* Für Tankbuster misst die Tankbuster-Tabelle jeden Treffer; `HostileCastingKnockback` und
`HostileCastingStop` haben keinen eigenen Speicher und keine Rechnung, also wirkt jede Abwehr dafür ohne Größe. *Nächster
Schritt:* je Liste klären, was die Größe dort entscheidet (ein Rückstoß verschiebt oft nur). *Konzept:* 08, 13.

### Vorausschau vor dem ersten Treffer · N

*Im Kampf:* Vor dem ersten Treffer eines Pulls ist die Vorausschau blind (2,5 s Anlauf). *Nächster Schritt:* klären, ob
der gemessene Anteil eines angekündigten Treffers (Konzept 13) die Lücke schon schließt, sonst die eigenen laufenden
Minderungen aus `DefensiveValues.g.cs` einrechnen. *Konzept:* 08.

### Flächenheilung um den Wirkenden: Radius gegen Trefferfläche · N

*Im Kampf:* `GetCanAffects` zählt ein Mitglied am Rand um seinen Trefferkreis zu großzügig mit; ob ein Spieler in einer
Fläche steht, entscheidet der Mittelpunkt seines Modells. Lux Solaris kann eine Anzahl erreicht sehen, die sie nicht
heilt. *Nächster Schritt:* alle Leser von `GetCanAffects` für freundliche Flächen erheben. *Konzept:* 08, „Wann Lux
Solaris zündet".

### Feste Werte: jeder offene Wert braucht seinen Loop · N, R

*Im Kampf:* Jede offene Zahl ist eine ungeprüfte Annahme über das Spiel; ein Patch ändert sie, der Code nicht.
`check_fixed_values.py` beziffert sie. *Zuerst:* `MitigationDebuffDuration` (fest 98, 15 s, 10 s) neben
`DefensiveValues.DurationOf`, das dieselbe Dauer aus dem Spiel liest – dieselbe Aufgabe an zwei Stellen. Danach die
Vorlaufzeiten der vorausschauenden Abwehr (Barde, Tänzer, Maschinist, Tanks) und die Jobs, die er spielt.

### Rotationen gegen The Balance abgleichen · N

*Im Kampf:* Schaden – je Job Opener, Burstausrichtung und Prioritäten gegen den Guide. Dazu die von `ActionId.resx`
ausgeblendeten Werte aus dem Job-Guide in den Generator und die deutschen Namen in `action_names_de.json`. *Konzept:* 14.

### Six-sided Star und Flamethrower · N

*Im Kampf:* Schaden. Werte und Einsatzregeln belegt (Job-Guide, The Balance, 02.10.2026), nicht gebaut. Six-sided Star
braucht die vorhergesagte Pause oder die Zeit bis zum Tod; Flamethrower bindet 11 s an Ort und Blickrichtung, gegen
seine Spielweise nur bei stehendem Rudel ohne angesagte Fläche. *Konzept:* 14, „Pausen und Phasenenden".

### Vorlauf: Dragoon und Viper · N

*Im Kampf:* Beide wirken im Countdown nichts, auch keinen Trank (kein `CountDownAction`). *Nächster Schritt:* an den
Openern von The Balance klären, ob eine Vorlaufaktion fällig ist. *Konzept:* 04.

### Audit der gesamten Codebasis · N, R, U

*Stand:* Phasen 1 bis 4 abgeschlossen (A8, A10, A82, A286–A290). *Offen:* der Rest von `DataCenter`, die Rotationen
Datei für Datei, `RotationSolver/UI`. Dokument dafür noch nicht angelegt; die Nummer 14 ist vergeben, es bekommt die
nächste freie.

## Technische Schuld

Ohne Wirkung im Kampf oder bewusst eingegangen; je Kosten und Auflösungsbedingung.

### Searing-Light-Abdeckungsmodell bildet die Phasenbuchung von Hand nach · R
*Kosten:* Ändert sich die Buchung in `UpdateSearingPhaseBook`, `AllSearingPhasesHeld` oder `BurstPhaseHeldByAnother`,
rechnet `searing_light_coverage.py` weiter die alte Regel, und Konzept 12 argumentiert mit ihren Zahlen. *Auflösung:*
jede solche Änderung trägt im selben Commit die Anpassung des Modus „plugin". *Konzept:* 12.

### `IBaseAction.IgnoreClipping` wird geschrieben, nie gelesen · N, R
*Kosten:* ein Flag, das eine Ausnahme verspricht, die nicht gilt. *Auflösung:* erheben, welche der Setzstellen in
`CustomRotation_Invoke` eine Ausnahme von der Anti-Clipping-Sperre rechtfertigen, und das Flag darauf eingrenzen.

### Status-Einstellungen auf der falschen Seite der Aktion · N, U
Vier Fundstellen in PvP und Bozja (`scan11.py`), außerhalb seines Profils; je eine Richtungsentscheidung, die eine
Beobachtung im Inhalt verlangt. *Auflösung:* erst, wenn er diese Inhalte nennt.

### Der erzeugte Merkmalssatz enthält keine Rollenmerkmale · N, R
`TraitRotationGetter.AddToList` verwirft Merkmale mit `ClassJob.RowId == 0`; Rollenmerkmale (Enhanced Reprisal, Feint,
Addle …) fehlen, ihre Stufen stehen als Zahl im Code. *Auflösung:* zusammen mit „Feste Werte"; vorher klären, ob
`RowId == 0` das Kennzeichen von Rollenmerkmalen ist.

### `CanUse` als Prüfung weist `Target` zu · N, R
*Kosten:* Code, der `X.Target` liest, ohne `X.CanUse` in derselben Entscheidung, liest das Ziel des letzten Erfolgs (55
Lesestellen außerhalb Churin, A286). *Auflösung:* je Stelle klären, ob das Ziel aus derselben Entscheidung stammt; erst
eine fremde Lesung begründet eine seiteneffektfreie Prüfung. *Konzept:* 03.

### `HasSurvivingShield` misst die kürzeste Schildrestzeit · N, R
Öffentlich, ohne Leser im Baum; der Tankbuster-Plan liest dieselbe Regel aus der Statusliste. Das Maximum tauschte den
Fehler in die gefährliche Richtung. *Auflösung:* eine Quelle für die Aufteilung des Schildwerts je Status. *Konzept:*
08, 09.

### Statuslisten ohne die Geschwister-Ids ihrer Wirkung · N, R
`scan14.py` erhebt die Klasse; offen u. a. `TankStanceStatus` und die Einzelprüfungen von Addle und Feint in
`GetCurrentMitigationPercent`. WrathCombo prüft dieselben Ids wie RSR. *Auflösung:* eine Quelle für die Id je Stufe.
*Konzept:* 03.

### Die Minderungsbilanz kennt zwei Schadensarten, die Datenquelle mehr · N, R
`GetCurrentMitigationPercent` rechnet „kein Cast", breath und sound als physisch; einziger Leser ist die Debug-Anzeige.
`IsPhysicalDamageIncoming` hat keinen Kampfleser. *Auflösung:* ein Verbraucher im Kampf; dann „unbekannt" eigens führen.
*Konzept:* 08.

### Die Aquapolis fehlt in der Zielpriorisierung der Schatzkarten-Dungeons · N, U
`IsInTheAquapolis` ohne Zweig in `TreasureDungeonPrio`. Außerhalb seines Profils. *Auflösung:* die Vorrang-Gegner aus
den Spieldaten des Gebiets.

### `Configs.Migrate` setzt zurück statt zu migrieren · N, R
Bei abweichender `CurrentVersion` fällt die ganze Konfiguration auf die Vorgaben; `Restore()` verweigert genau dann.
*Kosten:* Hebt Upstream die Version, sind alle seine Einstellungen weg. *Auflösung:* feldweise Migration.

### Doppelte Zustandswahl in den Zustandskommandos · N
`AdjustStateType` und die fünf `Cycle*`-Methoden lösen dieselbe Aufgabe; `DTRAllAuto` mit `ToggleAuto` kollabiert auf
zwei Zustände. *Auflösung:* erst mit Laufzeitbeobachtung des Zustandsautomaten; `DTRType` und `CycleType` sind
Ordinalvertrag. `AutodutyUpdateState` dupliziert `UpdateState` und löst sich mit. *Konzept:* 03.

### Die Aufnahme in die AoE-Liste trennt Raidwide und ausweichbare Fläche nicht · N
Bewertung und Reichweite sind gebaut (Konzept 13); die Aufnahme bleibt grob. *Auflösung:* ein Unterscheidungsmerkmal aus
den Spieldaten (`CastType`, `EffectRange`), hinter eigener Option. *Konzept:* 13.

### Zwei Werte des Dunkelritters hängen nicht an ihrer Strategie · N
`BlackestNightMinHostiles` und `BlackestNightHealthRatio` erscheinen ohne `Parent`; `ParentValue` kennt nur einen Wert.
*Auflösung:* beim Tooltip belassen, solange kein zweiter Fall auftritt. *Konzept:* 10.

### `SpreadDamagePaths` enthält keinen Spread-Marker · N
Alle Pfade sind Stack-Marker; folgenlos, weil `IsCastingAreaVfx` alle Listen liest. *Auflösung:* belegte Spread-Pfade.
*Konzept:* 13.

### Release-Paket enthält vermeidbaren Ballast · N, R
4,67 MB statt rund 2,3 MB (XML, Analyzer, PDB); Upstream verhält sich gleich. *Auflösung:* erst, wenn der
Veröffentlichungspfad prüfbar ist.

### VPR: leerer Zweig einer Struktur, die anderswo entscheidet · U
`VPR_Reborn.GeneralGCD`, Swiftskin's Bite und Sting. *Auflösung:* Adressat Upstream; bewusst nicht gelöscht (A11).

### Das Aktionsfenster rechnet die Bewegungssicherheit ein zweites Mal · U
`MainWindow.GetMovementSafetyStatus` neben `ActionTargetInfo.CheckMovementSafety`; die Regel „am Ziel ist ein Sprint nur
Schaden" fehlt im Fenster. *Auflösung:* `CheckMovementSafety` gibt Ergebnis und Grund heraus, das Fenster zeigt sie.

### Plugin-Identität ist die des Upstreams · N
`manifest.json` unverändert (`InternalName`, URLs). *Auflösung:* seine Entscheidung, wenn beide Installationen
nebeneinander gebraucht werden.

## Warten auf Daten oder Fremde

- **Samurai nutzt Feint und Tengentsu nicht** (fremde Meldung, PR #11, unbestätigt; statisch geprüft A265). Auflösung:
  ein Protokoll eines Samurai-Kampfs („area defence open, nothing chosen" gegen „hit you"). Konzept 13.
- **Treffer aus einer Quelle, die die Objekttabelle nicht hält** (Verdacht; Necrophobia 10.10., Düsterflare 17.10.).
  Auflösung: Protokollzeilen „hit you from a source the object table does not hold". Konzept 08.
- **BossModReborn: FATE-Modul in Occult Crescent läuft nach dem Weggang weiter** (am Quelltext `be8ff77` gefolgert).
  Adressat BossModReborn. Konzept 08, „Wann eine BossModReborn-Vorhersage ihn betrifft".
- **Kosten je Teilschritt** (Protokoll „update cost by step", A310) – Eingang für „Laufzeitkosten".
- **Tankbuster nach einem Marker ohne Wirken** (Protokoll „no cast taken for it", A308) – Eingang für
  „Tank-Unverwundbarkeit … nur reaktiv".
