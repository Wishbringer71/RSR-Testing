# 07 · Zielwahl der Heilung

## Vorgabe des Auftraggebers

**Oberste Priorität hat das gesamtheitliche Überleben der Gruppe.** Es sollen alle überleben; wo das
nicht für alle zugleich geht, gilt Triage.

**Maßgeblich ist dann, wer wie stark gefährdet ist zu sterben.** Nicht der Prozentsatz, nicht die
Rolle für sich, nicht die Barriere für sich — die Gefährdung.

**Bei gleicher Gefährdung entscheidet die Rolle, und zwar Heiler vor Tank vor Schadensausteiler.**
Seine Begründung ist keine Rangordnung nach Wichtigkeit, sondern nach Ersetzbarkeit: Der Tank kann
einige Zeit ohne Heiler bestehen — er trägt Minderungen, einen großen Lebenspool und eigene
Selbstheilung. Heilen kann dagegen nur der Heiler. Fällt er, fällt die Gruppe mit ihm, und zwar
verzögert, aber unvermeidlich. Die Rolle ist hier also selbst eine Gefährdungsgröße: Der Tod des
Heilers gefährdet zusätzlich alle anderen.

Daraus folgen die weiteren Regeln, die er genannt hat:

- **Hält der Tank die Aggro, hat er die Heilpriorität.** Er bekommt den Schaden, also bekommt er die
  Heilung.
- **Hält der Heiler die Aggro, muss der Heiler überleben.** Fällt er, fällt die Gruppe mit ihm, und
  er trägt weder die Minderungen noch den Lebenspool eines Tanks.
- **Sind mehrere zugleich betroffen — Tank und Heiler, oder auch Schadensausteiler —, entscheidet
  die Schadensrate.** Dann reicht „wer wird angegriffen" nicht mehr; es zählt, wie schnell die
  Gesundheit fällt.
- **Ein Schadensausteiler ohne Aggro bei 10 % Leben kann an einer Flächenaktion sterben.** Aggro ist
  also keine Bedingung für Gefährdung, sondern eine ihrer Ursachen.

## Was Gefährdung heißt

**Aggro sagt nicht, ob jemand Schaden bekommt — nur, ob er *gerichteten* Schaden bekommt.** Eine
Flächenaktion trifft ohne Rücksicht darauf, und wer wenig Puffer hat, stirbt daran, ob er angegriffen
wurde oder nicht. Vier Größen also, und keine davon genügt allein:

| Größe | Beantwortet | Im Baum vorhanden |
|---|---|---|
| **Effektive Gesundheit, absolut** | Wie viele Punkte liegen zwischen ihm und dem Tod? | ja — `GetEffectiveHp` (Gesundheit plus Barriere); die Zielwahl liest sie nur nicht |
| **Aggro** | Bekommt er gerichteten Schaden — Auto-Angriffe, Tankbuster? | ja — ein Gegner nennt sein Ziel über `TargetObject`, `ObjectHelper.CanProvoke` löst das bereits auf |
| **Angekündigter Flächenschaden** | Kommt Schaden, der ihn ohne Aggro trifft? | ja — `IsHostileCastingAOE` und die BossModReborn-Vorhersage (`BMRNextDamageIn`) |
| **Eingehende Schadensrate** | Wie schnell schwindet der Puffer? | **nein**, je Mitglied nicht |

Aus den ersten beiden folgt der Puffer, aus allen vieren die **Zeit bis zum Tod** — das Gegenstück zu
`GetTTK`, das RSR für Gegner bereits führt. Für Gruppenmitglieder fehlt es.

**Der kleine Puffer ist damit für sich gefährlich.** Wer bei 10 % steht, braucht keine Aggro, um an
der nächsten Flächenaktion zu sterben; die Aggro entscheidet nur, ob er auch ohne Mechanik fällt. Ein
Maß, das erst bei Aggro anschlägt, verfehlt genau diesen Fall.

**Prozentsatz und absolute Punkte sind beide Surrogate, und jedes bricht in einer anderen Lage.** Der
Prozentsatz normiert stillschweigend auf die erwartete Schadensrate: Ein Tank hat den größeren
Lebenspool und nimmt auch den größeren Schaden, ein Schadensausteiler beides kleiner — deshalb
vergleicht der Prozentsatz bei **gerichtetem** Schaden ungefähr die richtige Größe. Bei
**ungerichtetem** Flächenschaden bricht er, denn der trifft alle mit derselben absoluten Zahl, und
dort zählen die Punkte. Die absoluten Punkte brechen genau umgekehrt: Sie halten den Tank bei 30 %
für sicherer als den Schadensausteiler bei 45 %, obwohl der Tank den Dauerschaden nimmt.

Das Maß ist also nicht zu **tauschen**, sondern von der Lage abhängig zu machen — und die Lage ist
lesbar, weil angekündigter Flächenschaden im Baum steht.

## Sachstand

**Die Zielwahl fragt keine dieser Größen.** `ActionTargetInfo.FindHealTarget` entscheidet so:

| Rang | Bedingung | Schwelle |
|---|---|---|
| 1 | der Spieler selbst | ≤ `HealthSelfRatio` (0,40) |
| 2 | Heiler, nicht unverwundbar | ≤ `HealthHealerRatio` (0,40) |
| 3 | Tank, nicht unverwundbar | ≤ `HealthTankRatio` (0,45) |
| 4 | sonst: niedrigster **Prozentsatz** zuerst | — |

Die Rollenabkürzungen bilden die Vorgabe **teilweise** ab: Heiler und Tank stehen vor den übrigen.
Was fehlt, ist die Bedingung, die sie tragen soll. Ein Heiler bei 50 %, auf dem drei Gegner stehen,
wird behandelt wie ein Heiler bei 50 % ohne jede Bedrohung; ein Tank hinter einer Barriere wie einer
ohne; und ein Schadensausteiler mit kleinem Lebenspool wie einer mit großem.

**Die Schwellen sind Zielwahlschwellen, keine Heilschwellen.** Ob überhaupt geheilt wird, entscheiden
`HealthSingleAbility` und `HealthSingleSpell` weiter oben; `FindHealTarget` bekommt bereits ein durch
`healRatio` gefiltertes Feld und beantwortet nur noch, **wen** die ohnehin fallende Heilung trifft.
Eine dieser Rollenschwellen zu heben erzeugt deshalb keine zusätzliche Heilung und keine
Überheilung — es verschiebt die Reihenfolge.

### Befund: ein Rollen-Kurzschluss überholt den, der tatsächlich stirbt

**Die Rollenabkürzungen kehren sofort zurück, sobald ihre Schwelle erfüllt ist, und sehen dabei
niemanden sonst an.** `healerTars[0]` unter `HealthHealerRatio` beendet die Suche, `tankTars[0]`
unter `HealthTankRatio` ebenso — der Prozentvergleich in Rang 4 wird gar nicht mehr erreicht.

Im Kampf heißt das: **Ein Schadensausteiler bei 10 % wird übergangen, sobald der Tank bei 44 %
steht.** Das ist genau der Fall, den die Vorgabe nennt — „auch ein Damagedealer ohne Aggro mit 10 %
Leben kann bei einem AoE sterben" —, und er ist heute falsch entschieden. Der Tank bei 44 % hinter
Minderungen und einem großen Lebenspool ist nicht gefährdeter als ein Schadensausteiler bei 10 %; er
ist nur früher in der Reihenfolge.

**Das ist der schwerere der beiden Befunde**, weil er ohne jede neue Messung zu beheben ist: Wer
unter der Schwelle steht, an der der Baum selbst „dieser Spieler fällt gleich" sagt
(`HealthForDyingTanks`), gehört vor jeden Rollen-Kurzschluss. Die Rangfolge der Vorgabe — Heiler vor
Tank vor Schadensausteiler — gilt bei **gleicher** Gefährdung, und 10 % gegen 44 % ist keine gleiche
Gefährdung.

### Befund: die Rangfolge steht, die Schwellen kehren sie um

Die Prüfreihenfolge stellt den Heiler vor den Tank, wie die Vorgabe es verlangt. Die Zahlen tun es
nicht: Der Tank löst bei ≤ 45 % aus, der Heiler erst bei ≤ 40 %. **Im Band zwischen 40 % und 45 %
gewinnt deshalb der Tank, obwohl der Heiler gleich tief oder tiefer steht.**

Im Kampf sieht das so aus: Heiler und Tank stehen beide bei 44 %. Der Heiler-Zweig greift nicht
(44 > 40), der Tank-Zweig greift (44 ≤ 45) — die Heilung geht an den Tank. Der Heiler bekommt sie
erst, wenn er weitere vier Prozentpunkte verloren hat. Spielt der Auftraggeber selbst den Heiler,
trifft ihn derselbe Fall über `HealthSelfRatio`, das ebenfalls auf 0,40 steht: Er wird bei 44 %
zugunsten eines gleich stehenden Tanks übergangen.

**Die Verzerrung wirkt in dieselbe Richtung wie der Prozentvergleich.** Der Tank hat den größeren
Lebenspool, 45 % davon sind mehr Punkte als 40 % eines Heilerpools — die niedrigere Schwelle trifft
also ausgerechnet den, der pro Prozentpunkt weniger Puffer hat. Beide Effekte addieren sich statt
sich auszugleichen.

**Was für die Umkehr spricht, und wie weit es trägt:** Die höhere Tankschwelle bildet keine Priorität
ab, sondern die fehlende Schadensrate. Der Tank hält die Aggro, nimmt Auto-Angriffe und Tankbuster
und fällt deshalb schneller; ein Heiler bei 44 % steht im Regelfall still, ein Tank bei 44 % steht in
fünf Sekunden bei 20 %. Als Näherung der Rate ist die Differenz sinnvoll — und für den Regelfall, den
die Vorgabe mit „hält der Tank die Aggro, hat er die Heilpriorität" selbst nennt, ist sie richtig.

**Sie trägt nur nicht in dem Fall, für den die Vorgabe gemacht ist.** Das Surrogat ist rollenfest,
nicht lagefest: Es fragt, welche Rolle jemand hat, nicht, wer gerade Schaden bekommt. Hält der Heiler
die Aggro — der zweite Fall der Vorgabe —, dreht sich die Rate um, die Schwellen bleiben stehen. Dann
nimmt der Heiler den gerichteten Schaden, ohne Tankminderungen, ohne Tankpool, und wird gleichwohl
fünf Prozentpunkte später versorgt als der Tank, der in diesem Moment gar nichts abbekommt. Dasselbe
gilt für den Wall-to-Wall-Pull vor dem Einsammeln und für jeden ungerichteten Flächenschaden, der
beide gleich trifft.

**Damit ist der Befund keine falsche Zahl, sondern eine fehlende Bedingung.** Die Schwellendifferenz
ersetzt eine Messung, die es nicht gibt. Solange die Rate je Mitglied fehlt, ist jede feste Zahl an
dieser Stelle in der einen Lage richtig und in der anderen falsch.

**Kein Fork-Defekt:** `HealthTankRatio` 0,45, `HealthHealerRatio` 0,40 und `HealthSelfRatio` 0,40
stehen im Fork auf denselben Werten wie in `upstream/main`. Es sind die **Voreinstellungen im Code**;
welche Werte in der Konfiguration des Auftraggebers stehen, ist von hier aus nicht messbar.

**Zwei Verzerrungen im Prozentvergleich, beide entscheiden über Leben:** Ein Tank bei 50 % hinter
einer Barriere über 25 % seiner maximalen Gesundheit steht effektiv bei 75 %. Und 60 % eines kleinen
Lebenspools sind weniger Punkte als 50 % eines großen — derselbe Treffer tötet den mit den wenigeren
Punkten.

## Was zu bauen wäre

**Die Rate ist der fehlende Baustein, und er hat jetzt einen Verbraucher.** `DataCenter.DPSTaken`
misst die Gruppe als Ganzes: `DamageRec` trägt Zeitpunkt und Anteil, **kein Ziel**, und der einzige
Leser ist die Diagnoseanzeige. `DataCenter.RecordedHP` wird ausschließlich aus `AllHostileTargets`
gefüllt, weshalb `GetTTK` für eine Gruppen-Id `NaN` liefert. Es fehlt der Aufnehmer, nicht die
Quelle — bisher wurde er nicht gebaut, weil kein Verbraucher bestand (`TODO.md`). Die Zielwahl nach
Gefährdung ist dieser Verbraucher.

**Ohne die Rate lässt sich die Vorgabe zum größeren Teil umsetzen:** Der effektive Puffer in
absoluten Punkten, die Aggro und der angekündigte Flächenschaden sind sofort verfügbar. Sie decken
drei der vier genannten Fälle ab — Tank mit Aggro, Heiler mit Aggro, und den Schadensausteiler bei
10 %, der ohne Aggro an einer Flächenaktion stirbt. Die Rate wird erst gebraucht, wenn **mehrere**
zugleich unter Beschuss stehen; dann entscheidet sie, wer zuerst fällt.

**Die Reihenfolge des Bauens folgt dem Schweregrad, nicht dem Aufwand.** Zuerst die Notrangstufe:
Wer unter `HealthForDyingTanks` steht, kommt vor jeden Rollen-Kurzschluss. Das ist eine Bedingung vor
zwei bestehenden Zweigen, braucht keine neue Messung und behebt den Fall, in dem heute jemand stirbt.
Danach erst die Aggro und das lageabhängige Maß.

**Die Aggroabfrage löst den Schwellenbefund mit auf, und zwar an seiner Ursache.** Die
Schwellendifferenz vertritt heute die Aussage „der Tank bekommt den Schaden". Sobald diese Aussage
gemessen statt unterstellt wird, braucht es keine rollenfeste Zahl mehr: Wer die Aggro hält, steht
vorn; halten beide keine, greift die Rangfolge der Vorgabe, also Heiler vor Tank. Beide Lagen werden
dann richtig behandelt, und zwar mit **einer** Regel statt zwei gegenläufigen Zahlen. Die Rate bleibt
für den Fall übrig, dass mehrere zugleich Aggro haben.

**Bis dahin ist die Schwellendifferenz das kleinere Übel und bleibt stehen.** `HealthHealerRatio`
ohne die Aggroabfrage auf Tankhöhe zu ziehen, wäre der Tausch eines Fehlers gegen den anderen: Es
brächte den Heiler-mit-Aggro-Fall in Ordnung und verlöre den Regelfall, in dem der Tank tatsächlich
schneller fällt. Die Werte sind zudem Upstream-Voreinstellungen; sie ohne gemessenen Nutzen
abweichen zu lassen, kostet Merge-Aufwand ohne Gegenwert. Wer die eigene Gruppe anders erlebt, kann
den Wert in seiner Konfiguration heben — eine Codeänderung ist dafür nicht nötig, weil es eine reine
Zielwahlschwelle ist.

## Abgrenzung zur Schildanrechnung

Die entfernte Schildanrechnung (`AUDIT_LOG.md` A85) hat dieselbe Größe an der falschen Stelle
verwendet: Sie rechnete die Barriere auf die **Heilschwelle** und verzögerte damit die Heilung
überhaupt. Gesundheit und Barriere addieren sich aber — ein vollgeheilter Tank mit Barriere ist
besser geschützt als ein geschildeter mit wenig Gesundheit. Für die **Reihenfolge zwischen Zielen**
ist die Barriere dagegen eine zulässige Größe: Dort wird nicht gefragt, ob geheilt wird, sondern wer
zuerst. Der einführende Commit der Anrechnung (`27c7b6942`) nannte im Titel genau diese Frage und
änderte dann die Schwelle.

## Grenzen des Nachweises

Am Quelltext belegt: die Rangfolge samt Vorgabewerten, die Umkehr im Band 40–45 %, die Gleichheit der
drei Werte mit `upstream/main`, das Fehlen jeder Aggro- und Barrierenabfrage darin, die Verfügbarkeit
von `TargetObject` und `GetEffectiveHp`, und dass die Rate je Mitglied nicht erhoben wird. Der
Prüfgrad ist statische Selbstprüfung am Quelltext beider Stände.

Nicht belegbar: welche Reihenfolge im Kampf die bessere ist, und wie oft das Band 40–45 % mit einem
gleich tief stehenden Heiler überhaupt erreicht wird. Das entscheidet sich an einer Beobachtung — ob
ein Mitglied stirbt, während ein besser geschütztes zuerst versorgt wurde. Ebenfalls nicht messbar:
die tatsächliche Konfiguration des Auftraggebers.
