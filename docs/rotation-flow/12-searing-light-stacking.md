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
| **V4** | Die Bindung an die Beschwörung ganz lösen, Zündung bei Kampf und vorhandenem Ziel | gemessen, nie die beste Wahl — nicht umsetzen |
| **V7** | Zusätzlich außerhalb eines Fensters zünden, sobald der Buff vollständig abgelaufen ist — ohne jede Buchführung über andere | **erreicht das Schadensoptimum auf einen Prozentpunkt genau, zustandsfrei** — umsetzen |
| V5 / V6 | dasselbe, aber mit Buch über die Wiederholzeiten der anderen | gemessen wirkungslos, außerhalb des Bereichs sogar schädlich — verworfen |

**Der maßgebliche Bereich ist eins bis fünf.** Eine reguläre Achtergruppe trägt vier bis fünf
Schadensklassen, eine Vierergruppe zwei. Sechs und mehr Beschwörer sind Sondergruppen außerhalb des
regulären Spiels; sie bleiben im Modell abrufbar (`--all`), bestimmen aber keine Entscheidung.

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

## Die Fälle von einem bis fünf Beschwörern, gemessen

Die Prozentzahlen früherer Fassungen waren Kopfrechnungen, und eine davon war falsch. Sie stammen
jetzt aus `.github/scripts/audit/searing_light_coverage.py`, das die Regeln durchrechnet statt ihr
Ergebnis abzuschätzen: 20 s Wirkung, 120 s Wiederholzeit ab Zündung, Überschreiben statt Stapeln,
Beschwörungen 15 s alle 60 s in der Reihenfolge Solar, Bahamut, Solar, Phoenix, und die Sperre, die
fünf Sekunden vor Buff-Ende öffnet. Gemessen wird der Anteil der Kampfzeit mit laufendem Buff.

Gemessen wird bis fünf Beschwörer, weil dort die reguläre Gruppe endet. Die Spalte „Obergrenze" ist
das, was die Ladungen überhaupt hergeben: *n* × 20 s je 120 s.

**Synchrone Rotationen — der saubere Pull:**

| Beschwörer | heute | V2 | V4 | **V7** | Obergrenze |
|---|---|---|---|---|---|
| 1 | 17 % | 17 % | 17 % | 17 % | 17 % |
| 2 | 17 % | 33 % | 29 % | **33 %** | 33 % |
| 3 | 17 % | 33 % | 42 % | **50 %** | 50 % |
| 4 | 17 % | 33 % | 54 % | **66 %** | 67 % |
| 5 | 17 % | 33 % | 67 % | **83 %** | 83 % |

**Voll auseinandergelaufene Rotationen:**

| Beschwörer | heute | V2 | V4 | V7 | Obergrenze |
|---|---|---|---|---|---|
| 1 | 17 % | 17 % | 17 % | 17 % | 17 % |
| 2 | 33 % | 33 % | 33 % | 33 % | 33 % |
| 3 | 50 % | 50 % | 50 % | 50 % | 50 % |
| 4 | 66 % | 66 % | 66 % | 66 % | 67 % |
| 5 | 67 % | 67 % | 67 % | 67 % | 83 % |

**Das auffälligste Ergebnis steht in der ersten Tabelle: V7 trifft die Obergrenze auf den Punkt.**
Bei einem bis fünf Beschwörern holt die Regel aus den vorhandenen Ladungen heraus, was überhaupt
darin steckt. Mehr ist nicht möglich, ohne dass jemand zusätzliche Ladungen bekäme.

**Und das zweitauffälligste: Versatz hilft nur dem heutigen Code.** In der zweiten Tabelle liegen
alle vier Regeln gleichauf. Wo die Beschwörungsfenster ohnehin gestreut sind, trifft schon die enge
Regel die Lücken; die Erweiterungen finden nichts mehr vor. Umgekehrt heißt das: **V7 ist genau dort
stark, wo der heutige Code schwach ist** — beim sauberen, synchronen Pull, also dem Regelfall zu
Beginn eines Kampfes.

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

**Mehr Beschwörer heißt nicht immer mehr Abdeckung.** Außerhalb des regulären Bereichs zeigt sich
das deutlich: Bei halbem Versatz liefert V2 mit sieben Beschwörern 84 % und mit acht 83 %, und V5
bricht bei sieben von 99 % auf 60 % ein. Das ist kein Rechenfehler, sondern gierige Zuteilung — wer
zuerst in einem Fenster steht, zündet, und kann jemandem zuvorkommen, dessen Wiederholzeit eine
spätere Lücke gedeckt hätte. Für die Entscheidung ist das ohne Belang, weil sechs und mehr
Beschwörer keine reguläre Gruppe sind; für den Selbsttest des Skripts ist es entscheidend, der diese
Eigenschaft deshalb ausdrücklich **nicht** prüft. Eine frühere Fassung behauptete sie und war
widerlegt.

## Ausweichen statt Lockern

Der Auftraggeber hat die Erweiterung genauer gefasst, als sie hier zunächst stand: Nicht „zünde in
jedem Beschwörungsfenster", sondern „**weiche auf Bahamut oder Phoenix aus, falls Solar Bahamut
bereits durch einen anderen abgedeckt war**". Das ist nicht dasselbe, und der Unterschied ist zu
benennen.

**Im Code gibt es weder das eine noch das andere.** `SMN_Reborn.cs:205` ist die einzige Zündstelle,
und `burstInSolar` (`:203`) lässt ab Stufe 100 ausschließlich Solar zu. Es gibt keinen Zweig, der
ausweicht, und keinen Zustand, der eine Blockade festhält.

**Wo beide Fassungen dasselbe tun:** Wenn das Solar-Fenster durch einen fremden Buff gesperrt war,
zündet die pauschale Fassung im nächsten Demi — genau das, was die Ausweichfassung beabsichtigt. Für
den Kollisionsfall, um den es geht, sind sie deckungsgleich.

**Wo sie auseinandergehen:** Wird die eigene Wiederholzeit frei, während Bahamut oder Phoenix steht,
und lag gar keine Kollision vor — etwa weil der zweite Beschwörer tot ist, von Hand spielt oder die
Aktion abgeschaltet hat —, dann zündet die pauschale Fassung dort und verliert die Bündelung mit dem
stärksten eigenen Fenster. Die Ausweichfassung täte das nicht.

**Der Preis der genaueren Fassung ist Zustand.** „Ich wurde blockiert" lässt sich im Moment der
Blockade feststellen, aber nicht mehr, wenn der fremde Buff abgelaufen und das Solar-Fenster vorbei
ist. Es braucht einen Vermerk über Frames hinweg — dieselbe Art Gedächtnis, die V5 ohnehin mitbringt.

**Daraus folgt die Aufteilung der beiden Stufen:** In Stufe 1 ist die Gruppenprüfung die robuste
Näherung — sie schaltet die Erweiterung nur, wenn überhaupt ein zweiter Beschwörer da ist, und nimmt
den seltenen Fall in Kauf, dass dieser gerade nichts beiträgt. In Stufe 2, wo der Zustand für V5
ohnehin geführt wird, kann die Bedingung auf die genaue Fassung verschärft werden: ausweichen nur
nach tatsächlicher Blockade.

**Nicht gemessen, und der Grund ist zu nennen:** Der Unterschied zeigt sich nur bei ungünstig
liegender Wiederholzeit, und das Modell startet alle Beschwörer mit freier Wiederholzeit. Es kann
diesen Fall gar nicht erzeugen. Die Aussage oben ist damit aus der Regel abgeleitet, nicht gemessen.

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

## Die Lücke füllen, ohne Buch zu führen

Zwischen zwei Beschwörungsfenstern liegen bei synchronem Pull vierzig Sekunden, in denen kein Buff
läuft und niemand zünden darf. V7 schließt sie mit einer einzigen zusätzlichen Erlaubnis: **Zünde
außerhalb eines Fensters, sobald der laufende Buff vollständig abgelaufen ist** — die eigene
Wiederholzeit vorausgesetzt, die ohnehin geprüft wird.

**Zwei Bedingungen, und die zweite stammt aus der Messung, nicht aus der Überlegung.** Die Erlaubnis
verlangt einen **vollständig** abgelaufenen Buff, nicht bloß einen, dessen Sperre sich gelöst hat.
Die Sperre öffnet fünf Sekunden vor Ablauf, damit eine Auffrischung im Fenster möglich ist; außerhalb
eines Fensters ist dasselbe Verschwendung — eine ganze Ladung für wenige Sekunden Nettogewinn. Eine
Fassung ohne diese Bedingung wurde gemessen und fiel bei zwei Beschwörern **unter** V2.

### Die Buchführung, die nichts bringt

Der naheliegende Zusatz wäre, die Wiederholzeiten der anderen mitzuschreiben. Die Information ist
verfügbar: `IStatus.SourceId` benennt den Urheber, `StatusHelper.PlayerGetStatus` (`:1529`) liest ihn
bereits. Ab der ersten beobachteten Zündung eines Beschwörers steht fest, wann er frühestens
wiederkehren kann. Die Regel wäre dann: außerhalb eines Fensters nur zünden, wenn kein anderer
bekannter Beschwörer die Lücke decken könnte.

**Gemessen bringt das nichts.** Im gesamten maßgeblichen Bereich, bei jeder Versatzstufe, liefert die
Fassung mit Buchführung dieselben Werte wie die ohne — der Selbsttest des Modells hält das als
Invariante fest, damit es nicht unbemerkt aufhört zu gelten.

**Der Grund ist einfach und war zu übersehen:** Ein anderer Beschwörer kann ohnehin nicht zünden,
bevor seine eigene Wiederholzeit frei ist. Es gibt also nichts, worauf zurückzustehen wäre. Kann er,
dann zündet er — und die vorhandene Sperre verhindert, dass die eigene Ladung dabei verschwendet
wird. Kann er nicht, muss man selbst. Die Frage „könnte jemand anders" ist für die eigene Entscheidung
belanglos, weil beide Antworten zum selben Verhalten führen.

**Außerhalb des maßgeblichen Bereichs ist die Buchführung sogar schädlich.** Bei sieben und acht
Beschwörern erreicht die Fassung ohne Buch 100 %, die mit Buch 46 %. Das Zurückstehen führt dort in
eine ungünstige Selbstorganisation: Mehrere halten sich für denselben Kandidaten zurück, der dann
seinerseits gebunden ist.

**Und eine Erweiterung um ein Verfallsdatum** — wer überfällig ist, zählt nicht mehr — heilt nur den
Schaden, den die Buchführung selbst anrichtet. Gegen einen dreiminütigen Ausfall gemessen liefert sie
genau dasselbe wie die Fassung, die nie Buch geführt hat: 11, 22, 44, 66 Prozent bei zwei bis fünf
Beschwörern. Wer nichts aufschreibt, hat auch nichts zu vergessen.

### Was damit entfällt

Kein Zustand über Frames hinweg, keine Beobachtung fremder Statusquellen, keine Rücksetzpunkte bei
Kampf-, Gruppen- oder Zonenwechsel, kein Verfallsdatum. V7 ist zwei Bedingungen in derselben
Methode, in der heute `burstInSolar` steht. Damit entfällt auch der Einwand aus der Reichweite: Es
wird nichts beobachtet, was ausbleiben könnte.

## Richtlinien nach Lage — geprüft und nicht nötig

Der Auftraggeber hat gefragt, ob das Konzept dynamischer zu bauen wäre, mit unterschiedlichen
Richtlinien bei Abweichungen im Verlauf. Die Antwort fällt nach der Messung anders aus, als sie
zunächst ausfiel: **Die Dynamik ist bereits da, und zwar an der richtigen Stelle.**

Der laufende Buff ist selbst die Rückmeldung. Ob er steht, sagt alles, was für die eigene
Entscheidung zählt — ob jemand anders gerade gedeckt hat, ob eine Lücke offen ist, ob die eigene
Ladung gebraucht wird. Eine Regelung, die diesen einen beobachteten Wert auswertet, braucht kein
Modell der anderen Teilnehmer. Genau das tut V7, und deshalb schlägt es die Fassungen, die sich ein
solches Modell aufbauen.

**Weitergehende Richtlinien nach Lage sind deshalb nicht vorgeschlagen.** Ein Umschalten nach
gemessener Abdeckung oder erkanntem Versatz wäre ein Zustandsautomat — und die Projektregel warnt zu
Recht vor Automaten, die sich statisch nicht absichern lassen; der Eintrag zur doppelten
Zustandswahl in `TODO.md` ist genau daran hängengeblieben. Der Fall hier zeigt zusätzlich, dass mehr
Zustand nicht mehr Wirkung bedeutet: Die aufwendigste der geprüften Fassungen war die schlechteste.

## Abdeckung ist nicht Schaden — die Gegenprobe

Alle Zahlen bis hierher zählen Sekunden mit Buff. Das Ziel ist aber Schaden, und der fällt im
Zwei-Minuten-Zyklus nicht gleichmäßig: Raidverstärkungen und Abklingzeiten der ganzen Gruppe sind
auf das Burst-Fenster gebündelt, sodass dort ein Anteil des Schadens liegt, der weit über dem Anteil
an der Zeit liegt. Eine Regel, die viel Abdeckung an den falschen Stellen erzeugt, wäre deshalb
möglicherweise schlechter als eine, die wenig Abdeckung genau im Burst liefert.

**Gemessen ist sie es nicht.** Das Modell gewichtet jede Sekunde mit der Schadensdichte und zeigt den
Anteil des **Schadens**, der unter einem Buff fällt. Wie hoch der Burst-Anteil tatsächlich ist, hängt
von der Gruppenzusammensetzung ab und ist aus diesem Repository nicht zu klären — deshalb steht er
als Parameter, und die Tabelle zeigt drei Werte. Die Spalte „bestmöglich" ist die optimale
Platzierung derselben Ladungen: erst das Burst-Fenster, dann der Rest.

**Burst-Anteil 30 % (die mittlere der geprüften Annahmen):**

| Beschwörer | heute | V2 | **V7** | bestmöglich |
|---|---|---|---|---|
| 1 | 30 % | 30 % | 30 % | 30 % |
| 2 | 30 % | 44 % | **44 %** | 44 % |
| 3 | 30 % | 44 % | **58 %** | 58 % |
| 4 | 30 % | 44 % | **71 %** | 72 % |
| 5 | 30 % | 44 % | **85 %** | 86 % |

**Burst-Anteil 45 % — die für V7 ungünstigste der geprüften Annahmen**, weil sie den einen gut
platzierten Buff des heutigen Codes am stärksten aufwertet:

| Beschwörer | heute | V2 | **V7** | bestmöglich |
|---|---|---|---|---|
| 1 | 45 % | 45 % | 45 % | 45 % |
| 2 | 45 % | 55 % | **55 %** | 56 % |
| 3 | 45 % | 56 % | **66 %** | 67 % |
| 4 | 45 % | 56 % | **77 %** | 78 % |
| 5 | 45 % | 56 % | **88 %** | 89 % |

**Das Ergebnis ist in allen drei Annahmen dasselbe: V7 liegt innerhalb eines Prozentpunkts am
Optimum.** Bei zwei bis fünf Beschwörern ist das Schadensoptimum damit erreicht, nicht bloß
angenähert. Der Grund, warum die Bündelung nicht gewinnt: V7 gibt das Burst-Fenster nicht auf. Der
erste Zünder steht in seinem Solar-Fenster, das mit dem Burst der Gruppe zusammenfällt; die übrigen
füllen nur die Zeit danach. Und wer den Burst gedeckt hat, ist genau 120 Sekunden später — zum
nächsten Burst — wieder bereit.

**Der heutige Code verschenkt umso mehr, je stärker gebündelt wird.** Bei 45 % Burst-Anteil und fünf
Beschwörern deckt er 45 % des Schadens ab, möglich wären 89 %. Die Hälfte des Erreichbaren bleibt
liegen.

### Die Regression, die eine Gesamtzahl verdecken würde

Der Auftraggeber hat den Einwand geschärft: Ein Buff im Burst steigert einen Anteil des
Burst-Schadens, ein Buff in der Zwischenphase denselben Anteil eines viel kleineren Schadens. Wandert
Buffzeit aus dem Burst in die Zwischenphase, ist das eine **Regression** — und eine gewichtete
Gesamtzahl kann sie verstecken, weil der Zugewinn in der Zwischenphase den Verlust im Burst
rechnerisch ausgleicht.

**Deshalb wird die Burst-Abdeckung getrennt gemessen**, und das Ergebnis ist eindeutig: Bei jeder
Regel, jeder Beschwörerzahl und jeder Versatzstufe bleibt sie bei 99 bis 100 Prozent. **Es wandert
nichts aus dem Burst heraus.**

Der Grund steckt in der Taktung: Wer den Burst gedeckt hat, ist genau 120 Sekunden später wieder
bereit — zum nächsten Burst. Die Ladung, die den Burst deckt, bleibt also dauerhaft an den Burst
gebunden, und nur die **übrigen** Ladungen füllen die Zwischenzeit. V7 fügt Abdeckung hinzu, ohne
bestehende zu verschieben.

**Damit gilt die Folgerung des Auftraggebers: Sekunden zählen hier genauso wie Schaden.** Die
Gleichsetzung ist erlaubt, solange der Burst gedeckt bleibt — und genau das ist jetzt geprüft, statt
angenommen. Die Prüfung steht als Invariante im Selbsttest des Modells: Keine Erweiterung darf die
Burst-Abdeckung senken.

**Zwei Befunde aus dieser Messung selbst, beide festgehalten:**

Die erste Fassung der Burst-Messung lieferte durchgehend 0 % — ein `continue` stand vor der
Zündlogik, also zündete niemand. Die Vergleichsprüfung schlug trotzdem nicht an, weil 0 nicht
kleiner ist als 0. Ein Test, der nur zwei Zahlen ins Verhältnis setzt, merkt nicht, dass beide kaputt
sind; der Selbsttest verlangt jetzt zusätzlich, dass ein einzelner Beschwörer seinen eigenen Burst
tatsächlich deckt.

Die zweite Fassung zeigte einen Rückgang von 0,8 Prozentpunkten bei zwei Beschwörern — in genau der
Richtung, vor der der Einwand warnt. Nachgemessen mit zehnfach feinerem Zeitraster schrumpft er auf
0,14: Er skaliert mit der Rasterweite und ist damit Diskretisierung, kein Verlust. Die Toleranz der
Prüfung ist entsprechend begründet gesetzt, nicht aufgeweitet, bis es passt.

**Grenzen dieser Gegenprobe, und eine davon wirkt zugunsten von V7.** Der Burst-Anteil ist eine
Annahme. Der Schaden außerhalb des Bursts ist als gleichmäßig modelliert, was er nicht ist — die
Beschwörungsfenster der einzelnen Beschwörer sind selbst Spitzen. Da V7 gerade diese Fenster mit
abdeckt, wird sein Vorsprung eher unterschätzt als überschätzt. Nicht modelliert sind außerdem
Phasen ohne Ziel und Unterbrechungen des Schadens überhaupt.

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
einander, ohne sich zu widersprechen. Für V7 gilt dasselbe in stärkerem Maß: Je höher die Abdeckung,
desto öfter greift V1.

**V7 enthält V2.** Es ist als „V2 plus eine zusätzliche Erlaubnis" gebaut und im Modell auch so
gemessen. Beide sind deshalb nacheinander umsetzbar und einzeln prüfbar — oder in einem Schritt,
weil V7 ohne Zustandshaltung auskommt und damit nicht aufwendiger ist als V2 allein.

**Was V2 allein nicht löst.** Der synchrone Pull bleibt mit V2 bei 33 % gedeckelt, weil es dort nur
zwei Beschwörungsfenster je 120 Sekunden gibt. Ab drei Beschwörern liegt diese Decke unter der
Obergrenze der Ladungen — bei fünf Beschwörern 33 % gegenüber möglichen 83 %. Diese Lücke schließt
im maßgeblichen Bereich allein V7, und zwar vollständig.

Eine frühere Fassung behauptete an dieser Stelle, die Abdeckung bleibe auch mit V2 generell bei 33 %.
Das gilt nur synchron; bei auseinandergelaufenen Rotationen erreicht V2 deutlich mehr. Zurückgenommen
als `AUDIT_LOG.md` C41.

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

### V7 — Außerhalb eines Fensters zünden, wenn der Buff abgelaufen ist

**Kontext und Mechanismus:** oben, Abschnitt „Die Lücke füllen, ohne Buch zu führen". Zwei
Bedingungen zusätzlich zu V2: vollständig abgelaufener Buff, eigene Wiederholzeit frei.

**Konsequenzen, gemessen:** Bei einem Beschwörer unverändert. Bei drei bis fünf 50, 66 und 83 Prozent
statt 33 — die Obergrenze dessen, was die Ladungen hergeben. Bei einem dreiminütigen Ausfall eines
Beschwörers dieselben Werte wie die aufwendigste geprüfte Fassung.

**Bewertung: umsetzen, zusammen mit V2 und unter derselben Gruppenprüfung.** Zustandsfrei, damit
ohne Rücksetzpunkte und ohne die Reichweitenabhängigkeit, die jede Beobachtungsfassung mitbringt.

### V5 und V6 — verworfen, obwohl sie richtig gedacht waren

**Mechanismus:** V5 schreibt mit, wann jeder beobachtete Beschwörer frühestens wiederkehren kann
(`IStatus.SourceId` benennt den Urheber), und stellt außerhalb eines Fensters zurück, wenn ein
anderer die Lücke decken könnte. V6 ergänzt ein Verfallsdatum für Beobachtungen, die überfällig sind.

**Warum sie verworfen sind:** Ein anderer Beschwörer kann ohnehin nicht zünden, bevor seine eigene
Wiederholzeit frei ist — es gibt nichts, worauf zurückzustehen wäre, und gegen die Verschwendung
steht bereits die Sperre. Gemessen liefern beide im maßgeblichen Bereich exakt dieselben Werte wie
V7 ohne jede Buchführung; außerhalb davon liegt V5 bei sieben und acht Beschwörern mit 46 % weit
unter den 100 % von V7, weil das Zurückstehen in eine ungünstige Selbstorganisation führt. Das
Verfallsdatum von V6 heilt nur den Schaden, den die Buchführung selbst anrichtet.

**Der Gedanke war richtig, die Voraussetzung nicht.** Die Information ist tatsächlich verfügbar und
ableitbar; sie beantwortet nur keine Frage, die für die eigene Entscheidung zählt.

### Nullvariante

Für einen Beschwörer richtig und die Empfehlung. Ab zwei nicht mehr tragfähig, weil beide Verluste
dann vollständig greifen und zwei Beschwörer in einer Gruppe gewöhnlich sind.

## Empfehlung

**V1 und V7 umsetzen, beide unter der Gruppenprüfung. V4, V5 und V6 nicht.**

| | Gewinn im maßgeblichen Bereich | Preis |
|---|---|---|
| **V1** | Aetherflow-Ausgaben liegen ab zwei Beschwörern im laufenden Fenster statt daneben | eine zusätzliche Eigenschaft, keine Zustandshaltung |
| **V7** | bei drei bis fünf Beschwörern 50 / 66 / 83 % statt 33 % — die Obergrenze; bei zwei 33 % statt 17 % | zwei Bedingungen in derselben Methode, keine Zustandshaltung |

**Beide sind zustandsfrei.** Damit entfallen die Gründe, die eine Aufteilung in zwei Stufen sinnvoll
gemacht hätten: Es gibt keine Beobachtungsmechanik, die für sich zu erproben wäre. Wer will, kann
trotzdem zuerst nur die Fenstererweiterung (V2) setzen und die zweite Bedingung nachziehen — V7 ist
so gebaut.

**Die Gruppenprüfung bleibt der Schalter.** Bei einem einzelnen Beschwörer ändert sich nichts, und
das ist gemessen und nicht bloß beabsichtigt: Das Modell weist für einen Beschwörer in allen
Varianten 17 % aus. In einer zweiten Ausbaustufe ließe sich die Bedingung auf die genauere Fassung
verschärfen — ausweichen nur nach tatsächlicher Blockade —, was allerdings den Zustand verlangt, den
V7 gerade vermeidet.

**Noch nicht umgesetzt.** Der Zweig `claude/raise-swiftcast-weave-2` trägt Eingriffe am
Wiederbelebungspfad, deren Nachweis teilweise noch offen ist; sachfremde Änderungen daneben würden
die Zuordnung erschweren. Die Umsetzung gehört auf einen eigenen Zweig, nach Freigabe.

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
- Der Versatz bleibt über den ganzen Lauf **fest**. Real wächst er mit der Kampfdauer, weil jede
  Mechanik und jeder Tod die Zyklen weiter gegeneinander verschiebt. Deshalb kann das Modell die
  Frage nach dem Einpendeln über zwanzig oder vierzig Minuten nur zur Hälfte beantworten: Es zeigt,
  dass keine der Regeln eine Anlaufzeit braucht, aber nicht, wie eine Gruppe im Lauf eines langen
  Kampfes von der synchronen in die versetzte Tabelle wandert. Die Richtung dieser Wanderung steht
  fest — sie verbessert die Lage —, ihr Tempo nicht.

Nicht entschieden und nur im Spiel zu klären: welcher Ausgang bei gleichzeitiger Zündung eintritt;
wie groß der Wertunterschied zwischen einem Buff im Zwei-Minuten-Takt und einem daneben tatsächlich
ist; und wie stark der Versatz in einem echten Kampf ausfällt.
