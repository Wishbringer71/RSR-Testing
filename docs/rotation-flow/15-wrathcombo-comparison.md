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
| V2 | Zielmarkierungen: allgemeiner Präfix `vfx/lockon/eff/tank` statt einzelner Tank-Marker, dazu die bei RSR fehlenden Einträge (`sharelaser2tank`, `share_1`, zwei Dungeon-Sammelmarker) | Mehr Tankbuster und Sammeltreffer werden erkannt, bevor sie fallen; Einzel- und Flächenabwehr öffnen öfter rechtzeitig | Marker über dem Ziel — der Treffer ist angekündigt | Daten in Upstream-Code; Gegenrisiko: der Präfix kann andere Marker treffen |
| V3 | Kuratierte Raidwide-, Tankbuster- und Ignorier-Listen je Begegnung (`BattleData`, etwa Blicke, die wie Raidwides aussehen) | Weniger Fehlalarme bei Blickmechaniken, Raidwides ohne Flächen-Wurftyp werden erkannt | angekündigter Cast mit bekannter Wirkung | Datenübernahme aus fremdem Projekt (Lizenzhinweis nötig); Pflegeaufwand |
| V4 | Samurai Meditate erst nach kurzem Stillstand | Weniger abgebrochenes Meditate beim kurzen Anhalten zwischen zwei Bewegungen | — | kleine Option; die Wartezeit wäre eine neue Zahl |
| V5 | Tanzpartner neu wählen, wenn der Partner tot ist | Standard Finish und Devilment gehen nicht auf einen Toten | — | **Zu prüfen:** RSR wählt nur, solange kein Partner besteht; ob der Status beim Tod des Partners endet, sagt der Job-Guide nicht („Effect ends upon reuse") |
| V6 | Kerachole nicht über eine liegende Sacred Soil legen (`SGE_OverProtect`) | Gruppenminderung zweier Heiler verteilt sich auf zwei Treffer statt einen | — | Eingabe für die offene Frage „Streckung auch für Weiser/Gelehrter" (Konzept 08); laut Job-Guide stapeln beide (nur Kerachole/Taurochole nicht) |

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

## V1: Sofortaktionen der Heiler im Laufen (Job-Guide, abgerufen 27.09.2026)

Grundlage für das V1-Konzept; Kriterium seiner Präzisierung ist das Wirken im Laufen, also „Instant".

| Heiler | HoT oder Schild sofort | Schaden sofort | Betäubung |
|---|---|---|---|
| Weißmagier | Regen: sofort, 400 MP, 18 s | Dia (DoT), Afflatus Misery, Glare IV | Holy, Holy III: 1,5 s Wirkzeit, Betäubung 4 s |
| Astrologe | Aspected Benefic: sofort, 400 MP, Regen 15 s; Schild nur unter Neutral Sect | Combust III (DoT, 400 MP); Macrocosmos (180 s); Gravity II hat 1,5 s Wirkzeit | keine |
| Weiser | Eukrasia (sofort, Wiederaufnahme 1 s), dann Eukrasian Diagnosis: sofort, 800 MP, Schild 180 % der Heilung, nicht stapelbar mit Galvanize; Haima (oGCD, 120 s) | Eukrasian Dosis III (DoT, über Eukrasia), Toxikon II (Addersting), Phlegma III (Ladungen), Dyskrasia II (Fläche) | keine |
| Gelehrter | kein Sofortschild ohne Seraphism: Adloquium hat 2 s Wirkzeit, Manifestation nur unter Seraphism; Excogitation (oGCD, 45 s, greift bei 50 % HP) | Ruin II, Biolysis (DoT), Art of War II (Fläche) | keine |
| Fee/Seraph (Gelehrter) | Whispering Dawn (oGCD, Gruppen-HoT 21 s, 60 s); Aetherpact/Fey Union (oGCD, HoT auf ein Ziel gegen Feenanzeige, endet, wenn das Ziel mehr als 30 y von der Fee entfernt ist); Embrace automatisch, nur Heilung; Seraph: Seraphic Veil automatisch mit Schild | — | — |

Die Rollenaktionen der Heiler (Repose, Esuna, Lucid Dreaming, Swiftcast, Surecast, Rescue) enthalten keine Betäubung; Repose ist Schlaf mit 2,5 s Wirkzeit. Swiftcast bleibt nach seiner Vorgabe für Wiederbelebungen.

## Nicht übernommen, mit Grund

- **Heilziel-Wahl nach Rangfolge von Zielarten** (Mouseover, Fokus, niedrigste Gesundheit): RSRs Wahl
  nach Gefährdungsklasse und vorausberechneter Gesundheit (Konzept 07) misst mehr.
- **Staffelung der Raidwide-Antworten nach mittlerer Gruppengesundheit** (1/2/3 Antworten ab 60/30 %):
  feste Schwellen; RSR entscheidet über Gefährdungsklasse und Streckung (Konzept 08).
- **Burst-Erkennung über Zahl der Gruppenbuffs mit Ausnahme bei Schwäche** (`InGoodBurstPhase`):
  ohne belegten Nutzen gegenüber den eigenen Burstfenstern der Jobs.
- **„Protection"-Einstellungen** (Doppeldruck-Schutz für Meikyo, Enshroud usw.): betreffen Knopfdruck
  von Hand; RSR verhindert das über `StatusProvide`.
