# BLM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `BlackMageRotation`
- Rotation: `RotationSolver/RebornRotations/Magical/BLM_Default.cs`, `RotationSolver/RebornRotations/Magical/BLM_RP.cs`
- Matrix als Tabelle: `BLM.csv`

## Nutzung

direkt: 37 · ungenutzt: 1 · über andere Aktion: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Magier | Addle (`AddlePvE`) | 7560 | Ability | direkt |
| Magier | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Magier | Sleep (`SleepPvE`) | 25880 | Spell | ungenutzt |
| Magier | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Magier | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Aetherial Manipulation (`AetherialManipulationPvE`) | 155 | Ability | direkt |
| Job | Amplifier (`AmplifierPvE`) | 25796 | Ability | direkt |
| Job | Between the Lines (`BetweenTheLinesPvE`) | 7419 | Ability | direkt |
| Job | Blizzard (`BlizzardPvE`) | 142 | Spell | direkt |
| Job | Blizzard II (`BlizzardIiPvE`) | 25793 | Spell | direkt |
| Job | Blizzard III (`BlizzardIiiPvE`) | 154 | Spell | direkt |
| Job | Blizzard IV (`BlizzardIvPvE`) | 3576 | Spell | direkt |
| Job | Despair (`DespairPvE`) | 16505 | Spell | direkt |
| Job | Fire (`FirePvE`) | 141 | Spell | direkt |
| Job | Fire II (`FireIiPvE`) | 147 | Spell | direkt |
| Job | Fire III (`FireIiiPvE`) | 152 | Spell | direkt |
| Job | Fire IV (`FireIvPvE`) | 3577 | Spell | direkt |
| Job | Flare (`FlarePvE`) | 162 | Spell | direkt |
| Job | Flare Star (`FlareStarPvE`) | 36989 | Spell | direkt |
| Job | Foul (`FoulPvE`) | 7422 | Spell | direkt |
| Job | Freeze (`FreezePvE`) | 159 | Spell | direkt |
| Job | High Blizzard II (`HighBlizzardIiPvE`) | 25795 | Spell | über Blizzard II |
| Job | High Fire II (`HighFireIiPvE`) | 25794 | Spell | über Fire II |
| Job | High Thunder (`HighThunderPvE`) | 36986 | Spell | direkt |
| Job | High Thunder II (`HighThunderIiPvE`) | 36987 | Spell | direkt |
| Job | Ley Lines (`LeyLinesPvE`) | 3573 | Ability | direkt |
| Job | Manafont (`ManafontPvE`) | 158 | Ability | direkt |
| Job | Manaward (`ManawardPvE`) | 157 | Ability | direkt |
| Job | Paradox (`ParadoxPvE`) | 25797 | Spell | direkt |
| Job | Retrace (`RetracePvE`) | 36988 | Ability | direkt |
| Job | Scathe (`ScathePvE`) | 156 | Spell | direkt |
| Job | Thunder (`ThunderPvE`) | 144 | Spell | direkt |
| Job | Thunder II (`ThunderIiPvE`) | 7447 | Spell | direkt |
| Job | Thunder III (`ThunderIiiPvE`) | 153 | Spell | direkt |
| Job | Thunder IV (`ThunderIvPvE`) | 7420 | Spell | direkt |
| Job | Transpose (`TransposePvE`) | 149 | Ability | direkt |
| Job | Triplecast (`TriplecastPvE`) | 7421 | Ability | direkt |
| Job | Umbral Soul (`UmbralSoulPvE`) | 16506 | Spell | direkt |
| Job | Xenoglossy (`XenoglossyPvE`) | 16507 | Spell | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Amplifier | braucht Astral Fire or Umbral Ice | Fire |
| Blizzard II | Ausbau (Aspect Mastery IV) | High Blizzard II |
| Blizzard IV | braucht Umbral Ice | Blizzard |
| Blizzard | Knopf wird zu | Paradox |
| Despair | braucht Astral Fire | Fire |
| Fire II | Ausbau (Aspect Mastery IV) | High Fire II |
| Fire IV | braucht Astral Fire | Fire |
| Fire | Knopf wird zu | Paradox |
| Flare | braucht Astral Fire | Fire |
| Flare Star | Bedingung (kein Status) | the Astral Gauge is full |
| Foul | kostet | Polyglot |
| Freeze | braucht Umbral Ice | Blizzard |
| Manafont | braucht Astral Fire | Fire |
| Paradox | braucht Paradox (Erzeuger nicht im Text) | Paradox |
| Retrace | braucht Ley Lines and the effect duration will not be reset | Ley Lines |
| Thunder II | Ausbau (Thunder Mastery II) | Thunder IV |
| Thunder III | Ausbau (Thunder Mastery III) | High Thunder |
| Thunder IV | Ausbau (Thunder Mastery III) | High Thunder II |
| Thunder | Ausbau (Thunder Mastery) | Thunder III |
| Umbral Soul | braucht Umbral Ice | Blizzard |
| Xenoglossy | kostet | Polyglot |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Blizzard IV | Regel prüft | Freeze |
| Blizzard | Regel prüft | Transpose |
| Blizzard | Regel prüft | Umbral Soul |
| Fire III | Regel prüft | Fire |
| Fire IV | Regel prüft | Fire |
| Foul | Regel prüft | Amplifier |
| Foul | Regel prüft | Xenoglossy |
| Freeze | Regel prüft | Blizzard IV |
| Retrace | Regel prüft | Ley Lines |
| Retrace | StatusNeed LeyLines | Ley Lines |
| Swiftcast | Regel prüft | Paradox |
| Thunder II | Regel sperrt vorher | Thunder III |
| Thunder II | Regel sperrt vorher | Thunder IV |
| Thunder II | Regel sperrt vorher | Thunder |
| Thunder III | Regel sperrt vorher | Thunder II |
| Thunder III | Regel sperrt vorher | Thunder IV |
| Thunder III | Regel sperrt vorher | Thunder |
| Thunder | Regel sperrt vorher | Thunder II |
| Thunder | Regel prüft | Thunder III |
| Thunder | Regel sperrt vorher | Thunder III |
| Thunder | Regel sperrt vorher | Thunder IV |
| Transpose | Regel prüft | Fire III |
| Transpose | Regel prüft | Paradox |
| Triplecast | Regel prüft | Paradox |
| Triplecast | Regel sperrt vorher | Paradox |
| Triplecast | Regel prüft | Swiftcast |
| Triplecast | Regel sperrt vorher | Swiftcast |
| Xenoglossy | Regel prüft | Amplifier |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `MeteorPvE`, `SkyshardPvE`, `StarstormPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Sleep (`SleepPvE`, Spell): ungenutzt

## Abgleich Wirktext ↔ Code

Der Wirktext nennt eine Bedingung, die Basisrotation führt dafür weder `StatusNeed` noch `ActionCheck`. RSR verlässt sich dann auf die Nutzbarkeitsauskunft des Spiels, die `BasicCheck` nur für eine feste Liste von Ablehnungscodes liest. Kandidaten, keine Befunde.

- Manafont (`ManafontPvE`): Astral Fire
