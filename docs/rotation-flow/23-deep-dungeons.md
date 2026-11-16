# 23 · Tiefe Gewölbe

**Leitfrage:** Was tut RSR in den Tiefen Gewölben, das es nur dort gibt – Verwandlung, Pomander, Magicite, Demiklone
und Weihrauch?

Gilt für jeden Job. Die Regenerationstränke dieser Inhalte stehen bei den Tränken in `03-universal.md`
(„Regenerationstränke der Sonderinhalte"). Ordnung der Konzepte: `20-concept-order.md`.

## Tiefe Gewölbe: Verwandlung

*Was gilt (A614):* Verwandelt ein Gegenstand des Gewölbes dich, drückt RSR die Angriffe der Form, die das Spiel gerade
zulässt – für jeden Job gleich, in allen 52 Gebieten der Tiefen Gewölbe (Blatt `TerritoryType`, Nutzungsart 31, xivapi
06.11.2026; Pflichtrotation „Deep Dungeon", `DeepDungeonRotation`). Welche Form du hast, fragt RSR nicht ab: Es probiert
die bekannten Formaktionen, und das Spiel lässt nur die der laufenden Form zu (`ActionManagerStatusValid`). Die
Pomander stehen unten („Tiefe Gewölbe: Pomander").
- Die Formen und ihre Aktionen: Manticore Pummel, Succubus Void Fire II, Kuribu Heavenly Judge (Blatt `Transformation`,
  Zeilen 42, 43, 49); Dreadnaught Wrecking Ball und Rotosmash, Bomb Big Burst (BossMod `DeepDungeon.cs`, Werkzeugquelle;
  das Blatt führt für diese Formen keine Aktion). Reichweite, Wirkzeit, Fläche und Bodenziel kommen aus dem Aktionsblatt
  wie bei jeder Aktion.
- Beim Dreadnaught geht Wrecking Ball (Fläche, 2,5 s) ab der Gegnerzahl für Flächen, sonst Rotosmash. Die Formen mit
  nur einer Fläche (Succubus, Kuribu, Bomb) greifen auch einen einzelnen Gegner damit an.
- *Rocky Roll* (Mudball, ein Sprint 25 y nach vorn, Schaden auf dem Weg) geht nur auf den Befehl „Move forward"
  (`MoveForwardGCD`), wie jeder schadende Sprint aus der Ferne: Selbsttätig holte er dich für Schaden aus deiner
  Stellung, im Gewölbe womöglich auf eine Falle (seine Spielweise: „Nichts empfehlen, was ihn für Schaden aus einer
  sicheren Position holt"). Er trägt den Bewegungstyp eines festen Sprints wie En Avant, die Weite aus dem
  Aktionsblatt; mit „Check movement actions with BossModReborn (Experimental)" wird sein Landepunkt geprüft.
- *Spur:* Mit jeder Verwandlung eine Zeile ins `DefenseTrace.log` – welche Formaktionen das Spiel zulässt –, damit ein
  Upload zeigt, ob die Kette greift.

*Im Kampf:* Im Palast der Toten als Manticore schlägt RSR Pummel auf das Ziel, statt bis zum Ende der Verwandlung
nichts zu tun; in Eureka Orthos als Dreadnaught gegen drei Gegner Wrecking Ball.

*Begründung:* Verwandelt sind die Aktionen des Jobs gesperrt. RSR prüfte sie weiter, keine ging, und die Formaktionen
kannte es nur als Pflichtaktionen, die auf der Pflichtleiste stehen müssen (`IsOnSlot`) – dort stehen sie nie. Das
Muster, die Leiste nicht zu prüfen und dem Spiel zu folgen, nutzt RSR schon für die Sonderaktionen in Orbonne.

*Verworfen:* (a) Die Form am Status ablesen und je Form eine Liste führen: Für drei Formen nennt das Blatt keine Aktion,
und das Spiel weiß ohnehin, welche gilt. (b) Rocky Roll selbsttätig auf das Ziel: Bewegung für Schaden (oben).

*Gegenthese 1 – Eine Formfläche zieht Gegner, die noch nicht kämpfen:* wie jede Fläche; in den Gewölben entscheidet
„Deep dungeons: damage doesn't pull new enemies" (ab Werk an, „Tiefe Gewölbe: Gegnerkontrolle").
*Gegenthese 2 (ausgeliefert, und nichts ändert sich):* „Use duty actions" ist aus, oder das Spiel meldet die Aktion trotz
Verwandlung nicht frei. Ab Werk ist es an; ob das Spiel sie frei meldet, ist nicht beobachtet – die Spur zeigt es.
*Gegenthese 3 – Andere Pflichtrotationen für dieselben Gebiete:* keine (Stand 06.11.2026).

## Tiefe Gewölbe: Pomander

*Was gilt (A625):* Allein in einem Tiefen Gewölbe setzt RSR die Pomander, die vor einem Treffer schützen oder einen Tod
abwenden, für jeden Job gleich – Option „Use the deep dungeon's pomanders against bosses and in danger when alone", ab
Werk an.
- **Gegen einen Boss** (ein Gegner mit Boss-Symbol, `ObjectHelper.IsBossFromIcon`; die Bosse von Pilgers Pfad tragen
  Rang 2, BNpcBase 17933, 18546, 18666): Steel („Decreases damage received by 40%"), Strength („Increases damage dealt
  and HP recovered via healing magic by 30%") und Haste („swiftly execute weaponskills, spells, and auto-attacks"), jeder,
  solange er im Bestand ist und nicht schon wirkt. Vor dem Pull, sobald der Boss in Sicht steht (in der Gegnerliste: bis
  48 y, in Sicht), sonst im Kampf, solange er kämpft (`DataCenter.BossInCombat`, dieselbe Erkennung wie für die
  Gegenstände im Crucible). Vor dem Pull, weil jeder Einsatz die Aktionen sperrt (BossMod setzt nach `UsePomander` 2,1 s,
  Werkzeugquelle): drei Pomander im Kampf kosteten zu Beginn des Bosskampfs mehrere GCDs, vor dem Pull keinen. Steel und
  Strength wirken 8 min (consolegameswiki, Pilgrim's Traverse, 06.11.2026) und decken einen Bosskampf samt Anlauf; mehr
  als drei je Sorte hält niemand, ein vierter verfällt.
- **Vor dem eigenen Tod** (im Kampf in der kritischen Klasse, `ObjectHelper.IsInCriticalClass`): Raising („Instantly
  raises the first party member to be KO'd"), solange er nicht schon wirkt – er greift erst beim K.O., und dann ist es zu
  spät, ihn zu setzen; das Wiki rät genau dazu („Useful if player is about to die during a solo run"). Danach Witching
  („Transforms all targets within range…", Radius der Aktion des Pomanders aus dem Action-Blatt, 6269: 20 y nach der
  Seite), sobald ein Gegner, der kein Boss ist, dich in diesem Radius angreift: Er wird 30 s zu Frosch, Imp oder Huhn und
  trifft mit höchstens 60 % (Werkzeugquelle consolegameswiki). Ein zweiter Witching erst, nachdem du die kritische Klasse
  verlassen hast: Die verwandelten Gegner zielen weiter auf dich, und ohne diese Sperre folgte der nächste, sobald die
  Sperre des ersten endet (A626).
- **Erkannt am englischen Wirktext** des Blatts `DeepDungeonItem` (wie die Status), nicht an Zeilen: Pomander und
  Protomander derselben Wirkung zählen gleich. Welcher Gegenstand auf welchem der 16 Plätze liegt, sagt das Blatt
  `DeepDungeon` (Spalte PomanderSlot, Zeile = Gewölbe des Direktors, `DeepDungeonId`), wie BossMod es liest; jedes der
  vier Gewölbe führt jede der fünf Wirkungen auf genau einem Platz (Blätter, 06.11.2026), ein zweiter Einsatz derselben
  Wirkung kann also nicht folgen. Anzahl und Bit „aktiv" liest RSR aus dem Gewölbe selbst (`InstanceContentDeepDungeon`,
  `Items`), eingesetzt wird über `UsePomander`, einmal je Bild, solange RSR läuft, ohne Animationssperre und ohne Wirken.
- **Ein Einsatz gilt**, bis die Anzahl fällt oder der Pomander wirkt, höchstens einen GCD. Nimmt das Spiel ihn nicht,
  versucht RSR diesen Platz bis zum Ende des Kampfs oder bis zum nächsten Stockwerk nicht wieder; die Spurzeile dazu nennt
  das Bit „usable" des Spiels, dessen Bedeutung nur an seinem Namen in ClientStructs belegt ist und deshalb keinen Einsatz
  sperrt. Eine Spurzeile je Einsatz mit Grund.
- **Nie in der Gruppe:** Der Bestand ist geteilt (consolegameswiki); was ein Mitspieler damit vorhat, ist eine Annahme.
  Die Gruppenliste des Gewölbes (`Party`, Plätze ohne Objekt-Id zählen nicht) und die Gruppenliste von RSR müssen beide
  „allein" sagen.

*Im Kampf:* Allein in Pilgers Pfad stirbt man auf Stein 31 und tiefer ohne aktiven Raising, und der Lauf ist verloren
(consolegameswiki, „Party KOs"). Bisher setzte RSR keinen Pomander: Steel und Raising lagen im Bestand, während der Boss
traf und der Spieler fiel. Jetzt betritt er die Arena des Bosses auf Stockwerk 10, RSR setzt Steel, Strength und Haste,
bevor er angreift, und der erste Treffer des Bosses kommt um 40 % gemindert; fällt er unter die Schwelle der kritischen
Klasse, steht Raising, bevor der nächste Treffer ihn tötet. BossModReborns Gewölbe-Automatik setzt Pomander nur gegen
Überlauf (drei gleiche, eine goldene Truhe mit einem vierten; 27be422, 05.11.2026), Magicite gar nicht – die beiden setzen
also nichts doppelt; ruft RSR `UsePomander`, reiht BossMod es in seine eigene Warteschlange und führt es aus.

*Nicht gebaut, mit Grund:*
- Weihrauch, Magicite und Demiklone: eigener Abschnitt („Tiefe Gewölbe: Magicite, Demiklone und Weihrauch").
- Pomander, die das ganze Stockwerk treffen (Petrification, Storms, Lethargy, Frailty), Purity und Purification und die
  Karten- und Beutepomander: Entscheidungen über den Weg durch das Stockwerk, nicht über einen Moment im Kampf.
- Einen Steel für den letzten Boss eines Satzes aufheben: Wo der Satz endet, liest RSR nicht; gegen jeden Boss eingesetzt
  füllt die goldene Truhe nach, und über drei hinaus verfällt er ohnehin.
- Raising schon vor dem Bosskampf statt in der Gefahr: Ob seine Wirkung über das Stockwerk hält, ist unbelegt, und ab
  Stein 81 erscheint er nicht mehr – ein früh gesetzter, der mit dem Stockwerk verfällt, fehlt später.

*Gegenthese 1 – Der Bestand fehlt später:* Steel gegen einen leichten Boss fehlt gegen einen schweren. Preis benannt; gegen
das Aufheben spricht der Überlauf bei drei.
*Gegenthese 2 – Die Sperre in der Gefahr:* Raising im Kampf sperrt die Aktionen für die Dauer seines Einsatzes, in der
keine Abwehr fällt. Preis benannt; dagegen steht der Verlust des ganzen Laufs, und Raising wirkt beim K.O., gleich ob die
Abwehr noch kam. Eine flackernde kritische Klasse setzt ihn höchstens früher, er wirkt bis zum K.O.; Witching nur mit
einem Angreifer in Reichweite.
*Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Er spielt die Gewölbe nur in der Gruppe, die Option ist aus, oder
RSR ist vor dem Pull aus (Auto-Start erst im Kampf); dann bleibt es beim Einsatz im Kampf oder wie bisher. Die Spurzeile
zeigt jeden Einsatz.

## Tiefe Gewölbe: Magicite, Demiklone und Weihrauch

*Was gilt (A626):* Allein in einem Tiefen Gewölbe setzt RSR die Steine des Gewölbes – drei Plätze
(`InstanceContentDeepDungeon.Magicite`), Einsatz über `UseStone` – für jeden Job gleich, nach den Pomandern (die
gehen im selben Bild vor). Option „Use the deep dungeon's magicite, demiclones and incense against bosses and in danger
when alone", ab Werk an.
- **Rettung vor dem eigenen Tod** (im Kampf in der kritischen Klasse): ein Stein, der alle Gegner des Stockwerks außer
  Bossen tötet und der Gruppe 10 s Unverwundbarkeit gibt – Inferno-, Crag- oder Vortex-Magicite (Himmelssäule; gegen
  einen Boss dazu schwerer Schaden), ohne Boss auch Poisonfruit-Weihrauch (Pilgers Pfad). Auf einem Bossstockwerk macht
  Poisonfruit nur Schaden am Boss und rettet nicht; dort bleibt er liegen.
- **Gegen einen Boss im Kampf:** Elder-Magicite (tötet den Boss; „may not work if the boss is not aggroed first", deshalb
  erst im Kampf), sonst ein Demiklon (Eureka Orthos): Unei zuerst (heilt, schildet 20 % der maximalen LP), dann Onion
  Knight (Schaden und Heilung), dann Doga (Schaden, Versteinerung in der Fläche).
- **Gegen einen Boss vor dem Pull**, sobald er in Sicht steht, sonst im Kampf: Barkbalm-Weihrauch (für das Stockwerk
  +100 % maximale LP der Gruppe; eine Fee, die Schaden macht und unter 70 % LP heilt; „Particularly useful for bosses").
- **Nie:** Mazeroot-Weihrauch – Karte, Fallen, Durchgang: eine Entscheidung über den Weg, nicht über den Kampf.
- **Wirkungen nur im Wiki:** Die Blätter `DeepDungeonMagicStone` (Name, Tooltip) und `DeepDungeonDemiclone` (TitleCase,
  Description) führen Erzählung, keine Wirkung; die Wirkungen stehen in consolegameswiki (Heaven-on-High, Eureka Orthos,
  Pilgrim's Traverse, 06.11.2026; Werkzeugquelle). RSR erkennt einen Stein deshalb an seinem englischen Namen im Blatt,
  nicht an einem Wirktext.
- **Welcher Stein auf welchem Platz (A737):** Die Struktur hat drei Plätze (`InstanceContentDeepDungeon.Magicite`,
  FFXIVClientStructs 80cb4a7). Der Wert eines Platzes nennt die Sorte, gezählt ab 1 in der Liste `MagiciteSlot` des
  Gewölbes im Blatt `DeepDungeon`; 0 heißt leer. Wird ein Stein benutzt, rücken die übrigen nach vorn. Die Liste nennt Zeilen
  eines Steinblatts, und welches, sagt `DeepDungeonType` (EXDSchema): 1 `DeepDungeonMagicStone` (Himmelssäule, Liste
  1, 2, 3, 4: Inferno, Crag, Vortex, Elder), 2 `DeepDungeonDemiclone` (Eureka Orthos, Liste 1, 2, 3: Unei, Doga, Onion
  Knight). Für Pilgers Pfad (Typ 3, Liste 5, 4, 5) nennt EXDSchema kein Blatt, und die 5 steht in beiden. Gemessen in
  seinem Lauf vom 12.11.2026 (Stockwerke 71 bis 99, in der Gruppe): Wert 1 ist Barkbalm-Weihrauch (Demiklon-Blatt 5) – bei
  jedem der drei Einsätze, nach denen Titania erschien, verschwand eine 1 aus der Liste –, Wert 2 Mazeroot (Demiklon-Blatt
  4), Wert 3 Poisonfruit (Magicite-Blatt 5). Alle 29 Ablesungen gehen in dieser Lesart auf: Jeder Wechsel ist ein
  benutzter Stein und das Aufrücken, dazu neue Funde.
  - *Warum nicht mehr wie bisher:* A626 las den Wert als Zeilennummer im Blatt. In Himmelssäule und Orthos stimmen Wert und
    Zeile überein, weil die Liste dort mit 1, 2, 3 beginnt; in Pilgers Pfad trug keine Ablesung („nothing" auf allen
    Plätzen), und keine Regel konnte einen Weihrauch setzen. Den Statusagenten (`AgentDeepDungeonStatus`), den A626 zuerst
    las, zeigte derselbe Lauf mit sinnlosen Einträgen; RSR liest ihn nicht mehr.
- **Spur:** Je Stockwerk eine Zeile im `DefenseTrace.log` mit den drei Werten und der Sorte je Platz, dazu eine Zeile je
  Einsatz und je Abweisung.
- **Einsatz wie bei den Pomandern:** einmal je Bild, solange RSR läuft, ohne Animationssperre und ohne Wirken; gilt, bis
  der Platz einen anderen Wert zeigt, höchstens einen GCD; ein abgewiesener Platz erst nach dem Kampf oder auf dem
  nächsten Stockwerk wieder. Je Anlass einer: eine Rettung erst wieder, nachdem du die kritische Klasse verlassen hast
  (die Gegner fallen, bevor die Unverwundbarkeit sichtbar ist, und ohne Sperre folgte der zweite Stein); Elder und ein
  Demiklon einmal je Kampf; Barkbalm einmal je Stockwerk, er gilt für das ganze. Nie in der Gruppe: Der Bestand ist geteilt (Wiki, Heaven-on-High: „kept in a shared
  inventory"), dieselbe Prüfung wie bei den Pomandern.

*Im Kampf:* Allein in der Himmelssäule auf Stockwerk 23 fällt der Spieler unter die Schwelle der kritischen Klasse, drei
Gegner greifen an; RSR setzt Inferno-Magicite, alle Gegner des Stockwerks fallen, und er steht 10 s unverwundbar. In Pilgers
Pfad betritt er die Arena auf Stockwerk 30, RSR brennt Barkbalm, bevor er angreift, und er geht mit doppelten LP in den
Bosskampf. In Eureka Orthos kommt Unei zu Beginn des Bosskampfs und heilt mit.

*Nicht gebaut, mit Grund:*
- Poisonfruit oder ein Primal-Magicite gegen den Boss ohne Gefahr: Schaden am Boss gegen die Rettung auf einem späteren
  Stockwerk – die Sicherheit geht vor, ein Tod beendet den Lauf allein.
- Ein Demiklon vor dem Pull: Wie lange er bleibt, ist unbelegt.
- Halten für den letzten Boss eines Satzes: Wo der Satz endet, liest RSR nicht.

*Gegenthese 1 – Der Stein fehlt später:* Inferno gegen einen Notfall auf Stockwerk 12 fehlt gegen den Boss auf 20. Preis
benannt; der Notfall beendet sonst den Lauf, der Boss nur vielleicht.
*Gegenthese 2 – Der Tod während der Beschwörung:* „if the user dies during Feo Ul's animation, the attack will not go off,
but the item will still be consumed" (Poisonfruit). Preis benannt; Raising geht im selben Fall vorher (Pomander zuerst).
*Gegenthese 3 (ausgeliefert, und nichts ändert sich):* In der Himmelssäule galt bis A807 kein Gebiet als Tiefes
Gewölbe: Ihr Gebietsblatt nennt keine `ContentFinderCondition`, und `IsInDeepDungeons` las die Art 0 – Pomander und
Magicite ruhten dort. Seit A807 gilt die `ContentFinderCondition`, die das Gebiet nennt (Konzept 03, „Inhalt eines
Gebiets"). Die Zuordnung der Werte zu den Sorten ist für Pilgers Pfad gemessen (A737), für Himmelssäule und Eureka
Orthos aus der Liste `MagiciteSlot` abgeleitet; stimmt sie dort nicht, nennt die Spurzeile je Stockwerk eine falsche
Sorte, und ein Upload zeigt den Widerspruch.

## Tiefe Gewölbe: Gegnerkontrolle – kein unnötiger Pull, Zusatzgegner schlafen

**Seine Vorgabe (15.11.2026):** „Spezielle Option für Deep Dungeons erstellen, so dass regulär weiter mit aoes gepullt
werden kann und auch geheilt, aber in Deep Dungeons zwei Optionen bestehen, einmal für heilung (mit dem damit
verbundenen Schaden) und einmal für Schaden (z.b. beim pullen, aber auch bei laufenden Kämpfen um adds zu vermeiden)."
Dazu sein Vorschlag: „Vielleicht ist in Deep Dungeons auch crowd Control von geaggroedent mobs durch sleep sinnvoll."
Sein Hinweis (16.11.2026): „Man kann Schlaf auch verlängern, hält nur kürzer. Am besten bevor er ausläuft." Und
(16.11.2026): „die gegner, welche irrtümlich mit schlafen, werden durch den nächsten angriff durch gruppenmitglieder eh
geweckt". Seine Präzisierung (16.11.2026): „der schlaf ist wie gesagt dafür da, dass angreifende gegner besser
crowdcontrolled werden. der tank sucht sich dann das nächste schlafende ziel, wenn alle anderen angreifer tot sind."
Vorher (A833): „Gerade in Deep Dungeons ist aggrokontrolle und gegnermanagement überlebenswichtig", und für Dungeons
und FATEs „gut, wenn alle Gegner in der Nähe schnell zusammen kommen".

*Anforderungen (A834; seine Worte → prüfbares Kriterium → Stelle → Messweg):*
| # | Seine Angabe | Kriterium | Stelle | Messweg |
|---|---|---|---|---|
| G1 | „regulär weiter mit aoes gepullt werden kann und auch geheilt" | außerhalb der Tiefen Gewölbe ändert keine der neuen Optionen etwas; „Don't pull new enemies with AoE" bleibt dort der Schalter (ab Werk aus); in den Gewölben entscheiden deren Optionen (seit A837) | `DataCenter.PullsIfHit` | Spur: keine Zeile außerhalb der Gewölbe |
| G2 | „einmal für heilung (mit dem damit verbundenen Schaden)" | in den Gewölben trifft eine Aktion, die die Gruppe heilt und dabei Gegner trifft (Assise, Makrokosmos, Pneuma), keinen Gegner, der noch nicht kämpft – außer ein Gruppenmitglied ist in der kritischen Klasse | Option „heals that also deal damage", `ActionTooltip.HealsToo` | Spur „deep dungeon: … waits" |
| G3 | „einmal für Schaden (z.b. beim pullen, aber auch bei laufenden Kämpfen um adds zu vermeiden)" | in den Gewölben zieht keine Fläche RSRs einen Gegner, der noch nicht kämpft – Schaden wie Schlaf; im Kampf kein neues Ziel, das noch nicht kämpft; das Ziel des Pulls selbst bleibt | Option „damage", `PullsIfHit`, `ObjectHelper.IsAttackable` | Spur „deep dungeon: … not a new target" |
| G4 | „crowd Control von geaggroedent mobs durch sleep" (Vorschlag) | greifen in den Gewölben mehrere Gegner die Gruppe an (ihr Ziel ist ein Gruppenmitglied, allein der Spieler), legt ein Heiler oder Magier die schlafen, die nicht bekämpft werden – bekämpft ist das Ziel des Spielers und jedes Ziel eines Tanks der Gruppe (Präzisierung 16.11.2026); RSRs Flächen wecken sie nicht, und angegriffen werden sie erst, wenn kein wacher mehr steht | Option „sleep", `DeepDungeonCrowd` | Spur „deep dungeon: sleep …" |
| G5 | „Man kann Schlaf auch verlängern, hält nur kürzer. Am besten bevor er ausläuft." (Hinweis) | ein Schläfer, dessen Schlaf vor dem nächsten möglichen Schlaf ausliefe, wird erneut schlafen gelegt, solange ein wacher Gegner bekämpft wird und die Art nicht abgenutzt ist; nicht früher, weil die Verlängerung kürzer hält | Option „sleep", `SleepFurtherAttacker` | Spur „deep dungeon: sleep renewed …" |
| G6 | „dran denken, dass einige schlaf spells aoe sind. da sind dann sofort mehrere betäubt. also zielwahl eher mittig aller attackierenden gegner" (Hinweis und Vorschlag, 16.11.2026; „betäubt" meint hier eingeschläfert) | ein Schlaf mit Wirkbereich (Schlaf des Magiers, 5 y um das Ziel) geht auf den Gegner, dessen Kreis die meisten wachen Angreifer einschläfert, die danach schlafen bleiben; ein Schlaf ohne Wirkbereich (Schlummer) wählt wie bisher | `SleepFurtherAttacker` | Spur „deep dungeon: sleep chosen … N more asleep in its area“ |

*Grundlagen:*
- *Was zieht:* Schaden an einem Gegner, der noch nicht kämpft. Heilhass verteilt das Spiel auf die Gegner im Kampf
  (Akhmorning, Feindschaft, Shadowbringers); Heilung allein zieht keinen ruhenden Gegner. Gegner der Gewölbe greifen auch
  auf Sicht, Geräusch und Nähe an (consolegameswiki, „Aggression") – das ist Laufen, nicht Rotation.
- *Wer „noch nicht kämpft":* kein Ziel und nicht im Kampf (A833, `DataCenter.NotFightingYet`).
- *Welche Aktionen heilen und treffen:* Wirktext, der Gegner trifft (A826, A832), eine Heilung nennt („Restores … HP")
  und Gruppenmitglieder („party members"): Assise, Makrokosmos (Zeile 25874), Pneuma (24318, eine Linie), in Kreszentia
  „Das Jüngste Gericht" (41637). Nicht dazu: Aktionen, deren Heilung nur den Spieler oder das Kardion-Ziel trifft –
  Heiliger Kreis (Paladin), Dyskrasia, Eukrasische Dyskrasia, Abyssal Drain; sie sind Schaden mit einem Nebeneffekt und
  fallen unter die Schadensoption. Gelesen am englischen Blatt wie die Einstufung (Abgleich aller Aktionen mit Flächentext
  und Heiltext, 15.11.2026).
- *Zieht ein Schlaf?* Ein Schlaf ist eine feindliche Aktion; ein ruhender Gegner, den er trifft, schläft, steht aber im
  Kampf und greift beim Aufwachen an (*unbelegt, Schluss aus der Feindschaftsregel des Spiels:* jede feindliche Aktion
  erzeugt Feindschaft). Deshalb zieht auch die Fläche des Magier-Schlafs (5 y um sein Ziel) einen Gegner – mit 30 s
  Verzögerung.
- *Schlaf:* Repose (Heiler), Sleep (Magier), 30 s, endet beim ersten Schaden; Bosse und viele Gegner der Gewölbe sind
  immun (Führer je Stockwerk, icy-veins Palast der Toten 1–50 und 51–100). RSR kennt den Schlaf schon: die Schlafregel
  gegen gegnerisches Wirken (Konzept 08, 13), mit `SleepInterruptWatch.CanStillBeSlept` (kein Boss, keine Schlafresistenz,
  keine gelernte Sperre der Art, nicht zu oft in diesem Kampf).
- *Verlängern (sein Hinweis, 16.11.2026):* Ein erneuter Schlaf auf einen Schlafenden setzt die Dauer neu, kürzer als
  zuvor; nach mehreren Schlafen widersteht der Gegner eine Weile (Gemeinschaftsquellen der Schlafwache, Square-Enix-Forum
  2013 und 2016: „30s, 15s, 8s, then immune"; kein Spieltext – die Zahlen sind unbestätigt, die Regel braucht sie nicht). Wie viele Schlafe eine Art
  verträgt, lernt die Schlafwache je Art (`SleepDiminishing`) und zählt die gelandeten je Kampf; `CanStillBeSlept` sperrt
  den Schlaf, sobald die Grenze erreicht ist.
- *Was RSR vor A834 nicht tat:* Einen schlafenden Gegner schützte keine Fläche, außer er trug eine Stopp-Markierung („Never
  attack targets with stop markers"); die Zielwahl griff ihn an wie jeden. Ein schlafen gelegter Zusatzgegner wachte
  beim nächsten Flächenangriff auf. Die Zielart ab Werk („All targets when solo in duty") wählte allein in den Gewölben
  auch im Kampf einen Gegner, der noch nicht kämpft, als neues Ziel.
- *Wie eine Aktion ihr Ziel findet:* Jede feindliche Aktion wählt ihr Ziel aus der Liste der Ziele
  (`TargetHelper.GetTargetsByRange` → `DataCenter.AllHostileTargets`), und eine Fläche auf ein Ziel scheidet aus, wenn ihr
  Kreis einen Unberührbaren fängt (`HitsUntouchable`, `MustNotBeHit`). Ein gehaltener Schläfer steht nicht in der Liste
  und ist unberührbar – für Schaden gewollt, für einen Schlaf nicht: Ein Schlaf weckt niemanden.

*Optionen:*
- (0) Nullvariante: der allgemeine Schalter, von Hand je Inhalt umgestellt – kostet ihn in Dungeons und FATEs den
  schnellen Zusammenzug oder in den Gewölben den Schutz.
- (1) Der allgemeine Schalter gilt in den Gewölben von selbst – eine Option, nicht zwei; Heilung und Schaden nicht trennbar
  (gegen G2/G3).
- (2) Zwei Optionen nur für die Gewölbe, Heilung mit Schaden und Schaden, ab Werk an; der allgemeine Schalter bleibt
  daneben – gewählt.
- (3) Wie (2), dazu die Zielwahl im Kampf der Gewölbe nur unter kämpfenden Gegnern (Teil von G3) – gewählt; über die
  Zielart „Solo Deep Dungeons" ginge das nur allein und nur mit Umstellen der Zielart, die auch außerhalb gilt.
- (4) Zusatzgegner schlafen (G4) – mit Flächenschutz und Zurückstellen des Schlafenden in der Zielwahl; ohne beides weckt
  RSR ihn selbst.
- Rückbau: drei Optionen aus; nichts Gespeichertes hängt daran.

*Abwägung im Kampf:*
- (2)+(3) *Stärken:* Allein auf Stockwerk 140 der Himmelssäule kämpft er gegen einen Gegner, ein zweiter steht 15 y
  daneben und ruht im Raum (noch kein Kampf): Assise wartet, bis der erste fällt oder der zweite angreift; die Schadensflächen ebenso;
  fällt der erste, wählt RSR den zweiten nicht von selbst, er zieht ihn, wenn er will. *Schwächen:* Ein Heiler mit
  Assise im Kampf heilt einmal weniger, solange ein ruhender Gegner in 20 y steht; ein Tank verliert die Flächen, mit denen
  er sammeln würde – in den Gewölben gewollt. *Bedrohung:* Gegner, die das Spiel aneinander bindet, ziehen einander auch
  außerhalb der Reichweite nach; das erkennt RSR nicht.
- (4) *Stärken:* Drei Gegner greifen einen Weißmagier an; zwei schlafen, er kämpft gegen einen, der Schaden sinkt auf
  ein Drittel. *Schwächen:* je Schlaf ein GCD mit Wirkzeit; eine Fläche trifft nur die Wachen, der Kampf dauert länger;
  ein Schlafender wacht nach 30 s oder bei jedem Schaden (Gift, eine Fläche eines Mitspielers). *Bedrohungen:* Immunität
  ohne Status – der erste Schlaf geht ins Leere (Preis: ein GCD je Art, dann gelernt und je Spielversion gespeichert); in der Gruppe weckt
  ein Mitspieler ihn. Bis A836 schlief die Regel deshalb nur Gegner, die den Spieler selbst angreifen – meine Ableitung;
  in der Gruppe hält der Tank die Feindschaft, und dann schlief nie einer. Nach seiner Präzisierung (16.11.2026) gilt der
  Schlaf allen Angreifern der Gruppe; der Tank nimmt sich den nächsten Schläfer, wenn die übrigen Angreifer tot sind.

*Gebaut (A834, nach der Prüfrunde überarbeitet):*
- Drei Optionen unter „Deep dungeon" (Abschnitt der Gewölbe), ab Werk an (code.md: neue Regeln werden getestet):
  „Deep dungeons: heals that also deal damage don't pull new enemies", „Deep dungeons: damage doesn't pull new
  enemies", „Deep dungeons: put further enemies that attack your party to sleep".
- Heilung mit Schaden: Die Prüfung „würde ziehen" (`DataCenter.GuardsAgainstPull`, `NotFightingYet`) gilt in den Gewölben
  mit der ersten Option für Aktionen, die laut Wirktext die Gruppe heilen, mit der zweiten für jede andere feindliche
  Fläche – auch den Schlaf, der zieht wie Schaden (Grundlagen). *In den Gewölben entscheiden ihre Optionen, außerhalb der
  allgemeine Schalter* (Konzeptprüfung 16.11.2026): Bis dahin galt der allgemeine Schalter auch in den Gewölben zusätzlich,
  und mit ihm wartete die Heilung selbst dann, wenn ein Gruppenmitglied kritisch war – die Ausnahme unten griff nicht.
  Zwei Schalter für dieselbe Lage widersprechen sich sonst; der für den Inhalt eigene geht vor. Wer in den Gewölben mit
  Flächen ziehen will, schaltet deren Option ab.
- *Notfall:* Ist ein Gruppenmitglied in der kritischen Klasse (dieselbe wie in der Heilkette), wartet die Heilung nicht.
  Grund: In den Gewölben endet ein Tod allein den Lauf; der gezogene Gegner kommt erst nach Sekunden und trifft auf einen
  geheilten Spieler, gegen ihn stehen Schlaf, Flucht und Pomander. *Schluss, unbelegt am Spiel:* dass die Heilung den
  Tod verhindert, wo sonst keine greift; die Ausnahme lässt sie nur zu, die Heilkette wählt.
- Schaden, Zielwahl: Mit der zweiten Option ist im Kampf in den Gewölben ein Gegner, der noch nicht kämpft, kein Ziel.
  Außerhalb des Kampfes bleibt die Zielart; das Ziel, das er zieht, kämpft danach. Jeder so ausgenommene Gegner steht
  einmal je Stockwerk in der Spur. *Preis:* Will er im Kampf bewusst einen weiteren Gegner ziehen, greift RSR ihn nicht an,
  solange er ruht – ein Angriff von Hand zieht ihn, danach kämpft er und ist Ziel. Ihn als Ziel zuzulassen, weil er sein
  Ziel ist, hilft nicht: Ein vor dem Kampf gewähltes Ziel, das ruht, während ein anderer Gegner angreift, würde dann
  gezogen – genau der Zusatzgegner, den die Option vermeiden soll.
- Schlaf: Ein Heiler oder Magier im Kampf in den Gewölben, dessen Gruppe mindestens zwei wache Gegner angreifen (ihr Ziel
  ist ein Gruppenmitglied, allein der Spieler; wie RSR die Gruppe zählt, `IsParty`), legt einen davon schlafen, der nicht bekämpft wird
  (nicht das Ziel des Spielers, nicht das Ziel eines Tanks der Gruppe), nicht stirbt (`IsDying`), keinen Schaden über Zeit trägt (der
  nächste Tick weckte ihn) und schlafen kann (`CanStillBeSlept`) – nicht, solange ein Gruppenmitglied in der kritischen
  Klasse ist (Heiler), und nicht unter Swiftcast (seine Regel: für die Wiederbelebung). Von mehreren schläft der mit den
  meisten LP zuerst – der Gegner, der am längsten gegen die Gruppe stünde; bei Gleichstand der mit der kleineren Id.
  Grund für die zweite Stufe: Gegner einer Art erscheinen mit gleichen LP, und die Reihenfolge der Gegnerliste wechselt von Frame
  zu Frame; ohne feste Reihenfolge sprang die Wahl je Frame zwischen ihnen (Upload vom 16.11.2026, Pilgers Pfad: zwei
  frisch erschienene Gegner mit je 591 262 LP, bis zu 36 Spurzeilen „sleep chosen“ je Sekunde statt einer). Im Kampf
  änderte das nur, welchen der beiden der Schlaf traf; die Spur, an der die Regel gemessen wird, war unlesbar.
  *Schlaf mit Wirkbereich (G6):* Der Schlaf des Magiers legt das Ziel und jeden Gegner bis 5 y um es schlafen (Wirktext
  #25880, Blatt `Action`, Wirkbereich 5). Er geht deshalb auf den Gegner, dessen Kreis die meisten Angreifer einschläfert,
  die danach schlafen bleiben – wach, nicht bekämpft, nicht sterbend, ohne Schaden über Zeit, ihre Art schlafbar; in einer
  geschlossenen Gruppe ist das der in der Mitte. Ein schon schlafender Gegner im Kreis zählt dagegen, weil ein neuer Schlaf
  kürzer hält (G5); erst danach entscheiden die LP und die Id. Die gehaltenen Schläfer liest die Wahl aus ihrer eigenen Liste (`DeepDungeonCrowd.Sleepers`), denn in der Liste der Ziele stehen sie nicht (oben, „Wie eine Aktion ihr Ziel findet“ in den Grundlagen); aus der Liste der Ziele nur einen Schläfer, der bekämpft wird. Gewählt wird nur ein Ziel, dessen Kreis die übrigen Sperren
  erfüllt (ruhende und unberührbare Gegner, unten). *Verworfen:* der Gegner, der dem Schwerpunkt aller Angreifer am
  nächsten steht – zwischen zwei Gruppen trifft er keinen weiteren; die Zahl misst, wozu die Mitte dient. *Im Kampf:* Fünf
  Angreifer, drei dicht beieinander, zwei am Rand: bisher schlief der mit den meisten LP, oft ein Randgegner, allein; jetzt
  schläft der mittlere der drei und mit ihm die beiden anderen. Für den Weißmagier ändert sich nichts, Schlummer trifft
  nur sein Ziel.
- *Verlängern (G5):* Vor einem neuen Schläfer prüft die Regel die Schlafenden. Liefe ein Schlaf aus, bevor ein Schlaf
  im nächsten GCD noch landen könnte – Restdauer höchstens Wirkzeit, laufender GCD, gemessene Verzögerung bis zur Wirkung
  und ein GCD –, legt sie ihn erneut schlafen, den mit der kürzesten Restdauer zuerst, bei gleicher Restdauer – ein Schlaf des Magiers trifft mehrere zugleich – den mit der kleineren Id; vorher nicht, denn jede
  Verlängerung kostet vom Rest des alten Schlafs und hält kürzer. Nur solange ein wacher Gegner die Gruppe angreift: Ist
  keiner mehr da, wird der Schläfer ohnehin das nächste Ziel. Dieselben Ausschlüsse wie beim ersten Schlaf (Grenze der Art,
  ruhende Gegner im Bereich). Wacht er trotzdem auf, greift er an und zählt wieder
  als wacher Angreifer.
- Wer schläft, ob RSR, er oder ein Mitspieler ihn schlafen legte, trifft keine Fläche RSRs (`MustNotBeHit`), und er wird
  Ziel erst, wenn kein wacher Gegner mehr steht. Grund: Der Schlaf eines anderen hält denselben Zusatzgegner fern.
  *Ausnahme:* was bekämpft wird – das Ziel des Spielers und jedes Ziel eines Tanks. Schläft es mit ein, bleibt es Ziel,
  und der nächste Angriff weckt es (Hinweis unten). Ein Tank mit RSR greift Schläfer erst an, wenn kein wacher Gegner
  mehr steht (seine Präzisierung); seine Flächen sparen sie ebenso aus.
  *Kosten:* Welche Ziele bekämpft und wer zur Gruppe gehört, liest ein Durchgang über die Figuren je Frame vor dem
  Listenbau, nur in den Gewölben mit der Option an.
- *Zielen auf den Schläfer (Konzeptprüfung 16.11.2026):* Ein Schlaf weckt niemanden. Deshalb zählt ein gehaltener
  Schläfer für einen Schlaf nicht als unberührbar (`ActionTargetInfo.HitsUntouchable`, für Schlummer und Schlaf) – das
  gilt auch für die Schlafregel gegen gegnerisches Wirken (Konzept 08) in den Gewölben. Und der Schlaf dieser Regel geht
  über ein Regelziel (`ActionTargetInfo.RuleTargetId`) auf genau den gewählten Gegner, auch wenn er nicht in der Liste der
  Ziele steht – geprüft wie ein befohlenes Ziel (Reichweite, Sicht, ob das Spiel die Aktion auf ihn zulässt). Was der
  Kreis sonst nicht treffen darf, prüft die Regel dann selbst: ruhende Gegner (G3) und jeder andere Unberührbare
  (Stacheln, Stopp-Markierung, von einer Tötungsregel gehalten). Bis dahin gingen beide Wege über die Liste: Eine
  Verlängerung fand ihr Ziel nie (der Schläfer steht nicht darin), und der Schlaf der Magier schied aus, sobald ein
  Schläfer im Kreis lag – nach dem ersten Schlaf schlief kein zweiter Gegner daneben ein.
- *Nachsteuerung (code.md, „Entscheidung zur Laufzeit"):* Ob ein Schlaf auf eine Art wirkt, lernt die bestehende
  Schlafwache (`SleepInterruptWatch.ObserveOwnSleep`) aus der Antwort des Spiels für jedes Ziel jedes eigenen Schlafs –
  widerstanden, unberührt, nach mehreren Schlafen abgenutzt –, gespeichert je Art und Spielversion; `CanStillBeSlept`
  liest sie. Eine eigene Buchführung braucht die Regel nicht. Sie merkt sich nur den Gegner, auf den gerade ein Schlaf gedrückt
  wurde – von ihr oder von ihm –, bis sein Status erscheinen konnte (Wirkzeit plus ein GCD), damit der nächste GCD ihn
  nicht ein zweites Mal wählt. Ab dem Drücken, nicht ab der Wahl: sonst wählte der nächste Frame vor dem Drücken einen
  anderen Gegner.
- *Mitschläfer (sein Hinweis, 16.11.2026):* Der Schlaf der Magier trifft 5 y um den gewählten Gegner und legt alles darin
  schlafen, auch das Ziel des Spielers. Ein Gegner, der „irrtümlich mitschläft", wird vom nächsten Angriff eines
  Gruppenmitglieds geweckt – allein vom nächsten eigenen. Bis A835 wählte die Regel deshalb keinen Zusatzgegner näher
  als 5 y am Ziel; da nahkämpfende Gegner dicht stehen, schlief beim Magier dann meist keiner. *Jetzt:* Die Regel wählt
  auch nahe Gegner. Das mitschlafende Ziel bleibt Ziel – sonst wechselte RSR davon weg und schonte es mit den Flächen –,
  und der nächste Angriff weckt es; bis dahin trifft es nicht. Weitere wache Angreifer im Kreis schlafen mit und bleiben
  draußen wie jeder Schläfer. Ruhende Gegner im Kreis sperren den Schlaf weiter (G3).
  - *Gegenthese (das Ziel einzuschläfern kostet):* widerlegt; der Angriff, der es weckt, wäre ohnehin gekommen, und bis
    dahin fällt sein Angriff aus. Preis: Eine Fläche um das Ziel wartet, solange ein Mitschläfer darin liegt; RSR greift das
    Ziel dann mit Einzelangriffen an.
  - *Gegenthese (der Spieler hat sein Ziel selbst schlafen gelegt und will es nicht angreifen):* Dann wechselt er das Ziel
    selbst; solange es sein Ziel ist, greift RSR es an. *Preis:* ein Schlaf, den er auf sein Ziel legt, ohne umzuschalten,
    hält nur bis zum nächsten Angriff.

*Konzeptprüfung (16.11.2026, A837):* Zwei Befunde hoher Schwere am Code: Die Verlängerung (G5) konnte nie greifen, und
der Schlaf der Magier ging neben einem Schläfer nicht (beides „Zielen auf den Schläfer"). Ursache: Die Schlafregel nahm
die Zielwahl der Schadensaktionen, die Schläfer gewollt ausschließt; die Prüfung von G5 rechnete nur den Zeitpunkt nach,
nicht den Weg zum Ziel. Dazu: Vorrang der Gewölbe-Optionen vor dem allgemeinen Schalter (Notfall griff sonst nicht), der
Preis eines bewusst gezogenen Gegners, „wacher Angreifer" für die Verlängerung, veraltete Stellen in allen vier
Abschnitten.

*Prüfrunde (A834, unabhängiger Prüfer, eine Runde):* Zwei Befunde hoher Schwere – ein Befund am Vorgehen, nicht am
Artefakt. (1) Die Regel führte eine eigene Buchführung, ob ein Schlaf landete, und erklärte eine Art für nicht schlafbar,
wenn einen GCD nach dem Wirken kein Status stand; das doppelte die Schlafwache und hätte bei Verzögerung, Abbruch oder
einem Schlaf auf ein anderes Ziel falsch gelernt. Die Anforderung G4 verlangt das Lernen, nicht eine zweite Mechanik
dafür – entfernt, die Schlafwache trägt es. (2) Die Schadensoption galt auch für den Schlaf; der Prüfer las den Text
„damage" als Grenze. Geprüft am Zweck (REGEL: der erkennbare Zweck entscheidet): G3 will Zusatzgegner vermeiden, und
ein geschlafener Ruhender ist einer mit 30 s Verzögerung (Grundlagen). Behalten; der Optionstext nennt den Schlaf jetzt.
Mittlere Befunde eingearbeitet: Notfall (G2), Gruppenheilung statt jeder Heilung im Text, Schaden über Zeit, Spur für
G2 und G3, Preis der Magier (seit A836 durch seinen Hinweis zu Mitschläfern aufgehoben). Niedrige: die Zuordnung der Spur der parierenden Gegner im Listenbau, Kosten (der
Schläfer wird je Gegner nur über seinen Status gelesen, nur in den Gewölben).

*Tragende Annahmen und Vorhersage:*
- Gegner, die das Spiel aneinander bindet (Rudel, die Begleiter der Tisiphone), ziehen einander auch ohne Treffer nach;
  das erkennt RSR nicht. *Unbelegt*, ob es in den Gewölben solche Bindungen gibt; ein Gegner, der so kommt, hat danach ein
  Ziel und zählt als kämpfend.
- Ein Schlaf zieht einen ruhenden Gegner (Grundlagen, unbelegt).
- *Vorhersage:* In seiner nächsten Spur aus den Gewölben stehen bei Kämpfen mit zwei oder mehr Angreifern die Zeilen
  „deep dungeon: sleep chosen for …", danach „deep dungeon: sleep landed on …" oder ein gelernter Widerstand der Schlafwache; neben einem
  ruhenden Gegner „deep dungeon: … waits" vor Assise oder Makrokosmos. Widerlegt, wenn im Kampf mit zwei Angreifern keine
  Schlafzeile steht, obwohl ein Heiler oder Magier spielte und kein Mitglied kritisch war.

*Falsifikation:*
- (1) *Kein Defekt – der allgemeine Schalter genügt:* widerlegt; er gilt überall, und er will in Dungeons und FATEs den
  Zusammenzug (A833).
- (2) *Option falsch – „noch nicht kämpft" trifft im Gewölbe auch Gegner, die gleich kommen:* Ein Gegner, der dich
  bemerkt und läuft, hat ein Ziel – er kämpft und zählt nicht als neu. Einer ohne Ziel ruht. Preis: keiner bekannt.
- (3) *Ausgeliefert, und nichts ändert sich:* Er spielt die Gewölbe als Weißmagier oder Magier allein; ohne Gegner im
  Radius ändert sich nichts, mit ruhendem Gegner wartet die Fläche, mit zwei Angreifern schläft einer – sichtbar in der
  Spur. Widersteht eine Art dem Schlaf, lernt die Schlafwache es und speichert es je Spielversion.
- *Gegenthese (Verlängern lohnt nicht, der dritte Schlaf hält nur 8 s):* Widerlegt für die Zahlen der Forenquellen: Ein GCD
  Wirkzeit (2,5 s) steht gegen 8 s, in denen der Gegner nicht trifft; danach sperrt die gelernte Grenze den Schlaf, und
  die Regel versucht keinen vierten. *Preis:* In der Gruppe kann ein Mitspieler denselben Schläfer verlängern; dann geht
  ein GCD doppelt – die Regel sieht nur eigene Schlafe als laufend.
- *Ausgeliefert, und die Verlängerung ändert nichts (16.11.2026 gehalten, behoben):* Der Weg zum Ziel führte über die
  Liste, in der kein Schläfer steht. Seitdem das Regelziel; die Spur „sleep chosen … renewed" zeigt jede Verlängerung.
- *Gegenthese (Schlaf kostet mehr, als er bringt):* Gegen zwei schwache Gegner wäre eine Fläche schneller. Die These
  gilt für Gegner, die gleich fallen – deshalb nimmt die Regel sie aus (`IsDying`). Für die übrigen widerlegt: Ein GCD
  Wirkzeit steht gegen bis zu 30 s, in denen einer statt zwei trifft; nach seiner Spielweise (Sicherheit vor Schaden,
  gewichtet mit der Wahrscheinlichkeit) zählt der sichere Treffer der zwei Angreifer. Wer Schaden anders gewichtet,
  schaltet die Option ab.
