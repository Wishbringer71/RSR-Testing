# TODO — nur offene Arbeit

Getrennt nach Defekt (Abweichung vom beabsichtigten Verhalten), technischer Schuld (bewusst eingegangener Kompromiss mit Auflösungsbedingung) und offener Arbeit. Je Eintrag der betroffene Personenkreis: **N** Endnutzer des Plugins · **R** Autoren abgeleiteter Rotationen, die gegen `RotationSolver.Basic` kompilieren (bis 7.5.6.13 als Paket, seither als DLL aus `latest.zip`) · **U** Upstream-Pflege.

## Reihenfolge

Nach Wichtigkeit, also nach der Wirkung im Kampf: Sicherheit der Gruppe vor Schaden, gewichtet mit der
Wahrscheinlichkeit, in seinem Profil (seine Vorgabe, 17.10.2026: „todo nach wichtigkeit abarbeiten. jeweils im loop
bearbeiten"). Stand 17.10.2026; fortgeschrieben mit jedem Befund.

1. Tanks: Hilfe für den Co-Tank vor seinem Tankbuster (mit dem Defekt der Revolverklinge); Rückstoß und Unterbrechung messen; DRK: Bruch von The Blackest Night; Rotationen gegen The Balance.
2. Offene Arbeit, dann technische Schuld, soweit ihre Auflösungsbedingung erfüllt ist.

Wartet auf seine Entscheidung (Loop durch, Vorlage im Eintrag): Tankwechsel von der anderen Seite.

Warten auf Daten, nicht bearbeitbar: Aktualisierungskosten je Teilschritt (Protokoll „update cost by step“, A310); Tankbuster nach einem Marker ohne Wirken (Protokoll „no cast taken for it“, A308); Treffer aus einer Quelle, die die Objekttabelle nicht hält (Protokoll); Samurai
Feint und Tengentsu (Protokoll eines Samurai); BossModReborn-FATE-Modul in Occult Crescent (Adressat BossModReborn).

## Defekte

### BossModReborn: Ein FATE-Modul in Occult Crescent läuft nach seinem Weggang weiter · N

Am Quelltext gefolgert (BossModReborn be8ff77, 13.10.2026), im Spiel nicht beobachtet. `OpenWorldFate` will sich
zurücksetzen, sobald der Spieler 40 y von der Arenamitte weg ist (`CheckReset`). Die Mitte setzt aber
`SimpleBossModule.UpdateModule` in jedem Durchlauf auf die Position des Spielers, also greift die Entfernung nie.
In Inhalten mit Inhaltsnummer, Occult Crescent eingeschlossen, lädt `BossModuleManager` ein aktives Modul auch
nicht nach Entfernung ab (seit be21ea6a2, 01.10.2026). Das Modul eines FATE, an dem er teilnahm, bleibt deshalb aktiv,
bis dessen Boss den Kampf verlässt. Ein später aktives Modul löst es nicht ab, weil die Ablösung eine höhere
Priorität verlangt und beide aktiv sind. Im Kampf: RSR liest die Vorhersagen des verlassenen FATE (ein Raidwide-Cast
nennt jedes Gruppenmitglied als getroffen), und für das Critical Engagement, in das er weitergeht, kommt keine. Über
die Schnittstelle nicht erkennbar: Die Arena des FATE-Moduls wandert mit dem Spieler, und BossModReborn nennt den
Namen des Bosses, nicht die Figur. Adressat ist BossModReborn.

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`, „Wann eine BossModReborn-Vorhersage ihn betrifft"

### Samurai nutzt Feint und Tengentsu im Kampf nicht (fremde Meldung) · N

**Konzept:** `docs/rotation-flow/13-aoe-damage-classification.md`
Gemeldet unter PR #11 (05.10.2026, ein Nutzer des Releases 7.5.6.13+wsh1, von ihm beantwortet): Seit 7.5.6.13+wsh1
setze der Samurai im Kampf weder Feint noch Tengentsu ein. Unbestätigt; kein Protokoll. *Statisch geprüft (A265):*
Zwischen 7.5.6.10+wsh1 und 7.5.6.13+wsh1 kam für den Samurai genau ein neues Gatter dazu: Tengentsu und Third Eye
im Flächenpfad nur, wenn der Treffer ihn selbst erreicht (A218, beabsichtigt; seit A233 zentral geprüft, seit A271
nicht mehr zusätzlich in der Rotation). Feint hat im Flächen-
und Einzelpfad kein neues Gatter; die Flächenflagge wurde im selben Fenster erweitert (Gruppe statt Spieler, A218),
nicht verengt. Die Hypothese, die Einzelziel-Regel (eb3fce2eb) nehme Raidwides die Flagge, ist an den Spieldaten
widerlegt: Von 850 Listeneinträgen ist einer ein Einzelziel mit Wirkreichweite (Holy Bladedance, ein Tankbuster).
Unverändert seit früher: beide nur ohne Zanshin Ready (`!HasZanshinReady`, Upstream), und die Bewertung „lohnt
Minderung" (`Skip mitigation for small area casts`). *Messmittel:* `DefenseTrace.log` schreibt jetzt auch „area
defence open, nothing chosen" mit den Quellen; zusammen mit „hit you" trennt eine Aufzeichnung eines Samurai-Kampfs
„Flagge nie offen" von „offen, alles abgelehnt". Auflösung: ein solches Protokoll.

### Tanks: Hilfe für den Co-Tank vor seinem Tankbuster (Nascent Flash, Intervention, The Blackest Night, Herz des Korunds) · N, U

**Stand (17.10.2026, Loop begonnen):** The Balance setzt Nascent Flash „most often on the co-tank" (Warrior Basic Guide,
02.10.2026); dieselbe Lage haben alle vier Tanks mit ihrer Hilfe für andere (Konzept 09, „Die Abwehr der vier Tanks",
Rolle „Hilfe für andere") – nach „Universell zuerst" eine Regel auf Stufe „Tanks", je Job nur die Aktion. Gebaut ist sie
nirgends; nur die Revolverklinge versucht es über die Zielart `Tankbuster`.

**Defekt, gefunden in diesem Loop (Text bindet):** `DataCenter.IsCastingTankVfx` füllt `TankbusterTargets` für einen
Tank nur mit Markern auf ihm selbst (`if (!isTank || isPlayerTarget)`, upstream `cbcc9d537`). Die Einstellung der
Revolverklinge „Only use on tankbuster targets prioritizing self" (Herz des Korunds, Aurora; ab Werk gewählt) erreicht
den Co-Tank deshalb nie. Die Bedingung vermengt zwei Ausgaben: ob ein Marker den Spieler betrifft (für ihn richtig) und
wen ein Tankbuster trifft (dort falsch). Gezielte Tankbuster-Zauber auf andere Mitglieder stehen gar nicht in der Liste.

**Bausteine für die Regel:**
- *Erkennung (Stufe „alle"):* Marker auf einem anderen Tank der eigenen Gruppe (ohne Zeitangabe, A308); gezielter
  gelisteter oder gemessener Tankbuster auf ihn (Restzeit des Wirkens); BossModReborn nur, wenn die Maske ihn benennt –
  belegt ist nur Bit 0 = Spieler, die Zuordnung der übrigen Plätze zu Gruppenmitgliedern nicht.
- *Entscheidung (Stufe „Tanks"):* Hilfe auf ihn, solange der Druck vor dem Snapshot ankommt (A319), er nicht über den
  Treffer unverwundbar ist und die Hilfe nicht schon trägt; nicht, wenn der Spieler sie selbst braucht (Krieger:
  bestehende Triage `NeedsBloodwhettingHimself`; die anderen: eigener Tankbuster vor Ablauf der Abklingzeit oder
  kritische Klasse). Zielwahl ohne neue `TargetType` prüfen (Vertrag: wird die Zielart gespeichert?).
- *Upstream-Bauform:* Raw Intuition/Bloodwhetting fällt nur, solange das Ziel des Kriegers ihn anvisiert
  (`PlayerIsTargetOnSelf`). Ein Off-Tank mit Tankbuster-Marker bekommt es aus der Einzelabwehr nicht.

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md` („Krieger: Urimpuls für sich oder Urflackern für andere",
„Die Abwehr der vier Tanks")

### Rotationen gegen The Balance abgleichen · N

Seit 27.09.2026 ist die Referenz erreichbar. Offen: je Job die Standardrotation (Opener, Burstausrichtung, Prioritäten) gegen den Guide prüfen — „werden die optimalen Kombinationen genutzt" über die Fensterprüfung hinaus (Konzept 14). Dazu: die von `ActionId.resx` ausgeblendeten Werte (Dauern, Potenzen, Statusnamen) aus dem Job-Guide in den Generator übernehmen, und die deutschen Namen aus dem deutschen Job-Guide in `action_names_de.json`.

### Tank-Unverwundbarkeit vor Tankbustern ohne Aktionsangabe oder ohne Messung weiter nur reaktiv · N

**Stand (A260, A308):** Vor einem gewirkten Tankbuster, den die Tankbuster-Tabelle selbst bei vollen LP und mit allen
eigenen Minderungen für tödlich hält, geht die Unverwundbarkeit vorher aus (Konzept 09, „Gemessener Tankbuster: das
geringste Mittel"). Seit A308 zählt dazu auch das Wirken einer gemessenen Aktion ohne Ziel, während ein Tankbuster-Marker
auf dir steht. Weiter nur reaktiv, also erst unter `HealthForDyingTanks`, bleibt sie
- bei einem Marker ohne Wirken einer gemessenen Aktion (der Treffer kommt ohne Wirkbalken, oder das Wirken beginnt erst,
  nachdem der Marker die Warteschlange verlassen hat);
- bei Tankbustern, die nur BossModReborn ankündigt: Die Vorhersage nennt die Aktion nicht;
- beim ersten Auftreten einer Aktion, solange kein Spieler davon getroffen wurde.

**Offen:** Wie oft die Zuordnung greift, zeigt das Protokoll („tankbuster coming at you (by the marker on you)"). Für
einen Marker ohne Wirken bräuchte es eine gelernte Zeit vom Marker bis zum Treffer je Marker und Ort; ob eine solche
Frist „ohne Risiko" trägt, ist ungeprüft – voller Loop vor jedem Bau. Ebenfalls offen: Stärkungen des Gegners, die nicht
als Damage Up benannt sind, werden beim Messen nicht erkannt; wie oft es sie gibt, ist nicht erhoben. Umgekehrt bleibt
ein Gegner, der den ganzen Kampf ein Damage Up trägt, ohne Messung, und eine Vorhersage unter Damage Up unterschätzt
(sichere Richtung; die Wirktexte nennen keine Stärke, mit der sich hochrechnen ließe).

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Tankwechsel von der anderen Seite: Herausforderung, wenn der Haupttank nach einem Tankbuster in Gefahr ist · N · zur Entscheidung (Vorlage)

**Kontext.** Hält der Co-Tank den Gegner und lässt ihn ein Tankbuster mit einer Verwundbarkeit zurück oder so tief, dass
eine Wiederholung ihn tötet, ohne dass er unter `HealthForDyingTanks` fällt, tut RSR auf deiner Seite nichts. Seine
eigene Hälfte des Wechsels (Geteiltes Leid auf dich) drückt er selbst; spielt RSR für dich den zweiten Tank, fehlt bei
einer Tankwechsel-Mechanik die Herausforderung, und der nächste Treffer trifft ihn. Meine Ableitung, nicht seine Vorgabe
(A255, A256). Seit A318 gilt: Unter `HealthForDyingTanks` provoziert RSR unter eigener Option („Provoke the enemy off a
co-tank at the invulnerability HP", ab Werk an).

**Betroffene Stellen.** `ObjectHelper.CanProvoke` (Rettungszweig), `TankSwapWatch` (Gefahr eines anderen Tanks: dieselbe
Messung, die heute prüft, ob der Empfänger deines Geteilten Leids selbst in Gefahr ist – Verwundbarkeit aus dem
Tankbuster oder LP mit Schild unter dem erwarteten Anteil einer Wiederholung), die Option aus A318.

**Mechanik (belegt).** Herausforderung: „placing yourself at the top of a target's enmity list while gaining additional
enmity" (`ActionId.resx`). *Zu spät ist sie ab dem Wirkbeginn oder Marker des nächsten Tankbusters* (seine Frage vom
17.10.2026, geprüft; Konzept 09, „Wann eine Herausforderung den Treffer noch umlenkt"): Das gezielte Wirken trifft das
Ziel vom Wirkbeginn, der Marker das markierte Mitglied (BossModReborn `be8ff77`; seine Angabe B2b, 15.08.2026). Offen
bleiben nur das Wirken auf den Wirker selbst (BMR nimmt das aktuelle Ziel an, unbelegt) und die weiteren Einschläge eines
mehrfachen Tankbusters. Die Herausforderung muss also vor dem nächsten Wirkbeginn liegen – in der Praxis gleich nach dem
Treffer, der den Co-Tank in Gefahr bringt. Haltung: „Significantly increases enmity generation"; RSR schaltet sie nur
ein, wenn kein anderer Tank sie trägt. Ob deine an ist, liest die Regel (`HasTankStance`); die Vorlage hatte bis zu seiner
Frage „wer sagt, dass meine haltung aus ist?" ungekennzeichnet angenommen, sie sei aus. Laut Icy Veins (nur als
Suchauszug gelesen, die Seite weist Skripte ab) gehört die Haltung vor die Herausforderung, weil deren Zusatz an
Feindseligkeit von ihr abhängt.

**Sein Geteiltes Leid auf dich (sein Hinweis, 17.10.2026).** Der Co-Tank kann seine Hälfte zuerst drücken: Geteiltes Leid
gibt dir 25 % seiner Feindseligkeit. Danach greift der Gegner dich an, wenn du vorher über der Hälfte seiner
Feindseligkeit lagst (dieselbe Rechnung wie bei deinem Geteilten Leid, Konzept 09); sonst bleibt er bei ihm.
- *Er bewegt den Gegner selbst:* Der Gegner greift dich an; jede Variante unten provoziert nur einen Gegner, der ihn
  angreift, wirkt dann also nicht. Kein doppelter Einsatz. Den nächsten Tankbuster nimmst du, dein Tankbuster-Plan
  rechnet ihn.
- *Er bewegt ihn nicht* (du liegst unter der Hälfte seiner Feindseligkeit – etwa ohne Haltung; mit Haltung ist das
  weniger wahrscheinlich, aber nicht ausgeschlossen): Der Gegner bleibt bei ihm, obwohl er ihn abgeben will. Heute schreibt RSR nur „… shirked to you" ins Protokoll und tut nichts; erst deine Herausforderung
  vollendet den Wechsel. Seine Geste ist das stärkste Zeichen, das es für einen gewollten Wechsel gibt – stärker als
  Verwundbarkeit oder Ankündigung, denn es ist seine Entscheidung.
- *Ob die Geste bei dir ankommt,* ist nicht beobachtet: Der Effektsatz von Geteiltem Leid mit dir als Ziel wird gelesen
  (`Watcher`), in deinen sechs Protokollen steht keine solche Zeile; ob dort ein Co-Tank Geteiltes Leid auf dich
  wirkte, zeigen sie nicht.
- *Ihr seid beide in Gefahr:* keiner gibt ab, keiner provoziert – wie auf deiner Seite.

**Optionen und Konsequenzen über den Kampf.**
- *0 – Nullvariante (wie jetzt):* Rettung erst unter `HealthForDyingTanks`. Bei einer Tankwechsel-Mechanik musst du
  selbst provozieren; tust du es nicht, nimmt der Co-Tank den zweiten Treffer mit Verwundbarkeit und stirbt, wenn seine
  Unverwundbarkeit nicht bereit ist.
- *A – Herausforderung bei Gefahr allein:* Jede Verwundbarkeit aus einem Tankbuster löst sie aus. Wahrscheinlichkeit
  gemessen nur an der Verwundbarkeit selbst: Sie sagt, dass ein zweiter Treffer tödlich wäre, nicht, ob er kommt. In
  Kämpfen ohne Wechselmechanik läuft sie meist vor dem nächsten Tankbuster ab. Folge je Auslösung ohne zweiten Treffer:
  der Gegner dreht sich (eine Frontalattacke streicht dabei über die Gruppe), der Co-Tank verliert ihn, du hältst ihn
  ohne Haltung nicht, und Herausforderung fehlt 30 s für einen Gegner auf einem Heiler.
- *B – Herausforderung bei Gefahr, wenn ein nächster Tankbuster desselben Gegners vor dem Ende der Verwundbarkeit
  angekündigt ist:* gleich nach dem Treffer, der ihn in Gefahr bringt, nicht erst zum Wirken – das Wirken oder der
  Marker des nächsten ist zu spät (oben). Wahrscheinlichkeit gemessen an der Ankündigung, die vor dem Wirken liegt:
  BossModReborns Zeitleiste (`Timeline.NextTankbusterIn`, Übergang des markierten Zustands). Ohne BMR-Modul gibt es
  diese Ankündigung nicht; dann bliebe nur das Wirken auf den Wirker selbst (Umlenkbarkeit unbelegt) – B ist ohne Modul
  praktisch die Nullvariante. Nicht bei Unverwundbarkeit des Co-Tanks (`451d9e90`) und nicht, solange du selbst in
  Gefahr bist.
- *C – wie B, zusätzlich mit Haltung:* Ist deine Haltung aus, schaltet RSR sie vor der Herausforderung ein (Icy Veins,
  oben); ist sie an, ändert C nichts. Zwei Haltungen gleichzeitig, bis der Co-Tank seine ablegt.
- *D – wie B, zusätzlich auf seine Geste:* Hat der Co-Tank Geteiltes Leid auf dich gewirkt und greift der Gegner ihn
  weiter an, provoziert RSR sofort, ohne auf eine Ankündigung zu warten. Die Geste gilt, bis der nächste Tankbuster
  dieses Gegners jemanden trifft oder der Gegner dich angreift (Ereignisse, keine Frist). Nicht, solange du selbst in
  Gefahr bist, und nicht bei seiner Unverwundbarkeit. Kommt die Geste nicht an, ist D gleich B. Seine Geste ist das
  Zeichen, dass er den Treffer nicht nehmen will (sein Hinweis); sie trägt auch ohne BMR-Modul. Liegt sie erst nach dem
  Wirkbeginn des nächsten Tankbusters, trifft dieser ihn trotzdem – die Herausforderung zählt dann für die Treffer danach.
- *Rückbau:* jede Variante hinter der Option aus A318 (erweiterter Text) oder einer eigenen; aus = Nullvariante.

**Gegenproben gegen meine Empfehlung (B).**
1. *Der Co-Tank plant für den zweiten Treffer seine Unverwundbarkeit* (Holmgang u. a.). RSR kann fremde Abklingzeiten
   nicht lesen und nähme ihm den Gegner; du stündest im Treffer ohne Vorbereitung – dein Tankbuster-Plan sieht ihn erst,
   wenn das Wirken auf dich zeigt. Hält: Das ist der stärkste Einwand, und er lässt sich am Code nicht ausräumen.
   Deshalb nur mit angekündigtem Treffer und als Option, nicht als feste Regel.
2. *BossModReborn ist keine verlässliche Quelle* (kein Modul und keine Ankündigung sehen gleich aus). Folge: B tut dann
   nichts – gleich der Nullvariante, kein Schaden.
3. *Er gibt den Gegner selbst ab* (Geteiltes Leid auf dich, ob Mensch oder RSR). Bewegt es den Gegner, wirkt B nicht
   (oben). Bewegt es ihn nicht, wartet B auf eine Ankündigung, obwohl er den Wechsel schon will – deshalb D. Gegen D
   hält Einwand 1 nicht: Wer abgibt, plant den zweiten Treffer nicht selbst.
4. *Mehrere Gegner greifen ihn an:* BossModReborn nennt den Wirker eines Tankbusters nicht. B provoziert dann nur den
   Gegner, dessen Wirken sichtbar ist; D den Gegner, den er angreift, wenn es genau einer ist. Grenze.
5. *Tankbuster auf die ersten beiden der Feindseligkeit:* Ein Wechsel ändert dort nichts; er nimmt den Treffer weiter
   als Zweiter. Kein Einwand gegen B oder D – sie kosten dann eine Herausforderung, nicht mehr.

**Empfehlung:** B mit C und D zusammen – Herausforderung gleich nach dem Treffer, wenn BossModReborn den nächsten vor
Ende der Verwundbarkeit ankündigt, oder sofort auf seine Geste; die Haltung vorher ein, falls sie aus ist. Hinter der
Option aus A318, deren Text dann alle Fälle nennt; ab Werk an (Sicherheit vor Schaden, gewichtet mit Ankündigung oder
seiner Entscheidung). Seine Einschätzung (17.10.2026): „b, c die wahrscheinlichste option", die Geste als Hinweis.

**Seine Entscheidung:** welche Kombination aus B, C, D (oder 0/A), und ob ab Werk an.

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`

### Dieselbe Frage steht bei Tankbustern, Rückstoß und Unterbrechung offen · N

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`, `docs/rotation-flow/13-aoe-damage-classification.md`
Für Tankbuster ist sie beantwortet: Die Tankbuster-Tabelle misst jeden Treffer ungemindert auf jedem Spieler und trägt den Tankbuster-Plan (A259, Konzept 09). Offen bleiben `HostileCastingKnockback` und `HostileCastingStop` mit derselben Struktur: Der Messpfad im Effekt-Handler ist derselbe; was fehlt, ist je Liste ein eigener Speicher und die passende Rechnung, und je Liste die Frage, was die Größe dort entscheidet (ein Rückstoß schadet oft nicht, er verschiebt).

### DRK: Ob The Blackest Night auf dem Pull bricht, misst niemand · N, U

`BlackestNightUsage` (`DRK_Reborn.cs`) bietet drei Stufen für den Selbstschutz-Zweig: wie bisher; Tankbuster oder großer Pull ohne laufende große Minderung; dasselbe plus Gesundheitsschwelle. Voreingestellt ist seit A227 (29.09.2026) **die dritte** (`TankbusterHeavyPullOrLowHealth`). Zwei Größen der engeren Stufen sind Annahmen: dass vier Gegner die Verbrauchsrate von 3,6 % der maximalen Gesundheit pro Sekunde erreichen, und dass ein Tankbuster die unter Minderung angehobene Schwelle mitnimmt.

**Umgesetzt nach Vorgabe des Auftraggebers, Wall-to-Wall.** Steht The Blackest Night, wirkt der Dunkelritter auf einem Gruppenpull weder Reflexion noch Abtausch, und der Weißmagier betäubt nicht mit Sanctus; im Bosskampf gilt die Regel nicht. `HoldMitigationForBarrier()` sperrt die beiden Reflexion-Zweige und den Abtausch-Zweig auf dem Pull, `ShouldHoldHolyForBarrier()` die Betäubungsseite — jetzt **voreingestellt an** und ebenfalls auf den Gruppenpull begrenzt. Begründung und Abgrenzung stehen in `docs/rotation-flow/10-drk-blackest-night.md`. Prüfgrad: statisch und Compile.

**Die Verlangsamung durch Dritte ist messbar und praktisch vernachlässigbar.** Die Behauptung, sie sei von hier aus nicht zu erfassen, war falsch: Der Debuff steht auf den **Gegnern**, und `PackSlowed` misst ihn bereits genau so — `SurveyHostileStatus(JobRange, SlowStatus, out slowed)`, ohne zu fragen, wer ihn gelegt hat. Ein fremder Abtausch ist damit sichtbar wie der eigene.

Er fällt trotzdem kaum ins Gewicht, und der Grund liegt in der Wirkweise der Aktion: Abtausch verlangsamt nur Gegner, die den **Träger** treffen. Der Tank zieht die Aggro, also schlagen die Gegner ihn und nicht den Schadensausteiler; dessen Abtausch müsste erst Aggro und Treffer abbekommen, bevor überhaupt eine Verlangsamung entsteht. Die Wirkrichtung ist außerdem einseitig: Ein fremder Slow kann das **Zünden** der Barriere verzögern, weil `PackSlowed` es sperrt — die stehende Barriere hat umgekehrt keinen Einfluss darauf, ob der andere Spieler seine Aktion wirkt.

**Empfehlung: nicht bearbeiten.** Eine eigene Regel dafür hätte keinen belegbaren Anwendungsfall.

**Was offen bleibt, ist die Messung.** Ob die Barriere unter den engeren Stufen tatsächlich bricht, beobachtet niemand, und eine Spielbeobachtung ist nicht seine Aufgabe (CLAUDE.md, „Entscheidung zur Laufzeit"): Die Regel müsste ihren Bruch selbst zählen (Dark Arts nach der Barriere) und die Gegnerschwelle nachsteuern. Voller Loop vor dem Bau. Dieselbe Zählung beantwortet zwei Nebenfragen: was die Rückhaltung des Weißmagiers kostet (Sanctus ist sein einziger Flächenzauber, ein zurückgehaltener GCD fällt auf Einzelzielschaden zurück) und ob `BlackestNightMinHostiles` (Vorgabe 4) zu hoch liegt, sodass The Blackest Night im Wall-to-Wall zu selten kommt. Sie ist bewusst eine **eigene** Option der Rotation und nicht der globale `MitigationSustainHostileCount`, weil die Frage hier eine andere ist: nicht „lohnt eine Minderung", sondern „reicht der Schadensstrom, um 25 % der maximalen Gesundheit in sieben Sekunden aufzuzehren".

### `DPSTaken` zählt Treffer der letzten 5 ms statt 5 s · N, R, U

`DataCenter.DPSTaken` nimmt nur Einträge, die jünger als `TimeSpan.FromMilliseconds(5)` sind, und rechnet die Zeitspanne
mit 2,5 ms Zuschlag; gemeint sind nach Form und Name Sekunden. Das Ergebnis ist fast immer 0. Gelesen wird es nur von
der Debug-Anzeige (`MainWindow_Debug`), aber es ist öffentlich und damit für abgeleitete Rotationen lesbar. Im Kampf:
keine Wirkung, solange niemand es liest. Gefunden bei der Suche nach einem Maß für den Schadenseingang des Tanks (A304).
Herkunft: Upstream, seit 3d1f2ccb2 (15.04.2023, „add a api for DPSTaken"), dort unverändert. Offen: ob ein Leser es
braucht; dann Einheit richten (Adressat auch Upstream).

### Treffer aus einer Quelle, die die Objekttabelle nicht hält, gehen ganz verloren · N (Verdacht)

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`, „Wann eine BossModReborn-Vorhersage ihn betrifft"
ECommons löst die Quelle eines Effektsatzes über die Objekttabelle auf (`Svc.Objects.SearchById`); fehlt sie dort,
ist sie leer, und `Watcher.ActionFromEnemy` verwirft den Satz ganz: keine Zeile „hit you", keine Schadensaufnahme,
keine Messung der Flächentabelle. Dasselbe gilt für eine Figur, die weder Gegner noch Kämpfer ist.

**Im Kampf, falls es vorkommt:** Ein Raidwide aus einer Hilfsfigur zählt für RSR nicht als Treffer; die Vorhersage
der Gesundheit fehlt dieser Schaden, und die Flächentabelle bewertet die Aktion nie.

**Vorkommen nicht belegt.** Zweimal wurden Raidwides vorhergesagt, deren Schaden Hilfsfiguren bringen, und im
Protokoll stand kein Treffer auf ihn: Necrophobia (10.10.2026) und Düsterflare im Critical Engagement Dark Artistry
(17.10.2026, Occult Crescent). Ob er nicht getroffen wurde oder die Quelle fehlte, ist offen. Seit A299 schreibt das
Protokoll jeden solchen Treffer („hit you from a source the object table does not hold" bzw. „not read as an enemy").
Auflösung: ein Protokoll aus einem solchen Kampf; zeigt es die Zeilen, folgt der Loop, wie ein Satz ohne Quelle
verarbeitet wird.

## Technische Schuld

Bewusst eingegangene Kompromisse und erfasste Befunde ohne Wirkung im Kampf, je mit Kosten und Auflösungsbedingung.

### Searing-Light-Abdeckungsmodell bildet die Phasenbuchung des Plugins von Hand nach · R

**Konzept:** `docs/rotation-flow/12-searing-light-stacking.md`
Der Modus „plugin" in `searing_light_coverage.py` rechnet die Phasenbuchung von `SummonerRotation.UpdateSearingPhaseBook` nach (gehalten = zwei bewertete Fenster in Folge besetzt, eigener Buff bewertet nicht) — am 14.10.2026 gleich (A286, K5, P1). Die CI hält die Invarianten des Modells, nicht seine Übereinstimmung mit dem Code.

**Kompromiss:** Eine Verknüpfung über Textmuster im C#-Code prüfte Schreibweisen, nicht Verhalten (ein Surrogat); das Modell aus dem Code zu erzeugen, hieße einen C#-Interpreter für einen Teil der Rotation zu bauen.
**Kosten:** Ändert sich die Buchung im Code, liefert das Modell weiter Zahlen für die alte Regel, ohne dass etwas fehlschlägt — und Konzept 12 argumentiert mit ihnen.
**Auflösungsbedingung:** jede Änderung an `UpdateSearingPhaseBook`, `AllSearingPhasesHeld` oder `BurstPhaseHeldByAnother` trägt im selben Commit die Anpassung des Modus „plugin"; die Prüfung dafür ist der Diff dieser drei Stellen beim Review.

### `SwiftcastBuffer` hat keinen Leser, und ihre Absicht ist überholt · N

`Configs.cs`, `SwiftcastBuffer`, definiert die Einstellung (0,6 s, eigene Oberfläche, eigene Dokumentation „how early before next GCD should RSR use swiftcast for raise"). Eine Volltextsuche über den Baum findet genau diese eine Fundstelle: Sie wird nirgends gelesen.

Sie ist nicht nur unverbunden, sondern in ihrer dokumentierten Bedeutung unerfüllbar geworden. Sie besagt, Spontanität solle erst fallen, wenn nur noch `SwiftcastBuffer` Restzeit auf dem GCD liegt — bei 0,6 s liegt dieses Fenster fast vollständig, bei 0 vollständig in dem Bereich, den `RSCommands_Actions.DoAction` für Fähigkeiten sperrt. Sie zu verdrahten hieße, den in `docs/rotation-flow/11-raise-dispatch.md` behobenen Defekt an einer zweiten Stelle neu zu bauen; deshalb ist sie bei der dortigen Behebung bewusst unangetastet geblieben.

**Auflösung:** entweder entfernen — dann ist zu prüfen, ob der Name in gespeicherter Nutzerkonfiguration liegt und ein Migrationspfad nötig ist — oder als **Untergrenze** im Einschiebefenster neu definieren, also „wie weit oberhalb der Sperre darf Spontanität frühestens fallen". Die zweite Lesart erhält die Absicht des Autors und ist mit der Sperre vereinbar. Beides ist eine Entscheidung über Nutzerkonfiguration und gehört nicht in den Behebungsvorgang.

### `InterruptDelay` und `ProvokeDelay` haben keinen Leser · N

`Configs.cs`, `InterruptDelay` und `ProvokeDelay`. Beide sind `Vector2` mit Vorgabe `(0,5 s; 1 s)`, eigener Beschriftung und Wertebereich — und versprechen damit eine Zufallsverzögerung vor dem Unterbrechen beziehungsweise vor Provoke. Gelesen werden sie nirgends.

Die Klasse ist belegt, weil die beiden Geschwister derselben Bauart **gelesen** werden: `RaiseDelay2` und `EsunaDelay` speisen die `ObjectListDelay`-Instanzen in `TargetUpdater` (`_raisePartyTargets` und Geschwister). Für Provoke- und Unterbrechungsziele gibt es keine solche Instanz; `TargetUpdater` setzt `DataCenter.ProvokeTarget` und `DataCenter.InterruptTarget` ohne jede Verzögerung.

**Nicht verdrahtet, und das ist keine offene Entscheidung, sondern das Ergebnis der Regeln.** Drei Gründe, die zusammen nur einen Schluss zulassen:

1. *Es gibt keinen Migrationsweg.* `Configs.Migrate` ist keine Migration, sondern ein Zurücksetzen: bei abweichender Version `return new Configs()`. Eine geänderte Vorgabe erreicht deshalb nur Neuinstallationen; wer das Plugin schon benutzt, hat `(0,5; 1)` in seiner gespeicherten Konfiguration stehen und bekäme die Verzögerung beim Verdrahten tatsächlich eingeschaltet.
2. *Damit verletzt jede Verdrahtung die Feature-Toggle-Regel*, die für eine Änderung ohne Nachweismöglichkeit das bisherige Standardverhalten als Vorgabe verlangt. Das bisherige Verhalten ist „keine Verzögerung", und es lässt sich ohne Migration nicht erhalten.
3. *Der Nutzen liegt außerhalb der Zielrichtung.* Die Verzögerung dient der Tarnung, nicht der Kampfwirkung. Bei der Unterbrechung wirkt sie sogar gegen den Zweck der Aktion: bis zu eine Sekunde kann das Fenster des Zaubers verbrauchen, der gestoppt werden soll.

**Auflösungsbedingung:** Sobald ein echter Migrationsmechanismus existiert — einer, der einzelne Felder umstellt, statt die Datei zu verwerfen —, ist zu verdrahten und die Vorgabe zugleich auf `(0; 0)` zu ziehen. Vorher ist die wirkungslose Einstellung das kleinere Übel gegenüber einer ungefragt eingeschalteten Verzögerung.

### `TargetColor` wird nicht gelesen, und ihr Elternverweis zeigt auf sie selbst · N

`TargetColor` in `Configs.cs` trägt `[UI("Target color", Parent = nameof(TargetColor))]` — die Eigenschaft nennt sich selbst als Elternschalter. Die Zeile darüber, `TeachingModeColor`, zeigt die richtige Bauart mit `Parent = nameof(TeachingMode)`; Kennzeichen eines Klons ohne Anpassung (Parnas, *Ignorant Surgery*).

**Der Elternverweis ist folgenlos, entgegen der ersten Einschätzung.** `SearchableCollection.cs:39` nimmt ausschließlich `CheckBoxSearch` in die Elternliste auf, also boolesche Einstellungen. `TargetColor` ist ein `Vector4`, landet nie darin, der `TryGetValue` schlägt fehl, und der Eintrag wird auf oberster Ebene einsortiert. Kein Absturz, keine Rekursion in `GetParent`, nur eine Einrückung, die fehlt.

**Der wirkliche Befund ist der fehlende Leser:** Die Farbe wird im gesamten Baum nicht gelesen, wirkt also ohnehin nicht.

**Nicht behoben,** weil die Behebung voraussetzt zu wissen, wo die Farbe gezeichnet werden sollte und welcher Schalter der gemeinte Elternteil ist. Beides geht aus dem Code nicht hervor, und eine erfundene Zuordnung wäre schlechter als der sichtbare Rest. Technische Schuld, kein Defekt mit Wirkung.

### `IBaseAction.IgnoreClipping` wird geschrieben und nirgends gelesen · N, R

`IBaseAction.IgnoreClipping` ist das Flag; `CustomRotation_Invoke.cs` setzt es an fünf Stellen (gezählt 12.10.2026). Kein einziger Leser im gesamten Baum. Der Name benennt genau den Mechanismus, der beim Wiederbelebungsdefekt gefehlt hat: die Anti-Clipping-Sperre für einen Einzelfall aufheben.

Nicht behoben, weil der Wirkungsbereich den Vorgang sprengt. Das Flag wird in `Invoke` breit gesetzt, auch für Fälle ohne Bezug zur Wiederbelebung; ein Leser in `RSCommands_Actions.DoAction` würde die Sperre praktisch überall aushebeln und damit das Verhalten jeder Rotation ändern. Die Behebung verlangt zuerst eine Entscheidung, für welche Aktionen das Flag gelten soll.

**Auflösungsbedingung:** eine Erhebung, welche der sechs Setzstellen eine Ausnahme rechtfertigen, und eine Engführung des Flags auf diese.

### Status-Einstellungen auf der falschen Seite der Aktion · N, U

`ActionSetting` liest `StatusProvide` und `StatusNeed` gegen `Player.Object`, `TargetStatusProvide` und `TargetStatusNeed` gegen das Ziel. Eine ID, die der Spieler nie tragen kann, macht die erste Sperre wirkungslos und die zweite dauerhaft blockierend. `scan11.py` erhebt die Klasse; nach der Behebung der beiden Blaumagier-Stellen (A32) bleiben vier Fundstellen, deren richtige Auflösung PvP- beziehungsweise Bozja-Verhalten voraussetzt.

- `RedMageRotation.ModifyEnchantedZwerchhauPvP` und `ModifyEnchantedRedoublementPvP` (PvP): `StatusProvide` trägt 3238 und 3239, die Schadenswirkungen auf dem **Ziel**; die Barrieren auf dem Spieler sind 3235 und 3236, und die Nachbarin `ModifyEnchantedRipostePvP` greift mit 3234 auf diese Hälfte zu. Die Feldwahl ist damit belegbar falsch — welche Behebung richtig ist, aber nicht: Nach dem Blaumagier-Befund ist ebenso denkbar, dass die **greifende** Zeile (Riposte) der Defekt ist, weil eine Barrieren-Sperre die feste Kombo Riposte → Zwerchhau → Redoublement → Scorch unterbrechen kann. Beide Lesarten sind ohne PvP-Beobachtung nicht zu trennen.
- `ScholarRotation.ModifyDeploymentTacticsPvP` (PvP): `StatusProvide` trägt `Biolysis_3089`, wirkungslos. Als `TargetStatusProvide` gelesen widerspräche es dem `TargetStatusNeed` derselben ID zwei Zeilen darüber und machte die Aktion unbenutzbar; ersatzloses Streichen ist daher die wahrscheinliche Auflösung. `SCH_Default.PVP.cs` hat sich mit einer eigenen `!IsLastAction`-Sperre beholfen.
- `BozjaRotation.cs:368` (Lost Paralyze III): `StatusProvide` trägt `Paralysis`, den Ziel-Debuff. Hier ist die Sperre fachlich plausibel — der Zauber hat keinen nennenswerten anderen Zweck und Lost Actions haben begrenzte Ladungen —, aber die Verschiebung aktiviert eine Sperre, die nie gegriffen hat.
- `BozjaRotation.cs:140` (Lost Excellence): `StatusNeed = [Weakness]`. Anders als die übrigen wirkt diese Prüfung, weil der Spieler `Weakness` tragen kann; sie beschränkt Lost Excellence auf den geschwächten Zustand. Ein Zusammenhang zwischen beiden ist nicht erkennbar — fachliche Frage, kein Seitenfehler.

**Auflösungsbedingung:** je Fundstelle eine Beobachtung im betreffenden Inhalt oder eine Rückfrage an den Upstream-Autor. Alle vier stammen aus Upstream und sind dort unverändert.

**Empfehlung: liegen lassen.** Alle vier verbliebenen Fundstellen liegen in PvP oder Bozja, also außerhalb des Nutzungsprofils, und jede verlangt eine Richtungsentscheidung, die eine Beobachtung im jeweiligen Inhalt voraussetzt. Die Klasse ist vollständig erhoben und durch `scan11.py` gegen Rückfall gesichert — das ist der Zweck der Erfassung, die Bearbeitung ist es hier nicht.

### Der erzeugte Merkmalssatz enthält keine Rollenmerkmale · N, R

`TraitRotationGetter.AddToList` verwirft jedes Merkmal mit `item.ClassJob.RowId == 0`. Rollenmerkmale sind keiner einzelnen Klasse zugeordnet und fallen damit sämtlich heraus: Im erzeugten `Rotation.resx` findet sich kein `EnhancedReprisalTrait`, `EnhancedRampartTrait`, `EnhancedSecondWindTrait`, `EnhancedSwiftcastTrait`, `EnhancedFeintTrait`, `EnhancedAddleTrait` — null Treffer für jedes davon.

**Wirkung:** Wo eine Rollenaktion durch ein Merkmal aufgewertet wird, muss die Stufe als Zahl im Code stehen, statt vom Merkmalsobjekt gelesen zu werden. Zwei Stellen tun das bereits, beide mit 98 für dieselbe Aufwertung (`MitigationDebuffDuration`, `EnhancedReprisalLevel`), inzwischen über eine gemeinsame Konstante.

**Nicht sofort behoben:** Die Änderung liegt im Generator, der nur mit installiertem Spiel läuft, und sie vergrößert den erzeugten Satz um eine ganze Kategorie — Wirkungsbereich und Nutzen sind vor dem Eingriff zu erheben. Zu klären ist außerdem, ob `ClassJob.RowId == 0` tatsächlich das Kennzeichen von Rollenmerkmalen ist oder nur eines von mehreren Merkmalen ohne Klassenbezug.

### `CanUse` als Prüfung, nicht als Wahl — mit Zuweisung als Nebenwirkung · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`CanUse` als bloße Prüfung weist `Target` zu. Anlass waren die beiden Sanctus-Regeln, die mit `DiaPvE.CanUse(out _) || AeroIiPvE.CanUse(out _) || AeroPvE.CanUse(out _)` eine Ersatzgarantie prüften; beide tun es nicht mehr (A178, A231). Es ist dieselbe Nebenwirkung, die beim Wiederbelebungspfad eine eigene Vorkehrung nötig gemacht hat (`RaisePendingAndCastable` sichert und stellt den Zielüberschreiber wieder her, A56).

**Umfang, erhoben statt geschätzt: 120 Fundstellen in 25 Dateien** — `BeirutaRDM` 16, `SGE_Reborn` 11, `NIN_Reborn` 10, `BeirutaSGE` 10, dazu `CustomRotation_Ability`, `CustomRotation_GCD`, `CustomRotation_Items` und die UI. Das Muster ist damit nicht die Ausnahme, die diese Rotation eingeführt hat, sondern die Hausform des gesamten Baums. Das ändert die Frage: Nicht „sollen diese zwei Stellen anders gebaut werden", sondern „hat der Baum eine seiteneffektfreie Prüfung nötig".

**Die Vorfrage, am Code beantwortet (14.10.2026, A286 H1):** RSR bietet keine seiteneffektfreie Prüfung. `BaseAction.CanUse` setzt bei Erfolg `PreviewTarget` und `Target`, schreibt außerhalb der Vorschau `Setting.EndSpecial` und protokolliert über `ActionTracer` — seit A284 erscheint eine abgelehnte Prüfung im Abwehrpfad deshalb auch als „refused before" in `DefenseTrace`. Eine abgelehnte Prüfung setzt kein Ziel; die Zuweisung steht hinter allen Prüfungen.

**Folgen hätte die Nebenwirkung nur über einen zweiten Leser:** Code, der `X.Target` liest, ohne `X.CanUse` in derselben Entscheidung gerufen zu haben, liest das Ziel des letzten erfolgreichen Aufrufs, Prüfung oder nicht. Erhoben: 55 solche Lesestellen außerhalb der Churin-Dateien (kein `X.CanUse` in den drei Zeilen davor), überwiegend in Beiruta-Rotationen und Upstream-Code der Reborn-Rotationen (Barde, Viper, Ninja, Duty). **Offen, meine Arbeit:** je Stelle klären, ob das gelesene Ziel aus derselben Entscheidung stammt; erst wenn eine Stelle ein fremdes Ziel liest, ist eine seiteneffektfreie Prüfung (Wirkungsbereich: alle Rotationen und die Paketnutzer) überhaupt begründet.

### `HasSurvivingShield` misst die **kürzeste** Schildrestzeit, nicht die längste · N, R

**Konzept:** `docs/rotation-flow/09-tank-selfprotection.md`, `docs/rotation-flow/13-aoe-damage-classification.md`
`StatusHelper.HasSurvivingShield`: `GetObjectShield() > 0 && !WillStatusEnd(horizon, false, ShieldStatus)`. `WillStatusEnd` stützt sich auf `StatusTime`, und das liefert das **Minimum** über alle vorhandenen gelisteten Status (`MinStatusRemainingTime`). Beantwortet wird damit „laufen **alle** Barrieren noch?", während der Doku-Kommentar derselben Methode „has an active shield that will still be up" sagt — Singular.

**Wirkung:** Ein Ziel mit mehreren Barrieren — der Regelfall in einer Gruppe mit Heiler, etwa Galvanize plus Eukrasian Diagnosis plus eine Gruppenbarriere — gilt als ungeschützt, sobald die **kürzeste** unter den Horizont fällt, obwohl `GetObjectShield()` weiterhin einen Wert meldet. Der Fehler zeigt in dieselbe Richtung wie der behobene (A43): zu viel Heilung, nie zu wenig. Deshalb hat die Erweiterung der Liste ihn nicht verschlimmert, sondern nur häufiger sichtbar gemacht.

**Seit der Entfernung der Schildanrechnung (A85) hat die Methode keinen Leser mehr im Baum**; der Tankbuster-Plan
liest dieselbe Regel (früheste Barriere entscheidet) aus der Statusliste selbst nach
(`TankbusterForecast.BarrierStandsThrough`, A263), damit kein vom Bestätigungspaket vorhergesagter Status als stehend
gilt; der Befund unten gilt für beide. Sie bleibt trotzdem: `public static` in `RotationSolver.Basic`, also Teil der Paketschnittstelle für abgeleitete Rotationen — ihre Entfernung wäre ein Signaturbruch für den Betroffenenkreis R, und die Methode ist für ihre eigene Frage („hält die Barriere noch") richtig gebaut. Der Befund unten betrifft ihre Genauigkeit, nicht ihre Berechtigung.

**Warum nicht sofort behoben:** Die naheliegende Umkehr auf das Maximum tauscht den Fehler nur aus. Sie würde ein Ziel als geschützt werten, dessen große Barriere gerade ausläuft, solange irgendeine kleine bleibt — und `GetObjectShield()` liefert nur den Gesamtwert, nicht die Aufteilung je Status. Eine Unterschätzung kostet eine überflüssige Heilung, eine Überschätzung kostet einen Tod.

**Auflösungsbedingung:** Laufzeitbeobachtung, welche der beiden Abweichungen tatsächlich auftritt, oder eine Quelle für die Aufteilung des Schildwerts auf die einzelnen Status. **Empfehlung bis dahin: belassen** — die heutige Richtung ist die sichere.

### Statuslisten und Einzelprüfungen ohne die Geschwister-Ids ihrer Wirkung · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
Das Spiel führt jede Wirkung unter mehreren Status-Ids desselben Anzeigenamens — eine je Fassung der Fähigkeit, dazu PvP-Formen und die Fassungen, in die eine Trait aufwertet. Eine handgepflegte Aufzählung, die eine Id nennt und die Geschwister auslässt, antwortet für den Träger der ausgelassenen Id **falsch**, nicht nur ungenau. `scan14.py` erhebt die Klasse über alle Statuslisten in `StatusHelper.cs` (A49) und nennt die jeweils aktuelle Zahl; sie steht hier bewusst nicht, weil sie mit jeder Ergänzung wächst. Genau das ist nach der Behebung von `RampartStatus` und `ReprisalStatus` geschehen: Je mehr Ids geführt sind, desto mehr Gruppen haben einen Vertreter, und desto mehr Geschwister werden überhaupt sichtbar. Eine steigende Zahl ist hier also Fortschritt, nicht Verfall.

**Was davon kein Defekt ist:** 114 Treffer stammen aus `PhantomDispellable` und `PurifyPvPStatuses`, die bewusst Teilmengen sind. Weitere Treffer sind andere Wirkungen unter geteiltem Namen — `Nebula_3051` (Reflexion), `Bloodwhetting_3030` (Lebensraub), `Holmgang` 88 und 1305 auf dem Ziel statt dem Träger (C15). Der Scan druckt zu jedem Kandidaten die Wirkbeschreibung und die Marke `same opening`/`differs`; entscheiden muss ein Leser.

**Zwei benannte Fundstellen, an denen die Zuordnung offen ist:**

- `TankStanceStatus` führt `IronWill` (79) und `RoyalGuard_1833`, nicht aber `IronWill_393`, `IronWill_2843` und `RoyalGuard` (392) — gleicher Anzeigename, gleicher Wirktext „Enmity is increased." Die Liste entscheidet, wen die Zielwahl für einen Tank hält (`ActionTargetInfo.cs`, sieben Stellen, darunter `FindTankTarget` und `FindKardia`) und ob `HasTankStance` für den Spieler greift. `Defiance_1396` und `Grit_1397` („Damage dealt and taken are reduced.") sind die Fassungen vor Shadowbringers und gehören **nicht** hinein.
- `GetCurrentMitigationPercent` (`CustomRotation_OtherInfo`) liest `StatusID.Addle` und `StatusID.Feint` als **einzelne** Id. `Addle_1988` (Geltungsbereich BLM SMN RDM BLU PCT, keine PvP-Aktion in `ActionId.resx`) und `Feint_2185` existieren; die Minderungsbilanz zählt eine vorhandene Schwächung dann als nicht vorhanden. Einzelprüfungen dieser Art sieht `scan14.py` nicht — er erhebt nur Ids in Listen.

**Stand der beiden Fundstellen (28.09.2026):** WrathCombo, das dieselben Abfragen im Spiel stellt, prüft die Tankhaltungen mit Iron Will 79, Defiance 91, Grit 743, Royal Guard 1833 und die Schwächung mit Addle 1203 — genau die Ids, die RSR führt (Status: anderes Werkzeug, keine Spielquelle). Ein Beleg für einen Defekt an diesen Stellen liegt damit nicht vor; offen bleibt die Klasse.

**Warum nicht behoben:** Welche Id das Spiel je Stufe tatsächlich setzt, ist aus den Daten nicht zu entscheiden. Bei `Reprisal_2101` trug der Beleg — genau eine PvE-Aktion, Geltungsbereich auf die vier Jobs verengt, belegter Trait-Stufenwert; bei den Tankhaltungen tut er das nicht, und eine falsch aufgenommene Id kehrt die Antwort in die andere Richtung um. **Auflösungsbedingung:** Laufzeitbeobachtung, welche Id ein Tank beziehungsweise ein Zauberer im Ziel trägt, oder eine Quelle für die Id-Zuordnung je Stufe.

**Empfehlung: erfassen, nicht bearbeiten.** Behoben ist, was eine Wirkkette im Code liest und wo der Beleg trägt. Der Rest ist eine Klasse ohne Schranke: Ein Rückgabewert, den nur Wegsehen grün hält, wäre schlechter als keiner, und `scan14.py` hält die Liste jederzeit wieder abrufbar.

### Die Minderungsbilanz kennt zwei Schadensarten, die Datenquelle drei · N, R

**Konzept:** `docs/rotation-flow/08-mitigation-synergy.md`
Confession und die 15 % von Troubadour, Tactician und Shield Samba sind seit A179 eingerechnet; `IsPhysicalDamageIncoming` prüft seit A179 die physischen Zeilen 1–4.

`GetCurrentMitigationPercent` gewichtet sechs Faktoren binär nach `incomingMagical ? 0.90f : 0.95f` — Addle, Feint, Fey Illumination, Magick Barrier und zwei weitere. Der Wert kommt aus `DataCenter.IsMagicalDamageIncoming()`, das `AttackType.RowId == 5` prüft. Das Blatt kennt aber mehr Werte als 5 und 7; und wenn gerade **niemand** wirkt, ist `CastActionId` überall 0 und die Antwort ebenfalls `false`. Beide Fälle rechnet die Bilanz als **physisch** — mit vertauschten Vorzeichen: Addle zählt dann −5 % statt −10 %, Feint −10 % statt −5 %.

**Der Baustein, der das trennen würde, ist vorhanden und nicht verdrahtet.** `IsPhysicalDamageIncoming()` (`AttackType.RowId == 7`) hat im ganzen Baum **keinen Leser** — kein Kampfpfad, nicht einmal die Diagnoseanzeige, die ihr magisches Gegenstück zeigt. Das ist die Bauform, die `CLAUDE.md` als fehlende Verdrahtung statt tote Stelle führt (Beleg `ResetAvailabilityCheck`): Mit beiden Prädikaten ließe sich „unbekannt" von „physisch" unterscheiden, statt es stillschweigend zusammenzulegen.

**Wirkung: klein, und das ist belegt, nicht vermutet.** `GetCurrentMitigationPercent` hat genau **einen** Verbraucher, die Diagnosezeile im Debug-Reiter (`MainWindow_Debug`, `0.0–0.95`). Kein Kampfpfad liest sie; in meiner eigenen Arbeit dieser Sitzung ist sie als **Vorbild** zitiert, nicht als Aufrufziel. Der Fehler zeigt sich also derzeit nur in einer Anzeige — aber die Bilanz ist öffentlich und damit für abgeleitete Rotationen lesbar, und Konzept 08 sieht sie als künftige Entscheidungsgrundlage.

**Zwei weitere Klon-Reste an derselben Stelle**, ohne eigene Wirkung: Die beiden Methoden sind Kopien mit geänderter Konstante, und die `<remarks>` der physischen Fassung sagt „Returns early on the first confirmed **magical** cast". Der Kommentar bleibt stehen, bis die Stelle bearbeitet wird — ihn allein anzugleichen würde den Beleg der Entstehung tilgen.

**Zuordnung belegt (A179, `v2.xivapi.com`):** 1 斬 slashing, 2 突 piercing, 3 打 blunt, 4 射 shot, 5 魔法 magic, 6 ブレス breath, 7 音波 sound, 8 limit break (das Blatt führt nur japanische Namen). Offen bleibt die Bilanz selbst: Sie rechnet „kein Cast", breath und sound als physisch. **Auflösungsbedingung:** ein Verbraucher im Kampf; dann die Fälle „unbekannt" und „weder noch" eigens führen.

**Empfehlung: erfassen.** Solange nur eine Anzeige betroffen ist, wäre eine Umstellung auf geratener Zuordnung teurer als der Fehler. Wird die Bilanz zur Entscheidungsgrundlage, ist sie vorher aufzulösen — dann gehört auch der dritte Fall benannt, statt ihn als physisch zu führen.

### Die Aquapolis fehlt in der Zielpriorisierung der Schatzkarten-Dungeons · N, U

`ObjectHelper.TreasureDungeonPrio` zählt neun Schatzkarten-Dungeons auf und nennt zu jedem die NPCs, die Vorrang haben — Namazu Stickywhisker in den Lost Canals, Alpaca of Fortune in Cenote Ja Ja Gural, Vaultkeeper in Vault Oneiron. `DataCenter.IsInTheAquapolis` ist der zehnte und **einzige** ohne Zweig; von den elf Flags der Region `Treasure Hunt` ist es das einzige ohne jeden Leser im Baum.

**Entstehung belegt:** Konstante und Aufzählung stammen aus demselben Commit (`4ee849eca`, 14.05.2026, „Add Treasure Dungeon support"). Die Stelle war also nie vollständig — *Ignorant Surgery* in Parnas' Sinn, nicht das Altern einer zuvor richtigen Aufzählung.

**Kandidat, nicht Befund.** Die Gegenhypothese ist nicht widerlegt: Hat die Aquapolis gar keinen Vorrang-NPC dieser Art, dann ist ihre Auslassung aus der Aufzählung richtig und der Befund verschiebt sich auf die Konstante, die dann keinen Verwender braucht. Das ist eine Spieltatsache, und sie ist von hier aus nicht belegbar — das Enum `NPCName` ist aus den Spieldaten erzeugt, führt aber keinen Territoriumsbezug, und die Wikis sind vom Egress gesperrt.

**Auflösungsbedingung:** welche Gegner in der Aquapolis Vorrang haben — zuerst aus den Spieldaten (BNpcName des Gebiets) und Referenzquellen, wie bei den übrigen neun; seine Angabe nur, wo die Quellen schweigen (A286, H3). **Empfehlung: erfassen, nicht bearbeiten** — Schatzkarten stehen nicht im benannten Nutzungsprofil, und ohne die Namen wäre jeder Zweig geraten.

### Zustandsabfragen, die bei jedem Lesen neu über Gruppe oder Gegner laufen · N, R

**Konzept:** `docs/rotation-flow/03-universal.md`
`DataCenter` führt **15 öffentliche statische Eigenschaften, deren Getter iteriert** — darunter genau die, die in den heißen Pfaden stehen: `PartyTank`, `RefinedHP`, `PartyMembersHP`, `NumberOfHostilesInRange`, `NumberOfHostilesInMaxRange`, `AverageTTK`, `DPSTaken`, `AreHostilesCastingKnockback`. Dazu die Helfer derselben Bauform: `SurveyHostileOutput` fragt je Gegner im Radius sechs Status ab, `GetCurrentMitigationPercent` vier, `SurveyStuns` einen.

**Der schwerste Fall war `RefinedHP`, und Upstream hat ihn behoben.** `5ffd22056` (Upstream, 12.09.2026, mit 7.5.6.6 eingezogen) legt einen Cache mit 15 ms Lebensdauer davor, ebenso für `AverageTTK` und die Gruppen-HP-Kennzahlen; `ResetAllRecords` verwirft ihn. Der folgende Absatz beschreibt den Stand davor und bleibt als Begründung stehen, weil das Muster in den übrigen Eigenschaften unverändert vorliegt: Es baute bei **jedem** Lesen ein neues `Dictionary` über alle Gruppenmitglieder, mit einem `try/catch` je Mitglied — und es ist die Nachschlagetabelle, aus der `ObjectHelper.GetHealthRatio` und `GetPlayerHealthRatio` ihren Wert holen. Eine Tabelle, die bei jedem Nachschlagen neu gebaut wird, ist unabhängig von der Häufigkeit eine verkehrte Konstruktion. `GetHealthRatio()` hat 129 Aufrufstellen im Baum.

**Die Kosten misst der Code jetzt selbst (A286, H2).** Aufrufstellen sind kein Maß für Aufrufe je Frame: die meisten liegen in Rotationen, die nie zugleich laufen. `UpdateCost` misst jeden RSR-Aktualisierungsschritt vom ersten bis zum letzten Handler desselben Frames und schreibt nach jedem Kampf eine Zeile in `DefenseTrace.log`: Schritte, Mittel und Höchstwert in ms und den Anteil an der Framezeit der Frames, in denen RSR lief.

**Warum trotzdem kein Defekt:** Das Verhalten ist richtig, nur der Aufwand ist es vielleicht nicht. `PartyMembers` ist auf die eigene Gruppe begrenzt (Allianzmitglieder gehen in `AllianceMembers`, geprüft an `TargetUpdater`), es geht also um maximal acht Einträge je Aufbau.

**Gemessen (sein Protokoll vom 16.10.2026, Stand 36ea60ee9, Weißmagier, 32 Kämpfe):** im Mittel 1,58 bis 2,36 ms je Aktualisierungsschritt, 8,8 bis 12,3 % der Framezeit der Frames, in denen RSR lief; Höchstwerte meist 5 bis 11 ms, zweimal über einer Framezeit bei 60 Bildern je Sekunde (16,6 ms und 36,8 ms). Sein Protokoll vom 17.10.2026 (Stand
a323a6303, Krieger, 12 Kämpfe, die meisten in Occult Crescent): im Mittel 1,55 bis 3,63 ms, 7,2 bis 14,6 % der Framezeit, Höchstwerte
bis 70,0 ms, 43,0 ms und 29,0 ms – mehr Figuren, mehr Last. Seit A299 nennt die Zeile die Gruppengröße.

**Auflösungsbedingung:** trägt RSR einen spürbaren Anteil der Framezeit oder ragen einzelne Schritte heraus, folgt der Umbau, sonst wird der Eintrag geschlossen. **Stand:** Beides trifft nach der Messung zu (rund ein Zehntel der Framezeit; Einzelschritte bis 70 ms). Der Umbau ist damit offene Arbeit: erst erheben, welche Teile des Schritts die Zeit tragen, dann umbauen. **Seit A310 misst der Code das selbst:** Nach jedem Kampf folgt der Kostenzeile „update cost by step" mit Mittel und Höchstwert je Teilschritt (Ziele, BossModReborn, Zustand, Rotation, Aktion, Tank-Beobachtung und die übrigen Handler) und der Zusammensetzung des langsamsten Durchlaufs samt der Speicherbereinigungen, die währenddessen liefen. Eine Bereinigung hält alle verwalteten Threads an, auch für Zuweisungen anderer Plugins; eine Spitze mit Bereinigung ist dann nicht RSRs Rechenzeit. Statisch geprüft: Die gelernten Tabellen schreibt RSR auf dem Thread-Pool (`Task.Run`), nicht im Durchlauf; `DefenseTrace` schreibt zeilenweise mit `AutoFlush`. Nächster Schritt: das nächste Protokoll auswerten, dann gezielt umbauen. **Auflösungsweg, falls ja:** Das Muster liegt im Baum bereits vor — `TargetUpdater` schreibt `PartyMembers`, `AllianceMembers` und `AllHostileTargets` einmal je Frame in `DataCenter`. `RefinedHP` gehört auf denselben Weg. Zu bedenken ist, dass `InEffectTime` zeitbasiert ist: ein Wert je Frame ist damit nicht bitgleich zum Wert je Lesen, also eine Verhaltensänderung ohne Nachweismöglichkeit und nach Projektregel nur mit beibehaltener Voreinstellung zu bauen.

**Empfehlung: erfassen.** Ohne Messung wäre jede Umstellung eine Verbesserung auf Verdacht, und der Betroffenenkreis umfasst die Paketnutzer: `RefinedHP` und die übrigen Eigenschaften sind öffentlich, ihr Aufrufverhalten ist Teil des Vertrags.

### `Configs.Migrate` ist kein Migrationspfad, sondern ein Zurücksetzen · N, R

`Configs.Migrate` lautet vollständig: weicht die gespeicherte `Version` von `CurrentVersion` ab, wird `new Configs()` zurückgegeben — **die gesamte Nutzerkonfiguration fällt auf die Vorgaben zurück**. Der Kommentar „Implement migration logic if needed" weist die Stelle als bewusst offenen Platzhalter aus, nicht als Fehler; sie ist damit technische Schuld und kein Defekt.

**Kosten, und sie sind höher als sie aussehen.** Die Schuld ist nicht nur ein fehlendes Bequemlichkeitsmerkmal, sie **blockiert andere Behebungen**: Jede Korrektur, die einen Vorgabewert ändern muss, um das bisherige Verhalten zu erhalten, ist ohne Feldmigration nicht durchführbar. Der Eintrag zu `InterruptDelay`/`ProvokeDelay` weiter oben ist genau daran gescheitert. Dieselbe Sperre trifft künftig jede Einstellung, deren Vorgabe sich als falsch erweist.

Hinzu kommt die unmittelbare Wirkung für den Auftraggeber: Erhöht Upstream `CurrentVersion` — etwa weil dort ein Feld hinzukommt —, sind beim nächsten Start **alle** eigenen Einstellungen weg. Ein Sicherungsstand lässt sich über `Backup()` anlegen; `Restore()` verweigert allerdings genau dann, wenn die Version abweicht, also im einzigen Fall, in dem man ihn bräuchte.

**Auflösungsbedingung:** eine feldweise Migration, die die gespeicherte Version liest und nur die geänderten Felder umstellt, statt die Datei zu verwerfen. Erst danach sind Vorgabewert-Korrekturen überhaupt möglich.

### Doppelte Zustandswahl in den Zustandskommandos · N

**Konzept:** `docs/rotation-flow/03-universal.md`
Die Zustandswahl liegt an zwei Orten: implizit in `AdjustStateType`, wo `/rotation Auto` über `UpdateTargetingIndex` selbst durch die Zielarten schaltet, sofern `ToggleAuto` aus ist; explizit in den fünf `Cycle*`-Methoden, die dieselbe Aufgabe erneut lösen und über `CycleType` bzw. `DTRType` am Chatkommando und am Leistenklick hängen. Da die `Cycle*` ebenfalls `DoStateCommandType` rufen, greift `AdjustStateType` auch dort; die Toggle-Optionen wirken dadurch als Krücken für fehlende Übergänge, statt als unabhängige Achse.

**Kosten:** `DTRAllAuto` kollabiert mit aktivem `ToggleAuto` auf Off ↔ Auto(0), die Zielarten-Rotation ist dann tot. Umgekehrt ist `ToggleAuto` bei `DTRManualAuto` der einzige Ausschaltweg über die Leiste — ein pauschales Umgehen der Toggle-Auswertung würde ihn beseitigen.

**Empfehlung: belassen.** Die Kosten treffen zwei Bedienvarianten, nicht das Kampfverhalten, und jeder Umbau berührt einen Zustandsautomaten, dessen Übergänge statisch nicht abzusichern sind — der einzige bisher geprüfte Eingriff hätte einer Variante den einzigen Ausschaltweg genommen.

**Auflösungsbedingung:** erst mit einer Möglichkeit zur Laufzeitbeobachtung; ein Zustandsautomat mit acht Zuständen, fünf Zykluswegen und zwei Schaltern ist statisch nicht abzusichern. Bei einem Eingriff ist der Persistenzvertrag zu beachten: `DTRType` und `CycleType` liegen als Ordinalzahlen in der Nutzerkonfiguration, ihre Reihenfolge ist nicht frei änderbar.

Geprüfte Nicht-Fehlstellen: `DTRManualAuto` bildet den vom Enum-Text beschriebenen Zwei-Zustands-Zyklus ab (kein Fehler, AUDIT_LOG A14); ein zu großer `TargetingIndex` kann keinen Indexfehler auslösen, `DataCenter.TargetingType` rechnet `% Count`.

### Die Aufnahme in die AoE-Liste unterscheidet Raidwide und ausweichbare Fläche nicht · N

`Watcher.ActionFromEnemy` nimmt eine Gegneraktion dauerhaft in `HostileCastingArea` auf, wenn die Gruppe mindestens vier Mitglieder hat, die Aktion eine Wirkzeit besitzt, zur Kategorie Spell/Weaponskill/Ability gehört und **jedes** Gruppenmitglied im selben Effektsatz Schaden genommen hat. „Record AOE actions" ist ab Werk an.

**Was daran offen ist, ist die Aufnahme, nicht mehr die Bewertung.** Die Größenordnung ist seit A99 bis A102 gemessen, gespeichert und wird beim Verbrauch verrechnet — vollständiger Stand in `docs/rotation-flow/13-aoe-damage-classification.md`. Damit kostet eine zu klein bewertete Aktion keine Abklingzeit mehr. Die Aufnahme selbst bleibt grob: Ob sich echte Raidwides beim Lernen von ausweichbaren Flächen unterscheiden lassen, ist ohne Spieldaten nicht entscheidbar (Kandidaten: `CastType`, `EffectRange`), und die Reichweitenprüfung `AreaCastCanReachPlayer` entschärft den Fall nur für den Spieler selbst.

**Bewertung: technische Schuld, kein Defekt.** Eine Verschärfung der Aufnahmebedingung wäre eine Verhaltensänderung ohne Nachweismöglichkeit und gehörte deshalb hinter eine eigene Option. Geprüfte Nicht-Fehlstelle: das Speichern läuft asynchron, kein blockierendes Schreiben im Kampfpfad.

**Drei benannte Grenzen der Bewertung, alle erfasst und keine behoben:**

- **Serien kleiner Einschläge.** Mehrere kleine Treffer kurz hintereinander summieren sich; jeder einzeln unter jeder Schwelle, zusammen tödlich. Eine Einzelwertprüfung sieht das nicht. Die Ablage trägt einen Anteil je Aktion, keine Folge über die Zeit.
- **Der VFX-Zweig umgeht die Rechnung.** `IsCastingAreaVfx` erkennt Stack- und Spread-Marker über Effektpfade statt über Aktions-Ids; für die gibt es kein Potential, also greift die Bewertung dort nicht. Konsistent mit „unbewertet heißt mindern", aber diese Auslöser bleiben grob.
- **Die Rückrechnung um die wirkende Minderung ist verworfen, nicht vergessen.** `GetCurrentMitigationPercent` ist eine Aufzählung bekannter Status, also im Zweifel unvollständig — und eine Näherung, die den Wert **erhöht**, ist gefährlicher als eine Beobachtung, die ihn zu niedrig ansetzt und sich beim nächsten ungeminderten Treffer selbst korrigiert (Begründung in Konzept 13, Abschnitt Falsifikation).

**Statt auf eine Beobachtung von ihm zu warten, misst die Regel selbst (A205):** `AreaCastReachedPlayer` hält je gelisteter Aktion fest, ob ihre letzte Landung den lebenden Spieler traf; unter „Skip area defence for casts that missed you" (ab Werk an, Nutzen nicht belegt) öffnet ein Cast, der ihn zuletzt verfehlte, seine Flächenabwehr nicht, bis ein Treffer das widerlegt. Offen bleibt die Aufnahme selbst.

### Zwei Werte des Dunkelritters hängen nicht an der Strategie, für die sie gelten · N

**Konzept:** `docs/rotation-flow/10-drk-blackest-night.md`
`DRK_Reborn.BlackestNightMinHostiles` und `BlackestNightHealthRatio` gelten nur für die engeren Optionen von `BlackestNightUsage` — die Gegnerzahl für zwei davon, die Gesundheitsschwelle für eine. Beide tragen kein `Parent`, erscheinen also immer und unverändert eingerückt, auch wenn die eingestellte Strategie sie gar nicht liest. Ihre Labels verwiesen zusätzlich auf „die Option oben", obwohl zwischen ihnen und `BlackestNightUsage` andere Einstellungen stehen; das ist mit A103 berichtigt, die Kopplung nicht.

**Warum nicht mitbehoben:** `ShouldShowRotationConfigInternal` vergleicht `ParentValue` gegen **einen** Wert. Für die Gesundheitsschwelle wäre die Kopplung damit korrekt möglich, für die Gegnerzahl nicht — sie gilt für zwei Strategien. Eine halbe Kopplung wäre schlechter als keine: Sie ließe den Nutzer glauben, die sichtbaren Werte seien genau die wirksamen.

**Auflösung:** entweder `ParentValue` auf mehrere zulässige Werte erweitern — Wirkungsbereich ist der gemeinsame Zeichenpfad aller Rotationen, Betroffenenkreis R und U — oder es beim Tooltip belassen, der jetzt sagt, für welche Option jeder Wert gilt. **Empfehlung: beim Tooltip belassen**, solange kein zweiter Fall dieser Art auftritt; der Nutzen ist eine Einrückung, die Kosten sind eine Signaturerweiterung im Upstream-Pfad.

### `SpreadDamagePaths` enthält keinen Spread-Marker · N

**Konzept:** `docs/rotation-flow/13-aoe-damage-classification.md`
`DataCenter.SpreadDamagePaths`. Zwei der vier Pfade stehen wortgleich in `SharedDamagePaths`, die anderen beiden sind laut eigenem Kommentar „AOE share markers", also ebenfalls Stack-Marker. Ohne Fehlwirkung, weil `IsCastingAreaVfx` alle drei Listen prüft. **Kosten:** eine Kategorie, die etwas anderes verspricht, als sie enthält. Nebenbefund: `SharedDamagePaths` führt `vfx/lockon/eff/com_trg01_0c` zweimal (2022 und 2024), im `FrozenSet` folgenlos.

**Entstehung belegt** (A41): eingeführt in `33e6acb1` vom 07.05.2026, einem Sammel-Refactoring („Refactor for var usage, safety checks, and plugin compat"), und zwar bereits mit den beiden Duplikaten und dem übernommenen Kommentar. Das ist *Ignorant Surgery* nach Parnas — Klon einer Nachbarliste ohne Anpassung —, kein späteres Veralten. Die Historie war dafür zu vertiefen; im flachen Klon lag der Einführungs-Commit vor dem Anfang.

**Empfehlung: Nullvariante, Punkt offen führen.** Für RSR ist der Unterschied ohne Verbraucher — Spread wie Stack lösen dieselbe Gruppenmitigation aus, eine Positionierung leitet das Plugin nicht. Die Kategorie zu füllen setzt Spieldaten voraus, die hier fehlen: Eine Websuche nach den beiden einzigartigen Pfaden (`x6fd_loc04m_5s1v`, `m0922tar_a0w`) bleibt ohne Treffer, ihre Zuordnung ist damit unbelegt. Die Liste zu streichen ist nur dann verhaltensneutral, wenn genau diese beiden nach `SharedDamagePaths` übernommen werden — und tilgt dann das Signal, das den offenen Punkt sichtbar hält. Wer dennoch aufräumen will: die kleinste verhaltensneutrale Fassung ist das Entfernen der beiden Duplikate `x6r9_loc01_t0a1` und `x6r9_loc02_t0a1` aus dieser Liste.

### Release-Paket enthält vermeidbaren Ballast · N, R

**Am Artefakt belegt** (`latest.zip` von 7.5.7.1+wsh1, 4,67 MB, gemessen 14.10.2026, A279): Nutzlast sind `RotationSolver.dll`, `RotationSolver.Basic.dll`, `ECommons.dll`, `RebornMaterial.dll` (die Oberflächenbibliothek des Upstream-Umbaus, A253) und `RotationSolver.json` samt den beiden `deps.json`, gepackt 2,28 MB; dazu kommen `RotationSolver.Basic.xml` (7,99 MB, gepackt 0,73 MB), der Analyzer samt Symbolen (5,73 MB, gepackt 0,74 MB) und `RotationSolver.Basic.pdb` (1,35 MB, gepackt 0,93 MB). Das NuGet-Paket ist seit dem Upstream-Umbau (`IsPackable=false`) tatsächlich nicht mehr im ZIP.

**Kein Fork-Defekt, sondern Upstream-Verhalten** (A40, A279): Das Upstream-Release 7.5.7.1 enthält dieselben zwölf Dateien in denselben Rollen, 4,50 MB gegenüber 4,67 MB (gemessen 14.10.2026); so schon 7.5.5.41 und 7.5.6.0. Der Fork weicht im Dateibestand nicht ab; auch der Abhängigkeitsgraph beider `deps.json` ist deckungsgleich. Eine Bereinigung wäre damit eine eigene Abweichung im Verpackungspfad.

**Ursache** ist weder die Prune-Regel — `PruneOutputDlls` arbeitet auf `ReferenceCopyLocalPaths` und erfasst nichts davon — noch das `OutputPath` der Projektdatei: der Bauschritt in `publish.yaml` baut mit `--output .\build`, wodurch die Ausgaben aller beteiligten Projekte in einem Verzeichnis landen, das DalamudPackager packt. `GeneratePackageOnBuild` bediente dabei die Autoren abgeleiteter Rotationen; Upstream hat es mit dem UI-Umbau abgeschaltet (A253).

**Kosten:** 4,67 MB Download statt rund 2,3 MB (Stand 14.10.2026), funktional folgenlos.

**Auflösungsbedingung:** Der naheliegende Weg trägt nicht — der Standard-Target von DalamudPackager reicht `Exclude` nicht durch und läuft nur, solange keine eigene `DalamudPackager.targets` im Projektverzeichnis liegt; diese müsste den vollständigen Task-Aufruf samt aller Manifest-Felder nachbauen. `Exclude` vergleicht exakt über `List.Contains` (im Paket DalamudPackager, nicht in diesem Baum), kennt also keine Muster, und das NuGet-Paket trägt die Version im Dateinamen. Der Build-Workflow kompiliert nur und prüft das Paket nicht. Aufgreifen erst, wenn der Veröffentlichungspfad prüfbar ist. Geprüft: die XML-Dokumentation wird zur Laufzeit nicht gelesen.

**Empfehlung: nicht aufgreifen.** Der Auftraggeber hat die Spielbarkeit des Release-ZIPs zur Anforderung gemacht. Ein Eingriff in den Verpackungspfad ist genau die Klasse von Änderung, deren Ergebnis erst am fertigen Release sichtbar wird — und dieser Pfad läuft nur auf einen Tag, wird von keiner Prüfung abgedeckt und weicht dann zusätzlich vom Upstream ab. 2,4 MB Download stehen gegen das Risiko, ein nicht ladbares Paket zu bauen.

### VPR: leerer Zweig einer Struktur, die anderswo eine Entscheidung trägt · U

`VPR_Reborn.GeneralGCD`, die Stellen von Swiftskin's Bite und Swiftskin's Sting. Das Muster `!HasHunterAndSwift` kommt viermal vor; der Vorspann `!IsHunter && !IsSwift` trägt nur an der Coil-Stelle (Hunter's und Swiftskin's Coil, Wahl nach Position) eine eigene Entscheidung, an Bite und Sting wiederholt er die folgende Regel, an der Den-Stelle fehlt er ganz. Weder ein fehlender Inhalt noch dessen Entbehrlichkeit ist belegbar. **Bewusst nicht gelöscht** (AUDIT_LOG A11): Die Entfernung wäre verhaltensneutral, würde aber die Asymmetrie verdecken, die den Befund sichtbar macht. **Auflösung:** Adressat ist der Upstream. **Empfehlung: belassen** — der leere Zweig ist der einzige Hinweis auf die Lücke, und ohne den Autor ist nicht zu entscheiden, ob Inhalt fehlt oder der Vorspann überflüssig ist.

### `AutodutyUpdateState` dupliziert `UpdateState` · U

`RSCommands_StateSpecialCommand.cs`: rund 100 wortgleiche Zeilen, abweichend nur die Fälle `TargetOnly` und `AutoDuty` (`TargetingTypeOverride = targetingType` statt `null`) und der Zustandstext. **Kosten:** jede künftige Änderung am Zustandsautomaten muss an zwei Stellen erfolgen. **Auflösung:** über einen optionalen `TargetingType?`-Parameter zusammenführen, sobald an dieser Stelle ohnehin gearbeitet wird. **Empfehlung: nicht eigens angehen** — die Duplizierung kostet erst bei der nächsten Änderung am Zustandsautomaten etwas, und genau die ist nach dem Punkt oben ohne Laufzeitbeobachtung nicht zu empfehlen. Beide Punkte lösen sich gemeinsam oder gar nicht.

### Das Aktionsfenster rechnet die Bewegungssicherheit ein zweites Mal · U

**Befund (A212):** `MainWindow.GetMovementSafetyStatus` (bis zum Upstream-Umbau vom 04.10.2026 in `RotationConfigWindow`) wiederholt `ActionTargetInfo.CheckMovementSafety` in eigener Form. Die beiden laufen schon auseinander: Die Regel „am Ziel stehend ist ein Sprint nur Schaden" fehlt im Fenster. Die festen Sprünge rechnen seit A313 in beiden gleich (Weite aus dem Wirktext entlang der Blickrichtung). **Kosten:** Das Fenster zeigt eine andere Antwort als die, nach der die Regel entscheidet; im Kampf ändert es nichts. **Auflösung:** `CheckMovementSafety` gibt Ergebnis und Grund heraus, das Fenster zeigt sie an.

### Plugin-Identität ist unverändert die des Upstreams · N

`manifest.json` ist gegenüber `upstream/main` unverändert: `InternalName: RotationSolver`, `RepoUrl` und `IconUrl` verweisen auf FFXIV-CombatReborn, `AcceptsFeedback` ist gesetzt. Ein Fork-Build meldet sich damit unter derselben Plugin-Identität an wie das Upstream-Plugin. Bisher mitgeführt, nicht als Entscheidung erfasst; aufgefallen beim Abgleich der README gegen den Ist-Stand (A39).

**Kosten — Inferenz, nicht am Artefakt belegt:** Dalamud adressiert Plugin-Verzeichnis, Konfigurationsablage und Aktualisierung über den `InternalName`. Daraus folgte, dass Fork- und Upstream-Installation einander verdrängen, dieselbe gespeicherte Konfiguration benutzen und ein Feedback-Weg beim Upstream endet. Belegt ist allein der Manifest-Inhalt; das Installationsverhalten ist in dieser Umgebung nicht prüfbar.

**Gegenposition:** Die gemeinsame Identität ist zugleich der Nutzen — die Nutzerkonfiguration überdauert einen Wechsel zwischen Upstream und Fork, und die Abweichung zum Upstream bleibt null (Betroffenengruppe U).

**Auflösungsbedingung:** Zu entscheiden erst, wenn beide Installationen nebeneinander gebraucht werden. Ein eigener `InternalName` trennt dann auch die gespeicherte Konfiguration; ohne Migrationspfad verlöre der Nutzer seine Einstellungen. Die Freigabe liegt beim Auftraggeber.

**Empfehlung: belassen.** Die geteilte Identität kostet nur, wenn beide Fassungen gleichzeitig installiert sein sollen; sie nützt bei jedem Wechsel zwischen ihnen, weil die Nutzerkonfiguration erhalten bleibt.

## Offene Arbeit

### Tanks: Reflexion und Gruppenbarriere auf demselben Raidwide · N

Seine Protokolle vom 01.10.2026 zeigen Abschütteln und Reflexion fast immer auf demselben Raidwide, eine Sekunde auseinander. WrathCombo verteilt beide auf aufeinanderfolgende Raidwides (je 10 s Sperre gegeneinander); The Balance: stapeln für große Treffer, verteilen über mehrere. RSR kennt die gemessene Größe je Raidwide (`HostileCastingAreaPotential`), Stufe 2 der Abwehr nach Treffergröße (Konzept 08) wäre der Ort. Offen: Fallarbeit für alle vier Tanks (Divine Veil, Dark Missionary, Heart of Light). Konzept 09, „Krieger: die übrige Abwehr", Referenzen.

### Flächenheilung um den Wirkenden: Radius gegen Trefferfläche statt Mittelpunkt · N

Ob ein Spieler in einer Fläche steht, entscheidet der Mittelpunkt seines Modells (Akhmorning, „Raiding Fundamentals", 02.10.2026). `GetCanAffects` rechnet Trefferfläche zu Trefferfläche und zählt ein Mitglied am Rand um seinen Trefferkreis zu großzügig mit. Im Kampf: Lux Solaris und andere Flächenheilungen um den Wirkenden können eine verlangte Anzahl erreicht sehen, die die Heilung nicht trifft. Erhebung aller Leser von `GetCanAffects` für freundliche Flächen offen; Konzept 08, „Wann Lux Solaris zündet".

### Six-sided Star und Flamethrower: Regeln belegt, nicht gebaut · N

Quellen gefunden (Job-Guide und The Balance, 02.10.2026; Konzept 14, „Pausen und Phasenenden"):
- *Six-sided Star (Monk):* 780 + 80 je Chakra, 4 s Wiederholzeit; Einsatz als letzter GCD vor Pause, Rückzug aus einer Fläche oder Tod des Ziels, wenn nur einer passt. Braucht die vorhergesagte Pause (BossModReborn) oder die selbstkorrigierte Zeit bis zum Tod; ohne Modul nur über die Zeit bis zum Tod.
- *Flamethrower (Machinist):* Schwellen von The Balance (ohne Gauge ab 2 Zielen über dem Füller, ab 6 über Air Anchor; nie vor Hypercharge, Chain Saw/Excavator, Drill/Bio; keine große Abklingzeit in den 11 s). Bindet 11 s an Ort und Blickrichtung — gegen seine Spielweise nur bei stehendem Rudel ohne angesagte Fläche zulässig.

### Vorlauf: Dragoon und Viper ohne `CountDownAction` · N

Gemessen am 02.10.2026 (Konzept 04, „Was offen bleibt"). Ohne `CountDownAction` wirkt ein Job im Countdown nichts (`CustomRotation_Invoke.cs`, Basis liefert `null`), auch keinen Trank. Zu klären an den Openern von The Balance und an den Wirktexten, ob dort eine Vorlaufaktion fällig ist. Der Trank im Vorlauf ist dabei die Gesamtfrage: `UseBurstMedicine` im Vorlauf rufen nur die vier Heiler, Barde, Maschinist, Dunkelritter und Revolverklinge auf.

### Vorausschau vor dem ersten Treffer: eigene Minderungen rechnerisch · N

Die Vorausschau liest den gemessenen Gesundheitsverlauf; der ist netto und braucht keine Minderungssätze, setzt
aber erst nach dem ersten Treffer ein. Vorher könnte sie die eigenen laufenden Minderungen aus
`DefensiveValues.g.cs` einrechnen (Konzept 08, Tabelle „Ergebnis“). Zu klären: ob der gemessene Anteil eines
angekündigten Treffers (Konzept 13) diese Lücke schon schließt, weil er die beim Messen liegende Minderung enthält.

### Feste Werte im Fork: jeder offene Wert braucht seinen Loop · N, R

**Vorgabe des Auftraggebers:** keine festen Werte im Code, alles im Spiel ableitbar, eine Ausnahme erst nach einem vollständigen Loop zu genau diesem Wert (CLAUDE.md). Die Liste führt `.github/scripts/audit/fixed_values.json`, und `check_fixed_values.py` beziffert die offenen Werte bei jedem Lauf.

**Was im Kampf daran hängt:** Jede dieser Zahlen ist eine Annahme über das Spiel, die niemand nachprüft. Dazu gehören die Vorlaufzeiten der vorausschauenden Abwehr bei Barde, Tänzer, Maschinist und den Tanks (15 und 20 Sekunden statt der Wirkdauer der jeweiligen Aktion), die Stufe, ab der Addle, Feint und Reprisal länger wirken, die Gruppengröße, ab der die Schadenstabelle misst, und die GCD-Zahlen, ab denen Lux Solaris und Tempera Grassa vor dem Ablauf ihres Status fallen. Ändert ein Patch die Größe, bleibt der Code bei der alten Zahl, und nichts schlägt fehl.

**Reihenfolge nach seinem Nutzungsprofil:** zuerst die Werte in den Jobs, die er spielt (Beschwörer, Weißmagier, Tanks, die zentrale Abwehr in `CustomRotation_OtherInfo`), dann der Rest. Die Vorlaufzeiten der Abwehr sind vermutlich über `DefensiveValues.DurationOf` ableitbar; wo ein Merkmal die Dauer im Wirktext leer lässt, braucht es einen eigenen Weg.

### Audit + Code-Review der gesamten Codebasis

Umfang: `RotationSolver.Basic` (48k Zeilen) · RebornRotations (21k) · ExtraRotations (15k) · Updaters (4k) · UI (11k) · Commands/IPC/Data (3k). Der ganze Baum, Upstream-Code eingeschlossen. Phasen 1 bis 4 sind abgeschlossen (AUDIT_LOG A8, A10).

- **Kern tief lesen:** Rest von `DataCenter`; `StateUpdater`, `TargetUpdater`, `ActionTargetInfo`, `BaseAction`/`ActionBasicInfo`, `CustomRotation_Ability`/`GCD`, `Watcher`, `MajorUpdater`, `ObjectHelper`/`StatusHelper` sind gelesen.
- **Rotationen je Job:** Dispatch-Reihenfolge, Gates, Status-IDs, Zielwahl; bisher nur über die Scanner abgedeckt, nicht Datei für Datei.
- **`RotationSolver/UI`** jenseits der Paar- und Totcode-Scans.
- **Dokumentation** in `docs/rotation-flow/14-codebase-audit.md` — **noch nicht angelegt**. Die früher hier genannte Nummer 07 ist seit `07-heal-target-priority.md` vergeben; der Verweis zeigte damit auf ein Dokument, das es unter diesem Namen nie geben konnte.

Der zweite Durchgang mit allen Prüfmitteln ist geführt (A82) und hat die Skripte selbst instand gesetzt; als Messlage für den Rest des Blocks ist er damit verbraucht.

**Stand:** Die Nachprüfung der vorgemerkten Commits, die vorgehen musste, ist abgeschlossen (A286–A290); die Prüfmittel aus jenem Fenster sind nachgeprüft. Von den übrigen offenen Punkten ist dieser hier der einzige, der weder auf eine Entscheidung noch auf eine Spielbeobachtung wartet — die übrigen sind entweder erfasste Fremdbefunde oder brauchen Laufzeitdaten. Sinnvoller Einstieg ist jetzt das Lesen, nicht das Messen: Die Skripte erfassen Muster, die schon einmal aufgefallen sind, und der verbliebene Bestand — der Rest von `DataCenter`, die Rotationen Datei für Datei, die UI — ist genau der Teil, den kein Muster abdeckt.
