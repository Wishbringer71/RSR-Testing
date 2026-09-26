# GNB — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `GunbreakerRotation`
- Rotation: `RotationSolver/RebornRotations/Tank/GNB_Reborn.cs`
- Matrix als Tabelle: `GNB.csv`

## Nutzung

direkt: 44 · nur gelesen: 1 · ungenutzt: 1

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
| Job | Abdomen Tear (`AbdomenTearPvE`) | 16157 | Ability | direkt |
| Job | Aurora (`AuroraPvE`) | 16151 | Ability | direkt |
| Job | Blasting Zone (`BlastingZonePvE`) | 16165 | Ability | direkt |
| Job | Bloodfest (`BloodfestPvE`) | 16164 | Ability | direkt |
| Job | Bow Shock (`BowShockPvE`) | 16159 | Ability | direkt |
| Job | Brutal Shell (`BrutalShellPvE`) | 16139 | Weaponskill | direkt |
| Job | Burst Strike (`BurstStrikePvE`) | 16162 | Weaponskill | direkt |
| Job | Camouflage (`CamouflagePvE`) | 16140 | Ability | direkt |
| Job | Continuation (`ContinuationPvE`) | 16155 | Ability | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Danger Zone (`DangerZonePvE`) | 16144 | Ability | direkt |
| Job | Demon Slaughter (`DemonSlaughterPvE`) | 16149 | Weaponskill | direkt |
| Job | Demon Slice (`DemonSlicePvE`) | 16141 | Weaponskill | direkt |
| Job | Double Down (`DoubleDownPvE`) | 25760 | Weaponskill | direkt |
| Job | Eye Gouge (`EyeGougePvE`) | 16158 | Ability | direkt |
| Job | Fated Brand (`FatedBrandPvE`) | 36936 | Ability | direkt |
| Job | Fated Circle (`FatedCirclePvE`) | 16163 | Weaponskill | direkt |
| Job | Gnashing Fang (`GnashingFangPvE`) | 16146 | Weaponskill | direkt |
| Job | Great Nebula (`GreatNebulaPvE`) | 36935 | Ability | direkt |
| Job | Heart of Corundum (`HeartOfCorundumPvE`) | 25758 | Ability | direkt |
| Job | Heart of Light (`HeartOfLightPvE`) | 16160 | Ability | direkt |
| Job | Heart of Stone (`HeartOfStonePvE`) | 16161 | Ability | direkt |
| Job | Hypervelocity (`HypervelocityPvE`) | 25759 | Ability | direkt |
| Job | Jugular Rip (`JugularRipPvE`) | 16156 | Ability | direkt |
| Job | Keen Edge (`KeenEdgePvE`) | 16137 | Weaponskill | direkt |
| Job | Lightning Shot (`LightningShotPvE`) | 16143 | Weaponskill | direkt |
| Job | Lion Heart (`LionHeartPvE`) | 36939 | Weaponskill | direkt |
| Job | Nebula (`NebulaPvE`) | 16148 | Ability | direkt |
| Job | No Mercy (`NoMercyPvE`) | 16138 | Ability | direkt |
| Job | Noble Blood (`NobleBloodPvE`) | 36938 | Weaponskill | direkt |
| Job | Reign of Beasts (`ReignOfBeastsPvE`) | 36937 | Weaponskill | direkt |
| Job | Release Royal Guard (`ReleaseRoyalGuardPvE`) | 32068 | Ability | ungenutzt |
| Job | Royal Guard (`RoyalGuardPvE`) | 16142 | Ability | direkt |
| Job | Savage Claw (`SavageClawPvE`) | 16147 | Weaponskill | direkt |
| Job | Solid Barrel (`SolidBarrelPvE`) | 16145 | Weaponskill | direkt |
| Job | Sonic Break (`SonicBreakPvE`) | 16153 | Weaponskill | direkt |
| Job | Superbolide (`SuperbolidePvE`) | 16152 | Ability | direkt |
| Job | Trajectory (`TrajectoryPvE`) | 36934 | Ability | direkt |
| Job | Wicked Talon (`WickedTalonPvE`) | 16150 | Weaponskill | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Abdomen Tear | braucht Ready to Tear (Erzeuger nicht im Text) | Ready to Tear |
| Brutal Shell | Combo nach | Keen Edge |
| Burst Strike | kostet | Cartridge |
| Continuation | Knopf wird zu | Abdomen Tear |
| Continuation | Knopf wird zu | Eye Gouge |
| Continuation | Knopf wird zu | Fated Brand |
| Continuation | Knopf wird zu | Hypervelocity |
| Continuation | Knopf wird zu | Jugular Rip |
| Danger Zone | Ausbau (Danger Zone Mastery) | Blasting Zone |
| Demon Slaughter | Combo nach | Demon Slice |
| Double Down | kostet | Cartridge |
| Eye Gouge | braucht Ready to Gouge (Erzeuger nicht im Text) | Ready to Gouge |
| Fated Brand | braucht Ready to Raze (Erzeuger nicht im Text) | Ready to Raze |
| Fated Circle | kostet | Cartridge |
| Gnashing Fang | kostet | Cartridge |
| Gnashing Fang | Knopf wird zu | Savage Claw |
| Heart of Stone | Ausbau (Heart of Stone Mastery) | Heart of Corundum |
| Hypervelocity | braucht Ready to Blast (Erzeuger nicht im Text) | Ready to Blast |
| Jugular Rip | braucht Ready to Rip (Erzeuger nicht im Text) | Ready to Rip |
| Lion Heart | Combo nach | Noble Blood |
| Nebula | Ausbau (Nebula Mastery) | Great Nebula |
| Noble Blood | Knopf wird zu | Lion Heart |
| Noble Blood | Combo nach | Reign of Beasts |
| Reign of Beasts | braucht Ready to Reign | Bloodfest |
| Reign of Beasts | Knopf wird zu | Noble Blood |
| Savage Claw | Combo nach | Gnashing Fang |
| Savage Claw | Knopf wird zu | Wicked Talon |
| Solid Barrel | Combo nach | Brutal Shell |
| Sonic Break | braucht Ready to Break (Erzeuger nicht im Text) | Ready to Break |
| Wicked Talon | Combo nach | Savage Claw |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Abdomen Tear | StatusNeed ReadyToTear | Status ReadyToTear |
| Aurora | Regel sperrt vorher | Gnashing Fang |
| Aurora | Regel sperrt vorher | No Mercy |
| Blasting Zone | Regel prüft | Bloodfest |
| Blasting Zone | Regel sperrt vorher | Danger Zone |
| Blasting Zone | Regel sperrt vorher | Demon Slice |
| Blasting Zone | Regel prüft | Double Down |
| Blasting Zone | Regel sperrt vorher | Gnashing Fang |
| Blasting Zone | Regel prüft | No Mercy |
| Blasting Zone | Regel sperrt vorher | No Mercy |
| Blasting Zone | Regel sperrt vorher | Sonic Break |
| Bow Shock | Regel sperrt vorher | Danger Zone |
| Bow Shock | Regel sperrt vorher | Gnashing Fang |
| Bow Shock | Regel sperrt vorher | No Mercy |
| Brutal Shell | Regel sperrt vorher | Bloodfest |
| Brutal Shell | Regel sperrt vorher | Gnashing Fang |
| Brutal Shell | Regel sperrt vorher | Keen Edge |
| Brutal Shell | Regel sperrt vorher | No Mercy |
| Burst Strike | Regel sperrt vorher | Bloodfest |
| Burst Strike | Regel sperrt vorher | No Mercy |
| Camouflage | Regel sperrt vorher | Gnashing Fang |
| Camouflage | Regel sperrt vorher | No Mercy |
| Danger Zone | Regel prüft | Double Down |
| Danger Zone | Regel sperrt vorher | Gnashing Fang |
| Danger Zone | Regel sperrt vorher | No Mercy |
| Demon Slaughter | Regel sperrt vorher | Bloodfest |
| Demon Slaughter | Regel sperrt vorher | Brutal Shell |
| Demon Slaughter | Regel sperrt vorher | Gnashing Fang |
| Demon Slaughter | Regel sperrt vorher | Keen Edge |
| Demon Slaughter | Regel sperrt vorher | No Mercy |
| Demon Slice | Regel sperrt vorher | Bloodfest |
| Demon Slice | Regel prüft | Bow Shock |
| Demon Slice | Regel sperrt vorher | Brutal Shell |
| Demon Slice | Regel sperrt vorher | Danger Zone |
| Demon Slice | Regel prüft | Double Down |
| Demon Slice | Regel sperrt vorher | Gnashing Fang |
| Demon Slice | Regel sperrt vorher | Keen Edge |
| Demon Slice | Regel prüft | No Mercy |
| Demon Slice | Regel sperrt vorher | No Mercy |
| Double Down | Regel sperrt vorher | Bloodfest |
| Double Down | Regel sperrt vorher | No Mercy |
| Eye Gouge | StatusNeed ReadyToGouge | Status ReadyToGouge |
| Fated Circle | Regel sperrt vorher | Bloodfest |
| Fated Circle | Regel sperrt vorher | Brutal Shell |
| Fated Circle | Regel sperrt vorher | Gnashing Fang |
| Fated Circle | Regel sperrt vorher | Keen Edge |
| Fated Circle | Regel sperrt vorher | No Mercy |
| Gnashing Fang | Regel sperrt vorher | Bloodfest |
| Gnashing Fang | Regel sperrt vorher | No Mercy |
| Great Nebula | Regel sperrt vorher | Gnashing Fang |
| Great Nebula | Regel prüft | Nebula |
| Great Nebula | Regel sperrt vorher | No Mercy |
| Great Nebula | Regel prüft | Rampart |
| Heart of Corundum | Regel sperrt vorher | Gnashing Fang |
| Heart of Corundum | Regel prüft | Heart of Stone |
| Heart of Corundum | Regel sperrt vorher | No Mercy |
| Heart of Light | Regel sperrt vorher | Gnashing Fang |
| Heart of Light | Regel sperrt vorher | No Mercy |
| Heart of Stone | Regel sperrt vorher | Gnashing Fang |
| Heart of Stone | Regel prüft | Heart of Corundum |
| Heart of Stone | Regel sperrt vorher | No Mercy |
| Hypervelocity | StatusNeed ReadyToBlast | Status ReadyToBlast |
| Jugular Rip | StatusNeed ReadyToRip | Status ReadyToRip |
| Keen Edge | Regel sperrt vorher | Bloodfest |
| Keen Edge | Regel sperrt vorher | Brutal Shell |
| Keen Edge | Regel sperrt vorher | Gnashing Fang |
| Keen Edge | Regel sperrt vorher | No Mercy |
| Lightning Shot | Regel sperrt vorher | Bloodfest |
| Lightning Shot | Regel sperrt vorher | Brutal Shell |
| Lightning Shot | Regel sperrt vorher | Gnashing Fang |
| Lightning Shot | Regel sperrt vorher | Keen Edge |
| Lightning Shot | Regel sperrt vorher | No Mercy |
| Lion Heart | Regel sperrt vorher | Bloodfest |
| Lion Heart | Regel prüft | Double Down |
| Lion Heart | Regel prüft | Gnashing Fang |
| Lion Heart | Regel sperrt vorher | No Mercy |
| Nebula | Regel sperrt vorher | Gnashing Fang |
| Nebula | Regel prüft | Great Nebula |
| Nebula | Regel sperrt vorher | No Mercy |
| Nebula | Regel prüft | Rampart |
| No Mercy | Regel prüft | Brutal Shell |
| No Mercy | Regel prüft | Burst Strike |
| No Mercy | Regel prüft | Demon Slaughter |
| No Mercy | Regel prüft | Demon Slice |
| No Mercy | Regel prüft | Double Down |
| No Mercy | Regel prüft | Fated Circle |
| No Mercy | Regel prüft | Gnashing Fang |
| No Mercy | Regel prüft | Keen Edge |
| No Mercy | Regel prüft | Reign of Beasts |
| No Mercy | Regel prüft | Solid Barrel |
| Noble Blood | Regel sperrt vorher | Bloodfest |
| Noble Blood | Regel prüft | Double Down |
| Noble Blood | Regel prüft | Gnashing Fang |
| Noble Blood | Regel sperrt vorher | No Mercy |
| Rampart | Regel sperrt vorher | Gnashing Fang |
| Rampart | Regel prüft | Great Nebula |
| Rampart | Regel prüft | Nebula |
| Rampart | Regel sperrt vorher | No Mercy |
| Reign of Beasts | Regel sperrt vorher | Bloodfest |
| Reign of Beasts | StatusNeed ReadyToReign | Bloodfest |
| Reign of Beasts | Regel prüft | Double Down |
| Reign of Beasts | Regel prüft | Gnashing Fang |
| Reign of Beasts | Regel sperrt vorher | No Mercy |
| Reprisal | Regel sperrt vorher | Gnashing Fang |
| Reprisal | Regel sperrt vorher | No Mercy |
| Savage Claw | Regel sperrt vorher | Bloodfest |
| Savage Claw | Regel sperrt vorher | No Mercy |
| Solid Barrel | Regel sperrt vorher | Bloodfest |
| Solid Barrel | Regel sperrt vorher | Brutal Shell |
| Solid Barrel | Regel sperrt vorher | Gnashing Fang |
| Solid Barrel | Regel sperrt vorher | Keen Edge |
| Solid Barrel | Regel sperrt vorher | No Mercy |
| Sonic Break | Regel sperrt vorher | Bloodfest |
| Sonic Break | Regel sperrt vorher | No Mercy |
| Wicked Talon | Regel sperrt vorher | Bloodfest |
| Wicked Talon | Regel sperrt vorher | No Mercy |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

keine

### Verlängerung, Aufbau, Umschalten

- Umschalten (endet bei erneutem Einsatz): Royal Guard

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Cartridge | Demon Slaughter | Burst Strike, Double Down, Fated Circle, Gnashing Fang |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Fenster, deren Verbraucher an Bedingungen hängt

Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster (Konzept 14, „Werden die Fenster genutzt"). Kandidaten, von Hand bewertet.

- Sonic Break: 1 Aufruf(e), mit Rückfall vor Ablauf

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Abdomen Tear (1), Blasting Zone (1), Brutal Shell (2), Burst Strike (1), Eye Gouge (1), Gnashing Fang (1), Hypervelocity (1), Jugular Rip (1), Keen Edge (1), Reprisal (1), Savage Claw (1), Solid Barrel (1), Wicked Talon (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `GunmetalSoulPvE`, `ShieldWallPvE`, `StrongholdPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Continuation (`ContinuationPvE`, Ability): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Release Royal Guard (`ReleaseRoyalGuardPvE`, Ability): ungenutzt
