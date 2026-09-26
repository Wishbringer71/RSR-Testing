# PCT — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `PictomancerRotation`
- Rotation: `RotationSolver/RebornRotations/Magical/PCT_Reborn.cs`
- Matrix als Tabelle: `PCT.csv`

## Nutzung

direkt: 42 · nur gelesen: 6 · ungenutzt: 1

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Magier | Addle (`AddlePvE`) | 7560 | Ability | direkt |
| Magier | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Magier | Sleep (`SleepPvE`) | 25880 | Spell | ungenutzt |
| Magier | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Magier | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Aero II in Green (`AeroIiInGreenPvE`) | 34657 | Spell | direkt |
| Job | Aero in Green (`AeroInGreenPvE`) | 34651 | Spell | direkt |
| Job | Blizzard II in Cyan (`BlizzardIiInCyanPvE`) | 34659 | Spell | direkt |
| Job | Blizzard in Cyan (`BlizzardInCyanPvE`) | 34653 | Spell | direkt |
| Job | Claw Motif (`ClawMotifPvE`) | 34666 | Spell | direkt |
| Job | Clawed Muse (`ClawedMusePvE`) | 34672 | Ability | direkt |
| Job | Comet in Black (`CometInBlackPvE`) | 34663 | Spell | direkt |
| Job | Creature Motif (`CreatureMotifPvE`) | 34689 | Spell | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Fanged Muse (`FangedMusePvE`) | 34673 | Ability | direkt |
| Job | Fire II in Red (`FireIiInRedPvE`) | 34656 | Spell | direkt |
| Job | Fire in Red (`FireInRedPvE`) | 34650 | Spell | direkt |
| Job | Hammer Brush (`HammerBrushPvE`) | 34679 | Spell | direkt |
| Job | Hammer Motif (`HammerMotifPvE`) | 34668 | Spell | direkt |
| Job | Hammer Stamp (`HammerStampPvE`) | 34678 | Spell | direkt |
| Job | Holy in White (`HolyInWhitePvE`) | 34662 | Spell | direkt |
| Job | Landscape Motif (`LandscapeMotifPvE`) | 34691 | Spell | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Living Muse (`LivingMusePvE`) | 35347 | Ability | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Maw Motif (`MawMotifPvE`) | 34667 | Spell | direkt |
| Job | Mog of the Ages (`MogOfTheAgesPvE`) | 34676 | Ability | direkt |
| Job | Polishing Hammer (`PolishingHammerPvE`) | 34680 | Spell | direkt |
| Job | Pom Motif (`PomMotifPvE`) | 34664 | Spell | direkt |
| Job | Pom Muse (`PomMusePvE`) | 34670 | Ability | direkt |
| Job | Rainbow Drip (`RainbowDripPvE`) | 34688 | Spell | direkt |
| Job | Retribution of the Madeen (`RetributionOfTheMadeenPvE`) | 34677 | Ability | direkt |
| Job | Scenic Muse (`ScenicMusePvE`) | 35349 | Ability | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Smudge (`SmudgePvE`) | 34684 | Ability | direkt |
| Job | Star Prism (`StarPrismPvE`) | 34681 | Spell | direkt |
| Job | Starry Muse (`StarryMusePvE`) | 34675 | Ability | direkt |
| Job | Starry Sky Motif (`StarrySkyMotifPvE`) | 34669 | Spell | direkt |
| Job | Steel Muse (`SteelMusePvE`) | 35348 | Ability | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Stone II in Yellow (`StoneIiInYellowPvE`) | 34660 | Spell | direkt |
| Job | Stone in Yellow (`StoneInYellowPvE`) | 34654 | Spell | direkt |
| Job | Striking Muse (`StrikingMusePvE`) | 34674 | Ability | direkt |
| Job | Subtractive Palette (`SubtractivePalettePvE`) | 34683 | Ability | direkt |
| Job | Tempera Coat (`TemperaCoatPvE`) | 34685 | Ability | direkt |
| Job | Tempera Grassa (`TemperaGrassaPvE`) | 34686 | Ability | direkt |
| Job | Thunder II in Magenta (`ThunderIiInMagentaPvE`) | 34661 | Spell | direkt |
| Job | Thunder in Magenta (`ThunderInMagentaPvE`) | 34655 | Spell | direkt |
| Job | Water II in Blue (`WaterIiInBluePvE`) | 34658 | Spell | direkt |
| Job | Water in Blue (`WaterInBluePvE`) | 34652 | Spell | direkt |
| Job | Weapon Motif (`WeaponMotifPvE`) | 34690 | Spell | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Wing Motif (`WingMotifPvE`) | 34665 | Spell | direkt |
| Job | Winged Muse (`WingedMusePvE`) | 34671 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Aero II in Green | braucht (Erzeuger nicht im Text) | Aetherhues |
| Aero II in Green | braucht not under the effect of Subtractive Palette | Subtractive Palette |
| Aero II in Green | Knopf wird zu | Water II in Blue |
| Aero in Green | braucht (Erzeuger nicht im Text) | Aetherhues |
| Aero in Green | braucht not under the effect of Subtractive Palette | Subtractive Palette |
| Aero in Green | Knopf wird zu | Water in Blue |
| Blizzard II in Cyan | Knopf wird zu | Stone II in Yellow |
| Blizzard II in Cyan | braucht Subtractive Palette | Subtractive Palette |
| Blizzard in Cyan | Knopf wird zu | Stone in Yellow |
| Blizzard in Cyan | braucht Subtractive Palette | Subtractive Palette |
| Clawed Muse | braucht (Erzeuger nicht im Text) | a claw is painted on the Creature Canvas |
| Comet in Black | braucht (Erzeuger nicht im Text) | Black Paint |
| Comet in Black | kostet | Black Paint |
| Comet in Black | braucht Monochrome Tones | Eigenschaft Enhanced Palette |
| Creature Motif | Knopf wird zu | Claw Motif |
| Creature Motif | Knopf wird zu | Maw Motif |
| Creature Motif | Knopf wird zu | Pom Motif |
| Creature Motif | Knopf wird zu | Wing Motif |
| Fanged Muse | braucht (Erzeuger nicht im Text) | fangs are painted on the Creature Canvas |
| Fire II in Red | Knopf wird zu | Aero II in Green |
| Fire in Red | Knopf wird zu | Aero in Green |
| Hammer Brush | braucht (Erzeuger nicht im Text) | Hammer Time |
| Hammer Brush | Combo nach | Hammer Stamp |
| Hammer Brush | Knopf wird zu | Polishing Hammer |
| Hammer Stamp | braucht (Erzeuger nicht im Text) | Hammer Time |
| Hammer Stamp | Knopf wird zu | Hammer Brush |
| Holy in White | braucht White Paint | Eigenschaft Enhanced Artistry |
| Holy in White | kostet | White Paint |
| Landscape Motif | Knopf wird zu | Starry Sky Motif |
| Living Muse | Knopf wird zu | Clawed Muse |
| Living Muse | braucht a Creature Motif is depicted on the Creature Canvas | Creature Motif |
| Living Muse | Knopf wird zu | Fanged Muse |
| Living Muse | Knopf wird zu | Pom Muse |
| Living Muse | Knopf wird zu | Winged Muse |
| Mog of the Ages | braucht (Erzeuger nicht im Text) | Moogle Portrait |
| Mog of the Ages | Knopf wird zu | Retribution of the Madeen |
| Polishing Hammer | braucht (Erzeuger nicht im Text) | Hammer Time |
| Polishing Hammer | Combo nach | Hammer Brush |
| Pom Muse | braucht (Erzeuger nicht im Text) | a pom is painted on the Creature Canvas |
| Retribution of the Madeen | braucht (Erzeuger nicht im Text) | Madeen Portrait |
| Scenic Muse | Knopf wird zu | Starry Muse |
| Star Prism | braucht Starstruck | Eigenschaft Enhanced Pictomancy V |
| Starry Muse | braucht (Erzeuger nicht im Text) | combat |
| Starry Muse | braucht (Erzeuger nicht im Text) | when a starry sky is painted on the Landscape Canvas |
| Steel Muse | Knopf wird zu | Striking Muse |
| Stone II in Yellow | braucht (Erzeuger nicht im Text) | Aetherhues |
| Stone II in Yellow | braucht Subtractive Palette | Subtractive Palette |
| Stone II in Yellow | Knopf wird zu | Thunder II in Magenta |
| Stone in Yellow | braucht (Erzeuger nicht im Text) | Aetherhues |
| Stone in Yellow | braucht Subtractive Palette | Subtractive Palette |
| Stone in Yellow | Knopf wird zu | Thunder in Magenta |
| Striking Muse | braucht (Erzeuger nicht im Text) | combat |
| Striking Muse | braucht (Erzeuger nicht im Text) | when a hammer is painted on the Weapon Canvas |
| Subtractive Palette | kostet | Palette |
| Tempera Grassa | braucht Tempera Coat | Tempera Coat |
| Thunder II in Magenta | braucht (Erzeuger nicht im Text) | Aetherhues II |
| Thunder II in Magenta | braucht Subtractive Palette | Subtractive Palette |
| Thunder in Magenta | braucht (Erzeuger nicht im Text) | Aetherhues II |
| Thunder in Magenta | braucht Subtractive Palette | Subtractive Palette |
| Water II in Blue | braucht (Erzeuger nicht im Text) | Aetherhues II |
| Water II in Blue | braucht not under the effect of Subtractive Palette | Subtractive Palette |
| Water in Blue | braucht (Erzeuger nicht im Text) | Aetherhues II |
| Water in Blue | braucht not under the effect of Subtractive Palette | Subtractive Palette |
| Weapon Motif | Knopf wird zu | Hammer Motif |
| Winged Muse | braucht (Erzeuger nicht im Text) | a pair of wings is painted on the Creature Canvas |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Claw Motif | Regel prüft | Creature Motif |
| Claw Motif | Regel prüft | Living Muse |
| Hammer Brush | ComboIds | Hammer Stamp |
| Hammer Motif | Regel prüft | Steel Muse |
| Hammer Motif | Regel prüft | Weapon Motif |
| Maw Motif | Regel prüft | Creature Motif |
| Maw Motif | Regel prüft | Living Muse |
| Polishing Hammer | ComboIds | Hammer Brush |
| Pom Motif | Regel prüft | Creature Motif |
| Pom Motif | Regel prüft | Living Muse |
| Star Prism | StatusNeed Starstruck | Starry Muse |
| Starry Sky Motif | Regel prüft | Scenic Muse |
| Striking Muse | Regel prüft | Rainbow Drip |
| Swiftcast | Regel prüft | Claw Motif |
| Swiftcast | Regel prüft | Hammer Motif |
| Swiftcast | Regel prüft | Maw Motif |
| Swiftcast | Regel prüft | Pom Motif |
| Swiftcast | Regel prüft | Rainbow Drip |
| Swiftcast | Regel prüft | Starry Sky Motif |
| Swiftcast | Regel prüft | Wing Motif |
| Tempera Grassa | StatusNeed TemperaCoat | Tempera Coat |
| Wing Motif | Regel prüft | Creature Motif |
| Wing Motif | Regel prüft | Living Muse |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `ChromaticFantasyPvE`, `SkyshardPvE`, `StarstormPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Creature Motif (`CreatureMotifPvE`, Spell): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Landscape Motif (`LandscapeMotifPvE`, Spell): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Living Muse (`LivingMusePvE`, Ability): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Scenic Muse (`ScenicMusePvE`, Ability): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Sleep (`SleepPvE`, Spell): ungenutzt
- Steel Muse (`SteelMusePvE`, Ability): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Weapon Motif (`WeaponMotifPvE`, Spell): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
