# BST — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `BeastmasterRotation`
- Rotation: `RotationSolver/RebornRotations/Limited Jobs/BST_Reborn.cs`
- Matrix als Tabelle: `BST.csv`

## Nutzung

direkt: 31 · ungenutzt: 9 · über andere Aktion: 8

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| Job | Avalanche Axe (`AvalancheAxePvE`) | 44884 | Weaponskill | direkt |
| Job | Axeblade Bite (`AxebladeBitePvE`) | 44883 | Weaponskill | direkt |
| Job | Beast Mode (`BeastModePvE`) | 44886 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Beastskin (`BeastskinPvE`) | 44896 | Ability | direkt |
| Job | Borrow (`BorrowPvE`) | 44895 | Ability | direkt |
| Job | Borrow (`BorrowPvE_47238`) | 47238 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Borrow (`BorrowPvE_47239`) | 47239 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Borrow (`BorrowPvE_47240`) | 47240 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Borrow (`BorrowPvE_47241`) | 47241 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Borrow (`BorrowPvE_47242`) | 47242 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Borrow (`BorrowPvE_47243`) | 47243 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Borrow (`BorrowPvE_47244`) | 47244 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Borrow (`BorrowPvE_47245`) | 47245 | Ability | über gleichnamige Aktion `BorrowPvE` |
| Job | Brutal Rage (`BrutalRagePvE`) | 44930 | Weaponskill | direkt |
| Job | Calamity (`CalamityPvE`) | 44933 | Weaponskill | direkt |
| Job | Capture (`CapturePvE`) | 44880 | Ability | ungenutzt |
| Job | Challenge (`ChallengePvE`) | 46750 | Ability | ungenutzt |
| Job | Cloud Skim (`CloudSkimPvE`) | 44898 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Cloud Skim (`CloudSkimPvE_45038`) | 45038 | Ability | ungenutzt |
| Job | Cloud Skim (`CloudSkimPvE_45039`) | 45039 | Ability | ungenutzt |
| Job | Cloud Skim (`CloudSkimPvE_45040`) | 45040 | Ability | ungenutzt |
| Job | Cloud Skim (`CloudSkimPvE_45041`) | 45041 | Ability | ungenutzt |
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
| Job | Tempered Release (`TemperedReleasePvE_47092`) | 47092 | Ability | direkt |
| Job | Third Battlehorn (`ThirdBattlehornPvE`) | 44894 | Ability | direkt |
| Job | Trick (`TrickPvE`) | 47093 | Ability | direkt |
| Job | Vileskin (`VileskinPvE`) | 44897 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Avalanche Axe | Ausbau (Instinctual Mastery) | Brutal Rage |
| Avalanche Axe | Knopf wird zu | Brutal Rage |
| Avalanche Axe | kostet | TP |
| Axeblade Bite | Combo nach | Smash Axe |
| Beast Mode | Knopf wird zu | Beastskin |
| Beast Mode | Knopf wird zu | Cloud Skim |
| Beast Mode | Knopf wird zu | Cloud Skim |
| Beast Mode | Knopf wird zu | Cloud Skim |
| Beast Mode | Knopf wird zu | Cloud Skim |
| Beast Mode | Knopf wird zu | Cloud Skim |
| Beast Mode | Knopf wird zu | Quelling Wave |
| Beast Mode | Knopf wird zu | Scaleskin |
| Beast Mode | Knopf wird zu | Scouring Ash |
| Beast Mode | Knopf wird zu | Seedsower |
| Beast Mode | Knopf wird zu | Soul Crush |
| Beast Mode | Knopf wird zu | Vileskin |
| Beastskin | braucht Beast Kinship (Erzeuger nicht im Text) | Beast Kinship |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Borrow | braucht One with Nature (Erzeuger nicht im Text) | One with Nature |
| Brutal Rage | kostet | TP |
| Calamity | kostet | TP |
| Cloud Skim | braucht Cloud Kinship (Erzeuger nicht im Text) | Cloud Kinship |
| Gale Axe | Ausbau (Instinctual Mastery) | Calamity |
| Gale Axe | Knopf wird zu | Calamity |
| Gale Axe | kostet | TP |
| Hawkish Talons | kostet | TP |
| Mistral Axe | Ausbau (Instinctual Mastery) | Hawkish Talons |
| Mistral Axe | Knopf wird zu | Hawkish Talons |
| Mistral Axe | kostet | TP |
| Parting Blow | Bedingung (kein Status) | combat |
| Quelling Wave | braucht Wave Kinship (Erzeuger nicht im Text) | Wave Kinship |
| Rally | Bedingung (kein Status) | combat |
| Rallying Cheer | Bedingung (kein Status) | combat |
| Risen Fall | kostet | TP |
| Scaleskin | braucht Scale Kinship (Erzeuger nicht im Text) | Scale Kinship |
| Scouring Ash | braucht Ash Kinship (Erzeuger nicht im Text) | Ash Kinship |
| Seedsower | braucht Seed Kinship (Erzeuger nicht im Text) | Seed Kinship |
| Shieldsplitter | Combo nach | Axeblade Bite |
| Soul Crush | braucht Soul Kinship (Erzeuger nicht im Text) | Soul Kinship |
| Spinning Axe | Ausbau (Instinctual Mastery) | Risen Fall |
| Spinning Axe | Knopf wird zu | Risen Fall |
| Spinning Axe | kostet | TP |
| Tempered Release | Bedingung (kein Status) | combat and under the effect of One with Nature |
| Tempered Release | Bedingung (kein Status) | combat and under the effect of One with Nature |
| Trick | kostet | TP |
| Vileskin | braucht Vile Kinship (Erzeuger nicht im Text) | Vile Kinship |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Avalanche Axe | StatusNeed VolantHeart | Gale Axe |
| Beastskin | StatusNeed BeastKinship | Status BeastKinship |
| Beastskin | StatusNeed BeastKinship_4644 | Status BeastKinship_4644 |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Borrow | StatusNeed OneWithNature | Status OneWithNature |
| Brutal Rage | StatusNeed Moonstalker | Calamity |
| Brutal Rage | StatusNeed Moonstalker | Hawkish Talons |
| Calamity | StatusNeed Sunstrider | Brutal Rage |
| Calamity | StatusNeed Sunstrider | Risen Fall |
| Cloud Skim | StatusNeed CloudKinship | Status CloudKinship |
| Cloud Skim | StatusNeed CloudKinship_4646 | Status CloudKinship_4646 |
| Cloud Skim | StatusNeed CloudKinship | Status CloudKinship |
| Cloud Skim | StatusNeed CloudKinship_4646 | Status CloudKinship_4646 |
| Cloud Skim | StatusNeed CloudKinship | Status CloudKinship |
| Cloud Skim | StatusNeed CloudKinship_4646 | Status CloudKinship_4646 |
| Cloud Skim | StatusNeed CloudKinship | Status CloudKinship |
| Cloud Skim | StatusNeed CloudKinship_4646 | Status CloudKinship_4646 |
| Cloud Skim | StatusNeed CloudKinship | Status CloudKinship |
| Cloud Skim | StatusNeed CloudKinship_4646 | Status CloudKinship_4646 |
| First Battlehorn | Regel prüft | Second Battlehorn |
| First Battlehorn | Regel prüft | Third Battlehorn |
| Gale Axe | StatusNeed EldritchHeart | Spinning Axe |
| Hawkish Talons | StatusNeed Sunstrider | Brutal Rage |
| Hawkish Talons | StatusNeed Sunstrider | Risen Fall |
| Mistral Axe | StatusNeed RampantHeart | Avalanche Axe |
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
| Scaleskin | StatusNeed ScaleKinship | Status ScaleKinship |
| Scaleskin | StatusNeed ScaleKinship_4649 | Status ScaleKinship_4649 |
| Scouring Ash | StatusNeed AshKinship | Status AshKinship |
| Scouring Ash | StatusNeed AshKinship_4651 | Status AshKinship_4651 |
| Second Battlehorn | Regel prüft | First Battlehorn |
| Second Battlehorn | Regel prüft | Third Battlehorn |
| Seedsower | StatusNeed SeedKinship | Status SeedKinship |
| Seedsower | StatusNeed SeedKinship_4647 | Status SeedKinship_4647 |
| Snarl | StatusNeed UnnamedStatus_2552 | Status UnnamedStatus_2552 |
| Soul Crush | StatusNeed SoulKinship | Status SoulKinship |
| Soul Crush | StatusNeed SoulKinship_4650 | Status SoulKinship_4650 |
| Spinning Axe | StatusNeed DurantHeart | Mistral Axe |
| Tempered Release | StatusNeed OneWithNature | Status OneWithNature |
| Tempered Release | StatusNeed OneWithNature | Status OneWithNature |
| Tempered Release | ActionCheck liest | Tempered Release |
| Tempered Release | Regel prüft | Tempered Release |
| Third Battlehorn | Regel prüft | First Battlehorn |
| Third Battlehorn | Regel prüft | Second Battlehorn |
| Trick | StatusNeed RampantHeart | Avalanche Axe |
| Trick | StatusNeed VolantHeart | Gale Axe |
| Trick | StatusNeed DurantHeart | Mistral Axe |
| Trick | Regel prüft | Parting Blow |
| Trick | StatusNeed EldritchHeart | Spinning Axe |
| Vileskin | StatusNeed VileKinship | Status VileKinship |
| Vileskin | StatusNeed VileKinship_4645 | Status VileKinship_4645 |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Snarl | sonstige | hebt Enmity Up auf | Enmity Up |

### Verlängerung, Aufbau, Umschalten

keine

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Familiar TP | Rallying Cheer | — |
| TP | Quelling Wave, Rally, Scaleskin, Scouring Ash, Shieldsplitter, Vileskin | Avalanche Axe, Brutal Rage, Calamity, Gale Axe, Hawkish Talons, Mistral Axe, Risen Fall, Spinning Axe, Trick |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Fenster, deren Verbraucher an Bedingungen hängt

Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster (Konzept 14, „Werden die Fenster genutzt"). Kandidaten, von Hand bewertet.

- Avalanche Axe: 3 Aufruf(e), **ohne Rückfall vor Ablauf**
- Beastskin: 1 Aufruf(e), **ohne Rückfall vor Ablauf**
- Borrow: 3 Aufruf(e), **ohne Rückfall vor Ablauf**
- Gale Axe: 3 Aufruf(e), **ohne Rückfall vor Ablauf**
- Mistral Axe: 3 Aufruf(e), **ohne Rückfall vor Ablauf**
- Scaleskin: 2 Aufruf(e), **ohne Rückfall vor Ablauf**
- Spinning Axe: 3 Aufruf(e), **ohne Rückfall vor Ablauf**
- Tempered Release: 3 Aufruf(e), **ohne Rückfall vor Ablauf**
- Tempered Release: 3 Aufruf(e), **ohne Rückfall vor Ablauf**
- Trick: 3 Aufruf(e), **ohne Rückfall vor Ablauf**

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Shield Charge (1), Smash Axe (1)

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Beast Mode (`BeastModePvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Capture (`CapturePvE`, Ability): ungenutzt
- Challenge (`ChallengePvE`, Ability): ungenutzt
- Cloud Skim (`CloudSkimPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Cloud Skim (`CloudSkimPvE_45038`, Ability): ungenutzt
- Cloud Skim (`CloudSkimPvE_45039`, Ability): ungenutzt
- Cloud Skim (`CloudSkimPvE_45040`, Ability): ungenutzt
- Cloud Skim (`CloudSkimPvE_45041`, Ability): ungenutzt
- Gauge (`GaugePvE`, Ability): ungenutzt
