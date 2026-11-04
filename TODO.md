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
2. Allein ohne Heiler kommt der Heiltrank erst von Hand: RSR stand 13 s in der kritischen Klasse ohne Heilfähigkeit,
   der Trank kam bei 5 % von Hand, er starb (Beastmaster, Sägerschrei, 04.11.2026).
3. Beastmaster: Am Pull der Begleiter, der das Rudel aufhält, mit One with Nature: Allein gegen ein Rudel verbrennt der
   erste Begleiter Entfesseln für Schaden, und wenn die Gefahr kommt, ist nichts Schützendes übrig; er starb daran
   (Südwald, 04.11.2026).
4. Heilungen in „Heilung -" verpuffen: fünf GCDs Schaden und MP ohne Wirkung, solange der Status liegt; kein Leben,
   denn die Heilung wirkt dort ohnehin nicht.
5. Heilung bricht ab, weil ein Regen-Tick die Schwelle kreuzt: verlorene Wirkzeit, das Ziel bleibt an der Schwelle.
6. Beastmaster: Ob Analysieren die LP einrechnet: Anfreunden markiert sicher, aber früher als nötig, wenn die Stufe der
   Chance schon bei mehr LP die beste ist; jeder verlorene Pakt kostet einen weiteren Lauf (seine Vorgabe: Anfreunden
   hat oberste Priorität).
7. Beastmaster: Feindseligkeit und Gegenstände im Crucible: Ohne Zähnefletschen trägst du jeden Treffer des Bretts
   selbst, und kein Bestien-Heiltrank kommt von RSR; im ersten Brett starb niemand.
8. Beastmaster: Brechende Welle als Schaden: mit einem Wavekin auf dem Borgen-Horn ein Schadens-GCD, der nie kommt.
   Danach: Beastmaster: im Crucible nach der Schwäche der Figur rufen (Schaden, kein Leben).
9. Fremde Repositories auf Verbesserungen für RSR prüfen: Jeder Fund kann ein Defekt im Verhalten einer Klasse sein,
   den RSR heute im Kampf zeigt; zuerst die Rotationswerkzeuge für alle Jobs, Daedalus vorn (aktiv, alle Jobs,
   eigene Kampfvorhersage).
10. Flächenbetäuber in Kreszentia für The Blackest Night: kostet MP, kein Leben.
11. Ein Blaumagier in Heiler-Mimikry zählt nicht als Heiler: Mitspieler unter RSR heilen und mindern, als lebte keiner;
   kostet Schaden, kein Leben.

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

### Ein Blaumagier in Heiler-Mimikry zählt nicht als Heiler · N
Im Kampf: Ist ein Mitspieler Blaumagier mit „Aetheric Mimicry: Healer", zählt RSR für einen Tank oder Damage Dealer
keinen lebenden Heiler: Die Abwehr öffnet sich schon bei jedem Angriff („Without a living healer, spend your defence"),
und GCD-Heilungen warten nicht auf ihn. Kein Leben in Gefahr, es kostet Schaden und Abklingzeiten. *Ort:*
`DataCenter.AnyLivingHealer` liest nur die Job-Rolle; die Zielart ohne Tank und Heiler liest die Mimikry schon (A526).
*Nächster Schritt:* Loop: dieselbe Lesart für alle Leser, mit dem Spieler selbst als Blaumagier-Heiler
(`OnlyHealAsNonHealIfNoHealers`, `HoldPartyHeal`). *Konzept:* 07.

## Offene Arbeit

### Allein ohne Heiler kommt der Heiltrank erst von Hand · N, U
Im Kampf: Beastmaster allein gegen Myrmidonen-Prinzessin und Gefolge (Sägerschrei, 04.11.2026); seine LP fielen in 75 s
von 93 % auf 6 %, ab 18:28:18 meldete RSR „critical class … no heal ability", den Ultratrank drückte er bei 135 von 2.415
LP selbst, er starb 2 s später. *Ort:* `HpPotionItem.CanUse`, „Use HP potions" (Upstream, ab Werk aus, keine
dokumentierte Entscheidung) und die feste Schwelle „Use HP potions below this HP" (50 %); der Trank liest weder Gefahr
noch kritische Klasse noch einen lebenden Heiler. *Nächster Schritt:* Loop für alle Jobs: Trank nach der Gefahr
(Vorhersage, kritische Klasse, kein Heiler) statt nach der festen Schwelle, Voreinstellung nach seiner Regel
(Sicherheit vor Schaden) gegen Kosten und die 75 s, die er die Tinktur sperrt. *Konzept:* 03 („Eine Wiederverwendung
für alle Arzneien"), 07.

### Beastmaster: Feindseligkeit und Gegenstände im Crucible · N
Im Kampf: Zähnefletschen lässt den Begleiter 45 s deinen Schaden tragen; RSRs Einstellung dafür (Upstream) ist ab Werk
aus, und die Gegenstände des Bretts (Bestien-Heiltrank, -Pulver, Gegengift, Verhärtungsmittel) setzt RSR nie ein. *Ort:*
`BST_Reborn.FamiliarTankSwapAbility`, `InstanceContentCrucibleOfTheUnbroken.UseItem`. *Nächster Schritt:* Loop mit dem
Brett als Ganzem: LP von dir und den Begleitern bleiben über alle Kämpfe stehen, die Campsite heilt dich allein mehr, ein
gefallener Begleiter fehlt bis zum Horn der Rückkehr; daraus, ob die Einstellung ab Werk an gehört und wann ein
Gegenstand seinen Platz im Fach wert ist. *Konzept:* 01 („Tanken", „Gegenstände des Crucible").

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

### Beastmaster: Ob Analysieren die LP einrechnet · N
Im Kampf: Anfreunden markiert, sobald der größte bisherige Einbruch des Gegners ihn vor der nächsten Markierung töten
könnte (A2), am Zornigen Ichor etwa bei 15 % LP. Wie die Chance mit den LP fällt, sagt keine Quelle; Analysieren meldet
sie in fünf Stufen. Rechnet die Stufe die aktuellen LP ein, könnte RSR markieren, sobald sie die beste meldet: sicher
und ohne Chance zu verschenken. *Ort:* `BeastmasterFamiliars.CaptureNow`; Antwort im Chat (LogMessage 11217–11222).
*Nächster Schritt:* Loop: RSR drückt Analysieren (Talent, 3 s, keine Feindseligkeit) auf eine fehlende Bestie, liest die
Antwort aus dem Chat und schreibt Stufe und LP ins Protokoll; ändert sich die Stufe mit den LP, wird sie Teil von A2.
*Konzept:* 01, „Was gilt beim Anfreunden".

### Beastmaster: Am Pull der Begleiter, der das Rudel aufhält, mit One with Nature · N
Im Kampf, tödlich (Südwald, 04.11.2026, 13:25): Allein gegen drei Gegner, auf den Hörnern Raptor, Büffel, Fliegenfalle
(B5: Rudel, noch keine Gefahr im Gebiet). Der Raptor kam zuerst (B3, „its Trick brings least") und setzte Entfesseln für
Schaden ein, 1,5 s bevor RSR die Gefahr sah; War Cry des Büffels, das das Rudel 15 s betäubt, lag danach in der
gemeinsamen Abklingzeit, Borgen samt Saatsäher war am Kampfbeginn verbraucht, kein Begleiter heilte. Von 781 auf 57 LP
in 9 s, tot. Am Pull um 10:45:05 (04.11.2026) brachten Raptor, Büffel und Fliegenfalle je 500 auf jeden Gegner; den
Gleichstand entschied die eingestellte Rufreihenfolge („First/Second/Third Horn"), Raptor kam zuerst, War Cry betäubte
das Rudel erst nach dessen Einsatz. Unter der Belegung zwischen den Kämpfen (B5) wechselt, welcher Begleiter auf welchem
Horn sitzt; die Reihenfolge trifft dann zufällig. Dazu steht nach einem Kampf der letzte Begleiter ohne One with Nature
draußen (Schluss aus dem Code, `BST_Reborn.AttackAbility`). *Ort:* `BST_Reborn.SituationalHorn` (Gleichstand),
`GeneralAbility`, B5. *Nächster Schritt:* Loop: Gleichstand nach Spiel entscheiden (vor einem Rudel der aufhaltende
Begleiter zuerst), Ruf außerhalb des Kampfes für One with Nature; die drei Einstellungen danach nur noch für die Wahl
nach Lage aus, mit Erklärtext. *Konzept:* 01, „Belegung der Hörner in der Rotation".

### Beastmaster: im Crucible nach der Schwäche der Figur rufen · N
Im Kampf: Gegen eine Figur des Crucible mit einer Schwäche (Pisco-Dämonen-Ebenbild: Hieb; anfällig für Gewicht und
Gift) ruft RSR den Begleiter nach dem Wert seiner Dressur, nicht nach der Schadensart; der passende steht kürzer draußen.
*Ort:* `BST_Reborn.SituationalHorn` (B3). *Nächster Schritt:* Recherche, ob die Blätter Schwäche und Schadensart tragen
(XBM-Blätter, `XBMPet`); dann Loop. *Konzept:* 01, „Crucible: Schwächen der Figuren".

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

### Der erste Kampf einer Sitzung hält einen Durchlauf bis 170 ms an · N
In jedem Upload vom 01. bis 04.11.2026 und bei jedem Job: der langsamste Durchlauf des ersten Kampfes nach dem Start
20 bis 170 ms, fast ganz im Schritt „next action (rotation)", ohne Speicherbereinigung; danach unter 10 ms. *Kosten:* ein
Ruckler am ersten Pull je Sitzung. *Auflösung:* Loop, sobald er einen Ruckler am Pull bemerkt oder die erste Aktion
messbar später kommt; erster Schritt: die Teilschritte der Rotation in diesem Durchlauf messen. *Konzept:* 03, „Was ein
Frame kostet".

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
