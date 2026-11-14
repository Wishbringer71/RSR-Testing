# 22 · Masked Carnivale

**Leitfrage:** Mit welchem Set und welcher Wahl der Zauber geht der Blaumagier eine Stufe des Masked Carnivale an?

Nur der Blaumagier spielt das Masked Carnivale. Wertregel, Mondflöte, Synergien und der Verbesserer des Sets, auf denen
dieses Konzept aufbaut, stehen in `01-jobs.md`, Abschnitt „BLU". Ordnung der Konzepte: `20-concept-order.md`.

- *Masked Carnivale* (seine Frage, 03.11.2026: ob ein Set vor dem Betreten so aus bekannten Zaubern optimiert werden
  kann, dass es die besten Gewinnchancen hat). Ja, aus Daten, die der Client schon liefert:
  - *Die Stufe:* Die Einweisung (`AgentAozContentBriefing`, FFXIVClientStructs b69073a) führt die gewählte Stufe
    (`SelectedContentIndex`), ihre Gegner je Akt (`AozArrangementData`) und die Wochenaufgaben mit Bedingungen. Das
    Blatt `AOZContent` nennt je Stufe die drei Akte, ihren Kampftyp sowie Ideal- und Standardzeit.
  - *Die Gegner:* RSR liest das Blatt `AOZContentBriefingBNpc` schon (`MaskedCarnivaleHelper`, seit Upstreams Umbau
    a09dc9257). Es enthält je Gegner Schwäche oder Immunität gegen sechs Elemente und vier Angriffsarten, dazu zehn
    Felder zu Slow, Petrifikation, Lähmung, Unterbrechung, Blind, Stun, Schlaf, Bind, Heavy und festem oder tödlichem
    Schaden. Diese Felder heißen „…Vuln“, bedeuten aber Immunität (unten, „Die Kontrollfelder bedeuten Immunität“, A762).
    Die Einweisung im Spiel zeigt dieselben Daten.
  - *Die Wahl:* je Akt die gelernten Zauber nach Wert gegen diese Gegner. Schwäche zählt hoch, Immunität fällt weg,
    jeder Gegner braucht einen Zauber, der ihn trifft. Dazu, was die Führer für die Mechanik der Stufe nennen:
    Unterbrechung mit Spucksardine, Stimme des Widders mit Ultravibration („Vibe Check“), Heilung, Abwehr. Warum keine
    Kette aus der Synergietabelle dazukommt, steht unten („Kontrollketten im Satz“).
  - *Wann:* Das Set wird geschrieben, solange die Einweisung offen ist und eine Stufe gewählt ist, vor dem Betreten,
    außerhalb des Kampfes. Ob das Spiel das Set in der Stufe selbst ändern lässt, ist offen; der Entwurf braucht es
    nicht.
  - *Die Gegner einer Stufe stehen in den Blättern* (A627): `AOZContent` (die Zeile, deren Order die Stufe ist) nennt je
    Akt Kampftyp und Zeile – Kampftyp 1 eine Gruppe in `AOZArrangement` (Unterzeilen mit `AOZContentBriefingBNpc`), 2 und
    3 einen Boss in `AOZBoss` (Werkzeugquelle BlueMageSetBuilder 213d3d9, dort gegen die Stufennamen geprüft). Die Wahl
    nach Widerständen hängt damit nicht an den Ids der Einweisung (`AozArrangementData.Enemies`), deren Bedeutung
    FFXIVClientStructs nicht nennt; die Messzeile „carnivale briefing" (A511) bleibt als Gegenprobe und nennt seit A647
    neben den Ids der Einweisung die Gegner, die RSR aus den Blättern liest. Gebaut mit A647 (unten, „Schwäche und
    Immunität der Stufe").
  - *Und seine Entscheidung:* Für eine Stufe wählt er sein Set selbst; seit A510 schreibt der Verbesserer nach einer
    eigenen Änderung nicht mehr hinein. Ein Satz für eine Stufe geht deshalb nur auf seinen Klick ins Set (unten).
  - *Grenze:* Die Blätter beschreiben Widerstände, nicht die Mechaniken einer Stufe (etwa Reflexion oder ein Gegner,
    der zur Mitte zieht). Diese tragen die Stufenführer von Blue Academy (unten).
- *Satz je Stufe aus den Stufenführern* (A627): Solange die Einweisung des Masked Carnivale offen ist und ein Blaumagier
  spielt, zeigt das Fenster „RSR - Masked Carnivale set" für die gewählte Stufe die Zauber, die der Stufenführer von
  Blue Academy (mage.blue) nötig oder empfohlen nennt, und schreibt sie auf „Apply" ins aktive Set; ohne den Klick
  ändert sich nichts.
  - *Die Daten:* `carnivale_sets.py` liest die Führer (RoarkGit/Blue-Academy, Abschnitte „Required Spells" und
    „Recommended Spells") und die Zaubernummern (RoarkGit/Limited-Job-Data) in `CarnivaleSets.g.cs`: je Stufe die Punkte
    der Listen, je Punkt die genannten Zauber als Nummern in der Reihenfolge des Texts, ob er nötig oder empfohlen ist,
    ob er nur Beispiele nennt („such as") und ob er einen Füllzauber meint („filler"). Nur diese Tatsachen, kein Text:
    Blue Academy nennt keine Lizenz. Zwei CI-Schritte halten das Lesen mit einem Selbsttest und jede Nummer gegen das
    Blatt `AozActionTransient`.
  - *Die Zauber im Spiel:* Nummer → `AozActionTransient` (Spalte Number) → `AozAction.Action`; gelernt heißt, wie beim
    Verbesserer, freigeschaltet (`IsQuestUnlocked`). Je Punkt gilt der erste gelernte der genannten Zauber; bei einem
    Füllzauber-Beispiel, das nicht gelernt ist, ein gelernter Zauber desselben Elements (`Action.Aspect`) und derselben
    Abklinggruppe mit höchstens dessen Abklingzeit. Was keiner deckt, zeigt das Fenster als fehlend mit den Namen.
  - *Die Stufe:* der Eintrag, den die Einweisung gewählt hat (`AozContentData.SelectedContentIndex`, als Stufe Index + 1
    gelesen – ein Schluss), mit dem Namen der Stufe aus `ContentFinderCondition` (Inhaltstyp Masked Carnivale, nach
    Inhalt geordnet) in der Sprache des Clients, damit ein Fehlgriff neben dem Namen der Einweisung sichtbar ist; zwei
    Knöpfe wechseln die Stufe.
  - *Schreiben:* auf „Apply", außerhalb des Kampfes, nicht zwischen Gebieten, ein Platz je Bild über
    `AssignBlueMageActionToSlot` mit Rücklesen, wie der Verbesserer. Nötige Zauber zuerst, dann empfohlene; was schon im
    Set steht, bleibt, wo es steht. Erst freie Plätze, dann Plätze, deren Zauber kein Punkt der Stufe nennt, vom
    letzten Platz an rückwärts (die vorderen Plätze trägt meist sein eigenes Gerüst). Lehnt das Spiel einen Platz ab,
    hält das Schreiben mit einer Meldung im Fenster an. Der Verbesserer sieht die Änderung als die des Spielers und
    schreibt in dieser Sitzung nichts mehr hinein (A510) – das Set für die Stufe bleibt stehen.
  - *Im Kampf:* Er öffnet die Einweisung für Stufe 26, das Fenster nennt Exuviation und Eerie Soundwave als nötig,
    Plaincracker als empfohlen; zwei stehen im Set, Eerie Soundwave fehlt, „Apply" setzt ihn auf den letzten Platz mit
    einem Füllzauber, den die Stufe nicht nennt. Ohne ihn bleibt der Mirrorknight in Akt 1 unter Alternate Plumage
    „functionally invulnerable to damage" (Führer zu Stufe 26), und die Stufe ist nicht zu gewinnen.
  - *Gegenthese 1 – Der Führer veraltet:* Blue Academy datiert jeden Führer mit Patch; der Generator liest den Stand
    seines Laufs, und die CI prüft nur die Nummern. Preis benannt: Ein neuer Führer kommt mit dem nächsten Lauf.
  - *Gegenthese 2 – Die falsche Stufe:* Liest RSR den Index falsch, steht der Name der falschen Stufe im Fenster neben
    dem der Einweisung; „Apply" schreibt nur auf seinen Klick.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Er öffnet die Einweisung nicht als Blaumagier oder klickt nie.
    Dann bleibt alles wie bisher.
- *Schwäche und Immunität der Stufe* (A647): Im Masked Carnivale zählt RSR, wogegen ein Gegner schwach oder immun ist –
  im Kampf bei der Wahl des Füllzaubers, vor dem Kampf im Satz der Stufe.
  - *Die Tatsache:* Das Blatt `AOZContentBriefingBNpc` führt je Gegner Schwäche (20) oder Immunität (21) gegen die sechs
    Elemente und die Angriffsarten Hieb, Stich, Schlag und Magie; die Einweisung im Spiel zeigt dieselben Daten. Eine
    Schwäche verdoppelt den Schaden dieses Elements (Blue Academy, „Basics", Community-Quelle; die Blätter nennen keine
    Zahl). RSR misst den Faktor selbst (unten) und rechnet mit 2, bis er gemessen ist.
  - *Im Kampf vorher:* Die Füllreihenfolge (D5) verglich jeden Füllzauber mit Schallwelle nach Potenz je Zeit, ohne das
    Ziel. Stufe 3, Zipacna (schwach gegen Wasser, immun gegen Erde): Wasserkanone (200, Wasser) verlor gegen Schallwelle
    (210, Wind), dazu verlangte die Regel auf einem Ziel keine längere Wirkzeit (2,0 gegen 1,0 s, `Action.Cast100ms`) – RSR
    wirkte Schallwelle für 210 je GCD, wo Wasserkanone 400 traf. Stufe 2, Akt 1, Arena-Marshmallow (schwach gegen Eis,
    immun gegen Wind): Schallwelle ging nicht (die Immunitätssperre), jeder Füllzauber verlor trotzdem den Vergleich gegen
    ihre 210, und als Rückfall kam Wasserkanone mit einfachem Schaden statt eines Eiszaubers mit doppeltem. Die
    Immunitätssperre („Don't use aspected actions on enemies immune to that aspect", ab Werk an) las nur das Element: Gegen
    die Schleime der Stufe 2 (immun gegen Hieb, Stich und Schlag) und Guimauve (Stufe 9, ebenso) blieb Kobold-Faust
    (Schlag) als letzter Rückfall erlaubt.
  - *Im Kampf nachher:* Ein Füllzauber zählt mit seiner Potenz mal dem Faktor des Ziels für ihn – 0 bei Immunität gegen
    sein Element oder seine Angriffsart, der gemessene Schwächefaktor bei Schwäche, sonst 1 –, Schallwelle ebenso.
    Steht das Ziel einem Zauber schwächer gegenüber als Schallwelle (Schwäche gegen ihn, nicht gegen Wind), entfällt die
    Bedingung der gleichen Wirkzeit: Ein doppelter Treffer wiegt den kleineren Webraum auf (Wasserkanone 400 in 2,5 s
    gegen 210; der Preis ist eine Fähigkeit, die einen GCD später gewoben wird). Gegen Zipacna geht Wasserkanone, gegen
    den Marshmallow ein gelernter Eiszauber im Set. Die Immunitätssperre liest jetzt auch die Angriffsart; ihr Text
    („immune to that aspect") meint den Zweck, keine Wirkung auf einen immunen Gegner zu verschwenden.
  - *Messung:* `OwnHitWatch` trennt im Masked Carnivale die Treffer auf ein Ziel, das gegen den Zauber schwach ist, vom
    Schaden je Potenz und führt sie gesondert; der Schwächefaktor ist ihr Schaden je Potenz geteilt durch den der übrigen,
    je Stufe und Gebiet wie dieser. Der Schaden je Potenz selbst bleibt so der einfache, den 1000 Nadeln und der
    Raketenwerfer gegen den Füllzauber halten; gegen ein schwaches Ziel zählt der Füllschaden mit dem Faktor von
    Schallwelle auf ihm.
  - *Vor dem Kampf, der Satz der Stufe:* Die Gegner je Akt liest RSR aus den Blättern (oben). Ein Element, gegen das
    ein Gegner der Stufe schwach ist und das kein Füllpunkt des Führers nennt, bekommt einen empfohlenen Punkt: der gelernte
    Füllzauber dieses Elements (ohne eigene Abklingzeit, mit Potenz im Wirktext) mit der meisten Potenz je Zeit, mit dem
    Gegner und dem Akt als Grund. Ist ein Gegner der Stufe gegen den Zauber eines Punkts immun, nennt das Fenster ihn
    dabei; der Punkt bleibt, weil der Führer Gründe kennt, die das Blatt nicht trägt (Stufe 28: der Feuerzauber gilt den
    Zombies, die erst im Kampf erscheinen, Durinn ist laut Blatt immun gegen Feuer). Erhebung am Blatt (07.11.2026): neue
    Punkte bekommen Stufe 7 (Wind, Feuer, Blitz), 8 (Wind, Feuer), 11 (Wind), 12 (Feuer neben dem Wasser des Führers),
    17 (Blitz) und 21 (Feuer); Stufe 2, 3, 4, 6, 20, 26 und 29 nennen alle Schwächen ihrer Gegner schon. Wind ist
    Schallwelle und steht meist schon im Set.
  - *Gegenthese 1 (kein Defekt, Schallwelle ist der Füllzauber):* Bei gleichem Faktor bleibt sie es; gegen einen
    schwachen Gegner nennen alle Führer den Zauber seiner Schwäche (Stufe 2: „use a spell that corresponds to each slime's
    weakness"). Widerlegt.
  - *Gegenthese 2 (Option falsch, „Only use actions of an aspect the enemy is weak to" reicht):* Die Einstellung (ab Werk
    aus) sperrt gegen einen Gegner mit Widerständen jeden Zauber, der nicht seine Schwäche trifft – ohne Zauber dieser
    Schwäche im Set wirkt RSR dann gar nichts Elementares. Der Faktor wählt den besten vorhandenen und bleibt, wo nichts
    trifft, bei Schallwelle. Die Einstellung bleibt für den, der es so will.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Ohne einen Zauber der Schwäche im Set ändert sich im Kampf
    nichts; dafür schlägt der Satz der Stufe ihn vor, auf seinen Klick. Ohne Einweisung und ohne Klick bleibt sein Set.
    Ist der Faktor im Spiel nicht 2, misst RSR ihn nach dem ersten schwachen Treffer neben einem einfachen.
- *Die Kontrollfelder bedeuten Immunität* (A762): Die zehn Felder `SlowVuln` bis `FlatOrDeathVuln` des Blatts
  `AOZContentBriefingBNpc` sind gesetzt, wo der Gegner gegen die Wirkung **immun** ist. Die Namen stammen aus EXDSchema
  (Werkzeugquelle, a1e24a0 vom 17.10.2026) und sagen das Gegenteil; Upstream (a09dc9257) und BlueMageSetBuilder
  (213d3d9) lesen sie wörtlich.
  - *Belege:* Die Stufenführer von Blue Academy (0f5aaf1) nennen 18 Gegner, deren Zauber Spucksardine unterbricht.
    Bei 17 ist das Feld Unterbrechung leer, beim einzigen gesetzten Feld unter ihnen, the Forefather (Stufe 22), schreibt
    der Führer „Burst cannot be interrupted“. Ausnahme ist Carmilla (Stufe 13): Feld gesetzt, der Führer unterbricht
    Beguiling Mist. Auric's Guide (astralflamesblog, Community-Quelle) nennt für 19 Gegner, ob Todeszauber, Heavy oder
    Schlaf wirken; alle 19 passen zur Deutung als Immunität, keiner zur wörtlichen. Beispiele: Die Schildkröten der
    Stufe 5 sind „invulnerable to everything … except Death spells“ (alle Kontrollfelder gesetzt, Tod leer). Die
    Zyklopen der Stufe 16 sind „immune to Death/Flat Damage“, aber nicht gegen Heavy oder Schlaf (Tod gesetzt, Heavy und
    Schlaf leer). Azulmagia, Typhon und Siegfried haben alle zehn Felder gesetzt; wörtlich gelesen wären sie gegen
    Schlaf, Stun und Tod verwundbar.
  - *Im Kampf vorher:* Die zehn Sperren „Don't use … actions on immune enemies“ (ab Werk aus) sperrten, eingeschaltet,
    genau die anfälligen Gegner und ließen die immunen frei. Mit der Unterbrechungssperre unterbrach RSR Goldors Blizzard
    III (Stufe 32), Silence des Scribe (Stufe 24) und Fluid Swing des Shikigami (Stufe 29) nicht. Die Führer nennen
    diese Unterbrechungen nötig. Gegen the Forefather hätte RSR die Sardine dagegen geworfen. Dazu sperrte jede Sperre
    auch Gegner, die das Blatt nicht führt, etwa den Shabti der Stufe 15, den der Führer mit Schlaf ausschaltet.
  - *Im Kampf nachher:* Eine Sperre greift nur bei einem Gegner, den das Blatt als immun führt. `IsVulnerableTo…`
    meldet einen bekannten Gegner ohne gesetztes Feld. Die Unterbrechungssperre greift nicht, solange das Spiel den
    laufenden Zauber des Ziels als unterbrechbar meldet: Das Spiel liefert die Tatsache für genau diesen Zauber und hat
    Vorrang (Carmilla).
  - *Gegenthese 1 (kein Defekt, die Namen stimmen):* Dann wären Typhon, Siegfried und Azulmagia gegen alles anfällig,
    die Schildkröten nur gegen Tod nicht, und der Führer könnte the Forefather unterbrechen. Widerlegt an 36 von 37
    Aussagen zweier unabhängiger Quellen.
  - *Gegenthese 2 (Option falsch, die Sperren entfernen):* Gegen einen immunen Gegner verschwendet ein Kontrollzauber
    einen GCD. Die Sperren tun nach der Behebung, was ihr Text sagt; sie bleiben, ab Werk aus.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Mit allen Sperren aus wie ab Werk ändert sich im Kampf nichts;
    die Behebung wirkt bei ihm, sobald er eine einschaltet, und im Satz der Stufe (unten).
- *Zauber für die Mechanik aus dem Fließtext der Führer* (A762): Der Generator las nur die Listen „Required Spells“ und
  „Recommended Spells“. Für die Stufen 1, 8, 10, 13, 15, 17, 18 und 21 haben die Führer keine Liste; RSR schlug dort nur
  Schwächezauber vor. Was sie nennen, steht im Text der Akte.
  - *Gelesen werden* zwei Tatsachen, je als empfohlener Punkt, wenn keine Liste des Führers den Zauber schon nennt:
    - Ein Satz in einem Akt, der einen Zauber mit Spucksardine unterbrechen lässt, ohne „cannot be interrupted“ (Stufe 22
      verneint es), ergibt den Punkt Spucksardine.
    - Ein Satz, nach dem Gegner der Stufe „Vibe Checked“ werden können, ergibt die Punkte Stimme des Widders und
      Ultravibration. Die Grundlagenseite von Blue Academy nennt den Vibe Check als genau diese zwei Zauber. Stufe 6
      zählt mit („cannot be Vibe Checked, but the other enemies can“).
    - Nicht gelesen werden Abschnitte zu Errungenschaften (Schlussstich, den er nicht nutzt; Konzept 01) und alle übrigen
      Nennungen im Text. Diese sind Wahlmöglichkeiten („can be helpful“), Verneinungen („if you … have Mighty Guard on“)
      oder Aktionen außerhalb des Sets (Schlaf, Sprint). Erhoben am Stand 0f5aaf1: Die übrigen Nennungen der Stufen 16,
      29, 31 und 32 sind von dieser Art.
  - *Im Kampf:* Stufe 1, zwei Schleime und ein Dullahan, kein Punkt bisher. Jetzt schlägt das Fenster Stimme des Widders
    und Ultravibration vor. BLU_Reborn wirkt Ultravibration nach dem Widder auf gefrorene Gegner, und der Akt endet nach
    zwei GCDs statt nach dem Abtragen dreier Gegner. Stufe 10, Crom Dubh: Spucksardine kommt ins Set. BLU_Reborn
    unterbricht damit King's Will (`TargetType.Interrupt`); unbrochen wird der Gegner größer, teilt mehr aus und nimmt
    weniger.
  - *Gegenthese 1 (kein Defekt, sein Set hat die Zauber ohnehin):* Das Grundset der Grundlagenseite enthält Widder und
    Ultravibration, nicht aber Spucksardine. Ob sein Set sie hat, weiß RSR erst im Spiel; der Punkt zeigt es als „im
    Set“, statt etwas zu schreiben.
  - *Gegenthese 2 (Option falsch, den ganzen Text lesen):* Die Erhebung oben zeigt Verneinungen und Wahlmöglichkeiten,
    die als Punkt falsch wären. Zwei Satzarten mit klarer Bedeutung tragen; eine neue Satzart in einem neuen Führer
    bleibt ungelesen, bis der Generator sie kennt.
  - *Gegenthese 3 (ausgeliefert, und nichts ändert sich):* Ohne Klick auf „Apply“ bleibt das Set. Wirkt der Vibe Check
    gegen einen Gegner nicht, etwa Kreios ohne Gefrieren, kostet er zwei GCDs. Die Führer nennen ihn nur, wo er wirkt.
- *Jeder Gegner braucht einen Zauber, der ihn trifft* (A762): Ist ein Gegner der Stufe gegen jeden schädigenden
  Zauber im Vorschlag und im Set immun, bekommt er einen empfohlenen Punkt. Das ist der gelernte Füllzauber mit der
  meisten Potenz je Zeit gegen ihn: Faktor 0 bei Immunität gegen Element oder Angriffsart, sonst der Schwächefaktor.
  Es ist dieselbe Regel wie die Schwächepunkte (oben), für Immunität statt Schwäche.
  - *Im Kampf:* Stufe 24, Akt 1: Der Magus ist gegen Magie immun, der Viking gegen Hieb, Stich und Schlag. Der Führer
    nennt keinen Füllzauber. Ein Set nur mit Magie kann den Magus nicht verwunden, und der Akt endet erst mit der
    Zeitgrenze, verloren. Jetzt schlägt das Fenster den besten gelernten physischen Zauber vor. Die Grundlagenseite
    empfiehlt dafür für jede Stufe einen magischen und einen physischen Füllzauber.
  - *Gegenthese:* Fast jedes Set trägt beides. Dann zeigt das Fenster nichts Neues, und es kostet nichts.
- *Kontrollketten im Satz – nicht gebaut* (A762): Ketten aus der Synergietabelle (Rösselsprünge gegen Bind und Slow,
  Rakete gegen Tod/fest) kommen nicht in den Satz. Nach der korrigierten Deutung ist kein Boss und kein Einzelgegner
  einer Stufe zugleich für Bind und Slow anfällig (der Kreislauf braucht beide), und keiner für Tod oder festen Schaden
  (Übersicht unten). Anfällig sind nur Begleitgegner, die schneller fallen, als eine Kette sich lohnt. Wo ein Boss
  für eine Wirkung anfällig ist (Kreios: Slow, Petrifikation), wählt die Rotation unter den Zaubern im Set nach Wert
  (Konzept 01, „Wert statt Reihenfolge“). Die Stufe gewinnt man mit der Mechanik, nicht mit 190 Potenz je GCD mehr.
- *Übersicht der Stufen* (A762, Blätter des Clients über xivapi v2 und Blue Academy 0f5aaf1, erhoben 13.11.2026):
  Gegner je Akt; Schwäche (+) und Immunität (−) gegen Element und Angriffsart; „anfällig für“ heißt, das Kontrollfeld
  ist leer. „Führer“ zählt die Punkte der Listen. „neu“ nennt die Punkte aus dem Fließtext. Schwäche- und
  Abdeckungspunkte hängen am gelernten Repertoire und am Set und entstehen erst im Spiel.

| Stufe | Akt: Gegner | schwach · immun | anfällig für | Führer | neu (A762) |
|---|---|---|---|---|---|
| 1 | 1: arena slime ×2<br>1: arena dullahan |  −Schlag<br> −Hieb | Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Stun, Slow, Petr., Lähmung, Blind, Tod/fest | 0 nötig, 0 empf. | Stimme des Widders, Ultravibration |
| 2 | 1: arena pudding<br>1: arena marshmallow<br>1: arena bavarois<br>2: arena gelato<br>2: arena flan<br>2: arena licorice | +Wind −Feuer/Hieb/Stich/Schlag<br>+Eis −Wind/Hieb/Stich/Schlag<br>+Erde −Blitz/Hieb/Stich/Schlag<br>+Feuer −Eis/Hieb/Stich/Schlag<br>+Blitz −Wasser/Hieb/Stich/Schlag<br>+Wasser −Erde/Hieb/Stich/Schlag | Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Unterbr., Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Unterbr., Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Unterbr., Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Unterbr., Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Unterbr., Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Unterbr., Blind, Tod/fest | 0 nötig, 6 empf. | Spucksardine, Stimme des Widders, Ultravibration |
| 3 | 1: Zipacna | +Wasser −Erde | Slow, Lähmung, Unterbr., Blind | 0 nötig, 1 empf. | Spucksardine |
| 4 | 1: arena bat ×2<br>1: arena wolf<br>2: Kreios | +Blitz<br>+Feuer<br>+Blitz | Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Slow, Petr., Lähmung, Unterbr., Blind | 0 nötig, 2 empf. | Stimme des Widders, Ultravibration |
| 5 | 1: brotherly gil turtle ×3 | – | Petr., Tod/fest | 1 nötig, 0 empf. | Stimme des Widders, Ultravibration |
| 6 | 1: arena catoblepas ×2<br>1: arena mandragora ×2<br>2: arena catoblepas<br>2: arena mandragora<br>2: arena eye ×2 | –<br>–<br>–<br>–<br>+Blitz | Bind, Heavy, Slow<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Bind, Heavy, Slow<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest | 0 nötig, 1 empf. | Stimme des Widders, Ultravibration |
| 7 | 1: lava slime<br>1: ice sprite ×2<br>2: ice sprite ×4<br>2: lava slime ×4<br>3: gladiatorial node ×2<br>3: lava slime ×6 | +Wind −Feuer/Schlag<br>+Feuer −Eis<br>+Feuer −Eis<br>+Wind −Feuer/Schlag<br>+Blitz<br>+Wind −Feuer/Schlag | nichts<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>nichts<br>nichts<br>nichts | 1 nötig, 0 empf. | – |
| 8 | 1: arena cherry bomb<br>1: arena snoll ×4<br>1: arena bomb ×2<br>2: arena Progenitrix<br>2: arena snoll ×3<br>2: arena cherry bomb ×3 | +Wind −Feuer<br>+Feuer −Eis<br>+Wind −Feuer<br>+Wind −Feuer<br>+Feuer −Eis<br>+Wind −Feuer | Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Bind, Heavy, Slow, Blind, Tod/fest<br>Bind, Heavy, Slow, Blind, Tod/fest<br>Stun, Schlaf, Slow, Petr., Lähmung, Unterbr., Blind<br>Bind, Heavy, Slow, Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest | 0 nötig, 0 empf. | Spucksardine, Stimme des Widders, Ultravibration |
| 9 | 1: Guimauve |  −Hieb/Stich/Schlag | Slow, Unterbr., Blind | 0 nötig, 6 empf. | Spucksardine |
| 10 | 1: Crom Dubh |  −Hieb | Slow, Unterbr., Blind | 0 nötig, 0 empf. | Spucksardine |
| 11 | 1: arena gas bomb ×2<br>2: arena gas bomb ×4 | +Wind −Feuer<br>+Wind −Feuer | nichts<br>nichts | 0 nötig, 1 empf. | – |
| 12 | 1: arena roselet ×4<br>2: Hydnora | +Feuer<br>+Wasser −Erde | Slow, Blind, Tod/fest<br>Stun, Blind | 2 nötig, 1 empf. | – |
| 13 | 1: arena haagenti<br>1: arena vodoriga ×2<br>2: Carmilla | –<br>–<br>– | Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Slow, Blind | 0 nötig, 0 empf. | Spucksardine, Stimme des Widders, Ultravibration |
| 14 | 1: arena jam ×2<br>2: arena jam ×4 |  −Schlag<br> −Schlag | nichts<br>nichts | 1 nötig, 0 empf. | – |
| 15 | 1: bestial node | – | Slow, Unterbr., Blind | 0 nötig, 0 empf. | Spucksardine |
| 16 | 1: arena cyclops ×2<br>2: Tikbalang | –<br>– | Schlaf, Heavy, Lähmung, Blind<br>Slow, Blind | 0 nötig, 1 empf. | – |
| 17 | 1: right claw<br>1: left claw<br>2: Kreios | +Blitz<br>+Blitz<br>+Blitz | Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Slow, Petr., Lähmung, Unterbr., Blind | 0 nötig, 0 empf. | Spucksardine, Stimme des Widders, Ultravibration |
| 18 | 1: arena manticore<br>1: firesand keg ×12<br>2: arena manticore ×2<br>2: firesand keg ×12 | –<br>–<br>–<br>– | Slow, Petr., Lähmung, Blind<br>nichts<br>Slow, Petr., Lähmung, Blind<br>nichts | 0 nötig, 0 empf. | Stimme des Widders, Ultravibration |
| 19 | 1: Reflective Rebekkah<br>2: Reflective Rebekkah | –<br>– | Unterbr., Blind<br>Unterbr., Blind | 0 nötig, 1 empf. | Spucksardine |
| 20 | 1: Typhon<br>2: Ultros<br>3: Typhon | –<br>+Feuer<br>– | nichts<br>Unterbr.<br>nichts | 1 nötig, 5 empf. | Spucksardine |
| 21 | 1: arena imp ×2<br>2: Apademak | +Feuer −Eis<br>– | Stun, Schlaf, Slow, Petr., Lähmung, Unterbr., Blind, Tod/fest<br>Unterbr., Blind | 0 nötig, 0 empf. | Spucksardine, Stimme des Widders, Ultravibration |
| 22 | 1: arena grenade ×4<br>2: the Forefather | –<br>– | Tod/fest<br>Slow, Blind | 0 nötig, 1 empf. | – |
| 23 | 1: kronprinz behemoth | – | Slow, Blind | 1 nötig, 0 empf. | – |
| 24 | 1: arena viking<br>1: arena magus<br>2: arena scribe<br>3: Epilogi |  −Hieb/Stich/Schlag<br> −Magie<br>–<br>– | Stun, Schlaf, Bind, Heavy, Slow, Petr., Lähmung, Blind, Tod/fest<br>Slow, Unterbr., Blind, Tod/fest<br>Slow, Unterbr., Blind<br>Slow, Blind | 0 nötig, 1 empf. | Spucksardine |
| 25 | 1: Azulmagia<br>2: Azulmagia<br>3: Azulmagia | –<br>–<br>– | nichts<br>nichts<br>nichts | 1 nötig, 0 empf. | – |
| 26 | 1: arena mirrorknight<br>2: Papa Humbaba | –<br>+Erde −Blitz | Unterbr.<br>nichts | 2 nötig, 1 empf. | Spucksardine |
| 27 | 1: Typhon | – | nichts | 2 nötig, 1 empf. | – |
| 28 | 1: Durinn |  −Feuer | Unterbr. | 1 nötig, 1 empf. | Spucksardine |
| 29 | 1: shikigami of the pyre<br>2: shikigami of the undertow | +Wind −Feuer<br>+Blitz −Wasser | Unterbr.<br>Unterbr. | 2 nötig, 3 empf. | Spucksardine |
| 30 | 1: Siegfried<br>2: Siegfried<br>3: Siegfried | –<br>–<br>– | nichts<br>nichts<br>nichts | 1 nötig, 3 empf. | – |
| 31 | 1: Gogo, Master of Mimicry<br>2: Gogo, Master of Mimicry | –<br>– | Unterbr.<br>Unterbr. | 3 nötig, 0 empf. | Spucksardine |
| 32 | 1: Goldor<br>2: Goldor | –<br>– | Unterbr.<br>Unterbr. | 7 nötig, 2 empf. | – |
