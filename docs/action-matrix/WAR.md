# WAR — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `WarriorRotation`
- Rotation: `RotationSolver/RebornRotations/Tank/WAR_Reborn.cs`
- Matrix als Tabelle: `WAR.csv`

## Nutzung

direkt: 40 · ungenutzt: 1

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Tanks | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Tanks | Interject (`InterjectPvE`) | 7538 | Ability | direkt |
| Tanks | Low Blow (`LowBlowPvE`) | 7540 | Ability | direkt |
| Tanks | Provoke (`ProvokePvE`) | 7533 | Ability | direkt |
| Tanks | Rampart (`RampartPvE`) | 7531 | Ability | direkt |
| Tanks | Reprisal (`ReprisalPvE`) | 7535 | Ability | direkt |
| Tanks | Shirk (`ShirkPvE`) | 7537 | Ability | direkt |
| Job | Berserk (`BerserkPvE`) | 38 | Ability | direkt |
| Job | Bloodwhetting (`BloodwhettingPvE`) | 25751 | Ability | direkt |
| Job | Chaotic Cyclone (`ChaoticCyclonePvE`) | 16463 | Weaponskill | direkt |
| Job | Damnation (`DamnationPvE`) | 36923 | Ability | direkt |
| Job | Decimate (`DecimatePvE`) | 3550 | Weaponskill | direkt |
| Job | Defiance (`DefiancePvE`) | 48 | Ability | direkt |
| Job | Equilibrium (`EquilibriumPvE`) | 3552 | Ability | direkt |
| Job | Fell Cleave (`FellCleavePvE`) | 3549 | Weaponskill | direkt |
| Job | Heavy Swing (`HeavySwingPvE`) | 31 | Weaponskill | direkt |
| Job | Holmgang (`HolmgangPvE`) | 43 | Ability | direkt |
| Job | Infuriate (`InfuriatePvE`) | 52 | Ability | direkt |
| Job | Inner Beast (`InnerBeastPvE`) | 49 | Weaponskill | direkt |
| Job | Inner Chaos (`InnerChaosPvE`) | 16465 | Weaponskill | direkt |
| Job | Inner Release (`InnerReleasePvE`) | 7389 | Ability | direkt |
| Job | Maim (`MaimPvE`) | 37 | Weaponskill | direkt |
| Job | Mythril Tempest (`MythrilTempestPvE`) | 16462 | Weaponskill | direkt |
| Job | Nascent Flash (`NascentFlashPvE`) | 16464 | Ability | direkt |
| Job | Onslaught (`OnslaughtPvE`) | 7386 | Ability | direkt |
| Job | Orogeny (`OrogenyPvE`) | 25752 | Ability | direkt |
| Job | Overpower (`OverpowerPvE`) | 41 | Weaponskill | direkt |
| Job | Primal Rend (`PrimalRendPvE`) | 25753 | Weaponskill | direkt |
| Job | Primal Ruination (`PrimalRuinationPvE`) | 36925 | Weaponskill | direkt |
| Job | Primal Wrath (`PrimalWrathPvE`) | 36924 | Ability | direkt |
| Job | Raw Intuition (`RawIntuitionPvE`) | 3551 | Ability | direkt |
| Job | Release Defiance (`ReleaseDefiancePvE`) | 32066 | Ability | ungenutzt |
| Job | Shake It Off (`ShakeItOffPvE`) | 7388 | Ability | direkt |
| Job | Steel Cyclone (`SteelCyclonePvE`) | 51 | Weaponskill | direkt |
| Job | Storm's Eye (`StormsEyePvE`) | 45 | Weaponskill | direkt |
| Job | Storm's Path (`StormsPathPvE`) | 42 | Weaponskill | direkt |
| Job | Thrill of Battle (`ThrillOfBattlePvE`) | 40 | Ability | direkt |
| Job | Tomahawk (`TomahawkPvE`) | 46 | Weaponskill | direkt |
| Job | Upheaval (`UpheavalPvE`) | 7387 | Ability | direkt |
| Job | Vengeance (`VengeancePvE`) | 44 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Berserk | Ausbau (Berserk Mastery) | Inner Release |
| Bloodwhetting | gemeinsame Abklingzeit | Nascent Flash |
| Chaotic Cyclone | kostet | Beast |
| Chaotic Cyclone | braucht Nascent Chaos | Eigenschaft Nascent Chaos |
| Decimate | kostet | Beast |
| Decimate | Knopf wird zu | Chaotic Cyclone |
| Fell Cleave | kostet | Beast |
| Fell Cleave | Knopf wird zu | Inner Chaos |
| Infuriate | Bedingung (kein Status) | combat |
| Inner Beast | kostet | Beast |
| Inner Beast | Ausbau (Inner Beast Mastery) | Fell Cleave |
| Inner Chaos | kostet | Beast |
| Inner Chaos | braucht Nascent Chaos | Eigenschaft Nascent Chaos |
| Inner Release | Knopf wird zu | Primal Wrath |
| Maim | Combo nach | Heavy Swing |
| Mythril Tempest | Combo nach | Overpower |
| Orogeny | gemeinsame Abklingzeit | Upheaval |
| Primal Rend | braucht Primal Rend Ready granted by Inner Release | Inner Release |
| Primal Rend | Knopf wird zu | Primal Ruination |
| Primal Ruination | braucht Primal Ruination Ready | Primal Rend |
| Primal Wrath | braucht Wrathful (Erzeuger nicht im Text) | Wrathful |
| Raw Intuition | Ausbau (Raw Intuition Mastery) | Bloodwhetting |
| Steel Cyclone | kostet | Beast |
| Steel Cyclone | Ausbau (Steel Cyclone Mastery) | Decimate |
| Storm's Eye | Combo nach | Maim |
| Storm's Path | Combo nach | Maim |
| Vengeance | Ausbau (Vengeance Mastery) | Damnation |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Berserk | ActionCheck liest | Inner Release |
| Berserk | Regel prüft | Inner Release |
| Berserk | Regel prüft | Storm's Eye |
| Damnation | Regel prüft | Rampart |
| Damnation | Regel prüft | Vengeance |
| Decimate | Regel sperrt vorher | Primal Rend |
| Decimate | Regel prüft | Storm's Eye |
| Fell Cleave | Regel sperrt vorher | Primal Rend |
| Fell Cleave | Regel prüft | Storm's Eye |
| Heavy Swing | Regel sperrt vorher | Primal Rend |
| Infuriate | Regel prüft | Inner Release |
| Inner Beast | Regel prüft | Fell Cleave |
| Inner Beast | Regel sperrt vorher | Primal Rend |
| Inner Beast | Regel prüft | Storm's Eye |
| Inner Release | Regel prüft | Storm's Eye |
| Maim | ComboIds | Heavy Swing |
| Maim | Regel sperrt vorher | Primal Rend |
| Mythril Tempest | ComboIds | Overpower |
| Mythril Tempest | Regel sperrt vorher | Primal Rend |
| Onslaught | Regel prüft | Upheaval |
| Overpower | Regel sperrt vorher | Primal Rend |
| Primal Rend | StatusNeed PrimalRendReady | Inner Release |
| Primal Ruination | Regel sperrt vorher | Primal Rend |
| Rampart | Regel prüft | Damnation |
| Rampart | Regel prüft | Vengeance |
| Raw Intuition | Regel prüft | Bloodwhetting |
| Steel Cyclone | Regel prüft | Decimate |
| Steel Cyclone | Regel sperrt vorher | Primal Rend |
| Steel Cyclone | Regel prüft | Storm's Eye |
| Storm's Eye | ComboIds | Maim |
| Storm's Eye | Regel sperrt vorher | Primal Rend |
| Storm's Path | ComboIds | Maim |
| Storm's Path | Regel sperrt vorher | Primal Rend |
| Tomahawk | Regel sperrt vorher | Primal Rend |
| Upheaval | StatusNeed SurgingTempest | Storm's Eye |
| Vengeance | Regel prüft | Damnation |
| Vengeance | Regel prüft | Rampart |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Shake It Off | Abwehr | hebt Thrill of Battle auf | Thrill of Battle |

### Verlängerung, Aufbau, Umschalten

- Inner Release verlängert Surging Tempest um 10 s, höchstens auf 60 s
- Mythril Tempest verlängert Surging Tempest um 30 s, höchstens auf 60 s
- Storm's Eye verlängert Surging Tempest um 30 s, höchstens auf 60 s
- Berserk: 3 Stapel Berserk
- Inner Release: 3 Stapel Inner Release
- Umschalten (endet bei erneutem Einsatz): Defiance

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Beast | Infuriate | Chaotic Cyclone, Decimate, Fell Cleave, Inner Beast, Inner Chaos, Steel Cyclone |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Fell Cleave (1), Heavy Swing (1), Maim (1), Nascent Flash (1), Reprisal (1), Storm's Eye (2), Storm's Path (2), Upheaval (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `LandWakerPvE`, `ShieldWallPvE`, `StrongholdPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Release Defiance (`ReleaseDefiancePvE`, Ability): ungenutzt
