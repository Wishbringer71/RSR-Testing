# 10 · The Blackest Night beim Dunkelritter

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Stand dar; die Prüfhistorie steht in
`AUDIT_LOG.md` (A44 bis A50).

## Ergebnis

The Blackest Night ist keine Verteidigung wie die anderen: Sie kostet 3000 MP und zahlt sie nur
zurück, wenn ihre Barriere **vollständig** aufgezehrt wird. Die Rotation behandelte sie dennoch wie
Rampart oder Reprisal — als beliebiges Glied einer Prioritätsliste, ohne eigene Bedingung. Drei
Eingriffe in `DRK_Reborn.cs`, alle auf Upstream-Code:

1. **Der Party-Zweig fragt seine Option ab** (`:155`). `BlackLantern` soll steuern, ob die Fähigkeit
   auf das Party-Mitglied mit den niedrigsten HP geht, wurde aber nie gelesen. Defektbehebung.
2. **Der Selbstschutz-Zweig bekommt eine Zeitpunktwahl** (`BlackestNightUsage`, `:35`) mit drei
   Stufen. Voreinstellung bleibt das heutige Verhalten.
3. **Zwei Prüfgrößen liegen zentral** in `CustomRotation_OtherInfo`: `TankbusterOnMe` (`:1377`) und
   `HasMajorMitigation` (`:1365`); `SurveyStuns` hat eine Überladung mit der Trefferzahl bekommen
   (`:544`).
4. **Die Gegenrichtung steht beim Weißmagier** (`WHM_Reborn.ShouldHoldHolyForBarrier()`, Option
   `HoldHolyForBlackestNight`): Sanctus wartet, solange ein Tank die Barriere trägt. Ohne diese
   Seite behandelt die Betäubungsbedingung nur den Fall, dass der Heiler zuerst da war.

| Stufe | Bedingung |
|---|---|
| `WheneverDefensesOpen` (Voreinstellung) | keine zusätzliche Bedingung — Verhalten wie bisher |
| `TankbusterOrHeavyPull` | `TankbusterOnMe` **oder** der gestaffelte Pull |
| `TankbusterHeavyPullOrLowHealth` | zusätzlich Gesundheit ≤ `BlackestNightHealthRatio` (Vorgabe 60 %) |

Der **gestaffelte Pull** verlangt fünf Dinge gleichzeitig:

| Bedingung | Grund |
|---|---|
| `NumberOfHostilesInRange >= BlackestNightMinHostiles` (Vorgabe 4) | erst ab genug Gegnern erreicht der Schadensstrom die Verbrauchsrate |
| `!HasMajorMitigation` | eine große Minderung senkt den Strom unter diese Rate |
| `!GroupStunRunning()` | eine Gruppenbetäubung hält den Strom ganz an |
| `!PackSlowed()` | eine verlangsamte Gruppe verdünnt ihn für die Dauer des Debuffs |
| Reprisal ist erledigt | die kostenlose Gruppenminderung gehört zuerst gewirkt |

Beim Tankbuster gilt **keine** davon, und der Notfallzweig bei niedriger Gesundheit wartet ebenfalls
auf nichts. Der zentrale Auslöser `AutoStatus.DefenseSingle` bleibt unangetastet; er bedient alle
Tanks und ihre gesamte Verteidigungskette, und ein Eingriff dort hat den Wirkungsbereich, den
`AUDIT_LOG` C9 bereits einmal als Fehlgriff belegt hat.

## Warum

### Die Verbrauchsbedingung

Die Aktionsbeschreibung (`ActionId.resx`, 7393) ist eindeutig: „Creates a barrier around self or
target party member that absorbs damage totaling 25% of target's maximum HP. Duration: 7s **Grants
Dark Arts when barrier is completely absorbed.**" Der Gegenwert der 3000 MP ist nicht die Barriere,
sondern Dark Arts — ein kostenloses Edge oder Flood of Shadow. Eine zur Hälfte verbrauchte Barriere
gibt nichts zurück.

Daraus folgt eine von Ausrüstung und Inhalt unabhängige Schwelle, weil beide Seiten an derselben
Bezugsgröße hängen:

> Aufgezehrt wird die Barriere, wenn der eingehende Schaden **25 % der maximalen Gesundheit in
> 7 Sekunden** erreicht — im Mittel **rund 3,6 % pro Sekunde**, gemessen **nach** allen Minderungen.

Diese eine Zahl trägt alles Folgende: die Gegnerzahl, die Minderungsgrenze und die Betäubungsregel
sind nur die drei Wege, auf denen sie verfehlt wird.

### Zwei Lagen: Einschlag gegen Strom

Die Schwelle wird auf zwei verschiedenen Wegen erreicht, und was für die eine Lage richtig ist, ist
für die andere falsch.

**Beim Tankbuster kommt der Schaden als ein Paket, und Stapeln ist richtig.** Bei einer
Gesamtminderung *m* wird die Barriere vollständig aufgezehrt, sobald der Einschlag *b* — in Prozent
der maximalen Gesundheit — die Bedingung `b · (1 − m) ≥ 25 %` erfüllt:

| laufende Minderung | *m* | nötiger Einschlag *b* |
|---|---|---|
| keine | 0 % | 25 % |
| Oblation | 10 % | 27,8 % |
| Rampart | 20 % | 31,3 % |
| Shadow Wall | 30 % | 35,7 % |
| Shadowed Vigil | 40 % | 41,7 % |
| Shadow Wall + Rampart | 44 % | 44,6 % |

Ein Tankbuster liegt regelmäßig darüber — *als Inferenz gekennzeichnet*, eine belastbare Quelle für
Busterschaden in Prozent der Tankgesundheit liegt hier nicht vor. Belegt ist die Struktur: Ein
einzelnes Paket nimmt die angehobene Schwelle mit. Gleichzeitige Minderung kostet den Auslöser also
nicht, und sie zu meiden hieße, Überleben gegen 600 Potenz zu tauschen.

**Im Wall-to-Wall kommt der Schaden als Strom, und Staffeln ist richtig.** Zwei Minderungen
gleichzeitig decken dieselben Sekunden doppelt ab und lassen den Rest ungedeckt; nacheinander gelegt
decken sie die doppelte Zeit. Für die Barriere kommt hinzu, dass eine parallele große Minderung den
Strom unter die Verbrauchsrate drückt — aus 3,6 % pro Sekunde werden 5,1 % unter Shadow Wall und
6,0 % unter Shadowed Vigil.

Andere **Barrieren** sind in beiden Lagen kein Hindernis: Die Verbrauchsreihenfolge des Spiels führt
The Blackest Night auf Rang 3, Eukrasian Diagnosis auf 4 und Divine Benison auf 12 (A36,
Spielerdokumentation). Ein Heilerschild wird nach der eigenen Barriere aufgezehrt.

### Die fünf Bedingungen des Pull-Zweigs

**Genug Gegner.** Die Schwelle steht als eigene Rotationsoption, nicht als Ableitung der
Mitigations-Sustain-Zahl: Dort geht es um die Aufrechterhaltung eines Debuffs, hier um eine
Verbrauchsrate. Was die Zahl leisten kann, ist, die Fähigkeit aus Lagen herauszuhalten, in denen
sicher zu wenig Schaden kommt — zwei Gegner erreichen 3,6 % pro Sekunde nur, wenn jeder 1,8 %
beiträgt, was für gewöhnlichen Trash unplausibel ist. Was sie **nicht** leisten kann, ist eine
Garantie: Dafür müsste der Schaden je Gegner bekannt sein, und der hängt an Inhalt, Stufe und
Gegnertyp. Die Vorgabe 4 entspricht rund 0,9 % je Gegner und Sekunde und ist eine Annahme; deshalb
ist die Zahl einstellbar, und die Beobachtung im Spiel entscheidet.

**Keine große Minderung.** In der harten Fassung — keine andere Verteidigung darf laufen — ist die
Forderung nicht erfüllbar. Die Dauern summieren sich, jede Fähigkeit nur einmal gewirkt:

| Fähigkeit | Dauer | Minderung |
|---|---|---|
| Reprisal | 10 s, ab Stufe 98 15 s | −10 %, ab Stufe 98 −15 % (auf den Gegnern) |
| Oblation | 2 × 10 s | −10 % |
| Dark Mind | 10 s | −10 % / −20 % |
| Dark Missionary | 15 s | −5 % / −10 % |
| Rampart | 20 s | −20 % |
| Shadow Wall / Shadowed Vigil | 15 s | −30 % / −40 % |

Zusammen **95 Sekunden** — mehr, als ein Wall-to-Wall-Pull dauert. Sperrte jede davon die Barriere,
käme sie nie. Die Grenze verläuft deshalb bei der Wirkungsstärke: Die schwachen (10 bis 15 %) heben
die nötige Rate um ein Neuntel bis ein Sechstel, die starken (ab 20 %) um ein Viertel bis zwei
Drittel.
`StatusHelper.RampartStatus` führt genau die starken, und dieselbe Liste tragen Shadow Wall und
Shadowed Vigil bereits als `StatusProvide` (`DarkKnightRotation.cs:238`, `:404`) — die Staffelung
ist im Projekt etabliert, The Blackest Night stand nur außerhalb. Über `StatusProvide` kann sie
diese Staffelung nicht ausdrücken, weil ihr eigener Status eine Barriere ist und kein
Minderungsstatus; deshalb steht die Prüfung in der Rotation.

Oblation bleibt zusätzlich aus einem zweiten Grund außen vor: Sie steht im selben Pfad **vor** der
Barriere, die sonst hinter ihrer eigenen Vorgängerin hängen bliebe.

**Keine Gruppenbetäubung.** Eine Betäubung ist der Grenzfall der Minderung: Für ihre Dauer kommt
nicht weniger Schaden, sondern gar keiner. Sanctus — Holy, ab Stufe 82 Holy III — hält alles im
Umkreis von acht Yalm 4 Sekunden lang an (`ActionId.resx` 139, 25860), und der Weißmagier hält das
im Trash absichtlich aufrecht: `WHM_Reborn.cs:498` streckt Sanctus über `SurveyStuns`, solange die
Gegner betäubbar sind. Erst wenn sie `StunResistance` tragen (39, „Immune to stun effects"), läuft
der Strom wieder.

Wer betäuben kann, entscheidet über die Reichweite der Regel:

| Rolle | Aktion | Wirkung | Dauer |
|---|---|---|---|
| Heiler | Sanctus, Sanctus III — **nur Weißmagier** | Fläche, 8 Yalm | 4 s |
| Tank | Schildhieb — nur Paladin | Einzelziel | 6 s |
| Tank | **Tiefschlag — alle Tanks, auch der Dunkelritter** | Einzelziel | 5 s |
| Nahkampf | Fußfeger | Einzelziel | 3 s |
| Occult Crescent | Occult Falcon, Mineuchi, Variant Ultimatum | Fläche / Einzelziel | 4–6 s |

Gelehrter, Astrologe und Weiser haben keine. Eine Fallunterscheidung nach Gruppenzusammensetzung
braucht die Regel trotzdem nicht — sie misst den Status **auf den Gegnern**, ist ohne Betäubung
also von selbst wirkungslos.

Der Quantor folgt aus dieser Tabelle. Tiefschlag trägt der Dunkelritter selbst, und RSR wirkt es
über den Unterbrechungspfad (`CustomRotation_Ability.cs:575`): „**irgendein** Gegner betäubt" hätte
die eigene Unterbrechung die eigene Barriere sperren lassen, während sieben von acht Gegnern weiter
zuschlagen. „**Alle** Gegner betäubt" fällt um, sobald ein Nachzügler unbetäubt dazustößt, obwohl
der Strom erkennbar steht. Maßgeblich ist der **Anteil**: mindestens zwei betäubte Gegner und
mindestens die Hälfte der Gegner in Reichweite. Gemessen wird über die Jobreichweite
(`DataCenter.JobRange`, für Tanks drei Yalm) — dieselbe Menge, über die auch die Gegnerzahl zählt,
denn beide Bedingungen beantworten dieselbe Frage.

Zwischen zwei Anwendungen von Sanctus läuft die Betäubung etwa einen globalen Cooldown lang aus; der
Halt trägt deshalb noch drei Sekunden über diese Lücke, aber nur solange die Gegner überhaupt
betäubbar sind. Mit ihrer Immunität endet er von selbst — „zurückhalten, bis die Betäubungen nicht
mehr wirken" braucht keinen eigenen Zähler.

**Reprisal zuerst.** Der Pfad gibt je Gelegenheit **eine** Aktion zurück und arbeitet von oben nach
unten: Oblation (10) · The Blackest Night (20) · Dark Mind · Shadowed Vigil/Shadow Wall · Rampart ·
… · Reprisal (`:175`, `:180` im Flächenpfad, am Ende auch im Einzelpfad). Bei 15 Sekunden
Abklingzeit gewinnt die Barriere fast jede Gelegenheit, und die Rollenaktion landet erst, wenn jene
zufällig nicht verfügbar ist. Für einen Pull ist das die verkehrte Rangfolge:

| | Reprisal | The Blackest Night |
|---|---|---|
| Kosten | nur Abklingzeit | 3000 MP |
| Wirkung | −10 % Schaden aller Gegner | Barriere über 25 % der maximalen Gesundheit |
| Reichweite | ganze Gruppe | ein Charakter |
| Dauer | 15 s | 7 s |

Gegen einen Tankbuster ist die Rangfolge gleichgültig, weil dort eine einzelne Gelegenheit
entscheidet — deshalb gilt auch diese Bedingung nur im Pull-Zweig.

**Keine verlangsamte Gruppe.** Verlangsamung ist nicht bloß ein Zauberer-Debuff: Ihr Wirktext nennt
neben Wirk- und Wiederholzeit ausdrücklich die **Verzögerung der Automatikangriffe**, und Trash-Gegner
liefern den Großteil ihres Schadens genau darüber. Eine verlangsamte Gruppe verdünnt den Strom
deshalb etwa um die Stärke des Debuffs — Armlänge legt Verlangsamung +20 % auf jeden physischen
Angreifer für 15 s, dieselbe Größenordnung wie Rampart und damit jenseits der Linie, ab der die
Barriere in sieben Sekunden nicht mehr aufgezehrt wird.

Dieselbe Anteilsregel wie bei der Betäubung, aus demselben Grund: ein verlangsamter Gegner von acht
sagt über den Strom nichts. Der Unterschied liegt im Zeitverlauf — eine Betäubung hält den Strom an
und läuft in Sekunden aus, eine Verlangsamung verdünnt ihn fünfzehn Sekunden lang. Ein Nachlauffenster
hat diese Bedingung deshalb nicht; sie endet mit dem Debuff.

*Herkunft des Befunds:* Rückfrage des Auftraggebers, ob die Regel die anhaltende Verlangsamung
mitprüft. Sie tat es nicht — und die Erhebung dahinter (`scan16.py`) zeigt, dass die
Verlangsamungswirkung im gesamten Baum an keiner Stelle gelesen wurde, obwohl jeder Tank sie über
eine Rollenaktion mitbringt.

### Warum die Bedingungen zusammenpassen

Sie sind keine unabhängigen Filter, sondern beschreiben denselben Zeitpunkt. Während des
Einsammelns läuft der Dunkelritter, die Gegner folgen verstreut, und nur ein Teil steht in
Schlagreichweite: wenig Gegner, schwacher Strom — die Gegnerzahl hält die Barriere heraus. Am
**Ende** des Pulls steht alles beieinander, die Zahl ist am höchsten und der Strom am stärksten, und
genau dann hat der Dunkelritter seine großen Minderungen typischerweise noch nicht gezogen, weil
zuvor kaum etwas auf ihn einschlug. Die Betäubungsphase fällt oft mit diesem Moment zusammen, weil
der Heiler zu betäuben beginnt, sobald die Gruppe steht; sie ist deshalb — zusammen mit der
Verlangsamung — eine der beiden Bedingungen, die **aufschieben statt auszuschließen**.

### Die Gegenrichtung: der Weißmagier hält Sanctus zurück

Die Betäubungsbedingung behandelt die Kollision nur von einer Seite. Trifft der Dunkelritter zuerst,
läuft die Barriere, und der Heiler unterbricht den Schadensstrom, gegen den sie aufgezehrt werden
müsste. Deshalb steht in `WHM_Reborn` die Gegenbedingung: `ShouldHoldHolyForBarrier()` hält Sanctus
zurück, solange ein Gruppenmitglied in Tankrolle The Blackest Night trägt
(`StatusHelper.FullAbsorbRewardStatus`).

**Das ist kein Warten aufeinander.** Jede Seite wartet nur, während der Zustand der anderen aktiv
ist, und beide Zustände laufen von selbst ab — die Betäubung nach vier Sekunden, die Barriere nach
sieben. Ein Zustand, in dem beide warten, ist nicht erreichbar; sind beide frei, handeln beide. Was
die zweite Regel herstellt, ist Nachrang für den, der später kommt.

**Und der Gruppenschutz spricht nicht dagegen.** Im Wall-to-Wall liegt die Aggro beim Tank, der
Schaden also auch, und derselbe Tank trägt die Barriere. Die Betäubung verhindert dort genau den
Schaden, den die Barriere aufgefangen hätte — statt doppelten Schutzes entsteht doppelte
Verschwendung: die Barriere samt Dark Arts verfällt, und das Betäubungsbudget von rund sieben
Sekunden bis zur Immunität ist verbraucht.

Zwei Grenzen halten die Kosten klein, und die Kosten sind real — Sanctus ist der einzige
Flächenzauber dieses Jobs, ein zurückgehaltener GCD fällt auf Einzelzielschaden zurück:

| Grenze | Wirkung |
|---|---|
| nur solange die Betäubung noch landen könnte (`headroom`) | mit der Betäubungsimmunität endet die Rückhaltung für den Rest des Pulls |
| nur solange die Barriere die Wirkzeit überdauert (`WillStatusEnd` gegen `Info.CastTime`) | eine auslaufende Barriere ist kein Wartegrund |

Die Regel steht hinter `HoldHolyForBlackestNight`, Vorgabe aus. `StatusHelper.FullAbsorbRewardStatus`
führt nur The Blackest Night: Jede andere Barriere ist reiner Schutz, bei dem ein unverbrauchter Rest
ein gutes Ergebnis ist — nur hier ist der vollständige Verbrauch die Bedingung einer Belohnung.

### Der Ausgangsbefund

Drei Stellen wirken die Fähigkeit, und zwei davon trugen keine eigene Bedingung:

| Weg | Fundstelle | Zustand vorher |
|---|---|---|
| Opener | `:92` | `remainTime <= 3f` im Countdown — Dark Arts steht zum Kampfbeginn bereit; unverändert |
| Party-Schild | `:155` | Ziel unter `BlackLanternRatio`, **ohne** `BlackLantern` zu prüfen |
| Selbstschutz | `:303` | **keine** eigene Bedingung, zweite Priorität nach Oblation |

Beim Party-Zweig ist die Absicht dreifach belegt: Der Optionstext verspricht Schaltbarkeit, die
Oblation-Nachbarzeile prüft ihr Gegenstück `OblationLantern`, und `ChurinDRK.cs:173` prüft
`BlackLantern` am selben Zweig. Verschärfend blendet die Oberfläche den Schwellwert aus, solange der
Schalter aus ist (`Parent = nameof(BlackLantern)`, `:21`) — die einzige Stellschraube des laufenden
Zweigs war unsichtbar.

Der Selbstschutz-Zweig öffnet mit `AutoStatus.DefenseSingle`, und dessen Tank-Zweig
(`StateUpdater.ShouldAddDefenseSingle`) genügt schon eine dieser Lagen: zwei Gegner in Nahreichweite, die den
Spieler anvisieren — die begleitende Gesundheitsbedingung steht per Vorgabe auf 100 %
(`Configs._healthForAutoDefense`) und schränkt nichts ein —, oder ein einziges `IsHostileCastingToTank`, das über
den Rückfall „castet auf sein eigenes Ziel" (`DataCenter.IsHostileCastingArea`) jeden nicht unterbrechbaren
Trash-Cast trifft, oder `BMRTankbusterImminent`. Für Rampart und Reprisal ist dieser Auslöser
unbedenklich; The Blackest Night ist die einzige Aktion des Pfades mit Ressourcenkosten und
Verbrauchsbedingung.

`TankbusterOnMe` stützt sich deshalb auf `IsHostileCastingTankBusterAtMe` und
`BMRTankbusterImminent`, nicht auf `IsHostileCastingToTank` — dieselbe Unterscheidung, die C10 für
diese beiden Größen herausgearbeitet hat.

## Verworfene Optionen

**Am zentralen Auslöser ansetzen.** `ShouldAddDefenseSingle` verschärfen oder den Rückfall in
`IsHostileCastingTank` entfernen. Das öffnet und schließt die Verteidigungskette **aller** Tanks;
C9 belegt, wie weit eine an einer Stelle plausible Änderung an diesem Flag reicht.

**Den Verbrauch messen statt die Lage zu prüfen.** Der eingehende Schaden ist im Effekt-Handler
sichtbar (`Watcher`, `damageEffect.value`). Verworfen aus derselben Kostenrechnung, die den
Messbaustein verworfen hat: Ein Ringpuffer läuft in jedem Kampf für jeden Nutzer, der Nutzen entsteht
bei einem Job in einer Fähigkeit — und der Schadensfluss der letzten Sekunden sagt nichts über die
nächsten sieben.

**Eine reine MP-Schwelle.** Begrenzt, wie oft die Fähigkeit ausfällt, nicht ob sie sich lohnt. Bei
vollem Vorrat bliebe das beanstandete Verhalten unverändert.

**Eine Sperre, solange Dark Arts anliegt.** Der Auslöser stapelt nicht, die Barriere bleibt aber
wertvoll — die Sperre griffe ausgerechnet dann, wenn der Tank Schutz braucht.

**Eine Sperre gegen andere Barrieren.** Nicht nötig: The Blackest Night wird vor den Heilerschilden
aufgezehrt (A36).

**Die Staffelungsbedingung auf alle Lagen ausdehnen.** Auf den Tankbuster übertragen, würde sie die
Fähigkeit unterdrücken, während Rampart (20 s) oder Shadow Wall (15 s) laufen, obwohl der Einschlag
die angehobene Schwelle mitnimmt — C18 in neuer Gestalt.

**Die Reihenfolge im Pfad selbst umstellen**, statt Reprisal über eine Bedingung vorzuziehen. Der
direktere Weg, aber er ändert das Verhalten für alle Nutzer in allen Lagen, während die Bedingung
hinter der Option bleibt. In `TODO.md` mit Auflösungsbedingung geführt.

**Nullvariante.** Lässt eine im Spiel beobachtete Fehlausgabe stehen und die Option `BlackLantern`
wirkungslos.

## Konsequenzen

**Endnutzer (N).** Ohne Umstellung ändert sich nur der Party-Zweig, und der nur in der
Voreinstellung, in der `BlackLantern` ausgeschaltet ist. Wer die Fähigkeit auf Gruppenmitglieder
legen will, schaltet die Option ein und erhält das bisherige Verhalten.

**Autoren abgeleiteter Rotationen (R).** `TankbusterOnMe`, `HasMajorMitigation` und die
`SurveyStuns`-Überladung sind additiv; die vorhandene Signatur bleibt bestehen und delegiert. Kein
Bruch, Eintrag in `CHANGELOG.md`.

**Upstream-Pflege (U).** Alle Eingriffe liegen in `DRK_Reborn.cs` und `CustomRotation_OtherInfo.cs`.
Nimmt Upstream die Verdrahtung von `BlackLantern` selbst vor, entfällt unsere Zeile im Merge
folgenlos.

**Randbedingungen.** Der Opener bleibt außen vor: Im Countdown gibt es weder Tankbuster noch Pull,
eine engere Stufe würde dort nur Dark Arts für die Eröffnung kosten. Rotationskonfigurationen
speichern Enums als **Namen**, nicht als Ordinalzahlen (`RotationConfigBase.cs:208`) — die
Reihenfolge der Stufen ist frei, ihre Bezeichner sind Vertrag. Und der MP-Haushalt bleibt
unberührt: `CheckDarkSide` gibt MP für Edge erst oberhalb von 8500 frei (`:525`) und behandelt den
Fall „Dark Arts liegt an, während die Barriere noch läuft" bereits selbst (`:515`).

**Was das nicht leistet.** Ob vier Gegner die Verbrauchsrate im gespielten Inhalt erreichen, ob eine
halb betäubte Gruppe den Strom weit genug drückt und ob die engeren Stufen insgesamt besser
abschneiden — all das ist ohne Spielbeobachtung nicht zu belegen. Deshalb ist die Voreinstellung das
alte Verhalten, sind Gegnerzahl und Gesundheitsschwelle einstellbar, und deshalb steht der offene
Rest in `TODO.md`.
