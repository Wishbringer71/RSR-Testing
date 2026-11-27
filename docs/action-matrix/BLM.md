# BLM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-11-27; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `BlackMageRotation`
- Rotation: `RotationSolver/Rotations/Magical/BlackMage/BLM_Reborn.cs`
- Matrix als Tabelle: `BLM.csv`

## Nutzung

direkt: 33 · nur gelesen: 6 · ungenutzt: 1

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
| Job | High Blizzard II (`HighBlizzardIiPvE`) | 25795 | Spell | direkt |
| Job | High Fire II (`HighFireIiPvE`) | 25794 | Spell | direkt |
| Job | High Thunder (`HighThunderPvE`) | 36986 | Spell | nur gelesen |
| Job | High Thunder II (`HighThunderIiPvE`) | 36987 | Spell | nur gelesen |
| Job | Ley Lines (`LeyLinesPvE`) | 3573 | Ability | direkt |
| Job | Manafont (`ManafontPvE`) | 158 | Ability | direkt |
| Job | Manaward (`ManawardPvE`) | 157 | Ability | direkt |
| Job | Paradox (`ParadoxPvE`) | 25797 | Spell | direkt |
| Job | Retrace (`RetracePvE`) | 36988 | Ability | direkt |
| Job | Scathe (`ScathePvE`) | 156 | Spell | direkt |
| Job | Thunder (`ThunderPvE`) | 144 | Spell | nur gelesen |
| Job | Thunder II (`ThunderIiPvE`) | 7447 | Spell | nur gelesen |
| Job | Thunder III (`ThunderIiiPvE`) | 153 | Spell | nur gelesen |
| Job | Thunder IV (`ThunderIvPvE`) | 7420 | Spell | nur gelesen |
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
| Retrace | braucht Ley Lines, and the effect duration will not be reset | Ley Lines |
| Thunder II | Ausbau (Thunder Mastery II) | Thunder IV |
| Thunder III | Ausbau (Thunder Mastery III) | High Thunder |
| Thunder IV | Ausbau (Thunder Mastery III) | High Thunder II |
| Thunder | Ausbau (Thunder Mastery) | Thunder III |
| Umbral Soul | braucht Umbral Ice | Blizzard |
| Xenoglossy | kostet | Polyglot |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Blizzard II | Regel sperrt vorher | Blizzard III |
| Blizzard II | Regel sperrt vorher | Fire II |
| Blizzard II | Regel sperrt vorher | Flare |
| Blizzard II | Regel prüft | Freeze |
| Blizzard II | Regel sperrt vorher | Freeze |
| Blizzard III | Regel prüft | Fire III |
| Blizzard III | Regel prüft | Flare |
| Despair | Regel prüft | Fire IV |
| Despair | Regel prüft | Paradox |
| Fire III | Regel prüft | Blizzard IV |
| Fire III | Regel prüft | Fire |
| Fire IV | Regel prüft | Paradox |
| Fire | Regel prüft | Fire III |
| Fire | Regel sperrt vorher | Fire III |
| Foul | Regel prüft | Xenoglossy |
| Lucid Dreaming | Regel prüft | Blizzard III |
| Retrace | Regel prüft | Ley Lines |
| Retrace | StatusNeed LeyLines | Ley Lines |
| Swiftcast | Regel prüft | Blizzard III |
| Swiftcast | Regel prüft | Transpose |
| Transpose | Regel prüft | Blizzard III |
| Transpose | Regel sperrt vorher | Blizzard III |
| Transpose | Regel prüft | Fire II |
| Transpose | Regel sperrt vorher | Fire II |
| Transpose | Regel prüft | Fire |
| Transpose | Regel prüft | Flare |
| Transpose | Regel sperrt vorher | Flare |
| Transpose | Regel prüft | Freeze |
| Transpose | Regel sperrt vorher | Freeze |
| Transpose | Regel prüft | Umbral Soul |
| Triplecast | Regel prüft | Blizzard III |
| Triplecast | Regel prüft | Transpose |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

keine

### Verlängerung, Aufbau, Umschalten

keine

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| MP | Umbral Soul | — |
| Polyglot | — (nicht im Wirktext) | Foul, Xenoglossy |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Fenster, deren Verbraucher an Bedingungen hängt

Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster (Konzept 14, „Werden die Fenster genutzt"). Kandidaten, von Hand bewertet.

- Retrace: 2 Aufruf(e), **ohne Rückfall vor Ablauf**

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Addle (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `MeteorPvE`, `SkyshardPvE`, `StarstormPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- High Thunder (`HighThunderPvE`, Spell): nur gelesen
- High Thunder II (`HighThunderIiPvE`, Spell): nur gelesen
- Sleep (`SleepPvE`, Spell): ungenutzt
- Thunder (`ThunderPvE`, Spell): nur gelesen
- Thunder II (`ThunderIiPvE`, Spell): nur gelesen
- Thunder III (`ThunderIiiPvE`, Spell): nur gelesen
- Thunder IV (`ThunderIvPvE`, Spell): nur gelesen

## Abgleich Wirktext ↔ Code

Der Wirktext nennt eine Bedingung, die Basisrotation führt dafür weder `StatusNeed` noch `ActionCheck`. RSR verlässt sich dann auf die Nutzbarkeitsauskunft des Spiels, die `BasicCheck` nur für eine feste Liste von Ablehnungscodes liest. Kandidaten, keine Befunde.

- Manafont (`ManafontPvE`): Astral Fire
