# 04 · Zielkonzept

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Stand dar; die
Prüfhistorie steht in `AUDIT_LOG.md` (A7, Register #66).

## Ergebnis

Der strukturelle Umbau ist abgeschlossen. Was blieb, sind vier **Verhaltensfragen**,
die keine Codeentscheidung sind, sondern eine Spielbeobachtung brauchen.

| Punkt | Inhalt | Stand |
|---|---|---|
| A3 | Base-Call-Wächter in der CI | umgesetzt, `check_base_calls.py` in `build.yaml` |
| A2′ | Wächter gegen widersprüchliche Level-Prädikate | umgesetzt, dieselbe Prüfung |
| A4a | Zweige oberster Ebene zu benannten Stufen | umgesetzt für BLU, PhantomDefault, PCT, SAM, SMN; MCH bewusst ausgelassen |
| A4b | Die Stufenliste gilt als Reihenfolge, nicht als Namensvorschrift | Konvention, angewandt |
| B1 | `SwiftRaisePending` | umgesetzt |
| B3 · B4 · B5 · C1 | Reprisal-Platzierung, Slot-Asymmetrien, MNK-Heilslot, DRG-Trait-Gates | offen — Spielfragen |
| A1 · A2 · B2 · Rollenebene | — | verworfen, Begründung unten |

**Reihenfolge nach Wirkbreite:** A wirkt auf alle Jobs, B auf eine Gruppe, C auf einen
Job. Später kommende Stufen setzen frühere voraus, nie umgekehrt.

## Prämissen

### Merge-Kosten sind für diesen Branch kein Argument

Der Branch setzt keine Code-Kompatibilität zum Original voraus. Künftige
Upstream-Commits werden auf ihren **Inhalt** geprüft — welche Verbesserung, welche
Fehlerbehebung, welche Erweiterung sie bringen — und inhaltlich nachgezogen, nicht als
Patch appliziert. Der Merge ist damit ohnehin Handarbeit, unabhängig davon, wie die
Datei hier strukturiert ist; der Zusatzaufwand durch Umbau ist ~0.

Eine Ablehnung, die sich **nur** auf Merge-Kosten stützt, trägt hier deshalb nicht.
Die unten aufgeführten Ablehnungen tragen aus anderen Gründen.

### Generierte Artefakte werden nicht von Hand geändert

```
CustomRotation                     handgeschrieben, gemeinsam
      ↓
{Job}Rotation                      partial: eine Hälfte generiert, eine handgeschrieben
      ↓
{Job}_Reborn                       handgeschrieben, je Job
```

| Artefakt | Ort | Status |
|---|---|---|
| `public partial class WhiteMageRotation` (Gauge, Job-Helfer) | `RotationSolver.Basic/Rotations/Basic/*.cs`, 23 Dateien | handgeschrieben, eingecheckt |
| `public abstract partial class WhiteMageRotation : CustomRotation` (alle Aktionen, `AllBaseActions`, `AllTraits`) | `RotationSolver.SourceGenerators/Properties/Rotation.resx`, 23× in 1,98 MB | **generierter Text**, eingecheckt |
| Emission zur Compile-Zeit | `StaticCodeGenerator.GenerateRotations` | – |
| Erzeugung der resx | `RotationSolver.GameData/Program.cs` → `RotationGetter` | offline, **braucht `C:\FF14\game\sqpack`** |

> **Umgebaut wird handgeschriebener Code. Generierte Artefakte (`Rotation.resx` und
> alles, was der Source-Generator daraus emittiert) werden nicht von Hand geändert —
> sie werden vom nächsten Generatorlauf überschrieben, und der Generator ist hier
> nicht ausführbar.**

Eine zweite Regel folgt aus der Führung dieses Dokuments selbst:

> **Ablehnungsgründe sind zu protokollieren, nicht nur Ablehnungen.** Ohne den
> vermerkten Grund ist bei einer Prämissenänderung nicht unterscheidbar, welche
> Ablehnung neu zu entscheiden ist und welche weiter trägt.

## A · Global (alle Jobs)

### A3 · Base-Call-Prüfung in der CI

`return base.FalscheMethode(out act);` kompiliert, ist im Diff unsichtbar und war mit
**neun** Fällen die häufigste Fehlerklasse im gesamten `AUDIT_LOG`. Die Prüfung
verlangt, dass jede `override bool X(...)` im `base.`-Aufruf nur `X` nennt; Ausnahmen
stehen in einer Allowlist mit Begründung, deren gewünschter Zustand leer ist. Kein
Laufzeitcode.

### A2′ · Wächter gegen widersprüchliche Level-Prädikate

`!X.EnoughLevel && X.CanUse(...)` kann nie wahr werden — der Zweig ist tot. Das ist
exakt die Form des RDM-Impact-Bugs und jetzt ein Build-Fehler.

Die Prüfung überspringt Bedingungen mit `||`. Das im Repo verbreitete und **korrekte**
Idiom ist die explizite Level-Klammer `(X.EnoughLevel && …) || !X.EnoughLevel`, in der
beide Terme vorkommen, aber in verschiedenen Ästen; ohne echten Ausdrucksparser wären
das sämtlich Fehlalarme.

### A4a · Zweige oberster Ebene zu benannten Stufen

Gemessen über alle 31 PvE-Rotationsdateien lagen **1239** Zweige auf oberster Ebene,
im `GeneralGCD` Median 16 und Maximum 80 (BLU). Umgebaut sind die fünf Dateien mit dem
größten Nutzen: BLU, PhantomDefault, PCT, SAM, SMN. `GeneralGCD` ist dort ein
Dispatcher über benannte Stufen; eingefügt wurden nur Methodengrenzen (208 +, 5 −, 0
verschoben).

**MCH ist bewusst ausgelassen:** Ein `return base` mitten in der Kette bricht die
ganze Methode ab; in eine Stufe verschoben liefe die nächste weiter. Das ist keine
verhaltensgleiche Extraktion.

Die Abnahmebedingungen, die diese Auslassung gefunden haben, gelten **pro Datei**:

1. Keine Extraktion über eine Local hinweg, die vor und nach der Schnittgrenze gelesen
   wird — sonst zuerst die Local in die Stufe hineinziehen.
2. Jedes `return false` in der extrahierten Region prüfen: „Stufe greift nicht"
   (unkritisch) oder „Methode abbrechen" (braucht ein `out`-Flag). Im Zweifel Datei
   überspringen.
3. Reihenfolge der Zweige bleibt exakt erhalten; der Diff muss zeigen, dass nur
   verschoben wurde.
4. CI-Build grün, die beiden Wächter laufen ohnehin mit.

Unter etwa 15 Zweigen lohnt der Umbau nicht.

### A4b · Die Stufenliste ist eine Reihenfolge, kein Namensschema

| Stufe | Bedeutung |
|---|---|
| 0 | verlässt vor jeder Rotationslogik |
| 1 | rettet einen laufenden Zustand (Combo, Buff) |
| 2 | Overcap-Schutz / Ressourcenverbrauch |
| 3 | nur im Burst-Fenster |
| 4 | Ziel-Debuff aufrechterhalten |
| 5 | an Gegneranzahl gekoppelt |
| 6 | reihenfolge-/positionsgebunden |
| 7 | Standardaktion |
| 8 | kein Ziel / außerhalb Kampf |

Diese Ordnung — Recovery vor Resource vor Burst vor Dot vor Aoe vor Combo vor Filler —
ist eine Prioritätsaussage und über alle Jobs vergleichbar. **Der Name dagegen ist die
Information des Jobs.** `BLM_Default` zeigt es: Seine Helfer heißen `GoIce`,
`MaintainIce`, `DoFire`, `AddThunder`, `UsePolyglot` — `GoIce` sagt mehr als
`IceRecovery`, und eine erzwungene Taxonomie würde die Datei verschlechtern. Beides in
einen Bezeichner zu zwingen wäre der Fehler; die Reihenfolge ist gemeinsam, der Name
gehört dem Job.

Die Konvention galt schon zuvor für fork-eigenen Code und ist dort angewandt —
`ShouldSustainMitigationDebuff`, `TrySustain…OnTank`, `TryAddleBeforeDamage`,
`SwiftRaisePending` sind genau solche benannten Stufen mit Domänennamen.

## B · Gruppenebene

Alle B-Punkte sind **benannte Helfer auf `CustomRotation`**, keine neue Vererbung.

### B1 · Heiler — `SwiftRaisePending` · umgesetzt

13 wortgleiche Kopien von
`(HasSwift || IsLastAction(SwiftcastPvE)) && SwiftLogic && MergedStatus.HasFlag(AutoStatus.Raise)`
in vier Dateien → eine Definition, 13 Verwendungen von einem Wort.

### B3 · Tanks — Reprisal-Platzierung · offen

DRK/GNB haben den Sustain in Area **und** Single, PLD/WAR nur in Single. Die Ursache
ist die Upstream-Platzierung von Reprisal je Job, also begründet — aber das Ergebnis
ist, dass dieselbe Fähigkeit rollenintern uneinheitlich reagiert. Die Angleichung
erfordert eine Spielentscheidung, keine Code-Entscheidung.

### B4 · Physische Fernkämpfer — Slot-Mengen · offen

Keine zwei der drei Jobs belegen dieselben Slots (DNC ohne `DefenseSingleAbility`, MCH
ohne `HealSingleAbility`, BRD als einziger mit `DispelAbility`). Reihenfolge: prüfen →
begründen → erst danach ändern.

### B5 · Melee — MNK-Heilslot · offen

MNK überschreibt `HealAreaAbility` statt `HealSingleAbility`, obwohl Second Wind eine
Einzelziel-Selbstheilung ist. Einzige Gruppenabweichung ohne erkennbare Begründung.

## C · Jobebene

Nach der Zuordnung zu A4a bleibt von C ein einziger eigenständiger Punkt.

| Job | Punkt | Zuordnung |
|---|---|---|
| BLU · PhantomDefault · PCT · SAM · SMN | 80 / 33 / 32 / 27 / 23 Zweige | A4a, umgesetzt |
| MCH | 23 Zweige | A4a, bewusst ausgelassen |
| DRG | 8 `Trait.EnoughLevel`/`!Trait…`-Paare | **offen, Spielfrage** |
| DRG · VPR | kein `CountDownAction` | offen, prüfen ob Lücke oder Absicht |
| RPR · SAM · WAR | kein `EmergencyAbility` | offen, dito |

### C1 · DRG-Trait-Paare — der Fund, der die Vereinfachung blockiert

Achtmal steht in `DRG_Reborn.cs` dasselbe Muster:

```csharp
if (LanceMasteryIiTrait.EnoughLevel)  { if (HeavensThrustPvE.CanUse(out act)) return true; }
if (!LanceMasteryIiTrait.EnoughLevel) { if (FullThrustPvE.CanUse(out act))    return true; }
```

Die naheliegende Vereinfachung ist, die Gates zu streichen — `CanUse` prüft
`EnoughLevel` bereits selbst (`ActionBasicInfo.cs:452`). **Das ist nicht
verhaltensgleich, und der Unterschied ist kein Randfall.** Die gegateten Fassungen
schließen einander aus; die ordnende Fassung ist ein Fallback. Sie unterscheiden sich
genau dann, wenn `HeavensThrustPvE.CanUse` aus einem **anderen Grund als dem Level**
fehlschlägt — Combo nicht offen, Reichweite, Status. Dann versucht die ordnende
Schreibweise zusätzlich die Vorgängeraktion, die gegatete nicht.

Ob das schadet, hängt daran, ob `FullThrustPvE.CanUse` oberhalb der Traitstufe
überhaupt noch `true` liefern kann. `BaseAction.Use()` castet `ID`, nicht `AdjustedID`
(`BaseAction.cs:278/301`), verlässt sich also auf die Aktionsersetzung des Spiels — der
Cast wäre folgenlos richtig, aber die Combo-Buchführung von RSR läuft über die andere
Aktion. Ohne Spielbeobachtung nicht entscheidbar.

## Was ausgeschlossen wurde und warum

**Eine Rollenebene in der Vererbung.** Ausgeschlossen, und der Grund ist unabhängig von
Merge-Kosten: Die Basisklasse `: CustomRotation` steht im **generierten** Teil der
`partial class`, und C# erlaubt die Angabe nur in einem Teil — ein `: HealerRotation`
in der handgeschriebenen Hälfte ist ein Compilefehler. Sie zu ändern hieße,
`Rotation.resx` von Hand zu editieren, was der nächste Generatorlauf wortlos
überschreibt; und neu erzeugen kann dieser Branch die resx nicht, weil der Generator
die hier fehlenden Spieldateien liest. Unabhängig davon leistet eine Rollenebene
nichts, was ein rollenbenannter Helfer auf `CustomRotation` nicht auch leistet.

**A1 — ein `SustainGCD`-Slot im Dispatch.** Ausgeschlossen, weil die drei Aufrufstellen
je Heiler **keine Duplikate** sind. Die *Bedingung* liegt seit `6b40600` in genau einem
Helfer je Job; was dreifach dasteht, ist die **Position**, und die ist bewusst
verschieden:

| Methode | Position | Aussage |
|---|---|---|
| `GeneralGCD` | zuerst | Sustain schlägt Schaden |
| `HealSingleGCD` | nach reaktivem HoT, vor Cure II | Sustain schlägt Direktheilung, aber nicht den reaktiven HoT |
| `HealAreaGCD` | zuletzt | Sustain verliert gegen jede AoE-Heilung |

Drei Positionen sind drei Prioritätsaussagen. Ein zentraler Slot kann nur eine davon
ausdrücken und löscht die anderen beiden stillschweigend — Informationsverlust, nicht
Entdopplung. Konkret: Zieht man den Sustain-Aufruf bei WHM aus `HealSingleGCD` heraus,
gewinnt Cure II diese GCD, und der Sustain verhungert wieder, sobald der Tank Schaden
nimmt. Damit ist auch U2 aus `03-universal.md` anders zu bewerten: Die Wiederholung ist
der Preis dafür, dass ein Job seine Prioritäten pro Dispatch-Slot selbst setzen kann.

**A2 — `FirstUsable` statt Level-Ketten.** Ausgeschlossen an der Messung. Von **65**
echten Aufstiegsketten in 16 Dateien sind nur **25** gleichförmig konvertierbar. Die
übrigen 40 scheitern daran, dass die Gate-Aktion oft eine **andere** ist als die
gecastete (`!SummonIfritPvE… && RubyRuinPvE.CanUse`) und dass der Prädikat-Typ
innerhalb derselben Kette zwischen `EnoughLevel` und `Info.EnoughLevelAndQuest()`
wechselt. Eine Konvertierung des sauberen Viertels ersetzt *ein* Idiom durch *zwei* —
das Gegenteil des Ziels — und die Fehlerklasse bliebe an 40 Stellen bestehen. Sie ist
stattdessen durch A2′ vollständig und ohne Produktivcode geschlossen.

**B2 — `TryRangedPull(out act)` für Tanks.** Ausgeschlossen, weil es nichts zu teilen
gibt: Die vier Endzweige stehen bereits in der minimalen Form, der einzige Unterschied
ist der Aktionsname, und der ist die eigentliche Information. Ein gemeinsamer Helfer
bräuchte je Job ein `protected override IBaseAction RangedPull => TomahawkPvE;` —
gleiche Zeilenzahl, ein zusätzliches abstraktes Mitglied auf `CustomRotation`, und beim
Lesen ein Sprung in die Basisklasse für eine Information, die vorher direkt dastand.

## Nachweisbarkeit

| Ebene | Möglich | Nicht möglich |
|---|---|---|
| Statisch | Beide Wächter laufen in der CI und finden ihre Fehlerklassen vollständig; A4a-Diffs zeigen, dass nur Methodengrenzen eingefügt wurden | — |
| Kompilierung | CI | — |
| Laufzeit | Spielbeobachtung für B3, B4, B5 und C1 | Belegt ist keiner der vier Punkte |

Kein Punkt dieses Dokuments ist spielgetestet. Alle Zahlen sind aus dem Code gezählt,
alle Wirkungen statisch hergeleitet. Für A3, A2′, A4a und B1 genügt das, weil sie
verhaltensgleich sind; für B3, B4, B5 und C1 genügt es nicht, weshalb sie offen stehen.

## Was für die offenen Punkte fehlt

B3, B4, B5 und C1 sind keine Struktur-, sondern Verhaltensfragen. Ihre Diffs wären
winzig. Was fehlt, ist die Spielbeobachtung — nicht die Machbarkeit und nicht die
Erlaubnis.
