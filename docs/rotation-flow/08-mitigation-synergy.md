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
| Aussetzbedingung aus dem **Betaeubungsgrund**, hinter `StretchHolyStun` (Standard aus) | umgesetzt (`WHM_Reborn.ShouldStretchHolyStun`) |
| Aussetzbedingung aus dem **Mitigationsgrund** — eine fremde Minderung traegt bereits | umgesetzt (`WHM_Reborn.ShouldHoldHolyWhilePackSlowed`, Standard an) |
| **Stunbarkeit** als Bedingung ueber allen drei Aussetzregeln | umgesetzt (`headroom` aus `SurveyStuns`) |
| Aussetzbedingung als **Anteil** der verlangsamten Gegner, mit Mindestzahl | umgesetzt (`HoldHolyMinSlowedHostiles`, Standard 3) |
| **Schranke** der Aussetzregel: der Rest muss bewaeltigbar sein | umgesetzt und **gemessen statt gesetzt**: `AnyPartyMemberFallingWithinHealWindow`. Die frühere Zahl (`HoldHolyMaxHostileOutput` 600) war meine Setzung und ist zum optionalen Deckel mit Standard 0 = aus geworden |
| **Schadensrate je Gruppenmitglied**, netto nach allem | umgesetzt: die Gruppe steht in `RecordedHP`, `GetTTK` antwortet fuer sie |
| **Selbstkorrektur** der Schaetzung gegen ihren eigenen Fehler | umgesetzt (`ScoreTtkForecast`, `GetCorrectedTTK`); Rohzeit, korrigierte Zeit und Faktor stehen in der Diagnoseanzeige |
| **Vorausschau** als Ersatzgroesse an allen Heilentscheidungen | umgesetzt (`GetForecastSurvivingShare` und die drei davon abgeleiteten Getter), hinter `HealAheadOfDamage`, Standard aus |
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
  Krieger eine, Dunkelritter eine, Beschwoerer eine. Der Maler haelt zwei, aber Tempera Grassa
  **entfernt** Tempera Coat („Removes Tempera Coat to create a barrier…") — eine Umwandlung, keine
  Alternative. Die uebrigen sind Bozja-Aktionen ausserhalb des Nutzungsprofils.

**Die Auswahlregel war gebaut und ist zurueckgebaut worden**, nachdem die Falsifikationsstufe das
ergeben hat (A118). Sie haette im ganzen Baum nie gegriffen.

**Was stattdessen wirkt, ist Stufe 2 — und sie schliesst eine Luecke, die dieses Konzept ohnehin
fuehrt.** Siehe „Heilung vor dem angekuendigten Treffer" weiter unten.

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

**Beim Beschwoerer trifft das auf die Zuendregel unten.** Steht die Flaechenheilungsflagge wegen
eines angekuendigten Treffers, ist Lux Solaris der Zweig, der sie bedient — und der Wurf faellt
**vor** dem Einschlag statt danach.

## Wann eine verfallende Heilung zuendet

**Die Heilschwellen sind fuer die teure Heilung eines Heilers gebaut, und fuer eine verfallende
Nebenheilung sind sie das falsche Mass.** `AutoStatus.HealAreaAbility` verlangt zweierlei zugleich:
die Streuung der Gruppengesundheit unter `HealthDifference` (0,25 im Code) **und** ihren Durchschnitt
unter `HealthAreaAbility` (0,75 im Code; beides je Job einstellbar, seine eigenen Werte sind von hier
nicht messbar). Die Streuungsbedingung ist der Grund, warum eine Flaechenheilung ausbleibt, wenn
**einer** getroffen wurde: Genau dann ist die Streuung gross. Das ist fuer einen Heilzauber richtig —
eine teure Flaechenheilung fuer einen einzelnen Verletzten ist der falsche Tausch.

**Fuer eine Aktion, die ohnehin verfaellt, ist es der falsche Tausch in die andere Richtung.** Lux
Solaris kostet kein MP und keinen GCD; ihr einziger Preis ist der Einschiebeplatz, und sie erlischt
mit Refulgent Lux. Die Frage lautet dort nicht „lohnt Flaechenheilung“, sondern **„ist dieser Wurf
verschwendet“**.

**Vorgabe des Auftraggebers, woertlich:** „Hier besteht aber nur eine gewisse Zeit die Möglichkeit zu
heilen. Am besten, wenn die bestehe Gesundheit gerade so hoch ist, dass die Heilung auf 100 % der Hp
kommt.“

| Lage | Antwort |
|---|---|
| Fehlbetrag kleiner als die Heilung | warten — der Ueberschuss verpufft, und das Fenster laeuft noch |
| Fehlbetrag erreicht die Heilung | zuenden — sie kommt vollstaendig an |
| Fenster laeuft aus, irgendjemand ist verletzt | zuenden — ungenutzt ist sie ganz verloren |

**Die Groesse dafuer ist der gemessene Heilwert, nicht die Potenz.** 500 Potenz sind von hier aus
nicht in Lebenspunkte umzurechnen: Heilkraft, Ausruestung und Verstaerkungen entscheiden darueber,
und sie aendern sich. Gemessen wird sie stattdessen — der Effekt-Handler sieht jede eigene Heilung
mit ihrem tatsaechlichen Wert (`Watcher.ActionFromSelf`, `ActionEffectType.Heal`), und
`DataCenter.GetObservedHealPerCast` gibt ihn geglaettet zurueck. **Das ist die selbstkorrigierende
Sonde, die dieses Konzept von jeder Regelaenderung verlangt:** Sie erhebt und bewertet im selben
Zug, korrigiert sich mit jedem Wurf, folgt einem Ausruestungswechsel innerhalb weniger Einsaetze und
verlangt vom Auftraggeber kein Ablesen.

**Der Anlauf ist benannt:** Vor der ersten beobachteten Landung ist der Wert 0, und 0 heisst
*unbekannt*, nicht *heilt nichts*. Dann gilt das bisherige Verhalten — Heilflagge plus
Verfallsklausel —, statt eine Zahl anzunehmen. Ebenfalls benannt: Der Wert ist ein **absoluter**
Betrag und trifft jedes Mitglied mit einem anderen Anteil; verglichen wird er deshalb mit dem
groessten Fehlbetrag der Gruppe (`DataCenter.LargestMissingHp`), nicht mit einem Durchschnittsanteil.
Kritische Heilungen streuen den Messwert, weshalb er geglaettet und nicht ueberschrieben wird.

**Die Verfallsklausel kostet hier fast nichts, und das ist am Wirktext belegt:** Refulgent Lux laeuft
30 s, die Demi-Phase 15 s. Die letzten GCDs des Status liegen also **hinter** der Burstphase, wo der
Angriffszweig duenn ist — der Einschiebeplatz, den die Klausel dort nimmt, ist kein Burstplatz.

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
Vorlaufzeit = GCD-Rest + ein voller GCD
```

Beides aus dem Spielzustand, keine gesetzte Zahl. Ein Heiler unter Presence of Mind blickt kuerzer
voraus — richtig, er kann frueher handeln.

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

**Standard aus.** Die Wirkung ist mit den hier verfuegbaren Mitteln nicht zu belegen — statische
Pruefung und Kompilierung sagen nichts darueber, ob der Tank steht. Bei ausgeschalteter Einstellung
liefern alle vier Getter exakt die heutigen Werte, die Nullvariante ist also eingebaut.

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
wird, ob ein Gegner verlangsamt ist, nicht wie stark. Welche Id Rückstoß tatsächlich setzt, ist von
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
nirgends gelesen wurde: **Rückstoß (Arm’s Length)**. Sie ist als Rückstoßschutz eingeordnet
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

Drei Gesichtspunkte sind eine einzige Frage: **Wann lohnt es sich, einen GCD nicht in
Sanctus zu stecken?**

- **Der Grund:** Der DoT fehlt oder läuft aus, der eingeschobene Cast hat eigenen Wert.
- **Das Timing:** Läuft die Betäubung noch, streckt der Einschub sie, statt sie zu
  überschreiben. Ist das Ziel bereits resistent oder immun, ist die Betäubung ohnehin
  kein Argument mehr.
- **Der zweite Grund für dasselbe Timing:** Läuft eine fremde Mitigation, ist ein Stun
  jetzt weniger wert als später.

> Ein Nicht-Sanctus-GCD wird eingeschoben, wenn er eigenen Wert hat **und** die
> Betäubung dadurch nicht verloren geht — weil sie noch läuft, weil sie ohnehin nicht
> mehr wirkt, oder weil gerade eine stärkere Mitigation trägt.

### Ein einziger Einschub genügt

Bei einem GCD von 2,5 s und Stundauern von 4 / 2 / 1 s:

| Verlauf | Betäubungsdeckung | Intervalle |
|---|---|---|
| ohne Einschub, Casts bei 0 / 2,5 / 5,0 | **5,5 s** | 0–4,5 · 5,0–6,0 |
| ein Einschub, Casts bei 0 / 5,0 / 7,5 | **7,0 s** | 0–4,0 · 5,0–7,0 · 7,5–8,5 |
| zwei Einschübe, Casts bei 0 / 5,0 / 10,0 | **7,0 s** | 0–4,0 · 5,0–7,0 · 10,0–11,0 |

Der zweite Einschub bringt nichts mehr und kostet einen weiteren GCD. Der DoT reicht
also exakt aus. Die Zahlen stammen aus `.github/scripts/audit/stun_coverage.py`, das
die Regel GCD für GCD simuliert und im Repository liegt; bei verkürztem GCD (2,0 s
unter Presence of Mind) liefert die Regel sogar lückenlose Deckung von 0 bis 7 s.

### Warum die Bedingung so lautet, wie sie lautet

**Nicht über eine Restzeit, sondern über zwei Wahrheitswerte.** Eine Restzeit
beschreibt einen einzelnen Gegner. Im Pull kommen laufend ungestunnte Gegner hinzu;
dann sagt die Restzeit über die bereits Betäubten nichts über die Neuzugänge, und die
Regel würde strecken, obwohl ein Cast die Neuen mit **voller** Dauer erwischt hätte —
die Resistenz zählt je Gegner. Gestreckt wird deshalb, wenn **alle** Gegner im Radius
betäubt sind, oder wenn **keiner** von ihnen noch betäubt werden kann — in
beiden Fällen zusätzlich nur, solange überhaupt **eine Betäubung läuft**. Ohne
diesen Zusatz griff die zweite Hälfte auch nach der abgearbeiteten
Betäubungskette, wenn alle Gegner immun und keiner mehr betäubt ist: Dort gibt es
nichts zu schützen, und die Regel tauschte für den Rest des Pulls einen
Flächenzauber gegen einen Einzelzielzauber (A50). Bei einem einzelnen Gegner ist
die Bedingung gleichbedeutend mit der Restzeit-Bedingung.

Eine Schwelle der Form `Restzeit > ein GCD` wäre zusätzlich falsch gewesen: Nach dem
ersten Sanctus beträgt die Restzeit beim nächsten GCD noch 1,5 s, die Regel hätte den
einen nötigen Einschub gerade verhindert.

**Über den Wirkradius, nicht über die Jobreichweite.** `SurveyStuns` bekommt den
Wirkradius der Aktion (`HolyIiiPvE.Info.EffectRange`), nicht `DataCenter.JobRange` —
für einen Heiler sind das 25 Yalms Angriffsreichweite, nicht der Wirkradius von
Sanctus. Über die Jobreichweite gemessen hätten ferne, nie betäubte Gegner die Deckung
dauerhaft unvollständig erscheinen lassen und die Regel nie greifen lassen.
Nebenwirkung dieser Wahl: Die Messung gehört zur Aktion, nicht in einen
jobunabhängigen Updater — die Einhängung in den `MajorUpdater` und die dafür nötige
Änderung der Aktualisierungsreihenfolge entfallen.

**Über eine Statusgruppe, nicht über eine einzelne Id.** Die Spieldaten führen `Stun`
und rund ein Dutzend Varianten mit Zahlensuffix; welche davon Sanctus anlegt, ist
offline nicht bestimmbar. `StatusHelper.StunStatus` fasst sie nach dem im Projekt
etablierten Muster zusammen. Damit ist die Frage gegenstandslos, und fremde
Betäubungen zählen mit — was erwünscht ist, weil auch sie Schaden verhindern.

**Mit Ersatzgarantie.** Sanctus wird nur ausgesetzt, wenn ein Cast mit eigenem Wert
bereitsteht (`DiaPvE`, `AeroIiPvE`, `AeroPvE`). Fehlt er, ist Sanctus die richtige
Wahl; ein Ausweichen auf Glare wäre ein reiner Verlust.

## Vorhandene Bausteine

Der Entwurf erfindet wenig; das meiste lag im Baum und war nur nicht verbunden.

| Baustein | Fundstelle | Zustand |
|---|---|---|
| Mitigationsmessung: Gegner-Debuffs und Party-Buffs, verrechnet zu einem Schadensfaktor | `CustomRotation_OtherInfo.cs:534` | Gelesen nur zur Anzeige (`RotationConfigWindow.cs:4863`) |
| Betäubung, Verlangsamung und **deren Resistenzen** als Statuseffekte | `StatusID.Stun`, `.StunResistance`, `.Slow`, `.SlowResistance`, `.ArmsLength` | In den Spieldaten vorhanden; die Resistenzstufe ist damit direkt lesbar, eine eigene Buchführung über den Ereignisstrom ist **nicht** nötig |
| Statusabfragen mit Restzeit und Stapelzahl | `StatusHelper.HasStatus`, `.StatusTime`, `.StatusStack` | In Betrieb |
| Vorhersagefenster aus der BossModReborn-Timeline | `Configs.cs:742`, `:747`, ausgewertet in `StateUpdater.cs:185` | In Betrieb |
| Zentralisierte Nachzieh-Regel für Gegner-Debuffs | `CustomRotation_OtherInfo.cs:1327` | In Betrieb, 27 Aufrufstellen — Beleg, dass eine gemeinsame Regel über viele Jobs trägt |
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

**Der Einschub ist im mittleren Gegnerzahlbereich falsch.** **Hält stand.** Bei etwa
fünf bis sieben Zielen ist weder der DoT-Wert noch der Betäubungswert groß, der
entgangene Sanctus aber schon spürbar. Die Regel ist deshalb nicht als „immer"
formuliert, sondern an `StretchHolyMinHostiles` gebunden; dieser Bereich ist der
schwächste Teil.

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

Solange die Wirksamkeitsmessung nicht vorliegt, gilt die Feature-Toggle-Regel
unverändert: **jeder Schritt hinter eine eigene Option, Standard aus.** Die Messung
ist der Weg, diese Vorsichtsmaßnahme später begründet aufzuheben — sie gehört deshalb
vor die Übertragung auf weitere Aktionen.

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
