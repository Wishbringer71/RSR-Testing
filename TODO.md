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

1. Crucible-Wechselwirkungen, Stufe 2 – Ausrüstung, Futter, Weg und Zielwahl lesen die Paare noch nicht (Beastmaster,
   Vorrang); danach die Crucible-Zielfolge (Final Hourglass, Roulette, Aetheric Charge).
2. Blaumagier: RSRs nächste Aktion beendet Apokalypsis und Chelonian Gate – Schaden und Minderung des Spielers enden
   nach einem GCD statt nach 10 s.
3. Kegel und Ringe gegnerischer Zauber genau messen: Zauber- und Kanalbeginn nutzen RSRs Erkennung bei Kegel und Ring
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

### Crucible-Zielfolge: Final Hourglass, Roulette, Aetheric Charge · N
Im Kampf: Gegen Königs-Ahriman (Zweites Meisterbrett) soll Final Hourglass zuerst fallen und Roulette nicht getroffen
werden (BossmodReborn Tekz 1.0.68.6; eorzeantavern: die Sanduhr fällt, wenn Roulettes Zeiger nicht auf dich zeigt);
gegen den Taurus (Brett 2) Aetheric Charge vor den übrigen, bevor sie in 6, 12, 18 Yalm platzt und Damage Down legt.
RSRs Gefahrenfolge (K3) sieht beide nicht, weil sie bis zum Platzen keinen Schaden machen. Nächster Schritt: Loop –
Mechanik am Modul und an den Blättern (Zeiger, Platzen), allgemeine Regel „vor dem Platzen fallen", Kampfbeispiel.
Konzept 21, K3 und K12; Quelle A667.

### Crucible-Wechselwirkungen, Stufe 2 · N
Im Kampf und auf dem Brett: Die Skala in Laden und Schatzfeld zählt die Heiltranksammlung nicht (Auto-potion: 40 % erst
unter 50 % LP, nur gegen einen Boss) und damit auch nicht das Diebesgewand, das sie verlängert; Tötungsstapel (Dämonische
Rüstung, Stimulanztrunk-Simulakrum) zählen weder in der Zielwahl noch im Wert eines Flächengegenstands; Feiglingsmesser,
Rauchbombe und Diebesmesser zählen auf dem Weg nicht; die Zauberbücher und die Ungezähmten Mittel nimmt RSR nicht; der
Wert eines Schadensgegenstands zählt einen Gegner mit weniger LP als der Treffer voll (Milben: 522 LP, 1.304 bis 1.600
Schaden). Nächster Schritt: je Paar der Loop in der Reihenfolge Sicherheit vor Schaden – Heiltranksammlung auf der Skala, Flucht auf dem Weg, Tötungsstapel und Überschuss, Zauberbücher mit Messung nach dem Einsatz,
Ungezähmte Mittel unter Schutz. Konzept 21, K8 („Wechselwirkungen", A666), K10, K14, Weg.

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

- **Crucible: Lager, Weg, Endkampfplan und Gegenstände an seinem Log nachprüfen** (A661 bis A665; seine Vorgabe vom
  07.11.2026: „vorab im vollen kritischen loop prüfen, und auch nach einspielung der logs"). Sein Lauf von 18:08 lief
  auf einem Build davor und bestätigte die Ursachen (A665). Im Kampf: mehr Begleiter an der Campsite, der Schatz statt
  der Rast, wo beide sicher sind, der Plan des Endkampfs gleich der Wahl am Endkampf, übrige Heilung vor dem Endkampf,
  Schadensgegenstände nach Wert über die Kämpfe und erst, wenn kein Gegner mehr in ihre Reichweite kommt, Eis vor Feuer,
  das Zaubermittel vor dem Schaden, nichts übrig nach dem Endkampf. Auflösung: `DefenseTrace.log` und ACT-Mitschnitt
  seines nächsten Laufs ab A665 – die Zeilen „crucible fight …: … stored", „crucible campsite", „crucible route",
  „Crucible final fight's plan", „crucible item" (auch „waits"), „crucible items at the start of/after fight"; dann je
  Abweichung Loop. Konzept 21, K8 („Über das Brett", „Zeitpunkt im Kampf"), K10, K14, Weg.

- **Beastmaster: Bretter 2 bis 5 am ersten Mitschnitt prüfen** (Crucible-Strategien K1 bis K14 gebaut, zuletzt A643,
  A644). Im Kampf: Tötungsregeln (Tablitauren, Lamien, Morpho und Abaddon, Myconid in der Pfütze), Gegenangriff nach
  Angriffsart und die Eier des Zu sind aus Blättern, BossMod-Modulen und Daedalus' Feldnotizen gebaut, aber nicht an einem
  seiner Läufe gesehen. Auflösung: `DefenseTrace.log` und ACT-Mitschnitt seines ersten Laufs auf einem der Bretter 2 bis 5
  – die Zeilen „kill rule: … held/released", „left alone", „crucible route" und die Tode; dann je Abweichung Loop.
  Konzept 21 („Crucible of the Unbroken: Bretter, Kämpfe, Strategien").

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

- **Beastmaster: Ob Analysieren die LP einrechnet** (A546). Im Kampf: Anfreunden markiert sicher, aber vielleicht früher
  als nötig oder mit weniger Chance als möglich; jeder verlorene Pakt kostet einen weiteren Lauf (seine Vorgabe:
  Anfreunden hat oberste Priorität). Gebaut ist die Sonde: Analysieren auf eine fehlende Bestie beim ersten Sehen und
  nach der Markierung, Antwort und LP als Zeilen „gauge: …", dazu die Antwort des Spiels auf ein zweites Anfreunden.
  Auflösung: Zeilen „gauge" eines Abends mit fehlenden Bestien; dann Loop, ob die Stufe oder ein zweites Anfreunden in
  A2 eingeht. Konzept 01, „Was gilt beim Anfreunden".

- **Beastmaster: Nach dem Kampf ohne One with Nature in den nächsten** (A543). Im Kampf: Der letzte Begleiter bleibt
  nach einem Kampf ohne One with Nature draußen; am nächsten Pull geht er beim ersten Webfenster mit Scheidendem Schlag,
  dann kommt der Begleiter nach Lage (vor einem Rudel der aufhaltende, A542) – etwa ein GCD und der Ruf (1 s) später, als
  wenn er schon vor dem Pull mit One with Nature draußen stünde. Ein Ruf außerhalb des Kampfes braucht einen Rückzug ohne
  feindliches Ziel: Scheidender Schlag geht nur im Kampf (Wirktext 44891), der einzige Kandidat ist der allgemeine
  Begleiterbefehl „Away" (PetAction 1), keinem Begleiter zugeordnet. Auflösung: ob die Begleiterleiste des Beastmasters
  einen Befehl zum Zurücktreten führt (im Spiel sichtbar); dann Loop zum Ruf vor dem nächsten Gegner (B5). Konzept 01,
  „Belegung der Hörner in der Rotation".

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
