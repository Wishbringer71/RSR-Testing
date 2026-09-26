# WHM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `WhiteMageRotation`
- Rotation: `RotationSolver/RebornRotations/Healer/WHM_Reborn.cs`
- Matrix als Tabelle: `WHM.csv`

## Nutzung

direkt: 41 · ungenutzt: 2 · über andere Aktion: 1

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Heiler | Esuna (`EsunaPvE`) | 7568 | Spell | direkt |
| Heiler | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Heiler | Repose (`ReposePvE`) | 16560 | Spell | ungenutzt |
| Heiler | Rescue (`RescuePvE`) | 7571 | Ability | ungenutzt |
| Heiler | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Heiler | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Aero (`AeroPvE`) | 121 | Spell | direkt |
| Job | Aero II (`AeroIiPvE`) | 132 | Spell | direkt |
| Job | Aetherial Shift (`AetherialShiftPvE`) | 37008 | Ability | direkt |
| Job | Afflatus Misery (`AfflatusMiseryPvE`) | 16535 | Spell | direkt |
| Job | Afflatus Rapture (`AfflatusRapturePvE`) | 16534 | Spell | direkt |
| Job | Afflatus Solace (`AfflatusSolacePvE`) | 16531 | Spell | direkt |
| Job | Aquaveil (`AquaveilPvE`) | 25861 | Ability | direkt |
| Job | Assize (`AssizePvE`) | 3571 | Ability | direkt |
| Job | Asylum (`AsylumPvE`) | 3569 | Ability | direkt |
| Job | Benediction (`BenedictionPvE`) | 140 | Ability | direkt |
| Job | Cure (`CurePvE`) | 120 | Spell | direkt |
| Job | Cure II (`CureIiPvE`) | 135 | Spell | direkt |
| Job | Cure III (`CureIiiPvE`) | 131 | Spell | direkt |
| Job | Dia (`DiaPvE`) | 16532 | Spell | direkt |
| Job | Divine Benison (`DivineBenisonPvE`) | 7432 | Ability | direkt |
| Job | Divine Caress (`DivineCaressPvE`) | 37011 | Ability | direkt |
| Job | Glare (`GlarePvE`) | 16533 | Spell | direkt |
| Job | Glare III (`GlareIiiPvE`) | 25859 | Spell | direkt |
| Job | Glare IV (`GlareIvPvE`) | 37009 | Spell | direkt |
| Job | Holy (`HolyPvE`) | 139 | Spell | direkt |
| Job | Holy III (`HolyIiiPvE`) | 25860 | Spell | direkt |
| Job | Liturgy of the Bell (`LiturgyOfTheBellPvE`) | 25862 | Ability | direkt |
| Job | Liturgy of the Bell (`LiturgyOfTheBellPvE_28509`) | 28509 | Ability | über gleichnamige Aktion `LiturgyOfTheBellPvE` |
| Job | Medica (`MedicaPvE`) | 124 | Spell | direkt |
| Job | Medica II (`MedicaIiPvE`) | 133 | Spell | direkt |
| Job | Medica III (`MedicaIiiPvE`) | 37010 | Spell | direkt |
| Job | Plenary Indulgence (`PlenaryIndulgencePvE`) | 7433 | Ability | direkt |
| Job | Presence of Mind (`PresenceOfMindPvE`) | 136 | Ability | direkt |
| Job | Raise (`RaisePvE`) | 125 | Spell | direkt |
| Job | Regen (`RegenPvE`) | 137 | Spell | direkt |
| Job | Stone (`StonePvE`) | 119 | Spell | direkt |
| Job | Stone II (`StoneIiPvE`) | 127 | Spell | direkt |
| Job | Stone III (`StoneIiiPvE`) | 3568 | Spell | direkt |
| Job | Stone IV (`StoneIvPvE`) | 7431 | Spell | direkt |
| Job | Temperance (`TemperancePvE`) | 16536 | Ability | direkt |
| Job | Tetragrammaton (`TetragrammatonPvE`) | 3570 | Ability | direkt |
| Job | Thin Air (`ThinAirPvE`) | 7430 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Aero II | Ausbau (Aero Mastery II) | Dia |
| Afflatus Misery | Bedingung (kein Status) | the Blood Lily is in full bloom |
| Afflatus Rapture | kostet | Healing |
| Afflatus Solace | kostet | Healing |
| Divine Caress | braucht Divine Grace | Eigenschaft Enhanced Temperance |
| Glare IV | braucht Sacred Sight | Eigenschaft Enhanced Presence of Mind |
| Glare | Ausbau (Glare Mastery) | Glare III |
| Holy | Ausbau (Holy Mastery) | Holy III |
| Medica II | Ausbau (Medica Mastery) | Medica III |
| Stone II | Ausbau (Stone Mastery II) | Stone III |
| Stone III | Ausbau (Stone Mastery III) | Stone IV |
| Stone IV | Ausbau (Stone Mastery IV) | Glare |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Aero II | Regel prüft | Aero |
| Aero II | Regel prüft | Dia |
| Aero | Regel prüft | Aero II |
| Afflatus Rapture | Regel prüft | Afflatus Misery |
| Afflatus Solace | Regel prüft | Afflatus Misery |
| Aquaveil | Regel sperrt vorher | Divine Benison |
| Asylum | Regel sperrt vorher | Benediction |
| Dia | Regel prüft | Aero |
| Divine Benison | Regel sperrt vorher | Aquaveil |
| Divine Benison | Regel sperrt vorher | Benediction |
| Divine Caress | Regel sperrt vorher | Liturgy of the Bell |
| Divine Caress | Regel sperrt vorher | Temperance |
| Divine Caress | StatusNeed DivineGrace | Temperance |
| Glare | Regel prüft | Glare III |
| Holy III | Regel prüft | Holy |
| Holy | Regel prüft | Holy III |
| Liturgy of the Bell | Regel sperrt vorher | Temperance |
| Medica II | Regel prüft | Medica III |
| Medica III | Regel prüft | Medica II |
| Plenary Indulgence | Regel prüft | Afflatus Rapture |
| Plenary Indulgence | Regel prüft | Cure III |
| Plenary Indulgence | Regel sperrt vorher | Liturgy of the Bell |
| Plenary Indulgence | Regel prüft | Medica II |
| Plenary Indulgence | Regel prüft | Medica |
| Plenary Indulgence | Regel sperrt vorher | Temperance |
| Stone II | Regel prüft | Stone III |
| Stone III | Regel prüft | Stone IV |
| Stone IV | Regel prüft | Glare |
| Stone | Regel prüft | Stone II |
| Temperance | Regel sperrt vorher | Liturgy of the Bell |
| Tetragrammaton | Regel sperrt vorher | Benediction |
| Thin Air | Regel prüft | Raise |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BreathOfTheEarthPvE`, `HealingWindPvE`, `PulseOfLifePvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Repose (`ReposePvE`, Spell): ungenutzt
- Rescue (`RescuePvE`, Ability): ungenutzt
