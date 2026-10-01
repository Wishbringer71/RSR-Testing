# 16 · Abgleich mit dem Regelkatalog von xivanalysis

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie steht in
`AUDIT_LOG.md` (A234).

## Ergebnis

**Auftrag (01.10.2026):** „generell schauen, welche regeln dort für gute rotationen gelten und was bei rsr
verbesserungswürdig wäre". Dazu seine Präzisierung: Sicherheit geht vor; eine Rotation, die deshalb weniger
Schaden macht als die Erwartung von FFLogs, ist nicht schlechter.

**Quelle und ihr Status:** `xivanalysis/xivanalysis`, Stand 25.09.2026 (f532855), MIT-Lizenz, nur gelesen.
Es wertet FFLogs-Reports je Job aus, Patch 7.0 bis 7.5, und bewertet auf Schaden hin. Eine
Community-Auswertung, keine Spielquelle; Code wird nicht übernommen. Der Katalog sind die Vorschlagstexte und
Erwartungen der Module unter `src/parser/core/modules` und `src/parser/jobs/<job>/modules`.

**Befund:** Eine Lücke, behoben. Sonst deckt RSR die mechanisch prüfbaren Regeln ab oder weicht mit Grund
ab — meist mit einer Sicherheitsregel, also gewollt.

- **Behoben, Beschwörer: Searing Flash verfiel mit Ruby's Glimmer** (A234). Außerhalb einer Demi wirkte
  `SMN_Reborn` es nur auf einen sterbenden Boss. Mit weiteren Beschwörern fällt Searing Light auch in einen
  Titan- oder Ifrit-Block (Konzept 12); die nächste Demi liegt dann weiter weg, als Ruby's Glimmer hält, und
  die Aktion ging verloren. Jetzt fällt sie zusätzlich im letzten Einschiebefenster vor dem Ende des Status
  (`IsLastChanceBeforeStatusEnds`). Als einziger Beschwörer ändert sich nichts: Dort liegt Searing Light vor
  Solar Bahamut, und Searing Flash fällt in der Demi.

## Allgemeine Regeln (alle Jobs)

| Regel bei xivanalysis | RSR | Einordnung |
|---|---|---|
| GCD rollen lassen (Always Be Casting), nicht zu viel einweben | Ausführungssperre, Animationssperre, Einweben nach Fenster | abgedeckt |
| Kombos nicht brechen | Kombologik je Aktion | abgedeckt |
| Procs nicht verfallen lassen, nicht überschreiben | „Granted windows" (seit 7.5.6.13): Aktionen mit Statusbedarf bis zum Ablauf nutzbar; je Job Regeln „vor Ablauf" | abgedeckt; Lücke nur, wo eine Rotation die Aktion zusätzlich an ein Fenster bindet (Searing Flash, behoben) |
| DoTs nicht auf unverwundbare Ziele, nicht zu früh erneuern | Ziele mit Unverwundbarkeit ausgeschlossen (`InvincibleStatus`); Erneuerung nach „Number of GCDs before the DOT/Status effect is reapplied" | abgedeckt; Zeitpunkt der Erneuerung ist Einstellung, Wirkung nicht gemessen |
| Flächenaktionen nur gegen genug Ziele | `AoeCount` je Aktion | abgedeckt |
| Swiftcast vor Ablauf für einen Zauber nutzen | Bei Jobs mit Wiederbelebung für sie zurückgehalten | **abweichend mit Grund:** seine Vorgabe |
| Lucid Dreaming auf Abklingzeit halten | Bei MP-Schwelle | **abweichend mit Grund, seine Entscheidung (01.10.2026):** „lucid dreaming ist absichtlich an mp gebunden, da ansonsten evtl. cast, wenn full mp und das fehlt später, wenn dann wirklicher bedarf an mp herrscht. auch das ist sicherheit!" — bei vollem MP verfiele die Wirkung, und die Abklingzeit fehlte, wenn MP wirklich gebraucht wird |
| Defensiven „zu hilfreichen Zeiten" nutzen | Gefahrbezogene Abwehr (Konzepte 08, 09, 13) | abgedeckt, nach seiner Sicherheitsregel |
| Overheal vermeiden | Heilziel nach Gefahr, Vorab-Heilung, Schild nicht als Heilung (Konzept 07) | abweichend, wo die Sicherheit früher heilen lässt |
| Nicht sterben | — | Sicherheitsregeln |

## Jobs in seinem Schwerpunkt

**Beschwörer.** Aetherflow nicht verlieren: Fester und Necrotize fallen spätestens, wenn Energy Drain in zwei
GCDs bereitsteht — abgedeckt. Ruin IV nicht in der Demi — abgedeckt (`UseFillers`). Ruin IV vor dem nächsten
Energy Drain: abgedeckt, gerechnet aus Spieldaten (xivapi, 01.10.2026). Energy Drain gewährt Further Ruin für
60 s und hat 60 s Abklingzeit; RSR wirkt es in jeder Demi. Ein Minutenzyklus mit 2,5-s-GCD braucht: Demi 15 s,
Ifrit 13,5 s (Beschwörung 2,5 + zwei Ruby Rite je 3,0 + Crimson Cyclone und Strike je 2,5), Titan 12,5 s
(Beschwörung + vier Topaz Rite je 2,5), Garuda 12 s (Beschwörung 2,5 + vier Emerald Rite je 1,5 + Slipstream
3,5) — zusammen 53 s, mit einer Demi-Beschwörung als GCD 55,5 s. Es bleiben 4,5 bis 7 s, also ein bis zwei
Füllerplätze je Minute, mit kürzerem GCD mehr; Ruin IV hat unter den Füllern den ersten Platz (`UseFillers`).
Verloren geht es nur, wenn eine Wiederbelebung oder Heilung diesen Platz nimmt — gewollt. Höchstens zwei Ruin III je Minute: folgt aus der Reihenfolge
(Beschwörungen und Riten vor Füllern), nicht gemessen. Enkindle und Deathflare je Demi, Rekindle je Phoenix,
Mountain Buster nach jedem Topaz — abgedeckt. Sechs Demi-GCDs je Beschwörung — abgedeckt, außer wo eine
Wiederbelebung oder Heilung einen Platz nimmt (gewollt). Searing Flash je Searing Light — behoben (oben).
Slipstream über die volle Dauer: hängt am Gegner, nicht steuerbar. Physick „in Gruppeninhalten nicht":
**abweichend mit Grund** — RSR heilt als Nicht-Heiler nach seinen Einstellungen, Sicherheit vor Schaden.
Swiftcast: siehe oben.

**Weißmagier.** Glare IV alle drei Ladungen, Lilien nicht überlaufen lassen, Blood Lily nicht überlaufen,
Afflatus Solace vor Cure II, Divine Caress nach Temperance — abgedeckt. Divine Benison und Aquaveil „so oft
wie möglich", Temperance „oft", Divine Benison „nicht für Tankbuster halten": **abweichend mit Grund** — RSR
setzt sie gefahrbezogen ein und streckt sie über die Zeit (Konzept 08), Sicherheit vor Schaden. Dia nicht zu
früh erneuern: Einstellung, siehe allgemeine Regeln.

**Krieger.** Beast Gauge nicht überlaufen (Infuriate nur bis 50), Nascent Chaos nicht überschreiben (Infuriate
liefert den Status), Inner Release, Primal Wrath und Primal Ruination nutzen, Surging Tempest halten —
abgedeckt. Primal Rend nur in Zielnähe (`PrimalRendDistance2`, sonst nur mit den YEET-Optionen):
**abweichend mit Grund** — seine Bewegungsregel; verfällt Primal Rend Ready, weil das Ziel weiter weg steht,
ist das gewollt.

**Maschinist.** Battery nicht überlaufen (Königin vor 100), Heat nicht überlaufen, Wildfire mit Hypercharge,
Königin vor einer vorhergesagten Pause beenden, Double Check und Checkmate nicht über die Ladungen —
abgedeckt. Reassemble: xivanalysis erlaubt Drill, Air Anchor, Chain Saw, Excavator; RSR nimmt Drill erst
unterhalb von Chain Saw. Kein Verlust, gerechnet aus Spieldaten (xivapi, 01.10.2026): Alle vier treffen mit
Potenz 660, ein Reassemble auf Drill brächte also nicht mehr. Reassemble lädt alle 55 s, höchstens zwei
Ladungen. Air Anchor (40 s), Chain Saw (60 s) und Excavator (nach Chain Saw) geben in zwei Minuten sieben
geeignete GCDs auf gut zwei Ladungen; der längste Abstand zwischen zwei geeigneten GCDs ist höchstens 40 s,
kürzer als eine Ladezeit — eine Ladung läuft nie über.

*Die übersehene Wechselwirkung (seine Nachfrage, 01.10.2026):* Für die Potenz ist Drill gleichwertig, für den
Zeitpunkt nicht. Reassemble macht den größten Einzeltreffer des Maschinisten, und Gruppenbuffs vergrößern ihn
weiter; Buffs auf Kritische- oder Direkttrefferrate wirken unter Reassemble als Schaden („Increases damage dealt
when under an effect that raises critical hit rate or direct hit rate", Wirktext). The Balance (Leitfaden, keine
Spielquelle): „we can place Reassemble on any four of these GCDs depending on our opening alignment, the raid
buffs we have in our party" und „Attempt to use a Reassemble under raid buffs if you can. Reassemble charges can
be held for a pot window at two minutes". RSR legt Reassemble auf das nächste geeignete Werkzeug, sobald eine
Ladung da ist (`usedUp: true`), ohne Blick auf Gruppenbuffs. Drill einfach zuzulassen, änderte daran nichts —
eine Ladung ginge nur früher weg. Wert bekommt Drill erst mit einer Regel, die eine Ladung für das Buff-Fenster
aufhebt: Drill hat zwei Ladungen und steht dort fast immer bereit. Wie groß der Gewinn ist, hängt an Gruppe und
Buff-Zeiten und ist von hier nicht bezifferbar. Mehrzielig bleibt Drill unterlegen: Chain Saw und Excavator
treffen mehrere Gegner mit dem garantierten Treffer, Drill einen. Zur Entscheidung vorgelegt (TODO).

## Übrige Kampfjobs

**Die Klasse „Proc verfällt, Leiste läuft über, Status überschrieben" ist für alle Jobs schon geprüft**
(Konzept 14, „Werden die Fenster genutzt", A165–A167): Die Aktionsmatrix listet jeden Verbraucher eines
Fensters, dessen Aufrufe alle an eine eigene Bedingung gebunden sind und keinen Rückfall vor Ablauf tragen;
jeder Kandidat ist dort von Hand bewertet. Die xivanalysis-Regeln dieser Klasse decken sich mit dieser Liste.
Eine Bewertung dort war unvollständig — Searing Flash, oben behoben; die Matrix führt es jetzt „mit Rückfall vor
Ablauf". Die übrigen Kandidaten tragen eine Bedingung, die das Fenster selbst ist (Schnitter, Viper, Monk,
Ninja-Mudras, Rotmagier, Pictomancer), deren Zweck sie ist (Heilbedarf, Burst, Tanzschritte) oder eine
Sicherheitsregel (Primal Rend).

Am Code nachgesehen, weil xivanalysis sie eigens nennt: Baneful Impaction, Fire's und Wind's Reply,
Prefulgence, Tenri Jindo, Zanshin tragen eine Ablaufregel; Polyglot, Esprit, Federn, Munition, Blood, MP des
Dunkelritters eine Leistengrenze; Karten des Astrologen werden vor dem nächsten Ziehen ausgespielt.

Nicht einzeln geprüft sind die Fenstererwartungen (etwa acht GCDs je Lance Charge, zwölf je Heiltrank) und die
Reihenfolgeregeln (Viper, Schnitter, Paladin im Fight or Flight). Sie hängen an der Fensterplanung der
Rotationen und lassen sich am Code nicht ohne Kampfverlauf beurteilen; ein FFLogs-Report, durch xivanalysis
gelaufen, zeigt sie (er zeichnet mit ACT auf).

## Was ausgeschlossen wurde und warum

- **Regeln übernehmen, die seiner Sicherheitsregel widersprechen** (Swiftcast verbrauchen, Defensiven auf
  Abklingzeit statt auf Gefahr, Primal Rend auf Distanz): ausgeschlossen, seine Vorgabe.
- **xivanalysis als Maßstab für „besser"**: ausgeschlossen. Weniger Schaden aus einer Sicherheitsregel ist
  gewollt (seine Präzisierung); der Abgleich nennt Abweichung und Grund.
- **Laufzeitmessung über FFLogs als Teil einer Regel**: ausgeschlossen, sie wartete auf sein Hochladen und
  eine spätere Auswertung (`CLAUDE.md`, „Entscheidung zur Laufzeit"). Ein Report bleibt ein Prüfmittel.

## Falsifikation

- **Kein Defekt bei Searing Flash?** Widerlegt am Code: Außerhalb einer Demi verlangte die Rotation einen
  sterbenden Boss; Ruby's Glimmer läuft unabhängig davon ab.
- **Option falsch?** Hält nicht: Searing Flash kostet keinen GCD; im letzten Fenster vor Ablauf verdrängt es
  nichts, was später noch ginge. Lux Solaris hat Vorrang bei ihrem eigenen Ablauf (Konzept 08).
- **Ausgeliefert, und nichts ändert sich?** Als einziger Beschwörer: ja, beabsichtigt. Mit weiteren
  Beschwörern greift die Regel nur, wenn Searing Light außerhalb einer Demi fiel.
- **Klasse unvollständig erfasst?** Die Matrix findet nur Verbraucher mit eigener Bedingung in jedem Aufruf;
  ein Fenster, das an anderer Stelle verfällt (etwa weil ein höherer Zweig den Platz nimmt), zeigt erst ein
  Report.
