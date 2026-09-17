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

## Der Entwurf: Gefährdungsklassen statt einer Kette von Kurzschlüssen

**Keines der Einzelmaße trägt allein, und der Grund ist derselbe bei allen: Jedes ist ein Surrogat,
und jedes bricht in einer anderen Lage.** Die Lösung ist deshalb nicht, eines davon zu wählen,
sondern jedes dort einzusetzen, wo es das Richtige misst.

| Maß | Sein Vorteil | Wo es bricht |
|---|---|---|
| Prozentsatz | normiert stillschweigend auf die erwartete Rate — wer mehr Pool hat, nimmt auch mehr Schaden | bei ungerichtetem Flächenschaden, der alle mit derselben absoluten Zahl trifft |
| absolute effektive Punkte | genau dort richtig: sie sagen, wer den nächsten Einschlag nicht überlebt | hält den Tank bei 30 % für sicherer als den Schadensausteiler bei 45 %, obwohl der Tank den Dauerschaden nimmt |
| Rollenvorrang | bildet die Ersetzbarkeit ab, die die Vorgabe verlangt | überholt als Kurzschluss den, der tatsächlich stirbt |
| Rollenschwellen 45/40 | vertreten die fehlende Schadensrate | rollenfest statt lagefest: falsch, sobald der Heiler die Aggro hält |
| Aggro | misst, wer gerichteten Schaden bekommt, statt es aus der Rolle zu schließen | sagt nichts über Flächenschaden |

**Der Entwurf ordnet die Kandidaten in drei Gefährdungsklassen. Innerhalb einer Klasse entscheidet
das Maß, das für diese Lage das richtige ist; bei Gleichstand die Rolle.**

| Klasse | Wer hineinfällt | Ordnung darin | Warum dieses Maß |
|---|---|---|---|
| **1 — kritisch** | effektive Gesundheit ≤ `HealthForDyingTanks` (Vorgabe 0,15) | niedrigste **absolute** effektive Punkte zuerst | Wer hier steht, stirbt am nächsten Treffer, und ein Treffer ist eine absolute Zahl |
| **2 — Rollenvorrang, bedingt** | Heiler ≤ `HealthHealerRatio` oder Tank ≤ `HealthTankRatio`, **und** unter Beschuss | niedrigster **Prozentsatz** zuerst, bei Gleichstand Heiler vor Tank | Hier ist die Rate rollenproportional, und genau dafür ist der Prozentsatz das brauchbare Surrogat |
| **3 — übrige** | alle anderen | absolute Punkte, solange ein Flächenschaden angekündigt ist; sonst Prozentsatz | Ein Raidwide trifft alle mit derselben Zahl; ohne ihn gilt wieder die Rollenproportionalität |

**Über allen Klassen steht weiterhin die Unterscheidung geschützt/ungeschützt**, und zwar
unverändert: Wer eine Unverwundbarkeit trägt, wird nach hinten gestellt, nicht ausgeschlossen. Ohne
diesen äußersten Schlüssel kehrt sich Klasse 1 gegen sich selbst — ein Krieger der Dunkelheit unter
Superbolide steht auf 1 Trefferpunkt, hätte also die wenigsten absoluten Punkte von allen und stünde
vor jedem echten Notfall. Die Klassenordnung wirkt innerhalb der Ungeschützten.

**Die Gesundheitsschranke in Klasse 2 ist nicht schmückendes Beiwerk, sondern trägt die Klasse.**
Ohne sie wäre jeder Tank dauerhaft in Klasse 2, denn er trägt seine Tankhaltung immer — und ein Tank
bei 65 % stünde vor einem Schadensausteiler bei 30 % in Klasse 3. Das Kandidatenfeld fängt das nicht
ab: `healRatio` filtert bei 0,70, beide sind darin. Die beiden Rollenschwellen behalten damit genau
die Bedeutung, die sie heute haben, und bekommen zusätzlich die Bedingung, die ihnen fehlte.

**Gleichstandsregel in jeder Klasse: Heiler vor Tank vor Schadensausteiler** — die Triage der
Vorgabe, und nur dort, wo sie hingehört, nämlich bei gleicher Gefährdung.

**Was der Entwurf damit von jedem Maß nimmt und was er ablegt:**

- Der Rollenvorrang bleibt erhalten, **überholt aber niemanden mehr**, der tiefer steht: Er wirkt
  nur noch innerhalb einer Klasse. Der Schadensausteiler bei 10 % steht in Klasse 1, der Tank bei
  44 % in Klasse 2 — die Reihenfolge ist damit entschieden, bevor die Rolle überhaupt gefragt wird.
- Der Prozentsatz behält die Lage, in der er richtig ist, und verliert die, in der er blind ist.
- Die absoluten Punkte bekommen genau die zwei Lagen, für die sie gebaut sind, und keine weitere.
- Die Aggro tritt **neben** die Rollenschwellen, statt sie zu ersetzen: Die Schwelle sagt weiterhin
  „tief genug", die Aggro neu „und er bekommt tatsächlich Schaden". Damit ist der zweite Befund
  entschärft — hält der Heiler die Aggro und der Tank nicht, steht der Heiler in Klasse 2 und der
  Tank in Klasse 3, obwohl die Zahlen unverändert sind.
- Auch der Selbst-Kurzschluss rückt hinter Klasse 1. Er trägt denselben Überholfehler wie die beiden
  Rollenzweige: Heute wird ein Schadensausteiler bei 10 % übergangen, sobald der Spieler selbst bei
  39 % steht.
- **Kein Maß wird gewichtet und keine Zahl erfunden.** Die Klassen sind Ja/Nein-Fragen an
  vorhandene Größen; innerhalb einer Klasse wird verglichen, nicht verrechnet. Das ist der
  Unterschied zu einer gemeinsamen Gefährdungszahl, die einen Nenner bräuchte, den es nicht gibt:
  BossModReborn nennt Art und Zeitpunkt des nächsten Einschlags (`BMRNextDamageType`,
  `BMRNextDamageIn`), **nicht seine Höhe**.

**Was der Entwurf nicht löst, und das ist ehrlich zu benennen:** Die Schwellendifferenz 45/40 ist in
jeder Fassung entweder wirksam — dann kehrt sie im Band zwischen den beiden Werten die Rangfolge um —
oder unwirksam, dann ist eine Einstellung des Nutzers stillgelegt. Die Klassenordnung entschärft sie,
weil Klasse 1 und die Aggro davorstehen, aber sie beseitigt sie nicht. Ob die beiden Werte
vereinheitlicht werden, ist eine Wertentscheidung über eine Konfiguration und gehört dem
Auftraggeber.

**Die Rate bleibt der einzige fehlende Baustein**, und sie wird erst innerhalb von Klasse 2 gebraucht:
wenn mehrere zugleich unter Beschuss stehen und zu entscheiden ist, wer von ihnen zuerst fällt.
`DataCenter.DPSTaken` misst die Gruppe als Ganzes — `DamageRec` trägt Zeitpunkt und Anteil, **kein
Ziel** —, und `DataCenter.RecordedHP` wird ausschließlich aus `AllHostileTargets` gefüllt, weshalb
`GetTTK` für eine Gruppen-Id `NaN` liefert. Es fehlt der Aufnehmer, nicht die Quelle.

**Kosten der Erhebung, gemessen am Ort:** Die Aggro braucht keinen Vergleich je Mitglied. In
`TargetUpdater.UpdateLists`, wo `AllHostileTargets` ohnehin einmal je Bild aufgebaut wird, sammelt
ein Durchlauf über die Gegner deren `TargetObjectId` in ein Set — danach ist „wird angegriffen" eine
Nachschlageoperation. Der Aufwand wächst mit der Zahl der Gegner, nicht mit ihrem Produkt.

## Die Stufen

| Stufe | Inhalt | Nachweislage |
|---|---|---|
| **1** | Klasse 1 vor alle drei Kurzschlüsse ziehen | **umgesetzt** in `ActionTargetInfo.GeneralHealTarget`: ungeschützte Kandidaten auf oder unter `HealthForDyingTanks` (effektive Gesundheit, wie `CanProvoke` sie liest) werden vor Selbst-, Heiler- und Tankzweig zurückgegeben, geordnet nach absoluten effektiven Punkten, bei Gleichstand Heiler vor Tank vor Schadensausteiler |
| **2** | Klassen 2 und 3 mit Aggro und lageabhängigem Maß | Verbesserung, deren Nutzen ohne Spielbeobachtung eine Annahme bleibt → hinter eine Einstellung, Voreinstellung wie bisher |
| **3** | Rate je Mitglied | **umgesetzt**, aber anders als hier zunächst geplant — siehe unten |

### Stufe 3 ist nicht die Ordnung innerhalb einer Klasse geworden, sondern die Größe selbst

Geplant war, die Rate als **zusätzliches** Ordnungsmerkmal innerhalb von Klasse 2 einzusetzen. Beim
Bauen hat sich das als der schlechtere Zuschnitt erwiesen, und der Entwurf ist geändert worden:
Statt ein zweites Merkmal neben die Gesundheit zu stellen, wird die **gelesene Gesundheit selbst**
vorausberechnet. Jede der vier Entscheidungen dieser Methode — die Kandidatenordnung, die Schwelle
der kritischen Klasse, ihre Ordnung nach absoluten Punkten und der Selbstkurzschluss — liest jetzt
`ObjectHelper.GetForecast*` statt der aktuellen Größe.

**Warum das besser ist:** Ein zusätzliches Merkmal hätte eine zweite Rangregel erzeugt, die mit der
ersten auseinanderläuft, sobald eine von beiden angefasst wird. Die Ersetzung ändert **eine** Größe,
und alle vorhandenen Klassen, Schwellen und Kurzschlüsse erben die Vorausschau, ohne umgebaut zu
werden. Sie wirkt zudem auf Klasse 1 mit, was der ursprüngliche Zuschnitt nicht getan hätte — und
gerade dort entscheidet sich, wer stirbt.

**Was sie im Kampf ändert:** Ein Schwarzmagier bei 48 %, dessen Gesundheit in vier Sekunden
aufgebraucht ist, steht prognostiziert bei 3 % und fällt damit in Klasse 1 — vor den Tank-Kurzschluss,
der ihn bei 44 % des Tanks heute überholt. Rechnung und Grenzfälle in Konzept 08, Abschnitt „Der
Verbraucher".

Hinter `HealAheadOfDamage`, Standard aus; ausgeschaltet liefern alle vier Getter die heutigen Werte.
Die Nachweislage ist damit unverändert die der Stufe 2: Der Nutzen bleibt eine Annahme, bis er im
Spiel beobachtet ist.

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
