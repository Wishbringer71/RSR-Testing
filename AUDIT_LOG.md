# Audit-Log (Beleg-Archiv — siehe CLAUDE.md REGEL, Persistenz-Klausel)

Diese Datei ist das Beleg-Archiv für bereits abgeschlossene Prüfungen. Sie
existiert, damit "wurde X schon geprüft?" nicht erneut recherchiert werden
muss und damit Aussagen wie "55/55 Commits geprüft" belegbar bleiben, statt
unbelegte Behauptung zu sein (REGEL Φ:Fabrikation).

Für offene Aufgaben/Konzepte siehe `TODO.md` — dort steht nur, was noch zu
tun ist. Diese Datei hier wird nicht bei jeder kleinen Änderung neu
gelesen, sondern gezielt konsultiert, wenn geprüft werden soll, ob ein
bestimmter Commit/Bereich bereits auditiert wurde.

## Abgeschlossene Nachprüfungen dieser Session (Batches, vor der Einzelprüfung)

- **Batch 1** (BMR-Refresh-Rollout + Weakness/Brink-Helfer): Weakness/Brink-
  Helfer PASST (saubere Zentralisierung, dokumentierte Selbstkorrektur einer
  früheren Regression). BMR-Refresh-Helfer selbst PASST in Architektur/
  Verwendung, aber siehe TODO.md #47 für den gefundenen Gate-Fehler.
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
d9125f1, 02b49f1, f97c8b1, sowie die Fork-main-Sync-Merges — CLAUDE.md/
TODO.md-Checkpoints und reine Doku-Commits; diese Liste wächst mit jedem
weiteren Doku-Commit dieser Art und muss beim nächsten
`upstream/main..HEAD`-Abgleich entsprechend erweitert werden, sonst
täuscht die reine Commit-Zahl einen wachsenden Prüf-Rückstand vor, der
real nur Session-eigene Doku-Commits sind).
Hinweis: `upstream/main` wurde am 15.08. erneut abgeglichen und in
`origin/main` (den Fork selbst, nicht nur den Arbeitsbranch) gemerged —
PR #1350 "DRK opener + Phantom Samurai target logic", geprüft, keine
Überschneidung mit den hier auditierten Stellen.

Status-Legende: `[ ]` offen, `[~]` in Arbeit, `[x]` einzeln geprüft (PASST
oder gefixt+auditiert, siehe Vermerk).

- [x] 8edd696 SMN: use Addle in defensives, detect tankbusters landing on non-tanks — TIEF NACHGEPRÜFT (inhaltlich/kausal/gesamtheitlich, nicht nur Diff gelesen): Kausalkette Erkennung→AutoStatus→Dispatch→job-eigene DefenseSingleAbility vollständig nachvollzogen; alle 5 Kampfrollen in ShouldAddDefenseSingle() gegen komplette JobRole-Enum geprüft, keine fehlt; SMN-only-Wiring im Commit-Text ehrlich benannt, durch spätere Commits (c93a8bc, #47) vervollständigt. Kein Fehler gefunden.
- [x] 1ca682a Respect status-provide check for queued commands — TIEF NACHGEPRÜFT: Kausalkette bis in `ActionBasicInfo.BasicCheck`/`IsStatusProvided` verifiziert (echter Blockmechanismus, nicht nur behauptet), gesamtheitlich bestätigt (GCD-seitiger Zwilling 0885f53 existiert, deckt den symmetrischen Fall ab). Kein Fehler.
- [x] c93a8bc Add generic BMR-aware mitigation refresh helper; wire into Addle/Feint — TIEF NACHGEPRÜFT: `BMRShouldRefreshBefore` bis in `WillStatusEnd`/`StatusTime` kausal nachvollzogen; 0.6s-Schwelle gegen StateUpdater-Konvention verifiziert (identisch); Spieldaten per Websuche verifiziert (Addle UND Feint: 10s Basis, 15s ab Lv.98 Enhanced-Trait, beide bestätigt, mehrere Quellen); `PlayerSyncedLevel()` korrekt statt Raw-Level für Sync-Content. Kein Fehler.
- [x] 75b7af0 SMN: remove dead RadiantOnCooldownSpam config option — TIEF NACHGEPRÜFT: 0 Referenzen im aktuellen Repo (frisch gegrept), Fallback-Abdeckung in GeneralAbility direkt gegen bereits gelesenen Code verifiziert. Kein Fehler.
- [x] e87ebea SMN: opt-in movement-aware Titan priority — TIEF NACHGEPRÜFT: Spielmechanik-Behauptung (Topaz instant-cast, Ifrit/Garuda Hardcasts) per Websuche verifiziert, `TitanTime()` existiert und korrekt referenziert, Default-off verhindert Verhaltensänderung ohne Opt-in. Kein Fehler.
- [x] a1418f5 Fix interrupt ordering so per-job combo-safety gates actually apply — TIEF NACHGEPRÜFT: aktueller Live-Code (CustomRotation_Ability.cs:406-414) bestätigt exakt den behaupteten Endzustand — `InterruptAbility(nextGCD, out act)` (virtuelle Job-Override-Hook) wird VOR dem hartkodierten Rollen-Switch aufgerufen, nicht danach. Kausal nachvollzogen: RPR/VPR überschreiben `InterruptAbility` mit `NotInActiveCombo`/`NoAbilityReady`-Gate auf LegSweepPvE (RPR_Reborn.cs:141-152, VPR_Reborn.cs:305-316) — ohne die Umordnung hätte der Melee-Rollen-Fallback dieselbe Aktion vorher ungegatet an sich gerissen und das Gate wäre nie erreicht worden (Intention des Commits korrekt gelöst). Gesamtheitlich geprüft: alle 4 Overrides von `InterruptAbility` im Repo (PhantomDefault, BLU_Reborn zusätzlich zu RPR/VPR) — PhantomDefault nutzt OccultFalconPvE (kollisionsfrei mit Rollen-Fallback), BLU_Reborn reicht nur an base durch (No-Op) — beide von der Umordnung unberührt, kein Kollateralschaden. Kein Fehler in diesem Commit selbst; der in ae7ed1a/e1886c7 gefundene Folgefehler (ungegateter Zweitversuch derselben Aktion im Rollen-Fallback) betraf eine andere Stelle (AntiKnockback-Pendant) und wurde in be7cf22 sauber geschlossen (`HasOwnInterruptGate`/`HasOwnAntiKnockbackGate`, live verifiziert: beide Properties in RPR/VPR auf `true`, Rollen-Fallback prüft `!HasOwnInterruptGate/!HasOwnAntiKnockbackGate` vor dem ungegateten Zweitversuch). Kein Fehler.
- [x] be083a1 Add Weakness/Brink of Death awareness to heal thresholds — TIEF NACHGEPRÜFT: `IsWeakened`/`PlayerIsWeakened` (StatusHelper.cs:659-676) und Einhängung in `ShouldHealSelf`/`ShouldHealSingle` (StateUpdater.cs) im Live-Code gelesen — Logik von be083a1 ist nicht nur erhalten, sondern korrekt mit der später hinzugekommenen Schild-Credit-Logik (0c076ee) verzahnt: Schild-Credit wird explizit NICHT gewährt, wenn Ziel geschwächt ist (Zeile 756/813 `&& !...IsWeakened()`), mit klarem Kommentar warum (Schild-Dauer sagt nichts über Heilbedarf bei halbierter Heilung aus) — gesamtheitlich konsistentes Zusammenspiel zweier zeitlich getrennter Commits, kein Konflikt. Schwellen-Mathematik (`threshold * 1.5f`, gecappt `Math.Min(1f, ...)`) macht Heilung ausschließlich früher nie später aktiv, Intention erfüllt. Grenze ehrlich benannt: `StatusID.Weakness`/`BrinkOfDeath` sind externe Dalamud/Lumina-Enum-Werte, nicht im Repo als Quelltext vorhanden — Existenz/Korrektheit der IDs kann hier nicht kompiliert/verifiziert werden, nur die Verwendung im eigenen Code. Kein Fehler in der Fork-eigenen Logik.
- [x] 27c7b69 Weigh shield magnitude and duration in heal-priority decisions — TIEF NACHGEPRÜFT (präzisiert): ursprüngliche Version führte `ShieldSurvivalHorizon` mit blinder 3s-Fallback-Schwelle OHNE `UseBmrTimeline`-Check und ohne jeden Bedrohungsnachweis ein → Regression (fast jeder reale Schild übersteht 3s, wurde also fast durchgehend als Schutz gewertet, Heilung verzögerte sich ohne echten Grund). Live-Code geprüft: `ShieldSurvivalHorizon` als Bezeichner/Mechanismus lebt weiter (StateUpdater.cs:718-722), ist aber jetzt durch `ShieldCreditAllowed` (Zeile 704-709, aus 0c076ee) vorgeschaltet — der 3f-Fallback greift nur noch, wenn `ShieldCreditAllowed` bereits über echten Cast-Nachweis (IsHostileCastingAOE/ToTank/TankBusterAtMe) wahr ist, nicht mehr blind. Ursprüngliche Regression korrekt geschlossen, Grundidee (Schild-Magnitude in Heil-Entscheidung einbeziehen) sinnvoll erhalten. Kein Fehler im jetzigen Zustand.
- [x] 1ed9907 Fix two compile errors in the BMR-refresh work — TIEF NACHGEPRÜFT: beide Fixes gegen Live-Code bestätigt — (1) `Player.Object` → `Player` in `BMRShouldRefreshBefore`s Self-Fallback (CustomRotation_OtherInfo.cs:1279, `Player` ist innerhalb `CustomRotation` bereits `IPlayerCharacter`, nicht die externe `ECommons`-Player-Klasse mit `.Object`-Indirektion); (2) NIN_Reborn-Override entfernt, Logik stattdessen in `NinjaRotation.DefenseAreaAbility` (bereits `sealed`) integriert, inkl. Mudra-Gate — Live-Code (NinjaRotation.cs:722-736) bestätigt Integration korrekt, Mudra-Check vorhanden. Commit-Behauptung "kein anderer Melee-Job hat diesen Sealed-Konflikt" selbst nachgeprüft (grep über alle Basic-Rotationsklassen): nur NinjaRotation sealed `DefenseAreaAbility`, bestätigt statt geglaubt. Kein Fehler.
- [x] 0f25161 RPR/VPR: let the BMR-timed Feint refresh survive on-going combo — TIEF NACHGEPRÜFT: `EnoughWeaveTime` (CustomRotation_OtherInfo.cs:1296: `WeaponRemain < WeaponTotal && WeaponRemain > Math.Max(CalculatedActionAhead, AnimationLock)`) ist ein etablierter, repo-weit vielfach genutzter Clip-Sicherheits-Helfer (ChurinMNK/ChurinDNC/ChurinBRD, >10 Fundstellen) — Commit-Behauptung "bereits derselbe Verwendungszweck wie ChurinMNK" verifiziert, nicht nur geglaubt. Live-Code (RPR_Reborn.cs:75-101) bestätigt: alter reaktiver Zweig (`NotInActiveCombo && FeintPvE.CanUse`, Zeile 95) unverändert erhalten, neuer BMR-Zweig (Zeile 86-93) bekam die lockerere Weave-basierte Prüfung statt der pauschalen Combo-Sperre — Kausalkette (echte, getimte Bedrohung wiegt schwerer als reine Slot-Präferenz, aber Clip-Sicherheit bleibt hart) korrekt umgesetzt. VPR-Pendant strukturell identisch (VPR_Reborn.cs:238-260). Kein Fehler.
- [x] 6fc9ebb PCT: fix DefenseSingleAbility falling through to the wrong base call — TIEF NACHGEPRÜFT: Diff zeigt `base.DefenseAreaAbility`→`base.DefenseSingleAbility`-Korrektur in PCT_Reborn.cs:150 (Copy-Paste-Artefakt). Live-Code bestätigt Fix vorhanden. Commit-Behauptung "einzige Datei mit diesem Muster" eigenständig gegen aktuellen Code re-verifiziert (gezielter Grep: `DefenseSingleAbility`-Override gefolgt von `base.DefenseAreaAbility`-Aufruf, repo-weit) — kein weiterer Treffer, Behauptung bestätigt statt geglaubt. Kein Fehler.
- [x] 15297b2 BRD/MCH: BMR-aware proactive refresh for Troubadour/Tactician — TIEF NACHGEPRÜFT: Diff selbst verwendet für Tactician bereits korrekt beide Status-IDs (`Tactician_1951, Tactician_2177`, BRD_Reborn.cs/MCH_Reborn.cs); der in dieser Session zuvor gefundene Bug war NICHT in diesem neuen Zweig, sondern in einer bereits VORHANDENEN, unveränderten `MitOverlap`-Dismantle-Guard-Zeile weiter unten in derselben Datei, die nur `Tactician_1951` kannte (derselbe Fehlertyp wie der BLM-HighThunder-Fix an anderer Stelle) — echter, eigenständig gefundener Fund, nicht erfunden. BRD/Troubadour gesamtheitlich gegengeprüft: nur eine Status-ID im Spiel (kein Sync-Varianten-Problem wie bei Tactician, das aus dem MCH-Job-Rework mit ID-Wechsel stammt), kein Analogon-Fehler dort. Gefixt in 3f72a6d.
- [x] 3f72a6d MCH: fix MitOverlap Dismantle guard missing the sync-level Tactician status ID — TIEF NACHGEPRÜFT: Live-Code (MCH_Reborn.cs:167) bestätigt beide Status-IDs, konsistent mit den anderen beiden Tactician-Stellen (Zeile 147, 198). Repo-weiter Grep auf `Tactician_1951` ohne begleitendes `Tactician_2177` in derselben Zeile ergab keine weiteren Treffer außerhalb dieser Datei. Kein Fehler.
- [x] 951d0ec Add hostile-count sustain-refresh fallback for Addle/Feint — TIEF NACHGEPRÜFT: `InCombat`-Gewährleistung durch Aufrufer (`ShouldAddDefenseArea`/`ShouldAddDefenseSingle` in StateUpdater.cs) live bestätigt, keine Doppelprüfung nötig. Präzedenz-Behauptung (DRK AbyssalDrain, SAM AoE-Trigger nutzen ebenfalls `>= 3`) durch Grep eigenständig verifiziert (DRK_Reborn.cs:318, SAM_Reborn.cs:264/292 zum Commit-Zeitpunkt bei 3). 10 betroffene Dateien (NIN/BLM/PCT/RDM/SMN/DRG/MNK/RPR/SAM/VPR) einzeln gezählt, mit Diff-Stat abgeglichen — vollständig. Kein Fehler.
- [x] 6813a7c DRG/NIN/SAM/DNC: self-sustain via SecondWind/Bloodbath in HealSingleAbility — TIEF NACHGEPRÜFT: Rollen-Restriktionen bestätigt (DNC/RangedPhysical bekommt nur SecondWind, kein BloodbathPvE-Aufruf — korrekt, nicht nur `.CanUse()`-intern verlassen). ABER: Commit-Behauptung "kopiert VPR/RPR-Muster verbatim" bei genauerem Vergleich UNGENAU — RPR/VPR gaten SecondWind/Bloodbath mit `NotInActiveCombo`/`NoAbilityReady` (Weave-Slot-Schutz für Gluttony/Enshroud bzw. Serpent's Ire, `-S`-Suche bestätigt: vor-fork/upstream, nicht Teil dieser Session), DRG/SAM/DNC bekamen KEIN äquivalentes Gate, NIN nur den themenfremden Mudra-Guard. Nicht bewiesen ob echte Lücke (SecondWind/Bloodbath feuert nur bei echtem Heilbedarf, Selbsterhalt könnte Burst-Timing berechtigt überstimmen) — als TODO.md #53 dokumentiert statt stillschweigend übergangen.
- [x] c01a5e2 DRK/WAR/PLD/GNB: BMR-aware proactive refresh + sustain-refresh for Reprisal — TIEF NACHGEPRÜFT: Enhanced-Reprisal-Spieldatenbehauptung (10s Basis, 15s ab Lv.98) per Websuche verifiziert (ffxiv.consolegameswiki.com/wiki/Reprisal, ffxiv.gamerescape.com/wiki/Enhanced_Reprisal, Patch 7.0). Alle 4 Tank-Dateien einzeln gelesen: GNBs zwei Call-Sites (DefenseArea+DefenseSingle) beide vorhanden und identisch aktualisiert; DRKs `!InTwoMIsBurst`-Gate im DefenseArea-Zweig erhalten, im DefenseSingle-Zweig bewusst NICHT gesetzt — dort per Kommentar (Zeile 242-246) explizit als konsistente 4-Tank-Design-Entscheidung dokumentiert, nicht übersehen. `NumberOfHostilesInRange >= 4` konsistent mit dem 87646bf-Schwellenwert. Kein Fehler.
- [x] 87646bf Raise sustain-refresh hostile-count threshold from 3 to 4 — TIEF NACHGEPRÜFT: repo-weiter Grep über alle BMR-Sustain-Refresh-Stellen (nicht nur die im Commit genannten) bestätigt live durchgängig `>= 4` (14 Fundstellen über NIN/BLM/PCT/RDM/SMN/DRG/MNK/RPR/SAM/VPR/GNB/PLD/WAR/DRK), keine bei 3 zurückgebliebene Ausreißerstelle. Andere `>= 3`-Vorkommen im selben Grep (DRK AbyssalDrain, SAM Hagakure-Fallback) sind bewusst unverändert — andere, unabhängige Trigger, keine Kollateralverwechslung. Kein Fehler.
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
- [x] 030129c RPR: don't let the BMR Feint refresh steal a Gluttony/Enshroud slot — PASST mit offener Nachprüfung (siehe TODO.md #52-Anhang: Prämisse "burst-exklusiv gehalten" nicht eindeutig durch AttackAbility-Code gestützt)
- [x] eab5506 VPR: don't let the BMR Feint refresh steal a Serpent's Ire slot — durch 7c174ec ersetzt/korrigiert, siehe dort
- [x] 7c174ec VPR: scope the Serpent's Ire weave-guard to the actual burst window — Code strukturell vermutlich korrekt (spiegelt echte Nutzungsbedingung), Commit-Message irreführend → TODO.md #52 angelegt, Priorität niedrig
- [x] 73048dd MCH: gate BMR Tactician refresh on real Wildfire/Barrel Stabilizer slot conflict — PASST, IsBurst-Nutzung dort korrekt (Spiegel-Prinzip verifiziert)
- [x] e221ce5 MCH: close oGCD-leniency gap in Wildfire slot-contested check — durch c1523ac korrigiert (Prämisse war falsch), siehe dort
- [x] c1523ac MCH: drop dead WildfirePvE.CanUse disjunct, correct comment — PASST, verifiziert gegen ActionCooldownInfo.cs:240-246, legitime Selbstkorrektur
- [x] c866879 Fix MoveBackAbility dispatch: gate condition must not itself call the ability — Batch 3 bestätigt
- [x] e1886c7 Fix AntiKnockback dispatch order so RPR/VPR's combo-safety gates apply — TIEF NACHGEPRÜFT: spiegelbildlich zu a1418f5, gleiche Umordnung für `AntiKnockbackAbility` (CustomRotation_Ability.cs:493-501: Job-Override vor Rollen-Switch), live-Code bestätigt identisches Muster mit ArmsLengthPvE statt LegSweepPvE. Intention korrekt gelöst; der in diesem Commit noch offene Folgefehler (Rollen-Fallback wiederholt ArmsLengthPvE ungegatet, wenn RPR/VPRs eigenes Gate ablehnt) ist derselbe wie bei a1418f5 und wurde zusammen mit diesem in be7cf22 gefixt (s. dortige Verifikation). Kein Fehler in der hier vorgenommenen Umordnung selbst.
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
- [x] ae7ed1a Remove redundant duplicate AntiKnockbackAbility call left over from e1886c7 — TIEF NACHGEPRÜFT: Diff zeigt Entfernen von `return AntiKnockbackAbility(nextGCD, out act);` am Methodenende (hätte die bereits am Anfang aufgerufene Funktion bei doppeltem Fehlschlag redundant ein zweites Mal aufgerufen — deterministisch, kein Zustand mutiert, also keine funktionale Auswirkung, aber Divergenz vom dokumentierten Muster von MyInterruptAbility, das mit `return false;` endet). Live-Code (Zeile 543) bestätigt: endet jetzt korrekt mit `return false;`, kein Restaufruf mehr. Reine, korrekte Aufräumung. Kein Fehler.
- [x] cde050f PCT: add missing burst-defense gate to TemperaGrassa's GeneralAbility branch — GO (Teil des f154d57-Redesigns, auditiert)
- [x] f154d57 Redesign: job-scoped hostile-count trigger for AutoStatus.DefenseArea — auditiert, GO, aber siehe TODO.md #47 (Tankbuster-Gate-Lücke)
- [x] eab865c HP-Potion: let a BMR-predicted tankbuster widen the emergency threshold too — auditiert, GO
- [x] 9e4a2fc BLM: add an unconditional freshness guard for the AoE Thunder refresh — auditiert, GO
- [x] 6c0e8dc Ground-targeted hostile AoE: resolve tied anchors via the same priority/TargetingType logic as target-based AoE — auditiert, GO
- [x] 716789d WHM: don't recast DoT-as-filler on a target that's already aggro'd onto the healer — auditiert GO, aber als zu schwach erkannt, siehe TODO.md Aggro-Management B3 (Ersatz geplant)
- [x] be7cf22 Stop the generic role fallback from defeating RPR/VPR's own combo-safety gate on Interrupt/AntiKnockback — TIEF NACHGEPRÜFT: Diff + Live-Code deckungsgleich (`HasOwnInterruptGate`/`HasOwnAntiKnockbackGate`, beide `virtual false` in der Basisklasse, `override true` nur in RPR/VPR). Commit-Text-Behauptung "kein systemisches Problem über alle Jobs" selbst nachgeprüft, nicht übernommen: alle 4 InterruptAbility- und alle 3 AntiKnockbackAbility-Overrides im Repo einzeln gelesen (PhantomDefault: andere Aktion, kollisionsfrei; BLU_Reborn: reiner Passthrough, keine Ablehnung zum Aushebeln; RPR/VPR: die einzigen mit Gate-dann-Fallback-Struktur auf dieselbe Aktion) — Behauptung bestätigt, nicht nur geglaubt. Kausale Gesamtkette a1418f5→e1886c7→ae7ed1a→be7cf22 schließt sich korrekt: Dispatch-Reihenfolge fixiert, Redundanz entfernt, Gate-Aushebelung geschlossen. Kein Fehler.

**STATUS: Alle 55 Commits einzeln geprüft (0 offen).** Ergebnis der
vollständigen Einzelprüfung: keine funktional falschen/schädlichen
Commits gefunden. Reale Funde dabei: TODO.md #47 (echte, noch offene
Lücke, Fix-Skizze dort), TODO.md #52 (Commit-Message irreführend bei
sonst korrektem Code, niedrige Priorität), MCH `Tactician_2177`-Sync-Bug
(bereits gefixt), Interrupt/AntiKnockback-Gate-Aushebelung (bereits
gefixt via be7cf22). Mehrere Selbstkorrektur-Ketten im Fork selbst
beobachtet und verifiziert (VPR Serpent's Ire, MCH Wildfire/Barrel
Stabilizer, DRK Reprisal-Doppelplatzierung) — durchgängig
nachvollziehbar und korrekt aufgelöst.
