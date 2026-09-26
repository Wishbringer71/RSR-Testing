# RPR — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `ReaperRotation`
- Rotation: `RotationSolver/RebornRotations/Melee/RPR_Reborn.cs`
- Matrix als Tabelle: `RPR.csv`

## Nutzung

direkt: 42 · über andere Aktion: 3

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Nahkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Nahkämpfer | Bloodbath (`BloodbathPvE`) | 7542 | Ability | direkt |
| Nahkämpfer | Feint (`FeintPvE`) | 7549 | Ability | direkt |
| Nahkämpfer | Leg Sweep (`LegSweepPvE`) | 7863 | Ability | direkt |
| Nahkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Nahkämpfer | True North (`TrueNorthPvE`) | 7546 | Ability | direkt |
| Job | Arcane Circle (`ArcaneCirclePvE`) | 24405 | Ability | direkt |
| Job | Arcane Crest (`ArcaneCrestPvE`) | 24404 | Ability | direkt |
| Job | Blood Stalk (`BloodStalkPvE`) | 24389 | Ability | direkt |
| Job | Communio (`CommunioPvE`) | 24398 | Spell | direkt |
| Job | Cross Reaping (`CrossReapingPvE`) | 24396 | Weaponskill | direkt |
| Job | Enshroud (`EnshroudPvE`) | 24394 | Ability | direkt |
| Job | Executioner's Gallows (`ExecutionersGallowsPvE`) | 36971 | Weaponskill | direkt |
| Job | Executioner's Gibbet (`ExecutionersGibbetPvE`) | 36970 | Weaponskill | direkt |
| Job | Executioner's Guillotine (`ExecutionersGuillotinePvE`) | 36972 | Weaponskill | direkt |
| Job | Gallows (`GallowsPvE`) | 24383 | Weaponskill | direkt |
| Job | Gibbet (`GibbetPvE`) | 24382 | Weaponskill | direkt |
| Job | Gluttony (`GluttonyPvE`) | 24393 | Ability | direkt |
| Job | Grim Reaping (`GrimReapingPvE`) | 24397 | Weaponskill | direkt |
| Job | Grim Swathe (`GrimSwathePvE`) | 24392 | Ability | direkt |
| Job | Guillotine (`GuillotinePvE`) | 24384 | Weaponskill | direkt |
| Job | Harpe (`HarpePvE`) | 24386 | Spell | direkt |
| Job | Harvest Moon (`HarvestMoonPvE`) | 24388 | Spell | direkt |
| Job | Hell's Egress (`HellsEgressPvE`) | 24402 | Ability | direkt |
| Job | Hell's Ingress (`HellsIngressPvE`) | 24401 | Ability | direkt |
| Job | Infernal Slice (`InfernalSlicePvE`) | 24375 | Weaponskill | direkt |
| Job | Lemure's Scythe (`LemuresScythePvE`) | 24400 | Ability | direkt |
| Job | Lemure's Slice (`LemuresSlicePvE`) | 24399 | Ability | direkt |
| Job | Nightmare Scythe (`NightmareScythePvE`) | 24377 | Weaponskill | direkt |
| Job | Perfectio (`PerfectioPvE`) | 36973 | Weaponskill | direkt |
| Job | Plentiful Harvest (`PlentifulHarvestPvE`) | 24385 | Weaponskill | direkt |
| Job | Regress (`RegressPvE`) | 24403 | Ability | über Hell's Ingress |
| Job | Sacrificium (`SacrificiumPvE`) | 36969 | Ability | direkt |
| Job | Shadow of Death (`ShadowOfDeathPvE`) | 24378 | Weaponskill | direkt |
| Job | Slice (`SlicePvE`) | 24373 | Weaponskill | direkt |
| Job | Soul Scythe (`SoulScythePvE`) | 24381 | Weaponskill | direkt |
| Job | Soul Slice (`SoulSlicePvE`) | 24380 | Weaponskill | direkt |
| Job | Soulsow (`SoulsowPvE`) | 24387 | Spell | direkt |
| Job | Spinning Scythe (`SpinningScythePvE`) | 24376 | Weaponskill | direkt |
| Job | Unveiled Gallows (`UnveiledGallowsPvE`) | 24391 | Ability | über Blood Stalk |
| Job | Unveiled Gibbet (`UnveiledGibbetPvE`) | 24390 | Ability | über Blood Stalk |
| Job | Void Reaping (`VoidReapingPvE`) | 24395 | Weaponskill | direkt |
| Job | Waxing Slice (`WaxingSlicePvE`) | 24374 | Weaponskill | direkt |
| Job | Whorl of Death (`WhorlOfDeathPvE`) | 24379 | Weaponskill | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Blood Stalk | gemeinsame Abklingzeit | Gluttony |
| Blood Stalk | Knopf wird zu | Lemure's Slice |
| Blood Stalk | kostet | Soul |
| Blood Stalk | Knopf wird zu | Unveiled Gallows |
| Blood Stalk | Knopf wird zu | Unveiled Gibbet |
| Communio | Knopf wird zu | Perfectio |
| Cross Reaping | braucht Lemure Shroud (Erzeuger nicht im Text) | Lemure Shroud |
| Cross Reaping | kostet | Lemure Shroud |
| Enshroud | kostet | Shroud |
| Executioner's Gallows | braucht Executioner | Eigenschaft Enhanced Gluttony |
| Executioner's Gibbet | braucht Executioner | Eigenschaft Enhanced Gluttony |
| Executioner's Guillotine | braucht Executioner | Eigenschaft Enhanced Gluttony |
| Gallows | Knopf wird zu | Cross Reaping |
| Gallows | braucht Soul Reaver | Eigenschaft Enhanced Avatar |
| Gallows | Knopf wird zu | Executioner's Gallows |
| Gallows | braucht Soul Reaver | Unveiled Gallows |
| Gallows | braucht Soul Reaver | Unveiled Gibbet |
| Gibbet | braucht Soul Reaver | Eigenschaft Enhanced Avatar |
| Gibbet | Knopf wird zu | Executioner's Gibbet |
| Gibbet | braucht Soul Reaver | Unveiled Gallows |
| Gibbet | braucht Soul Reaver | Unveiled Gibbet |
| Gibbet | Knopf wird zu | Void Reaping |
| Gluttony | Knopf wird zu | Sacrificium |
| Gluttony | kostet | Soul |
| Grim Reaping | braucht Enshrouded (Erzeuger nicht im Text) | Enshrouded |
| Grim Reaping | kostet | Lemure Shroud |
| Grim Swathe | gemeinsame Abklingzeit | Gluttony |
| Grim Swathe | Knopf wird zu | Lemure's Scythe |
| Grim Swathe | kostet | Soul |
| Guillotine | braucht Soul Reaver | Eigenschaft Enhanced Avatar |
| Guillotine | Knopf wird zu | Executioner's Guillotine |
| Guillotine | Knopf wird zu | Grim Reaping |
| Guillotine | braucht Soul Reaver | Unveiled Gallows |
| Guillotine | braucht Soul Reaver | Unveiled Gibbet |
| Harvest Moon | braucht Soulsow | Soulsow |
| Hell's Egress | gemeinsame Abklingzeit | Hell's Ingress |
| Hell's Egress | Knopf wird zu | Regress |
| Hell's Ingress | gemeinsame Abklingzeit | Hell's Egress |
| Hell's Ingress | Knopf wird zu | Regress |
| Infernal Slice | Combo nach | Waxing Slice |
| Lemure's Scythe | gemeinsame Abklingzeit | Lemure's Slice |
| Lemure's Scythe | kostet | Void Shroud |
| Lemure's Slice | gemeinsame Abklingzeit | Lemure's Scythe |
| Lemure's Slice | kostet | Void Shroud |
| Nightmare Scythe | Combo nach | Spinning Scythe |
| Perfectio | braucht Perfectio Parata (Erzeuger nicht im Text) | Perfectio Parata |
| Plentiful Harvest | kostet | Immortal Sacrifice |
| Regress | braucht Threshold (Erzeuger nicht im Text) | Threshold |
| Sacrificium | braucht Enshrouded and Oblatio (Erzeuger nicht im Text) | Enshrouded and Oblatio |
| Soul Scythe | gemeinsame Abklingzeit | Soul Slice |
| Soul Slice | gemeinsame Abklingzeit | Soul Scythe |
| Soulsow | Knopf wird zu | Harvest Moon |
| Unveiled Gallows | braucht Enhanced Gallows | Executioner's Gibbet |
| Unveiled Gallows | braucht Enhanced Gallows | Gibbet |
| Unveiled Gallows | gemeinsame Abklingzeit | Gluttony |
| Unveiled Gallows | kostet | Soul |
| Unveiled Gibbet | braucht Enhanced Gibbet | Executioner's Gallows |
| Unveiled Gibbet | braucht Enhanced Gibbet | Gallows |
| Unveiled Gibbet | gemeinsame Abklingzeit | Gluttony |
| Unveiled Gibbet | kostet | Soul |
| Void Reaping | braucht Enshrouded (Erzeuger nicht im Text) | Enshrouded |
| Void Reaping | kostet | Lemure Shroud |
| Waxing Slice | Combo nach | Slice |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Blood Stalk | Regel prüft | Gluttony |
| Enshroud | Regel prüft | Feint |
| Enshroud | Regel prüft | Gluttony |
| Executioner's Gallows | Regel prüft | Blood Stalk |
| Executioner's Gallows | Regel prüft | Gluttony |
| Executioner's Gibbet | Regel prüft | Blood Stalk |
| Executioner's Gibbet | Regel prüft | Gluttony |
| Executioner's Guillotine | Regel prüft | Blood Stalk |
| Executioner's Guillotine | Regel prüft | Gluttony |
| Feint | Regel prüft | Enshroud |
| Feint | Regel prüft | Gluttony |
| Gluttony | Regel prüft | Enshroud |
| Gluttony | Regel prüft | Feint |
| Gluttony | Regel prüft | Plentiful Harvest |
| Grim Swathe | Regel prüft | Gluttony |
| Infernal Slice | Regel prüft | Slice |
| Infernal Slice | Regel prüft | Waxing Slice |
| Shadow of Death | Regel prüft | Arcane Circle |
| Shadow of Death | Regel prüft | Communio |
| Shadow of Death | Regel prüft | Plentiful Harvest |
| Waxing Slice | Regel prüft | Slice |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Plentiful Harvest | Angriff | nicht nutzbar unter Bloodsown Circle | Bloodsown Circle |

### Verlängerung, Aufbau, Umschalten

- Shadow of Death verlängert Death's Design um 30 s, höchstens auf 60 s
- Whorl of Death verlängert Death's Design um 30 s, höchstens auf 60 s

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Immortal Sacrifice | — (nicht im Wirktext) | Plentiful Harvest |
| Lemure Shroud | — (nicht im Wirktext) | Cross Reaping, Grim Reaping, Void Reaping |
| Shroud | — (nicht im Wirktext) | Enshroud |
| Soul | Harvest Moon, Soul Scythe, Soul Slice | Blood Stalk, Gluttony, Grim Swathe, Unveiled Gallows, Unveiled Gibbet |
| Void Shroud | — (nicht im Wirktext) | Lemure's Scythe, Lemure's Slice |

### Kandidaten für Selbsterhaltung

- Gallows: Status-Kreislauf mit Unveiled Gibbet
- Gibbet: Status-Kreislauf mit Unveiled Gallows

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Cross Reaping (2), Feint (1), Gallows (3), Gibbet (3), Gluttony (1), Harpe (1), Harvest Moon (1), Infernal Slice (1), Lemure's Slice (1), Shadow of Death (1), Slice (1), Soul Slice (1), Unveiled Gallows (1), Unveiled Gibbet (1), Void Reaping (2), Waxing Slice (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BladedancePvE`, `BraverPvE`, `TheEndPvE`

## Nicht direkt genutzt

keine
