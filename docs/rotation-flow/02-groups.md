# 02 · Ablaufstruktur je Jobgruppe

Aufbauend auf `01-jobs.md`. Diese Ebene zeigt pro Gruppe, was **gleich**,
**ähnlich** und **unterschiedlich** ist. Hooks, Optionen und Voreinstellungen am 02.10.2026 aus dem Code gemessen. Farbcode in allen Diagrammen:

- durchgezogen = bei jedem Job der Gruppe vorhanden
- gestrichelt = bei einigen Jobs der Gruppe vorhanden
- Raute = Verzweigung, an der sich die Jobs unterscheiden

---

## Heiler (WHM · AST · SGE · SCH)

```mermaid
flowchart TD
    A[GeneralGCD] --> B{Raise ansteht<br/>und Swiftcast bereit?}
    B -- ja --> C[Kurzschluss: base / RaiseGCD]
    B -- nein --> D{Sustain-Zweig<br/>vorhanden?}
    D -- "WHM AST" --> E[TrySustain…OnTank]
    D -- "SGE SCH" --> F[kein Sustain-Zweig]
    E --> G[Job-Ressource]
    F --> G
    G --> H[AoE-Zweig]
    H --> I[DoT-Kette]
    I --> J[Filler-Level-Kette]
    J --> K[base.GeneralGCD]

    style C fill:#4a3,color:#fff
    style E fill:#36c,color:#fff
    style F fill:#a33,color:#fff
```

### Identisch in allen vier

1. **Raise-Kurzschluss ganz oben.** `SwiftRaisePending`, je Datei privat und gleichlautend (`SwiftLogic` ist eine Einstellung
   jeder Rotation), 13 Verwendungen (16.11.2026)
   (jeder Heiler hat ihn zusätzlich in `HealSingleGCD` und `HealAreaGCD`). Der einzige Zweig, der die Methode verlässt, bevor
   irgendeine Rotationsentscheidung fällt.
2. **Reihenfolge AoE → DoT → Filler.** Ohne Ausnahme.
3. **Filler ist immer eine Level-Kette** (Glare/Malefic/Dosis/Broil).
4. **`CanHealSingleSpell`/`CanHealAreaSpell`** lesen dieselbe Methode, `base && GcdHealsAllowed(GCDHeal)`: Mit
   „Use GCDs to heal" aus gehen GCD-Heilungen, wenn kein anderer Heiler lebt oder ein Mitglied in der kritischen Klasse
   steht – die universelle Schicht, der jede strategische Rückhaltung weicht (Konzept 08, A360). Paladin, Rotmagier und
   Beschwörer lesen sie ebenso; bis A360 trugen alle sieben je eine wortgleiche Kopie ohne die Gefahrenausnahme.

### Ähnlich, aber abweichend

| | WHM | AST | SGE | SCH |
|---|---|---|---|---|
| Kurzschlüsse | 1 (Swift; Thin Air entfernt, A401) | 1 | 1 | 1 |
| Sustain-Zweig in `GeneralGCD` | Regen | Aspected Benefic | **keiner** (entfernt, A5) | **keiner** |
| Pflege auf dem Tank im Pull (`TryPullUpkeepOnTank`) | Regen | Aspected Benefic | Eukr. Diagnosis | **nicht gebaut** |
| HP-Schwelle der Pflege | `RegenHeal` 0.3 | `AspectedBeneficHeal` 0.4 | keine (Konzept 15) | – |
| `GCDHeal`-Default | **true** | **true** (A433) | false | **true** |
| Ressourcenlogik in GeneralGCD | Lily | **keine** | Phlegma | MP-Schwelle |
| Zweistufiger Cast | nein | nein | **ja** (Eukrasia) | nein |
| Pet-Verwaltung | nein | nein | nein | **ja** (Eos) |

### Wo die Gruppe wirklich auseinanderläuft

Nur an drei Stellen: **Eukrasia-Vorstufe** (SGE), **Pet** (SCH) und
**Kartenlogik** (AST, komplett außerhalb `GeneralGCD`). Alles andere ist
dieselbe Leiter mit anderen Aktionsnamen.

---

## Tanks (PLD · WAR · DRK · GNB)

```mermaid
flowchart TD
    A[GeneralGCD] --> B[Ressource: Kartuschen / Blut / Beast Gauge]
    B --> C{Gegneranzahl}
    C -- "≥ AoeCount" --> D[AoE-Combo]
    C -- "sonst" --> E[ST-Combo]
    D --> F[Ranged-Pull-Fallback]
    E --> F
    F --> G[base.GeneralGCD]

    H[DefenseAreaAbility] --> J[Reprisal-Sustain]
    L[DefenseSingleAbility] --> J

    style J fill:#36c,color:#fff
```

### Identisch in allen vier

1. **Ressource → AoE/ST-Verzweigung → Combo → Ranged-Fallback.** Gleiche
   Makrostruktur, gleiche Reihenfolge.
2. **Ranged-Pull-Fallback am Ende von `GeneralGCD`**, direkt vor `base`, nur
   durch das eigene `CanUse` gegated: Tomahawk (WAR), Lightning Shot (GNB),
   Shield Lob (PLD), Unmend (DRK). Vier strukturgleiche Zeilen.
3. **`AoeCount = 2`** für die AoE-Aggro-Aktion — bei allen vier explizit
   überschrieben (globaler Default wäre 3).
4. **Kein Raise; GCD-Heilung nur beim Paladin** (Clemency, `HealSingleGCD`).
5. **Reflexion in Flächen- und Einzelabwehr** über `ShouldSustainMitigationDebuff` (00bc9c6f), am Tankbuster mit
   `HoldReprisalForRaidwide` (A244).
6. **`HasOwnArmsLengthPullRule`**: Abtausch im Pull nach der eigenen Regel, nie auf einen Boss (A236).

### Wo die Gruppe auseinanderläuft

| | PLD | WAR | DRK | GNB |
|---|---|---|---|---|
| `EmergencyAbility` | ✓ Reborn | ✓ Basisschicht (Holmgang) | ✓ Basis + Reborn | ✓ Basis + Reborn |
| `HealSingleGCD` | ✓ Clemency | – | – | – |
| `HealSingleAbility` | – | ✓ | ✓ | ✓ |
| `MyInterruptGCD` | ✓ | – | – | – |
| Configs | 16 | 17 | 12 | **3** |

### Ausgaben in der Fläche erst, wo sie mindestens so viel bringen

**Sachstand:** Die Ausgabe-Aktionen von Krieger und Dunkelritter gehen in die Fläche erst, wo sie auf den Gegnern in
ihrer Reichweite mindestens so viel Potenz bringen wie die Einzelversion auf einem – nach den Wirktexten des laufenden
Patches (`CustomRotation.AreaWorthOver`, `ActionTooltip.AreaAtLeastSingle`; A473). Die Gegnerzahl der Aktion bleibt
die Untergrenze („Number of targets needed to use this action"). Bei Gleichstand geht die Fläche: gleicher Schaden,
Feindseligkeit auf allen. Die Flächen-Combo (Overpower, Unleash, Total Eclipse, Demon Slice) bleibt bei 2 Gegnern für
die Feindseligkeit (oben, „Identisch in allen vier").

| Paar (Wirktext, Patch 7.5) | Lohnt ab | RSR vorher |
|---|---|---|
| Krieger: Decimate 180 gegen Fell Cleave 580 | 4 | 2 |
| Krieger: Chaotic Cyclone 200 gegen Inner Chaos 700 (beide kritisch direkt) | 4 | 2 |
| Krieger: Orogeny 150 gegen Upheaval 420 | 3 | 2 |
| Krieger: Steel Cyclone 160 gegen Inner Beast 330 (unter Stufe 60) | 3 | 2 |
| Dunkelritter: Quietus 240 gegen Bloodspiller 600 | 3 | 2 |
| Dunkelritter: Flood of Shadow 160 gegen Edge of Shadow 460 (darunter Flood/Edge of Darkness 100/300) | 3 | 2 |
| Dunkelritter: Impalement 300 gegen Torcleaver 820 (der stärkste Schritt der Delirium-Combo) | 3 | 2 |

The Balance stimmt für den Krieger überein (Decimate und Chaotic Cyclone ab 4, Orogeny ab 3; Stand des Guides 7.4,
27.12.2025, an 7.5 nicht angepasst – die Rechnung aus den Wirktexten von 7.5 kommt auf dieselben Zahlen). **Im Kampf:**
Bei zwei oder drei Gegnern ging ein Teil jedes Inner Release und jeder Blut-Ausgabe in die Fläche und verlor Schaden –
bei zwei Gegnern Decimate 360 statt Fell Cleave 580, Chaotic Cyclone 400 statt Inner Chaos 700. Die Feindseligkeit auf
dem zweiten und dritten Gegner hält die Flächen-Combo. Ab der Zahl der Tabelle bleibt alles, wie es war.

**Nicht geändert:** Paladin – Holy Circle und Holy Spirit sind unter Divine Might (250/500) und Requiescat (350/700),
wo sie in der Rotation fallen, bei zwei Gegnern gleich stark; die Revolverklinge liegt seit A471 richtig (Fated Circle
300 gegen Burst Strike 420, ab 2). Die Abyssal Drain des Dunkelritters vergleicht schon so (A391).

### Feindseligkeit im Pull: die Revolverklinge gibt ihre Patronen in der Fläche aus

**Sachstand:** Im Pull wirkt die Revolverklinge ihre Einzelzielaktionen nur noch, wo die Fläche nicht mehr bringt
(A471): Eine Patrone geht auf Fated Circle statt Burst Strike, sobald dessen Gegnerzahl erreicht ist (ab Werk 2);
Gnashing Fang beginnt nicht, wenn Fated Circle drei oder mehr Gegner erreicht; Sonic Break fällt nicht bei sechs und
mehr, bei vier und fünf nur, wenn das Ziel nach seiner gemessenen Restzeit die ganze Wirkdauer des DoT lebt, bei drei
die halbe. So steht es in der Referenz (The Balance, Gunbreaker Basic Guide, abgerufen 31.10.2026: „Use Fated Circle
on 2 targets or more", „Stop using Gnashing Fang on 3 or more targets", „Use Sonic Break on up to 5 targets", bei 4
Zielen nur mit voller, bei 3 mit halber Wirkdauer).

**Der Hinweis.** Ein Bekannter, von ihm weitergegeben (31.10.2026): Unter RSR (Upstream) erzeuge die Revolverklinge
bei Flächen und bei einzelnen Gegnern weniger Feindseligkeit als die anderen Tanks. Sein Zusatz, ebenfalls ein Hinweis
(31.10.2026): „gerade bei wall to wall pulls, wenn die gunblade in die gegnergruppe rennt, dauert es gefühlt länger,
dass sie bei allen gegnern aggro hat, als bei den anderen tankklassen. und beim weiterziehen zur nächsten gegnergruppe
scheint sie schneller wieder die aggro zu verlieren, wenn die damagedealer beim laufen schaden auf die gegner machen."

**Ursache am Code.** `GNB_Reborn.GeneralGCD` prüfte Burst Strike, die Gnashing-Fang-Kette (Gnashing Fang, Savage Claw,
Wicked Talon) und Sonic Break vor Fated Circle und der Flächen-Combo, ohne die Gegner zu zählen. Unter No Mercy – im
Pull fast immer gleich zu Beginn – gingen so etwa fünf bis sechs von acht GCDs auf einen einzigen Gegner (Schluss aus
der Reihenfolge, nicht gemessen); die übrigen bekamen Feindseligkeit nur aus Double Down, Bow Shock und den
Rand-Treffern von Reign of Beasts. Eine Aktion auf ein Ziel erzeugt nur bei diesem Ziel Feindseligkeit. Die anderen
Tanks prüfen ihre Flächenversion zuerst: Krieger Chaotic Cyclone und Decimate vor Inner Chaos und Fell Cleave,
Mythril Tempest vor Storm's Path; Paladin Holy Circle vor Holy Spirit, Prominence vor Royal Authority (seine
Atonement-Kette entsteht nur aus Royal Authority, die er im Pull nicht wirkt); Dunkelritter Quietus vor Bloodspiller,
Impalement vor Scarlet Delirium, Stalwart Soul vor Souleater. Die Revolverklinge war der Einzelfall der Gruppe, deshalb
sitzt die Korrektur in ihrer Rotation, nicht in der Tankschicht.

**Im Kampf.** Im Wall-to-wall-Pull trifft jede Patrone jetzt alle Gegner im Umkreis (Fated Circle und sein Folgeangriff
Fated Brand), und ohne Gnashing-Fang-Kette bleiben die GCDs auf der Flächen-Combo: Die Gegner, an denen die Damage
Dealer ziehen, bekommen von der Revolverklinge laufend Feindseligkeit, beim Hineinlaufen wie beim Weiterziehen. Gegen
zwei Gegner bleibt Gnashing Fang; gegen einen ändert sich nichts. Schaden: nach der Referenz die bessere Wahl, also kein
Preis. *Schwäche:* Die Gegner werden über Fated Circles Reichweite um die Revolverklinge gezählt; ein Rudel, das noch
heranläuft, zählt erst, wenn es dort steht – dann beginnt Gnashing Fang womöglich auf dem ersten Gegner und läuft zu
Ende. Gezählt wird über Demon Slice; ist es in der Aktionsliste abgeschaltet oder steht „AoE usage“ auf Off, zählt die
Rotation keinen Gegner und wirkt wie vorher. *Bedrohung:* Wer die Gegnerzahl von Fated Circle hochsetzt, bekommt Burst
Strike länger, wie er es einstellt. Ohne Fated Circle (unter Stufe 72) gilt die alte Reihenfolge.

Die frühere Spaltung bei Reflexion (Paladin und Krieger nur in der Einzelabwehr) ist aufgehoben (00bc9c6f).
`HasHostileCountAoeMitigation` gibt es nicht mehr: Es setzte `DefenseArea` allein auf die Gegnerzahl und
öffnete damit die ganze Abwehrkette (b8018cf0, Konzept 08).

---

## Melee (DRG · MNK · NIN · RPR · SAM · VPR)

```mermaid
flowchart TD
    A[GeneralGCD] --> B{Combo-Ablauf<br/>gefährdet?}
    B -- "RPR" --> C[Combo-Rettung]
    B -- nein --> D[Burst-Fenster]
    C --> D
    D --> E{Combo-Darstellung}
    E -- "DRG SAM NIN RPR" --> F[if-Kette]
    E -- "VPR" --> G[switch über Status-Tupel]
    E -- "MNK" --> H[Formen-Automat]
    F --> I[AoE-Zweig]
    G --> I
    H --> I
    I --> J[Ranged-Fallback]
    J --> K[base.GeneralGCD]

    style G fill:#4a3,color:#fff
    style H fill:#4a3,color:#fff
```

### Identisch in allen sechs

1. **Feint-Sustain** über `ShouldSustainMitigationDebuff(StatusID.Feint)` in
   `DefenseAreaAbility` **und** `DefenseSingleAbility` — eine Zeile pro Stelle, zwölf Stellen gesamt; beim Ninja
   in der versiegelten `NinjaRotation`.
2. **Ranged-Fallback am Ende** (Piercing Talon, Writhing Snap, Harpe …); der Mönch hat keinen.
3. **`HealSingleAbility`** (Second Wind/Bloodbath) bei allen sechs.

### Wo die Gruppe auseinanderläuft

| | DRG | MNK | NIN | RPR | SAM | VPR |
|---|---|---|---|---|---|---|
| `CountDownAction` | ✓ (Sprung zum Ziel) | ✓ (mit Sprung zum Ziel) | ✓ | ✓ | ✓ | ✓ (Sprung zum Ziel) |
| `EmergencyAbility` | ✓ | ✓ | ✓ | **fehlt** | **fehlt** | ✓ |
| `HealAreaAbility` | – | **✓** (Earth's Reply, Mantra) | – | – | – | – |
| `HasOwnInterruptGate` | – | – | – | ✓ | – | ✓ |
| Combo-Form | if-Kette | Automat | Automat | if-Kette | if-Kette | switch |
| Trait-Duplikate (`if (X.EnoughLevel)` und `if (!X.EnoughLevel)`, 16.11.2026) | **7 Paare** | – | – | – | – | – |

Der Mönch führt Second Wind und Bloodbath seit 4b3c9412 in `HealSingleAbility`; `HealAreaAbility` trägt seine
Gruppenheilungen.

### Positionals: wo stehen, wer bewegt, wann True North

Geltender Stand (A819, A926): RSR wählt nach Stellung, wirft True North, wenn der nächste GCD sein Positional verfehlen
würde oder der Spieler so knapp an der Grenze steht, dass das Spiel schon einmal anders geurteilt hat, und meldet die
gebrauchte Seite nur, wo ein Schritt etwas bringt. Bewegen tut BossModReborn: mit seiner KI, sonst schaltet RSR
sein Modul „Automatic movement“ für das Fenster der Seite ein („Bewegung in die Seite über BossMods Bewegungsmodul“).

**Grundlagen**
- *Geometrie:* Um die Mitte des Ziels liegen vier Sektoren zu 90°, bezogen auf seine Blickrichtung: vorn, zwei
  Flanken, hinten. Die Grenze hinten/Flanke liegt 45° neben der Rückseite. Gemessen wird von der Mitte des Ziels zur
  Mitte des Spielers. Belegt in vier Werkzeugen, übereinstimmend: RSR `FindEnemyPositional` (cos 45°), BossModReborn
  `AIHints.GoalSingleTarget` und `GoToPositional`, Avarice `Util` (90–135° Flanke, 135–180° hinten), Resonant. Dazu
  Gemeinschaftsquellen: Square-Enix-Forum („epicenter" des Zielkreises), akhmorning („Position = Mitte des Modells").
  Kein Spielblatt nennt den Winkel.
- *Wer Positionals hat (7.x):* alle sechs Nahkämpfer, jeder mit beiden Seiten
  (`ConfigurationHelper.ActionPositional`; consolegameswiki, Kategorie „Positional actions"; Avarice
  `PositionalPotencies`):

  | Job | hinten | Flanke |
  |---|---|---|
  | DRG | Chaos Thrust/Chaotic Spring, Wheeling Thrust | Fang and Claw |
  | MNK | Demolish | Snap Punch/Pouncing Coeurl |
  | NIN | Aeolian Edge, Trick Attack | Armor Crush |
  | RPR | Gallows, Executioner's Gallows | Gibbet, Executioner's Gibbet |
  | SAM | Gekko | Kasha |
  | VPR | Hindsting Strike, Hindsbane Fang, Swiftskin's Coil | Flanksting Strike, Flanksbane Fang, Hunter's Coil |

  Die Regel sitzt auf der Ebene Nahkämpfer; keine Untergruppe weicht ab, Heiler, Tanks und Fernkämpfer haben keine.
  Erfasst, nicht bearbeitet: Blaumagier (Goblin Punch, vorn) und Sonderinhalte (Lost Assassination).
- *Kein Positional:* Gegner mit `IsOmnidirectional` (BNpcBase) und Gegner unter Directional Disregard (Status 3808)
  werden von überall voll getroffen. So lesen es RSR (`HasPositional`), BossModReborn (`Actor.Omnidirectional`) und
  WrathCombo (`TargetNeedsPositionals`).
  - *Sein Hinweis (15.11.2026):* Endbosse am Rand der Arena, hinter die man nicht kommt, brauchen weder Positional
    noch True North.
  - Am Blatt geprüft (xivapi v2, 15.11.2026), Stichprobe von fünf:
    - rundum markiert, alle drei am Rand: Zodiark (BNpcBase 12877), Valigarmanda (16664), Cloud of Darkness im
      Chaotic (17950);
    - nicht markiert, beide in der Mitte: Rubicante (15756), Hydaelyn (13549).
  - Für einen rundum markierten Gegner wirft RSR kein True North, wählt frei und meldet keine Seite.
- *Wann geurteilt wird:*
  - Die Aktionsanfrage trägt Blickrichtung und Richtung zum Ziel, nicht den Ort (BossMod `ClientIPC.ActionRequest`).
    Der Server urteilt also mit dem Ort, den der Client zuletzt gemeldet hat.
  - Sofortige Waffenfertigkeiten urteilen beim Drücken (akhmorning, Gemeinschaftsquelle).
  - Positionsmeldungen gehen in festen Takten; das Square-Enix-Forum nennt 0,3 s, unbestätigt.
  - Schluss (abgeleitet): Ein Schritt muss vor dem Drücken beendet und gemeldet sein.
- *Ob es traf:* Der Schadenseintrag trägt im dritten Parameter den Bonus in Prozent der Gesamtpotenz, Kombo und
  Positional zusammen (Avarice `Memory`, xivanalysis `bonusPercent`). Ein eigenes Kennzeichen für das Positional gibt
  es nicht. Avarice und xivanalysis führen die Werte je Aktion, Stufe und Buff in Tabellen von Hand.
- *Vorbilder* (Stand je Klon, 15.11.2026):
  - Avarice (PunishXIV, cbf6a9c): Stellungslinien, Trefferrückmeldung als Bild, Ton und Chat, Statistik. Liest die
    Seite aus RSRs `DesiredPositionalChanged` und aus WrathCombo.
  - Positional Assistant (VariableVixen, bbbde44): Linien um das Ziel.
  - Resonant (aulus-asina, 8a27d1a): Sektoren, Nahkampfkreise 2,1 y und 3 y, Hitbox des Spielers.
  - PixelPerfect (Haplo064, 162c07c): Hitbox und Ringe.
  - BossModReborn (35a908ad7) und BossMod (c50e67f3): Stellungshinweis in der Welt (`ShowPositionals`), Bewegung durch
    KI und Modul.
  - WrathCombo (a457838): „Dynamic True North" mit Ladungsrückhalt (`RPR_ST_TrueNorthDynamicHoldCharge`).
  - Splatoon zeichnet nur und ist kein Positionswerkzeug.
- *Was RSR tut:*
  - Wahl nach Stellung bei SAM, RPR, VPR und NIN: die Aktion, für die er gerade richtig steht, zuerst
    (`CanHitPositional`). DRG und MNK folgen ihrer festen Folge.
  - True North im letzten Webfenster, wenn der nächste GCD sein Positional verfehlen würde
    (`StateUpdater.ShouldAddPositional`, Option „AutoUseTrueNorth").
  - Die Seite des nächsten GCD geht als `DesiredPositional` über IPC hinaus.
- *Wer sie liest:*
  - BossModReborn bewegt den Spieler dorthin. Das tut ihre KI („Follow target", Schalter „Follow
    RotationSolverReborn's desired positional", ab Werk an) oder ihr Modul „Misc AI: Goes to specified positional"
    mit dem Kantenpuffer „Stand at positional edges", 0,5, 1,5 oder 3 y.
  - Ihre Navigation stellt Sicherheit vor Uptime vor Positional (`NavigationDecision`).

**Die beste Stellung**
- *Seine Frage (15.11.2026):* „wahrscheinlich hinten im 45 grad winkel".
- Bestätigt: auf der Diagonale hinten, der Grenze zwischen hinten und Flanke. Von dort liegt jede Seite einen
  Bruchteil eines Schritts entfernt.
  - Der Weg ist Abstand zur Mitte mal Winkel. Bei 5 y Abstand sind 43° bis 47° rund 0,35 y, bei 6 y/s
    Laufgeschwindigkeit 0,06 s.
  - Je näher an der Mitte, desto kürzer der Weg.
- *Grenze 1:* Dreht das Ziel – Tankwechsel, der Tank zieht es –, wandert die Grenze mit. Genau auf ihr kippt jede
  kleine Drehung die Seite.
- *Grenze 2:* Die Mechanik bestimmt den Platz (Schweif, Rückenkegel, Verteilung); Sicherheit geht vor.

**Sein Vorschlag (15.11.2026): an der Grenze minimal „zittern“, damit er für die Rotation immer richtig steht**

Optionen: N (wie heute), B1 RSR bewegt selbst, B2 BossModReborn bewegt nach RSRs Seite (besteht), B3 RSR schärft
Seite und True North und misst das Urteil des Spiels, B4 nur Anzeige. Rückbau von B3: nichts wird gespeichert; ohne
die Teile gilt N.

- *B1, RSR bewegt selbst* (die Gehrichtung ersetzen wie BossMods `MovementOverride`, oder Tasten senden):
  - Stärke: wirkt ohne BossMod.
  - Schwächen:
    - RSR hat keine Gefahrenkarte. Ein Schritt über die Grenze kann in eine Fläche führen oder unter Pyretic schaden.
    - Der Schritt bricht Zauber und Kanäle: Iaijutsu, Harpe, Communio, Ten Chi Jin, Meditate.
    - Er kämpft gegen BossMods KI und gegen seine eigene Bewegungstaste.
  - Bedrohung: seine Vorgabe, nichts zu empfehlen, was ihn für Schaden aus einer sicheren Position holt; Konzept 13,
    B6 („RSR dreht, bewegt aber nie").
  - Bilanz (15.11.): verworfen. Am 19.11. erneut verworfen, mit anderem Grund: BossMods Bewegungsmodul leistet es ohne
    eigenen Hook („Bewegung in die Seite über BossMods Bewegungsmodul“).
- *B2, BossModReborn bewegt:*
  - Stärke: genau sein Vorschlag. Mit „Stand at positional edges" geht BossMod nur über die Kante, und nur wo der
    Platz sicher ist.
  - Schwäche (a): Unter True North und vor einem Ziel ohne Positional meldete RSR trotzdem eine Seite.
    - BossMods KI prüft True North nicht (`AIBehaviour`, nur das Modul prüft es) und läuft umsonst.
    - Avarice zeigte eine Seite, die nichts bringt.
  - Schwäche (b): Genau an der Kante kann eine kleine Drehung des Ziels zwischen Drücken und Urteil die Seite kippen.
    Ob das vorkommt, maß niemand.
- *B3, RSR schärft Seite und True North* – beseitigt B2(a), macht B2(b) messbar und steuert nach:
  - *B3a:*
    - `DesiredPositional` meldet keine Seite, solange True North den nächsten GCD noch deckt oder das Ziel kein
      Positional hat. Der Wert wird jeden Frame neu bestimmt, nicht nur beim Wechsel des GCD.
    - `CanHitPositional` gilt unter True North für beide Seiten; die Wahl nach Stellung folgt dann der Reihenfolge der
      Rotation.
  - *B3b, Urteil des Spiels* (`PositionalWatch`):
    - Gemessen wird beim Drücken jeder positionalen Aktion, auch seiner eigenen: gebrauchte Seite, Abstand zur Grenze
      in Grad, True North.
    - Das wird mit dem Bonus aus dem Schadenseintrag verglichen.
    - Die Bonuswerte lernt RSR selbst, ohne Tabelle, je Aktion, Kombostand und Stufe:
      - Ein Wert unter True North oder tief in der richtigen Seite (näher an ihrer Mitte als an der Grenze) ist ein
        Treffer.
      - Ein Wert tief in der falschen Seite ohne True North ist ein Fehlschlag.
      - Ein Wert in beiden Mengen urteilt nicht.
  - *B3c, Unsicherheitsband:*
    - Urteilt das Spiel gegen RSRs Messung, wächst ein Band auf diesen Abstand zur Grenze. Das gilt für einen
      Fehlschlag mit richtiger Messung ebenso wie für einen Treffer mit falscher.
    - Steht der Spieler für den nächsten GCD auf der richtigen Seite, aber innerhalb des Bands, wirft RSR True North
      nur bei vollen Ladungen. Eine Ladung bleibt so für die Mechanik.
    - Das Band gilt bis zum nächsten Gebietswechsel.
  - Stärken: kein Lauf umsonst, Treffer werden messbar, die Kante wird sicherer.
  - Schwäche: B3c verbraucht eine Ladung, die sonst bis zur Aufladung stehen bliebe. Der Puffer sinkt von 2 auf 1, bis
    sie zurück ist.
  - Bedrohung: Ein Ausreißer – das Ziel dreht genau beim Drücken – weitet das Band für das Gebiet. Preis: True North
    bei vollen Ladungen öfter als nötig, nie die letzte Ladung.
- *B4, nur Anzeige:* zeigt, bewegt nicht. Avarice gibt es schon und liest RSRs Seite; RSR baut keine eigene.
- Abgleich: Sein Ziel ist „immer richtig stehen". B2 bewegt, B3 sorgt dafür, dass die Bewegung nur kommt, wo sie
  etwas bringt, und dass ein Fehlurteil an der Kante True North auslöst. Keine stille Verengung: Ohne BossMods
  Bewegung bleibt die Wahl nach Stellung, True North und B3c.

**Sein Hinweis (15.11.2026): True North, wenn der Platz durch Gefahren belegt ist**
- So wirkt die Regel schon. Steht er im letzten Webfenster nicht auf der Seite des nächsten GCD, wirft RSR True
  North, solange eine Ladung da ist – ob die Fläche dort belegt ist oder aus einem anderen Grund.
- *Gegenthese:* Ladungen für belegte Flächen aufheben und ohne Gefahr nicht werfen (wie WrathCombos Ladungsrückhalt).
  Kampfbeispiel, DRG ohne Bewegung durch die KI:
  - Ohne Rückhalt: einmal ohne Gefahr zu spät, True North geht von 2 auf 1. Dann belegt eine Fläche den Rücken 12 s
    lang; True North deckt 10 s, ein GCD verfehlt.
  - Mit Rückhalt: der erste GCD verfehlt, die Fläche wird mit zwei Ladungen gedeckt.
  - Beide Wege kosten einen Fehlschlag. Der Rückhalt gewinnt nur bei einer Gefahr, die kurz nach einem Fehlwurf kommt
    und länger dauert als eine Wirkdauer. Er verliert in jedem Fall, in dem keine Gefahr folgt.
- Bilanz: Ausgleich, kein Gewinn; nicht gebaut. Mit BossMods KI gibt es den Fall ohne Gefahr kaum, weil sie ihn vor
  dem letzten Webfenster hinstellt.

**Befund 19.11.2026: Niemand bewegte ihn (A926, A928)**
- *Seine Rüge (19.11.2026):* „bei den positionals hatte ich dir eine klare vorgabe gegeben, mit dem hinweis auf zittern also
  minimalste bewegungen in die richtige position. du meintest du kriegst das über befehle an bmr hin. also war das auch
  gelogen.“ Seit dieser Rüge gilt sein Vorschlag vom 15.11. als *Vorgabe*: minimale Bewegung in die richtige Stellung.
- *Am Quelltext (BossModReborn 8d5ff8e21, 15.11.2026):*
  - „Goes to specified positional“ (`GoToPositional`) legt nur ein Zielgebiet in die Hinweise (`GoalZones`), mit einem
    Kantenpuffer von 0, 0,5, 1,5 oder 3 y. Bewegt wird daraus von einem von zwei Wegen: der KI (`AIController`) oder dem
    Modul „Automatic movement“ (`NormalMovement`, Ziel „Pathfind“), das ohne KI läuft, solange es in der aktiven
    Voreinstellung (Preset) steht.
  - Andere Plugins steuern Voreinstellungen über IPC: `Presets.Create`, `Presets.SetActive`, `Presets.ClearActive`,
    `Presets.GetActive`, `Presets.AddTransientStrategy`.
  - Weder KI noch eine Voreinstellung mit „Automatic movement“ waren an der Puppe aktiv: Er stand still auf der Diagonale.
  - Widerrufen (C235, C236): „BossMod bewegt ihn“ (15.11., ohne Prüfung der KI) und „ohne KI bewegt BossMod nie, keine
    Schnittstelle nimmt einen Schritt an“ (A926; die unabhängige Prüfung fand das Modul und die Schnittstelle).
- *Gemessen (seine Spur, 19.11.2026, Living Memory, Puppe):* Er stand still auf der Diagonale hinten, 0,2° in der Flanke.
  - Monk: Demolieren zehnmal im Abstand von 17,6 bis 37 s, sechsmal mit True North (Treffer, 14 %) und viermal ohne (0 %,
    verfehlt); Tigerpeitschenhieb bei +0,2° stets Treffer. True North lädt eine Ladung je 45 s; bei diesem Abstand
    bleibt fast jedes zweite Demolieren ohne Ladung.
  - Samurai: an derselben Stelle −0,1° vor der Flanke. Gekko bei +0,1° hinten stets 61 %; Kasha ohne True North dreimal
    56 % gegen 61 % mit – verfehlt, aber als „unknown“ gelesen.
  - *Geometrie bestätigt:* Das Spiel urteilt auf 0,1° genau wie RSR (45°-Grenze, Mitte zu Mitte). `ShouldAddPositional`
    (`FindEnemyPositional`, cos 45°) und `PositionalWatch` (`SideMargin`) messen dieselbe Grenze. True North fehlte
    allein an den Ladungen.
- *Zweiter Befund:* Die Lernregel (B3b) urteilt nur über Drücke „tief“ in einer Seite; an der Kante blieb das Band 0,0°.

**Bewegung in die Seite über BossMods Bewegungsmodul (A928)**

*Optionen nach der unabhängigen Prüfung (A928):*
- *N, True North allein:* an der Puppe ohne Ladung bei fast jedem zweiten Demolieren.
- *B1, eigener Hook auf die Gehsteuerung:* ausgearbeitet in A926, von der Prüfung angegriffen mit drei Befunden hoher
  Schwere: unbegrenzter Weg bis um das Ziel herum, keine Erkennung eines Treffers durch den Schritt, Sondermodi
  (Stillstand, Fehlleitung) nicht abgedeckt; dazu eine Signatur, die nach einem Patch eine falsche Funktion treffen
  kann, eine unfertige Verkettung mit BossMods eigenem Hook und Lücken in der Nachbildung seiner Eingabeprüfung. Jede
  dieser Lücken ist in BossMods `MovementOverride` und `NormalMovement` schon gelöst. Verworfen: Es baute nach, was in
  seinem Spiel schon läuft, mit eigenem Absturzrisiko.
- *B2a, BossMods KI:* bewegt, wenn er sie einschaltet; dann folgt sie RSRs Seite schon (`FollowRSRDesiredPositional`).
- *B2b, BossMods „Automatic movement“, von RSR nur im Fenster der Seite eingeschaltet:* Gewählt.
  - Stärken: Gefahrenkarte und Wegsuche von BossMod; Sondermodi (Pyretic, Stillstand, Rückstoß) über „Special“;
    Fehlleitung und Vorrang seiner Taste in BossMods Hook; das Ziel ist das nächste Stück der Seite mit 0,5 y Puffer –
    ein Weg um die Kante, nicht um das Ziel. [Schätzung: BossMods Wegsuche nimmt das nächste Feld höchsten Werts, also
    den kürzesten Weg in die Seite.]
  - Schwächen: Nur mit BossMod; steht eine eigene Voreinstellung von ihm aktiv, fasst RSR sie nicht an (dann N). Im
    Fenster weicht BossMods Wegsuche auch Flächen aus – eine Bewegung aus der Gefahr, nie in sie.
  - Bedrohung: BossMod ändert Namen von Modul, Spur oder Option; dann lehnt `Presets.Create` ab, die Spur schreibt es
    einmal, und es gilt N.

*Was gilt (gebaut, A928, Ebene Nahkämpfer; `PositionalMove`, Einstellung „Move into positionals through BossMod“):*
- *Fenster:* Der nächste GCD ist positional, True North deckt ihn nicht, sein Ziel hat ein Positional und sieht nicht
  ihn an (allein in der offenen Welt dreht es sich mit), und er steht nicht 0,5 y tief in der Seite. Das Fenster endet
  mit dem Druck dieses GCD oder wenn eine Bedingung fällt. Ein positionaler oGCD, der vor diesem GCD ansteht (Trick
  Attack), hält das Fenster zu, bis er gedrückt ist.
- *Voraussetzungen:* BossMod läuft, seine KI navigiert nicht, keine andere Voreinstellung ist aktiv, und kein Blick
  steht an (Konzept 13, Blickwache: im Standardmodus dreht Gehen die Figur; ob BossMods Bewegung die Blickrichtung hält,
  ist unbelegt).
- *Im Fenster:* RSR legt einmal je Sitzung die Voreinstellung „RSR Positionals“ an („Automatic movement“: Ziel
  Pathfind; „Goes to specified positional“: Kantenpuffer 0,5 y), setzt deren Seite als vorübergehende Strategie auf die
  des GCD und aktiviert sie; am Ende des Fensters deaktiviert RSR sie wieder. Er bleibt danach stehen, wo er ist.
- *Selbstkontrolle* (code.md, „Entscheidung zur Laufzeit“):
  - Fehlschlag ist: Er kam auf der Seite an (beim Druck gemessen) und das Spiel verfehlt trotzdem; oder ein gegnerischer
    Treffer, der nicht die ganze Gruppe trifft (kein Raidwide), erreicht ihn im Fenster. Nicht: BossMod hielt ihn an (Gefahr), er ging selbst, das Ziel drehte sich.
  - Erster Fehlschlag: keine Fenster mehr für dieses Ziel. Zweiter im Gebiet: keine bis zum Gebietswechsel; dann gilt N.
  - Spur: „positional move: BossMod walks you to the rear/flank for …“, „… pressed …° inside its side, true north
    yes/no – hit/miss/unknown“, „off for … – …“, „not now – <Grund>“ (einmal je Grund), „stopped for this target/area –
    …“. Hat BossMod keine Preset-Schnittstelle (ältere Fassung), fragt RSR beim nächsten Fenster erneut.
- *Absturz:* Bleibt „RSR Positionals“ aktiv, weil eine Sitzung ohne Schließen endete, schaltet RSR sie beim ersten Lauf
  mit BossMod einmal ab (Spur: „… was still active from an earlier session“) – sonst wiche BossMod bis zum Abschalten
  von Hand selbständig aus.
- *Option:* „Move into positionals through BossMod“ (Gruppe Positionals), ab Werk an (seine Vorgabe). Aus gilt N.
- *Lernregel (B3b):* bleibt; die geplante Erweiterung an der Kante entfällt. Mit 0,5 y Puffer steht er nicht mehr an der
  Kante, und ein Urteil nach dem kleinsten gelernten Bonus täuscht, wo ein Buff die Potenz ändert (Kazematoi).

*Befunde der Prüfung und ihre Antwort:*

| Befund | Antwort |
|---|---|
| B1 Weg unbegrenzt | BossMods Ziel ist die nächste Stelle der Seite; Weg und Ort prüft seine Wegsuche |
| B2 schädlicher Fehlschlag nicht erkannt | Treffer im Fenster zählt als Fehlschlag (Selbstkontrolle) |
| B3 Stillstand, Fehlleitung fehlen | BossMods „Special“ und sein Hook decken sie |
| B4 nur Endpunkt geprüft | BossMods Wegsuche plant über die Karte |
| B5 Signatur nach Patch | kein eigener Hook |
| B6 Modul und Schnittstelle übersehen | Grundlage dieser Fassung; C236 |
| B7 Verkettung der Hooks | kein eigener Hook; KI und fremde Voreinstellung gehen vor |
| B8 Lernregel mit Buffs | Erweiterung entfällt |
| B9 Abbruch als Fehlschlag | nur Ankunft mit Verfehlen und Treffer im Fenster zählen |
| B10 Anker nur positionaler Druck | Fenster hängt am nächsten GCD, nicht am vorigen Druck |
| B11 Trick Attack | positionaler oGCD hält das Fenster zu |
| B12 Abstand aus falschem Maß | Kantenpuffer 0,5 y wie BossMods Option; eine Drehung des Ziels deckt True North bei vollen Ladungen (B3c) |
| B13 Rückstoß im Schritt | BossMods „Special“ |
| B14 Eingabeprüfung | BossMods Hook |
| B15 Zahlen und alter Stand | berichtigt in diesem Abschnitt |

*Falsifikation:*
- (1) *Kein Defekt:* widerlegt am Mitschnitt (viermal Demolieren mit 0 %, dreimal Kasha mit 56 %).
- (2) *Option falsch:* BossMod bewegt ihn in eine Fläche, die es nicht kennt – derselbe Preis wie seine KI; die
  Selbstkontrolle stellt nach einem Treffer im Fenster ab.
- (3) *Ausgeliefert, nichts ändert sich – warum?* BossMod fehlt, eine eigene Voreinstellung ist aktiv, BossMod lehnt die
  Voreinstellung ab, oder das Ziel sieht ihn an. Jeder Fall schreibt seinen Grund einmal in die Spur.

*Anforderungen (seine Vorgaben → Kriterium → Ort → Beleg heute → Messweg):*

| # | Vorgabe | prüfbares Kriterium | Ort | Beleg heute | Messweg |
|---|---|---|---|---|---|
| P1 | Minimale Bewegung in die richtige Stellung (15.11., 19.11.) | Ziel ist die nächste Stelle der Seite mit 0,5 y Puffer; nach dem Fenster keine Bewegung | „Was gilt“, Fenster | BossMod-Quelltext | „positional move: … arrived“ |
| P2 | Sicherheit vor Schaden | Weg und Ziel aus BossMods Gefahrenkarte; kein Fenster unter seiner KI oder fremden Voreinstellung | Voraussetzungen | BossMod-Quelltext | „positional move: … stopped“ |
| P3 | Kein Supergau, im Spiel lernen (14.11.) | nach einem Fehlschlag kein Fenster für das Ziel, nach dem zweiten im Gebiet keines | Selbstkontrolle | statisch | „positional move: … off“ |
| P4 | Seine Taste geht vor (Spielweise) | Eingabe von ihm überstimmt BossMods Bewegung | BossMods Hook | BossMod-Quelltext | – |
| P5 | Alle Nahkämpfer (Universell zuerst) | jeder GCD in `ActionPositional`; positionale oGCDs halten das Fenster | Fenster | Code | Zeilen bei MNK, SAM, DRG, RPR, VPR, NIN |

*Tragende Annahmen:*
- A1: Modul-, Spur- und Optionsnamen gelten in seinem BossMod wie in 8d5ff8e21. Prüfung: `Presets.Create` antwortet
  „false“, die Spur schreibt es.
- A2: Deaktivieren hält die Bewegung im selben Frame an. [Schätzung: BossMods Modul läuft nur aus der aktiven
  Voreinstellung.] Prüfung: „positional move: … arrived“ gegen den Abstand beim Druck.

**Wechselwirkungen**
- *Bewegungssperre beim Wirken (Konzept 03):* BossMods Gehen für Positionals fällt zwischen die GCDs. Die
  Nahkämpfer wirken kaum (Iaijutsu, Harpe); dort hält BossMod über `MaxCastTime`.
- *Blickwache (Konzept 13):* Sie dreht und bewegt nicht. True North dreht nichts.
- *Avarice:* liest dasselbe Signal und zeigt unter True North jetzt keine Seite mehr.

**Falsifikation**
- *Kein Defekt:* B3a widerlegt die These am Code: Die KI lief unter True North umsonst (BossModReborn
  `AIBehaviour`). B3c ebenso: An der Kante bleibt ein Fehlurteil des Spiels ohne Antwort.
- *Option falsch:* Das Band lerne aus Zufall. Dagegen: Nur ein Urteil des Spiels gegen RSRs Messung weitet es, und es
  verbraucht nur Ladungen, die sonst stünden.
- *Ausgeliefert, nichts ändert sich – warum?* Wenn nie ein Bonuswert gelernt wird, weil er nie True North nutzt und
  nie tief in einer Seite steht. Dann bleibt das Band null und B3c wirkt nicht; B3a wirkt davon unabhängig. Die
  Spurzeile mit „unknown" zeigt den Fall.

**Messweg**
- Je positionale Aktion eine Spurzeile „positional: <Aktion> needs <Seite> …, <Abstand>° inside …, true north
  yes/no – hit/miss/unknown (bonus n%), band b°".
- Vorhersage `positional-learns` (A819): Nach einer Zeile mit True North und einem Treffer folgt für dieselbe Aktion
  eine Zeile „hit" ohne True North.

---

## Physische Fernkämpfer (BRD · MCH · DNC)

```mermaid
flowchart TD
    A[GeneralGCD] --> B[DoT / Ressourcen-Erhalt]
    B --> C[Burst-Fenster]
    C --> D{Gegneranzahl}
    D -- AoE --> E[AoE-Level-Kette]
    D -- ST --> F[ST-Level-Kette]
    E --> G[base.GeneralGCD]
    F --> G

    H[Gruppen-Mitigation] -.-> I{welche Aktion}
    I -- BRD --> J[Troubadour]
    I -- MCH --> K[Tactician]
    I -- DNC --> L[Shield Samba]

    style J fill:#36c,color:#fff
    style K fill:#36c,color:#fff
    style L fill:#36c,color:#fff
```

### Identisch in allen drei

1. **Gruppen-Minderung** (Troubadour/Tactician/Shield Samba) —
   dieselbe Rolle, dieselbe Dauer (15 s), dieselbe BMR-Auslösung (`BMRShouldRefreshBefore` vor Raidwide und
   Tankbuster). Die drei sind eine Familie: Die neue ersetzt die stehende, gleich von wem, und aufgefrischt wird nur,
   wenn keine der drei den angekündigten Treffer noch abdeckt (Konzept 08, „Eine fremde Minderung derselben Familie
   zählt wie die eigene", A592).
2. **Level-Ketten dominieren den Filler.**
3. **Kein Raise, keine GCD-Heilung; Second Wind in `HealSingleAbility` bei allen drei.**

### Wo die Gruppe auseinanderläuft

| | BRD | MCH | DNC |
|---|---|---|---|
| `HealAreaAbility` | – | – | **✓** (Curing Waltz, Improvisation) |
| `DispelAbility` | **✓** (Warden's Paean) | – | – |
| `MoveForwardAbility` | – | – | ✓ (En Avant) |
| `MoveBackAbility` | ✓ (Repelling Shot) | – | – |
| Level-Ketten-Glieder | 2 | **12** | 0 |

Die verbleibenden Unterschiede folgen den Aktionen, die nur ein Job hat (A7, 4b3c9412).

---

## Magische Fernkämpfer (SMN · RDM · PCT · BLM)

```mermaid
flowchart TD
    A[GeneralGCD] --> B{Phasen-/Zustandsautomat}
    B -- "BLM" --> C[InFireOrIce – ausgelagert]
    B -- "SMN" --> D[Bahamut / Phoenix / Solar]
    B -- "RDM" --> E[Mana-Balance]
    B -- "PCT" --> F[Motif / Muse]
    C --> G[Filler]
    D --> G
    E --> G
    F --> G
    G --> H[base.GeneralGCD]

    style C fill:#4a3,color:#fff
```

### Identisch in allen vier

1. **Addle-Sustain** über `ShouldSustainMitigationDebuff(StatusID.Addle)` in
   beiden Defense-Slots (beim Beschwörer über `TryAddleBeforeDamage`).
2. **Ein Zustandsautomat als Kern**, Filler nur als Rest.

### Wo die Gruppe auseinanderläuft

| | SMN | RDM | PCT | BLM |
|---|---|---|---|---|
| Automat liegt in | benannten Stufen (A4a) | `GeneralGCD` | benannten Stufen (A4a) | **privaten Methoden** |
| Top-Level-Zweige vor A4a | 24 | 22 | 33 | **7** |
| `HealSingleGCD` | ✓ | ✓ | – | – |
| `RaiseGCD` | ✓ | ✓ | – | – |
| `MoveForwardGCD` | ✓ | – | – | – |
| Configs | 13 | 9 | 8 | 5 |

BLM erreichte mit **7** Top-Level-Zweigen dieselbe fachliche Abdeckung, für die
PCT **33** brauchte. Der Unterschied ist reine Ablauforganisation, nicht
Job-Komplexität; A4a hat PCT und SMN deshalb in benannte Stufen zerlegt.

---

## Gruppenvergleich in einer Tabelle

| Gruppe | gemeinsame Makrostruktur | echte Abweichungen | unbegründete Abweichungen |
|---|---|---|---|
| Heiler | Kurzschluss → Sustain → Ressource → AoE → DoT → Filler | Eukrasia, Pet, Karten | Gelehrter ohne Pflege im Pull (kein Sofortschild, Konzept 15) |
| Tanks | Ressource → AoE/ST → Combo → Ranged-Fallback | – | Unverwundbarkeit an zwei Orten (Konzept 05) |
| Melee | Burst → Combo → AoE → Ranged-Fallback | Mudra, Formen, Positionals | – (DRG, MNK und VPR haben seit 060b8fc82 den Sprung zum Ziel im Countdown, `04`, „Der Sprung zum Ziel am Ende des Countdowns“) |
| Phys. Ranged | DoT/Ressource → Burst → AoE/ST | Tänze (DNC) | – |
| Mag. Ranged | Automat → Filler | Elementphasen | Automat mal ausgelagert, mal inline |
