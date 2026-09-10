# Wiederbelebung: Auswahl und Ausführung

## Sachstand

Der Weg zur Wiederbelebung ist unterbrochen, und zwar zwischen Auswahl und Ausführung. Die Rotation
wählt Spontanität (Swiftcast) in einem Zeitfenster, in dem die Ausführungsschicht grundsätzlich
keine Fähigkeit zulässt. Die Aktion erscheint deshalb im Vorschaufenster und wird nie gewirkt; die
Rotation macht stattdessen weiter Schaden und Heilung, und die Wiederbelebung kommt erst zustande,
wenn ein Frame zufällig den einen Punkt trifft, an dem beide Fenster sich berühren.

Die Behebung verlegt die Zündung von Spontanität aus dem GCD-Pfad in den Fähigkeitenpfad, wo das
Einschieben zwischen zwei GCDs hingehört und wo bereits eine dafür gebaute Stelle steht. Der
GCD-Pfad meldet danach die Wiederbelebung selbst, nicht ihr Hilfsmittel.

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

## Die gewählte Lösung

Der GCD-Pfad meldet die Wiederbelebung, sobald sie wirkbar ist und Spontanität zur Verfügung steht.
Der Fähigkeitenpfad erkennt an dieser Meldung, dass eine Wiederbelebung ansteht, und schiebt
Spontanität im regulären Einschiebefenster ein. Im folgenden GCD liegt der Spontanitäts-Status vor,
und die bereits vorhandene erste Stufe von `RaiseSpell` gibt die Wiederbelebung sofort frei.

Ablauf mit der Änderung:

1. GCD läuft, ein Toter ist da, Spontanität bereit → `GCD()` meldet die Wiederbelebung.
2. `Invoke` sieht `CanUseGCD == false` und ruft den Fähigkeitenpfad mit dieser Meldung auf.
3. `CustomRotation_Ability.cs:705` erkennt die Wiederbelebung als nächsten GCD und zündet
   Spontanität. Das Einschiebefenster liegt bei `WeaponRemain > 0,5 s`, wo die Ausführungssperre
   nicht greift — die Fähigkeit wird tatsächlich gewirkt.
4. Nächster GCD: Spontanitäts-Status liegt an, `RaiseSpell` gibt die Wiederbelebung ohne Wirkzeit
   frei.

Die Ausführungsschicht wird nicht angefasst. Ihre Sperre ist richtig, und der Eingriff dort hätte
den größten denkbaren Wirkungsbereich: `IBaseAction.IgnoreClipping` wird an sechs Stellen gesetzt,
auch für Fälle ohne Bezug zur Wiederbelebung, und ein Leser in `DoAction` würde die Anti-Clipping-
Regel praktisch überall aushebeln.

**Präzedenz im eigenen Baum, zweifach.** `SMN_Reborn.cs:374` zündet Spontanität in
`EmergencyAbility` und prüft dort `nextGCD.IsTheSameTo(false, ResurrectionPvE)` — genau die
Konstruktion, die hier fehlt. `SMN_Reborn.cs:478` löst dieselbe Frage für Slipstream über
`skipCastingCheck`. Der Entwurf ist damit hausüblich und kein Neubau.

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

## Mitbehobene Defekte derselben Kette

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

## Erfasst, nicht behoben

- **`IBaseAction.IgnoreClipping` wird geschrieben und nirgends gelesen** (sechs Schreibzugriffe in
  `CustomRotation_Invoke.cs`, Definition in `IBaseAction.cs:14`). Begründung oben; Wirkungsbereich
  zu groß für diesen Vorgang.
- **`ObjectHelper.CanBeRaised` prüft für jeden Job `ActionID.RaisePvE`** (`:657`), also die Aktion
  des Weißmagiers. Ob `ActionManager.CanUseActionOnTarget` mit einer nicht erlernten Aktion
  antwortet wie mit einer erlernten, ist offline nicht entscheidbar und extern nicht dokumentiert.
  Auflösung verlangt Laufzeitbeobachtung mit einem anderen Rezzer.
- **`GetPriorityDeathTarget` prüft `deathTanks.Count > 1`** (`TargetUpdater.cs:385`), wo `> 0`
  gemeint ist. Folgenlos für die Frage, *ob* wiederbelebt wird, weil der Rückfallzweig denselben
  Tank findet; die Rangfolge zwischen totem Tank und totem Heiler kippt aber je nachdem, ob ein oder
  zwei Tanks liegen. Eigener Vorgang, weil eine Rangfolgeänderung eine eigene Begründung braucht.

## Grenzen des Nachweises

Statische Prüfung am Quelltext, Versionsgeschichte für die Entstehung, und die Laufzeitbeobachtung
des Auftraggebers für die Wirkung: Spontanität erscheint im Vorschaufenster und wird nicht gewirkt,
unabhängig von Bewegung. Keine eigene Laufzeitbeobachtung, kein Vier-Augen-Prinzip. Dass die
Änderung die Wiederbelebung im Spiel tatsächlich beschleunigt, ist damit begründet, nicht gemessen.
