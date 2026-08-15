# TODO / Offene Punkte (persistent — siehe CLAUDE.md REGEL, Persistenz-Klausel)

Diese Datei existiert, damit offene Konzepte und Findings eine
Kontextkomprimierung überleben. Bei Sitzungsbeginn lesen. Neue Findings
während der Arbeit hier ergänzen, nicht nur im Chat/Task-Tool belassen.

Nur offene Arbeit steht hier. Der vollständige Beleg-Trail (alle
abgeschlossenen Batch- und Einzelcommit-Prüfungen, 55/55 Commits Fork vs.
Upstream) liegt in `AUDIT_LOG.md` — dort nachsehen, bevor ein Commit/
Bereich erneut geprüft wird, um Doppelarbeit zu vermeiden.

## Offene Konzepte / Fixes (noch nicht umgesetzt)

### #46 — Pre-Pull-Schutz (HoT/Schild) auf Tank vor Wall-to-Wall-Erstcharge, mit Erneuerung während des Pulls
Status: Sinnhaftigkeit bestätigt, Umfang teilweise geprüft, NICHT umgesetzt.
WHM hat bereits `UsePreRegen` (Regen auf Tank, Countdown 3-5s vor Pull) —
deckt "vor dem ersten Charge" ab, aber zeitbasiert statt distanzbasiert,
UND ohne Erneuerung während des laufenden Pulls (`CountDownAction` läuft
nur vor Kampfbeginn; die reguläre `HealSingleGCD`-Regen-Nutzung ist rein
reaktiv HP-schwellenbasiert, nicht proaktiv auf den pullenden Tank
ausgerichtet — echte, bisher unentdeckte Lücke). SCH hat ein Äquivalent
mit Adloquium (Schild, 6-7s Countdown, config-gated `AdloquiumDuringCountdown`).
KORREKTUR (Nutzerhinweis): Schilde sind NICHT nur ein einmaliger Pre-Pull-
Cast wie ursprünglich unterstellt — sie können genau wie ein HoT sowohl
vor Pull-Beginn ALS AUCH während des laufenden Pulls erneuert werden.
#46 ist damit kein reines "HoT vs. WHM"-Thema, sondern gilt gleichermaßen
für Schild-Heiler (SCH) — dieselbe Lücke (keine Erneuerung während des
Pulls) betrifft SCH vermutlich genauso. AST/SGE haben aktuell gar keinen
Pre-Pull-Tank-Schutz. Voller Scope (alle 4 Healer, je eigenes Tool/Design)
noch nicht konzipiert — Rückfrage zum weiteren Vorgehen läuft.

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
