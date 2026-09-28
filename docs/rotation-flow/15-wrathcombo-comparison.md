# 15 · Vergleich mit WrathCombo: Ideen für RSR

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie steht in
`AUDIT_LOG.md` (A169).

## Ergebnis

**Auftrag (seine Vorgabe, 27.09.2026):** „sind darin ideen zu rotationen und unseren bisherigen patches
vorhanden, die den eigenen plugin verbessern könnten? im loop nach erledigter arbeit prüfen".

**Quelle und ihr Status:** `PunishXIV/WrathCombo`, Stand 25.09.2026, BSD-3-Lizenz, nur gelesen
(Kopie unter `/home/user/punishxiv/wrathcombo`, keine Änderung dort). Ein anderes Rotations-Plugin, keine
Spielquelle: Es zeigt, wie ein zweites Werkzeug entscheidet. Spielmechanik ist am offiziellen Job-Guide
belegt, wo sie trägt. Code wird nicht übernommen.

**Umgesetzt (A169), weil RSR dort auf etwas wartete, das nicht kommt:**
- **Maschinist:** Mit „Only use Wildfire on Boss targets" hielt RSR Heat auf jedem Nicht-Boss für ein
  Wildfire zurück, das dort nie fällt; Hypercharge kam erst bei 100 Heat. Im Kampf: gegen Trash weniger
  Überhitzungen, Heat läuft über. Jetzt fällt Hypercharge dort wie ohne Wildfire-Ausrichtung, sobald die
  Werkzeuge es zulassen, und nur, wenn die Kombo die Überhitzung überdauert (Dauer aus dem Wirktext).
  WrathCombo macht es ebenso (`ShouldUseHyperchargeST`).

**Schon vorhanden in RSR (kein Handlungsbedarf):**
- Tankbuster- und Sammeltreffer-Erkennung über Zielmarkierungen (VFX): RSR hat sie, laut Kommentar
  nach WrathCombo gebaut; die Pfadlisten unterscheiden sich in einzelnen Einträgen (siehe Vorschläge).
- Wiederbelebungsreihenfolge und Heiler-Vorrang (Konzept 11), Oath-Überlauf des Paladins
  (`WhenToSheltron`), Überlauf-Schutz von Lilien, Polyglot, Beast Gauge, Munition.
- Samurai Meditate in der Pause (Konzept 14): WrathCombo verlangt zusätzlich einige Sekunden Stillstand.

**Bestätigt offene Entscheidungen (Konzept 14):** Addersgall-Schutz des Weisen (WrathCombo: Druochole ab
3 Stapeln), Shake It Off nur ohne eigene Minderungen im Selbstheilungspfad, Passage of Arms mit
Kanal-Sperre, Improvisation als kurzes Regen.

## Vorschläge, zur Entscheidung

| # | Idee (WrathCombo) | Was sich im Kampf ändert | Woran die Wahrscheinlichkeit gemessen wird | Einordnung |
|---|---|---|---|---|
| V1 | Vorbeugendes Regen oder Schild auf den Tank **ohne Countdown**, sobald er außerhalb des Kampfs nahe an Gegner kommt (`PreEmptiveHot`, `PreEmptiveShield`: Weißmagier Regen, Astrologe Aspected Benefic, Weiser Eukrasian Diagnosis, Gelehrter Adloquium) | Dungeon-Pulls: Der Tank hat Regen oder Schild, bevor der erste Treffer fällt. RSR tut das heute nur im Countdown (Weißmagier „UsePreRegen") | Tank außerhalb des Kampfs in Reichweite eines Gegners — der Pull ist nah | Stufe „Heiler"; neue Option, ab Werk aus bis zu seiner Entscheidung |
| V2 | Zielmarkierungen: allgemeiner Präfix `vfx/lockon/eff/tank` statt einzelner Tank-Marker, dazu die bei RSR fehlenden Einträge (`sharelaser2tank`, `share_1`, zwei Dungeon-Sammelmarker) | Mehr Tankbuster und Sammeltreffer werden erkannt, bevor sie fallen; Einzel- und Flächenabwehr öffnen öfter rechtzeitig | Marker über dem Ziel — der Treffer ist angekündigt | **Gebaut (A189)** mit der Negativliste aus seiner Vorgabe, siehe „V2: Stand der Umsetzung" |
| V3 | Kuratierte Raidwide-, Tankbuster- und Ignorier-Listen je Begegnung (`BattleData`, etwa Blicke, die wie Raidwides aussehen) | Weniger Fehlalarme bei Blickmechaniken, Raidwides ohne Flächen-Wurftyp werden erkannt | angekündigter Cast mit bekannter Wirkung | Datenübernahme aus fremdem Projekt (Lizenzhinweis nötig); Pflegeaufwand |
| V4 | Samurai Meditate erst nach kurzem Stillstand | Weniger abgebrochenes Meditate beim kurzen Anhalten zwischen zwei Bewegungen | — | **Geprüft (A188):** nicht bauen, siehe „V4: Ergebnis" |
| V5 | Tanzpartner neu wählen, wenn der Partner tot ist | Standard Finish und Devilment gehen nicht auf einen Toten | — | **Gebaut (A186)**, siehe „V5: Stand der Umsetzung" |
| V6 | Kerachole nicht über eine liegende Sacred Soil legen (`SGE_OverProtect`) | Gruppenminderung zweier Heiler verteilt sich auf zwei Treffer statt einen | — | **Geprüft (A187):** nicht bauen, siehe „V6: Ergebnis" |

## Seine Angaben zu den Vorschlägen (27.09.2026)

| # | Wortlaut | Einordnung |
|---|---|---|
| V1 | „hatten wir doch schon mit dem hot des whm als standard vorhanden. schild bei schildheilern ist ebenso sinnvoll mit regelmäßiger erneuerung bei ablauf/verfall solange walltowall läuft. gleiche bedingungen wie bei whm hot. gibt es denn geringere instaschilde, die ohne aufwand erneuerbar sind, und die nicht overschilden?" | Hinweis (Weißmagier-Regen, am Code zu prüfen) und Vorgabe (Schildheiler: Schild mit Erneuerung bei Ablauf oder Verbrauch während Wall-to-Wall, Bedingungen wie beim Weißmagier-Regen), dazu eine Frage |
| V2 | „negativliste ingame aufbauen, wenn ein vfx nachträglich als tankbuster falsifiziert wurde. (sicher speichern, nicht das gleiche debakel wie mit schadenstabelle bei aoe)" | Vorgabe |
| V3 | „schauen, ob die infos aus wrath auch ingame verfügbar wären (ohne wrath, nur aus den normal vorhandenen daten)" | Prüfauftrag |
| V4 | „im vollen loop antithese aufstellen und versuchen zu widerlegen. ebenso den zeitraum bewerten (ab wann es sich überhaupt lohnt). dabei notwendige bewegungen für positionals mit berücksichtigen. aber auch true north, um eben die positionswechsel zu reduzieren. oder ist das ein anderes problem (meditate nur ausserhalb kampf?)" | Prüfauftrag mit Frage |
| V5 | „tanzpartner wärend todes aufheben und während des rezzdebuffs neu bewerten, wer am meisten schaden verurschachen würde. wenn rezzdebuff dann nach ablauf weg ist, erneut neu bewerten." | Vorgabe |
| V6 | „im konzept klären: im vollen loop antithese aufstellen und versuchen zu widerlegen." | Prüfauftrag |
| V1 | „beim konzept zum preschild und prehot: der whm kann während des laufens den hot erneuern. das laufen ist also wichtig, ebenso die zeitverlustfreie erneuerung. weiterhin kann der whm während des laufens auch einen dot wirken. wenn man also das auf schildheiler und astro ausweiten will: ist der hot des astros instawirkbar? hat der astro einen insta-schadensspell? haben die schildheiler instaschilde? da während des laufens MP regenerieren, können es sogar vorzugsweise MP-verbrauchende schilde sein, solange sie insta sind. haben die schildheiler insta-schadensspells? stehenbleiben ist erst am ende des wall-to-walls, da wo der whm dann holy casted. wenn die anderen heiler ebenfalls stuns haben, diese so wie beim whm nutzen. das alles ebenfalls im vollständigen kritischen loop bearbeiten." | Hinweis (der Weißmagier erneuert Regen und wirkt einen DoT im Laufen, am Code zu prüfen); Präzisierung seiner V1-Vorgabe: Kriterium ist das Wirken **im Laufen** ohne Zeitverlust, also nur Sofortaktionen; MP-verbrauchende Sofortschilde ausdrücklich zulässig; Stehenbleiben erst am Ende des Wall-to-Wall (wo der Weißmagier Holy wirkt); Betäubungen der anderen Heiler wie beim Weißmagier nutzen; dazu vier Fragen nach Sofort-HoT und Sofort-Schadenszauber (Astrologe) sowie Sofortschild und Sofort-Schadenszauber (Schildheiler); voller kritischer Loop |
| V1 | „beim gelehrten könnte evtl. die fee helfen? deren fähigkeiten mit prüfen im konzept" | Prüfauftrag: die Feen- und Seraph-Fähigkeiten des Gelehrten in das V1-Konzept aufnehmen |
| V1 | „also hat der gelehrte doch über die fee auch einen hot. der wie beim astro oder whm genutzt werden kann. da heiler dem tank folgen, sollten die 30y nicht so schlimm sein. was ist mit dots beim schildheiler? instacastbar? überhaupt vorhanden?" | Vorschlag: Aetherpact/Fey Union als Pre-HoT des Gelehrten wie Regen und Aspected Benefic; seine Einschätzung der 30-y-Grenze ist im Loop zu prüfen (die Fee folgt dem Gelehrten, der Tank läuft im Wall-to-Wall voraus); dazu eine Frage nach DoTs der Schildheiler — Antwort in der Tabelle „V1: Sofortaktionen" |
| V1 | „punkt 2 ist uninteressant, da der heiler dem tank folgt" (28.09.2026, zur 30-y-Grenze von Fey Union) | Hinweis zur Spielweise: Der Heiler folgt dem Tank, die 30-y-Grenze der Fee trägt kein Argument gegen Aetherpact im Pull |

## V1: Sofortaktionen der Heiler im Laufen (Job-Guide, abgerufen 27.09.2026)

Grundlage für das V1-Konzept; Kriterium seiner Präzisierung ist das Wirken im Laufen, also „Instant".

| Heiler | HoT oder Schild sofort | Schaden sofort | Betäubung |
|---|---|---|---|
| Weißmagier | Regen: sofort, 400 MP, 18 s | Dia (DoT), Afflatus Misery, Glare IV | Holy, Holy III: 1,5 s Wirkzeit, Betäubung 4 s |
| Astrologe | Aspected Benefic: sofort, 400 MP, Regen 15 s; Schild nur unter Neutral Sect | Combust III (DoT, 400 MP); Macrocosmos (180 s); Gravity II hat 1,5 s Wirkzeit | keine |
| Weiser | Eukrasia (sofort, Wiederaufnahme 1 s), dann Eukrasian Diagnosis: sofort, 800 MP, Schild 180 % der Heilung, nicht stapelbar mit Galvanize; Haima (oGCD, 120 s) | DoT Eukrasian Dosis I–III und Eukrasian Dyskrasia (Fläche), sofort über Eukrasia, 400 MP; Toxikon II (Addersting), Phlegma III (Ladungen), Dyskrasia II (Fläche) | keine |
| Gelehrter | kein Sofortschild ohne Seraphism: Adloquium hat 2 s Wirkzeit, Manifestation nur unter Seraphism; Excogitation (oGCD, 45 s, greift bei 50 % HP) | Ruin II; DoT Bio, Bio II, Biolysis (alle sofort, 300–400 MP); Art of War II (Fläche) | keine |
| Fee/Seraph (Gelehrter) | Whispering Dawn (oGCD, Gruppen-HoT 21 s, 60 s); Aetherpact/Fey Union (oGCD, HoT auf ein Ziel gegen Feenanzeige, endet, wenn das Ziel mehr als 30 y von der Fee entfernt ist); Embrace automatisch, nur Heilung; Seraph: Seraphic Veil automatisch mit Schild | — | — |

Die Rollenaktionen der Heiler (Repose, Esuna, Lucid Dreaming, Swiftcast, Surecast, Rescue) enthalten keine Betäubung; Repose ist Schlaf mit 2,5 s Wirkzeit. Swiftcast bleibt nach seiner Vorgabe für Wiederbelebungen.

## V1: Stand der Umsetzung (A184)

**Eine Regel für alle Heiler** („Universell zuerst", Stufe „Heiler"): `TryPullUpkeepOnTank` in
`CustomRotation_OtherInfo.cs`. Sie hält eine Sofort-Regen- oder Sofort-Barriereaktion auf dem Tank, solange
er sich einer Gruppe nähert oder sie bindet (`TankApproachingMobGroup`, nur Dungeons), und erneuert sie,
sobald der Status fehlt oder innerhalb der Auffrischfrist endet. Eine verbrauchte Barriere nimmt das Spiel
vom Ziel; dieselbe Prüfung erneuert deshalb bei Ablauf wie bei Verbrauch. Nur die Aktion ist je Job
verschieden. Die Regel gibt nie das MP aus, das die eigene Wiederbelebung braucht (Wirkkosten beider Aktionen
aus den Aktionsdaten) — meine Ableitung aus seiner Vorgabe, Swiftcast für Wiederbelebungen zu halten: Ein
Heiler, der den Toten nicht aufheben kann, hat die Sicherheit der Gruppe für einen Tank ausgegeben, der nicht
in Gefahr war.

| Heiler | Aktion | Stand |
|---|---|---|
| Weißmagier | Regen, auch im Countdown | vorhanden (`UsePreRegen`), jetzt über die gemeinsame Regel |
| Astrologe | Aspected Benefic | vorhanden (`UsePreAspectedBenefic`), jetzt über die gemeinsame Regel |
| Weiser | Eukrasia + Eukrasian Diagnosis, beide sofort | **neu** (`UsePreEukrasianDiagnosis`, ab Werk an wie beim Weißmagier). Rang in der Eukrasia-Wahl: nach Flächen- und Einzelabwehr, vor den DoTs. Nicht gelegt, solange der Tank Galvanize oder Eukrasian Prognosis trägt (laut Wirktext nicht stapelbar). Keine Gesundheitsuntergrenze: Anders als Regen ist Eukrasian Diagnosis selbst der beste Sofort-GCD im Notfall. Außerhalb des Kampfs erst, wenn ein Gegner in Reichweite des Weisen ist — sonst nimmt die bestehende Eukrasia-Bereinigung den Status wieder ab. |
| Gelehrter | kein Sofortschild (Adloquium 2 s Wirkzeit) | nicht gebaut: Stehenbleiben erst am Ende des Wall-to-Wall (seine Präzisierung). Sein Vorschlag Aetherpact/Fey Union als Vorab-HoT bleibt offen; Gegenargumente: Jede andere Feenaktion (Whispering Dawn, Fey Illumination, Fey Blessing) beendet die Verbindung, und auf einem vollen Tank verbraucht Fey Union Feenanzeige ohne Heilung (Nachschub nur über Aetherflow-Aktionen, je +10). Die 30-y-Grenze zählt nicht, der Heiler folgt dem Tank (sein Hinweis). Ob die Fee während Fey Union Embrace aussetzt, ist nicht belegt. Er kollidiert zudem mit zwei bestehenden Einstellungstexten („Remove Aetherpact if … above 90 %", „Do not start Aetherpact if … above 80 %"); zur Entscheidung vorgelegt. |

**Konsequenz, die er kennen muss (Schlussfolgerung, am Spiel nicht gemessen):** Bei Regen fällt die
Erneuerung alle 18 s. Eine Barriere bricht in einem großen Pull, sobald der Tank ihren Wert an Schaden
nimmt. Bricht sie jeden GCD, gehen alle GCDs des Weisen an sie statt an Dosis, und 800 MP je Erneuerung
leeren den Vorrat bis zur Wiederbelebungsreserve. Wie oft sie bricht, hängt vom Pull ab und ist von hier
nicht messbar. Ob „Verfall" in seiner Vorgabe den Verbrauch meint oder nur den Ablauf, entscheidet er
(gebündelte Vorlage).

## V5: Stand der Umsetzung (A186)

Seine Vorgabe: Partnerschaft während des Todes aufheben, während der Schwäche nach einer Wiederbelebung neu
wählen, nach ihrem Ende erneut. Gebaut als `DancerRotation.DancePartnerNeedsChange` (Stufe „Tänzer": nur er hat
einen Partner), gelesen von `DNC_Reborn` vor Closed Position: trifft sie zu, fällt Ending, und Closed Position
wählt über die bestehende Partnerwahl neu.

- **Tod:** Ist der Partner tot oder trägt niemand mehr den Partnerstatus des Tänzers, endet die Partnerschaft
  sofort. Ob das Spiel den Partnerstatus beim Tod entfernt, sagt der Job-Guide nicht; die Regel greift in beiden
  Fällen.
- **Schwäche:** Trägt der Partner Weakness, Brink of Death oder Damage Down — dieselbe Menge, die die
  Partnerwahl ausschließt —, wird gewechselt, sobald ein anderes Gruppenmitglied ohne sie verfügbar ist. Ohne
  Alternative bleibt er Partner: geschwächt teilt er die Buffs noch.
- **Nach der Schwäche:** Steht ein Mitglied mit höherem Rang in der Partnerpriorität verfügbar, wird gewechselt.
  „Wer am meisten Schaden verursacht" ist diese Priorität, dieselbe Ordnung wie bei der ersten Wahl. Ein in den
  Einstellungen namentlich gewählter Partner wird nur für Tod oder Schwäche ersetzt.
- **Zeitpunkt:** Für einen Wechsel erst, wenn Closed Position bereit ist (30 s Abklingzeit, Job-Guide) — sonst
  hätte der Tänzer bis dahin keinen Partner. Nie während eines Tanzes, damit das Finish den Partner noch
  erreicht. Ending hat 1 s Abklingzeit.

**Folge im Kampf:** Nach der Wiederbelebung des Partners gehen Standard Finish und Devilment an ein anderes
Gruppenmitglied, bis die Schwäche endet; danach zurück an den ersten. Ein neuer Partner erhält Standard Finish
erst mit dem nächsten Standard Finish.

## V2: Stand der Umsetzung (A189)

**Erkennung breiter:** Die Tankbuster-Pfade sind Präfixe: `vfx/lockon/eff/tank` (jeder Tank-Marker) und
`vfx/lockon/eff/sharelaser2tank` (jeder geteilte Tank-Laser) statt der vier Einzelpfade. Bei den Sammeltreffern
kommen `share_1` und der Sammelmarker aus San d'Oria: The Second Walk dazu. `target_ae_s5f` aus WrathCombos
Liste bleibt draußen: Dort gilt es nur für ein Gebiet und trifft laut deren Kommentar auch Verteilmarker. Die
Pfade sind Spieldaten (Asset-Namen), kein übernommener Code.

**Negativliste (seine Vorgabe: im Spiel aufbauen, sicher speichern):** `TankbusterMarkerWatch`.
- **Beobachtung:** Jeder erkannte Marker auf einem Gruppenmitglied öffnet eine Beobachtung. Beschädigt eine
  gegnerische Aktion das markierte Mitglied — jede Höhe, auch ein von einer Barriere geschluckter Treffer,
  Auto-Attacken ausgenommen, Quelle anvisierbar oder unsichtbarer Helfer —, ist der Marker bestätigt. Endet die
  Beobachtung ohne solchen Treffer, ist der Pfad widerlegt und kommt auf die Liste. Die Entscheidung
  (`IsCastingTankVfx`, `IsTankbusterVfxOnPlayer`) überspringt Pfade auf der Liste.
- **Selbstkorrektur statt Zählschwelle:** Die Beobachtung läuft für gelistete Pfade weiter; ein späterer Treffer
  nach demselben Marker nimmt den Pfad wieder heraus. Ein falscher Eintrag kostet eine ausgelassene Abwehr beim
  nächsten Auftreten und korrigiert sich dann. Deshalb genügt eine Beobachtung, und es gibt keine feste Zahl.
- **Wann eine Beobachtung endet:** wenn der Marker die VFX-Warteschlange verlassen hat — dieselbe Warteschlange,
  aus der die Entscheidung liest — und jeder gegnerische Zauber, der beim Erscheinen des Markers lief, geendet
  hat, plus ein GCD für das Eintreffen. Beide Fehler neigen zur sicheren Seite: Ein zu langes Fenster lässt
  anderen Schaden den Marker bestätigen, er bleibt Tankbuster wie bisher.
- **Ohne Urteil verworfen:** Das markierte Mitglied ist beim Ende tot oder nicht mehr in der Gruppe; der Kampf
  endet vorher.
- **Speicherung:** `TankbusterMarkerFalsified.json` im Konfigurationsordner, kein Download. Es gelten die Schutzwege
  aller Listen: nie geschrieben, wenn nicht geladen; atomar über eine Temporärdatei; eine unlesbare Datei wird
  beiseitegelegt statt überschrieben. Geändert wird die Liste nur im Spielthread, geschrieben wird eine dort
  gezogene Kopie. Zurücksetzen: Datei löschen („Keine Funktion ohne Bedarf").

**Grenze:** Die Warteschlange hält einen Marker ohne bekannte Dauer fünf Sekunden (bestehender Wert in
`MajorUpdater`). Ein Tankbuster, der später als fünf Sekunden plus ein GCD nach dem Marker fällt und keinen
laufenden Zauber hat, würde fälschlich widerlegt — und beim nächsten Treffer nach demselben Marker korrigiert.

## V4: Ergebnis (A188)

**These (WrathCombo):** Meditate erst nach einigen Sekunden Stillstand, damit es beim kurzen Anhalten zwischen zwei
Bewegungen nicht abbricht. **Ergebnis: nicht bauen;** das Pausenverhalten aus A162 bleibt.

Wirktext (Job-Guide, 28.09.2026): Meditate, 60 s Abklingzeit, „Gradually increases your Kenki Gauge", 15 s, im
Kampf dazu bis zu 3 Stapel Meditation; endet bei jeder anderen Aktion und bei Bewegung, auch beim Drehen; löst
die Abklingzeit der Waffenfertigkeiten aus und ist während ihr nicht nutzbar. Wie viel Kenki je Takt und wie oft,
nennt der Text nicht, und die Spieldaten (Status 1231 „Storing Kenki.") auch nicht — unbelegt.

- **Wo RSR Meditate wirkt:** nur in der Pause (`InCombatPause`: im Kampf, kein Gegner in 25 y) und nicht in
  Bewegung. Deine Frage „oder ist das ein anderes Problem (Meditate nur außerhalb Kampf?)" beantwortet das: Es
  fällt nie, solange ein Gegner in Reichweite steht. **Positionals und True North berühren es deshalb nicht** —
  Stellungswechsel für Positionals gibt es nur mit einem Gegner in Nahkampfreichweite, und dann ist keine Pause.
- **Ab wann es sich lohnt:** ab dem ersten Takt. In der Pause gibt es nichts zu schlagen; die GCD-Sperre, die
  Meditate auslöst, kostet dort nichts. Jeder Takt Kenki und jeder Stapel Meditation (für Shoha) ist Gewinn
  gegenüber null. Eine Wartezeit schöbe diesen Gewinn in jeder Pause um ihre Länge nach hinten und ließe kurze
  Pausen ganz aus — und wäre eine neue feste Zahl ohne Grundlage.
- **Was ein Abbruch kostet:** die 60 s Abklingzeit, also Meditate in einer weiteren Pause innerhalb dieser Zeit;
  endet die Pause genau beim Einsatz, verschiebt die GCD-Sperre die erste Waffenfertigkeit um höchstens einen GCD.
  Beides ist klein gegen den Takt-Gewinn in jeder Pause, in der er stehen bleibt; rechnen lässt es sich ohne die
  Taktwerte nicht.
- **Läuft er im Kampf aus 25 y heraus,** ohne dass eine Pause ist, fällt Meditate beim ersten Stillstand und bricht
  beim Zurücklaufen (Konzept 14, „Folgen, bewusst hingenommen").

## V6: Ergebnis (A187)

**These (WrathCombo):** Der Weise legt Kerachole nicht, solange Sacred Soil eines Gelehrten liegt; die
Gruppenminderung zweier Heiler verteilt sich so auf zwei Treffer. **Ergebnis: nicht bauen.** Das bisherige
Verhalten bleibt — Kerachole fällt, wenn die Abwehr- oder Heilregel es verlangt.

Wirktexte (Job-Guide, 28.09.2026): Kerachole 30 s Abklingzeit, −10 % für den Weisen und alle in 30 y, 15 s, dazu
Regen 100 Potenz und 7 % MP, kostet 1 Addersgall; nicht stapelbar nur mit Taurochole. Sacred Soil 30 s, eine
Fläche mit 15 y Radius, in der Gruppenmitglieder 90 % des Schadens erleiden, 15 s, dazu Regen. Addersgall: ein
Stapel alle 20 s, höchstens 3. Beide sind verschiedene Status und stapeln: zusammen −19 %.

Warum die These nicht trägt:
- **Die Fläche deckt nicht dieselben Mitglieder.** Sacred Soil wirkt nur auf den, der darin steht; Kerachole auf
  alle in 30 y um den Weisen. Eine liegende Fläche heißt nicht, dass die Gruppe gemindert ist.
- **Das Zurückhalten kostet Belegtes.** Bis zu 15 s Warten lässt Addersgall bei vollem Vorrat überlaufen (ein Stapel
  je 20 s) und verschiebt Regen und MP-Rückgabe von Kerachole.
- **Der Nutzen hängt am Verhalten eines anderen Spielers.** Ob der nächste Treffer ohne Minderung käme, hängt davon
  ab, wann der Gelehrte Sacred Soil wieder legt — beide haben 30 s Abklingzeit. Das ist eine Annahme, kein
  tragender Grund.
- **Die eigene Streckung des Weisen** ist auf Stufe „Heiler" als leer begründet (Konzept 08); die These würde sie
  nur für einen fremden Auslöser einführen.

Was die These richtig sieht: Zwei volle Gruppenminderungen auf einen Treffer, der auch mit einer gehalten würde, sind
ein verschenkter zweiter Treffer. Das beantwortet schon die Schranke der Minderung (Konzept 13): Ein als klein
gemessener Treffer löst die Abwehr nicht aus, und seit die Schadenstabelle misst (A185), greift sie.

## Nicht übernommen, mit Grund

- **Heilziel-Wahl nach Rangfolge von Zielarten** (Mouseover, Fokus, niedrigste Gesundheit): RSRs Wahl
  nach Gefährdungsklasse und vorausberechneter Gesundheit (Konzept 07) misst mehr.
- **Staffelung der Raidwide-Antworten nach mittlerer Gruppengesundheit** (1/2/3 Antworten ab 60/30 %):
  feste Schwellen; RSR entscheidet über Gefährdungsklasse und Streckung (Konzept 08).
- **Burst-Erkennung über Zahl der Gruppenbuffs mit Ausnahme bei Schwäche** (`InGoodBurstPhase`):
  ohne belegten Nutzen gegenüber den eigenen Burstfenstern der Jobs.
- **„Protection"-Einstellungen** (Doppeldruck-Schutz für Meikyo, Enshroud usw.): betreffen Knopfdruck
  von Hand; RSR verhindert das über `StatusProvide`.
