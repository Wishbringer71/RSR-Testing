# 04 · Zielkonzept

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Stand dar; die
Prüfhistorie steht in `AUDIT_LOG.md` (A7, Register #66).

## Stand

Der strukturelle Umbau ist abgeschlossen. Von den vier Verhaltensfragen, die danach offen standen, sind drei
im Code gelöst (A7, 05.09.2026). Die vierte (C1) ist keine offene Frage mehr: Die Vereinfachung ist verworfen,
und die geltende Form bleibt.

| Punkt | Inhalt | Stand |
|---|---|---|
| A3 | Base-Call-Wächter in der CI | umgesetzt, `check_base_calls.py` in `build.yaml` |
| A2′ | Wächter gegen widersprüchliche Level-Prädikate | umgesetzt, dieselbe Prüfung |
| A4a | Zweige oberster Ebene zu benannten Stufen | umgesetzt für BLU, PhantomDefault, PCT, SAM, SMN; MCH bewusst ausgelassen |
| A4b | Die Stufenliste gilt als Reihenfolge, nicht als Namensvorschrift | Konvention, angewandt |
| B1 | `SwiftRaisePending` | umgesetzt |
| B3 · B4 · B5 | Reprisal-Platzierung, Slot-Lücken der Schadensausteiler, MNK-Heilslot | umgesetzt (A7: 00bc9c6f, 4b3c9412) |
| C1 | DRG-Trait-Gates | Vereinfachung verworfen, Gates bleiben |
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
| `public abstract partial class WhiteMageRotation : CustomRotation` (alle Aktionen, `AllBaseActions`, `AllTraits`) | `RotationSolver.SourceGenerators/Properties/Rotation.resx`, 23× (2,05 MB am 02.10.2026) | **generierter Text**, eingecheckt |
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
exakt die Form des RDM-Impact-Bugs und ist ein Build-Fehler.

Die Prüfung überspringt Bedingungen mit `||`. Das im Repo verbreitete und **korrekte**
Idiom ist die explizite Level-Klammer `(X.EnoughLevel && …) || !X.EnoughLevel`, in der
beide Terme vorkommen, aber in verschiedenen Ästen; ohne echten Ausdrucksparser wären
das sämtlich Fehlalarme.

### A4a · Zweige oberster Ebene zu benannten Stufen

Gemessen am 05.09.2026 (A7) lagen über alle 31 PvE-Rotationsdateien **1239** Zweige auf oberster Ebene,
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
in vier Dateien → eine private Eigenschaft `SwiftRaisePending` je Heilerdatei (ae51e6c3), 13 Verwendungen. Sie
liegt nicht auf `CustomRotation`, anders als der Abschnittskopf sagt: Sie liest `SwiftLogic`, eine Option jeder
Heilerrotation, und eine zentrale Fassung müsste diese Option mitziehen. Die Option ist als Name in der gespeicherten
Konfiguration ein Vertrag.

### B3 · Tanks — Reprisal-Platzierung · umgesetzt

Alle vier Tanks führen Reflexion in `DefenseAreaAbility` und in `DefenseSingleAbility` (00bc9c6f). Vorher fehlte
sie bei Paladin und Krieger in der Flächenabwehr, obwohl die `RotationDesc` des Kriegers sie dort schon nannte.
Am Tankbuster wartet sie auf einen angesagten Raidwide, wenn der Tank gedeckt ist (Konzept 08, A244).

### B4 · Physische Fernkämpfer — Slot-Mengen · umgesetzt

Die Lücken sind geschlossen (4b3c9412): Maschinist und Barde haben Second Wind in `HealSingleAbility`, der Tänzer
`DefenseSingleAbility`. Die übrigen Unterschiede folgen den Aktionen, die nur ein Job hat: `DispelAbility` beim
Barden (Warden's Paean), `HealAreaAbility` beim Tänzer (Curing Waltz, Improvisation), `MoveForwardAbility` beim Tänzer (En Avant),
`MoveBackAbility` beim Barden (Repelling Shot).

### B5 · Melee — MNK-Heilslot · umgesetzt

Second Wind und Bloodbath stehen beim Mönch in `HealSingleAbility` (4b3c9412). `HealAreaAbility` trägt nur noch die
Flächenheilungen Earth's Reply und Mantra.

## C · Jobebene

Nach der Zuordnung zu A4a bleibt von C ein einziger eigenständiger Punkt.

| Job | Punkt | Zuordnung |
|---|---|---|
| BLU · PhantomDefault · PCT · SAM · SMN | 80 / 33 / 32 / 27 / 23 Zweige | A4a, umgesetzt |
| MCH | 23 Zweige | A4a, bewusst ausgelassen |
| DRG | 6 `Trait.EnoughLevel`/`!Trait…`-Paare (gezählt am 02.10.2026) | Vereinfachung verworfen |
| DRG · VPR | `CountDownAction` nur mit dem Sprung zum Ziel | geschlossen: Sprung außerhalb der Nahkampfreichweite wie beim Monk (unten, „Der Sprung zum Ziel am Ende des Countdowns", A347) |
| RPR · SAM · WAR | kein `EmergencyAbility` in der Reborn-Datei | keine Lücke: WAR in der Basisschicht, RPR/SAM ohne eigene Notfallaktion |

### C1 · DRG-Trait-Paare — der Fund, der die Vereinfachung blockiert

Sechsmal steht in `DRG_Reborn.cs` dasselbe Muster (gezählt am 02.10.2026):

```csharp
if (LanceMasteryIiTrait.EnoughLevel)  { if (HeavensThrustPvE.CanUse(out act)) return true; }
if (!LanceMasteryIiTrait.EnoughLevel) { if (FullThrustPvE.CanUse(out act))    return true; }
```

Die naheliegende Vereinfachung ist, die Gates zu streichen — `CanUse` prüft
`EnoughLevel` bereits selbst (`ActionBasicInfo.BasicCheck`). **Das ist nicht
verhaltensgleich, und der Unterschied ist kein Randfall.** Die gegateten Fassungen
schließen einander aus; die ordnende Fassung ist ein Fallback. Sie unterscheiden sich
genau dann, wenn `HeavensThrustPvE.CanUse` aus einem **anderen Grund als dem Level**
fehlschlägt — Combo nicht offen, Reichweite, Status. Dann versucht die ordnende
Schreibweise zusätzlich die Vorgängeraktion, die gegatete nicht.

**Entscheidung: Die Gates bleiben.** Die gegatete Form ist das geltende Verhalten. Es gibt keinen Defekt, den eine
Vereinfachung beheben würde. Sie wäre nur eine Umformung mit möglicher Verhaltensänderung, also bleibt die Form.
`BaseAction.Use()` wirkt bei Aktionen auf ein Ziel `AdjustedID`, bei Bodenzielen (Zweig `TargetInfo.IsTargetArea`)
`ID`. Die frühere Aussage, `Use()` wirke immer `ID`, war falsch (C106).

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

## Prüfmittel und Prüfgrad

| Ebene | Möglich | Nicht möglich |
|---|---|---|
| Statisch | Beide Wächter laufen in der CI und finden ihre Fehlerklassen vollständig; A4a-Diffs zeigen, dass nur Methodengrenzen eingefügt wurden | — |
| Kompilierung | CI | — |
| Laufzeit | — | — |

Alle Zahlen sind aus dem Code gezählt, alle Wirkungen statisch hergeleitet. Für A3, A2′, A4a und B1 genügt das,
weil sie verhaltensgleich sind. B3, B4 und B5 schließen je eine Lücke, in der vorher keine Aktion fiel.

## Countdown und Notfallslot

Die Countdown-Aktionen aller Standardrotationen decken sich mit den Openern von The Balance (gelesen 25.10.2026, A346):
Vorwirken des ersten Zaubers bei Paladin, Heilern, Black Mage, Summoner, Red Mage, Pictomancer und Reaper; Fernangriff
kurz vor dem Pull bei Warrior und Gunbreaker; The Blackest Night und Unmend beim Dark Knight; Mudras und Suiton beim
Ninja; Meikyo Shisui und True North beim Samurai; Standard Step beim Dancer; Reassemble und Air Anchor beim
Maschinisten. Beim Trank liegen einige um Sekunden anders (Gunbreaker vor dem Pull statt nach dem zweiten GCD, Dancer
im Kampf statt bei −1 s); seine 30 s decken die Fenster in beiden Fällen. Dragoon: Standard-Opener ohne Vorlauf, True
Thrust zum Pull, Trank nach dem zweiten GCD; die Variante mit Piercing Talon gilt bei einem Ziel, das vorher nicht
erreichbar ist, mit laut The Balance vergleichbarem Schaden, nicht gebaut.

### Der Sprung zum Ziel am Ende des Countdowns

**Monk, Viper und Dragoon springen in der letzten halben Sekunde des Countdowns ans Ziel, wenn es außerhalb der
Nahkampfreichweite steht; in Reichweite nicht** (`CustomRotation.PullGapCloser`, A347). Monk mit Thunderclap, Viper mit
Slither, Dragoon mit Winged Glide; alle drei treffen nicht und bewegen nur. Einstellung „Dash to the target at the end of
the countdown when out of melee range", ab Werk an. Nahkampfreichweite ist die Reichweite der ersten Waffenfertigkeit
des Jobs aus dem Spiel (3 y), gemessen von Trefferkreis zu Trefferkreis wie die Anzeige des Spiels.

- *Seine Angaben (25.10.2026):* „Ich spiele bislang keinen monk, also weiß ich es nicht. Monk ist nahkämpfer, daher
  sollte er im angriffsrange stehen." – Hinweis. „Gerade wenn man Gegner explizit bei Wall-to-wall so anspringt, dass
  man mittig für aoe Angriffe rauskommt, kann es Sinn machen." – Vorschlag, unten geprüft.
- *Quellen:* The Balance, Viper-Opener (Patch 7.55): Slither bei −0,5 s. Gunbreaker: „Trajectory can be used at
  -0.66s to replace Lightning Shot". Monk Basic Guide: „uptime should be your top priority … how physically close you
  are to an enemy". Dragoon Basic Guide: Winged Glide „does not deal damage so you can use it freely whenever you need
  to gapclose longer distances". Job-Guide 7.5: Thunderclap, Slither, Winged Glide „Rush to a targeted enemy's
  location", 20 y.
- *Im Kampf* (`melee_gapcloser_model.py`; Laufgeschwindigkeit und Animationssperre nennt keine Quelle, gerechnet mit
  5–7 y/s und 0,5–0,9 s): Steht er zum Pull 8 y entfernt, fällt die erste Waffenfertigkeit zu Fuß 0,8 s später, bei
  12 y 1,5 s, bei 20 y 2,8 s; mit dem Sprung 0–0,4 s später. Knapp außerhalb der Reichweite, bis 4–6 y je nach
  Annahme, kostet der Sprung bis 0,4 s mehr als die Schritte (Mitte der Annahmen 0,2 s); das Modell rechnet den Weg ab
  dem Pull, ohne die Zeit, bis er losläuft. Upstreams Monk sprang auch in Reichweite – eine Ladung und bis 0,4 s
  Animationssperre für nichts – und mit gesetztem Fokus auf einem Gruppenmitglied zu diesem; beides entfällt.
- *Warum seine Regel zur Bewegung nicht dagegen steht:* Sie schließt aus, ihn für Schaden aus einer sicheren Position
  zu holen. Zum Pull hat der Gegner noch nichts gewirkt, und die Position eines Nahkämpfers ist dann am Ziel (sein
  Hinweis). Wer zum Pull bewusst Abstand hält – ein Gegner, der zum Kampfbeginn springt oder zurückstößt –, schaltet die
  Einstellung ab. Der Sicherheitscheck über BossModReborn gilt, wenn er ihn eingeschaltet hat (ab Werk aus).
- *Nicht gebaut:* eine Schwelle an der Stelle, ab der der Sprung schneller ist als die Schritte. Sie bräuchte die
  Laufgeschwindigkeit und die Animationssperre des Sprungs, die keine Quelle nennt; messen ließen sich beide erst aus
  früheren Sprüngen und Wegen. Der Unterschied ist höchstens 0,4 s einmal je Pull, der Gewinn darüber Sekunden.
- *Nicht einbezogen:* Samurai (Gyoten kostet 10 Kenki, zum Pull keine), Reaper (Hell's Ingress springt 15 y in
  Blickrichtung, nicht zum Ziel), Ninja (Shukuchi zielt auf einen Ort), Tanks (ihr Pull ist eine eigene Regel:
  Fernangriff, beim Dark Knight Shadowstride).

**Sein Vorschlag Wall-to-wall: geprüft, nicht automatisiert.** Ein Sprung auf einen Gegner eines gesammelten Rudels
bringt die Flächenangriffe (5 y um den Spieler) an fast alle: Modell, 5 Gegner im Umkreis 4 y, im Mittel 4,6 bis 4,8
erreicht, beim Halt am ersten erreichbaren Gegner 2,5; der Sprung zum mittigsten Gegner bringt gegenüber dem eigenen Ziel
nur bei lockeren Rudeln mehr (6 y: 4,1 gegen 3,4). Auf seinen Befehl (Vorwärtsbewegung) springt RSR schon heute zum
Ziel und holt damit den Großteil. Automatisch im Kampf nicht: Der Sprung endet am Trefferkreis des Ziels auf der Seite,
von der er kommt – meist vor den Gegnern, die zum Tank schauen –, und deren Flächen sieht RSR ohne BossModReborn-Modul
nicht; der Sicherheitscheck ist ab Werk aus. Nach seiner Regel geht die Sicherheit der Gruppe, gewichtet mit der
Wahrscheinlichkeit, vor; Frontalangriffe von Trash-Gruppen sind im Dungeon häufig.

Der Notfallslot ist keine Lücke. Der Krieger hat ihn in der Basisschicht (`WarriorRotation.EmergencyAbility`,
Holmgang). Schnitter und Samurai haben keine eigene Notfallaktion; ihre Abwehr läuft über die Abwehrslots, und den
Rollen-Notfall stellt `CustomRotation.EmergencyAbility`.

## Schadensflächen erst, wo die Gegner bleiben

**Eine Schadensfläche fällt erst, wenn mindestens ein Gegner seit einem GCD in ihrem Radius ist und keiner, der in
diesem GCD darin war, ihn verlassen hat** (`CustomRotation.DamageAreaSettled`, `HostileMovement.AreaHolds`, A350). Wer
sich innerhalb der Fläche bewegt, zählt als geblieben; wer dazukommt, hält sie nicht auf. Betroffen sind die Aktionen,
deren Wirktext eine Fläche legt, die „any enemies who enter" trifft: Salted Earth (Dunkelritter, um die eigenen Füße),
Doton (Ninja, um die eigenen Füße), Slipstream (Beschwörer, Sturm um das Ziel). Radius ist der Wirkbereich aus dem
Aktionsblatt (je 5 y), gemessen wie die Flächenprüfung des Spiels: Trefferkreis im Radius. Einstellung „Lay damage areas
only where the enemies stay", ab Werk an: Die Bedingung ist seine, das Maß „ein GCD in der Fläche" meine Ableitung und im
Kampf noch nicht gesehen; ausgeschaltet fällt die Fläche, sobald er selbst steht.

- *Seine Angaben (25.10.2026):* „Wichtig bei allen Klassen mit Schadensflächen: erst dann, wenn Bewegung nach
  walltowall stoppt. Prüfen im Loop" – Hinweis (eine Fläche bleibt liegen, ein Rudel, das seinem Tank folgt, läuft
  hinaus) und Vorschlag für die Bedingung. Dazu sein Hinweis: „gegner [können sich] innerhalb der schadensfläche
  bewegen …, ohne diese zu verlassen" – was eine Fläche verschwendet, ist ein Gegner, der sie verlässt, nicht einer, der
  in ihr den Platz wechselt.
- *Im Kampf:* Beim Wall-to-wall zieht der Tank Rudel hinter sich her; hält er kurz, laufen sie noch auf ihn zu, und zieht
  er weiter, folgen sie. Früher fiel die Fläche, sobald der Spieler selbst stand – eine Pause des Tanks genügte, und
  Salted Earth (90 s Abklingzeit) oder Doton lagen dort, wo das Rudel nicht blieb. Jetzt fällt sie, wenn das Rudel
  angekommen ist und dort bleibt, auch wenn es sich am Tank noch zurechtschiebt. Ein Boss, der nicht wandert, ist ohnehin
  in der Fläche; wandert er aus ihr hinaus, wartet sie, bis er wieder einen GCD lang in einer bleibt.
- *Modell* (`damage_area_model.py`, Annahmen: 6 y/s, Gegner verschieben sich alle 1–2 s um 1–3 y am Tank, Fläche 5 y für
  15 s): Tank hält 1,5 s und zieht weiter – früher lag die Fläche an der Pause und traf nichts, jetzt nach der Ankunft am
  Ziel mit allen 25 möglichen Treffern. Rudel schiebt sich am Ziel – die erste Fassung (keiner bewegt sich einen GCD lang)
  legte die Fläche nie, die jetzige nach 1,5–2 s. Zwei Nachzügler – die strengere Variante, die jeden Gegner in der
  Fläche einen GCD lang darin verlangt, wartet bis 9,6 s, die jetzige legt sie bei 4,7 s mit denselben Treffern.
- *Gemessen:* Das Spiel gibt nur für den Spieler an, ob er sich bewegt. Für Gegner führt RSR eine Spur der Positionen
  über den letzten GCD, einen Punkt bei jeder Änderung. Ein GCD in der Fläche unterscheidet den Halt von der Pause: Ein
  Rudel, das seinem Tank mit Laufgeschwindigkeit folgt, durchquert 5 y in unter einer Sekunde. Ein Gegner, der eben erst
  erscheint, ist noch nirgends geblieben.
- *Wo entschieden wird:* Salted Earth in `BaseAction.CanUse` für jede Rotation, die es drückt. Doton beim Beginn der
  Mudras (`NIN_Reborn`): Bleibt das Rudel noch nicht, kommt Katon, Doton mit der nächsten Ladung. Slipstream
  (`SMN_Reborn`): Der Sturm ist nur Zusatzwirkung eines Treffers (520 Potenz auf das Ziel); es wartet, bis die Gegner um
  sein Ziel bleiben, solange Emerald Rites übrig sind, und fällt spätestens, wenn sie verbraucht sind. Eine Aktion, die
  nicht auf einer Leiste liegen kann (Aktionsblatt `IsPlayerAction` – ein Ninjutsu, ein Schritt von Ten Chi Jin), prüft
  `CanUse` nicht: Sie ist dort entschieden, wo ihre Folge beginnt.
- *Nicht einbezogen:* Earthly Star (Astrologe) – vor allem eine Heilung; sie wartet nicht auf die Gegner, das hielte eine
  Heilung für Schaden zurück. Ley Lines (Schwarzmagier) ist keine Schadensfläche; ob sie fällt, hängt an der Bewegung des
  Spielers selbst. Ten Chi Jin: Die Folge steht mit ihrem ersten Schritt fest und dauert Sekunden; Doton darin bleibt,
  wie es ist.
- *Grenzen:* Eine Pause des Tanks, die länger als ein GCD dauert, sieht aus wie ein Halt; zieht er danach weiter, liegt
  die Fläche, wo er war (Modell: 4 bis 5 von 25 Treffern, wie früher). Ein Gegner, der am Rand ein- und austritt, hält
  sie je einen GCD auf. Fremde Rotationen, die Salted Earth drücken, folgen der Prüfung mit; Doton und Slipstream nur in
  den Standardrotationen.
