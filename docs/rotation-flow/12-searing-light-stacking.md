# Searing Light bei mehreren Beschwörern

*Zum Namen: Der Auftraggeber nennt die Aktion „Gleißender Schein". Der Job-Guide von Square Enix ist
vom Egress gesperrt — wiederholt geprüft, nicht erinnert —, eine belegte Zuordnung deutscher zu
englischer Bezeichnung steht damit nicht zur Verfügung. Dass **Searing Light** gemeint ist, ist aus
der Fragestellung geschlossen und als Schluss gekennzeichnet. Das Dokument benutzt durchgehend den
englischen Bezeichner.*

## Sachstand

Bei **einem** Beschwörer ist der Ablauf richtig. Ab **zwei** reicht das genutzte Zündfenster nicht
mehr aus, und der Verlust ist strukturell, nicht graduell: Jeder Beschwörer darf Searing Light nur
während seiner Solar-Bahamut-Beschwörung zünden, und dieses Fenster kommt nur alle 120 Sekunden —
genau so oft wie die Aktion selbst. Sind die Rotationen synchron, fallen alle Gelegenheiten
zusammen, und alle bis auf eine verfallen.

Drei Eingriffe sind möglich. Einer ist eine Defektbehebung ohne Gegenargument, einer eine Erweiterung
mit einer Bedingung, einer ist abzulehnen:

| | Inhalt | Bewertung |
|---|---|---|
| **V1** | Den Searing Light eines anderen Beschwörers als Buff-Fenster für die eigenen Aetherflow-Ausgaben werten | umsetzen |
| **V2** | Das Zündfenster auf alle großen Beschwörungen erweitern, sobald ein zweiter Beschwörer in der Gruppe ist | umsetzen, mit Gruppenprüfung als Schalter |
| **V4** | Die Bindung an die Beschwörung ganz lösen, Zündung bei Kampf und vorhandenem Ziel | gemessen, im Nutzungsprofil nicht besser — nicht umsetzen |
| **V5** | Zusätzlich außerhalb eines Fensters zünden, wenn kein anderer bekannter Beschwörer die Lücke decken kann | gemessen, bei drei bis sechs Beschwörern am besten, bricht bei sieben ein — nicht umsetzen |

**Der begrenzende Faktor ist nicht die Wiederholzeit, sondern das Zündfenster.** Sechs Beschwörer
haben zusammen genug Ladungen für lückenlose Abdeckung (6 × 20 s = 120 s). Dass sie nicht ankommt,
liegt allein daran, wann gezündet werden darf.

## Die Zeitstruktur

Ohne sie ist keine der Fragen zu beantworten. Alle Größen außerhalb des Quelltextes stammen aus
Fremdquellen (siehe Nachweisgrenzen).

| Größe | Wert |
|---|---|
| Searing Light, Wirkdauer | 20 s |
| Searing Light, Wiederholzeit | 120 s |
| Wirkung | +5 % Schaden für die ganze Gruppe |
| Große Beschwörung, Standzeit | 15 s |
| Große Beschwörung, Wiederholzeit | 60 s |
| Reihenfolge der großen Beschwörungen | Solar Bahamut → Bahamut → Solar Bahamut → Phoenix |

Daraus folgt die entscheidende Asymmetrie: **Pro 120 Sekunden gibt es zwei Beschwörungsfenster, aber
nur eines davon ist Solar Bahamut.** Die Rotation zündet ausschließlich im Solar-Fenster
(`SMN_Reborn.cs:203`, lokale Variable `burstInSolar`). Ein Beschwörer hat damit genau **eine**
Gelegenheit pro Wiederholzeit — und sie liegt bei allen Beschwörern einer synchron gestarteten
Gruppe zur selben Zeit.

Zum Vergleich: Für lückenlose Abdeckung wären sechs Zündungen pro 120 Sekunden nötig (6 × 20 s).

## Überschreiben, nicht stapeln

Der Auftraggeber hat darauf hingewiesen, und die Folge ist schärfer, als „verschwendet" es trifft.
Ein zweiter Searing Light auf einen laufenden ersten stapelt nicht, er **ersetzt** ihn — die Wirkung
bleibt bei 5 %, die Restzeit springt zurück auf 20 Sekunden.

Daraus folgt ein Wert, der vom Zeitpunkt abhängt, nicht von der Zündung selbst:

| Zündung von Beschwörer B, nachdem A gezündet hat | Nettogewinn |
|---|---|
| sofort (Restzeit 20 s) | 0 s — die Ladung ist vollständig verloren |
| nach 10 s (Restzeit 10 s) | 10 s |
| nach 19 s (Restzeit 1 s) | 19 s |
| nach Ablauf | 20 s, volle Wirkung |

Der vorhandene Schutz nutzt genau das: `IsStatusProvided` (`ActionBasicInfo.cs:691`) sperrt die
Zündung, solange der Status nicht innerhalb von `StatusRefreshGcdCount` GCDs endet — Vorgabe 2
(`ActionConfig.cs:66`), also etwa fünf Sekunden vor Ablauf. Der Schutz verhindert damit die teuren
Fälle und erlaubt die billigen. **Er ist richtig gebaut und bleibt unangetastet.**

Entscheidend dafür ist `StatusFromSelf = false` (`SummonerRotation.cs:490`): `PlayerGetStatus`
(`StatusHelper.cs:1529`) filtert nur bei `isFromSelf` auf die eigene Quelle, hier zählt also jeder
fremde Buff. Sein Gegenstück `HasSearingLight` (`SummonerRotation.cs:271`) ruft
`PlayerHasStatus(true, …)` und zählt nur den eigenen — auch das ist für seine ursprüngliche Frage
richtig. Aus dem Zusammentreffen beider entsteht der Befund von V1.

## Die Fälle von einem bis acht Beschwörern, gemessen

Die Prozentzahlen früherer Fassungen waren Kopfrechnungen, und eine davon war falsch. Sie stammen
jetzt aus `.github/scripts/audit/searing_light_coverage.py`, das die Regeln durchrechnet statt ihr
Ergebnis abzuschätzen: 20 s Wirkung, 120 s Wiederholzeit ab Zündung, Überschreiben statt Stapeln,
Beschwörungen 15 s alle 60 s in der Reihenfolge Solar, Bahamut, Solar, Phoenix, und die Sperre, die
fünf Sekunden vor Buff-Ende öffnet. Gemessen wird der Anteil der Kampfzeit mit laufendem Buff.

**Synchrone Rotationen — der saubere Pull:**

| Beschwörer | heute (nur Solar) | V2 (alle Demis) | V4 (ohne Bindung) |
|---|---|---|---|
| 1 | 17 % | 17 % | 17 % |
| 2 | 17 % | **33 %** | 29 % |
| 3 | 17 % | 33 % | 42 % |
| 4 | 17 % | 33 % | 54 % |
| 5 | 17 % | 33 % | 67 % |
| 6 | 17 % | 33 % | 79 % |
| 7 | 17 % | 33 % | 92 % |
| 8 | 17 % | 33 % | **100 %** |

**Voll auseinandergelaufene Rotationen — nach Toden, Bewegung, Betäubungen:**

| Beschwörer | heute (nur Solar) | V2 (alle Demis) | V4 (ohne Bindung) |
|---|---|---|---|
| 1 | 17 % | 17 % | 17 % |
| 2 | 33 % | 33 % | 33 % |
| 3 | 50 % | 50 % | 50 % |
| 4 | 66 % | 66 % | 66 % |
| 5 | 67 % | 67 % | 67 % |
| 6 | 67 % | 74 % | 79 % |
| 7 | 67 % | **90 %** | 92 % |
| 8 | 67 % | **96 %** | 100 % |

Die Zwischenstufe (halb auseinandergelaufen) liegt dazwischen; das Skript gibt sie mit aus.

### Was die Zahlen sagen

**Die Rechnung des Auftraggebers geht auf, mit zwei Korrekturen.** Searing Light wirkt 20 Sekunden,
nicht 15 — die 15 sind die Standzeit der Beschwörung. Und die Wiederholzeit läuft ab der Zündung,
nicht ab Buff-Ende; ein Intervall von 135 Sekunden gibt es nicht, der Zyklus ist 120. Damit liegt die
Schwelle für rechnerisch lückenlose Abdeckung bei **sechs** Beschwörern, nicht erst bei acht:
6 × 20 s = 120 s. Die Schlussfolgerung — bei acht Beschwörern wäre der Buff nahezu dauerhaft — ist
richtig und wird von der Messung bestätigt, allerdings nur unter Bedingungen, die weiter unten
stehen.

**Der Versatz ist der stärkste einzelne Hebel, stärker als jede der drei Zündregeln.** Bei acht
Beschwörern und heutigem Code steigt die Abdeckung allein durch auseinandergelaufene Rotationen von
17 % auf 67 %. Der Grund: Gestreute Beschwörungsfenster treffen die Lücken zwischen den Buffs, die
bei synchronem Pull sämtlich unbesetzt bleiben.

**Mehr Beschwörer heißt nicht immer mehr Abdeckung.** Bei halbem Versatz liefert V2 mit sieben
Beschwörern 84 % und mit acht 83 %. Das ist kein Rechenfehler des Modells, sondern gierige Zuteilung
und damit ein echter Effekt: Wer zuerst in einem Fenster steht, zündet — und kann damit jemandem
zuvorkommen, dessen eigene Wiederholzeit eine spätere Lücke gedeckt hätte. Kein Client sieht die
Wiederholzeiten der anderen und kann zurückstehen. Der Selbsttest des Skripts prüft diese
Eigenschaft deshalb ausdrücklich **nicht**; eine frühere Fassung behauptete sie und war widerlegt.

## Die Gruppenzusammensetzung als Schalter

Die Erweiterung darf nicht bedingungslos gelten, und der Auftraggeber hat den richtigen Ort dafür
benannt: die Zusammensetzung der Gruppe.

**Warum die Prüfung nötig ist.** Bei einem einzelnen Beschwörer ist die Erweiterung nicht neutral.
Wird seine Wiederholzeit zu einem Zeitpunkt frei, an dem gerade Bahamut oder Phoenix steht — nach
verzögertem Kampfbeginn, nach einer Unterbrechung, nach einer Phase ohne Ziel —, zündet er künftig
dort statt im nächsten Solar-Fenster. Das kostet ihn die Bündelung mit seinem stärksten Fenster und
verschiebt ihn dauerhaft aus dem Takt der Gruppe. Der Regelfall würde also für einen Gewinn im
Sonderfall bezahlen.

**Mechanismus.** Die Zahl der weiteren Beschwörer in der Gruppe ist aus `DataCenter.PartyMembers` und
`IsJobs(Job.SMN)` zu ermitteln — dasselbe Muster, das `DataCenter.HasLivingRaiser` für die
Rezzerfrage benutzt. Die Erweiterung des Zündfensters gilt nur, wenn mindestens ein weiterer
Beschwörer lebt. Tote zählen nicht mit, denn sie zünden nichts.

**Was die Prüfung nicht leisten kann:** Sie sieht nicht, ob der andere Beschwörer überhaupt RSR
benutzt, ob seine Rotation dieselbe ist oder ob er von Hand spielt. Sie beantwortet nur „kann
überhaupt ein zweiter Searing Light kommen" — und genau das ist die Frage, auf die es ankommt.

**Nicht empfohlen: einfach `BahamutBurst` übernehmen.** Das wäre die kleinste Textänderung, brächte
aber eine zweite Verhaltensänderung mit: `BahamutBurst` ist zusätzlich an `CanBurst` gebunden, also
an `AutoStatus.Burst`, der nur gesetzt ist, wenn der Nutzer den Burst-Befehl gibt oder
`Service.Config.AutoBurst` an ist (`StateUpdater.cs:847`). `burstInSolar` prüft das heute nicht. Zwei
Änderungen in einer Zeile sind nicht auswertbar, wenn der Spieltest fehlschlägt.

## Versatz zwischen den Rotationen

Der Auftraggeber nennt Tod, Bewegung und Betäubung. Der Befund dazu ist nicht der erwartete: **Der
Versatz ist kein Problem, sondern der Verbündete der Erweiterung.**

| Ursache | Wirkung auf den Zyklus |
|---|---|
| Tod und Wiederbelebung | Beschwörungen und Wiederholzeiten laufen weiter, die Rotation setzt aber versetzt wieder ein; zusätzlich kostet die Schwäche nach der Wiederbelebung Schaden |
| Bewegung | Wirkzeitgebundene Zauber entfallen, die Beschwörungskette verschiebt sich um GCDs |
| Betäubung, Stille, Bewegungsunfähigkeit | dasselbe, in Stufen |
| Phasenwechsel ohne Ziel | die Beschwörung wird nicht gestartet, das Fenster verschiebt sich um bis zu 60 s |

Alle vier streuen die Beschwörungsfenster über die Zeit. Bei synchronen Rotationen liegen zwei
Fenster je 120 Sekunden; bei versetzten liegen bis zu *2n* Fenster verteilt, und jedes davon ist eine
Gelegenheit, die der laufende Sperrmechanismus korrekt filtert — er lässt zünden, wenn kein Buff
steht, und blockiert, wenn einer steht.

Daraus folgt: Der Versatz ist der **stärkste einzelne Hebel** — bei acht Beschwörern hebt er die
Abdeckung allein, ohne jede Codeänderung, von 17 % auf 67 %. Keine der Zündregeln bewirkt im
synchronen Fall auch nur annähernd so viel.

## Was über die anderen Beschwörer bekannt ist

Eine frühere Fassung dieses Dokuments behauptete, kein Client kenne die Wiederholzeiten der anderen
und eine Staffelung sei deshalb unmöglich. **Das ist falsch, und der Auftraggeber hat es widerlegt**
(`AUDIT_LOG.md` C42): Man weiß zwar nicht, wann ein anderer Beschwörer zünden *wird* — aber ab
seiner ersten Zündung weiß man, wann er frühestens wieder kann.

Die Information liegt im Status selbst. `IStatus.SourceId` benennt den Urheber, und
`StatusHelper.PlayerGetStatus` (`:1529`) liest ihn bereits — die Unterscheidung eigener und fremder
Buffs beruht darauf. Sieht ein Client einen Searing Light mit fremder Quelle, kennt er damit den
Urheber und über die Restzeit auch den Zündzeitpunkt. Frühestmögliche Wiederkehr: Zündung plus 120
Sekunden.

**Was daraus eine Regel macht (V5):** Halte dich an die Beschwörungsfenster wie in V2 — und zünde
zusätzlich außerhalb eines Fensters, wenn kein anderer *bekannter* Beschwörer die kommende Lücke
überhaupt decken kann. Das ist der informierte Mittelweg zwischen V2 (wartet immer auf ein Fenster)
und V4 (zündet blind, sobald möglich).

**Eine Bedingung davon stammt aus der Messung, nicht aus der Überlegung.** Die erste Fassung der
Regel erlaubte das Zünden außerhalb, sobald die Sperre sich löste — also in den letzten fünf Sekunden
des laufenden Buffs. Gemessen fiel sie damit bei zwei Beschwörern **unter** V2: Eine ganze Ladung
wird für wenige Sekunden Nettogewinn verbrannt. Was innerhalb eines Fensters als Auffrischung
sinnvoll ist, ist außerhalb Verschwendung. Die Regel verlangt deshalb, dass der Buff **vollständig
abgelaufen** ist.

**Gemessene Abdeckung (synchroner Pull):**

| Beschwörer | V2 | V4 | **V5** |
|---|---|---|---|
| 2 | 33 % | 29 % | 33 % |
| 3 | 33 % | 42 % | **50 %** |
| 4 | 33 % | 54 % | **66 %** |
| 5 | 33 % | 67 % | **83 %** |
| 6 | 33 % | 79 % | **99 %** |
| 7 | 33 % | 92 % | 60 % |
| 8 | 33 % | 100 % | 60 % |

**Der Einbruch bei sieben und acht ist kein Messfehler, sondern die Grenze des Ansatzes.** Bei sechs
Beschwörern verteilen sich die Ladungen gerade so, dass jede Lücke gedeckt wird — 99 %. Kommt ein
siebter dazu, besetzt er eine Lücke einen Moment zu früh, die Staffelung zerfällt, und das Muster
läuft in eine ungünstige Selbstorganisation. Dieselbe gierige Zuteilung wie oben, hier mit
sichtbarer Folge: Die Regel ist **nicht robust gegenüber der Gruppengröße**. Ein Client, der nur
weiß, wann andere frühestens *könnten*, kann nicht verhindern, dass mehrere dieselbe Lücke anpeilen.

**Bewertung: nicht umsetzen, aber festhalten.** Der Gedanke ist richtig und die Information
tatsächlich verfügbar. Gegen die Umsetzung sprechen drei Dinge: Im Nutzungsprofil (ein bis zwei
Beschwörer) bringt V5 gegenüber V2 **nichts** — beide liefern 33 %. Der Gewinn liegt bei drei bis
sechs Beschwörern, und dort bricht er bei sieben wieder ein. Und der Preis ist ein Gedächtnis über
Frames hinweg: beobachtete Zündungen je Urheber, mit Rücksetzen bei Kampfende, Gruppenwechsel und
Zonenwechsel — deutlich mehr Zustand als V1 und V2 zusammen, für einen Fall, der praktisch nicht
eintritt.

## Gesamtbetrachtung

Die drei Fragen greifen ineinander, und die Reihenfolge ihrer Behandlung ist nicht beliebig.

**Der Sperrmechanismus ist die Grundlage und bleibt.** Er ist das einzige Abstimmungsmittel zwischen
Clients, die einander nicht kennen. Jede Erweiterung des Zündfensters ist nur deshalb ungefährlich,
weil er dahinter steht: Mehr Gelegenheiten führen nicht zu mehr Überschreibungen, sondern zu mehr
genutzten Lücken.

**V1 und V2 wirken in verschiedene Richtungen und stören einander nicht.** V1 verbessert, was der
**gesperrte** Beschwörer während eines fremden Buffs tut; V2 verbessert, **wann** er selbst zünden
darf. V1 wirkt auch dann, wenn V2 nicht greift — etwa bei einem Beschwörer, der von Hand spielt und
seinen Buff zu einem beliebigen Zeitpunkt setzt.

**Die Wirkungsbereiche sind getrennt.** V1 berührt drei Bedingungen in `SMN_Reborn.AttackAbility`,
die ausschließlich Aetherflow-Ausgaben steuern. V2 berührt eine lokale Variable derselben Methode.
Keine der beiden Änderungen verlässt die Beschwörer-Rotation; die Basisklasse, die
Aktionseinstellungen und der Sperrmechanismus bleiben unberührt. Betroffen ist allein der Endnutzer,
und nur als Beschwörer.

**Eine Wechselwirkung ist zu benennen:** Mit V2 zündet ein zweiter Beschwörer bei Sekunde 60. Damit
liegt ab dann häufiger ein fremder Buff — was V1 häufiger wirksam macht. Die beiden verstärken
einander, ohne sich zu widersprechen.

**Was keiner der Vorschläge löst — und die frühere Fassung sagte es falsch.** Sie behauptete, die
Abdeckung bleibe auch mit V2 bei 33 %. Das gilt nur für den synchronen Pull; sobald die Rotationen
auseinanderlaufen, erreicht V2 bei sieben und acht Beschwörern 90 bis 96 %. Die Aussage widersprach
dem Versatz-Abschnitt desselben Dokuments und ist zurückgenommen (`AUDIT_LOG.md` C41).

Was bleibt: Der synchrone Pull ist mit V2 bei 33 % gedeckelt, weil es dort schlicht nur zwei
Beschwörungsfenster je 120 Sekunden gibt. Diese Lücke schließt nur V4 — und V4 ist im Nutzungsprofil
nicht besser, siehe unten.

## Die Vorschläge im Einzelnen

### V1 — Den fremden Buff als Buff-Fenster werten

**Kontext:** `SMN_Reborn.cs:320`, `:332`, `:348` bevorzugen Painflare, Necrotize und Fester unter
`inSolarUnique && HasSearingLight`, und `HasSearingLight` zählt nur den eigenen Buff. Ein gesperrter
Beschwörer hält seine Aetherflow-Ausgaben also zurück, während ein 5-%-Fenster auf ihm liegt.

**Mechanismus:** Eine zweite, benannte Eigenschaft neben `HasSearingLight`, die
`PlayerHasStatus(false, …)` ruft, und die drei Bedingungen fragen nach ihr.

**Konsequenzen:** Bei einem Beschwörer wirkungslos — beide Prüfungen fallen zusammen. Ab zwei landen
die Ausgaben im laufenden Fenster. Ein Nachteil ist nicht erkennbar: Der Buff wirkt multiplikativ,
unabhängig von seiner Herkunft.

**Bewertung: Defektbehebung.** Die Bedingung soll „im Buff-Fenster" heißen und sagt „in meinem
Buff-Fenster". Der Auftraggeber hat das ausdrücklich bestätigt.

### V2 — Zündfenster auf alle großen Beschwörungen, bei mehreren Beschwörern

**Kontext und Mechanismus:** oben, Abschnitte „Alternative Fenster" und „Gruppenzusammensetzung".

**Konsequenzen, gemessen:** Bei einem Beschwörer unverändert (17 %). Bei zwei 33 % statt 17 % im
synchronen Fall. Bei auseinandergelaufenen Rotationen und sieben bis acht Beschwörern 90 bis 96 %
statt 67 %. Der Buff bei Sekunde 60 ist weniger wert als der bei Sekunde 0, aber mehr als keiner.

**Bewertung: Erweiterung mit Bedingung.** Die Gruppenprüfung ist der Feature-Toggle, den die
Projektregel für eine nicht nachweisbare Verhaltensänderung verlangt — nur ist der Schalter hier
nicht der Nutzer, sondern die Lage, und das ist die bessere Lösung: Sie schaltet genau dann, wenn die
Voraussetzung tatsächlich vorliegt.

### V4 — Zündung ganz von der Beschwörung lösen

Der Vorschlag, nach dem der Auftraggeber ausdrücklich gefragt hat: ein Ansatz jenseits der
Beschwörungsfenster. Er ist gemessen worden, statt ihn wie in der früheren Fassung mit einem Argument
abzutun.

**Mechanismus:** Zünden, sobald kein Buff steht, die eigene Wiederholzeit frei ist und ein Ziel im
Kampf vorliegt. Die letzte Bedingung entschärft den Einwand der früheren Fassung — der Buff verpufft
nicht in einer Phase ohne Gegner.

**Konsequenzen, gemessen:** Im synchronen Fall die mit Abstand höchste Abdeckung — 100 % bei acht
Beschwörern gegenüber 33 % mit V2. Bei auseinandergelaufenen Rotationen liegt es fast gleichauf mit
V2 (92 gegen 90 bei sieben, 100 gegen 96 bei acht).

**Und der Grund, es dennoch nicht zu nehmen, steht in derselben Messung:** Im tatsächlichen
Nutzungsprofil ist es nicht besser, sondern schlechter. Bei **zwei** Beschwörern synchron liefert V4
29 % gegenüber 33 % bei V2 — es zündet früher und bringt damit die Wiederholzeiten in eine
ungünstigere Lage. Bei einem Beschwörer ist die gemessene Abdeckung zwar gleich, der Schaden aber
geringer, weil die Zündung die Bündelung mit dem stärksten eigenen Fenster verliert; das misst dieses
Modell nicht, denn es zählt Sekunden und keinen Schaden.

**Bewertung: nicht umsetzen.** V4 gewinnt erst ab drei Beschwörern und richtig deutlich erst ab
sechs — Gruppen, die es im Spiel praktisch nicht gibt. Bezahlt würde das mit einer Verschlechterung
genau dort, wo Gruppen tatsächlich stehen. Sollte der Auftraggeber je in einer Gruppe mit sechs oder
mehr Beschwörern spielen, ist der Eintrag hier und die Zahlen liegen vor.

### Nullvariante

Für einen Beschwörer richtig und die Empfehlung. Ab zwei nicht mehr tragfähig, weil beide Verluste
dann vollständig greifen und zwei Beschwörer in einer Gruppe gewöhnlich sind.

## Empfehlung

**V1 und V2 umsetzen, V4 nicht.** V1 und V2 verstärken einander und bewegen sich in getrennten
Wirkungsbereichen; beide ändern nur ab zwei Beschwörern etwas. V4 bringt seinen Gewinn erst in
Gruppengrößen, die im Spiel nicht vorkommen, und zahlt dafür bei zwei Beschwörern drauf — das ist
gemessen, nicht abgeschätzt.

**Noch nicht umgesetzt.** Der Zweig `claude/raise-swiftcast-weave-2` trägt fünf ungemessene Eingriffe
am Wiederbelebungspfad, deren Spieltest offen ist; sachfremde Änderungen daneben würden dessen
Auswertung beschädigen. Die Umsetzung gehört auf einen eigenen Zweig, nach Freigabe.

## Erfasst, nicht bearbeitet

`ChurinSMN.cs` trägt denselben V1-Befund (`:1015`, Aetherflow-Ausgabe an `HasSearingLight`). Beim
Zündfenster ist die fremde Rotation dagegen bereits weiter: Sie benutzt `BahamutBurst` (`:948`),
zündet also in jeder großen Beschwörung — allerdings ohne Gruppenprüfung, also auch bei einem
einzelnen Beschwörer. Fremde Rotationsdatei mit eigener Abstimmung; der Befund wird benannt, nicht
behoben.

## Grenzen des Nachweises

Am Quelltext belegt: die gesamte Kette von der Zündbedingung über die Aktionseinstellung und die
Sperrlogik bis zu den Vorgabewerten und der Filterung nach Statusquelle; ebenso, dass die Basisklasse
mit `BahamutBurst` bereits eine weitere Fassung des Zündfensters führt und dass `SMN_Reborn` sie
nicht benutzt.

Aus Fremdquellen: Wirkdauer, Wiederholzeit und Stärke von Searing Light, Standzeit und Wiederholzeit
der großen Beschwörungen, ihre Reihenfolge, und dass Ruby's Glimmer aus der eigenen Ausführung
stammt. Dass ein zweiter Searing Light überschreibt statt zu stapeln, ist die Angabe des
Auftraggebers.

Die Abdeckungszahlen stammen aus `.github/scripts/audit/searing_light_coverage.py`, das die oben
genannten Regeln durchrechnet. Das ist eine Messung am Modell, keine am Spiel — und das Modell hat
benannte Grenzen:

- Es zählt **Sekunden mit Buff, nicht Schaden**. Ein Buff außerhalb des Zwei-Minuten-Takts buffft
  weniger Schaden als einer darin; das fällt in diesen Zahlen nicht auf und ist der Hauptgrund, V4
  nicht allein nach der Abdeckung zu beurteilen.
- Es teilt Fenster **gierig** zu: Wer zuerst darf, zündet. Real entscheidet der Zufall des
  Sekundenbruchteils. Bei nahezu gleichzeitigen Zündungen kann die Wirklichkeit davon abweichen.
- Der Versatz ist als **gleichmäßige** Verteilung modelliert. Im Kampf entsteht er ungleichmäßig und
  in Sprüngen.

Nicht entschieden und nur im Spiel zu klären: welcher Ausgang bei gleichzeitiger Zündung eintritt;
wie groß der Wertunterschied zwischen einem Buff im Zwei-Minuten-Takt und einem daneben tatsächlich
ist; und wie stark der Versatz in einem echten Kampf ausfällt.
