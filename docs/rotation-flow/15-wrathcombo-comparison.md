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

## Nicht übernommen, mit Grund

- **Heilziel-Wahl nach Rangfolge von Zielarten** (Mouseover, Fokus, niedrigste Gesundheit): RSRs Wahl
  nach Gefährdungsklasse und vorausberechneter Gesundheit (Konzept 07) misst mehr.
- **Staffelung der Raidwide-Antworten nach mittlerer Gruppengesundheit** (1/2/3 Antworten ab 60/30 %):
  feste Schwellen; RSR entscheidet über Gefährdungsklasse und Streckung (Konzept 08).
- **Burst-Erkennung über Zahl der Gruppenbuffs mit Ausnahme bei Schwäche** (`InGoodBurstPhase`):
  ohne belegten Nutzen gegenüber den eigenen Burstfenstern der Jobs.
- **„Protection"-Einstellungen** (Doppeldruck-Schutz für Meikyo, Enshroud usw.): betreffen Knopfdruck
  von Hand; RSR verhindert das über `StatusProvide`.
