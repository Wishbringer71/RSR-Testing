# 08 · Synergie von Schadensvermeidung und Schadenserzeugung

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Stand dar; die
Prüfhistorie steht in `AUDIT_LOG.md` (A20).

## Ergebnis

**Zweck der ganzen Regelfamilie ist die Heilbarkeit des Tanks, und der Engpass ist die Gegnerzahl.**
Bei drei Gegnern traegt ein HoT; bei neun laeuft der Strom jeder Faehigkeit davon. Der eingehende
Schaden **einschliesslich Schadensreduktion und Mitigation** soll deshalb zu jedem Zeitpunkt
bestimmte Grenzwerte nicht ueberschreiten — und entscheidend ist nicht, wie **stark** gedrosselt
wird, sondern wie **lange**: Die Drosselung kauft die Zeit, in der der eigene Schaden die Gegnerzahl
senkt.

**Fuenf Vorgaben des Auftraggebers ordnen alles Weitere, und sie gelten zusammen:**

1. **Strecken statt stapeln.** Faellt alles zugleich, ist die Drosselung nach Sekunden verbraucht und
   der volle Strom trifft eine unveraendert grosse Gruppe.
2. **Heilung vor Minderung.** Wo der Strom zu gross wird, ist zuerst zu heilen. Gemindert wird, wo
   die Heilung nicht reicht.
3. **Die Barriere zaehlt im Zaehler, nicht im Nenner.** Sie drosselt nichts, bewertet aber, ob der
   Traeger ueberlebt und ob genug Zeit zum Heilen bleibt.
4. **Ausgesetzt wird nur bei Stunbarkeit.** Ist im Wirkbereich alles betaeubt oder immun, gibt es
   nichts zu strecken und nichts zu sparen; dann kostet das Aussetzen nur den Flaechenzauber.
5. **Das Mittel wird nach der Groesse des Treffers gewaehlt, nicht nach seiner Verfuegbarkeit.**
   Gesucht ist Deckung: Der Anteil des Mittels soll den Anteil des Treffers erreichen, nicht
   uebertreffen. Reicht die Gesundheit des schwaechsten Mitglieds nicht, geht Heilung vor; reicht
   auch die volle Gesundheit nicht, kommen Barriere und Minderung zusaetzlich. Ausgeschrieben im
   Abschnitt darunter.

**Gewaehlt ist eine zentrale Bremse bei dezentraler Ausloesung:** Die vorhandenen Ausloeser bleiben,
hinzu kommt eine Pruefung, die *zurueckhaelt*. Faellt sie aus, verhaelt sich RSR wie zuvor.

**Der Grenzwertanspruch ist heute zur Haelfte erfuellt, und der fehlende Teil ist seit Kurzem
messbar.** Gemessen wurde bisher allein die Gegnerseite; die persoenlichen Minderungen des Tanks
gehen in keine Rechnung ein. Die Luecke schliesst nicht eine Tabelle von Minderungssaetzen, sondern
die Beobachtung: Der Gesundheitsverlauf je Gruppenmitglied ist bereits netto und braucht keine Liste.
Die Schaetzung daraus ist **praeventiv** — bei 90 % Gesundheit meldet sie den Tod acht Sekunden im
Voraus — und sie **korrigiert sich selbst**, indem sie ihre eigene Vorhersage jede Sekunde gegen den
tatsaechlichen Verlauf haelt. Ein externer Beobachter ist dafuer nicht noetig. Bevor eine Kampfregel
darauf aufsetzt, ist der gemessene Fehlerfaktor in der Diagnoseanzeige zu beurteilen.

| Baustein | Stand |
|---|---|
| Messung: `SurveyStuns`, `SurveyHostileStatus`, `StatusHelper.StunStatus` und `SlowStatus` | umgesetzt in `CustomRotation_OtherInfo` und `StatusHelper` |
| Aussetzbedingung aus dem **Betaeubungsgrund**, hinter `StretchHolyStun` (Standard an) | umgesetzt (`WHM_Reborn.ShouldStretchHolyStun`) |
| Aussetzbedingung aus dem **Mitigationsgrund** — eine fremde Minderung traegt bereits | umgesetzt (`WHM_Reborn.ShouldHoldHolyWhilePackSlowed`, Standard an) |
| **Stunbarkeit** als Bedingung ueber allen drei Aussetzregeln | umgesetzt (`headroom` aus `SurveyStuns`; bei der Streckung laufende oder frische Betaeubung aus `WHM_Reborn.LongestStunInRadius`) |
| Aussetzbedingung als **Anteil** der verlangsamten Gegner, mit Mindestzahl | umgesetzt (`HoldHolyMinSlowedHostiles`, Standard 3) |
| **Schranke** der Aussetzregel: der Rest muss bewaeltigbar sein | umgesetzt und **gemessen statt gesetzt**: `AnyPartyMemberFallingWithinHealWindow`. Die frühere Zahl (`HoldHolyMaxHostileOutput` 600) war meine Setzung und ist zum optionalen Deckel mit Standard 0 = aus geworden |
| **Schadensrate je Gruppenmitglied**, netto nach allem | umgesetzt: die Gruppe steht in `RecordedHP`, `GetTTK` antwortet fuer sie |
| **Selbstkorrektur** der Schaetzung gegen ihren eigenen Fehler | umgesetzt (`ScoreTtkForecast`, `GetCorrectedTTK`); Rohzeit, korrigierte Zeit und Faktor stehen in der Diagnoseanzeige |
| **Vorausschau** als Ersatzgroesse an allen Heilentscheidungen | umgesetzt (`GetForecastSurvivingShare` und die drei davon abgeleiteten Getter), hinter `HealAheadOfDamage`, Standard an |
| Vorausschau auch in der **Flaechenheilung** (`PartyMembersAverHP` und Geschwister) | erfasst, nicht bearbeitet — siehe `TODO.md`; 83 Leser ausserhalb der Heilkette, darunter fremde Rotationen |
| Minderungen des Tanks **rechnerisch** erfassen (Vorausschau vor dem ersten Treffer) | offen, siehe `TODO.md` — braucht Saetze je Status aus `Action.resx` |
| **Derselbe Satz je Aktion traegt zwei Zwecke**, und das war bisher nicht gesehen: die Vorausschau in der Zeile darueber **und** die Wahl des Mittels nach Treffergroesse (Vorgabe 5). Wer ihn baut, loest beide Punkte | offen, siehe `TODO.md` |
| Restzeit der Barriere (`HasSurvivingShield` misst die kuerzeste statt der laengsten) | offen, siehe `TODO.md` |
| Erhebung der uebrigen Doppelnutzen-Aktionen | umgesetzt als `scan16.py`; ein Fund im Tank-/Heilerprofil (Rueckstoss) |
| Rueckstoss auch **als** Minderungswerkzeug wirken | offen, siehe `TODO.md` — Zielkonflikt mit der Rolle als einziger Rueckstossschutz |
| Wirksamkeitsmessung im Spiel | offen |
| **Sonden, die es schon gibt** — ohne sie ist im Kampf nicht zu sehen, ob eine Regel greift: `DataCenter.AreaMitigationSkipped` nennt je Aktions-Id, wo die Flächenbewertung eine Minderung verworfen hat; Rohzeit, korrigierte Zeit und Fehlerfaktor der Schätzung stehen in der Diagnoseanzeige | in Betrieb, in keinem Konzept genannt gewesen |

## Die Antwort auf einen eingehenden Treffer

**Dieser Abschnitt ist der Knoten zwischen vier Konzepten, und die Zustaendigkeiten sind getrennt:**

| Frage | Konzept |
|---|---|
| Wie gross ist der eingehende Treffer? | `13-aoe-damage-classification.md` — Messung je Aktion, Anteil am schwaechsten Mitglied |
| Welches Mittel antwortet darauf, und in welcher Reihenfolge? | **hier**, Vorgabe 5 und die Tabelle unten |
| Wer wird geheilt, wenn geheilt wird? | `07-heal-target-priority.md` — Gefaehrdung vor Rolle, Rolle vor Prozentsatz |
| Wann darf der Tank ein eigenes Mittel zuruecknehmen? | `09-tank-selfprotection.md` — lexikographische Rangordnung, Ueberleben zuerst |
| Wann ist die grosse Barriere das richtige Mittel? | `10-drk-blackest-night.md` — sie ist zugleich der Maszstab fuer „gross" |

**Heilung, Barriere und Minderung sind drei Antworten auf dieselbe Frage, und die Groesse des
Treffers entscheidet, welche davon richtig ist.** Bezugsgroesse ist das schwaechste Gruppenmitglied —
bei gleichem absolutem Schaden traegt der Spieler mit der geringsten Maximalgesundheit den hoechsten
Anteil, und genau diesen Anteil legt `13-aoe-damage-classification.md` je Aktion ab.

**Vorgabe des Auftraggebers, woertlich:** „wenn schaden nur 10% auf spieler mit geringster maxhp
verursacht, dann reicht ein schild, was 10% blockiert. oder sogar weniger bis kein schild. wenn ein
schaden 70% verursacht von maxhp des geringsten spielers, dann sollte das schild moeglichst hoch
sein, optimal 70%." Und die Ausnahme: „die aktuelle hp liegt unter dem schadenswert. dann waere aber
eine heilung sinnvoll bis max maxhp. wenn dann die hp unter dem schadenswert liegt, sollte
zusaetzlich geschildet werden. bzw. der schadensoutput reduziert."

| Lage des schwaechsten Mitglieds | Antwort |
|---|---|
| Treffer kleiner als die **aktuelle** Gesundheit | Deckung in Hoehe des Treffers; bei kleinen Werten auch gar keine |
| Treffer erreicht die aktuelle, bleibt unter der maximalen | **zuerst heilen**, Ziel ist die Maximalgesundheit — danach wieder Deckung |
| Treffer uebersteigt auch die maximale Gesundheit | Heilung allein rettet nicht: Barriere **und** Minderung zusaetzlich, bis der Rest darunter liegt |

**Das ist Vorgabe 2 dieses Konzepts, zu Ende gedacht.** „Heilung vor Minderung" sagte bisher nur die
Reihenfolge; die Tabelle sagt, **woran** sich entscheidet, ob der Fall ueberhaupt eintritt. Und sie
loest die dritte Vorgabe ein: Die Barriere steht im Zaehler, also addiert sie sich in Zeile drei zur
Minderung, statt mit ihr zu konkurrieren.

**Der Wert je Abwehraktion liegt vor.** `generate_defensive_values.py` liest ihn aus den Wirktexten
in `ActionId.resx` und `DutyAction.resx` und erzeugt `RotationSolver.Basic/Data/DefensiveValues.g.cs`;
die CI stellt die erzeugte Datei gegen die Wirktexte. Drei Formen, bewusst getrennt gehalten, weil
sie **nicht** dasselbe bedeuten:

| Form | Wirktext | Was sie im Kampf tut |
|---|---|---|
| Minderung am Traeger | „Reduces damage taken by 20%" (Rampart) | nimmt einen Anteil **des Treffers**, skaliert also mit ihm |
| Minderung am Gegner | „physical damage dealt by 5% and magic damage dealt by 10%" (Addle) | dasselbe ueber den Angreifer, und **je Schadensart verschieden** |
| Barriere | „absorbs damage totaling 20% of your maximum HP" (Schimmerschild) | absorbiert feste Punkte; gegen einen kleinen Treffer bleibt der Rest ungenutzt, gegen einen grossen ist sie aufgebraucht |

**Von Stufe 1 der Vorgabe — die Deckung nach Treffergroesse waehlen — ist nichts gebaut, und der
Grund ist ein Messergebnis, keine Kostenfrage.** Die Auswahl setzt voraus, dass ein Job mehrere
Mittel derselben Art zur Wahl hat. Gemessen an der erzeugten Tabelle trifft das nirgends zu:

- **Minderungen kennen kein „zu gross".** Sie nehmen einen Anteil des Treffers, es bleibt nichts
  uebrig, und 30 % eines kleinen Treffers sind klein. Die Vorgabe „ein Schild, das 10 % blockiert"
  ist eine Aussage ueber **Barrieren**.
- **Barrieren mit ausgeschriebenem Anteil gibt es wenige**, und kein Job haelt zwei davon zur Wahl:
  Krieger eine, Dunkelritter eine, Beschwoerer eine. Der Pictomancer haelt zwei, aber Tempera Grassa
  **entfernt** Tempera Coat („Removes Tempera Coat to create a barrier…") — eine Umwandlung, keine
  Alternative. Die uebrigen sind Bozja-Aktionen ausserhalb des Nutzungsprofils.

**Die Auswahlregel war gebaut und ist zurueckgebaut worden**, nachdem die Falsifikationsstufe das
ergeben hat (A118). Sie haette im ganzen Baum nie gegriffen.

**Was stattdessen wirkt, ist Stufe 2 — und sie schliesst eine Luecke, die dieses Konzept ohnehin
fuehrt.** Siehe „Heilung vor dem angekuendigten Treffer" weiter unten.

## Die Abwehrsperren (E1): allgemeine Schranke, Sonderregeln je Job

**Sachstand (A159), nach seiner Vorgabe „universell zuerst": Jede strategische Rückhaltung einer
Abwehraktion weicht, sobald die Gruppe in Gefahr ist. Das ist eine Regel für alle Jobs; welche
Rückhaltung ein Job überhaupt kennt, bleibt seine Sonderregel.** Gebaut in
`CustomRotation_DefenseHold` (`HoldAreaDefense`, `HoldSingleDefense`, `AreaDefenseStretched`,
`SingleDefenseStretched`).

### Die Stufen

| Stufe | Regel | Warum hier |
|---|---|---|
| alle | **Schranke:** Eine Rückhaltung weicht bei Gefährdungsklasse 1 (`ObjectHelper.IsInCriticalClass`: ungeschützt, vorausgerechnete effektive Gesundheit auf oder unter `HealthForDyingTanks`, Konzept 07). Flächenabwehr: irgendein lebendes Mitglied dort, oder der angekündigte, gemessene Flächentreffer brächte ein ungeschütztes dorthin. Einzelabwehr: der Spieler selbst oder ein Tank dort | Konzept 09 verlangt es für den Tank („jede Rückhaltung erst, wenn Stufe 1 gesichert ist"); der Grund gilt für jede Rolle |
| alle | **Streckungsbaustein:** Nach einer Auslöseraktion ruht die übrige eigene Abwehr, bis die Wirkung laut Wirktext ausläuft (die Dauer, die zur Minderung gehört), gezählt ab dem Einsatz laut Aktionsprotokoll. Hält der Auslöser noch eine Ladung (gelesen an seiner Wiederaufladegruppe, nicht am Knopf), streckt er nicht | derselbe Mechanismus stand zweimal mit festen Zahlen im Code (Weißmagier, Astrologe) |
| Heiler | leer | Nur Weißmagier und Astrologe strecken; Gelehrter und Weiser nicht. Eine Heilerregel änderte zwei Jobs ohne belegten Nutzen |
| Tanks | **Abtausch (Arm’s Length) im Pull für seine Verlangsamung** (`ArmsLengthSlowsPull`, A194): jeder Tank mit eigener Option „Use Arm's Length on a pull for its Slow", ab Werk an; ein Pull sind so viele Gegner in Reichweite wie die globale Zahl „Number of hostiles" der Abwehr (Dunkelritter: seine Barrierenzahl, dazu die Barrierenrückhaltung), **Bosse nicht mitgezählt** (`SlowableHostilesInRange`, A236): Abtausch mindert den Treffer nicht, der ihn auslöst, also bringt er an einem Boss-Tankbuster nichts — sein Hinweis vom 01.10.2026, Boss allein in der Arena; die zentrale Rückfallstufe der Einzelabwehr wirkt Abtausch nur noch für Rotationen ohne eigene Pull-Regel (`HasOwnArmsLengthPullRule`) und nur unter derselben Bedingung, vorher ohne jede Gegnerzahl; nicht, solange die Gruppe schon verlangsamt ist, und nicht, solange BossModReborn einen Rückstoß nach Wirkende und vor Ende der Abklingzeit ankündigt (A212, A219). Eine Rückhaltung gibt es auf dieser Stufe nicht: Burst-Rückhaltung nur bei Dunkelritter und Revolverklinge, bei beiden an ein eigenes Burstfenster gebunden | kostet nur die eigene Abklingzeit und wirkt bei jedem Tank gleich; die Beobachtung, auf die der TODO-Eintrag wartete, wäre eine Spielbestätigung als Aufgabe an ihn gewesen |
| Damage Dealer | leer, eine Frage an ihn | Barde, Pictomancer und Tänzer führen dieselbe Einstellung „Prevent the use of defense abilties during burst" (ab Werk an), Maschinist, Dragoon und Viper feste Rückhaltungen. Eine gemeinsame Regel wäre möglich; ihr Einstellungstext bindet, siehe unten |
| Job | die Auslöser und Rückhaltungen selbst | siehe nächste Tabelle |

### Die Sonderregeln je Job

| Job | Rückhaltung | Umfang |
|---|---|---|
| Weißmagier | Streckung nach Temperance oder Liturgy of the Bell (je 20 s laut Wirktext); Einzelabwehr nach Divine Benison (15 s) oder Aquaveil (8 s) | Flächen- und Einzelabwehr |
| Astrologe | Streckung nach Macrocosmos (15 s) oder Collective Unconscious (10 s, die Dauer der Minderung; der Ring steht 18 s); dieselben Auslöser halten die Einzelbarriere | Flächenfähigkeit, Flächen-GCD, Einzel-GCD |
| Dunkelritter | Burstfenster (`InTwoMIsBurst`): Dark Missionary, Reflexion, Oblation auf sich; Barriere wartet auf Bruch (`HoldMitigationForBarrier`) | Fläche; die Barrierenrückhaltung auch in der Einzelabwehr |
| Revolverklinge | Einschub vor dem No-Mercy-Auftakt; No-Mercy-Fenster: Heart of Light, Reflexion | Fläche; Auftakt auch Einzel und Heilung auf sich oder einen Tank (Heart of Corundum, Aurora) |
| Maschinist | Überhitzung, Wildfire, Full Metal Field; umkämpfter Burst-Einschub | Fläche und Einzel |
| Dragoon | unmittelbar nach Stardiver | Fläche und Einzel; die eigene Heilung (Second Wind, Bloodbath) weicht, wenn er selbst in Klasse 1 steht (`HoldSelfHeal`) |
| Viper | Einschub für Serpent's Ire im Burst | Fläche und Einzel |

**Heilungen sind eingeschlossen,** wo die Rückhaltung strategisch ist. Erhoben sind alle frühen
Rücksprünge der Heil- und Notfallmethoden der Standardrotationen (A161); strategisch ohne eigene
Einstellung sind nur die zwei in der Tabelle (Revolverklinge, Dragoon).

**Nicht über die Schranke:**
- *Eigene Sicherheitsvorgaben:* Swiftcast für eine anstehende Wiederbelebung (vier Heiler); das
  Living-Dead-Fenster, Walking Dead und die Heilverbote — seine Entscheidungen (Konzepte 08 und 09).
- *Technik:* Aussperrungen durch das Spiel (Phantom-Job), Mudra in Ausführung (Ninja), Tanzschritte
  (Tänzer), die Doppeldrucksperren von Radiant Aegis und Benediction, die Reihenfolge Recitation →
  Excogitation.
- *Einstellungen, deren Text die Rückhaltung ohne Ausnahme anordnet:* „Prioritize Microcosmos over all
  other healing when available" und die Strategie für Essential Dignity (Astrologe), dazu die
  Burst-Einstellung von Barde, Pictomancer und Tänzer (unten). Der Text bindet.

**Barde, Pictomancer, Tänzer:** Deren Einstellung „Prevent the use of defense abilties during burst" sagt
ohne Ausnahme „verhindern". Ihr Text bindet; die Schranke greift dort deshalb nicht. Ob sie weichen
soll, ist seine Entscheidung (Einstellungstext und Vorgabe).

### Was die Sperren im Spiel bewirken

- **Weißmagier:** Plenary Indulgence und Temperance fallen auf denselben Treffer (je −10 %,
  nacheinander −19 %), weil erst Temperance die Streckung auslöst. Danach warten Divine Caress und
  Liturgy auf den nächsten Treffer.
  - **Kein Stapelschutz im Sinn der Spielmechanik:** Verschiedene Status wirken zusammen. Nur
    derselbe Status aus zwei Quellen (Reflexion zweier Tanks, Addle, Feint; Kerachole und Taurochole)
    wirkt nicht doppelt, und das regelt RSR getrennt davon (`StatusFromSelf = false`).
  - **Vorteil:** Zwei Raidwides im Abstand von 20 bis 60 s bekommen beide etwas. Ohne Streckung ginge
    alles auf den ersten, und Plenary käme erst nach 60 s zurück. Das ist sein Prinzip aus „Wozu die
    Aussetzbedingungen da sind": strecken statt verdoppeln, solange die Gruppe hält.
  - **Preis:** Divine Caress verfällt mit Divine Grace (30 s ab Temperance) und bleibt in den letzten
    10 s nutzbar. Kommt dort kein Treffer, ist es verloren. Ohne Streckung fiele es auf denselben
    Treffer wie Temperance; welches von beiden mehr wert ist, hängt am Abstand der Treffer. Die
    Schranke deckt den Fall, in dem es auf den ersten Treffer ankommt.
- **Dunkelritter:** Herkunft ist die Balance-Rotation 6.38 (c97be9ec5).
  - Damals hieß Burst „mindestens die Hälfte der Gruppe im Zwei-Minuten-Burst"
    (`RatioOfMembersIn2minsBurst`). Heute zählen nur die eigenen Abklingzeiten: Delirium kühlt ab
    (Blood Weapon ist durch Blood Weapon Mastery Delirium, gelesen über die angepasste Id), und Living
    Shadow liegt unter 15 s zurück.
  - Der Zweck ist Schaden in den 20 s der Gruppenbuffs. Größenordnung (Überschlag, nicht gemessen):
    rund 0,3 % des Dunkelritter-Schadens je zwei Minuten, falls ein Edge of Shadow dadurch aus den
    Buffs fällt.
- **Zusammenspiel mit anderen Klassen:**
  - Keine Sonderregel liest eine andere Klasse.
  - Mit Dunkelritter und Revolverklinge, beide mit RSR, gibt im gemeinsamen Burst keiner der beiden
    Tanks Gruppenminderung, außer die Schranke greift.
  - Zwei Heiler mit RSR antworten auf dasselbe Signal im selben Moment. Die Streckung verteilt nur die
    eigenen Mittel über die Zeit.

### Warum die Schranke so misst

- **Der gemessene Anteil wird nicht um die liegende Minderung gekürzt.** `Watcher.ActionFromEnemy`
  speichert den höchsten je gelandeten Anteil, nach der damals liegenden Minderung, und hebt ihn nur
  an. Die jetzt liegende Minderung abzuziehen zählte sie doppelt. Die Schätzung irrt also Richtung
  „gefährlich" — die Rückhaltung weicht eher zu oft als zu spät.
- **Klasse 1 schon jetzt zählt mit,** auch ohne gemessenen Treffer: Wer dort steht, stirbt am nächsten
  Treffer (Konzept 07). Damit weicht die Rückhaltung auch vor ungemessenen Zaubern und im
  Dauerstrom eines Gruppenpulls.
- **Einzelabwehr nur für Spieler und Tanks:** Sie wird für einen Tankbuster oder die eigene Gefahr
  geöffnet, und das meiste, was sie zurückhält, wirkt nur auf den Wirkenden (Camouflage, Rampart,
  Heart of Corundum). Ein Damage Dealer in Gefahr am anderen Ende der Arena gewönne nichts; für ihn
  antwortet die Heilkette.
- **Ein angekündigter Tankbuster allein ist kein Grund.** Er ist genau das Signal, das die Einzelabwehr
  öffnet; wiche die Rückhaltung ihm, wäre sie in dem Moment aufgelöst, in dem sie gefragt ist — zwei
  Tankbuster in Folge verlören beide die Streckung. Einen gemessenen Anteil wie beim Flächentreffer
  gibt es für Tankbuster nicht; es bleibt Klasse 1.
- **Unverwundbare zählen nicht:** Ein Tank unter Hallowed Ground oder Superbolide steht absichtlich
  niedrig und ist durch den Treffer nicht gefährdet.
- **Einsatzzeit aus dem Aktionsprotokoll, Ladung aus der Wiederaufladegruppe, nie vom Knopf:** Manche
  Knöpfe werden während der Wirkung zu einer anderen Aktion (Liturgy of the Bell zur zweiten
  Auslösung, Macrocosmos zu Microcosmos), deren Abklingzeit nicht die des Auslösers ist. Die frühere
  Jobregel las den Knopf; ob sie deshalb während der Wirkung nie hielt, ist ohne Laufzeit nicht
  belegt, die neue Lesart ist in beiden Fällen richtig. Bei geladenen Aktionen (Divine Benison) misst
  die Wiederaufladung nicht die Zeit seit dem Einsatz; deshalb das Protokoll.
- **Die Dauer ist die, die zur Minderung gehört:** der erste Wert, den der Wirktext nach der Minderung
  nennt. Collective Unconscious gibt dem Ring 18 s und der Minderung 10 s; es zählt die Minderung.
- **Keine neue Zahl:** `HealthForDyingTanks` ist seine Einstellung der Gefährdungsklasse, die Dauern
  stehen in den Wirktexten.
- **Ersetzt A146** („frei bei großem Treffer"). Das hätte im Fall zweier großer, einzeln tragbarer
  Raidwides im Abstand von 25 s alles auf den ersten gelegt und für den zweiten nichts gelassen.

### Folgen, bewusst hingenommen

- **Astrologe:** Die Streckung dauert jetzt so lange wie die Wirkung laut Wirktext: 15 s nach
  Macrocosmos und 10 s nach Collective Unconscious, statt der früheren festen 30 und 20 s, die zu
  keiner Wirkung passten.
- **Ein Treffer ohne Zauberleiste** wird nur über Klasse 1 erkannt. Die Schranke kann ihn sonst
  nicht vorhersehen.
- **Ein Tankbuster auf einen gesunden Tank** löst keine Rückhaltung; erst Klasse 1.

### Woran jede Sicherheitsregel die Wahrscheinlichkeit misst

Seine Präzisierung: Eine Schutzmaßnahme, die Schaden kostet, lohnt, wo ein Treffer angekündigt oder
wahrscheinlich ist. Die Regeln dieses Konzepts messen das so:

| Regel | Maß der Wahrscheinlichkeit |
|---|---|
| Flächenabwehr (`AutoStatus.DefenseArea`) | ein laufender Flächenzauber; mit `MitigateBigAreaCastsEvenIfInterruptible` auch ein unterbrechbarer, dessen gemessener Anteil die größte Barriere übersteigt; ein BossMod-Raidwide im Fenster; der Befehl „Defense Area" von Hand |
| Einzelabwehr (`AutoStatus.DefenseSingle`) | ein Zauber auf einen Tank (Tankbuster) oder ein BossMod-Tankbuster im Fenster; beim Heiler zusätzlich, wie viele Gegner den Tank angreifen |
| Heilung vor dem angekündigten Treffer | die vorausberechnete Gesundheit nach dem laufenden Cast — der Treffer ist angekündigt |
| Schranke der Rückhaltungen | Gefährdungsklasse 1 jetzt, oder ein angekündigter, gemessener Treffer brächte ein ungeschütztes Mitglied dorthin |
| Burst-Rückhaltungen (Jobtabelle oben) | keine eigene: Sie halten ohne Blick auf die Wahrscheinlichkeit und weichen nur der Schranke |
| Streckung | eine eigene Minderung liegt noch; der nächste Treffer wird von ihr getragen, bis sie ausläuft |
| Kanalsperre (Paladin, Astrologe) | der angekündigte Treffer steht noch aus (`DataCenter.AreaHitPending`); danach löst sie (Konzept 14, „Wechselwirkungen und Zeit") |

**Im Kampf ablesbar:** Unter „Defense hold" im Diagnosefenster steht je Regel, ob sie zuletzt hielt
oder warum sie wich, seit Kampfbeginn — nur zur Kontrolle, weil eine wartende Abwehr sonst nicht von
einer nie gefragten zu unterscheiden ist.

## Heilung vor dem angekuendigten Treffer

**Jede Heilschwelle im Baum liest die Gesundheit, die ein Mitglied **hat**. Keine liest die, die es
haben wird, wenn der bereits laufende Cast einschlaegt.** Ein Mitglied bei 60 % vor einem
45-%-Raidwide steht ueber jeder Schwelle und stirbt daran. Das ist Stufe 2 der Vorgabe, woertlich:
„die aktuelle hp liegt unter dem schadenswert. dann waere aber eine heilung sinnvoll bis max maxhp."

**Die Groesse dafuer wird seit A99–A102 gemessen und war bisher nur fuer eine Frage im Gebrauch** —
ob gemindert wird. Dieselbe Zahl beantwortet die andere Haelfte: ob **vorher** zu heilen ist.
`DataCenter.AnnouncedHitDropsAnyoneBelow` stellt die Frage einmal, und beide Seiten lesen sie.

| Schalter | Frage | Stand |
|---|---|---|
| `Skip mitigation for small area casts` | Oeffnet der Treffer die Abwehrkette? | **an** als Vorgabewert |
| `Heal ahead of an announced area cast` | Wird vor dem Treffer geheilt? | **aus** als Vorgabewert |

**Die Barriere zaehlt hier mit, und das widerspricht A85 nicht.** A85 hat die Barriere aus der
allgemeinen Heilschwelle entfernt, weil sie keine Gesundheit herstellt — ein Tank bei 40 % hinter
einem Schild steht bei 40 %, sobald der Schild ungenutzt ablaeuft. Hier ist die Frage eine andere:
ueberlebt er **diesen** Treffer. Gegen ihn wird die Barriere verbraucht und faengt ihn ab; sie
herauszurechnen hiesse, eine Heilung zu fordern, die der Schild bereits bezahlt hat.

**Gefragt wird an derselben Schwelle, die die Flagge ohnehin benutzt**, nur einen Cast frueher —
nicht an einer schaerferen. Die Regel kann deshalb nicht dort heilen, wo der Baum ohnehin nicht
geheilt haette. Und sie stellt die Frage **selbst** (`IsHostileCastingAOE`), statt einen anderswo
abgelegten Wert zu lesen: Sonst haenge sie daran, ob der Verteidigungszweig im selben Bild vorher
lief — und der steht hinter `UseAoeDefense`, sodass die Regel bei abgeschalteter Flaechenabwehr
still nie gefeuert haette.

**Beim Beschwoerer gilt fuer Lux Solaris die eigene Regel** („Wann Lux Solaris zuendet"), auch auf dem
Heilpfad. Sie ist reaktiv; vor einem angekuendigten Treffer faellt sie nur, wenn ein Mitglied im Radius
schon in Gefaehrdungsklasse 1 steht, dem Wirkenden oder jedem anderen im Radius eine volle Heilung fehlt
oder das Fenster verfaellt.

**Die Heil-oGCD nimmt den ersten Einschiebeplatz, die Minderung den naechsten (A140).** Die Regel setzt
auch `HealAreaAbility`, und der Dispatch fragt Heil-Faehigkeiten vor `DefenseArea`. Das ist Vorgabe 2
in der Reihenfolge, die sie verlangt: Die Vorausheilung faellt nur, wenn der Treffer jemanden unter
die Schwelle druecken wuerde — genau der Fall, in dem zuerst zu heilen ist. Die Minderung eines
grossen Treffers verliert dabei keinen Weg, nur einen Platz: Ein angekuendigter Cast laeuft in der
Regel ueber mehrere GCDs, also ueber mehrere Einschiebeplaetze. **Verdraengt** wird sie nur, wenn die
Ankuendigung kuerzer ist als ein GCD; fuer diese Lage steht die BossModReborn-Vorhersage bereit, die
vor dem Cast mindert. Schluss aus Zweigreihenfolge und Castdauer, im Spiel nicht beobachtet. Ob ein
Treffer je ohne Minderung einschlug, weil der letzte Platz an eine Heilung ging, zeigt keine Anzeige;
das ist eine benannte Grenze dieses Abschnitts, kein geaendertes Verhalten.

## Die proaktive Schicht haengt fast vollstaendig an BossModReborn

**Vorgabe des Auftraggebers:** „bossmod liefert nicht für jeden boss werte, sondern nur für
unterstützte module. und da ist der abdeckungsgrad in bossmod auch unterschiedlich. sich auf bossmod
zu 100% zu verlassen ist fahrlässig."

**Erhoben, nicht geschaetzt** (Lauf vom 20.09.2026): Der Baum kennt zwei Ebenen der Abwehr, und die
Trennung verlaeuft genau entlang der Frage, woher die Vorhersage kommt.

| Ebene | Ausloeser | Quelle | Faellt aus, wenn |
|---|---|---|---|
| **proaktiv** — mindern, **bevor** etwas ankommt | `BMRShouldRefreshBefore(BMRTankbusterIn \| BMRRaidwideIn, …)` bei allen vier Tanks, bei Barde, Maschinist, Taenzer und beim Beschwoerer (Schimmerschild) | ausschliesslich BMR | kein Modul, oder ein Modul ohne diese Ereignisart |
| **reaktiv** — antworten, wenn der Cast **laeuft** | `IsHostileCastingAOE`, `IsHostileCastingToTank`, `IsHostileCastingTankBusterAtMe`, Gegnerzahl in `ShouldSustainMitigationDebuff` | eigene Beobachtung | der Cast ist unterbrechbar, zu kurz, oder ausserhalb des Ein-GCD-Fensters |

**Kein Alleinstand, aber eine vollstaendige Ebene.** Jede proaktive Stelle hat einen reaktiven
Nachbarn, die Abwehr faellt also nicht aus — sie kommt **spaeter**, naemlich erst, wenn der Cast
schon laeuft und den Vorfilter passiert hat. Das ist der Unterschied zwischen „vor dem Einschlag
gedeckt" und „waehrend des Einschlags gedeckt".

**Der Ausfall ist still, und das ist der eigentliche Mangel.** Beide Ausfallarten — kein Modul, oder
ein Modul, das Raidwides nicht fuehrt — erreichen den Baum als `float.MaxValue`, und jede Pruefung
gegen ein Zeitfenster liest das als „es kommt nichts". Eine ausbleibende Minderung sieht damit aus
wie ein ruhiger Kampf. Die Diagnoseseite „BMR Data" nennt deshalb jetzt das aktive Modul und, je
Ereignisart, ob es dafuer ueberhaupt eine Vorhersage liefert.

**Seine Begruendung, woertlich:** „daher die eigene liste mit dem aoe-schadensausmaß.“ Die Messung je
Aktion ist als **Ersatz** fuer die fehlende Verlaesslichkeit gedacht, nicht als Zusatz — und sie
beantwortet zudem eine Frage, die BMR gar nicht stellt: wie hart der Treffer ist.

**Die eigene Messung ist die einzige BMR-unabhaengige Vorhersagequelle im Baum**, und sie ist bisher
an genau einer Stelle proaktiv verdrahtet: `IsHostileCastingLargeArea` (Konzept 13) liest den
gemessenen Anteil je Aktion und braucht dafuer kein fremdes Plugin. Dass dieselbe Quelle die
proaktive Schicht der uebrigen Jobs tragen koennte, ist erfasst und nicht gebaut — `TODO.md`.

### BMR sagt wann, nicht wie hart — und daran wird die Abklingzeit verschenkt

**Spielbeobachtung des Auftraggebers, Ewige Koenigin, Anfangsphase:** zwei Flaechenangriffe
nacheinander, erst ein kleiner, dann ein grosser. Die Vorhersage feuert auf den ersten, Schimmerschild
oder Tactician geht dafuer hinaus, und beim zweiten ist die Barriere aufgebraucht oder die Minderung
abgelaufen.

**Die Ursache steht in der Bedingung selbst.** `BMRShouldRefreshBefore` prueft die **Zeit** bis zum
naechsten Ereignis und ob der eigene Status bis dahin abgelaufen waere — mehr nicht. Eine Groesse
kommt darin nicht vor, und BMR liefert auch keine: Die Schnittstelle nennt den Zeitpunkt, nicht die
Wucht. **Das ist das Loch, das die Groessenbewertung offen gelassen hat:** Der reaktive Weg fragt seit
A108 „ist dieser Treffer eine Abklingzeit wert"; der proaktive hat es nie gefragt.

**Eine Barriere ist dabei der klarere Fall als eine Minderung**, und das folgt aus dem Unterschied,
den dieses Konzept ohnehin fuehrt: Sie gibt **feste Punkte** aus, also frisst ein kleiner Treffer sie
ganz auf. Eine Minderung skaliert mit dem Treffer und geht nicht verloren — sie laeuft nur ab, bevor
der grosse kommt.

**Die Groesse eines laufenden Casts ist bekannt.** Sie wird je Aktion gemessen. Laeuft also gerade
ein bewertet **kleiner** Flaechencast, ist er der wahrscheinlichste Gegenstand einer Vorhersage, die
auf die unmittelbare Zukunft zeigt — und die proaktive Auffrischung kann auf das naechste Ereignis
warten. Gebaut hinter `Hold a predicted mitigation while a small cast is running`, **Vorgabewert
an**, mit `ProactiveMitigationHeld` als Sonde.

**Als Heuristik gekennzeichnet, nicht als Beweis:** Nichts hier belegt, dass die Vorhersage den Cast
meint, der gerade laeuft. Laeuft nichts, oder ist der laufende Cast nie gemessen worden, wird keine
Aussage getroffen und die Vorhersage wie bisher befolgt.

#### Zwei Ladungen — und warum sie hier trotzdem nicht ausgespielt werden

**Schimmerschild hat auf Stufe 100 zwei Ladungen**, belegt am Merkmalstext: „Enhanced Radiant Aegis
[480] — Allows the accumulation of charges for consecutive uses of Radiant Aegis. Maximum Charges:
2“ (Angabe des Auftraggebers, am Merkmal bestätigt). Troubadour, Tactician und Shield Samba haben
eine.

**Damit scheint der Fall gelöst:** Bei zwei Treffern in Folge deckt eine Ladung den ersten, die
zweite den zweiten — beide voll, ohne Raten. **Diese Lösung wird nicht genommen, und der Grund ist
eine dokumentierte Entscheidung des Auftraggebers.**

A9 (`6704335d`) hat den ungegateten Radiant-Aegis-Zweig auf seine Meldung hin entfernt, der Schild
gehe ohne Gefahr hinaus. Die dort festgehaltene Begründung ist genau diese: **„`usedUp: true` gab
dabei auch die zweite Ladung frei — bei echter Gefahr war keine mehr da.“** Er hat es bei der Arbeit
an dieser Regel wiederholt: die Doppelzündung von Schimmerschild war neben Addle einer der ersten
Fehler, die der Fork behoben hat.

**Die Frage wird nicht vorgelegt, sondern im Kampf entschieden — und das ist der Unterschied
zwischen dem Buchstaben seiner Entscheidung und ihrem Grund.** A9 verbietet nicht „die zweite Ladung
ausgeben“, sondern nennt die Folge: „bei echter Gefahr war keine mehr da.“ Das ist eine Aussage
über **Verfügbarkeit**, und Verfügbarkeit ist zur Laufzeit ausrechenbar:

> Ausgeben ist unbedenklich, wenn danach noch eine Ladung steht **oder** die nächste vor dem
> vorhergesagten Ereignis zurück ist. Sonst ist die Reserve echt, und die Zurückhaltung gilt.

Der A9-Fall selbst bleibt unberührt: Dort gab es überhaupt keine Vorhersage, dieser Zweig wird also
nie erreicht.

### Die Regel bewertet sich selbst, statt eine Ablesung zu verlangen

**Vorgabe des Auftraggebers, und sie gilt hier zum zweiten Mal:** „entscheidung immer im spiel, nicht
retroperspektive auswertung.“ Ein Zähler, den jemand lesen und berichten muss, kostet je Zahl einen
Kampf, ein Ablesen, einen Bericht und eine Runde.

**Die Zurückhaltung trifft eine prüfbare Vorhersage:** „Das Ereignis, auf das die Vorhersage zeigt,
ist nicht der kleine Cast, der gerade läuft — es kommt noch ein größerer.“ Die nächsten Sekunden
beantworten das. Der Effekt-Handler sieht jeden eingehenden Treffer; kommt innerhalb der Statusdauer
einer auf Höhe der großen Barriere, war die Zurückhaltung richtig, sonst hat sie eine Minderung
verschenkt.

**Und die Regel handelt nach ihrem eigenen Ergebnis:** Unterhalb des Gleichstands — öfter falsch als
richtig — hält sie nicht mehr zurück. Der Gleichstand ist keine gesetzte Zahl, sondern der
Break-even des Tauschs, den sie macht. Die Bilanz gilt je Kampf und wird mit
`DataCenter.ResetAllRecords` verworfen; sie von einem Boss in den nächsten zu tragen hieße, den einen
nach dem Muster des anderen zu beurteilen.

**Die Anzeige ist damit Zweitverwertung, nicht Zweck** — sie meldet das Urteil, das die Regel bereits
gefällt hat, und nennt ausdrücklich, wenn die Regel sich selbst stillgelegt hat.

#### Die genaue Frage laesst sich nicht stellen, und das ist gemessen

**Naheliegender waere:** erheben, auf **welches Ereignis** die Vorhersage anspielt, dessen Bewertung
in der eigenen Liste nachschlagen und nur bei „gering" aussetzen. Der Auftraggeber hat genau das
vorgeschlagen. **Die Information existiert auf der Gegenseite nicht** — geprueft an
BossmodReborns Quelltext am 20.09.2026, nicht angenommen:

| Endpunkt | Was er zurueckgibt | Aktionsbezug |
|---|---|---|
| `Timeline.NextRaidwideIn` | `module.StateMachine.NextTransitionWithFlag(StateHint.Raidwide)` — ein **Zustandsuebergang**, den der Modulautor als Raidwide markiert hat | keiner; es gibt keinen Cast dahinter |
| `Hints.NextRaidwideDamageIn` | Aktivierungszeit des ersten `PredictedDamage`-Eintrags dieses Typs | keiner; `DamagePrediction` traegt genau `Players`, `Activation`, `Type` |

**Und selbst mit Groessen waere der gemeldete Fall nicht zu loesen:** Die Vorhersageliste wird nicht
herausgegeben. Jeder Endpunkt liefert den **ersten** passenden Eintrag. „Es kommen zwei, der zweite
ist der grosse" ist ueber die Schnittstelle nicht lesbar.

**Daraus folgt die Bauform, nicht aus Bequemlichkeit:** Was gerade laeuft, ist die einzige
Groesseninformation, die in diesem Moment vorliegt. Die Regel ist deshalb so gut, wie diese Zuordnung
trifft — und nicht besser.

### Ein Gegner-Debuff in der Einzelabwehr fehlt dem angesagten Raidwide

**Stand:** Reflexion (Reprisal, alle vier Tanks), Zermürben (Feint, Nahkämpfer) und Stumpfsinn (Addle, Magier) stehen
in der Einzelabwehr ihrer Rotationen, jeweils hinter der Nachziehregel `ShouldSustainMitigationDebuff` und als
schlichtes `CanUse` (erhoben 01.10.2026: WAR, PLD, GNB, DRK, SAM, RPR, MNK, VPR, DRG, RDM, PCT, BLM; der
Beschwörer über `TryAddleBeforeDamage`). Die Einzelabwehr öffnet bei Tanks für jeden Zauber auf ihr Ziel und jeden
Tankbuster, bei Schadensausteilern für einen Tankbuster auf sie selbst. Fällt der Debuff dort, deckt er nur diesen
Treffer — und fehlt dem nächsten Raidwide, der die ganze Gruppe trifft. The Balance nennt Reprisal auf einen
Tankbuster deshalb „very situational". Dieselbe Bauform hat Abschütteln in der Einzelheilung des Kriegers
(Konzept 09, Befund 6, S3).

Wirkung und Abklingzeit (Job-Guide): Reflexion −10 % ausgeteilter Schaden der Gegner im Umkreis 5 Yalm, 15 s, 60 s;
Zermürben −10 % physisch, −5 % magisch, Stumpfsinn −10 % magisch, −5 % physisch, je 15 s und 90 s. Verschiedene Debuffs
wirken nebeneinander, zwei gleiche nicht (`TargetStatusProvide` sperrt einen zweiten).

| Lage | heute | X1: zurückhalten, wenn ein Raidwide in (Wirkdauer, Abklingzeit] angesagt ist |
|---|---|---|
| kein Modul, oder Modul ohne Raidwides | Debuff am Tankbuster | unverändert — BossModReborn meldet `float.MaxValue`, also keine Ansage |
| Raidwide innerhalb der Wirkdauer (≤ 15 s) | Debuff deckt beide | unverändert, deckt beide |
| Raidwide nach Wirkdauer, vor Ende der Abklingzeit | deckt den Tankbuster; der Raidwide kommt ohne diesen Debuff | Tankbuster ohne die 10 %, Raidwide mit — über die Flächenabwehr, die den Debuff dann wirkt |
| Raidwide erst nach der Abklingzeit | deckt den Tankbuster | unverändert |
| Spieler oder ein Tank in Gefährdungsklasse 1 | — | die Rückhaltung weicht (`HoldSingleDefense`) |
| Einzelabwehr geöffnet von einem gewöhnlichen Zauber auf das Ziel des Tanks | Debuff verbraucht | gehalten — hier gewinnt die Regel am meisten |
| Pull ohne Modul | Reflexion im Pull | unverändert |

**Was abzuwägen ist, gerechnet an einem Beispiel** (Annahme: Tankbuster 80 % der Tankgesundheit, Raidwide 40 % je
Mitglied): Am Tankbuster spart der Debuff 8 % *einer* Gesundheit, am Raidwide 4 % *jeder*, in einer leichten Gruppe 16 %,
in einer vollen 32 % zusammen. Für die Sicherheit zählt nicht die Summe, sondern der Schwächste: Am Raidwide ist das ein
Heiler oder Schadensausteiler mit weniger Gesundheit, am Tankbuster der Tank. Beide können sterben; die
Gefährdungsklasse fängt den Fall, in dem es knapp wird.

**Grenzen:**
- BossModReborn nennt nur den *nächsten* Raidwide und keine Höhe (Abschnitt „BMR sagt wann"). Ist er klein und der
  Tankbuster groß, hält X1 in die falsche Richtung. Prüfbar ist das zur Laufzeit: Kam innerhalb der Abklingzeit
  kein Raidwide, war die Rückhaltung umsonst — dieselbe Selbstbewertung wie `ScoreProactiveHold` kann X1 stilllegen,
  wenn sie öfter falsch als richtig liegt.
- Ein zweiter Tank kann den Raidwide mit seiner Reflexion decken. Das ist Verhalten eines anderen Spielers, also kein
  tragender Grund.

**Optionen:** X0 belassen; X1 wie oben, als Regel für **alle Rollen** zentral (eine Prüfung „Raidwide in (Dauer,
Abklingzeit] angesagt", gelesen von jeder Einzelabwehr mit Gegner-Debuff), mit Selbstbewertung; X2 wie X1 und dieselbe
Prüfung für Abschütteln in der Einzelheilung (Dauer der Barriere 30 s, Abklingzeit 90 s); X3 Debuffs in der Einzelabwehr
nur noch bei Gefahr — verworfen, weil es auch die Fälle ohne Raidwide-Konflikt verliert. **Empfehlung: X2**, als Option
ab Werk an. Zur Entscheidung (TODO).

## Wann Lux Solaris zuendet

**Sachstand der Regel, aus seinen Vorgaben vom 26.09.2026 (A151 bis A153), umgesetzt in A154 bis A156**
(`SMN_Reborn.LuxSolarisDecision`, gefragt von allen drei Wegen).

**Was Lux Solaris ist.** **Hinweis des Auftraggebers:** Lux Solaris ist eine Point-Blank-Flaeche vom
Wirkenden aus, wie Holy beim Weissmagier. Die Wirktexte stuetzen das gleich: Holy trifft „all nearby
enemies", Lux Solaris heilt „own HP and the HP of all nearby party members". Im Baum gehoeren beide
damit in die Zweige fuer Flaechen mit Reichweite 0 — Holy in den feindlichen, Lux Solaris in das
freundliche Gegenstueck (`ActionTargetInfo.FindTarget`); welche Reichweite das Spiel zur Laufzeit meldet,
zeigt die Beschwoerer-Anzeige. Eine
reaktive Flaechenheilung um den Beschwoerer (Wirktext 36997: „Restores
own HP and the HP of all nearby party members", Heilpotenz 500), wirkbar nur unter Refulgent Lux, das
Summon Solar Bahamut fuer 30 s gewaehrt (36992). Kein Schild, keine Minderung: Was sie vor einem Treffer
tut, zaehlt nach dem Treffer nicht mehr. Sie kostet kein MP und keinen GCD, nur einen
Einschiebeplatz, und sie ist eine Beigabe — ungenutzt verfaellt sie mit Refulgent Lux.

**Die Regel, in der Reihenfolge der Pruefung:**

1. **Verbote zuerst** (Vorgabe 2: „negativvorgaben wie nicht casten, weil sonst schaden eingeht,
   muessen beachtet werden"). Lux Solaris faellt auf keinem Weg:
   - unter **Shackled Healing** (Status 4564: „Use of HP-restoring actions will inflict Shackles of
     Penitence on those nearby"), solange ein anderes Gruppenmitglied in der Naehe steht — die Strafe
     traefe die Umstehenden;
   - unter **Scalebound** (1495: „unable to heal wounds via any method save mega potions") — die
     Heilung wirkt nicht;
   - solange ein Dunkelritter im **Living-Dead-Fenster** gehalten wird und im Radius steht (Vorgabe 5:
     „bei living death ist es aber im wahrsten sinne toedlich") — die Heilung naehme ihm den Ausloeser.
     **Auch dann, wenn ein anderes Mitglied in Gefaehrdungsklasse 1 steht, und auch vor dem Verfall**
     (seine Entscheidung vom 26.09.2026): In Savage und Extreme ist Living Dead die Antwort auf einen
     Tankbuster, ein toter Tank ist meist der Wipe, und die Mechanik hat Vorrang vor der Heilung eines
     anderen Mitglieds — zumal ein Tankbuster selten mit einem Flaechenangriff zusammenfaellt und die
     Aggro beim Tank liegt. Lux Solaris wird aufgehoben, bis Walking Dead eintritt, und heilt dann mehrere
     einschliesslich des Tanks (Punkt 2). Verfaellt Refulgent Lux waehrend der Sperre, ist das der
     hingenommene Preis. Fuer die Heilaktionen der Heiler bleibt Konzept 09: Dort trifft eine
     Flaechenheilung den Traeger als Nebenwirkung, weil die Gruppe vorgeht; Lux ist eine Beigabe.
     **Beide Stellungen des Schalters:** Die Sperre folgt `IsHeldForDeathTrigger`, also
     `WithholdHealingForLivingDead`. Ist der Schalter aus, will der Spieler den Tod als Ausloeser nicht —
     RSR zuendet Living Dead dann auch als letzte Rettung (Konzept 09) —, und Lux sperrt nicht.
2. **Nur im Radius** (Vorgabe 4: „eine umkreispruefung ist immer sinnvoll"). Jeder Bedarf wird an den
   lebenden, heilbaren Mitgliedern **im Wirkradius um den Beschwoerer** gemessen, nie an der ganzen Gruppe.
   Den Radius liefert das Spiel (`EffectRange`). Ist im Radius niemand verletzt, faellt nichts.
   **Die eigene AoE-Anzahl der Aktion gilt fuer jeden Wurf**, den Heilbefehl eingeschlossen: Ihr
   Einstellungstext „Number of targets needed to use this action" bindet, und ein Ziel einer Heilung ist
   ein Verletzter — so zaehlt der allgemeine Flaechenheil-Zweig (`GetCanAffects` laesst bei einer Heilung
   die Vollen weg) und wendet die Zahl auf jede Verwendung an. Ab Werk 1: „jemand im Radius ist verletzt".
   Ein Dunkelritter unter **Walking Dead** zaehlt dabei als Verletzter (Vorgabe 5: „lieber casten, bevor
   lux solaris ungenutzt verfaellt, vor allem, wenn auch noch andere gruppenmitglieder davon geheilt
   werden") — anders als bei den Heilaktionen der Heiler (Konzept 09), weil Lux sonst verfaellt.
3. **Normalfall: ohne Ueberheilung** (Vorgabe 6). Lux Solaris faellt,
   - wenn **dem Beschwoerer selbst** mindestens eine volle Heilung fehlt („wenn ich schaden erleide und lux
     solaris mich damit nicht ueberheilt, ist lux solaris korrekt angewendet"), oder
   - wenn **jedem anderen** Mitglied im Radius mindestens eine volle Heilung fehlt („erst lux solaris
     anwenden, wenn es auch bei allen anderen gruppenmitgliedern im radius nicht ueberheilt"); steht
     niemand anderes im Radius, gilt dieser Fall nicht.
   Die Heilmenge ist die gemessene (unten), nicht die Potenz.
4. **Ausnahme: bedrohlich geringe Gesundheit** (Vorgabe 6). Steht ein Mitglied im Radius in
   Gefaehrdungsklasse 1 (Konzept 07: effektive Gesundheit auf oder unter `HealthForDyingTanks`), faellt
   Lux sofort, ohne Ruecksicht auf Ueberheilung. Das Mass ist das vorhandene der Heilkette, keine neue
   Zahl.
5. **Ausnahme: kurz vor dem Verfall** (Vorgaben 1 und 6). Vor Ablauf von Refulgent Lux zaehlt nur, ob die
   Heilung ueberhaupt etwas bewirkt: Ist im Radius irgendjemand verletzt, **auch wenig**, faellt sie.
   Dabei wird gewichtet (Vorgabe 1: „minor heilung oder andere aktion"): Eine solche Kleinheilung nimmt
   keinen Platz, den eine Aktion braucht, die durch Aufschub an Wert verliert. Faellt Lux dadurch
   ungenutzt weg, ist das der geringere Verlust.
   **Erhoben (A152):** Das Verfallsfenster liegt 15 bis 30 s nach der Beschwoerung, also hinter der
   Demi-Phase; die Zweige der Demi (Energy Siphon, Enkindle, Sunflare, Deathflare) greifen dort nicht.
   Schutzaktionen, Heiltrank und Heilflaggen stehen im Dispatch vor `AttackAbility`, sie verdraengt Lux
   nie. Es konkurrieren die Schadens-Faehigkeiten der Primalphase:
   - **Fester / Necrotize** (Aetherflow): verlieren durch einen Platz Aufschub nichts; ein Ueberlauf
     droht erst, wenn Energy Drain wieder bereitsteht, und dessen Abklingzeit laeuft seit der
     Solar-Phase.
   - **Mountain Buster** (nur unter Titan's Favor): haengt an einem Status. Wie lange der liegt und ob der
     naechste Topaz Rite eine unverbrauchte Gunst ueberschreibt, steht nicht im Repository (unbelegt). Zur
     Laufzeit ist es lesbar: Endet der Status vor dem naechsten Einschiebefenster, ist ein Gegner in
     Reichweite und ist Mountain Buster eingeschaltet und erlernt, verliert die Aktion durch Aufschub
     ihren Wert und geht vor. Ohne die letzte Bedingung nahme niemand den Platz, und Lux verfiele.
   - **Searing Flash** (unter Ruby's Glimmer) konkurriert kaum: Ausserhalb einer Demi wirkt die Rotation
     es auf einen sterbenden Boss und im letzten Einschiebefenster vor dem Ende von Ruby's Glimmer (A234);
     ihm sonst vorzugehen hielte Lux fuer eine Aktion, die nicht kommt (A155).
   Die Gewichtung lautet damit, ohne neue Zahl: Die Kleinheilung am Verfall geht vor jede
   Schadens-Faehigkeit, deren Wert durch einen Platz Aufschub nicht verfaellt, und hinter jede, deren
   ermoeglichender Status bis zum naechsten Fenster endet. In drei GCDs liegen rund sechs Plaetze; dass
   Lux dabei leer ausgeht, ist die Ausnahme (Schluss aus der Platzzahl, nicht gemessen).

**Eine Entscheidung fuer alle Wege.** Heilflagge (`HealAreaAbility`), `AttackAbility` und
`GeneralAbility` fragen dieselbe Entscheidung; heute folgt jeder Weg einer eigenen Bedingung, und genau
daraus entstanden die Befunde in A150. Ausgenommen ist nur der **manuelle Heilbefehl**: Er ist sein
ausdruecklicher Wunsch und prueft allein die Verbote aus Punkt 1 und die AoE-Anzahl aus Punkt 2 —
diese, weil ihr Einstellungstext jede Verwendung bindet.

**Folgen der Regel, bewusst hingenommen:**
- Vor der ersten bestaetigten Landung seit dem Gebietswechsel ist die Heilmenge unbekannt; dann greifen
  nur die Punkte 4 und 5.
- Die Vorausheilung vor einem angekuendigten Treffer („Heal ahead of an announced area cast") bedient
  Lux nicht mehr, ausser ueber Punkt 4 oder wenn dem Beschwoerer oder jedem anderen im Radius eine volle
  Heilung fehlt — Lux ist
  reaktiv, die Vorausheilung bleibt Sache der Heiler.
- Ein Dunkelritter unter Walking Dead steht bei 1 HP in Gefaehrdungsklasse 1; Lux faellt dann sofort.
  Das ist gewollt (Punkt 1, seine Entscheidung): die aufgehobene Heilung fuer mehrere, einschliesslich
  des Tanks.
- Ob der Radius von Mitte oder Trefferflaeche gemessen wird, ist unbelegt; verwendet wird das Mass der
  Zielwahl (`GetCanAffects`, Trefferflaeche zu Trefferflaeche).

**Die Heilmenge ist gemessen, nicht aus der Potenz gerechnet, und es zaehlt die kleinste.** 500 Potenz
sind von hier nicht in Lebenspunkte umzurechnen; der Effekt-Handler sieht jede eigene Heilung mit ihrem
Wert (`Watcher.ActionFromSelf`, `DataCenter.RecordHealEffect`). **Vorgabe des Auftraggebers:** Eine
Heilung kann kritisch und damit besonders gross ausfallen, ein Schild ebenso; fuer die Prognose der
Notwendigkeit zaehlt immer das **minimale** Heilpotential, nie das maximale. Gespeichert wird deshalb die
kleinste Heilung, die das ganze Potential zeigt — eine, die weniger heilte, als dem Ziel fehlte, oder jede,
sobald das Spiel nachweislich Ueberheilung mitmeldet. Der Wert gilt bis zum naechsten Gebietswechsel (die
Gegenstandsstufensynchronisation wird je Inhalt gesetzt), nicht nur bis zum Kampfende. Vorher ist er
unbekannt, und nur die Punkte 4 und 5 greifen. Ziele ausserhalb der Gruppenliste (Chocobo, NPC ohne
die NPC-Einstellung) werden nicht gemessen. **Grenze, bewusst behalten:** Ein Ziel mit gesenkter Heilwirkung drueckt das
Minimum fuer den Rest des Gebiets (Schluss, A155). Die Folge im Kampf: Lux haelt die Heilung fuer
kleiner, als sie ist, und faellt eher — mit mehr Ueberheilung, nie spaeter. Das ist die Richtung seiner
Vorgabe (minimales Potential); ein Ausreisserfilter wuerde sie umkehren und braeuchte eine neue Zahl
(A156).

**Selbstpruefung der Messung: gemessen wird nur, was der Gesundheitsanstieg bestaetigt.** Seine Frage
dazu: „nicht bei denen messen, die damit vollgeheilt sind, weil sonst eine ueberheilung nicht
festgestellt werden kann — oder kann sie festgestellt werden?" Sie kann, wenn die Gesundheit beim Effekt
bekannt ist: Meldet das Spiel einem Ziel mehr, als ihm fehlte, meldet es die Bruttoheilung, und dann
zeigt jeder Betrag das ganze Potential. Ob die Gesundheit beim Effekt noch die vor der Heilung ist, ist
nicht belegt — der vorhandene Code (`GetPartyMemberHPRatio`) rechnet mit beiden Reihenfolgen. Deshalb
haelt `DataCenter.RecordHealEffect` jeden Wurf mit Betrag und Gesundheit je Ziel zurueck, und
`GetPartyMemberHPRatio` bestaetigt ein Ziel erst, wenn seine Gesundheit um das steigt, was die Heilung
von dort aus hinzufuegen kann (Betrag, hoechstens bis voll). Gemessen werden nur bestaetigte Ziele;
was bis zum Ende des Effektfensters (`EffectEndTime`, dasselbe, das die Heilprojektion abwartet) nicht
bestaetigt ist, faellt heraus. Kam die Aktualisierung vor dem Effekt, steigt nichts mehr, und das Ziel
wird nicht gemessen; ein Treffer dazwischen verhindert die Bestaetigung ebenso — beides die sichere
Richtung. **Grenzen:** Eine fremde Heilung im selben Fenster kann ein solches Ziel faelschlich
bestaetigen; dann ist der Fehlbetrag zu klein angesetzt, und der Wurf gilt womoeglich als Bruttomeldung,
was das Minimum senkt (Lux faellt dann eher). Ein zweiter eigener Heilwurf im selben Fenster schliesst
den ersten vorzeitig ab. Beides zeigt die Anzeige als unbestaetigte Ziele.

**Im Kampf ablesbar** (Beschwoerer-Anzeige): was die Entscheidung gerade sagt, und getrennt davon, warum
und wann sie zuletzt zum Wurf riet („last chosen") — der Wurf verbraucht Refulgent Lux, die erste Zeile
springt danach auf „no Refulgent Lux". Eine Wahl ist noch kein Wurf; ob er fiel, zeigt die Landezeile
aus dem Effekt selbst: Zeitpunkt, wie viele Ziele der Gesundheitsanstieg bestaetigte und wie viele nicht,
welcher Anteil fehlende Gesundheit traf und ob Ueberheilung gemeldet wird. Dazu die gemessene Heilmenge
und der Radius. Lux erscheint **nicht** unter „Area heal
around you": Sie wird auf den Wirkenden gezielt und laeuft nicht durch den Flaechenheil-Zweig.

**Verworfen, mit Grund:** der groesste Einzelfehlbetrag der ganzen Gruppe als Ausloeser (A150: misst
ausserhalb des Radius, zuendet bei einem Mitglied, uebergeht den Living-Dead-Traeger); die Vorhersage des
Treffers (A149: fuer eine reaktive Heilung unnoetig); der geglaettete Mittelwert der Heilmenge (ein
kritischer Treffer verschob ihn).

**Verfallsfenster:** die letzten drei GCDs von Refulgent Lux, weiter ein offener fester Wert
(`fixed_values.json`). Der Loop dazu (A154): Lux braucht einen Platz, hoechstens zwei Aktionen gehen nach
Punkt 5 vor, also drei Plaetze; bei zwei Plaetzen je Fenster sind das zwei Fenster, dazu das laufende,
dessen Plaetze schon verbraucht sein koennen — drei GCDs. Seit A155 geht nur noch Mountain Buster vor,
womit zwei GCDs reichten; der Wert bleibt, bis der Loop neu gefuehrt ist. Die Praemisse „zwei Plaetze je Fenster" ist
nicht aus dem Spiel abgeleitet, deshalb bleibt der Wert offen.

## Was ein Baustein mehrfach traegt

**Die offenen Punkte dieser Konzeptfamilie haengen an weniger Bausteinen, als ihre Zahl vermuten
laesst.** Wer einen davon baut, schliesst mehrere Punkte zugleich — das ist der Grund, die Konzepte
gemeinsam zu lesen und nicht einzeln.

**Ein Wirkungswert je Aktion, aus dem eigenen Wirktext — vier offene Punkte.**

| Offener Punkt | Konzept | Was der Wert dort beantwortet |
|---|---|---|
| Vorausschau vor dem **ersten** Treffer | hier | Wieviel Schaden der angekuendigte Einschlag traegt, bevor eine Beobachtung vorliegt |
| Wahl des Mittels nach Treffergroesse (Vorgabe 5) | hier | Welche Barriere, welche Minderung den Treffer deckt |
| Die Minderungsbilanz kennt Betaeubung und Verlangsamung nicht | hier, „Die Luecke" | Um wieviel eine Drosselung den Strom senkt — gemessen: `GetCurrentMitigationPercent` rechnet Addle, Feint, Dismantle und Reprisal, sonst nichts |
| Rueckstoss auch **als** Minderungswerkzeug | `TODO.md` | Dass seine Verlangsamung in derselben Groessenordnung wirkt wie Rampart |

Der Wert ist **erzeugbar**, nicht handzufuehren, und das ist gemessen statt vermutet: Im Lauf vom
19.09.2026 nennen **69** Wirktexte in `ActionId.resx` die Formel „reduces damage taken by X %“, mit
ausgeschriebenem Prozentsatz — 10, 15, 20, 25, 30, 40, 50 und 99 %; die Barrieren nennen ihren
Anteil ebenso (25 %, 15 %, 10 %). `RotationSolver.GameData` liest dieselben Blätter ohnehin aus. Das unterscheidet ihn von der hier verworfenen
Statussatz-Tabelle, deren Einwand die Pflege war.

**Die Schadensart aus BossModReborn ist ab sofort benutzbar, und das ist neu.** `PredictedDamageType`
kommt als `int` ueber die IPC-Grenze und wurde direkt in das eigene Enum gecastet; ob die Gegenseite
gleich nummeriert, galt als ungeprueft und nicht pruefbar. Gemessen am 20.09.2026 gegen
`BossMod/BossModule/AIHints.cs`: **beide Enums stimmen** (`None, Tankbuster, Raidwide, Shared` und
`Normal, Pyretic, NoMovement, Freezing, Misdirection`). Damit ist `BMRDamageType` eine belastbare
Groesse und keine Wette mehr — sie trennt Tankbuster von Raidwide und beantwortet damit
**Einzel- gegen Flaechenabwehr**, nicht physisch gegen magisch. Letzteres bleibt offen und ist der
Grund, warum die Minderungsbilanz Addle und Feint weiterhin binaer gewichtet.

**Der gemessene Wert einer eigenen Heilung — dieselbe Bauform, die andere Haelfte der Frage.** Das
Schadenspotential je Gegneraktion sagt, wie gross der Treffer ist; `GetObservedHealPerCast` sagt, wie
weit die eigene Antwort reicht. Erst beide zusammen beantworten Vorgabe 5 in Punkten statt in
Kategorien: Ob eine Heilung den Fehlbetrag schliesst, ob sie ueberheilt, und ob Heilung allein
reicht oder Barriere und Minderung hinzu muessen. Gebaut wurde er fuer die Zuendregel oben, er steht
aber fuer **jede** Heilaktion zur Verfuegung, weil der Effekt-Handler nicht nach Job unterscheidet.
Was er nicht beantwortet: den Wert einer Aktion, die noch nie gewirkt wurde — dafuer bleibt der
Wirktext die Quelle.

**Das gemessene Schadenspotential je Gegneraktion — drei Fragen in drei Konzepten.** Es liegt seit
A99 bis A102 vor (`13-aoe-damage-classification.md`) und wird bisher an **einer** Stelle gelesen:

| Frage | Konzept | Heute |
|---|---|---|
| Wie gefaehrlich ist der angekuendigte Flaechenschaden? | `07-heal-target-priority.md` | binaer (`IsHostileCastingAOE`) — die Groesse bleibt ungenutzt |
| Wie steht es vor dem ersten Treffer eines Pulls? | hier | blind; die Beobachtung braucht 2,5 s Anlauf |
| Wie hart schlaegt dieser Tankbuster, Rueckstoss, Stopp? | `13-…`, Abschnitt „Was der Baustein eroeffnet" | dieselbe Messstruktur, je Liste fehlt der eigene Speicher |

**Die Sonden sind der gemeinsame Nachweisweg, und sie haben selbst zu entscheiden.** Keine Regel
dieser Familie ist am Code zu belegen — ob sie im Kampf greift, zeigt erst die Laufzeit. Daraus folgt
aber **nicht**, Daten zur spaeteren Durchsicht zu sammeln: Vorgabe des Auftraggebers ist, dass eine
Sonde zur Laufzeit **erhebt und bewertet**, weil jede Auswertung ueber das Modell einen Kampf, ein
Ablesen, einen Bericht und eine Runde kostet — je Messwert. Das Vorbild steht in diesem Konzept:
`ScoreTtkForecast` haelt die eigene Vorhersage gegen den Verlauf, `GetCorrectedTTK` rechnet den
Fehler heraus, und niemand muss etwas ablesen. `AreaMitigationSkipped` ist die schwaechere Form —
sie zeigt, was die Regel verworfen hat, korrigiert sich aber nicht selbst. Wer eine Regel dieser
Familie aendert, liefert die selbstkorrigierende Sonde mit oder sagt ausdruecklich, warum hier keine
zu bauen ist.

**Nachgerechnet, mit geteiltem Ergebnis:** `searing_light_coverage.py` aus
`12-searing-light-stacking.md` beantwortet dieselbe Frage — wie viele Sekunden deckt ein Effekt ab,
wenn mehrere Quellen ihn nach Regeln zuenden. Zwei seiner Annahmen passen hier aber nicht: Es
kennt genau **eine** Aktion (Dauer und Wiederholzeit sind Konstanten), und es rechnet mit
**Ueberschreiben statt Stapeln**. Die Drosselungen dieses Konzepts sind ungleich lang und stapeln
multiplikativ — „Strecken statt stapeln“ ist hier die **Vorgabe**, nicht die Mechanik, und genau
den Vergleich beider Strategien kann ein Modell ohne Stapeln nicht fuehren. Uebertragbar ist der
Kern: Zeitschritt-Simulation mit Quellen, Dauer, Wiederholzeit und Zuendregel. Eine
Zweitverwendung verlangt also, Quellenliste und Stapelverhalten zu Parametern zu machen.

## Die Vorgaben des Auftraggebers

### Wozu die Aussetzbedingungen da sind

**Der Zweck ist die Heilbarkeit des Tanks, und der Engpass ist die Gegnerzahl.** Bei drei Gegnern
genuegt ein HoT, um den Tank zu halten; bei neun ist der eingehende Strom auch mit allen Faehigkeiten
und Zaubern kaum noch aufzuholen. Also muss der Strom gedrosselt werden - Verlangsamung, Betaeubung
durch Sanctus und was sonst zur Verfuegung steht.

**Entscheidend ist dabei nicht, wie stark gedrosselt wird, sondern wie lange.** Faellt alles zugleich,
ist die Drosselung nach wenigen Sekunden verbraucht und der volle Strom trifft einen Tank, dessen
Gruppe noch genauso gross ist. Gestreckt dagegen haelt sie so lange an, bis der eigene Schaden die
Gegnerzahl gesenkt hat - und dann traegt der Tank, was uebrig ist. Die Drosselung kauft die Zeit, in
der die Gegner sterben.

Daraus folgen beide Regeln dieses Konzepts:

- **Der Einschub nach dem ersten Stun.** Die Betaeubung durch Sanctus haelt vier Sekunden, seine
  Erholzeit betraegt zweieinhalb - ein sofort folgender zweiter Sanctus fiele also mitten in die
  laufende Betaeubung und ueberschriebe sie, statt sie zu verlaengern. Ein anderer Zauber dazwischen
  legt die zweite Betaeubung ans Ende der ersten.
- **Das Aussetzen bei fremder Drosselung.** Traegt bereits eine andere Minderung, ist die Betaeubung
  jetzt weniger wert als spaeter; sie wird aufgehoben, damit sie den Zeitraum verlaengert, statt ihn
  zu verdoppeln.

**Bedingung ueber allem, und sie gilt fuer jede dieser Regeln: ausgesetzt wird nur, solange die
Gegner ueberhaupt noch betaeubt werden koennen.** Ist die Betaeubungskette abgearbeitet und alles im
Wirkbereich immun, gibt es nichts mehr zu strecken und nichts mehr zu sparen - dann ist das Aussetzen
sinnfrei und kostet nur den Flaechenzauber. Umgesetzt ist das als `headroom` aus `SurveyStuns`, also
"mindestens ein Gegner ist weder betaeubt noch resistent", in allen drei Aussetzregeln.

**Zweite Bedingung ueber allem: der verbleibende Strom muss bewaeltigbar sein.** Der Anteil sagt, dass
gedrosselt wird - er sagt nicht, dass der Rest durchzuheilen ist. Drei Gegner bei voller Leistung
traegt ein HoT, neun laufen jeder Faehigkeit davon; dort ist die Betaeubung **jetzt** mehr wert als
spaeter, gleich wie gross der verlangsamte Anteil ist. Eine Drosselung zu strecken, die der Tank
nicht lange genug ueberlebt, um von ihr zu haben, ist kein Gewinn. Das ist die **Schranke** der
Aussetzregel und nicht ihr Ausloeser — und sie zaehlt nur, wo ueberhaupt betaeubt werden kann.

**Bewaeltigbar wird gemessen, nicht gesetzt.** Die Frage lautet nicht „wie viele Gegner stehen da",
sondern „haelt die Gruppe". `AnyPartyMemberFallingWithinHealWindow` beantwortet sie am
Gesundheitsverlauf: Faellt ein Mitglied innerhalb der Zeit, die der Einschub kostet — GCD-Rest plus
ein GCD —, wird nicht ausgesetzt. Faellt niemand, wird ausgesetzt, gleich wie gross das Paket ist.

**Warum das die Gegnerzahl schlaegt, am Spielgeschehen:** Neun Gegner, die ein Heiler im Griff hat,
sind genau die Lage, in der das Strecken der Drosselung am meisten bringt — die alte Zahl hat dort
gesperrt. Drei Gegner, die den Tank umbringen, sind die Lage, in der es nichts bringt — die alte
Zahl hat dort freigegeben. Das Maß wies in beiden Faellen in die falsche Richtung, weil es die
Gegnerseite maß statt der eigenen.

*Herkunft der ersetzten Zahl:* Die beiden Eckwerte stammen vom Auftraggeber (300 tragbar, 900
aussichtslos); der Vorgabewert 600 dazwischen war **meine** Setzung und kein Messergebnis. Als
optionaler Deckel bleibt `HoldHolyMaxHostileOutput` erhalten, Standard 0 = aus — eine Einstellung
zu entfernen verwirft einen bereits gespeicherten Nutzerwert.

### Der Grenzwert gilt fuer den gesamten Schadenseingang

**Es geht nicht um die Verlangsamung, sondern um die Kontrolle des eingehenden Schadens.** Dazu
dienen Reflexion, The Blackest Night und die uebrigen Minderungen des Tanks ebenso. Der
Schadenseingang **einschliesslich eingerechneter Schadensreduktion und Mitigation** soll zu jedem
Zeitpunkt bestimmte Grenzwerte nicht ueberschreiten.

**Gegengeprueft: die Umsetzung deckt davon eine Haelfte ab.** Der Befund steht hier vollstaendig,
weil er groesser ist als die Sanctus-Regel, an der er auffiel.

| Was zu messen waere | Stand im Baum |
|---|---|
| Drosselung **auf der Gegnerseite** — Slow, Reflexion, Feint, Stumpfsinn, Dismantle | `HostileOutputPercent`, je Satz aus dem Wirktext belegt |
| Minderung **auf der eigenen Seite**, gruppenweit — Sacred Soil, Temperance, Troubadour | `GetCurrentMitigationPercent`, aber fuer einen **einzelnen bevorstehenden Treffer** gebaut, mit Magisch/Physisch-Heuristik, nicht fuer den Dauerstrom |
| Minderung **des Tanks persoenlich** — Rampart, Bollwerk, Sentinel, Schattenwall, Vengeance, Bloodwhetting | **fehlt vollstaendig.** `StatusHelper.RampartStatus` fuehrt die Ids, wird aber ausschliesslich als `StatusProvide` benutzt, also zur Doppelbelegungssperre — nie zur Messung |
| Die Saetze dieser Minderungen | **fehlen.** `RampartStatus` ist eine reine Id-Liste; Rampart und Sentinel mindern verschieden stark. Ohne Satz je Status ist keine Rechnung moeglich; belegbar waeren sie aus den Wirktexten in `Action.resx` |

**Der ganze Anspruch ist aus Laufzeitbeobachtung erfuellbar, ohne eine einzige statische Vorgabe** —
und er ist es inzwischen. Das ist der Weg mit dem kleinsten Eingriff und der groessten Deckung, weil
die Maschinerie dafuer bereits im Baum stand; sie war nur nicht auf die Gruppe angewandt.

`DataCenter.RecordedHP` ist eine Zeitreihe von Gesundheitsanteilen **je Objekt-Id**: einmal je
Sekunde ein Eintrag, 240 tief, also vier Minuten Historie. `ObjectHelper.GetTTK` liest daraus fuer
eine beliebige Id den Verlauf, bildet einen gleitenden Mittelwert und schaetzt die Zeit bis auf null.
Die Methode fragt nichts weiter als `GameObjectId` — sie ist generisch. Gefuellt wurde die Reihe
lange ausschliesslich aus `AllHostileTargets`, weshalb sie fuer ein Gruppenmitglied `NaN` lieferte;
seit A91 nimmt `TargetUpdater.UpdateTimeToKill` die Gruppe mit auf.

**Was ein beobachteter Verlauf leistet, das eine Hochrechnung nicht leistet:** Er ist bereits netto.
Jede Minderung, jede Mitigation, jede Barriere und jede fremde Heilung stecken darin, ohne dass
irgendeine Liste gepflegt werden muesste — auch die, die es im Baum gar nicht gibt. Damit entfaellt
der ganze Bedarf an Minderungssaetzen je Status, und mit ihm die Alterung, der eine solche Liste
unterliegt.

**Und der Grenzwert wird damit relativ statt absolut.** Die Frage lautet nicht mehr „liegt der
Schaden unter X", sondern **„ist die Restzeit kuerzer als die Zeit, die meine Heilung braucht"** —
und beide Seiten sind zur Laufzeit bekannt: die Restzeit aus dem Verlauf, die Heilzeit aus der
Restzeit des GCD und der Wirkzeit des Zaubers. Damit ist keine einzige gesetzte Zahl mehr noetig,
auch nicht die Grenze zwischen „bewaeltigbar" und „aussichtslos".

**Heilung verfaelscht die Messung nicht, sie beantwortet die Frage mit.** Steigt der
Gesundheitsanteil, liefert `GetTTK` `NaN` — kein Todeszeitpunkt absehbar. Der beobachtete Verlauf ist
also der **Nettotrend** und damit unmittelbar die Antwort auf „komme ich mit dem Heilen nach": Faellt
er trotz laufender Heilung, reicht sie nicht.

**Die Groesse ist praeventiv, nicht rueckwaertsgewandt.** Bei 90 % Gesundheit meldet `GetTTK`
„in acht Sekunden tot" — das ist die Vorhersage, und sie liegt acht Sekunden vor dem Ereignis, um
das es geht. Genau darauf zielt die Vorgabe des Auftraggebers, dass **vorab** einzugreifen ist und
der Tank gar nicht erst fallen soll. Was die Groesse nicht kann, ist enger als frueher hier stand:

- **Blind fuer den ersten Treffer.** Vor 2,5 Sekunden Beobachtung (`CheckSpan`) liefert `GetTTK`
  `NaN`, und abgetastet wird einmal je Sekunde. Der Eroeffnungsschlag eines Pulls ist daraus nicht
  vorhersehbar — dafuer bleibt die Vorhersage von BossModReborn zustaendig. Ab dem zweiten Treffer
  hat die Reihe eine Rate, und die Vorausschau steht.
  **Diese Grenze ist inzwischen adressierbar, und zwar ebenfalls aus Beobachtung:** Ein angekuendigter
  Cast, dessen Schadenspotential aus frueheren Einschlaegen bekannt ist, sagt den ersten Treffer
  voraus, bevor er faellt. Konzept `13-aoe-damage-classification.md` fuehrt das aus; es ersetzt den
  unten beschriebenen Weg ueber Statussaetze durch gemessene Einschlaege und braucht damit keine
  gepflegte Tabelle.
- **Traegheit, und sie ist gemessen.** `GetTTK` misst den Abfall seit dem **ersten** beobachteten
  Wert geteilt durch die **gesamte** verstrichene Zeit — eine Durchschnittsrate ueber den Kampf,
  keine Momentanrate. Ein ploetzlicher Einbruch wird darin verwaessert, und der Fehler geht in die
  gefaehrliche Richtung: Die gemeldete Restzeit ist **zu lang**, eine Regel darauf griffe zu spaet.
  Behoben ist das nicht durch eine gesetzte Zahl, sondern durch Selbstkorrektur, siehe unten.
- **Keine Zuordnung.** Der Verlauf sagt, **dass** die Gesundheit faellt, nicht **warum**. Eine Regel,
  die entscheiden soll, ob gerade Reflexion oder Rueckstoss das richtige Mittel ist, findet die
  Antwort darin nicht.

**Die Schaetzung prueft sich selbst, und dafuer braucht es keinen externen Beobachter.** Vor einer
Sekunde hat `GetTTK` gesagt, dieses Mitglied erreiche in N Sekunden null; die soeben abgelegte
Abtastung sagt, was die Gesundheit tatsaechlich getan hat. Der Vergleich beider ist Arithmetik auf
zwei Zahlen, die das Plugin ohnehin haelt — jede Sekunde, fuer jedes Mitglied, ohne je wegzusehen.
Er leistet damit mehr, als eine berichtete Spielsitzung leisten koennte.

- **Gemessen wird ein Faktor**, kein Satz: tatsaechlicher Abfall geteilt durch vorhergesagten Abfall
  (`ObjectHelper.ScoreTtkForecast`). 1,0 heisst, der Trend hielt; 2,0 heisst, die Gesundheit fiel
  doppelt so schnell wie vorhergesagt, die Rohzahl war also doppelt zu lang.
- **Nur Abfaelle zaehlen.** Ein steigender Anteil heisst, dass eine Heilung angekommen ist; darueber,
  wie gut der Fall vorhergesagt war, sagt er nichts, also wird die Abtastung uebersprungen statt als
  negativer Beitrag verrechnet.
- **Geglaettet und begrenzt** auf 0,25 bis 4, damit eine einzelne Spitze den Faktor nicht uebernimmt.
- **Gelesen wird die korrigierte Zeit** (`GetCorrectedTTK` = `GetTTK` geteilt durch den Faktor). Die
  Korrektur kostet keine einzige gesetzte Zahl; sie faellt aus der Beobachtung.

**Bewertet wird sie neben ihrem Verbraucher, nicht vor ihm.** Rohzeit, korrigierte Zeit und Faktor
stehen je Mitglied in der Diagnoseanzeige, dazu der prognostizierte Gesundheitsanteil, den die
Regeln lesen. Bleibt der Faktor ueber einen Pull hinweg nahe 1, genuegt der schlichte Trend und die
Korrektur ist ueberfluessig; laeuft er hoch, sobald eine Gruppe anbindet, ist die Rohzahl die zu
spaete und die korrigierte die zu nehmende.

### Der Verbraucher: vorausberechnete Gesundheit statt zweiter Mechanismus

**Der Fehler der heutigen Heilkette ist eine Verwechslung von Pegel und Rate.** Jede Schwelle —
`HealthSingleAbility`, `HealthTankRatio`, `HealthForDyingTanks` — vergleicht einen **Stand**. Die
Gefahr ist aber ein **Zufluss**: Wer schnell faellt, unterschreitet seine Schwelle mit weniger
Restzeit, als die dadurch ausgeloeste Heilung zum Ankommen braucht — GCD-Rest, dann Cast. Der Zauber
geht nach dem Tod heraus. Bei schwachem Zufluss ist dieselbe Schwelle frueh genug; die Korrektur
muss deshalb mit der Rate skalieren und darf kein fester Abschlag sein.

**Gewaehlt ist, die gelesene Groesse zu ersetzen, nicht einen Mechanismus danebenzustellen.** Die
Frage einer Regel lautet nicht mehr „wie steht dieses Mitglied", sondern „wie steht es, wenn meine
Heilung ankommt". Damit erben Schwellen, Rangstufen und Kurzschluesse die Vorausschau, ohne dass
einer von ihnen umgebaut wird.

**Das Tor entscheidet vor allen anderen, und es wurde beim Bauen zuerst uebersehen.**
`FindHealTarget` verwirft jeden Kandidaten, dessen Gesundheit nicht unter `AutoHealRatio` liegt
(Vorgabewert 0,8), **bevor** Rangstufe und Kurzschluesse ihn je sehen. Auf der schlichten Groesse
gelesen faellt damit genau der Fall heraus, fuer den die Vorausschau gebaut ist: ein Tank bei 90 %,
der in sechs Sekunden bei null ist. Die uebrigen vier Lesestellen haetten weiterhin richtig
ausgesehen, waehrend die Wirkung vollstaendig ausgeblieben waere. Der Filter liest deshalb ebenfalls
die Vorausschau — das ist keine Ueberheilung, denn die Grenze soll einen Zauber davor bewahren, an
jemanden zu gehen, der ihn nicht braucht, und wer beim Landen bei 34 % steht, braucht ihn.

```
Anteil = max(0, 1 − Vorlaufzeit / korrigierte Restzeit)
Vorlaufzeit (GCD-Heilung)  = GCD-Rest + ein voller GCD
Vorlaufzeit (oGCD-Heilung) = max(verbleibende Ausfuehrungssperre, Restwirkzeit eines laufenden Zaubers)
```

Beides aus dem Spielzustand, keine gesetzte Zahl. Ein Heiler unter Presence of Mind blickt kuerzer
voraus — richtig, er kann frueher handeln. Eine oGCD-Heilung landet, sobald Ausfuehrungssperre und
ein laufender Zauber sie freigeben — ueber einen Zauber hinweg laesst sich nichts einweben. Mit dem
GCD-Vorlauf fielen Benediction, Essential Dignity oder Druochole bis zu einen GCD zu frueh und fehlten
danach einem echten Notfall. Durchgerechnet: korrigierte Restzeit des Tanks 10 s, GCD 2,5 s zur Haelfte
abgelaufen. GCD-Vorlauf 3,75 s, Anteil 0,625: Ein Tank bei 48 % liest sich als 30 %, und eine
Benediction-Schwelle von 30 % fiele jetzt statt in knapp vier Sekunden. Vorlauf 0,5 s (Sperre): liest
sich als 46 %, sie faellt, wenn er wirklich dort ankommt. Der Ursprung (1026d5f37, „the rest of the GCD,
then the cast") war fuer GCD-Heilungen gebaut; oGCDs kamen darin nicht vor. Der Text der Einstellung
verspricht „the health … by the time a heal started now would land" und bindet (A213). Unterschieden
wird an allen Lesern: Heilflaggen (Faehigkeit gegen Zauber), Heilzielwahl (`IsRealGCD` der Aktion) und
die Schwellen der Jobrotationen. Die kritische Klasse behaelt den GCD-Vorlauf, weil sie fuer
Heilung, Abwehr-Halt und Lux Solaris dieselbe Definition ist.

**Heilart heisst, wann die Heilung landet — nicht, ob die Aktion ein oGCD ist** (A221). Vier oGCDs
heilen erst mit dem naechsten GCD und blicken deshalb wie eine GCD-Heilung voraus, bei der Schwelle wie
bei der Zielwahl (`ActionSetting.HealsWithNextGcd`, gesetzt in den Basisrotationen):

| Aktion | Wirktext (Job-Guide) | Womit die Heilung landet |
|---|---|---|
| Synastry | „the bond will also recover HP equaling 40% of the original spell" | mit der naechsten Einzelheilung (Benefic, Benefic II, Aspected Benefic) |
| Krasis | „Increases HP recovery via healing actions … by 20%" | mit der Heilung danach; der Weise wirkt es nur vor einer Heil-GCD |
| Soteria | „increasing the cure potency of Kardion effects … by 70%" | mit Kardion, das beim naechsten Schadenszauber heilt |
| Emergency Tactics | wandelt den Schild der naechsten Succor/Adloquium in Heilung | mit dieser GCD |

Mit dem kurzen Vorlauf lasen ihre Schwellen den Stand bei Ende der Ausfuehrungssperre, ihre Heilung
landet aber einen GCD spaeter: Bei steilem Verlauf fielen sie zu spaet. In der Zielwahl wirkt das Merkmal
nur, wo sie ueber das Heilziel waehlt: Krasis im Heilpfad (Override `Heal`) waehlt dann wie die Heil-GCD,
die es verstaerkt. Synastry laeuft in `EmergencyAbility` vor jedem Override und ohne eigenen `TargetType`
(Vorgabe `Big`); ihre Zielwahl liest den Vorlauf nicht. Beim
kurzen Vorlauf bleiben die oGCDs, die sofort wirken: Benediction, Essential Dignity, Taurochole,
Druochole, Haima, Excogitation (liegt bereit und loest selbst aus), Aetherpact, Second Wind, Equilibrium,
Thrill of Battle; ebenso Raw Intuition/Bloodwhetting und Nascent Flash, deren Minderung und Barriere
sofort wirken und deren Heilung mit jedem Waffenskill folgt. Bloodbath („Converts a portion of physical
damage dealt into HP") heilt mit dem naechsten physischen Treffer; zaehlen Autoangriffe dazu
(Schluss, nicht belegt), liegt der kurze Vorlauf naeher als ein voller GCD, sonst um hoechstens den GCD-Rest
daneben.

**Verworfen: ein zweiter Ausloeser samt eigener Rangstufe.** Er waere der naheliegende Weg gewesen
und ist der schlechtere: Zwei Mechanismen, die dieselbe Frage entscheiden, laufen auseinander, sobald
einer von beiden angefasst wird. Ausserdem haette er zwei Haelften gebraucht, die einzeln wirkungslos
sind — ein Ausloeser ohne Zielwahl heilt den Falschen, eine Zielwahl ohne Ausloeser greift erst,
wenn ohnehin geheilt wird.

**Selbstbegrenzend im teuren Fall.** Steht die Gruppe stabil, ist der Nettotrend nicht fallend,
`GetTTK` liefert `NaN` und der Anteil ist 1 — kein Unterschied zu heute, kein zusaetzlicher Zauber,
kein MP. Die Vorausschau erscheint genau dann, wenn der Trend nach unten dreht, und waechst mit
seiner Steilheit.

**Was im Kampf anders wird, durchgerechnet.** GCD 2,5 s, halb abgelaufen, Vorlaufzeit 3,75 s.

| Lage | Heute | Mit Vorausschau |
|---|---|---|
| Tank 90 %, korrigierte Restzeit 6 s (Anteil 0,375 → 34 %) | Heilung faellt erst bei 45 % real, rund 3 s spaeter | Heilung faellt sofort — ein GCD Vorsprung |
| Tank 44 %, Restzeit 20 s (→ 36 %) · Schwarzmagier 48 %, Restzeit 4 s (→ 3 %) | Tank-Kurzschluss greift bei 44 ≤ 45, der Magier stirbt | Der Magier faellt mit 3 % in die kritische Rangstufe und wird davor abgefangen |
| Gruppe stabil bei 80 %, kein Nettoabfall | — | — (identisch) |
| Zwei Mitglieder, beide Restzeit unter der Vorlaufzeit (Anteil 0) | — | Beide auf 0 Punkten; es entscheidet die Rolle: Heiler vor Tank vor Schadensausteiler |

Die letzte Zeile ist die Rangfolge des Auftraggebers, und sie faellt hier von selbst an der Stelle an,
an der er sie haben will: **bei gleicher Gefaehrdung**, nicht davor.

**Zwei benannte Ungenauigkeiten.**

- *Die Barriere wird mitskaliert.* `GetForecastEffectiveHp` multipliziert effektive Punkte
  einschliesslich Schild mit einem Anteil, der aus dem **Gesundheits**verlauf ohne Schild stammt.
  Solange der Schild traegt, faellt die Gesundheit nicht, der Anteil ist 1 und nichts geschieht; der
  Fall „Schild vorhanden **und** Gesundheit faellt" tritt nur auf, wenn eine frische Barriere auf
  einen noch fallenden Trend trifft. Dann unterschaetzt die Rechnung den Puffer, also in die sichere
  Richtung.
- *Kein Flatterschutz.* Greift die Heilung, steigt die Gesundheit, die Restzeit wird `NaN` und die
  Vorausschau faellt weg. Ein begonnener Cast wird davon nicht abgebrochen, und „heilen, bis es
  reicht" ist das gewollte Verhalten — eine Hysterese ist deshalb nicht gebaut, aber auch nicht
  gemessen.

**Standard an** (seit 29.09.2026, seine Regel: Voreinstellung ist der im Kampf sinnvollere Wert). Belegt ist die
Wirkung mit den hier verfuegbaren Mitteln nicht — statische Pruefung und Kompilierung sagen nichts darueber, ob
der Tank steht. Bei ausgeschalteter Einstellung liefern alle vier Getter die Werte ohne Vorausschau, die
Nullvariante ist also eingebaut.

**Die Groesse, die den Anspruch unmittelbar erfuellen wuerde, existiert bereits — und ist unbrauchbar
gebaut.** `DataCenter.DPSTaken` misst den **tatsaechlich angekommenen** Schaden, also bereits nach
allen Minderungen und Mitigationen; hochrechnen muesste man dafuer gar nichts. Ihr Zeitfenster
betraegt jedoch fuenf Millisekunden, waehrend ein Bild rund sechzehn dauert — die Groesse sieht damit
fast immer nichts. Sie ist Upstream-Code, und ihr einziger Leser im Baum ist die Diagnoseanzeige.

**Eine Barriere gehoert in den Zaehler, nicht in den Nenner.** The Blackest Night senkt die Rate
nicht, es absorbiert eine Menge: Der Strom laeuft unveraendert weiter, er trifft nur zuerst den
Schild. Als **Faktor** der Ratenrechnung waere sie deshalb falsch — sie drosselt nichts.

**Sie bewertet aber sehr wohl, ob der Tank ueberlebt und ob genug Zeit zum Heilen bleibt**, und das
ist der Punkt, an dem sie zaehlt. Die Groesse, um die es geht, ist die Zeit bis zum kritischen
Zustand: **Puffer geteilt durch Rate**. Die Barriere vergroessert den Puffer, sie verkleinert die
Rate nicht — also Zaehler, nicht Nenner. Genau daran haengt die Frage, die eine Grenzwertregel
eigentlich stellt: Nicht „ist der Zufluss hoch", sondern „reicht die Zeit, die er mir laesst, fuer die
Heilung, die ich brauche".

Dieselbe Trennung traegt bereits zwei bestehende Entscheidungen, und sie ist dieselbe in beide
Richtungen: In der **Heilschwelle** zaehlt die Barriere nicht, weil sie den Heilbedarf nicht senkt
(A85) — in der **Ueberlebensfrage** zaehlt sie, weil sie Schaden abfaengt. `GetEffectiveHp` fuehrt
sie deshalb, und die kritische Rangstufe der Heilzielwahl liest genau diese Groesse (Konzept 07).

**Was zur Zeitrechnung fehlt, ist der Nenner, nicht der Zaehler.** Der Puffer einschliesslich
Barriere ist vorhanden und wird gelesen; die Rate je Mitglied ist es nicht — derselbe fehlende
Baustein wie bei der Heilzielwahl. Ohne ihn ist „reicht die Zeit" nicht zu berechnen, und die
Barriere bleibt auf ihre heutige Rolle beschraenkt: Sie hebt den Puffer, aus dem die
Ueberlebensbewertung ihre Antwort zieht.

**Die Restzeit der Barriere selbst waere die zweite Haelfte dieser Frage.** `StatusHelper.HasSurvivingShield` wurde
dafuer gebaut und hat heute keinen Leser; ihr bekannter Defekt — sie misst die **kuerzeste**
Schildrestzeit statt der laengsten — steht in `TODO.md`. Wer die Zeitrechnung baut, loest ihn mit,
denn eine Barriere, die vor der Heilung ausläuft, kauft keine Zeit.

**Der scheinbare Zielkonflikt mit der Barrierenregel ist aufgeloest, und zwar durch eine Rangregel
des Auftraggebers: Heilung vor Minderung.** Seine Vorgabe zu The Blackest Night bleibt unveraendert —
bei einem Gruppenpull wird waehrend der Barriere keine Minderung gewirkt, damit der Schild moeglichst
immer vollstaendig aufgezehrt wird. Der Traeger soll den Strom also **abbekommen** und ihn zugleich
**ueberleben**, und das geht allein ueber die Heilung: Jede Minderung nimmt genau den Strom weg, der
den Schild brechen soll, eine Heilung nimmt ihm nichts.

**Damit ist auch die Richtung der Grenzwertregel festgelegt.** Wird ein Grenzwert ueberschritten, ist
die erste Antwort die Heilung und nicht die Minderung; gemindert wird, wo die Heilung nicht reicht.
Gegengeprueft und bereits erfuellt: In beiden Dispatch-Pfaden steht die Heilung vor der Verteidigung
(`HealAreaAbility`/`HealSingleAbility` vor `DefenseAreaAbility`/`DefenseSingleAbility`, im GCD-Pfad
ebenso), sodass bei gleichzeitig gesetzten Zustaenden die Heilung ohne weiteres Zutun gewinnt.

### Die Aussetzbedingung ist ein Anteil, mit Schranke

**Sanctus wird aufgeschoben, solange mehr als die Hälfte der Gegner im Wirkbereich verlangsamt ist
und mindestens drei von ihnen den Slow tragen.** Beide Teile sind seine Angabe, und beide haben eine
eigene Aufgabe: Der **Anteil** sagt, dass der Strom als Ganzes gedrosselt ist und nicht ein
Nachzügler; die **Mindestzahl** verhindert, dass ein Rest von zwei Gegnern den Anteil rechnerisch
erfüllt. Aufgeschoben heißt aufgeschoben, nicht aufgegeben — sobald die Bedingung nicht mehr
zutrifft, fällt Sanctus wieder.

**Gehalten wird, solange die Bedingung gilt, ohne Ersatzvorbehalt** (A178). Seine Beobachtung vom
27.09.2026: „nach abtausch wird holy wie gewünscht ausgesetzt, bis alle gegner gemach haben. dann setzt
holy aber wieder ein" (Gemach ist der deutsche Name des Slow-Status 9). Die Regel trug bis dahin die
Ersatzgarantie der Streckung: ausgesetzt nur, solange Dia oder Aero noch ein Ziel ohne DoT hat. Während
des Aussetzens legt der Weißmagier aber genau diese DoTs, einen je GCD; nach einem DoT je Gegner war
der Vorbehalt verbraucht, und Sanctus fiel in die noch laufende Verlangsamung. Die Streckung trägt
die Garantie seit A231 ebenfalls nicht mehr; sie war in beiden Regeln meine Abwägung. Jetzt fällt
der gehaltene GCD auf die DoTs, solange einer fehlt, danach auf Glare. **Preis, im Kampf:** Ab drei
Gegnern trifft Sanctus in Summe mehr als Glare; jeder gehaltene GCD nach dem letzten DoT kostet diese
Differenz an Schaden, und der Pull dauert entsprechend länger. Dafür steht die Betäubung zur
Verfügung, wenn die Verlangsamung abläuft — das ist der Zweck seiner Regel. Die Schranke (ein
Gruppenmitglied fällt) und der abschaltbare Deckel bleiben; das Diagnosefenster nennt unter der
Rotation, ob und warum zuletzt gehalten wurde.

Umgesetzt als `WHM_Reborn.ShouldHoldHolyWhilePackSlowed` über `SurveyHostileStatus` im Wirkbereich
von Sanctus; die Mindestzahl steht als `HoldHolyMinSlowedHostiles` (Vorgabewert 3) hinter derselben
Einstellung. Streng mehr als die Hälfte: 3 von 5 hält, 3 von 6 hält nicht, 4 von 6 hält.

**Die vorige Messgröße war Leistung statt Kopfzahl, und sie hat im Spiel nichts bewirkt.** Sie
summierte die Restleistung aller Gegner im Radius gegen `AoeCount * 100` — die Flächenschwelle in
der Einheit, in der sich Minderungen ausdrücken lassen, und sie ging ebenfalls auf eine Korrektur
des Auftraggebers zurück (C52). Ihre eigene Dokumentation hielt bereits fest, dass sie „nur bei
genau `AoeCount` Gegnern greift": Ein Gegner mehr trägt für sich mindestens 80 und hebt die Summe
über die Schwelle, gleich wie viele verlangsamt sind. Ein Wall-to-Wall-Pull hält immer mehr Gegner,
als der Zauber braucht — dort ist die Regel nie eingetreten. Genau das hat der Auftraggeber
beobachtet: Sanctus fiel weiter, obwohl der Slow fast alle Gegner erfasst hatte. Fünf Gegner mit
vier verlangsamten ergaben 432 gegen eine Schwelle von 300.

**Der Fehler war nicht die Zahl, sondern die Frage.** Die Leistungssumme beantwortet „lohnt sich für
diesen Pull noch ein Flächenzauber", und die Antwort lautet fast immer ja. Die Regel hat aber zu
fragen: „wird der Strom bereits gebändigt" — und das ist ein Anteil, keine Summe.

**Wo der eigene Anteil daran liegt, und er liegt nicht bei der Wahl des Maßes:** Die Einschränkung
„greift nur bei genau `AoeCount` Gegnern" wurde bei der Umsetzung erkannt und als **richtige
Eigenschaft** ausgeschrieben — „that narrow reach is correct, not a shortfall". Damit war der
Nachweis, dass die Regel im Regelfall wirkungslos ist, bereits geführt und ist gleichwohl nicht
vorgelegt worden. Eine erkannte Bedingung, unter der ein Eingriff im gesamten maßgeblichen Bereich
nichts tut, ist keine Eigenschaft, sondern seine Widerlegung, und gehört dem Auftraggeber
vorgetragen, bevor sie im Spiel auffällt.

**Die Leistungsrechnung ist zweimal versetzt worden und steht jetzt ganz hinten.** Zuerst war sie der
Auslöser der Regel — dort maß sie die falsche Frage und die Regel griff im Wall-to-Wall nie (C59).
Dann war sie die Schranke am Ende, mit einem von mir gesetzten Trennwert. Heute ist die Schranke
gemessen (`AnyPartyMemberFallingWithinHealWindow`), und die Leistungsrechnung bleibt als
**abschaltbarer Deckel** darüber, Standard aus. `HostileOutputPercent` und `SurveyHostileOutput`
messen unverändert, was sie immer gemessen haben; ihre Faktoren stammen aus den Wirktexten, die
Verlangsamung als einzige Nicht-Minderung über die Angriffsrate umgerechnet (C57).

*Warum sie nicht ganz entfällt:* Sie ist eine gespeicherte Nutzereinstellung. Sie zu entfernen
verwürfe einen Wert, den der Auftraggeber möglicherweise gesetzt hat, und die Rechnung selbst ist
richtig — falsch war nur, sie über die Gegnerseite entscheiden zu lassen, wo die eigene Seite die
Frage beantwortet.

**Die Betäubung bleibt aus der Bedingung heraus.** Sie wäre die stärkste Drosselung überhaupt — ein
betäubter Gegner trägt null —, aber ihre Frage ist eine zeitliche: Die Betäubung dauert länger als
der Recast, ein zweiter Sanctus überschriebe sie, statt sie zu verlängern. Eine Momentaufnahme kann
das nicht ausdrücken; sie sagt „betäubt, also nicht wirken", während die Streckung gerade **später
wieder** betäuben will. Beide Regeln stehen deshalb nebeneinander und nicht ineinander.

**Grenze der Zählung, benannt statt verschwiegen:** `SlowStatus` führt zwölf Ids einschließlich
Slow+, und die Erhebung unterscheidet sie nicht. Für eine Anteilsregel ist das ohne Belang — gezählt
wird, ob ein Gegner verlangsamt ist, nicht wie stark. Welche Id Abtausch tatsächlich setzt, ist von
hier aus nicht zu bestimmen; bliebe die Regel im Spiel weiterhin wirkungslos, wäre das die nächste
zu prüfende Ursache.

## Warum die Streckung richtig ist

### Die Größenordnung

Die Betäubungsressource ist **einmalig je Pull**, nicht wiederkehrend: Erste
Anwendung 4 s, zweite 2 s, dritte 1 s, danach 45 s Immunität. Ein stehender
Gruppenpull ist selten länger als diese 45 s, das Zurücksetzen der Resistenz fällt
also praktisch nicht mehr in den Kampf. Zu verteilen sind damit genau **7 Sekunden
Betäubung**, und die Frage lautet nicht, wie viele Zyklen man unterbringt, sondern
wie man diese sieben Sekunden legt.

**Posten 1 — Ausnutzung der Dauern.** Bei einem GCD von rund 2,5 s deckt
ununterbrochenes Nachcasten 0–4,5 s und 5,0–6,0 s ab, zusammen etwa **5,5 s**: Die
zweite Anwendung fällt, während die erste noch läuft, und ihre kürzere Dauer verfällt
teilweise. Wird jeweils erst nach Ablauf nachgecastet, stehen die vollen **7 s** zur
Verfügung.

**Posten 2 — Entzerrung gegen fremde Mitigation.** Reprisal senkt den Gegnerschaden
um 10 % für 10 s. Liegt die Betäubung darin, ist ihr Reprisal-Anteil verloren:
7 s × 10 % = **0,7 s** je Gegner.

| | überlappend | entzerrt |
|---|---|---|
| Betäubungsdauer genutzt | 5,5 s | 7,0 s |
| Reprisal-Anteil erhalten | 0,3 | 1,0 |
| **Summe je Gegner** | **5,8** | **8,0** |

Posten 1 ist damit mehr als doppelt so schwer wie Posten 2: Die Streckung trägt den
Nutzen, die Abstimmung mit fremder Mitigation ist ein Zusatz. Modell, keine Messung —
Annahmen: gleichmäßiger Schaden, stehender Kampf, alle Werkzeuge verfügbar,
Nachcasten im GCD-Takt.

### Die Gegnerzahl gibt den Ausschlag nicht

Naheliegend ist, der Einschub lohne mit steigender Gegnerzahl zunehmend. Das trifft
für den absoluten Betrag zu, für die Entscheidung nicht: **Der Preis skaliert mit
derselben Zahl.** Betäubungsgewinn und Sanctus-Verlust wachsen beide linear mit der
Gegnerzahl, ihr Verhältnis bleibt konstant. Nicht mitskaliert nur der DoT-Anteil: Ein
Einzelziel-DoT bringt unabhängig von der Gegnerzahl denselben Betrag, während sein
Preis mit ihr steigt.

| Posten | Nutzen skaliert mit n | Preis skaliert mit n | Folge |
|---|---|---|---|
| Betäubungsstreckung | ja | ja | gegnerzahl-neutral |
| DoT-Einschub | nein | ja | lohnt bei **wenigen** Zielen |

Die Gegnerzahl bleibt deshalb als **Untergrenze** sinnvoll — unterhalb von drei Zielen
fällt Sanctus wegen `AoeCount` ohnehin nicht, und Einzelziele sind meist
betäubungsimmun. Sie ist ein Filter, kein Gewicht.

### Vermiedener und erzeugter Schaden sind nicht verrechenbar

Die Aufgabe eines Heilers ist, Tank und Gruppe am Leben zu halten; Schaden ist
wichtig, steht aber im Rang darunter. Die beiden Größen bilden eine **Rangordnung,
keine Verrechnung**: erst Überleben sichern, dann Schaden maximieren.

Daraus folgt eine Umkehrung der Beweislast. Die gestreckte Betäubung liefert mehr
Deckung als das Nachcasten im GCD-Takt — 7 s gegen 5,5 s — und ist damit in einer
Rangordnung mit Schadensvermeidung an erster Stelle **immer** die richtige Wahl,
sobald sie anwendbar ist. Nicht die Streckung braucht eine Rechtfertigung, sondern
ihr Unterlassen.

Zwei Einschränkungen stehen dagegen und dürfen nicht aufgelöst werden:

- **Mitigation hat keinen linearen Wert.** Sie ist folgenlos, solange niemand stirbt,
  und entscheidend, wenn sie einen Tod verhindert. Im gut gehaltenen Trash-Pull
  vermeidet die Streckung Schaden, den niemand gespürt hätte — die Rangordnung greift
  dort ins Leere, schadet aber auch nicht.
- **Schaden ist selbst eine Form der Schadensvermeidung.** Ein schneller Kill verkürzt
  den Kampf. Der Anteil des Heilers am Gruppenschaden ist allerdings klein, weshalb
  dieser Rückkopplungseffekt den Vorrang nicht umkehrt.

**Warum die Architektur das nicht von selbst löst.** Der Dispatch prüft Heilung und
Verteidigung vor dem Schadenszweig, setzt die Rangordnung also bereits um — aber nur
**reaktiv**: Ein Heilflag entsteht, wenn HP fehlen. Eine Betäubung ist Prävention und
wirkt, bevor Heilbedarf sichtbar wird. Für Prävention hat die Architektur keinen Ort,
und deshalb liegt die Betäubung im Schadenszweig — nicht weil sie nachrangig wäre,
sondern weil sie dort mangels Alternative gelandet ist. Das ist der eigentliche Befund
hinter der gesamten Frage.

## Die Lücke

**Aktionen mit doppelter Wirkung sind nur nach einer ihrer Wirkungen eingeordnet.**
Sanctus steht im Schadenszweig; dass es betäubt, ist im Entscheidungsmodell nicht
vorhanden. Assize steht im Angriffs-oGCD (`WHM_Reborn.AttackAbility`); dass es heilt,
ebenfalls nicht. Umgekehrt kennt `GetCurrentMitigationPercent` die Wirkung von
Reprisal, aber weder Betäubung noch Verlangsamung, obwohl beide wie
Schadensreduktion wirken. Beide sind inzwischen an der Stelle gelesen, an der sie
eine Entscheidung ändern — der Barrierenregel des Dunkelritters —, in der
Minderungsbilanz selbst aber weiterhin nicht.

Die Erhebung dazu ist geführt und liegt als `scan16.py` im Repository: Sie nimmt
jede PvE-Aktion, deren Wirktext eine Kontroll- oder Minderungswirkung auf Gegner
nennt, und fragt, ob der Baum den zugehörigen Status irgendwo liest. Im
Tank- und Heilerprofil bleibt **eine** Aktion übrig, deren zweite Wirkung
nirgends gelesen wurde: **Abtausch (Arm’s Length)**. Sie ist als Rückstoßschutz eingeordnet
(`CustomRotation_Ability.AntiKnockbackAbility`), legt aber zugleich
Verlangsamung +20 % auf jeden physischen Angreifer für 15 Sekunden. Der Wirktext
der Verlangsamung nennt ausdrücklich die Verzögerung der **Automatikangriffe**,
aus denen Trash-Gegner den Großteil ihres Schadens liefern — die Wirkung liegt
damit in derselben Größenordnung wie Rampart. Die übrigen Treffer der Erhebung
liegen in Bozja und den Tiefen Gewölben, also außerhalb des Nutzungsprofils, und
sind erfasst statt bearbeitet.

Zweite, unabhängig belegte Fundstelle: Im Schadenszweig steht der Sanctus-Block
**vor** dem DoT-Block. Sobald `AoeCount = 3` erfüllt ist, greift Sanctus und der DoT
wird nie gesetzt — bei einem Boss mit zwei Adds läuft also nie Dia. Dass das nicht
gewollt ist, zeigt der DoT selbst: `ModifyDiaPvE` (`WhiteMageRotation.cs:300-311`)
setzt `TargetStatusProvide` gegen Nachlegen und `IsRestrictedDOT` gegen ungeeignete
Ziele — beide Vorkehrungen laufen bei AoE ins Leere, weil der Zweig nicht erreicht
wird.

## Die Regel

**Seine Vorgabe für den Einschub, ohne Ermessensspielraum:** Die Betäubung durch Sanctus hält vier
Sekunden, seine Erholzeit beträgt zweieinhalb; ein sofort folgender zweiter Sanctus fiele mitten in
die laufende Betäubung und überschriebe sie. Ein anderer Zauber dazwischen legt die zweite Betäubung
ans Ende der ersten. Darüber steht seine Bedingung für jede Aussetzregel: ausgesetzt wird nur,
solange die Gegner überhaupt noch betäubt werden können (A90).

> Unmittelbar nach einem Sanctus geht der nächste GCD an einen anderen Zauber, wenn die Betäubung
> dieses Sanctus noch liefe, sobald ein jetzt begonnener zweiter Sanctus landet.

Der eingeschobene GCD fällt an den DoT, wo einer fällig ist, sonst an Glare. Weitere Bedingungen
trägt die Regel nicht: keine Gegnerzahl außer der Flächenprüfung von Sanctus selbst, keine
Abwägung gegen den Schaden, keine Ausnahme für einen nachrückenden Gegner.

Die beiden anderen Aussetzregeln — Aussetzen bei fremder Drosselung (Verlangsamung) und bei der
Barriere des Dunkelritters — stehen unter „Die Vorgaben des Auftraggebers" und in Konzept 10.

### Ein einziger Einschub genügt

Sanctus hat eine Wirkzeit, und die Betäubung beginnt, wo der Zauber landet. Bei GCD und Wirkzeit
von je 2,5 s und Betäubungsdauern von 4 / 2 / 1 s:

| Verlauf | Betäubungsdeckung | Eingeschobene GCDs |
|---|---|---|
| ohne Einschub | **5,5 s** (2,5–7,0 · 7,5–8,5) | 0 |
| Einschub, solange die Betäubung den nächsten Sanctus überdauert | **7,0 s** (2,5–6,5 · 7,5–9,5 · 10,0–11,0) | 1 |
| Einschub, solange irgendeine Betäubung läuft | **7,0 s** (2,5–6,5 · 10,0–12,0 · 15,0–16,0) | 3 |

Nach der zweiten Anwendung (2 s) landet der nächste Sanctus ohnehin erst nach deren Ende; ein
Einschub dort kostet einen GCD und bringt nichts. Unter Presence of Mind (GCD und Wirkzeit 2,0 s)
liefert die Regel lückenlose Deckung von 2 bis 9 s, ebenfalls mit einem Einschub. Die Zahlen stammen
aus `.github/scripts/audit/stun_coverage.py` (Aufruf mit GCD und Wirkzeit), das die Regel GCD für
GCD simuliert und einen Selbsttest trägt.

### Warum die Bedingung so lautet, wie sie lautet

**Nach dem Sanctus, nicht nach irgendeiner Betäubung.** Seine Vorgabe spricht von zwei Sanctus und
einem Zauber dazwischen; die Regel fragt deshalb `IsLastGCD` nach Sanctus. Nach dem eingeschobenen
Zauber geht der nächste Sanctus hinaus, gleich was läuft. Eine Betäubung durch den Tank (Low Blow
auf einem Gegner) hält Sanctus damit nicht fest.

**Gegen die Landung, nicht gegen null.** Die Restzeit der Betäubung wird mit der Zeit verglichen,
bis ein jetzt begonnener Sanctus landet: Rest des GCD plus Wirkzeit (`GetCastTime`, angepasst, also
mit Presence of Mind). Gemessen wird die längste laufende Betäubung im Wirkradius — überschrieben
würde jede, die die Landung überdauert.

**Die Betäubung, die noch nicht auf den Gegnern steht.** Wirkzeit und Erholzeit sind gleich lang;
der nächste GCD ist in dem Moment frei, in dem der erste Sanctus landet, und seine Betäubung steht
dann womöglich noch nicht auf den Gegnern. Ein Gegner im Radius, der weder betäubt noch resistent
ist, bekommt gleich die erste, volle Anwendung. Sie zählt mit der Dauer aus dem Wirktext
(`DefensiveValues.DurationOf`, Sanctus 4 s). Die frühere Regel las an genau dieser Stelle „keine
Betäubung" und gab Sanctus frei (Schluss aus Code und Wirkzeit, im Spiel nicht beobachtet).

**Seine Bedingung über allem.** Steht im Radius weder eine laufende Betäubung noch ein noch
betäubbarer Gegner, gibt es nichts zu strecken; Sanctus geht hinaus. Gegner mit Resistenz zählen
nicht als frisch, weil ihre nächste Anwendung kürzer ist und nach dem Modell nicht mehr überlappt.

**Über den Wirkradius, nicht über die Jobreichweite.** Gemessen wird im Wirkradius der Aktion
(`HolyIiiPvE.Info.EffectRange`), nicht in `DataCenter.JobRange` — für einen Heiler sind das 25 Yalms
Angriffsreichweite. Über die Jobreichweite gemessen hätten ferne, nie betäubte Gegner als frisch
gegolten.

**Über eine Statusgruppe, nicht über eine einzelne Id.** Die Spieldaten führen `Stun` und rund ein
Dutzend Varianten mit Zahlensuffix; welche davon Sanctus anlegt, ist offline nicht bestimmbar.
`StatusHelper.StunStatus` fasst sie nach dem im Projekt etablierten Muster zusammen.

**Ausgeschlossen, weil sie seine Vorgabe einschränkten** (bis 01.10.2026 im Code, A231):
- *Ersatzgarantie* — Sanctus nur ausgesetzt, wenn ein DoT den GCD übernimmt (A19, meine Abwägung
  „Glare wäre ein reiner Verlust"). Seine Vorgabe sagt „ein anderer Zauber"; Glare ist einer.
- *Ausnahme für einen unbetäubten, noch betäubbaren Gegner im Radius* — meine Ableitung, dass ein
  Neuzugang die volle Dauer sofort bekommen soll. Sie hob die Streckung im gestaffelten Pull fast
  immer auf.
- *Mindestzahl `StretchHolyMinHostiles`* für die Streckung — Sanctus prüft seine Zielzahl selbst.
  Die Einstellung gilt weiter für das Aussetzen bei der Barriere des Dunkelritters.
- *H2 aus A19*, die Ablehnung von `Restzeit > ein GCD`, rechnete ohne Wirkzeit. Mit Wirkzeit gleich
  GCD ist sie dieselbe Bedingung wie die jetzige.

**Seine Beobachtung (29.09.2026): Das zweite Sanctus fiel sofort, statt die Betäubung auslaufen zu
lassen.** Die Regel war seit dem 10.09. unverändert. Drei ihrer Wege ließen Sanctus sofort fallen und
sind entfernt: die Ersatzgarantie, die Ausnahme für einen Neuzugang, die Mindestzahl; dazu der
wahrscheinlichste, der Vergleich mit einer Betäubung, die bei der Entscheidung noch nicht auf den
Gegnern stand. `DefenseTrace.log` nennt je Entscheidung nach einem Sanctus den Grund mit Restzeit und
Landezeit.

## Vorhandene Bausteine

Der Entwurf erfindet wenig; das meiste lag im Baum und war nur nicht verbunden.

| Baustein | Fundstelle | Zustand |
|---|---|---|
| Mitigationsmessung: Gegner-Debuffs und Party-Buffs, verrechnet zu einem Schadensfaktor | `CustomRotation_OtherInfo.cs:534` | Gelesen nur zur Anzeige (`RotationConfigWindow.cs:4863`) |
| Betäubung, Verlangsamung und **deren Resistenzen** als Statuseffekte | `StatusID.Stun`, `.StunResistance`, `.Slow`, `.SlowResistance`, `.ArmsLength` | In den Spieldaten vorhanden; die Resistenzstufe ist damit direkt lesbar, eine eigene Buchführung über den Ereignisstrom ist **nicht** nötig |
| Statusabfragen mit Restzeit und Stapelzahl | `StatusHelper.HasStatus`, `.StatusTime`, `.StatusStack` | In Betrieb |
| Vorhersagefenster aus der BossModReborn-Timeline | `Configs.cs:742`, `:747`, ausgewertet in `StateUpdater.cs:185` | In Betrieb |
| Zentralisierte Nachzieh-Regel für Gegner-Debuffs | `CustomRotation.ShouldSustainMitigationDebuff` | In Betrieb, 25 Aufrufstellen in den Standardrotationen (Stand 26.09.2026) — Beleg, dass eine gemeinsame Regel über viele Jobs trägt |
| Gegnerzahl-Schwelle als etabliertes Muster | `Configs.MitigationSustainHostileCount` gegen `NumberOfHostilesInRange` | In Betrieb |
| Trennung von Mitigation und Schaden im Dispatch | `CustomRotation_GCD.cs`: HealArea 240, HealSingle 282, DefenseArea 322, DefenseSingle 337, GeneralGCD erst 449 | In Betrieb |

## Was ausgeschlossen wurde und warum

**Nullvariante.** Ausgeschlossen: Die bezifferte Überlappung bliebe, der DoT bliebe
bei AoE unerreichbar, `GetCurrentMitigationPercent` bliebe Anzeige.

**Je Rotation einzeln umsetzen.** Ausgeschlossen: Verstößt gegen die
Defektklassen-Regel; dasselbe Muster altert je Datei getrennt.

**Zentrale Auslösung — ein gemeinsamer Trigger, der Mitigation anfordert.**
Ausgeschlossen am belegten Rückbau in diesem Fork: `HasHostileCountAoeMitigation`
setzte `AutoStatus.DefenseArea` und öffnete damit die gesamte Defensivkette statt der
einen gemeinten Zeile. Ausschlaggebend ist das Ausfallverhalten — eine Bremse, die
nicht greift, führt zum heutigen Verhalten zurück; ein Öffner, der fälschlich feuert,
löst die ganze Kette aus.

**Den DoT-Block vor den Sanctus-Block ziehen.** Ausgeschlossen, weil dann der
**erste** Stun einen GCD später fiele — eine Verzögerung der Schadensvermeidung
zugunsten von Schaden, also die Umkehrung des Vorrangs, der dieses Konzept trägt.
Zusätzlich wäre die Verschiebung der teuerste Teil im Upstream-Merge. Der
Sanctus-Block bleibt, wo er ist, und bekommt eine Aussetzbedingung; der vorhandene
DoT-Block darunter fängt den freigewordenen GCD auf. Damit begrenzt sich der
Merge-Aufwand auf eine Zeile.

**Sanctus in den Verteidigungszweig verschieben.** Ausgeschlossen aus demselben
Grund: durch die Aussetzbedingung überflüssig.

**Ein HP- und ein BMR-Notfallvorbehalt an der Aussetzbedingung.** Ausgeschlossen,
weil beide vor nichts schützen: Der Dispatcher ruft sämtliche Heil- und
Verteidigungszweige **vor** `GeneralGCD` auf, ein kritischer Gruppenzustand erreicht
diesen Code also gar nicht; und ein vorhergesagter Raidwide kommt vom Boss, nicht von
den betäubbaren Trash-Gegnern. Ein Vorbehalt, der nachweislich nichts abfängt, ist
toter Code.

## Falsifikation

**Es liegt kein Defekt vor.** Widerlegt durch die bezifferte Größenordnung und durch
die Existenz von `GetCurrentMitigationPercent` — die Entwurfsabsicht ist vorhanden,
die Verdrahtung fehlt. Für den DoT-Fall zusätzlich durch die beiden Vorkehrungen an
`ModifyDiaPvE`, die bei AoE nie zur Wirkung kommen.

**Die gewählte Option ist falsch.** Widerlegt für die Bremse, nicht für einen
Auslöser — siehe den belegten Rückbau oben. Für die Übertragung auf weitere Aktionen
hält der Einwand teilweise stand, weshalb sie von Beobachtung abhängig gemacht ist.

**Der Einschub ist im mittleren Gegnerzahlbereich falsch.** Widerlegt durch seine Vorgabe und
seine Spielweise: Die Streckung ist Schutz, der Einschub kostet einen GCD Schaden je Pull, und
Sicherheit geht vor Schaden. Eine Bindung an eine Gegnerzahl ist entfernt (A231).

**Nicht widerlegt:** dass der Nutzen im Spiel eintritt. Die Rechnung ist ein Modell.

## Nachweisbarkeit

### Wirksamkeitsmessung im Spiel

Ein Nachweis, dass die Rotation besser spielt, ist **nicht** unmöglich: RSR sieht den
Ereignisstrom und kann sich selbst messen. `Watcher.ActionFromEnemy` wertet jeden
gegnerischen Treffer aus, summiert die Schadensanteile und legt sie über
`DataCenter.AddDamageRec` als `DamageRec(ReceiveTime, Ratio)` in eine Warteschlange
(`DataCenter.AddDamageRec`, gefüllt aus `Watcher.cs`). Der erlittene Schaden über die Zeit ist damit
bereits erfasst — gebraucht wird nur eine Auswertung je Kampf statt eines gleitenden
Fensters.

| Kennzahl | Quelle — **keine davon ist gebaut**, die Namen sind Vorschläge | Aussage |
|---|---|---|
| Erlittener Schadensanteil je Pull | `_damages`, summiert zwischen Kampfbeginn und -ende | Das Zielkriterium |
| Genutzte Betäubungsdauer | `StunCoverage` über die Zeit integriert | Ob die 7 s ausgeschöpft wurden |
| Überlappungsanteil | Anteil der Betäubungszeit, in der die Minderungsquote bereits erhöht **wäre** — eine Größe dieses Namens gibt es nicht | Ob Posten 2 greift |
| DoT-Laufzeitanteil | Zeit mit aktivem DoT geteilt durch Kampfdauer | Ob der DoT-Grund wirkt |

**Versuchsanordnung.** Dieselbe Instanz, derselbe Pull, Option abwechselnd an und aus,
mehrere Durchläufe. Weil alle vier Kennzahlen innerhalb des Plugins anfallen, genügt
eine Anzeige im Einstellungsfenster; ein externes Werkzeug ist nicht nötig.

**Was die Messung nicht leistet.** Sie ist nicht kontrolliert: Gegnerzahl,
Tankverhalten und Gruppenzusammensetzung schwanken zwischen Durchläufen und überdecken
einen Effekt in der Größenordnung weniger Prozent leicht. Sie taugt daher, um eine
**Verschlechterung** zu erkennen und die Größenordnung einzugrenzen, nicht um einen
kleinen Gewinn zu beweisen.

### Übrige Ebenen

| Ebene | Möglich |
|---|---|
| Statisch | `stun_coverage.py` simuliert die Regel GCD für GCD und trägt seinen Selbsttest; für die Übertragung zusätzlich ein Skript, das Aktionen mit Mitigationswirkung im Schadenszweig findet |
| Kompilierung | CI |

Solange die Wirksamkeitsmessung nicht vorliegt, kommt **jeder Schritt hinter eine eigene Option**;
voreingestellt ist der im Kampf sinnvollere Wert (seine Regel, 29.09.2026). Die Messung ist der Weg,
den Nutzen zu belegen — sie gehört deshalb vor die Übertragung auf weitere Aktionen.

## Konsequenzen

**Endnutzer.** Bei eingeschalteter Option fallen Mitigationswerkzeuge seltener, aber
gezielter; die frei werdenden GCDs gehen in Schaden, und die DoT-Uptime steigt.
Spürbar im großen Flächenpull, folgenlos im Einzelzielkampf — was durch die
Gegnerzahl-Schwelle so gewollt ist.

**Autoren abgeleiteter Rotationen.** Die Messung ergänzt die öffentliche Oberfläche
von `RotationSolver.Basic` additiv; keine Signatur bricht. Wer die neuen Prüfungen
nicht aufruft, merkt nichts.

**Upstream-Pflege.** Der Eingriff liegt in `CustomRotation_OtherInfo` und
`WHM_Reborn`, beides Dateien mit regelmäßiger Upstream-Aktivität. Neue Teile liegen in
eigenen Regionen; kein Block wandert.

## Offene Punkte zu diesem Konzept

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.
