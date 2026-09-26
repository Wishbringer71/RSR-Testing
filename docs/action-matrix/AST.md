# AST — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `AstrologianRotation`
- Rotation: `RotationSolver/RebornRotations/Healer/AST_Reborn.cs`
- Matrix als Tabelle: `AST.csv`

## Nutzung

direkt: 49 · ungenutzt: 6

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Heiler | Esuna (`EsunaPvE`) | 7568 | Spell | direkt |
| Heiler | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Heiler | Repose (`ReposePvE`) | 16560 | Spell | ungenutzt |
| Heiler | Rescue (`RescuePvE`) | 7571 | Ability | ungenutzt |
| Heiler | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Heiler | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Ascend (`AscendPvE`) | 3603 | Spell | direkt |
| Job | Aspected Benefic (`AspectedBeneficPvE`) | 3595 | Spell | direkt |
| Job | Aspected Helios (`AspectedHeliosPvE`) | 3601 | Spell | direkt |
| Job | Astral Draw (`AstralDrawPvE`) | 37017 | Ability | direkt |
| Job | Benefic (`BeneficPvE`) | 3594 | Spell | direkt |
| Job | Benefic II (`BeneficIiPvE`) | 3610 | Spell | direkt |
| Job | Celestial Intersection (`CelestialIntersectionPvE`) | 16556 | Ability | direkt |
| Job | Celestial Opposition (`CelestialOppositionPvE`) | 16553 | Ability | direkt |
| Job | Collective Unconscious (`CollectiveUnconsciousPvE`) | 3613 | Ability | direkt |
| Job | Combust (`CombustPvE`) | 3599 | Spell | direkt |
| Job | Combust II (`CombustIiPvE`) | 3608 | Spell | direkt |
| Job | Combust III (`CombustIiiPvE`) | 16554 | Spell | direkt |
| Job | Divination (`DivinationPvE`) | 16552 | Ability | direkt |
| Job | Earthly Star (`EarthlyStarPvE`) | 7439 | Ability | direkt |
| Job | Essential Dignity (`EssentialDignityPvE`) | 3614 | Ability | direkt |
| Job | Exaltation (`ExaltationPvE`) | 25873 | Ability | direkt |
| Job | Fall Malefic (`FallMaleficPvE`) | 25871 | Spell | direkt |
| Job | Gravity (`GravityPvE`) | 3615 | Spell | direkt |
| Job | Gravity II (`GravityIiPvE`) | 25872 | Spell | direkt |
| Job | Helios (`HeliosPvE`) | 3600 | Spell | direkt |
| Job | Helios Conjunction (`HeliosConjunctionPvE`) | 37030 | Spell | direkt |
| Job | Horoscope (`HoroscopePvE`) | 16557 | Ability | direkt |
| Job | Horoscope (`HoroscopePvE_16558`) | 16558 | Ability | direkt |
| Job | Lady of Crowns (`LadyOfCrownsPvE`) | 7445 | Ability | direkt |
| Job | Lightspeed (`LightspeedPvE`) | 3606 | Ability | direkt |
| Job | Lord of Crowns (`LordOfCrownsPvE`) | 7444 | Ability | direkt |
| Job | Macrocosmos (`MacrocosmosPvE`) | 25874 | Spell | direkt |
| Job | Malefic (`MaleficPvE`) | 3596 | Spell | direkt |
| Job | Malefic II (`MaleficIiPvE`) | 3598 | Spell | direkt |
| Job | Malefic III (`MaleficIiiPvE`) | 7442 | Spell | direkt |
| Job | Malefic IV (`MaleficIvPvE`) | 16555 | Spell | direkt |
| Job | Microcosmos (`MicrocosmosPvE`) | 25875 | Ability | direkt |
| Job | Minor Arcana (`MinorArcanaPvE`) | 37022 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Neutral Sect (`NeutralSectPvE`) | 16559 | Ability | direkt |
| Job | Oracle (`OraclePvE`) | 37029 | Ability | direkt |
| Job | Play I (`PlayIPvE`) | 37019 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Play II (`PlayIiPvE`) | 37020 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Play III (`PlayIiiPvE`) | 37021 | Ability | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Stellar Detonation (`StellarDetonationPvE`) | 8324 | Ability | direkt |
| Job | Sun Sign (`SunSignPvE`) | 37031 | Ability | direkt |
| Job | Synastry (`SynastryPvE`) | 3612 | Ability | direkt |
| Job | Umbral Draw (`UmbralDrawPvE`) | 37018 | Ability | direkt |
| Job | the Arrow (`TheArrowPvE`) | 37024 | Ability | direkt |
| Job | the Balance (`TheBalancePvE`) | 37023 | Ability | direkt |
| Job | the Bole (`TheBolePvE`) | 37027 | Ability | direkt |
| Job | the Ewer (`TheEwerPvE`) | 37028 | Ability | direkt |
| Job | the Spear (`TheSpearPvE`) | 37026 | Ability | direkt |
| Job | the Spire (`TheSpirePvE`) | 37025 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Aspected Helios | Ausbau (Aspected Helios Mastery) | Helios Conjunction |
| Astral Draw | Knopf wird zu | Umbral Draw |
| Astral Draw | gemeinsame Abklingzeit | Umbral Draw |
| Combust II | Ausbau (Combust Mastery II) | Combust III |
| Combust | Ausbau (Combust Mastery) | Combust II |
| Gravity | Ausbau (Gravity Mastery) | Gravity II |
| Malefic II | Ausbau (Malefic Mastery II) | Malefic III |
| Malefic III | Ausbau (Malefic Mastery III) | Malefic IV |
| Malefic IV | Ausbau (Malefic Mastery IV) | Fall Malefic |
| Malefic | Ausbau (Malefic Mastery) | Malefic II |
| Minor Arcana | Knopf wird zu | Lady of Crowns |
| Minor Arcana | Knopf wird zu | Lord of Crowns |
| Oracle | braucht Divining | Eigenschaft Enhanced Divination |
| Play I | Knopf wird zu | the Balance |
| Play I | Knopf wird zu | the Spear |
| Play II | Knopf wird zu | the Arrow |
| Play II | Knopf wird zu | the Bole |
| Play III | Knopf wird zu | the Ewer |
| Play III | Knopf wird zu | the Spire |
| Sun Sign | braucht Suntouched | Eigenschaft Enhanced Neutral Sect |
| Umbral Draw | gemeinsame Abklingzeit | Astral Draw |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Aspected Benefic | Regel sperrt vorher | Collective Unconscious |
| Aspected Benefic | Regel sperrt vorher | Macrocosmos |
| Aspected Benefic | Regel prüft | Neutral Sect |
| Aspected Helios | Regel prüft | Helios Conjunction |
| Benefic II | Regel sperrt vorher | Aspected Benefic |
| Benefic | Regel sperrt vorher | Aspected Benefic |
| Celestial Intersection | Regel sperrt vorher | Essential Dignity |
| Collective Unconscious | Regel sperrt vorher | Macrocosmos |
| Combust II | Regel prüft | Combust III |
| Combust | Regel prüft | Combust II |
| Divination | Regel sperrt vorher | Aspected Benefic |
| Divination | Regel sperrt vorher | Benefic II |
| Divination | Regel sperrt vorher | Benefic |
| Divination | Regel sperrt vorher | Synastry |
| Gravity | Regel prüft | Gravity II |
| Helios Conjunction | Regel sperrt vorher | Collective Unconscious |
| Helios Conjunction | Regel sperrt vorher | Macrocosmos |
| Helios Conjunction | Regel prüft | Neutral Sect |
| Horoscope | Regel prüft | Helios Conjunction |
| Horoscope | Regel prüft | Helios |
| Horoscope | StatusNeed Horoscope | Horoscope |
| Horoscope | StatusNeed HoroscopeHelios | Status HoroscopeHelios |
| Lady of Crowns | Regel prüft | Astral Draw |
| Lightspeed | Regel prüft | Divination |
| Lord of Crowns | Regel prüft | Divination |
| Lord of Crowns | Regel prüft | Umbral Draw |
| Macrocosmos | Regel sperrt vorher | Collective Unconscious |
| Malefic II | Regel prüft | Malefic III |
| Malefic III | Regel prüft | Malefic IV |
| Malefic IV | Regel prüft | Fall Malefic |
| Malefic | Regel prüft | Malefic II |
| Microcosmos | StatusNeed Macrocosmos | Macrocosmos |
| Neutral Sect | Regel prüft | Aspected Benefic |
| Neutral Sect | Regel prüft | Aspected Helios |
| Neutral Sect | Regel sperrt vorher | Collective Unconscious |
| Neutral Sect | Regel prüft | Helios Conjunction |
| Neutral Sect | Regel sperrt vorher | Macrocosmos |
| Oracle | StatusNeed Divining | Divination |
| Stellar Detonation | Regel sperrt vorher | Aspected Benefic |
| Stellar Detonation | Regel sperrt vorher | Benefic II |
| Stellar Detonation | Regel sperrt vorher | Benefic |
| Stellar Detonation | StatusNeed GiantDominance | Earthly Star |
| Stellar Detonation | Regel sperrt vorher | Synastry |
| Sun Sign | StatusNeed Suntouched | Neutral Sect |
| the Arrow | Regel prüft | Umbral Draw |
| the Balance | Regel prüft | Divination |
| the Bole | Regel prüft | Astral Draw |
| the Ewer | Regel prüft | Astral Draw |
| the Spear | Regel prüft | Divination |
| the Spire | Regel prüft | Umbral Draw |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Collective Unconscious | Abwehr | endet bei jeder weiteren Aktion oder Bewegung (Kanal); Aktionssperre: AstlockCasting (aus) hält GCD, AstlockCasting (aus) hält Fähigkeit | jede Aktion |

### Verlängerung, Aufbau, Umschalten

keine

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| MP | Astral Draw, Umbral Draw | — |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Aspected Benefic (2), Aspected Helios (2), Combust III (1), Fall Malefic (1), Macrocosmos (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `AstralStasisPvE`, `BreathOfTheEarthPvE`, `HealingWindPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Minor Arcana (`MinorArcanaPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Play I (`PlayIPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Play II (`PlayIiPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Play III (`PlayIiiPvE`, Ability): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Repose (`ReposePvE`, Spell): ungenutzt
- Rescue (`RescuePvE`, Ability): ungenutzt
