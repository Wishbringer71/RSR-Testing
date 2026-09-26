# MCH — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `MachinistRotation`
- Rotation: `RotationSolver/RebornRotations/Ranged/MCH_Reborn.cs`
- Matrix als Tabelle: `MCH.csv`

## Nutzung

direkt: 36 · ungenutzt: 8 · über andere Aktion: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Fernkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Fernkämpfer | Foot Graze (`FootGrazePvE`) | 7553 | Ability | ungenutzt |
| Fernkämpfer | Head Graze (`HeadGrazePvE`) | 7551 | Ability | direkt |
| Fernkämpfer | Leg Graze (`LegGrazePvE`) | 7554 | Ability | ungenutzt |
| Fernkämpfer | Peloton (`PelotonPvE`) | 7557 | Ability | direkt |
| Fernkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Job | Air Anchor (`AirAnchorPvE`) | 16500 | Weaponskill | direkt |
| Job | Arm Punch (`ArmPunchPvE`) | 16504 | Weaponskill | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Auto Crossbow (`AutoCrossbowPvE`) | 16497 | Weaponskill | direkt |
| Job | Automaton Queen (`AutomatonQueenPvE`) | 16501 | Ability | direkt |
| Job | Barrel Stabilizer (`BarrelStabilizerPvE`) | 7414 | Ability | direkt |
| Job | Bioblaster (`BioblasterPvE`) | 16499 | Weaponskill | direkt |
| Job | Blazing Shot (`BlazingShotPvE`) | 36978 | Weaponskill | direkt |
| Job | Chain Saw (`ChainSawPvE`) | 25788 | Weaponskill | direkt |
| Job | Checkmate (`CheckmatePvE`) | 36980 | Ability | direkt |
| Job | Clean Shot (`CleanShotPvE`) | 2873 | Weaponskill | direkt |
| Job | Crowned Collider (`CrownedColliderPvE`) | 25787 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Detonator (`DetonatorPvE`) | 16766 | Ability | über Wildfire |
| Job | Dismantle (`DismantlePvE`) | 2887 | Ability | direkt |
| Job | Double Check (`DoubleCheckPvE`) | 36979 | Ability | direkt |
| Job | Drill (`DrillPvE`) | 16498 | Weaponskill | direkt |
| Job | Excavator (`ExcavatorPvE`) | 36981 | Weaponskill | direkt |
| Job | Flamethrower (`FlamethrowerPvE`) | 7418 | Ability | ungenutzt |
| Job | Full Metal Field (`FullMetalFieldPvE`) | 36982 | Weaponskill | direkt |
| Job | Gauss Round (`GaussRoundPvE`) | 2874 | Ability | direkt |
| Job | Heat Blast (`HeatBlastPvE`) | 7410 | Weaponskill | direkt |
| Job | Heated Clean Shot (`HeatedCleanShotPvE`) | 7413 | Weaponskill | direkt |
| Job | Heated Slug Shot (`HeatedSlugShotPvE`) | 7412 | Weaponskill | direkt |
| Job | Heated Split Shot (`HeatedSplitShotPvE`) | 7411 | Weaponskill | direkt |
| Job | Hot Shot (`HotShotPvE`) | 2872 | Weaponskill | direkt |
| Job | Hypercharge (`HyperchargePvE`) | 17209 | Ability | direkt |
| Job | Pile Bunker (`PileBunkerPvE`) | 16503 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Queen Overdrive (`QueenOverdrivePvE`) | 16502 | Ability | über Rook Overdrive |
| Job | Reassemble (`ReassemblePvE`) | 2876 | Ability | direkt |
| Job | Ricochet (`RicochetPvE`) | 2890 | Ability | direkt |
| Job | Roller Dash (`RollerDashPvE`) | 17206 | Weaponskill | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Rook Autoturret (`RookAutoturretPvE`) | 2864 | Ability | direkt |
| Job | Rook Overdrive (`RookOverdrivePvE`) | 7415 | Ability | direkt |
| Job | Rook Overload (`RookOverloadPvE`) | 7416 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Scattergun (`ScattergunPvE`) | 25786 | Weaponskill | direkt |
| Job | Slug Shot (`SlugShotPvE`) | 2868 | Weaponskill | direkt |
| Job | Split Shot (`SplitShotPvE`) | 2866 | Weaponskill | direkt |
| Job | Spread Shot (`SpreadShotPvE`) | 2870 | Weaponskill | direkt |
| Job | Tactician (`TacticianPvE`) | 16889 | Ability | direkt |
| Job | Wildfire (`WildfirePvE`) | 2878 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Auto Crossbow | Bedingung (kein Status) | firearm is Overheated |
| Automaton Queen | kostet | Battery |
| Automaton Queen | gemeinsame Abklingzeit | Queen Overdrive |
| Barrel Stabilizer | Bedingung (kein Status) | combat |
| Bioblaster | gemeinsame Abklingzeit | Drill |
| Blazing Shot | Bedingung (kein Status) | firearm is Overheated |
| Chain Saw | Knopf wird zu | Excavator |
| Clean Shot | Ausbau (Clean Shot Mastery) | Heated Clean Shot |
| Clean Shot | Combo nach | Heated Slug Shot |
| Clean Shot | Combo nach | Slug Shot |
| Detonator | braucht Wildfire | Wildfire |
| Excavator | braucht Excavator Ready | Eigenschaft Enhanced Multiweapon II |
| Full Metal Field | braucht Full Metal Machinist | Eigenschaft Enhanced Barrel Stabilizer |
| Gauss Round | Ausbau (Double-barrel Mastery) | Double Check |
| Heat Blast | Ausbau (Heat Blast Mastery) | Blazing Shot |
| Heat Blast | Bedingung (kein Status) | firearm is Overheated |
| Heated Clean Shot | Combo nach | Heated Slug Shot |
| Heated Slug Shot | Combo nach | Heated Split Shot |
| Hot Shot | Ausbau (Hot Shot Mastery) | Air Anchor |
| Hypercharge | kostet | Heat |
| Queen Overdrive | gemeinsame Abklingzeit | Automaton Queen |
| Ricochet | Ausbau (Double-barrel Mastery) | Checkmate |
| Rook Autoturret | Ausbau (Promotion) | Automaton Queen |
| Rook Autoturret | kostet | Battery |
| Rook Autoturret | gemeinsame Abklingzeit | Rook Overdrive |
| Rook Overdrive | Ausbau (Promotion) | Queen Overdrive |
| Rook Overdrive | gemeinsame Abklingzeit | Rook Autoturret |
| Slug Shot | Ausbau (Slug Shot Mastery) | Heated Slug Shot |
| Slug Shot | Combo nach | Heated Split Shot |
| Slug Shot | Combo nach | Split Shot |
| Split Shot | Ausbau (Split Shot Mastery) | Heated Split Shot |
| Spread Shot | Ausbau (Spread Shot Mastery) | Scattergun |
| Wildfire | Knopf wird zu | Detonator |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Air Anchor | Regel prüft | Chain Saw |
| Air Anchor | Regel prüft | Drill |
| Air Anchor | Regel prüft | Excavator |
| Air Anchor | Regel sperrt vorher | Heat Blast |
| Air Anchor | Regel sperrt vorher | Hypercharge |
| Air Anchor | Regel prüft | Wildfire |
| Automaton Queen | Regel prüft | Air Anchor |
| Automaton Queen | Regel prüft | Chain Saw |
| Automaton Queen | Regel prüft | Clean Shot |
| Automaton Queen | Regel prüft | Excavator |
| Automaton Queen | Regel prüft | Heated Clean Shot |
| Automaton Queen | Regel prüft | Hot Shot |
| Barrel Stabilizer | Regel sperrt vorher | Full Metal Field |
| Barrel Stabilizer | Regel sperrt vorher | Wildfire |
| Bioblaster | Regel sperrt vorher | Heat Blast |
| Bioblaster | Regel sperrt vorher | Hypercharge |
| Chain Saw | Regel prüft | Air Anchor |
| Chain Saw | Regel prüft | Drill |
| Chain Saw | Regel prüft | Excavator |
| Chain Saw | Regel sperrt vorher | Heat Blast |
| Chain Saw | Regel sperrt vorher | Hypercharge |
| Chain Saw | Regel prüft | Wildfire |
| Checkmate | Regel prüft | Full Metal Field |
| Checkmate | Regel sperrt vorher | Full Metal Field |
| Checkmate | Regel prüft | Ricochet |
| Checkmate | Regel sperrt vorher | Wildfire |
| Clean Shot | Regel sperrt vorher | Heat Blast |
| Clean Shot | Regel prüft | Heated Clean Shot |
| Clean Shot | ComboIds | Heated Slug Shot |
| Clean Shot | Regel sperrt vorher | Hypercharge |
| Clean Shot | ComboIds | Slug Shot |
| Clean Shot | Regel prüft | Slug Shot |
| Dismantle | Regel sperrt vorher | Wildfire |
| Double Check | Regel prüft | Full Metal Field |
| Double Check | Regel sperrt vorher | Full Metal Field |
| Double Check | Regel prüft | Gauss Round |
| Double Check | Regel sperrt vorher | Wildfire |
| Drill | Regel sperrt vorher | Heat Blast |
| Drill | Regel sperrt vorher | Hypercharge |
| Excavator | Regel prüft | Air Anchor |
| Excavator | Regel prüft | Chain Saw |
| Excavator | Regel prüft | Drill |
| Excavator | Regel sperrt vorher | Heat Blast |
| Excavator | Regel sperrt vorher | Hypercharge |
| Excavator | Regel prüft | Wildfire |
| Full Metal Field | Regel prüft | Air Anchor |
| Full Metal Field | Regel prüft | Chain Saw |
| Full Metal Field | Regel prüft | Drill |
| Full Metal Field | Regel prüft | Excavator |
| Full Metal Field | Regel sperrt vorher | Heat Blast |
| Full Metal Field | Regel sperrt vorher | Hypercharge |
| Full Metal Field | Regel prüft | Wildfire |
| Gauss Round | Regel prüft | Double Check |
| Gauss Round | Regel prüft | Full Metal Field |
| Gauss Round | Regel sperrt vorher | Full Metal Field |
| Gauss Round | Regel sperrt vorher | Wildfire |
| Heat Blast | Regel prüft | Blazing Shot |
| Heated Clean Shot | Regel sperrt vorher | Heat Blast |
| Heated Clean Shot | ComboIds | Heated Slug Shot |
| Heated Clean Shot | Regel sperrt vorher | Hypercharge |
| Heated Clean Shot | Regel prüft | Slug Shot |
| Heated Slug Shot | Regel sperrt vorher | Heat Blast |
| Heated Slug Shot | ComboIds | Heated Split Shot |
| Heated Slug Shot | Regel sperrt vorher | Hypercharge |
| Heated Slug Shot | Regel prüft | Split Shot |
| Heated Split Shot | Regel sperrt vorher | Heat Blast |
| Heated Split Shot | Regel sperrt vorher | Hypercharge |
| Hot Shot | Regel sperrt vorher | Heat Blast |
| Hot Shot | Regel sperrt vorher | Hypercharge |
| Hypercharge | Regel sperrt vorher | Air Anchor |
| Hypercharge | Regel sperrt vorher | Chain Saw |
| Hypercharge | Regel sperrt vorher | Drill |
| Hypercharge | Regel prüft | Full Metal Field |
| Hypercharge | Regel sperrt vorher | Hot Shot |
| Hypercharge | Regel sperrt vorher | Spread Shot |
| Hypercharge | Regel prüft | Wildfire |
| Reassemble | Regel sperrt vorher | Full Metal Field |
| Reassemble | Regel sperrt vorher | Wildfire |
| Ricochet | Regel prüft | Checkmate |
| Ricochet | Regel prüft | Full Metal Field |
| Ricochet | Regel sperrt vorher | Full Metal Field |
| Ricochet | Regel sperrt vorher | Wildfire |
| Rook Autoturret | Regel prüft | Air Anchor |
| Rook Autoturret | Regel prüft | Automaton Queen |
| Rook Autoturret | Regel prüft | Chain Saw |
| Rook Autoturret | Regel prüft | Clean Shot |
| Rook Autoturret | Regel prüft | Excavator |
| Rook Autoturret | Regel prüft | Heated Clean Shot |
| Rook Autoturret | Regel prüft | Hot Shot |
| Scattergun | Regel sperrt vorher | Heat Blast |
| Scattergun | Regel sperrt vorher | Hypercharge |
| Slug Shot | Regel sperrt vorher | Heat Blast |
| Slug Shot | Regel prüft | Heated Slug Shot |
| Slug Shot | ComboIds | Heated Split Shot |
| Slug Shot | Regel sperrt vorher | Hypercharge |
| Slug Shot | ComboIds | Split Shot |
| Slug Shot | Regel prüft | Split Shot |
| Split Shot | Regel sperrt vorher | Heat Blast |
| Split Shot | Regel prüft | Heated Split Shot |
| Split Shot | Regel sperrt vorher | Hypercharge |
| Spread Shot | Regel prüft | Air Anchor |
| Spread Shot | Regel prüft | Chain Saw |
| Spread Shot | Regel prüft | Drill |
| Spread Shot | Regel sperrt vorher | Full Metal Field |
| Spread Shot | Regel sperrt vorher | Heat Blast |
| Spread Shot | Regel prüft | Hot Shot |
| Spread Shot | Regel sperrt vorher | Hypercharge |
| Spread Shot | Regel prüft | Scattergun |
| Spread Shot | Regel sperrt vorher | Wildfire |
| Tactician | Regel sperrt vorher | Wildfire |
| Wildfire | Regel prüft | Full Metal Field |
| Wildfire | Regel sperrt vorher | Full Metal Field |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BigShotPvE`, `DesperadoPvE`, `SatelliteBeamPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Arm Punch (`ArmPunchPvE`, Weaponskill): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Crowned Collider (`CrownedColliderPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Flamethrower (`FlamethrowerPvE`, Ability): ungenutzt
- Foot Graze (`FootGrazePvE`, Ability): ungenutzt
- Leg Graze (`LegGrazePvE`, Ability): ungenutzt
- Pile Bunker (`PileBunkerPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Roller Dash (`RollerDashPvE`, Weaponskill): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Rook Overload (`RookOverloadPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
