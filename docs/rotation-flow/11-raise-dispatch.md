# Wiederbelebung: Auswahl und Ausführung

## Sachstand

Die Wiederbelebung kommt im automatischen Betrieb verspätet oder gar nicht zustande, weil Auswahl
und Ausführung dieselbe Uhr mit einander ausschließenden Bedingungen lesen. Die Ursache ist statisch
am Quelltext und durch eine Laufzeitbeobachtung des Auftraggebers belegt.

Die Behebung zündet Spontanität dort, wo die Ausführungsschicht eine Fähigkeit überhaupt durchlässt
— im Einschiebefenster — und lässt den GCD-Pfad unangetastet. Sie ist **im Spiel bestätigt**: Die
Wiederbelebung fällt teils sofort, teils nach wenigen Sekunden, gegenüber „über 15 Sekunden oder
länger" vorher, und keine andere Fähigkeit bleibt aus (`AUDIT_LOG.md` A73).

**Die verbleibenden Sekunden sind Bauart, kein Rest des Defekts.** Der Einschub greift nur bei
`WeaponRemain > 0,5 s`, danach geht die Wiederbelebung als nächster GCD hinaus — dazwischen liegt die
Restzeit des laufenden GCD. Mit der Vorgabe `RaisePlayerFirst = aus` kann zusätzlich eine Heilung
dazwischenkommen. Eine Verkürzung darüber hinaus ist eine Entscheidung über diese Einstellung, keine
Fehlerbehebung. Daneben stehen vier Korrekturen an den Bedingungen, unter denen überhaupt
entschieden wird, wer wiederbelebt: die Hartwirk-Zweige, die Bezugsmenge der Nur-Heiler-Modi, die
Verdrahtung der Phönixfeder und deren Zieleignung.

## Ursache

`WeaponRemain` ist derselbe Wert wie `DataCenter.DefaultGCDRemain`
(`CustomRotation_OtherInfo.cs:1624`). Auswahl und Ausführung messen dieselbe Größe, und ihre
Bedingungen schließen einander aus:

| Stelle | Bedingung | Schicht |
|---|---|---|
| `CustomRotation_GCD.RaiseSpell`, Zweig `RaisePlayerBySwift` | wählt Spontanität **nur** bei `WeaponRemain <= 0.5f` | Auswahl |
| `RSCommands_Actions.DoAction` | verweigert **jede** Fähigkeit bei `0 < DefaultGCDRemain <= 0.5f` | Ausführung |
| `RSCommands_Actions.cs:46` | dieselbe Sperre im Klick-Gate | Ausführung |
| `CustomRotation_Ability.cs:28` | dieselbe Sperre im Fähigkeiten-Dispatcher | Auswahl |

Spontanität ist eine Fähigkeit. Die beiden Fenster decken sich bis auf den einzigen Punkt
`DefaultGCDRemain == 0`. Daraus folgt das beobachtete Verhalten vollständig:

- `> 0,5 s`: Spontanität wird nicht einmal gewählt. `RaiseSpell` liefert `false`, der Dispatcher
  fällt durch, Heilung oder Schaden gewinnt den GCD.
- `0 – 0,5 s`: Spontanität wird gewählt und im Vorschaufenster angezeigt — und von `DoAction`
  verworfen. In diesem Frame geschieht nichts, denn der GCD-Pfad hat mit `return act` abgebrochen.
- genau `0`: der einzige Zustand, in dem Auswahl und Ausführung zusammenpassen.

Bewegung spielt keine Rolle: Spontanität hat keine Wirkzeit, die Wirkzeitsperre in
`ActionBasicInfo.NeedsCasting` (`:604`) wird gar nicht erreicht.

### Der Beleg aus dem Spiel

Der Auftraggeber hat berichtet, dass die Wiederbelebung sofort erfolgt, sobald er von automatisch
auf manuell stellt und den Toten anvisiert. Das ist ein natürliches Experiment und bestätigt die
Ursachenanalyse: `ActionTargetInfo.cs:117` lässt im manuellen Modus ein **feindliches** Ziel nur zu,
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

## Der Entwurf

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
| `nextGCD` | wurde zur Wiederbelebung umgeschrieben — **447 Fundstellen** im Baum lesen ihn, darunter Schimmerschild beim Beschwörer | unverändert |
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
| an | `CustomRotation_GCD.cs:123` | Notfall, Unterbrechung, Reinigung, Provokation |
| **aus (Vorgabe)** | `:350` | zusätzlich **die gesamte Heilung** (`:235`) und die Einzelziel-Verteidigung (`:333`) |

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

- `HardCastNormal` (`:130`, `:357`) fragt über `SwiftcastComingForRaise`, ob die Rotation
  Spontanität überhaupt noch für diese Wiederbelebung ausgeben wird. Nur wenn nicht, wird hartgewirkt.
- `HardCastSwiftCooldown` und `HardCastOnlyHealerSwiftCooldown` (`:140`, `:159`, `:367`, `:409`)
  wägen ab, ob die Wirkzeit kürzer ist als die Wartezeit auf Spontanität. Diese Abwägung bleibt
  unangetastet; ergänzt ist allein der Fall `!RaisePlayerBySwift`, in dem nichts kommt, worauf zu
  warten wäre.

Die naheliegende Vereinheitlichung — `!SwiftcastComingForRaise` in allen sechs Zweigen — wäre falsch
und ist verworfen: Sie hätte in den vier Abwägungszweigen sofort hartgewirkt, sobald Spontanität in
Erholung ist, und damit genau die Abwägung entfernt, die dem Wahlwert seinen Namen gibt.

Die Verhaltenswirkung der Korrektur ist auszählbar und beschränkt sich auf **eine** von vier
Kombinationen:

| Einstellung | Erholung | alt | neu | |
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
| `HardCastOnlyHealer` | „**while Swiftcast is on cooldown** and other healers are dead" | **nur** die Rezzer-Bedingung | **Widerspruch, offen** |
| `HardCastOnlyHealerSwiftCooldown` | beides | beides | stimmig |

### Die Bezugsmenge: wer gilt als lebender Rezzer

`HardCastOnlyHealer` und `HardCastOnlyHealerSwiftCooldown` bauten zwei Mengen toter beziehungsweise
vorhandener **Heiler der eigenen Gruppe** und verglichen deren Größe. Die gemeinte Frage ist eine
andere: Hartwirken kostet acht Sekunden GCD und lohnt nur, wenn es sonst niemand übernehmen kann.

| | maß bisher | gemeint |
|---|---|---|
| Wer zählt | nur Heiler | Rezzer: Heiler, Beschwörer, Rotmagier |
| Stufe | ungeprüft | ein Rotmagier unter 64 hat Verraise nicht |
| Bezugsmenge | immer die eigene Gruppe | die Menge, aus der das Ziel stammen darf — also nach `RaiseType` |
| Einzelheiler, solo | beide Mengen leer, `> 0` scheitert → **nie Hartwirk** | niemand sonst da → Hartwirken ist genau richtig |
| Ausdruckskraft | Größenvergleich | „niemand außer mir" |

Beide Fragen — die des Hartwirkens und die der Feder — laufen jetzt über
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
zählte und die Feder zurückhielt, die dort der einzige Weg war. Beide Stellen teilen sich jetzt
dieselben zwei Konstanten; das ist die Konstruktion, die nicht wieder auseinanderläuft. Die Stufe
eines Gruppenmitglieds stammt aus `ICharacter.Level`; ob der Wert in einem synchronisierten Inhalt
die synchronisierte oder die wahre Stufe meldet, ist von hier nicht entschieden. In beiden Fällen ist
er besser als die bisherige Annahme, jeder lebende Heiler, Beschwörer und Rotmagier könne
wiederbeleben.

### Die Phönixfeder

Verdrahtet im Fähigkeitenpfad (`CustomRotation_Ability.cs:374`), hinter der Heilung und vor den
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
| Wirken beginnt | nichts Besonderes; der Leichnam bleibt Kandidat | ein zweiter Rezzer kann denselben wählen |
| Wirkung landet | das Ziel trägt `StatusID.Raise` | `GetDeath` schließt es aus (`TargetFilter.cs:105`) |
| Der Tote nimmt an | keine eigene Meldung nötig | schon vorher ausgeschlossen |

Die Zeit bis zur Annahme durch den Gefallenen ist für die Entscheidung damit ohne Bedeutung: Der
Status trägt den Ausschluss, nicht die Annahme. Läuft der Status ungenutzt ab, wird der Leichnam
wieder Kandidat — richtig so.

Zwei Lücken bleiben, beide zwischen Absenden und Statusrückmeldung:

- **Gleiches Ziel, mehrere Wirker.** Während eines acht Sekunden langen Hartwirkens trägt das Ziel
  noch keinen Status. Ein zweiter Rezzer in derselben Gruppe kann es wählen. Jeder Client entscheidet
  allein; das ist ein Abstimmungsproblem und kein Fehler der Auswahl.
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

Die Filter in `GetDeath` sind vollständig und schließen jeweils sinnvoll aus: kein Wiederbelebungs-
oder Verweigerungsstatus, Entfernung über 30 Yalm, fehlende Sichtlinie, Gruppen- oder
Allianzzugehörigkeit. Der Auftraggeber hat bestätigt, dass Leichen ruhig liegen und anvisierbar sind,
womit `IsTargetMoving` und `IsTargetable` als Ursache ausscheiden.

Eine Unstimmigkeit ist erfasst, nicht behoben: Der Sonderfall für `PartyAndAllianceHealers` greift
**vor** der Umkehrung durch die Einstellung `H2`, die in allen anderen Modi die Reihenfolge dreht. In
diesem einen Modus bleibt sie damit wirkungslos.

### Was aus dem Quelltext nicht zu entscheiden ist

Ob `HardCastOnlyHealer` bei einem einzigen Heiler greifen *soll*, folgt weder aus dem Code noch aus
dem Optionstext. Der Optionstext verspricht zusätzlich „while Swiftcast is on cooldown", was dieser
Zweig nicht prüft; ob der Vorbehalt in die Bedingung gehört oder aus dem Text zu streichen ist, ist
eine Festlegung über die Bedeutung der Einstellung. Die Existenz von
`HardCastOnlyHealerSwiftCooldown` spricht dafür, dass er gemeint war.

## Verworfene Optionen

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

## Erfasst, nicht behoben

- **`IBaseAction.IgnoreClipping` wird geschrieben und nirgends gelesen** (sechs Schreibzugriffe in
  `CustomRotation_Invoke.cs`, Definition in `IBaseAction.cs:14`). Wirkungsbereich zu groß für diesen
  Vorgang: Ein Leser in `DoAction` hebelte die Anti-Clipping-Regel überall aus.
- **`Configs.Migrate` ist ein Zurücksetzen, kein Migrationspfad** (`Configs.cs:1440`). Das ist nicht
  nur unbequem, es **sperrt andere Behebungen**: Jede Korrektur, die einen Vorgabewert ändern muss,
  um das bisherige Verhalten zu erhalten, ist ohne Feldmigration nicht durchführbar. Daran ist die
  Verdrahtung von `InterruptDelay` und `ProvokeDelay` gescheitert.
- **`TargetColor` hat keinen Leser.**
- **Die Aufzählung der Wiederbelebungsaktionen im Einschiebezweig veraltet.**
  `CustomRotation_Ability.cs` prüft `nextGCD.IsTheSameTo(true, RaisePvE, EgeiroPvE, ResurrectionPvE,
  AscendPvE)`. Verraise des Rotmagiers und Angel Whisper des Blaumagiers fehlen, obwohl beide
  Rotationen `Raise` setzen. Dieselbe Alterungsursache wie die Hauptursache: eine handgepflegte Liste
  statt der vorhandenen Fähigkeitsprüfung über `Raise`. Folgenlos, solange der zweite Zweig
  (`RaisePendingAndCastable`) greift, der die Liste nicht braucht.
- **`H2` wirkt im Modus `PartyAndAllianceHealers` nicht**, weil dessen Sonderfall vor der Umkehrung
  steht.

## Grenzen des Nachweises

Statische Prüfung am Quelltext, Versionsgeschichte für die Entstehung, Fremddokumentation für
`UseAction`, und die Laufzeitbeobachtung des Auftraggebers für die Wirkung: Spontanität erscheint im
Vorschaufenster und wird nicht gewirkt, unabhängig von Bewegung; im manuellen Betrieb mit
anvisiertem Leichnam fällt die Wiederbelebung sofort.

Für den Kernpfad liegt die Laufzeitbestätigung inzwischen vor (A73): teils sofortige, teils um
wenige Sekunden verzögerte Wiederbelebung, ohne Ausfall anderer Fähigkeiten. Kein Vier-Augen-Prinzip.

**Ungemessen bleiben die drei einstellungsabhängigen Eingriffe desselben Zweigs** — Federverdrahtung,
Zieleignung über den Item-Status und Stufenprüfung der Rezzereigenschaft sowie die Hartwirk-Korrektur
und die Bezugsmenge der Nur-Heiler-Modi. Sie liegen hinter Vorgaben, die im Spieltest nicht verändert
wurden, und sind in `TODO.md` als offener Nachweis geführt.
