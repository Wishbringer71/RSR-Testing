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
vom 26.10.2026 („todo nach priorität und umsetzbarkeit in loops abarbeiten") läuft: Flamethrower ist gebaut (A380),
der Abgleich mit The Balance bis auf den Monk abgeschlossen (A381–A387, A390), zwei Schuldpunkte aufgelöst (A388,
A389), die AoE-Schwelle an die AoE-Aktion gebunden (A391), im Audit fünf Defekte behoben (A393–A395, A397, A399) und die Bodenflächen an die Gruppe gelegt (A398). Was nur den Monk betrifft, rückt nach hinten: Er spielt ihn bislang nicht (seine Angabe, 25.10.2026), die Wirkung
in seinem Profil ist heute null.

1. Audit der gesamten Codebasis
2. Rotationen gegen The Balance abgleichen (nur noch der Monk)
3. Six-sided Star

Danach technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

## Defekte

Keine bekannt.

## Offene Arbeit

### Rotationen gegen The Balance abgleichen · N

*Im Kampf:* Schaden – je Job Opener, Burstausrichtung und Prioritäten gegen den Guide. Abgeglichen: Maschinist
(A342), Opener von Dragoon und Viper (A343), AoE-Schwellen aller Jobs (A344), AoE-Wahl des Ninja (A345), Beschwörer
(A375), Weißmagier (A376), die vier Tanks (A377), die übrigen Heiler (A378), Ressourcen und Buffs der übrigen
Schadensausteiler (A379), die Burstfenster von Samurai, Barde, Schnitter, Ninja, Pictomancer und Schwarzmagier
(A381–A386), die Rotationseinstellungen mit Spieltatsache (A382, A383, A385, A387). *Nächster Schritt:* das
Burstfenster des Monk; die Vor-Pull-Aktionen der übrigen Schadensausteiler decken sich mit den Openern (A390), danach
folgen sie den abgeglichenen Prioritäten. Ausgeblendete Werte liest `ActionTooltip` zur Laufzeit aus den Tooltips, die
deutschen Namen gehören in `action_names_de.json`. *Konzept:* 14, „Vergleich mit The Balance".

### Six-sided Star · N

*Im Kampf:* Schaden des Monks vor einer Pause oder dem Tod des Ziels. Wert und Einsatzregel belegt (Job-Guide, The
Balance, 02.10.2026), nicht gebaut; braucht die vorhergesagte Pause oder die Zeit bis zum Tod. *Konzept:* 14, „Pausen
und Phasenenden".

### Audit der gesamten Codebasis · N, R, U

*Im Kampf:* unbekannt, bis gelesen – jeder Befund bisher war ein Defekt mit Wirkung (A8, A10, A82). *Stand:* Phasen
1 bis 4 abgeschlossen, `DataCenter` mechanisch und inhaltlich (A392–A396), `BaseAction` (A397), `ActionTargetInfo` bis zur
AoE-Zählung (A398, A399). *Nächster Schritt:* `ActionTargetInfo.FindTargetByType`, die Rotationen seiner häufigen
Jobs, die Oberfläche. *Konzept:* 19.

## Technische Schuld

Ohne Wirkung im Kampf oder bewusst eingegangen; je Kosten und Auflösungsbedingung.

### Searing-Light-Abdeckungsmodell bildet die Phasenbuchung von Hand nach · R
*Kosten:* Ändert sich die Buchung in `UpdateSearingPhaseBook`, `AllSearingPhasesHeld` oder `BurstPhaseHeldByAnother`,
rechnet `searing_light_coverage.py` weiter die alte Regel, und Konzept 12 argumentiert mit ihren Zahlen. *Auflösung:*
jede solche Änderung trägt im selben Commit die Anpassung des Modus „plugin". *Konzept:* 12.

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

### Die Modelle von Maschinist und Samurai bilden ihre Rotation von Hand nach · R
*Kosten:* Ändert sich der Ruf der Königin oder die GCD-Folge in `MCH_Reborn`, die GCD-Folge oder der Meikyo-Zweig in
`SAM_Reborn`, rechnen `machinist_balance_model.py` und `samurai_balance_model.py` weiter die alte Regel, und Konzept 14
nennt ihre Ergebnisse. *Auflösung:* jede solche Änderung trägt im selben Commit die Anpassung des Modells. *Konzept:* 14.

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
