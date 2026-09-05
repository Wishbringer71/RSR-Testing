# 06 · Was dieser Fork gegenüber dem Original ändert — vollständig, mit Beleg

Dieses Dokument existiert, weil der Autor des Originals die Änderungen als
„Trial & Error ohne Verständnis der Codebasis, über 4000 Zeilen die nichts
richtig machen" bezeichnet hat. Es beantwortet das nachprüfbar: jede
Abweichung, ihr Zweck, und wo sie sich als falsch erwiesen hat, was daraus
geworden ist.

Stand: `upstream/main` = `f5c8432`, Branch `claude/rotation-flow-refactor`.
Alles unten ist gegen genau diesen Stand gemessen und mit
`git diff upstream/main` reproduzierbar.

---

## 1 · Der Umfang, richtig gezählt

| | |
|---|---|
| Diff gesamt gegen `upstream/main` | 4474 Zeilen |
| davon **Markdown** (`TODO.md`, `AUDIT_LOG.md`, `docs/`) | **3112** — wird nicht ausgeliefert |
| davon CI (Prüfskript + Workflow-Job) | 268 — läuft nicht im Spiel |
| davon **C#** | **1094 hinzugefügt, 106 entfernt**, 43 Dateien |
| davon leer / Klammern / Kommentar | 156 / 234 / 201 |
| **tatsächliche Anweisungen** | **~500** |

Die Zahl „über 4000" stimmt für den Rohdiff. Für Produktivcode stimmt sie
nicht: das sind rund 500 Anweisungen über 43 Dateien, im Schnitt zwölf pro
Datei. (Stand nach dem zweiten Durchgang, §6; davor ~470 — der Zuwachs sind
im Wesentlichen die vervollständigte Schild-Statusliste und die
Reprisal-Statusprüfung.)

---

## 2 · Was sich als falsch erwiesen hat

Diese vier Punkte waren echte Fehler. Sie sind entfernt oder
zurückgebaut — nicht verteidigt.

### 2.1 SGE-Sustain war ein Dauerläufer über den ganzen Pull

`EukrasianDiagnosisPvE.Setting.TargetStatusProvide` ist
`[EukrasianDiagnosis, Galvanize]` — der **Schildstatus selbst**
(`SageRotation.cs:156`). Ein Schild wird durch Schaden aufgebraucht. Im
Wall-to-Wall, wofür die Funktion geschrieben war, platzt die Barriere in
Sekunden, `WillStatusEndGCD` meldet „abgelaufen", und der Zweig — der in
`GeneralGCD` **vor** Phlegma und allem Schaden stand — legte sie neu. Zwei
GCDs pro Platzer, den ganzen Pull.

WHMs Regen und ASTs Aspected Benefic sind HoTs und ticken ihre Dauer ab; für
sie trägt derselbe Helfer und bleibt. **Nur SGE war betroffen, Feature
entfernt.**

### 2.2 Weakness-Schwellenfaktor heilte praktisch immer

Weakness halbiert empfangene Heilung, also wurde die Heilschwelle mit 1,5
multipliziert. Mit den Standardwerten (`_healthSingleAbility = 0.7`,
`_healthSingleSpell = 0.65`, `Configs.cs:1369/1375`) ergibt das 1,05 → auf
1,0 geklemmt, bzw. 0,975. Ein geschwächter Spieler galt damit bei **jeder**
HP unter voll als heilbedürftig — 100 s lang bei Weakness, 300 s bei Brink of
Death, also nach jedem Rez.

Die Prämisse war zusätzlich falsch: halbierte Heilung heißt, die HP steigen
langsamer, und darauf reagieren die bestehenden Schwellen bereits von selbst.
**Entfernt, samt der nur dafür angelegten `IsWeakened`-Helfer.**

### 2.3 WHM-DoT-Guard prüfte das Ziel des vorigen Casts

`BaseAction.Target` ist eine schlichte Auto-Property, die **ausschließlich in
`CanUse`** zugewiesen wird (`BaseAction.cs:257`). Der Guard las
`DiaPvE.Target.Target` **vor** dem ersten `CanUse` des Frames und prüfte damit
das Ziel des letzten erfolgreichen Casts. Die Absicht — keine Aggro durch
DoT-Refresh auf einen Gegner, der schon auf einem drauf ist — trägt ohnehin
nicht: der Gegner greift bereits an. **Auf Upstream-Form zurückgebaut.**

### 2.4 Ein Upstream-Feature war gelöscht

Upstreams WHM castet zwischen 5 s und 3 s auf dem Countdown einen Pre-Pull-Regen
**und Divine Benison** auf den Tank. Der Fork hatte das entfernt, mit der
Begründung, Dungeons hätten keinen Countdown. Das stimmt für Dungeons — aber
Trials und Raids haben einen, und dort tat der Zweig genau das Richtige. Der
GeneralGCD-Filler deckt den Dungeon-Fall ab; beides ergänzt sich.
**Wiederhergestellt, `CountDownAction` ist wieder deckungsgleich.**

### 2.5 Nebenbefunde derselben Art

- Ein `[WSH 16/18]`-Marker im Fenstertitel täuschte eine Versionierung vor und
  wurde nie aktualisiert. Entfernt; `RotationSolverPlugin.cs` ist wieder
  identisch mit Upstream.
- Der Befehlspfad (`IBaseAction.ForceEnable = true`) hatte
  `skipStatusProvideCheck: true` verloren. Das ist Upstreams bewusste
  Entscheidung für eine **ausdrücklich befohlene** Aktion. Zurückgenommen —
  zunächst nur im oGCD-Pfad; der GCD-Zwilling folgte im zweiten Durchgang
  (§6).
- Zwei Kommentare behaupteten Entfernungen (SGE-, AST-Countdown), die es nie
  gab. Entfernt.
- Zwei erfundene Konstanten ersetzt: der Co-Tank-Notfallwert durch
  `Service.Config.HealthForDyingTanks`, das der Nutzer ohnehin einstellt; die
  unerklärte 1-Yalm-Zugabe auf die Gap-Closer-Reichweite ersatzlos.

---

## 3 · Was Fehlerbehebungen **in** Upstream-Code sind

Diese Änderungen beheben Fehler, die im Original stehen. Sie sind der Grund,
warum dieser Fork überhaupt existiert.

| Datei | Fehler im Original | Wirkung |
|---|---|---|
| `ActionTargetInfo.cs:88` | `continue` in einer inneren `for`-Schleife statt in der äußeren `foreach` — der Block war wirkungslos | Ziele auf der Restricted-DoT-Sperrliste wurden nicht übersprungen. Die **korrekte** Fassung desselben Guards steht 80 Zeilen tiefer in derselben Datei |
| `DataCenter.AverageTTK` | Fallback `0f`, wenn noch kein Ziel eine Schätzung hat | Jeder TTK-Verbraucher las „Kampf endet sofort" und blockierte u. a. Auto-Heilung für die ersten ~2,5 s jedes Pulls |
| 9 × `base.X`-Aufrufe | Overrides riefen eine **andere** Basismethode (z. B. `DefenseSingleGCD` → `base.DefenseAreaGCD`) | Dispatch-Kette lief still an falscher Stelle weiter. Kompiliert, im Diff unsichtbar |
| `RDM_Reborn` Impact | `!ImpactPvE.EnoughLevel && ImpactPvE.CanUse` | Bedingung nie wahr, Zweig tot |
| `BLM_Default` Thunder | Refresh-Gate listete `HighThunder_3872` nicht | Ab Sync 92 wurde ein frischer AoE-DoT bei jedem Cast abgeschnitten |
| `MoveBackAbility` | Methode im `if`-Kopf **und** im Rumpf aufgerufen | Doppelaufruf, falsche Reihenfolge gegenüber der Duty-Rotation |
| `MyInterruptAbility` / `AntiKnockback` | Rollen-Default lief **vor** dem Job-Override | RPR/VPR gaben Leg Sweep bzw. Arm's Length wegen ihres Combo-Gates ab, der Default nahm sie ungegatet trotzdem |
| `PhantomDefault` | Occult Ether/Potion mit `out _` statt `out act` | Aktion wurde erkannt, aber nie zurückgegeben |
| 4 × `BMR*Within` | prüften `Service.Config.UseBmrTimeline` nicht | Reagierten, obwohl der Nutzer die BMR-Zeitleiste abgeschaltet hatte |

Zwei dieser Klassen sind zusätzlich per CI ausgeschlossen, damit sie nicht
wiederkehren (`.github/scripts/check_base_calls.py`): falsches `base.`-Ziel,
widersprüchliches Level-Prädikat, und wirkungslose Guard-Schleife. Der
Wächter läuft in unter zehn Sekunden und ist gegen den jeweils behobenen Fall
validiert — er meldet ihn vor dem Fix und schweigt danach.

---

## 4 · Was Erweiterungen sind, und woran sie hängen

Alle Erweiterungen bauen auf **vorhandenen** Strukturen auf statt neue
einzuziehen: keine neue Vererbungsebene, keine neuen Dispatch-Slots, keine
Änderung an generierten Artefakten.

| Erweiterung | Umfang | Aufhängung |
|---|---|---|
| `BMRShouldRefreshBefore` — Status vor einem vorhergesagten Ereignis erneuern | 1 Helfer, 13 Aufrufstellen | Alle 13 Horizonte gegen die Wirkdauer aus den Spieldaten geprüft, kein einziger Abweichler (s. `AUDIT_LOG.md`) |
| `ShouldSustainMitigationDebuff` — Addle/Feint/Reprisal aufrechterhalten | 1 Helfer statt 25 Kopien der Bedingung | `MitigationDebuffDuration` folgt dem Enhanced-Trait auf Stufe 98 |
| `TankApproachingMobGroup` — Tank-Sustain im Wall-to-Wall | 1 Helfer, 2 Heiler | Im Spiel vom Nutzer verifiziert; Schwellen als Job-Config, nicht fest verdrahtet |
| Schild auf Effective-HP anrechnen | 2 Properties in `StateUpdater` | Nur wenn ein Grund für erwarteten Schaden vorliegt, sonst zählt ein frischer Schild ewig |
| DPS-Selbstschutz (Second Wind / Bloodbath) | je 3–4 Zeilen bei 7 Jobs | Füllt `HealSingleAbility`/`DefenseSingleAbility`, die dort leer waren |
| Bewegungsslots GNB · WHM · BRD | je 8 Zeilen | Laufen nur unter `AutoStatus.MoveForward`/`MoveBack`, können die Schadensrotation nicht erreichen |
| `SwiftRaisePending` | 1 Property je Heiler | Ersetzt 13 wortgleiche Kopien derselben Bedingung |

---

## 5 · Was offen bleibt

- **Release-Tag.** Der Fork hatte **0 Tags**, Upstream 952; `publish.yaml`
  triggert ausschließlich auf Tags `*.*.*.*`, ohne Tag ist jede Version
  `1.0.0.0`. Das Schema `<upstream>+wsh<n>` ist umgesetzt (`publish.yaml`
  spaltet am `+`, das Fenster zeigt die Informationsversion); der erste Tag
  `7.5.5.41+wsh1` ist noch zu setzen — aus der Arbeitsumgebung heraus nicht
  möglich (403 auf Tag-Push).
- **Spielfragen**, die aus dem Code nicht entscheidbar sind: Reprisal-Platzierung
  bei den vier Tanks, Slot-Asymmetrien der phys. Fernkämpfer, MNKs Heilslot,
  DRGs Trait-Gates. Alle in `TODO.md` einzeln geführt, keiner ungeprüft
  angeglichen.
- **`_lastHp`** in `DataCenter.GetPartyMemberHPRatio` ist toter Code — im
  Original wie hier. Nicht angefasst, dokumentiert.

---

## 6 · Zweiter Durchgang: Review-Loop über den gesamten Diff

Nach dem Audit oben wurde der komplette Diff noch einmal Hunk für Hunk
gelesen, mit anderen Mustern: Mehrspieler-Interaktion (zwei Tanks, zwei
Melees, zwei Caster in einer Gruppe), Randbedingungen (Null-Ziel,
Unverwundbarkeit, Level-Sync), Duplikate, tote Symbole, Diff-Rauschen.
Ergebnis: zwölf Commits — fünf Verhaltensfehler im Fork-Code, zwei im
Upstream-Code, der Rest Hygiene.

| Commit | Fund | Art |
|---|---|---|
| `5bb4d39f` | Der Gegnerzahl-Zweig des Sustain-Helfers ignorierte den Zielstatus; mit `skipStatusProvideCheck` überschrieb der zweite Tank/Melee/Caster den laufenden Reprisal/Feint/Addle | Fehler, Fork |
| `f107eda9` | Reprisal hatte kein `TargetStatusProvide`, die reaktive Zeile feuerte trotz Co-Tank-Reprisal. Zwei Status-IDs gleichen Namens (753/1193); welche die Aktion setzt, ist offline nicht entscheidbar — beide werden geprüft | Fehler, Upstream |
| `451d9e90` | Der Co-Tank-Provoke zog den Boss von einem Tank unter Superbolide / Living Dead / Holmgang | Fehler, Fork |
| `5b778336` | Der GCD-Befehlspfad hatte `skipStatusProvideCheck` weiter verloren; nur der oGCD-Zwilling war zurückgesetzt | Fehler, Fork |
| `28c0e1fc` | NIN fehlte das `HasHostileCountAoeMitigation`-Flag, das alle anderen Sustain-Jobs setzen | Fehler, Fork |
| `990daaeb` | `ShieldStatus` kannte weder Divine Benison noch The Blackest Night — die Schild-Anrechnung war für die häufigsten Tank-Schilde blind | Lücke, Fork |
| `c1d0ba45` | `foreach` in `CalculateDamageFactor` ohne jede Wirkung (seit 0246bea5) | toter Code, Upstream |
| `2df7dc4e` | `TargetType.SafeDotTarget` ohne Aufrufer | toter Code, Fork |
| `bd65f0d4` | UTF-8-BOM in elf Dateien, die Upstream ohne BOM führt | Rauschen |
| `3b5e50d5`, `bfc52584` | doppelter Slot-Konflikt-Block (MCH), dreifach ausgeschriebene BMR-Bedingung, Einzeiler-Wrapper | Duplikate |
| `ff0d8d43` | Prüfskript: `foreach`, klammerloses `continue`, Expression-Bodied-Overrides | CI |

Ein weiterer Durchgang mit denselben Mustern über den bereinigten Diff fand
nichts mehr: jedes neu deklarierte Symbol wird referenziert, jede der 38
`skipStatusProvideCheck: true`-Stellen steht hinter einer statusprüfenden
Bedingung, jede Spiegel-Behauptung in einem Kommentar wurde gegen den
referenzierten Code geprüft.

---

## 7 · Wie man das nachprüft

```
git remote add upstream https://github.com/FFXIV-CombatReborn/RotationSolverReborn.git
git fetch upstream
git diff upstream/main -- '*.cs'          # der gesamte Produktivcode-Unterschied
python3 .github/scripts/check_base_calls.py
```

Der Build läuft in der CI dieses Forks gegen dieselbe Dalamud-Version wie im
Original (`.github/workflows/build.yaml`, unverändert bis auf einen
zusätzlichen Job).
