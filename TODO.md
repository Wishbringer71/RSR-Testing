# TODO — nur offene Arbeit

Je Eintrag: was im Kampf anders wäre, wo es sitzt, der nächste Schritt oder die Auflösungsbedingung, das Konzept.
Begründungen, Messungen und die Geschichte stehen im Konzept und im Archiv (`AUDIT_LOG.md`), nicht hier. Ein
abgeschlossener Punkt verschwindet aus dieser Datei; das Konzept nennt dann den geschlossenen Stand.
Betroffene: **N** Endnutzer · **R** Autoren abgeleiteter Rotationen (`RotationSolver.Basic`) · **U** Upstream-Pflege.

## Reihenfolge

Nach der Wirkung im Kampf: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit, in seinem Profil
(seine Vorgabe, 17.10.2026: „todo nach wichtigkeit abarbeiten. jeweils im loop bearbeiten"). Seine Aufträge vom
18.10.2026 sind erledigt: TODO berichtigt (A322), Prüfläufe geordnet (A323), Laufzeitkosten umgestellt (A325). Sein
Auftrag vom 24.10.2026 läuft: die einfachen Punkte zuerst (A338), die schweren wichtigen danach ebenso. Sein Auftrag vom
25.10.2026, Upstream der letzten zehn Tage mit den eigenen Patches inhaltlich abzugleichen, ist erledigt (A354), der
Defekt daraus behoben (A356). Sein Auftrag vom 25.10.2026 nachmittags: nach Priorität und Umsetzbarkeit, je im Loop;
der Tank-Punkt ist entschieden (A357), sein Auftrag zu den Einstellungen ist erledigt (A358, Konzept 18), der Punkt
daraus gebaut (A359). Sein Auftrag vom 25.10.2026 abends wiederholt beide: das TODO weiter, danach ein zweiter
Durchgang des Einstellungs-Audits, Rotationseinstellung für Rotationseinstellung. Seine Frage vom 25.10.2026 abends zur
Abwehr ohne Heiler ist umgesetzt (A361), die Phönixfeder daraus ebenso (A372). Seine Frage
vom 25.10.2026 abends zu Chorts Türmen und dem Glockenspiel ist beantwortet und umgesetzt (A365). Die Heiler-Hilfe auf
einen kleinen gemessenen Tankbuster ist gebaut (A373); die Reflexion daraus steht als eigener Punkt.

1. Reflexion auf einen kleinen gemessenen Tankbuster des anderen Tanks
2. Rotationen gegen The Balance abgleichen
3. Six-sided Star und Flamethrower
4. AoE-Schwelle an drei Orten
5. Audit der gesamten Codebasis

Danach technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

## Defekte

Keine bekannt.

## Offene Arbeit

### Reflexion auf einen kleinen gemessenen Tankbuster des anderen Tanks · N

*Im Kampf:* Trifft ein gemessener kleiner Tankbuster nur den anderen Tank, geht deine Reflexion in der Einzelabwehr
weiter, obwohl Heiler und Tanks ihm seit A373 dafür keine Hilfe mehr geben; beim nächsten Raidwide oder schweren Treffer
fehlt sie 60 s. *Nächster Schritt:* Loop – die Reflexion an derselben Größenregel messen; sie zielt auf den Gegner,
also über die Quelle der Einzelabwehr statt über die Zielwahl, und eine BossModReborn-Vorhersage ohne Ziel daneben
muss sie weiter öffnen. *Konzept:* 09, „Hilfe für das Ziel eines Tankbusters".

### Rotationen gegen The Balance abgleichen · N

*Im Kampf:* Schaden – je Job Opener, Burstausrichtung und Prioritäten gegen den Guide. Der Maschinist ist abgeglichen
(A342), von Dragoon und Viper der Opener (A343), die AoE-Schwellen aller Jobs (A344), die AoE-Wahl des Ninja (A345). *Nächster Schritt:* die übrigen Jobs je Opener,
Burst und Prioritäten; ausgeblendete Werte liest `ActionTooltip` zur Laufzeit aus den Tooltips,
die deutschen Namen gehören in `action_names_de.json`. Dabei je Job die Rotationseinstellungen, die eine Spieltatsache
tragen (Leistenhöchststand, Restzeiten, Lieddauern), aus dem Spiel ableiten (Konzept 18, „Rotationseinstellungen").
*Konzept:* 14, „Vergleich mit The Balance".

### Six-sided Star und Flamethrower · N

*Im Kampf:* Schaden. Werte und Einsatzregeln belegt (Job-Guide, The Balance, 02.10.2026), nicht gebaut. Six-sided Star
braucht die vorhergesagte Pause oder die Zeit bis zum Tod; Flamethrower bindet 11 s an Ort und Blickrichtung, gegen
seine Spielweise nur bei stehendem Rudel ohne angesagte Fläche. *Konzept:* 14, „Pausen und Phasenenden".

### AoE-Schwelle an drei Orten · R, U

*Im Kampf:* nichts, solange die drei Wege dieselbe Schwelle meinen – die meisten Jobs über `AoeCount` im Zielsystem, der
Samurai über `NumberOfHostilesInRange >= n`, Blaumagier und Viper über eigene Zählungen. Eine Änderung an einem Weg
erreicht die anderen nicht. Keine Laufzeitfrage (A325). *Nächster Schritt:* je Job prüfen, ob die eigene Zählung
dieselbe Menge zählt wie `AoeCount`; erst dann zusammenführen. *Konzept:* 03, U5.

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
4,71 MB (7.5.7.3+wsh1) statt rund 2,3 MB (XML, Analyzer, PDB); Upstream verhält sich gleich. *Auflösung:* erst, wenn der
Veröffentlichungspfad prüfbar ist.

### VPR: leerer Zweig einer Struktur, die anderswo entscheidet · U
`VPR_Reborn.GeneralGCD`, Swiftskin's Bite und Sting. *Auflösung:* Adressat Upstream; bewusst nicht gelöscht (A11).

### Das Aktionsfenster rechnet die Bewegungssicherheit ein zweites Mal · U
`MainWindow.GetMovementSafetyStatus` neben `ActionTargetInfo.CheckMovementSafety`; die Regel „am Ziel ist ein Sprint nur
Schaden" fehlt im Fenster. *Auflösung:* `CheckMovementSafety` gibt Ergebnis und Grund heraus, das Fenster zeigt sie.

### Das Maschinisten-Modell bildet `MCH_Reborn` von Hand nach · R
*Kosten:* Ändert sich der Ruf der Königin oder die GCD-Folge in `MCH_Reborn`, rechnet `machinist_balance_model.py`
weiter die alte Regel, und Konzept 14 nennt seine Ergebnisse. *Auflösung:* jede solche Änderung trägt im selben Commit
die Anpassung des Modells. *Konzept:* 14.

### Plugin-Identität ist die des Upstreams · N
`manifest.json` unverändert (`InternalName`, URLs). *Auflösung:* seine Entscheidung, wenn beide Installationen
nebeneinander gebraucht werden.

## Warten auf Daten oder Fremde

- **Samurai nutzt Feint und Tengentsu nicht** (fremde Meldung, PR #11, unbestätigt; statisch geprüft A265). Auflösung:
  ein Protokoll eines Samurai-Kampfs („area defence open, nothing chosen" gegen „hit you"). Konzept 13.
- **Treffer aus einer Quelle, die die Objekttabelle nicht hält** (Verdacht; Necrophobia 10.10., Düsterflare 17.10.).
  Der Fall „in der Tabelle, aber keine Figur der Gegnerseite" ist seit A327 gelesen; offen bleibt der ohne Objekt.
  Auflösung: Protokollzeilen „hit you from a source the object table does not hold". Konzept 08.
- **BossModReborn: FATE-Modul in Occult Crescent läuft nach dem Weggang weiter** (am Quelltext `be8ff77` gefolgert).
  Adressat BossModReborn. Konzept 08, „Wann eine BossModReborn-Vorhersage ihn betrifft".
- **Ein gegnerisches Wirken stoppen: was die gespeicherten Tabellen lernen** (experimentell, seine Vorgaben 18.10. und
  23.10.2026, A329, A334, A337). Erstes Protokoll mit der Regel (23.10.2026): Sanctga 19-mal gewählt, Schlummer
  dreimal gelandet, vier Arten als immun gespeichert. Offen: ob Schlaf unterbrechbares Wirken stoppt (ein Versuch, nicht
  gewirkt), welche Anzahl bis zur Immunität die Regel lernt, welchen Rest beim Eintreffen. Auflösung: weitere
  Protokolle mit Heiler oder Magier in Instanzen, danach die Bewertung, ob die Regel experimentell bleibt. Konzept 08,
  „Ein gegnerisches Wirken stoppen".
