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
    a09dc9257). Es enthält je Gegner Schwäche oder Immunität gegen sechs Elemente und vier Angriffsarten. Dazu kommt die
    Verwundbarkeit für Slow, Petrifikation, Lähmung, Unterbrechung, Blind, Stun, Schlaf, Bind, Heavy und festen oder
    tödlichen Schaden. Die Einweisung im Spiel zeigt dieselben Daten. RSR sperrt damit heute Zauber gegen immune
    Gegner, wählt das Set aber nicht danach.
  - *Die Wahl:* je Akt die gelernten Zauber nach Wert gegen diese Gegner. Schwäche zählt hoch, Immunität fällt weg,
    eine Verwundbarkeit öffnet eine Kette aus der Synergietabelle, etwa Petrifikation mit Bohrkanone oder Ultravibration.
    Dazu Heilung, Abwehr und Unterbrechung (Spucksardine), wo ein Gegner unterbrechbar ist.
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
