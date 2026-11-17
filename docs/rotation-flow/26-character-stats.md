# 26 · Charakterwerte: Formeln, Stufen und Materia

**Leitfrage:** Welcher Charakterwert bringt wann welchen Bonus, ab welchem Wert tritt erst wieder eine Verbesserung ein, und
welche Materia gehört für den meisten Schaden in ein angelegtes Ausrüstungsset?

**Sein Auftrag (17.11.2026), als Nebenprojekt:** „genaue formeln ermitteln, welche characterwerte wann welche bonis bringen.
anscheinend gibt es auch eine quantifizierung bei den werten mit sprüngen, ab welchem wert erst wieder eine verbesserung
eintritt. das könnte grundlage sein für empfehlungen, welche materia man für optimalen schaden in angelegte ausrüstungssets
zu stehen wäre. hierzu recherche im netz, bei plugins und daraus ein konzept erstellen.“ Dazu seine Hinweise derselben Folge:
Jede Klasse kann eigene Ausrüstung und Materia tragen, also eigenes Tempo; die Ausrüstung ändert sich im Kampf nicht;
Essen ist ein Buff mit Bestand, Effekt und Dauer (Konzept 03, „Die eigene Geschwindigkeit“). Der Auftrag ist ein Konzept;
gebaut wird erst auf seinen Auftrag.

## Quellen

Je Quelle ihr Status; keine ist Spieldokumentation des Herstellers.

- *Spielblätter* (xivapi v2, 17.11.2026, Spielstand a72e645591e5bf8b): `BaseParam` (Namen und Beschreibung je Wert, je
  Ausrüstungsplatz der Anteil an der Grenze, `MeldParam`), `ParamGrow` (Stufe 100: `BaseSpeed` 420, `LevelModifier` 2780),
  `Materia` (Wert je Grad), `ItemFood`, `ItemLevel`, `Item`.
- *akhmorning, Allagan Studies* (Stand 7.0, Seiten „Stats“, „Critical Hit“, „Direct Hit“, „Determination“, „Skill & Spell
  Speed“, „Tenacity“, „Piety“, „Defense“; gelesen 17.11.2026): die Formeln auf Stufe 100 und die Stufentabellen.
  Community-Forschung; für den Hauptwert steht dort keine Formel („notorious for being the most difficult stat to resolve
  accurately“).
- *Character Panel Refined* (Kouzukii/ffxiv-characterstatus-refined, 0e4b68c vom 02.05.2026, MIT): Dalamud-Plugin, zeigt im
  Charakterfenster je Wert den vorigen und nächsten Stufenwert; Stufenwerte je Stufe 1 bis 100 als Tabelle (Main, Sub,
  Div), Formeln wie akhmorning, Grenzen je Ausrüstungsplatz aus den Blättern erzeugt. Werkzeugquelle, nur gelesen.
- *xivgear.app, Gear Planner* (xiv-gear-planner/gear-planner, 0f457ce vom 14.11.2026; keine Lizenzdatei gefunden, also nur
  gelesen): Formelbibliothek `xivmath.ts` (dieselben Formeln, dazu Hauptwert, Waffenschaden, Hast, Grenze je Gegenstand),
  ein Materia-Löser (`meldsolver.ts`), der Meld-Kombinationen aufzählt, nach Tempo bündelt und je Bündel mit einer
  Rotationssimulation des Jobs bewertet. Werkzeugquelle.
- *consolegameswiki, „Materia“* (gelesen 17.11.2026): Werte je Grad, Regeln der Plätze; „This maximum is handled separately
  for each substat“.

## Die Werte und ihre Formeln

Stufe 100; ⌊x⌋ abrunden. `Sub` = 420, `Div` = 2780 (beide `ParamGrow`), `Main` = 440 (nur Werkzeugquellen und die
Formeln von akhmorning; in keinem Blatt gefunden – unbelegt am Spiel, durch die Formel der Entschlossenheit gegenprüfbar,
siehe „Prüfweg“). Auf anderen Stufen dieselben Formeln mit den Stufenwerten der jeweiligen Stufe – wichtig bei
Stufensynchronisation.

| Wert (BaseParam) | Wirkung | Formel | Punkte je Stufe |
|---|---|---|---|
| Kritischer Treffer (27) | Chance | (⌊200 · (W − Sub) / Div⌋ + 50) / 1000 | Div/200 = 13,9 |
| Kritischer Treffer (27) | Schaden eines kritischen Treffers | (⌊200 · (W − Sub) / Div⌋ + 1400) / 1000 | 13,9 (dieselbe Stufe) |
| Direkter Treffer (22) | Chance, +25 % Schaden | ⌊550 · (W − Sub) / Div⌋ / 1000 | Div/550 = 5,05 |
| Entschlossenheit (44) | Schaden und Heilung | (1000 + ⌊140 · (W − Main) / Div⌋) / 1000 | Div/140 = 19,9 |
| Unbeugsamkeit (19) | Schaden und Heilung (Tanks) | (1000 + ⌊112 · (W − Sub) / Div⌋) / 1000 | Div/112 = 24,8 |
| Unbeugsamkeit (19) | erlittener Schaden (Tanks) | (1000 − ⌊200 · (W − Sub) / Div⌋) / 1000 | 13,9 |
| Schnelligkeit (45), Zaubertempo (46) | GCD, Wirkzeit | ⌊GCD₀ · (1000 + ⌈130 · (Sub − W) / Div⌉) / 10000⌋ / 100 | 21,4 je Tempostufe |
| Schnelligkeit, Zaubertempo | Stärke von DoT, HoT, Autoangriff | (1000 + ⌊130 · (W − Sub) / Div⌋) / 1000 | 21,4 |
| Frömmigkeit (6) | MP je Tick (Heiler) | 200 + ⌊150 · (W − Main) / Div⌋ | Div/150 = 18,5 |
| Verteidigung (21), Magieabwehr (24) | erlittener Schaden | (100 − ⌊15 · W / Div⌋) / 100 | Div/15 = 185 |
| Konstitution (3) | LP | Faktor je Stufe und Rolle (Werkzeugtabelle) | stetig |
| Hauptwert (1, 2, 4, 5) | Schaden | (⌊Faktor · (W − Main) / Main⌋ + 100) / 100, Faktor je Stufe und Tank/Nicht-Tank | stetig, Faktor unbelegt am Spiel |

**Die Sprünge.** Jede Formel rundet ab; ein Wert zwischen zwei Stufen bringt nichts, bis er die nächste erreicht. Die
nächste Stufe eines Werts W mit Faktor k und Bezug B (Sub oder Main) liegt bei ⌈(⌊k · (W − B) / Div⌋ + 1) · Div / k + B⌉;
Punkte darüber bis dahin sind ohne Wirkung. Der GCD springt gröber als seine Tempostufe: Bei 2,5 s ändert sich die angezeigte
Länge um 0,01 s etwa alle vier Tempostufen (akhmorning). Kleine Stufen (Direkter Treffer, 5 Punkte) verschwenden fast nichts,
große (Entschlossenheit 20, Unbeugsamkeit 25, Tempo 21 und GCD rund 86) viel.

**Materia** (Blatt `Materia`; consolegameswiki): je Grad +1, +2, +3, +4, +6, +16, +8, +24, +12, +36, +18, +54 (I bis XII)
für jeden Kampfwert. Grade VI, VIII, X und XII nur in die festen Plätze und den ersten zusätzlichen Platz, in die weiteren
zusätzlichen Plätze die Grade darunter (XI = +18). Je Gegenstand und Wert gilt eine Grenze; was darüber hinausgeht, wirkt
nicht. Die Grenze rechnet `xivgear` aus den Blättern: ⌊⌊`ItemLevel`[Wert] · `BaseParam.MeldParam`[`Item.BaseParamModifier`]
/ 100⌋ · Anteil des Platzes (`BaseParam`, etwa Kopf 85, Brust 135, Ring 67, Zweihandwaffe 140) / 100⌋ / 10, gerundet. Der
größte Wert eines Gegenstands steht schon an dieser Grenze (Character Panel Refined erzeugt die Grenze je Platz und
Gegenstandsstufe aus eben diesem größten Wert); Materia darin wirkt nicht.

**Essen** erhöht einen Wert um min(Obergrenze, ⌊Wert · Anteil⌋) (`ItemFood`; Konzept 03; Character Panel Refined rechnet den
Wert ohne Essen genau so zurück). Solange Wert · Anteil unter der Obergrenze bleibt, bringt jeder Materiapunkt in diesem Wert
den Anteil des Essens obendrauf; darüber bringt das Essen nichts mehr dazu. Die Empfehlung rechnet das Essen mit, das gerade
wirkt.

## Erst was einsetzbar ist, dann das Optimum

Seine Präzisierung (17.11.2026): „man müsste für das set das optimum herausfinden und dann schauen, was wo einsetzbar ist.
bzw. umgekehrt. welche felder erlauben, wo welche materia einzusetzen, ob overmelding überhaupt bei dem gegenstand geht, so
dass man erst einmal sieht, was man überhaupt einsetzen könnte, und daraus halt die optimale kombination zu ermitteln“.

**Schritt 1 – je angelegtem Gegenstand, was einsetzbar ist** (alles aus Spiel und Blättern, nichts angenommen):
- *Plätze:* `Item.MateriaSlotCount` feste Plätze; `Item.IsAdvancedMeldingPermitted`, ob zusätzliche Plätze gehen; höchstens
  fünf Materia je Gegenstand (`InventoryItem` führt fünf Felder `Materia` und `MateriaGrades`). Beispiel (Blatt, 17.11.2026):
  die Raid-Helme der Stufe 730 („Augmented Quetzalli“) haben 2 feste Plätze, kein Overmelding.
- *Grade je Platz:* feste Plätze und der erste zusätzliche jeden Grad bis XII (+54), die weiteren zusätzlichen höchstens XI
  (+18) (consolegameswiki). Ein zusätzlicher Platz kann misslingen; die Chance hängt an Platz, Grad und Qualität (dort 17, 10,
  7, 5 % ab Grad V bei HQ) – für das Optimum ohne Belang, für den Preis nicht: Die Empfehlung nennt sie.
- *Raum je Wert:* Grenze des Gegenstands für den Wert aus dem Spiel (`InventoryItem.GetParameterMaxValue(BaseParam, Item)`,
  FFXIVClientStructs) minus sein Wert ohne Materia (`GetParameterValue(…, includeMateria: false, …)`). Die Formel aus Gear
  Planner bleibt Gegenprobe. Im Beispiel: Kritisch 236 an der Grenze 236, also kein Raum; Direkt 165, Raum 71; jeder Wert,
  den der Helm nicht trägt, Raum 236.
- *Belegung:* welche Materia schon in welchem Platz sitzt (`Materia`, `MateriaGrades`), damit die Empfehlung den Umbau nennt
  (Herauslösen kann die Materia zerstören; Quelle des Satzes: consolegameswiki, nicht geprüft).
- *Bestand an Materia:* wie viele Steine jeder Art und jedes Grades er im Inventar hat; die Empfehlung rechnet wahlweise
  nur mit vorhandenen oder nennt, was fehlt.

Daraus die Menge der möglichen Belegungen je Gegenstand: für jeden Platz ein Wert (oder leer) mit dem höchsten erlaubten
Grad, was über den Raum geht, verfällt. Ein niedrigerer Grad lohnt nur, wo er den Raum genauer füllt und ein anderer Platz
den Rest trägt; die Aufzählung prüft auch ihn.

**Schritt 2 – das Optimum darin:** über alle Gegenstände die erreichbaren Summen je Wert, bewertet wie unten, mit dem
laufenden Essen. **Umgekehrt** dient das freie Optimum – die beste Verteilung derselben Zahl Punkte ohne Plätze und Grenzen –
als obere Schranke: Es zeigt, was die Grenzen des Sets kosten, und kürzt die Suche (eine Teilbelegung, die selbst mit dem
freien Rest die beste gefundene nicht mehr erreicht, fällt weg). Ergebnis je Platz: Wert und Grad, ob vorhanden, ob
zusätzlicher Platz mit Chance; je Wert die Punkte bis zur nächsten Stufe; der Gewinn gegenüber den angelegten Melds.

## Was eine Empfehlung braucht

- *Bestand:* angelegte Ausrüstung (`InventoryType.EquippedItems`) mit Schritt 1; die Werte des Charakters
  (`PlayerState.Attributes`, Konzept 03) als Gegenprobe der Summe; Job und Stufe (Stufenwerte, Rolle für Unbeugsamkeit).
- *Bewertung:* der erwartete Schadensfaktor aus den Werten, die Materia bewegt: f(Entschlossenheit) · f(Unbeugsamkeit, nur
  Tanks) · (1 + Chance_krit · (Schaden_krit − 1)) · (1 + 0,25 · Chance_direkt), dazu das Tempo (unten). Hauptwert und
  Waffenschaden bewegt Materia auf Stufe 100 nicht; sie sind für den Vergleich zweier Melds ein gemeinsamer Faktor und fallen
  heraus – der unbelegte Faktor des Hauptwerts trägt die Empfehlung also nicht.
- *Tempo:* Mehr Tempo bringt GCDs je Zeit und stärkere DoTs und Autoangriffe, aber ob eine Tempostufe einem Job nützt, hängt
  an seiner Rotation (Buff-Fenster, die eine feste Zahl GCDs fassen; A873 für das Weben). Gear Planner beantwortet das mit
  einer Simulation je Job. Ohne Simulation bleibt die GCD-Stufe eine Vorgabe des Spielers – die heutige oder eine gewählte –,
  und jeder Tempopunkt über ihrer Schwelle zählt nur mit seiner DoT- und Autoangriffsstärke.
- *Lösung (Schritt 2):* Melds hängen voneinander ab (Grenzen je Gegenstand, Sprünge über das ganze Set); einzeln die beste
  Materia je Platz zu wählen verfehlt das Optimum, wo ein Punkt eine Stufe verfehlt. Die Belegungen aus Schritt 1 sind je
  Gegenstand wenige, über elf Gegenstände viele: Über die Gegenstände hinweg werden die erreichbaren Summen fortgeschrieben,
  dominierte verworfen, mit dem freien Optimum als Schranke; am Ende jede Summe mit dem Essen exakt bewertet.

## Wege zur Empfehlung

- *0 – nichts:* Er nutzt Character Panel Refined (Stufen) und xivgear.app (Melds). Stärke: keine Arbeit, xivgear simuliert
  jeden Job. Schwäche: zwei fremde Werkzeuge, xivgear außerhalb des Spiels, das angelegte Set muss er übertragen.
- *1 – Stufen anzeigen:* je Wert der nächste und vorige Stufenwert und die Punkte ohne Wirkung, aus dem angelegten Set. Stärke:
  klein, exakt, ohne Annahme über Rotationen. Schwäche: keine Empfehlung; deckt sich mit Character Panel Refined.
- *2 – Melds empfehlen ohne Simulation (empfohlen):* Bewertung wie oben bei fester GCD-Stufe, Lösung über die Summen.
  Stärke: im Spiel, aus dem angelegten Set und dem laufenden Essen, für jeden Job gleich (Ebene „alle“), Sprünge und Grenzen
  exakt. Schwäche: Tempo nur als gewählte Stufe; Jobs, deren Wert stark an der Tempostufe hängt, brauchen die Wahl des
  Spielers.
- *3 – Melds mit Simulation:* wie Gear Planner, je Job eine Rotationssimulation. Stärke: beantwortet auch das Tempo. Schwäche:
  eine Simulation je Job ist ein eigenes Werk und veraltet mit jedem Patch; RSR hat Rotationen, aber kein Zeitmodell, das
  Schaden über einen Kampf rechnet.
- *Antithese gegen 2:* Ein Optimierer ohne Simulation empfiehlt bei Jobs mit Tempostufe falsch. Entkräftet nur, wenn die
  GCD-Stufe fest bleibt: Die Empfehlung ändert dann nie das Tempo, sondern verteilt nur Kritisch, Direkt, Entschlossenheit
  und Unbeugsamkeit. Hält für den Fall, dass der Spieler eine andere Stufe will – dann wählt er sie, und die Empfehlung
  rechnet für diese.
- *Antithese gegen „Schaden zuerst“:* Seine Regel „Sicherheit der Gruppe vor Schaden“ (CLAUDE.md, „Spielweise“): Für Tanks
  mindert Unbeugsamkeit den erlittenen Schaden, für Heiler hält Frömmigkeit MP. Die Empfehlung rechnet deshalb für Tanks
  Unbeugsamkeit mit ihren beiden Wirkungen und nimmt bei Heilern eine Mindestfrömmigkeit als Vorgabe; die Gewichtung von
  Schutz gegen Schaden ist seine Entscheidung (Option mit Voreinstellung nach seiner Regel).
- *Antithese gegen 1 und 2:* Character Panel Refined zeigt die Stufen schon. Für 2 hält das nicht: Es empfiehlt keine Melds.

## Prüfweg

- Die Formeln gegen das Spiel: Das Charakterfenster zeigt Chance und Schaden kritischer Treffer, Chance direkter Treffer und
  den GCD; je Formel muss der Wert aus `PlayerState.Attributes` genau den angezeigten ergeben. Der Wert von `Main` lässt sich
  so an der Entschlossenheit prüfen (sie nutzt ihn), bevor eine Empfehlung auf ihm steht.
- Die Grenzen: `GetParameterMaxValue` des Spiels gegen die Formel aus Gear Planner je angelegtem Gegenstand; dazu die rote
  Färbung im Meld-Fenster (consolegameswiki).
- Die Lösung gegen eine vollständige Aufzählung an einem kleinen Set (zwei, drei Gegenstände), wo das Durchprobieren aller
  Melds geht.

## Offen

- `Main` auf Stufe 100 (440): in keinem Blatt gefunden; Werkzeugquellen und akhmorning stimmen überein. Bis zur Prüfung am
  Charakterfenster unbelegt am Spiel.
- Der Faktor des Hauptwerts je Stufe (Werkzeugtabelle „after some testing“): für die Empfehlung ohne Belang (fällt heraus),
  für jede Aussage über absoluten Schaden unbelegt.
- Ob Hast (BaseParam 47) einen Wert trägt, zeigt die Spur aus Konzept 03 (A872).
