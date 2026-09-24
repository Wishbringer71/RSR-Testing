# Searing Light bei mehreren Beschwörern

*Zum Namen: Der Auftraggeber nennt die Aktion „Gleißender Schein". Die Zuordnung zu **Searing Light**
ist belegt und in `.github/scripts/audit/action_names_de.json` geführt; Quelle ist seine eigene
Angabe. Das Dokument benutzt den englischen Bezeichner, weil der Code ihn trägt.*

## Vorgabe des Auftraggebers: eine Regel, die sich der Lage anpasst

**Die Regel hat nicht fest zu sein, sondern sich optimal auf die bestehende Situation
auszurichten.** Sie darf dabei auf Annahmen aufbauen und diese Schritt für Schritt korrigieren,
sobald die Beobachtung ihnen widerspricht:

1. **Annahme: Ich bin der Erste.** Der Kampf beginnt mit Solar Bahamut, und Searing Light wird dort
   gezündet.
2. **Kommt mir ein anderer Beschwörer zuvor**, halte ich meine Ladung für diese Solar-Phase zurück —
   ein zweiter Buff auf einen laufenden ersten wäre doppelt gezündet und damit verschwendet. Das
   Zurückhalten gilt nur für diese Phase, nicht grundsätzlich.
3. **Nächster Versuch in der nächsten anstehenden Burstphase** (Bahamut oder Phoenix), wieder unter
   der Annahme, dass ich dort der Erste bin.
4. **Kommt mir auch dort jemand zuvor**, rückt der Versuch auf die darauffolgende Burstphase weiter —
   die übriggebliebene von Phoenix beziehungsweise Bahamut, erneut unter derselben Annahme.
5. **Dabei ist zu beachten**, dass der Beschwörer, der beim ersten Solar Bahamut gezündet hat, beim
   nächsten Solar Bahamut wieder dran sein kann. Auch hier gilt zunächst die Annahme, dass ich als
   Erster zünde.
6. **Trifft auch das nicht zu** und er kommt erneut zuvor, ist von der Wahrscheinlichkeit auszugehen,
   dass es bei den übrigen ebenso läuft — bei vier Beschwörern in der Gruppe also durchgehend. Dann
   gehört das eigene Searing Light in eine **Zwischenphase**, und zwar in die, in der der eigene
   Schaden am größten ist: Ifrit oder Titan.

**Die Bauform ist ein hybrides, dynamisches Modell:** eine Regel, die sich an die Lage anpasst und
bei Bedarf zwischen Strategien **wechselt**, statt einer festen Wahl zu folgen. Die Messung stützt
das — keine der geprüften Einzelregeln gewinnt überall: Bei auseinandergelaufenen Rotationen ist die
heutige, enge Regel bereits optimal, weil jeder Beschwörer seinen Buff in seiner stärksten Phase
setzt und die Streuung die Lücken von selbst schließt; bei synchronem Pull mit fremden Beschwörern
gewinnt die Buchführung; folgen alle derselben Regel, gewinnt das reine Lückenfüllen. Eine feste
Regel muss daher in mindestens einer dieser Lagen unterlegen sein.

**Die Buchführung ist genau dafür da.** Sie beantwortet die Frage, in welcher Lage man sich befindet:
Wer belegt welche Phase, und tut er es wiederholt? Erst aus der Wiederholung folgt, dass eine Phase
dauerhaft vergeben ist — ein einmaliges Zuvorkommen ist Zufall und darf keine Phase kosten.

**Gruppengrößen, auf die die Anpassung auszurichten ist:** In einer Vierergruppe sind regulär bis zu
zwei Beschwörer möglich, in einer Achtergruppe bis zu fünf. Eine Vierergruppe aus vier und eine
Achtergruppe aus acht Beschwörern sind nicht auszuschließen, haben bei der Optimierung aber
nachrangig zu gelten: **Vorrang haben die regulären Gruppen.**

**Was damit widerlegt ist:** Die Annahme, ab zwei Beschwörern fielen die Gelegenheiten ohnehin
zusammen. Spielstile und RSR-Einstellungen unterscheiden sich, und schon der Zeitpunkt des
Kampfeintritts streut die Zyklen. Der synchrone Fall ist ein Randfall, kein Regelfall — und eine
Regel, die nur ihn behandelt, behandelt den seltensten Fall.

## Sachstand

**Bei einem Beschwörer fällt Searing Light vor der großen Beschwörung, und die Beschwörung wartet
unter Bedingungen auf den Buff. Ob sie überhaupt warten soll, ist offen und als Entscheidung in
`TODO.md` vorgelegt.** Stand in `SMN_Reborn`:

1. **Zündung.** Im Einschiebeplatz **vor** der großen Beschwörung, sobald deren Abklingzeit bis zum
   nächsten GCD abläuft (`burstAboutToStart`, A114); sonst in der Phase selbst (`burstInSolar`).
   Freigegeben wird „bis zum nächsten GCD bereit", nicht „abgelaufen" (A126).
2. **Warten der Beschwörung** (`searingSettled`, A115, A127). Sie wartet, wenn der Buff bereit ist oder
   so früh zurückkehrt, dass er noch in den Einschiebeplatz des GCDs passt, den das Warten kostet —
   einen GCD ab jetzt, abzüglich des Vorlaufs, mit dem jedes Einschiebefenster schließt. Sie wartet
   nicht bei laufendem Buff gleich welcher Herkunft, bei ausgeschaltetem Burst oder ausgeschaltetem
   Searing Light, unterhalb von Stufe 66, und mit einem zweiten Beschwörer in der Gruppe nur auf einen
   bereits bereiten Buff.

**Vorgabe des Auftraggebers (A115):** „Somit muss ja searing light aktiv sein, bevor der erste
burstschaden entsteht."

**Wann der erste Burstschaden entsteht, ist am Wirktext belegt — und es ist nicht die Beschwörung.**
`Summon Solar Bahamut` (`ActionId.resx`, 36992) nennt **keine Potenz**: „Enters Lightwyrm Trance and summons
Solar Bahamut to fight your target. Solar Bahamut will execute Luxwave automatically on the targets
attacked by you after summoning.“ Der Beschwörungs-GCD richtet also nichts aus; Luxwave (160) folgt den
**eigenen** Angriffen. Der erste Schaden der Phase ist damit der erste GCD danach — Umbral Impulse (640)
samt automatischem Luxwave, zusammen 800 Potenz.

**Jedes Warten der Beschwörung wirkt über den ganzen Kampf.** Die Abklingzeiten der Beschwörung und
des Buffs laufen ab ihrer **Nutzung**. Ein Warte-GCD verschiebt deshalb die Solar-Phase, jede folgende
Demi-Phase und den nächsten Buff um denselben Betrag — nicht gegeneinander, sondern gemeinsam gegen
das Zwei-Minuten-Fenster der übrigen Gruppenbuffs. Wartet sie in jeder Solar-Phase, summiert sich das.
Genau diese Form hat der Auftraggeber als einziger Beschwörer gemeldet: Searing Light rutschte „immer
mehr, je länger der kampf lief" nach hinten, und „cooldown von searing light ist später nicht fertig,
wenn burst phase läuft. das ist die konsequenz."

**Zwei Wege im Code führen zu einem Warte-GCD je Solar-Phase:**

- **Die Freigabe kam zu spät — behoben (A126).** Freigegeben wurde erst bei **abgelaufener**
  Abklingzeit der Beschwörung. Die läuft auf einem GCD-Zeitpunkt ab, wenn sechzig Sekunden eine ganze
  Zahl von GCDs sind; der Platz davor war dann vorbei, und die Beschwörung wartete einen GCD.
- **Vor der Beschwörung fehlt der Einschiebeplatz — offen.** Ist der GCD davor ein Zauber mit
  Wirkzeit, dessen Wiederholzeit kaum darüber liegt, bleibt dahinter kein Platz für eine Fähigkeit:
  `EnoughWeaveTime` verlangt mehr Restzeit als den Vorlauf. Der Buff kann dann nicht vor der
  Beschwörung fallen, sie wartet auf ihn, und ist auch der Warte-GCD ein solcher Zauber, noch einen.
  Der Auftraggeber nennt genau diese Lage: vor der Beschwörung läuft „meist ifrit", also Ruby Rite.
  *Schluss aus Code und Wirktext; die Längen von Wirk- und Wiederholzeit sind Fremdquelle —
  `ModifyRubyRitePvE` liest die Wirkzeit erst zur Laufzeit (`GetCastTime()`).*

**Warten ist nie besser als der Platz hinter der Beschwörung — bis auf einen Fall.** Die Beschwörung
ist ein GCD ohne Schaden. Ihr Einschiebefenster liegt deshalb vor dem ersten Burstschaden, und es ist
dasselbe Fenster, das ein sofort wirkender Warte-GCD anböte. Jeder Buff, den ein Warte-GCD noch
trägt, fällt ebenso in den Platz hinter der Beschwörung — ohne dass eine Demi-Phase sich verschiebt;
ein Warte-GCD mit Wirkzeit trägt ihn gar nicht. Die Vorgabe aus A115 ist damit auch ohne Warten
erfüllt. Was das Warten allein kauft, ist Schutz vor Konkurrenz im Platz **hinter** der Beschwörung:
Ihr Wirktext gewährt Refulgent Lux, damit wird Lux Solaris dort wirkbar, und der Heilzweig kommt vor
dem Angriffszweig. Hinter einem sofort wirkenden GCD passen in der Regel zwei Fähigkeiten; Lux Solaris
nimmt höchstens eine. *Unbelegt: dass die Beschwörung sofort wirkt, und die Zahl der Plätze bei der
jeweiligen Vorlaufeinstellung.* Die Vorlage in `TODO.md` rechnet die Wege durch.

**Auch das Warten innerhalb der Demi ist teurer als sein Ertrag.** Hält man statt der Beschwörung den
ersten GCD der Phase an, bis die offenen Fähigkeiten gewoben sind, verschiebt sich keine Demi — die
Abklingzeit läuft ab der Beschwörung —, aber jeder folgende GCD. Der Zyklus verliert dadurch den
billigsten GCD anteilig: 0,2 bis 0,7 Sekunden kosten 32 bis 112 Potenz, gerechnet mit Ruin III
(400 je 2,5 s). Bei einem GCD unter 2,5 Sekunden kommt ein zweiter Verlust hinzu:
`smn_phase_potency.py` zählt dann sieben statt sechs Würfe in der Phase, der letzte Umbral Impulse
liegt nur 0,12 bis 0,6 Sekunden vor dem Ende der 15 Sekunden, und ein Clip darüber kostet ihn, rund
400 Potenz. Der Ertrag ist höchstens der Buff auf dem ersten Umbral Impulse, 40 Potenz. Die übrigen
Fähigkeiten der Phase — Energy Drain, zweimal Necrotize, Exodus, Sunflare, Searing Flash, Lux
Solaris — brauchen kein Warten: rund acht auf zwölf Einschiebeplätze. *Clipdauer aus Ausführungssperre
und Latenz, beide nicht im Repository, der Code liest sie erst zur Laufzeit.*

**Mehrere Beschwörer — Prüfvorschlag des Auftraggebers, geprüft und so umgesetzt:** „die prüfung der
abklingzeit darf aber nicht dazu führen, dass alle demis verzögert werden (siehe mehrere Beschwörer in
gruppe), da erfolgt ein ausweichen auf den nächsten demi bzw. im negativfall auf den stärksten
primal." Mit einem zweiten Beschwörer wartet die Beschwörung auf keinen abkühlenden Buff; die Ladung
geht, sobald sie zurück ist, in das erste Fenster, das die Zündregel oben öffnet — jede große
Beschwörung, und sind alle belegt, der stärkste Primal-Block nach Standort.

**Gewartet wird nur auf einen Buff, der vor der Beschwörung auch fallen kann.** Drei Fälle hielten die
Beschwörung ohne Ende fest und sind geschlossen:

- **Ein fremdes Searing Light läuft.** Der Buff stapelt nicht, und der eigene lässt sich über einen
  laufenden nicht wirken (`StatusProvide` ohne Eigenbindung). Gefragt wurde nur nach dem **eigenen**:
  Der war bereit, konnte nicht gehen, und die Beschwörung wartete, bis der fremde auslief — in jeder
  Demi-Phase. Jetzt genügt ein laufender Buff gleich welcher Herkunft.
- **Burst ausgeschaltet.** Den Platz vor der Beschwörung öffnet nur der Burstzweig; bei ausgeschaltetem
  Burst fiel der Buff dort nie.
- **Searing Light ausgeschaltet.** Ein nie gezündeter Buff geht nie auf Abklingzeit.

**Gelesen wird die Bereitschaft der Beschwörung, nicht der nächste GCD.** Andernfalls entstünde dasselbe
Henne-Ei-Problem wie bei der Wiederbelebung (Konzept 11): Der Buff wartete darauf, angekündigt zu
werden, und die Ankündigung auf den Buff.

**Das verbleibende Risiko ist benannt, nicht beseitigt — und für diesen Job ist es kleiner, als die
allgemeine Zweigliste vermuten lässt.** „Heilung oder Verteidigung“ heißt beim Beschwörer konkret
Schimmerschild und Addle; seine einzige nennenswerte Heilung ist die Flächenheilung aus einer
laufenden Demi. Und genau die kann **vor** der Beschwörung gar nicht feuern: `ModifyLuxSolarisPvE`
setzt `StatusNeed = [StatusID.RefulgentLux]`, `ModifyRekindlePvE` prüft `InPhoenix` — beide
Bedingungen entstehen **aus** der Phase, die noch nicht begonnen hat. Vor der Beschwörung bleiben
damit Schimmerschild (`ModifyRadiantAegisPvE`, `ActionCheck = () => DataCenter.HasPet()`) und Addle,
und die nur bei gesetzter Verteidigungsflagge. Eine Absicherung über `CanUse` als Prüfung scheidet
aus; das ist die Defektklasse aus `TODO.md`.

**Hinter der Beschwörung kehrt sich das um, und das ist der wahrscheinlichere Grund für die
Spielbeobachtung.** Der Wirktext der Beschwörung gewährt selbst Refulgent Lux („Additional Effect:
Grants Refulgent Lux Duration: 30s“). In dem Augenblick, in dem die Beschwörung aufgeht, wird Lux
Solaris also wirkbar — und `HealAreaAbility` fragt die Kette **vor** `AttackAbility`
(`CustomRotation_Ability.cs:169` und `:188` gegen den Angriffszweig weiter unten), kann den
Einschiebeplatz hinter der Beschwörung also nehmen, sobald die Flächenheilungsflagge steht. **Die
Beschwörung erzeugt ihren eigenen Konkurrenten um den Platz dahinter; der Platz davor hat diesen
Konkurrenten nicht.** Das ist ein zweites, vom Zeitpunktargument unabhängiges Argument für die
Zündung vor der Beschwörung — und ein Schluss aus Wirktext und Zweigreihenfolge, keine
Spielbeobachtung.

**Keine Sonde, und das ist die Vorgabe des Auftraggebers:** Eine Messung, deren Auswertung über das
Modell läuft, kostet je Wert einen Kampf, ein Ablesen, einen Bericht und eine Runde. Diese
Entscheidung fällt stattdessen im Code, aus dem, was der GCD-Pfad ohnehin schon als nächste Aktion
gewählt hat.

**Wer den Platz nehmen kann, ist sehr wohl bestimmbar — nur nicht, wer es im Einzelfall tut.**
`03-universal.md` führt die Zweigkette des Fähigkeitenpfads: Notfall, Unterbrechung, Reinigung,
Rettungsrückgriff, Haltung, Rückstoßschutz, Positionierung, Flächen- und Einzelheilung, Tempo,
Spott, Flächen- und Einzelverteidigung, Bewegung, Trank, Phönixfeder — und **danach** erst der
Angriffszweig, in dem Searing Light an erster Stelle steht. Die offene Frage ist damit kleiner als
zuvor beschrieben: Sie lautet nicht „welcher Zweig“, sondern „wie oft greift einer von ihnen in
genau diesem Fenster“ — und das ist eine Messfrage, keine Lesefrage. Für den Beschwörer schrumpft
sie nach dem Abschnitt oben weiter zusammen: Vor der Beschwörung kommen von dieser ganzen Kette nur
Schimmerschild und Addle überhaupt in Betracht, und auch die nur bei gesetzter Verteidigungsflagge.

**Warum das Schaden kostet, in Zahlen aus dem Wirktext** (`ActionId.resx`, beides dort wörtlich):
Searing Light wirkt **20 Sekunden**, Summon Solar Bahamut dauert **15 Sekunden**. Zu Beginn gezündet
deckt der Buff die ganze Phase ab und läuft fünf Sekunden darüber hinaus — dieser Überhang ist
eingeplant und harmlos. Jede Sekunde Verzug tauscht dagegen eine gebuffte Sekunde **innerhalb** der
Phase gegen eine **danach**: Die Demi-GCDs tragen 947 bis 1217 Potenz, die Zwischenblöcke höchstens
632, und die fünf Prozent wirken auf den jeweils darunterliegenden Wert. Ein Verzug von zwei bis drei
GCDs verschiebt damit rund 30 Potenz je GCD von der starken in die schwache Phase. Der Verlust
trifft zudem nicht ihn allein: Der Buff gilt für die nahen Gruppenmitglieder mit.

Ab **zwei** Beschwörern reicht zusätzlich das genutzte Zündfenster nicht
mehr aus: Jeder Beschwörer darf Searing Light nur während seiner Solar-Bahamut-Beschwörung zünden,
und dieses Fenster kommt nur alle 120 Sekunden — genau so oft wie die Aktion selbst. Treffen zwei
Beschwörer im selben Fenster aufeinander, verfällt eine Ladung.

**Wie oft das eintritt, hängt an der Lage und nicht an der Beschwörerzahl.** Spielstile und
RSR-Einstellungen unterscheiden sich, und schon der Zeitpunkt des Kampfeintritts streut die Zyklen;
der synchrone Pull ist der Randfall, nicht der Regelfall. Deshalb ist die Antwort keine feste Regel,
sondern eine, die die Lage erkennt und zwischen Strategien wechselt.

| | Inhalt | Bewertung |
|---|---|---|
| **V1** | Den Searing Light eines anderen Beschwörers als Buff-Fenster für die eigenen Aetherflow-Ausgaben werten | umsetzen |
| **V2** | Das Zündfenster auf alle großen Beschwörungen erweitern, sobald ein zweiter Beschwörer in der Gruppe ist | umsetzen, mit Gruppenprüfung als Schalter |
| **V8** | Die hybride Regel des Auftraggebers: in jeder freien Burstphase zünden, und erst wenn **alle** Phasenarten dauerhaft belegt sind, in die stärkste Zwischenphase ausweichen | **umsetzen** |
| V4 | Die Bindung an die Beschwörung ganz lösen, Zündung bei Kampf und vorhandenem Ziel | gemessen, nie die beste Wahl — nicht umsetzen |
| V7 | Außerhalb eines Fensters zünden, sobald der Buff abgelaufen ist, ohne jede Buchführung | **zurückbauen** — blind: gewinnt eine Lage, verliert zwei andere |
| V5 / V6 | Buch über die **Wiederholzeiten** der anderen führen | die Frage war falsch gestellt, s. unten — in V8 aufgegangen |

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
nur eines davon ist Solar Bahamut.** Das ist die Ausgangslage, gegen die alle Vorschläge dieses
Dokuments gerechnet sind: Die Zündung hing allein an `burstInSolar` (`SMN_Reborn.cs`), ein
Beschwörer hatte damit genau **eine** Gelegenheit pro Wiederholzeit, und sie lag bei allen
Beschwörern einer synchron gestarteten Gruppe zur selben Zeit. Den heutigen Stand nennt „Die
Umsetzung“ weiter unten.

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

Entscheidend dafür ist `StatusFromSelf = false` in `ModifySearingLightPvE`: `PlayerHasStatus`
(`StatusHelper.cs:1164`, Quellenfilter in `AnyStatusMatches`) filtert nur bei `isFromSelf` auf die eigene Quelle, hier zählt also jeder
fremde Buff. Sein Gegenstück `HasSearingLight` (`SummonerRotation.cs:271`) ruft
`PlayerHasStatus(true, …)` und zählt nur den eigenen — auch das ist für seine ursprüngliche Frage
richtig. Aus dem Zusammentreffen beider entsteht der Befund von V1.

## Was im Kampf ankommt, gemessen

**Das Maß ist der Anteil des eigenen Schadens, der unter einem Buff fällt** — nicht die Zahl der
Sekunden mit Buff. Searing Light hebt den Schaden um 5 %, eine Sekunde ist also wert, was sie
produziert, und eine Solar-Sekunde trägt 1217 Potenz je GCD gegen 495 in einem Primal-Block, Faktor
2,46 (`smn_phase_potency.py`). Sekunden zu zählen beantwortet deshalb eine andere Frage als „wo zahlt
sich die Ladung aus"; die Abdeckungstabellen weiter unten bleiben als Zwischengröße stehen und sind
als solche zu lesen.

**Die Gruppe, die tatsächlich vorkommt, ist die gemischte:** Ein Beschwörer folgt dieser Regel, die
anderen sind fremde Spieler mit eigener Rotation. Lauf vom 13.09.2026, über alle Reihenfolgen
gemittelt, wer bei gleichzeitiger Gelegenheit zuerst zündet:

**V8 ist V2 plus eine einzige weitere Klausel** — Punkt 6 der Vorgabe: außerhalb einer Burstphase
zünden, sobald **alle** Phasenarten dauerhaft belegt sind. Solange irgendeine frei ist, verhält sich
V8 wie V2. Daraus folgt, womit gemessen werden muss: nicht gegen die heutige Regel, sondern gegen V2,
und gegen Gegenspieler, die die Phasen tatsächlich belegen.

**Gegenspieler, die nur Solar nutzen** — Bahamut und Phoenix bleiben frei, die Klausel greift nie:

| Lage | heute | V2 | V7 (gebaut) | V8 |
|---|---|---|---|---|
| synchroner Pull | 26,6 % | 46,4 % | 38,8 % | 46,4 % |
| halb versetzt | 46,3 % | 46,3 % | 46,3 % | 46,3 % |
| voll versetzt | 60,8 % | 60,8 % | 60,8 % | 60,8 % |

**Gegenspieler, die jede Phasenart belegen** — die Lage, für die Punkt 6 gemacht ist:

| Lage | heute | V2 | V7 (gebaut) | **V8** |
|---|---|---|---|---|
| synchroner Pull | 47,9 % | 47,9 % | 60,0 % | **56,4 %** |
| halb versetzt | 46,3 % | 46,3 % | 46,3 % | 46,3 % |
| voll versetzt | 60,8 % | 60,8 % | 60,8 % | 60,8 % |

(drei Beschwörer; über alle Reihenfolgen gemittelt, wer bei gleichzeitiger Gelegenheit zuerst zündet.
Zwei bis fünf Beschwörer liegen innerhalb eines Prozentpunkts, außer bei zwei Beschwörern, wo die
Klausel mangels Belegung nicht greift.)

**Damit steht die Bewertung:** V8 ist nirgends schlechter als V2 und in der einen Lage, für die seine
zusätzliche Klausel gebaut ist, um achteinhalb Prozentpunkte besser. Das ist die Bedingung, die eine
Anpassung erfüllen muss — sie darf keine Lage verschlechtern, um eine andere zu gewinnen.

**Sobald die Rotationen auseinanderlaufen, ist die heutige enge Regel bereits optimal** — jeder
Beschwörer setzt seinen Buff in seine stärkste Phase, und die Streuung schließt die Lücken von
selbst. Alle Unterschiede entstehen im synchronen Fall.

**V7 ist der Ausreißer und deshalb zurückzubauen.** Es liegt im synchronen Pull gegen belegte Phasen
vorn (60,0 % gegen 56,4 %), weil blindes Füllen dort zufällig trifft — und es verliert in zwei
anderen Lagen: gegen Gegenspieler auf Solar fällt es auf 38,8 % gegen 46,4 %, und bei zwei
Beschwörern auf derselben Regel mit voll versetzten Rotationen auf 39,6 % gegen 47,7 % der heutigen
Regel. Es gibt die eigene Burstphase auf, ohne dass eine Kollision vorliegt. Genau das unterscheidet
eine blinde Regel von einer, die die Lage liest.

**Warum die Klausel nicht früher greifen darf, ist die Potenzdichte.** Solar trägt 1217 je GCD,
Bahamut 950, Phoenix 947; eine Zwischenphase im besten Fall 632 und im Mittel 495. Eine ausgelassene
Gelegenheit kostet dabei nichts: Die Ladung bleibt liegen, ihre Wiederholzeit beginnt erst mit der
Zündung, und die nächste Burstphase kommt in höchstens 60 Sekunden. Erst wenn **keine** Burstphase
mehr zu bekommen ist, ist die Zwischenphase besser als gar nichts — und dann ist sie es deutlich.

## Die Fälle von einem bis fünf Beschwörern, Abdeckung in Sekunden

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
Beschwörer keine reguläre Gruppe sind; für den Selbsttest des Skripts ist es entscheidend: Er darf
keine Monotonie zwischen Beschwörerzahl und Abdeckung verlangen, weil sie nicht gilt.

## Ausweichen statt Lockern

Der Auftraggeber hat die Erweiterung eng gefasst: Nicht „zünde in
jedem Beschwörungsfenster", sondern „**weiche auf Bahamut oder Phoenix aus, falls Solar Bahamut
bereits durch einen anderen abgedeckt war**". Das ist nicht dasselbe, und der Unterschied ist zu
benennen.

**Im Code gibt es weder das eine noch das andere.** `SMN_Reborn.cs:240` ist die einzige Zündstelle,
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
benannt: die Zusammensetzung der Gruppe. Welche Zusammensetzungen überhaupt vorkommen und wie sie sich
unterscheiden, ist in `02-groups.md` erhoben; dieses Konzept setzt darauf auf, statt die Frage
ein zweites Mal zu beantworten.

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
`Service.Config.AutoBurst` an ist (`StatusFromCmdOrCondition` in `StateUpdater.cs`). `burstInSolar` prüft das heute nicht. Zwei
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

**Zweitverwendung des Modells — nachgerechnet, und sie trägt nur nach einer Verallgemeinerung.**
`searing_light_coverage.py` beantwortet die Frage „wie viele Sekunden eines Kampfes deckt ein
nicht stapelbarer Effekt ab, wenn n Quellen ihn nach festen Regeln zünden“ — und genau diese Frage
stellt `08-mitigation-synergy.md` bei der Streckung der Drosselung. Zwei Annahmen des Modells
stehen dem aber entgegen:

- **Es kennt nur eine Aktion.** `BUFF` (20 s) und `RECAST` (120 s) sind Konstanten; die Streckung
  hat es mit ungleichen Quellen zu tun — die Sanctus-Betäubung vier Sekunden, die Verlangsamung
  des Rückstoßes fünfzehn, die Minderungen wieder anders.
- **Es rechnet mit Überschreiben, nicht mit Stapeln** (`buff_until = t + BUFF   # overwrite, never
  stack`). Minderungen stapeln dagegen multiplikativ; „Strecken statt stapeln“ ist dort eine
  **Vorgabe des Auftraggebers**, keine Spielmechanik, und ein Modell, das das Stapeln gar nicht
  abbilden kann, kann den Vergleich zwischen beiden Strategien nicht führen.

Was übertragbar bleibt, ist der Kern: die Zeitschritt-Simulation mit Quellen, Dauer, Wiederholzeit
und einer Zündregel, samt der Trennung „Abdeckung ist nicht Schaden“. Eine Zweitverwendung hieße
also, Quellenliste und Stapelverhalten zu Parametern zu machen — kein Zufallstreffer, aber auch
kein bloßes Aufrufen.

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

### Die Buchführung: die Frage war falsch gestellt

Der naheliegende Zusatz wäre, die Wiederholzeiten der anderen mitzuschreiben. Die Information ist
verfügbar: `IStatus.SourceId` benennt den Urheber, `StatusHelper.PlayerHasStatus` (`:1164`) liest ihn
über den Quellenfilter in `AnyStatusMatches` bereits. Ab der ersten beobachteten Zündung eines Beschwörers steht fest, wann er frühestens
wiederkehren kann. Die Regel wäre dann: außerhalb eines Fensters nur zünden, wenn kein anderer
bekannter Beschwörer die Lücke decken könnte.

**Als „wer könnte die nächste Lücke decken" gestellt, bringt die Frage nichts.** Im gesamten
maßgeblichen Bereich liefert diese Fassung dieselben Werte wie die ohne Buch, und der Grund steht
unten: Niemand kann vor seiner eigenen Wiederholzeit zünden, es gibt also nichts, worauf
zurückzustehen wäre.

**Als „wer besetzt dauerhaft welche Phase" gestellt, ist sie der Kern der Regel.** Das ist die
Auskunft, die ein Client wirklich hat: Der Status nennt seine Quelle, und aus den beiden Perioden
folgt, wohin ein fremder Zünder zurückkehrt. Searing Light kommt nach 120 Sekunden wieder, ein
Beschwörungsfenster alle 60 — ein Zünder trifft also stets Fenster **gleicher Parität**. Ein fremder
Cast in einem meiner geraden Fenster heißt: Er nimmt dauerhaft Solar. Einer in einem ungeraden: Er
wechselt zwischen Bahamut und Phoenix, beide sind vergeben. Und ein Cast **zwischen** meinen Fenstern
sagt über meine Phasen gar nichts — er zündet in seiner eigenen, die von meiner weggelaufen ist.

**Belegt gilt eine Phase erst bei Wiederholung.** Ein einmaliges Zuvorkommen ist Zufall; erst wenn
derselbe Beschwörer nach seiner Wiederholzeit erneut dort steht, gehört ihm die Phase. Die frühere
Fassung buchte schon beim ersten Mal und gab dadurch Phasen auf, die niemand hielt — bei fünf
Beschwörern und vollem Versatz kostete das 53 gegen 78 Prozent.

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

**Was die Buchführung in diesem Fall kostet, ist beziffert: die Hälfte.** Ohne Verfallsdatum kommt sie
über denselben Ausfall auf 11, 11, 22, 33 Prozent — bei drei Beschwörern und mehr genau halb so viel
Abdeckung wie die Fassung ohne Buch. Der Vorgang im Kampf ist der, den jeder kennt: Ein Beschwörer
stirbt, wird gerade wiederbelebt oder steht in einer Mechanik. Wer Buch führt, hält seine eigene
Ladung zurück, weil der Ausgefallene rechnerisch „dran" wäre — und der zündet nie. Wer nur fragt, ob
gerade ein Buff läuft, zündet und deckt die Lücke. Damit ist die einfache Regel der aufwendigen nicht
bloß ebenbürtig, sondern im ungünstigen Fall überlegen; der Selbsttest des Modells hält das fest.

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

**Zwei Eigenschaften der Messung tragen dieses Ergebnis, und beide stehen als Invariante im
Selbsttest:**

Ein Vergleich zweier Zahlen merkt nicht, wenn beide kaputt sind. Steht die Burst-Abdeckung
durchgehend auf 0 %, ist 0 nicht kleiner als 0, und die Prüfung bleibt still — genau der Fall, den
ein `continue` vor der Zündlogik erzeugt. Der Selbsttest verlangt deshalb zusätzlich, dass ein
einzelner Beschwörer seinen eigenen Burst tatsächlich deckt.

Die Toleranz des Vergleichs ist begründet gesetzt, nicht aufgeweitet, bis es passt. Der einzige
gemessene Rückgang — 0,8 Prozentpunkte bei zwei Beschwörern, in genau der Richtung, vor der der
Einwand warnt — schrumpft mit zehnfach feinerem Zeitraster auf 0,14. Er skaliert mit der Rasterweite
und ist damit Diskretisierung, kein Verlust.

**Grenzen dieser Gegenprobe, und eine davon wirkt zugunsten von V7.** Der Burst-Anteil ist eine
Annahme. Der Schaden außerhalb des Bursts ist als gleichmäßig modelliert, was er nicht ist — die
Beschwörungsfenster der einzelnen Beschwörer sind selbst Spitzen. Wie hoch diese Spitzen sind, ist im
folgenden Abschnitt beziffert: ein Beschwörungsfenster trägt das Zwei- bis Zweieinhalbfache eines
gleich langen Primalfensters. Da V7 gerade diese Fenster mit abdeckt, wird sein Vorsprung
unterschätzt, nicht überschätzt. Unterbrechungen des Schadens sind nicht modelliert.

**Phasen ohne Ziel waren als Modellgrenze benannt und sind keine.** Sie wären eine, wenn V7 dort
zünden könnte — eine Ladung auf einen Abschnitt ohne Gegner ist reiner Verlust. Der Pfad gibt das
aber nicht her: `AttackAbility` wird in `CustomRotation_Ability.cs:383` nur unter
`HasHostilesInRange` aufgerufen, die Zielprüfung steht also vor jeder Zündung, der heutigen wie der
neuen. Das Modell darf diesen Fall auslassen, weil der Code ihn ausschließt.

## Was eine Phase wert ist

**Ein Solar-Bahamut-Fenster trägt 7300 Potenz, ein Bahamut- oder Phoenix-Fenster 78 Prozent davon und
ein gleich langes Primalfenster 41 Prozent.** Der Zwei-Minuten-Zyklus ist damit nicht annähernd
gleichmäßig: Auf ein Viertel der Zeit entfällt gut die Hälfte des Schadens. Die Zahlen stammen aus
`.github/scripts/audit/smn_phase_potency.py`; Aufbau der Phasen aus der Dispatch-Reihenfolge in
`SMN_Reborn.cs`, Dauer 15 s aus dem Tooltip-Text der drei Beschwörungen in `ActionId.resx`, Einzelziel,
Stufe 100, ohne Searing Light.

| Fenster (15 s) | Potenz | je GCD | gegen Solar | gegen Primalfenster |
|---|---|---|---|---|
| Solar Bahamut | 7300 | 1217 | 100 % | 2,46× |
| Bahamut | 5700 | 950 | 78 % | 1,92× |
| Phoenix | 5680 | 947 | 78 % | 1,91× |
| Primal (Mittel) | 2970 | 495 | 41 % | 1,00× |

**Bahamut und Phoenix sind gleichwertig**, obwohl sie verschieden aussehen: Phoenix' stärkerer Füller
(580 gegen 500) gleicht genau aus, dass seine Astral-Flow-Aktion Rekindle heilt statt zu schaden,
während Bahamut dort Deathflare mit 500 Potenz hat. Der Vorsprung von Solar Bahamut kommt aus drei
Quellen zugleich: stärkerer Füller (640), stärkerer Begleitangriff (Luxwave 160 gegen 150) und vor
allem die beiden Abschlüsse Sunflare 1000 und Exodus 1500 gegen Deathflare 500 und Akh Morn 1300.

**Der Abstand zur Zwischenphase ist in Wahrheit noch größer als die Tabelle zeigt.** RSR parkt
zusätzlich Energy Drain, zweimal Necrotize und Searing Flash im Solar-Fenster — Necrotize und Fester
hinter der Bedingung `inSolarUnique && HasSearingLight` (`SMN_Reborn.cs`), Searing Flash, weil es erst
durch Searing Light entsteht. Das sind 1800 Potenz obendrauf, 25 Prozent mehr, und sie stehen nicht in
der Tabelle, weil der Vergleich ohne Searing Light geführt ist.

**Ein Punkt des Modells ist offen und ändert nichts.** Ob die Beschwörung selbst einen GCD kostet, ist
aus den Artefakten nicht eindeutig zu entscheiden: Ihr Tooltip sagt, sie teile keinen Recast mit
anderen Aktionen, und anders als Slipstream fehlt ihr der Satz, der den Recast auf alle übrigen Zauber
überträgt — das liest sich als GCD-frei; RSR ruft sie dagegen aus `GeneralGCD` auf. Das Skript rechnet
beide Lesarten. Die Rangfolge ist in beiden dieselbe, nur der Abstand zur Zwischenphase schrumpft von
2,46× auf 2,19×.

### Die Sonderaktion der drei Primals

**Ifrit hat die stärkste, und zwar in jedem der drei sinnvollen Maße.** Neben den beiden
wiederholbaren Gemshine-Formen für Einzel- und Gruppenschaden gewährt jeder Primal genau eine
Sonderaktion; sie unterscheiden sich nicht nur in der Potenz, sondern auch darin, ob sie einen
GCD-Platz kosten.

| Sonderaktion | Potenz | GCDs | je GCD | Zugewinn gegenüber dem Füller |
|---|---|---|---|---|
| Ifrit — Crimson Cyclone + Crimson Strike | 1120 | 2 | 560 | +320 |
| Titan — Mountain Buster | 160 | 0 | — | +160 |
| Garuda — Slipstream | 520 | 1 | 520 | +120 |

Die letzte Spalte ist das eigentliche Maß: Eine Sonderaktion, die einen GCD belegt, verdrängt einen
Füller (Ruin III, 400) und ist nur die Differenz wert. Mountain Buster kostet keinen GCD und behält
deshalb seinen vollen Wert, bleibt aber absolut der kleinste Beitrag. Dieselbe Rangfolge ergibt sich
für die ganzen Blöcke: Ifrit 632 Potenz je GCD, Titan 464, Garuda 407.

### Was in die Restzeit von Searing Light noch hineingeht

**Zwei bis drei Attacken, zusammen 800 bis 1360 Potenz — und welche es sind, entscheidet nicht die
Potenz, sondern die Gießzeit.** Searing Light deckt 20 Sekunden, das Solar-Fenster 15; nach dem
letzten Beschwörungs-GCD bleiben bei 2,50 s Wiederholzeit 5,0 Sekunden Buff, also zwei GCD-Plätze und
die daran gewebten Fähigkeiten. Gezählt wird nicht nach Plätzen, sondern auf der Uhr: Ein Zauber, der
innerhalb des Buffs beginnt und nach seinem Ende fertig wird, bekommt ihn nicht.

| Zuerst gerufen | Attacken im Buff | Potenz | was hineingeht |
|---|---|---|---|
| **Ifrit** | 2 | **1360** | Inferno 800, Crimson Cyclone 560 |
| Titan (heutige Voreinstellung) | 3 | 1300 | Earthen Fury 800, Topaz Rite 340, Mountain Buster 160 |
| Garuda | 1 | 800 | Aerial Blast 800 — **Slipstream fällt heraus** |

**Der Ausreißer ist Garuda, nicht Ifrit.** Slipstream beginnt auf dem zweiten Platz bei 17,5 Sekunden
und ist mit seiner Gießzeit erst nach dem Buffende fertig; der Buff greift beim Fertigwerden, nicht
beim Anfangen. Rechnerisch käme Slipstream mit Swiftcast auf 1320, und RSR hat dafür die Option
`AddSwiftcastOnGaruda` — **diese Möglichkeit steht hier aber nicht offen**, weil der Auftraggeber
Swiftcast für Wiederbelebungen zurückhält. Die 800 Potenz sind für Garuda zuerst damit fest. Titan
verliert dagegen nichts: Seine
GCDs sind sofort wirksam, und Mountain Buster kostet keinen Platz, sondern wird gewebt — deshalb
liefert Titan die meisten Attacken bei fast derselben Potenz.

**Bei schnellerer Wiederholzeit ändert sich die Rangfolge nicht.** Bei 2,45 s und 2,40 s trägt das
Beschwörungsfenster sieben statt sechs Zauber, die Restzeit schrumpft auf 2,85 beziehungsweise 3,20
Sekunden — es bleiben dieselben zwei Plätze und dieselben Werte.

**Wer nicht heranspringt, verliert bei Ifrit doppelt.** Crimson Strike entsteht erst aus Crimson
Cyclone („Grants Crimson Strike Ready", `ActionId.resx`); wird der Anlauf aus Sicherheitsgründen
ausgelassen, fällt der Ifrit-Block von 3160 Potenz über fünf GCDs auf 2040 über drei, und die beiden
frei werdenden Plätze gehen an den Füller zurück — 568 Potenz je GCD statt 632. Im Bufffenster ist der
Rückschlag größer, weil der Ersatz auf dem zweiten Platz Ruby Rite ist und Ruby Rite eine Gießzeit hat:

| Zuerst gerufen, ohne Anlauf | Attacken im Buff | Potenz |
|---|---|---|
| **Titan** | 3 | **1300** |
| Ifrit | 1 bis 2 | 800 bis 1420 |
| Garuda | 1 | 800 |

**Titan ist der einzige Block, dessen Wert weder an der Position noch an einer unbelegten Gießzeit
hängt.** Seine GCDs sind sofort wirksam, und Mountain Buster wird gewebt, kostet also keinen Platz.
Die Spanne bei Ifrit ist genau die offene Gießzeit von Ruby Rite: sofort wirksam landet es im Buff,
eine volle Wiederholzeit lang nicht mehr.

**Der Gewinn bleibt klein.** Zwischen der besten und der schlechtesten Reihenfolge liegen 560 Potenz
innerhalb des Buffs; der Buff steigert um 5 Prozent, also 28 Potenz gegen 30 440 Potenz
Zyklusleistung — 0,09 Prozent. Zwischen Ifrit und der heutigen Voreinstellung Titan sind es 60
Potenz, drei Potenz Schaden, 0,01 Prozent. **Die Voreinstellung ist damit bereits nahezu optimal, und
die einzige Reihenfolge, die wirklich etwas kostet, ist Garuda zuerst — die sie ohnehin vermeidet.**

**Bedeutung bekommt die Reihenfolge erst dort, wo V7 zündet.** Liegt der Buff vollständig außerhalb
eines Beschwörungsfensters, füllen ihn acht GCDs Primalblock: Ifrit zuerst 4800 Potenz, Titan zuerst
3920, Garuda zuerst 3740. Zwischen bester und schlechtester Reihenfolge liegen dann 1060 Potenz,
0,17 Prozent des Zyklus. Auch das bleibt klein.

**Die Voreinstellung bleibt, und das ist die Entscheidung des Auftraggebers.** Ifrit zuerst lohnt nur,
wenn man ohnehin in Nahkampfreichweite des Ziels steht; der Anlauf von Crimson Cyclone in eine
Burstphase hinein ist ein Positionsrisiko, das 0,01 Prozent Schaden nicht rechtfertigen. Titan ist
sicher, erlaubt Bewegung und kostet 60 Potenz — drei Potenz Schaden je Zyklus.

**Dieselbe Bedingung gilt für den Ausweichblock des Zündfensters, und sie ist dort umgesetzt (A112, A113).**
Sind alle drei Hauptphasen — Solar, Bahamut, Phoenix — dauerhaft von anderen Beschwörern belegt, wird in den
Primalblock ausgewichen: **Titan**, oder **Ifrit genau dann, wenn der Spieler ohnehin am Ziel steht**.
Dann entfällt der Anlauf, seine Voraussetzung ist erfüllt, und die höhere Zahl gilt ohne Positionsrisiko.
Gemessen wird an derselben Schwelle, die die Rotation für genau diese Frage schon führt — `CrimsonCycloneDistance`.

**Zwei Einstellungen stützen diese Wahl, beide am Code belegt.** `PreferTitanWhileMoving`
zieht in `SummonPrimals` Titan bei Bewegung vor, unabhängig von der eingestellten Reihenfolge;
voreingestellt aus. Und `AddCrimsonCyclone` ist voreingestellt **an** und bedeutet ausweislich seines
Optionstexts und der Bedingung in `:483` — `AddCrimsonCyclone || DistanceToPlayer() <=
CrimsonCycloneDistance` —, dass die Distanzprüfung übersprungen wird: RSR springt aus beliebiger
Entfernung heran. Wer den Anlauf auf Nahkampfreichweite begrenzen will, schaltet die Option aus; dann
greifen die drei Yalm aus `CrimsonCycloneDistance`.

Dass Slipstream eine Gießzeit hat, ist aus `AddSwiftcastOnGaruda` belegt, und dass die Topaz-GCDs
sofort wirken, aus dem Optionstext von `PreferTitanWhileMoving`.

*Offen bleibt die Länge der Gießzeiten.* Belegt ist aus dem Repository nur, **dass** Slipstream und die
Ruby- und Emerald-GCDs eine haben. Die drei Sekunden für Slipstream sind Fremdquelle. Das Prüfmittel
führt Aktionen mit unbelegter Gießzeit als sofort wirksam und benennt sie ausdrücklich, damit aus
einer fehlenden Zahl kein stiller Nullbefund wird.

### Woher die Zahlen kommen

Alle Potenzen ohne Merkmalsaufwertung stammen aus `ActionId.resx` in diesem Repository und sind damit
am Artefakt belegt: Umbral Impulse 640, Luxwave 160, Wyrmwave 150, Scarlet Flame 150, Sunflare 1000,
Exodus 1500, Deathflare 500, Akh Morn 1300, Revelation 1300, Necrotize 500, Searing Flash 700, Energy
Drain 100.

**Vierzehn Werte sind nicht am Repository belegt**, weil das Spiel die Zahl im Tooltip leer lässt,
sobald ein Merkmal sie überschreibt — `ActionId.resx` enthält an diesen Stellen wörtlich „with a
potency of ." Betroffen sind Astral Impulse 500, Fountain of Fire 580, Ruin III 400, Ruin IV 520, Ruby
Rite 620, Topaz Rite 340, Emerald Rite 280, Crimson Cyclone 560, Crimson Strike 560, Slipstream 520,
Mountain Buster 160, Inferno 800, Earthen Fury 800, Aerial Blast 800. Sie stammen aus
Suchmaschinenzusammenfassungen; die Primärquellen — Job-Guide, FFXIV-Wiki, Icy Veins, The Balance —
sind vom Egress dieser Umgebung gesperrt. Ein Kalibrierungspunkt spricht für sie: Für Umbral Impulse
nennt dieselbe Quelle 640, und das ist der Wert, den `ActionId.resx` unabhängig belegt.

Die Tragfähigkeit der Aussagen hängt unterschiedlich stark daran. Der Abstand der drei
Beschwörungsfenster untereinander ruht überwiegend auf belegten Werten. Der Abstand zur Zwischenphase
und die Rangfolge der drei Sonderaktionen ruhen auf den unbelegten. Inferno, Earthen Fury und Aerial
Blast sind mit 800 gleich angesetzt und heben sich im Vergleich der Reihenfolgen ohnehin auf.

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
Beschwörungsfenster. Er ist gemessen, nicht mit einem Argument abgetan.

**Mechanismus:** Zünden, sobald kein Buff steht, die eigene Wiederholzeit frei ist und ein Ziel im
Kampf vorliegt. Die letzte Bedingung trägt den naheliegenden Einwand ab — der Buff verpufft nicht in
einer Phase ohne Gegner.

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

**V1 und V8 umsetzen, V7 zurückbauen. V4, V5, V6 nicht.**

V8 enthält V2 vollständig und ergänzt es um eine Klausel, die nur greift, wenn alle Burstphasen
dauerhaft belegt sind. Wer V8 baut, baut V2 mit.

| | Gewinn im maßgeblichen Bereich | Preis |
|---|---|---|
| **V1** | Aetherflow-Ausgaben liegen ab zwei Beschwörern im laufenden Fenster statt daneben | eine zusätzliche Eigenschaft, keine Zustandshaltung |
| **V8** | beim synchronen Pull 46 % des eigenen Schadens unter Buff statt 27 %, und 56 % statt 48 %, sobald fremde Beschwörer alle Phasenarten belegen; bei versetzten Rotationen unverändert | Zustand über den Kampf: je Phasenart der zuletzt beobachtete fremde Zünder und wie oft er wiederkam |
| ~~V7~~ | im synchronen Pull gegen belegte Phasen vorn | verliert gegen Gegenspieler auf Solar und bei zwei Beschwörern mit vollem Versatz, dort unter die heutige Regel |

**V8 verlangt Zustand, und das ist der bewusst gezahlte Preis.** Ohne Beobachtung lässt sich nicht
erkennen, ob alle Phasenarten belegt sind, und ohne diese Erkennung bleibt nur die Wahl zwischen
„nie ausweichen" (V2, verschenkt den belegten Fall) und „blind ausweichen" (V7, verschenkt zwei
andere). Der Zustand ist klein und selbstheilend: drei Einträge, jeder verfällt, wenn der Zünder nach
seiner Wiederholzeit plus Nachfrist ausbleibt. Rücksetzpunkte bei Kampf-, Gruppen- und Zonenwechsel
sind damit nicht nötig — ein neuer Kampf beginnt ohne gültige Einträge, weil alle verfallen sind.

**Die Buchführung beantwortet genau eine Frage, und das ist ihre Aufgabenteilung:** Sind **alle**
Phasenarten dauerhaft belegt? Sie sagt **nicht**, welche Phase anzustreben ist — angestrebt wird
immer jede freie Burstphase, die gerade steht. Das ist der Unterschied zwischen „für diese Phase
zurückhalten" und „diese Phase aufgeben"; die Vorgabe sagt ausdrücklich das Erste, und ihr Punkt 5
versucht Solar in der nächsten Runde erneut.

**Was zu beobachten ist, beobachtet der Client ohnehin:** `IStatus.SourceId` nennt den Urheber des
laufenden Buffs, und die eigene Phase steht fest. Mehr braucht die Regel nicht — insbesondere keine
Abfrage fremder Abklingzeiten, die es nicht gibt.

**Die Reihenfolge der Primals gehört nicht dazu, und sie bleibt, wie sie ist.** Gemessen trägt Ifrit
zuerst 0,01 Prozent gegenüber der Voreinstellung, 0,09 gegenüber der schlechtesten Reihenfolge und
0,17 außerhalb eines Beschwörungsfensters. Dem steht der Anlauf von Crimson Cyclone in die Burstphase
gegenüber; Titan ist sicher, erlaubt Bewegung und kostet drei Potenz Schaden je Zyklus. Was hier
aussteht, ist deshalb keine Codeänderung, sondern die Empfehlung, `AddCrimsonCyclone` auszuschalten
und `PreferTitanWhileMoving` einzuschalten.

**Die Gruppenprüfung bleibt der Schalter.** Bei einem einzelnen Beschwörer ändert sich nichts, und
das ist gemessen und nicht bloß beabsichtigt: Das Modell weist für einen Beschwörer in jeder Variante
denselben Wert aus.

**Ein Ansatz ist geprüft und verworfen:** einem Beschwörer, der in der Gruppe steht, aber nie zündet,
die stärkste Phase zu überlassen. Der Gedanke trägt — wer im selben Fenster steht und jede
Gleichzeitigkeit verliert, bringt seine Ladung nie in den Kampf, und Ausweichen brächte zwei Ladungen
statt einer ins Spiel. Gemessen bringt es beim synchronen Pull einen Prozentpunkt und kostet bei
halbem Versatz zwei, weil die Regel „durch mich blockiert" nicht von „diesen Zyklus still" trennen
kann. Wer falsch rät, verschenkt die beste Phase umsonst.

## Die Umsetzung

**Umgesetzt sind V1, V2 und V8; V8 hat V7 ersetzt.** Die blinde Teilbedingung `!HasAnySearingLight`
im Zündausdruck ist entfernt, an ihrer Stelle entscheidet die Klausel aus Punkt 6 dasselbe Ausweichen
aus der Lage heraus.

| Ort | Eingriff | Stand |
|---|---|---|
| `SummonerRotation.cs` | `HasAnySearingLight` — `PlayerHasStatus(false, …)` statt `true`, also der Buff gleich welcher Herkunft | umgesetzt |
| `SummonerRotation.cs` | `AnotherSummonerInParty` — lebender Beschwörer in der Gruppe, Stufe aus `SearingLightPvE.Level` | umgesetzt |
| `SMN_Reborn.cs` (dreimal) | V1: Painflare, Necrotize und Fester fragen nach `HasAnySearingLight` | umgesetzt |
| `SMN_Reborn.cs` | Zündfenster `burstInSolar \|\| (AnotherSummonerInParty && (inBigInvocation \|\| !HasAnySearingLight))` | umgesetzt, entspricht V7 |
| `SummonerRotation.cs` | **V8**: Phasenbuch je Phasenart (`UpdateSearingPhaseBook`, `AllSearingPhasesHeld`), fortgeschrieben in `UpdateInfo` | umgesetzt |
| `SMN_Reborn.cs` | Zündfenster `burstInSolar \|\| (AnotherSummonerInParty && (inBigInvocation \|\| (AllSearingPhasesHeld && (TitanActive \|\| (IfritActive && am Ziel stehend)))))` — V7 ersetzt | umgesetzt |
| `SMN_Reborn.cs` | Zündung zusätzlich im Platz **vor** der großen Beschwörung: `burstAboutToStart` = Burst an und Beschwörung bis zum nächsten GCD bereit (A114, A126) | umgesetzt |
| `SMN_Reborn.cs` | Die Beschwörung wartet auf den Buff (`searingSettled`), Bedingungen im Abschnitt „Sachstand" (A115, A127) | umgesetzt; ob sie warten soll, ist offen (`TODO.md`) |

**V8 hat V7 ersetzt und nicht ergänzt.** V7 zündet blind, sobald der Buff aus ist; V8 entscheidet
dasselbe aus der Lage. Beides nebeneinander hieße, dass die blinde Bedingung die überlegte jedes Mal
überholt.

**Das Buch im Plugin ist einfacher gebaut als im Modell, und zwar bewusst.** Das Modell führt je
Phasenart den zuletzt beobachteten fremden Zünder samt Zeitstempel und lässt den Eintrag nach
Wiederholzeit plus Nachfrist verfallen. Die Umsetzung führt je Phasenart nur einen Zähler: Beim
Betreten einer Burstphase steigt er, wenn ein fremdes Searing Light läuft, und wird auf null gesetzt,
wenn keines läuft. Drei Folgen, alle geprüft:

- **Kein Zeitwert wird gebraucht und keiner erfunden.** Die Rücksetzung geschieht durch die
  Beobachtung selbst — wer aufhört zu zünden, wird beim nächsten Durchgang nicht mehr angetroffen.
- **Wechselnde Zünder werden richtiger behandelt als im Modell.** Teilen sich zwei fremde Beschwörer
  eine Phase, erreicht im Modell keiner von beiden die zweite Beobachtung und die Phase gilt als
  frei; für den eigenen Beschwörer ist sie gleichwohl verloren. Der Zähler ohne Urheber beantwortet
  die Frage, die zählt: Ist diese Phase für mich zu haben?
- **Der eigene Buff urteilt nicht.** Läuft die eigene Ladung, wird das Fenster übergangen statt
  gebucht oder gelöscht, sonst löschte eine in Solar gesetzte Ladung beim Betreten von Bahamut
  dessen Eintrag auf die Kraft eines selbst gewirkten Buffs hin.

Außerhalb des Kampfes wird das Buch geleert; ein neuer Kampf beginnt also bei V2-Verhalten und
erreicht die Punkt-6-Klausel erst, wenn jede Phasenart zweimal belegt angetroffen wurde.

**Die Stufenschwelle kommt aus den Spieldaten, nicht aus einer Zahl im Code.** `SearingLightPvE.Level`
liest `ClassJobLevel` der Aktion; ein Beschwörer unterhalb dieser Stufe hat kein Searing Light zu
geben, und ihn mitzuzählen hielte diesen hier für einen Buff zurück, der nicht kommen kann. Genau
diesen Fehler hatte `AnyLivingRaiser` beim Rotmagier gemacht.

**Die Allianz wird nicht gefragt.** Searing Light erreicht nach seinem eigenen Text „nearby party
members"; ein Beschwörer in einer anderen Allianzgruppe verstärkt diesen Spieler nie. Die Frage ist
damit eine Gruppenfrage, keine Allianzfrage — anders als bei der Wiederbelebung, wo die Allianz je
nach `RaiseType` mitzählt.

**Zwei Stellen bleiben bewusst unverändert.** Die Burst-Medizin (`UseBurstMedicine` in
`SMN_Reborn.cs`) fragt weiter
nach dem **eigenen** Buff: Sie ist eine Fünfzehn-Minuten-Ressource und gehört in das eigene
Solar-Fenster, das stärkste des Zyklus; an einen fremden Buff gehängt landete sie irgendwo. Und
`ChurinSMN.cs` trägt denselben V1-Befund (`:995`, `:1015`), ist aber fremdes Werk mit eigener
Abstimmung — erfasst, nicht bearbeitet.

### Die Kopplung zwischen Zündung und Beschwörungswahl

**Die Zündregel für mehrere Beschwörer wirkt auf die Beschwörung zurück, und diese Rückwirkung ist
gesperrt.** Wer außerhalb des Solar-Fensters zündet, setzt Searing Light zu einem anderen Zeitpunkt auf
Abklingzeit. Wartete die Solar-Beschwörung dann auf den Buff, verschöbe die Ausweichregel die
teuerste Phase des Zyklus — eine Verschiebung um ein Fenster kostete 1600 Potenz, mehr als der
gesamte Zugewinn an Buffzeit. Deshalb wartet die Beschwörung mit einem zweiten Beschwörer in der
Gruppe auf keinen abkühlenden Buff, und bei einem laufenden fremden Buff auf gar keinen (Abschnitt
„Sachstand"). Es gibt genau **einen** Aufruf der großen Beschwörung mit dieser Bedingung; der frühere
Doppelaufruf, dessen zweiter Zweig unerreichbar war, ist seit A115 zusammengeführt.

**Offen und nicht aus dem Repository zu entscheiden:** ob Summon Solar Bahamut seine Abklingzeit mit
Bahamut und Phoenix teilt. Die Wirktexte aller drei sagen „does not share a recast timer with any
other actions"; `burstAboutToStart` liest auf Stufe 100 die Abklingzeit von Solar. Teilt sie sie nicht,
steht diese Freigabe von Ablauf der eigenen Abklingzeit bis zur nächsten Solar-Phase offen, und ein
früh bereiter Buff fiele dann sofort statt vor der Beschwörung. Im Regelfall — der Buff kehrt mit der
Solar-Phase zurück — ist das ohne Wirkung.

**Ein zweiter Einwand, der bleibt, aber nicht neu ist:** Läuft der Buff aus, sind mehrere Beschwörer
gleichzeitig frei und können im selben Augenblick zünden; einer verschwendet. Das Modell schreibt
sequenziell zu und bildet das nicht ab. Die erweiterten Fenster verteilen die Gelegenheiten und machen
die Kollision seltener, nicht häufiger.

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

## Offene Punkte zu diesem Konzept

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.
