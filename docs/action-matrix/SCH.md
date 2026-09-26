# SCH — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `ScholarRotation`
- Rotation: `RotationSolver/RebornRotations/Healer/SCH_Reborn.cs`
- Matrix als Tabelle: `SCH.csv`

## Nutzung

direkt: 44 · ungenutzt: 5 · über andere Aktion: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Heiler | Esuna (`EsunaPvE`) | 7568 | Spell | direkt |
| Heiler | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Heiler | Repose (`ReposePvE`) | 16560 | Spell | ungenutzt |
| Heiler | Rescue (`RescuePvE`) | 7571 | Ability | ungenutzt |
| Heiler | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Heiler | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Accession (`AccessionPvE`) | 37016 | Spell | direkt |
| Job | Adloquium (`AdloquiumPvE`) | 185 | Spell | direkt |
| Job | Aetherflow (`AetherflowPvE`) | 166 | Ability | direkt |
| Job | Aetherpact (`AetherpactPvE`) | 7437 | Ability | direkt |
| Job | Art of War (`ArtOfWarPvE`) | 16539 | Spell | direkt |
| Job | Art of War II (`ArtOfWarIiPvE`) | 25866 | Spell | über Art of War |
| Job | Baneful Impaction (`BanefulImpactionPvE`) | 37012 | Ability | direkt |
| Job | Bio (`BioPvE`) | 17864 | Spell | direkt |
| Job | Bio II (`BioIiPvE`) | 17865 | Spell | direkt |
| Job | Biolysis (`BiolysisPvE`) | 16540 | Spell | direkt |
| Job | Broil (`BroilPvE`) | 3584 | Spell | direkt |
| Job | Broil II (`BroilIiPvE`) | 7435 | Spell | direkt |
| Job | Broil III (`BroilIiiPvE`) | 16541 | Spell | direkt |
| Job | Broil IV (`BroilIvPvE`) | 25865 | Spell | direkt |
| Job | Chain Stratagem (`ChainStratagemPvE`) | 7436 | Ability | direkt |
| Job | Concitation (`ConcitationPvE`) | 37013 | Spell | direkt |
| Job | Consolation (`ConsolationPvE`) | 16546 | Ability | direkt |
| Job | Deployment Tactics (`DeploymentTacticsPvE`) | 3585 | Ability | direkt |
| Job | Dissipation (`DissipationPvE`) | 3587 | Ability | direkt |
| Job | Dissolve Union (`DissolveUnionPvE`) | 7869 | Ability | ungenutzt |
| Job | Embrace (`EmbracePvE`) | 802 | Spell | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Emergency Tactics (`EmergencyTacticsPvE`) | 3586 | Ability | direkt |
| Job | Emergency Tactics (`EmergencyTacticsPvE_37037`) | 37037 | Ability | über gleichnamige Aktion `EmergencyTacticsPvE` |
| Job | Energy Drain (`EnergyDrainPvE`) | 167 | Ability | direkt |
| Job | Excogitation (`ExcogitationPvE`) | 7434 | Ability | direkt |
| Job | Expedient (`ExpedientPvE`) | 25868 | Ability | direkt |
| Job | Fey Blessing (`FeyBlessingPvE`) | 16543 | Ability | direkt |
| Job | Fey Illumination (`FeyIlluminationPvE`) | 16538 | Ability | direkt |
| Job | Indomitability (`IndomitabilityPvE`) | 3583 | Ability | direkt |
| Job | Lustrate (`LustratePvE`) | 189 | Ability | direkt |
| Job | Manifestation (`ManifestationPvE`) | 37015 | Spell | direkt |
| Job | Physick (`PhysickPvE`) | 190 | Spell | direkt |
| Job | Protraction (`ProtractionPvE`) | 25867 | Ability | direkt |
| Job | Recitation (`RecitationPvE`) | 16542 | Ability | direkt |
| Job | Resurrection (`ResurrectionPvE`) | 173 | Spell | direkt |
| Job | Ruin (`RuinPvE`) | 163 | Spell | direkt |
| Job | Ruin II (`RuinIiPvE`) | 172 | Spell | direkt |
| Job | Sacred Soil (`SacredSoilPvE`) | 188 | Ability | direkt |
| Job | Seraphic Veil (`SeraphicVeilPvE`) | 16548 | Spell | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Seraphism (`SeraphismPvE`) | 37014 | Ability | direkt |
| Job | Succor (`SuccorPvE`) | 186 | Spell | direkt |
| Job | Summon Eos (`SummonEosPvE`) | 17215 | Spell | direkt |
| Job | Summon Seraph (`SummonSeraphPvE`) | 16545 | Ability | direkt |
| Job | Whispering Dawn (`WhisperingDawnPvE`) | 16537 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Accession | braucht Seraphism | Seraphism |
| Adloquium | Knopf wird zu | Manifestation |
| Aetherflow | Bedingung (kein Status) | combat |
| Aetherpact | kostet | Faerie |
| Art of War | Ausbau (Art of War Mastery) | Art of War II |
| Baneful Impaction | braucht Impact Imminent | Eigenschaft Enhanced Chain Stratagem |
| Bio II | Ausbau (Corruption Mastery II) | Biolysis |
| Bio | Ausbau (Corruption Mastery) | Bio II |
| Broil II | Ausbau (Broil Mastery III) | Broil III |
| Broil III | Ausbau (Broil Mastery IV) | Broil IV |
| Broil | Ausbau (Broil Mastery II) | Broil II |
| Concitation | Knopf wird zu | Accession |
| Dissipation | Bedingung (kein Status) | a faerie is summoned and you are in combat |
| Energy Drain | kostet | Aetherflow |
| Excogitation | kostet | Aetherflow |
| Indomitability | kostet | Aetherflow |
| Lustrate | kostet | Aetherflow |
| Manifestation | braucht Seraphism | Seraphism |
| Ruin | Ausbau (Broil Mastery) | Broil |
| Sacred Soil | kostet | Aetherflow |
| Seraphism | Bedingung (kein Status) | a faerie or Seraph is summoned and you are in combat |
| Succor | Ausbau (Succor Mastery) | Concitation |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Aetherpact | Regel sperrt vorher | Excogitation |
| Art of War | Regel prüft | Broil III |
| Art of War | Regel prüft | Broil IV |
| Baneful Impaction | StatusNeed ImpactImminent | Status ImpactImminent |
| Bio II | Regel prüft | Art of War |
| Bio II | Regel prüft | Broil III |
| Bio | Regel prüft | Bio II |
| Biolysis | Regel prüft | Broil IV |
| Broil II | Regel prüft | Broil III |
| Broil III | Regel prüft | Broil IV |
| Deployment Tactics | Regel prüft | Recitation |
| Dissipation | Regel prüft | Aetherflow |
| Dissipation | Regel prüft | Fey Blessing |
| Dissipation | Regel prüft | Summon Seraph |
| Dissipation | Regel prüft | Whispering Dawn |
| Emergency Tactics | Regel prüft | Accession |
| Emergency Tactics | Regel prüft | Concitation |
| Emergency Tactics | Regel prüft | Succor |
| Energy Drain | Regel prüft | Aetherflow |
| Energy Drain | Regel prüft | Dissipation |
| Excogitation | Regel prüft | Recitation |
| Fey Blessing | Regel sperrt vorher | Excogitation |
| Lustrate | Regel sperrt vorher | Excogitation |
| Ruin | Regel prüft | Art of War |
| Ruin | Regel prüft | Bio II |
| Seraphism | Regel prüft | Summon Seraph |
| Summon Seraph | Regel prüft | Fey Blessing |
| Summon Seraph | Regel prüft | Whispering Dawn |
| Whispering Dawn | Regel sperrt vorher | Excogitation |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

keine

### Verlängerung, Aufbau, Umschalten

- Umschalten (endet bei erneutem Einsatz): Aetherpact

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Aetherflow | — (nicht im Wirktext) | Energy Drain, Excogitation, Indomitability, Lustrate, Sacred Soil |
| Faerie | — (nicht im Wirktext) | Aetherpact |
| MP | Aetherflow | — |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Fenster, deren Verbraucher an Bedingungen hängt

Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster (Konzept 14, „Werden die Fenster genutzt"). Kandidaten, von Hand bewertet.

- Baneful Impaction: 1 Aufruf(e), mit Rückfall vor Ablauf

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Biolysis (1), Broil IV (1), Seraphic Veil (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `AngelFeathersPvE`, `BreathOfTheEarthPvE`, `HealingWindPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Dissolve Union (`DissolveUnionPvE`, Ability): ungenutzt
- Embrace (`EmbracePvE`, Spell): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Repose (`ReposePvE`, Spell): ungenutzt
- Rescue (`RescuePvE`, Ability): ungenutzt
- Seraphic Veil (`SeraphicVeilPvE`, Spell): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
