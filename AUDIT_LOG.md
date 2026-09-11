# Audit-Log — Beleg-Archiv

Archiv abgeschlossener Prüfungen dieses Forks. Zweck: „wurde X schon geprüft?" ist hier nachlesbar, und jede Zahl („59 Commits geprüft") hat einen Beleg. Offene Arbeit steht ausschließlich in `TODO.md`; Regeln in `CLAUDE.md`.

Aufbau: **A** Vorgänge in chronologischer Reihenfolge, je Vorgang Anlass → Ergebnis → Belege; **B** Commit-Register aller Fork-Commits mit Prüfstatus; **C** widerrufene Aussagen dieses Archivs.

Statusbegriffe: **GEFIXT** (Code geändert) · **KEIN FEHLER** (geprüft, nichts zu tun) · **VERWORFEN** (Idee/Fix zurückgenommen) · **KORRIGIERT** (frühere Aussage hier widerrufen, s. Teil C). Prüftiefe: *statisch* = Code/Diff gelesen · *CI* = kompiliert und Prüfskript sauber · *Spiel* = vom Nutzer beobachtet. Ohne Zusatz gilt *statisch + CI*.

---

## A · Vorgänge

### A1 · Aggro-Management (Nutzerthema)

**Anlass:** WHM castete den DoT im Wall-to-Wall wiederholt auf ein Ziel, das bereits auf ihm hing. Daraus ein rollenbewusstes Konzept: Nicht-Tanks vermeiden Aggro, wo es nichts kostet; Tanks übernehmen sie aktiv, auch bei Co-Tank-Tod.

| Baustein | Status | Kern | Beleg |
|---|---|---|---|
| B1 generischer „wer greift Nicht-Tank an"-Helfer | VERWORFEN | verfrühte Abstraktion; jeder Verwender bekommt sein Prädikat | CLAUDE.md nennt genau diese Entscheidung als Beleg für ungeprüften „Nichtbedarf" |
| B2a Provoke-Distanzcheck `>` → `<` | **KORRIGIERT → auf Upstream zurückgesetzt** | s. C1 | — |
| B2b Notfall-Provoke auf kritisch verwundeten Co-Tank | GEFIXT | `CanProvoke`: Ziel ist Co-Tank, lebt, wird vom Boss anvisiert, Effective-HP ≤ Schwelle; ohne Distanz-Gate. Rein reaktiv: ein angekündigter Buster ist nicht mehr umlenkbar (Websuche + Nutzer), nur Folgeschaden; bei Mehrfach-Einschlägen bleibt ~1 s Fenster (Nutzer), knapp, aber real. Schwelle später auf `HealthForDyingTanks` umgestellt (A4), Invuln-Gate ergänzt (A7-5) | ObjectHelper.cs |
| B2c Range-Pull-Fallback der Tanks | KEIN FEHLER | Tomahawk/Lightning Shot/Shield Lob/Unmend sitzen am Ende von `GeneralGCD`, nur durch eigenes `CanUse` gegated | WAR 416 · GNB 544 · PLD 479 · DRK 430 |
| B3 WHM-Dia-Zielumlenkung `TargetType.SafeDotTarget` | GEFIXT → später **entfernt** | ergänzte den Skip aus 716789d um einen Fallback auf ein Ziel ohne Aggro auf dem Heiler; nach dem Revert des WHM-DoT-Blocks (5755ad5b) ohne Aufrufer, entfernt in 2df7dc4e | — |
| B4 Pre-Pull-Sicherheit | → A2 | | |

### A2 · Pre-Pull- und Sustain-HoT auf dem Tank (#46) mit zehn Nachträgen

**Anlass:** Nutzer-Ziel: HoT/Schild auf dem Tank während des Laufens (Anlauf oder Bewegung im Pull) ohne Swiftcast, einheitlich für alle Heiler.

**Faktenbasis (Websuche):** Regen, Aspected Benefic, Eukrasian Diagnosis sind instant; Adloquium hat 2 s Cast, SCH hat kein reines instant Schild/HoT auf Einzelziel (Whispering Dawn ist AoE ab Fee, bereits reaktiv genutzt) → SCH bewusst unverändert.

| Schritt | Commit | Status | Inhalt |
|---|---|---|---|
| Umsetzung | 0fd058d | GEFIXT | Sustain-Check je Heiler am Ende von `GeneralGCD`, Optionen `UsePreRegen` (erweitert), `UsePreAspectedBenefic`, `UsePreEukrasianDiagnosis` |
| Nachtrag 1: HoT-Spam | 89665b7 | GEFIXT | `CanUse(targetOverride: Tank)` löst über `FindTankTarget` auf, das `CheckStatus` nie aufruft → Restdauer wurde nie geprüft. Fix: `WillStatusEndGCD(StatusRefreshGcdCount, …, TargetStatusProvide)` explizit nach `CanUse` |
| Nachtrag 2: Auslöser | fd19aad | GEFIXT | Countdown-Trigger entfernt (Dungeons haben keinen Countdown); neuer Trigger `TankApproachingMobGroup`: Tank innerhalb Gap-Closer-Reichweite (20 y, alle vier Tank-Gap-Closer) von Hostiles, keine Trials/Raids |
| Nachtrag 3 | 60d5773 · 04d364d · b1f2c61 | GEFIXT | Null-Check in der Hostile-Schleife; SGE-Eukrasia-Druck ohne Dauer-Check; **Starvation**: der Check stand am Ende von `GeneralGCD` und kam im Kampf nie zum Zug → an den Anfang (nach Raise-Early-Outs) |
| Nachtrag 4: HealAreaGCD-Starvation | — | GEFIXT (Spiel bestätigt) | dieselbe Lücke in `HealAreaGCD` aller drei Heiler; Check dort am Ende vor `base`, damit er keine reaktive AoE-Heilung verdrängt |
| Nachtrag 5: Pre-Pull weiterhin aus | — | **KORRIGIERT** (s. C3) | Radius 21 → 26 y als Zeitfenster-Hypothese; Spiel: wirkungslos |
| Nachtrag 6: Ursache | — | GEFIXT, dann präzisiert | Startgruppen haben < 4 Mobs; `mobsInRange >= 4` konnte vor dem Pull nie wahr werden |
| Nachtrag 7 | — | GEFIXT | 4+ war Ausstiegs-, kein Eintrittskriterium: außer Kampf jeder Hostile, im Kampf `WallToWall`-Schwelle |
| Nachtrag 8 | — | GEFIXT | Pre-Pull-Schwelle 2 (ein Streuner ist kein Pull) |
| Nachtrag 9 | — | GEFIXT | beide Schwellen als `[RotationConfig]` je Heiler (Default 2 / 3), Methode `TankApproachingMobGroup(prePull, wallToWall)` |
| Nachtrag 10: UI-Ort | — | KEIN FEHLER | Job-Tab statt Auto-Tab (Auto-Tab = geteilte `[JobConfig]`-Felder, unsere Werte sind job-eigen); `Parent` auf den Toggle gesetzt |
| Später | 5755ad5b | VERWORFEN (SGE) | SGE-Sustain entfernt: Eukrasian Diagnosis ist ein Schild, wird durch Schaden verbraucht, das Refresh-Signal „Status weg" feuerte im Pull alle paar Sekunden. WHM Regen / AST Aspected Benefic sind HoTs, bleiben |

Offen gebliebene, dokumentierte Grenzen: `TerritoryContentType` nur `Trials`/`Raids` ausgeschlossen (Alliance/Variant/Deep Dungeon laufen über die Mob-Zahl); Präzedenz für „Aspected Benefic bevorzugt bei Bewegung" war bereits in AST `HealSingleGCD` vorhanden.

### A3 · `AverageTTK`-Nullfallback (Nutzer-Meldung: keine Heilung am Pull-Start)

**Anlass:** Party sinkt unter 50 % ohne Heilung, „meist wenn keine oGCDs da sind".
**Befund:** `_avgTTK = count > 0 ? total / count : 0f` — solange kein Ziel Trefferhistorie hat (`GetTTK` = NaN für ~2,5 s), ist der Mittelwert 0; `CanUseHealAction` verlangt `AverageTTK > AutoHealTimeToKill` → alle Heil-Flags aus, GCD und oGCD. Betrifft die ersten 2,5 s jedes Pulls und jede neue Add-Welle.
**Fix:** GEFIXT — Fallback `float.PositiveInfinity` (Getter und `ResetAllRecords`). Alle Verbraucher vergleichen `>`/`>=` (`IsLongerThan`, `BaseAction.IsTimeToKillValid`, NinjaRotation), also an der Quelle korrekt. Bug existiert identisch in Upstream.
**Fortsetzung:** der Rest der Meldung (#54) wurde in A8 gelöst — dieselbe Gate-Kette, anderer Auslöser.

### A4 · Review-Funde #57–#62 (Duplikate, Magic Numbers, Kommentare)

Status: alle sechs umgesetzt, CI grün, Verhalten bei Standardwerten unverändert.

| Fund | Commit | Ergebnis |
|---|---|---|
| #58/#59 25 byte-identische Dauer-Ternaries, 26× `>= 4` | 00a426b | `MitigationDebuffDuration`, `ShouldSustainMitigationDebuff`, Config `MitigationSustainHostileCount` (Default 4). `AutoDefenseNumber` war **kein** Wiederverwendungskandidat: zählt Angreifer auf mich, nicht Gegner in Reichweite |
| #60 drei divergente Heiler-Kopien | 6b40600 | je ein `TrySustain…OnTank`; kleine bewusste Verhaltensänderung: GeneralGCD-Pfad bekam die HP-Schwelle, die die Einstellung ohnehin dokumentiert |
| #57/#61 Magic Numbers, doppelte Lookups | 0fe7bed | benannte Konstanten; StateUpdater liest dieselbe Config wie die Job-Zweige; `DataCenter.PartyTank` als einzige Definition; `FindTankTarget` bleibt eigen (priorisiert Tank-Stance) |
| #62 Kommentare | f9e0eff | Inline-Prosa 312 → 237 Zeilen; nicht auf Baseline gedrückt, Rest erklärt Mechanik |

Messung danach: Ternary 25→1, `>= 4` 26→0, Sustain-Aufrufe 9→3, doppelter `SearchById` 2→1.

### A5 · Fork-Audit-Roadmap (05.09.2026)

**Anlass:** Der Originalautor nennt die Änderungen „Trial & Error, >4000 Zeilen die nichts richtig machen". Auftrag: belegen oder widerlegen, korrigieren, codearm.

| Phase | Ergebnis |
|---|---|
| 0 Faktenbasis | Rohdiff 4521 Zeilen: 2830 Markdown, 260 CI, 1431 C# → **558 Anweisungen** über 43 Dateien. Kommentardichte der Ergänzungen 27–50 % gegen Hausmaß 4–20 %: **Stilvorwurf trifft zu.** Fork hatte **0 Tags** (Upstream 952), jede Version `1.0.0.0`: **Versionierungsvorwurf trifft zu.** |
| 1 Substanzprüfung je Bereich (StateUpdater · Ability · OtherInfo · DataCenter/Helpers/Actions · Heiler · Tanks · Melee/Range/Magical · Duty/Extra/PvP) | vier echte Fehler in Fork-Code gefunden und zurückgebaut (5755ad5b): SGE-Sustain (s. A2), Weakness-Faktor ×1,5 (landete bei 1,05 → jeder Geschwächte galt als heilbedürftig), WHM-DoT-Guard prüfte das Ziel des vorigen Casts (`Target` wird erst in `CanUse` gesetzt), `[WSH 16/18]`-Marker im Fenstertitel. Details: `docs/rotation-flow/06-fork-audit.md` §2 |
| 2 Korrekturen | alle umgesetzt |
| 3 Stil | Kommentarüberhang 283 → 102 Zeilen |
| 4 Versionierung | Schema `<upstream>+wsh<n>` (B3): `publish.yaml` spaltet am `+`, Fenster zeigt `InformationalVersion`; Release `7.5.5.41+wsh1` am 05.09. 08:38 UTC auf `ba269301` veröffentlicht |
| 5 CI + Doku | `06-fork-audit.md`, Build grün |

Nebenvorgänge derselben Phase:
- **Restricted-DoT-Filter** (ActionTargetInfo.cs:86): `continue` in der inneren `for` statt der äußeren `foreach` — Ziele der Sperrliste (NameId 9214) wurden nie übersprungen; die korrekte Fassung stand 80 Zeilen tiefer. GEFIXT; Fehlerklasse als dritte Prüfung in `check_base_calls.py`.
- **Refresh-Horizonte gegen Wirkdauern**: alle 13 `BMRShouldRefreshBefore`-Stellen exakt gedeckt (Troubadour/Vigil/Nebula/Tactician/Guardian/Damnation 15 s, Radiant Aegis 30 s, Rampart 20 s). Zwei Auswertungsfehler auf dem Weg (PvP-Variante gewann per `setdefault`; Rampart fehlte, weil Rollenaktion in `Action.resx`). Addle/Feint/Reprisal haben im Sheet keine Zahl (levelskaliert) → `MitigationDebuffDuration` ist dort richtig.
- **#71**: Branch `claude/bmr-mitigation-refresh` war vollständig enthalten (`f2db49b1` Vorfahre), war Löschfall statt Sync-Fall; Nutzer schloss PR #2 und löschte ihn.
- **#67 Upstream-Inhaltsprüfung**: 8 Upstream-Commits seit Branch-Punkt `ee055ca` (53822a8 BRD-Songs · e003bce DRK · df1a8c9 FATE-Targeting · 7b8a2f5 Oblation · 0bde9ed UI-Crash · b5a91d7 SGE · 69f4844 GNB · 83e4d0e BLU Exuviation) einzeln gegen die eigenen Patches geprüft, `f5c8432` gemergt (49d7f8a), alle Branches 0 ausstehend.

### A6 · Code-Review-Loop über den gesamten Diff (05.09.2026)

**Anlass:** „codereview der patches … loop bis alle Fehler beseitigt", umfänglich. 46 Code-Dateien, 2819 Diff-Zeilen Hunk für Hunk; zweiter Durchgang mit strukturellen Scans (43 neue Symbole alle referenziert, alle 38 `skipStatusProvideCheck: true` hinter statusprüfender Bedingung, Spiegel-Behauptungen in Kommentaren gegen den Code) — kein neuer Fund.

| # | Commit | Fund | Art |
|---|---|---|---|
| 1 | 5bb4d39f | Gegnerzahl-Zweig von `ShouldSustainMitigationDebuff` prüfte den Zielstatus nicht; mit `skipStatusProvideCheck` überschrieb der zweite Tank/Melee/Caster den laufenden Debuff. Fix: Zweig verlangt Abwesenheit/Ablauf ≤ 2 GCDs auf `HostileTarget`. Antithese „reaktive Zeile prüft doch": gilt nur für Area-Aufrufer und erst nach der Sustain-Zeile. Nebenfix: Debuff mit null-Ziel prüft nicht den Spieler | Fehler Fork |
| 2 | 2df7dc4e | `TargetType.SafeDotTarget` ohne Aufrufer | tot Fork |
| 3 | bd65f0d4 | UTF-8-BOM in 11 Dateien, die Upstream ohne BOM führt (206/269 Upstream-Dateien haben BOM, diese nicht) | Rauschen |
| 4 | 5b778336 | GCD-Befehlspfad: 5755ad5b hatte nur den oGCD-Zwilling zurückgesetzt (s. C2) | Fehler Fork |
| 5 | 451d9e90 | Co-Tank-Provoke zog den Boss von Superbolide/Living Dead/Holmgang; Gate `NoNeedHealingInvuln()` | Fehler Fork |
| 6 | 28c0e1fc | NIN ohne `HasHostileCountAoeMitigation`, Flag in `NinjaRotation` | Fehler Fork |
| 7 | 990daaeb | `ShieldStatus` um 15 Barrieren ergänzt (Divine Benison, TBN, Brutal Shell, Stem the Tide, Shade Shift, Manaward, Radiant Aegis, Tempera Coat/Grassa, Crest of Time Borrowed, Catalyze, Consolation, Differential Diagnosis, Holosakos, Haimatinon); kann nicht über-krediten (`GetObjectShield() > 0` bleibt Voraussetzung); Guardian's Will nicht aufgenommen (unbestätigt) | Lücke Fork |
| 8 | 3b5e50d5 | MCH-Doppelblock → `BurstWeaveSlotContested`, gegen `AttackAbility` 233–268 geprüft | Duplikat |
| 9 | bfc52584 | `DataCenter.BMRTankbusterImminent` statt dreifacher Bedingung; `UseHpPotion` ohne Durchreich-Parameter; `AnyLivingTankInParty` inline | Duplikat |
| 10 | c1d0ba45 | Upstream-`foreach` in `CalculateDamageFactor` ohne Rumpf (seit 0246bea5) | tot Upstream |
| 11 | ff0d8d43 | Prüfskript: `foreach`, klammerloses `continue`, Expression-Bodied-Overrides | CI |
| 12 | f107eda9 | Reprisal ohne `TargetStatusProvide` (Upstream); zwei Status-IDs 753/1193 gleichen Namens, Zuordnung offline nicht entscheidbar (xivapi/garlandtools/gamerescape gesperrt) → beide in `StatusHelper.ReprisalStatus`, alle Reprisal-Prüfungen darüber | Fehler Upstream |

Verworfen: `params StatusID[]`-Allokation je Aufruf (Hausmuster, s. `HasStatus`).
Geprüft, kein Fehler: Interrupt/AntiKnockback-Umordnung (alle vier Overrides) · `AverageTTK = ∞` bei beiden Verbrauchern · `FindTankTarget` wählt aus `PartyMembers` (WHM/AST-Kommentar trifft zu) · `HealSingleAbility`-Basis leer · `RadiantOnCooldownSpam` upstream nie gelesen · PhantomDefault `out act` · `publish.yaml -split '\+'` · `FindTargetAreaHostile`-Spread wie Upstream 724 · Potion-Ausschluss-IDs · Doppelnullprüfung wie `IsHostileCastingStop`.

### A7 · TODO-Abarbeitung (05.09.2026)

| Punkt | Status | Ergebnis |
|---|---|---|
| #70 Release | war erledigt | Tag/Release seit 08:38 UTC vorhanden; fälschlich als offen geführt (C4) |
| #54 WHM heilt nicht | GEFIXT c6a0a40c | `CanUseHealAction` verlangte `AverageTTK > AutoHealTimeToKill` (8 s) auch für Heiler; die Option hängt unter `UseHealWhenNotAHealer` und meint Nicht-Heiler. `GetTTK` (Rate × Rest-HP) fällt bei einem Pack mit Mobs unter 50 % typisch unter 8 s → alle Heil-Flags aus, `GeneralGCD`/Holy erreicht: jedes gemeldete Merkmal folgt daraus. Gate nur noch für Nicht-Heiler. **Kette vollständig belegt:** `HPNotFull` → `CanUseHealAction` → `NonHealerHealLogic` (Heiler: wahr) → `ShouldHealSingle` (20 % < jede Schwelle, Schild-Credit braucht echten Schild, Invuln nur bei Status) → Dispatch `CanHealSingleSpell` = `GCDHeal \|\| aliveHealerCount == 1` (PartyMembers enthält den Spieler, `IsParty` :711) → `WHM.HealSingleGCD` (Solace, Regen/Sustain nur > 0.3, Cure II) → Zielwahl (`AutoHealRatio` 0.8, kein Bewegungs-Block, Holy wurde gecastet). Kein Glied blockiert |
| Nebenfund | GEFIXT d045e47f | `GetCanTargets` wandte „Only attack targets in view"/Sichtkegel auch auf Heilziele an; Mitspieler hinter der Kamera nicht heilbar. `IsTargetFriendly \|\| TargetOnScreen`. Default aus |
| #55 `_lastHp` | GEFIXT a2a3ec35 | nie beschrieben, Vergleich unerreichbar; entfernt |
| #63 WHM 0.3 / AST 0.4 | KEIN FEHLER | gegensätzliche Semantik ist begründet: Regen reaktiv nur `>` (reiner HoT), Aspected Benefic reaktiv nur `<` (Sofortheil + HoT, instant); Sustain-Boden `>` bei beiden ohne Lücke |
| #65 B3 Reprisal PLD/WAR | GEFIXT 00bc9c6f | in `DefenseAreaAbility` wie DRK/GNB; WARs RotationDesc versprach es bereits |
| #65 B4/B5 Rollen-Lücken | GEFIXT 4b3c9412 | MCH/BRD Second Wind, MNK `HealSingleAbility`, DNC `DefenseSingleAbility` + BMR-Shield-Samba |
| #65 C1 DRG-Trait-Gates | KEIN FEHLER | `CanUse` prüft über `AdjustedID`; Gates redundant, schützen aber die Per-Aktion-Config |
| #68 ChurinDRK Oblation | GEFIXT 52a0817d | `!IsLastAbility(false, OblationPvE)` wie 7b8a2f5 |
| #69 ungenutzte Aktionen | abgearbeitet | Shade Shift, Shukuchi, Horoscope waren falsch gelistet (Basis-Partials nicht durchsucht); SAM Yaten → `MoveBackAbility` (f90c7bf7); Tsubame/Tridisaster/Play/EmergencyTactics_37037 korrekt ungenutzt (Morph/Zweit-ID); Meditate/Flamethrower/Six-sided Star/Overdrives/Liturgy/Dissolve Union: Features ohne Trigger, kein Fehler; PLD-Invuln-Ort durch Cover begründet |
| #72 Buster auf DPS | GEFIXT 4b3c9412 | DefenseSingle für DPS ist ein Einzelziel-Fall; je Job die reaktive Zeile nachgezogen (Addle BLM/PCT/RDM, Feint DRG/MNK/NIN/RPR/VPR mit Job-Gates, SAM zusätzlich Third Eye/Tengentsu, Troubadour/Tactician/Shield Samba BRD/MCH/DNC); Sustain-Zeilen bleiben |
| #66 A4a Dispatch-Stufen | UMGESETZT 4889395f · 157a9ad3 · e6428c19 · e07ceb4b · 672e92ee | `GeneralGCD` von BLU/PhantomDefault/PCT/SAM/SMN als `\|\|`-Dispatcher über benannte Stufen; nur Methodengrenzen eingefügt: 208 +, 5 −, 0 verschoben. MCH ausgelassen: `return base` mitten in der Kette bricht die ganze Methode ab, in einer Stufe liefe die nächste weiter. Keine Local über eine Grenze (SAM `isTargetBoss` in Stufe 1), kein `return false` in den Regionen |

Check-in-Trigger `trig_01NLjkn2dFqmrZXhmxJcWGsQ` ließ sich nicht löschen (Tool nicht angeboten); einmal gefeuert, ignoriert.

### A8 · Audit der gesamten Codebasis, Phase 1: mechanische Scans (05.09.2026)

**Anlass:** Nutzerauftrag „Audit und Code-Review über die gesamte Codebasis", also der ganze Baum inklusive Upstream-Code, nicht nur der Fork-Diff. Werkzeug: `scan.py` über 269 Dateien, acht Fehlerklassen, die dieses Repo tatsächlich schon hatte.

| Fund | Status | Ergebnis |
|---|---|---|
| SAM `MeikyoShisuiCountdown` | GEFIXT ebaa44c7 | `[Range(0, 1, Seconds)]` bei Default 14 s: jede Bedienung des Reglers hätte den Wert auf ≤ 1 s gekappt. Auf 0–15 s gesetzt (Wirkdauer von Meikyo Shisui) |
| BLU `UseBasicInstinct` / `UseMightyGuard` | GEFIXT 3c40d9e4 | Beide Optionen standen in der Oberfläche, wurden aber nirgends gelesen; Aktionen liefen bedingungslos. Defaults sind `true`, Standardverhalten also unverändert |
| Neun veraltete `RotationDesc` | GEFIXT 93f05e68 | Attribute nannten Aktionen, die die Methode nie benutzt (SMN/ChurinSMN Lux Solaris in DefenseArea, BLM/Rabbs Transpose+Retrace, AST/BeirutaAST Arrow+Ewer, PLD Requiescat/Imperator/FoF). Die Rotations-Info im Fenster log damit |
| Elf ungelesene `RotationConfig` | GEFIXT 232d472e | MNK `AutoFormShift`, BLM `ExtendTimeSafely`, BRD `OGCDTimers`, SMN `SecondTypeOpenerLogic`, SGE `ZoeHeal`/`OGCDHeal`, BeirutaSGE `TaurocholeHeal`/`DruocholeHeal`, PhantomDefault `PrayHeal` — samt der auskommentierten Blöcke, für die sie einmal gedacht waren |
| Toter Code in `TargetUpdater` | GEFIXT e224e3f7 | `OldUpdateTargets` (auskommentiert) plus die nur von ihr gerufenen `GetPartyMembers`/`GetAllianceMembers`/`GetMembers`/`GetAllHostileTargets`/`GetClosestTarget`; die aktive `UpdateTargets` füllt dieselben Listen selbst |
| GNB:379 · BRD:614/619 `CanUse(out _)` + `return true` | KEIN FEHLER | Es sind Vorbedingungs-Abfragen innerhalb eines `if`, dessen äußeres `CanUse(out act)` bereits gesetzt hat |
| Sechs `RotationNotes`/`Info_DoNotChange` | KEIN FEHLER | Reine Anzeigetexte, absichtlich ohne Leser |
| 60 `.Target.Target.`-Dereferenzen | KEIN FEHLER | Kein Nullreference-Risiko. Die dort gerufenen Member sind ausnahmslos Erweiterungsmethoden mit eigenem Null-Zweig (`GetHealthRatio` → 0, `DistanceToPlayer` → `float.MaxValue`, `IsBossFromIcon`/`IsBossFromTTK`/`IsDying`/`HasStatus` → false), und jeder echte Instanzzugriff (`CurrentHp`, `CurrentMp`) steht hinter einem erfolgreichen `CanUse` derselben Aktion oder einem eigenen Null-Check (BLU:512). Wo `Target` ohne vorheriges `CanUse` gelesen wird (ChurinSMN 809/1152/1352, ChurinDRK 269), steht der Wert des letzten erfolgreichen Aufrufs, praktisch aus dem Vorframe: höchstens ein Frame Verzug, keine belegbare Fehlwirkung |

### A9 · Mitigation ohne Gefahr (Nutzer-Meldung) und Versionsbezeichnung (05.09.2026)

**Anlass:** „Schimmerschild und Stumpfsinn werden zu oft gecastet, obwohl keine Gefahr vorliegt. Evtl. Reaktionen falsch verdrahtet, z. B. bei Flächenschäden, denen man problemlos ausweichen kann?" — Radiant Aegis und Addle, beide beim SMN.

| Fund | Status | Ergebnis |
|---|---|---|
| Gegneranzahl-Fallback in `ShouldAddDefenseArea` | GEFIXT b8018cf0 | Eigener Fehler aus A7. Der Fallback hielt `AutoStatus.DefenseArea` bei ≥ 4 Gegnern in Reichweite über den gesamten Pull gesetzt. Das Flag öffnet nicht die eine Sustain-Zeile, sondern die komplette Defensivkette des Jobs — und für Melee/Ranged ruft der Dispatcher auf demselben Flag zusätzlich `DefenseSingleAbility` (CustomRotation_Ability.cs:291). Zwölf Jobs meldeten `HasHostileCountAoeMitigation`, darunter SMN, RDM, PCT, BLM: Selbstschilde und Gegner-Debuffs gingen auf Trash dauerhaft raus. Fallback entfernt; das Flag hatte danach keine Leser mehr und ist samt Interface-Member, Basisimplementierung und zwölf Overrides weg. `ShouldSustainMitigationDebuff` bleibt unberührt |
| SMN Radiant Aegis in `GeneralAbility` | GEFIXT 6704335d | `if (!IsLastAction(false, RadiantAegisPvE) && InCombat)` ohne weitere Bedingung. `GeneralAbility` läuft in jedem freien Weave-Slot ohne Gefahren-Gate (Ability-Dispatch :380, nach `AttackAbility` :371, kostet also keinen Burst-Slot). Einziger Schutz war `StatusProvide`, also feuerte der Schild etwa alle 30 s neu, und `usedUp: true` gab dabei auch die zweite Ladung frei — bei echter Gefahr war keine mehr da. Herkunft upstream (2c998686 „Adjusted SMN shield spam logic, again"). Entfernt; bleibt über die BMR-Raidwide-Vorhersage und die Defense-Pfade |
| Gesamtheitlichkeit: gleiches Muster anderswo | KEIN FEHLER | Scan über alle Mitigations-Aktionen in ungegateten Methoden (`GeneralAbility`/`AttackAbility`/`EmergencyAbility`/`GeneralGCD`): zehn Treffer, neun davon mit echter Bedingung (PCT/BeirutaPCT Grassa an DefenseArea oder ablaufendem Tempera Coat, SAM und WAR an HP-Schwellen, SMN:193 an der BMR-Vorhersage). SMN:198 war die einzige bedingungslose Stelle im Baum |
| `IsHostileCastingTank`-Fallback in `…TankBusterAtMe` | **KORRIGIERT → GEFIXT d9a99de7** | Zunächst als KEIN FEHLER eingestuft, s. C10. `IsHostileCastingTankBusterAtMe` lief über `IsHostileCastingTank`, dessen letzte Zeile `return h.CastTargetObjectId == h.TargetObjectId` lautet — wahr für praktisch jeden nicht unterbrechbaren Cast über GCD-Länge auf das gerade angegriffene Ziel. Für einen Tank eine brauchbare Näherung, für alle anderen heißt es: ein Trash-Mob wählt mich als Ziel und die gesamte Einzelziel-Defensivkette öffnet sich. Vom Nutzer im Spiel gemeldet. Zählt jetzt nur noch gesicherte Tankbuster: Aktion aus der kuratierten `HostileCastingTank`-Liste oder Tankbuster-Lock-on-VFX auf dem Spieler. `IsHostileCastingToTank` behält den Fallback, Tank-Verhalten unverändert |
| `IsHostileCastingArea` ohne Betroffenheitsprüfung | GEFIXT 6588832b | Der Vergleich ging ausschließlich gegen die Aktions-ID. Gegner werden bis 48 y gesammelt, also setzte jeder Mob in diesem Radius mit einer gelisteten Aktion `AutoStatus.DefenseArea` — ein Flächeneffekt am anderen Ende eines großen Packs wurde mitigiert wie ein Raidwide. Zusätzliche Bedingung: der Effekt muss den Spieler erreichen können, ein Radius r trifft niemanden jenseits von r. Ausnahmen bleiben `EffectRange == 0` (deckt sowohl ungepflegte Werte als auch die partyweiten Treffer ohne eigenen Radius ab) und ein Cast, der auf den Spieler zielt, weil ein bodenplatzierter Effekt seinem Ziel folgt statt seinem Verursacher |
| Selbstlernende `HostileCastingArea` | offen | in `TODO.md`; durch die Reichweitenprüfung entschärft, aber nicht behoben |
| Versionsbezeichnung „1.0.0.0 + lange Zeichenfolge" | GEFIXT 1c259f10 | Zwei Ursachen. Kein Projekt setzte eine Version, also meldete jeder Build außerhalb eines Tag-Publish den SDK-Default 1.0.0. Und seit .NET 8 hängt das SDK `SourceRevisionId` — den vollen Commit-Hash, von Source Link automatisch gesetzt — an `InformationalVersion`, und genau dieses Attribut zeigt der Fenstertitel (RotationSolverPlugin.cs:275). **Am Artefakt verifiziert:** das veröffentlichte `7.5.5.41+wsh1` trägt `7.5.5.41+wsh1.ba269301c98a192395ccb9e9826be9e890e6ea18`. Default-Version gesetzt, Hash-Anhang aus; der Publish-Workflow überschreibt die Version weiterhin aus dem Tag |

### A10 · Audit der gesamten Codebasis, Phasen 2 bis 4 (05.09.2026)

**Anlass:** Fortsetzung von A8. Phase 2 und 3 mit den Skripten `scan2.py`/`scan3.py` über die Rotationsbäume, Phase 4 über Konfiguration, Oberfläche, Kommandos, IPC und die BMR-Updater. Jeder Scanner wurde vor dem Lauf gegen konstruierte Defekte selbstgetestet; `scan3.py` hatte dabei einen Offset-Fehler (Klasse b fand systematisch nichts), `scan4.py` erkannte mehrzeilige Attributblöcke nicht — beide korrigiert und erneut geprüft, bevor die Ergebnisse verwendet wurden.

| Fund | Status | Ergebnis |
|---|---|---|
| Vier Prozentwert-Schwellen gegen 0..1 statt 0..100 | GEFIXT ad00090e | `PhantomDefault` (Drain Touch Emergency/Healy, Devour) und `BLU` (Missile) verglichen `GetEffectiveHpPercent()` (0..100) mit Konfigurationswerten, die als `[Range(0,1,Percent)]` deklariert und damit als Verhältnis gespeichert sind. Die Bedingungen waren praktisch immer wahr. Mit `* 100f` skaliert |
| `BaseAction.Config` erzeugte die Default-Konfiguration je Aufruf | GEFIXT efc4d039 | `GetDefaults()` legte bei jedem Getter-Zugriff ein neues `ActionConfig` an, im Entscheidungspfad also mehrfach je Frame und Aktion. Einmal erzeugt und gecacht |
| `Rabbs_BLM` Alt-Flare-Opener veränderte eine verworfene Instanz | GEFIXT d0523a8d | `ModifyAltFlareOpenerPvE` bekam die Einstellung einer anderen `BaseAction`-Instanz als der zurückgegebenen; die Änderungen wirkten nicht. Auf eine Instanz zusammengezogen |
| `ShouldCheckStatus` wurde im Provide-Zweig nicht gelesen | GEFIXT 331c1254 | `CheckStatus` hatte einen unerreichbaren Frühausstieg über das tote `ShouldCheckTargetStatus`, und `IsStatusProvided` prüfte den Schalter gar nicht: Wer „Status prüfen" abschaltete, bekam den Provide-Check trotzdem. Frühausstieg entfernt, beide Zweige lesen jetzt `Config.ShouldCheckStatus`; das nirgends gelesene `ShouldCheckTargetStatus` ist samt Debug-Anzeige weg |
| Zweiter Duty-Aufruf im Einzelheilpfad unerreichbar | GEFIXT f2384007 | In `CustomRotation_GCD` stand `HealSingleGCD` der Duty-Rotation zweimal hintereinander, der zweite hinter `IsInOccultCrescentOp \|\| HasVariantCure`. Der erste Aufruf ist bedingungslos, der zweite konnte nie zusätzlich greifen. Die Asymmetrie zum Flächenheilpfad bleibt als offener Punkt in `TODO.md` |
| Null-Prüfung nach der Dereferenzierung | GEFIXT 6189c4cb | `IsTopPriorityHostile` rief `battleChara.GetNamePlateIcon()` vor der eigenen Null-Prüfung; die Erweiterungsmethode greift ohne eigenen Null-Zweig auf die Struktur zu. Prüfung vorgezogen |
| BMR-Verfügbarkeit auf den ersten Tick eingerastet | GEFIXT 5de07717 | `BossModUpdater` und `BMRPlanUpdater` lösten `IsEnabled` einmalig auf und merkten sich das Ergebnis. `ResetAvailabilityCheck()` hat baumweit keinen Aufrufer (verifiziert per Grep), der zweite Reset steht im `catch`, das bei `_isAvailable == false` nicht erreichbar ist. Dalamud meldet das Laden anderer Plugins nicht; wer BossModReborn nach RSR startet oder in der Sitzung aktiviert, hatte alle BMR-Werte bis zum Neuladen auf ihrem Ausfallwert — die BMR-gestützte Mitigations-Zeitsteuerung war damit still abgeschaltet. Verfügbarkeit wird jetzt alle 5 s neu erhoben, dasselbe Intervall wie der bestehende Fallback-Poll |
| 46 Treffer „Level-Gate auf fremde Aktion" | KEIN FEHLER | Durchweg das legitime Muster `!HöhereAktion.EnoughLevel && NiedrigereAktion.CanUse(...)` |
| 41 Treffer „gleicher Rumpf in zwei aufeinanderfolgenden `if`" | überwiegend KEIN FEHLER | Echte Fallunterscheidungen. Zwei geprüft: BRD 398/417 ist eine bewusste Staffelung (3 s vs. 7,5 s plus Lied-Bedingung), VPR 590/973 ist echte Redundanz → `TODO.md` |
| Vier unausgeglichene ImGui-Paare | KEIN FEHLER | Zählartefakte: `PopStyleVar(2)`/`PopStyleColor(3)` schließen mehrere Pushes in einem Aufruf, und die beiden Treffer in `RotationConfigWindow` sind Methodendefinitionen, keine Aufrufe. Nebenfund: dieselben Definitionen sind tot → `TODO.md` |
| 60 `[Range]`/Default-Paare in `Configs.cs` | KEIN FEHLER | Kein Default außerhalb seines deklarierten Bereichs, keine doppelten Eigenschaftsnamen. Die Klasse hatte in A8 einen echten Treffer (SAM `MeikyoShisuiCountdown`), ist jetzt sauber |

**Erreichter Prüfgrad:** statische Selbstprüfung plus selbstgetestete Skripte, Kompilierung über die GitHub-Action (`DispatchChain`, `Build`). Keine Laufzeitbeobachtung im Spiel. Nicht als Ganzes gelesen und daher weiterhin offen: `RotationSolver/UI` jenseits der Paar- und Totcode-Scans, der Rest von `DataCenter`, sowie der Job-für-Job-Durchgang durch die Rotationen, der bisher nur über die Scanner abgedeckt ist.

### A11 · Entscheidungsvorlage E1 bis E4: Kontextermittlung und Umsetzung (05.09.2026)

**Anlass:** Die erste Vorlage war eine Optionsliste ohne durchgerechnete Konsequenzen und schob zwei Fragen als Prüfaufgabe an den Auftraggeber zurück, obwohl beide aus Quellen zu beantworten waren. Nach Rüge nachgeholt: externe Recherche (BossModReborn-Quellcode, Dalamud-Quellcode, Spiel-Fachliteratur), Messung statt Schätzung beim Konfliktrisiko, und die Prüfung jeder Fundstelle gegen die Gegenhypothese „nicht tot, sondern unverdrahtet".

| Fund | Status | Ergebnis |
|---|---|---|
| `SpecialMode` gegen die IPC-Grenze verschoben | GEFIXT 8dc2bd65 | `Hints.SpecialModeType` liefert `(int)hints.ImminentSpecialMode.mode`, `BossModUpdater:91` castet direkt in unser Spiegel-Enum. BossModReborn deklariert `AIHints.SpecialMode` ohne explizite Werte, also 0–4; unser Spiegel übersprang die 3 (`Freezing = 4`, `Misdirection = 5`). Unser `Freezing` entsprach damit dem fremden `Misdirection`, unser `Misdirection` nichts. Ohne Wirkung, weil alle drei Leser nur gegen `Pyretic` (= 1 auf beiden Seiten) vergleichen. `PredictedDamageType` gleich mitgeprüft: stimmt überein |
| Timeline-Werte ohne Vorzeichenbehandlung (E1) | GEFIXT 06c60e97 | **Am fremden Quellcode belegt:** jeder Timeline-Endpunkt rechnet `(float)(next - DateTime.Now).TotalSeconds` und meldet `float.MaxValue` nur bei fehlender Vorhersage. Der Wert läuft bei jedem Ereignis durch 0 ins Negative, bis die State Machine nachzieht — genau dafür existiert der Filter auf der Hints-Seite. Da beide in dasselbe `Math.Min` gehen, schlug ein veraltetes −2 der Timeline eine gültige Hints-Vorhersage von 3 s, und alle Verbraucher (`> 0.6f`) sahen „keine Vorhersage": die Mitigation für das Folgeereignis entfiel spurlos. Raidwide, Tankbuster und Knockback laufen jetzt über einen gemeinsamen Helfer. Downtime und Vulnerable bleiben roh, dort trägt das Vorzeichen die Information. Die erste Vorlage hatte diese Frage als Ablesung im Spiel an den Auftraggeber zurückgegeben, obwohl die Antwort im Quelltext stand |
| `StartOnFieldOpInCombat2` (E3) | GEFIXT 33f8cdff | Der Gegner-Ausschluss ist beabsichtigt — der Zweig reagiert auf Mitspieler im Kampf —, die `&&`-Verknüpfung kehrte aber den Puppen-Ausschluss um: eine Übungspuppe ist ein Gegner und blieb damit der einzige Gegnertyp, der den automatischen Start noch auslöste. **Auslösbarkeit belegt statt vermutet:** in allen drei abgefragten Gebieten steht eine Puppe am Lager, also dort, wo sich Spieler sammeln. Zusätzlich der `if`-Block entfernt, dessen Rumpf nur noch ein auskommentiertes Log war |
| Ungenutzter Code, je Fall gegen „unverdrahtet" geprüft (E4) | GEFIXT 364433e6 | `IncrementState` verlor seinen Aufrufer in `e62d9123`, das den Leistenklick auf die `DTRType`-Fallunterscheidung umstellte; keine der 14 `[EzIPC]`-Methoden führt darauf, Fremdplugins erreichen es also auch nicht. Die Ablösung ist zudem besser: `IncrementState` erkennt das Zyklusende an `TargetingType == Big`, was nur gilt, wenn `Big` die letzte konfigurierte Zielart ist. Die `BeginChild`-Wrapper samt `IsFailed` wurden in `701554b0` wörtlich durch `ImRaii.Child` ersetzt (heute 66 solcher Konstrukte im Fenster) und waren überdies vertragswidrig: Dear ImGui verlangt `EndChild` unabhängig vom Rückgabewert, ein Wiederanschluss hätte den Fehler zurückgeholt. Zwei `.csproj.Backup.tmp` aus dem SDK-Upgrade 14.0.2 → 15.0.0, von keiner Projektdatei referenziert, entfernt und per `.gitignore` ausgeschlossen |
| VPR-Blöcke (E4) | KEIN FEHLER, nicht gelöscht | **Empfehlung gedreht.** Zunächst als wirkungsneutrale Redundanz zur Löschung vorgeschlagen. Im Verbund geprüft: das Muster `!HasHunterAndSwift` kommt viermal vor, der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle Inhalt (positionsbewusste Wahl samt Wechselsperre), an der Den-Stelle fehlt er ganz. Ein fehlender Inhalt ist nicht belegbar, ein vollständiger ebenso wenig — Löschen würde die Asymmetrie verbergen statt etwas zu verbessern |
| UI-Wrapper (E4) | **Empfehlung gedreht** | Zunächst „behalten und die Falle kommentieren", nachdem das Konfliktrisiko über die Dateiaktivität geschätzt worden war (7 bzw. 12 Upstream-Commits). Regionsgenau gemessen (`git log -L … upstream/main`) sind es 0 für die VPR-Regionen und 2 für die UI-Region; das Argument trug also in die Gegenrichtung. Zusammen mit dem ImGui-Vertragsbruch wurde daraus „löschen" |
| Zyklus-Kommandos (E2) | offen, Erweiterung abgelehnt | Als Zustandsfolge ausgewertet: Ausschalten kostet je nach Variante 1 bis 5 Klicks und ist bei `DTRManualAuto` gar nicht möglich. `ToggleAuto` ist dort der einzige Ausschaltweg — der zunächst vorgeschlagene `applyToggle`-Parameter hätte ihn beseitigt, der Vorschlag ist damit widerlegt. Ein zweiter Eingabekanal am Leisteneintrag (`OnClick` bekommt `ClickType` und `ModifierKeys`, verworfen mit `_ =>`) wurde vorgeschlagen und vom Auftraggeber abgelehnt. Bleibt unverändert, Befunde in `TODO.md` |

**Nachprüfung E3 (Wirkungskette vollständig verfolgt):** Die Prämisse hält — `GetAllTargets` nimmt jedes anvisierbare `IBattleChara` außer Begleitern auf, Mitspieler also eingeschlossen, und Übungspuppen sind bei der Standardeinstellung `DisableTargetDummys = false` enthalten. Der Fix bleibt richtig und ist konsistent zur Nachbarstelle `StartOnAttackedBySomeone2`, die `IsDummy` bereits als eigenständigen Ausschluss verwendet. Zwei Restbefunde, die er nicht berührt, stehen in `TODO.md`: der Gegner-Test prüft die Mitgliedschaft in einer mehrfach gefilterten Liste statt der Typzugehörigkeit, und der eigene Spieler wird nicht ausgeschlossen. Der beim Fix entfernte leere `if`-Block war die letzte Spur des zweiten Punktes — verhaltensneutral entfernt, aber ein getilgtes Signal.

**Erreichter Prüfgrad:** statische Selbstprüfung gegen fremden Quellcode (BossModReborn, Dalamud), Kompilierung über die GitHub-Action. Keine Laufzeitbeobachtung; insbesondere ist nicht verifiziert, ob die Übungspuppen aller drei Gebiete dieselbe `NameId` 541 tragen, die `IsDummy` prüft.

### A12 · Entstehungsursachen der offenen Befunde (05.09.2026)

**Anlass:** Frage nach der Kausalität der Entstehung — warum wurde das so gebaut. Erhoben an der Historie (`git log -S` auf die jeweilige Stelle, Einführungs-Commit und dessen Diff), nicht aus Vermutung. Wo nur ein Schluss möglich ist, steht es als solcher.

| Muster | Belege | Mechanismus |
|---|---|---|
| **Veraltende Positivliste** — eine Aufzählung ersetzt eine Fähigkeitsprüfung und wächst bei Erweiterungen nicht mit | Duty-Heilzweig: `7c15f3ed` (09.07.2025) führte `IsInOccultCrescentOp \|\| HasVariantCure` ein und zählte damit **alle damals vorhandenen** Duty-Heilquellen auf; die Bozja-Heilaktionen kamen mit `dbf2f4e0`/`fce39580` (25./28.05.2026) zehn Monate später und wurden nicht nachgetragen. Ebenso `GetHostileTypeDescription` mit vier von fünf Enum-Werten | Die Aufzählung ist zum Zeitpunkt ihrer Entstehung vollständig und wird durch eine spätere Erweiterung an anderer Stelle unvollständig, ohne dass etwas fehlschlägt |
| **Kopie ohne Anpassung** — eine Nachbarstelle wird geklont, ein Teil bleibt unverändert | `CycleStateManualAuto` aus `CycleStateManual` (`e62d9123`); `AutodutyUpdateState` aus `UpdateState` (`0b29eaa6`); VPR Bite/Sting aus der Coil-Struktur; der Puppen-Filter aus dem Muster derselben Datei | Der Klon ist syntaktisch gültig und läuft, die nicht mitgezogene Anpassung bleibt unbemerkt |
| **Kommentar an Code angeglichen statt umgekehrt** | `CycleStateManualAuto` trug bei Einführung den Kommentar „If currently in Manual mode, turn Off" über einem `DoStateCommandType(Auto)`; `2771dd95` (01.10.2025) änderte den Kommentar auf „switch to Auto" | Der Widerspruch zwischen Absicht und Umsetzung wird aufgelöst, indem die Absichtsbeschreibung fällt. **Einschränkung:** welche Seite die Kopie war, ist nicht entscheidbar — `CycleStateManual` trägt denselben Kommentar über einem korrekten `Off`, der Kommentar kann also ebenso der übernommene Rest sein wie der Code die Fehlkopie. Beide Methoden entstanden in `e62d9123`. Festzuhalten bleibt der Vorgang: der Widerspruch wurde später zugunsten des Codes aufgelöst, ohne ihn zu prüfen |
| **Refactoring entfernt Aufrufer, lässt Definition stehen** | `IncrementState` (`e62d9123`), `BeginChild`/`IsFailed` (`701554b0`), `GetHostileTypeDescription`/`SetTargetingType` (`e3b57004`) | Der Compiler meldet ungenutzte private Methoden nicht als Fehler; die Reste akkumulieren |
| **Bibliothekskonfiguration trifft Auslieferung** | `d07d7b66` („fix: add a nuget package") aktivierte `GeneratePackageOnBuild`, damit `RotationSolver.Basic` als NuGet-Paket für Fremdrotationen bereitsteht; zusammen mit `GenerateDocumentationFile` und einem gemeinsamen Ausgabeverzeichnis landet beides im Plugin-Zip | Zwei legitime Ziele — Bibliothek veröffentlichen, Plugin ausliefern — teilen sich ein Ausgabeverzeichnis, ohne dass die Auslieferung gefiltert wird |
| **Fremdschnittstelle als Zahl statt als Typ behandelt** | `SpecialMode`-Spiegelenum gegen `AIHints.SpecialMode` verschoben; Timeline-Vorzeichen nur auf der Hints-Seite normalisiert | Über die IPC-Grenze kommt ein `int`; ohne Abgleich mit der Quelle bleibt eine Abweichung folgenlos, bis der betroffene Wert gelesen wird |
| **Feature aus anderer Epoche als Entscheidungseingang wiederverwendet** | Die selbstlernende `HostileCastingArea` stammt aus der frühen UI-Phase (`51ad02c6` u. a.) und wurde später Eingang der Mitigationsentscheidung | Ein als Komfortfunktion gebautes Merkmal erbt keine Anforderungen an Genauigkeit, die seine spätere Verwendung stellt |

**Nicht belegbar:** die Absicht hinter `SpreadDamagePaths` — die Liste entstand in `33e6acb1` („Refactor for var usage, safety checks, and plugin compat"), also in einem Sammel-Refactoring ohne erkennbare fachliche Begründung. Ob eine Trennung von Spread- und Stack-Markern geplant und nie gefüllt wurde, oder ob die Liste von Beginn an eine Fehlkopie war, geht aus dem Commit nicht hervor.

**Folge für die Behebung:** Vier der sieben Muster sind durch die Behebung des Einzelfalls nicht erledigt. Bei der veraltenden Positivliste ist die Aufzählung durch eine Fähigkeitsprüfung zu ersetzen, sonst veraltet sie erneut; beim Klonmuster ist die gemeinsame Struktur zu extrahieren; bei der Auslieferung ist das Ausgabeverzeichnis zu trennen; bei der Fremdschnittstelle braucht es einen wiederholbaren Abgleich statt einer einmaligen Korrektur.

### A13 · Nachgeholte Falsifikationsstufe zur Entscheidungsvorlage (06.09.2026)

**Anlass:** Die Vorlage war gestellt, ohne dass Stufe 6 des Loops für die einzelnen Punkte ausgeschrieben war. Nachgeholt mit beiden Hypothesen je Punkt. Ergebnis: zwei eigene Befunde fallen, zwei werden gehärtet, eine Empfehlung wird durch eine bessere ersetzt.

| Punkt | H1 „kein Defekt" | H2 „gewählte Option falsch" | Ergebnis |
|---|---|---|---|
| Bozja-Flächenheilung | **widerlegt.** Die Gegenhypothese wäre, dass die Aktionen über den Einzelheilpfad ohnehin erreichbar sind. `BozjaDefault.HealSingleGCD` bietet aber nur `LostCurePvE`; `LostCureIII`, `LostCureIV` und `LostFullCure` stehen ausschließlich in `HealAreaGCD` und sind über keinen anderen Weg erreichbar | Alternative „Fähigkeitsprüfung statt Aufzählung" bleibt die sauberere Konstruktion, steht aber in keinem Verhältnis zu zwei Zeilen in fremdem Code | Befund **gehärtet**, Empfehlung unverändert |
| Release-Paket | **widerlegt.** Die Existenz von `PruneOutputDlls` belegt, dass ein schlankes Paket beabsichtigt ist — die Absicht ist dokumentiert, nur unvollständig umgesetzt | **bestätigt.** Die Empfehlung „Ausgabeverzeichnisse trennen" war wirkungslos, weil `publish.yaml` mit `--output .\\build` baut. DalamudPackager kennt laut eigener Dokumentation `Exclude` und `Include` (semikolongetrennt, relativ zum Ausgabepfad) und packt sonst das gesamte Ausgabeverzeichnis | Empfehlung **ersetzt** durch `Exclude` in der Projektdatei |
| Field-Op, Surrogat-Test | **hält stand.** `AllHostileTargets` ist die Liste der Gegner, die RSR ohnehin als Ziele führt; diese brauchen keinen Sonderweg zum Autostart, Objekte außerhalb dagegen schon. Der Test ist unter dieser Lesart stimmig | — | Befund **zurückgezogen** |
| Field-Op, Spielerausschluss | **hält stand.** Der Optionstext lautet „Auto turn on auto mode when in combat in Bozja/Eureka/Occult Fate/CE", während die Nachbaroptionen ihr Subjekt ausdrücklich nennen („when party is in combat", „when attacked"). Der eigene Kampfeintritt ist das gemeinte Ereignis | — | Befund **zurückgezogen**; die Begründung des bereits umgesetzten Puppen-Fixes `33f8cdff` war entsprechend zu eng, der Fix selbst bleibt richtig |
| Beschreibungsmethoden | **widerlegt.** `e3b57004` entfernte einen zusammenhängenden UI-Block — Anzeigezeile „Current Targeting Mode" und Schalter „Change Targeting to Autoduty Mode" —, nicht versehentlich eine Verdrahtung | — | Befund bestätigt, Löschung bleibt empfohlen; der Wegfall des AutoDuty-Schnellschalters bleibt als Nebenfrage offen |

**Nullvariante nachgetragen:** Sie fehlte in der Vorlage als eigenständige Option, obwohl Stufe 2 sie verlangt. Für jeden Punkt gilt: Belassen ist funktional möglich, die Kosten sind je Punkt in der Vorlage beziffert.

**Restunsicherheit:** Ob `LostCureIII`/`IV`/`FullCure` im Spiel tatsächlich flächig heilen, ist nicht verifiziert — die einschlägige Wiki-Domain ist über den Netzwerk-Proxy dieser Umgebung nicht erreichbar. Für die Bewertung ist es unerheblich, weil die Duty-Rotation sie im Flächenpfad anbietet und nur dieser Pfad abgeschnitten ist; für die Frage, ob die Platzierung selbst richtig ist, wäre es zu klären.

### A14 · Umsetzung der Entscheidungsvorlage (06.09.2026)

| Fund | Status | Ergebnis |
|---|---|---|
| Duty-Flächenheilung in Bozja unerreichbar | GEFIXT 30ec4690 | Bedingung `IsInOccultCrescentOp \|\| HasVariantCure` aus dem automatischen Flächenheilzweig entfernt, damit fragen alle vier Heilzweige die Duty-Rotation gleich. Duty-Rotationen ohne Flächenheilung liefern `false`, worauf die anderen drei Zweige ohnehin bauen |
| Beschreibungsmethoden ohne Aufrufer | GEFIXT f7b150c8 | `GetHostileTypeDescription` und `SetTargetingType` entfernt. Ablösung belegt: `e3b57004` nahm den zugehörigen UI-Block heraus, `813c7d73` hatte beide zusammen mit ihm eingeführt |
| `/rotation Auto <Zahl>` ohne Wirkung im Aus-Zustand | GEFIXT 5058dda7 | Der numerische Weg setzt `TargetingIndex` jetzt dort, wo es der Namensweg tut, statt es `UpdateTargetingIndex` hinter dem `DataCenter.State`-Wächter zu überlassen. Negativer Eingabewert wird korrekt in den Bereich gefaltet |
| `DoOneCommandType` mit totem Parameter | GEFIXT b85312df | Auf `WithPlayerRole(Action<JobRole>)` reduziert; drei Lambdas, die nie ausgeführt wurden, und eine Generik ohne Träger sind entfallen |
| Leisteneintrag je Frame neu gesetzt | GEFIXT c10fb06a | Zuweisung nur noch bei geändertem Text oder Symbol, wie es `UpdateToast` in derselben Datei bereits handhabt |
| `DTRManualAuto` ohne Ausschaltweg | KEIN FEHLER, geschlossen | Der Enum-Text lautet „Cycle between Manual and Auto", der Code bildet genau diesen Zwei-Zustands-Zyklus ab. Ein fehlender Ausgang ist mit der Beschreibung vereinbar; welche Seite der Kopie den Fehler trug, ist historisch nicht entscheidbar (s. A12) |
| Release-Paket, 14 MB Ballast | **nicht umgesetzt**, Abwägung dokumentiert | Der Umsetzungsweg trägt nicht: `DalamudPackager.targets` reicht `Exclude` im Standard-Target nicht durch, der Task läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt. Der vorgesehene Weg verlangt, diese Datei anzulegen und den vollständigen Task-Aufruf samt aller Manifest-Felder nachzubauen. Zudem vergleicht `Exclude` laut Task-Quelle (`DalamudPackager.cs:187`) exakt über `List.Contains`, kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Ein nachgebauter Task-Aufruf am Veröffentlichungspfad, den der Build-Workflow nicht prüft — er kompiliert, er packt nicht —, steht in keinem Verhältnis zu 3,5 MB Downloadgröße. Geprüfte Nebenfrage: die XML-Dokumentation wird zur Laufzeit nicht gelesen, ein Ausschluss wäre fachlich unbedenklich |

**Erreichter Prüfgrad:** statische Selbstprüfung, Kompilierung über die GitHub-Action. Keine Laufzeitbeobachtung. Der Flächenheil-Fix ist im Code vollständig nachvollziehbar (Zweig, Duty-Rotation, Aktionen), aber nicht im Spiel bestätigt.

### A15 · Vollständiger Loop-Durchgang über die offenen Punkte (06.09.2026)

**Anlass:** Erneute Prüfung aller offenen Punkte, erstmals gegen die vier neu aufgenommenen Regeln — Persistenz- und Schnittstellenvertrag, Betroffenenkreis, Feature Toggle bei fehlender Nachweismöglichkeit, Trennung von Defekt und technischer Schuld.

| Punkt | Ergebnis der Neuprüfung |
|---|---|
| Selbstlernende AoE-Liste | **Von Defekt zu technischer Schuld herabgestuft.** Die tragende Behauptung — ein gelernter Eintrag sei nur durch Editieren der Datei zurückzunehmen — ist widerlegt, s. C12. Es bleibt der Zeitraum zwischen Fehllernen und Nutzerkorrektur, entschärft durch die Reichweitenprüfung. Neu: eine Verschärfung der Lernbedingung wäre eine Verhaltensänderung ohne Nachweismöglichkeit und gehört damit hinter eine eigene Option statt in den Standardpfad |
| Zyklus-Kommandos | Bleibt technische Schuld. Neu erhoben: `DTRType` und `CycleType` liegen als Ordinalzahlen in der Nutzerkonfiguration, ihre Reihenfolge ist bei einem Umbau nicht frei änderbar — der Persistenzvertrag ist Teil der Auflösungsbedingung |
| Release-Paket | Bleibt technische Schuld. Betroffenenkreis präzisiert: `GeneratePackageOnBuild` bedient die Autoren abgeleiteter Rotationen, die Downloadgröße trifft die Endnutzer; eine Auflösung darf die erste Gruppe nicht ausschließen |
| VPR, `AutodutyUpdateState` | Bleiben technische Schuld mit Adressat Upstream; unverändert |
| `SpreadDamagePaths` | Bleibt technische Schuld; ohne Fehlwirkung, weil alle drei Listen geprüft werden |
| ChurinDNC | Einziger verbleibender Defekt. Unverändert nicht behoben, weil die Absicht der fremden Rotation nicht belegbar und der Eingriff ohne Spieltest nicht abzusichern ist |
| Prüfskripte ohne Selbsttest | Neu aufgenommene technische Schuld aus der vorigen Runde, mit Auflösung vor dem zweiten Audit-Durchgang |

`TODO.md` ist entsprechend nach Defekt, technischer Schuld und offener Arbeit gegliedert; jeder Eintrag trägt jetzt seinen Betroffenenkreis (Endnutzer, Autoren abgeleiteter Rotationen, Upstream-Pflege), und jede technische Schuld ihre Kosten und Auflösungsbedingung.

---

### A16 · Neubewertung aller Patches gegen die vier neuen Regelachsen (06.09.2026)

**Anlass:** Auftrag, alle bisherigen Patches vollständig nach dem erweiterten Loop neu zu bewerten. A15 hatte die Achsen auf die offenen Punkte angewandt; hier werden sie auf den umgesetzten Code angewandt.

**Methodenwahl.** Der Fork liegt 294 Commits vor `upstream/main`, davon rund 80 nach dem letzten Release. Eine Bewertung Fundstelle für Fundstelle wäre die von der Gesamtheitlichkeits-Anforderung ausgeschlossene Betrachtungsweise. Statt dessen wurde je Achse ein Maß gebildet, das den Wirkungsbereich selbst misst, und als Prüfskript hinterlegt. Erhoben vor Beginn: `git rev-list --left-right --count upstream/main...HEAD` = `0 294`, also keine ausstehenden Upstream-Commits.

**Bezugsgröße korrigiert.** Der Persistenz- und der Paketvertrag bestehen gegenüber der Version, die ausgeliefert wurde, nicht gegenüber Upstream: die gespeicherten Einstellungen und die kompilierten Fremdrotationen stammen vom Fork. Alle vertragsbezogenen Messungen laufen deshalb gegen den Tag `7.5.5.41+wsh1`, nicht nur gegen `upstream/main`.

| Achse | Maß | Ergebnis |
|---|---|---|
| Feature Toggle | `scan5.py`: Änderungen nach der Dispatch-Kette klassifiziert — die immer laufenden Methoden gegen die, die `StateUpdater` erst über ein `AutoStatus`-Flag freischaltet | 6 Zeilen in ungegateten Methoden. PCT prüft `AutoStatus.DefenseArea` selbst, AST und WHM hängen an `UsePreAspectedBenefic` bzw. `UsePreRegen`, Phantom an `OccultEtherSelf`/`OccultEtherThreshold`, SMN ist der bereits in A9 erfasste Fall. Kein Verstoß offen |
| Persistenzvertrag | `scan6.py`: Ordinalwerte aller Enums gegen die Basisrevision, getrennt danach, ob das Enum aus der gespeicherten Konfiguration erreichbar ist | 0 Vertragsbrüche gegen beide Basen. Einziger Ordinalwechsel ist `SpecialMode` (A11), und dieses Enum ist nicht persistiert |
| Paketvertrag | `scan7.py`: öffentliche und geschützte Member von `RotationSolver.Basic`, entfernt oder mit geänderter Parameterzahl, gegen den Release-Tag | 2 Funde, beide entfernt: `HasHostileCountAoeMitigation` und `ActionConfig.ShouldCheckTargetStatus` |
| Defekt vs. Schuld | Gliederung in `TODO.md` | Unverändert seit A15: ein Defekt, sechs technische Schulden |

**Fund zum Paketvertrag.** Beide entfernten Member waren im ausgelieferten Stand `7.5.5.41+wsh1` enthalten und sind heute nirgends mehr im Baum. Eine abgeleitete Rotation, die `HasHostileCountAoeMitigation` überschreibt oder `Config.ShouldCheckTargetStatus` liest, kompiliert gegen die nächste Paketversion nicht mehr. Für die gespeicherte Konfiguration ist das folgenlos — Dalamud deserialisiert mit `MissingMemberHandling.Ignore` —, für die Autoren abgeleiteter Rotationen ist es nach Semantic Versioning eine Major-Änderung. Berichtet, nicht bewertet: die Wahl der nächsten Versionsnummer gehört zur Release-Freigabe.

**Zurückgenommen: der Befund zu `MitigationSustainHostileCount`.** Im Verlauf dieser Runde war festgehalten worden, die Gegnerzahl-Heuristik sei nicht abschaltbar und habe zur ursprünglichen Beschwerde über dauerhaft gecastetes Addle beigetragen. Die Entstehungskette widerlegt den zweiten Teil und entkräftet den ersten: `c01a5e23` führte die Schwelle als feste 3 ein, `87646bf1` hob sie auf 4, `00a426b1` machte sie ohne Verhaltensänderung konfigurierbar, und `5bb4d39f` schloss genau die Wirkung, die beklagt worden war — der Zweig verlangt seither, dass der Debuff auf dem Ziel fehlt oder binnen zwei GCDs endet. Eine Option im Sinne der Feature-Toggle-Regel besteht; was fehlt, ist allein ein harter Aus-Zustand, dessen Bedarf nicht belegt ist. Nicht in `TODO.md` aufgenommen.

**Zwei Erkennungsfehler in den eigenen Prüfmitteln, beide mit leerer Ausgabe.** `scan5.py` erkannte Eigenschaften mit Ausdruckskörper nicht als eigenen Gültigkeitsbereich und schrieb deren Zeilen der darüberstehenden Methode zu; dadurch stand MCH `BurstWeaveSlotContested` fälschlich unter `EmergencyAbility`. `scan6.py` meldete zweimal einen sauberen Baum: `git ls-tree` nimmt den Glob-Pfadfilter von `git ls-files` nicht an, weshalb die Basisrevision leer blieb, und die Erkennung persistierter Enums sah nur öffentliche Member, wodurch der `[JobConfig]`-Generator hinter `TargetHostileType` unsichtbar blieb. Beide Fälle sind jetzt Bestandteil der jeweiligen Selbsttests. Das bestätigt die Regel, die den stillen Nullbefund als nicht unterscheidbar vom sauberen Baum behandelt: ohne Selbsttest wären drei Achsen als geprüft gemeldet worden, ohne geprüft worden zu sein.

**Erreichter Prüfgrad:** statische Prüfung mit Skript, dazu Belegführung an der Versionsgeschichte. Keine Laufzeitbeobachtung.

---

### A17 · Umsetzung von Option A zum Paketvertrag (06.09.2026)

**Anlass:** Freigabe der in A16 vorgelegten Option A — Ausweis der entfernten Member als Major-Änderung, Code unverändert.

**Befund bei der Umsetzung: der Ausweis kann nicht numerisch erfolgen.** `Directory.Build.props` bindet `<Version>` an die Upstream-Version und kennzeichnet den Fork über das Build-Metadatum `+wsh1`. NuGet entfernt Build-Metadaten bei der Normalisierung — `1.0.7+r3456` wird als `1.0.7` behandelt ([Package versioning](https://learn.microsoft.com/nuget/concepts/package-versioning#normalized-version-numbers)) —, und `publish.yaml:41` übergibt `PackageVersion` ohnehin ohne Suffix. Eine Major-Änderung hat in diesem Schema keine Stelle, an der sie sichtbar würde. Option A wurde deshalb dokumentarisch umgesetzt: `CHANGELOG.md` mit einem Abschnitt „Unreleased", der beide Member, ihren Wegfallgrund und die Folgen für Paketkonsumenten benennt und die Sonderlage der Versionsnummer begründet.

**Neuer Defekt, dabei erhoben.** Dieselbe Kette ergibt, dass das im Release-ZIP mitgelieferte `.nupkg` unter der Identität `RotationSolverReborn.Basic 7.5.5.41` läuft, also unter derselben wie das Upstream-Paket, bei abweichendem Inhalt. Als Defekt in `TODO.md` erfasst, nicht behoben: die Wahl zwischen eigenem `PackageId`, Prerelease-Label und Verzicht auf die Paketauslieferung berührt die Autoren abgeleiteter Rotationen verschieden und gehört zur Release-Entscheidung.

**Korrektur an `scan7.py`, in drei Schritten.** Die in A16 berichtete Zahl von zwei Funden bleibt, ihr Zustandekommen war jedoch unvollständig belegt. Erstens las das Skript keine Interface-Member, da diese keinen Sichtbarkeitsmodifikator tragen; der Rückbau von `HasHostileCountAoeMitigation` betraf auch `ICustomRotation`. Zweitens erzeugte die anschließende Schlüsselung nach deklarierendem Typ 55 Scheinbefunde, weil ein Prosakommentar mit dem Wort „struct" als Typdeklaration gelesen wurde und alle folgenden Member der Datei umhängte — der Kommentar stammt aus `6189c4cb` und existiert nur auf einer Seite des Vergleichs. Drittens zählte das Skript `internal` deklarierte Interface-Member mit, die außerhalb der Assembly nicht sichtbar sind; `ICustomRotation.HasHostileCountAoeMitigation` ist genau so deklariert und damit **kein** Bruch der Paketoberfläche. Erst das dritte Ergebnis ist das gemessene. Alle drei Fälle sind jetzt Bestandteil des Selbsttests.

**Widerlegte Zwischenannahme.** Beim Lesen des Einführungs-Commits `00a426b1` entstand der Verdacht, dessen Aussage „Behaviour is unchanged at default settings" sei falsch, weil `ShouldSustainMitigationDebuff` heute eine zusätzliche `WillStatusEndGCD`-Bedingung trägt. Der Verdacht ist widerlegt: die Verschärfung kam erst mit `5bb4d39f`, der Commit-Text war zutreffend.

**Erreichter Prüfgrad:** statische Prüfung mit Skript, Belege aus Versionsgeschichte und Fremddokumentation (Microsoft Learn). Keine Laufzeitbeobachtung; das Paketverhalten ist nicht am Artefakt nachgestellt worden.

---

### A18 · Thin Air an den MP-Druck koppeln (07.09.2026)

**Anlass:** Auftraggeberwunsch, die Ladung für teure Zauber erst dann einzusetzen, wenn die MP knapp sind und Klartraum (Lucid Dreaming) sie nicht auffangen kann. Der Rez-Zweig und die Reservierung der letzten Ladung sind ausdrücklich als richtig bestätigt und bleiben unangetastet.

**Ausgangslage am Artefakt.** `WHM_Reborn.cs` zündet Thin Air aus zwei Gründen: der nächste GCD kostet mindestens `ThinAirNeed` MP (Standard 1000), oder ein Rez steht an. Der Kostenzweig las bislang ausschließlich `action.Info.MPNeed`; der eigene MP-Stand kam in der Bedingung nicht vor.

**Optionen.** Nullvariante (Verhalten belassen, Wunsch als technische Schuld führen) · eigener MP-Schwellenwert für Thin Air · Anbindung an den bereits vorhandenen `LucidDreamingMpThreshold` · Kopplung an eine Aufzählung der Heilzauber statt an die Kostenschwelle.

**Abwägung und Wahl.** Die Anbindung an `LucidDreamingMpThreshold` (`Configs.cs:1309-1311`, `[JobConfig]`, Standard 6000) gewinnt: Es ist derselbe Wert, an dem `ModifyLucidDreamingPvE` entscheidet, ob Klartraum fällt, und beide Seiten der Entscheidung lesen damit dieselbe Zahl. Ein zweiter Schwellenwert wäre eine zweite Wahrheit, die auseinanderlaufen kann. Die Aufzählung der Heilzauber scheidet nach Parnas' *Lack of Movement* aus — sie veraltet mit dem nächsten Zauber, während die Kostenschwelle das gemeinte Merkmal direkt misst.

**Falsifikation.** Gegen die Notwendigkeit spricht, dass Thin Air zwei Ladungen mit je 60 s hat und eine „verschwendete" Ladung nachlädt; dagegen steht, dass die Reservierungsoption die Knappheit der Ladungen bereits als real anerkennt. Gegen die gewählte Bedingung spricht der Fall, dass Klartraum zwar läuft — und damit auf Cooldown ist —, die MP aber trotzdem unter der Schwelle liegen; dann feuert Thin Air zusätzlich. Das ist gewollt: Wenn die MP trotz laufender Regeneration unter der Schwelle bleiben, ist die Lage tatsächlich knapp.

**Randfall, der die Bedingung geformt hat.** „Klartraum auf Cooldown" allein greift zu kurz: Ist Klartraum abgeschaltet oder das Level nicht erreicht, ist die Aktion nie auf Cooldown, und die Bedingung wäre dauerhaft falsch — Thin Air hätte dann bei aktivierter Option nie mehr auf einem teuren Zauber gelegen. `UnderMpPressure` prüft deshalb `!EnoughLevel || !IsEnabled || Cooldown.IsCoolingDown`.

**Umsetzung.** `ThinAirOnMpPressureOnly`, Standard `false`. Damit bleibt das ausgelieferte Verhalten unverändert, wie es die Feature-Toggle-Regel für Verbesserungen ohne Nachweismöglichkeit verlangt — eine Laufzeitbeobachtung ist hier nicht verfügbar. Der Rez-Zweig ist bewusst nicht berührt: ein Rez soll auch bei vollen MP von der Ladung profitieren.

**Bewusst nicht mitgeändert:** `BeirutaWHM.cs:397-404` trägt dieselbe Konstruktion, ist aber eine fremde Rotation; eine Änderung dort setzt die Absicht ihres Autors voraus.

**Erreichter Prüfgrad:** statische Prüfung und CI-Kompilierung. Keine Laufzeitbeobachtung, die Wirkung im Spiel ist nicht nachgestellt.

---

### A19 · Kritische Neuprüfung des Synergie-Konzepts (07.09.2026)

**Anlass:** Auftrag, Konzept und Umsetzungsplan erneut vollständig im Loop zu prüfen und mögliche Hindernisse nicht nur zu benennen, sondern zu beseitigen.

**Vier Hindernisse, drei davon im eigenen Entwurf.**

| # | Hindernis | Auflösung |
|---|---|---|
| H1 | Der Einschub braucht einen Cast mit Eigenwert; der DoT deckt nur einen GCD, danach bliebe Glare als reiner Verlust | Durchgerechnet: **ein einziger Einschub genügt** für die vollen 7 s. Ein zweiter bringt nichts und kostet einen GCD. Der DoT reicht exakt aus |
| H2 | Die Bedingung `StunRemainingShortest > GCDTime(1)` war falsch | Sie hätte nie gegriffen — nach dem ersten Sanctus beträgt die Restzeit 1,5 s, weniger als ein GCD. Richtig ist `> 0`, was zugleich selbstbegrenzend genau einen Einschub erzeugt |
| H3 | Die geplante Blockverschiebung (DoT vor Sanctus) verzögert den **ersten** Stun um einen GCD und kehrt damit den Vorrang der Schadensvermeidung um, den das Konzept trägt | Verzicht auf die Verschiebung. Der Sanctus-Block bleibt und bekommt eine Aussetzbedingung; der vorhandene DoT-Block fängt den GCD auf. Nebenwirkung: der teuerste Merge-Posten des Plans entfällt |
| H4 | Welche der rund zwölf Betäubungs-Ids Sanctus anlegt, ist offline nicht bestimmbar | Bündelung zu einer Statusgruppe nach dem Muster `StatusHelper.RangePhysicalDefense`. Die Frage wird gegenstandslos, fremde Betäubungen zählen mit — erwünscht |

**Folge für den Plan:** Vier Schritte werden zu drei. Der frühere Schritt A ist kein eigener mehr — er und die Streckung sind zwei Gründe für dieselbe Aussetzung, und die Aussetzbedingung kennt beide. Neu ist eine **Ersatzgarantie**: Sanctus wird nur ausgesetzt, wenn ein Cast mit Eigenwert bereitsteht (`DiaPvE.CanUse(out _)`), sonst bleibt Sanctus die richtige Wahl.

**Verifiziert statt angenommen:** `DataCenter.JobRange` ist `public static float` (`DataCenter.cs:912`), `AllHostileTargets` ist `public static List<IBattleChara>` (`:49`), Statusgruppen mit mehreren Ids sind etabliert (`StatusHelper.cs:295-310`), und die Spieldaten führen `Stun`, `StunResistance`, `Slow`, `SlowResistance` und `ArmsLength`.

**Neues Prüfmittel:** `.github/scripts/audit/stun_coverage.py` simuliert eine Einschubregel GCD für GCD und weist Deckung und Kosten aus. Es hat H2 gefunden und liegt deshalb versioniert im Repository, mit Selbsttest gegen vier konstruierte Fälle — darunter die verworfene Regel, die dort als „greift nie" belegt ist. Ergebnis bei 2,5 s GCD: heute 5,5 s, Konzeptregel 7,0 s für genau einen eingeschobenen GCD; bei 2,0 s GCD lückenlose Deckung.

**Erreichter Prüfgrad:** statische Prüfung, Modellrechnung mit Selbsttest, Verifikation der verwendeten Bausteine am Quellcode. Keine Laufzeitbeobachtung; die Modellannahmen (gleichmäßiger Schaden, feste GCD-Länge) sind nicht am Spiel geprüft.

---

### A20 · Zweite Konzeptprüfung und Umsetzung der Schritte 1 und 2 (07.09.2026)

**Anlass:** Auftrag, Konzept und Plan erneut vollständig zu prüfen, Hindernisse zu beseitigen und die im Konzept stehenden Teile umzusetzen.

**Zwei weitere Hindernisse, beide im eigenen Entwurf.**

| # | Hindernis | Auflösung |
|---|---|---|
| H5 | Die Messskizze filterte auf `DataCenter.JobRange` — für Heiler **25 Yalms**, die Angriffsreichweite, nicht der 8-Yalm-Wirkradius von Sanctus. Ferne, nie betäubte Gegner hätten die Deckung dauerhaft unvollständig erscheinen lassen; die Regel hätte nie gegriffen | Radius als Parameter, gespeist aus `Info.EffectRange`. Nebenwirkung: Die Messung gehört zur Aktion, nicht in einen jobunabhängigen Updater — die Einhängung in den `MajorUpdater` entfällt |
| H6 | `StunRemainingShortest > 0` beschreibt einen einzelnen Gegner. Bei laufend hinzukommenden Gegnern hätte die Regel gestreckt, obwohl ein Cast die Neuzugänge mit voller Dauer erwischt hätte — die Resistenz zählt je Gegner | Zwei Wahrheitswerte statt einer Zeit: gestreckt wird, wenn alle im Radius betäubt sind oder keiner mehr betäubt werden kann. Bei einem Gegner gleichbedeutend, das Modell bleibt gültig |

**Zwei Notfallvorbehalte gestrichen, nicht gebaut.** Der HP-Vorbehalt ist wirkungslos, weil der Dispatcher alle Heil- und Verteidigungszweige vor `GeneralGCD` aufruft — ein kritischer Zustand erreicht den Code nicht. Der BMR-Vorbehalt ist wirkungslos, weil ein vorhergesagter Raidwide vom Boss kommt und von der Betäubung nicht berührt wird. Ein Vorbehalt, der nachweislich nichts abfängt, ist toter Code.

**Umsetzung.** `StatusHelper.StunStatus` (18 Ids) und `.StunResistanceStatus` (2), beide aus den generierten Spieldaten belegt; `CustomRotation_OtherInfo.SurveyStuns(radius, out allStunned, out headroom)`; in `WHM_Reborn` die Optionen `StretchHolyStun` (Standard aus) und `StretchHolyMinHostiles` (Standard 3) sowie `ShouldStretchHolyStun()` als Bedingung vor dem unveränderten Sanctus-Block. Kein Block verschoben.

**Ersatzgarantie** über die gesamte DoT-Kaskade (`DiaPvE`, `AeroIiPvE`, `AeroPvE`), nicht nur über `DiaPvE` — auf niedrigerem Level ist Dia nicht verfügbar, und ohne die Kaskade wäre der ausgesetzte GCD auf Glare gefallen, also reiner Verlust.

**Erreichter Prüfgrad:** statische Prüfung, Verifikation aller verwendeten Bausteine am Quellcode (`RotationConfigAttribute.Parent`, `IBaseAction.Info.EffectRange`, `AllHostileTargets`, `DistanceToPlayer`), Modellrechnung mit Selbsttest, CI-Kompilierung. Keine Laufzeitbeobachtung: dass die Regel im Spiel den gemeinten Zeitpunkt trifft, ist nicht belegt — deshalb Standard aus.

### A21 · Abarbeitung der drei ungeprüften Punkte aus Konzept 09 (07.09.2026)

**Anlass:** Das zweite Audit zu `docs/rotation-flow/09-tank-selfprotection.md` ließ drei Punkte als ungeprüft stehen — ob `ClarityOfCorundum` den Catharsis-Auslöser abbildet, wie zwei gleichzeitige Schilde ihren Verbrauch aufteilen, und welche Holmgang-Id das Spiel setzt. Sie waren Hindernisse der geplanten Umsetzung, nicht Optionen, und wurden deshalb abgearbeitet, bevor etwas vorgelegt wurde.

**Die Prüfung hat mehr widerlegt als bestätigt.** Drei eigene Vorbefunde sind gefallen:

| Vorbefund | Widerlegung | Folge |
|---|---|---|
| Gunbreaker als Klasse-A-Rückhaltefall (Fallvarianten 10, 10b) | `Status.resx`, Status 2685: der Heilstoß löst „upon falling below a certain level **or expiration of effect duration**" aus — er kommt in jedem Fall, ist also nicht raubbar. Nebenbefund: Der auslösertragende Bezeichner ist `CatharsisOfCorundum`, nicht `ClarityOfCorundum` (2684, reine Schadensreduktion) | Fall ganz entfallen, Umfang von O2 halbiert |
| „Mehr Gesamtpuffer heißt in **jedem** Aufteilungsmodell geringere Verbrauchswahrscheinlichkeit" | Unbelegte Verallgemeinerung. Für Heilerschilde ist überliefert, dass der stärkere den schwächeren verdrängt; unter dieser Mechanik *ersetzt* ein Heilerschild die Barriere, statt sie zu schonen — entgegengesetzte Gegenmaßnahme. Zudem führt der Client nur einen `ShieldPercentage`-Wert je Charakter (`ObjectHelper.cs:3372`), die Aufteilung ist also **nicht beobachtbar** | Schluss zurückgenommen; The Blackest Night vom Kern zum schwächsten Teil des Vorhabens |
| „Drei von vier Holmgang-Ids fehlen, vier `HallowedGround`-Ids fehlen" | Nullbefund über den bloßen Bezeichner. 88 und 1305 sind „Unable to move" — der Bewegungs-Debuff auf dem Ziel; 1304 die PvP-Selbstform. `Holmgang_409` ist die richtige und bereits vorhanden. Bei `HallowedGround` trifft die Meldung zu | Forderung auf `HallowedGround`, `HallowedGround_1302` und `UndeadRebirth` eingegrenzt |

Zusätzlich trug das Paladin-Beispiel in der Falsifikation nicht: `HallowedGround` steht in keiner Form in `NoNeedHealingStatus`, löst also gar keine Unterdrückung aus. Der tragfähige Beleg ist Superbolide. Am Sachverhalt ändert das nichts, am Beleg alles.

**Drei neue Befunde.**

1. **`SCH_Reborn.cs:830`** trägt dieselbe invertierte Prüfung wie `ActionTargetInfo.cs:3536`. Damit ist der Fund eine Defektklasse mit benennbarer Ursache: `NoNeedHealingInvuln` liefert `true` bei *fehlendem* Schutz, der Name sagt das Gegenteil.
2. **`UndeadRebirth` (3255) fehlt in `NoNeedHealingStatus`.** Die Aktionsbeschreibung (`ActionId.resx`, Aktion 3638) belegt drei Living-Dead-Phasen, nicht zwei; die dritte ist ein Zustand, in dem der Dunkelritter nicht sterben kann. *Lack of Movement* nach Parnas — die Aufzählung war korrekt und wurde durch eine Spielerweiterung unrichtig.
3. **Der Zielwahl-Defekt wiegt schwerer als bewertet.** Die Kette `WillStatusEnd`→`StatusTime` (`StatusHelper.cs:781`, `:845`) liefert bei fehlendem Status `0f` und damit `true`. Im Regelfall bleibt `healingNeededObjs` leer, und *alle* darauf aufbauenden Stufen laufen ins Leere — Heiler-Vorrang, Tank-Vorrang und die Auswahl des am schwersten Verletzten. Die generische Heilzielwahl ist blind für den Schwerstverletzten und wählt faktisch immer den Tank. Rangstufe 1, nicht Stufe 3.

**Prüfmittel statt Einzelfall.** Weil dieselbe Defektklasse zweimal von Hand gefunden wurde, ist sie als Skript versioniert: `.github/scripts/audit/scan8.py` meldet bool-Member mit negierendem Namen, die im Baum mit beiden Polaritäten gelesen werden, mit Selbsttest gegen konstruierte Fälle.

**Der erste Lauf fand eine zweite, unverwandte Defektklasse:** `IsConditionCannotTarget()` wird an sieben Stellen mit `return null` statt `continue` ausgewertet (`FindDancePartner`, `FindKardia`, `FindTankTarget`), während drei strukturgleiche Nachbarstellen derselben Datei `continue` verwenden. Ein Gruppenmitglied in einer Zwischensequenz beendet damit die gesamte Zielsuche. Die in beiden Zweigen wortgleiche Debug-Meldung ist der Beleg für *Ignorant Surgery*. In `TODO.md` erfasst, nicht umgesetzt — außerhalb des Auftrags und mit erheblichem Blast Radius.

**Ergebnis für das Umsetzungskonzept:** aus zwei Schritten wurden drei, nach Beleglage geordnet. Was im zweiten Audit als Kern galt — die Rückhaltung —, ist der schwächste Teil; was dort als Nebenschritt lief, betrifft das Überleben der Gruppe.

**Erreichter Prüfgrad:** statische Prüfung an Repository-Artefakten (`Status.resx`, `ActionId.resx`, `StatusHelper.cs`, `ActionTargetInfo.cs`, `ObjectHelper.cs`, `SCH_Reborn.cs`), eine Websuche zur Schildmechanik ohne offizielle Quelle, Selbsttest und Erstlauf des neuen Skripts. Keine Laufzeitbeobachtung, kein Vier-Augen-Prinzip. Reine Dokumentations- und Werkzeugänderung; am Verhalten des Plugins ist nichts geändert.

### A22 · Living Dead als Zeitproblem, Tankbuster-Wirklichkeit, AoE-Schwellwert (07.09.2026)

**Anlass:** Drei Einwände des Auftraggebers aus der Spielpraxis, plus ein Prüfauftrag zur selbstlernenden AoE-Liste mit ausdrücklicher Gegenpositionspflicht („widerlegen oder modifiziert aufnehmen").

**Der schwerste Befund betrifft die eigene Fragestellung.** Alle bisherigen Fassungen von Konzept 09 fragten *liegt der Status* — richtig ist in beiden Living-Dead-Phasen eine Frage nach **Rate und Restzeit**: Phase 1 nützt nur, wenn der Tod vor Ablauf eintritt; Phase 2 nur, wenn die kumulierte Heilung bis zum Ablauf die volle Maximalgesundheit erreicht. Daraus folgen zwei bisher fehlende Verhaltensweisen: der Späteingriff in der Ablaufsekunde von Phase 1, wenn die Hochrechnung kippt (Fall 1c), und die Staffelung in Phase 2 — am Anfang billige Unterstützung, voller Eingriff erst bei nicht tragendem Kurs (Fälle 4, 4a).

**`TankSurvivesWithoutMe` trägt seine Zusage nicht.** Drei Gründe, alle am Artefakt oder an der Begegnungswirklichkeit belegt: `BMRNextTankbusterIn` ist eine einzelne Zahl ohne Angabe des Ziels, bildet also den Zielwechsel zwischen zwei Tanks nicht ab; sie nennt den Beginn einer Sequenz, nicht Trefferzahl oder Gesamtschaden; und manche Buster töten auch einen vollgeheilten und geschildeten Tank, wenn er seine eigene Notfallfähigkeit nicht zündet.

**Gegenposition dazu ausdrücklich geprüft und benannt:** Daraus folgt *nicht*, die Rückhaltung freizugeben. Dass Heilung manchmal nichts nützt, macht Zurückhaltung nicht besser, nur nicht schlechter — und welcher Fall vorliegt, ist vorab unbekannt. Die tragfähige Folgerung ist strenger: **Jedes erkannte Tankbuster-Fenster hebt die Rückhaltung auf, HP-unabhängig**, weil in diesem Fenster nichts beurteilbar ist. Die HP-Schwelle bleibt als zweite, unabhängige Aufhebung.

**Messmittel erhoben, Befund präzisiert.** Die geforderte Hochrechnung ist heute nicht berechenbar, aber der Grund ist ein anderer als „nicht messbar": `RecordedHP` führt ausschließlich Gegner (`TargetUpdater.cs:513-535`), weshalb `GetTTK` auf ein Gruppenmitglied `NaN` liefert; und die Effektpakete fremder Heiler passieren beide Watcher-Filter (`Watcher.cs:17-18`), werden also von niemandem gelesen. **Es fehlt der Aufnehmer, nicht die Quelle.** Zwei Genauigkeitsgrenzen sind benannt statt geglättet: 1 Hz ist für ein Zehn-Sekunden-Fenster zu grob, und ein Gesundheitsdelta ist ein Surrogat für kumulierte Heilung, weil sich Heilung und Schaden im selben Intervall aufheben. Daraus folgt ein eigener Ringpuffer statt Mitbenutzung und Paketauswertung statt Delta.

Als Schutz gegen ein Feigenblatt gilt: `TriggerStillReachable` liefert ohne Messung `false`. **Fehlende Messung darf nie Rückhaltung begründen.**

**Prüfauftrag AoE-Liste: Kern bestätigt, Form widerlegt, modifizierte Aufnahme vorgeschlagen.** Die Lücke ist real — `HashSet<uint>` kennt keine Größenordnung, eine Zwei-Prozent-Fläche löst dieselbe Gruppenmitigation aus wie eine Sechzig-Prozent-Fläche —, und der Betrag liegt an der Lernstelle bereits vor (`Watcher.cs:137` liest `damageEffect.value`, prüft nur `> 0`). Die vorgeschlagene Form scheitert an vier Einwänden: **Zirkelschluss** (gemessen wird der Schaden nach damaliger Mitigation; ein Ausschluss zerstört seine eigene Voraussetzung), **Alterung** eines absoluten Schwellwerts mit Item-Level und Sync, **falscher Maßstab** (`DefenseArea` ist überwiegend prozentuale Minderung, die nicht „verschwendet" wird, und die eigentliche Kostenseite ist der Cooldown), und **Serien** kleiner Einschläge, die eine Einzelwertprüfung nicht sieht. Die modifizierte Form — höchsten beobachteten *Anteil* an der Maximalgesundheit mitspeichern und erst beim Verbrauch gegen eine Nutzerschwelle prüfen, Standard 0 — umgeht alle vier. Ihre Kosten sind ebenfalls benannt: `HostileCastingArea` ist gespeicherte Konfiguration, ein Typwechsel bricht die Datei und verlangt einen Migrationspfad.

**Erreichter Prüfgrad:** statische Prüfung an `DataCenter.cs`, `TargetUpdater.cs`, `ObjectHelper.cs`, `Watcher.cs`, `StateUpdater.cs`, `OtherConfiguration.cs`. Keine Laufzeitbeobachtung, kein Vier-Augen-Prinzip. Die Trefferzahlen der genannten Begegnungen sind nicht nachgeschlagen — sie belegen nur, dass mehrfach einschlagende Buster existieren, was für die Konstruktion genügt. Reine Dokumentation; am Verhalten des Plugins ist nichts geändert.

### A23 · Die Aufhebungsregeln waren für den Hauptfall verkehrt herum (07.09.2026)

**Anlass:** Einwand des Auftraggebers gegen die einen Durchgang zuvor eingeführte Regel, jedes Tankbuster-Fenster hebe die Rückhaltung auf. Für Living Dead ist das falsch — dort will man, dass der Dunkelritter stirbt.

**Der Einwand trifft, und er trifft mehr als die eine Regel.** Beide üblichen Aufhebungen sind für Living Dead Phase 1 kontraproduktiv:

| Aufhebung | Bei A-nichttödlich | Bei Living Dead Phase 1 | Seit wann falsch |
|---|---|---|---|
| Niedrige Gesundheit | richtig — der Tank droht zu sterben | **falsch** — niedrige Gesundheit ist der erwünschte Zustand | drittes Audit |
| Tankbuster-Fenster | richtig — die Sequenz ist nicht beurteilbar | **falsch** — der Buster liefert den Auslöser | viertes Audit |

**Die fehlende Unterscheidung, die beide Fehler erzeugt hat:** Klasse A wurde nie danach getrennt, **ob der Auslöser der Tod selbst ist**. Living Dead ist der einzige Fall, in dem das Ereignis, das ein Heiler sonst um jeden Preis verhindert, das erwünschte ist. The Blackest Night und Excogitation lösen unterhalb des Todes aus; dort gelten die üblichen Regeln unverändert. Die Taxonomie führt deshalb jetzt A-tödlich und A-nichttödlich getrennt, und `MayWithholdForTrigger` zerfällt in zwei Bedingungen.

**Die Gegenhypothese ist hier nicht bloß schwach, sondern nicht formulierbar:** Gesucht war ein Fall, in dem Heilung in Phase 1 den Tank rettet *und* Phase 2 erhält. Den gibt es nicht — eine Heilung, die den Tod verhindert, verhindert per Konstruktion den Auslöser. Der Auftraggeber benennt beide Ausgänge zutreffend: Entweder tötet der Buster den Tank auch vollgeheilt, dann war die Heilung wirkungslos; oder sie rettet ihn, dann hat sie Phase 2 verhindert.

**Zweite Korrektur derselben Prüfung: Der Messbaustein ist nicht Voraussetzung.** Der vierte Durchgang hatte ihn allen Schritten vorangestellt. Die Kernanforderung ist jedoch eine **Uhrregel** über `StatusTime(LivingDead)` — zurückhalten, bis die Restzeit den Vorlauf des Heilmittels unterschreitet — und braucht keine Rate. Damit sinkt der Aufwand von Schritt 3 erheblich, und der Messbaustein rutscht ans Ende der Reihenfolge.

Daraus folgt eine Vorgabe für die Umsetzung: **Vorrang für ein Heilmittel ohne Wirkzeit** (Benediction, Tetragrammaton). Nicht aus Effizienz, sondern weil der Vorlauf den einzigen echten Fehler der Uhrregel bestimmt — sie kann in den letzten Sekunden einen Tod verhindern, der noch rechtzeitig gekommen wäre. Ein knapper Vorlauf hält dieses Fenster klein. Die Fehlerrichtung ist die billigere: Ein verlorener Auslöser kostet einen Cooldown, ein zu spät geretteter Tank den Kampf.

**Erreichter Prüfgrad:** statische Prüfung und Durchspielen der Zeitverläufe; keine Laufzeitbeobachtung, kein Vier-Augen-Prinzip. Reine Dokumentation.

### A24 · Lernzeitpunkt der AoE-Liste und die Frage nach der Potenz (07.09.2026)

**Anlass:** Zwei Rückfragen des Auftraggebers zum AoE-Listen-Vorschlag — wann die Aufnahme erfolgt, und ob statt eines absoluten Betrags die Potenz zu messen wäre.

**Zeitpunkt: bereits richtig, die Sorge ist gegenstandslos.** Gelernt wird nach dem Effekt, nicht bei Cast-Beginn — `Watcher.Enable` (`Watcher.cs:17`) hängt `ActionFromEnemy` an `ActionEffect.ActionEffectEvent`, und dass dort Schadenswerte aus `set.TargetEffects` gelesen werden, ist der Beleg. `Cast100ms > 0` prüft eine Eigenschaft der Aktion, keinen laufenden Wirkvorgang. Die Größenordnung steht beim Lernen also fest.

Die Gegenseite dazu: **Verwendet** wird die Liste bei Cast-Beginn (`IsHostileCastingArea`, `DataCenter.cs:2445`). Aus beidem ergibt sich die tragende Konstruktion — beim ersten Vorkommen wird nie mitigiert, ab dem zweiten schon.

**Potenz: Ziel bestätigt, Größe abgelehnt, ein echter Vorteil des Vorschlags anerkannt.** Das Anliegen — eine gegenüber Level und Item-Level stabile Größe — trifft zu und wird vom Anteil an der Maximalgesundheit erfüllt. Vier Gründe gegen die Potenz: Verfügbarkeit für Gegneraktionen ist **unbelegt** (im gesamten Baum wird keine Potenz gelesen; die 465 Vorkommen in `ActionId.resx` sind Freitext in Beschreibungen von Spieleraktionen); Potenz altert an der Inhaltsachse, wo der Anteil an beiden Achsen stabil ist; Potenz ist eine Eingangsgröße einer Formel, nicht die Gefahrenaussage selbst; und der Vergleichspartner liegt bereits in derselben Einheit vor, weil das Spiel Schilde als Prozent der Maximalgesundheit führt (`ICharacter.ShieldPercentage`, gelesen in `ObjectHelper.cs:3372`).

**Anerkannt und nicht kleingeredet:** Die Potenz wäre **mitigationsfrei** und würde damit den Zirkelschluss vollständig beseitigen, den der beobachtete Betrag mitschleppt. Das ist ein sachlicher Vorteil. Er wird hier nur ersatzweise über die Höchstwertregel aufgefangen — schwächer, aber belegt verfügbar. Findet sich eine auswertbare Potenz für Gegneraktionen, gehört sie als zusätzliches Feld neben den Anteil, nicht an dessen Stelle.

**Unterschieden wurde dabei zwischen Vorhersage und Messung:** Bei Heilzaubern ist die Potenz die Rechengröße, weil der Ausgabewert vor dem Wirken zu bestimmen ist. Hier ist er bereits eingetreten und beobachtet — wer das Ergebnis hat, braucht die Formel nicht.

**Erreichter Prüfgrad:** statische Prüfung an `Watcher.cs`, `DataCenter.cs`, `ObjectHelper.cs`, `ActionId.resx` und dem gesamten `.cs`-Baum auf Potenzfelder. Die Frage, ob das Datenblatt des Spiels für Gegneraktionen eine numerische Potenz führt, ist aus diesem Repository nicht zu klären und ausdrücklich als unbelegt geführt. Reine Dokumentation.

### A25 · Nachprüfung der Thin-Air-Schwelle: 6000 oder 2400 (07.09.2026)

**Anlass:** Rückfrage des Auftraggebers, welche Grenze in A18 gewählt wurde — die Klartraum-Schwelle oder die für eine Wiederbelebung nötigen 2400 MP — und warum.

**Was tatsächlich im Code steht.** `WHM_Reborn.cs:148-151` prüft `CurrentMp < Service.Config.LucidDreamingMpThreshold`, also **6000** (`Configs.cs:1311`, `[JobConfig]`, je Job einstellbar). Nicht 2400. Die zweite Schwelle der Bedingung ist `ThinAirNeed` (Standard 1000, `WHM_Reborn.cs:71`) und betrifft die Kosten des nächsten Zaubers, nicht den MP-Stand.

**Die damalige Begründung war Konsistenz, nicht Bedarf.** A18 hielt fest: derselbe Wert, an dem `ModifyLucidDreamingPvE` (`CustomRotation_Actions.cs:45`) entscheidet, ob Klartraum fällt — eine Wahrheit statt zweier, die auseinanderlaufen können. Das ist eine gültige, aber schwache Begründung: Sie sagt, warum keine *zweite* Zahl, nicht warum *diese* Zahl.

**Nachgeprüfte sachliche Begründung.** Sie trägt stärker als die formale:

*Erstens beantworten die beiden Zahlen verschiedene Fragen.* 6000 ist eine **Auslöseschwelle** — ab wann zählt eine Ersparnis überhaupt. 2400 wäre eine **Reserve** — was nie unterschritten werden darf. Eine Reserve ist keine Auslöseschwelle, und beide an dieselbe Stelle zu setzen verwechselt Vorbeugung mit Notbremse.

*Zweitens wäre 2400 als Auslöseschwelle zu spät.* Thin Air hat zwei Ladungen mit 60 s Erholung und spart je die vollen Kosten eines Zaubers. Von 6000 auf 2400 sind 3600 MP — bei Heilzaubern um 1500 MP knapp zweieinhalb Zauber. Zwei Thin-Air-Ladungen decken davon rund 3000 MP. **Die Kapazität der Fähigkeit überbrückt also gerade die Strecke von der Auslöseschwelle bis zur Wiederbelebungsgrenze.** Erst bei 2400 zu beginnen hieße, mit dem Bremsen anzufangen, wenn die Strecke schon verbraucht ist.

*Drittens ist es sachlich derselbe Zustand.* Klartraum und Thin Air beantworten dieselbe Lage — MP-Druck — mit verschiedenen Mitteln. Die Bedingung verlangt ausdrücklich, dass Klartraum *nicht* kann (nicht gelernt, abgeschaltet oder in Erholung); Thin Air übernimmt dann dessen Rolle. Dass es dabei dieselbe Schwelle liest, ist nicht Bequemlichkeit, sondern die richtige Modellierung.

**Gegenposition geprüft: Wäre eine 2400-MP-Reserve zusätzlich sinnvoll?** Erhebung: Im gesamten Baum existiert **keine** MP-Reserve für Wiederbelebung — weder `ReserveMp` noch ein gleichwertiges Konstrukt. Gegen ihre Einführung spricht die vom Auftraggeber selbst vorgegebene Rangordnung: Eine Reserve hielte Heilung zurück, um eine spätere Wiederbelebung zu ermöglichen. Heilung verhindert den Tod, Wiederbelebung repariert ihn — die Reserve stellte damit eine nachrangige Wirkung über das Überleben. **Sie wäre nach der Rangordnung falsch.**

**Die Wiederbelebung ist zudem bereits anders abgesichert, und zwar wirksamer.** `ThinAirLastChargeUsage = ReserveLastChargeForRaise` (`WHM_Reborn.cs:155`) hält die letzte Ladung zurück, und der Wiederbelebungszweig der Bedingung (`:156`) ist von `ThinAirOnMpPressureOnly` und `UnderMpPressure` **unabhängig** — er greift immer. Mit Thin Air kostet die Wiederbelebung null MP; die 2400 werden dann gar nicht gebraucht. Die Reserve schützt die richtige Ressource, nämlich die Ladung, nicht das MP.

**Restlücke, ehrlich benannt:** Sind Thin Air in Erholung **und** MP unter den Wiederbelebungskosten, fällt die Wiederbelebung still aus — `ActionBasicInfo.cs:632` prüft `CurrentMp >= MPNeed` korrekt, meldet aber nichts. Das ist kein Defekt, aber eine unbeantwortete Lage. Sie zu schließen hieße, eine Reserve einzuführen, und damit die Rangordnung zu verletzen; sie bleibt deshalb offen und wird hier nur festgehalten.

**Nicht belegt:** Der Auftraggeber nennt 2400 MP als Kosten der Wiederbelebung. Im Code sind Aktionskosten nicht statisch hinterlegt — `ActionBasicInfo.MPNeed` (`:274-284`) liest sie zur Laufzeit über `ActionManager.GetActionCost`. Die Zahl ist hier also als Angabe des Auftraggebers übernommen, nicht am Artefakt geprüft. Für den Schluss ist das unerheblich: Die Argumentation hängt an der Größenordnung, nicht am exakten Wert.

**Ergebnis: keine Änderung.** Die Wahl bleibt bei `LucidDreamingMpThreshold`, nun mit einer Begründung aus der Sache statt allein aus der Konsistenz. **Erreichter Prüfgrad:** statische Prüfung an `WHM_Reborn.cs`, `Configs.cs`, `CustomRotation_Actions.cs`, `ActionBasicInfo.cs` sowie eine Volltextsuche nach MP-Reserven im gesamten Baum. Keine Laufzeitbeobachtung.

### A26 · Rückrechnung statt Vorhersage — eine zu absolute eigene Aussage korrigiert (07.09.2026)

**Anlass:** Einwand des Auftraggebers gegen den Schluss aus A24, die Potenz sei hier ein Umweg, weil das Ergebnis bereits beobachtet vorliege. Sein Gegenargument: Der Rechenweg lässt sich umkehren — wer den absoluten Wert hat, kann die Störfaktoren herausrechnen.

**Der Einwand trifft, und die frühere Formulierung war zu absolut.** „Eine Formel braucht nicht, wer das Ergebnis hat" gilt für die reine Beobachtung, übersieht aber, dass eine Beobachtung um bekannte Störfaktoren **bereinigt** werden kann. Das führt zu einer besseren Größe als der bloß beobachtete Anteil und behebt den Zirkelschluss, den A24 noch der Höchstwertregel überlassen hatte.

**Das Werkzeug ist bereits vorhanden**, was in A24 übersehen wurde: `CustomRotation_OtherInfo.GetCurrentMitigationPercent()` (`:585`) rechnet die wirkenden Minderungen multiplikativ zu einem Schadensfaktor zusammen, mit getrennter Skalierung nach physisch und magisch. Im Effekt-Handler aufgerufen — also zum Trefferzeitpunkt — liefert sie den Divisor: `Rohanteil = (value / MaxHp) / Schadensfaktor`.

**Die Rückrechnung endet einen Schritt vor der Potenz, und das ist keine Ausrede.** Bereinigen lässt sich um alles, was der Client kennt. Nicht bekannt sind die Angriffswerte des Gegners und die Verteidigungswerte des Getroffenen; ohne sie führt kein Weg vom ungeminderten Schaden zur Potenz der Aktion. Gebraucht wird sie dort aber auch nicht: Das Ziel — eine minderungsfreie, nicht alternde Größe — ist beim ungeminderten Anteil erreicht, und der letzte Schritt würde gerade die Bezugsgröße entfernen, auf die es ankommt.

**Vier Grenzen der Rückrechnung, benannt statt übergangen:** Die Funktion ist eine **Aufzählung** bekannter Status und altert nach demselben *Lack of Movement*-Muster wie die Statuslisten; sie ist parameterlos und gruppenbezogen, nicht zielbezogen; Verwundbarkeitsstapel wirken in die Gegenrichtung und fehlen; und der Deckel bei 0,95 verfälscht bei extremer Stapelung. Der Restfehler rechtfertigt die Höchstwertregel weiterhin — aber als Absicherung, nicht als Ersatz für die Bereinigung.

**Erreichter Prüfgrad:** statische Prüfung an `CustomRotation_OtherInfo.cs` und `Watcher.cs`. Reine Dokumentation.

### A27 · Umsetzung der hindernisfreien Punkte, Planung des Rests (07.09.2026)

**Anlass:** Auftrag, die offenen Punkte im Loop zur Umsetzung zu planen, das Hindernisfreie umzusetzen und dort, wo Hindernisse bestehen, Konzept und Planung anzupassen.

**Umgesetzt, nach Risiko geordnet.**

| Commit | Eingriff | Wirkungsbereich |
|---|---|---|
| `36983734` | Selbsttests für `scan.py`, `mitscan.py`, `scan2.py` | kein Produktivcode |
| `26e507cd` | Excogitation-Invertierung (`SCH_Reborn.cs`) | eine Rotation, eine Fähigkeit |
| `a443c380` | `IsConditionCannotTarget`: siebenmal `return null` → `continue` | vier Zielsuchen, darunter `FindTankTarget` |
| `f1e8844a` | Heilzielwahl: Invertierung behoben, Filter → Prioritätsstufe | zentrale Heilzielwahl aller Jobs |

**Der Selbsttest-Nachtrag fand sofort einen eigenen Erkennungsfehler.** `scan.py`-Prüfung (f) — doppelte aufeinanderfolgende `if`-Bedingungen — konnte nie anschlagen: Sie suchte `'out act'` in einer Zeichenkette, aus der zuvor `re.sub(r'\s+', '', …)` sämtliche Leerzeichen entfernt hatte. Der Nullbefund dieser Klasse war so lange wertlos, wie es die Prüfung gab. Nach der Korrektur greift sie, und der Baum ist an dieser Stelle tatsächlich sauber. Das ist der fünfte Erkennungsfehler, den ein Selbsttest in diesem Repository aufgedeckt hat.

**Ein Befund entstand erst bei der Umsetzung.** Der Umbau der Zielwahl verlangte eine Unterscheidung, die `NoNeedHealingStatus` nicht macht: `Mounted` nullifiziert Heilungsempfang laut eigener Beschreibung vollständig, ist also ein **Ausschluss**; eine echte Unverwundbarkeit verschiebt den Bedarf nur und ist eine **Herabstufung**. `HpRecoveryDown` mindert Heilung, hebt sie nicht auf, und gehört nach der Rangordnung ebenfalls nicht in den Ausschluss. Dafür ist `StatusHelper.HealingIneffectiveStatus` entstanden; `NoNeedHealingStatus` blieb unangetastet, weil `StateUpdater` und `ObjectHelper` es mit der älteren Bedeutung lesen und es zur Paketoberfläche gehört.

**Drei Hindernisse bei Schritt 3, alle mit derselben Ursache: die falsche Schicht.**

| # | Hindernis | Folge |
|---|---|---|
| H1 | Der Vorlauf der Uhrregel hängt am Heilmittel — letzte Sekunde bei einer Fähigkeit ohne Wirkzeit, ein bis zwei GCDs bei einem Zauber. `GeneralHealTarget` kennt die Rotation nicht | Die Uhrregel gehört in die Rotation, nicht in die zentrale Zielwahl |
| H2 | „Kapazitätsprüfung einmalig vor Eintritt" verlangt, den Phasenbeginn festzuhalten. `ActionTargetInfo` ist dort zustandslos | Der Zustand gehört zum Messbaustein oder in die Rotation |
| H3 | Eine Rückhaltung in `ActionTargetInfo` liefe über `Service.Config`, also die globale Konfiguration — sachlich ist sie eine Rotationsentscheidung | Die Option gehört zu den `[RotationConfig]`-Einträgen der Heilerrotation |

**Das Konzept hatte Schritt 3 stillschweigend zentral verortet**, weil Schritt 2 dort liegt. Das war ein Fehler der Zuordnung, nicht der Regel; das Konzept trägt die Korrektur jetzt als eigenen Abschnitt.

**Zweiter Befund derselben Planung:** Die Herabstufung wirkt heute nur, wenn ein *anderes* Gruppenmitglied das Heilflag bereits ausgelöst hat. `StateUpdater.ShouldHealSingle` liest dieselbe Prüfung weiterhin als harten Ausschluss und liefert einen Wahrheitswert, in dem eine Rangfolge begrifflich nicht unterzubringen ist. Ist der geschützte Tank der einzige Verletzte, wird also weiterhin gar nicht geheilt. Daran hängt auch, warum die Ergänzung von `HallowedGround`, `HallowedGround_1302` und `UndeadRebirth` weiterhin ausgesetzt bleibt: Heute ergänzt, entstünde für Paladine unter Hallowed Ground genau der Defekt neu, den der TODO-Eintrag beschreibt.

**Erreichter Prüfgrad:** statische Selbstprüfung, Selbsttests und Erstlauf der Skripte, Klammerbilanz und Schleifenkontext der sieben `continue`-Stellen von Hand geprüft, CI-Kompilierung. Keine Laufzeitbeobachtung und kein Vier-Augen-Prinzip. Dass die neue Rangfolge im Spiel besser spielt, ist damit **nicht** belegt — belegt ist nur, dass die alte den am schwersten Verletzten nicht erreichen konnte.

### A28 · Audit und Code-Review der Umsetzung aus A27 (07.09.2026)

**Anlass:** Auftrag, die Umsetzung kritisch zu prüfen, mit Freigabe für Korrekturen.

**Erreichter Prüfgrad, ehrlich benannt:** Selbstprüfung **plus** ein Code-Review mit eigenem Kontext über den Bereich `c08bccfe..HEAD`. Das ist mehr als das erneute Lesen des eigenen Diffs, aber es bleibt maschinelle Prüfung ohne menschliches Vier-Augen-Prinzip und ohne Laufzeitbeobachtung.

**Selbstprüfung — ein Befund, eine Entlastung.**

Der Befund: Der neue Sortier-Komparator wertete `NoNeedHealingInvuln()` und `GetHealthRatio()` bei **jedem** Vergleich aus, also O(n log n) statt O(n) mal, jeweils mit Statuslisten-Durchlauf, im Kampfpfad. Beide Schlüssel werden jetzt einmal je Mitglied gelesen (`8e669e05`). Nebenwirkung derselben Korrektur: Die Schlüssel liegen für die Dauer der Sortierung fest — `NoNeedHealingInvuln` vergleicht eine laufende Reststatuszeit gegen ein GCD-Fenster, und ein genau auf dieser Grenze liegender Status hätte zwischen zwei Vergleichen kippen können, was `List.Sort` bei inkonsistentem Komparator werfen lässt. Dieses Fenster ist schmal und wurde **nicht beobachtet**; tragend ist das Laufzeitargument.

Die Entlastung: Geprüft wurde, ob die Aufhebung des Filters Tote in die Kandidatenliste bringt — ein Toter hätte Gesundheitsanteil 0 und stünde ganz vorn. Er tut es nicht: `GetCanTargets` entfernt ihn eine Ebene höher, wo `CanUseTo` das Spiel selbst über `ActionManager.CanUseActionOnTarget` fragt. Vollständig Geheilte fallen an derselben Stelle heraus.

**Code-Review — vier Befunde, drei davon echte Regressionen dieser Runde.**

| # | Fundstelle | Befund |
|---|---|---|
| 1 | `ActionTargetInfo.cs` Rollenstufen | **Die Herabstufung wurde umgangen.** Die Heiler- und Tank-Abkürzungen liefern ihren Kandidaten sofort zurück, sobald er unter `HealthHealerRatio`/`HealthTankRatio` fällt — sie laufen also *vor* der Sortierung. Mit geschützten Mitgliedern gefüttert hoben sie die Herabstufung genau im Zielfall auf: Ein Superbolide-Tank bei 1 HP unterschreitet die Tankschwelle sofort und gewann gegen ein ungeschütztes Mitglied bei 20 % |
| 2 | `StatusHelper.cs` | `Mounted_1520` stand ohne Beleg in `HealingIneffectiveStatus`. Status 1420 sagt „HP recovery … nullified", 1520 ist „Riding atop a Rathalos" und sagt nichts über Heilung. Ein verletztes Mitglied damit wäre ganz aus der Heilzielwahl gefallen |
| 3 | `SCH_Reborn.cs` | Fehlender `IsDead`-Schutz. Eine Leiche hat Anteil 0 und, mit gelöschten Status, keinen Schutzstatus — erfüllt nach dem Entfernen der Negation also beide Hälften und verbraucht Recitation für eine Excogitation, die nicht landen kann |
| 4 | `ActionTargetInfo.cs` Selbstabkürzung | Umgeht die Kandidatenliste und damit den neuen Filter: zielte weiterhin auf einen Spieler unter `Mounted` |

Alle vier behoben (`2b03843e`). Befund 1 ist der schwerste — die Änderung erreichte ohne ihn den Fall nicht, für den sie geschrieben wurde, und der Kommentar im Code behauptete das Gegenteil.

**Befund 2 ist ein wiederholter eigener Fehler.** Drei Commits zuvor steht als C15 im Archiv, dass ein Nullbefund über den bloßen Bezeichner kein Beleg ist — hergeleitet an genau den Holmgang-Ids. Danach wurde `Mounted_1520` allein wegen des gleichen Namens aufgenommen. Die Regel war notiert und wurde beim nächsten Anlass nicht angewandt; die Lehre daraus betrifft nicht die Regel, sondern den Zeitpunkt ihrer Anwendung: Sie gehört an die Stelle, an der eine Statusliste **geschrieben** wird, nicht nur an die, an der sie geprüft wird.

### A29 · Einzelabarbeitung: StateUpdater, Living Dead, Messbaustein (07./08.09.2026)

**Anlass:** Auftrag, die verbliebenen Punkte einzeln im vollständigen Loop zu prüfen, umzusetzen, zu auditieren und einem Code-Review zu unterziehen.

**Vorgehen je Punkt:** Research am Artefakt → Optionen mit Nullvariante → Abwägung → Falsifikation → Umsetzung → Selbstprüfung → Code-Review mit eigenem Kontext → Korrekturen. Drei Punkte, zusammen **sechs Commits** und **dreizehn Review-Befunde**, davon zehn Regressionen oder Fehler dieser Runde.

#### Punkt 1 — `StateUpdater`-Umstellung (`7e2ba594`, `4b15d27f`)

Ein Schutzstatus senkt die Heilschwelle auf `HealthProtectedRatio`, statt das Flag zu unterdrücken. Damit erreicht die Zielwahl-Herabstufung aus A27 überhaupt erst ihren Zweck: Ohne Flag ruft der Dispatcher `HealSingleGCD` nicht auf.

| Review-Befund | Kern |
|---|---|
| `HealthForDyingTanks` war der falsche Regler | `[JobConfig]`, über `DataCenter.Job` aufgelöst und mit `PvEFilter = Tank` auf einem Heiler nicht editierbar. Der Kommentar behauptete, es sei dieselbe Zahl wie in den Tank-Rotationen — sie ist es nicht |
| Fehlende Klemmung | `Range(0,1)` erlaubt eine geschützte Schwelle über der normalen; `Math.Min` sichert die Invariante |
| `Mounted` in der Schwellensenkung | Nullifiziert Heilung — Ausschluss, nicht Herabstufung |
| `HpRecoveryDown` desgleichen | Mindert Heilung nur; das ist ein Grund, **härter** zu heilen. In der eigenen Fallmatrix zuvor als „gewollte Lockerung" fehlbewertet |
| `NoNeedHealingStatus` vermengte drei Kategorien | Beide Nicht-Invulnerabilitäten entfernt; der einzige `HasTankInvuln`-Verwender fragt „already invuln'd" |
| Doku-Attribut ohne Präzedenz | Hätte die Einstellung von den beiden getrennt, mit denen sie über `Math.Min` zusammenwirkt |

#### Punkt 2 — Living Dead (`d54b47c0`, `2e215715`)

**Der Research widerlegte die eigene Umsetzung aus Punkt 1**: Die Schwellensenkung hätte Phase 1 bei zehn Prozent geheilt — genau der vom Auftraggeber als sinnlos benannte Fall. `DeathTriggeredStatus` trennt den einen Status, dessen Auslöser der eigene Tod ist, und behält für ihn den vollen Halt.

**Zweiter Research-Befund: Die Uhrregel existierte bereits.** `WillStatusEndGCD` meldet einen Status vor seinem Ablauf als endend. Konzept 09 hatte diese Freigabe als mildernden Nebenumstand eines anderen Mechanismus behandelt und angenommen, die Uhrregel müsse erst gebaut werden.

| Review-Befund | Kern |
|---|---|
| **Uhr auf der falschen Liste** | `StatusTime` liefert das *früheste* Ablaufen über die **ganze** `NoNeedHealingStatus`. Eine fremde kurze Unverwundbarkeit hätte das Fenster als endend gemeldet — und dasselbe Prädikat wählte dann die Schwelle, sodass das Ziel **bereitwilliger** geheilt worden wäre als ein ungeschütztes |
| **Der Tod ist nicht immer gewollt** | RSR zündet Living Dead selbst als Notrettung bei `HealthForDyingTanks`. Dort ist der Tod die Katastrophe, und Walking Dead verlangt danach eine volle Maximalgesundheit an Heilung in zehn Sekunden. Der Halt liegt seither hinter `WithholdHealingForLivingDead`, Standard aus — was das Konzept vorgesehen und die Umsetzung fallen gelassen hatte |
| Halt fehlte in der Zielwahl | Die Selbstabkürzung hätte den Träger aus dem Auslöser heraus geheilt, sobald ein anderer das Flag setzt |

#### Punkt 3 — Messbaustein: Nullvariante gewählt (`ff46f0a8`, `cea66c79`)

**Ergebnis: nicht gebaut, und das ist der Befund.** Er hatte genau einen vorgesehenen Verbraucher — die Hochrechnung, ob der Tod rechtzeitig eintritt —, und die ist durch `InDeathTriggerWindow` anders beantwortet. Kein Verbraucher im Baum, aber Laufzeitkosten bei allen Nutzern in jedem Kampf: Ein Baustein ohne Verbraucher ist Vorratsarbeit.

**Ein eigener Fehler im selben Commit, vom Review gefangen:** Der Restfehler der Uhrregel sollte durch einen kürzeren Vorlauf gesenkt werden — ein GCD statt zwei. Das war falsch, weil der Vorlauf bis zur **Entscheidung** misst, nicht bis zum Landen der Heilung: Im ungünstigsten Fall muss der laufende GCD auslaufen und ein Zauber darauf fertig werden, zusammen zwei GCDs. Ein GCD lässt die Heilung nach Ablauf landen, wenn der Tank bereits ungeschützt ist. Zurückgenommen; die halbe Phase ist der Preis dafür, dass die Heilung überhaupt ankommt.

Mit der Umstellung wurde außerdem möglich, was drei Runden lang zurückgestellt war: `HallowedGround`, `HallowedGround_1302` und `UndeadRebirth` sind ergänzt.

**Erreichter Prüfgrad:** statische Selbstprüfung, Fallmatrizen je Änderung, drei Code-Reviews mit eigenem Kontext, CI-Kompilierung. Keine Laufzeitbeobachtung, kein menschliches Vier-Augen-Prinzip. Dass die neuen Schwellen im Spiel besser spielen, ist **nicht** belegt; belegt ist, dass die alte Konstruktion einen geschützten Tank als einzigen Verletzten gar nicht erreichte.


### A30 · Einzelabarbeitung: argumentlose `params`-Prädikate (08.09.2026)

**Anlass:** Fortsetzung des Auftrags, jeden verbliebenen Punkt einzeln im vollständigen Loop zu prüfen, umzusetzen, zu auditieren und einem Code-Review zu unterziehen. Punkt: die fünf Fundstellen `IsLastAction() == IsLastGCD()` beziehungsweise `== IsLastAbility()`.

**Research am Artefakt.** `IsLastAction`, `IsLastGCD` und `IsLastAbility` sind `params ActionID[]`-Überladungen; ohne Argument durchsucht `IsActionID` eine leere Liste und liefert `false`. Der Vergleich `false == false` ist konstant wahr. Die gemeinte Prüfung liegt als `IActionHelper.IsLastActionGCD()` vor und wird in `CustomRotation_Ability.cs:688`, `697`, `705` korrekt verwendet.

**Entstehungsrichtung.** `git log -S` weist `HasWeaved()`, `CanEarlyWeave` **und** `IActionHelper.IsLastActionAbility()` demselben Commit zu: `0246bea5` (25.05.2026). Das passende Mittel lag also im selben Commit und wurde nicht benutzt — **Ignorant Surgery** nach Parnas, nicht Lack of Movement. Kein späterer Eingriff hat eine Prämisse gebrochen; die Stelle war von Anfang an inkonsistent zur eigenen Entwurfsabsicht, die der Kommentar darüber wörtlich ausschreibt.

**Umgesetzt.** Vier Rotationsstellen auf `IActionHelper.IsLastActionGCD()` umgestellt (`DRG_Reborn.cs:131`, `WHM_Reborn.cs:156` zweimal, `BeirutaWHM.cs:401-402` zweimal); dort ist der Ausdruck selbst der Befund, und die Behebung tut, was die Zeile sagt. `HasWeaved()` auf `IActionHelper.IsLastActionAbility()` umgestellt, mit einem Zusatz, den die Kausalitätsprüfung erbrachte: `ResetAllRecords` setzt `LastAction`, `LastGCD` und `LastAbility` gemeinsam auf `None`, weshalb die nackte Gleichheit auch dann hält, wenn überhaupt nichts benutzt wurde. Der Nullwert-Ausschluss steht in `HasWeaved()` statt im Helferpaar, weil dessen Symmetrie und dessen fünf bestehende Verwender sonst mit betroffen wären.

**Der Code-Review widerlegte die naheliegende Vervollständigung.** `CanEarlyWeave => (!HasWeaved() || WeaponRemain > LateWeaveWindow) && CanWeave` sieht nach der Behebung von `HasWeaved()` wie die zweite Hälfte derselben Reparatur aus. Die Wirkungsanalyse zeigt das Gegenteil: Die Disjunktion **weitet** `CanEarlyWeave` ins späte Fenster aus, sobald noch nichts geweavt wurde, und `ChurinMNK.TryUseRiddleOfFire` (`:804`) liest `CanEarlyWeave` ausschließend — `if (!IsEnabled || CanEarlyWeave || !CanLateWeave) return false;`. Solange `HasWeaved()` konstant wahr war, waren früh und spät exakte Komplemente und die Zeile bedeutete „nur im späten Slot"; mit geweckter Disjunktion fiele Riddle of Fire im Einzel-Weave-Fall vollständig aus. `ChurinBRD` liest dieselbe Eigenschaft dreimal einschließend, einmal als benutzergewählte Zeitpunktoption `WandererWeave.Early`, deren Bedeutung die Ausweitung verwischt.

`CanEarlyWeave` steht deshalb jetzt auf `WeaponRemain > LateWeaveWindow && CanWeave` — dem Verhalten, gegen das jeder Verbraucher geschrieben wurde —, und die Frage Disjunktion oder Konjunktion ist als technische Schuld erfasst. Das ist die Regel für Verhaltensänderungen ohne Nachweismöglichkeit, angewandt auf den Fall, in dem sie unbequem ist: Die Behebung sah vollständiger aus als das, was sich belegen ließ.

**Prüfmittel.** `scan9.py` erfasst die Defektklasse: bool-Prädikate mit `params`-Feld, die mit leerer Argumentliste gerufen werden, ausgenommen Namen mit echter parameterloser Überladung, auf die C# stattdessen bindet. Gegen den Vor-Zustand des Baums gelaufen meldet es beide Hälften der `HasWeaved`-Zeile, gegen den Nach-Zustand nichts, bei 18 `params`-Prädikaten im Suchraum — der Nullbefund ist damit von einem stillen Fehlschlag unterscheidbar. Repo-weit gibt es keine weitere Fundstelle: Einzelfall in der Ausprägung, Defektklasse in der Wiederholbarkeit, und das Skript adressiert die zweite.

**Erreichter Prüfgrad:** statische Selbstprüfung, Prüfskript mit Selbsttest und Gegenprobe am konstruierten Defekt, Code-Review mit eigenem Kontext, CI-Kompilierung. Keine Laufzeitbeobachtung. Dass die vier Rotationsstellen im Spiel besser weaven, ist **nicht** belegt; belegt ist, dass die Einschränkung, die sie ausschreiben, bisher nicht existierte.


### A31 · Einzelabarbeitung: der falsche Holmgang-Status in den Beiruta-Rotationen (08.09.2026)

**Research am Artefakt.** `Status.resx` führt vier Statuseinträge unter dem Anzeigenamen Holmgang: 88 „Unable to move until effect fades" ↓, 409 „Most attacks cannot reduce your HP to less than 1" ↑, sowie 1304 und 1305, beide ↓. Der Schutz auf dem Krieger ist 409; 88 ist der Bewegungs-Debuff auf dessen Ziel. Die zentralen Listen in `StatusHelper` (`:440`, `:579`) und `WAR_Reborn` führen durchgehend 409 — der Fork enthält den Beleg für die richtige Zuordnung also selbst.

**Gesamtheitlichkeit: Der TODO-Eintrag war unvollständig.** Er nannte vier Fundstellen; es sind fünf. `BeirutaSGE.cs:193` trägt dieselbe Prüfung und fehlte in der Aufzählung — der Nachweis, dass die Erhebung „alle strukturell gleichen Stellen" beim ersten Mal nicht vollständig war.

**Inhaltlichkeit förderte den schwereren Defekt zutage.** Drei der fünf Prüfungen sperren die Einzelheilung außer bei Living Dead und Holmgang auch bei **Walking Dead**. Deren Wirkbeschreibung (`Status.resx` 811) lautet: „Most attacks will not reduce HP below 1. Restoring HP with each weaponskill successfully delivered and spell cast. The inability to restore 100% of HP before timer runs out will result in KO." Heilung ist dort die Überlebensbedingung, nicht die Höflichkeit; eine Heilsperre tötet den Dunkelritter. Beim Weißmagier trifft sie unter anderem Benediktion — die Vollheilung, die genau diese Phase beantwortet. `StatusHelper.NoNeedHealingStatus` hat `WalkingDead` aus demselben Grund ausdrücklich auskommentiert.

**Entstehungsrichtung.** Alle fünf Stellen stammen aus dem Upstream-Merge `0246bea5` und stehen in `upstream/main` unverändert. Kein späterer Eingriff hat eine Prämisse gebrochen; die Zuordnung war von Anfang an falsch. Betroffen sind Endnutzer der Beiruta-Rotationen und die Upstream-Pflege.

**Umgesetzt.** Statt die Aufzählungen zu flicken, verweisen alle fünf auf `StatusHelper.NoNeedHealingStatus`. Das ist die Parnas-Antwort auf eine alternde Konstruktion: die Aufzählung wird durch die gepflegte Liste ersetzt, sonst fallen dieselben Stellen nach der nächsten Ergänzung erneut zurück. Nebeneffekt und Beleg zugleich: Paladin und Gunbreaker fehlten in vier der fünf Listen vollständig — die fünfte, `BeirutaSCH.HasDefenseSingleLockoutStatus`, zählte sie auf und belegt damit, dass der Autor sie derselben Klasse zurechnet.

**Prüfmittel.** `scan10.py` erfasst die Defektklasse dahinter: Statusbezeichner, deren Anzeigename von einem Status entgegengesetzter Polarität geteilt wird. Bei 4489 Mitgliedern liegen 111 in solchen Gruppen, 12 Bezeichner werden an 26 Stellen benutzt — eine von Hand prüfbare Liste. Gegen den Vor-Zustand gelaufen meldet das Skript alle fünf Holmgang-88-Stellen einschließlich der übersehenen SGE-Stelle, gegen den Nach-Zustand keine. Dieselbe Falle hat dieses Projekt einmal von der anderen Seite erwischt: Nullbefund über den Bezeichner `HallowedGround` ohne Prüfung der Wirkbeschreibung (C15).

**Ein zweiter Defekt aus derselben Liste, außerhalb dieses Punktes.** `RedMageRotation.cs:756` und `:763` setzen `StatusProvide` — den Status auf dem **Spieler** — auf 3238 und 3239, die Schadenswirkungen auf dem **Ziel**; die Barrieren auf dem Spieler sind 3235 und 3236. Die Nachbarzeile `:749` greift richtig zu, und dieselbe Datei trennt `StatusProvide` von `TargetStatusProvide` überall sonst konsistent. Erfasst in `TODO.md`, Behebung im nächsten Einzelzyklus.

**Erreichter Prüfgrad:** statische Selbstprüfung, Wirkbeschreibungen als Quelle, Prüfskript mit Selbsttest und Gegenprobe am konstruierten Defekt, Code-Review mit eigenem Kontext, CI-Kompilierung. Keine Laufzeitbeobachtung; der Auftraggeber nutzt die Beiruta-Rotationen nach eigener Angabe nicht. Belegt ist, dass die Holmgang-Sperre nie gegriffen hat und die Walking-Dead-Sperre der Wirkbeschreibung des Status widerspricht.


### A32 · Einzelabarbeitung: Status-Einstellungen auf der falschen Seite (08.09.2026)

**Anlass:** Der in A31 nebenbei erfasste RDM-PvP-Fund, im Einzelzyklus aufgenommen. Er blieb keine Einzelstelle.

**Research am Artefakt.** `ActionBasicInfo.IsStatusProvided` und `IsStatusNeeded` lesen `StatusProvide` beziehungsweise `StatusNeed` gegen `Player.Object`; `ActionTargetInfo.CheckStatus` liest die beiden `Target*`-Felder gegen das Ziel. Eine Status-ID, die der Spieler nie tragen kann, macht die erste Sperre wirkungslos und die zweite dauerhaft blockierend — beides ohne Compilerfehler und ohne beobachtbaren Fehlschlag.

**Erhebung statt Einzelfall.** `scan11.py` prüft die vier Felder gegen die ↑/↓-Markierung, die der Statusgenerator in jeden Doku-Block schreibt. Sechzehn Einstellungen kamen zurück. Der Wert der Liste lag darin, wie viele davon **keine** Defekte sind: der Dunkelritter trägt `WalkingDead` tatsächlich selbst, Lost Manawall legt `Heavy` auf den Spieler, `SprintPenalty` und `Bind` sind bewusste Sperren gegen sinnlose Nutzung, und Eerie Soundwave nennt in `TargetStatusNeed` die Buffs, die es raubt. Die Regel kann das nicht trennen; ein Leser kann es.

**Der schwerste Fund war nicht der gesuchte.** `ModifyPeripheralSynthesisPvE` führte `Lightheaded_2501` in **beiden** spielerseitigen Feldern. Die `StatusNeed`-Hälfte blockierte die Aktion vollständig — `IsStatusNeeded` meldet true, solange der Status fehlt, und der Spieler trägt diesen Ziel-Debuff nie. `BLU_Reborn` rief die Aktion an zwei Stellen mit **je einem anderen** Skip-Flag: `skipStatusNeed` in `UseFiller`, `skipStatusProvideCheck` in `SpendCooldowns`. Nur das erste löst die Blockade; die Zeile in `SpendCooldowns` konnte nie feuern. Zwei Umgehungen derselben Einstellung, eine davon geraten — die Ignorant-Surgery-Signatur auf der Verbraucherseite.

**Die Falsifikation widerlegte die naheliegende Behebung.** „Einstellung auf die passende Seite verschieben" schien für alle Fundstellen die Antwort. Für beide Blaumagier-Stellen ist sie falsch, und das ließ sich nur außerhalb des Baums klären: Magic Hammer ist ein 250-Potenz-Flächenzauber, der zusätzlich 1000 MP zurückgibt und in `UseFiller` steht — `Conked` ist sein Nebeneffekt, nicht sein Zweck. Peripheral Synthesis steigt von 220 auf 400 Potenz, **solange** `Lightheaded` auf dem Ziel liegt. Eine Ziel-Sperre hätte beide Zauber genau dann unterdrückt, wenn sie am meisten wert sind. Beide Einstellungen sind deshalb entfernt statt verschoben, und die zwei Skip-Flags in `BLU_Reborn` damit gegenstandslos.

Das ist der Beleg für die Regel, verfügbare Quellen auszuschöpfen, bevor eine Grenze behauptet wird — hier in der anderen Richtung: bevor eine Behebung als offensichtlich behandelt wird. Zwei Websuchen haben zwei Verschlechterungen verhindert.

**Nicht umgesetzt, mit Begründung.** Vier Fundstellen bleiben offen, weil ihre Auflösung PvP- oder Bozja-Verhalten voraussetzt: RDM-PvP (zwei Zeilen), SCH-PvP Deployment Tactics, Bozja Lost Paralyze III. Die fünfte, Bozja Lost Excellence mit `StatusNeed = [Weakness]`, ist kein Seitenfehler — der Spieler kann `Weakness` tragen, die Prüfung wirkt also; fragwürdig ist ihr fachlicher Sinn. Alle in `TODO.md`.

**Erreichter Prüfgrad:** statische Selbstprüfung, Prüfskript mit Selbsttest, Wirkbeschreibungen und zwei externe Quellen als Beleg, Code-Review mit eigenem Kontext, CI-Kompilierung. Keine Laufzeitbeobachtung. Belegt ist, dass `PeripheralSynthesisPvE` ohne Skip-Flag nicht nutzbar war und eine ihrer beiden Aufrufstellen tot; nicht belegt ist, dass die Blaumagier-Rotation dadurch im Spiel messbar besser läuft.


### A33 · Konzeptdokumente auf Urteilsstil umgestellt (08.09.2026)

**Anlass:** Vorgabe des Auftraggebers. Konzepte seien im Gutachtenstil geschrieben —
Annahmen, revidierte Annahmen, erneute Anpassungen bis zum Ergebnis —, weshalb beim
Lesen nicht erkennbar sei, was der aktuelle Sachstand ist. Zu prüfen war, ob der
Urteilsstil die Umsetzung verbessert, und die Regel gegebenenfalls allgemein zu fassen.

**Der Befund ist am Artefakt belegt, nicht nur plausibel.** `09-tank-selfprotection.md`
trug sieben Abschnitte reiner Prozesshistorie und einen Nachtrag, der einleitend
feststellte, „mehrere Aussagen weiter oben" seien überholt. `08-mitigation-synergy.md`
bezeichnete sich in Zeile 4 als „Konzept ohne Code", während beide Schritte gebaut sind
(A20). `04-concept.md` führte A4a als „offen", obwohl fünf Dateien umgebaut sind
(Register #66). `06-fork-audit.md` schrieb „Was offen bleibt: Nichts. `TODO.md` ist
leer" bei vierzehn offenen Einträgen. `03-universal.md` führte U2 weiter als Schwäche,
obwohl `04` sie ausdrücklich widerlegt und das dort auch vermerkt — die Korrektur war
nie zurückgetragen worden.

**Falsifikation, drei Gegenpositionen.** *Die Historie sei der Beleg* — widerlegt: Die
verworfenen Optionen bleiben mitsamt Begründung, das ist ADR-Bestandteil; was entfällt,
ist die Reihenfolge der eigenen Irrtümer, und die liegt bereits als A21–A23, A27–A29 und
C13–C22 im Archiv. Dieselbe Information wurde doppelt geführt. *Der Urteilsstil verdecke
Unsicherheit* — widerlegt: Die Kalibrierungspflicht gilt unabhängig vom Aufbau, und der
Gutachtenstil ist hier schlechter, weil er einen Fehlerpfad hat, den der Urteilsstil
nicht hat: Wer auf halber Strecke aufhört, hält eine zurückgenommene Position für den
Stand. *Der Aufwand* — begrenzt: Drei Dokumente trugen echte Chronik, vier sind
Bestandsaufnahmen ohne Entscheidungsverlauf.

**Eine Abgrenzung war nötig und ist Teil der Regel geworden.** Die Historie des
**Gegenstands** ist Inhalt, die des **Dokuments** nicht. `06-fork-audit.md` existiert
gerade, um zu sagen, wo die Fork-Änderungen sich als falsch erwiesen haben; ohne diese
Abgrenzung hätte die Regel es entkernt.

**Umgesetzt.** `09` von 963 auf 330 Zeilen, `08` und `04` neu gefasst, alle drei mit
vorangestelltem Ergebnis, Umsetzungsstand als Tabelle und einem Abschnitt „Was
ausgeschlossen wurde und warum". `03`, `01`, `05`, `06` gezielt nachgezogen. Die
Kürzung ist fast ausschließlich entfallene Chronik; die drei überholten Stände sind
gegen den Code gemessen und richtiggestellt, nicht fortgeschrieben.

**Prüfmittel.** `scan12.py` meldet Chronik-Überschriften und Formulierungen, die eine
Aussage gegen eine frühere Fassung desselben Dokuments stellen, und schließt
Codevarianten über den Satzkontext aus. Gegen den Vor-Zustand 43 Treffer, danach keiner.

**Blameless Postmortem zur Priorisierung derselben Sitzung.** Aus einer
Zwei-Zeilen-Fundstelle in der Rotmagier-PvP-Konfiguration wurde ein vollständiger
Durchgang über Blaumagier, PvP und Bozja (A32), während die Punkte des laufenden
Auftrags warteten. Ursache war die Defektklassenregel, die die *Erhebung* vollständig
verlangt — sie sagt nichts über die *Bearbeitung*. Der Auftraggeber hat das als
fehlgeleitetes Investment beanstandet. Die fehlende Regel ist ergänzt: Priorität folgt
dem Auftrag und dem Nutzungsprofil, Fundstellen außerhalb davon werden erfasst, nicht
bearbeitet. Der Befund selbst bleibt gültig, die Reihenfolge war falsch.

**Erreichter Prüfgrad:** statische Selbstprüfung, Prüfskript mit Selbsttest und
Gegenprobe am Vor-Zustand, Abgleich jedes berichteten Stands gegen den Code
(`git show upstream/main`, Zählung benannter Stufen je Datei, `git ls-remote --tags`).
Kein Vier-Augen-Prinzip.


### A34 · Die Living-Dead-Rückhaltung hielt nur auf einem Teil der Wege (08.09.2026)

**Anlass:** Nachfrage des Auftraggebers, warum der Punkt weiter als offen geführt wird,
mit dem Auftrag, kritisch zu prüfen, was wie umgesetzt wurde. Die Prüfung hat ihn
bestätigt — allerdings aus einem anderen Grund, als der Titel des Punktes nannte.

**Was gebaut war.** Die Rückhaltung wirkte auf der Flag-Ebene (`ShouldHealSingle`,
`ShouldHealSelf`) und in der Selbstabkürzung von `GeneralHealTarget`. Drei Stellen, an
denen die Bedingung `Service.Config.WithholdHealingForLivingDead && …InDeathTriggerWindow()`
wortgleich ausgeschrieben stand.

**Was fehlte.** Die **Kandidatenliste** derselben Zielwahl kannte den Halt nicht: Zeile
3526 filtert `HealingIneffectiveStatus`, mehr nicht. Und der Kommentar unmittelbar
darunter behauptete das Gegenteil — die Selbstabkürzung brauche „die gleichen Prüfungen,
die ein Ziel aus der Liste heraushalten", und nannte dabei ausdrücklich den
Living-Dead-Fall. Ein Widerspruch zwischen Kommentar und Code, und nach der
Auslegungsregel ist der Widerspruch der Befund.

**Die Wirkungskette, vollständig verfolgt.** Der Träger stand in `ranked`, als geschützt
nach hinten sortiert. Solange ein ungeschütztes Mitglied in der Kandidatenmenge liegt,
gewinnt dieses — deshalb fiel es nicht auf. Das Heilflag kann aber von einem Mitglied
gesetzt worden sein, das **außerhalb der Reichweite der gewählten Heilaktion** liegt;
dann ist der Träger der einzige Kandidat und wird geheilt. Und liefert `GeneralHealTarget`
`null`, greift der Fallback auf den Träger der Tank-Haltung — also genau auf den
Dunkelritter, der Grit trägt. Der Halt war damit in dem Zustand wirkungslos, für den er
gebaut wurde.

**Umgesetzt.** Die Bedingung ist zu `ObjectHelper.IsHeldForDeathTrigger` /
`PlayerIsHeldForDeathTrigger` zusammengezogen — in `ObjectHelper`, weil `StatusHelper`
bewusst keine Konfiguration liest — und der Halt sitzt jetzt im **Eingangsfilter** von
`FindHealTarget`. Das ist der kleinste wirksame Eingriff: Kandidatenliste, Heiler- und
Tankpass, Auswahl des am schwersten Verletzten sowie beide Fallbacks schöpfen aus
derselben Menge und sind mit einer Zeile abgedeckt, statt mit vier verstreuten Prüfungen,
von denen wieder eine vergessen werden kann. Rohe Vorkommen der Einstellung außerhalb
des Helfers: keine.

**Zwei Wege bleiben offen, mit Begründung.** Flächenheilung trifft den Träger als
Nebenwirkung; sie zu unterlassen würde die Gruppe für den Auslöser opfern und verstieße
gegen die Rangordnung. Fremde Rotationen mit `targetOverride: TargetType.Tank` (Beiruta)
umgehen die zentrale Zielwahl, halten aber über ihre eigene Sperrliste ohnehin zurück —
nach der Priorisierungsregel erfasst, nicht bearbeitet.

**Zum Titel des Punktes.** „Gehört in die Heilerrotationen, nicht in die Zielwahl" war
die Schlussfolgerung aus H1 und ist nicht umgesetzt worden: Die Rückhaltung liegt
zentral. Das ist begründet — der Vorlauf ist die einzige rotationsabhängige Größe, und
er ist mit zwei GCDs konservativ gewählt. Der Restfehler bleibt bestehen und steht in
`09-tank-selfprotection.md` unter „Was offen bleibt". Der Punkt war also offen, aber die
Lücke lag nicht dort, wo sein Titel sie vermutete.

**Erreichter Prüfgrad:** statische Selbstprüfung mit vollständiger Verfolgung der
Wirkungskette bis zu beiden Fallbacks, Erhebung aller `TargetType.Heal`-Pfade und aller
Stellen mit selbstgesetztem Heilziel, CI-Kompilierung. Keine Laufzeitbeobachtung.


### A35 · The Blackest Night: Werkzeugwahl statt Rückhaltung (08.09.2026)

**Anlass:** Frage des Auftraggebers, ob sich ein Schild zurückstellen lässt, bis
`BlackestNight` aus der Statusliste verschwindet, und stattdessen Heilung oder ein HoT
zu wirken sei — mit der Begründung, Heilung und HoT wirkten sich auf die
TBN-Barriere nicht aus. Die Prüfung bestätigt beide Prämissen und legt zwei eigene
Fehler des Konzepts offen.

**Erste Korrektur: Heilung berührt den Auslöser nicht.** Konzept 09 führte in der
Klasse-A-Tabelle, fremde Heilung wirke „indirekt: hebt die HP, sodass weniger Schaden
gegen die Barriere läuft". Das ist mechanisch falsch. Eine Barriere absorbiert
eingehenden Schaden **vor** den HP; ihr Verbrauch hängt am Schaden, nicht am
Gesundheitsstand. Die einzige Kopplung läuft umgekehrt: Stirbt der Träger, bevor die
Barriere aufgebraucht ist, entfällt Dark Arts — Heilung wirkt also **für** den
Auslöser. Damit ist TBN kein Fall, in dem Heilung zurückzuhalten wäre.

**Zweite Korrektur: die falsche Frage.** Der Fall war verworfen worden, weil „der
Auslöser nicht beobachtbar" sei — der Client führt je Charakter genau einen
Schildwert. Das stimmt, ist aber nicht die Frage, die die Regel braucht. Für eine
Werkzeugwahl genügt „liegt TBN?", und das steht in der Statusliste
(`StatusID.BlackestNight` 1178, PvP-Form 1308). Dieselbe Fehlerform wie C19: eine
schwerere Frage gestellt als nötig und den Fall daran scheitern lassen.

**Dritte Korrektur: der Mechanismus.** Die frühere Sorge war „mehr Gesamtpuffer =
langsamerer Verbrauch" beziehungsweise Verdrängung des schwächeren Schildes durch den
stärkeren. Der überlieferte Mechanismus ist ein anderer: eine feste
**Verbrauchsreihenfolge** mehrerer Barrieren. TBN hat gegenüber typischen
Heilerschilden Vorrang, einzelne Barrieren stehen aber davor und können verhindern,
dass es rechtzeitig aufgezehrt wird; genannt werden Radiant Aegis und Eukrasian
Diagnosis. Quellenstatus: Spielerforen und Job-Guides, **teils widersprüchlich** — eine
Quelle ordnet TBN über Eukrasian Diagnosis ein, eine andere darunter. Zwei
Primärquellen (Consolegames-Wiki, ein Lodestone-Blog mit systematischer
Prioritätsliste) sind vom Netzwerk-Egress dieser Umgebung blockiert. Die Reihenfolge
bleibt unbelegt.

**Warum die Entscheidung daran trotzdem nicht hängt.** Solange TBN liegt, ist der
Träger bereits geschildet. Ein zweiter Schild verdoppelt vorhandenen Schutz, während
die HP ungefüllt bleiben, die er nach dem Ablauf braucht; Heilung leistet dort mehr und
kostet den Auslöser nichts. Fällt die Prioritätsfrage ungünstig aus, verhindert die
Zurückstellung zusätzlich den Verlust von Dark Arts. Der Vorteil besteht in beiden
Zweigen der offenen Frage — die Reihenfolge bestimmt nur seine Größe, nicht sein
Vorzeichen.

**Gegenposition, die stehen bleibt:** Gegen einen einschlagenden Tankbuster verhindert
ein Schild, statt zu reparieren. Ist der Buster größer als die TBN-Barriere, gehört der
zweite Schild dazu. Die Regel braucht deshalb dieselbe Aufhebung wie die übrigen: bei
Buster-Fenster oder niedriger Gesundheit fällt der Schild.

**Umsetzungsweg, nicht gebaut.** Der Eingriff gehört in `ActionTargetInfo.CheckStatus`;
eine Aktion ist ohne neue Liste als schildgewährend erkennbar, wenn ihr
`TargetStatusProvide` einen Eintrag aus `StatusHelper.ShieldStatus` enthält. Zwei
Vorbedingungen sind dabei aufgefallen und als eigene Defekte erfasst:
`ModifyDivineBenisonPvE` setzt `StatusProvide` statt `TargetStatusProvide`, obwohl
Divine Benison auf fremde Ziele geht — die Doppelbelegungssperre greift dort nie, und
die Erkennung sähe die Aktion nicht; und `StatusID.Intersection` (1889) samt
`Intersection_4040` fehlt in `ShieldStatus`, obwohl beide als „A magicked barrier is
nullifying damage" ausgewiesen sind.

**Erreichter Prüfgrad:** statische Selbstprüfung am Artefakt (Statusbeschreibungen,
`ShieldStatus`, alle `TargetStatusProvide` der Heilerschilde), zwei Websuchen, zwei
gescheiterte Primärquellenabrufe. Keine Laufzeitbeobachtung. Kein Code geändert.


### A36 · Die Verbrauchsreihenfolge belegt — die eigene Empfehlung fällt (08.09.2026)

**Anlass:** Aufforderung des Auftraggebers, es erneut zu versuchen. Der einzige in A35
gescheiterte Vorgang war der Abruf der Primärquellen zur Barriere-Reihenfolge; A35
hatte daraus „unbelegt" gemacht und die Regel trotzdem empfohlen.

**Erst der Umgebungsweg, dann die Quelle.** Die Umgebung dokumentiert für
Egress-Fehler einen eigenen Diagnoseweg (`/root/.ccr/README.md`,
`$HTTPS_PROXY/__agentproxy/status`), den A35 nicht benutzt hatte — die Grenze war
behauptet, ohne den vorgesehenen Weg zu prüfen. Nachgeholt: Der Status weist keine
Relay-Fehler aus, und die README ordnet den Fall als Organisationsrichtlinie ein, die
ausdrücklich **nicht** zu umgehen, sondern zu melden ist. Die Sperre ist damit bestätigt
und korrekt behandelt. Was fehlte, war nicht der Zugriff, sondern eine bessere Suche.

**Die nachgeholte Suche liefert die Reihenfolge.** The Blackest Night hat
Verbrauchsrang 3, Eukrasian Diagnosis 4, Divine Benison 12 — der niedrigere Rang wird
zuerst aufgezehrt. Damit steht TBN **vor** allen Schilden, die ein Heiler auf einen Tank
legen kann.

**Folge: die Empfehlung aus A35 fällt.** Ein Heilerschild verzögert die TBN-Absorption
nicht, kostet also kein Dark Arts. Er geht auch nicht verloren, sondern absorbiert,
sobald TBN aufgebraucht ist — Zurückstellen spart nichts, es verschiebt nur. Von der
vorgeschlagenen Regel bleibt die gewöhnliche Dringlichkeitsfrage übrig, und die ist
bereits gelöst: `BlackestNight` steht in `StatusHelper.ShieldStatus` und geht über
`GetEffectiveHpPercent` in die Heilentscheidung ein.

**Die dritte genannte Barriere ist gegenstandslos.** Radiant Aegis ist laut
`Status.resx` **(SMN)** — ein Selbstschild des Beschwörers, der nie auf dem
Dunkelritter liegt. Der Job-Guide, der sie neben Eukrasian Diagnosis als vorrangig
führt, beschreibt insoweit einen Fall, den es beim Tank nicht gibt.

**Ein Quellenkonflikt bleibt, eng begrenzt.** Für Eukrasian Diagnosis widersprechen
sich Liste (Rang 4, also hinter TBN) und Job-Guide (vorrangig). Betroffen wäre allein
der Weise. Für Weißmagier, Gelehrten und Astrologen sagen beide Quellen dasselbe.

**Was aus dem Durchgang bleibt.** Die beiden Korrekturen aus A35 stehen unverändert:
Heilung berührt den Auslöser nicht (C24), und die Frage „liegt TBN?" ist beantwortbar
(C25). Ebenso die zwei nebenbei gefundenen Defekte — Divine Benison auf der Zielseite
und die fehlenden `Intersection`-Ids —, die von The Blackest Night unabhängig sind und
in `TODO.md` bleiben. Was fällt, ist allein die abgeleitete Regel.

**Lehre.** A35 hat aus einer widersprüchlichen Quellenlage eine Empfehlung gemacht und
die Widersprüchlichkeit mit dem Argument entschärft, der Vorteil bestehe „in beiden
Zweigen". Dieses Argument war falsch, weil es einen dritten Zweig übersah: dass der
zusätzliche Schild gar nichts kostet, weil er nicht verfällt. Eine offene Frage ist
kein Anlass, das Ergebnis gegen sie zu immunisieren — sie ist ein Anlass, sie zu
schließen.

**Nachgereichte Prämissenprüfung.** Die Frage des Auftraggebers, ob der Dunkelritter TBN
auch auf andere wirken kann, deckte eine ungeprüfte Annahme auf: `ActionId.resx` (7393)
sagt „self or **target party member**". Damit ist der Träger nicht zwingend der
Dunkelritter, und die Abtuung von Radiant Aegis war falsch begründet (C27). Am Ergebnis
ändert das nichts — die Schilde, die ein *Heiler* legen kann, stehen weiter hinter TBN.
Zwei neue Fundstellen fielen dabei an, beide Instanzen bereits erfasster Klassen:
`ModifyTheBlackestNightPvE` setzt `StatusProvide` statt `TargetStatusProvide`, und
`BlackestNight_1308` kommt im Code nirgends vor.

**Erreichter Prüfgrad:** statische Selbstprüfung am Artefakt, Proxy-Diagnose nach der
Umgebungsdokumentation, drei Websuchen. Die Reihenfolge ruht auf Spielerdokumentation,
nicht auf einer offiziellen Beschreibung. Kein Code geändert.


### A37 · Die beiden Schild-Defektklassen behoben (08.09.2026)

**Anlass:** Freigabe des Auftraggebers für die zwei Klassen, die aus der
Blackest-Night-Prüfung (A35/A36) abgefallen waren.

**Klasse 1 — Schildaktionen prüfen die falsche Seite.** `ModifyDivineBenisonPvE` und
`ModifyTheBlackestNightPvE` setzten `StatusProvide`, das gegen `Player.Object` gelesen
wird. Beide Aktionen gehen aber auf Fremdziele: `ActionId.resx` (7393) beschreibt The
Blackest Night als „Creates a barrier around self or **target party member**",
`DRK_Reborn.cs:128` castet mit `targetOverride: TargetType.LowHP`, und `BeirutaWHM.cs:516`
liest `DivineBenisonPvE.Target.Target`. Die Wirkung war **zweiseitig**, was zuvor nur zur
Hälfte erfasst war: Die Sperre gegen Doppelbelegung griff auf Fremdzielen nie — **und** sie
blockierte die Aktion vollständig, sobald der Spieler den Status selbst trug, auch wenn er
jemand anderen schilden wollte. Beim Dunkelritter wiegt das doppelt: Eine überschriebene
eigene Barriere wirft den Dark-Arts-Auslöser weg, für den sie gezündet wurde. Drei
Geschwisteraktionen (`AdloquiumPvE`, `EukrasianDiagnosisPvE`, `CelestialIntersectionPvE`)
belegen das richtige Muster.

**Klasse 2 — Barrieren mit mehreren Ids nur einfach geführt.** Der Fund war „drei fehlende
Ids"; die Erhebung mit `scan13.py` ergab **21 in 15 Gruppen**, darunter Galvanize,
Eukrasian Diagnosis, Divine Benison, Haima und Blackest Night — die Schilde, die ein Heiler
täglich sieht. Der Defekt ist schwerer als eine Ungenauigkeit: `HasSurvivingShield` liest
`GetObjectShield() > 0 && !WillStatusEnd(…, ShieldStatus)`, und `WillStatusEnd` meldet einen
**abwesenden** Status als endend. Eine ungeführte Barriere zählt damit als gar keine, und
ihr Träger erscheint verletzter, als er ist. Alle 21 sind ergänzt, dazu Celestial
Intersection als Heiler-Einzelschild in normalem Inhalt.

**Was der Filter ausschließt, und warum das nötig war.** `scan13.py` entscheidet die
Zugehörigkeit an der Wirkbeschreibung, nicht am Namen. Drei Nachbarn wären sonst über ihren
Namen mit eingesammelt worden und gehören nicht hinein: `Catalyze_3088` mindert Schaden,
statt ihn aufzuhalten; `DivineVeil` (726) erzeugt eine Barriere erst bei späterer Heilung;
`Haimatinon_2870`/`_3111` sind der Stapelzähler, der eine Barriere wiederherstellt, nicht
die Barriere. Ein pauschales Übernehmen aller gleichnamigen Ids hätte drei Fehleinstufungen
erzeugt.

**Betroffenenkreis, ausdrücklich benannt.** Die Umstellung auf `TargetStatusProvide` wirkt
mittelbar auf fremde Rotationen: `ChurinDRK.cs:173`, `:196` und `BeirutaWHM.cs:516`
vergleichen `…Target.Target` mit einem erwarteten Ziel. Fällt ein Ziel wegen des nun
greifenden Filters aus der Kandidatenmenge, liefert `Target.Target` ein anderes und der
Vergleich schlägt fehl. Nach der Vorgabe des Auftraggebers ist das zulässig — die
Einschränkung gilt der direkten Bearbeitung fremder Dateien, nicht der mittelbaren Wirkung
zentraler Änderungen —, aber es ist eine reale Verhaltensänderung in fremdem Werk und
gehört benannt. Die Schildliste selbst ist davon frei: Kein Churin- oder Beiruta-Modul liest
`ShieldStatus`, `HasSurvivingShield` oder `GetEffectiveHpPercent`.

**Nicht umgesetzt.** 55 Barrieregruppen ohne jeden Vertreter in `ShieldStatus`, gemischt
PvE, PvP und Duty. Sie brauchen je eine eigene Lesung nach Geltungsbereich — der Filter des
Skripts trennt Barrieren von Schadensminderung, nicht PvE von PvP. In `TODO.md` mit der
Vorsortierung erfasst.

**Erreichter Prüfgrad:** statische Selbstprüfung, Prüfskript mit Selbsttest gegen die drei
Abgrenzungsfälle, Gegenprobe (21 gemeldet vor der Änderung, 0 danach), CI-Kompilierung.
Keine Laufzeitbeobachtung.


### A38 · Paketidentität: Prerelease-Label statt Build-Metadaten (08.09.2026)

**Anlass:** Entscheidung des Auftraggebers zwischen den drei vorgelegten Wegen —
eigener `PackageId`, Prerelease-Label, Verzicht auf die Paketauslieferung. Gewählt:
Prerelease-Label `-wsh1`.

**Der Defekt, noch einmal am Artefakt gemessen.** `publish.yaml:23` bildete
`$numericVersion = ($tag -split '\+')[0]`, weil `AssemblyVersion` und `FileVersion`
rein numerisch sein müssen (CS7034). Zeile 42 reichte **denselben** Wert als
`PackageVersion` weiter. Das ausgelieferte `.nupkg` hieß damit
`RotationSolverReborn.Basic 7.5.5.41` — die Upstream-Identität. Zweiter, bis dahin
nicht erfasster Fundort: `Directory.Build.props` setzt nur `<Version>`, und
`RotationSolver.Basic` baut mit `GeneratePackageOnBuild`, sodass **jeder** lokale und
PR-Build ebenfalls ein Paket unter der Upstream-Identität erzeugte.

**An der offiziellen Quelle belegt** (Microsoft Learn, „Package versioning"), weil hier
ein Schnittstellenvertrag berührt ist:

| Frage | Beleg |
|---|---|
| Ist `7.5.5.41-wsh1` gültig? | `NuGetVersion` kennt ein viertes Segment `Revision`; ausgenommen Prerelease- und Metadaten-Label lautet die Form `Major.Minor.Patch.Revision` |
| Geht das Label in die Identität ein? | Build-Metadaten werden bei der Normalisierung entfernt, Prerelease-Label nicht |
| Sehen ältere Clients das Paket? | SemVer-2.0-spezifisch ist eine Version nur bei **punktgetrenntem** Label oder bei Build-Metadaten. `-wsh1` hat keinen Punkt, ist also SemVer-1.0-konform und für alle Clients sichtbar — anders als `-wsh.1` |
| Was kostet es die Verbraucher? | „By default, NuGet does not include pre-release versions"; wer das Paket bezieht, muss die Version exakt angeben oder Prerelease zulassen |

**Was die Wahl nicht leistet, und das gehört benannt.** Bei gleichem `PackageId` sortiert
`7.5.5.41-wsh1` **unter** `7.5.5.41` — NuGet wählt eine Version ohne Suffix zuerst. In
einem Feed mit beiden Paketen bekommt ein Verbraucher ohne ausdrückliche Version weiterhin
Upstream. Der Zweck der Entscheidung war Unterscheidbarkeit, und die ist erreicht; Vorrang
wäre nur über einen eigenen `PackageId` zu haben.

**Umgesetzt.** `publish.yaml` leitet `packageVersion` aus dem Tag ab (`+` → `-`) und bricht
ab, wenn der Tag dem Schema `<upstream>+wsh<n>` nicht folgt — ohne diese Schranke käme der
Defekt bei einem schemawidrigen Tag still zurück. `Directory.Build.props` setzt
`<PackageVersion>7.5.5.41-wsh1</PackageVersion>` für alle untagged Builds.

**Der CHANGELOG-Kopf war nach der Änderung falsch** und ist mit korrigiert: Er begründete
seine Existenz damit, dass die Nummer den Fork nicht kennzeichnen könne. Das trifft nicht
mehr zu. Was bleibt, ist der eigentliche Grund — der numerische Teil folgt dem
Upstream-Release, nicht der Kompatibilität dieses Forks, kann also keinen Bruch der
Paketoberfläche ausdrücken.

**Nicht erledigt:** der Release-Ballast, der dasselbe `.nupkg` betrifft. Eigene Frage, eigener
Eintrag in `TODO.md`.

**Erreichter Prüfgrad:** statische Selbstprüfung, offizielle Herstellerdokumentation für alle
vier Vertragsfragen, CI-Kompilierung. Der Veröffentlichungspfad selbst ist nicht geprüft — er
läuft nur auf einen Tag, und `build.yaml` kompiliert lediglich.

---

### A39 · Die README beschrieb ausschließlich den Upstream (08.09.2026)

**Anlass:** Auftrag „auch readme aktualisieren und anpassen", unmittelbar nach der Umstellung
der Paketidentität (A38).

**Befund, gemessen statt erinnert.** `git diff upstream/main -- README.md` ist leer: Die
Root-README ist zeichengleich mit der Upstream-Fassung. Sie nennt folglich weder diesen Fork
noch irgendeine seiner Abweichungen — ihre Badges zählen Upstream-Downloads, ihre
Installationsanleitung fügt das Upstream-Plugin-Repository hinzu, ihre Release-Links zeigen auf
Upstream-Tags (7.4.1.10, 7.4.5.35), und der Abschnitt „Latest version of RSR for each FFXIV
version" ist für einen Fork auf `7.5.5.41` gegenstandslos. Wer das Repository auf GitHub öffnet,
liest die Beschreibung eines anderen Projekts.

**Wirkungsbereich erhoben.** Die Identität dieses Forks steht an fünf Stellen und war an keiner
davon aus der README erreichbar: `publish.yaml` (Tag-Schema `<upstream>+wsh<n>`, Asset
`latest.zip`), `Directory.Build.props` (`PackageVersion 7.5.5.41-wsh1`),
`RotationSolver.Basic.csproj` (`PackageId RotationSolverReborn.Basic`, unverändert),
`CHANGELOG.md` (Brüche der Paketoberfläche) und `manifest.json`. Ebenso wenig verwiesen war die
Arbeitsdokumentation des Forks — `docs/rotation-flow/`, `AUDIT_LOG.md`, `TODO.md`,
`.github/scripts/audit/`, `CLAUDE.md`. Die zweite README des Baums,
`.github/scripts/audit/README.md`, ist aktuell (scan9 bis scan13 dokumentiert) und war nicht
Gegenstand.

**Konfliktrisiko regionsgenau statt über Dateiaktivität.** Der Klon ist flach; über die
enthaltenen 152 Upstream-Commits (25.05. bis 04.09.2026) berührt genau einer `README.md`, und
`git blame upstream/main` setzt **alle** 43 Zeilen auf diesen einen Commit. Upstream schreibt
die Datei also nicht abschnittsweise fort, sondern als Ganzes, wenn er sie anfasst. Das
entscheidet die Form des Eingriffs: ein eigener Block **vor** dem Upstream-Text, dieser selbst
unangetastet. Ein Konflikt entsteht dann nur beim nächsten Gesamt-Rewrite und ist trivial
aufzulösen — eigenen Block behalten, Upstream-Text übernehmen.

**Verworfene Optionen.** *Nullvariante:* Die Startseite behauptet weiterhin die
Upstream-Identität, und ein Bezieher des Pakets findet die Prerelease-Bedingung aus A38 nicht.
*README vollständig neu schreiben:* maximiert die Merge-Fläche gegenüber der Gruppe U und wirft
Upstream-Inhalte weg, die für dieses Repository unverändert gelten. *Separates `FORK.md`:*
GitHub zeigt auf der Startseite die README, nicht eine Nebendatei — der Hinweis erreicht den
Leser nicht, dem er gilt.

**Was bewusst nicht in die README kam, weil unbelegt.** Wie ein Fork-Build zu installieren ist:
Der Baum enthält kein `pluginmaster.json` (`git ls-files` ohne Treffer), der Weg über das
Release-Asset ist in dieser Umgebung nicht nachprüfbar, und eine erfundene Anleitung wäre
schlechter als keine. Die README stellt deshalb nur fest, dass die vorhandene Anleitung den
Upstream betrifft. Ebenso ausgelassen sind die Folgen der geteilten Plugin-Identität; sie sind
Inferenz und stehen als solche gekennzeichnet in `TODO.md`.

**Mit korrigiert.** Der TODO-Eintrag „Zwei entfernte öffentliche Member seit dem letzten
Release" begründete den Ausweis im CHANGELOG noch damit, das Fork-Suffix sei
SemVer-Build-Metadatum — seit A38 unzutreffend. Ersetzt durch den Grund, der trägt: Der
numerische Teil folgt dem Upstream-Release, nicht der Kompatibilität dieses Forks.

**Neu erfasst.** `manifest.json` ist gegenüber Upstream unverändert, `InternalName` und
`RepoUrl` eingeschlossen. Als technische Schuld mit Gegenposition und offener Freigabe in
`TODO.md` aufgenommen, nicht im Vorbeigehen geändert: Ein eigener `InternalName` trennt die
gespeicherte Nutzerkonfiguration und wäre ohne Migrationspfad ein Verlust.

**Nachgetragen zu A38:** Der CI-Lauf 144 zu `20b665b8` ist grün (`conclusion: success`).

**Erreichter Prüfgrad:** statische Selbstprüfung, gestützt auf Messungen am Repository
(`git diff` gegen `upstream/main`, `git blame`, `git ls-files`, `git ls-remote --tags origin`).
Die Datei enthält keinen Code; die Verweisziele wurden im Baum nachgesehen, die Darstellung auf
GitHub selbst ist nicht geprüft.

---

### A40 · Upstream-Angleichung und Prüfung des Release-ZIPs (08.09.2026)

**Anlass:** zwei Aufträge — „Das zip Release muss eine spielbare Version enthalten" und „fork vom
upstream her angleichen".

**Ergebnis vorweg:** Das ausgelieferte ZIP ist im Aufbau deckungsgleich mit dem Upstream-Release,
das nachweislich spielbar ist. Was fehlte, war nicht sein Inhalt, sondern sein Stand: Upstream hat
am 08.09.2026 das Release `7.5.6.0` herausgegeben, und dessen **einzige** Änderung gegenüber
`7.5.5.41` ist der Wechsel der Dalamud-Bezugsquelle auf den Staging-Kanal. Der Fork steht jetzt auf
diesem Stand.

**Messung am Artefakt.** Drei Release-Assets heruntergeladen und ausgelesen — Fork `7.5.5.41+wsh1`,
Upstream `7.5.5.41` und Upstream `7.5.6.0`:

| Prüfpunkt | Ergebnis |
|---|---|
| Dateibestand | je 12 Dateien, gleiche Namen und Rollen. Nutzlast `RotationSolver.dll`, `RotationSolver.Basic.dll`, `ECommons.dll`, `RotationSolver.json`; der übrige Inhalt ist in allen drei ZIPs derselbe Ballast |
| Größe | Fork 5.346.696 Bytes, Upstream 7.5.6.0 5.336.327 — der Unterschied liegt in den eigenen Assemblies |
| Manifest | identisch bis auf `AssemblyVersion` (7.5.5.41 / 7.5.6.0); `DalamudApiLevel` in beiden 15 |
| `deps.json` | Bibliotheks- und Laufzeitlisten deckungsgleich, einziger Unterschied der eigene Eintrag `RotationSolver.Basic/<version>` |
| fehlende Laufzeit-DLLs | neun in `deps.json` genannte DLLs liegen in **keinem** der ZIPs, in allen dieselben — sie stellt Dalamud |

Kein fehlender Bestandteil, keine zusätzliche Abhängigkeit, kein struktureller Unterschied zum
spielbaren Upstream-Paket.

**Ursache des Fehlerbildes, so weit belegbar.** Der Tag `7.5.6.0` zeigt auf `b6de6807`;
`git rev-list --count 7.5.5.41..7.5.6.0` ergibt 1, und dieser eine Commit ändert eine Zeile in
`publish.yaml`: `dalamud-distrib/latest.zip` → `dalamud-distrib/stg/latest.zip`. Primärquelle
`goatcorp/dalamud-distrib`: Release-Kanal `15.0.3.2`, Staging-Kanal `15.0.3.2-36-g43e65e2b6`. Die
Dalamud-Dokumentation beschreibt den Ablauf nach einem Spielpatch — Dalamud lädt ein Plugin nicht
mehr, bis beide nachgezogen sind, und Staging folgt `master` schneller als Release.
**Inferenz, als solche gekennzeichnet:** dass das Fork-ZIP beim Auftraggeber nicht lief, folgt
daraus, dass es vor diesem Wechsel gebaut wurde. Am Artefakt ist das nicht zu belegen; dazu fehlt
die Fehlermeldung aus dem Spiel.

**Angleichung.** `git merge upstream/main` konfliktfrei — Upstreams Zeile und die
Fork-Änderungen an `publish.yaml` liegen in verschiedenen Schritten derselben Datei. Nachweis
danach: `git rev-list --left-right --count upstream/main...HEAD` = 0 / 344.

**Drei Folgeänderungen, jede mit eigenem Grund:**

1. `Directory.Build.props` auf `7.5.6.0`, `7.5.6.0+wsh1`, `7.5.6.0-wsh1`. Der Kommentar band diese
   Werte an den letzten **Fork**-Tag; sobald ein Upstream-Release darüber hinaus gemergt ist,
   untertreibt das, was der Code ist. Umgestellt auf den Upstream-Release-Stand, auf dem der
   gemergte Baum steht — der Kommentar ist mit korrigiert, statt den Widerspruch stehen zu lassen.
2. `build.yaml` auf denselben Dalamud-Kanal wie `publish.yaml`. Andernfalls kompiliert die
   PR-Prüfung gegen andere Assemblies als der Release-Build, und die stärkste hier erreichbare
   Prüfung misst nicht mehr das Auslieferungsartefakt. Die Zeile ist eine Fork-Abweichung und als
   solche in `TODO.md` geführt, mit der Bedingung, sie mitzuziehen, wenn Upstream zurückwechselt.
3. Der Ballast-Eintrag in `TODO.md` ist um den Messbeleg ergänzt: Upstream liefert denselben
   Ballast aus, der Fork weicht nicht ab. Empfehlung dort ergänzt, ihn nicht aufzugreifen.

**Was das nicht leistet:** Ein Release entsteht erst mit einem Tag, und die Freigabe dafür liegt
beim Auftraggeber. Bis dahin bleibt `7.5.5.41+wsh1` das jüngste Fork-Release, gebaut gegen den
Release-Kanal.

**Erreichter Prüfgrad:** Artefaktprüfung an drei heruntergeladenen ZIPs, Messungen an der
Versionsgeschichte, Primärquelle für die beiden Dalamud-Kanäle, Fachdokumentation für den
Patch-Ablauf, CI-Kompilierung gegen den Staging-Kanal. Nicht geprüft: der Ladevorgang im Spiel und
der Veröffentlichungspfad selbst, der nur auf einen Tag läuft.

---

### A41 · Barrierekandidaten nach der Aktion sortiert, nicht nach dem Statusnamen (08.09.2026)

**Anlass:** Auftrag, die offenen Punkte im Loop zu prüfen, zu begründen und Empfehlungen
auszusprechen. Erster Punkt: die 55 Barrieregruppen ohne Vertreter in `StatusHelper.ShieldStatus`.

**Ergebnis:** Von den 61 Ids stehen **15** hinter einer PvE-Aktion, die eine Barriere erzeugt; zehn
davon sind Jobbarrieren im Nutzungsprofil des Auftraggebers. Die zuvor vorgelegte Sechserliste war
zu zwei Dritteln falsch und ließ acht der zehn aus (C28).

**Warum der Statustext nicht reicht.** PvE- und PvP-Form einer Fähigkeit tragen denselben
Anzeigenamen **und** dieselbe Wirkbeschreibung — „A magicked barrier is nullifying damage". Der
Geltungsbereich in Klammern nennt nur den Job. Die frühere Liste hatte daraus geschlossen, ein
Jobkürzel bedeute PvE; das ist ein Surrogat, und es zeigte in die falsche Richtung:

| Id | Statustext | PvE-Aktion desselben Namens | Folge |
|---|---|---|---|
| `Aquaveil_3086` (WHM) | Barriere | „Reduces damage taken by a party member or self by 15%" | PvP-Form, nicht aufnehmen |
| `HolySheltron_3026` (PLD) | Barriere | „Reduces damage taken by 15% … Grants Knight's Resolve" | PvP-Form, nicht aufnehmen |
| `DivineCaress` (WHM) | Barriere | „Creates a barrier … absorbs damage equivalent to a heal of 400 potency" | aufnehmen |
| `ShakeItOff` (WAR) | Barriere | „Creates a barrier … absorbs damage totaling 15% of maximum HP" | aufnehmen |

**Die Erhebung, vollständig.** Zehn Jobbarrieren: `ShakeItOff` 1457 und `ShakeItOff_1993` 1993
(WAR) · `SeraphicVeil` 1917, `SeraphicVeil_2040` 2040, `SeraphicVeil_3097` 3097 (SCH-Seraph) ·
`NeutralSect_1921` 1921 und `NeutralSect_3988` 3988 (AST) · `TheSpire_3892` 3892 (AST-Karte) ·
`ImprovisedFinish` 2697 (DNC) · `DivineCaress` 3903 (WHM). Fünf Occult-Crescent-Ids:
`OccultUnicorn` 4243, `BlessedRain` 4253, `MagicShell` 4788, `SteadfastStance` 4800, `Lance` 5319.
Die übrigen 46 sind PvP-Formen, entfernte Nocturnal-Sekt-Status des Astrologen oder Duty- und
Gegenstandseffekte ohne Spieleraktion.

**Das Prüfmittel trägt die Erkenntnis jetzt selbst.** `scan13.py` liest zusätzlich `ActionId.resx`
und schreibt hinter jeden Kandidaten, was die Aktion gleichen Namens in PvE tut — „PvE barrier",
„PvE action grants no barrier", „PvP only", „no action of this name" — und listet am Ende die
PvE-Barrieren gesondert auf. Der Selbsttest deckt alle vier Fälle an konstruierten Aktionen ab,
darunter genau den Fall, der den Fehler ausgelöst hat: Aquaveil mit Barrierestatus und
mindernder PvE-Aktion. Ohne diese Ergänzung wäre die Korrektur eine Einzelfallbehebung geblieben
und derselbe Fehlschluss bei der nächsten Erweiterung erneut möglich.

**Empfehlung:** die zehn Jobbarrieren aufnehmen, die fünf Occult-Ids zurückstellen. Die zehn sind
Instanzen derselben Defektklasse, die A37 behoben hat — eine fehlende Id kehrt die Antwort um,
statt sie zu vergröbern — und liegen im Nutzungsprofil. Die Occult-Ids sind harmlos, aber ohne
Nutzen, solange der Inhalt nicht gespielt wird. **Nicht umgesetzt**, weil der Auftrag Prüfung,
Begründung und Empfehlung verlangt hat und die Aufnahme selbst nicht freigegeben ist.

**Erreichter Prüfgrad:** statische Prüfung gegen `Status.resx` und `ActionId.resx`, Prüfskript mit
Selbsttest, CI-Kompilierung. Die Wirkung im Spiel ist nicht beobachtet.

---

### A42 · Durchsicht aller offenen Punkte mit Empfehlung (08.09.2026)

**Anlass:** Auftrag, die offenen Punkte zu prüfen, zu begründen und Empfehlungen auszusprechen.
Jeder Eintrag aus `TODO.md` trägt danach eine Empfehlung; die beiden Punkte mit neuem Messergebnis
stehen als A40 und A41 gesondert.

**Neu gemessen wurde einer:** `SpreadDamagePaths`. Der Einführungs-Commit lag außerhalb des flachen
Klons, die Historie war dafür zu vertiefen (`git fetch --deepen`, 152 → 1844 Commits). Ergebnis:
`33e6acb1` vom 07.05.2026, ein Sammel-Refactoring, legt die Liste **bereits vollständig so** an, wie
sie heute ist — mit den beiden Einträgen, die wortgleich in `SharedDamagePaths` stehen, und mit dem
von dort übernommenen Kommentar „Duty-specific AOE share markers" über einer Liste namens *Spread*.
Das ist *Ignorant Surgery* nach Parnas, kein späteres Veralten. Die Zuordnung der beiden
einzigartigen Pfade bleibt unbelegt: Eine Websuche nach `x6fd_loc04m_5s1v` und `m0922tar_a0w`
liefert nichts.

| Offener Punkt | Empfehlung | Tragender Grund |
|---|---|---|
| Barrieregruppen ohne Vertreter | zehn Jobbarrieren aufnehmen, fünf Occult-Ids zurückstellen | belegte Defektklasse im Nutzungsprofil (A41) |
| ChurinDNC-Vorzeichen | nicht bearbeiten | fremde Rotation, Behebung verlangt die Entscheidung des Autors |
| Statusfelder auf der falschen Seite | liegen lassen | alle vier Reste in PvP/Bozja, Klasse erhoben und durch `scan11.py` gesichert |
| `CanEarlyWeave` | belassen | beide Wiederherstellungen verschieben fremdes Verhalten |
| Doppelte Zustandswahl | belassen | Zustandsautomat statisch nicht abzusichern |
| `AutodutyUpdateState` | nicht eigens angehen | Kosten fallen erst mit dem Umbau an, der oben nicht empfohlen ist |
| Selbstlernende AoE-Liste | Nullvariante (entschieden, A24–A26) | Kosten-Nutzen trägt nicht, UI-Kopplung über vier Listen |
| `SpreadDamagePaths` | Nullvariante | kein Verbraucher für die Unterscheidung, Zuordnung der Pfade unbelegt |
| Release-Ballast | nicht aufgreifen | Upstream liefert denselben Ballast, Verpackungspfad ungeprüft (A40) |
| Staging-Kanal in `build.yaml` | folgen, bis Upstream zurückwechselt | Prüfung muss das Auslieferungsartefakt messen (A40) |
| VPR-Leerzweig | belassen | der leere Zweig ist der Hinweis auf die Lücke |
| Zwei entfernte öffentliche Member | mit dem nächsten Tag erledigen | „Unreleased" braucht eine vergebene Nummer |
| Plugin-Identität | belassen | geteilte Identität erhält die Nutzerkonfiguration beim Wechsel |
| Mitigations-Synergie Schritt 3 | warten | Übertragung vor dem Nachweis vervielfacht einen möglichen Fehler |
| Messgrundlage für Raten | nicht bauen (entschieden, A29) | kein Verbraucher mehr, Kosten bei allen Nutzern |
| Codebasis-Audit Phasen 5+ | **nächster Arbeitsblock** | einziger Punkt ohne Vorbedingung |

**Das Bild, das die Durchsicht ergibt:** Von sechzehn Punkten wartet genau einer auf nichts — der
zweite Durchgang der dreizehn Prüfskripte über den bereinigten Baum. Zwei warten auf eine Freigabe
(die zehn Barrieren, die fünf Occult-Ids), fünf auf Laufzeitbeobachtung, vier sind erfasste
Fremdbefunde mit dem Upstream als Adressat, drei sind bereits entschiedene Nullvarianten, und einer
hängt am nächsten Release-Tag. Es gibt keinen offenen Punkt, der einen belegten Defekt im
Nutzungsprofil unbehandelt lässt — außer den zehn Barrieren, deren Aufnahme freizugeben ist.

**Erreichter Prüfgrad:** statische Prüfung und Versionsgeschichte, für `SpreadDamagePaths`
zusätzlich eine erfolglose externe Recherche. Keine Laufzeitbeobachtung.

---

### A43 · Die fünfzehn fehlenden Barrieren aufgenommen und die Lücke verriegelt (08.09.2026)

**Anlass:** Freigabe des Auftraggebers, den Empfehlungen aus A42 zu folgen, mit der Angabe, dass er
Occult Crescent spielt — womit auch die fünf zurückgestellten Ids in den Auftrag fallen. Vorgabe:
kritische Umsetzung im vollständigen Loop.

**Umgesetzt:** `StatusHelper.ShieldStatus` von 45 auf 60 Ids. Zehn Jobbarrieren — `ShakeItOff`
1457/1993, `SeraphicVeil` 1917/2040/3097, `NeutralSect_1921`/`_3988`, `TheSpire_3892`,
`ImprovisedFinish` 2697, `DivineCaress` 3903 — und fünf Occult-Crescent-Barrieren `OccultUnicorn`
4243, `BlessedRain` 4253, `MagicShell` 4788, `SteadfastStance` 4800, `Lance` 5319. Jede einzeln
gegen `Status.resx` (Wirkbeschreibung) und `ActionId.resx` (die PvE-Aktion erzeugt eine Barriere)
belegt.

**Wirkungsbereich vor der Änderung erhoben, nicht danach.** `ShieldStatus` hat genau einen Leser,
`HasSurvivingShield`; dieser hat genau zwei, `StateUpdater.cs:719` und `:776`. Beide heben dort die
Gesundheitsquote auf `GetEffectiveHpPercent` an — und beide nur, wenn `ShieldCreditAllowed` gilt,
also eine BMR-Vorhersage oder ein erkannter Cast tatsächlich Schaden erwarten lässt. Die Änderung
wirkt damit ausschließlich in Situationen, in denen ein Schild gegen einen konkreten Treffer
gerechnet wird, nicht im Leerlauf.

**Der Nebenbefund, den erst diese Erhebung sichtbar gemacht hat.** `WillStatusEnd` stützt sich auf
`StatusTime`, und das liefert das **Minimum** über alle vorhandenen gelisteten Status. Beantwortet
wird also „laufen *alle* Barrieren noch?", während der Doku-Kommentar „has an active shield that
will still be up" sagt. Für einen Träger mehrerer Barrieren — in einer Gruppe mit Heiler der
Regelfall — bedeutet das: Sobald die kürzeste unter den Horizont fällt, gilt er als ungeschützt.

Damit stand die Frage, ob die Aufnahme netto schadet, denn fast alle fünfzehn sind
**Gruppen**barrieren und liegen typischerweise zusätzlich zu einem Heilerschild. Die Antwort ist
nein, und sie folgt aus der Richtung des Fehlers: Beide Abweichungen — die behobene wie die
verbliebene — führen zu **überflüssiger Heilung**, nie zu ausbleibender. Der bisherige Zustand
verlor die Anrechnung, sobald gar kein gelisteter Status vorlag; der neue verliert sie, wenn die
kürzeste von mehreren ausläuft. Ein Ziel, das nur eine der neu aufgenommenen Barrieren trägt —
etwa ein DPS unter Shake It Off ohne Heilerschild —, gewinnt die Anrechnung vollständig. Der
Nebenbefund ist als eigener Defekt in `TODO.md` erfasst, mit der Begründung, warum die naheliegende
Umkehr auf das Maximum ihn nur austauscht: Unterschätzung kostet eine Heilung, Überschätzung einen
Tod.

**Verworfene Optionen.** *Nullvariante:* lässt eine belegte Inversion im Nutzungsprofil stehen.
*Nur die zehn Jobbarrieren:* der Auftraggeber spielt Occult Crescent, die fünf sind Party-Barrieren
auf denselben Zielen. *Die Aufzählung durch eine Erzeugung ersetzen* — ein Generator, der
`ShieldStatus` aus den Wirkbeschreibungen bildet: verworfen, weil die Trennung PvE/PvP an der
Aktionsbeschreibung hängt und damit heuristisch bleibt; eine Fehlklassifikation wäre dann still und
im Code nicht mehr sichtbar. Stattdessen bleibt die Liste explizit, und die Alterung wird an der
Schranke abgefangen.

**Die Wiederholbarkeit ist adressiert, nicht nur der Fundort.** `scan13.py` endet jetzt mit
Rückgabewert 1, sobald ein Kandidat mit dem Label „PvE barrier" ungelistet ist, und läuft im
`DispatchChain`-Job von `build.yaml` — dem Job ohne .NET, der in Sekunden antwortet; der Scan
selbst braucht 0,1 s. Das ist die Antwort auf *Lack of Movement*: Die Liste war bei ihrer
Entstehung richtig und wurde durch Erweiterungen anderswo unvollständig, **ohne dass etwas
fehlschlug**. Jetzt schlägt etwas fehl.

**Nachweis.** Vor der Änderung meldete der Scan 15 ungelistete PvE-Barrieren, danach 0; fehlende
Geschwister 0; verbliebene Gruppen 44 mit 46 Ids, keine davon mit PvE-Barriereaktion. Gegenprobe am
konstruierten Defekt: `ShakeItOff` wieder entfernt → Rückgabewert 1 und die Id wird benannt;
wieder eingefügt → 0. Selbsttest des Skripts unverändert grün.

**Betroffenenkreis benannt.** Gruppe **R**: `ShieldStatus` ist öffentlich, die Signatur bleibt, der
Inhalt ändert sich — eine abgeleitete Rotation, die die Liste liest, sieht mehr Ids und damit
weniger Ziele als ungeschützt. In `CHANGELOG.md` eingetragen, weil die Versionsnummer das nicht
ausdrücken kann. Gruppe **U**: eine weitere Zeile in `build.yaml`, die Upstream nicht hat.

**Erreichter Prüfgrad:** statische Prüfung gegen beide Ressourcendateien, Prüfskript mit Selbsttest
und Gegenprobe, CI-Kompilierung, CI-Schranke. Nicht geprüft: die Wirkung im Spiel — dafür wäre zu
beobachten, ob eine Heilung auf einen beschildeten Träger ausbleibt.

---

### A44 · The Blackest Night: Verdrahtung nachgeholt, Zeitpunkt wählbar gemacht (09.09.2026)

**Anlass:** Frage des Auftraggebers aus dem Spiel — wann und ab wie vielen Gegnern der Dunkelritter
The Blackest Night nutzt; die Barriere werde in diesen Lagen nie aufgezehrt. Danach der Auftrag,
ein Konzept im Loop zu erstellen und umzusetzen. Konzept: `docs/rotation-flow/10-drk-blackest-night.md`.

**Die Zahl, die die Frage beantwortet.** Aus der Aktionsbeschreibung (`ActionId.resx` 7393):
Barriere = 25 % der maximalen Gesundheit, Dauer 7 s, Dark Arts **nur bei vollständiger Absorption**.
Daraus folgt eine ausrüstungs- und inhaltsunabhängige Schwelle, weil beide Seiten an der maximalen
Gesundheit hängen: aufgezehrt wird die Barriere ab **rund 3,6 % der maximalen Gesundheit pro
Sekunde** eingehenden Schadens — gemessen **nach** allen Minderungen. Die Umrechnung auf eine
Gegnerzahl ist danach reine Arithmetik (`n = 3,6 / x` bei *x* % je Gegner und Sekunde); der Wert
*x* selbst ist aus dem Repository nicht zu belegen und bleibt als solcher gekennzeichnet.

Belegbar ist dagegen die Richtung: Die Rotation wirkt **Oblation (−10 %) unmittelbar vor** The
Blackest Night — Priorität 10 gegen 20 im selben Pfad —, und jede Minderung erhöht die nötige
Gegnerzahl um denselben Anteil. Die Reihenfolge des Pfades arbeitet also gegen die Bedingung, unter
der sich die Fähigkeit bezahlt macht. Der Tank-Haltung ist das nicht anzulasten: Grit erhöht laut
Beschreibung ausschließlich die Feindseligkeit.

**Zwei Ursachen, beide in Upstream-Code, beide behoben:**

1. **`DRK_Reborn.cs:151`** legte The Blackest Night auf das Party-Mitglied mit den niedrigsten HP,
   ohne `BlackLantern` zu lesen — die Option, die genau das schalten soll und ausgeliefert **aus**
   ist. Sie war deklariert und als `Parent` des Schwellwerts genannt, sonst nirgends. Dreifach
   belegte Absicht: Optionstext, die Oblation-Nachbarzeile mit `OblationLantern &&`, und
   `ChurinDRK.cs:173` mit `BlackLantern &&` am selben Zweig. Verdrahtung nachgeholt. Verschärfend
   war die Oberfläche: `Parent = nameof(BlackLantern)` blendet den Schwellwert aus, solange der
   Schalter aus ist — die einzige Stellschraube des laufenden Zweigs war unsichtbar.
2. **`DRK_Reborn.cs:225`** zog die Fähigkeit im Selbstschutzpfad ohne eigene Bedingung. Der Pfad
   öffnet mit `AutoStatus.DefenseSingle`, und dessen Tank-Zweig genügt entweder zwei Gegner im
   Nahbereich — die begleitende Gesundheitsbedingung steht per Vorgabe auf 100 % und schränkt nichts
   ein — oder ein einziges `IsHostileCastingToTank`, das über den Rückfall „castet auf sein eigenes
   Ziel" jeden nicht unterbrechbaren Trash-Cast trifft. Neue Rotationsoption `BlackestNightUsage`
   mit drei Stufen; Voreinstellung ist das heutige Verhalten.

**Die Prüfgröße ist zentral abgelegt.** `CustomRotation.TankbusterOnMe` fasst
`IsHostileCastingTankBusterAtMe` und `BMRTankbusterImminent` zusammen und ist damit für jede
Rotation verfügbar. Bewusst **nicht** `IsHostileCastingToTank` — dieselbe Unterscheidung, die C10
für diese beiden Größen bereits herausgearbeitet hat.

**Verworfen, mit Begründung im Konzept:** der Eingriff am zentralen Auslöser (Wirkungsbereich, C9);
eine Messung des Schadensflusses (dieselbe Kostenrechnung, die den Messbaustein verworfen hat, und
der Fluss der letzten Sekunden sagt nichts über die nächsten sieben); eine reine MP-Schwelle (trifft
die falsche Größe); eine Sperre, solange Dark Arts anliegt (die Barriere bleibt wertvoll, und die
Sperre griffe in der Lage, in der der Tank Schutz braucht); die Nullvariante.

**Zwei Randbedingungen mitgeprüft.** Der Countdown-Zweig bleibt außen vor — dort gibt es keinen
Tankbuster, eine engere Stufe würde Dark Arts für die Eröffnung verlieren. Und Rotationsoptionen
speichern Enums als **Namen**, nicht als Ordinalzahlen (`RotationConfigBase.cs:208`); die
Reihenfolge der Stufen ist damit frei, ihre Bezeichner sind Vertrag.

**Erreichter Prüfgrad:** statische Selbstprüfung entlang der vollständigen Kette (Aktionstext →
`AutoStatus.DefenseSingle` → Rotationszweig), Spielbeobachtung des Auftraggebers als Anlass,
CI-Kompilierung. Nicht beobachtet: ob die engeren Stufen im Spiel besser abschneiden — deshalb
steht die Voreinstellung auf dem alten Verhalten, und der Punkt bleibt als technische Schuld in
`TODO.md`.

---

### A45 · Wall-to-Wall ist die zweite Lage, und sie verlangt das Gegenteil (09.09.2026)

**Anlass:** Einwand des Auftraggebers gegen A44 — The Blackest Night soll auch im
Wall-to-Wall-Pull genutzt werden, dort aber allein, damit die Minderung „so effizient und lang wie
möglich" ausfällt. Der Einwand trifft, und er deckt zwei Fehler in A44 auf.

**Erster Fehler: die Stufe hätte die Lage ausgeschlossen.** `TankbusterOnly` verlangte einen
erkannten oder vorhergesagten Tankbuster. In einem Trash-Pull gibt es keinen — die Fähigkeit wäre
dort **nie** gekommen, obwohl gerade dort der Schadensstrom die Barriere aufzehrt. Die Stufe heißt
jetzt `TankbusterOrHeavyPull` und lässt beide Lagen zu.

**Zweiter Fehler: die pauschale Ablehnung der Staffelungsregel** (C29). Geprüft worden war sie
gegen die Buster-Lage, wo Stapeln richtig ist; das Ergebnis wurde ungeprüft auf die Dauerschaden-Lage
übertragen. Dort gilt das Gegenteil, aus zwei unabhängigen Gründen: Zwei Minderungen gleichzeitig
decken dieselben Sekunden doppelt und lassen den Rest ungedeckt — nacheinander gelegt decken sie die
doppelte Zeit —, und die parallele Minderung senkt den Strom unter die Rate, die die Barriere in
sieben Sekunden aufzehrt (3,6 % der maximalen Gesundheit pro Sekunde ohne Minderung, 5,1 % unter
Shadow Wall, 6,0 % unter Shadowed Vigil).

**Die Konstruktion war schon da.** `ShadowWallPvE` und `ShadowedVigilPvE` tragen
`StatusProvide = StatusHelper.RampartStatus` (`DarkKnightRotation.cs:238`, `:404`) und überlappen
sich deshalb nie — die Staffelung ist im Projekt etabliert, The Blackest Night stand nur außerhalb.
Es kann sie über `StatusProvide` auch nicht ausdrücken, weil sein eigener Status eine Barriere ist
und kein Minderungsstatus. Deshalb zwei neue Prüfgrößen in `CustomRotation_OtherInfo`:
`HasMajorMitigation` liest dieselbe `RampartStatus`-Liste, `InHeavyPull` misst
`NumberOfHostilesInRange >= MitigationSustainHostileCount` (in A47 durch eine eigene Option
ersetzt).

**Keine neue Zahl.** Die Gegnerschwelle ist die vorhandene aus der Mitigations-Sustain-Regel
(Vorgabe 4), nicht ein zweiter Schwellwert daneben. Dass vier Gegner die Rate von 3,6 % erreichen,
bleibt eine Annahme — sie entspricht rund 0,9 % je Gegner und Sekunde und ist hier nicht belegbar.

**Grenze der Regel, benannt statt verschwiegen.** Während einer langen Minderung fällt ein Fenster
von The Blackest Night aus; die MP fließen dann in Edge of Shadow. Wer die Zahl der
Dark-Arts-Auslösungen maximieren wollte, müsste die Minderungskette auflösen — das Gegenteil einer
streckenden Abdeckung. Die Staffelung gilt deshalb nur im Pull-Zweig: beim Tankbuster bleibt Stapeln
richtig, und der Notfallzweig bei niedriger Gesundheit wartet nicht auf das Ende einer Minderung.

**Erreichter Prüfgrad:** statische Prüfung, Konzept `docs/rotation-flow/10-drk-blackest-night.md`
neu gefasst, CI-Kompilierung. Die Wirkung im Spiel ist nicht beobachtet; die Voreinstellung bleibt
deshalb das alte Verhalten.

---

### A46 · Reflexion vor der Barriere: die Reihenfolge im Verteidigungspfad (10.09.2026)

**Anlass:** Auflösung der offenen Namensfrage aus A45 durch den Auftraggeber — die deutschen
Bezeichnungen gehören zu den **Tank-Rollenaktionen**. Belegt per Websuche: **Reflexion = Reprisal**,
**Abtausch = Shirk**. Der eigene Nullbefund davor war im falschen Suchraum entstanden (C30).

**Der Befund am Artefakt.** `DefenseSingleAbility` gibt je Gelegenheit genau eine Aktion zurück und
arbeitet von oben nach unten: Oblation (10) · **The Blackest Night (20)** · Dark Mind · Shadowed
Vigil/Shadow Wall · Rampart · … · **Reprisal (`:296`, `:301`)**. Reprisal steht damit am Ende, die
Barriere fast am Anfang. Da sie nur 15 Sekunden Abklingzeit hat, gewinnt sie nahezu jede
Gelegenheit, und Reprisal landet erst, wenn sie zufällig nicht verfügbar ist.

Für einen Pull ist das die verkehrte Rangfolge, und die Gegenüberstellung ist eindeutig:

| | Reprisal (Reflexion) | The Blackest Night |
|---|---|---|
| Kosten | nur Abklingzeit | 3000 MP |
| Wirkung | −10 % Schaden **aller** Gegner | Barriere über 25 % der maximalen Gesundheit |
| Reichweite | die ganze Gruppe | ein Charakter |
| Dauer | 15 s (ab Stufe 98) | 7 s |

**Umgesetzt:** Der Pull-Zweig verlangt zusätzlich, dass Reprisal bereits liegt, auf Abklingzeit ist
oder mangels Stufe ausfällt. Gegen einen Tankbuster gilt die Bedingung nicht — dort entscheidet eine
einzelne Gelegenheit, und die Rangfolge zwischen beiden ist gleichgültig.

**Nicht umgesetzt, und warum:** die Reihenfolge im Pfad selbst umzustellen. Das wäre der direktere
Weg, ändert aber das Verhalten für alle Nutzer und alle Lagen, während die Bedingung im Zweig hinter
der Option `BlackestNightUsage` bleibt und nur greift, wer sie umstellt. Als Vorlage erfasst.

**Abtausch (Shirk) gehört nicht in die Minderungskette.** Die Aktion überträgt Feindseligkeit und
mindert keinen Schaden; RSR führt sie zentral über `AutoStatus.Shirk`
(`CustomRotation_Ability.cs:123`). Kein Eingriff nötig.

**Erreichter Prüfgrad:** statische Prüfung der Pfadreihenfolge, Websuche für die Namenszuordnung,
CI-Kompilierung. Nicht beobachtet: wie oft Reprisal dadurch im Spiel tatsächlich vorzieht.

---

### A47 · Die Pull-Bedingung vollständig gemacht: Betäubung, Gegnerzahl, eigene Minderungen (10.09.2026)

**Anlass:** Der Auftraggeber stellt klar, worum es ihm von Anfang an ging. Beim Tankbuster ist alles
richtig — die Fähigkeit soll kommen, die Barriere wird aufgezehrt, das Stapeln mit anderen
Verteidigungen ist korrekt. Sein Problem ist ausschließlich der Wall-to-Wall-Pull: Dort sei die
Fähigkeit so stark, dass sie mit anderen Verteidigungen interferiert; wenn der Heiler Sanctus wirkt
und betäubt, bringe sie gar nichts; sie solle allein stehen und erst ab mehr als drei Gegnern
kommen. Vorgabe: alles erneut im vollständigen Loop prüfen, das Konzept optimieren, Fehler
beseitigen.

**Abgleich mit dem Stand aus A45/A46 — drei Punkte trafen bereits zu, drei fehlten.** Zutreffend
waren die Trennung der beiden Lagen, die Staffelung gegen große Minderungen und die Reprisal-Vorfahrt.
Es fehlten:

1. **Die Betäubung.** Sanctus — Holy, ab Stufe 82 Holy III — betäubt 4 Sekunden im Umkreis von acht
   Yalm (`ActionId.resx` 139, 25860). In dieser Zeit kommt **kein** Schaden, die Barriere verfällt
   also vollständig statt nur langsamer zu verfallen. Damit ist die Betäubung nicht ein weiterer
   Minderungsfall, sondern dessen Grenzwert. Neue Prüfgröße `AnyHostileStunned(radius)`.
2. **Die Gegnerzahl als eigene Größe.** A45 hatte sie an `MitigationSustainHostileCount` gekoppelt,
   um „eine Zahl statt zwei" zu haben. Das war die falsche Sparsamkeit: Die Sustain-Zahl beantwortet
   die Frage nach der Debuff-Aufrechterhaltung, hier geht es um die Verbrauchsrate einer Barriere.
   Jetzt eigene Option `BlackestNightMinHostiles`, Vorgabe 4 — „mehr als drei", wie verlangt.
3. **Der Verweis auf die Erwartung des Auftraggebers**, die Zahl möge den Verbrauch *sichern*. Das
   kann sie nicht, und das steht jetzt im Konzept: Ohne den Schaden je Gegner ist keine Schwelle
   beweisbar; die Zahl hält nur Lagen heraus, in denen sicher zu wenig kommt.

**Die harte Fassung des Wunsches ist nicht erfüllbar, und das ist gerechnet.** „Keine andere
Verteidigung darf laufen" scheitert an den Dauern: Reprisal 15 s, Oblation 2 × 10 s, Dark Mind 10 s,
Dark Missionary 15 s, Rampart 20 s, Shadow Wall/Vigil 15 s — zusammen **95 Sekunden**, jede Fähigkeit
nur einmal gewirkt. Ein Wall-to-Wall-Pull dauert selten so lange. Sperrte jede davon die Barriere,
käme sie praktisch nie. Die Grenze verläuft deshalb bei der Wirkungsstärke: Die schwachen (10 %)
heben die nötige Rate um ein Neuntel, die starken (ab 20 %) um ein Viertel bis zwei Drittel.
`StatusHelper.RampartStatus` führt genau die starken. Oblation bleibt zusätzlich deshalb außen vor,
weil es im selben Pfad **vor** der Barriere steht und diese sonst hinter der eigenen Vorgängerin
hängen bliebe.

**Der Pull-Zweig verlangt jetzt vier Dinge:** genug Gegner · keine große Minderung aktiv · keine
laufende Betäubung im Acht-Yalm-Umkreis · Reprisal erledigt. Für den Tankbuster gilt keine davon.

**Gewählt wurde die Tatsache, nicht die Prognose.** Geprüft wird, ob **gerade** betäubt ist, nicht ob
noch betäubt werden *könnte*. Letzteres ist eine Aussage über den nächsten Zauber eines anderen
Spielers und würde die Fähigkeit über den ganzen frühen Pull sperren — die Phase mit dem höchsten
Schadensdruck.

**Vorbedingung eingehalten:** Upstream war zwischenzeitlich zwei Commits weiter (Tag `7.5.6.1`,
„Fixes for Dalamud version 15.0.3.4"); vor der Codeänderung gemergt, danach 0 / 355.

**Erreichter Prüfgrad:** statische Prüfung, Aktionstexte als Quelle für alle Dauern und Radien,
CI-Kompilierung. Nicht beobachtet: ob vier Gegner die Verbrauchsrate im gespielten Inhalt erreichen —
dafür ist die Zahl einstellbar.

---

### A48 · Gruppenbetäubung: Quantor und Bezugsmenge nachgezogen (10.09.2026)

**Anlass:** Rückfrage des Auftraggebers, ob außer dem Weißmagier andere Heiler betäuben können —
und, nach der ersten Korrektur, die Feststellung: „Es geht um Gruppenstuns, nicht um einzelne
Gegner. Du arbeitest zu oberflächlich." Die Kritik trifft; die Bedingung war zweimal falsch gefasst
(C31).

**Die Erhebung, die die Frage beantwortet.** Alle PvE-Aktionen mit Betäubungswirkung, nach Rolle:

| Rolle | Aktion | Wirkung | Dauer |
|---|---|---|---|
| Heiler | Sanctus (Holy), Holy III — **nur Weißmagier** | Fläche, 8 Yalm | 4 s |
| Tank | Schildhieb — nur Paladin | Einzelziel | 6 s |
| Tank | **Tiefschlag — alle Tanks, auch der Dunkelritter** | Einzelziel | 5 s |
| Nahkampf | Fußfeger | Einzelziel | 3 s |
| Occult Crescent | Occult Falcon, Mineuchi, Variant Ultimatum | Fläche / Einzelziel | 4–6 s |

Gelehrter, Astrologe und Weiser haben keine. Eine Fallunterscheidung nach Gruppenzusammensetzung
braucht die Regel dennoch nicht: Sie misst den Status **auf den Gegnern**. Ohne Betäubung ist die
Bedingung nie erfüllt.

**Der eigentliche Befund liegt im Quantor.** Der Dunkelritter trägt Tiefschlag selbst, und RSR
wirkt es über den Unterbrechungspfad (`CustomRotation_Ability.cs:575`). Damit scheitern beide
naheliegenden Formulierungen: „irgendein Gegner betäubt" hätte die eigene Unterbrechung die eigene
Barriere sperren lassen; „alle Gegner betäubt" fällt um, sobald ein Nachzügler unbetäubt zur Gruppe
stößt, obwohl der Strom erkennbar steht. Maßgeblich ist der **Anteil** — mindestens zwei betäubte
Gegner und mindestens die Hälfte der Gegner in Reichweite. Eine Flächenbetäubung erfüllt das, eine
Einzelbetäubung nicht.

**Zweiter Fehler derselben Oberflächlichkeit: die Bezugsmenge.** Die Betäubungsprüfung lief über
acht Yalm, die Gegnerzahl über die Jobreichweite (`DataCenter.JobRange`, für Tanks drei Yalm) —
zwei verschiedene Mengen für zwei Bedingungen, die dieselbe Frage beantworten sollen. Beide messen
jetzt über die Jobreichweite: die Gegner, die tatsächlich zuschlagen.

**Umgesetzt:** `SurveyStuns` erhält eine **Überladung** mit der Trefferzahl (`out int
stunnedCount`) — additiv, die vorhandene Signatur bleibt und ruft die neue auf, damit kein
Paketbruch für Gruppe R entsteht. `GroupStunRunning()` in `DRK_Reborn` wertet Anteil und
Nachlauffenster aus; `AnyHostileStunned` ist wieder entfernt.

**Was der Auftraggeber zum zeitlichen Verlauf ergänzt hat**, ist im Konzept aufgenommen: Am Ende des
Pulls steht die Gruppe beieinander, die Gegnerzahl ist am höchsten, und der Dunkelritter hat seine
großen Minderungen typischerweise noch nicht gezogen — die Bedingungen „genug Gegner" und „keine
große Minderung" treffen also im selben Moment zu. Die Betäubungsphase fällt oft mit ebendiesem
Moment zusammen; sie schiebt auf statt auszuschließen und endet mit der Betäubungsimmunität.

**Erreichter Prüfgrad:** statische Prüfung, vollständige Erhebung der Betäubungsaktionen aus
`ActionId.resx`, CI-Kompilierung. Nicht beobachtet: der Anteilsschwellwert im Spiel — ob eine halbe
betäubte Gruppe den Strom weit genug drückt, ist eine Annahme.

---

### A49 · Statuslisten mit fehlenden Geschwistern: aus dem Einzelfall wird eine Defektklasse (10.09.2026)

**Anlass:** Die Ergänzung des Auftraggebers, auch andere Gruppenmitglieder — meist physische
Schadensklassen — trügen Betäubungen. Die Prüfung der Frage ergab, dass die Regel davon nicht
berührt ist: `StatusHelper.StunStatus` führt **alle 18** Ids mit dem Anzeigenamen „Stun", und die
Bedingung misst den Status auf den Gegnern, nicht die Gruppenzusammensetzung. Quelle der
Betäubung ist damit gleichgültig. Die Erhebung, die das belegen sollte, deckte jedoch eine größere
Sache auf.

**Der Befund ist die Bauform, nicht die Liste.** `ShieldStatus` war in A43 unvollständig, weil das
Spiel jede Wirkung unter mehreren Ids desselben Anzeigenamens führt und eine handgepflegte
Aufzählung nach der nächsten Erweiterung still veraltet — Parnas' *Lack of Movement*. Dieselbe
Bauform tragen alle 24 Listen in `StatusHelper.cs`. Die Frage einmal an alle gestellt
(`scan14.py`, neu): **187 fehlende Geschwister über 17 Listen**.

**Zwei davon sind Defekte im Sinn der umgekehrten Antwort und sind behoben.** Maßstab war nicht die
Fundzahl, sondern ob eine Wirkkette im Code die Liste liest:

- **`RampartStatus`** ohne `Rampart_1978` — die Fassung, die ein Tank ab Stufe 94 trägt
  (Geltungsbereich „PLD WAR DRK GNB", Wirktext um die Heilaufwertung der Trait ergänzt). Zwei
  Verbraucher waren dadurch blind: das in A47 eingeführte `HasMajorMitigation`, und die bereits
  vorhandene `StatusProvide`-Staffelung, die Shadow Wall und Shadowed Vigil von einem laufenden
  Rampart fernhält. Aufgenommen wurden `Rampart_1191`, `Rampart_1978`, `Rampart_4168` und
  `HallowedGround_1302`.
- **`ReprisalStatus`** ohne `Reprisal_2101`. Belegt über die Aktion, nicht über den Namen: In
  `ActionId.resx` steht nur `ReprisalPvE` (7535), eine PvP-Form existiert nicht — der
  Geltungsbereich „PLD WAR DRK GNB" statt der geteilten Rolle GLA MRD PLD WAR DRK GNB ist hier
  also die Signatur der Trait-Fassung. *Enhanced Reprisal* hebt auf Stufe 98 die Minderung auf
  15 % und die Dauer auf 15 s. Betroffen sind alle vier Tanks: `ReprisalPvE` trägt die Liste als
  `TargetStatusProvide` (`CustomRotation_Actions.cs:73`), die Sperre gegen erneutes Anwenden sah
  die Schwächung eines Endstufen-Tanks nie; dazu `ShouldSustainMitigationDebuff` in PLD/WAR/DRK/GNB
  und die Minderungsbilanz in `GetCurrentMitigationPercent`. Für den laufenden Auftrag zählt die
  Bedingung `reprisalDone` in `ShouldUseBlackestNightOnSelf`, die genau diesen Status liest.

**Was bewusst draußen bleibt, meldet der Scan weiter**, und das ist Absicht: `Nebula_3051` und
`Bloodwhetting_3030` teilen den Namen einer Minderung, sind aber deren Reflexions- und
Lebensraubhälfte; `Holmgang` 88 und 1305 sitzen auf dem *Ziel* der Unverwundbarkeit (C15). Ein
gemeinsamer Anzeigename macht zwei Ids nicht zur selben Wirkung — der Scan druckt deshalb zu jedem
Kandidaten die Wirkbeschreibung und die Marke `same opening` / `differs` und **entscheidet nicht**.

**Keine CI-Schranke, anders als bei `scan13.py`.** Dort ist die Mitgliedschaft aus der Aktion
maschinell entscheidbar; hier sind von den 186 verbliebenen Treffern 114 Rauschen aus zwei Listen,
die bewusst Teilmengen sind (`PhantomDispellable`, `PurifyPvPStatuses`). Ein Rückgabewert, den man
nur durch Wegsehen grün hält, wäre schlechter als keiner.

**Grenze der Erhebung, offen geführt:** Der Scan sieht nur Ids in den Listen. Prüfpunkte, die eine
einzelne Id direkt nennen, altern genauso und sind nicht erfasst — belegt an
`GetCurrentMitigationPercent`, das `StatusID.Addle` und `StatusID.Feint` bar liest, während
`Addle_1988` (Geltungsbereich BLM SMN RDM BLU PCT, keine PvP-Aktion) und `Feint_2185` existieren.
Ebenso offen ist `TankStanceStatus`: geführt sind `IronWill` (79) und `RoyalGuard_1833`, nicht aber
`IronWill_393`, `IronWill_2843` und `RoyalGuard` (392) bei gleichem Wirktext „Enmity is increased."
Welche Id das Spiel heute setzt, ist aus den Daten nicht zu entscheiden; `Defiance_1396` und
`Grit_1397` („Damage dealt and taken are reduced.") sind erkennbar die Fassungen vor Shadowbringers
und gehören nicht hinein. Beides ist in `TODO.md` erfasst, nicht bearbeitet — die Zuordnung
verlangt Laufzeitbeobachtung, und der Auftrag nennt sie nicht.

**Erreichter Prüfgrad:** statische Prüfung, Prüfskript mit Selbsttest gegen einen konstruierten
Rampart-Defekt, Zuordnung der Reprisal-Fassung über `ActionId.resx` und eine Websuche zum
Trait-Stufenwert, CI-Kompilierung. Nicht beobachtet: welche Id das Spiel je Stufe tatsächlich
setzt — die Regel fragt deshalb nach *irgendeiner* der Fassungen.

---

### A50 · Kein Blocker für Sanctus bei laufender Barriere — und ein Fehlauslöser in der Betäubungsstreckung (10.09.2026)

**Anlass:** Frage des Auftraggebers, ob der Weißmagier sein Sanctus zurückhält, während die Barriere des Dunkelritters läuft.

**Antwort am Artefakt: nein, an keiner Stelle.** Erhoben wurde der gesamte Wirkungsbereich der Aktion:

- `HolyPvE` und `HolyIiiPvE` tragen in `WhiteMageRotation.cs:202` und `:353` nur `IsFriendly = false`, die Freischaltquest und `AoeCount = 3` — keine `StatusNeed`, keine `TargetStatusNeed`, keine Sperre über einen fremden Status.
- Die einzige Aussetzbedingung ist `ShouldStretchHolyStun()` (`WHM_Reborn.cs:489`), gelesen an genau einer Stelle (`:566`). Sie liest ausschließlich die Betäubungslage über `SurveyStuns` — Gegnerzahl im Wirkradius, betäubt, immun. Weder `ShieldStatus` noch `BlackestNight` noch der Tank kommen darin vor.
- Sie ist zudem standardmäßig **aus** (`StretchHolyStun = false`).

Die Kopplung zwischen beiden Jobs ist damit einseitig: Der Dunkelritter weicht der Betäubung aus (A48), der Weißmagier weicht der Barriere nicht aus.

**Bewertung: Die Kopplung fehlt und gehört ergänzt.** Die erste Antwort lehnte das mit zwei Gründen ab, die beide nicht tragen (C32).

Der erste war eine behauptete wechselseitige Sperre. Als Zustandsfolge ausgeschrieben gibt es sie nicht: Wartet der eine, läuft der Zustand des anderen — und beide laufen von selbst ab, die Betäubung nach vier Sekunden, die Barriere nach sieben. Ein Zustand, in dem beide aufeinander warten, ist nicht erreichbar; es gibt nur den Fall, dass beide gleichzeitig frei sind, und dann handeln beide. Was die zweite Regel bewirkt, ist kein Warten aufeinander, sondern Nachrang für den, der später kommt.

Der zweite war der Gruppenschutz. Im Wall-to-Wall-Pull liegt die Aggro beim Tank, der Schaden also auch — und derselbe Tank trägt die Barriere. Die Betäubung verhindert dort genau den Schaden, den die Barriere ohnehin aufgefangen hätte, am selben Ziel. Ein zusätzlicher Schutz entsteht nicht, verbraucht werden beide Ressourcen: die Barriere verfällt ungenutzt, damit auch Dark Arts, und das Betäubungsbudget von rund sieben Sekunden bis zur Immunität ist fort.

**Umgesetzt: `ShouldHoldHolyForBarrier()`**, hinter der Option `HoldHolyForBlackestNight` (Vorgabe aus, weil die Wirkung nicht statisch belegbar ist). Sanctus wartet, solange ein Gruppenmitglied in Tankrolle The Blackest Night trägt. Zwei Grenzen halten die Kosten klein, und die Kosten sind real — Sanctus ist der einzige Flächenzauber dieses Jobs, ein zurückgehaltener GCD fällt auf Einzelzielschaden zurück:

- **Nur solange die Betäubung noch landen könnte.** Ist jeder Gegner im Wirkradius immun (`headroom` aus `SurveyStuns`), kann der Zauber den Schadensstrom nicht mehr unterbrechen und geht normal heraus. Damit endet die Rückhaltung mit der Betäubungsimmunität — sie betrifft das Anfangsfenster des Pulls, nicht den Rest.
- **Nur solange die Barriere die Wirkzeit überdauert.** Endet sie vor dem Landen des Zaubers, ist das Warten gegenstandslos (`WillStatusEnd` gegen `Info.CastTime`).

**Eigene Statusliste statt `ShieldStatus`:** `StatusHelper.FullAbsorbRewardStatus` führt nur The Blackest Night (1178, 1308). Der Grund ist inhaltlich, nicht sparsam — jede andere Barriere ist reiner Schutz, bei dem ein unverbrauchter Rest ein gutes Ergebnis ist. Nur hier ist der vollständige Verbrauch die Bedingung einer Belohnung (Aktion 7393), und nur deshalb ist ihr Verfall ein Verlust. Die Liste ist zugleich der Ort, an dem `scan14.py` die Geschwisterfrage künftig stellt.

**Nebenbefund aus derselben Erhebung, behoben:** Die Streckungsbedingung setzte auch dann aus, wenn **kein** Gegner betäubt ist. `!headroom` heißt „kein Gegner in Reichweite, den dieser Zauber noch betäuben könnte", und verallgemeinert damit richtig von „alle betäubt" auf „zwei betäubt, einer immun". Für sich genommen trifft es aber auch die Lage nach der Betäubungskette: alle immun, keiner mehr betäubt. Dann gibt es keine Betäubung, die vor dem Überschreiben zu schützen wäre, und die Regel tauschte für den Rest des Pulls bei jedem fälligen Schadenszauber über Zeit einen Flächenzauber gegen einen Einzelzielzauber. Der Optionstext sagt „while every enemy it would hit is already stunned", der Kommentar „so the stun is not overwritten **while it still runs**" — beide decken diesen Fall nicht; ein Widerspruch zwischen Absicht und Code.

**Umgesetzt:** `stunned == 0` als zusätzliche Ausschlussbedingung, über die in A48 eingeführte `SurveyStuns`-Überladung. Der Eingriff wirkt nur einschränkend — die Regel setzt seltener aus, nie häufiger —, betrifft also den Gruppenschutz nicht.

**Erreichter Prüfgrad:** statische Prüfung, vollständige Erhebung aller Sperrstellen für beide Sanctus-Aktionen, CI-Kompilierung. Nicht beobachtet: wie oft die Immunitätslage im Spiel eintritt, bevor der Pull endet.

---

### A51 · Heiler- und Tank-Konzepte nachgeprüft, und die Verlangsamung, die niemand las (10.09.2026)

**Anlass:** Auftrag, die Konzepte 08 bis 10 erneut vollständig zu prüfen und zusätzlich zu erheben, ob weitere Synergieeffekte erreichbar sind. Dazu die Rückfrage des Auftraggebers, ob die Sanctus-Rückhaltung auch auf die anhaltende Verlangsamung der Gegner achtet.

**Erster Befund: die Dokumente selbst waren an mehreren Stellen überholt.** Das fiel nicht bei der Lektüre auf, sondern erst durch eine Erhebung — die Prüfung nach Augenschein hatte die Stellen zuvor zweimal übersehen.

- `09-tank-selfprotection.md` führte zwei Lücken der Schildanrechnung als offene Defekte, die längst geschlossen sind: `ModifyDivineBenisonPvE` steht auf der Zielseite, `Intersection`/`Intersection_4040` stehen in `ShieldStatus` (A43). Der Abschnitt lag zudem unter „Was offen bleibt".
- Derselbe Text erklärte pauschal, für The Blackest Night sei „gar keine Sonderregel richtig". Das galt für Heilung und Schild und ist dort weiter richtig; seit A50 gibt es eine für den **Schadensstrom**. Ohne Abgrenzung liest ein Abschnittsleser den überholten Stand — genau der Fehlerpfad, den der Urteilsstil vermeiden soll.
- `08-mitigation-synergy.md` beschrieb die Streckungsbedingung ohne die in A50 ergänzte Forderung nach einer laufenden Betäubung und führte die Übertragung auf weitere Doppelnutzen-Aktionen als offen, obwohl sie inzwischen erhoben ist.

**Zweiter Befund: die Zeilenverweise altern, und zwar als Klasse.** `scan15.py` (neu) paart jedes `Datei.cs:Zeile`-Zitat mit den Bezeichnern daneben und prüft, ob der Bezeichner dort steht. Erster Lauf: 122 Zitate, 26 Befunde; nach Behebung zweier Fehler im Scan selbst — in einer Tabellenzeile mit zwei Zitaten wurden die Bezeichner vermischt, und das Abschneiden am Nachbarzitat zerlegte die Backtick-Paarung — blieben zehn echte in den geltenden Dokumenten. `AUDIT_LOG.md` ist ausgenommen und wird getrennt ausgewiesen: Das Archiv datiert seine Befunde, eine seither verschobene Zeile ist dort kein Fehler.

Behoben wurde **nicht die Nummer, sondern die Bauform**: Wo ein eindeutiger Bezeichner existiert, steht jetzt er. Eine Zeilennummer altert bei jedem Commit, ein Bezeichner erst bei einer Umbenennung — und die fällt beim Kompilieren auf. Das ist die Konsequenz aus Parnas' *Lack of Movement*, angewandt auf die Dokumentation statt auf den Code.

**Dritter Befund, der die Rückfrage beantwortet: Rückstoß (Arm’s Length) verlangsamt, und niemand liest das.** `scan16.py` (neu) nimmt den Wirktext jeder PvE-Aktion, zieht die Kontroll- und Minderungswirkungen auf Gegner heraus und fragt, ob der Baum den zugehörigen Status je liest. 1457 PvE-Aktionen, 52 mit einer solchen Wirkung, **ein** Fund im Tank- und Heilerprofil, der eine Entscheidung ändert:

| Aktion | zweite Wirkung | Stand vorher |
|---|---|---|
| Rückstoß (Arm’s Length) (7548) | Verlangsamung +20 % auf jeden physischen Angreifer, 15 s | nur als Rückstoßschutz eingeordnet (`AntiKnockbackAbility`); die Verlangsamung wird nirgends gelesen |

Die eigene erste Bewertung war dabei falsch und wurde durch die Quelle widerlegt: Ich hatte den abgeschnittenen Wirktext gelesen und Verlangsamung für einen reinen Zauberer-Debuff gehalten. Der vollständige Text nennt neben Wirk- und Wiederholzeit ausdrücklich die **Verzögerung der Automatikangriffe** — aus denen Trash-Gegner den Großteil ihres Schadens liefern. Die Drosselung liegt damit in der Größenordnung von Rampart und jenseits der Linie, ab der die Barriere in sieben Sekunden nicht mehr aufgezehrt wird.

**Umgesetzt:** `StatusHelper.SlowStatus` (alle zwölf Ids desselben Anzeigenamens, Slow+ eingeschlossen), `CustomRotation_OtherInfo.SurveyHostileStatus` als schlichter Zähler für „wie viele Gegner in Reichweite tragen einen dieser Status", und `DRK_Reborn.PackSlowed()` als fünfte Bedingung des Pull-Zweigs. Dieselbe Anteilsregel wie bei der Betäubung, aber **ohne** Nachlauffenster: Eine Betäubung hält den Strom an und läuft in Sekunden aus, eine Verlangsamung verdünnt ihn fünfzehn Sekunden lang und endet mit dem Debuff.

**Was die Frage nach der Quelle angeht** — geprüft wird der Status **auf den Gegnern**, nicht der Buff auf dem Tank. Das ist dieselbe Wahl wie bei den Betäubungen und aus demselben Grund richtig: Die Verlangsamung entsteht erst, wenn ein Gegner den Träger tatsächlich trifft, der Buff allein sagt darüber nichts, und fremde Quellen (Blaumagier, Phantom-Jobs) zählen mit.

**Offen und vorgelegt, nicht umgesetzt:** Rückstoß (Arm’s Length) auch **als** Minderungswerkzeug zu wirken. Der Nutzen ist erheblich — kostenlos, 120 s Abklingzeit, 15 s Drosselung —, aber sie ist zugleich der einzige Rückstoßschutz des Jobs, und ob im nächsten Kampfabschnitt ein Rückstoß kommt, kann RSR nicht wissen. In `TODO.md` mit Empfehlung geführt.

**Nicht geklärt: der deutsche Name.** Der Auftraggeber nennt die Fähigkeit „Abtausch"; C30 hat denselben Namen Shirk zugeordnet. Shirk überträgt Feindseligkeit und verlangsamt nichts — die vom Auftraggeber beschriebene Wirkung gehört eindeutig zu Rückstoß (Arm’s Length). Welche der beiden Aktionen im deutschen Client „Abtausch" heißt, ist hier **nicht** belegbar: Die drei Quellen, die es klären würden (Lodestone-Datenbank, Garland Tools, XIVAPI), sind vom Egress gesperrt, und zwei Suchmaschinenzusammenfassungen widersprechen einander. Für die Umsetzung ist das folgenlos, weil sie über die Wirkung geht; für die Reihenfolgebedingung aus A46 ist es das nicht — steht dort „Abtausch" für Rückstoß (Arm’s Length), fehlt in `reprisalDone` eine zweite Stufe. Als offene Frage geführt.

**Erreichter Prüfgrad:** statische Prüfung, zwei neue Prüfskripte mit Selbsttest, Wirktext-Beleg aus `Status.resx` und `ActionId.resx`, Websuche zur Auto-Attack-Frage, CI-Kompilierung. Nicht beobachtet: ob die Anteilsschwelle für Verlangsamung im Spiel trägt.

---

### A52 · Code-Review des Heiler- und Tankbestands, und die Versionsnummer, die niemand nachzieht (10.09.2026)

**Anlass:** Auftrag, nach der Konzeptprüfung ein Audit und Code-Review der bisherigen Heiler- und Tank-Patches durchzuführen; dazu die Frage, warum die Versionsnummer beim Angleichen an Upstream nicht automatisch angepasst wurde.

**Review-Ergebnis: keine neuen Defekte.** Geprüft wurde mit den vorhandenen Prüfmitteln über den gesamten Heiler- und Tankbestand (`scan`, `mitscan`, `scan2` bis `scan4`, `scan8` bis `scan11`), dazu die in dieser Sitzung gebauten Regeln von Hand gegen ihre Wirkkette. Die Skripte meldeten drei Treffergruppen, alle drei Fehlanzeigen — sie sind jetzt in `.github/scripts/audit/README.md` als bekannt vermerkt, damit der nächste Durchgang sie nicht erneut aufrollt:

- `scan.py` liest bei „RotationDesc nennt X, Rumpf benutzt X nie" nur den unmittelbaren Methodenrumpf. Sacred Soil (Gelehrter), Sheltron und Holy Sheltron (Paladin) und Eukrasian Prognosis II (Weiser) werden sehr wohl gewirkt, nur aus einer Hilfsmethode oder weiter unten im Pfad.
- `scan2.py` trennt bei „wiederholte Bedingung" keine Rollenzweige. `ShouldAddDefenseSingle` prüft `IsHostileCastingTankBusterAtMe` und `BMRTankbusterImminent` je zweimal, aber in verschiedenen Rollen — der Zweig für Schadensklassen trägt zusätzlich `PartyTank == null` (A6).
- `scan11.py` meldet `ModifyLivingDeadPvE`; das ist der in seinem eigenen Abschnitt bereits beschriebene Fall eines Debuffs, den die Aktion wirklich auf den Spieler legt.

**Drei Genauigkeitsmängel eigener Arbeit, eingearbeitet:**

1. `HasMajorMitigation` liest `RampartStatus`, und diese Liste führt zwei Einträge, die keine Stromdrosselung sind: `LivingDead` mindert nichts, sondern verschiebt den Tod, und `Bloodwhetting` ist eine 10-%-Minderung unterhalb der Linie, die die Liste sonst zieht. Für den einen Verbraucher stimmt die Antwort trotzdem — beim Dunkelritter aus einem anderen Grund —, aber ein zweiter Verbraucher würde darüber stolpern. Als Bemerkung an der Eigenschaft festgehalten.
2. `ShouldHoldHolyForBarrier` greift nur, wenn ein Gruppenmitglied **in Tankrolle** die Barriere trägt. Die Beschränkung war umgesetzt, aber nicht begründet: Die Barriere kann auf jedem liegen, und auf einer Schadensklasse ist die Betäubung eher das, was sie am Leben hält — dort wäre die Rückhaltung ein Tausch von Leben gegen Ressource.
3. Die Wirkkette wurde gegengeprüft: `ShouldUseBlackestNightOnSelf` wirkt ausschließlich im Selbstwurf des Verteidigungspfads. Der Countdown-Zweig und der Party-Zweig sind unberührt.

**Zur Versionsnummer — die Antwort ist, dass es nichts zum Automatisieren gab.** Am Artefakt gemessen:

| Beobachtung | Beleg |
|---|---|
| Upstream führt im Quellbaum **keine** Version | `git show upstream/main:Directory.Build.props` enthält keine `<Version>`-Zeile |
| Die Version entsteht erst beim Veröffentlichen aus dem Tag | `publish.yaml` übergibt `AssemblyVersion`, `FileVersion`, `PackageVersion`, `InformationalVersion` aus `env.tag` |
| Die drei Versionszeilen sind eine Fork-Ergänzung | ohne sie meldete die Assembly 1.0.0, und das Paket trüge die nackte Upstream-Identität |

Ein Merge kann die Zahl also nicht mitbringen, weil dort nichts ist, was mitkäme. Sie ist eine **handgepflegte Zahl, die eine Tatsache anderswo spiegelt** — den höchsten Upstream-Tag in der eigenen Historie —, und damit dieselbe Alterungsform wie eine handgepflegte Statusliste. Sie ist auch genauso gealtert: bei A40 auf 7.5.6.0 gesetzt, während Upstream inzwischen 7.5.6.1 getaggt hat, dessen Commit in unserer Historie liegt.

**Umgesetzt:** Zahl auf 7.5.6.1 gezogen (alle drei Zeilen samt Markern), der Kommentar an der Stelle sagt jetzt, dass nichts sie automatisch nachzieht, und `check_fork_version.py` misst die Lücke. Verglichen wird gegen die Upstream-Tags, die **Vorfahr von HEAD** sind — die Zahl behauptet, auf welchem Release der Fork steht, nicht welches existiert. Rückgabewert 1, wenn sie zurückliegt.

**Nicht in die CI eingehängt, sondern vorgelegt:** Eine Schranke im Build ist eine Entscheidung über den Veröffentlichungspfad, und der liegt beim Auftraggeber. Bis dahin gehört das Skript in den Sync-Ablauf, direkt nach `git fetch --prune --tags upstream`.

**Erreichter Prüfgrad:** statische Selbstprüfung, neun Prüfskripte über den Heiler- und Tankbestand, Messung am Repository-Zustand statt aus dem Gesprächsverlauf, CI-Kompilierung. Kein Vier-Augen-Prinzip, keine Laufzeitbeobachtung.

---

### A53 · Die Versionsnummer wird abgeleitet, Rückstoß wird gewirkt (10.09.2026)

**Anlass:** Zwei Beanstandungen des Auftraggebers, beide berechtigt und beide auf Regeln bezogen, die dieses Projekt bereits führt.

**1. Die Versionsnummer beantwortet die Frage nicht, für die es sie gibt.** Wer aus den Fork-Quellen compiliert, will der Anzeige entnehmen, welchen Upstream-Stand der Build enthält. Angezeigt wurde 7.5.6.0, während der Baum die Änderungen von 7.5.6.1 trug. A51 und A52 hatten den Einzelfall behoben (Zahl nachgezogen) und ein Prüfskript daneben gestellt — das ist nach der eigenen Vorgabe zu wenig: Wiederholt sich die Entstehungsursache, ist die **Wiederholbarkeit** zu adressieren, nicht der Fundort. Eine handgepflegte Zahl, die eine Tatsache im Repository spiegelt, veraltet beim nächsten Sync, den jemand vergisst, und ein Skript, das niemand aufruft, hält sie nicht aktuell (C34).

**Umgesetzt:** Das Target `DeriveUpstreamVersion` in `Directory.Build.props` liest bei jedem Build `git describe --tags --abbrev=0 --exclude=*wsh* --match=[0-9]*` — den neuesten von HEAD aus erreichbaren Upstream-Tag, also genau die Frage „welchen Release enthält dieser Code", nicht „welchen gibt es". Fork-Tags tragen den `wsh`-Marker und sind ausgeschlossen, damit `7.5.5.41+wsh1` nicht für einen Upstream-Release einsteht.

Zwei Ausstiege, beide beabsichtigt: Ist `AssemblyVersion` bereits gesetzt, tut das Target nichts — das ist der Veröffentlichungspfad, der den gepushten Tag übergibt und Vorrang hat. Fehlt git, ist der Checkout flach oder ist kein Upstream-Tag erreichbar, scheitert der Aufruf und die Literale in der Datei gelten; ein Quellpaket ohne Historie baut also weiterhin. Beide Fälle melden eine Zeile in die Build-Ausgabe.

**2. Rückstoß wird jetzt auch gewirkt, nicht nur gelesen.** Der Auftraggeber hat den Einwand entkräftet, der den Punkt offen hielt: **Rückstoß ist eine Bossmechanik und im Wall-to-Wall praktisch nie vorhanden** — der Zielkonflikt zwischen Minderung und Rückstoßschutz besteht dort also nicht. Damit fällt der Grund weg, die Aktion für einen Fall aufzuheben, der in dieser Lage nicht eintritt.

`ShouldUseArmsLengthOnPull()` wirkt sie im Verteidigungspfad des Dunkelritters **vor** der Barriere, hinter der Option `UseArmsLengthOnPull` (Vorgabe aus). Die Gegnerschwelle ist bewusst dieselbe wie bei der Barriere und keine zweite Zahl, die davon wegdriften könnte; sie ist zugleich das, was Trash von Boss trennt.

Dazu die **fehlende zweite Stufe der Reihenfolgebedingung**, die seit A46 offen war: „Reflexion und Abtausch zuerst" meinte zwei kostenlose Minderungen, umgesetzt war nur Reprisal. `armsLengthDone` ergänzt die zweite — erledigt ist sie, wenn sie nicht wirkbar ist oder bereits läuft, wobei eine abklingende Rückstoß-Aktion eine ist, die auf diesem Pull gewirkt wurde.

**3. Der Namensfehler, der das aufgehalten hat.** Die Zuordnung der Aktion war zweimal falsch und einmal erfunden (C33). Der deutsche Name ist nach Angabe des Auftraggebers **Rückstoß**; „Armlänge" war eine eigene Übersetzung und ist repo-weit ersetzt. Die Namensregel in `CLAUDE.md` ist entsprechend verschärft: Ein deutscher Name wird nie gebildet, sondern belegt oder übernommen; liegt keiner vor, steht der englische Bezeichner.

**Der erste Anlauf brach den Build**, und zwar an einer Stelle, die keine Rotation berührt: Der Kommentar über dem Target zitierte die git-Kommandozeile im Fließtext, und XML verbietet `--` innerhalb eines Kommentars. MSBuild importiert `Directory.Build.props` vor allem anderen, also scheiterte jedes Projekt mit MSB4024, bevor eine Zeile übersetzt wurde. Kosten: ein voller Windows-Lauf für einen Fehler, den ein XML-Parser in einer Sekunde findet.

**Daraus die Schranke:** `check_msbuild_xml.py` parst alle MSBuild-Dateien und läuft im `DispatchChain`-Job, dem Job ohne .NET. Der Selbsttest konstruiert genau diesen Defekt — einen Kommentar mit `--` — und verlangt, dass der Parser ihn ablehnt. Das ist dieselbe Antwort wie bei den Statuslisten: nicht der Einzelfall wird behoben, sondern die Wiederholbarkeit.

**Erreichter Prüfgrad:** statische Prüfung, `git describe` am Repository gegen den erwarteten Wert getestet, XML-Wohlgeformtheit als CI-Schranke, CI-Kompilierung. **Nicht** geprüft: dass das MSBuild-Target unter Windows die Versionseigenschaften rechtzeitig setzt — das zeigt erst ein Release-Build oder die Build-Ausgabe eines lokalen Compilats. Wer den nächsten Build macht, sieht in der Ausgabe die Zeile „Fork version derived from the repository".

### A54 · Die Wiederbelebung wurde gewählt und nie ausgeführt (10.09.2026)

**Anlass:** Nutzermeldung, seit Beginn bestehend: Heiler und übrige Rezzer wirken die Wiederbelebung verzögert oder gar nicht, während Schaden und Heilung weiterlaufen — trotz `RaisePlayerFirst`. Nachgeschobene Laufzeitbeobachtungen des Auftraggebers, die den Befund tragen: die Verzögerungswerte stehen auf 0; die Toten liegen ruhig und sind anvisierbar; es tritt im Stehen wie im Laufen auf; und **im Vorschaufenster erscheint Spontanität und wird nie gewirkt**.

**Befund.** `WeaponRemain` ist `DataCenter.DefaultGCDRemain` (`CustomRotation_OtherInfo.cs:1624`). `CustomRotation_GCD.cs:560` wählte Spontanität **nur** bei `WeaponRemain <= 0.5f`; `RSCommands_Actions.cs:78` verweigert **jede** Fähigkeit bei `0 < DefaultGCDRemain <= 0.5f` (gespiegelt bei `:46` und in `CustomRotation_Ability.cs:28`). Spontanität ist eine Fähigkeit, die Fenster sind bis auf `DefaultGCDRemain == 0` deckungsgleich. Über 0,5 s wurde sie nicht einmal gewählt und der Dispatcher fiel zu Heilung und Schaden durch; darunter wurde sie gewählt, angezeigt und verworfen. Beide Umgehungen waren zu: der Hartwirk-Zweig verlangte Spontanität in Erholung, die ohne Nutzung nie eintrat, und der Einschiebe-Pfad verlangte die Wiederbelebung als `nextGCD`, die dort nie stand.

**Entstehung — Parnas, *Lack of Movement*.** `f22be318` (10.01.2025) führte den Zweig ein, als es keine Ausführungssperre gab; `92f109d3` (02.03.2026) fügte die Sperre hinzu und entwertete ihn, ohne dass etwas fehlschlug. Vierzehn Monate ohne Signal.

**Vier eigene Fehldiagnosen auf dem Weg, alle vom Auftraggeber widerlegt** (Einzelheiten in C35): Verzögerungsparameter, `IsTargetMoving` auf der Leiche, Anvisierbarkeit, und zuletzt die Bewegungssperre in `NeedsCasting` — Letztere zweimal behauptet, obwohl der Nutzer „ich stehe mal, und mal bewege ich mich" gesagt hatte. Die Beobachtung „Spontanität steht im Fenster" hat den Fall entschieden, nicht die Codelektüre; sie beweist, dass `RaiseGCD` true lieferte und der Defekt hinter der Auswahl saß.

**Umsetzung.** Der GCD-Pfad meldet die Wiederbelebung statt Spontanität, und zwar nur bei laufendem GCD (`!ActionHelper.CanUseGCD`), damit bei freiem GCD nicht ohne Instant hartgewirkt wird. Der Einschiebe-Pfad (`CustomRotation_Ability.cs`) zündet Spontanität davor, im Fenster oberhalb der Sperre. Gegen `Raise` statt gegen eine Vier-Ids-Liste, der Verraise und Angel Whisper fehlten. Die beiden `HardCastNormal`-Zweige fragen über `SwiftcastComingForRaise`, ob Spontanität noch kommt, statt ob sie in Erholung ist; die vier Abwägungszweige behalten ihren Wirkzeit-gegen-Wartezeit-Vergleich und bekommen nur den Fall `!RaisePlayerBySwift` dazu. Mit abgeschaltetem `RaisePlayerBySwift` wurde sonst überhaupt nicht wiederbelebt. Die einheitliche Ersetzung in allen sechs Zweigen war zwischenzeitlich umgesetzt und ist im Code-Review dieses Vorgangs als eigene Regression zurückgenommen worden: sie hätte die Abwägung entfernt, die `HardCastSwiftCooldown` ausmacht. Die Ausführungsschicht ist unangetastet; ihre Sperre ist richtig.

**Falsifikation.** Zwei Hypothesen widerlegt: „kein Defekt" scheitert an der Deckungsgleichheit der Fenster und an der Laufzeitbeobachtung; „andere oGCD verdrängt Spontanität" scheitert daran, dass `EmergencyAbility` als **erster** Zweig des Fähigkeiten-Dispatchers läuft, vor Interrupt, Dispel, Heilung und Angriff. Präzedenz im Baum zweifach: `SMN_Reborn.cs:374` und `:478`.

**Beim Umsetzen widerlegte sich ein Teil des eigenen Konzepts.** `SwiftcastBuffer` sollte verdrahtet werden; die Prüfung ergab, dass ihre dokumentierte Bedeutung („warte, bis nur noch so viel Restzeit bleibt") bei 0,6 s fast vollständig und bei 0 vollständig im gesperrten Fenster liegt. Eine Verdrahtung hätte den Defekt an zweiter Stelle neu gebaut. Sie bleibt unangetastet und ist in `TODO.md` erfasst.

**Prüfmittel.** `.github/scripts/audit/scan17.py`, im `DispatchChain`-Job. Es liest das Sperrfenster **aus der Ausführungsschicht** statt es anzunehmen und meldet jede Auswahl im GCD-Pfad, die darin liegt; der Selbsttest konstruiert die exakte alte Zeile und verlangt ihre Ablehnung.

**Erfasst, nicht behoben** (alle in `TODO.md`): `CanBeRaised` prüft jobunabhängig `RaisePvE`; `IgnoreClipping` wird sechsfach geschrieben und nie gelesen; `GetPriorityDeathTarget` prüft `deathTanks.Count > 1`.

**Wirksamkeitsprüfung (Stufe 10) — die Defektklasse hinter dem Fund.** Zwei ungelesene Einstellungen in einem einzigen Codepfad sind kein Zufall, deshalb wurde die Klasse erhoben: `scan18.py` prüft jede in `Configs.cs` deklarierte öffentliche Einstellung auf einen Leser. Von 75 haben sieben keinen. Neu daraus: **`InterruptDelay` und `ProvokeDelay`** — dieselbe Bauart wie `RaiseDelay2` und `EsunaDelay`, die gelesen werden, während diese beiden nirgends ankommen; und **`TargetColor`**, die sich in `[UI(... Parent = nameof(TargetColor))]` selbst als Elternschalter nennt und zusätzlich keinen Leser hat. Alle drei sind in `TODO.md` erfasst, keine davon behoben: Die Verdrahtung der Verzögerungen verlangsamt das Kampfverhalten und ist eine Entscheidung des Auftraggebers, und bei `TargetColor` ist der gemeinte Elternschalter nicht aus dem Code zu erschließen. Bei der Erhebung selbst wurde ein Methodenfehler abgefangen: Die erste Fassung leitete Eigenschaftsnamen aus privaten `[UI]`-Feldern ab und meldete fünf Namen, die in der Datei überhaupt nicht vorkommen. Das Skript deckt private Felder deshalb bewusst nicht ab und schreibt diese Lücke aus, statt Befunde zu erfinden.

**Erreichter Prüfgrad:** statische Prüfung, Versionsgeschichte für die Entstehung, zwei Prüfskripte mit Selbsttest, CI-Kompilierung. **Keine** eigene Laufzeitbeobachtung und kein Vier-Augen-Prinzip — das Code-Review dieses Vorgangs war Selbstkontrolle und hat immerhin eine selbst eingebaute Regression gefunden, ersetzt aber das Vier-Augen-Prinzip nicht. Dass die Wiederbelebung im Spiel nun zügig fällt, ist begründet, nicht gemessen.

### A55 · Die eigene Entscheidungsvorlage geprüft: drei von vier Punkten waren keine (10.09.2026)

**Anlass:** Auftrag, alle offenen Fragen im vollständigen Loop daraufhin zu prüfen, ob eine Entscheidung des Auftraggebers wirklich nötig ist, welche Entscheidung optimal wäre, und ob die vorgelegten überhaupt die richtigen sind — gesamtheitlich und kausal auf die bisherige Programmierrichtung hin.

**Ergebnis: Von vier vorgelegten Punkten war einer gar kein Defekt, zwei sind ohne den Auftraggeber entscheidbar, und bei einem war die eigene Empfehlung falsch.** Übrig bleibt keine Entscheidung, die er treffen muss.

| Vorgelegt als | Prüfergebnis |
|---|---|
| `deathTanks.Count > 1` sei ein Tippfehler für `> 0` | **Kein Defekt, zurückgenommen** (C36). Der Block wurde in `9190888d` als Ganzes neu geschrieben, es gibt keine ersetzte Vorgängerzeile. Die Staffelung ist eine Fallunterscheidung mit Sinn: bei **einem** toten Tank hält der Co-Tank, der Heiler ist wichtiger; bei **zwei** hält niemand mehr, dann muss zuerst ein Tank hoch |
| `CanBeRaised` brauche Laufzeitbeobachtung mit einem Nicht-Weißmagier | **Ohne ihn entscheidbar, behoben.** Die Semantik der nativen Funktion ist tatsächlich nicht ermittelbar — FFXIVClientStructs bindet `CanUseActionOnTarget` als bloße Signatur mit Byte-Pattern, ohne Dokumentation. Sie wird aber nicht gebraucht: Die Prüfung ist per Signatur aktionsbezogen und wurde mit einer Aktion geführt, die der Spieler nicht besitzt. Liefert sie dafür `false`, waren alle Nicht-Weißmagier-Rezzer blockiert; liefert sie `true`, war die Zeile wirkungslos. **Beide Ausgänge machen die Konstruktion falsch**, also entscheidet der unbekannte Rückgabewert nichts |
| `TargetColor` brauche die Angabe des gemeinten Elternschalters | **Keine Entscheidung, und der gemeldete Elternfehler ist folgenlos.** `SearchableCollection.cs:45` nimmt nur `CheckBoxSearch` in die Elternliste auf; ein `Vector4` landet nie darin, der Verweis läuft ins Leere und der Eintrag sortiert auf oberster Ebene. Kein Absturz, keine Rekursion. Der wirkliche Befund ist der fehlende Leser, und dessen Behebung verlangt zu erfinden, wo die Farbe zu zeichnen wäre — deshalb erfasst, nicht vorgelegt |
| `InterruptDelay`/`ProvokeDelay`: Empfehlung „verdrahten mit Vorgabe (0;0)" | **Eigene Empfehlung war falsch.** Sie setzt voraus, dass gespeicherte Werte auf die neue Vorgabe gezogen werden können. `Configs.Migrate` (`:1440`) kann das nicht: Es gibt bei abweichender Version `new Configs()` zurück, verwirft also die ganze Datei. Bestandsnutzer haben `(0,5; 1)` gespeichert und bekämen die Verzögerung eingeschaltet — ein Verstoß gegen die Feature-Toggle-Regel. Richtig ist deshalb: **nicht verdrahten**, mit der fehlenden Migration als Auflösungsbedingung |

**Der eigentliche Fund liegt eine Ebene tiefer.** `Configs.Migrate` ist kein Migrationspfad, sondern ein Zurücksetzen. Das ist nicht nur eine Unbequemlichkeit, sondern eine **Sperre für andere Behebungen**: Jede Korrektur, die einen Vorgabewert ändern muss, um das bisherige Verhalten zu erhalten, ist ohne Feldmigration nicht durchführbar. Der Verzögerungs-Eintrag ist genau daran gescheitert. Als technische Schuld mit dieser Begründung in `TODO.md` aufgenommen; für den Auftraggeber unmittelbar bedeutsam, weil eine Upstream-Erhöhung von `CurrentVersion` seine Einstellungen löscht und `Restore()` ausgerechnet bei Versionsabweichung verweigert.

**Umsetzung.** `CanBeRaised` beantwortet die Zielfrage jetzt zielbezogen (`IsTargetable`) und überlässt die Aktionsfrage den Aufrufern, die ihre eigene Aktion kennen — dem Zauber über `Raise.CanUse`, dem Gegenstand über `BaseItem`. Dieselbe Trennung wie in A54: Lage von Ausführbarkeit lösen. **Risikoabschätzung:** Die Änderung kann nur Ziele zulassen, nie ausschließen; ein zusätzlich zugelassenes Ziel fällt in der nachgelagerten `CanUse`-Kette durch. `IsOtherPlayerOutOfDuty` bleibt unangetastet — dort ist die Absicht eine andere (Zielbarkeit eines Fremdspielers) und der Autor hat mit „Raise oder Cure" bereits kompensiert.

**Erreichter Prüfgrad:** statische Prüfung, Versionsgeschichte, Fremdquelle (FFXIVClientStructs) ausgeschöpft und als undokumentiert belegt, Prüfskripte, CI-Kompilierung. Keine Laufzeitbeobachtung.

---


### A56 · Zweiter Anlauf am Wiederbelebungspfad, diesmal ohne den GCD-Pfad anzufassen (10.09.2026)

**Anlass:** Auftrag, den Defekt erneut anzugehen, nachdem der erste Versuch zurückgenommen war („die aktuelle Situation ist unbefriedigend"), mit Konzept, vollständigem Loop, Audit und Code-Review.

**Entscheidender neuer Beleg — ein natürliches Experiment des Auftraggebers.** Er berichtete, dass die Wiederbelebung sofort erfolgt, wenn er von automatisch auf manuell stellt und den Toten anvisiert. Das bestätigt die Ursachenanalyse am Artefakt: `ActionTargetInfo.cs:117` lässt im manuellen Modus ein feindliches Ziel nur als angewähltes Hauptziel zu; wer einen Toten anvisiert, hat keines, sämtliche Angriffe fallen aus, der GCD bleibt frei und `DefaultGCDRemain` steht auf `0` — dem einzigen Punkt, an dem Auswahl (`WeaponRemain <= 0.5f`) und Ausführungssperre (`> 0f`) zusammenpassen. Im automatischen Modus ist dieser Punkt von laufender Aktion, Animationssperre oder Klickverzögerung überdeckt.

**Die Change Impact Analysis, die beim ersten Versuch gefehlt hat, wurde diesmal geführt und beziffert:** 447 `nextGCD`-Fundstellen im Baum, davon auswertende Stellen in DRG, GNB, NIN, SAM, SMN und den PvP-Rotationen. Genau dieser Kreis wurde beim ersten Versuch umgeschrieben und hat Radiant Aegis gekostet. Der zweite Entwurf lässt `GCD()` und `RaiseSpell` unverändert, womit alle 447 unberührt bleiben.

**Umsetzung.** Der vorhandene Spontanitäts-Zweig in `EmergencyAbility` bekommt einen zweiten Auslöser: `RaisePendingAndCastable()`. Er verlangt eine anstehende Wiederbelebung (`AutoStatus.Raise`), eine wirkbare Wiederbelebungsaktion und **kein** laufendes Spontanität; letzteres begrenzt den Zweig auf höchstens einen Frame je Gelegenheit.

**Zwei Funde aus dem eigenen Code-Review, beide vor dem Commit eingearbeitet:**
- *Fensterlage.* `Ability()` kehrt bei `0 < WeaponRemain <= 0.5f` sofort zurück, und bei freiem GCD ruft `Invoke` den Fähigkeitenpfad gar nicht auf. Der Zweig kann also ausschließlich bei `WeaponRemain > 0,5 s` greifen — genau dem Fenster, in dem `DoAction` eine Fähigkeit durchlässt. Das war zu belegen, nicht anzunehmen.
- *Zielüberschreibung.* Wiederbelebungsaktionen führen keinen eigenen Zieltyp (nur `IsFriendly`); ihr Ziel kommt aus `TargetType.Death`, das allein der GCD-Pfad setzt. Eine Wirkbarkeitsprüfung im Fähigkeitenpfad ohne diese Überschreibung durchsucht die falsche Menge, und `CanUse` weist `Target` als Nebenwirkung zu. `RaisePendingAndCastable` setzt die Überschreibung deshalb selbst und **stellt den vorherigen Wert wieder her**, statt ihn zu löschen, weil der Dispatcher eigene Überschreibungen um seine Zweige legt.

**Verdrängungsprüfung.** Nach dem Zweig stehen in `EmergencyAbility` nur zwei weitere: Second Wind für Nahkämpfer und für physische Fernkämpfer bei Doom-Status. Für einen Rezzer fällt dort nichts aus. Verbraucht wird ein Einschiebefenster, einmal je Gelegenheit.

**Nachtrag auf Rückfrage des Auftraggebers — der Fall, dass Spontanität nicht zur Verfügung steht.** Die Frage deckte eine Lücke des Entwurfs auf: Er behandelte allein die verfügbare Spontanität. Erhebung: Bei Spontanität in Erholung greift der Hartwirk-Zweig und die Wiederbelebung wird hart gewirkt — bauartbedingt nur im Stehen, weil Stufe (C) `!IsMoving` verlangt. Bei **abgeschalteter** `RaisePlayerBySwift` dagegen wurde überhaupt nicht wiederbelebt: Für einen Heiler zündet die Rotation Spontanität ausschließlich über den Wiederbelebungspfad — die beiden anderen Zünder (`CustomRotation_Ability.cs:688`, `:697`) sind auf `JobRole.RangedMagical` eingeschränkt —, sie geht also nie in Erholung, und der Hartwirk-Zweig fordert genau diese Erholung. Alle drei Stufen von `RaiseSpell` sind damit zu. Die Beschreibung der Einstellung sagt zu, Spontanität nicht dafür zu verwenden, nicht das Wiederbeleben einzustellen.

Die Behebung stammt aus PR #7 und war mit dem Revert verlorengegangen; sie ist zurückgeholt: Die Hartwirk-Zweige fragen über `SwiftcastComingForRaise`, ob Spontanität für diese Wiederbelebung noch kommt, statt ob sie in Erholung ist. **Die Wirkung ist als Wahrheitstabelle ausgezählt und beschränkt sich auf eine von vier Kombinationen** — Einstellung aus und Spontanität bereit; bei eingeschalteter Einstellung, der Vorgabe, ist das Verhalten bitidentisch. Das ist zugleich der Nachweis, dass diese Korrektur die zurückgenommene Regression nicht verursacht haben kann: Sie greift in deren Konfiguration überhaupt nicht. Die vier Abwägungszweige behalten ihren Wirkzeit-gegen-Wartezeit-Vergleich und bekommen allein den Fall dazu, in dem nichts kommt, worauf zu warten wäre.

**Erreichter Prüfgrad:** statische Prüfung, Wirkungsbereich beziffert statt geschätzt (447 `nextGCD`-Leser, Wahrheitstabelle der Hartwirk-Bedingung), Prüfskripte, CI-Kompilierung. **Keine Laufzeitbeobachtung.** Der Vorgang gilt ausdrücklich als **nicht abgeschlossen**: Er liegt auf einem eigenen Branch, damit der zurückgenommene Stand als sicherer Rückfall erhalten bleibt, und wird erst nach einer Spielbeobachtung des Auftraggebers als behoben geführt. Das ist die unmittelbare Folge aus C37 — dort war der als „begründet, nicht gemessen" ausgewiesene Prüfgrad ehrlich benannt, aber nicht zum Anlass genommen, die Änderung vom Build des Auftraggebers fernzuhalten.

---

### A57 · Alle Anwendungsfälle der Wiederbelebung durchgegangen (11.09.2026)

**Anlass:** Auftrag, sämtliche Anwendungsfälle beim Wiederbeleben im vollständigen Loop kritisch durchzugehen.

**Erhebung entlang der sechs steuernden Größen:** Rolle und Job, Zustand von Spontanität, `HardCastRaiseType` (fünf Werte), `RaisePlayerFirst`, `RaiseType` (sechs Werte), Bewegung. Vollständig in `docs/rotation-flow/11-raise-dispatch.md` ausgeschrieben.

**Struktureller Befund ohne Defektcharakter:** Der Wiederbelebungsblock existiert zweimal. Bei `RaisePlayerFirst` steht er auf `:123` vor der Heilung, ohne die Einstellung auf `:350` dahinter — also hinter der gesamten Heilung und der Einzelziel-Verteidigung. Die Vorgabe ist **aus**. Das ist die dokumentierte Bedeutung der Einstellung, erklärt aber, warum ihre Wahl das beobachtete Verhalten stark verschiebt.

**Neue Defekte, alle in `TODO.md` erfasst, keiner behoben:**

1. **Die Phönixfeder ist vollständig unverdrahtet.** `CustomRotation_Items.UsePhoenixDown` ist fertig implementiert — samt korrekt gesicherter Zielüberschreibung, dasselbe Muster, das in dieser Sitzung für `RaisePendingAndCastable` gebaut wurde — und hat keinen Aufrufer. Die Wirkung reicht weiter: `DataCenter.CanRaise()` liefert jobunabhängig wahr, sobald die Einstellung an ist und eine Feder im Gepäck liegt, wodurch Tanks und Schadensjobs `AutoStatus.Raise` gesetzt bekommen und den Wiederbelebungsblock durchlaufen, in dem nichts geschehen kann. Dritter Fall derselben Klasse nach `SwiftcastBuffer` und `IgnoreClipping`.
2. **`HardCastOnlyHealer`, drei Defekte an einem Zweig.** Der Optionstext verspricht „while Swiftcast is on cooldown", der Code prüft es nicht. Beide Heilermengen schließen den Spieler aus, weshalb ein einzelner Heiler nie hart wirkt — in einer Vierergruppe ist die Einstellung wirkungslos. Und die Bedingungsreihenfolge ruft `RaiseSpell` mit seinen Nebenwirkungen auf `Target` und `ShouldEndSpecial` vor der billigen Mengenprüfung. `HardCastOnlyHealerSwiftCooldown` wiederholt die Mengenbildung wortgleich.

**Geprüft und ohne Befund:** Die `RaiseType`-Varianten werden in `GetDeathTarget` getrennt behandelt, die Allianz-Zweige ohne Doppelzählung. Die Filterkette in `GetDeath` ist vollständig und schließt jeweils sinnvoll aus. Der Rotmagier war von der Kernursache nie betroffen, weil Dualcast in `StatusHelper.SwiftcastStatus` steht und damit Stufe (A) von `RaiseSpell` ohnehin greift. Die Kombination `NoHardCast` mit abgeschalteter `RaisePlayerBySwift` belebt niemanden wieder — das ist gewollt, beide Wege sind bewusst abgeschaltet.

**Nicht entscheidbar:** Ob `HardCastOnlyHealer` beim Einzelheiler greifen soll, folgt weder aus Code noch Optionstext. Das ist eine Festlegung, keine Erhebung.

**Erreichter Prüfgrad:** statische Prüfung am Quelltext, jede Zelle der Matrix belegt. Keine Laufzeitbeobachtung.

---

### A58 · Die Nur-Heiler-Hartwirkmodi messen die falsche Menge (11.09.2026)

**Anlass:** Zwei Präzisierungen des Auftraggebers zur Use-Case-Analyse aus A57: Die Einstellung, außerhalb der Gruppe wiederzubeleben, sei „auch bei einem Heiler in der Gruppe oder sogar solo" mitzudenken; und wenn in anderen Gruppen einer Allianz kein Heiler und kein Rezzer mehr lebe, solle den Einstellungen entsprechend wiederbelebt werden.

**Befund.** `HardCastOnlyHealer` und `HardCastOnlyHealerSwiftCooldown` bauten zwei Mengen toter beziehungsweise vorhandener **Heiler der eigenen Gruppe** und verglichen ihre Größe. Die gemeinte Frage ist eine andere: Hartwirken kostet acht Sekunden GCD und lohnt nur, wenn es sonst niemand übernehmen kann. Drei Abweichungen:

1. **Heiler statt Rezzer.** Ein lebender Beschwörer oder Rotmagier belebt ebenso wieder. Der Baum weiß das an anderer Stelle bereits — `PheonixDownItem.AnyLivingRaiserInParty` zählt Heiler, SMN und RDM.
2. **Der Spieler war aus beiden Mengen gefiltert.** Als einziger Heiler sind damit beide leer: `0 == 0` trifft zu, die Nachbedingung `deadhealers.Count > 0` nicht. **In jeder Vierergruppe und solo wurde nie hart gewirkt** — genau dort, wo Hartwirken der einzige Weg ist, dass überhaupt jemand hochkommt.
3. **Ein Größenvergleich kann „niemand sonst" nicht ausdrücken,** nur „so viele tot wie vorhanden".

**Bezugsmenge — hier hat der Auftraggeber eine erste, zu enge Fassung korrigiert.** Der erste Entwurf beschränkte die Prüfung auf die eigene Gruppe, begründet damit, dass fremde Heiler keine verlässliche Reserve seien. Das trifft für die offene Welt zu, nicht für einen Allianzraid: Dort sind die anderen Allianzen echte Gruppen mit eigenen Rezzern, und solange dort einer lebt, ist die Wiederbelebung deren Sache — lebt keiner mehr, ist Hartwirken der einzige Weg. Die Bezugsmenge folgt deshalb `RaiseType`: Gruppe allein bei `PartyOnly` und `PartyHealersOnly`, Gruppe und Allianz bei den Allianzmodi und `All`. `AllOutOfDuty` bleibt bewusst ausgenommen — Fremde in der offenen Welt sind keine Reserve, und sie mitzuzählen blockierte das Hartwirken praktisch immer.

**Umsetzung.** `AnyOtherLivingRaiser()` mit `HasLivingRaiser(members)` als Mengenprüfung, in allen vier Zweigen. Nebenbei behoben: Die Bedingungsreihenfolge rief bisher `RaiseSpell` — mit seinen Nebenwirkungen auf `Target` und `ShouldEndSpecial` — **vor** der Mengenprüfung auf; jetzt steht die nebenwirkungsfreie Prüfung vorn.

**Nicht mitbehoben und weiter offen:** Der Optionstext verspricht zusätzlich „while Swiftcast is on cooldown", was der Code nicht prüft. Ob der Vorbehalt in die Bedingung gehört oder aus dem Text zu streichen ist, ist eine Festlegung über die Bedeutung der Einstellung; die Existenz von `HardCastOnlyHealerSwiftCooldown` spricht dafür, dass er gemeint war. In `TODO.md` erfasst.

**Phönixfeder — Absicht bestätigt, Verdrahtung bleibt offen.** Der Auftraggeber hat die gemeinte Bedingung genannt: kein Rezzer in der Gruppe oder alle tot, soweit der Inhalt es zulässt. Beides ist im Code bereits vorhanden — `CanUseThis` prüft `!AnyLivingRaiserInParty()`, und `BaseItem.CanUse` fragt `GetActionStatus` gegen `ConfigurationHelper.BadStatus`, deckt die Inhaltssperre also ab. Nicht vorhanden ist der Aufruf. Bei der Prüfung der Einhängung kam ein zweiter Defekt zutage: Das Hausmuster für Gegenstände (`CustomRotation_Ability.cs:361`) **meldet** eine Aktion, `UsePhoenixDown` dagegen **wirkt selbst** und setzt zusätzlich `act` — eine Einhängung nach Hausmuster verbrauchte die Feder zweimal. Der Grund für das Selbstwirken entfällt zudem, weil `BaseItem.Use` die Feder (Item 4570) bereits eigens auf `DataCenter.DeathTarget` wirkt. Deshalb nicht in den laufenden Testzweig aufgenommen, sondern als eigener Vorgang erfasst.

**Erreichter Prüfgrad:** statische Prüfung, Fallunterscheidung je `RaiseType` am Quelltext belegt, Prüfskripte, CI-Kompilierung. Keine Laufzeitbeobachtung.

---

### A59 · Die Phönixfeder verdrahtet, und das Doppelwirken dabei beseitigt (11.09.2026)

**Anlass:** Der Auftraggeber hat die gemeinte Bedingung genannt — kein Rezzer in der Gruppe oder alle tot, dann Feder, soweit der Inhalt es zulässt — und sie an einem Szenario ausgeschrieben: In einer Achtergruppe eines Allianzraids sind die eigenen Heiler tot und niemand in der eigenen Gruppe kann wiederbeleben; dann soll ein Federträger einen Heiler hochbringen, bei mehreren Trägern auch den zweiten, und wenn auch in den anderen Allianzgruppen keine Heiler und Rezzer mehr leben, ebenso dort.

**Ausgangslage.** Die Bedingung stand bereits richtig im Code (`PheonixDownItem.CanUseThis` mit `!AnyLivingRaiserInParty()`), und die Inhaltssperre ebenfalls: `BaseItem.CanUse` fragt `GetActionStatus` gegen `ConfigurationHelper.BadStatus`, das Spiel meldet also selbst, wenn Gegenstände im Inhalt verboten sind. Gefehlt hat allein der Aufruf — `UsePhoenixDown` hatte keinen.

**Der Defekt, der die Verdrahtung blockiert hätte.** `UsePhoenixDown` wirkte die Feder **selbst** (`phoenixdown.Use()`) und setzte zusätzlich `act`. Das Hausmuster für Gegenstände ist aber das Melden — `CustomRotation_Ability.cs` hängt den Heiltrank über `UseHpPotion(nextGCD, out act)` ein, und `RSCommands.DoAction` ruft `Use()` auf dem gemeldeten Ergebnis auf. Eine Einhängung nach Hausmuster hätte also **zwei Federn für eine Leiche** verbraucht. Der Grund für das Selbstwirken entfällt zudem: `BaseItem.Use` führt für Item 4570 einen eigenen Zweig, der auf `DataCenter.DeathTarget` zielt und HQ wie NQ behandelt. Der `Use()`-Aufruf ist entfernt, die Methode meldet jetzt nur.

**Bezugsmenge vereinheitlicht.** `AnyLivingRaiserInParty` in `PheonixDownItem` und die neue Rezzer-Prüfung aus A58 stellten dieselbe Frage in zwei Fassungen. Beide zeigen jetzt auf `DataCenter.AnyLivingRaiser(bool excludeSelf)`: dieselbe Menge nach `RaiseType` — Gruppe bei `PartyOnly` und `PartyHealersOnly`, Gruppe und Allianz unter den Allianzmodi und `All`, `AllOutOfDuty` ausgenommen — und dieselben Rezzerjobs (Heiler, Beschwörer, Rotmagier). Der Unterschied liegt allein im Parameter, und er ist sachlich: Die Feder fragt „kann das niemand richtig", zählt den Spieler also mit, weil ein lebender Rezzer seinen Zauber statt eines Gegenstands benutzt; die Hartwirkmodi fragen „kann es jemand **anders**", dort ist der Spieler der Entscheidende.

**Einhängung.** Im Fähigkeitenpfad hinter der Heilung, vor dem Angriff: Jemanden am Leben zu halten geht vor, jemanden aufzusammeln kostet nur ein Einschiebefenster. Die Zielüberschreibung setzt `UsePhoenixDown` selbst und stellt sie wieder her. Die Fähigkeitensperre in `DoAction` greift nicht, weil sie `nextAction is BaseAction` prüft und ein `BaseItem` das nicht ist.

**Zum Szenario mit mehreren Federträgern:** Dass jeder Träger einen anderen Toten nimmt, ist nicht zu garantieren — jeder Client entscheidet für sich, und eine Feder wirkt ohne Wirkzeit, sodass der Wiederbelebungsstatus des Ziels erst im Folgeframe sichtbar wird. Die vorhandene Absicherung (`GetDeath` schließt Ziele mit laufendem Wiederbelebungsstatus aus) greift also erst danach. Ein gleichzeitiger Einsatz auf dasselbe Ziel bleibt möglich; das ist ein Verteilungsproblem ohne Koordination und nicht im Client lösbar.

**Erreichter Prüfgrad:** statische Prüfung, Ausführungsweg für gemeldete Gegenstände am Quelltext belegt (`DoAction` ruft `Use()`), Prüfskripte, CI-Kompilierung. **Keine Laufzeitbeobachtung.** Der Eingriff liegt auf demselben Zweig wie der Wiederbelebungsversuch aus A56 und die Mengenkorrektur aus A58 — auf Wunsch des Auftraggebers ohne Trennung in eigene Zweige. Schlägt der Spieltest fehl, sind drei ungemessene Änderungen gleichzeitig wirksam; das ist bei der Auswertung zu berücksichtigen.

---

### A60 · Zweiter Durchgang durch die Anwendungsfälle, nach einem Einwand des Auftraggebers (11.09.2026)

**Anlass:** Der Auftraggeber hat die Aussage aus A59, eine Verdrahtung nach Hausmuster hätte „zwei Federn" gekostet, mit einem Sachargument bestritten: Ein Wiederbelebungsvorgang dauert mehrere Sekunden, und er hat gefragt, ob die Zeit zwischen Wirken und Annahme gemeint sei. Der Einwand trifft (C38) und hat den Durchgang auf eine Achse gelenkt, die in A57 fehlte: die zeitliche.

**Zeitachse, jetzt belegt.** Maßgeblich ist nicht die Annahme durch den Gefallenen, sondern der Statuswechsel: `TargetFilter.GetDeath:105` schließt ein Ziel mit `StatusID.Raise` aus, sobald die Wirkung gelandet ist. Die Annahme selbst braucht keine eigene Behandlung; läuft der Status ungenutzt ab, wird der Leichnam wieder Kandidat, was richtig ist. Offen bleiben genau zwei Spannen, beide zwischen Absenden und Statusrückmeldung: das gleichzeitige Hartwirken zweier Rezzer auf dasselbe Ziel (Abstimmungsproblem ohne Koordination, nicht im Client lösbar) und der Serverumlauf nach einer Feder.

**Befund 1 — die Rezzereigenschaft ignorierte die Stufe.** `DataCenter.HasLivingRaiser` zählte jeden lebenden Heiler, Beschwörer und Rotmagier als Rezzer, während `CanRaise()` für den Spieler selbst seit jeher Stufe 12 beziehungsweise 64 verlangt. Ein Rotmagier unter 64 — jeder stufensynchronisierte Durchgang durch ältere Inhalte — hielt damit die Feder zurück, die dort der einzige Weg war. Beide Stellen teilen sich jetzt dieselben zwei Konstanten. Die Stufe eines Gruppenmitglieds stammt aus `ICharacter.Level`; ob der Wert in einem synchronisierten Inhalt die synchronisierte oder die wahre Stufe meldet, ist von hier nicht entschieden und ausdrücklich so vermerkt.

**Befund 2 — die Zieleignung der Feder war als Zauberfrage gestellt.** `PheonixDownItem.ItemCheck` und `UsePhoenixDown` fragten beide `ObjectHelper.CanBeRaised`, also `CanUseActionOnTarget` gegen den Zauber Wiederbelebung. Die Feder fragt jetzt den Spielclient nach sich selbst: `GetActionStatus(ActionType.Item, 4570, Ziel) == 0`. Das beantwortet Reichweite, Sichtlinie, Wiederbelebbarkeit, Inhaltsverbot und einen bereits laufenden Wiederbelebungsstatus in einer Frage — und deckt damit genau die Spanne ab, nach der der Auftraggeber gefragt hat. `== 0` statt der Liste `BadStatus`, weil eine zielbezogene Ablehnung in dieser Liste nicht vorkommt und sonst als Erlaubnis gelesen würde.

**Befund 3 — eine wirkungslose Zielüberschreibung.** `UsePhoenixDown` legte `TargetType.Death` um seine Schleife. `BaseItem` liest `DataCenter.DeathTarget` in `CanUse` wie in `Use` unmittelbar und kennt das Überschreibungssystem nicht; die Überschreibung wirkte nichts und täuschte eine Zielwahl vor. Entfernt.

**Erfasst, nicht behoben:** Der Sonderfall für `RaiseType.PartyAndAllianceHealers` in `GetPriorityDeathTarget` steht vor der Umkehrung durch `H2`, die damit in diesem einen Modus wirkungslos bleibt.

**Erreichter Prüfgrad:** statische Prüfung, Fremddokumentation (FFXIVClientStructs zu `UseAction`), Prüfskripte, CI-Kompilierung. Keine Laufzeitbeobachtung. Der Eingriff liegt auf demselben Zweig wie A56, A58 und A59.

---

### A61 · Eine Fehlerklasse aus der eigenen Abwicklung geschlossen (11.09.2026)

**Anlass:** Beim Verschieben zweier Hilfsmethoden nach `DataCenter` blieben ihre Rümpfe in `CustomRotation_GCD.cs` stehen. Dreißig Compilerfehler, alle Folge einer Stelle, und der Zweig stand bis zum Ende eines vollständigen Windows-Builds rot.

**Ursache am System, nicht an der Sorgfalt:** Zwischen der Bearbeitung und der CI sieht in dieser Arbeitsumgebung niemand die Datei an — es gibt kein lokales .NET. Eine Fehlerklasse, die eine Sekunde kostet, kostete deshalb eineinhalb Minuten Windows-Läufer und eine Runde Verzögerung.

**Behebung:** `.github/scripts/audit/check_cs_structure.py`, erster Schritt im Linux-Auftrag. Zwei Prüfungen, weil die erste die zweite nicht abdeckt: Klammerbilanz außerhalb von Kommentaren, Zeichenketten und Zeichenliteralen; und ein Anweisungsschlüsselwort auf Elementebene eines Typs, wo ausschließlich Deklarationen stehen dürfen — ein zurückgebliebener Rumpf mit zufällig ausgeglichener Bilanz fällt nur dort auf.

**Wirksamkeit gemessen, nicht behauptet:** gegen den tatsächlich kaputten Commit `c3e1126e` ausgeführt, nennt das Skript Zeile 450 zuerst — dieselbe Zeile, die der Compiler zuerst nannte. Der Selbsttest trägt alle drei Fälle: saubere Datei, verwaister Rumpf, verwaister Rumpf mit ausgeglichener Bilanz.

---

### A62 · Upstream 7.5.6.2 eingebunden und ausgewertet (11.09.2026)

**Anlass:** Rückfrage des Auftraggebers, ob das Upstream-Update eingebunden und analysiert wurde. Es war beides nicht — geprüft war nur auf Dateiebene, ob der Wiederbelebungspfad berührt ist.

**Zustand, frisch gemessen.** `origin/main` steht auf `1391de57` und ist mit Upstream bis `1896086b` (PR #1365) synchron. Ausstehend waren genau **zwei** Commits: `2fe925f4` „Beastmaster" und der Merge `317de0ed`, auf dem das Tag `7.5.6.2` sitzt. Die zuvor berichteten 21 Commits waren gegen eine tote lokale Referenz gemessen (C40).

**Umfang:** 7278 Einfügungen, 1311 Löschungen in 18 Dateien. Schwerpunkt ist der neue Job: `BestiaryHelper.cs`, `BST_Reborn.cs` an Stelle von `BSM_Reborn.cs`, ein stark erweitertes `BeastmasterRotation.cs` und die generierten Ressourcen.

**Berührung mit Fork-Abweichungen, regionsgenau geprüft statt über Dateiaktivität geschätzt:**

- `DataCenter.cs` +183 Zeilen, vollständig im Bereich ab Zeile 59 (Begleiter-Zustand: `ActivePet`, `BMPet`, `BMPetKinType`, `BMPetAffinity`, dazu auskommentierte Pet-Erkennung). Die Helfer dieses Vorgangs liegen ab Zeile 874. Keine Überschneidung, weder textlich noch sachlich.
- `ActionTargetInfo.cs`: `Range` wird zur Eigenschaft mit vier Beastmaster-Sonderfällen, vier Aktionen kommen in `IsSpecialAbility`, der Rest ist auskommentierter Fang-Code. Die im Konzept belegte Stelle zur manuellen Zielwahl ist unberührt; nur ihre Zeilennummer verschiebt sich.
- `ActionBasicInfo.Range` wechselt von den Tabellendaten (`_action.Action.Range`) auf die Clientabfrage `ActionManager.GetActionRange`. Das ist eine allgemeine Verhaltensänderung, aber der Leserkreis ist klein und liegt außerhalb des Kampfpfads dieses Forks: drei fremde Rotationen (`BeirutaNIN`, `BeirutaRDM`, `Rabbs_BLM`) und zwei Anzeigestellen. `ActionTargetInfo.Range`, das die Zielwahl benutzt, fragte den Client schon vorher.
- **`publish.yaml`: der eine Punkt mit Handlungsbedarf.** Upstream stellt Dalamud vom Staging- zurück auf den Release-Kanal. Die Fork-Versionslogik (numerische Fassung, Paketkennung mit `wsh`-Suffix, Abbruch bei fehlendem Suffix) hat der Merge erhalten, die Kanalzeile hat er übernommen. Damit lief `build.yaml` auf einem anderen Kanal als der Veröffentlichungspfad — genau die Bedingung, unter der `TODO.md` das Nachziehen vorsah. `build.yaml` ist nachgezogen, der Punkt aus `TODO.md` nach hier überführt.

**Prüfgrad:** Probe-Merge konfliktfrei (`git merge-tree --write-tree`, danach der wirkliche Merge ohne Konflikt), alle sechs Prüfskripte auf dem gemergten Baum grün, CI-Kompilierung. Keine Laufzeitbeobachtung; der neue Job ist nicht Teil des Nutzungsprofils und wurde nicht bewertet.

---

### A63 · Die Messung des Repository-Zustands abgesichert (11.09.2026)

**Anlass:** C40 — eine Zustandsaussage war gegen eine lokale Referenz gemessen, die seit dem 18.08.2026 niemand mehr fortgeschrieben hatte.

**Ursache am System.** Diese Arbeitsumgebung ist ein **langlebiger** Klon, kein Neuklon je Sitzung: Das Reflog von `main` trägt fünf eigene Upstream-Merges, und vier lokale Zweige zeigen auf Gegenstücke, die auf `origin` gelöscht sind. In einem solchen Klon ist jede lokale Referenz eine Aussage über die Vergangenheit. `main` wurde seit August ausschließlich **auf GitHub** durch Pull-Request-Merges fortgeschrieben; ein lokaler Zweig folgt dem nie von allein, und `git branch -vv` sagte es die ganze Zeit an: „behind 382".

**Behebung:** `.github/scripts/audit/check_sync_state.py`. Es holt beide Gegenstellen mit `--prune`, misst **HEAD** gegen `upstream/main` statt einen benannten lokalen Zweig, und benennt jeden lokalen Zweig, der hinterherhinkt oder dessen Gegenstück fort ist. Rückgabewert 1, wenn HEAD hinter Upstream liegt — das ist die Vorbedingung jeder Codeänderung. Der Selbsttest baut den Fall in einem Wegwerf-Repository nach: Gegenstelle wandert weiter, lokaler Zweig bleibt stehen, Erkennung wird verlangt.

**Nicht in der CI**, und das ist kein Versehen: Dort ist der Klon frisch und jede Referenz aktuell, es gäbe nichts zu finden. Der Fehler gehört zur dauerhaften Arbeitskopie.

**Mitbehoben:** Die lokale `main` ist auf `origin/main` nachgezogen (reines Vorspulen, 382 Commits, keine eigenen Commits darauf). Die vier Zweige mit gelöschtem Gegenstück und `backup/pre-msgfix` sind Löschfälle und dem Auftraggeber zur Freigabe vorgelegt, nicht gelöscht.

---

### A64 · Verwaiste lokale Zweige abgewickelt (11.09.2026)

**Anlass:** Die Zustandsmessung aus A63 legte fünf lokale Zweige offen, die niemand mehr braucht. Freigabe durch den Auftraggeber erteilt.

**Verifikation vor der Löschung, je Zweig.** Gezählt wurden Commits, die weder in `origin/main` noch in einem der beiden lebenden Arbeitszweige stecken:

| Zweig | Eigene Commits | Nachweis |
|---|---|---|
| `claude/bmr-mitigation-refresh` | 0 | vollständig in `origin/main` |
| `claude/release-tag-limits` | 0 | über PR #5 gemergt |
| `claude/rotation-flow-refactor` | 0 | über PR #4 gemergt |
| `backup/pre-msgfix` | 8 | Sicherungsstand vor dem Umschreiben der Commit-Nachrichten; alle acht Titel einzeln in `origin/main` wiedergefunden, nur unter anderen Hashes |
| `claude/release-notes` | 2 | PR #6, vom Auftraggeber ohne Merge geschlossen |

**Der einzige Zweig mit eigenem Inhalt war `claude/release-notes`,** und auch dort geht nichts verloren: GitHub hält die Köpfe geschlossener Pull Requests dauerhaft unter `refs/pull/<n>/head`. Gemessen statt angenommen — `git ls-remote origin refs/pull/6/head` liefert genau `d84759c0`, den Kopf des gelöschten Zweigs; für die PRs #3, #4 und #5 ebenso. Das ist zugleich die allgemeine Auflösung für künftige Fälle dieser Art: Ein Zweig, dessen Arbeit durch einen Pull Request gelaufen ist, ist auch nach dem Schließen kein Verlustfall.

**Eine Erkenntnis wurde vor der Löschung gerettet.** `d84759c0` trug eine Verschärfung in `CLAUDE.md`, die im geltenden Stand fehlte: Ein Release lässt sich von hier aus nicht nur nicht auslösen, sondern auch nicht nachträglich beschriften — `PATCH` auf einen Release antwortet `403 Creating, editing, or deleting releases is not permitted for this session type`. Sie ist übernommen. Der zweite Teil jenes Zweigs — Titel und Text für die Release-Seite in `publish.yaml` — bleibt verworfen: Der Auftraggeber hat den Pull Request selbst geschlossen, und der Inhalt ist über PR #6 einsehbar, falls er ihn doch will.

**Nicht gelöscht:** `claude/raise-swiftcast-weave` — er trägt PR #7 und ist der benannte Rückfallstand für den laufenden Wiederbelebungsvorgang.

**Nebenbefund, ebenfalls gemessen:** Der Probe-Zweig `tmp-push-probe`, den diese Umgebung auf `origin` nicht selbst löschen konnte, ist fort. `git branch -r` führt neben den beiden Arbeitszweigen nur noch `origin/main`. Der offene Punkt aus PR #5 ist damit erledigt.

---

### A65 · Die Fork-Version benennt wieder den Upstream-Stand, den sie enthält (11.09.2026)

**Anlass:** Rückfrage des Auftraggebers, ob im Arbeitszweig `7.5.6.2` gesetzt ist. War es nicht.

**Befund.** `Directory.Build.props` trug weiter `7.5.6.1`, obwohl der Zweig seit A62 den Upstream-Stand 7.5.6.2 enthält. Das ist die Fehlerform, die der Kommentar derselben Datei bereits beschreibt — „It named 7.5.6.0 while the tree already held 7.5.6.1" —, zum zweiten Mal aufgetreten. Ursache am Ablauf: Der Upstream-Merge zog die Zahl nicht nach, und `check_fork_version.py`, das genau das meldet, wurde nach dem Merge nicht ausgeführt, obwohl die Build-Ausgabe ausdrücklich dazu auffordert. Literale auf `7.5.6.2` gezogen, in allen drei Feldern samt `+wsh1`- und `-wsh1`-Markierung.

**Warum die Prüfung nicht von allein lief, und was daran der eigentliche Befund ist.** Sie hing in keinem Arbeitsablauf. Der Grund dafür liegt tiefer, als er zunächst aussah, und die erste Fassung dieser Einhängung trug bereits die falsche Begründung: Es ist nicht nur die flache Klonung der CI. **Die eigene Gegenstelle trägt überhaupt keine Upstream-Tags** — `git ls-remote --tags origin` liefert ausschließlich `7.5.5.41+wsh1` und `7.5.6.1+wsh1` —, und Tags lassen sich von hier aus nicht pushen. Ein `git describe` findet dort also auch mit vollständiger Historie nichts.

**Behebung, dreiteilig:**

1. `fetch-depth: 0` im Linux-Auftrag, damit die Historie überhaupt vorliegt.
2. Ein eigener Schritt, der `upstream` hinzufügt und dessen Tags holt. Ohne ihn ist die Frage in der CI nicht beantwortbar.
3. **Der stille Nullbefund wird laut.** `check_fork_version.py` gab bei fehlenden Tags `0` zurück, meldete also Erfolg, ohne geprüft zu haben — in der CI wäre die Prüfung grün gewesen und wirkungslos, was schlechter ist als keine Prüfung. Der neue Schalter `--require-tags`, den allein die CI setzt, macht daraus einen Fehlschlag; die nachsichtige Fassung bleibt für Arbeitskopien ohne Upstream-Gegenstelle.

**Erreichter Prüfgrad:** Skript gegen beide Ausgänge ausgeführt (mit Tags grün, ohne Tags und mit Schalter rot), Arbeitsablauf gegen einen YAML-Parser geprüft, alle sieben Prüfskripte grün. Ob der Tag-Abruf auf dem Läufer durchgeht, zeigt erst der Lauf selbst.

---

### A66 · Erste Laufzeitbeobachtung zum zweiten Wiederbelebungsversuch (11.09.2026)

**Meldung des Auftraggebers:** „schimmerschild klappt bislang, rezz klappt bislang automatisch."

**Was das belegt.** Die Regression aus C37 ist nicht zurückgekehrt: Radiant Aegis wird weiter gewirkt, der Einschub im Fähigkeitenpfad verdrängt sie also nicht. Das ist der Punkt, an dem der erste Versuch gescheitert ist, und die Bestätigung ist genau dort wertvoll, wo der Entwurf sie beansprucht hat — `nextGCD` bleibt unangetastet, seine 447 Leser ebenfalls. Und die Wiederbelebung erfolgt im Automatikbetrieb, ohne Umschalten auf manuell.

**Was das nicht belegt, und der Unterschied ist der Kern des Vorgangs.** Gemeldet ist, *dass* wiederbelebt wird, nicht *wie schnell*. Die ursprüngliche Beanstandung lautete „es dauert manchmal über 15 Sekunden", nicht „es geschieht nicht". Solange die Dauer nicht beurteilt ist, bleibt der gemeldete Defekt unbestätigt behoben. Das Wort „bislang" in beiden Hälften ist ebenfalls ernst zu nehmen: eine vorläufige Beobachtung, kein abgeschlossener Test.

**Abdeckung des Tests, gegen die Einstellungsvorgaben geprüft.** Von den fünf Eingriffen auf dem Zweig kann diese Beobachtung nur zwei berühren — den Spontanitäts-Einschub (A56) und, negativ, die Nichtverdrängung anderer Fähigkeiten. Die drei übrigen liegen hinter Einstellungen abseits der Vorgabe: die Phönixfeder mit Zieleignung und Stufenprüfung (`UsePhoenixDown` ist ab Werk aus), die Hartwirk-Korrektur (nur bei abgeschaltetem `RaisePlayerBySwift`) und die Bezugsmenge der Nur-Heiler-Modi. Sie bleiben ungemessen, und das ist bei der Auswertung eines späteren Fehlschlags zu berücksichtigen.

**Erreichter Prüfgrad:** Laufzeitbeobachtung des Auftraggebers für einen Teil der Wirkung, vorläufig. Keine Aussage zur Dauer, keine Beobachtung der drei einstellungsabhängigen Eingriffe.

---

### A67 · Searing Light bei mehreren Beschwörern, Fälle eins bis acht (11.09.2026)

**Anlass:** Frage des Auftraggebers, wann die Aktion gewirkt wird und was bei einer Gruppe aus acht Beschwörern geschähe, die alle diesen Fork laufen haben; anschließend präzisiert auf alle Gruppengrößen von eins bis acht.

**Zum Namen:** Der Auftraggeber nennt die Aktion „Gleißender Schein". Der Job-Guide ist vom Egress gesperrt — erneut geprüft, nicht erinnert —, eine belegte Zuordnung steht also nicht zur Verfügung. Dass Searing Light gemeint ist, ist aus der Fragestellung geschlossen und im Konzept als Schluss gekennzeichnet; gearbeitet wird mit dem englischen Bezeichner.

**Kette vollständig erhoben:** Zündung unter `burstInSolar` (`SMN_Reborn.cs:204`), Aktionseinstellung mit `StatusProvide = [SearingLight]` und `StatusFromSelf = false` (`SummonerRotation.cs:490`), Sperrlogik in `IsStatusProvided` (`ActionBasicInfo.cs:691`), Vorgabewerte `ShouldCheckStatus = true` und `StatusRefreshGcdCount = 2` (`ActionConfig.cs:50`, `:66`), Quellenfilter in `PlayerGetStatus` (`StatusHelper.cs:1529`). Die Beschwörung ist ihrerseits an Searing Lights Wiederholzeit gekoppelt (`SMN_Reborn.cs:467`).

**Befund: Der Doppelzündungsschutz ist richtig gebaut, seine Folge nicht behandelt.** `StatusFromSelf = false` sperrt korrekt gegen jeden fremden Buff; `HasSearingLight` zählt korrekt nur den eigenen. Beide Einstellungen sind für ihre jeweilige Frage richtig — und aus ihrem Zusammentreffen entstehen ab zwei Beschwörern drei Verluste: kein Nachzünden im selben Zyklus (der fremde Buff hält 20 Sekunden, die eigene Beschwörung 15), keine bevorzugte Aetherflow-Ausgabe im laufenden fremden Fenster, und dadurch kein Ruby's Glimmer und kein Searing Flash.

**Ab sechs Beschwörern** wäre durchgehende Buff-Abdeckung möglich (sechs mal zwanzig Sekunden gleich eine Wiederholzeit); genau die entgeht.

**Vorgelegt:** V1 (fremden Buff als Buff-Fenster behandeln) mit Empfehlung zur Umsetzung — Abweichung zwischen Absicht und Umsetzung, bei einem Beschwörer wirkungslos, ohne erkennbaren Nachteil. V2 (Zündung von der eigenen Beschwörung lösen) mit Empfehlung dagegen — Verhaltensänderung ohne Nachweismöglichkeit, die im Regelfall schadet. Nullvariante geprüft und für V1 verworfen, weil zwei Beschwörer in einer Gruppe gewöhnlich sind.

**Nicht umgesetzt**, weil der Auftrag die Erarbeitung im Konzept war und ein sachfremder sechster Eingriff die Auswertung des offenen Spieltests am Wiederbelebungspfad beschädigt hätte.

**Erreichter Prüfgrad:** statische Prüfung am Quelltext für die gesamte Kette; Wirkdauer, Wiederholzeit und Stärke von Searing Light, Standzeit von Solar Bahamut und Herkunft von Ruby's Glimmer aus Fremdquellen. Keine Laufzeitbeobachtung. Nicht entschieden: welcher Ausgang bei gleichzeitiger Zündung eintritt.

---

### A68 · Die Sync-Messung erkannte einen flachen Klon nicht (11.09.2026)

**Anlass:** Die Arbeitsumgebung wurde mitten in der Sitzung gegen einen **neuen, flachen** Klon getauscht — die lokalen Zweige waren fort, `main` stand auf einem alten Stand, `upstream` war als Gegenstelle nicht eingerichtet. Das widerlegt zugleich die Annahme aus A63, die Arbeitskopie sei durchgehend langlebig; die daraus gezogene Regel — lokale Referenzen sind kein Zustandsnachweis — gilt dadurch erst recht.

**Befund am eigenen Prüfmittel.** `check_sync_state.py` meldete „HEAD is up to date with upstream/main" und „0 behind, 123 ahead". Nach `git fetch --unshallow` lauteten dieselben Zahlen „0 behind, **390** ahead". In einem flachen Klon läuft `rev-list` über Historie, die nicht da ist; das Ergebnis sieht nicht falsch aus, es ist falsch. Dass die erste Zahl zufällig stimmte, macht den Fall schlimmer statt besser — genau die Form des stillen Nullbefunds, gegen die `check_fork_version.py` kurz zuvor mit `--require-tags` abgesichert worden war, nur an der nächsten Stelle.

**Behebung:** Das Skript prüft `git rev-parse --is-shallow-repository` und bricht mit Rückgabewert 1 ab, bevor es irgendeine Zahl nennt. Der Selbsttest trägt den Fall jetzt mit: ein `--depth 1`-Klon eines Wegwerf-Repositorys muss als flach erkannt werden.

---

### A69 · Searing Light, zweiter Durchgang: Zeitstruktur, alternative Fenster, Versatz (11.09.2026)

**Anlass:** Der Auftraggeber hat A67 in vier Punkten vertieft: Searing Light stapelt nicht, sondern überschreibt; der fremde Buff ist für die eigene Burstphase nutzbar (Bestätigung von V1); ab wann passt die Zündung nicht mehr in die genutzten Fenster, und welche anderen Schadensphasen kämen in Frage; die Gruppenzusammensetzung wäre vorab zu prüfen; und zwischen den Rotationen entsteht Versatz durch Tod, Bewegung und Betäubung. Auftrag: jede Frage in einem eigenen vollständigen Loop und zusätzlich gesamtheitlich.

**Die Zeitstruktur war die fehlende Grundlage.** A67 hatte die Verluste benannt, aber nicht beziffert, warum sie mit der Zahl der Beschwörer nicht wachsen. Der Grund ist eine Asymmetrie: Große Beschwörungen stehen 15 Sekunden und kehren alle 60 wieder, in der Reihenfolge Solar Bahamut, Bahamut, Solar Bahamut, Phoenix. Solar Bahamut kommt damit alle 120 Sekunden — genau so oft wie Searing Light selbst. Da `SMN_Reborn.cs:203` ausschließlich dort zündet, hat jeder Beschwörer **eine** Gelegenheit je Wiederholzeit, und bei synchronen Rotationen fallen alle zusammen.

**Folge, gegen die Erwartung:** Die Abdeckung bleibt bei 20 Sekunden je 120 — bei zwei Beschwörern wie bei acht. Der gesamte Zuwachs an Ladungen verfällt. Die Schwelle liegt bei **zwei**, nicht bei einer höheren Zahl.

**Zum Überschreiben.** Die Angabe des Auftraggebers schärft den Befund: Der Wert einer zweiten Zündung hängt allein vom Zeitpunkt ab — sofort null, nach zehn Sekunden zehn, nach Ablauf voll. Der vorhandene Sperrmechanismus nutzt genau das, weil er zwei GCDs vor Ablauf öffnet (`StatusRefreshGcdCount = 2`). Er ist richtig gebaut und bleibt unangetastet; er ist zugleich das einzige Abstimmungsmittel zwischen Clients, die einander nicht kennen.

**Alternative Fenster — und die Rotationsbasis führt sie bereits.** `BahamutBurst` (`SummonerRotation.cs:231`) ist ab Stufe 100 in jeder großen Beschwörung wahr. `ChurinSMN` benutzt sie (`:948`), `SMN_Reborn` nicht. Die Erweiterung bringt ein zweites Fenster bei Sekunde 60 und verdoppelt die Abdeckung auf 40 Sekunden je 120. Sie unverändert zu übernehmen wäre allerdings falsch: `BahamutBurst` ist zusätzlich an `CanBurst` und damit an `AutoStatus.Burst` gebunden (`StateUpdater.cs:847`), was `burstInSolar` heute nicht prüft — zwei Verhaltensänderungen in einer Zeile sind bei einem fehlschlagenden Spieltest nicht auseinanderzuhalten.

**Gruppenzusammensetzung als Schalter, und warum sie nötig ist.** Bei einem einzelnen Beschwörer ist die Erweiterung nicht neutral: Wird seine Wiederholzeit frei, während Bahamut oder Phoenix steht, zündet er künftig dort, verliert die Bündelung mit seinem stärksten Fenster und fällt aus dem Zwei-Minuten-Takt. Die Prüfung über `DataCenter.PartyMembers` und `IsJobs(Job.SMN)` — dasselbe Muster wie `HasLivingRaiser` — schaltet die Erweiterung nur, wenn die Voraussetzung tatsächlich vorliegt. Das ist der von der Projektregel verlangte Feature-Toggle, nur an der Lage statt am Nutzer.

**Versatz: der erwartete Befund trat nicht ein.** Tod, Bewegung, Betäubung und zielfreie Phasen streuen die Beschwörungsfenster über die Zeit. Damit ist der Versatz kein Problem, sondern der Verbündete der Erweiterung — mehr Fenster fallen in Zeiten ohne laufenden Buff, und der Sperrmechanismus filtert sie korrekt. Eine ausdrückliche Staffelung zwischen Spielern ist weder nötig noch möglich: Kein Client kennt die Wiederholzeiten der anderen.

**Gesamtheitlich:** V1 und V2 wirken in getrennte Richtungen (was der Gesperrte tut / wann er zünden darf), stören einander nicht und verstärken sich — mit V2 liegt häufiger ein fremder Buff, was V1 häufiger wirksam macht. Beide bleiben innerhalb der Beschwörer-Rotation; Basisklasse, Aktionseinstellungen und Sperrmechanismus bleiben unberührt. Betroffen ist allein der Endnutzer als Beschwörer.

**Benannte Grenze:** Auch mit beiden Vorschlägen bleibt die Abdeckung bei 33 % statt der theoretisch möglichen 100 %. Der Rest setzt Absprache zwischen den Spielern voraus, die ein Rotationshelfer nicht herstellen kann.

**Nicht umgesetzt**, weil der Auftrag die Konzeptarbeit war und der laufende Zweig fünf ungemessene Eingriffe am Wiederbelebungspfad trägt.

**Erreichter Prüfgrad:** statische Prüfung am Quelltext für die gesamte Kette einschließlich der ungenutzten `BahamutBurst`-Fassung und der Burst-Bindung; Zeitgrößen und Beschwörungsreihenfolge aus Fremdquellen; das Überschreiben nach Angabe des Auftraggebers. Keine Laufzeitbeobachtung. Die Abdeckungszahlen sind Abschätzungen aus diesen Größen, keine Messungen.

---

### A70 · Die Abdeckung gemessen statt geschätzt, und zwei Ansätze mehr geprüft (11.09.2026)

**Anlass:** Der Auftraggeber hat nachgerechnet — 15 Sekunden Wirkung bei 120 Sekunden Wiederholzeit ergäben 135, geteilt durch acht Beschwörer knapp 17 Sekunden, also müsste der Buff dauerhaft stehen können. Anschließend die Aufforderung, die Gruppengrößen zwei bis acht zu prüfen und alternative Ansätze zu finden, und schließlich der Hinweis, dass die Wiederholzeit eines anderen Beschwörers ab dessen erster Zündung bekannt ist.

**Zwei Zahlen der Rechnung sind falsch, die Schlussfolgerung ist richtig.** Searing Light wirkt 20 Sekunden, nicht 15 — die 15 sind die Standzeit der Beschwörung. Die Wiederholzeit läuft ab der Zündung, nicht ab Buff-Ende; ein Intervall von 135 Sekunden existiert nicht. Die Schwelle für rechnerisch lückenlose Abdeckung liegt damit bei **sechs** Beschwörern (6 × 20 s = 120 s), nicht erst bei acht. Dass bei acht nahezu dauerhafte Abdeckung möglich ist, bestätigt die Messung.

**Prüfmittel statt Kopfrechnung.** `.github/scripts/audit/searing_light_coverage.py` rechnet die Regeln durch: Wirkung, Wiederholzeit, Überschreiben statt Stapeln, Beschwörungsfenster, Sperrverhalten, Versatz. Es hat drei Aussagen dieses Vorgangs korrigiert, die alle plausibel klangen:

- Die Behauptung, die Abdeckung bleibe auch mit V2 bei 33 %, gilt nur für den synchronen Pull; bei auseinandergelaufenen Rotationen erreicht V2 bei sieben und acht Beschwörern 90 bis 96 % (C41).
- V4 — die Bindung an die Beschwörung ganz zu lösen — war in A69 mit einem Argument abgetan worden. Gemessen ist es bei **zwei** Beschwörern sogar schlechter als V2 (29 gegen 33 %), weil frühere Zündungen die Wiederholzeiten ungünstiger legen. Das Argument war richtig, die Begründung nicht.
- Die erste Fassung der informierten Regel V5 erlaubte das Zünden, sobald die Sperre sich löst, also in den letzten fünf Sekunden des laufenden Buffs. Sie fiel damit unter V2: Außerhalb eines Fensters verbrennt das eine volle Ladung für wenige Sekunden Gewinn. Die Regel verlangt jetzt einen vollständig abgelaufenen Buff.

**V5, der Ansatz des Auftraggebers, ist tragfähig und wird trotzdem nicht empfohlen.** Die Information ist tatsächlich verfügbar — `IStatus.SourceId` benennt den Urheber, `PlayerGetStatus` liest ihn bereits —, und die Regel ist bei drei bis sechs Beschwörern die beste aller geprüften (50 bis 99 % gegenüber 33 % heute). Dagegen stehen drei Befunde: Im Nutzungsprofil von einem bis zwei Beschwörern bringt sie gegenüber V2 **nichts**; bei sieben und acht bricht sie auf 60 % ein, weil die Staffelung in eine ungünstige Selbstorganisation läuft; und sie verlangt ein Gedächtnis über Frames hinweg mit Rücksetzpunkten bei Kampf-, Gruppen- und Zonenwechsel.

**Befund über das Modell, nicht nur damit:** Mehr Beschwörer bedeuten nicht immer mehr Abdeckung. Gierige Zuteilung — wer zuerst in einem Fenster steht, zündet — kann jemandem zuvorkommen, dessen Wiederholzeit eine spätere Lücke gedeckt hätte. Der Selbsttest prüft diese Eigenschaft deshalb ausdrücklich nicht; eine frühere Fassung behauptete sie und war widerlegt.

**Zweiter Befund am eigenen Werkzeug:** Die Vergleiche im Selbsttest liefen zunächst mit einer Toleranz von 1e-9 gegen ein Modell mit 0,1-Sekunden-Raster und schlugen auf einen Unterschied von 0,02 Prozentpunkten an. Eine Prüfung, die enger ist als die Auflösung ihres Gegenstands, misst das Raster statt die Sache. Toleranz auf die Größenordnung eines Rasterschritts gesetzt und begründet.

**Unverändert empfohlen bleiben V1 und V2.** Beide wirken im tatsächlichen Nutzungsprofil, beide sind ohne Zustandshaltung umsetzbar, und keine der drei Messungen hat etwas gegen sie ergeben.

**Erreichter Prüfgrad:** Modellrechnung mit Selbsttest gegen vier Invarianten; alle Eingangsgrößen aus dem Quelltext oder aus den im Konzept benannten Fremdquellen. Das Modell zählt Sekunden mit Buff, nicht Schaden — ein Buff außerhalb des Zwei-Minuten-Takts buffft weniger davon, was in keiner der Zahlen erscheint. Keine Laufzeitbeobachtung.

---

### A71 · Der maßgebliche Bereich ist eins bis fünf, und das kehrt die Empfehlung um (11.09.2026)

**Anlass:** Der Auftraggeber hat den Betrachtungsbereich begründet eingegrenzt und eine zweite Bedingung gelockert. Eine reguläre Achtergruppe trägt vier bis fünf Schadensklassen, eine Vierergruppe zwei; sechs bis acht Beschwörer sind Sondergruppen außerhalb des regulären Spiels. Und: Die Verteilung muss sich nicht sofort einstellen — Raidkämpfe dauern bis zu zwanzig Minuten, Ultimates bis zu vierzig, die Ablösezeiten dürfen sich einpendeln.

**Die Eingrenzung kehrt die Bewertung von V5 um.** In A70 war V5 mit drei Gründen abgelehnt worden; zwei davon fallen mit dem Bereich weg. Der Einbruch auf 60 % trat bei sieben und acht Beschwörern auf — kein regulärer Spielbetrieb. Und „der Gewinn liegt bei drei bis sechs" ist kein Einwand mehr, wenn der Bereich bei fünf endet.

**Was die Messung im Bereich eins bis fünf zeigt:** V5 trifft die Obergrenze der Ladungen auf den Punkt — 17, 33, 50, 66, 83 Prozent gegen eine Obergrenze von 17, 33, 50, 67, 83. Mehr ist aus den vorhandenen Ladungen nicht zu holen. V2 bleibt im synchronen Fall ab zwei Beschwörern bei 33 %, liegt ab drei also unter dem Erreichbaren, bei fünf Beschwörern um fünfzig Prozentpunkte.

**Zweiter Befund derselben Messung, gegen die Erwartung:** Versatz nützt nur dem heutigen Code. Bei voll auseinandergelaufenen Rotationen liegen alle vier Regeln gleichauf — wo die Fenster ohnehin gestreut sind, finden die Erweiterungen nichts mehr vor. Umgekehrt heißt das, dass V5 genau dort stark ist, wo der heutige Code am schwächsten ist: beim sauberen, synchronen Pull, also zu Beginn jedes Kampfes.

**Zum Einpendeln, gemessen über zwanzig und über vierzig Minuten:** Es findet nicht statt, und es ist auch nicht nötig. Erste zwei Minuten und letzte zwei Minuten liefern bei jeder Regel und jeder Gruppengröße denselben Wert; V5 liegt von der ersten Periode an auf seinem Endwert. Bei V2 ändert sich ebenfalls nichts, aber aus dem gegenteiligen Grund — bei festem Versatz bleibt das Muster, in dem es begonnen hat.

**Was das Modell dabei nicht kann, und das ist zu benennen:** Es hält den Versatz über den ganzen Lauf fest. Real wächst er mit der Kampfdauer, weil jede Mechanik und jeder Tod die Zyklen weiter verschiebt. Eine Gruppe wandert im Lauf eines langen Kampfes also von der synchronen in die versetzte Tabelle. Die Richtung steht fest — sie verbessert die Lage —, das Tempo nicht. Der lange Kampf ist damit der Fall, der sich von selbst bessert; der Anfang jedes Kampfes ist der, der es nicht tut.

**Neue Empfehlung, in zwei Stufen:** Erst V1 und V2 — ohne Zustandshaltung, wirksam im häufigsten Fall von ein bis zwei Beschwörern, einzeln im Spiel beurteilbar. Dann V5 darauf, das im Bereich drei bis fünf die Obergrenze erreicht. V5 ist als „V2 plus eine zusätzliche Erlaubnis" gebaut und im Modell auch so gemessen, die Stufen sind also unabhängig prüfbar.

**Erfasst, nicht behoben:** V5 hängt daran, den fremden Buff zu sehen. Searing Light reicht dreißig Yalm; zündet ein Beschwörer weiter entfernt, fehlt die Beobachtung. Die Fehlerrichtung ist die zurückhaltende — ein bekannter Beschwörer, dessen Zündung verpasst wurde, gilt als bereit und hält die eigene Zündung zurück.

**Erreichter Prüfgrad:** Modellrechnung über zehn, zwanzig und vierzig Minuten mit Selbsttest gegen vier Invarianten. Keine Laufzeitbeobachtung. Das Modell zählt Sekunden mit Buff, nicht Schaden.

---

### A72 · Ausweichregel, Kampfgebiet und eine geregelte statt gesteuerte Fassung (11.09.2026)

**Anlass:** Drei Nachfragen des Auftraggebers — ob es eine Fokussierung auf Bahamut und Phoenix gibt, falls Solar bereits durch einen anderen abgedeckt war; wie groß ein Raid- oder Prüfungsgebiet üblicherweise ist; und ob das Konzept dynamischer zu bauen wäre, mit unterschiedlichen Richtlinien bei Abweichungen im Verlauf.

**Zur Ausweichregel: Es gibt keine, und die Formulierung des Auftraggebers ist genauer als die bisherige.** `SMN_Reborn.cs:205` ist die einzige Zündstelle, `burstInSolar` (`:203`) lässt ab Stufe 100 nur Solar zu; kein Zweig weicht aus, kein Zustand hält eine Blockade fest. Das Konzept hatte die Erweiterung als pauschale Lockerung beschrieben — „zünde in jedem Beschwörungsfenster" —, gemeint ist aber eine Ausweichregel: „weiche aus, falls Solar belegt war". Im Kollisionsfall sind beide deckungsgleich; sie gehen auseinander, wenn die eigene Wiederholzeit während Bahamut frei wird, ohne dass eine Kollision vorlag. Die genauere Fassung verlangt denselben Zustand, den V5 ohnehin mitbringt, und wird deshalb der zweiten Stufe zugeordnet. **Nicht gemessen**, weil das Modell alle Beschwörer mit freier Wiederholzeit startet und diesen Fall gar nicht erzeugen kann — die Aussage ist aus der Regel abgeleitet.

**Zum Kampfgebiet: keine belastbare Zahl gefunden.** Die Recherche nach einem üblichen Arenadurchmesser blieb ohne verwertbares Ergebnis, und im Quelltext steht er nicht. Die Angabe des Auftraggebers — keine langen Wege — wird als solche geführt.

**Der Einwand aus dem Reichweitenargument war überzeichnet und ist korrigiert.** Die Beobachtungslücke fällt mit der Wirkungslücke zusammen: Wer den Buff nicht bekommt, hat auch nichts von ihm, und für den ist die eigene Zündung dann richtig. Die vorhandene Sperre leistet das von selbst, weil sie den Status **auf dem Spieler selbst** prüft. Betroffen ist allein die Buchführung, und auch die nur außerhalb eines Beschwörungsfensters.

**Zur Dynamik: ja, und es ist der stärkste Einzelschritt nach V1.** V5 ist bereits halb geregelt — es zählt nur Beschwörer, die tatsächlich gezündet haben. Seine Lücke liegt in der anderen Richtung: Es **vergisst nicht**. Wer einmal gezündet hat, steht dauerhaft mit „kommt in 120 Sekunden wieder" in den Büchern; stirbt er danach oder hört auf zu zünden, halten sich alle anderen für eine Lücke zurück, die er nie füllt.

**V6 setzt ein Verfallsdatum.** Ist ein beobachteter Beschwörer um mehr als eine Buffdauer überfällig, zählt er nicht mehr; kommt er zurück, trägt seine nächste Zündung ihn wieder ein. Gemessen an einem dreiminütigen Ausfall, gemessen über dieses Fenster: bei drei Beschwörern 22 statt 11 Prozent, bei vier 44 statt 22, bei fünf 66 statt 33. **Das Verfallsdatum verdoppelt die Abdeckung im Störungsfenster.** Bei zwei Beschwörern ändert sich nichts, weil dort nach einem Ausfall nur einer übrig ist.

**Der Preis ist gering, weil der Zustand ohnehin geführt wird:** ein Zeitstempel je beobachtetem Beschwörer und eine Verfallsprüfung. Eine Buchführung, die nie vergisst, ist schlechter als gar keine — sie wird mit wachsender Kampfdauer immer falscher.

**Wo die Dynamik zu enden hat, ausdrücklich benannt:** Die Projektregel warnt vor Zustandsautomaten, die statisch nicht abzusichern sind; der Eintrag zur doppelten Zustandswahl ist genau daran hängengeblieben. V6 ist keiner — eine Größe je Beschwörer und zwei Ableitungen daraus. Weitergehende Richtlinien nach Lage, etwa ein Umschalten nach gemessener Abdeckung oder erkanntem Versatz, wären ein Automat und sind bewusst nicht vorgeschlagen.

**Stufe 2 der Empfehlung ist damit V6 statt V5.**

**Erreichter Prüfgrad:** Modellrechnung mit Selbsttest, jetzt auch gegen ein Ausfallszenario. Keine Laufzeitbeobachtung. Das Modell zählt Sekunden mit Buff, nicht Schaden, und kann den Fall der ungünstig liegenden Wiederholzeit nicht erzeugen.

---

### A73 · Der Wiederbelebungsdefekt ist im Spiel bestätigt behoben (11.09.2026)

**Beobachtung des Auftraggebers:** „die rezzgeschwindigkeit war in einigen fällen sofort, in anderen hat es ein paar sekunden gedauert."

**Gegen den Ausgangsbefund gehalten.** Gemeldet worden war: „manchmal machen heiler und sonstige rezzer keine rezzes sofort. es dauert manchmal über 15 sekunden oder länger, bis ein rezz durchgeführt wird" — bei laufendem Schaden und laufender Heilung. Der Zustand jetzt ist teils sofort, teils wenige Sekunden. **Der gemeldete Defekt ist damit behoben**, und zwar zusammen mit der zweiten Hälfte der Auflösungsbedingung, die A66 schon gestützt hatte: Es bleibt keine andere Fähigkeit aus, Radiant Aegis kommt weiter.

**Die verbleibende Wartezeit ist Bauart, nicht Rest des Defekts, und sie ist an der Kette belegt.** Drei Glieder bestimmen sie:

1. Der Einschub kann nur im Einschiebefenster greifen. `CustomRotation_Ability.cs:28` kehrt bei `0 < WeaponRemain <= 0.5f` sofort zurück, und bei freiem GCD ruft `Invoke` den Fähigkeitenpfad gar nicht erst auf. Gezündet wird also bei `WeaponRemain > 0,5 s`.
2. Danach geht die Wiederbelebung als nächster GCD hinaus — `RaiseSpell` Stufe (A) verlangt `HasSwift || IsLastAction(SwiftcastPvE)`. Bis dahin vergeht die Restzeit des laufenden GCD, also zwischen 0,5 Sekunden und einer vollen Wiederholzeit; bei laufendem Zauber mit Wirkzeit entsprechend mehr.
3. Mit der Vorgabe `RaisePlayerFirst = aus` liegt der Wiederbelebungsblock hinter der gesamten Heilung (`CustomRotation_GCD.cs:350`). Ist gleichzeitig zu heilen, gewinnt die Heilung den GCD, und die Wiederbelebung rückt einen weiteren GCD nach hinten.

„Sofort" tritt danach ein, wenn Spontanität bereits lag oder der GCD fast frei war; „ein paar Sekunden", wenn ein Zauber lief oder eine Heilung dazwischenkam. Beides ist das vorhergesagte Verhalten. Eine Verkürzung darüber hinaus wäre nur über die Einstellung `RaisePlayerFirst` zu haben — das ist eine Nutzerentscheidung über den Vorrang zwischen Heilen und Aufheben, kein Defekt.

**Damit schließt sich die Kette dieses Vorgangs:** Ursache statisch belegt (A54), durch das Manual-Experiment des Auftraggebers bestätigt, erster Behebungsversuch im Spiel gescheitert und vollständig zurückgenommen (C37), zweiter Versuch entworfen (A56), im Loop geprüft (A57, A60) und jetzt im Spiel bestätigt. Der Prüfgrad ist erstmals in diesem Vorgang **Laufzeitbeobachtung**, nicht nur statische Prüfung.

**Was der Spieltest nicht abdeckt und in `TODO.md` weitergeführt wird:** Die drei einstellungsabhängigen Eingriffe desselben Zweigs — Phönixfeder samt Zieleignung und Stufenprüfung, Hartwirk-Korrektur bei abgeschaltetem `RaisePlayerBySwift`, Bezugsmenge der Nur-Heiler-Modi. Sie liegen hinter Vorgaben, die der Auftraggeber nicht verändert hat, und sind deshalb mit dem Kernpfad nicht mitgetestet worden.

**Folge für PR #7:** Der Zweig `claude/raise-swiftcast-weave` wurde als Rückfallstand geführt, für den Fall, dass der zweite Versuch ebenfalls scheitert. Dieser Fall ist nicht eingetreten. Ob der Pull Request geschlossen wird, entscheidet der Auftraggeber.

---

### A74 · Die Buchführung über andere Beschwörer bringt nichts, und das Schadensoptimum ist erreicht (11.09.2026)

**Zwei Einwände des Auftraggebers, beide durchschlagend.**

**Erstens:** „wieso halten sich andere bei searing light für eine lücke zurück? sie haben doch eigene abklingzeiten, so dass sie gar nicht vor ihrem eigenen neuen fenster zünden können." Der Einwand trifft die Konstruktion von V5 und V6 im Kern. Die eigene Wiederholzeit wird in der Regel zuerst geprüft; wer nicht bereit ist, kommt gar nicht bis zur Frage nach den anderen. Für den, der bereit ist, gibt es nichts, worauf zurückzustehen wäre: Kann ein anderer zünden, tut er es — und gegen die Verschwendung steht bereits die vorhandene Sperre. Kann er nicht, muss man selbst.

**Gemessen bestätigt.** V7 — dieselbe Erlaubnis ohne jede Buchführung, nur „Buff vollständig abgelaufen" und „eigene Wiederholzeit frei" — liefert im gesamten maßgeblichen Bereich und bei jeder Versatzstufe **exakt dieselben Werte** wie V5. Auch gegen den dreiminütigen Ausfall, für den V6 gebaut war: 11, 22, 44, 66 Prozent — identisch. Wer nichts aufschreibt, hat auch nichts zu vergessen.

**Außerhalb des maßgeblichen Bereichs ist die Buchführung sogar schädlich:** Bei sieben und acht Beschwörern erreicht V7 100 %, V5 dagegen 46 %. Das Zurückstehen führt dort in eine ungünstige Selbstorganisation. V5 und V6 sind damit verworfen, und die Invariante „Buchführung bringt nichts" steht als Prüfung im Selbsttest des Modells, damit sie nicht unbemerkt aufhört zu gelten.

**Was entfällt:** Zustand über Frames, Beobachtung fremder Statusquellen, Rücksetzpunkte bei Kampf-, Gruppen- und Zonenwechsel, Verfallsdatum — und der Reichweiteneinwand, weil nichts mehr beobachtet wird, das ausbleiben könnte. Die zweistufige Empfehlung wird einstufig: V1 und V7, beide zustandsfrei, beide unter der Gruppenprüfung.

**Zweitens:** „die frage ist eher, ist das schadensoptimum bei 2-5 beschwörern erreicht." Das trifft die Grenze, die das Modell bis dahin selbst benannt hatte — es zählte Sekunden, nicht Schaden. Der Schaden fällt im Zwei-Minuten-Zyklus nicht gleichmäßig: Raidverstärkungen und Abklingzeiten sind auf das Burst-Fenster gebündelt.

**Das Modell gewichtet jetzt nach Schadensdichte und vergleicht gegen die optimale Platzierung derselben Ladungen** (erst das Burst-Fenster, dann der Rest). Ergebnis bei Burst-Anteilen von 17, 30 und 45 Prozent: **V7 liegt überall innerhalb eines Prozentpunkts am Optimum.** Bei zwei bis fünf Beschwörern ist das Schadensoptimum damit erreicht, nicht bloß angenähert.

**Warum die Bündelung nicht gewinnt:** V7 gibt das Burst-Fenster nicht auf. Der erste Zünder steht in seinem Solar-Fenster, das mit dem Gruppen-Burst zusammenfällt; die übrigen füllen nur die Zeit danach. Und wer den Burst gedeckt hat, ist genau 120 Sekunden später — zum nächsten Burst — wieder bereit. Der heutige Code verschenkt dagegen umso mehr, je stärker gebündelt wird: bei 45 Prozent Burst-Anteil und fünf Beschwörern 45 gegen mögliche 89 Prozent.

**Grenzen der Gegenprobe, eine davon zugunsten von V7:** Der Burst-Anteil ist eine Annahme und aus diesem Repository nicht zu klären; der Schaden außerhalb des Bursts ist als gleichmäßig modelliert, obwohl die Beschwörungsfenster selbst Spitzen sind — da V7 gerade diese abdeckt, wird sein Vorsprung eher unterschätzt. Phasen ohne Ziel sind nicht modelliert.

**Lehre, in zwei Sätzen:** Eine Konstruktion, die Information sammelt, ist erst dann gerechtfertigt, wenn gezeigt ist, dass die Entscheidung ohne sie anders ausfiele. Und ein Modell, das ein Surrogat misst — hier Sekunden statt Schaden —, ist erst dann belastbar, wenn die Gegenprobe mit der gemeinten Größe dieselbe Rangfolge liefert.

**Erreichter Prüfgrad:** Modellrechnung mit Selbsttest gegen sechs Invarianten, darunter „keine Regel schlägt das Optimum" und „Buchführung bringt nichts". Keine Laufzeitbeobachtung.

---

### A75 · Die Burst-Abdeckung getrennt gemessen — keine Verschiebung aus dem Burst (11.09.2026)

**Anlass:** Der Auftraggeber hat den Einwand gegen die Schadensgewichtung geschärft: Ein Buff im Burst steigert einen Anteil des Burst-Schadens, derselbe Buff in der Zwischenphase denselben Anteil eines viel kleineren Schadens. Wandert Buffzeit aus dem Burst heraus, ist das eine Regression — und eine gewichtete **Gesamtzahl** kann sie verdecken, weil der Zugewinn in der Zwischenphase den Verlust im Burst rechnerisch ausgleicht. Seine Folgerung: Sekunden zählen genauso wie Schaden.

**Der Einwand war berechtigt und die Prüfung fehlte.** A74 hatte gezeigt, dass V7 in der gewichteten Gesamtzahl am Optimum liegt — aber nicht, dass diese Zahl nicht aus einer Verschiebung entstanden ist. Das Modell misst die Burst-Abdeckung jetzt getrennt.

**Ergebnis: Es wandert nichts aus dem Burst.** Bei jeder Regel, jeder Beschwörerzahl von eins bis fünf und jeder Versatzstufe bleibt die Abdeckung des Burst-Fensters bei 99 bis 100 Prozent. Der Grund steckt in der Taktung: Wer den Burst gedeckt hat, ist genau 120 Sekunden später wieder bereit — zum nächsten Burst. Diese Ladung bleibt dauerhaft an den Burst gebunden; nur die übrigen füllen die Zwischenzeit. V7 fügt Abdeckung hinzu, ohne bestehende zu verschieben.

**Damit gilt die Folgerung des Auftraggebers.** Die Gleichsetzung von Sekunden und Schaden ist hier erlaubt — nicht allgemein, sondern weil der Burst gedeckt bleibt. Das steht jetzt als Invariante im Selbsttest: Keine Erweiterung darf die Burst-Abdeckung senken.

**Zwei Befunde am Prüfmittel selbst, beide aus diesem Durchgang:**

Die erste Fassung der Burst-Messung lieferte durchgehend 0 Prozent. Ein `continue` stand vor der Zündlogik, sodass niemand zündete. **Die Vergleichsprüfung schlug trotzdem nicht an, weil 0 nicht kleiner ist als 0** — ein Test, der nur zwei Zahlen ins Verhältnis setzt, merkt nicht, dass beide kaputt sind. Der Selbsttest verlangt jetzt zusätzlich, dass ein einzelner Beschwörer seinen eigenen Burst tatsächlich deckt. Dieselbe Fehlerform wie der stille Nullbefund in A68, an einer anderen Stelle.

Die zweite Fassung zeigte einen Rückgang von 0,8 Prozentpunkten — in genau der Richtung des Einwands. Statt die Toleranz aufzuweiten, wurde nachgemessen: Bei zehnfach feinerem Zeitraster schrumpft der Rückgang auf 0,14 Prozentpunkte, skaliert also mit der Rasterweite und ist Diskretisierung. Die Toleranz der Burst-Prüfung ist entsprechend hergeleitet — `STEP / BURST_WINDOW`, weil das Burst-Fenster nur ein Sechstel des Zyklus ausmacht und derselbe Rasterfehler dort relativ sechsmal schwerer wiegt.

**Erreichter Prüfgrad:** Modellrechnung mit Selbsttest gegen jetzt acht Invarianten. Keine Laufzeitbeobachtung.

### A76 · Was eine Beschwörungsphase wert ist, und was in die Restzeit von Searing Light passt (11.09.2026)

**Anlass:** Zwei Fragen des Auftraggebers. Erstens der prozentuale Schadensanteil von Solar Bahamut gegen Bahamut gegen Phoenix und gegen ein gleich langes Fenster aus Ifrit, Titan oder Garuda, ohne Searing Light. Zweitens, ob die stärkste der drei einmaligen Primal-Sonderaktionen vorgezogen werden sollte, solange der Buff noch läuft, und wie viele Attacken in die Restzeit überhaupt passen. A75 hatte genau diese Lücke als Modellgrenze benannt: Der Schaden außerhalb des Bursts war als gleichmäßig angesetzt, obwohl die Beschwörungsfenster selbst Spitzen sind.

**Prüfmittel:** `.github/scripts/audit/smn_phase_potency.py`, neu. Phasenaufbau aus der Dispatch-Reihenfolge in `SMN_Reborn.cs`, Dauern und Potenzen aus `ActionId.resx`, Selbsttest gegen sechs Invarianten.

**Ergebnis Phasenwert:** Solar-Bahamut-Fenster 7300 Potenz, Bahamut 5700, Phoenix 5680, gleich langes Primalfenster 2970 — also 100 / 78 / 78 / 41 Prozent. Bahamut und Phoenix sind gleichwertig, weil Phoenix' stärkerer Füller genau ausgleicht, dass seine Astral-Flow-Aktion heilt statt zu schaden. Dazu parkt RSR 1800 Potenz an Fähigkeiten im Solar-Fenster, die an Searing Light gebunden sind und deshalb nicht in den Vergleich gehören.

**Ergebnis Restzeit:** Nach dem Beschwörungsfenster bleiben fünf Sekunden Buff. Hinein gehen bei Ifrit zuerst zwei Attacken mit 1360 Potenz, bei Titan drei mit 1300, bei Garuda **eine** mit 800 — Slipstream beginnt auf dem zweiten Platz und wird erst nach dem Buffende fertig. Der Unterschied zwischen bester und schlechtester Reihenfolge ist 560 Potenz, bei fünf Prozent Verstärkung 28 Potenz gegen 30 440 Potenz Zyklusleistung.

**Bewertung: kein Eingriff.** Die Reihenfolge ist bereits als Einstellung vorhanden (`SummonOrderType`), die Voreinstellung beginnt mit Titan und ist damit nahezu optimal, und die einzige teure Reihenfolge vermeidet sie ohnehin. Eine Automatik nach Bufflage würde das Bewegungsrisiko von Crimson Cyclone in die Burstphase legen, ohne dass der Gewinn hier nachweisbar wäre.

**Zwei Befunde am Prüfmittel selbst:**

Die erste Fassung der Restzeitrechnung zählte GCD-Plätze statt Zeitpunkte. Damit lag Slipstream mit 1320 Potenz scheinbar vor Titan — die Gießzeit fiel unter den Tisch, und die Rangfolge stand falsch herum im Konzept, bevor die zeitgenaue Fassung sie umgeworfen hat. Dieselbe Fehlerform wie beim Konfliktrisiko über Dateiaktivität: Ein Surrogat misst nicht den Wirkungsbereich.

Die zweite Fassung verlor die gewebte Fähigkeit des führenden Blocks. Mountain Buster steht in der Blockliste hinter vier Topaz-GCDs, die nicht mehr ins Fenster passen, und die Schleife brach vorher ab. Der Selbsttest verlangt jetzt ausdrücklich, dass die gewebte Aktion des führenden Blocks erscheint.

**Erreichter Prüfgrad:** Potenzrechnung mit Selbsttest, statisch gegen die Artefakte. Keine Laufzeitbeobachtung, kein Schadensrechner.

**Belegschwäche, ausdrücklich:** Vierzehn Potenzen und alle Gießzeiten sind nicht am Repository belegt. `ActionId.resx` lässt die Zahl leer, sobald ein Merkmal sie überschreibt — dort steht wörtlich „with a potency of ." Die Werte stammen aus Suchmaschinenzusammenfassungen; Job-Guide, FFXIV-Wiki, Icy Veins und The Balance sind vom Egress dieser Umgebung gesperrt. Ein Kreuztreffer stützt sie: Für Umbral Impulse nennt die Fremdquelle 640, und diesen Wert belegt `ActionId.resx` unabhängig.

---

## B · Commit-Register (Fork vs. `upstream/main`)

Jeder Commit einzeln geprüft: löst er ein reales Kampfproblem, codearm, gibt es Besseres. Ausgenommen: Marker-Bumps, Merge-Commits, Netto-Null-Revert-Paare (5ae845b+37e47d0, 4358fc0+c82ea88, 6ebdb14+27abd85, 6717e5d+4e09493), Doku-Commits.

| Commit | Inhalt | Prüfung / Ergebnis |
|---|---|---|
| 8edd696 | SMN Addle in Defensives, Buster auf Nicht-Tank erkennen | Kette Erkennung→AutoStatus→Dispatch nachvollzogen, alle 5 Rollen abgedeckt. KEIN FEHLER |
| 1ca682a | Status-Provide-Check auf oGCD-Befehlspfad | Mechanismus echt, Richtung falsch → KORRIGIERT (C2), revertiert 5755ad5b |
| c93a8bc | `BMRShouldRefreshBefore` + Addle/Feint | bis `WillStatusEnd` nachvollzogen; 10/15 s ab Lv 98 per Websuche. KEIN FEHLER |
| 75b7af0 | `RadiantOnCooldownSpam` entfernt | in Upstream deklariert, nie gelesen. KEIN FEHLER |
| e87ebea | SMN Titan bei Bewegung (Opt-in) | Topaz instant per Websuche; Default aus. KEIN FEHLER |
| a1418f5 · e1886c7 · ae7ed1a · be7cf22 | Interrupt/AntiKnockback: Job-Override vor Rollen-Default; Redundanz entfernt; `HasOwnInterruptGate`/`HasOwnAntiKnockbackGate` gegen ungegateten Zweitversuch | alle vier Override-Stellen gelesen (RPR/VPR gegatet, BLU Passthrough, Phantom andere Aktion). Kette schließt sich. KEIN FEHLER |
| be083a1 · 0c076ee | Weakness-Faktor und Schild-Credit | Faktor später als falsch erkannt und entfernt (A5); Schild-Credit durch `ShieldCreditAllowed` an Bedrohungsnachweis gebunden. Stand: KEIN FEHLER |
| 27c7b69 | Schild-Magnitude/-Dauer in Heilentscheidung | ursprünglich blinde 3-s-Schwelle (Regression), durch 0c076ee vorgeschaltet. KEIN FEHLER im Stand |
| 1ed9907 | zwei Compile-Fehler | `Player` statt `Player.Object`; NIN-Logik in versiegelte `NinjaRotation.DefenseAreaAbility`. KEIN FEHLER |
| 0f25161 · 2d5e7dc | RPR/VPR/SAM: BMR-Feint über `EnoughWeaveTime` statt Combo-Sperre | Helfer repo-weit etabliert; reaktive Zeile unverändert. KEIN FEHLER |
| 6fc9ebb · 1f5dbb1 · 099e051 · e38cfe2 · 1092b59 · 700e870 | `base.X`-Fehlaufrufe PCT ×2, AST, Hardboiled, BeirutaPCT, BRD/WHM-PvP | Methodenzugehörigkeit je Zeile geprüft, repo-weit kein weiterer Treffer; Klasse per CI geschlossen. KEIN FEHLER |
| 15297b2 · 3f72a6d | BRD/MCH BMR-Troubadour/Tactician; `Tactician_2177` in `MitOverlap`-Guard | Sync-Status-ID war in bestehender Zeile gefehlt; repo-weit sonst keine. KEIN FEHLER |
| 951d0ec · 87646bf | Gegnerzahl-Sustain, Schwelle 3 → 4 | 10 Dateien gezählt, `>= 4` durchgängig. KEIN FEHLER (Doppelanwendung erst A6-1) |
| 6813a7c · e9b687c | DRG/NIN/SAM/DNC Second Wind/Bloodbath; DRG Stardiver-Guard | DRG war einzige Methode ohne den dateiweiten Guard. SAM/DNC kein Muster. KEIN FEHLER |
| c01a5e2 · 4a01682 · 76a683b · b896c6d · 470de85 | Tanks: Reprisal-Sustain, Vengeance/Rampart-Familie BMR, DRK Single-Block, `!InTwoMIsBurst` entfernt, unerreichbares Wall/Vigil-Paar entfernt | Wirkdauern per Websuche (Shadow Wall 15 s seit 5.1, Rampart 20 s); Cross-Tank-Konsistenz geprüft. KEIN FEHLER |
| b1b187c · 0b3afc7 · eab865c | Notfall-Potion bei Buster (reaktiv → proaktiv) | `CanUseEmergency` behält Fehl-HP-Wächter; Parameter-Kette geschlossen (später in bfc52584 vereinfacht). KEIN FEHLER |
| 28361f2 · 16d4475 | `bmrTankbusterImminent` vereinheitlicht, DPS-Zweig nur ohne lebenden Tank | alle 5 Rollen gegen Enum. KEIN FEHLER |
| 14a15df · 3e3b7f7 · 0af7957 · 7626f9f | `UseBmrTimeline` in Helfer und `BMR*Within`; ChurinDNC `BMRActive`; Doku der Fenstergrenzen | zentral statt verstreut; 7 Aufrufstellen in MCH gezählt. KEIN FEHLER |
| 0a31836 | RDM `!Impact.EnoughLevel && Impact.EnoughLevel` | Kontradiktion behoben. KEIN FEHLER |
| 0885f53 | Status-Provide-Check GCD-Befehlspfad | wie 1ca682a → KORRIGIERT (C2), revertiert 5b778336 |
| 030129c · eab5506 · 7c174ec · 2b6e1d8 | RPR/VPR Slot-Guards für Feint | RPR-Prämisse „Burstfenster" war ungenau (Ressourcen-Zyklus), Kommentar korrigiert; VPR `IsBurst` spiegelt `AttackAbility`. KEIN FEHLER |
| 73048dd · e221ce5 · c1523ac | MCH Slot-Konflikt-Gate | `CooldownCheck` gegen `ActionCooldownInfo.cs:240` geprüft: totes Disjunkt korrekt entfernt. KEIN FEHLER |
| c866879 | `MoveBackAbility` doppelt aufgerufen | Reihenfolge wie `MoveForward`. KEIN FEHLER |
| 0f24ed3 | PhantomDefault `out _` statt `out act` | `CanUse` setzt `act = this` immer. KEIN FEHLER |
| 53c8018 · 9e4a2fc | BLM `HighThunder` ST/AoE | alle drei Stellen. KEIN FEHLER |
| cde050f · f154d57 | PCT Burst-Gate für Grassa; `HasHostileCountAoeMitigation` job-gescoped | 11 Overrides gezählt, PLD/WAR-Ausschluss bestätigt. KEIN FEHLER (NIN fehlte → A6-6) |
| 6c0e8dc | Ground-AoE-Tiebreak über `FindTargetByType` | kann Erfolg nicht in Fehlschlag wandeln (`filteredHasAny`). KEIN FEHLER |
| 716789d | WHM DoT nicht auf aggro'tes Ziel | später auf Upstream zurückgesetzt (A5: prüfte `Target` vor `CanUse`) |
| 0fd058d · 89665b7 · fd19aad · 60d5773 · 04d364d · b1f2c61 | Pre-Pull/Sustain (A2) | s. A2 |
| 00a426b · 6b40600 · 0fe7bed · f9e0eff | #57–#62 (A4) | s. A4 |
| 5755ad5b | vier Fork-Fehler zurückgebaut | s. A5 |
| A6: 5bb4d39f · 2df7dc4e · bd65f0d4 · 5b778336 · 451d9e90 · 28c0e1fc · 990daaeb · 3b5e50d5 · bfc52584 · c1d0ba45 · ff0d8d43 · f107eda9 | Review-Loop | s. A6 |
| A7: c6a0a40c · a2a3ec35 · 52a0817d · 00bc9c6f · 4b3c9412 · f90c7bf7 · d045e47f · 4889395f · 157a9ad3 · e6428c19 · e07ceb4b · 672e92ee | TODO-Abarbeitung, A4a | s. A7 |
| A8: ebaa44c7 · 3c40d9e4 · 93f05e68 · 232d472e · e224e3f7 | Codebasis-Audit Phase 1 | s. A8 |
| A9: b8018cf0 · 6704335d · 1c259f10 · 9f815bf3 · d9a99de7 · 6588832b | Mitigations-Trigger, Version | s. A9 |
| A10: ad00090e · efc4d039 · d0523a8d · 331c1254 · f2384007 · 6189c4cb · 5de07717 | Codebasis-Audit Phasen 2–4 | s. A10 |
| A11: 8dc2bd65 · 06c60e97 · 33f8cdff · 364433e6 | Entscheidungsvorlage E1–E4 | s. A11 |

---

## C · Widerrufene Aussagen dieses Archivs

| # | Frühere Aussage | Widerlegung | Stand |
|---|---|---|---|
| C1 | B2a: `Vector3.Distance > 5` in `CanProvoke` „blockiert den häufigsten Fall", Fix `<` | `CanProvoke` misst Center-zu-Center inkl. Y, der Rest der Datei Kante-zu-Kante (`DistanceToPlayer`); bei `HitboxRadius >= 5` ist die Center-Distanz immer ≥ 5,5, `>` war also nie ein Blocker — `<` blockierte Boss-Nahkampf und Fern-Provoke. Lehre: Distanzschwelle nie ohne Messfunktion bewerten | auf Upstream zurückgesetzt |
| C2 | 1ca682a/0885f53 „Kein Fehler" | `skipStatusProvideCheck: true` ist Upstreams Ausnahme für befohlene Aktionen; ohne Flag feuerte ein befohlenes oGCD/GCD nicht | 5755ad5b (oGCD), 5b778336 (GCD) |
| C3 | Nachtrag 5: Pre-Pull-HoT fehlt wegen Zeitfenster, Radius 21 → 26 y | Spiel: wirkungslos; Ursache war die Mob-Schwelle (Nachtrag 6/7) | Radius zurück auf 20 y |
| C4 | #70 „Release-Tag steht noch aus" (Bericht, TODO, 06-fork-audit) | `git ls-remote --tags origin`: Tag seit 08:38 UTC vorhanden; Stand war aus dem Gesprächsverlauf zitiert | korrigiert, Regel in CLAUDE.md |
| C5 | #72 als Optionsliste „(a) belassen / (b) Fallback" | Antwort ist je Job anders; zu Ende gedacht in A7 | umgesetzt |
| C6 | #54 „Bestätigung im Spiel offen" als Nutzeraufgabe | Kette im Code prüfbar und geprüft (A7) | geschlossen |
| C7 | Nachtrag 6: Mob-Schwelle komplett entfernen | Nutzer: 4+ war Ausstiegskriterium, kein Eintrittskriterium | Nachtrag 7 |
| C8 | #47-Zwischenkorrektur: BRD/MCH ausgeschlossen, „Troubadour/Tactician nur gegen Magie" | `PredictedDamageType` ist Trefferform, nicht Schadensart; beide mindern jeglichen Schaden (Websuche) | beide einbezogen |
| C9 | cde050f/f154d57 „`HasHostileCountAoeMitigation` job-gescoped · KEIN FEHLER" | Geprüft wurde nur, ob das Flag die richtigen Jobs trifft — nicht, was das gesetzte Flag auslöst. `AutoStatus.DefenseArea` öffnet die ganze Defensivkette und bei Melee/Ranged zusätzlich `DefenseSingleAbility`, nicht die eine Sustain-Zeile. Vom Nutzer im Spiel als Dauer-Casten von Radiant Aegis und Addle gemeldet. Lehre: Ein Trigger ist an dem zu messen, was er auslöst, nicht daran, wen er trifft | Fallback und Flag entfernt (A9, b8018cf0) |
| C12 | „Selbstlernende AoE-Liste … unumkehrbar: Zurücknehmen lässt sich ein Eintrag nur durch Editieren der Datei" (TODO, mehrfach fortgeschrieben; stützte die Bewertung in A9) | Zweifach widerlegt: `RotationConfigWindow.cs:3745` bietet den Schalter „Reset and Update AOE List" (`ResetHostileCastingArea`, lädt die gepflegte Liste über `InitOne(..., forceDownload: true)` neu), und `DrawActionsList` erlaubt das Entfernen einzelner Einträge über Kontextmenü und Entf-Taste. Die Codedokumentation in `OtherConfiguration.cs:31` empfiehlt den Reset ausdrücklich nach jedem Patch. Lehre: Die Behauptung, eine Bedienmöglichkeit fehle, ist eine Aussage über die Oberfläche und dort zu prüfen, nicht aus dem Datenmodell zu schließen | Eintrag von Defekt zu technischer Schuld herabgestuft (A15) |
| C11 | Erste Entscheidungsvorlage: VPR-Blöcke „löschen", UI-Wrapper „behalten", Toggle-Konflikt per `applyToggle` lösen, Timeline-Vorzeichen im Spiel ablesen lassen | Vier Fehler in einer Vorlage: das Konfliktrisiko war über die Dateiaktivität geschätzt statt regionsgenau gemessen (0 statt 7 für VPR, 2 statt 12 für die UI-Region); die VPR-Blöcke wurden nicht gegen die Gegenhypothese „unverdrahtet statt tot" geprüft; `applyToggle` hätte `DTRManualAuto`-Nutzern den einzigen Ausschaltweg genommen, was erst die Auswertung als Zustandsfolge zeigte; und die Vorzeichenfrage stand im Quelltext von BossModReborn, war also keine Prüfaufgabe für den Auftraggeber. Lehre: eine Vorlage ohne durchgerechnete Konsequenzen ist keine Vorlage, und verfügbare Quellen sind vor der Vorlage auszuschöpfen | A11: alle vier revidiert |
| C13 | Konzept 09, erste Fassung: Gunbreaker Catharsis of Corundum sei ein Rückhaltefall (Fallvarianten 10/10b), Kandidat-Bezeichner `ClarityOfCorundum` | `Status.resx` 2685: der Heilstoß löst auch „upon expiration of effect duration" aus, ist also nicht raubbar; und der Bezeichner war ohnehin falsch — 2684 ist reine Schadensreduktion. Lehre: Ein Auslöser gilt erst als raubbar, wenn seine Auslösebedingung vollständig gelesen ist, nicht nur ihr erster Halbsatz | Fall gestrichen (A21) |
| C14 | Konzept 09, erste Fassung: „Der Schluss hängt davon nicht ab — mehr Gesamtpuffer heißt in **jedem** Aufteilungsmodell geringere Verbrauchswahrscheinlichkeit" | Die Formel „in jedem Modell" ersetzte die fehlende Prüfung, statt sie zu kennzeichnen. Verdrängen statt Addieren ist ein Modell, in dem der Schluss kippt; und mit einem einzigen `ShieldPercentage`-Wert je Charakter ist die Aufteilung ohnehin nicht beobachtbar. Lehre: Eine Allquantifizierung über unbekannte Modelle ist keine Absicherung gegen eine Wissenslücke, sondern deren Glättung | Schluss zurückgenommen (A21) |
| C15 | TODO und Konzept 09: „Keine der vier `HallowedGround`-Ids und drei von vier Holmgang-Ids fehlen in der Liste" | Nullbefund über den Bezeichner ohne Prüfung der Wirkbeschreibung: Holmgang 88 und 1305 sind der Bewegungs-Debuff auf dem Ziel, 1304 die PvP-Selbstform; die vorhandene `Holmgang_409` ist die richtige. Dieselbe Prüfung fand dafür die tatsächlich fehlende `UndeadRebirth`. Lehre: dieselbe Regel wie bei den deutschen Aktionsnamen — ein Name ist kein Beleg, die Beschreibung ist es | Forderung eingegrenzt und um `UndeadRebirth` ergänzt (A21) |
| C16 | Konzept 09, alle Fassungen bis einschließlich der dritten: Living Dead als **Zustandsfrage** behandelt („liegt der Status → zurückhalten", „Phase 2 → ja, zwingend") | Die Fähigkeit nützt nur, wenn der Tod *vor* Ablauf eintritt, und Phase 2 nur, wenn die Heilung *bis* zum Ablauf reicht — beides Fragen nach Rate und Restzeit. Die Zustandsfassung kannte weder den Späteingriff in der Ablaufsekunde noch die Staffelung zwischen billiger Frühunterstützung und teurem Späteingriff. Lehre: Wo eine Fähigkeit eine Frist setzt, ist die Frist der Gegenstand der Regel, nicht das Vorhandensein des Status | Fälle 1c und 4/4a ergänzt, `TriggerStillReachable` eingeführt (A22) |
| C17 | Konzept 09, drittes Audit: `TankSurvivesWithoutMe` sichere die Rückhaltung ab („erst prüfen, ob der Tank ohne mich überlebt") | Die Zusage ist aus den vorhandenen Größen nicht herstellbar: `BMRNextTankbusterIn` nennt weder Ziel noch Trefferzahl noch Gesamtschaden, und manche Buster töten auch vollgeheilt und geschildet. Eine Gesundheitsschwelle beantwortet keine dieser drei Fragen. Lehre: Eine Vorbedingung, die mehr verspricht, als ihre Datenlage hergibt, ist gefährlicher als keine — sie erzeugt Vertrauen, wo Unsicherheit herrscht | Buster-Fenster als eigene, HP-unabhängige Aufhebung vorgezogen (A22) |
| C18 | Konzept 09, drittes und viertes Audit: die Aufhebungen der Rückhaltung gelten für **alle** Auslöser — niedrige Gesundheit (drittes) und jedes Tankbuster-Fenster (viertes) | Für Living Dead ist beides kontraproduktiv: Dort **ist der Tod der Auslöser**. Niedrige Gesundheit ist der erwünschte Zustand, und der Buster liefert genau das Ereignis, auf das die Fähigkeit wartet. Eine Heilung, die den Tod verhindert, verhindert per Konstruktion den Auslöser — eine Gegenhypothese ist nicht formulierbar. Ursache war die fehlende Unterscheidung, ob der Auslöser der Tod selbst ist oder unterhalb davon liegt. Lehre: Eine Aufhebungsregel gilt nur für die Auslöserklasse, für die sie hergeleitet wurde; ihre Verallgemeinerung ist zu begründen, nicht zu unterstellen | Klasse A in A-tödlich und A-nichttödlich getrennt, `MayWithholdForTrigger` in zwei Bedingungen zerlegt (A23) |
| C19 | Konzept 09, viertes Audit: der Messbaustein sei Voraussetzung der Living-Dead-Regelung, und ohne Messung dürfe nie zurückgehalten werden | Die Kernanforderung „im letzten Augenblick retten" ist eine **Uhrregel** über die Reststatuszeit und braucht keine Rate; der Messbaustein verkleinert nur den Restfehler. Und der Rückfall war falsch herum: Bei Living Dead heißt „nicht zurückhalten" heilen, was die Fähigkeit entwertet. Lehre: Bevor eine Anforderung eine neue Messung verlangt, ist zu prüfen, ob sie eine Frage nach der Zeit statt nach einer Rate ist | Messbaustein ans Ende der Reihenfolge, Uhrregel als Stufe 1 (A23) |
| C20 | A24 zur AoE-Liste: „Eine Formel braucht nicht, wer das Ergebnis hat" — die Potenz sei hier ein Umweg, weil der Wert bereits beobachtet vorliege | Zu absolut. Der Rechenweg lässt sich **umkehren**: Eine Beobachtung kann um bekannte Störfaktoren bereinigt werden, und `GetCurrentMitigationPercent()` (`CustomRotation_OtherInfo.cs:585`) liefert den Divisor bereits — im Effekt-Handler aufgerufen also zum Trefferzeitpunkt. Damit entfällt der Zirkelschluss weitgehend, den A24 noch der Höchstwertregel überließ. Die Rückrechnung endet erst vor der Potenz selbst, weil Angriffs- und Verteidigungswerte außerhalb des Clients liegen — dorthin muss sie aber auch nicht. Lehre: Bevor eine beobachtete Größe als endgültig behandelt wird, ist zu prüfen, ob ihre Störfaktoren bekannt und herausrechenbar sind | Rückrechnung in den Vorschlag aufgenommen, Höchstwertregel zur Absicherung herabgestuft (A26) |
| C21 | A29 Punkt 1: `HpRecoveryDown` in der Schwellensenkung sei eine „gewollte Lockerung" — vorher hart ausgeschlossen, jetzt ab 15 % geheilt | Der Status mindert Heilung („Healing effects received are reduced"), er hebt sie nicht auf. Wer weniger Heilung *empfängt*, braucht **mehr** davon, nicht später. Die eigene Fallmatrix hatte den Fall aufgeführt und als Gewinn verbucht, statt zu fragen, was der Status bedeutet. Lehre: Eine Fallmatrix belegt, dass eine Regel greift — nicht, dass sie richtig greift | Aus `NoNeedHealingStatus` entfernt (A29) |
| C22 | A29 Punkt 3: kürzerer Vorlauf der Uhrregel (ein GCD statt zwei) senke den Restfehler, die Heilung lande ohnehin noch im Schutzfenster | Der Vorlauf misst bis zur **Entscheidung**. Bis die Heilung landet, müssen der laufende GCD auslaufen und eine Wirkzeit vergehen — zusammen zwei GCDs. Bei einem GCD landet sie nach Ablauf, wenn der Tank ungeschützt ist. Lehre: Bei einer Zeitregel ist zu prüfen, welches Ereignis sie datiert — Entscheidung, Beginn oder Wirkung sind drei verschiedene Zeitpunkte | Vorlauf zurückgenommen (A29, `cea66c79`) |
| C10 | A9: „`IsHostileCastingTank`-Fallback · KEIN FEHLER, nicht angetastet" | Die Begründung galt der Tankbuster-Erkennung für Tanks (`IsHostileCastingToTank`) und wurde ungeprüft auf `…TankBusterAtMe` übertragen, obwohl das eine andere Frage beantwortet: nicht „kommt ein Tankbuster", sondern „kommt einer auf mich". Für Nicht-Tanks ist der Fallback dort schlicht falsch. Nutzer: „wenn mich ein Mob aus einer großen Mobgruppe angreift, wird bereits Schimmerschild und Stumpfsinn gecastet" | `…TankBusterAtMe` auf gesicherte Tankbuster eingeschränkt (A9, d9a99de7) |
| C23 | A31 und der daraus abgeleitete TODO-Eintrag: die beiden RDM-PvP-Zeilen seien durch die Nachbarzeile belegt und „im nächsten Einzelzyklus" auf 3235/3236 zu setzen | Die Feldwahl ist belegbar falsch, die Behebung war es nicht. Dieselbe Erhebung fand zwei Blaumagier-Stellen, an denen die analoge Verschiebung die Zauber genau dann gesperrt hätte, wenn sie am meisten wert sind (Magic Hammer: 250 Potenz plus 1000 MP; Peripheral Synthesis: 220 → 400 Potenz **mit** liegendem Debuff). Damit ist auch für RDM offen, ob die zwei wirkungslosen Zeilen der Defekt sind oder die eine wirksame. Lehre: Aus „das Feld ist nachweislich falsch" folgt nicht „die Umbuchung auf das andere Feld ist richtig" — der Zweck der Aktion entscheidet, nicht die Feldsemantik | A32: Blaumagier-Zeilen entfernt, RDM/SCH/Bozja als offen erfasst |
| C24 | Konzept 09, Klasse-A-Tabelle: fremde Heilung wirke bei The Blackest Night „indirekt: hebt die HP, sodass weniger Schaden gegen die Barriere läuft" | Mechanisch falsch. Eine Barriere absorbiert eingehenden Schaden vor den HP; ihr Verbrauch hängt am Schaden, nicht am Gesundheitsstand. Die einzige Kopplung läuft umgekehrt — Heilung verhindert den Tod, der den Auslöser vereiteln würde, und wirkt damit **für** ihn. Lehre: Bevor eine Wechselwirkung als Kostenposten geführt wird, ist ihre Richtung an der Mechanik zu prüfen, nicht aus der Nähe zweier Größen zu schließen | A35: Zeile ersetzt, Fall 5 von Rückhaltung auf Werkzeugwahl umgestellt |
| C25 | Konzept 09: The Blackest Night sei nicht behandelbar, weil „der Auslöser nicht beobachtbar" ist | Richtig, aber nicht die Frage, die die Regel braucht. Für eine Werkzeugwahl genügt „liegt TBN?", und das steht in der Statusliste. Dieselbe Fehlerform wie C19: eine schwerere Frage gestellt als nötig und den Fall daran scheitern lassen — dort eine Rate statt einer Uhr, hier der Verbrauch statt des Vorhandenseins. Lehre: Vor dem Verwerfen wegen fehlender Messbarkeit ist zu prüfen, welche Größe die Regel wirklich braucht | A35: Fall wieder aufgenommen, Umsetzungsweg mit zwei Vorbedingungen beschrieben |
| C26 | A35: die Schild-Zurückstellung bei The Blackest Night sei zu empfehlen, weil „der Vorteil in beiden Zweigen der offenen Frage" bestehe | Die nachgeholte Recherche schließt die Frage: TBN hat Verbrauchsrang 3, Eukrasian Diagnosis 4, Divine Benison 12 — TBN wird vor jedem Heilerschild aufgezehrt, Dark Arts bleibt unberührt. Und der zurückgestellte Schild verfällt nicht, er absorbiert nach TBN; Zurückstellen spart also nichts. Der dritte, übersehene Zweig war „der Schild kostet gar nichts". Lehre: Eine offene Frage ist kein Anlass, das Ergebnis gegen sie zu immunisieren, sondern sie zu schließen — und der als gesperrt gemeldete Egress war kein Grund, die Recherche für erschöpft zu halten | A36: Regel zurückgenommen, Fall 5 auf „kein Sonderfall" gesetzt |
| C27 | A36: Radiant Aegis sei für die TBN-Frage gegenstandslos, weil sie „ein Selbstschild des Beschwörers ist, der nie auf dem Dunkelritter liegt" | Falsch herum gedacht. Nicht Radiant Aegis wandert, sondern **The Blackest Night**: `ActionId.resx` (7393) beschreibt es als „Creates a barrier around self or **target party member**", und `DRK_Reborn.cs:128` legt es mit `targetOverride: TargetType.LowHP` auf Fremdziele. Ein Beschwörer kann also seinen eigenen Radiant Aegis tragen **und** zusätzlich TBN — der vom Job-Guide beschriebene Fall ist real, nur vom Heiler nicht beeinflussbar. Der Auftraggeber hat die ungeprüfte Prämisse benannt. Lehre: Bevor ein Fall über den Träger eines Status ausgeschlossen wird, ist die Zielmenge der Aktion an ihrer Beschreibung zu belegen, nicht aus dem Jobnamen zu schließen | A36 ergänzt; zwei Defekte daraus in TODO.md |
| C28 | TODO-Eintrag zu den fehlenden Barrieregruppen: sechs Ids seien die „PvE-Spielerbarrieren, die vermutlich hineingehören" (`Aquaveil_3086`, `DivineCaress`, `Epicycle`, `GuardiansWill`, `HolySheltron_3026`, `ImprovisedFinish`) | Vier der sechs sind es nicht, und acht echte fehlten. Die Prüfung über die **Aktion** gleichen Namens zeigt: Aquaveil und Holy Sheltron senken in PvE nur den erlittenen Schaden, die Barriere-Ids 3086 und 3026 gehören zu ihren PvP-Formen; `Epicycle` hat nur eine PvP-Aktion, `GuardiansWill` gar keine. Nicht genannt waren dagegen `ShakeItOff` (1457/1993), `SeraphicVeil` (1917/2040/3097), `NeutralSect` (1921/3988), `TheSpire_3892` — Barrieren, die ein Heiler in fast jedem Gruppenkampf sieht. Ursache: Die Liste war nach dem Jobkürzel im Status-Scope gebildet, einem Surrogat, das PvE und PvP nicht trennt — beide Formen tragen denselben Anzeigenamen und dieselbe Wirkbeschreibung. Lehre: Wo zwei Formen einer Fähigkeit denselben Text tragen, entscheidet nicht der Status, sondern die Aktion, die ihn verleiht | `scan13.py` um die Aktionszuordnung erweitert, Eintrag neu gefasst (A41) |
| C29 | A44 und die Antwort dazu: der Vorschlag, The Blackest Night nicht zusammen mit anderen Schilden und Minderungen zu wirken, sei „ein Surrogat, das die falsche Größe misst" — pauschal abgelehnt | Für den Tankbuster richtig, für den Wall-to-Wall-Pull falsch. Dort kommt der Schaden als **Strom**, nicht als Paket: Zwei Minderungen gleichzeitig decken dieselben Sekunden doppelt und lassen den Rest ungedeckt, und die parallele Minderung senkt den Strom unter die Rate, die die Barriere in sieben Sekunden aufzehrt (3,6 % → 5,1 % unter Shadow Wall). Die Prüfung war gegen die Buster-Lage geführt und ihr Ergebnis ungeprüft auf die Dauerschaden-Lage übertragen — dieselbe Fehlerform, die C18 für die Aufhebungsregeln festgehalten hat, diesmal in der Gegenrichtung. Der Auftraggeber hat die fehlende Lage benannt. Lehre: Bevor ein Vorschlag verworfen wird, ist zu prüfen, für welche Auslöserklasse er gilt — eine Regel kann für die eine richtig und für die andere falsch sein, und dann ist die Antwort eine Fallunterscheidung, keine Ablehnung | A45: `TankbusterOrHeavyPull` mit Staffelungsbedingung, Konzept 10 neu gefasst |
| C30 | A45-Nachtrag: die deutschen Namen „Reflexion" und „Abtausch" ließen sich keiner Aktion zuordnen, „keine trägt einen dieser Namen erkennbar" | Der Suchraum war falsch gewählt. Gesucht wurde ausschließlich unter Aktionen mit **Betäubungswirkung**, weil beide Namen im selben Satz wie die Stuns des Weißmagiers standen. Reflexion ist Reprisal, Abtausch ist Shirk — Tank-Rollenaktionen ohne Betäubung, die in diesem Filter gar nicht auftauchen konnten. Der Auftraggeber hat den richtigen Suchraum genannt („die deutschen Beschreibungen der Tankskills"), danach war die Zuordnung in einer Websuche belegt. Lehre: Der Suchraum folgt der Rollen- und Kategoriezuordnung des gesuchten Gegenstands, nicht dem Satz, in dem er erwähnt wurde — ein Nullbefund im falschen Raum ist kein Nullbefund | A46: Reihenfolgebedingung umgesetzt, Namensfrage im TODO geschlossen |
| C31 | A47: Die Betäubungssperre prüfte `AnyHostileStunned` — „irgendein Gegner im Umkreis ist betäubt"; der erste Korrekturversuch setzte dagegen „**alle** Gegner betäubt" | Beide Quantoren messen die falsche Größe, und der Auftraggeber hat den richtigen Begriff genannt: Es geht um **Gruppenbetäubungen**. „Irgendeiner" zählt Tiefschlag mit — die Einzelbetäubung, die der Dunkelritter selbst trägt und die RSR über den Unterbrechungspfad wirkt (`CustomRotation_Ability.cs:575`) —, sodass die eigene Unterbrechung die eigene Barriere gesperrt hätte. „Alle" fällt um, sobald ein einzelner Nachzügler unbetäubt zur Gruppe stößt, obwohl der Schadensstrom erkennbar steht. Maßgeblich ist der **Anteil**: eine Flächenbetäubung erfasst das Rudel, eine Einzelbetäubung einen daraus. Zweiter Fehler derselben Oberflächlichkeit: Die Betäubungsprüfung lief über acht Yalm, die Gegnerzahl-Prüfung über die Jobreichweite — zwei verschiedene Mengen für zwei Bedingungen, die dieselbe Frage beantworten sollen. Lehre: Bei einer Bedingung über eine Menge sind Quantor **und** Bezugsmenge Teil des Befunds; beide folgen aus der Wirkung, nicht aus der bequemeren Prüfung | A48: `GroupStunRunning()` mit Anteilsregel über die Jobreichweite, `SurveyStuns`-Überladung mit Trefferzahl |
| C32 | A50, erste Fassung: der Weißmagier solle Sanctus **nicht** zurückhalten, während die Barriere des Dunkelritters läuft — begründet mit einer wechselseitigen Sperre und mit dem Vorrang des Gruppenschutzes | Beide Gründe tragen nicht. Die Sperre wurde behauptet, nicht als Zustandsfolge ausgeschrieben: Jede der beiden Regeln wartet nur, während der Zustand der anderen aktiv ist, und beide Zustände laufen von selbst ab (Betäubung vier Sekunden, Barriere sieben) — ein Zustand, in dem beide aufeinander warten, ist nicht erreichbar. Und der Gruppenschutz ist im Wall-to-Wall keiner: Die Aggro liegt beim Tank, der Schaden also auch, und derselbe Tank trägt die Barriere — die Betäubung verhindert genau den Schaden, den die Barriere aufgefangen hätte. Statt doppeltem Schutz entsteht doppelte Verschwendung: Barriere und Dark Arts verfallen, das Betäubungsbudget ist verbraucht. Der Auftraggeber hat widersprochen („Wenn tdn an ist und der Buff beim Dunkelritter läuft, schadet Holy mehr als es hilft"). Lehre: Die Warnung vor einer wechselseitigen Sperre ist selbst eine Aussage über einen Zustandsautomaten und verlangt dieselbe Auswertung wie jede andere — dieselbe Vorgabe, an der schon `applyToggle` gescheitert ist | A50: `ShouldHoldHolyForBarrier()` hinter `HoldHolyForBlackestNight`, begrenzt auf laufenden Betäubungsspielraum und die Restdauer der Barriere |
| C33 | C30 und A51 zur Rollenaktion Arm's Length (deutsch **Rückstoß**, Stufe 32): erst Shirk zugeordnet, dann in A51 als „nicht entscheidbar" geführt, weil die Namensquellen vom Egress gesperrt sind — und die Frage dem Auftraggeber erneut vorgelegt | Dreifach falsch, und jede Stufe war vermeidbar. Die Zuordnung zu Shirk war schon in C30 widerlegt; A51 beschrieb die Wirkung selbst richtig (Verlangsamung, gehört zu Arm's Length) und schloss daraus nicht auf die Aktion; und die daraus folgende Rückfrage stellte etwas erneut, das der Auftraggeber bereits beantwortet hatte. Anschließend wurde für dieselbe Aktion der deutsche Name „Armlänge" **erfunden** — eine Ad-hoc-Übersetzung, also genau das, was die Namensregel seit dem Ex-Machina-Fall untersagt. Gesperrte Quellen sind kein Grund, eine Angabe des Auftraggebers als offen zu führen, und erst recht keiner, einen Namen selbst zu bilden. Lehre: ein deutscher Name wird nie gebildet, nur belegt oder von ihm übernommen; liegt keiner vor, steht der englische Bezeichner (in CLAUDE.md aufgenommen) | A53: `armsLengthDone` als zweite Stufe der Reihenfolgebedingung, Arm's Length wird im Pull gewirkt, „Armlänge" repo-weit ersetzt |
| C34 | A51 und A52: die Fork-Versionsnummer sei durch Nachziehen auf 7.5.6.1 und ein Prüfskript hinreichend behandelt | Nicht hinreichend. Der Auftraggeber compiliert aus den Fork-Quellen und will der Anzeige entnehmen, welchen Upstream-Stand der Build enthält; eine handgepflegte Zahl beantwortet das nur bis zum nächsten Sync, den jemand vergisst — und genau das war eingetreten. Ein Prüfskript, das niemand aufruft, ist kein Ersatz für eine Ableitung. Die Zahl steht jetzt aus `git describe` und stimmt bei jedem Build von selbst. Lehre: Wo eine Zahl eine Tatsache im Repository spiegelt, ist sie abzuleiten, nicht zu pflegen; die Behebung des Einzelfalls ist erst dann vollständig, wenn die Wiederholbarkeit adressiert ist | A53: Target `DeriveUpstreamVersion` in `Directory.Build.props` |
| C35 | A54, vier aufeinanderfolgende Fassungen zur Ursache der ausbleibenden Wiederbelebung: (a) die Zufallsverzögerung `RaiseDelay2`, (b) `IsTargetMoving` auf der Leiche in `GetDeath`, (c) die Anvisierbarkeitsprüfung, (d) die Wirkzeitsperre bei eigener Bewegung in `NeedsCasting` | Alle vier falsch, alle vier vom Auftraggeber mit je einem Satz widerlegt: die Werte stehen auf 0; die Toten liegen ruhig; sie sind anvisierbar; es tritt im Stehen wie im Laufen auf. Die vierte war die schlechteste: Sie wurde aufgestellt, **nachdem** er „ich stehe mal, und mal bewege ich mich" gesagt hatte, also aus einer Aussage, die die Bewegung gerade als Variable ausschloss — und nach seinem „falsch." ein zweites Mal vertreten. Gemeinsame Ursache aller vier: Es wurde die Kette von der Datenquelle zur Aktion vorwärts gelesen und beim ersten plausibel aussehenden Filter angehalten, statt zu fragen, welche Beobachtung die Hypothese widerlegen würde. Entschieden hat der Fall erst eine Beobachtung, die vorher nicht erfragt worden war — „im Vorschaufenster erscheint Spontanität und wird nie gewirkt" —, und die beweist, dass die Auswahl gelang und der Defekt dahinter lag. Lehre: Liegt ein Laufzeitbeobachter vor, ist zuerst zu erheben, **was er sieht**, statt Hypothesen zu bilden, die er längst ausgeschlossen hat; und eine Nutzeraussage, die eine Variable ausschließt, ist ein Beleg und keine Bestätigung der eigenen Vermutung | A54: Ursache in der Kollision von Auswahl- und Ausführungsfenster belegt, Behebung im Einschiebepfad, `scan17.py` als Schranke |
| C36 | A54 und PR #7: `GetPriorityDeathTarget` prüfe `deathTanks.Count > 1`, wo `> 0` gemeint sei — als Nebenfund gemeldet und in `TODO.md` mit einer Empfehlung versehen | Kein Defekt. Die Entstehungsprüfung wurde erst nach der Meldung geführt und ergab, dass `9190888d` den Block vollständig neu schrieb — es gibt keine Vorgängerzeile, die `> 0` gelautet hätte, also auch keinen Umbau, bei dem eine Null verlorengehen konnte. Inhaltlich ist die Staffelung eine begründete Fallunterscheidung: Ein toter Tank lässt den Co-Tank halten, dann ist der Heiler das dringendere Ziel; zwei tote Tanks lassen niemanden halten, dann geht der Tank vor. Der Fund entstand daraus, dass eine ungewöhnliche Zahl als Tippfehler gelesen wurde, ohne die Gegenhypothese zu prüfen, dass sie Absicht ist — dieselbe Form wie der Kalibrierungsbeleg zu CountAllianceTanks. Lehre: Eine auffällige Konstante ist ein Anlass zur Prüfung, kein Befund; erst die Entstehungs- und Absichtsprüfung entscheidet, und sie gehört **vor** die Meldung, nicht danach | Eintrag aus `TODO.md` ersatzlos entfernt, PR-Beschreibung berichtigt |
| C37 | A54/A55 und PR #7: der Wiederbelebungspfad sei behoben — Ursache belegt, Umsetzung compile- und skriptgrün, Prüfgrad als „begründet, nicht gemessen" benannt | Die Umsetzung war im Spiel **schlechter als der Defekt** und ist vollständig zurückgenommen. Der Auftraggeber hat den Branch kompiliert und gemeldet: keine Wiederbelebung **und** kein Schimmerschild mehr. Ursache: Die Änderung stellte die Meldung des Wiederbelebungszaubers von „fast nie" auf „fast immer" um. Damit endete der GCD-Dispatcher, solange ein Toter in Reichweite lag, in seinem Wiederbelebungsblock (`return act` vor Heilung und Schaden bei `RaisePlayerFirst`) — und `nextGCD` war für den gesamten Fähigkeitenpfad die Wiederbelebung statt des normalen Zaubers, wodurch jeder Zweig, der `nextGCD` liest, anders entschied; beim Beschwörer fiel so Radiant Aegis aus. Die gleichzeitige `CanBeRaised`-Lockerung verschärfte es, weil `AutoStatus.Raise` dadurch häufiger stand. **Der methodische Fehler ist nicht die Analyse, sondern die Wirkungsprüfung:** Die Meldung eines GCD wurde als folgenloser Hinweis behandelt, obwohl sie den Dispatcher beendet **und** die Eingabe des gesamten Fähigkeitenpfads ist. Die Change Impact Analysis erhob den Weg der Wiederbelebung, nicht den Kreis der `nextGCD`-Leser. Zweiter Fehler: Der als „begründet, nicht gemessen" benannte Prüfgrad wurde zwar ehrlich ausgewiesen, aber nicht als Grund behandelt, die Änderung zurückzuhalten, statt sie zum Bauen freizugeben. Lehre: Wo eine Änderung die Eingabe einer nachgelagerten Entscheidungskette verschiebt, ist der Betroffenenkreis diese Kette — sie ist vor der Umsetzung auszuzählen; und ein Eingriff in den zentralen Dispatcher gehört nicht ohne Laufzeitbeobachtung in einen Branch, den der Auftraggeber baut | Code auf `main`-Stand zurückgesetzt, Defekt in `TODO.md` als offen geführt, `scan17.py` führt die Stelle als bekannt-offen, Konzept 11 auf den gescheiterten Versuch umgeschrieben |
| C38 | A59 und der Codekommentar in `UsePhoenixDown`: eine Verdrahtung nach Hausmuster hätte „zwei Federn für eine Leiche" verbraucht | Als Tatsache ausgegeben, was eine Inferenz war. Belegt ist allein, dass `UseAction` in einem Frame zweimal aufgerufen worden wäre — einmal in `UsePhoenixDown`, einmal in `DoAction`. Ob daraus zwei Verbräuche werden, ist von dieser Seite nicht entscheidbar. Die Fremddokumentation stützt den Verdacht allerdings deutlicher als der ursprüngliche Text: `UseAction` reiht eine während einer Sperre eintreffende Aktion ein, statt sie zu verwerfen. Unabhängig davon bleibt der zweite, belegbare Grund, den der Kommentar gar nicht nannte — `DoAction` verbucht das Ergebnis des zweiten Aufrufs als das Ergebnis, sodass `CurrentAction`, `_lastActionID` und `_lastUsedTime` einem Aufruf folgen, der nichts getan hat. Lehre: Eine Folge, die aus Spielinterna erst entstehen müsste, ist als Inferenz zu kennzeichnen, auch wenn die daraus abgeleitete Korrektur richtig bleibt | Kommentar und Konzept auf die belegbare Aussage zurückgeführt; die Entfernung des `Use()`-Aufrufs bleibt |
| C39 | A59, Begründung der Federverdrahtung: `ObjectHelper.CanBeRaised` sei für Federträger die falsche Prüfung und habe sie **blockiert**, weil ein Tank oder Schadensjob `RaisePvE` nicht besitzt | Die Blockade ist durch die Beobachtung des Auftraggebers widerlegt: Er erlebt Wiederbelebungen als Beschwörer, der `RaisePvE` ebenfalls nicht besitzt — `CanUseActionOnTarget` antwortet also nicht nach Erlernbarkeit. Die Umstellung auf die Item-Statusfrage bleibt richtig, aber als Gewinn an Genauigkeit (Reichweite, Inhaltsverbot, laufender Wiederbelebungsstatus), nicht als Beseitigung einer Sperre. Lehre: Eine Beobachtung des Auftraggebers aus dem laufenden Vorgang ist auch dann als Beleg heranzuziehen, wenn sie zu einem anderen Zweck berichtet wurde | Begründung in Konzept und A60 korrigiert; Code unverändert |
| C40 | Bericht vom 11.09.2026: „Upstream ist 21 Commits weiter … `DataCenter.cs` hat 262 geänderte Zeilen, und genau dort liegt der neue Helfer", mit der Empfehlung, den Sync bis nach dem Spieltest zu verschieben | Beide Zahlen stammen aus einer Messung gegen die **lokale** `main`, die seit dem 18.08.2026 nicht fortgeschrieben war — `git branch -vv` wies sie als „behind 382" aus. Gemessen gegen `origin/main` sind es **zwei** Commits, und die 262 Zeilen waren längst eingearbeitet; ausstehend waren 183 Zeilen Beastmaster-Zustand in einem Bereich, der die Helfer dieses Vorgangs nicht berührt. Zwei Abweichungen von der eigenen Regel in einer Zeile: `CLAUDE.md` schreibt `upstream/main...HEAD` vor, benutzt wurde `...main`; und gefetcht wurde nur `upstream`, nicht `origin`. Lehre: In einer dauerhaften Arbeitskopie ist eine lokale Zweigreferenz nie ein Zustandsnachweis, auch nicht für den eigenen Standardzweig | Zahlen korrigiert (A62), Messung durch `check_sync_state.py` abgesichert (A63), Regel in `CLAUDE.md` verschärft |
| C41 | A69 und das Konzept `12-searing-light-stacking.md`: „Die Abdeckung bleibt auch mit V2 bei 33 % statt der theoretisch möglichen 100 %" | Gilt nur für den synchronen Pull. Gemessen erreicht V2 bei auseinandergelaufenen Rotationen 90 % (sieben Beschwörer) und 96 % (acht). Die Aussage widersprach dem Versatz-Abschnitt desselben Dokuments, der die Streuung der Fenster als Gewinn beschreibt — ein innerer Widerspruch, den der Auftraggeber mit einer Überschlagsrechnung aufgedeckt hat. Lehre: Eine Prozentzahl aus dem Kopf ist eine Schätzung und als solche zu kennzeichnen oder zu messen; hier war ein Modell in einer Stunde gebaut | Konzept korrigiert, `searing_light_coverage.py` als Prüfmittel angelegt (A70) |
| C42 | Konzept `12-searing-light-stacking.md`, Versatz-Abschnitt: „Eine ausdrückliche Staffelung zwischen den Spielern ist weder nötig noch möglich — kein Client kennt die Wiederholzeiten der anderen" | Der Auftraggeber hat widersprochen: Man weiß zwar nicht, wann ein anderer zünden wird, aber ab seiner ersten Zündung, wann er frühestens wieder kann. Die Information liegt im Status selbst — `IStatus.SourceId` benennt den Urheber, und `StatusHelper.PlayerGetStatus` liest ihn bereits für die Unterscheidung eigener und fremder Buffs. Die Aussage war damit durch Code widerlegt, den dieses Dokument an anderer Stelle selbst zitiert. Lehre: Eine Unmöglichkeitsbehauptung ist gegen die Daten zu prüfen, die das eigene System ohnehin liest | Als V5 ausgearbeitet, gemessen und mit anderer Begründung verworfen (A70) |
| C43 | A72 und das Konzept: V6 sei nötig, weil V5 „nicht vergisst" — wer einmal gezündet hat, stehe dauerhaft in den Büchern und halte die anderen von einer Lücke zurück, die er nie füllt | Der Auftraggeber hat die Prämisse widerlegt: Die anderen haben eigene Abklingzeiten und können ohnehin nicht vor ihrem eigenen Fenster zünden, es gibt also nichts, wovon sie zurückzuhalten wären. Gemessen liefert die Fassung **ohne jede Buchführung** überall dieselben Werte wie V5 und V6, im Ausfallszenario eingeschlossen; bei sieben und acht Beschwörern ist die Buchführung sogar aktiv schädlich (46 gegen 100 Prozent). Nicht nur V6 war überflüssig, sondern die ganze Beobachtungsmechanik. Lehre: Bevor eine Konstruktion Information sammelt, ist zu zeigen, dass die Entscheidung ohne sie anders ausfiele | V5 und V6 verworfen, V7 an ihre Stelle; Invariante im Selbsttest verankert (A74) |

