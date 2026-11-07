# 20 · Ordnung der Konzepte

**Leitfrage:** Wo steht eine Regel, damit jeder, der sie für seinen Job, seine Rolle oder seinen Inhalt sucht, sie
findet – und sie nur einmal steht?

Sein Auftrag (05.11.2026): „generelle prüfung aller konzepte, ob eine neuorganisation nicht sinnvoll wäre: allgemein für
alle, dann nach gruppierungen heiler, tank, melee (drei sorten = monk+samurai, schnitter+dragoon, ninja+viper),
fernkämpfer, magier. dann weiter unterteilt auf jede einzelne klasse und auch die sonderklassen wie bluemage und
beastmaster. parallele konzepte für dungeons, endgegner, savage, occult crescent. immer da, wo bedarf besteht und mit
querverweisen zu den anderen betroffenen konzepten." Dazu zwei Vorgaben, die die Ordnung schon festlegen:
„Universell zuerst" (26.09.2026, für die Konzepte sein Hinweis vom 05.11.2026, CLAUDE.md) – eine Regel steht auf der
höchsten Stufe, auf der sie für alle Mitglieder gleich gilt, eine leere Stufe wird begründet – und „ein Gegenstand gehört
in das bestehende Konzept, das ihn trägt; ein neues Dokument nur, wenn keines ihn trägt; Länge ist kein Grund"
(01.10.2026, `.claude/rules/konzepte.md`).

## Die Ordnung

Zwei Achsen, und jede Regel steht an genau einer Stelle:

- **Ebene, für wen sie gilt** – die Stufen seiner Vorgabe:
  - *alle Jobs:* `03-universal.md`, dazu die Querschnittskonzepte der Kampfantwort (`07` Heilziel, `08` Antwort auf
    einen Treffer, `09` Schutz des Tanks, `11` Wiederbelebung, `13` Größe einer Fläche). Sie sind nach ihrer Frage
    gebaut, nicht nach der Rolle, weil eine Antwort auf einen Treffer Heiler, Tank und Damage Dealer in einer Kette
    verbindet; nach Rollen geteilt stünde dieselbe Kette in drei Dateien.
  - *Rolle:* `02-groups.md` – Heiler, Tanks, Damage Dealer als Gesamtheit, darunter Nahkämpfer, physische Fernkämpfer,
    Magier.
  - *Nahkampf-Paare* (Mönch und Samurai, Dragoon und Schnitter, Ninja und Viper): Die Stufe bleibt leer. Keine Regel
    gilt heute für genau eines dieser Paare; die Unterschiede in `02` laufen quer zu ihnen (der Countdown fehlt Dragoon
    und Viper, `EmergencyAbility` Schnitter und Samurai, ein eigenes Unterbrechungstor haben Schnitter und Viper;
    Erhebung 07.11.2026). Entsteht eine solche Regel, bekommt `02` den Abschnitt des Paars.
  - *Job:* `01-jobs.md`, je Job ein Abschnitt, die begrenzten Jobs eingeschlossen; Sonderjobs mit eigenem Regelwerk
    (Beastmaster, Blaumagier) bleiben dort. Was ein einzelner Job mit einer allgemeinen Regel tut, steht beim Job und
    verweist auf sie.
- **Inhalt, wo sie gilt** – parallel zu den Ebenen: Eine Regel, die nur in einem Inhalt greift, steht im Konzept dieses
  Inhalts, wenn der Inhalt ein eigenes Regelwerk trägt; sonst an ihrer Ebene, und der Index führt sie unter dem Inhalt.
  - *Eigenes Konzept:* das Crucible (`21-crucible.md`, nur Beastmaster), das Masked Carnivale (`22-masked-carnivale.md`,
    nur Blaumagier), die Tiefen Gewölbe (`23-deep-dungeons.md`, alle Jobs). Jedes trägt Bretter, Stufen oder Böden,
    Gegenstände und Fenster, die es nur dort gibt, auf hunderten Zeilen.
  - *Nur im Index:* Dungeons, Endgegner, Savage und High-End, Unreal, Occult Crescent, Eureka und Bozja. Ihre Regeln
    sind Lagen einer allgemeinen Regel (der Pull in `09`, die Tinktur in High-End-Inhalten in `03`, Phantom-Aktionen in
    `03`, `07`, `08`, `11`, `13`); aus ihnen ein eigenes Konzept zu bauen, hieße die Regel zu teilen oder zu kopieren.
    Der Index (`README.md`, „Inhalte") nennt je Inhalt die Abschnitte.
- **Übrige Konzepte** nach ihrer Frage, ohne Ebene: `04` (Umbau), `05` und `14` (Aktionsabdeckung, Abhängigkeiten),
  `06` (Archiv), `10` (Barriere des Dunkelritters), `12` (Searing Light), `15` und `16` (fremde Werkzeuge), `17`
  (Maschinist), `18` (Einstellungen), `19` (Audit), `20` (diese Ordnung).

## Der Bestand vor der Ordnung

Erhoben am 07.11.2026:

- 19 Konzepte, 18.345 Zeilen. `01-jobs.md` allein 3.179: Blaumagier 492, Beastmaster 2.246, davon das Crucible 1.062.
  `03-universal.md` 1.607, davon die Tiefen Gewölbe 158.
- Inhaltsthemen über die Konzepte verteilt (Treffer je Datei): Crucible 01 (70) und 03 (10), Masked Carnivale 01 (11),
  Tiefe Gewölbe 03 (8), Occult Crescent in acht Konzepten (je 1 bis 3), Savage in sieben, Dungeon in zwölf.
- Verweise: 511 Code-Kommentare der Form „concept NN", davon rund 30 auf Abschnitte, die umziehen (Crucible, Carnivale,
  Tiefe Gewölbe); 704 Nennungen im Archiv; 25 in den Ausnahmen fester Werte. Die Prüfskripte lesen
  `docs/rotation-flow/*.md` flach.

## Wirkung im Kampf

Mittelbar. Eine Regel, die im Abschnitt eines Jobs steht, sucht niemand für den nächsten Job; sie fehlt ihm, bis der
Fall dort auffällt. Beispiele, an denen die Regel zu spät oder doppelt stand: der Fernkampf-Rückfall beim Annähern
(A605, für fünf Nahkämpfer, steht in `03` C4), die Tinktur außerhalb des Kampfes (A433, bei Upstream in drei Rotationen,
zentral in `03`), die Zielart ohne Tank und Heiler (A541, für Beastmaster und Blaumagier, steht in `03`). Die Ordnung
nach Ebenen besteht also schon für die meisten Regeln; was fehlt, sind die Inhalte als eigene Achse und ein Index,
der beide Achsen zeigt.

## Warum diese Ordnung

**Erwogen:**

- **0 – nichts:** Die Konzepte bleiben, wie sie sind.
- **A – volle Hierarchie in Verzeichnissen:** je Ebene ein Verzeichnis (alle, Rollen, Paare, Jobs, Inhalte), jede Regel
  aus `07` bis `13` an ihre Ebene verschoben, jeder Job eine Datei.
- **B – Inhalte als eigene Konzepte, Index mit beiden Achsen:** Die drei Inhalte mit eigenem Regelwerk ziehen aus `01`
  und `03` in eigene Konzepte; der Index zeigt Ebenen und Inhalte; die Paar-Stufe bleibt begründet leer; Code-Verweise
  auf die umgezogenen Abschnitte werden umgeschrieben, und eine Prüfung hält jeden Code-Verweis „concept NN, "Titel"" an
  einer Überschrift der Datei NN.
- **C – nur der Index:** wie B, ohne Umzug.

**Im Kampf und in der Pflege gegeneinander gehalten:**

- **0:** Stärke – kein Aufwand, kein Verweis veraltet. Schwäche – das Crucible bleibt als Drittel des Job-Konzepts unter
  dem Beastmaster; wer eine Regel eines Inhalts sucht (Dungeon, Occult Crescent), öffnet bis zu zwölf Dateien.
- **A:** Stärke – jede Ebene hat ihren Ort. Schwäche – die Ketten der Kampfantwort (`08` verbindet Heilung, Barriere,
  Minderung über alle Rollen) zerfielen in Rollendateien, dieselbe Kette an drei Stellen, gegen die Vorgabe vom
  01.10.2026; die Prüfskripte lesen keine Unterverzeichnisse; 511 Code-Verweise und alle Abschnittstitel müssten
  umziehen. Bedrohung – jeder Verweis, der beim Umzug übersehen wird, zeigt auf einen Abschnitt, den es dort nicht mehr
  gibt, und nichts schlägt an.
- **B:** Stärke – die drei Inhalte, die ein eigenes Regelwerk tragen, stehen für sich und werden unter ihrem Namen
  gefunden; der Beastmaster-Abschnitt trägt nur noch den Job; Inhalte ohne eigenes Regelwerk stehen im Index statt in
  einer Kopie. Schwäche – rund 30 Code-Verweise ziehen um; zwei Dateien (Job und Inhalt) statt einer für den
  Beastmaster im Crucible. Chance – die neue Prüfung der Code-Verweise findet auch heute schon veraltete Titel.
  Bedrohung – ein Umzug, der einen Abschnitt halb mitnimmt, lässt eine Regel an zwei Stellen.
- **C:** Stärke – kein Umzug. Schwäche – das Crucible bleibt im Job-Konzept, gegen seinen Auftrag „parallele konzepte",
  und der Index allein trägt die Last.

*Ergebnis:* **B.**

## Gegenthesen

- *1 – Kein Bedarf (Länge ist kein Grund, 01.10.2026):* Hielte, wenn der Umzug nur die Länge kürzte. Das Crucible ist
  aber ein eigener Gegenstand – Bretter, Wege, Läden, Schätze, Lager, Gegenstände, Endkampf –, den es nur in diesem
  Inhalt gibt; ebenso die Stufen des Carnivale und die Böden der Gewölbe. Sein Auftrag vom 05.11.2026 nennt Inhalte als
  parallele Konzepte. Widerlegt für diese drei; für Inhalte ohne eigenes Regelwerk hält sie, deshalb dort nur der
  Index.
- *2 – Option falsch, A ist richtig:* Die volle Hierarchie folgt dem Wortlaut seines Auftrags. Sie bricht aber die
  Kampfantwort in Rollendateien und kopiert die Kette; sein Auftrag sagt „immer da, wo bedarf besteht". Widerlegt.
- *3 – Ausgeliefert, und nichts ändert sich:* Im Kampf ändert der Umzug nichts unmittelbar; er ändert, wo die nächste
  Regel geschrieben und gesucht wird. Messbar ist das am Index und an der Prüfung der Verweise; ein falscher Verweis
  fällt in der CI auf. Für den Auftrag – Regeln auffindbar machen – nicht widerlegt; der Preis ist ein Umzug.

## Plan

1. `21-crucible.md` aus `01` (Beastmaster, ab „Crucible of the Unbroken"), `22-masked-carnivale.md` aus `01`
   (Blaumagier, „Masked Carnivale", „Satz je Stufe aus den Stufenführern", „Schwäche und Immunität der Stufe"),
   `23-deep-dungeons.md` aus `03` (die drei Abschnitte „Tiefe Gewölbe"). Text unverändert, davor ein Kopf mit Leitfrage und
   dem Verweis auf die Ebene, auf der er aufbaut; an der alten Stelle ein Satz mit dem Verweis.
2. Index: Ebenen-Tabelle und Inhalte-Tabelle in `README.md`.
3. Code-Verweise auf die umgezogenen Abschnitte umschreiben; neue Prüfung `check_concept_refs.py`: Jeder Code-Kommentar
   „concept NN, "Titel"" nennt eine Überschrift oder einen fetten Absatztitel der Datei NN; Selbsttest gegen einen
   konstruierten falschen Verweis.
4. Archiv bleibt, wie es ist: Es ist die Geschichte; `archive.py` findet die Konzepte eines Eintrags durch Suche in
   allen Dateien, auch in den neuen.
5. TODO-Einträge, deren *Konzept:* auf einen umgezogenen Abschnitt zeigt, auf die neue Datei.
