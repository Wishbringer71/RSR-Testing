# 04 · Zielkonzept

Ergebnis des internen Audits (Council → Critic → Antithese → Evaluation →
Revision). Dokumentiert ist der Endstand plus die Gegenargumente, die ihn
geformt haben — nicht der geglättete Verlauf.

**Reihenfolge nach Wirkbreite:** A wirkt auf alle Jobs, B auf eine Gruppe,
C auf einen Job. Später kommende Stufen setzen frühere voraus, nie umgekehrt.

---

## Prämisse dieses Branches (korrigiert — die erste Fassung war falsch)

Die erste Fassung dieses Dokuments hat mehrere Punkte mit **Merge-Kosten**
begründet: der Fork ziehe `upstream/main` nach, also mache struktureller Umbau
jeden künftigen Merge zur Handarbeit.

**Diese Prämisse gilt für diesen Branch nicht.** Er setzt keine
Code-Kompatibilität zum Original mehr voraus. Künftige Upstream-Commits werden
auf ihren **Inhalt** geprüft — welche Verbesserung, welche Fehlerbehebung,
welche Erweiterung bringen sie — und inhaltlich nachgezogen, nicht als Patch
appliziert. Damit ist der Merge ohnehin Handarbeit, unabhängig davon, wie die
Datei hier strukturiert ist. Der Zusatzaufwand durch Umbau ist ~0.

Jede Ablehnung, die **nur** auf Merge-Kosten beruhte, ist damit hinfällig und
unten neu entschieden. Ablehnungen aus anderen Gründen bleiben — sie werden
unten explizit als solche gekennzeichnet, damit erkennbar ist, welche
Begründung noch trägt.

---

## Randbedingung, die alles andere begrenzt

```
CustomRotation                     handgeschrieben, gemeinsam
      ↓
{Job}Rotation                      partial: eine Hälfte generiert, eine handgeschrieben
      ↓
{Job}_Reborn                       handgeschrieben, je Job
```

**Faktenlage (nachgeprüft, die erste Fassung war hier ungenau):**

| Artefakt | Ort | Status |
|---|---|---|
| `public partial class WhiteMageRotation` (Gauge, Job-Helfer) | `RotationSolver.Basic/Rotations/Basic/*.cs`, 23 Dateien | handgeschrieben, eingecheckt |
| `public abstract partial class WhiteMageRotation : CustomRotation` (alle Aktionen, `AllBaseActions`, `AllTraits`) | `RotationSolver.SourceGenerators/Properties/Rotation.resx`, 23× in 1,98 MB | **generierter Text**, eingecheckt |
| Emission zur Compile-Zeit | `StaticCodeGenerator.GenerateRotations` (Analyzer-Referenz in `RotationSolver.Basic.csproj`) | – |
| Erzeugung der resx | `RotationSolver.GameData/Program.cs` → `RotationGetter` | offline, **braucht `C:\FF14\game\sqpack`** |

Es gibt **keine Rollenebene**, und sie bleibt abgelehnt — aber die Begründung
ist eine andere als in der ersten Fassung:

1. Die Basisklasse `: CustomRotation` steht im **generierten** Teil der
   `partial class`. C# erlaubt die Basisklassenangabe nur in einem Teil (oder
   identisch in mehreren); ein `: HealerRotation` in der handgeschriebenen
   Hälfte ist deshalb ein Compilefehler, kein Ausweg.
2. Sie zu ändern heißt, `Rotation.resx` von Hand zu editieren. Das ist ein
   **generiertes Artefakt** — jede Neuerzeugung (Spielpatch, neue Aktionen)
   überschreibt die Änderung wortlos. Dieser Konflikt hat nichts mit
   Upstream-Kompatibilität zu tun und überlebt die Prämissenkorrektur.
3. Neu erzeugen kann dieser Branch die resx nicht: der Generator liest die
   Spieldateien, die hier nicht vorliegen.
4. Und selbst wenn: eine Rollenebene leistet nichts, was ein rollenbenannter
   Helfer auf `CustomRotation` nicht auch leistet. Das war schon in der ersten
   Fassung der zweite Grund und ist von der Prämisse unabhängig.

**Entscheidung unverändert, Begründung ersetzt: keine neue Vererbungsebene.**
Rollenlogik lebt als rollenbenannter Helfer, so wie
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

## A4 · Stufen — NEU BEWERTET nach der Prämissenkorrektur

Die erste Fassung hat A4 auf eine reine Konvention für neuen Code
zurückgestuft. Der erste ihrer beiden Gründe war **Merge-Kosten** und ist
hinfällig. Der zweite („Teilanwendung ist selbstzerstörerisch") war nie ein
Argument gegen A4, sondern eines gegen die *halbe* A4 — er verlangt
Vollanwendung, und die war unter der alten Prämisse nicht bezahlbar. Jetzt ist
sie es. **A4 wird deshalb vollständig neu entschieden, nicht nur reaktiviert.**

**Problem (U1), nachgemessen über alle 31 PvE-Rotationsdateien:**

| | |
|---|---|
| Zweige auf oberster Ebene, alle Dispatch-Methoden zusammen | **1239** |
| `GeneralGCD`-Zweige: Median / Maximum | **16** / **80** (BLU) |
| über 30 Zweige in `GeneralGCD` | BLU 80, PhantomDefault 33, PCT 32 |

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

### Der Loop zu A4, zweiter Durchgang (ohne Merge-Kosten-Argument)

**Council.** *Wartender:* 1239 Zweige auf einer Ebene sind der größte
verbleibende Strukturmangel; nichts anderes im Konzept wirkt so breit.
*Job-Autor:* Ich will beim Lesen sehen, was mein Job tut, nicht eine Taxonomie
wiedererkennen. *Spieler:* Verhalten muss identisch bleiben, sonst ist es egal,
wie es aussieht.

**Critic — der Einwand, der neu ist und nichts mit Merge-Kosten zu tun hat.**
Der Beleg für A4 ist BLM_Default. Aber was BLM_Default tatsächlich tut, ist
nicht das vorgeschlagene Vokabular. Seine 14 privaten Helfer heißen `GoIce`,
`MaintainIce`, `DoFire`, `AddThunder`, `UsePolyglot`, `MaintainStatus` — Namen
**aus der Fachlogik des Jobs**, nicht aus einer festen Neunerliste. Der
Kronzeuge für A4 widerlegt A4s Namensteil. `GoIce` sagt mehr als
`IceRecovery`; eine erzwungene Taxonomie würde diese Datei *verschlechtern*.

**Critic, zweiter Einwand.** „Mechanische Extraktion ist verhaltensgleich" gilt
nicht ausnahmslos. In BLM_Default steht

```csharp
if (InFireOrIce(out act, out var mustGo)) return true;
if (mustGo) return false;                 // Abbruch der GANZEN Methode
```

Ein `return false` innerhalb einer herausgezogenen Region bedeutet dort nicht
„diese Stufe greift nicht", sondern „keine GCD in diesem Frame". Das überlebt
die Extraktion nur mit einem zusätzlichen `out`-Parameter — und der ist selbst
ein Lesbarkeitsverlust. Dasselbe gilt für Locals, die über die Schnittgrenze
hinweg benutzt werden (DRG: `doomSpikeRightNow`).

**Antithese.** Beide Einwände treffen **verschiedene Hälften** von A4. Der
erste trifft die Namensvorschrift, der zweite die Extraktion. Sie fallen nicht
gemeinsam. Die Extraktion ist der Teil mit dem Nutzen (BLM 6 Zweige gegen PCT
32 für vergleichbare fachliche Abdeckung); die Namensvorschrift ist der Teil
mit dem Streit. Und der zweite Einwand ist kein Ablehnungsgrund, sondern eine
**Prüfpflicht pro Datei**: Locals über die Schnittgrenze und `return false`
mit Abbruchbedeutung sind beide statisch auffindbar.

**Evaluation.** Was den ersten Einwand trägt, ist nicht „Domänennamen sind
schöner", sondern: die Neunerliste beschreibt in Wahrheit keine *Namen*,
sondern eine *Reihenfolge*. Recovery vor Resource vor Burst vor Dot vor Aoe vor
Combo vor Filler ist eine Prioritätsaussage, und die ist tatsächlich über alle
Jobs vergleichbar. Der Name dagegen ist die Information des Jobs. Beides in
einen Bezeichner zu zwingen war der Fehler.

**Revision — A4 zerfällt in zwei Punkte mit verschiedenem Status:**

| | Inhalt | Status |
|---|---|---|
| **A4a** | Zweige auf oberster Ebene zu benannten privaten Methoden zusammenfassen, Datei für Datei | **umzusetzen**, breitester verbleibender Punkt |
| **A4b** | Die Neunerliste gilt als **Reihenfolge**, nicht als Namensvorschrift. Namen kommen aus der Fachlogik des Jobs. | Konvention |

**Abnahmebedingungen für A4a, pro Datei, aus dem Critic-Einwand abgeleitet:**

1. Keine Extraktion über eine Local hinweg, die vor und nach der Schnittgrenze
   gelesen wird — sonst zuerst die Local in die Stufe hineinziehen.
2. Jedes `return false` in der extrahierten Region prüfen: „Stufe greift nicht"
   (unkritisch) oder „Methode abbrechen" (braucht `out`-Flag, wie
   `InFireOrIce`). Im Zweifel Datei überspringen.
3. Reihenfolge der Zweige bleibt exakt erhalten. Der Diff muss zeigen, dass
   nur verschoben wurde.
4. CI-Build grün, und die beiden Wächter (A3, A2′) laufen ohnehin mit.

**Reihenfolge nach Nutzen pro Datei:** BLU (80) → PhantomDefault (33) →
PCT (32) → SAM (27) → MCH/SMN (23). Unter ~15 Zweigen lohnt es nicht.

**Was von der alten Fassung bleibt:** Die Konvention galt schon bisher für
fork-eigenen Code, und dort ist sie angewandt —
`ShouldSustainMitigationDebuff`, `TrySustain…OnTank`, `TryAddleBeforeDamage`,
`SwiftRaisePending` sind genau solche benannten Stufen mit Domänennamen. A4b
schreibt nur fest, was ohnehin praktiziert wurde.

---

# B · Gruppenebene

Alle B-Punkte sind **benannte Helfer auf `CustomRotation`**, keine neue
Vererbung. Sie werden erst nach A umgesetzt, weil A2/A4 sie kürzer machen.

## B1 · Heiler — `SwiftRaisePending`

13 wortgleiche Kopien von
`(HasSwift || IsLastAction(SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise)`
in vier Dateien → eine Definition, 13 Verwendungen von einem Wort.

## B2 · ~~Tanks — `TryRangedPull(out act)`~~ → VERWORFEN, kein Nutzen

Die vier Endzweige stehen bereits in der minimalen Form:

```csharp
if (TomahawkPvE.CanUse(out act)) { return true; }   // WAR, analog GNB/PLD/DRK
```

Es gibt **keine geteilte Bedingung** zum Herausziehen — anders als bei B1, wo
ein langer Ausdruck viermal wortgleich dastand. Der einzige Unterschied ist der
Aktionsname, und der ist die eigentliche Information.

Ein gemeinsamer Helfer bräuchte je Job ein
`protected override IBaseAction RangedPull => TomahawkPvE;`: gleiche Zeilenzahl,
zusätzlich ein neues abstraktes Mitglied auf `CustomRotation`, und beim Lesen
ein Sprung in die Basisklasse für eine Information, die vorher direkt dastand.
Strikt schlechter in allen drei Kriterien des Auftrags.

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

# C · Jobebene — NEU BEWERTET

Die erste Fassung hat C pauschal mit Merge-Kosten verworfen. Das ist hinfällig.
Nach der Neuprüfung zerfällt C in drei Gruppen mit verschiedenem Status:

| Job | Punkt | Status nach Neuprüfung |
|---|---|---|
| PCT | 32 Zweige, 12 Farbaktionen in sechs parallelen Strängen | **A4a-Fall**, Rang 3 |
| BLU | 80 Zweige auf einer Ebene | **A4a-Fall**, Rang 1 |
| SAM · MCH · SMN | 27 / 23 / 23 Zweige | **A4a-Fälle**, Rang 4–6 |
| DRG | 8 `Trait.EnoughLevel`/`!Trait…`-Paare | **offen, Spielfrage** — s.u. |
| MCH | 12 Level-Kettenglieder | folgt A2, und A2 steht verworfen — kein eigener Punkt mehr |
| DRG · VPR | kein `CountDownAction` | offen, prüfen ob Lücke oder Absicht |
| RPR · SAM · WAR | kein `EmergencyAbility` | offen, dito |

Damit bleibt von C als eigenständiger Punkt nur das DRG-Trait-Muster übrig —
alles andere ist entweder ein A4a-Fall oder eine offene Spielfrage.

## C1 · DRG-Trait-Paare — der Fund, der die Vereinfachung blockiert

Achtmal steht in `DRG_Reborn.cs` dasselbe Muster (Zeilen 288–405):

```csharp
if (LanceMasteryIiTrait.EnoughLevel)  { if (HeavensThrustPvE.CanUse(out act)) return true; }
if (!LanceMasteryIiTrait.EnoughLevel) { if (FullThrustPvE.CanUse(out act))    return true; }
```

Die naheliegende Vereinfachung ist, die Gates zu streichen — `CanUse` prüft
`EnoughLevel` bereits selbst (`ActionBasicInfo.cs:452`):

```csharp
if (HeavensThrustPvE.CanUse(out act)) return true;
if (FullThrustPvE.CanUse(out act))    return true;
```

**Das ist nicht verhaltensgleich, und der Unterschied ist kein Randfall.** Die
gegateten Fassungen schließen einander aus; die ordnende Fassung ist ein
Fallback. Sie unterscheiden sich genau dann, wenn `HeavensThrustPvE.CanUse` aus
einem **anderen Grund als dem Level** fehlschlägt — Combo nicht offen,
Reichweite, Status. Dann versucht die zweite Fassung zusätzlich die
Vorgängeraktion, die erste nicht.

Ob das schadet, hängt daran, ob `FullThrustPvE.CanUse` oberhalb der Traitstufe
überhaupt noch `true` liefern kann. `BaseAction.Use()` castet `ID`, nicht
`AdjustedID` (`BaseAction.cs:278/301`), verlässt sich also auf die
Aktionsersetzung des Spiels — der Cast wäre folgenlos richtig, aber die
Combo-Buchführung von RSR läuft über die andere Aktion.

**Ohne Spielbeobachtung nicht entscheidbar. Nicht ungeprüft vereinfachen** —
das wäre exakt der Fehler von #56 (Provoke-Distanz).

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

| Schritt | Inhalt | Zeilen ± | Risiko | Prüfbar durch | Stand |
|---|---|---|---|---|---|
| 1 | A3 CI-Prüfung | +40 (nur CI) | keins | findet die 9 historischen Fälle | erledigt |
| 2 | A2′ Level-Prädikat-Wächter | +30 (nur CI) | keins | Fixture + sauberer Lauf | erledigt |
| 3 | ~~A1 Sustain-Slot~~ | entfällt | – | verworfen, siehe A1 | – |
| 4 | B1 `SwiftRaisePending` | −20 | gering | CI-Build | erledigt |
| 5 | **A4a**, Datei für Datei: BLU → PhantomDefault → PCT → SAM → MCH → SMN | negativ | mittel, pro Datei | Abnahmebedingungen 1–4 unter A4 + CI-Build | **offen** |
| 6 | B3 · B4 · B5 · C1 im Spiel prüfen | 0 | – | Spielbeobachtung, dann entscheiden | offen |

Schritt 1 und 2 sind reine Gewinne ohne Verhaltensänderung und kamen deshalb
zuerst — sie sichern Schritt 5 ab, der als einziger noch offener Punkt
tatsächlich Produktivcode bewegt.

Schritt 5 ist bewusst **eine Datei pro Commit**. Die Abnahmebedingungen sind
per Datei zu prüfen, nicht per Serie; eine Datei, deren Locals oder
`return false`-Semantik die Extraktion nicht zulassen, wird übersprungen und
hier vermerkt.


---

# Stand nach der Neubewertung

Jeder Punkt ist daraufhin geprüft worden, **worauf seine Ablehnung beruhte**.
Nur wer sich auf Merge-Kosten stützte, wurde neu entschieden.

| Punkt | Ergebnis | Beruhte die Ablehnung auf Merge-Kosten? |
|---|---|---|
| A3 Base-Call-Wächter | **umgesetzt** | – |
| A2′ Level-Prädikat-Wächter | **umgesetzt** | – |
| B1 `SwiftRaisePending` | **umgesetzt** | – |
| A1 Sustain-Slot | verworfen, **bleibt** | nein — Position ist Information |
| A2 `FirstUsable` | verworfen, **bleibt** | nein — nur 25 von 65 Ketten gleichförmig |
| B2 `TryRangedPull` | verworfen, **bleibt** | nein — es gibt nichts zu teilen |
| Rollenebene | verworfen, **Begründung ersetzt** | teilweise — trägt jetzt auf generiertem Artefakt |
| **A4a Stufen-Extraktion** | **reaktiviert, umzusetzen** | **ja — Ablehnung hinfällig** |
| A4b Stufen-Vokabular | zu Reihenfolge umgedeutet | eigener Critic-Befund, s. A4 |
| **C PCT/BLU/SAM/MCH/SMN** | **in A4a aufgegangen** | **ja — Ablehnung hinfällig** |
| C1 DRG-Trait-Paare | offen, Spielfrage | nein — neu gefundene Semantikfalle |
| B3/B4/B5 | offen, Spielfragen | nein |

## Die übergreifende Erkenntnis — ersetzt

Die alte Fassung schloss:

> ~~In einem Fork, der einen aktiven Upstream nachzieht, ist strukturelles
> Umbauen der nachgezogenen Dateien keine tragfähige Verbesserungsstrategie.~~

Diese Regel folgte aus der Prämisse, die für diesen Branch nicht gilt. Was
nach der Neuprüfung tatsächlich trägt, ist enger und hat nichts mit Upstream
zu tun:

> **Umgebaut wird handgeschriebener Code. Generierte Artefakte
> (`Rotation.resx` und alles, was der Source-Generator daraus emittiert)
> werden nicht von Hand geändert — sie werden vom nächsten Generatorlauf
> überschrieben, und der Generator ist hier nicht ausführbar.**

Und eine zweite, die aus dem zweiten Durchgang stammt:

> **Ablehnungsgründe sind zu protokollieren, nicht nur Ablehnungen.** Vier
> Punkte hier standen jahrelang „verworfen" da; ohne den vermerkten Grund wäre
> nach der Prämissenkorrektur nicht unterscheidbar gewesen, welche vier davon
> neu zu entscheiden sind und welche nicht.

## Was das für die offenen Punkte heißt

B3, B4, B5 und C1 sind keine Struktur-, sondern Verhaltensfragen
(Reprisal-Platzierung, Slot-Asymmetrien, MNK-Heilslot, DRG-Trait-Gates). Ihre
Diffs wären winzig. Was fehlt, ist die Spielbeobachtung — nicht die
Machbarkeit und nicht die Erlaubnis.
