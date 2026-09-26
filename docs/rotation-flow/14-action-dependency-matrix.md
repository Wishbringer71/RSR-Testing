# 14 · Abhängigkeitsmatrix der Aktionen je Job

Entwurfsdokument nach ADR-Struktur. Es stellt den geltenden Sachstand dar; die Prüfhistorie steht in
`AUDIT_LOG.md` (A160).

## Ergebnis

**Auftrag (seine Vorgabe):** „erstelle eine abhängigkeitsmatrix aller spells, abilitys und skills eines
jobs zueinander. mache das für jeden job einzeln. finde heraus, ob irgendwelche fähigkeiten nicht
genutzt werden. prüfe das konzept und die matrix danach kritisch auf fehler bei der erstellung und den
wechselwirkungen."

**Die Matrix wird erzeugt, nicht von Hand geführt:** `.github/scripts/audit/generate_action_matrix.py`
schreibt je Job `docs/action-matrix/<JOB>.md` (Aktionen, Stufe, Nutzung, Beziehungen) und
`<JOB>.csv` (die Matrix selbst: Zeile = Aktion, Spalte = worauf sie sich bezieht, Zelle = Art der
Beziehung). `docs/action-matrix/README.md` fasst die Nutzung je Job zusammen. Das Skript trägt einen
Selbsttest gegen konstruierte Texte und konstruierten Code; `--check` meldet veraltete Dateien und
läuft in der CI.

**Ungenutzt ist in den Standardrotationen keine Kampfaktion, die ein Spieler auslösen kann und die
eine Schadens-, Heil- oder Schutzfunktion ohne Sonderlage hat.** Was maschinell als „ungenutzt"
erscheint, zerfällt in fünf Klassen; nur die letzte ist eine Frage an ihn (Stand 26.09.2026):

| Klasse | Aktionen | Warum nicht genutzt |
|---|---|---|
| Behälter | Gemshine, Precious Brilliance, Astral Flow (SMN) · Play I–III, Minor Arcana (AST) · Serpent's Tail, Twinfang, Twinblood (VPR) · Riposte, Zwerchhau, Redoublement, Moulinet, Reprise (RDM) · Motive und Musen (PCT) · Iaijutsu, Tsubame-gaeshi (SAM) · Masterful Blitz (MNK) · Ninjutsu (NIN) · Continuation (GNB) | Der Knopf wird zu einer anderen Aktion; RSR ruft die Zielaktion, und das Spiel führt die angepasste Id aus (`BaseAction.Use`, `AdjustedID`) |
| Begleiter und Automatik | Akh Morn, Revelation, Exodus, Wyrmwave, Scarlet Flame, Luxwave, Everlasting Flight (SMN) · Embrace, Seraphic Veil (SCH) · Arm Punch, Roller Dash, Pile Bunker, Crowned Collider, Rook Overload (MCH) · Hollow Nozuchi (NIN) | Wirktext: „cannot be assigned to a hotbar"; der Begleiter oder ein Auslöser führt sie aus |
| Limit Breaks | je Job drei | ohne Wirktext im Datensatz; RSR castet keine PvE-Limit-Breaks (Konzept 05) |
| Hilfsaktionen | Sleep, Repose, Rescue, Leg Graze, Foot Graze, das Ablegen der Tankhaltung (Release …), Dissolve Union, Ending | Sie wirken auf Mitspieler oder die Gruppenlage (Rescue zieht einen Spieler, das Ablegen der Haltung gibt die Feindseligkeit ab, Schlaf bricht beim ersten Treffer). Nicht automatisiert — Schluss aus der Wirkung, kein Beleg für eine Absicht |
| **Werkzeuge für Pausen und Phasenenden** | Meditate (SAM) · Six-sided Star (MNK) · Queen Overdrive, Rook Overdrive, Flamethrower (MCH) | Kein Auslöser im Code. Im Archiv als „Features ohne Trigger, kein Fehler" geführt (#69); **mit `BMRDowntimeWithin` gibt es den Auslöser heute** — siehe „Offen" |

## Die Stufen (seine Vorgabe „universell zuerst")

Das Skript ordnet jede Aktion der höchsten Stufe zu, auf der jeder Job der Stufe sie hat. Die Stufen
sind seine: alle → Heiler · Tanks · Damage Dealer → Fernkämpfer · Magier · Nahkämpfer →
Monk und Samurai · Dragoon und Schnitter · Ninja und Viper → Job.

| Stufe | Gemeinsame Aktionen |
|---|---|
| alle | Sprint |
| Tanks | Rampart, Reflexion (Reprisal), Provoke, Shirk, Interject, Low Blow, Arm's Length |
| Heiler | Esuna, Lucid Dreaming, Swiftcast, Surecast, Repose, Rescue |
| Damage Dealer | — |
| Nahkämpfer | Second Wind, Bloodbath, Feint, Leg Sweep, True North, Arm's Length |
| Fernkämpfer | Second Wind, Head Graze, Leg Graze, Foot Graze, Peloton, Arm's Length |
| Magier | Addle, Lucid Dreaming, Swiftcast, Surecast, Sleep |
| Nahkampf-Paare | — |

**Befund zur Stufenordnung (Gegenposition, zur Kenntnis):** Die gemeinsamen Aktionen des Spiels
folgen dem Baum nicht überall.
- Arm's Length haben Tanks, Nahkämpfer und Fernkämpfer, Second Wind haben Nahkämpfer und
  Fernkämpfer. Auf der Stufe „Damage Dealer" steht deshalb nichts, weil die Magier sie nicht haben.
- Lucid Dreaming, Swiftcast und Surecast teilen Heiler und Magier, über die Grenze zwischen Heilern und
  Damage Dealern hinweg.
- Die Nahkampf-Paare teilen keine Aktion; sie sind im Spiel Ausrüstungsgruppen, keine
  Aktionsgruppen (Schluss).

Für Regeln heißt das: Eine Regel über Arm's Length oder Swiftcast sitzt nicht in einem Ast des
Baums, sondern quer dazu. Die Matrix zeigt es; eine Entscheidung verlangt es nicht.

## Was die Matrix enthält

**Zwei Ebenen, getrennt geführt**, weil eine Abweichung zwischen ihnen selbst ein Befund ist:

| Ebene | Quelle | Beziehungen |
|---|---|---|
| Spiel | Wirktexte in `ActionId.resx`, Eigenschaften in `Rotation.resx` | Combo nach · Knopf wird zu · gemeinsame Abklingzeit · braucht Status · darf Status nicht haben · Bedingung ohne Status · kostet Ressource · Ausbau durch Eigenschaft |
| RSR | Basisrotation (`Modify…`: `StatusNeed`, `StatusProvide`, `ComboIds`, `ActionCheck`) und Standardrotation | StatusNeed · ComboIds · ActionCheck liest · Regel prüft (Bedingung derselben oder einer umschließenden Abfrage) · Regel sperrt vorher (frühere Abfrage derselben Methode, die ohne Wirken zurückkehrt) |

**Nutzung je Aktion:**
- *direkt*: gewirkt — `CanUse` mit echtem Ausgabeziel (`out act`, `out var act`) oder als die
  auszuführende Aktion zurückgegeben (`return X;`, `act = X`) — in der Standardrotation, in der
  Basisrotation außerhalb der `Modify`-Rümpfe, im zentralen Dispatch, oder über eine
  Stellvertreter-Eigenschaft (`TankStance`, `Raise`).
- *über andere Aktion*: Eine gewirkte Aktion wird laut Wirktext oder Eigenschaft zu ihr, oder eine
  gleichnamige Variante mit anderer Id wird gewirkt (die Mudras des Ninja, `JinPvE_18807`).
- *nur geprüft*: nur als `CanUse(out _)` gefragt, eine Bedingung, kein Wirken.
- *nur gelesen*: kommt in anderen Bedingungen vor, wird aber nie gewirkt.
- *ungenutzt*: keines davon.

**Abgleich Wirktext ↔ Code:** Nennt der Wirktext eine Statusbedingung und führt die Basisrotation
dafür weder `StatusNeed` noch `ActionCheck`, steht die Aktion in der Liste. Unter den bewerteten Jobs
bleiben Manafont (BLM) und die Eukrasia-Aktionen (SGE); der Blaumagier führt weitere, unbewertet.
Beides ist kein Befund, aus demselben Grund: Die Bedingung steht in einer Eigenschaft, die der
Abgleich nicht verfolgt.
- Manafont wird in `BLM_Default` und `BLM_RP` nur innerhalb von `if (InAstralFire)` gewirkt.
- Die Eukrasia-Aktionen wirkt `SGE_Reborn` nur unter Eukrasia: hinter `if (!HasEukrasia) { … return … }`
  oder mit `HasEukrasia &&` in der Bedingung. Sie sind der Knopf, zu dem Dosis, Diagnosis und
  Prognosis unter Eukrasia werden.

## Grenzen, bewusst hingenommen

- **Keine Stufenangaben im Datensatz.** Ob eine Aktion unter Stufensynchronisation fehlt, ist hier
  nicht erkennbar. Die Ausbauketten zeigen nur, dass der Griff auf der Grundaktion liegt (Konzept 05).
- **Bedingungen über Eigenschaften oder Hilfsmethoden werden nicht verfolgt.** `InTwoMIsBurst`,
  `HoldAreaDefense(…)` und ähnliche erscheinen nicht als Kante zu den Aktionen, die sie lesen.
- **„direkt" heißt: Es gibt einen Aufruf.** Ob er erreichbar ist (Optionen, Stufe, Lage), sagt die
  Matrix nicht.
- **Begleiter und Behälter** werden am Wirktext erkannt („cannot be assigned to a hotbar", „changes
  to"), nicht an Spieldaten.
- **Ressourcenerzeuger** stehen in den Wirktexten selten; die Kante „kostet" zeigt den Verbraucher,
  selten den Erzeuger. Ressourcennamen aus zwei Wörtern kennt das Skript als Liste (Soul Voice, Lemure
  Shroud, …), weil die Texte das vorige Feld in den Namen laufen lassen.
- **Vom Spiel ausgeblendete Namen:** Hängt ein Combo-Vorgänger von einer Eigenschaft ab, lässt der
  Wirktext ihn leer (wie eine Potenz); dann fehlt die Combo-Kante.
- **Nicht erfasst:** Zusatzrotationen (`ExtraRotations`, darunter Churin), PvP. Blaumagier und
  Beastmaster werden erzeugt, aber nicht bewertet (begrenzte Jobs, außerhalb seines Profils).
- **Die Zahlen altern** mit jeder Rotationsänderung; die erzeugten Dateien tragen ihr Datum,
  `--check` zeigt, ob sie noch stimmen.

## Offen

**Werkzeuge für Pausen und Phasenenden (Monk, Samurai, Machinist):** Im Kampf fehlt heute
- Six-sided Star vor einer Pause oder am Kampfende (Monk),
- Meditate in der Pause (Samurai: Kenki und Meditation ohne Ziel),
- Queen/Rook Overdrive vor einer Pause, damit die Königin ihren Abschluss nicht verliert
  (Machinist).

Flamethrower ist ein Flächenkanal; in der zentralen Steuerung ist er schon vorgesehen
(Positionssperre beim Kanal, `Configs` und `MovingUpdater`), keine Rotation ruft ihn. Ob einer
davon sich lohnt, hängt an der Pausenvorhersage (`BMRDowntimeWithin`, nur mit Modul) und am Job.
Diese Jobs liegen außerhalb dessen, was er bisher genannt hat; nach seiner Regel erst bearbeiten,
wenn er sie nennt (TODO).
