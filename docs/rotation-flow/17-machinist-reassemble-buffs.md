# 17 · Maschinist: Reassemble in die Gruppenbuffs legen?

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie steht in
`AUDIT_LOG.md` (A235). Vorgeschichte: Konzept 16, Maschinist.

## Ergebnis

**Auftrag (01.10.2026):** prüfen, „wer alles in der gruppe ist und gruppenbuffs liefern kann, wie häufig diese
gecasted werden könnten und wie die abklingzeit wäre, und welcher der gerade möglichen gruppenbuffs den größten
effekt hat … wie optimaler schaden erzielt werden kann vs. den nachteilen eines zurückhaltens."

**Empfehlung, zur Entscheidung:** Option O1 bauen — eine Reassemble-Ladung für das nächste vorhergesagte
Buff-Fenster zurückhalten, nur wenn die Gruppe Buff-Jobs hat; im Fenster auch Drill zulassen. Der Gewinn ist
klein, aber ohne Verlust an Ladungen; das einzige Risiko ist eine gehaltene Ladung beim Tod des Gegners, und das
misst die Regel selbst (Zeit bis zum Tod). Ohne Buff-Jobs in der Gruppe ändert sich nichts. Sicherheit ist nicht
berührt: Reassemble ist eine reine Schadensaktion.

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

## Falsifikation

- **Kein Nutzen?** Widerlegt im Modell: Jeder Gruppenbuff erhöht den Wert eines Reassemble im Fenster; der Nachteil
  ist ohne Ladungsverlust. Nicht widerlegt ist die Höhe — sie hängt an den Werten des Spielers und an der Gruppe.
- **Option falsch?** O2 verfehlt die Gruppe; O3 ist bei seinem Spielprofil (leichte Gruppen, Zufallsgruppen) kaum
  besser als O1 und hat mehr Fehlerstellen. O1 hält.
- **Ausgeliefert, und nichts ändert sich?** In Gruppen ohne Buff-Jobs — beabsichtigt. Mit Buff-Jobs, wenn die
  Vorhersage nie trifft, weil Buffs nur im Opener fallen (kurze Kämpfe): dann hält die Regel nie lange, kein Schaden.
  Prüfbar am Kampfverlauf: ein FFLogs-Report zeigt, ob die Reassembles im Fenster lagen.
