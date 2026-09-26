# SMN — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `SummonerRotation`
- Rotation: `RotationSolver/RebornRotations/Magical/SMN_Reborn.cs`
- Matrix als Tabelle: `SMN.csv`

## Nutzung

direkt: 72 · nur gelesen: 1 · ungenutzt: 10 · über andere Aktion: 3

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Magier | Addle (`AddlePvE`) | 7560 | Ability | direkt |
| Magier | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Magier | Sleep (`SleepPvE`) | 25880 | Spell | ungenutzt |
| Magier | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Magier | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Aethercharge (`AetherchargePvE`) | 25800 | Spell | direkt |
| Job | Akh Morn (`AkhMornPvE`) | 7449 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Astral Flare (`AstralFlarePvE`) | 25821 | Spell | direkt |
| Job | Astral Flow (`AstralFlowPvE`) | 25822 | Spell | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Astral Impulse (`AstralImpulsePvE`) | 25820 | Spell | direkt |
| Job | Brand of Purgatory (`BrandOfPurgatoryPvE`) | 16515 | Spell | direkt |
| Job | Crimson Cyclone (`CrimsonCyclonePvE`) | 25835 | Spell | direkt |
| Job | Crimson Strike (`CrimsonStrikePvE`) | 25885 | Spell | direkt |
| Job | Deathflare (`DeathflarePvE`) | 3582 | Ability | direkt |
| Job | Dreadwyrm Trance (`DreadwyrmTrancePvE`) | 3581 | Spell | direkt |
| Job | Emerald Catastrophe (`EmeraldCatastrophePvE`) | 25834 | Spell | direkt |
| Job | Emerald Disaster (`EmeraldDisasterPvE`) | 25829 | Spell | direkt |
| Job | Emerald Outburst (`EmeraldOutburstPvE`) | 25816 | Spell | direkt |
| Job | Emerald Rite (`EmeraldRitePvE`) | 25825 | Spell | direkt |
| Job | Emerald Ruin (`EmeraldRuinPvE`) | 25810 | Spell | direkt |
| Job | Emerald Ruin II (`EmeraldRuinIiPvE`) | 25813 | Spell | direkt |
| Job | Emerald Ruin III (`EmeraldRuinIiiPvE`) | 25819 | Spell | direkt |
| Job | Energy Drain (`EnergyDrainPvE`) | 167 | Ability | direkt |
| Job | Energy Siphon (`EnergySiphonPvE`) | 16510 | Ability | direkt |
| Job | Enkindle Bahamut (`EnkindleBahamutPvE`) | 7429 | Ability | direkt |
| Job | Enkindle Phoenix (`EnkindlePhoenixPvE`) | 16516 | Ability | direkt |
| Job | Enkindle Solar Bahamut (`EnkindleSolarBahamutPvE`) | 36998 | Ability | direkt |
| Job | Everlasting Flight (`EverlastingFlightPvE`) | 16517 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Exodus (`ExodusPvE`) | 36999 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Fester (`FesterPvE`) | 181 | Ability | direkt |
| Job | Fountain of Fire (`FountainOfFirePvE`) | 16514 | Spell | direkt |
| Job | Gemshine (`GemshinePvE`) | 25883 | Spell | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Lux Solaris (`LuxSolarisPvE`) | 36997 | Ability | direkt |
| Job | Luxwave (`LuxwavePvE`) | 36993 | Spell | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Mountain Buster (`MountainBusterPvE`) | 11428 | Ability | direkt |
| Job | Necrotize (`NecrotizePvE`) | 36990 | Ability | direkt |
| Job | Outburst (`OutburstPvE`) | 16511 | Spell | direkt |
| Job | Painflare (`PainflarePvE`) | 3578 | Ability | direkt |
| Job | Physick (`PhysickPvE`) | 190 | Spell | direkt |
| Job | Precious Brilliance (`PreciousBrilliancePvE`) | 25884 | Spell | ungenutzt — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Radiant Aegis (`RadiantAegisPvE`) | 25799 | Ability | direkt |
| Job | Radiant Aegis (`RadiantAegisPvE_25841`) | 25841 | Ability | über gleichnamige Aktion `RadiantAegisPvE` |
| Job | Rekindle (`RekindlePvE`) | 25830 | Ability | direkt |
| Job | Resurrection (`ResurrectionPvE`) | 173 | Spell | direkt |
| Job | Revelation (`RevelationPvE`) | 16518 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Ruby Catastrophe (`RubyCatastrophePvE`) | 25832 | Spell | direkt |
| Job | Ruby Disaster (`RubyDisasterPvE`) | 25827 | Spell | direkt |
| Job | Ruby Outburst (`RubyOutburstPvE`) | 25814 | Spell | direkt |
| Job | Ruby Rite (`RubyRitePvE`) | 25823 | Spell | direkt |
| Job | Ruby Ruin (`RubyRuinPvE`) | 25808 | Spell | direkt |
| Job | Ruby Ruin II (`RubyRuinIiPvE`) | 25811 | Spell | direkt |
| Job | Ruby Ruin III (`RubyRuinIiiPvE`) | 25817 | Spell | direkt |
| Job | Ruin (`RuinPvE`) | 163 | Spell | direkt |
| Job | Ruin II (`RuinIiPvE`) | 172 | Spell | direkt |
| Job | Ruin III (`RuinIiiPvE`) | 3579 | Spell | direkt |
| Job | Ruin IV (`RuinIvPvE`) | 7426 | Spell | direkt |
| Job | Scarlet Flame (`ScarletFlamePvE`) | 16519 | Spell | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Searing Flash (`SearingFlashPvE`) | 36991 | Ability | direkt |
| Job | Searing Light (`SearingLightPvE`) | 25801 | Ability | direkt |
| Job | Slipstream (`SlipstreamPvE`) | 25837 | Spell | direkt |
| Job | Summon Bahamut (`SummonBahamutPvE`) | 7427 | Spell | direkt |
| Job | Summon Carbuncle (`SummonCarbunclePvE`) | 25798 | Spell | direkt |
| Job | Summon Emerald (`SummonEmeraldPvE`) | 25804 | Spell | direkt |
| Job | Summon Garuda (`SummonGarudaPvE`) | 25807 | Spell | direkt |
| Job | Summon Garuda II (`SummonGarudaIiPvE`) | 25840 | Spell | direkt |
| Job | Summon Ifrit (`SummonIfritPvE`) | 25805 | Spell | direkt |
| Job | Summon Ifrit II (`SummonIfritIiPvE`) | 25838 | Spell | direkt |
| Job | Summon Phoenix (`SummonPhoenixPvE`) | 25831 | Spell | über Summon Bahamut |
| Job | Summon Ruby (`SummonRubyPvE`) | 25802 | Spell | direkt |
| Job | Summon Solar Bahamut (`SummonSolarBahamutPvE`) | 36992 | Spell | direkt |
| Job | Summon Titan (`SummonTitanPvE`) | 25806 | Spell | direkt |
| Job | Summon Titan II (`SummonTitanIiPvE`) | 25839 | Spell | direkt |
| Job | Summon Topaz (`SummonTopazPvE`) | 25803 | Spell | direkt |
| Job | Sunflare (`SunflarePvE`) | 36996 | Ability | direkt |
| Job | Topaz Catastrophe (`TopazCatastrophePvE`) | 25833 | Spell | direkt |
| Job | Topaz Disaster (`TopazDisasterPvE`) | 25828 | Spell | direkt |
| Job | Topaz Outburst (`TopazOutburstPvE`) | 25815 | Spell | direkt |
| Job | Topaz Rite (`TopazRitePvE`) | 25824 | Spell | direkt |
| Job | Topaz Ruin (`TopazRuinPvE`) | 25809 | Spell | direkt |
| Job | Topaz Ruin II (`TopazRuinIiPvE`) | 25812 | Spell | direkt |
| Job | Topaz Ruin III (`TopazRuinIiiPvE`) | 25818 | Spell | direkt |
| Job | Tri-disaster (`TridisasterPvE`) | 25826 | Spell | über Outburst |
| Job | Umbral Flare (`UmbralFlarePvE`) | 36995 | Spell | direkt |
| Job | Umbral Impulse (`UmbralImpulsePvE`) | 36994 | Spell | direkt |
| Job | Wyrmwave (`WyrmwavePvE`) | 7428 | Spell | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Aethercharge | Ausbau (Aethercharge Mastery) | Dreadwyrm Trance |
| Akh Morn | braucht Demi-Bahamut is summoned (Erzeuger nicht im Text) | Demi-Bahamut is summoned |
| Astral Flare | braucht Dreadwyrm Trance | Dreadwyrm Trance |
| Astral Flow | Knopf wird zu | Crimson Cyclone |
| Astral Flow | Knopf wird zu | Deathflare |
| Astral Flow | Knopf wird zu | Rekindle |
| Astral Flow | Knopf wird zu | Slipstream |
| Astral Flow | Knopf wird zu | Sunflare |
| Astral Impulse | braucht Dreadwyrm Trance | Dreadwyrm Trance |
| Brand of Purgatory | braucht Firebird Trance (Erzeuger nicht im Text) | Firebird Trance |
| Crimson Cyclone | Knopf wird zu | Crimson Strike |
| Crimson Cyclone | braucht Ifrit's Favor | Eigenschaft Elemental Mastery |
| Crimson Cyclone | braucht Ifrit's Favor | Summon Ifrit II |
| Crimson Strike | braucht Crimson Strike Ready | Crimson Cyclone |
| Deathflare | braucht Dreadwyrm Trance | Dreadwyrm Trance |
| Dreadwyrm Trance | Ausbau (Enhanced Dreadwyrm Trance) | Summon Bahamut |
| Emerald Catastrophe | kostet | Attunement |
| Emerald Disaster | kostet | Attunement |
| Emerald Outburst | kostet | Attunement |
| Emerald Rite | kostet | Attunement |
| Emerald Ruin II | kostet | Attunement |
| Emerald Ruin III | kostet | Attunement |
| Emerald Ruin | kostet | Attunement |
| Energy Drain | kostet | Aetherflow |
| Energy Siphon | gemeinsame Abklingzeit | Energy Drain |
| Enkindle Bahamut | Knopf wird zu | Enkindle Phoenix |
| Enkindle Bahamut | Knopf wird zu | Enkindle Solar Bahamut |
| Exodus | braucht Solar Bahamut is summoned (Erzeuger nicht im Text) | Solar Bahamut is summoned |
| Fester | kostet | Aetherflow |
| Fester | Ausbau (Enhanced Fester) | Necrotize |
| Fountain of Fire | braucht Firebird Trance (Erzeuger nicht im Text) | Firebird Trance |
| Gemshine | Knopf wird zu | Emerald Rite |
| Gemshine | Knopf wird zu | Ruby Rite |
| Gemshine | Knopf wird zu | Topaz Rite |
| Lux Solaris | braucht Refulgent Lux | Summon Solar Bahamut |
| Necrotize | kostet | Aetherflow |
| Outburst | Ausbau (Outburst Mastery) | Tri-disaster |
| Painflare | kostet | Aetherflow |
| Precious Brilliance | Knopf wird zu | Emerald Catastrophe |
| Precious Brilliance | Knopf wird zu | Ruby Catastrophe |
| Precious Brilliance | Knopf wird zu | Topaz Catastrophe |
| Radiant Aegis | braucht Carbuncle is summoned (Erzeuger nicht im Text) | Carbuncle is summoned |
| Rekindle | braucht Firebird Trance (Erzeuger nicht im Text) | Firebird Trance |
| Revelation | braucht Demi-Phoenix is summoned (Erzeuger nicht im Text) | Demi-Phoenix is summoned |
| Ruby Catastrophe | kostet | Attunement |
| Ruby Disaster | kostet | Attunement |
| Ruby Outburst | kostet | Attunement |
| Ruby Rite | kostet | Attunement |
| Ruby Ruin II | kostet | Attunement |
| Ruby Ruin III | kostet | Attunement |
| Ruby Ruin | kostet | Attunement |
| Ruin II | Ausbau (Ruin Mastery II) | Ruin III |
| Ruin III | Knopf wird zu | Astral Impulse |
| Ruin III | Knopf wird zu | Fountain of Fire |
| Ruin III | Knopf wird zu | Umbral Impulse |
| Ruin IV | braucht Further Ruin | Eigenschaft Enhanced Energy Siphon |
| Searing Flash | braucht Ruby's Glimmer | Eigenschaft Enhanced Searing Light |
| Slipstream | braucht Garuda's Favor | Summon Garuda II |
| Summon Bahamut | braucht Carbuncle is summoned (Erzeuger nicht im Text) | Carbuncle is summoned |
| Summon Bahamut | Ausbau (Enhanced Summon Bahamut) | Summon Phoenix |
| Summon Bahamut | Knopf wird zu | Summon Phoenix |
| Summon Bahamut | Knopf wird zu | Summon Solar Bahamut |
| Summon Emerald | braucht Emerald Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Emerald Arcanum and Carbuncle is summoned |
| Summon Emerald | Ausbau (Emerald Summoning Mastery) | Summon Garuda |
| Summon Garuda II | braucht Emerald Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Emerald Arcanum and Carbuncle is summoned |
| Summon Garuda | braucht Emerald Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Emerald Arcanum and Carbuncle is summoned |
| Summon Garuda | Ausbau (Enkindle II) | Summon Garuda II |
| Summon Ifrit II | braucht Ruby Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Ruby Arcanum and Carbuncle is summoned |
| Summon Ifrit | braucht Ruby Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Ruby Arcanum and Carbuncle is summoned |
| Summon Ifrit | Ausbau (Enkindle II) | Summon Ifrit II |
| Summon Phoenix | braucht Carbuncle is summoned (Erzeuger nicht im Text) | Carbuncle is summoned |
| Summon Ruby | braucht Ruby Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Ruby Arcanum and Carbuncle is summoned |
| Summon Ruby | Ausbau (Ruby Summoning Mastery) | Summon Ifrit |
| Summon Solar Bahamut | braucht Carbuncle is summoned (Erzeuger nicht im Text) | Carbuncle is summoned |
| Summon Titan II | braucht Topaz Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Topaz Arcanum and Carbuncle is summoned |
| Summon Titan | Ausbau (Enkindle II) | Summon Titan II |
| Summon Titan | braucht Topaz Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Topaz Arcanum and Carbuncle is summoned |
| Summon Topaz | Ausbau (Topaz Summoning Mastery) | Summon Titan |
| Summon Topaz | braucht Topaz Arcanum and Carbuncle is summoned (Erzeuger nicht im Text) | Topaz Arcanum and Carbuncle is summoned |
| Sunflare | braucht Lightwyrm Trance (Erzeuger nicht im Text) | Lightwyrm Trance |
| Topaz Catastrophe | kostet | Attunement |
| Topaz Disaster | kostet | Attunement |
| Topaz Outburst | kostet | Attunement |
| Topaz Rite | kostet | Attunement |
| Topaz Ruin II | kostet | Attunement |
| Topaz Ruin III | kostet | Attunement |
| Topaz Ruin | kostet | Attunement |
| Tri-disaster | Knopf wird zu | Brand of Purgatory |
| Tri-disaster | Knopf wird zu | Umbral Flare |
| Umbral Flare | braucht Lightwyrm Trance (Erzeuger nicht im Text) | Lightwyrm Trance |
| Umbral Impulse | braucht Lightwyrm Trance (Erzeuger nicht im Text) | Lightwyrm Trance |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Aethercharge | Regel prüft | Dreadwyrm Trance |
| Crimson Cyclone | StatusNeed IfritsFavor | Summon Ruby |
| Crimson Strike | StatusNeed CrimsonStrikeReady_4403 | Crimson Cyclone |
| Deathflare | Regel sperrt vorher | Energy Drain |
| Deathflare | Regel sperrt vorher | Energy Siphon |
| Deathflare | Regel sperrt vorher | Enkindle Bahamut |
| Deathflare | Regel sperrt vorher | Enkindle Phoenix |
| Deathflare | Regel sperrt vorher | Enkindle Solar Bahamut |
| Deathflare | Regel sperrt vorher | Summon Bahamut |
| Dreadwyrm Trance | Regel prüft | Summon Bahamut |
| Emerald Ruin | Regel prüft | Summon Garuda |
| Energy Drain | Regel sperrt vorher | Energy Siphon |
| Energy Drain | Regel sperrt vorher | Summon Bahamut |
| Enkindle Bahamut | Regel sperrt vorher | Energy Drain |
| Enkindle Bahamut | Regel sperrt vorher | Energy Siphon |
| Enkindle Bahamut | Regel sperrt vorher | Summon Bahamut |
| Enkindle Phoenix | Regel sperrt vorher | Energy Drain |
| Enkindle Phoenix | Regel sperrt vorher | Energy Siphon |
| Enkindle Phoenix | Regel sperrt vorher | Enkindle Bahamut |
| Enkindle Phoenix | Regel sperrt vorher | Enkindle Solar Bahamut |
| Enkindle Phoenix | Regel sperrt vorher | Summon Bahamut |
| Enkindle Solar Bahamut | Regel sperrt vorher | Energy Drain |
| Enkindle Solar Bahamut | Regel sperrt vorher | Energy Siphon |
| Enkindle Solar Bahamut | Regel sperrt vorher | Enkindle Bahamut |
| Enkindle Solar Bahamut | Regel sperrt vorher | Summon Bahamut |
| Fester | Regel sperrt vorher | Deathflare |
| Fester | Regel sperrt vorher | Energy Drain |
| Fester | Regel sperrt vorher | Energy Siphon |
| Fester | Regel sperrt vorher | Enkindle Bahamut |
| Fester | Regel sperrt vorher | Enkindle Phoenix |
| Fester | Regel sperrt vorher | Enkindle Solar Bahamut |
| Fester | Regel sperrt vorher | Necrotize |
| Fester | Regel sperrt vorher | Painflare |
| Fester | Regel sperrt vorher | Searing Flash |
| Fester | Regel sperrt vorher | Searing Light |
| Fester | Regel sperrt vorher | Summon Bahamut |
| Fester | Regel sperrt vorher | Sunflare |
| Lux Solaris | StatusNeed RefulgentLux | Status RefulgentLux |
| Mountain Buster | Regel sperrt vorher | Deathflare |
| Mountain Buster | Regel sperrt vorher | Energy Drain |
| Mountain Buster | Regel sperrt vorher | Energy Siphon |
| Mountain Buster | Regel sperrt vorher | Enkindle Bahamut |
| Mountain Buster | Regel sperrt vorher | Enkindle Phoenix |
| Mountain Buster | Regel sperrt vorher | Enkindle Solar Bahamut |
| Mountain Buster | Regel sperrt vorher | Searing Flash |
| Mountain Buster | Regel sperrt vorher | Summon Bahamut |
| Mountain Buster | Regel sperrt vorher | Sunflare |
| Necrotize | Regel sperrt vorher | Deathflare |
| Necrotize | Regel sperrt vorher | Energy Drain |
| Necrotize | Regel sperrt vorher | Energy Siphon |
| Necrotize | Regel sperrt vorher | Enkindle Bahamut |
| Necrotize | Regel sperrt vorher | Enkindle Phoenix |
| Necrotize | Regel sperrt vorher | Enkindle Solar Bahamut |
| Necrotize | Regel sperrt vorher | Painflare |
| Necrotize | Regel sperrt vorher | Searing Flash |
| Necrotize | Regel sperrt vorher | Searing Light |
| Necrotize | Regel sperrt vorher | Summon Bahamut |
| Necrotize | Regel sperrt vorher | Sunflare |
| Painflare | Regel sperrt vorher | Deathflare |
| Painflare | Regel sperrt vorher | Energy Drain |
| Painflare | Regel sperrt vorher | Energy Siphon |
| Painflare | Regel sperrt vorher | Enkindle Bahamut |
| Painflare | Regel sperrt vorher | Enkindle Phoenix |
| Painflare | Regel sperrt vorher | Enkindle Solar Bahamut |
| Painflare | Regel sperrt vorher | Searing Flash |
| Painflare | Regel sperrt vorher | Summon Bahamut |
| Painflare | Regel sperrt vorher | Sunflare |
| Ruby Ruin | Regel prüft | Summon Ifrit |
| Ruin II | Regel prüft | Ruin III |
| Ruin IV | StatusNeed FurtherRuin_2701 | Status FurtherRuin_2701 |
| Ruin | Regel prüft | Ruin II |
| Searing Flash | Regel sperrt vorher | Deathflare |
| Searing Flash | Regel sperrt vorher | Energy Drain |
| Searing Flash | Regel sperrt vorher | Energy Siphon |
| Searing Flash | Regel sperrt vorher | Enkindle Bahamut |
| Searing Flash | Regel sperrt vorher | Enkindle Phoenix |
| Searing Flash | Regel sperrt vorher | Enkindle Solar Bahamut |
| Searing Flash | Regel sperrt vorher | Fester |
| Searing Flash | Regel sperrt vorher | Necrotize |
| Searing Flash | Regel sperrt vorher | Painflare |
| Searing Flash | Regel sperrt vorher | Searing Light |
| Searing Flash | StatusNeed RubysGlimmer | Status RubysGlimmer |
| Searing Flash | Regel sperrt vorher | Summon Bahamut |
| Searing Flash | Regel sperrt vorher | Sunflare |
| Slipstream | StatusNeed GarudasFavor | Summon Emerald |
| Slipstream | Regel prüft | Swiftcast |
| Summon Emerald | Regel prüft | Summon Garuda |
| Summon Garuda | Regel prüft | Summon Garuda II |
| Summon Ifrit | Regel prüft | Summon Ifrit II |
| Summon Ruby | Regel prüft | Summon Ifrit |
| Summon Titan | Regel prüft | Summon Titan II |
| Summon Topaz | Regel prüft | Summon Titan |
| Sunflare | Regel sperrt vorher | Deathflare |
| Sunflare | Regel sperrt vorher | Energy Drain |
| Sunflare | Regel sperrt vorher | Energy Siphon |
| Sunflare | Regel sperrt vorher | Enkindle Bahamut |
| Sunflare | Regel sperrt vorher | Enkindle Phoenix |
| Sunflare | Regel sperrt vorher | Enkindle Solar Bahamut |
| Sunflare | Regel sperrt vorher | Summon Bahamut |
| Topaz Ruin | Regel prüft | Summon Titan |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

keine

### Verlängerung, Aufbau, Umschalten

- Summon Emerald: 4 Stapel Wind Attunement
- Summon Garuda: 4 Stapel Wind Attunement
- Summon Garuda II: 4 Stapel Wind Attunement
- Summon Ifrit: 2 Stapel Fire Attunement
- Summon Ifrit II: 2 Stapel Fire Attunement
- Summon Ruby: 2 Stapel Fire Attunement
- Summon Titan: 4 Stapel Earth Attunement
- Summon Titan II: 4 Stapel Earth Attunement
- Summon Topaz: 4 Stapel Earth Attunement

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Aetherflow | — (nicht im Wirktext) | Energy Drain, Fester, Necrotize, Painflare |
| Attunement | — (nicht im Wirktext) | Emerald Catastrophe, Emerald Disaster, Emerald Outburst, Emerald Rite, Emerald Ruin, Emerald Ruin II, Emerald Ruin III, Ruby Catastrophe, Ruby Disaster, Ruby Outburst, Ruby Rite, Ruby Ruin, Ruby Ruin II, Ruby Ruin III, Topaz Catastrophe, Topaz Disaster, Topaz Outburst, Topaz Rite, Topaz Ruin, Topaz Ruin II, Topaz Ruin III |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Fenster, deren Verbraucher an Bedingungen hängt

Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster (Konzept 14, „Werden die Fenster genutzt"). Kandidaten, von Hand bewertet.

- Lux Solaris: 1 Aufruf(e), **ohne Rückfall vor Ablauf**
- Ruin IV: 1 Aufruf(e), **ohne Rückfall vor Ablauf**
- Searing Flash: 2 Aufruf(e), **ohne Rückfall vor Ablauf**
- Slipstream: 1 Aufruf(e), **ohne Rückfall vor Ablauf**

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Addle (1), Aethercharge (1), Astral Impulse (1), Crimson Cyclone (1), Crimson Strike (1), Emerald Rite (1), Fountain of Fire (1), Ruby Rite (1), Ruin III (1), Ruin IV (1), Slipstream (1), Summon Garuda II (1), Summon Ifrit II (1), Summon Titan II (1), Topaz Rite (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `SkyshardPvE`, `StarstormPvE`, `TeraflarePvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Akh Morn (`AkhMornPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Astral Flow (`AstralFlowPvE`, Spell): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Everlasting Flight (`EverlastingFlightPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Exodus (`ExodusPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Gemshine (`GemshinePvE`, Spell): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Luxwave (`LuxwavePvE`, Spell): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Precious Brilliance (`PreciousBrilliancePvE`, Spell): ungenutzt — Behälter: der Knopf wird zu anderen Aktionen
- Revelation (`RevelationPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Scarlet Flame (`ScarletFlamePvE`, Spell): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Sleep (`SleepPvE`, Spell): ungenutzt
- Wyrmwave (`WyrmwavePvE`, Spell): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
