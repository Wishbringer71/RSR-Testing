# 09 · Tank-Selbstschutz

**Leitfrage:** Womit schützt sich ein Tank gegen einen Treffer, und was dürfen Heilung, Hilfe für andere und die
übrigen Pfade der Rotation diesem Schutz nicht nehmen?

Das Dokument gibt den geltenden Stand; die Prüfhistorie steht in `AUDIT_LOG.md`, zurückgenommene Aussagen dort in
Abschnitt C.

## Antwort

- **Gemessener Tankbuster:** das billigste Bündel eigener Abwehr, mit dem der Tank ihn übersteht; die
  Unverwundbarkeit nur, wenn keines reicht; „alles" nur, wenn auch sie nicht verfügbar ist. Was der Plan nicht
  braucht, hält RSR auf allen Pfaden zurück. Zwei Optionen, ab Werk an.
- **Pull:** Die Gegnerzahl öffnet die Einzelabwehr; kurze Minderung, Abtausch und Reflexion fallen sofort, die große
  Minderung und Schutzwall erst, wenn seine Gesundheit beim gemessenen Verlauf innerhalb ihrer Wirkdauer unter die
  eingestellte Linie fiele (ab Werk 30 %). Option, ab Werk an (Abschnitt „Die Abwehr im Pull").
- **Kein Heiler lebt:** In diesem Kampf stellt niemand mehr her, was er verliert; jede Sekunde, die er länger lebt, ist
  Zeit für eine Phönixfeder oder eine Wiederbelebung. Seine Einzelabwehr öffnet, sobald ein Gegner ihn angreift; eine
  Minderung geht, sobald sie bis zum nächsten fälligen Tankbuster wieder bereit ist. Pull-Rückhaltung, feste Staffel und
  die Reserve des Tankbuster-Plans entfallen. Jede Rolle, Option, ab Werk an (Abschnitt „Ohne Heiler: den eigenen Tod
  hinauszögern").
- **Tankbuster ohne Messung** (Vorhersage ohne Wirken, nie gemessene Aktion, erstes Auftreten eines Markers im
  Gebiet): die Einzelabwehr der Tank-Rotationen, je vorhergesagtem Tankbuster eine große Minderung oder Schutzwall und
  die kurzen dazu; die Unverwundbarkeit reaktiv unter `HealthForDyingTanks`. Ein Marker auf dir neben dem Wirken einer
  gemessenen Aktion, die auf niemanden gerichtet ist, gilt als gemessener Tankbuster auf dich. Ein Marker ganz ohne
  Wirken wird mit dem Treffer geplant, der ihm im selben Gebiet schon einmal folgte (A340, „Ein Marker ohne Wirken").
- **Krieger:** Urflackern geht an ein anderes Mitglied, wenn er Urimpuls nicht selbst braucht oder das Mitglied die
  Triage gewinnt; Abschütteln wartet, solange es ihm laufende oder vom Plan gebrauchte eigene Minderungen nähme.
- **Hilfe für das Ziel eines Tankbusters:** Trägt ein anderes Mitglied – meist der andere Tank – einen
  Tankbuster-Marker oder wirkt ein Gegner einen bekannten Tankbuster auf ihn, setzt jeder Tank seine Hilfe für andere
  auf ihn, und die Heiler setzen ihre Einzelabwehr auf ihn. Je Tank eine Option, ab Werk an.
- **Tankwechsel:** Geteiltes Leid auf den anderen Tank nach einem Tankbuster, der dich in Gefahr lässt; zurück
  provoziert, wenn er den nächsten Tankbuster genommen hat und du wieder sicher bist. Option, ab Werk an.
- **Schutzmechaniken mit Auslöser und die Heilung:** Geschützte Tanks sind nachrangige, nicht ausgeschlossene
  Heilziele. Unter Living Dead hält RSR Heilung zurück, solange der Tod noch rechtzeitig kommt (Option, ab Werk aus);
  mit der Option aus wird der Träger geheilt wie ein Ungeschützter, weil sein Fall auf 0 Walking Dead ist und kein
  verschonter Treffer. Für The Blackest Night gibt es keine Rückhaltung.

## Rangordnung

Alles Folgende ist an dieser Ordnung zu messen. Sie ist **lexikographisch**: Eine
nachrangige Stufe darf eine vorrangige nie aufwiegen, gleich wie groß ihr Betrag
wäre.

1. **Das Überleben des Tanks.**
2. **Die Mitnahme positiver Effekte** — Dark Arts, Catharsis, die Selbstheilung von
   Walking Dead.
3. **Die Vermeidung unnötiger Aktionen** — verbrauchte oGCDs, MP, Cooldowns.

**Das Maß für „unnötig" auf Stufe 3 ist die Größe des Treffers, nicht das Gefühl.** Vorgabe des
Auftraggebers, ausgeschrieben in `08-mitigation-synergy.md` („Die Antwort auf einen eingehenden
Treffer"): Gesucht ist Deckung in Höhe des Treffers, gemessen am schwächsten Mitglied. Ein Zehn-
Prozent-Tick rechtfertigt keine große Barriere, ein Treffer oberhalb der vollen Gesundheit dagegen
Barriere **und** Minderung zusammen. Stufe 1 bleibt davon unberührt: Wo das Überleben in Frage steht,
entscheidet nicht die Sparsamkeit.

Daraus folgt die tragende Vorbedingung: **Jede Rückhaltung wird erst geprüft, wenn
Stufe 1 gesichert ist.** Eine Rückhaltung, die einen Effekt der Stufe 2 sichern
will, ohne vorher Stufe 1 zu prüfen, ist die verbotene Aufwiegung. Am schwersten
bei Living Dead: Der Verzicht auf Heilung führt planmäßig in Walking Dead, wo das
Überleben an kumulierter Heilung in Höhe der **vollen maximalen HP** hängt. Kann
der Heiler die nicht aufbringen, tauscht der Verzicht sicheres gegen unsicheres
Überleben.

Eine Lesart bleibt bewusst ausgeklammert: „Überleben des Tanks" gilt hier als
Vorrang *innerhalb* der Frage, ob eine Tank-Schutzmechanik respektiert wird — nicht
als genereller Vorrang des Tanks vor der Gruppe. Für diesen anderen Fall führt RSR
bereits eine eigene Rangfolge in `ActionTargetInfo.GeneralHealTarget`: zuerst wer gleich fällt
(kritische Klasse, Konzept 07), dann Selbst → Heiler → Tank → niedrigste Gesundheit; mit „Choose
the heal target by danger" statt der Rollen-Kurzwege die Gefährdungsklassen 2 und 3 (Stand 29.09.2026).
Sie hier ebenfalls umzustellen wäre eine zweite, größere Änderung.

## Gemessener Tankbuster: das geringste Mittel

**Sachstand:** Für einen gewirkten Tankbuster, dessen Höhe die Tankbuster-Tabelle kennt (Konzept 13), plant RSR je
Durchlauf das billigste Bündel eigener Minderungen, mit dem der Tank den Treffer übersteht. Die Unverwundbarkeit
kommt nur, wenn kein Bündel reicht. „Alles" geht nur hinaus, wenn auch sie nicht verfügbar ist. Was der Plan nicht
braucht, hält RSR auf allen Pfaden zurück. Ein Marker ohne Wirken wird mit dem dort zuletzt gemessenen Treffer
geplant, ohne Zurückhalten außer unter der Unverwundbarkeit (Abschnitt „Ein Marker ohne Wirken"); beide
Einstellungstexte nennen das. Zwei Optionen, ab Werk an:
- „Spend only the mitigation a measured tankbuster needs": Minderungsdrücke und Zurückhalten; Tanks und Heiler.
- „Use the invulnerability before a tankbuster nothing less survives": die Unverwundbarkeit vor dem Treffer; nur Tanks.

Die Regel sitzt auf der Stufe Tanks: Die Rechnung ist für alle vier gleich, die Aktionen liest sie aus den Wirktexten.
Die Hilfe für andere – Einzelabwehr der Heiler, Hilfe der Tanks – sitzt in der zentralen Zielwahl, für alle, die
helfen (Abschnitt „Hilfe für das Ziel eines Tankbusters", A373); Damage Dealer betrifft sie nicht.
- Erkennung in `TankbusterForecast`: Sie liest keine Option und wählt keine Aktion. Sie liest die Casts aller Gegner,
  auch unsichtbarer Helfer, die einen gelisteten oder gemessenen Tankbuster auf ihn wirken (`DataCenter.AreaCasters`,
  A303); vorher sah sie nur anvisierbare Gegner.
  - Dazu ein solcher Cast, der auf kein anderes Gruppenmitglied gerichtet ist – auf den Wirkenden selbst, auf den
    Boden –, während ein Tankbuster-Marker auf ihm steht, der nicht als falsifiziert gelernt ist (A308). Viele Kämpfe
    markieren den Tank und lassen Boss oder Helfer ohne Ziel wirken. Einmal ihm zugerechnet, bleibt der Cast seiner,
    solange er läuft, auch wenn der Marker die Warteschlange verlassen hat; ein neuer Cast derselben Aktion (Wirkzeit
    springt zurück) braucht den Marker wieder. Ein Cast auf ein anderes Mitglied ist dessen Tankbuster.
  - Das Protokoll schreibt solche Casts als „tankbuster coming at you (by the marker on you)".
- Entscheidung in `CustomRotation_Tankbuster.cs`: `UpdateTankbusterPlan` (je Durchlauf), `TankbusterPlanAbility`
  (Ausführung, Fähigkeiten-Slot 9), `HoldDefenceForTankbuster` (Zurückhalten über `IBaseAction.HoldDefenceOnSelf`,
  Gatter in `BaseAction.CanUse`, Ablehnung „HeldForTheInvulnerability").
- Für Heiler: `StateUpdater.ShouldAddDefenseSingle` (Unverwundbarkeit); die Hilfe für andere nach der Größe des
  gemessenen Treffers in `TankbusterHelp`, gelesen von `ActionTargetInfo` und `BaseAction.CanUse`.

### Seine Angaben

- *Hinweis (04.10.2026):* Ob ein Tankbuster tödlich ist, lässt sich ohne den erwarteten Schaden nicht sagen – erst nach
  dem Einschlag oder aus Erfahrungswerten. Eine Unverwundbarkeit bei jedem Tankbuster wäre schädlich. Die
  Erfahrungswerte liefert die Tankbuster-Tabelle.
- *Vorschlag (04.10.2026), geprüft und übernommen:* „rein ökonomisch … für einen tankbuster, welcher 100% tödlich ist,
  nur noch invul nimmt und alle anderen schilde, debuffs und mitigations wegläßt" – „das war der sinn der idee."
- *Präzisierung:* „ich habe nie gesagt, dass bei tankbustern nur eine große minderung erlaubt ist."
- *Vorgabe:* „wenn klar ist, dass man den tankbuster nur mit zwei aktuell verfügbaren (also nicht auf cooldown)
  minderungen überleben kann, dann sollte man diese auch bei verfügbarkeit nutzen. gerade dann, wenn invul
  aufgebraucht ist. und selbst wenn invul verfügbar ist, gilt immer das geringste notwendige mittel und invul ist
  ‚mit kanonen auf spatzen schießen'. manchmal absolut notwendig, manchmal das mittel zweiter wahl, wenn es anders
  auch geht (aber dann ohne risiko)."
- *Hinweis:* Externe Effekte sind mitzubewerten – ein Debuff eines anderen auf dem Boss, der beim Einschlag noch
  wirkt; ein großer Heilerschild auf dem Tank, der beim Einschlag noch steht.
- *Präzisierung zu „alles":* „nicht invul und dann noch zusätzlich buffs, sondern alles raushauen, wenn invul nicht
  verfügbar ist, alle buffs zusammen aber eigentlich nicht reichen, aber man hofft, dass irgendjemand anderes noch
  einen debuff oder ein schild raushaut."
- *Entscheidung D1 (12.10.2026):* „unverwundbarkeit ist die ultimative lösung … wenn tankbuster durch "alles" nicht
  überlebt werden kann, dann ist auf alles zu verzichten und nur unverwundbarkeit zu nehmen."
- *Hinweis (12.10.2026):* „wenn andere Dinge rausgehen können, dann kann es auch die unverwundbarkeit."
- *Entscheidung D2 (12.10.2026):* „es geht um das beste ergebnis für die gruppe."
- *Entscheidung D3 (12.10.2026):* „sichere option gegen unsicher tauschen ist für gruppe nicht unbedingt gut."
- *Hinweise (12.10.2026):* „unverwundbarkeit nach tod bringt nichts, "alles" nach schadenseingang bringt auch nix."

### Die Zustände je Wirken

Für das früheste gemessene Wirken c auf dich rechnet RSR in jedem Durchlauf neu, nur für Tanks und nur, wenn eine der
beiden Optionen an ist:

| Zustand | Wann | Was gedrückt wird | Was zurückgehalten wird |
|---|---|---|---|
| Z0 kein Plan | kein gemessenes Wirken auf dich, oder jede Fähigkeit ist über das Ende des Wirkens hinaus gesperrt | nichts | nichts; unter Heiligem Boden oder Meteoritenfall über den nächsten GCD: alles Bewertete auf dich |
| Z1 gedeckt | eine Unverwundbarkeit steht über H (Status oder eigener Druck) | nichts | alles Bewertete auf dich |
| Z2 Bündel S | das billigste Bündel S übersteht c, und seine ungedrückten Glieder passen in die Druckplätze bis zur Frist; S darf leer sein | jedes Glied von S in seinem Fenster, bis zur Frist | alles Bewertete auf dich außer S; ein Glied von S erst ab seinem Fenster; im letzten GCD nichts mehr, wenn ein Glied fehlt; nach der Frist nichts mehr |
| Z3 Unverwundbarkeit | kein Bündel übersteht c, sie ist einen GCD vor dem Einschlag verfügbar | sie, in ihrem Fenster; nach einer Ablehnung im nächsten Durchlauf neu | alles Bewertete auf dich, solange R > GCD |
| Z4 alles | kein Bündel, keine verfügbare Unverwundbarkeit | jedes verfügbare Glied in seinem Fenster bis zur Frist, das wertvollste zuerst | nichts |

- **Verfügbar** heißt für die Unverwundbarkeit: Option an, erlernt und freigeschaltet, aktiviert, auf der Leiste, über
  der TTK-Schwelle der Aktion und vor dem letzten GCD abgeklungen (`InvulnerabilityReadyBy`). Das ist alles, was
  RSRs eigene Prüfung ablehnen würde und vorher feststeht.
- **Eine Ablehnung der Unverwundbarkeit beim Druck macht nie Z4.** Was sie im Spiel ablehnt – Stun, Amnesie –,
  lehnt jede andere Fähigkeit ebenso ab (Mechanik unten). „Alles" könnte also auch nicht hinausgehen. Gälte das
  einmal nicht, endet das Zurückhalten im letzten GCD, und die anderen Pfade sind frei.
- **Ablehnung einer Minderung:** Lehnt `CanUse` ab, ohne dass Animationssperre, eigener Zauber oder eine Sperre jeder
  Fähigkeit (Stun, Amnesie) das erklärt, wird sie einen GCD lang nicht gedrückt. Für die Frage, ob ein Bündel reicht, zählt sie weiter, solange ihre Sperre vor dem
  letzten GCD endet; Bündel ohne abgelehnte Glieder gehen vor. Eine einmalige Ablehnung führt so nicht zur
  Unverwundbarkeit, eine, die bis zuletzt anhält, schon. Ablehnungen gelten je Wirken (eigene Nummer, auch bei zwei
  gleichen Wirken ohne Pause, erkannt am Zurückspringen der Wirkzeit).
- **Übergänge** ergeben sich aus den Tatsachen: Heilung hebt B (Z3 → Z2, solange die Unverwundbarkeit nicht gedrückt
  ist), Auto-Angriffe senken B (größeres S oder Z3), eine fremde Reflexion senkt P, ein gedrücktes Glied steht und
  fällt aus den Kandidaten. Fallen nach einem Bündel die LP, sodass keines mehr reicht, zieht der Plan die
  Unverwundbarkeit noch.

**Die Optionen im Einzelnen:** Ohne „Spend only …" drückt der Plan keine Minderung und hält nichts zurück; weil dann
niemand ein Bündel sicher drückt, zählt für Z2 nur das leere Bündel, also was schon steht oder gedrückt ist. Ein
Bündel, auf dessen Druck die Rotation nur hoffen kann, ist nicht „ohne Risiko". Ohne „Use the invulnerability …"
wird aus Z3 Z4.

**Wann zurückgehalten wird:** nur mit „Spend only …" und nur, wenn c das einzige bekannte Wirken auf dich ist – kein
zweites Wirken auf dich, gemessen oder nicht, und keine BossModReborn-Vorhersage vor c (bei „Use BMR timeline", gleich
wie weit voraus). Ein Tankbuster-Marker auf dir, während c läuft, gilt als der von c. Z1 hält auch neben einem zweiten
Wirken zurück, wenn die Deckung über jedes Wirken reicht.

**Invarianten**, solange zurückgehalten wird (aus keinem Pfad):
1. Die Unverwundbarkeit wird nie geplant, solange ein Bündel reicht, dessen Glieder bis zum letzten GCD noch gedrückt
   werden können – auch eines mit einem eben abgelehnten Glied.
2. Steht eine Unverwundbarkeit über H oder ist sie für c gedrückt, geht für c keine Minderung mehr hinaus (Z1), auch
   nicht vom Plan selbst und auch ohne „Spend only …" (seine Präzisierung zu „alles"). Holmgang und Totenerweckung
   eingeschlossen; wie viele LP nach ihrem Ende bleiben, rechnet der Plan nicht.
3. Das Zurückhalten trifft nie Heilung, Hilfe für andere, nach außen wirkende Flächenabwehr oder das Befohlene; ein
   Befehl gibt nur sich selbst frei.
4. Ein Glied von S geht aus keinem Pfad vor seinem Fenster hinaus; fehlt im letzten GCD eines, endet das Zurückhalten.
5. Ein Kandidat braucht eine bekannte Wirkdauer von mehr als zwei GCD: Sein Fenster muss sich vor dem letzten GCD
   öffnen.
6. Nach einem Druck gibt es keinen Durchlauf, in dem die Aktion weder Kandidat noch stehend ist.

### Die Rechnung

1. **Was der Treffer nimmt (P):** der höchste je gemessene ungeminderte Anteil der Aktion an deinen maximalen LP
   (Konzept 13), mal jede Minderung, die über H steht – deine (gegen seine Schadensart, Dark Mind je Art), die
   Reflexion des Co-Tanks, Zermürben oder Stumpfsinn auf dem Boss, eine Minderung eines Heilers auf dir – und geteilt
   durch eine Erhöhung der maximalen LP, die dann steht (`TankbusterTable.PredictedShare`).
2. **Was du hast (B):** LP jetzt plus die Barrieren, die über H stehen; läuft eine vorher ab, zählt keine (die früheste
   entscheidet). Ein großer Heilerschild, der hält, zählt voll – sein Hinweis.
3. **Was du noch tun kannst:** jede eigene Abwehr, die erlernt, aktiviert und nach ihren eigenen Prüfungen nutzbar ist
   (Ressourcen eingeschlossen), bis einen GCD vor dem Einschlag abgeklungen ist, eine bekannte Wirkdauer von mehr als
   zwei GCD hat, sich rechnen lässt – Minderung gegen diese Schadensart, Barriere oder Erhöhung der maximalen LP – und
   beim Einschlag nicht schon steht. Ein Knopf zählt einmal (Rachsucht/Verdammnis, Urinstinkt/Urimpuls).
4. **Das geringste Mittel:** unter allen Bündeln dieser Abwehr das billigste, mit dem P unter B bleibt, abzüglich der
   Linie, an der die eigene Unverwundbarkeit gezogen wird (A304, siehe „Überlebt heißt: über der Linie"). Billig nach der
   Abklingzeit des Verbrauchten, bei Gleichstand das kleinere Bündel – was am wenigsten Abklingzeit kostet, lässt am
   meisten für den nächsten Tankbuster (meine Lesart von „geringstes notwendiges Mittel", nicht seine Regel). Reicht
   der Treffer schon ohne alles nicht zum Tod, ist das Bündel leer.

**H, der Horizont:** Restzeit des Wirkens R plus ein GCD für das Eintreffen des Treffers. **Entschieden (A315): bleibt.**
Nach dem Snapshot-Beleg (Mechanik) genügte für einen einzelnen Treffer das Ende des Wirkens; in seinen Protokollen als
Krieger kam er 0,26 bis 0,39 s nach dem Ende des Wirkbalkens (drei Treffer, 13.10.2026). Der GCD darüber ist eine
Sicherheitsmarge, kein Ersatz für eine unbekannte Zeit: Er deckt einen Tankbuster mit mehreren oder nachlaufenden
Treffern („Ein Tankbuster ist nicht ein Einschlag", Abschnitt „Living Dead ist ein Zeitproblem") und Holmgang, dessen
Untergrenze beim Zufügen zählt. Sein Preis liegt auf Stufe 3 der Rangordnung – Fenster öffnen bis zu einen GCD später,
kurze Glieder brauchen eine Wirkdauer über zwei GCD –, ein kürzerer Horizont riskierte Stufe 1. Dasselbe gilt für die
übrigen GCD-Größen des Plans: das Ende des Zurückhaltens im letzten GCD (Sicherheitsnetz neben den Druckplätzen, A307),
die Bestätigung, die nach einem GCD verfällt (ein Ablauf an der gemessenen Laufzeit zählte einen verspätet bestätigten
Druck als verloren und drückte bei Ladungen doppelt), und die Ablehnung für MP oder Reichweite, deren Ende das Spiel
nicht nennt.

**Überlebt heißt: über der Linie** (A304). Ein Bündel reicht nur, wenn es den Tank über „The HP% for tank to use
invulnerability" (`HealthForDyingTanks`, ab Werk 15 %) zurücklässt. An oder unter dieser Linie zieht jeder der vier Tanks
seine Unverwundbarkeit selbst (`EmergencyAbility`), sobald der Treffer gelandet ist – Bündel und Unverwundbarkeit wären
dann beide weg, und bis dahin steht der Tank mit fast nichts vor dem nächsten Auto-Angriff. Reicht kein Bündel bis über
die Linie, plant RSR gleich die Unverwundbarkeit, sonst „alles". Die Linie ist seine Einstellung, keine gewählte Zahl;
auf 0 gestellt gilt die Rechnung bis zum Tod wie vor A304. Das Modell (`tankbuster_model.py`, Prüfung O9 und eigene
Szenarien) hält fest, dass ein geplantes Bündel den Tank über der Linie lässt.

**Ohne Risiko,** wie er es für jedes Mittel unterhalb der Unverwundbarkeit verlangt: der höchste gemessene Treffer,
die LP jetzt und nur, was beim Einschlag steht. Eine Heilung, die bis dahin kommen könnte, zählt nicht – sie ist eine
Annahme über das, was jemand tun wird. Was schon steht, ist keine Annahme: Das unterscheidet die Fremdeffekte seines
Hinweises von einer erhofften Hilfe.

**Wann eine eigene Abwehr als stehend zählt, bevor ihr Status liegt:** Jeder eigene Druck einer bewerteten Abwehr mit
bekannter Wirkdauer und jeder Unverwundbarkeit ist ein Datensatz vom Druck an (`BaseAction.Use`), mit Ziel,
Zeitpunkt, der Restzeit einer eigenen Kopie ihres Status beim Druck und ob der neue Status seither erschienen ist
(`TankbusterForecast.OwnPending`, Urteil `PendingStands`).
- Er lebt, bis der neue Status erscheint – eine eigene Kopie dort, wo die Aktion landet, die länger läuft als eine beim
  Druck lief –, längstens einen GCD ohne Bestätigung des Servers (Effekt-Handler), mit Bestätigung längstens ihre
  Wirkdauer.
- Solange er lebt, zählt die Aktion als stehend, wenn sie in ihrem Fenster hinausging (beim Druck festgehalten), sonst,
  wenn ihre Wirkdauer ab dem Druck über H reicht.
- Eine Erneuerung zählt nicht zusätzlich, solange die alte Kopie schon über H reicht; sie steckt bereits in P.
- Status liest RSR nur aus der Statusliste selbst, nie den, den ein Bestätigungspaket vorhersagt
  (`DataCenter.ApplyStatus` meldet ihn mit unendlicher Restzeit).
- Bei den großen Minderungen ist die Statusangabe der Aktion die gemeinsame Sperrliste; deshalb gilt der Status, an den
  ihr Wirktext gebunden ist, und die Statusangabe nur, wo der Wirktext keinen nennt (Unverwundbarkeiten).

**Die Uhr:** Die Erkennung erhebt einmal je Bild, nach der Rotation. Der Plan rückt R und H um die Zeit seit der
Erhebung vor (hochauflösende Uhr), sodass er alles mit derselben Uhr vergleicht; ein Datensatz, dessen Status die
Erhebung noch nicht gesehen hat, zählt als stehend, auch wenn der Status im laufenden Bild schon liegt.

### Die Ausführung

- **Zeitpunkt:** jede geplante Minderung, sobald der Einschlag nicht weiter entfernt ist als ihre Wirkdauer weniger
  einem GCD (`DefensiveValues.DurationOf`), bis zur Frist vor dem Snapshot (Abschnitt „Spielerweite Sperre und Frist").
  Früher liefe sie vor dem Einschlag ab. Die Unverwundbarkeit ebenso, aber bis zum Ende des Wirkens.
- **Die Sperrgruppe des Spiels** (eine große Minderung zur Zeit, `RampartStatus`) wird für einen geplanten Druck
  übergangen: Ob die Minderung beim Einschlag schon steht, hat der Plan selbst gefragt, und die Staffelung ist genau
  das, was er für einen gemessenen Treffer ersetzt (seine Präzisierung).
- **Ziel:** jeder geplante Druck geht auf dich (`TargetType.Self`). Steht dieselbe Aktion mit einem Objektziel als
  Befehl in der Warteschlange (Upstream 7.5.6.22: DoActions oder abgefangenes Makro, etwa The Blackest Night auf
  `<2>`), geht auch der Druck des Plans dorthin; der Plan zählt sie dann nicht für dich und rechnet ohne sie weiter –
  sein Befehl entscheidet (K5).

### Das Zurückhalten

Abgelehnt wird jede eigene Aktion mit Wirktextwert (`DefensiveValues.For` ungleich leer), die auf dich zielt und nicht
freigegeben ist – Minderung, auch je Schadensart (Dark Mind), Barriere, Erhöhung der maximalen LP (Kampfrausch),
Block (Bulwark), Reflexion um dich –, aus jedem Pfad der Rotation: Einzelabwehr, Notfall, allgemeine Fähigkeiten.
Freigegeben ist in Z2 ein Glied von S ab seinem Fenster.
- **Frei bleiben** Hilfe für ein anderes Mitglied, die Heilpfade (Herz des Korunds heilt einen Tank, den Meteoritenfall
  auf 1 LP setzte; erkannt an einem eigenen Schalter), in der Flächenabwehr, was über dich hinaus wirkt (Reflexion,
  Abschütteln für einen Raidwide), und was du befiehlst.
- **Kein Heiler lebt:** Die Reserve entfällt; neben dem Plan geht der Rest mit auf den Tankbuster (Abschnitt „Ohne
  Heiler: den eigenen Tod hinauszögern"). Die Rückhaltung unter einer Unverwundbarkeit bleibt – unter ihr trifft der
  Tankbuster nicht.
- **Heiler:** keine Einzelabwehr für einen gewirkten Tankbuster, wenn jedes seiner Ziele unter Heiligem Boden oder
  Meteoritenfall über den Einschlag hinaus steht. Eine BossModReborn-Vorhersage öffnet sie weiterhin – sie sagt nicht,
  wen sie trifft –, ein Tankbuster-Marker ebenso, er nennt keinen Zeitpunkt. Den Plan des Tanks kennt der Heiler nicht.

### Andere Pfade, die dem Plan ein Glied nehmen

**Hilfe für andere (K2–K4), universell für alle vier Tanks:** Eine Hilfe, die ein Glied des Plans verbraucht –
dieselbe Aktion (The Blackest Night, Herz des Korunds, Oblation bei nur noch einer Ladung), dieselbe Abklinggruppe
(Urflackern ↔ Urinstinkt/Urimpuls) oder dieselbe Ressource (Intervention, Cover ↔ Holy Sheltron über Eid) –, wartet,
bis das Glied gedrückt ist oder der Plan es nicht mehr braucht (`HelpTakesNeededMember`, Gatter in
`BaseAction.CanUse`, Ablehnung „TakesWhatTheTankbusterPlanNeeds").
- *Gebraucht* ist ein Glied nur, wenn kein überlebendes Bündel ohne es bleibt. Bleibt eines, geht die Hilfe, und der
  Plan rechnet ohne das Glied weiter, meist teurer. Die Unverwundbarkeit zählt dabei nicht als Ersatz, sonst erzwänge
  jede Hilfe die Kanone auf Spatzen.
- *Triage* (`HelpWinsTriage`), wenn beide es brauchen: Ein kritischer Heiler gewinnt („fällt der Heiler, fällt die
  Gruppe"). Ein kritischer Schadensausteiler nicht („am ehesten entbehrlich"). Ein kritischer anderer Tank gewinnt,
  solange deine Unverwundbarkeit vor dem letzten GCD übernehmen kann und du selbst nicht kritisch bist; dann geht die
  Hilfe, der Plan zieht für dich die Unverwundbarkeit, und beide überleben. Kann sie es nicht, behältst du das Glied
  (D3): Dein Glied wirkt gegen einen gemessenen Treffer zu bekanntem Zeitpunkt; den kritischen Stand des anderen
  können die Heiler bis zu seinem nächsten Treffer noch ändern.
- Für einen ungemessenen Tankbuster gilt die Triage des Kriegers (Abschnitt „Krieger: Urimpuls für sich oder Urflackern für andere").

**Abschütteln hebt Glieder auf (K1):** Es hebt Rachsucht, Verdammnis, Urinstinkt, Urimpuls und Kampfrausch auf und kommt
aus Einzelheilung und Flächenabwehr, die beide vom Zurückhalten frei sind (`WAR_Reborn.ShakeItOffTakesNeeded`).
- Ein gedrücktes Glied, dessen Status noch nicht liegt, zählt wie ein liegendes.
- Ist ein solches Glied gebraucht, hält Abschütteln auch bei Gefahr für die Gruppe zurück, **außer** der gemessene
  Raidwide tötet nach den Zahlen einen Heiler oder mehr als ein Mitglied (D2): Dann kostet er die Gruppe mehr als ein
  toter Tank. Tötet er genau einen Schadensausteiler, bleibt das Glied – ein toter Tank kostet meist den Kampf (Konzept
  08). Gezählt wird mit dem gemessenen Anteil des Raidwides an den vorhergesagten effektiven LP jedes Mitglieds;
  Unverwundbare und Tote zählen nicht (`DataCenter.AnnouncedHitKills`).

**Befohlenes Objektziel (K5):** siehe Ausführung, „Ziel".

**Reflexion für den Raidwide (K6, A302):** Die Einzelabwehr hält Reflexion am Tankbuster zurück, wenn der Tank
gedeckt ist und ein Raidwide nach ihrem Ende und vor ihrer neuen Bereitschaft angesagt ist (Konzept 08, „Gegner-Debuffs
am Tankbuster und am Raidwide"). Der Plan folgt derselben Regel in seiner eigenen Form: Reflexion ist dann ein
vorbehaltenes Glied (`PlanCandidate.Reserved`, Bedingung `RaidwideAfterEffectBeforeRecast` wie in
`HoldReprisalForRaidwide`). Gewählt wird zuerst das billigste Bündel ohne vorbehaltenes Glied, dann mit ihm, zuletzt
mit einem eben abgelehnten. „Gedeckt" heißt hier: ein Bündel ohne Reflexion überlebt. Reicht keines ohne sie, geht sie –
nach seinem Kriterium kommt dann der Tank ohne sie in ernste Bedrängnis. Vor A302 wählte der Plan Reflexion ab Stufe 98
(Dauer aus dem Merkmal, A266) als billigstes Glied (60 s Wiederholzeit) auch dann, und am folgenden Raidwide fehlten
jedem 10 %. Das Protokoll nennt es: „(Reprisal kept for the announced raidwide)".

### Spielerweite Sperre und Frist

**Sachstand (A307):** Eine Sperre jeder Fähigkeit macht aus einer Ablehnung keine Antwort über die Aktion; ein Druck für c
geht nur, solange er vor dem Snapshot beim Server ankommt; ein Bündel ist nur wählbar, wenn seine noch zu drückenden
Glieder bis dahin hineinpassen.

1. *Erkennung (Stufe „alle", zentral, `StatusHelper.PlayerAbilitiesLocked`):* ein Status, dessen Zeile im Blatt des
   Clients `LockActions` trägt (Fähigkeitsprüfung statt Liste), oder Amnesie (Zeilen 5, 1092, 4210, über die Namen
   des Generators aus der englischen Fassung). Die Restzeit ist die längste dieser Status; nennt keiner ein Ende, gilt
   sie als jetzt endend. Die Erkennung urteilt nicht.
2. *Ablehnung unter Sperre:* wie unter der Animationssperre keine Antwort; Neuversuch im nächsten Durchlauf, für jedes
   Glied und für die Unverwundbarkeit.
3. *Frist für eine Minderung* (`MitigationDeadline`): Ende des Wirkens weniger der gemessenen Laufzeit von Anfrage bis
   Wirkung (`TankbusterForecast.RoundTrip`). Der Client sieht das Ende einen halben Umlauf nach dem Server, ein
   Druck braucht einen halben Umlauf hin; also muss er einen ganzen Umlauf vor dem gezeigten Ende hinaus. Vor der
   ersten Messung liegt die Frist am Ende des Wirkens.
   - Gemessen an jeder eigenen Anfrage ohne Wirkzeit in dem Moment, in dem sie zum Server hinausgeht – gedrückt von
     RSR, vom Spieler oder aus der Warteschlange des Spiels: Ihre Sequenz ändert sich (`ActionManagerEx`), und die
     Wirkung, die dieselbe Sequenz zurückträgt (`SourceSequence`), beendet die Messung. Eine eingereihte oder
     abgewiesene Anfrage ändert keine Sequenz und zählt nicht. Es gilt die letzte Messung.
   - Bis A340 maß RSR ab dem eigenen Druck, wenn keine Animationssperre lief, und paarte nach Aktions-Id. Die
     Wiederholzeit des GCDs prüfte das nicht: RSR drückt den nächsten GCD bis `CalculatedActionAhead` vor ihrem Ende
     (ab Werk ein Viertel GCD), das Spiel reiht ihn dann ein (ClientStructs: „action is queued"). Meldet `UseAction`
     dabei Erfolg – unbelegt, keine Quelle nennt den Rückgabewert –, zählte die Wartezeit zum Umlauf, und der Abstand
     (Punkt 5) wurde gelesen, bevor die Sperre gesetzt war: ein Bild statt der Sperre, also beliebig viele
     Druckplätze. Die Messung an der Anfrage ist in beiden Fällen richtig.
   - *Für die Unverwundbarkeit keine Frist vor dem Ende des Wirkens:* Holmgangs Untergrenze zählt beim Zufügen, nach
     dem Snapshot (Mechanik); für Heiligen Boden, Meteoritenfall und Totenerweckung ist es unbelegt. Bis zum Ende des
     Wirkens gedrückt, kann sie noch halten; ein Tod, den sie nicht verhindert, kostet nichts weiter. Nach dem Ende
     des Wirkens gibt es keinen Plan mehr – seine Hinweise („unverwundbarkeit nach tod bringt nichts") betreffen die
     Zeit danach.
4. *Nach der Frist* drückt der Plan für c keine Minderung mehr, auch nicht in „alles", und das Zurückhalten für c
   endet (Z2). Was die anderen Pfade danach drücken, drücken sie mit eigenen Gründen für die Treffer danach.
5. *Druckplätze* (`PressSlots`): Von dem Moment an, in dem Animationssperre, eigener Zauber oder Sperre enden
   (`ObjectHelper.GetInstantHealLeadTime`, Restzeit der Sperre), passt bis zur Frist je gemessenem Abstand ein Druck
   (`TankbusterForecast.PressSpacing`: Zeit von der gemessenen Anfrage, bis ihre Animationssperre abgelaufen ist).
   - Ein Bündel ist nur wählbar, wenn seine ungedrückten Glieder hineinpassen; sonst die Unverwundbarkeit (ein
     Druck, bis zum Ende des Wirkens), ohne sie „alles", das Glied zuerst, das allein am meisten vom Treffer nimmt.
   - Ein Ersatz für ein Glied, das eine Hilfe für andere nähme (K2–K4), zählt nur, wenn er hineinpasst.
   - Vor der ersten Messung zählt der Plan keine Plätze (Grenze).
6. *Sperre über das Ende des Wirkens hinaus:* Für c gibt es nichts zu tun; kein Plan, nichts zurückgehalten, eine
   Zeile im Protokoll.

**Im Kampf:** Nach einem kurzen Stun gehen die Glieder im ersten Durchlauf danach; bleiben zu wenige Plätze, geht die
Unverwundbarkeit allein. Vorher sperrte eine Ablehnung unter Stun das Glied einen GCD: Reichte diese Sperre in den
letzten GCD, fiel es aus dem Plan, und der Plan zog die Unverwundbarkeit, wo nach dem Stun noch Zeit für das Bündel
war. Ein Druck, der nach dem Snapshot ankäme, unterbleibt; vorher kostete er eine Abklingzeit, ohne den Treffer zu
ändern. Endet der Stun nach dem Ende des Wirkens, geht für diesen Treffer nichts hinaus.

**Warum so, und was ausgeschlossen ist:**
- Vorbeugend vor einem Stun zu drücken scheidet aus: Ein Stun ist nur über ein BossModReborn-Modul vorhersagbar, und
  dafür gibt es keinen Weg ohne BossModReborn.
- Die Sperre an `GetActionStatus` zu erkennen scheidet aus: Welche Rückgabe einen Stun meint, ist unbelegt; der Status
  ist belegt und nennt die Restzeit.
- Ein falsch erkannter Status mit `LockActions`, unter dem Fähigkeiten doch gehen, kostet nichts: keine Ablehnung
  notiert, Neuversuch in jedem Durchlauf. Eine übersehene Sperre lässt das alte Verhalten stehen – sichere Richtung.
- Die Frist kommt aus der gemessenen Laufzeit, nicht aus einer festen Marge; läge der Snapshot später als belegt,
  unterbliebe sonst ein Druck, der noch gewirkt hätte.
- Die Abklinggruppe der großen Minderungen und die Frist für die Unverwundbarkeit bleiben unberührt.
- Nur Druckplätze ohne die Frist, oder nur die Sperre ohne die Plätze, scheidet aus: Ohne Plätze bliebe nach einem
  späten Stun ein Bündel geplant, das nicht mehr ganz hinausgeht, und der Tank stürbe. Die alte Sperre für einen GCD
  hatte diesen Fall zufällig verdeckt, weil sie die Glieder aus dem Plan nahm.

**Dieselbe Frist für die Ansagen von BossModReborn (A319, Stufe „alle", `DataCenter.PressReachesBeforeSnapshot`):**
Jeder Pfad, der auf eine Ansage hin drückt – die Fenster für Tankbuster, Flächentreffer und Rückstoß, die
Auffrischung vor einer Ansage (`BMRShouldRefreshBefore`), über sie die Einzelabwehr der Heiler und die Tränke –, gilt nur,
solange ein Druck von jetzt an noch vor dem angesagten Zeitpunkt beim Server ankommt: Restzeit bis zur Ansage mehr als
die Zeit, bis ein Druck hinausgehen kann (Animationssperre, eigener Zauber, Sperre jeder Fähigkeit), plus die gemessene
Laufzeit. Der Zeitpunkt von BossModReborn ist der Snapshot: das Ende des Wirkens (`CastFinishAt`) oder die Auslösung der
Aktion in seinen Bausteinen, der Übergang des markierten Zustands in seinem Zeitplan (Code `be8ff77`, 17.10.2026; bei
Markern ohne Wirken eine Schätzung des Modulautors). Vorher galt eine feste Grenze von 0,6 s, ohne Herleitung.
*Im Kampf:* Ein Druck, der nach dem Snapshot ankäme, unterbleibt und die Abklingzeit bleibt für den nächsten Treffer;
einer, der noch rechtzeitig ankommt, geht auch unter 0,6 s. *Grenze:* Für Barrieren, Heilungen und den Schutz vor
Rückstoß zählt vermutlich das Zufügen, das nach dem Snapshot liegt (unbelegt); dort ist die Frist um diesen Abstand zu
früh – dieselbe Richtung wie die alte Grenze, nur kürzer.

**Grenzen:** Vor der ersten gemessenen Laufzeit und dem ersten gemessenen Abstand – praktisch nur bis zur ersten Anfrage
ohne Wirkzeit nach dem Laden des Plugins – gilt das alte Verhalten. Die letzte Messung schwankt mit dem Netz; eine zu lange macht die
Frist zu früh, eine zu kurze zu spät. Die Plätze rechnen nicht mit Fenstern, die sich erst nach dem ersten Platz öffnen;
nach Invariante 5 öffnet sich jedes Fenster mehr als einen GCD vor dem Einschlag.

**Messmittel:** `DefenseTrace.log` nennt je Druck des Plans die gemessene Laufzeit („round trip … s") neben der
Restzeit des Wirkens; die Plan-Zeile nennt die Druckplätze, wenn ein überlebendes Bündel nicht mehr hineinpasst; eine
Zeile, wenn eine Sperre über das Ende des Wirkens reicht.

### Die Abwehr der vier Tanks

„Kandidat": kann Glied eines Bündels sein. „Gehalten": das Zurückhalten erfasst sie. Werte und Dauern aus den
Wirktexten über den Generator (`DefensiveValues.g.cs`); wo der Wirktext die Dauer offen lässt, aus dem Merkmal, das
sie setzt – zur Laufzeit aus dem Spiel (`TraitDurations`), sonst aus `trait_durations.json`. Unterhalb der Stufe des
Merkmals gilt die Dauer, die der Tooltip der Aktion auf dieser Stufe nennt: der Wirktext des Clients, von Dalamud
ausgewertet wie im Spiel (`TraitDurations.TryBelowTrait`, A316). Übernommen wird sie nur, wenn sie kürzer ist als die
des Merkmals – eine Auswertung, die die Synchronisation überginge, zeigte dessen Wert, und eine zu lang angenommene
Dauer ließe die Minderung vor dem Treffer ablaufen. Reichweite, Radius,
Abklingzeit und Gruppe aus den Spieldaten (xivapi, Blatt `Action`, 12.10.2026).

| Job | Aktion | Wirkung laut Wirktext | Dauer | Reichweite/Radius | Abklingzeit, Gruppe | Rolle | Gehalten |
|---|---|---|---|---|---|---|---|
| alle | Schutzwall | −20 % | 20 | 0/0 | 90, 47 | Kandidat | ja |
| alle | Reflexion | Gegner −10 % | 15 ab Stufe 98, darunter laut Tooltip | 0/5 | 60, 45 | Kandidat | ja |
| alle | Abtausch | Rückstoßschutz, Slow | – | 0/0 | 120, 49 | unbewertet | nein |
| PLD | Sentinel / Guardian | −30 % / −40 % | 15 | 0/0 | 120, 20 | Kandidat | ja |
| PLD | Bulwark | Block | 10 | 0/0 | 90, 16 | kein Kandidat (Block lässt sich nicht rechnen) | ja |
| PLD | Sheltron / Holy Sheltron | −15 % (Zusatzwirkung nicht gezählt) | 6 ab Stufe 74, darunter laut Tooltip / 8 | 0/0 | 5, 3; Eid 50 | Kandidat | ja |
| PLD | Intervention, Cover | Intervention −10 % auf ein anderes Mitglied (weitere −10 % mit Schutzwall oder Guardian, nicht gezählt); Cover übernimmt Treffer | 8 ab 82, darunter laut Tooltip / 12 | 30 bzw. 20/0 | 10 bzw. 120; Eid 50 | Hilfe für andere | nein |
| PLD | Divine Veil, Passage of Arms | Gruppe | – | 0/30 bzw. 8 | 90 / 120 | Gruppenwerkzeug | nein |
| PLD | Heiliger Boden | unverwundbar | 10 | 0/0 | 420, 25 | Unverwundbarkeit | – |
| WAR | Rachsucht / Verdammnis | −30 % / −40 % | 15 | 0/0 | 120, 22 | Kandidat | ja |
| WAR | Urinstinkt / Urimpuls | −10 % | 6 / 8 | 0/0 | 25, 7 | Kandidat | ja |
| WAR | Urflackern | −10 % auf ein anderes Mitglied | 8 ab 82, darunter laut Tooltip | 30/0 | 25, 7 | Hilfe für andere | nein |
| WAR | Kampfrausch | maximale LP +20 % | 10 | 0/0 | 90, 16 | Kandidat | ja |
| WAR | Abschütteln | Gruppenbarriere; hebt Kampfrausch, Rachsucht/Verdammnis, Urinstinkt/Urimpuls auf | – | 0/30 | 90, 15 | Gruppenwerkzeug | nein |
| WAR | Holmgang | LP nicht unter 1 | 10 | 6/0 | 240, 25 | Unverwundbarkeit | – |
| DRK | Shadow Wall / Shadowed Vigil | −30 % / −40 % | 15 | 0/0 | 120, 21 | Kandidat | ja |
| DRK | Dark Mind | physisch −10 %, magisch −20 % | 10 | 0/0 | 60, 9 | Kandidat je Schadensart | ja |
| DRK | The Blackest Night | Barriere 25 % der maximalen LP | 7 | 30/0 | 15, 3 | Kandidat; auf andere Hilfe | ja |
| DRK | Oblation | −10 % | 10 | 30/0 | 60, 19, 2 Ladungen | Kandidat; auf andere Hilfe | ja |
| DRK | Dark Missionary | Gruppe | – | 0/30 | 90, 15 | Gruppenwerkzeug | nein |
| DRK | Totenerweckung | Tod → Walking Dead | 10 | 0/0 | 300, 25 | Unverwundbarkeit | – |
| GNB | Camouflage | −10 % | 20 | 0/0 | 90, 16 | Kandidat | ja |
| GNB | Nebula / Great Nebula | −30 % / −40 % und maximale LP +20 % | 15 | 0/0 | 120, 22 | Kandidat | ja |
| GNB | Heart of Stone / Herz des Korunds | −15 % | 7 / 8 | 30/0 | 25, 6 | Kandidat; auf andere Hilfe | ja |
| GNB | Heart of Light | Gruppe | – | 0/30 | 90, 15 | Gruppenwerkzeug | nein |
| GNB | Aurora | Regeneration | – | 30/0 | 60, 20 | Heilpfad | nein |
| GNB | Meteoritenfall | unverwundbar, LP auf 1 | 10 | 0/0 | 360, 25 | Unverwundbarkeit | – |

**Das Wertemodell** (`DefensiveValue`, öffentlich im Paket): eigene Minderung, Minderung des Gegners physisch und
magisch, Barriere als Positionswerte; dazu als nicht-positionale Eigenschaften eigene Minderung je Schadensart
(`SelfPhysical`, `SelfMagical`, gelesen über `SelfAgainst`), Erhöhung der maximalen LP (`MaxHp`) und Block
(`Guards`); `ProtectsBearer` sagt, ob eine Aktion ihren Nutzer schützt. Der Konstruktor mit vier Werten bleibt
(Vertrag für abgeleitete Rotationen). Was zurückgehalten wird, erzeugt der Generator aus dem Wirktext
(Fähigkeitsprüfung statt Liste).

**Die Pfade**, die eine Abwehr auf dich drücken können:

| Pfad | Zurückgehalten | Was dort fällt |
|---|---|---|
| Befehl (`CommandNextAction`, `ForceEnable`) | nein (sein Befehl) | jede |
| Countdown | – (vor dem Kampf kein Wirken, kein Plan) | The Blackest Night, Divine Veil |
| `EmergencyAbility` (unter `HealthForDyingTanks`) | ja, außer der Unverwundbarkeit | Unverwundbarkeit; PLD Heiliger Boden, Cover, Intervention |
| Tankbuster-Plan | – | Bündel oder Unverwundbarkeit |
| Heilpfade | nein (`HealPathRunning`) | WAR Abschütteln, Urflackern; GNB Aurora |
| Flächenabwehr | nur was nur dich trifft | Reflexion, Gruppenwerkzeuge, DRK Oblation/TBN |
| Einzelabwehr (eigener Befehl: nur ihr Pfad frei) | ja | alle Kandidaten, Reflexion, Bulwark, Dark Mind, Kampfrausch, Abtausch |
| `GeneralAbility` | ja | WAR Urinstinkt/Urimpuls, Kampfrausch, Äquilibrium; PLD Eid (Holy Sheltron, Intervention) |
| Hilfe für andere (Ziel ≠ du) | nein, außer K2–K4 | Intervention, Cover, Urflackern, TBN, Herz des Korunds, Oblation |

**Die Tatsachen**, mit Quelle, Zeitpunkt und Ausfall:

| Tatsache | Quelle | erhoben | Ausfall |
|---|---|---|---|
| R, H | Wirkzeit des Gegners | `TankbusterForecast.Update`, einmal je Bild nach der Rotation; im Plan auf „jetzt" vorgerückt | Wirken abgebrochen: verschwindet; neu begonnen: neue Nummer |
| P | Tabellen-Höchstwert × Status, die über H stehen | dieselbe Erhebung | ungemessen: kein Plan; unter Damage Up gemessen: nicht gespeichert |
| B | LP + Barrieren über H (früheste) | dieselbe Erhebung | Barriere ohne gelisteten Status zählt nicht |
| Datensatz | Druck (`BaseAction.Use`), Bestätigung (Effekt-Handler), Status gesehen (Erhebung) | Spielthread bzw. Erhebung | Bestätigung fehlt: Datensatz verfällt nach einem GCD |
| Ablehnung | `CanUse` falsch außerhalb Animationssperre, eigenem Zauber und Sperre jeder Fähigkeit | Plan | – |
| Laufzeit, Abstand | letzter eigener Druck ohne Wirkzeit und ohne laufende Sperre: bis zur Bestätigung, bis zum Ende seiner Animationssperre | Spielthread bzw. Erhebung | vor der ersten Messung: Frist am Ende des Wirkens, Plätze ungezählt |
| Sperre | Status mit `LockActions` oder Amnesie, Restzeit aus dem Status | Plan | kein Ende genannt: endet jetzt |
| BossModReborn | Zeitleiste, Ziel des ersten Eintrags | Plugin-Abfrage | kein Modul: keine Vorhersage |

### Mechanik (Belege)

- *Unverwundbarkeiten* (xivapi, Blatt `Status`): Heiliger Boden 82 und 1302, Meteoritenfall 1836 „Impervious to most
  attacks"; Holmgang 409, Undead Rebirth 3255, Invulnerability 4275 „Most attacks cannot reduce your HP to less than
  1". Im Code `StatusHelper.ImperviousStatus`, `StatusHelper.InvulnerabilityStatus`. Je 10 s; Abklingzeiten Heiliger
  Boden 420 s, Holmgang 240 s, Totenerweckung 300 s, Meteoritenfall 360 s.
- *Totenerweckung* (Job-Guide): Fällt die Gesundheit unter Living Dead auf 0, folgt Walking Dead; wird bis zu dessen
  Ablauf Heilung in Höhe der maximalen LP aufgenommen, folgt Undead Rebirth, sonst der Tod. Die geplante
  Totenerweckung verlagert den Tod auf eine Heilaufgabe (Abschnitt „Living Dead ist ein Zeitproblem, kein Zustandsproblem").
- *Wirkdauern und Werte* aus den Wirktexten; Urimpuls und Schiltron nur mit der Grundminderung, nicht der kurzen
  Zusatzwirkung der ersten Sekunden – das unterschätzt sie, die sichere Richtung.
- *Kategorie:* Alle vier Unverwundbarkeiten und alle Glieder des Plans sind Fähigkeiten (`ActionCategory` Ability,
  xivapi 12.10.2026).
- *Sperre:* Das Blatt `Status` trägt `LockActions` bei Stun („Unable to execute actions"), Schlaf, Versteinerung und
  rund 260 weiteren Status, etwa Deep Freeze, Down for the Count, Bubble Gaol. Amnesie (Zeilen 5, 1092, 4210: „Unable
  to use abilities") sperrt alle Fähigkeiten ohne dieses Feld. Silence und Pacification treffen nur Zauber und
  Waffenfertigkeiten (xivapi 12.10.2026).
- *Snapshot:* Der Schaden eines gewirkten Angriffs wird festgelegt, wenn der Wirkbalken endet, und später zugefügt;
  eine Minderung danach wirkt nicht mehr. Die LP-Untergrenze von Holmgang zählt, wenn sie beim Zufügen steht.
  Community-Quellen (consolegameswiki „Snapshot", akhmorning „Raiding Fundamentals", 12.10.2026), keine offizielle.
  Für Totenerweckung, Heiligen Boden und Meteoritenfall ist unbelegt, ob Snapshot oder Zufügen zählt; der Plan drückt
  sie deshalb bis zum Ende des Wirkens (Abschnitt „Spielerweite Sperre und Frist").

### Grenzen

**Die Rechnung irrt zu viel Mittel,** wo der Höchstwert über dem typischen Treffer liegt, eine Heilung vor dem
Einschlag nicht zählt, Zusatzwirkungen fehlen, H die Sicherheitsmarge eines GCD trägt und ein Wert unter Verwundbarkeit mit mehr
Stapeln gemessen wurde als jetzt liegen (nur in der Wechsellage). Das kostet Abklingzeit, nicht Leben – im
schlimmsten Fall eine Unverwundbarkeit (240 bis 420 s) für einen Treffer, den Minderung getragen hätte; sie fehlt dann
beim nächsten wirklich tödlichen. Umgekehrt irrt die Messung fast nur nach unten (Konzept 13): Ein Urteil „tödlich
auch mit allem" aus einem zu kleinen Wert ist erst recht richtig.

**Zu wenig Mittel** (der Tank fällt mit Plan):
- Ein Treffer über dem bisher gemessenen Höchstwert, etwa unter einer Stärkung des Bosses. Ein Treffer unter Damage Up
  wird nicht gespeichert, aber auch nicht hochgerechnet: Die Wirktexte nennen keine Stärke. Stärkungen unter anderem
  Namen bleiben unerkannt.
- Ein Wert unter Verwundbarkeit aus weniger Stapeln als jetzt.
- Ein zu niedrig gespeicherter Höchstwert (Konzept 13: eine unbezifferte Minderung beim Treffer). Mit „Spend only …"
  ist das schlimmer als ohne Tabelle: Der Plan wählt zu wenig und hält den Rest zurück.
- Über der Linie der Unverwundbarkeit, aber knapp: Nach dem Einschlag zählt die Rechnung nicht weiter, die Abwehr ist
  wieder frei, und die Linie ist seine Einstellung, nicht der gemessene Schadenseingang bis zur nächsten Heilung.

**Die Unverwundbarkeit kommt eher zu oft:** nur, wenn kein Bündel reicht – bei LP jetzt. Ein Tank, der tief in den
Tankbuster geht, bekommt sie eher, auch wenn ein Heiler ihn noch hochgeheilt hätte. Das ist der Preis von „ohne
Risiko"; die Neurechnung gibt sie zurück, wenn die Heilung vor dem Ziehen kommt.

**Lagen:**
- *Allein, Vierer-Instanz:* wirkt; die Regel braucht keinen Co-Tank.
- *Zwei RSR-Tanks:* Jeder plant für die Tankbuster auf ihn; die Reflexion des anderen zählt, sobald sie steht; Hilfe
  zwischen ihnen regelt die Triage.
- *BossModReborn ohne Modul:* unverändert; die Regel liest keine Vorhersage.
- *Stufensynchron:* Abwehr zählt nur, wenn erlernt; unterhalb der Stufe eines Merkmals gilt die Dauer aus dem
  Tooltip (A316), sonst ist die Aktion kein Kandidat. Ob Dalamuds Auswertung der synchronisierten Stufe folgt, ist
  nicht belegt; folgt sie nicht, zeigt sie den Wert des Merkmals, und der wird verworfen. Dass gemessene Anteile auf
  der Stufe ihres Inhalts vergleichbar bleiben, ist ein Schluss, nicht gemessen.
- *Mehrere Wirken auf dich:* geplant wird für das früheste gemessene; zurückgehalten wird dann nichts. Liegen zwei in
  einem GCD, deckt ein Minderungsplan nur das erste.
- *Ein ungemessenes Wirken oder eine frühere BossModReborn-Vorhersage* neben einem gemessenen: Der Plan drückt, hält
  aber nichts zurück.
- *Ein Tankbuster nur als BossModReborn-Vorhersage, eine noch nie gemessene Aktion, ein Marker beim ersten Auftreten
  im Gebiet oder ohne Ziel des Spielers:* kein Plan; dann gilt die Abwehr ohne Messung (Abschnitt „Krieger: die übrige
  Abwehr", Staffelung) und die reaktive Unverwundbarkeit unter `HealthForDyingTanks`. Die Vorhersage nennt keine
  Aktion, und beim ersten Auftreten ist nichts gemessen – mehr als die reaktive Unverwundbarkeit gibt es dort ohne
  Annahme nicht. Ein Marker neben dem zielosen Wirken einer gemessenen Aktion ist geplant (Erkennung, A308), ein
  Marker ganz ohne Wirken mit dem gelernten Treffer (nächster Abschnitt). Die Marker-Zeit ist die der Warteschlange (fünf Sekunden für einen Marker auf einem
  Gruppenmitglied, Upstream; seit A334 nicht mehr dessen eigene Restwirkzeit); ein Cast, der erst danach beginnt, wird
  ihm nicht zugerechnet.
- *Tankwechsel:* hält bei bereiter Unverwundbarkeit, die RSR für jeden nächsten Treffer nutzen würde
  (`CustomRotation.InvulnerabilityUsable`).
- *Der Boss trägt den ganzen Kampf ein Damage Up:* kein Treffer wird gespeichert, kein Plan.

**Warum nicht die alte Abwehr:** Sie gibt je Tankbuster eine große Minderung. Ein gemessener Treffer, der zwei braucht,
tötet damit – seine Präzisierung nennt genau das –, und die reaktive Unverwundbarkeit zündet unter der
Sterbe-Schwelle, die ein Treffer von voller Gesundheit in einem Schritt überspringt. Die Staffelung aus seinem
Protokoll vom 01.10.2026 (zwei große auf einen Tankbuster, nichts auf den nächsten) entsteht im Plan nur, wenn jeder
Tankbuster zwei braucht, und dann ist der nächste mit der Unverwundbarkeit oder mit allem geplant.

### Ein Marker ohne Wirken

**Ein Tankbustermarker auf dir, zu dem kein Wirken kommt, wird mit dem Treffer geplant, der demselben Marker im selben
Gebiet zuletzt folgte** (A340). Die Tabelle maß solche Treffer schon (der Treffer nach einem Marker zählt als
Tankbuster), der Plan las aber nur Wirken: Gegen einen Marker allein gab es nie mehr als die Staffelung und die
Unverwundbarkeit unter der Linie, die ein Treffer von voller Gesundheit in einem Schritt überspringt. In seinem
Protokoll vom 23.10.2026 (Heiler) kamen 8 von 14 Markern auf Tanks ohne Wirken in der Entscheidungszeile, darunter vor
Blitzga und Blitzbogen, die danach gemessen wurden.

- **Gelernt** (`TankbusterMarkerWatch`, `TankbusterMarkerHits.json`, kein Download): Bestätigt ein Treffer einen
  Marker auf einem Gruppenmitglied, gleich welchem, merkt RSR für Gebiet und Pfad die Aktion des ersten Treffers und
  die Zeit vom Erscheinen des Markers bis zu ihm. Eine Aktion, die schon die ganze Gruppe traf, bestätigt den Marker,
  wird aber nicht gelernt (Raidwide-Regel, A337). Folgt beim nächsten Mal eine andere Aktion, ersetzt sie den Eintrag;
  dieselbe Aktion behält die kürzere Zeit, damit eine danach getaktete Abwehr eher früh als spät kommt.
- **Genutzt** (`TankbusterForecast`, `Learned`): Steht ein nicht verworfener Marker auf dir, kommt kein Wirken auf dich
  zu und hat die Tabelle die gelernte Aktion gemessen, gilt sie als Tankbuster auf dich, fällig nach der gelernten
  Zeit ab dem Erscheinen des Markers. Gerechnet wird ohne Debuffs am Gegner – der Treffer kann von einem Helfer kommen,
  den ein Debuff auf dem Boss nicht erreicht –; Reprisal und Ähnliches zählen nicht und werden nicht geplant. Der Plan
  wählt Bündel oder Unverwundbarkeit wie sonst. Ohne Ziel des Spielers oder nach Ablauf der gelernten Zeit kein Plan.
- **Zurückgehalten wird nur unter der geplanten Unverwundbarkeit,** die jeden Treffer abhält. Ein Bündel hält die
  Staffelung nicht zurück: Kündigt der Marker diesmal einen anderen Treffer an, ließe ein Bündel für den gelernten den
  Tank mit weniger als der Abwehr des Markers. Damit gibt RSR hier mehr aus, als „Spend only the mitigation a measured
  tankbuster needs" verspricht – der Text gilt dem gemessenen Tankbuster, und welcher kommt, ist hier eine Vorhersage,
  kein Wirken (Stufe 3 der Rangordnung gegen Stufe 1).
- **Was geschieht, wenn die Annahme nicht stimmt:** Folgt diesmal ein kleinerer Treffer, war die Unverwundbarkeit
  vielleicht vergeudet, und der Eintrag ersetzt sich. Folgt ein größerer, steht wenigstens die Staffelung, wie ohne
  Plan. Kommt der Treffer früher als je zuvor, kann die Unverwundbarkeit noch ausstehen, während die Staffelung
  zurückgehalten wird – nur beim frühesten Auftreten, weil die kürzeste Zeit gilt. Die Zeit ist auf Seite des Clients
  gemessen (Erscheinen des Effekts bis zum Effekt-Handler).
- **Im Protokoll:** „tankbuster coming at you (learned from the marker on you)". Ein Treffer nach einem Marker, für den
  weder Wirken noch gelernter Treffer vorlag, schreibt weiter „no cast taken for it".

### Prüfmittel

- `tankbuster_model.py` (CI, sobald sich das Skript ändert; A323) setzt die Regeln dieses Abschnitts um – nicht den Code – und lässt sie gegen eine
  simulierte Welt laufen: Bilder unterschiedlicher Länge mit Rucklern, Erhebung nach der Rotation, Laufzeit zum
  Server, Status vor oder nach der Bestätigung, Ablehnungen, ein Stun beim Druck der Unverwundbarkeit, Abschütteln und
  Urflackern als freie Pfade, andere Pfade, die jederzeit drücken wollen, alle vier Schalterstellungen, knappe
  Treffer, bei denen jedes Glied gebraucht wird; seit A307 ein Stun, der alle Pfade sperrt (im Fenster, kurz vor dem
  Ende, vom Anfang bis kurz vor das Ende), und der Einschlag nach dem Snapshot: Eine Minderung zählt, wenn der Server
  sie bis zum Ende des Wirkens angewandt hat, Holmgang bis zum Zufügen.
  - Geprüft: überlebt, was der Plan überlebbar nannte (an der wahren Welt); nach der Unverwundbarkeit nichts
    Bewertetes mehr; die Unverwundbarkeit nie, solange ein Bündel reicht; jedes gedrückte Glied steht am Einschlag und
    wird bis dahin genau einmal gezählt; ein freier Pfad macht aus einem überlebten Treffer keinen Tod und erzwingt
    keine Unverwundbarkeit; „alles" nur, solange die Unverwundbarkeit nicht verfügbar ist; keine Minderung des Plans
    kommt nach dem Snapshot an (O10); bleibt nach einem Stun ein Druck und ist die Unverwundbarkeit verfügbar, überlebt
    der Tank (O11).
  - Der Selbsttest schaltet die Defekte ein, die frühere Fassungen des Codes hatten (Archiv A263, A267), dazu die
    Sperre für einen GCD unter Stun, den Druck bis zum Ende des Wirkens und die Bündelwahl ohne Druckplätze (A307);
    jeder muss gefunden werden.
  - Ausgelassen: ein langer Stun, bevor der Abstand der Drücke gemessen ist (Grenze oben).
  - Nicht abgedeckt, dort trägt nur die Herleitung: ein zweites Wirken und BossModReborn-Vorhersagen, Befehle, die
    Unverwundbarkeit nach einem Bündel bei fallenden LP, Minderung je Schadensart, die Kopplung über Eid, die anderen
    drei Tanks (gleiche Rechnung, andere Zahlen). Das Modell übernimmt H = R + GCD (Abschnitt „Die Rechnung", A315).
- `DefenseTrace.log` schreibt je Wirken auf dich „tankbuster coming at you: … takes …% … you have …%", je Änderung des
  Plans „tankbuster plan for … : Rampart + Damnation, leaving …% against …%" (oder „survivable as it stands, nothing
  spent", „the invulnerability already covers it, nothing else", „nothing less survives it: the invulnerability",
  „nothing survives it and no invulnerability, everything goes (…)"), je Ablehnung einer Minderung eine Zeile und je
  Druck die „used"-Zeile. Nachgesteuert wird über die Tabelle: Jeder höhere Treffer hebt ihren Wert, und der Plan
  rechnet je Durchlauf neu.
- Prüfgrad: Modell, Prüfskripte, Compile über die CI; im Spiel nicht beobachtet.

## Die Abwehr im Pull

**Sachstand.** Die Einzelabwehr öffnet im Pull nach der Gegnerzahl: mindestens „Use tank mitigation when this many
enemies target you" (ab Werk 2) Gegner in 3 Yalm, die ihn anvisieren, und mindestens einen davon hat er selbst
angegriffen. Kurze Minderung,
Abtausch und Reflexion fallen sofort. Die große Minderung – Guardian oder Sentinel, Damnation oder Vengeance, Shadowed
Vigil oder Shadow Wall, Great Nebula oder Nebula – und Schutzwall warten, bis seine Gesundheit beim gemessenen
Netto-Verlauf innerhalb ihrer eigenen Wirkdauer unter „Health the big mitigation waits for" (ab Werk 30 %) fiele
(`CustomRotation.BigMitigationHeldInPull`, `GetHealthRatioIn` mit der selbstkorrigierten Zeit bis zum Tod; Wirkdauer aus
den Wirktexten). Vor dem ersten Verlauf eines Pulls – die ersten 2,5 s – warten sie auch. Danach geht jede, sobald sie
gebraucht wird; feste Abstände gibt es dann nicht mehr, sie schließen einander nur noch über ihren Status aus. Option
„Hold the big mitigation and Rampart in a pull until your health needs them", ab Werk an.

Die Rückhaltung gilt nur, wenn allein der Pull die Abwehr geöffnet hat. Ein Tankbuster-Marker oder gelistetes Wirken auf
ihn, ein gelistetes Wirken auf einen Tank, eine Tankbuster-Vorhersage von BossModReborn oder sein Befehl öffnen sie ohne
Rückhaltung und mit der alten Staffel. Als strategische Rückhaltung läuft sie über die universelle Schicht
(`HoldSingleDefense`): Steht er oder ein Tank in der kritischen Klasse, fällt sie weg. Lebt kein Heiler, entfallen
Rückhaltung und Staffel ganz (Abschnitt „Ohne Heiler: den eigenen Tod hinauszögern").

Eine Methode für alle vier Tanks: `StaggeredBigMitigation(big, upgraded)` trägt, was Paladin, Krieger, Dunkelritter und
Revolverklinge als je eigene, wortgleiche Kopie führten; die vier Rotationen rufen sie mit ihren Aktionen auf.

**Warum.** Die Gegnerzahl sagt, dass ein Pull da ist, nicht, ob er die großen Abklingzeiten braucht – zwei schwache
Gegner kosteten eine große Minderung, die beim nächsten großen Pull fehlte. Ob ein Pull bewältigbar ist, misst der
Verlauf, netto nach jeder Heilung und Minderung (Konzept 08, „bewältigbar wird gemessen, nicht gesetzt"). Die
Gegnerzahl bleibt der Vorbote für das, was vor dem ersten Treffer fallen muss und wenig kostet: kurze Minderung,
Abtausch, Reflexion. Die Linie ist seine Haltung – wie nah am Rand er die großen Abklingzeiten zurückhält –, also eine
Einstellung; die Laufzeit urteilt, ob die Lage sie erreicht (Konzept 18, „Haltung mit Laufzeiturteil").

**Modell** (`tank_pull_defense_model.py`, Regelkreis: ein Heiler heilt nach der Vorausschau, der Tank antwortet nach der
Regel; je Fall 100 zufällige Dungeon-Läufe zu 20 Pulls – kleine Rudel, Wall-to-Wall-Pulls von 6 bis 12 Gegnern,
einzelne starke Gegner; Summen, gerechnet am 25.10.2026):

| Fall | heute: Tote / s unter 30 % | gebaut: Tote / s unter 30 % | große + Schutzwall heute / gebaut | GCD-Heilungen heute / gebaut |
|---|---|---|---|---|
| Grundfall | 19 / 506 | 5 / 180 | 1161 / 783 | 5824 / 5845 |
| 25 % mehr Schaden | 147 / 2086 | 83 / 1292 | 1161 / 929 | 7696 / 7687 |
| schwächerer Heiler | 78 / 1584 | 36 / 948 | 1161 / 863 | 7086 / 7167 |
| großer Pull, die ersten 4 s doppelt hart | 64 / 1352 | 27 / 768 | 1161 / 846 | 6872 / 6826 |

In den ersten 5 s eines großen Pulls ist die Zeit unter 30 % gleich (harter Auftakt: 72 s wie heute): Die Rückhaltung
kostet den Auftakt nichts, weil dort die kurzen Mittel fallen und der Verlauf nach 2,5 s steht. Die Linie, gerechnet im
Grundfall: 15 % – 3 Tote, 248 s; 30 % – 5 Tote, 180 s; 45 % – 8, 274; 65 % – 13, 355. Je tiefer die Linie, desto mehr
Abklingzeit bleibt für den Moment, in dem sie gebraucht wird; bei 15 % steht er schon länger tief und der Heiler heilt
mehr.

**Bilanz im Kampf.** *Stärke:* Ein Rudel, durch das ihn sein Heiler trägt, kostet keine große Abklingzeit; der
Wall-to-Wall-Pull danach hat sie. Fällt er schnell, kommt sie, sobald der Verlauf es zeigt, und die zweite ohne Wartezeit
nach der ersten. *Schwäche:* Die ersten 2,5 s eines Pulls ohne große Minderung; im Modell ohne Folge, weil die kurzen
Mittel dort liegen. Leicht mehr GCD-Heilungen des Heilers (0,4 % im Grundfall, 1,1 % beim
schwächeren Heiler). *Chance:* Ein menschlicher Heiler, der
spät heilt, löst sie über den Verlauf früher aus – die Regel misst das Ergebnis, nicht seine Absicht. *Bedrohung:* Ein
Rudel, das in den ersten Sekunden sehr hart zuschlägt (Zauber aller Gegner zugleich); gerechnet als doppelter Auftakt,
dort nicht schlechter als heute, mit der Gefahrenklasse als Netz. *Überwiegt:* der Nutzen, deutlich – weniger Tote und
weniger Zeit tief in jedem gerechneten Fall.

**Ausgeschlossen.** *Ein zusätzlicher Auslöser bei weniger Gegnern* (die Abwehr öffnet auch bei einem Gegner, wenn der
Verlauf fällt): im Modell in jedem Fall schlechter, weil er Abklingzeiten an einzelnen starken Gegnern verbraucht, die
der Heiler trägt. *Die große sofort ab sechs Gegnern:* im harten Auftakt etwas besser (24 statt 27 Tote), sonst gleich
oder schlechter; die Sechs leitet sich aus nichts ab. *Die alte Staffel unter der Rückhaltung behalten:* im Modell nur
Kosten (Grundfall 10 statt 5 Tote). *Die mittleren Abklingzeiten* (Camouflage, Dark Mind, Bulwark, Kampfrausch): nicht
modelliert, also nicht einbezogen; Kampfrausch hat seine eigene Verlaufsregel.

**Grenzen.** Das Modell trägt Annahmen ohne Quelle (Schaden je Gegner, Heiler nach Vorausschau, Pullmischung); keine
Selbstheilung des Tanks, keine Barrieren. Die Richtung hielt in allen vier Fällen. *Prüfgrad:* Modell mit Selbsttest,
statisch, Compile; im Spiel nicht beobachtet. Messmittel ist die Selbstkorrektur der Zeit bis zum Tod
(`ScoreTtkForecast`), auf der die Rückhaltung urteilt.

## Ohne Heiler: den eigenen Tod hinauszögern

**Sachstand.** Lebt in der Gruppe kein Heiler (`DataCenter.AnyLivingHealer`; der Spieler zählt, wenn er selbst Heiler
ist), gilt mit der Option „Without a living healer, spend your defence to stay alive longer" (ab Werk an), für jede
Rolle:

1. *Die Einzelabwehr öffnet*, sobald ein Gegner den Spieler als Ziel hat (`StateUpdater.ShouldAddDefenseSingle`,
   `DataCenter.UnhealedUnderAttack`) – nicht erst bei einem Tankbuster oder ab der Pull-Zahl. Die übrigen Quellen
   bleiben, wie sie sind.
2. *Eine Minderung auf ihn geht, sobald sie bis zum nächsten fälligen Tankbuster wieder bereit ist*: eine Ladung bleibt
   übrig, oder die Abklingzeit einer Ladung ist vorbei, bevor ihr Fenster vor dem Treffer beginnt (Wirkdauer weniger ein
   GCD). Sonst wartet sie (`BaseAction`, Ablehnung „KeptForTheNextTankbuster"). Fällig ist der nächste Tankbuster nach
   der Vorhersage von BossModReborn, sonst nach dem Abstand in diesem Kampf: letzter Treffer plus kürzester Abstand, der
   vom Kampfbeginn bis zum ersten eingerechnet (`TankbusterForecast.NextOnPlayerIn`, gefüttert aus `Watcher`). Ohne
   Tankbuster in diesem Kampf ist keiner fällig, und nichts wartet; blieb er zwei Abstände lang aus, gilt der Kampf als
   weitergegangen (Ausnahme im Register, A361). Öffnet ein Tankbuster selbst die Abwehr – Wirken auf ihn oder einen
   Tank, Vorhersage, sein Befehl (`DataCenter.TankbusterOpensDefence`) –, gilt diese Prüfung nicht: Was dann geht, ist
   für diesen Treffer.
3. *Tank:* Die große Minderung und Schutzwall warten weder auf den Verlauf (Pull-Rückhaltung) noch auf die feste Staffel;
   sie schließen einander nur über ihren Status aus (`DefenceSpentWithoutHealer` in `StaggeredBigMitigation`).
4. *Tank, gemessener Tankbuster:* Der Plan drückt sein billigstes Bündel wie sonst; den Rest hält er nicht mehr für den
   nächsten zurück (`HoldDefenceForTankbuster`), er geht über die Einzelabwehr mit auf diesen. Unter einer
   Unverwundbarkeit über den Treffer bleibt alles zurückgehalten.

Ein lebender Wiederbeleber ändert daran nichts. Die Prüfung gilt je Kampf: Ein Toter kann erst zum Startpunkt der
Instanz zurück, wenn niemand mehr im Kampf ist (Return), also kehrt der Heiler zwischen zwei Kämpfen zurück – nach jedem
Trash-Pull ebenso wie nach einem Boss, auch im Allianz-Raid. In Prüfungen und Normal-Raids ist der Bosskampf ein
durchgehender Kampf, Trash ist dort selten.

**Seine Angaben** (25.10.2026). *Hinweis:* „ist berücksichtigt, dass der tank mehr minderungen nehmen muss, wenn heiler
tot ist und kein rezzer für heiler bereitsteht?" – ohne Heilung bleibt jeder Schaden. *Hinweis:* „vielleicht hat ja ein
anderer spieler eine phoenixfeder oder man selbst. dann muss man das eigene sterben lange genug hinauszögern, bis sich die
möglichkeit bietet, zu rezzen." – das Maß ist die Zeit, die er nach dem Heiler noch lebt. *Hinweis:* „es geht um EINEN
kampf. zwischen den kämpfen kann der heiler zurück zum ausgang bei instanzen gehen. auch bei ally-raids. also muss nicht
zwischen trashmob-pulls und boss unterschieden werden. nur bei prüfungen und normal-raids ist es ein durchgehender kampf,
da sind dann aber seltener trashmobs."

**Mechanik** (Status je Quelle). *Return:* in PvE nicht nutzbar, solange man selbst oder ein Gruppenmitglied im Kampf
ist; in einer Instanz zum Startpunkt (Fan-Wiki consolegameswiki, „Return"; deckt sich mit seiner Angabe). *Phönixfeder*
seit Patch 7.3 (05.08.2025): im Kampf nutzbar in 4er-Dungeons, 4er-Prüfungen, Feldoperationen und weiteren Inhalten, nicht
in 8er-Prüfungen und Raids; 8 s Wirkzeit, 15 Yalm, 360 s Wiederverwendung, geteilt mit den Arzneien (Fan-Wiki
consolegameswiki, „Phoenix Down", Patch-Historie dort; kein offizieller Text gesichtet). Ob ein anderer Spieler eine Feder
trägt, sieht der Client nicht.

**Warum.** Mit Heiler füllt sich, was er verliert, wieder auf; eine Minderung lohnt dort, wo der Heiler nicht
hinterherkommt – das messen Pull-Rückhaltung und Plan. Ohne Heiler ist jeder Treffer endgültig, und wie lange er lebt,
hängt an der Summe dessen, was seine Minderungen über den Kampf abfangen. Eine Minderung, die bereit liegt, ohne dass
sie gebraucht wird, verschenkt Laufzeit – außer, ein Tankbuster kommt in ihrer Abklingzeit: Er nimmt mehr als die
Autoattacken im selben Fenster, und was ihn abdeckt, deckt die Autoattacken danach mit. Daher die Prüfung auf den
nächsten fälligen Tankbuster statt einer Rückhaltung nach Gesundheit.

**Modell** (`tank_pull_defense_model.py`, Abschnitt ohne Heiler; der Heiler stirbt zu einem zufälligen Zeitpunkt des
Kampfs; Selbstheilung des Tanks pauschal 15 % alle 25 s unter 75 %, Unverwundbarkeit bei 15 %; gerechnet am 25.10.2026).
*Großer Pull*, Heiler stirbt in den ersten 60 %, 400 Pulls: Der Tank stirbt heute in 142, gebaut in 133, mit Staffel statt
ohne in 163; gebaut rettet ihn in 9 Pulls, in denen er heute stirbt, und verliert ihn in keinem, den er heute übersteht.
*Boss*, Sekunden, die er nach dem Heiler lebt, je 200 Kämpfe:

| Fall | heute | Plan ohne Reserve | dazu Öffnen ohne Prüfung | gebaut | mit bekanntem nächstem Tankbuster |
|---|---|---|---|---|---|
| Heiler stirbt nach 40–120 s | 62,1 | 79,4 | 72,5 | 79,4 (+28 %) | 79,7 |
| dasselbe, 25 % mehr Schaden | 48,9 | 63,3 | 57,5 | 63,3 | 63,6 |
| Heiler stirbt vor dem ersten Tankbuster | 62,4 | 80,9 | 62,6 | 66,8 | 80,9 |
| Boss ohne Tankbuster | 135,8 | 135,8 | 183,3 | 185,9 (+37 %) | 185,9 |

**Bilanz im Kampf.** *Stärke:* Er lebt nach dem Tod des Heilers länger – im großen Pull übersteht er mehr, gegen einen
Boss mit Tankbustern gut ein Viertel länger, gegen einen ohne gut ein Drittel –, Zeit für eine Feder oder eine
Wiederbelebung. *Schwäche:* Stirbt der Heiler vor dem ersten Tankbuster eines Bosses und sagt kein BossModReborn-Modul ihn
voraus, findet der erste weniger bereit (66,8 statt 80,9 s, noch über heute). *Chance:* Mit laufendem Modul kennt die
Regel den nächsten Tankbuster; die Spalte „bekannt" zeigt, dass dann nichts verloren geht. *Bedrohung:* Mit „Heal and raise
Party NPCs" aus zählt der Heiler einer Begleitgruppe nicht als Mitglied – dann gilt die Regel trotz lebendem Heiler; das
kostet bereitliegende Abklingzeiten, keinen Schaden außer dem MP des Dunkelritters für The Blackest Night, die auch dann
nur fällt, wenn der Schaden sie brechen würde (A338). Allein unterwegs gilt sie ebenso, mit denselben Kosten.
*Überwiegt:* der Nutzen, in jedem gerechneten Fall gleich oder besser als heute.

**Ausgeschlossen.** *Nur ohne lebenden Wiederbeleber:* ob und wann er wiederbelebt, ist das Verhalten eines anderen
Spielers, nach seiner Regel nie ein tragender Grund; eine Feder in fremder Tasche ist unsichtbar; und bis ein
Wiederbelebter heilt, vergehen Wirken, Annehmen und Schwäche. *Trash und Boss getrennt behandeln:* seine Angabe, eine
Regel je Kampf; im Pull ist kein Tankbuster fällig, also greift dort nichts zurück. *Die Abwehr öffnen ohne Prüfung auf
den nächsten Tankbuster:* mit Tankbustern schlechter als nur die Reserve aufzuheben (72,5 statt 79,4 s). *Die Staffel
behalten:* im Pull 163 statt 133 Tote. *Eine erste Fassung, die die Pulls eines Dungeons als eine Kette mit
dauerhaft totem Heiler rechnete* und die Pull-Rückhaltung deshalb unverändert ließ, ist auf seinen Hinweis verworfen
(A361). *Flächenabwehr:* unverändert – das Strecken verteilt Abdeckung auf mehrere Treffer, was ohne Heiler ebenso gilt,
und „Skip mitigation for small area casts" misst den Treffer an der Gesundheit der Mitglieder (`HitDropsBelow`), mindert
also wieder, sobald die Gruppe ohne Heilung tiefer steht.
*Living Dead, Holmgang, Superbolide:* unverändert; Walking Dead trägt der Dunkelritter mit seinen eigenen Angriffen
(Abschnitt „Living Dead ist ein Zeitproblem").

**Grenzen.** Das Modell trägt Annahmen ohne Quelle (Selbstheilung pauschal, Bosswerte, Zeitpunkt des Heilertods); ein
Tankbuster in mehreren Teilen zählt als einer, wenn die Teile innerhalb eines GCD fallen. *Prüfgrad:* Modell mit
Selbsttest, statisch, Compile; im Spiel nicht beobachtet. Die Abwehr schreibt in `DefenseTrace.log`, warum sie offen
steht und wann der nächste Tankbuster fällig ist; der Plan vermerkt, wenn er ohne Heiler nichts zurückhält.

## Krieger: Urimpuls für sich oder Urflackern für andere

**Seine Vorgabe (29.09.2026), im Wortlaut:** „nascent flash dahingehend im vollständigen loop bewerten: ist der
tankbuster tödlich? braucht der tank nach dem tankbuster viel heilung? ist der tankbuster mit bestehenden
schilden, minderung und bestehenden hot oder sonstiger verfügbarer heilung so gut wegsteckbar, dass nascent flash
auf andere gecasted werden kann. auf wen soll nascent flash denn gecasted werden? auf heiler? heiler haben eine
höhere priorität als normale damagedealer. auch da dann prüfen: was ist beim ziel vorhanden? schilde, bestehender
hot, bestehende minderung. haben die ziele debuffs, hat der tank debuffs? bewertungstriage = muss auf jemanden
durch tod verzichtet werden und wer ist am ehestens entbehrlich?" Einordnung: Vorgabe mit Kriterien für die
vorgelegte Entscheidung (TODO, A191). Der Satz „Heiler haben eine höhere Priorität als normale Damagedealer"
bestätigt die Rollenordnung aus Konzept 07; dort gilt sie bei gleicher Gefährdung.

### Mechanik

Wirktexte, Job-Guide und Spieldaten, abgerufen 29.09.2026.

| | Bloodwhetting (82, auf sich) | Nascent Flash (76, auf ein Mitglied, nicht auf sich) |
|---|---|---|
| Minderung | −10 % für 8 s, dazu Stem the Flow −10 % für 4 s | dieselbe auf dem **Ziel** (Nascent Glint, Stem the Flow) |
| Barriere | Stem the Tide, 400 Potenz, 20 s | dieselbe auf dem **Ziel** |
| Heilung | 400 Potenz je Waffenfertigkeit, 8 s, auf den Krieger | 400 Potenz je Waffenfertigkeit auf den **Krieger** (Nascent Flash) **und** 100 % davon auf das Ziel |
| Abklingzeit | 25 s, gemeinsam (Abklingzeitgruppe 7, auch Raw Intuition) | |

**Daraus folgt die tragende Feststellung: Nascent Flash auf einen anderen kostet den Krieger keine Heilung.** Er
heilt sich damit genauso wie mit Bloodwhetting. Er verliert Minderung und Barriere: in den ersten 4 s rund 19 %
weniger Schaden (0,9 × 0,9: Minderungen multiplizieren sich, The Balance rechnet Bloodwhetting genauso, „100 x 0.9 x 0.9 = 81“), danach
bis 8 s 10 %, und 400 Potenz Barriere. Die zweite seiner Fragen, ob der Tank nach dem Tankbuster viel Heilung
braucht, trägt deshalb keinen Grund, Nascent Flash zurückzuhalten.

### Sachstand im Code

- **Defekt, behoben (A226): Nascent Flash ging nur an Unverwundbare.** Der Zielfilter las
  `!t.NoNeedHealingInvuln()`. Die Funktion ist wahr, solange **keine** Unverwundbarkeit liegt; die Negation ließ
  also nur Geschützte durch — einen Tank unter Holmgang, Superbolide, Hallowed Ground oder Living Dead, sonst
  niemanden. Seit Upstream c3fac720b heilte der Krieger damit praktisch nie ein anderes Mitglied; der in A191
  vorgelegte Konflikt am Tankbuster trat so gar nicht auf. Dieselbe Verwechslung ist die dritte ihrer Art im Baum
  (Heilzielwahl, Excogitation); `check_invuln_polarity.py` hält sie in der CI.
- Nascent Flash liegt im Heilpfad der Fähigkeiten (`HealSingleAbility`, Upstream c3fac720b). Dieser läuft im
  Dispatch vor der Einzelabwehr und vor dem allgemeinen Pfad. Ziel ist ein Mitglied unter
  `Nascent Flash Heal Threshold` (0,6, Vorausschau), nicht unverwundbar, sortiert nach Einstellung „Nascent Flash
  target priority": niedrigster **Prozentsatz der aktuellen Gesundheit** (ohne Vorausschau, ohne Barriere),
  Heiler zuerst oder nur Heiler.
- Bloodwhetting fällt für sich
  - in der Einzelabwehr (Tankbuster, Beschuss) mit „Use Bloodwhetting/Raw intuition on single enemies" oder
    bei mehr als zwei Gegnern in Reichweite, und nur, solange der Gegner den Krieger anvisiert;
  - im allgemeinen Pfad reaktiv unter „Bloodwhetting/Raw intuition heal threshold" (0,7).
  „single enemies" steht ab Werk **an** (A237, seine Regel für Voreinstellungen); bis dahin stand es aus, und vor
  einem Boss-Tankbuster fiel Bloodwhetting gar nicht, erst danach reaktiv. Mit der Einstellung aus gilt das weiter.

### Bewertung nach seinen Kriterien

**Ist der Tankbuster tödlich?** Für einen gewirkten Tankbuster, den die Tankbuster-Tabelle kennt (Konzept 13),
rechnet es der Plan (Abschnitt „Gemessener Tankbuster: das geringste Mittel"): Der Krieger braucht Urimpuls genau dann,
wenn der Plan ihn als gebrauchtes Glied führt (`NeedsBloodwhettingHimself` liest dann `TankbusterPlanNeeds`). Ohne
Messung – ein Marker, eine BossModReborn-Vorhersage, eine noch nie gemessene Aktion – kennt RSR keine Höhe:
BossModReborn meldet Art, Zeitpunkt und Getroffene (Konzept 07). **Dann gilt ein angekündigter Tankbuster auf den
Krieger als möglicherweise tödlich**: Er ist angekündigt, also wahrscheinlich, und seine Höhe ist unbekannt — nach
seiner Spielweise geht dann die Sicherheit vor.

**Ist er mit Bestehendem wegsteckbar?** Mit Messung beantwortet es der Plan. Ohne Höhe nur in einem Fall entscheidbar, und der ist vollständig: Liegt
eine Unverwundbarkeit (Holmgang, oder jede andere aus `NoNeedHealingStatus`) auf dem Krieger, während der
Tankbuster auf ihn gewirkt wird, kann er ihn nicht töten. Schilde, laufende Minderungen, HoTs und die Heiler senken
den Treffer oder füllen nach; ob das genügt, hängt an der Höhe, die fehlt.

**Wird Bloodwhetting für den Tankbuster überhaupt gewirkt?** Das ist die Frage, die der Code beantworten kann, und
sie entscheidet, ob eine Zurückhaltung etwas bewirkt. Fällt Bloodwhetting nicht vor dem Tankbuster (ab Werk bei
einem einzelnen Boss), hält eine Zurückhaltung Nascent Flash vom Mitglied fern, ohne dass der Krieger am Treffer
etwas davon hat; seine reaktive Nutzung danach ersetzt Nascent Flash bis auf Minderung und Barriere, und die Heilung
bekommt er ohnehin. Die Zurückhaltung für den Tankbuster gilt deshalb nur, wenn die Einzelabwehr Bloodwhetting
wirken würde — dieselbe Bedingung an derselben Stelle (`BloodwhettingForDefense`), nicht kopiert.

**Wann ein Tankbuster „auf ihn und vor der Abklingzeit" kommt:**
- ein gelisteter Tankbuster-Zauber auf ihn oder ein Tankbuster-Marker auf ihm (`IsHostileCastingTankBusterAtMe`,
  wenige Sekunden voraus);
- BossModReborn sagt einen Tankbuster innerhalb der Abklingzeit von Nascent Flash voraus (25 s, aus den
  Spieldaten). Wen er trifft, sagt BossModReborn nur, solange er der nächste vorhergesagte Treffer im
  Minderungsfenster ist (`BMRTankbusterHitsPlayer`). Weiter voraus gilt als Näherung: auf ihn, solange sein
  Ziel ihn anvisiert (Schluss, nicht belegt — Tankbuster auf den zweiten in der Feindseligkeit gibt es).
- In allen drei Fällen nur, solange sein Ziel ihn anvisiert: Das ist die eigene Prüfung von Raw
  Intuition/Bloodwhetting (`PlayerIsTargetOnSelf`). Ein Off-Tank mit Tankbuster-Marker bekommt Bloodwhetting aus
  der Einzelabwehr nicht, die Zurückhaltung hielte dort umsonst. Dass Bloodwhetting dann fehlt, ist eine
  Upstream-Bauform der Aktion, nicht dieser Regel; der andere Tank kann ihm dann mit seiner Hilfe beistehen
  (Abschnitt „Hilfe für das Ziel eines Tankbusters").
Ohne BossModReborn-Modul bleibt nur der sichtbare Zauber oder Marker; die Zurückhaltung greift dann nur Sekunden
vorher. Das ist eine Grenze, kein stiller Ausfall: Die Regel hält dann seltener, nie falsch.

**Auf wen?** Nascent Flash ist eine Heilung samt Minderung und Barriere. Für die Zielwahl der Heilung gilt seine
Vorgabe aus Konzept 07: **wer am stärksten gefährdet ist zu sterben; bei gleicher Gefährdung Heiler vor Tank vor
Schadensausteiler.** Genau das tut die Heilzielwahl des Baums (`FindHealTarget`): die kritische Klasse zuerst, nach
absoluten effektiven Punkten; mit „Choose the heal target by danger" die Klassen 2 und 3; Gleichstand nach Rolle.
Die drei vorhandenen Einstellungen tun es nicht: „Lowest HP party member" sortiert nach aktuellem Prozentsatz und
übergeht Barriere und Verlauf; „Healers first" zieht jeden Heiler unter der Schwelle einem sterbenden
Schadensausteiler vor, was die Rollenordnung über die Gefährdung stellt; „Healers only" lässt alle anderen aus.
**Deshalb eine vierte Einstellung: „By danger" — die Heilzielwahl.** Die drei bestehenden behalten ihre Bedeutung,
weil ihr Text bindet.

**Was beim Ziel vorhanden ist:**
- *Schilde* zählen in der kritischen Klasse (effektive Gesundheit). Wer hinter einer Barriere steht, fällt nicht in
  sie. Für die Heilschwelle zählen sie nicht, nach seiner Entscheidung A85: Gesundheit und Schild ersetzen einander
  nicht.
- *HoTs und Minderungen* wirken über den gemessenen Verlauf: Er ist netto aller Heilung und Minderung, die
  Vorausschau liest ihn (mit „Heal ahead of incoming damage" an).
- *Ein Nascent Glint eines anderen Kriegers* sperrt das Ziel (`TargetStatusProvide`).
- *Unverwundbare* sind ausgeschlossen (Filter), *Heilung wirkungslos* (`HealingIneffectiveStatus`, Mounted)
  ebenfalls — die Heilzielwahl schloss sie schon aus, die Sortierung nach Prozentsatz nicht.

**Debuffs:**
- *Beim Ziel:* Doom zählt als 1 % Gesundheit und steht damit vorn — Nascent Flash heilt nicht voll, aber Minderung
  und Heilung helfen, und die volle Heilung kommt aus dem Heilpfad der Heiler. `HpRecoveryDown` mindert nur die
  Heilung; Minderung und Barriere wirken voll, das Ziel bleibt Kandidat. Eine Verwundbarkeit erhöht den Eingang;
  der Verlauf zeigt ihn.
- *Beim Krieger:* Eine Verwundbarkeit macht den nächsten Tankbuster härter. Mit Messung führt die Tankbuster-Tabelle
  den Wert unter Verwundbarkeit eigens (Konzept 13); ohne Höhe ändert sie an der Regel nichts, die bereits vorsichtig
  ist. Heilstrafen (Scalebound, Shackled Healing) sperren den ganzen Heilpfad
  schon (`PlayerHealingPunished`).

### Die Triage: wer verzichtet, wenn beide es brauchen

Sein Bedarf an Bloodwhetting besteht, wenn **(a)** ein Tankbuster auf ihn vor der Abklingzeit kommt und Bloodwhetting
dafür gewirkt würde, oder **(b)** er selbst in der kritischen Klasse steht. Dann geht Nascent Flash nur an ein
Mitglied, das die Triage gegen ihn gewinnt, und das folgt seiner Rollenordnung nach Ersetzbarkeit (Konzept 07) und
dem Grundsatz „sicher vor möglich":

| Mitglied | gewinnt gegen seinen Bedarf? | Grund |
|---|---|---|
| nicht kritisch, gleich welche Rolle | nein | es stirbt nicht am nächsten Treffer; die Heiler decken es |
| **Heiler**, kritisch | **ja** | fällt der Heiler, fällt die Gruppe mit ihm (seine Begründung) |
| **Tank**, kritisch | ja, solange der Krieger selbst nicht kritisch ist; vor einem gemessenen Tankbuster nur, solange die Unverwundbarkeit des Kriegers übernehmen kann (D3) | gleiche Rolle; sein Tod ist sicher, der ungemessene Tankbuster möglich; der gemessene ist es nicht mehr |
| **Schadensausteiler**, kritisch | **nein** | am ehesten entbehrlich: wiederbelebbar, während ein toter Tank meist den Kampf kostet (Konzept 08) |

**Gegenposition:** Bloodwhetting ist am Tankbuster ein kleiner Anteil — rund 19 % in 4 s und eine Barriere — und oft
nicht der Unterschied zwischen Leben und Tod. Nascent Flash auf einen kritischen Schadensausteiler kann dagegen genau
diesen Unterschied machen. Für einen gemessenen Tankbuster entscheidet deshalb der Plan, ob Urimpuls gebraucht ist;
ohne Messung opfert die Regel im Zweifel einen Schadensausteiler für einen Treffer, den der Krieger auch ohne
Bloodwhetting überlebt hätte, und folgt damit seiner Ordnung.

### Grenzen

- Mit „single enemies" aus fällt Bloodwhetting vor dem Tankbuster eines einzelnen Bosses nicht; dann gibt es den
  Konflikt am Tankbuster nicht, und die Regel hält dort auch nicht (Bedingung (a)). Bedingung (b) besteht immer.
- Die Näherung „wen sein Ziel anvisiert, der bekommt den Tankbuster" kann irren; dann hält der Krieger
  Nascent Flash für einen Treffer, der den anderen Tank trifft — und gerade der könnte es brauchen. Der Fehler geht
  in die vorsichtige Richtung und endet, sobald der Tankbuster im Minderungsfenster steht und die Maske ihn richtig
  zuordnet. Die Triage kann einen Schadensausteiler kosten (Gegenposition oben).
- Ohne Tankbuster-Signal greift nur (b). Ohne Bloodwhetting in der
  Einzelabwehr greift nur (b) — richtig so, siehe oben. Mit „By danger" ändert sich die Wahl nur, wo kritische
  Klasse, Barriere oder Verlauf vom Prozentsatz abweichen; sonst wählt sie denselben.

### Gebaut

Umgesetzt mit A226.


- `WAR_Reborn.BloodwhettingForDefense`: die Bedingung der Einzelabwehr, gelesen von ihr und von der Zurückhaltung.
- Einstellung **„Keep Bloodwhetting for yourself when you need it"**, ab Werk **an** (seine Regel für Voreinstellungen,
  29.09.2026): hält Nascent Flash nach (a)/(b) und der Triage zurück.
- Vierte Zielwahl **„By danger"**, ab Werk gewählt (angehängt; Rotationseinstellungen speichern den Namen).
- Heilung-wirkungslos-Ausschluss in allen vier Zielwahlen.
- Der Zielfilter liest `CanBeHealed()` (bis Upstream 7.5.7.1 `NoNeedHealingInvuln()`) mit der richtigen Polarität (belegter Defekt, ohne Schalter).
- **Nascent Flash vor dem Tankbuster eines anderen** (A328): Option „Use Nascent Flash on another party member a
  tankbuster is aimed at", ab Werk an; in der Einzelabwehr nach Urimpuls für sich, mit der Triage oben (Abschnitt
  „Hilfe für das Ziel eines Tankbusters"). The Balance, Warrior Basic Guide, Abschnitt Makros (abgerufen 02.10.2026):
  „Nascent Flash goes on a friend. Most often on the co-tank of your eight-person party."

## Krieger: die übrige Abwehr

**Seine Fragen und Aufträge (01.10.2026):** „abtausch auf tankbuster durch boss bringt nichts, wird aber gecasted"
(Boss allein in der Arena, Krieger); „ist kampfrausch vor tankbuster nicht sinnvoll? warum nicht genutzt?"; „evtl.
auch bei gruppenpulls wall to wall sinnvoll. alle defskills warrior im vollen loop prüfen, bewerten, schauen, wie
bislang genutzt"; „in die bestehenden konzepte einarbeiten im vollen loop, kritisch alles bewerten". Einordnung:
Beobachtung mit Hinweis auf die Lage, Frage, Prüfauftrag.

**Maßstab:** seine Spielweise — Sicherheit vor Schaden, gewichtet mit der Wahrscheinlichkeit des Treffers. Ein
gemessener Tankbuster gehört dem Plan (Abschnitt „Gemessener Tankbuster: das geringste Mittel"): Sein Zurückhalten
lässt aus den Pfaden unten nur seine Glieder durch. Ohne Messung gilt ein angekündigter Tankbuster auf ihn als
wahrscheinlich und möglicherweise tödlich (Abschnitt „Krieger: Urimpuls für sich oder Urflackern für andere"). Für einen Pull misst die Wahrscheinlichkeit der gemessene Gesundheitsverlauf. Keine
der Abwehrfähigkeiten kostet Schaden; ihr Preis ist allein die Abklingzeit, also ob sie beim nächsten Bedarf fehlt.
Referenz für Rotationen: The Balance, Warrior Basic Guide, „Staying Alive" (abgerufen 01.10.2026).

### Die Fähigkeiten

Job-Guide deutsch und englisch, abgerufen 01.10.2026.

| Fähigkeit | Stufe | Abklingzeit | Wirkung |
|---|---|---|---|
| Schutzwall (Rampart) | 8 | 90 s | −20 % für 20 s, Heilung auf ihn +15 % |
| Tiefschlag (Low Blow), Zwischenruf (Interject) | 12, 18 | 25 s, 30 s | Betäubung, Unterbrechung |
| Reflexion (Reprisal) | 22 | 60 s | Gegner im Umkreis von 5 Yalm −10 % Schaden für 15 s |
| Kampfrausch (Thrill of Battle) | 30 | 90 s | maximale Gesundheit +20 % und aufgefüllt, Heilung auf ihn +20 %, 10 s |
| Abtausch (Arm's Length) | 32 | 120 s | Rückstoßschutz 6 s; wer ihn physisch trifft, bekommt Gemach +20 % für 15 s |
| Rachsucht (Vengeance) → Verdammnis (Damnation) | 38 → 92 | 120 s | −30 % → −40 % für 15 s; Verdammnis danach Regeneration (Status Primeval Impulse) |
| Holmgang | 42 | 240 s | Gesundheit fällt 10 s lang nicht unter 1 |
| Urinstinkt (Raw Intuition) → Urimpuls (Bloodwhetting) | 56 → 82 | 25 s, geteilt mit Urflackern | −10 % (6 s → 8 s), Heilung je Waffenfertigkeit; Urimpuls dazu −10 % für 4 s und eine Barriere |
| Äquilibrium (Equilibrium) | 58 | 60 s | Heilung 1200 Potenz und Regeneration |
| Abschütteln (Shake It Off) | 68 | 90 s | Gruppenbarriere 15 % der Maximalgesundheit; hebt Kampfrausch, Verdammnis und Urimpuls auf, +2 % je aufgehobenem Effekt |
| Urflackern (Nascent Flash) | 76 | 25 s, geteilt | Urimpuls-Wirkung auf ein Mitglied, Heilung auf ihn |

The Balance führt sie als Stapel für Tankbuster, von „Reprisal | Thrill | Rampart | Bloodwhetting" bis zum „Kitchen
Sink" aus allen fünf, mit Holmgang als letzter Stufe. Urimpuls heilt je Treffer, „very powerful in dungeons";
Äquilibrium ist „great when used with Thrill of Battle, Rampart or both". Abschütteln bemisst seine Barriere an der
durch Kampfrausch erhöhten Maximalgesundheit („Big Value"). Reflexion auf einen Tankbuster ist „very situational",
weil sie für den nächsten Raidwide fehlen kann.

### Wie RSR sie nutzt, und was davon richtig ist

Die Einzelabwehr des Kriegers (`WAR_Reborn.DefenseSingleAbility`) öffnet zweimal: für einen Tankbuster oder Zauber
auf ihn (Marker, gelisteter Zauber, Vorhersage innerhalb von „Tankbuster mitigation lead time",
ab Werk 3 s) und für einen Pull (`TankPullOnPlayer`: mindestens „Use tank mitigation when this many enemies target you" Gegner auf ihm in 3 Yalm, er wird
getroffen). Je Einwebeplatz fällt die erste passende Fähigkeit in dieser Reihenfolge:

0. Unter **Holmgang** und unter 30 % Gesundheit fällt nichts weiter (Upstream).
1. **Verdammnis/Rachsucht**, dann **Schutzwall** für einen von BossModReborn vorhergesagten Tankbuster, vor allen
   kurzen (A241). Schutzwall nur, wenn die große Minderung nicht läuft und nicht eben fiel (A243): je Tankbuster eine
   der beiden.
2. **Abtausch** auf einem Rudel gewöhnlicher Gegner für den Slow, Bosse nicht mitgezählt (A236). *Richtig:* Er mindert
   den Treffer nicht, der ihn auslöst. Die zentrale Rückfallstufe, die ihn auf jeden Tankbuster warf, ist geschlossen.
3. **Urimpuls/Urinstinkt** mit „single enemies" (ab Werk an, A237) oder mehr als zwei Gegnern. *Richtig:* Er ist der
   Kern jedes Stapels und heilt im Pull je Treffer.
4. **Kampfrausch** vor einem Tankbuster auf ihn innerhalb seiner Wirkdauer (A237).
5. **Verdammnis/Rachsucht** und **Schutzwall** ohne Vorhersage, über die gemeinsame Methode aller Tanks
   (`StaggeredBigMitigation`, Abschnitt „Die Abwehr im Pull"): im Pull erst, wenn der gemessene Verlauf sie braucht;
   sonst gestaffelt wie bisher – Verdammnis, wenn Schutzwall bereit ist oder vor mehr als 60 s fiel; Schutzwall, wenn
   Verdammnis vor mehr als 30 s fiel (unterhalb der Stufe von Rachsucht ohne Bedingung). Beide schließen einander über
   `StatusHelper.RampartStatus` aus.
6. **Reflexion**, nachgeführt oder sobald sie fehlt; am Tankbuster zurückgehalten, wenn er gedeckt ist und ein
   Raidwide nach ihrem Ende ansteht (`HoldReprisalForRaidwide`, A244).

Außerhalb der Einzelabwehr:
- **Kampfrausch** und **Äquilibrium** reaktiv unter je 0,6 Gesundheit (Vorausschau bis zur Landung); Kampfrausch steht
  davor, also wirkt Äquilibrium unter ihm wie von The Balance empfohlen.
- **Kampfrausch** zusätzlich, wenn die Gesundheit beim gemessenen Verlauf **innerhalb seiner Wirkdauer** unter diese
  Schwelle fiele (A239).
- **Urimpuls** reaktiv unter 0,7, solo immer.
- **Holmgang** als Notfall bei „Health of dying tank" (ab Werk 15 %).
- **Abschütteln** als Gruppenheilung und Flächenabwehr.
- **Urflackern**: Abschnitt „Krieger: Urimpuls für sich oder Urflackern für andere".
- **Tiefschlag** und **Zwischenruf**: zentral, nicht auf Bosse.

### Die Regeln je Fähigkeit, mit Grund

**1. Urimpuls hält Verdammnis, Schutzwall und Reflexion nicht zurück (A238).** Zwei Sperren taten dasselbe: Die
Einzelabwehr brach ab, solange Urimpuls oder Urinstinkt lief (Upstream, seit 141f9b27a), und Urimpuls stand in
`RampartStatus`, der Liste der *großen* Minderungen, die Schutzwall und Verdammnis als Doppelbelegungssperre tragen.
Im Kampf hieß das:
- Am **Pullbeginn**, wenn das Rudel vollzählig ist, kamen Verdammnis und Reflexion erst nach den acht Sekunden von
  Urimpuls.
- Bei **jedem Tankbuster**, für den Urimpuls fiel, kamen Verdammnis und Schutzwall gar nicht. Seit „single enemies" ab
  Werk an ist (A237), wäre das jeder Tankbuster eines einzelnen Bosses gewesen. Die Sperre hätte die Änderung von A237
  ins Gegenteil verkehrt.

Die Liste beschreibt sich selbst als „the big personal mitigations"; Urimpuls ist die kurze Abklingzeit des Kriegers wie
Heart of Corundum, Holy Sheltron und The Blackest Night bei den anderen, und von denen steht keine darin. Der Eintrag
widersprach also dem erklärten Zweck der Liste, und der Code folgt dem Kommentar. Beide Sperren sind entfernt;
Schutzwall und Verdammnis staffeln weiter gegeneinander. Andere Leser der Liste: `HasMajorMitigation` (nur Dunkelritter,
der nie Urimpuls trägt) und seit A244 `HoldReprisalForRaidwide` (alle Tanks, für den eigenen Charakter). Gegenposition: Ohne Sperre überlappen Urimpuls und Verdammnis. Das ist der Stapel „Damnation +
Bloodwhetting" der Referenz und kostet nichts, weil Urimpuls nach 25 s wieder bereit ist.

**2. Kampfrausch vor dem Treffer (A237, A239).**
- *Tankbuster:* Er fällt vor einem Tankbuster auf ihn, innerhalb seiner Wirkdauer (`TankbusterOnMeWithin`, zentral für
  jeden Tank; Dauer aus den Wirktexten). „Auf ihn" heißt: Marker oder gelisteter Tankbuster-Zauber auf ihm, oder eine
  Vorhersage, die ihn nennt oder, wo sie niemanden nennt, wen sein Ziel anvisiert (Schluss). Nicht unter
  Unverwundbarkeit. Option „Use Thrill of Battle before a tankbuster on you", ab Werk an. Er fällt auch auf einen
  Tankbuster, den Verdammnis schon nimmt. *Verworfen (A248):* ihn dort zurückzuhalten, damit er auf den schwächer
  gedeckten Tankbuster unter Schutzwall fällt, wie WrathCombo es tut („align with Rampart", Stand 25.09.2026,
  keine Spielquelle). Das hilft nur, wenn der nächste Tankbuster binnen seiner 90 s Abklingzeit kommt. Kommen sie
  im Abstand von zwei Minuten, nimmt Verdammnis jeden, und Kampfrausch fiele auf keinen. Den Abstand zum übernächsten
  Tankbuster liefert keine Quelle; BossModReborn sagt nur den nächsten an. The Balance führt „Damnation + Thrill"
  und „Rampart + Thrill + Bloodwhetting" als gleich starke Stapel („The 60s"); eine Zuordnung zu Schutzwall
  nennt sie nicht.
- *Pull (sein Hinweis):* Er fällt, sobald die Gesundheit beim gemessenen Verlauf innerhalb der zehn Sekunden seiner
  Wirkung unter „Thrill Of Battle Heal Threshold" fiele (`GetHealthRatioIn`, die selbstkorrigierte Zeit bis zum Tod).
  Damit liegen die 20 % Gesundheit und die verstärkte Heilung an, solange das Rudel vollzählig zuschlägt, und sie
  tragen die Heilung von Urimpuls und Äquilibrium. Halten die Heiler ihn stabil, ist der Verlauf nicht fallend, und er
  fällt nicht. Keine neue Zahl: Schwelle seine, Dauer aus den Wirktexten. Option „Use Thrill of Battle when your health
  will fall below its threshold within its duration", ab Werk an.
- *Gegenposition, geprüft:* Früher eingesetzt, fehlt er 90 s als Notheilung. Für den Notfall bleiben Äquilibrium (60 s),
  Urimpuls (25 s) und Holmgang. Und reaktiv füllt er dieselben 20 % erst auf, wenn der Treffer gelandet ist; im Pull
  verliert er nichts, wenn er früher kommt, weil der Verlauf den Bedarf schon belegt.
- *Grenze:* Der Verlauf ist linear fortgeschrieben. Ein Rudel, das stirbt, fällt langsamer als vorhergesagt; dann fiel
  Kampfrausch etwas zu früh, in die vorsichtige Richtung.

**3. Urimpuls vor dem Tankbuster eines einzelnen Bosses (A237).** „single enemies" stand ab Werk aus, also
fiel er gegen einen einzelnen Boss erst nach dem Treffer. Nach seiner Regel für Voreinstellungen (A227) steht er an.
*Restrisiko:* Die Einzelabwehr öffnet auch für einen ungelisteten Zauber eines Gegners auf ihn. Dann kann Urimpuls beim
eigentlichen Tankbuster noch abklingen (25 s).

**4. Kein Abtausch auf einen Boss-Tankbuster (A236).**

**5. Je vorhergesagtem Tankbuster ohne Messung eine große Minderung oder Schutzwall (A243).** Für einen gemessenen
ersetzt der Plan diese Staffelung. Beide Zweige
für einen vorhergesagten Tankbuster prüften nur, ob ihr eigener Status bis zum Treffer abläuft, und übergingen die
gegenseitige Sperre. Lagen beide bereit, fielen beide auf denselben Tankbuster.

*Beleg, sein Protokoll vom 01.10.2026, Restaurierter Löwe:* Tankbuster etwa alle 61 s.
- 20:13:43: Verdammnis und Schutzwall, Treffer 30 %.
- 20:14:45: keine von beiden bereit, Treffer 85 %.
- 20:16:02: beide, Treffer 31 %.
- 20:17:05: keine, Treffer 85 %.

Bei Neo Garula dasselbe: 27 % unter beiden, 49 s später 65 % ohne. Abwechselnd eingesetzt (Verdammnis 120 s, Schutzwall
90 s, Job-Guide) wären alle fünf Tankbuster beim Löwen gedeckt gewesen, je mit einer der beiden.

*Referenz:* The Balance empfiehlt je Tankbuster „one of Rampart or your 40 % cooldown, plus your short cooldown".

*Regel:* Schutzwall fällt für einen vorhergesagten Tankbuster nur, wenn die große Minderung ihn nicht nimmt — sie läuft
nicht und fiel nicht eben erst (`RampartTakesPredictedTankbuster`, zentral). Das gilt für alle vier Tanks. Die kurzen
Minderungen (Urimpuls, Kampfrausch, Heart of Corundum, Oblation, Sheltron, The Blackest Night) kommen wie bisher dazu.
Das Fenster für Schutzwall ist seine Dauer aus dem Wirktext, keine feste Zahl.

*Gegenpositionen:*
- *Ein Tankbuster braucht beide:* möglich bei einem Treffer weit über der Maximalgesundheit. Für diesen
  Fall bleiben Holmgang (bei „Health of dying tank") und die kurzen Minderungen. Ein Tankbuster gleich nach dem ersten
  bekäme mit beiden auf dem ersten gar nichts mehr, wie im Protokoll.
- *Ausgeliefert, nichts ändert sich:* Ohne Vorhersage greifen diese Zweige nicht. Dann staffelt die Zeitregel darunter,
  die beide schon nicht zugleich zulässt.

**Die Zeitregel ohne Vorhersage** (Schutzwall 30 s nach der großen, die große 60 s nach Schutzwall, Upstream
abe6132d3) bleibt unverändert. Durchgerechnet mit `tank_mitigation_stagger_model.py`: Direkt aneinander schützt einen
einzelnen Pull um 2–7 % besser. In einer Wall-to-Wall-Folge verschiebt es nur, welcher Pull ganz ohne beide bleibt; im
Mittel liegen beide Varianten höchstens 3 Prozentpunkte auseinander, mal die eine, mal die andere vorn. Am Boss ohne
Vorhersage ändert die Reihenfolge nichts. Ohne einen Unterschied im Kampf gibt es keinen Grund für einen Eingriff. Die
Zahlen 30 und 60 bleiben als offene feste Werte geführt.

**6. Abschütteln gegen die eigenen Status und gegen den Raidwide (A245).** Abschütteln hat zwei Wege, und beide
wirkten es als Erstes, ohne einen eigenen Status zu prüfen:
- *Einzelheilung* (`HealSingleAbility`): Sie öffnet, sobald irgendein Mitglied in der Vorausschau unter
  „HealthSingleAbility" fällt (ab Werk 0,7).
- *Flächenabwehr:* Sie öffnet bei einem erkannten Flächenzauber oder einem angesagten Raidwide.

Wirkung (Job-Guide): Barriere 15 % der Maximalgesundheit je Mitglied, 30 s; Heilung 300 Potenz und Regeneration;
Abklingzeit 90 s. Es hebt Kampfrausch, Verdammnis und Urimpuls auf, je +2 % Barriere. Die Barriere des Kriegers bemisst
sich an seiner Maximalgesundheit vor der Aufhebung (The Balance). The Balance führt Abschütteln unter den
Gruppenwerkzeugen, die auf raidweiten Schaden gehören.

| Fall | Gewinn | Kosten | Jetzt |
|---|---|---|---|
| Einzelheilung, Verdammnis/Rachsucht oder Urimpuls läuft | +2 % Barriere, Heilung für ein Mitglied unter 70 % | bis 15 s −40 % (−30 %) auf ihn, oder Urimpuls' Minderung und Heilung | **wartet**, bis sie enden |
| Einzelheilung, Raidwide nach Ende der Barriere und vor neuer Bereitschaft angesagt | Barriere für eine Einzellage | fehlt der Gruppe am Raidwide | **wartet** |
| Einzelheilung, Kampfrausch läuft | Barriere an der erhöhten Gesundheit, +2 % | Rest von Kampfrausch | frei |
| Raidwide, Verdammnis läuft, kein Tankbuster vor ihrem Ende | Barriere für alle | Rest von Verdammnis | frei |
| Raidwide, Verdammnis läuft **für einen Tankbuster, der vor ihrem Ende landet** | Barriere für alle | 40 % am Tankbuster | **wartet** bis nach dem Tankbuster |
| ein Mitglied in Gefährdungsklasse 1, oder der gemessene Raidwide brächte eines dorthin | — | — | jede Rückhaltung weicht (`HoldAreaDefense`), außer der nächsten Zeile |
| gemessener Tankbuster, der Plan braucht Rachsucht/Verdammnis, Urinstinkt/Urimpuls oder Kampfrausch | Barriere für alle | der Tank stirbt nach den Zahlen am Tankbuster | **wartet** auch bei Gefahr für die Gruppe, außer der Raidwide tötet nach den Zahlen einen Heiler oder mehr als ein Mitglied (D2; Abschnitt „Gemessener Tankbuster", K1) |

Das folgt seinem Kriterium: Wen bringt der Verzicht in ernste Bedrängnis? Am Raidwide verliert die Gruppe die Barriere,
er den Rest seiner Minderung, also bekommt die Gruppe sie. Steht ihm ein Tankbuster unter Verdammnis bevor, verlöre er
40 % an einem möglicherweise tödlichen Treffer; die Gruppe verliert eine Barriere, und gerät sie dadurch in Gefahr,
weicht die Rückhaltung.

*Gegenpositionen:*
- In der Einzelheilung wartet die Heilung für ein Mitglied unter 70 % bis zu 15 s. Ein Mitglied in
  Gefahr bekommt sie trotzdem (die Rückhaltung weicht); darüber heilen die Heiler.
- Ohne Modul gibt es keine Raidwide-Ansage, dann entfällt dieser Teil. Ohne
  sichtbaren Tankbuster entfällt der Teil am Raidwide. Die Statusprüfung wirkt immer.

**7. Reflexion am Tankbuster oder am Raidwide (A244).** Reflexion liegt auf dem Gegner und schützt jeden, den er
während ihrer 15 s trifft (sein Hinweis). Eine Wahl gibt es nur, wenn der Raidwide nach ihrem Ende und vor ihrer neuen
Bereitschaft landet. Dann wartet sie, wenn der Tank seinen Tankbuster schon mit eigener großer Minderung oder Schutzwall
nimmt. Fälle, Quellen und Grenzen in Konzept 08, „Gegner-Debuffs am Tankbuster und am Raidwide", weil die Regel dort für
alle Rollen sitzt.

**8. Die große Minderung zuerst (A241).** Sein Protokoll, 21:11:58: Marker auf ihm, dann
Urimpuls, Kampfrausch und Verdammnis erst 0,7 s vor dem Treffer. Das Fenster der Einzelabwehr öffnet bei einer
Vorhersage wenige Sekunden vorher, und die kleinen Minderungen standen vorn. Deshalb stehen die Zweige für einen
vorhergesagten Tankbuster bei allen vier Tanks am Anfang. Zusammen mit Befund 5 fällt dort je Tankbuster eine große
Minderung oder Schutzwall, dann die kurzen.

**9. Reflexion und Gruppenwerkzeug wechseln sich über eine Folge von Raidwides ab** (alle vier Tanks,
`TankAreaDefenseStretched`, A340). Nach Reflexion oder dem Gruppenwerkzeug des Jobs – Heiliger Quell und Waffengang,
Abschütteln, Dunkler Bote, Wackeres Herz – wartet das andere, solange das erste wirkt: eine Minderung für die Dauer
aus dem Wirktext, eine Barriere, bis sie verbraucht ist (fehlt ihr Status beim Tank einen GCD nach dem Druck). Beide
gehen zusammen, wenn ein Mitglied in Gefährdungsklasse 1 steht oder der angekündigte Treffer, wie gemessen, ein
ungeschütztes dorthin brächte – die Schranke der allgemeinen Streckung (Konzept 08, „Die Abwehrsperren"). Option „Spread
Reprisal and the party mitigation over a run of area attacks", ab Werk an.
- *Befund:* Alle vier Rotationen wirkten das Gruppenwerkzeug und im nächsten Einwebeplatz Reflexion, beide auf
  denselben Raidwide (sein Protokoll vom 01.10.2026; am Code bis A340 unverändert).
- *Referenz:* The Balance stapelt beide für einen großen Treffer und verteilt sie über mehrere. Die Größe misst die
  Flächentabelle; die Schranke liest sie, ohne die schon stehende Minderung abzuziehen, also bekommt ein großer Treffer
  weiter beide. Sein Grundsatz (Konzept 08, „Wozu die Aussetzbedingungen da sind"): die Dauer der Drosselung zählt,
  strecken statt verdoppeln, solange die Gruppe hält.
- *Im Kampf:* Zwei Raidwides kurz nacheinander bekommen je ein Werkzeug statt der erste beide und der zweite keines. Ein
  ungemessener Raidwide bekommt eines – beim ersten Auftreten weiß RSR nicht, dass er groß ist; danach misst ihn die
  Tabelle.
- *Grenzen:* Ob Reflexion auf dem Boss den Treffer eines unsichtbaren Helfers mindert, ist unbelegt; hält sie das
  Gruppenwerkzeug zurück und trifft der Helfer, fehlt dem Raidwide eine Minderung, außer die Schranke weicht. Eine
  Reflexion aus der Einzelabwehr (Tankbuster) hält das Gruppenwerkzeug ebenso, solange sie wirkt. Die Reflexion eines
  anderen Tanks zählt nicht: Die Streckung liest nur die eigenen Aktionen.

**Ohne eigene Regel:**
- *Äquilibrium* (reaktiv, unter Kampfrausch), *Holmgang* (Notfall bei „Health of dying tank", ab Werk 15 %; vorbeugend
  vor einem gemessenen Tankbuster, den nichts Geringeres trägt, über den Plan).
- *Tiefschlag/Zwischenruf* (zentral) und *Urflackern* (A226).

### Referenzen

Stand 02.10.2026.

| Frage | The Balance | WrathCombo (Stand 25.09.2026, keine Spielquelle) | xivanalysis | RSR |
|---|---|---|---|---|
| Große Minderung und Schutzwall auf einen Tankbuster | je Tankbuster eine von beiden plus kurze | Vengeance nicht innerhalb 20 s nach Rampart und umgekehrt („Prevent double big mits") | — | eine je Tankbuster (A243) |
| Urimpuls vor dem Tankbuster | Teil jedes Stapels | auf angekündigten Tankbuster | zählt nur die Nutzung | ja, „single enemies" an (A237) |
| Kampfrausch vor dem Tankbuster | in den Stapeln, mit Verdammnis wie mit Rampart („The 60s“) | nur ohne andere große oder mit Rampart, sonst Notfall | zählt nur die Nutzung | auf jeden Tankbuster, für den er bereit ist (A237); WrathCombos Ausrichtung verworfen (A248) |
| Reflexion am Tankbuster | Gruppenwerkzeuge auf Raidwides | im Bosskampf nur bei angesagtem Gruppenschaden | — | wartet, wenn der Tank gedeckt ist (A244) |
| Abschütteln | Barriere an erhöhter Gesundheit | im Bosskampf bei Gruppenschaden, nicht innerhalb 10 s nach Reflexion; außerhalb nur ohne Kampfrausch, Verdammnis, Vengeance, Urimpuls | zählt nur die Nutzung | wartet bei Verdammnis/Urimpuls (A245) und für gebrauchte Glieder des Plans |
| Reflexion und Abschütteln auf demselben Raidwide | mal stapeln, mal verteilen, je nach Größe | verteilt (je 10 s Sperre gegeneinander) | — | verteilt, solange das erste wirkt; beide bei Gefahr nach der gemessenen Größe (A340) |

*xivanalysis* bewertet Abwehr nur danach, ob sie genutzt wurde („find helpful times"), ohne Zeitpunktregel. *FFLogs*
liefert die Kampfdaten, keine Regeln; ein Report von ihm würde zeigen, welche Minderung auf welchem Treffer lag.

### Grenzen

- Die Näherung „wen sein Ziel anvisiert" kann bei Tankbustern auf den zweiten in der Feindseligkeit irren. Dann geht
  Kampfrausch umsonst, in die vorsichtige Richtung.
- Mehr Überlappung am Pullbeginn kann bei einem langen Pull hinten Deckung kosten. Gemindert durch die Staffelung von
  Schutzwall und Verdammnis, die bleibt, und durch Urimpuls alle 25 s.
- Ohne Modul, Marker oder gelisteten Zauber gibt es kein Tankbuster-Signal; Kampfrausch fällt dann nach dem Verlauf
  oder reaktiv.
- Fehlt im Fenster der Einzelabwehr (ab Werk 3 s) ein Einwebeplatz, fällt keine ihrer Minderungen. Das gilt für alle
  Tanks und ist bestehende Bauform.
- Hat er „single enemies" selbst auf aus gestellt, bleibt Urimpuls gegen den Boss reaktiv.
- Mit „Heal ahead of incoming damage" aus bleibt die Pull-Regel von Kampfrausch wirksam; sie liest den Verlauf
  unabhängig davon.

## Hilfe für das Ziel eines Tankbusters

**Sachstand (A328):** Vor einem Tankbuster auf ein anderes Mitglied – meist den anderen Tank – setzt jeder Tank seine
Hilfe für andere auf dieses Mitglied: Paladin Intervention, Krieger Urflackern, Dunkelritter The Blackest Night,
Revolverklinge Herz des Korunds. Die Heiler setzen ihre Einzelabwehr auf dasselbe Mitglied. Beides unterbleibt, wo
der gemessene Tankbuster keine Hilfe braucht (A373, Zielwahl).

**Im Kampf:** Trägt der andere Tank einen Tankbuster-Marker oder wirkt ein Gegner einen bekannten Tankbuster auf ihn,
bekommt er vor dem Einschlag deine Hilfe, und von den Heilern ihre Einzelabwehr statt dessen, den der Boss gerade
angreift. Bis A328 erreichte ihn keine dieser Hilfen gezielt: Herz des Korunds ging in der Einstellung ab Werk („Only
use on tankbuster targets prioritizing self") nie an ihn und an dich selbst nur mit Marker; Intervention ging an den,
den sein eigenes Ziel angreift, auch bei einem Pull, und nur an einen Tank mit Haltung; Urflackern und The Blackest
Night gingen nur nach Gesundheitsschwellen; Aquaveil, Exaltation und die übrige Einzelabwehr der Heiler gingen an den
Angegriffenen, auch wenn der Marker den anderen Tank nannte.

**Erkennung** (Stufe „alle", ohne Option, `DataCenter.IsTankbusterAimedAt`): ein bekannter Tankbuster zielt auf das
Mitglied –
- ein Tankbuster-Marker auf ihm, der nicht auf der Liste der Marker ohne Treffer steht (`TankbusterTargetIds`, je Bild
  neu im `TargetUpdater`, für jede Rolle, nur Mitglieder der Gruppe, als Ids, damit kein verschwundenes Objekt weiterlebt);
- ein Gegner, auch eine Hilfsfigur, wirkt auf ihn eine Aktion der Tankbuster-Liste oder eine, die die
  Tankbuster-Tabelle gemessen hat, im Fenster der Wirk-Erkennung (der vorletzte GCD vor dem Ende des Wirkens). Das Ziel
  eines gezielten Wirkens steht ab Wirkbeginn fest (Abschnitt „Mechanik" des Tankwechsels);
- für dich auch eine BossModReborn-Vorhersage, deren Maske dich nennt.

Nicht gezählt wird die Lesart „ein Gegner wirkt auf den, den er angreift", die die Einzelabwehr eines Tanks öffnet: Sie
trifft jeden ununterbrechbaren Trash-Zauber auf den Tank eines Pulls und ist für eine Hilfe, die MP oder Eid kostet,
falsch (C10, wie `TankbusterOnMe`). Eine gemessene Aktion ist ab ihrem zweiten Wirken bekannt.

**Flagge:** Ein Tankbuster-Marker auf einem Mitglied der Gruppe öffnet die Einzelabwehr (`IsCastingTankVfx`), beim Tank
auch auf dem anderen Tank. Bis A328 zählte beim Tank nur ein Marker auf ihm selbst (Upstream `a89f402d0`), eine
Bedingung aus der Zeit, als seine Einzelabwehr nur seine eigene Minderung war; seit A233 fragt, was nur ihn schützt, ob
der Treffer ihn erreicht (`SingleHitReachesPlayer`), und die Flagge trägt die Hilfe für andere und Reflexion. Ein
Marker außerhalb der Gruppe – auf einem anderen Allianztrupp – öffnet nichts, wie bei den Flächenmarkern (A220). Die
Liste entstand bis A328 als Nebenwirkung dieser Antwort und nur, wenn der Zustand sie abfragte (Upstream `e34a801fe`);
ein Schadensausteiler, ausgeschaltete Einzelabwehr oder eine vorher greifende Pull-Regel ließen sie veralten.

**Zielwahl** (zentral, `ActionTargetInfo`):
- Zieltyp `Tankbuster`: unter den Mitgliedern, die die Aktion nehmen kann (Reichweite, eigene Filter, Prüfung des
  Spiels), eines, auf das ein Tankbuster zielt; du zuerst, wie „prioritizing self" sagt.
- Zieltyp „angegriffen", den die Einzel- und die Flächenabwehr für jede freundliche Aktion ohne eigene Zielangabe
  setzen: Steht unter den Kandidaten eines, auf das ein Tankbuster zielt, nur diese; sonst wie bisher, wen sein eigenes
  Ziel angreift.
- Unter „Spend only the mitigation a measured tankbuster needs" fällt ein Mitglied heraus, das jeden auf es
  gerichteten Tankbuster unter Heiligem Boden oder Meteoritenfall nimmt (sein Vorschlag vom 04.10.2026). Bei einem
  Marker oder einer Vorhersage gilt das nicht, beide nennen hier keine Einschlagszeit.
- Unter derselben Option fällt ein anderes Mitglied heraus, dessen gemessener Tankbuster keine Hilfe braucht (A373):
  Er nimmt weniger, als ein großer Schild auffängt (The Blackest Night, 25 % laut Wirktext, `LargestStatedBarrierShare`),
  und lässt ihm mit dem, was beim Einschlag steht, mindestens die Schwelle, ab der deine Rotation ihn mit einem
  Einzelheil-GCD heilt („HealthSingleSpell", ab Werk 65 %). Das ist die Größenregel der Flächenabwehr
  (`AreaShareCallsForMitigation`): Minderung, wo sonst Heilung nötig wäre; ein großer Treffer gilt immer als groß.
  - Erkennung (`DataCenter.AimedTankbusterLeaves`, ohne Option und Schwelle): genau ein Tankbuster auf ihm – ein Wirken
    auf ihn, ein Tankbuster-Wirken auf kein anderes Mitglied neben einem Marker auf ihm, sonst neben dem Marker der dort
    gelernte Treffer (Abschnitt „Ein Marker ohne Wirken"). Gerechnet wie im Plan des Tanks: der höchste gemessene
    Treffer unter der Minderung, die beim Einschlag steht, gegen LP jetzt plus die Barrieren, die dann stehen.
  - Nichts gesagt, also Hilfe wie bisher: zwei Tankbuster auf ihm, ein ungemessener, ein Marker ohne Wirken und ohne
    gelernten Treffer, eine BossModReborn-Vorhersage auf ihm, ein Mitglied, das kein Tank ist (die Tabelle bemisst
    Treffer auf Tanks, A312).
  - Nicht für dich selbst – dein Plan entscheidet für dich –, nicht in der Einzelabwehr, die du befiehlst, und nicht
    in Heilpfad und Flächenabwehr (Aurora und Herz des Korunds heilen dort).
  - Gilt für jede Hilfe dieser Zieltypen: die Einzelabwehr der Heiler und die Hilfe der Tanks für andere.
- Was in der Einzelabwehr auf kein Mitglied zielt und über dich hinaus wirkt – Reflexion, die Gruppenminderung der
  Weisen, Samba, Troubadour, Taktiker –, unterbleibt, wenn der Treffer dich nicht erreicht, mindestens ein Tankbuster
  auf ein Mitglied zielt, jedes solche Mitglied nichts braucht (Unverwundbarkeit oder kleiner gemessener Treffer) und
  BossModReborn keinen Tankbuster vorhersagt (`TankbusterHelp.OnlyForMembersNeedingNothing`, Ablehnung
  „NoTankbusterHereNeedsIt", A374). Dieselbe Entscheidung wie für die Hilfe auf ein Ziel, an einer Stelle
  (`TankbusterHelp`), damit Zielwahl und Aktionsprüfung nicht auseinanderlaufen.
- Ein so herausgefallenes Mitglied kommt in der Einzelabwehr nicht über den Ersatz „wen sein Ziel angreift" zurück
  (A373). Eine BossModReborn-Vorhersage hält die Einzelabwehr des Heilers neben einem gedeckten Tankbuster offen (sie
  nennt kein Ziel, Review von A260), und über diesen Ersatz ging die Hilfe dann doch an den Tank, den der Boss angreift
  – Wasserschleier auf einen Tank unter Heiligem Boden. In seinen Protokollen trugen 96 von 118 Entscheidungen der
  Einzelabwehr mit Tankbuster-Quelle auch eine solche Vorhersage (gemessen 26.10.2026). Heilpfad, Flächenabwehr und
  die befohlene Einzelabwehr behalten den Ersatz.
- Zielt ein Tankbuster auf mehrere (ein doppelter Tankbuster auf beide Tanks), geht die Hilfe an das Ziel, auf dem die
  wenigsten schadensmindernden Wirkungen stehen – Minderung mit Wirktextwert oder Barriere, von wem auch immer
  (`LeastProtected`, A364); bei Gleichstand wie bisher. Bis dahin entschied nur die Gesundheit, und alle Hilfe ging an
  denselben: Im Protokoll vom 25.10.2026 setzte ein Weißmagier Göttlichen Segen und Wasserschleier auf einen Tank,
  dessen Treffer die Barriere ganz nahm, während der andere 74 % seiner LP nahm. Du selbst bleibst zuerst.

**Entscheidung** (Stufe „Tanks", `CustomRotation.HelpTankbusterTarget`): in der Einzelabwehr, nie auf dich – deine
eigene Abwehr und ihre Optionen decken dich –, je Tank unter seiner Option:

| Tank | Hilfe | Option | ab Werk | Reihenfolge und Triage |
|---|---|---|---|---|
| PLD | Intervention, nur auf einen Tank (Text: „CoTank"), ohne die Haltungsbedingung der Aktion | „Use Intervention on CoTank during tankbusters" | an | nach der eigenen Abwehr |
| WAR | Urflackern | „Use Nascent Flash on another party member a tankbuster is aimed at" | an | nach der eigenen Abwehr und Urimpuls; „Keep Bloodwhetting for yourself when you need it" mit Triage; „Only use Nascent Flash if Tank Stance is off" gilt |
| DRK | The Blackest Night | „Use The Blackest Night on another party member a tankbuster is aimed at" | an | nach der eigenen Abwehr und der Barriere für sich („When to use The Blackest Night on yourself") |
| GNB | Herz des Korunds, darunter Heart of Stone; Aurora im Heilpfad | „How to use Heart Of Stone/Heart Of Corundum", „How to use Aurora": „Only use on tankbuster targets prioritizing self" | gewählt | du zuerst; Zieltyp `Tankbuster` direkt |

„Nach der eigenen Abwehr" kostet keine Zeit, wenn der Tankbuster nur den anderen trifft: Was nur dich schützt, lehnt
die Einzelabwehr dann ab (A233), und die Hilfe fällt im selben Durchlauf; vor ihr kann nur Reflexion gehen. Trifft er
beide, kommt deine eigene Abwehr zuerst – Rangstufe 1, das Überleben des Tanks. Gegen den eigenen gemessenen Bedarf
entscheidet die Triage K2–K4 (Abschnitt „Andere Pfade, die dem Plan ein Glied nehmen").

**Warum so:**
- Seine Spielweise: Sicherheit der Gruppe vor Schaden, gewichtet mit der Wahrscheinlichkeit. Ein Marker oder ein
  bekannter Tankbuster ist angekündigt. Der Preis ist eine kurze Abklingzeit (Urflackern, Herz des Korunds), 50 Eid
  oder 3000 MP, die The Blackest Night beim Bruch als Dark Arts zurückgibt.
- Referenz, The Balance (abgerufen 18.10.2026; Krieger 02.10.2026): Krieger „Nascent Flash goes on a friend. Most often
  on the co-tank"; Dunkelritter Makros für The Blackest Night auf den Co-Tank (`<2>`) und „facing down a tankbuster in
  a high-end raid or trial, it's almost impossible for it not to break"; Paladin Co-Tank-Makros für Intervention. Der
  Basic Guide der Revolverklinge nennt keinen Einsatz am Co-Tank; Herz des Korunds auf Tankbuster-Ziele ist die
  Upstream-Einstellung.
- Universell zuerst: Erkennung und Zielwahl gelten für jede Rolle; Heiler und Tanks lesen dieselbe. Die Optionen
  stehen je Tank, weil jede Hilfe einen anderen Preis hat; Paladin und Revolverklinge hatten ihre schon.
- Ein Wirken zählt im Fenster der Wirk-Erkennung, nicht ab Wirkbeginn: Die Hilfen wirken 7 bis 8 s (Wirktexte,
  Tabelle „Die Abwehr der vier Tanks"), ein längeres Wirken überdauerte sie, wenn sie bei seinem Beginn fielen. Im
  Fenster liegt der Druck vor dem Snapshot, und es ist dasselbe, in dem das Wirken die Einzelabwehr öffnet.
- Voreinstellung nach seiner Regel (29.09.2026): an. Die Paladin-Option stand bis A328 aus; sie wirkte bis A201 nie
  und danach nicht nach ihrem Text.

**Ausgeschlossen:**
- Die Lesart „Wirken auf sein Ziel" als Erkennung (oben, C10).
- BossModReborn für andere: Nur Bit 0 der Maske ist belegt (Konzept 07). Ein nur von BossModReborn angekündigter
  Tankbuster auf den anderen Tank öffnet deine Einzelabwehr, nennt aber kein Ziel; dann bleibt die Hilfe aus.
- Cover: übernimmt den Treffer, der Paladin nähme den Tankbuster selbst; bleibt Notfall unter seiner Schwelle.
- Oblation auf den anderen Tank: nicht gebaut; The Blackest Night ist die größere Hilfe und refinanziert sich.
- Die Linie der Unverwundbarkeit („The HP% for tank to use invulnerability") als Maß, ob ein gemessener Tankbuster
  Hilfe braucht: Sie ließe fast jeden ohne – ein Treffer von 40 % lässt einen vollen Tank bei 60 %. Dieselbe Linie ist
  für die Flächenabwehr aus diesem Grund verworfen („a rule that cannot fire in its own domain", `DataCenter`). Für die
  eigene Abwehr des Tanks gilt sie weiter: Dort geht es um das geringste Mittel, mit dem er überlebt, und hinter der
  Linie steht seine Unverwundbarkeit; Hilfe von anderen ersetzt Heilung.
- Reflexion neben einer BossModReborn-Vorhersage: Sie geht weiter, auch wenn der sichtbare Tankbuster nur ein Mitglied
  trifft, das nichts braucht. Die Vorhersage nennt hier kein Ziel und kann einen zweiten Treffer auf den anderen Tank
  meinen (Grund der Entscheidung aus A261); anders als die Hilfe auf ein Ziel mindert Reflexion auch diesen. In seinen
  Protokollen trug das 96 von 118 Entscheidungen mit Tankbuster-Quelle – die Regel unten wirkt also vor allem in
  Kämpfen ohne Modul.

**Lagen:**
- *Zwei RSR-Tanks:* Der Getroffene schützt sich, der andere hilft ihm. Dieselbe Hilfe zweimal verhindert die
  Bereitstellungsprüfung (`TargetStatusProvide`: Nascent Glint, The Blackest Night, Intervention); bei Herz des
  Korunds prüft sie nur den Wirker.
- *Tankbuster auf beide:* Jeder Tank zuerst auf sich: die Revolverklinge über „prioritizing self", der Krieger behält
  Urimpuls nach der Triage, der Dunkelritter folgt erst seiner Einstellung für sich, der Paladin gibt Intervention erst
  nach Holy Sheltron und seinen großen Minderungen.
- *Allein, Vierer-Instanz, Allianzraid:* kein anderer Tank in der Gruppe. Hilfe nur, wenn ein Tankbuster ein anderes
  Mitglied trifft (Tank tot, Marker auf einem anderen); der Paladin dann nicht (Text: CoTank).
- *Stufensynchron:* jede Hilfe erst ab ihrer Stufe (`EnoughLevel`), Herz des Korunds darunter als Heart of Stone.
- *BossModReborn ohne Modul:* unverändert, Marker und Wirken tragen.
- *Tankbuster ohne Marker, auf den Wirker selbst gewirkt oder nie gemessen:* nicht erkannt, keine Hilfe; eine
  gemessene Aktion, die auf das Mitglied gewirkt wird, ab ihrem zweiten Wirken.
- *Kleiner Tankbuster beim ersten Auftreten:* Hilfe wie bisher; die Messung entsteht erst mit dem Treffer. So lag der
  Fall aus seinem Protokoll vom 25.10.2026: Doppeleinschlag war vor der Entscheidung nie gemessen und wurde mit dem
  Treffer gespeichert (20 % ungemindert), samt dem Treffer nach dem Marker. Beim nächsten Mal trägt beides.
- *Ein Tankbuster aus mehreren Treffern derselben Aktion:* Die Tabelle führt den höchsten Einzeltreffer, die Regel
  rechnet einen. In seinen 74 Tankbuster-Messungen bis 25.10.2026 folgte keine zweite derselben Aktion binnen 5 s;
  belegt ist der Fall also nicht, ausgeschlossen auch nicht. Die Grenze trifft den Plan des Tanks ebenso.

**Messmittel:** `DefenseTrace.log` schreibt „single defence -> Intervention … on <Mitglied>" (Urflackern, The Blackest
Night, Herz des Korunds; das Ziel seit A335, wenn es nicht der Spieler ist) mit den Quellen der Abwehr – Marker auf wem,
Wirken auf wen – und danach die „used"-Zeile; eine Hilfe, die für einen gemessenen kleinen Tankbuster ausbleibt, einmal
je Treffer „tankbuster help held: … takes …% of max HP and leaves …%", eine zurückgehaltene Reflexion als Ablehnung
„NoTankbusterHereNeedsIt" in der nächsten Zeile der Einzelabwehr. Die Entscheidung fällt aus Marker und Wirken;
wo sie die Tabelle liest, steuert die Tabelle nach – jeder höhere Treffer hebt ihren Wert, und die nächste Hilfe geht
wieder.

**Prüfgrad:** statisch, Prüfskripte, Compile über die CI; im Spiel nicht beobachtet.

## Tankwechsel nach einem Tankbuster: Geteiltes Leid auf den anderen Tank

**Sein Vorschlag (04.10.2026), als Option geprüft:** „bei inhalten mit mehr als einem tank (also keine instanzen
mit wall to wall), wenn gerade ein tankbuster auf dich erfolgt ist und du danach kritische gesundheit hat, bzw. einen
debuff, der einen zweiten tankbuster auf dich tödlich enden läßt (also auch solche spezialeffekte wie holmgang,
totenerweckung etc. auf cooldown sind), dann geteiltes leid auf den tank casten, welcher am wenigsten aggro hat bzw.
die höchste gesundheit."

**Seine Angaben dazu (04.10.2026):**
- *Präzisierung:* „wenn unverwundbarkeit bereit ist, dann braucht aggro nicht gewechselt werden."
- *Hinweis:* „es kann ja umgekehrt sein, dass der andere tank sterben könnte, weil seine unverwundbarkeit nicht bereit
  ist, und er ebenfalls geteiltes leid auf mich gewirkt hat."
- *Vorgabe:* „weiterhin sollte erst dann mit provozieren wieder aggro aufgebaut werden, wenn debuff und kritischer
  zustand abgelaufen sind."
- *Präzisierung dazu:* „am besten, wenn auch unverwundbarkeit wieder bereitsteht."
- *Präzisierung (als Frage gestellt, geprüft und übernommen):* „müsste das zurückprovozieren nicht erst dann
  erfolgen, wenn auch der andere tank einen tankbuster bekommen hat?"
- *Hinweis (17.10.2026, zur Vorlage „Tankwechsel von der anderen Seite"):* „der andere tank kann ebenfalls aggro
  abgeben und auf mich übertragen." Mechanik belegt (Geteiltes Leid, unten): Sein Geteiltes Leid gibt dir 25 % seiner
  Feindseligkeit und bewegt den Gegner nur, wenn du schon über der Hälfte seiner liegst. Auf deiner Seite war es
  bedacht (kein Zurückgeben an einen Tank in Gefahr; seine Geste steht nur im Protokoll), in der Vorlage für die andere
  Seite nicht; dort jetzt eingearbeitet (`TODO.md`).
- *Hinweis (17.10.2026):* „aktives agieren des anderen tanks wäre eigentlich ein hinweis darauf, dass er den
  tankbuster nicht nehmen will, weil z.b. sein invul noch nicht wieder da ist." Seine Geste ist damit das Zeichen für
  einen gewollten Wechsel; eingearbeitet in die Vorlage (Variante D).
- *Frage (17.10.2026), geprüft:* „ist es bei beginnendem wirken eines tankbusters nicht bereits zu spät, das ziel zu
  wechseln?" Ja, für das gezielte Wirken und den Marker (unten, Mechanik „Wann eine Herausforderung den Treffer noch
  umlenkt"). Seine eigene Angabe vom 15.08.2026 (Archiv, B2b) sagte dasselbe: ein angekündigter Tankbuster ist nicht
  mehr umlenkbar, bei mehreren Einschlägen bleibt zwischen ihnen rund 1 s.
- *Frage (17.10.2026):* „wer sagt, dass meine haltung aus ist?" Niemand – die Vorlage hatte es ungekennzeichnet
  angenommen. Die Regel liest die Haltung (`HasTankStance`), statt sie anzunehmen.

**Gebaut (A255–A257), Option „Shirk the co-tank after a tankbuster that leaves you in danger", ab Werk an:**
Erkennung in `TankSwapWatch`, Entscheidung in `CustomRotation.TankSwapAbility`, für alle Tanks an einer Stelle (Stufe
Tanks; Geteiltes Leid ist eine Rollenaktion, für keinen Tank-Job gilt etwas anderes). Geteiltes Leid geht auf den
anderen Tank, wenn alles zutrifft:

1. **Ein Tankbuster hat dich getroffen.** Tankbuster heißt: die Aktion steht in der Tankbuster-Liste, oder ein
   Tankbuster-Marker stand auf dir, der nicht auf der gelernten Liste der Marker ohne Treffer steht. Gemessen werden
   der Schadensanteil an deinen maximalen LP und die Status, die genau dieser Treffer auf dich legt (Effektsatz).
2. **Ein zweiter Treffer wäre tödlich.** Entweder trägst du noch eine Verwundbarkeit, die der Tankbuster gelegt hat
   (`VulnerabilityUp`, `PhysicalVulnerabilityUp`, `MagicVulnerabilityUp` – erkannt an den Bezeichnern, die der
   Generator aus dem englischen Statusblatt bildet, also unabhängig von der Client-Sprache). Oder LP und Schild zusammen liegen nicht über dem
   härtesten Anteil, der für diese Aktion in diesem Kampf auf dir gemessen wurde, oder der Vorhersage der
   Tankbuster-Tabelle für dich unter der jetzt stehenden Minderung, je nachdem, was höher ist (Konzept 13, „Die
   Tankbuster-Tabelle"; „eine Wiederholung bringt dich um").
   Das zweite Kriterium erlischt von selbst, sobald du über diese Linie geheilt bist.
3. **Deine Unverwundbarkeit ist nicht verfügbar.** Verfügbar heißt hier: aktiviert, erlernt, abgeklungen und mit
   einer Schwelle über 0 (`HealthForDyingTanks`). Unter einer laufenden Unverwundbarkeit, Living Dead oder Walking
   Dead geschieht nichts.
4. **Der andere Tank behält den Gegner danach.** Das ist gemessen, nicht angenommen; siehe Mechanik.
5. **Der andere Tank ist nicht selbst in Gefahr** (sein Hinweis). Er trägt keine Verwundbarkeit, und seine LP mit
   Schild liegen über dem, was dieser Tankbuster ihn erwartbar kostet: dem härtesten gemessenen Anteil oder der
   Vorhersage der Tankbuster-Tabelle unter seiner jetzt stehenden Minderung, gerechnet auf seine maximalen LP. Ob
   *seine* Unverwundbarkeit bereit ist, kann RSR nicht lesen: Die Abklingzeiten anderer Spieler liegen nicht vor.
   Gemessen wird deshalb, ob ein Treffer ihn umbrächte, nicht, ob er sich retten könnte. Hat er dir den Gegner mit
   Geteiltem Leid gegeben, weil er in Gefahr ist, zeigt genau diese Messung das, und du gibst ihn nicht zurück.
   Seine Geste selbst schreibt das Protokoll nur mit („… shirked to you").

**Zurückholen (seine Vorgabe und Präzisierungen):** Hat dein Geteiltes Leid den Gegner bewegt, provoziert RSR ihn
zurück, sobald
- der Tank, der ihn jetzt hält, den nächsten Tankbuster dieses Gegners genommen hat,
- die Verwundbarkeit abgelaufen ist und LP mit Schild über dem liegen, was eine Wiederholung erwartbar kostet
  (wie oben), und
- deine Unverwundbarkeit wieder bereit ist. Wo RSR sie gar nicht einsetzen würde (auf dieser Stufe nicht erlernt,
  abgeschaltet, Schwelle 0), wartet es darauf nicht.

Warum erst nach seinem Tankbuster: Ein Wechsel folgt dem Takt der Tankbuster. Früher zurückgeholt, nähmst du den
nächsten selbst, und die Abklingzeiten, die der Co-Tank dafür bereithält, blieben ungenutzt; der Gegner drehte
einmal mehr als nötig. Danach wechseln sich die Treffer ab, und trägt er nun selbst eine Verwundbarkeit, ist das
genau der Moment, in dem er dich braucht. Kommt lange kein Tankbuster, behält er den Gegner. Das ist ungefährlich,
denn er ist ein Tank, und fällt er unter die Sterbe-Schwelle, nimmt die automatische Herausforderung ihm den Gegner
ab, sofern du nicht selbst in Gefahr bist. Trifft sein Tankbuster, solange dein Debuff noch läuft, wartet das
Zurückholen auf deine übrigen Bedingungen.

Greift der Gegner dich schon wieder an, ist nichts zurückzuholen. Zurückgeholt wird nach jedem Wechsel, bei dem
dein Geteiltes Leid ausgeführt wurde und der Gegner danach den Empfänger angreift – auch wenn der Co-Tank vorher
provoziert hatte. Hat er ihn ohne dein Geteiltes Leid übernommen, holt RSR ihn nicht zurück: Das war ein geplanter
Wechsel.

**Kein Zurückholen, solange du in Gefahr bist:** Die automatische Herausforderung nimmt einem Co-Tank den Gegner ab,
der unter `HealthForDyingTanks` fällt (`ObjectHelper.CanProvoke`, Option „Take the enemy from a co-tank in danger").
Solange du selbst nach einem Tankbuster in Gefahr
bist, tut sie das nicht mehr. Ohne diese Sperre hätte sie dir den Gegner zurückgeholt, während dein Debuff noch
läuft – genau das, was seine Vorgabe ausschließt. Diese Sperre gilt unabhängig von der Option, weil sie eine
bestehende Regel an seine Vorgabe bindet.

**Die Zielwahl:** unter den anderen Tanks der Gruppe die, die lebend, anvisierbar, in Reichweite sind, selbst keine
Verwundbarkeit tragen und den Gegner nach der Übertragung behalten würden. Davon der, den die wenigsten Gegner
angreifen, bei Gleichstand der mit den meisten LP. „Am wenigsten Aggro" lese ich als „am wenigsten belastet" (meine
Ableitung, nicht seine Regel): Wörtlich – die geringste Feindseligkeit auf diesem Gegner – wäre es genau der Tank, bei
dem Geteiltes Leid am wenigsten bewirkt. Die Frage stellt sich nur bei drei und mehr Tanks in einer Gruppe; in Achter-
Inhalten gibt es genau einen anderen.

### Die andere Seite: den Gegner einem Co-Tank in Gefahr abnehmen

**Seine Entscheidung (17.10.2026):** „die kombination im loop genau prüfen und umsetzen" – B, C und D der Vorlage; ab
Werk an nach seiner Regel für Voreinstellungen. Gebaut mit A321 unter der Option **„Take the enemy from a co-tank in
danger"** (bis A321 „Provoke the enemy off a co-tank at the invulnerability HP", nur die Rettung unter
`HealthForDyingTanks`). Erkennung in `TankSwapWatch` (Stufe „alle", ohne Option), Entscheidung in
`ObjectHelper.CanProvoke` (Stufe „Tanks"), gedrückt über die Herausforderung des Baums.

RSR nimmt einem anderen Tank der Gruppe den Gegner, der ihn angreift, wenn
1. **seine LP mit Schild auf oder unter `HealthForDyingTanks`** liegen (die Rettung seit `4e62dc3a5`);
2. **(B) BossModReborn den nächsten Tankbuster ankündigt** und er ihn noch mit Verwundbarkeit nähme (Restzeit der
   längsten Verwundbarkeit über der angekündigten Zeit) oder ihn nach seinem LP-Verlauf nicht überlebte: LP zur
   angekündigten Zeit (`GetHealthRatioIn`, der korrigierte Verlauf) mit Schild nicht über dem, was die
   Tankbuster-Tabelle für den Tankbuster erwartet, mit dem dieser Gegner zuletzt traf. Sofort, sobald das gilt – nicht
   erst zum Wirken (Mechanik, „Wann eine Herausforderung den Treffer noch umlenkt");
3. **(D) er Geteiltes Leid auf dich gewirkt hat** und der Gegner ihn weiter angreift – sein Hinweis: Wer abgibt, will
   den nächsten Treffer nicht nehmen. Die Bitte gilt, bis der nächste Tankbuster jemanden trifft oder ihn kein Gegner
   mehr angreift.

**(C) Die Haltung zuerst:** Der Pfad der Herausforderung schaltet sie vorher ein, wenn sie aus ist
(`CustomRotation_Ability`, Herausforderung); das galt schon für jede automatische Herausforderung. Ob sie an ist,
liest RSR (`HasTankStance`).

**Nicht** – für B und D: solange du selbst nach einem Tankbuster in Gefahr bist oder eine Verwundbarkeit trägst (bei B:
noch zur angekündigten Zeit), solange er unverwundbar ist (bei B: über die angekündigte Zeit, `ImperviousThrough`; bei
D: wie die Rettung, `CanBeHealed`), und wenn mehrere Gegner ihn angreifen, ohne dass dein Ziel darunter ist
(BossModReborn nennt keinen Wirker). Zusätzlich nur für B: ohne Ankündigung; wenn du den angekündigten Treffer selbst
nicht überlebtest; wenn die Ansage dich als Ziel nennt (Maske des ersten Eintrags); wenn der Gegner schon irgendein Wirken auf ihn
richtet – ab Wirkbeginn, nicht erst im letzten GCD wie die Tankbuster-Erkennung – oder ein Tankbuster-Marker auf ihm
steht (Ziel fest); und für einen Gegner, dessen Tankbuster ihn nach einer
Übernahme trotzdem traf. D hält bei Wirken, Marker und dieser Lernliste nicht – es ist seine Bitte, keine Lesart.

**Warum so:** Der gefährliche Treffer ist der nächste; umlenken lässt er sich nur vor seinem Wirkbeginn oder Marker.
Die Wahrscheinlichkeit misst B an der Ankündigung, D an seiner Entscheidung (seine Regel „Sicherheit … gewichtet mit
der Wahrscheinlichkeit"). Die Verwundbarkeit allein (Variante A) sagt nicht, ob ein Treffer kommt, und blieb verworfen.

**Selbst nachgesteuert:** Greift der Gegner nach der Herausforderung dich an, ist die Übernahme offen; der nächste
Tankbuster dieses Gegners entscheidet. Trifft er dich: gehalten. Trifft er weiter ihn, war sein Ziel anders festgelegt
(Marker, feste Zuweisung) – für diesen Gegner gibt es in diesem Kampf keine Übernahme mehr auf eine Ankündigung. Springt
der Gegner vorher zu ihm zurück (seine Haltung wog schwerer), zählt das nicht gegen den Gegner; eine weiter bestehende
Gefahr öffnet eine neue Übernahme.

### Mechanik

Belege abgerufen 04.10.2026.

- **Geteiltes Leid** (Shirk, #7537): „Du überträgst 25 % deiner Feindseligkeit auf das ausgewählte Gruppenmitglied",
  St. 48, 120 s, 25 y (Job-Guide deutsch und englisch). Nicht auf dich selbst, nicht auf Allianzmitglieder (xivapi:
  `CanTargetSelf`, `CanTargetAlliance` falsch). In Allianzraids ist der andere Tank also nie erreichbar.
- **Herausforderung** (Provoke, #7533) setzt den Wirker an die Spitze der Feindseligkeitsliste – das Werkzeug der
  *anderen* Seite eines Tankwechsels.
- **Wann eine Herausforderung den Treffer noch umlenkt** (BossModReborn `be8ff77`, 17.10.2026, Gemeinschaftsquelle;
  dazu seine Angabe B2b): Ein gezieltes Wirken trifft das Ziel, das es beim Wirkbeginn hatte – `SingleTargetCast`
  sagt den Treffer auf `CastInfo.TargetID` voraus; nur ein Wirken auf den Wirker selbst rechnet es dem aktuellen Ziel
  des Gegners zu („assume self-targeted casts actually hit main target", Annahme des Autors). Ein Marker legt das Ziel
  fest (Bausteine mit Marker sagen den Treffer auf dem markierten Mitglied voraus). Bei mehreren Einschlägen nimmt den
  ersten das beim Beginn festgelegte Ziel, jeden weiteren, wen der Gegner dann anvisiert (`TankSwap`: „target of the
  first hit is locked when mechanic starts, then subsequent targets are selected based on who the boss targets";
  Hinweis „Provoke!" zwischen den Einschlägen). Eine Herausforderung wirkt also auf den nächsten Treffer nur, wenn sie
  vor dessen Wirkbeginn oder Marker liegt, oder zwischen den Einschlägen eines mehrfachen.
- **Wann Geteiltes Leid allein den Gegner bewegt:** Danach hältst du (1 − s) deiner Feindseligkeit, der Empfänger
  gewinnt s davon. Er wird zum Ziel, wenn er danach vor dir und vor allen anderen liegt; von dir aus gerechnet
  braucht er vorher mehr als 1 − 2s, bei s = 25 % also mehr als die Hälfte deiner Feindseligkeit. s kommt aus dem
  Wirktext über den Generator (`DefensiveValues.EnmityTransferOf`), nicht aus dem Code.
- **Die Feindseligkeit der Gruppe** auf dein aktuelles Ziel liest das Spiel selbst (`UIState.Hate`, je Mitglied 0 bis
  100 relativ zur Spitze – so in FFXIVClientStructs dokumentiert, Gemeinschaftsquelle, und so liest sie WrathCombo).
  Ist dein Ziel nicht die Quelle des Tankbusters, ist die Zahl nicht lesbar, und die Regel hält.
- **The Balance** führt den Tankwechsel als letzte Stufe nach der Unverwundbarkeit („It Still Kills Me: Holmgang
  (invulnerability), Tank Swap (mechanical requirement)", Krieger-Leitfaden) und ein Makro für Geteiltes Leid auf
  den Co-Tank. WrathCombo kennt Geteiltes Leid nur als Zielumlenkung beim manuellen Drücken, keine eigene Auslösung.

### Wo die Umsetzung von seinem Wortlaut abweicht

**Verengung, mit Grund:** Liegt der andere Tank bei höchstens der Hälfte deiner Feindseligkeit, wirkt die Regel
nicht, obwohl seine Bedingungen erfüllt sind. Geteiltes Leid würde dort nichts daran ändern, wer den nächsten Treffer
nimmt. Es kostete aber die Aktion für 120 s und fehlte beim geplanten Wechsel, wenn der Co-Tank provoziert und du
mit Geteiltem Leid nachlegen sollst. Hat der Co-Tank schon provoziert, liegt er an der Spitze; dann geht Geteiltes
Leid sofort und festigt den Wechsel, wie es die Referenz vorsieht.

### Wechselwirkungen und Lagen

- **Zwei RSR-Tanks:** Nur der Getroffene wirkt. Der Empfänger gibt dir den Gegner nicht zurück, solange du eine
  Verwundbarkeit trägst oder eine Wiederholung nicht überlebst – ein Tank in Gefahr ist kein Empfänger. Sind beide
  in Gefahr, wirkt keiner, und keiner provoziert.
- **Haltung:** RSR schaltet die Haltung nur ein, wenn kein anderer lebender Tank der eigenen Gruppe sie trägt
  (`StateUpdater.AnyOtherPartyTankWithStance`). Ein RSR-Co-Tank liegt daher meist weit unter der Hälfte, und die
  Regel hält. Das ist richtig: Ohne Haltung behielte er den Gegner nicht. Vor einer Herausforderung schaltet RSR deine
  Haltung ein, wenn sie aus ist; zwei Haltungen stehen dann, bis der Co-Tank seine ablegt. Behält er sie und holt den
  Gegner mit seinem Schaden zurück, steht „take-over undone" im Protokoll. Stirbt der Haupttank, legt RSR die Haltung
  an. Gezählt wird so erst seit Upstream 7.5.7.5: Vorher las die Prüfung die Allianzliste, die die eigene Gruppe nie
  enthält – in einer Gruppe mit zwei Tanks legten beide die Haltung an, und im Allianzraid hielt die Haltung eines
  Tanks aus einer anderen Gruppe deine zurück, sobald `RaiseType` die Allianz einschloss (A332).
- **Automatische Herausforderung** (`ObjectHelper.CanProvoke`): Gegner auf Nicht-Tanks nach „Auto provoke when there
  is another tank in party" (bei zwei Tanks in der eigenen Gruppe nur mit ihr, ab Werk an), und – unter eigener Option
  „Take the enemy from a co-tank in danger", ab Werk an (A318, A321) – der Gegner eines Co-Tanks in Gefahr (oben, „Die
  andere Seite"). Mit der ersten Option aus nimmt RSR in einer Gruppe mit zwei Tanks keinem Nicht-Tank den Gegner ab;
  das überlässt du dann dem anderen Tank, so sagt es ihr Text. Wirksam ist das erst seit Upstream 7.5.7.5 – vorher zählte
  dieselbe Allianzliste, fand den zweiten Tank nie und provozierte trotzdem (A332). Eine Rettung des Nicht-Tanks gegen
  die ausgeschaltete Option ist nicht gebaut: Sie fiele genau in den Fall, den ihr Text ausschließt. Mit „Provoke
  anything" gilt die Übernahme ebenso. Sie ruht, solange du selbst in Gefahr bist (oben, „Kein Zurückholen").
- **Zwei RSR-Tanks, die andere Seite:** Er wirkt Geteiltes Leid auf dich (seine Regel), du provozierst (B oder D) –
  beide schieben den Gegner zu dir. Zurück holt er ihn nach seiner eigenen Regel, weil sein Geteiltes Leid ihn bewegt
  hat; nach deiner Herausforderung allein holst du ihn nicht zurück, du gibst ihn nach deinem nächsten gefährlichen
  Treffer mit Geteiltem Leid ab.
- **Allein, Vierer-Instanz, Allianzraid:** Es gibt keinen anderen Tank in der Gruppe, die Regel ist still.
- **BossMod ohne Modul:** Dein Geteiltes Leid liest keine Vorhersage; Erkennung über Liste, Marker und Effektsatz. Die
  Übernahme nach B braucht die Ankündigung und ruht ohne Modul; die Rettung und D gelten weiter.
- **Stufensynchron:** Geteiltes Leid erst ab St. 48 (`EnoughLevel`), darunter still.

### Begründung und Grenzen

- Bei geplanten Wechseln festigt die Regel nur, was ohnehin geschieht. Den Ausschlag gibt der ungeplante Fall: ein
  Tankbuster, für den die Abklingzeiten fehlen. Seine Kämpfe vom 01.10.2026 zeigen „Schramme" zweimal mit 85 %, beide
  Male ohne vorher gewählte Abwehr; die 30 bis 39 % davor und danach kamen nach Verdammnis und Schutzwall. Dass beim
  85-%-Treffer keine Minderung bereit war, ist geschlossen, nicht gemessen; ob dort ein zweiter Tank in der Gruppe war,
  zeigt das Protokoll nicht. Wie oft der Fall vorkommt, ist offen.
- Die Herausforderung des Co-Tanks wirkt sicherer, aber die drückt der andere Spieler. Die Gegenseite baut A321
  (oben, „Die andere Seite"). Ohne BossModReborn-Modul und ohne seine Geste nimmt RSR ihm den Gegner erst unter
  `HealthForDyingTanks`. Unmittelbar nach einem Treffer kann sein LP-Verlauf steiler sein als nach der Heilung; B nimmt
  dann früher als nötig – die sichere Richtung. Ein Tankbuster auf die ersten beiden der Feindseligkeit trifft ihn auch
  nach einem Wechsel; dann kostet B eine Herausforderung.
- Spielt der Co-Tank ohne Haltung, wirkt die Regel nie, weil sie dort nichts bewirken könnte. Das Protokoll schreibt je
  Tankbuster den Grund; eine falsche Prognose zieht die Regel selbst nach (unten).
- Das Zurückholen folgt dem Takt der Tankbuster (oben). Dass der Wechsel bei bereiter Unverwundbarkeit hält, trägt auch
  gegen einen tödlichen Treffer: Vor einem gemessenen Tankbuster, den nichts Geringeres trägt, zieht der Plan sie
  (Abschnitt „Gemessener Tankbuster: das geringste Mittel"). Für Tankbuster nur als Marker oder Vorhersage und für
  ungemessene Aktionen zündet sie weiter erst unter der Sterbe-Schwelle.

### Messmittel und Nachsteuerung

`DefenseTrace.log` schreibt je erkanntem Tankbuster „tankbuster on you: … vulnerability …". Hält die Regel, schreibt
sie einmal je Tankbuster „tank swap held: …" mit dem Grund (Unverwundbarkeit bereit, kein anderer Tank, Ziel nicht
die Quelle, Feindseligkeit zu gering, Geteiltes Leid im Abklingen). Wählt sie, schreibt sie „tank swap (…) ->
Shirk" mit den Feindseligkeitswerten; ist die Aktion ausgeführt, folgt die „used"-Zeile. Den Ausgang misst der erste
Auto-Angriff der Quelle danach: „moved the enemy" oder „did not move". Im zweiten Fall verlangt jeder weitere
Wechsel in diesem Kampf mehr als das Verhältnis, das versagt hat. Gezählt wird erst ab der Ausführung, eine Wahl
ohne Ausführung verschiebt nichts. Den Erfolg zeigt schon das Ziel des Gegners: Wechselt es auf den Empfänger, gilt
der Wechsel sofort als gelungen. Wartet das Zurückholen, steht einmal je Wechsel „tank swap back waits: …"
(der Co-Tank hat noch keinen Tankbuster genommen, Unverwundbarkeit noch nicht bereit, Herausforderung im
Abklingen oder außer Reichweite); trifft der Tankbuster den Co-Tank, steht „tankbuster on …; holt RSR zurück, steht
„tank swap back … -> Provoke". Ob die Geste des Co-Tanks (Geteiltes Leid auf dich) als eigener Effektsatz bei dir
ankommt, ist nicht beobachtet; fehlt die Zeile „… shirked to you", entfällt D, B bleibt.

Die andere Seite schreibt: „take-over from … held: …; …" einmal je Co-Tank und Grund (kein Angekündigtes, Ziel fest,
du selbst in Gefahr …), „take over … (…)" mit der Herausforderung, „took … over from …" sobald der Gegner dich angreift,
dann je nach nächstem Tankbuster „take-over held" oder „take-over did not move …", und „take-over undone", wenn er vorher
zurückspringt.

**Prüfgrad:** statisch, Prüfskripte, Compile über die CI; im Spiel nicht beobachtet.

## Schutzmechaniken mit Auslöser und die Heilung

Mehrere Tank-Fähigkeiten warten auf ein **Ereignis**, das fremde Heilung abfangen kann. Tragend ist dabei nicht die
Rückhaltung, sondern die richtige Heilzielwahl: `CanBeHealed()` (bis Upstream 7.5.7.1 `NoNeedHealingInvuln()`) liefert `true`, wenn *kein* Schutzstatus
liegt; zwei Aufrufer lasen sie mit dem entgegengesetzten Vorzeichen, und die generische Zielwahl war dadurch blind für
den am schwersten Verletzten (Abschnitt „Gebaut"). Die Rückhaltung selbst ist ein eng umgrenzter Sonderfall: Von den
Lagen (Abschnitt „Lagen und richtige Handlung“) bleibt ein echter Rückhaltefall, Living Dead, umgesetzt als Uhrregel hinter einer
Option (ab Werk aus, Grund unter „Die Living-Dead-Rückhaltung“).
Für The Blackest Night ist keine eigene Rückhalteregel richtig: Heilung berührt den Auslöser nicht, und ein
Heilerschild wird erst nach der TBN-Barriere aufgezehrt. Die Barriere verschiebt auch die Heilschwelle nicht (A85);
in die Zielwahl geht sie über die effektive Gesundheit ein (Gefährdungsklasse 1, Konzept 07).

*Abgrenzung:* Das gilt für Heilung und Schild. Für den **Schadensstrom** gilt das Gegenteil – der Weißmagier hält
Sanctus zurück, solange ein Tank die Barriere trägt, weil die Betäubung den Schaden anhält, an dem sie aufgezehrt
werden müsste. Das steht in Konzept 10.

### Klassen nach Auslöser

Entscheidend ist nicht, ob eine Fähigkeit schützt, sondern **ob sie auf ein Ereignis
wartet**, das der Heiler abfangen kann.

#### Klasse A — auslöserbehaftet

| Job | Fähigkeit | Auslöser | Auslöser ist… | Was fremde Heilung bewirkt |
|---|---|---|---|---|
| DRK | Living Dead | HP fallen auf 0 | **der Tod selbst** | verhindert den Tod, also den Übergang in Walking Dead samt dessen Selbstheilung |
| DRK | The Blackest Night | Barriere wird **vollständig** absorbiert | ein Schadensereignis unterhalb des Todes | **nichts.** Eine Barriere absorbiert vor den HP; ihr Verbrauch hängt am eingehenden Schaden, nicht am Gesundheitsstand. Heilung wirkt hier sogar *für* den Auslöser, weil sie den Tod verhindert, der ihn vereiteln würde |
| SCH | Excogitation | HP-Schwelle auf dem Ziel | ein Schadensereignis unterhalb des Todes | vorzeitige Heilung entwertet die eigene, bereits gesetzte Fähigkeit |

**Die vierte Spalte trennt die Klasse in zwei, und diese Trennung entscheidet über
alles Weitere.**

**A-tödlich — der Auslöser *ist* der Tod.** Living Dead ist der einzige Fall. Das
Ereignis, das der Heiler sonst um jeden Preis verhindert, ist hier das erwünschte.
Jede sonst geltende Regel — „bei niedriger Gesundheit heilen", „bei drohendem
Tankbuster heilen" — ist hier nicht bloß unnötig, sondern **kontraproduktiv**: Sie
zielt auf die Verhinderung dessen, worauf die Fähigkeit wartet.

**A-nichttödlich — der Auslöser liegt unterhalb des Todes.** The Blackest Night und
Excogitation. Der Tod bleibt die Katastrophe; die üblichen Aufhebungsregeln gelten
unverändert.

#### Klasse A+ — The Blackest Night: eine Frage der Werkzeugwahl, nicht der Rückhaltung

Die Fähigkeit kostet **3000 MP** und legt eine Barriere über 25 % der maximalen HP
für 7 Sekunden. Dark Arts — und damit ein kostenloser Edge oder Flood of Shadow —
wird nur gewährt, wenn die Barriere **vollständig** absorbiert wird. Wird sie es
nicht, sind die 3000 MP ersatzlos ausgegeben: Edge of Shadow kostet seinerseits
3000 MP und verlängert Darkside um 30 Sekunden, deren Abriss zehn Prozent Schaden
kostet.

**Heilung und HoT berühren diesen Auslöser nicht.** Eine Barriere absorbiert
eingehenden Schaden, bevor er die HP erreicht; wie schnell sie aufgezehrt wird, hängt
allein am eingehenden Schaden. Der Gesundheitsstand des Trägers ändert daran nichts.
Die einzige Kopplung läuft in die **andere** Richtung: Stirbt der Träger, bevor die
Barriere aufgebraucht ist, entfällt Dark Arts — Heilung wirkt also für den Auslöser,
nicht gegen ihn. *Schluss aus der Wirkbeschreibung („nullifying damage") und der
Auslösebedingung („completely absorbed"), nicht wörtlich belegt.*

**Ein zweiter Schild schadet ebenfalls nicht.** Mehrere Barrieren auf demselben
Charakter werden in einer festen **Verbrauchsreihenfolge** aufgezehrt, nicht anteilig,
und The Blackest Night steht in dieser Reihenfolge **vor** den Schilden, die ein Heiler
auf einen Tank legen kann:

| Barriere | Verbrauchsrang |
|---|---|
| The Blackest Night | 3 |
| Eukrasian Diagnosis (SGE) | 4 |
| Divine Benison (WHM) | 12 |

Der niedrigere Rang wird zuerst aufgezehrt. Ein Heilerschild verzögert die Absorption
von TBN also nicht — Dark Arts wird davon nicht berührt. Er geht dabei auch nicht
verloren: Er bleibt liegen und absorbiert, sobald TBN aufgebraucht ist. Zurückstellen
spart deshalb nichts, es verschiebt nur.

*Quellenstatus:* Gemeinschaftsquelle — die Tabelle „Barrier Consumption Priority" im consolegameswiki
(abgerufen 02.10.2026), die sich auf einen Lodestone-Blog und einen Beitrag auf X stützt; keine offizielle
Beschreibung. Sie führt The Blackest Night auf Rang 3 und Eukrasian Diagnosis auf Rang 4 und, mit
Differential Diagnosis, auf Rang 24 — beide hinter TBN: Kein Heilerschild verzögert seine Absorption. Weitere Ränge derselben Tabelle, die andere Regeln berühren: Stem the Tide aus
Bloodwhetting und Nascent Flash Rang 8, Divine Benison 12, Radiant Aegis 18, Shake It Off 19, Divine
Veil 20.

**Der Träger ist nicht zwingend der Dunkelritter.** `ActionId.resx` (Aktion 7393)
beschreibt TBN als „Creates a barrier around **self or target party member**" — die
Barriere kann auf jedem Gruppenmitglied liegen, und der Party-Zweig in `DRK_Reborn` nutzt das mit
`targetOverride: TargetType.LowHP`. Ein Beschwörer kann also Radiant Aegis tragen und zusätzlich TBN
bekommen. Radiant Aegis steht auf Rang 18, also hinter TBN (Rang 3): Sie verzögert die Absorption von TBN
nicht. Was vor TBN steht, sind nur Crest of Time Borrowed und die Tempera-Schilde (Ränge 1 und 2) — eigene
Schilde von Schnitter (Arcane Crest, ab Stufe 84) und Piktomant, die ein Heiler ebenso wenig steuert.

**Damit bleibt kein Grund, den Schild zurückzustellen.** Was bleibt, ist die gewöhnliche
Dringlichkeitsfrage: Ein Träger mit TBN ist bereits geschützt und deshalb weniger dringend
zu versorgen als ein ungeschütztes Gruppenmitglied. Das beantwortet die Zielwahl: `StatusID.BlackestNight` steht in
`StatusHelper.ShieldStatus`, die effektive Gesundheit (`GetEffectiveHp`) zählt die Barriere mit, und die
Gefährdungsklasse 1 ordnet nach ihr (Konzept 07). Die Heilschwelle liest die Barriere nicht; eine frühere
Anrechnung dort verschob, **ob** überhaupt geheilt wird, statt **wen** zuerst, und ist entfernt (A85).

#### Klasse B — Unverwundbarkeit

Heilung während der Phase wirkt nicht auf den Schutz, wohl aber auf das, was danach
kommt.

| Job | Fähigkeit | Wirkung auf die HP | Lage nach Ablauf |
|---|---|---|---|
| PLD | Hallowed Ground | unverändert | so niedrig wie beim Zünden |
| WAR | Holmgang | fallen nicht unter 1 | möglicherweise 1 HP |
| GNB | Superbolide | **sofort auf 1** | 1 HP, sofern nicht geheilt wurde |
| DRK | Undead Rebirth | fallen nicht unter 1 | Erfolgszustand |

**Living Dead hat drei Phasen.** Die Aktionsbeschreibung im Repository
(`ActionId.resx`, Aktion 3638) ist die Primärquelle:

1. **Living Dead**, 10 s. Fällt die Gesundheit auf 0, tritt kein Tod ein, sondern
   der Wechsel nach Walking Dead. Die Selbstheilung — 1500 Potenz je getroffenem
   Waffenskill oder gewirktem Zauber — hängt bereits an *dieser* Phase.
2. **Walking Dead**, 10 s. Angriffe drücken die Gesundheit nicht unter 1. Wird bis
   zum Ablauf kumuliert Heilung in Höhe der **maximalen** Gesundheit aufgenommen,
   folgt Phase 3; sonst tritt der Tod ein. Die Quelle sagt „an amount of HP totaling
   your maximum HP is restored" ohne Einschränkung auf die Quelle — fremde Heilung
   zählt mit, und bei heutigen Tank-Gesundheitswerten trägt die Selbstheilung davon
   nur einen Bruchteil.
3. **Undead Rebirth**, Restlaufzeit von Walking Dead. Der Dunkelritter kann nicht
   sterben, und der Auslöser ist eingelöst. Der einzige Zustand der ganzen Taxonomie,
   in dem Zurückhaltung weder Stufe 1 noch Stufe 2 berührt, sondern rein Stufe 3
   bedient.

#### Klasse C — reine Schadensreduktion

Rampart, Sentinel, Shadow Wall, Nebula, Sheltron und Holy Sheltron, Bulwark,
Camouflage, Oblation, Heart of Corundum und Clarity of Corundum (die
Reduktionsanteile), Reprisal. Keine Wechselwirkung.

#### Klasse D — Selbstheilung ohne Auslöser

Bloodwhetting und Raw Intuition, Aurora, Clemency, Nascent Flash. Fremde Heilung
addiert sich, verhindert aber nichts.

### Living Dead ist ein Zeitproblem, kein Zustandsproblem

Die Frage lautet nicht *liegt der Status?*, sondern in beiden Phasen eine über
**Rate und Restzeit**.

**Phase 1 — reicht der eingehende Schaden, um vor Ablauf auf 0 zu kommen?** Living
Dead nützt nur, wenn der Tod **innerhalb** der zehn Sekunden eintritt. Zeichnet sich
ab, dass der Tod erst nach Ablauf käme, war die Rückhaltung nicht nur nutzlos — sie
hat den Tank zehn Sekunden ungeheilt gelassen, der nun mit niedriger Gesundheit ohne
Schutz dasteht. Dann ist **im letzten Augenblick** zu handeln, ausreichend und nicht
symbolisch.

Zurückgehalten wird also nicht, *weil* der Status liegt, sondern solange der Tod
noch rechtzeitig kommt.

**Umgesetzt ist das in `StatusHelper.InDeathTriggerWindow` als zwei Bedingungen, nicht
als eine Uhr.** Die Zurückhaltung endet einen Vorlauf von zwei GCDs vor Ablauf, damit
die Heilung noch landen kann — **aber nur, solange der Träger über
`HealthForDyingTanks` steht.** Darunter läuft sie bis zum Ablauf durch, weil dort der
Fall auf 0 die wahrscheinliche Fortsetzung ist und der Vorlauf sonst genau den Auslöser
wegheilen würde, für den die ganze Regel da ist. Die Uhr allein hätte die Fälle 1c und
1e (Tabelle „Lagen und richtige Handlung“) nicht mehr unterschieden: Sie beantwortet „wie viel Zeit
bleibt", nicht „kommt der Tod noch".

**Phase 2 — heilt er sich schnell genug selbst?** Walking Dead verlangt kumuliert
eine volle Maximalgesundheit in zehn Sekunden. Die Selbstheilung liefert 1500 Potenz
je Waffenskill oder Zauber, bei rund 2,4 s GCD also etwa vier Auslösungen. Daraus
folgt ein gestaffeltes Verhalten statt eines Schalters:

| Zeitpunkt in Phase 2 | Verhalten | Begründung |
|---|---|---|
| Anfang | **leicht unterstützen** — Regeneration, ein günstiges HoT | Der Beitrag zählt voll gegen die geforderte Summe, kostet aber wenig |
| Mitte | Kurs prüfen, weiter leicht unterstützen | Solange der Kurs trägt, ist ein großer Zauber vergeudet |
| Kurs reicht nicht | **eingreifen, in voller Höhe** | Die Alternative ist der Tod am Phasenende |
| Kurs trägt bis zum Ende | nichts weiter | Der Rest ist Überheilung |

#### Phase 2: Walking Dead

**Vorgabe des Auftraggebers (25.09.2026, A147):** „walking dead läßt solange es läuft den darkknight sich
selbst durch angriffe heilen. […] also vertraut man am anfang (bei eben den 1hp) darauf, dass der tank
sich selbst heilt, indem er bei gegnern schaden verursacht. das soll nur leicht mit einem hot
unterstützt werden. erst wenn der timer des effektes ausläuft bzw. klar ist, dass der darkknight sich
in der verbleibenden zeit nicht selbst durch angriff (vollständig) heilen kann, soll unterstützt werden.
das kann z.b. durch fehlende gegnerzahlen oder durch ein anstehendes event passieren, wo der darkknight
nicht mehr angreifen kann." Der Wirktext bestätigt die Mechanik (Living Dead, `ActionId.resx` 3638).

**Sachstand:** `StatusHelper.WalkingDeadCarriedBySelfHeal` sagt, ob der Träger noch von seinen eigenen
Angriffen getragen wird. Solange das gilt, nimmt ihn keine Heilaktion als Ziel, die keinen HoT aus
`SingleHots` verleiht (`ActionTargetInfo.FindTarget`), und er zählt nicht als Grund für eine
Flächenheilung um den Wirkenden. Regen des Weißmagiers ist unter Walking Dead von der
`RegenHeal`-Sperre ausgenommen. Die volle Unterstützung setzt ein, sobald einer dieser Fälle eintritt:

| Auslöser | Maß, aus dem Spiel |
|---|---|
| Timer läuft aus | derselbe Vorlauf wie bei der Living-Dead-Sperre (zwei GCDs bis zur Entscheidung) |
| kein Gegner in Reichweite | Spielreichweite von Hard Slash, Trefferfläche zu Trefferfläche |
| angekündigtes Ereignis | BossModReborn-Auszeit vor Ablauf; ohne Modul erst reaktiv über die Reichweite |
| Tank-Limitbruch auf der Gruppe | Status Last Bastion, Land Waker, Dark Force oder Gunmetal Soul |
| er schafft es nicht | Gesundheit seit Beginn des Fensters, auf die Restzeit hochgerechnet, bleibt unter 100 % |

Der Kurs ist netto: Schaden zieht ab, er unterschätzt also die kumulierte Heilung, und die Freigabe
kommt eher zu früh als zu spät. Im ersten GCD gibt es noch nichts zu messen; dann wird ihm vertraut.

**Hinweis des Auftraggebers zu „most attacks":** Es gibt Raidwides, die alle nur mit dem Limitbruch
eines Tanks überleben; das ist wahrscheinlich die Ausnahme, die der Wirktext meint (seine Deutung).
Es sind sehr wenige, seine Beispiele: die Alexander-Raids, die Prüfung gegen den Krieger des Lichts.
Deshalb gibt nicht jeder Raidwide die volle Unterstützung frei — das höbe das Vertrauen am Anfang bei
jedem Raidwide auf —, sondern erst der Tank-Limitbruch auf der Gruppe: Er wird für genau diesen
Treffer gezogen, und bei 1 HP stünde der Träger schutzlos davor.

**Grenzen:** Rotationen, die ihr Heilziel selbst wählen statt über `FindTarget` (fremde Rotationen,
direkte Aufrufe von `FindTargetByType`), sehen die Sperre nicht. Die Flächenheilflagge rechnet den
Träger bei 1 HP weiter in ihre Mittelwerte ein. Ein solcher Treffer ohne Tank-Limitbruch wird nicht
erkannt. Der Limitbruch selbst wird im selben Bild erkannt, in dem sein Status erscheint; die Heilung
kommt aber nur vor dem Einschlag an, wenn dazwischen noch ein Einschiebeplatz (Benediction) oder ein
GCD mit Wirkzeit (Cure II) liegt. Wie viel Zeit zwischen Limitbruch und Einschlag liegt, entscheidet der
Tank, der ihn zieht.

#### Die Aufhebungen kehren sich für Living Dead um

| Aufhebung | Bei A-nichttödlich | Bei Living Dead Phase 1 |
|---|---|---|
| Niedrige Gesundheit | hebt auf — der Tank droht zu sterben | **hebt nicht auf** — niedrige Gesundheit ist der erwünschte Zustand, sie bringt den Auslöser näher |
| Tankbuster-Fenster | hebt auf — die Sequenz ist nicht beurteilbar | **hebt nicht auf** — der Buster liefert den Auslöser |

Eine Vollheilung bei zehn Prozent Gesundheit in Phase 1 geht auf zwei Weisen daneben,
und beide treffen zu: **Sie rettet nicht**, wenn der Buster auch vollgeheilt tötet;
und **sie schadet**, wenn sie rettet, weil sie dann den Übergang in Phase 2 verhindert
und die Fähigkeit entwertet, die der Tank für genau diesen Moment gezündet hat. Einen
dritten Ausgang gibt es nicht: Eine Heilung, die den Tod in Phase 1 verhindert,
verhindert per Konstruktion den Auslöser. Das ist keine Abwägung, sondern eine
Identität.

**Für Phase 1 bleibt damit genau eine Aufhebung** — und sie hat nichts mit Gefahr zu
tun, sondern mit der Uhr:

> Die Rückhaltung wird aufgehoben, **wenn der Tod nicht mehr vor dem Ende der ersten
> Phase eintreten würde.**

Dazu die Vorbedingung aus Fall 1b (Tabelle „Lagen und richtige Handlung“), die vor der Phase liegt: Wer die Heilmenge für
Phase 2 gar nicht aufbringen kann, darf den Weg über den Tod nicht einschlagen.

#### Warum eine Überlebensprüfung diese Zusage nicht tragen kann

Eine Prüfung der Form „erst feststellen, ob der Tank ohne mich überlebt, dann
zurückhalten" ist aus den vorhandenen Größen **nicht herstellbar**:

- **Tankbuster wechseln das Ziel mitten in der Sequenz.** `DataCenter.BMRNextTankbusterIn`
  nennt nur den Zeitpunkt; wen er trifft, sagt BossModReborn nur für den ersten Eintrag im
  Minderungsfenster (`DataCenter.BMRTankbusterHitsPlayer`). `IsHostileCastingTankBusterAtMe` ist
  spielerzentriert (`DataCenter.IsHostileCastingTankBusterAtMe`).
- **Ein Tankbuster ist nicht ein Einschlag.** Mehrfach einschlagende Buster sind eine
  Folge von Treffern; die Vorhersage nennt den Beginn, nicht die Anzahl und nicht die
  Gesamtsumme.
- **Manche Buster töten auch einen vollgeheilten und geschildeten Tank.** Die
  stillschweigende Annahme „wenn ich rechtzeitig heile, überlebt er" trifft dort nicht
  zu.

Für einen **gewirkten, gemessenen** Tankbuster gibt es einen Höchstwert (Tankbuster-Tabelle, Konzept 13),
den der Plan des Tanks rechnet (Abschnitt „Gemessener Tankbuster: das geringste Mittel"). **Die Living-Dead-Regel
liest ihn nicht, und das ist entschieden (A306).** In keiner Richtung trägt er eine Entscheidung der Rückhaltung:

- *Verlängern* — im Vorlauf über `HealthForDyingTanks` weiter zurückhalten, weil der Tankbuster ihn vor Ablauf auf 0
  brächte (Fall 1d): Die Tabelle führt nur den **höchsten** gesehenen Wert. Sie sagt „kann töten", nicht „tötet". Für
  den Plan ist das die sichere Richtung, er nimmt eher zu viel Abwehr; für die Rückhaltung die gefährliche. Liegt der
  wirkliche Treffer unter der Gesundheit, steht der Träger nach ihm mit einem Rest, ungeheilt, und Living Dead läuft
  im selben oder nächsten GCD ab – heute wäre die Heilung vor dem Treffer gekommen. Der Gewinn steht dem nur in einem
  gleich schmalen Band gegenüber: Wo die Heilung ihn ohnehin nicht über den Treffer hebt, spart er einen Heil-GCD;
  wo sie es täte, bleibt der Auslöser erhalten. Beides ohne Untergrenze des Treffers nicht zu trennen.
- *Früher freigeben* — weil der Tankbuster ihn nicht auf 0 bringt: Der übrige Schadensstrom kann es weiter, und in
  Phase 1 kostet das Zurückhalten kein Überleben, solange der Vorlauf rechtzeitig freigibt.
- *Mit der Option aus* — die Absenkung aufheben, weil ein gemessener Treffer ihn in Walking Dead brächte: in der
  allgemeinen Lösung enthalten. Mit der Option aus gilt Living Dead für die Heilung gar nicht mehr als Schutz
  (Abschnitt „Die Living-Dead-Rückhaltung"), gemessen oder nicht.

Für Marker, Vorhersagen und nie gemessene Aktionen bleibt die Prüfung ohnehin unmöglich.

Daraus folgt **nicht**, die Rückhaltung unter Living Dead freizugeben. Dass Heilung manchmal nichts
nützt, macht Zurückhaltung nicht besser, nur nicht schlechter — und welcher Fall
vorliegt, ist vorab nicht erkennbar. Für **A-nichttödliche** Auslöser folgt umgekehrt: keine
Rückhaltung, weil die Prognose dort unmöglich ist und der Tod die Katastrophe bleibt (Abschnitt
„Ausgeschlossen“).

### Lagen und richtige Handlung

Vollständig über die Dimensionen Fähigkeitsklasse × Gesundheitsstand × Heileraktion.
„Richtig" meint die Handlung, die dem Vorrang der Überlebenssicherung folgt.

| # | Lage | Heilung richtig? | Schild richtig? | Begründung |
|---|---|---|---|---|
| 1 | DRK, Living Dead aktiv, **Tod tritt vor Ablauf ein**, Heilkapazität für Phase 2 gesichert | **nein** | **nein** | Stufe 1 ist gesichert, also darf Stufe 2 entscheiden |
| 1b | dieselbe Lage, **Heilkapazität nicht gesichert** | **ja** | ja | Stufe 1 schlägt Stufe 2: sicheres gegen unsicheres Überleben zu tauschen ist verboten |
| 1c | DRK, Living Dead aktiv, **Tod würde nicht mehr vor Phasenende eintreten** | **ja, rechtzeitig vor Ablauf** | ja | Die **einzige** Aufhebung in Phase 1 |
| 1d | DRK, Living Dead aktiv, **Tankbuster-Fenster erkannt** | **nein** | **nein** | Der Buster liefert den Auslöser |
| 1e | DRK, Living Dead aktiv, **HP sehr niedrig**, Tod noch vor Ablauf zu erwarten | **nein** | **nein** | Niedrige Gesundheit ist hier erwünschter Zustand, keine Gefahrenmeldung |
| 2 | DRK, Living Dead aktiv, HP niedrig, Ablauf fern | wie 1 / 1b / 1e | dito | Der Gesundheitsstand entscheidet in dieser Phase nicht |
| 3 | DRK, Living Dead läuft ab, Tod nicht eingetreten | **ja, dringend** | ja | Ausführungsform von 1c: mit Vorlauf, damit die Heilung vor dem Ablauf wirkt |
| 4 | DRK, Walking Dead aktiv, **Kurs trägt** | leicht unterstützen | nein, wirkungslos bei 1 HP | Billige Beiträge zählen voll gegen die Summe |
| 4a | DRK, Walking Dead aktiv, **Kurs reicht nicht** | **ja, in voller Höhe** | nein | Die Alternative ist der Tod am Phasenende |
| 4b | DRK, **Undead Rebirth** aktiv | nein, nachrangig | nein | Bedingung erfüllt, reine Stufe 3 |
| 5 | DRK, TBN aktiv | **nach normaler Regel** | **nach normaler Regel** | **Kein Sonderfall.** Heilung berührt den Auslöser nicht, und ein Heilerschild wird erst nach TBN aufgezehrt. Die Barriere zählt in der effektiven Gesundheit der Zielwahl (Konzept 07), nicht in der Heilschwelle (A85) |
| 6 | GNB, Superbolide aktiv | **ja** | ja | HP stehen auf 1; das Fenster ist die einzige gefahrlose Gelegenheit |
| 7 | WAR, Holmgang aktiv, HP heruntergedrückt | **ja** | ja | wie 6 |
| 8 | PLD, Hallowed Ground aktiv, beim Zünden wenig HP | **ja** | ja | Die HP bleiben unverändert; nach Ablauf steht er, wo er stand |
| 9 | PLD, Hallowed Ground aktiv, beim Zünden viel HP | nein, nachrangig | nein | Einziger Fall echter Entbehrlichkeit in Klasse B |
| 10 | GNB, Heart of Corundum liegt | **nach normaler Regel** | nach normaler Regel | **Kein Rückhaltefall** — der Catharsis-Stoß löst auch bei Ablauf aus |
| 11 | GNB, Catharsis ausgelöst | **ja** | ja | Der Auslöser ist verbraucht |
| 12 | Klasse C aktiv | nach normaler Regel | nach normaler Regel | keine Wechselwirkung |
| 13 | Klasse D aktiv | nachrangig | nach normaler Regel | Die Selbstheilung trägt einen Teil, ersetzt sie nicht |
| 14 | Mehrere brauchen Heilung, Tank geschützt | Tank **nachrangig**, andere zuerst | dito | Der Schutz verschiebt die Dringlichkeit, hebt den Bedarf nicht auf |
| 15 | Nur der geschützte Tank braucht Heilung | **ja** | ja | Das Fenster ist die sicherste Gelegenheit |

Zeile 14 und 15 sind der Kern: **Der Schutz verschiebt die Reihenfolge, er hebt den
Bedarf nicht auf.** Handeln schadet nur unter Living Dead in Phase 1 (Zeilen 1, 1d, 1e und 2) —
und dort nur, solange Stufe 1 gesichert ist. Von einundzwanzig Lagen bleibt damit **ein** echter
Rückhaltefall (Living Dead, Phase 1), dem drei Aufhebungen (1b, 1c samt Ausführungsform 3, 4a)
gegenüberstehen; eine Lage ist reine Ressourcenschonung ohne Risiko (4b). Eine Bedingung, die häufiger nicht gilt als gilt,
ist kein Leitmotiv: Der belastbare Kern bleibt die Richtigstellung der Zielwahl, und
der einzige verbliebene Sonderfall ist Living Dead.

Die Zeilen 1 bis 3 gelten mit `WithholdHealingForLivingDead` an. Mit der Option aus ist der Tod nicht gewollt, und
Living Dead ist für die Heilung kein Schutz: geheilt wird nach normaler Regel, wie in Zeile 12 (A306). Die Zeilen 4
bis 4b gelten in beiden Stellungen.

### Gebaut

#### Die Zielwahl (Rangstufe 1)

`CanBeHealed()` (bis Upstream 7.5.7.1 `NoNeedHealingInvuln()`) ruft `WillStatusEndGCD(2, 0, false, …)`. Die Kette
`WillStatusEnd` → `StatusTime` (beide in `StatusHelper.cs`) liefert bei
**fehlendem** Status die Zeit `0f`, und `(0 >= 0 || !HasStatus) && 0 <= time` ergibt
`true`. Der Rückgabewert bedeutet also: *kein schützender Status aktiv, Heilung ist
zuzulassen* — der Name sagt das Gegenteil, was die ganze Defektklasse erklärt.

`GeneralHealTarget` sammelte mit `if (!o.NoNeedHealingInvuln())` folglich genau die
Gruppenmitglieder mit **aktivem Schutz**. Im Regelfall blieb die Liste leer, alle
darauf aufbauenden Stufen liefen ins Leere, und der Aufrufer fiel auf den Träger der
Tank-Haltung zurück. Praktisch: **Die generische Heilzielwahl war blind für den am
schwersten verletzten Gruppenteil und wählte faktisch immer den Tank.** War die Liste
einmal gefüllt, wurde bevorzugt der Unverwundbare geheilt. Derselbe Fehler stand ein
zweites Mal in der Excogitation-Heilzielwahl von `SCH_Reborn`, wo Excogitation nur auf gerade unverwundbare
Ziele ging; dort liest sie `CanBeHealed()` ohne Negation.

Umgesetzt ist beides zusammen, denn getrennt wäre der erste Schritt schädlich: Sobald
die Liste normale Ziele enthält, fielen die geschützten heraus und der Zufallszustand
kippte in den entgegengesetzten. Geschützte Ziele sind seither **nachrangig, nicht
ausgeschlossen**, und die fehlenden Ids `HallowedGround`, `HallowedGround_1302` und
`UndeadRebirth` sind ergänzt — zwingend nach dem Umbau, weil sie davor das *Ob* und
danach nur noch die *Reihenfolge* ändern.

Im `StateUpdater` senkt ein Schutzstatus seither die Heilschwelle auf
`HealthProtectedRatio`, statt das Heilflag zu unterdrücken. Ohne diese Umstellung
erreicht die Herabstufung ihren Zweck nicht: Ohne Flag ruft der Dispatcher
`HealSingleGCD` gar nicht auf.

#### Die Living-Dead-Rückhaltung

Sie liegt hinter `WithholdHealingForLivingDead` (Standard **aus**) und benutzt
`StatusHelper.InDeathTriggerWindow` auf einer eigenen Liste `DeathTriggeredStatus`,
die genau den einen Status führt, dessen Auslöser der eigene Tod ist. Die Paarung aus
Einstellung und Fenster steht als `ObjectHelper.IsHeldForDeathTrigger` an **einem**
Ort; die Bedingung selbst kommt im Baum kein zweites Mal vor.

**Ein Halt muss auf jedem Weg halten, auf dem eine Heilung den Träger erreicht.** Eine
einzelne Lücke schwächt ihn nicht, sie hebt ihn auf — der Träger wird aus dem Auslöser
herausgeheilt und die Fähigkeit ist umsonst gezündet. Erfasst sind deshalb:

| Weg | Ort |
|---|---|
| Heilflag für ein Gruppenmitglied | `StateUpdater.ShouldHealSingle` |
| Heilflag für den Spieler selbst | `StateUpdater.ShouldHealSelf` |
| Kandidatenliste, Rollenpässe, Auswahl des am schwersten Verletzten, Tank-Haltungs-Fallback und `partyMembers[0]`-Fallback | Eingangsfilter von `ActionTargetInfo.FindHealTarget` |
| Selbstabkürzung, die die Kandidatenliste umgeht | eigene Prüfung in `GeneralHealTarget` |
| Aetherpact des Gelehrten, dessen Heilstrom nach dem Start ohne Zielwahl weiterläuft | `SCH_Reborn.HealSingleAbility` löst die Verbindung, solange `IsHeldForDeathTrigger` gilt; bis A306 bei jedem Living Dead, auch mit der Option aus und nach der Freigabe vor Ablauf |

Der Eingangsfilter ist der tragende Teil: Das Heilflag kann von einem **anderen**
Gruppenmitglied gesetzt worden sein, und liegt dieses außerhalb der Reichweite der
gewählten Heilaktion, bleibt der Träger als einziger Kandidat übrig. Ohne den Filter
holt ihn spätestens der Tank-Haltungs-Fallback zurück — ein Dunkelritter trägt Grit.

Zwei Wege bleiben bewusst offen. **Flächenheilung** trifft den Träger als
Nebenwirkung; sie deswegen zu unterlassen hieße, die Gruppe für den Auslöser zu
opfern, und verstieße gegen die Rangordnung. **Fremde Rotationen, die ihr Heilziel
selbst setzen** (`targetOverride: TargetType.Tank`, in den Beiruta-Heilern) umgehen
die zentrale Zielwahl; sie halten allerdings über ihre eigene Sperrliste ohnehin
zurück, dort sogar unabhängig von der Einstellung.

Die Uhrregel musste nicht gebaut werden: `WillStatusEndGCD` meldet einen Status vor
seinem Ablauf als endend. Sie braucht aber eine **eigene** Liste — `StatusTime`
liefert das Minimum über die ganze übergebene Liste, sodass eine fremde kurze
Unverwundbarkeit das Fenster als endend gemeldet hätte.

Die Option ist nötig, weil RSR Living Dead selbst als Notrettung bei
`HealthForDyingTanks` zündet. Dort ist der Tod die Katastrophe, und Walking Dead
verlangt danach eine volle Maximalgesundheit an Heilung in zehn Sekunden.


**Der Hebel ist die Option, nicht der Grenzwert.** Je nach Stellung von `WithholdHealingForLivingDead`:
- **Option an:** Der Träger wird gar nicht geheilt, solange das Todesfenster läuft
  (`IsHeldForDeathTrigger`). Das Fenster endet zwei GCDs vor Ablauf, außer der Träger steht auf oder
  unter `HealthForDyingTanks` (`DeathStillLikely`, A88) — dann läuft es bis zum Ablauf.
- **Option aus:** Der Träger wird geheilt wie ein Ungeschützter: Schwelle, Zielordnung, kritische Klasse und
  die Vorausheilung vor einem angekündigten Treffer lesen `ObjectHelper.IsUnprotectedForHealing`, das Living Dead
  dann nicht als Schutz zählt (A306). Der Einstellungstext verspricht für Aus „he is healed normally, the death
  never happens", und er nennt als Grund für Aus die Notrettung, mit der RSR Living Dead bei `HealthForDyingTanks`
  selbst zündet. Bis A306 las die Heilkette Living Dead auch dann als Unverwundbarkeit und senkte die Schwelle
  für die erste Hälfte auf `HealthProtectedRatio` (0,15). Bei gleicher Voreinstellung beider Werte zog die
  Heilung den Notgeretteten knapp über 15 % und ließ ihn dort; der Strom, der ihn zuvor auf 15 % gebracht hatte,
  brachte ihn auf 0 und in Walking Dead, das dann eine volle Maximalgesundheit an Heilung in zehn Sekunden
  verlangt. Ein Fall auf 0 unter Living Dead ist kein verschonter Treffer wie unter Holmgang, sondern der
  Übergang in diese Frist.

`HealthProtectedRatio` gilt damit für Living Dead in keiner Stellung, nur für die übrigen Invulnerabilitäten der
Liste (Holmgang, Superbolide, Hallowed Ground); seine Beschreibung sagt das. Die Leser, die fragen, ob ein Treffer
ihn jetzt töten kann, lesen weiter `CanBeHealed`: der Provoke des Co-Tanks, der den Gegner nicht von einem Tank unter
Living Dead zieht (`451d9e90`), und die eigenen Tankbuster-Cooldowns (`TankbusterOnMeWithin`, Tankbuster-Plan). Die
Einstellung spricht von Heilung.

**Welche Abwehr des Dunkelritters die Heilentscheidung berührt** (erhoben A141):

| Fähigkeit | Pfad | Wirkung auf die Heilschwelle |
|---|---|---|
| Living Dead | Option an: `IsHeldForDeathTrigger`; Option aus: keine (`IsUnprotectedForHealing`) | an: keine Heilung im Todesfenster; aus: die normale Schwelle, wie oben |
| Walking Dead | in `NoNeedHealingStatus` auskommentiert | keine — richtig, dort ist Heilung überlebensnotwendig |
| The Blackest Night | Schildanteil des Spiels (`ShieldPercentage`) im effektiven Puffer | keine auf die Schwelle, seit die Schildanrechnung entfernt ist (A85); der Schild zählt im Puffer der Vorausschau und der Sterbegefährdung (`GetEffectiveHp`) |
| Shadow Wall, Rampart | `RampartStatus` | keine: gelesen als `StatusProvide`, von `HasMajorMitigation` und von `HoldReprisalForRaidwide`, jeweils für den eigenen Charakter |
| Dark Mind, Oblation, Dark Missionary, Reprisal | in keiner heilrelevanten Liste bzw. am Gegner | keine |

Schadensreduktion und Barriere wirken auf keine Heilschwelle; nur die Invulnerabilität tut es. Die Rate, die aus
Minderung folgt, geht über die Vorausschau ein (`GetForecastSurvivingShare`), nicht über Listen.
#### Die Barriere senkt den Heilbedarf nicht

**Ein Schild verhindert Schaden, er stellt keine Gesundheit her.** Ein vollgeheilter Tank **mit**
Barriere ist besser geschützt als ein geschildeter Tank mit wenig Gesundheit; beide Größen addieren
sich, sie ersetzen einander nicht. Eine laufende Barriere ist damit die Gelegenheit, das Polster zu
vergrößern, nicht der Grund, es kleiner zu lassen. Läuft sie ungenutzt ab, steht der Träger
unverändert tief; fängt sie den Treffer, ist sie verbraucht und er steht ebenso tief — verändert hat
sich nur die verbleibende Zeit.

Die frühere Anrechnung (`CreditShieldToEffectiveHp`) rechnete den Schild auf die Gesundheitsquote und
verzögerte damit die Einzelziel-Heilung um die Barrierengröße — bei The Blackest Night 25
Prozentpunkte. Sie ist entfernt (A85); das Verhalten entspricht wieder dem Upstream.

**Heilung steht dem Aufzehren der Barriere nicht entgegen**, und darin liegt der Unterschied zur
Minderungssperre: Wie schnell eine Barriere verbraucht wird, hängt allein am eingehenden Schaden. Die
einzige Kopplung läuft umgekehrt — stirbt der Träger vorher, entfällt Dark Arts. Was den Verbrauch
verhindert, ist Schadensminderung, und dagegen steht die Sperre in Konzept 10.

#### Schildanrechnung: Divine Benison und Intersection

| Stelle | Wirkung | Stand |
|---|---|---|
| `ModifyDivineBenisonPvE` setzte `StatusProvide` statt `TargetStatusProvide` | Divine Benison geht auf **fremde** Ziele. Die Doppelbelegungssperre prüfte damit den Spieler statt das Ziel und griff nie; zugleich sperrte sie die Aktion ganz, sobald der Weißmagier den Schild selbst trug. Die drei Geschwister (`AdloquiumPvE`, `EukrasianDiagnosisPvE`, `CelestialIntersectionPvE`) nutzen `TargetStatusProvide` | behoben, beide Ids auf der Zielseite |
| `StatusID.Intersection` (1889) und `Intersection_4040` fehlten in `StatusHelper.ShieldStatus` | Beide sind als „A magicked barrier is nullifying damage" ausgewiesen. Der Schild eines Astrologen zählte nicht zur effektiven Gesundheit; sein Ziel erschien verletzter, als es ist | behoben (A43) |

#### Prüfmittel und Prüfgrad

`.github/scripts/audit/scan8.py` meldet Prädikate mit negierendem Namen, die im Baum mit beiden Polaritäten gelesen
werden, `check_invuln_polarity.py` jede negierte Lesart von `CanBeHealed` (und dem alten Namen) ohne Grund – genau die hier
gefundene Defektklasse. Prüfgrad: Prüfskripte, Fallmatrix, Compile über die CI; im Spiel nicht beobachtet. Dass die
Rückhaltung besser spielt, ist nicht belegt und steht deshalb hinter einer Option; die Richtigstellung der Zielwahl
ist eine belegte Defektbehebung und hat keinen Schalter – er würde den fehlerhaften Zustand konservieren.

### Ausgeschlossen

**Nullvariante** — nichts ändern. Ausgeschlossen: zwei belegte Defekte der Rangstufe 1
blieben stehen.

**Nur Living Dead behandeln, Zielwahl unangetastet.** Ausgeschlossen: Es hätte den
größeren, bereits belegten Fehler stehen gelassen, um einen Sonderfall zu bedienen.

**Eine Rückhaltung von Heilung für The Blackest Night oder Excogitation.**
Ausgeschlossen für **Excogitation**, weil sein Auslöser nicht vorhersehbar ist: Ob der
Schaden kommt, der die Schwelle unterschreitet, ist unbekannt. Ausgeschlossen für
**The Blackest Night**, weil Heilung dessen Auslöser gar nicht berührt — eine Barriere
absorbiert vor den HP. Was dort bleibt, ist keine Rückhaltung, sondern die
Nachrangigkeit eines **zweiten Schildes**; siehe Klasse A+.

**Gunbreaker Catharsis of Corundum als Rückhaltefall.** Ausgeschlossen am Artefakt:
`Status.resx` beschreibt Status 2685 mit „HP will be restored automatically upon
falling below a certain level **or expiration of effect duration**". Der Heilstoß
kommt in jedem Fall; es gibt nichts zurückzuhalten. Der auslösertragende Bezeichner
heißt `CatharsisOfCorundum` (2685, in Dawntrail zusätzlich 4296); `ClarityOfCorundum`
(2684) ist reine Schadensreduktion und gehört nach Klasse C.

**Eine Gesundheitsschwelle statt der Prioritätsstufe.** Ausgeschlossen, weil sie
Fall 14 nicht trifft: Sie kennt den Vergleich mit anderen Gruppenmitgliedern nicht.
Die Prioritätsstufe leistet beides, und `GeneralHealTarget` besitzt die Rangfolge
bereits — es fehlte nur eine Ebene.

**Ein eigener Messbaustein für Heilraten auf Gruppenmitglieder.** Ausgeschlossen und
überflüssig. Ausgeschlossen war er, weil die Uhrregel seinen einzigen
vorgesehenen Verbraucher ersetzt; überflüssig ist er, weil die Größe ohne neuen
Baustein entstanden ist: `DataCenter.RecordedHP` nimmt die Gruppenmitglieder seit A91
mit auf, und `GetTTK` wertet sie seitdem für jede Gruppen-Id aus. Der Eingriff war eine
Schleife neben der bestehenden, kein Ringpuffer und kein dritter Effekt-Handler.

| Größe | Vorhanden? | Beleg |
|---|---|---|
| Restzeit des Status | **ja** | `StatusHelper.StatusTime(…)` |
| Historie der Gesundheit über die Zeit | **für Gegner und Gruppe** | `DataCenter.RecordedHP` für die Gruppe, gefüllt in `TargetUpdater.UpdateTimeToKill` aus `PartyMembers`; Gegner in Upstreams `TimeToKillTracker` (30 s, seit 3c6ce9ee2, A341) |
| Zeit bis zum Tod eines Gruppenmitglieds | **ja** | `ObjectHelper.GetCorrectedTTK`, gegen den eigenen Vorhersagefehler kalibriert |
| Abtastrate der Historie | **1 Hz** | `TargetUpdater.TimeToKillUpdateIntervalMs` |
| Eingehende Heilung auf ein Party-Mitglied | **nicht gesondert ausgewertet, und nicht nötig** | Der Gesundheitsverlauf ist bereits netto: Eine fremde Heilung zeigt sich als steigender Anteil, `GetTTK` antwortet dann `NaN` |
| Schadensbetrag eines Gegnertreffers | **verfügbar und gelesen** | `Watcher.FullAmount`, voller Betrag auch über 65.535 Punkte (A140, A144) |

Zwei Genauigkeitsgrenzen bestehen fort: Die 1-Hz-Abtastung ist für ein
Zehn-Sekunden-Fenster grob, und ein Gesundheitsdelta ist ein Surrogat für kumulierte
Heilung — fallen Heilung und Schaden in dasselbe Intervall, heben sie sich auf, obwohl
die Heilung gegen die von Walking Dead geforderte Summe zählt. Für den Weg **zur Null**
ist das unerheblich, für den Weg **zur aufgenommenen Heilmenge** nicht; Fall 4a beantwortet A147 deshalb nur
näherungsweise, über den Gesundheitsanstieg seit Beginn des Fensters.

**`HpRecoveryDown` und `Mounted` in der Schwellensenkung.** Ausgeschlossen: `Mounted`
nullifiziert Heilung und gehört in einen Ausschluss, nicht in eine Herabstufung;
`HpRecoveryDown` mindert Heilung nur und ist damit ein Grund, **härter** zu heilen.

### Grenzen der Living-Dead-Regel

**Der Vorlauf der Uhrregel misst bis zur Entscheidung, nicht bis zum Landen der
Heilung.** Im ungünstigsten Fall muss der laufende GCD auslaufen und ein Zauber mit
Wirkzeit darauf fertig werden — zusammen zwei GCDs. Der Vorlauf muss daher mindestens
zwei GCDs betragen. Wie viele Sekunden das sind, steht nicht fest: Gemessen wird die
**tatsächliche** Erholzeit des Spielers, die das Spiel meldet
(`DataCenter.DefaultGCDTotal`; läuft kein GCD, die Länge, die das Spiel für einen Waffenskill rechnet,
`ActionManagerHelper.GetDefaultAdjustedRecastTime`, A240), also
verkürzt Zaubertempo den Vorlauf mit. Gegen die zehn Sekunden von Living Dead
(`ActionId.resx`, Aktion 3638) ist es rund die halbe Phase.

**Der Vorlauf greift nicht, solange der Tod noch erreichbar ist** — das ist Fall 1e der
Tabelle „Lagen und richtige Handlung“, und er ist die Bedingung, unter der der Preis des Vorlaufs überhaupt
tragbar ist. Ohne ihn wird die Startbahn für die Heilung mit genau dem Ergebnis
bezahlt, das die Zurückhaltung erzeugen soll: Ein Träger, der tief genug für den Fall
auf 0 steht, würde in der Vorlaufzeit über die Linie zurückgeheilt, Living Dead liefe
ungenutzt ab, und der Tank hätte seine Unverwundbarkeit für nichts ausgegeben. Die
Grenze ist `HealthForDyingTanks` — der Wert, bei dem die Tank-Rotationen die
Unverwundbarkeit selbst zünden, also die Aussage des Baums über „dieser Tank fällt
gleich". Darüber kommt der Tod nicht mehr, und die Heilung soll laufen, bevor der
Träger wieder sterblich ist; darunter kommt er, und nichts darf ihn aufhalten.

**Verloren geht dabei nichts:** Läuft Living Dead ab, endet die Zurückhaltung mit ihm,
und der Träger ist wieder gewöhnliches Heilziel. Was er nicht mehr kann, ist über die
Schwelle geheilt zu werden, solange der Auslöser noch erreichbar ist.

Ein kürzerer Vorlauf senkt den Preis weiter, lässt die Heilung aber nach Ablauf landen,
wenn der Tank bereits ungeschützt ist. Eine Fähigkeit ohne Wirkzeit würde beides lösen
— welche Heilung gleich fällt, ist jedoch eine Rotationsentscheidung, die die zentrale
Schicht weder kennt noch erzwingen kann.

**Die gestaffelte Phase-2-Unterstützung** (Fälle 4 und 4a) ist gebaut (A147, Abschnitt „Phase 2: Walking Dead"):
`WalkingDeadCarriedBySelfHeal` lässt nur HoTs an den Träger, solange er sich selbst trägt, und gibt die volle
Unterstützung bei den dort genannten Auslösern frei. Die Kursprognose rechnet die Gesundheit seit Beginn des Fensters
auf die Restzeit hoch, nicht die aufgenommene Heilmenge; fallen Heilung und Schaden in dieselbe Abtastung, unterschätzt
sie den Kurs (Grenze unter „Ausgeschlossen").

### Betroffene

**Endnutzer.** Die Heilzielwahl aller Jobs folgt der Rangfolge: Der am schwersten verletzte Schadensausteiler ist
wählbar, wo vor der Richtigstellung regelmäßig der Tank gewählt wurde.

**Autoren abgeleiteter Rotationen.** `NoNeedHealingStatus` behält seine Signatur,
ändert aber seinen Inhalt — eine Verhaltensänderung ohne Compilerfehler für jede
abgeleitete Rotation, die die Liste liest. Im `CHANGELOG` vermerkt.

**Upstream.** Der Eingriff liegt in `ActionTargetInfo`, `StatusHelper` und
`StateUpdater`, alle mit regelmäßiger Upstream-Aktivität.

## Offene Punkte

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.
