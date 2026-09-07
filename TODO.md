# TODO — nur offene Arbeit

Getrennt nach Defekt (Abweichung vom beabsichtigten Verhalten), technischer Schuld (bewusst eingegangener Kompromiss mit Auflösungsbedingung) und offener Arbeit. Je Eintrag der betroffene Personenkreis: **N** Endnutzer des Plugins · **R** Autoren abgeleiteter Rotationen, die `RotationSolver.Basic` als Paket beziehen · **U** Upstream-Pflege.

## Defekte

### ChurinDNC wertet die BMR-Downtime ohne Vorzeichenprüfung aus · N, U

`ChurinDNC.cs:777-843` (Upstream) liest `BMRNextDowntimeIn`/`-EndIn` ohne Vorzeichenprüfung. BossModReborn liefert diese Werte als `(Aktivierung − jetzt)`, sie sind während einer laufenden Downtime also negativ, und die Rotation kann „Downtime läuft" nicht von „Downtime kommt gleich" unterscheiden: `if (BMRNextDowntimeIn >= 15f) return;` kehrt dann nicht zurück, und die folgende `<`-Bedingung ist immer erfüllt. Die Normalisierung der Schadensvorhersagen ist erledigt (AUDIT_LOG A11); hier wäre ein Filter falsch, weil das Vorzeichen die Information trägt.

Nicht behoben, weil die Absicht dieser fremden Rotation ohne ihren Autor nicht belegbar ist und eine Änderung ohne Spieltest nicht abzusichern wäre. Auflösung: Rückfrage an den Upstream-Autor oder Laufzeitbeobachtung.

### Das Fork-NuGet-Paket trägt die Identität des Upstream-Pakets · R

`RotationSolver.Basic.csproj` setzt `PackageId` auf `RotationSolverReborn.Basic` — denselben Bezeichner, unter dem Upstream veröffentlicht. `publish.yaml:41` übergibt `PackageVersion=${{ env.numericVersion }}`, also die Version **ohne** das Fork-Suffix. Das im Release-ZIP mitgelieferte `.nupkg` heißt damit `RotationSolverReborn.Basic 7.5.5.41` und ist von der gleichnamigen Upstream-Fassung nicht zu unterscheiden, obwohl es einen anderen Inhalt hat.

Das Suffix nachzureichen behebt es nicht: NuGet entfernt SemVer-2.0-Build-Metadaten bei der Normalisierung — `1.0.7+r3456` wird als `1.0.7` behandelt ([Package versioning](https://learn.microsoft.com/nuget/concepts/package-versioning#normalized-version-numbers)) —, und dieselbe Quelle hält fest, dass ein Repository zwei Pakete gleicher normalisierter Version nicht als verschiedene Pakete führen soll. Ein unterscheidendes Merkmal wäre nur ein eigener `PackageId` oder ein Prerelease-Label (`-wsh1`), das in die Identität eingeht.

**Kosten:** Wer das Paket aus dem Release-ZIP in einen lokalen Feed legt, kann es nicht vom Upstream-Paket unterscheiden; NuGet löst je Identität einmal auf und cacht. Genau in diesem Fall werden die entfernten Member (siehe unten) als unerklärlicher Compilefehler sichtbar. Kein Fehlverhalten zur Laufzeit des Plugins.

**Auflösungsbedingung:** Die Wahl zwischen eigenem `PackageId`, Prerelease-Label und dem Verzicht auf die Paketauslieferung trifft der Auftraggeber; alle drei berühren die Autoren abgeleiteter Rotationen unterschiedlich. Zusammen mit dem Eintrag zum Release-Ballast zu entscheiden, der dasselbe `.nupkg` betrifft.

### Argumentlose `IsLastAction()`-Vergleiche sind konstant wahr · N, U

`IsLastAction()`, `IsLastGCD()` und `IsLastAbility()` sind `params ActionID[]`-Überladungen. Ohne Argument prüft `IsActionID` eine leere Liste und liefert `false` (`IActionHelper.cs:196-211`). Der Ausdruck `IsLastAction() == IsLastGCD()` ist damit `false == false` und immer wahr; für `IsLastAction() == IsLastAbility()` gilt dasselbe. Die gemeinte Prüfung existiert bereits als `IActionHelper.IsLastActionGCD()` (`DataCenter.LastAction == DataCenter.LastGCD`) und wird in `CustomRotation_Ability.cs:688`, `697` und `705` korrekt verwendet.

Fünf Fundstellen, alle aus Upstream übernommen und dort unverändert vorhanden:

- `WHM_Reborn.cs:135` (zweimal) und `BeirutaWHM.cs:401-402` (zweimal) — sollen Thin Air auf das Weave-Fenster unmittelbar nach einem GCD beschränken.
- `DRG_Reborn.cs:131` — dieselbe Absicht vor Stardiver.
- `CustomRotation_OtherInfo.cs:1798` in `HasWeaved()`. Der Kommentar darüber schreibt die gemeinte Prüfung wörtlich aus, der Code führt sie nicht aus — nach der Auslegungsregel ist der Widerspruch der Befund, nicht der Kommentar.

**Wirkung:** In den vier Rotationsstellen entfällt eine Einschränkung, die nie gegriffen hat; Thin Air und Stardiver können damit in jedem Weave-Slot statt nur im ersten fallen. Schwerer wiegt `HasWeaved()`: über `CanEarlyWeave` (`CustomRotation_OtherInfo.cs:1353`) kollabiert `(!HasWeaved() || WeaponRemain > LateWeaveWindow)` auf die zweite Hälfte, die „noch nicht geweavt"-Bedingung fällt also weg. Verbraucher sind `ChurinMNK` (drei Stellen) und `ChurinBRD` (vier); die Reborn-Rotationen nutzen `CanEarlyWeave` nicht.

**Auflösung:** Ersetzung durch `IsLastActionGCD()` beziehungsweise durch eine gleichwertige Prüfung für `HasWeaved()`. Für die beiden Churin-Rotationen ist die Absicht des fremden Autors zu berücksichtigen, weil die Behebung deren Weave-Zeitpunkte tatsächlich verschiebt.

### Die Heilzielauswahl legt die Invulnerabilitätsprüfung invertiert aus · N

`NoNeedHealingInvuln()` (`StatusHelper.cs:653`) ist `WillStatusEndGCD(2, 0, false, NoNeedHealingStatus)` und liefert **true**, wenn der Schutzstatus fehlt oder binnen zwei GCDs endet — also „heilen ist wieder sinnvoll". Der Name sagt das Gegenteil, und die beiden Aufrufstellen folgen verschiedenen Lesarten:

- `StateUpdater.cs:726` und `:771` folgen der Implementierung: `if (h == 0 || !NoNeedHealingInvuln()) return false;` — kein Heilflag, solange der Schutz läuft. **Richtig.**
- `ActionTargetInfo.cs:3536` in `GeneralHealTarget` folgt dem Namen: `if (!o.NoNeedHealingInvuln()) healingNeededObjs.Add(o);` — aufgenommen wird, wessen Schutz **noch läuft**. **Invertiert.**

**Wirkung:** In die priorisierte Zielliste kommen nur Spieler mit laufendem Living Dead, Holmgang oder Superbolide; alle übrigen fallen heraus. Normalerweise ist die Liste damit leer, und `FindHealTarget` greift auf `filteredGameObjects[0]` zurück (`ActionTargetInfo.cs:3524`) — die gesamte Sortierung nach Gesundheit und die Rollenreihenfolge Selbst → Heiler → Tank laufen also ins Leere, ohne dass es auffällt. Trägt dagegen ein Tank gerade Living Dead und braucht ein anderer Spieler Heilung, wird **der geschützte Tank bevorzugt** — das Gegenteil der Absicht, und beim Dunkelritter verhindert es womöglich den Tod, der die Selbstheilung von Walking Dead erst freischaltet.

**Auflösung:** `if (o.NoNeedHealingInvuln())` an der einen Stelle. Der Wirkungsbereich ist allerdings die zentrale Heilzielwahl aller Jobs: Nach der Korrektur greift die Priorisierung erstmals wirklich, was das Heilverhalten breit verändert. Freigabepflichtig, nicht nebenbei zu ändern. Der irreführende Name ist getrennt zu behandeln — ihn anzupassen, ohne die Aufrufstellen zu prüfen, würde den Beleg tilgen.

### Die Heilunterdrückung bei Unverwundbarkeit prüft den Gesundheitsstand nicht · N

`NoNeedHealingStatus` unterdrückt Heilung, solange ein Schutzstatus läuft. Die dahinterliegende Annahme — wer nicht sterben kann, braucht keine Heilung — gilt nur, wenn der Tank den **Ablauf** des Schutzes überlebt. Genau das prüft die Regel nicht.

Zündet ein Paladin Hallowed Ground bei fünf Prozent Gesundheit, um einen Schlag zu überstehen, lässt die Fähigkeit seine HP unverändert. Nach zehn Sekunden steht er mit denselben fünf Prozent da — und die zehn Sekunden waren das einzige Fenster, in dem ohne Gegendruck hätte aufgeheilt werden können. Dasselbe gilt für Holmgang und erst recht für Superbolide, das die HP sofort auf 1 setzt.

**Die Unverwundbarkeit ist damit nicht der Grund, Heilung auszusetzen, sondern die beste Gelegenheit, sie anzubringen.** Die Zwei-GCD-Freigabe vor Ablauf mildert das, reicht aber nicht: Einen Tank von wenigen Prozent auf sicher zu bringen, dauert länger als zwei GCDs.

| Job | Fähigkeit | Wirkung auf die HP | Heilung im Schutzfenster |
|---|---|---|---|
| PLD | Hallowed Ground | unverändert, kein Schaden geht durch | nötig, wenn beim Zünden wenig HP standen |
| WAR | Holmgang | fallen nicht unter 1 | nötig, sobald er heruntergedrückt wurde |
| GNB | Superbolide | **sofort auf 1** | zwingend |
| DRK | Living Dead | Tod wird in Walking Dead umgewandelt | Phase 1 **schädlich**, Phase 2 erforderlich |

**Zwei frühere Aussagen dieses Eintrags sind damit falsch** und hier ersetzt: dass ein Schild auf Holmgang oder Superbolide wirkungslos sei (er überdauert die Phase und liegt, wenn der Tank am verwundbarsten ist), und dass bei Hallowed Ground Heilung wirklich unnötig sei (sie ist es nur, wenn der Paladin mit hohen HP gezündet hat — was die Regel nicht wissen kann).

**Richtige Konstruktion:** nicht ausschließen, sondern **in der Priorität herabstufen**. Ein geschützter Tank soll hinter jedem ungeschützten Gruppenmitglied stehen, aber geheilt werden, wenn sonst niemand Bedarf hat — im Schutzfenster, wo es am sichersten ist. `GeneralHealTarget` besitzt bereits eine Rangfolge (Selbst → Heiler → Tank → niedrigste Gesundheit); nötig wäre eine zusätzliche Stufe, keine Ausschlussliste. Einzige echte Ausnahme bleibt der Dunkelritter in Phase 1.

**Zusätzlich, unabhängig davon:** Keine der vier `HallowedGround`-Ids (82, 1302, 2287, 2794) steht in der Liste, und von vier Holmgang-Ids nur `Holmgang_409`, während `BeirutaWHM.cs:236` gegen `StatusID.Holmgang` (88) prüft. Welche das Spiel setzt, ist statisch nicht zu klären. Solange die Regel in ihrer heutigen Form falsch ist, wäre das Ergänzen der fehlenden Ids allerdings eine Verschlimmerung — erst die Konstruktion, dann die Abdeckung.

**Auflösungsbedingung:** gemeinsam mit dem vorigen Punkt, weil beide dieselbe Prüfung betreffen. Beide sind in `docs/rotation-flow/09-tank-selfprotection.md` als Teil eines umfassenderen Bildes eingeordnet: Dort sind sämtliche Tank-Selbstschutzmechaniken danach getrennt, ob sie einen Auslöser haben, den fremde Heilung oder ein fremder Schild abfangen kann. Die Einzelheiten und der Umsetzungsplan stehen dort, nicht hier.

## Technische Schuld

### Doppelte Zustandswahl in den Zustandskommandos · N

Die Zustandswahl liegt an zwei Orten: implizit in `AdjustStateType`, wo `/rotation Auto` über `UpdateTargetingIndex` selbst durch die Zielarten schaltet, sofern `ToggleAuto` aus ist; explizit in den fünf `Cycle*`-Methoden, die dieselbe Aufgabe erneut lösen und über `CycleType` bzw. `DTRType` am Chatkommando und am Leistenklick hängen. Da die `Cycle*` ebenfalls `DoStateCommandType` rufen, greift `AdjustStateType` auch dort; die Toggle-Optionen wirken dadurch als Krücken für fehlende Übergänge, statt als unabhängige Achse.

**Kosten:** `DTRAllAuto` kollabiert mit aktivem `ToggleAuto` auf Off ↔ Auto(0), die Zielarten-Rotation ist dann tot. Umgekehrt ist `ToggleAuto` bei `DTRManualAuto` der einzige Ausschaltweg über die Leiste — ein pauschales Umgehen der Toggle-Auswertung würde ihn beseitigen.

**Auflösungsbedingung:** erst mit einer Möglichkeit zur Laufzeitbeobachtung; ein Zustandsautomat mit acht Zuständen, fünf Zykluswegen und zwei Schaltern ist statisch nicht abzusichern. Bei einem Eingriff ist der Persistenzvertrag zu beachten: `DTRType` und `CycleType` liegen als Ordinalzahlen in der Nutzerkonfiguration, ihre Reihenfolge ist nicht frei änderbar.

Geprüfte Nicht-Fehlstellen: `DTRManualAuto` bildet den vom Enum-Text beschriebenen Zwei-Zustands-Zyklus ab (kein Fehler, AUDIT_LOG A14); ein zu großer `TargetingIndex` kann keinen Indexfehler auslösen, `DataCenter.TargetingType` rechnet `% Count` (`DataCenter.cs:284-302`).

### Selbstlernende AoE-Liste wächst ohne fachliche Schranke · N

`Watcher.ActionFromEnemy:111-148` nimmt eine Gegner-Aktion dauerhaft in `HostileCastingArea` auf, wenn die Party mindestens vier Mitglieder hat, die Aktion eine Wirkzeit besitzt, zur Kategorie Spell/Weaponskill/Ability gehört und **jedes** Party-Mitglied im selben Effektsatz Schaden genommen hat. „Record AOE actions" ist standardmäßig an.

**Korrektur einer früheren Aussage:** Der Eintrag behauptete, ein gelernter Eintrag lasse sich nur durch Editieren der Datei zurücknehmen. Das ist zweifach widerlegt — `RotationConfigWindow.cs:3745` bietet „Reset and Update AOE List" (`ResetHostileCastingArea`, lädt die gepflegte Liste neu), und `DrawActionsList` erlaubt das Entfernen einzelner Einträge über Kontextmenü und Entf-Taste. Die Codedokumentation empfiehlt den Reset ausdrücklich nach jedem Patch. Damit ist dies kein Defekt, sondern eine Automatik mit vorhandenen Korrekturwerkzeugen.

**Kosten:** Zwischen einer falsch gelernten Aktion und der nächsten Nutzerkorrektur mitigiert RSR auf einen ausweichbaren Effekt. Durch die Reichweitenprüfung in `IsHostileCastingArea` entschärft.

**Auflösungsbedingung:** ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen (Kandidat: `CastType`/`EffectRange`), ist ohne Spieldaten nicht entscheidbar. Eine Verschärfung wäre eine Verhaltensänderung ohne Nachweismöglichkeit und gehörte deshalb hinter eine eigene Option, nicht in den Standardpfad. Geprüfte Nicht-Fehlstelle: das Speichern läuft asynchron, kein blockierendes Schreiben im Kampfpfad.

### `SpreadDamagePaths` enthält keinen Spread-Marker · N

`DataCenter.cs:2036-2043`. Zwei der vier Pfade stehen wortgleich in `SharedDamagePaths` (2025-2026), die anderen beiden sind laut eigenem Kommentar „AOE share markers", also ebenfalls Stack-Marker. Ohne Fehlwirkung, weil `IsCastingAreaVfx` alle drei Listen prüft. **Kosten:** eine Kategorie, die etwas anderes verspricht, als sie enthält. **Auflösung:** entweder echte Spread-Marker ergänzen oder die Liste streichen — beides erfordert Spieldaten, die offline nicht vorliegen. Nebenbefund: `SharedDamagePaths` führt `vfx/lockon/eff/com_trg01_0c` zweimal (2022 und 2024), im `FrozenSet` folgenlos.

### Release-Paket enthält vermeidbaren Ballast · N, R

**Am Artefakt belegt** (`latest.zip` von 7.5.5.41+wsh1, 5,35 MB): Nutzlast sind `RotationSolver.dll`, `RotationSolver.Basic.dll`, `ECommons.dll` und `RotationSolver.json`; dazu kommen `RotationSolver.Basic.xml` (7,52 MB), der Analyzer samt Symbolen (5,59 MB), das NuGet-Paket (1,54 MB) und `RotationSolver.Basic.pdb` (1,27 MB).

**Ursache** ist weder die Prune-Regel — `PruneOutputDlls` arbeitet auf `ReferenceCopyLocalPaths` und erfasst nichts davon — noch das `OutputPath` der Projektdatei: `publish.yaml:42` baut mit `--output .\build`, wodurch die Ausgaben aller beteiligten Projekte in einem Verzeichnis landen, das DalamudPackager packt. `GeneratePackageOnBuild` bedient dabei bewusst die Autoren abgeleiteter Rotationen.

**Kosten:** 5,35 MB Download statt rund 1,8 MB, funktional folgenlos.

**Auflösungsbedingung:** Der naheliegende Weg trägt nicht — der Standard-Target von DalamudPackager reicht `Exclude` nicht durch und läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt; diese müsste den vollständigen Task-Aufruf samt aller Manifest-Felder nachbauen. `Exclude` vergleicht exakt über `List.Contains` (`DalamudPackager.cs:187`), kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Der Build-Workflow kompiliert nur und prüft das Paket nicht. Aufgreifen erst, wenn der Veröffentlichungspfad prüfbar ist. Geprüft: die XML-Dokumentation wird zur Laufzeit nicht gelesen.

### VPR: leerer Zweig einer Struktur, die anderswo eine Entscheidung trägt · U

`VPR_Reborn.cs:591-597` und `975-981`. Das Muster `!HasHunterAndSwift` kommt viermal vor; der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (751-807) Inhalt, an der Den-Stelle (424-493) fehlt er ganz. Weder ein fehlender Inhalt noch dessen Entbehrlichkeit ist belegbar. **Bewusst nicht gelöscht** (AUDIT_LOG A11): Die Entfernung wäre verhaltensneutral, würde aber die Asymmetrie verdecken, die den Befund sichtbar macht. **Auflösung:** Adressat ist der Upstream.

### `AutodutyUpdateState` dupliziert `UpdateState` · U

`RSCommands_StateSpecialCommand.cs`: rund 100 wortgleiche Zeilen, abweichend nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. **Kosten:** jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. **Auflösung:** über einen optionalen `TargetingType?`-Parameter zusammenführen, sobald an dieser Stelle ohnehin gearbeitet wird.

### Zwei entfernte öffentliche Member seit dem letzten Release · R

`scan7.py` misst die Paketoberfläche von `RotationSolver.Basic` gegen den Tag `7.5.5.41+wsh1`: `CustomRotation_BasicInfo.HasHostileCountAoeMitigation` (`public virtual`) und `ActionConfig.ShouldCheckTargetStatus` sind seither ersatzlos entfallen. Beide Entfernungen sind sachlich belegt — das Flag öffnete die gesamte Defensivkette, die Option las niemand —, aber sie waren im ausgelieferten Paket enthalten.

**Kosten:** Eine abgeleitete Rotation, die das Flag überschreibt oder die Option liest, kompiliert gegen die nächste Paketversion nicht mehr. Für die gespeicherte Nutzerkonfiguration folgenlos, weil Dalamud fehlende Member beim Deserialisieren überspringt.

**Auflösungsbedingung:** Der Ausweis erfolgt in `CHANGELOG.md`, weil die Versionsnummer ihn nicht tragen kann: sie folgt der Upstream-Version, und das Fork-Suffix ist SemVer-Build-Metadatum ohne Präzedenz. Offen bleibt allein, den Abschnitt „Unreleased" bei der nächsten Versionsvergabe auf die dann vergebene Version zu setzen. Der Zeitpunkt der Veröffentlichung liegt beim Auftraggeber.

### Übertragung der Mitigations-Synergie auf weitere Doppelnutzen-Aktionen · N

Schritt 3 aus `docs/rotation-flow/08-mitigation-synergy.md`. Die Schritte 1 und 2 — Messung und Aussetzbedingung für Sanctus — sind umgesetzt (AUDIT_LOG A20). Offen ist die Übertragung auf weitere Aktionen, die Schaden erzeugen und zugleich Schaden vermeiden.

**Zur Führung dieses Punktes:** Die Einzelheiten stehen im Konzeptdokument, nicht hier. `TODO.md` führt die offene Arbeit und verweist; eine zweite Beschreibung derselben Sache würde mit der ersten auseinanderlaufen. Was hier stehen muss, ist allein, dass noch etwas offen ist und wo es beschrieben wird.

**Auflösungsbedingung:** erst nach Beobachtung der Schritte 1 und 2 im Spiel. Kandidatensuche über ein Prüfskript, nicht über Erinnerung.

### Drei Prüfskripte ohne Selbsttest · —

`.github/scripts/audit/scan.py`, `mitscan.py` und `scan2.py` haben keinen Selbsttest gegen konstruierte Defekte, `scan3.py` bis `scan7.py` schon — dort deckte er bisher vier Erkennungsfehler auf, die sonst als sauberer Baum durchgegangen wären, zuletzt zwei in `scan6.py` (AUDIT_LOG A16). **Kosten:** ein Nullbefund dieser drei ist nicht belastbar. Derzeit liefern alle drei Treffer, die Lücke hat also nichts verdeckt. **Auflösung:** vor dem zweiten Audit-Durchgang nachrüsten.

## Offene Arbeit

### Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 sind abgeschlossen (AUDIT_LOG A8, A10).

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Zweiter Durchgang** mit den Skripten aus `.github/scripts/audit/` über den bereinigten Baum, nach dem Nachrüsten der fehlenden Selbsttests.
- **Dokumentation** in `docs/rotation-flow/07-codebase-audit.md`.
