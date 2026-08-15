# TODO / Findings (persistent — siehe CLAUDE.md REGEL, Persistenz-Klausel)

Diese Datei existiert, damit offene Konzepte, Findings und Audit-Ergebnisse
eine Kontextkomprimierung überleben. Bei Sitzungsbeginn lesen. Neue Findings
während der Arbeit hier ergänzen, nicht nur im Chat/Task-Tool belassen.

## Offene Konzepte / Fixes (noch nicht umgesetzt)

### #46 — Pre-Pull-HoT auf Tank vor Wall-to-Wall-Erstcharge
Status: Idee erfasst, Konzept noch nicht entwickelt (welcher Healer, welcher
HoT, Distanz-/Timing-Bedingung, Interaktion mit bestehender Pre-Pull-Regen-
Logik z.B. WHM `UsePreRegen`). Braucht vollen Konzept→Kritik→Plan→Kritik→
Umsetzung→Audit-Zyklus.

### #47 — `ShouldAddDefenseArea()` prüft `BMRNextTankbusterIn` nicht
Status: Bug verifiziert (`StateUpdater.cs:170-197` prüft nur
`BMRNextRaidwideIn`, nicht Tankbuster — im Unterschied zu
`ShouldAddDefenseSingle()`, Zeile 199+, die beides prüft). Addle/Feint/
Reprisal-Kommentare (SMN/RDM/PCT/BLM/SAM/RPR/MNK/VPR/DRG/DRK/WAR/PLD/GNB)
begründen sich explizit mit "jede Schadensart inkl. reiner Tankbuster" —
bei reiner Tankbuster-Vorhersage ohne Raidwide wird `AutoStatus.DefenseArea`
aber nie gesetzt.
Fix-Skizze (AKTUALISIERT nach Einzel-Audit von 76a683b/c01a5e2/16d4475):
NICHT das Gate pauschal erweitern (gleicher Blast-Radius-Fehler wie beim
ersten DefenseArea-Redesign-Versuch, der revertiert wurde). Stattdessen:
Das Repo hat bereits ein etabliertes Workaround-Muster — Reprisal bei
DRK/GNB und Addle bei SMN sind bewusst in BEIDEN Methoden platziert
(DefenseAreaAbility UND DefenseSingleAbility), sodass sie über
ShouldAddDefenseSingle's reicheren Tankbuster-Trigger erreichbar bleiben,
auch wenn ShouldAddDefenseArea (die eigentliche Lücke) nichts prüft.
WAR/PLD-Reprisal war nie betroffen (lag von Anfang an nur in
DefenseSingleAbility). TATSÄCHLICH NOCH BETROFFEN (kein DefenseSingle-
Gegenstück): RDM/PCT/BLM-Addle, SAM/RPR/MNK/VPR/DRG/NIN-Feint,
BRD-Troubadour, MCH-Tactician. Fix: dasselbe Doppel-Platzierungs-Muster
auf diese verbleibenden Fälle anwenden (konsistenter mit bestehendem
Code als ein neues Opt-in-Flag). Noch nicht implementiert, noch nicht
kritisch geprüft.

### #52 — VPR Serpent's-Ire-Weave-Guard (`7c174ec`): `IsBurst` ist kein
Echtzeit-Burst-Fenster-Signal
Status: VERIFIZIERT (Kernaussage), Fix noch offen. `VPR_Reborn.cs:256`
(`!(IsBurst && SerpentsIrePvE.CanUse(out _))`) nutzt `IsBurst`, um den
BMR-Feint-Refresh-Guard auf ein "echtes Burstfenster" zu verengen — aber
`IsBurst => MergedStatus.HasFlag(AutoStatus.Burst)`
(`DutyRotation.cs:565`) wird in `StateUpdater.cs:869-872` gesetzt via
`if (!status.HasFlag(AutoStatus.Burst) && Service.Config.AutoBurst)
status |= AutoStatus.Burst;` — d.h. `IsBurst` ist bei Default-Settings
(`AutoBurst = true`, Standard) praktisch IMMER wahr, kein zeitlich
begrenztes Fenster. `73048dd` (MCH) hat genau dieses Muster selbst
entdeckt und dokumentiert (dritter Versuch, nachdem der zweite Versuch
mit `!IsBurst` als "dead code" erkannt und reverted wurde), aber nie
rückwirkend auf VPR angewendet — Grep über alle RebornRotations zeigt
`VPR_Reborn.cs:256` als einzige Stelle, die `IsBurst` auf diese Art
(Fenster-Verengung eines Weave-Guards statt reiner Burst-CD-Nutzungs-
entscheidung) verwendet. Folge: `7c174ec`s Fix ist für Default-Nutzer
ein No-op — reduziert sich zu `!SerpentsIrePvE.CanUse(out _)`, exakt der
Zustand vor dem "Fix" (`eab5506`).
NICHT verifiziert (Laufzeitverhalten, aus Code allein nicht zu klären):
ob das Ursprungsproblem aus `eab5506` überhaupt real ist — ob
`SerpentsIrePvE.CanUse` in dieser Codebasis tatsächlich lange "ready"
bleibt, oder ob `AttackAbility` es im selben Decision-Tick sofort
verbraucht (dann wäre der ganze Guard unnötig, ähnlich wie MCHs erste
zwei revertierte Versuche). KORREKTUR nach Abgleich mit `e221ce5` (MCH-Folgecommit): Schwere nach
unten korrigiert. MCHs `wildfireSlotContested`/`barrelStabilizerSlotContested`
nutzen `IsBurst` genauso (praktisch immer wahr) — dort verifiziert korrekt,
weil es exakt die echte Cast-Bedingung in `AttackAbility` spiegelt (die
selbst genauso `IsBurst`-gegated ist, "Spiegel-Prinzip"). VPRs `AttackAbility`
castet Serpent's Ire ebenfalls nur unter `if (IsBurst) {...CanUse...}` —
`7c174ec`s Guard spiegelt also strukturell exakt die echte Nutzungsbedingung,
genau wie bei MCH als korrekt etabliert. Der CODE ist damit vermutlich NICHT
defekt. Was bleibt: die Commit-Message von `7c174ec` ("scopes the guard back
to the narrow window it was meant for") ist IRREFÜHREND formuliert — sie
suggeriert echte Zeitfenster-Verengung, die es (da IsBurst kein Zeitfenster
ist) nicht gibt. Reines Dokumentations-/Selbstdiagnose-Problem, keine
funktionale Lücke. PRIORITÄT ENTSPRECHEND GESENKT (nicht mehr aktiver Bug,
nur Kommentar-Korrektur nötig).
Auch zu prüfen (separates, eigenständiges Item, nicht mit #52 vermischen):
`030129c` (RPR Gluttony/Enshroud-Guard, kein `IsBurst` verwendet) — Prämisse
"burst-exklusiv gehalten" durch `AttackAbility`-Code nicht eindeutig
gestützt (Gluttony/Enshroud dort ressourcen-/comboZustand-gegated, nicht
`IsBurst`-gegated; EnshroudPooling-Mechanik macht die Frage aber nicht
trivial). Noch offen, braucht ggf. eigenen Zyklus falls sich beim Weiter-
Audit ein echter Impact zeigt.

## Aggro-Management (großes, mehrteiliges Thema — vom Nutzer initiiert)

Kontext: WHM spammt DoT bei Wall-to-Wall-Pulls z.T. wiederholt auf dasselbe
(bereits aggro'te) Ziel. Daraus entwickelt: rollenbewusstes Aggro-Framework
für RSR insgesamt (Nicht-Tank: Aggro vermeiden wo ohne Nachteil möglich;
Tank: Aggro aktiv/schnell übernehmen, auch bei Co-Tank-Tod oder drohend
tödlichem Tankbuster).

Bausteine (Reihenfolge nach Risiko/Nutzen, jeder einzeln audit-fähig):

- **B2a — Provoke-Distanzbug**: VERIFIZIERT, NOCH NICHT GEFIXT.
  `ObjectHelper.cs:112`: `Vector3.Distance(target.Position, Player.Object.Position) > 5`
  — vergleicht Boss-Position mit der Position des Spielers, der Provoke
  casten will, verlangt >5y Abstand. Ein Tank in normaler Nahkampf-
  Positionierung liegt oft darunter → `CanProvoke` liefert `false`, obwohl
  der Boss sichtbar einen DPS/Healer angreift. Erklärt das gemeldete
  "unzuverlässige" Verhalten in Raid/Savage bei Co-Tank-Tod/Notfall.
  Nächster Schritt: Konzept für den Fix (was sollte die Distanzbedingung
  stattdessen prüfen, falls überhaupt etwas — evtl. ersatzlos streichen),
  kritische Prüfung, dann Umsetzung + Audit.

- **B2b — Notfall-Provoke bei drohend tödlichem Tankbuster**: Konzept
  skizziert (BMR-Tankbuster-Vorhersage + `GetEffectiveHpPercent` des
  Co-Tanks kombiniert, statt neuer encounter-spezifischer Vulnerability-
  Stack-Erkennung), aber noch NICHT kritisch geprüft/geplant/umgesetzt.
  Höchstes Restrisiko im ganzen Aggro-Thema (BMR-Encounter-Abdeckung +
  `GetEffectiveHpPercent` erstmals für fremdes Party-Mitglied statt für
  sich selbst). Sollte nach B2a kommen, nicht davor.

- **B2c — Verifikation Range-Pull-Fallback**: Vermutlich kein Code-Fix
  nötig — `ShieldLobPvE`/`TomahawkPvE`/`UnmendPvE`/`LightningShotPvE` laufen
  bereits unbedingt als GeneralGCD-Fallback. Noch nicht formal als
  "kein Handlungsbedarf" abgeschlossen.

- **B3 — WHM Dia Ziel-Umlenkung (`TargetType.SafeDotTarget`)**: Konzept
  fertig entwickelt, noch nicht implementiert. Ersetzt (nicht ergänzt!)
  den bereits gelieferten reaktiven Fix in Commit 716789d
  (`DiaPvE.Target.Target?.TargetObject != Player`) — sonst blockiert die
  alte Bedingung den neuen `targetOverride`-Pfad, bevor er je greift
  (bereits als Konzeptfehler erkannt, siehe Sitzungsverlauf). Mechanismus:
  neuer `TargetType`-Wert + `FindSafeDotTarget()` in `ActionTargetInfo.cs`,
  gespiegelt an `FindProvokeTarget()`/`DataCenter.ProvokeTarget`-Muster;
  aktiviert nur per `targetOverride` im WHM-DOTUpkeep-Zweig, kein anderer
  Job/Aufruf betroffen.

- **B4 — Pre-Pull-Sicherheit**: siehe #46. Noch kein Konzept.

- **B1 — generischer "wer greift Nicht-Tank an"-Helfer**: VERWORFEN als
  eigener Baustein (verfrühte Abstraktion, nur 2 gegenläufige Verwender
  bisher). Jeder Verwender bekommt sein eigenes kleines Prädikat.

## Abgeschlossene Nachprüfungen dieser Session (Referenz — nicht erneut prüfen)

- **Batch 1** (BMR-Refresh-Rollout + Weakness/Brink-Helfer): Weakness/Brink-
  Helfer PASST (saubere Zentralisierung, dokumentierte Selbstkorrektur einer
  früheren Regression). BMR-Refresh-Helfer selbst PASST in Architektur/
  Verwendung, aber siehe #47 für den gefundenen Gate-Fehler.
- **Batch 2** (BMR-Timeline-Gates: BMR*Within-Helper, ChurinDNC BMREnabled-
  Fix, BMRRaidwideMitWindow-Doku): alle drei PASSEN, inkl. Widerlegung der
  ursprünglichen 15s/10s-vs-5s/3s-Konfliktsorge.
- **Batch 3** (9 Dispatch-/Base-Call-Fixes): alle 9 bestätigt korrekt,
  inkl. repo-weitem automatisiertem Scan ohne weitere Treffer desselben
  Musters.
- **Batch 4** (Interrupt-Ordering, SMN Primal-Reihenfolge, RDM Impact-Bug,
  PhantomDefault act-Bug, AntiKnockback): 3/5 bestätigt. 2/5 (Interrupt-
  Ordering, AntiKnockback) hatten einen bisher unentdeckten Folgefehler —
  der generische Rollen-Fallback in `MyInterruptAbility`/`AntiKnockback`
  nutzte dieselbe Fähigkeit (LegSweep/Arm's Length) ungegatet erneut,
  nachdem RPR/VPRs eigenes Combo-Sicherheitsgate sie mid-combo korrekt
  abgelehnt hatte — Gate damit faktisch wirkungslos. Gefixt (Commit
  be7cf22, `HasOwnInterruptGate`/`HasOwnAntiKnockbackGate`) und auditiert
  (GO).

## Vollständige Einzel-Nachprüfung: ALLE Patches Fork vs. Upstream (`upstream/main..HEAD`)

Nutzer-Anweisung: nicht batchweise, sondern **einzeln** — jeder Commit für
sich: löst er ein reales Gameplay-/Kampf-Problem, löst er es gut/codearm,
gibt es eine bessere Alternative? Ersetzt/erweitert die 4 Batches oben, die
nur einen Teil abdeckten (viele der unten gelisteten Commits — v.a. der
MCH-Tactician-Strang, die job-spezifischen BMR-Rollout-Commits und die
DRG/NIN/SAM/DNC-Selbstheilung — wurden bisher NIE einzeln inhaltlich
geprüft, nur die zugrunde liegende Helper-Architektur als Ganzes).

Ausgeschlossen (kein Gameplay-Code, nicht einzeln zu prüfen): Marker-Bump-
Commits (6f839e9, 0b0469c, b6f4872, 4902b42, 60bb9af, 27d78f8, fb3af74,
bfd0a4c), Merge-Commits (9160d68, 81dcbc7, e615624, 0bb6311), Netto-Null-
Revert-Paare (5ae845b+37e47d0, 4358fc0+c82ea88, 6ebdb14+27abd85,
6717e5d+4e09493 — jeweils vollständig zurückgenommen, nichts Aktives übrig),
sowie die Meta-Commits dieser Session selbst (cce250b, b0f8ef7, 3f6d262,
5bd971e, feddd8e, e41e54c, a165c34, 1705eb7, 57fbe2b, d70adee, ccc4947,
d9125f1, 02b49f1, f97c8b1 — CLAUDE.md/TODO.md-Checkpoints; deren Liste
wächst mit jedem weiteren TODO.md-Commit dieser Art und muss beim
nächsten `upstream/main..HEAD`-Abgleich entsprechend erweitert werden,
sonst täuscht die reine Commit-Zahl einen wachsenden Prüf-Rückstand vor,
der real nur Session-eigene Doku-Commits sind).
Hinweis: `upstream/main` selbst bewegt sich weiter (Stand dieser Prüfung:
2 neue Commits seit letztem Fetch, PR #1350 "DRK opener + Phantom Samurai
target logic" — geprüft, keine Überschneidung mit den hier auditierten
Stellen, aber noch nicht in den Fork gemerged).

Status-Legende: `[ ]` offen, `[~]` in Arbeit, `[x]` einzeln geprüft (PASST
oder gefixt+auditiert, siehe Vermerk).

- [x] 8edd696 SMN: use Addle in defensives, detect tankbusters landing on non-tanks — einzeln geprüft, PASST (IsHostileCastingTank-Fallback-Heuristik ist vorbestehend, keine neue Schwäche; RadiantAegis-Refresh inzwischen auf gemeinsamen Helfer migriert)
- [x] 1ca682a Respect status-provide check for queued commands — einzeln geprüft, PASST (ForceEnable überschneidet sich nicht mit entferntem skipStatusProvideCheck)
- [x] c93a8bc Add generic BMR-aware mitigation refresh helper; wire into Addle/Feint — einzeln geprüft, PASST (Level-Gate konsistent über alle 9 Verwender, Combo-Safety-Gates wickeln neuen Zweig korrekt ein, NIN-Lücke korrekt geschlossen)
- [x] 75b7af0 SMN: remove dead RadiantOnCooldownSpam config option — einzeln geprüft, PASST (Grep bestätigt: keine Referenz mehr im Repo)
- [x] e87ebea SMN: opt-in movement-aware Titan priority — Batch 4 bestätigt, PASST
- [x] a1418f5 Fix interrupt ordering so per-job combo-safety gates actually apply — TEILWEISE, Folgefehler gefunden+gefixt in be7cf22 (s.u.)
- [x] be083a1 Add Weakness/Brink of Death awareness to heal thresholds — einzeln geprüft, PASST (Schwellenwert-Mathematik korrekt, nur früher nie später)
- [x] 27c7b69 Weigh shield magnitude and duration in heal-priority decisions — Ursprungsversion der Batch-1-Regression (blinde 3s-Fallback-Schwelle ohne Bedrohungsnachweis), kein aktuell lebender Zustand mehr, vollständig durch 0c076ee ersetzt
- [x] 1ed9907 Fix two compile errors in the BMR-refresh work — PASST, beide Compile-Fix-Begründungen gegen aktuellen Code verifiziert (Player-Typ, NinjaRotation sealed override)
- [x] 0f25161 RPR/VPR: let the BMR-timed Feint refresh survive on-going combo — einzeln geprüft, PASST (EnoughWeaveTime ist reine Clip-Sicherheit, bewusste Abwägung Sicherheit vs. Combo-Position, nur im neuen BMR-Zweig, alter reaktiver Zweig unverändert strenger)
- [x] 6fc9ebb PCT: fix DefenseSingleAbility falling through to the wrong base call — Batch 3 bestätigt
- [x] 15297b2 BRD/MCH: BMR-aware proactive refresh for Troubadour/Tactician — einzeln geprüft, PASST + NEUER BUG GEFUNDEN+GEFIXT: MCH `MitOverlap`-Dismantle-Guard kannte nur `Tactician_1951`, nicht die Sync-100-Variante `Tactician_2177` (derselbe Fehlertyp wie BLM-Thunder-Fix), gefixt in `3f72a6d` (s.u.)
- [x] 3f72a6d MCH: fix MitOverlap Dismantle guard missing the sync-level Tactician status ID — der eigene Fix für den o.g. Fund, auditiert (Zeile 167 enthält jetzt beide Status-IDs, konsistent mit Zeile 147)
- [x] 951d0ec Add hostile-count sustain-refresh fallback for Addle/Feint — einzeln geprüft, PASST (InCombat extern gewährleistet, >=3-Schwelle-Präzedenz bestätigt: DRK AbyssalDrain, SAM AoE-Trigger)
- [x] 6813a7c DRG/NIN/SAM/DNC: self-sustain via SecondWind/Bloodbath in HealSingleAbility — einzeln geprüft, PASST
- [x] 87646bf Raise sustain-refresh hostile-count threshold from 3 to 4 — einzeln geprüft, PASST (alle 9 konsistent auf 4, Kollateral-Sorge widerlegt)
- [x] c01a5e2 DRK/WAR/PLD/GNB: BMR-aware proactive refresh + sustain-refresh for Reprisal — einzeln geprüft, PASST
- [x] b1b187c Add emergency HP-potion threshold for a confirmed incoming tankbuster — superseded durch eab865c (bereits GO auditiert), PASST
- [x] 4a01682 WAR/DRK/PLD/GNB: BMR-aware proactive refresh for Vengeance/Rampart family — einzeln geprüft, PASST (Sync-Level-Ternary korrekt, kein HighThunder/Tactician-Fehlertyp)
- [x] 28361f2 Unify BMR-tankbuster-imminent check in ShouldAddDefenseSingle, extend to DPS — einzeln geprüft (deckt sich mit direkt gelesenem #47-Code), PASST
- [x] 14a15df BMRShouldRefreshBefore: respect Service.Config.UseBmrTimeline (default off) — Batch 2 bestätigt
- [x] 0b3afc7 Make the emergency HP-potion trigger proactive, not just reactive — superseded durch eab865c (bereits GO), PASST
- [x] 0a31836 RDM: fix dead Impact branch (tautological EnoughLevel check) — Batch 4 bestätigt
- [x] 470de85 DRK: remove unreachable ungated Shadow Wall/Shadowed Vigil checks — Batch 3 bestätigt
- [x] 0885f53 Respect status-provide check for the GCD-queued-command path too — PASST, symmetrisch zu 1ca682a verifiziert
- [x] 1f5dbb1 PCT: fix GeneralAbility falling through to the wrong base call — Batch 3 bestätigt
- [x] 76a683b DRK: add the Reprisal BMR/sustain block to DefenseSingleAbility too — PASST, Doppel-Platzierung gegen aktuellen Code verifiziert (Basis für #47-Präzisierung)
- [x] 16d4475 Scope the DPS proactive-tankbuster branch to no-live-tank scenarios — PASST, AnyLivingTankInParty()-Gate gegen aktuellen Code verifiziert
- [x] 0c076ee Fix shield-credit heal-priority regression + Weakness interaction — Batch 1 bestätigt (dokumentierte Selbstkorrektur)
- [x] 2d5e7dc SAM: let the BMR-timed Feint refresh survive Zanshin window — PASST, verifiziert konsistent mit RPR/VPR EnoughWeaveTime-Muster
- [x] 030129c RPR: don't let the BMR Feint refresh steal a Gluttony/Enshroud slot — PASST mit offener Nachprüfung (siehe #52-Anhang: Prämisse "burst-exklusiv gehalten" nicht eindeutig durch AttackAbility-Code gestützt)
- [x] eab5506 VPR: don't let the BMR Feint refresh steal a Serpent's Ire slot — durch 7c174ec ersetzt/korrigiert, siehe dort
- [x] 7c174ec VPR: scope the Serpent's Ire weave-guard to the actual burst window — Code strukturell vermutlich korrekt (spiegelt echte Nutzungsbedingung), Commit-Message irreführend → #52 angelegt, Priorität niedrig
- [x] 73048dd MCH: gate BMR Tactician refresh on real Wildfire/Barrel Stabilizer slot conflict — PASST, IsBurst-Nutzung dort korrekt (Spiegel-Prinzip verifiziert)
- [x] e221ce5 MCH: close oGCD-leniency gap in Wildfire slot-contested check — durch c1523ac korrigiert (Prämisse war falsch), siehe dort
- [x] c1523ac MCH: drop dead WildfirePvE.CanUse disjunct, correct comment — PASST, verifiziert gegen ActionCooldownInfo.cs:240-246, legitime Selbstkorrektur
- [x] c866879 Fix MoveBackAbility dispatch: gate condition must not itself call the ability — Batch 3 bestätigt
- [x] e1886c7 Fix AntiKnockback dispatch order so RPR/VPR's combo-safety gates apply — TEILWEISE, Folgefehler gefunden+gefixt in be7cf22 (s.u.)
- [x] 099e051 Fix AST DefenseSingleGCD calling base.DefenseAreaGCD instead of base.DefenseSingleGCD — Batch 3 bestätigt
- [x] e38cfe2 Fix HardboiledDefault DefenseAreaGCD calling base.HealSingleGCD — Batch 3 bestätigt
- [x] 0f24ed3 Fix PhantomDefault discarding act for the party-target Occult Ether/Potion branch — Batch 4 bestätigt
- [x] 53c8018 Fix BLM Thunder refresh guard missing the single-target HighThunder status ID — durch 9e4a2fc (AoE-Seite) ergänzt, GO
- [x] 3e3b7f7 Gate the BMR*Within helper family on UseBmrTimeline — Batch 2 bestätigt
- [x] 0af7957 ChurinDNC: gate BMR-driven Finishing Move logic on UseBmrTimeline + BMRActive — Batch 2 bestätigt
- [x] b896c6d DRK: drop !InTwoMIsBurst from the proactive DefenseSingleAbility Reprisal block — Batch 3 bestätigt
- [x] 7626f9f Document that BMRRaidwideMitWindow/BMRTankbusterMitWindow cap proactive refresh, not just the trigger — Batch 2 bestätigt
- [x] 1092b59 Fix BeirutaPCT DefenseSingleAbility calling base.DefenseAreaAbility — Batch 3 bestätigt
- [x] 700e870 Fix BRD/WHM PvP EmergencyGCD calling base.GeneralGCD instead of base.EmergencyGCD — Batch 3 bestätigt
- [x] ae7ed1a Remove redundant duplicate AntiKnockbackAbility call left over from e1886c7 — TEILWEISE, Folgefehler gefunden+gefixt in be7cf22 (s.u.)
- [x] cde050f PCT: add missing burst-defense gate to TemperaGrassa's GeneralAbility branch — GO (Teil des f154d57-Redesigns, auditiert)
- [x] f154d57 Redesign: job-scoped hostile-count trigger for AutoStatus.DefenseArea — auditiert, GO, aber siehe #47 (Tankbuster-Gate-Lücke)
- [x] eab865c HP-Potion: let a BMR-predicted tankbuster widen the emergency threshold too — auditiert, GO
- [x] 9e4a2fc BLM: add an unconditional freshness guard for the AoE Thunder refresh — auditiert, GO
- [x] 6c0e8dc Ground-targeted hostile AoE: resolve tied anchors via the same priority/TargetingType logic as target-based AoE — auditiert, GO
- [x] 716789d WHM: don't recast DoT-as-filler on a target that's already aggro'd onto the healer — auditiert GO, aber als zu schwach erkannt, siehe Aggro-Management B3 (Ersatz geplant)
- [x] be7cf22 Stop the generic role fallback from defeating RPR/VPR's own combo-safety gate on Interrupt/AntiKnockback — auditiert, GO

**STATUS: Alle 55 Commits einzeln geprüft (0 offen).** Ergebnis der
vollständigen Einzelprüfung: keine funktional falschen/schädlichen
Commits gefunden. Reale Funde dabei: #47 (echte, noch offene Lücke,
Fix-Skizze oben), #52 (Commit-Message irreführend bei sonst korrektem
Code, niedrige Priorität), MCH `Tactician_2177`-Sync-Bug (bereits
gefixt), Interrupt/AntiKnockback-Gate-Aushebelung (bereits gefixt via
be7cf22). Mehrere Selbstkorrektur-Ketten im Fork selbst beobachtet und
verifiziert (VPR Serpent's Ire, MCH Wildfire/Barrel Stabilizer, DRK
Reprisal-Doppelplatzierung) — durchgängig nachvollziehbar und korrekt
aufgelöst.

## Wichtig für zukünftige Sessions

Diese Datei existiert nur auf dem Branch, auf dem sie committet wurde.
Falls sie fehlt, obwohl an diesem Repo gearbeitet wird: das dem Nutzer
explizit melden (siehe CLAUDE.md), nicht stillschweigend neu anfangen.
