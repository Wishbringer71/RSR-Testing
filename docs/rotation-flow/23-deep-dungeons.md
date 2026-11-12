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

*Gegenthese 1 – Eine Formfläche zieht Gegner, die noch nicht kämpfen:* wie jede Fläche; „Don't pull new enemies with
AoE" gilt auch hier.
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
*Gegenthese 3 (ausgeliefert, und nichts ändert sich):* In Pilgers Pfad ist der Agent ohne Fenster leer – dann bleibt dort
alles wie bisher; die Spurzeile je Stockwerk zeigt es, und ein Upload belegt die Zuordnung. In Himmelssäule und Eureka
Orthos trägt die Spalte, solange die abgeleitete Bedeutung des Werts stimmt; stimmt sie nicht, zeigt die Spurzeile den
Widerspruch zum Agenten.
