# 10 · The Blackest Night beim Dunkelritter

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Stand dar; die Prüfhistorie steht in
`AUDIT_LOG.md` (A44).

## Ergebnis

Zwei Eingriffe in `DRK_Reborn.cs`, beide auf Upstream-Code:

1. **Der Party-Zweig fragt seine Option ab.** `BlackLantern` steuert laut Optionstext, ob The
   Blackest Night auf das Party-Mitglied mit den niedrigsten HP geht, wird in der Bedingung aber
   nicht gelesen. Die Verdrahtung wird nachgeholt — eine Defektbehebung, kein Verhaltensentwurf.
2. **Der Selbstschutz-Zweig bekommt eine Zeitpunktwahl.** Eine neue Rotationsoption
   `BlackestNightUsage` mit drei Stufen entscheidet, bei welcher Lage die Fähigkeit auf den eigenen
   Charakter geht. Voreinstellung ist das heutige Verhalten. Die engeren Stufen lassen sie in zwei
   Lagen zu — beim erkannten oder vorhergesagten Tankbuster **ohne** weitere Bedingung, und im
   großen Pull **nur, solange keine große Minderung läuft** —, wahlweise ergänzt um eine
   Gesundheitsschwelle als Notfall.

Die beiden Lagen verlangen entgegengesetzte Behandlung: Gegen einen Einschlag stapelt man
Verteidigung, gegen einen Schadensstrom staffelt man sie. Das ist der Grund, warum die
Staffelungsbedingung nur an einem der beiden Zweige hängt.

Nicht angetastet bleibt der zentrale Auslöser `AutoStatus.DefenseSingle`. Er bedient alle Tanks und
alle Verteidigungsaktionen; ein Eingriff dort hätte den Wirkungsbereich, den `AUDIT_LOG` C9 bereits
einmal als Fehlgriff belegt hat.

## Warum

### Was die Fähigkeit kostet und was sie einbringt

Die Aktionsbeschreibung (`ActionId.resx`, 7393) ist eindeutig: „Creates a barrier around self or
target party member that absorbs damage totaling 25% of target's maximum HP. Duration: 7s **Grants
Dark Arts when barrier is completely absorbed.**" Der Gegenwert der 3000 MP ist also nicht die
Barriere allein, sondern Dark Arts — ein kostenloses Edge oder Flood of Shadow —, und der entsteht
**nur bei vollständiger Absorption**. Eine zur Hälfte verbrauchte Barriere gibt nichts zurück.

### Wie viel Schaden das verlangt

Die Schwelle folgt unmittelbar aus den beiden Zahlen der Beschreibung und ist von Ausrüstung und
Inhalt unabhängig, weil beide Seiten an der maximalen Gesundheit hängen:

> Aufgezehrt wird die Barriere, wenn der eingehende Schaden **25 % der maximalen Gesundheit in
> 7 Sekunden** erreicht — also im Mittel **rund 3,6 % der maximalen Gesundheit pro Sekunde**.

Maßgeblich ist der Schaden **nach** allen Minderungen, denn die Barriere absorbiert, was nach ihnen
übrig bleibt. Daraus folgt die Umrechnung auf eine Gegnerzahl: Trägt ein einzelner Gegner *x* %
der maximalen Gesundheit pro Sekunde bei, sind **n = 3,6 / x** Gegner nötig.

| Schaden je Gegner (% max. HP/s) | benötigte Gegner |
|---|---|
| 0,5 | 8 |
| 1,0 | 4 |
| 1,5 | 3 |
| 2,0 | 2 |

**Der Wert von *x* ist aus diesem Repository nicht zu belegen** und hängt an Inhalt, Stufe,
Gegnertyp und Angriffsgeschwindigkeit; die Tabelle ist Arithmetik, keine Aussage über die
Spielwelt. Was sich belegen lässt, ist die Richtung: Jede eigene Minderung erhöht die nötige
Gegnerzahl, weil sie den Schaden senkt, der die Barriere aufzehrt.

Und genau das tut die Rotation, bevor sie The Blackest Night wirkt. Im selben Pfad steht Oblation
(−10 %) auf Priorität 10, The Blackest Night auf Priorität 20 — die vorgeschaltete Minderung
verlangt rund 11 % mehr eingehenden Schaden für denselben Auslöser. Bei Shadow Wall (−30 %) oder
Shadowed Vigil (−40 %) wäre der Aufschlag entsprechend größer. Die Reihenfolge des Pfades arbeitet
also gegen die Bedingung, unter der die Fähigkeit sich bezahlt macht. Der Tank-Haltung ist das
nicht anzulasten: Grit erhöht ausschließlich die Feindseligkeit und mindert keinen Schaden.

### Zwei Lagen, die entgegengesetzte Behandlung verlangen

Die Schwelle wird auf zwei ganz verschiedenen Wegen erreicht, und was für die eine Lage richtig
ist, ist für die andere falsch. Das ist der Kern dieses Konzepts.

**Andere Barrieren sind in beiden Lagen kein Hindernis.** Die Verbrauchsreihenfolge des Spiels führt
The Blackest Night auf Rang 3, Eukrasian Diagnosis auf 4 und Divine Benison auf 12 (A36,
Spielerdokumentation). Ein Heilerschild wird **nach** der eigenen Barriere aufgezehrt und verzögert
deren Verbrauch nicht.

**Beim Tankbuster ist Stapeln richtig.** Bei einer Gesamtminderung *m* wird die Barriere vollständig
aufgezehrt, sobald der Einschlag *b* — in Prozent der maximalen Gesundheit — die Bedingung
`b · (1 − m) ≥ 25 %` erfüllt:

| laufende Minderung | *m* | nötiger Einschlag *b* |
|---|---|---|
| keine | 0 % | 25 % |
| Oblation | 10 % | 27,8 % |
| Rampart | 20 % | 31,3 % |
| Shadow Wall | 30 % | 35,7 % |
| Shadowed Vigil | 40 % | 41,7 % |
| Shadow Wall + Rampart | 44 % | 44,6 % |

Ein Tankbuster liegt regelmäßig darüber — *als Inferenz gekennzeichnet*, eine belastbare Quelle für
Busterschaden in Prozent der Tankgesundheit liegt hier nicht vor. Belegt ist die Struktur: Der
Einschlag kommt als **ein** Paket, das die angehobene Schwelle mitnimmt. Die gleichzeitige Minderung
kostet den Auslöser also nicht, und sie zu meiden hieße, Überleben gegen 600 Potenz aus Dark Arts zu
tauschen.

**Im Wall-to-Wall ist Staffeln richtig.** Hier kommt der Schaden nicht als Paket, sondern als
Strom über die Dauer des Pulls. Zwei Minderungen gleichzeitig decken dann dieselben Sekunden doppelt
ab und lassen den Rest ungedeckt; nacheinander gelegt decken sie die doppelte Zeit. Für die Barriere
gilt zusätzlich: Läuft parallel eine große Minderung, sinkt der Strom unter die Rate, die sie in
sieben Sekunden aufzehrt — die 3,6 % pro Sekunde werden zu 5,1 % unter Shadow Wall und zu 6 % unter
Shadowed Vigil. Beides zeigt in dieselbe Richtung: The Blackest Night gehört in eine Lücke der
Minderungskette, nicht in deren Mitte.

**Das ist keine neue Konstruktion, sondern die vorhandene.** Shadow Wall und Shadowed Vigil tragen
`StatusProvide = StatusHelper.RampartStatus` (`DarkKnightRotation.cs:238`, `:404`) und überlappen
sich deshalb nie. The Blackest Night kann dieselbe Staffelung nicht über `StatusProvide` ausdrücken,
weil sein eigener Status eine Barriere ist und kein Minderungsstatus — die Prüfung muss deshalb in
der Rotation stehen. `CustomRotation.HasMajorMitigation` liest genau diese Liste.

**Was das kostet, und warum es trotzdem so bleibt.** Während einer langen Minderung fällt ein
Fenster von The Blackest Night aus; die MP fließen dann in Edge of Shadow, also in Schaden. Wer
stattdessen die Zahl der Dark-Arts-Auslösungen maximieren wollte, müsste die Minderungskette
auflösen — genau das Gegenteil einer streckenden Abdeckung.

### Wo die Fähigkeit heute gezogen wird

Drei Wege, alle in `DRK_Reborn.cs`:

| Weg | Fundstelle | Bedingung |
|---|---|---|
| Opener | `:69` | `remainTime <= 3f` im Countdown — Dark Arts steht zum Kampfbeginn bereit |
| Party-Schild | `:128` | Ziel mit den niedrigsten HP unter `BlackLanternRatio` (50 %), sobald `AutoStatus.DefenseArea` gesetzt ist |
| Selbstschutz | `:181` | **keine eigene Bedingung**; zweite Priorität nach Oblation, vor Dark Mind, Shadow Wall/Vigil und Rampart |

Der Selbstschutz-Weg öffnet sich mit `AutoStatus.DefenseSingle`. Dessen Tank-Zweig
(`StateUpdater.cs:232-276`) genügt eine von drei Lagen:

- **ab zwei Gegnern:** `tarOnMeCount >= AutoDefenseNumber` (Voreinstellung 2) innerhalb von 3 Yalm,
  die den Spieler anvisieren, mehr als 30 % aller Gegner in Nahreichweite, mindestens einer hat
  angegriffen. Die begleitende Gesundheitsbedingung `HealthForAutoDefense` steht per Vorgabe auf
  `1`, also 100 % (`Configs.cs:764`), und schränkt damit nichts ein.
- **ein einziger castender Gegner:** `IsHostileCastingToTank`. Die Erkennung
  (`DataCenter.cs:2441`) nimmt einen Tankbuster aus der gelernten Liste **oder** — als Rückfall —
  jeden Gegner, der auf sein eigenes Ziel castet. Für den Tank, der die Gruppe hält, trifft das auf
  jeden nicht unterbrechbaren Cast von mehr als einem GCD Länge zu.
- **Timeline:** `BMRTankbusterImminent`.

### Der Defekt im Party-Zweig

`BlackLantern` ist deklariert (`:18`) und als übergeordnetes Element des Schwellwerts genannt
(`:21`) — gelesen wird die Option nirgends im Baum. Die Absicht ist dreifach belegt: Der Optionstext
verspricht Schaltbarkeit, die Oblation-Zeile daneben (`:135`) prüft ihr Gegenstück
`OblationLantern`, und `ChurinDRK.cs:173` prüft bei genau diesem Zweig `BlackLantern`. Es fehlt die
Verdrahtung, nicht die Entwurfsabsicht.

Verschärfend wirkt die Oberfläche: Weil der Schwellwert `Parent = nameof(BlackLantern)` trägt, wird
er ausgeblendet, solange der Schalter aus ist. Der Nutzer sieht also weder den Schalter greifen noch
die einzige Stellschraube des Zweigs, der trotzdem läuft.

### Der teure Fall im Selbstschutz-Zweig

Der Auslöser sagt „Verteidigung ist angebracht", nicht „es kommt Schaden in Höhe von 25 % der
maximalen Gesundheit". Für Rampart, Dark Mind und Reprisal ist das unerheblich — sie kosten nichts
außer ihrer Abklingzeit. The Blackest Night ist die einzige Aktion dieses Pfades mit einer
Verbrauchsbedingung, und sie ist die einzige mit MP-Kosten. Trifft der Auslöser eine Lage, die die
Schwelle oben nicht erreicht, sind 3000 MP ausgegeben und kein Dark Arts entstanden.

Der Auftraggeber hat genau das im Spiel beobachtet: The Blackest Night wird bei wenigen Gegnern
gezogen, die Barriere nicht aufgezehrt. Diese Beobachtung ist der Beleg, den eine rein statische
Prüfung nicht liefern kann.

## Die Regel

Der Party-Zweig prüft künftig `BlackLantern`. Sonst ändert sich dort nichts — insbesondere bleibt
`BlackLanternRatio` die Schwelle, und der Zweig bleibt an `AutoStatus.DefenseArea` gebunden.

Der Selbstschutz-Zweig fragt `BlackestNightUsage`:

| Stufe | Bedingung | Für wen |
|---|---|---|
| `WheneverDefensesOpen` (Voreinstellung) | wie bisher: keine zusätzliche Bedingung | unverändertes Verhalten für alle, die nichts umstellen |
| `TankbusterOrHeavyPull` | `TankbusterOnMe` **oder** (`InHeavyPull` **und nicht** `HasMajorMitigation`) | wer die Fähigkeit als Tankbuster-Antwort und als gestaffeltes Glied der Wall-to-Wall-Kette führt |
| `TankbusterHeavyPullOrLowHealth` | zusätzlich: Gesundheit ≤ `BlackestNightHealthRatio` (Vorgabe 60 %) | wer sie auch als Notschild will |

`InHeavyPull` misst `NumberOfHostilesInRange >= MitigationSustainHostileCount` — dieselbe Schwelle,
mit der die Rotation schon entscheidet, ob Addle, Feint und Reprisal im Trash dauerhaft gehalten
werden (Vorgabe 4). Damit gibt es für „genug Trash" eine Zahl im Projekt und nicht zwei, und wer sie
verstellt, verstellt beide Regeln gemeinsam. Ob vier Gegner die Rate von 3,6 % erreichen, ist eine
Annahme — sie folgt aus rund 0,9 % je Gegner und Sekunde und ist hier nicht belegbar.

Die Staffelungsbedingung gilt **nur** für den Wall-to-Wall-Zweig. Beim Tankbuster bleibt sie außen
vor, weil dort Stapeln richtig ist, und bei niedriger Gesundheit ebenfalls: Ein Notschild wartet
nicht auf das Ende einer Minderung.

Beide Bedingungen stehen als `TankbusterOnMe` in `CustomRotation_OtherInfo.cs` und sind damit für
jede Rotation verfügbar, nicht nur für diese. `IsHostileCastingTankBusterAtMe` ist dort bewusst
gewählt und nicht `IsHostileCastingToTank`: Erstere Fassung (`DataCenter.cs:2097`) kennt den
Rückfall „castet auf sein eigenes Ziel" nicht, sondern verlangt einen Tankbuster-Wirbel auf dem
Spieler oder einen Treffer in der gelernten Liste. Das ist dieselbe Unterscheidung, die
`AUDIT_LOG` C10 für diese beiden Größen bereits herausgearbeitet hat.

**Der Opener bleibt unberührt.** Die Zeitpunktwahl gilt nur für den Selbstschutz-Zweig. Im
Countdown gibt es keinen Tankbuster und keine niedrige Gesundheit; eine engere Stufe würde die
Fähigkeit dort ausfallen lassen und damit Dark Arts für die Eröffnung verlieren — das Gegenteil des
Zwecks.

**Zum Persistenzvertrag.** Rotationskonfigurationen speichern Enums als **Namen**, nicht als
Ordinalzahlen (`RotationConfigBase.cs:208`, `Enum.Parse(type, value)`). Die Reihenfolge der Stufen
ist deshalb frei änderbar, ihre Bezeichner sind es nicht.

**Zum MP-Haushalt.** Die Option „Keep at least 3000 MP" (`TheBlackestNight`) hält Vorrat für diese
Fähigkeit zurück: `CheckDarkSide` gibt MP für Edge und Flood of Shadow erst oberhalb von 8500 frei
(`DRK_Reborn.cs:525`). Wird The Blackest Night seltener gewirkt, bleibt die Reserve stehen, ohne
etwas zu kosten — der Vorrat ist bei 10000 gedeckelt, und oberhalb von 8500 fließt er über Edge
ohnehin ab. Dieselbe Eigenschaft behandelt bereits den Fall „Dark Arts liegt an, während The
Blackest Night noch läuft" (`:515`), was zeigt, dass die Wechselwirkung zwischen Barriere und
Auslöser dem ursprünglichen Autor bewusst war.

Die Voreinstellung bleibt das heutige Verhalten, weil der Nutzen der engeren Stufen für andere
Spieler eine Annahme bleibt — die Beobachtung liegt für ein Nutzungsprofil vor, nicht allgemein.
Das ist die Projektregel für Verhaltensänderungen ohne allgemeinen Nachweis.

## Verworfene Optionen

**Am zentralen Auslöser ansetzen.** `ShouldAddDefenseSingle` an eine Schadensabschätzung binden oder
den Rückfall in `IsHostileCastingTank` entfernen. Verworfen: Der Auslöser öffnet für jeden Tank die
gesamte Verteidigungskette. Eine Verschärfung dort nimmt Rampart und Reprisal Gelegenheiten weg, die
sie zu Recht nutzen — und C9 belegt, wie eine an einer Stelle plausible Änderung an diesem Flag sich
über die ganze Kette auswirkt.

**Den Verbrauch messen statt die Lage zu prüfen.** Der eingehende Schaden ist im Effekt-Handler
sichtbar (`Watcher`, `damageEffect.value`), eine Regel „nur wenn der beobachtete Schadensfluss
3,6 % der maximalen Gesundheit pro Sekunde übersteigt" wäre also grundsätzlich baubar. Verworfen aus
demselben Grund wie der Messbaustein in `TODO.md`: Ein Ringpuffer über eingehende Treffer läuft in
jedem Kampf für jeden Nutzer, der Nutzen entstünde bei einem Job in einer Fähigkeit, und der
Schadensfluss der vergangenen Sekunden sagt nichts über die nächsten sieben.

**Eine reine MP-Schwelle.** „The Blackest Night nur oberhalb von *n* MP" ist billig, trifft aber die
falsche Größe: Sie begrenzt, wie oft die Fähigkeit ausfällt, nicht ob sie sich lohnt. Bei vollem
MP-Vorrat bliebe das beobachtete Verhalten unverändert.

**Eine Sperre, solange Dark Arts anliegt.** Naheliegend, weil ein zweiter Auslöser nicht stapelt.
Verworfen: Die Barriere selbst bleibt auch dann wertvoll, und die Sperre griffe ausgerechnet in der
Lage, in der der Tank Schutz braucht. Der verlorene Auslöser ist der kleinere Schaden.

**Eine Sperre gegen andere Barrieren.** Anders als die Minderungssperre nicht übernommen: The
Blackest Night liegt in der Verbrauchsreihenfolge vor den Heilerschilden und wird deshalb von ihnen
nicht verzögert (A36).

**Die Staffelungsbedingung auf alle Lagen ausdehnen.** Sie gilt bewusst nur im Wall-to-Wall-Zweig.
Auf den Tankbuster übertragen, würde sie die Fähigkeit ausgerechnet dann unterdrücken, wenn Rampart
(20 s) oder Shadow Wall (15 s) läuft — Zustände, die über weite Strecken zutreffen —, obwohl der
Einschlag die angehobene Schwelle mitnimmt. Das wäre C18 in neuer Gestalt: eine Aufhebungsregel, die
für eine Auslöserklasse hergeleitet und ungeprüft auf eine andere übertragen wird.

**Nullvariante.** Lässt eine im Spiel beobachtete Fehlausgabe stehen und die Option `BlackLantern`
wirkungslos.

## Konsequenzen

**Endnutzer (N).** Ohne Umstellung ändert sich nur der Party-Zweig — und der nur für Nutzer, die
`BlackLantern` ausgeschaltet gelassen haben, also in der Voreinstellung. Wer die Fähigkeit auf
Gruppenmitglieder legen will, schaltet die Option ein und erhält das bisherige Verhalten.

**Upstream-Pflege (U).** Beide Eingriffe liegen in `DRK_Reborn.cs`, einer Upstream-Datei. Die
Verdrahtung ist eine Zeile; die Zeitpunktwahl fügt eine Option, ein Enum und eine Hilfsmethode
hinzu. Nimmt Upstream die Verdrahtung selbst vor, entfällt unsere Zeile im Merge folgenlos.

**Autoren abgeleiteter Rotationen (R).** Nicht betroffen: `DRK_Reborn` ist Teil des Plugins, nicht
des Pakets `RotationSolver.Basic`. Keine Signatur ändert sich.

**Was das nicht leistet.** Die Frage, ab wie vielen Gegnern sich die Fähigkeit lohnt, bleibt ohne
Messung des eingehenden Schadens unbeantwortet; die Regel weicht ihr aus, indem sie auf die Lage
abstellt statt auf eine Zahl. Und ob die engeren Stufen im Spiel besser abschneiden, ist erst nach
einer Beobachtung zu sagen — die Voreinstellung ist deshalb das alte Verhalten.
