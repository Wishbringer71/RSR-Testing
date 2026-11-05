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

1. Allein ohne Heiler kommt kein Heiltrank: RSR stand 13 s in der kritischen Klasse ohne Heilfähigkeit, kein Trank
   von RSR trotz eingeschalteter Option, er drückte ihn bei 6 % selbst und starb (Beastmaster, Sägerschrei, 04.11.2026).
2. Beastmaster: Crucible-Strategien: Ein K.O. beendet den Lauf, und ein Begleiter, den ein späterer Kampf braucht, kommt dort mit den LP an, die ihm
   die früheren ließen.
3. Fremde Repositories auf Verbesserungen für RSR prüfen: Jeder Fund kann ein Defekt im Verhalten einer Klasse sein,
   den RSR heute im Kampf zeigt; zuerst die Rotationswerkzeuge für alle Jobs, Daedalus vorn (aktiv, alle Jobs,
   eigene Kampfvorhersage), dann Beastmaster und Blaumagier (sein Auftrag vom 05.11.2026).
4. Konzepte neu ordnen (allgemein, Rollen, Gruppen, Jobs, Inhalte): keine direkte Wirkung im Kampf; eine Regel, die
   im Konzept eines Jobs steht, wird für dessen Gruppe übersehen, und eine Inhaltsregel ist über mehrere Konzepte
   verteilt.

Danach die technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

## Defekte

Keine offenen.

## Offene Arbeit

### Allein ohne Heiler kommt kein Heiltrank · N, U
Im Kampf: Beastmaster allein gegen Myrmidonen-Prinzessin und Gefolge (Sägerschrei, 04.11.2026); seine LP fielen in 75 s
von 93 % auf 6 %, ab 18:28:18 meldete RSR „critical class … no heal ability", den Ultratrank drückte er bei 135 von 2.415
LP selbst, er starb 2 s später. „Use HP potions" ist bei ihm an (seine Angabe, 05.11.2026), und RSR trank den Ultratrank
am 23.10. und 03.11. selbst, stets unter 40 %; an allen Beastmaster-Sitzungen vom 04.11. kein einziges Mal, auch nicht
in den Webfenstern 18:28:02–15 bei 36 bis 25 % LP. *Ort:* `CustomRotation_Ability.Ability` vor `UseHpPotion`,
`HpPotionItem.CanUse`, `BaseItem.CanUse`; welche Bedingung hielt, zeigt der Upload nicht. *Nächster Schritt:* Die Zeile
„critical class" für den Spieler nennt je Trank im Inventar die erste Sperre (Schalter, Schwelle,
Fehl-LP, Spielstatus) und den Tinktur-Vorrang; mit dem nächsten Upload die Ursache belegen, dann Loop: der Trank
nach der Gefahr statt nach der festen Schwelle, für alle Jobs. *Konzept:* 03 („Eine Wiederverwendung für alle
Arzneien"), 07.

### Beastmaster: Crucible-Strategien · N
Im Kampf: Fällst du im Brett, ist der Lauf verloren, und RSR plant die Begleiter nicht über das Brett: Ein Heiler, der im vierten Kampf gebraucht wird, kann bis dahin auf 25 von 550
LP gefallen sein (04.11.2026). *Ort:* `BST_Reborn`, `CrucibleBoard`, `BeastmasterCrucible*.cs`. *Gebaut:* Stufe 1 (A548,
A549: Daten zur Laufzeit, K1, K2, K3 ohne den Bedarf der Begleiter), K9 (A550), K8 (A551 bis A554: Heilung, Gegenmittel, Bestien-Lebensfeder, Sand der
Zeit, Bestien-Heiltranksammlung, Schutz, Seren, Bestien-Heilmittelsammlung, Resistenzmittel, Bestien-Goldnadel,
Bestien-Ausdauermittel, Schadensgegenstände, Stärkungsmittel im Endkampf; A555).
*Gebaut auch:* K6 (A562, Hörner im Hornfenster), K7 (A563, Kader auf Wunsch im Teamfenster), K10 Campsite (A564, wer mitruht), K10 Laden (A569, A570: Kaufliste im Ladenfenster, Kauf auf Wunsch), K12 (A572, Eier des Zu), K13 (A578, Schatz: Empfehlung im Schatzfenster, Wahl auf Wunsch). *Nächster Schritt:* K4 (Sonde A557 „crucible: … stopped its cast" zeigt die unterbrechende Aktion), K5; K10 Weg ist eine Frage des Umfangs an ihn, am Ende des TODO; K11 (Kader, LP je Begleiter und Feld: Sonde A556 „crucible agents …" zeigt, ob das Spiel sie im Brett füllt;
danach Loop am Ende des TODO). *Konzept:* 01,
„Crucible of the Unbroken: Bretter, Kämpfe, Strategien".

### Fremde Repositories auf Verbesserungen für RSR prüfen · N, R, U
Sein Auftrag (02.11.2026): „alle von dir gefundenen repos, die du benannt hast prüfen, ob daraus generell
verbesserungen für rsr entstehen könnten, für die grundstruktur, für die rotationen und das verhalten der einzelnen
klassen". Im Kampf: Was ein anderes Werkzeug für eine Mechanik, eine Lage oder einen Job gelöst hat, fehlt RSR
vielleicht; jeder Fund wird am Spiel belegt, bevor er ein Defekt oder eine Verbesserung heißt. Code nur lesen, nicht
übernehmen (Lizenzen: GPL, AGPL, LGPL, MIT, ohne Lizenz). Die Repositories, nach Zweck:
- *Rotationen für alle Jobs:* ofnature/Daedalus, PunishXIV/WrathCombo, ahwootech/BRUTAL, BeirutaNa1na/RotationSolverReborn
  (Fork von Upstream), RhyTamok/XIVdrg.
- *Beastmaster:* Kriss-Hietala/BeastmasterAssist, Rin-0617/BeastHelper, lexxxgo/Masterbeast; Daedalus und WrathCombo
  dazu ausgewertet (A488); neu (A532): erdelf/AutoDuty (Crucible-Automatik), GluttonyCombo (`BST_CrucibleAdvisor`,
  Schwäche je Ziel), BOCXIV/Beastmaster-JP (Brett-Gegenstände), Namen aus BeastHelper; vollständige Suche offen.
- *Blaumagier:* die Rotationswerkzeuge oben und BossMod sind ausgewertet (A506 bis A511); eigene Repositories zum
  Blaumagier (Zaubersets, Masked Carnivale) noch zu suchen.
- *Kampfvorhersage und Unterbrechen:* FFXIV-CombatReborn/BossmodReborn, awgil/ffxiv_bossmod, sourpuh/ffxiv_interjection.
- *Auswertung von Kämpfen:* xivanalysis/xivanalysis, OverlayPlugin/cactbot, xiv-stats/xiv-damage-samples,
  Ahriqi/astbalanceimages.
- *Spiel-API und Spieldaten:* goatcorp/Dalamud, aers/FFXIVClientStructs, NightmareXIV/ECommons, NotAdam/Lumina,
  xivdev/EXDSchema, xivapi/ffxiv-datamining, SapphireServer/Sapphire (Server-Nachbau: Formeln und Abläufe, keine
  Spielquelle), LTS-FFXIV/RebornMaterial.
- *Automatisierung und Werkzeuge:* Knightmore/Henchman, Knightmore/Underlings, getsetmind/dalamudmcp,
  t3knical/DalamudPlugins.
Sein Auftrag vom 05.11.2026 dazu: „alle bekannten repos zu rotationen, zu beastmaster, zu bluemage analysiere, inwieweit
erkenntisse daraus in rsr eingebracht werden können und die vorhandenen konzepte ergänzen und verbessern könnten".
*Ort:* Grundstruktur (`RotationSolver/Basic`, Updaters, Zielwahl, Abwehr), die Rotationen unter `RebornRotations`, die
Jobkonzepte. *Nächster Schritt:* Daedalus für Sicherheit ausgewertet (A561; A570 bis A575: Ladengewichte und Brettzustände,
Stacheln, Flächen und Zu-Eier, Heiler-Triage, Reinigung, Abwehr und Kampfvorhersage); offen sind je Job die Regeln, die RSR
nicht kennt, Job für Job nach seinem Profil (Maschinist zuerst, er spielt ihn am häufigsten); danach die übrigen in der
Reihenfolge der Liste. Maschinist in Arbeit (Daedalus 74659a6, 03.11.2026): Full Metal Field und Reassemble treffen RSR
nicht (Job-Guide: „not affected by Reassemble", RSR setzt es nicht darauf). Offen: die Königin auf einer Gruppe, die vor
ihrem Abschluss stirbt – `MCH_Reborn` überspringt die TTK-Sperre der Einstellung (seit 6d77e5089); Queen Overdrive vor
dem Ende der Gruppe, das die Sperre von 16 s heute gerade dann verweigert; das Zurückhalten vor einer vorhergesagten
Pause (The Balance); ob die Batterie außerhalb des Kampfes bleibt, ist unbelegt (Messung im Code); das Maß „letzter
Gegner im Kampf stirbt" statt des Mittelwerts `AverageTTK` für alle Jobs prüfen. Konzept 14, Abschnitt Maschinist. Je Fund ein Loop, je Repository ein Archiveintrag mit Commit und Datum. *Konzept:* 15, erweitert vom Vergleich mit WrathCombo auf die übrigen Werkzeuge; je Fund dazu das
tragende (01 Jobs, 08 Minderung, 13 Flächenschaden und Tankbuster).


### Konzepte neu ordnen: allgemein, Rollen, Gruppen, Jobs, Inhalte · N
Sein Auftrag (05.11.2026): „generelle prüfung aller konzepte, ob eine neuorganisation nicht sinnvoll wäre: allgemein für
alle, dann nach gruppierungen heiler, tank, melee (drei sorten = monk+samurai, schnitter+dragoon, ninja+viper),
fernkämpfer, magier. dann weiter unterteilt auf jede einzelne klasse und auch die sonderklassen wie bluemage und
beastmaster. parallele konzepte für dungeons, endgegner, savage, occult crescent. immer da, wo bedarf besteht und mit
querverweisen zu den anderen betroffenen konzepten. das in einem eigenen konzept im vollen kritischen loop mit antithese
und versuch der widerlegung analysieren und bei sinnhaftigkeit planen". Im Kampf mittelbar: Was im Konzept eines Jobs
steht (etwa die Zielart ohne Tank und Heiler für Beastmaster und Blaumagier, das Crucible im Beastmaster-Teil), sucht
man für eine Gruppe oder einen Inhalt nicht dort; eine Regel, die so übersehen wird, fehlt dem nächsten Job. *Ort:*
`docs/rotation-flow/` (19 Konzepte nach Leitfrage, `01-jobs.md` allein für alle Jobs), Index `README.md`,
`check_concept_links.py`, Verweise aus Code, Archiv und TODO auf Konzeptabschnitte. *Nächster Schritt:* eigenes
Konzept (kein bestehendes trägt die Ordnung der Konzepte; der Index ist Navigation), Loop: Ist-Erhebung (Abschnitte je
Ebene, Doppelungen, Verweise), Optionen mit Nullvariante, je Option Antithese und Widerlegungsversuch (Verweise
veralten, Abschnitte doppeln sich, Prüfskripte und Archiv nennen Pfade), Plan nur bei Sinnhaftigkeit. Die Stufung folgt
seiner Vorgabe „Universell zuerst" (26.09.2026, CLAUDE.md), die bisher nur den Code ordnete: Die Stufen sind gesetzt,
der Loop klärt, wie die Konzepte ihnen folgen und wo Inhaltskonzepte daneben stehen. *Konzept:* `README.md`, neues Konzept zur Ordnung.

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

- **Blaumagier: Set je Stufe im Masked Carnivale** (seine Frage, 03.11.2026; A511). Im Kampf: Vor einer Stufe stünden
  die gelernten Zauber im Set, gegen die ihre Gegner schwach oder verwundbar sind. Offen ist, was die Gegner-Ids der
  Einweisung sind. Auflösung: die Zeile „carnivale briefing" im nächsten `DefenseTrace.log`, nachdem er die Einweisung
  als Blaumagier geöffnet und eine Stufe gewählt hat; dann Loop zur Wahl als eigene Option. Konzept 01, „Masked
  Carnivale".

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
