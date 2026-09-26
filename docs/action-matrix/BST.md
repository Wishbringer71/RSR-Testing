# BST — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `BeastmasterRotation`
- Rotation: `RotationSolver/RebornRotations/Limited Jobs/BST_Reborn.cs`
- Matrix als Tabelle: `BST.csv`

## Nutzung

direkt: 30 · ungenutzt: 5

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| Job | Avalanche Axe (`AvalancheAxePvE`) | 44884 | Weaponskill | direkt |
| Job | Axeblade Bite (`AxebladeBitePvE`) | 44883 | Weaponskill | direkt |
| Job | Beast Mode (`BeastModePvE`) | 44886 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Beastskin (`BeastskinPvE`) | 44896 | Ability | direkt |
| Job | Borrow (`BorrowPvE`) | 44895 | Ability | direkt |
| Job | Brutal Rage (`BrutalRagePvE`) | 44930 | Weaponskill | direkt |
| Job | Calamity (`CalamityPvE`) | 44933 | Weaponskill | direkt |
| Job | Capture (`CapturePvE`) | 44880 | Ability | ungenutzt |
| Job | Challenge (`ChallengePvE`) | 46750 | Ability | ungenutzt |
| Job | Cloud Skim (`CloudSkimPvE`) | 44898 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | First Battlehorn (`FirstBattlehornPvE`) | 44881 | Ability | direkt |
| Job | Gale Axe (`GaleAxePvE`) | 44889 | Weaponskill | direkt |
| Job | Gauge (`GaugePvE`) | 44882 | Ability | ungenutzt |
| Job | Hawkish Talons (`HawkishTalonsPvE`) | 44931 | Weaponskill | direkt |
| Job | Mistral Axe (`MistralAxePvE`) | 44887 | Weaponskill | direkt |
| Job | Parting Blow (`PartingBlowPvE`) | 44891 | Ability | direkt |
| Job | Quelling Wave (`QuellingWavePvE`) | 44900 | Spell | direkt |
| Job | Rally (`RallyPvE`) | 44905 | Ability | direkt |
| Job | Rallying Cheer (`RallyingCheerPvE`) | 44904 | Ability | direkt |
| Job | Risen Fall (`RisenFallPvE`) | 44932 | Weaponskill | direkt |
| Job | Scaleskin (`ScaleskinPvE`) | 44901 | Ability | direkt |
| Job | Scouring Ash (`ScouringAshPvE`) | 44903 | Ability | direkt |
| Job | Second Battlehorn (`SecondBattlehornPvE`) | 44892 | Ability | direkt |
| Job | Seedsower (`SeedsowerPvE`) | 44899 | Ability | direkt |
| Job | Shield Charge (`ShieldChargePvE`) | 44893 | Ability | direkt |
| Job | Shieldsplitter (`ShieldsplitterPvE`) | 44885 | Weaponskill | direkt |
| Job | Smash Axe (`SmashAxePvE`) | 44879 | Weaponskill | direkt |
| Job | Snarl (`SnarlPvE`) | 46751 | Ability | direkt |
| Job | Soul Crush (`SoulCrushPvE`) | 44902 | Ability | direkt |
| Job | Spinning Axe (`SpinningAxePvE`) | 44888 | Weaponskill | direkt |
| Job | Sprint (`SprintPvE`) | 3 | System | direkt |
| Job | Tempered Release (`TemperedReleasePvE`) | 44890 | Ability | direkt |
| Job | Third Battlehorn (`ThirdBattlehornPvE`) | 44894 | Ability | direkt |
| Job | Trick (`TrickPvE`) | 47093 | Ability | direkt |
| Job | Vileskin (`VileskinPvE`) | 44897 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Avalanche Axe | Ausbau (Instinctual Mastery) | Brutal Rage |
| Avalanche Axe | Knopf wird zu | Brutal Rage |
| Avalanche Axe | kostet | Minimum TP |
| Axeblade Bite | Combo nach | Smash Axe |
| Beast Mode | Knopf wird zu | Beastskin |
| Beast Mode | Knopf wird zu | Cloud Skim |
| Beast Mode | Knopf wird zu | Quelling Wave |
| Beast Mode | Knopf wird zu | Scaleskin |
| Beast Mode | Knopf wird zu | Scouring Ash |
| Beast Mode | Knopf wird zu | Seedsower |
| Beast Mode | Knopf wird zu | Soul Crush |
| Beast Mode | Knopf wird zu | Vileskin |
| Beastskin | braucht (Erzeuger nicht im Text) | Beast Kinship |
| Borrow | braucht (Erzeuger nicht im Text) | One with Nature |
| Brutal Rage | kostet | TP |
| Calamity | kostet | TP |
| Cloud Skim | braucht (Erzeuger nicht im Text) | Cloud Kinship |
| Gale Axe | Ausbau (Instinctual Mastery) | Calamity |
| Gale Axe | Knopf wird zu | Calamity |
| Gale Axe | kostet | Minimum TP |
| Hawkish Talons | kostet | TP |
| Mistral Axe | Ausbau (Instinctual Mastery) | Hawkish Talons |
| Mistral Axe | Knopf wird zu | Hawkish Talons |
| Mistral Axe | kostet | Minimum TP |
| Parting Blow | braucht (Erzeuger nicht im Text) | combat |
| Quelling Wave | braucht (Erzeuger nicht im Text) | Wave Kinship |
| Rally | braucht (Erzeuger nicht im Text) | combat |
| Rallying Cheer | braucht (Erzeuger nicht im Text) | combat |
| Risen Fall | kostet | TP |
| Scaleskin | braucht (Erzeuger nicht im Text) | Scale Kinship |
| Scouring Ash | braucht (Erzeuger nicht im Text) | Ash Kinship |
| Seedsower | braucht (Erzeuger nicht im Text) | Seed Kinship |
| Shieldsplitter | Combo nach | Axeblade Bite |
| Soul Crush | braucht (Erzeuger nicht im Text) | Soul Kinship |
| Spinning Axe | kostet | Minimum TP |
| Spinning Axe | Ausbau (Instinctual Mastery) | Risen Fall |
| Spinning Axe | Knopf wird zu | Risen Fall |
| Tempered Release | braucht (Erzeuger nicht im Text) | combat |
| Tempered Release | braucht (Erzeuger nicht im Text) | under the effect of One with Nature |
| Trick | kostet | Minimum TP |
| Vileskin | braucht (Erzeuger nicht im Text) | Vile Kinship |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Avalanche Axe | StatusNeed VolantHeart | Gale Axe |
| Beastskin | StatusNeed BeastKinship | Status BeastKinship |
| Beastskin | StatusNeed BeastKinship_4644 | Status BeastKinship_4644 |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | Regel sperrt vorher | Tempered Release |
| Brutal Rage | StatusNeed Moonstalker | Calamity |
| Brutal Rage | StatusNeed Moonstalker | Hawkish Talons |
| Brutal Rage | Regel sperrt vorher | Tempered Release |
| Calamity | StatusNeed Sunstrider | Brutal Rage |
| Calamity | StatusNeed Sunstrider | Risen Fall |
| Calamity | Regel sperrt vorher | Tempered Release |
| Cloud Skim | StatusNeed CloudKinship | Status CloudKinship |
| Cloud Skim | StatusNeed CloudKinship_4646 | Status CloudKinship_4646 |
| First Battlehorn | Regel prüft | Second Battlehorn |
| First Battlehorn | Regel prüft | Third Battlehorn |
| Gale Axe | StatusNeed EldritchHeart | Spinning Axe |
| Hawkish Talons | StatusNeed Sunstrider | Brutal Rage |
| Hawkish Talons | StatusNeed Sunstrider | Risen Fall |
| Hawkish Talons | Regel sperrt vorher | Tempered Release |
| Mistral Axe | StatusNeed RampantHeart | Avalanche Axe |
| Parting Blow | Regel sperrt vorher | Tempered Release |
| Quelling Wave | StatusNeed BlazeSpikes_5465 | Status BlazeSpikes_5465 |
| Quelling Wave | StatusNeed DamageUp_1225 | Status DamageUp_1225 |
| Quelling Wave | StatusNeed DamageUp_2550 | Status DamageUp_2550 |
| Quelling Wave | StatusNeed MagicDamageUp_5020 | Status MagicDamageUp_5020 |
| Quelling Wave | StatusNeed PhysicalDamageUp_2074 | Status PhysicalDamageUp_2074 |
| Quelling Wave | StatusNeed PopotoSkin | Status PopotoSkin |
| Quelling Wave | StatusNeed WaveKinship | Status WaveKinship |
| Quelling Wave | StatusNeed WaveKinship_4648 | Status WaveKinship_4648 |
| Risen Fall | StatusNeed Moonstalker | Calamity |
| Risen Fall | StatusNeed Moonstalker | Hawkish Talons |
| Risen Fall | Regel sperrt vorher | Tempered Release |
| Scaleskin | StatusNeed ScaleKinship | Status ScaleKinship |
| Scaleskin | StatusNeed ScaleKinship_4649 | Status ScaleKinship_4649 |
| Scouring Ash | StatusNeed AshKinship | Status AshKinship |
| Scouring Ash | StatusNeed AshKinship_4651 | Status AshKinship_4651 |
| Second Battlehorn | Regel prüft | First Battlehorn |
| Second Battlehorn | Regel prüft | Third Battlehorn |
| Seedsower | StatusNeed SeedKinship | Status SeedKinship |
| Seedsower | StatusNeed SeedKinship_4647 | Status SeedKinship_4647 |
| Seedsower | Regel sperrt vorher | Tempered Release |
| Shield Charge | Regel sperrt vorher | Tempered Release |
| Snarl | StatusNeed UnnamedStatus_2552 | Status UnnamedStatus_2552 |
| Soul Crush | StatusNeed SoulKinship | Status SoulKinship |
| Soul Crush | StatusNeed SoulKinship_4650 | Status SoulKinship_4650 |
| Soul Crush | Regel sperrt vorher | Tempered Release |
| Spinning Axe | StatusNeed DurantHeart | Mistral Axe |
| Tempered Release | StatusNeed OneWithNature | Status OneWithNature |
| Third Battlehorn | Regel prüft | First Battlehorn |
| Third Battlehorn | Regel prüft | Second Battlehorn |
| Trick | StatusNeed RampantHeart | Avalanche Axe |
| Trick | StatusNeed VolantHeart | Gale Axe |
| Trick | StatusNeed DurantHeart | Mistral Axe |
| Trick | Regel prüft | Parting Blow |
| Trick | StatusNeed EldritchHeart | Spinning Axe |
| Vileskin | StatusNeed VileKinship | Status VileKinship |
| Vileskin | StatusNeed VileKinship_4645 | Status VileKinship_4645 |

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Beast Mode (`BeastModePvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Capture (`CapturePvE`, Ability): ungenutzt
- Challenge (`ChallengePvE`, Ability): ungenutzt
- Cloud Skim (`CloudSkimPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Gauge (`GaugePvE`, Ability): ungenutzt
