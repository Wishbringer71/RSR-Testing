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
- *Seit 7.58* (Patchnotes vom 10.11.2026, Lodestone):
  - Beim Verlassen des Crucible stellt das Spiel die Hornbelegung von davor wieder her. Draußen setzt B5 (Konzept 01)
    wie bisher vor dem nächsten Kampf.
  - Die Empfehlung des Spiels im Teamfenster behält schon gewählte Begleiter und füllt nur freie Plätze.
  - Das Teamfenster zeigt den synchronisierten und den ursprünglichen Rang und sortiert nach dem ursprünglichen. RSR
    liest den synchronisierten (`GetSyncedRank`, `IsPetOverSyncedRank`, K7); beide Funktionen haben neue Signaturen
    (FFXIVClientStructs 80cb4a7b7), gleiche Namen und Parameter.
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
- Punkte (Wiki, Spieldaten): Zuschläge für keine Gegenstände, keine Rast, keine Schwächung, kleine Teams, gefallene
  oder keine gefallenen Begleiter, volle LP am Ende. Sie stehen teils gegen deine Sicherheit. RSR wiegt jeden Bonus, den
  eine Handlung verwirkt, gegen ihren Sicherheitsgewinn (K15): Schutz, wo der Schaden wahrscheinlich ist, kein Schutz,
  der ohne Gefahr Punkte kostet.
  - *Sein Hinweis (11.11.2026):* „jedes brett hat nach dem endboss eine bewertung aufgrund von punkten, die man durch das
    verhalten dort sammelt. die ausbeute ist nicht optimal." Dazu: „bei den brettern gibt es punktebonis wenn man
    gewisse dinge macht oder sein lässt. nicht campt, nicht isst, weniger begleiter mitnimmt etc. recherchiere dazu".
    Bis A689 waren Punkte kein Ziel einer Regel (meine Ableitung); das kappte die Entscheidung, statt das Optimum zu
    verbessern (seine Vorgabe vom 07.11.2026). Gebaut in K15 (A689 bis A691, A695, A697) und K7 (Teamgröße A698,
    Gattung A699); offen ist die Rate der Rest-LP (TODO, Warten auf Daten).
  - *Was die Wertung bringt* (consolegameswiki, eorzeantavern, abgerufen 11.11.2026): Reste (faded und bright Remnants
    of Resilience) und Begleiter-EP anteilig zur Punktzahl, dazu Beastmaster-EP; neun Ränge von Apprentice bis
    Legendary, Legendary ab 17.875 Punkten auf den drei Unbroken-Brettern, ab 18.766 auf den Meisterbrettern. Die
    Saisonwertung (seit 7.57) zählt nur Läufe im dritten Grad mit „Participate in rankings". Wie viele Reste je Punkt,
    nennt keine Quelle (unbelegt).
  - *Grundpunkte* (consolegameswiki): je Gegnerfeld 250, je Elitefeld 800 (seit 7.57, vorher 625), Endgegner 1.000,
    Abschluss bis 5.000 (voll außer bei einem verlorenen Lauf), dazu die verbleibenden HP des Spielers absolut, nicht
    als Anteil.
  - *Boni* (beide Quellen gleich, wo nicht anders vermerkt; die Spieldaten `XBMContent.BonusPoints` bestätigen jeden Wert
    und entscheiden die beiden Abweichungen: Völlerei 1.500, Universalgenie 150/300/450; Wettrüsten, Sammelleidenschaft,
    Völlerei, Talerregen und Universalgenie vergibt Brett 1 nicht, A695): kein Begleiter gefallen 1.000 (sonst je nach Zahl der
    Gefallenen 750, 500, 250); Team aus 6–7 Begleitern 1.250, aus 4–5 2.500, aus 1–3 4.250; Spieler am Ende bei voller
    HP 250; alle Begleiter am Ende bei voller HP 1.000; keine Verwundbarkeit oder Schadensminderung erlitten 1.250;
    keine Ausrüstung erworben 750, zehn Teile gehalten 500 (nicht Brett 1); kein Gegenstand benutzt 750, zehn
    gehalten 1.000 (nicht Brett 1); nicht gefüttert 750, mindestens 20-mal gefüttert (nur drittes Brett und zweites
    Meisterbrett; 1.500 laut consolegameswiki, 750 laut eorzeantavern); keine Marken ausgegeben 500, mindestens 2.000
    gehalten 500 (nicht Brett 1); nicht gerastet 1.000 (seit 7.57, vorher 375); Team nur aus einer Gattung 750; erster,
    zweiter, dritter Grad 4.000, 6.250, 9.000; Universality 5-, 10-, 15-mal (nur zweites Meisterbrett; 150/300/450 oder
    250/500/750).
  - *Was RSR davon bewegt:* Rast (K10), Futter (K10), Kauf und Schatz (K13), Gegenstände (K8), Weg (Gegner- und
    Elitefelder bringen Punkte, Schatz und Lager keine), Teamvorschlag und Hornwahl (K7, K14: Teamgröße, Gattung),
    das Überleben und die HP der Begleiter und des Spielers am Ende. Nicht: die Grade (seine Wahl beim Betreten).

*Die Bretter* (`XBMContentBattle`, `XBMBattleDetail`; Schwäche in Klammern; deutsche Namen aus den Blättern):
- *Erstes Brett:* Knochen-Ritter und -Bischof (Schlag; der Ritter wehrt mit Forward Guard Angriffe von vorn ab, nicht
  die der Begleiter, Ossify unterbrechbar); Erzdämonen-Ebenbild (Blitz); Pisco-Dämonen-Ebenbild (Hieb; anfällig für
  Gift und Gewicht); Terrormilben- und Schattenmilben-Ebenbild (Eis; Tankbuster mit Gift); Oger-Ebenbild (Wind) mit
  Irrlicht- und Riesen-Irrlicht-Ebenbild (Wasser; anfällig für Fessel und Gewicht). Endgegner Seelenlose (Pas de Seul,
  Stich): ruft nach etwa 50 s (sein Mitschnitt vom 07.11.2026: in drei Läufen 48 bis 50 s nach dem ersten Treffer) eine
  Sukkubus-Magierin und zwei -Ritterinnen (Feuer; die Magierin stärkt ihn mit
  Fanaticism, 9 s Wirkzeit, anfällig für Unterbrechen).
- *Zweites Brett:* Manticore (Wind; Tankbuster); Taurus (Stich), ruft Ahriman (Eis; Blick mit Verhängnis); Wyvern
  (Blitz); Leerenmagus (Erde) mit Zombie (Eis); Älterer und Jüngerer Taurus (Feuer; der Jüngere stärkt); Dämon (Hieb),
  Böse Waffe (Eis), Eufel (Erde). Endgegner Losfrox Tintenklecks (Schlag) mit Töffelchen (Wasser).
- *Drittes Brett:* Kavalier (Feuer) mit Knochen-Bischof (Schlag); Ymir (Eis) mit Sahagin (Schlag, Dreadwash
  unterbrechbar); Montisaurus (Erde) mit Hahn und Henne (Schlag); Catblepus (Wind; Blick mit Versteinerung); Lakhamu
  (Wasser) mit Golem (Wasser, stärkt); Sirene (Stich, ruft Gegner; greift seit 7.58 während Unmooring Melody nicht mehr
  an) mit schlurfendem und kriechendem Ebenbild (Feuer);
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
  Schlag, Wasser; Heiler und Stärker); Behemoth (Erde) mit Riese (Blitz) und Blitz-Exergon (Erde); Königs-Ahriman-Ebenbild (Eis)
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
  Geprüft und verworfen (A660): Schaden an Begleitern nach ihrem Bedarf über das Brett gewichten – die Gefahr eines
  Gegners ist, was er austeilt, gleich wen es trifft; wer es nimmt, entscheidet K9 nach den LP und K11 nach dem Bedarf.
  Eine Umrechnung zwischen deinen LP und denen eines Begleiters gibt keine Quelle her, und unter Zähnefletschen trifft
  jeder Gegner denselben, die Reihenfolge änderte sich nicht. Stärkung anderer: Auf Brett 1 soll K4 die
  Fanaticism der Sukkubus-Magierin (Aktion 46928) unterbrechen, und A582 stellt sie nach dem Blatt vorn; die Größe einer
  Stärkung nennt kein Blatt. Eine Unterbrechung ist in keinem seiner Läufe belegt: In allen vier vom 07.11.2026 stoppte
  ihr Tod den Cast (16:13:12 nach 351 Schaden auf 37 LP; 18:11:50 und 18:46:09 ebenso, „Sonde" in K4, A665). *Was gilt:* Zuerst fällt der Gegner mit der größten Gefahr je
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
  *Gegner, die zu einem Verbündeten laufen* (A660; sein Hinweis vom 07.11.2026: „die irrlichter kommen von aussen aus
  vier richtungen und fliegen auf den mittigen oger zu. wenn sie ihn erreichen, entsteht schaden beim spieler. also
  muss der spieler zu den irrlichtern rennen. oder es muss ein aoe (falls vorhanden) gezündet werden, der alle
  irrlichter zusammen trifft, wenn sie gerade nah genug beieinander sind, aber noch weit genug weg vom oger"). *Im
  Kampf:* Der Oger von Brett 1 wird nach 52 bis 55 s unangreifbar (Burning Ward, 46918, Status 4175; BossModReborn
  27be422, Daedalus 2644cbe, Werkzeugquellen); je Welle kommen sechs Irrlicht-Ebenbilder (171 LP) und zwei Riesen-
  Irrlicht-Ebenbilder (1575 LP) aus vier Richtungen zur Mitte und explodieren dort (Verbrannte Erde, 46919 und 49728,
  60 y) – Schaden und Verwundbarkeit + bei dem, den sie treffen (Daedalus: in 17 Kämpfen bekam der Begleiter unter
  Deckung 14 von 15 Stapeln, ohne Deckung der Spieler 13 von 14; 14 Tode in 17 Kämpfen). Gemessen an seinen Läufen vom
  05., 06. und 07.11.2026: Ankunft 9 bis 25 s nach dem Erscheinen; in fünf Oger-Kämpfen starb kein Irrlicht. Am
  07.11. sprang RSRs Ziel 30 s lang zwischen ihnen (Spur 16:03:25 bis 16:03:55), jedes traf es einmal, und der Oger
  stand währenddessen unangreifbar bei 4532 LP.
  *Was gilt:* Ein Gegner läuft zu einem anderen Gegner, wenn er mit euch im Kampf steht (er hat ein Ziel seiner
  Feindseligkeit), keinen von euch ansieht und dem Hauptgegner näher kommt – um mehr als seine eigene Größe, und länger als
  einen GCD so beobachtet. Das Blickziel trennt ihn vom gewöhnlichen Angreifer, der auf dich zuläuft, während du am
  Hauptgegner stehst: Wer einen von euch angreift, sieht ihn an, sobald er losläuft; wer einem Skript folgt, sieht
  keinen von euch an. Gemessen an seinem Mitschnitt vom 11.11.2026: alle 24 Irrlichter aus drei Oger-Kämpfen ohne Blickziel, vom
  Erscheinen an mit dir oder dem deckenden Begleiter als Ziel der Feindseligkeit; alle 17 Gegnerarten, die an dem Tag
  im Crucible auf jemanden zuliefen und ihn angriffen, mit Blickziel spätestens eine Abfrage (unter 1 s) nach dem
  ersten Schritt, die Schattenmilben 5 s davor (`Character.LookAt`, FFXIVClientStructs 80cb4a7; im Mitschnitt das Feld
  PCTargetID, OverlayPlugin 43afdd5; Werkzeugquellen). Wer einmal einen von euch angesehen hat, gilt für den Rest
  des Kampfs als Angreifer. Der Hauptgegner ist der mit den meisten LP, der in diesem Kampf angreifbar war; er bleibt es,
  solange er lebt, auch unanvisierbar – der Oger steht unter Burning Ward unanvisierbar in der Mitte, und nur die
  Irrlichter sind anvisierbar. Wechselt der Hauptgegner, beginnt die Messung neu. Die Ankunft: verbleibender Weg
  geteilt durch die Annäherung der letzten ein bis zwei GCD – nicht seit dem Erscheinen, denn ein Irrlicht steht erst
  rund 2 s und läuft dann mit 1 y je Sekunde, und der Schnitt seit dem Erscheinen meldete die Ankunft rund 0,3 s zu
  spät. Wo der Weg endet und was dort geschieht, zeigt die erste Aktion eines Laufenden: ihr Abstand zum Hauptgegner
  gilt für seine Art (Irrlichter am 11.11.2026: viermal 3,83 bis 3,85 y von der Mitte, die Trefferkreise berührten
  sich erst bei 2,88 y), und sie zeigt, ob die Ankunft einen von euch trifft – dich, ein Gruppenmitglied oder deinen
  Begleiter unter Deckung. Gelernt je Gegnerart, solange du im Gebiet bleibst. Damit: (1) Unter gleich dringlichen Gegnern – gleiche
  Gefahr je LP, wie sechs gleiche Irrlichter – kommt der dir nächste zuerst, denn Laufweg ist verlorener Schaden; bei
  laufenden Gegnern holt so jeder Schlag einen, der dir entgegenkommt, statt dich zu einem zu schicken, der vorher
  ankommt. (2) Steht die erste Ankunft binnen der Zeit bevor, die Zähnefletschen braucht, bis die Deckung steht (ein
  GCD), und deckt dich der Begleiter nicht, kommt Zähnefletschen, sofern K9 es nicht verbietet (der Begleiter fiele):
  Die Explosion und ihre Verwundbarkeit treffen dann ihn, nicht dich. Ist der Ankunftsabstand der Art noch nicht
  gelernt, kommt Zähnefletschen, sobald der Gegner als laufend erkannt ist: Wie nah er heran muss, weiß bis zur
  ersten Ankunft niemand, und die Deckung hält 45 s, länger als der Marsch beider Wellen (Ankunft bis 25 s nach dem
  Erscheinen, die zweite Welle 16 s nach der ersten). Eine Art, deren Ankunft keinen von euch traf, löst kein
  Zähnefletschen mehr aus – das Pheromon der Manticore etwa schwebt 14 s und zerplatzt dann für sich
  (Sphärensplitterung, 15:22:26); trifft eine spätere Ankunft doch, gilt die Art wieder als treffend und bleibt es.
  (3) Flächen: Die vorhandenen Regeln zählen die
  laufenden Gegner wie alle; aus vier Richtungen liegen sie erst nahe der Mitte beisammen, und dorthin führt (1).
  *Nicht gebaut, mit Grund:* eine Fläche zurückhalten, bis mehr Irrlichter in Reichweite kommen – wie viele noch kommen
  und wann, sagt die Annäherung je Gegner nur für die schon sichtbaren, und eine gehaltene Fläche, die keinen weiteren
  trifft, kostet ihren Einsatz; die Flächenregeln feuern ab zwei in Reichweite. *Gegenthese 1:* Der Nächste ist bald
  angekommen, und der Schlag ist verloren. Teilweise widerlegt: Er kommt dir entgegen und ist in Reichweite, ein
  Irrlicht fällt in einem Schlag (171 LP); ein Riesen-Irrlicht braucht mehrere, und kommt es vorher an, war jeder
  andere Weg länger. *Gegenthese 2:* Zähnefletschen schickt den Begleiter in die Explosion. Gewollt: Seine LP sind ein
  Horn, deine der Lauf (K11), und K9 verbietet es, wenn ihn der nächste Verlust fällen würde. *Gegenthese 3:* Das
  Blickziel fehlt auch einem Angreifer einen Augenblick lang. Deshalb zählt ein Gegner erst nach einem GCD ohne
  Blickziel als laufend; die Irrlichter marschieren 19 s. *Gegenthese 4:* Ein laufender Gegner, dessen Ankunft nur dem
  Hauptgegner nützt, löst beim ersten Mal Zähnefletschen aus – die Käfer des Administrators auf Brett 4 stärken und
  heilen ihn (BossModReborn 2087153c2, `AdministratorPiece`: Aktion 48734 auf den Administrator, Damage Up und
  Rehabilitation). Preis: einmal je Art und Besuch hält der Begleiter ein Fenster lang statt dir; danach nicht mehr.
  Ob die Käfer ein Ziel der Feindseligkeit tragen, ist nicht gemessen (kein Mitschnitt von Brett 4); ohne eines
  erkennt RSR sie nicht als laufend, und Zähnefletschen bleibt aus, was dort stimmt. *Ausgeliefert, und nichts
  ändert sich?* Wenn du selbst woanders stehst (RSR bewegt dich nicht) oder kein Begleiter draußen ist. *Warum das
  Blickziel nötig ist (11.11.2026):* In Kampf 4 (Schattenmilben), Kampf 8 (Ahrimane) und Kampf 10 (Zombie) kamen
  gewöhnliche Angreifer dem Hauptgegner näher, weil du an ihm standst; ohne das Blickziel sähen sie aus wie
  Irrlichter, und Zähnefletschen käme ohne Anlass (viermal an dem Tag). Ein Verschwinden ohne Tod ist keine Ankunft:
  Am Kampfende verschwinden alle Gerufenen mit vollem Leben. In den drei Oger-Kämpfen des Tages stand die Deckung bei
  allen vier Explosionen schon durch K9 (A724; `approach_model.py` spielt Irrlichter, Milben, Ahrimane und Zombie
  gegen die Erkennung).
  *Falsifikation:* Kein Unterschied zu „wenigste LP"? Im Atomos-, Medusa- und Oger-Kampf doch. Option falsch? Eine
  verrauschte Messung ließe das Ziel springen – der Wechsel verlangt einen Unterschied über ihrer Unsicherheit. Nichts
  ändert sich? Im Crucible überschreibt heute A525 die Zielart; die neue Regel tritt dort an ihre Stelle, die Spur nennt
  jeden Wechsel mit Grund.
  *Verhängnis des Taurus (A683; sein Lauf vom 08.11.2026):* Tödlicher Strahl (Mortal Ray, 48143) trifft alle und legt
  Verhängnis (5421) für 25 s; das Blatt führt es (`XBMBattleDetailAction` Zeile 38 am Taurus-Ebenbild, Kampfzeile 8).
  Geheilt wird es nur durch Stehen auf einer der vier Kacheln um die Mitte, die in vier Schritten zu je 2 s auflädt
  (BMR 27be422 `TaurusPiece.Doom`: „Cleanse Doom!", Zielzonen für die KI; Werkzeugquelle). Am 08.11.2026 kam es dreimal:
  zweimal geheilt oder mit dem Kampf beendet, einmal abgelaufen – um 19:01:12 standest du 0,5 s vor Ablauf auf der
  Kachel, um 19:01:14 der Tod. RSR bewegt nicht. Seine Deckung ist Soulreaper's Armor („Doom Resistance +100%", XBMItem
  29) aus Laden oder Schatz (K10, Regel 2); der Laden davor empfahl keine. Ob er sie anbot, zeigte die Spur nicht; seit
  A683 nennt die Ladenzeile jedes Angebot. Ob Beast Remedy Kit („Automatically cures the next status ailment") Verhängnis
  heilt, ist unbelegt; es zählt dafür nicht.
  *Gegner, die erst später schaden (A674; Quelle A667):* Auf Brett 2 setzt der Taurus mit Ray of Ignorance eine Aetheric
  Charge auf dich, die in Ringen von 6, 12 und 18 Yalm platzt, hohen Magieschaden macht und Damage Down legt (BMR
  27be422 `TaurusPiece.AethericCharge`, CrucibleSux-Recherche 12; Tekz' Modul reiht sie hinter die Ahrimane, deren Blick
  Verhängnis legt und die sich selbst sprengen). Die Charge wirkt ihre Platzer selbst (48146–48148, je 1,5 s Wirkzeit;
  BMR 27be422), zwölf im Abstand von 3,07 s, je vier auf 6, 12 und 18 Yalm (Tekz `AethericChargeBursts`); sie ist
  angreifbar (Tekz-Vorrang 3). Ihre Treffer zählen damit bei ihr selbst. Bis zum ersten Platzen hat sie nicht getroffen
  und zählt in K3 wie jeder Ungemessene das Mittel der übrigen. *Was gilt:* Ein ungemessener Gegner zählt zuerst mit der gemessenen Gefahr seiner Art
  in diesem Gebiet (Konzept 03, „Ungemessene Gegner", A674) – die zweite Charge eines Kampfs, und jede in einem späteren
  Kampf desselben Bretts, steht damit von ihrem Erscheinen an mit der Gefahr der ersten. Die erste zählt bis dahin mit dem
  Mittel des Kampfs, nach dem Blatt gewichtet (A582), und mit ihrem Wirken, sobald sie es ankündigt. *Die Sanduhr
  des Todes beim Königs-Ahriman-Ebenbild* (Zweites Meisterbrett, Elite 4, Zug 13 rechts) schadet erst mit ihrem Fall;
  Regel und Quellen: K12, „Königs-Ahriman-Ebenbild". *Gegenthesen A674:* (1) Eine Art schadet je Kampf anders. Widerlegt: Gezählt
  wird ihre Gefahr je Sekunde bis zum Fall, über ihre ganze Lebenszeit – genau die Größe, nach der K3 reiht –, und die
  eigene Messung ersetzt sie mit dem ersten Treffer; ein anderes Brett ist ein anderes Gebiet. (2) Ein gefallener Gegner
  verdünnte seine Art über die Zeit nach seinem Tod. Beseitigt: Seine Zeit endet, wo er zuletzt in der Gegnerliste stand
  oder traf. (3) Unsichtbare Helfer mit dem Namen des Bosses machten die Art des Bosses zur Summe von Einzeltreffern über
  je einen GCD. Beseitigt: Es zählen nur Gegner, die in der Gegnerliste standen.
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
    sobald eine Regel sie liest. Am 07.11.2026 (18:08) 17 Abbrüche: zehn mit viertem Wert 0, nach denen die Wirkung kam
    (Tödliche Durchbohrung traf 0,3 s danach; ACT „Cancelled"), und sieben mit 1 (ACT „Interrupted"), jeder mit dem
    Tod des Wirkenden – sein letzter Treffer überstieg seine LP, die Todeszeile folgte 2 s später (A665). Der vierte Wert
    1 ist der Tod, keine Unterbrechung; eine Unterbrechung durch einen Begleiter ist bis heute nicht beobachtet.
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
  - *Teamgröße nach Punkten (A698):* Ein kleineres Team bringt In Unterzahl I bis III (`XBMScoreBonus` 4 bis 6: 6 oder
    7, 4 oder 5, 1 bis 3 Begleiter; 1.250, 2.500, 4.250 auf jedem Brett). Die Größen liest RSR aus den Bedingungen des
    Blatts („a team of 6 or 7 familiars"), die Punkte aus `XBMContent` (K15); `TeamSize` (10 bis 15) ist die Obergrenze,
    keine Pflicht. *Im Kampf:* Weniger Begleiter heißt mehr Einsätze je Begleiter, und LP kehren ohne Rast nicht zurück
    (K11). Fehlt am Ende ein Begleiter mit LP, trägt der Spieler die Feindseligkeit (K9), und seine Last steigt über die
    gelernte; fehlt eine Schwäche, dauert ein Kampf länger. *Regel:* Die gierige Wahl oben ordnet die Begleiter nach
    ihrem Beitrag; ihre ersten n sind das beste Team der Größe n. Der *Kern* sind die Picks bis zum letzten, der eine
    Schwäche neu abdeckt oder als Heiler oder Unterbrecher gebraucht wird. RSR empfiehlt die kleinste Größe einer Stufe
    (ihre Obergrenze: 3, 5, 7), die (1) den Kern fasst und (2) deren LP-Vorrat ohne einen Begleiter – Größe weniger
    eins, mal mittlere LP der Begleiter im Teamfenster (Schluss: die LP der Begleiter außerhalb des Teams zeigt kein Blatt,
    `XBMPetParamGrow` führt nur Konstitution) – mindestens die LP fasst, die die Begleiter auf diesem Brett in einem Lauf je verloren haben (gelernt
    je Brett, der schwerste Lauf, Datei `CrucibleFamiliarLoss.json`). Ohne gemessenen Lauf auf dem Brett bleibt die volle
    Größe (sichere Seite). Das Fenster nennt Größe, Bonus und den Vergleich; übernehmen bleibst du (C).
    *Gegenthese 1 – Der Verlust wächst mit weniger Begleitern:* Die Gegner teilen denselben Schaden aus, gleich wie viele
    Begleiter ihn tragen; mehr Einsätze je Begleiter verteilen ihn nur anders. Ein Begleiter, der fällt, fehlt danach;
    deshalb zählt der Vorrat einen weniger: Fällt einer, fassen die übrigen den schwersten gemessenen Verlust noch.
    *Preis:* Fällt einer, verliert der Lauf Keine Verluste (1.000) und erhält Minimale Verluste (250 bis 750); gegen
    1.250 bis 4.250 für die Größe. *Gegenthese 2 – Die Summe fasst nicht, was ein Einsatz kostet (hält; Prüfung vom 11.11.2026, A702):* Seine Läufe
    vom 11.11.2026 – Brett 1 dreimal: Begleiterverlust 1.375 bis 1.836 LP je Lauf bei rund 600 LP je Begleiter; Brett 2
    zweimal: 3.046 bis 3.145 LP bei rund 920 – zeigen Einsätze bis 500 LP (Fliegenfalle 667 auf 167, Gigantoad 438,
    Morbol 400 im Endkampf). Ein Begleiter trägt oft nur einen schweren Einsatz; was nach ihm bleibt, reicht für den
    nächsten nicht, und K11 hält ihn dann zurück. Die Summe der LP überschätzt das Team. *Verbesserte Regel (gebaut,
    A703):* RSR lernt je Brett die Verluste der Einsätze einzeln, absteigend geordnet, und hält je Rang den größten über
    alle Läufe (der größte Einsatz je gesehen, der zweitgrößte je gesehen …) – eine Hülle, die kein Lauf übersteigt.
    Für eine Größe n legt es diese Einsätze absteigend je auf den der n weniger einen Begleiter mit den meisten
    verbliebenen LP, der danach noch steht; die LP sind die der Begleiter, die die Wahl nimmt (im Teamfenster der
    Agent, sonst das Mittel des Teams). Passen alle, trägt n jeden gemessenen Lauf mit einem in Reserve; die Summe
    bleibt als Anzeige. Der Preis „Mittel" bleibt nur für Begleiter außerhalb des Teamfensters. Die Rechnung kennt keine
    Heilung der Begleiter (Trick, Gegenstand, Rast) – sie liegt auf der sicheren Seite.
    *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Er übernimmt die volle Empfehlung, weil
    noch kein Lauf gemessen ist. Der erste Lauf mit voller Größe misst den Verlust; ab dem zweiten empfiehlt RSR kleiner.
  - *Team aus einer Gattung (A699):* „Freund der Biestlinge" bis „Freund der …" (`XBMScoreBonus`, je Gattung eine Zeile,
    Bedingung „a team of only beastkin"; 750 auf jedem Brett). RSR läuft dieselbe gierige Wahl je Gattung, nur über deren
    Begleiter, mit derselben Größenregel. Ein solches Team gilt, wenn es jede Schwäche des Bretts trifft, die das gemischte
    trifft, mindestens so viele Heiler und Schützer hält, wie das gemischte für den Boss braucht, und den Unterbrecher,
    wo das gemischte einen hält (nur Seelen-Begleiter unterbrechen, K4). Dann ist es so sicher wie das gemischte und
    bringt 750 Punkte mehr, dazu die Punkte seiner Größe; unter mehreren das mit den meisten Punkten. Levelt RSR
    nachhängende Begleiter (A579), tauscht es im Team einer Gattung nur gegen einen derselben Gattung. *Im Kampf:* Wo
    eine Gattung die Schwächen nicht trägt, ändert sich nichts. *Gegenthese:* Gleiche Abdeckung heißt nicht gleicher
    Schaden – ein Begleiter, der eine Schwäche nur schwach trifft, verlängert den Kampf. Die Hornwahl wägt im Kampf nach
    gemessenem Schaden (K16); die Kaderwahl hat diese Messung je Begleiter erst nach einem Lauf. *Preis:* Ein Kampf
    dauert länger, wenn der Begleiter der Gattung die Schwäche schwächer trifft als der gemischte. Ein längerer Kampf hebt
    die gelernte Last dieses Laufs, und der Lasttest (K15) wägt den nächsten Lauf danach. *Gegenthese (ausgeliefert, und
    nichts ändert sich):* Trägt keine Gattung alle Schwächen des Bretts, bleibt das gemischte Team; die Empfehlung nennt
    dann keine Gattung. *Ausgeschlossen:* ein Team einer Gattung auch ohne volle Abdeckung – eine fehlende Schwäche
    verlängert einen Kampf sicher, die 750 Punkte wiegen das nur, wo der Lasttest es trägt, und den rechnet die Kaderwahl
    vor dem Brett nicht.
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
    *Mechanik:* Die Rast teilt einen Gesamtanteil auf dich und die mitruhenden Begleiter, je Teilnehmer ein Anteil der
    eigenen maximalen LP, höchstens 100 %. Der Gesamtanteil hängt am Brett: auf Brett 1 90 % (allein 90 %, mit einem
    Begleiter je 45 %, mit zweien je 30 %; Wiki; „Rest and recover 45%…" in Daedalus' Feldnotiz; 30 % mit zweien in
    seinen drei Läufen vom 07.11.2026 aus der Bestätigung gelesen), auf dem Ersten Meisterbrett an der Campsite von Zug 6
    150 % (allein 100 %, gekappt, mit einem je 75 %, zwei 50 %, drei 37 %, vier 30 %; consolegameswiki „First Master's
    Board" und die Recherche von CrucibleSux, 07.11.2026). Für die übrigen Bretter und Lager nennt keine Quelle Zahlen,
    und kein Blatt führt den Gesamtanteil (`XBMContentCamp` hat nur `FamiliarRecoverCount`, EXDSchema latest
    a1e24a0c). Wie viele mitruhen dürfen, legt das Lager fest (`XBMContentCamp`, Zeile als
    Brett gelesen, Unterzeile je Lager, 1 bis 4: erstes Brett 2, 2; zweites 2, 3; drittes 3, 3; Erstes Meisterbrett 2, 4;
    Zweites 1, 2, 3). Ein ruhender Begleiter verliert sein Futter („Familiars that rest will lose current effects of
    feed", Addon 17657; „G2 Primafodder" gibt +30 % maximale LP, „Honey Simular" −25 % physische Verwundbarkeit,
    `XBMItem` Typ 3), ein gefallener wird nicht geheilt.
    *Was gilt:* RSR wählt vor, ruhen (`[3]`) oder gehen drückt er. Begleiter ruhen mit, solange deine LP nach der Rast
    die schwerste Strecke bis zur nächsten Rast tragen (A661; seine Meldung vom 07.11.2026, unten): Auf jedem Weg von
    hier zählt RSR die Kämpfe bis zur nächsten Campsite – auf einem Weg ohne sie bis einschließlich des Endkampfs –,
    jeden mit dem schwersten Schaden, den er dir je gemacht hat (*Last je Kampf*, unten), und nimmt den schwersten Weg.
    Liegen deine LP nach der Rast darüber, darf deine Rast kürzer ausfallen als allein; kennt RSR einen Kampf auf diesen
    Wegen noch nicht oder kennt es die Wege nicht, musst du voll herauskommen. Unter dieser Bedingung die Zahl, die den
    Begleitern die meisten LP bringt (je Begleiter höchstens, was ihm fehlt), und dafür die stehenden, nicht vollen,
    deren Futter die Rast nicht kostet (unten), mit dem kleinsten LP-Anteil; bei Gleichstand weniger. Sonst ruhst du
    allein. Das Fenster nennt die Strecke und ihre Last.
    *Futter an der Rast (A669):* Ruht ein Gefütterter, endet sein Futter – außer er aß ein Lilien-Simulakrum („Preserves
    the effects of feed when resting at a campsite"): Er ruht wie ein Ungefütterter und behält es. Aß er ein
    Lassi-Simulakrum („Completely restores HP at campsites"; consolegameswiki „Beast Feed", 07.11.2026: „Satiety is reset
    and feed bonuses are lost if the familiar is restored at a campsite space unless if they have been fed a Lily
    Simular"), füllt ihn die Rast ganz statt des Anteils. Ohne Lilie ruht er nur, wenn er danach mehr wirksame LP hat als
    jetzt – volle LP ohne sein Futter gegen seine LP jetzt durch den Schadensfaktor seines Futters (erlittener Schaden) –,
    denn sein Futter endet mit der Rast. Jeder andere Gefütterte ohne Lilie ruht nicht. Ob ein Begleiter, den die Rast
    ganz füllt, den Anteil der übrigen mitteilt, ist unbelegt: RSR rechnet deinen Anteil, als teile er mit (er zählt in
    n), und liest das Angebot des Spiels, als teile er nicht (der kleinere Gesamtanteil) – beides gibt dir eher zu wenig
    Rast als zu viel. Grund: Bis A669 ruhte kein Gefütterter, auch keiner mit Lilie, deren einziger Zweck diese Rast ist
    (K8 nannte das als gebaut; C178), und ein Lassi-Begleiter bekam an der Rast nur den Anteil. *Gegenthese A669-1 (Lassi
    ohne Lilie):* Ruht er, verliert er für den Endkampf auch +30 % maximale LP und anderes Futter. Widerlegt in seinem
    Maß: Er ruht nur, wenn seine wirksamen LP danach höher sind; was das Futter an Schaden gibt, wiegt die Regel nicht –
    Preis: weniger Schaden, Sicherheit vor Schaden. *Gegenthese A669-2 (Teilen unbelegt):* oben, beide Annahmen zur
    sicheren Seite. *Ausgeliefert, und nichts ändert sich?* Wenn vor einer Rast keiner gefüttert ist – K14 empfiehlt
    Futter für den Endkampf, nach der letzten Rast –; dann trifft die Regel, was du selbst vorher fütterst. Ein Zufallsfeld zählt mit seinem schwersten möglichen Kampf und nie als Campsite.
    *Last je Kampf (A661):* Am Ende jedes Kampfs merkt sich RSR je Kampf (`XBMBattleDetail`-Zeile) den Schaden, den du
    genommen hast, als Anteil deiner maximalen LP ohne Ausrüstung – zurückgerechnet mit den Stücken, die du da trugst
    (maximale LP, erlittener Schaden, physische und magische Verwundbarkeit, Ausweichen, wie K10 Laden) –, auf ganze
    Prozent aufgerundet, nur steigend, über alle Sitzungen (gelernte Datei `CrucibleFightLoads.json`, Reiter
    „Crucible fights"). Gelesen wird er mit der Ausrüstung von jetzt: mal (1 − erlittener Schaden), geteilt durch
    (1 + maximale LP). Die Verwundbarkeiten nach Art lässt die Lesung weg, weil die Datei die Art nicht trägt; sie
    überschätzt dann, nie unter. Deine eigene Heilung zieht sie nicht ab. Zum Schaden zählen die Treffer und die Ticks
    von Schaden über Zeit auf dir (A722; das Spiel meldet die Ticks getrennt von den Treffern, bis dahin fehlten sie:
    im Elitekampf von Brett 1 am 11.11.2026, Lauf 1, 147 LP aus Treffern und 138 aus Gift, Lauf 3 57 und 145). Was ein
    Schutz abfängt (K13) und was zu Kampfbeginn fällt, rechnet weiter nur über die Treffer. Grund: Die Kämpfe liegen fest auf ihren
    Feldern (K14, „Der Weg aus den Blättern"), und der Endkampf ist schwerer als jeder davor – Brett 1 am 07.11.2026
    (ACT-Mitschnitt, Schaden auf dich je Kampf durch deine maximalen LP): 4, 18, 3 und 17 % (Elite), der Endkampf 35 %.
    Der schwerste bisherige Kampf hätte ihn halbiert, ein Mittel noch mehr.
    *Anteil der Rast (A667):* Den Anteil liest RSR aus der Bestätigung, sobald sie offen ist: Gesamtanteil = gelesener
    Anteil × Teilnehmer, solange der Anteil unter 100 % liegt – darüber ist er gekappt und sagt nur „mindestens". Er
    gilt für dieses Lager sofort und wird je Brett gelernt, über alle Sitzungen der kleinste gelesene (gelernte Datei
    `CrucibleCampPools.json`, Reiter „Crucible campsites"): Zwei Lager eines Bretts können sich unterscheiden, und der
    kleinere überschätzt nie. Ohne Lesung gilt 90 %, der kleinste bekannte. Jeder Teilnehmer bekommt den Gesamtanteil
    geteilt durch die Teilnehmer, höchstens 100 %. Bis A667 galt eine Lesung für jedes Brett und jedes Lager, ohne
    Kappung: Nach dem Meisterbrett hätte RSR auf Brett 1 mit einem Begleiter 75 % statt 45 % erwartet – ein Begleiter
    mehr ruht, und du kommst mit 30 % deiner LP weniger heraus, als die Rechnung trägt. Grund der Regel: Eine zu kleine
    Rast lässt einen Begleiter weniger heilen, eine zu große lässt dich zu schwach in den nächsten Kampf.
    Die Grenze des Lagers lernt RSR am Klick, den das Fenster nicht annimmt (oder mit dem es einen anderen abwählt). Futter liest es aus den Werten des
    Fensters (`ReaderXBMPetParty.FedCurrent`, ECommons, Werkzeugquelle; Daedalus liest dieselbe Stelle); stimmen deren
    LP nicht mit denen des Agenten überein, wählt RSR nichts. Beim Öffnen springt die Auswahl kurz auf den ganzen Kader
    (Daedalus, im Spielbetrieb); RSR handelt erst, wenn sie einen GCD lang steht. Ändert er sie danach von Hand, bleibt
    sie für dieses Lager. Einstellung „Pick who rests with you at a Crucible campsite (Crucible only)", ab Werk an.
    Das Fenster „RSR - Crucible campsite" zeigt diese Vorauswahl mit Grund; an der letzten Campsite vor dem Endkampf
    empfiehlt es dazu, wer für ihn mitruht (K14, A631, sein Vorschlag vom 06.11.2026). Die Grenze des Lagers steht im
    Blatt (`XBMContentCamp`, Unterzeile = Index des Lagers auf dem Brett, K14), gelernt wird nur noch eine kleinere.
    *Beispiel:* Du bei 75 %, Fledermaus 20 %, Fliegenfalle 40 %, Schleim 90 %, Lager für zwei: Mit zweien bekommst du
    30 % (voll) und die beiden ersten je 30 %, zusammen 60 % für die Begleiter; mit einer nur 45 %. RSR wählt Fledermaus
    und Fliegenfalle. Du bei 40 %, Strecke bis zur nächsten Rast 55 %: Mit einem kommst du auf 85 %, mit zweien auf 70 %
    – beides über 55 %; RSR nimmt zwei. Kennt RSR einen Kampf der Strecke noch nicht, ruhst du allein (85 % statt voll).
    *Im Kampf (seine Lage, 07.11.2026, Brett 1):* Lager für zwei, du bei 876 LP (rund 68 %), Goobbue 55 %, Spriggan
    116/491 (24 %). Mit einem kommst du voll heraus, mit zweien auf 98 %. Die Regel vor A661 nahm nur den Goobbue („you
    come out at full HP"); er setzte den Spriggan von Hand dazu. Mit den Lasten seines Laufs vom 07.11.: Auf Zug 3 liegen
    rechts bis zur Campsite auf Zug 7 die Kämpfe auf Zug 4 und 6 (3 + 17 = 20 %), links über den Schatz bis
    einschließlich des Endkampfs 55 % – 98 % liegen darüber, Goobbue und Spriggan ruhen, der Spriggan steht danach bei
    54 %. Auf Zug 7 (nur noch der Endkampf, 35 %) ebenso.
    *Verworfen:* (a) „Use HP potions below this HP" als Grenze (meine erste Fassung): Bei 13 % brächte dich eine Rast zu
    zweit auf 58 %, über jede übliche Linie – genau Daedalus' Feldfall vom 25.10.2026 (Rast mit dem Goobbue bei 13 %,
    Tod im nächsten Kampf). (b) Linie plus größter bisheriger Verlust je Kampf: ein Kampf statt der Strecke, und der
    größte bisherige unterschätzt den Endkampf (35 gegen 18 % auf Brett 1). (c) Daedalus' feste 50 %: eine Zahl ohne
    Herleitung, nur ein Begleiter, und zwischen 50 und 55 % kämst du nicht voll heraus. (d) Voll herauskommen, immer
    (A564 bis A661, meine Ableitung): Du kommst nie schlechter heraus als allein, aber ein Begleiter auf 24 % bleibt
    draußen, wo dir 2 % fehlen, die keine Strecke braucht – seine Meldung. (e) Jeder Kampf so schwer wie der schwerste
    gemessene (die Obergrenze des Ladens vor A661): unterschätzt den Endkampf wie (b). (f) Bis zum Endkampf statt bis zur
    nächsten Rast: Die nächste Rast heilt dich allein um 90 %, bis dorthin musst du tragen; der Weg über den Schatz
    zählt den Endkampf ohnehin mit.
    *Gegenthese 1 (erledigt mit A661):* Bei 54 % und einem Begleiter auf 5 % ruhtest du allein; du gewönnest 45 % für ihn
    gegen 1 % für dich. Jetzt ruht er mit, sobald die Strecke weniger als 99 % kostet; nur auf einem Brett, dessen
    Kämpfe RSR noch nicht kennt, bleibt es beim alten Preis (er geht mit 5 % in den nächsten Kampf; K8 und K9 bleiben,
    und mit einem Klick nimmt er ihn dazu). *Gegenthese A661-1:* Ein Kampf trifft dich schwerer als je zuvor. Dann
    kommst du tiefer an als in jedem früheren Lauf; die Heilgegenstände (K8) bleiben, und RSR merkt sich den neuen
    Höchstwert. Die Strecke zählt jeden Kampf mit seinem schwersten Lauf, zusammen also schwerer als jeder einzelne Lauf.
    *Gegenthese A661-4:* Ein Kampf, in dem du fielst, steht mit dem Schaden bis zu deinem Fall in der Datei, also mit
    rund einer vollen Leiste; jede Strecke über ihn verlangt danach volle LP wie vor A661. Nicht widerlegt, Preis
    benannt: Ein Kampf, der dich einmal ganz geleert hat, kann es wieder; die Begleiter ruhen dort seltener mit. Der Reiter
    „Crucible fights" zeigt den Wert; die Datei löschst du von Hand, wenn du anders spielst. *Gegenthese A661-2:* Du nimmst den leichteren Weg. Dann war die Bedingung strenger als nötig; ein Begleiter ruht
    vielleicht nicht mit, wo er dürfte – Preis klein, Richtung sicher. *Gegenthese A661-3:* Die Begleiter heilen dich im
    Kampf; der gelernte Schaden ist der ohne deren Heilung abgezogen, er überschätzt also. *Gegenthese 2:* Der am meisten verletzte ist nicht unbedingt der,
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
    *Noch leeres Fenster:* Öffnet sich der Laden, bevor das Spiel seine Werte gefüllt hat, wirft ECommons' Leser
    `ArgumentOutOfRangeException`. Bis A764 brach das die Dienste des Beastmasters für den Frame ab, Schatz und Route
    eingeschlossen (am 08.11.2026 fünfmal). Jetzt wartet RSR einen Frame auf die gefüllten Werte, wie beim Futter.
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
    geht ins `DefenseTrace.log`. Die Liste, Sicherheit zuerst, solange Marken und Plätze reichen – 1, 2 und 4 zuerst,
    dann 3, 5 und 6 auf einer Skala (A659, unten):
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
    *Eine Skala, sobald ein Kampf gemessen ist (A659; TODO aus A656):* Nach 1, 2 und 4 wählt RSR Ausrüstung,
    Heilgegenstände und Schutzgegenstände zusammen auf der Skala des Schatzes (K13, „Eine Skala für Ausrüstung und
    Gegenstände"): der Anteil deiner LP, den jedes Angebot über die Kämpfe bis zum Endgegner bringt. Gewählt wird Stück
    für Stück das mit dem größten Wert je Marke, neu gerechnet nach jedem Stück (Ausrüstung auf die gewählte, Tränke auf
    die gewählten), über jede Startwahl wie in 3; von allen Auswahlen gilt die mit dem größten Wert, bei Gleichstand die
    billigere. Ein Heilgegenstand zählt höchstens, was du noch heilen kannst: die Kämpfe auf dem schwersten Weg bis
    einschließlich des Endkampfs, jeder mit seiner Last je Kampf (Campsite oben, A661), ein noch ungelernter so schwer wie
    dein schwerster gemessener, plus was dir jetzt fehlt, abzüglich der Heilung, die du schon hältst und die die Auswahl
    schon nahm. Grund: Vorher stand jede Ausrüstung mit messbarem Gewinn vor jedem Trank. Im Laden auf Zug 8 von Brett 1
    (nur noch der Endgegner, 07.11.2026) empfahl RSR so die Mithril-Rüstung für 858 Marken mit „+4 %"; der
    Bestien-Heiltrank I für 35 Marken bringt 10 %, sofern du sie noch heilen kannst. Die Obergrenze verhindert das
    Gegenteil, zehn Tränke für drei Kämpfe zu 18 %. *Berichtigt (A661):* Bis A661 rechnete sie jeden Kampf, den Endkampf
    eingeschlossen, so schwer wie deinen schwersten gemessenen, mit der Begründung, im Endkampf von Brett 1 hätten dich
    seine Zauber mit 1 bis 9 % getroffen wie davor. Das stimmte je Treffer, nicht je Kampf: Der Endkampf dauerte doppelt so
    lange und kostete dich 35 %, der schwerste davor 18 % – die Obergrenze lag dort bei der Hälfte dessen, was du heilen
    musstest. Vor der ersten Messung gelten 3, 5 und 6 in dieser Reihenfolge.
    *Warum die Zustände vor der Skala (A659):* 4 deckt, was LP und Minderung nicht abfangen – Kontrolle, verhinderte
    Aktionen, Schaden über Zeit, der dich auch bei vollen LP weiter trifft; die Skala wiegt nur LP. Vorher stand 3 vor
    4: Eine Rüstung mit wenigen Prozent ging einem Paralyse-Serum vor, wenn die Marken nicht für beides reichten.
    *Gegenthese (Vorrat für spätere Läden):* Marken, die jetzt in Tränke gehen, fehlen einem besseren Angebot später.
    Nicht widerlegt, Preis benannt: Was ein späterer Laden anbietet, steht nirgends; ein Trank behält seinen Wert, und
    die Obergrenze kauft keinen, den du nicht brauchst.
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
    *Wo du stehst (A632, A715):* Am Hornfenster das Feld des Kampfs (K14) – ein Feld mit diesem Kampf oder ein
    Zufallsfeld, das er werden kann: Die Kämpfe der Zufallsfelder stehen in den Blättern nur als deren Möglichkeit
    (Bretter 2 bis 5, neun Felder; ffxiv-datamining 52a72b3). Öffnet danach das Fenster einer Campsite, eines Ladens
    oder eines Schatzes, ist es auf den Wegen voraus das erste Feld, das diese Art ist oder als Zufallsfeld werden
    kann – über Felder hinweg, deren Fenster RSR nicht sah, aber nie über einen festen Kampf: den hätte das Hornfenster
    genannt. Steht der Lauf schon auf einem Feld dieser Art, bewegt ihn nur ein Fenster, das neu aufgeht, und nur auf
    das unmittelbar nächste Feld (Brett 3: Schatz, dann Zufallsfeld mit Schatz). Passen mehrere Felder – eine
    Gabelung, deren Abzweige beide mit dieser Art beginnen (Brett 4 nach dem ersten Elitegegner: Schatz oder
    Zufallsfeld mit Schatz; Brett 5 nach dem ersten und dem letzten Laden) –, sagt das Fenster nicht, welches: Der
    Lauf bleibt auf der Gabelung stehen, bis der nächste Kampf ihn setzt; gerechnet wird solange über beide Abzweige,
    die schwerere Seite zählt. Gibt es kein solches Feld voraus, bleibt das letzte bekannte. So folgt RSR dem Lauf
    Feld für Feld, ohne Position.
    Bis A715 suchte ein Kampf nur unter den festen Kampffeldern und ein Fenster nur unter den unmittelbar nächsten
    Feldern, das erste passende. Ein Zufallskampf ließ den Lauf auf dem Feld davor stehen – er zählte weiter als
    bevorstehend, eine Rast als noch möglich –, und an den drei Gabelungen mit Schatz auf beiden Seiten stand der
    Lauf je nach Abzweig auf dem falschen. Das Modell `crucible_route_model.py` geht jeden Weg jedes Bretts mit jedem
    Ausgang jedes Zufallsfelds: Die alte Regel stand auf den Brettern 2 bis 5 in 271 Fällen hinter einem schon
    geschlagenen Kampf und in 162 auf einem Feld, auf das der Lauf nie kam; die neue in keinem – sie steht auf den
    Brettern 4 und 5 an den mehrdeutigen Gabelungen zurück (207 Fälle), nie hinter einem Kampf (gerechnet 12.11.2026).
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
    - *Vorausrechnung (seine Präzisierung: wie beim Schach; seine Vorgabe vom 07.11.2026: „sich immer auf die summe
      der anstehenden kämpfe ausrichten"; A663):* je Abzweig alle Felder bis einschließlich des Endkampfs. An einer
      Gabelung dazwischen die bessere Fortsetzung (die wählst du dann selbst), an einem Zufallsfeld die schlechtere
      seiner Möglichkeiten (Sicherheit vor Glück); ein Zufallsfeld zählt nicht als Rast. Danach rechnet RSR am nächsten
      Feld neu. Bis A663 endete die Rechnung vor der nächsten Campsite nach dem Zusammenlauf; was danach kam – auf Brett 1
      der Endkampf mit 35 % –, sah sie nicht.
    - *Zustand:* deine LP-Anteil jetzt; dein Vorrat an Heilgegenständen als Anteil deiner LP (K8, nur die für dich); der
      niedrigste LP-Anteil der Heiler und Schützer aus dem Endkampfplan (K11, K14) und deren größter Verlust je Einsatz
      (`FinalLossPerUse`). Die LP der Heiler sind die gemessenen (K11, „Die LP des Kaders hängen einen Kampf nach"):
      Die Empfehlung fällt gleich nach einem Kampf, und das Teamfenster des Spiels zeigt dann noch die LP von davor;
      bis A716 las sie diese (am 11.11.2026 zeigte das Teamfenster um 15:11:03, gleich nach Kampf 1, Morbol mit 667 und
      Raptor mit 589 LP, mit der Meldung des nächsten Kampfs um 15:11:34 dann mit 480 und 555). Dasselbe gilt für die
      Futterempfehlung im Laden und „keine Rast nötig" (K15).
    - *Felder:* Campsite wie K10 – Begleiter ruhen mit, solange deine LP danach die Strecke bis zur nächsten Rast tragen
      (A661), so viele, wie das Lager nimmt (`XBMContentCamp`); einen gefallenen Begleiter heilt sie nicht (Wiki, oben),
      ein toter Begleiter spricht also nicht für die Campsite. Laden, Schatz: keine LP, aber Gegenstände (vermerkt, Marken
      genannt). Ein Kampf mit dem, was er dich kostet (A721): sein gelernter Nettoverlust und seine tiefste Senke
      ohne deine eigene Heilung, mit der Heilung der Begleiter (K15, „Woran ein leichtes Brett feststeht") – die Senke
      für den tiefsten Stand im Kampf, der Nettoverlust für den Stand danach; wo beides noch nicht gemessen ist, seine
      rohe Last je Kampf (K10 Campsite, A661); ein noch ungelernter Gegner mit der mittleren gemessenen Last eines Kampfs
      abzüglich deiner Heilung (`CrucibleBoard.Loads`, wie K10 Laden), Elite und Endgegner mit der schwersten
      gemessenen. Bis A721 zählte jeder Kampf mit der rohen Last, ohne die Heilung der Begleiter: im Endkampf von
      Brett 1 gelernte 60 % der LP ohne Ausrüstung gegen einen gemessenen Verlust von höchstens 12 % (11.11.2026, drei
      Läufe). Die Vorhersage stand entsprechend tief (Lauf 1, 14:58:07: „after the final fight: you ~-5 %"; ohne
      seinen Heiltrank stand er nach dem Endkampf bei 43 %), und ein Abzweig mit mehr Kämpfen galt eher als unsicher: am 11.11.2026 um 15:56:00 der Elitekampf (800
      Punkte) mit „~-40 %" gegen den Weg mit der Rast. Der Endkampf-Heiler verliert in einem Kampf vor dem Endkampf seinen größten Verlust
      je Einsatz nur, solange er danach noch einen übrig hätte – sonst hält K11 ihn zurück, und er verliert nichts (A663).
    - *Flucht (A671):* Ein Gegnerfeld lässt die Flucht zu, Elite und Endgegner nicht (CrucibleSux-Recherche 01, be66eda);
      die Bestien-Rauchbombe ermöglicht sie („Use by selecting 'Flee' upon an enemy space"), das Feiglingsmesser bietet sie
      mit 35 % an und gibt danach Guerrilla Tactics. Fliehen kostet die Beute des Felds, keine LP. Hältst du Rauchbomben,
      rechnet die Vorausrechnung an jedem Gegnerfeld beide Fortsetzungen – kämpfen oder mit einer Bombe fliehen – und
      nimmt die bessere; das Fenster nennt das Feld, vor dem du fliehst („flee from the fight at move 5 with a smoke
      bomb"), denn die Flucht wählst du im Spiel selbst. Das Feiglingsmesser zählt in der Vorausrechnung nicht: Ob es die
      Flucht anbietet, ist Zufall, und *sicher* rechnet ohne Glück; bietet es sie an, nimm sie (das Fenster sagt es, solange
      du es trägst) – sie spart den Schaden des Kampfs. Mit dem Diebesmesser bringt ein Schatzfeld zwei Gegenstände und
      zählt doppelt. *Gegenthese A671-1 (Beute):* Fliehen kostet Marken, die ein späterer Laden in Heilung tauschen
      könnte. Nicht widerlegt; was ein Laden anbietet, steht nirgends, der gesparte Schaden ist sicher – Sicherheit vor
      Beute. *Gegenthese A671-2 (Flucht nicht genommen):* Fliehst du nicht, stimmt die Rechnung nicht mehr. Widerlegt: Nach
      dem Feld rechnet RSR mit deinen LP neu (Vorausrechnung).
    - *Sicher* ist ein Abzweig, wenn du in jedem Kampf bis einschließlich des Endkampfs mit Vorrat lebst – dein
      tiefster Stand auf dem Weg, jeder Kampf mit seiner Senke auf dem Stand davor, die Rasten dazwischen (A721; bis
      dahin zählte nur der Stand nach dem Endkampf, und eine Rast spät am Weg glich einen Stand unter null davor
      rechnerisch aus) –, und wenn der schwächste Endkampf-Heiler vor dem Endkampf einen Einsatz
      übersteht (seine LP über seinem größten Verlust je Einsatz, wie „übersteht den Einsatz" am Hornfenster, K6) oder
      voll ist: Mehr als volle LP bringt ihm kein Weg, an ihm liegt es dann nicht (A716). Kostet ein Einsatz mehr, als
      der kleinste Heiler LP hat, war sonst jeder Weg unsicher – am 11.11.2026 in Lauf 3: ein Einsatz bis 522 LP, Puk hat
      521, „no way is safe" bei 26 % eigenen LP nach dem Endkampf, empfohlen die Rast statt des Schatzes; derselbe
      Ausfall wie vor A663 mit zwei Verlusten.
      Empfohlen wird unter den sicheren der mit den meisten Feldern, die Gegenstände bringen (Schatz, Laden; ein
      Zufallsfeld nur, wenn schon seine schlechteste Möglichkeit einer ist), sonst der mit den meisten LP danach; ist
      keiner sicher, der, nach dem du mit Vorrat am höchsten stehst, bei Gleichstand der mit den gesünderen
      Heilern. *Warum nicht mehr zwei Verluste (A663):* Bis A663 verlangte „sicher" am Ende zwei größte Verluste für den
      Heiler und zog ihm in jedem Kampf einen ab. Auf Brett 1 kostete ein Einsatz bis zu 343 von 550 LP (62 %, A637):
      Zwei sind mehr als ein voller Begleiter. Jeder Weg war unsicher, und unter unsicheren gewann der mit der Rast, weil
      sie deine LP hebt – seine Beobachtung „es wird immer nur camp vorgeschlagen". Zwei Verluste sind das Maß der Reserve
      vor einem *anderen* Kampf (K11: dann bleibt einer für den Endkampf); im Endkampf braucht er einen.
    - *Grund im Fenster:* Die Empfehlung nennt, was der andere Abzweig gebracht hätte – bei Campsite gegen Gegenstände
      die LP, die die Rast dir und den Endkampf-Heilern bis nach dem Endkampf mehr ließe, und ob beide Wege sicher sind.
    - *Seine Frage (07.11.2026):* „es wird immer nur camp vorgeschlagen. ist die auswahl so korrekt?" Nein (A663), aus
      drei Gründen, je an ihrer Stelle: zwei Verluste je Einsatz für den Heiler (*Warum nicht mehr zwei Verluste*), ein
      Laden auf jedem Weg (*Gegenstände je Feld*, unten) und die Rechnung, die vor der nächsten Rast endete
      (*Vorausrechnung*). Sein Log prüft das nach (Skill „upload-auswertung").
    - *Vor der ersten Last:* Kennt RSR einen Kampf auf einem Weg weder aus der Last je Kampf noch aus einem gemessenen
      Kampf dieser Sitzung, keine Rechnung: die Campsite, solange du oder ein Endkampf-Heiler nicht voll steht, sonst der
      Abzweig mit Gegenständen. Das Fenster sagt es.
    - *Im Kampf (seine Lage, 07.11.2026, gerechnet mit den Lasten seines Laufs):* Brett 1 nach Zug 1, du bei rund 93 %,
      Vorrat 36 %, Endkampf-Heiler voll. Links Gegner und Campsite, rechts Gegner und Schatz; zusammen laufen die Wege
      in Zug 4; danach Zug 4 (3 %), Schatz auf Zug 5, Elite (17 %), auf Zug 7 Schatz oder Campsite, Laden, Endkampf
      (35 %). Den Gegner auf Zug 2 rechnet RSR auf beiden Seiten mit 18 %. Links: 75 %, Rast auf 100 %, nach Zug 4 und 6
      80 %; auf Zug 7 ist der Schatz die bessere Fortsetzung (beide sicher, ein Gegenstand mehr), nach dem Endkampf 45 %,
      drei Felder mit Gegenständen. Rechts: 75 %, Schatz, nach Zug 4 und 6 55 %, Schatz auf Zug 7, nach dem Endkampf 20 %
      und 36 % Vorrat, vier Felder. Beide sicher; empfohlen: rechts, ein Gegenstand mehr – wie seine Einschätzung.
      *Gegenstände je Feld (A663):* Bis A663 zählte nur, ob ein Weg überhaupt Gegenstände bringt. Der Laden auf Zug 8
      liegt auf jedem Weg; mit ihm brachte jeder Weg Gegenstände, es entschieden deine LP danach, und die Rast gewann –
      die zweite Hälfte von „es wird immer nur camp vorgeschlagen". Jetzt zählen die Felder mit Gegenständen; was jeder Weg
      hat, hebt sich auf.
    - *Im Kampf (Gegenbeispiel):* Nach Kampf 3 bei 55 %, Vorrat 30 %, noch ein Kampf zu 35 %, dann der Endkampf zu 50 %;
      die Fledermaus als Endkampf-Heilerin bei 40 %, ihr größter Verlust je Einsatz 45 %. Rechts über den Schatz:
      55 − 35 − 50 = −30, mit Vorrat 0 – du überlebst nicht sicher, und die Fledermaus übersteht keinen Einsatz (40 unter
      45). Links ruht sie nach dem Kampf mit dir (20 % plus 45 % sind 65 %, über den 50 % des Endkampfs), sie steht danach
      bei 85 %, du gehst mit 15 % aus dem Endkampf. Empfohlen: links, die Campsite.
    - *Verworfen:* (a) ein Plan fürs ganze Brett beim Betreten – seine Präzisierung; (b) Punkte und Marken als Ziel – er
      nannte Gesundheit, Ausrüstung, Vorrat und Heilbedarf; (c) die Rechnung bis zum ersten Kampf des Abzweigs (A632) –
      sie sah nicht, was die Abzweige unterscheidet: Auf Brett 1 liegen Campsite und Schatz hinter je einem Gegner, das
      Fenster verglich zwei gleiche Gegner und nannte die Seite nicht.
    - *Gegenthese 1:* Die mittlere Last stammt aus anderen Kämpfen; der nächste ist schwerer. Seit A661 trägt jeder Kampf,
      den RSR einmal sah, seine eigene schwerste Last; die Mittelung bleibt nur für Kämpfe, die es auf keinem Lauf sah,
      und Elite und Endgegner rechnen dann mit der schwersten. *Gegenthese A663-1 (Heiler im Kampf):* Verlangt eine
      Gefahr im Gebiet einen Heiler mehr (B5), kommt ein knapper Endkampf-Heiler doch auf ein Horn und verliert mehr, als
      die Rechnung annimmt. Preis benannt: selten, und das Hornfenster zeigt es. *Gegenthese A663-2 (Summe statt
      Strecke):* Bis zum Endkampf zu rechnen lässt eine spätere Rast mitzählen, die du vielleicht nicht nimmst. Nicht so
      gebaut: An einer Gabelung zählt die bessere Fortsetzung, die du dort selbst wählst und die RSR dir dann wieder
      empfiehlt. *Gegenthese 2:* RSR verliert die Spur (ein Fenster, das es
      nicht liest). Dann rechnet es vom letzten bekannten Feld und zeigt Abzweige, die schon hinter dir liegen, bis das
      nächste Fenster oder Hornfenster es wieder auf das nächste passende Feld voraus setzt („Wo du stehst"); das Fenster
      nennt das Feld, von dem es rechnet. *Gegenthese 3:* Gegenstände
      aus Schatz und Laden sind nicht bezifferbar, bevor man sie sieht. Nicht widerlegt: Sie zählen nur als Vorzug unter
      sicheren Abzweigen. *Gegenthese 4 (Kamera):* Blickt er vom Endgegner zurück, sind links und rechts vertauscht.
      Teilweise widerlegt: Das Feld steht neben der Richtung. *Ausgeliefert, und nichts ändert sich?* Ohne Hornfenster im
      Lauf, ohne Brettplan im Blatt, auf Gabelungen ohne Unterschied bis zum Endkampf, oder wenn dir die Summe der Kämpfe
      auf dem Weg ohne Rast den Tod brächte – dann bleibt es bei der Campsite, und das ist richtig.
    Werkzeugquelle für die Vorlage: BeastMasterHelper (t3knical, Fork von anmili2022/Beastmaster) bevorzugt an einer
    Abzweigung die Campsite, wenn der Spieler verletzt ist, und prüft die LP des Kaders (Anleitung, 06.11.2026, A623).
- *K16 – Hornwahl lernt aus dem Kampf* (seine Meldung vom 11.11.2026: „beastmaster begleiter werden nicht optimal für die
  einzelnen kämpfe auf den brettern ausgewählt"; A692). *Im Kampf:* Derselbe Kampf auf Brett 1 lief in drei Läufen mit
  drei Hornsätzen 64 bis 81 s; gegen die schlaganfälligen Knochen (Kampf 1) setzte Lauf 1 Spriggan (Entfesseln: Schlag),
  die Läufe 2 und 3 keinen Begleiter mit Schlag, obwohl Spriggan und Karpfenkröte frei waren. Die Wertung je Begleiter
  (`Option`) ist eine Schätzung aus Blättern – Potenz, Reichweite, Schwäche, Natur des Horns (das Entfesseln zählt nur
  auf einem Horn mit „Tempered") –; ob sie den Kampf trifft, misst RSR an keinem Ausgang, und gleiche Werte entscheidet
  der Zufall der Reihenfolge.
  - *Messen (A711; die Fassung aus A692 ist widerrufen, C185):* Je Kampf (Zeile von `XBMBattleDetail`) und Begleiter der
    Schaden je Potenz seiner Treffer mit Dressur und Entfesseln: Summe des Schadens über Summe der Potenzen der Blätter,
    über alle Sitzungen aufaddiert (`CrucibleFamiliarHits.json`). Je Treffer gemessen, hängt der Wert nicht daran, wie
    lange der Begleiter draußen war und welche Rolle er im Kampf hatte. A692 teilte den Schaden durch die Zeit zwischen
    erstem und letztem Treffer und hielt den besten Wert: Ein Begleiter, der nur für Entfesseln und Scheidenden Schlag
    kam, lag weit über dem, der blieb, und zwei Treffer kurz nacheinander ergaben beliebig hohe Werte – das Maß
    belohnte die Rolle, die B3 dem Begleiter mit der schwächeren Dressur gibt.
  - *Nachsteuern:* Die Schätzung rechnet je Begleiter Potenz mal einem Zuschlag für die Schwäche. Für jeden im Kampf
    gemessenen Begleiter bildet RSR das Verhältnis gemessen zu geschätzt (Schaden je Potenz über dem Zuschlag der
    Schätzung) und multipliziert die Schadenswertung eines Begleiters mit seinem Verhältnis über dem Mittel der
    Verhältnisse aller dort gemessenen. Trifft die Schätzung, ist der Faktor 1; trifft ein Begleiter eine Schwäche, die
    sie ihm nicht anrechnet, oder steht er unter dem Rang des Bretts, weicht er ab. Je Begleiter zählen die Treffer
    seines jetzigen Rangs, sonst die des nächsten darunter (unten, „Gelernte Werte und der Rang der Begleiter"). Ungemessene behalten die Schätzung,
    ebenso ein Kampf mit nur einem gemessenen Begleiter. Sicherheit (übersteht einen Einsatz, heilt, schützt,
    unterbricht) bleibt unberührt und geht vor.
  - *Gegenthese 1 – Streuung:* Kritische Treffer streuen. Die Summe über alle Treffer und Läufe mittelt sie; nach wenigen
    Treffern schwankt der Faktor noch. Preis: In den ersten Läufen kann ein Begleiter mit Glück vorn liegen; jeder
    weitere Treffer zieht den Wert zum Mittel.
  - *Gegenthese 2 – Der Auto-Angriff fehlt:* Er hat keine Potenz im Blatt und zählt in der Schätzung der Hornwahl nicht;
    gemessen und geschätzt decken dieselben Quellen.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Ein Begleiter, den RSR nie setzt, wird nie gemessen. Er
    behält die Schätzung, und wo sie gleichauf liegt, entscheidet die Reihenfolge wie bisher.
  - *Anzeige und Spur:* Reiter „Crucible familiars" unter den gelernten Werten (Kampf, Begleiter, Schaden je Potenz,
    gemessene Potenz); je Kampfende eine Zeile „crucible fight …: familiar damage per potency".
  - *Teamgröße und Gattung:* im Kader (K7, „Teamgröße nach Punkten" A698, „Team aus einer Gattung" A699); die Größe
    misst den Verlust der Begleiter je Brett.
- *Gelernte Werte und der Rang der Begleiter* (A729; Befund der Quellcode-Prüfung, A728). *Was gilt:* Jeder im Crucible
  gelernte Wert – deine rohe Last je Kampf (`CrucibleFightLoads`), Nettoverlust und Senke (`CrucibleFightNet`), der
  Verlust der Begleiter je Lauf, je Einsatz und je Kampf (`CrucibleFamiliarLoss`), der Schaden je Potenz der Begleiter
  (`CrucibleFamiliarHits`) – wird zusätzlich je **Rangstufe** gelernt (Dateien `…ByRank.json`) und aus der Stufe
  gelesen, die zum Kader des Laufs passt.
  - *Rangstufe des Laufs:* das abgerundete Mittel der Ränge des Kaders, je Begleiter der synchronisierte Rang
    (`GetSyncedRank`), höchstens der Synchronrang des Bretts (`XBMContent.SyncRank`) und genau dieser, wenn der Begleiter
    über ihm steht (`IsPetOverSyncedRank`). Gelesen, wenn das Teamfenster das Brett zeigt (`AgentXBMPetParty.ContentId`):
    beim Betreten und mit jedem Hornfenster vor einem Kampf; die Spur nennt sie („crucible team rank …"). Vor dem Brett
    (Teamgröße, K7) gilt die Stufe der gewählten Begleiter. Für den Schaden je Potenz zählt der Rang des Begleiters
    selbst: Seine Werte stehen je Rang im Blatt.
  - *Lesen:* RSR nimmt die gemessenen Stufen auf oder unter der des Kaders, die nächste zuerst, und hält den größten Wert
    über sie, bis sie zusammen mindestens zwei Läufe zählen (die wenigsten, die eine Streuung zeigen; dieselbe Schwelle
    wie für das Senken einer Schicht, Konzept 13, Regel 5). Zählen sie weniger, gilt der Höchstwert über alle Läufe wie
    bisher. Der Schaden je Potenz liest die Summen der nächsten gemessenen Stufe auf oder unter dem Rang des Begleiters,
    sonst die über alle Ränge. Ist die Stufe nicht lesbar, gilt der Höchstwert über alle Läufe.
  - *Warum:* Stärke kommt im Crucible aus dem Rang. `XBMPetParamGrow` führt die Werte je Begleiter und Rang (Zeile 2:
    Stärke 50 auf Rang 1, 130 auf Rang 5; xivapi/ffxiv-datamining 52a72b3), und das Spiel gleicht nur nach unten an:
    Wer über dem Synchronrang steht, kämpft mit dessen Werten, wer darunter steht, mit seinen eigenen. Stärkere Begleiter
    beenden einen Kampf früher und verlieren weniger LP. Wie stark der Rang den Schaden hebt, rechnet kein Blatt vor
    (Schluss, unbelegt), aber die Richtung steht fest: Die Werte steigen mit jedem Rang. Ein Höchstwert über alle Läufe
    bleibt deshalb bei dem schwächsten Kader stehen, mit dem ein Brett je lief.
  - *Im Kampf:* Sobald zwei Läufe mit dem stärkeren Kader gemessen sind, rechnen Rast (K10), Laden, Weg, „leichtes Brett"
    (K15) und Teamgröße (K7) mit den Lasten dieses Kaders. Eine Rast, ein Kauf oder ein Begleiter, den nur die Läufe mit
    den schwachen Begleitern verlangten, entfällt. Damit bleiben Sparsamkeit (750) und In Unterzahl (1.250 bis 4.250)
    öfter erreichbar. Die Hornwahl (K16) misst jeden Begleiter an seinem jetzigen Rang statt an seinen schwächeren
    Treffern von früher.
  - *Gegenthese 1 – Kein Defekt, der Rang ändert die Last kaum:* widerlegt durch das Blatt. Die Werte steigen je Rang
    deutlich (oben). Ob ein Lauf leichter war, entscheidet ohnehin die Messung: Ist die stärkere Stufe nicht leichter,
    liest RSR dieselben Werte wie vorher.
  - *Gegenthese 2 – Option falsch, das Mittel des Kaders misst nicht, wer kämpft:* Auf die Hörner kommen drei aus dem
    Kader, und K6 wählt sie je Kampf. Zwei Kader mit gleichem Mittel können verschieden kämpfen, etwa einer mit zwei
    schwachen Begleitern zum Leveln (A579). Teilweise widerlegt: Gelernt und gelesen wird mit demselben Maß, und jede
    Stufe hält den Höchstwert ihrer Läufe. Erst zwei Läufe senken einen Wert, und jeder Lauf hebt den Wert seiner Stufe,
    wenn er schwerer war. Die bessere Messung, nach den Begleitern auf den Hörnern, scheitert am Lesen: Vor dem Kampf,
    für den Weg und die Rast, steht nicht fest, wer auf die Hörner kommt. *Preis:* Ein Lauf, der schwerer wird als seine
    Stufe, kostet höchstens die Rast oder den Kauf, die der alte Höchstwert verlangt hätte. Die Heilung im Kampf und die
    Trankschwelle lesen keinen dieser Werte und bleiben unberührt. Was ein Kampf über seinen Wert hinaus kostet, zeigt
    `LoadLeft` als unbekannt, und RSR rechnet für den Rest auf der sicheren Seite.
  - *Gegenthese 3 – Ausgeliefert, und nichts ändert sich:* (a) Ist der Rang im Teamfenster nicht lesbar, ist die Stufe
    unbekannt, und es gilt der Höchstwert über alle Läufe wie bisher. Die Spur zeigt das an der Zeile „crucible team rank".
    (b) Stehen alle Begleiter schon über dem Synchronrang, lernen die neuen Läufe auf der obersten Stufe. Die alten Läufe
    mit schwächeren Begleitern stehen nur im Höchstwert über alle Läufe, und nach zwei neuen Läufen gilt die oberste
    Stufe. (c) Ein Kader, der schwächer ist als jeder gemessene, findet keine Stufe unter sich. Dann gilt der Höchstwert
    über alle Läufe, also dasselbe wie bisher.
  - *Ausgeschlossen:* (1) Den Höchstwert verfallen lassen oder nach jedem Rangaufstieg löschen. Das verwirft auch die
    schweren Läufe, die noch gelten, und jeder Aufstieg begänne mit einem Brett ohne Messung. (2) Die Last nach dem
    Verhältnis der Werte im Blatt umrechnen. Das braucht eine Schadensformel der Begleiter, und die nennt keine Quelle.
- *K15 – Punkte der Bewertung* (seine Hinweise vom 11.11.2026, Grundlagen oben; A689). *Im Kampf und auf dem Weg:* Jeder
  Lauf vom 11.11.2026 verschenkte „kein Gegenstand" (750) schon vor dem ersten Kampf mit Sand der Zeit als Versicherung,
  „keine Marken ausgegeben" (500) mit dem ersten Kauf und „keine Ausrüstung" (750) mit einem Schatz; Lauf 3 rastete
  (1.000). Gestorben ist er dabei nie; die schwerste Last war der Endkampf mit 60 % seiner LP ohne Ausrüstung.
  - *Grundsatz (für jeden Bonus, den eine Handlung RSRs oder eine Empfehlung verwirkt):* Ein noch erhaltener Bonus wiegt
    gegen den Sicherheitsgewinn der Handlung. Sie geht, wo der Lauf ohne sie nicht sicher ist, sonst nicht. Ist der
    Bonus verwirkt, kostet jede weitere gleiche Handlung nichts mehr, und sie geht wie bisher. Gemessen wird die
    Sicherheit mit dem Test des Wegs (A663): Der Spieler übersteht jeden Kampf bis einschließlich Endkampf mit dessen
    schwerster gelernter Last, und der schwächste Endkampfheiler übersteht einen Einsatz. Die schwerste je gesehene
    Last ist die pessimistische Schätzung; ein Lauf, der sie trägt, ist mit hoher Wahrscheinlichkeit sicher. Wo ein
    Kampf auf dem Weg nie gemessen wurde, ist die Sicherheit unbekannt, und die Handlung geht wie bisher (sichere Seite).
    Das ist seine Spielweise auf die Punkte übertragen: Schutz, wo der Schaden wahrscheinlich ist, kein Schutz, der
    ohne Gefahr Punkte kostet.
  - *Woran ein leichtes Brett feststeht (A705, A706, A709; sein Auftrag vom 11.11.2026: „sobald feststeht, dass das
    aktuelle brett einfach zu lösen ist, können weniger begleiter mitgenommen werden, kann auf essen verzichtet werden,
    kann auf campen verzichtet werden"):* An dem, was er auf dem Weg wirklich verliert, Kampf für Kampf, ohne die Mittel,
    um die es geht.
    - *Sachstand vor A706:* Der Test des Wegs summierte je Kampf den rohen Schaden am Spieler (`LearnLoad`). Die Heilung
      eines Begleiters fehlte darin. Gelernt waren am 11.11.2026 für Brett 1 7, 22, 22, 21 und 60 % seiner LP ohne
      Ausrüstung, zusammen 132 %, für Brett 2 zusammen 151 % (`CrucibleFightLoads.json` aus seinem Upload): Am Anfang
      eines Bretts trug er den Weg nach dieser Rechnung nie.
    - *Was gemessen wird:* je Kampf zwei Größen, als Anteil seiner LP ohne Ausrüstung, je die größte über alle Läufe:
      der *Nettoverlust* (LP zu Beginn weniger LP am Ende) und die *tiefste Senke* (LP zu Beginn weniger die niedrigsten LP
      im Kampf) – beide **ohne seine eigene Heilung** (Gegenstände, eigene Aktionen), mit der Heilung der Begleiter.
      Die eigene Heilung zieht RSR in dem Augenblick ab, in dem seine LP um sie steigen, längstens einen GCD nach
      ihrer Meldung (A720): Was bis dahin nicht als Anstieg ankam – ein Treffer im selben Augenblick nahm es –, verfällt,
      statt später die Heilung eines Begleiters als eigene abzuziehen und den Kampf schwerer zu schreiben, als er war.
      A706 zog sie nicht ab: Es maß, was mit den Gegenständen übrig blieb, und sparte dann genau diese Gegenstände ein.
    - *Der Test:* Der Weg verlangt so viele LP, dass er in jedem Kampf über der Trankschwelle bleibt („Use HP potions
      below this HP") – unter ihr ginge ohnehin ein Heilgegenstand, und Sparsamkeit wäre verloren. Der Bedarf eines Wegs
      ist, von hinten gerechnet: die Senke des Kampfs, oder sein Nettoverlust plus der Bedarf des Wegs danach, je nachdem
      was größer ist; über Abzweigungen der schwerste Weg, an einem Zufallsfeld der schwerste Kampf. Er trägt den Weg,
      wenn seine LP (mit gehaltener Heilung, wo die Regel sie zählt) über Bedarf plus Trankschwelle liegen. Ein Kampf
      ohne Messung macht den Test unbekannt (sichere Seite). `CarriesToFinal` rechnet so; Sparsamkeit,
      Sammelleidenschaft, Geizhals, Mit leeren Händen, Schlaflose Nächte und Heilfasten lesen es. Die Teamgröße liest
      die Einsätze der Begleiter (K7). Die Wegempfehlung rechnet seit A721 mit denselben beiden Größen (K10, „Weg",
      Felder). Die rohe Last (A661) bleibt für die Rast nach K10, den Laden und die Heilung, die kein Kampf mehr
      braucht, wo sie Mittel dosiert: Sie rechnet ohne die Heilung der Begleiter und hält deshalb eher zu viel Heilung
      vor als zu wenig – der Preis sind Marken und ungenutzte Heilgegenstände am Ende, nicht LP.
    - *Modell an seinen fünf Läufen vom 11.11.2026* (ACT, gerechnet wie der Code misst: die eigene Heilung abgezogen,
      sobald die LP um sie steigen; Anteile der LP mit der Ausrüstung des jeweiligen Laufs; Nettoverlust/Senke, je der
      größte über die Läufe): Brett 1 – Kampf 1 7/7, Kampf 3 23/23, Kampf 4 4/4, Kampf 5 23/23, Endkampf 12/12 %. Bedarf
      ab Brettbeginn 68 %, mit 40 % Trankschwelle 108 %: zu Beginn trägt er Brett 1 nicht. Vor dem Endkampf verlangt der
      Weg 12 + 40 = 52 %; er stand dort bei 48 % (Lauf 1), 82 % (Lauf 2) und 83 % (Lauf 3) – in den Läufen 2 und 3 trug
      er ihn: Die Rast in Lauf 3 und Futter und Käufe vor dem Endkampf waren dafür nicht nötig. Brett 2 – 12/12, 30/30,
      11/11, 8/8, Endkampf 48/48 %: ab Brettbeginn 109 %, vor dem Endkampf 48 + 40 = 88 %; er stand dort in beiden
      Läufen bei 96 % und trug ihn. Die erste Fassung dieses Modells (A709) zog die eigene Heilung schon mit ihrer
      Meldung ab, bevor die LP stiegen, und rechnete die Senken der Kämpfe mit Heiltrank zu tief (Kampf 5 43, Endkampf
      Brett 1 36, Kampf 10 33, Endkampf Brett 2 76 %); der Code misst seit A709 beim Anstieg (C186).
      *Ergebnis:* Leicht ist ein Brett nicht als Ganzes, sondern ab der Stelle, von der an der Rest leicht ist; RSR
      stellt das an jeder Entscheidung neu fest. Mit stärkeren Begleitern sinken die Verluste, und die gelernten Werte
      folgen ihnen je Rangstufe („Gelernte Werte und der Rang der Begleiter", A729).
    - *Gegenthesen:* (1) *Kein Defekt:* widerlegt – A706 lernte für den Endkampf von Brett 1 einen Nettoverlust von 0 %,
      weil ein Bestien-Heiltrank 505 LP heilte. (2) *Option falsch – ohne die Gegenstände wären auch die Begleiter anders
      eingesetzt worden:* Der Test misst, was mit dem Kader und der Hornwahl des gemessenen Laufs geschah; hält die
      Vorhersage nicht, greift die Heilung unter der Trankschwelle wie bisher, und der Kampf lernt seinen höheren Wert.
      Preis: ein Bonus, der in diesem Lauf verloren geht. (3) *Ausgeliefert, und nichts ändert sich:* Bei seinen Werten
      trägt er beide Bretter erst vor dem Endkampf – dort entfallen Rast, Futter und Käufe; mehr ändert sich, sobald
      seine Begleiter den Rang des Bretts erreichen.
  - *Kein Gegenstand (750; „Sparsamkeit", `XBMScoreBonus` 15, gebaut A689):* Unmittelbare Gefahr geht vor und bleibt ungeschmälert: Heilung unter der Trankschwelle,
    Mittel gegen Gift und Blindheit, Schutz gegen einen angekündigten Treffer, der ihn unter die Trankschwelle brächte,
    Heilung und Schutz des Begleiters, der vor dem nächsten Webfenster fiele. *Wann ein angekündigter Treffer Gefahr ist
    (A704):* Solange ein Gegenstandsbonus auf dem Spiel steht und er den Weg trägt, gehen Schutz (Schritt 6) und
    Elementschutz (Schritt 9) nur, wenn der Treffer nach seiner gemessenen Größe – gezielt: die Tabelle gezielter Treffer
    (`AimedHitLeaves`, A540), Fläche: der angekündigte Anteil (`AnnouncedAreaShare`) – ihn unter die Trankschwelle
    brächte, oder wenn seine Größe nicht gemessen ist (sichere Seite). Der Schutz im Endkampf, „während ein Gegner ihn
    angreift", ist dann Versicherung und bleibt liegen. *Im Kampf (seine Läufe vom 11.11.2026):* Tome of the Impervious
    ging bei 78 % LP gegen den Tankbuster der Terrormilbe, Wildfire Weakener bei 63 % gegen einen Feuertreffer des Ogers;
    kein Treffer nahm in diesen Läufen mehr als 17 % seiner LP – beide kosteten Sparsamkeit (750), ohne dass ein Treffer
    ihn unter 40 % gebracht hätte. *Gegenthese 1 – Gift nach dem Treffer:* Der Tankbuster der Milben legt Gift; die Last
    des Gifts trägt das Gegenmittel gegen Gift, nicht der Schutz. *Gegenthese 2 – Die gemessene Größe unterschätzt:* Die
    Tabelle hält je Treffer den größten gemessenen Anteil; ein größerer bringt ihn unter die Trankschwelle, dort greift
    die Heilung, und die Tabelle lernt den größeren Anteil. *Preis:* einmal Heilung unter der Schwelle statt Schutz davor. *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Die Größe ist auf einem Brett nie
    gemessen – dann geht der Schutz wie bisher, und die Tabelle lernt aus dem Treffer. Die Versicherungen ohne Gefahr – Sand der Zeit, Wiederaufstehen, Auto-Trank gegen
    einen Endgegner – und Schaden aus Gegenständen gehen bei erhaltenem Bonus nur, wenn der Spieler die schwerste Last
    bis einschließlich Endkampf nicht mit seinen LP trägt (`CrucibleRoute.WorstAhead` ohne Lager, mit der getragenen
    Ausrüstung). Heilung, die kein Kampf mehr braucht, bleibt im Endkampf bei erhaltenem Bonus liegen: Was verbleibende LP
    an Punkten bringen, nennt keine Quelle (unbelegt), die 750 sind belegt.
  - *Keine Marken ausgegeben (500; „Geizhals", 19), keine Ausrüstung (750; „Mit leeren Händen", 13; gebaut A690):*
    - *Laden:* Solange in diesem Lauf nichts gekauft wurde und der Spieler mit seinen LP und der gehaltenen Heilung die
      schwerste Last bis einschließlich Endkampf trägt, empfiehlt der Laden nur den Schutz gegen einen Zustand der
      kommenden Kämpfe, der unabhängig von den LP tötet (Stufe 2, etwa Verhängnis auf Brett 2): Gegen ihn hilft keine
      LP-Rechnung. Kein zweites Leben, keine Ausrüstung, keine Heilung. Gekauft gilt, sobald der Laden eine Zeile als
      gekauft zeigt, von RSR oder von Hand.
    - *Schatz:* Solange in diesem Lauf keine Ausrüstung erworben ist und der Spieler den Weg trägt, empfiehlt der Schatz
      das beste Angebot ohne Ausrüstung, wenn es eines gibt; sonst wie bisher.
    - *Im Kampf:* Auf Brett 1 verliert er so weder 500 noch 750 für Käufe, die die gelernten Lasten nicht verlangen;
      auf Brett 2 kauft er weiter den Schutz gegen Verhängnis, wenn der Laden ihn führt.
    - *Gegenthese:* Ein zweites Leben schützt gegen alles. Teilweise widerlegt: Der Lasttest deckt die Kämpfe, die
      gemessen sind; die unmittelbare Gefahr im Kampf bleibt gedeckt. Preis wie bei Sparsamkeit (Gegenthese 1).
    - *Futter im Laden:* folgt der Futterregel (Heilfasten, unten, A691).
    - *Spur:* „crucible score: shop kept …", „a purchase in this run …", „treasure without gear …".
  - *Nicht gerastet (1.000; „Schlaflose Nächte", 21), nicht gefüttert (750; „Heilfasten", 17; gebaut A691):* Rast und
    Futter dienen den Begleitern und dem Spieler. Nicht nötig sind sie, wenn der Spieler mit LP und gehaltener Heilung
    die schwerste Last bis einschließlich Endkampf trägt und der schwächste Endkampfheiler jetzt mehr als einen Einsatz
    hat – K11 hält ihn zurück, bevor er unter einen Einsatz fiele, er kommt also mit einem an. Für die Rast genügt
    auch ein voller Heiler: Eine Rast hebt ihn nicht über volle LP (A716). Für das Futter nicht: Futter hebt sein
    Maximum und senkt, was er nimmt, und kann ihm den Einsatz erst tragen. Dann:
    - *Lager:* Solange im Lauf nicht gerastet wurde, rät das Fenster, ohne Rast weiterzugehen, nennt den Punktpreis und
      wählt niemanden aus; die Rast bestätigt er selbst. Als gerastet gilt ein Lauf, sobald das Spiel den Anteil der
      Rast anbietet (die Abfrage vor der Rast); bricht er dort ab, gibt RSR den Bonus trotzdem auf – die sichere Seite.
    - *Futter:* Solange im Lauf kein Begleiter gefüttert wurde, empfiehlt der Laden kein Futter für den Endkampf.
      Gefüttert gilt, sobald RSR an einem Begleiter belegte Futterplätze liest.
    - *Im Kampf:* In Lauf 3 vom 11.11.2026 ruhten Raptor und Fliegenfalle vor dem Endkampf; er kam mit 96 % heraus, der
      Endkampf kostete höchstens 60 %. Ohne Rast hätte er 1.000 Punkte behalten.
    - *Gegenthese:* Begleiter, die nicht Endkampfheiler sind, kämpfen mit weniger LP und fallen eher („Keine
      Verluste", 1.000). Teilweise widerlegt: Fällt einer, verliert der Lauf diesen Bonus, nicht den Sieg; Preis
      benannt. Ein gefallener Begleiter kommt mit dem Horn der Rückkehr zurück.
  - *Weg (gebaut A689):* Unter sicheren Wegen der mit mehr Punkten – Gegnerfeld 250, Elitefeld 800 –, dann der, der bei erhaltenem
    Bonus ohne Gegenstände sicher ist, dann wie bisher mehr Felder mit Gegenständen, mehr LP. Ein Lager auf dem Weg
    kostet nur, wenn gerastet wird.
  - *Woher RSR weiß, was erhalten ist:* je Lauf (`CrucibleBoard.Run`) aus dem, was es sieht – Gegenstände im
    Crucible-Inventar weniger geworden (benutzt, von RSR oder von Hand; auch weggeworfen, dann gibt RSR den Bonus auf,
    die sichere Seite), Marken weniger, Ausrüstung im Lauf getragen, gerastet, gefüttert. `XBMScoreBonus` führt Namen
    und Bedingungen, die Punkte je Bonus und Brett führt `XBMContent.BonusPoints`: 33 Einträge, einer je Zeile von
    `XBMScoreBonus` (EXDSchema latest; in Lumina benannt im experimentellen Blatt, dessen Diagnose `PendingExcelSchema` RSR an der einen
    Stelle unterdrückt, im regulären als `Unknown0` bis `Unknown32`;
    Werte geprüft an ffxiv-datamining 52a72b3 vom 10.11.2026). RSR liest sie für das laufende Brett, das `XBMContent` über
    die ContentFinderCondition nennt (1088 bis 1092; A695). Die Punkte je Feld (250, 800) führt kein Blatt; sie stehen
    fest im Code, mit Quelle (A689). Spur: „crucible score: …", wenn die Gegenstände für den Bonus liegen bleiben oder
    wieder gehen und wenn der Bonus fällt.
  - *Sammelleidenschaft (1.000, außer Brett 1; `XBMScoreBonus` 16; gebaut A695):* Zehn Gegenstände am Ende – alle zehn
    Plätze des Inventars belegt. Hält der Spieler zehn auf einem Brett, das den Bonus vergibt, kostet jeder Gegenstand
    ohne Gefahr den Bonus wie bei Sparsamkeit, und es gilt dieselbe Regel: Er bleibt liegen, solange der Spieler den Weg
    bis einschließlich Endkampf trägt. Unmittelbare Gefahr geht vor. Anders als Sparsamkeit kehrt der Bonus zurück,
    sobald wieder zehn Plätze belegt sind.
  - *Am Ende des Endkampfs (A695):* Die Rest-LP des Spielers zählen absolut, nicht als Anteil (consolegameswiki,
    FullBoosts, MMOGah; eine Rate nennt keine Quelle, unbelegt). Dazu Unversehrter Bändiger (7: Spieler bei 100 % LP)
    250 und Unversehrte Bestien (8: alle Begleiter bei 100 % LP) 1.000 auf jedem Brett.
    - *Auffüllen (gebaut):* Steht kein Gegenstandsbonus mehr auf dem Spiel (Sparsamkeit verwirkt, Sammelleidenschaft
      nicht zu halten) und fällt der letzte Gegner nach der Restzeit-Vorhersage vor dem nächsten GCD
      (`PackTimeToKill` höchstens `DefaultGCDTotal`), nimmt die übrige Heilung den kleinsten Gegenstand, der den Spieler
      auf 100 % bringt; Überheilung kostet dort nichts mehr. Steht noch ein Gegenstandsbonus auf dem Spiel, geht in
      diesem letzten GCD gar keine übrige Heilung mehr, auch wenn der Weg vorher als nicht getragen galt (A712): Der
      Kampf endet, und der Bonus wiegt mehr als die 250. Vorher wie bisher der größte ohne Überheilung: Die LP am
      Ende sind Anfangs-LP plus jede Heilung minus jeder Schaden, solange nichts überheilt – wann ein Gegenstand geht,
      ändert sie nicht, nur der letzte entscheidet über die 100 %.
    - *Gegen einen erhaltenen Gegenstandsbonus:* Ein Heilgegenstand am Ende bringt 250 und die LP-Punkte seiner
      Heilung, gegen 750 (Sparsamkeit) oder 1.000 (Sammelleidenschaft). Ohne die Rate bleibt der belegte Bonus. *Preis:*
      Liegt die Rate über (750 − 250) je geheilten LP, verliert der Lauf Punkte. *Messmittel (gebaut):* RSR schreibt
      jedes Fenster des Crucible (Name beginnt mit „XBM") beim ersten Öffnen je Lauf mit seinen Werten in
      `DefenseTrace.log` („crucible window …"), also auch die Abrechnung am Ende samt LP-Punkten. Aus ihr folgt die Rate.
      Selbst nachsteuern kann RSR erst, wenn der Aufbau dieses Fensters bekannt ist; den nennt keine Quelle. Einen Text
      unter den Werten liest RSR über Dalamuds `SafeMemory`, Seite für Seite bis zum Nullbyte. Ein Zeiger auf
      unlesbaren Speicher steht als „unreadable“ in der Zeile. Bis A764 las RSR den Text ungeprüft. Am 13.11.2026 um
      18:50 traf das beim Öffnen eines Crucible-Fensters einen solchen Zeiger, und das Spiel stürzte ab
      (`AccessViolationException` in `BestiaryTrace.Values`; .NET kann sie nicht abfangen).
    - *Unversehrte Bestien (gebaut):* Fällt der letzte Gegner des Endkampfs vor dem nächsten GCD, stehen alle übrigen
      Begleiter des Kaders bei voller LP und fehlt dem Begleiter im Feld LP, geht der kleinste Heilgegenstand „to self or
      familiar", der ihn auf 100 % bringt, an ihn – vor dem Auffüllen des Spielers, 1.000 vor 250. Die LP der übrigen
      liest RSR wie für die Wahl zwischen den Kämpfen (K11, A585): was ein Begleiter zuletzt mit aus dem Kampf nahm,
      solange der Agent den Kampf nicht übernommen hat, sonst den Agenten; ein gefallener zählt mit 0. Er geht, wo die
      Punkte des Bonus (`XBMContent`) die Gegenstandsboni übersteigen, die der Gebrauch kostet: Sparsamkeit, solange sie
      steht, und Sammelleidenschaft, solange zehn gehalten werden. Auf jedem Brett also gegen Sparsamkeit allein (1.000
      gegen 750), nicht gegen beide (1.000 gegen 1.750). Crucible Ash heilt „self and nearby allies" und erreicht den
      Begleiter beim Gegner nicht (A577); es zählt hier nicht. *Wahrscheinlichkeit:* Ohne Rast trägt jeder Begleiter,
      der kämpfte, seine Verluste bis ans Ende (K11, gemessen 04.11.2026: 25 bis 60 % bei mehreren); der Fall tritt ein,
      wenn außer dem Begleiter im Feld keiner LP verlor oder alle wieder voll sind. Wo er nicht eintritt, kostet die
      Regel nichts.
    - *Gegenthesen:* (1) Ein Treffer nach dem Auffüllen senkt die LP wieder. Innerhalb eines GCD vor dem Ende ist das
      möglich; dann fehlen die 250, die LP-Punkte bleiben höher als ohne die Heilung. (2) Die Restzeit-Vorhersage irrt:
      Der Gegenstand geht zu früh (wie (1)) oder gar nicht – ohne Bonus auf dem Spiel ist er nach dem Ende wertlos, ein
      Verlust entsteht nicht, nur kein Gewinn. (3) Ausgeliefert, und nichts ändert sich: Kein Heilgegenstand ist am Ende
      übrig, weil die übrige Heilung ihn vorher ohne Überheilung nahm. Die LP-Punkte sind dieselben (oben); nur die 250
      fehlen, wenn kein Gegenstand bis 100 % übrig bleibt.
  - *Nicht hier:* Teamgröße und Gattung (bis 4.250 und 750) gehören zum Kader (K7, „Teamgröße nach Punkten", „Team aus
    einer Gattung").
  - *Gegenthese 1 – Die schwerste gelernte Last unterschätzt den nächsten Lauf:* Ein Kampf kann schwerer werden als je
    gesehen (anderer Begleiter, Pech bei Zuständen). Seit A729 liest RSR die Lasten der Läufe mit einem gleich starken
    oder schwächeren Kader, mindestens zwei Läufe, und nicht mehr den Höchstwert über alle; die gelesene Last liegt
    dadurch näher am Lauf („Gelernte Werte und der Rang der Begleiter"). Teilweise widerlegt: Die unmittelbare Gefahr im Kampf bleibt
    abgedeckt – Heilung unter der Trankschwelle, Schutz vor angekündigten Treffern –, nur die Versicherung im Voraus
    entfällt. Preis: Ein Tod, den Sand der Zeit aufgefangen hätte, wenn ein Kampf alle LP über der Trankschwelle in
    einem Zug nimmt; in seinen fünf Läufen vom 11.11.2026 (Brett 1 und 2) nahm kein Treffer mehr als 17 % seiner LP
    (Magischer Schlag des Pisco-Dämonen-Ebenbilds, `DefenseTrace.log`).
  - *Gegenthese 2 – Er will die Punkte nicht:* Er nannte die Ausbeute selbst als Ziel; Sicherheit bleibt durch den Test
    gewahrt.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Ohne gemessene Lasten (erster Lauf auf einem Brett) gilt der
    Lauf als unbekannt, und alles geht wie bisher. Ab dem zweiten Lauf greift die Regel.
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
  - *Empfehlung an der Campsite:* Das Fenster „RSR - Crucible campsite" zeigt, wen RSR vorwählt und warum (K10: so
    viele, wie deine LP danach die Strecke bis zur nächsten Rast tragen, A661). An der letzten Campsite empfiehlt es
    dazu, wer für den Endkampf mitruhen soll:
    unter den stehenden, nicht vollen Begleitern, deren Futter die Rast nicht kostet (K10, „Futter an der Rast"), zuerst
    die Heiler und Schützer aus dem Plan des Endkampfs
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
    jedem Kampf, an der Campsite, beim Füttern –, für diesen Lauf (A662: bis dahin für das Gebiet; ein neuer Lauf auf
    demselben Brett ist dasselbe Gebiet und erbte so die Plätze und die Ausrüstung des vorigen, bis das Fenster wieder
    stand). Ein Begleiter frisst dasselbe Futter nicht zweimal: Das Spiel lehnt
    eine Wiederholung ab (LazyCrucible, dajoey/lalalazy cb5bf32, `DESIGN.md` §2: in ihren aufgezeichneten Futterfenstern
    war jede Ablehnung eine Wiederholung; Werkzeugquelle) – RSR empfiehlt keinem Begleiter ein Futter, das er schon trägt
    oder das ihm die Liste schon zuteilt (A679). Empfohlen wird nur für einen freien Platz, Stück für Stück (A662): das Paar aus Futter und Begleiter, das
    einem Heiler oder Schützer des Plans am meisten bringt, dann das, das einem der übrigen am meisten bringt, so weit die
    Marken nach der Kaufliste reichen; darüber hinaus mit Vermerk. Die Liste nennt die Reihenfolge, und beim Füttern
    nennt RSR für das Futter in der Hand nach derselben Regel den Empfänger – wer der Liste folgt, bekommt dort dieselben
    Namen. Den Begleiter nennt das Fenster mit seinen LP, wie das Begleiterfenster sie zeigt („Kolibri 374/487"), nicht
    nur mit einem Anteil. Hat RSR das Fenster in diesem Lauf noch nicht gesehen, oder liest es keinen Höchstwert,
    gilt je Begleiter ein Platz, und das Fenster sagt es. *Warum Stück für Stück (A662):* Bis A662 verteilte die Liste
    reihum (je Begleiter einer, Heiler zuerst, dann die zweiten Plätze), das Füttern nannte den mit dem größten Gewinn –
    zwei Regeln. In seinem Laden auf Zug 8 (07.11.2026) stand in der Liste Meerengel für den Goobbue und Champignon für
    den Kolibri; nach dem Meerengel nannte das Füttern für den Champignon den Goobbue. Reihum hing zudem an der
    Kaufreihenfolge: Wer erst den Meerengel fütterte, bekam für den Champignon wieder den ersten in der Runde. Stück für
    Stück, Heiler zuerst, folgt dem Zweck des Futters (die Heiler des Endkampfs halten dich und die anderen) und gibt in
    jeder Reihenfolge, die der Liste folgt, dieselben Empfänger. *Gegenthese:* Ein Schadensbegleiter auf wenig LP bekommt
    nichts, solange einem Heiler ein Futter noch etwas bringt. Nicht widerlegt, Preis benannt: Schaden vor Sicherheit
    tauscht seine Vorgabe nicht; sein Futter kommt, sobald die Heiler nichts mehr gewinnen oder keinen Platz haben. Grund
    der Plätze: Vorher empfahl RSR jedem Begleiter eins, ohne seine
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
    Fenster, wem es zugedacht ist, nach den Plätzen, die das Fenster jetzt zeigt, mit der Regel der Liste: unter den
    Heilern und Schützern des Endkampfplans mit freiem Platz der, dem es am meisten bringt, sonst unter den übrigen; hat
    keiner einen freien Platz oder bringt es keinem etwas, sagt es das. RSR klickt dort nichts.
  - *Sperre:* Die Campsite-Wahl (K10) erkennt das Lager an Modus 4. Den nennt ClientStructs „buy feed result"; Daedalus
    sah ihn im Spiel an der Campsite. Läuft ein Futterkauf (Modus 3 oder 5 gesehen, seit das Fenster offen ist, oder ein
    Futter im Fenster), klickt RSR dort nicht: Ein Klick wählte sonst, wer frisst.
  - *Verworfen:* (a) Am letzten Lager ohne Klick für die Begleiter rasten, auch wenn du nicht voll herauskommst – die
    Bilanz hängt an Endkampf-Lasten, die nur für dich gemessen sind; (b) Futter schon an jedem Laden – vor einer Campsite
    geht es verloren oder hält den Begleiter vom Rasten ab; (c) Futter nach Schaden zuerst – Sicherheit vor Schaden.
  - *Der Plan des Endkampfs (A662; seine Meldung vom 07.11.2026: „ich sollte gobue füttern und dann wird sie nicht für
    den kampf genommen. also war das futter verschwendet"):* Futter, Rast für den Endkampf und Reserve (K11) lesen einen
    Plan; das Hornfenster des Endkampfs wählt die Hörner. Beides ist jetzt dieselbe Rechnung mit denselben Eingaben: LP
    jetzt (K11, wie das Hornfenster sie liest), „übersteht den Einsatz", synchronisierter Rang, Futter. RSR rechnet den
    Plan neu, sobald sich davon etwas ändert, außerhalb des Kampfes auf dem Brett, nicht nur am Hornfenster vor einem
    anderen Kampf (wie bisher nur mit „Set the battlehorns between fights", denn ohne sie wählt RSR die Hörner nicht).
    „Übersteht den Einsatz" zählt im Plan mit den LP von jetzt erst, wenn keine Campsite mehr kommen kann: Davor zählen
    alle als überstehend, weil eine Rast ihre LP noch hebt und der Plan sagt, wen sie heben soll (K11: ein knapper
    Endkampf-Heiler ruht zuerst); danach sind die LP von jetzt die im Endkampf. Futter zählt in jeder Hornwahl auf dem Brett (K6): Ein Begleiter mit „Damage Dealt +x %" bringt
    entsprechend mehr Schaden, einer mit „Damage Taken −x %" übersteht einen Einsatz, wenn seine LP über dem größten
    Verlust je Einsatz mal dem verbleibenden Anteil liegen. Damit hebt Futter nur den Wert dessen, der es frisst: Ein
    Plan, der ihn enthielt, bleibt der beste oder wird von einem anderen mit ihm geschlagen – das Futter geht nicht an
    einen, den die Hornwahl danach liegen lässt. *Im Kampf vorher:* Der Plan stammte vom Hornfenster vor dem Elitekampf,
    mit den LP von damals, ohne „übersteht den Einsatz" (alle zählten als überstehend) und ohne Futter; am Endkampf wählte
    RSR mit den LP danach, mit dieser Prüfung und wieder ohne Futter neu. In seinem Lauf stand im Laden „Meerengel →
    Goobbue, heals or protects in the final" und „Champignon → Kolibri"; gefüttert wurden der Goobbue, am Endkampf lagen
    Karpfenkröte, Morbol und Kobalos auf den Hörnern. Welche der Abweichungen dort den Ausschlag gab, zeigt keine Spur
    (keine Datei zu diesem Lauf); dass zwei Rechnungen mit verschiedenen Eingaben liefen, ist am Code belegt.
    *Gegenthese (gleichauf):* Unter gleichem Schaden entscheidet am Hornfenster noch, wer schon auf einem Horn steht
    („behalten", B5). Das ändert den Plan nur bei exakt gleichem Schaden, nach Heilern und „übersteht den Einsatz".
    *Gegenthese (LP fallen nach dem Futter):* Auf Brett 1 liegt zwischen dem letzten Laden und dem Endkampf kein Kampf
    (Zug 8 Laden, Zug 9 Endgegner); liegt auf einem Brett einer dazwischen, rechnet RSR den Plan danach neu, und das
    Futter kann an einen gegangen sein, der dort fiel – derselbe Preis wie jede Vorbereitung vor einem Kampf. *Gegenthese Futterplätze:* Ruht ein
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
  Heilung, mit der Mehrheilung deiner Ausrüstung, höchstens was du noch heilen kannst (K10, A659). Schützender Gegenstand
  (A668): was er im schwersten gemessenen Kampf abfängt, solange er steht – die Minderung ihren Anteil am Schaden im besten
  Fenster ihrer Dauer, die Reflexion den magischen Schaden darin, Blink die physischen Treffer in Folge, so viele wie
  Stapel, innerhalb seiner Dauer. Die Treffer misst RSR wie die Last (Zeit, Schaden, Angriffsart, in diesem Gebiet, sonst
  über alle Bretter seit dem Laden); ohne gemessene Treffer zählt der ganze Kampf. Dauer und Stapel liest RSR aus dem
  Wirktext („Duration: 60s", „Grants five stacks of Blink"). *Paare:* Ausrüstung, die die Dauer eines Gegenstands
  verlängert („When carrying an ice shield, duration increases to 90s"), zählt den Zuwachs auf die gehaltenen; ein
  angebotener Gegenstand zählt mit der Dauer, die deine Ausrüstung ihm gibt. Ein Mittel, dessen Status einem gehaltenen
  Gegenstand mehr gibt (Spirit of the Breathtaker: drei Blink mehr), zählt diesen Zuwachs. Trägst du eine Rote Schleife
  oder wählt die Skala sie, heilt jedes Serum 50 % seines Ziels („When carrying a crimson ribbon, restores 50% …"): Es
  zählt als Heilgegenstand, und die Schleife zählt die Heilung der gehaltenen Seren, beides höchstens was du noch heilen
  kannst. Der Arzneibeutel („Procures one random beast potion upon resting at campsites", A669) zählt je Campsite, die
  auf jedem Weg zum Endgegner liegt, den kleinsten Bestien-Trank (G1 bis G4: 10, 23, 36, 50 %, also 10 %), mit der
  Mehrheilung deiner Ausrüstung, höchstens was du noch heilen kannst: Welcher kommt, ist Zufall, und eine Campsite auf
  nur einem Weg ist nicht sicher – die Skala unterschätzt ihn eher. Eine Rauchbombe (A671) zählt die Last des schwersten
  Gegnerkampfs auf dem kürzesten Weg zum Endgegner, den die gehaltenen Bomben noch nicht abdecken (die zweite Bombe den
  zweitschwersten); das Feiglingsmesser seine Fluchtchance mal die Last aller Gegnerkämpfe auf diesem Weg – beides nur,
  weil das Wegfenster die Flucht empfiehlt (Weg, „Flucht"). *Grund (A668):* Bis dahin zählte ein Schutzgegenstand den ganzen schwersten Kampf. Auf Brett 1 dauerte ein Kampf
  61 bis 148 s (15 Kämpfe am 07.11.2026, ACT-Mitschnitt); das beste 30-s-Fenster hielt 51 bis 100 % seines Schadens, im
  Endkampf 55 %, das beste 60-s-Fenster im Endkampf 83 %, das 90-s-Fenster 92 %; die fünf stärksten Treffer in Folge 48
  bis 100 %, im Endkampf 48 % (27 Treffer, 23 davon Auto-Angriffe; alle Werte über alle Angriffsarten, den magischen
  Anteil trennt der Mitschnitt nicht). Ein Gegenstand von 30 s zählte so im Endkampf fast doppelt und konnte einen
  Heilgegenstand verdrängen, der mehr bringt. Eine gleichmäßige Verteilung (Dauer durch
  Kampflänge, dort 20 bis 49 %) unterschätzt: Der Schaden ballt sich. *Gegenthese A668-1 (bestes Fenster):* K8 setzt
  Schutz gegen einen angekündigten Treffer oder im Endkampf, solange ein Gegner dich angreift – nicht genau am Anfang des
  besten Fensters. Nicht widerlegt; das beste Fenster ist eine obere Grenze, die Skala überschätzt Schutz weniger als
  zuvor, nicht gar nicht – Preis benannt. *Gegenthese A668-2 (Schleife ohne Seren):* Seren, die erst ein späterer Laden
  bringt, zählen beim Kauf der Schleife nicht. Nicht widerlegt; was ein Laden anbietet, steht nirgends. *Gegenthese
  A668-3 (Messung der Sitzung):* Die Treffer gelten seit dem Laden von RSR; nach einem Neustart zählt bis zum ersten
  Kampf der ganze Kampf. Widerlegt für die Skala: Ohne gemessenen Kampf gilt sie ohnehin nicht (Reihenfolge oben).
  *Heiltranksammlung (A670):* Auto-potion stellt 40 % her, sobald deine LP unter 50 % fallen, und verfällt beim Auslösen
  mit 50 % (unter Spirit of the Breathtaker 10 %); K8 setzt sie zu Beginn eines Bosskampfs, sie steht 150 s (mit
  Diebesgewand 240 s). Die Skala zählt, was davon unter die Linie fällt: von dem, was du noch heilen kannst, der Teil, der
  dich unter 50 % brächte, davon der Anteil des schwersten Kampfes in den ersten 150 s ab seinem ersten Treffer, und
  davon der Erwartungswert der Auslösungen – die erste sicher, jede weitere mit der Chance, dass die vorige nicht verfiel,
  jede höchstens, was dann noch fehlt. Alle Werte liest RSR aus dem Text. Das Diebesgewand zählt den Zuwachs der Dauer,
  das Flinkheitsmittel den der Chance. *Beispiel (ohne Mehrheilung, der schwerste Kampf ganz in 150 s):* Fehlen dir bis
  einschließlich des Endkampfs nach deinen Tränken noch
  90 %, brächte dich das 40 % unter die Linie: Die erste Auslösung zählt 40 %. Fehlen 120 %: 40 % und mit 50 % weitere
  30 %, zusammen 55 %; mit dem Flinkheitsmittel 67 %. Fehlen nur 40 %, zählt sie nichts – deine Tränke tragen dich, und
  K8 setzt die Sammlung ohnehin erst gegen einen Boss. *Gegenthese A670-1 (gehaltene Sammlung):* Eine gehaltene Sammlung
  zählt nicht in der Heilung, die du schon hältst; die Skala kann für dieselbe Not einen Trank dazu wählen. Nicht
  widerlegt; ihr Wert hängt an derselben Not, die er deckt – Preis: ein Trank zu viel, nie einer zu wenig.
  *Gegenthese A670-2 (Bosskampf kürzer):* Dauert der Bosskampf kürzer als 150 s, bringt das Diebesgewand nichts; das
  zählt die Skala so, weil der Anteil nach 150 s dann 0 ist. Die
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
  *Der Laden:* Er wählt mehrere Stücke mit Marken auf derselben Skala, Stück für Stück nach dem Wert je Marke (K10,
  „Eine Skala, sobald ein Kampf gemessen ist", A659).
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
      *Gesehen (sein Lauf vom 08.11.2026, Kampf 11; A683):* Der Ältere wurde bei 414 LP gehalten (Rücklage 576 LP).
      Danach war er für seinen Endlos-Schwung 26 s nicht anwählbar. Der Jüngere wurde bei 176 LP gehalten, bis der
      Ältere zurück war. Beide fielen 2 s nacheinander, ohne Endless Slashes. Weil RSR den Älteren in diesen 26 s nicht
      mitzählte, war der Jüngere frei und kam dem Fall auf etwa 3 s nahe; seitdem zählt jedes lebende Mitglied (Konzept
      03, „Tötungsregeln", Gegenthese 1).
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
    - *Königs-Ahriman-Ebenbild (14688), Zweites Meisterbrett – nur zu einer Zeit, sonst zuerst (A675):* Roulette (49418)
      bindet dich neben eine Sanduhr des Todes (final hourglass, 14689; deutsche Namen: xivapi v2, `language=de`) und setzt eine namenlose Nadel in die Mitte (BNpcBase 19738).
      Dass die Sanduhr schwer anzuvisieren war, behebt 7.58 (Patchnotes); RSR liest sie aus der Liste anwählbarer
      Gegner. Die Nadel zeigt nur
      entlang der vier Achsen und rückt in festem Takt eine Vierteldrehung weiter; bricht die Sanduhr, feuert der nächste
      Takt Death (49422, Kegel, 23 Yalm laut Blatt) entlang der Nadel, und Death legt Verhängnis (5421, „Certain death
      when counter reaches zero", nicht entfernbar; xivapi v2, 07.11.2026). Ungebrochen trifft Death ebenso, wenn ihr
      Wirken nach rund 25 s endet. Quellen: GluttonyCombo cb5bf32 (07.11.2026, BSD-3; `BST_CrucibleLogic`,
      `BST_CrucibleData`: Aufzeichnungen ihrer Läufe – Death tötete sechsmal von sechs, Takt 1,5605 s aus 16 Death, Drehung
      0,49 s nach dem Takt, Treffer bis Bruch 0,4 und 0,67 s, größter Treffer 67 % ihrer LP; ihre Halteregel laut
      Versionshinweis im Spiel unbestätigt), Icy Veins („The hourglass cleaves in the cursor's direction when it dies"),
      Tekz' Modul (Sanduhr Vorrang 6, Nadel −1). *Was gilt:*
      - *Zuerst:* Die Sanduhr geht jedem Gegner vor (Konzept 03, Regel 3): Ihr Wirkende tötet wie ein Bruch zur falschen
        Zeit. GluttonyCombo verlor einen Lauf, weil sie neben Hapalit und Dirty Eye nie angegriffen wurde.
      - *Nicht zur falschen Zeit:* Hat sie LP verloren, ist sie kein Ziel und trifft keine Fläche, solange ein Bruch durch
        einen Schlag jetzt Death auf dich lenken kann. Der Bruch landet frühestens nach der gemessenen Laufzeit eines
        Tastendrucks bis zur Antwort des Servers (`RoundTrip`) und spätestens einen Takt danach (die gemessenen 0,4 und
        0,67 s liegen darunter); Death feuert am ersten Takt danach – je Druck einer von zwei aufeinanderfolgenden Takten.
        Ein Takt trifft dich, wenn die Nadel dann in deine Halbebene zeigt und du in Reichweite von Death stehst (samt
        deines Trefferkreises): Die Breite des Kegels nennt kein Blatt, aber kein Kegel unter 180° erreicht dich von der
        Seite oder von hinten.
      - *Takt aus der Nadel:* Den Takt misst RSR am Abstand zweier Drehungen, den Drehsinn an der Drehung, den Versatz des
        Takts vor der Drehung an einem beobachteten Death mit der Drehung danach; bis dahin gelten GluttonyCombos
        Messungen (1,5605 s, 0,49 s).
      - *Nicht halten:* bei vollen LP – der erste Schlag bricht sie nicht (größter gemessener Treffer 67 %); wenn getragene
        Ausrüstung gegen Verhängnis feit („Doom Resistance +100%") – dann fällt sie, sobald es geht; solange die Nadel
        nicht gelesen ist (keine Drehung gesehen); und wenn der nächste sichere Druck erst nach dem Ende ihres Wirkens
        bräche – dann trifft Death ohnehin, und warten gewinnt nichts.
      - *Wie lange:* In deine Halbebene zeigen zwei der vier Richtungen, sobald du nicht genau auf einer Achse stehst – ein
        Viertel der Zeit ist frei, das Halten endet spätestens nach drei Takten (rund 4,7 s); ihr Wirken lässt rund 25 s.
        Der Preis ist Zeit, nicht LP: Eine schmalere Annahme (GluttonyCombos 60°) gäbe die Hälfte frei, träfe dich aber, wenn
        der Kegel breiter ist und du schräg stehst. Modell `roulette_needle_model.py` (07.11.2026): Bei Kegeln von 45°, 60°
        und 89° halber Breite und einem Verzug von 0 bis einem Takt bricht kein freigegebener Druck die Sanduhr auf dich;
        ohne Regel trifft Death in einem Viertel bis der Hälfte der Brüche, mit der Richtung zum Zeitpunkt des Drucks oder
        ohne Laufzeit ebenfalls.
      *Im Kampf:* Roulette bindet dich östlich der Mitte, etwas nördlich der Achse; die Nadel rückt jeden Takt weiter
      (Norden, Westen, Süden, Osten). In deine Hälfte zeigen Osten und Norden. Dein erster Schlag nimmt der Sanduhr 60 %.
      Als dein nächster GCD bereit ist, wären die möglichen Takte des Bruchs „Süden, Osten": RSR schlägt sie nicht, sondern
      das Hapalit-Ebenbild, bis die möglichen Takte „Westen, Süden" sind – höchstens drei Takte –; dann bricht der Schlag sie, und
      Death geht ins Leere. Ohne die Regel bräche der Schlag sie sofort, und Death träfe dich beim Takt nach Osten:
      Verhängnis, nach 6 s tot, Lauf beendet.
      *Gegenthesen:* (1) Dein Begleiter bricht sie zur falschen Zeit. Er folgt deinem Ziel (meist binnen 1,7 s, Konzept
      03, „Gegner mit Stacheln") und trifft sie also auch, nachdem RSR sie freigab, und noch bis zu 1,7 s in einen
      unsicheren Abschnitt hinein. Preis: Liegen ihre LP unter einem Schlag des Begleiters, bricht er sie mit der
      Wahrscheinlichkeit des unsicheren Anteils (die Hälfte, schräg drei Viertel) – nicht abwendbar ohne Befehl an den
      Begleiter, den der Beastmaster nicht hat; RSRs eigener Schlag, der größte, fällt nur noch in sichere Abschnitte.
      (2) Ein erster Schlag bricht sie doch (eine Stärkung über 50 % auf 67 %). Dann gilt der Zufall des Takts wie ohne
      Regel. (3) Ausgeliefert, und nichts ändert sich: Die Nadel ist kein Ziel und steht in keiner Gegnerliste. RSR sucht
      sie in der ganzen Objekttabelle nach ihrer Basis; findet es sie nicht, hält es nichts, und die Spur nennt es.
      *Verworfen:* GluttonyCombos Schwelle „75 % LP" und Kegelbreite 60°: Zahlen, wo das Spiel einen Zustand (LP verloren)
      und die Geometrie (Halbebene) liefert. Aus dem Kegel gehen: Roulette bindet dich.
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
  *Gegenthese 3:* RSR kennt das aktuelle Feld nicht. Widerlegt (A632, A656): Das Hornfenster nennt den Kampf, der auf
  genau einem Feld liegt, und RSR merkt sich, ob dessen Kampf vorbei ist. *Ergebnis:* K11 ist die Planungsebene über K6, K7, K8, K9 und K10; sie alle lesen dieselbe
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
  *Das Maß je Kampf statt je Brett (A710; sein Auftrag vom 11.11.2026: „analyse, welche begleiter am besten für mehrere
  Kämpfe auf dem brett geeignet sind").*
  - *Gemessen (fünf Läufe vom 11.11.2026, Hornfenster und Kader-LP im Protokoll; der Agent hängt einen Kampf nach, der
    Endkampf fehlt deshalb):* Was ein Einsatz kostet, hängt am Kampf und an der Rolle im Kampf, kaum am Begleiter. In
    jedem Kampf stehen drei Begleiter auf den Hörnern, und meist verliert einer fast alles: der, der zuletzt gerufen
    bleibt und unter Zähnefletschen trägt. Brett 1: Kampf 1 kostete je Einsatz höchstens 187 LP, Kampf 3 273, Kampf 4
    500, Kampf 5 522; Raptor stand zwölfmal auf einem Horn und verlor zusammen 882 LP, die Fliegenfalle viermal und
    verlor 1.209. Brett 2: bis 728 LP je Einsatz bei rund 1.000 LP je Begleiter. Für mehrere Kämpfe taugt also, wer in
    vielen Kämpfen eine Schwäche trifft (K7 zählt das) und dort nicht der ist, der bleibt; wer bleibt, taugt für einen
    schweren Kampf oder zwei leichte.
  - *Defekt:* Die Reserve (Stufe 1 und 2) maß mit dem größten Verlust irgendeines Einsatzes auf dem Brett. Nach dem
    ersten Lauf hieß es vor Kampf 1: „held for the final fight, a use costing up to 500 HP: Puk, Pugil" – Pugil mit 667
    LP galt als knapp (667 unter zweimal 500), obwohl Kampf 1 höchstens 187 LP je Einsatz kostete; in 17 von 25
    Hornfenstern hielt RSR Begleiter zurück. Die Heiler fehlten in frühen Kämpfen, die sie kaum etwas gekostet hätten.
  - *Optionen:* (a) Nullvariante. (b) Verlust je Begleiter lernen – er folgt der Rolle, nicht dem Begleiter (oben).
    (c) Verlust je Kampf: der größte eines Einsatzes in diesem Kampf, über die Sitzungen gelernt; für den anstehenden
    Kampf sein Wert, für den Endkampf seiner, ohne Messung der größte des Bretts wie bisher. (d) Dazu je Rolle (wer
    bleibt, wer kurz kommt) – die Rolle fällt erst im Kampf (B3), das Hornfenster kennt sie nicht.
  - *Abwägung im Kampf:* (c) *Stärken:* Ein Heiler des Endkampfs kommt in einem leichten Kampf auf ein Horn, wenn ihm
    danach noch ein Einsatz im Endkampf bleibt: Pugil mit 667 LP vor Kampf 1 (187) und einem Endkampf, dessen Einsatz
    gemessen ist. Die frühen Kämpfe haben einen heilenden Begleiter mehr zur Wahl. *Schwächen:* Der Wert eines Kampfs ist
    der des Begleiters, der dort blieb; ein kurz gerufener verliert weniger – das Maß hält weiter eher zu oft zurück.
    *Bedrohungen:* Ein Kampf wird schwerer als gemessen (anderer Begleiter bleibt); dann lernt er den höheren Wert, und
    Stufe 2 nimmt dem knappen Heiler das Zähnefletschen. *Chancen:* Dieselbe Größe sagt der Teamgröße (K7), was ein
    Einsatz je Kampf kostet.
  - *Gewählt:* (c). Knapp ist ein Endkampf-Heiler, dem nach einem Einsatz im anstehenden Kampf weniger als ein Einsatz
    im Endkampf bliebe. Am Lager, wo kein Kampf ansteht, gilt der Endkampf-Wert für beides.
  - *Falsifikation:* *Kein Defekt:* widerlegt an Pugil vor Kampf 1. *Option falsch:* Der Verlust im Endkampf ist nie
    gemessen, weil der Begleiter erst nach dem Kampf abtritt – der Einsatz schließt beim Rückzug nach dem Kampf und
    zählt für den zuletzt geführten Kampf. *Ausgeliefert, und nichts ändert sich:* Vor der ersten Messung eines Kampfs
    gilt der größte Wert des Bretts wie bisher.
  *Stufe 2, gebaut (A590): Zähnefletschen und Campsite nach Bedarf.* Endkampf-Heiler heißen die Begleiter, die in der
  Wahl für den Endkampf heilen oder schützen (Stufe 1; seit A662 neu gerechnet, sobald sich LP, Futter oder Hörner
  ändern, mit der Rechnung des Hornfensters, K14 „Der Plan des Endkampfs"); knapp ist
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
  - *Campsite und Agent:* Die Campsite-Wahl (K10) liest Fenster und Agent und überlässt dir die Wahl, wenn ihre LP sich
    unterscheiden („the window's HP … is not the team's"). In seinem Lauf vom 07.11.2026 stimmten beide: RSR ließ Drakon
    und Golem ruhen und erwartete 30 % je Begleiter, das Spiel bot 30 % (Spur 16:04:49 bis 16:05:04).
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
K10 (Campsite A564, Laden A569, Weghinweis A632). Brett 2 lief am 08.11.2026 (A683). Offen ist nur die Prüfung der
Bretter 3 bis 5 und der Meisterbretter an seinem ersten Lauf dort (TODO, „Warten auf Daten": „Beastmaster: Bretter 3
bis 5 und die Meisterbretter am ersten Mitschnitt prüfen").

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
     Linie hebt, sonst der größte; bei gleicher Heilung der Heiltrank vor dem Pulver. Dazu, auch über der Linie, die
     Heilung, die keiner mehr braucht (A664, „Über das Brett"); vor beidem gegen einen Boss das Bestien-Ausdauermittel,
     solange es nicht steht (Synergie, ebenda) – nur vor Heilung für dich, und nicht mehr, wenn der Endkampf vor dem
     nächsten GCD endet (A717): Es zahlt sich über den Rest des Kampfs aus, und am Ende hebt es das Maximum, zu dem
     die Heilung gerade auffüllen soll. Bis A717 trat es auch an die Stelle der Heilung für den Begleiter („Unversehrte
     Bestien", K15) und ging auf ihn – sein Maximum stieg, er war nicht voll, der Gegenstandsbonus trotzdem weg.
     Normale Heiltränke sperrt das Brett.
     Die LP liest RSR wie beim Heiltrank: jetzt und, da im Brett kein Heiler lebt, beim nächsten Webfenster einen GCD
     voraus (Konzept 03, „Die Schwelle nach der Vorhersage, wo kein Heiler lebt", A641).
  2. Im Kampf gegen Gift oder Blindheit auf dir das passende Mittel.
  3. Steht kein Reraise, die stärkere Bestien-Lebensfeder, im Kampf und außerhalb. Ihr Wirktext nennt keine Dauer, also
     gilt sie bis zu einem Fall. Geht Reraise, ohne dass du fielst, schreibt die Spur das, und von da an kommt eine
     Lebensfeder nur noch gegen einen Boss. Als gegangen zählt nur, was RSR im selben Lauf lückenlos gesehen hat
     (A714): RSR folgt dem Status in jedem Bild, an oder aus – „Turn off when you die" schaltet RSR mit dem Fall ab, und
     der Fall darf dabei nicht ungesehen bleiben –, und urteilt nicht über eine Unterbrechung hinweg: nicht beim
     Verlassen des Bretts (dort nimmt das Spiel jeden Status), nicht in einem neuen Lauf, nicht nach einer Pause der
     Beobachtung von mehr als einem GCD (Ladebild), nicht nach dem Endkampf. Am 11.11.2026 schrieb RSR „Reraise went
     without a fall", als er nach Lauf 1 das Brett neu betrat, und hielt danach jede Lebensfeder bis zu einem Boss – auf
     Brett 2 vom Aufheben bis zum Endkampf, durch beide Elitekämpfe.
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
     (A642) –, vor seinem ersten Treffer im Kampf der größte Einzeltreffer eines Begleiters auf diesem Brett (A658), vor
     jeder Messung unter „Use HP potions below this HP" als Anteil seiner LP, der Bestien-Heiltrank („to self or familiar"), der ihn mit dem kleinsten Wert über
     diese Marke hebt, sonst der größte. Das Bestien-Pulver nicht: Es heilt dich und die Verbündeten in deiner Nähe, und
     der Begleiter steht am Gegner. Nie der letzte Heilgegenstand: Er bleibt dir, denn deine LP sind der Lauf, seine ein
     Horn. Danach derselbe Schutz wie in 6 gegen einen angekündigten Treffer auf ihn, wenn er nicht mehr LP hält als
     er bis zum nächsten Webfenster verlieren kann; im Endkampf gegen jeden angekündigten Treffer auf ihn.
     *Warum der Verlust je Sekunde* (A642): Halten ihn vier Gegner mit je 100 alle 3 s, verliert er rund 130 LP je
     Sekunde; sein größter Treffer bleibt 100, und bei 400 LP fiele er im nächsten GCD, ohne dass der größte Treffer es
     anzeigt. Herausfordern liest dieselbe Rate schon (K9); bleibt es aus, weil du das Fenster nicht hieltest, ist der
     Trank der Rest. Eine Rechnung, nicht gemessen; auf Brett 1 fiel kein Begleiter.
     *Vor dem ersten Treffer im Kampf* (A658): die Marke ist der größte Einzeltreffer, den ein Begleiter auf diesem
     Brett nahm, solange er in diesem Kampf noch keinen nahm; erst ohne jede Messung auf dem Brett „Use HP potions below
     this HP" als Anteil seiner LP. Grund: Ein Begleiter, der mit wenig LP auf ein Horn kommt, fällt sonst am ersten
     Treffer, wenn seine LP über deiner Trankschwelle, aber unter dem Treffer liegen, den die Gegner dieses Bretts
     austeilen; nach dem ersten Treffer im Kampf gilt dessen Maß wie bisher.
     *Früher heilen?* (seine Frage vom 07.11.2026 zu Ziz 264/487 und Golem 439/550 auf den Hörnern: „warum werden die
     nicht vorab geheilt im kampf?"; A658): Nein, außer vor dem ersten Treffer wie eben. Seine LP tragen sich über das
     Brett, und ein Trank heilt früh dieselbe Menge wie spät – früh kostet er nur den Trank früher, und dreierlei
     spricht für spät: Eine Campsite heilt ihn umsonst (in seinem Lauf ruhte Golem auf Zug 7), ein Begleiter, den kein
     späterer Kampf braucht, braucht die LP nicht (Ziz und Golem standen nicht im Endkampfplan), und spät überheilt der
     Trank nicht. Früh spricht nur dafür, dass er den Treffer nicht übersteht, bevor der Trank kommt – das deckt die
     Marke. In seinem Kampf 4 bekam Golem bei 118 LP den Bestien-Heiltrank III (Spur 15:58:53); Ziz blieb darüber.
     *Verworfen:* (a) früh heilen unter einem festen Anteil (Daedalus: unter 35 % „worn") – eine Zahl ohne Herleitung,
     und vor einer Campsite verschenkt sie Tränke, die dir im Endkampf fehlen; (b) einen Heiler des Endkampfplans, dem
     die LP dafür fehlen, nach der letzten Campsite früh heilen – K11 hält ihn dann ohnehin von den Hörnern, und kommt
     er doch, heilt ihn die Marke in derselben Menge; (c) gesunde vor angeschlagenen auf die Hörner (A637, Variante b:
     kostet Schaden ohne Risiko, solange er den Einsatz übersteht); (d) außerhalb des Kampfs heilen – ob ein Trank einen
     nicht gerufenen Begleiter erreicht, ist unbelegt, und nötig ist es nach (b) nicht.
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
      solange er nicht steht, auf dich. Eines, das den magischen Schaden steigert („Increases magic damage dealt",
      Bestien-Zaubermittel, 15 s), geht, solange du einen Schadensgegenstand hältst, erst unmittelbar vor ihm: wenn einer
      jetzt ginge (13), kommt zuerst das Mittel, im nächsten Zug der Gegenstand („Synergien", A665). Außerhalb des
      Endkampfs bleiben sie liegen: Ihr Wert für einen späteren Kampf ist nicht bekannt (K11), im Endkampf ist er null.
  13. Schaden (zweite Einstellung) auf dein Gegnerziel, sobald kein weiterer Gegner mehr in seine Reichweite kommen kann
     („Zeitpunkt im Kampf", A665): im Endkampf jeder Schadensgegenstand, der wertvollste zuerst, spätestens wenn der
     Kampf enden könnte, bevor alle gehaltenen Gegenstände genommen sind; sonst der, der hier mindestens so viel bringt
     wie im besten anstehenden Kampf, sobald er mehr Gegner trifft als dein Ziel; einer mit Rückstoß nur, wenn er jeden
     trifft, der kämpft, im Endkampf auch dann spätestens vor dessen Ende („Über das Brett", A664). Wert ist Potenz mal
     der Summe über die getroffenen Gegner aus eins plus dem gemessenen Schwächezuschlag, wo das Element ihre Schwäche
     ist (K1), und der Zusatzpotenz eines Status, den das Ziel trägt (Reißzahn des Feuers unter Deep Freeze), je Gegner
     höchstens seine LP (A672: seine LP durch den gemessenen Schaden je Potenz der Gegenstände); bei Gleichstand der ohne
     Rückstoß. *Überschuss (A672):* Der Treffer eines Gegenstands kommt von dir unter der Aktion seines englischen Namens
     (Reißzahn des Windes, Aktion 47011; Reißzahn des Wassers 47008); RSR misst daran den Schaden je Potenz, wie K1 den der
     Aktionen, ohne Schwächezuschlag, je Gebiet. Am 07.11.2026 auf Brett 1: 1.140 bis 1.192 Schaden je 2.000 Potenz auf
     die Terrormilbe (0,57 bis 0,6), 1.304 bis 1.600 auf die Schattenmilbe mit 522 LP – zwei Drittel des Treffers dort
     verfielen. Im besten anstehenden Kampf zählt jede Art höchstens die größten LP, die RSR an ihr gesehen hat; ohne
     Messung zählt der Treffer voll wie zuvor. Grund: Drei Milben zählten wie drei volle Treffer – zwei Drittel des
     Treffers waren kein Schaden. *Wert in verhinderter Last (A676):* Wo RSR die Last der Kämpfe kennt, wiegt es einen
     Schadensgegenstand nicht nach Potenz, sondern nach der Last, die er dir erspart: die Last, die der Kampf noch bringt,
     mal dem Anteil der Gegner-LP, den er nimmt. Jetzt ist das die gelernte Last dieses Kampfes (K10, „Last je Kampf")
     abzüglich dessen, was er dich schon kostete, und der Anteil seines Schadens (Potenz mal gemessenem Schaden je Potenz,
     je Gegner höchstens seine LP) an den LP der Gegner im Kampf samt der Arten, die noch kommen (die größten LP, die RSR an
     ihnen sah); in einem anstehenden Kampf seine gelernte Last mal dem Anteil an den LP seiner Arten. Der Gegenstand geht
     jetzt, wenn er hier mindestens so viel Last erspart wie im besten anstehenden Kampf; unter mehreren, die gehen, der
     mit der größten. Fehlt eine Last, der Schaden je Potenz oder die LP einer Art, gilt der Vergleich in Potenz wie oben; ebenso, wenn
     der Kampf dich schon mehr kostete als je zuvor – was er noch bringt, ist dann unbekannt.
     *Grund:* Sicherheit vor Schaden: Potenz auf einen harmlosen Gegner erspart keine LP, und deine LP tragen von Kampf zu
     Kampf bis zur Rast. Die Annahme dahinter: Ein Kampf kostet dich im Verhältnis zu den Gegner-LP, die du noch abtragen
     musst – nimmt der Gegenstand ein Drittel davon, endet der Kampf um ein Drittel früher. *Im Kampf (Rechenbeispiel; welche
     Last zum Milbenkampf gehört und die LP des Endkampfs sind nicht gemessen):* Kostet der Milbenkampf 18 % und nimmt ein
     Reißzahn dort drei Schattenmilben (je 522 LP) und einen Teil der Terrormilbe – angenommen zwei Fünftel der Gegner-LP –,
     erspart er rund 7 % deiner LP; nimmt er im Endkampf (35 %) ein Fünfzigstel der Gegner-LP, unter 1 %. Er geht im
     Milbenkampf; nach Potenz wartete er auf den Endkampf. *Gegenthesen:* (1) Die gelernte Last ist die schwerste, nicht die übliche; ein
     anstehender Kampf wirkt so schwerer als jetzt. Gleich für alle Kämpfe; im eigenen Kampf zieht RSR das schon Erlittene
     ab. Preis: Ein Gegenstand wartet eher, als dass er zu früh geht – im Endkampf geht er ohnehin. (2) Die Last folgt nicht
     den Gegner-LP (ein harmloser Boss mit vielen LP, gefährliche kleine Zusatzgegner). Dann zählt ein Treffer auf die
     kleinen zu wenig; die Zielwahl (K3) bringt sie ohnehin zuerst. (3) Ausgeliefert, und nichts ändert sich: Ohne gelernte
     Last eines Kampfes (vor seinem ersten Ende mit RSR) bleibt die Potenz. Legt ein Gegenstand, der jetzt ebenfalls ginge, den Status, den der gewählte
     ausnutzt, auf das Ziel, geht er zuerst (Reißzahn des Eises vor dem des Feuers): Beide gehen nacheinander, und nur
     diese Reihenfolge bringt die Zusatzpotenz. Ein Gegenstand,
     dessen Element nur die Schwäche noch kommender Gegner trifft, wartet auf sie (wie K2).
- *Über das Brett (A664; seine Vorgaben vom 07.11.2026):* „nach dem letzten fight ist zuviel ungenutzt und verfällt.
  sollte besser über alle kämpfe eines bretts hinweg genutzt werden, so dass möglichst das zum letzten kampf am besten
  passende dort genutzt wird, das andere vorher in den kämpfen, wo es am ehesten passt"; „synergieeffekte der items prüfen
  und optimieren"; „sich immer auf die summe der anstehenden kämpfe ausrichten". Was nach dem Endkampf übrig ist, ist
  wertlos (oben). Es gilt:
  - *Heilung, die keiner mehr braucht (zu 1):* Liegt die Heilung, die du hältst, über dem, was die anstehenden Kämpfe dich
    kosten können – die Summe auf dem schwersten Weg bis einschließlich Endkampf, jeder Kampf mit seiner Last je Kampf
    (K10 Campsite), ein ungelernter so schwer wie dein schwerster gemessener, dazu was dir jetzt fehlt –, ist der Rest
    übrig. Davon geht im Kampf der größte Heilgegenstand, der dich nicht überheilt (dir fehlt mindestens, was er heilt),
    auch über deiner Trank-Linie. Im Endkampf ist alles übrig. Grund: Ein Trank heilt früh, was er spät heilt, solange er
    nicht überheilt; früh hält er dich höher gegen den nächsten großen Treffer, und nach dem Endkampf ist er nichts mehr.
  - *Schaden dorthin, wo er am meisten bringt (zu 13):* Jeder Schadensgegenstand hat in jedem Kampf einen Wert –
    Potenz mal der Summe über die getroffenen Gegner aus eins plus Schwächezuschlag. Im laufenden Kampf zählt, wen er jetzt
    trifft; in einem anstehenden Kampf (`CrucibleRoute.BattlesAhead`, der Endkampf eingeschlossen) jede Gegnerart, die
    das Blatt für ihn führt, einmal – mehr Gegner einer Art kennt das Blatt nicht, die Schätzung liegt eher zu tief, und
    der Gegenstand geht eher jetzt als später. Er geht jetzt, wenn er hier mindestens so viel bringt wie im besten
    anstehenden Kampf; im Endkampf jeder. Das ersetzt „einer je Kampf" – die Setzung stand nur, weil die Kämpfe davor
    unbekannt waren (K14 kennt sie). So bleibt für den Endkampf, was dort am meisten bringt (ein Element, das die Schwäche
    des Endgegners trifft), und ein Reißzahn für Rudel geht im Rudel.
  - *Zeitpunkt im Kampf (zu 13; A665; sein Hinweis vom 07.11.2026):* „bei rückstoßeffekt wird einer der gegner ins aus
    geschleudert, die anderen sind vom effekt zu weit entfernt. somit war der effekt wahrscheinlich zu früh." Sein
    Mitschnitt vom 07.11.2026 (Build vor A664) bestätigt ihn in allen drei Läufen: Der Reißzahn ging 0,06, 0,7 und 0,2 s
    nach dem Erscheinen der drei Schattenmilben, traf die Terrormilbe und die eine Milbe, die 5 bis 11 Yalm von ihr
    erschien; die beiden anderen erschienen 17 bis 25 Yalm entfernt (Reichweite 12 Yalm plus 1,8 Yalm Größe), standen 4
    bis 6 s und liefen dann in 2 bis 3 s zum Begleiter. Die getroffene Milbe flog 7 statt 15 Yalm – bis an den Rand der
    Arena, 22,5 Yalm von ihrer Mitte (meine Folgerung aus den Positionen) – und fiel an den 1.304 bis 1.600 Schaden (522
    LP), nicht am Rand. Bis A665 ging ein Schadensgegenstand, sobald er zwei Gegner traf. Es gilt: Ein
    Schadensgegenstand wartet, solange noch ein Gegner in seine Reichweite kommen kann – (a) ein Gegner außerhalb, der
    weder getroffen noch gewirkt hat (`EnemyDanger`, laufender Cast): Er ist unterwegs oder steht nach dem Erscheinen wie
    die Milben; wer von außerhalb trifft oder wirkt, kämpft von dort und kommt nicht; (b) solange er nur sein Ziel trifft,
    eine Gegnerart dieses Kampfes, die das Blatt führt und die noch nicht erschien (K2) – im Endkampf von Brett 1 kommen
    die drei Sukkubi 48 bis 50 s nach dem ersten Treffer, in allen drei Läufen. Kann keiner mehr kommen, entscheidet die Regel oben. Außerhalb des
    Endkampfs verliert Warten nichts: Endet der Kampf, bleibt der Gegenstand für die anstehenden. Im Endkampf geht er
    spätestens, wenn der Gegner mit den meisten LP – mit ihm endet der Kampf – nach seiner korrigierten Restzeit fällt,
    bevor die gehaltenen Schadens- und Stärkungsgegenstände je einen GCD bekommen haben. Grund: Ein Gegner, der in die
    Reichweite läuft, ist ein Treffer mehr ohne Preis; ein Gegenstand, der nach dem Endkampf übrig ist, ist verloren.
    Die unzielbaren Helfer des Endgegners (Brett 1: elf Kopien der Seelenlosen mit 44 LP) stehen in keiner Gegnerliste
    und halten nichts auf.
  - *Rückstoß (zu 13; sein Hinweis):* Ein Gegenstand mit Rückstoß geht nur, wenn er jeden Gegner trifft, der gerade kämpft
    – sonst wirft er die Überlebenden aus dem Kreis, in dem Begleiter und Flächen die übrigen treffen. Im Endkampf geht er
    spätestens vor dessen Ende (oben), auch wenn ein Gegner fern bleibt; bis A665 verfiel er dort. Bis A664 entschied der
    Rückstoß nur bei gleichem Wert.
  - *Wechselwirkungen (A666; alle 203 Wirktexte von `XBMItem` – Ausrüstung, Gegenstände, Futter –, xivapi 07.11.2026;
    consolegameswiki „Crucible Items" und „Crucible of the Unbroken", 07.11.2026, nennen dieselben Paare, keine weiteren;
    seine Vorgabe vom 07.11.2026, sie zu Beginn zu erheben):* Jeder Querverweis eines Textes auf einen anderen Gegenstand,
    ein Ausrüstungsstück oder einen Status, und wo RSR darüber entscheidet:
    - *Gebaut, beim Einsatz im Kampf:* (1) Stärkung des magischen Schadens trägt die Reißzähne, Sternensturm, die
      Gegenschläge der Rosenrüstung und die Zusatztreffer der Elementäxte (Flammende, Frostige Axt u. a.) („affected by magic damage increase effects");
      das Bestien-Zaubermittel („Increases magic damage dealt by 30%", 15 s) geht unmittelbar vor dem ersten
      Schadensgegenstand (12, A665) – 15 s ab Beginn des Endkampfs wären vorbei, bevor die Sukkubi kommen. (2) Reißzahn
      des Eises legt Deep Freeze (20 s), der des Feuers trifft darunter mit 1.000 Potenz mehr; der des Eises geht zuerst
      (13, A665). Deep Freeze aus dem Entfesseln der Chimäre zählt ebenso, RSR liest den Status am Ziel. (3) Sternensturm
      senkt die Feuerresistenz des Ziels um 20 % (60 s): Ginge der Reißzahn des Feuers, geht Sternensturm zuerst (13,
      A666); er gibt 50 TP je getroffenem Gegner. (4) Spirit of the Breathtaker (Bestien-Flinkheitsmittel, bis
      Kampfende) gibt dem Grimoire der Unantastbarkeit drei Blink mehr (acht statt fünf) und senkt die Verfallschance
      des Auto-potion der Bestien-Heiltranksammlung von 50 auf 10 %: Geht eines der beiden, geht das Flinkheitsmittel
      unmittelbar davor (A666), auch außerhalb des Endkampfs. RSR liest das aus dem Text: Nennt der Text eines
      Gegenstands einen Status, den ein gehaltenes Mittel dir gibt, geht das Mittel zuerst. (5) Die Seren heilen 50 %
      der LP ihres Ziels, solange du eine Rote Schleife trägst („When carrying a crimson ribbon, restores 50% of target's
      total HP"): RSR zählt sie dann als Heilgegenstand – in Schritt 1, im Überschuss über die anstehenden Kämpfe und in
      den Summen –, ihr Schutz kommt dazu (A666); im Laden und auf dem Schatzfeld zählen sie so, sobald die Schleife
      getragen oder gewählt ist, und die Schleife zählt die gehaltenen Seren mit (K13, A668). (6) Das Bestien-Ausdauermittel (+20 % maximale LP bis Kampfende) geht
      gegen einen Boss vor dem Heilgegenstand (11 vor 1, A664) und damit vor dem Blutsauger-Reißzahn, dessen Potenz mit
      den maximalen LP steigt (1.500 bis 3.300); außerhalb eines Bosskampfs nicht vorgezogen – es fehlte dem nächsten.
    - *Dauer durch Ausrüstung:* Bestien-Verhärtungsmittel mit Eisschild 90 statt 60 s, Bestien-Verkürzungsmittel mit
      Diebeshandschuhen 90 s, Aufbrausmittel mit Silberrandbinokel 60 statt 30 s, Blutsaugerserum mit Dämonischer
      Rüstung 90 s, Grimoire der Reflexion mit Verhextem Hut 60 s, Heiltranksammlung mit Diebesgewand 240 statt 150 s,
      Bestien-Feder mit Ring der Euphorie 250 statt 100 TP. Beim Einsatz liest keine Regel die Dauer – Schutz geht gegen
      angekündigte Treffer, Stärkung im Endkampf zu Beginn –, die Wahl im Kampf ändert sich nicht. Im Laden und auf dem
      Schatzfeld zählt die Skala den Zuwachs bei den Schutzgegenständen – Verhärtungsmittel mit Eisschild, Grimoire der
      Reflexion mit Verhextem Hut – und die drei Blink mehr, die das Flinkheitsmittel dem Grimoire der Unantastbarkeit gibt
      (K13, A668); im Endkampf von Brett 1 lag im besten 60-s-Fenster 83 % des Schadens, im besten 30-s-Fenster 55 %
      (07.11.2026, über alle Angriffsarten; den magischen Anteil trennt der Mitschnitt nicht). Die
      Heiltranksammlung mit Diebesgewand zählt sie ebenso (K13, A670); die übrigen Paare tragen nichts, was die Skala
      misst (Stärkung, TP, Lebensentzug).
    - *Auslöser an Ausrüstung und Futter:* bei kritischem Treffer (Bestienmesser, Donnerrüstung, Bananen-, Zitronen-,
      Tarantel-Simulakrum u. a.), beim Ausweichen (Ninja-Gewand, Kopfsalat-Simulakrum), beim Blocken (Windschild,
      Schützender Schild), bei erlittenem Schaden (Engelsrobe, Trugweste, Rosenrüstung u. a.), bei einer Tötung (Dämonische
      Rüstung; Stimulanztrunk- und Weihwasser-Simulakrum: High Wire bis vier Stapel, je +10 bis 15 % Schaden und −5 bis
      8 % erlittener Schaden, 60 bis 180 s), bei einer Kombo mit Begleiterfertigkeit (Stimulanzöl-Simulakrum). Die
      Stärkungen der kritischen Trefferquote und des Ausweichens tragen die ersten beiden; sie gehen im Endkampf zu
      Beginn und wirken so am längsten – keine Änderung. Tötungsstapel machen schwache Gegner wertvoll: Ein
      Flächengegenstand, der drei Schattenmilben tötet, gibt mit Dämonischer Rüstung drei Stapel High Wire (je −5 %
      erlittener Schaden, +10 % Schaden, 180 s, bis vier; Tonikum-Futter für den Begleiter je −8 %, 60 s), Magnum-Wasser
      dem Begleiter Regeneration. *Bewertet (A672):* Ein Stapel ist Schutz über die Zeit nach der Tötung; Wert (13) zählt
      Potenz, Zielwahl (K3) Gefahr je Sekunde bis zum Fall – keine der beiden misst ihn. Auf Brett 1 brächten drei Milben
      auf einmal −15 % für den Rest eines Kampfes, der 4 bis 18 % deiner LP kostete: höchstens rund 2 % je Kampf. *Was gilt
      (A677):* Trägst du Ausrüstung, die bei einer Tötung einen Stapel gibt, der erlittenen Schaden senkt („Grants one
      stack of … upon defeating an enemy", Wirkung „reduces damage taken by …%", „Maximum Stacks"), zählt ein
      Schadensgegenstand, der Gegner tötet, in ersparter Last (Schritt 13) dazu, was die Stapel dir im Rest des Kampfes
      ersparen: je getöteter Gegner ein Stapel bis zum Höchstwert, mal der Senkung, mal der Last, die nach dem Gegenstand
      bleibt; in einem anstehenden Kampf ebenso, ab null Stapeln. Ob die Tötung durch einen Gegenstand den Stapel gibt,
      sagt kein Text; RSR misst es selbst: Fällt ein Gegner binnen eines GCD nach dem Treffer eines Gegenstands und
      steigen deine Stapel binnen eines GCD danach, zählen sie ab dann; bleiben sie unter dem Höchstwert stehen, zählen sie
      nicht. Verglichen wird mit den Stapeln im Bild, bevor der Gegner als gefallen gesehen wurde (A719): Kommt der
      Stapel im selben Bild wie der Fall – in welcher Reihenfolge das Spiel beides meldet, sagt keine Quelle –, stand er
      sonst schon im Vergleichswert, und die Messung schloss für die ganze Sitzung auf „zählt nicht". Bis zur ersten
      Messung der Sitzung zählen sie nicht. Futter für den Begleiter (Tonikum) senkt seinen Schaden,
      nicht deine Last, und zählt hier nicht. *Gegenthese A677-1:* Eine andere Tötung im selben GCD – dein Schlag, dein
      Begleiter – hebt die Stapel, und die Messung schließt zu Unrecht auf „zählt". Preis: die Stapel eines Gegenstands zu
      Unrecht gezählt, höchstens die Senkung mal der Restlast (oben: auf Brett 1 rund 2 % je Kampf). *Ausgeliefert, und
      nichts ändert sich?* Ohne diese Ausrüstung, ohne gelernte Last (dann gilt die Potenz) und bis zur ersten Messung.
    - *Rast und Futter:* Ein ruhender Begleiter verliert sein Futter, mit Lilien-Simulakrum nicht; mit
      Lassi-Simulakrum füllt ihn die Rast ganz. Die Campsite wählt danach (K10, „Futter an der Rast", A669). Arzneibeutel:
      ein zufälliger Bestien-Trank je Rast, auf der Skala mit dem kleinsten je sicherer Campsite (K13, A669).
    - *Weg:* Feiglingsmesser (35 % Flucht von normalen Gegnerfeldern, danach Guerrilla Tactics), Bestien-Rauchbombe
      (Flucht) und Diebesmesser (zwei Gegenstände auf Schatzfeldern) zählen im Wegfenster und auf der Skala (Weg,
      „Flucht"; K13; A671).
    - *Zauberbücher (A678):* Grimoire der Zauberschmiede und des Stahlstachels machen 180 s jeden Angriff magisch oder
      physisch, auch den der Begleiter („Ensures all attacks deal magic damage", „… deliver physical damage"). Die
      Abwehrstufe wirkt (derselbe Auto-Angriff traf Stufe 2 mit 57, Stufe 5 mit 37), und unter Zauberschmiede trägt das
      Zaubermittel jeden Angriff. Was ein Buch bringt, belegt kein Text: ob der umgewandelte Treffer seinen Aspekt und
      damit die Schwäche behält, und wie er gegen die andere Abwehr wirkt; keine Quelle nennt es (Suche 07.11.2026:
      consolegameswiki, icy-veins, GluttonyCombo führt die Bücher nur als Angriffsgegenstand). *Was gilt:* Ein Buch geht
      wie eine Stärkung nur im Endkampf (12). Hat RSR auf diesem Brett gemessen, dass es den Schaden hebt, geht es zu
      Beginn, vor den Stärkungen – das Zaubermittel trägt dann jeden Angriff; ungemessen geht es, wenn der Endkampf endet
      (dieselbe Bedingung wie bei den Schadensgegenständen, 13): Danach verfiele es, und sein Einsatz misst; gemessen ohne
      Gewinn nie. *Messung:* Solange ein Buch wirkt, das RSR nahm, ist ein Treffer umgewandelt, wenn er in der Art des Buchs
      kommt und seine Aktion die andere hat. RSR teilt seinen Schaden je Potenz durch den derselben Aktion in ihrer
      eigenen Art auf derselben Gegnerart (K1, gleiche Abwehrstufe, gleiche Schwäche); das Mittel ist der Gewinn, je
      Brett. Ob ein umgewandelter Treffer ein Element trägt, nennt die Spur beim ersten. *Gegenthesen:* (1) Am Ende des
      Endkampfs wirkt es nur Sekunden: Die Messung braucht einen umgewandelten Treffer einer Aktion, deren eigene Art auf
      dieser Gegnerart gemessen ist; fehlt er, bleibt das Buch ungemessen, und der nächste Lauf misst wieder. (2) Der
      Gewinn hängt an den Gegnern: Er gilt je Brett, gemessen an dessen Endkampf, und geht im nächsten Lauf desselben
      Bretts zu Beginn. Preis: Wo ein Brett wiederholt läuft, zählt der Endkampf; ein Buch in früheren Kämpfen nimmt RSR
      nicht, wie keine Stärkung. (3) Ausgeliefert, und nichts ändert sich: Ohne Buch im Vorrat; ein Buch, das du selbst
      nimmst, misst RSR nicht.
    - *Ungezähmte Mittel (A673):* Sie geben mehr als ihr gezähmtes Gegenstück (Stärke +45 % physisch, Magie +50 %,
      Intensität +50 % kritisch, Ausweichen +25 % für 60 statt 30 s, Konstitution +30 % maximale LP) und legen dir einen
      Zustand auf: Betäubung 15 s, Nightmare 30 s („Troubled sleep is preventing the execution of actions", Status 5383),
      Versteinerung 15 s, Blind 30 s, Pollen 180 s (Schaden über Zeit, Status 1507). RSR liest sie wie das gezähmte Mittel
      – Stärkung, bei Konstitution maximale LP – und nimmt sie nur, solange getragene Ausrüstung dich gegen genau diesen
      Zustand feit („… Resistance +100%"): Umbral-Armband gegen Betäubung, Rote Schleife gegen Versteinerung und Blind,
      Silberrandbinokel gegen Blind. Gegen Nightmare und Pollen feit keine Ausrüstung (Schwarze Kapuze schützt vor Schlaf;
      ob das Nightmare deckt, ist unbelegt – RSR nimmt es nicht an); diese beiden nimmt RSR nie. Ein Serum zählt nicht als
      Schutz: Welchen Status es dir gibt und ob er steht, liest RSR nicht. Grund: K8 nimmt nie einen Gegenstand, der dir
      einen Zustand zufügt; mit der Resistenz kommt der Zustand nicht an, und das Mittel ist das stärkere ohne Preis.
      *Gegenthese A673-1 (+100 % nicht immun):* Hält die Resistenz den Zustand nicht immer ab, trifft er dich doch.
      Teilweise widerlegt: Der Laden zählt dieselbe Resistenz als Schutz gegen tödliche Zustände (Schritt 2), das Spiel
      nennt sie „+100%"; Preis: ein Zustand, falls doch nicht – eine Betäubung von 15 s im Endkampf.
    - *Punkte:* „Habitual Horder" (zehn Gegenstände am Ende, 1.000 Punkte) steht gegen „nichts verfällt"; seine Meldung
      gilt der Nutzung, Punkte nannte er nicht – RSR richtet sich nach der Nutzung.
    - *Gegenthese A666-1 (Flinkheitsmittel früh):* Geht es mit dem Grimoire in einem frühen Kampf, fehlt es im Endkampf
      (Bewegung +20 %, Ausweichen +5 %, Giftresistenz). Nicht widerlegt; drei Blink oder ein Auto-potion, das bleibt,
      wiegen in dem Kampf, in dem sie gebraucht werden, mehr als Bewegung im Endkampf – Preis benannt. *Gegenthese
      A666-2 (Serum als Heilung):* Geht ein Serum als Heilung, fehlt es gegen den Zustand eines späteren Kampfes.
      Teilweise widerlegt: Sein Schutz wirkt 180 bis 300 s ab dem Einsatz mit; es bleibt der Preis für einen Zustand in
      einem späteren Kampf. *Ausgeliefert, und nichts ändert sich?* Ohne die Paare im Vorrat; ohne gelesene Ausrüstung
      (vor der ersten Anzeige im Lauf) zählen die Seren nicht als Heilung.
  - *Gegenthese A664-1 (Heilung früh):* Ein früh genommener Trank fehlt, wenn ein Kampf schwerer wird als je zuvor. Nur
    der Überschuss über die schwerste Summe geht, und nur ohne Überheilen; Preis: der Überschuss, wenn ein Kampf alle
    früheren übertrifft. *Gegenthese A664-2 (Schätzung der Gegner):* Ein späterer Kampf mit vielen Gegnern einer Art wäre
    der bessere Ort. Nicht widerlegt; die Schätzung irrt zu früh statt zu spät, und zu spät heißt nach dem Endkampf
    verfallen – seine Meldung. *Gegenthese A664-3 (Stärkung früher):* Hält er mehr Mittel eines Status, als der Endkampf
    nacheinander nimmt, verfällt der Rest. Nicht widerlegt; die Spur nennt seit A665 zu Beginn und Ende jedes Kampfes,
    was er hält („Nachweis zur Laufzeit"). *Ausgeliefert, und nichts
    ändert sich?* Ohne Weg aus den Blättern (dann zählt kein anstehender Kampf, und der Gegenstand geht wie im Endkampf)
    und wo Schutz, Seren und Resistenzmittel an Anlässen hängen, die nicht kommen – die bleiben, wie bisher.
  - *Gegenthese A665-1 (Warten kostet den Endkampf):* Der Gegenstand wartet auf die Sukkubi, und der Endgegner fällt
    vorher. Widerlegt: Er geht spätestens, wenn der Endgegner nach seiner korrigierten Restzeit fällt, bevor jeder
    gehaltene Gegenstand einen GCD bekam; ist die Restzeit nicht bekannt (unverwundbar, eben erst getroffen), wartet er
    weiter – Preis: ein Endgegner, der ohne messbare Restzeit fällt, lässt ihn liegen, Wahrscheinlichkeit gering, weil
    die Restzeit nach 2 s Schaden steht. *Gegenthese A665-2 (Gegner, der nie kommt und nie handelt):* Ein Gegner außerhalb,
    der weder trifft noch wirkt (eingeschläfert, gelähmt, ein Objekt), hält den Gegenstand auf. Nicht widerlegt; außerhalb
    des Endkampfs bleibt er für die anstehenden, im Endkampf geht er vor dessen Ende – Preis: die Treffer auf Gegner, die
    in der Wartezeit fallen. *Gegenthese A665-3 (Überschuss):* Ein Treffer auf einen Gegner mit weniger LP als der Treffer
    zählt voll – die Milben (522 LP) nahmen 1.304 bis 1.600. Seit A672 widerlegt, sobald ein Gegenstand gemessen ist:
    je Gegner höchstens seine LP (13, „Überschuss"). *Ausgeliefert, und nichts ändert sich?* Wenn die kommenden Gegner beim
    Erscheinen schon getroffen oder gewirkt haben, oder das Blatt die späte Art nicht führt – dann geht der Gegenstand
    wie bisher.
- *Wie:* `InstanceContentCrucibleOfTheUnbroken.UseItem` (FFXIVClientStructs, in Dalamuds Fassung vorhanden) wirkt auf
  das Soft- oder Hard-Ziel. RSR setzt das Soft-Ziel kurz auf dich, deinen Begleiter oder dein Gegnerziel und stellt es
  zurück, wie BeastHelper (be40a1e, Werkzeugquelle, nur gelesen). Die Status liest RSR nach ihrem englischen Namen. Die
  Reichweite eines Wurfs nennt kein Blatt; das Gegnerziel ist das, das RSR ohnehin angreift.
- *Nachweis zur Laufzeit:* Nach dem Einsatz wartet RSR, bis der Platz einen anderen Gegenstand zeigt, höchstens einen GCD.
  Bleibt er gleich, schreibt die Spur „not taken" und versucht diesen Platz in diesem Kampf nicht mehr. Ob ein Gegenstand
  gerade gesperrt ist, prüft das Spiel über eine Funktion, die FFXIVClientStructs nicht führt; RSR prüft stattdessen die
  Animationssperre und den Verbrauch. Zu Beginn und am Ende jedes Kampfes nennt die Spur, was du hältst („crucible items
  at the start of fight …", „… after it"); wartet ein Schadensgegenstand auf kommende Gegner, nennt sie es einmal je
  Kampf. Was nach dem Endkampf übrig ist, steht damit in der Spur (A665; seine Meldung, zu viel verfalle).
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
  - Ein Schadensgegenstand je Kampf vor dem Endkampf war eine Setzung (Zahl der Kämpfe unbekannt); seit A664 entscheidet
    der Wert hier gegen den besten anstehenden Kampf („Über das Brett").
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
  Futter zählt (A662): Schaden mal (1 + „Damage Dealt" seines Futters), und er übersteht den Einsatz, wenn seine LP über
  dem größten Verlust je Einsatz mal (1 − „Damage Taken" seines Futters) liegen; jedes Futter multipliziert wie bei
  Ausrüstung (meine Ableitung). Der Plan des Endkampfs ist dieselbe Wahl (K14).
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
- *Elemente eines Begleiters* (A718): Welche Schwächen Trick, Release und Auto-Angriff eines Begleiters treffen, liest
  RSR aus dem Aktionsblatt und, sobald ein Treffer der Aktion gesehen wurde, aus dem Treffer (A582). K2 und die
  Teamempfehlung lesen das je Begleiter zusammengefasst und neu, sobald ein Treffer die Elemente einer Aktion anders
  gelehrt hat; bis A718 blieb die erste Zusammenfassung der Sitzung stehen. Für die Begleiter nennt das Blatt Aspekt
  und Angriffsart (alle 50 Auto-Angriffe, ffxiv-datamining 52a72b3), der Fall ist also selten.
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
