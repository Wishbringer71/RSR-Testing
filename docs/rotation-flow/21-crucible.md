# 21 · Crucible of the Unbroken

**Leitfrage:** Wie spielt der Beastmaster die Bretter des Crucible – Kader, Hörner, Wege, Läden, Schätze, Lager,
Gegenstände und den Endkampf?

Nur der Beastmaster kämpft im Crucible. Die Regeln des Jobs, auf denen dieses Konzept aufbaut – die Hörner nach Lage
(B0 bis B5), Final Sting und das Halten der Hörner (R1), die Zielart ohne Tank und Heiler, Snarl und Challenge –
stehen in `01-jobs.md`, Abschnitt „BST"; wo hier „oben" steht, meint es dieses Konzept. Ordnung der Konzepte: `20-concept-order.md`.

**Crucible of the Unbroken: Bretter, Kämpfe, Strategien** (A532). Sein Auftrag vom 05.11.2026: „alle bretter von
crucible researchen, analysiere, strategien entwickeln. konzept bauen. im vollen kritischen loop"; dazu sein Beispiel
als Vorschlag (K2) und seine Fragen, ob RSR im Brett Begleiter wählen und Gegenstände einsetzen kann.

*Quellen* (05.11.2026): Spielblätter über xivapi v2 mit dem Schema exdschema@2:rev:a1e24a0c (`XBMContent`,
`XBMContentBattle`, `XBMBattleDetail` mit einer Unterzeile je Gegner: Name, Schwäche, Werte, Zustandsresistenz über
`BNpcResist`, zwei Aktionen; `XBMPet`, `Pet.Abilities`, `XBMItem`, `XBMContentCamp`), en und de mit Zeilen-Id;
BossModReborn 577675d (31.10.2026), Module aller fünf Bretter; consolegameswiki „Crucible of the Unbroken" und „First
Board of the Unbroken"; BeastHelper be40a1e (26.10.2026, AGPL, nur gelesen) und FFXIVClientStructs b69073a (02.11.2026),
beide Werkzeugquellen; sein Mitschnitt vom 04.11.2026.

*Regeln des Spiels, die eine Strategie tragen:*
- Fünf Bretter (`XBMContent`): erstes Stufe 30, Rang 5, Kader 10; zweites 40, 10, 12; drittes 50, 15, 14; Erstes
  Meisterbrett 50, 20, 12; Zweites Meisterbrett 50, 25, 15. Ein Brett ist ein Weg aus Feldern mit Abzweigungen: Kampf,
  Elite, Endgegner, Campsite, Laden, Schatz, Zufall (Wiki).
- Den Kader wählst du vor dem Betreten; je Kampf gehen drei auf die Hörner, einer ist draußen. Die Belegung gilt je
  Kampf und wird vor ihm gesetzt (Wiki; seit 7.57 automatisch nur bei höchstens drei im Team).
- Keine natürliche LP-Regeneration, für dich und die Begleiter (Wiki); deine LP bleiben von Kampf zu Kampf stehen
  (gemessen 04.11.2026). Normale Heiltränke sind gesperrt, nur Brett-Gegenstände und Essen.
- Campsite: Du ruhst mit bis zu einem bis vier Begleitern (`XBMContentCamp.FamiliarRecoverCount`, je Lager); die Heilung
  teilt sich nach dem Wiki zum ersten Brett „allein 90 %, mit einem je 45 %, mit zweien je 30 %", die Hauptseite sagt,
  du heilst immer selbst. Gemessen einmal (04.11.2026, 19:44–19:47): +30 % deiner LP. Ein gefallener Begleiter wird
  dort nicht geheilt.
- Fällst du, ist der Lauf verloren, außer eine Bestien-Lebensfeder, ein Opferring oder Sand der Zeit greift. Fällt
  ein Begleiter, fehlt er bis zum Horn der Rückkehr.
- Vermeidbare Flächen legen dem Getroffenen Verwundbarkeit oder Schadensminderung auf; hält dein Begleiter die
  Feindseligkeit, trägt er sie, und Schaden nimmt er aus ihnen keinen. Unvermeidbare Flächen treffen euch beide (Wiki).
- Jeder Gegner trägt eine Schwäche aus neun Arten (Hieb, Stich, Schlag, Feuer, Eis, Wasser, Wind, Erde, Blitz) oder
  keine, dazu Anfälligkeiten für Zustände (`BNpcResist`). Wie viel mehr Schaden eine Schwäche bringt, nennt keine
  Quelle (unbelegt).
- Punkte (Wiki): Zuschläge für keine Gegenstände, keine Rast, keine Schwächung, kleine Teams, gefallene oder keine
  gefallenen Begleiter. Sie stehen teils gegen deine Sicherheit; RSR richtet sich nach Sicherheit vor Schaden
  („Spielweise"), Punkte sind kein Ziel einer Regel (meine Ableitung).

*Die Bretter* (`XBMContentBattle`, `XBMBattleDetail`; Schwäche in Klammern; deutsche Namen aus den Blättern):
- *Erstes Brett:* Knochen-Ritter und -Bischof (Schlag; der Ritter wehrt mit Forward Guard Angriffe von vorn ab, nicht
  die der Begleiter, Ossify unterbrechbar); Erzdämonen-Ebenbild (Blitz); Pisco-Dämonen-Ebenbild (Hieb; anfällig für
  Gift und Gewicht); Terrormilben- und Schattenmilben-Ebenbild (Eis; Tankbuster mit Gift); Oger-Ebenbild (Wind) mit
  Irrlicht- und Riesen-Irrlicht-Ebenbild (Wasser; anfällig für Fessel und Gewicht). Endgegner Seelenlose (Pas de Seul,
  Stich): ruft nach etwa 45 s eine Sukkubus-Magierin und zwei -Ritterinnen (Feuer; die Magierin stärkt ihn mit
  Fanaticism, 9 s Wirkzeit, anfällig für Unterbrechen).
- *Zweites Brett:* Manticore (Wind; Tankbuster); Taurus (Stich), ruft Ahriman (Eis; Blick mit Verhängnis); Wyvern
  (Blitz); Leerenmagus (Erde) mit Zombie (Eis); Älterer und Jüngerer Taurus (Feuer; der Jüngere stärkt); Dämon (Hieb),
  Böse Waffe (Eis), Eufel (Erde). Endgegner Losfrox Tintenklecks (Schlag) mit Töffelchen (Wasser).
- *Drittes Brett:* Kavalier (Feuer) mit Knochen-Bischof (Schlag); Ymir (Eis) mit Sahagin (Schlag, Dreadwash
  unterbrechbar); Montisaurus (Erde) mit Hahn und Henne (Schlag); Catblepus (Wind; Blick mit Versteinerung); Lakhamu
  (Wasser) mit Golem (Wasser, stärkt); Sirene (Stich, ruft Gegner) mit schlurfendem und kriechendem Ebenbild (Feuer);
  Kämpfer, Kaktor, Blütenkaktor (heilt), Soldat (Blitz) und Wächter (Eis, deckt andere). Endgegner Guttler (keine) mit
  Thanatos (Blitz).
- *Erstes Meisterbrett:* Strix (Stich; Federn als Adds); Morbol, Keimling, Ochu (Hieb) mit Aasbrühe (Eis); Titanenwurz
  (Wind) mit Falkenkönigin (Schlag); Frostdrache und Eis-Exergon (Feuer); Gargoyle (keine; Tankbuster); Administrator,
  Käfer, Fußsoldat, Brummonaut, Drohne (Blitz; Confound unterbrechbar) mit Kobra (Stich); Golem (Wasser); Baumschrat,
  Schnecke, Schössling (Schlag) mit Schaudermilbe und Biloko (Eis; Biloko stärkt); Progenitrix, Granate, Bomber (Wind)
  mit Snoll (Feuer) und Pyrobolos (Wasser). Endgegner Borgny Giftschleuder (Erde) mit vergiftender Substanz (Wasser).
- *Zweites Meisterbrett:* Flauros und Blitz-Exergon (Erde); Gedankenschinder (Hieb) mit Fliegerpilz (Wasser);
  Butzemann (Eis) mit Glotzauge und Bomber (Wind; Blick mit Versteinerung) und Licht-Exergon (keine); Atomos (Schlag)
  mit Gremlins und Vodoriga (Stich), Pudding (Wind), Creme (Blitz), Fran (Erde), Bedrohlicher Dahak (Eis); Drakon
  (Wind) mit Dornmull (Feuer), Abaddon (Wasser), Morpho (Schlag); Durga (Feuer, ruft Gegner) mit Drehturm (Schlag);
  Medusa (Stich, ruft Gegner) mit Lamia (Stich, Raise unterbrechbar) und Zyklop (keine); Chimära (Wasser); Sphinx
  (Hieb; Rätsel); Gigantis und Zyklop (keine) mit erstarrtem Blitz (Erde) und Zunder (Wind); Moogle-Kampf (Blitz,
  Schlag, Wasser; Heiler und Stärker); Behemoth (Erde) mit Riese (Blitz) und Blitz-Exergon (Erde); Königs-Ahriman (Eis)
  mit Hapalit (Wind) und Trübauge (Eis). Endgegner Lauda Spruchspalterin (keine) mit Thanatos (Blitz).
- Häufigste Schwächen: erstes Brett gleich verteilt; zweites Eis; drittes Blitz und Schlag; Erstes Meisterbrett Blitz,
  Wind, Schlag; Zweites Meisterbrett Schlag, Blitz, Erde, Wind.

*Schadensarten der Begleiter* (`Pet.Abilities`, Aspekt und Angriffsart der Dressur D und des Entfesselns E; Pet-Zeilen
47–97, de): Hieb Cu Sith (D, E), Puk (E), Mantis (D), Dullahan (D); Stich Schaudermilbe, Vespula (D, E), Kaktor, Kolibri
(D, E), Treiberameise, Morbol, Kobra, Hydra, Karlabos (D); Schlag Schaf, Opo-Opo, Mandragora, Büffel, Goobbue,
Montisaurus (D), Spriggan, Karpfenkröte (E); Feuer Baumhörnchen, Puk, Spriggan, Drakon, Chimäre (D), Raptor, Behemoth
(E); Eis Ziz, Uragnit, Karpfenkröte, Raptor, Eisgolem (D), Chimäre (E); Wasser Pugil, Megalokrabbe, Krabbe, Apkallu,
Salamander (D), Megalokrabbe, Apkallu, Karlabos (E); Wind Kapgeier, Fliegenfalle, Geist, Kleinlibelle, Rafflesia (D),
Kapgeier, Baumschrat, Montisaurus (E); Erde Dodo, Golem, Riesenwurm, Baumschrat, Verrottende Goobbue (D), Golem,
Riesenwurm (E); Blitz Kobalos, Gespenst, Adaman-Taimai, Coeurl, Behemoth (D), Coeurl (E). Unterbrechen setzen Kobalos,
Dullahan, Golem, Spriggan und Eisgolem (`XBMPet.InflictsInterrupt`).

*Strategien* (je mit Wirkung im Kampf, Abwägung, Gegenthese; Stand am Ende):
- *K1 – Schwäche treffen.* Unter den bereiten Hörnern zählt ein Begleiter, dessen Dressur oder Entfesseln die Schwäche
  der Gegner trifft, um den Zuschlag der Schwäche mehr (B3 rechnet heute Potenz mal Treffer). Den Zuschlag lernt RSR an
  den eigenen Treffern je Brett (unten, „K1, Zuschlag gemessen"); bis zur ersten Messung gilt der Wert aus seinen
  Mitschnitten, +27 % (A582). *Stärke:* der passende Begleiter steht länger draußen. *Schwäche:* ein Brett mit
  gleich verteilten Schwächen gewinnt wenig. *Gegenthese:* Der Zuschlag ist klein, und eine Fläche auf drei Gegner zählt
  mehr. Nicht widerlegt, aber genau das rechnet das Produkt: Ein kleiner gemessener Zuschlag ändert die Reihenfolge
  nicht.
- *K2 – Horn für spätere Gegner aufheben* (sein Vorschlag, Beispiel Seelenlose). Das Blatt nennt je Kampf alle Gegner;
  wer zu Beginn fehlt, kommt später. Trifft ein bereiter Begleiter deren Schwäche und keinen der Anwesenden, ruft RSR
  ihn nicht im Zyklus, sondern sobald sie erscheinen, und greift sie an, bis sie fallen (die Zielart „wenigste LP" tut
  das schon, A525). *Beleg:* Am 04.11.2026 um 19:48 kam der Raptor (Entfesseln Feuer) 8 s nach Kampfbeginn und ging
  6 s später; sein Feuer traf die Seelenlose (Stich). Die drei feuerschwachen Sukkubi erschienen 45 s nach Beginn, alle
  Hörner klangen ab. *Stärke:* die Gegner, die den Boss stärken oder dich zusätzlich treffen, fallen früher. *Schwäche:*
  das gehaltene Horn ruht bis zu ihrem Erscheinen, ein Einsatz weniger im Kampf. *Gegenthese:* Die Adds fallen auch so
  schnell (1.700 bis 2.000 LP gegen 19.600 des Bosses). Teilweise: Der Gewinn hängt am Zuschlag (K1) und an der Dauer
  bis zum Ruf; das Halten kostet nur, solange das Horn sonst einen Einsatz hätte. Der Ruf kommt laut Blatt sicher. Gilt
  für alle Kämpfe mit Ruf: Seelenlose, Taurus, Sirene, Ymir, Kämpfer, Strix, Progenitrix, Drakon, Durga, Medusa,
  Flauros.
- *K3 – Wen RSR angreift: größter Nutzen für den Kampf und das Brett* (seine Vorgabe vom 05.11.2026: „entscheidung, wer
  angegriffen werden soll nach analyse des größten nutzens für das gesamte spielfeld und darüber hinaus für das gesammte
  brett"; im Crucible; außerhalb dieselbe Regel ohne den Bedarf des Bretts, Konzept 03, „Zielart ohne Tank und Heiler", A537).
  *Gebaut* (A541, A549, A582, `EnemyDanger`): Gefahr je Sekunde aus den Treffern auf die Gruppe und auf deinen
  Begleiter, angekündigte Treffer, Heilungen auf andere Gegner, geteilt durch die LP, Wechsel über der Messunsicherheit;
  vor jeder Messung die Werte des Blatts; ein Gegner ohne eigene Messung mit einer Gefahr, die aus den gemessenen
  geschätzt ist; der Zuschlag der Schwäche auf die Zeit bis zum Fall (Konzept 03, „Zielart ohne Tank und Heiler").
  Offen: Schaden an Begleitern gewichtet nach ihrem Bedarf über das Brett (K11; heute zählt er in LP wie deiner),
  Stärkung anderer. *Was gilt:* Zuerst fällt der Gegner mit der größten Gefahr je
  Sekunde bis zu seinem Fall: seine Gefahr je Sekunde geteilt durch die Zeit, bis er fällt. Diese Reihenfolge macht die
  Summe des Schadens über den Kampf am kleinsten (Regel von Smith, Ablaufplanung); „wenigste LP zuerst" ist ihr Sonderfall
  bei gleicher Gefahr aller Gegner. *Gefahr:* sein Schaden an dir und an deinen Begleitern, je als Anteil an den LP, die
  der Getroffene für den Rest des Bretts braucht (dein Lauf, ihre Einsätze nach K11) – Schaden an einem Begleiter, der
  später unersetzlich ist, wiegt so mehr als an einem, der nicht mehr gebraucht wird; dazu, was er für andere tut: geheilte
  LP (Blütenkaktor, Biloko, Kupoheiler), die LP eines zurückgeholten Gegners (Raise der Lamia), der Mehrschaden der
  Gestärkten (Sukkubus-Magierin, Jüngerer Taurus, Melomog), die Wirkung beim Erreichen des Verbündeten (Irrlichter, die zum
  Oger laufen). *Zeit bis zum Fall:* seine LP durch deinen Schaden auf ihm, mit dem Zuschlag seiner Schwäche (K1). *Start
  und Nachsteuern:* vor der ersten Messung die Werte des Blatts (Stärke, Intelligenz, Konstitution, Aktionen), dann der
  gemessene Schaden je Gegner und Sekunde (`hit you` je Quelle, Treffer auf Begleiter) und die korrigierte Restzeit.
  *Im Kampf:* Atomos-Kampf mit acht Gegnern: Pudding, Creme und Fran (Fire III, Water III, Thunder III) vor den Gremlins, auch wenn
  ein Gremlin weniger LP hat; Medusa-Kampf: die Lamia, sobald sie Raise wirkt; Oger-Kampf: die Irrlichter, bevor sie den
  Oger erreichen. *Gegenthese 1:* Zu Kampfbeginn fehlen Messungen. Widerlegt: Das Blatt gibt Werte je Gegner, die Messung
  ersetzt sie nach den ersten Treffern. *Gegenthese 2:* Zielwechsel kosten Laufwege und Rhythmus. Teilweise: Gewechselt
  wird nur, wenn die neue Reihenfolge den Unterschied über die Messunsicherheit hinaus zeigt; die Ausweichbewegung bleibt
  bei BossModReborn. *Gegenthese 3:* Endet ein Kampf mit dem Fall des Hauptgegners, wäre er zuerst richtig. Offen: In
  seinen Läufen fielen die Gerufenen immer vor dem Hauptgegner (04.11.2026); die Spur hält fest, ob beim Fall des
  Hauptgegners noch Gegner leben und ob der Kampf dann endet. *Gegenthese 4 (sein Grund zu A525, „weniger gegner machen
  weniger schaden"):* trägt die Regel – sie rechnet genau diesen Schaden, statt ihn über die LP zu schätzen.
  *Falsifikation:* Kein Unterschied zu „wenigste LP"? Im Atomos-, Medusa- und Oger-Kampf doch. Option falsch? Eine
  verrauschte Messung ließe das Ziel springen – der Wechsel verlangt einen Unterschied über ihrer Unsicherheit. Nichts
  ändert sich? Im Crucible überschreibt heute A525 die Zielart; die neue Regel tritt dort an ihre Stelle, die Spur nennt
  jeden Wechsel mit Grund.
- *K4 – Unterbrechen* (A583). Unterbrochen wird über Borrow: Bei einem Seelen-Begleiter wird Bestienmodus zu Soul Crush,
  „Interrupts the use of a target's action", bei Erfolg 1.000 statt 300 Potenz und 100 TP (Wirktext 44902, xivapi v2).
  Seelen-Begleiter sind die vier, die `XBMPet.InflictsInterrupt` markiert: Dullahan, Golem, Spriggan, Eisgolem
  (ffxiv-datamining `XBMPet.csv`, 05.11.2026); eorzeantavern (Dullahan) und icy-veins (Spriggan, Golem, Eisgolem) nennen
  Soul Crush als den Unterbrecher des Bretts (05.11.2026, Gemeinschaftsquellen). Ihre Dressuren und Entfesseln
  unterbrechen nicht; ihre Texte schweigen dazu (A557).
  - *Gebaut:* Soul Crush geht zuerst auf einen Gegner, dessen Zauber das Spiel als unterbrechbar meldet, sonst wie bisher
    (W4, A486).
  - *Welche Kämpfe, gebaut (A587):* Kein Blatt sagt es; das Spiel meldet es mit dem Zauber (`IsCastInterruptible`). RSR
    merkt sich im Brett jede Gegnerart (BNpcName), die unterbrechbar zaubert, über alle Sitzungen
    (`InterruptibleCasterKinds.json`, wie die schlafresistenten Arten: Zählung, nur steigend; eine Zeile beim ersten Mal je
    Kampf). BossModReborn (27be42256, 05.11.2026, Werkzeugquelle) führt in seinen Crucible-Modulen: Verknöchern des
    Knochen-Ritters und Blutschwert der Seelenlose („Heals caster on hit") auf Brett 1, Dreadwash (Ymir) auf Brett 3,
    Confound (Administrator: „you will get confused for 12 seconds and most likely die") auf Brett 4, Raise der Lamia und
    die Zauber der Schleime (Gigantis) auf Brett 5. RSR übernimmt die Liste nicht, es lernt sie im ersten Lauf.
  - *Hornfenster (K6):* Nennt das Blatt des anstehenden Kampfes eine gelernte Art, zählt ein Seelen-Begleiter auf einem
    Horn, das One with Nature für Borrow verbraucht, als Unterbrecher: in der Wahl nach den verlangten heilenden oder
    schützenden und dem passenden Typ, vor dem Schaden. Grund: Ein gestoppter Confound erspart die Verwirrung, ein
    gestopptes Raise eine zurückgeholte Lamia, ein gestopptes Verknöchern den Schadensbonus des Ritters.
  - *Kader (K7):* Führt ein Kampf des Bretts eine gelernte Art, nimmt die Empfehlung nach den Begleitern für den Boss
    einen Seelen-Begleiter auf, sofern er freigeschaltet ist.
  - *Gegenthese 1:* Die Seelen-Art bringt sonst wenig (Borrow statt Entfesseln). Teilweise: Gewählt wird er nur für
    Kämpfe mit gelernten Arten, und nur einer; das Horn mit Borrow ist ohnehin eingestellt (ab Werk das dritte).
  - *Gegenthese 2:* Ein Zauber, der nur manchmal unterbrechbar ist, macht die Art dauerhaft zum Grund. Nicht widerlegt;
    der Preis ist ein Seelen-Begleiter statt eines stärkeren Schadensbegleiters in diesen Kämpfen.
  - *Ausgeliefert, und nichts ändert sich?* Vor dem ersten Kampf gegen eine Art kennt RSR sie nicht; ohne
    Seelen-Begleiter im Bestiarium bleibt die Wahl, wie sie war. Sein Kader vom 05.11.2026 hat keinen.
  - *Sonde (A557):* Meldet der Server im Brett den Abbruch eines gegnerischen Casts (ActorControl 15), schreibt RSR die
    letzte eigene Aktion auf diesen Gegner mit den Effektarten ihrer Einträge und dem Abstand in die Spur. Am 05.11.2026
    vier Abbrüche, keiner durch eine Unterbrechung: drei mit viertem Wert 0, einer (Pisco-Dämonen-Ebenbild, viertem Wert 1)
    0,05 s vor dem Ende des Kampfes, also sein Tod; die Effektarten 3, 62 und 27 seines Schildspalters sind Schaden, ein
    in ECommons unbenannter Typ und „StartActionCombo" (ECommons 74af164). Die Effektart „Interrupt" führt ECommons als
    75, BossModReborn als 76; welche gilt, kann sein Spielprogramm (`ffxiv_dx11.exe`, Upload 05.11.2026) entscheiden,
    sobald eine Regel sie liest.
- *K5 – Zustände* (A589). *Daten:* Jeder Gegner eines Kampfes verweist auf eine Zeile von `BNpcResist`
  (`XBMBattleDetail.Resist`, Lumina `Unknown4`); sie sagt je Zustand, ob er ihn abwehrt – in der Reihenfolge von EXDSchema
  Slow, Petrify, Paralysis, Interrupt, Blind, Poison, Stun, Sleep, Bind, Heavy, Death (Lumina `Unknown0` bis `Unknown10`;
  ffxiv-datamining `BNpcResist.csv`, 05.11.2026). Welche Zustände ein Begleiter setzt, sagt `XBMPet` (`InflictsSlow` bis
  `InflictsDeath`, dieselbe Reihenfolge). Über alle 122 Gegner der Bretter: offen für Slow 54, Poison 53, Death 42, Heavy
  41, Stun 40, Blind 33, Bind 31, Petrify 23, Paralysis 22, Sleep 18, Interrupt 12; die Endgegner und die meisten
  Elitegegner wehren alles ab außer Poison, Blind oder Heavy.
  - *Gebaut, Stufe 1 – ein stoppendes Entfesseln zählt nur gegen Anfällige:* Ein Entfesseln, das stoppt (C1: Stun, oder
    Petrification und Deep Freeze, beide als Petrify geführt – das Blatt markiert die Chimäre, deren Entfesseln „deeply
    freezes", als `InflictsPetrify`), gilt im Crucible weder als Schutz vor einem Rudel noch als passend (K6), wenn ein
    Gegner des Kampfes den Zustand laut Blatt abwehrt, und wird beim Ansturm nicht vorgezogen (C1), wenn einer der
    Angreifer ihn abwehrt. *Im Kampf:* Auf Brett 1 sind nur Milbenbrut und Sukkubus-Ritterin für Stun offen. Bis A589
    zählte K6 den Büffel vor jedem Rudel als schützend und passend, und C1 zog ihn beim Ansturm vor, bis `StunWatch` die
    Immunität nach dem ersten Versuch gelernt hatte – gegen Knochen-Ritter und -Bischof, Irrlichter und Pisco-Dämon
    verpufft sein War Cry (aus Code und Blatt abgeleitet; in seinen Mitschnitten kam der Büffel auf Brett 1 nicht vor).
    Jetzt kommt an seiner Stelle ein Begleiter, der heilt oder schützt, oder der mit dem meisten Schaden.
    *Gegenthese 1:* Ein Gegner fehlt im Blatt (Zufallsfeld). Dann gilt wie bisher die gelernte Immunität. *Gegenthese 2:*
    Das Blatt irrt. Die gelernte Immunität bleibt daneben; ein Gegner, den das Blatt offen nennt und der doch abwehrt,
    fällt nach dem ersten Versuch heraus wie bisher.
  - *Gebaut, Stufe 2 (A644) – gesenkte Treffsicherheit zählt als Schutz, wo sie beziffert ist:* Zwei Entfesseln nennen
    eine Zahl: Sticky Tongue („Reduces target's accuracy by 10%", 44996) und Ultrasonics („Reduces targets' accuracy by
    10%", 44972; ActionTransient, xivapi v2, 07.11.2026); `XBMPet` führt beide Begleiter als `InflictsBlind`. Ein solches
    Entfesseln zählt in B2, B5 und im Hornfenster (K6) wie eines, das schützt – die getroffenen Gegner treffen laut
    Wirktext um 10 % schlechter, eine Größe wie die der Schutzentfesseln (Water Wall: magische Verwundbarkeit −20 %) –,
    im Crucible aber
    nur, wenn kein Gegner des Kampfes (vor ihm) bzw. keiner der kämpfenden Gegner (in ihm) laut `BNpcResist` Blind abwehrt.
    Außerhalb des Crucible kennt RSR keine Abwehr und zählt es.
    - *Im Kampf:* Vor einem Rudel aus Gegnern, die Blind nicht abwehren, und einer Gefahr im Gebiet setzt das Hornfenster
      einen Begleiter mit Ultrasonics als den verlangten schützenden, wenn keiner da ist, der heilt, schützt oder stoppt;
      bisher zählte er nur mit seinem Schaden, und das Horn blieb ohne Schutz.
  - *Nicht gebaut, mit Grund:* Slow (Regurgitate, Bad Breath, Formic Pheromones), Paralysis (Pins and Nails, Charged
    Whisker, Bad Breath) und Blind ohne Zahl beziffert kein Text („Afflicts targets with Slow"; die Statustexte 9, 15, 17
    nennen keine Größe). Als Schutz gezählt, könnten sie einen Begleiter vom Horn drängen, der nachweislich heilt oder
    schützt – im Kampf ein Verlust an Sicherheit, den kein Beleg aufwiegt; als Schaden zählen sie weiter. Formic Pheromones
    mindert zwar Wasser, Erde und Eis um 15 %, zählt aber nach C2 nicht: Eine Minderung, die Elemente nennt, hält
    physischen Schaden nicht ab. Sleep endet am nächsten
    Schaden, den die Flächen des Beastmaster sofort setzen; Death („may doom", „Occasionally reduces enemy HP to a single
    digit") ist ein zufälliger Fall des Gegners, also Schaden. Poison, Heavy und Bind schützen nicht: Gift ist Schaden,
    Gewicht und Fessel halten nur die Bewegung.
  - *Gegenthese (ausgeliefert, und nichts ändert sich):* Ohne einen der beiden Begleiter im Kader, auf einem Horn ohne
    Entfesseln oder gegen Gegner, die Blind abwehren – die Endgegner wehren fast alles ab außer Poison, Blind oder Heavy,
    dort zählt es also.
- *K6 – Hörner vor dem Kampf belegen.* B5 setzt die Hörner zwischen den Kämpfen per Serverbefehl, im Crucible bisher
  nicht. Im Brett öffnet das Spiel vor jedem Kampf das Hornfenster (`XBMPetParty`, `AgentXBMPetParty.Mode` 2); es
  behält die Hörner nicht zwischen den Kämpfen und füllt sie selbst nur bei höchstens drei im Team (Daedalus 74659a6,
  03.11.2026, `CrucibleHornSetter`, Werkzeugquelle aus dem Spielbetrieb; Wiki). Dort sind der Kader mit LP je Platz
  (`SelectedPets`, `PetCurrentHealth`), die gewählten Plätze (`SelectedPetIds`) und der anstehende Kampf
  (`AgentXBMStageDetailList`, Eintrag der Art 3) lesbar; gesetzt wird mit dem Rückruf des Fensters, ein Klick je
  Platz, nach jedem Klick neu gelesen, ohne den Kampf zu starten. Belegung nach K1 und K2 mit einem Heiler, wenn die
  Gefahr im Brett das verlangt. Die Sonde (A556, A561) schreibt eine Zeile, sobald das Hornfenster aufgeht.
- *K7 – Kader vor dem Brett* (Loop vom 05.11.2026; seine Frage, ob RSR „auf wunsch begleiter auswählen" kann; seine
  Vorgabe: das Spielerlebnis vor der Schwierigkeit der Umsetzung). *Optionen:* A nichts; B die Empfehlung zeigen, sobald
  das Teamfenster offen ist; C dazu ein Knopf „übernehmen", der sie auf deinen Wunsch setzt; D ungefragt setzen.
  *Im Kampf und davor:* Unter A und B wählst du 10 bis 15 Begleiter selbst und weißt nicht ohne Nachschlagen, welche Schwäche
  das Brett verlangt; unter C ein Klick; unter D überschreibt RSR einen Kader, den du anders geplant hast. *Gegenthese 1
  (Absturz):* BeastHelper dokumentiert einen Absturz durch nachgebaute Fensterereignisse mit leerem Ereigniszeiger.
  Teilweise widerlegt: C nutzt die Spielfunktionen, die das Fenster selbst ruft (`AgentXBMPetParty.TogglePet`,
  `ConfirmSelection`), prüft nach jedem Schritt die Auswahl (`SelectedPets`) und hört bei einer Abweichung auf; ob sie aus
  einem Plugin heraus tragen, belegt Daedalus im Spielbetrieb (`CrucibleRosterLoader`, 74659a6): Sie tragen, ein
  Begleiter kann abgelehnt werden („TogglePet(14) refused", 25.10.2026), dann geht es ohne ihn weiter; die Spur nennt
  jeden Schritt. *Gegenthese 2:* Das Brett hat
  Abzweigungen und Zufallsfelder. Widerlegt: Das Blatt nennt alle Kämpfe des Bretts, Zufallsfelder ziehen aus ihnen.
  *Gegenthese 3:* Ein Begleiter unter dem Rang des Bretts ist schwächer. Eingearbeitet: Rang mindestens wie das Brett
  synchronisiert (`GetSyncedRank`, `IsPetOverSyncedRank`), sonst nachrangig. *Gegenthese 4 (Aufwand):* nach seiner
  Vorgabe kein Grund. *Ergebnis:* C, entschieden (seine Frage zeigt den Wunsch, „auf Wunsch" schließt D aus): Wahl nach
  Abdeckung der Schwächen aller Kämpfe des Bretts, gewichtet nach Zahl der Gegner, dazu ein Heiler und, wo Gegner
  unterbrechbar wirken, einer, der unterbricht.
- *K8 – Gegenstände* (gebaut: A551 bis A555, unten „Stufe 3: K8"). Über `InstanceContentCrucibleOfTheUnbroken.UseItem(Platz, Bestie)` auf dein Ziel. Sicherheit zuerst
  (seine Regel): Bestien-Heiltrank oder -Pulver, wenn deine LP in die Gefahr fallen und keine Heilung bereit ist;
  Bestien-Gegengift und Bestien-Augentropfen gegen den passenden Zustand; Bestien-Verhärtungsmittel vor einem angekündigten
  Tankbuster; Bestien-Lebensfeder oder Sand der Zeit außerhalb des Kampfes vor dem Endgegner. Schaden: der Reißzahn der passenden Schwäche auf die
  gerufenen Gegner (K2) oder ein Rudel – so hat er es am 04.11.2026 um 19:04 selbst getan. *Gegenthese:* Gegenstände
  sind ein Vorrat für das ganze Brett. Teilweise: Sicherheitsgegenstände gehen nur in der Gefahr; Schadensgegenstände
  nur auf gerufene Gegner oder vor dem Endgegner. Der Punktezuschlag ohne Gegenstände steht dagegen: Option.
- *K9 – Feindseligkeit (Zähnefletschen, Provozieren).* Gebaut (A550; oben, „Tanken"): ab Werk an, Provozieren dazu am
  größten Treffer des Kampfs. Neu belegt (Wiki): Hält der Begleiter die Feindseligkeit, trägt er
  die Schwächungen vermeidbarer Flächen und nimmt aus ihnen keinen Schaden. Zähnefletschen schützt dich also auch dort,
  wo du nicht ausweichst. *Gegenthese:* Seine LP bleiben über das Brett stehen, ein gefallener fehlt. Teilweise:
  Provozieren vor seinem Fall (am größten bisher gesehenen Treffer, nicht an festen 30 %) hält ihn; die Campsite heilt
  dich allein am meisten (gemessen +30 % bei geteilter Rast), also kostet verteilter Schaden dort Heilung. Empfehlung:
  die Einstellung ab Werk an, auf dem letzten Horn, mit Provozieren am größten Treffer; Loop bei der Umsetzung.
- *K10 – Zwischen den Kämpfen (Campsite, Laden, Weg, Rauchbombe):* bisher als „nicht RSR" ausgenommen, mit dem Grund,
  dass andere Werkzeuge es führen. Nach seiner Vorgabe vom 05.11.2026 (Spielerlebnis vor Umsetzung) ist das kein Grund
  mehr.
  - *Campsite, gebaut (A564):* Das Campsite-Fenster ist der Begleiter-Agent im Modus 4 mit sichtbarem `XBMPetParty`
    (Daedalus 74659a6, `CrucibleAutoPilot.Rest`, im Spielbetrieb 25. und 27.10.2026; FFXIVClientStructs aad0de6 nennt
    Modus 4 „buy feed result" – darum prüft RSR nach jedem Klick die Auswahl). Die Liste zeigt den Kader in
    Bildschirmfolge, `SelectedPetIds` die gewählten Plätze, ein Rückruf `[1, Platz]` schaltet einen Platz um.
    *Mechanik:* Die Rast teilt ihre Heilung, je Teilnehmer ein Anteil der eigenen maximalen LP: allein 90 %, mit einem
    Begleiter je 45 %, mit zweien je 30 % (Wiki; 45 % zeigt die Bestätigung „Rest and recover 45%…" in Daedalus'
    Feldnotiz, +30 % deiner LP in deiner Messung vom 04.11.2026). Für drei und vier folgt aus derselben Teilung 22,5
    und 18 % – meine Ableitung, unbelegt. Wie viele mitruhen dürfen, legt das Lager fest (`XBMContentCamp`, Zeile als
    Brett gelesen, Unterzeile je Lager, 1 bis 4: erstes Brett 2, 2; zweites 2, 3; drittes 3, 3; Erstes Meisterbrett 2, 4;
    Zweites 1, 2, 3). Ein ruhender Begleiter verliert sein Futter („Familiars that rest will lose current effects of
    feed", Addon 17657; „G2 Primafodder" gibt +30 % maximale LP, „Honey Simular" −25 % physische Verwundbarkeit,
    `XBMItem` Typ 3), ein gefallener wird nicht geheilt.
    *Was gilt:* RSR wählt vor, ruhen (`[3]`) oder gehen drückt er. Begleiter ruhen nur mit, wenn du trotzdem voll
    herauskommst – du kommst nie schlechter heraus als allein. Unter dieser Bedingung die Zahl, die den Begleitern die
    meisten LP bringt (je Begleiter höchstens, was ihm fehlt), und dafür die stehenden, nicht vollen, ungefütterten mit
    dem kleinsten LP-Anteil; bei Gleichstand weniger. Sonst ruhst du allein. Die Grenze des Lagers lernt RSR am Klick,
    den das Fenster nicht annimmt (oder mit dem es einen anderen abwählt); den Anteil liest es aus der Bestätigung,
    sobald sie offen ist, und rechnet ab dann mit ihm (ohne Lesung 90 % geteilt). Futter liest es aus den Werten des
    Fensters (`ReaderXBMPetParty.FedCurrent`, ECommons, Werkzeugquelle; Daedalus liest dieselbe Stelle); stimmen deren
    LP nicht mit denen des Agenten überein, wählt RSR nichts. Beim Öffnen springt die Auswahl kurz auf den ganzen Kader
    (Daedalus, im Spielbetrieb); RSR handelt erst, wenn sie einen GCD lang steht. Ändert er sie danach von Hand, bleibt
    sie für dieses Lager. Einstellung „Pick who rests with you at a Crucible campsite (Crucible only)", ab Werk an.
    Das Fenster „RSR - Crucible campsite" zeigt diese Vorauswahl mit Grund; an der letzten Campsite vor dem Endkampf
    empfiehlt es dazu, wer für ihn mitruht (K14, A631, sein Vorschlag vom 06.11.2026). Die Grenze des Lagers steht im
    Blatt (`XBMContentCamp`, Unterzeile = Index des Lagers auf dem Brett, K14), gelernt wird nur noch eine kleinere.
    *Beispiel:* Du bei 75 %, Fledermaus 20 %, Fliegenfalle 40 %, Schleim 90 %, Lager für zwei: Mit zweien bekommst du
    30 % (voll) und die beiden ersten je 30 %, zusammen 60 % für die Begleiter; mit einer nur 45 %. RSR wählt Fledermaus
    und Fliegenfalle. Du bei 40 %: Schon mit einem kämst du nur auf 85 % statt voll – du ruhst allein.
    *Verworfen:* (a) „Use HP potions below this HP" als Grenze (meine erste Fassung): Bei 13 % brächte dich eine Rast zu
    zweit auf 58 %, über jede übliche Linie – genau Daedalus' Feldfall vom 25.10.2026 (Rast mit dem Goobbue bei 13 %,
    Tod im nächsten Kampf). (b) Linie plus größter bisheriger Verlust je Kampf: Vor dem Endgegner unterschätzt sie den
    nächsten Kampf, und welches Feld folgt, kennt RSR an der Campsite nicht; sie weicht von „voll" nur ab, wo du schon
    niedrig stehst. (c) Daedalus' feste 50 %: eine Zahl ohne Herleitung, nur ein Begleiter, und zwischen 50 und 55 %
    kämst du nicht voll heraus.
    *Gegenthese 1:* Bei 54 % und einem Begleiter auf 5 % ruhst du allein; du gewönnest 45 % für ihn gegen 1 % für dich.
    Nicht widerlegt, Preis benannt: Er geht mit 5 % in den nächsten Kampf und fällt am ersten Treffer, bis zum Horn der
    Rückkehr; K8 (Bestien-Heiltrank auf ihn) und K9 (Provozieren am größten Treffer) bleiben, und mit einem Klick nimmt
    er ihn dazu. Dagegen steht: Deine LP sind der Lauf. *Gegenthese 2:* Der am meisten verletzte ist nicht unbedingt der,
    den ein späterer Kampf braucht – das beantwortet erst K11 mit den Sondenzeilen. *Gegenthese 3:* Ein gefütterter
    Begleiter mit wenig LP bleibt draußen, obwohl die Rast ihn retten könnte. Nicht widerlegt: Ob die LP das gekaufte
    Futter wert sind, hängt am Futter; er entscheidet mit einem Klick.
  - *Laden, gebaut (A569):* Das Ladenfenster `XBMContentsItemShop` führt deine Marken, je Angebot gezeigt,
    `XBMItem`-Zeile, Preis, ermäßigt und verkauft, dann deine Gegenstände und deine Ausrüstung (ECommons
    `ReaderXBMContentsItemShop`, Werkzeugquelle; Daedalus 74659a6 liest dieselben Stellen, Aufnahme 22.10.2026, und kauft
    Zeile r mit dem Rückruf `[2, r]`). Gegenstände hältst du höchstens zehn (`InstanceContentCrucibleOfTheUnbroken`,
    zehn Plätze), Ausrüstung ebenso (zehn Plätze im Fenster; Daedalus: das Spiel lehnt ein elftes Stück ab).
    Ausrüstung wirkt ohne Zutun bis zum Ende des Laufs, Gegenstände verbraucht K8. Die Art steht im Blatt
    (`XBMItemType`: „Beast Gear", „Crucible Item", Futter).
    *Marken und Preise* stehen als Text im Fenster, mit dem Tausendertrenner der Client-Sprache: „1.421" im deutschen,
    „1,421" im englischen Client. RSR liest die Ziffern der ersten Zahl, gleich welcher Trenner zwischen ihren
    Dreiergruppen steht (A657). Grund: ECommons' `FirstNumber` kennt nur das Komma und las „1.421" als 1; im deutschen
    Client sah RSR ab 1000 Marken eine einzige und meldete „Nothing here adds safety within your tokens and slots" (sein
    Bild vom 07.11.2026: 1421 Marken im Spiel, „1 tokens" im Fenster). Alle übrigen Zahlen, die RSR aus Text liest,
    stammen aus den englischen Blättern.
    *Was du trägst, steht nicht zur Wahl:* Ein Laden listet auch Ausrüstung, die du schon trägst, und das Spiel lehnt
    ihren Kauf ab („Du besitzt die Mithril-Rüstung bereits und kannst den Gegenstand deshalb nicht erwerben", sein Bild
    vom 07.11.2026). Ein Angebot, dessen `XBMItem`-Zeile unter deiner Ausrüstung steht, fällt deshalb aus Kaufliste und
    Schatz-Skala (A657). Grund: Vorher rechnete die Liste es als zweites Stück auf das erste (erlittener Schaden −15 %
    zweimal) und empfahl die Mithril-Rüstung für 858 Marken, die du trugst; „Buy" endete an der abgelehnten Zeile.
    *Was gilt:* Solange der Laden offen ist, zeigt das Fenster „RSR - Crucible shop" eine Kaufliste mit Preis und Grund
    je Posten; „Buy" kauft sie Zeile für Zeile, geprüft am Verkauft-Merker. Fragt das Spiel nach, wartet RSR auf deine
    Antwort: nach Ja die nächste Zeile, nach Nein endet der Kauf; ebenso, wenn das Spiel eine Zeile nicht annimmt (ob der
    Kauf nachfragt, ist unbelegt – BeastHelper be40a1e beantwortet eine Kaufrückfrage, „noch nicht im Spiel bestätigt").
    Ohne den Klick kauft RSR nichts (wie K7, Entscheidung C in A533: ein Schritt, den keiner zurücknimmt, nur auf deinen
    Wunsch). Das Fenster steht, solange der Laden offen ist, auch wenn die Liste leer ist, mit dem Grund; jede neue Liste
    geht ins `DefenseTrace.log`. Die Liste, Sicherheit zuerst, solange Marken und Plätze reichen:
    1. Ein zweites Leben, solange du keines hältst: Ring of Sacrifice oder ein Gegenstand mit Reraise.
    2. Gegen jeden Zustand der Kämpfe des Bretts, der mit dem Tod endet (Wirktext nennt KO oder den Tod: Verhängnis,
       Steinfluch – Status 5421, 437), eine Resistenz oder ein Serum, sofern nichts Gehaltenes ihn deckt. Grund: Gegen
       ihn helfen weder LP noch Minderung.
    3. Schutz- und Heilausrüstung auf einer Skala: die LP, mit denen du aus einer Strecke von Kämpfen kommst, die mit
       dem schwersten endet – L = 1 − (n − 1) × max(0, D − h − c) − max(0, Dmax − h), als Anteil deiner maximalen LP.
       *D* und *Dmax* sind dein Schaden in einem Kampf, im Mittel und im schwersten, mit der Ausrüstung:
       (1 − erlittener Schaden) × (P × (1 − physische Verwundbarkeit) × (1 − Ausweichen) + M × (1 − magische
       Verwundbarkeit) + S) / (1 + maximale LP), über alle Stücke multipliziert. P, M und S sind dein gemessener
       physischer, magischer und sonstiger Schaden je beendetem Kampf, nach der Angriffsart der treffenden Aktion
       (`Action.AttackType`: Hieb bis Schuss physisch, Magie magisch, alles andere sonstig – dieselbe Zuordnung wie
       Tankbuster und K8), zurückgerechnet auf die Zeit ohne deine gehaltene Ausrüstung, in diesem Gebiet, sonst im
       Lauf. Ohne Messung kostet ein Kampf eine volle Leiste, geteilt nach Stärke und Intelligenz der Gegner des Bretts
       im Blatt (meine Zuordnung: Stärke trifft physisch, Intelligenz magisch; ohne Brett alle Kämpfe des Crucible).
       *h* ist deine eigene Heilung auf dich je Kampf (deine Aktionen und Gegenstände), gemessen wie D, mal 1 plus der
       Mehrheilung der Ausrüstung, geteilt durch ihren LP-Faktor; ohne Messung 0, dann zählt Mehrheilung nichts. *c*
       sind die LP am Kampfende (Ring of Curing). *n* sind die Kämpfe zwischen zwei Lagern, im Lauf gemessen (Kämpfe
       vom Betreten des Bretts oder vom letzten Lagerfenster bis zum nächsten), bis zur ersten Messung 1 – dann zählt
       nur der schwerste Kampf, und Schutz geht vor Heilung am Kampfende. RSR wählt die Auswahl mit dem größten L; bringt
       kein Stück mehr L, zählen die wirksamen LP (1 + maximale LP) / ((1 − erlittener Schaden) × (P' × … + S')) mit
       den Anteilen P', M', S' – Schutz gegen den Kampf, der schwerer wird als jeder gemessene, etwa den Endkampf.
       Ausweichen zählt als Chance, einem physischen Treffer zu entgehen, gestapelte Stücke multiplizieren (beides
       meine Ableitung, unbelegt). Nicht gezählt: Blockstärke (wie oft du blockst, ist unbekannt) und Zufallswirkungen
       (Blink, Steinhaut, Rehabilitation, Aufsaugen) – die Liste unterschätzt diese Stücke, sie überschätzt keines.
       Gesucht wird über jede Startwahl: je Angebot einmal als erstes gesetzt, danach Stück für Stück das mit dem
       größten Gewinn je Marke; von allen so gefundenen Auswahlen gilt die beste, bei Gleichstand die billigere. Die
       Zeile nennt, was das Stück an Bewegung oder Aktionstempo kostet (Genji Armor: Bewegung −20 %; Ice Shield: Slow
       +10 %), gegengerechnet mit deiner übrigen Ausrüstung (Mystic Boots: Bewegung +50 %).
    4. Gegen jeden übrigen Zustand der Kämpfe des Bretts, der dich die Sicherheit kostet – er nimmt dir die Kontrolle,
       verhindert Aktionen, auch nur manchmal (Paralyse, Status 5382 und 5388: „sometimes preventing the execution of
       actions"), oder fügt Schaden über Zeit zu (Gift, 5140 und 5141) –, eine Resistenz oder ein Serum, sofern nichts
       Gehaltenes ihn deckt. Blind (5381, 5389) nicht: Es senkt nur deine Trefferquote. Daedalus 74659a6 führt dieselben
       Zustände je Brett aus einem Brettführer (zweites Brett Verhängnis; drittes Paralyse; Erstes Meisterbrett Gift und
       Paralyse, „poison resistance trivializes it"; Zweites Meisterbrett Paralyse, Verhängnis, Stein,
       „anti-paralysis mandatory"); die Blätter geben für jedes Brett mindestens diese.
    5. Heiltränke, der stärkste zuerst, bei Gleichstand der billigere.
    6. Schutzgegenstände: Minderung nach ihrem Anteil, Abwehr physischer, Reflexion magischer Angriffe.
    Nie Futter in dieser Liste (die Rast beendet es, Campsite oben); Futter für den Endkampf empfiehlt das Fenster
    getrennt darunter, für die freien Futterplätze der Begleiter des Endkampfplans (K14, A631, A657, sein Vorschlag vom
    06.11.2026); Ausrüstung, die dir selbst einen Zustand zufügt (Thunder Axe:
    Betäubung, Demonic Helm: Verhängnis, Flame Knife: Chains of Condemnation, Wind Mask: Keen Edge, Schaden über Zeit auf
    dir) – wie K8 nie einen Gegenstand nimmt, der dir einen Zustand zufügt. Ausrüstung, die nichts davon hebt, fällt
    heraus (Empress Hairpin senkt die maximalen LP). Schaden empfiehlt die Liste nicht; was du dazu kaufst, nutzt K8.
    Einstellung „Recommend what to buy in a Crucible shop (Crucible only)", ab Werk an; sie zeigt das Fenster, kaufen
    tut der Knopf.
    *Belege der Skala:* Daedalus 74659a6 im Feld: „Every Crucible death so far was HP attrition (no healing, HP carried
    between battles)"; „Your HP does not refill between a board's battles and your death ends the board" (v0.1.104).
    Seine Messung vom 04.11.2026 (K11) zeigt dasselbe an den Begleitern.
    *Beispiel (Preise angenommen):* 900 Marken, Ring of Curing (600, 25 % am Kampfende) und Mythril Armor (600, −15 %
    erlittener Schaden, +15 % LP) im Laden, gemessen D = 30 %, Dmax = 50 % je Kampf, keine eigene Heilung, n = 3. Ohne
    beides: L = 1 − 2 × 0,30 − 0,50 = −0,10, du fielst im dritten Kampf. Ring: 1 − 2 × 0,05 − 0,50 = 0,40. Rüstung:
    D 0,22, Dmax 0,37: 1 − 2 × 0,22 − 0,37 = 0,19. RSR empfiehlt den Ring. Mit n = 1 (vor jedem Kampf ein Lager): Ring
    0,50, Rüstung 0,63 – die Rüstung. Ein Brett, dessen Endkampf Verhängnis zufügt, und Soulreaper's Armor im Laden:
    Sie kommt vor beidem (Schritt 2).
    *Verworfen:* (a) ungefragt kaufen (Daedalus' Autopilot): Marken sind auch Punkte (Coin Clutcher, Rolling in Riches),
    und was du sparst, entscheidest du. (b) Nur anzeigen ohne Knopf: Jede Zeile wäre ein eigener Klick und eine Rückfrage
    mehr; der Knopf kostet nichts, solange er nur auf Wunsch kauft. (c) Schaden mitempfehlen: Sicherheit zuerst; Schaden
    verkürzt die Kämpfe, aber um wie viel, hängt an deinem Anteil am Schaden der Begleiter – unbekannt. (d) Schutz vor
    Heilung am Kampfende, nach wirksamen LP allein (meine erste Fassung, A569): Bei mittlerem Schaden je Kampf rettet der
    Ring mehr LP, wie das Beispiel zeigt; die wirksamen LP sehen nur den einzelnen Treffer. (e) Daedalus' feste
    Gewichte je Wert (Maximum HP 1,0, Damage Taken 1,5, Ring of Curing 1,2 je Prozent): Zahlen ohne Herleitung, blind
    für den gemessenen Schaden. (f) Nur Zustände, die Kontrolle nehmen oder töten (meine erste Fassung): Paralyse und
    Gift fehlten, die Daedalus' Brettführer für drei Bretter verlangt.
    *Gegenthese 1:* Genji Armor (+30 % LP, −20 % erlittener Schaden) verlangsamt dich um 20 %; eine Fläche, der du
    sonst entkämst, trifft dich. Widerlegt bis auf einen Fall: Der Weg aus der Fläche dauert ein Viertel länger; das
    ändert nur etwas, wenn du ihn mit weniger als einem Fünftel der Wegzeit Vorsprung geschafft hättest. Auch dann
    trifft dich der Treffer mit 80 % Schaden auf 130 % LP, also mit 62 % seines Anteils ohne die Rüstung, während sie
    jeden anderen Treffer ebenso mindert. Den Ausgang kehrt sie nur bei einer Fläche um, deren Schaden ohne die Rüstung
    mehr als das 1,6-Fache deiner LP beträgt (eine Fläche, die töten soll), und nur bei spätem Ausweichen (meine
    Rechnung; wie viele solche Flächen ein Brett führt, ist unbelegt). Die Zeile nennt die Verlangsamung, Mystic Boots
    heben sie auf. Ice Shield (Slow +10 %) ebenso: Deine Aktionen kommen um ein Zehntel langsamer (meine Lesart nach dem
    Gegenstück Haste im selben Blatt: Wirk- und Abklingzeit, Auto-Angriff), dein Schadenseingang sinkt um drei Zehntel.
    *Gegenthese 2:* Der Gewinn je Marke bevorzugt billige Stücke vor einem teuren besseren. Widerlegt für ein teures
    Stück: Die Suche setzt jedes Angebot einmal an den Anfang. Es bleibt der Fall zweier teurer Stücke gegen mehrere
    billige mit höherem Gewinn je Marke; dort liegt die Auswahl wenig unter der besten (Beispiel in wirksamen LP: zwei
    Stücke zu 500 Marken mit je +30 % gegen eines davon und zwei zu 200 mit je +13 %: 1,66 statt 1,69; meine Rechnung).
    *Gegenthese 3:* Gemessene Kämpfe sagen wenig über den Endkampf, der härter trifft. Teilweise widerlegt: Dmax ist der
    schwerste gemessene Kampf, und bringt kein Stück mehr L, entscheiden die wirksamen LP – Schutz gegen den Kampf über
    jedem gemessenen. Es bleibt: Liegt der Endkampf weit über Dmax, kann ein Ring of Curing vor einer Rüstung stehen,
    die dort mehr gerettet hätte; Preis: die Minderung des Rüstungsstücks im Endkampf. Wahrscheinlichkeit: nur bei
    knappen Marken und langen Strecken, wo der Ring auf den Strecken davor mehr LP rettet.
    *Gegenthese 4:* n aus früheren Strecken passt nicht zum Rest des Bretts. Teilweise widerlegt: Jedes Brett führt zwei
    oder drei Lager (`XBMContentCamp`: 2, 2, 2, 2, 3), die Strecken ähneln sich also; die Ausrüstung hält den ganzen
    Lauf, und für ihn zählt die übliche Strecke, nicht die laufende. Bis zur ersten Messung zählt n = 1 und damit Schutz
    vor Heilung am Kampfende. Es bleibt: Ist die nächste Strecke kürzer als die gemessenen, überschätzt RSR den Ring;
    Preis: die Minderung im schwersten Kampf dieser Strecke. Den Weg, den du nimmst, kennt RSR nicht; seit A631 kennt es
    das Brett und den letzten Kampf (K14).
  - *Weg:* Seine Vorgabe (06.11.2026): „und auf den brettern von crucible generell hinweise, welche wege man nehmen sollte,
    basierend auf gesundheit der begleiter, ausstattung, verbrauchsgegenständen, eigenen heilbedarf", „immer für den
    nächsten schritt", „die empfehlung soll sich am aktuellen stand des fortschritts ausrichten" und „den kann man halt
    nicht am anfang des brettes fürs ganze brett voraussehen, sondern für den oder die nächsten schritte. ähnlich der
    vorausplanung beim schach" (Präzisierungen): gerechnet wird ab dem Feld, auf dem du stehst, mit dem, was der Lauf bis
    dahin gebracht hat, einen oder wenige Züge voraus und nach jedem Feld neu – kein Plan fürs ganze Brett. Gebaut wird der Hinweis für die nächste Gabelung, nicht das Laufen: Das Brett wird gelaufen, nicht
    geklickt (Daedalus läuft es mit vnavmesh ab), und danach hat er nicht gefragt. Die Felder und Wege liest RSR seit A631
    aus den Blättern (K14, „Der Weg aus den Blättern").
    *Wo du stehst (A632):* Am Hornfenster das Feld des Kampfs (K14). Öffnet danach das Fenster einer Campsite, eines Ladens
    oder eines Schatzes, ist es das Feld dieser Art – oder ein Zufallsfeld, das sie werden kann – unter den nächsten Feldern.
    Gibt es dort keins, bleibt das letzte bekannte Feld. So folgt RSR dem Lauf Feld für Feld, ohne Position.
    *Was gilt (A632, A656; seine Vorgaben vom 07.11.2026 zu seinem Screenshot von Brett 1: „da wäre eine empfehlung
    hilfreich. eher links oder rechts gehen" und „solche empfehlungen gehören auch ingame rein"; seine Frage dazu: „aktuell
    bei fast vollen hp und keinen toten begleiter also eher die truhe als das camp. oder ist das camp besser?"):* Hat das
    aktuelle Feld mehr als einen Nachfolger, zeigt das Fenster „RSR - Crucible route" außerhalb des Kampfs je Abzweig
    seine Richtung und seine Felder bis dorthin, wo die Wege wieder zusammenlaufen („Left: fight, then campsite"), wo er
    dich an der nächsten Rast lässt, und empfiehlt einen – mit dem Grund gegen den anderen im Fenster.
    - *Wann:* erst, wenn das Feld, auf dem du stehst, erledigt ist – nach seinem Kampf oder auf einer Campsite, einem
      Laden, einem Schatz (seine Vorgabe, 07.11.2026, zu einem Screenshot am Hornfenster vor dem Elitegegner: „warum wird
      die empfehlung vor dem kampf gegeben. der kampf kann doch noch den zustand von mir und meinen begleitern
      beeinflussen, so dass ein anderer weg sinnvoller wäre"). Am Hornfenster steht das Feld des Kampfs schon fest
      (K14), der Kampf aber aus; gewählt wird der Weg erst danach, mit den LP danach. RSR merkt sich je Feld, ob seit
      dem Hornfenster ein Kampf lief und endete.
    - *Richtung:* aus dem gezeichneten Brett (`XBMContentStageEventMap`, Spalte 0 die Gitterspalte nach Osten, Spalte 1
      die Gitterzeile nach Süden, Zeilen der Art 1 das Feld selbst; Daedalus 2644cbe, 06.11.2026, liest sie so).
      Links, geradeaus oder rechts, gesehen vom Start zum Endgegner: das Vorzeichen des Kreuzprodukts der Brettrichtung
      (Feld des Starts zum Feld des Endgegners) mit dem Schritt vom aktuellen Feld zum Abzweig. Geprüft an seinem
      Screenshot vom 07.11.2026: Brett 1, nach Zug 1 links Gegner und Campsite, rechts Gegner und Schatz; nach dem
      Elitegegner links Schatz, rechts Campsite – so steht es im Plan. Auf allen fünf Brettern liegen die Abzweige
      jeder Gabelung in verschiedenen Spalten (Daedalus' Brettdaten aus dem Client, 31.10.2026; Werkzeugquelle). Die
      „vertauschten Seiten", deretwegen A632 nur das Feld nannte, betrafen die Reihenfolge der Zeilen in
      `XBMContentStageEvent` und die Weltkoordinaten von Daedalus' Laufweg (`KnownFlippedForks`, 27.10.2026), nicht den
      gezeichneten Plan. Das Feld steht weiter dabei, falls die Kamera anders blickt.
    - *Vorausrechnung (seine Präzisierung: wie beim Schach):* je Abzweig die Felder bis zum ersten Feld, das jeder
      Abzweig erreicht (Zusammenlauf), dann weiter bis vor die nächste Campsite oder den Endgegner: Erst dort zeigt sich,
      ob man die Rast hier braucht. An einer Gabelung dazwischen die bessere Fortsetzung (die wählst du dann selbst), an
      einem Zufallsfeld die schlechtere seiner Möglichkeiten (Sicherheit vor Glück); ein Zufallsfeld zählt nicht als
      Rast. Danach rechnet RSR am nächsten Feld neu.
    - *Zustand:* deine LP-Anteil jetzt; dein Vorrat an Heilgegenständen als Anteil deiner LP (K8, nur die für dich); der
      niedrigste LP-Anteil der Heiler und Schützer aus dem Endkampfplan (K11, K14) und deren größter Verlust je Einsatz
      (`FinalLossPerUse`).
    - *Felder:* Campsite wie K10 – Begleiter ruhen nur mit, wenn du voll herauskommst, so viele, wie das Lager nimmt
      (`XBMContentCamp`); einen gefallenen Begleiter heilt sie nicht (Wiki, oben), ein toter Begleiter spricht also nicht
      für die Campsite. Laden, Schatz: keine LP, aber Gegenstände (vermerkt, Marken genannt); Gegner: die mittlere
      gemessene Last eines Kampfs abzüglich deiner Heilung (`CrucibleBoard.Loads`, wie K10 Laden); Elite und Endgegner:
      die schwerste gemessene; die Heiler verlieren je Kampf ihren größten Verlust je Einsatz.
    - *Sicher* ist ein Abzweig, wenn du an der nächsten Rast oder vor dem Endgegner mit Vorrat noch einen schwersten Kampf
      trägst und kein Endkampf-Heiler knapp wird (K11, `ShortForFinal`). Empfohlen wird unter den sicheren der, der
      Gegenstände bringt (Schatz, Laden mit Marken), sonst der mit den meisten LP danach; ist keiner sicher, der, nach dem
      du mit Vorrat am höchsten stehst, bei Gleichstand der mit den gesünderen Heilern.
    - *Grund im Fenster:* Die Empfehlung nennt, was der andere Abzweig gebracht hätte – bei Campsite gegen Gegenstände
      die LP, die die Rast dir und den Endkampf-Heilern bis zur nächsten Rast mehr ließe, und ob beide Wege sicher sind –
      und wo die nächste Rast liegt.
    - *Vor der ersten gemessenen Last:* keine Rechnung; die Campsite, solange du oder ein Endkampf-Heiler nicht voll steht,
      sonst der Abzweig mit Gegenständen. Das Fenster sagt es.
    - *Im Kampf (seine Lage, 07.11.2026):* Brett 1 nach Zug 1, du bei rund 93 %, ein Kampf kostete rund 4 %, Vorrat 36 %,
      Endkampf-Heiler voll. Links Gegner und Campsite, rechts Gegner und Schatz; zusammen laufen die Wege in Zug 4, die
      nächste Campsite liegt rechts nach dem Elitegegner (Zug 7). Bis vor sie: links 100 % nach der Rast, dann zwei Kämpfe
      (Zug 4, Elite in Zug 6) auf rund 92 %; rechts rund 81 %. Beide sicher; empfohlen: rechts, der Schatz. Die Rast
      brächte dir rund 11 % und den Heilern nichts, die sie schon voll stehen. Das bestätigt seine Einschätzung.
    - *Im Kampf (Gegenbeispiel):* Nach Kampf 3 bei 55 %, zwei Bestien-Heiltränke (je 30 %), die Fledermaus als
      Endkampf-Heilerin bei 40 %, ein Kampf kostet 35 %, der schwerste 50 %. Rechts über den Schatz kommt die Fledermaus
      knapp an die nächste Rast – nicht sicher. Links ruht sie mit (du kommst mit einem Begleiter voll heraus), sie steht
      danach bei 85 %. Empfohlen: links, die Campsite.
    - *Verworfen:* (a) ein Plan fürs ganze Brett beim Betreten – seine Präzisierung; (b) Punkte und Marken als Ziel – er
      nannte Gesundheit, Ausrüstung, Vorrat und Heilbedarf; (c) die Rechnung bis zum ersten Kampf des Abzweigs (A632) –
      sie sah nicht, was die Abzweige unterscheidet: Auf Brett 1 liegen Campsite und Schatz hinter je einem Gegner, das
      Fenster verglich zwei gleiche Gegner und nannte die Seite nicht.
    - *Gegenthese 1:* Die mittlere Last stammt aus anderen Kämpfen; der nächste ist schwerer. Teilweise widerlegt: Elite und
      Endgegner rechnen mit der schwersten, und „sicher" verlangt einen schwersten Kampf Reserve. Bis zur nächsten Rast
      sind es auf den fünf Brettern ein bis drei Kämpfe; der Fehler der Mittelung wächst mit ihnen, bleibt aber auf
      diese Strecke beschränkt, für die die Rast entscheidet. *Gegenthese 2:* RSR verliert die Spur (ein Fenster, das es
      nicht liest; ein Zufallsfeld, das ein anderes Fenster öffnet). Dann rechnet es vom letzten bekannten Feld und zeigt
      Abzweige, die schon hinter dir liegen; das Fenster nennt das Feld, von dem es rechnet. *Gegenthese 3:* Gegenstände
      aus Schatz und Laden sind nicht bezifferbar, bevor man sie sieht. Nicht widerlegt: Sie zählen nur als Vorzug unter
      sicheren Abzweigen. *Gegenthese 4 (Kamera):* Blickt er vom Endgegner zurück, sind links und rechts vertauscht.
      Teilweise widerlegt: Das Feld steht neben der Richtung. *Ausgeliefert, und nichts ändert sich?* Ohne Hornfenster im
      Lauf, ohne Brettplan im Blatt oder auf Gabelungen ohne Unterschied bis zur nächsten Rast.
    Werkzeugquelle für die Vorlage: BeastMasterHelper (t3knical, Fork von anmili2022/Beastmaster) bevorzugt an einer
    Abzweigung die Campsite, wenn der Spieler verletzt ist, und prüft die LP des Kaders (Anleitung, 06.11.2026, A623).
- *K14 – Endkampf vorbereiten: Lager und Futter* (A631; seine Vorschläge vom 06.11.2026: „beim letzten rasten vor dem
  endgegnern im brett wäre eine empfehlung hilfreich, welche beiden begleiter man zum heilen auswählen sollte" und „beim
  händler für essen im brett sollte nahrung für die begleiter gegen endgegner brett empfohlen werden, welche nahrung für
  welchen begleiter. also drei stück"). *Im Kampf:* Wer an der letzten Campsite mitruht, kommt mit einem Anteil von
  30 % (zu zweit) mehr LP in den Endkampf; Futter gibt einem Begleiter bis zu +50 % maximale LP, −10 bis −35 % erlittenen
  Schaden oder weniger Verwundbarkeit. Ohne beides trägst du die Treffer des Endgegners, wenn seine Heiler fallen.
  - *Der Weg aus den Blättern* (Daedalus c4a788f, `CrucibleRoute.cs`, am Client gelesen 31.10.2026 und an zwei Brettplänen
    des Wikis Feld für Feld geprüft; Werkzeugquelle): `XBMContentStageEvent` hat je Brett eine Unterzeile je Feld, roh
    gelesen Spalte 0 der Zug, 1 die Art (1 Start, 2 Gegner, 3 Elite, 4 Endgegner, 5 Laden, 6 Campsite, 7 Schatz, 8
    Zufall), 2 ein Index – bei einem Kampf die Unterzeile von `XBMContentBattle` (0 der Endgegner), bei einem Zufallsfeld
    die von `XBMContentRandomStageEvent`, deren Zeile in `XBMRandomStageEvent` nennt, was es werden kann (Art, Index).
    `XBMContentStageEventMap` zeichnet das Brett: Zeilen mit Art größer 1 sind Wege von Feld zu Feld. Die Blätter tragen
    keine Spaltennamen (EXDSchema ohne XBM-Schemas, 09.09.2026); RSR liest sie roh wie Daedalus. Brett 1: Zug 1 Gegner,
    2 Gegner|Gegner, 3 Campsite|Schatz, 4 Gegner, 5 Schatz, 6 Elite, 7 Schatz|Campsite, 8 Laden, 9 Endgegner.
  - *Wo du stehst:* Das Spiel führt das Feld nirgends (ClientStructs 0adb2e5: weder Agent noch Direktor; Daedalus
    bestimmt es aus der Position auf dem Brettplan). RSR kennt den letzten Kampf: Das Hornfenster nennt ihn
    (`XBMBattleDetail`-Zeile), und die ist auf genau einem Feld. Das widerlegt Gegenthese 3 von K11, soweit sie den
    letzten Kampf betrifft.
  - *Letzte Campsite:* Eine Campsite ist die letzte, wenn vom letzten Kampf aus keine weitere – auch kein Zufallsfeld, das
    eine werden kann – erst nach einem weiteren Kampf erreichbar ist. Ohne lesbaren Plan (keine Wegzeilen) gilt keine als
    die letzte. Brett 1: die auf Zug 7; die auf Zug 3 nicht, weil nach dem Kampf auf Zug 4 noch eine kommen kann.
  - *Empfehlung an der Campsite:* Das Fenster „RSR - Crucible campsite" zeigt, wen RSR vorwählt und warum (K10 wie
    bisher: nur wenn du voll herauskommst). An der letzten Campsite empfiehlt es dazu, wer für den Endkampf mitruhen soll:
    unter den stehenden, ungefütterten, nicht vollen Begleitern zuerst die Heiler und Schützer aus dem Plan des Endkampfs
    (K11, `FinalHealers`), dann die übrigen Begleiter dieses Plans, dann die übrigen nach ihrem LP-Anteil; so viele, wie
    das Lager nimmt (gelernt; vorher alle, die zählen). Es nennt, mit welchem Anteil du herauskommst. „Apply" wählt die
    Empfehlung im Fenster; ruhen drückst du. Die Vorauswahl ohne Klick bleibt bei K10: Deine LP sind der Lauf, und ob
    dein Rest gegen die Heilung deiner Heiler im Endkampf trägt, entscheidest du mit dem Klick.
  - *Futter für den Endkampf:* Das Ladenfenster empfiehlt für die Begleiter aus dem Plan des Endkampfs (drei Hörner)
    Futter aus dem Angebot, das zu ihrer Art passt („Suitable for …kin", `XBMItem`, Art „Feed"; Art je Begleiter aus
    `XBMPet`). Maß ist, was ihn im Endkampf hält: seine wirksamen LP – Anteil jetzt plus Heilung beim Fressen, höchstens
    die neuen maximalen LP, geteilt durch den erlittenen Schaden nach Abzug von Schaden, physischer und magischer
    Verwundbarkeit und Ausweichen, gewichtet mit Stärke gegen Intelligenz der Gegner des Endkampfs (Blatt) –, gerechnet
    auf das Futter, das er schon trägt (Wirkungen multipliziert wie bei Ausrüstung, meine Ableitung, unbelegt). Bei
    Gleichstand mehr Schaden („Damage Dealt", auch physisch oder magisch), dann das billigere. Ein Futter, das weder
    wirksame LP noch gezählten Schaden bringt, empfiehlt RSR nicht (Bananen-Simulakrum: Ätherausbruch und Schaden nach
    kritischem Treffer, beides ungezählt); eines, das nur Schaden bringt, nach allen, die LP bringen, und mit diesem Grund.
    Nie Futter, das sich selbst einen Zustand zufügt (Blood, Mucus, Black Scorpion Simular), wie bei Ausrüstung.
    *Futterplätze (sein Hinweis vom 07.11.2026: „mehr verfüttern als felder da sind, ist nicht möglich"):* Jeder
    Begleiter frisst höchstens so viel Futter, wie er Plätze hat – in seinem Fenster Adaman-Taimai, Drakon und Kaktor je
    zwei, Geist fünf. Das Begleiterfenster (`XBMPetParty`) führt je Begleiter, wie viele er trägt und tragen kann, und
    welche (ECommons `FedCurrent`, `FedMax`, `FedItems`, Werte 72, 73 und ab 7 einer Zeile, Werkzeugquelle; Daedalus
    2644cbe liest 72 an derselben Stelle). RSR merkt sich das je Begleiter, sooft das Fenster steht – am Hornfenster vor
    jedem Kampf, an der Campsite, beim Füttern –, für dieses Brett. Empfohlen wird nur für einen freien Platz: erst je
    Begleiter einer, Heiler und Schützer zuerst, dann die zweiten Plätze, so weit die Marken nach der Kaufliste reichen;
    darüber hinaus mit Vermerk. Hat RSR das Fenster auf diesem Brett noch nicht gesehen, oder liest es keinen Höchstwert,
    gilt je Begleiter ein Platz, und das Fenster sagt es. Grund: Vorher empfahl RSR jedem Begleiter eins, ohne seine
    Plätze zu kennen, und nahm das erste passende Angebot auch ohne Gewinn: Bananen-Simulakrum für Drakon, der beide
    Plätze belegt hatte, mit „effective HP rises 0 %"; „heals or protects" vor dem Grund war die Rolle des Begleiters
    und las sich als Wirkung des Futters.
    *Seine Lage (07.11.2026, Laden auf Zug 8 von Brett 1, danach der Endgegner):* Plan Adaman-Taimai, Drakon, Kaktor.
    Toast für Adaman-Taimai, Porcinipilz für Drakon (dazu ein Eier-Simulakrum von vorher), Frischkäse für Kaktor (dazu
    Beeren) – danach waren Drakon und Kaktor voll, Honig füllte Adaman-Taimais zweiten Platz. Richtig wäre danach:
    kein Futter mehr, weil kein Platz frei ist.
    Ob eine Campsite danach noch kommen kann, steht dabei: Ruht ein Gefütterter dort, verliert er das Futter („Familiars
    that rest will lose current effects of feed"); Lily Simular hält es. Gekauft und gefüttert wird von Hand. Namen von
    Futter und Begleitern zeigt das Fenster in der Sprache des Clients (Blatt `XBMItem`; der Begleiter, wie ihn das
    Begleiterfenster nennt), sonst englisch.
  - *Beim Füttern:* Öffnet das Begleiterfenster zum Füttern (`AgentXBMPetParty` Modus 3 „buy feed", 4 „buy feed result",
    5 „feed detail", ClientStructs 0adb2e5; das Futter in `ReaderXBMPetParty.FeedItem`, ECommons 65be12d), nennt das
    Fenster, wem es zugedacht ist, nach den Plätzen, die das Fenster jetzt zeigt: der Begleiter des Endkampfplans mit
    freiem Platz, dem es am meisten bringt; hat keiner einen freien Platz oder bringt es keinem etwas, sagt es das.
    RSR klickt dort nichts.
  - *Sperre:* Die Campsite-Wahl (K10) erkennt das Lager an Modus 4. Den nennt ClientStructs „buy feed result"; Daedalus
    sah ihn im Spiel an der Campsite. Läuft ein Futterkauf (Modus 3 oder 5 gesehen, seit das Fenster offen ist, oder ein
    Futter im Fenster), klickt RSR dort nicht: Ein Klick wählte sonst, wer frisst.
  - *Verworfen:* (a) Am letzten Lager ohne Klick für die Begleiter rasten, auch wenn du nicht voll herauskommst – die
    Bilanz hängt an Endkampf-Lasten, die nur für dich gemessen sind; (b) Futter schon an jedem Laden – vor einer Campsite
    geht es verloren oder hält den Begleiter vom Rasten ab; (c) Futter nach Schaden zuerst – Sicherheit vor Schaden.
  - *Gegenthese 1:* Der Plan des Endkampfs stammt vom letzten Hornfenster; seither änderten sich LP. Teilweise widerlegt:
    Er ändert sich mit dem Kader, kaum mit den LP; die Empfehlung liest die LP jetzt. *Gegenthese Futterplätze:* Ruht ein
    Gefütterter an der Campsite und kommt der Laden ohne Kampf danach (Brett 1: Zug 7 Campsite, Zug 8 Laden), sieht RSR
    seine Plätze noch belegt, bis das Fenster wieder steht. Nicht widerlegt, Preis benannt: Die Ladenliste empfiehlt ihm
    dann zu wenig; beim Füttern liest RSR die Plätze neu und nennt ihn. RSR selbst lässt keinen Gefütterten ruhen
    (Campsite oben). *Gegenthese 2:* Ein Zufallsfeld wird
    keine Campsite, und die als „nicht letzte" gezeigte war doch die letzte. Preis: Die Empfehlung fürs Ende fehlt dort,
    die Vorauswahl bleibt. *Gegenthese 3:* Ob „Maximum HP +X %" die aktuellen LP mit hebt, war offen; gerechnet wurde
    ohne. Sein Mitschnitt vom 07.11.2026 bestätigt das: Kaktor stand vor dem Frischkäse-Simulakrum (+50 % maximale LP,
    26 % beim Fressen) auf 550/550 und danach auf 763/823 – die LP bleiben, die Heilung beim Fressen zählt auf das neue
    Maximum (550 + 26 % von 823 = 764). *Ausgeliefert, und nichts ändert sich?* Ohne Hornfenster in diesem Lauf (kein letzter
    Kampf bekannt) oder ohne Futter im Angebot.

- *K13 – Schatz* (A578; seine Meldung vom 05.11.2026: „bei den schätzen gibt es keine empfehlung, was zu wählen ist").
  *Mechanik:* Ein Schatzfeld bietet einen Gegenstand aus einem halbzufälligen Vorrat zur Wahl, keinen doppelt; mit einem
  Diebesmesser zwei (consolegameswiki, „Crucible of the Unbroken", 05.11.2026). Das Fenster `XBMContentsTreasure` führt
  bis zu vier Angebote als `XBMItem`-Zeile an den Feldwerten 6 + 5·i, gewählt wird mit dem Rückruf `[2, i]`, geschlossen
  mit `[0]`; nach einer Wahl mit Diebesmesser bleibt es offen (Daedalus 74659a6, `CrucibleAutoPilot.TakeTreasure` und
  `CrucibleAutoPilotRules`, Feldnotiz 25.10.2026; Werkzeugquelle aus dem Spielbetrieb).
  *Wahl:* dieselbe Rangfolge wie im Laden (oben), mit Preis null und einer Wahl je Klick: ein zweites Leben, solange
  keins gehalten wird; Schutz gegen einen Zustand des Bretts, der im Tod endet; das Ausrüstungsstück, das nach der Skala
  des Ladens am meisten LP nach der Strecke lässt, bei Gleichstand die meisten effektiven LP; Schutz gegen die übrigen
  Zustände des Bretts; Heilgegenstände; schützende Gegenstände. Bringt kein Angebot Sicherheit, ein Gegenstand für
  Schaden oder Stärkung (K8 setzt sie im Endkampf ein), sonst Ausrüstung, die dir nicht schadet. Ausrüstung, die dir
  einen Zustand zufügt, nie. *Was gehalten wird:* die Gegenstände aus dem Inventar des Bretts; die Ausrüstung, wie sie
  der letzte Laden dieses Bretts zeigte, dazu was RSR selbst aus Schätzen genommen hat.
  *Fenster:* „RSR – Crucible treasure" mit der Empfehlung, dem Grund und einem Knopf „Take", der genau sie nimmt; ohne
  Klick nimmt RSR nichts (wie Laden und Kader, Entscheidung C, A533). Einstellung „Recommend what to take from a
  Crucible treasure", ab Werk an.
  *Eine Skala für Ausrüstung und Gegenstände (A656; seine Fragen vom 07.11.2026 zu einem Schatz mit
  Ninja-Augenklappe, Mystischen Schuhen, Bestien-Pulver III und Flammenschild, empfohlen war das Pulver: „warum ein
  verbrauchsgegenstand, warum kein permanentes item für alle noch anstehenden kämpfe? ist die auswahl korrekt? und
  warum wurde die auswahl getroffen"; seine Vorgabe: „der grund der auswahl sollte im tooltip drin stehen"):* Nach
  zweitem Leben und Schutz gegen einen tödlichen Zustand (oben, unverändert) zählt jedes Angebot in derselben Einheit:
  LP-Anteile, die es dir über den Rest des Bretts bringt. Ausrüstung: der Schaden, den sie in den übrigen Kämpfen
  verhindert (mittlere Last je Kampf, schwerste für Elite und Endgegner, Ausweichen nur gegen den physischen Teil),
  dazu ihre Mehrheilung auf die gehaltenen Heilgegenstände („Increases HP recovery …" mal Vorrat). Heilgegenstand: seine
  Heilung, mit der Mehrheilung deiner Ausrüstung. Schützender Gegenstand: sein Anteil des schwersten Kampfs. Die
  übrigen Kämpfe sind die auf dem kürzesten Weg zum Endgegner, der Endgegner eingeschlossen. Was die Skala nicht misst –
  Bewegung, Block, Hast, kritische Treffer, Schaden, Absorbieren eines Elements –, nennt das Fenster beim Angebot, statt es
  zu verschweigen. Vor der ersten gemessenen Last zählt die Reihenfolge oben.
  *Grund im Fenster:* Jedes Angebot steht im Fenster mit seinem Namen in der Sprache des Clients, seinem Wert auf der
  Skala und dem, was sie nicht misst; das empfohlene zuerst, mit dem Abstand zum nächstbesten.
  *Seine Lage (07.11.2026, Zahlen aus seinem Routenfenster kurz davor):* Ein Kampf kostete rund 4 %, Vorrat 36 %,
  danach noch drei Kämpfe (Gegner, Elitegegner, Endgegner). Die Mystischen Schuhe: Ausweichen 8 % auf den physischen
  Teil von drei Kämpfen, rund 1 %, und +15 % auf 36 % Vorrat,
  rund 5 % – zusammen rund 6 %, dazu 50 % schneller laufen (nicht gemessen). Das Bestien-Pulver III: 40 % für dich und
  die Verbündeten in der Nähe, einmal. Ninja-Augenklappe und Flammenschild: keine Sicherheit auf der Skala (Hast,
  kritische Treffer, Block, Schaden, Feuer absorbieren). Die Wahl stimmt also; ihr Grund im Fenster fehlte. Der Grund
  war vorher schwächer, als er schien: Die Rangfolge stellte jede Ausrüstung mit messbarem Gewinn vor jeden
  Heilgegenstand, und die Schuhe zählten nur nicht, weil die Laden-Skala ihre Mehrheilung allein an der im Kampf
  gemessenen Heilung maß. Mit gezählter Mehrheilung hätte die alte Rangfolge die Schuhe (rund 6 %) vor das Pulver
  (40 %) gestellt.
  *Der Laden:* Er kauft mehrere Stücke mit Marken; die Skala ersetzt dort noch nicht die Rangfolge (TODO, „Laden:
  Ausrüstung und Gegenstände auf einer Skala").
  *Gegenthese 1 (Ausrüstung unbekannt):* Vor dem ersten Laden kennt RSR deine Ausrüstung nicht, und was ein Zufallsfeld
  gab, sieht es nicht. Nicht widerlegt: Die Empfehlung kann ein Stück vorziehen, dessen Wirkung du schon trägst; Preis:
  eine schwächere Wahl. Ausrüstungsplätze vor dem ersten Laden: RSR nimmt einen freien an.
  *Gegenthese 2 (Daedalus' feste Gewichte):* Daedalus wertet mit festen Gewichten je Wert (LP +1,0, Schaden genommen
  −1,5 …). RSR rechnet wie im Laden am gemessenen Schaden der Kämpfe (A570) – dieselbe Skala an beiden Stellen, keine
  zweite.
  *Gegenthese 3 (Wahl von Hand mit Diebesmesser):* Nimmt er die erste Wahl selbst, weiß RSR nicht, welche Zeile genommen
  ist, und kann sie erneut empfehlen. Nicht widerlegt; das Spiel gibt sie nicht zweimal, und die zweite Wahl trifft er.

- *K12 – Die Eier des Zu* (A572, aus Daedalus 74659a6, v0.1.112: „Every egg broken before it hatches makes the Zu
  stronger, and a run died there after three were broken: a weaponskill went straight into an egg, Shield Charge (it hits
  everything within 6 yalms of its target) broke the two beside a hatched Cockerel, and Tempered Release went out at a
  Pullet"). Der zu piece (Namensblatt 14572) ist ein Elitegegner des dritten Bretts; seine Eier sind „zu egg" (14575,
  14576). *Was gilt:* Solange ein zu piece lebt, ist ein zu egg kein Ziel und trifft keine Fläche (Konzept 03, „Gegner,
  die keine Fläche treffen darf") – Schildansturm, Parting Blow und jede andere Fläche suchen einen Mittelpunkt ohne Ei
  oder warten. Einstellung „Crucible of the Unbroken - Zu logic", ab Werk an, neben Upstreams Ymir- und Siren-Regel.
  *Verworfen:* Daedalus' Freigabe der Flächen, sobald der Zu unter 15 % fällt: eine Zahl ohne Herleitung; ob ein Ei dann
  noch schadet, belegt nichts. *Gegenthese:* Ohne Flächen dauert der Kampf länger. Widerlegt im Sinne seiner Spielweise:
  Ein stärkerer Zu kostet LP, die längere Dauer Schaden.
  - *Die übrigen Tötungsregeln der Bretter* (A588; die Regeln selbst: Konzept 03, „Tötungsregeln"; Einstellung „Kill
    rules of known fights", ab Werk an). Quelle der Kämpfe: BossModReborn 27be42256 (05.11.2026, Werkzeugquelle,
    Module „Contributed" oder „WIP"), Daedalus 74659a6 (Feldnotizen), GluttonyCombo 1.0.4.305 (15.10.2026, Mitschnitte),
    die Blätter (xivapi v2, 05.11.2026).
    - *Tablitauren, Brett 2 – gemeinsam:* younger und elder tablitaur piece (14556, 14555). Fällt einer, beginnt der
      andere Endless Slashes (48214, 9 s Wirkzeit laut Blatt), danach trifft eine Linie von 80 × 70 Yalm die Arena
      (48216; BossModReborn: „Enrage – if one dies before the other too early"). Daedalus im Feld (25.10.2026): ein
      Stier fiel, der andere stand auf 72 %, der Überlebende tobte. Fenster 9 s, Rücklage 4,5 s des Stroms.
    - *Lamien, Medusa-Kampf, Brett 5 – gemeinsam:* lamia piece (14661). Eine lebende Lamia wirkt Raise (49287, 10 s) auf
      eine tote; unterbrechbar (K4). Fenster 10 s. Daedalus (02.11.2026): „should just balance anyway".
    - *Morpho und Abaddon, Drachen-Kampf, Brett 5 – erst danach:* Ein morpho piece (14656) fällt nicht, solange ein
      abaddon piece (14655) lebt – kein Ziel, keine Fläche: Sein Tod verwirrt dich (BossModReborn: „Killing MorphoPiece
      will cause confuse debuff as well, must be eaten by AbaddonPiece"; sein Zauber Delusion Dust trifft jeden mit
      „Confused", eorzeantavern). Ein abaddon piece ist kein Ziel, solange ein morpho piece lebt: Er frisst sie (BossModReborn:
      „Don't kill until all MorphoPiece are dead"; GluttonyCombo: nie vor ihnen töten). Ihn treffende Flächen bleiben
      erlaubt: Er hat viele LP, und erst sein Fall vor dem letzten Morpho schadet. *Verworfen:* GluttonyCombos „bis 35 %
      anschlagen und halten" – eine Zahl ohne Herleitung; die Zeit bis zum Fressen kürzt es nicht, und seine Regeneration
      (Rehabilitation, eine Minute, mit jedem Morpho erneuert; Quelling Wave entfernte sie in 15 Würfen nie) hebt den
      Halt wieder. Lebt kein Abaddon mehr, fallen Morphos wie jeder Gegner (Wit's End verwirrt nach 16 Stufen ohnehin).
    - *Myconiden, Gedankenschinder-Kampf, Brett 5 – nur am Ort:* Ein myconid piece (14634) fällt nur in einer Pfütze von
      Void Water III, solange der mindflayer piece (14633) lebt – außerhalb kein Ziel, keine Fläche. Außerhalb legt sein
      Tod Spore Spill (49196, 6 Yalm, 1 s, Gift) und eine Pilzzone; in der Pfütze nicht (Spieltext im Kampf: „Moisture
      dampens the proliferation of spores!", consolegameswiki; eorzeantavern; Forum). Die Pfütze ist das Bodenobjekt
      2005400 (BossModReborn 0x1E9998), ihr Radius der Wirkradius von Void Water III (49200: 8 Yalm laut Blatt);
      verbraucht ist sie im Ereigniszustand 7 (BossModReborn, `EventState`). BossModReborn prüft mit 6 Yalm – das ist der
      Radius von Spore Spill, nicht der Pfütze. Hinein gehst du oder BossModReborn: Dessen Modul will dich in den Pfützen
      (`PersistentInvertibleVoidzone`, ab Kampfbeginn umgekehrt), die Myconiden folgen dir.
    - *Versteinerter Zyklop, Medusa-Kampf:* allgemein, Konzept 03, „Ein Treffer tötet ihn".
    - *Barbmole, Drachen-Kampf:* allgemein, Konzept 03, „Gegner mit Stacheln" (Needles Out).
    - *Nicht gebaut, mit Grund:* Loosefrox und Chewchum (Brett 2): BossModReborn schont Chewchum, solange Loosefrox lebt,
      Daedalus nennt sie in einem Kommentar ein Paar, das zusammen fallen muss; kein Blatt und kein Feldbeleg sagt, was
      geschieht – die Gefahrenreihenfolge (K3) ordnet. Gigantis-Schleime (Brett 5): Welcher fallen soll, hängt daran, ob
      der Spieler die Keule auf den anderen lenkt; RSR lenkt nicht, und ein geschonter Schleim wirkt Rupture auf die
      ganze Arena (unterbrechbar, K4). Eisbomben des Progenitrix (Brett 4): Ihr Fall bei Feuerpfützen räumt diese;
      Stellung ist Sache des Spielers, Ice Spikes meidet die Stachelregel. Reihenfolgen ohne Mechanik (Bavarois,
      Boogyman, Zu-Küken, Fronten aus Zusatzgegnern vor dem Boss): K3 ordnet nach Gefahr; BossModReborn ordnet dort
      „Zusatzgegner vor Boss" fest, K3 rechnet es. Golem-Herz (Brett 4, 20 s nach dem zweiten Golem): keine Wahl, nur
      Schaden. Atomic Ray der Durga (49272, „pet cover will not take it", Daedalus nach icy-veins): RSR löst
      Zähnefletschen nach LP aus, nicht nach angekündigten Flächen (K9), und rechnet nirgends mit der Deckung gegen sie.

- *K11 – Begleiter über das Brett planen* (sein Hinweis und seine Vorgabe vom 05.11.2026: „da begleiter über die
  spielfelder des brettes hinweg schaden ansammeln und hp sinkt, auch strategische planung, falls begleiter mehrfach
  eingesetzt werden sollen, damit diese an den spielfeldern, wo sie unbedingt notwendig sind, auch bereitstehen").
  *Gemessen (04.11.2026):* Im zweiten Lauf fiel die Fledermaus – einer der beiden Heiler – bis zum vierten Kampf auf 25
  von 550 LP, die Fliegenfalle bis zum Endgegner auf 236 von 608, der Schleim auf 265 von 462; im ersten Lauf Opo-Opo,
  Megalokrabbe und Pugil auf etwa 55 bis 60 %. Ohne Regeneration trägt jeder Einsatz seine LP in den nächsten Kampf.
  *Was gilt (Plan):* RSR führt je Brett eine Bedarfsliste aus dem Blatt: je Kampf die Begleiter, die dort unersetzlich
  sind – der einzige mit der Schwäche der gerufenen Gegner (K2), der einzige, der einen unterbrechbaren Zauber stoppt
  (K4), ein Heiler für die gefährlichen Kämpfe –, und je Begleiter seine LP und den Schaden, den er je Einsatz bisher
  nahm. Daraus: Ein Begleiter, der später unersetzlich ist, geht in früheren Kämpfen nur auf ein Horn, wenn seine LP
  danach für seinen Kampf reichen (K6); er trägt keinen Schaden für dich über Zähnefletschen (K9); an der Campsite ruht er
  zuerst, wenn seine LP nicht reichen (K10); ein Bestien-Heiltrank auf ihn, wenn keine Campsite mehr vor seinem Kampf
  liegt (K8). Fällt er trotzdem, kommt das Horn der Rückkehr vor seinem Kampf. *Gegenthese 1:* Der Weg verzweigt, Felder
  sind zufällig – welcher Kampf kommt, steht nicht fest. Teilweise widerlegt: Die Liste gilt für die Kämpfe, die auf dem
  gewählten Weg noch erreichbar sind, und wird nach jeder Abzweigung neu gerechnet; Zufallsfelder ziehen aus den Kämpfen
  des Bretts. *Gegenthese 2:* Wer zurückgehalten wird, fehlt in den frühen Kämpfen, wo er ebenfalls passt. Nicht
  widerlegt: Gegen die Gefahr, ihn im wichtigen Kampf nicht zu haben, steht die Gefahr im frühen; RSR wägt mit dem
  gemessenen Schaden je Einsatz und hält ihn nur zurück, solange ein anderer Begleiter den frühen Kampf trägt.
  *Gegenthese 3:* RSR kennt das aktuelle Feld nicht. Offen: Das Spiel führt das Feld im Brett
  (`AgentXBMStageDetailList`, Werkzeugquelle); ob es lesbar ist, zeigt die Umsetzung, sonst gilt der Plan für alle
  Kämpfe des Bretts. *Ergebnis:* K11 ist die Planungsebene über K6, K7, K8, K9 und K10; sie alle lesen dieselbe
  Bedarfsliste.
  *Stand der Umsetzung (A550, A556):* Die Kämpfe je Brett liefert das Blatt (`XBMContentBattle`: Zeile je Brett 1 bis 5
  mit 6, 7, 8, 10 und 14 Kämpfen, Unterzeile je Kampf mit Verweis auf `XBMBattleDetail`, Unterzeile 0 der Endkampf).
  Kader und Feld führt FFXIVClientStructs seit 17.09.2026 (d44bc123, 3120c745; Stand aad0de66 vom 05.11.2026,
  Werkzeugquelle), in Dalamuds Fassung vorhanden: `AgentXBMPetParty` mit `SelectedPetIds` (Kader, `XBMPet`-Zeilen),
  `PetCurrentHealth` und `PetMaxHealth` (15 Plätze) und `Mode` (1 Vorschau des Kaders, 2 Vorschau aus dem Inhalt);
  `AgentXBMStageDetailList` mit `Mode` (1 laufendes Brett), Einträgen je Feld und Kampf (`EntryType` 3 mit
  `BattleDetailId`, gebaut aus dem Director) und `EntrySelection`. Gefüllt sind sie
  jedenfalls im Hornfenster vor jedem Kampf: Daedalus liest dort Kader, LP und den anstehenden Kampf (A561); der Kader
  ist `SelectedPets` in Fensterreihenfolge, `SelectedPetIds` hält die gewählten Plätze, nicht Begleiter. Ob sie bei
  geschlossenen Fenstern stehen bleiben, zeigt die Sonde (A556, A561): Sie schreibt sie beim Betreten des Bretts, beim
  Öffnen des Hornfensters, beim Erkennen eines Kampfes und an seinem Ende („crucible agents …"), nur Felder, keine
  Spielfunktion. Die Planungsregel setzt am Hornfenster an (K6); der Loop am Ende des TODO baut darauf auf.
  *Was die Sonde zeigt (05.11.2026, Brett 1):* Kader und LP sind im Hornfenster, im Kampf und an der Campsite gefüllt;
  der Feldagent nennt nur den anstehenden Kampf (eine Zeile), den weiteren Weg nicht. Sicher kommt nur der Endkampf.
  *Die LP des Kaders hängen einen Kampf nach* (A585): Der Agent übernimmt die Verluste eines Kampfes erst mit der
  Meldung des nächsten, nach einer Campsite sofort. Die Fledermaus stand im Hornfenster vor Kampf 4 (20:52:14) mit
  608 LP, erschien in Kampf 4 aber mit 458 (ACT, 20:54:08); der Raptor ebenso (535 angezeigt, 395 beim Ruf). Im Kampf
  sieht RSR die wahren LP am Begleiter selbst: Er erscheint mit dem Stand, mit dem er zuletzt ging. Die LP, mit denen
  RSR wählt (K6, K11), sind deshalb je Begleiter die zuletzt im Kampf gesehenen, solange der Agent seither nicht
  nachgezogen hat; zieht er nach (nächster Kampf, Campsite), gilt wieder er. Ein Begleiter, der fiel, zählt mit 0.
  *Abtreten ist kein Fall* (A637): Nach dem Scheidenden Schlag oder einem Entfesseln, nach dem er abtritt, meldet das
  Objekt des Begleiters bis zum Verschwinden „tot". RSR zählte das als Fall, der Einsatz ging mit 0 LP in die Messung, und
  der größte Verlust je Einsatz stand nach dem ersten Abtreten bei den vollen LP des Begleiters. Am 06.11.2026 waren das
  550 nach Kampf 1, obwohl kein Begleiter fiel: Gigantoad trat um 17:25:24 mit 550 von 550 ab, der Mitschnitt führt auf
  dem ganzen Brett keinen Tod eines Begleiters. Auf diesem Wert ruhten die Reserve für den Endkampf (Stufe 1 unten), die
  Rast (K10, K14) und der Weg (K10 „Weg": ein Heiler des Endkampfs galt erst ab dem Doppelten als versorgt, also nie).
  Ein Fall zählt deshalb nur, solange der Begleiter nicht fortgeschickt ist (`FamiliarLeaving`); seine LP gehen mit dem
  letzten gelesenen Wert über 0 in die Messung. Ein Begleiter, der nach dem Fortschicken noch fällt, bevor er
  verschwindet, zählt mit diesem Wert – selten, und dann um höchstens einen Treffer zu niedrig.
  Verluste je Einsatz auf Brett 1 (Ruf bis Rückzug): Raptor 140, Cu Sith 277 (unter Zähnefletschen), Fledermaus 150,
  Schleim 283, Fliegenfalle 310, Pugil 209; vor dem Endkampf gewannen Raptor und Schleim an der Campsite je 153.
  *Stufe 1, gebaut (A585): den Endkampf versorgen.*
  - *Reserve:* Vor jedem anderen Kampf bestimmt RSR im Hornfenster, wen es für den Endkampf des Bretts belegen würde –
    dieselbe Wahl wie dort (Boss, so viele, die heilen oder schützen, wie `BossSafeFamiliars` verlangt, nach einer Gefahr
    im Gebiet einer mehr), aus dem lebenden Kader mit seinen LP. Die heilenden oder schützenden davon sind die Reserve.
  - *Verlust je Einsatz:* gemessen im Kampf, je Ruf: LP beim Erscheinen minus LP beim Gehen (beim Fall alle). Maß ist
    der größte Verlust auf dem Brett (eher zu groß).
  - *Regel:* Ein Reserve-Begleiter geht in einem früheren Kampf auf kein Horn, wenn ihm nach einem weiteren solchen
    Verlust weniger als ein Verlust für den Endkampf bliebe – solange ohne ihn so viele heilende oder schützende
    Begleiter bleiben, wie dieser Kampf verlangt, und jedes Horn belegt wird. Sonst wird der mit den meisten LP zuerst
    wieder zugelassen. Vor der ersten Messung keine Sperre. Eine Zeile je gesperrtem Begleiter.
  - *Im Kampf (Brett 1, gerechnet):* Vor Kampf 5 lag das Maß bei 283 (Schleim in Kampf 4). Fledermaus (458) und
    Schleim (228; der Agent zeigte noch 511), die Heiler des Endkampfs, wären in Kampf 5 nicht auf ein Horn gekommen,
    solange andere die Hörner füllen und dieser Kampf keinen Heiler verlangt; er setzte die Fledermaus dort von Hand,
    sie verlor nichts. Der Fall
    vom 04.11.2026 (Fledermaus bis zum vierten Kampf auf 25 von 550) kam aus einer Gefahr im Gebiet, nach der jeder Kampf
    einen Heiler verlangt: Dann nimmt RSR zuerst einen heilenden oder schützenden Begleiter außerhalb der Reserve.
  - *Gegenthese 1 (Campsite):* Eine Campsite zwischen jetzt und dem Endkampf heilt 30 bis 90 %; die Regel rechnet sie
    nicht ein und hält die Reserve öfter zurück als nötig. Preis: ein Begleiter, dessen Dressur heilt, fehlt in einem
    frühen Kampf ohne Gefahr – dort fragt B2 ihn nicht; mit Gefahr verlangt der Kampf ihn, und er kommt.
  - *Gegenthese 2 (Maß):* Der größte Fall stammt oft aus einem Begleiter, der unter Zähnefletschen alles trug; ein
    Heiler auf einem anderen Horn verliert weniger. Nicht widerlegt; das Maß hält eher zu oft zurück, nie zu selten.
  - *Gegenthese 3 (früher Tod):* Wer die Reserve schont, kann im frühen Kampf fallen. Widerlegt, soweit RSR es sieht:
    Verlangt der Kampf heilende oder schützende Begleiter, bleiben sie; nur Schaden geht verloren.
  - *Ausgeliefert, und nichts ändert sich?* Wenn die Reserve genug LP für zwei Verluste hat, oder vor der ersten Messung,
    oder wenn er die Hörner von Hand setzt.
  *Stufe 2, gebaut (A590): Zähnefletschen und Campsite nach Bedarf.* Endkampf-Heiler heißen die Begleiter, die in der
  Wahl für den Endkampf heilen oder schützen (Stufe 1, aus dem letzten Hornfenster vor einem anderen Kampf); knapp ist
  einer, dem nach einem weiteren Verlust in Höhe des größten auf dem Brett weniger als ein solcher bliebe – dasselbe Maß
  wie die Reserve.
  - *Zähnefletschen:* In einem Kampf vor dem Endkampf geht es nicht auf einen knappen Endkampf-Heiler, außer deine LP
    stehen an der Untergrenze (`PlayerHpFloor`). Unter Zähnefletschen nimmt er alles, was dir galt (Brett 1: Cu Sith
    verlor so 277 LP, der größte Verlust des Bretts), und seine LP kommen nur über Gegenstände und die Campsite zurück;
    deine heilen er und die anderen Heiler im Endkampf mit jeder Dressur wieder auf. *Im Kampf:* Vor Kampf 5 auf Brett 1
    stand die Fledermaus mit 458 LP bei einem Maß von 283 – knapp; deckte sie dich dort, fehlte sie im Endkampf. *Preis:*
    Du trägst die Treffer dieses Kampfes selbst, bis zur Untergrenze; dort deckt sie dich wieder.
  - *Campsite:* Unter den Verwundeten ruhen zuerst die knappen Endkampf-Heiler, dann wie bisher die am stärksten
    verwundeten (K10). *Im Kampf:* Ein Heiler bei 60 % ruht vor einem Schadensbegleiter bei 40 %, wenn die Campsite nur
    einen mitnimmt und der Heiler knapp ist; der Schadensbegleiter verliert die Heilung, nicht den Endkampf.
  - *Verworfen:* Unersetzliche Schwächentreffer für den Endkampf zurückhalten wie die Heiler. Sie bringen dort Schaden,
    kein Überleben; früh geschont, verlängern sie die frühen Kämpfe um denselben Schaden. Die Bilanz ist offen, solange
    kein Endkampf am Schaden scheitert; der Endkampf wählt sie ohnehin (K6, Schwäche im Wert).
  - *Gegenthese 1:* Die Endkampf-Wahl stammt aus dem letzten Hornfenster; seither können LP und Kader sich geändert
    haben. Teilweise: Die Wahl ändert sich nur mit dem Kader, die LP liest die Regel jetzt (Begleiter draußen,
    Campsite-Fenster). *Gegenthese 2:* Ohne Zähnefletschen fällst du schneller. Widerlegt bis zur Untergrenze, ab der es
    wieder gilt. *Ausgeliefert, und nichts ändert sich?* Vor der ersten Verlustmessung, im Endkampf und wenn die
    Endkampf-Heiler genug LP haben; die Spur nennt jedes verweigerte Zähnefletschen.
  - *Offen:* Die Campsite-Wahl (K10) liest Fenster und Agent und überlässt dir die Wahl, wenn ihre LP sich unterscheiden
    („the window's HP … is not the team's"); ob der Agent dort nachgezogen hat, zeigt die nächste Campsite mit RSR
    (A576 läuft seitdem auch bei ausgeschaltetem RSR).
  *Stufe 3, gebaut (A643): der einzige Unterbrecher für einen späteren Kampf.* Steht im lebenden Kader genau ein
  Seelen-Begleiter (Soul Crush über Borrow, K4) und verbraucht ein Horn One with Nature für Borrow, dann gehört er zur
  Reserve, wenn ein noch erreichbarer Kampf des Bretts eine Gegnerart führt, die RSR unterbrechbar zaubern sah
  (`InterruptibleCasterKinds.json`, K4), der anstehende aber keine – mit demselben Maß und denselben Grenzen wie die
  Endkampf-Heiler: knapp, wenn ihm nach einem weiteren Verlust in Höhe des größten auf dem Brett weniger als ein solcher
  bliebe; zurückgehalten nur, solange die Hörner sich füllen und dieser Kampf genug heilende oder schützende behält.
  *Noch erreichbar:* die Kämpfe auf den Wegen ab dem anstehenden Feld (`CrucibleRoute.BattlesAhead`, auch was ein
  Zufallsfeld werden kann, der Endkampf eingeschlossen); sind die Wege nicht gelesen, alle Kämpfe des Bretts.
  - *Im Kampf:* Auf Brett 4 zaubert der Administrator Confound („you will get confused for 12 seconds and most likely
    die", BossModReborn, Werkzeugquelle); unterbrechen kann nur Soul Crush. Kam der einzige Seelen-Begleiter in den
    Kämpfen davor auf ein Horn, nahm er dort Schaden und stand im Kampf des Administrators mit zu wenig LP – oder fiel.
    *Nachher:* Er bleibt in den Kämpfen davor vom Horn, sobald ein weiterer Einsatz ihn unter das Maß brächte. Gelernt
    sind bisher zwei Arten von Brett 1 (BNpcName 14531, 14542, seine Datei vom 06.11.2026); jede weitere lernt RSR beim
    ersten Zauber.
  - *Nicht so – der einzige Schwächentreffer eines späteren Kampfes:* aus demselben Grund wie für den Endkampf (Stufe 2,
    verworfen): Er bringt dort Schaden, kein Überleben, und früh geschont verlängert er die frühen Kämpfe um denselben
    Schaden. Ein Heiler für einen späteren gefährlichen Kampf: Die Gefahr eines Kampfes kennt RSR vorher nicht; nach einer
    Gefahr im Gebiet verlangt jeder Kampf einen (B5), und die Reserve hält die des Endkampfs.
  - *Gegenthese 1 (kein Gewinn):* Soul Crush geht nur, wenn sein Horn Borrow verbraucht und die Kinship läuft; ohne
    solches Horn hält die Regel nichts zurück. *Gegenthese 2 (Preis):* In den Kämpfen davor fehlt er – ein Schadens-
    oder Schutzbegleiter mehr auf dem Horn; mit Unterbrechern im anstehenden Kampf bleibt er. *Ausgeliefert, und nichts
    ändert sich?* Vor der ersten Verlustmessung, ohne gelernte Art auf dem weiteren Weg, mit zwei Seelen-Begleitern im
    Kader oder wenn er genug LP für zwei Verluste hat.

*Falsifikation:* Kein Defekt, weil das erste Brett zweimal ohne Tod fiel? Das gelang mit Gegenständen und Zähnefletschen
von seiner Hand; die Meisterbretter haben Rang 20 und 25 und Gegner ohne Schwäche. Option falsch? K1 ohne gemessenen
Zuschlag ist wirkungslos, aber nicht falsch; K2 kann ein Horn umsonst halten, wenn ein gerufener Gegner nie erscheint
oder der Kampf vorher endet. Gehalten wird deshalb nur, solange ein anderer Begleiter draußen steht oder ein anderes Horn
bereit ist; ist das gehaltene das einzige und keiner draußen, ruft RSR es: Ohne Begleiter fehlen Dressur, Kombo und
Heilung sofort, der Zuschlag gegen die Adds später nur vielleicht. Ausgeliefert, und nichts ändert sich? Liest Lumina die XBM-Spalten
anders als das Schema (wie bei `XBMPet`, Spalte `Unknown4`), fehlen Schwäche und Gegnerliste still; RSR schreibt deshalb
beim Betreten, ob es die Daten des Bretts gelesen hat.

*Stufen* (alle gebaut): 1. Daten zur Laufzeit (Schwäche und Gegnerliste je Kampf, Schadensart je Begleiter) mit K1, K2
und K3; 2. K11 (Bedarfsliste und LP je Begleiter) mit K9, Stufe 3 A643; 3. K8; 4. K6 und K7; danach K4, K5 (Stufe 2 A644),
K10 (Campsite A564, Laden A569, Weghinweis A632). Offen ist nur die Prüfung der Bretter 2 bis 5 an seinem ersten Lauf
dort (TODO, „Warten auf Daten": „Beastmaster: Bretter 2 bis 5 am ersten Mitschnitt prüfen").

**Stufe 1, erster Teil: Daten und K2** (A548, gebaut; K1 und der Rest von K3 folgen):
- *Kämpfe aus dem Blatt:* `XBMBattleDetail` hat eine Zeile je Kampf und eine Unterzeile je Gegner. Gelesen werden der
  Name (`BNpcName`) und die Schwäche (`XBMElement`). Die Reihenfolge der Spalten belegt EXDSchema, Zweig latest
  (abgerufen 05.11.2026, Werkzeugquelle): Name, Icon, Action1, Action2, Resist, Strength, Intelligence, PhysResistance,
  MagResistance, Constitution, Weakness. Die gebaute Lumina liest sie als `Unknown0` bis `Unknown10`; ihre benannte
  Fassung (`Experimental`) ist als vorläufig gesperrt (PendingExcelSchema), wie bei `XBMPet`.
  Probe: Der Knochen-Ritter (Zeile 1, Unterzeile 0) trägt 7 = Schlag, wie oben.
- *Der laufende Kampf:* Im Brett und im Kampf ist es die Zeile, die die meisten anwesenden Gegner nennt (nach `NameId`),
  bei Gleichstand die mit den wenigsten Gegnern. Nennt eine alle, ist es also sie; ein Anwesender, den kein Blatt
  führt (ein Helfer mit eigenem Namen), verdeckt den Kampf nicht. Wer beim Erkennen anwesend ist, gilt als „zu Beginn"; wer in der Zeile steht und
  in diesem Kampf noch nicht zu sehen war, als „kommt noch" – ein Gekommener, der fällt, kommt nicht wieder. Nach dem
  Kampf wird neu erkannt.
- *Was ein Begleiter trifft:* Dressur und Entfesseln (`FamiliarProfile`) je mit ihrem Element (`Action.Aspect` Feuer bis
  Wasser) oder, bei physischem Aspekt, ihrer Angriffsart (Hieb, Stich, Schlag). Zugeordnet wird über die englischen Namen
  von `XBMElement` und RSRs Aufzählungen `Aspect` und `AttackType`, ohne eigene Zahl. Ein Zauber ohne Element trifft
  keine Schwäche.
- *Selbstprüfung beim Betreten:* Eine Zeile „crucible: … fights … read" nennt, wie viele Kämpfe und Gegner gelesen sind,
  und als Probe die Schwäche des ersten Gegners. Fehlt sie oder steht dort 0, ruhen die Regeln dieser Stufe, und die
  Zeile sagt es. Erkennt RSR einen Kampf, nennt eine Zeile die Anwesenden und die Kommenden mit ihrer Schwäche.
- *K2, gebaut:* In der Hornwahl nach Lage (B3, unter „familiars by the situation") wird ein bereites Horn im Zyklus
  übersprungen, dessen Begleiter die Schwäche eines kommenden Gegners trifft und keine der Anwesenden. Das gilt nur,
  solange ein anderes, nicht gehaltenes Horn bereit ist; sonst kommt er wie bisher (Falsifikation oben). Erscheint ein
  Kommender, dessen Schwäche ein bereiter Begleiter trifft, wird dieser zuerst gerufen – nach Gefahr (B1) und dem
  Aufhalten eines Rudels (C1). Eine Zeile nennt das Halten einmal je Kampf und Horn und den Ruf.
- *Im Kampf:* Seelenlose (Stich), Raptor auf einem Horn (Entfesseln Feuer), Sukkubi (Feuer) nach 45 s. Der Raptor
  bleibt im Zyklus zurück, solange ein anderes Horn bereit ist, und kommt, sobald die Sukkubi erscheinen.
- *Beim Anziehen (A581; sein Hinweis vom 05.11.2026: „raptor für feuer, wo noch keine adds waren, die gegen feuer
  allergisch sind"):* Der erste Begleiter kommt mit dem Anziehen, Sekunden bevor das Spiel den Kampf meldet; bis dahin
  war kein Kampf bekannt, und K2 hielt nichts zurück – am 05.11.2026 kam der Raptor um 21:05:38, der Kampf begann
  21:05:42, die Sukkubi 21:06:34. Jetzt gilt bis zur Meldung der Kampf, den das Hornfenster zuletzt nannte
  (`CrucibleBoard.Upcoming`), mit den Gegnern, die gerade in der Gegnerliste stehen (`PresentNow`); wer aus seinem Blatt
  dort fehlt, kommt noch. Ohne Hornfenster (kein Kampf genannt) bleibt es wie vorher.

**Stufe 3: K8, Gegenstände des Bretts** (A551 bis A555, gebaut). Zwei Einstellungen, beide ab Werk an: „Use the
board's items that keep you and your familiar alive (Crucible only)" und „Use the board's damage items and buffs: damage
on a pack once a fight, all of them in the final fight (Crucible only)".
- *Welche* (`XBMItem`, xivapi Schema a1e24a0c, 05.11.2026; Name in Spalte 2, englischer Wirktext in Spalte 3, daraus Art
  und Größe; die deutschen Namen aus demselben Blatt mit `language=de`, Zeilen in `action_names_de.json`):
  - Heilung: Bestien-Heiltrank I bis IV („Restores 10/23/36/50% of HP to self or familiar") und Bestien-Pulver I bis III
    („… to self and nearby allies").
  - Bestien-Lebensfeder I und II („Grants Reraise to self … 70/95% chance of automatic revival upon KO", keine Dauer genannt).
  - Sand der Zeit („Grants Sandstill to self … Returns you to the moment combat commences upon incapacitation", nur
    außerhalb des Kampfes).
  - Bestien-Heiltranksammlung („Auto-potion: Restores 40% of HP automatically when HP falls below 50%", 150 s).
  - Bestien-Gegengift und Bestien-Augentropfen (heilen Gift und Blindheit und 25 % LP).
  - Schutz: Bestien-Verhärtungsmittel („Grants Tough Skin to self or familiar … Reduces damage taken by 20%", 60 s),
    Grimoire der Unantastbarkeit („five stacks of Blink … Nullifies most physical attacks", 120 s), Grimoire der
    Reflexion („Grants Reflect … Reflects most magic attacks back at the caster", 30 s).
  - Gegen ein Element: die Bestien-Feuer-, -Wasser-, -Erd-, -Blitz-, -Wind- und -Eisresistenzmittel I und II („Grants
    Through Fire to self and nearby allies … 50/70% chance to absorb fire damage as HP", 15 s).
  - Für den Begleiter: Bestien-Goldnadel („Instantly cures most instances of petrification for familiar, restoring 50% of
    maximum HP … Additionally grants Stoneskin", 50 % seiner LP, 60 s).
  - Mehr LP: Bestien-Ausdauermittel („Grants Liquid Constitution while also restoring 10% of HP to self or familiar …
    Increases maximum HP by 20% Effect ends upon combat completion").
  - Stärkung: Bestien-Stärkungs-, -Zauber-, -Aufbraus-, -Wendigkeitsmittel („Grants Liquid Strength/Magic/Intensity/
    Evasion to self or familiar": physischer oder magischer Schaden +30 %, kritische Trefferquote +30 %, Ausweichen
    +25 %, 15 bis 30 s), Bestien-Verkürzungsmittel (Hasty Spirit, Wirk- und Abklingzeiten −15 %, 60 s),
    Bestien-Flinkheitsmittel (Spirit of the Breathtaker bis Kampfende), Bestien-Feder (100 TP),
    Bestien-Blutsaugerserum (Life Siphon, 10 % des Schadens als LP, 60 s). Die Reißzähne und Sternensturm sagen:
    „Damage is affected by magic damage increase effects of other beast gear, items, or abilities".
  - Zustände: Bestien-Heilmittelsammlung („Grants Auto-remedy to self … Automatically cures the next status ailment
    suffered", 240 s) und die Seren I und II (Bestien-Antigift-, -Antiparalyse-, -Antiblind-, -Antiversteinerung-,
    -Antischlaf-Serum: „Increases poison/paralysis/blind/petrification/sleep resistance by 100%", 180 s; II auch für
    Verbündete in der Nähe, 300 s).
  - Schaden: die sechs Reißzähne (Feuer 2.500, die übrigen 2.000 Potenz auf das Ziel und alle Gegner in 12 Yalm; dazu je
    Element Deep Freeze, Paralysis, Heavy, ein entfernter Stärkungszustand oder 15 Yalm Rückstoß), der
    Blutsauger-Reißzahn (Stich, 1.500 bis 3.300 nach deinen maximalen LP, teils als LP aufgenommen) und Sternensturm
    (Feuer 3.000 in 18 Yalm). Art, Potenz, Radius und Rückstoß liest RSR aus dem Text, das Element ordnet es über die
    englischen Namen von `XBMElement` der Schwäche zu (wie K1).
- *Der Endkampf:* Unterzeile 0 jedes Bretts in `XBMContentBattle` (`Unknown0`) ist der Kampf gegen den Endgegner: Zeilen
  6, 13, 21, 31, 45 mit Seelenlose, Losfrox Tintenklecks, Guttler, Borgny Giftschleuder, Lauda Spruchspalterin (xivapi,
  05.11.2026); die übrigen Unterzeilen sind die Kämpfe davor. Brett-Gegenstände gehen nach dem Lauf verloren, höchstens
  zehn passen ins Inventar (consolegameswiki, „Crucible of the Unbroken", Abschnitt „Ephemeral Items", 05.11.2026). Im
  Endkampf zählt der Vorrat deshalb nicht mehr.
- *Die Zustände eines Kampfes:* `XBMBattleDetail` führt je Gegner zwei Aktionen (`Unknown2`, `Unknown3` →
  `XBMBattleDetailAction`: Aktion, Status, Zielart, Wirkart als `Unknown0` bis `Unknown3`; Spaltenfolge EXDSchema latest,
  05.11.2026). Über alle Bretter verursachen sie unter anderem Paralysis, Poison, Doom, Petrification, Blind, Confused,
  Heavy, Hysteria, Nightmare, Stone Curse, Devoured und Down for the Count. Als gefährlich zählt ein schädlicher Status
  (`StatusCategory` 2), der dir die Kontrolle nimmt (`LockActions` oder `LockControl`: Petrification, Hysteria, Nightmare,
  Confused, Devoured, Down for the Count, Stone Curse) oder dessen englischer Wirktext den Tod nennt (Doom, Stone Curse).
  Ob die Bestien-Heilmittelsammlung einen davon heilt, sagt keine Quelle („status ailment", unbelegt).
- *Wann, einer zur Zeit, in dieser Reihenfolge* (erste Einstellung, außer 12 und 13):
  1. Im Kampf unter „Use HP potions below this HP": der Heilgegenstand, der dich mit dem kleinsten Wert wieder über die
     Linie hebt, sonst der größte; bei gleicher Heilung der Heiltrank vor dem Pulver. Normale Heiltränke sperrt das Brett.
     Die LP liest RSR wie beim Heiltrank: jetzt und, da im Brett kein Heiler lebt, beim nächsten Webfenster einen GCD
     voraus (Konzept 03, „Die Schwelle nach der Vorhersage, wo kein Heiler lebt", A641).
  2. Im Kampf gegen Gift oder Blindheit auf dir das passende Mittel.
  3. Steht kein Reraise, die stärkere Bestien-Lebensfeder, im Kampf und außerhalb. Ihr Wirktext nennt keine Dauer, also
     gilt sie bis zu einem Fall. Geht Reraise, ohne dass du fielst, schreibt die Spur das, und von da an kommt eine
     Lebensfeder nur noch gegen einen Boss.
  4. Außerhalb des Kampfes, solange kein Sandstill steht, der Sand der Zeit.
  5. Gegen einen Gegner mit Boss-Symbol, solange kein Auto-potion steht, die Bestien-Heiltranksammlung.
  6. Schutz für dich gegen einen angekündigten Treffer auf dich: ein bekannter Tankbuster (Liste, gelernte Tabelle,
     Markierung, BossModReborn mit dir in der Maske; `DataCenter.IsTankbusterAimedAt`) oder ein gemessener gezielter
     Treffer, der eine Abwehr verlangt (A540). Im Endkampf auch, sobald ein Gegner dich angreift. Gegen einen
     physischen Treffer (`AttackType` 1 bis 4) das Grimoire der Unantastbarkeit, gegen einen magischen (5) das der
     Reflexion, sonst oder wenn es fehlt das Bestien-Verhärtungsmittel; nicht, solange Tough Skin, Blink oder Reflect
     auf dir steht.
  7. Für deinen Begleiter zuerst Heilung (A577; seine Meldung vom 05.11.2026: im Kampf kein Gegenstand, der die Begleiter
     heilt – die Einstellung verspricht „keep you and your familiar alive"): Hält er nicht mehr LP als er bis zum nächsten
     Webfenster verlieren kann – der größte Treffer, den er in diesem Kampf nahm (dasselbe Maß wie K9), und, solange die
     Gegner ihn angreifen, sein gemessener Verlust je Sekunde beim Halten (K9, A610) über einen GCD Vorlauf, das Größere
     (A642) –, vor dem ersten gemessenen Treffer unter „Use HP potions below this HP" als Anteil seiner LP, der Bestien-Heiltrank („to self or familiar"), der ihn mit dem kleinsten Wert über
     diese Marke hebt, sonst der größte. Das Bestien-Pulver nicht: Es heilt dich und die Verbündeten in deiner Nähe, und
     der Begleiter steht am Gegner. Nie der letzte Heilgegenstand: Er bleibt dir, denn deine LP sind der Lauf, seine ein
     Horn. Danach derselbe Schutz wie in 6 gegen einen angekündigten Treffer auf ihn, wenn er nicht mehr LP hält als
     er bis zum nächsten Webfenster verlieren kann; im Endkampf gegen jeden angekündigten Treffer auf ihn.
     *Warum der Verlust je Sekunde* (A642): Halten ihn vier Gegner mit je 100 alle 3 s, verliert er rund 130 LP je
     Sekunde; sein größter Treffer bleibt 100, und bei 400 LP fiele er im nächsten GCD, ohne dass der größte Treffer es
     anzeigt. Herausfordern liest dieselbe Rate schon (K9); bleibt es aus, weil du das Fenster nicht hieltest, ist der
     Trank der Rest. Eine Rechnung, nicht gemessen; auf Brett 1 fiel kein Begleiter.
     *Früher heilen?* Seine Frage vom 07.11.2026 zu Ziz (264/487) und Golem (439/550) auf den Hörnern: „warum werden
     die nicht vorab geheilt im kampf?" In seinem Kampf 4 bekam Golem bei 118 LP den Bestien-Heiltrank III (Spur
     15:58:53); Ziz blieb über seinem Verlust und bekam keinen. Ein früher Trank heilt dieselbe Menge, solange er nicht
     überheilt, denn die LP tragen sich über das Brett; er kostet den Trank früher, und den Vorrat brauchst du. Offen
     (TODO „Angeschlagene Begleiter auf den Hörnern").
     *Gegenthese 1 (Vorrat):* Jeder Trank für ihn fehlt dir später. Widerlegt bis auf den Rest: Der letzte bleibt dir,
     und Schritt 1 kommt vor diesem; es bleibt, dass ein früher Trank für ihn einem späteren Kampf fehlt, in dem du ihn
     selbst bräuchtest – Preis: ein Trank je Fall, Wahrscheinlichkeit nach der Zahl der Kämpfe bis zum nächsten Laden.
     *Gegenthese 2 (Dauerfeuer):* Trifft ein Gegner ihn ohne Pause, ginge Trank um Trank in ihn. Teilweise widerlegt: Ein
     Trank hebt ihn über seinen größten Treffer, der nächste kommt erst, wenn er wieder darunter fällt; der letzte bleibt
     dir; Provozieren (K9) nimmt ihm die Treffer am selben Maß.
     *Gegenthese 3 (er fällt trotzdem):* Ein Treffer größer als jeder bisherige fällt ihn auch nach dem Trank. Nicht
     widerlegt; das Maß lernt mit jedem Treffer dazu, und der Schutz danach greift bei angekündigten Treffern.
  8. Gegen die Zustände des Kampfes, sobald er erkannt ist und das Blatt für einen seiner Gegner einen gefährlichen
     Status führt: zuerst das Serum, dessen Text diesen Status nennt (je Status einmal im Kampf), sonst die
     Bestien-Heilmittelsammlung, solange kein Auto-remedy steht.
  9. Gegen einen Treffer mit Element: Ist ein Treffer auf dich angekündigt (wie 6) oder erreicht dich ein Flächentreffer
     (`DataCenter.AreaHitReachesPlayer`), und trägt ein laufender Cast dabei ein Element (`CrucibleBoard.ElementsOf`,
     wie K1), das Resistenzmittel dieses Elements mit der höheren Chance, solange sein Status nicht steht. Es wirkt
     15 s, der Cast landet innerhalb seiner Wirkzeit.
  10. Ist dein Begleiter versteinert (ein Status mit „Petrification" im englischen Namen), die Bestien-Goldnadel auf ihn.
  11. Gegen einen Gegner mit Boss-Symbol, solange kein Liquid Constitution steht, das Bestien-Ausdauermittel auf dich.
  12. Stärkung (zweite Einstellung), nur im Endkampf: jedes Mittel, das dir einen Status gibt („Grants … to self"),
      solange er nicht steht, auf dich; vor den Schadensgegenständen, weil eine Steigerung des magischen Schadens die
      Reißzähne mitträgt. Außerhalb des Endkampfs bleiben sie liegen: Ihr Wert für einen späteren Kampf ist nicht
      bekannt (K11), im Endkampf ist er null.
  13. Schaden (zweite Einstellung) auf dein Gegnerziel: im Endkampf jeder Schadensgegenstand, der wertvollste zuerst;
     sonst einer je Kampf, sobald er mehr Gegner trifft als dein Ziel. Wert ist Potenz mal der Summe über die
     getroffenen Gegner aus eins plus dem gemessenen Schwächezuschlag, wo das Element ihre Schwäche ist (K1); bei
     Gleichstand der ohne Rückstoß. Ein Gegenstand, dessen Element nur die Schwäche noch kommender Gegner trifft, wartet
     auf sie (wie K2).
- *Wie:* `InstanceContentCrucibleOfTheUnbroken.UseItem` (FFXIVClientStructs, in Dalamuds Fassung vorhanden) wirkt auf
  das Soft- oder Hard-Ziel. RSR setzt das Soft-Ziel kurz auf dich, deinen Begleiter oder dein Gegnerziel und stellt es
  zurück, wie BeastHelper (be40a1e, Werkzeugquelle, nur gelesen). Die Status liest RSR nach ihrem englischen Namen. Die
  Reichweite eines Wurfs nennt kein Blatt; das Gegnerziel ist das, das RSR ohnehin angreift.
- *Nachweis zur Laufzeit:* Nach dem Einsatz wartet RSR, bis der Platz einen anderen Gegenstand zeigt, höchstens einen GCD.
  Bleibt er gleich, schreibt die Spur „not taken" und versucht diesen Platz in diesem Kampf nicht mehr. Ob ein Gegenstand
  gerade gesperrt ist, prüft das Spiel über eine Funktion, die FFXIVClientStructs nicht führt; RSR prüft stattdessen die
  Animationssperre und den Verbrauch.
- *Selbstkorrektur der Sammlung:* Steht ein gefährlicher Status des Kampfes länger als einen GCD auf dir, während
  Auto-remedy steht, heilt die Sammlung ihn nicht. Die Spur nennt ihn, und gegen einen Kampf, dessen gefährliche Status
  alle so erkannt sind, kommt keine Sammlung mehr.
- *Gegenthesen:*
  - Gegenstände sind ein Vorrat für das ganze Brett. Teilweise widerlegt: Heilung erst an deiner Heiltrank-Linie und die
    kleinste passende zuerst; Reraise und Sandstill sind Versicherung, die nur vor dem Fall zählt; Schutz nur gegen
    angekündigte Treffer; im Endkampf ist der Vorrat wertlos.
  - Schutz gegen jeden Angriff statt nur gegen angekündigte: Solange der Begleiter die Feindseligkeit hält (K9, ab Werk
    an), trifft dich ein Tankbuster selten, und jeder Gegenstand wirkt einmal. Nicht widerlegt bleibt der Preis: Ein
    unangekündigter großer Treffer findet keinen Schutz.
  - Der Schutz für den Begleiter fehlt dir später. Er geht nur, wenn sein Fall droht; dann ziehst du die Feindseligkeit
    ohnehin auf dich (K9), sein Fall wäre sicher, dein späterer Bedarf ist es nicht.
  - Blink kann von den Angriffen anderer Gegner verbraucht werden, bevor der angekündigte Treffer landet. Ein wirkender
    Gegner greift nicht automatisch an; mehrere Gegner auf dich können Stapel kosten. Preis benannt.
  - Die Sammlung heilt den nächsten Zustand, auch einen harmlosen (Heavy). Nicht widerlegt; Preis: eine Sammlung. Das
    Serum ist für seinen Status sicher und geht deshalb vor.
  - Ein Schadensgegenstand je Kampf vor dem Endkampf ist eine Setzung: Wie viele Kämpfe noch kommen, liefert das Spiel
    nicht (K11). Sie folgt seinem eigenen Spiel am 04.11.2026 um 19:04, ein Reißzahn auf ein Rudel.
  - Ausgeliefert, und nichts ändert sich: Hält er keine dieser Gegenstände, oder erkennt RSR den Kampf nicht (Zeile
    „crucible fight …" fehlt), ruhen 8, 9, 12 und 13 und der Endkampf; die Spur zeigt beides.
  - Das Resistenzmittel hilft nur, wenn der Cast das Element trägt, das er anzeigt; ein Treffer, den nur BossModReborn
    ankündigt, nennt keines, dann wartet es. Preis benannt.
- *Nicht in diesem Teil:* Die „Ungezähmten" Mittel (Ungezähmte Bestien-Stärkungsmittel und verwandte) legen dir selbst
  Stun, Nightmare, Petrification, Blind oder Pollen auf – RSR setzt sie nie ein (Sicherheit). Das Horn der Rückkehr
  belebt einen gefallenen Begleiter nur auf dem Brett, außerhalb eines Kampfes; welcher gefallen ist, könnten die LP je
  Begleiter in `AgentXBMPetParty` zeigen – ob sie im Brett gefüllt sind, misst die Sonde aus K11 (A556). Bestien-Rauchbombe (Flucht über das Brett-Fenster), Auge des Diebs und des
  Händlers (Beute und Marken, keine Wirkung im Kampf): kein Einsatz durch RSR. Grimoire der Zauberschmiede und des
  Stahlstachels machen allen Schaden magisch oder physisch, auch den der Begleiter; ob das gegen einen Gegner hilft,
  hängt an seinen Resistenzen (`PhysResistance`, `MagResistance` im Blatt, Bedeutung der Zahlen unbelegt) – kein
  Einsatz, bis das belegt ist.

**Stufe 4: K6, Hörner im Hornfenster** (A561, A562, gebaut):
- *Wann:* Das Hornfenster ist offen (`AgentXBMPetParty` aktiv, `Mode` 2), außerhalb des Kampfes, die Einstellung „Set
  the battlehorns between fights for the next one" (B5) ist an; ihr Text nennt das Crucible jetzt. Ob RSR an ist, zählt
  nicht: Bis A576 lief die Wahl nur bei eingeschaltetem RSR, und RSR war vor dem Hornfenster stets schon aus (30 s nach
  dem Kampf, Gebietswechsel); die Hörner blieben, wie sie standen (seine Meldung, 05.11.2026). Dasselbe galt für K7, die
  Rast und den Laden; alle laufen jetzt als Dienst außerhalb des Kampfes (Konzept 03, „Vierte Invariante").
- *Spur:* Kann RSR im offenen Hornfenster den anstehenden Kampf oder den Kader nicht lesen, schreibt es einmal je
  Fenster „Crucible horn screen: … not readable"; die Hörner bleiben dann, wie sie stehen.
- *Eingaben:* der Kader in Fensterreihenfolge mit LP (`SelectedPets`, `PetCurrentHealth`, `PetMaxHealth`), die gewählten
  Plätze in Hornreihenfolge (`SelectedPetIds`), der anstehende Kampf (erste Kampfzeile der Feldliste, Art 3) und aus
  `CrucibleBoard` dessen Gegner mit Schwäche, ihre Zahl und ob es der Endkampf ist.
- *Wahl:* B5s Optionen und Wertung, je Horn mit dessen Einstellung für One with Nature, über den lebenden Kader:
  Endkampf wie ein Boss (zwei, die heilen oder schützen), Rudel nach der Zahl der Gegner aus dem Blatt, Schaden mal eins
  plus Schwächezuschlag mal Anteil der Gegner, deren Schwäche der Begleiter trifft (K1; vor der ersten Messung null),
  nach Gefahr im Brett einer mehr, der heilt oder schützt (B5); die schon gewählten zählen als „behalten". Als letzter
  Gleichstandsbrecher die Summe der LP-Anteile (Daedalus, Feldnotiz 30.10.2026: drei angeschlagene Begleiter auf allen
  Hörnern, der Beastmaster tankte und fiel), danach die Summe der synchronisierten Ränge (A579: der stärkere bei
  sonst gleicher Wahl); außerhalb des Crucible sind beide stets gleich, B5 ändert sich dort nicht. Ein Begleiter unter
  dem empfohlenen Rang des Bretts kommt nur in Frage, wenn weniger andere leben als Hörner da sind (A579, K7 „Leveln").
  Spurzeile „Crucible horn screen: horns set for battle …".
- *Klicks:* Rückruf des Fensters mit `[1, Platz]`, einer, dann neu lesen. Ist die Auswahl kein Anfang der gewünschten
  Reihenfolge, wird der zuletzt gewählte abgewählt, sonst der nächste gewünschte gewählt; höchstens doppelt so viele
  Klicks wie dreimal die Plätze im Kader, dann eine Spurzeile und Schluss. Ändert er die Auswahl selbst, bleibt sie für dieses
  Fenster. Den Kampf startet er.
- *Bereit (A580):* RSR liest und klickt erst, wenn das Fenster bereit ist: der Feld-Agent hat seine Einträge gebaut
  (`AgentXBMStageDetailList.AddonInitState` 2), der anstehende Kampf ist lesbar, das Fenster sichtbar. Vorher zeigt das
  Spiel noch die Hornplätze des vorigen Kampfes und leert sie Augenblicke später: Am 05.11.2026 öffneten vier von fünf
  Hornfenstern so (Sonde: „built 1", keine Kampfzeile, „horn positions 2 9 0"; 55 ms später leer). RSR hielt das Leeren
  für seine Handänderung und setzte in diesen vier Kämpfen nichts. Eine Änderung zählt erst als seine, wenn RSR die
  Auswahl des bereiten Fensters gesehen hat, und ein Leeren vor RSRs erstem Klick ist das Spiel. Wird das Fenster nie
  bereit, schreibt RSR das beim Schließen einmal in die Spur. Die Handerkennung der Belegung im Dungeon (B5) schweigt im
  Brett: Dort leert das Spiel die Hörner nach jedem Kampf, und sie hielt das für seine Hand.
- *Gegenthese 1 (Absturz):* BeastHelper dokumentiert einen Absturz durch nachgebaute Fensterereignisse. Der Rückruf mit
  Werten ist ein anderer Weg; Daedalus nutzt ihn im Spielbetrieb seit 22.10.2026. Nicht widerlegt bleibt: Ein Patch
  kann die Werte des Fensters ändern; die Prüfung nach jedem Klick hält dann an.
- *Gegenthese 2 (seine Wahl):* Er setzt die Hörner vielleicht selbst. Eine Handänderung hält; die Einstellung schaltet es
  ab.
- *Gegenthese 3 (angeschlagene Begleiter):* Gleicher Wert ist selten; ob ein Begleiter den Kampf übersteht, beantwortet
  erst K11 mit dem gemessenen Verlust je Kampf (Sonde A556, A561). *Gemessen (06.11.2026, Brett 1, A634, A637):* Der
  Schaden entschied jeden Kampf: Gigantoad lag in drei von fünf Kämpfen auf einem Horn, Spriggan, Morbol, Lamb und
  Apkallu in je zwei; Dodo, Treant und Mantis standen bis zum Endkampf mit vollen LP, Dodo nie auf einem Horn. Gekostet
  hat das nichts: Der größte Verlust eines Einsatzes war 343 (Lamb in Kampf 5), jeder gewählte Begleiter stand bei der
  Wahl darüber, keiner fiel, und die vollen hätten weniger Schaden gebracht. Ungenutzte LP am Ende des Bretts kosten
  nichts, ein schwächerer Begleiter kostet Schaden in diesem Kampf. Zu seiner Frage, ob Vorschlag oder Nutzung falsch
  war: der Kader stimmte, die Nutzung kostete auf Brett 1 nichts; falsch war die Messung des Verlusts (A637, K11
  „Abtreten ist kein Fall"), und eine Lücke bleibt – ein Begleiter, der einen Einsatz nicht mehr übersteht (unten).
- *Übersteht den Einsatz* (A637): Liegen die LP eines Begleiters nicht über dem größten auf diesem Brett gemessenen
  Verlust eines Einsatzes, könnte er in diesem Kampf fallen. Er zählt dann nicht als einer, der heilt oder schützt –
  seine Heilung endet mit seinem Fall –, und unter sonst gleichen Plänen gewinnt der mit mehr Begleitern, die den
  Einsatz überstehen, vor dem Schaden. Vor der ersten Messung überstehen ihn alle; außerhalb des Crucible ändert sich
  nichts.
  - *Im Kampf vorher:* Am 04.11.2026 stand die Fledermaus bis zum vierten Kampf bei 25 von 550 LP und zählte weiter als
    Heilerin; ein Einsatz kostete auf Brett 1 bis zu 310 (A585). Sie wäre beim ersten Treffer gefallen, ihre Heilung mit ihr.
    *Nachher:* Ein anderer, der heilt oder schützt und den Einsatz übersteht, nimmt ihr Horn; gibt es keinen, steht sie
    als Letzte dort, denn die Hörner füllen sich weiter.
  - *Gegenthese (ausgeliefert, und nichts ändert sich):* Auf Brett 1 vom 06.11.2026 ändert sich nichts – jeder Gewählte
    überstand seinen Einsatz. Die Regel greift erst, wenn ein Begleiter so weit gefallen ist; dann fehlt ohne sie der
    Heiler, mit dem B2 rechnet. Den Bedarf späterer Kämpfe außer dem Endkampf (K11, Bedarfsliste) trägt sie nicht.

**Stufe 4, zweiter Teil: K7, Kader auf Wunsch** (A563, gebaut; Entscheidung C, A533):
- *Wann:* Das Teamfenster des Crucible ist offen – `AgentXBMPetParty` aktiv, nicht im Hornfenster (`Mode` 2), mit
  einem Brett in `ContentId` (Daedalus 74659a6, `CrucibleRosterLoader`, aus dem Spielbetrieb), und nicht auf einem
  Brett. Dann zeigt RSR ein Fenster „RSR – Crucible team" mit der Empfehlung und einem Knopf „Apply"; ohne Klick ändert
  es nichts. *Nicht auf dem Brett* (seine Meldung, 06.11.2026, A634): Der Kader steht für den Lauf fest, aber derselbe
  Agent ist dort an der Campsite (`Mode` 4), beim Füttern (3 bis 5) und zwischen den Kämpfen (6) aktiv; RSR bot den
  Wechsel um 17:35 und 17:38 an, das Spiel nahm ihn nicht („The game did not remove Treant"). Daedalus'
  `CrucibleRosterLoader` prüft das Brett ebenso wenig.
- *Empfehlung:* so viele Begleiter, wie das Brett fasst (`XBMContent.Unknown34`, EXDSchema TeamSize: 10, 12, 14, 12,
  15). Zuerst so viele, die heilen oder schützen (Trick heilt, Tempered Release heilt oder schützt, Borrow schützt), wie
  B5 vor einem Boss verlangt (zwei, `BossSafeFamiliars`); dann der Reihe nach der, der die meisten noch schwach
  abgedeckten Gegner trifft: Gewicht je Schwäche ist die Zahl der Gegner aller Kämpfe des Bretts mit ihr
  (`XBMContentBattle` → `XBMBattleDetail`), und jeder schon gewählte Begleiter mit derselben Schwäche teilt es
  (Gewicht durch eins plus die Zahl der schon Gewählten, die sie treffen). Bei Gleichstand zuerst, wer über dem Rang des
  Bretts steht (`IsPetOverSyncedRank`), dann wer schon im Kader ist. Ein Seelen-Begleiter für Soul Crush (K4) wartet darauf, dass RSR weiß, welche Kämpfe unterbrechbar zaubern.
- *Leveln (seine Vorgabe, 05.11.2026, A579):* „zum leveln der begleiter sollten die begleiter bei der anfangsauswahl des
  brettes bevorzugt werden, die hinter den anderen hinterherleveln. ausnahme: sie sind kein adäquater ersatz für die
  kämpfe aufgrund falscher eigenschaften." *Mechanik:* Jeder Begleiter im Kader eines Laufs bekommt Erfahrung, mit oder
  ohne Horn (Daedalus 74659a6, `CrucibleLevelPlan`, Werkzeugquelle aus dem Spielbetrieb, am Spiel nicht bestätigt); der
  Rang reicht bis 25. Am Brett zeigt das Spiel den Rang synchronisiert (`AgentXBMPetParty.GetSyncedRank`): Wer über dem
  Synchronrang des Bretts steht (`IsPetOverSyncedRank`), zeigt ihn, darunter den eigenen. Das Brett nennt seinen
  Synchronrang und den empfohlenen Rang (`XBMContent`, Spalten 36 und 35; der empfohlene ist der Wert minus 2, wie das
  Spiel rechnet – EXDSchema, und Brett 2 zeigt „Recommended Beast Rank: 6 (Sync from 10)" bei 8, Daedalus' Feldnotiz):
  erstes Brett 1 und 5, zweites 6 und 10, drittes 11 und 15, Erstes Meisterbrett 16 und 20, Zweites 21 und 25 (xivapi,
  05.11.2026).
  *Regel:* Nach der Wahl oben tauscht RSR, vom höchsten Rang abwärts, jeden Gewählten gegen den Begleiter mit dem
  niedrigsten Rang außerhalb des Kaders, der ihn **adäquat ersetzt**: Er trifft jede Schwäche, die im Kader nur der
  Gewählte trifft; er heilt oder schützt, wenn der Gewählte das tut und sonst zu wenige blieben; und er steht mindestens
  auf dem empfohlenen Rang des Bretts. Getauscht wird nur nach unten: Der Ersatz liegt im Rang unter dem Gewählten.
  „Falsche Eigenschaften" sind damit die fehlende Schwäche, die fehlende Heilung oder der Schutz, und ein Rang unter der
  Empfehlung des Spiels. Begleiter auf Rang 25 lernen nichts mehr und tauschen nicht ein. Der Grund steht in der
  Empfehlung („levels: rank …").
  *Einstellung:* „Prefer familiars that lag behind in rank for the Crucible team", ab Werk an (seine Regel; wer den
  stärksten Kader will, schaltet sie ab).
  *Gegenthese 1 (schwächer im Kampf):* Ein Begleiter auf Rang 2 hat auf dem ersten Brett weniger Werte als einer auf 5.
  Teilweise widerlegt: Er tauscht nur ein, wo er mindestens auf der Empfehlung des Spiels steht; welche drei kämpfen,
  wählt K6 je Kampf, und K6 nimmt einen Begleiter unter der Empfehlung nur, wenn nicht genug andere leben, und bei
  Gleichstand den höheren Rang. Es bleibt: Zwischen Empfehlung und Synchronrang kämpft ein schwächerer Begleiter, wenn
  er der einzige ist, der eine Schwäche trifft – Preis: weniger Schaden und LP dieses Begleiters in diesem Kampf.
  *Gegenthese 2 (Erfahrung nur im Kampf):* Gibt das Spiel Erfahrung nur für eingesetzte Begleiter, levelt der Tausch nur,
  wenn K6 ihn aufs Horn nimmt. Nicht widerlegt, unbelegt; der Kader bleibt in jedem Fall so geeignet wie vorher.
  *Gegenthese 3 (Rang über dem Synchronrang unbekannt):* Zwei Begleiter über dem Synchronrang sind am Brett nicht zu
  unterscheiden. Nicht widerlegt: Unter ihnen tauscht RSR nicht; ein solcher ist für die Regel nie der Rückständige.
- *Umsetzen:* erst die Abgänge, dann die Zugänge, je ein `TogglePet`; danach wartet RSR, bis der Kader sich ändert,
  höchstens einen GCD. Nimmt das Spiel einen Zugang nicht, fällt er für diese Sitzung aus der Empfehlung, und es geht
  weiter; geht ein Abgang nicht, hört RSR auf. Jede Stufe steht in der Spur („crucible team: …"); bestätigen und
  schließen tut er das Fenster selbst.
- *Gegenthese 1 (Wahl nur über Schwächen):* Schaden je Begleiter und sein Rang fehlen. Teilweise widerlegt: Der Rang
  bricht Gleichstand, der Schaden im Kampf entscheidet B3 und K6 unter den Gewählten; nicht widerlegt bleibt, dass ein
  schwacher Begleiter mit passender Schwäche einen starken verdrängen kann. Darum nur auf Klick.
- *Gegenthese 2 (Absturz durch Spielfunktionen):* `TogglePet` ist die Funktion des Fensters selbst, Daedalus ruft sie im
  Spielbetrieb; RSR ruft sie nur auf Knopfdruck und nur bei offenem Teamfenster.

**Stufe 1, zweiter Teil: K1 und der Rest von K3** (A549, gebaut; Daten in `CrucibleBoard`):
- *K1, Zuschlag gemessen* (A549, neu gefasst A582): Keine Quelle nennt ihn; consolegameswiki, Eorzean Tavern,
  fullboosts und mmogah sagen nur, dass Schwächen zählen (05.11.2026). RSR misst ihn an jedem Treffer von dir und deinen
  Begleitern auf einen Gegner des laufenden Kampfs, je Aktion und Brett.
  - *Was ein Treffer trifft, sagt der Treffer:* Das Spiel meldet je Treffer im Effekteintrag Element und Angriffsart
    (zweiter Parameter: oben das Element, Feuer 1 bis Wasser 6, Ohne 7; unten die Angriffsart, Hieb 1, Stich 2,
    Schlag 3, Magie 5; ECommons `EffectEntry.AttackType`, im ACT-Mitschnitt das dritte Byte der Flaggen). Das
    Aktionsblatt führt bei deinen Äxten und deinem Auto-Angriff keine Angriffsart; das Spiel trifft mit ihnen als Hieb
    (0x71 bei jedem Axthieb und jedem deiner Auto-Angriffe auf Brett 1, 05.11.2026). Bei Dressur und Entfesseln stimmen
    Blatt und Treffer überein (Frostiger Atem 0x25 Eis, Fauliger Atem 0x15 Feuer, Gnadenloser Prankenhieb 0x71 Hieb);
    die Auto-Angriffe der Begleiter sind eigene Aktionen mit eigenem Element (Cu Sith Stich, Raptor und Fliegenfalle
    Eis, Pugil Wasser, Riesenwurm Erde).
  - *Gezählt* werden Aktionen mit fester Potenz: einer einfachen im Wirktext (`ActionTooltip.TryPlainPotency`) oder
    Auto-Angriffe (Kategorie Auto-attack). Die Dressuren steigen mit dem TP des Begleiters und bleiben draußen.
  - *Verglichen* wird je Aktion der Schaden auf schwache Gegner mit dem auf andere derselben Abwehrstufe aus dem Blatt
    (`PhysResistance` bei Hieb, Stich und Schlag, sonst `MagResistance`). So fallen Stärke und Rang des Begleiters und die
    Abwehr der Gegner heraus. Die Abwehr wiegt: Derselbe Auto-Angriff des Schleims traf die Terrormilbe (Stufe 2) mit 57,
    die Seelenlose (Stufe 5) mit 37.
  - Der Zuschlag ist das Mittel der Verhältnisse, gewichtet nach der kleineren Zahl der Treffer.
  - *Bis zur ersten Messung auf dem Brett* gilt +27 %: gemessen in seinen Mitschnitten vom 04. und 05.11.2026 an 18
    Paaren gleicher Abwehrstufe, gewichtet 1,271 (Hiebe auf das Pisco-Dämonen-Ebenbild gegen die Knochen +26 bis +30 %,
    Schildansturm als Schlag auf die Knochen gegen das Pisco +28 bis +30 %, Eis der Fliegenfalle auf die Terrormilbe
    gegen den Oger +26 %; A582). Ein Rückfall, keine Spielangabe; die Messung des Bretts ersetzt ihn.
  - Eine Zeile nennt den ersten Messwert je Brett.
  - *Warum neu gefasst* (A582): Auf Brett 1 maß RSR am 05.11.2026 in fünf Kämpfen keinen Zuschlag. Die Äxte galten ohne
    Angriffsart, die Dressuren haben keine feste Potenz, die Auto-Angriffe keine im Text; übrig blieben einige
    Entfesseln, und keines traf beide Arten. Was den Zuschlag liest, lief ohne ihn: K1 in der Hornwahl, K6 auf dem
    Hornfenster, K3, die Reißzähne (K8) und die Flutwelle (A558). Im ersten Kampf belegte RSR die Hörner mit Raptor,
    Riesenwurm und Cu Sith gegen die schlagschwachen Knochen, obwohl der Büffel (Dressur Schlag, Entfesseln betäubt das
    Rudel) im Kader stand; in den Kämpfen 3 bis 5 rief es den Begleiter, der die Schwäche traf, zuerst (Cu Sith gegen
    das Pisco, Raptor gegen die Milben, Fliegenfalle gegen den Oger): Er bekam den kurzen Einsatz statt des langen.
- *K1 in der Hornwahl (B3):* Der Wert einer Dressur zählt jeden getroffenen Gegner mit dem Zuschlag auf den Anteil des
  Begleiterschadens, der seine Schwäche trifft (unten, „K1 über alle Schadensquellen"). Der passende Begleiter bleibt so
  länger draußen.
- *K3, Zeit bis zum Fall:* Seine LP geteilt durch eins plus Zuschlag mal Anteil. Der Anteil ist der Teil deines Schadens
  in diesem Kampf, den Aktionen mit seiner Schwäche brachten. Gemessen, ohne Annahme, wer mehr Schaden macht.
- *K3 vor der ersten Messung:* Hat noch kein Gegner getroffen, reiht RSR im Crucible nach dem größeren Wert aus Stärke und
  Intelligenz des Blatts geteilt durch die LP. Bei Gleichstand und außerhalb des Crucible gilt: wenigste LP zuerst. Die
  Blattwerte sind Stufen von 1 bis 4, kein Schaden; sie ordnen nur, bis die Messung da ist.
- *Gegenthese 1 (Abwehr):* Die Abwehr schwacher und anderer Gegner unterscheidet sich und verzerrt den Zuschlag.
  Widerlegt seit A582: Verglichen wird nur innerhalb derselben Abwehrstufe; die 18 Paare streuen von 1,21 bis 1,40,
  physisch wie magisch.
- *Gegenthese 2 (Rückfall):* Der Wert von Brett 1 (Stufe 30) gilt auf anderen Brettern nicht. Unbelegt; die Messung des
  Bretts ersetzt ihn, sobald eine Aktion einen schwachen und einen gleich gepanzerten anderen Gegner getroffen hat –
  deine Äxte tun das in jedem Brett, das einen hiebschwachen Gegner führt. Bis dahin verschiebt ein falscher Wert nur,
  welcher Begleiter den langen Einsatz bekommt und welcher Gegner zuerst fällt; Schutz und Heilung wählt B2 vorher.
- *Ausgeliefert, und nichts ändert sich?* Wählen die Hörner und das Ziel ohnehin den Schwächentreffer, ändert der Zuschlag
  nichts. Auf Brett 1 änderte er drei Rufe und eine Hornbelegung (oben). Fehlt auf einem Brett jedes gleich gepanzerte
  Paar, bleibt der Rückfall; die Zeile zum ersten Messwert fehlt dann.
- *K1 über alle Schadensquellen* (A586): Ein Begleiter schlägt mit vier Quellen zu – Auto-Angriff, Dressur,
  Entfesseln, Scheidender Schlag. Auf Brett 1 (04. und 05.11.2026, alle Begleiter zusammen, 74.200 Schaden) trugen die
  Auto-Angriffe 32 %, Scheidender Schlag 32 %, Entfesseln 19 %, die Dressur 18 % (je Begleiter: Auto-Angriffe 17 bis
  57 %, Dressur 7 bis 43 %). Bis A586 zählte der Wert eines Begleiters (B3, K6) die Schwäche nur über Dressur und
  Entfesseln, und K6 zählte sie dann für den ganzen Begleiter: Der Raptor (Dressur Eis) galt gegen die Milben voll als
  Schwächentreffer, die Fliegenfalle (Dressur Wind, Auto-Angriff Eis) gar nicht.
  - *Was gilt:* Der Zuschlag wirkt auf den Teil des Schadens, dessen Quelle die Schwäche trifft: Wert × (1 + Zuschlag ×
    Anteil). Anteil = Summe der Anteile der Quellen mit diesem Element, geteilt durch die Summe der Quellen, die der
    Begleiter auf diesem Horn hat (Entfesseln nur auf einem Horn, das One with Nature dafür verbraucht); Scheidender
    Schlag trifft nie (ohne Element). Die Anteile misst RSR an den Treffern aller Begleiter seit dem Laden; bis jede
    Quelle getroffen hat, gelten die Anteile von Brett 1.
  - *Auto-Angriff:* `XBMPet.Action` (Lumina `Unknown5`, neben `Pet`; EXDSchema latest: Description, TrickDescription,
    TemperedDescription, Icon, Pet, Action, …), erkannt als die Spalte, deren Zeilen alle auf einen Auto-Angriff
    zeigen. Sein Element zählt auch für K2 (wen der Begleiter trifft) und K7 (welche Schwächen der Kader deckt).
  - *Im Kampf (Brett 1):* Gegen die Milben (Eis) zählt der Raptor (Dressur Eis, Auto-Angriff Eis, Entfesseln Feuer) mit
    einem Anteil von 0,50 auf einem Entfessel-Horn, die Fliegenfalle (Auto-Angriff Eis) mit 0,32; gegen die Seelenlose
    (Stich) Cu Sith über seine Auto-Angriffe mit 0,32, vorher gar nicht.
  - *Gegenthese:* Die Anteile je Begleiter weichen stark ab (Schleim 57 % Auto-Angriff, Raptor 17 %). Nicht widerlegt;
    gemessen wird über alle Begleiter, weil je Begleiter die Treffer eines Bretts nicht reichen. Der Fehler verschiebt
    nur, wie stark eine Schwäche zählt, nicht ob.

---
