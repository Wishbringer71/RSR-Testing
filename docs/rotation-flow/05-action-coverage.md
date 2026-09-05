# 05 · Aktions-Abdeckung — was die Jobs haben, was RSR davon benutzt

## Datenquelle

Die vollständige Aktionsliste liegt maschinenlesbar im Repo:
`RotationSolver.SourceGenerators/Properties/Rotation.resx` (1,98 MB,
eingecheckt) enthält für alle 23 Jobklassen jede Aktion mit Anzeigename,
Aktions-ID, Kategorie (Spell / Ability / Weaponskill / Limit Break), Jobliste
und **der Spielbeschreibung im Klartext**. Der Source-Generator emittiert
daraus zur Compile-Zeit die `abstract partial class {Job}Rotation`. Erzeugt
wird die Datei offline von `RotationSolver.GameData` aus den Spieldateien.

Damit ist der Abgleich „was hat der Job" gegen „was benutzt RSR" nicht
geschätzt, sondern gezählt.

## Methodik — und drei Fehler darin, die vor dem Ergebnis gefunden wurden

Die Zählung ist dreimal falsch gewesen, bevor sie stimmte. Alle drei Fehler
waren Scoping-Fehler derselben Art, deshalb stehen sie hier: sie sind die
eigentliche Lehre für jede künftige Messung dieser Art.

| Fehler | Wirkung | Korrektur |
|---|---|---|
| `RotationSolver.Basic/Rotations` als „Verwendung" mitgezählt | dort steht die **Deklaration**; `IcarusPvE` galt als benutzt, obwohl keine Rotation es rief | Verwendung nur in `RebornRotations` und `ExtraRotations` zählen |
| Bewegungserkennung per `\brush\b` | „Rush**es** to a target" nicht erfasst — SAM Gyoten, PLD Intervene, DRG Elusive Jump fehlten | Wortstamm statt Wort |
| Dispatch-Overrides nur in `RebornRotations` gesucht | die **handgeschriebenen Basic-Partials** überschreiben teils selbst; SGE hatte Icarus längst verdrahtet | beide Ebenen durchsuchen |

Der dritte Fehler hätte beinahe eine überflüssige Änderung produziert. Er ist
zugleich ein inhaltlicher Befund, siehe „Zwei Ebenen" unten.

## Zahlen

| | |
|---|---|
| Jobklassen mit generierten Aktionen | 22 (BST hat keine) |
| PvE-Aktionen gesamt | **1076** |
| davon in der Hauptrotation des Jobs verwendet | **879** |
| nur in einer ExtraRotation verwendet | 20 |
| nirgends verwendet | **177** |

## Warum „ungenutzt" meist kein Fehler ist

Die 177 zerfallen fast vollständig in Klassen, in denen Nichtverwendung
richtig ist:

| Klasse | Anzahl | Warum korrekt |
|---|---|---|
| Limit Breaks | 63 | RSR fährt keine PvE-Limit-Breaks |
| Pet-/Automaton-Aktionen | 49 | Beschreibung sagt selbst „cannot be assigned to a hotbar" — SMN-Demi-Angriffe, MCH-Queen, SCH-Faerie |
| Stance-Abbruch (`Release*`) | 4 | RSR bricht Tank-Stances nicht ab |
| Morph-Platzhalter | 3 | VPR `SerpentsTail`/`Twinfang`/`Twinblood` — „Changes to … when requirements are met"; RSR ruft die Zielaktion |
| Rest | 74 | s. u. |

### Der Befund, der den größten Teil des Rests erklärt: `AdjustedID`

`BaseAction.Use()` castet nicht die deklarierte ID, sondern
`adjustId = AdjustedID` (`BaseAction.cs:278/319`), und das ist
`ActionManager->GetAdjustedActionId(ID)` — die vom **Spiel** aufgelöste
Ersetzung.

Daraus folgt: **die Basisaktion ist der Griff, das Upgrade löst das Spiel
auf.** `ArtOfWarPvE.CanUse()` feuert ab Stufe 82 Art of War II. Deshalb ist es
richtig und nicht lückenhaft, dass `ArtOfWarIiPvE`, `HighFireIiPvE`,
`HighBlizzardIiPvE`, `DosisIiPvE`/`DosisIiiPvE`, `PhlegmaIiPvE`/`PhlegmaIiiPvE`,
`DyskrasiaIiPvE`, `JoltIiPvE`/`JoltIiiPvE`, `GyofuPvE` und
`ShadowOfTheDestroyerPvE` nirgends vorkommen.

**Die echte Fehlerklasse liegt woanders**, und der Fork hat sie schon zweimal
getroffen: nicht die *Aktion*, sondern die **Annahme über sie** veraltet mit
dem Upgrade. Der Status heißt nach dem Upgrade anders (Thunder → High Thunder,
AUDIT_LOG/TODO #44), die Potenz ändert sich, die Combo-Bedingung kann sich
ändern. Wer nach `X` sucht, findet solche Fehler nicht; wer nach „Statusliste
neben `X.CanUse` vollständig?" sucht, schon.

Dieselbe Erkenntnis trifft die level-gestaffelten Ketten: wo SCH sechs Zweige
zwischen `RuinPvE`/`BroilPvE`/`BroilIiPvE`/`BroilIiiPvE`/`BroilIvPvE`
unterscheidet, tut das Spiel dasselbe von allein. Ob die Ketten deshalb
entbehrlich sind, ist **nicht** aus dem Code entscheidbar — siehe C1 im
Zielkonzept, wo derselbe Punkt für DRG als Spielfrage geführt wird.

## Zwei Ebenen — der strukturelle Befund

Dispatch-Overrides liegen in **zwei** Schichten, ohne erkennbare Regel:

| Ebene | Beispiele |
|---|---|
| `RotationSolver.Basic/Rotations/Basic/{Job}Rotation.cs` (handgeschriebene Hälfte) | NIN `MoveForwardAbility`, SGE `MoveForwardAbility`, WAR `MoveForwardAbility`/`MoveForwardGCD`/`EmergencyAbility`, DRK/GNB `EmergencyAbility` (Invuln) |
| `RotationSolver/RebornRotations/…/{Job}_Reborn.cs` | alle übrigen — SAM, RPR, MNK, VPR, DRG, PLD, DRK, DNC, RDM, PCT, BLM |

Konkrete Folge, nicht nur Kosmetik:

- **Invulnerability** ist bei DRK, GNB und WAR in der Basisschicht verdrahtet
  (`EmergencyAbility`, gegated auf `Service.Config.HealthForDyingTanks`), bei
  **PLD** dagegen in `PLD_Reborn.cs:92/97` mit eigener Logik
  (`HallowedWithCover`). Vier Tanks, dieselbe Fähigkeitsklasse, zwei Orte und
  zwei Gates.
- Beim Lesen einer `{Job}_Reborn.cs` ist nicht erkennbar, ob ein Slot leer ist
  oder eine Ebene tiefer belegt. Genau daran ist die Messung oben gescheitert.

## Bewegungsfähigkeiten — geschlossene Lücke

Vollständige Erhebung aller Aktionen, deren Spielbeschreibung eine Ortsänderung
nennt, gegen die Belegung von `MoveForwardAbility` / `MoveBackAbility` (beide
laufen laut `CustomRotation_Ability.cs:325/342` **nur** bei gesetztem
`AutoStatus.MoveForward`/`MoveBack`, können die Schadensrotation also nicht
stören):

| Job | Aktion | Richtung | Vorher | Jetzt |
|---|---|---|---|---|
| GNB | Trajectory (2 Ladungen) | vor | nicht verdrahtet | `GNB_Reborn.MoveForwardAbility` |
| WHM | Aetherial Shift | vor | nicht verdrahtet | `WHM_Reborn.MoveForwardAbility` |
| BRD | Repelling Shot | zurück | nicht verdrahtet | `BRD_Reborn.MoveBackAbility` |
| SGE | Icarus | vor | **war** verdrahtet, eine Ebene höher | unverändert |
| BLM · DNC · DRK · DRG · MNK · NIN · PCT · RPR · RDM · SAM · SMN · VPR · WAR · PLD | — | — | verdrahtet | unverändert |

Bewusst **nicht** ergänzt: SAM `HissatsuYatenPvE`. Es ist keine reine
Bewegungsfähigkeit, sondern eine Schadensaktion mit Rückstoß, die zusätzlich
Enhanced Enpi gewährt — sie in den Bewegungsslot zu hängen ist eine
Rotationsentscheidung, keine Lückenschließung, und gehört ins Spiel geprüft.

## Verbleibender Rest — offene Kandidaten

Nach Abzug von Limit Breaks, Pet-Aktionen, Stance-Abbrüchen,
Morph-Platzhaltern und Upgrade-Griffen bleiben Aktionen, für die keiner dieser
Gründe greift. Sie sind in `TODO.md` #69 einzeln geführt und **nicht**
ungeprüft nachgerüstet — jede braucht eine Rotationsentscheidung, keine
Codeentscheidung.
