# 01 · Bewertungswerkzeuge für den Loop

Entwurfsdokument nach ADR-Struktur, auf Auftrag des Auftraggebers: zu prüfen, ob der Loop
Bewertungskonzepte wie die SWOT-Analyse und verwandte Hilfsmittel braucht. Es stellt den geltenden
Sachstand dar; die bindenden Regeln stehen in `CLAUDE.md`, die Prüfhistorie in `AUDIT_LOG.md` (A97).

## Ergebnis

**Der Loop ist ein Fehlerbehebungsverfahren und hat keinen Eingang für „hier wäre etwas möglich".**
Er beginnt mit „Research: Fehlerbild vom Fehler trennen" und bewertet Optionen gegen Schweregrad,
Aufwand, Blast Radius und Folgekosten. Alle vier Größen messen **Kosten und Risiko**. Keine Stufe
fragt, was ein Baustein **eröffnet**. Das ist kein Ausführungsmangel, sondern eine Lücke im Verfahren:
Wo kein Defekt vorliegt, springt der Loop gar nicht erst an.

**Belegt an der eigenen Historie, nicht behauptet.** Jede größere Möglichkeit dieses Projekts kam vom
Auftraggeber, nicht aus dem Loop:

| Vorgang | Bewertung durch den Loop | Wer die Möglichkeit einbrachte |
|---|---|---|
| Schadensrate je Gruppenmitglied | „Baustein ohne Verbraucher ist Vorratsarbeit" — verworfen | der Auftraggeber („was wäre umsetzbar durch Ermittlung während der Laufzeit?") |
| Güte der Schätzung messen | „braucht einen externen Beobachter" | der Auftraggeber („das kann der code auch selbst vollbringen") |
| Schadenspotential der Flächenaktionen | Nullvariante aus Kostengründen | der Auftraggeber, ausgearbeitet mit Höchstwert-Fortschreibung |
| Grenze „bewältigbar" | von mir gesetzt: 600 | der Auftraggeber („wo könnte Automation die Entscheidung übernehmen?") |

**Drei Werkzeuge schließen die belegten Lücken; drei weitere sind geprüft und nicht aufgenommen.**
Aufgenommen sind die Chancenfrage aus SWOT/TOWS als vierte Querschnittsanforderung, das Premortem als
dritte Hypothese der Falsifikationsstufe und Cynefin als Einordnung, welcher Prüfgrad überhaupt
erreichbar ist.

**Was nicht geschieht: eine Methodensammlung.** Der Loop hat zehn Stufen und drei
Querschnittsanforderungen; jede weitere Stufe verteuert jeden Vorgang. Aufgenommen wird nur, wofür
ein Fehlschlag in der eigenen Historie vorliegt.

## Die Quellenlage, geprüft statt übernommen

Zwei verbreitete Angaben halten der Prüfung nicht stand und werden hier deshalb nicht verwendet.

**Die Urheberschaft der SWOT-Analyse ist ungeklärt.** Die geläufige Zuschreibung an die Harvard
Business School — Learned, Christensen, Andrews und Guth, *Business Policy: Text and Cases*, 1965 —
gilt als widerlegt: In dem Buch kommen die vier Wörter vor, die SWOT-Analyse als Werkzeug jedoch
nicht, und Guth als letzter überlebender Beteiligter hat die Herkunft 2017 ausdrücklich bestritten.
Die Gegenerzählung (Albert Humphrey, Stanford, „SOFT" → „SWOT") ist verbreitet, aber ebenfalls nicht
gesichert belegt. **Folge für dieses Dokument:** Die Methode wird ohne Urheberzuschreibung benutzt;
ihre Brauchbarkeit hängt nicht daran.

**Die Wirksamkeit des Premortem ist schwächer belegt als die Fachliteratur nahelegt.** Belegt ist die
zugrunde liegende *prospektive Rückschau*: Mitchell, Russo und Pennington, „Back to the future:
Temporal perspective in the explanation of events", *Journal of Behavioral Decision Making* 2 (1989)
— ein Ereignis als bereits eingetreten zu unterstellen erhöht im Laborversuch die Fähigkeit, seine
Ursachen zu benennen, um rund 30 %. Das Premortem als Verfahren (Gary Klein, *Harvard Business
Review*, 2007) baut darauf auf, ist selbst aber **nicht** durch eine begutachtete Studie als
risikoaufdeckend belegt. **Folge:** Es wird als Denkform aufgenommen, nicht mit einer Prozentzahl
beworben.

## Was aufgenommen wird, und wogegen es belegt ist

### Die Chancenfrage — als vierte Querschnittsanforderung

**Das Werkzeug.** Von der SWOT-Analyse ist allein die Zelle *Opportunities* hier von Wert, und sie
allein erzeugt nichts: Eine Liste von Chancen ist kein Ergebnis. Was etwas erzeugt, ist die
**TOWS-Matrix** (Heinz Weihrich, 1982), die Strategien aus Kreuzungen ableitet — insbesondere die
Zelle **Stärke × Chance**: *Welche vorhandene Stärke trifft hier auf welche neue Möglichkeit?*

**Warum genau diese Zelle.** Dieses Projekt hat eine ungewöhnlich große Stärkenseite, die im Loop
nicht vorkommt: eine Gesundheitshistorie über vier Minuten, ein Effekt-Handler, der jeden Treffer
sieht, eine Vorhersage fremder Casts, eine Diagnoseanzeige. Jede der vier oben belegten Möglichkeiten
war eine Kreuzung aus einer dieser Stärken mit einer offenen Frage — und jede wurde übersehen, weil
der Loop bei der offenen Frage begann und nur nach ihrer Behebung suchte.

**Nicht als Stufe, sondern als Bedingung jeder Stufe.** Eine eigene Stufe stünde an einem Ort und
wäre damit überspringbar; die Lücke ist aber überall. Die Erhebung (Stufe 1) hat zu fragen, was die
Stelle **könnte**, nicht nur, was an ihr falsch ist. Die Optionen (Stufe 2) haben die Möglichkeit
mitzuführen, nicht nur die Behebung. Die Abwägung (Stufe 3) hat dem Aufwand einen Ertrag
gegenüberzustellen, den sie heute gar nicht erhebt.

**Denkhilfe innerhalb dieser Anforderung: das Kano-Modell** (Noriaki Kano, 1984) mit seiner
Unterscheidung in Basis-, Leistungs- und Begeisterungsmerkmale. Es ist kein eigener Schritt, aber es
benennt, was der heutige Loop strukturell erzeugt: ausschließlich Basismerkmale. `TODO.md` besteht zu
fast neun Zehnteln aus Defekten — das ist die Signatur eines Verfahrens, das nur auf Fehler anspringt.

### Das Premortem — als dritte Hypothese der Falsifikationsstufe

**Das Werkzeug.** Prospektive Rückschau: Das Vorhaben wird als **bereits gescheitert** unterstellt,
und die Gründe werden rückwärts gesucht. Der Unterschied zur vorhandenen Falsifikation ist nicht
graduell. Stufe 6 vertritt heute zwei Hypothesen — *es liegt kein Defekt vor* und *die gewählte Option
ist falsch*. Beide fragen nach der **Richtigkeit** der Analyse. Das Premortem fragt nach der
**Wirkung** der Umsetzung, und das ist eine andere Frage.

**Belegt an zwei eigenen Fehlschlägen.** Die Schildanrechnung wurde als „absolut sicher richtig"
berichtet, weil ihre Wirkkette am Code belegt war — die Frage, ob eine Barriere den Heilbedarf
überhaupt senkt, stellte keine der beiden Hypothesen. Und das Tor `AutoHealRatio` in der Zielwahl
hätte die gesamte Vorausschau wirkungslos gemacht: Beide Hypothesen waren geprüft und widerlegt, die
Änderung war fertig, und gefunden wurde das Tor erst beim Verfolgen der Aufrufkette. Die Frage, die
es sofort gefunden hätte, lautet: *Die Änderung ist ausgeliefert und es ändert sich nichts — warum?*

### Cynefin — als Einordnung, welcher Prüfgrad erreichbar ist

**Das Werkzeug.** Das Cynefin-Rahmenwerk (Dave Snowden) ordnet Problemräume in fünf Domänen — klar,
kompliziert, komplex, chaotisch und verworren — und weist jeder ein anderes Vorgehen zu. Für uns
zählt die Grenze zwischen **kompliziert** (analysierbar, Expertenurteil führt zur Antwort) und
**komplex** (Muster zeigen sich erst im Handeln; das Vorgehen ist *probe – sense – respond*).

**Der Befund, der die Aufnahme trägt, steht in fast jedem Archiveintrag dieses Projekts:** „Im Spiel
nicht beobachtet." Das ist kein Mangel an Sorgfalt, sondern die Folge davon, ein **komplexes** System
wie ein **kompliziertes** zu behandeln. Kampfverhalten in einer Live-Instanz mit fremden Spielern ist
nicht durch statische Analyse abschließend beurteilbar — nicht, weil die Analyse zu schwach wäre,
sondern weil die Antwort dort nicht liegt.

**Die Folge ist keine Entschuldigung, sondern eine Pflicht.** In der komplexen Domäne ist das erste
Element *probe* — eine Sonde. Wer eine Verhaltensänderung in dieser Domäne baut, hat das Messmittel
**mitzuliefern**, nicht am Ende festzustellen, dass die Wirkung unbelegt bleibt. Der Auftraggeber hat
dasselbe zweimal aus eigener Anschauung gesagt: „die beobachtung … kann der code auch selbst
vollbringen. dazu braucht es keinen externen beobachter!" Cynefin liefert die Begründung, warum das
die Regel sein muss und nicht der Einzelfall.

**Gegenprobe an einem Fall, in dem es getragen hat:** Die Aussage „wie oft `IsHostileCastingAOE` im
Trash-Pull wahr ist, ist von hier aus nicht messbar" stand in A94 als Grenze. Eine Anzeige der drei
Gefahrenquellen je Mitglied hat sie in A95 aufgehoben. Die Sonde kostete wenige Zeilen; die Grenze war
keine.

## Was geprüft und nicht aufgenommen wurde

**Die Engpasstheorie** (Eliyahu Goldratt) fragt vor jeder Verbesserung, ob die Stelle der Engpass ist
— sonst verpufft der Gewinn. Sachlich richtig und hier ohne Beleg: Die Reihenfolge der Arbeit folgt in
diesem Projekt dem Auftrag des Auftraggebers, nicht einer eigenen Priorisierung, und die Regel dazu
steht bereits in `CLAUDE.md` („Priorität folgt dem Auftrag, nicht der Fundlage"). Eine zweite
Priorisierungsregel daneben würde mit der ersten konkurrieren.

**Wardley Mapping** (Simon Wardley) trägt Komponenten auf Wertschöpfungskette und Entwicklungsgrad ab
und ist das stärkste der erwogenen Werkzeuge für die Frage „wohin entwickelt sich das". Nicht
aufgenommen, weil sein Ertrag an der Positionierung **gegenüber Wettbewerbern und Lieferanten** hängt
— eine Dimension, die dieses Fork nicht hat. Der Anteil, der hier trüge, ist die Unterscheidung
zwischen selbstgebauten und übernommenen Bausteinen, und die führt `06-fork-audit.md` bereits.

**Die Szenariotechnik** (Pierre Wack, Shell, 1970er) spielt mehrere plausible Zukünfte durch. Für
einen Fork, dessen Zukunft im Wesentlichen aus Upstream-Änderungen und Spielpatches besteht, ist der
Ertrag gering: Beide Quellen sind beobachtbar, nicht zu erdenken. Der Teil, der trüge — was passiert
bei der nächsten Erweiterung mit dieser Konstruktion — ist bereits als *Lack of Movement* nach Parnas
in der Querschnittsanforderung Kausalität enthalten.

## Konsequenzen

**Für den Loop.** Drei Ergänzungen, keine neue Stufe. Die Chancenfrage als vierte
Querschnittsanforderung, das Premortem als dritte Hypothese in Stufe 6, die Domänenfrage in der
Aufwandsregel der REGEL. Alle drei stehen in `CLAUDE.md`.

**Für die Kosten je Vorgang.** Die Chancenfrage und das Premortem sind je ein Satz Denkarbeit und
kosten nichts an Werkzeug. Die Domänenfrage kostet etwas: Sie verlangt bei Verhaltensänderungen ein
mitgeliefertes Messmittel. Das ist der einzige Punkt mit Aufwand, und er ersetzt Aufwand an anderer
Stelle — nämlich das nachträgliche Beantworten von Fragen, die im Spiel längst hätten sichtbar sein
können.

**Für den Auftraggeber.** Die Ergänzungen betreffen meine Arbeitsweise, nicht das Verhalten des
Plugins. Sie sind in `CLAUDE.md` eingetragen, weil das die Datei ist, die die Arbeitsweise trägt; wer
sie zurücknehmen will, streicht drei Absätze.
