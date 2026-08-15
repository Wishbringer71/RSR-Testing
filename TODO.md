# TODO / Offene Punkte (persistent — siehe CLAUDE.md REGEL, Persistenz-Klausel)

Diese Datei existiert, damit offene Konzepte und Findings eine
Kontextkomprimierung überleben. Bei Sitzungsbeginn lesen. Neue Findings
während der Arbeit hier ergänzen, nicht nur im Chat/Task-Tool belassen.

Nur offene Arbeit steht hier. Der vollständige Beleg-Trail (alle
abgeschlossenen Batch- und Einzelcommit-Prüfungen, 55/55 Commits Fork vs.
Upstream) liegt in `AUDIT_LOG.md` — dort nachsehen, bevor ein Commit/
Bereich erneut geprüft wird, um Doppelarbeit zu vermeiden.

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
Fix-Skizze (AKTUALISIERT nach Einzel-Audit von 76a683b/c01a5e2/16d4475, s.
AUDIT_LOG.md): NICHT das Gate pauschal erweitern (gleicher Blast-Radius-
Fehler wie beim ersten DefenseArea-Redesign-Versuch, der revertiert wurde).
Stattdessen: Das Repo hat bereits ein etabliertes Workaround-Muster —
Reprisal bei DRK/GNB und Addle bei SMN sind bewusst in BEIDEN Methoden
platziert (DefenseAreaAbility UND DefenseSingleAbility), sodass sie über
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
Status: VERIFIZIERT (Kernaussage), Fix noch offen, niedrige Priorität.
`VPR_Reborn.cs:256` (`!(IsBurst && SerpentsIrePvE.CanUse(out _))`) nutzt
`IsBurst`, um den BMR-Feint-Refresh-Guard auf ein "echtes Burstfenster" zu
verengen — aber `IsBurst => MergedStatus.HasFlag(AutoStatus.Burst)`
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
entscheidung) verwendet.
KORREKTUR nach Abgleich mit `e221ce5` (MCH-Folgecommit, s. AUDIT_LOG.md):
Schwere nach unten korrigiert. MCHs `wildfireSlotContested`/
`barrelStabilizerSlotContested` nutzen `IsBurst` genauso (praktisch immer
wahr) — dort verifiziert korrekt, weil es exakt die echte Cast-Bedingung
in `AttackAbility` spiegelt (die selbst genauso `IsBurst`-gegated ist,
"Spiegel-Prinzip"). VPRs `AttackAbility` castet Serpent's Ire ebenfalls
nur unter `if (IsBurst) {...CanUse...}` — `7c174ec`s Guard spiegelt also
strukturell exakt die echte Nutzungsbedingung, genau wie bei MCH als
korrekt etabliert. Der CODE ist damit vermutlich NICHT defekt. Was
bleibt: die Commit-Message von `7c174ec` ("scopes the guard back to the
narrow window it was meant for") ist IRREFÜHREND formuliert — sie
suggeriert echte Zeitfenster-Verengung, die es (da IsBurst kein
Zeitfenster ist) nicht gibt. Reines Dokumentations-/Selbstdiagnose-
Problem, keine funktionale Lücke. Nur Kommentar-Korrektur nötig, kein
aktiver Bug-Fix.
Auch zu prüfen (separates, eigenständiges Item, nicht mit #52
vermischen): `030129c` (RPR Gluttony/Enshroud-Guard, kein `IsBurst`
verwendet) — Prämisse "burst-exklusiv gehalten" durch `AttackAbility`-
Code nicht eindeutig gestützt (Gluttony/Enshroud dort ressourcen-/
comboZustand-gegated, nicht `IsBurst`-gegated; EnshroudPooling-Mechanik
macht die Frage aber nicht trivial). Noch offen, braucht ggf. eigenen
Zyklus falls sich beim Weiter-Audit ein echter Impact zeigt.

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
  Upstream-Sync-Check (gemäß CLAUDE.md-Regel, vor Arbeitsbeginn
  durchgeführt): Bug existiert IDENTISCH in `upstream/main` — kein
  Fork-eigener Fehler, von Anfang an geerbt, im Original ebenfalls nicht
  gefixt. Kein Doppelarbeit-Risiko für diesen Punkt.

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

## Wichtig für zukünftige Sessions

Diese Dateien (TODO.md, AUDIT_LOG.md) existieren nur auf dem Branch, auf
dem sie committet wurden. Falls sie fehlen, obwohl an diesem Repo
gearbeitet wird: das dem Nutzer explizit melden (siehe CLAUDE.md), nicht
stillschweigend neu anfangen. Beide Dateien bei Sitzungsbeginn lesen —
nicht nur TODO.md.
