# 05 · Aktions-Abdeckung — was die Jobs haben, was RSR davon benutzt

## Datenquelle

Die vollständige Aktionsliste liegt maschinenlesbar im Repo:
`RotationSolver.SourceGenerators/Properties/Rotation.resx` (2,05 MB am 02.10.2026,
eingecheckt) enthält für alle 23 Jobklassen jede Aktion mit Anzeigename,
Aktions-ID, Kategorie (Spell / Ability / Weaponskill / Limit Break), Jobliste
und **der Spielbeschreibung im Klartext**. Der Source-Generator emittiert
daraus zur Compile-Zeit die `abstract partial class {Job}Rotation`. Erzeugt
wird die Datei offline von `RotationSolver.GameData` aus den Spieldateien.

Damit ist der Abgleich „was hat der Job" gegen „was benutzt RSR" nicht
geschätzt, sondern gezählt.

## Methodik und ihre Fallstricke

Die Zahlen unten (Zählung vom 05.09.2026, Archiv #69) gelten unter drei Regeln, die diese Art Messung braucht. Alle drei
sind Scoping-Fragen derselben Art, und jede von ihnen verfälscht das Ergebnis in eine
andere Richtung:

| Regel | Warum | Ohne sie |
|---|---|---|
| Verwendung nur in den Rotationen der Jobordner zählen (`layout.shipped_rotation_files`, fremde dazu) | In `RotationSolver/Rotations/Core` steht die **Deklaration**, nicht die Verwendung | `IcarusPvE` gilt als benutzt, obwohl keine Rotation es ruft |
| Bewegungserkennung über den Wortstamm, nicht das Wort | Die Aktionstexte schreiben „Rush**es** to a target" | SAM Gyoten, PLD Intervene, DRG Elusive Jump fehlen |
| Dispatch-Overrides auf **beiden** Ebenen suchen | Die handgeschriebenen Basic-Partials überschreiben teils selbst | SGE erscheint als Lücke, obwohl Icarus längst verdrahtet ist |

Die dritte Regel ist zugleich ein inhaltlicher Befund, siehe „Zwei Ebenen" unten: Die
Verdrahtung liegt nicht überall auf derselben Ebene, und wer nur eine davon liest,
meldet Lücken, die keine sind.

## Zahlen

| | |
|---|---|
| Jobklassen mit generierten Aktionen | 22 (BST hat keine) |
| PvE-Aktionen gesamt | **1076** |
| davon in der Hauptrotation des Jobs verwendet | **879** |
| nur in einer ExtraRotation verwendet | 20 |
| nirgends verwendet | **177** |

## Warum „ungenutzt" meist kein Fehler ist

Die 177 zerfallen fast vollständig in Klassen, in denen Nichtverwendung
richtig ist:

| Klasse | Anzahl | Warum korrekt |
|---|---|---|
| Limit Breaks | 63 | bis A628 setzte RSR keine PvE-Limit-Breaks; seither nur ohne anderen Spieler in der Gruppe, über die Stufe, die das Spiel nennt (`LimitBreak1` bis `LimitBreak3`), nicht über die Aktion je Job (Konzept 03, „Limit Break allein“) |
| Pet-/Automaton-Aktionen | 49 | Beschreibung sagt selbst „cannot be assigned to a hotbar" — SMN-Demi-Angriffe, MCH-Queen, SCH-Faerie |
| Stance-Abbruch (`Release*`) | 4 | RSR bricht Tank-Stances nicht ab |
| Morph-Platzhalter | 3 | VPR `SerpentsTail`/`Twinfang`/`Twinblood` — „Changes to … when requirements are met"; RSR ruft die Zielaktion |
| Rest | 74 | s. u. |

### `AdjustedID` erklärt den größten Teil des Rests

`BaseAction.Use()` castet nicht die deklarierte ID, sondern
`adjustId = AdjustedID` (Eigenschaft `BaseAction.AdjustedID`, gelesen in `Use()`), und das ist
`ActionManager->GetAdjustedActionId(ID)` — die vom **Spiel** aufgelöste
Ersetzung. Ausnahme sind Aktionen auf einen Bodenpunkt: Dort wirkt `Use()` die deklarierte `ID` und bricht ab, wenn
das Spiel sie ersetzt hat (`BaseAction.Use()`, Zweig `TargetInfo.IsTargetArea`).

Daraus folgt: **die Basisaktion ist der Griff, das Upgrade löst das Spiel
auf.** `ArtOfWarPvE.CanUse()` feuert ab Stufe 82 Art of War II. Deshalb ist es
richtig und nicht lückenhaft, dass `ArtOfWarIiPvE`, `HighFireIiPvE`,
`HighBlizzardIiPvE`, `DosisIiPvE`/`DosisIiiPvE`, `PhlegmaIiPvE`/`PhlegmaIiiPvE`,
`DyskrasiaIiPvE`, `JoltIiPvE`/`JoltIiiPvE`, `GyofuPvE` und
`ShadowOfTheDestroyerPvE` nirgends vorkommen.

**Die echte Fehlerklasse liegt woanders**, und der Fork hat sie schon zweimal
getroffen: nicht die *Aktion*, sondern die **Annahme über sie** veraltet mit
dem Upgrade. Der Status heißt nach dem Upgrade anders (Thunder → High Thunder),
die Potenz ändert sich, die Combo-Bedingung kann sich
ändern. Wer nach `X` sucht, findet solche Fehler nicht; wer nach „Statusliste
neben `X.CanUse` vollständig?" sucht, schon.

Dieselbe Erkenntnis trifft die level-gestaffelten Ketten: wo SCH sechs Zweige
zwischen `RuinPvE`/`BroilPvE`/`BroilIiPvE`/`BroilIiiPvE`/`BroilIvPvE`
unterscheidet, tut das Spiel dasselbe von allein. Entbehrlich werden die Ketten dadurch nicht ohne Weiteres: Sie
unterscheiden sich vom ungegateten Aufruf, sobald `CanUse` der höheren Stufe aus einem anderen Grund als dem Level
scheitert. Für DRG ist die Vereinfachung deshalb verworfen (Zielkonzept, C1).

## Zwei Ebenen der Dispatch-Overrides

Dispatch-Overrides liegen in **zwei** Schichten, ohne erkennbare Regel:

| Ebene | Beispiele |
|---|---|
| `RotationSolver/Rotations/<Rolle>/<Job>/{Job}Rotation.cs` (handgeschriebene Hälfte) | NIN `MoveForwardAbility`, SGE `MoveForwardAbility`, WAR `MoveForwardAbility`/`MoveForwardGCD`/`EmergencyAbility`, DRK/GNB `EmergencyAbility` (Invuln) |
| `RotationSolver/Rotations/<Rolle>/<Job>/{Job}_Reborn.cs` | alle übrigen — SAM, RPR, MNK, VPR, DRG, PLD, DRK, DNC, RDM, PCT, BLM |

Konkrete Folge, nicht nur Kosmetik:

- **Invulnerability** ist bei DRK, GNB und WAR in der Basisschicht verdrahtet
  (`EmergencyAbility`, gegated auf `Service.Config.HealthForDyingTanks`), bei
  **PLD** dagegen in `PLD_Reborn.EmergencyAbility` mit eigener Logik. Welche Aktion
  die Unverwundbarkeit ist, nennt seit A255 für alle vier die Basisschicht
  (`Invulnerability`), und der Tankwechsel liest daraus, ob sie noch retten kann. Die reaktive Unverwundbarkeit hat
  zwei Gates: beim PLD `HallowedWithCover`, daneben bei allen die Schwelle `HealthForDyingTanks`. Vier Tanks, dieselbe
  Fähigkeitsklasse, zwei Orte und zwei Gates. Zusätzlich zieht sie der Tankbuster-Plan
  (`CustomRotation.TankbusterPlanAbility`, A260 und folgende), ohne die Schwelle, wenn kein Bündel eigener Abwehr einen
  gemessenen Tankbuster übersteht (Konzept 09).
- Beim Lesen einer `{Job}_Reborn.cs` ist nicht erkennbar, ob ein Slot leer ist
  oder eine Ebene tiefer belegt. Genau daran ist die Messung oben gescheitert.

## Bewegungsfähigkeiten

Vollständige Erhebung aller Aktionen, deren Spielbeschreibung eine Ortsänderung
nennt, gegen die Belegung von `MoveForwardAbility` / `MoveBackAbility` (beide
laufen laut `CustomRotation.Ability` (die Flaggenprüfung vor beiden Aufrufen) **nur** bei gesetztem
`AutoStatus.MoveForward`/`MoveBack`, können die Schadensrotation also nicht
stören; die drei direkten Aufrufer in `NIN_Reborn`, `WAR_Reborn` und `BeirutaNIN`
prüfen dieselbe Flagge, geprüft 29.09.2026):

| Job | Aktion | Richtung | Verdrahtet in |
|---|---|---|---|
| GNB | Trajectory (2 Ladungen) | vor | `GNB_Reborn.MoveForwardAbility` (644b30074) |
| WHM | Aetherial Shift | vor | `WHM_Reborn.MoveForwardAbility` (644b30074) |
| BRD | Repelling Shot | zurück | `BRD_Reborn.MoveBackAbility` (644b30074) |
| SGE | Icarus | vor | `SageRotation.MoveForwardAbility` (Basisschicht) |
| BLM · DNC · DRK · DRG · MNK · NIN · PCT · RPR · RDM · SAM · SMN · VPR · WAR · PLD | — | — | in der Rotation des Jobs |

Jede Aktion, deren Spielbeschreibung eine Ortsänderung nennt, ist damit verdrahtet.

SAM `HissatsuYatenPvE` liegt in `SAM_Reborn.MoveBackAbility` (f90c7bf7, A7). Es ist keine reine
Bewegungsfähigkeit, sondern eine Schadensaktion mit Rücksprung, die zusätzlich Enhanced Enpi gewährt. Im
Rückzugsslot fällt es nur bei gesetztem `AutoStatus.MoveBack`, also nur, wenn ein Rückzug ohnehin gewollt ist.

**Sicherheit des Landepunkts (A313, A498, A934).** Vor jeder Bewegungsaktion fragt RSR BossModReborn, ob der Weg in eine
Gefahrenzone führt (`ActionTargetInfo.CheckMovementSafety`, Schalter „Skip the check where this movement action lands (BossModReborn, vnavmesh)" je Aktion, `SkipPositionSafetyCheck`). Den
Landepunkt rechnet RSR selbst und fragt `Hints.IsDashSafe(from, to)`:
- Sprung auf ein Ziel: bis zum Rand seiner Trefferfläche; steht der Spieler schon dort (0 Yalm), nichts zu prüfen.
- fester Sprung entlang der Blickrichtung: die Weite aus dem Wirktext (`ActionSetting.BackstepDistance`) – Hell's
  Ingress und Smudge („Quickly dash 15 yalms forward"), Aetherial Shift (ebenso), En Avant („Quickly dash 10 yalms
  forward"), Hell's Egress („… 15 yalms backwards"), Elusive Jump („Executes a jump to a location 15 yalms behind
  you"), im PvP Elusive Jump, Epicycle und Hell's Ingress mit denselben Texten. Ohne bekannte Weite kein Landepunkt: mit
  BossModReborn abgelehnt, ohne es erlaubt. Seit die Prüfung im Kampf läuft (A498), hieße das für einen festen Sprung
  ohne Weite, dass er neben BossModReborn nie geht; deshalb trägt jeder feste Sprung der Basisrotationen seine Weite.
- Rücksprung vom Ziel (Displacement 15, Hissatsu: Yaten 10, Repelling Shot 10 Yalm, Wirktexte): weg vom Ziel um diese
  Weite. Repelling Shot („Jump 10 yalms away from current target") ist seit A313 ein solcher Rücksprung; als fester
  Rückwärtssprung zielte es auf den Spieler selbst, was die Aktion nicht annimmt.

Ein fester Sprung hat kein Ziel (Reichweite und Wirkradius 0 in den Blättern). Für solche Aktionen gibt die Zielwahl
sofort den Spieler zurück (`FindTarget`), und die Prüfung sitzt deshalb in diesem Pfad selbst. Bis A498 stand sie nur
hinter ihm: Kein fester Sprung wurde im Kampf geprüft, Hell's Ingress lief auf „Move Forward" in eine angekündigte
Fläche, während das Aktionsfenster „Fail" zeigte. Im Kampf jetzt: Auf einen Bewegungsbefehl bleibt der feste Sprung aus,
solange sein Landepunkt in einer Gefahrenzone liegt. Der Befehl gilt weiter, solange seine Dauer läuft („Special
state duration", ab Werk 3 s); vergeht die Fläche oder dreht sich der Spieler in der Zeit, geht der Sprung, sonst gar nicht.

**Arenagrenzen ohne BossMod-Modul** (sein Auftrag vom 19.11.2026: „wie erkennt man aber diese umrandungen bei inhalten
ohne bmr? über das gelände? über vnavmesh? konzept im vollen kritischen loop mit antithese und research“; A934).

*Sachstand.* Die Prüfung oben lief nur mit „Check movement actions with BossModReborn (Experimental)“, und die stand ab
Werk aus (Upstream 33e6acb1f, 07.05.2026); der Satz „Vor jeder Bewegungsaktion fragt RSR BossModReborn“ galt also nur
eingeschaltet. Mit ihr antwortet BossModReborn aus der Arena seines Moduls; ohne Modul gilt seine Vorgabe, ein Quadrat
von 30 Yalm Halbbreite um den Spieler (`AIHints.DefaultBounds`, 8d5ff8e21), und ohne BossModReborn ist jeder Landepunkt
sicher (`DataCenter.IsFixedDashSafe`). Im Kampf: Ohne Modul springt Elusive Jump, Hell's Egress, Displacement, Repelling
Shot oder Hissatsu: Yaten rückwärts, En Avant, Smudge oder Aetherial Shift vorwärts, ohne dass etwas den Rand kennt.
Rückstöße betrifft das nicht: Arm's Length und Surecast gehen auf jeden gelisteten Rückstoß, der den Spieler erreicht
(Konzept 13, A701).

*Grundlagen (Quellen, 19.11.2026).*
- *Wie das Spiel den Rand führt:* drei Arten. Ein Abgrund, wo der Boden endet; eine unsichtbare Wand, die nur hält;
  eine Todeswand am Rand, die beim Pull entsteht. Die Todeswand ist ein Objekt in der Mitte (BossModReborn führt es als
  „Deathwall“, 43 Dateien) und trifft mit einer namenlosen Ringaktion ohne Wirkzeit; ihr Blatt nennt den Ring als Form
  (`CastType` 10) und den äußeren Radius (`EffectRange` 30 oder 60), nicht den inneren, also nicht den Rand (Action 24246,
  24711, 41400, 49557 u. a., xivapi). Ihr Name steht nur in BossModReborns Listen, nicht im Spielblatt.
- *vnavmesh* (awgil/ffxiv_navmesh 38d2a24, 06.11.2026) baut sein Netz aus der Kollision der Zone: Löcher in Arenen
  (Materialbit 0x2000000) und unsichtbare Zonenwände sind nicht begehbar. Über IPC fragt man `Nav.IsReady` und
  `Query.Mesh.IsPointOnMesh(Punkt, halbe Höhe, auch Unerreichbares)`: ob unter einem Punkt Netz liegt. Eine Todeswand
  kennt es nicht – sie ist keine Kollision; keine Stelle im Quelltext nennt sie.
- *BossModReborn* führt jede Arena von Hand je Modul (`ArenaBounds`), die Todeswand meist als Änderung der Arena beim
  Pull; ohne Modul hat es keine.
- *Seine Mitschnitte* (20 Dateien, 10. bis 19.11.2026): keine der 20 Todeswand-Aktionen, die BossModReborn kennt, traf ihn
  oder ein Mitglied; kein Tod durch Sturz. Gemessen, wie oft die Lage eintrat: nie – sie zählt über alle Inhalte, nicht
  über seine Abende (CLAUDE.md, „Maßstab“).

*Optionen.*
- *O0 Nullvariante:* unverändert. Ohne Schalter und ohne Modul springt RSR blind.
- *O1 Schalter ab Werk an:* die bestehende Prüfung läuft immer; mit Modul kennt sie Rand und Flächen.
- *O2 Boden über vnavmesh:* unter dem Landepunkt muss Netz liegen; zuvor die Gegenprobe, dass unter dem Spieler selbst
  Netz liegt – sonst beschreibt das Netz diese Arena nicht (eine Plattform, die erst im Kampf erscheint), und die Frage
  bleibt offen.
- *O3 Todeswand lernen:* trifft eine namenlose Ringaktion ohne Wirkzeit und ohne Omen ein Mitglied, liegt der Rand
  innerhalb seines Abstands zur Quelle; Mitglieder, die sie nicht traf, liegen innen. Je Gebiet gelernt.
- *O4 Ohne Modul in einem Bosskampf kein fester Sprung.*

*Abwägung im Kampf.*
- *O1:* Stärke: Jeder Sprung eines Moduls geht nicht mehr in eine angekündigte Fläche oder über den Rand. Schwäche: Ein
  Sprung, dessen Weg eine Fläche kreuzt, bleibt aus – der Nahkämpfer erreicht das Ziel einen Moment später, ein
  Rücksprung mit Proc (Elusive Jump, Hell's Egress) fällt, solange die Fläche liegt. Seine Regel „Sicherheit vor Schaden“
  und das Text-Code-Gebot (Konzept 05 sagt „vor jeder Bewegungsaktion“) sprechen für an.
- *O2:* Stärke: ein Abgrund oder ein Loch im Boden ohne Modul und mit Modul, dessen Arena das Loch nicht kennt. Schwäche:
  braucht vnavmesh mit geladenem Netz für das Gebiet; ohne es wie bisher. Netz endet einen halben Yalm vor der Kante
  (Agentenradius): ein Sprung bis knapp an die Kante bleibt aus – der Preis ist ein Sprung, nicht ein Leben.
- *O3:* Stärke: kennt die Todeswand nach dem ersten Treffer. Schwäche: der erste Treffer, und in seinen Mitschnitten trat
  die Lage nie ein; ein Ring ohne Wirkzeit ist auch manche gewöhnliche Mechanik (Bedrohung: falsch gelernter Rand sperrt
  Sprünge in der Mitte). Bauen ohne einen einzigen beobachteten Fall hieße Mechanik ohne Anforderung (CLAUDE.md,
  „Prüfgrad“).
- *O4:* kappt die Entscheidung statt sie zu verbessern (CLAUDE.md, „Optimum“): Jeder Sprung fiele auch auf sicherem
  Boden weg.

*Antithesen.* (1) *Kein Defekt:* Er springt selten, BossModReborn hat für fast jeden Inhalt seines Profils ein Modul –
widerlegt für die Lage ohne Schalter: Dann fragt RSR auch mit Modul nicht, und der Schalter stand ab Werk aus. (2) *O2
falsch:* Das Netz kennt keine Plattformen, die eine Phase bringt – abgefangen durch die Gegenprobe am eigenen Standort.
(3) *Ausgeliefert, und nichts ändert sich:* Ohne vnavmesh oder ohne Netz für das Gebiet antwortet O2 nicht. Bei ihm
läuft vnavmesh und lädt beim Gebietswechsel sein Netz (Dalamud-Protokoll vom 16.11.2026, „Starting transition … to
'ffxiv/…'“); offen bleibt, ob es das Netz einer Instanz rechtzeitig fertig hat – bis dahin antwortet es „nicht bereit“,
und es gilt O1 allein. Eine Todeswand ohne Modul bleibt unbekannt (O3, offen).

*Entscheidung.* O1 und O2 zusammen, O3 als offene Arbeit mit Auflösungsbedingung: der erste beobachtete Treffer einer
Todeswand ohne Modul. Die Prüfung heißt jetzt „Check where movement actions land“ und steht ab Werk an; ihr
Name in der gespeicherten Konfiguration bleibt (`BmrSafetyCheckAuto`). Ein Landepunkt ohne Boden gilt als unsicher,
ein unbekannter (kein vnavmesh, kein Netz, der Spieler selbst nicht auf dem Netz) als sicher, wie ohne BossModReborn.
Geprüft werden die Landepunkte, die RSR selbst rechnet: fester Sprung, Rücksprung vom Ziel und Ort am Boden; ein Sprung
auf ein Ziel endet bei dem, der auf dem Boden steht. Kein unabhängiger Prüfer: Die Regel bewegt nicht, sie hält eine
Bewegung zurück, und ihr Fehler kostet einen Sprung.

| Anforderung | Kriterium | Stelle | Beleg | Messweg |
|---|---|---|---|---|
| Kein Sprung von RSR über einen Abgrund | Landepunkt ohne Netz → abgelehnt | `ActionTargetInfo.MovementSafety`, `DataCenter.FloorAt` | vnavmesh `IsPointOnMesh` | Spurzeile „… lands where vnavmesh has no floor“ |
| Kein falscher Abbruch auf unbekanntem Boden | Spieler nicht auf dem Netz → keine Antwort | `DataCenter.FloorAt` | Gegenprobe | keine Ablehnung aus diesem Grund, wo er steht |
| Prüfung ab Werk | Schalter an | `Configs.CombatAndTarget` | seine Regel „Sicherheit vor Schaden“ | Einstellungsfenster |

**Welche Aktion bewegt, sagen die Blätter** (`Action.AffectsPosition`). `check_movement_types.py` hält jede
Aktion des PvE mit diesem Merkmal gegen einen Bewegungstyp in ihrer Basisrotation: jede Jobaktion, der Blaumagier
eingeschlossen, und jede Inhaltsaktion, die eine Rotation wirkt (Phantomjobs in Kreszentia, Bozja, Quest-Gefechte); eine
ohne Typ fällt in der CI. Upstream setzte den Typ in 7.5.7.14 für Dragonfire Dive, Stardiver und Forked Raiju, die ihn im
Fork schon trugen (A174); En Avant, Smudge, Aetherial Shift und Elusive Jump hatten keinen und gingen auf den
Bewegungsbefehl ungeprüft.

Den Blaumagier und alle Inhaltsaktionen nahm die Prüfung bis A519 aus, weil beide als außerhalb seines Profils galten.
Ungeprüft blieben deshalb:
- *Gewissenstritt (J Kick) und Rasender Sturz (Flying Frenzy)*, Blaumagier, „Delivers a jumping physical attack to
  target". Die Rotation wirkt Gewissenstritt im Mondflöten-Fenster und Rasenden Sturz als Flächenfüller, aus jedem
  Abstand in Reichweite (25 und 20 Yalm), ohne Prüfung des Wegs.
- *Treten (Phantom Kick)*, Phantommönch, „Leap towards your target". Ab Werk aus. Eingeschaltet galt nur die eigene
  Grenze der Phantom-Rotation (ab Werk 5 Yalm), nicht „Only use damaging gap closers within this distance".
- *Kreszenter Schritt (Occult Featherfoot)*, Phantomwaldläufer, „Quickly dash 15 yalms forward". Lief auf „Move
  Forward" in eine angekündigte Fläche wie vor A498 En Avant.

Jetzt gilt:
- Die drei Sprünge auf das Ziel sind `HostileMovingAttack` und tragen die Grenze „Only use damaging gap closers within
  this distance" (`GapCloserAllowed` in der Basisrotation, wie Intervene und die Ansturme des Beastmasters, A358). Die
  Grenze der Phantom-Rotation bleibt als zweite Obergrenze.
- Kreszenter Schritt ist ein fester Sprung mit 15 Yalm aus dem Wirktext.
- Im Kampf: Gewissenstritt und Rasender Sturz gehen erst, wenn der Blaumagier nah genug am Ziel steht (ab Werk unter
  3 Yalm), auf einen Bewegungsbefehl, im Countdown oder wenn er sie selbst drückt; sonst bleiben sie bereit, und im
  Mondflöten-Fenster fehlt dann Gewissenstritt. Die Prüfung des Wegs läuft mit „Check where movement actions land" (seit
  A934 ab Werk an); die Abstandsgrenze gilt immer.

Ausnahmen, je mit Grund im Skript:
- Rescue: bewegt das Gruppenmitglied, nicht den Spieler.
- Steadfast Stance (Phantomtänzer): RSR wirkt es nur auf den Spieler. Den Ansturm gibt es nur auf ein Gruppenmitglied.
- Night Stallion (Quest-Gefecht): Die Weite des Rutschens nennt kein Text, und ohne Weite lehnte die Prüfung neben
  BossModReborn ab.
- Lost Seraph Strike: Bozja liegt außerhalb seines Profils und kommt in keinem seiner Mitschnitte vom 25.10. bis
  03.11.2026 vor. Erfasst, nicht bearbeitet.

**Bewegung zu einem Ort am Boden** (`SpecialActionType.GroundTargetMove`, A520): Böse Ahnung (Loom, Blaumagier),
Shukuchi (Ninja) und Cloud Skim (Beastmaster, alle fünf Fassungen) bewegen den Spieler zu einem Ort, den die Aktion setzt
(„Move quickly to the specified location", „… in the selected direction").
- *Bis A520:* ohne Bewegungstyp und ohne Bewegungszielwahl. Böse Ahnung galt als Bodenheilung (`IsFriendly`) und landete
  auf „Move Forward" an einem gespeicherten Punkt, an der Arenamitte, unter dem Boss oder bei einem Gruppenmitglied.
  Shukuchi, das die Ninja-Basisschicht auf „Move Forward" wirkt (`MoveForwardAbility`), lief in die Platzierung der
  Flächenangriffe, also auf Gegner. Beide ohne Prüfung des Wegs. Das Skript führte Shukuchi als „von keiner
  Fork-Rotation gewirkt"; das stimmte nicht.
- *Jetzt:* Die Zielwahl für Bewegung (`TargetType.Move`, `FindTargetAreaMove`) setzt den Ort. Mit „Move as far as
  possible with ground-targeted movement actions" (ab Werk aus) liegt er in voller Reichweite in Blickrichtung oder zur
  Bildschirmmitte („Aim movement actions at the screen center"). Sonst liegt er beim entferntesten Mitspieler oder Gegner
  im Kegel vor ihm („Movement target angle"), in Reichweite der Aktion. Der Weg dorthin wird geprüft
  (`MovementSafety`, wie beim Objekt-Sprung). Drückt er die Aktion selbst, fängt RSR sie nicht ab
  (`ActionQueueManager`): Den Ort, den er setzt, ersetzte sonst die Zielwahl.
- *Die Grenze im Kegel:* Die Zielwahl für Bewegung las „Only use damaging gap closers within this distance" (ab Werk
  3 Yalm). Ihr Text nimmt den Bewegungsbefehl aus („Not limited: a move-forward command"), und diese Zielwahl läuft nur
  auf ihn: für die Bodenziele, für die Sprünge mit Schaden (`HostileMovingAttack`) und für Freundessprünge ohne
  Fokus. Upstream setzte die Grenze dort im Oktober 2026 (6b52e1dfd); erreicht wurde sie bis A520 nur von diesen
  Sprüngen. Der Text bindet: Jetzt begrenzt dort nur die Reichweite der Aktion. Im Kampf: Auf „Move Forward" springen
  Intervene, Gewissenstritt oder Icarus ohne Fokus jetzt auf den entferntesten Kandidaten im Kegel, nicht nur auf einen
  bis 3 Yalm. Vorstöße auf Gegner (`HostileMovingForward`) nehmen auf den Befehl wie bisher dein Ziel.

**Nach einem Sprung zum Ziel kein Fernangriff** (`MeleeRangedAfterMove`, Upstream). Die Fernangriffe mit diesem Typ –
Piercing Talon, Throwing Dagger, Lightning Shot, Writhing Snap, Enpi, Shield Lob, Tomahawk, Harpe; beim Dunkelritter
ist Unmend nicht so markiert – bleiben einen Zug aus, wenn die letzte Aktion den Spieler zum Ziel
oder nach vorn bewegt hat – dort ist er jetzt im Nahkampf, und die Entfernung von vorher wählte sonst den schwächeren
Fernangriff. Welche Aktion das ist, sagt ihr Bewegungstyp (`IActionHelper.LastActionMovedForward`). Die Liste davor war
von Hand geführt: Sie nannte Rough Divide, das in 7.0 Trajectory wich, und kannte Trajectory, Shadowstride, Primal Rend,
Stardiver, Forked Raiju, Winged Glide und Slither nicht. Ein Rücksprung zählt nicht: Nach Elusive Jump ist Piercing Talon
gerade die Folge, die der Sprung freischaltet.

BossModReborns eigener Endpunkt `Hints.IsFixedDashSafe` nimmt Weite und Richtung aus der Blickrichtung, nicht zwei Punkte.
Abonniert als `(Vector3, Vector3)` schlug jeder Aufruf fehl, und der sichere Wrapper lieferte „unsicher": Solange
BossModReborn lief, lehnte RSR jeden festen Sprung und jeden Rücksprung ab (seit `33e6acb1f`, Upstream, 07.05.2026).

**Eine Prüfung, zwei Leser** (A465, für feste Sprünge erst seit A498 wahr). Das Aktionsfenster („BMR Safetycheck")
zeigt das Ergebnis und den Grund der Prüfung, die im Kampf entscheidet (`ActionTargetInfo.MovementSafety`), ohne eine
Ablehnung zu buchen.
`CheckMovementSafety` bucht sie für das Protokoll. Vorher rechnete das Fenster die Prüfung selbst nach und kannte die
Regel „am Ziel ist ein Sprung nur Schaden" nicht: Es zeigte „Fail" für einen Gapcloser, den RSR im Kampf drückte.

## Verbleibender Rest — offene Kandidaten

Nach Abzug von Limit Breaks, Pet-Aktionen, Stance-Abbrüchen,
Morph-Platzhaltern und Upgrade-Griffen bleiben Aktionen, für die keiner dieser
Gründe greift. Ihre Einzelprüfung steht im Archiv (#69, abgearbeitet); die heutige
Einordnung je Job, erzeugt statt gezählt, steht in `14-action-dependency-matrix.md`
und `docs/action-matrix/`. Keine davon ist ungeprüft nachgerüstet — jede braucht
eine Rotationsentscheidung, keine Codeentscheidung.
