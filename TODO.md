# TODO — nur offene Arbeit

## Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Kein Diff-Review, sondern der ganze Baum, Upstream-Code eingeschlossen. Phase 1 (mechanische Scans) ist abgeschlossen, s. AUDIT_LOG A8.

- **Kern tief lesen:** StateUpdater · TargetUpdater · DataCenter · ActionTargetInfo (Zielwahl) · BaseAction/ActionBasicInfo (CanUse/Use) · CustomRotation_Ability/GCD (Dispatch) · Watcher · MajorUpdater · ObjectHelper/StatusHelper.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; Extra-Rotationen nach denselben Mustern.
- **Zweiter Durchgang** mit denselben Scans über den bereinigten Baum.
- **Dokumentation** in `docs/rotation-flow/07-codebase-audit.md`.

## Duty-Rotationen werden im Flächen- und im Einzelheilpfad unterschiedlich erreicht

In `CustomRotation_GCD` fragt der automatische Einzelheilzweig die Duty-Rotation bedingungslos ab, der Flächenheilzweig dagegen nur unter `IsInOccultCrescentOp || HasVariantCure`. Duty-Rotationen sind ohnehin an das Territorium gebunden, die Zusatzbedingung ist also entweder überflüssig oder im Einzelheilpfad versehentlich weggelassen. Zu klären, welche der beiden Varianten gewollt ist, dann angleichen.

## Selbstlernende AoE-Liste verunreinigt die Mitigations-Trigger

`Watcher.ActionFromEnemy` speichert jede gecastete Gegner-Aktion dauerhaft in `HostileCastingArea`, sobald sie alle Party-Mitglieder mit Schaden trifft (ab 4 Mitgliedern); die Option „Record AOE actions" ist standardmäßig an, ausgeliefert werden bereits 850 IDs. Eine ausweichbare Boden-AoE, in der einmal alle stehen bleiben, gilt danach für immer als Gruppen-AoE und setzt bei jedem Cast `AutoStatus.DefenseArea`. Zurücknehmen lässt sich das nur durch Editieren der Datei. Offen: ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen (Kandidat: `CastType`/`EffectRange` der Aktion) — ohne Spieldaten nicht entscheidbar, blind verschärfen wäre Raten.

## `SpreadDamagePaths` ist keine eigene Kategorie

Zwei der vier Pfade (`x6r9_loc01_t0a1`, `x6r9_loc02_t0a1`) stehen wortgleich auch in `SharedDamagePaths`, die anderen beiden sind laut ihrem eigenen Kommentar „AOE share markers", also Stack- und keine Spread-Marker. `IsCastingAreaVfx` prüft alle drei Listen, die Trennung trägt damit nichts. Prüfen, ob echte Spread-Marker fehlen (dann Liste füllen) oder ob sie ganz entfallen kann.

## Source-Generator liegt im Release-Paket

`RotationSolver.SourceGenerators.dll` ist in `latest.zip` des Releases 7.5.5.41+wsh1 enthalten, obwohl `PruneOutputDlls` in `RotationSolver.csproj` nur RotationSolver, RotationSolver.Basic und ECommons behalten soll. Zur Laufzeit nutzlos. Prüfen, warum die Prune-Regel den Analyzer nicht erfasst.
