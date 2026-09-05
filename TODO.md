# TODO — nur offene Arbeit

## Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Kein Diff-Review, sondern der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 (mechanische Scans, Rotationsmuster, Konfiguration/Oberfläche/Kommandos/IPC) sind abgeschlossen, s. AUDIT_LOG A8 und A10.

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei gelesen.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Zweiter Durchgang** mit denselben Scans über den bereinigten Baum.
- **Dokumentation** in `docs/rotation-flow/07-codebase-audit.md`.

## BMR-Timelinewerte werden nicht auf `<= 0` gefiltert, die Hints-Werte schon

`BossModUpdater` normalisiert `hintsRaidwide`/`hintsTankbuster` bei `<= 0` auf `float.MaxValue`, mit der Begründung „endpoint missing/SafeWrapper default or damage already resolved". `timelineRaidwide`/`timelineTankbuster` kommen über denselben `SafeWrapper.AnyException` und bleiben ungefiltert, gehen aber in dasselbe `Math.Min`. Eine 0 aus der Timeline schlägt damit jede gültige Hints-Vorhersage, und alle Verbraucher (`BMRRaidwideWithin` fordert `> 0f`, `ShouldAddDefenseArea` `> 0.6f`) werten 0 als „keine Vorhersage" — die Mitigation unterbleibt also, obwohl eine Vorhersage vorlag. Ob die Timeline-Endpunkte tatsächlich 0 oder negativ liefern, ist offline nicht entscheidbar; ablesbar ist es im Debug-Reiter an `BMRDebugTimelineRaidwide`/`-Tankbuster` gegen `BMRDebugHintsRaidwide`/`-Tankbuster`. Erst danach angleichen, weil die symmetrische Filterung die Mitigation in genau diesem Fall häufiger auslöst.

## Zyklus-Kommandos unterliegen der Toggle-Semantik der Einzelkommandos

`CycleStateAuto`, `CycleStateManualAuto`, `CycleStateWithAllTargetTypes` und `CycleStateWithOneTargetTypes` implementieren ihre Zustandsfolge selbst, einschließlich eines eigenen Ausschaltzweigs, rufen dann aber `DoStateCommandType`, das über `AdjustStateType` die Optionen `ToggleAuto`/`ToggleManual` anwendet. Steht eine davon an, wird aus dem Wechsel „Manual → Auto" ein „Off", und der Zyklus über die Zielarten bricht ab. Die Optionstexte beziehen sich ausdrücklich auf `/rotation Auto` bzw. `/rotation Manual`, nicht auf die Zyklus-Kommandos; beide Defaults sind `false`. Zu klären, ob die Zyklus-Kommandos die Toggle-Semantik umgehen sollen (dann ein Parameter `applyToggle` an `DoStateCommandType`) oder ob das gewollt ist.

## `StartOnFieldOpInCombat2` filtert Gegner statt Übungspuppen aus

`RSCommands_Actions.cs:465` überspringt jedes Ziel, das in `AllHostileTargets` steht **und** keine Übungspuppe ist. Damit lösen ausgerechnet die echten Gegner den automatischen Start nicht aus, eine im Kampf befindliche Übungspuppe dagegen schon. Gemeint war vermutlich das Muster aus Zeile 509 (`!ObjectHelper.IsDummy(target)`) als eigene Bedingung. Zu klären, ob der Zweig auf Gegner oder auf andere Spieler reagieren soll, dann die Bedingung entsprechend trennen.

## Duty-Rotationen werden im Flächen- und im Einzelheilpfad unterschiedlich erreicht

In `CustomRotation_GCD` fragt der automatische Einzelheilzweig die Duty-Rotation bedingungslos ab, der Flächenheilzweig dagegen nur unter `IsInOccultCrescentOp || HasVariantCure`. Duty-Rotationen sind ohnehin an das Territorium gebunden, die Zusatzbedingung ist also entweder überflüssig oder im Einzelheilpfad versehentlich weggelassen. Zu klären, welche der beiden Varianten gewollt ist, dann angleichen.

## Selbstlernende AoE-Liste verunreinigt die Mitigations-Trigger

`Watcher.ActionFromEnemy` speichert jede gecastete Gegner-Aktion dauerhaft in `HostileCastingArea`, sobald sie alle Party-Mitglieder mit Schaden trifft (ab 4 Mitgliedern); die Option „Record AOE actions" ist standardmäßig an, ausgeliefert werden bereits 850 IDs. Eine ausweichbare Boden-AoE, in der einmal alle stehen bleiben, gilt danach für immer als Gruppen-AoE und setzt bei jedem Cast `AutoStatus.DefenseArea`. Zurücknehmen lässt sich das nur durch Editieren der Datei. Offen: ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen (Kandidat: `CastType`/`EffectRange` der Aktion) — ohne Spieldaten nicht entscheidbar, blind verschärfen wäre Raten.

## `SpreadDamagePaths` ist keine eigene Kategorie

Zwei der vier Pfade (`x6r9_loc01_t0a1`, `x6r9_loc02_t0a1`) stehen wortgleich auch in `SharedDamagePaths`, die anderen beiden sind laut ihrem eigenen Kommentar „AOE share markers", also Stack- und keine Spread-Marker. `IsCastingAreaVfx` prüft alle drei Listen, die Trennung trägt damit nichts. Prüfen, ob echte Spread-Marker fehlen (dann Liste füllen) oder ob sie ganz entfallen kann.

## Source-Generator liegt im Release-Paket

`RotationSolver.SourceGenerators.dll` ist in `latest.zip` des Releases 7.5.5.41+wsh1 enthalten, obwohl `PruneOutputDlls` in `RotationSolver.csproj` nur RotationSolver, RotationSolver.Basic und ECommons behalten soll. Zur Laufzeit nutzlos. Prüfen, warum die Prune-Regel den Analyzer nicht erfasst.

## Toter Code und Altlasten (Freigabe zur Löschung erforderlich)

Alle drei Punkte sind wirkungsneutral; eine Entfernung erhöht nur die Abweichung zum Upstream, deshalb hier gemeldet statt umgesetzt.

- **VPR-Redundanz:** `VPR_Reborn.cs:590-605` und `973-997`. Der Zweig `!IsHunter && !IsSwift` ist eine echte Verschärfung des unmittelbar folgenden `!IsSwift` bei identischem Rumpf (`SwiftskinsBitePvE.CanUse(...)`), kann das Verhalten also nachweislich nicht ändern.
- **Toter Code in `RotationConfigWindow.cs:5169-5190`:** die beiden privaten `BeginChild`-Überladungen und das nur von ihnen gerufene `IsFailed()` haben keinen Aufrufer. Die Wrapper hätten zudem einen latenten Fehler: sie geben bei `false` zurück, ohne dass ein `EndChild()` folgt.
- **Zwei eingecheckte Sicherungsdateien:** `RotationSolver/RotationSolver.csproj.Backup.tmp` und `RotationSolver.SourceGenerators/RotationSolver.SourceGenerators.csproj.Backup.tmp` stammen aus dem Upgrade Dalamud-SDK 14.0.2 → 15.0.0 und enthalten den alten Stand samt veralteter Paketversionen. Nicht Teil des Builds.

## `AutodutyUpdateState` dupliziert `UpdateState`

`RSCommands_StateSpecialCommand.cs`: beide Methoden sind über rund 100 Zeilen wortgleich; abweichend sind nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. Kein Fehlverhalten, aber jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. Zusammenführbar über einen optionalen `TargetingType?`-Parameter.
