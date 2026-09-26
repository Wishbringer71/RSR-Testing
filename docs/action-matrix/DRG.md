# DRG — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `DragoonRotation`
- Rotation: `RotationSolver/RebornRotations/Melee/DRG_Reborn.cs`
- Matrix als Tabelle: `DRG.csv`

## Nutzung

direkt: 40

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Nahkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Nahkämpfer | Bloodbath (`BloodbathPvE`) | 7542 | Ability | direkt |
| Nahkämpfer | Feint (`FeintPvE`) | 7549 | Ability | direkt |
| Nahkämpfer | Leg Sweep (`LegSweepPvE`) | 7863 | Ability | direkt |
| Nahkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Nahkämpfer | True North (`TrueNorthPvE`) | 7546 | Ability | direkt |
| Job | Battle Litany (`BattleLitanyPvE`) | 3557 | Ability | direkt |
| Job | Chaos Thrust (`ChaosThrustPvE`) | 88 | Weaponskill | direkt |
| Job | Chaotic Spring (`ChaoticSpringPvE`) | 25772 | Weaponskill | direkt |
| Job | Coerthan Torment (`CoerthanTormentPvE`) | 16477 | Weaponskill | direkt |
| Job | Disembowel (`DisembowelPvE`) | 87 | Weaponskill | direkt |
| Job | Doom Spike (`DoomSpikePvE`) | 86 | Weaponskill | direkt |
| Job | Draconian Fury (`DraconianFuryPvE`) | 25770 | Weaponskill | direkt |
| Job | Dragonfire Dive (`DragonfireDivePvE`) | 96 | Ability | direkt |
| Job | Drakesbane (`DrakesbanePvE`) | 36952 | Weaponskill | direkt |
| Job | Elusive Jump (`ElusiveJumpPvE`) | 94 | Ability | direkt |
| Job | Fang and Claw (`FangAndClawPvE`) | 3554 | Weaponskill | direkt |
| Job | Full Thrust (`FullThrustPvE`) | 84 | Weaponskill | direkt |
| Job | Geirskogul (`GeirskogulPvE`) | 3555 | Ability | direkt |
| Job | Heavens' Thrust (`HeavensThrustPvE`) | 25771 | Weaponskill | direkt |
| Job | High Jump (`HighJumpPvE`) | 16478 | Ability | direkt |
| Job | Jump (`JumpPvE`) | 92 | Ability | direkt |
| Job | Lance Barrage (`LanceBarragePvE`) | 36954 | Weaponskill | direkt |
| Job | Lance Charge (`LanceChargePvE`) | 85 | Ability | direkt |
| Job | Life Surge (`LifeSurgePvE`) | 83 | Ability | direkt |
| Job | Mirage Dive (`MirageDivePvE`) | 7399 | Ability | direkt |
| Job | Nastrond (`NastrondPvE`) | 7400 | Ability | direkt |
| Job | Piercing Talon (`PiercingTalonPvE`) | 90 | Weaponskill | direkt |
| Job | Raiden Thrust (`RaidenThrustPvE`) | 16479 | Weaponskill | direkt |
| Job | Rise of the Dragon (`RiseOfTheDragonPvE`) | 36953 | Ability | direkt |
| Job | Sonic Thrust (`SonicThrustPvE`) | 7397 | Weaponskill | direkt |
| Job | Spiral Blow (`SpiralBlowPvE`) | 36955 | Weaponskill | direkt |
| Job | Starcross (`StarcrossPvE`) | 36956 | Ability | direkt |
| Job | Stardiver (`StardiverPvE`) | 16480 | Ability | direkt |
| Job | True Thrust (`TrueThrustPvE`) | 75 | Weaponskill | direkt |
| Job | Vorpal Thrust (`VorpalThrustPvE`) | 78 | Weaponskill | direkt |
| Job | Wheeling Thrust (`WheelingThrustPvE`) | 3556 | Weaponskill | direkt |
| Job | Winged Glide (`WingedGlidePvE`) | 36951 | Ability | direkt |
| Job | Wyrmwind Thrust (`WyrmwindThrustPvE`) | 25773 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Chaos Thrust | Ausbau (Lance Mastery II) | Chaotic Spring |
| Chaos Thrust | Combo nach | Disembowel |
| Coerthan Torment | Combo nach | Sonic Thrust |
| Disembowel | Ausbau (Lance Mastery IV) | Spiral Blow |
| Doom Spike | Ausbau (Enhanced Coerthan Torment) | Draconian Fury |
| Doom Spike | Knopf wird zu | Draconian Fury |
| Draconian Fury | braucht Draconian Fire | Eigenschaft Enhanced Coerthan Torment |
| Draconian Fury | braucht Draconian Fire | Eigenschaft Lance Mastery |
| Drakesbane | Combo nach | Fang and Claw |
| Drakesbane | Combo nach | Wheeling Thrust |
| Fang and Claw | Knopf wird zu | Drakesbane |
| Full Thrust | Ausbau (Lance Mastery II) | Heavens' Thrust |
| Full Thrust | Combo nach | Vorpal Thrust |
| Jump | Ausbau (Jump Mastery) | High Jump |
| Lance Barrage | Combo nach | Raiden Thrust |
| Lance Barrage | Combo nach | True Thrust |
| Mirage Dive | braucht Dive Ready | High Jump |
| Nastrond | braucht Nastrond Ready | Eigenschaft Life of the Dragon |
| Raiden Thrust | braucht Draconian Fire | Eigenschaft Enhanced Coerthan Torment |
| Raiden Thrust | braucht Draconian Fire | Eigenschaft Lance Mastery |
| Rise of the Dragon | braucht Dragon's Flight | Eigenschaft Enhanced Dragonfire Dive |
| Spiral Blow | Combo nach | Raiden Thrust |
| Spiral Blow | Combo nach | True Thrust |
| Starcross | braucht Starcross Ready | Eigenschaft Enhanced Stardiver |
| Stardiver | braucht Life of the Dragon (Erzeuger nicht im Text) | Life of the Dragon |
| True Thrust | Ausbau (Lance Mastery) | Raiden Thrust |
| True Thrust | Knopf wird zu | Raiden Thrust |
| Vorpal Thrust | Ausbau (Lance Mastery IV) | Lance Barrage |
| Wheeling Thrust | Knopf wird zu | Drakesbane |
| Wyrmwind Thrust | kostet | Focus |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Battle Litany | Regel sperrt vorher | Disembowel |
| Battle Litany | Regel sperrt vorher | Stardiver |
| Bloodbath | Regel sperrt vorher | Stardiver |
| Chaos Thrust | ComboIds | Disembowel |
| Chaos Thrust | ComboIds | Spiral Blow |
| Chaotic Spring | ComboIds | Spiral Blow |
| Coerthan Torment | ComboIds | Sonic Thrust |
| Disembowel | ComboIds | Raiden Thrust |
| Disembowel | ComboIds | True Thrust |
| Dragonfire Dive | Regel prüft | Battle Litany |
| Dragonfire Dive | Regel sperrt vorher | Disembowel |
| Dragonfire Dive | Regel sperrt vorher | Stardiver |
| Drakesbane | ComboIds | Fang and Claw |
| Drakesbane | ComboIds | Wheeling Thrust |
| Elusive Jump | Regel sperrt vorher | Stardiver |
| Fang and Claw | ComboIds | Heavens' Thrust |
| Feint | Regel sperrt vorher | Stardiver |
| Full Thrust | ComboIds | Lance Barrage |
| Full Thrust | ComboIds | Vorpal Thrust |
| Geirskogul | Regel sperrt vorher | Disembowel |
| Geirskogul | Regel sperrt vorher | Stardiver |
| Heavens' Thrust | ComboIds | Lance Barrage |
| Heavens' Thrust | ComboIds | Vorpal Thrust |
| High Jump | Regel sperrt vorher | Disembowel |
| High Jump | Regel sperrt vorher | Dragonfire Dive |
| High Jump | Regel prüft | Geirskogul |
| High Jump | Regel sperrt vorher | Stardiver |
| Jump | Regel sperrt vorher | Disembowel |
| Jump | Regel sperrt vorher | Dragonfire Dive |
| Jump | Regel prüft | High Jump |
| Jump | Regel prüft | Lance Charge |
| Jump | Regel sperrt vorher | Stardiver |
| Lance Barrage | ComboIds | Raiden Thrust |
| Lance Barrage | ComboIds | True Thrust |
| Lance Charge | Regel prüft | Battle Litany |
| Lance Charge | Regel sperrt vorher | Disembowel |
| Lance Charge | Regel sperrt vorher | Stardiver |
| Life Surge | Regel sperrt vorher | Disembowel |
| Life Surge | Regel sperrt vorher | Stardiver |
| Mirage Dive | Regel sperrt vorher | Disembowel |
| Mirage Dive | Regel sperrt vorher | Dragonfire Dive |
| Mirage Dive | StatusNeed DiveReady | High Jump |
| Mirage Dive | StatusNeed DiveReady | Jump |
| Mirage Dive | Regel sperrt vorher | Stardiver |
| Nastrond | Regel sperrt vorher | Disembowel |
| Nastrond | Regel sperrt vorher | Dragonfire Dive |
| Nastrond | StatusNeed NastrondReady | Geirskogul |
| Nastrond | Regel sperrt vorher | Stardiver |
| Piercing Talon | Regel prüft | Winged Glide |
| Rise of the Dragon | Regel sperrt vorher | Disembowel |
| Rise of the Dragon | Regel sperrt vorher | Dragonfire Dive |
| Rise of the Dragon | StatusNeed DragonsFlight | Dragonfire Dive |
| Rise of the Dragon | Regel sperrt vorher | Stardiver |
| Second Wind | Regel sperrt vorher | Stardiver |
| Sonic Thrust | ComboIds | Doom Spike |
| Sonic Thrust | ComboIds | Draconian Fury |
| Spiral Blow | ComboIds | Raiden Thrust |
| Spiral Blow | ComboIds | True Thrust |
| Starcross | Regel sperrt vorher | Disembowel |
| Starcross | Regel sperrt vorher | Dragonfire Dive |
| Starcross | Regel sperrt vorher | Stardiver |
| Starcross | StatusNeed StarcrossReady | Stardiver |
| Vorpal Thrust | ComboIds | Raiden Thrust |
| Vorpal Thrust | ComboIds | True Thrust |
| Wheeling Thrust | ComboIds | Chaotic Spring |
| Winged Glide | Regel sperrt vorher | Stardiver |
| Wyrmwind Thrust | Regel sperrt vorher | Disembowel |
| Wyrmwind Thrust | Regel prüft | Draconian Fury |
| Wyrmwind Thrust | Regel sperrt vorher | Dragonfire Dive |
| Wyrmwind Thrust | Regel prüft | Raiden Thrust |
| Wyrmwind Thrust | Regel sperrt vorher | Stardiver |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

keine

### Verlängerung, Aufbau, Umschalten

keine

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Focus | — (nicht im Wirktext) | Wyrmwind Thrust |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Chaotic Spring (3), Disembowel (2), Drakesbane (1), Fang and Claw (2), Feint (1), Geirskogul (1), Heavens' Thrust (1), Jump (1), Nastrond (1), Piercing Talon (1), Raiden Thrust (1), Stardiver (1), True Thrust (1), Vorpal Thrust (1), Wheeling Thrust (2), Wyrmwind Thrust (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BladedancePvE`, `BraverPvE`, `DragonsongDivePvE`

## Nicht direkt genutzt

keine
