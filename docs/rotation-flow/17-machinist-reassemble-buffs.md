# 17 · Maschinist: Reassemble in die Gruppenbuffs legen?

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie steht in
`AUDIT_LOG.md` (A235). Vorgeschichte: Konzept 16, Maschinist.

## Ergebnis

**Auftrag (01.10.2026):** prüfen, „wer alles in der gruppe ist und gruppenbuffs liefern kann, wie häufig diese
gecasted werden könnten und wie die abklingzeit wäre, und welcher der gerade möglichen gruppenbuffs den größten
effekt hat … wie optimaler schaden erzielt werden kann vs. den nachteilen eines zurückhaltens."

**Empfehlung nach den Antithesen (unten), zur Entscheidung:** O1 bauen, als Option, ab Werk an. Die Regel wird
nur aktiv, wenn die Gruppe Buff-Jobs hat — RSR erkennt sie im Spiel (`PartyComposition`, `JobBuffs`; sein Hinweis,
01.10.2026). Ohne Buff-Jobs ändert sich nichts; mit ihnen wächst der Gewinn mit Zahl und Stärke der Buffs, bis gut
eine GCD je zehn Minuten in vollen Gruppen mit versetzten Buffs. Schranken: gemessener Takt (derselbe Buff zweimal
im Abstand seiner Abklingzeit gesehen), Zeit bis zum Tod, freier Einwebeplatz, und zwei Ladungen (ab Stufe 84;
darunter hielte die Regel die einzige Ladung fest). Sicherheit ist nicht berührt. Offene Teilfrage an ihn: ob „Burst aus“
auch dieses Halten abschaltet (Abschnitt „Alle Fälle“).

**Größenordnung, Modell** (`.github/scripts/audit/reassemble_buff_model.py`, angenommene Werte gekennzeichnet):
Eine Reassemble ist etwa drei Viertel einer 660er-Potenz wert (1320 gegen 835). In ein Buff-Fenster verschoben,
bringt sie je Buff 1 bis 4 Prozent eines Reassemble-Treffers mehr. In einer vollen Gruppe mit sechs Buffs im
selben Fenster sind es rund 14 Prozent, bis zu zwei verschobene Ladungen je zwei Minuten — zusammen etwa eine halbe
660er-Potenz je zwei Minuten. In seiner üblichen leichten Gruppe mit null bis zwei Buff-Jobs ein Bruchteil davon.

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
nennen für Krit: Die Rate wird zum Krit-Multiplikator addiert. Für Direkttreffer nennt keine der beiden Quellen
eine Regel — Battle Voice bleibt deshalb unbeziffert. Die Rangfolge bleibt über angenommene Krit-Multiplikatoren
1,5–1,7, Krit-Raten 20–30 % und Direkttreffer-Raten 30–50 % gleich (Selbsttest des Skripts); die Prozentwerte
selbst hängen an diesen angenommenen Werten des Spielers.

**Wie häufig:** Jeder Buff einmal je 120 s (Radiant Finale 110 s). In Gruppen, die ihre Buffs zusammenlegen, gibt es
ein Fenster je zwei Minuten mit allen Buffs; in Zufallsgruppen liegen sie auseinander, dann gibt es mehrere
schwächere Fenster. Reassemble lädt alle 55 s, zwei Ladungen: Je zwei Minuten lassen sich höchstens zwei Ladungen
in ein Fenster legen. Bei auseinanderliegenden Buffs gehört die Ladung in das Fenster mit dem größten Wert —
Krit-Buffs und Divination vor den 5-%-Buffs.

## Was RSR zur Laufzeit weiß

- **Wer in der Gruppe ist:** `PartyComposition`, und daraus die Buffs der Gruppe (`StatusList`, `JobBuffs` —
  Devilment fehlt dort, es wirkt nur auf den Partner).
- **Ob ein Buff liegt:** am Spieler (`PlayerHasStatus(false, …)`, jede Quelle) bzw. am Gegner für Chain Stratagem
  und Dokumori. `HasBuffs` fragt „alle zugleich" und taugt für „irgendeiner" nicht; die Regel braucht den Wert der
  liegenden Buffs.
- **Wann der nächste kommt:** nicht direkt — fremde Abklingzeiten sind nicht lesbar. Lesbar ist, wann ein Buff
  zuletzt anfing (Restzeit beim ersten Sehen). Vorhersage: letzter Beginn plus Abklingzeit aus den Spieldaten.
  **Selbstnachsteuerung:** Jede neue Beobachtung ersetzt die Vorhersage; kommt ein Buff nicht, verfällt sie nach
  einer Abklingzeit plus Fensterdauer, und die Regel hält nicht mehr.
- **Erstes Fenster im Kampf:** ohne Beobachtung; die Gruppen legen ihre Buffs in den Opener, und dort liegt bereits
  RSRs Reassemble aus dem Countdown. Die Regel greift ab dem zweiten Fenster.

## Optionen

- **O0 — Nullvariante:** Reassemble aufs nächste Werkzeug. Treffer im Fenster nur zufällig.
- **O1 — eine Ladung für das vorhergesagte Fenster halten:** Hat die Gruppe Buff-Jobs und kommt das nächste
  vorhergesagte Fenster, bevor die zweite Ladung voll ist, wird eine Ladung gehalten. Im Fenster darf Reassemble auch
  auf Drill. Die zweite Ladung läuft wie heute.
- **O2 — nur für das eigene Zwei-Minuten-Fenster (`IsBurst`, Heiltrank):** ohne Blick auf die Gruppe. Einfacher,
  trifft aber nur, wenn die Gruppe zufällig mitzieht.
- **O3 — Planer über alle kommenden Fenster:** wählt je Ladung das wertvollste. Mehr Aufwand; bei zusammengelegten
  Buffs gleich O1, bei verstreuten Buffs besser, aber mit mehr Vorhersagen, die irren können.
- **Rückbau** jeder Variante: die Option aus.

## Abwägung

| | Im Kampf | Gewinn | Nachteil | Aufwand |
|---|---|---|---|---|
| O0 | Reassemble fällt, wo es fällt | — | verschenkt den Buff-Bonus | — |
| O1 | eine Ladung wartet auf das Fenster | bis zwei Ladungen im Fenster | gehaltene Ladung beim Tod des Gegners | mittel: Vorhersage je Buff, Haltebedingung |
| O2 | wartet auf das eigene Fenster | nur bei zufälliger Deckung | wie O1 | klein |
| O3 | wählt das beste Fenster | bei verstreuten Buffs mehr | mehr Fehlvorhersagen | groß |

**Die Nachteile des Zurückhaltens, einzeln:**
- *Ladungen gehen nicht verloren:* Gehalten wird nur eine von zwei; die zweite lädt weiter. Ist sie voll, geht eine
  hinaus. Die Zahl der Reassembles im Kampf bleibt dieselbe — eine Ladung je 55 s.
- *Tod des Gegners vor dem Fenster:* Die gehaltene Ladung verfällt. Abfangen: nicht halten, wenn die Zeit bis zum Tod
  (`GetCorrectedTTK`, selbstkorrigiert) vor dem Fenster endet.
- *Pause im Fenster:* Ist der Boss bei Fensterbeginn weg, nützt die Ladung nichts. Mit BossModReborn-Vorhersage
  (`BMRDowntimeWithin`) nicht halten; ohne Modul erst sichtbar, wenn die Pause beginnt.
- *Überhitzung im Fenster:* Während der Überhitzung fallen keine Werkzeuge. Das Fenster dauert 20 s; ohne
  Überhitzung bleiben vier bis fünf GCDs. Mit Drill (zwei Ladungen) ist fast immer ein Werkzeug dabei.
- *Mehrere Gegner:* Chain Saw und Excavator treffen alle mit dem garantierten Treffer, Drill einen. Im Fenster bleibt
  deshalb die bisherige Reihenfolge; Drill nur, wenn kein anderes Werkzeug kommt.
- *Fehlvorhersage* (Buff-Spieler stirbt, verschiebt): Die Ladung wartet bis zur nächsten vollen zweiten Ladung, also
  höchstens 55 s, und geht dann normal — kein Verlust außer dem entgangenen Bonus.

## Abgleich mit seiner Anforderung

Gefragt waren Zusammensetzung, Häufigkeit, Abklingzeit, wertvollster Buff und Gewinn gegen Nachteil. Alles oben;
nicht beantwortet ist nur der Direkttreffer-Wert von Battle Voice (keine Quelle). Nicht ausgeweitet: Andere Jobs
mit ähnlichen Garantie-Aktionen (Samurai, Mönch, Schwarzmagier) wären dieselbe Frage; erst nach seiner Entscheidung
hier.

## Antithesen, und was davon bleibt

Die Simulation dazu steht im Modellskript (`simulate`, `timeline_report`): zehn Minuten, GCD 2,5 s, Werkzeuge sobald
bereit, Reassemble mit zwei Ladungen und Countdown-Einsatz. Ausgeblendet sind Überhitzung und andere Verzögerungen
der Werkzeuge — das begünstigt die Nullvariante, weil die Werkzeuge im Spiel gegen das Raster wandern. Ergebnis,
Reassembles gesamt / im Fenster:

| Lage der Buff-Fenster | O0 heute | O5 nur Drill im Fenster | O1 halten |
|---|---|---|---|
| im Zwei-Minuten-Takt ab Pull | 11 / 4 | 11 / 4 | 10 / 5 |
| 30 s versetzt | 11 / 4 | 11 / 4 | 10 / 9 |
| 70 s versetzt | 11 / 2 | 11 / 3 | 11 / 8 |

**A1 — „RSR trifft das Fenster schon von selbst; die Werkzeuge mit 40 und 60 s richten sich alle 120 s aus."**
*Teilweise wahr.* Im Takt ab Pull liegen heute 4 von 11 Reassembles im Fenster, mit Halten 5. Versetzt — und das
ist die Lage, sobald Überhitzung die Werkzeuge verschiebt oder die Gruppe ihre Buffs nicht in den Opener legt —
liegen heute 2 bis 4 im Fenster, mit Halten 8 bis 9. Entkräftet für versetzte Fenster, nicht für den Gleichtakt.

**A2 — „Der Gewinn ist zu klein für eine weitere Regel."** *Nicht entkräftet, für sein Spielprofil.* Je verschobener
Ladung bringt ein Fenster mit sechs Buffs rund 180 Potenz-Gegenwert, mit einem oder zwei Buffs 25 bis 70 (Modell,
angenommene Spielerwerte). Je zehn Minuten:

| | Gleichtakt (+1 im Fenster) | versetzt (+5 bis +6) |
|---|---|---|
| volle Gruppe, sechs Buffs | ~180 = 0,2 GCD | ~900–1100 = gut 1 GCD |
| leichte Gruppe, ein bis zwei Buffs | 25–70 | 125–420 = höchstens eine halbe GCD |

Zehn Minuten haben rund 240 GCDs; das sind 0,1 bis 0,5 Prozent in vollen Gruppen und in leichten Gruppen höchstens
0,2 Prozent. *Entkräftet durch Selbstbeschränkung (sein Hinweis, 01.10.2026):* Die Buff-Jobs der Gruppe sind im Spiel
erkennbar (`PartyComposition` gegen `JobBuffs`). Die Regel ist ohne Buff-Jobs untätig und kostet dort nichts; mit
ihnen ist der Gewinn klein, aber nie negativ. „Zu klein" ist damit kein Grund gegen die Regel, nur eine Aussage über
ihre Größe.

**A3 — „Die Regel stützt sich auf das Verhalten anderer Spieler" (`CLAUDE.md`: nie ein tragender Grund).**
*Teilweise entkräftet.* Der Grund zu halten ist tatsächlich die erwartete Buff-Zeit anderer. Entschärfbar: erst halten,
wenn derselbe Buff zweimal im Abstand seiner Abklingzeit beobachtet ist — dann ist der Takt gemessen, nicht
angenommen. Und eine Fehlvorhersage kostet keine Ladung, nur den Bonus. Die Abhängigkeit bleibt aber das Wesen der
Regel: Ohne fremde Buffs kein Gewinn.

**A4 — „Halten kostet eine Ladung."** *Entkräftet bis auf das Kampfende.* Die Simulation zeigt genau diese eine: Im
Gleichtakt trägt O1 seine gehaltene Ladung über das Kampfende (10 statt 11). Schranke: nicht halten, wenn die
selbstkorrigierte Zeit bis zum Tod (`GetCorrectedTTK`) vor dem Fenster endet. Sonst bleibt die Zahl der Reassembles
gleich — eine Ladung je 55 s (Selbsttest des Skripts).

**A5 — „Im Fenster ist kein Einwebeplatz frei."** *Mit Schranke entkräftet.* Im eigenen Zwei-Minuten-Fenster brauchen
Wildfire, Barrel Stabilizer, Hypercharge und Double Check/Checkmate Einwebeplätze; unter Überhitzung (1,5-s-GCDs)
gibt es je GCD nur einen. Reassemble muss vor dem Werkzeug eingewoben werden und darf keinen dieser Plätze nehmen —
die vorhandene Prüfung `BurstWeaveSlotContested` beantwortet genau das.

**A6 — „Full Metal Field ist im Fenster das stärkere Ziel."** *Entkräftet:* Der Wirktext sagt „Delivers a critical
direct hit … This action is not affected by Reassemble."

**A7 — „Die Krit-Regel stammt aus Gemeinschaftsquellen."** *Für die Entscheidung entkräftet:* Ob gehalten wird, hängt
nur daran, dass ein Buff kommt; die Rangfolge der Buffs braucht nur O3. Für den Betrag bleibt sie Annahme.

**A8 — „Mehrere Gegner: Drill trifft einen."** *Entkräftet:* Im Fenster bleibt die Reihenfolge Excavator, Chain Saw,
Air Anchor vor Drill; Drill nur, wenn kein anderes Werkzeug ins Fenster fällt.

**Was bleibt:** A3 als Wesenszug — ohne fremde Buffs kein Gewinn; entschärft durch den gemessenen Takt und dadurch,
dass eine Fehlvorhersage keine Ladung kostet. A2 ist durch die Selbstbeschränkung auf Gruppen mit Buff-Jobs entkräftet.

## Alle Fälle und ihre Folgen

| Fall | Was O1 tut | Folge im Kampf |
|---|---|---|
| keine Buff-Jobs in der Gruppe, oder allein | nichts | wie heute |
| Buff-Jobs, Takt noch nicht zweimal gesehen (erstes Fenster, kurze Kämpfe) | nichts | wie heute; das Opener-Fenster trifft schon die Countdown-Ladung |
| Buff-Jobs, Takt gemessen, Fenster kommt vor der vollen zweiten Ladung | hält eine Ladung | sie fällt im Fenster auf das nächste Werkzeug, auch Drill |
| Fenster kommt erst nach der vollen zweiten Ladung | nichts | die volle zweite Ladung geht ohnehin hinaus, kein Halten nötig |
| **unter Stufe 84 (eine Ladung, Job-Guide: Enhanced Reassemble)** | **nichts** | Halten hieße, die einzige Ladung liegen zu lassen — jede Sekunde Halten wäre verlorene Wiederaufladung. O1 setzt zwei Ladungen voraus |
| unter Stufe 94 (Drill eine Ladung, Enhanced Multiweapon) | hält wie oben | im Fenster ist seltener ein Werkzeug frei; fällt keins hinein, verfällt nichts, die Ladung geht nach dem Fenster |
| Gegner stirbt vor dem Fenster (Zeit bis zum Tod, selbstkorrigiert) | nichts | wie heute; Restfehler: die Prognose irrt kurz vor dem Tod, dann trägt O1 höchstens eine Ladung über das Kampfende |
| Pause im Fenster, vorhergesagt | nichts | wie heute |
| Pause im Fenster, ohne Modul | hält | die Ladung wartet bis zur vollen zweiten, höchstens 55 s, kein Ladungsverlust |
| Buff-Spieler tot oder verschiebt | hält vergeblich | höchstens 55 s Warten, entgangen ist nur der Bonus |
| mehrere Gegner | hält wie oben | im Fenster bleibt die Reihenfolge der Werkzeuge; Drill nur ohne anderes |
| Überhitzung im Fenster | hält | Reassemble braucht ein Werkzeug danach; ist kein Einwebeplatz frei (`BurstWeaveSlotContested`), fällt es nach der Überhitzung |
| Burst-Einstellung aus | eigener Fall | O1 betrifft fremde Buffs, nicht den eigenen Burst; gehalten wird trotzdem. Ob „Burst aus" auch das abschalten soll, ist seine Wahl — ich empfehle: ja, weil „Burst aus" meist heißt, keinen Schaden zu bündeln |
| Sicherheit | keiner | Reassemble mindert nichts und heilt nicht; kein Sicherheitsfall berührt |

## Falsifikation

- **Kein Nutzen?** Widerlegt im Modell: Jeder Gruppenbuff erhöht den Wert eines Reassemble im Fenster; der Nachteil
  ist ohne Ladungsverlust. Nicht widerlegt ist die Höhe — sie hängt an den Werten des Spielers und an der Gruppe.
- **Option falsch?** O2 verfehlt die Gruppe; O3 ist bei seinem Spielprofil (leichte Gruppen, Zufallsgruppen) kaum
  besser als O1 und hat mehr Fehlerstellen. O1 hält.
- **Ausgeliefert, und nichts ändert sich?** In Gruppen ohne Buff-Jobs — beabsichtigt. Mit Buff-Jobs, wenn die
  Vorhersage nie trifft, weil Buffs nur im Opener fallen (kurze Kämpfe): dann hält die Regel nie lange, kein Schaden.
  Prüfbar am Kampfverlauf: ein FFLogs-Report zeigt, ob die Reassembles im Fenster lagen.
