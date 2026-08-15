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
- [x] b1b187c Add emergency HP-potion threshold for a confirmed incoming tankbuster — TIEF NACHGEPRÜFT: `HpPotionItem.CanUseEmergency()` (HpPotionItem.cs:41-51) live gelesen — lässt bewusst nur das reaktive HP%-Gate weg, behält die Fehl-HP-Wächter-Bedingung (`MaxHp - CurrentHp >= MaxHp`, wobei `MaxHp` aus den eigenen Lumina-Itemdaten `_percent`/`_maxHp` berechnet wird, nicht geschätzt) — Overheal-Verschwendung bleibt ausgeschlossen, Kernbehauptung bestätigt. Ausgeschlossene Item-IDs (47102/22306/20309) im Kontext gelesen: das sind eigenständig behandelte Duty-spezifische Heilitems (Pilgrim's Traverse/Eureka) mit eigener Bedingung weiter unten — legitime, vorbestehende Abgrenzung, kein neuer Fehler. Kein Fehler.
- [x] 0b3afc7 Make the emergency HP-potion trigger proactive, not just reactive — TIEF NACHGEPRÜFT: Diff bestätigt exakt die spätere Lücke, die eab865c schließt — führt `bmrTankbusterImminent` nur als äußeres Gate um den `UseHpPotion`-Aufruf ein, übergibt es aber NICHT als Parameter in die Funktion, sodass der innere `CanUseEmergency`-Zweig weiterhin nur an `IsHostileCastingTankBusterAtMe` gebunden bleibt (reaktiv). Als eigenständiger Fund unabhängig nachvollzogen, nicht nur aus AUDIT_LOG-Text übernommen. Kein Fehler in diesem Commit selbst (korrekter Zwischenschritt), Lücke danach bewusst offen bis eab865c.
- [x] 4a01682 WAR/DRK/PLD/GNB: BMR-aware proactive refresh for Vengeance/Rampart family — TIEF NACHGEPRÜFT: Spieldatenbehauptung "alle acht Fähigkeiten (Basis+Upgrade) 15s, Rampart 20s" per Websuche geprüft — bei Shadow Wall zunächst WIDERSPRÜCHLICHE Ergebnisse (10s vs. 15s), durch gezielte Zusatzsuche aufgelöst: 15s ist korrekt, 10s war der Wert VOR Patch 5.1 (2019), seither 15s — Kalibrierungsfall für "unverifizierte Korrektur genauso prüfen wie Original" (REGEL Φ), hier: erste Fehlmeldung selbst erkannt und nicht unverifiziert übernommen. Rampart 20s separat bestätigt. Alle 4 Tank-Dateien einzeln gelesen: identisches Muster (Ternary für Status/Aktion nach `EnoughLevel`, aber einheitliche `15f`/`20f`-Konstante für beide Tier — bei tatsächlich identischer reeller Dauer beider Tiers kein Bug, anders als der oberflächlich ähnlich aussehende, aber tatsächlich unterschiedliche HighThunder/Tactician-Fall). `RampartStatus`-Array in StatusHelper.cs:433 existiert. Kein Fehler.
- [x] 28361f2 Unify BMR-tankbuster-imminent check in ShouldAddDefenseSingle, extend to DPS — TIEF NACHGEPRÜFT: Diff zeigt Extraktion der dreizeiligen Bedingung in eine gemeinsame lokale Variable (StateUpdater.cs:194-197) plus Erweiterung auf den DPS-Zweig, der sie vorher gar nicht hatte. Deckt sich mit der bereits in dieser Session vollständig gelesenen `ShouldAddDefenseSingle()` (StateUpdater.cs:199-311, alle 5 Kampfrollen gegen komplette JobRole-Enum geprüft) — dort verifiziert: `bmrTankbusterImminent` wird tatsächlich einmal berechnet und in Tank-, Healer- UND DPS-Zweig konsistent verwendet, keine Restduplikation, kein Rollen-Zweig übersehen. Kein Fehler.
- [x] 14a15df BMRShouldRefreshBefore: respect Service.Config.UseBmrTimeline (default off) — TIEF NACHGEPRÜFT: Diff fügt `!Service.Config.UseBmrTimeline` als ersten Kurzschluss-Check in `BMRShouldRefreshBefore` ein — deckt sich exakt mit dem in dieser Session bereits vollständig gelesenen aktuellen Funktionskörper (`if (!Service.Config.UseBmrTimeline || !BMRActive || predictedIn is not (> 0.6f and < float.MaxValue) || predictedIn > statusDuration) return false;`, CustomRotation_OtherInfo.cs:1276 ff., c93a8bc-Audit). Ein einziger Fix in der geteilten Helper-Funktion schließt die Lücke für ALLE Verwender gleichzeitig (Addle/Feint/Reprisal/Vengeance-Familie/Troubadour-Tactician) — genuine zentrale statt verstreute Korrektur, im Commit-Text selbst als eigener früherer Fehler benannt statt verschwiegen. Kein Fehler im jetzigen Zustand.
- [x] 0a31836 RDM: fix dead Impact branch (tautological EnoughLevel check) — TIEF NACHGEPRÜFT: `!ImpactPvE.EnoughLevel && ImpactPvE.EnoughLevel` ist eine logische Kontradiktion (immer falsch), Diff korrigiert zu `if (ImpactPvE.EnoughLevel)`. Live-Code (RDM_Reborn.cs:400) bestätigt Fix, Zeile 408 zeigt den erwarteten `!ImpactPvE.EnoughLevel`-Fallback-Zweig für den Scatter-Fall konsistent daneben. Kein Fehler.
- [x] 470de85 DRK: remove unreachable ungated Shadow Wall/Shadowed Vigil checks — TIEF NACHGEPRÜFT: Diff entfernt ein ungegatetes Zeilenpaar, das vor dem korrekt `EnoughLevel`-gegateten Paar stand und es damit für maxlevel-Charaktere permanent verdeckte (Shadow Wall statt Shadowed Vigil gecastet, Heal-Bonus verloren). Live-Code (DRK_Reborn.cs:203/208) bestätigt: nur noch das korrekt gegatete Paar vorhanden, kein ungegateter Vorgänger mehr. Konsistenz mit WAR/PLD/GNB (nur gegatetes Paar) bestätigt. Kein Fehler.
- [x] 0885f53 Respect status-provide check for the GCD-queued-command path too — TIEF NACHGEPRÜFT: symmetrisch zu 1ca682a (dort bereits bis `ActionBasicInfo.BasicCheck`/`IsStatusProvided` kausal nachvollzogen), hier derselbe Mechanismus für den GCD- statt oGCD-Pfad. Live-Code (CustomRotation_GCD.cs) bestätigt: `skipStatusProvideCheck: true` entfernt, kein Vorkommen mehr in der Datei. Kein Fehler.
- [x] 1f5dbb1 PCT: fix GeneralAbility falling through to the wrong base call — TIEF NACHGEPRÜFT: zweite Instanz derselben Copy-Paste-Fehlerklasse wie 6fc9ebb, diesmal `GeneralAbility`→`base.AttackAbility` statt `base.GeneralAbility`. Live-Code (PCT_Reborn.cs:241/257) bestätigt: Zeile 241 gehört zu `AttackAbility` (korrekt `base.AttackAbility`), Zeile 257 zu `GeneralAbility` (korrekt `base.GeneralAbility`) — Methodenzugehörigkeit einzeln nachverfolgt, nicht nur Zeilennummer aus dem Diff übernommen. Kein Fehler.
- [x] 76a683b DRK: add the Reprisal BMR/sustain block to DefenseSingleAbility too — TIEF NACHGEPRÜFT: Diff-Begründung (DefenseAreaAbility nur bei raidwide-förmigem Trigger erreichbar, reine Tankbuster-Prädiktion braucht den DefenseSingleAbility-Pfad) deckt sich mit der bereits verifizierten Dispatch-Architektur (CustomRotation_Ability.cs:285-317, ShouldAddDefenseSingle). Ursprüngliche Version hatte noch `!InTwoMIsBurst`-Gate (wie im DefenseArea-Zwilling) — dieses Gate existiert im JETZIGEN Live-Code nicht mehr (bewusst entfernt, s. b896c6d unten), kein Widerspruch, sondern dokumentierte spätere Design-Korrektur. Kein Fehler in diesem Commit.
- [x] 16d4475 Scope the DPS proactive-tankbuster branch to no-live-tank scenarios — TIEF NACHGEPRÜFT: `AnyLivingTankInParty()` (StateUpdater.cs:314-324) live gelesen, spiegelt strukturell exakt `AnyLivingHealerInParty()` (Zeile 327+) wie behauptet. Reaktiver, Cast-Ziel-verifizierter `IsHostileCastingTankBusterAtMe`-Zweig bewusst unangetastet (deckt den "Buster trifft trotz lebendem Tank die falsche Person"-Fall bereits korrekt ab, kein Gate nötig). `BMRTankbusterMitWindow`s `PvEFilter = JobFilterType.Tank`-UI-Filter korrekt entfernt (Live-Code bestätigt, Konfig gilt jetzt sichtbar für alle Rollen, passend zur erweiterten Nutzung). Kein Fehler.
- [x] 0c076ee Fix shield-credit heal-priority regression + Weakness interaction — TIEF NACHGEPRÜFT (bereits im Zuge der 27c7b69/be083a1-Prüfung vollständig gegen Live-Code verifiziert): `ShieldCreditAllowed` (StateUpdater.cs:704-709) ersetzt die blinde 3s-Schwelle durch echten Bedrohungsnachweis (BMR-Prädiktion ODER sichtbarer gefährlicher Cast), `!IsWeakened()`/`!PlayerIsWeakened()` verhindert Maskierung der Weakness-Schwellenerhöhung durch Schild-Credit — beide Fixes im aktuellen Code bestätigt vorhanden und korrekt verzahnt. Kein Fehler.
- [x] 2d5e7dc SAM: let the BMR-timed Feint refresh survive Zanshin window — TIEF NACHGEPRÜFT: Live-Code (SAM_Reborn.cs:93/101) bestätigt exakt das RPR/VPR-Muster (0f25161) — neuer proaktiver Zweig auf `EnoughWeaveTime`, alter reaktiver `!HasZanshinReady`-Fallback unverändert erhalten, keine Vermischung. Kein Fehler.
- [x] 030129c RPR: don't let the BMR Feint refresh steal a Gluttony/Enshroud slot — TIEF NACHGEPRÜFT: Diff und Live-Code (RPR_Reborn.cs:86-93) deckungsgleich, Guard `!(GluttonyPvE.CanUse(...) || EnshroudPvE.CanUse(...))` vorhanden. Prämisse "diese Aktionen sind eng ans Burstfenster gebunden" NICHT vollständig durch `AttackAbility`-Code bestätigbar (dort ressourcen-/comboZustand-gegated, nicht zeitfenster-gegated) — echte offene Frage, nicht stillschweigend als gelöst behandelt, dokumentiert in TODO.md #52. Guard selbst funktional nicht falsch (verhindert im schlimmsten Fall nur einen potenziell unnötigen Refresh-Verzicht), daher kein Bug, aber Nachprüfungsbedarf bleibt bestehen.
- [x] eab5506 VPR: don't let the BMR Feint refresh steal a Serpent's Ire slot — TIEF NACHGEPRÜFT: durch 7c174ec ersetzt/korrigiert (s. dort), als Zwischenschritt selbst nicht fehlerhaft, nur unpräzise in der Commit-Message.
- [x] 7c174ec VPR: scope the Serpent's Ire weave-guard to the actual burst window — TIEF NACHGEPRÜFT: `IsBurst => MergedStatus.HasFlag(AutoStatus.Burst)` (DutyRotation.cs:565) bei Default-`AutoBurst=true` praktisch dauerhaft wahr, kein echtes Zeitfenster — Commit-Message ("scopes back to the narrow window") irreführend. ABER: VPRs `AttackAbility` castet Serpent's Ire selbst nur unter `if (IsBurst) {...}` (Spiegel-Prinzip, live verifiziert) — der Guard spiegelt also strukturell exakt die reale Nutzungsbedingung, ist im Gegensatz zu 030129c NICHT unbestätigt. Code vermutlich korrekt, nur Dokumentation irreführend → TODO.md #52, niedrige Priorität.
- [x] 73048dd MCH: gate BMR Tactician refresh on real Wildfire/Barrel Stabilizer slot conflict — TIEF NACHGEPRÜFT: dritter Versuch nach zwei zuvor revertierten (4358fc0, 6ebdb14) — die Reverts selbst wurden als Netto-Null-Paare korrekt aus der Prüfliste ausgeschlossen (s.o.), hier nur der lebende dritte Versuch bewertet. `IsBurst` korrekt als Nicht-Signal erkannt und verworfen (deckt sich mit der bereits an anderer Stelle verifizierten `AutoStatus.Burst`-Dauerhaft-wahr-Erkenntnis), stattdessen echte Trigger-Bedingungen aus `AttackAbility` gespiegelt.
- [x] e221ce5 MCH: close oGCD-leniency gap in Wildfire slot-contested check — TIEF NACHGEPRÜFT: Prämisse ("CooldownCheck erlaubt oGCD-Nutzung bis zu 1 GCD vor HasOneCharge") eigenständig gegen `ActionCooldownInfo.cs:240-246` geprüft — FALSCH: `if (!_action.Info.IsRealGCD) { if (AnimationLock > 0f || !HasOneCharge) return false; }` verlangt für oGCDs `HasOneCharge` unbedingt, keine Kulanzspanne. Der hinzugefügte `|| WildfirePvE.CanUse(out _)`-Zusatz war damit beweisbar totes, aber harmloses Disjunkt. Korrekt durch c1523ac zurückgenommen.
- [x] c1523ac MCH: drop dead WildfirePvE.CanUse disjunct, correct comment — TIEF NACHGEPRÜFT: eigenständig gegen `ActionCooldownInfo.cs:240-246` bestätigt (nicht nur aus dem Commit-Text übernommen). Live-Code (MCH_Reborn.cs:141/192, beide Call-Sites) bestätigt: nur noch `HasOneCharge`, kein totes Disjunkt mehr. Legitime, verifizierte Selbstkorrektur einer Fehldiagnose, keine funktionale Auswirkung. Kein Fehler.
- [x] c866879 Fix MoveBackAbility dispatch: gate condition must not itself call the ability — TIEF NACHGEPRÜFT: Live-Code (CustomRotation_Ability.cs:346-353) bestätigt korrekte Reihenfolge (Duty-Rotation zuerst, dann eigene `MoveBackAbility`), spiegelt den direkt darüber liegenden, korrekt implementierten `MoveForwardAbility`-Block exakt — kein Doppelaufruf, keine invertierte Gate-Bedingung mehr. Kein Fehler.
- [x] e1886c7 Fix AntiKnockback dispatch order so RPR/VPR's combo-safety gates apply — TIEF NACHGEPRÜFT: spiegelbildlich zu a1418f5, gleiche Umordnung für `AntiKnockbackAbility` (CustomRotation_Ability.cs:493-501: Job-Override vor Rollen-Switch), live-Code bestätigt identisches Muster mit ArmsLengthPvE statt LegSweepPvE. Intention korrekt gelöst; der in diesem Commit noch offene Folgefehler (Rollen-Fallback wiederholt ArmsLengthPvE ungegatet, wenn RPR/VPRs eigenes Gate ablehnt) ist derselbe wie bei a1418f5 und wurde zusammen mit diesem in be7cf22 gefixt (s. dortige Verifikation). Kein Fehler in der hier vorgenommenen Umordnung selbst.
- [x] 099e051 Fix AST DefenseSingleGCD calling base.DefenseAreaGCD instead of base.DefenseSingleGCD — TIEF NACHGEPRÜFT: Live-Code (AST_Reborn.cs:460/468, beide Fallback-Returns derselben Methode) bestätigt `base.DefenseSingleGCD` an beiden Stellen. Kein Fehler.
- [x] e38cfe2 Fix HardboiledDefault DefenseAreaGCD calling base.HealSingleGCD — TIEF NACHGEPRÜFT: Live-Code (HardboiledDefault.cs:68) bestätigt `base.DefenseAreaGCD`. Kein Fehler.
- [x] 0f24ed3 Fix PhantomDefault discarding act for the party-target Occult Ether/Potion branch — TIEF NACHGEPRÜFT: Ursache kausal nachvollzogen (`BaseAction.CanUse` setzt `act = this` immer, `out _` verwarf das Ergebnis, `act` blieb auf dem zuletzt zugewiesenen — falschen — Wert stehen). Live-Code (PhantomDefault.cs:297 Ether, 576 Potion) bestätigt beide Stellen korrigiert (`out act` statt `out _`), konsistent mit dem bereits korrekten `OccultChakraPvE`-Muster zwei Zeilen darunter. Kein Fehler.
- [x] 53c8018 Fix BLM Thunder refresh guard missing the single-target HighThunder status ID — TIEF NACHGEPRÜFT: Live-Code (BLM_Default.cs:660/666/680) bestätigt `StatusID.HighThunder` inzwischen an ALLEN drei Stellen vorhanden (nicht nur der einen im Diff — dritte Stelle stammt aus dem AoE-Begleitfix 9e4a2fc), neben dem bereits vorhandenen `HighThunder_3872`. Kein Fehler.
- [x] 3e3b7f7 Gate the BMR*Within helper family on UseBmrTimeline — TIEF NACHGEPRÜFT: Live-Code (CustomRotation_OtherInfo.cs:1238-1260) bestätigt `Service.Config.UseBmrTimeline &&` an allen 4 Helfern (`BMRDowntimeWithin`/`BMRVulnWithin`/`BMRRaidwideWithin`/`BMRTankbusterWithin`). `BMRDowntimeWithin`s Verwenderzahl in MCH_Reborn.cs eigenständig gegengezählt: 7 Aufrufstellen (nicht nur die im Commit genannten 5 — spätere Commits nutzen den Helfer weiter, alle profitieren automatisch vom zentralen Fix). Kein Fehler.
- [x] 0af7957 ChurinDNC: gate BMR-driven Finishing Move logic on UseBmrTimeline + BMRActive — TIEF NACHGEPRÜFT: Live-Code (ChurinDNC.cs:775/826) bestätigt `Service.Config.UseBmrTimeline && BMRActive` ersetzt das schwächere `DataCenter.BMREnabled` (das nur "BMR-Plugin geladen" prüft, nicht Modul-Aktivität oder Opt-in) an beiden Stellen (`RemoveFinishingMove`, `CanUseActiveStandard`). Kein Fehler.
- [x] b896c6d DRK: drop !InTwoMIsBurst from the proactive DefenseSingleAbility Reprisal block — TIEF NACHGEPRÜFT: Cross-Tank-Konsistenzbehauptung eigenständig verifiziert (nicht nur aus Commit-Text übernommen) — GNB_Reborn.cs: `!HasNoMercy`-Gate nur bei DefenseArea-Reprisal (Zeilen 109/117), DefenseSingle-Reprisal (Zeilen 234/239, beide proaktiv+reaktiv) UNGEGATET; WAR/PLD haben in keiner der beiden Methoden ein Burst-Gate auf Reprisal. DRKs vorheriger `!InTwoMIsBurst`-Zusatz (aus 76a683b) war damit die Ausnahme, nicht die Konvention — Entfernen stellt Konsistenz her, statt sie zu brechen. Kein Fehler.
- [x] 7626f9f Document that BMRRaidwideMitWindow/BMRTankbusterMitWindow cap proactive refresh, not just the trigger — TIEF NACHGEPRÜFT: reiner Kommentar-Commit, kein Verhaltensunterschied. Inhaltliche Behauptung selbst geprüft (nicht nur Kommentartext übernommen): `ShouldAddDefenseArea`/`ShouldAddDefenseSingle` setzen `AutoStatus.DefenseArea`/`DefenseSingle`, BEVOR job-eigene `DefenseAreaAbility()`/`DefenseSingleAbility()` überhaupt aufgerufen werden — die dort verwendete `BMRShouldRefreshBefore`-Statusdauer (10-30s) kann als innere Schranke bei Standard-Einstellungen (5s/3s) nie relevant werden, da die äußere Fenstergrenze bereits vorher greift. Deckt sich mit bereits bestehendem TODO.md #36. Live-Code (Configs.cs:740-747) bestätigt Kommentartext vorhanden. Kein Fehler.
- [x] 1092b59 Fix BeirutaPCT DefenseSingleAbility calling base.DefenseAreaAbility — TIEF NACHGEPRÜFT: Live-Code (BeirutaPCT.cs:263/275) bestätigt: Zeile 263 `DefenseAreaAbility`→`base.DefenseAreaAbility` (korrekt, unverändert), Zeile 275 `DefenseSingleAbility`→`base.DefenseSingleAbility` (korrigiert) — Methodenzugehörigkeit einzeln zugeordnet. Kein Fehler.
- [x] 700e870 Fix BRD/WHM PvP EmergencyGCD calling base.GeneralGCD instead of base.EmergencyGCD — TIEF NACHGEPRÜFT: Commit-Begründung (Basis-`EmergencyGCD` enthält PvP-weite Notfall-Fallbacks — Guard/Recuperate/Elixir —, die bei `base.GeneralGCD` komplett übersprungen worden wären) inhaltlich nachvollzogen, nicht nur zitiert. Live-Code bestätigt `base.EmergencyGCD` in WHM_Default.PVP.cs:124 und BRD_Default.PVP.cs:161. Kein Fehler.
- [x] ae7ed1a Remove redundant duplicate AntiKnockbackAbility call left over from e1886c7 — TIEF NACHGEPRÜFT: Diff zeigt Entfernen von `return AntiKnockbackAbility(nextGCD, out act);` am Methodenende (hätte die bereits am Anfang aufgerufene Funktion bei doppeltem Fehlschlag redundant ein zweites Mal aufgerufen — deterministisch, kein Zustand mutiert, also keine funktionale Auswirkung, aber Divergenz vom dokumentierten Muster von MyInterruptAbility, das mit `return false;` endet). Live-Code (Zeile 543) bestätigt: endet jetzt korrekt mit `return false;`, kein Restaufruf mehr. Reine, korrekte Aufräumung. Kein Fehler.
- [x] cde050f PCT: add missing burst-defense gate to TemperaGrassa's GeneralAbility branch — TIEF NACHGEPRÜFT: Live-Code (PCT_Reborn.cs:246) bestätigt `(!BurstDefense || (BurstDefense && !InBurstStatus))`-Gate jetzt vorhanden, konsistent mit den anderen 3 Mitigationszweigen in derselben Datei (Zeile 113/122 u.a.). Kein Fehler.
- [x] f154d57 Redesign: job-scoped hostile-count trigger for AutoStatus.DefenseArea — TIEF NACHGEPRÜFT (größte Einzelüberprüfung dieser Nachprüfrunde, 11 Dateien betroffen): `HasHostileCountAoeMitigation` (ICustomRotation.cs:125, Default `false` in CustomRotation_BasicInfo.cs:161) live geprüft — genau die 11 im Commit genannten Jobs (SAM/RPR/MNK/VPR/DRG/GNB/DRK/RDM/PCT/BLM/SMN) überschreiben sie auf `true`, per Grep bestätigt, keiner fehlt/keiner zusätzlich. PLD/WAR-Ausschluss-Behauptung verifiziert (kein Override in beiden Dateien). StateUpdater.cs:184-192 bestätigt: Hostile-Count-Pfad nur bei `HasHostileCountAoeMitigation` durchlässig, genau der Blast-Radius-Fix ggü. dem revertierten Vorversuch (5ae845b/37e47d0). Behauptung "restliche 10 Jobs auf denselben Außerhalb-Konsumenten-Fehler geprüft, sauber" eigenständig nachvollzogen: Grep nach `AutoStatus.DefenseArea` in allen 10 Dateien ergab 0 Treffer — nur PCT hatte das Muster (weshalb `cde050f` nötig war). Kein Fehler, aber siehe TODO.md #47 (separate, bereits bekannte Tankbuster-Gate-Lücke in `ShouldAddDefenseArea`, nicht durch diesen Commit verursacht).
- [x] eab865c HP-Potion: let a BMR-predicted tankbuster widen the emergency threshold too — TIEF NACHGEPRÜFT: Live-Code bestätigt vollständige Kette — `bmrTankbusterImminent` wird jetzt als Parameter durchgereicht (CustomRotation_Ability.cs:372 → CustomRotation_Items.cs:233/250), einzige Quelle der Wahrheit bleibt `Ability()` (kein zweites Neuberechnen derselben Bedingung), Default `false` erhält alte Semantik für andere Aufrufer. Kausalkette Erkennung→proaktiver Trigger→Emergency-Schwelle jetzt für Schild(Addle/Feint via StateUpdater), Schadensdebuff und Potion konsistent gleichzeitig proaktiv, wie in der Commit-Message behauptet und hier bestätigt. Kein Fehler.
- [x] 9e4a2fc BLM: add an unconditional freshness guard for the AoE Thunder refresh — TIEF NACHGEPRÜFT: AoE-Pendant zu 53c8018. Live-Code (BLM_Default.cs:679-683) bestätigt dritten, unbedingten Pre-Check vor `ThunderIiPvE.CanUse` mit derselben kombinierten Status-ID-Liste wie die beiden ST-Guards darüber — schließt genau die Lücke, dass `ThunderIiiPvE`/`ThunderPvE`-Castbarkeit (Ziel-abhängig, nicht Freshness-abhängig) die einzigen vorherigen Wächter war. Kein Fehler.
- [x] 6c0e8dc Ground-targeted hostile AoE: resolve tied anchors via the same priority/TargetingType logic as target-based AoE — TIEF NACHGEPRÜFT (substanzielle Architekturänderung, sorgfältig geprüft): `targetOverride` war in der umschließenden `FindTargetArea`-Methode (Zeile 767) bereits als Parameter vorhanden, wie behauptet nur bis `FindTargetAreaHostile` durchgereicht (Live-Code Zeile 800/837 bestätigt Aufruf-Kette). Sicherheitsbehauptung ("kann Erfolg nicht in Fehlschlag verwandeln") bis auf Code-Ebene nachverfolgt: `tiedAnchors` wird vor dem `FindTargetByType`-Aufruf auf Nicht-Leerheit geprüft; `FindTargetByType`s internes Stop-Mark-Filtering (Zeile ~3652-3669) ersetzt die Arbeitsmenge nur, wenn das gefilterte Ergebnis selbst nicht-leer ist (`filteredHasAny`-Check) — Garantie hält, nicht nur behauptet. Kein Fehler.
- [x] 716789d WHM: don't recast DoT-as-filler on a target that's already aggro'd onto the healer — TIEF NACHGEPRÜFT: Diff bestätigt reine Skip-Lösung (`.TargetObject != Player`-Guard, kein Redirect) mit im Commit selbst ehrlich begründetem Verzicht auf Zielumlenkung (fehlende Per-Call-Hook-Infrastruktur, Blast-Radius-Abwägung explizit dokumentiert). Live-Code (WHM_Reborn.cs:502-537) bestätigt: ursprünglicher Skip-Guard weiterhin an allen 3 Stellen (Dia/AeroII/Aero) vorhanden UND von der später in dieser Session entwickelten `TargetType.SafeDotTarget`-Umlenkung (B3) korrekt ergänzt, nicht ersetzt — genau wie in TODO.md dokumentiert. Ursprünglicher Fix nicht falsch, nur unvollständig (DPS-Verlust durch reines Auslassen statt Umlenken) — Nachfolge-Arbeit bereits geleistet. Kein Fehler.
- [x] be7cf22 Stop the generic role fallback from defeating RPR/VPR's own combo-safety gate on Interrupt/AntiKnockback — TIEF NACHGEPRÜFT: Diff + Live-Code deckungsgleich (`HasOwnInterruptGate`/`HasOwnAntiKnockbackGate`, beide `virtual false` in der Basisklasse, `override true` nur in RPR/VPR). Commit-Text-Behauptung "kein systemisches Problem über alle Jobs" selbst nachgeprüft, nicht übernommen: alle 4 InterruptAbility- und alle 3 AntiKnockbackAbility-Overrides im Repo einzeln gelesen (PhantomDefault: andere Aktion, kollisionsfrei; BLU_Reborn: reiner Passthrough, keine Ablehnung zum Aushebeln; RPR/VPR: die einzigen mit Gate-dann-Fallback-Struktur auf dieselbe Aktion) — Behauptung bestätigt, nicht nur geglaubt. Kausale Gesamtkette a1418f5→e1886c7→ae7ed1a→be7cf22 schließt sich korrekt: Dispatch-Reihenfolge fixiert, Redundanz entfernt, Gate-Aushebelung geschlossen. Kein Fehler.
- [x] 0fd058d Add movement-safe pre-pull/sustain tank protection for WHM/AST/SGE (#46) — GEFIXT (statisch selbst-geprüft, kein Compile/Test), Herleitung vollständig in TODO.md #46 dokumentiert: Instant-Cast-Status je Fähigkeit per Websuche verifiziert (WHM Regen, AST Aspected Benefic, SGE Eukrasian Diagnosis), SCH bewusst ausgenommen (kein geeignetes reines instant HoT/Schild, explizit recherchiert). Neue Config-Optionen `UsePreAspectedBenefic`/`UsePreEukrasianDiagnosis`, `UsePreRegen` erweitert. Kein Fehler bekannt, noch nicht unabhängig/adversarial re-geprüft (frisch in dieser Sitzung geschrieben).
- [x] 2b6e1d8 Correct RPR Gluttony/Enshroud guard comment (resource-cycle, not burst window) — GEFIXT, Prämisse gegen `AttackAbility`-Code verifiziert (Shroud/Soul-Ressourcenzustand, nicht `IsBurst`), reine Kommentar-Korrektur, kein Verhaltensunterschied. Siehe TODO.md #52.
- [x] e9b687c DRG: add missing Stardiver weave guard to HealSingleAbility (#53) — GEFIXT, echte Inkonsistenz mit DRGs eigener datei-weiter Konvention (5 andere Ability-Dispatch-Methoden hatten den `IsLastAction(false, StardiverPvE)`-Guard bereits, `HealSingleAbility` nicht) gefunden und behoben. SAM/DNC/NIN geprüft, kein äquivalenter Fund. Siehe TODO.md #53.

**STATUS: 59 Commits in der Liste. 56 TIEF nachgeprüft (inhaltlich/kausal/
gesamtheitlich gegen aktuellen Live-Code, nicht nur den Diff, inkl. mehrerer
Websuchen zu Spielzeit-Behauptungen). 3 weitere (0fd058d/2b6e1d8/e9b687c,
alle nach Abschluss der 56er-Runde entstanden) GEFIXT/statisch selbst-
geprüft, noch nicht in derselben unabhängigen Tiefe re-geprüft — diese Liste
ist damit wieder vollständig gegenüber dem tatsächlichen Fork-Zustand
(vorher stand sie fälschlich als abgeschlossen, obwohl 3 neue Patches fehlten
— Fund und Korrektur auf Nutzerhinweis).**

Ergebnis der 56er-Runde: keine funktional falschen/schädlichen Commits
gefunden. Reale Funde dabei: MCH `Tactician_2177`-Sync-Bug (bereits gefixt),
Interrupt/AntiKnockback-Gate-Aushebelung (bereits gefixt via be7cf22).
Mehrere Selbstkorrektur-Ketten im Fork selbst beobachtet und verifiziert
(VPR Serpent's Ire, MCH Wildfire/Barrel Stabilizer, DRK Reprisal-
Doppelplatzierung) — durchgängig nachvollziehbar und korrekt aufgelöst.
TODO.md #47/#52/#53 (aus dieser Runde entstanden) sind inzwischen alle
GEFIXT/ABGESCHLOSSEN — vollständige Herleitung unten in diesem Dokument
(aus TODO.md verschoben, da TODO.md nur offene Arbeit führt).

## Feature-Arbeit & Aggro-Management (abgeschlossen, aus TODO.md verschoben)

TODO.md führt laut eigener Definition (Kopf der Datei) nur offene Arbeit.
Alles Folgende war dort mit Status GEFIXT/ABGESCHLOSSEN/VERWORFEN
eingetragen geblieben — Verstoß gegen die eigene Definition der Datei,
korrigiert durch Verschieben hierher (Beleg-Archiv).

### #46 — Pre-Pull-Schutz (HoT/Schild) auf Tank vor Wall-to-Wall-Erstcharge, mit Erneuerung während des Pulls
Status: GEFIXT für WHM/AST/SGE (statisch selbst-geprüft, kein Compile/Test), SCH bewusst unverändert (Begründung unten).

Nutzer-Zielvorgabe: Vereinheitlichung, einheitlicher Komfort für alle 4
Heiler. Präzisiert (Nutzer): der Kern ist Anbringen des HoT/Schilds
WÄHREND DES LAUFENS (Pre-Pull-Anlauf oder Bewegung im Pull), OHNE
Swiftcast zu verbrauchen — nur wenn eine echte Instant-Cast-Möglichkeit
für die jeweilige Fähigkeit bereits besteht. Kombi-oGCDs, die gleichzeitig
schilden UND heilen, sind bewusst ausgeklammert — die laufen bereits über
die bestehende reaktive Schwellenwert-Heilrota (z.B. SCH Excogitation),
keine Dopplung nötig.

Instant-Cast-Status je Fähigkeit per Websuche verifiziert:
- WHM Regen: instant (kein Cast-Zeit-Anteil). — Quelle: ffxiv.consolegameswiki.com/wiki/Regen
- AST Aspected Benefic: instant. — Quelle: ffxiv.consolegameswiki.com/wiki/Aspected_Benefic
- SGE Eukrasian Diagnosis (via Eukrasia-Stance): instant (beide GCDs der Sequenz ohne Cast-Zeit). — Quelle: ffxiv.consolegameswiki.com/wiki/Eukrasian_Diagnosis
- SCH Adloquium: 2s Cast-Zeit, NICHT instant (nur unter Seraphism/Manifestation instant, ein seltenes Burst-CD-Fenster, für diesen Zweck nicht geeignet). Excogitation ist zwar oGCD/instant, aber laut Nutzer bereits über die reaktive Heilrota abgedeckt (Kombi-oGCD-Fall) — kein neuer Code für SCH.

Nutzer-Nachfrage explizit geprüft: "SCH wäre nur sinnvoll bei einem reinen (nicht Heil+Schild-Kombi) instant HoT/Schild." Gezielt nachgesucht — Ergebnis: (a) KEIN reines instant Schild ohne Heilanteil bei SCH vorhanden (Websuche bestätigt explizit, alle Schild-Quellen — Adloquium/Succor/Consolation — sind Heil+Schild-Kombis). (b) EIN reines instant HoT existiert (`WhisperingDawnPvE`, Websuche bestätigt instant-cast, reine Regeneration ohne Sofortheil-/Schildanteil) — aber Code-Abgleich (SCH_Reborn.cs:198-238, `HealAreaAbility`) zeigt: es ist AoE, geht vom Fee-Standort aus, nicht auf den Tank gezielt richtbar, UND bereits als zentrales, stark genutztes AoE-Heiltool in die reaktive Rota eingebunden — Zweitverwendung für Pre-Pull würde entweder den Tank nicht gezielt treffen oder um dieselbe Fee-Ressource mit der bestehenden Nutzung konkurrieren. Kein geeignetes Tool gefunden, kein Versehen.
→ Ergebnis bestätigt: WHM/AST/SGE bekommen die volle Pre-Pull+Sustain-Funktion, SCH bleibt bei seinem bestehenden, unveränderten `AdloquiumDuringCountdown` (nur stationärer Pre-Pull-Cast, keine Bewegungs-Sustain). Das ist kein inkonsistentes Ergebnis, sondern spiegelt einen echten Spielmechanik-Unterschied zwischen den Jobs.

Umsetzung (alle als Low-Priority-Filler ans Ende der jeweiligen `GeneralGCD`
gesetzt, nach der DOTUpkeep-Präzedenz — feuert nur, wenn nichts
Höherprioritäres die GCD beansprucht; `.CanUse()`s eingebauter
Status-Check verhindert Doppel-Cast auf bereits aktiven Buff):
- WHM_Reborn.cs: `UsePreRegen` (bestehende Option, Beschreibung erweitert)
  gated jetzt zusätzlich einen `RegenPvE`-Sustain-Check auf `TargetType.Tank`
  am Ende von `GeneralGCD` — ergänzt den bereits vorhandenen Countdown-Cast,
  ersetzt ihn nicht.
- AST_Reborn.cs: neue Option `UsePreAspectedBenefic` (Default true) — sowohl
  neuer `CountDownAction`-Pre-Pull-Cast (3-5s-Fenster, wie WHM) als auch
  `GeneralGCD`-Sustain-Check, beide auf `AspectedBeneficPvE`/`TargetType.Tank`.
  AST hatte vorher gar keinen Pre-Pull-Tank-Schutz.
- SGE_Reborn.cs: neue Option `UsePreEukrasianDiagnosis` (Default true) —
  `CountDownAction`-Ergänzung direkt nach dem bereits bestehenden,
  ungated'ten `EukrasiaPvE`-Countdown-Press (Zeile ~132: presste Eukrasia
  schon vorher blind, nutzte es aber nie — echte, bisher unentdeckte Lücke)
  sowie `GeneralGCD`-Sustain-Check. BEWUSST selbstständig implementiert
  (Eukrasia+EukrasianDiagnosis direkt geprüft), NICHT über die bestehende
  `ChoiceEukrasia`/`_EukrasiaActionAim`-State-Machine geroutet — Analyse:
  eine Erweiterung dort hätte das Ziel-Override (`TargetType.Tank`) nicht
  sauber durch `DoEukrasianDiagnosis` durchreichen können, ohne diese
  Methode zu verändern (Blast-Radius). Race-Risiko genau geprüft: da mein
  Code als letztes in `GeneralGCD` läuft (nach `ChoiceEukrasia`, die JEDEN
  Tick zuerst läuft), kann eine gepresste Eukrasia auf einem späteren Tick
  von der bestehenden Logik für einen echten Bedarf (DefenseArea/Single,
  DoT-Refresh) "gestohlen" werden — das ist akzeptables, sogar korrektes
  Verhalten (echter Bedarf schlägt reinen Sustain-Filler), kein Bug, da
  mein Code beim nächsten freien Tick einfach erneut versucht.
- SCH_Reborn.cs: keine Änderung (s.o.).

Präzedenzfund bei der Umsetzung: AST_Reborn.cs:531 (bestehende
`HealSingleGCD`) hat bereits `IsMoving || GetHealthRatio() < AspectedBeneficHeal`
— bestätigt unabhängig, dass "Aspected Benefic bevorzugt während Bewegung"
schon ein etabliertes Muster in diesem Fork ist, kein neu erfundenes
Konzept. Neuer Sustain-Check in `GeneralGCD` überschneidet sich nicht damit
(andere Dispatch-Methode, nur erreicht wenn `AutoStatus.HealSingle` NICHT
gesetzt ist).

### #47 — `ShouldAddDefenseArea()` prüft `BMRNextTankbusterIn` nicht — GEFIXT (statisch selbst-geprüft, kein Compile/Test)
Bug: `StateUpdater.cs:170-197` prüft nur `BMRNextRaidwideIn`, nicht Tankbuster
— im Unterschied zu `ShouldAddDefenseSingle()`, die beides prüft. Bei reiner
Tankbuster-Vorhersage ohne Raidwide wird `AutoStatus.DefenseArea` nie gesetzt.
Fix: etabliertes Doppel-Platzierungs-Muster (DRK/GNB Reprisal, SMN Addle)
auf die tatsächlich betroffenen 9 Jobs angewendet — RDM/PCT/BLM-Addle,
SAM/RPR/MNK/VPR/DRG/NIN-Feint: derselbe proaktive BMR-Refresh-Block wurde
zusätzlich in `DefenseSingleAbility` eingefügt (neu angelegt wo nötig),
sodass er über `ShouldAddDefenseSingle`s reicheren Tankbuster-Trigger
erreichbar bleibt. Genuine Sicherheits-/Combo-Gates (EnoughWeaveTime,
Gluttony/Enshroud- bzw. Serpent's-Ire-Slot-Guards, DRG StardiverPvE-Guard,
NIN Mudra-Check) wurden mitgenommen; reine Präferenz-Gates (BurstDefense
bei PCT wurde dagegen bewusst mitgenommen, da es PCTs eigene etablierte
Konvention in DefenseSingle ist — Unterscheidung im Einzelfall geprüft,
nicht pauschal übernommen/weggelassen).
KORREKTUR DER KORREKTUR: Die erste Zwischen-Korrektur (BRD/MCH aus #47
ausgeschlossen, mit der Begründung "reine Raidwide-Werkzeuge, wirken nur
gegen Magieschaden") war SELBST falsch, auf zwei Ebenen. (1) `PredictedDamageType`
(Grundlage von `BMRRaidwideIn`/`BMRTankbusterIn`) verifiziert in
`BossModEnums.cs`: reine Trefferform-Klassifikation (None/Tankbuster/
Raidwide/Shared — wen trifft es), keine Schadensart-Unterscheidung.
(2) Per Websuche verifiziert (mehrere Quellen konsistent): Troubadour und
Tactician reduzieren tatsächlich JEGLICHEN Schaden, nicht nur Magieschaden
— exakt wie Reprisal/Addle/Feint. Beide Prämissen der Ausschluss-Begründung
waren unverifiziert/falsch. Korrektur zurückgenommen: BRD-Troubadour und
MCH-Tactician bekommen dieselbe Doppel-Platzierung wie die anderen 9 Jobs
— proaktiver Block zusätzlich in `DefenseSingleAbility` (neu angelegt),
mit `BMRTankbusterIn` (statt `BMRDamageIn`, passend zur bereits
bestehenden job-eigenen Konvention der raidwide-spezifischen statt
generischen BMR-Signale). MCHs Wildfire/Barrel-Stabilizer-Slot-Guards und
MultiTact-Bedingung mitgenommen (echte Sicherheits-/Nutzungs-Bedingungen,
keine Präferenz-Gates). #47 damit für alle ursprünglich identifizierten
11 Jobs vollständig umgesetzt.
WAR/PLD-Reprisal war nie betroffen (lag von Anfang an nur in
DefenseSingleAbility). Nur statisch verifiziert (kein Build/Test möglich,
kein `dotnet` in dieser Sandbox).

### #52 — VPR/RPR Weave-Guard-Kommentare: Burst-Fenster-Framing geprüft — ERLEDIGT
Status: ABGESCHLOSSEN. Beide Teilfragen einzeln aufgelöst.

VPR (`7c174ec`, `VPR_Reborn.cs:248-254`): Code-Kommentar bereits korrekt
und ehrlich formuliert (verifiziert, aktueller Live-Code gelesen) —
erklärt akkurat das Spiegel-Prinzip (Serpent's Ire in `AttackAbility`
selbst `IsBurst`-gegated, sitzt sonst die meiste Kampfzeit ungenutzt
bereit, `CanUse` allein würde Feint für die ganze Wartezeit blockieren).
Nur die GIT-COMMIT-MESSAGE von `7c174ec` selbst ("scopes the guard back
to the narrow window it was meant for") war irreführend — das ist
historischer Text, wird nicht rückwirkend umgeschrieben (kein Force-Push/
History-Rewrite ohne expliziten Nutzerauftrag). Keine Code-Änderung nötig.

RPR (`030129c`, `RPR_Reborn.cs`): Prämisse jetzt geprüft — `AttackAbility`
gated Gluttony/Enshroud tatsächlich NICHT über `IsBurst`, sondern über
RPRs eigenen Shroud-/Soul-Ressourcenzustand (`EnshroudPooling`,
`HasIdealHost`, `Soul == 100` etc., Zeilen 158-207) — die ursprüngliche
Kommentar-Formulierung "tightly time-boxed to their own burst window" war
damit ungenau (kein echtes Zeitfenster wie bei MCH Wildfire, sondern ein
Ressourcen-Zyklus-Slot). Code selbst NICHT defekt (Verhalten bleibt
sinnvoll: seltene, wertvolle Ressourcen-Slots verdienen denselben Schutz
vor Feint-Verdrängung wie ein Zeitfenster), nur die Begründung im Kommentar
war unpräzise — korrigiert (`RPR_Reborn.cs:83-86`, jetzt: Ressourcen-
Zyklus statt Burst-Fenster, gegen `AttackAbility` verifiziert).

### #53 — DRG/NIN/SAM/DNC SecondWind/Bloodbath in HealSingleAbility ohne
Weave-Slot-Gate (`6813a7c`), im Gegensatz zu RPR/VPR
Status: GEFIXT für DRG (statisch selbst-geprüft, kein Compile/Test), SAM/DNC/NIN geprüft und geschlossen (kein Fund).

Einzeln je Job auf ein KONKRETES, im jeweiligen File bereits etabliertes
Weave-Schutz-Muster geprüft (nicht nur spekulativ "könnte kollidieren"):

- **DRG — echter Fund, gefixt.** `DRG_Reborn.cs` hat ein datei-weites
  Muster: `MoveForwardAbility`, `MoveBackAbility`, `DefenseAreaAbility`,
  `DefenseSingleAbility` und `AttackAbility` beginnen JEWEILS mit
  `if (IsLastAction(false, StardiverPvE)) { return base.X(...); }` —
  eigene Logik wird direkt nach Stardiver (Sprungangriff mit Landeanimation)
  übersprungen, vermutlich um die Landung nicht zu clippen. Genau EINE
  Ability-Dispatch-Methode hatte diesen Guard NICHT: `HealSingleAbility`
  (SecondWind/Bloodbath) — eine echte, konkret belegte Inkonsistenz mit der
  eigenen Konvention der Datei, kein spekulatives "könnte sein". Gefixt:
  denselben Guard ergänzt (`DRG_Reborn.cs`).
- **SAM/DNC — geprüft, kein äquivalentes Muster gefunden.** Repo-Grep nach
  `IsLastAction`/vergleichbaren Cross-Methoden-Weave-Guards in beiden Dateien
  ergab keine Treffer — es gibt keine etablierte, im Code bereits verankerte
  Konvention, gegen die SecondWind/Bloodbath dort inkonsistent wären. Die
  ursprüngliche Sorge (SAMs Ogi-Namikiri-Fenster, DNCs Steps) bleibt
  theoretisch denkbar, aber ohne konkreten Code-Beleg — anders als bei DRG
  kein Fund, der einen Fix rechtfertigt. Ohne neuen Beleg geschlossen.
- **NIN — bereits mit Mudra-Guard versehen** (andere Zielrichtung als
  Weave-Slot-Schutz, aber verhindert bereits die naheliegendste Kollision:
  Cast während einer Ninjutsu-Sequenz). Kein weiterer Bedarf erkannt.

## Aggro-Management (großes, mehrteiliges Thema — vom Nutzer initiiert)

Kontext: WHM spammt DoT bei Wall-to-Wall-Pulls z.T. wiederholt auf dasselbe
(bereits aggro'te) Ziel. Daraus entwickelt: rollenbewusstes Aggro-Framework
für RSR insgesamt (Nicht-Tank: Aggro vermeiden wo ohne Nachteil möglich;
Tank: Aggro aktiv/schnell übernehmen, auch bei Co-Tank-Tod oder drohend
tödlichem Tankbuster).

Bausteine (Reihenfolge nach Risiko/Nutzen, jeder einzeln audit-fähig):

- **B2a — Provoke-Distanzbug**: GEFIXT (statisch selbst-geprüft, kein Compile/Test).
  `ObjectHelper.cs:113`: war `Vector3.Distance(target.Position, Player.Object.Position) > 5`
  — verlangte >5y Abstand zwischen Boss und dem provokierenden Tank, blockierte
  damit den häufigsten Fall (Tank bereits in Nahkampfreichweite, verliert Aggro
  an DPS/Healer). Konzept+Adversarial-Check (s. AUDIT_LOG.md für Details):
  downstream existiert bereits eine echte Reichweitenprüfung über das
  Action-Targeting-System (`FindProvokeTarget()`), `ShouldAddProvoke()` hat
  keine weitere Bremse gegen zu häufiges Auslösen außerhalb Allianz-Content
  — die Distanzbedingung war die einzige Sperre für den wichtigsten Fall.
  Geprüfte Alternativen: ersatzlos streichen (verworfen, entfernt evtl.
  beabsichtigten Pull-Start-Rauschfilter) vs. Vorzeichen umdrehen (gewählt
  — bewahrt mögliche Schutzfunktion, genauso codearm, risikoärmer).
  Fix: `>` → `<` (ein Zeichen), Klärungskommentar ergänzt. Upstream-Sync-
  Check vor Arbeitsbeginn: Bug existiert identisch in `upstream/main`,
  kein Fork-eigener Fehler, kein Doppelarbeit-Risiko. Nur statisch
  verifiziert (kein `dotnet` in dieser Sandbox, kein Build/Test möglich).

- **B2b — Notfall-Provoke bei kritisch verwundetem Co-Tank**: GEFIXT
  (statisch selbst-geprüft, kein Compile/Test — `ObjectHelper.cs`, `CanProvoke`).
  Konzept mehrfach überarbeitet, siehe Sitzungsverlauf für die volle
  Herleitung: ursprünglich BMR-Tankbuster-Vorhersage-basiert gedacht
  (Buster VOR dem Einschlag umlenken), aber verifiziert (Websuche +
  Nutzer-Erfahrung), dass ein bereits angekündigter Tankbuster nicht mehr
  umlenkbar ist — nur nachfolgender Schaden (regulär oder weitere
  Einschläge bei Mehrfach-Einschlag-Bustern wie Unreal Shinryu/Arkh Monh)
  ist noch beeinflussbar. Design daher auf rein REAKTIV umgestellt: Ziel
  ist ein Tank (Co-Tank), lebt, wird gerade noch vom Boss anvisiert, hat
  `GetEffectiveHpPercent() <= 25` (Schätzwert, nicht spielgetestet).
  Explizit dokumentierte Grenze: KEIN Schutz gegen One-Shot-Kaskaden aus
  voller/hoher HP (Nutzer-Beispiel: 2 Tanks + er selbst als SMN nacheinander
  von Mehrfach-Einschlägen getötet) — BMR liefert keine Schadenshöhen-
  Vorhersage, nur Timing/Trefferform, daher lässt sich "wird dieser
  Treffer tödlich sein" nicht vorab erkennen. Bewusst eng begrenzt
  umgesetzt (Nutzerentscheidung), nicht das volle ursprüngliche Konzept.
  Präzisierung durch Nutzer-Beispiel: Bei dieser Mechanik-Klasse ist nur
  der ERSTE Einschlag angekündigt (Cast/Marker, von BMR vorhersagbar) —
  Folge-Einschläge laufen automatisch als Teil derselben Funktion ohne
  eigene Ankündigung, treffen wer gerade nach Aggro-Reihenfolge dran ist.
  KORREKTUR nach Nutzer-Angabe: Es gibt tatsächlich ein Zeitfenster
  zwischen den Folge-Einschlägen, ca. 1 Sekunde pro Einschlag — meine
  vorherige Einschätzung "vermutlich gar kein Fenster" war zu pessimistisch,
  zurückgenommen. 1s ist knapp (Provoke hat kein Cast, aber Animation-Lock
  + RSRs Entscheidungsschleife + Netzwerklatenz müssen alle darunter
  passen), aber technisch ein reales, nutzbares Fenster, kein Nullfenster.
  Ob es in der Praxis zuverlässig genug reicht, bleibt ohne Spieltest
  unverifiziert — aber die Grenze ist "knapp/riskant", nicht "nicht
  vorhanden". Der Fix bleibt also auch für diese Mechanik-Klasse potentiell
  wirksam, nur mit engerer Erfolgsspanne als beim normalen Folgeschaden-Fall. Die vom Nutzer selbst
  genannten Standard-Lösungen für diese Mechanik-Klasse (alles auf einen
  Tank mit Invuln/hoher Mitigation/Zwischenheals stapeln, ODER kontrolliert
  nach Aggro-Reihenfolge verteilen) sind beide PROAKTIV — bestätigt, dass
  ein Mitigation-Stacking-Konzept (bei der Auswahl als Option 3 nicht
  gewählt) für genau diese Mechanik-Klasse der eigentlich wirksame Ansatz
  wäre, nicht Aggro-Shuffling. Als möglicher Folge-Punkt offen, nicht
  gestartet.
  Kein neues TargetType/DataCenter-Feld — Erweiterung des bestehenden
  `CanProvoke`/`ProvokeTarget`-Mechanismus (disjunkte Bedingung zu B2a,
  kein Distanz-Gate, da jeder verfügbare Tank reagieren soll). Nur
  statisch verifiziert.

- **B2c — Verifikation Range-Pull-Fallback**: ABGESCHLOSSEN, kein Code-Fix
  nötig. Verifiziert in allen 4 Tank-Rotationen: `TomahawkPvE` (WAR_Reborn.cs:416),
  `LightningShotPvE` (GNB_Reborn.cs:544), `ShieldLobPvE` (PLD_Reborn.cs:479),
  `UnmendPvE` (DRK_Reborn.cs:430) sitzen jeweils am Ende von `GeneralGCD`,
  direkt vor `base.GeneralGCD`, nur durch die eigene `.CanUse()` gegated —
  kein externes Blockier-Gate, kein Notfall-Szenario betroffen.

- **B3 — WHM Dia Ziel-Umlenkung (`TargetType.SafeDotTarget`)**: GEFIXT
  (statisch selbst-geprüft, kein Compile/Test). Umsetzung weicht von der ursprünglichen Skizze in zwei
  Punkten ab, aus gutem Grund: (1) ERGÄNZT den reaktiven Fix aus 716789d,
  ersetzt ihn nicht — die alte Bedingung (`DiaPvE.Target.Target?.TargetObject
  != Player`) bleibt als primärer Versuch stehen, der neue
  `targetOverride: TargetType.SafeDotTarget`-Zweig greift nur als Fallback,
  wenn das Standardziel unsicher ist UND `DOTUpkeep` aktiv ist. (2) KEIN
  `DataCenter.ProvokeTarget`-Muster (kein neues DataCenter-Feld, kein
  TargetUpdater-Eintrag) — `FindSafeDotTarget()` durchsucht stattdessen
  direkt die bereits gefilterte lokale `battleChara`-Kandidatenliste
  (schlankeres, ebenfalls etabliertes Muster, näher an `RandomMeleeTarget`
  als an `FindProvokeTarget`/`FindDispelTarget`, da hier keine
  Voll-Hostile-Liste mit Sonderlogik pro Frame nötig ist). Neuer
  `TargetType.SafeDotTarget`-Enum-Wert, `FindSafeDotTarget()` in beiden
  Switches in `ActionTargetInfo.cs`, Einhängung nur in WHMs DOTUpkeep-
  Zweig (Dia/AeroII/Aero je einzeln), kein anderer Job betroffen. Nur
  statisch verifiziert.

- **B4 — Pre-Pull-Sicherheit**: siehe #46 — GEFIXT für WHM/AST/SGE, SCH begründet unverändert.

- **B1 — generischer "wer greift Nicht-Tank an"-Helfer**: VERWORFEN als
  eigener Baustein (verfrühte Abstraktion, nur 2 gegenläufige Verwender
  bisher). Jeder Verwender bekommt sein eigenes kleines Prädikat.
