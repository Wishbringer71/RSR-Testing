# 01 · Ablaufstruktur je Job

Bestandsaufnahme, kein Entwurf. Grundlage: Override-Matrix und
`GeneralGCD`-Skelette, beide maschinell aus dem Quellcode extrahiert, nicht aus dem
Gedächtnis rekonstruiert.

Hook-Profile, Zeilenumfänge und Optionszahlen sind am 02.10.2026 aus dem Quellcode gemessen. Die Leitern wurden an
diesem Tag gegen die oberste Ebene von `GeneralGCD` geprüft. Für die
fünf Dateien, deren `GeneralGCD` nach `04-concept.md` A4a in benannte Stufen zerlegt
wurde — BLU, PhantomDefault, PCT, SAM, SMN —, ist die dortige Zweigzahl die vor dem
Umbau; die Aufrufreihenfolge ist unverändert. Wer die aktuellen Zahlen braucht, misst
sie neu, statt sie hier zu lesen.

**Zwei Nachbarkonzepte beantworten Fragen, die hier bewusst offen bleiben:** Welche Aktionen ein
Job überhaupt besitzt und welche davon der Baum benutzt, steht in `05-action-coverage.md`; was innerhalb
einer Rollengruppe gleich und verschieden ist, in `02-groups.md`. Dieses Dokument beschreibt
allein die **Struktur** des Ablaufs je Job.

## Lesehilfe

Jeder Job wird in zwei Sichten dargestellt.

**Hook-Profil** — welche der Dispatch-Slots der Job überhaupt belegt. Nicht
belegte Slots fallen auf `CustomRotation`/`base` zurück. Das Profil sagt, *wo*
ein Job in die zentrale Kette eingreift.

**Leiter** — die Reihenfolge der Entscheidungen innerhalb `GeneralGCD`, auf
Ebene-1-Zweige reduziert. Die Zahl links ist die Prioritätsstufe; gleiche Zahl
heißt „gehört fachlich zusammen".

Notation:

```
├ n  Rolle          konkrete Aktion / Bedingung
└ n  Rolle          letzter Zweig vor base.GeneralGCD
```

Wiederkehrende Rollen (in allen Jobs dieselbe Bedeutung):

| Rolle | Bedeutung |
|---|---|
| **Kurzschluss** | verlässt die Methode sofort, bevor irgendeine Rotationslogik läuft |
| `Sustain` | proaktives Aufrechterhalten eines eigenen Effekts, kein Reagieren |
| **Ressource** | Verbrauch/Overcap-Schutz einer Job-Ressource (Lily, Chakra, Aether …) |
| `Burst` | nur innerhalb eines Burst-Fensters relevant |
| `DoT` | Aufrechterhalten eines Ziel-Debuffs |
| `AoE` | Zweig, der an Gegneranzahl gekoppelt ist |
| `Combo` | positions-/reihenfolgengebundene Kette |
| `Filler` | Standardaktion, wenn nichts anderes greift |
| `Level-Kette` | dieselbe Aktion in mehreren Aufstiegsstufen, absteigend geprüft |

---

## Heiler

### WHM — 1118 LOC, 26 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealAreaGCD · HealSingleGCD · RaiseGCD
  oGCD    Emergency · General · Attack · DefenseArea · DefenseSingle · HealArea · HealSingle · MoveForward
  Flags   CanHealSingleSpell · CanHealAreaSpell · DisplayRotationStatus

GeneralGCD
  ├ 0  Kurzschluss    Swift + Raise    → base
  ├ 1  Sustain        TrySustainRegenOnTank
  ├ 2  Ressource      Afflatus Misery / Rapture (Lily-Overcap)
  ├ 3  Burst          Glare IV
  ├ 4  Ressource      Confession-Ablauf
  ├ 5  AoE            Holy-Zweig
  ├ 6  DoT            Aero-Kette
  ├ 7  Level-Kette    Glare III → Glare → Stone IV → Stone III → Stone II → Stone
  ├ 8  Ressource      Lily-Downtime-Verbrauch
  └ 9  DoT            Aero-Kette, zweiter Durchgang
```

Der frühere zweite Kurzschluss (Thin Air + Raise → `RaiseGCD`) umging die Einstellung „Hard-cast raise" und ist
entfernt (A401, Konzept 11). Die Level-Kette ist mit sechs Gliedern die längste im gesamten Repo.

### AST — 765 LOC, 21 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealAreaGCD · HealSingleGCD · RaiseGCD
                    · DefenseAreaGCD · DefenseSingleGCD
  oGCD    Emergency · General · Attack · DefenseArea · DefenseSingle · HealArea · HealSingle
  Flags   CanHealSingleSpell · CanHealAreaSpell · DisplayRotationStatus

GeneralGCD
  ├ 0  Kurzschluss    Swift + Raise → base
  ├ 1  Sustain        TrySustainAspectedBeneficOnTank
  ├ 2  AoE            Gravity II → Gravity          (Level-Kette)
  ├ 3  DoT            Combust III → II → Combust    (Level-Kette)
  └ 4  Filler         Fall Malefic → IV → III → II → Malefic  (Level-Kette)
```

Die flachste Heiler-Leiter: drei reine Level-Ketten hintereinander, keine
Ressourcenlogik in `GeneralGCD` (Karten laufen komplett über oGCD-Slots).

### SGE — 1004 LOC, 24 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealAreaGCD · HealSingleGCD · RaiseGCD
  oGCD    Emergency · General · Attack · DefenseArea · DefenseSingle · HealArea · HealSingle
  Flags   CanHealSingleSpell · CanHealAreaSpell · DisplayRotationStatus

GeneralGCD
  ├ 0  Kurzschluss    Swift + Raise → base
  ├ 1  Reaktiv        DoEukrasianPrognosis II / Prognosis / Diagnosis
  ├ 2  Ressource      Phlegma (Ladungs-Overcap)
  ├ 3  Zustand        Party-/Tank-Scan
  ├ 4  Bewegung       Toxikon bei IsMoving
  ├ 5  AoE            Eukrasian Dyskrasia → Dyskrasia
  ├ 6  DoT            Eukrasian Dosis III → II → Dosis  (Level-Kette)
  ├ 7  Filler         Dosis
  └ 8  Leerlauf       Eukrasia außerhalb Kampf / ohne Ziel, Anti-Brick
```

Einziger Heiler mit einem zweistufigen Cast-Modell (Eukrasia + Folgeaktion).
Das erzwingt eine eigene Vorstufe (`_EukrasiaActionAim`), die kein anderer Job
kennt — die acht Stufen sind größtenteils diesem Modell geschuldet. Der frühere Sustain-Zweig
`TrySustainEukrasianDiagnosisOnTank` ist entfernt (5755ad5b, A5): Er prüfte die Barriere selbst als
Erneuerungsgrund; Schaden verbraucht sie, und der Zweig legte sie im Pull alle paar Sekunden neu auf. Die
Barriere im Pull läuft über die gemeinsame Heilerregel `TryPullUpkeepOnTank` als Eukrasia-Ziel in Stufe 1
(`PullBarrierDue`, Option `UsePreEukrasianDiagnosis`, ab Werk an; Konzept 15, V1).

### SCH — 951 LOC, 24 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealAreaGCD · HealSingleGCD · RaiseGCD · DefenseAreaGCD
  oGCD    Emergency · Attack · DefenseArea · DefenseSingle · HealArea · HealSingle · Speed
  Flags   CanHealSingleSpell · CanHealAreaSpell · DisplayRotationStatus

GeneralGCD
  ├ 0  Kurzschluss    Swift + Raise → base
  ├ 1  Setup          Summon Eos (Pet-Existenz)
  ├ 2  Ressource      MP-Notschwelle
  ├ 3  Zustand        Party-Scan
  ├ 4  Prognose       Ballpark-TTK
  ├ 5  DoT            Bio-Kette
  └ 6  Bewegung       Ruin II bei Bewegungszeit
```

Einziger Heiler ohne `GeneralAbility`-Override und einziger mit
`SpeedAbility`. Als einziger Heiler ohne Pflege auf dem Tank im Pull: Er hat keinen Sofortschild (Adloquium
hat Wirkzeit), und Stehenbleiben ist erst am Ende des Wall-to-Wall vorgesehen (Konzept 15, V1).

---

## Tanks

Gemeinsam: alle vier belegen `CountDownAction`, `GeneralGCD`, `AttackAbility`, `DefenseAreaAbility`,
`DefenseSingleAbility`, und alle vier melden `HasOwnArmsLengthPullRule`. Keiner hat Raise. GCD-Heilung hat nur der
Paladin (Clemency).

### PLD — 565 LOC, 16 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealSingleGCD · MyInterruptGCD
  oGCD    Emergency · General · Attack · DefenseArea · DefenseSingle · MoveForward
  Flags   CanHealSingleSpell · DisplayRotationStatus · HasOwnArmsLengthPullRule
```

Einziger Tank mit `HealSingleGCD` (Clemency) und `CanHealSingleSpell`. Nutzt
als einziger Tank eine Magie-/Physik-Phasenteilung (Requiescat-Fenster).

### WAR — 688 LOC, 17 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD
  oGCD    General · Attack · DefenseArea · DefenseSingle · HealSingle
  Flags   HasOwnArmsLengthPullRule
```

Kein `EmergencyAbility`-Override in der Reborn-Datei; Holmgang liegt in der Basisschicht
(`WarriorRotation.EmergencyAbility`, Konzept 05 „Zwei Ebenen“). Die Selbstheilungen (Bloodwhetting, Thrill of Battle, Equilibrium) laufen über
`HealSingleAbility`; die Abwehr steht in Konzept 09.

### DRK — 840 LOC, 12 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD
  oGCD    Emergency · Attack · DefenseArea · DefenseSingle · HealSingle · MoveForward
  Flags   CanHealSingleAbility · HasOwnArmsLengthPullRule
```

Einziger Job im Repo mit `CanHealSingleAbility`-Override (TBN-Logik).

### GNB — 607 LOC, 3 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD
  oGCD    Emergency · Attack · DefenseArea · DefenseSingle · HealSingle · MoveForward
  Flags   HasOwnArmsLengthPullRule
```

Mit dem Schnitter die wenigsten Configs aller Jobs (3). Höchste Kartuschen-/Combo-Dichte in
`GeneralGCD`, aber fast keine Nutzer-Stellschrauben.

---

## Melee

### DRG — 421 LOC, 5 Configs

```
├ 1  AoE       Coerthan Torment / Sonic Thrust
├ 2  Combo     Trait-gestaffelte Kette (Lance Mastery I/II/IV, je ±Trait)
└ 3  Ranged    Piercing Talon
```

Auffällig: **sechs** `if (Trait.EnoughLevel) / if (!Trait.EnoughLevel)`-Paare
hintereinander — die Combo ist nach Trait-Stufen dupliziert statt datengetrieben.
Kein `CountDownAction`.

### MNK — 625 LOC, 5 Configs

```
├ 1  Ressource   Beast-Chakra-Zustand
├ 2  Burst       Winds/Fires Reply
├ 3  Form        Formless Fist → Opo-Opo-Form
├ 4  Burst       Perfect Balance ± Solar
└ 5  Leerlauf    kein Ziel in Reichweite
```

Formen-Zustandsautomat statt linearer Combo. Second Wind und Bloodbath in `HealSingleAbility`, Earth's Reply und
Mantra in `HealAreaAbility` (4b3c9412).

### NIN — 1152 LOC, 5 Configs

```
├ 1  Burst      Trick/Mug-Fenster + Ninjutsu-Vorbedingung
├ 2  Mudra      Ausführungszustand (zweigeteilt: !IsExecuting / IsExecuting)
├ 3  AoE        Hakke Mujinsatsu / Death Blossom
├ 4  Combo      Aeolian Edge → Gust Slash → Spinning Edge
└ 5  Leerlauf   Hide-Verwaltung außerhalb Kampf
```

Eine der größten Dateien (nach PhantomDefault, SMN und WHM) bei nur 5 Configs. Der Mudra-Zustandsautomat ist die
einzige Stelle im Repo, an der ein GCD über mehrere Frames „im Bau" ist.

### RPR — 594 LOC, 3 Configs

```
├ 1  Combo-Rettung  ablaufende Combo
├ 2  Ressource      Gluttony/Executioner
├ 3  Leerlauf       Soulsow
├ 4  DoT            Death's Design (mit eigener Refresh-Config)
├ 5  Burst          Enshroud / Soul Reaver
├ 6  Ressource      Soul Scythe / Soul Slice
├ 7  AoE            Nightmare/Spinning Scythe
└ 8  Ranged         Harvest Moon / Harpe
```

Einer von zwei Jobs mit `HasOwnInterruptGate` und eigenem
`AntiKnockbackAbility`-Override — beides wegen Combo-Sicherheit.

### SAM — 620 LOC, 4 Configs

```
├ 1  Burst      Ogi Namikiri / Kaeshi
├ 2  AoE        ≥3 Gegner: Tenka/Goken-Zweige
├ 3  AoE        ≥2 Gegner: Ogi
├ 4  Buff       Fugetsu/Fuka-Aufrechterhaltung
├ 5  Combo      Gekko/Kasha mit Positionals
└ 6  Opener     High-End-Duty-Sonderfall
```

Einziger Job mit expliziter, mehrstufiger Gegnerzahl-Staffelung (≥3 / ≥2)
direkt in `GeneralGCD`.

### VPR — 1058 LOC, 9 Configs

```
├ 1  Burst      Ouroboros / Generation-Kette (4 Stufen)
├ 2  Ressource  Serpent's Ire / Offering
├ 3  Combo      switch über (HasGrimHunter, HasGrimSkin)
├ 4  Combo      switch über 4 Stung/Bane-Flags
└ 5  Ranged     Uncoiled Fury / Writhing Snap
```

Einziger Job, der `switch` über Status-Tupel verwendet statt `if`-Ketten — die
kompakteste Combo-Darstellung im Repo. Kein `CountDownAction`.

---

## Physische Fernkämpfer

### BRD — 687 LOC, 12 Configs

```
├ 1  DoT        Iron Jaws (zwei Zweige: normal + Vorzieh-Refresh)
├ 2  Burst      Resonant Arrow / Apex / Radiant Encore / Blast Arrow
├ 3  AoE        Shadowbite → Wide Volley → Ladonsbite → Quick Nock
├ 4  DoT        Stormbite / Caustic Bite (je ±Level)
└ 5  Filler     Refulgent → Straight Shot → Burst Shot
```

Einziger Fernkämpfer mit `DispelAbility`-Override.

### MCH — 701 LOC, 7 Configs

```
Hook-Profil  CountDown · GeneralGCD · Emergency · Attack · DefenseArea · DefenseSingle · HealSingle
```

**12 Level-Ketten-Glieder** — mit Abstand die meisten aller Jobs. MCHs
`GeneralGCD` ist fast vollständig eine Abfolge von Aufstiegsstufen.

### DNC — 563 LOC, 4 Configs

```
Hook-Profil  CountDown · GeneralGCD · Emergency · Attack · DefenseArea · DefenseSingle
             · HealArea · HealSingle · MoveForward · DisplayRotationStatus
```

Einziger Fernkämpfer mit `HealAreaAbility` (Curing Waltz, Improvisation); `DefenseSingleAbility` trägt Shield Samba
(4b3c9412).

---

## Magische Fernkämpfer

### SMN — 1433 LOC, 13 Configs

Die Zündregel von Searing Light samt ihrer Abhängigkeit von der Gruppenzusammensetzung steht in
`12-searing-light-stacking.md` — hier nur die Ablaufstruktur.

```
├ 1  Setup      Summon Carbuncle
├ 2  Burst      Bahamut / Solar Bahamut / Dreadwyrm (4 Varianten nach Level+Burst)
├ 3  Primal     Slipstream / Crimson Cyclone / Crimson Strike
├ 4  Ressource  Gemshine / Precious Brilliance
├ 5  Phase      Bahamut/Phoenix/Solar-Zustand
├ 6  Burst      Brand of Purgatory / Umbral / Astral Flare
├ 7  AoE        Outburst
└ 8  Filler     Ruin III → Ruin II → Ruin  (Level-Kette)
```

### RDM — 707 LOC, 9 Configs

```
├ 1  Finisher   ManaStacks == 3
├ 2  Instant    Dualcast / Accelerate
├ 3  Burst      Resolution / Scorch
├ 4  Combo      Enchanted-Kette (je 2 Varianten: _45962 / Basis)
├ 5  Balance    WhiteMana vs BlackMana
└ 6  Filler     Verstone / Verfire / Vercure
```

Der Mana-Balance-Zweig ist die einzige Stelle im Repo, die zwei Ressourcen
gegeneinander abwägt statt eine gegen eine Schwelle.

### PCT — 614 LOC, 8 Configs

```
├ 1  Opener     CombatTime < 5
├ 2  Burst      Starry Muse / Star Prism
├ 3  Combo      Hammer-Kette
├ 4  Vorbereit. Motif-Zeichnung (Landscape/Creature/Weapon)
├ 5  Ressource  Paint/Comet-Cap
└ 6  Filler     12 Farbaktionen, zweistufig (II-Reihe → Basisreihe)
```

Die 12 Farbaktionen sind faktisch eine Level-Kette mit sechs parallelen
Strängen — die breiteste Filler-Struktur im Repo.

### BLM — 818 LOC, 5 Configs *(BLM_Default)*

```
├ 1  Burst     Flare Star
├ 2  Phase     InFireOrIce (Kernautomat, ausgelagert)
├ 3  Element   AddElementBase
├ 4  Filler    Scathe
└ 5  Erhalt    MaintainStatus
```

Fast die gesamte Logik liegt in privaten Hilfsmethoden statt in `GeneralGCD` — **das architektonische Gegenmodell zu
allen anderen Jobs** und der einzige Job, dessen `GeneralGCD` beim Lesen ohne
Kommentare verständlich ist.

*Hinweis:* `BLM_RP.cs` (199 LOC, 4 Configs) ist eine zweite, alternative BLM-Rotation mit eigener,
deutlich flacherer Thunder-Level-Kette. Die Override-Matrix oben führt beide
unter „BLM" zusammen und ist für `BLM_RP` deshalb nicht belastbar.

---

## Limited Jobs

### BLU — 1001 LOC, 10 Configs

Belegt die meisten Dispatch-Slots im Repo (28 überschriebene Mitglieder einschließlich Flags). `GeneralGCD` hatte
81 Zweige auf einer Ebene; seit A4a ist es ein Dispatcher über benannte Stufen, die Reihenfolge unverändert.

### BST — 538 LOC, 13 Configs

Seit dem Upstream-Umbau (07f9f7daf, 30.09.2026) eine eigene Rotation: `GeneralGCD` mit Quelling Wave,
Shieldsplitter, Axeblade Bite und Smash Axe. Begrenzter Job seit Patch 7.56 (Stufe 50, eigenes Solo-Duell „Crucible of
the Unbroken"); er hat ihn am 31.10.2026 genannt („z.B. Optimierung beastmaster"), damit gehört er in sein Profil.

**Geprüft am 31.10.2026 (A473), kein Defekt belegbar:**
- Die vier Äxte und ihre Formen bei 250 TP (Brutal Rage, Hawkish Talons, Risen Fall, Calamity) sind Waffenfertigkeiten in
  eigener Abklinggruppe 16, nicht in der GCD-Gruppe 58 (Spielblätter, xivapi: „do not share a recast timer with any
  other actions"); sie werden eingewoben, und `AttackAbility` ist ihr richtiger Platz.
- Der Innere Kompass verlangt für eine absichtliche Kombo die Richtung im Uhrzeigersinn (Volant → Rampant → Durant →
  Eldritch → Volant, offizieller Job-Guide). Die Basisschicht bildet das über die Herz-Status ab, die jede Axt verlangt
  und gewährt (`StatusNeed`/`StatusProvide`, etwa Avalanche Axe nach Volant Heart); ohne passendes Herz geht sie im
  zweiten Durchgang ohne diese Bedingung.
- Eine Referenz für die Rotation gibt es nicht: Der offizielle Job-Guide beschreibt nur Mechanik (Stand „Last Update: -"),
  The Balance führt keinen Beastmaster, icy-veins nur das Leveln. Optimieren ließe sich die Reihenfolge der Äxte und
  Tricks nach dem Kompass und der Kombozahl (Multiplikator bis 3,0 ab der fünften Kombo, Werkzeugquelle consolegameswiki),
  aber ohne Referenz und ohne einen Mitschnitt von ihm wäre das eine Annahme. Mit seinem ersten Mitschnitt als
  Beastmaster (ACT oder DefenseTrace) wird das gemessen.

**Anfreunden und Analysieren** (seine Frage vom 02.11.2026, ob sich vor dem Tod eines Gegners prüfen lässt, ob man sich
mit ihm anfreunden kann, und das Talent dann so einsetzen, dass es wahrscheinlich gelingt; A483):
- *Mechanik* (Spielblätter, xivapi v2, Action 44880 und 44882, Status 4624 und 4626, LogMessage 11217–11222, 11400,
  11425, 11443, 11455; 02.11.2026):
  - *Analysieren* (`GaugePvE`; Talent, 30 Yalm, 3 s Abklingzeit, kein Feindseligkeit, auch vom Reittier am Boden)
    antwortet im Chat:
    - „Mit diesem Ziel ist ein Pakt nicht möglich ...";
    - fünf Stufen von „... dürfte sehr schwierig sein" bis „... dürfte sehr einfach sein";
    - „Mit dieser Bestie hast du bereits einen Pakt geschlossen.";
    - „Du bist noch nicht stark genug ...".
  - *Anfreunden* (`CapturePvE`; Talent, 10 Yalm, 2,5 s, Attacke-Wert 100, kann das Ziel nicht kampfunfähig machen)
    markiert die Bestie für 120 s mit Freundschaftsempfänglich und dich mit Freundschaftsangebot. Der Pakt entscheidet
    sich beim Sieg über sie.
    - Die Chance ist umso größer, je niedriger ihre LP *beim Markieren* sind, und steigt, wenn ihre Stufe unter deiner
      liegt; über deiner Stufe ist kein Pakt möglich.
    - Der beste Zeitpunkt ist also der letzte vor dem tödlichen Treffer. Weil das Talent selbst nicht tötet, geht es
      auch bei 1 LP gefahrlos.
- *Nicht belegt:* ob die Stufe von Analysieren die aktuellen LP einrechnet; ob der Sieg durch einen Mitspieler zählt;
  eine Prozentzahl.
- *Anfreunden kostet einen GCD:* Es ist zwar ein Talent, liegt aber in der Abklinggruppe 58 der Waffenfertigkeiten
  (Action 44880, 2,5 s; xivapi v2, 02.11.2026) und ersetzt einen GCD mit 160 bis 600 durch einen mit 100.
- *Was eine Bestie ist, sagen die Blätter* (A487): Ein Gegner gehört zu einem Begleiter, wenn das Modell seiner
  Grundzeile (`BNpcBase.ModelChara`: Modell und Basis) dem Aussehen des Begleiters gleicht: `XBMPet` → `Pet` → erstes
  erlaubtes Aussehen (`PetMirage`) → `ModelChara`. In der gebauten Lumina heißen die Spalten `Pet.Unknown8`
  (`AllowedPetMirage[0]`) und `Pet.Unknown18` (zurück zu `XBMPet`); belegt am EXDSchema („latest", 17.10.2026) nach der
  Lage der Spalten und an xivapi (Eichhörnchen: Pet 48, PetMirage 10, Modell 2 und Basis 2, XBMPet 2). Anfreundbar ist
  eine Grundzeile mit dem Wert 5 in der unbenannten Spalte 10 (`BNpcBase.Unknown10`). So liest es WrathCombo
  (Werkzeugquelle); von den zehn Grundzeilen mit dem Modell des Eichhörnchens tragen vier die 5. Was die Spalte
  bedeutet, nennt keine Quelle. Die freigeschalteten Begleiter führt das Spiel in `XBMManager` (`IsPetUnlocked`, mit
  der XBMPet-Id).
- *RSR heute:* Die Beastmaster-Rotation setzte keines der beiden Talente ein. Die Basisschicht gab Anfreunden
  widersprüchliche Bedingungen: `TargetStatusNeed` und `TargetStatusProvide` verlangten beide Interest Captured
  (`ModifyCapturePvE`); das wirkte nicht, solange es niemand rief.

**Was gilt beim Anfreunden** (Option „Capture familiars you do not have yet", ab Werk an; A487):
- *A1 – Wen:* einen Gegner, der nach den Blättern ein Begleiter ist (oben), den du nach `XBMManager` noch nicht hast,
  der nicht schon markiert ist (Interest Captured, 4626), nicht über deiner Stufe steht und höchstens 10 y entfernt
  ist; solange du nicht schon einen markierst (Capturing Interest, 4624). Hat das Spiel die Liste noch nicht geladen
  (`XBMManager.State`), oder lehnt es Anfreunden für dieses Ziel ab (`GetActionStatus` mit dem Ziel, ohne
  Abklingzeit), keiner.
- *A2 – Wann:* wenn seine korrigierte Restzeit nicht länger ist als ein GCD. Dann fällt er voraussichtlich vor deinem
  nächsten GCD, dies ist der letzte, der ihn noch lebend trifft, und seine LP sind die niedrigsten, die ein Markieren
  noch erreicht. Weil Anfreunden nicht tötet, geht es auch bei 1 LP. Ohne Schätzung der Restzeit keine Markierung.
- *Preis:* ein GCD je Versuch, einmal je Gegner einer Art, die du noch nicht hast; misslingt der Pakt, beim nächsten
  dieser Art. Hast du sie, nie wieder.
- *Lagen:* In der Gruppe markiert RSR ebenso; ob der Sieg eines Mitspielers zählt, ist unbelegt, der Preis bleibt ein
  GCD. Unter Stufensynchronisation zählt deine synchronisierte Stufe. Mehrere Bestien: eine Markierung zur Zeit.
- Analysieren braucht die Regel nicht; die Chatmeldung zu lesen entfällt.
- *Gegenhypothesen:* Bedeutet die 5 etwas anderes, versucht RSR es an Gegnern ohne Pakt; das Spiel lehnt das Ziel dann
  ab, oder es kostet einen GCD je Gegner, nicht mehr – der Versuch fällt auf den letzten GCD vor seinem Tod. Kein
  Gewinn, weil Gegner zu schnell fallen? Ohne Restzeitschätzung markiert RSR nicht; das bleibt sichtbar daran, dass
  keine Markierung erscheint.

**Belegung der Hörner in der Rotation** (seine Vorgabe vom 02.11.2026: „wieso vorschlag? es soll in die rotation
eingearbeitet werden, situationsbezogen"; A487, A488):
- *Was die Rotation situationsbezogen tut:* Sie wählt unter den drei Begleitern seiner Hörner nach Lage (B0 bis B3):
  in der Gefahr den, der heilt oder schützt; ohne Gefahr den, dessen Dressur gegen die Gegner im Kampf am meisten
  bringt, für den langen Einsatz.
- *Kein Setzer im Client:* Der Client führt die drei Begleiter der Hörner im `ActionManager` und die freigeschalteten im
  `XBMManager` nur zum Lesen; eine Funktion, einen Agenten oder ein Paket, das die Belegung setzt, kennt
  FFXIVClientStructs nicht (b69073a vom 02.11.2026, nach „XBM", „Beastmaster", „Bestiary", „Pet" durchsucht). Die drei
  Bytes im Speicher zu überschreiben, änderte nur die Anzeige des Clients, nicht, was der Server beim Ruf beschwört.
- *Der Weg über das Bestienbuch* (Daedalus, ofnature/Daedalus e3a16de vom 01.11.2026, `BattlehornAssigner`,
  `BattlehornGame`; Werkzeugquelle, nach eigener Angabe im Spiel aufgezeichnet): Ein Plugin bedient das Buch wie der
  Spieler. Es öffnet das „Bestienbuch des Bändigers" (Hauptkommando, MainCommand 100), blättert über dessen Rückruf
  (`3, Seite`; 25 Begleiter je Seite in der Reihenfolge ihrer Nummer im Buch), öffnet mit `8, Kachel` das Kontextmenü
  eines Begleiters und wählt dort den Eintrag des Horns (Eintrag 0, 1, 2 für Horn I, II, III; Rückruf des Menüs
  `0, Eintrag, 0`). Nach etwa einer Sekunde liest es die Hornplätze zurück; die Zuweisung geht über den Server.
  - Ein Eintrag für das Horn, auf dem der Begleiter schon sitzt, nimmt ihn herunter.
  - Filter oder Sortierung des Buchs verschieben die Kacheln; dann landet die Wahl falsch.
  - Daedalus setzt nur außerhalb des Kampfes und erkennt den Menüeintrag am englischen Wort „Battlehorn".
- *Offen, und darum noch nicht gebaut:*
  - ob der Weg auf seinem deutschen Client gleich läuft: Werte des Rückrufs, Reihenfolge und Wortlaut des Menüs;
  - ob das Spiel ihn im Kampf zulässt; sonst bleibt nur die Zeit zwischen zwei Kämpfen, und deren Lage ist vor dem Pull
    nicht bekannt;
  - was die Rotation dann setzt.
  Die ersten beiden beantwortet die Aufzeichnung (unten) aus seinem Client.
- *Darum kein Vorschlag im Chat* (der erste Entwurf; er will keinen): Bis der Weg auf seinem Client belegt ist, steht
  der Punkt im TODO.

**Aufzeichnung des Bestienbuchs** (sein Auftrag vom 02.11.2026: „bau die aufzeichnung, sie soll sich dann aktivieren,
wenn das buch auf ist, und deaktivieren, wenn es geschlossen wurde"; A488):
- *Wann:* vom Öffnen des Bestienbuchs (`XBMMonsterNotebook` aufgebaut oder gezeigt) bis zum Schließen (abgebaut oder
  verborgen). Sonst schreibt sie nichts; keine Option, sie kostet nur, solange das Buch offen ist.
- *Was, je Zeile mit Uhrzeit, Kampfstatus und den drei Hornplätzen (Nummer im Buch und Begleiter):*
  - jedes Ereignis, das ein Agent empfängt (Agent, Art, alle Werte) – das ist der Rückruf, den ein Klick im Buch oder
    im Menü auslöst;
  - jedes Klickereignis an Bestienbuch und Kontextmenü (Ereignisart, Parameter);
  - jedes Kontextmenü, das aufgeht oder sich erneuert, mit allen Werten, also den Einträgen im Wortlaut des Clients;
  - jede Änderung der Hornplätze, sobald der Server sie bestätigt, und jeder Wechsel des Kampfstatus.
- *Wo:* `BestiaryTrace.log` im Konfigurationsordner von RSR neben `DefenseTrace.log` (der Ordner, den Dalamud dem Plugin
  gibt). Jede Öffnung des Buchs beginnt einen Abschnitt mit Datum, Uhrzeit, Version und Commit; die Datei wächst,
  Löschen setzt sie zurück.
- *Was sie nicht tut:* Sie klickt nichts, sendet nichts und ändert nichts; sie hört nur zu.

**Fremde Werkzeuge zum Beastmaster** (seine Fragen vom 02.11.2026, ob Masterbeast, BeastHelper, BeastmasterAssist und
Daedalus RSR verbessern können; A488; Stand: Kriss-Hietala/BeastmasterAssist db9e2e3 vom 14.09.2026, MIT;
Rin-0617/BeastHelper be40a1e vom 26.10.2026, AGPL-3.0; lexxxgo/Masterbeast 712bddd vom 19.09.2026, ohne Lizenz;
ofnature/Daedalus e3a16de vom 01.11.2026, GPL-3.0 und LGPL-3.0). Alle Werkzeugquellen; Code nur gelesen.
- *Daedalus* (Rotation für alle Jobs, Beastmaster „Artemis"): der Weg über das Bestienbuch (oben); der Hinweis, der
  zu R1 führte; vorgefertigte Belegungen nach Inhalt (Einzelziel, Dungeon-Rudel, Gruppenunterstützung, Leveln) mit
  Begleitern, deren Wert jenseits von Schaden, Heilung und Schutz liegt (Betäubung, Einfrieren, Verwundbarkeit,
  Gruppenstärkung) – B1 wertet das nicht, im TODO. Geprüft ohne Defekt: Daedalus wartet nach der Dressur, bis die
  Aktion des Begleiters gelandet ist, weil eine sofort folgende Axt sonst vor ihr auflöst und die Kombo verkehrt herum
  schließt; in RSR verlangt die Axt nach der Dressur das Herz, das erst die gelandete Dressur gibt (`StatusNeed`, Zweig 1
  des Kompasses). Anfreunden: Daedalus markiert etwa 20 s vor dem geschätzten Tod, um den Tod vor der Markierung zu
  vermeiden; RSR bleibt beim letzten GCD (A2), weil die Chance mit niedrigeren LP beim Markieren steigt (offizieller
  Job-Guide) – der Preis von A2 ist ein Gegner, der schneller fällt als geschätzt.
- *BeastmasterAssist* (Anzeige, keine Automatik): Empfehlungen zu den Aktionen der Kinships (Soul Crush, Scouring Ash,
  Vileskin, Scaleskin, Beastskin, Seedsower, Quelling Wave); RSR setzt alle schon ein. Neu wäre nur „TP für die
  Abschlüsse halten" (ab Stufe 50, 250 TP); ohne Messung, wie viel Potenz die Äxte darunter verlieren, nicht gewertet.
- *BeastHelper, Masterbeast*: auf den Crucible ausgerichtet (Wege über das Brett, Teamwahl über `XBMPetParty`,
  Gegenstände); für die Rotation nichts Neues.
- *WrathCombo* (d9c63e468 vom 27.10.2026): `OnLastHorn` liest die Hörner wie RSR bisher, ohne das Horn des Begleiters
  draußen auszunehmen.

**Begleiter nach Lage: Schaden, Heilung, Schutz** (A485). Sein Auftrag vom 02.11.2026: die Rotation mit eigenen Ideen
so ausbauen, dass die vorhandenen Begleiter passend zu Art und Zahl der Gegner möglichst viel Schaden machen, alle
Eigenschaften des Beastmasters und seiner Begleiter dafür auswerten und Synergien erzeugen. Dazu seine Fragen: ob die
Rotation die Kampfhörner wechselt, wenn ein Begleiter verschwunden ist; ob sich die Zuordnung im Kampf wechseln lässt
(„begleiter ab drei gegnern rufen, der aoes beherrscht", sein Vorschlag); ob Begleiter heilen können, dann auch der
Wechsel bei starkem Verlust an Gesundheit; ob Begleiter tanken und Feindseligkeit halten. Seine Vorgaben vom selben Tag:
Die vorhandenen Begleiter werden im Spiel erhoben, damit RSR keinen zu nutzen versucht, den er noch nicht hat; das Optimum
ist mit den vorhandenen Mitteln zu finden und zu nutzen. Stufe und Fähigkeiten des Spielers zählen mit: Er levelt den
Beastmaster noch, nicht alle Fähigkeiten stehen am Anfang zur Verfügung.

*Was der Beastmaster hat* (Spielblätter über xivapi v2: Action, ActionTransient, Trait, Status, XBMPet, Pet; 02.11.2026):
- *Eigene Kombo:* Smash Axe (160) → Axeblade Bite (500) → Shieldsplitter (600, +15 TP).
- *Instinktäxte:* vier Äxte (Volant, Rampant, Durant, Eldritch), ab 100 TP, 400 bis 1.000 mit den TP, jede gibt das Herz
  ihrer Affinität (7 s). Ab 250 TP (Stufe 50) die Flächenabschlüsse Brutal Rage, Hawkish Talons, Risen Fall, Calamity
  (1.200, Sunstrider oder Moonstalker).
- *Kombos:* Zwei Instinktfertigkeiten – deine oder die des Begleiters – innerhalb von 7 s geben Zusatzschaden. Im
  Uhrzeigersinn (Volant → Rampant → Durant → Eldritch → Volant) ist es eine absichtliche Kombo, Sunstrider mit
  Moonstalker die Kombo Universality; die Kettenzahl erhöht den Zusatzschaden. Eine Kombo mit einer Fertigkeit des
  Begleiters gibt einen Stapel Natural Instinct (Stufe 28), eine mit einer eigenen einen Stapel Mastered Instinct
  (Stufe 40). Rally und Rallying Cheer (120 s, ab Stufe 42 und 48 90 s) wandeln sie in TP und Begleiter-TP.
- *Kampfhörner* (2 s außerhalb, im Kampf 90 s ab dem Rückzug des Begleiters): Ein Ruf gibt One with Nature (Stufe 18):
  einmal Entfesseln oder Borrow. Ab Stufe 30 setzt der Ruf die Abklingzeit von Entfesseln zurück, ab 34 die von Borrow.
- *Dressur* (3 s): die Instinktfertigkeit des Begleiters, ab 100 Begleiter-TP, Attacke-Wert 500 bis 1.250 (bei neun
  Begleitern 450 bis 1.500) mit den Begleiter-TP; sie gibt das Herz seiner Affinität. Begleiter-TP kommen aus seinen
  automatischen Angriffen und dem Ätherstoß (Stufe 8) und aus Rallying Cheer.
- *Entfesseln* (30 s) und *Borrow* (30 s): je Begleiter verschieden (unten). Beide geben ab Stufe 44 und 46 Lingering
  Vantage: der nächste Scheidende Schlag mit 1.500 statt 1.000. Borrow gibt die Kinship seiner Familie für 90 s; ihr
  Ablauf steht still, solange der Begleiter draußen ist. Das Beast Mode dazu hat 90 s Abklingzeit (Scouring Ash 60 s).
  Was Entfesseln auf die Gruppe legt (Lebensentzug 21 s, −20 % Verwundbarkeit 48 s), läuft nach dem Rückzug weiter.
- *Scheidender Schlag* (10 s): Ätherstoß des Begleiters auf Ziel und Umstehende (8 Yalm), danach Rückzug.
- *Shield Charge* (60 s, ab Stufe 36 drei Ladungen): Ansturm, Fläche 300.
- *Stufen* (`Action.ClassJobLevel`, Trait; xivapi v2, 02.11.2026): Kampfhorn I ab 1, Scheidender Schlag 6, Dressur 8,
  Kampfhorn II 10, Entfesseln und One with Nature 18, Kampfhorn III 20, Borrow und Beast Mode 22, Battlehorn Mastery II
  und III 30 und 34. Die Aktionen der Begleiter haben keine eigene Stufe (`ClassJobLevel` 0); sie kommen mit Dressur,
  Entfesseln und Borrow.

*Die 50 Begleiter* (je Begleiter im Blatt `Pet` als `Abilities`: Dressur, Entfesseln, Ätherstoß, Threaten; über
`XBMPet.Pet` zu finden):
- *Dressur, Form:* Einzelziel (etwa Wespe, Coblyn, Mantis, Bat, Colibri, Coeurl, Cobra, Karlabos), Kreis um
  das Ziel (Stone Throw, Drenching Blow, Fireball, Dark Thunder, Bestial Thunder II, Acorn Bomb, Bloody Caress, Cursed
  Sphere, Flying Frenzy …), Kegel (Rake, Screwdriver, Wing Cutter, Ice Breath, Heave, Boulder Clap …) oder Linie
  (Somersault Slash, Fleece Butt, Natural Needles, Beatdown, Vine Probe). Wasser, Feuer, Erde, Wind, Blitz, Eis oder
  elementneutral magisch, sonst physisch (Hieb, Stich, Schlag).
- *Dressur, die heilt:* Digest (Schleim) und Blood Drain (Fledermaus) – ein Teil des Schadens kommt dir als LP zugute.
- *Entfesseln, Schaden:* etwa Foul Breath (Raptor, Kegel 1.000), Bottomless Desert (Wurm, 800 um den Begleiter), Breath
  Wing (Zu, 800), Blighted Bouquet (Rafflesia, 800), Tail Screw (Karlabos, 800 um das Ziel), War Cry (Buffalo, 750 und
  Betäubung), Final Sting (Wespe, 2.000 bis 3.000, danach Rückzug), Curse (Geist, 300 und 1.500 gegen Kranke).
- *Entfesseln, Schwächung:* Element −20 % für 60 s – Bubble Shower (Megalocrab, Wasser), Rockslide (Golem, Erde), Foul
  Breath (Raptor, Feuer), Charged Whisker (Coeurl, Blitz), The Ram's Voice (Chimera, Eis), Breath Wing (Zu, Wind);
  Verwundbarkeit +10 % für 30 s – Eerie Soundwave (Mantis, physisch), Necrotic Nectar (Fliegenfalle) und Peculiar Light
  (Salamander, magisch); Krankheit – Moldy Sneeze (Goobbue), White Breath (Hydra).
- *Entfesseln, Stärkung:* King's Will (Dullahan, +10 % physischer Schaden, 30 s), Frozen Heart (Eisgolem, +25 % magischer
  Schaden, 15 s), Hypodermic Hustle (Sabotender, +20 % Schaden mit Schaden über Zeit, 21 s), Scamper (Eichhörnchen,
  Tempo 15 %, 30 s).
- *Entfesseln, Heilung und Schutz:* Odium (Geshunpest, Lebensentzug für alle, 21 s); Vulcanize (Coblyn) und Harden Shell
  (Adamantoise) −20 % physische, Water Wall (Pugil) und Smoldering Scales (Drache) −20 % magische Verwundbarkeit für 48 s;
  Strut (Dodo) −15 % Erdschaden; Ultrasonics (Fledermaus) reinigt alle.
- *Entfesseln, Kontrolle:* Lullaby (Lamm, Schlaf um den Begleiter), War Cry (Betäubung), Petribreath und Stone Gaze
  (Versteinerung), Silkscreen und Syrup (Fessel), Formic Pheromones und Regurgitate (Verlangsamung), Pins and Nails
  (Lähmung), Bad Breath (alle drei).
- *Borrow nach Familie:* Beastkin – Beastskin (−25 % physischer Schaden, +5 % kritisch, 30 s); Vilekin – Vileskin (Block
  100 %, kein Rückstoß); Cloudkin – Cloud Skim (Bewegung); Seedkin – Seedsower (Gegner −10 % Schaden, Gift); Wavekin –
  Quelling Wave (350, TP); Scalekin – Scaleskin (Barriere 8 % gegen Magie); Soulkin – Soul Crush (Unterbrechung, 1.000);
  Ashkin – Scouring Ash (reinigt, heilt 10 %).

*Seine Fragen, beantwortet:*
- *Wechsel nach einem verschwundenen Begleiter:* Ja; ist keiner draußen, ruft `BST_Reborn` das nächste bereite Horn.
- *Zuordnung im Kampf:* Das Spiel führt die Begleiter der drei Hörner im `ActionManager` („selected Battlehorn pets,
  XBMPet RowIds"), die freigeschalteten im `XBMManager` (50 Bits, `IsPetUnlocked`); FFXIVClientStructs b69073a vom
  02.11.2026, Werkzeugquelle. Eine Funktion, die die Zuordnung setzt, führt es dort nicht; gesetzt wird sie im Menü des
  Bestienbuchs, ob auch im Kampf, ist unbelegt. RSR liest die Zuordnung und ändert sie nicht.
- *Heilen:* ja, über die heilende Dressur (Schleim, Fledermaus) und Odium. Scouring Ash heilt 10 % nur, wenn es einen
  schädlichen Effekt entfernt. Schützen über Beastskin, Scaleskin und Seedsower (Borrow) sowie Vulcanize, Harden Shell,
  Water Wall, Smoldering Scales und Strut (Entfesseln); dazu Kontrolle.
- *Tanken:* nur im Crucible. Dort lässt Snarl den Begleiter mit Threaten die Feindseligkeit ziehen und 45 s deinen
  Schaden auf sich nehmen; Challenge holt sie zurück. RSR hat das als Einstellung „Use Snarl and Challenge …" (ab Werk
  aus, nur Crucible). Außerhalb des Crucible ist kein Weg belegt.
- *Schwächen der Gegner:* keine Datenquelle. Was es gibt, sind die Schwächungen, die ein Begleiter selbst legt.

*Der Zyklus:* Ein Ruf gibt One with Nature, das Horn verbraucht es nach seiner Einstellung für Entfesseln oder Borrow
(„What to use One with Nature on …", ab Werk 1. und 2. Horn Entfesseln, 3. Borrow). Sobald es verbraucht ist und ein
anderes Horn bereitsteht, folgt Scheidender Schlag, dann der nächste Ruf. Steht kein Horn bereit, bleibt der Begleiter,
bis das erste wieder bereit ist (90 s nach seinem Rückzug). Über einen langen Kampf: zwei kurze Einsätze, ein langer, und
so fort. Die Reihenfolge ist eingestellt („First/Second/Third Horn"); die Option unten ordnet die bereiten Hörner nach
Lage.

**Was gilt** (B0 immer; B1 bis B3 unter der Option „Choose the battlehorn by the situation", ab Werk an):
- *B0 – Was er hat, nach Stufe* (seine Vorgaben).
  - RSR ruft nur die Begleiter der drei Hörner, wie das Spiel sie führt. Das Spiel belegt ein Horn nur mit einem
    Begleiter, den er hat. Ein leeres Horn zählt nicht, ebenso eines, das das Spiel nicht freigibt (Stufe, Abklingzeit).
  - Ein Horn wartet mit dem Scheidenden Schlag nur auf das, wofür es One with Nature verbraucht, und nur, solange diese
    Aktion kommen kann: Der Spieler hat sie (Stufe, Freischaltung), sie ist in RSR eingeschaltet, und bei Borrow läuft
    nicht schon die Kinship dieses Horns (die verweigert Borrow, und ihr Ablauf steht, solange der Begleiter draußen
    ist). Steht es auf Borrow, und Borrow kann nicht kommen (Stufe 18 bis 21, abgeschaltet), verbraucht es One with
    Nature für Entfesseln; auf „Neither" wartet es auf nichts. Bisher wartete jedes Horn, bis weder Entfesseln noch Borrow bereit war. One with Nature blieb dann
    stehen und hielt Entfesseln bereit, also kam kein Scheidender Schlag: Der Begleiter blieb draußen, die anderen
    Hörner ruhten (ab Werk das dritte Horn auf Stufe 20 und 21; auf „Neither" auf jeder Stufe ab 18). Ein Defekt, keine
    Wahl; deshalb nicht hinter der Option.
  - *R1 – Abtreten nur mit Ersatz* (A488). Zwei Aktionen lassen den Begleiter abtreten: Scheidender Schlag immer;
    Entfesseln bei einem Begleiter, dessen Entfesseln es laut Wirktext tut („Familiar retreats from combat upon
    execution"; heute nur Wespes Schlussstich, xivapi v2 Action 44954, language=de: 2.000 bis 3.000, steigend mit
    sinkenden LP des Ziels). Im Kampf beginnt die Abklingzeit eines Horns erst mit dem Rückzug seines Begleiters
    (Wirktext der Hörner) und dauert 90 s (consolegameswiki, BeastmasterAssist; Daedalus nennt 70 s allein nach drei
    Rückzügen in 40 s – Werkzeugquellen; das Blatt führt 2 s, den Wert außerhalb des Kampfes). Das Horn des Begleiters
    draußen ist deshalb nie sein Ersatz.
    - *Gilt:* RSR setzt beide nur, wenn ein anderes Horn mit einem Begleiter bereit ist. Ist die Belegung nicht lesbar
      (alle drei Plätze leer, obwohl einer draußen ist), zählt jedes andere bereite Horn. Schlussstich wartet dann, Wespe
      kämpft weiter und geht, sobald ein anderes Horn bereit ist; der Scheidende Schlag wartet ohnehin auf Entfesseln.
      „Only use Snarl and Challenge on Last Horn" liest dieselbe Frage: der letzte Begleiter heißt, kein anderes Horn
      ist bereit.
    - *Bisher:* Für den Scheidenden Schlag genügte irgendein bereites Horn, Schlussstich prüfte keines. Meldet das Spiel
      das Horn des Begleiters draußen als bereit – seine Abklingzeit läuft ja noch nicht –, ging er mit 1.000 oder 1.500,
      und du standest bis zu 90 s ohne Begleiter: keine Dressur, keine Kombo, kein Entfesseln, keine Heilung aus ihm.
      Ob das Spiel es so meldet, ist unbelegt; die Bedingung ist in beiden Fällen richtig, für Schlussstich ohnehin.
    - *Preis:* Am Ende eines Kampfes wäre ein Abtreten ohne Ersatz reiner Gewinn (bis 1.500 oder 3.000); RSR kennt das
      Kampfende nicht verlässlich – die Restzeit eines Ziels sagt nichts über Nachzügler. Unter Stufe 10, mit einem
      Horn, kein Scheidender Schlag.
- *B1 – Katalog aus den Spieldaten.* Je Begleiter auf einem Horn: Dressur und Entfesseln aus `Pet.Abilities` (erste und
  zweite Aktion). Zum Begleiter führt `XBMPet`; die Spalte mit seiner Pet-Zeile hat in Lumina keinen Namen. Gelesen wird die
  der beiden Zahlenspalten, deren Werte über alle Begleiter verschieden sind – die Pet-Ids (47 bis 97) sind es, die
  Fundorte nicht – und deren Pet-Zeile Aktionen führt. Trifft das auf keine oder beide zu, gibt es keinen Katalog, und die
  eingestellte Reihenfolge gilt. Form und Reichweite der Dressur aus dem Aktionsblatt (`CastType`, `EffectRange`),
  Attacke-Wert aus dem Tooltip (`ActionTooltip`, der erste genannte, auf der Stufe des Spielers). Die Zuordnung aus den
  Blättern entsteht einmal; was ein Begleiter bringt, liest RSR erst für die Begleiter auf den Hörnern und neu mit jeder
  Stufe, auch einer synchronisierten: beim Leveln ändert sich, was eine Aktion bringt. RSR drückt die Dressur,
  sobald 100 Begleiter-TP stehen; der Mindestwert ist deshalb der, den sie meist bringt (Schluss aus dem Code, nicht
  gemessen).
  - *Heilt:* die Dressur („absorbed as beastmaster HP") oder das Entfesseln („Life Siphon").
  - *Schützt:* das Entfesseln („reducing nearby party members' … vulnerability", „… damage taken") oder das Beast Mode
    seiner Familie („damage taken", „absorbs", „Reduces damage dealt").
  - Was zählt, Entfesseln oder Borrow, folgt der Einstellung seines Horns (nach B0). Keine Tabelle im Code.
  - Es zählt nur, was der Spieler hat: die Dressur ab Stufe 8, Entfesseln ab 18, Borrow ab 22, jeweils mit der
    Freischaltung, wie RSR sie für jede Aktion prüft. Unter 8 bringt kein Begleiter eine Dressur; alle sind gleich, und
    die eingestellte Reihenfolge entscheidet. Entfesseln und Borrow zählen außerdem nur, wenn sie mit dem Ruf kommen: ab
    Stufe 30 und 34 setzt der Ruf ihre Abklingzeit zurück, darunter nur, wenn sie nicht gerade abklingen. Sonst käme der
    Schutz bis zu 30 s nach dem Ruf.
  - *Nicht gezählt:* Vileskin (Block 100 %, 15 s): Wie viel ein Block beim Beastmaster abfängt, ist unbelegt, und RSR
    hält Vileskin für einen Rückstoß. Scouring Ash: heilt nur, wenn es einen schädlichen Effekt entfernt.
- *B2 – Gefahr zuerst.* Fällt deine vorausberechnete Gesundheit unter die Linie, an der RSR dich mit Fähigkeiten heilt
  („HealthSingleAbility", gerechnet wie dort: bis zu einer sofort landenden Heilung), oder stehst du in Klasse 1:
  - RSR ruft unter den bereiten Hörnern zuerst einen Begleiter, der heilt oder schützt.
  - Steht einer draußen, der weder heilt noch schützt, und ein solches Horn ist bereit, geht er mit Scheidender Schlag
    (1.000 oder 1.500), und der heilende kommt. Nicht, solange er nach Snarl deinen Schaden trägt (Crucible, Status
    „Covered"): Dann nimmt er ihn dir gerade ab.
  - Einer, dessen Dressur heilt, bleibt draußen, solange die Gefahr anhält: kein Scheidender Schlag für den Zyklus. Seine
    Heilung kommt nur, solange er draußen ist. Was Entfesseln oder Borrow bringt, läuft nach dem Rückzug weiter (oben);
    ein Begleiter, der so schützt, geht wie im Zyklus, und der nächste Ruf ist wieder einer, der heilt oder schützt.
  - Scaleskin (Barriere gegen Magie) setzt RSR in der Gefahr sofort ein, sonst nur für einen angekündigten Treffer oder
    ohne Heiler. Beastskin und Seedsower setzt es ohnehin ein, sobald Borrow sie freigibt.
  - *Messgröße der Wahrscheinlichkeit:* deine vorausberechnete Gesundheit (`GetForecastHealthRatio`, sofort landend wie
    bei Second Wind), also die gemessene Schadensrate.
- *B3 – Der Begleiter, der bleibt, zuletzt.* Ohne Gefahr ruft RSR unter mehreren bereiten Hörnern zuerst den, dessen
  Dressur gegen die Gegner um das Ziel am wenigsten bringt.
  - Der zuletzt gerufene bleibt, bis das erste Horn zurück ist (der lange Einsatz). Die anderen gehen nach Entfesseln oder
    Borrow und Scheidender Schlag.
  - Wert der Dressur = Treffer × Attacke-Wert; Treffer: 1 bei Einzelziel, sonst die Gegner im Kampf im Wirkradius um
    das Ziel, mindestens das Ziel. Eine ungezogene Gruppe daneben zählt nicht.
  - Bei gleichem Wert behält ein Begleiter, der heilt oder schützt, den langen Einsatz; danach die eingestellte
    Reihenfolge. Bei einem Gegner heißt das meist: der heilende bleibt, ohne Schaden zu kosten.
- Jede Wahl und jeder Wechsel aus Gefahr schreibt eine Zeile ins `DefenseTrace.log`: Horn, Begleiter, Grund.
- *Zustandszeile* (A489): Sobald sich ändert, was diese Regeln und das Anfreunden lesen, schreibt RSR eine Zeile
  „beastmaster state": Begleiter der drei Hörner, wie RSR sie liest; das Horn draußen; die Optionen „Choose the
  battlehorn by the situation" und „Capture familiars you do not have yet"; ob Anfreunden eingeschaltet ist; ob das
  Spiel die Liste der Begleiter geladen hat; Stand des Katalogs. Anlass: Am 02.11.2026 rief RSR in einer Stunde mit
  allen Regeln (b83dc3ce9) 28 Hörner, ohne dass eine Regel eine Zeile schrieb oder den Katalog anlegte. Ob die Hörner
  leer gelesen wurden, eine Option aus war oder Anfreunden gesperrt, sagen die Dateien nicht; die Zeile sagt es beim
  nächsten Kampf.
- *Welche Rotation läuft* (A490, für jeden Job): Ebenso möglich ist, dass gar nicht BST_Reborn lief, sondern eine
  Beastmaster-Rotation aus einer geladenen Bibliothek; sie drückte dieselben Hörner, ohne eine dieser Zeilen. Darum
  schreibt RSR bei jedem Wechsel der Rotation eine Zeile „rotation": Name, Bibliothek, Job und PvE oder PvP. Fehlt
  dann die Zustandszeile, obwohl BST_Reborn genannt ist, erreicht die Rotation `GeneralGCD` nicht.

*Gerechnet* (`bst_horn_model.py`, Annahmen dort benannt: Dressur alle 7,5 s, kurzer Einsatz 5 s, eigener Schaden 200 je
Sekunde, alle Gegner eines Rudels im Wirkradius):
- Gegen die feste Reihenfolge verliert B1 bis B3 nie Schaden. Bei einem Rudel, wenn der Flächenbegleiter sonst nicht
  zuletzt käme, sind es 4,7 % (Rudel für 40 s) bis 12,9 % (Rudel durchgehend) mehr.
- In einer Gefahr ab 40 s ist der heilende Begleiter sofort draußen; mit der festen Reihenfolge nach 55 s, wenn er zuerst
  dran war. Beginnt die Gefahr erst, nachdem sein Horn in die Abklingzeit ging (im Modell ab 100 s), hilft keine
  Reihenfolge.
- Anteil einer Gefahr bis Kampfende, in dem ein heilender Begleiter draußen ist oder ein Schutz aus Entfesseln läuft
  (Hörner Schleim, Wespe, Coblyn und ihre Umstellungen, ein Gegner): B2 hält 60 bis 100 %, die feste Reihenfolge 0 bis
  100 %. Hielte B2 jeden schützenden Begleiter draußen, fiele der Anteil in drei von neun Fällen auf 13 bis 27 %: Sein
  Schutz läuft nach 48 s ab, und der heilende kommt nicht. Ein Fall geht gegen B2 aus (60 statt 100 %): B3 gibt dem
  sicheren Begleiter den langen Einsatz, und beginnt die Gefahr kurz nach dessen Ende, liegt sein Horn in der
  Abklingzeit.
- Preis in Schaden: Bleibt der heilende Begleiter in einer Gefahr ab Kampfbeginn draußen, fehlen die kurzen Einsätze der
  anderen, bis zu 12,6 % Attacke-Wert über den Kampf.

*Warum so:*
- Die Hornwahl ist der einzige Hebel über die Begleiter; Entfesseln oder Borrow bleibt seine Einstellung je Horn, ihr Text
  bindet.
- Sicherheit vor Schaden: Als begrenzter Job spielt er den Beastmaster allein (Crucible, offene Welt); einen Heiler gibt
  es dort nicht, die heilenden Begleiter sind seine Heilung.
- Der Wechsel kostet: Der laufende Begleiter verliert seinen Rest, sein Horn geht 90 s in die Abklingzeit. Deshalb nur aus
  Gefahr, nicht für Schaden.

*Lagen:* Pull ohne Begleiter (alle drei bereit): B3 ordnet alle drei. Ein Rudel bei langem Einsatz: der lange Einsatz
gehört dem Flächenbegleiter, wenn er beim Rufen erkannt ist; ein Wechsel nur für Fläche findet nicht statt. Beim Leveln:
unter 10 ein Horn, unter 20 zwei, keine Wahl bei einem; unter 8 keine Dressur, alle Begleiter gleich; unter 18 kein One
with Nature, also kein Entfesseln und kein Borrow, „sicher" heißt dann nur eine heilende Dressur; unter 22 kein Borrow, ein
Horn darauf entfesselt (B0); unter 30 und 34 setzt ein Ruf die Abklingzeit von Entfesseln und Borrow nicht zurück, und das
Horn wartet auf seine Aktion, höchstens deren 30 s, statt One with Nature verfallen zu lassen. Bisher wartete es meist
schon, weil die andere Aktion unter One with Nature bereit stand; ging es ohne, weil beide abklangen, verfiel One with
Nature. Über einen Zyklus der drei Hörner sind das drei Entfesseln statt eines (Schluss aus dem Code, nicht gemessen). Stufensynchronisation: dieselben Grenzen auf der synchronisierten Stufe.
Option aus: die eingestellte Reihenfolge wie bisher, B0 gilt. BossModReborn liest die Regel nicht.

*Belegung der Hörner:* RSR wählt unter den drei, die er im Bestienbuch zuordnet; welche drei das sind, entscheidet die
Wahl am meisten. Für Rudel (Dungeons beim Leveln) einer mit Flächendressur, allein in der offenen Welt einer, dessen
Dressur heilt, oder einer, der mit Entfesseln oder Borrow schützt. Die Belegung selbst setzt RSR heute nicht (unten,
„Belegung der Hörner in der Rotation").

*Preis:* Die Wahl liest nur, was beim Rufen steht. Ändert sich die Zahl der Gegner im langen Einsatz, bleibt der
Begleiter. Kegel und Linie zählt B3 wie einen Kreis um das Ziel (die Stellung des Begleiters ist beim Rufen unbekannt) –
bei verteilten Gegnern zu viele Treffer. Bleibt ein heilender Begleiter in der Gefahr draußen, fehlen in dieser Zeit die
kurzen Einsätze der anderen (Entfesseln, Scheidender Schlag).

*Verworfen:*
- Die Zuordnung im Kampf ändern: Das geht nur über das Menü des Bestienbuchs, und ob es im Kampf geht, ist offen
  (unten, „Belegung der Hörner in der Rotation").
- Für Fläche mitten im Einsatz wechseln: kostet den Rest des laufenden und 90 s Horn.
- Eine feste Drei als Schwelle (sein Vorschlag): Die Dressur trifft je Gegner gleich stark; schon ab zwei Gegnern im
  Wirkradius bringt die Flächendressur mehr. Die Drei bleibt als sein Beispiel.
- *B4, Schwächung oder Stärkung zuerst* (Entfesseln, das die Dressur eines anderen trifft oder dich stärkt): gerechnet
  zwischen −1,8 % (ein Gegner) und +1,3 %. Die geänderte Reihenfolge verschiebt den Zyklus der Hörner mehr, als die 20 % auf
  eine Dressur bringen.
- Jeden Begleiter, der heilt oder schützt, in der Gefahr draußen halten (der erste Entwurf von B2): Ein Schutz aus
  Entfesseln läuft nach 48 s ab, und solange er draußen bleibt, kommt kein anderer (Gerechnet).
- Vileskin in der Gefahr: Was ein Block beim Beastmaster abfängt, ist unbelegt; für einen Rückstoß fehlte es dann 90 s.
- Den heilenden Begleiter als Reserve zurückhalten: Der Zyklus ruft jedes bereite Horn; zurückhalten hieße, ihn ganz zu
  sparen, und kostete je 90 s einen kurzen Einsatz.
- Schwächen der Gegner lernen: keine Datenquelle; aus eigenen Treffern je Element denkbar, später.
- Eine Tabelle der 50 Begleiter im Code: veraltet mit jedem Patch, die Spielblätter haben alles.

*Gegenhypothesen:*
- Kein Gewinn: Bei einem Gegner ändert sich am Schaden nichts; gewonnen wird dort nur in der Gefahr (B2). Bei mehreren
  rechnet das Modell.
- Option falsch: siehe Verworfen.
- Ausgeliefert, und nichts ändert sich – warum? Hat er nur Einzelziel-Begleiter zugeordnet oder keinen heilenden, wählt die
  Regel nichts anderes; findet der Katalog die Pet-Spalte nicht, gilt die eingestellte Reihenfolge. Die Trace-Zeile nennt
  beides. Unter Stufe 8 wirkt nur B0 und die Reihenfolge; unter 18 schützt kein Begleiter, nur die heilende Dressur
  zählt.

**Weben, Ansturm und Abschlüsse** (A486). Seine Frage vom 02.11.2026: ob oGCDs und GCDs richtig gemischt werden, ohne
Zeitverlust, mit Weben; danach die Rotation weiter analysieren und verbessern; dazu, ob WrathCombo Erkenntnisse hat.

*Was das Spiel festlegt* (Action, ActionTransient; xivapi v2, 02.11.2026):
- *GCD* (Abklinggruppe 58, 2,5 s): Smash Axe, Axeblade Bite, Shieldsplitter, Quelling Wave (Zauber), Capture.
- *Instinktäxte und ihre Abschlüsse:* Waffenfertigkeiten in eigener Gruppe 16 (5 s); „Instinctual skills do not share a
  recast timer with any other actions". Jede Axt wird bei 250 TP (ab Stufe 50) zu ihrem Abschluss: Avalanche Axe zu
  Brutal Rage, Mistral Axe zu Hawkish Talons, Spinning Axe zu Risen Fall, Gale Axe zu Calamity; alle vier 1.200.
- *Fähigkeiten:* Dressur (3 s), Scheidender Schlag (10 s), Entfesseln, Borrow, Beast Mode, Shield Charge (drei Ladungen),
  Rally, Rallying Cheer. Die Kampfhörner sind Fähigkeiten mit 1 s Wirkzeit, die einzigen Spieler-Fähigkeiten mit
  Wirkzeit im PvE (Abfrage über alle Aktionen).
- *Ansturm mit Schaden* (`AffectsPosition`, „Rushes forward"): Shield Charge (20 y, 300 um das Ziel), Brutal Rage und
  Hawkish Talons (25 y, 1.200 um das Ziel). Risen Fall ist Brutal Rage ohne Ansturm: gleicher Wert, gleiche Affinität
  (Sunstrider), gleicher Kreis von 6 y um das Ziel, gleiche Reichweite 25 y. Calamity ist der Moonstalker-Abschluss ohne
  Ansturm, aber auf einer Linie von 10 y vor dir statt im Kreis um das Ziel.

*Wie RSR mischt und webt:* Es ordnet nach Abklinggruppe, nicht nach Aktionsart (`IsGeneralGCD`: Gruppe 58). Die Äxte und
Abschlüsse sind also oGCDs und stehen in `AttackAbility`, die Kombo und Quelling Wave in `GeneralGCD`, die übrigen
Fähigkeiten in den Fähigkeitspfaden. Zwischen zwei GCDs drückt RSR Fähigkeiten, sobald die Animationssperre frei ist,
nicht in der letzten halben Sekunde vor dem GCD (`CustomRotation.Ability`, `RSCommands`; Upstreams Fenster, A388). Das
gilt für jeden Job.

*Befunde und was gilt:*
- *W1 – Kampfhorn verschiebt den GCD.* Das Fenster misst den Beginn einer Fähigkeit, nicht ihr Ende. Ein Horn, 0,6 s vor
  dem GCD gerufen, wirkt 1 s und verschiebt den GCD um rund eine halbe Sekunde; das widerspricht dem Zweck des Fensters
  („so an ability does not clip it"). *Gilt:* Eine Fähigkeit mit Wirkzeit geht zwischen zwei GCDs nur, wenn ihre
  Wirkzeit vor dem freien Fenster endet; zentral für jede Aktion im PvE, heute nur die Hörner (im PvP hat nur das
  Elixier Wirkzeit, 4,5 s, und bleibt, wie es war). Läuft kein GCD, oder fand RSR keinen (das Fenster gilt dann ohnehin
  nicht), gilt die Grenze nicht. *Ausnahme:* Ruft B2 in der Gefahr ein Horn, das heilt oder schützt, darf es in den GCD
  laufen – Sicherheit vor Schaden: eine Drittelsekunde GCD gegen bis zu einen GCD früher Heilung. *Preis:* Ein Horn, das im zweiten Webfenster
  fällig wird, kommt nach dem nächsten GCD, der Begleiter bis zu einem GCD später. Ob der Begleiter in dieser Zeit mehr
  bringt als der verschobene GCD, ist unbelegt: Die Blätter nennen keinen Wert für seine automatischen Angriffe, und
  seine Mitschnitte enthalten keinen Beastmaster-Kampf von ihm. Die Regel folgt der bestehenden Absicht, dass eine
  Fähigkeit den GCD nicht verschiebt.
- *W2 – Ansturm ohne Abstandsgrenze.* Shield Charge, Brutal Rage und Hawkish Talons gingen auf jeden Abstand bis 20 und
  25 y. Das widerspricht dem Text der Einstellung „Only use damaging gap closers within this distance" (bindet; A358 hat
  die Klasse für seine damaligen Jobs geschlossen, der Beastmaster kam erst am 31.10.2026 in sein Profil). Im Kampf:
  Steht er für eine Mechanik 15 y vom Boss, trägt ihn Brutal Rage bei 250 TP zurück in die Fläche. *Gilt:* in jeder
  Beastmaster-Rotation über `GapCloserAllowed` wie Intervene: Ziel näher als die Einstellung (ab Werk 3 y), oder er hat
  die Aktion selbst gedrückt, Vorwärts-Befehl, Countdown. Dazu der Typ `HostileMovingAttack`, der den Landepunkt gegen
  bekannte Gefahrenflächen prüft.
- *W3 – Abschluss ohne Bewegung zuerst.* Risen Fall vor Brutal Rage: gleicher Wert, kein Schritt. Zwischen Calamity und
  Hawkish Talons der, der mehr Gegner trifft (`AffectedTargets` der Zielwahl), bei Gleichstand Calamity. Die Reihenfolge
  der Affinitäten bleibt (Sunstrider vor Moonstalker); die Universality-Kombo bleibt, wo sie war. Steht er weiter als
  10 y, geht Risen Fall aus 25 y; nur Hawkish Talons wartet dann auf die Grenze aus W2.
- *W4 – Soul Crush unterbricht.* Er bringt 300, beim Unterbrechen 1.000 und 100 TP („Potency increases to 1,000 … upon
  interruption"). RSR setzte ihn nur in `AttackAbility` auf das Ziel. *Gilt:* zuerst im Unterbrechungspfad auf einen
  Wirker, dessen Wirken sich unterbrechen lässt; sonst wie bisher. Zurückgehalten für eine Unterbrechung wird er nicht:
  Wie oft in 30 s eine kommt, hängt am Inhalt, und jede verpasste Abklingzeit kostet 300.

*Was WrathCombo hat* (PunishXIV/WrathCombo d9c63e468 vom 27.10.2026, BST-Dateien zuletzt am 28.09.2026 geändert;
Werkzeugquelle, nur gelesen):
- Abschlüsse nur Risen Fall und Calamity, Brutal Rage und Hawkish Talons nie; Shield Charge nur in Nahkampfreichweite.
  Stützt W2 und W3 unabhängig.
- Die Affinität jeder Dressur als Tabelle von 50 Aktionen; daraus Dressur und Axt im Uhrzeigersinn in beide Richtungen
  (Dressur zuerst oder als zweite). RSR setzt die Dressur, sobald irgendein Herz steht, ohne ihre Affinität; die steht im
  Tooltip („Instinctual Affinity: Eldritch"). Umgesetzt als K1 (unten, „Kompass: Dressur und Axt im
  Uhrzeigersinn").
- Anfreunden: ein Ziel als Begleiter erkannt über `BNpcBase` und Modell, freigeschaltet über
  `XBMManager.IsPetUnlocked`, laufender Versuch am Status „Interest Captured" (4626). Umgesetzt in A1 (oben, „Was gilt
  beim Anfreunden"); WrathComboʼs Weg über `Pet.AllowedPetMirage` heißt in der gebauten Lumina `Pet.Unknown8`.
- Entfesseln nicht für Wespe (Final Sting), ein Burst aus Dressur, Axt, Rally und Rallying Cheer, eine Ladung Shield Charge
  zurückgehalten: ohne Begründung in der Quelle, nicht übernommen.

*Gegenhypothesen:* Kein Defekt bei W1? Der Rhythmus aus Beginn plus Wirkzeit ist Rechnung, kein Mitschnitt; ein Horn am
Beginn des Fensters verschiebt nichts und bleibt erlaubt. W2 falsch, weil der Ansturm Schaden bringt? Den Wert hat Risen
Fall ohne Ansturm; nur Hawkish Talons über 10 y wartet. Ausgeliefert, und nichts ändert sich? Steht er im Nahkampf, ändert
W2 nichts; die Wahl von W3 schreibt keine Zeile, sie ist ohne Bewegung gleichwertig.

**Kompass: Dressur und Axt im Uhrzeigersinn** (A487; sein Auftrag vom 02.11.2026, das TODO nach Priorität
abzuarbeiten).

*Was das Spiel festlegt* (Job-Guide na, „Instinctual Combos", abgerufen 02.11.2026; Wirktexte der Äxte, der Dressur und
der Traits Wild Heart III und IV, xivapi v2):
- Zwei Instinktfertigkeiten – deine oder die des Begleiters – innerhalb von 7 s schließen eine Kombo mit Zusatzschaden.
- Folgt die zweite im Uhrzeigersinn auf die erste (Volant → Rampant → Durant → Eldritch → Volant), ist es eine
  absichtliche Kombo: „greater additional damage", dazu leuchtet Sunstrider oder Moonstalker; beide Seiten geben
  Universality. Eine höhere Kettenzahl erhöht den Zusatzschaden.
- Schließt der Begleiter die Kombo, gibt es einen Stapel Natural Instinct, schließt deine Axt, einen Stapel Mastered
  Instinct.
- Die Äxte: Avalanche Rampant, Mistral Durant, Spinning Eldritch, Gale Volant. Die Dressur trägt die Affinität ihres
  Begleiters, im Tooltip „Instinctual Affinity" (Digest: Eldritch).
- Wie viel Zusatzschaden eine Kombo bringt, nennt kein Text; die Kombo-Aktionen (44910–44916) haben keinen Wirktext.

*Wie RSR heute verkettet* (`BST_Reborn.AttackAbility`):
- Zweig 1, wenn Mastered Instinct fehlt (deine Axt soll schließen): Dressur, dann die Axt, deren Bedingung das
  stehende Herz ist – also die im Uhrzeigersinn folgende. Absichtlich.
- Zweig 2, wenn Natural Instinct fehlt (der Begleiter soll schließen): eine Axt ohne Blick auf das Herz, in fester
  Reihenfolge Gale, Spinning, Mistral, Avalanche, danach die Dressur. Absichtlich wird das nur zufällig: nach Gale nur
  mit einem Rampant-Begleiter, und Gale selbst nur nach einem Eldritch-Herz.

*Was gilt (K1, ohne Option):* In Zweig 2 die Axt, die den Kompass im Uhrzeigersinn hält:
- Steht ein Herz, die Axt, die ihm folgt (ihre Bedingung ist dieses Herz).
- Steht keines, die Axt vor der Affinität der Dressur, damit die Dressur danach absichtlich schließt: vor Eldritch
  Mistral, vor Volant Spinning, vor Rampant Gale, vor Durant Avalanche.
- Fehlt diese Axt (Stufe), oder ist die Affinität unbekannt (kein Katalog), die bisherige Reihenfolge.
- Kosten wie bisher: eine Axt, dieselben TP, dieselbe Abklingzeit; nur welche der vier.

*Gerechnet* (`bst_compass_model.py`, erschöpfend über jedes Herz und jede Affinität, jede Stufe der Äxte): K1 bringt in
jedem Fall mindestens so viele absichtliche Glieder wie die feste Reihenfolge, in 12 von 20 Fällen eines mehr (ohne Herz
und Gale als Starter waren es nur bei einem Rampant-Begleiter zwei). Nichts wird schlechter, auch nicht mit fehlenden
Äxten.

*Verworfen:*
- Zweig 1 umdrehen (erst die Axt, die dem Herz folgt, dann die Dressur): gleiche Erwartung an absichtlichen Gliedern
  (eines sicher, eines zu einem Viertel), aber ein anderer Stapel – kein Gewinn, belegt durch dasselbe Modell.
- Die Dressur zurückhalten, bis sie absichtlich schließt: Die Kette bricht nach 7 s, und den Wert der Kettenzahl
  gegen den einer absichtlichen Kombo nennt kein Text.
- Eine Tabelle der 50 Affinitäten (WrathCombo): Der Tooltip hat sie.

*Gegenhypothesen:* Kein Gewinn, weil der Unterschied klein ist? Den Betrag nennt kein Text; K1 kostet nichts und verliert
nirgends. Ausgeliefert, und nichts ändert sich? Mit einem Rampant-Begleiter war Gale schon die richtige Axt; ohne Katalog
bleibt die alte Reihenfolge.

---

## Quantitative Auffälligkeiten

| Kennzahl | Wert | Bedeutung |
|---|---|---|
| Rotationsdateien mit `GeneralGCD`-Override | 24 / 24 | einziger wirklich universeller Hook |
| Rotationsdateien mit `AttackAbility`-Override | 24 / 24 | zweiter universeller Hook |
| Jobs ohne `CountDownAction` | 2 (DRG, VPR) | keine Lücke: kein Vorlauf, den seine Regeln zulassen (`04`, „Countdown und Notfallslot“) |
| Jobs ohne `EmergencyAbility` in der Reborn-Datei | 3 (RPR, SAM, WAR) | keine Lücke: WAR in der Basisschicht, RPR/SAM ohne eigene Notfallaktion (Konzept 04) |
| Echte Aufstiegsketten | 65 in 16 Dateien, davon 25 sauber gleichförmig (Zählung vom 20.08.2026) | siehe 04, Punkt A2 |
| Redundantes `X.EnoughLevel && X.CanUse` | 56, davon 44 außerhalb `ExtraRotations` | `CanUse` prüft das bereits selbst |
| Swiftcast/Raise-Kurzschluss | 13 Verwendungen von `SwiftRaisePending`, vier gleichlautende private Definitionen (eine je Heiler) | siehe 04, B1 |
| Spannweite Dateigröße | 199 (BLM_RP) – 1433 (SMN) LOC, ohne Duty-Rotationen | Faktor 7 |
| Spannweite Configs | 3 – 26 | Faktor 9 |
