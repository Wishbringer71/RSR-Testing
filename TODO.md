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
Fix-Skizze: gleiches job-scoped Opt-in-Muster wie `HasHostileCountAoeMitigation`
(Commit f154d57) — NICHT das Gate pauschal erweitern (gleicher Blast-Radius-
Fehler wie beim ersten DefenseArea-Redesign-Versuch, der revertiert wurde).
Noch nicht implementiert, noch nicht kritisch geprüft.

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

## Wichtig für zukünftige Sessions

Diese Datei existiert nur auf dem Branch, auf dem sie committet wurde.
Falls sie fehlt, obwohl an diesem Repo gearbeitet wird: das dem Nutzer
explizit melden (siehe CLAUDE.md), nicht stillschweigend neu anfangen.
