# VPR — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `ViperRotation`
- Rotation: `RotationSolver/RebornRotations/Melee/VPR_Reborn.cs`
- Matrix als Tabelle: `VPR.csv`

## Nutzung

direkt: 49 · ungenutzt: 3

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Nahkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Nahkämpfer | Bloodbath (`BloodbathPvE`) | 7542 | Ability | direkt |
| Nahkämpfer | Feint (`FeintPvE`) | 7549 | Ability | direkt |
| Nahkämpfer | Leg Sweep (`LegSweepPvE`) | 7863 | Ability | direkt |
| Nahkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Nahkämpfer | True North (`TrueNorthPvE`) | 7546 | Ability | direkt |
| Job | Bloodied Maw (`BloodiedMawPvE`) | 34619 | Weaponskill | direkt |
| Job | Death Rattle (`DeathRattlePvE`) | 34634 | Ability | direkt |
| Job | First Generation (`FirstGenerationPvE`) | 34627 | Weaponskill | direkt |
| Job | First Legacy (`FirstLegacyPvE`) | 34640 | Ability | direkt |
| Job | Flanksbane Fang (`FlanksbaneFangPvE`) | 34611 | Weaponskill | direkt |
| Job | Flanksting Strike (`FlankstingStrikePvE`) | 34610 | Weaponskill | direkt |
| Job | Fourth Generation (`FourthGenerationPvE`) | 34630 | Weaponskill | direkt |
| Job | Fourth Legacy (`FourthLegacyPvE`) | 34643 | Ability | direkt |
| Job | Hindsbane Fang (`HindsbaneFangPvE`) | 34613 | Weaponskill | direkt |
| Job | Hindsting Strike (`HindstingStrikePvE`) | 34612 | Weaponskill | direkt |
| Job | Hunter's Bite (`HuntersBitePvE`) | 34616 | Weaponskill | direkt |
| Job | Hunter's Coil (`HuntersCoilPvE`) | 34621 | Weaponskill | direkt |
| Job | Hunter's Den (`HuntersDenPvE`) | 34624 | Weaponskill | direkt |
| Job | Hunter's Sting (`HuntersStingPvE`) | 34608 | Weaponskill | direkt |
| Job | Jagged Maw (`JaggedMawPvE`) | 34618 | Weaponskill | direkt |
| Job | Last Lash (`LastLashPvE`) | 34635 | Ability | direkt |
| Job | Ouroboros (`OuroborosPvE`) | 34631 | Weaponskill | direkt |
| Job | Reaving Fangs (`ReavingFangsPvE`) | 34607 | Weaponskill | direkt |
| Job | Reaving Maw (`ReavingMawPvE`) | 34615 | Weaponskill | direkt |
| Job | Reawaken (`ReawakenPvE`) | 34626 | Weaponskill | direkt |
| Job | Second Generation (`SecondGenerationPvE`) | 34628 | Weaponskill | direkt |
| Job | Second Legacy (`SecondLegacyPvE`) | 34641 | Ability | direkt |
| Job | Serpent's Ire (`SerpentsIrePvE`) | 34647 | Ability | direkt |
| Job | Serpent's Tail (`SerpentsTailPvE`) | 35920 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Slither (`SlitherPvE`) | 34646 | Ability | direkt |
| Job | Steel Fangs (`SteelFangsPvE`) | 34606 | Weaponskill | direkt |
| Job | Steel Maw (`SteelMawPvE`) | 34614 | Weaponskill | direkt |
| Job | Swiftskin's Bite (`SwiftskinsBitePvE`) | 34617 | Weaponskill | direkt |
| Job | Swiftskin's Coil (`SwiftskinsCoilPvE`) | 34622 | Weaponskill | direkt |
| Job | Swiftskin's Den (`SwiftskinsDenPvE`) | 34625 | Weaponskill | direkt |
| Job | Swiftskin's Sting (`SwiftskinsStingPvE`) | 34609 | Weaponskill | direkt |
| Job | Third Generation (`ThirdGenerationPvE`) | 34629 | Weaponskill | direkt |
| Job | Third Legacy (`ThirdLegacyPvE`) | 34642 | Ability | direkt |
| Job | Twinblood (`TwinbloodPvE`) | 35922 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Twinblood Bite (`TwinbloodBitePvE`) | 34637 | Ability | direkt |
| Job | Twinblood Thresh (`TwinbloodThreshPvE`) | 34639 | Ability | direkt |
| Job | Twinfang (`TwinfangPvE`) | 35921 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Twinfang Bite (`TwinfangBitePvE`) | 34636 | Ability | direkt |
| Job | Twinfang Thresh (`TwinfangThreshPvE`) | 34638 | Ability | direkt |
| Job | Uncoiled Fury (`UncoiledFuryPvE`) | 34633 | Weaponskill | direkt |
| Job | Uncoiled Twinblood (`UncoiledTwinbloodPvE`) | 34645 | Ability | direkt |
| Job | Uncoiled Twinfang (`UncoiledTwinfangPvE`) | 34644 | Ability | direkt |
| Job | Vicepit (`VicepitPvE`) | 34623 | Weaponskill | direkt |
| Job | Vicewinder (`VicewinderPvE`) | 34620 | Weaponskill | direkt |
| Job | Writhing Snap (`WrithingSnapPvE`) | 34632 | Weaponskill | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| First Generation | kostet | Anguine Tribute |
| Fourth Generation | kostet | Anguine Tribute |
| Hunter's Bite | Knopf wird zu | Jagged Maw |
| Hunter's Coil | Knopf wird zu | Third Generation |
| Hunter's Den | Knopf wird zu | Third Generation |
| Hunter's Sting | Knopf wird zu | Flanksting Strike |
| Hunter's Sting | Knopf wird zu | Hindsting Strike |
| Reaving Fangs | Knopf wird zu | Second Generation |
| Reaving Fangs | Knopf wird zu | Swiftskin's Sting |
| Reaving Maw | Knopf wird zu | Second Generation |
| Reaving Maw | Knopf wird zu | Swiftskin's Bite |
| Reawaken | Knopf wird zu | Ouroboros |
| Reawaken | kostet | Serpent Offerings |
| Second Generation | kostet | Anguine Tribute |
| Serpent's Ire | braucht (Erzeuger nicht im Text) | combat |
| Serpent's Tail | Knopf wird zu | Death Rattle |
| Serpent's Tail | Knopf wird zu | First Legacy |
| Serpent's Tail | Knopf wird zu | Fourth Legacy |
| Serpent's Tail | Knopf wird zu | Last Lash |
| Serpent's Tail | Knopf wird zu | Second Legacy |
| Serpent's Tail | Knopf wird zu | Third Legacy |
| Steel Fangs | Knopf wird zu | First Generation |
| Steel Fangs | Knopf wird zu | Hunter's Sting |
| Steel Maw | Knopf wird zu | First Generation |
| Steel Maw | Knopf wird zu | Hunter's Bite |
| Swiftskin's Bite | Knopf wird zu | Bloodied Maw |
| Swiftskin's Coil | Knopf wird zu | Fourth Generation |
| Swiftskin's Den | Knopf wird zu | Fourth Generation |
| Swiftskin's Sting | Knopf wird zu | Flanksbane Fang |
| Swiftskin's Sting | Knopf wird zu | Hindsbane Fang |
| Third Generation | kostet | Anguine Tribute |
| Twinblood | Knopf wird zu | Twinblood Bite |
| Twinblood | Knopf wird zu | Twinblood Thresh |
| Twinblood | Knopf wird zu | Uncoiled Twinblood |
| Twinfang | Knopf wird zu | Twinfang Bite |
| Twinfang | Knopf wird zu | Twinfang Thresh |
| Twinfang | Knopf wird zu | Uncoiled Twinfang |
| Uncoiled Fury | kostet | Rattling Coil |
| Vicepit | gemeinsame Abklingzeit | Vicewinder |
| Vicewinder | gemeinsame Abklingzeit | Vicepit |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Feint | Regel prüft | Serpent's Ire |
| First Generation | StatusNeed Reawakened | Status Reawakened |
| First Legacy | StatusNeed Reawakened | Status Reawakened |
| Flanksbane Fang | Regel sperrt vorher | Hunter's Coil |
| Flanksbane Fang | Regel sperrt vorher | Swiftskin's Coil |
| Flanksting Strike | Regel sperrt vorher | Hunter's Coil |
| Flanksting Strike | Regel sperrt vorher | Swiftskin's Coil |
| Fourth Generation | StatusNeed Reawakened | Status Reawakened |
| Fourth Legacy | StatusNeed Reawakened | Status Reawakened |
| Hindsbane Fang | Regel sperrt vorher | Hunter's Coil |
| Hindsbane Fang | Regel sperrt vorher | Swiftskin's Coil |
| Hindsting Strike | Regel sperrt vorher | Hunter's Coil |
| Hindsting Strike | Regel sperrt vorher | Swiftskin's Coil |
| Hunter's Bite | Regel prüft | Swiftskin's Bite |
| Hunter's Sting | Regel sperrt vorher | Hunter's Coil |
| Hunter's Sting | Regel sperrt vorher | Swiftskin's Coil |
| Hunter's Sting | Regel prüft | Swiftskin's Sting |
| Ouroboros | StatusNeed Reawakened | Status Reawakened |
| Reaving Fangs | Regel prüft | Bloodied Maw |
| Reaving Fangs | Regel prüft | Flanksbane Fang |
| Reaving Fangs | Regel prüft | Flanksting Strike |
| Reaving Fangs | Regel prüft | Hindsbane Fang |
| Reaving Fangs | Regel prüft | Hindsting Strike |
| Reaving Fangs | Regel sperrt vorher | Hunter's Coil |
| Reaving Fangs | Regel prüft | Jagged Maw |
| Reaving Fangs | Regel sperrt vorher | Swiftskin's Coil |
| Reaving Maw | Regel prüft | Bloodied Maw |
| Reaving Maw | Regel prüft | Flanksbane Fang |
| Reaving Maw | Regel prüft | Flanksting Strike |
| Reaving Maw | Regel prüft | Hindsbane Fang |
| Reaving Maw | Regel prüft | Hindsting Strike |
| Reaving Maw | Regel prüft | Jagged Maw |
| Reawaken | Regel prüft | Serpent's Ire |
| Second Generation | StatusNeed Reawakened | Status Reawakened |
| Second Legacy | StatusNeed Reawakened | Status Reawakened |
| Serpent's Ire | Regel prüft | Feint |
| Serpent's Ire | Regel prüft | Reawaken |
| Steel Fangs | Regel prüft | Bloodied Maw |
| Steel Fangs | Regel prüft | Flanksbane Fang |
| Steel Fangs | Regel prüft | Flanksting Strike |
| Steel Fangs | Regel prüft | Hindsbane Fang |
| Steel Fangs | Regel prüft | Hindsting Strike |
| Steel Fangs | Regel sperrt vorher | Hunter's Coil |
| Steel Fangs | Regel prüft | Jagged Maw |
| Steel Fangs | Regel sperrt vorher | Swiftskin's Coil |
| Steel Maw | Regel prüft | Bloodied Maw |
| Steel Maw | Regel prüft | Flanksbane Fang |
| Steel Maw | Regel prüft | Flanksting Strike |
| Steel Maw | Regel prüft | Hindsbane Fang |
| Steel Maw | Regel prüft | Hindsting Strike |
| Steel Maw | Regel prüft | Jagged Maw |
| Swiftskin's Sting | Regel sperrt vorher | Hunter's Coil |
| Swiftskin's Sting | Regel sperrt vorher | Swiftskin's Coil |
| Third Generation | StatusNeed Reawakened | Status Reawakened |
| Third Legacy | StatusNeed Reawakened | Status Reawakened |
| Uncoiled Fury | Regel sperrt vorher | Hunter's Coil |
| Uncoiled Fury | Regel prüft | Serpent's Ire |
| Uncoiled Fury | Regel sperrt vorher | Swiftskin's Coil |
| Vicewinder | Regel sperrt vorher | Hunter's Coil |
| Vicewinder | Regel sperrt vorher | Swiftskin's Coil |
| Writhing Snap | Regel sperrt vorher | Hunter's Coil |
| Writhing Snap | Regel sperrt vorher | Swiftskin's Coil |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BladedancePvE`, `BraverPvE`, `WorldswallowerPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Serpent's Tail (`SerpentsTailPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Twinblood (`TwinbloodPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Twinfang (`TwinfangPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
