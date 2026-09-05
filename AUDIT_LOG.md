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
| **Kommentar an Code angeglichen statt umgekehrt** | `CycleStateManualAuto` trug bei Einführung den Kommentar „If currently in Manual mode, turn Off" über einem `DoStateCommandType(Auto)`; `2771dd95` (01.10.2025) änderte den Kommentar auf „switch to Auto" | Der Widerspruch zwischen Absicht und Umsetzung wird aufgelöst, indem die Absichtsbeschreibung fällt — danach ist der fehlende Ausschaltweg nicht mehr als Defekt erkennbar |
| **Refactoring entfernt Aufrufer, lässt Definition stehen** | `IncrementState` (`e62d9123`), `BeginChild`/`IsFailed` (`701554b0`), `GetHostileTypeDescription`/`SetTargetingType` (`e3b57004`) | Der Compiler meldet ungenutzte private Methoden nicht als Fehler; die Reste akkumulieren |
| **Bibliothekskonfiguration trifft Auslieferung** | `d07d7b66` („fix: add a nuget package") aktivierte `GeneratePackageOnBuild`, damit `RotationSolver.Basic` als NuGet-Paket für Fremdrotationen bereitsteht; zusammen mit `GenerateDocumentationFile` und einem gemeinsamen Ausgabeverzeichnis landet beides im Plugin-Zip | Zwei legitime Ziele — Bibliothek veröffentlichen, Plugin ausliefern — teilen sich ein Ausgabeverzeichnis, ohne dass die Auslieferung gefiltert wird |
| **Fremdschnittstelle als Zahl statt als Typ behandelt** | `SpecialMode`-Spiegelenum gegen `AIHints.SpecialMode` verschoben; Timeline-Vorzeichen nur auf der Hints-Seite normalisiert | Über die IPC-Grenze kommt ein `int`; ohne Abgleich mit der Quelle bleibt eine Abweichung folgenlos, bis der betroffene Wert gelesen wird |
| **Feature aus anderer Epoche als Entscheidungseingang wiederverwendet** | Die selbstlernende `HostileCastingArea` stammt aus der frühen UI-Phase (`51ad02c6` u. a.) und wurde später Eingang der Mitigationsentscheidung | Ein als Komfortfunktion gebautes Merkmal erbt keine Anforderungen an Genauigkeit, die seine spätere Verwendung stellt |

**Nicht belegbar:** die Absicht hinter `SpreadDamagePaths` — die Liste entstand in `33e6acb1` („Refactor for var usage, safety checks, and plugin compat"), also in einem Sammel-Refactoring ohne erkennbare fachliche Begründung. Ob eine Trennung von Spread- und Stack-Markern geplant und nie gefüllt wurde, oder ob die Liste von Beginn an eine Fehlkopie war, geht aus dem Commit nicht hervor.

**Folge für die Behebung:** Vier der sieben Muster sind durch die Behebung des Einzelfalls nicht erledigt. Bei der veraltenden Positivliste ist die Aufzählung durch eine Fähigkeitsprüfung zu ersetzen, sonst veraltet sie erneut; beim Klonmuster ist die gemeinsame Struktur zu extrahieren; bei der Auslieferung ist das Ausgabeverzeichnis zu trennen; bei der Fremdschnittstelle braucht es einen wiederholbaren Abgleich statt einer einmaligen Korrektur.

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
| C11 | Erste Entscheidungsvorlage: VPR-Blöcke „löschen", UI-Wrapper „behalten", Toggle-Konflikt per `applyToggle` lösen, Timeline-Vorzeichen im Spiel ablesen lassen | Vier Fehler in einer Vorlage: das Konfliktrisiko war über die Dateiaktivität geschätzt statt regionsgenau gemessen (0 statt 7 für VPR, 2 statt 12 für die UI-Region); die VPR-Blöcke wurden nicht gegen die Gegenhypothese „unverdrahtet statt tot" geprüft; `applyToggle` hätte `DTRManualAuto`-Nutzern den einzigen Ausschaltweg genommen, was erst die Auswertung als Zustandsfolge zeigte; und die Vorzeichenfrage stand im Quelltext von BossModReborn, war also keine Prüfaufgabe für den Auftraggeber. Lehre: eine Vorlage ohne durchgerechnete Konsequenzen ist keine Vorlage, und verfügbare Quellen sind vor der Vorlage auszuschöpfen | A11: alle vier revidiert |
| C10 | A9: „`IsHostileCastingTank`-Fallback · KEIN FEHLER, nicht angetastet" | Die Begründung galt der Tankbuster-Erkennung für Tanks (`IsHostileCastingToTank`) und wurde ungeprüft auf `…TankBusterAtMe` übertragen, obwohl das eine andere Frage beantwortet: nicht „kommt ein Tankbuster", sondern „kommt einer auf mich". Für Nicht-Tanks ist der Fallback dort schlicht falsch. Nutzer: „wenn mich ein Mob aus einer großen Mobgruppe angreift, wird bereits Schimmerschild und Stumpfsinn gecastet" | `…TankBusterAtMe` auf gesicherte Tankbuster eingeschränkt (A9, d9a99de7) |
