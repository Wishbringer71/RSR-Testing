# TODO — nur offene Arbeit

## Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Kein Diff-Review, sondern der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 (mechanische Scans, Rotationsmuster, Konfiguration/Oberfläche/Kommandos/IPC) sind abgeschlossen, s. AUDIT_LOG A8 und A10.

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei gelesen.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Zweiter Durchgang** mit denselben Scans über den bereinigten Baum.
- **Dokumentation** in `docs/rotation-flow/07-codebase-audit.md`.

## BMR-Timelinewerte werden nicht auf `<= 0` gefiltert, die Hints-Werte schon

`BossModUpdater` normalisiert `hintsRaidwide`/`hintsTankbuster` bei `<= 0` auf `float.MaxValue`, mit der Begründung „endpoint missing/SafeWrapper default or damage already resolved". `timelineRaidwide`/`timelineTankbuster` bleiben ungefiltert, gehen aber in dasselbe `Math.Min`.

**Am Quellcode belegt** (BossModReborn, `BossMod/Framework/IPCProvider.cs`): sämtliche Timeline-Endpunkte rechnen `(float)(next - DateTime.Now).TotalSeconds` und liefern nur bei fehlender Vorhersage `float.MaxValue`. Der Wert läuft also bei jedem Ereignis durch 0 ins Negative, bis die State Machine weiterschaltet. Dasselbe gilt für die Hints-Endpunkte, deren Filter genau deshalb existiert. Ein negativer Timelinewert gewinnt im `Math.Min` gegen jede gültige Hints-Vorhersage; alle Verbraucher fordern `> 0.6f` und sehen dann „keine Vorhersage". Betroffen sind `DataCenter.BMRTankbusterImminent` (5 Aufrufstellen), `ShouldAddDefenseArea` und 17 Job-Zeilen über `BMRShouldRefreshBefore`.

**Defektklasse, nicht Einzelfall:** ungefiltert sind alle sieben Timeline-Werte. Die Filterentscheidung ist aber je Wert verschieden: bei Schadensereignissen (Raidwide, Tankbuster, Knockback) bedeutet `<= 0` „vorbei", `float.MaxValue` ist die richtige Normalisierung. Bei Zustandsfenstern (Downtime, Vulnerable) trägt das Vorzeichen Information — negativ heißt „läuft bereits" —, ein pauschaler Filter würde sie zerstören. Offen bleibt die Auswertung in `ChurinDNC.cs:777-843` (Upstream), die `BMRNextDowntimeIn`/`-EndIn` ohne Vorzeichenprüfung liest und damit „Downtime läuft" nicht von „Downtime kommt gleich" unterscheidet; ob das der Absicht der Rotation widerspricht, ist ohne deren Autor nicht belegbar.

## Zyklus-Kommandos unterliegen der Toggle-Semantik der Einzelkommandos

Die Zustandswahl liegt an zwei Orten. Implizit in `AdjustStateType`: `/rotation Auto` schaltet dort über `UpdateTargetingIndex` selbst durch die Zielarten weiter, es sei denn `ToggleAuto` steht an, dann schaltet es ab — der Optionstext sagt das ausdrücklich („Normal behavior cycles between targeting settings"). Explizit in den fünf `Cycle*`-Methoden, die dieselbe Aufgabe noch einmal lösen, mit eigenem Ausschaltzweig, und über `CycleType` bzw. `DTRType` an das Zyklus-Chatkommando und den Klick in der Server-Info-Leiste gebunden sind.

Da die `Cycle*` am Ende ebenfalls `DoStateCommandType` rufen, greift `AdjustStateType` bei bereits aktivem Zustand auch dort: aus „Manual → Auto" wird „Off", und der Zyklus über die Zielarten bricht ab. Betroffen sind die fünf `Cycle*` plus das aufruferlose `IncrementState`; die automatischen Einschaltpfade in `RSCommands_Actions.cs` sind es nicht, weil sie sämtlich unter `!DataCenter.State` stehen und `AdjustStateType` dort nicht erreicht wird. Beide Toggle-Defaults sind `false`. Zu klären ist nicht nur, ob die `Cycle*` die Toggle-Semantik umgehen sollen, sondern ob die doppelte Zustandswahl bestehen bleibt.

## `StartOnFieldOpInCombat2` lässt Übungspuppen als Auslöser durch

`RSCommands_Actions.cs:465` überspringt jedes Ziel, das in `AllHostileTargets` steht **und** keine Übungspuppe ist. Der Gegner-Ausschluss ist beabsichtigt: `GetTargetsByRange(30f)` liefert ohne `getFriendly` alle Objekte, und der Zweig soll auf Mitspieler im Kampf reagieren, nicht auf Gegner. Durch die `&&`-Verknüpfung kehrt sich der Puppen-Ausschluss aber um — eine Übungspuppe ist ein Gegner und fällt damit aus dem `continue` heraus, ist also als einziger Gegnertyp weiter Auslöser. Richtig wäre `if (t != null && (DataCenter.AllHostileTargets.Contains(t) || ObjectHelper.IsDummy(t))) continue;`. **Auslösbarkeit belegt:** in allen drei abgefragten Gebieten stehen Übungspuppen — Bozjanische Südfront (14.1, 30.0), Zadnor (34.4, 35.5), Occult Crescent South Horn (37.6, 6.7) und North Horn (38.1, 39.3) —, und zwar an den Lagern, also dort, wo sich Spieler sammeln. Wer in 30 y einer Puppe steht, an der jemand übt, bekommt den automatischen Rotationsstart. Nebenbefund in derselben Schleife: Zeile 469-472 ist ein `if`-Block, dessen Rumpf nur noch aus einem auskommentierten Log besteht.

## Duty-Rotationen werden im Flächen- und im Einzelheilpfad unterschiedlich erreicht

In `CustomRotation_GCD` fragt der automatische Einzelheilzweig die Duty-Rotation bedingungslos ab, der Flächenheilzweig dagegen nur unter `IsInOccultCrescentOp || HasVariantCure`. Duty-Rotationen sind ohnehin an das Territorium gebunden, die Zusatzbedingung ist also entweder überflüssig oder im Einzelheilpfad versehentlich weggelassen. Zu klären, welche der beiden Varianten gewollt ist, dann angleichen.

## Selbstlernende AoE-Liste verunreinigt die Mitigations-Trigger

`Watcher.ActionFromEnemy` speichert jede gecastete Gegner-Aktion dauerhaft in `HostileCastingArea`, sobald sie alle Party-Mitglieder mit Schaden trifft (ab 4 Mitgliedern); die Option „Record AOE actions" ist standardmäßig an, ausgeliefert werden bereits 850 IDs. Eine ausweichbare Boden-AoE, in der einmal alle stehen bleiben, gilt danach für immer als Gruppen-AoE und setzt bei jedem Cast `AutoStatus.DefenseArea`. Zurücknehmen lässt sich das nur durch Editieren der Datei. Offen: ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen (Kandidat: `CastType`/`EffectRange` der Aktion) — ohne Spieldaten nicht entscheidbar, blind verschärfen wäre Raten.

## `SpreadDamagePaths` ist keine eigene Kategorie

Zwei der vier Pfade (`x6r9_loc01_t0a1`, `x6r9_loc02_t0a1`) stehen wortgleich auch in `SharedDamagePaths`, die anderen beiden sind laut ihrem eigenen Kommentar „AOE share markers", also Stack- und keine Spread-Marker. `IsCastingAreaVfx` prüft alle drei Listen, die Trennung trägt damit nichts. Prüfen, ob echte Spread-Marker fehlen (dann Liste füllen) oder ob sie ganz entfallen kann.

## Source-Generator liegt im Release-Paket

`RotationSolver.SourceGenerators.dll` ist in `latest.zip` des Releases 7.5.5.41+wsh1 enthalten, obwohl `PruneOutputDlls` in `RotationSolver.csproj` nur RotationSolver, RotationSolver.Basic und ECommons behalten soll. Zur Laufzeit nutzlos. Prüfen, warum die Prune-Regel den Analyzer nicht erfasst.

## Toter Code und Altlasten (Freigabe zur Löschung erforderlich)

Alle Punkte sind wirkungsneutral. Das Konfliktrisiko ist je Region gemessen (`git log -L <Bereich> upstream/main`), nicht über die Dateiaktivität geschätzt.

- **VPR-Redundanz:** `VPR_Reborn.cs:590-596` (`SwiftskinsBitePvE`) und `975-981` (`SwiftskinsStingPvE`). Der Zweig `!IsHunter && !IsSwift` ist eine echte Verschärfung des unmittelbar folgenden `!IsSwift` bei identischem Rumpf, kann das Verhalten also nachweislich nicht ändern. Beide Regionen wurden seit dem Fork-Punkt von keinem Upstream-Commit berührt.
- **Toter Code in `RotationConfigWindow.cs:5169-5190`:** die beiden privaten `BeginChild`-Überladungen und das nur von ihnen gerufene `IsFailed()` haben keinen Aufrufer. Die Wrapper hätten zudem einen latenten Fehler: sie geben bei `false` zurück, ohne dass ein `EndChild()` folgt. Region seit dem Fork-Punkt von zwei Upstream-Commits berührt (`f72a0a93`, `36af043e`).
- **`RSCommands_Actions.cs:24-34` `IncrementState()`** hat keinen Aufrufer. Enthält zudem denselben Toggle-Konflikt wie die Zyklus-Kommandos.
- **Zwei eingecheckte Sicherungsdateien:** `RotationSolver/RotationSolver.csproj.Backup.tmp` und `RotationSolver.SourceGenerators/RotationSolver.SourceGenerators.csproj.Backup.tmp` stammen aus dem Upgrade Dalamud-SDK 14.0.2 → 15.0.0 und enthalten den alten Stand samt veralteter Paketversionen. Nicht Teil des Builds, `.gitignore` hat kein `*.tmp`-Muster.

## `AutodutyUpdateState` dupliziert `UpdateState`

`RSCommands_StateSpecialCommand.cs`: beide Methoden sind über rund 100 Zeilen wortgleich; abweichend sind nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. Kein Fehlverhalten, aber jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. Zusammenführbar über einen optionalen `TargetingType?`-Parameter.
