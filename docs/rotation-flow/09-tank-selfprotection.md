# 09 · Tank-Selbstschutz

**Leitfrage:** Womit schützt sich ein Tank gegen einen Treffer, und was dürfen Heilung, Hilfe für andere und die
übrigen Pfade der Rotation diesem Schutz nicht nehmen?

Das Dokument gibt den geltenden Stand; die Prüfhistorie steht in `AUDIT_LOG.md`, zurückgenommene Aussagen dort in
Abschnitt C.

## Antwort

- **Gemessener Tankbuster:** das billigste Bündel eigener Abwehr, mit dem der Tank ihn übersteht; die
  Unverwundbarkeit nur, wenn keines reicht; „alles" nur, wenn auch sie nicht verfügbar ist. Was der Plan nicht
  braucht, hält RSR auf allen Pfaden zurück. Zwei Optionen, ab Werk an.
- **Tankbuster ohne Messung** (Marker, Vorhersage, nie gemessene Aktion): die Einzelabwehr der Tank-Rotationen, je
  vorhergesagtem Tankbuster eine große Minderung oder Schutzwall und die kurzen dazu; die Unverwundbarkeit reaktiv
  unter `HealthForDyingTanks`.
- **Krieger:** Urflackern geht an ein anderes Mitglied, wenn er Urimpuls nicht selbst braucht oder das Mitglied die
  Triage gewinnt; Abschütteln wartet, solange es ihm laufende oder vom Plan gebrauchte eigene Minderungen nähme.
- **Tankwechsel:** Geteiltes Leid auf den anderen Tank nach einem Tankbuster, der dich in Gefahr lässt; zurück
  provoziert, wenn er den nächsten Tankbuster genommen hat und du wieder sicher bist. Option, ab Werk an.
- **Schutzmechaniken mit Auslöser und die Heilung:** Geschützte Tanks sind nachrangige, nicht ausgeschlossene
  Heilziele. Unter Living Dead hält RSR Heilung zurück, solange der Tod noch rechtzeitig kommt (Option, ab Werk aus);
  für The Blackest Night gibt es keine Rückhaltung.

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
braucht, hält RSR auf allen Pfaden zurück. Zwei Optionen, ab Werk an:
- „Spend only the mitigation a measured tankbuster needs": Minderungsdrücke und Zurückhalten; Tanks und Heiler.
- „Use the invulnerability before a tankbuster nothing less survives": die Unverwundbarkeit vor dem Treffer; nur Tanks.

Die Regel sitzt auf der Stufe Tanks: Die Rechnung ist für alle vier gleich, die Aktionen liest sie aus den Wirktexten.
Der Heiler-Teil sitzt auf der Stufe Heiler; Damage Dealer betrifft sie nicht.
- Erkennung in `TankbusterForecast`: Sie liest keine Option und wählt keine Aktion. Sie liest die Casts aller Gegner,
  auch unsichtbarer Helfer, die einen gelisteten oder gemessenen Tankbuster auf ihn wirken (`DataCenter.AreaCasters`,
  A303); vorher sah sie nur anvisierbare Gegner.
- Entscheidung in `CustomRotation_Tankbuster.cs`: `UpdateTankbusterPlan` (je Durchlauf), `TankbusterPlanAbility`
  (Ausführung, Fähigkeiten-Slot 9), `HoldDefenceForTankbuster` (Zurückhalten über `IBaseAction.HoldDefenceOnSelf`,
  Gatter in `BaseAction.CanUse`, Ablehnung „HeldForTheInvulnerability").
- Für Heiler: `StateUpdater.ShouldAddDefenseSingle`.

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
| Z0 kein Plan | kein gemessenes Wirken auf dich | nichts | nichts; unter Heiligem Boden oder Meteoritenfall über den nächsten GCD: alles Bewertete auf dich |
| Z1 gedeckt | eine Unverwundbarkeit steht über H (Status oder eigener Druck) | nichts | alles Bewertete auf dich |
| Z2 Bündel S | das billigste Bündel S übersteht c; S darf leer sein | jedes Glied von S in seinem Fenster | alles Bewertete auf dich außer S; ein Glied von S erst ab seinem Fenster; im letzten GCD nichts mehr, wenn ein Glied fehlt |
| Z3 Unverwundbarkeit | kein Bündel übersteht c, sie ist einen GCD vor dem Einschlag verfügbar | sie, in ihrem Fenster; nach einer Ablehnung im nächsten Durchlauf neu | alles Bewertete auf dich, solange R > GCD |
| Z4 alles | kein Bündel, keine verfügbare Unverwundbarkeit | jedes verfügbare Glied in seinem Fenster | nichts |

- **Verfügbar** heißt für die Unverwundbarkeit: Option an, erlernt und freigeschaltet, aktiviert, auf der Leiste, über
  der TTK-Schwelle der Aktion und vor dem letzten GCD abgeklungen (`InvulnerabilityReadyBy`). Das ist alles, was
  RSRs eigene Prüfung ablehnen würde und vorher feststeht.
- **Eine Ablehnung der Unverwundbarkeit beim Druck macht nie Z4.** Was sie im Spiel ablehnt – Stun, Amnesie –,
  lehnt jede andere Fähigkeit ebenso ab (Mechanik unten). „Alles" könnte also auch nicht hinausgehen. Gälte das
  einmal nicht, endet das Zurückhalten im letzten GCD, und die anderen Pfade sind frei.
- **Ablehnung einer Minderung:** Lehnt `CanUse` ab, ohne dass Animationssperre oder eigener Zauber das erklärt, wird sie
  einen GCD lang nicht gedrückt. Für die Frage, ob ein Bündel reicht, zählt sie weiter, solange ihre Sperre vor dem
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

**H, der Horizont:** Restzeit des Wirkens R plus ein GCD für das Eintreffen des Treffers. *Offen (`TODO.md`, „der GCD
als Ersatz"):* Nach dem Snapshot-Beleg (Mechanik) genügt das Ende des Wirkens samt Laufzeit; H ist bis zu einen GCD
zu streng, die Richtung ist zu viel Mittel.

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
  einem GCD (`DefensiveValues.DurationOf`). Früher liefe sie vor dem Einschlag ab. Die Unverwundbarkeit ebenso.
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

### Spielerweite Sperre und Frist (geplant, nicht gebaut)

**Stand:** Gebaut ist nur der Teil für die Unverwundbarkeit (Zustände, „Ablehnung der Unverwundbarkeit"). Für
Minderungen gilt noch die Sperre für einen GCD, und der Plan drückt bis zum Ende des Wirkbalkens (`TODO.md`).

**Geplante Regeln:**
1. *Erkennung (Stufe „alle", zentral):*
   - Ob der Spieler gerade keine Fähigkeit benutzen kann, und wie lange noch: ein Status mit `LockActions` aus dem Blatt
     des Clients (Fähigkeitsprüfung statt Liste, sprachunabhängig) oder Amnesie (Wirktext der englischen Fassung).
   - Die Restzeit kommt aus dem Status, wo das Spiel sie nennt; sonst bleibt sie unbekannt.
   - Die Erkennung urteilt nicht; die Verbraucher entscheiden.
2. *Ablehnung unter Sperre:* keine Antwort, wie die Animationssperre; Neuversuch im nächsten Durchlauf, für jedes Glied
   und für die Unverwundbarkeit.
3. *Frist je Wirken:* Ein Druck für c hilft nur, wenn er vor dem Snapshot beim Server ankommt.
   - Frist = Ende des Wirkens weniger der gemessenen Laufzeit von Druck bis Bestätigung. Der Datensatz kennt den
     Zeitpunkt des Drucks und das Ereignis der Bestätigung, speichert dessen Zeitpunkt aber noch nicht. Vor der ersten
     Messung gibt es keine Frist vor dem Ende des Wirkens.
   - Für Holmgang (und, wenn belegt, Totenerweckung): Frist = Zufügen des Schadens, gemessen am Treffer selbst (Zeit
     vom Ende des Wirkens bis zum Effekt, je Aktion gespeichert). Ohne Messung gilt die Snapshot-Frist.
4. *Nach der Frist* drückt der Plan für c nichts mehr – keine Unverwundbarkeit, kein „alles" (seine Hinweise) – und
   das Zurückhalten für c endet. Was danach kommt, etwa Auto-Angriffe auf einen tiefen Tank, entscheiden die
   gewöhnlichen Pfade mit eigenen Gründen.
5. *Druckplätze:* Von jetzt bis zur Frist passen so viele Drücke, wie die gemessene Animationssperre je Druck zulässt;
   während einer Sperre zählen sie ab ihrem Ende.
   - Ein Bündel ist nur wählbar, wenn seine ungedrückten Glieder hineinpassen; sonst die Unverwundbarkeit (ein Platz),
     ohne sie „alles", das wertvollste Glied zuerst.
   - Ist das Ende der Sperre unbekannt, rechnet der Plan, als ende sie jetzt; er rechnet in jedem Durchlauf neu.
6. *Sperre über die Frist hinaus:* Für c gibt es nichts zu tun.

**Im Kampf:**
- Nach einem kurzen Stun gehen die Glieder im ersten Durchlauf danach; bleiben zu wenige Plätze, geht die
  Unverwundbarkeit allein.
- Endet er nach dem Snapshot, geht für diesen Treffer nichts hinaus.
- Ein Druck, der nach dem Snapshot ankäme, unterbleibt.

**Klasse über den Plan hinaus:** Jeder Pfad, der für einen vorhergesagten Treffer drückt (Fenster aus BossModReborn, die
Einzelabwehr der Heiler), kann nach dessen Frist drücken. Regel 4 gehört deshalb auf die Stufe „alle". Für
BossModReborn ist unbelegt, ob dessen Zeitangabe den Snapshot oder das Zufügen meint; erst nach diesem Beleg ist sie
dort anwendbar.

**Warum so, und was ausgeschlossen ist:**
- Vorbeugend vor einem Stun zu drücken scheidet aus: Ein Stun ist nur über ein BossModReborn-Modul vorhersagbar, und
  dafür gibt es keinen Weg ohne BossModReborn.
- Die Sperre an `GetActionStatus` zu erkennen scheidet aus: Welche Rückgabe einen Stun meint, ist unbelegt; der Status
  ist belegt und nennt die Restzeit.
- Ein falsch erkannter Status mit `LockActions`, unter dem Fähigkeiten doch gehen, kostet nichts: keine Sperre
  notiert, Neuversuch in jedem Durchlauf. Eine übersehene Sperre lässt das heutige Verhalten stehen – sichere
  Richtung.
- Die Frist kommt aus der gemessenen Laufzeit, nicht aus einer festen Marge; läge der Snapshot später als belegt,
  unterbliebe sonst ein Druck, der noch gewirkt hätte.
- Sichtbar wird die Änderung nur nach einer Ablehnung, im Rückfall „alles" und bei kurzem Wirken, denn der Plan drückt
  sonst früh. Die Plan-Zeile in `DefenseTrace.log` erhielte die Frist und je Druck den Abstand zu ihr; ein Druck nach
  der Frist wäre dann im hochgeladenen Protokoll ablesbar.

**Prüfmittel vor dem Code:** `tankbuster_model.py` bekommt einen Stun, der alle Pfade sperrt (im Fenster, über den
Snapshot hinaus, über das Fenster eines Glieds), die Prüfungen „kein Druck für c nach der Frist" und „bleibt nach einer
Sperre ein Platz und ist die Unverwundbarkeit verfügbar, überlebt der Tank, wenn kein passendes Bündel bleibt", und
die GCD-Sperre unter Stun sowie einen Druck nach der Frist als Defekte des Selbsttests.

### Die Abwehr der vier Tanks

„Kandidat": kann Glied eines Bündels sein. „Gehalten": das Zurückhalten erfasst sie. Werte und Dauern aus den
Wirktexten über den Generator (`DefensiveValues.g.cs`); wo der Wirktext die Dauer offen lässt, aus dem Merkmal, das
sie setzt – zur Laufzeit aus dem Spiel (`TraitDurations`), sonst aus `trait_durations.json`. Reichweite, Radius,
Abklingzeit und Gruppe aus den Spieldaten (xivapi, Blatt `Action`, 12.10.2026).

| Job | Aktion | Wirkung laut Wirktext | Dauer | Reichweite/Radius | Abklingzeit, Gruppe | Rolle | Gehalten |
|---|---|---|---|---|---|---|---|
| alle | Schutzwall | −20 % | 20 | 0/0 | 90, 47 | Kandidat | ja |
| alle | Reflexion | Gegner −10 % | 15 ab Stufe 98, darunter unbekannt | 0/5 | 60, 45 | Kandidat ab 98 | ja |
| alle | Abtausch | Rückstoßschutz, Slow | – | 0/0 | 120, 49 | unbewertet | nein |
| PLD | Sentinel / Guardian | −30 % / −40 % | 15 | 0/0 | 120, 20 | Kandidat | ja |
| PLD | Bulwark | Block | 10 | 0/0 | 90, 16 | kein Kandidat (Block lässt sich nicht rechnen) | ja |
| PLD | Sheltron / Holy Sheltron | −15 % (Zusatzwirkung nicht gezählt) | 6 ab Stufe 74 / 8 | 0/0 | 5, 3; Eid 50 | Kandidat | ja |
| PLD | Intervention, Cover | Intervention −10 % auf ein anderes Mitglied (weitere −10 % mit Schutzwall oder Guardian, nicht gezählt); Cover übernimmt Treffer | 8 ab 82 / 12 | 30 bzw. 20/0 | 10 bzw. 120; Eid 50 | Hilfe für andere | nein |
| PLD | Divine Veil, Passage of Arms | Gruppe | – | 0/30 bzw. 8 | 90 / 120 | Gruppenwerkzeug | nein |
| PLD | Heiliger Boden | unverwundbar | 10 | 0/0 | 420, 25 | Unverwundbarkeit | – |
| WAR | Rachsucht / Verdammnis | −30 % / −40 % | 15 | 0/0 | 120, 22 | Kandidat | ja |
| WAR | Urinstinkt / Urimpuls | −10 % | 6 / 8 | 0/0 | 25, 7 | Kandidat | ja |
| WAR | Urflackern | −10 % auf ein anderes Mitglied | 8 ab 82 | 30/0 | 25, 7 | Hilfe für andere | nein |
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
| Ablehnung | `CanUse` falsch außerhalb Animationssperre und eigenem Zauber | Plan | – |
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
  Für Totenerweckung, Heiligen Boden und Meteoritenfall ist unbelegt, ob Snapshot oder Zufügen zählt.

### Grenzen

**Die Rechnung irrt zu viel Mittel,** wo der Höchstwert über dem typischen Treffer liegt, eine Heilung vor dem
Einschlag nicht zählt, Zusatzwirkungen fehlen, H einen GCD zu streng ist und ein Wert unter Verwundbarkeit mit mehr
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
- *Stufensynchron:* Abwehr zählt nur, wenn erlernt; Merkmalsdauern unterhalb ihrer Stufe sind unbekannt, die Aktion
  dann kein Kandidat (`TODO.md`). Dass gemessene Anteile auf der Stufe ihres Inhalts vergleichbar bleiben, ist ein
  Schluss, nicht gemessen.
- *Mehrere Wirken auf dich:* geplant wird für das früheste gemessene; zurückgehalten wird dann nichts. Liegen zwei in
  einem GCD, deckt ein Minderungsplan nur das erste.
- *Ein ungemessenes Wirken oder eine frühere BossModReborn-Vorhersage* neben einem gemessenen: Der Plan drückt, hält
  aber nichts zurück.
- *Ein Tankbuster nur als Marker oder Vorhersage, oder eine noch nie gemessene Aktion:* kein Plan; dann gilt die
  Abwehr ohne Messung (Abschnitt „Krieger: die übrige Abwehr", Staffelung) und die reaktive Unverwundbarkeit unter
  `HealthForDyingTanks`.
- *Tankwechsel:* hält bei bereiter Unverwundbarkeit, die RSR für jeden nächsten Treffer nutzen würde
  (`CustomRotation.InvulnerabilityUsable`).
- *Der Boss trägt den ganzen Kampf ein Damage Up:* kein Treffer wird gespeichert, kein Plan.

**Warum nicht die alte Abwehr:** Sie gibt je Tankbuster eine große Minderung. Ein gemessener Treffer, der zwei braucht,
tötet damit – seine Präzisierung nennt genau das –, und die reaktive Unverwundbarkeit zündet unter der
Sterbe-Schwelle, die ein Treffer von voller Gesundheit in einem Schritt überspringt. Die Staffelung aus seinem
Protokoll vom 01.10.2026 (zwei große auf einen Tankbuster, nichts auf den nächsten) entsteht im Plan nur, wenn jeder
Tankbuster zwei braucht, und dann ist der nächste mit der Unverwundbarkeit oder mit allem geplant.

### Prüfmittel

- `tankbuster_model.py` (CI) setzt die Regeln dieses Abschnitts um – nicht den Code – und lässt sie gegen eine
  simulierte Welt laufen: Bilder unterschiedlicher Länge mit Rucklern, Erhebung nach der Rotation, Laufzeit zum
  Server, Status vor oder nach der Bestätigung, Ablehnungen, ein Stun beim Druck der Unverwundbarkeit, Abschütteln und
  Urflackern als freie Pfade, andere Pfade, die jederzeit drücken wollen, alle vier Schalterstellungen, knappe
  Treffer, bei denen jedes Glied gebraucht wird.
  - Geprüft: überlebt, was der Plan überlebbar nannte (an der wahren Welt); nach der Unverwundbarkeit nichts
    Bewertetes mehr; die Unverwundbarkeit nie, solange ein Bündel reicht; jedes gedrückte Glied steht am Einschlag und
    wird bis dahin genau einmal gezählt; ein freier Pfad macht aus einem überlebten Treffer keinen Tod und erzwingt
    keine Unverwundbarkeit; „alles" nur, solange die Unverwundbarkeit nicht verfügbar ist.
  - Der Selbsttest schaltet die Defekte ein, die frühere Fassungen des Codes hatten (Archiv A263, A267); jeder muss
    gefunden werden.
  - Nicht abgedeckt, dort trägt nur die Herleitung: ein zweites Wirken und BossModReborn-Vorhersagen, Befehle, die
    Unverwundbarkeit nach einem Bündel bei fallenden LP, Minderung je Schadensart, die Kopplung über Eid, die anderen
    drei Tanks (gleiche Rechnung, andere Zahlen). Das Modell übernimmt H = R + GCD (`TODO.md`, „der GCD als Ersatz").
- `DefenseTrace.log` schreibt je Wirken auf dich „tankbuster coming at you: … takes …% … you have …%", je Änderung des
  Plans „tankbuster plan for … : Rampart + Damnation, leaving …% against …%" (oder „survivable as it stands, nothing
  spent", „the invulnerability already covers it, nothing else", „nothing less survives it: the invulnerability",
  „nothing survives it and no invulnerability, everything goes (…)"), je Ablehnung einer Minderung eine Zeile und je
  Druck die „used"-Zeile. Nachgesteuert wird über die Tabelle: Jeder höhere Treffer hebt ihren Wert, und der Plan
  rechnet je Durchlauf neu.
- Prüfgrad: Modell, Prüfskripte, Compile über die CI; im Spiel nicht beobachtet.

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
  Upstream-Bauform der Aktion, nicht dieser Regel (TODO).
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
- **Nicht gebaut, mit Quelle:** Nascent Flash als Minderung für den **anderen Tank vor dessen Tankbuster**.
  The Balance, Warrior Basic Guide, Abschnitt Makros (abgerufen 02.10.2026): „Nascent Flash goes on a friend. Most
  often on the co-tank of your eight-person party." Damit ist der Einsatz am anderen Tank die Regelanwendung der
  Referenz, kein Schluss mehr. Gebaut ist er nicht: Er braucht die Erkennung „Tankbuster auf dem anderen Tank"
  (Marker oder Vorhersage mit Zielangabe) und die Abwägung gegen den eigenen Bedarf, die die Triage oben schon
  führt. Steht im TODO als offene Arbeit.

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
auf ihn (Marker, gelisteter Zauber, Vorhersage innerhalb von „Seconds before tankbuster to use single mitigation",
ab Werk 3 s) und für einen Pull (`TankPullOnPlayer`: mindestens „Number of hostiles" Gegner auf ihm in 3 Yalm, er wird
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
5. **Verdammnis/Rachsucht** und **Schutzwall** ohne Vorhersage, gestaffelt: Verdammnis, wenn Schutzwall bereit ist
   oder vor mehr als 60 s fiel; Schutzwall, wenn Verdammnis vor mehr als 30 s fiel (unterhalb der Stufe von Rachsucht
   ohne Bedingung). Beide schließen einander über `StatusHelper.RampartStatus` aus.
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
| Reflexion und Abschütteln auf demselben Raidwide | mal stapeln, mal verteilen, je nach Größe | verteilt (je 10 s Sperre gegeneinander) | — | **beide auf denselben** — offen (TODO) |

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
der unter `HealthForDyingTanks` fällt (`ObjectHelper.CanProvoke`). Solange du selbst nach einem Tankbuster in Gefahr
bist, tut sie das nicht mehr. Ohne diese Sperre hätte sie dir den Gegner zurückgeholt, während dein Debuff noch
läuft – genau das, was seine Vorgabe ausschließt. Diese Sperre gilt unabhängig von der Option, weil sie eine
bestehende Regel an seine Vorgabe bindet.

**Die Zielwahl:** unter den anderen Tanks der Gruppe die, die lebend, anvisierbar, in Reichweite sind, selbst keine
Verwundbarkeit tragen und den Gegner nach der Übertragung behalten würden. Davon der, den die wenigsten Gegner
angreifen, bei Gleichstand der mit den meisten LP. „Am wenigsten Aggro" lese ich als „am wenigsten belastet" (meine
Ableitung, nicht seine Regel): Wörtlich – die geringste Feindseligkeit auf diesem Gegner – wäre es genau der Tank, bei
dem Geteiltes Leid am wenigsten bewirkt. Die Frage stellt sich nur bei drei und mehr Tanks in einer Gruppe; in Achter-
Inhalten gibt es genau einen anderen.

### Mechanik

Belege abgerufen 04.10.2026.

- **Geteiltes Leid** (Shirk, #7537): „Du überträgst 25 % deiner Feindseligkeit auf das ausgewählte Gruppenmitglied",
  St. 48, 120 s, 25 y (Job-Guide deutsch und englisch). Nicht auf dich selbst, nicht auf Allianzmitglieder (xivapi:
  `CanTargetSelf`, `CanTargetAlliance` falsch). In Allianzraids ist der andere Tank also nie erreichbar.
- **Herausforderung** (Provoke, #7533) setzt den Wirker an die Spitze der Feindseligkeitsliste – das Werkzeug der
  *anderen* Seite eines Tankwechsels.
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
- **Haltung:** RSR schaltet die Haltung nur ein, wenn kein anderer Tank sie trägt. Ein RSR-Co-Tank liegt daher
  meist weit unter der Hälfte, und die Regel hält. Das ist richtig: Ohne Haltung behielte er den Gegner nicht.
- **Automatische Herausforderung** (`ObjectHelper.CanProvoke`, bei zwei Tanks nur mit „Auto provoke when there is
  another tank in party") greift bei Gegnern auf Nicht-Tanks und nimmt einem Co-Tank unter `HealthForDyingTanks`
  den Gegner ab. Den zweiten Fall sperrt die Regel, solange du selbst in Gefahr bist (oben, „Kein Zurückholen").
- **Allein, Vierer-Instanz, Allianzraid:** Es gibt keinen anderen Tank in der Gruppe, die Regel ist still.
- **BossMod ohne Modul:** Die Regel liest keine Vorhersage; Erkennung über Liste, Marker und Effektsatz.
- **Stufensynchron:** Geteiltes Leid erst ab St. 48 (`EnoughLevel`), darunter still.

### Begründung und Grenzen

- Bei geplanten Wechseln festigt die Regel nur, was ohnehin geschieht. Den Ausschlag gibt der ungeplante Fall: ein
  Tankbuster, für den die Abklingzeiten fehlen. Seine Kämpfe vom 01.10.2026 zeigen „Schramme" zweimal mit 85 %, beide
  Male ohne vorher gewählte Abwehr; die 30 bis 39 % davor und danach kamen nach Verdammnis und Schutzwall. Dass beim
  85-%-Treffer keine Minderung bereit war, ist geschlossen, nicht gemessen; ob dort ein zweiter Tank in der Gruppe war,
  zeigt das Protokoll nicht. Wie oft der Fall vorkommt, ist offen.
- Die Herausforderung des Co-Tanks wirkt sicherer, aber die drückt der andere Spieler. Die Gegenseite gibt es zum
  Teil: Fällt der Co-Tank unter `HealthForDyingTanks`, provoziert RSR. Trägt er nach einem Tankbuster eine
  Verwundbarkeit, ohne tief zu stehen, fehlt sie (`TODO.md`).
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
ankommt, ist nicht belegt; fehlt die Zeile, ändert das nichts an der Entscheidung.

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
den der Plan des Tanks rechnet (Abschnitt „Gemessener Tankbuster: das geringste Mittel"). Die Living-Dead-Regel liest
ihn nicht; ob sie es sollte, ist offen (`TODO.md`). Für Marker, Vorhersagen und nie gemessene Aktionen bleibt die
Prüfung unmöglich.

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


**Der Hebel ist die Option, nicht der Grenzwert.** Zwei Mechanismen, je nach Stellung von
`WithholdHealingForLivingDead`:
- **Option aus:** `StateUpdater.ShouldHealSingle` senkt die Schwelle unter einem Schutzstatus auf
  `HealthProtectedRatio` (0,15), solange Living Dead mehr als zwei GCDs Restzeit hat; danach kehrt die
  normale Schwelle zurück, unabhängig von der Gesundheit.
- **Option an:** Der Träger wird gar nicht geheilt, solange das Todesfenster läuft
  (`IsHeldForDeathTrigger`). Das Fenster endet zwei GCDs vor Ablauf, außer der Träger steht auf oder
  unter `HealthForDyingTanks` (`DeathStillLikely`, A88) — dann läuft es bis zum Ablauf.

`HealthProtectedRatio` anzuheben verschöbe den ersten Mechanismus und heilte **früher** im Fenster,
also gerade den Tod weg, auf den die Regel wartet. Wer den Todeseffekt will, schaltet
`WithholdHealingForLivingDead` ein; der Grenzwert ist nur für die
übrigen Invulnerabilitäten der Liste maßgeblich (Holmgang, Superbolide, Hallowed Ground), bei denen
kein Tod gewollt ist. Upstream heilt ein Ziel unter Invulnerabilität gar nicht; die Absenkung ist die
mildere Fassung. Ein zu früh gesetzter Living Dead (vom Auftraggeber bei 70 % im Wall-to-Wall
beobachtet) kostet damit zehn Sekunden automatischer Heilung ohne Anlass.

**Welche Abwehr des Dunkelritters die Heilentscheidung berührt** (erhoben A141):

| Fähigkeit | Pfad | Wirkung auf die Heilschwelle |
|---|---|---|
| Living Dead | `NoNeedHealingStatus` → `HealthProtectedRatio` | 0,15 statt der normalen Schwelle, wie oben |
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
| Historie der Gesundheit über die Zeit | **für Gegner und Gruppe** | `DataCenter.RecordedHP`, gefüllt in `TargetUpdater.UpdateTimeToKill` aus `AllHostileTargets` **und** `PartyMembers` |
| Zeit bis zum Tod eines Gruppenmitglieds | **ja** | `ObjectHelper.GetCorrectedTTK`, gegen den eigenen Vorhersagefehler kalibriert |
| Abtastrate der Historie | **1 Hz** | `TimeToKillUpdateInterval` |
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
