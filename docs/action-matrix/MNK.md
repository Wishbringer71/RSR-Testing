# MNK — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `MonkRotation`
- Rotation: `RotationSolver/RebornRotations/Melee/MNK_Reborn.cs`
- Matrix als Tabelle: `MNK.csv`

## Nutzung

direkt: 45 · nur gelesen: 1 · ungenutzt: 1 · über anderen Knopf: 1

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Nahkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Nahkämpfer | Bloodbath (`BloodbathPvE`) | 7542 | Ability | direkt |
| Nahkämpfer | Feint (`FeintPvE`) | 7549 | Ability | direkt |
| Nahkämpfer | Leg Sweep (`LegSweepPvE`) | 7863 | Ability | direkt |
| Nahkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Nahkämpfer | True North (`TrueNorthPvE`) | 7546 | Ability | direkt |
| Job | Arm of the Destroyer (`ArmOfTheDestroyerPvE`) | 62 | Weaponskill | direkt |
| Job | Bootshine (`BootshinePvE`) | 53 | Weaponskill | direkt |
| Job | Brotherhood (`BrotherhoodPvE`) | 7396 | Ability | direkt |
| Job | Celestial Revolution (`CelestialRevolutionPvE`) | 25765 | Weaponskill | direkt |
| Job | Demolish (`DemolishPvE`) | 66 | Weaponskill | direkt |
| Job | Dragon Kick (`DragonKickPvE`) | 74 | Weaponskill | direkt |
| Job | Earth's Reply (`EarthsReplyPvE`) | 36944 | Ability | direkt |
| Job | Elixir Burst (`ElixirBurstPvE`) | 36948 | Weaponskill | direkt |
| Job | Elixir Field (`ElixirFieldPvE`) | 3545 | Weaponskill | direkt |
| Job | Enlightened Meditation (`EnlightenedMeditationPvE`) | 36943 | Ability | direkt |
| Job | Enlightenment (`EnlightenmentPvE`) | 16474 | Ability | direkt |
| Job | Fire's Reply (`FiresReplyPvE`) | 36950 | Weaponskill | direkt |
| Job | Flint Strike (`FlintStrikePvE`) | 25882 | Weaponskill | direkt |
| Job | Forbidden Meditation (`ForbiddenMeditationPvE`) | 36942 | Ability | direkt |
| Job | Form Shift (`FormShiftPvE`) | 4262 | Weaponskill | direkt |
| Job | Four-point Fury (`FourpointFuryPvE`) | 16473 | Weaponskill | direkt |
| Job | Howling Fist (`HowlingFistPvE`) | 25763 | Ability | direkt |
| Job | Inspirited Meditation (`InspiritedMeditationPvE`) | 36941 | Ability | direkt |
| Job | Leaping Opo (`LeapingOpoPvE`) | 36945 | Weaponskill | direkt |
| Job | Mantra (`MantraPvE`) | 65 | Ability | direkt |
| Job | Masterful Blitz (`MasterfulBlitzPvE`) | 25764 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Perfect Balance (`PerfectBalancePvE`) | 69 | Ability | direkt |
| Job | Phantom Rush (`PhantomRushPvE`) | 25769 | Weaponskill | direkt |
| Job | Pouncing Coeurl (`PouncingCoeurlPvE`) | 36947 | Weaponskill | direkt |
| Job | Riddle of Earth (`RiddleOfEarthPvE`) | 7394 | Ability | direkt |
| Job | Riddle of Fire (`RiddleOfFirePvE`) | 7395 | Ability | direkt |
| Job | Riddle of Wind (`RiddleOfWindPvE`) | 25766 | Ability | direkt |
| Job | Rising Phoenix (`RisingPhoenixPvE`) | 25768 | Weaponskill | direkt |
| Job | Rising Raptor (`RisingRaptorPvE`) | 36946 | Weaponskill | direkt |
| Job | Rockbreaker (`RockbreakerPvE`) | 70 | Weaponskill | direkt |
| Job | Shadow of the Destroyer (`ShadowOfTheDestroyerPvE`) | 25767 | Weaponskill | über Arm of the Destroyer |
| Job | Six-sided Star (`SixsidedStarPvE`) | 16476 | Weaponskill | ungenutzt |
| Job | Snap Punch (`SnapPunchPvE`) | 56 | Weaponskill | direkt |
| Job | Steel Peak (`SteelPeakPvE`) | 25761 | Ability | direkt |
| Job | Steeled Meditation (`SteeledMeditationPvE`) | 36940 | Ability | direkt |
| Job | Thunderclap (`ThunderclapPvE`) | 25762 | Ability | direkt |
| Job | Tornado Kick (`TornadoKickPvE`) | 3543 | Weaponskill | direkt |
| Job | True Strike (`TrueStrikePvE`) | 54 | Weaponskill | direkt |
| Job | Twin Snakes (`TwinSnakesPvE`) | 61 | Weaponskill | direkt |
| Job | Wind's Reply (`WindsReplyPvE`) | 36949 | Weaponskill | direkt |
| Job | the Forbidden Chakra (`TheForbiddenChakraPvE`) | 3547 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Arm of the Destroyer | Ausbau (Arm of the Destroyer Mastery) | Shadow of the Destroyer |
| Bootshine | Ausbau (Beast Chakra Mastery) | Leaping Opo |
| Celestial Revolution | braucht (Erzeuger nicht im Text) | three Beast Chakra |
| Demolish | braucht (Erzeuger nicht im Text) | coeurl form |
| Earth's Reply | braucht (Erzeuger nicht im Text) | Earth's Rumination |
| Elixir Burst | braucht (Erzeuger nicht im Text) | three of the same Beast Chakra |
| Elixir Field | Ausbau (Beast Chakra Mastery) | Elixir Burst |
| Elixir Field | braucht (Erzeuger nicht im Text) | three of the same Beast Chakra |
| Enlightened Meditation | braucht (Erzeuger nicht im Text) | less than five chakra are open |
| Enlightenment | braucht (Erzeuger nicht im Text) | combat |
| Enlightenment | braucht (Erzeuger nicht im Text) | under the effect of five Chakra |
| Fire's Reply | braucht Fire's Rumination | Eigenschaft Enhanced Riddle of Fire |
| Flint Strike | Ausbau (Flint Strike Mastery) | Rising Phoenix |
| Flint Strike | braucht (Erzeuger nicht im Text) | three distinct Beast Chakra |
| Forbidden Meditation | braucht (Erzeuger nicht im Text) | less than five chakra are open |
| Four-point Fury | braucht (Erzeuger nicht im Text) | raptor form |
| Howling Fist | Ausbau (Howling Fist Mastery) | Enlightenment |
| Howling Fist | braucht (Erzeuger nicht im Text) | combat |
| Howling Fist | braucht (Erzeuger nicht im Text) | under the effect of five Chakra |
| Inspirited Meditation | Ausbau (Howling Fist Mastery) | Enlightened Meditation |
| Inspirited Meditation | braucht (Erzeuger nicht im Text) | less than five chakra are open |
| Masterful Blitz | Knopf wird zu | Celestial Revolution |
| Masterful Blitz | Knopf wird zu | Elixir Burst |
| Masterful Blitz | Knopf wird zu | Elixir Field |
| Masterful Blitz | Knopf wird zu | Flint Strike |
| Masterful Blitz | Knopf wird zu | Phantom Rush |
| Masterful Blitz | Knopf wird zu | Rising Phoenix |
| Masterful Blitz | Knopf wird zu | Tornado Kick |
| Phantom Rush | braucht (Erzeuger nicht im Text) | Lunar Nadi |
| Phantom Rush | braucht (Erzeuger nicht im Text) | Solar Nadi as well as three Beast Chakra |
| Pouncing Coeurl | braucht (Erzeuger nicht im Text) | coeurl form |
| Rising Phoenix | braucht (Erzeuger nicht im Text) | three distinct Beast Chakra |
| Rising Raptor | braucht (Erzeuger nicht im Text) | raptor form |
| Rockbreaker | braucht (Erzeuger nicht im Text) | coeurl form |
| Snap Punch | Ausbau (Beast Chakra Mastery) | Pouncing Coeurl |
| Snap Punch | braucht (Erzeuger nicht im Text) | coeurl form |
| Steel Peak | Ausbau (Steel Peak Mastery) | the Forbidden Chakra |
| Steel Peak | braucht (Erzeuger nicht im Text) | combat |
| Steel Peak | braucht (Erzeuger nicht im Text) | under the effect of five Chakra |
| Steeled Meditation | Ausbau (Steel Peak Mastery) | Forbidden Meditation |
| Steeled Meditation | braucht (Erzeuger nicht im Text) | less than five chakra are open |
| the Forbidden Chakra | braucht (Erzeuger nicht im Text) | combat |
| the Forbidden Chakra | braucht (Erzeuger nicht im Text) | under the effect of five Chakra |
| Tornado Kick | braucht (Erzeuger nicht im Text) | Lunar Nadi |
| Tornado Kick | Ausbau (Tornado Kick Mastery) | Phantom Rush |
| Tornado Kick | braucht (Erzeuger nicht im Text) | Solar Nadi as well as three Beast Chakra |
| True Strike | Ausbau (Beast Chakra Mastery) | Rising Raptor |
| True Strike | braucht (Erzeuger nicht im Text) | raptor form |
| Twin Snakes | braucht (Erzeuger nicht im Text) | raptor form |
| Wind's Reply | braucht Wind's Rumination | Eigenschaft Enhanced Riddle of Wind |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Brotherhood | Regel prüft | Riddle of Fire |
| Celestial Revolution | Regel prüft | Riddle of Fire |
| Earth's Reply | StatusNeed EarthsRumination | Riddle of Earth |
| Elixir Burst | Regel prüft | Riddle of Fire |
| Elixir Field | Regel prüft | Riddle of Fire |
| Enlightened Meditation | Regel sperrt vorher | Perfect Balance |
| Enlightened Meditation | Regel sperrt vorher | Thunderclap |
| Fire's Reply | Regel prüft | Bootshine |
| Fire's Reply | Regel prüft | Dragon Kick |
| Fire's Reply | Regel prüft | Leaping Opo |
| Fire's Reply | StatusNeed FiresRumination | Riddle of Fire |
| Flint Strike | Regel prüft | Riddle of Fire |
| Forbidden Meditation | Regel sperrt vorher | Perfect Balance |
| Forbidden Meditation | Regel sperrt vorher | Thunderclap |
| Form Shift | Regel sperrt vorher | Thunderclap |
| Inspirited Meditation | Regel sperrt vorher | Perfect Balance |
| Inspirited Meditation | Regel sperrt vorher | Thunderclap |
| Perfect Balance | Regel prüft | Bootshine |
| Perfect Balance | Regel prüft | Brotherhood |
| Perfect Balance | Regel prüft | Dragon Kick |
| Perfect Balance | Regel prüft | Leaping Opo |
| Perfect Balance | Regel prüft | Riddle of Fire |
| Phantom Rush | Regel prüft | Riddle of Fire |
| Riddle of Fire | Regel prüft | Brotherhood |
| Riddle of Fire | Regel prüft | Perfect Balance |
| Rising Phoenix | Regel prüft | Riddle of Fire |
| Steel Peak | Regel prüft | Brotherhood |
| Steel Peak | Regel prüft | the Forbidden Chakra |
| Steeled Meditation | Regel prüft | Forbidden Meditation |
| Steeled Meditation | Regel sperrt vorher | Perfect Balance |
| Steeled Meditation | Regel sperrt vorher | Thunderclap |
| the Forbidden Chakra | Regel prüft | Brotherhood |
| Tornado Kick | Regel prüft | Riddle of Fire |
| True North | Regel sperrt vorher | Thunderclap |
| Wind's Reply | StatusNeed WindsRumination | Riddle of Wind |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BladedancePvE`, `BraverPvE`, `FinalHeavenPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Masterful Blitz (`MasterfulBlitzPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Six-sided Star (`SixsidedStarPvE`, Weaponskill): ungenutzt
