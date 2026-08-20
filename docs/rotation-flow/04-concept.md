# 04 · Zielkonzept

Ergebnis des internen Audits (Council → Critic → Antithese → Evaluation →
Revision, drei Durchläufe). Dokumentiert ist der Endstand plus die
Gegenargumente, die ihn geformt haben — nicht der geglättete Verlauf.

**Reihenfolge nach Wirkbreite:** A wirkt auf alle Jobs, B auf eine Gruppe,
C auf einen Job. Später kommende Stufen setzen frühere voraus, nie umgekehrt.

---

## Randbedingung, die alles andere begrenzt

```
CustomRotation                     handgeschrieben, gemeinsam
      ↓
{Job}Rotation                      GENERIERT (RotationGetter.cs:47)
      ↓
{Job}_Reborn                       handgeschrieben, je Job
```

Es gibt **keine Rollenebene**. Eine einzuziehen hieße, den Codegenerator zu
ändern — Build-Werkzeug, hohes Risiko, und es bringt nichts, was ein benannter
Helfer auf `CustomRotation` nicht auch leistet. **Entscheidung: keine neue
Vererbungsebene.** Rollenlogik lebt als rollenbenannter Helfer, so wie
`ShouldSustainMitigationDebuff` es bereits vormacht.

---

# A · Global (alle Jobs)

## A1 · ~~Sustain-Slot im Dispatch~~ → VERWORFEN

**Der Vorschlag ist an der eigenen Abnahmebedingung gescheitert. Er bleibt
stehen, weil die Widerlegung die eigentliche Erkenntnis ist.**

Geplant war: ein `SustainGCD`-Slot im Dispatch zwischen den Heilmethoden und
`GeneralGCD`, damit proaktive Logik einmal statt dreimal geschrieben wird.

**Was dafür sprach, und was sich bestätigt hat:** `base.HealAreaGCD` und
`base.HealSingleGCD` tun nichts — sie setzen zwei Flags und liefern `false`
(`CustomRotation_GCD.cs:758-800`). Ein Verschieben aus „vor `base.X`" nach
„nachdem `X` false lieferte" wäre also tatsächlich verhaltensgleich gewesen.

**Was dagegen sprach und den Punkt gekippt hat:** Bei WHM steht der
Sustain-Aufruf in `HealSingleGCD` nicht am Ende, sondern **zwischen** dem
reaktiven Regen und Cure II. Zieht man ihn heraus, gewinnt Cure II diese GCD,
und der Sustain verhungert wieder, sobald der Tank Schaden nimmt — exakt die
vom Nutzer gemeldete und inzwischen behobene Regression.

**Die eigentliche Erkenntnis:** Die drei Aufrufstellen je Heiler sind **keine
Duplikate**. Die *Bedingung* liegt seit `6b40600` in genau einem Helfer je Job.
Was dreifach dasteht, ist die **Position**, und die ist bewusst verschieden:

| Methode | Position | Aussage |
|---|---|---|
| `GeneralGCD` | zuerst | Sustain schlägt Schaden |
| `HealSingleGCD` | nach reaktivem HoT, vor Cure II | Sustain schlägt Direktheilung, aber nicht den reaktiven HoT |
| `HealAreaGCD` | zuletzt | Sustain verliert gegen jede AoE-Heilung |

Drei Positionen sind drei Prioritätsaussagen. Ein zentraler Slot kann nur eine
davon ausdrücken und löscht die anderen beiden stillschweigend. Das ist
Informationsverlust, nicht Entdopplung.

**Damit ist U2 anders zu bewerten als in `03-universal.md` beschrieben:** Die
Wiederholung ist der Preis dafür, dass ein Job seine Prioritäten pro
Dispatch-Slot selbst setzen kann. Der teure Teil — dieselbe *Bedingung*
dreimal zu pflegen — ist bereits beseitigt.

## A2 · ~~`FirstUsable` statt Level-Ketten~~ → VERWORFEN, ersetzt durch A2′

**Der ursprüngliche Vorschlag hielt der Messung nicht stand und ist ersetzt.
Er bleibt hier stehen, weil die Widerlegung nützlicher ist als der Vorschlag.**

Begründet war A2 mit „52 Kettenglieder in 12 Dateien". Diese Zahl war **falsch
gemessen**: der Regex zählte jedes `!X.EnoughLevel &&`, und das ist
überwiegend gar keine Kette, sondern level-gestaffelte Logik — SAMs
Kenki-Schwelle, DRGs und GNBs Burst-Timings, NINs Mudra-Bedingungen.

Nachgemessen mit einem Parser, der Ketten als Ketten erkennt:

| | |
|---|---|
| echte Aufstiegsketten | **65** in 16 Dateien |
| davon sauber gleichförmig konvertierbar | **25** |
| nicht konvertierbar | **40** |

Die 40 scheitern an zwei Dingen: die Gate-Aktion ist oft eine **andere** als
die gecastete (`!SummonIfritPvE… && RubyRuinPvE.CanUse` — SMN prüft den
Beschwörungs-Level und castet den passenden Elementar-Ruin), und der
Prädikat-Typ wechselt innerhalb derselben Kette zwischen `EnoughLevel` und
`Info.EnoughLevelAndQuest()`.

**Warum das den Vorschlag kippt, nicht nur verkleinert:** Eine Konvertierung
des sauberen Viertels ersetzt *ein* Idiom durch *zwei*. Das Ziel war
„hochstrukturiert und selbsterklärend" — zwei nebeneinander bestehende
Schreibweisen für dieselbe Sache sind das Gegenteil. Und die Fehlerklasse,
derentwegen A2 überhaupt vorgeschlagen war, bliebe an 40 Stellen bestehen.

## A2′ · Wächter gegen widersprüchliche Level-Prädikate

Das eigentliche Ziel war nie „weniger Zeilen", sondern „diese Fehlerklasse
unmöglich machen". Das leistet ein Wächter **vollständig** und ohne
Produktivcode, während der Umbau es nur teilweise leistet.

`!X.EnoughLevel && X.CanUse(...)` kann nie wahr werden — der Zweig ist tot.
Das ist exakt die Form des RDM-Impact-Bugs. Jetzt Build-Fehler.

**Vom Critic erzwungene Verengung:** Bedingungen mit `||` werden übersprungen.
Das im Repo verbreitete und **korrekte** Idiom ist die explizite Level-Klammer
`(X.EnoughLevel && …) || !X.EnoughLevel`, in der beide Terme vorkommen, aber
in verschiedenen Ästen. Ohne echten Ausdrucksparser wären das alles
Fehlalarme — die erste Fassung meldete 16 Treffer, alle 16 falsch.

**Aufwand:** ~30 Zeilen im bereits vorhandenen Prüfskript, kein Produktivcode,
keine berührte Rotationsdatei.

## A3 · Base-Call-Prüfung in der CI

**Problem (U6):** `return base.FalscheMethode(out act);` kompiliert, ist im
Diff unsichtbar und war mit **neun** Fällen die häufigste Fehlerklasse im
gesamten AUDIT_LOG.

**Ziel:** Kein Laufzeitcode. Ein Prüfskript im vorhandenen Build-Workflow:
jede `override bool X(...)` darf im `base.`-Aufruf nur `X` nennen. Ausnahmen
über eine kurze Allowlist mit Begründung.

Die Machbarkeit ist belegt — der Parser, der die Skelette für `01-jobs.md`
extrahiert hat, ist genau dieser Parser.

**Aufwand:** ~40 Zeilen Skript, ein Schritt in `build.yaml`, kein Produktivcode.

## A4 · Stufen-Vokabular — als KONVENTION, nicht als Refactoring

**Problem (U1):** `GeneralGCD` hat zwischen 7 und 81 Zweige auf einer Ebene,
Median ~19. Keine lesbare Untergliederung.

**Das Vokabular** (jede Stufe eine private Methode, Name aus dieser Liste):

| Stufe | Name | Bedeutung |
|---|---|---|
| 0 | `…ShortCircuit` | verlässt vor jeder Rotationslogik |
| 1 | `…Recovery` | rettet einen laufenden Zustand (Combo, Buff) |
| 2 | `…Resource` | Overcap-Schutz / Ressourcenverbrauch |
| 3 | `…Burst` | nur im Burst-Fenster |
| 4 | `…Dot` | Ziel-Debuff aufrechterhalten |
| 5 | `…Aoe` | an Gegneranzahl gekoppelt |
| 6 | `…Combo` | reihenfolge-/positionsgebunden |
| 7 | `…Filler` | Standardaktion |
| 8 | `…Downtime` | kein Ziel / außerhalb Kampf |

Zielbild:

```csharp
protected override bool GeneralGCD(out IAction? act)
{
    if (RaiseShortCircuit(out act)) return true;
    if (LilyResource(out act))      return true;
    if (AeroDot(out act))           return true;
    if (HolyAoe(out act))           return true;
    if (GlareFiller(out act))       return true;
    return base.GeneralGCD(out act);
}
```

Dass das trägt, belegt BLM im Repo selbst: **7** Zweige für dieselbe fachliche
Abdeckung, für die PCT **33** braucht — allein durch benannte Methoden.

**Warum trotzdem kein Refactoring der Bestandsdateien.** Zwei Gründe, beide
gemessen:

1. **Merge-Kosten.** Dies ist ein Fork, der `upstream/main` nachzieht.
   Upstream hat in 90 Tagen **29 Commits** auf `RebornRotations/` gelegt —
   etwa alle drei Tage einer. Die meistgeänderten Dateien sind genau die, die
   A4 am stärksten umbauen würde: VPR 7, RDM 6, DRK 5, NIN 4, SMN 4, AST 4.
   Ein Umbau dieser Dateien macht jeden künftigen Upstream-Merge zur
   Handarbeit — dauerhaft, nicht einmalig.
2. **Teilanwendung ist selbstzerstörerisch.** Der Nutzen ist ein *gemeinsames*
   Vokabular. Eines, das die Hälfte der Jobs benutzt, ist keins — dasselbe
   Argument, das A2 gekippt hat.

**Was bleibt:** Das Vokabular gilt als **Konvention für fork-eigenen und neu
geschriebenen Code**. Kosten null, und es greift dort, wo die Eigenkomplexität
dieses Forks tatsächlich liegt. Faktisch ist es dort bereits angewandt:
`ShouldSustainMitigationDebuff`, `TrySustain…OnTank`, `TryAddleBeforeDamage`,
`SwiftRaisePending` sind genau solche benannten Stufen.

---

# B · Gruppenebene

Alle B-Punkte sind **benannte Helfer auf `CustomRotation`**, keine neue
Vererbung. Sie werden erst nach A umgesetzt, weil A2/A4 sie kürzer machen.

## B1 · Heiler — `SwiftRaisePending`

13 wortgleiche Kopien von
`(HasSwift || IsLastAction(SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise)`
in vier Dateien → eine Definition, 13 Verwendungen von einem Wort.

## B2 · Tanks — `TryRangedPull(out act)`

Vier strukturgleiche Endzweige (Tomahawk · Lightning Shot · Shield Lob ·
Unmend), jeweils letzte Zeile vor `base.GeneralGCD`. Ein Helfer, der die
job-eigene Aktion über eine bereits vorhandene abstrakte Eigenschaft zieht.

## B3 · Tanks — Reprisal-Platzierung vereinheitlichen

DRK/GNB haben den Sustain in Area **und** Single, PLD/WAR nur in Single. Die
Ursache ist die Upstream-Platzierung von Reprisal je Job, also begründet — aber
das Ergebnis ist, dass dieselbe Fähigkeit rollenintern uneinheitlich reagiert.
**Bewusst als offene Frage geführt, nicht blind angeglichen:** die Angleichung
erfordert eine Spielentscheidung, keine Code-Entscheidung.

## B4 · Phys. Fernkämpfer — Slot-Mengen angleichen

Keine zwei der drei Jobs belegen dieselben Slots (DNC ohne
`DefenseSingleAbility`, MCH ohne `HealSingleAbility`, BRD als einziger mit
`DispelAbility`). Erst prüfen, ob das fachlich begründet ist; nur dann
angleichen. Reihenfolge: prüfen → begründen → erst danach ändern.

## B5 · Melee — MNK-Heilslot

MNK überschreibt `HealAreaAbility` statt `HealSingleAbility`, obwohl Second
Wind eine Einzelziel-Selbstheilung ist. Einzige Gruppenabweichung ohne
erkennbare Begründung. Vor Änderung im Spiel prüfen.

---

# C · Jobebene

Erst nach A und B, und nur dort, wo nach A4 noch etwas übrig ist.

| Job | Punkt |
|---|---|
| DRG | 8 `if (Trait.EnoughLevel)/(!Trait…)`-Paare hintereinander → nach A2 datengetrieben |
| PCT | 33 Zweige, 12 Farbaktionen in sechs parallelen Strängen → nach A2/A4 auf ~8 Stufen |
| BLU | 81 Zweige auf einer Ebene → A4 anwenden, sonst unverändert lassen |
| MCH | 12 Level-Kettenglieder, die meisten aller Jobs → reiner A2-Fall |
| DRG · VPR | kein `CountDownAction` — prüfen, ob Lücke oder Absicht |
| RPR · SAM · WAR | kein `EmergencyAbility` — dito |

---

## Der Audit-Verlauf, der zu diesem Stand geführt hat

### Council

**Wartender:** Was 23-mal identisch dasteht, gehört einmal dazustehen.
**Spieler:** Interessiert nur, ob das Verhalten gleich bleibt. Ein Umbau, der
das Spielgefühl ändert, ist ein schlechter Umbau, egal wie sauber er aussieht.
**Upstream-Reviewer:** Jede Änderung an der Dispatch-Kette betrifft jeden Job.
Das ist die teuerste Änderungsklasse überhaupt.
**Job-Autor:** Ich will meinen Job lesen können, ohne die Basisklasse zu
kennen. Zu viel Zentralisierung nimmt mir das.

### Critic

1. **A1 ist genau das, was schon einmal gescheitert ist.** TODO #40:
   „zentraler Trigger hat zu großen Blast-Radius — bereits versucht+reverted".
2. **A2 allokiert.** `params IBaseAction[]` im Per-Frame-Pfad, in einem Repo,
   das überall `foreach` statt LINQ schreibt, um genau das zu vermeiden.
3. **A4 fasst 23 Dateien an.** Das ist das Gegenteil von codearm.
4. **A3 produziert Fehlalarme**, wo eine Methode absichtlich eine andere
   `base`-Methode ruft.

### Antithese

Zu 1: Der Unterschied ist prüfbar, nicht rhetorisch. #40 hat einen **Trigger**
eingebaut, der bei allen Jobs *zusätzliche Aktionen auslöste*. A1 fügt einen
**leeren Slot** ein (`=> false`). Für 20 von 23 Jobs ändert sich exakt nichts —
das ist im Diff nachweisbar, nicht Auslegungssache.

Zu 2: Berechtigt, und keine Grundsatzfrage, sondern eine Signaturfrage. Feste
Überladungen lösen es vollständig.

Zu 3: A4 ist **opt-in und dateiweise**. Kein Job muss migriert werden, damit
ein anderer davon profitiert. Und die Zeilenbilanz ist negativ, nicht positiv.

Zu 4: Berechtigt. Allowlist mit Begründungspflicht — das ist Standard für
solche Prüfungen und kostet eine Zeile pro Ausnahme.

### Evaluation

Der schwerste Einwand ist Nr. 1, und er wird durch das Leer-Slot-Argument
entkräftet — aber nur, weil es überprüfbar ist. **Deshalb gilt als
Umsetzungsbedingung für A1:** der Diff muss zeigen, dass für Jobs ohne
`SustainGCD`-Override kein Verhalten entsteht. Lässt sich das nicht zeigen,
fällt A1.

Nr. 2 hat die Signatur geändert (das ist die Revision, kein Zugeständnis).

Nr. 3 und 4 stehen, sind aber durch Zuschnitt (opt-in, Allowlist) beherrschbar.

Was der Loop **nicht** geleistet hat: keiner dieser Punkte ist spielgetestet.
Alle Zahlen sind aus dem Code gezählt, alle Wirkungen statisch hergeleitet.

### Revision (Stand jetzt)

- A2 von `params` auf feste Überladungen geändert.
- A1 um die explizite Abnahmebedingung „Diff zeigt Wirkungslosigkeit für
  Nicht-Nutzer" ergänzt.
- A3 um Allowlist ergänzt.
- B3/B4/B5 von „vereinheitlichen" auf „prüfen, dann entscheiden"
  zurückgestuft — es sind Spielfragen, keine Codefragen, und sie ungeprüft
  anzugleichen wäre derselbe Fehler wie bei der Provoke-Distanz.

---

## Umsetzungsreihenfolge

| Schritt | Inhalt | Zeilen ± | Risiko | Prüfbar durch |
|---|---|---|---|---|
| 1 | A3 CI-Prüfung | +40 (nur CI) | keins | findet die 9 historischen Fälle |
| 2 | A2′ Level-Prädikat-Wächter | +30 (nur CI) | keins | Fixture + sauberer Lauf |
| 3 | ~~A1 Sustain-Slot~~ | entfällt | – | verworfen, siehe A1 |
| 4 | A4 als Konvention | 0 | keins | gilt für neuen Code |
| 5 | B1 · B2 | −20 | gering | CI-Build |
| 6 | B3 · B4 · B5 prüfen | 0 | – | Spieltest, dann entscheiden |
| 7 | C, was übrig bleibt | negativ | gering | CI-Build |

Schritt 1 und 2 sind reine Gewinne ohne Verhaltensänderung und sollten zuerst
kommen — sie sichern alle folgenden Schritte ab.
