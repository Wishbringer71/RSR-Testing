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

### Die beiden Seiten sind nicht verrechenbar

Naheliegend wäre nun, vermiedenen gegen erzeugten Schaden aufzuwiegen. Das ist der
schwerwiegendere Denkfehler, und er lag den obigen Tabellen zugrunde: Die Aufgabe
eines Heilers ist, Tank und Gruppe am Leben zu halten; Schaden ist wichtig, steht
aber im Rang darunter. Ein lebender Tank oder Schadensausteiler erzeugt mehr
Schaden als ein Heiler — und ungleich mehr als ein toter. Die beiden Größen bilden
eine **Rangordnung, keine Verrechnung**: erst Überleben sichern, dann Schaden
maximieren.

Für die Regel folgt daraus eine Umkehrung der Beweislast. Die gestreckte Betäubung
liefert mehr Deckung als das Nachcasten im GCD-Takt — 7 s gegen 5,5 s. In einer
Rangordnung, deren erstes Kriterium die Schadensvermeidung ist, ist sie damit
**immer** die richtige Wahl, sobald sie überhaupt anwendbar ist. Der entgangene
Sanctus-Schaden ist nachrangig und taugt nicht als Gegengrund. Nicht die Streckung
braucht eine Rechtfertigung, sondern ihr Unterlassen.

Zwei Einschränkungen, die dagegen stehen und die die Regel nicht auflösen darf:

- **Mitigation hat keinen linearen Wert.** Sie ist folgenlos, solange niemand
  stirbt, und entscheidend, wenn sie einen Tod verhindert. Im gut gehaltenen
  Trash-Pull vermeidet die gestreckte Betäubung Schaden, den ohnehin niemand
  gespürt hätte. Die Rangordnung greift dort also ins Leere — sie schadet aber auch
  nicht.
- **Schaden ist selbst eine Form der Schadensvermeidung.** Ein schneller Kill
  verkürzt den Kampf und spart Gegner-Angriffe. Der Anteil des Heilers am
  Gruppenschaden ist allerdings klein, weshalb dieser Rückkopplungseffekt den
  Vorrang nicht umkehrt.

**Warum die Architektur das nicht von selbst löst.** Der Dispatch prüft Heilung und
Verteidigung vor dem Schadenszweig, setzt die Rangordnung also bereits um — aber
nur **reaktiv**: Ein Heilflag entsteht, wenn HP fehlen. Eine Betäubung ist
Prävention und wirkt, bevor Heilbedarf sichtbar wird. Für Prävention hat die
Architektur keinen Ort, und deshalb liegt die Betäubung im Schadenszweig — nicht
weil sie nachrangig wäre, sondern weil sie dort mangels Alternative gelandet ist.
Das ist der eigentliche Befund hinter der gesamten Frage.

Die Gegnerzahl bleibt als **Untergrenze** sinnvoll — unterhalb von drei Zielen
fällt Sanctus wegen `AoeCount` ohnehin nicht, und Einzelziele sind meist
betäubungsimmun. Sie ist ein Filter, kein Gewicht. Die HP-Schwellen bleiben als
Notfallvorbehalt, nicht als Abwägungsgröße.

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

## Kritische Prüfung des Entwurfs

Vier Hindernisse, drei davon im Entwurf selbst. Alle sind aufgelöst, nicht nur
benannt.

### H1 — Der Einschub braucht einen Cast mit Eigenwert

Der DoT deckt genau einen GCD ab; danach bliebe nur Glare, dessen Einschub bei
vielen Zielen ein erheblicher Schadensverlust ohne eigenen Gegenwert wäre.

**Aufgelöst durch Rechnung: ein einziger Einschub genügt.** Bei einem GCD von 2,5 s
und Stundauern von 4 / 2 / 1 s:

| Verlauf | Betäubungsdeckung | Intervalle |
|---|---|---|
| ohne Einschub, Casts bei 0 / 2,5 / 5,0 | **5,5 s** | 0–4,5 · 5,0–6,0 |
| ein Einschub, Casts bei 0 / 5,0 / 7,5 | **7,0 s** | 0–4,0 · 5,0–7,0 · 7,5–8,5 |
| zwei Einschübe, Casts bei 0 / 5,0 / 10,0 | **7,0 s** | 0–4,0 · 5,0–7,0 · 10,0–11,0 |

Der zweite Einschub bringt nichts mehr und kostet einen weiteren GCD. Der DoT
reicht also exakt aus, und die Regel darf höchstens einmal je Betäubungsphase
greifen.

Die Zahlen stammen nicht aus einer Überschlagsrechnung, sondern aus
`.github/scripts/audit/stun_coverage.py`, das die Regel GCD für GCD simuliert und
mit im Repository liegt. Es weist zugleich die Kosten aus: Die Konzeptregel erreicht
die vollen 7 s für **genau einen** eingeschobenen GCD. Bei verkürztem GCD (2,0 s
unter Presence of Mind) liefert sie sogar lückenlose Deckung von 0 bis 7 s.

### H2 — Die Bedingung im Entwurf war falsch

`StunRemainingShortest > GCDTime(1)` hätte den einen nötigen Einschub gerade
verhindert: Nach dem ersten Sanctus beträgt die Restzeit beim nächsten GCD noch
1,5 s, also weniger als ein GCD, und die Regel hätte nicht gegriffen.

Richtig ist **`StunRemainingShortest > 0`** — nachcasten, solange nichts läuft. Das
ist zugleich selbstbegrenzend: Nach dem Einschub liegt der nächste Cast hinter dem
Ablauf des Stuns, die zweite und dritte Betäubung dauern 2 s und 1 s und sind beim
jeweils folgenden GCD bereits vorbei. Es entsteht automatisch genau ein Einschub,
ohne Zähler. Gegenprobe mit verkürztem GCD (2,0 s durch Presence of Mind): 7,0 s
statt 5,0 s, ebenfalls genau ein Einschub.

### H3 — Die Blockverschiebung widerspricht der Rangordnung

Der ursprüngliche Schritt 2 zog den DoT-Block vor den Sanctus-Block. Damit fiele
der **erste** Stun einen GCD später — eine Verzögerung der Schadensvermeidung
zugunsten von Schaden, also genau die Umkehrung des Vorrangs, der dieses Konzept
trägt. Zusätzlich wäre die Verschiebung der teuerste Teil des Plans im
Upstream-Merge.

**Aufgelöst durch Verzicht auf die Verschiebung.** Der Sanctus-Block bleibt, wo er
ist, und bekommt eine Aussetzbedingung; der bereits vorhandene DoT-Block darunter
fängt den freigewordenen GCD auf. Kein Block wandert, der Merge-Aufwand entfällt,
und der erste Stun fällt unverändert sofort.

### H4 — Welche Betäubungs-Status-Id gilt

Die Spieldaten führen `Stun` und rund ein Dutzend Varianten mit Zahlensuffix. Welche
davon Sanctus anlegt, ist offline nicht bestimmbar.

**Aufgelöst durch Bündelung**: eine Statusgruppe nach dem im Projekt etablierten
Muster (`StatusHelper.RangePhysicalDefense`, `.PhysicalResistance`) fasst alle
Betäubungs-Ids zusammen. Damit ist die Frage gegenstandslos, und fremde Betäubungen
zählen mit — was erwünscht ist, weil auch sie Schaden verhindern.

### H5 — gemessen wurde über die falsche Gegnermenge

Die Messskizze filterte auf `DataCenter.JobRange`. Für einen Heiler sind das **25
Yalms** (`DataCenter.cs:912`) — die Angriffsreichweite, nicht der Wirkradius von
Sanctus. Gemessen worden wäre also über Gegner, die der Zauber nie trifft; ferne,
nie betäubte Gegner hätten die Deckung dauerhaft unvollständig erscheinen lassen und
die Regel nie greifen lassen.

**Aufgelöst durch einen Radius-Parameter.** `SurveyStuns(radius, …)` bekommt den
Wirkradius der Aktion (`HolyIiiPvE.Info.EffectRange`), nicht die Jobreichweite.
Nebenwirkung: Die Messung gehört damit nicht in einen jobunabhängigen Updater,
sondern zur Aktion — womit die Einhängung in den `MajorUpdater` und die dafür nötige
Änderung an der Aktualisierungsreihenfolge entfallen.

### H6 — die Bedingung trägt gemischte Gegnergruppen nicht

`StunRemainingShortest > 0` beschreibt einen einzelnen Gegner. Im Pull kommen
laufend ungestunnte Gegner hinzu; dann sagt eine Restzeit über die bereits
betäubten nichts über die neuen, und die Regel würde strecken, obwohl ein Cast die
Neuzugänge mit **voller** Dauer erwischt hätte — die Resistenz zählt je Gegner.

**Aufgelöst durch zwei Wahrheitswerte statt einer Zeit**: gestreckt wird, wenn
**alle** Gegner im Radius betäubt sind, oder wenn **keiner** von ihnen noch betäubt
werden kann. Bei einem einzelnen Gegner ist das gleichbedeutend mit der
Restzeit-Bedingung, das Modell bleibt also gültig; bei gemischten Gruppen
entscheidet es richtig.

### Zwei Notfallvorbehalte entfallen nach Prüfung

Der Entwurf trug einen HP- und einen BMR-Vorbehalt. Beide schützen vor nichts:

- Der Dispatcher ruft sämtliche Heil- und Verteidigungszweige **vor** `GeneralGCD`
  auf. Ein kritischer Gruppenzustand erreicht diesen Code also gar nicht.
- Ein vorhergesagter Raidwide kommt vom Boss, nicht von den betäubbaren
  Trash-Gegnern. Die Betäubung ändert daran nichts, ein Zurückhalten der Streckung
  also auch nicht.

Ein Vorbehalt, der nachweislich nichts abfängt, ist toter Code und wurde nicht
gebaut.

### Was aus der Prüfung für den Plan folgt

Der frühere Schritt A ist kein eigener Schritt mehr. Er und die Streckung sind zwei
Gründe für dieselbe Aussetzung, und die Aussetzbedingung kennt beide: Bei einem Boss
mit zwei Adds laufen nach dem ersten Sanctus die Add-Betäubungen, die Streckung
greift, und der DoT landet auf dem Boss — der ursprüngliche A-Fall, mit besserem
Zeitpunkt als bei einer festen Blockverschiebung. Sind alle Ziele immun, greift
`!StunHeadroom` und führt zum selben Ergebnis.

## Umsetzungsplan

Drei Schritte statt vier. Jeder hinter einer eigenen Option, Standard aus.

### Schritt 1 — Messung (kein Verhalten, keine Option nötig)

Wie oben beschrieben: drei Felder in `DataCenter`, gefüllt von einem Updater
zwischen `TargetUpdater.UpdateTargets()` und `ActionUpdater.UpdateNextAction()`,
dazu `StatusHelper.StunStatus` als Gruppe. Verifiziert: `DataCenter.JobRange` ist
`public static float` (`DataCenter.cs:912`), `AllHostileTargets` ist
`public static List<IBattleChara>` (`:49`), Statusgruppen mit mehreren Ids sind das
etablierte Muster (`StatusHelper.cs:295-310`).

Kein Aufrufer in einer Rotation. Absicherung: Kompilierung und ein Prüfskript.

### Schritt 2 — Aussetzbedingung am Sanctus-Block

In `WHM_Reborn.GeneralGCD`, am vorhandenen Block (518-527), ohne ihn zu verschieben:

```
var stretch = HolyStretchEnabled
    && !EmergencyMitigationNeeded
    && NumberOfHostilesInRange >= HostileCountThreshold
    && (allStunned || !headroom)      // aus SurveyStuns(radius, out allStunned, out headroom)
    && (DiaPvE.CanUse(out _) || AeroIiPvE.CanUse(out _) || AeroPvE.CanUse(out _));

if (!stretch && HolyPvE.EnoughLevel)
{
    ... unveraendert ...
}
```

Die letzte Bedingung ist die **Ersatzgarantie**: Sanctus wird nur ausgesetzt, wenn
ein Cast mit eigenem Wert bereitsteht. Fehlt er, ist Sanctus die richtige Wahl, und
ein Ausweichen auf Glare findet nicht statt.

`EmergencyMitigationNeeded` fasst die beiden Notfallvorbehalte: Gruppen-HP unter der
Schwelle, oder vorhergesagter Schaden innerhalb `BMRRaidwideMitWindow` /
`BMRTankbusterMitWindow`.

### Schritt 3 — Übertragung auf andere Doppelnutzen-Aktionen

Erst nach Beobachtung im Spiel. Kandidatensuche über ein Prüfskript statt über
Erinnerung: Aktionen, die in einem Schadenszweig stehen und einen Statuseffekt mit
Mitigationswirkung anlegen.

### Was der Plan nicht vorsieht

Sanctus in den Verteidigungszweig zu verschieben, und den DoT-Block zu verschieben.
Beides ist durch die Aussetzbedingung überflüssig geworden.

## Falsifikation

**Hypothese 1: Es liegt kein Defekt vor.** Widerlegt durch zweierlei: die bezifferte
Größenordnung, und die Existenz von `GetCurrentMitigationPercent` — die
Entwurfsabsicht ist vorhanden, die Verdrahtung fehlt. Für A zusätzlich durch die
beiden Vorkehrungen an `ModifyDiaPvE`, die bei AoE nie zur Wirkung kommen.

**Hypothese 2: Die gewählte Option ist falsch.** Widerlegt für die Bremse, nicht für
einen Auslöser — siehe O2 und den belegten Rückbau. Für Schritt 3 hält der Einwand
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
deshalb vor Schritt 3, der Übertragung auf weitere Aktionen.

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
in eigene Regionen, statt bestehende Blöcke umzubauen. Nach der Auflösung von H3
verschiebt der Plan keinen Block mehr: Schritt 2 fügt dem vorhandenen Sanctus-Block
eine Bedingung voran und lässt seinen Inhalt unberührt, was den Merge-Aufwand auf
eine Zeile begrenzt.
