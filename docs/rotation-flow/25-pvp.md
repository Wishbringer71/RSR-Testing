# 25 · PvP

**Leitfrage:** Was tut RSR in PvP, wo PvP eigene Aktionen, eigene Zustände und andere Gegner hat, und wo holt es dort
dasselbe Optimum heraus wie in PvE?

Gilt für jeden Job, der in PvP spielbar ist (21 Jobs; Blaumagier und Beastmaster haben kein PvP). Ordnung der Konzepte:
`20-concept-order.md`. Die Regeln für alle Jobs stehen in `03-universal.md`; dieses Konzept nennt, wo PvP von ihnen
abweicht und warum.

*Seine Vorgaben (13.11.2026):* In PvP dasselbe Optimum wie in PvE, zum eigenen Vorteil, obwohl er PvP äußerst selten
spielt. Eine Einschränkung, ob von mir, von Upstream oder aus einem anderen Werkzeug, bleibt nur bei technischer
Notwendigkeit. *Sein Hinweis (13.11.2026):* In PvP nutzt man andere Fähigkeiten; sie sind nicht mit den normalen zu
verwechseln.

## PvP: Grundlagen

*Was gilt:* Jeder Job hat in PvP einen eigenen Satz Aktionen (`Action.IsPvP`), getrennt von seinen PvE-Aktionen. RSR
lädt in einem PvP-Gebiet (`TerritoryType`, `IsPvpZone`) die PvP-Rotation des Jobs (`CombatType.PvP`) und lässt nur
PvP-Aktionen zu (`ActionBasicInfo`: Aktion und Gebiet müssen beide PvP oder beide PvE sein). Eine PvE-Regel, die eine
PvE-Aktion drückt, greift in PvP deshalb ins Leere. Das ist eine technische Grenze, keine Einschränkung; ob es dafür ein
PvP-Gegenstück gibt, steht beim jeweiligen Abschnitt.

*Gemeinsame Aktionen aller Jobs* (Job-Guide, `na.finalfantasyxiv.com/jobguide/`, abgerufen 13.11.2026):
- Guard: 99 % weniger Schaden und Immunität gegen Stun, Heavy, Bind, Silence, Deep Freeze, Rückstoß und Heranziehen,
  4 s, 30 s Abklingzeit. Endet mit jeder anderen Aktion.
- Purify: entfernt Stun, Heavy, Bind, Silence, Deep Freeze und Miracle of Nature, gewährt 2 s Resilience, 2000 MP.
- Recuperate: heilt dich mit 16.000 Potenz, 2000 MP.
- Standard-issue Elixir: LP und MP voll, 4,5 s Wirkzeit, bricht bei Schaden ab.
- Sprint: 50 % schneller, endet mit jeder anderen Aktion.

*Rollenaktionen* (eine je Rolle wählbar): Tanks Rampage, Rampart, Full Swing; Heiler Haelan, Stoneskin II, Diabrosis;
Nahkampf Bloodbath, Swift, Smite; physischer Fernkampf Dervish, Bravery, Eagle Eye Shot; Magie Comet, Phantom Dart,
Rust.

## PvP: Einschränkungen

*Was gilt (A755):* Jede Bedingung im Code, die in PvP etwas anders macht als in PvE, ist einer von drei Fällen. Die
Bestandsaufnahme umfasst Fork, Upstream und alles, was aus anderen Werkzeugen übernommen wurde.

1. *Technisch nötig, bleibt:*
   - Auswahl von Rotation und Aktionen nach PvP oder PvE (`RotationUpdater`, `ActionBasicInfo`, Befehle und
     Oberfläche).
   - Regeln, die PvE-Aktionen, PvE-Zustände oder PvE-Gegner lesen und in PvP nichts finden: Wiederbelebung
     (`CanRaise`; in PvP gibt es keine), Heilung gegen Schaden über Zeit (`HotAgainstDot`, PvE-Heilungen), Schlaf gegen
     Flächen (`SleepToStopAreaCasts`), Limit Break allein (PvE-Limit Break, PvP kennt eine Gruppe immer),
     Figuren zum Heilen (`HealToDefeat`), Tötungsregeln (`KillOrder`), Sperrzustände (`PlayerHasLockActions`) und
     Upstreams Sperre für liegende Strafzustände (`NoCastingStatus`; alle 31 Zustände stammen aus PvE-Bosskämpfen).
   - Guard: Jede Aktion beendet ihn (Wirktext). „PvP Guard control" hält deshalb alle Aktionen, solange er steht.
   - Das Elixier wirkt 4,5 s, länger als jeder GCD. Die Grenze für Fähigkeiten mit Wirkzeit (`01-jobs.md`, W1) hielte
     es für immer zurück. Sie gilt deshalb nur für Wirkzeiten unter dem GCD, in PvE und PvP gleich, statt PvP
     auszunehmen.
2. *Ohne technischen Grund, aufgehoben:*
   - Verkürzung der Animationssperre und Ausgleich der Bildrate (`03-universal.md`, „Weben, Animationssperre und
     Latenz"): meine Ableitung aus Fairness (A753).
   - Wache gegen Strafzustände bei der Landung (`ActionPenaltyWatch`, `03-universal.md`): von Upstreams Sperre
     übernommen, ohne Wirkung in PvP.
   - Gegner mit Stacheln (`03-universal.md`, „Gegner mit Stacheln"): Nebula des Revolverklingen-Kämpfers (3051) trifft
     in PvP jeden Angreifer mit 4.000 Potenz je Angriff. RSR meidet ihn wie jeden Gegner mit Stacheln.
   - Stopp-Markierung: Der Text der Einstellung „Never attack targets with stop markers" bindet, auch in PvP (Upstream
     nahm PvP aus, ohne Grund im Commit).
   - Notfallheilung vor dem GCD (`07-heal-target-priority.md`, „Die Notfallheilung zwischen zwei Zaubern"): Die
     PvP-Rotationen von Astrologe, Gelehrtem und Tänzer haben Heilfähigkeiten, die dort genauso früher gehen.
   - Abfangen eigener Tastendrücke („Intercept your action presses (PvE only)", Upstream 11.08.2025, ohne Grund im
     Commit): wird mit den Jobs geprüft (unten), weil in PvP Guard, Purify und Sprint sofort gelten müssen.
3. *Nicht aufgehoben, meine Grenze:* Upstream sperrt in PvP die Steuerung von RSR durch andere Plugins über IPC, schaltet
   RSR in PvP ab, solange „Auto PvP Series Grind" geladen ist, und wählt in PvP kein Ziel von selbst, wenn keines gewählt
   ist (Upstream 26.07.2025: „Enhanced target logic for botters"). Diese drei Sperren verhindern, dass RSR ohne
   anwesenden Spieler PvP-Partien spielt. Das ginge zulasten der anderen Spieler einer Partie, die gegen oder mit einem
   unbesetzten Platz spielen. Sie sind keine technische Notwendigkeit; ich hebe sie trotzdem nicht auf. Das Spiel eines
   anwesenden Spielers berührt keine von ihnen: Er wählt sein Ziel selbst, und RSR wählt für jede Aktion ihr Ziel wie
   in PvE.

*Im Kampf:* In PvP kommt nach jeder Sofortaktion die nächste Aktion um den gekürzten Netzrest früher. Gegen einen
Revolverklingen-Kämpfer unter Nebula greift RSR für deren 8 s ein anderes Ziel an, statt je Treffer 4.000 Potenz
zurückzunehmen. Ein Heiler mit Heilfähigkeit drückt sie vor dem GCD, wenn ein Mitglied zu fallen droht.
