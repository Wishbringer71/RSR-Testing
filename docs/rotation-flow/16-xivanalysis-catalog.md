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
| Lucid Dreaming auf Abklingzeit halten | Bei MP-Schwelle | abweichend; ab Werk wird es erst gebraucht, wenn MP fehlt. Ob „auf Abklingzeit" mehr MP für Wiederbelebungen bringt, ist nicht gemessen |
| Defensiven „zu hilfreichen Zeiten" nutzen | Gefahrbezogene Abwehr (Konzepte 08, 09, 13) | abgedeckt, nach seiner Sicherheitsregel |
| Overheal vermeiden | Heilziel nach Gefahr, Vorab-Heilung, Schild nicht als Heilung (Konzept 07) | abweichend, wo die Sicherheit früher heilen lässt |
| Nicht sterben | — | Sicherheitsregeln |

## Jobs in seinem Schwerpunkt

**Beschwörer.** Aetherflow nicht verlieren: Fester und Necrotize fallen spätestens, wenn Energy Drain in zwei
GCDs bereitsteht — abgedeckt. Ruin IV nicht in der Demi — abgedeckt (`UseFillers`). Ruin IV vor dem nächsten
Energy Drain: Ruin IV steht unter den Füllern an erster Stelle; ob zwischen zwei Energy Drains immer ein
Füllerplatz liegt, ist nicht gemessen. Höchstens zwei Ruin III je Minute: folgt aus der Reihenfolge
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
unterhalb von Chain Saw. Ob dadurch Ladungen von Reassemble verfallen, ist nicht gemessen.

## Übrige Kampfjobs, Stichprobe

Geprüft wurden die mechanisch prüfbaren Regeln „Proc verfällt", „Leiste läuft über", „Status überschrieben"
— die Klasse, in der die Lücke beim Beschwörer lag. Bei allen Stichproben steht in der Rotation eine Regel
„vor Ablauf" oder eine Leistengrenze:

- Astrologe: Karten vor dem nächsten Ziehen ausgespielt (`WillHaveOneCharge(3)`), Lightspeed mit Divination.
- Schwarzmagier: Polyglot nicht überlaufen (`IsPolyglotStacksMaxed`), Flare Star.
- Barde, Tänzer: Procs; Esprit ab 70 oder im Burst, Fan Dance bei vier Federn und Procs.
- Dragoon: Wyrmwind Thrust, Starcross, Nastrond.
- Dunkelritter: Blood gesammelt und ab 70 ausgegeben, Edge of Shadow ab 8500 MP, Salt and Darkness.
- Revolverklinge: Munition und Bloodfest (`OvercappedAmmo`), Continuation zuerst.
- Mönch: Fire's und Wind's Reply mit Ablaufregel.
- Ninja: Tenri Jindo, Phantom Kamaitachi (Ablaufregel seit 7.5.6.13).
- Piktomant: Star Prism unter Starstruck, Rainbow Drip.
- Rotmagier: Prefulgence mit Ablaufregel, Vice of Thorns, Grand Impact.
- Schnitter: Perfectio; Samurai: Zanshin, Tendo, Ogi Namikiri (Ablaufregel).
- Gelehrter: Baneful Impaction mit Ablaufregel.

Nicht einzeln geprüft sind die Fenstererwartungen (etwa acht GCDs je Lance Charge, zwölf je Heiltrank), die
Reihenfolgeregeln (Viper, Schnitter, Paladin im Fight or Flight) und Paladin, Weiser, Viper im Einzelnen. Sie
hängen an der Fensterplanung der Rotationen und lassen sich am Code nicht ohne Kampfverlauf beurteilen; ein
FFLogs-Report, durch xivanalysis gelaufen, zeigt sie (er zeichnet mit ACT auf).

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
- **Stichprobe zu klein?** Möglich: Geprüft ist die Klasse „Proc verfällt" an den naheliegenden Aktionen, nicht
  jede Aktion jedes Jobs. Ein Report zeigt weitere Fälle.
