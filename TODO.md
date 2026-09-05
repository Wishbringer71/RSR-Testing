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

## Offene Konzepte / Fixes (noch nicht umgesetzt)

### #54 WHM-Heilsuppression — Ursache gefunden und gefixt, Bestätigung im Spiel offen

Nutzer-Meldung (Klyteum, echte Mitspieler, einziger Heiler): oGCD-Heilung
zu Beginn ok, danach keine GCD-Heilung mehr trotz vollem Mana und Tank
unter 20 %, kein Castversuch; RSR castete stattdessen Holy. Gegner (>3)
unter 50 % HP.

Ursache (Commit `c6a0a40c`, Herleitung in AUDIT_LOG): `CanUseHealAction`
verlangte in Kampf `AverageTTK > AutoHealTimeToKill` (Default 8 s) auch
fuer Heiler, obwohl die Option in den Einstellungen unter
`UseHealWhenNotAHealer` haengt und „Stop healing when time to kill is lower
than" fuer Nicht-Heiler meint. `AverageTTK` ist ein Ratenschaetzer ueber
alle Gegner; sobald die meisten Mobs unter 50 % sind, faellt er unter 8 s
und saemtliche Heil-Flags gehen aus — genau das gemeldete Bild (kein
Castversuch, GeneralGCD/Holy erreicht, Schwellen und Mana irrelevant).

Offen ist nur die Bestaetigung: beim naechsten Wall-to-Wall pruefen, ob
die GCD-Heilung durchlaeuft, wenn die Mobs unter 50 % fallen. Wer die
alte Sperre fuer Heiler will, setzt sie nicht mehr ueber diese Option —
das ist bewusst so.

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

## Wichtig für zukünftige Sessions

Diese Dateien (TODO.md, AUDIT_LOG.md) existieren nur auf dem Branch, auf
dem sie committet wurden. Falls sie fehlen, obwohl an diesem Repo
gearbeitet wird: das dem Nutzer explizit melden (siehe CLAUDE.md), nicht
stillschweigend neu anfangen. Beide Dateien bei Sitzungsbeginn lesen —
nicht nur TODO.md.
