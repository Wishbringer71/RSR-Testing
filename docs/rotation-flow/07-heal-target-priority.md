# 07 · Zielwahl der Heilung

## Vorgabe des Auftraggebers

**Maßgeblich ist, wer wie stark gefährdet ist zu sterben.** Nicht der Prozentsatz, nicht die Rolle
für sich, nicht die Barriere für sich — die Gefährdung.

Daraus folgen die Regeln, die er genannt hat:

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
Maß, das erst bei Aggro anschlägt, verfehlt genau diesen Fall — und es ist der Fall, den die heutige
Rangfolge in ihrem letzten Rang eigentlich abdeckt, nur mit dem falschen Maß: Prozentsatz statt
absoluter Punkte.

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

## Abgrenzung zur Schildanrechnung

Die entfernte Schildanrechnung (`AUDIT_LOG.md` A85) hat dieselbe Größe an der falschen Stelle
verwendet: Sie rechnete die Barriere auf die **Heilschwelle** und verzögerte damit die Heilung
überhaupt. Gesundheit und Barriere addieren sich aber — ein vollgeheilter Tank mit Barriere ist
besser geschützt als ein geschildeter mit wenig Gesundheit. Für die **Reihenfolge zwischen Zielen**
ist die Barriere dagegen eine zulässige Größe: Dort wird nicht gefragt, ob geheilt wird, sondern wer
zuerst. Der einführende Commit der Anrechnung (`27c7b6942`) nannte im Titel genau diese Frage und
änderte dann die Schwelle.

## Grenzen des Nachweises

Am Quelltext belegt: die Rangfolge samt Vorgabewerten, das Fehlen jeder Aggro- und Barrierenabfrage
darin, die Verfügbarkeit von `TargetObject` und `GetEffectiveHp`, und dass die Rate je Mitglied nicht
erhoben wird.

Nicht belegbar: welche Reihenfolge im Kampf die bessere ist. Das entscheidet sich an einer
Beobachtung — ob ein Mitglied stirbt, während ein besser geschütztes zuerst versorgt wurde.
