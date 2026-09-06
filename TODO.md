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

### Leisteneintrag wird je Frame neu gesetzt

`MiscUpdater.UpdateEntry` läuft aus `MajorUpdater` in jedem Frame und weist `_dtrEntry.Text` eine neue `SeString` mit zwei Payloads sowie `OnClick` eine neue Lambda zu. Dalamuds `Text`-Setter vergleicht nicht, sondern setzt bedingungslos `Dirty = true`. Der Text ändert sich aber nur bei Zustandswechsel oder laufender Restzeit. `RSCommands.UpdateToast` in derselben Codebasis macht es richtig und vergleicht gegen `_lastToastMessage`; hier fehlt derselbe Vergleich.

Zwei weitere Befunde derselben Stelle, unabhängig von dieser Entscheidung zu beheben:

- **`/rotation Auto <Zahl>` wirkt nur bei bereits eingeschaltetem RSR.** Der Namensweg (`/rotation Auto LowHP`) setzt `Service.Config.TargetingIndex` direkt in `RSCommands_BasicInfo.cs:85`. Der numerische Weg reicht die Zahl nur als `index` weiter, und gesetzt wird sie erst in `UpdateTargetingIndex`, das über `AdjustStateType` hinter `if (DataCenter.State)` liegt. Im ausgeschalteten Zustand — dem normalen Fall beim Einschalten per Kommando — verpufft das Argument also, bei angeschaltetem `ToggleAuto` schaltet dasselbe Kommando stattdessen ab. Zwei Argumentformen desselben Kommandos mit unterschiedlicher Wirkung.
- **`DoOneCommandType` hat einen Parameter, der nie ausgeführt wird.** Der erste Parameter `Func<T, JobRole, string> sayout` wird im Rumpf nicht aufgerufen; die drei Aufrufstellen bauen dafür je eine Lambda. Auch der Rückgabewert von `doingSomething` wird verworfen (`_ = …`), womit die Generik samt `where T : struct, Enum` nichts trägt. Die Methode reduziert sich auf „Rolle ermitteln, bei `JobRole.None` abbrechen, Aktion ausführen".

Geprüfte Nicht-Fehlstelle: ein zu großer `TargetingIndex` kann nicht zum Indexfehler führen, `DataCenter.TargetingType` rechnet `% Count` und füllt eine leere Liste selbst auf (`DataCenter.cs:284-302`).

## `StartOnFieldOpInCombat2`: zwei Restbefunde nach dem Puppen-Fix

Die Nachprüfung der Wirkungskette bestätigt den Fix (AUDIT_LOG A11), legt aber zwei Punkte offen, die er nicht berührt.

**Der Gegner-Test prüft ein Surrogat.** `AllHostileTargets.Contains(t)` ist ein Identitätstest gegen eine mehrfach gefilterte Liste: `TargetUpdater.UpdateLists` nimmt einen Gegner nur auf, wenn er anvisierbar ist, unter 48 y steht, vom Augpunkt aus sichtbar ist (`CanSeeFrom`) und nicht unverwundbar. Ein Gegner ohne Sichtlinie oder in einer Unverwundbarkeitsphase steht damit zwar in `AllTargets` und wird von `GetTargetsByRange` geliefert, fällt aber nicht unter das `continue` — er kann den automatischen Start also weiterhin auslösen, obwohl der Zweig Gegner überspringen will. Inhaltlich richtig wäre der Typtest `t.IsEnemy()`, den `UpdateLists` selbst als Kriterium verwendet.

**Der eigene Spieler wird nicht ausgeschlossen.** `GetAllTargets` nimmt jedes `IBattleChara` auf, das anvisierbar und kein Begleiter ist — einschließlich des Spielers selbst. Belegt: `ObjectHelper.IsParty` (699-714) liefert für die eigene `GameObjectId` `true`, der Spieler wird in `UpdateLists` also der Party-Liste zugeordnet und dort per `continue` übersprungen; aus `AllTargets`, der Quelle von `GetTargetsByRange`, fällt er dadurch nicht heraus. `t.InCombat()` ist für ihn wahr, sobald er kämpft, womit der Zweig faktisch beim eigenen Kampfeintritt auslöst und die übrige Schleife entwertet. Das kollidiert mit `StartOnAttackedBySomeone2` weiter unten, das denselben Fall abdeckt, dabei aber `Manual` statt `Auto` wählt; da der Field-Op-Zweig zuerst läuft, gewinnt `Auto`. Der beim Puppen-Fix entfernte leere `if`-Block prüfte genau `t.GameObjectId != Player.Object.GameObjectId` und war die letzte Spur dieser Absicht — seine Entfernung war verhaltensneutral, hat aber das Signal getilgt, weshalb es hier festgehalten ist.

## Bozja-Flächenheilungen sind im automatischen Flächenheilpfad unerreichbar

`CustomRotation_GCD` hat vier Heilzweige. In den beiden `CommandStatus`-Zweigen (240, 282) und im automatischen Einzelheilzweig (299) wird die Duty-Rotation bedingungslos gefragt; nur der automatische Flächenheilzweig (257-263) stellt ihr `IsInOccultCrescentOp || HasVariantCure` voran.

**Wirkung belegt:** `CurrentDutyRotation` wird ohnehin territoriumsgebunden gesetzt (`RotationUpdater` über `DutyRotationChoice[TerritoryType]`), die Zusatzbedingung schränkt also innerhalb der Duties nochmals ein — und zwar genau auf Occult Crescent und Variant Dungeons. **Alle sieben Duty-Rotationen erhoben:** `HealAreaGCD` überschreiben nur `BozjaDefault` und `PhantomDefault`; `HealSingleGCD` zusätzlich `HardboiledDefault` und `VariantDefault`; `EmanationDefault`, `MonsterHunterDefault` und `OrbonneDefault` überschreiben keine Heilmethode. Die Bedingung deckt Phantom (Occult Crescent) und Variant ab — Variant hat aber gar kein `HealAreaGCD`, sodass im Flächenzweig ausschließlich Bozja betroffen ist. `BozjaDefault.HealAreaGCD` bietet `LostCureIII`, `LostCureIV` und `LostFullCure`; in Bozja und Zadnor sind beide Bedingungen falsch, weshalb diese drei Aktionen über den automatischen Flächenheilpfad nie erreicht werden. Über Einzelheil- und Befehlspfade sind sie erreichbar. Angleichen an die anderen drei Zweige, also Bedingung entfernen.

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

**Korrektur einer früheren Aussage:** Ursache ist auch nicht das `OutputPath` in `RotationSolver.csproj`. Der Veröffentlichungs-Workflow überschreibt es: `publish.yaml:42` baut mit `--output .\build`, und `:47` lädt `./build/RotationSolver/latest.zip` hoch. Ein `dotnet build --output` auf ein Projekt mit Projektverweisen legt die Ausgaben aller beteiligten Projekte in dasselbe Verzeichnis, aus dem DalamudPackager dann packt. Eine Trennung der Ausgabeverzeichnisse in den Projektdateien bliebe daher wirkungslos. Ansatzpunkte sind stattdessen: `--output` aus dem Workflow entfernen, oder die Paket- und Dokumentationserzeugung an den Release-Build koppeln, oder einen Ausschluss in DalamudPackager konfigurieren.

## Angriffskonfiguration: zweite, veraltete Beschreibungsquelle ohne Aufrufer

`RotationConfigWindow.cs:2156-2174`. `GetHostileTypeDescription` und `SetTargetingType` haben keinen Aufrufer mehr. Historie: `813c7d73` („Add support for AutoDuty plugin") führte beide **mit** Aufrufern ein — eine Zeile `Current Targeting Mode: …` und einen Schalter auf `AllTargetsCanAttack` —, `e3b57004` („7.45 hotfix 1 update") entfernte die Aufrufer und ließ die Definitionen stehen. Also abgelöst, nicht unverdrahtet.

Der Rest ist inhaltlich veraltet: `GetHostileTypeDescription` kennt vier der fünf `TargetHostileType`-Werte und liefert für `SoloDeepDungeonSmart` „Unknown Target Type"; die übrigen vier Texte weichen von den `[Description]`-Attributen des Enums ab und sind teils irreführend („Targets Have A Target" statt „Previously engaged targets (Non-Tanks)"). Damit liegen zwei Beschreibungsquellen für dieselbe Einstellung im Baum, von denen die aktive die Attribute sind (`ControlWindow.cs:214` über `GetDescription()`, und der attributgesteuerte Konfigurationszeichner).

Zu entscheiden: beide Methoden entfernen (Löschung freigabepflichtig) oder `GetHostileTypeDescription` auf `GetDescription()` zurückführen, falls die Anzeige wiederkommen soll. Offen bleibt, ob der Wegfall der Schnellumschaltung auf den Tank-Modus in `e3b57004` beabsichtigt war oder ein Kollateralschaden des Hotfixes — ohne Upstream-Kontext nicht entscheidbar.

## VPR: leerer Zweig einer Struktur, die anderswo eine echte Entscheidung trägt

`VPR_Reborn.cs:591-597` und `975-981`. Nicht zu löschen — die Begründung steht in AUDIT_LOG A11. Das Muster `!HasHunterAndSwift` kommt viermal vor, und der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (751-807) Inhalt, nämlich eine positionsbewusste Wahl samt Wechselsperre. An der Bite- und der Sting-Stelle ist er eine Kopie des Folgezweigs, an der Den-Stelle (424-493) fehlt er ganz. Ein fehlender Inhalt lässt sich nicht belegen (die AoE-Kette hat keine Positionals, bei Sting wird der Positionsfall oberhalb über `HasHind`/`HasFlank` entschieden), ein vollständiger ebenso wenig: `HunterOrSwiftEndsFirst` vergleicht Restlaufzeiten und ist im Fall „beide fehlen" nicht anwendbar, eine begründete Aufbaureihenfolge steht nirgends. Adressat der Inkonsistenz ist der Upstream.

## `AutodutyUpdateState` dupliziert `UpdateState`

`RSCommands_StateSpecialCommand.cs`: beide Methoden sind über rund 100 Zeilen wortgleich; abweichend sind nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. Kein Fehlverhalten, aber jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. Zusammenführbar über einen optionalen `TargetingType?`-Parameter.
