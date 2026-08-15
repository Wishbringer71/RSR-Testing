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
