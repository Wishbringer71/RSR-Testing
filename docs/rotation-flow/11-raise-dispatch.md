# 11 · Wiederbelebung: Auswahl und Ausführung

**Leitfrage:** Wann und auf welchem Weg fällt die Wiederbelebung?

## Sachstand

Spontanität für eine anstehende Wiederbelebung fällt im Einschiebefenster, dort, wo die Ausführungsschicht eine
Fähigkeit überhaupt durchlässt (`RaisePendingAndCastable` in `EmergencyAbility`); der GCD-Pfad bleibt unangetastet.
**Im Spiel bestätigt** (`AUDIT_LOG.md` A73): Die Wiederbelebung fällt teils sofort, teils nach wenigen Sekunden, und
keine andere Fähigkeit bleibt aus. Ohne diesen Zweig kam sie im automatischen Betrieb nach „über 15 Sekunden oder
länger" oder gar nicht, weil Auswahl und Ausführung dieselbe Uhr mit einander ausschließenden Bedingungen lesen
(„Ursache").

**Die verbleibenden Sekunden sind Bauart, kein Rest des Defekts.** Der Einschub greift nur bei
`WeaponRemain > 0,5 s`, danach geht die Wiederbelebung als nächster GCD hinaus — dazwischen liegt die
Restzeit des laufenden GCD. Mit der Vorgabe `RaisePlayerFirst = aus` kann zusätzlich eine Heilung
dazwischenkommen. Eine Verkürzung darüber hinaus ist eine Entscheidung über diese Einstellung, keine
Fehlerbehebung. Daneben stehen vier Korrekturen an den Bedingungen, unter denen überhaupt
entschieden wird, wer wiederbelebt: die Hartwirk-Zweige, die Bezugsmenge der Nur-Heiler-Modi, die
Verdrahtung der Phönixfeder und deren Zieleignung.

## Ursache

`WeaponRemain` ist derselbe Wert wie `DataCenter.DefaultGCDRemain`
(`WeaponRemain` in `CustomRotation_OtherInfo.cs`). Auswahl und Ausführung messen dieselbe Größe, und ihre
Bedingungen schließen einander aus:

| Stelle | Bedingung | Schicht |
|---|---|---|
| `CustomRotation_GCD.RaiseSpell`, Zweig `RaisePlayerBySwift` | wählt Spontanität **nur** bei `WeaponRemain <= 0.5f` | Auswahl |
| `RSCommands_Actions.DoAction` | verweigert **jede** Fähigkeit bei `0 < DefaultGCDRemain <= 0.5f` | Ausführung |
| `RSCommands_Actions.cs:46` | dieselbe Sperre im Klick-Gate (`DefaultGCDRemain`) | Ausführung |
| `CustomRotation.Ability`, Einschiebesperre (`WeaponRemain <= 0.5f`) | dieselbe Sperre im Fähigkeiten-Dispatcher | Auswahl |

Spontanität ist eine Fähigkeit. Die beiden Fenster decken sich bis auf den einzigen Punkt
`DefaultGCDRemain == 0`. Daraus folgt das beobachtete Verhalten vollständig:

- `> 0,5 s`: Spontanität wird nicht einmal gewählt. `RaiseSpell` liefert `false`, der Dispatcher
  fällt durch, Heilung oder Schaden gewinnt den GCD.
- `0 – 0,5 s`: Spontanität wird gewählt und im Vorschaufenster angezeigt — und von `DoAction`
  verworfen. In diesem Frame geschieht nichts, denn der GCD-Pfad hat mit `return act` abgebrochen.
- genau `0`: der einzige Zustand, in dem Auswahl und Ausführung zusammenpassen.

Bewegung spielt keine Rolle: Spontanität hat keine Wirkzeit, die Wirkzeitsperre in
`ActionBasicInfo.NeedsCasting` wird gar nicht erreicht.

### Der Beleg aus dem Spiel

Der Auftraggeber hat berichtet, dass die Wiederbelebung sofort erfolgt, sobald er von automatisch
auf manuell stellt und den Toten anvisiert. Das ist ein natürliches Experiment und bestätigt die
Ursachenanalyse: `ActionTargetInfo.cs:105` (`IsManual`) lässt im manuellen Modus ein **feindliches** Ziel nur zu,
wenn es das angewählte Hauptziel ist. Wer einen Toten anvisiert, hat kein feindliches Ziel, sämtliche
Angriffsaktionen fallen aus, der GCD bleibt frei, `DefaultGCDRemain` steht auf genau `0` — der eine
Punkt, an dem beide Fenster zusammenpassen.

Im automatischen Betrieb wird jeder frei werdende GCD sofort mit einem Angriff belegt; der Nullpunkt
ist praktisch immer von laufender Aktion, Animationssperre oder Klickverzögerung überdeckt. Daher
„dauert sehr lange" statt „geht nie".

### Entstehung

Die Konstruktion war bei ihrer Einführung richtig und wurde durch eine spätere Änderung an anderer
Stelle unrichtig, ohne dass etwas fehlschlug — Parnas' *Lack of Movement*.

- `f22be318`, 10.01.2025: der Spontanitäts-Zweig im GCD-Pfad mit `WeaponRemain <= 0.5f`. Eine
  Ausführungssperre für Fähigkeiten in diesem Fenster gab es noch nicht.
- `92f109d3`, 02.03.2026, vierzehn Monate später: die Sperre in `DoAction` kommt hinzu. Sie ist für
  sich richtig — sie verhindert, dass ein eingeschobenes oGCD den nächsten GCD verzögert — und
  entwertet dabei den Zweig von 2025.

Daraus folgt, dass die Behebung nicht die Zahl `0.5f` verschieben darf. Das reproduzierte dieselbe
Kopplung an einem anderen Punkt. Zu ersetzen ist die Zuständigkeit für das Einschieben.

## Die Regel

Der Fähigkeitenpfad zündet Spontanität bereits für die Wiederbelebung, aber nur, wenn diese schon
als nächster GCD gemeldet ist. Das kann den Ablauf nie **starten**, weil die Meldung ihrerseits
Spontanität voraussetzt. Ergänzt ist genau diese Lücke: Spontanität fällt auch dann, wenn eine
Wiederbelebung ansteht und wirkbar wäre (`RaisePendingAndCastable`).

Was dabei bewusst **nicht** geschieht, ist die Lehre aus einer Regression, die im Spiel des
Auftraggebers Schaden angerichtet hat und vollständig zurückgenommen wurde (`AUDIT_LOG.md` C37):

| | Verworfener Entwurf | Geltender Entwurf |
|---|---|---|
| GCD-Pfad | meldete die Wiederbelebung als nächsten GCD | unverändert |
| Dispatcher | endete im Wiederbelebungsblock vor Heilung und Schaden | läuft normal durch |
| `nextGCD` | wurde zur Wiederbelebung umgeschrieben — **447 Fundstellen** im Baum lasen ihn (gezählt 10.09.2026), darunter Schimmerschild beim Beschwörer | unverändert |
| Dauer | griff dauerhaft, solange ein Toter dalag | höchstens ein Frame je Gelegenheit, danach durch `HasSwift` gesperrt |

Die daraus folgende Regel: Wer eine Meldebedingung lockert, misst ihre Wirkung an beiden Pfaden und
zählt vorher aus, wer `nextGCD` liest.

**Warum der Zweig im richtigen Fenster läuft.** `Ability()` kehrt bei `0 < WeaponRemain <= 0.5f`
sofort zurück, und bei freiem GCD ruft `Invoke` den Fähigkeitenpfad gar nicht erst auf. Der Zweig
kann also nur bei `WeaponRemain > 0,5 s` greifen — dem Einschiebefenster, in dem die
Ausführungssperre nicht gilt.

**Warum er nichts verdrängt.** Nach ihm stehen in `EmergencyAbility` nur zwei Zweige, beide Second
Wind für Nahkämpfer beziehungsweise physische Fernkämpfer bei Doom-Status. Für einen Rezzer ist dort
nichts, was ausfallen könnte. Verbraucht wird ein Einschiebefenster, einmal je Gelegenheit.

**Der Zielüberschreibungs-Fallstrick.** Wiederbelebungsaktionen führen keinen eigenen Zieltyp, sie
sind nur `IsFriendly`; ihr Ziel stammt aus `TargetType.Death`, das ausschließlich der GCD-Pfad setzt.
Eine Wirkbarkeitsprüfung im Fähigkeitenpfad ohne diese Überschreibung durchsucht die falsche Menge —
und `CanUse` weist als Nebenwirkung `Target` zu, ließe die Aktion also auf ein fremdes Ziel zeigen.
`RaisePendingAndCastable` setzt die Überschreibung selbst und stellt den vorherigen Wert wieder her,
statt ihn zu löschen, weil der Fähigkeiten-Dispatcher eigene Überschreibungen um seine Zweige legt.

Für **Gegenstände** gilt das Umgekehrte: `BaseItem` liest `DataCenter.DeathTarget` in `CanUse` wie in
`Use` unmittelbar und kennt das Überschreibungssystem nicht. Eine Überschreibung um den Federzweig
wirkte deshalb nichts und täuschte eine Zielwahl vor, die nicht stattfindet; sie ist entfernt.

## Anwendungsfälle

Die Erhebung folgt den Größen, die den Pfad tatsächlich steuern. Jede Zelle ist am Quelltext belegt;
wo eine Aussage nicht entscheidbar war, steht das ausdrücklich dabei.

### Die zwei Einhängepunkte

Der Wiederbelebungsblock existiert **zweimal**, und eine Einstellung entscheidet, welcher läuft:

| `RaisePlayerFirst` | Ort | Was davor gewinnt |
|---|---|---|
| an | `CustomRotation.GCD`, Zweig nach `ProvokeGCD` | Notfall, Unterbrechung, Reinigung, Provokation |
| **aus (Vorgabe)** | `CustomRotation.GCD`, Zweig nach `DefenseSingleGCD`, vor `GeneralGCD` | zusätzlich `MoveForwardGCD`, **die gesamte Heilung** und die Flächen- und Einzelziel-Verteidigung |

Mit der Vorgabe steht die Wiederbelebung hinter jeder Heilung. Das ist die dokumentierte Bedeutung
der Einstellung und kein Defekt — es erklärt aber, warum ihre Wahl das beobachtete Verhalten stark
verändert.

### Zustand von Spontanität

| Zustand | Weg | Ergebnis |
|---|---|---|
| läuft (`HasSwift`) | Stufe (A) in `RaiseSpell` | Wiederbelebung sofort, ohne Wirkzeit |
| bereit, Einstellung an | Einschiebefenster (`EmergencyAbility`) | zündet, danach Fall 1 |
| in Erholung, Einstellung an | Hartwirk-Zweig | Hartwirk, **nur im Stehen** — Stufe (C) verlangt `!IsMoving` |
| bereit, Einstellung aus | Hartwirk-Zweig über `!SwiftcastComingForRaise` | Hartwirk im Stehen |
| `NoHardCast` **und** Einstellung aus | keiner | keine Wiederbelebung — beide Wege bewusst abgeschaltet, kein Defekt |

Der Rotmagier fällt aus der Reihe: Dualcast steht in `StatusHelper.SwiftcastStatus`, also ist
`HasSwift` nach jedem Zauber wahr und Stufe (A) greift ohnehin. Für ihn war der Pfad nie defekt.

**Die Hartwirk-Zweige verlangten einen Zustand, den sie selbst verhinderten.** Alle sechs feuerten
nur bei `SwiftcastPvE.Cooldown.IsCoolingDown`. Ist `RaisePlayerBySwift` abgeschaltet, zündet die
Rotation Spontanität nie — für einen Heiler sind die beiden anderen Zünder auf `JobRole.RangedMagical`
eingeschränkt —, die Erholung tritt nie ein, und es wurde überhaupt nicht wiederbelebt. Die
Beschreibung der Einstellung sagt zu, Spontanität nicht *dafür* zu verwenden, nicht, das
Wiederbeleben einzustellen.

Die Behebung unterscheidet zwei Zweigarten, weil sie verschiedene Fragen stellen:

- `HardCastNormal` (in beiden Raise-Zweigen von `CustomRotation.GCD`) fragt über `SwiftcastComingForRaise`, ob die Rotation
  Spontanität überhaupt noch für diese Wiederbelebung ausgeben wird. Nur wenn nicht, wird hartgewirkt.
- `HardCastSwiftCooldown` und `HardCastOnlyHealerSwiftCooldown` (ebenda)
  wägen ab, ob die Wirkzeit kürzer ist als die Wartezeit auf Spontanität. Diese Abwägung bleibt
  unangetastet; ergänzt ist allein der Fall `!RaisePlayerBySwift`, in dem nichts kommt, worauf zu
  warten wäre.

Die naheliegende Vereinheitlichung — `!SwiftcastComingForRaise` in allen sechs Zweigen — wäre falsch
und ist verworfen: Sie hätte in den vier Abwägungszweigen sofort hartgewirkt, sobald Spontanität in
Erholung ist, und damit genau die Abwägung entfernt, die dem Wahlwert seinen Namen gibt.

Die Verhaltenswirkung der Korrektur ist auszählbar und beschränkt sich auf **eine** von vier
Kombinationen:

| Einstellung | Erholung | ohne die Korrektur | mit ihr | |
|---|---|---|---|---|
| an | ja | wirkt | wirkt | gleich |
| an | nein | wirkt nicht | wirkt nicht | gleich |
| aus | ja | wirkt | wirkt | gleich |
| aus | nein | **wirkt nicht** | **wirkt** | der Fall, der niemanden wiederbelebte |

Bei eingeschalteter Einstellung — der Vorgabe — ist das Verhalten unverändert.

### `HardCastRaiseType`

| Wert | Optionstext | Code prüft | Bewertung |
|---|---|---|---|
| `NoHardCast` | nicht hart wirken | kein Zweig | stimmig |
| `HardCastNormal` | „while Swiftcast is on cooldown" | „kommt Spontanität noch" | stimmig |
| `HardCastSwiftCooldown` | „… and cooldown is higher than raise cast time" | genau das | stimmig |
| `HardCastOnlyHealer` | „while Swiftcast is on cooldown and other **healers** are dead" | Reservierung und Heiler-Bedingung | stimmig |
| `HardCastOnlyHealerSwiftCooldown` | beides, ebenfalls „healers" | beides | stimmig |

Die Reservierung ist in allen vier Hartwirk-Modi geprüft; das war zeitweise nur bei dreien der Fall.

**Der zweite Halbsatz zählt Heiler, nicht Rezzer — Vorgabe des Auftraggebers.** Die weitere Menge
hätte das Hartwirken zurückgehalten, weil ein Beschwörer lebt, also aufgrund einer Annahme darüber,
was ein fremder Spieler mit einer Leiche tun wird. Im Kampf ist das der Unterschied, ob der Tote in
der Achtergruppe jetzt aufsteht oder auf jemand anderen wartet. Seine Anweisung ist der Optionstext:
Diese Modi warten auf Heiler und auf sonst niemanden. Die Stufenprüfung bleibt in beiden Fällen — ein
Heiler unterhalb der Wiederbelebungsstufe hat nichts, worauf zu warten wäre.

**Die Feder behält die weite Menge**, weil seine Vorgabe dort die umgekehrte ist und ausdrücklich
vom Rezzer spricht: Eine Phönixfeder fliegt, wenn niemand mehr überhaupt wiederbeleben kann.

**Die Reservierung fragt zusätzlich, ob Spontanität dem Spieler überhaupt zur Verfügung steht.**
`IsCoolingDown` liest den Recast-Zeitgeber, und eine Aktion, die der Spieler nicht wirken kann, hat
keinen laufenden. Ohne eine Stufenprüfung meldete der Ausdruck für jeden Rezzer unterhalb der
Spontanitätsstufe — auch für den gesynchten in älterem Inhalt — dauerhaft „Spontanität ist bereit",
schloss damit jeden Hartwirk-Zweig, während der Spontanitätspfad an `CanUse` scheiterte: **niemand
wurde wiederbelebt.** Die Wiederbelebung kommt im Stufenaufstieg deutlich vor der Spontanität, die
Lücke ist also eine ganze Stufenspanne und kein Randfall. `SwiftcastComingForRaise` und
`HardCastBeatsWaitingForSwiftcast` prüfen deshalb `EnoughLevel` mit; auf voller Stufe ändert das
nichts (A86).

### Die Bezugsmenge: wer gilt als lebender Rezzer

`HardCastOnlyHealer` und `HardCastOnlyHealerSwiftCooldown` bauten zwei Mengen toter beziehungsweise
vorhandener **Heiler der eigenen Gruppe** und verglichen deren Größe. Die gemeinte Frage ist eine
andere: Hartwirken kostet acht Sekunden GCD und lohnt nur, wenn es sonst niemand übernehmen kann.

| | maß früher | gilt |
|---|---|---|
| Wer zählt | nur Heiler | **Hartwirken: nur Heiler** (Vorgabe, s. o.) · **Feder: Rezzer** — Heiler, Beschwörer, Rotmagier |
| Stufe | ungeprüft | ein Rotmagier unter 64 hat Verraise nicht |
| Bezugsmenge | immer die eigene Gruppe | die Menge, aus der das Ziel stammen darf — also nach `RaiseType` |
| Einzelheiler, solo | beide Mengen leer, `> 0` scheitert → **nie Hartwirk** | niemand sonst da → Hartwirken ist genau richtig |
| Ausdruckskraft | Größenvergleich | „niemand außer mir" |

Beide Fragen — die des Hartwirkens und die der Feder — laufen über
`DataCenter.AnyLivingRaiser(excludeSelf)`. Derselbe Satz Rezzerjobs, dieselbe Bezugsmenge; der
Parameter trägt den einzigen echten Unterschied. Die Feder fragt „kann das niemand richtig" und zählt
den Spieler mit, weil ein lebender Rezzer statt ihrer den Zauber wirkt. Die Nur-Heiler-Modi fragen
„kann es außer mir jemand", und dort ist der Spieler der Entscheidende.

Die Bezugsmenge folgt `RaiseType`. Bei `PartyOnly` und `PartyHealersOnly` bleibt es die Gruppe. Unter
den Allianzmodi und `All` zählt die Allianz mit, weil die anderen Allianzen echte Gruppen mit eigenen
Rezzern sind: Solange dort einer lebt, ist die Wiederbelebung deren Sache; lebt keiner mehr, ist
Hartwirken oder die Feder der einzige Weg, dass dort jemand hochkommt. `AllOutOfDuty` ist ausgenommen
— Fremde in der offenen Welt sind keine Reserve, und sie mitzuzählen blockierte beides praktisch
immer.

**Die Stufe gehört zur Rezzereigenschaft.** `CanRaise()` prüft sie für den Spieler seit jeher: 12 für
Heiler und Beschwörer, 64 für den Rotmagier. Für alle anderen wurde sie nicht geprüft, sodass ein
Rotmagier unter 64 — jeder stufensynchronisierte Durchgang durch ältere Inhalte — als lebender Rezzer
zählte und die Feder zurückhielt, die dort der einzige Weg war. Beide Stellen teilen sich
dieselben zwei Konstanten; das ist die Konstruktion, die nicht wieder auseinanderläuft. Die Stufe
eines Gruppenmitglieds stammt aus `ICharacter.Level`. Dass der Wert in einem synchronisierten Inhalt
die synchronisierte Stufe meldet, setzt WrathCombo voraus: Es zählt den Spieler nur dann als Teilnehmer
einer FATE, wenn `LocalPlayer.Level` höchstens deren Höchststufe ist (`InFATE`) — ein anderes Werkzeug,
keine Spielquelle. Meldete er die wahre Stufe, zählte ein unterstufiger Rezzer weiter als lebender
Rezzer; das ist der Stand vor der Stufenprüfung, nicht schlechter.

### Die Phönixfeder

Verdrahtet im Fähigkeitenpfad (`CustomRotation.Ability`, nach `UseHpPotion`), hinter der Heilung und vor den
Angriffsfähigkeiten: Jemanden am Leben zu halten geht vor, jemanden aufzuheben kostet ein
Einschiebefenster. Die Ausführungssperre schluckt sie nicht, weil diese auf `nextAction is BaseAction`
prüft und ein Gegenstand keiner ist.

Die Bedingung stand schon vollständig im Code (`PhoenixDownItem.CanUseThis`) und entspricht der
Vorgabe des Auftraggebers: Lebt in der Bezugsmenge kein Rezzer mehr, wirft, wer eine Feder hat. Das
Inhaltsverbot braucht keine eigene Prüfung — `BaseItem.CanUse` fragt den Spielclient über
`GetActionStatus`, und ein Inhalt, der Gegenstände verbietet, lehnt dort ab.

**Zieleignung.** Die Frage „darf diese Feder auf diesen Leichnam" wird an den Spielclient gestellt:
`GetActionStatus(ActionType.Item, 4570, Ziel)` muss genau `0` liefern. Damit beantwortet das Spiel
Reichweite, Sichtlinie, Wiederbelebbarkeit des Ziels, Inhaltsverbot und einen bereits laufenden
Wiederbelebungsstatus in einer einzigen Frage. Zuvor stand hier `ObjectHelper.CanBeRaised`, also
`CanUseActionOnTarget` gegen den **Zauber** Wiederbelebung — eine Aktionsfrage an der Stelle einer
Zielfrage, und ausgerechnet für die Jobs, die eine Feder tragen, eine Frage nach einem Zauber, den
sie nicht besitzen. Dass diese Prüfung nicht blockierte, ist aus der Beobachtung des Auftraggebers
belegt: Er erlebt Wiederbelebungen als Beschwörer, der `RaisePvE` nicht besitzt, also antwortet die
native Funktion nicht nach Erlernbarkeit. Der Austausch ist damit ein Gewinn an Genauigkeit, nicht
die Beseitigung einer Sperre. Die strengere Prüfung auf `== 0` statt gegen die Liste `BadStatus` ist
nötig, weil eine zielbezogene Ablehnung in dieser Liste nicht vorkommt und sonst als Erlaubnis
gelesen würde.

**Warum die Feder nicht selbst wirkt.** `UsePhoenixDown` meldet die Feder nur; gewirkt wird sie von
`RSCommands.DoAction`, wie bei jedem anderen Gegenstand. Zuvor rief die Methode `Use()` selbst **und**
setzte `act`, sodass eine Verdrahtung nach dem Hausmuster `UseAction` zweimal für denselben Leichnam
im selben Frame ausgelöst hätte. Der zweite Aufruf ginge nicht verloren: Die Dokumentation von
`UseAction` hält fest, dass eine während einer Sperre eintreffende Aktion **eingereiht** wird. Und
selbst wo das Spiel ablehnt, verbucht `DoAction` das Ergebnis des zweiten Aufrufs als das Ergebnis —
`CurrentAction`, `_lastActionID` und `_lastUsedTime` folgen dann einem Aufruf, der nichts getan hat.
Der Punkt betrifft den Verbrauch und die Buchführung, nicht die Dauer eines Wiederbelebungsvorgangs.

### Zeitlicher Ablauf: Wirken, Landen, Annehmen

Drei Zeitpunkte sind zu unterscheiden, und nur der mittlere ist für die Auswahl maßgeblich.

| Zeitpunkt | Was RSR sieht | Folge für die Zielmenge |
|---|---|---|
| Wirken beginnt | den Cast des Wirkenden samt Ziel (`CastTargetObjectId`) | `GetDeath` schließt das Ziel aus, solange ein anderer Spieler den Cast führt (A293) |
| Wirkung landet | das Ziel trägt `Raise` (148 oder 1140, `StatusHelper.RaiseStatus`) | `GetDeath` schließt es aus (`TargetFilter.cs:105`) |
| Der Tote nimmt an | keine eigene Meldung nötig | schon vorher ausgeschlossen |

Die Zeit bis zur Annahme durch den Gefallenen ist für die Entscheidung damit ohne Bedeutung: Der
Status trägt den Ausschluss, nicht die Annahme. Läuft der Status ungenutzt ab, wird der Leichnam
wieder Kandidat — richtig so.

**Wirkt ein anderer Spieler gerade eine Wiederbelebung auf einen Leichnam, wählt RSR ihn nicht**
(`TargetUpdater.UpdateCastsByOthers`, `DataCenter.RaisedByOthers`, A293). Der `Raise`-Status erscheint erst, wenn
die Wiederbelebung landet; während eines acht Sekunden langen Hartwirkens blieb der Leichnam Kandidat, und RSR ohne
Spontanität begann eine eigene Wiederbelebung auf ihn — zwei Casts für einen Leichnam, der langsamere verloren (seine
Meldung vom 15.10.2026). Den fremden Cast samt Ziel führt der Client für Gruppenmitglieder; BossModReborn liest ihn
ebenso, um ein Mitglied der Esuna eines anderen Heilers zu überlassen (`PartyHealth.Update`, ein anderes Werkzeug,
keine Spielquelle). Für NPC-Heiler (Trust, Duty Support) ist das ein Schluss, nicht belegt; fehlt der Cast, bleibt
das Verhalten wie vorher. Erkannt wird die Wiederbelebung an den Spieldaten wie die eigene
(`DeadTargetBehaviour` 1), also für jeden Job und jede Sonderaktion. Bricht der andere ab, ist der Leichnam beim nächsten
Durchlauf wieder Kandidat; landet seine Wiederbelebung, schließt ihn der Status aus. Gilt auch mit eigener Spontanität:
Swiftcast bleibt für einen Leichnam, den niemand versorgt. Jede Zurückstellung schreibt eine Zeile in
`DefenseTrace.log` („raise left to …", mit Ziel und Restzeit). Dieselbe Erhebung zählt fremde Esuna-Casts je Mitglied;
die eigene Esuna nimmt nur, was nach ihnen an entfernbaren Effekten übrig bleibt.

**Wirken beide schon, bricht RSR die eigene Wiederbelebung ab, sobald die fremde auf denselben Leichnam früher
landet** (`CancelCastUpdater.RaiseLandsFirstElsewhere`, A294). Das ist der Fall, den das Gruppenfenster mit einem
rötlich gefärbten Cast-Balken in der Zielanzeige zeigt (sein Hinweis, 15.10.2026). Die Färbung selbst beschreibt
keine Quelle — weder Patchnotes (6.0 führt nur die Zielanzeige der Casts im Gruppenfenster ein) noch Handbuch,
FFXIVClientStructs oder die Meldungsblätter des Spiels; ein eigenes Datenfeld dafür gibt es in den bekannten
Strukturen nicht. Der Client kann sie nur aus den beiden Casts mit gleichem Ziel ableiten (Schluss), und genau die
liest RSR. Die eigene Wiederbelebung landete bis dahin bis zum Ende und verbrauchte ihr MP an einem Leichnam, der
schon stand; abgebrochen wurde erst, wenn der `Raise`-Status sichtbar war. Gleiche Landezeit lässt den eigenen
Cast laufen: Der fremde Cast erreicht diesen Client später, als er begann, liest sich also als später landend
(Schluss); zwei Clients, die beide warteten, ließen den Leichnam liegen. Bricht der andere nach unserem Abbruch ab,
ist der Leichnam beim nächsten Durchlauf wieder Kandidat. `DefenseTrace.log` schreibt „raise stopped: …" mit dem
anderen Wirker und dem Vorsprung.

**`Raise` steht unter zwei Ids** (148 und 1140, gleicher Name, gleicher Wirktext „Teetering on the brink of
consciousness."; xivapi v2). BossModReborn und DelvUI lesen beide; RSR las in `GetDeath` und im Abbruch nur 148
(A294). Beide Stellen lesen jetzt `StatusHelper.RaiseStatus`.

Zwei Lücken bleiben:

- **Gleiche Landezeit.** Beginnen zwei Wirker im selben Augenblick, laufen beide Casts zu Ende.
- **Die eigene Feder.** Sie hat keine Wirkzeit, die Statusrückmeldung braucht dennoch einen
  Serverumlauf. In dieser Spanne liest der Leichnam auf unserer Seite weiter als tot. Genau dagegen
  steht die neue Zieleignung: Sie fragt nicht die eigene Buchführung, sondern den Client, der den
  eingereihten Vorgang kennt.

### Zielauswahl und `RaiseType`

Geprüft und ohne Befund: `PartyOnly` (Vorgabe) und `PartyHealersOnly` werden in `GetDeathTarget`
getrennt behandelt, die Allianz-Varianten fügen ohne Doppelzählung hinzu (`deathPartyIds`).
`GetPriorityDeathTarget` staffelt Tank, Heiler, Ersatzrezzer und Übrige; die Sonderregel für zwei
tote Tanks ist eine begründete Fallunterscheidung und kein Tippfehler (C36). Die Reihenfolge
entspricht der Vorgabe des Auftraggebers, wonach eine Feder zuerst einen Heiler aufheben soll.

**Vorgabe des Auftraggebers (25.09.2026):** „der nebenheiler soll den hauptheiler vor dem tank
rezzen", präzisiert: „ein tank sollte aggro halten. heiler rezzen ist wichtig, aber wenn man in der
zeit selbst totgeschlagen wird, bringt das nichts." **Der Code erfüllt beides:** Lebt noch ein Tank,
kommt der Heiler vor dem toten Tank; sind beide Tanks tot, zuerst ein Tank
(`deathTanks.Count > 1`, Upstream, C36), weil sonst niemand den Gegner hält.

**Abgleich (02.10.2026):** Das consolegameswiki, „Healer Guide" (Gemeinschaftsquelle), nennt dieselbe Ordnung:
„If both tanks are KO'd, raise a tank first. If both tanks are alive, raise your co-healer", danach Rotmagier und
Beschwörer, dann die übrigen Schadensausteiler — RSRs Staffel Tank/Heiler, Ersatzrezzer, Übrige. Es ordnet davor
„Heal yourself" und „Heal the tank(s)" ein: Lebende zu heilen ist billiger als Tote aufzuheben. Dem entspricht in RSR die Stellung
`RaisePlayerFirst = aus` (Abschnitt „Die zwei Einhängepunkte"); mit `an` steht die Wiederbelebung vor
jeder Heilung — für ihn als Tester sind beide Stellungen möglich. Abweichend vom Wiki: „Always use
Swiftcast to Raise" gilt bei ihm wörtlich — Swiftcast bleibt für Wiederbelebungen.

Die Filter in `GetDeath` sind vollständig und schließen jeweils sinnvoll aus: kein Wiederbelebungs-
oder Verweigerungsstatus, Entfernung über 30 Yalm, fehlende Sichtlinie, Gruppen- oder
Allianzzugehörigkeit. Der Auftraggeber hat bestätigt, dass Leichen ruhig liegen und anvisierbar sind,
womit `IsTargetMoving` und `IsTargetable` als Ursache ausscheiden.

**`H2` folgt seinem Optionstext (A145).** „Raise non-Healers from bottom of party list to the top
(Light Party 2 Healer Behavior)": Bei `H2` werden die Listen der toten Tanks, der Nebenheiler und der
übrigen umgedreht, die der Heiler nicht. Bis A145 drehte der Code auch die Heilerliste; der
Sonderfall für `PartyAndAllianceHealers` stand also schon im Einklang mit dem Text. Vorgabe des
Auftraggebers: Die Regeln der Einstellungstexte sind einzuhalten.

**Der Einschiebezweig für Swiftcast erkennt die Wiederbelebung an `Raise` (A141)**, nicht an einer Liste
von Ids: Eine solche Liste (vier Ids, ohne Verraise und Angel Whisper) war dieselbe Alterungsursache wie die
Hauptursache des Wiederbelebungsdefekts. Im Kampf änderte die Umstellung nichts, weil der zweite Zweig
(`RaisePendingAndCastable`) diese Jobs schon über `Raise` erreichte; ein künftiger Rezzer-Job fällt
nicht mehr still heraus.

## Ausgeschlossen

- **Nullvariante.** Scheidet aus: Der Defekt trifft jede Wiederbelebung jedes Rezzers und hat keine
  Selbstheilung.
- **Auswahlfenster verschieben** (`0.5f` durch einen anderen Wert ersetzen). Behandelt den Fundort,
  nicht die Ursache, und lässt eine oGCD-Zündung im GCD-Pfad stehen, die `Invoke` bei freiem GCD als
  GCD ausführt und sonst durch eine beliebige andere Fähigkeit ersetzt.
- **Die Wiederbelebung als nächsten GCD melden.** Umgesetzt, im Spiel gescheitert, vollständig
  zurückgenommen (C37) — siehe die Vergleichstabelle oben.
- **`IgnoreClipping` in `DoAction` lesen.** Belebte tote Konfiguration, aber mit einem Wirkungsbereich
  weit über die Wiederbelebung hinaus. Bleibt als eigener Punkt erfasst.
- **Ausführungssperre für Spontanität ausnehmen.** Behandelte das Symptom in der falschen Schicht und
  öffnete das Einschiebefenster für eine Aktion, die dort nicht hingehört.
- **`SwiftcastBuffer` verdrahten.** Die Einstellung (0,6 s, eigene Oberfläche, eigene Dokumentation
  „how early before next GCD should RSR use swiftcast for raise") kommt im ganzen Baum genau einmal
  vor: in ihrer Definition. Sie bleibt unverdrahtet, weil ihre dokumentierte Bedeutung mit der
  Ausführungssperre unvereinbar ist — sie besagt, Spontanität solle erst fallen, wenn nur noch
  `SwiftcastBuffer` Restzeit auf dem GCD liegt, und das ist bei 0,6 s fast vollständig und bei 0
  vollständig der Bereich, den `DoAction` sperrt. Sie zu verdrahten hieße, denselben Defekt ein
  zweites Mal zu bauen. Ihre Auflösung ist ein eigener Vorgang und in `TODO.md` erfasst.
- **Ein eigenes Gedächtnis gegen doppelte Federn.** Ein Vermerk „Ziel X wurde gerade bedient" wäre
  eine zweite Buchführung neben der des Spielclients und veraltet auf dieselbe Weise. Die Frage an
  `GetActionStatus` mit dem Ziel leistet dasselbe, ohne einen eigenen Zustand zu führen.

## Der Abbruch eines laufenden Zaubers trifft keine Wiederbelebung

**Der Abbruch bei totem Zielobjekt (`CancelCastUpdater`) gilt nur für feindliche Ziele** (Upstream 7.5.6.10, A119).
Vorher prüfte er allein `castTarget.CurrentHp == 0` — und das trifft **jedes** Wiederbelebungsziel; wer
`UseStopCasting` eingeschaltet hatte, brach damit jede hart gewirkte Wiederbelebung selbst ab.

**Die beiden Behebungen greifen ineinander, und erst zusammen tragen sie:** Der Fork sorgt dafür,
dass Swiftcast für die Wiederbelebung fällt und die Ankündigung nicht auf sich selbst wartet;
solange Swiftcast bereitsteht, ist die Wiederbelebung spontan und war vom Abbruch ohnehin nicht
betroffen. Der Fall, den der Fork allein nicht lösen konnte, ist der **ohne** Swiftcast — genau der,
in dem hart gewirkt wird, und genau der, den der Abbruch traf.

**Der Vorgabewert von `UseStopCasting` ist aus**, die Einstellung des Auftraggebers ist von hier aus
nicht messbar. Wer sie an hat, konnte ohne Swiftcast niemanden hochholen.

## Bekannte Mängel neben diesem Konzept

- **`IBaseAction.IgnoreClipping` wird geschrieben und nirgends gelesen** (fünf Schreibzugriffe in
  `CustomRotation_Invoke.cs`, Definition `IBaseAction.IgnoreClipping`). Wirkungsbereich zu groß für diesen
  Vorgang: Ein Leser in `DoAction` hebelte die Anti-Clipping-Regel überall aus.
- **`Configs.Migrate` ist ein Zurücksetzen, kein Migrationspfad.** Das ist nicht
  nur unbequem, es **sperrt andere Behebungen**: Jede Korrektur, die einen Vorgabewert ändern muss,
  um das bisherige Verhalten zu erhalten, ist ohne Feldmigration nicht durchführbar. Daran ist die
  Verdrahtung von `InterruptDelay` und `ProvokeDelay` gescheitert.
- **`TargetColor` hat keinen Leser.**

## Grenzen des Nachweises

Statische Prüfung am Quelltext, Versionsgeschichte für die Entstehung, Fremddokumentation für
`UseAction`, und die Laufzeitbeobachtung des Auftraggebers für die Wirkung: Spontanität erscheint im
Vorschaufenster und wird nicht gewirkt, unabhängig von Bewegung; im manuellen Betrieb mit
anvisiertem Leichnam fällt die Wiederbelebung sofort.

Für den Kernpfad liegt die Laufzeitbestätigung vor (A73): teils sofortige, teils um
wenige Sekunden verzögerte Wiederbelebung, ohne Ausfall anderer Fähigkeiten. Kein Vier-Augen-Prinzip.

**Die einstellungsabhängigen Eingriffe sind statisch belegt, nicht im Spiel beobachtet:** Federverdrahtung,
Zieleignung über den Gegenstandsstatus, Stufenprüfung der Rezzereigenschaft, Hartwirk-Korrektur, Bezugsmenge und
Swiftcast-Vorbehalt der Nur-Heiler-Modi. Ihre Wirkketten sind am Code nachvollzogen (A286, K7); dass
`GetActionStatus` für einen verwendbaren Gegenstand genau `0` liefert, nimmt WrathCombo ebenso an
(`Items.cs`, Prüfung `== 0` vor dem Trank) — ein anderes Werkzeug, keine Spielquelle. Sie liegen hinter Einstellungen,
die ab Werk aus oder anders stehen; eine Spielbeobachtung ist keine offene Aufgabe.

## Offene Punkte

Sie stehen in `TODO.md` und sind dort unter der Überschrift des Eintrags mit **Konzept:** auf dieses
Dokument gekennzeichnet — an **einer** Stelle statt in zweien, damit keine Kopie altert.
`.github/scripts/audit/check_concept_links.py` listet sie je Konzept und nennt zugleich, wie viele
Einträge überhaupt keinem Konzept zugeordnet sind.
