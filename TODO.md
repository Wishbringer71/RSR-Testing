# TODO — nur offene Arbeit

## Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Kein Diff-Review, sondern der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 (mechanische Scans, Rotationsmuster, Konfiguration/Oberfläche/Kommandos/IPC) sind abgeschlossen, s. AUDIT_LOG A8 und A10.

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei gelesen.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Zweiter Durchgang** mit denselben Scans über den bereinigten Baum.
- **Dokumentation** in `docs/rotation-flow/07-codebase-audit.md`.

## ChurinDNC wertet die BMR-Downtime ohne Vorzeichenprüfung aus

Die Normalisierung der Schadensvorhersagen ist erledigt (AUDIT_LOG A11). Offen bleibt der Zustandsfenster-Teil derselben Defektklasse: `ChurinDNC.cs:777-843` (Upstream) liest `BMRNextDowntimeIn`/`-EndIn` ohne Vorzeichenprüfung. Da BossModReborn diese Werte als `(Aktivierung − jetzt)` liefert, sind sie während einer laufenden Downtime negativ, und die Rotation kann „Downtime läuft" nicht von „Downtime kommt gleich" unterscheiden — `if (BMRNextDowntimeIn >= 15f) return;` kehrt dann nicht zurück, und die folgende `<`-Bedingung ist immer erfüllt. Ob das der Absicht dieser Rotation widerspricht, ist ohne deren Autor nicht belegbar; ein Filter wäre hier falsch, weil das Vorzeichen die Information trägt.

## Zyklus-Kommandos unterliegen der Toggle-Semantik der Einzelkommandos

Die Zustandswahl liegt an zwei Orten. Implizit in `AdjustStateType`: `/rotation Auto` schaltet dort über `UpdateTargetingIndex` selbst durch die Zielarten weiter, es sei denn `ToggleAuto` steht an, dann schaltet es ab — der Optionstext sagt das ausdrücklich („Normal behavior cycles between targeting settings"). Explizit in den fünf `Cycle*`-Methoden, die dieselbe Aufgabe noch einmal lösen, mit eigenem Ausschaltzweig, und über `CycleType` bzw. `DTRType` an das Zyklus-Chatkommando und den Klick in der Server-Info-Leiste gebunden sind.

Da die `Cycle*` am Ende ebenfalls `DoStateCommandType` rufen, greift `AdjustStateType` bei bereits aktivem Zustand auch dort: aus „Manual → Auto" wird „Off", und der Zyklus über die Zielarten bricht ab. Betroffen sind die fünf `Cycle*` plus das aufruferlose `IncrementState`; die automatischen Einschaltpfade in `RSCommands_Actions.cs` sind es nicht, weil sie sämtlich unter `!DataCenter.State` stehen und `AdjustStateType` dort nicht erreicht wird. Beide Toggle-Defaults sind `false`. Zu klären ist nicht nur, ob die `Cycle*` die Toggle-Semantik umgehen sollen, sondern ob die doppelte Zustandswahl bestehen bleibt.

### Bedienpfad: Ausschalten kostet je nach Variante 1 bis unendlich viele Klicks

Die fünf Varianten als Zustandsfolge ausgewertet, bei vier konfigurierten Zielarten und ausgeschalteten Toggle-Optionen. Gezählt sind Klicks auf den Server-Leisten-Eintrag, dem Hauptbedienweg:

| Variante | Folge | Umlauf | „Aus" aus dem Auto-Zustand |
|---|---|---|---|
| `DTRAuto` | Off → Auto → Off | 2 | 1 Klick |
| `DTRManual` | Off → Manual → Off | 2 | 2 Klicks (über Manual) |
| `DTRNormal` | Off → Auto(letzte) → Manual → Off | 3 | 2 Klicks |
| `DTRAllAuto` | Off → Auto(0) → … → Auto(3) → Manual → Off | 6 | bis zu 5 Klicks |
| `DTRManualAuto` | Off → Manual → Auto → Manual → Auto → … | — | **gar nicht** |

`DTRManualAuto` kennt keinen Rückweg nach Off: der letzte Zweig führt aus jedem aktiven Zustand nach Manual, der davor von Manual nach Auto. Der Enum-Text („Cycle between Manual and Auto") beschreibt das, aber über die Leiste ist RSR damit nicht mehr abschaltbar.

**Kausale Folge für die Toggle-Optionen:** `ToggleAuto` ist in dieser Variante der einzige Ausschaltweg — es verwandelt den Übergang Manual → Auto in Manual → Off. Ein pauschales Abschalten der Toggle-Auswertung in den Zyklus-Kommandos, wie zunächst vorgeschlagen, würde diesen Nutzern also den einzigen Ausschaltweg über die Leiste nehmen. Umgekehrt kollabiert `DTRAllAuto` mit aktivem `ToggleAuto` auf Off ↔ Auto(0), die Zielarten-Rotation ist dann vollständig tot. Die beiden Optionen sind damit keine unabhängige Achse, sondern Krücken für fehlende Übergänge.

### Der Leisteneintrag verwirft alle Eingabeinformation — Erweiterung abgelehnt

`IDtrBarEntry.OnClick` ist `Action<DtrInteractionEvent>`; das Ereignis trägt `ClickType` (links/rechts), `ModifierKeys` (Ctrl/Alt/Shift) und `Position`. `MiscUpdater.cs:55-71` verwirft es fünfmal mit `_ =>`. Die gesamte Bedienlast liegt damit auf einer einzigen Geste, weshalb überhaupt fünf Zyklusvarianten nötig sind — jede ist ein anderer Kompromiss aus derselben Geste. Ein zweiter Kanal (Rechtsklick oder Ctrl+Klick als „Aus") wurde vorgeschlagen und vom Auftraggeber abgelehnt; er wird nicht weiterverfolgt. Die obigen Bedienpfade bleiben damit wie beschrieben, einschließlich des fehlenden Ausschaltwegs bei `DTRManualAuto`. Randnotiz für künftige Überlegungen: Dalamud registriert nur `MouseOver`, `MouseOut` und `MouseClick`, das Scrollrad steht am Leisteneintrag also ohnehin nicht zur Verfügung.

Nicht mehr offen: der Frame-Takt des Leisteneintrags, die Wirkungslosigkeit von `/rotation Auto <Zahl>` im Aus-Zustand und der tote Parameter in `DoOneCommandType` sind behoben (AUDIT_LOG A14). `DTRManualAuto` ist als kein Fehler geschlossen — der Enum-Text beschreibt genau den Zwei-Zustands-Zyklus, den der Code abbildet.

Offen bleibt allein die strukturelle Frage: die Zustandswahl liegt weiterhin an zwei Orten, und die Toggle-Optionen wirken als Krücken für fehlende Übergänge. Eine Änderung daran wurde nach der Bedienpfad-Analyse zurückgestellt, weil sie ohne Spieltest nicht abzusichern ist.

Geprüfte Nicht-Fehlstelle: ein zu großer `TargetingIndex` kann nicht zum Indexfehler führen, `DataCenter.TargetingType` rechnet `% Count` und füllt eine leere Liste selbst auf (`DataCenter.cs:284-302`).

## Selbstlernende AoE-Liste verunreinigt die Mitigations-Trigger

`Watcher.ActionFromEnemy:111-148` speichert eine Gegner-Aktion dauerhaft in `HostileCastingArea`, wenn die Party mindestens vier Mitglieder hat, die Aktion eine Wirkzeit besitzt (`Cast100ms > 0`), zur Kategorie Spell/Weaponskill/Ability gehört und **jedes** Party-Mitglied im selben Effektsatz Schaden genommen hat. Die Option „Record AOE actions" ist standardmäßig an, ausgeliefert werden bereits 850 IDs.

Die Lernbedingung ist damit strenger als zunächst notiert — eine ausweichbare Boden-AoE wird nur gelernt, wenn wirklich alle hineingelaufen sind —, aber sie ist unumkehrbar: Zurücknehmen lässt sich ein Eintrag nur durch Editieren der Datei. Offen bleibt, ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen (Kandidat: `CastType`/`EffectRange` der Aktion); ohne Spieldaten nicht entscheidbar, blind verschärfen wäre Raten. Durch die Reichweitenprüfung in `IsHostileCastingArea` ist die Fehlwirkung entschärft, nicht behoben. Geprüfte Nicht-Fehlstelle: das Speichern läuft asynchron (`_ = SaveHostileCastingArea()`), also kein blockierendes Schreiben im Kampfpfad.

## `SpreadDamagePaths` enthält keinen einzigen Spread-Marker

`DataCenter.cs:2036-2043`. Zwei der vier Pfade (`x6r9_loc01_t0a1`, `x6r9_loc02_t0a1`) stehen wortgleich auch in `SharedDamagePaths` (2025-2026), die anderen beiden sind laut eigenem Kommentar „Duty-specific AOE share markers", also ebenfalls Stack-Marker. Die Liste trägt damit nichts, weil `IsCastingAreaVfx` ohnehin alle drei Listen prüft. Prüfen, ob echte Spread-Marker fehlen (dann Liste füllen) oder ob sie ganz entfallen kann. Nebenbefund: `SharedDamagePaths` führt `vfx/lockon/eff/com_trg01_0c` zweimal (2022 und 2024) — im `FrozenSet` folgenlos, aber ein Pflegehinweis.

## Release-Paket enthält 14 MB Ballast, die Prune-Regel greift dafür nicht

**Am Artefakt belegt** (`latest.zip` des Releases 7.5.5.41+wsh1, 5,35 MB komprimiert). Nutzlast sind `RotationSolver.dll` (1,09 MB), `RotationSolver.Basic.dll` (2,45 MB), `ECommons.dll` (0,78 MB) und `RotationSolver.json`. Dazu kommen vier Kategorien überflüssiger Artefakte:

| Datei | Größe | Ursache |
|---|---|---|
| `RotationSolver.Basic.xml` | 7,52 MB | `<GenerateDocumentationFile>True` in beiden Projekten |
| `RotationSolver.SourceGenerators.dll` + `.pdb` + `.deps.json` | 5,59 MB | Analyzer, zur Laufzeit nutzlos |
| `RotationSolverReborn.Basic.7.5.5.41.nupkg` | 1,54 MB | `<GeneratePackageOnBuild>True` in `RotationSolver.Basic.csproj:12` |
| `RotationSolver.Basic.pdb` | 1,27 MB | Debug-Symbole des Release-Builds |

Ursache ist nicht die Prune-Regel: `PruneOutputDlls` arbeitet auf `ReferenceCopyLocalPaths` und erfasst damit weder `.pdb` noch `.xml` noch `.nupkg` noch den Analyzer — die Analyzer-Referenz selbst ist mit `OutputItemType="Analyzer" ExcludeAssets="All"` korrekt eingebunden.

**Korrektur einer früheren Aussage:** Ursache ist auch nicht das `OutputPath` in `RotationSolver.csproj`. Der Veröffentlichungs-Workflow überschreibt es: `publish.yaml:42` baut mit `--output .\build`, und `:47` lädt `./build/RotationSolver/latest.zip` hoch. Ein `dotnet build --output` auf ein Projekt mit Projektverweisen legt die Ausgaben aller beteiligten Projekte in dasselbe Verzeichnis, aus dem DalamudPackager dann packt. Eine Trennung der Ausgabeverzeichnisse in den Projektdateien bliebe daher wirkungslos. **Nach Abwägung zurückgestellt (AUDIT_LOG A14):** Der naheliegende Weg trägt nicht. `DalamudPackager.targets` reicht `Exclude` im Standard-Target nicht durch — dieser läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt; der vorgesehene Weg verlangt, diese Datei anzulegen und den vollständigen Task-Aufruf samt aller Manifest-Felder nachzubauen. `Exclude` vergleicht laut Task-Quelle (`DalamudPackager.cs:187`) exakt über `List.Contains`, kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Der Build-Workflow kompiliert nur, er packt nicht, prüft das Ergebnis also nicht. Verbleibende Ansatzpunkte, falls der Punkt aufgegriffen wird: `--output` aus dem Veröffentlichungs-Workflow entfernen, oder Paket- und Dokumentationserzeugung an eine eigene Bedingung binden. Geprüft: die XML-Dokumentation wird zur Laufzeit nicht gelesen.

## VPR: leerer Zweig einer Struktur, die anderswo eine echte Entscheidung trägt

`VPR_Reborn.cs:591-597` und `975-981`. Nicht zu löschen — die Begründung steht in AUDIT_LOG A11. Das Muster `!HasHunterAndSwift` kommt viermal vor, und der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (751-807) Inhalt, nämlich eine positionsbewusste Wahl samt Wechselsperre. An der Bite- und der Sting-Stelle ist er eine Kopie des Folgezweigs, an der Den-Stelle (424-493) fehlt er ganz. Ein fehlender Inhalt lässt sich nicht belegen (die AoE-Kette hat keine Positionals, bei Sting wird der Positionsfall oberhalb über `HasHind`/`HasFlank` entschieden), ein vollständiger ebenso wenig: `HunterOrSwiftEndsFirst` vergleicht Restlaufzeiten und ist im Fall „beide fehlen" nicht anwendbar, eine begründete Aufbaureihenfolge steht nirgends. Adressat der Inkonsistenz ist der Upstream.

## `AutodutyUpdateState` dupliziert `UpdateState`

`RSCommands_StateSpecialCommand.cs`: beide Methoden sind über rund 100 Zeilen wortgleich; abweichend sind nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. Kein Fehlverhalten, aber jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. Zusammenführbar über einen optionalen `TargetingType?`-Parameter.
