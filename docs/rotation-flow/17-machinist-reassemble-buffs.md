# 17 · Maschinist: Reassemble in die Gruppenbuffs legen?

**Leitfrage:** Soll der Maschinist eine Reassemble-Ladung für die Gruppenbuffs halten, und welcher Buff zählt am
meisten?

Das Dokument gibt den geltenden Stand; die Prüfhistorie steht in `AUDIT_LOG.md` (A235, A246), die zurückgenommene
frühere Empfehlung in C105. Vorgeschichte: Konzept 16, Maschinist.

## Entscheidung

**Nein. Reassemble fällt wie bisher auf das nächste Werkzeug; RSR hält keine Ladung für fremde Buffs und reagiert
auch nicht auf einen liegenden Buff mit einer anderen Werkzeugwahl.** Nicht gebaut (A246).

**Sein Auftrag (01.10.2026):** prüfen, „wer alles in der gruppe ist und gruppenbuffs liefern kann, wie häufig diese
gecasted werden könnten und wie die abklingzeit wäre, und welcher der gerade möglichen gruppenbuffs den größten
effekt hat … wie optimaler schaden erzielt werden kann vs. den nachteilen eines zurückhaltens."

## Begründung

- **Der einzige Grund zu halten ist das Verhalten anderer Spieler:** die erwartete Buff-Zeit der Gruppe. Seine Regel
  in `CLAUDE.md`: „Das Verhalten anderer Spieler ist eine Annahme, nie ein tragender Grund." Auch ein gemessener Takt –
  derselbe Buff zweimal im Abstand seiner Abklingzeit gesehen – sagt fremdes Verhalten voraus, statt es zu sehen. Ohne
  fremde Buffs gibt es keinen Gewinn; die Abhängigkeit ist das Wesen der Regel.
- **Der Gewinn liegt im Promillebereich:** 0,1 bis 0,5 % in vollen Gruppen, höchstens 0,2 % in seinen leichten Gruppen
  mit null bis zwei Buff-Jobs (Modell unten, angenommene Spielerwerte). Ein Gewinn dieser Größe ist nach seinen Regeln
  keine Abwägung gegen eine Regel, die auf fremdem Verhalten ruht.
- **Die Reaktion ohne Vorhersage** – liegt ein Buff, darf Reassemble auch auf Drill – stützt sich nicht auf fremdes
  Verhalten, bringt laut Simulation aber höchstens einen Reassemble mehr ins Fenster je zehn Minuten (11/2 gegen 11/3),
  also ebenfalls Promille.

## Die Gruppenbuffs (Spieldaten)

Aus den Wirktexten (xivapi, 01.10.2026); alle 20 s Dauer, 120 s Abklingzeit, Radiant Finale 110 s:

| Buff | Job | Wirkung | Wert für einen Reassemble-Treffer (Modell) |
|---|---|---|---|
| Battle Litany | Dragoon | +10 % Krit-Rate | 3,0 % — am höchsten |
| Chain Stratagem | Gelehrter | +10 % Krit-Rate (auf dem Gegner) | 3,0 % — am höchsten |
| Divination | Astrologe | +6 % Schaden | 2,2 % |
| Radiant Finale | Barde | +2/4/6 % Schaden je Coda | bis 2,2 % |
| Searing Light, Embolden, Brotherhood, Starry Muse, Technical Finish | Beschwörer, Rotmagier, Mönch, Piktomant, Tänzer | +5 % Schaden | je 1,8 % |
| Dokumori | Ninja | +5 % erlittener Schaden (Gegner) | 1,8 % |
| Arcane Circle | Schnitter | +3 % Schaden | 1,1 % — am niedrigsten |
| Battle Voice | Barde | +20 % Direkttreffer-Rate | nicht bezifferbar (unten) |
| Devilment | Tänzer | +20 % Krit- und Direkttreffer-Rate | nur für den Tanzpartner |

**Warum die Krit-Buffs vorn liegen:** Laut Wirktext erhöht ein Buff auf Krit- oder Direkttreffer-Rate unter
Reassemble den Schaden. Wie stark, sagt das Spiel nicht; Gemeinschaftsquellen (consolegameswiki, Allagan Studies)
nennen für Krit: Die Rate wird zum Krit-Multiplikator addiert. Für Direkttreffer nennt keine der beiden Quellen eine
Regel — Battle Voice bleibt deshalb unbeziffert. Die Rangfolge bleibt über angenommene Krit-Multiplikatoren 1,5–1,7,
Krit-Raten 20–30 % und Direkttreffer-Raten 30–50 % gleich (Selbsttest des Skripts); die Prozentwerte hängen an diesen
angenommenen Werten. Für die Entscheidung trägt keine dieser Zahlen: Sie scheitert an fremdem Verhalten als Grund,
nicht an der Höhe.

**Wie häufig:** Jeder Buff einmal je 120 s (Radiant Finale 110 s). Gruppen, die ihre Buffs zusammenlegen, haben ein
Fenster je zwei Minuten; in Zufallsgruppen liegen sie auseinander. Reassemble lädt alle 55 s mit zwei Ladungen (ab
Stufe 84, Enhanced Reassemble): Je zwei Minuten ließen sich höchstens zwei Ladungen in ein Fenster legen.

**Größenordnung** (`.github/scripts/audit/reassemble_buff_model.py`, angenommene Werte gekennzeichnet): Eine
Reassemble ist etwa drei Viertel einer 660er-Potenz wert (1320 gegen 835). In ein Buff-Fenster verschoben, bringt sie
je Buff 1 bis 4 Prozent eines Reassemble-Treffers mehr; in einer vollen Gruppe mit sechs Buffs rund 14 Prozent, bis zu
zwei verschobene Ladungen je zwei Minuten — zusammen etwa eine halbe 660er-Potenz je zwei Minuten.

## Was RSR zur Laufzeit weiß

- **Wer in der Gruppe ist:** `PartyComposition`, und daraus die Buffs der Gruppe (`StatusList`, `JobBuffs` — Devilment
  fehlt dort, es wirkt nur auf den Partner).
- **Ob ein Buff liegt:** am Spieler (`PlayerHasStatus(false, …)`, jede Quelle) bzw. am Gegner für Chain Stratagem und
  Dokumori. `HasBuffs` fragt „alle zugleich" und taugt für „irgendeiner" nicht.
- **Wann der nächste kommt:** nicht direkt — fremde Abklingzeiten sind nicht lesbar. Lesbar ist nur, wann ein Buff
  zuletzt anfing; jede Vorhersage daraus ist eine über fremdes Verhalten.

## Ausgeschlossen

- **Eine Ladung für das vorhergesagte Fenster halten (O1):** ruht auf fremdem Verhalten (Begründung). Ihre übrigen
  Kosten wären beherrschbar gewesen – gehalten wird nur eine von zwei Ladungen, eine Fehlvorhersage kostet höchstens
  55 s Warten und den Bonus, nicht die Ladung; Kampfende und Pausen ließen sich über `GetCorrectedTTK` und
  `BMRDowntimeWithin` abfangen, ein belegter Einwebeplatz über `BurstWeaveSlotContested` –, aber keine davon hebt den
  tragenden Grund auf. Unter Stufe 84 (eine Ladung) hätte sie ohnehin nicht halten dürfen.
- **Nur für das eigene Zwei-Minuten-Fenster halten (O2):** trifft die Gruppe nur zufällig.
- **Ein Planer über alle kommenden Fenster (O3):** dieselbe Abhängigkeit mit mehr Vorhersagen.
- **Im Fenster Reassemble auch auf Drill (O5):** ohne fremdes Verhalten, aber Promille (Begründung).
- **Full Metal Field als Ziel im Fenster:** Der Wirktext sagt „Delivers a critical direct hit … This action is not
  affected by Reassemble."

## Grenzen der Zahlen

- Die Simulation (`simulate`, `timeline_report`: zehn Minuten, GCD 2,5 s, Werkzeuge sobald bereit, zwei Ladungen,
  Countdown-Einsatz) blendet Überhitzung und andere Verzögerungen der Werkzeuge aus. Das begünstigt die Nullvariante,
  weil die Werkzeuge im Spiel gegen das Raster wandern. Reassembles gesamt / im Fenster:

  | Lage der Buff-Fenster | heute | nur Drill im Fenster (O5) | halten (O1) |
  |---|---|---|---|
  | im Zwei-Minuten-Takt ab Pull | 11 / 4 | 11 / 4 | 10 / 5 |
  | 30 s versetzt | 11 / 4 | 11 / 4 | 10 / 9 |
  | 70 s versetzt | 11 / 2 | 11 / 3 | 11 / 8 |

- Der Gewinn je zehn Minuten: volle Gruppe, sechs Buffs, rund 180 Potenz-Gegenwert im Gleichtakt und 900–1100 versetzt
  (gut ein GCD); leichte Gruppe 25–70 bzw. 125–420 – bei rund 240 GCDs in zehn Minuten 0,1 bis 0,5 Prozent.
- Ob die Reassembles im Spiel im Fenster lagen, zeigt ein FFLogs-Report.
