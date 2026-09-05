# TODO / Offene Punkte (persistent — siehe CLAUDE.md REGEL, Persistenz-Klausel)

Diese Datei existiert, damit offene Konzepte und Findings eine
Kontextkomprimierung überleben. Bei Sitzungsbeginn lesen. Neue Findings
während der Arbeit hier ergänzen, nicht nur im Chat/Task-Tool belassen.

Nur offene Arbeit steht hier — ausnahmslos. Ein Punkt mit Status
GEFIXT/ABGESCHLOSSEN/VERWORFEN gehört NICHT mehr in diese Datei, sondern
wird nach `AUDIT_LOG.md` (Beleg-Archiv) verschoben, sobald der Status
feststeht — nicht hier mit erledigtem Status stehen gelassen. Der
vollständige Beleg-Trail (alle abgeschlossenen Batch- und
Einzelcommit-Prüfungen, Fork vs. Upstream, plus die komplette Herleitung
aller bisherigen Feature-/Aggro-Management-Arbeit) liegt in `AUDIT_LOG.md`
— dort nachsehen, bevor ein Commit/Bereich erneut geprüft oder ein
scheinbar neues Thema begonnen wird, um Doppelarbeit zu vermeiden.

## ROADMAP: Vollprüfung aller Fork-Patches gegen das Original (laufend)

Anlass: Der Originalautor haelt saemtliche Aenderungen fuer Trial&Error ohne
Verstaendnis der Codebasis, >4000 Zeilen die nichts richtig machen. Auftrag:
belegen oder widerlegen, korrigieren, mit so wenig Code wie moeglich, ohne das
inhaltliche Gesamtkonzept zu verlieren.

**Reihenfolge ist verbindlich. Keine Phase ueberspringen, keine Phase
abbrechen, weil eine spaetere interessanter wirkt.**

| Phase | Inhalt | Stand |
|---|---|---|
| 0 | Faktenbasis: Zeilenbilanz, Kommentarbilanz, Versionierung | **fertig** |
| 1 | Substanzpruefung je Bereich — feuert der Zweig, tut er das Richtige, ist er minimal, passt er zum Original | **fertig** |
| 1.1 | `Updaters/StateUpdater.cs` | fertig |
| 1.2 | `Rotations/CustomRotation_Ability.cs` | fertig |
| 1.3 | `Rotations/CustomRotation_OtherInfo.cs` | fertig |
| 1.4 | `DataCenter` · `ObjectHelper` · `StatusHelper` · `ActionTargetInfo` · `CustomRotation_Items` · `HpPotionItem` | fertig |
| 1.5 | Heiler: WHM · AST · SGE · SCH | fertig |
| 1.6 | Tanks: PLD · WAR · DRK · GNB | fertig |
| 1.7 | Melee · Phys. Range · Magical | fertig |
| 1.8 | Duty · ExtraRotations · PvP | fertig |
| 2 | Korrekturen umsetzen (rollierend je Fund) | laufend |
| 3 | Stilangleichung: Kommentardichte auf Hausmass je Bereich | **fertig** — Ueberhang 283 → 102, RebornRotations 26,6 % → 9,9 % |
| 4 | Versionierung des Forks in Ordnung bringen | **offen — braucht Nutzerentscheidung (Tag-Schema)** |
| 5 | CI gruen + Dokumentation des Ergebnisses | offen |

### Phase-0-Ergebnis (Faktenbasis)

- „ueber 4000 Zeilen": 4521 Zeilen Diff gegen `upstream/main`, davon **2830
  Markdown** (Doku/TODO/AUDIT_LOG, wird nie ausgeliefert), 260 CI, und
  **1431 C#**. Von den 1431: 178 leer, 413 Kommentar, 282 reine Klammern —
  **558 tatsaechliche Anweisungen** ueber 43 Dateien.
- Kommentardichte der Ergaenzungen gegen Hausmass je Bereich:
  RebornRotations 26,6 % (Haus 3,9 %), Updaters 37,3 % (6,2 %), Rotations-Kern
  49,7 % (19,6 %), Helpers/Actions 37,4 % (19,7 %). Ueberhang ~283 Zeilen.
  **Der Stilvorwurf trifft zu.**
- **Versionierung: der Vorwurf trifft zu.** Der Fork hat **0 Tags**, Upstream
  hat 952. `publish.yaml` triggert ausschliesslich auf Tags `*.*.*.*`, und
  `AssemblyVersion` wird nur dort aus dem Tag gestempelt. Ohne Tag ist jede
  Version **1.0.0.0**. `manifest.json` und `RotationSolver.json` sind
  unveraendert gegenueber Upstream, der Fehler liegt allein in den fehlenden
  Tags. Siehe Phase 4.

## Offene Konzepte / Fixes (noch nicht umgesetzt)

### #54 WHM-Heilsuppression nach oGCD-Erschöpfung — Root Cause weiterhin NICHT gefunden, alle bekannten Kandidaten ausgeschlossen

Nutzer-Meldung, wörtlich verifizierte Fakten (keine Interpretation):
Normales Dungeon (Klyteum), echte Mitspieler (keine Duty-Support-NPCs).
Schwellen: alle acht Werte (Ability+Spell, Single+Area, je mit/ohne HoT)
konsistent auf >70% (ohne HoT) / 55-65% (mit HoT) angehoben. oGCD-Heilung
funktionierte zu Beginn, dann oGCDs aufgebraucht. Danach feuerten
manaverbrauchende GCD-Heilsprüche (Cure/Cure II) trotz vollem Mana NICHT —
kein Castversuch, kein Balken (nicht "abgebrochen", sondern nie versucht).
Tank fiel bis unter 20% HP, kein Schild/Barriere aktiv. Gegner (>3, kein
Mitheiler) waren unter 50% HP, aber weit von Tod entfernt, Ausgang unklar.
Heiler castete in dieser Zeit aktiv eine AoE-Stun-Aktion (Holy, CC bei 3+
Gegnern) — selbst gewählt von RSRs Automatik, nicht manuell vom Nutzer
gedrückt. Kein Stun auf den Heiler selbst.

Der bereits gefixte `AverageTTK`-Nullfallback (siehe AUDIT_LOG.md) erklärt
dies NICHT vollständig (nur ~2.5s-Fenster am Pull-Start).

**Vollständig ausgeschlossene Kandidaten (mit Beleg):**
- `CancelCastUpdater.shouldStopHealing` (Configs.cs:713, Default `false`) —
  setzt bereits laufenden Cast voraus, gab es laut Nutzer nicht.
- `DataCenter.IsTyrantCastingSpecialIndicator()` (nur `IsInM11S`) — normales
  Dungeon, nicht M11S.
- Schild-Credit auf Effective-HP (`ShieldCreditAllowed`/`HasSurvivingShield`
  in `ShouldHealSingle`) — kein Schild/Barriere vorhanden.
- Getrennte Schwellenpaare `HealthSingleAbility(Hot)` vs.
  `HealthSingleSpell(Hot)` — alle acht Werte konsistent angehoben, nicht nur
  eine Zeile.
- Per-Action `ActionConfig.AutoHealRatio` (Default 0.8) — laut Nutzer nie
  verändert.
- `EmergencyGCD` (`CustomRotation_GCD.cs:72-79`) vor Heal-Branches — WHM
  PvE hat keine Override, Basisklasse macht in PvE nichts, keine
  `CurrentDutyRotation` für normales Dungeon.
- Manuelles Overriding durch den Spieler — Nutzer hat nichts gedrückt,
  RSR-Automatik hat Holy selbst gewählt.
- Upstream-Issue #1351 (NPC-Duty-Support-HP-Lesefehler) — laut Melder
  explizit nicht bei echten Mitspieler-Partys, hier echte Mitspieler.
- Keine weiteren passenden Upstream-Issues gefunden (durchsucht: heal, cure,
  "won't heal", "not healing", "GCD heal", holy — nur altes, 2024
  geschlossenes #70 mit gegenteiligem Symptom, nicht relevant).

**Noch nicht geprüft / fehlende Daten:** WHM
`HealSingleGCD`-Swiftcast+Raise-Kurzschluss (WHM_Reborn.cs ~336-364, nur
relevant wenn Rez ansteht — bei diesem Vorfall nicht erwähnt, daher
nachrangig). Kein Zugriff auf Live-Diagnosedaten (RSR-Debug-Statusfenster
zeigt `AutoStatus`/`MergedStatus`-Flags und tatsächlich verwendete
HP-Werte live) — ohne das keine weitere Eingrenzung per Code-Lektüre
möglich, da alle bekannten Codepfad-Kandidaten durchgeprüft sind.

**Nächster Schritt:** Beim nächsten Auftreten das RSR-Debug-Statusfenster
offen halten/Werte notieren (insbesondere ob `HealSingleSpell`-Flag gesetzt
war) — das ist der einzige noch verbleibende Weg, den Suchraum weiter
einzugrenzen.

KEIN Fix umsetzen, bevor eine Ursache tatsächlich belegt ist — Stand jetzt
ist nichts bestätigt.

### #55 `_lastHp` in `DataCenter.GetPartyMemberHPRatio` toter Code — Heil-Prädiktions-Cleanup greift nie

`RotationSolver.Basic/DataCenter.cs:1096/1115`: `_lastHp` wird deklariert und
per `TryGetValue` gelesen, aber NIRGENDS im Repo beschrieben
(`_lastHp[...] = ...` existiert nicht). Dadurch ist `lastHp` in
`GetPartyMemberHPRatio` immer `0`, und die Bedingung
`currentHp - lastHp == healedHp` (Zeile 1117, soll erkennen "die eigene
Heilung ist im echten HP-Wert angekommen, prädiktiven Eintrag entfernen")
kann praktisch nie wie beabsichtigt zutreffen. Folge: der prädiktive
HP-Ratio-Ausgleich (`Math.Min(1, (healedHp + currentHp) / maxHp)`, Zeile
1123) wird nicht wie vorgesehen durch echten HP-Abgleich beendet, sondern
nur dadurch, dass `DataCenter.HealHP` beim nächsten Self-Action-Effekt
(`Watcher.cs:211`) komplett neu zugewiesen wird — faktisch harmlos in der
Praxis (Fenster ohnehin nur `EffectTime`..`EffectEndTime`, typ. ~1.6-1.8s),
aber die vorgesehene Abgleichlogik ist funktional tot. Eigenständiger, von
der WHM-Heilsuppression unabhängiger Fund — nicht Ursache von #54 (Fenster
zu kurz für das gemeldete Muster), aber echter Bug, der bei Gelegenheit
bereinigt werden sollte (entweder `_lastHp` korrekt pflegen oder toten
Zweig entfernen).

### #63 Sustain-HP-Boden: Asymmetrie WHM/AST/SGE und umgekehrte Vergleichsrichtung bei AST

Aus dem Critic-Loop zur Vereinheitlichung in `6b40600`. Zwei unwiderlegte
Punkte, beide nur im Spiel entscheidbar:

1. **Asymmetrie**: der proaktive Sustain hat bei WHM einen HP-Boden von 0.3
   (`RegenHeal`), bei AST 0.4 (`AspectedBeneficHeal`), bei SGE **keinen**.
   Entweder gehoert der Boden ueberall hin (dann fehlt er SGE), oder
   nirgends (dann sind WHM/AST zu restriktiv). Fuer SGE gibt es keine
   bestehende Einstellung zum Wiederverwenden — eine neue zu erfinden waere
   Scope Creep ohne Belegt, deshalb bewusst offen gelassen.

2. **Vergleichsrichtung bei AST**: derselbe Wert `AspectedBeneficHeal` wird
   im reaktiven Zweig als `GetHealthRatio() < AspectedBeneficHeal`
   (AST_Reborn.cs:537, "heile ihn, er ist verletzt") und im proaktiven
   Helfer als `>` ("pflege den HoT, er ist gesund") verwendet. Das ist
   nicht falsch — die Schwelle trennt sinnvoll beide Faelle —, aber fuer
   den Nutzer nicht erkennbar: wer den Wert hochzieht, bekommt mehr
   reaktive Heilung UND weniger proaktiven Sustain. WHM ist konsistent
   (beide Zweige `>`).

**Pruefbar im Spiel**: waehrend eines Wall-to-Wall beobachten, ob der
HoT-Refresh aussetzt, sobald der Tank unter die Schwelle faellt, und ob
das stoert. Relevant vor allem in Inhalten mit zwei lebenden Heilern, wo
`CanHealSingleSpell` wegen `GCDHeal == false` (ASTs Default!) falsch ist
und `GeneralGCD` deshalb auch mit verletztem Tank erreicht wird.

### #65 Offen aus dem Zielkonzept: B3/B4/B5/C1 (Spielfragen, nicht Codefragen)

Branch `claude/rotation-flow-refactor`, PR #3. Siehe
`docs/rotation-flow/04-concept.md`. Vier Punkte brauchen eine
Spielentscheidung, keine Codeentscheidung:

- **B3** Reprisal-Sustain: DRK/GNB haben ihn in DefenseArea UND DefenseSingle,
  PLD/WAR nur in Single. Folgt der Upstream-Platzierung je Job, ist also
  begruendet — macht aber dieselbe Faehigkeit rollenintern uneinheitlich.
- **B4** Phys. Fernkaempfer: keine zwei der drei Jobs belegen dieselben
  Dispatch-Slots (DNC ohne DefenseSingleAbility, MCH ohne HealSingleAbility,
  BRD als einziger mit DispelAbility). Erst pruefen, ob fachlich begruendet.
- **B5** MNK ueberschreibt HealAreaAbility statt HealSingleAbility, obwohl
  Second Wind eine Einzelziel-Selbstheilung ist. Einzige Gruppenabweichung
  ohne erkennbare Begruendung.
- **C1** DRG-Trait-Paare (DRG_Reborn.cs:288-405, 8x
  `if (Trait.EnoughLevel){…} if (!Trait.EnoughLevel){…}`). Das Gate zu
  streichen ist NICHT verhaltensgleich: gegatet schliessen die Zweige einander
  aus, ungegatet werden sie zum Fallback. Unterschied greift, wenn die
  Upgrade-Aktion aus einem anderen Grund als dem Level nicht kann (Combo,
  Reichweite, Status). Zu klaeren ist, ob `FullThrustPvE.CanUse` oberhalb der
  Traitstufe noch `true` liefert.

Alle vier haben winzige Diffs. Was fehlt, ist die Beobachtung im Spiel. NICHT
ungeprueft angleichen — das waere derselbe Fehler wie bei der Provoke-Distanz
(#56).

### #66 A4a: Dispatch-Zweige zu benannten Stufen extrahieren (reaktiviert)

Aus der Neubewertung des Konzepts nach der Praemissenkorrektur (der Branch
setzt keine Code-Kompatibilitaet zum Original mehr voraus, Upstream wird
inhaltlich geprueft statt gemergt — damit ist das Merge-Kosten-Argument, das
A4 und C gekippt hatte, hinfaellig).

Gemessen ueber alle 31 PvE-Rotationsdateien: 1239 Zweige auf oberster Ebene in
den Dispatch-Methoden, `GeneralGCD`-Median 16, Maximum 80 (BLU).

Reihenfolge nach Nutzen, eine Datei pro Commit: BLU (80) → PhantomDefault (33)
→ PCT (32) → SAM (27) → MCH (23) → SMN (23). Unter ~15 Zweigen lohnt es nicht.

Abnahmebedingungen pro Datei (aus dem Critic-Durchgang, siehe Konzept A4):
1. Keine Extraktion ueber eine Local hinweg, die vor UND nach der
   Schnittgrenze gelesen wird (DRG: `doomSpikeRightNow`).
2. Jedes `return false` in der extrahierten Region einordnen: „Stufe greift
   nicht" (unkritisch) vs. „ganze Methode abbrechen" (braucht `out`-Flag, wie
   `BLM_Default.InFireOrIce(out act, out mustGo)`). Im Zweifel Datei
   ueberspringen und hier vermerken.
3. Zweigreihenfolge exakt erhalten; der Diff muss zeigen, dass nur verschoben
   wurde.
4. CI-Build gruen.

Namensgebung: Namen kommen aus der Fachlogik des Jobs (`GoIce`, `MaintainFire`,
`AddThunder` — so macht es BLM_Default bereits), NICHT aus einer festen
Taxonomie. Die Neunerliste im Konzept beschreibt die Reihenfolge der Stufen,
nicht ihre Namen.

### #67 Upstream-Inhaltsprüfung: 8 neue Commits seit dem Branch-Punkt

Neue Vorgabe fuer diesen Branch: Upstream wird NICHT gemergt, sondern auf
Inhalt geprueft — welche Verbesserung/Fehlerbehebung/Erweiterung bringt der
Commit, und gilt sie hier auch. Stand `git fetch upstream` am 05.09.2026,
Branch-Punkt `ee055ca` (16.08.2026), Upstream-Kopf `f5c8432`. 16 Commits
Rueckstand, davon 8 ohne Merge-Commits:

| Commit | Inhalt | Betrifft |
|---|---|---|
| `53822a8` | Bard-Songreihenfolge, Anpassung an Dalamud-Aenderung | BRD_Reborn |
| `e003bce` | DRK-Rotationsfixes | DRK_Reborn (hier geaendert!) |
| `df1a8c9` | Nicht-FATE-Mobs waehrend FATEs anders behandelt | Targeting, zentral |
| `7b8a2f5` | Doppelte Oblation-Nutzung, ECommons-Update | DRK_Reborn (hier geaendert!) |
| `0bde9ed` | Crash im Next-Action-Fenster bei ungueltigem Zielobjekt | UI |
| `b5a91d7` | SGE-Logik, Targeting-Probleme | SGE_Reborn (hier geaendert!) |
| `69f4844` | GNB-Fixes, strengere Status-Listen-Guards | GNB_Reborn |
| `83e4d0e` | BLU Exuviation wurde nicht als AoE-Heilung genutzt | BLU_Reborn |

Drei davon (DRK 2x, SGE) betreffen Dateien, die dieser Branch bereits
angefasst hat — dort ist die Pruefung nicht optional, sondern noetig, um
nicht gegen einen veralteten Stand zu arbeiten.

Auch `origin/main` ist um dieselben 16 Commits zurueck und hat 8 eigene.

### #68 Oblation-Doppelnutzung: Upstream-Fix greift nicht in ChurinDRK

Aus der Inhaltspruefung von `7b8a2f5` ("Fix double Oblation usage"). Upstream
hat drei Aufrufstellen in `DRK_Reborn.cs` in `if (!IsLastAbility(false,
OblationPvE))` gekapselt — `CanUse(..., usedUp: true)` gibt beide Ladungen
frei, also konnten in zwei aufeinanderfolgenden oGCD-Fenstern beide auf
dasselbe Ziel gehen, ohne dass die zweite etwas bewirkt.

Dieselbe Aufrufform steht ungeschuetzt in
`RotationSolver/ExtraRotations/Tank/ChurinDRK.cs:185`
(`OblationPvE.CanUse(out act, usedUp: true, skipStatusProvideCheck: false)`).
Upstream hat sie nicht mitgefixt. Gesamtheitlichkeitsfall: derselbe Fehler,
andere Datei.

Vor Uebernahme pruefen, ob ChurinDRK denselben Dispatch-Pfad hat (eigene
DefenseSingleAbility-Ueberschreibung, kein anderweitiger Ladungs-Guard)
— dann Guard analog setzen. Nicht blind kopieren.

### #69 Aktions-Abdeckung: verbleibende ungenutzte Aktionen (Rotationsfragen)

Aus der Abdeckungsanalyse, siehe `docs/rotation-flow/05-action-coverage.md`.
Nach Abzug von Limit Breaks, Pet-Aktionen, Stance-Abbruechen,
Morph-Platzhaltern und Upgrade-Griffen (die alle korrekt ungenutzt sind)
bleiben diese Aktionen, fuer die keiner dieser Gruende greift. Jede braucht
eine Rotations-/Spielentscheidung — NICHT ungeprueft nachruesten.

| Job | Aktion | Frage |
|---|---|---|
| SAM | `HissatsuYatenPvE` | Schaden + Rueckstoss + Enhanced Enpi. In `MoveBackAbility`? Kostet eine oGCD. |
| SAM | `MeditatePvE` | Kenki-Aufbau in der Downtime. Braucht ein Downtime-Signal. |
| SAM | `TsubamegaeshiPvE` | Iaijutsu-Wiederholung. Pruefen ob ueber `IaijutsuPvE`-Morph abgedeckt. |
| MCH | `FlamethrowerPvE` | Kanalisierte AoE-DoT, bricht bei Bewegung. Nur sinnvoll bei Downtime. |
| MCH | `RookOverdrivePvE` / `QueenOverdrivePvE` | manuelle Ausloesung des Automaton-Finishers. |
| NIN | `ShadeShiftPvE` | 20%-Selbstschild — `DefenseSingleAbility`-Kandidat, passt zum DPS-Selbstschutz-Thema (#12). |
| NIN | `ShukuchiPvE` | Bodengezielter Sprung. Kein passender Dispatch-Slot (nicht ziel-, sondern ortsbezogen). |
| WHM | `LiturgyOfTheBellPvE_28509` | manuelle Detonation der Glocke. |
| AST | `HoroscopePvE_16558` | Aktivierung des gelegten Horoscope. |
| AST | `PlayIPvE`/`PlayIiPvE`/`PlayIiiPvE`/`MinorArcanaPvE` | Pruefen, ob ueber die konkreten Kartenaktionen abgedeckt oder echte Luecke. |
| SCH | `DissolveUnionPvE` | Fey Union aufloesen. |
| SCH | `EmergencyTacticsPvE_37037` | zweite Emergency-Tactics-ID. |
| SMN | `TridisasterPvE` | AoE-Angriff. |
| MNK | `SixsidedStarPvE` | Schaden + Bewegungsgeschwindigkeit, Downtime-Werkzeug. |

Ausserdem offen aus derselben Analyse: **PLD-Invuln liegt anders als bei
DRK/GNB/WAR.** Bei den drei anderen Tanks haengt die Invulnerability in
`EmergencyAbility` der Basisschicht (`{Job}Rotation.cs`), gegated auf
`Service.Config.HealthForDyingTanks`; bei PLD in `PLD_Reborn.cs:92/97` mit
eigener Logik (`HallowedWithCover`). Vier Tanks, dieselbe Faehigkeitsklasse,
zwei Orte, zwei Gates. Erst klaeren, ob PLDs Cover-Sonderfall die Abweichung
rechtfertigt.

## Wichtig für zukünftige Sessions

Diese Dateien (TODO.md, AUDIT_LOG.md) existieren nur auf dem Branch, auf
dem sie committet wurden. Falls sie fehlen, obwohl an diesem Repo
gearbeitet wird: das dem Nutzer explizit melden (siehe CLAUDE.md), nicht
stillschweigend neu anfangen. Beide Dateien bei Sitzungsbeginn lesen —
nicht nur TODO.md.
