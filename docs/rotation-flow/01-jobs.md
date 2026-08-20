# 01 · Ablaufstruktur je Job

Grundlage: Override-Matrix und `GeneralGCD`-Skelette, beide maschinell aus dem
Quellcode extrahiert (nicht aus dem Gedächtnis rekonstruiert). Stand: Branch
`claude/bmr-mitigation-refresh`.

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
| `Kurzschluss` | verlässt die Methode sofort, bevor irgendeine Rotationslogik läuft |
| `Sustain` | proaktives Aufrechterhalten eines eigenen Effekts, kein Reagieren |
| `Ressource` | Verbrauch/Overcap-Schutz einer Job-Ressource (Lily, Chakra, Aether …) |
| `Burst` | nur innerhalb eines Burst-Fensters relevant |
| `DoT` | Aufrechterhalten eines Ziel-Debuffs |
| `AoE` | Zweig, der an Gegneranzahl gekoppelt ist |
| `Combo` | positions-/reihenfolgengebundene Kette |
| `Filler` | Standardaktion, wenn nichts anderes greift |
| `Level-Kette` | dieselbe Aktion in mehreren Aufstiegsstufen, absteigend geprüft |

---

## Heiler

### WHM — 633 LOC, 18 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealAreaGCD · HealSingleGCD · RaiseGCD
  oGCD    Emergency · General · Attack · DefenseArea · DefenseSingle · HealArea · HealSingle
  Flags   CanHealSingleSpell · CanHealAreaSpell

GeneralGCD
  ├ 0  Kurzschluss    ThinAir + Raise  → RaiseGCD
  ├ 0  Kurzschluss    Swift + Raise    → base
  ├ 1  Sustain        TrySustainRegenOnTank
  ├ 2  Ressource      Afflatus Misery / Rapture (Lily-Overcap)
  ├ 3  Burst          Glare IV
  ├ 4  Ressource      Confession-Ablauf
  ├ 5  AoE            Holy-Zweig
  ├ 6  DoT            Aero-Kette
  ├ 7  Level-Kette    Glare III → Glare → Stone IV → Stone III → Stone II → Stone
  └ 8  Ressource      Lily-Downtime-Verbrauch
```

Besonderheit: zwei Kurzschlüsse statt einem, weil WHM mit Thin Air eine eigene
Rez-Vorbedingung hat. Die Level-Kette ist mit sechs Gliedern die längste im
gesamten Repo.

### AST — 743 LOC, 20 Configs

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

### SGE — 997 LOC, 25 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealAreaGCD · HealSingleGCD · RaiseGCD
  oGCD    Emergency · General · Attack · DefenseArea · DefenseSingle · HealArea · HealSingle
  Flags   CanHealSingleSpell · CanHealAreaSpell · DisplayRotationStatus

GeneralGCD
  ├ 0  Kurzschluss    Swift + Raise → base
  ├ 1  Reaktiv        DoEukrasianPrognosis II / Prognosis / Diagnosis
  ├ 2  Sustain        TrySustainEukrasianDiagnosisOnTank
  ├ 3  Ressource      Phlegma (Ladungs-Overcap)
  ├ 4  Zustand        Party-/Tank-Scan
  ├ 5  Bewegung       Toxikon bei IsMoving
  ├ 6  AoE            Eukrasian Dyskrasia → Dyskrasia
  ├ 7  DoT            Eukrasian Dosis III → II → Dosis  (Level-Kette)
  ├ 8  Filler         Dosis
  └ 9  Leerlauf       Eukrasia außerhalb Kampf / ohne Ziel, Anti-Brick
```

Einziger Heiler mit einem zweistufigen Cast-Modell (Eukrasia + Folgeaktion).
Das erzwingt eine eigene Vorstufe (`_EukrasiaActionAim`), die kein anderer Job
kennt — die neun Stufen sind größtenteils diesem Modell geschuldet.

### SCH — 924 LOC, 23 Configs

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
`SpeedAbility`. Als einziger Heiler **kein** Sustain-Zweig — SCH ist von der
Wall-to-Wall-Mechanik dieses Forks bewusst ausgenommen (Schilde statt HoT).

---

## Tanks

Gemeinsam: alle vier belegen dieselben sieben Slots, keiner hat GCD-Heilung
oder Raise. Unterschiede stecken ausschließlich in `GeneralGCD` und in der
Mitigations-Staffelung.

### PLD — 531 LOC, 15 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealSingleGCD
  oGCD    Emergency · General · Attack · DefenseArea · DefenseSingle · MoveForward
  Flags   CanHealSingleSpell · DisplayRotationStatus
```

Einziger Tank mit `HealSingleGCD` (Clemency) und `CanHealSingleSpell`. Nutzt
als einziger Tank eine Magie-/Physik-Phasenteilung (Requiescat-Fenster).

### WAR — 444 LOC, 12 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD · HealSingleGCD
  oGCD    General · Attack · DefenseArea · DefenseSingle · HealSingle
```

Kein `EmergencyAbility`-Override. `HealSingleGCD` für Bloodwhetting-Kontext.
Einfachster Tank-Aufbau.

### DRK — 486 LOC, 8 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD
  oGCD    Emergency · Attack · DefenseArea · DefenseSingle · HealSingle · MoveForward
  Flags   CanHealSingleAbility · HasHostileCountAoeMitigation
```

Einziger Job im Repo mit `CanHealSingleAbility`-Override (TBN-Logik).

### GNB — 549 LOC, 2 Configs

```
Hook-Profil
  GCD     CountDown · GeneralGCD
  oGCD    Emergency · Attack · DefenseArea · DefenseSingle · HealSingle
  Flags   HasHostileCountAoeMitigation
```

Wenigste Configs aller Jobs (2). Höchste Kartuschen-/Combo-Dichte in
`GeneralGCD`, aber fast keine Nutzer-Stellschrauben.

---

## Melee

### DRG — 421 LOC, 5 Configs

```
├ 1  AoE       Coerthan Torment / Sonic Thrust
├ 2  Combo     Trait-gestaffelte Kette (Lance Mastery I/II/IV, je ±Trait)
└ 3  Ranged    Piercing Talon
```

Auffällig: **acht** `if (Trait.EnoughLevel) / if (!Trait.EnoughLevel)`-Paare
hintereinander — die Combo ist nach Trait-Stufen dupliziert statt datengetrieben.
Kein `CountDownAction`.

### MNK — 621 LOC, 6 Configs

```
├ 1  Ressource   Beast-Chakra-Zustand
├ 2  Burst       Winds/Fires Reply
├ 3  Form        Formless Fist → Opo-Opo-Form
├ 4  Burst       Perfect Balance ± Solar
└ 5  Leerlauf    kein Ziel in Reichweite
```

Formen-Zustandsautomat statt linearer Combo. Einziger Melee mit
`HealAreaAbility` statt `HealSingleAbility` — Asymmetrie ohne erkennbaren Grund.

### NIN — 1144 LOC, 5 Configs

```
├ 1  Burst      Trick/Mug-Fenster + Ninjutsu-Vorbedingung
├ 2  Mudra      Ausführungszustand (zweigeteilt: !IsExecuting / IsExecuting)
├ 3  AoE        Hakke Mujinsatsu / Death Blossom
├ 4  Combo      Aeolian Edge → Gust Slash → Spinning Edge
└ 5  Leerlauf   Hide-Verwaltung außerhalb Kampf
```

Größte Datei aller Jobs bei nur 5 Configs. Der Mudra-Zustandsautomat ist die
einzige Stelle im Repo, an der ein GCD über mehrere Frames „im Bau" ist.

### RPR — 601 LOC, 3 Configs

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

### SAM — 533 LOC, 4 Configs

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

### VPR — 1056 LOC, 9 Configs

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

### BRD — 611 LOC, 11 Configs

```
├ 1  DoT        Iron Jaws (zwei Zweige: normal + Vorzieh-Refresh)
├ 2  Burst      Resonant Arrow / Apex / Radiant Encore / Blast Arrow
├ 3  AoE        Shadowbite → Wide Volley → Ladonsbite → Quick Nock
├ 4  DoT        Stormbite / Caustic Bite (je ±Level)
└ 5  Filler     Refulgent → Straight Shot → Burst Shot
```

Einziger Fernkämpfer mit `DispelAbility`-Override.

### MCH — 654 LOC, 7 Configs

```
Hook-Profil  CountDown · GeneralGCD · Emergency · Attack · DefenseArea · DefenseSingle
```

**12 Level-Ketten-Glieder** — mit Abstand die meisten aller Jobs. MCHs
`GeneralGCD` ist fast vollständig eine Abfolge von Aufstiegsstufen.

### DNC — 521 LOC, 4 Configs

```
Hook-Profil  CountDown · GeneralGCD · Emergency · Attack · DefenseArea
             · HealArea · HealSingle · MoveForward · DisplayRotationStatus
```

Einziger Fernkämpfer **ohne** `DefenseSingleAbility`, aber mit
`HealAreaAbility` — spiegelbildliche Asymmetrie zu MNK.

---

## Magische Fernkämpfer

### SMN — 818 LOC, 14 Configs

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

### RDM — 710 LOC, 9 Configs

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

### PCT — 575 LOC, 8 Configs

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

### BLM — 199 LOC, 4 Configs *(BLM_Default)*

```
├ 1  Burst     Flare Star
├ 2  Phase     InFireOrIce (Kernautomat, ausgelagert)
├ 3  Element   AddElementBase
├ 4  Filler    Scathe
└ 5  Erhalt    MaintainStatus
```

Kleinste Nicht-Trivial-Rotation. Fast die gesamte Logik liegt in privaten
Hilfsmethoden statt in `GeneralGCD` — **das architektonische Gegenmodell zu
allen anderen Jobs** und der einzige Job, dessen `GeneralGCD` beim Lesen ohne
Kommentare verständlich ist.

*Hinweis:* `BLM_RP.cs` ist eine zweite, alternative BLM-Rotation mit eigener,
deutlich flacherer Thunder-Level-Kette. Die Override-Matrix oben führt beide
unter „BLM" zusammen und ist für `BLM_RP` deshalb nicht belastbar.

---

## Limited Jobs

### BLU — 964 LOC, 10 Configs

Belegt **alle 23** Dispatch-Slots — als einziger Job im Repo. `GeneralGCD` hat
81 Zweige auf einer Ebene, ohne Untergliederung. Struktur: eine flache
Prioritätsliste von Einzelzaubern.

### BSM — 51 LOC, 0 Configs

Belegt vier Slots, `GeneralGCD` besteht aus `return base.GeneralGCD(out act);`.
Der Minimalfall — nützlich als Referenz dafür, was ein Job *mindestens* braucht.

---

## Quantitative Auffälligkeiten

| Kennzahl | Wert | Bedeutung |
|---|---|---|
| Jobs mit `GeneralGCD`-Override | 23 / 23 | einziger wirklich universeller Hook |
| Jobs mit `AttackAbility`-Override | 23 / 23 | zweiter universeller Hook |
| Jobs ohne `CountDownAction` | 2 (DRG, VPR) | unbegründete Lücke |
| Jobs ohne `EmergencyAbility` | 3 (RPR, SAM, WAR) | unbegründete Lücke |
| Level-Ketten-Glieder gesamt | 52 in 12 Dateien | größte Einzelredundanz |
| Redundantes `X.EnoughLevel && X.CanUse` | 43 | `CanUse` prüft das bereits selbst |
| Swiftcast/Raise-Kurzschluss | 13 Kopien in 4 Dateien | identischer Wortlaut |
| Spannweite Dateigröße | 51 – 1144 LOC | Faktor 22 |
| Spannweite Configs | 0 – 25 | Faktor unbegrenzt |
