# 08 · Synergie von Schadensvermeidung und Schadenserzeugung

Entwurfsdokument nach ADR-Struktur, mit Umsetzungsplan. Beschreibt einen
Mechanismus, kein umgesetztes Verhalten: Stand dieses Dokuments ist ein Konzept
ohne Code.

## Kontext

Ausgangspunkt war die Frage, wie Sanctus (Holy) eingesetzt wird und ob seine
Betäubungswirkung berücksichtigt wird. Sie wird nicht berücksichtigt, und die Frage
öffnet ein allgemeineres Thema: RSR trifft Mitigationsentscheidungen **je Werkzeug
und reaktiv**. Es gibt keine Stelle, an der beantwortet wird, wie viel
Schadensvermeidung gerade anliegt und ob ein weiteres Werkzeug daran noch etwas
ändert.

Das ist nicht nur eine Frage der Mitigation. Jedes Werkzeug, das redundant fällt,
kostet einen GCD oder einen Weave-Slot, der Schaden erzeugt hätte. **Die Vermeidung
von Überlappung dient beiden Zielen gleichzeitig** — das ist der Leitgedanke und der
Grund, warum das Thema als Synergiefrage geführt wird.

### Rechenbeispiel als Größenordnung

Die Betäubungsressource ist **einmalig je Pull**, nicht wiederkehrend. Erste
Anwendung 4 s, zweite 2 s, dritte 1 s, danach 45 s Immunität. Ein stehender
Gruppenpull ist selten länger als diese 45 s, der Zurücksetzen der Resistenz fällt
also praktisch nicht mehr in den Kampf. Zu verteilen sind damit genau **7 Sekunden
Betäubung**, und die Frage lautet nicht, wie viele Zyklen man unterbringt, sondern
wie man diese sieben Sekunden legt.

**Posten 1 — Ausnutzung der Dauern.** Bei einem GCD von rund 2,5 s deckt
ununterbrochenes Nachcasten 0–4,5 s und 5,0–6,0 s ab, zusammen etwa **5,5 s** mit
einer Lücke: Die zweite Anwendung fällt, während die erste noch läuft, und ihre
kürzere Dauer verfällt teilweise. Wird jeweils erst nach Ablauf nachgecastet,
stehen die vollen **7 s** zur Verfügung. Gewinn: rund **1,5 s** je Gegner.

**Posten 2 — Entzerrung gegen fremde Mitigation.** Reprisal senkt den
Gegnerschaden um 10 % für 10 s. Liegt die Betäubung darin, ist ihr Reprisal-Anteil
verloren: 7 s × 10 % = **0,7 s** je Gegner.

| | überlappend | entzerrt |
|---|---|---|
| Betäubungsdauer genutzt | 5,5 s | 7,0 s |
| Reprisal-Anteil erhalten | 0,3 | 1,0 |
| **Summe je Gegner** | **5,8** | **8,0** |

Posten 1 ist damit **mehr als doppelt so schwer** wie Posten 2. Für die Reihenfolge
der Umsetzung heißt das: Die Streckung der Betäubung (B) trägt den Nutzen, die
Abstimmung mit fremder Mitigation (C) ist ein Zusatz.

Modell, keine Messung. Annahmen: gleichmäßiger Schaden, stehender Kampf, alle
Werkzeuge verfügbar, Nachcasten im GCD-Takt.

### Warum die Gegnerzahl den Ausschlag *nicht* gibt

Naheliegend ist die Annahme, der Einschub lohne sich mit steigender Gegnerzahl
zunehmend — der vermiedene Schaden multipliziert sich schließlich mit jedem Ziel.
Das trifft für den absoluten Betrag zu, für die Entscheidung aber nicht: **Der Preis
skaliert mit derselben Zahl.** Ein entgangener Sanctus kostet Schaden auf allen
getroffenen Zielen. Betäubungsgewinn und Sanctus-Verlust wachsen beide linear mit
der Gegnerzahl, ihr Verhältnis bleibt konstant.

Nicht mitskaliert nur der DoT-Anteil: Ein Einzelziel-DoT bringt unabhängig von der
Gegnerzahl denselben Betrag, während sein Preis mit ihr steigt.

| Posten | Nutzen skaliert mit n | Preis skaliert mit n | Folge |
|---|---|---|---|
| Betäubungsstreckung | ja | ja | gegnerzahl-neutral |
| DoT-Einschub | nein | ja | lohnt bei **wenigen** Zielen |

Die eigentliche Steuergröße ist damit nicht die Gegnerzahl, sondern das Verhältnis
von eingehendem zu ausgehendem Schaden: Steht die Gruppe unter Druck, ist
vermiedener Schaden mehr wert als erzeugter; ist der Pull ohnehin sicher, gilt das
Gegenteil. Dafür sind die vorhandenen HP-Schwellen das Maß, nicht
`NumberOfHostilesInRange`.

Die Gegnerzahl bleibt trotzdem als **Untergrenze** sinnvoll — unterhalb von drei
Zielen fällt Sanctus wegen `AoeCount` ohnehin nicht, und Einzelziele sind meist
betäubungsimmun. Sie ist ein Filter, kein Gewicht.

## Vorhandene Bausteine

Der Entwurf erfindet wenig; das meiste liegt im Baum und ist nur nicht verbunden.

| Baustein | Fundstelle | Zustand |
|---|---|---|
| Mitigationsmessung: Gegner-Debuffs (Addle, Feint, Dismantle, Reprisal) und Party-Buffs (Tank-LB3, Sacred Soil, Temperance, Kerachole, Troubadour-Familie, Dark Missionary u. a.), verrechnet zu einem Schadensfaktor | `CustomRotation_OtherInfo.cs:534` | Gelesen an genau einer Stelle: `RotationConfigWindow.cs:4863`, also **nur zur Anzeige** |
| Betäubung, Verlangsamung und **deren Resistenzen** als Statuseffekte | `StatusID.Stun` (+ Varianten), `StatusID.StunResistance`, `StatusID.Slow`, `StatusID.SlowResistance`, `StatusID.ArmsLength` | Vorhanden in den generierten Spieldaten, von keiner Rotation gelesen |
| Statusabfragen mit Restzeit und Stapelzahl | `StatusHelper.HasStatus`, `.StatusTime`, `.StatusStack` (`StatusHelper.cs:898`, `:807`, `:978`) | In Betrieb |
| Vorhersagefenster aus der BossModReborn-Timeline | `Configs.cs:742` (`BMRRaidwideMitWindow`, 5 s), `:747` (`BMRTankbusterMitWindow`, 3 s), ausgewertet in `StateUpdater.cs:185`, `DataCenter.cs:2640` | In Betrieb |
| Zentralisierte Nachzieh-Regel für Gegner-Debuffs | `CustomRotation_OtherInfo.cs:1327` (`ShouldSustainMitigationDebuff`) | In Betrieb, 27 Aufrufstellen. Beleg, dass eine gemeinsame Regel über viele Jobs trägt |
| Gegnerzahl-Schwelle als etabliertes Muster | `Configs.cs:757-759` (`MitigationSustainHostileCount`, 1–8, Standard 4) über `NumberOfHostilesInRange` | In Betrieb |
| Trennung von Mitigation und Schaden im Dispatch | `CustomRotation_GCD.cs`: HealArea 240, HealSingle 282, DefenseArea 322, DefenseSingle 337, GeneralGCD erst 449 | In Betrieb. Mitigation und Heilung haben Vorrang |

**Korrektur gegenüber der ersten Fassung dieses Dokuments:** Dort stand, die
Resistenzstufe einer Betäubung müsse aus dem Ereignisstrom des `Watcher`
mitgezählt werden. Das war eine unbelegte Annahme. Das Spiel führt sie als eigenen
Statuseffekt (`StunResistance`, analog `SlowResistance`), der über die vorhandenen
Statusabfragen direkt lesbar ist. Eine eigene Buchführung entfällt damit, und der
Aufwand des Vorhabens sinkt erheblich.

## Die Lücke

**Aktionen mit doppelter Wirkung sind nur nach einer ihrer Wirkungen eingeordnet.**
Sanctus steht im Schadenszweig (`WHM_Reborn.cs:518-527`); dass es betäubt, ist im
Entscheidungsmodell nicht vorhanden. Assize steht im Angriffs-oGCD (`:308`); dass es
heilt, ebenfalls nicht. Umgekehrt kennt `GetCurrentMitigationPercent` die Wirkung
von Reprisal, aber weder Betäubung noch Verlangsamung, obwohl beide wie
Schadensreduktion wirken.

Zweite, unabhängig belegte Fundstelle: Im Schadenszweig steht der Sanctus-Block
(518-527) **vor** dem DoT-Block (530-544). Sobald `AoeCount = 3` erfüllt ist, greift
Sanctus, und der DoT wird nie gesetzt — bei einem Boss mit zwei Adds läuft also nie
Dia. Dass das nicht gewollt ist, zeigt der DoT selbst: `ModifyDiaPvE`
(`WhiteMageRotation.cs:300-311`) setzt `TargetStatusProvide` gegen Nachlegen und
`IsRestrictedDOT` gegen ungeeignete Ziele — beide Vorkehrungen laufen bei AoE ins
Leere, weil der Zweig nicht erreicht wird.

## Die verwobene Entscheidung

Die drei bisher getrennt geführten Punkte sind eine einzige Frage: **Wann lohnt es
sich, einen GCD nicht in Sanctus zu stecken?**

- **A** liefert den Grund: Der DoT fehlt oder läuft aus, und der eingeschobene Cast
  hat damit eigenen Wert.
- **B** liefert das Timing: Läuft die Betäubung noch, streckt der Einschub sie, statt
  sie zu überschreiben. Ist das Ziel bereits resistent oder immun, ist die
  Betäubung ohnehin kein Argument mehr — dann entscheidet A allein.
- **C** liefert den zweiten Grund für dasselbe Timing: Läuft eine fremde Mitigation,
  ist ein Stun jetzt weniger wert als später.

Als eine Regel:

> Ein Nicht-Sanctus-GCD wird eingeschoben, wenn er eigenen Wert hat **und** die
> Betäubung dadurch nicht verloren geht — weil sie noch läuft, weil sie ohnehin
> nicht mehr wirkt, oder weil gerade eine stärkere Mitigation trägt. Bei Gefahr
> wird nicht eingeschoben.

A ist dabei die einzige Stufe, die **allein** einen Nutzen hat: Sie stellt die
DoT-Uptime her, unabhängig von jeder Betäubungsbetrachtung. B und C verbessern nur
ihren Zeitpunkt. Das bestimmt die Reihenfolge der Umsetzung.

## Optionen

**O0 — Nullvariante.** Verhalten belassen. Die bezifferte Überlappung bleibt, der
DoT bleibt bei AoE unerreichbar, `GetCurrentMitigationPercent` bleibt Anzeige.

**O1 — Je Rotation einzeln.** Verstößt gegen die Defektklassen-Regel; dasselbe
Muster altert je Datei getrennt.

**O2 — Zentrale Auslösung.** Ein gemeinsamer Trigger, der Mitigation anfordert.
Diese Bauform wurde in diesem Fork gebaut und zurückgebaut:
`HasHostileCountAoeMitigation` setzte `AutoStatus.DefenseArea` und öffnete die
gesamte Defensivkette statt der einen gemeinten Zeile.

**O3 — Zentrale Bremse, dezentrale Auslösung.** Vorhandene Auslöser bleiben; hinzu
kommt eine Prüfung, die **zurückhält**. Fällt sie aus, verhält sich RSR wie heute.

**O4 — Rückbau.** Messung entfernen, Thema schließen.

**Gewählt: O3**, ausschlaggebend ist das Ausfallverhalten. Eine Bremse, die nicht
greift, führt zum heutigen Verhalten zurück; ein Öffner, der fälschlich feuert,
löst die ganze Kette aus.

## Umsetzungsplan

Vier Schritte. Jeder ist für sich lieferbar, prüfbar und abschaltbar; jeder Schritt
lässt den vorherigen unverändert. Standard ist überall das heutige Verhalten.

### Schritt 1 — Messung (kein Verhalten)

**Drei getrennte Größen, kein gemeinsamer Prozentwert.** Ein früherer Entwurf sah
vor, Betäubung und Verlangsamung in den vorhandenen Mitigationsanteil
einzurechnen. Das wäre ein Surrogat: `GetCurrentMitigationPercent` liefert
`1 − damageFactor`, geklemmt auf 0 bis 0,95 (`CustomRotation_OtherInfo.cs:719-720`)
— eine Betäubung entspricht 1,0 und würde auf 0,95 gekappt. Vor allem aber ist sie
kategorisch anders als eine Schadensreduktion: binär statt anteilig, zeitlich
scharf begrenzt, resistenzbehaftet und zielbezogen statt gruppenweit. Beides in
einer Zahl zu führen verliert genau die Eigenschaften, wegen derer gemessen wird.

| Größe | Art | Quelle |
|---|---|---|
| `MitigationFraction` (vorhanden, unverändert) | gruppenweit, anteilig, Momentwert | `GetCurrentMitigationPercent()` |
| `StunCoverage` | Anteil der betroffenen Gegner mit Betäubung, dazu die **kürzeste** Restzeit unter ihnen | `StatusHelper.HasStatus` / `.StatusTime` mit `StatusID.Stun` und Varianten |
| `StunHeadroom` | ob ein neuer Stun noch Wirkung hätte | `StatusID.StunResistance` über `.StatusStack` |

Die kürzeste Restzeit, nicht der Mittelwert: Sobald der erste Gegner aufwacht,
läuft wieder Schaden ein. Ein Mittelwert würde die Deckung überzeichnen.

**Berechnungsort.** Nicht als statische Abfrage in den Rotationszweigen, sondern
als eigener Schritt im `MajorUpdater` vor `ActionUpdater.UpdateNextAction()`
(`MajorUpdater.cs:228`), Ergebnis in `DataCenter`. Grund: `GetCurrentMitigationPercent`
iteriert über alle Gegner **und** alle Party-Mitglieder; als Momentabfrage würde
sie mehrfach je Frame laufen. Vorbild ist `TargetUpdater.UpdateTargets()`, das
seine Listen einmal je Frame füllt.

Kein Aufrufer in einer Rotation — dieser Schritt ändert kein Verhalten und ist
durch Kompilierung und ein Prüfskript abgesichert.

**Konkrete Form.** Drei Felder in `DataCenter`, einmal je Frame gefüllt, dazu eine
Statusgruppe in `StatusHelper` nach dem dort etablierten Muster
(`public static StatusID[] X { get; } = [...]`, z. B. `StatusHelper.cs:295`):

```
public static float StunCoverage          // 0..1, Anteil der Gegner in Reichweite mit Betäubung
public static float StunRemainingShortest // Sekunden, kürzeste Restzeit unter den betäubten
public static bool  StunHeadroom          // mindestens ein Ziel, bei dem ein Stun noch wirkt
```

Der Updater, aufgerufen aus `MajorUpdater` nach `TargetUpdater.UpdateTargets()` und
vor `ActionUpdater.UpdateNextAction()`:

```
var jobRange = DataCenter.JobRange;
int total = 0, stunned = 0, headroom = 0;
var shortest = float.MaxValue;

foreach (var h in DataCenter.AllHostileTargets)
{
    if (h is null || h.DistanceToPlayer() >= jobRange) continue;
    total++;
    var remain = h.StatusTime(false, StatusHelper.StunStatus);
    if (remain > 0f) { stunned++; shortest = Math.Min(shortest, remain); }
    if (!h.HasStatus(false, StatusID.StunResistance)) headroom++;
}
```

Die Reichweitenprüfung ist wörtlich die von `NumberOfHostilesInRange`
(`DataCenter.cs:1421-1436`), damit Schwelle und Messung dieselbe Menge meinen.

**Die Immunität braucht keinen Timer.** Sie ist ein Statuseffekt, kein
Zeitfenster, das mitgeführt werden müsste: Solange `StunResistance` auf einem
Gegner liegt, ist `StunHeadroom` für ihn falsch; fällt der Status nach seinen 45
Sekunden ab, wird er von selbst wieder wahr, und die Regel greift ohne
Sonderbehandlung erneut. Der seltene lange Kampf, in dem die Betäubungen ein
zweites Mal verfügbar werden, ist damit kein Sonderfall im Code, sondern ergibt
sich aus derselben Abfrage. Das ist der zweite Grund, die Messung zustandsbasiert
statt zeitbasiert zu führen — der erste war der Wegfall der Buchführung.

**Grenzen der Messung, die keine Umsetzung beheben kann.**

- Ein betäubungsimmuner Gegner ist von einem, der nur noch nie betäubt wurde,
  nicht zu unterscheiden, solange kein Stun versucht wurde. Erst wenn nach einem
  Sanctus kein Betäubungsstatus entsteht, ist die Immunität belegt. Die Messung ist
  also erst ab dem zweiten Cast vollständig — was zur Regel passt, weil der erste
  Sanctus ohnehin fällt.
- Statusrestzeiten kommen vom Server; die letzten Zehntelsekunden sind wegen
  Latenz unzuverlässig. Schwellen sind deshalb in GCD-Einheiten zu formulieren
  (`GCDTime(1)`), nicht in Sekunden.

### Schritt 2 — A: DoT-Zweig vor Sanctus

In `WHM_Reborn.GeneralGCD` den DoT-Block (530-544) vor den Sanctus-Block (518-527)
ziehen, hinter eine Option:

```
[RotationConfig(CombatType.PvE, Name = "Keep the damage-over-time up in AoE, ahead of Holy")]
public bool DotAheadOfAoe { get; set; } = false;
```

Wirksam ohne Schritt 1 und ohne 3. `TargetStatusProvide` auf Dia verhindert, dass
der Einschub mehr als einen GCD je DoT-Laufzeit kostet.

### Schritt 3 — B/C: Zeitpunkt des Einschubs

Die Bedingung aus Schritt 2 wird um die Rückhalteprüfung ergänzt, als eigene
Option, damit Schritt 2 unabhängig bleibt:

```
protected static bool StunWouldBeWasted =>
    !DataCenter.StunHeadroom                       // resistent oder immun
    || DataCenter.StunRemainingShortest > GCDTime(1)  // laeuft noch, Einschub streckt sie
    || GetCurrentMitigationPercent() >= StunYieldThreshold; // fremde Mitigation traegt gerade
```

Vorbehalte, die den Rückhalt aufheben — sie stehen vor der Regel, nicht in ihr:

- `NumberOfHostilesInRange < HostileCountThreshold` (Muster:
  `MitigationSustainHostileCount`),
- vorhergesagter Schaden innerhalb `BMRRaidwideMitWindow` / `BMRTankbusterMitWindow`,
- Gruppen-HP unterhalb der eingestellten Schwelle.

### Schritt 4 — Übertragung auf andere Doppelnutzen-Aktionen

Erst wenn 1 bis 3 im Einsatz beobachtet wurden. Kandidatensuche über ein Prüfskript
statt über Erinnerung: Aktionen, die in einem Schadenszweig stehen und einen
Statuseffekt mit Mitigationswirkung anlegen.

### Was der Plan nicht vorsieht

Sanctus in den Verteidigungszweig zu verschieben. Das würde den Schadensfiller an
ein Mitigationsflag hängen, das dafür nicht gebaut ist.

## Falsifikation

**Hypothese 1: Es liegt kein Defekt vor.** Widerlegt durch zweierlei: die bezifferte
Größenordnung, und die Existenz von `GetCurrentMitigationPercent` — die
Entwurfsabsicht ist vorhanden, die Verdrahtung fehlt. Für A zusätzlich durch die
beiden Vorkehrungen an `ModifyDiaPvE`, die bei AoE nie zur Wirkung kommen.

**Hypothese 2: Die gewählte Option ist falsch.** Widerlegt für die Bremse, nicht für
einen Auslöser — siehe O2 und den belegten Rückbau. Für Schritt 4 hält der Einwand
teilweise stand, weshalb er von Beobachtung abhängig gemacht ist.

**Hypothese 3: Der Einschub ist im mittleren Gegnerzahlbereich falsch.** Hält
stand. Bei etwa fünf bis sieben Zielen ist weder der DoT-Wert noch der
Betäubungswert groß, der entgangene Sanctus aber schon spürbar. Die Regel wird
deshalb nicht als „immer" formuliert, sondern an eine Schwelle gebunden, und der
Bereich ist hier ausdrücklich als der schwächste benannt.

**Was nicht widerlegt ist:** dass der Nutzen im Spiel eintritt. Die Rechnung ist ein
Modell.

## Nachweisbarkeit

### Wirksamkeitsmessung im Spiel

Die frühere Fassung dieses Dokuments hielt einen Nachweis, dass die Rotation besser
spielt, schlicht für unmöglich. Das war zu bequem: RSR sieht den Ereignisstrom und
kann sich selbst messen.

`Watcher.ActionFromEnemy` wertet jeden gegnerischen Treffer aus, summiert die
Schadensanteile und legt sie über `DataCenter.AddDamageRec` als
`DamageRec(ReceiveTime, Ratio)` in eine Warteschlange (`DataCenter.cs:1294`,
`:1403-1411`). Der erlittene Schaden über die Zeit ist damit bereits erfasst —
gebraucht wird nur eine Auswertung je Kampf statt eines gleitenden Fensters.

Vier Kennzahlen, alle aus vorhandenen Quellen ableitbar:

| Kennzahl | Quelle | Aussage |
|---|---|---|
| Erlittener Schadensanteil je Pull | `_damages`, summiert zwischen Kampfbeginn und -ende | Das Zielkriterium |
| Genutzte Betäubungsdauer | `StunCoverage` über die Zeit integriert | Ob die 7 s ausgeschöpft wurden |
| Überlappungsanteil | Anteil der Betäubungszeit, in der `MitigationFraction` bereits über der Schwelle lag | Ob Posten 2 greift |
| DoT-Laufzeitanteil | Zeit mit aktivem DoT auf dem Hauptziel geteilt durch Kampfdauer | Ob A wirkt |

**Versuchsanordnung.** Dieselbe Instanz, derselbe Pull, Option abwechselnd an und
aus, mehrere Durchläufe. Weil die vier Kennzahlen alle innerhalb des Plugins
anfallen, genügt eine Anzeige im Einstellungsfenster neben dem bereits vorhandenen
Mitigationswert; ein externes Werkzeug ist nicht nötig.

**Was die Messung nicht leistet.** Sie ist nicht kontrolliert: Gegnerzahl,
Tankverhalten und Gruppenzusammensetzung schwanken zwischen Durchläufen und
überdecken einen Effekt in der Größenordnung weniger Prozent leicht. Sie taugt
daher, um eine **Verschlechterung** zu erkennen und die Größenordnung einzugrenzen,
nicht um einen kleinen Gewinn zu beweisen. Das ist trotzdem mehr als die bisherige
Annahme, es sei gar nichts messbar.

### Übrige Ebenen

| Ebene | Möglich |
|---|---|
| Statisch | Prüfskript nach Bauart der vorhandenen Audit-Skripte, mit Selbsttest: findet Aktionen mit Mitigationswirkung im Schadenszweig und gleicht sie gegen die Erhebung aus Schritt 1 ab |
| Kompilierung | CI |

Solange die Wirksamkeitsmessung nicht vorliegt, gilt die Feature-Toggle-Regel
unverändert: **jeder Schritt hinter eine eigene Option, Standard aus.** Die Messung
ist der Weg, diese Vorsichtsmaßnahme später begründet aufzuheben — sie gehört
deshalb vor Schritt 3, nicht danach.

## Konsequenzen

**Endnutzer.** Bei eingeschalteter Option fallen Mitigationswerkzeuge seltener, aber
gezielter; die frei werdenden GCDs gehen in Schaden, und die DoT-Uptime steigt.
Spürbar im großen Flächenpull, folgenlos im Einzelzielkampf — was durch die
Gegnerzahl-Schwelle so gewollt ist.

**Autoren abgeleiteter Rotationen.** Die Schritte 1 und 3 ergänzen die öffentliche
Oberfläche von `RotationSolver.Basic` additiv; keine Signatur bricht. Wer die neuen
Prüfungen nicht aufruft, merkt nichts.

**Upstream-Pflege.** Der Eingriff liegt in `CustomRotation_OtherInfo` und
`WHM_Reborn`, beides Dateien mit regelmäßiger Upstream-Aktivität. Neue Teile gehören
in eigene Regionen, statt bestehende Blöcke umzubauen. Schritt 2 verschiebt einen
bestehenden Block und erzeugt damit den größten Merge-Aufwand des Plans — die
Verschiebung sollte als eigener Commit ohne inhaltliche Änderung erfolgen.
