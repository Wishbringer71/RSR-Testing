# TODO — nur offene Arbeit

Je Eintrag: was im Kampf anders wäre, wo es sitzt, der nächste Schritt oder die Auflösungsbedingung, das Konzept.
Begründungen, Messungen und die Geschichte stehen im Konzept und im Archiv (`AUDIT_LOG.md`), nicht hier. Ein
abgeschlossener Punkt verschwindet aus dieser Datei; das Konzept nennt dann den geschlossenen Stand.
Betroffene: **N** Endnutzer · **R** Autoren abgeleiteter Rotationen (Namensräume `RotationSolver.Basic`) · **U** Upstream-Pflege.

## Reihenfolge

Nach der Wirkung im Kampf: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit, in seinem Profil
(seine Vorgabe, 17.10.2026: „todo nach wichtigkeit abarbeiten. jeweils im loop bearbeiten").

Vorrang vor allem (sein Auftrag, 05.11.2026): „und jetzt mach endlich beastmaster bei den brettern spielbar. alle damit
zusammenhängende todos haben vorrang. auch die, welche im laufe des researchs hinzukommen. abarbeiten im jeweils
kritischen vollständigen loop." Alles, was den Beastmaster im Crucible betrifft – Defekte, Crucible-Strategien, der
Heiltrank allein, die Kaderwahl zum Leveln, was die Auswertung der Uploads findet –, kommt vor jedem anderen Punkt.
Dazu sein Auftrag vom selben Abend: „generell bei allen kämpfen prüfen ob die rota optimiert werden kann und die
auswahl und das timing der beschwörung" – je Kampf aller fünf Bretter (Gegner, Schwächen, Aktionen aus den Blättern,
BossMod-Module) die Rotation des Beastmasters, die Wahl der Begleiter und den Zeitpunkt ihres Aufrufs im Loop prüfen
(Beispiel seiner Beobachtung: der Raptor gegen Feuer, bevor feueranfällige Gegner da waren; A581).

1. Blaumagier: RSRs nächste Aktion beendet Apokalypsis und Chelonian Gate – Schaden und Minderung des Spielers enden
   nach einem GCD statt nach 10 s.
2. Kegel und Ringe gegnerischer Zauber genau messen: Zauber- und Kanalbeginn nutzen RSRs Erkennung bei Kegel und Ring
   nicht, und mehrere Leser sehen hinter einem Kegel oder im Innenkreis eines Rings eine Gefahr, die keine ist.

Danach die technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

Ablauf danach (sein Auftrag, 05.11.2026): „nach abarbeitung der aktuellen todo, alle restlichen todos abarbeiten, dann
konzeptverbesserungen durchgehen, audit, codereview. dann wieder todo abarbeiten. alles in vollständigen loops" – also
der Beastmaster-Vorrang, dann jeder übrige Punkt dieser Liste, dann die Konzepte auf Verbesserungen, ein Audit und ein
Code-Review; was diese finden, kommt wieder hierher und in dieselbe Reihenfolge, jeder Schritt im vollständigen Loop.

## Defekte

### Blaumagier: RSRs nächste Aktion beendet Apokalypsis und Chelonian Gate · N
Im Kampf: Drückt der Blaumagier Apokalypsis (Linie, 10 s Schaden) oder Chelonian Gate (−20 % Schaden, 10 s, danach
Auspicious Trance), beendet RSRs nächste Aktion den Kanal („Effect ends upon using another action or moving"); RSR wirkt
beide nicht selbst, und keine Aktionssperre hält sie. Beim Flammenwerfer hält RSR dagegen jede Aktion außer den
Sicherheitswegen zurück (`DataCenter.FlamethrowerChannelRunning`, `BaseAction.CanUse`, „KeepsTheFlamethrowerChannel");
die Regel sitzt beim einen Job statt bei allen Kanälen, die ein Spieler laufen lässt. Die Bewegungssperre hält diese
Kanäle; ohne Aktionssperre endet ihr Nutzen mit RSRs nächster Aktion. *Ort:* `DataCenter.FlamethrowerChannelRunning`,
`BaseAction.CanUse`, `MedicineItem.CanUseThis`, `MovementLock.Channels`. *Nächster Schritt:* Loop, ob die Flammenwerfer-Regel
auf jeden Schadens- und Schutzkanal gehört, den RSR nicht selbst beendet (Universell zuerst), mit den Sicherheitswegen.
*Konzept:* 14, „Wo Angriff die Abwehr beendet: die Kanäle".

## Offene Arbeit

### Kegel und Ringe gegnerischer Zauber genau messen · N
Im Kampf: RSRs Formprüfung (`DataCenter.AreaCastReaches`) misst einen Kegel und einen Ring als vollen Kreis um den
Gegner; wer hinter einem Frontalkegel oder im sicheren Innenkreis eines Rings steht, zählt als getroffen. Deshalb sperrt
ein solcher Zauber keinen Zauber- und Kanalbeginn (`DataCenter.CastCertainlyDodgeable` nimmt nur Kreis und Linie), und
die Leser von `RaiseProtection.FirstDodgeableWithin` (Limit Break allein, Freigabe der Bewegungssperre) sehen dort eine
Gefahr, die keine ist. *Ort:* `DataCenter.AreaCastReaches`. *Nächster Schritt:* Winkel des Kegels und Innenradius des Rings
aus dem Omen der Aktion (Blatt Omen, Pfadname) belegen, dann messen und die Einschränkung in `CastCertainlyDodgeable`
aufheben. *Konzept:* 03, „Bewegungssperre beim Wirken und die KI von BossMod"; 13 (Formprüfung).

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

### VPR: leerer Zweig einer Struktur, die anderswo entscheidet · U
`VPR_Reborn.GeneralGCD`, Swiftskin's Bite und Sting. *Auflösung:* Adressat Upstream; bewusst nicht gelöscht (A11).

### Die Modelle von Maschinist, Samurai und Monk bilden ihre Rotation von Hand nach · R
*Kosten:* Ändert sich der Ruf der Königin oder die GCD-Folge in `MCH_Reborn`, die GCD-Folge oder der Meikyo-Zweig in
`SAM_Reborn`, Perfect Balance, die Buffs oder die GCD-Folge in `MNK_Reborn`, rechnen `machinist_balance_model.py`,
`samurai_balance_model.py` und `monk_balance_model.py` weiter die alte Regel, und Konzept 14 nennt ihre Ergebnisse. *Auflösung:* jede solche Änderung trägt im selben Commit die Anpassung des Modells. *Konzept:* 14.

### Der erste Kampf einer Sitzung hält einen Durchlauf bis 170 ms an · N
In jedem Upload vom 01. bis 04.11.2026 und bei jedem Job: der langsamste Durchlauf des ersten Kampfes nach dem Start
20 bis 170 ms, fast ganz im Schritt „next action (rotation)", ohne Speicherbereinigung; danach unter 10 ms. *Kosten:* ein
Ruckler am ersten Pull je Sitzung. *Auflösung:* Loop, sobald er einen Ruckler am Pull bemerkt oder die erste Aktion
messbar später kommt; erster Schritt: die Teilschritte der Rotation in diesem Durchlauf messen. *Konzept:* 03, „Was ein
Frame kostet".

### Plugin-Identität ist die des Upstreams · N
`manifest.json` unverändert (`InternalName`, URLs). Ein eigener interner Name verlegt den Ordner mit Einstellungen und
gelernten Tabellen, ein eigener angezeigter Name nimmt BossModReborn die Verbindung (es erkennt RSR an „Rotation Solver
Reborn"); beides kostet im Spiel, solange nicht beide Installationen gebraucht werden (A533). *Auflösung:* wenn er beide
nebeneinander braucht: neuer interner Name mit Übernahme des Ordners, angezeigter Name bleibt.

## Warten auf Daten oder Fremde

- **Crucible: Flucht, Tötungsstapel und Zauberbücher an seinem Log nachprüfen** (A671, A677, A678; seine Vorgabe vom
  07.11.2026: „vorab im vollen kritischen loop prüfen, und auch nach einspielung der logs"). Lager, Weg, Endkampfplan
  und die übrigen Gegenstandsregeln liefen in seinen sechs Läufen vom 08.11.2026 (A683). Im Kampf ungesehen blieben drei
  Regeln:
  - die Flucht mit der Rauchbombe über das Brett;
  - ein Tötungsstapel, den ein Gegenstand gewährt;
  - ein Treffer, den ein Zauberbuch wandelt.

  Auflösung: Zeilen „a kill by an item granted", „a hit the … tome converted" und die Flucht in einem Lauf, der diese
  Gegenstände hält; dann je Abweichung Loop. Konzept 21, K8 („Über das Brett", „Zauberbücher"), Weg.

- **Beastmaster: Bretter 3 bis 5 und die Meisterbretter am ersten Mitschnitt prüfen** (Crucible-Strategien K1 bis K14,
  zuletzt A643, A644). Brett 2 lief am 08.11.2026 (A683): Die Tablitauren fielen gemeinsam, die Aetheric Charges zählten
  nach ihrer gemessenen Gefahr. Gebaut, aber an keinem seiner Läufe gesehen, sind diese Regeln (Quellen: Blätter,
  BossMod-Module, Daedalus' Feldnotizen, GluttonyCombos Aufzeichnungen):
  - Tötungsregeln für Lamien, Morpho und Abaddon, Myconid in der Pfütze und die Sanduhr des Todes beim
    Königs-Ahriman-Ebenbild (A675);
  - der Gegenangriff nach Angriffsart;
  - die Eier des Zu.

  Auflösung: `DefenseTrace.log` und ACT-Mitschnitt seines ersten Laufs auf einem dieser Bretter, mit den Zeilen
  „kill rule: … held/released" (auch „a break now could fire Death", „roulette needle"), „left alone", „crucible route"
  und den Toden; dann je Abweichung Loop. Dazu der Vergleich mit LazyCrucibles Gewicht nach Hornbedarf für Futter und
  Rast (A679). Konzept 21 („Crucible of the Unbroken: Bretter, Kämpfe, Strategien", K3, K12).

- **Teraflare bricht auf Shinryu Unreal ab: welcher Abbrecher** (seine Meldung, 06.11.2026; A629). Im Kampf: In der
  letzten Phase startet sein LB3 und bricht wieder ab, mehrfach. RSR reicht Limit Breaks unverändert ans Spiel und
  schreibt jeden eigenen Abbruch mit Grund. Offen ist, ob BossMods KI ausweicht (Spielraum kürzer als die Restwirkzeit von
  4,5 s) oder BossMod abbricht. Auflösung: `DefenseTrace.log` und Dalamud-Log eines solchen Versuchs („cast cancelled by
  RSR" gegen „[AI] Movement will interrupt cast" oder „[CancelCast]"); liegt es an BossMod, Meldung dort. Konzept 03,
  „Limit Break allein", Absatz „Von Hand gedrückt".

- **Tiefe Gewölbe: welcher Weihrauch auf welchem Platz in Pilgers Pfad** (A626). Im Kampf: Allein in Pilgers Pfad setzt
  RSR Barkbalm vor dem Bosskampf und Poisonfruit in der Gefahr nur, wenn es den Platz lesen kann; das Blatt nennt dort
  keine Zuordnung, und Poisonfruit und Barkbalm teilen den Wert 5. Gelesen wird über den Statusagenten des Gewölbes, ob
  der ohne offenes Fenster gefüllt ist, ist unbelegt. Auflösung: die Zeilen „deep dungeon stones" eines Laufs in Pilgers
  Pfad mit Weihrauch im Bestand; dann die Zuordnung fest aus Wert und Agent. Konzept 23, „Tiefe Gewölbe: Magicite,
  Demiklone und Weihrauch".

- **Blaumagier: 1000 Nadeln gewirkt, obwohl nicht im Set** (sein Hinweis, 03.11.2026; A506). Im Kampf: RSR wirkt
  laut ihm einen Zauber außerhalb des Sets; der Code lässt das nicht zu. Auflösung: die Zeilen „blue mage set" und
  „blue mage spell outside the active set" im nächsten `DefenseTrace.log` als Blaumagier. Konzept 01, „Blaumagier",
  „Das Set".

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
