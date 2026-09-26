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

**Jede Kampfaktion, die ein Spieler auslösen kann und die ohne Sonderlage Schaden, Heilung oder
Schutz bringt, hat in den Standardrotationen einen Aufruf.** Ob dieser Aufruf erreicht wird — hinter
welcher Einstellung, auf welcher Stufe, in welcher Lage —, sagt die Matrix nicht (Grenzen). Was
maschinell als „ungenutzt" erscheint, zerfällt in sechs Klassen (Stand 26.09.2026):

| Klasse | Aktionen | Warum nicht genutzt |
|---|---|---|
| Behälter | Gemshine, Precious Brilliance, Astral Flow (SMN) · Play I–III, Minor Arcana (AST) · Serpent's Tail, Twinfang, Twinblood (VPR) · Riposte, Zwerchhau, Redoublement, Moulinet, Reprise (RDM) · Motive und Musen (PCT) · Iaijutsu, Tsubame-gaeshi (SAM) · Masterful Blitz (MNK) · Ninjutsu (NIN) · Continuation (GNB) | Der Knopf wird zu einer anderen Aktion; RSR ruft die Zielaktion, und das Spiel führt die angepasste Id aus (`BaseAction.Use`, `AdjustedID`) |
| Begleiter und Automatik | Akh Morn, Revelation, Exodus, Wyrmwave, Scarlet Flame, Luxwave, Everlasting Flight (SMN) · Embrace, Seraphic Veil (SCH) · Arm Punch, Roller Dash, Pile Bunker, Crowned Collider, Rook Overload (MCH) · Hollow Nozuchi (NIN) | Wirktext: „cannot be assigned to a hotbar"; der Begleiter oder ein Auslöser führt sie aus |
| Limit Breaks | je Job drei | ohne Wirktext im Datensatz; RSR castet keine PvE-Limit-Breaks (Konzept 05) |
| Hilfsaktionen | Sleep, Repose, Rescue, Leg Graze, Foot Graze, das Ablegen der Tankhaltung (Release …), Dissolve Union, Ending | Sie wirken auf Mitspieler oder die Gruppenlage (Rescue zieht einen Spieler, das Ablegen der Haltung gibt die Feindseligkeit ab, Schlaf bricht beim ersten Treffer). Nicht automatisiert — Schluss aus der Wirkung, kein Beleg für eine Absicht |
| **Knopfwechsel gesperrt** | Improvised Finish (DNC) · Detonator (MCH) | Die Basisaktion führt den Status, den die Zielaktion braucht, als `StatusProvide` und verweigert sich, solange der Knopf gewechselt hat — außer in den letzten `StatusRefreshGcdCount` GCDs des Status (ab Werk 2) oder mit ausgeschaltetem `ShouldCheckStatus`. Detonator: seit A164 schließt `WildfirePvE` den gewechselten Knopf auch dort aus; Wildfire zündet mit allen Stapeln von selbst. Improvised Finish: Defekt, siehe „Wechselwirkungen und Zeit" |
| **Ohne belegten Nutzen** | Six-sided Star (MNK) · Flamethrower (MCH) | Der Vorteil ist aus den Wirktexten nicht rechenbar — siehe „Pausen und Phasenenden". Meditate (SAM) und Rook/Queen Overdrive (MCH) wirkt die Rotation seit A162 in der Pause |

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

## Wechselwirkungen und Zeit

**Auftrag (seine Vorgabe):** „prüfe alle erfassten fähigkeiten der einzelen jobs auf vollständigkeit und
vollständiger beschreibung der fähigkeiten. prüfe wechselwirkungen mit anderen fähigkeiten.
nebenprüfung: schränkt die nutzung von defensive skills die offensiven skills ein oder hebt diese auf?
oder werden sie defensive skills durch die nutzung von offensive skills aufgehoben. […] evtl. ist bei
einigen ein quasi-perpetum-mobile möglich zu erschaffen oder positive effekte extrem auszureizen."
Dazu seine Präzisierung: Eine Sicherheitsbewertung braucht eine Wahrscheinlichkeit (CLAUDE.md).

**Sachstand (A163):** Der Generator liest aus den Wirktexten, was eine Aktion zeitlich und im
Zusammenspiel tut, und schreibt es je Job in den Abschnitt „Wechselwirkungen und Zeit" der Matrix.
Die Befunde unten sind daraus und aus dem Code erhoben; wo eine Spielmechanik nicht im Text steht, ist
sie als Schluss markiert.

### Wo Angriff die Abwehr beendet: die Kanäle

Drei Schutzaktionen sind Kanäle — „Effect ends upon using another action or moving". Jede weitere
Aktion beendet sie, auch ein Angriff. RSR wirkt nach jeder Aktion sofort die nächste; ohne Sperre
beendet es seinen eigenen Kanal also mit dem nächsten GCD oder der nächsten Fähigkeit.

| Aktion | Was der Kanal trägt (Wirktext) | Was nach dem Abbruch bleibt | RSR-Sperre (Voreinstellung) |
|---|---|---|---|
| Passage of Arms (Paladin) | Blockrate 100 %, Gruppe im Kegel hinter ihm nimmt 85 % Schaden, 18 s | nichts | `PldlockCasting` (aus): hält GCD und Fähigkeiten, solange der angekündigte Treffer aussteht (`DataCenter.AreaHitPending`). `PosPassageOfArms` (aus) sperrt Bewegung, nur mit `PoslockCasting` (aus) |
| Collective Unconscious (Astrologe) | Ring 18 s, darin Wheel of Fortune (Regen) fortlaufend | Minderung −10 % für 10 s, als Zusatzeffekt beim Wirken vergeben (Schluss aus dem Textaufbau, Status 849 ist eigener Status) | `AstlockCasting` (aus), gebaut wie beim Paladin |
| Improvisation (Tänzer) | Stapel Rising Rhythm alle 3 s bis 4; Regen 15 s | Regen (Schluss aus dem Textaufbau) | keine Aktionssperre; Bewegungssperre `PosImprovisation` (aus), nur mit `PoslockCasting` (aus) |

**Im Kampf, mit Voreinstellung:**
- **Paladin:** RSR wirkt Passage of Arms nur, wenn ein Flächentreffer angekündigt ist (Zauberleiste
  oder BossMod-Raidwide im Fenster). Landet der Treffer nach RSRs nächster Aktion — spätestens nach
  einem GCD —, schützt die Aktion niemanden, und ihre Abklingzeit ist verbraucht.
- **Mit Sperre:** Der Treffer ist angekündigt, die Wahrscheinlichkeit also hoch; die Sperre kostet
  GCDs bis zum Treffer. Das trägt seine Präzisierung. Beide Pfade halten, solange der Treffer aussteht
  (`DataCenter.AreaHitPending`: das Flächensignal oder ein BossMod-Raidwide im Fenster, auch in den
  letzten 0,6 s, in denen das Signal schon losgelassen hat); danach beendet die nächste Aktion den
  Kanal. So verlangt es der Einstellungstext („during AOE mitigations"). Bis A164 hielt der GCD-Pfad
  ohne diese Frage, bis eine Fähigkeit den Kanal beendete — bis zu 18 s ohne GCD, der Fall seines
  Extrembeispiels.
- **Astrologe:** Die Minderung bleibt nach dem Abbruch; der Kanal verlängert nur das Regen. Die Sperre
  hält dafür bis zum Treffer auch die GCD-Heilungen des Astrologen zurück. Ohne Sperre verliert er
  wenig.
- **Tänzer:** Improvised Finish — die Barriere, 5 % bei 0 bis 10 % bei 4 Stapeln — wirkt RSR nie:
  `ImprovisationPvE` führt `Improvisation` als `StatusProvide` und verweigert sich deshalb, solange der
  Knopf Improvised Finish ist; ein eigener Aufruf fehlt. Die nächste Aktion beendet den Tanz, und die
  Barriere verfällt. Im Kampf: Der Tänzer gibt für seine Zwei-Minuten-Gruppenaktion nur das Regen.

**Umgekehrt, Abwehr beendet Angriff:** Flamethrower (Machinist) ist ebenfalls ein Kanal; RSR wirkt ihn
nicht. Meditate (Samurai) wirkt RSR nur in der Pause. Dort beendet ihn jede Abwehr oder Heilung, die
ohne Gegner in Reichweite fällt: die eigene Heilung (Second Wind, Bloodbath), Tengentsu und Third Eye
bei angekündigtem Treffer oder eigener niedriger Gesundheit. Sicherheit vor Kenki — so gewollt. Die
Rückhaltung von Abwehr im Burst steht in Konzept 08, „Die Abwehrsperren".

### Wo eine Aktion einen Status aufhebt

| Aktion | hebt auf | Bewertung |
|---|---|---|
| Shake It Off (Krieger) | Thrill of Battle, Damnation, Bloodwhetting | **Befund.** Der Wirktext im Repository lautet „Dispels Thrill of Battle and increasing…" — ein stufenabhängiger Name ist ausgeblendet. Vollständig, nach dem Suchauszug der Wikis (die Seiten selbst sperrt der Egress): „Dispels Thrill of Battle, Damnation, and Bloodwhetting, increasing damage absorbed by 2% for each effect removed". Unter Stufe 92 und 82 stehen dafür vermutlich Vengeance und Raw Intuition (Schluss aus den Ausbauketten). Was verloren geht: Damnation −40 % Schaden für 15 s, Bloodwhetting −10 % mit Heilung je Waffenfertigkeit, Thrill of Battle +20 % Maximalgesundheit und +20 % erhaltene Heilung (Enhanced Thrill of Battle) — gegen +2 % Barriere je Effekt. RSR wirkt Shake It Off als Flächenabwehr und als Einzelheilung, ohne einen dieser Status zu prüfen. Im Kampf: Ein angekündigter Raidwide, während Damnation für einen Tankbuster liegt, nimmt dem Krieger 40 % Minderung vor dem Tankbuster |
| Tempera Grassa (Maler) | Tempera Coat | Zweck der Aktion; RSR wandelt nur bei angekündigtem Flächentreffer oder kurz vor Ablauf |
| Meisui (Ninja) | Shadow Walker | Zweck der Aktion. RSR wirkt es, während Trick Attack (Kunai's Bane) abkühlt, und zusätzlich (a) wenn Ten Chi Jin bereit ist — dessen Abfolge endet mit Suiton, das Shadow Walker neu gibt (Wirktext) —, (b) wenn Shadow Walker in zwei GCDs endet, oder (c) wenn Trick Attack nicht in 19 s bereit ist |
| Detonator (Machinist) | Wildfire | RSR wirkt ihn nie; Wildfire zündet mit allen Stapeln von selbst. Bis A164 konnte `WildfirePvE` in den letzten zwei GCDs von Wildfire als Detonator hinausgehen (gesperrter Knopfwechsel, siehe oben) und die übrigen Stapel abschneiden. Nicht gebaut: Detonator vor dem Tod des Ziels — ob die Ladung dann verfällt, steht in keiner Quelle |

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
`DefensiveValues` 0 s. Keine dieser Aktionen ist heute Auslöser einer Streckung; wäre sie es, würde
sie nie strecken — der Generator erfindet keinen Wert.

## Werden die Fenster genutzt?

**Auftrag (seine Vorgabe, 26.09.2026):** prüfen, ob die optimalen Kombinationen in den Rotationen
tatsächlich genutzt werden — „für alle im Loop".

**Sachstand (A165, A166):** Geprüft ist, ob ein gewährtes Fenster — ein Status, der eine Aktion für eine
Zeit freigibt (Proc, „Ready", Stapel), oder eine Ressource mit Obergrenze — verbraucht wird, bevor es
verfällt oder überläuft. Ob eine Abfolge die beste ist, ist nicht geprüft: Die Referenz dafür (Job-Guides
wie The Balance, das Rotations-Repository) sperrt der Egress, und die Wirktexte nennen die Dauer der
meisten „Ready"-Status nicht. Der Code braucht sie nicht: Er liest die Restzeit zur Laufzeit.

### Die gemeinsame Ursache: die Statusbedingung sperrte das Ende jedes Fensters

**Stufe „alle", behoben (A166).** Eine Aktion mit Statusbedingung (`StatusNeed`, auf der Zielseite
`TargetStatusNeed`) war gesperrt, sobald ihr Status in `StatusRefreshGcdCount` GCDs (ab Werk 2, rund 5 s)
endete. Die Einstellung heißt in der Oberfläche „Number of GCDs before the DOT/Status effect is
reapplied" und erscheint nur bei Aktionen, die einen Status vergeben; ihr Text bindet. Eingeführt hat
die Verwendung auf der Bedarfsseite Upstream-Commit `dc067e0d3` (April 2025, „for better status
determination"); vorher lag die Grenze bei 0.

- **Im Kampf:** Jeder Proc-Verbraucher aller Jobs war in den letzten ~5 s seines Fensters gesperrt. Wer
  ihn bis dahin nicht verbraucht hatte, verlor ihn. Jede Regel „vor Ablauf nutzen" mit kleinerem Horizont
  war wirkungslos (Rotmagier Prefulgence, Weißmagier Divine Caress, Gelehrter Baneful Impaction, Ninja
  Meisui, Maler Tempera Grassa).
- **Jetzt:** Der benötigte Status muss liegen, solange die Aktion wirkt — bei einem Zauber mit Wirkzeit
  bis zu deren Ende (`ActionBasicInfo.CastTime`, aus dem Spiel). Ein Soforteinsatz braucht nur den Status.
- **Betroffen:** alle Jobs (Endnutzer), jede abgeleitete Rotation mit `StatusNeed` (Paketnutzer),
  Upstream-Pflege.

### Rückfälle vor Ablauf, je Job (A165, A166)

Wartet ein Verbraucher auf eine Ausrichtung (Burst, Debuff), verfällt sein Fenster, wenn die Ausrichtung
nicht kommt. Behoben mit einem Rückfall im letzten GCD des Status; vorher bleibt jede Ausrichtung
unberührt.
- **Maschinist:** Hypercharged (Barrel Stabilizer, eine Überhitzung ohne Heat) wartete auf Wildfire.
  Wird Wildfire zurückgehalten — „Only use Wildfire on Boss targets" gegen Trash, oder vor einer Pause —,
  verfiel es. Der Rückfall fällt auch über einem Reassemble; das landet dann auf einem Blazing Shot, fünf
  freie Überhitzungs-Schüsse wiegen mehr. Gesperrt bleibt er während einer laufenden Überhitzung (die Basisrotation
  lässt Hypercharge dort nicht zu, `!IsOverheated`).
- **Samurai:** Ogi Namikiri wartete auf einem Boss auf Higanbana. Kommt Higanbana nicht — abgeschaltet,
  oder „Prevent Higanbana use if theres more than one target" (ab Werk an) sperrt es über
  `NumberOfAllHostilesInRange`, während der Flächenpfad von Ogi `NumberOfHostilesInRange` unter zwei
  sieht —, verfiel Ogi samt Kaeshi. Der Rückfall rechnet die Wirkzeit von Ogi mit ein.
- **Revolverklinge:** Sonic Break und Reign of Beasts fielen nur unter No Mercy. Vergeht No Mercy ohne
  Platz dafür (Pause), verfielen Ready to Break und Ready to Reign. Der Rückfall steht vor allen anderen
  GCDs, weil jeder andere einen GCD später noch kommen kann.
- **Ninja:** Phantom Kamaitachi wartete auf das Trick-Attack- oder Mug-Fenster; Bunshin läuft auf eigener
  Abklingzeit. Öffnet sich kein Fenster vor Ablauf, fällt es jetzt im letzten GCD.

**Zur Entscheidung (Weiser):** Addersgall läuft bei drei Stapeln über; einen Verbrauch vor dem
Überlauf gibt es nicht (der Weißmagier hat dafür „Use Lily at max stacks/about to overcap", ab Werk an).
Druochole gäbe je Stapel 7 % MP und eine Heilung (Wirktext).

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
  Ziel), Rotmagier (Verfire/Verstone nach Manabalance, Grand Impact, Enchanted Riposte nach Mana), Maler
  (Comet in Black, Star Prism, Subtractive Palette).
- *Die Bedingung ist der Zweck:* Heilbedarf (Horoscope, Pepsis), Option (Retrace), Burst (Radiant
  Encore, Reawaken, Starfall Dance, Technical Step, Flourish), Tanzschritte vor den Procs (Tänzer: Reverse
  Cascade, Fountainfall, Rising Windmill, Bloodshower warten, solange ein Schritt bereit ist; dass die
  Procs einen Tanz überdauern, ist ein Schluss — ihre Dauer blendet der Wirktext aus), Konzept 12 (Lux
  Solaris, Searing Flash, Ruin IV).
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
- *Fernkampf und Magie:* Barde, Tänzer, Maler, Rotmagier, Schwarzmagier (Polyglot vor dem Überlauf, beide
  Rotationen), Beschwörer (Konzept 12).

**Bewusst so, mit Grund:** *Barde:* Soul Voice bleibt bis zu 25 s bei 100, wenn Battle Voice kommt —
Apex Arrow im Burst ist mehr wert als der Überlauf (Schluss aus dem Regelaufbau).

**Nebenbefund, technische Schuld:** `UseBlood` im Dunkelritter hat keinen Leser (Blut für den Burst
aufsparen). Laut heutigem Wirktext kostet Living Shadow kein Blut mehr; ob Aufsparen für Delirium noch
etwas bringt, ist ohne Referenz nicht rechenbar.

## Pausen und Phasenenden

**Sachstand (A162):** Seine Angabe zum Profil — Machinist zwischendurch, alle anderen Kampfjobs
seltener — holt diese Jobs in die Bearbeitung.

**Die Pause selbst ist eine Regel für alle, was darin fällt, eine Regel des Jobs.**
- **Stufe „alle":** `CustomRotation.InCombatPause` — im Kampf, kein Gegner in 25 Yalm; nichts zu
  schlagen. Ohne BossModReborn lesbar. Die vorhergesagte Pause ist `BMRDowntimeWithin` (nur mit Modul).
- **Samurai:** Meditate in der Pause („Gradually increases your Kenki Gauge", nur im Kampf). Die
  Basiseinstellung verweigert es in Bewegung; es endet bei Bewegung.
- **Machinist:** Rook/Queen Overdrive im letzten GCD vor einer vorhergesagten Pause, wenn die Königin
  sonst erst in der Pause endet. Ihr Wirktext: Ungewirkt fällt der Abschluss „automatically immediately
  before shutting down" — in der Pause trifft er nichts. Nur mit Modul; ohne Vorhersage ist die Pause
  erst bekannt, wenn sie begonnen hat.

**Bestehendes Pausenverhalten bleibt, wie es ist,** weil es eine andere Frage beantwortet: Der Monk lädt
schon Chakra, wenn kein Gegner in **seiner** Reichweite steht (3 Yalm, auch wenn er nur für eine Mechanik
herausläuft); der Machinist verschießt Heat vor einer vorhergesagten Pause (`BmrDumpBeforeDowntime`); der
Schnitter wirkt Soulsow.

**Nicht gebaut, mit Grund:**
- *Six-sided Star (Monk):* Die Grundpotenz blendet der Wirktext aus. Ob Chakra über eine Pause
  verfällt, steht in keiner Quelle im Repository. Ein Vorteil gegenüber dem Aufsparen ist nicht belegt.
- *Flamethrower (Machinist):* keine Pausenaktion, sondern ein Flächenkanal. Wie oft er tickt, steht
  nicht im Wirktext; ein Vergleich mit dem Flächenfüller ist deshalb nicht rechenbar.

**Folgen, bewusst hingenommen:**
- Läuft der Samurai im Kampf aus 25 Yalm heraus, ohne dass eine Pause ist, fällt Meditate, sobald er
  steht. Es bricht bei der ersten Bewegung und kostet dann einen GCD-Takt.
- Liegt die vorhergesagte Pause später als gemeldet, verliert die Königin ihre letzten Angriffe
  zugunsten des Abschlusses.
