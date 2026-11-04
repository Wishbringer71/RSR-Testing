# TODO — nur offene Arbeit

Je Eintrag: was im Kampf anders wäre, wo es sitzt, der nächste Schritt oder die Auflösungsbedingung, das Konzept.
Begründungen, Messungen und die Geschichte stehen im Konzept und im Archiv (`AUDIT_LOG.md`), nicht hier. Ein
abgeschlossener Punkt verschwindet aus dieser Datei; das Konzept nennt dann den geschlossenen Stand.
Betroffene: **N** Endnutzer · **R** Autoren abgeleiteter Rotationen (Namensräume `RotationSolver.Basic`) · **U** Upstream-Pflege.

## Reihenfolge

Nach der Wirkung im Kampf: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit, in seinem Profil
(seine Vorgabe, 17.10.2026: „todo nach wichtigkeit abarbeiten. jeweils im loop bearbeiten").

1. Nach der Wiederbelebung wartet RSR die Unverwundbarkeit ab: Ein Heiler stirbt am ersten Treffer nach dem
   Aufstehen, ohne dass RSR etwas drückt.
2. Heilungen in „Heilung -" verpuffen: fünf GCDs Schaden und MP ohne Wirkung, solange der Status liegt; kein Leben,
   denn die Heilung wirkt dort ohnehin nicht.
3. Heilung bricht ab, weil ein Regen-Tick die Schwelle kreuzt: verlorene Wirkzeit, das Ziel bleibt an der Schwelle.
4. Beastmaster: One with Nature vor dem nächsten Pull auffrischen: Das Rudel wird erst Sekunden nach dem Pull betäubt
   statt mit ihm.
5. Beastmaster: Brechende Welle als Schaden: mit einem Wavekin auf dem Borgen-Horn ein Schadens-GCD, der nie kommt.
6. Fremde Repositories auf Verbesserungen für RSR prüfen: Jeder Fund kann ein Defekt im Verhalten einer Klasse sein,
   den RSR heute im Kampf zeigt; zuerst die Rotationswerkzeuge für alle Jobs, Daedalus vorn (aktiv, alle Jobs,
   eigene Kampfvorhersage).
7. Flächenbetäuber in Kreszentia für The Blackest Night: kostet MP, kein Leben.

Danach die technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist. Warten auf Daten: unten.

## Defekte

### Heilungen in „Heilung -" (HP Recovery Down) verpuffen · N
Im Kampf: Himmlischer Pfad setzte drei Mitglieder auf 10 % und legte 20 s „Heilung -" (Status 2852; Windurst: Die dritte
Etappe, 03.11.2026). RSR wirkte in dieser Zeit zwei Afflatus laetitiae, Resega, Vitaga und Assise; jede Heilung brachte
den Getroffenen ein bis zwei Prozent, gemessen etwa 98 % weniger. Erst nach Ablauf stiegen sie. Niemand starb, es kostete
fünf GCDs Schaden und MP. *Ort:* Heilzielwahl und Heilflaggen (`HealingIneffectiveStatus` kennt nur Status, die Heilung
ganz aufheben); der Wirktext nennt keine Stärke. *Nächster Schritt:* Loop: die Wirkung eigener Heilungen auf Träger eines
heilsenkenden Status messen (Effekt-Handler) und daraus je Status lernen, ob Heilen dort lohnt; Abwägung gegen ein
Mitglied in der kritischen Klasse. *Konzept:* 07.

## Offene Arbeit

### Nach der Wiederbelebung wartet RSR die Unverwundbarkeit ab · N, U
Im Kampf: Wiederbelebt bei 21 %, Zellexplosion auf ihn angekündigt; RSR drückte 5 s lang nichts, der Treffer kam
0,45 s nach Ablauf von Transcendent, er starb (M4, 03.11.2026). *Ort:* `RSCommands_Actions` (Sperre unter
Transcendent, Upstream), `AutoOffWhenDead`. *Nächster Schritt:* Loop: ob Heilung und Abwehr unter Transcendent gehen,
wenn ein angekündigter Treffer nach dessen Ende landet; ob ein Wiederbelebter wieder eingeschaltet wird. *Konzept:* 11,
„Nach der eigenen Wiederbelebung".

### Flächenbetäuber in Kreszentia für The Blackest Night · N
Im Kampf: Betäubt ein anderer Spieler mit einer Aktion seines Phantomjobs eine Gruppe, kann der Dunkelritter seine
Barriere in das Betäubungsfenster legen; sie bricht nicht, Dark Arts bleibt aus, es kostet MP, kein Leben. *Ort:*
Erkennung der Flächenbetäubung (Konzept 10, „Grenzen"), heute nur Holy und Holy III. *Nächster Schritt:* die Aktionen der
Phantomjobs mit Flächenbetäubung aus den Spielblättern erheben, Loop. *Konzept:* 10.

### Beastmaster: One with Nature vor dem nächsten Pull auffrischen · N
Im Kampf: Nach einem Kampf steht der letzte Begleiter ohne One with Nature draußen; beim nächsten Pull geht er mit dem
Scheidenden Schlag, und erst der nächste Ruf bringt ein Entfesseln – War Cry betäubt das Rudel erst Sekunden nach dem
Pull (Schluss aus dem Code, `BST_Reborn.AttackAbility`, nicht gemessen). Der Leitfaden der Gemeinschaft ruft außerhalb
des Kampfes einen anderen Begleiter und dann wieder den Büffel (Hörner außerhalb 2 s). *Ort:* `BST_Reborn.GeneralAbility`
(ruft heute nur ohne Begleiter draußen), B5. *Nächster Schritt:* Loop: Ruf außerhalb des Kampfes (1 s Wirkzeit, beim
Laufen), Reihenfolge am Pull. *Konzept:* 01, „Belegung der Hörner in der Rotation".

### Beastmaster: Brechende Welle als Schaden · N, U
Im Kampf: Mit einem Wavekin auf dem Borgen-Horn (etwa Pugil) kommt Brechende Welle nur gegen ein Ziel mit einer der
genannten Stärkungen; als Schaden (350, dazu TP) nie, weil ihre zweite Zeile hinter Schmetteraxt steht, die immer geht.
*Ort:* `BST_Reborn.GeneralGCD` (Upstream 2fe925f41). *Nächster Schritt:* Loop: Wert gegen die Kombo-Waffenfertigkeiten,
ob ein Zauber die Kombo bricht. *Konzept:* 01, „Bestienkönnen nach Borgen".

### Heilung bricht ab, weil ein Regen-Tick die Schwelle kreuzt · N, U
Im Kampf: In Toto-Rak (Weißmagier, 04.11.2026) brach RSR achtmal eine Einzelheilung mitten im Wirken ab, weil das Ziel
durch einen Tick von 8 bis 20 HP über die Schwelle stieg; die Wirkzeit war verloren, das Ziel blieb an der Schwelle.
*Ort:* `CancelCastUpdater.ShouldStopHealing` („Stop single target GCD healing after reaching threshold", Upstream,
experimentell). *Nächster Schritt:* Loop: abbrechen erst, wenn die Heilung zum größten Teil überheilte, statt beim
Kreuzen der Schwelle. *Konzept:* 07.

### Fremde Repositories auf Verbesserungen für RSR prüfen · N, R, U
Sein Auftrag (02.11.2026): „alle von dir gefundenen repos, die du benannt hast prüfen, ob daraus generell
verbesserungen für rsr entstehen könnten, für die grundstruktur, für die rotationen und das verhalten der einzelnen
klassen". Im Kampf: Was ein anderes Werkzeug für eine Mechanik, eine Lage oder einen Job gelöst hat, fehlt RSR
vielleicht; jeder Fund wird am Spiel belegt, bevor er ein Defekt oder eine Verbesserung heißt. Code nur lesen, nicht
übernehmen (Lizenzen: GPL, AGPL, LGPL, MIT, ohne Lizenz). Die Repositories, nach Zweck:
- *Rotationen für alle Jobs:* ofnature/Daedalus, PunishXIV/WrathCombo, ahwootech/BRUTAL, BeirutaNa1na/RotationSolverReborn
  (Fork von Upstream), RhyTamok/XIVdrg.
- *Beastmaster:* Kriss-Hietala/BeastmasterAssist, Rin-0617/BeastHelper, lexxxgo/Masterbeast; Daedalus und WrathCombo
  dazu ausgewertet (A488).
- *Kampfvorhersage und Unterbrechen:* FFXIV-CombatReborn/BossmodReborn, awgil/ffxiv_bossmod, sourpuh/ffxiv_interjection.
- *Auswertung von Kämpfen:* xivanalysis/xivanalysis, OverlayPlugin/cactbot, xiv-stats/xiv-damage-samples,
  Ahriqi/astbalanceimages.
- *Spiel-API und Spieldaten:* goatcorp/Dalamud, aers/FFXIVClientStructs, NightmareXIV/ECommons, NotAdam/Lumina,
  xivdev/EXDSchema, xivapi/ffxiv-datamining, SapphireServer/Sapphire (Server-Nachbau: Formeln und Abläufe, keine
  Spielquelle), LTS-FFXIV/RebornMaterial.
- *Automatisierung und Werkzeuge:* Knightmore/Henchman, Knightmore/Underlings, getsetmind/dalamudmcp,
  t3knical/DalamudPlugins.
*Ort:* Grundstruktur (`RotationSolver/Basic`, Updaters, Zielwahl, Abwehr), die Rotationen unter `RebornRotations`, die
Jobkonzepte. *Nächster Schritt:* Daedalus über alle Jobs: Zielwahl, Abwehr, Kampfvorhersage, je Job die Regeln, die
RSR nicht kennt; danach die übrigen in der Reihenfolge der Liste. Je Fund ein Loop, je Repository ein Archiveintrag mit
Commit und Datum. *Konzept:* 15, erweitert vom Vergleich mit WrathCombo auf die übrigen Werkzeuge; je Fund dazu das
tragende (01 Jobs, 08 Minderung, 13 Flächenschaden und Tankbuster).


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

### Plugin-Identität ist die des Upstreams · N
`manifest.json` unverändert (`InternalName`, URLs). *Auflösung:* seine Entscheidung, wenn beide Installationen
nebeneinander gebraucht werden.

## Warten auf Daten oder Fremde

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
