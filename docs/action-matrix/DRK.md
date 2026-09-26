# DRK — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `DarkKnightRotation`
- Rotation: `RotationSolver/RebornRotations/Tank/DRK_Reborn.cs`
- Matrix als Tabelle: `DRK.csv`

## Nutzung

direkt: 41 · ungenutzt: 1 · über andere Aktion: 1

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
| Job | Abyssal Drain (`AbyssalDrainPvE`) | 3641 | Ability | direkt |
| Job | Blood Weapon (`BloodWeaponPvE`) | 3625 | Ability | direkt |
| Job | Bloodspiller (`BloodspillerPvE`) | 7392 | Weaponskill | direkt |
| Job | Carve and Spit (`CarveAndSpitPvE`) | 3643 | Ability | direkt |
| Job | Comeuppance (`ComeuppancePvE`) | 36929 | Weaponskill | direkt |
| Job | Dark Mind (`DarkMindPvE`) | 3634 | Ability | direkt |
| Job | Dark Missionary (`DarkMissionaryPvE`) | 16471 | Ability | direkt |
| Job | Delirium (`DeliriumPvE`) | 7390 | Ability | direkt |
| Job | Disesteem (`DisesteemPvE`) | 36932 | Weaponskill | direkt |
| Job | Edge of Darkness (`EdgeOfDarknessPvE`) | 16467 | Ability | direkt |
| Job | Edge of Shadow (`EdgeOfShadowPvE`) | 16470 | Ability | direkt |
| Job | Flood of Darkness (`FloodOfDarknessPvE`) | 16466 | Ability | direkt |
| Job | Flood of Shadow (`FloodOfShadowPvE`) | 16469 | Ability | über Flood of Darkness |
| Job | Grit (`GritPvE`) | 3629 | Ability | direkt |
| Job | Hard Slash (`HardSlashPvE`) | 3617 | Weaponskill | direkt |
| Job | Impalement (`ImpalementPvE`) | 36931 | Weaponskill | direkt |
| Job | Living Dead (`LivingDeadPvE`) | 3638 | Ability | direkt |
| Job | Living Shadow (`LivingShadowPvE`) | 16472 | Ability | direkt |
| Job | Oblation (`OblationPvE`) | 25754 | Ability | direkt |
| Job | Quietus (`QuietusPvE`) | 7391 | Weaponskill | direkt |
| Job | Release Grit (`ReleaseGritPvE`) | 32067 | Ability | ungenutzt |
| Job | Salt and Darkness (`SaltAndDarknessPvE`) | 25755 | Ability | direkt |
| Job | Salted Earth (`SaltedEarthPvE`) | 3639 | Ability | direkt |
| Job | Scarlet Delirium (`ScarletDeliriumPvE`) | 36928 | Weaponskill | direkt |
| Job | Shadow Wall (`ShadowWallPvE`) | 3636 | Ability | direkt |
| Job | Shadowbringer (`ShadowbringerPvE`) | 25757 | Ability | direkt |
| Job | Shadowed Vigil (`ShadowedVigilPvE`) | 36927 | Ability | direkt |
| Job | Shadowstride (`ShadowstridePvE`) | 36926 | Ability | direkt |
| Job | Souleater (`SouleaterPvE`) | 3632 | Weaponskill | direkt |
| Job | Stalwart Soul (`StalwartSoulPvE`) | 16468 | Spell | direkt |
| Job | Syphon Strike (`SyphonStrikePvE`) | 3623 | Weaponskill | direkt |
| Job | The Blackest Night (`TheBlackestNightPvE`) | 7393 | Ability | direkt |
| Job | Torcleaver (`TorcleaverPvE`) | 36930 | Weaponskill | direkt |
| Job | Unleash (`UnleashPvE`) | 3621 | Spell | direkt |
| Job | Unmend (`UnmendPvE`) | 3624 | Spell | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Abyssal Drain | gemeinsame Abklingzeit | Carve and Spit |
| Blood Weapon | Ausbau (Blood Weapon Mastery) | Delirium |
| Bloodspiller | kostet | Blood |
| Bloodspiller | Ausbau (Enhanced Delirium) | Scarlet Delirium |
| Bloodspiller | Knopf wird zu | Scarlet Delirium |
| Carve and Spit | gemeinsame Abklingzeit | Abyssal Drain |
| Comeuppance | braucht Delirium | Delirium |
| Comeuppance | Combo nach | Scarlet Delirium |
| Comeuppance | Knopf wird zu | Torcleaver |
| Disesteem | braucht Scorn | Living Shadow |
| Edge of Darkness | Ausbau (Darkside Mastery) | Edge of Shadow |
| Edge of Darkness | gemeinsame Abklingzeit | Flood of Darkness |
| Edge of Shadow | gemeinsame Abklingzeit | Flood of Shadow |
| Flood of Darkness | gemeinsame Abklingzeit | Edge of Darkness |
| Flood of Darkness | Ausbau (Darkside Mastery) | Flood of Shadow |
| Flood of Shadow | gemeinsame Abklingzeit | Edge of Shadow |
| Impalement | braucht Delirium | Delirium |
| Quietus | kostet | Blood |
| Quietus | Ausbau (Enhanced Delirium) | Impalement |
| Quietus | Knopf wird zu | Impalement |
| Scarlet Delirium | Knopf wird zu | Comeuppance |
| Scarlet Delirium | braucht Delirium | Delirium |
| Shadow Wall | Ausbau (Shadow Wall Mastery) | Shadowed Vigil |
| Shadowbringer | braucht Darkside | Edge of Darkness |
| Shadowbringer | braucht Darkside | Edge of Shadow |
| Shadowbringer | braucht Darkside | Flood of Darkness |
| Shadowbringer | braucht Darkside | Flood of Shadow |
| Souleater | Combo nach | Syphon Strike |
| Stalwart Soul | Combo nach | Unleash |
| Syphon Strike | Combo nach | Hard Slash |
| Torcleaver | Combo nach | Comeuppance |
| Torcleaver | braucht Delirium | Delirium |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Blood Weapon | Regel prüft | Delirium |
| Comeuppance | ComboIds | Scarlet Delirium |
| Delirium | Regel prüft | Souleater |
| Disesteem | StatusNeed Scorn | Living Shadow |
| Edge of Shadow | Regel prüft | Hard Slash |
| Edge of Shadow | Regel prüft | Unmend |
| Hard Slash | Regel prüft | Souleater |
| Living Shadow | Regel prüft | Hard Slash |
| Rampart | Regel prüft | Shadow Wall |
| Rampart | Regel prüft | Shadowed Vigil |
| Salt and Darkness | StatusNeed SaltedEarth | Status SaltedEarth |
| Shadow Wall | Regel prüft | Rampart |
| Shadow Wall | Regel prüft | Shadowed Vigil |
| Shadowed Vigil | Regel prüft | Rampart |
| Shadowed Vigil | Regel prüft | Shadow Wall |
| Shadowstride | Regel sperrt vorher | Provoke |
| Souleater | ComboIds | Syphon Strike |
| Stalwart Soul | ComboIds | Unleash |
| Syphon Strike | ComboIds | Hard Slash |
| Syphon Strike | Regel prüft | Souleater |
| The Blackest Night | Regel sperrt vorher | Provoke |
| Torcleaver | ComboIds | Comeuppance |
| Unmend | Regel sperrt vorher | Provoke |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

keine

### Verlängerung, Aufbau, Umschalten

- Edge of Darkness verlängert Darkside um 30 s, höchstens auf 60 s
- Edge of Shadow verlängert Darkside um 30 s, höchstens auf 60 s
- Flood of Darkness verlängert Darkside um 30 s, höchstens auf 60 s
- Flood of Shadow verlängert Darkside um 30 s, höchstens auf 60 s
- Blood Weapon: 3 Stapel Blood Weapon
- Delirium: 3 Stapel Delirium
- Umschalten (endet bei erneutem Einsatz): Grit

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Blood | Delirium | Bloodspiller, Quietus |
| MP | Abyssal Drain, Carve and Spit, Comeuppance, Impalement, Scarlet Delirium, Stalwart Soul, Syphon Strike, Torcleaver | — |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Bloodspiller (1), Carve and Spit (1), Hard Slash (1), Reprisal (1), Souleater (2), Stalwart Soul (1), Syphon Strike (2)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `DarkForcePvE`, `ShieldWallPvE`, `StrongholdPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Release Grit (`ReleaseGritPvE`, Ability): ungenutzt
