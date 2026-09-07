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

In Sekunden-Äquivalenten verhinderten Schadens, bei gleichmäßigem Gegnerschaden.
Reprisal senkt den Gegnerschaden um 10 % für 10 s; die Betäubung durch Sanctus wirkt
4 s, bei Wiederholung 2 s und 1 s, danach ist das Ziel 45 s immun.

| | Betäubung im Reprisal-Fenster | Betäubung außerhalb |
|---|---|---|
| Betäubungsanteil | 4 s × 100 % = 4,0 | 4 s × 100 % = 4,0 |
| Reprisal-Anteil | 6 s × 10 % = 0,6 | 10 s × 10 % = 1,0 |
| **Summe** | **4,6** | **5,0** |

Über einen 90-Sekunden-Pull mit zwei Reprisal-Anwendungen und zwei Resistenzzyklen:
getrennt 2,0 + 14,0 = **16,0**, vollständig überlappt 14,0 + 0,6 = **14,6**. Rund
ein Zehntel der Mitigation geht an der Überlappung verloren, dazu die GCDs, die
dafür aufgewendet wurden.

Modell, keine Messung. Annahmen: gleichmäßiger Schaden, keine Bewegung, alle
Werkzeuge verfügbar.

### Zwei gegenläufige Skalierungen

Die Tabelle rechnet **pro Gegner**. Der absolute Nutzen der Betäubung multipliziert
sich mit der Zahl der getroffenen Ziele, weil alle drei Quellen flächig wirken. Aus
1,4 Sekunden-Äquivalenten werden bei fünfzehn Gegnern einundzwanzig.

Der Wert eines eingeschobenen Einzelziel-Casts skaliert dagegen **nicht** mit der
Gegnerzahl, und der Preis des Einschubs — ein entgangener Sanctus — steigt mit ihr.
Daraus folgt kein Widerspruch, sondern ein Regimewechsel:

| Gegnerzahl | Was den Einschub trägt |
|---|---|
| 3 bis 4 | Der DoT-Wert. Der entgangene Sanctus-Schaden ist klein, die DoT-Uptime relativ viel wert |
| ab etwa 8 | Der Betäubungswert. Der DoT fällt kaum ins Gewicht, die gestreckte Betäubung dagegen stark |
| dazwischen | Beides mittel — das ist der Bereich, in dem die Regel am wenigsten trägt und am ehesten falsch liegen kann |

Der Einschub ist damit in beiden Randbereichen aus je eigenem Grund richtig. Das
macht ihn robuster als eine Regel, die nur eine der beiden Begründungen kennt, und
benennt zugleich, wo er am schwächsten ist.

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

### Schritt 1 — Zielbezogene Messung (kein Verhalten)

Neu in `CustomRotation_OtherInfo`, neben `GetCurrentMitigationPercent`:

```
protected static float StunRemainingOn(IBattleChara? target)
protected static bool  StunStillEffectiveOn(IBattleChara? target)
protected static float MitigationFactorOn(IBattleChara? target)
```

`StunRemainingOn` liest `target.StatusTime(false, StatusID.Stun, …)`.
`StunStillEffectiveOn` prüft `StunResistance` samt Stapelzahl über
`target.StatusStack`. `MitigationFactorOn` multipliziert den Gruppenfaktor aus
`GetCurrentMitigationPercent()` mit den zielbezogenen Anteilen (Betäubung, Slow).

Die bestehende Funktion bleibt unverändert, damit die UI-Anzeige stabil bleibt.
Kein Aufrufer in einer Rotation — dieser Schritt ändert nichts und ist allein durch
Kompilierung und ein Prüfskript abgesichert.

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
protected bool StunWouldBeWasted(IBattleChara? target) =>
    !StunStillEffectiveOn(target)                       // resistent oder immun
    || StunRemainingOn(target) > GCDTime(1)             // läuft noch, Einschub streckt
    || MitigationFactorOn(target) <= StunYieldThreshold; // fremde Mitigation trägt gerade
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

| Ebene | Möglich | Nicht möglich |
|---|---|---|
| Statisch | Prüfskript nach Bauart der vorhandenen Audit-Skripte, mit Selbsttest: findet Aktionen mit Mitigationswirkung im Schadenszweig und gleicht sie gegen die Erhebung aus Schritt 1 ab | — |
| Kompilierung | CI | — |
| Laufzeit | Die UI zeigt den Mitigationsfaktor bereits; ein Verlauf statt eines Momentwerts würde Überlappungen sichtbar machen | Beweis, dass die Rotation besser spielt |

Weil der Nutzen nicht belegbar ist, gilt die Feature-Toggle-Regel: **jeder Schritt
hinter eine eigene Option, Standard aus.**

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
