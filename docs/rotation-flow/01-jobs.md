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
Job überhaupt besitzt und welche davon der Baum benutzt, steht in `05-action-coverage.md`; die
Zusammensetzung der Gruppe, in der ein Job spielt, in `02-groups.md`. Dieses Dokument beschreibt
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
  ├ 0  Kurzschluss    ThinAir + Raise  → RaiseGCD
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

Besonderheit: zwei Kurzschlüsse statt einem, weil WHM mit Thin Air eine eigene
Rez-Vorbedingung hat. Die Level-Kette ist mit sechs Gliedern die längste im
gesamten Repo.

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
Barriere im Pull läuft jetzt über die gemeinsame Heilerregel `TryPullUpkeepOnTank` als Eukrasia-Ziel in Stufe 1
(`PullBarrierDue`, Option `UsePreEukrasianDiagnosis`, ab Werk aus; Konzept 15, V1).

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
Shieldsplitter, Axeblade Bite und Smash Axe. Begrenzter Job, außerhalb seines Profils — erfasst, nicht bearbeitet.

---

## Quantitative Auffälligkeiten

| Kennzahl | Wert | Bedeutung |
|---|---|---|
| Rotationsdateien mit `GeneralGCD`-Override | 24 / 24 | einziger wirklich universeller Hook |
| Rotationsdateien mit `AttackAbility`-Override | 24 / 24 | zweiter universeller Hook |
| Jobs ohne `CountDownAction` | 2 (DRG, VPR) | offen, TODO „Vorlauf und Notfallslot“ |
| Jobs ohne `EmergencyAbility` in der Reborn-Datei | 3 (RPR, SAM, WAR) | keine Lücke: WAR in der Basisschicht, RPR/SAM ohne eigene Notfallaktion (Konzept 04) |
| Echte Aufstiegsketten | 65 in 16 Dateien, davon 25 sauber gleichförmig (Zählung vom 20.08.2026) | siehe 04, Punkt A2 |
| Redundantes `X.EnoughLevel && X.CanUse` | 56, davon 44 außerhalb `ExtraRotations` | `CanUse` prüft das bereits selbst |
| Swiftcast/Raise-Kurzschluss | 13 Verwendungen von `SwiftRaisePending`, vier gleichlautende private Definitionen (eine je Heiler) | siehe 04, B1 |
| Spannweite Dateigröße | 199 (BLM_RP) – 1433 (SMN) LOC, ohne Duty-Rotationen | Faktor 7 |
| Spannweite Configs | 3 – 26 | Faktor 9 |
