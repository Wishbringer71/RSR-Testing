# 08 · Synergie von Schadensvermeidung und Schadenserzeugung

**Leitfrage:** Womit wird auf einen eingehenden Treffer geantwortet — Heilung, Barriere oder Minderung?

Das Dokument gibt den geltenden Stand; die Prüfhistorie steht in `AUDIT_LOG.md` (A20 und folgende).

## Antwort

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

**Gemessen werden die Gegnerseite (Betäubung samt Abnahme und Immunität, Verlangsamung) und der Gesundheitsverlauf je
Gruppenmitglied.** Die persoenlichen Minderungen des Tanks gehen in die Vorausschau nicht als Tabelle ein, sondern
ueber diesen Verlauf: Er ist bereits netto und braucht keine Liste.
Die Schaetzung daraus ist **praeventiv** — bei 90 % Gesundheit meldet sie den Tod acht Sekunden im
Voraus — und sie **korrigiert sich selbst**, indem sie ihre eigene Vorhersage jede Sekunde gegen den
tatsaechlichen Verlauf haelt. Ein externer Beobachter ist dafuer nicht noetig; die Regeln, die darauf aufsetzen,
erben die Selbstkorrektur.

| Baustein | Stand |
|---|---|
| Messung: `SurveyStuns`, `SurveyHostileStatus`, `StatusHelper.StunStatus` und `SlowStatus` | umgesetzt in `CustomRotation_OtherInfo` und `StatusHelper` |
| Aussetzbedingung aus dem **Betaeubungsgrund**, hinter `StretchHolyStun` (Standard an) | umgesetzt (`WHM_Reborn.ShouldStretchHolyStun`) |
| Aussetzbedingung aus dem **Mitigationsgrund** — eine fremde Minderung traegt bereits | umgesetzt (`WHM_Reborn.ShouldHoldHolyWhilePackSlowed`, Standard an) |
| **Stunbarkeit** als Bedingung ueber allen drei Aussetzregeln | umgesetzt (`headroom` aus `SurveyStuns`; bei der Streckung laufende oder frische Betaeubung aus `WHM_Reborn.LongestStunInRadius`) |
| Aussetzbedingung als **Anteil** der verlangsamten Gegner, mit Mindestzahl | umgesetzt (`HoldHolyMinSlowedHostiles`, Standard 3) |
| **Schranke** der Aussetzregel: der Rest muss bewaeltigbar sein | umgesetzt und **gemessen statt gesetzt**: `AnyPartyMemberFallingWithinHealWindow`; `HoldHolyMaxHostileOutput` bleibt als optionaler Deckel, Standard 0 = aus |
| **Schadensrate je Gruppenmitglied**, netto nach allem | umgesetzt: die Gruppe steht in `RecordedHP`, `GetTTK` antwortet fuer sie |
| **Selbstkorrektur** der Schaetzung gegen ihren eigenen Fehler | umgesetzt (`ScoreTtkForecast`, `GetCorrectedTTK`) |
| **Vorausschau** als Ersatzgroesse an allen Heilentscheidungen | umgesetzt (`GetForecastSurvivingShare` und die drei davon abgeleiteten Getter), hinter `HealAheadOfDamage`, Standard an |
| Vorausschau auch in der **Flaechenheilung** | umgesetzt (A182): die Flaechenschwellen lesen eigene vorausberechnete Gruppenwerte (`DataCenter.ComputeForecastAreaStats`); `PartyMembersAverHP` und Geschwister bleiben unveraendert, weil sie viele Leser ausserhalb der Heilkette haben (Konzept 07) |
| Minderungen des Tanks **rechnerisch** erfassen (Vorausschau vor dem ersten Treffer) | nicht nötig (A340): Vor dem ersten Treffer gibt es keine Rate, die eine Minderung skalieren könnte; einen angekündigten, gemessenen Treffer sagt der gemessene Anteil voraus (Abschnitt „Was ein Baustein mehrfach trägt“) |
| **Derselbe Satz je Aktion traegt zwei Zwecke**: die Vorausschau in der Zeile darueber **und** die Wahl des Mittels nach Treffergroesse (Vorgabe 5) | Saetze erzeugt (`generate_defensive_values.py`); die Wahl nach Treffergroesse ist zurueckgebaut, weil kein Job zwei Barrieren zur Wahl haelt (A118, Abschnitt unten) |
| Restzeit der Barriere (`HasSurvivingShield` misst die kuerzeste statt der laengsten) | offen, siehe `TODO.md` |
| Erhebung der uebrigen Doppelnutzen-Aktionen | umgesetzt als `scan16.py`; ein Fund im Tank-/Heilerprofil (Rueckstoss) |
| Rueckstoss auch **als** Minderungswerkzeug wirken | umgesetzt (A194, A236): Abtausch im Pull fuer seine Verlangsamung, nicht vor einem angekuendigten Rueckstoss (A212, A219), nicht auf einen Boss |
| Wirksamkeitsmessung im Spiel | ersetzt durch die Selbstkorrektur der Vorausschau (`ScoreTtkForecast`); keine Ablesung als Aufgabe an ihn |
| **Sonde der Flächenbewertung**: `DataCenter.AreaMitigationSkipped` nennt je Aktions-Id, wo die Bewertung eine Minderung verworfen hat (Listenverwaltung) | in Betrieb |
| **Ein gegnerisches Wirken stoppen** (seine Vorgaben 18.10.2026 und 23.10.2026): Heiler und Magier stoppen das Wirken eines Gegners, der kein Boss ist – beim Weißmagier mit Sanctga, sonst mit Schlaf –, und speichern, was sie dabei lernen | umgesetzt (A329, A334), experimentell, ab Werk an; Abschnitt „Ein gegnerisches Wirken stoppen" |

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

**Zeile drei steht im Code über die allgemeine Schranke (A159):** Jede Rückhaltung weicht, sobald der
gemessene angekündigte Treffer ein ungeschütztes Mitglied in Gefährdungsklasse 1 brächte, und ein Treffer über
der Maximalgesundheit tut das immer. Die offene Abwehrkette gibt dann in jedem Einschiebeplatz das nächste
bereite Mittel aus, Barriere und Minderung nebeneinander; die Stapelsperren (Streckung des Weißmagiers,
Burst von Dunkelritter und Revolverklinge) sind genau die Rückhaltungen, die dort weichen. Die Schranke greift
früher als Zeile drei (schon an `HealthForDyingTanks`, nicht erst an null), also in der sicheren Richtung (A283).

**Das ist Vorgabe 2 dieses Konzepts, zu Ende gedacht.** „Heilung vor Minderung" allein sagt nur die
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

**Eine Auswahlregel gibt es deshalb nicht** (gebaut und zurueckgebaut, A118): Sie haette im ganzen Baum nie
gegriffen.

**Was stattdessen wirkt, ist Stufe 2 — und sie schliesst eine Luecke, die dieses Konzept ohnehin
fuehrt.** Siehe „Heilung vor dem angekündigten Treffer" weiter unten.

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
| alle | **Streckungsbaustein:** Nach einer Auslöseraktion ruht die übrige eigene Abwehr, bis die Wirkung laut Wirktext ausläuft (die Dauer, die zur Minderung gehört), gezählt ab dem Einsatz laut Aktionsprotokoll. In der Flächenabwehr endet sie früher, sobald die Wirkung beim Spieler fehlt: ein positiver Status vom Namen der Aktion, einen GCD nach dem Druck (eine verbrauchte Barriere, Glockenspiel nach dem zweiten Druck; A340); Reflexion liegt auf dem Gegner und behält ihre Dauer. Hält der Auslöser noch eine Ladung (gelesen an seiner Wiederaufladegruppe, nicht am Knopf), streckt er nicht | derselbe Mechanismus stand zweimal mit festen Zahlen im Code (Weißmagier, Astrologe) |
| Heiler | leer | Nur Weißmagier und Astrologe strecken; Gelehrter und Weiser nicht. Eine Heilerregel änderte zwei Jobs ohne belegten Nutzen |
| Tanks | **Reflexion und Gruppenwerkzeug im Wechsel** (`TankAreaDefenseStretched`, A340): die Streckung zwischen Reflexion und Heiliger Quell/Waffengang, Abschütteln, Dunkler Bote, Wackeres Herz; Option „Spread Reprisal and the party mitigation over a run of area attacks“, ab Werk an (Konzept 09, Regel 9). **Abtausch (Arm’s Length) im Pull für seine Verlangsamung** (`ArmsLengthSlowsPull`, A194): jeder Tank mit eigener Option „Use Arm's Length on a pull for its Slow", ab Werk an; ein Pull sind so viele Gegner in Reichweite wie die globale Zahl „Number of hostiles" der Abwehr (Dunkelritter: seine Barrierenzahl, dazu die Barrierenrückhaltung), **Bosse nicht mitgezählt** (`SlowableHostilesInRange`, A236): Abtausch mindert den Treffer nicht, der ihn auslöst, also bringt er an einem Boss-Tankbuster nichts — sein Hinweis vom 01.10.2026, Boss allein in der Arena; die zentrale Rückfallstufe der Einzelabwehr wirkt Abtausch nur noch für Rotationen ohne eigene Pull-Regel (`HasOwnArmsLengthPullRule`) und nur unter derselben Bedingung, vorher ohne jede Gegnerzahl; nicht, solange die Gruppe schon verlangsamt ist, und nicht, solange BossModReborn einen Rückstoß nach Wirkende und vor Ende der Abklingzeit ankündigt (A212, A219). Eine Rückhaltung gibt es auf dieser Stufe nicht: Burst-Rückhaltung nur bei Dunkelritter und Revolverklinge, bei beiden an ein eigenes Burstfenster gebunden | kostet nur die eigene Abklingzeit und wirkt bei jedem Tank gleich; die Beobachtung, auf die der TODO-Eintrag wartete, wäre eine Spielbestätigung als Aufgabe an ihn gewesen |
| Damage Dealer | **Heilung für die Gruppe** (`HoldPartyHeal`, A283): eine strategische Rückhaltung einer Heilung für andere weicht, wenn kein Heiler lebt oder ein Mitglied in Gefährdungsklasse 1 steht; erster Leser der Rotmagier | seine Vorgabe vom 14.10.2026 gilt für jeden Schadensausteiler, der andere heilt. Abwehr: Barde, Pictomancer und Tänzer führen dieselbe Einstellung „Prevent the use of defense abilties during burst" (seit A283 ab Werk aus), Maschinist, Dragoon und Viper feste Rückhaltungen; siehe unten |
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
ohne Ausnahme „verhindern"; ihr Text bindet, die Schranke greift dort deshalb nicht. Sie steht seit A283 ab Werk
**aus**, nach seiner Regel für Voreinstellungen und seiner Spielweise: Die Abwehrpfade laufen nur für einen
angekündigten oder gemessenen Treffer, die Flagge ist also das Wahrscheinlichkeitsmaß. Die Einstellung verwirft
genau diese Treffer im Burst, allein für Schaden. Der Preis von Troubadour, Shield Samba, Tempera und Addle im
Burst ist ein Einschub, der Gewinn des Haltens Schaden im Promillebereich. Ausgeschlossen: die Einstellung an
lassen und an der Schranke weichen lassen — das widerspräche ihrem Text; wer sie einschaltet, will sie ohne
Ausnahme.

**Rotmagier, Heilung und Wiederbelebung im Burst (seine Vorgabe, 14.10.2026):** „rezz während burst ist erlaubt"
wie beim Beschwörer; „heilung wenn notwendig und heiler tot sind oder mit heilen nicht nachkommen (z.b. kein mana
haben)". Sein Grund: „was nutzt eigener ausgeteilter schaden, wenn andere sterben und damit keinen schaden mehr
verursachen?" Umgesetzt (A283): „Prevent raising during burst combos" ab Werk aus; „Prevent healing during burst
combos" bleibt an und weicht über `HoldPartyHeal`, wenn kein Heiler lebt oder ein Mitglied in Gefährdungsklasse 1
steht. Fehlendes Mana der Heiler misst die Regel an seiner Wirkung (ein Mitglied fällt in Klasse 1), nicht am
Mana selbst: Welche Heilung ein Heiler mit seinem Rest noch wirken kann, hängt an Job und Stufe; die Wirkung
antwortet für alle gleich. Außerhalb des Bursts entscheiden weiter die Heilschwellen und die globalen
Nicht-Heiler-Einstellungen.

### HP-Trank im Notfall

Ein HP-Trank geht unter seiner eigenen HP-Schwelle aus (`CustomRotation.UseHpPotion`). Der Notfallzweig lässt
die Schwelle fallen und behält nur die Prüfung, dass genug HP fehlen, damit der Trank nicht verpufft. Er gilt
für einen Tankbuster, der den Spieler erreicht: ein Tankbuster-Cast auf ihn, oder ein von BossModReborn
vorhergesagter, dessen Maske ihn nennt; ohne Maske für einen Tank oder ohne lebenden Tank in der Gruppe
(`DataCenter.BMRTankbusterReachesPlayer`, dieselbe Regel wie die Einzelabwehr, A220). Bis A284 fragte der
BossModReborn-Teil nur, ob ein Tankbuster kommt: Ein Weißmagier bei 60 % trank für einen Tankbuster auf den
Tank — 40 % fehlten, der Ultratrank heilt 25 %, die Prüfung war erfüllt. `DefenseTrace.log` schreibt jeden
gewählten Trank mit dem Zweig, der ihn freigab, und den HP.

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
- **Glockenspiel (Liturgy of the Bell) ist reaktive Heilung, keine Minderung** (Wirktext, The Balance, Basic Guide,
  23.10.2026): fünf Stapel für 20 s; jeder Schaden am Weißmagier, höchstens einmal je Sekunde, heilt alle im
  Umkreis von 20 y um die Glocke mit 400 Potenz; was übrig ist, heilt mit 200 je Stapel beim Ablauf oder beim zweiten
  Druck. Abklingzeit 180 s.
  - *Der zweite Druck* (A340, Option „Press Liturgy of the Bell a second time when the party needs an area heal", ab
    Werk an): Auf dem Weg der Flächenheilung, vor Refugium, sobald die Glocke mindestens einmal geheilt hat (weniger als
    die fünf Stapel des Statusblatts) und die Gruppe unter seinen Flächenschwellen steht. Damit kommen auf einen
    einzelnen großen Treffer 400 Potenz mit dem Treffer und die übrigen 800 gleich danach, statt nach 20 s auf eine meist
    schon geheilte Gruppe. Vor der ersten Auslösung drückt RSR nicht: Das tauschte 400 je Stapel gegen 200, vor dem
    Treffer, für den die Glocke liegt. Mit dem zweiten Druck endet auch die Streckung von Linderung (Glockenspiel ist
    fort). Beides folgt seinem Ablauf vom 24.10.2026 und seinem Vorschlag vom 23.10.2026 (lohnt nur, solange der Gruppe
    so viel fehlt – gemessen an seinen Flächenschwellen statt an einer neuen Zahl).
  - *Mit „Limit Liturgy Of The Bell to multihit party stacks"* (bei ihm an, belegt durch die Reihenfolge der
    Ablehnungen am 22.10.2026, A335) geht Glockenspiel nur auf einen Mehrfach-Sammelmarker, dann vor allen anderen.
    Erkannt werden sieben Markerpfade (`MultiHitSharedPaths`); ein mehrfach treffender Raidwide ohne solchen Marker
    bekommt es nie. Am 18.–22.10.2026: zwei Einsätze.
  - *Ohne die Einstellung* kommt es zuletzt, wenn Plenary, Temperance und Divine Caress nicht bereit sind, und
    hält danach die übrige Abwehr 20 s zurück (Streckung).
  - *Sein Vorschlag (23.10.2026):* „es würde sich also nur lohnen, wenn die gesundheit der gruppe weit unten ist, so
    dass 2000 potenz heilung voll heilt, aber nicht überheilt, oder nur minimal". Geprüft: Für die Platzierung trägt
    die Bedingung nicht. Die 2000 sind kein einzelner Heilstoß, sondern fünf Auslösungen zu 400, jede erst nach einem
    Schaden am Weißmagier; die Glocke muss also vor den Treffern stehen, wenn die Gruppe meist noch voll ist, und
    jede Auslösung heilt frischen Schaden. Überheilt wird, wenn nur der Weißmagier getroffen wurde (etwa ein Schaden
    über Zeit auf ihm allein) – das hängt an der Art des Schadens, nicht an der Gesundheit beim Setzen. Für den
    zweiten Druck trägt sie: Die übrigen Stapel (200 je Stapel) lohnen nur, solange der Gruppe so viel fehlt.
  - *Sein Vorschlag und Hinweis (24.10.2026):* „es gibt neben den mehrfachhits doch hits, die die gesundheit stark
    senken (z.b. auf eins). wenn die bekannt sind, und glockenspiel vorher gewirkt wird, kommt der schaden, danach der
    automatische heal und danach dann der zweite teil aktiviert, der weiter hochheilt." Der Hinweis ist belegt: Seine
    gelernte Flächentabelle vom 24.10.2026 führt fünf Gruppentreffer ab 80 % der maximalen Gesundheit (Uneven Footing
    95 %, Thirty Souls 89 %, Divine Judgment 87 %, Panzer Vor 84 %, Hollow Omen 83 %) und 23 weitere ab 60 %. Ob einer
    davon die Gesundheit auf eins setzt statt Schaden zu machen, zeigt die Tabelle nicht. Der Vorschlag trägt: Auf einen
    solchen Treffer kommen 400 Potenz durch den Treffer selbst und gleich danach 800 durch den zweiten Druck
    (`LiturgyOfTheBellPvE_28509`, Folgeaktion, 1 s Abklingzeit) – 1200 sofort auf eine Gruppe, der fast alles fehlt,
    statt 800 nach 20 s auf eine meist geheilte. Bedingungen: Der Weißmagier wird getroffen (nur Schaden an ihm löst
    aus), die Gruppe steht im Umkreis der Glocke, und der Treffer ist vorher bekannt (gelernte Flächentabelle,
    BossModReborn-Vorhersage).
  - Zur Entscheidung in `TODO.md`: „Weißmagier: Glockenspiel auch vor großen Einzeltreffern (seine Entscheidung)" – mit
    „Limit Liturgy Of The Bell to multihit party stacks" an (bei ihm) setzt RSR die Glocke vor keinen Einzeltreffer.
- **Dunkelritter:** Herkunft ist die Balance-Rotation 6.38 (c97be9ec5).
  - Früher hieß Burst „mindestens die Hälfte der Gruppe im Zwei-Minuten-Burst"
    (`RatioOfMembersIn2minsBurst`, früher). Heute zählen nur die eigenen Abklingzeiten: Delirium kühlt ab
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
  Tankbuster in Folge verlören beide die Streckung. Für einen gewirkten, gemessenen Tankbuster gibt es
  einen Anteil (Tankbuster-Tabelle, Konzept 13); mit ihm rechnet der Plan des Tanks (Konzept 09), die Schranke liest
  ihn nicht – für sie bleibt Klasse 1.
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

- **Astrologe:** Die Streckung dauert so lange wie die Wirkung laut Wirktext: 15 s nach
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
| Flächenabwehr (`AutoStatus.DefenseArea`) | ein laufender Flächenzauber; mit `MitigateBigAreaCastsEvenIfInterruptible` auch ein unterbrechbarer, dessen gemessener Anteil die größte Barriere übersteigt; ein BossMod-Raidwide im Fenster, dessen Maske ihn oder die Gruppe einschließt oder fehlt („Wann eine BossModReborn-Vorhersage ihn betrifft"); der Befehl „Defense Area" von Hand |
| Einzelabwehr (`AutoStatus.DefenseSingle`) | ein Zauber auf einen Tank (Tankbuster), ein Tankbuster-Marker auf einem Mitglied der Gruppe – beim Tank seit A328 auch auf dem anderen Tank, dem seine Hilfe gilt (Konzept 09, „Hilfe für das Ziel eines Tankbusters") – oder ein BossMod-Tankbuster im Fenster; beim Heiler zusätzlich, wie viele Gegner den Tank angreifen |
| Heilung vor dem angekündigten Treffer | ein angekündigter, gemessener Treffer brächte ein ungeschütztes Mitglied mit fehlender Gesundheit in Gefährdungsklasse 1 — dieselbe Linie wie die Schranke |
| Schranke der Rückhaltungen | Gefährdungsklasse 1 jetzt, oder ein angekündigter, gemessener Treffer brächte ein ungeschütztes Mitglied dorthin |
| Burst-Rückhaltungen (Jobtabelle oben) | keine eigene: Sie halten ohne Blick auf die Wahrscheinlichkeit und weichen nur der Schranke |
| Streckung | eine eigene Minderung liegt noch; der nächste Treffer wird von ihr getragen, bis sie ausläuft |
| Kanalsperre (Paladin, Astrologe) | der angekündigte Treffer steht noch aus (`DataCenter.AreaHitPending`); danach löst sie (Konzept 14, „Wechselwirkungen und Zeit") |

Die Regeln entscheiden selbst; ob eine zuletzt hielt oder warum sie wich, führt das Diagnosefenster unter
„Defense hold", und keine Regel liest es.

## Heilung vor dem angekündigten Treffer

**Vor einem angekündigten, gemessenen Flächentreffer wird geheilt, wenn er ein ungeschütztes Mitglied in
Gefährdungsklasse 1 brächte und diesem Mitglied Gesundheit fehlt** (`DataCenter.AnnouncedHitCallsForHealFirst`,
A292). Sonst heilt der Baum erst nach dem Einschlag, an seinen gewohnten Schwellen. Das ist Stufe 2 seiner
Vorgabe, wörtlich: „die aktuelle hp liegt unter dem schadenswert. dann waere aber eine heilung sinnvoll bis max
maxhp." In der Tabelle unter „Die Antwort auf einen eingehenden Treffer" ist das die zweite Zeile; einen Treffer
unter der aktuellen Gesundheit beantwortet die erste mit Deckung, nicht mit Heilung.

**Nötig ist die Regel, weil jede Heilschwelle die Gesundheit liest, die ein Mitglied hat**, keine die, die es
nach dem laufenden Cast haben wird. Ein Mitglied bei 55 % vor einem 45-%-Raidwide steht danach bei 10 %, bevor
eine Schwelle den Treffer sieht.

**Die Linie ist die der Gefährdungsklasse 1, nicht null.** Seine Bedingung ist der tödliche Treffer; gefragt
wird wie bei der Schranke der Rückhaltungen (A159, A283) schon an `HealthForDyingTanks`, in der sicheren
Richtung: Wer nach dem Treffer bei wenigen Prozent steht, fällt am nächsten Tick. Eine Heilung landet dann auf
fehlender Gesundheit — dem Mitglied fehlen mindestens eins minus Trefferanteil minus Linie, vor einem
45-%-Treffer bei der Voreinstellung 15 % also 40 %. Nur ein Treffer nahe seiner ganzen Gesundheit lässt wenig
aufzufüllen. Ein Mitglied bei voller Gesundheit löst nichts aus; für es sind Barriere und Minderung die
Antwort, und die Schranke öffnet sie für denselben Treffer.

**Ausgeschlossen: die Heilschwellen der Flaggen als Linie** (`HealthAreaAbility`, `HealthAreaSpell`). So war
die Regel bis A292 gebaut, mit der Begründung, sie heile nur, wo der Baum ohnehin heilen würde, einen Cast
früher. Das trug nicht: Ein 45-%-Raidwide drückt eine volle Gruppe unter 65 %, also ging vor jedem solchen
Treffer eine Flächenheilung auf volle Gesundheit — eine direkte ganz verschwendet, ein HoT um seinen
Sofortanteil. Sein Hinweis (15.10.2026): „bei aoe-hots noch evtl. vertretbar, bei direkt aoe-heals jedoch
verschwendung". Dieselbe Heilung nach dem Einschlag landet auf fehlender Gesundheit.

**Ausgeschlossen: HoTs vor einem ungefährlichen Treffer gesondert zulassen.** Vor dem Treffer gewirkt, verliert
ein HoT seinen Sofortanteil an volle Gesundheit; nach dem Treffer gewirkt, wirkt er ganz, und seine Ticks setzen
etwa einen GCD später ein. Für einen Treffer, der niemanden gefährdet, gibt es keinen Grund, den Sofortanteil
herzugeben. Barrieren vor dem Treffer kommen über die Abwehrkette, nicht über diese Regel.

| Schalter | Frage | Stand |
|---|---|---|
| `Skip mitigation for small area casts` | Öffnet der Treffer die Abwehrkette? | **an** als Vorgabewert |
| `Heal ahead of an announced area cast` | Wird vor dem Treffer geheilt? | **an** als Vorgabewert (seine Regel für Voreinstellungen, 29.09.2026) |

**Beide Fragen lesen dieselbe gemessene Größe (A99–A102, Konzept 13), an verschiedenen Linien.** Die Minderung
fragt, ob der Treffer jemanden unter die Flächenheilschwelle drückt (`DataCenter.AnnouncedHitDropsAnyoneBelow`,
gelesen von `AreaCastIsWorthMitigating` und `AnnouncedHitIsSmall`): Sie kostet eine Abklingzeit und lohnt, wo
sonst eine Heilung nötig würde. Die Vorausheilung kostet Heilung, die auf voller Gesundheit verfällt, und lohnt
nur, wo der Treffer gefährdet.

**Die Barriere zählt hier mit, und das widerspricht A85 nicht.** A85 hat die Barriere aus der allgemeinen
Heilschwelle entfernt, weil sie keine Gesundheit herstellt — ein Tank bei 40 % hinter einem Schild steht bei
40 %, sobald der Schild ungenutzt abläuft. Hier ist die Frage eine andere: übersteht er **diesen** Treffer.
Gegen ihn wird die Barriere verbraucht und fängt ihn ab; sie herauszurechnen hieße, eine Heilung zu fordern,
die der Schild bereits bezahlt hat. Ungeschützt heißt ohne Unverwundbarkeit über zwei GCDs, wie die Heilkette es liest
(`IsUnprotectedForHealing`: Living Dead mit abgeschalteter Rückhaltung zählt nicht, Konzept 09): Ein Tank unter
Hallowed Ground steht absichtlich tief.

**Die Regel stellt die Frage selbst** (`StateUpdater.AnnouncedAreaCastIsUpForHealing`), statt einen anderswo
abgelegten Wert zu lesen: Sonst hinge sie daran, ob der Verteidigungszweig im selben Bild vorher lief — und der
steht hinter `UseAoeDefense`. Und sie fragt die **unbewerteten** Erkennungen (`IsHostileCastingAreaUnrated ||
IsHostileCastingLargeArea`), nicht `IsHostileCastingAOE`: Die trägt das Minderungsurteil samt seinem Schalter,
und die Regel urteilt selbst über den Treffer (Konzept 13, „Wen die Unterdrückung erreicht").

**Eine Heilung, die nur gegen Ressourcenüberlauf fällt, wartet bis nach dem angekündigten Treffer**
(`CustomRotation.HealForResourceWaitsForAreaHit`, A292): solange Flächenschaden angekündigt und nicht gelandet
ist (`DataCenter.AreaHitAnnounced`, die Erkennung ohne Minderungsurteil) und die Ressource nicht im nächsten
GCD verloren geht. Im Profil hat nur der Weißmagier eine solche Heilung auf die Fläche: Afflatus Rapture bei vollen
Lilien (`Use Lily at max stacks/about to overcap`). Kurz vor einem Raidwide gewirkt, landete sie auf voller
Gesundheit; nach ihm heilt derselbe GCD, was der Raidwide nahm, und die Blutlilie füllt sich gleich. Verloren geht
eine Lilie erst, wenn bei drei Lilien die nächste fällig ist; dann wird sofort verbraucht. Die übrigen Heiler
brauchen die Regel nicht: Druochole des Weisen verbraucht erst im letzten GCD vor dem Verlust und auf ein Ziel, die
dritte Ladung Essential Dignity des Astrologen fragt die Gesundheit. Afflatus Rapture zum Ablauf von Confession
fällt nach einer Plenary Indulgence aus der Abwehrkette hinter den Treffer, weil diese höchstens das
BossModReborn-Fenster vor ihm liegt (Voreinstellung 5 s, Confession 10 s); erst ein Fenster über 10 s ließe
Confession vor dem Treffer enden. Schluss aus den Zeiten, im Spiel nicht beobachtet.

**`DefenseTrace.log` schreibt jede ausgelöste Vorausheilung einmal je Ankündigung**: Aktion, das gefährdete
Mitglied, Trefferanteil (A292). Das Protokoll vom 14.10.2026 zeigte Treffer und Abwehr, aber nicht die Heilungen
davor.

**Beim Beschwoerer gilt fuer Lux Solaris die eigene Regel** („Wann Lux Solaris zündet"), auch auf dem
Heilpfad. Sie ist reaktiv; vor einem angekuendigten Treffer faellt sie nur, wenn ein Mitglied im Radius
schon in Gefaehrdungsklasse 1 steht, dem Wirkenden oder jedem anderen im Radius eine volle Heilung fehlt
oder das Fenster verfaellt.

**Die Heil-oGCD nimmt den ersten Einschiebeplatz, die Minderung den naechsten (A140).** Die Regel setzt
auch `HealAreaAbility`, und der Dispatch fragt Heil-Faehigkeiten vor `DefenseArea`. Das ist Vorgabe 2
in der Reihenfolge, die sie verlangt: Die Vorausheilung faellt nur, wenn der Treffer jemanden
gefaehrden wuerde — genau der Fall, in dem zuerst zu heilen ist. Die Minderung eines
grossen Treffers verliert dabei keinen Weg, nur einen Platz: Ein angekuendigter Cast laeuft in der
Regel ueber mehrere GCDs, also ueber mehrere Einschiebeplaetze. **Verdraengt** wird sie nur, wenn die
Ankuendigung kuerzer ist als ein GCD; fuer diese Lage steht die BossModReborn-Vorhersage bereit, die
vor dem Cast mindert. Schluss aus Zweigreihenfolge und Castdauer, im Spiel nicht beobachtet. Ob ein
Treffer je ohne Minderung einschlug, weil der letzte Platz an eine Heilung ging, zeigt keine Anzeige;
das ist eine benannte Grenze dieses Abschnitts, kein geaendertes Verhalten.

## Die proaktive Schicht hängt fast vollständig an BossModReborn

**Vorgabe des Auftraggebers:** „bossmod liefert nicht für jeden boss werte, sondern nur für
unterstützte module. und da ist der abdeckungsgrad in bossmod auch unterschiedlich. sich auf bossmod
zu 100% zu verlassen ist fahrlässig."

**Erhoben am Code (12.10.2026):** Der Baum kennt zwei Ebenen der Abwehr, und die Trennung verlaeuft
genau entlang der Frage, woher die Vorhersage kommt.

| Ebene | Ausloeser | Quelle | Faellt aus, wenn |
|---|---|---|---|
| **proaktiv** — mindern, **bevor** etwas ankommt | `BMRShouldRefreshBefore(BMRTankbusterIn \| BMRRaidwideIn \| BMRDamageIn, …)` bei allen vier Tanks, bei Barde, Maschinist, Taenzer, beim Beschwoerer (Schimmerschild) und zentral für Addle, Feint und Reflexion (`ShouldSustainMitigationDebuff`); `IsHostileCastingLargeArea` als einziger Weg aus der eigenen Messung | BMR, bis auf den großen gemessenen Cast | kein Modul, oder ein Modul ohne diese Ereignisart |
| **reaktiv** — antworten, wenn der Cast **laeuft** | `IsHostileCastingAOE`, `IsHostileCastingToTank`, `IsHostileCastingTankBusterAtMe`, Gegnerzahl in `ShouldSustainMitigationDebuff` | eigene Beobachtung | der Cast ist unterbrechbar, zu kurz, oder ausserhalb des Ein-GCD-Fensters |

**Kein Alleinstand, aber eine vollstaendige Ebene.** Jede proaktive Stelle hat einen reaktiven
Nachbarn, die Abwehr faellt also nicht aus — sie kommt **spaeter**, naemlich erst, wenn der Cast
schon laeuft und den Vorfilter passiert hat. Das ist der Unterschied zwischen „vor dem Einschlag
gedeckt" und „waehrend des Einschlags gedeckt".

**Der Ausfall ist still, und das ist der eigentliche Mangel.** Beide Ausfallarten — kein Modul, oder
ein Modul, das Raidwides nicht fuehrt — erreichen den Baum als `float.MaxValue`, und jede Pruefung
gegen ein Zeitfenster liest das als „es kommt nichts". Eine ausbleibende Minderung sieht damit aus
wie ein ruhiger Kampf. Die Diagnoseseite „BMR Data" nennt das aktive Modul und, je
Ereignisart, ob es dafuer ueberhaupt eine Vorhersage liefert. Ohne Modul hält `ShouldSustainMitigationDebuff`
die Gegner-Debuffs über die Gegnerzahl (`MitigationSustainHostileCount`) aufrecht, weil Trash meist kein Modul hat:
sobald der Debuff auf dem Ziel fehlt oder in so vielen GCDs endet, wie die Einstellung der Aktion nennt („Number of
GCDs before the DOT/Status effect is reapplied"; bis A340 fest zwei, an der Einstellung vorbei).

**Seine Begruendung, woertlich:** „daher die eigene liste mit dem aoe-schadensausmaß.“ Die Messung je
Aktion ist als **Ersatz** fuer die fehlende Verlaesslichkeit gedacht, nicht als Zusatz — und sie
beantwortet zudem eine Frage, die BMR gar nicht stellt: wie hart der Treffer ist.

**Die eigene Messung ersetzt BossModReborn dort, wo sie es kann: im Cast (A311).** Eine gemessene Größe sagt, wie hart
ein Treffer ist, nicht wann er kommt. Den frühesten Zeitpunkt ohne BossModReborn liefert der Beginn des Casts; ab dort
antwortet die reaktive Ebene, und sie liest die Messung bereits: das Zurückhalten kleiner gemessener Flächen („Skip
mitigation for small area casts"), große unterbrechbare Casts (`IsHostileCastingLargeArea`, Konzept 13), die Vorausheilung
und die Plan-Rechnung der Tanks (Konzept 09). Erhoben: Jede der acht Rotationen mit einem `BMRShouldRefreshBefore`-Zweig
(vier Tanks, Barde, Maschinist, Tänzer, Beschwörer) hat dahinter einen Zweig, der beim laufenden Cast drückt; keine Regel
hängt allein an der Vorhersage. Die proaktive Ebene vor dem Cast lässt sich aus der Messung nicht bauen, weil ihr der
Zeitpunkt fehlt. **Grenze:** Ein Treffer ohne Wirkbalken und ohne Modul ist vorher nicht zu sehen; dort bleibt nur die
Heilung danach. Einen Kampfverlauf zu lernen (Zeit vom Beginn oder von einem Cast bis zum Treffer) wäre ein eigenes
Vorhaben, nicht beauftragt.

### BMR sagt wann, nicht wie hart — und daran wird die Abklingzeit verschenkt

**Spielbeobachtung des Auftraggebers, Ewige Koenigin, Anfangsphase:** zwei Flaechenangriffe
nacheinander, erst ein kleiner, dann ein grosser. Die Vorhersage feuert auf den ersten, Schimmerschild
oder Tactician geht dafuer hinaus, und beim zweiten ist die Barriere aufgebraucht oder die Minderung
abgelaufen.

**Die Ursache steht in der Bedingung selbst.** `BMRShouldRefreshBefore` prueft die **Zeit** bis zum
naechsten Ereignis und ob der eigene Status bis dahin abgelaufen waere — mehr nicht. Eine Groesse
kommt darin nicht vor, und BMR liefert auch keine: Die Schnittstelle nennt den Zeitpunkt, nicht die
Wucht. **Das ist das Loch, das die Groessenbewertung offen laesst:** Der reaktive Weg fragt seit A108 „ist
dieser Treffer eine Abklingzeit wert"; der proaktive fragt es nur über die Zurückhaltung unten.

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

#### Die genaue Frage lässt sich nicht stellen, und das ist gemessen

**Naheliegender waere:** erheben, auf **welches Ereignis** die Vorhersage anspielt, dessen Bewertung
in der eigenen Liste nachschlagen und nur bei „gering" aussetzen. Der Auftraggeber hat genau das
vorgeschlagen. **Die Information existiert auf der Gegenseite nicht** — geprueft an
BossmodReborns Quelltext am 20.09.2026, nicht angenommen:

| Endpunkt | Was er zurueckgibt | Aktionsbezug |
|---|---|---|
| `Timeline.NextRaidwideIn` | `module.StateMachine.NextTransitionWithFlag(StateHint.Raidwide)` — ein **Zustandsuebergang**, den der Modulautor als Raidwide markiert hat | keiner; es gibt keinen Cast dahinter |
| `Hints.NextRaidwideDamageIn` | Aktivierungszeit des ersten `PredictedDamage`-Eintrags dieses Typs | keiner; `DamagePrediction` (BossModReborn) traegt genau `Players`, `Activation`, `Type` |

**Und selbst mit Groessen waere der gemeldete Fall nicht zu loesen:** Die Vorhersageliste wird nicht
herausgegeben. Jeder Endpunkt liefert den **ersten** passenden Eintrag. „Es kommen zwei, der zweite
ist der grosse" ist ueber die Schnittstelle nicht lesbar.

**Daraus folgt die Bauform, nicht aus Bequemlichkeit:** Was gerade laeuft, ist die einzige
Groesseninformation, die in diesem Moment vorliegt. Die Regel ist deshalb so gut, wie diese Zuordnung
trifft — und nicht besser.

### Wann eine BossModReborn-Vorhersage ihn betrifft

**Eine Raidwide-Vorhersage öffnet die Abwehr, wenn sie ihn oder seine Gruppe trifft oder nicht sagt, wen** — eine
weitere Zurückhaltung gibt es nicht. Eine Ankündigung ist nach seiner Spielweise der Grund für Schutz, und
BossModReborn sagt zu einer Vorhersage nur, wann und wen, nicht welche Aktion (Tabelle oben). Was darüber hinaus zu
prüfen wäre, lässt sich an dieser Schnittstelle nicht prüfen, ohne die Abwehr vor echten Raidwides mitzunehmen
(„Ausgeschlossen").

**Wen sie trifft** (`BossModUpdater`, A273): Eine Raidwide-Vorhersage, die nennt, wen sie trifft, und weder ihn noch
mehr als ein Gruppenmitglied einschließt, ist kein Flächentreffer auf diese Gruppe. BossModReborns eigener Typ sagt
„expected to hit everyone and deal minor damage; also used for spread components". Er ist zugleich der Typ jeder
Vorhersage, die ein Baustein ohne eigenen Typ anlegt: Türme für ihre Abnehmer, Köder für die Geköderten, ein
Sturmangriff für die, die in seiner Linie stehen (`AIHints.AddPredictedDamage` und seine Aufrufer, BossModReborn
be8ff77, gelesen 13.10.2026). Jeder Eintrag führt die Maske der Getroffenen, je Baustein über alle Türme, Köder oder
Spreads zusammen. Lesbar ist sie nur für den ersten Eintrag der Liste; ist der ein Raidwide, ist er auch der erste
Raidwide, weil die Liste nach Zeitpunkt sortiert ist. Plätze 0 bis 7 sind die Gruppe, Platz 0 er (`PartyState`).
Dieselbe Lesart wie beim Cast (Konzept 13, „reaches the party", A218) und beim Tankbuster (A220).

Im Kampf: Ein Turm, ein Köder oder ein Sturmangriff auf einen anderen öffnet weder die Flächenabwehr noch die
Auffrischungen davor. Trifft er zwei aus der Gruppe, bleibt die Gruppenminderung. Die eigene Abwehr (Schimmerschild,
die Abwehr, die nur ihn schützt) fällt nur, wenn er selbst darunter ist (`BMRRaidwideHitsPlayer`, gelesen in
`AreaHitReachesPlayer` und `SMN_Reborn.RadiantAegisAheadOfRaidwide`). Unbekannt bleibt die Maske, wenn ein anderer
Eintrag vorn steht oder die Vorhersage aus der Zeitleiste kommt; die Zeitleiste ist eine Zustandsmarkierung des
Modulautors („someone better press some save") ohne Getroffene. Dann gilt die Vorhersage als Treffer auf jeden. Jede
Abwehrzeile im Protokoll nennt Modul, Endpunkt und „hits you".

**Anlass, sein Protokoll vom 04.–10.10.2026** (A272, A275): Allein über BossModReborn geöffnet wurde die Abwehr bei
174 Vorhersagen, gebündelt nach dem vorhergesagten Zeitpunkt; bei den meisten steht im Protokoll kein Treffer auf
ihn. Was sie waren, ist aus dem Protokoll nicht bestimmbar, weil es weder Modul noch Endpunkt noch Maske nennt. Zwei
Erklärungen sind am Quelltext belegt: Türme, Köder und Spreads auf andere (die Maske oben), und ein FATE-Modul in
Occult Crescent, das nach seinem Weggang weiterläuft (am Quelltext gefolgert, `TODO.md`, Adressat BossModReborn).
Eine dritte ist offen: Im Kampf gegen Necrophobia (Forked Tower: Magic, 10.10.2026) steht von 12:32 bis 12:44 kein
Treffer auf ihn, während sieben Raidwides vorhergesagt wurden und Necrophobias Tankbuster zweimal gemessen wurde.
BossModReborn führt für diesen Kampf den Raidwide Hail of Hellflares, dessen Schaden von Hilfsfiguren kommt. Dasselbe
Bild im Critical Engagement Dark Artistry (Occult Crescent, 17.10.2026): vier Vorhersagen für Düsterflare (Dark Flare;
im Modul ein Raidwide auf einen Cast des Bosses an sich selbst, den Schaden bringt eine Hilfsfigur ohne Cast),
Abwehr, Abschütteln und Reflexion gingen hinaus, und im ganzen Kampf steht kein Treffer auf ihn. Ob er nicht
getroffen wurde oder das Protokoll die Quelle nicht sieht, ist nicht bestimmt. Am Quelltext belegt ist ein Weg für
das Zweite: ECommons löst die Quelle über die Objekttabelle auf, und einen Satz ohne auflösbare Quelle verwirft der
Effekt-Handler ganz, ohne Zeile, Schadensaufnahme und Messung (`TODO.md`). Seit A299 schreibt das Protokoll jeden
Treffer aus einer Quelle, die RSR nicht als Gegner liest. Sein Protokoll vom 18.10.2026 belegt einen zweiten Weg: Alle
47 Treffer des Dungeonbosses Hammerschädel kamen aus Figuren in der Objekttabelle, die der Handler nur nicht las – er
nahm von nicht anvisierbaren Figuren nur Kämpfer an, und Teile eines Gegners oder Figuren ohne Unterart fielen heraus.
Seit A327 liest er jede Figur der Gegnerseite (`ObjectHelper.IsEnemySideFigure`), und die Protokollzeile nennt bei
einem verbliebenen Fall Unterart und Anvisierbarkeit. So zeigten seine Protokolle vom 19.–22.10.2026 die letzte
Lücke: Die Helfer von Prüfungen, Raids und Dungeons haben die Unterart 11, die A327 nicht kannte; 127 ihrer Aktionen
trafen ungelesen, bis 88 % auf acht Mitglieder. Seit A334 zählen sie mit. Ob Necrophobia und Düsterflare derselbe Fall waren, ist nicht
bestimmt (Schluss naheliegend, nicht belegt). „Kein Treffer im Protokoll" ist bis dahin kein sicheres Maß für
„Fehlvorhersage". Seine Frage vom 13.10.2026, ob es Raidwides waren oder Flächen, außerhalb
derer er stand, führte zur Maske.

**Eine Vorhersage, die mit einem kleinen Cast kam, öffnet die Flächenabwehr nicht** (A301,
`DataCenter.BMRRaidwideCasts`, `BmrRaidwideIsSmallListedCast`, gelesen in `StateUpdater.ShouldAddDefenseArea`). Der Text
von `Skip mitigation for small area casts` sagt zu, dass ein gelernter Flächencast, nach dem alle über der
Flächenheilschwelle bleiben, die Gruppenminderung nicht mehr auslöst; der Cast-Weg hielt das ein, der Zweig über
BossModReborn öffnete für jede Vorhersage. In seinen Protokollen vom 06. bis 16.10.2026 fielen so 22 Abwehraktionen
(Vollkommener Ablass, Linderung, Schimmerschild, Stumpfsinn) auf gelistete Casts mit 1 bis 23 %.
- *Zuordnung über den Beginn:* BossModReborn nennt zu einer Vorhersage keine Aktion, und ihr Zeitpunkt bindet sie
  nicht an einen Cast – gegen einen laufenden gelisteten Cast lag er in seinen Protokollen 0,4 bis 2,9 s nach dessen
  Ende, je nach Modul. Ein Raidwide-Baustein eines Moduls legt die Vorhersage aber an, wenn der Cast beginnt, und
  nimmt sie mit seinem Ende weg (`Components.RaidwideCast`, Eintritt `CastFinishAt`). Taucht ein Eintrittszeitpunkt
  in demselben Durchlauf auf, in dem ein Cast beginnt, kam er mit diesem Cast. Gezählt werden alle gegnerischen Casts,
  die dann begannen, auch unsichtbare Helfer mit ungelisteten Aktionen.
- *Gehalten wird nur*, wenn jeder dieser Casts gelistet und gemessen ist, keiner die Gruppenminderung verlangt
  (dieselbe Größenregel wie der Cast-Weg, `AreaShareCallsForMitigation`) und alle noch gewirkt werden. Nach ihrem Ende
  kann hinter der Vorhersage ein späterer Treffer stehen; dann antwortet der Zweig wie zuvor. Jeder unbekannte Fall –
  kein Cast kam mit der Vorhersage, einer ist ungelistet oder ungemessen – öffnet wie bisher.
- `DefenseTrace.log` schreibt je Vorhersage „area defence held: … came with …, rated …".
- *Grenzen:* Verdeckt die Vorhersage des kleinen Casts eine spätere große, die BossModReborn erst danach nennt,
  öffnet die Abwehr für die große erst, wenn der kleine Cast endet; liegen beide weniger als einen Druckplatz
  auseinander, kann die Minderung zu spät kommen, die vorher beim kleinen gefallen wäre und den großen mitgedeckt
  hätte. Ein Modul, das die Vorhersage nicht am Beginn anlegt, bleibt ohne Zuordnung, die Abwehr öffnet wie bisher.
- *Abgrenzung:* Die vorausschauende Auffrischung (`BMRShouldRefreshBefore`) hat für dieselbe Frage ihre eigene
  Einstellung (`HoldProactiveMitigationForSmallCast`, C75) und bleibt bei ihr.

**Ausgeschlossen:**
- *Die Vorhersagen eines Moduls nach ihrer Trefferquote beiseitelegen* — gebaut und zurückgenommen (A272, A275).
  BossModReborn nennt zu keiner Vorhersage eine Aktion. Eine Quote je Modul zählt deshalb Türme auf andere und echte
  Raidwides desselben Moduls zusammen, und ihr Maß „eingetroffen" ist das Protokollmaß, das bei Necrophobia nichts
  sah. Über sein Protokoll nachgerechnet, nahm sie die Abwehr vor elf Raidwides von 15 % bis 66 %, die nur
  BossModReborn angekündigt hatte.
- *Die Größe der Vorhersage aus dem Treffer lernen* — BossModReborn nennt keine Aktion zur Vorhersage, und die
  eingetroffenen streuen von 0 % bis über 40 %; der Höchstwert eines Moduls bewertete jede Vorhersage als groß.
  Eingetroffene Vorhersagen von 0–1 % bekommen deshalb dieselbe Abwehr wie große, wenn kein klein bewerteter Cast
  mit ihnen kam (siehe „Eine Vorhersage, die mit einem kleinen Cast kam").
- *Vorhersagen eines Moduls verwerfen, in dessen Arena er nicht steht* — ändert in seinem Inhalt nichts. In
  Occult Crescent werden die Module der Critical Engagements (30 von 30) und des Forked Tower: Magic (4 von 4) nur
  aktiv, wenn er beim Pull in der Arena steht (`CheckPull`), und die Arena eines FATE-Moduls (24 von 24,
  `OpenWorldFate`) wandert mit dem Spieler. Den Fall, den sie fangen sollte — ein Kampf, den er verlassen hat —,
  sieht die Arena deshalb gerade beim FATE nicht.

**Grenzen:**
- Ein Raidwide hinter einem Einzelmechanismus auf einen anderen, der zur selben Zeit oder kurz davor fällt, ist bis
  zu dessen Auflösung unsichtbar. Löst der Einzelmechanismus so kurz vor dem Raidwide auf, dass ein Druck den
  Snapshot nicht mehr erreicht (der Mindestvorlauf der Leser, `DataCenter.PressReachesBeforeSnapshot`, A319), kommt
  der Raidwide über BossModReborn gar nicht mehr; der Cast-Weg bleibt.
- Ohne Modul fehlt die Vorhersage ganz, und es bleibt der reaktive Weg.

### Gegner-Debuffs am Tankbuster und am Raidwide

**Geltender Stand (A244):** Ein Tank hält Reflexion in seiner Einzelabwehr zurück, wenn alle vier Bedingungen gelten:
- ein Tankbuster trifft ihn;
- seine eigene große Minderung, Schutzwall oder eine Unverwundbarkeit läuft schon (`StatusHelper.RampartStatus`);
- BossModReborn sagt einen Raidwide an, der nach dem Ende der Reflexion und vor ihrer neuen Bereitschaft landet;
- weder er noch ein Tank steht in Gefährdungsklasse 1.

In jeder anderen Lage fällt Reflexion wie bisher. Das gilt für alle vier Tanks (`HoldReprisalForRaidwide`, zentral).

**Seine Angaben (01.10.2026):**
- *Kriterium, Vorgabe:* „gewisse skills helfen bei raidwides allen, während sie bei tankbuster nur dem tank helfen. da
  ist immer die frage, wie stark der tankbuster und wie stark die raidwides sind, kommt der tank ohne den skill beim
  tankbuster in ernste bedrängnis, oder eher die anderen bei raidwides".
- *Hinweis, Mechanik:* „die benannte debuffs wirken auf den schadensgeber und damit sind dann einzelperson wie auch
  gruppe geschützt". Bestätigt am Wirktext (Job-Guide): Reflexion „Reduces damage dealt by nearby enemies by 10 %"
  für 15 s; Zermürben und Stumpfsinn mindern den Schaden des Ziels.

**Was daraus folgt, und was nicht:**
- Der Debuff wird nicht „an" einen Treffer gegeben. Er liegt auf dem Gegner und mindert jeden Treffer, den der
  während der 15 s austeilt.
- Liegen Tankbuster und Raidwide innerhalb von 15 s, deckt ein rechtzeitig gelegter Debuff beide, gleich in welcher
  Reihenfolge. Hier gibt es nichts zu entscheiden; der Code tut es bereits, weil der Debuff beim ersten der beiden
  fällt.
- Eine Wahl gibt es nur, wenn der zweite Treffer nach dem Ende des Debuffs und vor seiner neuen Bereitschaft landet
  (Reflexion 60 s, Zermürben und Stumpfsinn 90 s).

**Quellen zum Fall mit Wahl:**
- The Balance, „Becoming a better tank": Gruppenwerkzeuge der Tanks, Reflexion eingeschlossen, gehören „most of the time"
  auf raidweiten Schaden, „since reducing damage on everyone at once is more valuable than reducing damage only on
  yourself".
- Für den Tankbuster hat der Tank eigene Werkzeuge: eine große Minderung oder Schutzwall, dazu die kurzen.
- Akhmorning, „Raiding Fundamentals" (Gemeinschaftsquelle):
  - Ein Debuff muss liegen, bevor der Treffer festgelegt wird, also während des Zauberbalkens. Zwischen dem Ende des
    Balkens und dem Schaden gelegt, wirkt er nicht.
  - Schaden, den unsichtbare Hilfsfiguren austeilen, mindert ein Debuff auf dem sichtbaren Boss „usually" nicht; es
    gibt Ausnahmen (E7S).

**Die Fälle mit Wahl und ihre Entscheidung nach seinem Kriterium:**

| Lage | Tank ohne Debuff am Tankbuster | Gruppe ohne Debuff am Raidwide | Entscheidung |
|---|---|---|---|
| eigene große Minderung oder Schutzwall läuft | gedeckt; die 10 % fehlen ihm, die Deckung trägt | jedes Mitglied verliert 10 %, kein persönliches Werkzeug dieser Größe | **halten** für den Raidwide |
| keine eigene Deckung (alles verbraucht) | ernste Bedrängnis möglich (ohne Messung gilt ein angesagter Tankbuster als möglicherweise tödlich, Konzept 09) | wie oben | **an den Tankbuster** |
| Spieler oder ein Tank in Gefährdungsklasse 1 | — | — | an den Tankbuster (die Rückhaltung weicht) |
| Tankbuster auf einem anderen Tank | dessen Deckung ist hier nicht bekannt | wie oben | wie bisher, an den Tankbuster |
| Pull | — | — | wie bisher (kein Tankbuster, Reflexion trifft das ganze Rudel) |
| kein Modul oder keine Raidwide-Ansage | — | Raidwide nicht bekannt | wie bisher |

**Grenzen:**
- *Hilfsfiguren:* Kommt der angesagte Raidwide von einer Hilfsfigur, wirkt Reflexion dort vermutlich nicht. Dann hält
  die Regel umsonst; der Tank war am Tankbuster gedeckt, verloren sind 10 % an ihm. Ob ein Raidwide von einer Hilfsfigur
  kommt, misst RSR nicht. Das Protokoll nennt seit A242 je Landung die getroffenen Gruppenmitglieder; im 48-Spieler-Inhalt
  landeten die gelisteten Casts nur auf ihrem Wirkenden, der Treffer kam als gleichnamige Folgeaktion oder gar nicht
  (A272, Konzept 13).
- *Andere Spieler:* Ein zweiter Tank kann den Raidwide mit seiner Reflexion decken. Das ist Verhalten eines anderen
  Spielers, kein tragender Grund.
- *Zermürben und Stumpfsinn:* Die Einzelabwehr eines Schadensausteilers öffnet für einen Tankbuster auf ihn selbst. Eine
  große persönliche Minderung hat er nicht, die Bedingung „gedeckt" gilt nie. Die Stufe der Schadensausteiler bleibt
  deshalb leer; ihre Debuffs fallen wie bisher.

**Belege aus seinen Protokollen vom 01.10.2026:** Krieger, Restaurierter Löwe. Tankbuster kamen etwa alle 61 s und
trafen ihn mit 30–47 % unter einer großen Minderung, mit 85 % ohne. Seine Raidwides wurden mit bis zu 37 % gemessen
(Klosterdämon). Unter seiner eigenen Deckung ist der Tank also nicht in ernster Bedrängnis; die Gruppe verliert am
Raidwide 10 % jedes Mitglieds.

**Verworfen:**
- „An den Tankbuster, weil er sicher und sofort kommt" (meine Aussage vom 01.10.2026, C105). Ein angesagter Raidwide ist
  ebenso angekündigt, und die Reihenfolge der beiden ist nicht fest.
- „Zurückhalten, sobald ein Raidwide angesagt ist", ohne Blick auf die eigene Deckung. Das ließe einen ungedeckten Tank
  am Tankbuster ohne Reflexion.
- Die gemessene Tankbuster-Höhe als Voraussetzung. Die eigene Deckung beantwortet die Frage seines Kriteriums ohne
  sie, auch für ungemessene Tankbuster.

**Im Tankbuster-Plan dieselbe Regel (A302):** Für einen gemessenen Tankbuster ist Reflexion dann ein vorbehaltenes Glied;
gewählt wird zuerst das billigste Bündel ohne sie, und nur wenn keines ohne sie überlebt, geht sie (Konzept 09,
„Reflexion für den Raidwide"). Vorher nahm der Plan sie als billigstes Glied, und am Raidwide fehlten jedem 10 %.

## Wann Lux Solaris zündet

**Sachstand der Regel, aus seinen Vorgaben vom 26.09.2026 (A151 bis A153), umgesetzt in A154 bis A156**
(`SMN_Reborn.LuxSolarisDecision`, gefragt von allen drei Wegen).

**Was Lux Solaris ist.** **Hinweis des Auftraggebers:** Lux Solaris ist eine Point-Blank-Flaeche vom
Wirkenden aus, wie Holy beim Weissmagier. Die Wirktexte stuetzen das gleich: Holy trifft „all nearby
enemies", Lux Solaris heilt „own HP and the HP of all nearby party members". Im Baum gehoeren beide
damit in die Zweige fuer Flaechen mit Reichweite 0 — Holy in den feindlichen, Lux Solaris in das
freundliche Gegenstueck (`ActionTargetInfo.FindTarget`). Eine reaktive Flaechenheilung um den Beschwoerer (Wirktext 36997: „Restores
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
   - **Mountain Buster** (nur unter Titan's Favor): haengt an einem Status. Der naechste Topaz Rite
     ueberschreibt eine unverbrauchte Gunst: The Balance, Summoner Basic Guide (02.10.2026): „weave each proc
     immediately when you gain one, otherwise you will lose a use/'overwrite' it on the next cast of Topaz
     Rite"; der Job-Guide: „Effect of Titan's Favor ends upon execution of certain summoner actions". Zur
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
`GeneralAbility` fragen dieselbe Entscheidung (`LuxSolarisDecision`); dass jeder Weg einer eigenen Bedingung
folgte, war die Ursache der Befunde in A150. Ausgenommen ist nur der **manuelle Heilbefehl**: Er ist sein
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
- **Der Rand des Radius bleibt großzügig gezählt** (A340). Die Zielwahl rechnet Trefferfläche zu Trefferfläche
  (`GetCanAffects` über `TargetHelper.GetTargetsByRange`, Leser: die Flächenheilungen um den Wirkenden samt Lux Solaris
  und der Zielwahl daraus) und zählt ein Mitglied bis etwa einen Yalm jenseits des Radius mit. Für gegnerische Flächen
  auf Spielern entscheidet der Mittelpunkt des Modells (Akhmorning, „Raiding fundamentals"); ob dasselbe für eine
  eigene Heilung auf Mitglieder gilt oder ob deren Trefferfläche zählt, belegt keine Quelle (gesucht 24.10.2026). Ohne
  Beleg ändert RSR die Zählung nicht – sie könnte sonst einen Erreichten auslassen. Selbst nachmessen lässt es sich
  nicht verlässlich: Zwischen dem Moment, in dem der Server den Wirkbereich festlegt, und dem Effekt beim Client bewegen
  sich Mitglieder um mehr als dieses eine Yalm (Laufgeschwindigkeit rund 6 y/s, unbelegt). Im Kampf: Ein Mitglied am
  äußersten Rand kann mitgezählt werden, ohne geheilt zu werden.

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
den ersten vorzeitig ab.

Lux Solaris laeuft **nicht** durch den Flaechenheil-Zweig („Area heal around you"): Sie wird auf den
Wirkenden gezielt.

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

## Was ein Baustein mehrfach trägt

**Die offenen Punkte dieser Konzeptfamilie haengen an weniger Bausteinen, als ihre Zahl vermuten
laesst.** Wer einen davon baut, schliesst mehrere Punkte zugleich — das ist der Grund, die Konzepte
gemeinsam zu lesen und nicht einzeln.

**Ein Wirkungswert je Aktion, aus dem eigenen Wirktext — er trug vier Punkte; erzeugt ist er (`DefensiveValues.g.cs`).**

| Offener Punkt | Konzept | Was der Wert dort beantwortet |
|---|---|---|
| Vorausschau vor dem **ersten** Treffer | hier; für angekündigte, gemessene Treffer gedeckt durch den gemessenen Anteil (A340) | Wieviel Schaden der angekuendigte Einschlag traegt, bevor eine Beobachtung vorliegt |
| Wahl des Mittels nach Treffergroesse (Vorgabe 5) | hier; zurueckgebaut, kein Job haelt zwei Barrieren zur Wahl (A118) | Welche Barriere, welche Minderung den Treffer deckt |
| Die Minderungsbilanz kennt Betaeubung und Verlangsamung nicht | hier, „Die Luecke" | Um wieviel eine Drosselung den Strom senkt — am Code: `GetCurrentMitigationPercent` rechnet auf Gegnerseite Addle, Feint, Dismantle und Reprisal, dazu die Gruppenminderungen auf den Mitgliedern; Betaeubung und Verlangsamung nicht |
| Rueckstoss auch **als** Minderungswerkzeug | umgesetzt (A194, A236) | Dass seine Verlangsamung in derselben Groessenordnung wirkt wie Rampart |

Der Wert ist **erzeugbar**, nicht handzufuehren, und das ist gemessen statt vermutet: Im Lauf vom
19.09.2026 nennen **69** Wirktexte in `ActionId.resx` die Formel „reduces damage taken by X %“, mit
ausgeschriebenem Prozentsatz — 10, 15, 20, 25, 30, 40, 50 und 99 %; die Barrieren nennen ihren
Anteil ebenso (25 %, 15 %, 10 %). `RotationSolver.GameData` liest dieselben Blätter ohnehin aus. Das unterscheidet ihn von der hier verworfenen
Statussatz-Tabelle, deren Einwand die Pflege war.

**Die Schadensart aus BossModReborn ist benutzbar.** `PredictedDamageType` kommt als `int` ueber die
IPC-Grenze und wird direkt in das eigene Enum gecastet — ein Vertrag ohne Compilerpruefung (CLAUDE.md,
„Vertraege"). Gemessen am 20.09.2026 gegen
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

**Das gemessene Schadenspotential je Gegneraktion — wo es gelesen wird und wo nicht.** Es liegt seit
A99 bis A102 vor (`13-aoe-damage-classification.md`). Gelesen wird es vom Minderungsurteil
(`AreaCastIsWorthMitigating`), von der Vorausheilung (`AnnouncedHitCallsForHealFirst`), vom zweiten Weg für
große unterbrechbare Casts (`IsHostileCastingLargeArea`), von der Zurückhaltung der proaktiven Minderung
(`AnnouncedHitIsSmall`) und vom Tankbuster-Plan, wenn er gegen einen Raidwide abwägt (`AnnouncedHitKills`,
Konzept 09). Offen:

| Frage | Konzept | Stand |
|---|---|---|
| Wie steht es vor dem ersten Treffer eines Pulls? | hier | angekündigt und gemessen: vorhergesagt (Vorausheilung `AnnouncedHitCallsForHealFirst`, Minderung, Schranke, Tankbuster-Plan); unangekündigt (Trash-Auftakt): blind für den Verlauf, 2,5 s Anlauf, ohne Annahme nicht vorhersagbar (A340) |
| Wie hart schlaegt ein Rueckstoss, ein Stopp-Cast? | `13-…`, Abschnitt „Was der Baustein eroeffnet" | keine Entscheidung liest es: beide Listen öffnen nur Rückstoßschutz und Innehalten (A340) |

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

## Ein gegnerisches Wirken stoppen

**Seine Angaben, im Wortlaut:**
- Vorgabe (18.10.2026): „kann sleep der magiewirkenden klassen unterbrechen? falls dazu eine chance besteht, einbauen.
  bzw. als experimentelle option einbauen mit logging, ob damit gegner aoes gestoppt werden können, und welche".
- Präzisierung (23.10.2026): „schlaf sollte doch als meleependant für unterbrechungen genutzt werden, also auch auf
  unterbrechbare casts, solange das zeitfenster reicht."
- Präzisierung (23.10.2026): „ausserdem steht sleep nicht in konkurenz zu holy beim whm. wenn holy stunnen kann, ist
  das vorzuziehen. speziell bei gruppen."
- Vorgabe (23.10.2026): „und die speicherung des gewonnenen wissens ingame wird ingame nicht ausgewertet und persistent
  genutzt" – „nimm dir ein beispiel an der aoe schadenspotentialspeicherung" – „wichtig ist zu wissen, wo sleep nicht
  wirkt, damit es nicht immer wieder neu versucht wird".

**Sachstand (A329, A334):** Heiler und Magier stoppen das Wirken eines Gegners, der kein Boss ist, wenn es eine
Fläche auf die Gruppe ist oder ein Wirken, das das Spiel als unterbrechbar führt – das, was ein Nahkämpfer mit
Stolperfalle stoppt –, solange es lang genug läuft. Zuerst mit dem eigenen Stopper des Jobs: beim Weißmagier Sanctus bzw.
Sanctga, der jeden nicht immunen Gegner im Umkreis betäubt. Wo der nicht reicht, mit Schlummer bzw. Schlaf. Was die
Versuche zeigen, speichert RSR über die Sitzung hinaus und richtet jede weitere Entscheidung danach. Option „Stop an
enemy's cast with the job's stun or a sleep (experimental)", ab Werk an. Jeder Versuch steht in `DefenseTrace.log`.

**Im Kampf:** Ein Angriff eines Gegners, der kein Boss ist, bricht ab, bevor er landet. Der Weißmagier wirkt dafür
Sanctga statt eines Schlafs, betäubt damit jeden Gegner im Umkreis und macht dabei Schaden; die anderen kostet es
einen GCD ohne Schaden. Gegen Gegnerarten, die widerstanden haben, und Aktionen, die trotz rechtzeitigem Schlaf
losgingen, versucht RSR keinen Schlaf mehr, auch nicht nach einem Neustart; Sanctga betäubt sie weiter.

### Was seine Protokolle zeigen

Seine Protokolle vom 19.–22.10.2026 (Weißmagier, A334):

- *Schlaf wirkt auf nicht unterbrechbares Wirken.* Alle Versuche bis auf einen galten Wirken, das das Spiel als nicht
  unterbrechbar führt. Drei endeten deutlich vor ihrer Zeit (0,8, 1,3 und 2,1 s früher), vier knapp (0,1 bis 0,3 s).
- *Der Zeitpunkt entscheidet.* Jedes deutlich abgebrochene Wirken endete 0,7 bis 1,2 s, nachdem der Schlaf gelandet
  war. Beide, die trotzdem losgingen, hatten beim Eintreffen 0,5 s Rest. Sechs liefen mit 0,6 bis 0,9 s Rest ohne
  Wirkung zu Ende – abgebrochen oder ins Leere gegangen, das Protokoll zeigt es nicht. *Schluss, nicht belegt:* Der
  Status greift einen Augenblick nach dem Eintreffen der Wirkung.
- *Widerstand:* 13 Gegnerarten, darunter nicht als Boss markierte Gegner von Allianzraids und Prüfungen.
- *Unterbrechbares Wirken:* ein Versuch, ohne Ausgang.
- *Was fehlte:* Ein Weißmagier schlief Gegner ein, die ein Sanctga mit betäubt hätte; das Gelernte begann jede Sitzung
  leer, sodass jede immune Art und jede nicht stoppbare Aktion wieder einen GCD kostete.


### Mechanik

Belege abgerufen 18.10.2026 (xivapi v2, Blätter `Action` und `Status`, englisch und deutsch).

| | Schlaf (Sleep, #25880) | Schlummer (Repose, #16560) |
|---|---|---|
| Rolle, Stufe | Magier, 10 | Heiler, 8 |
| Wirkzeit, Wiederholzeit | 2,5 s, GCD | 2,5 s, GCD |
| Reichweite, Wirkbereich | 30 y; das Ziel und Gegner bis 5 y um es | 30 y; nur das Ziel |
| Wirktext | „Puts target and all nearby enemies to sleep. Duration: 30s" | „Afflicts target with Sleep. Duration: 30s" |

- *Status Schlaf (#3):* „Overwhelming drowsiness is preventing the execution of actions"; sperrt Aktionen und
  Bewegung (`LockActions`, `LockMovement`) wie Betäubung (#2). Immun macht „Sleep Resistance" (#1354, „Immune to sleep
  effects").
- *Unterbrechung:* Kein Wirktext und kein Job-Guide sagt, dass Schlaf ein Wirken abbricht. Gemeinschaftsquellen sagen
  es: Final Fantasy Wiki (fandom), „Sleep can potentially be used to interrupt casting if the target is vulnerable to
  the status … interrupting non-boss mobs during dungeons"; Spielerbeiträge (Steam und Square-Enix-Forum, 2022) nennen
  es für aufladende Flächenangriffe, auch unter laufendem Schaden. Schluss: Dieselbe Sperre bricht als Betäubung das
  Wirken eines nicht immunen Gegners ab, darauf baut die Tiefschlag-Regel des Baums. Status: Gemeinschaftsquelle und
  Schluss, nicht offiziell; deshalb experimentell und gemessen.
- *Aufwachen:* Schaden weckt den Gegner (Gemeinschaftsquelle, fandom). Ob die Unterbrechung dennoch gilt, misst die
  Regel.
- *Feindseligkeit durch Schlaf:* unbelegt; die gefundene Quelle stammt aus Version 1.0.

- *Sanctus und Sanctga* (Wirktexte, `ActionId.resx`): „Deals unaspected damage … to all nearby enemies. Additional
  Effect: Stun Duration: 4s", Potenz 140 bzw. 150, Umkreis 8 y um den Weißmagier, Wirkzeit wie Schlummer. Eine
  Betäubung bricht das Wirken eines nicht immunen Gegners ab (Grundlage der Tiefschlag-Regel). Jede weitere
  Betäubung halbiert sich, bis der Gegner vorübergehend immun ist (`StunWatch`, A272).
- *Abnutzung:* Wiederholter Schlaf auf denselben Gegner hält kürzer, bis er eine Weile widersteht, getrennt von der
  Betäubung gezählt. Gemeinschaftsquellen (Square-Enix-Forum, Threads 104440 vom 10.10.2013 und 290466 vom
  22.04.2016): 30 s, 15 s, etwa 8 s, danach widerstanden; das Ende der Immunität nennen sie mit 30 bis 60 s, uneinig.
  Keine offizielle Quelle und kein Status zeigt es; deshalb lernt die Regel die Anzahl, statt sie einzutragen. Fandom
  („Sleep (Final Fantasy XIV)"): Oberhalb Stufe 50 sind Gegner in Inhalten über die Inhaltssuche „typically immune".
- *Ziehen:* Schlaf trifft bis 5 y um das Ziel, Sanctga 8 y um den Weißmagier – auch Gegner, die noch nicht kämpfen.
  „Don't attack new mobs by AoE." (ab Werk aus) verbietet das für jede Flächenaktion; die Regel hält sich daran.

### Mitwirkende Regeln (derselbe GCD, dasselbe Ziel)

| Regel | Berührung | Wie sie zusammenwirken |
|---|---|---|
| Sanctus-Einsatz des Weißmagiers (ab drei Gegnern, `AoeCount`) | derselbe Zauber | Der Stopp wirkt Sanctga auch gegen einen einzelnen Gegner; die Anzahl gilt dort nicht, weil der Abbruch der Grund ist, nicht der Schaden. |
| Sanctus-Streckung (A50, A272) | dieselbe Betäubung | Ein wirkender Gegner ist nicht betäubt; liegt die Betäubung des letzten Sanctga schon auf ihm, ist sein Wirken ohnehin gestoppt. Kein Konflikt. |
| Sanctus-Zurückhaltung für die Barriere des Dunkelritters und bei verlangsamter Gruppe | hält Sanctus zurück | Der Stopp geht vor: Ein laufender Angriff ist sicher, die Gründe der Zurückhaltung sind spätere oder Ressourcen-Gründe (seine Spielweise). |
| Heilung, Notfall | der GCD des Heilers | Steht ein Mitglied in der kritischen Klasse, gehört der GCD der Heilung. |
| Wiederbelebung, Swiftcast | der GCD, Spontanzauber | Swiftcast wird nie für den Stopp verbraucht (seine Vorgabe). |
| Unterbrechung und Betäubung anderer Spieler | dasselbe Wirken | Annahme über fremde Spieler, kein Grund zu warten; bei unterbrechbarem Wirken sagt die Protokollzeile, dass ein anderer es gewesen sein kann. |
| BossModReborn | – | Seine Module markieren Wirken zum Unterbrechen oder Betäuben, geben das aber nicht über IPC heraus (Stand 20.10.2026). |

### Erkennung

`SleepInterruptWatch`, ohne Option und ohne Schwelle:
- ein Gegner der Feindesliste, kein Boss (Rang 2 und 6 im Blatt BNpcBase), dessen Wirken eine Fläche ist, die dich
  oder zwei lebende Mitglieder erreicht (`DataCenter.AreaCastReachesParty`), **oder** das das Spiel als unterbrechbar
  führt, nach derselben Erkennung wie Stolperfalle der Nahkämpfer (`ObjectHelper.CanInterrupt`; mindestens 2 s, kein
  Kegel, kein Ring, keine Fläche unter 8 y);
- für den Schlaf: ohne Sleep Resistance, nicht von einer gespeicherten widerstehenden Art, in diesem Kampf seltener
  eingeschläfert, als seine Art (sonst irgendeine Art) vor einem Widerstand vertrug, und nicht gerade von einem Schlaf
  getroffen (sein Status zeigt sich erst später; am 21.10.2026 gingen zwei Schlafe auf dasselbe Wirken). Gezählt wird
  jeder Schlaf auf den Gegner, wer ihn auch wirkte; und seine Aktion ist nicht als vom Schlaf nicht stoppbar
  gespeichert (unten). Für die Betäubung gilt diese Sperre nicht: Eine Betäubung bricht auch ein Wirken ab, das unter
  Schlaf weiterläuft (`IsCastWorthStopping` ohne, `IsCastToStop` mit der Sperre).

### Entscheidung

`CustomRotation.StopEnemyCast`, für Heiler und Magier zusammen, im GCD-Pfad nach Notfall und Unterbrechung, vor
Reinigung, Herausforderung, Wiederbelebung und Heilung. Unter mehreren das Wirken, das zuerst endet:

1. *Der eigene Stopper des Jobs* (`CustomRotation.StopEnemyCastByJob`; Stufe „Job", weil nur der Weißmagier unter
   Heilern und Magiern eine Betäubung hat): Sanctga, wenn der Gegner im Umkreis steht, sich noch betäuben lässt
   (`StunWatch.CanStillBeStunned`, kein Boss) und die Zeit reicht.
2. *Sonst der Schlaf*, wenn die Zeit reicht.

„Die Zeit reicht" heißt für beide: Vom Wirken ist mehr übrig als die eigene Wirkzeit (null unter Dualcast oder
Triplecast), der laufende GCD, die gemessene Zeit vom Ende des eigenen Wirkens bis zur Wirkung (vor der ersten Messung
die Laufzeit eines Drucks) und der gelernte Rest beim Eintreffen. Nicht unter Swiftcast. Beim Heiler nicht, solange ein
Mitglied in der kritischen Klasse steht. Nicht, wenn die Flächenaktion unter „Don't attack new mobs by AoE." einen
Gegner träfe, der noch nicht kämpft.

### Gespeichertes Wissen

Nach dem Vorbild der gelernten Flächentabelle (`HostileCastingAreaPotential`): drei gelernte Tabellen in
`OtherConfiguration.Files`, beim Start geladen, bei jedem Ausgang gespeichert, beim Speichern mit der Datei
zusammengeführt (Zählungen und Zeiten nur höher, die Anzahl bis zur Abnutzung nur niedriger), nie geschrieben, wenn
das Laden nicht fertig wurde, unlesbar als `.corrupt` beiseitegelegt. Ohne Knopf zum Verwerfen: Die Datei löscht er bei geschlossenem Spiel. Der Ladezustand steht in der
Kopfzeile des Protokolls.

| Tabelle | Schlüssel | Inhalt | Wofür |
|---|---|---|---|
| `SleepResistantKinds.json` | Gegnerart (BaseId) | wie oft sie einen ersten Schlaf als Status oder Aktion abwehrte | Die Art wird nicht mehr eingeschläfert. |
| `SleepDiminishing.json` | Gegnerart (BaseId) | die wenigsten Schlafe in einem Kampf, nach denen sie widerstand (beim Zusammenführen die kleinere Zahl) | Ein Gegner wird in einem Kampf höchstens so oft eingeschläfert. |
| `SleepOutcomes.json` | Aktion des Gegners | Abbrüche und die längste Zeit von der Landung bis zum Abbruch; Fehlschläge und der größte Rest, mit dem der Schlaf trotzdem zu spät kam | Die Regel liest daraus bei jeder Entscheidung die Haltezeit, den nötigen Rest und die nicht stoppbaren Aktionen. |

Ausgewertet wird bei jeder Entscheidung:
- *Haltezeit:* die längste Zeit von der Landung bis zum Abbruch über alle Aktionen.
- *Nicht stoppbar:* eine Aktion ohne Abbruch, die losging, obwohl der Schlaf mindestens die Haltezeit vor ihrem Ende lag.
- *Nötiger Rest:* der größte Rest eines Fehlschlags, der unter der Haltezeit lag, plus ein Bild. Ein solcher Schlaf kam
  zu spät; die Aktion bleibt erlaubt. Vor dem ersten Abbruch gibt es keine Haltezeit: Dann zählt jeder Fehlschlag zum
  nötigen Rest, und erst ein Abbruch sortiert die Fehlschläge mit großem Rest zu den nicht stoppbaren Aktionen.

Ein Widerstand wird gespeichert, wenn das Spiel den Status oder die ganze Aktion als widerstanden meldet – seine
Vorgabe: Wo Schlaf nicht wirkt, soll er nicht immer wieder versucht werden. Lag in diesem Kampf schon ein Schlaf auf
dem Gegner, ist es die Abnutzung (sein Hinweis vom 23.10.2026), nicht die Immunität der Art: Gespeichert wird dann
die Anzahl, und der Gegner ruht für diesen Kampf. „Unverwundbar" (etwa in einer Übergangsphase) sperrt die Art nur
für die Sitzung, weil es keine Immunität gegen Schlaf beweist. Die Laufzeit vom
Wirken zur Wirkung hängt an der Verbindung und wird in jeder Sitzung neu gemessen.

### Protokoll

| Ausgang | Zeile in `DefenseTrace.log` |
|---|---|
| Stopper des Jobs gewählt | „stop a cast -> Sanctga …" |
| Schlaf gewählt | „sleep to stop a cast -> …" (Gegner, Art, Aktion, Restzeit, Unterbrechbarkeit, benötigte Zeit) |
| gewählt, aber nicht gewirkt | „sleep not cast on …" |
| widerstanden | „sleep resisted by …: immune, its kind is stored" bzw. „… after N sleep(s) in this fight: worn down" |
| unverwundbar | „sleep did not reach …" (nur für die Sitzung) |
| Wirken bricht ab | „sleep stopped …" mit der Zeit seit der Landung |
| zu spät | „sleep did not stop …; too late" mit dem neuen nötigen Rest |
| nicht stoppbar | „sleep did not stop …; … not tried on it again" |
| nach dem Ende gelandet, Gegner gefallen, zugleich betäubt | wie bisher |

### Warum so

- Seine Spielweise: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit. Ein laufendes Wirken
  landet, wenn niemand es abbricht.
- Sanctga vor dem Schlaf (seine Präzisierung): Er betäubt jeden Gegner im Umkreis, macht Schaden und kostet weniger MP;
  der Schlaf kostet einen GCD ohne Schaden und wird vom nächsten Treffer geweckt.
- Universell zuerst: Die Regel gilt für jede Rolle mit einem Schlaf; der Stopper des Jobs ist ein Einhängepunkt, den
  bisher nur der Weißmagier füllt.
- Ein Fehlschlag mit wenig Rest beweist den falschen Zeitpunkt, nicht eine nicht stoppbare Aktion.
- Voreinstellung an, nach seiner Regel (29.09.2026).

### Ausgeschlossen

- Swiftcast für den Stopp (seine Vorgabe).
- Bosse.

### Grenzen und Lagen

- *Kurzes Wirken:* Beide brauchen ihre Wirkzeit, den laufenden GCD, die Laufzeit und den Rest; ein kurzes Wirken lässt
  keinen Platz. Rotmagier mit Dualcast und Schwarzmagier mit Triplecast schlafen sofort und verbrauchen den Sofortzauber.
- *Betäubung erschöpft:* Nach wiederholter Betäubung ist ein Gegner vorübergehend immun (`StunWatch`); dann bleibt der
  Schlaf.
- *Lange Kämpfe:* Die Abnutzung endet nach einer Weile wieder; die Regel sperrt den Gegner trotzdem bis zum Kampfende,
  weil die Dauer unbelegt ist. Das kostet höchstens einen möglichen Schlaf auf einem langlebigen Gegner.
- *Wiederholtes Wirken:* Wirkt der geweckte Gegner erneut, kostet jeder Abbruch wieder einen GCD.
- *Vierer-Instanz:* die Hauptlage. *Allianzraid:* gezählt wird die eigene Gruppe.
- *Stufensynchron:* Schlaf ab Stufe 10, Schlummer ab 8, Sanctus ab 45, Sanctga ab 82 (`EnoughLevel`).
- *Zwei Rechner:* Beide Tabellen werden beim Speichern mit der Datei zusammengeführt; gleichzeitig laufende Spiele
  verlieren keine Messung.

**Falsifikation.**
- *Kein Defekt:* widerlegt – ein Schlaf, wo Sanctga betäubt hätte, kostet einen GCD und den Schaden; jede Sitzung
  begann leer.
- *Option falsch:*
  - Sanctga gegen einen einzelnen Gegner kostet gegenüber Blendka rund 160 Potenz; der Schlaf kostet den ganzen GCD.
  - Eine verbrauchte Betäubung fehlt der Streckung später; ein laufender Angriff wiegt schwerer (seine Spielweise).
  - Eine falsch gespeicherte nicht stoppbare Aktion bliebe für den Schlaf gesperrt; Sanctga versucht sie weiter. Sie
    wird nur gespeichert, wenn der Schlaf mindestens die gemessene Haltezeit vor dem Ende lag und kein Abbruch der
    Aktion bekannt ist.
- *Ausgeliefert, nichts ändert sich:* Steht ein Wirkender außerhalb von 8 y, bleibt es beim Schlaf; ohne Fehlschlag
  bleibt der nötige Rest null. Beides zeigt das Protokoll.

**Prüfgrad:** Protokolle gelesen; statisch, Prüfskripte, Compile lokal; im Spiel nicht beobachtet. Die Regel lernt
selbst, das Protokoll zeigt es.

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
"mindestens ein Gegner ist weder betaeubt noch immun", in allen drei Aussetzregeln und beim Dunkelritter.
Immun heißt, was der Kampf gezeigt hat (`StunWatch`, A272): eine Betäubung, die das Spiel als widerstanden
meldet, oder eine betäubende Aktion, die traf, ohne zu betäuben. Die Abnahme nach jeder Anwendung zeigt das
Spiel nicht als Status; „Stun Resistance" (39, 1349) ist die Dauerimmunität bestimmter Gegner (xivapi).

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

### Der Grenzwert gilt für den gesamten Schadenseingang

**Es geht nicht um die Verlangsamung, sondern um die Kontrolle des eingehenden Schadens.** Dazu
dienen Reflexion, The Blackest Night und die uebrigen Minderungen des Tanks ebenso. Der
Schadenseingang **einschliesslich eingerechneter Schadensreduktion und Mitigation** soll zu jedem
Zeitpunkt bestimmte Grenzwerte nicht ueberschreiten.

**Was der Baum davon misst**, vollstaendig, weil die Frage groesser ist als die Sanctus-Regel, an der sie
auffiel:

| Was zu messen waere | Stand im Baum |
|---|---|
| Drosselung **auf der Gegnerseite** — Slow, Reflexion, Feint, Stumpfsinn, Dismantle | `HostileOutputPercent`, je Satz aus dem Wirktext belegt |
| Minderung **auf der eigenen Seite**, gruppenweit — Sacred Soil, Temperance, Troubadour | `GetCurrentMitigationPercent`, aber fuer einen **einzelnen bevorstehenden Treffer** gebaut, mit Magisch/Physisch-Heuristik, nicht fuer den Dauerstrom |
| Minderung **des Tanks persoenlich** — Rampart, Bollwerk, Sentinel, Schattenwall, Vengeance | **nicht als Satz gemessen.** `StatusHelper.RampartStatus` fuehrt die Ids und dient als Doppelbelegungssperre (`StatusProvide`) und als Ja/Nein-Pruefung „gedeckt“ (`HasMajorMitigation`, `HoldReprisalForRaidwide`) — nie zur Messung |
| Die Saetze dieser Minderungen | **liegen vor**, aus den Wirktexten erzeugt (`DefensiveValues.g.cs`); die Ratenrechnung braucht sie nicht, weil der beobachtete Verlauf netto ist (unten) |

**Der ganze Anspruch ist aus Laufzeitbeobachtung erfuellt, ohne eine einzige statische Vorgabe.** Das
ist der Weg mit dem kleinsten Eingriff und der groessten Deckung, weil die Maschinerie dafuer bereits im
Baum stand; sie war nur nicht auf die Gruppe angewandt.

`DataCenter.RecordedHP` ist eine Zeitreihe von Gesundheitsanteilen **je Objekt-Id**: einmal je
Sekunde ein Eintrag, 240 tief, also vier Minuten Historie. `ObjectHelper.GetTTK` liest daraus fuer
eine beliebige Id den Verlauf, bildet einen gleitenden Mittelwert und schaetzt die Zeit bis auf null.
Die Methode fragt nichts weiter als `GameObjectId` — sie ist generisch. `TargetUpdater.UpdateTimeToKill`
fuellt die Reihe fuer die Gegner und fuer die Gruppe (A91); vorher lieferte sie fuer ein Gruppenmitglied
`NaN`.

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
der Tank gar nicht erst fallen soll. Was die Groesse nicht kann:

- **Blind fuer den ersten Treffer.** Vor 2,5 Sekunden Beobachtung (`CheckSpan`) liefert `GetTTK`
  `NaN`, und abgetastet wird einmal je Sekunde. Der Eroeffnungsschlag eines Pulls ist daraus nicht
  vorhersehbar — dafuer bleibt die Vorhersage von BossModReborn zustaendig. Ab dem zweiten Treffer
  hat die Reihe eine Rate, und die Vorausschau steht.
  **Für angekündigte Treffer ist die Grenze geschlossen, aus Beobachtung:** Ein angekuendigter Cast, dessen
  Schadenspotential aus frueheren Einschlaegen bekannt ist, sagt den ersten Treffer voraus, bevor er
  faellt (Konzept 13), und die Vorausheilung, das Minderungsurteil, die Schranke und der Tankbuster-Plan lesen ihn,
  ohne auf den Verlauf zu warten (A340). Offen bleibt der Auftakt eines Pulls ohne Ankündigung: Dort fehlt jede
  Größe außer der Gegnerzahl. Eine gelernte Rate je Gegner wäre eine neue Vorhersage für 2,5 s, in denen die
  Schwellen der Heilkette schon auf die aktuelle Gesundheit antworten und die Tanks ihre Pull-Abwehr nach der
  Gegnerzahl setzen – nicht gebaut. Die eigenen Minderungen einzurechnen hülfe hier nicht: Vor dem ersten Treffer
  gibt es keine Rate, die sie skalieren könnten.
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

**Einer Ablesung bedarf die Korrektur nicht.** Bleibt der Faktor ueber einen Pull hinweg nahe 1, wirkt
sie nicht; laeuft er hoch, sobald eine Gruppe anbindet, ist die Rohzahl die zu spaete, und die Regeln
lesen ohnehin die korrigierte.

### Der Verbraucher: vorausberechnete Gesundheit statt zweiter Mechanismus

**Eine Schwelle, die einen Stand vergleicht, verwechselt Pegel und Rate.** `HealthSingleAbility`,
`HealthTankRatio`, `HealthForDyingTanks` sind Staende; die Gefahr ist aber ein **Zufluss**: Wer schnell faellt, unterschreitet seine Schwelle mit weniger
Restzeit, als die dadurch ausgeloeste Heilung zum Ankommen braucht — GCD-Rest, dann Cast. Der Zauber
geht nach dem Tod heraus. Bei schwachem Zufluss ist dieselbe Schwelle frueh genug; die Korrektur
muss deshalb mit der Rate skalieren und darf kein fester Abschlag sein.

**Gewaehlt ist, die gelesene Groesse zu ersetzen, nicht einen Mechanismus danebenzustellen.** Die
Frage einer Regel lautet nicht mehr „wie steht dieses Mitglied", sondern „wie steht es, wenn meine
Heilung ankommt". Damit erben Schwellen, Rangstufen und Kurzschluesse die Vorausschau, ohne dass
einer von ihnen umgebaut wird.

**Das Tor entscheidet vor allen anderen.**
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
die es verstaerkt. Synastry waehlt seit A305 mit eigenem `TargetType.Heal` wie die Einzelheilung, die sie
wiederholt, und liest dabei den Vorlauf; vorher waehlte sie nach `Big` und traf das Heilziel nur ueber die
Zielueberschreibung, die eine Heil-GCD stehen liess. Beim
kurzen Vorlauf bleiben die oGCDs, die sofort wirken: Benediction, Essential Dignity, Taurochole,
Druochole, Haima, Excogitation (liegt bereit und loest selbst aus), Aetherpact, Second Wind, Equilibrium,
Thrill of Battle; ebenso Raw Intuition/Bloodwhetting und Nascent Flash, deren Minderung und Barriere
sofort wirken und deren Heilung mit jedem Waffenskill folgt. Bloodbath („Converts a portion of physical
damage dealt into HP") heilt mit dem naechsten physischen Treffer; zaehlen Autoangriffe dazu
(Schluss; das consolegameswiki, „Bloodbath“, 02.10.2026, nennt nur physischen gegen magischen Schaden, Autoangriffe nicht), liegt der kurze Vorlauf naeher als ein voller GCD, sonst um hoechstens den GCD-Rest
daneben.

**Verworfen: ein zweiter Ausloeser samt eigener Rangstufe.** Er waere der naheliegende Weg gewesen
und ist der schlechtere: Zwei Mechanismen, die dieselbe Frage entscheiden, laufen auseinander, sobald
einer von beiden angefasst wird. Ausserdem haette er zwei Haelften gebraucht, die einzeln wirkungslos
sind — ein Ausloeser ohne Zielwahl heilt den Falschen, eine Zielwahl ohne Ausloeser greift erst,
wenn ohnehin geheilt wird.

**Selbstbegrenzend im teuren Fall.** Steht die Gruppe stabil, ist der Nettotrend nicht fallend,
`GetTTK` liefert `NaN` und der Anteil ist 1 — kein Unterschied zum Stand ohne Vorausschau, kein zusaetzlicher Zauber,
kein MP. Die Vorausschau erscheint genau dann, wenn der Trend nach unten dreht, und waechst mit
seiner Steilheit.

**Was im Kampf anders wird, durchgerechnet.** GCD 2,5 s, halb abgelaufen, Vorlaufzeit 3,75 s.

| Lage | ohne Vorausschau | mit Vorausschau |
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
  gemessen. Abbrechen kann ihn nur „Stop single target GCD healing after reaching threshold" (ab Werk aus), und
  nur so, wie ihr Text sagt: Das Ziel brauchte die Heilung beim Beginn, ist seitdem geheilt worden und braucht sie
  nicht mehr (`CancelCastUpdater.ShouldStopHealing`, A332). Flacht nur der Trend ab, ohne dass jemand heilt, laeuft
  der Cast weiter. Seit Upstream 7.5.7.5 wirkt diese Einstellung ueberhaupt; bis A332 fragte sie, ob noch irgendwer
  eine Heilung braucht, und brach damit eine Heilung fuer vorhergesagte Gefahr ab, sobald der Trend abflachte.

**Standard an** (seit 29.09.2026, seine Regel: Voreinstellung ist der im Kampf sinnvollere Wert). Belegt ist die
Wirkung mit den hier verfuegbaren Mitteln nicht — statische Pruefung und Kompilierung sagen nichts darueber, ob
der Tank steht. Bei ausgeschalteter Einstellung liefern alle vier Getter die Werte ohne Vorausschau, die
Nullvariante ist also eingebaut.

**Eine Groesse, die angekommenen Schaden schon misst, gibt es — sie traegt den Anspruch aber nicht.**
`DataCenter.DPSTaken` misst den **tatsaechlich angekommenen** Schaden, also bereits nach allen Minderungen und
Mitigationen, ueber die letzten fuenf Sekunden (seit upstream 7.5.7.5; vorher fuenf Millisekunden und damit fast
immer nichts, A331). Sie misst aber die Gruppe als Ganzes — `DamageRec` traegt kein Ziel — und sagt deshalb nicht,
wem der Strom gilt; die Vorausschau braucht die Rate je Mitglied. Sie ist Upstream-Code, und ihr einziger Leser im
Baum ist die Diagnoseanzeige (Stand 22.10.2026).

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

**Zaehler und Nenner sind beide vorhanden.** Der Puffer einschliesslich Barriere wird gelesen
(`GetEffectiveHp`), die Rate je Mitglied steht seit A91 (`RecordedHP`, `GetCorrectedTTK`), und die Vorausschau
oben rechnet aus beiden. Die Barriere hebt den Puffer, aus dem die Ueberlebensbewertung ihre Antwort zieht.

**Die Restzeit der Barriere selbst waere die zweite Haelfte dieser Frage.** `StatusHelper.HasSurvivingShield` wurde
dafuer gebaut und hat keinen Leser (Stand 12.10.2026; der Tankbuster-Plan liest die Schildrestzeit selbst); ihr bekannter Defekt — sie misst die **kuerzeste**
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

**Gehalten wird, solange die Bedingung gilt, ohne Ersatzvorbehalt** (A178). Der gehaltene GCD fällt auf
die DoTs, solange einer fehlt, danach auf Glare. **Preis, im Kampf:** Ab drei Gegnern trifft Sanctus in Summe
mehr als Glare; jeder gehaltene GCD nach dem letzten DoT kostet diese Differenz an Schaden, und der Pull
dauert entsprechend länger. Dafür steht die Betäubung zur Verfügung, wenn die Verlangsamung abläuft — das ist
der Zweck seiner Regel. Die Schranke (ein Gruppenmitglied fällt) und der abschaltbare Deckel bleiben.
**Warum ohne Vorbehalt:** Ein Ersatzvorbehalt — ausgesetzt nur, solange Dia oder Aero noch ein Ziel ohne DoT
hat — war beim Aussetzen nach einem DoT je Gegner verbraucht, und Sanctus fiel in die noch laufende
Verlangsamung; so seine Beobachtung vom 27.09.2026: „nach abtausch wird holy wie gewünscht ausgesetzt, bis alle
gegner gemach haben. dann setzt holy aber wieder ein" (Gemach ist der deutsche Name des Slow-Status 9). Der
Vorbehalt war meine Abwägung, nicht seine Regel; die Streckung trägt ihn ebenfalls nicht (A231).

Umgesetzt als `WHM_Reborn.ShouldHoldHolyWhilePackSlowed` über `SurveyHostileStatus` im Wirkbereich
von Sanctus; die Mindestzahl steht als `HoldHolyMinSlowedHostiles` (Vorgabewert 3) hinter derselben
Einstellung. Streng mehr als die Hälfte: 3 von 5 hält, 3 von 6 hält nicht, 4 von 6 hält.

**Ein Anteil, keine Leistungssumme.** Die Restleistung aller Gegner im Radius gegen `AoeCount * 100`
beantwortet „lohnt sich für diesen Pull noch ein Flächenzauber", und die Antwort lautet fast immer ja: Ein
Gegner mehr als `AoeCount` trägt für sich mindestens 80 und hebt die Summe über die Schwelle, gleich wie
viele verlangsamt sind (fünf Gegner mit vier verlangsamten: 432 gegen 300). Als Auslöser griff sie im
Wall-to-Wall nie (C52, C59). Die Regel hat zu fragen: „wird der Strom bereits gebändigt" — ein Anteil.
Die Leistungsrechnung bleibt als **abschaltbarer Deckel** hinter der gemessenen Schranke
(`AnyPartyMemberFallingWithinHealWindow`), Standard aus. `HostileOutputPercent` und `SurveyHostileOutput`
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
wird, ob ein Gegner verlangsamt ist, nicht wie stark. Welche der Ids Abtausch setzt, nennen weder Wirktext
noch Aktionsdaten; gezählt wird deshalb die ganze Gruppe.

## Warum die Streckung richtig ist

### Die Größenordnung

Die Betäubungsressource ist **einmalig je Pull**, nicht wiederkehrend: Erste
Anwendung 4 s, zweite 2 s, dritte 1 s, danach Immunität. The Balance bestätigt die Halbierung und die Summe (White
Mage Basic Guide, 02.10.2026: „halving its duration with each application until they become temporarily immune to
stun … the cumulative 7s of stun helps mitigate a significant amount of tank damage in trash pulls"); wie lange
die Immunität hält, nennt es nicht (45 s steht ohne Quelle). Die Regeln brauchen die Zahl nicht: `StunWatch` liest
Anwendungen und Immunität aus dem Kampf (A272). Zu verteilen sind damit genau **7 Sekunden
Betäubung**, und die Frage lautet nicht, wie viele Zyklen man unterbringt, sondern
wie man diese sieben Sekunden legt.

**Posten 1 — Ausnutzung der Dauern.** Bei einem GCD von rund 2,5 s deckt
ununterbrochenes Nachcasten 0–4,5 s und 5,0–6,0 s ab, zusammen etwa **5,5 s**: Die
zweite Anwendung fällt, während die erste noch läuft, und ihre kürzere Dauer verfällt
teilweise. Wird jeweils erst nach Ablauf nachgecastet, stehen die vollen **7 s** zur
Verfügung.

**Posten 2 — Entzerrung gegen fremde Mitigation.** Reprisal senkt den Gegnerschaden
um 10 % für 15 s (unterhalb von Stufe 98 für 10 s). Liegt die Betäubung darin, ist ihr Reprisal-Anteil verloren:
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

### Grenzen der Begründung

- **Der Einschub im mittleren Gegnerzahlbereich** kostet einen GCD Schaden je Pull. Die Streckung ist Schutz, und
  Sicherheit geht vor Schaden (seine Vorgabe und Spielweise); eine Bindung an eine Gegnerzahl gibt es deshalb nicht
  (A231).
- **Die Übertragung auf weitere Aktionen** hat die Erhebung (`scan16.py`) auf Abtausch eingegrenzt; gebaut (A194).
- **Nicht belegt ist, dass der Nutzen im Spiel eintritt.** Die Rechnung ist ein Modell.

## Die Lücke

**Aktionen mit doppelter Wirkung sind nur nach einer ihrer Wirkungen eingeordnet.**
Sanctus steht im Schadenszweig; dass es betäubt, ist im Entscheidungsmodell nicht
vorhanden. Assize steht im Angriffs-oGCD (`WHM_Reborn.AttackAbility`); dass es heilt,
ebenfalls nicht. Umgekehrt kennt `GetCurrentMitigationPercent` die Wirkung von
Reprisal, aber weder Betäubung noch Verlangsamung, obwohl beide wie
Schadensreduktion wirken. Beide werden an der Stelle gelesen, an der sie eine
Entscheidung ändern — der Barrierenregel des Dunkelritters —, in der
Minderungsbilanz selbst nicht.

Die Erhebung dazu ist geführt und liegt als `scan16.py` im Repository: Sie nimmt
jede PvE-Aktion, deren Wirktext eine Kontroll- oder Minderungswirkung auf Gegner
nennt, und fragt, ob der Baum den zugehörigen Status irgendwo liest. Im
Tank- und Heilerprofil bleibt **eine** Aktion übrig, deren zweite Wirkung
nirgends gelesen wurde: **Abtausch (Arm’s Length)** — gelesen (A194, A236: Abtausch im Pull für seine
Verlangsamung). Sie war als Rückstoßschutz eingeordnet
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
gewollt ist, zeigt der DoT selbst: `ModifyDiaPvE` (`WhiteMageRotation.cs:334-345`)
setzt `TargetStatusProvide` gegen Nachlegen und `IsRestrictedDOT` gegen ungeeignete
Ziele — beide Vorkehrungen laufen bei AoE ins Leere, weil der Zweig nicht erreicht
wird. Seit A231 fällt der GCD nach einem Sanctus auf den fälligen DoT (Abschnitt „Die Regel“).

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

**Die Betäubung, die noch nicht in der Statusliste steht.** Wirkzeit und Erholzeit sind gleich lang;
der nächste GCD ist in dem Moment frei, in dem der erste Sanctus landet, und seine Betäubung steht
dann womöglich noch nicht in der Statusliste der Gegner. Die Effektmeldung des Sanctus sagt aber schon,
wen er betäubt hat: `StunWatch` zählt je Gegner die Anwendungen und rechnet die gerade gelandete mit
der Dauer aus dem Wirktext (`DefensiveValues.DurationOf`, Sanctus 4 s), halbiert je vorheriger
Anwendung (The Balance: „halving its duration with each application"). Sobald die Betäubung in der
Statusliste steht, gilt deren Restzeit, und ihre volle Länge korrigiert die Zählung.

**Seine Bedingung über allem.** Läuft im Radius keine Betäubung und hat der eben gelandete Sanctus
niemanden betäubt, gibt es nichts zu strecken; Sanctus geht hinaus. Bis A272 galt ein Gegner ohne
Betäubung und ohne Resistenzstatus als frisch und bekam eine volle Anwendung zugerechnet; weil das Spiel
die Abnahme nicht als Status zeigt, hielt die Regel so auch Rudel fest, die nicht mehr betäubt werden
konnten — in seinem Protokoll vom 04.–10.10.2026 bei 134 von 180 Wahlen nach einem Sanctus.

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

Anlass war seine Beobachtung vom 29.09.2026: Das zweite Sanctus fiel sofort, statt die Betäubung auslaufen
zu lassen. Diese drei Wege und der Vergleich mit einer noch nicht stehenden Betäubung ließen es fallen.
`DefenseTrace.log` nennt je Entscheidung nach einem Sanctus den Grund mit Restzeit und Landezeit.

## Vorhandene Bausteine

Der Entwurf erfindet wenig; das meiste lag im Baum und war nur nicht verbunden.

| Baustein | Fundstelle | Zustand |
|---|---|---|
| Mitigationsmessung: Gegner-Debuffs und Party-Buffs, verrechnet zu einem Schadensfaktor | `CustomRotation.GetCurrentMitigationPercent` | Gelesen nur zur Anzeige (`MainWindow_Debug`, Stand 12.10.2026) |
| Betäubung und Verlangsamung als Statuseffekte | `StatusHelper.StunStatus`, `.SlowStatus`, `StatusID.ArmsLength` | In Betrieb. „Stun Resistance" (39, 1349) ist die Dauerimmunität bestimmter Gegner, nicht die Abnahme nach jeder Anwendung; die zeigt das Spiel nicht als Status, `StunWatch` liest sie aus dem Ereignisstrom (A272). Ob es eine Abnahme der Verlangsamung gibt und ob sie sichtbar ist, ist unbelegt |
| Statusabfragen mit Restzeit und Stapelzahl | `StatusHelper.HasStatus`, `.StatusTime`, `.StatusStack` | In Betrieb |
| Vorhersagefenster aus der BossModReborn-Timeline | `Configs.BMRRaidwideMitWindow` und Geschwister, ausgewertet in `StateUpdater` | In Betrieb |
| Zentralisierte Nachzieh-Regel für Gegner-Debuffs | `CustomRotation.ShouldSustainMitigationDebuff` | In Betrieb, 25 Aufrufstellen in den Standardrotationen (Stand 26.09.2026) — Beleg, dass eine gemeinsame Regel über viele Jobs trägt |
| Gegnerzahl-Schwelle als etabliertes Muster | `Configs.MitigationSustainHostileCount` gegen `NumberOfHostilesInRange` | In Betrieb |
| Trennung von Mitigation und Schaden im Dispatch | `CustomRotation_GCD.cs`, Reihenfolge in Konzept 03 („Die Slot-Kette“): Heilung, dann Abwehr, `GeneralGCD` zuletzt | In Betrieb |

## Ausgeschlossen

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

## Prüfmittel

### Selbstprüfung zur Laufzeit statt Ablesung

**Eine Wirksamkeitsmessung im Spiel ist verworfen, nach seiner Vorgabe** („entscheidung immer im spiel, nicht retroperspektive auswertung“): Kennzahlen je
Pull (erlittener Schadensanteil, genutzte Betäubungsdauer, Überlappungsanteil, DoT-Laufzeit) und eine
Versuchsanordnung mit der Option abwechselnd an und aus. Beides wartete auf seine Ablesung und machte ihn zum Teil
des Regelkreises. Was die Regeln dieses Konzepts zur Laufzeit selbst prüft: die Vorausschau gegen ihren eigenen
Fehler (`ScoreTtkForecast`), die proaktive Zurückhaltung gegen ihre Trefferbilanz (oben); `DefenseTrace.log` nennt
je Entscheidung nach einem Sanctus den Grund, für den Fall, dass ein Defekt nur zur Laufzeit zu finden ist.

### Statische Prüfung

| Ebene | Möglich |
|---|---|
| Statisch | `stun_coverage.py` simuliert die Regel GCD für GCD und trägt seinen Selbsttest; für die Übertragung zusätzlich ein Skript, das Aktionen mit Mitigationswirkung im Schadenszweig findet |
| Kompilierung | CI |

Jeder Schritt steht hinter einer eigenen Option; voreingestellt ist der im Kampf sinnvollere Wert (seine Regel,
29.09.2026).

## Betroffene

**Endnutzer.** Beim Weißmagier fällt Sanctus seltener hintereinander: Die Betäubung deckt
nach dem Modell 7 statt 5,5 Sekunden je Pull, und während einer fremden Verlangsamung hält er den
Flächenzauber zurück; die frei werdenden GCDs gehen an DoTs oder Glare. Spürbar im großen
Flächenpull; im Einzelzielkampf fällt Sanctus nicht (eigene Zielzahl der Aktion).

**Autoren abgeleiteter Rotationen.** Die Messung ergänzt die öffentliche Oberfläche
von `RotationSolver.Basic` additiv; keine Signatur bricht. Wer die neuen Prüfungen
nicht aufruft, merkt nichts.

**Upstream-Pflege.** Der Eingriff liegt in `CustomRotation_OtherInfo` und
`WHM_Reborn`, beides Dateien mit regelmäßiger Upstream-Aktivität. Neue Teile liegen in
eigenen Regionen; kein Block wandert.

## Offene Punkte

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.
