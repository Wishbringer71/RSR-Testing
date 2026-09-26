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
| Aero II in Green | braucht Aetherhues and not under the effect of Subtractive Palette | Subtractive Palette |
| Aero II in Green | Knopf wird zu | Water II in Blue |
| Aero in Green | braucht Aetherhues and not under the effect of Subtractive Palette | Subtractive Palette |
| Aero in Green | Knopf wird zu | Water in Blue |
| Blizzard II in Cyan | Knopf wird zu | Stone II in Yellow |
| Blizzard II in Cyan | braucht Subtractive Palette | Subtractive Palette |
| Blizzard in Cyan | Knopf wird zu | Stone in Yellow |
| Blizzard in Cyan | braucht Subtractive Palette | Subtractive Palette |
| Clawed Muse | Bedingung (kein Status) | a claw is painted on the Creature Canvas |
| Comet in Black | braucht Black Paint and Monochrome Tones (Erzeuger nicht im Text) | Black Paint and Monochrome Tones |
| Comet in Black | kostet | Paint |
| Creature Motif | Knopf wird zu | Claw Motif |
| Creature Motif | Knopf wird zu | Maw Motif |
| Creature Motif | Knopf wird zu | Pom Motif |
| Creature Motif | Knopf wird zu | Wing Motif |
| Fanged Muse | Bedingung (kein Status) | fangs are painted on the Creature Canvas |
| Fire II in Red | Knopf wird zu | Aero II in Green |
| Fire in Red | Knopf wird zu | Aero in Green |
| Hammer Brush | braucht Hammer Time (Erzeuger nicht im Text) | Hammer Time |
| Hammer Brush | Combo nach | Hammer Stamp |
| Hammer Brush | Knopf wird zu | Polishing Hammer |
| Hammer Stamp | braucht Hammer Time (Erzeuger nicht im Text) | Hammer Time |
| Hammer Stamp | Knopf wird zu | Hammer Brush |
| Holy in White | braucht White Paint | Eigenschaft Enhanced Artistry |
| Holy in White | braucht White Paint | Rainbow Drip |
| Holy in White | kostet | White Paint |
| Landscape Motif | Knopf wird zu | Starry Sky Motif |
| Living Muse | Knopf wird zu | Clawed Muse |
| Living Muse | Knopf wird zu | Fanged Muse |
| Living Muse | Knopf wird zu | Pom Muse |
| Living Muse | Knopf wird zu | Winged Muse |
| Living Muse | Bedingung (kein Status) | a Creature Motif is depicted on the Creature Canvas |
| Mog of the Ages | braucht Moogle Portrait (Erzeuger nicht im Text) | Moogle Portrait |
| Mog of the Ages | Knopf wird zu | Retribution of the Madeen |
| Polishing Hammer | braucht Hammer Time (Erzeuger nicht im Text) | Hammer Time |
| Polishing Hammer | Combo nach | Hammer Brush |
| Pom Muse | Bedingung (kein Status) | a pom is painted on the Creature Canvas |
| Retribution of the Madeen | braucht Madeen Portrait (Erzeuger nicht im Text) | Madeen Portrait |
| Scenic Muse | Knopf wird zu | Starry Muse |
| Star Prism | braucht Starstruck | Eigenschaft Enhanced Pictomancy V |
| Starry Muse | Bedingung (kein Status) | combat and when a starry sky is painted on the Landscape Canvas |
| Steel Muse | Knopf wird zu | Striking Muse |
| Stone II in Yellow | braucht Aetherhues and Subtractive Palette | Subtractive Palette |
| Stone II in Yellow | Knopf wird zu | Thunder II in Magenta |
| Stone in Yellow | braucht Aetherhues and Subtractive Palette | Subtractive Palette |
| Stone in Yellow | Knopf wird zu | Thunder in Magenta |
| Striking Muse | Bedingung (kein Status) | combat and when a hammer is painted on the Weapon Canvas |
| Subtractive Palette | kostet | Palette |
| Tempera Grassa | braucht Tempera Coat | Tempera Coat |
| Thunder II in Magenta | braucht Aetherhues II and Subtractive Palette | Subtractive Palette |
| Thunder in Magenta | braucht Aetherhues II and Subtractive Palette | Subtractive Palette |
| Water II in Blue | braucht Aetherhues II and not under the effect of Subtractive Palette | Subtractive Palette |
| Water in Blue | braucht Aetherhues II and not under the effect of Subtractive Palette | Subtractive Palette |
| Weapon Motif | Knopf wird zu | Hammer Motif |
| Winged Muse | Bedingung (kein Status) | a pair of wings is painted on the Creature Canvas |

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

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Fire II in Red | Angriff | nicht nutzbar unter Subtractive Palette | Subtractive Palette |
| Fire in Red | Angriff | nicht nutzbar unter Subtractive Palette | Subtractive Palette |
| Hammer Motif | sonstige | nicht nutzbar unter Hammer Time | Hammer Time |
| Holy in White | Angriff | nicht nutzbar unter Monochrome Tones | Monochrome Tones |
| Starry Sky Motif | sonstige | nicht nutzbar unter Starry Muse | Starry Muse |
| Tempera Grassa | Abwehr | hebt Tempera Coat auf | Tempera Coat |

### Verlängerung, Aufbau, Umschalten

- Subtractive Palette: 3 Stapel Subtractive Palette

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Paint | — (nicht im Wirktext) | Comet in Black |
| Palette | — (nicht im Wirktext) | Subtractive Palette |
| White Paint | — (nicht im Wirktext) | Holy in White |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Fenster, deren Verbraucher an Bedingungen hängt

Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster (Konzept 14, „Werden die Fenster genutzt"). Kandidaten, von Hand bewertet.

- Star Prism: 1 Aufruf(e), **ohne Rückfall vor Ablauf**
- Tempera Grassa: 2 Aufruf(e), mit Rückfall vor Ablauf

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Addle (1), Aero in Green (1), Blizzard in Cyan (1), Comet in Black (1), Fire in Red (1), Hammer Brush (1), Hammer Stamp (1), Holy in White (1), Mog of the Ages (1), Polishing Hammer (1), Pom Muse (1), Stone in Yellow (1), Thunder in Magenta (1), Water in Blue (1), Winged Muse (1)

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
