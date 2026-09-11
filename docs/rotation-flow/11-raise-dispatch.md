# Wiederbelebung: Auswahl und Ausführung

## Sachstand

Der zweite Behebungsversuch steht zum Test und ist **im Spiel nicht bestätigt**. Der erste ist
gescheitert und zurückgenommen; was ihn scheitern ließ, bestimmt den Entwurf des zweiten.

Die Ursache ist belegt und durch eine Laufzeitbeobachtung des Auftraggebers bestätigt: Auswahl und
Ausführung lesen dieselbe Uhr mit einander ausschließenden Bedingungen. Der zweite Versuch zündet
Spontanität dort, wo die Ausführungsschicht eine Fähigkeit überhaupt durchlässt — im
Einschiebefenster —, und lässt den GCD-Pfad dabei vollständig unangetastet.

## Der Beleg aus dem Spiel: warum manuell funktioniert

Der Auftraggeber hat berichtet, dass die Wiederbelebung sofort erfolgt, wenn er von automatisch auf
manuell stellt und den Toten anvisiert. Das ist kein Zufall, sondern ein natürliches Experiment, und
es bestätigt die Ursachenanalyse:

`ActionTargetInfo.cs:117` lässt im manuellen Modus ein **feindliches** Ziel nur zu, wenn es das
angewählte Hauptziel ist. Wer einen Toten anvisiert, hat kein feindliches Ziel — sämtliche
Angriffsaktionen finden keines und fallen aus. Der GCD bleibt frei, `DefaultGCDRemain` steht auf
genau `0`, und das ist der einzige Zustand, in dem beide Fenster zusammenpassen: Die Auswahl in
`RaiseSpell` verlangt `WeaponRemain <= 0.5f` (bei 0 erfüllt), die Sperre in `DoAction` greift erst
bei `> 0f` (bei 0 also nicht). Spontanität wird gewirkt, im nächsten Frame greift die
`HasSwift`-Stufe, die Wiederbelebung geht sofort raus.

Im automatischen Modus wird jeder frei werdende GCD sofort mit einem Angriff belegt; der Nullpunkt
ist praktisch immer von laufender Aktion, Animationssperre oder Klickverzögerung überdeckt. Daher
„dauert sehr lange" statt „geht nie" — es braucht einen Zufallstreffer.

## Der Entwurf, und was ihn vom gescheiterten unterscheidet

Der Fähigkeitenpfad zündet Spontanität bereits für die Wiederbelebung, aber nur, wenn diese schon
als nächster GCD gemeldet ist. Das kann den Ablauf nie **starten**, weil die Meldung ihrerseits
Spontanität voraussetzt. Der Entwurf ergänzt genau diese eine Lücke: Spontanität fällt auch dann,
wenn eine Wiederbelebung ansteht und wirkbar wäre.

Was dabei bewusst **nicht** geschieht, ist die Lehre aus dem Fehlversuch:

| | Erster Versuch (zurückgenommen) | Zweiter Versuch |
|---|---|---|
| GCD-Pfad | meldete die Wiederbelebung als nächsten GCD | unverändert |
| Dispatcher | endete im Wiederbelebungsblock vor Heilung und Schaden | läuft normal durch |
| `nextGCD` | wurde zur Wiederbelebung umgeschrieben — **447 Fundstellen** im Baum lesen ihn, darunter Schimmerschild beim Beschwörer | unverändert |
| Dauer | griff dauerhaft, solange ein Toter dalag | höchstens ein Frame je Gelegenheit, danach durch `HasSwift` gesperrt |

**Warum der Zweig im richtigen Fenster läuft:** `Ability()` kehrt bei `0 < WeaponRemain <= 0.5f`
sofort zurück, und bei freiem GCD ruft `Invoke` den Fähigkeitenpfad gar nicht erst auf. Der Zweig
kann also nur bei `WeaponRemain > 0,5 s` greifen — dem Einschiebefenster, in dem die
Ausführungssperre nicht gilt.

**Warum er nichts verdrängt:** Nach ihm stehen in `EmergencyAbility` nur zwei Zweige, beide Second
Wind für Nahkämpfer beziehungsweise physische Fernkämpfer bei Doom-Status. Für einen Rezzer ist
dort nichts, was ausfallen könnte. Ein Einschiebefenster wird verbraucht — einmal je Gelegenheit.

**Der Zielüberschreibungs-Fallstrick.** Wiederbelebungsaktionen führen keinen eigenen Zieltyp, sie
sind nur `IsFriendly`; ihr Ziel stammt aus `TargetType.Death`, das ausschließlich der GCD-Pfad
setzt. Eine Wirkbarkeitsprüfung im Fähigkeitenpfad ohne diese Überschreibung durchsucht die falsche
Menge — und `CanUse` weist als Nebenwirkung `Target` zu, ließe die Aktion also auf ein fremdes Ziel
zeigen. `RaisePendingAndCastable` setzt die Überschreibung deshalb selbst und stellt den vorherigen
Wert wieder her, statt ihn zu löschen, weil der Fähigkeiten-Dispatcher eigene Überschreibungen um
seine Zweige legt.

## Wenn Spontanität nicht zur Verfügung steht

Der Einschiebe-Entwurf oben deckt nur den Fall ab, dass Spontanität bereit ist. Die beiden anderen
Fälle verhalten sich verschieden, und einer davon war ein eigener Defekt.

**Spontanität in Erholung, Einstellung an.** `IsCoolingDown` ist wahr, der Hartwirk-Zweig greift,
`RaiseSpell` wird mit `mustUse` aufgerufen und die Wiederbelebung wird hart gewirkt. Das
funktioniert — mit einer bauartbedingten Grenze: Stufe (C) verlangt `!IsMoving`, weil ein
Acht-Sekunden-Zauber im Laufen nicht zustande kommt. Wer sich bewegt, wartet auf Spontanität. Das
ist keine Fehlfunktion, sondern die Spielmechanik; der Einschiebe-Entwurf verkürzt die Wartezeit auf
die Erholung von Spontanität.

**Einstellung abgeschaltet: es wurde überhaupt nicht wiederbelebt.** Die Kette ist geschlossen und
am Artefakt belegt:

- Für einen Heiler zündet diese Rotation Spontanität **ausschließlich** über den
  Wiederbelebungspfad. Die beiden anderen Zünder (`CustomRotation_Ability.cs:688` und `:697`) sind
  auf `JobRole.RangedMagical` eingeschränkt; Heiler tragen `JobRole.Healer`.
- Ist die Einstellung aus, wird Spontanität also nie gezündet, geht nie in Erholung, und
  `IsCoolingDown` bleibt falsch.
- Damit feuert der Hartwirk-Zweig nie. Stufe (B) verlangt die Einstellung selbst, Stufe (C) braucht
  `mustUse`, das nur der Hartwirk-Zweig setzt. Alle drei Wege sind zu.

Die Beschreibung der Einstellung sagt zu, Spontanität nicht **dafür** zu verwenden — nicht, das
Wiederbeleben einzustellen.

**Behebung: die Hartwirk-Zweige fragen nach der richtigen Größe.** Statt „ist Spontanität in
Erholung" fragen sie „kommt Spontanität für diese Wiederbelebung überhaupt noch". Die
Verhaltenswirkung ist auszählbar und beschränkt sich auf **eine** von vier Kombinationen:

| Einstellung | Erholung | alt | neu | |
|---|---|---|---|---|
| an | ja | wirkt | wirkt | gleich |
| an | nein | wirkt nicht | wirkt nicht | gleich |
| aus | ja | wirkt | wirkt | gleich |
| aus | nein | **wirkt nicht** | **wirkt** | der Fall, der niemanden wiederbelebte |

Bei eingeschalteter Einstellung — der Vorgabe — ist das Verhalten unverändert. Das ist zugleich der
Nachweis, dass diese Korrektur nicht die Ursache der zurückgenommenen Regression gewesen sein kann:
Sie greift in deren Konfiguration überhaupt nicht.

Die vier Zweige mit Wirkzeit-gegen-Wartezeit-Abwägung (`HardCastSwiftCooldown` und
`HardCastOnlyHealerSwiftCooldown`) behalten diese Abwägung unverändert und bekommen allein den Fall
dazu, in dem es nichts gibt, worauf zu warten wäre.

### Restrisiken, offen benannt

- Eine Ladung Spontanität kann verpuffen, wenn ein anderer Heiler die Wiederbelebung übernimmt,
  nachdem sie gezündet wurde. Begrenzt dadurch, dass `GetDeath` ein Ziel mit laufendem
  Wiederbelebungsstatus ausschließt, also aus der Menge fällt, sobald jemand anders wirkt.
- Ein Einschiebefenster geht für Spontanität weg statt für eine Heil- oder Schadensfähigkeit.
- **Nicht gemessen:** Ob die Wiederbelebung im Spiel nun zügig fällt, ist begründet und nicht
  beobachtet. Der erste Versuch war ebenfalls compile- und skriptgrün und trotzdem falsch.

## Warum es heute nicht funktioniert

`WeaponRemain` ist derselbe Wert wie `DataCenter.DefaultGCDRemain`
(`CustomRotation_OtherInfo.cs:1624`). Auswahl und Ausführung messen also dieselbe Größe, und ihre
Bedingungen schließen einander aus:

| Stelle | Bedingung | Wirkung |
|---|---|---|
| `CustomRotation_GCD.cs:560` | wählt Spontanität **nur** bei `WeaponRemain <= 0.5f` | Auswahl |
| `RSCommands_Actions.cs:78` (`DoAction`) | verweigert **jede** Fähigkeit bei `0 < DefaultGCDRemain <= 0.5f` | Ausführung |
| `RSCommands_Actions.cs:46` | dieselbe Sperre im Klick-Gate | Ausführung |
| `CustomRotation_Ability.cs:28` | dieselbe Sperre im Fähigkeiten-Dispatcher | Auswahl |

Spontanität ist eine Fähigkeit. Die beiden Fenster decken sich bis auf den einzigen Punkt
`DefaultGCDRemain == 0`. Daraus folgt das beobachtete Verhalten vollständig:

- `> 0,5 s`: Spontanität wird nicht einmal gewählt. `RaiseSpell` liefert `false`, der Dispatcher
  fällt durch, und Heilung oder Schaden gewinnt den GCD.
- `0 – 0,5 s`: Spontanität wird gewählt und angezeigt — und von `DoAction` verworfen. In diesem
  Frame geschieht nichts, denn der GCD-Pfad hat mit `return act` bereits abgebrochen.
- genau `0`: der einzige Zustand, in dem Auswahl und Ausführung zusammenpassen.

Beide Umgehungen sind ebenfalls verschlossen. Der Hartwirk-Zweig (`:130`) verlangt
`SwiftcastPvE.Cooldown.IsCoolingDown`; Spontanität kommt aber nie zum Einsatz und geht deshalb nie
in Erholung. Der Einschiebe-Pfad (`CustomRotation_Ability.cs:705`) verlangt `nextGCD` als
Wiederbelebung; im Fehlerfenster ist `nextGCD` jedoch Spontanität selbst, außerhalb davon die
Heilung. Es gibt damit keinen Zustand, aus dem der Ablauf von allein herausfindet.

Bewegung spielt keine Rolle. Spontanität hat keine Wirkzeit, und die Wirkzeitsperre in
`ActionBasicInfo.NeedsCasting` (`:604`) wird gar nicht erst erreicht.

## Entstehung

Die Konstruktion war bei ihrer Einführung richtig und ist durch eine spätere Änderung an anderer
Stelle unrichtig geworden, ohne dass etwas fehlschlug — Parnas' *Lack of Movement*.

- `f22be318`, 10.01.2025: der Spontanitäts-Zweig im GCD-Pfad mit `WeaponRemain <= 0.5f`. Eine
  Ausführungssperre für Fähigkeiten in diesem Fenster gab es noch nicht.
- `92f109d3`, 02.03.2026, vierzehn Monate später: die Sperre in `DoAction` kommt hinzu. Sie ist für
  sich richtig — sie verhindert, dass ein eingeschobenes oGCD den nächsten GCD verzögert — und
  entwertet dabei den Zweig von 2025.

Daraus folgt, dass die Behebung nicht die Zahl `0.5f` verschieben darf. Das reproduzierte nur
dieselbe Kopplung an einem anderen Punkt. Zu ersetzen ist die Konstruktion: die Zuständigkeit für
das Einschieben.

## Der zurückgenommene erste Versuch

Er meldete die Wiederbelebung aus dem GCD-Pfad als nächsten GCD und überließ das Einschieben dem
Fähigkeitenpfad. Der Gedanke war richtig, die Stelle falsch: Die Meldung eines GCD ist kein
folgenloser Hinweis, sondern beendet den Dispatcher **und** setzt die Eingabe des gesamten
Fähigkeitenpfads. Beides ist oben in der Vergleichstabelle beziffert. Vollständige Aufarbeitung des
Fehlers in `AUDIT_LOG.md` C37.

Die Lehre, die den zweiten Entwurf bestimmt: Wer eine Meldebedingung lockert, misst ihre Wirkung an
beiden Pfaden und zählt vorher aus, wer `nextGCD` liest.

### Verworfene Optionen

- **Nullvariante.** Scheidet aus: Der Defekt trifft jede Wiederbelebung jedes Rezzers und hat keine
  Selbstheilung.
- **Auswahlfenster verschieben** (`0.5f` durch einen anderen Wert ersetzen). Behandelt den Fundort,
  nicht die Ursache, und lässt eine oGCD-Zündung im GCD-Pfad stehen, die `Invoke` bei freiem GCD als
  GCD ausführt und sonst durch eine beliebige andere Fähigkeit ersetzt.
- **`IgnoreClipping` in `DoAction` lesen.** Belebte tote Konfiguration, aber mit einem
  Wirkungsbereich weit über die Wiederbelebung hinaus. Bleibt als eigener Punkt erfasst.
- **Ausführungssperre für Spontanität ausnehmen.** Behandelte das Symptom in der falschen Schicht
  und öffnete das Einschiebefenster für eine Aktion, die dort nicht hingehört.

## Im selben Versuch mitgeändert, ebenfalls zurückgenommen

**Die Aufzählung der Wiederbelebungsaktionen veraltet.** `CustomRotation_Ability.cs:705` prüft
`nextGCD.IsTheSameTo(true, RaisePvE, EgeiroPvE, ResurrectionPvE, AscendPvE)`. Verraise des
Rotmagiers und Angel Whisper des Blaumagiers fehlen, obwohl beide Rotationen `Raise` setzen. Das ist
dieselbe Alterungsursache wie oben: eine handgepflegte Liste statt der vorhandenen
Fähigkeitsprüfung. Die Rotationsbasis führt die Aktion bereits als `Raise`
(`CustomRotation_Actions.cs:280`); der Vergleich geht künftig dagegen.

**Die Hartwirk-Zweige verlangen einen Zustand, den sie selbst verhindern.** Alle sechs feuerten nur
bei `SwiftcastPvE.Cooldown.IsCoolingDown`. Ist `RaisePlayerBySwift` abgeschaltet, zündet die
Rotation Spontanität nie, die Erholung tritt nie ein, und es wird überhaupt nicht wiederbelebt —
obwohl die Beschreibung der Einstellung nur zusagt, dass Spontanität nicht *dafür* benutzt wird.

Die Behebung unterscheidet die beiden Zweigarten, weil sie verschiedene Fragen stellen:

- `HardCastNormal` (`:130`, `:357`) fragt über `SwiftcastComingForRaise`, ob die Rotation
  Spontanität überhaupt noch für die Wiederbelebung ausgeben wird. Nur wenn nicht, wird hartgewirkt.
- `HardCastSwiftCooldown` und `HardCastOnlyHealerSwiftCooldown` (`:140`, `:182`, `:367`, `:409`)
  wägen ab, ob die Wirkzeit kürzer ist als die Wartezeit auf Spontanität. Diese Abwägung bleibt
  unangetastet; ergänzt ist allein der Fall `!RaisePlayerBySwift`, in dem das Warten sinnlos ist,
  weil nichts kommt, auf das man warten könnte.

Die naheliegende Vereinheitlichung — `!SwiftcastComingForRaise` in allen sechs Zweigen — wäre
falsch und ist im Code-Review dieses Vorgangs verworfen worden: Sie hätte in den vier
Abwägungszweigen sofort hartgewirkt, sobald Spontanität in Erholung ist, und damit genau die
Abwägung entfernt, die dem Wahlwert seinen Namen gibt.

## `SwiftcastBuffer`: erfasst, bewusst nicht verdrahtet

Die Einstellung (0,6 s, eigene Oberfläche, eigene Dokumentation „how early before next GCD should
RSR use swiftcast for raise") kommt im ganzen Baum genau einmal vor: in ihrer Definition. Sie hat
keinen Leser, und daneben steht die hartkodierte `0.5f`.

Sie bleibt unverdrahtet, weil ihre dokumentierte Bedeutung mit der Ausführungssperre unvereinbar
ist. Sie besagt, Spontanität solle erst fallen, wenn nur noch `SwiftcastBuffer` Restzeit auf dem
GCD liegt — bei 0,6 s ist das fast vollständig der Bereich, den `DoAction` für Fähigkeiten sperrt,
und bei einem Wert von 0 vollständig. Sie zu verdrahten hieße, denselben Defekt an einer zweiten
Stelle neu zu bauen.

Die Einstellung ist damit nicht bloß unverbunden, sondern seit `92f109d3` in ihrer Absicht
überholt: Der Zeitpunkt, den sie steuern wollte, ist heute per Politik oGCD-frei. Ihre Auflösung —
Entfernung samt Migrationspfad oder Neudefinition als Untergrenze im Einschiebefenster — ist ein
eigener Vorgang und in `TODO.md` erfasst.

## Die Zielprüfung, die eine Aktionsprüfung war — ebenfalls zurückgenommen

`ObjectHelper.CanBeRaised` beantwortet eine **Zielfrage** — ist dieser Leichnam überhaupt ein
gültiges Wiederbelebungsziel — und beantwortete sie mit einer **Aktionsprüfung**:
`CanUseActionOnTarget(ActionID.RaisePvE, …)`, für jeden Job dieselbe Aktion, nämlich die des
Weißmagiers. Gelehrter, Astrologe, Weiser, Rotmagier, Beschwörer und Blaumagier beleben mit einer
anderen wieder, und `PheonixDownItem` — der eine Aufrufer, den es gerade für Jobs **ohne**
Wiederbelebung gibt — fragte, ob der Spieler einen Zauber wirken kann, den er nie besitzt.

Die Prüfung war in beiden möglichen Ausgängen falsch, weshalb der unbekannte Rückgabewert nichts
entscheidet: Antwortet die native Funktion für eine nicht erlernte Aktion `false`, war jeder
Nicht-Weißmagier-Rezzer blockiert; antwortet sie `true`, tat die Zeile nichts. FFXIVClientStructs
bindet `CanUseActionOnTarget` als bloße Signatur mit Byte-Pattern und ohne Dokumentation, die
Semantik ist also von außen nicht zu klären. Ein Indiz liegt im Nachbarcode: `IsOtherPlayerOutOfDuty`
hedgt denselben Aufruf mit „Raise **oder** Cure" ab, was nur sinnvoll ist, wenn eine nicht erlernte
Aktion `false` liefert.

Aufgelöst wird das mit derselben Trennung wie oben: Die Zielfrage bleibt zielbezogen
(`IsTargetable`), die Aktionsfrage übernehmen die Aufrufer, die ihre eigene Aktion kennen — der
Zauber über `Raise.CanUse` mit Reichweite, MP, Stufe und Freischaltung, der Gegenstand über
`BaseItem`. Die übrigen Zielbedingungen stehen ohnehin schon in `TargetFilter.GetDeath`:
Zugehörigkeit zu Gruppe oder Allianz, Entfernung, Sichtlinie, laufender Wiederbelebungsstatus.

**Warum das sicher ist:** Die Änderung kann Ziele nur zulassen, nie ausschließen. Ein zusätzlich
zugelassenes Ziel, das sich als unbrauchbar erweist, fällt in derselben `CanUse`-Kette durch, die
den Zauber ohnehin prüft. `IsOtherPlayerOutOfDuty` bleibt unangetastet: dort ist die Absicht die
Zielbarkeit eines Fremdspielers, nicht die Wiederbelebbarkeit.

## Erfasst, nicht behoben

- **`IBaseAction.IgnoreClipping` wird geschrieben und nirgends gelesen** (sechs Schreibzugriffe in
  `CustomRotation_Invoke.cs`, Definition in `IBaseAction.cs:14`). Wirkungsbereich zu groß für
  diesen Vorgang: Ein Leser in `DoAction` hebelte die Anti-Clipping-Regel überall aus.
- **`Configs.Migrate` ist ein Zurücksetzen, kein Migrationspfad** (`Configs.cs:1440`). Das ist
  nicht nur unbequem, es **sperrt andere Behebungen**: Jede Korrektur, die einen Vorgabewert
  ändern muss, um das bisherige Verhalten zu erhalten, ist ohne Feldmigration nicht durchführbar.
  Daran ist die Verdrahtung von `InterruptDelay` und `ProvokeDelay` gescheitert.
- **`TargetColor` hat keinen Leser.** Der zunächst gemeldete selbstbezügliche Elternverweis ist
  folgenlos — nur Kontrollkästchen werden Elternelemente, ein `Vector4` sortiert schlicht nach oben.

## Grenzen des Nachweises

Statische Prüfung am Quelltext, Versionsgeschichte für die Entstehung, und die Laufzeitbeobachtung
des Auftraggebers für die Wirkung: Spontanität erscheint im Vorschaufenster und wird nicht gewirkt,
unabhängig von Bewegung. Keine eigene Laufzeitbeobachtung, kein Vier-Augen-Prinzip. Dass die
Änderung die Wiederbelebung im Spiel tatsächlich beschleunigt, ist damit begründet, nicht gemessen.
