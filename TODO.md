# TODO — nur offene Arbeit

Je Eintrag: was im Kampf anders wäre, wo es sitzt, der nächste Schritt oder die Auflösungsbedingung, das Konzept.
Begründungen, Messungen und die Geschichte stehen im Konzept und im Archiv (`AUDIT_LOG.md`), nicht hier. Ein
abgeschlossener Punkt verschwindet aus dieser Datei; das Konzept nennt dann den geschlossenen Stand.
Betroffene: **N** Endnutzer · **R** Autoren abgeleiteter Rotationen (Namensräume `RotationSolver.Basic`) · **U** Upstream-Pflege.

## Reihenfolge

Nach der Wirkung im Kampf: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit, in seinem Profil
(seine Vorgabe, 17.10.2026: „todo nach wichtigkeit abarbeiten. jeweils im loop bearbeiten").

1. Die gelernten Tabellen: entschieden (Konzept 13, A451); gebaut nach Regel 12 des Entwurfs, erst die Revision über
   einen Spielpatch (als Nächstes Stufe 2, nach seinem ersten Upload mit Stufe 1, A456), dann die Anzeige. Beide ändern den Kampf nur mittelbar, über einen
   geprüften oder berichtigten Lernwert – die Sperren unter ihnen (Schlaf) aber dauerhaft.

Danach die technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

## Defekte

Keine offenen.

## Offene Arbeit

### Gelernte Tabellen über einen Spielpatch: Revision statt Höchstwert · N
Sein Auftrag (28.10.2026): „das lernlisten-konzept müsste auch überarbeitet in hinblick auf anstehende patches. […]
nach patch werden die bestehenden listen als gegeben angesehen, aber noch nicht als verifiziert. also solange anwendung
der bisherigen erkenntnisse, bis durch neue erkenntnisse (also eine neue revision der liste) diese bestätigt werden oder
eben falsifiziert werden und in der neuen liste als ungültig markiert bzw. in der alten gelöscht werden. das neue
revisionssystem muss sicher aufgebaut sein, dass nicht irrtümlich erkenntnisse gelöscht werden, bzw. alte daten dauerhaft
unwidersprochen und ungeprüft nach einem versionssprung des spiels beibehalten werden." Im Kampf: Ändert ein Patch eine
Mechanik, gilt der alte Lernwert weiter. Ein stärkerer Treffer hebt ihn an, weil jedes Speichern den höheren Wert
behält; ein abgeschwächter Treffer senkt ihn nie, und RSR gibt weiter Abwehr und Heilung für den alten aus. Ein Gegner,
der nun schläft, bleibt als resistent geführt, eine Serie, die nicht mehr kommt, bleibt gelernt, ein Wirken, das kein
Pyretisch mehr anlegt, hält weiter Zauber an. Offen sind die Stufen 2 bis 5; die Messungen je Spielversion führen Fläche
und Tankbuster schon (A456). *Ort:* `OtherConfiguration.RevisedStore` (Fläche, Tankbuster),
`OtherConfiguration.LearnedFile` (die übrigen: Zusammenführung mit `Math.Max`, `Math.Min` oder `Higher`), die Werttypen
`SleepReading`, `DotReading`/`DotArea`, dazu `HitSeriesOnPlayer`, `TankbusterMarkerWithoutHit` und
`ActionPenaltyCasts`. *Nächster Schritt:* Stufe 2, Regel 4 für Fläche und Tankbuster – ein übernommener Wert spart
keinen Schutz, bis eine saubere Messung der laufenden Version ihn bestätigt (Konzept 13, „Gelernte Tabellen über
Spielversionen", Regel 12); die offene Frage hat er entschieden, asymmetrisch (A451). Gebaut, sobald sein erster Upload
mit Stufe 1 zeigt, dass der Speicherweg im Spiel trägt (Kopfzeile mit lesbarer Spielversion, `.rev.json` mit laufenden
Schichten): Stufe 1 soll ihn prüfen, bevor eine Abwehr von ihm abhängt. *Konzept:* 13.

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
`SleepOutcomes`, `SleepResistantKinds`, `SleepDiminishing`, `HitSeriesOnPlayer`, `DotOnParty`, `ActionPenaltyCasts`.
*Nächster Schritt:*
Anzeige ja, Bearbeitung nein (Konzept 13, „Gelernte Tabellen über Spielversionen", Regel 10; seine Vorgabe A173); gebaut
als letzte Stufe der Revision. *Konzept:* 13 (gelernte Tabellen), 18 (Einstellungen).

## Technische Schuld

Ohne Wirkung im Kampf oder bewusst eingegangen; je Kosten und Auflösungsbedingung.

### Gleiche Stufe ohne Gegenstandsstufengrenze wird gemessen wie synchron · N
*Kosten:* In einer Gruppe ohne Beschränkung bei der Stufe des Inhalts fällt jeder Treffer kleiner aus und wird abgelegt;
ist er der einzige, spart die Größenregel im nächsten synchronisierten Lauf zu Unrecht. Erkannt wird nur eine Stufe über
der des Inhalts (`DataCenter.AboveContentLevel`, A452). *Auflösung:* eine Quelle, die den laufenden Inhalt als Gruppe
ohne Beschränkung ausweist; `ContentsFinderQueueInfo.PoppedContentIsUnrestrictedParty` beschreibt den zuletzt
gestarteten. *Konzept:* 13.

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
- **Ein gewählter Schlaf geht nicht hinaus** (siebenmal an zwei Abenden, A453, A457). Ein oGCD schließt das Fenster
  nicht (ein bereiter GCD geht vor, Fähigkeiten ruhen in den letzten 0,5 s); übrig bleiben Bewegung und ein Mitglied
  in der kritischen Klasse, beides gewollt. Auflösung: der Grund, den die Zeile „sleep not cast …" nennt (A459); heißt er
  „too little time" kurz nach der Wahl, ist die Bedarfsrechnung ein Defekt. Konzept 08, „Ein gegnerisches Wirken
  stoppen".
