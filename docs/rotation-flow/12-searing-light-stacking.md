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
| **V3** | Die Zündung ganz von der Beschwörung lösen | ablehnen |

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

## Die Fälle von einem bis acht Beschwörern

Angenommen sind synchrone Rotationen (gemeinsamer Pull, niemand stirbt) und ein Kampf von mindestens
120 Sekunden. Der Versatz-Fall steht weiter unten und ändert das Bild erheblich.

| Beschwörer | Zündungen je 120 s | Buff-Abdeckung | Ungenutzte Ladungen je 120 s | Was zusätzlich verloren geht |
|---|---|---|---|---|
| 1 | 1 | 20 s (17 %) | 0 | nichts — Referenzfall |
| 2 | 1 | 20 s (17 %) | 1 | bei einem Spieler: Aetherflow-Fenster, Ruby's Glimmer, Searing Flash |
| 3 | 1 | 20 s (17 %) | 2 | bei zwei Spielern |
| 4 | 1 | 20 s (17 %) | 3 | bei drei Spielern |
| 5 | 1 | 20 s (17 %) | 4 | bei vier Spielern |
| 6 | 1 | 20 s (17 %) | 5 | bei fünf — und ab hier wäre **100 % Abdeckung** möglich |
| 7 | 1 | 20 s (17 %) | 6 | bei sechs |
| 8 | 1 | 20 s (17 %) | 7 | bei sieben |

Die Zeile ist absichtlich eintönig: **Die Abdeckung steigt mit der Zahl der Beschwörer nicht an.**
Sie bleibt bei einem Fenster je 120 Sekunden, weil alle an dieselbe Gelegenheit gebunden sind. Der
gesamte Zuwachs an Ladungen verfällt.

### Wann es nicht mehr passt

**Ab zwei.** Das ist keine graduelle Verschlechterung, sondern die Schwelle: Ein Fenster kann eine
Ladung sinnvoll aufnehmen, die zweite ist in demselben Fenster wertlos (siehe Überschreiben-Tabelle,
Zeile 1). Zwei Beschwörer in einer Gruppe sind gewöhnlich; acht sind der Grenzfall, der die Frage nur
zuspitzt.

### Warum der Gesperrte nicht später nachzündet

Die Sperre löst sich etwa fünf Sekunden vor Buff-Ende, also bei Sekunde 15. Die Beschwörung, die das
Zünden erlaubt, steht ebenfalls 15 Sekunden. Beide Fenster enden im selben Moment — der Gesperrte
verpasst seine Gelegenheit um Sekunden. Danach ist `burstInSolar` falsch, und das bleibt es bis zur
nächsten Solar-Beschwörung 120 Sekunden später, wo dieselbe Kollision erneut auftritt.

Ein zweiter Ausgang ist nicht auszuschließen und von hier nicht entscheidbar: Zünden alle im selben
Sekundenbruchteil, hat der Status den Serverumlauf noch nicht hinter sich, keiner sieht ihn, und alle
zünden. Dann sind *n−1* Ladungen **verbraucht** statt zurückgehalten, bei gleichem Ergebnis.

## Alternative Fenster: die übrigen großen Beschwörungen

Der Vorschlag des Auftraggebers — nicht nur Solar Bahamut, sondern auch Bahamut oder Phoenix — trifft
den wirksamen Punkt, und die Rotationsbasis stellt die dafür nötige Eigenschaft bereits bereit.

`BahamutBurst` (`SummonerRotation.cs:231`) ist ab Stufe 100 in **jeder** großen Beschwörung wahr.
`ChurinSMN` benutzt sie so (`:948`); `SMN_Reborn` benutzt sie nicht, sondern baut mit `burstInSolar`
eine engere eigene Variable. Das ist kein Versehen, sondern eine erkennbare Entwurfsabsicht: Solar
Bahamut ist die stärkste der großen Beschwörungen, und den Gruppenbuff mit dem eigenen stärksten
Schadensfenster zu bündeln ist bei **einem** Beschwörer richtig.

**Was die Erweiterung einbringt:** Ein zweites Fenster je 120 Sekunden, bei Sekunde 60. Zu diesem
Zeitpunkt ist der Buff des ersten Beschwörers seit 40 Sekunden abgelaufen, die Sperre greift also
nicht, und ein zweiter Beschwörer zündet mit voller Wirkung.

| Beschwörer | heute | mit Erweiterung |
|---|---|---|
| 1 | 20 s (17 %) | 20 s (17 %) — unverändert, siehe Gruppenprüfung |
| 2 | 20 s (17 %) | **40 s (33 %)** |
| 3 – 8 | 20 s (17 %) | **40 s (33 %)** |

Der Gewinn tritt vollständig beim Schritt von einem auf zwei Fenster ein und wächst danach nicht
weiter — bei synchronen Rotationen gibt es nicht mehr als zwei Beschwörungsfenster je 120 Sekunden.
Wer mehr will, braucht V3, und V3 ist abzulehnen.

**Der Einwand, der bestehen bleibt:** Ein Searing Light bei Sekunde 60 liegt außerhalb des
Zwei-Minuten-Takts, in dem die übrige Gruppe ihre eigenen Verstärkungen bündelt. Er buffft dort
weniger Schaden als bei Sekunde 0. Sein Wert ist damit geringer als der des ersten, aber deutlich
größer als null — und die Alternative ist nicht „Buff bei Sekunde 0", sondern „gar kein zweiter
Buff". Der Einwand schwächt den Vorschlag ab, widerlegt ihn nicht.

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

Daraus folgt zweierlei. Erstens: Die Erweiterung wirkt bei Versatz **stärker** als in der Rechnung
oben, weil mehr Fenster in Zeiten fallen, in denen kein Buff läuft. Zweitens: Eine ausdrückliche
Staffelung zwischen den Spielern ist weder nötig noch möglich — kein Client kennt die
Wiederholzeiten der anderen, und die einzige verfügbare Abstimmung ist der Buff selbst, den alle
sehen.

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

**Was keiner der Vorschläge löst:** Die Abdeckung bleibt auch mit V2 bei 33 % statt der theoretisch
möglichen 100 %. Der Rest ist ohne Absprache zwischen den Spielern nicht zu holen, und eine solche
Absprache kann ein Rotationshelfer nicht herstellen. Das ist die Grenze, und sie ist zu benennen
statt zu überspielen.

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

**Konsequenzen:** Bei einem Beschwörer unverändert, ab zwei verdoppelte Abdeckung, bei Versatz mehr.
Der Buff bei Sekunde 60 ist weniger wert als der bei Sekunde 0, aber mehr als keiner.

**Bewertung: Erweiterung mit Bedingung.** Die Gruppenprüfung ist der Feature-Toggle, den die
Projektregel für eine nicht nachweisbare Verhaltensänderung verlangt — nur ist der Schalter hier
nicht der Nutzer, sondern die Lage, und das ist die bessere Lösung: Sie schaltet genau dann, wenn die
Voraussetzung tatsächlich vorliegt.

### V3 — Zündung ganz von der Beschwörung lösen

**Mechanismus:** Zünden, sobald kein Buff steht und die eigene Wiederholzeit frei ist.

**Konsequenzen:** Höchste Abdeckung, aber die Bindung an ein eigenes Schadensfenster fällt ganz weg.
Der Beschwörer zündet dann möglicherweise in einer Phase ohne Ziel, kurz vor einem Phasenwechsel oder
während die Gruppe nichts angreift. Der Buff verpufft, die Ladung ist für 120 Sekunden weg.

**Bewertung: ablehnen.** Der Gewinn ist unbelegt, der Verlust benennbar.

### Nullvariante

Für einen Beschwörer richtig und die Empfehlung. Ab zwei nicht mehr tragfähig, weil beide Verluste
dann vollständig greifen und zwei Beschwörer in einer Gruppe gewöhnlich sind.

## Empfehlung

**V1 und V2 umsetzen, V3 nicht.** Beide zusammen, weil sie einander verstärken und sich in getrennten
Wirkungsbereichen bewegen; beide unter derselben Gruppenprüfung nachvollziehbar, weil beide nur ab
zwei Beschwörern etwas ändern.

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

Nicht entschieden und nur im Spiel zu klären: welcher Ausgang bei gleichzeitiger Zündung eintritt;
wie groß der Wertunterschied zwischen einem Buff im Zwei-Minuten-Takt und einem bei Sekunde 60
tatsächlich ist; und wie stark der Versatz in einem echten Kampf ausfällt. Die Rechnungen in diesem
Dokument sind Abschätzungen aus den oben genannten Größen, keine Messungen.
