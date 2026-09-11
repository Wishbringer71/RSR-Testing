# Searing Light bei mehreren Beschwörern

*Zum Namen: Der Auftraggeber nennt die Aktion „Gleißender Schein". Der Job-Guide von Square Enix ist
vom Egress gesperrt — erneut geprüft, nicht erinnert —, eine belegte Zuordnung deutscher zu
englischer Bezeichnung steht damit nicht zur Verfügung. Dass **Searing Light** gemeint ist, ist aus
der Fragestellung geschlossen (ein Gruppenbuff, bei dem acht Beschwörer kollidieren können) und als
Schluss gekennzeichnet. Das Dokument benutzt durchgehend den englischen Bezeichner.*

## Sachstand

Bei **einem** Beschwörer ist der Ablauf richtig. Ab **zwei** entstehen drei Verluste, und sie wachsen
mit der Zahl der Beschwörer: der blockierte Spieler zündet in diesem Fenster nicht mehr, er behandelt
den laufenden fremden Buff nicht als Buff-Fenster für seine Aetherflow-Ausgaben, und er bekommt
mangels eigener Zündung kein Ruby's Glimmer und damit kein Searing Flash.

Der Doppelzündungsschutz selbst ist vorhanden und richtig gebaut. Was fehlt, ist die Behandlung des
Zustands **nach** dem Blockieren.

## Wann Searing Light gewirkt wird

Die Kette in `SMN_Reborn.cs`, vollständig:

| Stufe | Stelle | Bedingung |
|---|---|---|
| Dispatcher | `AttackAbility`, `:204` | `burstInSolar` — Solar Bahamut läuft (ab Stufe 100), sonst Bahamut, unterhalb der Beschwörungsstufe immer wahr |
| Aktion | `SummonerRotation.cs:490` | `ActionCheck = InCombat`, `TargetType.Self` |
| Sperre | derselbe Block | `StatusProvide = [SearingLight]` mit `StatusFromSelf = false` |

Die Beschwörung ihrerseits ist an Searing Light gekoppelt: `SMN_Reborn.cs:467` verlangt für Solar
Bahamut `IsBurst && !SearingLightPvE.Cooldown.IsCoolingDown`. Beide Zyklen hängen also aneinander —
Searing Light fällt nur während der großen Beschwörung, und die große Beschwörung wartet auf Searing
Lights Wiederholzeit.

**Größenordnungen** (Fremdquelle, siehe Nachweisgrenzen): Searing Light wirkt 20 Sekunden, hat 120
Sekunden Wiederholzeit und erhöht den Schaden der Gruppe um 5 %. Solar Bahamut steht 15 Sekunden.
Das Buff-Fenster ist damit **länger** als die Beschwörung, in der es gezündet werden darf.

## Der Kollisionsschutz und seine Reichweite

`IsStatusProvided` (`ActionBasicInfo.cs:691`) blockiert die Aktion, wenn der Spieler den Status
`SearingLight` trägt und dieser nicht innerhalb von `StatusRefreshGcdCount` GCDs endet. Zwei
Vorgabewerte bestimmen die Schärfe: `ShouldCheckStatus = true` und `StatusRefreshGcdCount = 2`
(`ActionConfig.cs:50`, `:66`). Der Schutz löst sich also **zwei GCDs vor Ablauf** des fremden Buffs,
nicht erst danach.

Entscheidend ist `StatusFromSelf = false`: `PlayerGetStatus` (`StatusHelper.cs:1529`) filtert nur bei
`isFromSelf` auf `status.SourceId == playerId`. Mit `false` zählt **jeder** Searing Light, von wem
auch immer. Das ist die richtige Einstellung für diese Frage, und sie ist bewusst gesetzt.

Ihr Gegenstück ist ebenso bewusst: `HasSearingLight` (`SummonerRotation.cs:271`) ruft
`PlayerHasStatus(true, …)`, zählt also **nur den eigenen** Buff. Auch das ist für seine ursprüngliche
Frage richtig — „läuft mein Burst-Fenster" —, und genau daraus entsteht der zweite Verlust unten.

## Die Fälle von einem bis acht Beschwörern

Die Zahl der Beschwörer ändert nichts an der Mechanik, nur an der Höhe des Verlusts. Qualitativ gibt
es drei Bereiche.

| Anzahl | Was geschieht | Bewertung |
|---|---|---|
| **1** | Zündet zu Beginn der eigenen großen Beschwörung, Buff 20 s, Aetherflow-Ausgaben liegen im eigenen Fenster, Ruby's Glimmer kommt, Searing Flash folgt | richtig, kein Befund |
| **2** | Einer zündet, der zweite wird gesperrt. Beim zweiten: kein Searing Light in diesem Fenster, keine bevorzugte Aetherflow-Ausgabe, kein Searing Flash | erster Verlust, drei Teile |
| **3 – 5** | Wie 2, mit *n−1* gesperrten Spielern je Fenster | Verlust wächst linear |
| **6 – 8** | Wie 2. Zugleich wäre ab sechs Beschwörern **durchgehende** Buff-Abdeckung möglich (6 × 20 s = 120 s = eine Wiederholzeit), und genau die entgeht | Verlust am größten, zugleich der größte entgangene Gewinn |

**Der Grund, warum der Gesperrte nicht nachzündet:** Seine Zündbedingung ist `burstInSolar`. Der
fremde Buff hält 20 Sekunden, seine eigene Beschwörung nur 15. Wenn die Sperre sich löst — zwei GCDs
vor Buff-Ende, also etwa bei Sekunde 15 — ist sein Beschwörungsfenster gerade abgelaufen. Er zündet
erst, wenn die nächste große Beschwörung läuft **und** zu diesem Zeitpunkt kein fremder Buff steht.
Bei gleichzeitigem Pull laufen alle Zyklen synchron, sodass sich die Lage bei jeder Runde
wiederholt.

**Ein zweiter Ausgang ist nicht auszuschließen und hier nicht entscheidbar:** Wenn alle Beschwörer im
selben Sekundenbruchteil zünden, hat der Status den Serverumlauf noch nicht hinter sich, keiner sieht
ihn, und alle zünden. Dann sind *n−1* Ladungen verbraucht statt zurückgehalten — bei gleichem
Buff-Ergebnis. Welcher der beiden Ausgänge eintritt, hängt am Zeitversatz zwischen den Spielern und
ist nur im Spiel zu beobachten.

## Die drei Verluste, einzeln

**1 — Das Buff-Fenster wird nicht nachbesetzt.** Beschrieben oben. Nicht der Schutz ist falsch,
sondern seine Folgenlosigkeit: Wer gesperrt wurde, hat keinen zweiten Anlauf innerhalb desselben
Zyklus.

**2 — Der laufende fremde Buff gilt nicht als Buff-Fenster.** `SMN_Reborn.cs:320`, `:332`, `:348`
bevorzugen Painflare, Necrotize und Fester unter `inSolarUnique && HasSearingLight` — und
`HasSearingLight` zählt nur den eigenen Buff. Ein gesperrter Beschwörer hält seine
Aetherflow-Ausgaben also zurück, während ein 5-%-Buff auf ihm liegt, und gibt sie nur über die
Nebenbedingungen aus (sterbender Boss, drohender Überlauf bei `EnergyDrainPvE.Cooldown.WillHaveOneChargeGCD(2)`).
Das ist der klarste der drei Befunde: Für die Frage „lohnt sich mein Aetherflow-Schaden jetzt
besonders" ist die Herkunft des Buffs ohne Bedeutung. Die Umsetzung weicht hier von der erkennbaren
Absicht ab.

**3 — Searing Flash entfällt.** `ModifySearingFlashPvE` verlangt `StatusNeed = [RubysGlimmer]`
(`SummonerRotation.cs:542`), und Ruby's Glimmer entsteht laut Fremdquelle aus der **eigenen**
Ausführung von Searing Light (Trait ab Stufe 96). Wer nie zündet, bekommt es nie. Dieser Verlust
folgt aus Verlust 1 und verschwindet mit dessen Behebung.

## Vorschläge

### V1 — Den laufenden Buff als Buff-Fenster behandeln

**Kontext:** Verlust 2. **Betroffene Stellen:** `SMN_Reborn.cs:320`, `:332`, `:348`.

**Mechanismus:** Die drei Bedingungen fragen zusätzlich nach einem Searing Light beliebiger Herkunft,
etwa über eine benannte Eigenschaft neben `HasSearingLight`, die `PlayerHasStatus(false, …)` ruft.

**Konsequenzen:** Bei einem Beschwörer ändert sich **nichts** — beide Prüfungen fallen zusammen. Ab
zwei landen die Aetherflow-Ausgaben des Gesperrten im laufenden Fenster statt außerhalb. Ein Nachteil
ist nicht erkennbar: Der Buff wirkt multiplikativ auf den Schaden, unabhängig davon, wer ihn gesetzt
hat. Betroffenenkreis: nur Endnutzer, nur Beschwörer.

**Bewertung:** Defektbehebung, keine Geschmacksfrage — die Bedingung sollte „im Buff-Fenster" heißen
und sagt „in meinem Buff-Fenster".

### V2 — Nachzünden, sobald der Buff ausläuft

**Kontext:** Verlust 1. **Betroffene Stelle:** `SMN_Reborn.cs:204`.

**Mechanismus:** Die Zündung nicht allein an `burstInSolar` binden, sondern zusätzlich zulassen, wenn
kein Searing Light steht, die eigene Wiederholzeit frei ist und der Kampf läuft.

**Konsequenzen, und hier liegt der Haken:** Für einen einzelnen Beschwörer ist die Kopplung an die
eigene Beschwörung **richtig** — sie bündelt den Gruppenbuff mit dem eigenen Schadensfenster und mit
dem Zwei-Minuten-Takt der übrigen Gruppe. Eine Lockerung würde bei einem Beschwörer Schaden kosten,
bei mehreren welchen gewinnen. Die Wirkung ist mit statischer Prüfung nicht zu belegen.

**Bewertung:** Verhaltensänderung ohne Nachweismöglichkeit. Nach der Projektregel gehört sie hinter
eine abschaltbare Einstellung mit dem bisherigen Verhalten als Vorgabe — und selbst dann bleibt
offen, ob der Nutzen die zusätzliche Einstellung rechtfertigt.

### V3 — Nichts tun

**Konsequenz:** Acht Beschwörer in einer Gruppe sind kein Nutzungsprofil, sondern ein Grenzfall. Zwei
sind es allerdings nicht: Eine Gruppe mit zwei Beschwörern ist gewöhnlich, und dort greifen alle drei
Verluste bereits vollständig.

**Bewertung:** Für V2 tragfähig, für V1 nicht.

## Empfehlung

**V1 umsetzen, V2 nicht.** V1 behebt eine Abweichung zwischen Absicht und Umsetzung, ist bei einem
Beschwörer wirkungslos und hat keinen erkennbaren Nachteil. V2 tauscht einen belegten Nutzen im
Regelfall gegen einen unbelegten im Sonderfall und käme allenfalls als abschaltbare Einstellung in
Frage.

Nicht umgesetzt, weil der laufende Vorgang es nicht verlangt: Der Auftrag war die Erarbeitung im
Konzept. Der Zweig `claude/raise-swiftcast-weave-2` trägt zudem bereits fünf ungemessene Eingriffe
am Wiederbelebungspfad; ein sechster, sachfremder würde die Auswertung des offenen Spieltests
beschädigen.

## Erfasst, nicht bearbeitet

`ChurinSMN.cs` trägt dasselbe Muster: Die Zündung hängt an `BahamutBurst && InBigSummon &&
BigSummonGCDLeft <= 5` (`:953`), die Aetherflow-Ausgabe an `HasSearingLight` (`:1015`), also erneut am
eigenen Buff. Fremde Rotationsdatei mit eigener Abstimmung — der Befund wird benannt, nicht behoben.

## Grenzen des Nachweises

Statische Prüfung am Quelltext für die gesamte Kette: Dispatcher, Aktionseinstellung, Statusprüfung,
Vorgabewerte der Aktionskonfiguration und die Filterung nach Statusquelle sind alle am Artefakt
belegt.

Aus Fremdquellen und damit außerhalb dieses Baums: Wirkdauer, Wiederholzeit und Stärke von Searing
Light, die Standzeit von Solar Bahamut und die Herkunft von Ruby's Glimmer.

Nicht entschieden: welcher der beiden Ausgänge bei gleichzeitiger Zündung eintritt (Sperre greift
oder Serverumlauf ist zu langsam), und wie oft die große Beschwörung bei einem gesperrten Beschwörer
tatsächlich wiederkehrt — sein Searing-Light-Cooldown läuft mangels Zündung nicht an, was den
gekoppelten Zyklus verschiebt. Beides braucht Laufzeitbeobachtung.
