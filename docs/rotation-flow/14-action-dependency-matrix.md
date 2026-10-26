# 14 · Abhängigkeitsmatrix der Aktionen je Job

**Leitfrage:** Wie hängen die Aktionen eines Jobs voneinander ab, und welche wirkt der Baum nie?

Die Prüfhistorie steht in `AUDIT_LOG.md` (A160, A162–A167).

## Antwort

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

**Jede Kampfaktion, die ein Spieler auslösen kann und die ohne Sonderlage Schaden, Heilung oder
Schutz bringt, hat in den Standardrotationen einen Aufruf — außer Six-sided Star.**
Ob ein Aufruf erreicht wird — hinter welcher Einstellung, auf welcher Stufe, in welcher Lage —, sagt
die Matrix nicht (Grenzen). Was maschinell nicht als gewirkt erscheint („ungenutzt" oder „nur
gelesen"), zerfällt in sechs Klassen (Stand der erzeugten Dateien: 01.10.2026):

| Klasse | Aktionen | Warum nicht gewirkt |
|---|---|---|
| Behälter | Gemshine, Precious Brilliance, Astral Flow (SMN) · Play I–III, Minor Arcana (AST) · Serpent's Tail, Twinfang, Twinblood (VPR) · Riposte, Zwerchhau, Redoublement, Moulinet, Reprise (RDM) · Motive und Musen (PCT) · Iaijutsu, Tsubame-gaeshi (SAM) · Masterful Blitz (MNK) · Ninjutsu (NIN) · Continuation (GNB) | Der Knopf wird zu einer anderen Aktion; RSR ruft die Zielaktion, und das Spiel führt die angepasste Id aus (`BaseAction.Use`, `AdjustedID`). Wo eine Regel den Behälter in einer Bedingung liest, steht er als „nur gelesen" |
| Begleiter und Automatik | Akh Morn, Revelation, Exodus, Wyrmwave, Scarlet Flame, Luxwave, Everlasting Flight (SMN) · Embrace, Seraphic Veil (SCH) · Arm Punch, Roller Dash, Pile Bunker, Crowned Collider, Rook Overload (MCH) · Hollow Nozuchi (NIN) | Wirktext: „cannot be assigned to a hotbar"; der Begleiter oder ein Auslöser führt sie aus |
| Limit Breaks | je Job drei | ohne Wirktext im Datensatz; RSR castet keine PvE-Limit-Breaks (Konzept 05) |
| Hilfsaktionen | Sleep, Repose, Rescue, Leg Graze, Foot Graze, das Ablegen der Tankhaltung (Release …) | Sie wirken auf Mitspieler oder die Gruppenlage (Rescue zieht einen Spieler, das Ablegen der Haltung gibt die Feindseligkeit ab, Schlaf bricht beim ersten Treffer). Nicht automatisiert — Schluss aus der Wirkung, kein Beleg für eine Absicht. Dissolve Union gehört nicht dazu: Der Gelehrte löst damit seine Verbindung über der Schwelle seiner Einstellung und vor Living Dead (A324) |
| **Knopfwechsel gesperrt** | Detonator (MCH) | `WildfirePvE` führt Wildfire als `StatusProvide` und schließt den gewechselten Knopf aus (`ModifyWildfirePvE`: `!DetonatorPvEReady`); Wildfire zündet mit allen Stapeln von selbst. Siehe „Wo eine Aktion einen Status aufhebt" |
| **Mit Quelle, nicht gebaut** | Six-sided Star (MNK) | Job-Guide und The Balance nennen Werte und Einsatzregel — siehe „Pausen und Phasenenden" |

Improvised Finish (DNC) gehörte zur gesperrten Klasse: `ImprovisationPvE` führt Improvisation als
`StatusProvide` und verweigert sich, solange der Knopf gewechselt hat. `DNC_Reborn` ruft es deshalb
eigens (siehe „Wechselwirkungen und Zeit").

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

**Die gemeinsamen Aktionen des Spiels folgen dem Baum nicht überall** (Gegenposition zur
Stufenordnung, zur Kenntnis):
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
| Zeit und Wechselwirkung | Wirktexte; die Aktionssperren am Anfang der GCD- und Fähigkeitswahl und die Bewegungssperren (`Configs`) | Art (Abwehr, Heilung, Angriff, sonstige) · Kanal und die RSR-Sperre, die ihn hält · hebt Status auf · nicht nutzbar unter Status · Verlängerung mit Obergrenze · Stapel · Umschalten · Ressource erzeugt und verbraucht · Kreisläufe · leere Werte im Wirktext |

**Nutzung je Aktion:**
- *direkt*: gewirkt — `CanUse` mit echtem Ausgabeziel (`out act`, `out var act`) oder als die
  auszuführende Aktion zurückgegeben (`return X;`, `act = X`) — in der Standardrotation, in der
  Basisrotation außerhalb der `Modify`-Rümpfe, im zentralen Dispatch, oder über eine
  Stellvertreter-Eigenschaft (`TankStance`, `Raise`).
- *über andere Aktion*: Eine gewirkte Aktion wird laut Wirktext oder Eigenschaft zu ihr, oder eine
  gleichnamige Variante mit anderer Id wird gewirkt (die Mudras des Ninja, `JinPvE_18807`). Nicht, wenn
  die Basisaktion per `StatusProvide` genau den Status ausschließt, den die Zielaktion braucht: Dann
  verweigert sie sich, solange der Knopf gewechselt ist, und die Zielaktion ist *ungenutzt* mit diesem
  Grund. Die Sperre gilt nicht in den letzten `StatusRefreshGcdCount` GCDs des Status und nicht bei
  ausgeschaltetem `ShouldCheckStatus`; dort geht der Wurf der Basisaktion als Zielaktion hinaus.
- *nur geprüft*: nur als `CanUse(out _)` gefragt, eine Bedingung, kein Wirken.
- *nur gelesen*: kommt in anderen Bedingungen vor, wird aber nie gewirkt.
- *ungenutzt*: keines davon.

**Abgleich Wirktext ↔ Code:** Nennt der Wirktext eine Statusbedingung und führt die Basisrotation
dafür weder `StatusNeed` noch `ActionCheck`, steht die Aktion in der Liste. Unter den bewerteten Jobs
stehen dort Manafont (BLM), Improvised Finish (DNC) und die Eukrasia-Aktionen (SGE); der Blaumagier
führt weitere, unbewertet. Keiner ist ein Befund, aus demselben Grund: Die Bedingung steht in einer
Eigenschaft oder einem umschließenden `if`, das der Abgleich nicht verfolgt.
- Manafont wird in `BLM_Default` und `BLM_RP` nur innerhalb von `if (InAstralFire)` gewirkt.
- Improvised Finish wirkt `DNC_Reborn` nur unter `ImprovisedFinishPvEReady` — der Knopf von
  Improvisation ist dann Improvised Finish, Improvisation liegt also.
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
- **Nicht erfasst:** Zusatzrotationen (`ExtraRotations`), PvP. Blaumagier und Beastmaster werden
  erzeugt, aber nicht bewertet (begrenzte Jobs, außerhalb seines Profils).
- **Die Zahlen altern** mit jeder Rotationsänderung; die erzeugten Dateien tragen ihr Datum,
  `--check` zeigt, ob sie noch stimmen.

## Wechselwirkungen und Zeit

**Auftrag (seine Vorgabe):** „prüfe alle erfassten fähigkeiten der einzelen jobs auf vollständigkeit und
vollständiger beschreibung der fähigkeiten. prüfe wechselwirkungen mit anderen fähigkeiten.
nebenprüfung: schränkt die nutzung von defensive skills die offensiven skills ein oder hebt diese auf?
oder werden sie defensive skills durch die nutzung von offensive skills aufgehoben. […] evtl. ist bei
einigen ein quasi-perpetum-mobile möglich zu erschaffen oder positive effekte extrem auszureizen."
Dazu seine Präzisierung: Eine Sicherheitsbewertung braucht eine Wahrscheinlichkeit (CLAUDE.md).

**Sachstand:** Der Generator liest aus den Wirktexten, was eine Aktion zeitlich und im Zusammenspiel
tut, und schreibt es je Job in den Abschnitt „Wechselwirkungen und Zeit" der Matrix. Die Befunde unten
sind daraus und aus dem Code erhoben; wo eine Spielmechanik nicht im Text steht, ist sie als Schluss
markiert.

### Wo Angriff die Abwehr beendet: die Kanäle

Drei Schutzaktionen sind Kanäle — „Effect ends upon using another action or moving". Jede weitere
Aktion beendet sie, auch ein Angriff. RSR wirkt nach jeder Aktion sofort die nächste; ohne Sperre
beendet es seinen eigenen Kanal also mit dem nächsten GCD oder der nächsten Fähigkeit.

| Aktion | Was der Kanal trägt (Wirktext) | Was nach dem Abbruch bleibt | RSR-Sperre (Voreinstellung) |
|---|---|---|---|
| Passage of Arms (Paladin) | Blockrate 100 %, Gruppe im Kegel hinter ihm nimmt 85 % Schaden, 18 s | nichts | `PldlockCasting` (an): hält GCD und Fähigkeiten, solange der angekündigte Treffer aussteht (`DataCenter.AreaHitPending`). `PosPassageOfArms` (aus) sperrt Bewegung, nur mit `PoslockCasting` (aus) |
| Collective Unconscious (Astrologe) | Ring 18 s, darin Wheel of Fortune (Regen) fortlaufend | Minderung −10 % für 10 s, als Zusatzeffekt beim Wirken vergeben (Schluss aus dem Textaufbau, Status 849 ist eigener Status) | `AstlockCasting` (aus), gebaut wie beim Paladin |
| Improvisation (Tänzer) | Stapel Rising Rhythm alle 3 s bis 4; Regen 15 s | Regen (Schluss aus dem Textaufbau) | keine Aktionssperre; Bewegungssperre `PosImprovisation` (aus), nur mit `PoslockCasting` (aus) |

**Im Kampf:**
- **Paladin:** RSR wirkt Passage of Arms nur, wenn ein Flächentreffer angekündigt ist (Zauberleiste
  oder BossMod-Raidwide im Fenster). Die Sperre hält GCD und Fähigkeiten, solange der Treffer aussteht
  (`DataCenter.AreaHitPending`: das Flächensignal oder ein BossMod-Raidwide im Fenster, auch in den
  letzten 0,6 s, in denen das Signal schon losgelassen hat); danach beendet die nächste Aktion den
  Kanal. So verlangt es der Einstellungstext („during AOE mitigations"). Der Treffer ist angekündigt,
  die Wahrscheinlichkeit also hoch; die Sperre kostet GCDs bis zum Treffer — das trägt seine
  Präzisierung, und ab Werk an steht sie nach seiner Regel für Voreinstellungen (A250). Ohne Sperre
  schützt die Aktion niemanden, wenn der Treffer nach RSRs nächster Aktion landet — spätestens nach
  einem GCD —, und ihre Abklingzeit ist verbraucht. Ohne die Frage nach dem Treffer hielte der GCD-Pfad,
  bis eine Fähigkeit den Kanal beendet — bis zu 18 s ohne GCD, der Fall seines Extrembeispiels; so war
  es bis A164.
- **Astrologe:** Die Minderung bleibt nach dem Abbruch; der Kanal verlängert nur das Regen. Die Sperre
  hielte dafür bis zum Treffer auch die GCD-Heilungen des Astrologen zurück. Ohne Sperre verliert er
  wenig; deshalb steht sie ab Werk aus.
- **Tänzer:** `DNC_Reborn` schließt den Tanz sofort mit Improvised Finish ab: 5 % Barriere für 30 s für
  die Gruppe in 8 y (0 Stapel, Job-Guide); verloren geht höchstens der Rest eines GCDs Regen, den die
  nächste Aktion ohnehin beendet hätte.
  *Stapel vor einem angekündigten Treffer aufbauen entfällt (A320).* Die Barriere wächst je Stapel um einen Prozentpunkt
  der maximalen LP, der vierte um zwei (6, 7, 8, 10 %); ein Stapel kostet drei Sekunden ohne jede Aktion und ohne
  Bewegung („Effect ends upon using another action or moving", `ActionId.resx`). Ob der Zuwachs einen Tod verhindert,
  ist nur rechenbar, wo die Größe des Treffers bekannt ist – beim laufenden, gemessenen Wirken
  (`AnnouncedAreaShare`). Dort bleiben bis zum Snapshot drei bis fünf Sekunden, also höchstens ein Stapel: ein
  Prozentpunkt mehr, gegen drei Sekunden ohne Aktion, und nur, wenn Improvisation gerade läuft – RSR beginnt sie für
  die Heilung nach einem Treffer, nicht vor ihm. Eine Ansage von BossModReborn nennt keine Größe, dort wäre jeder
  Stapel eine Wette. Die Wahrscheinlichkeit, dass die Regel einen Tod verhindert, ist nicht messbar und nach der
  Rechnung klein; die Kosten fallen sicher an.

**Umgekehrt, Abwehr beendet Angriff:** Flamethrower (Machinist) ist ebenfalls ein Kanal, einer für Schaden (Einsatzregel:
„Vergleich mit The Balance", Maschinist). Solange er läuft – sein Status liegt, der Maschinist steht und ein Gegner steht im
Kegel vor ihm (`DataCenter.FlamethrowerChannelRunning`) –, verweigert `BaseAction.CanUse` jede andere Aktion
(„KeepsTheFlamethrowerChannel"), einen Trank ebenso (`MedicineItem`). Durch geht, was ein Sicherheitsweg verlangt –
Heilung, Flächen- und Einzelabwehr, Unterbrechung, Reinigung, Wiederbelebung, Provokation, Schutz gegen Rückstoß – und was
der Spieler drückt. Sicherheit vor Schaden: Der Kanal ist Schaden, und ein Treffer wiegt mehr als die restlichen Ticks.
Bewegt sich der Spieler, endet der Kanal ohnehin; RSR sperrt seine Bewegung nur mit „Lock movement while casting" und
dem Haken bei Flamethrower (beide ab Werk aus). Steht kein Gegner mehr im Kegel, gibt die Sperre frei, und die nächste
Aktion beendet den leeren Kanal. `DefenseTrace.log` schreibt bei jedem Ende, wie lange er lief, die letzte Aktion und
ob der Spieler sich bewegte. Apokalypsis (Blaumagier) ist derselbe Kanal als Linie; außerhalb seines Profils erfasst,
keine Sperre. Meditate (Samurai) wirkt RSR nur in der Pause. Dort beendet ihn jede Abwehr oder Heilung, die
ohne Gegner in Reichweite fällt: die eigene Heilung (Second Wind, Bloodbath), Tengentsu und Third Eye
bei angekündigtem Treffer auf ihn oder eigener niedriger Gesundheit. Sicherheit vor Kenki — so gewollt.
Die Rückhaltung von Abwehr im Burst steht in Konzept 08, „Die Abwehrsperren".

### Wo eine Aktion einen Status aufhebt

| Aktion | hebt auf | Wie RSR damit umgeht |
|---|---|---|
| Shake It Off (Krieger) | Thrill of Battle, Damnation, Bloodwhetting | Vollständig laut offiziellem Job-Guide (27.09.2026): „Dispels Thrill of Battle, Damnation, and Bloodwhetting, increasing damage absorbed by 2% for each effect removed"; der Wirktext im Repository blendet die stufenabhängigen Namen aus („Dispels Thrill of Battle and increasing…"). Auf niedrigerer Stufe Vengeance statt Damnation — so prüft es WrathCombo; Raw Intuition vor Bloodwhetting ist ein Schluss aus den Ausbauketten. RSR wägt die Aufhebung je Weg ab: Die Einzelheilung wartet, solange Damnation, Vengeance oder Bloodwhetting liegt; die Flächenabwehr wartet nur, solange Damnation (Vengeance) für einen Tankbuster auf ihn liegt, der vor ihrem Ende landet; beide warten, solange der Tankbuster-Plan einen dieser Status braucht. Thrill of Battle hält keinen Weg auf — die Barriere bemisst sich an der erhöhten Maximalgesundheit (The Balance). Regeln und Gründe: Konzept 09, „Krieger: die übrige Abwehr", Regel 6 |
| Tempera Grassa (Pictomancer) | Tempera Coat | Zweck der Aktion; RSR wandelt nur bei angekündigtem Flächentreffer oder in den letzten zwei GCDs von Tempera Coat |
| Meisui (Ninja) | Shadow Walker | Zweck der Aktion. RSR wirkt es nur, während Trick Attack (Kunai's Bane) abkühlt, und dann, wenn (a) Ten Chi Jin bereit ist — dessen Abfolge endet mit Suiton, das Shadow Walker neu gibt (Wirktext) —, (b) Shadow Walker in zwei GCDs endet oder (c) Trick Attack nicht in 19 s bereit ist und Ten Chi Jin abkühlt |
| Detonator (Machinist) | Wildfire | RSR wirkt ihn nie; Wildfire zündet mit allen Stapeln von selbst. `ModifyWildfirePvE` schließt den gewechselten Knopf aus, weil `WildfirePvE` sonst in den letzten zwei GCDs von Wildfire (Sperrlücke von `StatusProvide`, siehe oben) als Detonator hinausginge und die übrigen Stapel abschnitte. Nicht gebaut: Detonator vor dem Tod des Ziels — ob die Ladung dann verfällt, nennt keine Quelle (geprüft 02.10.2026: Job-Guide, consolegameswiki „Wildfire“, The Balance); WrathCombo wirkt Detonator ebenfalls nicht |

**Vergleich mit WrathCombo** (Community-Plugin, Stand 10.10.2026; keine Spielquelle — es zeigt, wie ein
anderes Werkzeug entscheidet):
- *Shake It Off:* Im Selbstheilungspfad nur, wenn weder Thrill of Battle noch Damnation, Vengeance oder
  Bloodwhetting liegt (`safeToShakeItOff`); im Pfad für angekündigten Gruppenschaden ohne diese Prüfung.
  Bestätigt die Aufhebung auch von Vengeance auf niedriger Stufe. The Balance (Basic Guide, 01.10.2026)
  wertet die Aufhebung von Thrill of Battle dagegen als Gewinn; RSR folgt darin The Balance.
- *Passage of Arms:* Die Option „Block Combos for Passage of Arms" (`PLD_BlockForWings`) hält alle
  Kombos, solange der Kanal liegt — ohne Frage nach dem Treffer; beenden muss der Spieler.
- *Improvisation:* als Füller außerhalb von Technical Finish, wenn mehr als zwei Verbündete in 8 Yalm
  stehen; Improvised Finish nutzt WrathCombo nirgends. Es behandelt Improvisation also als kurzes Regen.
- *Addersgall (Weiser):* „Addersgall Protect" wirkt Druochole auf das Mitglied mit der niedrigsten
  Gesundheit (sonst auf sich), sobald die Stapel eine Schwelle erreichen (ab Werk 3).

**Nicht nutzbar unter einem Status** (Hammer Motif unter Hammer Time, Fire in Red unter Subtractive
Palette, Ten Chi Jin unter Kassatsu, Plentiful Harvest unter Bloodsown Circle u. a.) sind
Angriffsabfolgen innerhalb eines Jobs; keine berührt Abwehr.

### Aufbau, Verlängerung, Erstattung: kein Perpetuum mobile

- **Jede Verlängerung hat eine Obergrenze von 60 s:** Darkside (Dunkelritter), Surging Tempest
  (Krieger), Death's Design (Schnitter). RSR frischt alle drei vor Ablauf auf.
- **Kein Kreislauf aus den Texten trägt sich selbst.** Die einzigen Status-Kreisläufe (Schnitter):
  Gallows braucht Soul Reaver und gibt Enhanced Gibbet; Unveiled Gibbet braucht Enhanced Gibbet und gibt
  Soul Reaver — für 50 Soul Gauge (Wirktext). Ebenso Gibbet und Unveiled Gallows. Jede Runde kostet also
  Soul Gauge. Ressourcenkreisläufe (A kostet, was B erzeugt, und umgekehrt) findet der Generator keine.
- **Dieser Nullbefund ist schwach:** Die meisten Gauge-Gewinne stehen nicht in den Texten — die
  Combo-Aktionen blenden sie mit der Potenz aus (Slice, Hakaze, Gibbet), MP-Kosten fehlen ganz. Ein
  Kreislauf über eine solche Aktion ist hier nicht erkennbar. Der Kreislauf-Finder ist gegen
  konstruierte Kreisläufe selbstgetestet; er findet, was die Texte hergeben, nicht mehr.
- **Erstattungen, die an Wahrscheinlichkeit hängen:** Tempera Coat (−60 s eigene Abklingzeit) und
  Tempera Grassa (−30 s) erstatten, wenn die Barriere ganz aufgezehrt wird; The Blackest Night gibt
  dann Dark Arts; Haima und Panhaima legen eine neue Barriere. Der Ertrag fällt nur, wenn Schaden
  kommt. RSR wirkt sie auf ein Signal hin — angekündigter Treffer, Tankbuster, Heilbedarf; The
  Blackest Night auch in den letzten 3 s des Countdowns, vor dem Pull. Das ist die Wahrscheinlichkeit,
  die seine Präzisierung verlangt.
- **Erstattungen im Angriffsablauf** (Heat Blast → Gauss Round und Ricochet −15 s; Chaotic Cyclone
  und Inner Chaos → Infuriate −5 s; Enhanced Harpe → Hell's Ingress und Egress −5 s) verbraucht die
  Rotation ohnehin.

### Unvollständige Beschreibungen

Viele Wirktexte lassen einen Wert leer, den eine Eigenschaft oder die Stufe setzt: Potenzen,
Dauern (Reflexion, Addle, Feint, Sheltron, Nascent Flash), Statusnamen (Gluttony: „Grants 2 stacks of
Duration"; Shake It Off: „Dispels Thrill of Battle and increasing"). Die Matrix zählt sie je Aktion.
Eine Lücke in einer Aufzählung aufgehobener Status verfälscht die Wechselwirkungen: Der Generator sieht
dort nur den ersten Namen. Wirkung auf RSR: Eine leere Dauer ergibt in
`DefensiveValues` 0 s. Keine dieser Aktionen ist Auslöser einer Streckung; wäre sie es, würde
sie nie strecken — der Generator erfindet keinen Wert.

## Werden die Fenster genutzt?

**Auftrag (seine Vorgabe, 26.09.2026):** prüfen, ob die optimalen Kombinationen in den Rotationen
tatsächlich genutzt werden — „für alle im Loop".

**Sachstand:** Geprüft ist, ob ein gewährtes Fenster — ein Status, der eine Aktion für eine Zeit
freigibt (Proc, „Ready", Stapel), oder eine Ressource mit Obergrenze — verbraucht wird, bevor es
verfällt oder überläuft. Ob eine Abfolge die beste ist, prüft dieser Abschnitt nicht; das prüft „Vergleich mit
The Balance", bisher für den Maschinisten (TODO, „Rotationen gegen The Balance abgleichen"). Die Dauern der
Fenster, die `ActionId.resx` ausblendet, belegt der offizielle Job-Guide: Ready to Break, Ready to
Reign, Ogi Namikiri Ready, Hypercharged, Full Metal Machinist je 30 s, Phantom Kamaitachi Ready 45 s,
Silken Symmetry und Silken Flow 30 s. Der Code braucht sie nicht: Er liest die Restzeit zur Laufzeit.

### Die Statusbedingung: Der Status muss bis zum Losgehen liegen

**Stufe „alle":** Eine Aktion mit Statusbedingung (`StatusNeed`, auf der Zielseite `TargetStatusNeed`)
ist nutzbar, solange der benötigte Status liegt, bis die Aktion losgeht und ihr Wirken endet
(`ActionBasicInfo.NeededStatusMargin`: Wirkzeit, bei einem GCD dazu die Restzeit des laufenden GCD).
Ein GCD wird vor dem Ablauf des GCD gewählt (`CalculatedActionAhead`) und geht los, wenn er abläuft;
deshalb zählt die Restzeit mit. Die Wirkzeit ist die angepasste des Spiels — unter Swiftcast oder
Dualcast null (belegt an WrathCombo, das „Wiederbelebung nur mit Swiftcast" über
`GetAdjustedCastTime == 0` prüft).

**Warum so:** Bis A166 war eine solche Aktion gesperrt, sobald ihr Status in `StatusRefreshGcdCount`
GCDs (ab Werk 2, rund 5 s) endete. Die Einstellung heißt in der Oberfläche „Number of GCDs before the
DOT/Status effect is reapplied" und erscheint nur bei Aktionen, die einen Status vergeben; ihr Text
bindet, und er spricht vom Erneuern, nicht vom Verbrauchen. Eingeführt hatte die Verwendung auf der
Bedarfsseite Upstream-Commit `dc067e0d3` (April 2025, „for better status determination"); vorher lag
die Grenze bei 0. Im Kampf war damit jeder Proc-Verbraucher aller Jobs in den letzten ~5 s seines
Fensters gesperrt, und jede Regel „vor Ablauf nutzen" mit kleinerem Horizont war wirkungslos
(Rotmagier Prefulgence, Weißmagier Divine Caress, Gelehrter Baneful Impaction, Ninja Meisui,
Pictomancer Tempera Grassa).

**Wo eine Grenze bleibt:** Einige Statusbedingungen sind keine Proc-Verbraucher, sondern „nur unter dem
Buff". Retrace (Schwarzmagier) versetzt Ley Lines und lohnt nur, wenn der Kreis den nächsten GCD
überdauert; diese Grenze steht in seinem `ActionCheck`. Upheaval (Krieger, braucht Surging Tempest)
geht sofort los und trifft unter dem Buff, solange er liegt; keine Grenze nötig.

**Betroffen:** alle Jobs (Endnutzer), jede abgeleitete Rotation mit `StatusNeed` (Paketnutzer),
Upstream-Pflege.

### Rückfälle vor Ablauf, je Job

Wartet ein Verbraucher auf eine Ausrichtung (Burst, Debuff), verfällt sein Fenster, wenn die Ausrichtung
nicht kommt. Ein Rückfall wirkt ihn in der letzten Gelegenheit; vorher bleibt jede Ausrichtung
unberührt.
- **Stufe „alle":** `CustomRotation.IsLastChanceBeforeStatusEnds(action, status)` — wahr, wenn die nächste
  Gelegenheit einen GCD nach dieser käme und das Fenster dann vor Losgehen und Wirken der Aktion endete
  (GCD-Restzeit + GCD-Länge + Wirkzeit, alle aus dem Spiel). Worauf ein Verbraucher wartet, bleibt die
  Regel des Jobs. Leser: Maschinist, Samurai, Revolverklinge, Ninja (unten) und Beschwörer (Searing Flash).
- **Rückfälle mit eigenem Horizont** (Paladin Atonement-Kette, Rotmagier Prefulgence, Weißmagier
  Divine Caress, Gelehrter Baneful Impaction, Ninja Meisui, Pictomancer Tempera Grassa, Monk Fire's/Wind's
  Reply): Fähigkeiten ohne Wirkzeit oder mit großzügigem Horizont; sie greifen, seit die
  Statusbedingung das Ende des Fensters nicht mehr sperrt. Nicht auf den gemeinsamen Baustein
  umgestellt, weil es sie nicht ändert.
- **Maschinist:** Hypercharged (Barrel Stabilizer, eine Überhitzung ohne Heat) wartet in den übrigen
  Pfaden auf Wildfire. Wird Wildfire zurückgehalten — „Only use Wildfire on Boss targets" gegen Trash,
  oder vor einer Pause —, fällt Hypercharge in der letzten Gelegenheit. Liegt Reassemble für ein Werkzeug
  bereit, wartet der Rückfall bis nach dessen GCD, sofern Hypercharged bis zum Einschub danach hält; sonst
  geht Hypercharge vor, und Reassemble landet auf einem Blazing Shot — fünf freie Überhitzungs-Schüsse
  wiegen mehr. Gesperrt bleibt er während einer laufenden Überhitzung (die Basisrotation lässt
  Hypercharge dort nicht zu, `!IsOverheated`).
- **Samurai:** Ogi Namikiri wartet auf einem Boss auf Higanbana. Kommt Higanbana nicht — abgeschaltet,
  oder „Prevent Higanbana use if theres more than one target" (ab Werk an) sperrt es über
  `NumberOfAllHostilesInRange`, während der Flächenpfad von Ogi `NumberOfHostilesInRange` unter zwei
  sieht —, fällt Ogi in der letzten Gelegenheit; sonst verfiele es samt Kaeshi. Der Rückfall rechnet die
  Wirkzeit von Ogi mit ein.
- **Revolverklinge:** Sonic Break und Reign of Beasts fallen nur unter No Mercy. Vergeht No Mercy ohne
  Platz dafür (Pause), fallen sie in der letzten Gelegenheit vor dem Ende von Ready to Break und Ready to
  Reign. Der Rückfall steht vor allen anderen GCDs, weil jeder andere einen GCD später noch kommen kann —
  Reign of Beasts aber nicht mitten in einer Gnashing-Fang- oder Reign-Kombo: Es übernähme den
  Kombo-Schritt und kostete Savage Claw und Wicked Talon samt Fortsetzungen.
- **Ninja:** Phantom Kamaitachi wartet auf das Trick-Attack- oder Mug-Fenster; Bunshin läuft auf eigener
  Abklingzeit. Öffnet sich kein Fenster vor Ablauf, fällt es im letzten GCD, nicht während einer
  Mudra-Ausführung.
- **Gelehrter:** Recitation wird für einen Verbraucher gedrückt (Excogitation, Indomitability, Adloquium,
  Succor/Concitation). Fällt der Anlass weg, bevor er greift – ein anderer heilt den Tank, die Flagge fällt –, oder
  findet Excogitation ihr Heilziel woanders, nimmt kein Verbraucher Recitation, und sie verfiele nach 15 s (A314). In
  der letzten Gelegenheit geht dann Excogitation auf den Tank: Sie heilt, wenn er unter die Hälfte fällt oder wenn sie
  ausläuft, ist also nie verschenkt. Liegt sie auf Abklingzeit, ein kritisches Adloquium auf ihn, dessen Barriere die
  nächsten Treffer aufbrauchen. Indomitability nicht: Auf voller Gruppe verfiele die Heilung, und die Fähigkeit fehlte
  dann 30 s lang.

**Weiser, Addersgall (A203):** Bei drei Stapeln und dem nächsten innerhalb eines GCD fällt Druochole (7 % MP
und eine Heilung, Wirktext) — kein Zielkonflikt, weil der Stapel sonst verloren ginge und nach dem Nachwachsen
wieder drei für Notfälle stehen. Die Uhr des Weisen zählt die verstrichene Zeit (Dalamud); `AddersgallTime`
rechnet sie in die Restzeit um. Prämisse: Die Uhr läuft bei drei Stapeln weiter. Der Job-Guide sagt
dazu nichts („alle zwanzig Sekunden, höchstens 3, lädt auch außerhalb des Kampfes"); belegt ist sie nur
mittelbar durch The Balance („be sure to not let it overcap" — ein Überlaufen setzt die laufende Uhr
voraus). Stimmt sie nicht, steht die Uhr bei vollem Vorrat, `AddersgallEndAfterGCD(1)` wird nie wahr, und
Druochole fällt nur nach Heilbedarf — kein Schaden, nur kein Gewinn. Gleiche Klasse: Die Lilien
des Weißmagiers haben ihren eigenen Überlaufschutz (`LilyAfterGCD`); die übrigen Heiler haben keine
selbst nachwachsenden Stapel.

### Methode und Werkzeug

Je Job die Verbraucher von Stapeln, Procs und begrenzten Ressourcen im Code nachgeschlagen und gegen
Ablauf und Überschreiben geprüft — ganz gelesen: Maschinist, Krieger, Paladin, Dunkelritter,
Revolverklinge; bei den übrigen die Stellen dieser Verbraucher. Dazu die Generatorliste „Fenster, deren
Verbraucher an Bedingungen hängt": Verbraucher mit eigener Statusbedingung, einem „…Ready" oder einer
`Has…`-Statuseigenschaft im `ActionCheck`, deren kein Aufruf ein schlichtes
`if (X.CanUse(out act)) { return true; }` direkt im Methodenrumpf ist, und ob ein Aufruf in seinem `if`
oder einem umschließenden `if` vor Ablauf feuert (`WillStatusEnd` mit dem eigenen Status). Ob der
Rückfall-Horizont die Wirkzeit deckt, prüft die Liste nicht; das steht je Fall oben.

### Die übrigen Kandidaten, bewertet

- *Die Bedingung ist das Fenster selbst oder Struktur:* Schnitter (Gallows, Gibbet und Executioner's
  unter Soul Reaver bzw. Executioner, Communio unter Enshroud, Perfectio), Viper (Legacies unter
  Reawakened, die Twinfang-/Twinblood-Folgen), Monk (Stufenprüfungen), Ninja (Mudra-Ausführung, Ninjutsu-
  Ziel), Rotmagier (Verfire/Verstone nach Manabalance, Grand Impact, Enchanted Riposte nach Mana), Pictomancer
  (Comet in Black, Star Prism, Subtractive Palette).
- *Die Bedingung ist der Zweck:* Heilbedarf (Horoscope, Pepsis), Option (Retrace), Burst (Radiant
  Encore, Reawaken, Starfall Dance, Technical Step, Flourish), Tanzschritte vor den Procs (Tänzer: Reverse
  Cascade, Fountainfall, Rising Windmill, Bloodshower warten, solange ein Schritt bereit ist; die Procs
  gelten 30 s laut Job-Guide, ein Tanz ist kürzer), Lux Solaris (Konzept 08, „Wann Lux Solaris zündet“), Ruin IV
  (Konzept 16, „Jobs in seinem Schwerpunkt“), Slipstream (wartet, bis die Gegner um sein Ziel bleiben, und fällt spätestens, sobald die
  Emerald Rites verbraucht sind; Garuda's Favor endet nicht mit der Zeit, nur mit bestimmten Beschwörer-Aktionen,
  Job-Guide – Konzept 04, „Schadensflächen erst, wo die Gegner bleiben"). Searing Flash fällt
  vor dem Ende von Ruby's Glimmer im Rückfall: Mit weiteren Beschwörern fällt Searing Light auch in
  einen Titan- oder Ifrit-Block, und die nächste Demi kommt nach dem Ende des Status (Konzept 16).
- *Sicherheit:* Krieger Primal Rend auf Distanz nur mit den Sprung-Optionen — seine Vorgabe zu Bewegung.
- *Niedrige Stufe:* Straight Shot, Trick Attack.

**Geprüft, ohne Befund:**
- *Tanks:* Paladin (Atonement-Kette, Divine Might, Requiescat, Goring Blade, Blade of Honor), Krieger
  (Infuriate überschreibt Nascent Chaos nicht, Beast Gauge läuft nicht über), Dunkelritter (Delirium-Kombo,
  Disesteem vorn), Revolverklinge (Fortsetzungen vorn, Munition ohne Überlauf).
- *Heiler:* Weißmagier (Lilien-Überlauf per Option, Sacred Sight), Gelehrter (Energy Drain leert
  Aetherflow vor dessen Abklingzeit), Astrologe (Karten vor dem nächsten Ziehen).
- *Nahkampf:* Monk (Fire's/Wind's Reply mit Rückfall), Dragoon (Wyrmwind Thrust vor dem Überlauf),
  Schnitter, Viper.
- *Fernkampf und Magie:* Barde, Tänzer, Pictomancer, Rotmagier, Schwarzmagier (Polyglot vor dem Überlauf, beide
  Rotationen), Beschwörer (Konzept 12).

**Bewusst so, mit Grund:** *Barde:* Soul Voice bleibt bis zu 25 s bei der Schwelle „Soul Voice Threshold
for Apex Arrow" (ab Werk 100), wenn Battle Voice kommt — Apex Arrow im Burst ist mehr wert als der
Überlauf (Schluss aus dem Regelaufbau).

**Dunkelritter, Blut für den Burst:** Bloodspiller und Quietus fallen unter Delirium, im
Zwei-Minuten-Fenster (`InTwoMIsBurst`) oder über 70 Blut; sonst wird Blut aufgespart (`UseBlood`).
Unterhalb der Stufe von Delirium und mit Delirium-Stapeln fallen sie frei. Grund: The Balance (7.5,
Leitfaden, keine Spielquelle): Bloodspiller unter Gruppenbuffs oder gegen Überlauf, mit möglichst viel
Blut bis 70 in Delirium — Delirium gibt drei Stapel Blood Weapon zu je +10 (Job-Guide). Im Kampf: Mehr
Bloodspiller fallen in die Gruppenbuffs statt davor. Vor A197 las keine Regel `UseBlood`, und
Bloodspiller fiel bei 50 Blut, wann immer möglich.

## Pausen und Phasenenden

**Sachstand:** Seine Angabe zum Profil — Machinist zwischendurch, alle anderen Kampfjobs
seltener — holt diese Jobs in die Bearbeitung (A162).

**Die Pause selbst ist eine Regel für alle, was darin fällt, eine Regel des Jobs.**
- **Stufe „alle":** `CustomRotation.InCombatPause` — im Kampf, kein Gegner in 25 Yalm; nichts zu
  schlagen. Ohne BossModReborn lesbar. Die vorhergesagte Pause ist `BMRDowntimeWithin` (nur mit Modul).
- **Samurai:** Meditate in der Pause („Gradually increases your Kenki Gauge", nur im Kampf). Die
  Basiseinstellung verweigert es in Bewegung; es endet bei Bewegung.
- **Schnitter:** Soulsow in der Pause (`ReaperRotation.SoulsowFitsThePause`, A271). Im Kampf 5 s Wirkzeit („Cast time is
  instant when used outside of battle"); The Balance, Reaper Basic Guide (12.10.2026): „Make sure to get one Harvest Moon
  between each pull and downtime!" Harvest Moon trifft mit 800 Potenz und gibt 10 Soul (Job-Guide); RSR wirkt es, wenn
  der Schnitter nicht im Nahkampf zuschlagen kann. Mit Modul fällt Soulsow nur, wenn die Wirkzeit vor dem vorhergesagten
  Ende der Pause fertig ist (`BMRDowntimeEndIn`); ohne Modul ist die Länge der Pause unbekannt, und es fällt.
- **Machinist:** Rook/Queen Overdrive im letzten GCD vor einer vorhergesagten Pause, wenn die Königin
  sonst erst in der Pause endet. Ihr Wirktext: Ungewirkt fällt der Abschluss „automatically immediately
  before shutting down" — in der Pause trifft er nichts. Nur mit Modul; ohne Vorhersage ist die Pause
  erst bekannt, wenn sie begonnen hat.

**Bestehendes Pausenverhalten bleibt, wie es ist,** weil es eine andere Frage beantwortet: Der Monk lädt
schon Chakra, wenn kein Gegner in **seiner** Reichweite steht (3 Yalm, auch wenn er nur für eine Mechanik
herausläuft); der Machinist verschießt Heat vor einer vorhergesagten Pause (`BmrDumpBeforeDowntime`, ab
Werk an).

**Nicht gebaut, mit Grund:**
- *Six-sided Star (Monk):* Job-Guide (02.10.2026): 780 Potenz, +80 je offenem Chakra, schließt alle Chakren,
  Wiederholzeit 4 s — zwei GCDs lang. The Balance, Monk Basic Guide: „two GCDs is always more potency than doing
  one SSS"; nützlich, wo die Zeit nicht für zwei GCDs reicht — „before running out of a big aoe, before a boss
  dies, or before a boss goes untargetable". Damit ist die Einsatzregel belegt: der letzte GCD vor einer Pause oder
  dem Tod des Ziels, wenn nur noch einer passt. Ob Chakra über eine Pause verfällt, nennt keine der Quellen.
  WrathCombo (Stand 25.09.2026) führt die Aktion nur als Id und wirkt sie nicht. Offen im TODO.
**Folgen, bewusst hingenommen:**
- Läuft der Samurai im Kampf aus 25 Yalm heraus, ohne dass eine Pause ist, fällt Meditate, sobald er
  steht. Es bricht bei der ersten Bewegung und kostet dann einen GCD-Takt.
- Liegt die vorhergesagte Pause später als gemeldet, verliert die Königin ihre letzten Angriffe
  zugunsten des Abschlusses.
- Kehrt das Ziel ohne Modul während der 5 s von Soulsow zurück, kommt der erste GCD danach bis zu einer Wirkzeit
  später. Läuft der Schnitter nur aus 25 Yalm heraus, fällt Soulsow wie Meditate, sobald er steht, und bricht bei
  der ersten Bewegung ab; dafür steht danach Harvest Moon für den nächsten Rückzug bereit.

## Vergleich mit The Balance

**Sachstand:** Abgeglichen ist der Maschinist (24.10.2026) gegen The Balance, Machinist Basic Guide und Openers
(beide „Patch Applicable: 7.3", zuletzt geändert am 05.08.2025); die Potenzen nennt der offizielle Job-Guide (Patch
7.5). Der Beschwörer, die vier Tanks und die vier Heiler sind abgeglichen (26.10.2026, A375 bis A378), von den übrigen
Schadensausteilern Ressourcen und Buffs (A379); die Folgen in ihren Burstfenstern sind offen (TODO, „Rotationen gegen The Balance abgleichen"). Abweichungen, die nur Schaden
betreffen, werden behoben, wo sich die bessere Wahl aus dem Spiel ableiten lässt und keine seiner Regeln dagegen steht;
die übrigen nennt der Abgleich mit Grund. Weniger Schaden aus einer Sicherheitsregel bleibt (seine Präzisierung).

### Maschinist

**Auto Crossbow fällt nur, wo es mehr bringt als Blazing Shot samt dem, was Blazing Shot nach sich zieht**
(`MachinistRotation.AutoCrossbowOutdamagesBlazingShot`, gelesen von `MCH_Reborn`, A342). Blazing Shot nimmt Double
Check und Checkmate je 15 s Wiederholzeit ab, und beide treffen jeden Gegner im Umkreis von 5 Yalm um ihr Ziel; Auto
Crossbow nimmt ihnen nichts ab. Verglichen wird Auto Crossbow auf den Gegnern im Kegel mit Blazing Shot samt
Überhitzungsbonus und dem Anteil an Double Check und Checkmate, den die gekürzte Wiederholzeit kauft – auf so vielen
Gegnern, wie diese beiden von ihrem Ziel aus erreichen. Unter Stufe 92 treten Gauss Round und Ricochet an ihre Stelle,
unter Stufe 68 Heat Blast an die von Blazing Shot.

- *Woher die Zahlen kommen:* Potenzen, Abschlag je weiterem Gegner, die 15 s und der Überhitzungsbonus aus den
  Tooltips zur Laufzeit (`ActionTooltip`, englisches Blatt `ActionTransient` über Dalamuds SeString-Auswertung),
  die Wiederholzeit aus dem Spiel, die Zahl der erreichten Gegner aus der Zielwahl der beiden Aktionen. Damit folgt die
  Schwelle dem laufenden Patch und dem Stand der Gegner.
- *Was es im Kampf ergibt* (Job-Guide 7.5, `machinist_balance_model.py`): Stehen alle Gegner im Umkreis von Double Check
  und Checkmate, fällt Auto Crossbow ab 6 Gegnern; bei 3 Gegnern bringt Blazing Shot 692 gegen 540. Steht einer außerhalb,
  ab 4, stehen zwei außerhalb, ab 3. The Balance nennt dieselben Schwellen („Blazing Shot on 1-5, Auto Crossbow on 6+";
  „If 1 target is out of range, Auto Crossbow is a gain on 4+ targets"). Vorher fiel Auto Crossbow ab 3 Gegnern im
  Kegel – im gesammelten Rudel bis zu 22 % weniger je Hypercharge-GCD.
- *Seine Einstellung bleibt die Untergrenze:* Die AoE-Zahl der Aktion (ab Werk 3) muss erfüllt sein, bevor verglichen
  wird. Unter 3 Gegnern gewinnt Auto Crossbow nie.
- *Rückfall:* Lässt sich eine Zahl nicht lesen, geht Auto Crossbow wie vorher nach seiner AoE-Zahl. `DefenseTrace.log`
  schreibt je Stufe einmal, was gelesen wurde oder was fehlte.
- *Grenzen, bewusst hingenommen:* Ob die SeString-Auswertung einer Stufensynchronisierung folgt, ist nicht belegt;
  folgt sie nicht, rechnet die Regel mit den Potenzen der vollen Stufe. Der Vergleich nimmt an, dass Double Check und
  Checkmate ihre gewonnene Zeit auch nutzen – am Ende eines Rudels, wenn Ladungen verfallen oder Gegner sterben,
  überschätzt er Blazing Shot.

**Flamethrower fällt anstelle des Füllers auf ein stehendes Rudel** (`MachinistRotation.FlamethrowerFits`, gelesen von
`MCH_Reborn` hinter „Use Flamethrower on a pack that stands still", ab Werk an, A380). Er trifft jeden Gegner im Kegel vor
dem Maschinisten 10 s lang jede Sekunde mit 120 Potenz (Job-Guide; die Sekunde nennt nur der deutsche Text, „im
Sekundentakt") und löst die GCD-Abklingzeit aus; jede andere Aktion, Bewegung und Drehung beendet ihn.

- *Wann:* im Kampf, nicht überhitzt; auf so vielen Gegnern im Kegel, wie seine AoE-Zahl verlangt (ab Werk 2); kein
  Excavator und kein Full Metal Field bereit; Hypercharge weder nutzbar noch – bei genug Heat oder Hypercharged –
  innerhalb des Kanals wieder bereit; Air Anchor (darunter Hot Shot) und Chain Saw nicht innerhalb des Kanals bereit,
  Barrel Stabilizer und Wildfire ebenso nicht, solange Burst an ist (Wildfire nicht, wo die Rotation es ohnehin
  zurückhält: „Only use Wildfire on Boss targets" gegen ein Ziel ohne Boss-Symbol); Drill füllt seine Ladungen nicht
  innerhalb des Kanals auf. Der Kanal zählt ab dem Druck so lange, wie der Tooltip als Dauer nennt.
- *Wo:* Das Rudel steht (`DamageAreaSettled` mit dem Radius der Aktion – dieselbe Bewegungserkennung wie für
  Schadensflächen, aber nicht an deren Einstellung gebunden; die Option der Rotation trägt die Entscheidung), kein Gegner
  wirkt einen Flächenangriff (`IsHostileCastingAOE`), und die Gegner überleben den Kanal, wo ihre Zeit bis zum Tod bekannt
  ist. Grund: Der Kanal bindet ihn an Ort und Blickrichtung; eine angesagte Fläche zwingt ihn zur Bewegung und kostet den
  Rest.
- *Warum ab 2:* Modell (`machinist_balance_model.py`, Job-Guide 7.5): je GCD-Zeit 300 Potenz je Gegner. Auf einem
  Gegner verliert er gegen die Heated-Kombination (320), auf zwei bringt er 600 gegen 320 – mit dem Wert von Heat und
  Batterie, die der Füller einbringt, 596: gleichauf. Ab drei liegt er auch dann vorn (900 gegen 848, gegen Scattergun).
  The Balance: „If gauge is not usable, beats filler on 2+". Beide Lesarten von „gauge is not usable" – die Leiste ist
  gerade nicht ausgebbar, oder was der Füller einbringt, wird nicht mehr ausgegeben – ergeben dieselbe Zahl. Die AoE-Zahl
  stand bis A380 auf 6 („Beats Air Anchor (incl. battery) on 6+"); gewirkt hat RSR ihn nie.
- *Zielwahl, ein Defekt der Klasse:* Das Spielblatt führt Flamethrower als auf den Wirkenden gezielt (`CastType` 1,
  Reichweite 0, Wirkradius 12). Die Zielwahl hielt ihn deshalb für eine Einzelzielaktion, suchte ein Ziel in 0 Yalm,
  fand keins und überging die AoE-Zahl – auch die Oberfläche blendete sie aus. Unter allen Spieleraktionen haben nur
  Flamethrower und Apokalypsis (Blaumagier) diese Form (xivapi, 26.10.2026). Jetzt liest `ActionTargetInfo` für diese
  Form die Gestalt aus dem Wirktext („in a cone before you", „in a straight line before you",
  `ActionTooltip.StatedShapeBeforeYou`) und zählt die Gegner im Kegel vor dem Spieler wie bei jedem anderen Kegel, den
  er selbst wirft.
- *Abweichung von The Balance:* Der Guide nennt 11 s als Fenster ohne Herleitung; RSR rechnet ab dem Druck mit der
  angegebenen Dauer (10 s). Wird eine der großen Abklingzeiten genau in dieser letzten Sekunde bereit, kommt sie bis zu
  einer Sekunde später. Reassemble, Double Check und Checkmate wartet die Regel nicht ab: Der Guide nennt sie nicht, und
  ein Drittel einer Ladung, das an der vollen Leiste verfällt (rund 60 Potenz je Aktion), wiegt weniger als der Kanal ab
  drei Gegnern gewinnt. Auf zwei Gegnern mit nutzbarer Leiste wöge es mehr; dort liegen die Ladungen aber selten voll,
  weil `MCH_Reborn` beide bei eingeschaltetem Burst bis zur letzten Ladung verschießt. Während des Kanals wartet auch die Automatenkönigin; Batterie verfällt dabei nicht, weil der
  Kanal keine einbringt.
- *Grenzen, bewusst hingenommen:* Der Halbwinkel des Kegels ist der, den die Zielwahl für alle Kegel nimmt (60°); das
  Spiel nennt ihn nicht. Auf zwei Gegnern mit nutzbarer Leiste ist der Gewinn null – die Regel tauscht dort Gleiches
  gegen Gleiches und gewinnt erst, wo die Leiste verfiele oder mehr Gegner stehen.

**Weitere Punkte, gegen The Balance geprüft:**

| Punkt | The Balance | RSR | Stand |
|---|---|---|---|
| Opener | Reassemble vor dem Pull, Air Anchor zum Pull; Variante mit dem zweiten Drill nach dem Hypercharge-Fenster | Reassemble ab 4,75 s, Air Anchor bei 0,1 s, danach dieselbe Folge | deckt sich mit der Variante (Modell, A342) |
| Trank im Opener | 2 s vor dem Pull | ab 1 s, nur mit „Use burst medicine in countdown (requires auto burst option on)" (Option der Rotation, ab Werk aus) | Ob 1 s Air Anchor verzögert, hängt an der Animationssperre des Tranks, die keine der Quellen nennt; nicht geändert |
| Hypercharge vor einem Werkzeug | nicht, wenn Air Anchor, Drill oder Chain Saw in unter 8 s bereit wird (drei GCDs Überhitzung, 7,5 s) | `ToolChargeSoon`: dieselben 8 s; bei Drill erst, wenn beide Ladungen voll würden | deckt sich; Drill hat zwei Ladungen, verloren geht erst eine volle |
| Scattergun | ab 3 Gegnern | AoE-Zahl 3 | deckt sich |
| Bioblaster | schlägt Drill ab 3, Air Anchor ab 4, nie Chain Saw/Excavator | vor Air Anchor, Drill, Excavator und Chain Saw, ab 3 Gegnern | Bei genau drei Gegnern und gleichzeitig bereitem Air Anchor kommt Bioblaster einen GCD früher. Beide fallen innerhalb zweier GCDs; gleich ist die Potenz, nur Air Anchor oder Chain Saw rückt einen GCD nach hinten. Die Grenze 4 hängt am Wert der Batterie, den kein Tooltip nennt (The Balance: 266 Potenz je 10 Batterie, Stand 7.3). Nicht gebaut |
| Automatenkönigin | 60 im Opener, 90 nach einer Minute, danach 100 direkt nach Air Anchor in jedem Zwei-Minuten-Fenster | dieselben ersten beiden; danach ruft sie der Schutz gegen das Überlaufen der Batterie | Modell über zehn Minuten (24.10.2026): keine Batterie verloren, die Königinnen verlassen nach der zweiten Minute die Zwei-Minuten-Fenster. Ihr Schaden zieht aus keinem eigenen Buff des Maschinisten Nutzen (Wildfire zählt nur seine Waffenfertigkeiten); der Gewinn der Ausrichtung kommt allein aus den Buffs der anderen. Nach seiner Regel ist fremdes Verhalten kein tragender Grund – derselbe Schluss wie für Reassemble (Konzept 17). Nicht gebaut |
| Flamethrower | ab 2 Gegnern, wenn die Leiste nicht nutzbar ist; nie vor Hypercharge und den Werkzeugen; keine große Abklingzeit in den 11 s des Kanals | anstelle des Füllers, Einsatzregel oben | deckt sich bis auf die Länge des Fensters (A380) |

### AoE-Schwellen aller Jobs

Erhoben am 24.10.2026 (A344): die Schwellen, die The Balance je Job nennt, gegen die AoE-Zahlen der Aktionen, wo nötig
mit den Potenzen von 7.5 aus dem Job-Guide nachgerechnet. Den Potenzvergleich führt `ActionTooltip.TryPotencyOn` und
`AreaOutdamagesSingle` für alle Jobs an einer Stelle; er liest die Tooltips zur Laufzeit und fällt auf die AoE-Zahl
zurück, wo eine Zahl fehlt. `DefenseTrace.log` nennt einmal je Aktion und Stufe einen Tooltip, der keine Potenz nennt.

- *Deckt sich (3+):* Dragoon, Viper, Samurai, Red Mage, Summoner, Bard, Black Mage (AoE-Rotation), Reaper bis auf die
  Fälle unten. Ohne ausdrückliche Zahl im Guide: Monk, Pictomancer, Dancer und die Heiler; nicht nachgerechnet.
- *Tanks (2 statt 3):* RSR wirkt die AoE-Kombos von Paladin, Krieger, Dark Knight und Gunbreaker ab 2 Gegnern, The
  Balance nennt für den Schaden 3 (Dark Knight unter Stufe 94: 2). Bleibt: Der zweite Gegner hält sonst nur die
  Feindseligkeit vom Pull, und The Balance selbst rät für Dungeons „In multi-target situations, use AoE so that you can
  keep aggro on all mobs" (Dark Knight Basic Guide). Sicherheit der Gruppe vor Schaden (seine Regel).
- *Black Mage, Foul:* schlägt Xenoglossy ab 2 Gegnern (600 + 450 gegen 890; The Balance: „Foul is now a gain in
  2-target situations over Xenoglossy"). RSR wirkte es ab 3. Jetzt AoE-Zahl 2, und beide Rotationen prüfen den
  Potenzvergleich dazu, der auf Stufen entscheidet, auf denen Xenoglossy noch gewinnt.
- *Reaper:* Grim Swathe (140 je Gegner) gegen Unveiled Gibbet/Gallows (440) erst ab 4, gegen Blood Stalk (340) ab 3;
  Guillotine und Grim Reaping mit liegendem Enhanced-Buff ebenso knapp. Die Abweichung liegt bei höchstens 20 Potenz
  je Einsatz. Nicht geändert.
- *Ninja* (A345): The Balance steht dort auf 7.05; seither sind die Potenzen geändert, gerechnet ist mit 7.5.
  `NIN_Reborn` schaltete seine AoE-Wahl an der AoE-Zahl von Death Blossom um (3 Gegner um den Spieler). Das bleibt für
  die Paare, deren Schwelle 3 ist, und weicht dem Vergleich, wo sie anders liegt:
  - *Hellfrog Medium statt Bhavacakra* (unter Higi Deathfrog Medium statt Zesho Meppo): 250 (400) je Gegner gegen 400
    (700) – ab 2 Gegnern, außer Meisui hebt den Einzeltreffer auf 550 (850). AoE-Zahl jetzt 2; darüber entscheidet
    `HellfrogOutdamagesBhavacakra` mit den Tooltip-Potenzen samt Meisui-Bonus. Bei zwei Gegnern außerhalb von Meisui
    100 Potenz mehr je 50 Ninki.
  - *Goka Mekkyaku statt Hyosho Ranryu* unter Kassatsu: 850 je Gegner gegen 1300, Kassatsu hebt beide gleich – ab 2
    Gegnern. `GokaOutdamagesHyosho` zählt, wen Goka von seinem Ziel aus erreicht; ohne volle AoE-Einstellung oder
    lesbare Zahl gilt die alte Weiche. Bei zwei Gegnern 400 Potenz mehr je Kassatsu.
  - *Doton nur, wenn die Gegner seine Dauer überleben* – die Bedingung von The Balance („if targets remain in it for
    the full duration"). `DotonOutlived` hält die durchschnittliche Zeit bis zum Tod der Gegner gegen die Dauer aus
    dem Tooltip (18 s); sonst Katon. Solange die Zeit unbekannt ist, wie bisher. WrathCombo nimmt dafür eine
    Gesundheitsschwelle des Ziels (40 %, Einstellung).
  - *Unverändert, gerechnet:* Katon statt Raiton ab 3 (350 je Gegner gegen 740 plus den Raiju, den Raiton gewährt, 700
    statt eines Kombo-GCDs); Huton statt Suiton ab 3; Doton statt Raiton ab 3. *Unverändert, nicht gebaut:* Death
    Blossom und Hakke Mujinsatsu lohnen ohne liegendes Doton erst ab 4 (110 je Gegner gegen rund 410 der
    Einzelkombo); RSR legt Doton ab 3 Gegnern selbst, sobald der Ninja steht und das Rudel bleibt (Konzept 04,
    „Schadensflächen erst, wo die Gegner bleiben"); im Anlauf eines Rudels kommt Katon.

### Dragoon und Viper: Opener

Geprüft ist bisher nur der Vorlauf (A343); Burst und Prioritäten sind offen.

- *Dragoon* (The Balance, Openers, Patch 7.5): kein Vorlauf im Standard-Opener; der Trank nach dem zweiten GCD, mit
  Lance Charge, vor Battle Litany. RSR nimmt ihn, sobald Battle Litany liegt – einen GCD später; der Trank wirkt 30 s
  und deckt das Fenster weiter. Nicht geändert.
- *Viper* (Openers, Patch 7.55): Slither bei −0,5 s – RSR springt in der letzten halben Sekunde, wenn das Ziel
  außerhalb der Nahkampfreichweite steht (Konzept 04, „Der Sprung zum Ziel am Ende des Countdowns", A347); der Trank
  fällt nach dem dritten GCD. RSR nahm ihn mit „Experimental Pot Usage(used up to 5 seconds before SerpentsIre comes
  off cooldown)" nur in den letzten 5 s vor dem Ende der Abklingzeit von Serpent's Ire, nie solange Serpent's Ire
  bereit lag, also nie im Opener. Der Text schließt das ein; der Code folgt ihm jetzt (`WillHaveOneCharge`). Ohne die
  Option (ab Werk aus) ändert sich nichts.
- *Viper, Restzeit der Buffs vor Reawaken* (je 10 s, Einstellungen „How long on the status time for Swift/Hunt needs to
  be to allow reawaken use"): Die Folge bis zum Druck auf Ouroboros dauert unter Swiftscaled 8,7 s – Reawaken 2,2 s und
  vier Generationen zu je 2,0 s Wiederholzeit, je um 15 % gekürzt (Job-Guide, abgerufen am 25.10.2026). 10 s decken sie
  mit Puffer, Hunter's Instinct liegt also auf allen Treffern der Folge. Dass ein Buff danach bis zur nächsten
  Erneuerung ausläuft, ist die Abwägung, die die Einstellung dem Spieler lässt (früher Reawaken gegen kurze Lücke); The
  Balance nennt keinen Wert (Basic Guide, abgerufen am 25.10.2026). Nicht geändert.

### Beschwörer

Abgeglichen am 26.10.2026 gegen The Balance, Summoner Basic Guide („Patch Applicable: 7.55", zuletzt geändert am
31.07.2026; die Opener-Seite zeigt die Folge nur als Bild). Die Searing-Light-Regeln selbst stehen in Konzept 12.

**Die Necrotize der ungeraden Minute warten auf das Zwei-Minuten-Fenster** (`SMN_Reborn.AttackAbility`, A375). The
Balance: „Necrotize from your odd minute Energy Drain … should be saved for 2-minute buff windows as long as they are
used before the next Energy Drain." RSR gab den Aetherflow aus, sobald Energy Drain in zwei GCDs bereit wurde (Schutz
gegen das Überlaufen, Konzept 16). Energy Drain kehrt aber zur selben Zeit zurück wie Solar Bahamut: in der ungeraden
Minute gewirkt, nach 60 s wieder bereit, während der Demi-Takt 60 s beträgt. Die beiden Necrotize fielen so jedes Mal
wenige Sekunden vor Searing Light und dem Fenster der Gruppe.
- *Jetzt:* Sind die Burst-Beschwörung (Solar Bahamut, darunter Bahamut) und Searing Light spätestens mit Energy Drain
  wieder bereit und ist die nächste große Beschwörung die des Bursts, wartet der Aetherflow; im Fenster gibt ihn der
  bestehende Zweig aus, und Energy Drain, das einen leeren Aetherflow verlangt (`ActionCheck`), folgt eine Einwebung
  später. Der Takt verschiebt sich dadurch einmal um diese Einwebung und bleibt dann, weil Energy Drain und der
  Demi-Takt dieselben 60 s haben; das Fenster der Demi (15 s) fasst Energy Drain weiter.
- *Der Überlaufschutz* rechnet jetzt eine Einwebung je Ladung (`WillHaveOneChargeGCD(AetherflowStacks)`) statt fester
  zwei GCDs: Jede Ladung braucht einen Platz. Bei zwei Ladungen dasselbe wie vorher.
- *Im Kampf:* je zwei Minuten zwei Necrotize unter Searing Light (5 %) und den übrigen Buffs der Gruppe statt davor. Der
  eigene Teil: 2 × 500 Potenz × 5 % = 50 Potenz alle zwei Minuten – Promillebereich; der Teil aus den Buffs der anderen
  ist eine Annahme über sie, kein tragender Grund. Gebaut, weil es aus Spieldaten folgt und nichts kostet.
- *Nicht gewartet* wird, wenn der Burst automatisch aus ist, die nächste große Beschwörung nicht die des Bursts ist
  (ein anderer Beschwörer hält sie, Konzept 12) oder eine der beiden später zurückkommt als Energy Drain. Stirbt das
  Ziel, gehen die Ladungen wie bisher.

**Weitere Punkte, gegen The Balance geprüft:**

| Punkt | The Balance | RSR | Stand |
|---|---|---|---|
| Opener | Titan zuerst; Ruin III vorab gewirkt | Ruin III im Countdown, Reihenfolge ab Werk Titan, Garuda, Ifrit | deckt sich |
| Searing Light | „on cooldown to align with all other two minute party buffs" | im Solar-Fenster, Sonderfälle mehrerer Beschwörer | Konzept 12 |
| Energy Drain | früh in der Demi | in jeder großen Beschwörung, sobald der Aetherflow leer ist | deckt sich |
| Sunflare, Exodus, Akh Morn, Revelation | unter Buffs | sofort in der großen Beschwörung, nach Searing Light | deckt sich |
| Mountain Buster | jede Gunst sofort einweben | nach Searing Light und Lux Solaris, vor dem Aetherflow | deckt sich |
| Ruin IV | eine je Zyklus, nie in einer Demi | nur außerhalb der Demi und der Primal-Phase (`UseFillers`) | deckt sich |
| Ruin III | so selten wie möglich, Demis nicht verzögern | nur als letzter Füller | deckt sich |
| Ifrit | in Nahkampfreichweite, Crimson Cyclone schließt auf | Crimson Cyclone nur bis 3 Yalm (Einstellung), Ifrit im Rückfallblock nur am Ziel | weicht ab: Aus einer sicheren Position holt ihn RSR nicht für Schaden (seine Regel) |
| Swiftcast | Wiederbelebung zuerst, Slipstream nur situativ | Wiederbelebung (Einstellung an), alle Rotationsoptionen dafür ab Werk aus | deckt sich mit seiner Regel |
| Lux Solaris | für einen Raidwide halten, vor Ablauf nutzen | eigene Entscheidung (Konzept 08) | deckt sich |
| Rekindle | immer nutzen | niedrigster Anteil, sonst er selbst; spätestens drei GCDs vor Phasenende | deckt sich |
| Trank | Opener spät eingewebt, danach im Sechs-Minuten-Fenster | unter dem eigenen Searing Light, nach „Tincture usage" (ab Werk nur in High-End-Inhalten); nach 270 s Abklingzeit ist das nächste eigene Fenster das der sechsten Minute | deckt sich |
| Lucid Dreaming | bei rund 70 % MP | Schwelle in MP, ab Werk 6000 von 10000 | seine Einstellung (Konzept 18) |
| Physick | kein Einsatz im Endgame | nur mit „Use Physick above level 30" (ab Werk aus) | deckt sich |

### Weißmagier

Abgeglichen am 26.10.2026 gegen The Balance, White Mage Basic Guide („Patch Applicable: 7.3", zuletzt geändert am
07.08.2025, Opener-Abschnitt am 15.09.2025). Der Guide ist ein Werkzeugüberblick, keine feste Rotation; was er zur
Heilung sagt, prüft RSR an seinen Sicherheitsregeln, nicht am Schaden.

**Göttlicher Segen an der Ladungsgrenze** (`TryChargedBarrierAtCap`, Konzept 08, A376). The Balance: „Try to not let
this overcap on charges - you're almost always guaranteed to get value out of this just slapping it on the tank for
autos." RSR setzte ihn nur in Abwehr- und Heilpfad; stand niemand in Gefahr, ruhten beide Ladungen, und die
Wiederaufladung stand still. Jetzt geht die Ladung an der Grenze auf den Angegriffenen, wo keine Barriere liegt. Im
Kampf: eine Barriere von 500 Potenz mehr je 30 s, solange der Tank angegriffen wird; die zweite Ladung bleibt für die
Gefahr, und weil ein Auslöser mit einer Ladung zurück nicht streckt, hält die Barriere den Wasserschleier für einen
Tankbuster danach nicht zurück. Dieselbe Regel gilt für Celestial Intersection des Astrologen.

**Weitere Punkte, gegen The Balance geprüft:**

| Punkt | The Balance | RSR | Stand |
|---|---|---|---|
| Opener | Glare III bei −2,3 s, Trank nach dem Pull, Presence of Mind nach dem dritten, Assize nach dem vierten GCD | Glare im Countdown; mit „Use the balance Opener in High-End Duties" Presence of Mind ab dem dritten, Assize ab dem vierten GCD | deckt sich |
| Trank im Opener | nach dem Pull | bei unter 3 s im Countdown, nach „Tincture usage" | weicht um rund drei Sekunden ab: Der Trank endet so viel früher, also vor dem letzten Glare seines Fensters. Nicht geändert – der Unterschied liegt unter einem GCD Trankwirkung, und das Fenster der Gruppe ist eine Annahme über sie |
| Dia | dauerhaft, außer das Ziel stirbt in unter 15 s | DoT-Regel nach Zeit bis zum Tod gegen den Füller (`DotEffect`) | deckt sich, genauer: rechnet die Gewinnschwelle statt fester 15 s |
| Glare IV | drei Einsätze unter Presence of Mind | sobald verfügbar | deckt sich |
| Holy | in Dungeons stark (Betäubung), im Raid ab drei Gegnern | AoE-Zahl 3 (A344), dazu Streckung und Rückhaltungen der Betäubung | deckt sich |
| Afflatus Misery | unter Buffs, wo es geht; keine Blutlilie überlaufen lassen | mit Opener-Option unter Buffs oder Presence of Mind; Lilien nur bei weniger als drei Blutlilien | deckt sich |
| Angefangene Lilien-Sätze | vor Kampfende abschließen | keine eigene Regel | nicht gebaut: Schaden im Promillebereich, und am Kampfende ist eine Heilung ohne Bedarf keine Sicherheit |
| Assize, Presence of Mind | auf Abklingzeit | auf Abklingzeit, im Opener verzögert | deckt sich |
| Göttlicher Segen | Ladungen nicht überlaufen lassen | gefahrbezogen, an der Ladungsgrenze auf den Angegriffenen (oben) | deckt sich seit A376 |
| Wasserschleier | für einen gewöhnlichen Tankbuster nie nötig; für ungeplanten Schaden | gefahrbezogen; nicht für einen gemessenen kleinen Tankbuster (Konzept 09, A373) | deckt sich in der Richtung; für große Treffer gibt RSR ihn weiter, Sicherheit vor Schaden |
| Tetragrammaton, Benediction | nicht für Notfälle zurückhalten | Tetragrammaton im Einzelheilpfad mit allen Ladungen; Benediction unter seiner Schwelle | Benediction weicht ab: seine Einstellung |
| Plenary Indulgence | vor GCD-Flächenheilungen | vor Afflatus Rapture, Medica und Cure III | deckt sich |
| Asylum | in Dungeon-Pulls als Regen auf den Tank | bei Flächenheilbedarf; mit „Use Asylum as soon as a single player heal …" auch für einen Einzelnen | weicht ab: Im Pull hält RSR Regen auf dem Tank (`TrySustainRegenOnTank`); Asylum bleibt für die Gruppe |
| Lucid Dreaming | bei rund 80 % MP | Schwelle in MP, ab Werk 6000 von 10000 | seine Einstellung (Konzept 18) |
| Thin Air | für MP-teure Heilungen und Wiederbelebung | Einstellungen, ab Werk nur unter MP-Druck | deckt sich |
| Wiederbelebung | mit Swiftcast und Thin Air | Swiftcast bleibt dafür (seine Regel); Thin Air vor der Wiederbelebung | deckt sich |

### Tanks

Abgeglichen am 26.10.2026 gegen die Basic Guides von The Balance: Krieger („Patch Applicable: 7.4", 27.12.2025),
Paladin (7.4, 17.12.2025), Dunkelritter (7.5, 14.08.2026), Revolverklinge (7.4, 17.12.2025). Ihre Abwehr steht in
Konzept 09; hier der Schaden. Keine Änderung: Alle Kernregeln decken sich. Die AoE-Schwelle der Tanks bleibt bei 2
(oben, „AoE-Schwellen aller Jobs").

| Job | Punkt | The Balance | RSR | Stand |
|---|---|---|---|---|
| Krieger | Opener | Tomahawk bei −0,7 s, Infuriate eingewebt | Tomahawk bei unter 0,54 s, Infuriate in der ersten Einwebung | deckt sich |
| Krieger | Surging Tempest | bei 7 bis 15 s Rest erneuern, nie über 31 s | Storm's Eye neun GCDs vor dem Ablauf (`StatusRefreshGcdCount`) | deckt sich: Bei rund 22 s Rest plus 30 s bleibt die Wirkung unter der Obergrenze von 60 s, es geht nichts verloren |
| Krieger | Beast Gauge | Storm's Path nicht bei 90 bis 100, Infuriate nicht bei 60 bis 100 | Storm's Path bis 80, Infuriate bis 50 (`ActionCheck`) | deckt sich |
| Krieger | Upheaval, Inner Release | auf Abklingzeit | auf Abklingzeit, Inner Release mit Surging Tempest | deckt sich |
| Krieger | Onslaught | drei Ladungen in den geraden, eine in den ungeraden Fenstern | im Burst und vor dem Überlauf, nur im Stand; die Reichweite begrenzt die zentrale Sprungprüfung | weicht ab, wo Onslaught ihn bewegen würde: seine Regel (Konzept 04) |
| Paladin | Fight or Flight | auf Abklingzeit; Imperator direkt danach, vor Confiteor | Fight or Flight mit bereiten Folgen; Imperator und Requiescat in Fight or Flight | deckt sich |
| Paladin | Circle of Scorn, Expiacion | in jedem Fight-or-Flight-Fenster | erst, wenn Fight or Flight und Imperator laufen oder abklingen | deckt sich |
| Paladin | Atonement-Folge | zwischen den Fenstern aufheben, vor dem nächsten Royal Authority ausgeben | in Fight or Flight, vor dem Ablauf oder wenn Rage of Halone bereit ist | deckt sich |
| Paladin | Intervene | zwei Ladungen in Fight or Flight | im Stand, mit Option alle Ladungen in Fight or Flight | deckt sich, mit seiner Bewegungsregel |
| Dunkelritter | Edge of Shadow | gegen MP-Überlauf, gegen das Auslaufen von Darkside, im Burst | dieselben drei Lagen, Rücklage für The Blackest Night | deckt sich (A197) |
| Dunkelritter | Bloodspiller, Shadowbringer | Blut nur im Burst oder gegen Überlauf; beide Ladungen Shadowbringer ins Zwei-Minuten-Fenster | Blut wie The Balance (A197); Shadowbringer eine Ladung an der Grenze, die letzte im Burst | deckt sich: Im 120-s-Takt ist die zweite Ladung genau zum Fenster zurück |
| Revolverklinge | No Mercy | jede Minute, Einwebposition je GCD-Geschwindigkeit | auf Stufe 100 mit liegendem Bloodfest; beide haben 60 s Abklingzeit (Job-Guide 7.5) | deckt sich im Takt; die Einwebposition (früh oder spät je Geschwindigkeit) ist nicht geprüft |
| Revolverklinge | Kartuschen | nicht überlaufen, keine Kombo mit voller Leiste abschließen | Burst Strike vor dem Abschluss bei voller Leiste (`IsAmmoCapped`) | deckt sich |

### Astrologe, Gelehrter, Weiser

Abgeglichen am 26.10.2026 gegen die Basic Guides von The Balance: Astrologe („Patch Applicable: 7.3", 07.08.2025),
Gelehrter (7.4, 27.12.2025), Weiser (7.3, 07.08.2025). Keine Änderung außer der Ladungsregel des Astrologen (oben,
„Weißmagier", A376).

| Job | Punkt | The Balance | RSR | Stand |
|---|---|---|---|---|
| Astrologe | Divination | auf Abklingzeit, auch in Dungeon-Pulls; Oracle darunter | mit automatischem Burst im Stand; Oracle, sobald bereit | deckt sich |
| Astrologe | Karten | The Balance und Lord of Crowns aus dem Ziehen der ersten Minute bis Divination halten | The Balance und The Spear halten, solange Divination in 66 s zurückkommt; Lord of Crowns, solange in 45 s, außer das nächste Ziehen steht an | deckt sich |
| Astrologe | Earthly Star | auf Abklingzeit, bei voller Stärke zünden | auf Abklingzeit (Option „Use Earthly Star as an attack while moving"), Zündung bei Giant Dominance | deckt sich |
| Astrologe | Celestial Intersection | Barriere auf ein Ziel | Abwehr- und Heilpfad, an der Ladungsgrenze auf den Angegriffenen (A376) | deckt sich |
| Gelehrter | Aetherflow, Dissipation | nicht mit Ladungen übrig | Aetherflow nur ohne Ladung (`ActionCheck`); Energy Drain leert sie zwei GCDs vor der Rückkehr | deckt sich |
| Gelehrter | Ruin II | nur für Bewegung, 25 % schwächer als Broil | nur nach einer Bewegungszeit ohne DoT-Ziel | deckt sich |
| Gelehrter | Heilordnung | frei vor Aetherflow vor GCD | Feenfähigkeiten und Aetherflow-Heilungen vor GCD-Heilungen; GCD-Heilungen nach „Use GCDs to heal" | deckt sich |
| Weiser | Addersgall | großzügig nutzen, nicht überlaufen | Druochole kurz vor der vierten Ladung (A203), Rhizomata bei höchstens einer | deckt sich |
| Weiser | Kardia, Psyche | Kardia immer auf jemandem; Psyche auf Abklingzeit | beides so | deckt sich |
| Weiser | Phlegma | Ladungen in die Buffs der Gruppe nach der zweiten Minute | eine Ladung vor dem Überlauf oder in Bewegung | weicht ab: Der Weise hat keinen eigenen Gruppenbuff; das Fenster der anderen ist eine Annahme über sie, kein tragender Grund (seine Regel) |
| alle | Swiftcast | für Wiederbelebung aufheben | Swiftcast bleibt für Wiederbelebungen (seine Regel) | deckt sich |

### Samurai: Meikyo Shisui und Higanbana

**Keine Änderung** (26.10.2026, A381). The Balance (Samurai Basic Guide, „Patch Applicable: 7.4"; Opener-Bild „Dawntrail
Patch 7.05") nutzt Meikyo Shisui im Fenster jeder Minute, um Higanbana „right on time" zu setzen – Iaijutsu verbraucht
keinen Meikyo-Stapel –, und drückt Meikyo nach seiner „15 second rule", wenn Higanbana ausläuft, während die Sen noch
aufgebaut werden. `SAM_Reborn` macht es umgekehrt: Meikyo wartet, solange Higanbana in sein Erneuerungsfenster fällt
(ab Werk 6 GCDs), und Higanbana fällt nur ohne Meikyo-Stapel. Meikyo geht sonst nach jedem Midare oder Tendo, solange eine
Ladung da ist, und an der vollen Ladung.

- *Was es im Kampf ergibt* (`samurai_balance_model.py`, ein Boss, zehn Minuten, Potenzen Job-Guide 7.5): Mit der Regel
  von `SAM_Reborn` liegt Higanbana 93,7 % der Zeit (die ersten 15 s eingerechnet), 15,8 s werden überschrieben; keine
  Meikyo-Ladung verfällt, kein Sen doppelt, 12 Tendo und 17 Midare. Mit der Folge von The Balance, so übernommen, wie
  `SAM_Reborn` sie tragen könnte: 97,3 %, aber 79,2 s überschrieben, gleich viele Tendo und Midare. Über 60 Kombinationen
  aus GCD-Länge (2,10 bis 2,20 s), Erneuerungsfenster (2, 4, 6 GCDs) und Kampflänge (3 bis 15 min) bringt sie im Mittel
  +0,29 % Potenz, zwischen −0,59 % und +1,60 %, in 4 Fällen weniger.
- *Warum nicht gebaut:* Der Gewinn liegt im Promillebereich und unter der Unschärfe des Modells (Positionale stets
  getroffen, keine Kritischen, keine Fähigkeiten außer Meikyo und Ikishoten); sein Vorzeichen wechselt mit GCD-Länge und
  Kampflänge. Was The Balance darüber hinaus trägt – Higanbana „buffed", in den Gruppenbuffs der anderen –, ist nach
  seiner Regel kein tragender Grund.
- *Opener:* The Balance setzt den zweiten Meikyo gleich nach dem ersten Tendo Kaeshi und Higanbana in GCD 7;
  `SAM_Reborn` hält ihn, weil noch kein Higanbana liegt, und setzt Higanbana in GCD 8 (Modell: 15,2 s). Ikishoten fällt
  einen GCD später als im Bild – nach dem dritten statt dem zweiten GCD, weil `SAM_Reborn` zwei GCDs Kampfzeit abwartet
  (`CombatElapsedLessGCD(2)`) –, der Trank danach statt nach dem ersten GCD; Senei, Ogi Namikiri und Zanshin folgen in
  derselben Reihenfolge, alles unter Fugetsu. Die Verschiebung ändert den eigenen Schaden nicht, nur die Lage zu den
  Gruppenbuffs der anderen.

### Barde: Lieder und Burst

**Deckt sich** (26.10.2026, A382) mit The Balance, Bard Basic Guide („Patch Applicable: 7.4", zuletzt geändert
27.12.2025). Geändert ist nur, woher die Länge eines Liedes kommt.

| Punkt | The Balance | RSR | Stand |
|---|---|---|---|
| Liedfolge | WM → MB → AP; „~43 seconds" in Wanderer's Minuet und Mage's Ballad, „~34 seconds" in Army's Paeon, Lieder bis 45 s | erstes bis drittes Lied als Einstellung, ab Werk dieselbe Folge; 43/43/34 s als Einstellung | deckt sich. Die 45 s stand als feste Zahl im Code; jetzt kommt die Dauer aus dem Tooltip des Liedes, aus der erzeugten Tabelle, wo er keine nennt (`BardRotation.SongDuration`) |
| Pitch Perfect | in Wanderer's Minuet bei drei Stapeln, früher nur gegen den Überlauf von Empyreal Arrow; Rest vor dem Liedwechsel ausgeben | bei drei; bei zwei, wenn Empyreal Arrow im nächsten GCD bereit wird und Radiant Finale abkühlt; Rest, wenn das Lied in 3 s endet | deckt sich |
| Iron Jaws | „with 4-7 seconds left" | in den letzten zwei GCDs der DoTs (Erneuerungsfenster ab Werk), dazu am Ende der eigenen Buffs | deckt sich |
| Apex Arrow | „around halfway through Mage's Ballad … by about 21-19s on the timer", früher bei 100 Soul Voice | in Mage's Ballad ab 80 Soul Voice, sobald höchstens 18 s übrig sind; bei 100 sofort außerhalb von Raging Strikes | 1 bis 3 s später als der Guide; die Bedingung `SongEndAfter(22) && SongEndAfter(18)` enthält die 22 nur scheinbar – ein Fenster 22–18 war wohl gemeint. Im Kampf ohne Belang: Der Pfeil fällt im selben Lied und vor demselben Burst; nicht geändert |
| Barrage, Empyreal Arrow, Sidewinder, Heartbreak Shot | Barrage auf Refulgent Arrow im Burst; Empyreal Arrow alle 15 s; Sidewinder etwa in der Mitte von Mage's Ballad; Heartbreak Shot nicht bei drei Ladungen | Barrage unter Raging Strikes; Empyreal Arrow auf Abklingzeit, kurz vor Raging Strikes zurückgehalten; Ladungsregeln der Rotation | deckt sich |

### Schadensausteiler: Ressourcen und Buffs

Geprüft am 26.10.2026 gegen die Basic Guides von The Balance (Stand je Guide: Dragoon 7.5, Viper 7.55, Reaper 7.5,
Samurai 7.4, Monk 7.4, Ninja 7.05, Barde 7.4, Tänzer 7.40, Schwarzmagier 7.4, Rotmagier 7.4, Pictomancer 7.4):
die Regeln gegen das Überlaufen der Ressourcen und für die eigenen Buffs, je an der Stelle im Code, die sie trägt.
Keine Änderung. Nicht einzeln geprüft sind die Folgen innerhalb eines Burstfensters (Mudra-Planung des Ninja vor
Kunai's Bane, Enshroud-Folgen des Reaper, Motivfolgen des Pictomancer, Feuer-Eis-Takt des Schwarzmagiers, Monk); das
bleibt offene Arbeit. Die Meikyo-Folgen des Samurai stehen unter „Samurai: Meikyo Shisui und Higanbana", die Liedfolge
des Barden unter „Barde: Lieder und Burst".

| Job | The Balance | RSR | Stand |
|---|---|---|---|
| Ninja | Ninki nie überlaufen lassen; Kazematoi nie überlaufen | Ausgabe bei 100 Ninki; Armor Crush nur unter 4 Kazematoi | deckt sich |
| Reaper | Soul Slice ohne Überlauf; Gluttony mit beiden Executioner; Arcane Circle auf Abklingzeit | Soul Slice bis 50 Soul, Gluttony ab 50 (`ActionCheck`); Arcane Circle ab 3,5 s Kampfzeit | deckt sich |
| Viper | Vicewinder vor der zweiten Ladung; eine Rattling Coil zurück, vor Serpent's Ire ausgeben | Vicewinder mit allen Ladungen; Uncoiled Fury am Höchststand oder an seiner Einstellung, vor dem Tod des Ziels | deckt sich |
| Samurai | Kenki und Meditation nicht überlaufen | Shoha bei voller Meditation; Kenki-Ausgaben ab 25 bzw. 50 | deckt sich |
| Dragoon | High Jump alle 30 s, Wyrmwind Thrust sobald möglich, Battle Litany auf das Zwei-Minuten-Fenster | auf Abklingzeit; Opener-Abgleich A343 | deckt sich |
| Tänzer | drei Federn für das Zwei-Minuten-Fenster, die vierte ausgeben; Esprit ins Fenster tragen; Standard Step vor Technical Step halten | Fächertanz mit mehr als drei Federn oder unter Devilment; Saber Dance im Burst oder ab 70 Esprit; Standard Step wartet, wenn Technical Step in 5 s kommt | deckt sich |
| Rotmagier | Embolden und Manafication auf Abklingzeit; Prefulgence unter Buffs; Fleche, Contre Sixte, Engagement, Corps-a-corps auf Abklingzeit; Swiftcast für Verthunder, Veraero, Impact oder Verraise | Embolden und Manafication auf Abklingzeit; Prefulgence unter Embolden oder vor dem Ablauf; je eine Ladung Engagement und Corps-a-corps für Embolden; Swiftcast auf Verthunder und Veraero | deckt sich; Swiftcast in der Rotation verletzt seine Regel nicht: Der Rotmagier belebt mit Dualcast wieder (Konzept 11) |
| Schwarzmagier | Polyglot nicht überlaufen | Xenoglossy bei vollem Polyglot, bevor Enochian endet oder Amplifier zurückkommt | deckt sich |
| Barde, Pictomancer | Bloodletter nicht bei drei Ladungen; Musen nicht überlaufen | Ladungsregeln der Rotationen | deckt sich, nicht vertieft |
