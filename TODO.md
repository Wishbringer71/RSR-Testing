# TODO — nur offene Arbeit

Je Eintrag: was im Kampf anders wäre, wo es sitzt, der nächste Schritt oder die Auflösungsbedingung, das Konzept.
Begründungen, Messungen und die Geschichte stehen im Konzept und im Archiv (`AUDIT_LOG.md`), nicht hier. Ein
abgeschlossener Punkt verschwindet aus dieser Datei; das Konzept nennt dann den geschlossenen Stand.
Betroffene: **N** Endnutzer · **R** Autoren abgeleiteter Rotationen (`RotationSolver.Basic`) · **U** Upstream-Pflege.

## Reihenfolge

Nach der Wirkung im Kampf: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit, in seinem Profil
(seine Vorgabe, 17.10.2026: „todo nach wichtigkeit abarbeiten. jeweils im loop bearbeiten").

1. Pyretisch trifft mitten in einen eigenen Zauber – tötete zweimal an einem Abend.
2. Einzelabwehr-Streckung bei einem doppelten Tankbuster – der Treffer kommt sicher, und ein Tank steht ohne Hilfe.
3. Flächentabelle misst unter der stehenden Minderung – bewertet nur den ersten Treffer einer Aktion falsch.
4. Laufende HoTs fehlen in den HoT-Listen – kostet Schaden, nicht Sicherheit.
5. Die gelernten Tabellen, als ein Konzept und eine Vorlage, gebaut erst nach seiner Freigabe: erst die Revision über
   einen Spielpatch, weil sie festlegt, welchen Stand ein Wert hat, dann Anzeige und Bearbeitung in den Einstellungen,
   die diesen Stand zeigen. Beide ändern den Kampf nur mittelbar, über einen geprüften oder berichtigten Lernwert.

Danach die technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

## Defekte

### Laufende HoTs fehlen in den HoT-Listen · N, R
Läuft die Regeneration aus Kerachole (Weiser) oder Celestial Opposition (Astrologe) auf der Gruppe oder eine
Selbstheilung auf einem Tank (etwa Aurora), heilen RSRs Heiler, als liefe nichts: Die Schwellen sinken nicht, und eine
GCD-Heilung fällt, wo die Regeneration getragen hätte. Die Liste soll nach ihrem eigenen Kommentar jeden laufenden
Gruppen-HoT tragen. *Ort:* `StatusHelper.AreaHots`, `SingleHots`; Leser `StateUpdater.GetHealingOfTimeRatio`,
`SelfHealingOfTimeRatio`, `ActionTargetInfo`. *Nächster Schritt:* Loop zur Erkennung am Spielblatt – vorhanden als
`StatusHelper.IsHealOverTime` (84 Status mit „Regenerating HP over time"; die Bodeneffekte mit eigenem Wirktext) –, die
Listen als Rückfall; Gruppe oder Einzelziel nach dem Wirkbereich der gewährenden Aktion. *Konzept:* 08 („Was RSR heute tut" im Abschnitt „Schaden über Zeit").

## Offene Arbeit

### Pyretisch trifft mitten in einen eigenen Zauber · N
Ein Gegnerwirken, das Pyretisch anlegt („Heißes Herz", #43322, 4,7 s, im Pilgerpfad), tötet, wenn es landet, während
ein eigener Zauber läuft: Der Schaden kommt im selben Augenblick wie der Status (26.10.2026: rund 1,03 Mio. auf ihn,
nachdem RSR 0,8 s vor der Landung Wirbelströmung begann; dasselbe Wirken tötete sechs Minuten zuvor ein Mitglied). Der
Abbruch unter Pyretisch (A338) kommt danach. *Ort:* `DataCenter.IsHostileStop` (Stoppliste, ab Werk aus, das Wirken
nicht gelistet), `Watcher` (Effektmeldung: Status 960 als Effektart 0x0E in der Landung). *Nächster Schritt:* Loop, ob
RSR Wirken lernt, die einen Status der Liste „No-casting debuffs" anlegen, und vor ihrer Landung keinen Zauber beginnt,
der dann noch liefe; Option und Voreinstellung gehören in den Loop. *Konzept:* 03.

### Einzelabwehr-Streckung bei einem doppelten Tankbuster · N
Ein Tankbuster auf beide Tanks („Bittere Ernte", Zoraal Ja): Nach dem Göttlichen Segen auf den einen wartet
Wasserschleier, bis der Segen ausläuft – auch für den anderen Tank, der denselben Treffer nimmt; ein Tank bleibt ohne
Hilfe. *Ort:* `CustomRotation_DefenseHold.SingleDefenseStretched`, `WHM_Reborn.DefenseSingleAbility`; dieselbe Frage
bei jedem Heiler mit zwei Einzelmitteln. *Nächster Schritt:* Loop, ob die Streckung je Ziel gilt. *Konzept:* 08.

### Flächentabelle misst unter der stehenden Minderung · N, R
Ein Raidwide, dessen erste Landung die Gruppe voll gemindert nimmt, steht zu klein in der Tabelle; Glockenspiel,
Vorausheilung und „Skip mitigation for small area casts" lesen den kleinen Wert, bis ein schwächer geminderter Treffer
ihn anhebt. *Ort:* `Watcher.TryAreaShare`, die Leser in `DataCenter` (`AreaShareCallsForMitigation`, `HitDropsBelow`).
*Nächster Schritt:* ungemindert speichern wie die Tankbuster-Tabelle (`TankbusterTable.MitigationFactor`), die Leser
rechnen die jetzt stehende Minderung je Mitglied ein. *Konzept:* 13.

### Gelernte Tabellen über einen Spielpatch: Revision statt Höchstwert · N
Sein Auftrag (28.10.2026): „das lernlisten-konzept müsste auch überarbeitet in hinblick auf anstehende patches. […]
nach patch werden die bestehenden listen als gegeben angesehen, aber noch nicht als verifiziert. also solange anwendung
der bisherigen erkenntnisse, bis durch neue erkenntnisse (also eine neue revision der liste) diese bestätigt werden oder
eben falsifiziert werden und in der neuen liste als ungültig markiert bzw. in der alten gelöscht werden. das neue
revisionssystem muss sicher aufgebaut sein, dass nicht irrtümlich erkenntnisse gelöscht werden, bzw. alte daten dauerhaft
unwidersprochen und ungeprüft nach einem versionssprung des spiels beibehalten werden." Im Kampf: Ändert ein Patch eine
Mechanik, gilt der alte Lernwert weiter. Ein stärkerer Treffer hebt ihn an, weil jedes Speichern den höheren Wert
behält; ein abgeschwächter Treffer senkt ihn nie, und RSR gibt weiter Abwehr und Heilung für den alten aus. Ein Gegner,
der nun schläft, bleibt als resistent geführt, eine Serie, die nicht mehr kommt, bleibt gelernt. *Ort:*
`OtherConfiguration.LearnedFile` (Zusammenführung mit `Math.Max`, `Math.Min` oder `Higher`, Speichern), die Werttypen
`TankbusterReading`, `SleepReading`, `DotReading`/`DotArea`, dazu `HitSeriesOnPlayer` und `TankbusterMarkerWithoutHit`;
Vorbild der Upstream-Listen: `BuiltInList`/`SetList` (eingebaute Einträge, eigene Änderungen als Differenz,
ausgemusterte Ids). *Nächster Schritt:* Konzept im vollständigen kritischen Loop mit Antithese und Widerlegungsversuch,
danach Audit des Konzepts: Revision je Spielversion; nach einem Versionssprung gelten die alten Werte weiter, aber als
unbestätigt, bis eine Messung der neuen Revision sie bestätigt oder widerlegt. Seine Bedingung: Nichts wird irrtümlich
gelöscht, und nichts bleibt nach einem Versionssprung dauerhaft unwidersprochen und ungeprüft stehen. Umsetzung erst
nach ausführlicher Präsentation und Begründung (seine Vorgabe, 28.10.2026), zusammen mit „Gelernte Tabellen des Forks in
den Einstellungen zeigen und bearbeiten". *Konzept:* 13.

### Gelernte Tabellen des Forks in den Einstellungen zeigen und bearbeiten · N
Sein Auftrag (28.10.2026): „prüfen, ob es sinnvoll ist, die durch den fork selbst erstellten lernlisten ähnlich der
listen vom upstream (aoe, tankbuster, rückstoß etc.) auch in den settings anzeigbar und editierbar zu machen mit den
dazugehörigen werten wie name, gegner, schaden, effekt etc.). dass dann an den beispielen des upstreams ausrichten und
auch dort integrieren." Im Kampf: Ein falsch gelernter Wert – ein zu groß gemessener Tankbuster, ein als resistent
geführter Gegnertyp, ein Marker ohne Treffer, ein überschätzter DoT – steuert Abwehr, Heilung und Schlaf, bis die ganze
Datei bei geschlossenem Spiel gelöscht wird; die Liste zeigt heute nur die Anteile der Flächen- und der
Tankbuster-Tabelle neben Upstreams AoE- und Tankbuster-Liste. *Ort:* `MainWindow_List` (`DrawListActions`,
`DrawActionsList`, Vorbild: Upstreams Listen mit Hinzufügen, Zwischenablage, Entfernen, Zurücksetzen); die Speicher in
`OtherConfiguration`: `HostileCastingAreaPotential`, `TankbusterPotential`, `TankbusterMarkerWithoutHit`,
`SleepOutcomes`, `SleepResistantKinds`, `SleepDiminishing`, `HitSeriesOnPlayer`, `DotOnParty`. *Nächster Schritt:*
Prüfung und Konzept im kritischen Loop, mit Antithese und Widerlegungsversuch, danach Audit des Konzepts; erst dann
Code. Abzuwägen gegen seine Vorgabe aus A173 („unnötige funktionen erzeugen auch unnötige fehlerursachen", Löschen von
Hand statt Knopf); berührt das Ergebnis sie, lege ich es ihm vor. Die Anzeige zeigt auch den Stand eines Werts nach
„Gelernte Tabellen über einen Spielpatch: Revision statt Höchstwert"; beide Konzepte gehen zusammen. Umsetzung erst nach
ausführlicher Präsentation und Begründung, wie das Ziel erreicht wird (seine Vorgabe, 28.10.2026). *Konzept:* 13
(gelernte Tabellen), 18 (Einstellungen).

## Technische Schuld

Ohne Wirkung im Kampf oder bewusst eingegangen; je Kosten und Auflösungsbedingung.

### Searing-Light-Abdeckungsmodell bildet die Phasenbuchung von Hand nach · R
*Kosten:* Ändert sich die Buchung in `UpdateSearingPhaseBook`, `AllSearingPhasesHeld` oder `BurstPhaseHeldByAnother`,
rechnet `searing_light_coverage.py` weiter die alte Regel, und Konzept 12 argumentiert mit ihren Zahlen. *Auflösung:*
jede solche Änderung trägt im selben Commit die Anpassung des Modus „plugin". *Konzept:* 12.

### Status-Einstellungen auf der falschen Seite der Aktion · N, U
Vier Fundstellen in PvP und Bozja (`scan11.py`), außerhalb seines Profils; je eine Richtungsentscheidung, die eine
Beobachtung im Inhalt verlangt. *Auflösung:* erst, wenn er diese Inhalte nennt.

### Die Beiruta-Heiler lesen das Ziel des letzten Erfolgs · N, R
BeirutaAST, BeirutaWHM und BeirutaSGE lesen `X.Target.Target` vor `X.CanUse`, an 17 Stellen, und prüfen so das Ziel des
letzten Erfolgs, anfangs keines (A415); der eigene Code liest überall das Ziel derselben Entscheidung. Fremde
Rotationen, außerhalb seines Profils. *Auflösung:* wenn er sie nennt – dann eine seiteneffektfreie Zielabfrage im Paket,
die ihr Muster trägt. *Konzept:* 03.

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

### Doppelte Zustandswahl in den Zustandskommandos · N
`AdjustStateType` und die fünf `Cycle*`-Methoden lösen dieselbe Aufgabe; `DTRAllAuto` mit `ToggleAuto` kollabiert auf
zwei Zustände. *Auflösung:* erst mit Laufzeitbeobachtung des Zustandsautomaten; `DTRType` und `CycleType` sind
Ordinalvertrag. `AutodutyUpdateState` dupliziert `UpdateState` und löst sich mit. *Konzept:* 03.

### Die Aufnahme in die AoE-Liste trennt Raidwide und ausweichbare Fläche nicht · N
Die Aufnahme in die Liste bleibt grob; Bewertung und Reichweite trennen schon (Konzept 13). *Auflösung:* ein Unterscheidungsmerkmal aus
den Spieldaten (`CastType`, `EffectRange`), hinter eigener Option. *Konzept:* 13.

### Zwei Werte des Dunkelritters hängen nicht an ihrer Strategie · N
`BlackestNightMinHostiles` und `BlackestNightHealthRatio` erscheinen ohne `Parent`; `ParentValue` kennt nur einen Wert.
*Auflösung:* beim Tooltip belassen, solange kein zweiter Fall auftritt. *Konzept:* 10.

### Release-Paket enthält vermeidbaren Ballast · N, R
4,71 MB (7.5.7.3+wsh1) statt rund 2,3 MB (XML, Analyzer, PDB); Upstream verhält sich gleich. *Auflösung:* erst, wenn der
Veröffentlichungspfad prüfbar ist.

### VPR: leerer Zweig einer Struktur, die anderswo entscheidet · U
`VPR_Reborn.GeneralGCD`, Swiftskin's Bite und Sting. *Auflösung:* Adressat Upstream; bewusst nicht gelöscht (A11).

### Das Aktionsfenster rechnet die Bewegungssicherheit ein zweites Mal · U
`MainWindow.GetMovementSafetyStatus` neben `ActionTargetInfo.CheckMovementSafety`; die Regel „am Ziel ist ein Sprint nur
Schaden" fehlt im Fenster. *Auflösung:* `CheckMovementSafety` gibt Ergebnis und Grund heraus, das Fenster zeigt sie.

### Die Modelle von Maschinist, Samurai und Monk bilden ihre Rotation von Hand nach · R
*Kosten:* Ändert sich der Ruf der Königin oder die GCD-Folge in `MCH_Reborn`, die GCD-Folge oder der Meikyo-Zweig in
`SAM_Reborn`, Perfect Balance, die Buffs oder die GCD-Folge in `MNK_Reborn`, rechnen `machinist_balance_model.py`,
`samurai_balance_model.py` und `monk_balance_model.py` weiter die alte Regel, und Konzept 14 nennt ihre Ergebnisse. *Auflösung:* jede solche Änderung trägt im selben Commit die Anpassung des Modells. *Konzept:* 14.

### Plugin-Identität ist die des Upstreams · N
`manifest.json` unverändert (`InternalName`, URLs). *Auflösung:* seine Entscheidung, wenn beide Installationen
nebeneinander gebraucht werden.

## Warten auf Daten oder Fremde

- **Samurai nutzt Feint und Tengentsu nicht** (fremde Meldung, PR #11, unbestätigt; statisch geprüft A265). Auflösung:
  ein Protokoll eines Samurai-Kampfs („area defence open, nothing chosen" gegen „hit you"). Konzept 13.
- **Treffer aus einer Quelle, die die Objekttabelle nicht hält** (Verdacht). Offen ist der Treffer ohne Objekt; der
  Fall „in der Tabelle, aber keine Figur der Gegnerseite" wird gelesen.
  Auflösung: Protokollzeilen „hit you from a source the object table does not hold". Konzept 08.
- **BossModReborn: FATE-Modul in Occult Crescent läuft nach dem Weggang weiter** (am Quelltext `be8ff77` gefolgert).
  Adressat BossModReborn. Konzept 08, „Wann eine BossModReborn-Vorhersage ihn betrifft".
- **Ein gegnerisches Wirken stoppen: was die gespeicherten Tabellen lernen** (experimentell, seine Vorgaben 18.10.,
  23.10. und 27.10.2026). Offen: ob Schlaf unterbrechbares Wirken stoppt, welche Anzahl bis zur Immunität die Regel
  lernt, welchen Rest beim Eintreffen. Auflösung: Protokolle mit Heiler oder Magier in Instanzen, danach die Bewertung,
  ob die Regel experimentell bleibt. Konzept 08, „Ein gegnerisches Wirken stoppen".
