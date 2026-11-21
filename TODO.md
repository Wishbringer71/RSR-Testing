# TODO — nur offene Arbeit

Je Eintrag: was im Kampf anders wäre, wo es sitzt, der nächste Schritt oder die Auflösungsbedingung, das Konzept.
Begründungen, Messungen und die Geschichte stehen im Konzept und im Archiv (`AUDIT_LOG.md`), nicht hier. Ein
abgeschlossener Punkt verschwindet aus dieser Datei; das Konzept nennt dann den geschlossenen Stand.
Betroffene: **N** Endnutzer · **R** Autoren abgeleiteter Rotationen (öffentliche Schnittstelle von `Engine`, `Configuration` und den Basisklassen der Rotationen) · **U** Upstream-Pflege.

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

Was auf Daten oder auf ihn wartet, steht unten.

Danach die technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

Ablauf danach (sein Auftrag, 05.11.2026): „nach abarbeitung der aktuellen todo, alle restlichen todos abarbeiten, dann
konzeptverbesserungen durchgehen, audit, codereview. dann wieder todo abarbeiten. alles in vollständigen loops" – also
der Beastmaster-Vorrang, dann jeder übrige Punkt dieser Liste, dann die Konzepte auf Verbesserungen, ein Audit und ein
Code-Review; was diese finden, kommt wieder hierher und in dieselbe Reihenfolge, jeder Schritt im vollständigen Loop.

## Defekte

## Offene Arbeit

- **Jede Rotation gegen The Balance messen** (seine These, 20.11.2026: „meine these ist nun, dass alle von dir optimierten
  rotas schlechter als die von the balance sind. beweise das gegenteil!“; A950). *Im Kampf:* Wo die Rotation weniger
  Potenz über den Kampf bringt als der Guide, fehlt Schaden in jedem Kampf. *Stand:* Ein Vergleich in Potenz über den
  Kampf besteht nur für Samurai (The Balance im Mittel +0,29 %), Monk (+0,03 %) und – ohne Potenzurteil – den
  Maschinisten (Batterie); für 20 Jobs keiner. *Nächster Schritt:* je Job ein Modell nach dem Muster von
  `samurai_balance_model.py` (Folge von The Balance gegen die der Rotation, ein Boss, Potenzen aus dem Job-Guide, Kampflänge
  und GCD variiert), Boss-Jobs seines Profils zuerst; wo The Balance vorn liegt, die Regel verbessern, bis sie nicht mehr
  dahinter liegt (sein Ziel vom 19.11.2026). Samurai zuerst: die Meikyo-Regel liegt im Mittel 0,29 % zurück
  (Konzept 14, „Samurai: Meikyo Shisui und Higanbana“, dort „nicht gebaut“ vor seinem Ziel). Vorrang haben die
  Stellen, an denen eine Rotation ein Burstfenster gegen das Raster von The Balance verschiebt oder bindet (warten,
  binden, vorziehen): Sie brauchen den Modellbeleg (Konzept 03, „Seine Frage, ein strategischer Fehler?“, A958).
  *Konzept:* 14.

## Technische Schuld

Ohne Wirkung im Kampf oder bewusst eingegangen; je Kosten und Auflösungsbedingung.

### Rotmagier: Manaschwellen aus Kosten und Gewinn herleiten · N
*Kompromiss:* Die Schwellen für den Beginn der Nahkampf-Kombo (60, 50, 70, 90 nach der Zeit bis Embolden; Pooling
92|81) sind Upstreams feste Zahlen (A957). *Kosten:* Weicht der Managewinn je GCD ab (Stufe, Zaubertempo), kommt die
Kombo zu früh oder zu spät vor das Fenster. *Auflösung:* Schwellen aus Kombokosten und Mana je GCD bis Embolden, Modell
gegen The Balance (73|73 ab 15 s vorher, 92|81 für die Doppel-Kombo). *Konzept:* 14 („Rotmagier: Mana vor Embolden“).

### Searing-Light-Abdeckungsmodell bildet die Phasenbuchung von Hand nach · R
*Kosten:* Ändert sich die Buchung in `UpdateSearingPhaseBook`, `AllSearingPhasesHeld` oder `BurstPhaseHeldByAnother`,
rechnet `searing_light_coverage.py` weiter die alte Regel, und Konzept 12 argumentiert mit ihren Zahlen. *Auflösung:*
jede solche Änderung trägt im selben Commit die Anpassung des Modus „plugin". *Konzept:* 12.

### Status-Einstellungen auf der falschen Seite der Aktion · N, U
Fundstellen in Bozja (`scan11.py`), außerhalb seines Profils; je eine Richtungsentscheidung, die eine Beobachtung im
Inhalt verlangt. *Auflösung:* erst, wenn er Bozja nennt. *Konzept:* 19.

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
den Spieldaten des Gebiets. *Konzept:* 03, „Inhalt eines Gebiets".

### Doppelte Zustandswahl in den Zustandskommandos · N
`AdjustStateType` und die fünf `Cycle*`-Methoden lösen dieselbe Aufgabe; `DTRAllAuto` mit `ToggleAuto` kollabiert auf
zwei Zustände. *Auflösung:* erst mit Laufzeitbeobachtung des Zustandsautomaten; `DTRType` und `CycleType` sind
Ordinalvertrag. `AutodutyUpdateState` dupliziert `UpdateState` und löst sich mit. *Konzept:* 03.

### Zwei Werte des Dunkelritters hängen nicht an ihrer Strategie · N
`BlackestNightMinHostiles` und `BlackestNightHealthRatio` erscheinen ohne `Parent`; `ParentValue` kennt nur einen Wert.
*Auflösung:* beim Tooltip belassen, solange kein zweiter Fall auftritt. *Konzept:* 10.

### VPR: leerer Zweig einer Struktur, die anderswo entscheidet · U
`VPR_Reborn.GeneralGCD`, Swiftskin's Bite und Sting. *Auflösung:* Adressat Upstream; bewusst nicht gelöscht (A11). *Konzept:* 19.

### Die Modelle von Maschinist, Samurai und Monk bilden ihre Rotation von Hand nach · R
*Kosten:* Ändert sich der Ruf der Königin oder die GCD-Folge in `MCH_Reborn`, die GCD-Folge oder der Meikyo-Zweig in
`SAM_Reborn`, Perfect Balance, die Buffs oder die GCD-Folge in `MNK_Reborn`, rechnen `machinist_balance_model.py`,
`samurai_balance_model.py` und `monk_balance_model.py` weiter die alte Regel, und Konzept 14 nennt ihre Ergebnisse. *Auflösung:* jede solche Änderung trägt im selben Commit die Anpassung des Modells. *Konzept:* 14.

### Fork-Member in Dateien von Upstream · U
276 Member (`fork_placement.json`; 563 bei A742, 390 nach A746, 314 nach A749, 311 vor A825, 276 nach A825), vor allem statische Felder mit Startwert
und Einstellungen, die stehen bleiben müssen, dazu Structs, Interfaces und kleinere Dateien. *Kosten:* Ein Sync, der neben ihnen ändert, kann einen
Konflikt bringen. *Auflösung:* Ein Member zieht beim nächsten Eingriff in ihn in die Teildatei seiner Klasse
(`code.md`, „Struktur und Kosten"); neue lässt `check_fork_placement.py` nicht zu. *Konzept:* 24.

### BurstWatch führt reine Hilfsaktionen des Jobs · N
„ready but unused over the fight" nennt „Analysieren" (Beastmaster, `Gauge`) mit „(not asked)": eine Aktion des Jobs
ohne Schaden, die keine Rotation fragt; die Ausnahmen gelten nur Aktionen aller Klassen, GCDs, Abwehr und Bewegung
(A855, A864). *Kosten:* eine Angabe ohne Wirkung im Kampf in jeder Zeile des Beastmasters. *Auflösung:* ein
Merkmal aus den Blättern, das Hilfsaktionen von Fähigkeiten mit Wirkung trennt, ohne echte Abklingzeiten wie Buffs
auszuschließen; bis dahin bleibt die Vorhersage `burstwatch-no-general-actions` für den Beastmaster widerlegt (A897).
Gesucht am 20.11.2026 (A937): Im Aktionsblatt gleicht Gauge in Kategorie, Ziel, Reichweite, Abklingzeit und Kosten
Capture; kein Feld trennt sie.
*Konzept:* 03, „Bereit, aber nicht genutzt".

### Plugin-Identität ist die des Upstreams · N
`manifest.json` unverändert (`InternalName`, URLs). Ein eigener interner Name verlegt den Ordner mit Einstellungen und
gelernten Tabellen, ein eigener angezeigter Name nimmt BossModReborn die Verbindung (es erkennt RSR an „Rotation Solver
Reborn"); beides kostet im Spiel, solange nicht beide Installationen gebraucht werden (A533). *Auflösung:* wenn er beide
nebeneinander braucht: neuer interner Name mit Übernahme des Ordners, angezeigter Name bleibt. *Konzept:* 24.

## Warten auf Daten oder Fremde

- **Ninja: Stillstand, wenn die Puppe ihn fallen lässt** (seine Meldung, 20.11.2026; Ursache A956). *Im Kampf:* Die Puppe
  setzt ihr Ziel auf keines, der Kampf endet, RSR greift nicht wieder an, bis er selbst anschlägt. *Ort:* Zielwahl an
  Puppen (`ObjectHelper.IsAttackable`), nach seinem Ziel. *Auflösung:* sein nächster Puppen-Upload mit einem
  Build ab a45c8cc8e: Nach einem Fallenlassen kommt die nächste Aktion des Spielers auf die Puppe binnen eines GCD.
  *Konzept:* 03 („Welche Puppe dein Kampf ist“).

- **Rotationen aller Jobs über den Augenblick hinaus** (sein Auftrag vom 19.11.2026; A901). *Wirkung im Kampf:*
  Verstärker, Zeitgeber und DoTs laufen heute in vorhergesagte Pausen, Ressourcen bleiben beim Tod des Ziels übrig, der
  eigene Burst richtet sich nach der eigenen Uhr statt nach den Raidbuffs, die wirklich auf dir liegen. *Nächster
  Schritt:* K3 – Beobachtung gebaut (A904); nach seinem nächsten Upload aus einer Gruppe mit Raidbuffs die Spur
  „group burst:“ auswerten, dann eigene Verstärker auf den Gruppenburst warten lassen; K1 erster Schritt gebaut
  (A914: allein jede Pause im Fenster, in der Gruppe nur im ersten GCD), K2 gebaut (A916: gehaltene Ladungen vor dem
  Tod des Bosses frei), K5 erster Schritt gebaut (A917: Flächen zählen nur, wer den Treffer erlebt), danach K1 mit der
  Uhr der Gruppe aus K3, dann K6 je Job (im eigenen Fenster ohne gemessenen Verlust: alle eigenen Burstfenster voll, A933, A937) – mit den Leisten und Aufbauten am Ende eines Rudels (K5, A918) und den
  Bewegungsmitteln (K4, A919: allgemein geprüft, kein gemessener Verlust). *Konzept:* 03, „F · Über den
  Augenblick hinaus".

- **Rotation jedes Jobs: Synergien und Kaskaden innerhalb des Jobs** (sein Auftrag vom 19.11.2026: „danach die
  rotationen jedes jobs auf verbesserungspotential prüfen. innerhalb des jobs durch auswertung der eigenschaften der
  funktionen, angriffe, skills, talente und allen drumrum, auf synergieeffekte und kaskaden. im vollen kritischen loop
  mit antithese, audit und codereview mit fehlerbehebung“); seine Präzisierung desselben Tages: „ziel ist die
  aufrechterhaltung der schadensgeneriereung, aufrechterhaltung der gruppengesundheit und erhöhung des schadensoutputs bei
  bosskämpfen, raids und prüfungen. danach bei allen anderen inhalten und kämpfen“ – Maßstab je Befund: Schaden ohne
  Lücke, Gesundheit der Gruppe, mehr Schaden; Reihenfolge Boss, Raid, Prüfung vor allem anderen. Umfang (seine Präzisierung, 19.11.2026: „ich habe nicht
  gesagt, dass du nur die jobs bearbeiten sollst, die ich spiele. ich habe alle gesagt“): alle Jobs – die 21 Kampfjobs,
  Blaumagier und Beastmaster –, je Job jede Rotation; Churin und Beiruta, soweit mit den Änderungen des Forks verträglich (CLAUDE.md, seine Präzisierung). Ziel (seine Vorgabe, 19.11.2026): „ziel für alle rotationen ist es, besser als
  die von the balance zu werden.“ – gemessen an Potenz über den Kampf; wo eine Sicherheitsregel bewusst Schaden kostet,
  zählt das nicht als schlechter (CLAUDE.md, „Spielweise“). *Wirkung im Kampf:* Wo ein Merkmal, ein Status oder eine
  Aktion eine andere verstärkt oder freischaltet und die Rotation das nicht nutzt, fehlt Schaden in jedem Kampf.
  *Stand:* drei Durchgänge über alle Rotationen, die er in den Mitschnitten vom 10. bis 19.11.2026 spielte (A931 bis
  A933): Freischalter, Burst-Procs, Burstfenster und die messbaren Regeln von xivanalysis; gebaut daraus Lilie im Laufen
  (Weißmagier) und Life Surge (Dragoon); diese beiden Klassen auch in den 13 Churin- und Beiruta-Rotationen geprüft,
  ohne Befund. Die 13 Churin- und Beiruta-Rotationen sind verträglich (A936).
  Ihre 157 Einstellungstexte decken sich mit dem Code (A936). Blaumagier statisch: jeder Zauber, den die Matrix als
  ungenutzt führte, gebaut oder mit Grund ausgeschlossen (A940 bis A945). *Nächster Schritt:* Blaumagier, Beastmaster und die
  Burstfolgen der 13 am nächsten Mitschnitt, in dem sie laufen. *Konzept:* 14 (je Job), 03 „F · Über den Augenblick hinaus“.

- **Blaumagier: Apokalypsis** (A944). *Im Kampf:* Liegt Apokalypsis ohne Sterblichkeit im Set, wirkt RSR ihn nie.
  Ob der Kanal (140 über Zeit, 10 s, 120 s Abklingzeit) vier GCDs schlägt, hängt am Tickabstand, den weder Wirktext
  noch Blatt noch die gelesenen Quellen nennen. Auflösung: ein Mitschnitt, in dem er gewirkt wird (ACT Zeile 24 je
  Tick oder FFLogs-Ticks); dann Wert gegen den Füller im Loop. *Konzept:* 01, „Angriffs- und Kontrollzauber“.

- **Todeswand ohne BossMod-Modul lernen** (A934, O3 in Konzept 05, „Arenagrenzen ohne BossMod-Modul“). Sein Hinweis
  und Vorschlag vom 20.11.2026, in Arbeit (A935): „meist enteht in der anfangsphase dort ein vfx, um den rand optisch
  anzuzeigen oder die texturbeschaffenheit oder meshes sind ort anders (stecheln, zacken oder was, wasdas spielfeld
  nicht hat. kann man das erkennen? voller loop“; sein Hinweis desselben Tages: „manche arenen sind eckig, andere rund,
  einige haben sonderformen“. Geprüft in A935: Effekt und Textur sagen nicht, wo die Grenze liegt; als erster Kontakt dient
  die Spurzeile „arena edge: …“. Auflösung jetzt: diese Zeile in einem Upload, dann die Lernregel nach
  Winkelsektoren im Loop. Abgrund und
  Löcher prüft vnavmesh; eine Todeswand ohne Modul kennt RSR nicht – sie ist keine Kollision, und ihr Blatt nennt den
  inneren Radius nicht. Im Kampf: Ein Sprung von RSR könnte in sie landen. Auflösung: der erste beobachtete Treffer
  einer namenlosen Ringaktion ohne Wirkzeit auf ihn oder ein Mitglied ohne Modul (ACT-Mitschnitt); dann Lernregel je
  Gebiet im Loop.

- **Nebenprojekt: Charakterwerte, Stufen und Materia-Empfehlung** (A874, A875). Konzept 26 steht; seine Angabe
  (17.11.2026): „nebenthema zuende. wird irgendwann von mir neu angesprochen“. Auflösung: sein neuer Auftrag.


- **Maschinist: Schachmatt sperrt Doppelschach** (A868). Ist Schachmatt gewählt, aber gesperrt, kommt auch Doppelschach
  nicht – an der Puppe nach einem Jobwechsel rund 30 s ohne Ability [Schätzung: im Inhalt kaum]. Ort:
  `MCH_Reborn.AttackAbility`, Auswahl `whichToUse` ohne Rückfall. Auflösung: nächste Puppensitzung mit Maschinist nach
  einem anderen Job, Vorhersage `mch-checkmate-hides-double-check`; trifft sie zu, Konzept 03 fortschreiben und den
  Rückfall bauen. Konzept 03, „Bereit, aber nicht genutzt“.

- **Absturz einer zweiten Installation am 11.11.2026, 21:18** (A736). Das Spiel endete als Dunkelritter in der
  Azim-Steppe 0,5 s nach Kampfbeginn, ohne die Plugins zu entladen; Build b184ee7fc (08.11.2026). Das `DefenseTrace.log`
  nennt keinen Fehler. Auflösung: das Dalamud-Protokoll dieser Minuten (und ein Bericht des Absturzbehandlers, falls
  vorhanden) – zeigt es eine Ausnahme aus RSR, Loop an der Stelle; sonst Punkt schließen. *Konzept:* 19.

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
  in der kritischen Klasse, beides gewollt (A738: Bewegung). Auflösung: der erste Grund, den die Zeile „sleep not
  cast …" nennt; heißt er „too little time" kurz nach der Wahl, ist die Bedarfsrechnung ein Defekt. Konzept 08, „Ein gegnerisches Wirken
  stoppen".
