# 18 · Einstellungen: Entscheidung des Spielers oder Automatik

**Leitfrage:** Welche Einstellung trifft eine Entscheidung, die nur der Spieler treffen kann, und welche beantwortet
eine Frage, die das Spiel selbst oder eine Auswertung zur Laufzeit besser beantwortet?

Sein Auftrag (25.10.2026): „bei allen einstellungen, wo entscheidungen zu treffen sind (an/aus, werte in ui
einzutragen), ob dies richtige entscheidungen für den spieler sind, oder ob einige davon eher automatismen im spiel
sind, die durch routinen und ingame auswertungen besser geregelt werden können."

## Sachstand

- **Eine Grenze galt nicht, wo ihr Text sie verspricht – behoben.** „Only use damaging gap closers within this
  distance" (ab Werk 3 Yalm) verspricht, dass jeder Gapcloser mit Schaden nur auf nähere Ziele geht. Die Zielwahl
  las die Grenze nur für die reinen Vorstöße (Onslaught, Trajectory, Shadowstride, Corps-a-corps, Gyoten). Die
  Sprungangriffe – Intervene, Primal Rend, Crimson Cyclone, Stardiver, Dragonfire Dive, Forked Raiju – laufen über die
  gewöhnliche Zielwahl und lasen sie nicht. Der Paladin sprang mit Intervene aus bis zu 20 Yalm auf sein Ziel, sobald
  er stand; der Dragoon mit Stardiver und Dragonfire Dive ebenso, weil seine eigenen Grenzen ab Werk 20 Yalm sind. Jetzt
  liest jeder dieser Sprünge die zentrale Grenze (`CustomRotation.GapCloserAllowed`), eine Grenze der Rotation gilt
  zusätzlich (unten, „Abstand für Sprungangriffe").
- **Die Heilschwellen auf den Gruppendurchschnitt lasen den Stand – behoben.** „Heal ahead of incoming damage"
  verspricht die Vorausschau für jede Heilschwelle; sechs Schwellen der Heiler-Rotationen und die Füllheilung lasen
  die Gesundheit jetzt statt beim Eintreffen der Heilung (unten, „Heilung").
- **„Use GCDs to heal" weicht der Gefahr – behoben.** Aus, überließ die Einstellung GCD-Heilungen dem anderen Heiler (bei
  Paladin, Rotmagier und Beschwörer den Heilern) auch dann, wenn ein Mitglied gleich fällt; Astrologe und Weiser haben
  sie ab Werk aus. Jetzt heilt die Rotation, sobald ein Mitglied in der kritischen Klasse steht (unten,
  „Rotationseinstellungen"; A360).
- **Die Wiederbelebung wartet ab Werk nicht mehr.** „Raise delay range" stand ab Werk auf 3 s, eine Tarnung ohne
  Nutzen im Kampf; ab Werk jetzt 0 bis 0 s (unten, „Tarnung"; Konzept 11).
- **Alles Übrige ist eingeordnet, ohne Änderung.** Die übrigen Zahlen, die eine Spieltatsache tragen, haben entweder
  keinen messbaren Gewinn (Vorlaufzeiten der BossModReborn-Zeitleiste, Lucid Dreaming), oder ihre Ableitung gehört in
  den Abgleich mit The Balance (Leisten- und Statusgrenzen der Rotationen).
- **Die Pull-Abwehr der Tanks liest jetzt den gemessenen Verlauf.** Die Gegnerzahl öffnet sie weiter, die große
  Minderung und Schutzwall warten aber, bis der Verlauf sie braucht (Konzept 09, „Die Abwehr im Pull").
- **„Engage settings" bleibt seine Entscheidung** (der Fall, an dem der Auftrag entstand, Konzept 04): Wen RSR angreift,
  entscheidet, welche Gruppe gezogen wird – im Dungeon die Frage, ob ein zweites Rudel mitkommt.

## Maßstab

Eine Einstellung ist **seine Entscheidung**, wenn das Spiel die Frage nicht beantworten kann: eine Vorliebe, eine
Haltung zum Risiko, ein Preis außerhalb des Kampfes (Tränke, Tinkturen), eine Steuerung (Auto, Manuell, Befehle), die
Darstellung, eine Absprache mit der Gruppe. Sie ist **eine Automatik**, wenn ihr Wert eine Tatsache des Spiels trägt –
Kosten, Dauer, Reichweite, Leistenhöchststand – oder eine Größe, die RSR misst – Schadensverlauf, Zeit bis zum Tod,
Latenz. Dann gilt seine Vorgabe „Keine festen Werte" (CLAUDE.md, „Entwurfsregeln für den Code").

Dazwischen liegen zwei Formen, die beide richtig sind:

| Form | Was der Spieler setzt | Was zur Laufzeit urteilt | Beispiel |
|---|---|---|---|
| Haltung mit Laufzeiturteil | die Grenze, ab der ihm etwas zu gefährlich ist | ob die Lage sie erreicht | Heilschwellen mit „Heal ahead of incoming damage" |
| Notausschalter | nur, ob eine Automatik läuft | alles andere | die Kampflogik je Inhalt, die Regeln des Forks |

Geprüft ist je Einstellung, was sie im Kampf bewirkt (der Leser im Code), nicht ihr Name.

## Bestand

Erhoben am 25.10.2026 am Zweig: 298 Einstellungen mit eigener Beschriftung in `Configs` und acht Heilschwellen je Job
ohne eigene Beschriftung (in der Tabelle unter „Automatic healing thresholds"); in den 22 Standardrotationen seiner
Jobs (PvE, `RebornRotations`) 243 Rotationseinstellungen, davon 151 Schalter, 74 Zahlen, 18 Auswahlen und Texte. Außerhalb seines
Profils erfasst, nicht bewertet: 19 PvP-Einstellungen, 13 der Maskerade, die Rotationen für PvP, Inhalte und
begrenzte Jobs, die Zusatzrotationen.

| Gruppe | Zahl | Einordnung |
|---|---|---|
| Darstellung (Fenster, Zeitleiste, Farben, Größen, Lehrmodus) | 49 | seine Entscheidung |
| Steuerung (Ein- und Ausschalten, Befehle, Abfangen, Bewegungssperre, Ablaufverfolgung) | 40 | seine Entscheidung |
| Kampflogik je Inhalt (Savage bis Schatzgewölbe) | 36 | Notausschalter: das Urteil liest den Zustand des Kampfes |
| Aktionen (Flächen, Verbrauchsgüter, Unterbrechen, Burst, Abwehr an und aus) | 34 | seine Entscheidung; Ausnahmen unten |
| Heilung | 28 | Haltung mit Laufzeiturteil; Regeln des Forks als Notausschalter |
| Ziele | 28 | seine Entscheidung; Ausnahmen unten |
| Abwehr | 20 | Regeln des Forks als Notausschalter; Zahlen unten |
| Wiederbelebung | 9 | seine Entscheidung; Verzögerung unten |
| Tarnung (Zufallsverzögerungen) | 9 | seine Entscheidung, Voreinstellung nach dem Kampf |
| Sonderfunktionen (Bewegungssperren, Fallschaden, Sperrzeiten) | 7 | seine Entscheidung |
| Takt (Countdown-Vorlauf, Vorlauf des GCD, Aktualisierung) | 6 | Laufzeitgröße, unten |

## Einordnung im Einzelnen

### Darstellung, Steuerung, Sonderfunktionen

Was ein Fenster zeigt, wann RSR an- und ausgeht, welche Tasten er selbst drückt, ob ein Haken ins Spiel eingreift (kein
Fallschaden): Das Spiel kennt seine Wünsche nicht. Keine Automatik denkbar, keine Änderung.

### Tarnung: Zufallsverzögerungen

Neun Einstellungen lassen RSR vor einer Aktion zufällig warten, damit er weniger mechanisch aussieht. Das ist seine
Entscheidung – wie sichtbar er RSR nutzt, weiß nur er –, aber ihr Preis fällt im Kampf an. Nach seiner Vorgabe zur
Voreinstellung (der Wert, der im Spielgeschehen sinnvoller ist) wartet RSR ab Werk dort nicht, wo Warten etwas kostet:

| Einstellung | ab Werk | Begründung |
|---|---|---|
| „Interrupt delay range", „Provoke delay range" | 0 bis 0 s | eine wartende Unterbrechung verpasst ihr Wirken; eine wartende Provokation lässt den Gegner länger beim Falschen (Konzept 11) |
| „Raise delay range" | 0 bis 0 s | der Gefallene liegt länger; er nimmt selbst an, wann er aufsteht (Konzept 11) |
| „Dispel delay range", „Auto-target delay range" | 0 bis 0 s | schon so |
| „Downtime healing delay range" | 0,5 bis 1 s, bleibt | sie entprellt „nichts zu tun": Ein Augenblick ohne Ziel – ein Rudel stirbt, das nächste ist noch nicht gewählt – löste sonst eine Füllheilung statt des nächsten Angriffs aus (Upstream 5b16e58ed, „fix: add delay for heal when nothing to do") |
| „Out of combat delay range" | 2 bis 3 s, bleibt | sie entprellt das Kampfende für Sprint und Peloton; zwischen zwei Rudeln fällt der Kampfzustand kurz ab |
| „Stop casting delay range" | 0,5 bis 1 s, bleibt | nur mit „Stop casting when the target dies" (ab Werk aus); ob das Spiel einen Zauber auf ein totes Ziel ohnehin abbricht, ist nicht belegt – ohne Beleg kein Eingriff |
| „Delay between clicks" | 0,1 bis 0,2 s, bleibt | kürzer als die Animationssperre jeder Aktion (das Spiel sperrt nach einer Aktion länger, als die Spanne höchstens erlaubt, 0,25 s): kein Preis im Kampf |
| „Presses per action" | 2 bis 3, bleibt | simulierte Tastendrücke auf der Leiste, kein Einfluss auf die Aktion |

Seine eigenen Werte sind von hier aus nicht messbar; die Verzögerung der Wiederbelebung hatte er auf 0 gesetzt (Archiv,
C35).

### Ziele

Wen RSR angreift („Engage settings"), welche Markierungen gelten, FATE-, Jagd- und Questziele, Bildschirm und
Sichtkegel, Übungspuppen: Absichten, die das Spiel nicht kennt.

- **„Engage settings" bleibt seine Entscheidung.** Ein Angriff auf einen ungebundenen Gegner zieht ihn. Ab Werk greift
  RSR in der Gruppe nur gebundene Gegner an, also zählt auch die Flächenschwelle eines Tanks nur sie (Konzept 04,
  „Bilanz im Kampf"). Ungebundene Gegner mitzuzählen hieße, mit der Fläche zu ziehen, was der Spieler nicht gezogen
  hat – ein zweites Rudel, eine Patrouille. Den Preis eines solchen Fehlgriffs trägt die Gruppe, der Gewinn wäre eine
  Fläche einen GCD früher; Sicherheit vor Schaden. *Gegenposition:* Upstream beschriftet „All targets that are in range
  for any abilities (Tanks/Autoduty)" als Wahl für Tanks, und ein Tank im Wall-to-Wall will jedes Rudel auf dem Weg.
  Nur weiß RSR nicht, welches Rudel er mitnehmen will – das Rudel vor der Bosstür, die Patrouille im Rücken –, also
  bleibt die Wahl je Job bei ihm, die Voreinstellung bei der vorsichtigen. „Don't pull new enemies with AoE" (ab Werk
  aus) ist dieselbe Entscheidung für die Flächen aller Jobs.
- **„Boss time to kill" (90 s), „Dying time to kill" (10 s), „Dying HP" (2 %)** sind Stellvertreter. Ob ein Gegner ein
  Boss ist, sagt das Spiel selbst (`IsBossFromIcon`, Rang im Blatt `BNpcBase`); der Beschwörer liest beide Wege. Was
  „stirbt bald" heißt, hängt am Verbraucher – ob sich ein DoT, ein Ressourcenabbau, eine Positionierung noch lohnt –
  und gehört in dessen Regel, nicht in eine gemeinsame Zahl. Kein Gewinn im Kampf belegt; ein Umbau würde jede
  Rotation berühren, die `IsDying` liest. Nicht geändert.
- **„Only use damaging gap closers within this distance"** ist seine Haltung zum Risiko, in seinem Profil gesetzt
  durch seine Spielweise (nichts, was ihn aus sicherer Position holt). Ihre Lücke: unten.

### Abstand für Sprungangriffe

**Eine Grenze für alle, die Rotationen nur enger.** Die zentrale Grenze gilt für jeden Angriff, der den Spieler auf
sein Ziel trägt. Nicht begrenzt sind: ein Vorwärts-Befehl, diese Aktion, wenn er sie selbst in die Warteschlange
gelegt hat (abgefangener Druck oder `/rotation`-Befehl), der Countdown, und ein Schalter der Rotation, den er
ausdrücklich für Sprünge ohne Rücksicht auf den Abstand einschaltet. Der Zustand „Abfangen" selbst öffnet nichts: Er
gilt nach jedem Druck einige Sekunden lang, auch nach einer Heilung, und hätte in dieser Zeit jeden Sprung freigegeben.

| Aktion | Typ | Grenze vorher | Grenze jetzt |
|---|---|---|---|
| Onslaught, Trajectory, Shadowstride, Corps-a-corps, Hissatsu: Gyoten | Vorstoß | zentral | zentral, unverändert |
| Intervene (Paladin) | Sprungangriff | **keine** | zentral (`PaladinRotation`, gilt für jede Paladin-Rotation) |
| Stardiver, Dragonfire Dive (Dragoon) | Sprungangriff | die Rotation, ab Werk 20 Yalm | zentral und die Rotation |
| Primal Rend (Krieger) | Sprungangriff | die Rotation, ab Werk 3,5 Yalm | zentral und die Rotation; „Dangerous"-Schalter unverändert |
| Crimson Cyclone (Beschwörer) | Sprungangriff | die Rotation, ab Werk 3 Yalm | zentral und die Rotation; „at any range"-Schalter unverändert |
| Forked Raiju (Ninja) | Sprungangriff | nur mit „Use Forked Raiju … outside of range (Dangerous)" oder auf Befehl | unverändert |

*Wie es dazu kam.* Bis Mai 2026 war Intervene ein Vorstoß und las die Grenze über die Zielwahl der Vorstöße. Upstream
gab den Sprungangriffen einen eigenen Typ (33e6acb1f, 07.05.2026), damit sie die gewöhnliche Zielwahl nehmen – ein
Angriff braucht ein gültiges feindliches Ziel, die Zielwahl der Vorstöße nimmt das harte Ziel oder das fernste im
Kegel. Die Grenze stand in der Zielwahl der Vorstöße und blieb dort. Primal Rend und Crimson Cyclone hatten vor dem
Umbau keinen wirksamen Vorstoßtyp (die Zeile stand auskommentiert) und brachten eigene Grenzen mit; Stardiver,
Dragonfire Dive und Forked Raiju bekamen den Typ im Fork (A174), für die Bewegungsprüfung mit BossModReborn.

*Warum nicht in der Zielwahl.* Dort hätte die Grenze die ausdrücklichen „Dangerous"-Schalter des Kriegers, des
Beschwörers und des Ninja außer Kraft gesetzt, deren Text Sprünge auf jeden Abstand verspricht. Die Grenze sitzt
deshalb als Zielfilter bei Intervene, das keinen solchen Schalter hat, und als Bedingung neben der eigenen Grenze der
übrigen Rotationen. Eine künftige Sprungaktion ohne beides läse sie nicht; das ist der Preis, benannt in „Grenzen".

*Ausgeschlossen:* die Grenzen der Rotationen entfernen – sie dürfen enger sein als die zentrale, und ihr Text („Max
distance") verspricht nur eine Obergrenze, die weiter gilt; die Rückwärtssprünge mit Angriff (Displacement, Yaten)
begrenzen – sie schließen keine Lücke, sie öffnen eine.

### Heilung

**Haltung mit Laufzeiturteil, so gebaut.** Die acht Heilschwellen je Job, die Rollenschwellen („Heal tanks first",
„… healers first", „… yourself first") und die Schwelle für Füllheilungen sind seine Grenzen. Wann eine Lage sie
erreicht, urteilt die Laufzeit: „Heal ahead of incoming damage" liest statt der Gesundheit jetzt die beim Eintreffen
einer jetzt begonnenen Heilung (Konzept 08), „Choose the heal target by danger" die Gefahr statt der Rolle (Konzept 07).
Jede Heilschwelle der Standardrotationen der Heiler liest diese Vorausschau: die auf ein Mitglied über
`GetForecastHealthRatio`, die auf den Gruppendurchschnitt über `PartyMembersForecastAverHP`, die Füllheilung („When
the lowest party HP is below") über das vorausberechnete Minimum.

- **Die Schwellen auf den Gruppendurchschnitt lasen den Stand – behoben.** Horoscope und Lady of Crowns (Astrologe),
  Holos und Pneuma (Weiser), Fey Blessing und Indomitability unter der Notfallschwelle sowie Recitation mit
  Indomitability (Gelehrter) verglichen `PartyMembersAverHP`, die Füllheilung `PartyMembersMinHP` und
  `PartyMembersDifferHP` – den Stand, nicht die Gesundheit beim Eintreffen. Der Text von „Heal ahead of incoming
  damage" nennt jede Heilschwelle; A182 hatte die zentralen Flächenschwellen umgestellt, diese blieben. Beim Weisen
  stand der Widerspruch in einer Bedingung: der Tank nach Vorausschau, die Gruppe nach Stand. Im Kampf: Pneuma als
  Gruppenheilung (ein GCD mit Wirkzeit) fällt um die Strecke früher, die die Gruppe bis zum Eintreffen verliert; bei
  den Fähigkeiten reicht die Vorausschau nur über die Animationssperre, dort ändert sich fast nichts. Mit der
  Vorausschau aus sind alle Werte die bisherigen.

- **Schutzschwellen der Tanks lesen die Gesundheit ohne Vorausschau** (Cover, Intervention, The Blackest Night,
  Tengentsu des Samurai). Keine Lücke: Die Vorausschau einer sofort wirkenden Fähigkeit reicht nur über die
  Animationssperre (`GetForecastHealthRatio(instant)`), und der Text von „Heal ahead of incoming damage" nennt
  Heilschwellen, nicht Schutz.
- **„AoE heal HP spread"** wählt zwischen Flächen- und Einzelheilung über die Streuung der Gesundheit. Eine Rechnung über
  die wirksame Heilmenge je Mitglied wäre genauer; kein Fall im Kampf belegt, dass die Streuung falsch wählt. Nicht
  geändert.
- **„Stop healing when enemies will die within"** gilt nur für Heilungen von Tanks und Damage Dealern (`StateUpdater`);
  ein Heiler heilt unabhängig davon. Klein, nicht geändert.
- **„Heal a target under an invulnerability only below this HP"** ist eine Haltung (wie tief ein Tank unter
  Unverwundbarkeit fallen darf); Living Dead folgt eigenen Regeln (Konzept 09).

### Abwehr

Die Regeln des Forks (Tankbuster-Plan, Übernahme vom Mit-Tank, Streckung der Gruppenwerkzeuge, „Spend only the
mitigation a measured tankbuster needs") urteilen selbst und sind ab Werk an; ihr Schalter ist Notausschalter.

- **Pull-Abwehr: Gegnerzahl als Vorbote, der Verlauf für die großen Abklingzeiten.** „Use tank mitigation when this
  many enemies target you" (ab Werk 2) öffnet die Einzelabwehr; kurze Minderung, Abtausch und Reflexion fallen sofort.
  Die große Minderung und Schutzwall warten, bis seine Gesundheit beim gemessenen Verlauf innerhalb ihrer Wirkdauer
  unter „Health the big mitigation waits for" (ab Werk 30 %) fiele – eine Haltung mit Laufzeiturteil wie die
  Heilschwellen. Im Regelkreis-Modell in jedem gerechneten Fall weniger Tote und weniger Zeit unter 30 % als die reine
  Zählung (Konzept 09, „Die Abwehr im Pull"). Die Gegnerzahl bleibt, weil vor dem ersten Treffer nichts anderes
  vorhersagt; „Use tank mitigation below this HP" (ab Werk 100 %, also immer) bleibt als Pegel über der ganzen Abwehr.
- **Ohne Heiler.** „Without a living healer, spend your defence to stay alive longer" (ab Werk an) ist eine Regel, die
  selbst urteilt: Lebt kein Heiler, öffnet die Einzelabwehr bei jedem Angriff auf ihn, und eine Minderung geht, sobald
  sie bis zum nächsten fälligen Tankbuster wieder bereit ist – fällig nach BossModReborn oder nach dem Abstand der
  Tankbuster in diesem Kampf (Konzept 09, „Ohne Heiler: den eigenen Tod hinauszögern"). Keine Zahl für ihn: Ob ein
  Heiler lebt, wann der nächste Tankbuster fällig ist und wie lange eine Abklingzeit läuft, liefert das Spiel. Der
  Schalter ist Notausschalter.
- **„Use invulnerability below this HP"** ist die letzte Stufe unter dem Tankbuster-Plan; die Stufe darüber urteilt
  am gemessenen Treffer (Konzept 09). Haltung, nicht geändert.
- **Vorlaufzeiten der BossModReborn-Zeitleiste** (Fläche 5 s, Tankbuster 3 s, Rückstoß 3 s; nur mit „Use BossModReborn's
  timeline for proactive mitigation", ab Werk aus). Der Wert trägt eine Spieltatsache: Die Minderung muss einen
  Einwebplatz vor dem Treffer finden (ein GCD und die Animationssperre) und darf nicht vor ihm ablaufen (ihre
  Wirkdauer, die die Jobregeln über `BMRShouldRefreshBefore` schon lesen). Bei 2,5 s GCD erfüllen die Vorgaben beide
  Seiten. Eine Untergrenze aus dem GCD änderte nur einen Wert, den er selbst unter einen GCD setzt – gegen den Text
  der Einstellung, die ihm die Wahl gibt. Die Ungenauigkeit der Vorhersage von BossModReborn ist nicht gemessen; sie
  wäre der eigentliche Grund für einen Spielraum. Nicht geändert.

### Wiederbelebung, Verbrauchsgüter, MP

- **Wen und womit wiederbelebt wird**, ob Swiftcast dafür bleibt: seine Entscheidung (Swiftcast bleibt für
  Wiederbelebungen, CLAUDE.md, „Spielweise").
- **„Don't raise below this MP"** (2400) ist ab Werk genau der Preis der Wiederbelebung; das Spiel prüft den Preis
  ohnehin, die Einstellung wirkt erst darüber, als Rücklage für Heilungen. Seine Haltung. Der Weißmagier wirkt unter Thin
  Air auch darunter (`WhiteMageRotation`).
- **Tränke, Tinkturen, Phönixfeder:** Sie kosten außerhalb des Kampfes; seine Entscheidung.
- **„Use Lucid Dreaming below this MP"** (6000) trägt eine Spieltatsache, das Überlaufen der MP während der Wirkung.
  Die Grenze hängt am eigenen Verbrauch, den RSR nicht misst; der Unterschied im Kampf betrifft nur MP. Nicht geändert.

### Takt

„Countdown lead time" und „Action ahead" hängen an der Latenz, die Einstellungen der Animationssperre sind Teil eines
Schalters, der ab Werk aus ist. Eine Selbststeuerung aus gemessener Latenz wäre denkbar; kein Fehlverhalten im Kampf
belegt. Seine Einstellung, nicht geändert.

### Kampflogik je Inhalt

36 Schalter, ab Werk an, je einer für die Sonderlogik eines Kampfes (Immunität ohne passenden Status, Zielreihenfolge).
Das Urteil liest den Zustand des Kampfes; der Schalter nimmt sie heraus, wenn sie einmal falsch greift. Richtig so.

### Rotationseinstellungen

| Art | Beispiele | Einordnung |
|---|---|---|
| Heil- und Schutzschwellen | „Health threshold for Cover", Essential Dignity je Ladung, Taurochole | Haltung; die Heilschwellen der Heiler lesen die Vorausschau |
| Gegnerzahlen | Aspected Benefic und Eukrasian Diagnosis vor dem Pull, Holy-Rückhaltung des Weißmagiers, The Blackest Night im Pull | Haltung, Vorbote vor dem ersten Treffer; wie die Pull-Abwehr der Tanks |
| GCD-Heilungen anderen überlassen | „Use GCDs to heal" (Heiler: dem zweiten Heiler; Paladin, Rotmagier, Beschwörer: den Heilern) | Haltung (Schaden statt GCD-Heilung, solange andere heilen), seit A360 mit der Gefahrenausnahme: Steht ein Mitglied in der kritischen Klasse, heilt die Rotation trotzdem – sonst hinge das Überleben am Verhalten des anderen Spielers |
| ausdrückliche Gefahrenschalter | Primal Rend „Dangerous", Crimson Cyclone „at any range", Forked Raiju „Dangerous" | seine Entscheidung gegen die eigene Sicherheit; ab Werk aus |
| Abstände | Stardiver, Dragonfire Dive, Primal Rend, Crimson Cyclone | jetzt unter der zentralen Grenze |
| Werte aus Leiste, Status und Abklingzeit | Sheltron bei 100 Oath, Paint-Grenze 5, Restzeiten für Reawaken, Death's Design bei 10 s, Lieddauern des Barden | Spieltatsachen; abzuleiten im Abgleich je Job mit The Balance (`TODO.md`, „Rotationen gegen The Balance abgleichen"), weil dort ohnehin jede Regel des Jobs gegen den Guide steht |
| Opener, Reihenfolgen | Opener des Weisen und Dunkelritters, Liedfolge, Beschwörungsreihenfolge | Strategie; der Abgleich mit The Balance setzt die Voreinstellung |

**Einzeln geprüft** (zweiter Durchgang, sein Auftrag vom 25.10.2026 abends): jede der 243 Einstellungen an ihren Lesern
im Code.
- *Leser:* jede wird gelesen.
- *Text gegen Code:* ein Widerspruch, „Use GCDs to heal" (oben, behoben). Was „Set to 0 to disable" verspricht, hält der
  Code (Sheltron prüft `> 0`, Intervention und Cover mit einer Schwelle, die bei 0 nie greift).
- *Swiftcast:* Ab Werk verbrauchen ihn nur die beiden Schalter des Piktomanten (Rainbow Drip, Motiv); er hat keine
  Wiederbelebung. Die Rotations-Swiftcasts des Beschwörers sind ab Werk aus, die Heiler halten ihn mit „Swiftcast
  restriction" für die Wiederbelebung, der Weißmagier die letzte Ladung von Thin Air.
- *Bewegung:* Jeder ausdrückliche Gefahrenschalter (Primal Rend, Crimson Cyclone, Forked Raiju, Displacement nach
  Engagement, Corps-a-corps in Bewegung) ist ab Werk aus. Die Vorstöße, die die Rotationen ab Werk nutzen (Onslaught im
  Burst, Corps-a-corps), stehen unter der zentralen Abstandsgrenze.
- *Für den Abgleich mit The Balance vorgemerkt* (Spieltatsache oder Strategie, keine Spielerentscheidung):
  - Restzeiten von Swift und Hunt vor Reawaken (je 10 s) und der Bündelzeitpunkt vor Serpent's Ire (75 s), Viper
  - Paint-Grenze 5, Piktomant – abgeglichen (A385): fünf Ladungen sind die Obergrenze des Spiels (The Balance); die
    Einstellung erlaubt eine niedrigere Schwelle
  - Lieddauern 43/43/34 s, Barde – abgeglichen (A382): The Balance nennt dieselben Zeiten; die Liedlänge selbst kommt
    jetzt aus dem Spiel
  - Death's Design bei 10 s, Schnitter – abgeglichen (A383): The Balance nennt keine Zahl; die Einstellung gilt nur mit
    der eigenen Zeitwahl, ab Werk aus, sonst plant die zentrale DoT-Planung
  - Meikyo Shisui bei 14 s im Countdown, Samurai – abgeglichen (A381): der Opener von The Balance setzt ihn bei −14 s
  - Earthly Star bei 4 s im Countdown, Astrologe
- *Aus dem Abgleich mit The Balance umgestellt:* „Use Leylines in combat when standing still" (Schwarzmagier) ist ab
  Werk an (A386). Ab Werk aus setzte RSR Ley Lines nie; The Balance setzt sie etwa auf Abklingzeit, und ein vertaner
  Kreis kostet Schaden, keine Sicherheit (Konzept 14, „Schwarzmagier: Feuer und Eis, Ley Lines").
  - die Schätzung „8 % der eigenen Gesundheit als Gruppenschaden je Sekunde", mit der der Gelehrte Bio auf kurzlebige
    Ziele vermeidet – eine Stellvertretergröße neben der gemessenen Zeit bis zum Tod, die die zentrale DoT-Planung
    schon liest

## Bilanz im Kampf

**Abstand für Sprungangriffe.** *Stärke:* Ein Paladin, der für eine Mechanik abseits steht, springt nicht mehr mit
Intervene zurück auf den Boss – in die Fläche, der er ausgewichen ist; ein Dragoon nicht mehr mit Stardiver oder
Dragonfire Dive aus 20 Yalm. Am Ziel (unter 3 Yalm) ist jeder Sprung wie bisher nur Schaden. *Schwäche:* Steht er
länger abseits, wartet der Sprung. Intervene hat zwei Ladungen bei 30 s Abklingzeit – eine Ladung geht nur verloren,
wenn er lange genug fernbleibt, dass beide voll sind. Stardiver ist an Life of the Dragon gebunden (20 s); bleibt der
Dragoon die ganze Zeit fern, entfällt er und mit ihm Starcross. Dragonfire Dive (120 s) verschiebt sich nur. Werte aus
dem Job-Guide, abgerufen am 25.10.2026. *Chance:* Dieselbe Prüfung steht jeder Rotation offen (`GapCloserAllowed`,
öffentlich im Paket). *Bedrohung:* Ein Spieler, der die Sprünge
als Rückweg zum Boss nutzte, läuft jetzt selbst; sein Vorwärts-Befehl hebt die Grenze auf. *Überwiegt:* die Sicherheit.
Der Fall, in dem der Sprung schadet – er steht abseits, weil dort eine Mechanik liegt –, ist der wahrscheinliche
Grund, abseits zu stehen; der Verlust ist ein verschobener oder einzelner verlorener Angriff.

**Heilschwellen auf den Gruppendurchschnitt.** *Stärke:* Pneuma als Gruppenheilung geht los, wenn die Gruppe beim
Eintreffen unter der Schwelle stünde, nicht erst, wenn sie schon darunter steht – im Takt der übrigen Heilschwellen.
*Schwäche:* Wie jede Vorausschau schlägt sie nach einer kleinen Delle etwas früher an. Für das einzelne Mitglied kostete
das im Regelkreis keine zusätzliche Heilung (Konzept 08, A356); für den Durchschnitt ist das nicht eigens modelliert,
sondern daraus geschlossen, weil er aus denselben Einzelwerten gebildet wird. Bei den Fähigkeiten reicht die
Vorausschau nur über die Animationssperre. *Chance:*
`PartyMembersForecastAverHP` steht jeder Rotation offen. *Bedrohung:* keine neue – mit der Vorausschau aus bleibt alles
beim Stand. *Überwiegt:* der Nutzen, klein, und die Schwellen einer Rotation meinen wieder denselben Zeitpunkt.

**Verzögerung der Wiederbelebung.** *Stärke:* Ein Gefallener steht bis zu 3 s früher auf. *Schwäche:* keine im Kampf;
zwei Wiederbeleber auf denselben Leichnam verhindert der Ausschluss, sobald einer wirkt (Konzept 11). *Bedrohung:*
RSR wirkt sichtbar schneller als ein Mensch – das ist die Abwägung, die der Spieler mit der Einstellung trifft.
*Überwiegt:* der Nutzen; seine eigene Einstellung stand ohnehin auf 0.

## Grenzen

- Die Grenze für Sprungangriffe sitzt bei den Aktionen, nicht in der Zielwahl. Eine neue Sprungaktion von Upstream ohne
  eigene Prüfung läse sie nicht; beim Abgleich eines Syncs ist jede neue Aktion vom Typ `HostileMovingAttack` darauf
  zu prüfen.
- Die Zusatzrotationen (`ExtraRotations`) sind nicht angepasst: Wer dort Intervene nutzt, bekommt die Grenze über
  `PaladinRotation`; Forked Raiju in `BeirutaNIN` bleibt, wie diese Rotation es baut.
- Seine Konfiguration ist nicht messbar. Eine Aussage über eine Voreinstellung sagt nichts über seinen Wert.
- *Prüfgrad:* statisch am Code (jeder Leser der genannten Einstellungen), Versionsgeschichte für die Herkunft,
  Compile; im Spiel nicht beobachtet. Die Wirkung ist ohne Laufzeitfrage: Der Sprung fällt oder nicht, je nach Abstand.
