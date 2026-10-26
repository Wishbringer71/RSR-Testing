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
vom 25.10.2026 abends zu Chorts Türmen und dem Glockenspiel ist beantwortet und umgesetzt (A365). Die Hilfe auf einen
kleinen gemessenen Tankbuster ist gebaut, für Heiler und Tanks (A373), die Reflexion daraus ebenso (A374). Sein Auftrag
vom 26.10.2026 („todo nach priorität und umsetzbarkeit in loops abarbeiten") ist erledigt: Flamethrower und Six-sided
Star sind gebaut (A380, A413), der Abgleich mit The Balance ist für jeden Kampfjob abgeschlossen (A381–A387, A390,
A412), drei Schuldpunkte aufgelöst (A388, A389, A410), ein vierter für den eigenen Code (A415), die AoE-Schwelle an die AoE-Aktion gebunden (A391), die
Bodenflächen an die Gruppe gelegt (A398) und das Audit der gesamten Codebasis abgeschlossen, mit den Defekten aus A393–A395,
A397, A399, A401–A409 und A412 (A414).

Offene Arbeit gibt es keine; als Nächstes die technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten
auf Daten: unten.

## Defekte

Keine bekannt.

## Offene Arbeit

Keine.

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
Bewertung und Reichweite sind gebaut (Konzept 13); die Aufnahme bleibt grob. *Auflösung:* ein Unterscheidungsmerkmal aus
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

- **`DefenseTrace.log` vom 26.10.2026 ohne eine Zeile nach dem Kopf** (Build `01db0e4e3`, drei Kämpfe in der Porta
  Decumana laut ACT). Jeder Kampf schreibt an seinem Ende zwei Zeilen „update cost over the fight". Ursache von hier
  nicht bestimmbar (A416). Auflösung: die Datei des nächsten Abends; fehlen die Zeilen wieder, ist es ein Defekt im
  Schreiben, sonst war die hochgeladene Datei eine frühe Kopie.

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
