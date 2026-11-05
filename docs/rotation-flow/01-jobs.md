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

**Zielart:** Ohne Tank und Heiler neben ihm greift der Blaumagier den Gegner mit den wenigsten LP zuerst an, wie der
Beastmaster; ein Blaumagier neben ihm zählt in der Rolle seiner Mimikry (Konzept 03, „Zielart ohne Tank und Heiler";
A526).

**Blaumagier: Konzept für Rotation, Synergien und Set** (sein Auftrag vom 03.11.2026: „die blaumagier rotation scheint
nicht zu funktionieren. es wird meist nur der kaktor spell genutzt, was bei highlevel sinnfrei ist … erstelle konzept im
loop"; dazu seine Vorgaben „nutze alle gelernten spells optimal aus, suche synergieeffekte" und „finde auch weitere
sinnvolle kombinationen und rotationen anhand der wirkungen der spells"; A506). Mit dem Auftrag gehört der Blaumagier
zu seinem Profil. Namen: deutsch nach den Spielblättern (xivapi, `language=de`, Zeilen-Id), Bezeichner englisch.

*Quellen* (Stand 03.11.2026):
- **Spielblätter:** alle 125 Blaumagier-Aktionen mit Wirktext, Wirk- und Abklingzeit, Reichweite und Abklinggruppe
  (xivapi). Die Werte unten stammen daraus.
- **Blue Academy** (`blue-academy.pages.dev`, „Updated for Patch 7.5 (May 16, 2026)"): Spielerquelle für den Blaumagier,
  wie The Balance für die anderen Jobs.
- **WrathCombo** 1e64c866c vom 03.11.2026, BossMod `ffxiv_bossmod` 3bbbdf5e0 vom 02.11.2026 (Rotation „xan BLU", als WIP
  geführt), UltimateCombo 7fa9359 vom 04.10.2026 (StackBLU): Werkzeugquellen, Code nur gelesen.
- **consolegameswiki**, „Blue Mage": Regeln des Sets, Stufe 80.

**Befund vor Stufe 1** (am Code von `BLU_Reborn` und `BlueMageRotation` bis A506; behoben mit A507, Regeln unter „Wert
statt Reihenfolge" und „Mondflöte"):
- *D1, 1000 Nadeln* (Id 11397, vom Kaktor gelernt; vermutlich sein „Kaktor-Zauber"):
  - Er stand in `UseSituational` vor allen Füllzaubern. Seine einzige Bedingung war, dass das Ziel mindestens 1000 HP
    hat; auf hoher Stufe trifft das auf jedes Ziel zu.
  - Laut Blatt wirkt er 6 s und macht feste 1000 Schaden, aufgeteilt auf alle Gegner in 4 Yalm. Schallwelle (Sonic
    Boom, 18308) wirkt 1 s mit Potenz 210.
  - Er lohnt nur, solange 2,4 GCDs Füllzauber weniger als 1000 Schaden machen, also weniger als rund 417 Schaden je
    GCD; das ist nur ganz unten in den Stufen der Fall.
  - Dieselbe Klasse ohne Wertvergleich: Rakete (Missile, 50 % der aktuellen HP, geringe Chance), Raketenwerfer
    (Launcher), Dimensionsverschiebung (Dimensional Shift, 5 s), Schweifschraube, Verhängnis, Todesstufe 5.
  - Rakete ging ohne den Schalter „Use Low Chance abilities", obwohl ihr Wirktext die geringe Chance nennt; der
    Raketenwerfer, dessen Text keine nennt, stand darunter.
- *D2, Mondflöte* (11415): Sie stand in `UseUtilityAndDebuffs` unter nur `InCombat`.
  - Sie gibt 15 s Waxing Nocturne (+50 % Schaden), danach 15 s Waning Nocturne. In dieser Zeit gibt es keine Aktion:
    keine Heilung, keine Abwehr, keine Wiederbelebung.
  - RSR zündete sie neu, sobald Waning endete. Ohne gesammelten Burst ist das ein Wechsel aus 15 s mit 150 % und 15 s mit
    0 %, im Mittel 75 % des Schadens.
  - Rechnung: Ohne Burst bringt ein Fenster +630 Potenz und kostet 1470 (sechs GCDs Waning und den eigenen GCD), also
    −840 je Fenster.
  - Das wiegt schwerer als D1. Die Hälfte der Kampfzeit kann der Blaumagier nichts tun.
- *D3, die beiden Rösselsprünge:*
  - Laut Blatt verstärkt Weißer Rösselsprung (18310) gegen Bind und setzt Slow. Schwarzer Rösselsprung (18311)
    verstärkt gegen Slow und setzt Bind.
  - In `BlueMageRotation` las Weißer Rösselsprung Blind statt Bind, und Schwarzer setzte Blind statt Bind.
- *D4, Pfeife* (Whistle, 18309): Sie verstärkt nur den nächsten physischen Zauber um 80 %.
  - `MaintainDoTs` setzte sie vor Gesang der Gemarterten (Song of Torment), einen magischen Zauber; dort verfiel sie.
- *D5, Füllreihenfolge:*
  - Heiligenstrahl (Saintly Beam, Potenz 100, 500 nur gegen Untote) stand ohne Prüfung auf Untote vor Schallwelle.
  - Perpetueller Strahl (Perpetual Ray, 3 s Wirkzeit) und viele Flächenzauber mit `AoeCount` 1 standen ebenfalls davor.
  - Wirkzeiten von 2 bis 3 s lassen kaum Raum zum Weben. Schallwelle (1 s) ist bei Blue Academy, WrathCombo und
    BossMod der Füllzauber.
- *D6, Surpanakha* (18323), kein Defekt: Vier Ladungen; jede verstärkt die nächste um 50 % (bis dreifach), jede andere
  Aktion bricht die Kette. RSR beginnt bei vollen Ladungen. Die Fortsetzung steht in `EmergencyAbility` vor jeder anderen
  Fähigkeit und in `EmergencyGCD` vor jedem GCD; dazwischen fällt keine andere Regel (am Code).
- *D7, Diamantrücken am Ende jedes Waxing:* `EmergencyGCD` nahm ihn, sobald Waxing in zwei GCDs endete. Laut Wirktext
  wechselt er sofort in Waning und macht 10 s unbeweglich: Waxing endete früher, und der Spieler konnte keiner Fläche
  ausweichen.
- *D8, Kribbeln:* Es sollte vor dem Dreizack gehen, verlangte aber, dass der letzte GCD schon Kribbeln war. Sein eigener
  Status sperrt es dann; es ging nie.
- *D9, Tatami-gaeshi:* Seine Einstellung nannte Kribbeln als Zustand auf dem Ziel, ein Klon aus Kribbeln; laut Blatt setzt
  es Stun.
- *D11, Mette marcato:* Die vierte Schwinge des Urteils gibt Winged Redemption (10 s, Mette 440 statt 220); RSR wirkte
  Mette nie. Replikator (Peripheral Synthesis) stand ohne Bedingung vor allen Füllzaubern (220 in 2 s), auch gegen ein Ziel, das
  gegen Lightheaded schon immun war. (Behoben mit A508, Stufe 2.)
- *D10, Einstieg in den Rösselsprung-Kreislauf:* Er übersprang die eigene Statusvoraussetzung des Spielers statt der des
  Ziels (`skipStatusNeed` statt `skipTargetStatusNeedCheck`) und ging nie; ein Rösselsprung fiel nur, wenn Bind oder
  Slow schon von anderswo auf dem Ziel standen.

**Das Set** (sein Hinweis vom 03.11.2026: RSR wirke offenbar Zauber außerhalb des Sets oder schreibe es im Kampf um;
1000 Nadeln stehe bei ihm nicht im Set und werde trotzdem gewirkt):
- *Spielregel* (consolegameswiki; seit Patch 4.5, Lodestone-Patchnotes): 24 aktive Zauber; „Active actions cannot be
  changed during combat or during certain duties"; bis zu fünf gespeicherte Sets. Das Set ändert die Hotbar nicht.
  - Seine Frage, ob ein Patch das änderte: In den Patchnotes bis 7.5 ist keine Änderung am aktiven Set gefunden. Gesucht
    wurde im Web, 03.11.2026; die Notizen zu 7.1 bis 7.5 sind nicht einzeln gelesen.
  - Die Auswertung der Patches 7.5 bis 7.57 (A473) fand keine Änderung am Blaumagier.
- *RSR am Code:*
  - Es liest das Set jeden Frame (`ActionUpdater.UpdateSlots`, `GetActiveBlueMageActionInSlot`).
  - Ein Blaumagier-Zauber besteht die Grundprüfung nur im Set (`ActionBasicInfo.IsOnSlot`). Außerhalb des Sets gehen nur
    Folgezauber, die das Spiel an die Stelle eines gesetzten Zaubers legt: Weißer Tod (23268) über Eisiger Nebel,
    Göttlicher Katarakt (23274) über Barriere.
  - Einen Aufruf, der das Set schreibt (`SetBlueMageActions`, `AssignBlueMageActionToSlot`), gibt es im ganzen
    Repository nicht.
  - Die Aktionsliste der Einstellungen zeigt dagegen jeden Blaumagier-Zauber mit Schalter, auch die außerhalb des Sets
    (in einer Gruppe ohne Namen, `RotationUpdater.GroupActions`). Das legt nahe, RSR könne sie nutzen.
- *Andere Werkzeuge:*
  - BossMod und WrathCombo lesen das Set nur.
  - WrathCombo setzt einen Zauber auf Klick in seinem Einstellungsfenster auf einen freien Platz.
  - UltimateCombo tauscht beim Schatzkarten-Tank sechs Plätze selbst, nur außerhalb des Kampfes, ohne Ziel und ohne
    laufenden Zauber.
  - Keines schreibt das Set im Kampf.
- *Upstream hatte einen Setzer* (seine Frage, ob ältere Versionen das konnten; Versionsgeschichte):
  - 82a3891d2 vom 19.12.2024: Jede Blaumagier-Rotation nannte ihre Zauber (`ActiveActions`), und RSR setzte sie als
    aktives Set (`SetBlueMageActions`). Die Einstellung hieß „Automatically set needed Blue Mage actions" und stand unter
    den Grundeinstellungen, ab Werk an. Die Liste der damaligen Grundrotation enthielt 1000 Nadeln.
  - 4f322f30d vom 18.09.2025 („BLU auto set hotfix"): Aufruf abgeschaltet, mit dem Kommentar „disabled for now while i
    figure out what is causing it to crash to desktop". Die Einstellung wurde unter neuem Namen ab Werk aus.
  - a09dc9257 vom 24.04.2026 (Umbau des Blaumagiers, `BLU_Reborn`): Setzer und Einstellung entfernt. Seitdem gibt es
    keine solche Einstellung mehr, weder unter Extras noch anderswo.
  - *Am alten Code sichtbare Absturzursachen* (Schluss, nicht im Spiel belegt):
    - Er übergab dem Client nur so viele Ids, wie die Rotation nannte. Der Client liest für das Set aber 24 Plätze
      (`ActionManager` führt 24, `FixedSizeArray24`) und hätte dann hinter dem Feld weitergelesen.
    - Er rief im Konstruktor der Rotation. Das ist beim Laden, ohne Prüfung auf Kampf und ohne Gewähr, dass es auf dem
      Spiel-Thread lief.
- *Widerspruch, offen:* Code und Spielregel sagen, 1000 Nadeln geht nur im aktiven Set. Seine Beobachtung sagt etwas
  anderes. Möglich sind drei Dinge:
  - Das aktive Set enthält den Zauber doch; gespeicherte Sets und Hotbar zeigen etwas anderes. Möglich ist, dass der
    alte Setzer ihn einmal gesetzt hat (Schluss).
  - Die Lesung liefert etwas anderes als erwartet.
  - Ein Weg umgeht die Prüfung.
  Statisch nicht entscheidbar; Messmittel unter Stufe 0.

**Synergien aus den Wirktexten** (Spielblätter; wo eine Wirkung über den Text hinaus angenommen ist, steht es dabei):

| Erzeuger | Zustand | Verbraucher | Wert gegen Schallwelle (210) | Bedingung, Grenze |
|---|---|---|---|---|
| Schwarzer Rösselsprung (18311) | Bind | Weißer Rösselsprung (18310): 400, setzt Slow | +190 je GCD | Danach im Wechsel: Schwarzer 400 gegen Slow, setzt Bind. Ein Kreislauf aus 400ern, solange Bind und Slow landen. Linie 20 Yalm. Bosse sind meist dagegen immun; gemessen wird, ob der Zustand landet |
| Schlechter Atem (11388), Eisstachel (11418, 50 % Chance) | Slow | Schwarzer Rösselsprung: 400 | +190 | wie oben |
| Aqua-Atem (11390) | Dropsy | Hochstrom (11387): 220 statt 180, Lähmung 30 s; Schneesturm (18297): gefriert | Fläche | Rudel |
| Stimme des Widders (11419) | Deep Freeze 12 s | Ultravibration (23277): tötet gefrorene Gegner; Stimme des Drachen (11420): 400 | Rudel sofort tot | Ultravibration wirkt nicht auf höherstufige und „certain others" (Bosse). Stimme des Drachen spart 8 Yalm Innenkreis aus; der Widder trifft nur 6 Yalm. Der Drache trifft dieselben Gegner also nur nach einem Stellungswechsel, den RSR nicht macht |
| Petrifikation Stufe 5 (11414), geringe Chance | Petrification | Bohrkanone (11398): 600; Ultravibration | +390 | nur Gegnerstufe als Vielfaches von 5, nicht höher als die eigene |
| Eisiger Nebel (23267) + erlittener Treffer | Touch of Frost | Weißer Tod (23268): 400, Deep Freeze | +190 | nur wenn man getroffen wird (Tank) |
| Verwirrung (11403, Stun 6 s), Bombenwurf (11396, 3 s), Spitze Zunge (11412, 4 s), Tatami-gaeshi (23266, 3 s) | Stun | Blankes Messer (11400): 450 | Verwirrung und zweimal Messer 900 gegen 630 | Messer reicht 3 Yalm. Für ihn nicht dafür in den Nahkampf gehen (CLAUDE.md, „Spielweise"). Gegner bauen Widerstand gegen Stun auf |
| Replikator (23286) | Lightheaded 5 s | Senfbombe (23279): Schaden über Zeit nur bei Lightheaded; Replikator selbst 400 beim ersten Mal | Fläche | Wiederholung macht immun |
| Mjam Mjam Matschkuchen (34569) | Begrimed | Großes Reinemachen (34570): Regen je Gegner, bis Potenz 300 | Erhaltung | Rudel, Tank allein |
| Barriere (23273), 30 % der max. HP erlitten | Auspicious Trance | Göttlicher Katarakt: 500, unter Tank-Mimikry 1000 | Tank | Bewegung oder eine andere Aktion beendet die Barriere |
| Schwinge des Urteils (34576), viermal | Winged Redemption 10 s | Mette marcato (34574): 440 statt 220 | +230 | Die Abklingzeit der Schwinge setzt sich dreimal zurück, die vierte kostet 400 |
| Pfeife (18309) | +80 % auf den nächsten physischen Zauber | Dreizack (23264, 3 × 150), Schwinge, Messer gegen Stun | lohnt ab 263 Potenz | eigener GCD. Nicht mit Borstigkeit zusammen (Harmonized/Boost) |
| Kribbeln (23265) | +100 Potenz auf den nächsten physischen Zauber, selbst 100 Potenz | Dreizack (+100 je Treffer nach Spielerpraxis, Text nennt es nicht), Schwinge | Dreizack: +400 gegen 210 | eigener GCD, 2 s |
| Borstigkeit (11393) | +50 % auf den nächsten Zauber | Matra-Magie (800 unter DPS-Mimikry), Zauberatem (Schaden über Zeit), Göttlicher Katarakt, Weißer Tod | lohnt ab 420 Potenz | Vor Gesang der Gemarterten (550 über die ganze Dauer) bringt sie +275 gegen 210, wenig |
| Mondflöte (11415) | +50 % auf allen Schaden 15 s, dann 15 s nichts | Burst (unten) | +3035 je Fenster mit Burst, −840 ohne | Sicherheit, unten |
| Einschüchtern (11411) oder Unheimliche Aura (11421), gemeinsame Abklingzeit 60 s | +5 % erlittener Schaden (Aura nur Magie, aber Fläche) | ganze Gruppe | Gruppenwert | 15 s, passt auf das Mondflöten-Fenster |
| Äther-Mimikry (18322) | DPS / Tank / Heiler | Matra-Magie ×2; Verschlingen 70 s, Katarakt 1000, Drachenkraft 40 %, Skiltron 100; Vitakupo 500, Gobhaut 250, Exuviation 300, Stotram heilt | Rolle | Ziel ist ein Gruppenmitglied der Rolle, nicht man selbst. Die Rolle wählt der Spieler; RSR liest sie (`IsTank`, `IsHealer`) |
| Totalabwehr (11417) | −40 % Schaden erlitten und ausgeteilt | Goblingruß (34563): 220, von vorn 320 | Tank | nur als Tank oder allein |
| Kampfinstinkt (23276) | +100 % Schaden und Heilung, hebt die Strafe der Totalabwehr auf | alles | ×2 | nur allein in Inhalten für mehrere Spieler |
| Wetter Regen, Schauer, Gewitter | – | Pitsche, patsche (34580): 1000 statt 500 | +500 | Wetter des Gebiets; RSR liest es für den Blaumagier noch nicht |
| eigener Chocobo in der Gruppe | – | Choco-Meteo (23284): 300 | Fläche | |
| Untote | – | Heiligenstrahl: 500, Fläche ohne Abschlag | +290 | woran RSR Untote erkennt, ist offen; ohne das nur ab drei Gegnern (100 je Gegner) |

**Ziel-Rotation nach Rolle** (Blue Academy 7.5, WrathCombo, BossMod; die Rolle aus der Äther-Mimikry):
- *DPS, Einzelziel:* Füllzauber Schallwelle. Gesang der Gemarterten erneuern; Zauberatem (eine Wirkung je Gruppe) oder
  Ewige Flammen (34579, eine je Gruppe, bis zum Kampfende).
  - Zermalmender Geist (23275) auf Abklingzeit.
  - Schwinge des Urteils mit Pfeife und Kribbeln, dann Mette marcato.
  - Alle 2 Minuten das Mondflöten-Fenster. Vorher: Pfeife, Kribbeln, Zermalmender Geist. Im Fenster: Gewissenstritt
    (J Kick), Dreizack, Monddämmerung (Nightbloom), Schwinge, Federregen, Pitsche patsche, Schwinge, Schockschlag,
    Sterblichkeit (Being Mortal), Borstigkeit, Swiftcast, viermal Surpanakha, Matra-Magie, Phantomhast. Phantomhast
    kanalisiert in Waning weiter (Blue Academy: „the damage buff snapshots").
  - WrathCombo hält die Burst-Zauber zurück, sobald Monddämmerung in höchstens 30 s bereit ist.
- *DPS, Rudel:* Hydrosog (23282) zieht zusammen. Stimme des Widders und Ultravibration räumen das Rudel ab (Blue
  Academy, UltimateCombo). Sonst Rösselsprung-Kreislauf, Elektrogenese (18298) und die Flächenzauber.
  - Gegen einzelne Dungeon-Bosse, die für Tod verwundbar sind: Rakete (Blue Academy, mit Liste der Bosse).
- *Tank:* Totalabwehr; Goblingruß von vorn als Füllzauber, sonst Schallwelle; Verschlingen.
  - Vor Tankbustern Barriere und Drachenkraft; Katarakt, sobald er bereit ist.
  - Einschüchtern; Schlechter Atem als Minderung; Matschkuchen und Reinemachen zur Erhaltung.
- *Heiler:* Vitakupo, Weißer Wind, Engelspeise, Stotram (Heilung), Gobhaut, Exuviation (reinigt).
- *Bewusst nicht:*
  - Schlussstich, Selbstzerstörung, Assimilation (der Blaumagier stirbt; Assimilation bleibt hinter seinem Schalter,
    ab Werk aus).
  - Bestialischer Zorn (kostet 50 % der max. HP) und Rachestoß (500 nur unter 20 % HP): nur als Option für Spieler,
    die Schaden vor Sicherheit stellen, ab Werk aus (CLAUDE.md, „Spielweise").

**Wert statt Reihenfolge** (die Behebung von D1, D3 bis D5 und D8 bis D10 als Klasse; Stufe 1, gebaut mit A507):
- *Schaden je Potenz:* RSR misst ihn an den eigenen Treffern, im Effekt-Handler, der jeden Treffer sieht.
  - Es zählen Zauber, deren Wirktext genau eine Potenz ohne Bedingung nennt: kein „increased", kein Schaden über Zeit,
    keine Mehrfachtreffer. Gemessen am Hauptziel, der Betrag geteilt durch die Potenz.
  - Mittel über die Treffer derselben Stufe im selben Gebiet, neu bei einem Wechsel. Kritische, Direkttreffer und die
    Buffs, unter denen auch der Füllzauber fiele, sind darin; genau das braucht der Vergleich.
  - So steuert es sich nach Stufe, Ausrüstung, Stufensynchronisation und Kampfinstinkt nach.
- *Füllschaden:* Schaden je Potenz mal die Potenz der Schallwelle aus dem Wirktext, je GCD; ohne Schallwelle im Set die
  der Wasserkanone.
- *1000 Nadeln:* der feste Schaden aus dem Wirktext, höchstens die HP der Gegner in seiner Reichweite (er wird geteilt),
  gegen den Füllschaden über seine Wirkzeit. Ohne Messung geht er nicht.
- *Raketenwerfer, Dimensionsverschiebung:* der Anteil aus dem Wirktext mal die HP aller erreichten Gegner, gegen den
  Füllschaden über die Wirkzeit. Beim Raketenwerfer ist der Anteil das Mittel der vier zufälligen Anteile im Text.
- *Geringe Chance:* Die Einstellung „Use Low Chance abilities" bindet nach ihrem Text.
  - Rakete nennt im Wirktext eine geringe Trefferchance und geht nur unter ihr; bisher ging sie ohne.
  - Der Raketenwerfer nennt keine und geht nach Wert; bisher stand er unter dem Schalter.
  - Schweifschraube, Verhängnis und Todesstufe 5 bleiben darunter.
- *Höhere Stufe:* Jeder Zauber mit festem oder tödlichem Schaden („Has no effect on enemies whose level is higher than
  your own") geht nicht gegen einen Gegner höherer Stufe.
- *Gelernte Immunität:*
  - Landet ein solcher Zauber auf einer Gegnerart ohne Schaden und ohne Fehlschlag (Miss), ist die Art dagegen immun.
    RSR sperrt die Klasse für diese Art bis zum Ende der Sitzung. Ein Fehlschlag ist die geringe Chance und sperrt nicht.
  - Ebenso für Zustände: Meldet das Spiel für einen gesetzten Zustand Widerstand (Effektarten 52, 55, „status no
    effect"), widersteht die Art ihm. Eine Kette, die ihn braucht, beginnt gegen sie nicht mehr (Rösselsprünge).
  - Sitzung statt Datei: Die Tabellen auf Platte tragen Revision und Quarantäne (Konzept 13). Hier kostet ein Irrtum einen
    Zauber je Art und Sitzung. Im Masked Carnivale gelten zuerst die Widerstände aus dem Blatt.
- *Füllreihenfolge (D5):* Schallwelle ist der Füllzauber. Ein anderer Zauber ohne eigene Abklingzeit geht vor ihr nur,
  - wenn er mindestens zwei Gegner trifft und über sie mehr Potenz je Zeit bringt (Potenz über die Ziele aus dem
    Wirktext; Zeit ist die längere von Wirkzeit und GCD), oder
  - wenn er auf einem Ziel mehr Potenz je Zeit bringt, ohne länger zu wirken. Eine längere Wirkzeit kostet Webraum für
    die vielen Fähigkeiten des Blaumagiers; darum ist Schallwelle (1 s) bei Blue Academy, WrathCombo und BossMod der
    Füllzauber, auch gegen Feuersturm mit 220 in 2 s.
  - Steht sein verstärkender Zustand schon auf dem Ziel (Bind, Slow, Stun, Petrifikation, Deep Freeze), gilt die
    verstärkte Potenz, und er geht vor ihr.
  - Ohne Schallwelle im Set bleibt die bisherige Reihenfolge.
  - Heiligenstrahl geht damit ab drei Gegnern (100 je Gegner gegen 210), solange RSR Untote nicht erkennt. Perpetueller
    Strahl (220 in 3 s) geht nie als Füllzauber.
- *Verstärker (D4, D8):*
  - Pfeife nur vor einem physischen Zauber, der damit über einen Füllzauber kommt: vor dem Dreizack (dreimal 150, +80 %:
    +360 gegen 210). Nicht mehr vor dem Gesang der Gemarterten, einem magischen Zauber.
  - Kribbeln vor dem Dreizack. Bisher verlangte RSR, dass der letzte GCD schon Kribbeln war; weil Kribbeln nicht über
    seinen eigenen Status gesetzt wird, ging es nie.
  - Reihenfolge Pfeife, Kribbeln, Dreizack, sobald der Dreizack in so vielen GCDs bereit ist, wie Verstärker fehlen.
  - Borstigkeit bleibt vor dem Gesang (550 über die Dauer, +50 %: +275 gegen 210). Der Gesang wartet nicht mehr auf den
    Status der Pfeife, nur auf den der Borstigkeit.
- *Tatami-gaeshi (D9):* Die Einstellung nannte Kribbeln als Zustand auf dem Ziel, ein Klon aus Kribbeln. Laut Blatt setzt
  es Stun für 3 s. Künftig Stun, und der Masked Carnivale sperrt es gegen Gegner ohne Stun-Anfälligkeit.

**Mondflöte** (D2, D7; Stufe 1, gebaut mit A507; seine Regel, mit der Wahrscheinlichkeit gewichtet):
- *Wert:* Das Fenster geht, wenn der Zuschlag auf das, was in Waxing fällt, den Verlust durch Waning übersteigt.
  - Zuschlag und beide Dauern stehen im Wirktext der Mondflöte (+50 %, 15 s, 15 s).
  - In Waxing fällt jeder Schadenszauber im Set mit eigener Abklingzeit länger als der GCD, der innerhalb von Waxing
    bereit ist: seine erste Potenz aus dem Wirktext, je verfügbarer Ladung. Teilen sich zwei die Abklingzeit (Gewissenstritt
    und Quasar, Federregen und Eruption, Monddämmerung und Beide Enden, Schockschlag und Bergsprenger; Wirktexte), zählt nur
    der stärkere. Dazu der Füllzauber für die übrigen GCDs.
  - Verlust: der GCD der Mondflöte und die GCDs in Waning, je mit der Potenz des Füllzaubers.
  - Kampfende: Ist das Ziel der letzte Gegner in Reichweite und seine korrigierte Restzeit bekannt, zählt nur die Zeit
    bis dahin. Eine Mondflöte kurz vor dem Ende kostet dann nur ihren eigenen GCD.
  - Sagt BossMod in Waxing eine Downtime voraus, geht kein Fenster.
  - Mehrfachtreffer (Dreizack, Matra-Magie, die Steigerung der Surpanakha) zählen nur mit der ersten Potenz. Das
    Fenster geht also eher zu selten als zu oft; mit dem Burst liegt es weit über der Schwelle (+3035 gegen −840, oben).
- *Der Burst wartet:* Kommt die Mondflöte (Wert und Sicherheit erfüllt, weder Waxing noch Waning), warten die
  Schadensfähigkeiten mit Abklingzeit und der Beginn der Surpanakha-Kette.
  - Vorher gehen Pfeife und Kribbeln, wenn der Dreizack in Waxing bereit ist; sie fallen so nicht in die Waxing-GCDs.
  - Das Warten reicht bis zur Mondflöte, gewöhnlich die GCDs für Pfeife, Kribbeln und Flöte. Es endet, sobald es mehr
    kostet, als das Fenster bringt: Jede gehaltene Fähigkeit kommt um die gehaltene Zeit später zurück, über einen langen
    Kampf ein Anteil eines Einsatzes je Sekunde ihrer Abklingzeit. Übersteigt die gehaltene Zeit mal diese Kosten den
    Gewinn des Fensters, gehen sie ohne Flöte; das trifft, wenn der Spieler sich bewegt und die Flöte (2 s Wirkzeit) nicht
    wirken kann. Kostet das Warten nach der Restzeit des Ziels einen Einsatz, wartet nichts (`HoldCostsUse`).
- *Sicherheit* (Option „Moon Flute only when it is safe", ab Werk an; seine Regel als Voreinstellung). Die Mondflöte
  geht nur, wenn alles davon zutrifft:
  - nicht unter Tank- oder Heiler-Mimikry: 15 s Waning ohne Abwehr und ohne Heilung;
  - kein Mitglied in der kritischen Klasse und keine angesagte Fläche, die eines hineinbrächte (`AreaDefenseDanger`,
    Konzept 07);
  - die eigene Gesundheit, mit ihrem gemessenen Verlauf über Waxing und Waning fortgeschrieben, bleibt über der
    kritischen Klasse;
  - BossMod sagt in Fensterlänge keine Raidwide und keinen Tankbuster auf ihn voraus.
  - Gemessen wird die Wahrscheinlichkeit am Gesundheitsverlauf, an der Ansage und an der Zeitleiste von BossMod. Ohne
    Modul bleiben die ersten beiden; eine Raidwide, die nur die Zeitleiste kennt, sieht RSR dann nicht voraus.
  - Ein Tankbuster auf einen anderen Tank zählt nicht: Als DPS hilft der Blaumagier ihm mit keiner seiner Aktionen.
  - Hält sie wegen der Sicherheit, steht der Grund einmal je Wechsel im `DefenseTrace.log`, und der Burst wartet nicht.
  - Verworfen: „allein nur unter Kampfinstinkt und mit voller Gesundheit". Kampfinstinkt wirkt laut Wirktext nur in
    Inhalten für mehrere Spieler; die Bedingung sperrte die Flöte im Masked Carnivale ganz. Volle Gesundheit sagt nicht,
    ob in Waning Schaden kommt; das sagt der Verlauf, allein wie in der Gruppe.
- *Diamantrücken (D7):* Laut Wirktext wechselt er aus Waxing sofort in Waning und macht 10 s unbeweglich, bei −90 %
  Schaden. Bisher nahm RSR ihn am Ende jedes Waxing, sobald er im Set war: Waxing endete bis zu zwei GCDs früher, und der
  Spieler konnte in Waning keiner Fläche ausweichen. Künftig nur, wenn die eigene Gesundheit über Waning fortgeschrieben
  in die kritische Klasse fiele; dann wiegt der Schaden schwerer als die Bewegung.
- Für Spieler, die Schaden vorziehen, schaltet die Option die Sicherheitsbedingungen ab; Wert und Diamantrücken bleiben.

**Alle gelernten Zauber nutzen: das Set** (seine Vorgabe):
- Gelernt ist ein Zauber, wenn `UIState.IsUnlockLinkUnlockedOrQuestCompleted` seinen `UnlockLink` bestätigt. RSR prüft
  das schon in `ActionBasicInfo`; WrathCombo liest es ebenso.
- *Verbessern statt ersetzen* (Stufe 3, gebaut mit A509): RSR nimmt dem Spieler nichts, was er selbst drückt. Heilung,
  Abwehr, Bewegung, Mimikry, Wiederbelebung, Unterbrechung, Provokation und jeder Zauber mit einem Zusatzeffekt bleiben
  im Set. Ersetzt wird nur, was RSR auf dieser Stufe nachweislich nicht oder schlechter wirkt:
  - *Ersetzbar* ist ein Schadenszauber ohne eigene Abklingzeit, dessen Wirktext genau eine bedingungslose Potenz und
    keinen Zusatzeffekt nennt, wenn er weder der beste Füllzauber auf einem Ziel noch der beste auf mehreren ist; dazu
    ein Zauber mit festem Schaden (1000 Nadeln), der den Füllschaden nach Messung nicht schlägt (Stufe 1). Der beste
    Füllzauber auf einem Ziel weicht Schallwelle, wenn sie mehr Potenz je Sekunde bringt (Wasserkanone: 200 in 2 s
    gegen 210 in 1 s, die GCD-Länge als Untergrenze).
  - Ein Abklingzauber im Set bleibt: Seine erste Potenz unterschätzt Dreizack (dreimal 150) und Schwinge des Urteils
    (vier Würfe, danach Mette); ein Tausch nach ihr nähme gerade die Ketten heraus.
  - *Hinzu kommen*, in dieser Reihenfolge, auf freie oder ersetzbare Plätze: Schallwelle als Füllzauber, wenn sie fehlt;
    die Partner der Ketten, deren einer Teil schon im Set steht (Dreizack mit Pfeife und Kribbeln, Schwinge mit Mette,
    Stimme des Widders mit Ultravibration, ein Rösselsprung mit dem anderen, Replikator mit Senfbombe, Matra-Magie mit
    Borstigkeit); dann gelernte Schadenszauber mit Abklingzeit aus Abklinggruppen ohne Vertreter im Set, nach Potenz je
    Sekunde.
  - Ein Platz wird nur überschrieben, wenn sein Zauber nicht abklingt; UltimateCombo tauscht ebenso nur bereite Zauber.
  - *Sein Set ist seine Entscheidung:* Ändert der Spieler das Set selbst (ein gespeichertes Set geladen, ein Zauber für
    eine Carnivale-Stufe gesetzt), schreibt RSR in dieser Sitzung nicht mehr hinein; im Masked Carnivale nie. Grund: Für
    eine Stufe wählt er Zauber nach dem Element, gegen das ihre Gegner schwach sind (Wasserkanone gegen einen
    feuerschwachen); als Füllzauber gemessen wäre sie ersetzbar, und Schallwelle (Wind) träfe dort vielleicht einen
    immunen Gegner.
- *Schreiben* nur unter diesen Bedingungen:
  - während RSR an ist; außerhalb des Kampfes, ohne Ziel, ohne laufenden Zauber und ohne vorgemerkte Aktion, wie
    UltimateCombo;
  - auf dem Spiel-Thread, aus dem Frame-Update der Rotation, nie aus einem Konstruktor;
  - Platz für Platz über `AssignBlueMageActionToSlot`, den Weg, den WrathCombo (auf Klick) und UltimateCombo (für den
    Schatzkarten-Tank) im Betrieb nutzen; RSRs ClientStructs führt dieselbe Signatur. Nicht über `SetBlueMageActions`:
    Daran stürzte Upstreams Setzer vermutlich ab (unvollständiges Feld, oben);
  - eine Änderung je Frame; im nächsten Frame liest RSR den Platz zurück. Steht der Zauber nicht dort, hat das Spiel
    abgelehnt, etwa in einem Inhalt, der den Wechsel verbietet: Dann schreibt RSR in diesem Gebiet nichts mehr.
  - Jede Änderung steht im `DefenseTrace.log` mit Platz, altem und neuem Zauber und Grund.
- *Option* „Improve the Blue Mage set from your learned spells" in der Rotation, ab Werk an (seine Vorgabe „nutze alle
  gelernten spells optimal aus"). Seine gespeicherten Sets im Zauberbuch bleiben; ein Klick stellt sein Set wieder her.
  Die Hotbar bleibt unberührt.
- *Bedrohung:* Ein Absturz beim Schreiben. Dagegen: der im Betrieb erprobte Weg, nur außerhalb des Kampfes, ein Platz je
  Frame, Rücklesen. Ob das Spiel einen Platz mit einem abklingenden Zauber ablehnt oder den Zauber verwirft, ist nicht
  belegt; RSR überschreibt einen solchen Platz nicht.
- *Masked Carnivale* (seine Frage, 03.11.2026: ob ein Set vor dem Betreten so aus bekannten Zaubern optimiert werden
  kann, dass es die besten Gewinnchancen hat). Ja, aus Daten, die der Client schon liefert:
  - *Die Stufe:* Die Einweisung (`AgentAozContentBriefing`, FFXIVClientStructs b69073a) führt die gewählte Stufe
    (`SelectedContentIndex`), ihre Gegner je Akt (`AozArrangementData`) und die Wochenaufgaben mit Bedingungen. Das
    Blatt `AOZContent` nennt je Stufe die drei Akte, ihren Kampftyp sowie Ideal- und Standardzeit.
  - *Die Gegner:* RSR liest das Blatt `AOZContentBriefingBNpc` schon (`MaskedCarnivaleHelper`, seit Upstreams Umbau
    a09dc9257). Es enthält je Gegner Schwäche oder Immunität gegen sechs Elemente und vier Angriffsarten. Dazu kommt die
    Verwundbarkeit für Slow, Petrifikation, Lähmung, Unterbrechung, Blind, Stun, Schlaf, Bind, Heavy und festen oder
    tödlichen Schaden. Die Einweisung im Spiel zeigt dieselben Daten. RSR sperrt damit heute Zauber gegen immune
    Gegner, wählt das Set aber nicht danach.
  - *Die Wahl:* je Akt die gelernten Zauber nach Wert gegen diese Gegner. Schwäche zählt hoch, Immunität fällt weg,
    eine Verwundbarkeit öffnet eine Kette aus der Synergietabelle, etwa Petrifikation mit Bohrkanone oder Ultravibration.
    Dazu Heilung, Abwehr und Unterbrechung (Spucksardine), wo ein Gegner unterbrechbar ist.
  - *Wann:* Das Set wird geschrieben, solange die Einweisung offen ist und eine Stufe gewählt ist, vor dem Betreten,
    außerhalb des Kampfes. Ob das Spiel das Set in der Stufe selbst ändern lässt, ist offen; der Entwurf braucht es
    nicht.
  - *Nicht belegt, darum noch nicht gebaut:* Was die Gegner-Ids der Einweisung (`AozArrangementData.Enemies`) sind,
    sagt FFXIVClientStructs nicht. Dass sie Zeilen von `AOZContentBriefingBNpc` sind, ist ein Schluss. Messmittel (A511):
    Beim Öffnen der Einweisung als Blaumagier schreibt `DefenseTrace.log` je gewählter Stufe „carnivale briefing" mit
    den Ids je Akt und dem Namen, den das Blatt für jede nennt. Stimmen die Namen mit den Gegnern der Stufe überein,
    trägt der Schluss.
  - *Und seine Entscheidung:* Für eine Stufe wählt er sein Set selbst; seit A510 schreibt der Verbesserer nach einer
    eigenen Änderung nicht mehr hinein. Eine Stufenwahl durch RSR wäre darum eine eigene Option, die er einschaltet.
  - *Grenze:* Die Blätter beschreiben Widerstände, nicht die Mechaniken einer Stufe (etwa Reflexion oder ein Gegner,
    der zur Mitte zieht). Für die beste Gewinnchance zählen auch die Stufen-Leitfäden als zweite Quelle, je Stufe
    geprüft. Ohne sie bleibt es eine Wahl nach Widerständen.

**Stufen:**
- *Stufe 0, Messung* (gebaut mit A506):
  - `DefenseTrace.log` schreibt das aktive Set des Blaumagiers, sobald es sich ändert.
  - Es schreibt jede eigene Blaumagier-Aktion, die landet, obwohl sie nicht im gelesenen Set steht; Folgezauber über
    ihren Grundplatz ausgenommen.
  - Das entscheidet seinen Hinweis zu 1000 Nadeln.
- *Stufe 1, Defekte* (gebaut mit A507): D1, D3 bis D5 und D8 bis D10 nach „Wert statt Reihenfolge"; D2 und D7 nach
  „Mondflöte". Messmittel im `DefenseTrace.log`: „moon flute comes" mit Fensterwert, Gewinn und Verlust; „moon flute
  held" mit dem Sicherheitsgrund, einmal je Wechsel; „damage per potency: first measure" je Stufe und Gebiet; „state
  resisted", „state did not land" und „fixed or lethal spell without effect" je gelernter Gegnerart.
- *Stufe 2, Synergien* (gebaut mit A508; jede Kette prüft, ob ihr Zustand landet, über `OwnHitWatch`):
  - *Rösselsprung-Kreislauf, Pfeife und Kribbeln vor dem Dreizack:* mit Stufe 1 (A507).
  - *Schwinge des Urteils → Mette marcato:* Unter Winged Redemption geht Mette (440) vor allem anderen der Abklingzeiten.
  - *Borstigkeit → Matra-Magie:* nur unter DPS-Mimikry (achtmal 100: +400 für einen GCD von 210); ohne sie achtmal 50,
    +200, weniger als der GCD. Nicht, solange die Mondflöte kommt; in Waxing geht sie.
  - *Replikator → Senfbombe:* Replikator nur auf ein Ziel ohne Lightheaded, auf dem sein letzter Treffer den
    Zustand gesetzt hat (400 statt 220); danach Senfbombe auf das Ziel unter eigenem Lightheaded (Schaden über Zeit,
    15 s), nicht zweimal hintereinander. Lightheaded nutzt sich laut Wirktext ab („Repeated use"); ein Ziel, auf dem er
    nicht mehr landet, ist für eine Weile immun, seine Art nicht. Darum je Ziel, nicht je Art.
  - *Stimme des Widders → Ultravibration:* Widder nur, wo Ultravibration töten kann: Ziel nicht höherer Stufe, Art nicht
    als immun gegen die Klasse gelernt, Deep Freeze auf der Art nicht als widerstanden gelernt. Seit Stufe 1 sperrt die
    gelernte Immunität Ultravibration; ohne diese Bedingung bliebe sie bereit, und der Widder ginge jeden GCD.
  - *Stun → Blankes Messer:* Verwirrung (6 s Stun, kein Schaden, zwei Messer: 900 gegen 630), Tatami-gaeshi (220, 3 s,
    ein Messer) oder Bombenwurf (200, 3 s), nur wenn der Spieler schon in Messerreichweite (3 Yalm) steht, das Ziel
    nicht betäubt ist, noch betäubt werden kann (`StunWatch`) und Stun und ein Messer überlebt. Gewöhnlich als Tank-
    Mimikry im Nahkampf; RSR bewegt niemanden dafür.
  - *Petrifikation Stufe 5 → Bohrkanone, Ultravibration:* unter „Use Low Chance abilities" (Wirktext: geringe Chance).
    Die Zielprüfung liest „Enemy level must be a multiple of 5" aus dem Wirktext, für Todesstufe 5 ebenso.
  - *Heiligenstrahl gegen Untote:* RSR lernt Untote je Art am Treffer: Liegt der Schaden näher an der erhöhten Potenz
    (500) als an der einfachen (100), gemessen am Schaden je Potenz, ist die Art untot; dann geht der Strahl auch auf
    einem Ziel vor Schallwelle. Gelernt wird nur, wo er schon fiel (ab drei Gegnern).
  - *Nicht gebaut:* Pitsche, patsche wartet nicht auf Regen (das Wetter wechselt nach Spielstunden, Warten kostete den
    Einsatz); Aqua-Atem → Hochstrom bleibt Flächenwert (180 bzw. 220 in 2 s schlägt Schallwelle nur auf mehreren Zielen).
  - *Immunität lernen nur aus Zaubern mit HP-Schaden:* Rakete, Raketenwerfer, Dimensionsverschiebung und Schweifschraube
    beantworten eine verfehlte Chance mit einem Fehlschlag. Verhängnis, Todesstufe 5, Petrifikation und Ultravibration
    setzen einen Zustand oder töten; wie das Spiel dort eine verfehlte Chance meldet, ist nicht belegt, also lehren sie
    nichts. Die gelernte Immunität sperrt aber die ganze Klasse (Schluss: Eine Art, gegen die Prozentschaden nichts
    tut, ist meist auch gegen Tod immun).
- *Stufe 3, Set* (gebaut mit A509): „Verbessern statt ersetzen" oben. Offen bleibt die Wahl je Stufe im Masked
  Carnivale (unten); der Verbesserer schreibt dort wie überall, nach Wert ohne die Widerstände der Stufe.

**Falsifikation:**
- *Kein Defekt?* D1, D2 und D3 sind am Code und an den Blättern belegt. Die Rechnung zeigt D2 als Verlust auch an
  Schaden.
- *Option falsch?*
  - Eine feste Reihenfolge wie bei WrathCombo trüge die Burst-Logik, aber nicht die Stufen und die Lage; die
    Wertregel steuert sich nach.
  - Ein Set im Kampf umzuschreiben, verbietet das Spiel.
- *Ausgeliefert, und nichts ändert sich?*
  - Steht ein Zauber nicht im Set, bleibt jede Regel stumm. Darum misst Stufe 0 das Set, und Stufe 3 setzt es.
  - Ohne DPS-Mimikry bringt Matra-Magie die Hälfte; das trägt die Wertregel.
- *Stufe 1, gebaut:*
  - *Kein Defekt?* D7 bis D10 sind wie D1 bis D3 am Code und am Wirktext belegt.
  - *Option falsch?* Die Mondflöte geht nach Wert gierig: Sie geht, sobald ein Fenster jetzt mehr bringt als kostet,
    auch zur Minute. Dort liegen je Abklinggruppe eine 30- und 60-s-Fähigkeit (Gewissenstritt 300, Schockschlag 400,
    Eruption 300, zwei Ladungen Surpanakha 400, Zermalmender Geist 400 als GCD) mit fünf Füllzaubern bei 2850 Potenz, also +1425
    gegen 1470: knapp darunter, die Flöte geht dort gewöhnlich nicht. Geht sie doch, weil mehr bereit ist, bringt sie
    mehr als sie kostet; die 120-s-Fähigkeiten sind zur Minute ohnehin nicht bereit, das 2-Minuten-Fenster verschiebt
    sich nicht. Eine Ankerregel (nur mit Monddämmerung) trüge auf niedriger Stufe ohne Monddämmerung gar kein Fenster.
  - *Ausgeliefert, und nichts ändert sich?*
    - Liest RSR den Wirktext der Mondflöte nicht, geht sie nicht mehr; vorher ging sie immer. Das ist die sichere
      Richtung, und `DefenseTrace.log` nennt jeden Wirktext ohne Potenz einmal.
    - Ohne Messung des Schadens je Potenz geht 1000 Nadeln nicht. Die erste eigene Schallwelle misst ihn.
    - Meldet das Spiel Immunität gegen festen Schaden anders als ohne Schaden, ohne Fehlschlag und ohne
      Unverwundbarkeitsstatus, lernt RSR sie nicht; welche Antwort es gibt, ist nicht belegt. Dann wiederholt sich ein
      solcher Zauber gegen diese Art; die Zeile „fixed or lethal spell without effect" fehlt dann im Protokoll.
- *Stufe 2, gebaut:*
  - *Kein Defekt?* D11 am Code (kein Aufruf von Mette marcato; Replikator ohne Bedingung).
  - *Option falsch?* Verwirrung trägt keinen Schaden: Landet ihr Stun nicht, ist ein GCD verloren, bis `StunWatch` die
    Immunität kennt. Gegen Bosse ist das die Regel; die Bedingung „noch betäubbar" fängt den zweiten Versuch.
  - *Ausgeliefert, und nichts ändert sich?* Jede Kette braucht beide Zauber im Set; ohne Stufe 3 entscheidet sein Set.
- *Stufe 3, gebaut:*
  - *Kein Defekt?* Am Code belegt: Außerhalb des Sets besteht kein Blaumagier-Zauber die Grundprüfung, und nichts schrieb
    das Set; ein gelernter Zauber, der fehlt, fiel nie.
  - *Option falsch?* Ein Set, das RSR ganz nach Rolle neu wählt, nähme dem Spieler Werkzeuge, die er selbst drückt
    (Bewegung, Mimikry, Zauber für eine Strategie); die Rollenseiten von Blue Academy nennen je Rolle 26 bis 35 Zauber
    für 24 Plätze, ohne Rangfolge. Darum nur Verbessern.
  - *Ausgeliefert, und nichts ändert sich?* Ein volles Set ohne ersetzbaren Füllzauber bleibt, wie es ist; das ist
    gewollt. Lehnt das Spiel einen Platz ab, schreibt RSR in dem Gebiet nichts mehr; die Zeile „now holds" fehlt dann.
- *Prüfgrad:* statisch an Code, Blättern, Spieler- und Werkzeugquellen; die Mondflöten-Rechnung aus Blattpotenzen;
  Compile und alle 43 Prüfschritte grün. Das Nachziehen von Dauerschaden in Waxing ist Spielerpraxis. Im Spiel steuern
  sich Schaden je Potenz und gelernte Immunität selbst nach; Fensterwert und Haltegrund schreibt das Protokoll.

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

**Geprüft am 03.11.2026 (A510), kein weiterer Defekt belegbar:** dieselben Fehlerklassen wie beim Blaumagier (A507,
A508) am Beastmaster gesucht.
- *Überspring-Schalter:* Die Äxte, der Trick und die Finisher überspringen die eigene Voraussetzung des Spielers
  (Herz-Status, Sunstrider, Moonstalker) mit `skipStatusNeed`, Quelling Wave die des Ziels (ein Buff zum Auflösen)
  mit `skipTargetStatusNeedCheck`; beides passt.
- *Statusketten:* Die vier Äxte verlangen und gewähren die Herzen im Uhrzeigersinn ohne Lücke; die Finisher wechseln
  Sunstrider und Moonstalker.
- *Quelling Wave* (350, +10 TP, mit Auflösung +50 TP; Wirktext): Es geht zuerst mit einem Buff zum Auflösen, ohne ihn
  erst nach der Kombo. Mitten in der Kombo bräche es sie (160, 500, 600: im Mittel 420 je GCD gegen 350); so steht es.
- Seit A507 misst `OwnHitWatch` auch beim Beastmaster den Schaden je Potenz (Smash Axe nennt eine bedingungslose
  Potenz); heute liest ihn keine Beastmaster-Regel.
- Die beiden offenen Punkte („die Regeln schrieben keine Zeile", Hornbelegung setzen) warten auf seine Protokolle.

**Anfreunden und Analysieren** (seine Frage vom 02.11.2026, ob sich vor dem Tod eines Gegners prüfen lässt, ob man sich
mit ihm anfreunden kann, und das Talent dann so einsetzen, dass es wahrscheinlich gelingt; A483):
- *Mechanik* (Spielblätter, xivapi v2, Action 44880 und 44882, Status 4624 und 4626, LogMessage 11217–11222, 11400,
  11425, 11443, 11455; 02.11.2026):
  - *Analysieren* (`GaugePvE`; Talent ohne Wirkzeit, 30 Yalm, 3 s Abklingzeit in eigener Gruppe 18, auch vom Reittier
    am Boden; ob es Feindseligkeit erzeugt, sagt keine Quelle)
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
- *Nicht belegt:* ob die Stufe von Analysieren die aktuellen LP einrechnet (die Sonde unten misst es); ob der Sieg durch
  einen Mitspieler zählt; eine Prozentzahl; ob ein zweites Anfreunden auf eine schon markierte Bestie die LP neu festhält
  (consolegameswiki, „Beastmaster", Tipps: „use Capture again to get a better chance before finishing it off";
  Gemeinschaftsquelle, ungeprüft).
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
- *A2 – Wann* (seine Vorgabe vom 04.11.2026: „anfreunden hat oberste priorität, was nützen schnelle tode der gegner,
  wenn man den kampf immer wiederholen muss, weil anfreunden und begleiter bekommen nicht klappt. daher muss die nutzung
  dieser fähigkeit so sicher sein, dass immer genug zeit bleibt, damit die fähigkeit wirkt."; A525): mit dem letzten GCD,
  dessen Markierung nach allem, was der Gegner bisher an Schaden nahm, sicher vor seinem Fall ankommt. RSR markiert,
  sobald das Warten auf den nächsten GCD ihn vor dessen Markierung fallen lassen könnte:
  - *Einbruch:* Seine LP sind höchstens so groß wie der größte Einbruch über eine so lange Spanne – Rest des laufenden
    GCD + ein GCD + Landezeit, auf ganze Abtastungen aufgerundet –, und zwar der größere von zweien:
    - *seiner:* aus seiner Gesundheitshistorie (die letzten 30 s, einmal je Sekunde abgetastet, `TimeToKillTracker`),
      beginnend mit der letzten Abtastung bei vollen LP, seine LP in diesem Augenblick als jüngster Wert. Ist sie kürzer
      als die Spanne, gilt der Einbruch seit dieser vollen Abtastung, auf die Spanne hochgerechnet.
    - *der des Gebiets:* der größte Einbruch in LP, den irgendein Gegner in diesem Gebiet über eine so lange Spanne
      erlitten hat, seinen tödlichen Treffer eingeschlossen (die erste Abtastung nach dem Tod liest 0; der Körper liegt
      noch einen Moment), geteilt durch seine eigenen maximalen LP. Ein Gebietswechsel setzt ihn zurück.
  - *Oder Restzeit:* Seine korrigierte Restzeit ist nicht länger als diese Spanne.
  - Keine untere Grenze: Auch eine Markierung, die vielleicht zu spät kommt, geht noch. Weil Anfreunden nicht tötet, geht
    es auch bei 1 LP. Anfreunden steht vor jeder anderen Waffenfertigkeit der Rotation; außerhalb des Kampfes nur auf das
    Ziel, das RSR ohnehin angreift (sein aktuelles Ziel), damit es keinen anderen Gegner zieht.
  - *Warum der Einbruch des Gebiets* (sein Hinweis vom 04.11.2026: „anfreunden ist immer noch viel zu spät gerade bei
    einfachen trashmobs wie apkallu sterben die mobs viel zu schnell"; A527): Ein Gegner bei vollen LP hat bei RSR keine
    Historie (`TimeToKillTracker.Record` verwirft sie bei vollen LP), sie beginnt mit der ersten Abtastung nach dem ersten
    Treffer, eine je Sekunde, und den tödlichen Treffer tastet RSR nie ab. Ohne Historie markierte die Regel nicht; der
    erste Treffer, oft der größte Teil, fehlte im Einbruch. Ein Gegner, der zwei, drei GCDs nach dem ersten Treffer
    fällt, war tot, bevor die Regel etwas sah. Gerechnet (`bst_capture_model.py`, 04.11.2026; Trash fällt 2 bis 8 s nach
    dem ersten Treffer, stoßweise): Die Regel aus A525 markierte 64 % dieser Kills rechtzeitig, die jetzige 99 % ab dem
    zweiten Kill im Gebiet; den ersten Kill eines Gebiets 68 %, weil noch nichts gelernt ist. Nachgespielt an seinem
    Mitschnitt vom 04.11.2026 nachmittags (Build 59065ea56, vor dieser Regel; Annahme: Anfreunden ginge zu jedem seiner
    GCDs ab dem ersten Treffer): rechtzeitig markiert hätte sie Drakon 12 von 12, Apkallu 11 von 11, Sandwurm 42 von 42,
    Coeurl 5 von 5, Sandstein-Golem 7 von 8, Kolibri 3 von 5. Die Regel davor markierte in dieser Sitzung fünfmal selbst;
    siebenmal wählte sie Anfreunden auf einen Drakon, der fiel, bevor der GCD frei war. 13 Markierungen drückte er
    selbst, elf davon bei vollen LP.
  - *Preis auf Trash:* Wer schneller fällt als eine Spanne, wird schon mit dem ersten GCD markiert, im Modell bei 96 % LP:
    eine kleine Chance je Kill, aber jede weitere Bestie derselben Art ist ein neuer Versuch; ein Kill ohne Markierung hat
    keinen. Fällt ein Gegner langsamer, kommt die Markierung später, bei weniger LP. Gegen einen Boss gilt der größere der
    beiden Einbrüche; schlug die Gruppe auf Trash härter als auf ihn, markiert RSR ihn etwas früher als nach seiner
    eigenen Historie.
  - *Warum die Landezeit:* Der Pakt entscheidet sich beim Fall eines Gegners, der „Freundschaftsempfänglich" trägt
    (Status 4626, Wirktext). Die Markierung kommt nach der Wirkung von Anfreunden: 0,62 s später, in seinen Mitschnitten
    vom 02.11. (Killermantis) und 03.11.2026 (Flechten-Goobbue); 0,69 s nach dem Drücken am 04.11.2026 (Zorniger Ichor).
    Fällt er vorher, gibt es keinen Pakt. RSR misst die Landezeit selbst, vom Drücken bis zur Markierung, und nimmt die
    zuletzt gemessene. Bis zur ersten Messung gelten die 0,62 s.
  - *Warum der Einbruch, nicht nur die Restzeit:* Die Restzeit ist ein Mittel über bis zu 30 s; der Schaden kommt in
    Stößen (Dressur, Entfesseln, Kritische Treffer, eingewobene Äxte). Am Zornigen Ichor am 04.11.2026 (11:52, sein
    Mitschnitt) fiel er über 4 s im Mittel 815 LP, höchstens 1.753 (15,4 % seiner LP), also gut das Doppelte. Gerechnet
    (`bst_capture_model.py`, 04.11.2026; Annahmen dort): Bei gleichmäßigem Fall markiert die Regel alle Kills rechtzeitig
    (die Restzeit-Regel 98 %), in Stößen 99 % (76 %); beschleunigt sich der Fall in den letzten 5 s auf das 1,5-fache,
    94 bis 95 % (50 bis 51 %), auf das Doppelte 83 bis 85 % (26 bis 33 %).
  - *Der Preis:* Sie markiert früher, also bei mehr LP: im Modell bei 9 % (gleichmäßig) und 16 % (in Stößen) der LP der
    letzten 30 s, die Restzeit-Regel bei 7 bis 8 %. Am Ichor am 04.11.2026 hätte sie um 11:52:43 bei 14,7 % markiert;
    er selbst markierte um 11:52:46 bei 12,5 %, und der Pakt kam: Der Schleim war bis dahin nicht freigeschaltet, sonst
    hätte RSR den Ichor nicht beobachtet, und steht ab 15:06 auf seinem Horn; die Zeile „no pact" kam zu früh (A528).
    Am Nachmittag kamen Pakte bei 9,8 % (Drakon), 36 % (Coeurl), 89 % (Kolibri) und etwa 100 % LP (Sandwurm, gedrückt
    bei vollen LP; sein Mitschnitt): Auch bei vielen LP ist die Chance nicht verschwindend, auf Gegner unter seiner
    Stufe. Wie die Chance mit den LP fällt, ist nicht belegt.
    Der Wirktext sagt „umgekehrt proportional". Zusammenfassungen einer Suche nennen 12,5 % als Grenze (Gamer Escape)
    und raten zu unter 10 %, bei zähen Zielen unter 5 % (Icy Veins); beide Seiten weisen Skripte ab, ungeprüft. Ist die
    Chance wörtlich umgekehrt proportional zu den LP, bringt die späte Regel je Kill trotz ihrer Fehlschläge mehr Pakte
    (Ichor: etwa 4 gegen 13 % LP beim Markieren); fällt sie nur grob mit den LP, die sichere. Seine Vorgabe entscheidet
    für die sichere: Ein verpasster Kill ist sicher verloren, eine Markierung bei mehr LP nur unwahrscheinlicher.
  - *Restzeit von jetzt:* Die korrigierte Restzeit wird einmal je Sekunde abgelegt. Gelesen wird sie abzüglich ihres
    Alters, sonst ist sie bis zu 1 s zu lang (gilt für jeden Leser, `GetCorrectedTTK`).
  - *Die Regeln davor:*
    - „Restzeit nicht länger als ein GCD", aus der abgelegten Restzeit, ohne Landezeit: Sie kam am 04.11.2026 zweimal zu
      spät. Morgens fiel Anfreunden auf den Zornigen Ichor 1,1 s vor dem Kampfende in die zweite Hälfte eines GCD. Um
      10:47 wählte RSR es 1,7 s vor seinem Fall, und es ging nicht mehr hinaus (Build 0ee04305b).
    - „Der letzte GCD, dessen Markierung noch ankommt" (Landezeit < Restzeit ≤ Landezeit + ein GCD, A523): Sie kam am
      04.11.2026 nicht zum Zug, weil er Anfreunden um 11:52:46 selbst drückte (Build 3ec6f295b; keine Zeile „Capture on"
      von RSR). Im Modell verliert sie in Stößen ein Viertel der Kills.
  - *Spur:* Wählt RSR Anfreunden, schreibt es eine Zeile mit dem Grund: größter Einbruch über die Spanne und die LP
    des Gegners, oder seine Restzeit. Danach schreibt es, wann die Markierung ankam (Landezeit), bei wie viel LP, und ob
    RSR oder er sie gedrückt hat – RSR, wenn es Anfreunden auf diesen Gegner innerhalb eines GCD vor dem Drücken gewählt
    hatte –; ob der Gegner markiert fiel und ob das Spiel den Begleiter danach als freigeschaltet führt. Das Urteil „kein
    Pakt" fällt erst, wenn der Körper des Gegners verschwunden ist oder er das Gebiet verlässt: Das Spiel trägt den Pakt
    etwa 2 s nach dem Fall ein, mit der Niederlage (Drakon, Kolibri, Coeurl am 04.11.2026); geurteilt im nächsten Frame,
    hieß es beim Schleim und beim Sandwurm „no pact", obwohl beide kamen (A528). Fällt ein Gegner, der ein fehlender
    Begleiter wäre, ohne Markierung, schreibt es auch das, mit seiner letzten Restzeit.
  - *Verworfen:*
    - Eine zweite Markierung später, bei weniger LP: Ob das Spiel sie annimmt und ob sie die Chance neu setzt, belegt
      keine Quelle (Wirktext; consolegameswiki, eorzeantavern, ffxiv-offduty, 04.11.2026). Messen lässt es sich nicht:
      Die Markierung trägt keinen Wert (Statusparameter 0 bei 100 und bei 12,5 % LP), der Wurf ist unsichtbar. Sie
      kostete einen GCD je Versuch ohne belegten Nutzen.
    - Ein fester GCD Vorlauf auf die Restzeit-Regel: bei gleichmäßigem Fall bei 15 statt 9 % LP, in Stößen nicht
      sicherer (99 %), bei Beschleunigung teils besser, teils schlechter.
    - Den Schaden am Ziel zurückhalten, bis es markiert ist: Der Begleiter schlägt weiter, und gegen einen Boss allein
      gibt es kein anderes Ziel; der Einbruch misst schon, was kommen kann.
    - Sofort markieren, wie WrathCombo (1e64c866c vom 03.11.2026, Werkzeugquelle: jedes Ziel, sobald es eine fehlende
      Bestie ist): sicher, aber bei voller LP. Seine Markierung bei 100 % um 10:57 am 04.11.2026 brachte keinen Pakt.
    - Etwa 20 s vor dem geschätzten Tod, wie Daedalus: noch mehr LP.
- *Preis:* ein GCD je Versuch, einmal je Gegner einer Art, die du noch nicht hast; misslingt der Pakt, beim nächsten
  dieser Art. Hast du sie, nie wieder.
- *Lagen:* In der Gruppe markiert RSR ebenso; ob der Sieg eines Mitspielers zählt, ist unbelegt, der Preis bleibt ein
  GCD. Der Einbruch enthält dann den Schaden der Gruppe. Unter Stufensynchronisation zählt deine synchronisierte Stufe.
  Mehrere Bestien: eine Markierung zur Zeit. Drückt er Anfreunden selbst, markiert RSR diesen Gegner nicht mehr; der
  Pakt hängt dann an seinem Zeitpunkt.
- *Analysieren als Sonde* (A546): A2 braucht Analysieren nicht. Ob die Stufe die LP einrechnet, misst RSR im Kampf
  selbst. Es drückt Analysieren auf eine kämpfende Bestie, die dir fehlt, und zwar zweimal: beim ersten Sehen und nach
  deiner Markierung, jeweils im letzten freien Webfenster. Analysieren ist ein Talent ohne Wirkzeit mit eigener
  Abklingzeit und kostet keinen GCD. Die Antwort liest RSR als Spielmeldung (`IChatGui.LogMessage`, Zeilen 11217 bis
  11222, unabhängig von der Sprache) und schreibt sie mit den LP der Bestie in `DefenseTrace.log` („gauge: …").
  - Nach der zweiten Lesung urteilt die Zeile. *Stufe gewandert:* Analysieren rechnet die LP ein, und die Sonde endet für
    die Sitzung. *Stufe gehalten:* Analysieren las die Bestie, nicht ihre LP – es sei denn, beide Lesungen liegen in einer
    Stufe. Dieselbe Zeile nennt, was das Spiel zu einem zweiten Anfreunden auf die markierte Bestie sagt (Status 0: es
    nähme es an).
  - *Preis im Kampf:* zwei Webfenster je fehlender Bestie und je Lesung zwei Chatzeilen („Du setzt Analysieren ein." und
    die Antwort). Gegner außerhalb des Kampfs fragt die Sonde nicht. Ob Analysieren Feindseligkeit erzeugt, sagt keine
    Quelle, und eine zweite gezogene Bestie wäre ein Preis, den die Sonde nicht kosten darf. Sie läuft nur, solange
    „Capture familiars you do not have yet" an ist.
  - *Warum eine Sonde und keine Regel:* Ob ein Markieren bei der besten Stufe mehr Pakte bringt als A2, hängt an zwei
    Unbekannten. Die erste ist, ob die Stufe die LP einrechnet; sie beantwortet die Sonde. Die zweite ist, wie weit die
    Chance innerhalb einer Stufe noch steigt; sie bleibt offen, weil die Stufen grob sind. Eine Regel, die nach der Stufe
    markiert, setzte beides voraus. Eine Nachsteuerung zur Laufzeit ist deshalb erst baubar, wenn die erste Antwort
    vorliegt.
  - *Messung* (seine Mitschnitte vom 04.11.2026, Chatmeldungen): Bis zum Pakt brauchte es bei Apkallu fünf
    Markierungen, bei Coeurl vier, bei Golem drei, bei Kolibri und Drakon je zwei (beim Drakon kam eine zu spät). Bei
    Schleim, Dullahan, Riesenwurm, Morbol und Treiberameise reichte je eine. Die Fehlschläge häufen sich also bei
    einzelnen Arten; das passt zu einer Schwierigkeit je Bestie, wie sie das Wiki nennt („intrinsic").
- *Zweites Anfreunden* (Gemeinschaftsquelle, ungeprüft; oben): Träfe der Tipp zu, könnte RSR früh markieren und spät
  nachmarkieren. Die frühe Markierung wäre sicher, die späte brächte die Chance – A2 ohne sein Risiko. Heute schließt A1
  markierte Bestien aus. Ob das Spiel ein zweites Anfreunden annimmt, misst die Sonde mit. Ob es die LP neu festhält,
  misst nichts in RSR, nur die Zahl der Pakte über viele Versuche. Hielte es die erste LP fest, kostete eine frühe
  Markierung die Chance.
- *Weg ohne Kampf* (ffxiv-offduty, Gemeinschaftsquelle, ungeprüft): Bestien, die nur in einem Inhalt vorkommen, soll
  es gegen Währung aus dem Crucible als Kornago-Kürbis geben.
- *Gegenhypothesen:*
  - Bedeutet die 5 etwas anderes, versucht RSR es an Gegnern ohne Pakt; das Spiel lehnt das Ziel dann ab, oder es
    kostet einen GCD je Gegner, nicht mehr.
  - *Kein Defekt?* Die Regel davor hätte am 04.11.2026 um 11:52 vielleicht rechtzeitig markiert; sie kam nicht zum Zug.
    Der gemessene Schaden am Ichor war aber stoßweise (Höchstwert über 4 s gut das Doppelte des Mittels), und dort
    verliert sie im Modell ein Viertel der Kills.
  - *Option falsch?* Der Preis oben. Er hält als Einschränkung; nach seiner Vorgabe überwiegt die Sicherheit.
  - *Ausgeliefert, und nichts ändert sich – warum?* Ein Stoß größer als jeder der letzten 30 s (das erste Entfesseln
    nach einer Pause, Bestienkönnen) bleibt das Restrisiko, im Modell bei Verdopplung 15 bis 17 % der Kills. Der erste
    schnell fallende Gegner eines Gebiets bleibt ein Risiko, solange RSR dort noch keinen Fall gelernt hat; dann sagt die
    Zeile „unmarked, without Capture" es mit seiner letzten Restzeit.

**Belegung der Hörner in der Rotation** (seine Vorgabe vom 02.11.2026: „wieso vorschlag? es soll in die rotation
eingearbeitet werden, situationsbezogen"; A487, A488, A522):
- *Was die Rotation tut:* Im Kampf wählt sie unter den drei Begleitern seiner Hörner nach Lage (B0 bis B3). Zwischen
  den Kämpfen setzt sie die drei selbst, passend zum nächsten Kampf (B5).
- *Der Befehl* (seine Aufzeichnung vom 03.11.2026, Build a885b43bf, zehn Zuweisungen): Jede Zuweisung im Bestienbuch
  schickt der Client als `GameMain.ExecuteCommand(2420, Horn I, Horn II, Horn III, 0)`, mit den Nummern der drei
  Begleiter im Buch (XBMPet-Id); 0 leert ein Horn. Die Hornplätze zeigen die neue Belegung 10 bis 16 ms danach; sie geht also
  über den Server. Beim Öffnen des Buchs kommt `2421` ohne Werte. Damit setzt RSR die Hörner ohne das Buch, unabhängig
  von Sprache, Filter und Sortierung. Die drei Bytes im Speicher zu überschreiben, änderte dagegen nur die Anzeige des
  Clients, nicht, was der Server beim Ruf beschwört.
- *Im Kampf nimmt der Server keine Zuweisung an* (sein Protokoll vom 04.11.2026, Build ad1013fb8, eine FATE im
  Südlichen Thanalan auf Stufe 22–23; A521). Auf seine Vorgabe vom 03.11.2026 („beastmaster begleiter im kampf anpassen.
  nicht die aktuellen, sondern die beiden anderen. mal mit cooldown, mal ohne versuchen", „ich will einfach nur
  austesten, in welchen situationen der wechsel klappen könnte") versuchte RSR den Befehl im Kampf in vier Lagen – das
  Horn bereit oder abklingend, ein Begleiter draußen oder keiner –, je einfach, nach dem Öffnungsbefehl des Buchs
  (`2421`) und mit weggesteckter Waffe (seine Idee: „vielleicht muss auch einfach die kampfhaltung beendet werden").
  - *Im Kampf:* zwölf Versuche, keiner angenommen. Die Hornplätze behielten die Belegung, der Server meldete nichts.
  - *Außerhalb des Kampfes:* vier von vier angenommen, nach 0,08 bis 0,38 s, auch mit Begleiter draußen und gezogener
    Waffe.
  - Die Probe ist mit ihrer Option und ihren Protokollzeilen entfernt (seine Vorgabe, 04.11.2026: „da der experimentelle
    versuch der neubelegung der hörner im kampf erfolglos war, kann der teil inkl. logging dieser funktion entfernt
    werden. nicht. dass ein anderer spieler diese option irrtümlich aktiviert, die keinen nutzen hat"). Sie kostete je
    Sitzung bis zu 27 s ohne Begleiter.
- *B5 – Belegung vor dem nächsten Kampf* (sein Vorschlag vom 04.11.2026, eine Mischung aus „nach dem letzten Kampf" und
  „feste Vorlagen je Inhaltsart": „im dungeon hat man walltowall pulls für trashmobs. da sind aoe schäden am
  hilfreichsten, also wie daedalus. aber wenn man zum boss kommt, vor kampfbeginn, wechsel auf eine kombination für
  bosse/einzelgegener. zwischen den trashmob-kämpfen feststellung ob eher schutz oder dmg benötigt wird, bzw. heilung.
  wechsel wenn ausserhalb des aggrobereiches"; Option „Set the battlehorns between fights for the next one", ab Werk an):
  - *Wann:* nur außerhalb des Kampfes. „Außerhalb des Aggrobereichs" heißt für den Server: nicht im Kampf (oben). RSR
    plant neu, sobald sich ändert, was die Wahl liest – ein Kampf endet, ein anderer Gegner wird zum nächsten, ein Boss
    taucht auf, Gebiet, Stufe oder Hörner wechseln –, und schickt eine Zuweisung nur, wenn die beste Belegung von der
    stehenden abweicht. Nicht beim Wirken, nicht tot, nicht ohne geladene Begleiterliste oder Katalog, nicht im Crucible:
    Dort stellt das Brett die Begleiter (`XBMPetParty`, BeastHelper), und ob der Server dort zuweist, ist nicht gemessen.
  - *Seine Hand geht vor:* Ändert er die Hörner selbst, gilt seine Belegung bis zum Ende des nächsten Kampfes; solange das
    Bestienbuch offen ist, plant RSR nicht. Eine Änderung zählt als seine, wenn sie nicht die ist, die RSR zuletzt
    geschickt hat. Sonst nähme RSR ihm eine bewusste Wahl vor dem Pull im selben Augenblick wieder weg.
  - *Was als Nächstes kommt:* Der nächste Gegner ist der nächste lebende Gegner, der noch nicht kämpft, in RSRs
    Gegnerliste (48 y, in Sicht). Trägt er das Boss-Symbol (`BNpcBase.Rank` 2 oder 6, `IsBossFromIcon`), kommt ein Boss:
    ein Ziel. Sonst ein Rudel: so viele Gegner, wie im letzten Kampf in diesem Gebiet zugleich gegen ihn, seine Gruppe
    oder seinen Begleiter kämpften – nicht die, die ein anderer Spieler daneben bekämpft –, mindestens so viele, wie seit
    dem letzten Kampf zusammen in Reichweite einer Fläche um einen nächsten Gegner standen. Diese Zahl steigt bis zum
    nächsten Kampf nur. Neu gelesen wechselte sie beim Gehen mit dem jeweils nächsten Gegner: Am 04.11.2026 setzte RSR in
    1,5 s sechsmal abwechselnd Vespula und Raptor (Rudel aus einem, dann aus zwei Gegnern; A523). Jede Fläche zählt das
    ganze Rudel, wie im Modell: Ein Tank sammelt es.
    - *Beleg zum Boss-Symbol:* Die drei Bosse der Kupferglocken-Mine tragen Rang 6, 6 und 2 (Kottos, BNpcBase 14460;
      Zorniger Ichor, 14448; Gyges der Große, 14537), ihre Gegner davor 0 (14449, 1401, 1421; xivapi v2, 04.11.2026). In
      seinem Mitschnitt vom 02.11.2026 standen die Bosse 13 bis 30 s vor dem ersten Treffer in der Objektliste.
  - *Wie viel Sicherheit:* Vor einem Boss zwei Begleiter, die heilen oder schützen; nach einer Gefahr in diesem Gebiet
    einer mehr – vor einem Rudel also einer, vor einem Boss drei. Gefahr ist, was B2 so nennt (vorausberechnete
    Gesundheit unter der Linie der Fähigkeitsheilung oder Klasse 1), dazu ein Tod; sie gilt bis zum Gebietswechsel.
    - *Messgröße der Wahrscheinlichkeit:* der Boss selbst – seine drei Tode vom 02.11.2026 fielen gegen einen Boss oder
      FATE-Boss (A489) – und die Gefahr, die RSR in diesem Gebiet gemessen hat, also die Schadensrate gegen ihn mit allem,
      was die Gruppe heilt. Ein Heiler in der Gruppe zählt darüber mit.
    - *Bis zum Gebietswechsel, nicht nur nach dem letzten Kampf:* Sonst verhinderte der sichere Begleiter die Gefahr,
      fiele beim nächsten Kampf weg und käme danach wieder; jeder zweite Kampf ohne ihn.
  - *Die Wahl* unter allen freigeschalteten Begleitern mit Katalogeintrag, auf der Stufe des Spielers; was ein Horn
    gerade hält, auch ein leeres, bleibt eine Wahl (sonst bliebe bei weniger Begleitern als Hörnern keine Belegung übrig):
    1. so viele, die heilen oder schützen, wie verlangt – wie B2 es liest, je Horn nach dem, wofür es One with Nature
       verbraucht; Aufhalten nach C1 zählt nur vor einem Rudel;
    2. darunter die Art, die diesen Kampf am besten deckt: vor einem Boss einer, dessen Dressur heilt – B2 hält ihn die
       ganze Gefahr draußen, ein Schutz aus Entfesseln endet nach 48 s –; vor einem Rudel einer, dessen Entfesseln es
       aufhält (C1);
    3. der meiste Schaden gegen diesen Kampf: die Dressur und, auf einem Horn, das One with Nature für Entfesseln
       verbraucht, das Entfesseln – je Attacke-Wert mal die Gegner, die seine Fläche erreicht, auf ein Ziel einmal; ein
       Mehrfachtreffer („sixfold") mit der Zahl seiner Treffer;
    4. nach einer Gefahr im Gebiet mehr, die heilen oder schützen;
    5. möglichst viele seiner Begleiter auf ihrem Horn;
    6. mehr, die heilen oder schützen, dann mehr, deren Dressur heilt.
  - *Das Horn des Begleiters draußen bleibt.* Was eine Zuweisung mit dem Begleiter draußen macht, ist nicht gemessen:
    Die Probe änderte nur Hörner ohne Begleiter draußen. Nach einem Kampf steht meist einer draußen; dann setzt RSR die
    beiden anderen. Beim nächsten Kampf geht er mit dem Scheidenden Schlag, und die beiden gesetzten folgen; sein One
    with Nature hat er im letzten Kampf verbraucht (offen, wartet auf einen Rückzug ohne feindliches Ziel: TODO „Beastmaster:
    Nach dem Kampf ohne One with Nature in den nächsten", A543).
  - *Was im Kampf anders wird* (seine Hörner vom 02.11.2026: Gespenst – sein Entfesseln Odium heilt –, Fledermaus –
    ihre Dressur heilt –, Opo-Opo; Ergebnis des Modells unten):
    - Vor einem Rudel setzt RSR Büffel und Raptor auf die beiden Hörner, die entfesseln. War Cry trifft das Rudel mit
      750 und betäubt es 15 s, Foul Breath trifft es mit 1.000; Opo-Opo bleibt auf dem Horn, das borgt (Fläche wie die
      anderen).
    - Vor dem Boss: Fledermaus und Schleim, deren Dressur heilt; Opo-Opo steht noch draußen. Kommt die Gefahr, ruft B2
      einen der beiden und hält ihn draußen, bis sie vorbei ist.
    - War er im Dungeon schon in Gefahr, kommen vor dem Boss Pugil, Schleim und Fledermaus.
  - *Gerechnet* (`bst_assign_model.py` auf dem Zyklus von `bst_horn_model.py`, 04.11.2026; Annahmen dort benannt: die bis
    Stufe 22 fangbaren Begleiter, seine darunter, drei Rudel zu fünf je 45 s, ein Boss 150 s mit Gefahr ab 30 s):
    - Gegenüber seinen festen Hörnern auf den Rudeln +20,0 % Attacke-Wert, am Boss +1,2 %, die Gefahr am Boss wie zuvor
      zu 100 % gedeckt.
    - Über 324 Fälle (Rudel 15 bis 90 s mit 2 bis 8 Gegnern, Boss 60 bis 300 s, Gefahr ab 2, 30 oder 100 s, vier
      Hornsätze): auf den Rudeln nie weniger. Ohne Gefahr am Boss −2,5 % schlimmstenfalls, −0,3 % im Mittel; mit
      Gefahr bis −15,2 %, weil B2 den heilenden Begleiter draußen hält (Preis von B2, unten). Die Gefahr nie schlechter
      gedeckt als mit seinen Hörnern, im Mittel um 0,29 des Anteils besser.
  - *Lagen:* In der offenen Welt und in FATEs ebenso: Ein Rudel ist, was zuletzt zugleich kämpfte; ein FATE-Boss mit
    Symbol gilt als Boss. In der Gruppe mit Heiler sieht RSR seltener eine Gefahr; die Rudel bleiben dann reiner Schaden.
    Unter Stufe 8 keine Dressur, es zählen Entfesseln (ab 18) und Sicherheit; unter 10 und 20 setzt RSR nur die Hörner,
    die es gibt. Unter 30 kommt Entfesseln nicht mit jedem Ruf (30 s Abklingzeit); die Wahl zählt es trotzdem, B2 prüft
    im Kampf, ob es kommt. Stufensynchronisation: auf der synchronisierten Stufe. BossModReborn liest die Regel nicht.
  - *Spur:* Jede Zuweisung schreibt eine Zeile ins `DefenseTrace.log` – was kommt (Boss mit Namen oder Rudel mit Zahl),
    die verlangte Sicherheit, je Horn alter und neuer Begleiter – und danach, ob der Server sie nahm. Ändert sich der Plan,
    ohne dass eine Zuweisung nötig ist, eine Zeile „kept". Die Zustandszeile nennt die Option.
  - *Preis:*
    - Seine eigene Belegung ändert sich zwischen den Kämpfen; wer sie selbst setzt, schaltet die Option aus.
    - Ein Rudel vor einem sichtbaren Boss kämpft er mit der Boss-Belegung, weil der Boss dann der nächste Gegner ist.
    - Ein Boss, der erst beim Betreten der Arena erscheint und sofort angreift, findet die Rudel-Belegung vor; B3 wählt
      unter ihr, und auf ein Ziel bringen Flächen-Dressuren gleich viel.
    - Entfesseln, die schwächen oder stärken (Verwundbarkeit, Gruppenstärkung), zählen nicht als Schaden, wie in B1 bis
      B3.
  - *Verworfen:*
    - Nichts setzen: auf den Rudeln 20 % weniger als B5 mit seinen Hörnern (Gerechnet).
    - Nur nach dem letzten Kampf: Vor dem Boss stünde die Rudel-Belegung.
    - Feste Vorlagen je Inhaltsart wie bei Daedalus: Sie kennen weder seine Begleiter noch seine Stufe, und im Dungeon
      gälte „Rudel" auch für den Boss.
    - Einer statt zwei vor dem Boss: ohne Gefahr +5,7 % am Boss im Mittel, aber in 36 von 324 Fällen eine spätere Gefahr
      schlechter gedeckt, bis auf 0 %; das Horn des einen heilenden Begleiters klingt dann ab.
    - Nur die Dressur werten (erster Entwurf): Er wählte Schutz statt Schaden aus Entfesseln (Pugil statt Raptor) und
      brachte die Fledermaus nicht zurück; auf den Rudeln −0,6 %, die Gefahr am Boss zu 23 % statt 100 % gedeckt.
    - Das Horn des Begleiters draußen mit umsetzen: Was mit ihm geschieht, ist nicht gemessen; der Gewinn wäre ein Horn,
      dessen Begleiter schon ohne One with Nature draußen steht.
    - Einen Heiler in der Gruppe eigens zählen: Die gemessene Gefahr enthält ihn schon.
  - *Antithesen, mit Versuch der Widerlegung:*
    - „Eine Boss-Belegung bringt keinen Schaden, jede Dressur hat 500": Für die Dressur auf ein Ziel stimmt das. Vor dem
      Boss ändert sich die Sicherheit und das Entfesseln (Büffels Betäubung, gegen die Bosse oft immun sind, gegen
      Schaden). Hält als Einschränkung: Der Gewinn am Boss liegt in der gedeckten Gefahr, nicht im Schaden (+1,2 %).
    - „Zwei sichere vor jedem Boss kosten Schaden, auch wo keine Gefahr kommt": −0,3 % im Mittel, −2,5 % schlimmstenfalls;
      mit einem dagegen in 36 von 324 Fällen eine Gefahr ohne Deckung. Nicht widerlegt; nach seiner Gewichtung überwiegt
      die Deckung, weil Bosse ihn getötet haben (A489).
    - „Die Gefahr bis zum Gebietswechsel hält nach einem einzelnen Fehler einen sicheren Begleiter": trifft zu. Mit
      einem, der zugleich Fläche hat und aufhält (Büffel, Ziz, Chimäre) oder heilt (Gespenst), kostet das auf Rudeln
      nichts; nur wer allein Einzelziel-Heiler hat, verliert auf einem Horn die Fläche. Preis benannt.
    - „Der nächste Gegner ist nicht der nächste Kampf": In der offenen Welt steht oft einer näher, den er nicht angreift;
      dann gilt seine Art. Ein Rudel zählt ab dem ersten Kampf nach dem letzten Kampf, und ein Boss in der Nähe bringt die
      Boss-Belegung, die auf ein Ziel nichts an Dressur kostet.
  - *Gegenhypothesen:*
    - Kein Gewinn: Hat er nur Einzelziel-Begleiter, oder steht das Beste schon auf den Hörnern, ändert B5 nichts; die
      Zeile „kept" zeigt es.
    - Option falsch: siehe Antithesen. Ein Gegner ohne Boss-Symbol (Elite, manche FATE-Gegner) gilt als Rudel der letzten
      Größe; B2 und B3 wählen im Kampf weiter.
    - Ausgeliefert, und nichts ändert sich – warum? Ohne Begleiterliste oder Katalog plant RSR nicht (Zustandszeile),
      im Crucible nicht, ohne sichtbaren Gegner nicht. Folgt ein Kampf ohne Pause auf den anderen, gibt es kein Fenster
      außerhalb des Kampfes. Nimmt der Server nicht an, zeigt es die Zeile „NOT TAKEN".
- *Der Weg über das Bestienbuch* (Daedalus, ofnature/Daedalus e3a16de vom 01.11.2026, `BattlehornAssigner`,
  `BattlehornGame`; Werkzeugquelle, nach eigener Angabe im Spiel aufgezeichnet): Ein Plugin bedient das Buch wie der
  Spieler. Es öffnet das „Bestienbuch des Bändigers" (Hauptkommando, MainCommand 100), blättert über dessen Rückruf
  (`3, Seite`; 25 Begleiter je Seite in der Reihenfolge ihrer Nummer im Buch), öffnet mit `8, Kachel` das Kontextmenü
  eines Begleiters und wählt dort den Eintrag des Horns (Eintrag 0, 1, 2 für Horn I, II, III; Rückruf des Menüs
  `0, Eintrag, 0`). Nach etwa einer Sekunde liest es die Hornplätze zurück; die Zuweisung geht über den Server.
  - Ein Eintrag für das Horn, auf dem der Begleiter schon sitzt, nimmt ihn herunter.
  - Filter oder Sortierung des Buchs verschieben die Kacheln; dann landet die Wahl falsch.
  - Daedalus setzt nur außerhalb des Kampfes und erkennt den Menüeintrag am englischen Wort „Battlehorn".
- *Analogien* (seine Frage, 03.11.2026, mit seinem Hinweis auf den Blaumagier und auf BOCCHI in Kreszentia):
  - *Blaumagier in RSR:* RSR liest das aktive Set jeden Frame aus dem Client
    (`ActionManager.GetActiveBlueMageActionInSlot`, `ActionUpdater`). Ein Zauber des Blaumagiers geht nur, wenn er im
    Set steht (`ActionBasicInfo.IsOnSlot` in der Grundprüfung). Ausnahme sind die Folgezauber, die das Spiel an die
    Stelle eines gesetzten Zaubers legt: White Death über Cold Fog, Divine Cataract über Chelonian Gate
    (`RequiredBluSlotActionId`). Ein Zauber außerhalb des Sets geht nicht, und RSR setzt das Set nie.
  - *Setzer im Client:* Für den Blaumagier kennt FFXIVClientStructs Setzer (`ActionManager.SetBlueMageActions`,
    `AssignBlueMageActionToSlot`, `SwapBlueMageActionSlots`), für Kreszentia
    `PublicContentOccultCrescent.ChangeSupportJob(byte)`. Beides gilt auf b69073a vom 02.11.2026.
  - *BOCCHI* (OhKannaDuh/BOCCHI 9403174 vom 24.10.2026, AGPL-3.0, nur gelesen) ruft `ChangeSupportJob` direkt auf:
    - nur außerhalb des Kampfes und erst 4 s nach seinem Ende;
    - nicht beim Wirken, Springen oder Gebietswechsel (`PhantomJobChangeGate`);
    - danach wartet es höchstens 5 s auf den Status des neuen Phantomjobs.
  - *Für den Beastmaster:* Die Leseseite ist dieselbe. RSR liest die drei Hörner aus dem `ActionManager`
    (`BeastmasterRotation.CurrentPets`), wie das Blaumagier-Set. Die Folgeaktionen eines Begleiters hängen an seinem
    Horn wie Folgezauber an ihrem Platz. Eine Funktion wie `ChangeSupportJob` kennt der Client für die Hörner nicht;
    die Zuweisung ist ein allgemeiner Serverbefehl (oben, „Der Befehl").
  - *Was davon nutzbar ist:* Der Klick im Buch geht über `GameMain.ExecuteCommand` (oben, „Der Befehl"). RSR setzt
    die Hörner wie BOCCHI den Phantomjob nur außerhalb des Kampfes (B5) und liest die Hornplätze danach zurück.

**Aufzeichnung des Bestienbuchs** (sein Auftrag vom 02.11.2026: „bau die aufzeichnung, sie soll sich dann aktivieren,
wenn das buch auf ist, und deaktivieren, wenn es geschlossen wurde"; A488):
- *Wann:* vom Öffnen des Bestienbuchs (`XBMMonsterNotebook` aufgebaut oder gezeigt) bis zum Schließen (abgebaut oder
  verborgen). Sonst schreibt sie nichts; keine Option, sie kostet nur, solange das Buch offen ist.
- *Was, je Zeile mit Uhrzeit, Kampfstatus und den drei Hornplätzen (Nummer im Buch und Begleiter):*
  - jedes Ereignis, das ein Agent empfängt (Agent, Art, alle Werte) – das ist der Rückruf, den ein Klick im Buch oder
    im Menü auslöst;
  - jedes Klickereignis an Bestienbuch und Kontextmenü (Ereignisart, Parameter);
  - jedes Kontextmenü, das aufgeht oder sich erneuert, mit allen Werten, also den Einträgen im Wortlaut des Clients;
  - jede Änderung der Hornplätze, sobald der Server sie bestätigt, und jeder Wechsel des Kampfstatus;
  - jeder Aufruf von `GameMain.ExecuteCommand` (Befehl, vier Werte, Ergebnis), auf den der Client einen Klick abbildet
    (A505). Der Abschnitt nennt am Anfang, ob der Mithörer dafür steht, und am Ende, wie viele Aufrufe kamen. Null bei
    einer Zuweisung heißt: Die Hörner gehen nicht über diesen Befehl.
- *Wo:* `BestiaryTrace.log` im Konfigurationsordner von RSR neben `DefenseTrace.log` (der Ordner, den Dalamud dem Plugin
  gibt). Jede Öffnung des Buchs beginnt einen Abschnitt mit Datum, Uhrzeit, Version und Commit; die Datei wächst,
  Löschen setzt sie zurück.
- *Was sie nicht tut:* Sie klickt nichts, sendet nichts und ändert nichts; sie hört nur zu. Auch den Befehlsaufruf
  reicht sie unverändert weiter. Der Mithörer darauf steht nur, solange das Buch offen ist.

**Fremde Werkzeuge zum Beastmaster** (seine Fragen vom 02.11.2026, ob Masterbeast, BeastHelper, BeastmasterAssist und
Daedalus RSR verbessern können; A488; Stand: Kriss-Hietala/BeastmasterAssist db9e2e3 vom 14.09.2026, MIT;
Rin-0617/BeastHelper be40a1e vom 26.10.2026, AGPL-3.0; lexxxgo/Masterbeast 712bddd vom 19.09.2026, ohne Lizenz;
ofnature/Daedalus e3a16de vom 01.11.2026, GPL-3.0 und LGPL-3.0). Alle Werkzeugquellen; Code nur gelesen.
- *Daedalus* (Rotation für alle Jobs, Beastmaster „Artemis"): der Weg über das Bestienbuch (oben); der Hinweis, der
  zu R1 führte; vorgefertigte Belegungen nach Inhalt (Einzelziel, Dungeon-Rudel, Gruppenunterstützung, Leveln) mit
  Begleitern, deren Wert jenseits von Schaden, Heilung und Schutz liegt (Betäubung, Einfrieren, Verwundbarkeit,
  Gruppenstärkung) – daraus C1 bis C3 unter „Was gilt" (A491). Geprüft ohne Defekt: Daedalus wartet nach der Dressur, bis die
  Aktion des Begleiters gelandet ist, weil eine sofort folgende Axt sonst vor ihr auflöst und die Kombo verkehrt herum
  schließt; in RSR verlangt die Axt nach der Dressur das Herz, das erst die gelandete Dressur gibt (`StatusNeed`, Zweig 1
  des Kompasses). Anfreunden: Daedalus markiert etwa 20 s vor dem geschätzten Tod, um den Tod vor der Markierung zu
  vermeiden; RSR markiert mit dem letzten GCD, dessen Markierung nach dem größten bisherigen Einbruch des Gegners noch
  sicher ankommt (A2), also später, weil die Chance mit niedrigeren LP beim Markieren steigt (offizieller Job-Guide).
  Der Preis von A2 ist ein Stoß, größer als jeder, den der Gegner bis dahin nahm.
- *BeastmasterAssist* (Anzeige, keine Automatik): Empfehlungen zu den Aktionen der Kinships (Soul Crush, Scouring Ash,
  Vileskin, Scaleskin, Beastskin, Seedsower, Quelling Wave); RSR setzt alle schon ein. Neu wäre nur „TP für die
  Abschlüsse halten" (ab Stufe 50, 250 TP); ohne Messung, wie viel Potenz die Äxte darunter verlieren, nicht gewertet.
- *Crucible: Bretter, Schwächen, Gegenstände:* unten, „Crucible of the Unbroken: Bretter, Kämpfe, Strategien" (A532).
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
  02.11.2026, Werkzeugquelle. Eine Funktion, die die Zuordnung setzt, führt es dort nicht; das Buch setzt sie mit einem
  Serverbefehl. Im Kampf nimmt der Server keine Zuweisung an (gemessen, A521); zwischen den Kämpfen setzt RSR sie
  selbst (B5, oben „Belegung der Hörner in der Rotation").
- *Heilen:* ja, über die heilende Dressur (Schleim, Fledermaus) und Odium. Scouring Ash heilt 10 % nur, wenn es einen
  schädlichen Effekt entfernt. Schützen über Beastskin, Scaleskin und Seedsower (Borrow) sowie Vulcanize, Harden Shell,
  Water Wall, Smoldering Scales und Strut (Entfesseln); dazu Kontrolle.
- *Tanken:* nur im Crucible (Wirktexte, xivapi v2 Action 46750–46752, en und de, 04.11.2026). Zähnefletschen
  (Snarl, 15 s Abklingzeit): Der Begleiter setzt Drohen (Threaten) – höchste Feindseligkeit für 15 s –, nimmt 45 s
  sämtlichen Schaden, der dir galt (dein Status „Gedeckt", seiner „Deckung"), und nimmt dir „Erhöhte Feindseligkeit".
  Provozieren (Challenge, 15 s): du stehst 30 s oben auf der Liste, die Deckung endet. Außerhalb des Crucible ist kein
  Weg belegt.
  - *In RSR:* die Einstellung „Use Snarl and Challenge …" (Upstream, im Fork ab Werk an, unten): Zähnefletschen ab 70 % LP des
    Begleiters, Provozieren bei 30 %, unter 30 % deiner LP trägt der Begleiter alles, auch bis zu seinem Tod; wahlweise
    nach dem Vergleich beider LP; ab Werk nur auf dem letzten Horn, also wenn kein anderes Horn bereitsteht. Dazu ein
    Zähnefletschen ohne Schalter gegen Status 2552 im ersten Kampf des ersten Bretts (Upstream-Kommentar; dort wehrt
    der Knochenritter mit Forward Guard Angriffe von vorn ab, nicht die der Begleiter, consolegameswiki).
  - *Zyklus und Deckung (A530):* Solange der Begleiter draußen dich deckt, schickt ihn RSR nicht für den Zyklus weg –
    kein Scheidender Schlag, kein Entfesseln, nach dem er geht (Wespes Schlussstich); die Deckung endet mit ihrer Dauer, mit Provozieren oder mit seinem Fall. Grund: Wer zähnefletschen ließ – du oder die
    Einstellung –, wollte, dass er deinen Schaden trägt, und im Crucible greift der Gegner nur dich an; ein Treffer ist
    also sicher. Im Upload vom 04.11.2026 abends endeten 6 von 22 Deckungen nach 0,7 bis 3,4 s durch den Scheidenden
    Schlag des Zyklus, und die 15 s Abklingzeit sperrten die nächste. Preis: Das nächste Horn kommt bis zu 45 s später,
    mit ihm sein Entfesseln oder Borrow, und die Abklingzeit des gehaltenen Horns beginnt erst mit seinem Rückzug. In
    der Gefahr galt das schon (B2).
  - *Ab Werk an (K9, A550):* Ohne Tank greift der Gegner nur dich an. Fällst du, ist der Lauf verloren, fällt ein
    Begleiter, fehlt nur er bis zum Horn der Rückkehr. Die LP der Begleiter sind also ein zweiter Vorrat, und Sicherheit
    geht vor (CLAUDE.md, „Spielweise"). Der Preis bleibt: LP bleiben über das ganze Brett stehen (gemessen 04.11.2026),
    und die Campsite heilt dich allein um 90 %, mit einem Begleiter je 45 %, mit zweien je 30 % (consolegameswiki).
    Verteilter Schaden kostet dort also Heilung.
  - *Provozieren am größten Treffer (K9, A550):* Zusätzlich zu den Schwellen holt Provozieren den Schaden zurück, sobald
    der Begleiter nicht mehr LP hält als der größte Treffer, den ein Begleiter in diesem Kampf nahm. Gemessen wird er als
    größter Fall seiner LP von einem Bild zum nächsten; mehrere Treffer im selben Bild zählen zusammen, also eher zu groß.
    So fällt er nicht am nächsten solchen Treffer. Unter deiner LP-Untergrenze trägt er weiter alles.
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
  der drei Id-Spalten (Lumina `Unknown4` bis `Unknown6`, im EXDSchema in dieser Reihenfolge Pet, Action, Location), deren
  Werte über alle Begleiter verschieden sind – die Pet-Ids (47 bis 97) sind es, Aktionen und Fundorte nicht – und deren
  Pet-Zeile Aktionen führt. Trifft das auf keine oder mehrere zu, gibt es keinen Katalog, und die eingestellte Reihenfolge
  gilt. Bis A520 prüfte RSR nur `Unknown5` und `Unknown6`, also Action und Location: Auf seinem Client entstand der Katalog
  nie (Protokoll vom 03.11.2026, jede Zustandszeile „NO CATALOG: the familiars' Pet column cannot be told (neither of
  two)"), und B1 bis B3, C1 bis C3 und die Dressur nach Affinität wirkten nicht; es galt nur B0. Form und Reichweite der Dressur aus dem Aktionsblatt (`CastType`, `EffectRange`),
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
  - Einer, dessen Dressur heilt, bleibt draußen, solange die Gefahr anhält: kein Scheidender Schlag für den Zyklus; einer,
    der dich nach Zähnefletschen deckt, ebenso, auch ohne Gefahr (oben, „Zyklus und Deckung"). Seine
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
- *Nutzen jenseits von Schaden, Heilung und Schutz* (A491; TODO-Punkt aus A488). Die Wirktexte aller 50 Begleiter
  (xivapi v2, 02.11.2026) legen diesen Nutzen fast ganz ins Entfesseln. Die Dressur trägt nur Blind (Dodo, Riesenwurm),
  Gift, Albtraum (Baumschrat) und eine kleine Chance auf Todesurteil (Geist). Was davon die Wahl ändert:
  - *C1 – Aufhalten zählt als Schutz gegen ein Rudel.* Ein Entfesseln, das die Gegner um das Ziel aufhält, zählt in B2
    und B3 wie eines, das schützt: Betäubung (War Cry, Büffel, 15 s um den Begleiter), Versteinerung (Petribreath, Ziz,
    21 s im Kegel), Tiefkühlung (The Ram's Voice, Chimäre, 20 s). Das Statusblatt trägt bei allen drei `LockActions`
    („preventing the execution of actions").
    - *Bedingungen:* mindestens zwei Gegner im Kampf greifen dich an; jeder von ihnen steht in der Reichweite um das Ziel
      (Kegel wie Kreis, wie B3); keiner trägt das Boss-Symbol; bei Betäubung ist keiner immun (`StunWatch`), und auf
      keinem liegt eine Betäubung seit seiner letzten Immunität. Die nächste wäre halbiert (The Balance).
    - *Gerechnet* (`bst_control_model.py`): Hält es zwei oder drei Angreifer 15 s auf, verhindert es in jeder Gefahr
      von 10 bis 60 s mehr Schaden als −20 % Verwundbarkeit über 48 s. Bei drei Angreifern und 30 s Gefahr ist es das
      2,5-Fache. Eine halbierte Betäubung oder ein Boss unter den Angreifern kehrt das um; deshalb die Bedingungen.
    - *Am Pull zuerst* (A542): Kommt ein Rudel auf dich zu und lebt neben dir kein Tank, ruft RSR ohne Gefahr zuerst
      den Begleiter, dessen Entfesseln es aufhält, vor B3 und vor der eingestellten Reihenfolge – sofern sein Horn One
      with Nature für Entfesseln verbraucht und Entfesseln mit dem Ruf kommt. Als Angreifer zählt dann jeder Gegner im
      Kampf, gleich wen er in den ersten Augenblicken anvisiert; er wendet sich dir zu, niemand hält ihn. Grund (Südwald,
      04.11.2026, 13:25): Raptor, Büffel und Fliegenfalle brachten auf das Rudel je 500, den Gleichstand entschied die
      Reihenfolge, der Raptor verbrauchte Entfesseln für Schaden, und als 1,5 s später die Gefahr kam, lag War Cry in der
      gemeinsamen Abklingzeit; 781 auf 57 LP in 9 s, tot. Mit dem Büffel zuerst betäubt War Cry das Rudel 15 s, bevor es
      Schaden macht. Preis: der Schaden aus dem Entfesseln des anderen Begleiters kommt später. Mit Tank zählt wie zuvor
      nur, wer dich anvisiert.
    - *Ein Angreifer allein zählt nicht.* Das ist der Fall des Elitegegners oder Bosses (seine drei Tode vom 02.11.2026,
      A489). Ist der Gegner immun, ohne dass das Spiel es zeigt, stünde die Gefahr ohne Schutz da, und der heilende
      Begleiter käme zu spät.
    - *Nicht gezählt:* Schlaf (Lullaby, Schaf) – der nächste Treffer weckt, und RSR greift weiter an (Gemeinschaftsquelle,
      Konzept 08). Lähmung (nur „sometimes"). Fesseln, Schwere, Rückstoß, Heranziehen – sie halten keinen Gegner auf, der
      schon an dir steht. Todesurteil und einstellige LP – nur eine Chance. Stone Gaze (Kobra) – trifft nur das Ziel, also
      nie ein Rudel.
  - *C2 – Elementarschutz zählt nicht als Schutz* (Defekt in B1, ohne Option). Strut (Dodo: Erdschaden −15 %) und Formic
    Pheromones (Treiberameise: Wasser-, Erd- und Eisschaden −15 %) galten wegen „damage taken" als Schutz. Gegen
    physischen oder nicht-elementaren Schaden halten sie nichts ab, und welches Element ein Gegner nutzt, sagt kein
    Blatt. Jetzt zählt „damage taken" nur, wenn der Satz kein Element nennt. Smoldering Scales (magische Verwundbarkeit
    −20 %) und Beastskin (physischer Schaden −25 %) bleiben Schutz.
  - *C3 – Kein Schaden für dich in der Gefahr.* Hypodermic Hustle (Kaktor) gibt der Gruppe +20 % Schaden, „but inflicts
    damage over time"; wie viel, nennt kein Text. Solange du in Gefahr bist, wartet dieses Entfesseln. Der Kaktor bleibt
    dann draußen, oder B2 schickt ihn für einen sicheren Begleiter weg. Preis: der Schadensbonus kommt später.
  - *Unverändert, mit Grund:*
    - Gruppenstärkung (King's Will, Frozen Heart, Scamper) und Verwundbarkeit oder Resistenz am Gegner: zeitlich
      ausrichten bringt zwischen −1,8 und +1,3 % (B4, verworfen).
    - Reinigen (Ultrasonics, Fledermaus): Die Fledermaus zählt schon über ihre heilende Dressur.
    - Stärkung entfernen (Bloodcurdling Caw, Kapgeier): Welche Stärkung eines Gegners zählt, liest RSR nirgends.
    - Belegungen nach Inhalt wie bei Daedalus: Das ist die Wahl der drei Begleiter (B5, oben „Belegung der Hörner in
      der Rotation"), nicht die Wahl unter ihnen.
  - *Gegenhypothesen:*
    - Kein Gewinn: Ohne Rudel und ohne Gefahr ändert C1 nichts. C2 ändert nur etwas mit Dodo oder Treiberameise auf einem
      Horn.
    - Option falsch: Ein Rudel, das nicht vom Boss-Symbol erfasst wird und doch immun ist (Elitegruppen), kostet einen
      Ruf ohne Wirkung. Bei Betäubung zeigt `StunWatch` die Immunität nach dem ersten Fehlschlag; bei Versteinerung und
      Tiefkühlung bleibt das offen. Der Preis ist ein Ruf, danach kommt der nächste sichere.
    - Ausgeliefert, und nichts ändert sich: wenn er keinen Begleiter mit Kontrolle hat (Büffel ab Stufe 8 einfangbar,
      Ziz ab 16, Chimäre ab 38), Entfesseln nicht hat (vor Stufe 18) oder die Hörner auf Borrow stehen.
- Jede Wahl und jeder Wechsel aus Gefahr schreibt eine Zeile ins `DefenseTrace.log`: Horn, Begleiter, Grund.
- *Zustandszeile* (A489): Sobald sich ändert, was diese Regeln und das Anfreunden lesen, schreibt RSR eine Zeile
  „beastmaster state": Begleiter der drei Hörner, wie RSR sie liest; das Horn draußen; die Optionen „Choose the
  battlehorn by the situation" und „Capture familiars you do not have yet"; ob Anfreunden eingeschaltet ist; ob das
  Spiel die Liste der Begleiter geladen hat; Stand des Katalogs; ob die Zielart „wenigste LP" gilt, weil neben ihm
  kein Tank und kein Heiler lebt (Konzept 03, A525). Anlass: Am 02.11.2026 rief RSR in einer Stunde mit
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

*Belegung der Hörner:* Im Kampf wählt RSR unter den drei Begleitern der Hörner; welche drei das sind, entscheidet die
Wahl am meisten. Zwischen den Kämpfen setzt RSR sie nach dem nächsten Kampf: vor einem Rudel Flächen, vor einem Boss
zwei, die heilen oder schützen (B5, oben „Belegung der Hörner in der Rotation").

*Preis:* Die Wahl liest nur, was beim Rufen steht. Ändert sich die Zahl der Gegner im langen Einsatz, bleibt der
Begleiter. Kegel und Linie zählt B3 wie einen Kreis um das Ziel (die Stellung des Begleiters ist beim Rufen unbekannt) –
bei verteilten Gegnern zu viele Treffer. Bleibt ein heilender Begleiter in der Gefahr draußen, fehlen in dieser Zeit die
kurzen Einsätze der anderen (Entfesseln, Scheidender Schlag).

*Verworfen:*
- Die Zuordnung im Kampf ändern: Der Server nimmt sie im Kampf nicht an (gemessen, A521; oben, „Belegung der Hörner in
  der Rotation").
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
  bekannte Gefahrenflächen prüft. Schildansturm nach eigener Grenze: bis zur Nahkampfreichweite des Jobs, unabhängig
  von der Einstellung, nie weiter (W5).
- *W3 – Abschluss ohne Bewegung zuerst.* Risen Fall vor Brutal Rage: gleicher Wert, kein Schritt. Zwischen Calamity und
  Hawkish Talons der, der mehr Gegner trifft (`AffectedTargets` der Zielwahl), bei Gleichstand Calamity. Die Reihenfolge
  der Affinitäten bleibt (Sunstrider vor Moonstalker); die Universality-Kombo bleibt, wo sie war. Steht er weiter als
  10 y, geht Risen Fall aus 25 y; nur Hawkish Talons wartet dann auf die Grenze aus W2.
- *W4 – Soul Crush unterbricht.* Er bringt 300, beim Unterbrechen 1.000 und 100 TP („Potency increases to 1,000 … upon
  interruption"). RSR setzte ihn nur in `AttackAbility` auf das Ziel. *Gilt:* zuerst im Unterbrechungspfad auf einen
  Wirker, dessen Wirken sich unterbrechen lässt; sonst wie bisher. Zurückgehalten für eine Unterbrechung wird er nicht:
  Wie oft in 30 s eine kommt, hängt am Inhalt, und jede verpasste Abklingzeit kostet 300.
- *W5 – Schildansturm auf ein Rudel vorn* (seine Beobachtung vom 04.11.2026: „schildansturm wird anscheinend nicht als
  aoe genutzt. selbst wenn man in der gegnergruppe steht"; seine Vorgabe vom selben Tag: „ansturm unter 3y nutzen, auf
  den gegner in der mitte der gruppe, so dass großmöglichster schaden auf die höchste gegnerzahl gewirkt werden kann";
  A524, A525). Schildansturm (Action 44893, ab Stufe 24, 60 s, ab Stufe 36 drei Ladungen) trifft das Ziel und jeden
  Gegner bis 6 y darum mit 300. *Gilt:*
  - Trifft er mehr als einen Gegner, kommt er nach dem Abschluss und vor Dressur und Äxten, mit jeder Ladung; auf einen
    Gegner bleibt er zuletzt, und ohne Begleiter draußen hält er dort Ladungen (Upstream 8f258dbc5).
  - Nur auf Gegner bis zur Nahkampfreichweite des Jobs (3 y, `DataCenter.JobRange`), unabhängig von der Einstellung aus
    W2 (seine Entscheidung B vom 04.11.2026: „schildansturm entscheidung b"; A529): Steht sie tiefer, etwa auf 0 y,
    springt der Schildansturm dennoch bis 3 y; steht sie höher, nie weiter als 3 y. Alle anderen Anstürme halten die
    Einstellung. Drückt er ihn selbst, per Vorwärts-Befehl oder im Countdown, gilt keine Grenze. Unter den Gegnern bis
    3 y nimmt RSR den, dessen 6 y die meisten Gegner erreichen: die Mitte der Gruppe. Bei Gleichstand ordnet die Zielart
    (ohne Tank und Heiler: wenigste LP, Konzept 03). Der Landepunkt wird gegen bekannte Gefahrenflächen geprüft
    (BossModReborn), bei 0 y nicht.
  - *Warum vorn:* Er stand als letzte Angriffsfähigkeit hinter allem, was nach Sekunden wiederkommt: Dressur (3 s),
    Äxte (5 s), Entfesseln, Borgen, Scheidender Schlag. In den Trash-Kämpfen seines Laufs (5 bis 15 s) blieb ihm kaum
    ein Platz (Schluss aus dem Code).
  - *Gemessen am 04.11.2026* (Build 3ec6f295b, mit W5): Im Südwald um 11:56 lag er 23 s bereit, mit mehr als einem
    Gegner in 3 y, und kam nicht; RSR schrieb keine Zeile „Shield Charge on". Einige Anstürme im Mitschnitt drückte er
    selbst („ich habe die fähigkeiten zwischendurch manuell ausgelöst, da sie nicht kamen").
  - *Die Ursache* (Haltezeile am Nachmittag, Build 59065ea56, A528): Seine Einstellung „Only use damaging gap closers
    within this distance" steht auf 0,0 y. RSR verlangte „näher als" die Einstellung, und näher als 0 y steht kein
    Gegner: Jeder Ansturm war gesperrt, Schildansturm wie Brutal Rage und Hawkish Talons, bei jedem Job. Jetzt gilt die
    Grenze einschließlich, wie der Text „within" sagt und wie seine Regel: Bei 0 y am Ziel ist ein Ansturm nur Schaden.
    In 79 der 1.051 Haltezeilen stand der nächste Gegner bei 0,0 y. Zwischen 0 und 3 y widersprachen sich seine Vorgabe
    „unter 3y" und seine Einstellung 0 y; ihm vorgelegt, er entschied B: der Schildansturm nach eigener Grenze (oben),
    die Einstellung für alle anderen Anstürme. Verworfen (A): die Einstellung auf 3 y, sie hätte jeden Ansturm aller
    Jobs bis 3 y freigegeben.
  - *Preis von B:* Der Schildansturm kann ihn bis zu 3 y ins Rudel ziehen, auch aus einer Stelle, an die er vor einer
    Fläche ausgewichen ist, wenn BossModReborn die Fläche nicht kennt.
  - *Haltezeile:* Liegt er bereit, mit mehr als einem Gegner in Reichweite, und kommt nicht, schreibt RSR einmal je
    Kampf und Art des Grundes (bis A528 je Wortlaut, und der nannte den Abstand, der sich jeden Frame änderte), woran es
    liegt – ausgeschaltet, AoE-Einstellung, kein erlaubtes Ziel (mit Abstand zum nächsten
    Gegner, Nahkampfreichweite, Abstandseinstellung und „No new hostiles"), oder das Ziel erreicht nur einen Gegner.
  - *Preis:* Dressur oder Axt kommen dann einen Platz später; ihre Abklingzeiten sind kurz, die 60 s des Ansturms laufen
    früher an. Steht er weiter als 3 y vom nächsten Gegner, kommt der Ansturm nicht, auch nicht als Einzelziel; das war
    vorher bis zur Abstandseinstellung möglich.
- *Bestienkönnen nach Borgen* (seine Beobachtung vom 04.11.2026: „das jeweilige bestienkönnen habe ich auch nicht
  gesehen"; A524). Borgen gibt die Kinship der Familie des Begleiters; Bestienkönnen wird dann zur Aktion dieser Familie
  (Familie aus XBMPet, im Spiel Addon 17741–17748; deutsche Namen xivapi Action language=de):
  - *Biesthaut* (Beastkin, etwa Opo-Opo, Büffel) und *Saatsäher* (Seedkin, etwa Fliegenfalle): sofort im Kampf, sobald
    Borgen sie freigibt. Saatsäher stand bis A524 als letzte Angriffsfähigkeit, hinter Schildansturm und Seelenschmerz,
    obwohl B2 und B5 einen Seedkin auf einem Borgen-Horn als Schutz zählen.
  - *Schuppenpanzer* (Scalekin, etwa Raptor): in der Gefahr sofort, sonst für einen angekündigten Treffer oder ohne
    Heiler (B2).
  - *Seelenschmerz* (Soulkin): zum Unterbrechen (W4), sonst als letzte Angriffsfähigkeit.
  - *Gezieferpanzer* (Vilekin, etwa Vespula) nur gegen einen Rückstoß, *Wolkenstreich* (Cloudkin, etwa Fledermaus) nur als
    Bewegung, *Läuternde Asche* (Ashkin) nur zum Reinigen.
  - *Brechende Welle* (Wavekin, etwa Pugil): nur gegen ein Ziel mit einer der genannten Stärkungen. Die zweite Zeile, die
    sie ohne diese Bedingung nutzen sollte, steht hinter Schmetteraxt und wird nie erreicht (Upstream 2fe925f41); offen:
    TODO „Beastmaster: Brechende Welle als Schaden".
  - Borgen kommt höchstens einmal je Ruf seines Horns, und das Horn mit Borgen ist ab Werk das dritte. In kurzen
    Kämpfen kommt es oft gar nicht an die Reihe; sein Bestienkönnen dann auch nicht.
- *Kampfzeile* (A524): Am Ende jedes Kampfes schreibt RSR, wie oft Schildansturm, Borgen und jedes Bestienkönnen kamen –
  von RSR oder von ihm, gezählt an den Wirkungen – und wie lange Schildansturm bei mehr als einem Gegner in Reichweite
  bereit lag, ohne zu kommen. Bis A525 zählte sie jede Aktion in jedem Frame neu („Shield Charge 73" in einem Kampf am
  04.11.2026), weil sie den Zeitpunkt aus der Zeit seit der letzten Aktion zurückrechnete; jetzt zählt sie die
  Aufzeichnung der Aktionen nach ihrer Zeit.

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


**Crucible of the Unbroken: Bretter, Kämpfe, Strategien** (A532). Sein Auftrag vom 05.11.2026: „alle bretter von
crucible researchen, analysiere, strategien entwickeln. konzept bauen. im vollen kritischen loop"; dazu sein Beispiel
als Vorschlag (K2) und seine Fragen, ob RSR im Brett Begleiter wählen und Gegenstände einsetzen kann.

*Quellen* (05.11.2026): Spielblätter über xivapi v2 mit dem Schema exdschema@2:rev:a1e24a0c (`XBMContent`,
`XBMContentBattle`, `XBMBattleDetail` mit einer Unterzeile je Gegner: Name, Schwäche, Werte, Zustandsresistenz über
`BNpcResist`, zwei Aktionen; `XBMPet`, `Pet.Abilities`, `XBMItem`, `XBMContentCamp`), en und de mit Zeilen-Id;
BossModReborn 577675d (31.10.2026), Module aller fünf Bretter; consolegameswiki „Crucible of the Unbroken" und „First
Board of the Unbroken"; BeastHelper be40a1e (26.10.2026, AGPL, nur gelesen) und FFXIVClientStructs b69073a (02.11.2026),
beide Werkzeugquellen; sein Mitschnitt vom 04.11.2026.

*Regeln des Spiels, die eine Strategie tragen:*
- Fünf Bretter (`XBMContent`): erstes Stufe 30, Rang 5, Kader 10; zweites 40, 10, 12; drittes 50, 15, 14; Erstes
  Meisterbrett 50, 20, 12; Zweites Meisterbrett 50, 25, 15. Ein Brett ist ein Weg aus Feldern mit Abzweigungen: Kampf,
  Elite, Endgegner, Campsite, Laden, Schatz, Zufall (Wiki).
- Den Kader wählst du vor dem Betreten; je Kampf gehen drei auf die Hörner, einer ist draußen. Die Belegung gilt je
  Kampf und wird vor ihm gesetzt (Wiki; seit 7.57 automatisch nur bei höchstens drei im Team).
- Keine natürliche LP-Regeneration, für dich und die Begleiter (Wiki); deine LP bleiben von Kampf zu Kampf stehen
  (gemessen 04.11.2026). Normale Heiltränke sind gesperrt, nur Brett-Gegenstände und Essen.
- Campsite: Du ruhst mit bis zu einem bis vier Begleitern (`XBMContentCamp.FamiliarRecoverCount`, je Lager); die Heilung
  teilt sich nach dem Wiki zum ersten Brett „allein 90 %, mit einem je 45 %, mit zweien je 30 %", die Hauptseite sagt,
  du heilst immer selbst. Gemessen einmal (04.11.2026, 19:44–19:47): +30 % deiner LP. Ein gefallener Begleiter wird
  dort nicht geheilt.
- Fällst du, ist der Lauf verloren, außer eine Bestien-Lebensfeder, ein Opferring oder Sand der Zeit greift. Fällt
  ein Begleiter, fehlt er bis zum Horn der Rückkehr.
- Vermeidbare Flächen legen dem Getroffenen Verwundbarkeit oder Schadensminderung auf; hält dein Begleiter die
  Feindseligkeit, trägt er sie, und Schaden nimmt er aus ihnen keinen. Unvermeidbare Flächen treffen euch beide (Wiki).
- Jeder Gegner trägt eine Schwäche aus neun Arten (Hieb, Stich, Schlag, Feuer, Eis, Wasser, Wind, Erde, Blitz) oder
  keine, dazu Anfälligkeiten für Zustände (`BNpcResist`). Wie viel mehr Schaden eine Schwäche bringt, nennt keine
  Quelle (unbelegt).
- Punkte (Wiki): Zuschläge für keine Gegenstände, keine Rast, keine Schwächung, kleine Teams, gefallene oder keine
  gefallenen Begleiter. Sie stehen teils gegen deine Sicherheit; RSR richtet sich nach Sicherheit vor Schaden
  („Spielweise"), Punkte sind kein Ziel einer Regel (meine Ableitung).

*Die Bretter* (`XBMContentBattle`, `XBMBattleDetail`; Schwäche in Klammern; deutsche Namen aus den Blättern):
- *Erstes Brett:* Knochen-Ritter und -Bischof (Schlag; der Ritter wehrt mit Forward Guard Angriffe von vorn ab, nicht
  die der Begleiter, Ossify unterbrechbar); Erzdämonen-Ebenbild (Blitz); Pisco-Dämonen-Ebenbild (Hieb; anfällig für
  Gift und Gewicht); Terrormilben- und Schattenmilben-Ebenbild (Eis; Tankbuster mit Gift); Oger-Ebenbild (Wind) mit
  Irrlicht- und Riesen-Irrlicht-Ebenbild (Wasser; anfällig für Fessel und Gewicht). Endgegner Seelenlose (Pas de Seul,
  Stich): ruft nach etwa 45 s eine Sukkubus-Magierin und zwei -Ritterinnen (Feuer; die Magierin stärkt ihn mit
  Fanaticism, 9 s Wirkzeit, anfällig für Unterbrechen).
- *Zweites Brett:* Manticore (Wind; Tankbuster); Taurus (Stich), ruft Ahriman (Eis; Blick mit Verhängnis); Wyvern
  (Blitz); Leerenmagus (Erde) mit Zombie (Eis); Älterer und Jüngerer Taurus (Feuer; der Jüngere stärkt); Dämon (Hieb),
  Böse Waffe (Eis), Eufel (Erde). Endgegner Losfrox Tintenklecks (Schlag) mit Töffelchen (Wasser).
- *Drittes Brett:* Kavalier (Feuer) mit Knochen-Bischof (Schlag); Ymir (Eis) mit Sahagin (Schlag, Dreadwash
  unterbrechbar); Montisaurus (Erde) mit Hahn und Henne (Schlag); Catblepus (Wind; Blick mit Versteinerung); Lakhamu
  (Wasser) mit Golem (Wasser, stärkt); Sirene (Stich, ruft Gegner) mit schlurfendem und kriechendem Ebenbild (Feuer);
  Kämpfer, Kaktor, Blütenkaktor (heilt), Soldat (Blitz) und Wächter (Eis, deckt andere). Endgegner Guttler (keine) mit
  Thanatos (Blitz).
- *Erstes Meisterbrett:* Strix (Stich; Federn als Adds); Morbol, Keimling, Ochu (Hieb) mit Aasbrühe (Eis); Titanenwurz
  (Wind) mit Falkenkönigin (Schlag); Frostdrache und Eis-Exergon (Feuer); Gargoyle (keine; Tankbuster); Administrator,
  Käfer, Fußsoldat, Brummonaut, Drohne (Blitz; Confound unterbrechbar) mit Kobra (Stich); Golem (Wasser); Baumschrat,
  Schnecke, Schössling (Schlag) mit Schaudermilbe und Biloko (Eis; Biloko stärkt); Progenitrix, Granate, Bomber (Wind)
  mit Snoll (Feuer) und Pyrobolos (Wasser). Endgegner Borgny Giftschleuder (Erde) mit vergiftender Substanz (Wasser).
- *Zweites Meisterbrett:* Flauros und Blitz-Exergon (Erde); Gedankenschinder (Hieb) mit Fliegerpilz (Wasser);
  Butzemann (Eis) mit Glotzauge und Bomber (Wind; Blick mit Versteinerung) und Licht-Exergon (keine); Atomos (Schlag)
  mit Gremlins und Vodoriga (Stich), Pudding (Wind), Creme (Blitz), Fran (Erde), Bedrohlicher Dahak (Eis); Drakon
  (Wind) mit Dornmull (Feuer), Abaddon (Wasser), Morpho (Schlag); Durga (Feuer, ruft Gegner) mit Drehturm (Schlag);
  Medusa (Stich, ruft Gegner) mit Lamia (Stich, Raise unterbrechbar) und Zyklop (keine); Chimära (Wasser); Sphinx
  (Hieb; Rätsel); Gigantis und Zyklop (keine) mit erstarrtem Blitz (Erde) und Zunder (Wind); Moogle-Kampf (Blitz,
  Schlag, Wasser; Heiler und Stärker); Behemoth (Erde) mit Riese (Blitz) und Blitz-Exergon (Erde); Königs-Ahriman (Eis)
  mit Hapalit (Wind) und Trübauge (Eis). Endgegner Lauda Spruchspalterin (keine) mit Thanatos (Blitz).
- Häufigste Schwächen: erstes Brett gleich verteilt; zweites Eis; drittes Blitz und Schlag; Erstes Meisterbrett Blitz,
  Wind, Schlag; Zweites Meisterbrett Schlag, Blitz, Erde, Wind.

*Schadensarten der Begleiter* (`Pet.Abilities`, Aspekt und Angriffsart der Dressur D und des Entfesselns E; Pet-Zeilen
47–97, de): Hieb Cu Sith (D, E), Puk (E), Mantis (D), Dullahan (D); Stich Schaudermilbe, Vespula (D, E), Kaktor, Kolibri
(D, E), Treiberameise, Morbol, Kobra, Hydra, Karlabos (D); Schlag Schaf, Opo-Opo, Mandragora, Büffel, Goobbue,
Montisaurus (D), Spriggan, Karpfenkröte (E); Feuer Baumhörnchen, Puk, Spriggan, Drakon, Chimäre (D), Raptor, Behemoth
(E); Eis Ziz, Uragnit, Karpfenkröte, Raptor, Eisgolem (D), Chimäre (E); Wasser Pugil, Megalokrabbe, Krabbe, Apkallu,
Salamander (D), Megalokrabbe, Apkallu, Karlabos (E); Wind Kapgeier, Fliegenfalle, Geist, Kleinlibelle, Rafflesia (D),
Kapgeier, Baumschrat, Montisaurus (E); Erde Dodo, Golem, Riesenwurm, Baumschrat, Verrottende Goobbue (D), Golem,
Riesenwurm (E); Blitz Kobalos, Gespenst, Adaman-Taimai, Coeurl, Behemoth (D), Coeurl (E). Unterbrechen setzen Kobalos,
Dullahan, Golem, Spriggan und Eisgolem (`XBMPet.InflictsInterrupt`).

*Strategien* (je mit Wirkung im Kampf, Abwägung, Gegenthese; Stand am Ende):
- *K1 – Schwäche treffen.* Unter den bereiten Hörnern zählt ein Begleiter, dessen Dressur oder Entfesseln die Schwäche
  der Gegner trifft, um den Zuschlag der Schwäche mehr (B3 rechnet heute Potenz mal Treffer). Den Zuschlag lernt RSR an
  den eigenen Treffern: Schaden je Potenz auf schwache gegen andere Gegner, je Brett. Bis zur ersten Messung zählt er
  nicht. *Stärke:* der passende Begleiter steht länger draußen. *Schwäche:* ohne Messung keine Wirkung; ein Brett mit
  gleich verteilten Schwächen gewinnt wenig. *Gegenthese:* Der Zuschlag ist klein, und eine Fläche auf drei Gegner zählt
  mehr. Nicht widerlegt, aber genau das rechnet das Produkt: Ein kleiner gemessener Zuschlag ändert die Reihenfolge
  nicht.
- *K2 – Horn für spätere Gegner aufheben* (sein Vorschlag, Beispiel Seelenlose). Das Blatt nennt je Kampf alle Gegner;
  wer zu Beginn fehlt, kommt später. Trifft ein bereiter Begleiter deren Schwäche und keinen der Anwesenden, ruft RSR
  ihn nicht im Zyklus, sondern sobald sie erscheinen, und greift sie an, bis sie fallen (die Zielart „wenigste LP" tut
  das schon, A525). *Beleg:* Am 04.11.2026 um 19:48 kam der Raptor (Entfesseln Feuer) 8 s nach Kampfbeginn und ging
  6 s später; sein Feuer traf die Seelenlose (Stich). Die drei feuerschwachen Sukkubi erschienen 45 s nach Beginn, alle
  Hörner klangen ab. *Stärke:* die Gegner, die den Boss stärken oder dich zusätzlich treffen, fallen früher. *Schwäche:*
  das gehaltene Horn ruht bis zu ihrem Erscheinen, ein Einsatz weniger im Kampf. *Gegenthese:* Die Adds fallen auch so
  schnell (1.700 bis 2.000 LP gegen 19.600 des Bosses). Teilweise: Der Gewinn hängt am Zuschlag (K1) und an der Dauer
  bis zum Ruf; das Halten kostet nur, solange das Horn sonst einen Einsatz hätte. Der Ruf kommt laut Blatt sicher. Gilt
  für alle Kämpfe mit Ruf: Seelenlose, Taurus, Sirene, Ymir, Kämpfer, Strix, Progenitrix, Drakon, Durga, Medusa,
  Flauros.
- *K3 – Wen RSR angreift: größter Nutzen für den Kampf und das Brett* (seine Vorgabe vom 05.11.2026: „entscheidung, wer
  angegriffen werden soll nach analyse des größten nutzens für das gesamte spielfeld und darüber hinaus für das gesammte
  brett"; im Crucible; außerhalb dieselbe Regel ohne den Bedarf des Bretts, Konzept 03, „Zielart ohne Tank und Heiler", A537).
  *Gebaut* (A541, `EnemyDanger`): der allgemeine Teil – Gefahr je Sekunde aus den Treffern auf die Gruppe, angekündigte
  Treffer, Heilungen auf andere Gegner, geteilt durch die LP, Wechsel über der Messunsicherheit –, auch im Crucible.
  Offen in Stufe 1: die Werte des Blatts vor der ersten Messung, der Zuschlag der Schwäche (K1), Schaden an Begleitern
  gewichtet nach ihrem Bedarf über das Brett (K11), Stärkung anderer. *Was gilt:* Zuerst fällt der Gegner mit der größten Gefahr je
  Sekunde bis zu seinem Fall: seine Gefahr je Sekunde geteilt durch die Zeit, bis er fällt. Diese Reihenfolge macht die
  Summe des Schadens über den Kampf am kleinsten (Regel von Smith, Ablaufplanung); „wenigste LP zuerst" ist ihr Sonderfall
  bei gleicher Gefahr aller Gegner. *Gefahr:* sein Schaden an dir und an deinen Begleitern, je als Anteil an den LP, die
  der Getroffene für den Rest des Bretts braucht (dein Lauf, ihre Einsätze nach K11) – Schaden an einem Begleiter, der
  später unersetzlich ist, wiegt so mehr als an einem, der nicht mehr gebraucht wird; dazu, was er für andere tut: geheilte
  LP (Blütenkaktor, Biloko, Kupoheiler), die LP eines zurückgeholten Gegners (Raise der Lamia), der Mehrschaden der
  Gestärkten (Sukkubus-Magierin, Jüngerer Taurus, Melomog), die Wirkung beim Erreichen des Verbündeten (Irrlichter, die zum
  Oger laufen). *Zeit bis zum Fall:* seine LP durch deinen Schaden auf ihm, mit dem Zuschlag seiner Schwäche (K1). *Start
  und Nachsteuern:* vor der ersten Messung die Werte des Blatts (Stärke, Intelligenz, Konstitution, Aktionen), dann der
  gemessene Schaden je Gegner und Sekunde (`hit you` je Quelle, Treffer auf Begleiter) und die korrigierte Restzeit.
  *Im Kampf:* Atomos-Kampf mit acht Gegnern: Pudding, Creme und Fran (Fire III, Water III, Thunder III) vor den Gremlins, auch wenn
  ein Gremlin weniger LP hat; Medusa-Kampf: die Lamia, sobald sie Raise wirkt; Oger-Kampf: die Irrlichter, bevor sie den
  Oger erreichen. *Gegenthese 1:* Zu Kampfbeginn fehlen Messungen. Widerlegt: Das Blatt gibt Werte je Gegner, die Messung
  ersetzt sie nach den ersten Treffern. *Gegenthese 2:* Zielwechsel kosten Laufwege und Rhythmus. Teilweise: Gewechselt
  wird nur, wenn die neue Reihenfolge den Unterschied über die Messunsicherheit hinaus zeigt; die Ausweichbewegung bleibt
  bei BossModReborn. *Gegenthese 3:* Endet ein Kampf mit dem Fall des Hauptgegners, wäre er zuerst richtig. Offen: In
  seinen Läufen fielen die Gerufenen immer vor dem Hauptgegner (04.11.2026); die Spur hält fest, ob beim Fall des
  Hauptgegners noch Gegner leben und ob der Kampf dann endet. *Gegenthese 4 (sein Grund zu A525, „weniger gegner machen
  weniger schaden"):* trägt die Regel – sie rechnet genau diesen Schaden, statt ihn über die LP zu schätzen.
  *Falsifikation:* Kein Unterschied zu „wenigste LP"? Im Atomos-, Medusa- und Oger-Kampf doch. Option falsch? Eine
  verrauschte Messung ließe das Ziel springen – der Wechsel verlangt einen Unterschied über ihrer Unsicherheit. Nichts
  ändert sich? Im Crucible überschreibt heute A525 die Zielart; die neue Regel tritt dort an ihre Stelle, die Spur nennt
  jeden Wechsel mit Grund.
- *K4 – Unterbrechen.* Ein Begleiter, der unterbricht, gegen anfällige Gegner mit langem Zauber (Ossify, Fanaticism,
  Dreadwash, Confound, Raise). Welche seiner Aktionen unterbricht, ist am Blatt noch zu belegen; danach ein Loop.
- *K5 – Zustände.* Gift, Gewicht, Fessel gegen anfällige Gegner: Wirkung je Zustand nicht beziffert, nach K1 bis K4.
- *K6 – Hörner vor dem Kampf belegen.* B5 setzt die Hörner zwischen den Kämpfen per Serverbefehl, im Crucible bisher
  nicht. Vor einem Kampf im Brett kennt RSR dessen Gegner nicht aus der Sicht, aber aus dem Blatt, sobald das Feld
  bekannt ist. Zwei Wege: B5s Befehl (ob der Server ihn dort annimmt, ist nicht gemessen) oder die Auswahl im Fenster
  (`XBMPetParty`, Rückrufe; BeastHelper und AutoDuty nutzen sie, Werkzeugquellen). Belegung nach K1 und K2 mit einem
  Heiler, wenn die Gefahr im Brett das verlangt.
- *K7 – Kader vor dem Brett* (Loop vom 05.11.2026; seine Frage, ob RSR „auf wunsch begleiter auswählen" kann; seine
  Vorgabe: das Spielerlebnis vor der Schwierigkeit der Umsetzung). *Optionen:* A nichts; B die Empfehlung zeigen, sobald
  das Teamfenster offen ist; C dazu ein Knopf „übernehmen", der sie auf deinen Wunsch setzt; D ungefragt setzen.
  *Im Kampf und davor:* Unter A und B wählst du 10 bis 15 Begleiter selbst und weißt nicht ohne Nachschlagen, welche Schwäche
  das Brett verlangt; unter C ein Klick; unter D überschreibt RSR einen Kader, den du anders geplant hast. *Gegenthese 1
  (Absturz):* BeastHelper dokumentiert einen Absturz durch nachgebaute Fensterereignisse mit leerem Ereigniszeiger.
  Teilweise widerlegt: C nutzt die Spielfunktionen, die das Fenster selbst ruft (`AgentXBMPetParty.TogglePet`,
  `ConfirmSelection`), prüft nach jedem Schritt die Auswahl (`SelectedPets`) und hört bei einer Abweichung auf; ob sie aus
  einem Plugin heraus tragen, zeigt erst der erste Lauf, die Spur nennt jeden Schritt. *Gegenthese 2:* Das Brett hat
  Abzweigungen und Zufallsfelder. Widerlegt: Das Blatt nennt alle Kämpfe des Bretts, Zufallsfelder ziehen aus ihnen.
  *Gegenthese 3:* Ein Begleiter unter dem Rang des Bretts ist schwächer. Eingearbeitet: Rang mindestens wie das Brett
  synchronisiert (`GetSyncedRank`, `IsPetOverSyncedRank`), sonst nachrangig. *Gegenthese 4 (Aufwand):* nach seiner
  Vorgabe kein Grund. *Ergebnis:* C, entschieden (seine Frage zeigt den Wunsch, „auf Wunsch" schließt D aus): Wahl nach
  Abdeckung der Schwächen aller Kämpfe des Bretts, gewichtet nach Zahl der Gegner, dazu ein Heiler und, wo Gegner
  unterbrechbar wirken, einer, der unterbricht.
- *K8 – Gegenstände* (gebaut: A551 bis A555, unten „Stufe 3: K8"). Über `InstanceContentCrucibleOfTheUnbroken.UseItem(Platz, Bestie)` auf dein Ziel. Sicherheit zuerst
  (seine Regel): Bestien-Heiltrank oder -Pulver, wenn deine LP in die Gefahr fallen und keine Heilung bereit ist;
  Bestien-Gegengift und Bestien-Augentropfen gegen den passenden Zustand; Bestien-Verhärtungsmittel vor einem angekündigten
  Tankbuster; Bestien-Lebensfeder oder Sand der Zeit außerhalb des Kampfes vor dem Endgegner. Schaden: der Reißzahn der passenden Schwäche auf die
  gerufenen Gegner (K2) oder ein Rudel – so hat er es am 04.11.2026 um 19:04 selbst getan. *Gegenthese:* Gegenstände
  sind ein Vorrat für das ganze Brett. Teilweise: Sicherheitsgegenstände gehen nur in der Gefahr; Schadensgegenstände
  nur auf gerufene Gegner oder vor dem Endgegner. Der Punktezuschlag ohne Gegenstände steht dagegen: Option.
- *K9 – Feindseligkeit (Zähnefletschen, Provozieren).* Gebaut (A550; oben, „Tanken"): ab Werk an, Provozieren dazu am
  größten Treffer des Kampfs. Neu belegt (Wiki): Hält der Begleiter die Feindseligkeit, trägt er
  die Schwächungen vermeidbarer Flächen und nimmt aus ihnen keinen Schaden. Zähnefletschen schützt dich also auch dort,
  wo du nicht ausweichst. *Gegenthese:* Seine LP bleiben über das Brett stehen, ein gefallener fehlt. Teilweise:
  Provozieren vor seinem Fall (am größten bisher gesehenen Treffer, nicht an festen 30 %) hält ihn; die Campsite heilt
  dich allein am meisten (gemessen +30 % bei geteilter Rast), also kostet verteilter Schaden dort Heilung. Empfehlung:
  die Einstellung ab Werk an, auf dem letzten Horn, mit Provozieren am größten Treffer; Loop bei der Umsetzung.
- *K10 – Zwischen den Kämpfen (Campsite, Laden, Weg, Rauchbombe):* bisher als „nicht RSR" ausgenommen, mit dem Grund,
  dass andere Werkzeuge es führen. Nach seiner Vorgabe vom 05.11.2026 (Spielerlebnis vor Umsetzung) ist das kein Grund
  mehr; offen, im eigenen Loop: wen die Campsite heilt (dich, die heilenden, die tragenden), welche Gegenstände im Laden
  die Sicherheit tragen, welcher Weg Lager und Läden mitnimmt.

- *K11 – Begleiter über das Brett planen* (sein Hinweis und seine Vorgabe vom 05.11.2026: „da begleiter über die
  spielfelder des brettes hinweg schaden ansammeln und hp sinkt, auch strategische planung, falls begleiter mehrfach
  eingesetzt werden sollen, damit diese an den spielfeldern, wo sie unbedingt notwendig sind, auch bereitstehen").
  *Gemessen (04.11.2026):* Im zweiten Lauf fiel die Fledermaus – einer der beiden Heiler – bis zum vierten Kampf auf 25
  von 550 LP, die Fliegenfalle bis zum Endgegner auf 236 von 608, der Schleim auf 265 von 462; im ersten Lauf Opo-Opo,
  Megalokrabbe und Pugil auf etwa 55 bis 60 %. Ohne Regeneration trägt jeder Einsatz seine LP in den nächsten Kampf.
  *Was gilt (Plan):* RSR führt je Brett eine Bedarfsliste aus dem Blatt: je Kampf die Begleiter, die dort unersetzlich
  sind – der einzige mit der Schwäche der gerufenen Gegner (K2), der einzige, der einen unterbrechbaren Zauber stoppt
  (K4), ein Heiler für die gefährlichen Kämpfe –, und je Begleiter seine LP und den Schaden, den er je Einsatz bisher
  nahm. Daraus: Ein Begleiter, der später unersetzlich ist, geht in früheren Kämpfen nur auf ein Horn, wenn seine LP
  danach für seinen Kampf reichen (K6); er trägt keinen Schaden für dich über Zähnefletschen (K9); an der Campsite ruht er
  zuerst, wenn seine LP nicht reichen (K10); ein Bestien-Heiltrank auf ihn, wenn keine Campsite mehr vor seinem Kampf
  liegt (K8). Fällt er trotzdem, kommt das Horn der Rückkehr vor seinem Kampf. *Gegenthese 1:* Der Weg verzweigt, Felder
  sind zufällig – welcher Kampf kommt, steht nicht fest. Teilweise widerlegt: Die Liste gilt für die Kämpfe, die auf dem
  gewählten Weg noch erreichbar sind, und wird nach jeder Abzweigung neu gerechnet; Zufallsfelder ziehen aus den Kämpfen
  des Bretts. *Gegenthese 2:* Wer zurückgehalten wird, fehlt in den frühen Kämpfen, wo er ebenfalls passt. Nicht
  widerlegt: Gegen die Gefahr, ihn im wichtigen Kampf nicht zu haben, steht die Gefahr im frühen; RSR wägt mit dem
  gemessenen Schaden je Einsatz und hält ihn nur zurück, solange ein anderer Begleiter den frühen Kampf trägt.
  *Gegenthese 3:* RSR kennt das aktuelle Feld nicht. Offen: Das Spiel führt das Feld im Brett
  (`AgentXBMStageDetailList`, Werkzeugquelle); ob es lesbar ist, zeigt die Umsetzung, sonst gilt der Plan für alle
  Kämpfe des Bretts. *Ergebnis:* K11 ist die Planungsebene über K6, K7, K8, K9 und K10; sie alle lesen dieselbe
  Bedarfsliste.
  *Stand der Umsetzung (A550, A556):* Die Kämpfe je Brett liefert das Blatt (`XBMContentBattle`: Zeile je Brett 1 bis 5
  mit 6, 7, 8, 10 und 14 Kämpfen, Unterzeile je Kampf mit Verweis auf `XBMBattleDetail`, Unterzeile 0 der Endkampf).
  Kader und Feld führt FFXIVClientStructs seit 17.09.2026 (d44bc123, 3120c745; Stand aad0de66 vom 05.11.2026,
  Werkzeugquelle), in Dalamuds Fassung vorhanden: `AgentXBMPetParty` mit `SelectedPetIds` (Kader, `XBMPet`-Zeilen),
  `PetCurrentHealth` und `PetMaxHealth` (15 Plätze) und `Mode` (1 Vorschau des Kaders, 2 Vorschau aus dem Inhalt);
  `AgentXBMStageDetailList` mit `Mode` (1 laufendes Brett), Einträgen je Feld und Kampf (`EntryType` 3 mit
  `BattleDetailId`, gebaut aus dem Director) und `EntrySelection`. Ob das Spiel sie füllt, während ein Brett läuft und
  die Fenster zu sind, sagt keine Quelle. Die Sonde (A556) schreibt sie beim Betreten des Bretts, beim Erkennen eines
  Kampfes und an seinem Ende in die Spur („crucible agents …"); sie liest nur Felder und ruft keine Spielfunktion. Die
  Planungsregel wartet auf diese Zeilen; der Loop am Ende des TODO baut auf ihnen auf.

*Falsifikation:* Kein Defekt, weil das erste Brett zweimal ohne Tod fiel? Das gelang mit Gegenständen und Zähnefletschen
von seiner Hand; die Meisterbretter haben Rang 20 und 25 und Gegner ohne Schwäche. Option falsch? K1 ohne gemessenen
Zuschlag ist wirkungslos, aber nicht falsch; K2 kann ein Horn umsonst halten, wenn ein gerufener Gegner nie erscheint
oder der Kampf vorher endet. Gehalten wird deshalb nur, solange ein anderer Begleiter draußen steht oder ein anderes Horn
bereit ist; ist das gehaltene das einzige und keiner draußen, ruft RSR es: Ohne Begleiter fehlen Dressur, Kombo und
Heilung sofort, der Zuschlag gegen die Adds später nur vielleicht. Ausgeliefert, und nichts ändert sich? Liest Lumina die XBM-Spalten
anders als das Schema (wie bei `XBMPet`, Spalte `Unknown4`), fehlen Schwäche und Gegnerliste still; RSR schreibt deshalb
beim Betreten, ob es die Daten des Bretts gelesen hat.

*Stufen* (TODO „Beastmaster: Crucible-Strategien"): 1. Daten zur Laufzeit (Schwäche und Gegnerliste je Kampf,
Schadensart je Begleiter) mit K1, K2 und K3; 2. K11 (Bedarfsliste und LP je Begleiter) mit K9; 3. K8; 4. K6 und K7;
danach K4, K5, K10.

**Stufe 1, erster Teil: Daten und K2** (A548, gebaut; K1 und der Rest von K3 folgen):
- *Kämpfe aus dem Blatt:* `XBMBattleDetail` hat eine Zeile je Kampf und eine Unterzeile je Gegner. Gelesen werden der
  Name (`BNpcName`) und die Schwäche (`XBMElement`). Die Reihenfolge der Spalten belegt EXDSchema, Zweig latest
  (abgerufen 05.11.2026, Werkzeugquelle): Name, Icon, Action1, Action2, Resist, Strength, Intelligence, PhysResistance,
  MagResistance, Constitution, Weakness. Die gebaute Lumina liest sie als `Unknown0` bis `Unknown10`; ihre benannte
  Fassung (`Experimental`) ist als vorläufig gesperrt (PendingExcelSchema), wie bei `XBMPet`.
  Probe: Der Knochen-Ritter (Zeile 1, Unterzeile 0) trägt 7 = Schlag, wie oben.
- *Der laufende Kampf:* Im Brett und im Kampf ist es die Zeile, die die meisten anwesenden Gegner nennt (nach `NameId`),
  bei Gleichstand die mit den wenigsten Gegnern. Nennt eine alle, ist es also sie; ein Anwesender, den kein Blatt
  führt (ein Helfer mit eigenem Namen), verdeckt den Kampf nicht. Wer beim Erkennen anwesend ist, gilt als „zu Beginn"; wer in der Zeile steht und
  in diesem Kampf noch nicht zu sehen war, als „kommt noch" – ein Gekommener, der fällt, kommt nicht wieder. Nach dem
  Kampf wird neu erkannt.
- *Was ein Begleiter trifft:* Dressur und Entfesseln (`FamiliarProfile`) je mit ihrem Element (`Action.Aspect` Feuer bis
  Wasser) oder, bei physischem Aspekt, ihrer Angriffsart (Hieb, Stich, Schlag). Zugeordnet wird über die englischen Namen
  von `XBMElement` und RSRs Aufzählungen `Aspect` und `AttackType`, ohne eigene Zahl. Ein Zauber ohne Element trifft
  keine Schwäche.
- *Selbstprüfung beim Betreten:* Eine Zeile „crucible: … fights … read" nennt, wie viele Kämpfe und Gegner gelesen sind,
  und als Probe die Schwäche des ersten Gegners. Fehlt sie oder steht dort 0, ruhen die Regeln dieser Stufe, und die
  Zeile sagt es. Erkennt RSR einen Kampf, nennt eine Zeile die Anwesenden und die Kommenden mit ihrer Schwäche.
- *K2, gebaut:* In der Hornwahl nach Lage (B3, unter „familiars by the situation") wird ein bereites Horn im Zyklus
  übersprungen, dessen Begleiter die Schwäche eines kommenden Gegners trifft und keine der Anwesenden. Das gilt nur,
  solange ein anderes, nicht gehaltenes Horn bereit ist; sonst kommt er wie bisher (Falsifikation oben). Erscheint ein
  Kommender, dessen Schwäche ein bereiter Begleiter trifft, wird dieser zuerst gerufen – nach Gefahr (B1) und dem
  Aufhalten eines Rudels (C1). Eine Zeile nennt das Halten einmal je Kampf und Horn und den Ruf.
- *Im Kampf:* Seelenlose (Stich), Raptor auf einem Horn (Entfesseln Feuer), Sukkubi (Feuer) nach 45 s. Der Raptor
  bleibt im Zyklus zurück, solange ein anderes Horn bereit ist, und kommt, sobald die Sukkubi erscheinen.

**Stufe 3: K8, Gegenstände des Bretts** (A551 bis A555, gebaut). Zwei Einstellungen, beide ab Werk an: „Use the
board's items that keep you and your familiar alive (Crucible only)" und „Use the board's damage items and buffs: damage
on a pack once a fight, all of them in the final fight (Crucible only)".
- *Welche* (`XBMItem`, xivapi Schema a1e24a0c, 05.11.2026; Name in Spalte 2, englischer Wirktext in Spalte 3, daraus Art
  und Größe; die deutschen Namen aus demselben Blatt mit `language=de`, Zeilen in `action_names_de.json`):
  - Heilung: Bestien-Heiltrank I bis IV („Restores 10/23/36/50% of HP to self or familiar") und Bestien-Pulver I bis III
    („… to self and nearby allies").
  - Bestien-Lebensfeder I und II („Grants Reraise to self … 70/95% chance of automatic revival upon KO", keine Dauer genannt).
  - Sand der Zeit („Grants Sandstill to self … Returns you to the moment combat commences upon incapacitation", nur
    außerhalb des Kampfes).
  - Bestien-Heiltranksammlung („Auto-potion: Restores 40% of HP automatically when HP falls below 50%", 150 s).
  - Bestien-Gegengift und Bestien-Augentropfen (heilen Gift und Blindheit und 25 % LP).
  - Schutz: Bestien-Verhärtungsmittel („Grants Tough Skin to self or familiar … Reduces damage taken by 20%", 60 s),
    Grimoire der Unantastbarkeit („five stacks of Blink … Nullifies most physical attacks", 120 s), Grimoire der
    Reflexion („Grants Reflect … Reflects most magic attacks back at the caster", 30 s).
  - Gegen ein Element: die Bestien-Feuer-, -Wasser-, -Erd-, -Blitz-, -Wind- und -Eisresistenzmittel I und II („Grants
    Through Fire to self and nearby allies … 50/70% chance to absorb fire damage as HP", 15 s).
  - Für den Begleiter: Bestien-Goldnadel („Instantly cures most instances of petrification for familiar, restoring 50% of
    maximum HP … Additionally grants Stoneskin", 50 % seiner LP, 60 s).
  - Mehr LP: Bestien-Ausdauermittel („Grants Liquid Constitution while also restoring 10% of HP to self or familiar …
    Increases maximum HP by 20% Effect ends upon combat completion").
  - Stärkung: Bestien-Stärkungs-, -Zauber-, -Aufbraus-, -Wendigkeitsmittel („Grants Liquid Strength/Magic/Intensity/
    Evasion to self or familiar": physischer oder magischer Schaden +30 %, kritische Trefferquote +30 %, Ausweichen
    +25 %, 15 bis 30 s), Bestien-Verkürzungsmittel (Hasty Spirit, Wirk- und Abklingzeiten −15 %, 60 s),
    Bestien-Flinkheitsmittel (Spirit of the Breathtaker bis Kampfende), Bestien-Feder (100 TP),
    Bestien-Blutsaugerserum (Life Siphon, 10 % des Schadens als LP, 60 s). Die Reißzähne und Sternensturm sagen:
    „Damage is affected by magic damage increase effects of other beast gear, items, or abilities".
  - Zustände: Bestien-Heilmittelsammlung („Grants Auto-remedy to self … Automatically cures the next status ailment
    suffered", 240 s) und die Seren I und II (Bestien-Antigift-, -Antiparalyse-, -Antiblind-, -Antiversteinerung-,
    -Antischlaf-Serum: „Increases poison/paralysis/blind/petrification/sleep resistance by 100%", 180 s; II auch für
    Verbündete in der Nähe, 300 s).
  - Schaden: die sechs Reißzähne (Feuer 2.500, die übrigen 2.000 Potenz auf das Ziel und alle Gegner in 12 Yalm; dazu je
    Element Deep Freeze, Paralysis, Heavy, ein entfernter Stärkungszustand oder 15 Yalm Rückstoß), der
    Blutsauger-Reißzahn (Stich, 1.500 bis 3.300 nach deinen maximalen LP, teils als LP aufgenommen) und Sternensturm
    (Feuer 3.000 in 18 Yalm). Art, Potenz, Radius und Rückstoß liest RSR aus dem Text, das Element ordnet es über die
    englischen Namen von `XBMElement` der Schwäche zu (wie K1).
- *Der Endkampf:* Unterzeile 0 jedes Bretts in `XBMContentBattle` (`Unknown0`) ist der Kampf gegen den Endgegner: Zeilen
  6, 13, 21, 31, 45 mit Seelenlose, Losfrox Tintenklecks, Guttler, Borgny Giftschleuder, Lauda Spruchspalterin (xivapi,
  05.11.2026); die übrigen Unterzeilen sind die Kämpfe davor. Brett-Gegenstände gehen nach dem Lauf verloren, höchstens
  zehn passen ins Inventar (consolegameswiki, „Crucible of the Unbroken", Abschnitt „Ephemeral Items", 05.11.2026). Im
  Endkampf zählt der Vorrat deshalb nicht mehr.
- *Die Zustände eines Kampfes:* `XBMBattleDetail` führt je Gegner zwei Aktionen (`Unknown2`, `Unknown3` →
  `XBMBattleDetailAction`: Aktion, Status, Zielart, Wirkart als `Unknown0` bis `Unknown3`; Spaltenfolge EXDSchema latest,
  05.11.2026). Über alle Bretter verursachen sie unter anderem Paralysis, Poison, Doom, Petrification, Blind, Confused,
  Heavy, Hysteria, Nightmare, Stone Curse, Devoured und Down for the Count. Als gefährlich zählt ein schädlicher Status
  (`StatusCategory` 2), der dir die Kontrolle nimmt (`LockActions` oder `LockControl`: Petrification, Hysteria, Nightmare,
  Confused, Devoured, Down for the Count, Stone Curse) oder dessen englischer Wirktext den Tod nennt (Doom, Stone Curse).
  Ob die Bestien-Heilmittelsammlung einen davon heilt, sagt keine Quelle („status ailment", unbelegt).
- *Wann, einer zur Zeit, in dieser Reihenfolge* (erste Einstellung, außer 12 und 13):
  1. Im Kampf unter „Use HP potions below this HP": der Heilgegenstand, der dich mit dem kleinsten Wert wieder über die
     Linie hebt, sonst der größte; bei gleicher Heilung der Heiltrank vor dem Pulver. Normale Heiltränke sperrt das Brett.
  2. Im Kampf gegen Gift oder Blindheit auf dir das passende Mittel.
  3. Steht kein Reraise, die stärkere Bestien-Lebensfeder, im Kampf und außerhalb. Ihr Wirktext nennt keine Dauer, also
     gilt sie bis zu einem Fall. Geht Reraise, ohne dass du fielst, schreibt die Spur das, und von da an kommt eine
     Lebensfeder nur noch gegen einen Boss.
  4. Außerhalb des Kampfes, solange kein Sandstill steht, der Sand der Zeit.
  5. Gegen einen Gegner mit Boss-Symbol, solange kein Auto-potion steht, die Bestien-Heiltranksammlung.
  6. Schutz für dich gegen einen angekündigten Treffer auf dich: ein bekannter Tankbuster (Liste, gelernte Tabelle,
     Markierung, BossModReborn mit dir in der Maske; `DataCenter.IsTankbusterAimedAt`) oder ein gemessener gezielter
     Treffer, der eine Abwehr verlangt (A540). Im Endkampf auch, sobald ein Gegner dich angreift. Gegen einen
     physischen Treffer (`AttackType` 1 bis 4) das Grimoire der Unantastbarkeit, gegen einen magischen (5) das der
     Reflexion, sonst oder wenn es fehlt das Bestien-Verhärtungsmittel; nicht, solange Tough Skin, Blink oder Reflect
     auf dir steht.
  7. Derselbe Schutz für deinen Begleiter gegen einen angekündigten Treffer auf ihn, wenn er nicht mehr LP hält als der
     größte Treffer, den er in diesem Kampf nahm – dasselbe Maß, mit dem Provozieren dich zum Ziel macht (K9). Im
     Endkampf gegen jeden angekündigten Treffer auf ihn.
  8. Gegen die Zustände des Kampfes, sobald er erkannt ist und das Blatt für einen seiner Gegner einen gefährlichen
     Status führt: zuerst das Serum, dessen Text diesen Status nennt (je Status einmal im Kampf), sonst die
     Bestien-Heilmittelsammlung, solange kein Auto-remedy steht.
  9. Gegen einen Treffer mit Element: Ist ein Treffer auf dich angekündigt (wie 6) oder erreicht dich ein Flächentreffer
     (`DataCenter.AreaHitReachesPlayer`), und trägt ein laufender Cast dabei ein Element (`CrucibleBoard.ElementsOf`,
     wie K1), das Resistenzmittel dieses Elements mit der höheren Chance, solange sein Status nicht steht. Es wirkt
     15 s, der Cast landet innerhalb seiner Wirkzeit.
  10. Ist dein Begleiter versteinert (ein Status mit „Petrification" im englischen Namen), die Bestien-Goldnadel auf ihn.
  11. Gegen einen Gegner mit Boss-Symbol, solange kein Liquid Constitution steht, das Bestien-Ausdauermittel auf dich.
  12. Stärkung (zweite Einstellung), nur im Endkampf: jedes Mittel, das dir einen Status gibt („Grants … to self"),
      solange er nicht steht, auf dich; vor den Schadensgegenständen, weil eine Steigerung des magischen Schadens die
      Reißzähne mitträgt. Außerhalb des Endkampfs bleiben sie liegen: Ihr Wert für einen späteren Kampf ist nicht
      bekannt (K11), im Endkampf ist er null.
  13. Schaden (zweite Einstellung) auf dein Gegnerziel: im Endkampf jeder Schadensgegenstand, der wertvollste zuerst;
     sonst einer je Kampf, sobald er mehr Gegner trifft als dein Ziel. Wert ist Potenz mal der Summe über die
     getroffenen Gegner aus eins plus dem gemessenen Schwächezuschlag, wo das Element ihre Schwäche ist (K1); bei
     Gleichstand der ohne Rückstoß. Ein Gegenstand, dessen Element nur die Schwäche noch kommender Gegner trifft, wartet
     auf sie (wie K2).
- *Wie:* `InstanceContentCrucibleOfTheUnbroken.UseItem` (FFXIVClientStructs, in Dalamuds Fassung vorhanden) wirkt auf
  das Soft- oder Hard-Ziel. RSR setzt das Soft-Ziel kurz auf dich, deinen Begleiter oder dein Gegnerziel und stellt es
  zurück, wie BeastHelper (be40a1e, Werkzeugquelle, nur gelesen). Die Status liest RSR nach ihrem englischen Namen. Die
  Reichweite eines Wurfs nennt kein Blatt; das Gegnerziel ist das, das RSR ohnehin angreift.
- *Nachweis zur Laufzeit:* Nach dem Einsatz wartet RSR, bis der Platz einen anderen Gegenstand zeigt, höchstens einen GCD.
  Bleibt er gleich, schreibt die Spur „not taken" und versucht diesen Platz in diesem Kampf nicht mehr. Ob ein Gegenstand
  gerade gesperrt ist, prüft das Spiel über eine Funktion, die FFXIVClientStructs nicht führt; RSR prüft stattdessen die
  Animationssperre und den Verbrauch.
- *Selbstkorrektur der Sammlung:* Steht ein gefährlicher Status des Kampfes länger als einen GCD auf dir, während
  Auto-remedy steht, heilt die Sammlung ihn nicht. Die Spur nennt ihn, und gegen einen Kampf, dessen gefährliche Status
  alle so erkannt sind, kommt keine Sammlung mehr.
- *Gegenthesen:*
  - Gegenstände sind ein Vorrat für das ganze Brett. Teilweise widerlegt: Heilung erst an deiner Heiltrank-Linie und die
    kleinste passende zuerst; Reraise und Sandstill sind Versicherung, die nur vor dem Fall zählt; Schutz nur gegen
    angekündigte Treffer; im Endkampf ist der Vorrat wertlos.
  - Schutz gegen jeden Angriff statt nur gegen angekündigte: Solange der Begleiter die Feindseligkeit hält (K9, ab Werk
    an), trifft dich ein Tankbuster selten, und jeder Gegenstand wirkt einmal. Nicht widerlegt bleibt der Preis: Ein
    unangekündigter großer Treffer findet keinen Schutz.
  - Der Schutz für den Begleiter fehlt dir später. Er geht nur, wenn sein Fall droht; dann ziehst du die Feindseligkeit
    ohnehin auf dich (K9), sein Fall wäre sicher, dein späterer Bedarf ist es nicht.
  - Blink kann von den Angriffen anderer Gegner verbraucht werden, bevor der angekündigte Treffer landet. Ein wirkender
    Gegner greift nicht automatisch an; mehrere Gegner auf dich können Stapel kosten. Preis benannt.
  - Die Sammlung heilt den nächsten Zustand, auch einen harmlosen (Heavy). Nicht widerlegt; Preis: eine Sammlung. Das
    Serum ist für seinen Status sicher und geht deshalb vor.
  - Ein Schadensgegenstand je Kampf vor dem Endkampf ist eine Setzung: Wie viele Kämpfe noch kommen, liefert das Spiel
    nicht (K11). Sie folgt seinem eigenen Spiel am 04.11.2026 um 19:04, ein Reißzahn auf ein Rudel.
  - Ausgeliefert, und nichts ändert sich: Hält er keine dieser Gegenstände, oder erkennt RSR den Kampf nicht (Zeile
    „crucible fight …" fehlt), ruhen 8, 9, 12 und 13 und der Endkampf; die Spur zeigt beides.
  - Das Resistenzmittel hilft nur, wenn der Cast das Element trägt, das er anzeigt; ein Treffer, den nur BossModReborn
    ankündigt, nennt keines, dann wartet es. Preis benannt.
- *Nicht in diesem Teil:* Die „Ungezähmten" Mittel (Ungezähmte Bestien-Stärkungsmittel und verwandte) legen dir selbst
  Stun, Nightmare, Petrification, Blind oder Pollen auf – RSR setzt sie nie ein (Sicherheit). Das Horn der Rückkehr
  belebt einen gefallenen Begleiter nur auf dem Brett, außerhalb eines Kampfes; welcher gefallen ist, könnten die LP je
  Begleiter in `AgentXBMPetParty` zeigen – ob sie im Brett gefüllt sind, misst die Sonde aus K11 (A556). Bestien-Rauchbombe (Flucht über das Brett-Fenster), Auge des Diebs und des
  Händlers (Beute und Marken, keine Wirkung im Kampf): kein Einsatz durch RSR. Grimoire der Zauberschmiede und des
  Stahlstachels machen allen Schaden magisch oder physisch, auch den der Begleiter; ob das gegen einen Gegner hilft,
  hängt an seinen Resistenzen (`PhysResistance`, `MagResistance` im Blatt, Bedeutung der Zahlen unbelegt) – kein
  Einsatz, bis das belegt ist.

**Stufe 1, zweiter Teil: K1 und der Rest von K3** (A549, gebaut; Daten in `CrucibleBoard`):
- *K1, Zuschlag gemessen:* Keine Quelle nennt ihn; consolegameswiki und Eorzean Tavern sagen nur, dass Schwächen
  zählen (05.11.2026). RSR misst ihn an jedem Treffer von dir und deinen Begleitern auf einen Gegner des laufenden Kampfs.
  Gezählt wird der Schaden je Potenz, getrennt nach Treffern auf seine Schwäche und anderen, je Aktion und Brett.
  - Gemessen ist erst, wenn dieselbe Aktion beides getroffen hat. So fallen Stärke und Rang des Begleiters heraus; die
    Abwehr der Gegner bleibt drin.
  - Der Zuschlag ist das Mittel der Verhältnisse, gewichtet nach der kleineren Zahl der Treffer; bis dahin zählt er nicht.
  - Gezählt werden nur Aktionen mit einer einfachen Potenz im Wirktext (`ActionTooltip.TryPlainPotency`) und mit Element
    oder physischer Angriffsart.
  - Eine Zeile nennt den ersten Messwert je Brett.
- *K1 in der Hornwahl (B3):* Der Wert einer Dressur zählt jeden getroffenen Gegner, dessen Schwäche sie trifft, mit dem
  Zuschlag. Der passende Begleiter bleibt so länger draußen.
- *K3, Zeit bis zum Fall:* Seine LP geteilt durch eins plus Zuschlag mal Anteil. Der Anteil ist der Teil deines Schadens
  in diesem Kampf, den Aktionen mit seiner Schwäche brachten. Gemessen, ohne Annahme, wer mehr Schaden macht.
- *K3 vor der ersten Messung:* Hat noch kein Gegner getroffen, reiht RSR im Crucible nach dem größeren Wert aus Stärke und
  Intelligenz des Blatts geteilt durch die LP. Bei Gleichstand und außerhalb des Crucible gilt: wenigste LP zuerst. Die
  Blattwerte sind Stufen von 1 bis 4, kein Schaden; sie ordnen nur, bis die Messung da ist.
- *Gegenthese:* Die Abwehr schwacher und anderer Gegner unterscheidet sich, also verzerrt sie den Zuschlag. Nicht
  widerlegt: Eine Aktion, die beide Arten trifft, mittelt über Gegner mit unterschiedlicher Abwehr, und jede weitere Messung
  verschiebt das Mittel. Die Folge ist klein, denn K1 ordnet nur die Hörner, und K3 wechselt das Ziel nur über der
  Messunsicherheit (A541).
- *Ausgeliefert, und nichts ändert sich?* Trifft keine Aktion im Brett beide Arten, bleibt der Zuschlag 0, und B3 und K3
  laufen wie vor Stufe 1. Die Zeile zum ersten Messwert fehlt dann.

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
