# BRD — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `BardRotation`
- Rotation: `RotationSolver/RebornRotations/Ranged/BRD_Reborn.cs`
- Matrix als Tabelle: `BRD.csv`

## Nutzung

direkt: 39 · ungenutzt: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Fernkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Fernkämpfer | Foot Graze (`FootGrazePvE`) | 7553 | Ability | ungenutzt |
| Fernkämpfer | Head Graze (`HeadGrazePvE`) | 7551 | Ability | direkt |
| Fernkämpfer | Leg Graze (`LegGrazePvE`) | 7554 | Ability | ungenutzt |
| Fernkämpfer | Peloton (`PelotonPvE`) | 7557 | Ability | direkt |
| Fernkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Job | Apex Arrow (`ApexArrowPvE`) | 16496 | Weaponskill | direkt |
| Job | Army's Paeon (`ArmysPaeonPvE`) | 116 | Ability | direkt |
| Job | Barrage (`BarragePvE`) | 107 | Ability | direkt |
| Job | Battle Voice (`BattleVoicePvE`) | 118 | Ability | direkt |
| Job | Blast Arrow (`BlastArrowPvE`) | 25784 | Weaponskill | direkt |
| Job | Bloodletter (`BloodletterPvE`) | 110 | Ability | direkt |
| Job | Burst Shot (`BurstShotPvE`) | 16495 | Weaponskill | direkt |
| Job | Caustic Bite (`CausticBitePvE`) | 7406 | Weaponskill | direkt |
| Job | Empyreal Arrow (`EmpyrealArrowPvE`) | 3558 | Ability | direkt |
| Job | Heartbreak Shot (`HeartbreakShotPvE`) | 36975 | Ability | direkt |
| Job | Heavy Shot (`HeavyShotPvE`) | 97 | Weaponskill | direkt |
| Job | Iron Jaws (`IronJawsPvE`) | 3560 | Weaponskill | direkt |
| Job | Ladonsbite (`LadonsbitePvE`) | 25783 | Weaponskill | direkt |
| Job | Mage's Ballad (`MagesBalladPvE`) | 114 | Ability | direkt |
| Job | Nature's Minne (`NaturesMinnePvE`) | 7408 | Ability | direkt |
| Job | Pitch Perfect (`PitchPerfectPvE`) | 7404 | Ability | direkt |
| Job | Quick Nock (`QuickNockPvE`) | 106 | Weaponskill | direkt |
| Job | Radiant Encore (`RadiantEncorePvE`) | 36977 | Weaponskill | direkt |
| Job | Radiant Finale (`RadiantFinalePvE`) | 25785 | Ability | direkt |
| Job | Raging Strikes (`RagingStrikesPvE`) | 101 | Ability | direkt |
| Job | Rain of Death (`RainOfDeathPvE`) | 117 | Ability | direkt |
| Job | Refulgent Arrow (`RefulgentArrowPvE`) | 7409 | Weaponskill | direkt |
| Job | Repelling Shot (`RepellingShotPvE`) | 112 | Ability | direkt |
| Job | Resonant Arrow (`ResonantArrowPvE`) | 36976 | Weaponskill | direkt |
| Job | Shadowbite (`ShadowbitePvE`) | 16494 | Weaponskill | direkt |
| Job | Sidewinder (`SidewinderPvE`) | 3562 | Ability | direkt |
| Job | Stormbite (`StormbitePvE`) | 7407 | Weaponskill | direkt |
| Job | Straight Shot (`StraightShotPvE`) | 98 | Weaponskill | direkt |
| Job | Troubadour (`TroubadourPvE`) | 7405 | Ability | direkt |
| Job | Venomous Bite (`VenomousBitePvE`) | 100 | Weaponskill | direkt |
| Job | Wide Volley (`WideVolleyPvE`) | 36974 | Weaponskill | direkt |
| Job | Windbite (`WindbitePvE`) | 113 | Weaponskill | direkt |
| Job | the Wanderer's Minuet (`TheWanderersMinuetPvE`) | 3559 | Ability | direkt |
| Job | the Warden's Paean (`TheWardensPaeanPvE`) | 3561 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Apex Arrow | Knopf wird zu | Blast Arrow |
| Apex Arrow | kostet | Soul Voice |
| Army's Paeon | Bedingung (kein Status) | combat |
| Blast Arrow | braucht Blast Arrow Ready | Eigenschaft Enhanced Apex Arrow |
| Bloodletter | Ausbau (Bloodletter Mastery) | Heartbreak Shot |
| Heartbreak Shot | gemeinsame Abklingzeit | Rain of Death |
| Heavy Shot | Ausbau (Heavy Shot Mastery) | Burst Shot |
| Mage's Ballad | Bedingung (kein Status) | combat |
| Quick Nock | Ausbau (Quick Nock Mastery) | Ladonsbite |
| Radiant Encore | braucht Radiant Encore Ready | Eigenschaft Enhanced Radiant Finale |
| Refulgent Arrow | braucht Hawk's Eye or Barrage | Barrage |
| Resonant Arrow | braucht Resonant Arrow Ready | Eigenschaft Enhanced Barrage |
| Shadowbite | braucht Hawk's Eye or Barrage | Barrage |
| Straight Shot | braucht Hawk's Eye (Erzeuger nicht im Text) | Hawk's Eye |
| Straight Shot | Ausbau (Straight Shot Mastery) | Refulgent Arrow |
| the Wanderer's Minuet | Bedingung (kein Status) | combat |
| Venomous Bite | Ausbau (Bite Mastery) | Caustic Bite |
| Wide Volley | braucht Hawk's Eye (Erzeuger nicht im Text) | Hawk's Eye |
| Wide Volley | Ausbau (Wide Volley Mastery) | Shadowbite |
| Windbite | Ausbau (Bite Mastery) | Stormbite |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Barrage | Regel prüft | Empyreal Arrow |
| Barrage | Regel sperrt vorher | Iron Jaws |
| Barrage | Regel prüft | Raging Strikes |
| Barrage | Regel sperrt vorher | Straight Shot |
| Barrage | Regel sperrt vorher | Venomous Bite |
| Barrage | Regel sperrt vorher | Windbite |
| Battle Voice | Regel prüft | Mage's Ballad |
| Battle Voice | Regel prüft | Radiant Finale |
| Battle Voice | Regel prüft | Raging Strikes |
| Bloodletter | Regel sperrt vorher | Battle Voice |
| Bloodletter | Regel sperrt vorher | Empyreal Arrow |
| Bloodletter | Regel sperrt vorher | Radiant Finale |
| Burst Shot | Regel sperrt vorher | Barrage |
| Burst Shot | Regel sperrt vorher | Blast Arrow |
| Burst Shot | Regel sperrt vorher | Caustic Bite |
| Burst Shot | Regel sperrt vorher | Iron Jaws |
| Burst Shot | Regel sperrt vorher | Stormbite |
| Burst Shot | Regel sperrt vorher | Venomous Bite |
| Burst Shot | Regel sperrt vorher | Windbite |
| Caustic Bite | Regel sperrt vorher | Barrage |
| Caustic Bite | Regel sperrt vorher | Blast Arrow |
| Caustic Bite | Regel sperrt vorher | Stormbite |
| Empyreal Arrow | Regel sperrt vorher | Battle Voice |
| Empyreal Arrow | Regel sperrt vorher | Radiant Finale |
| Empyreal Arrow | Regel prüft | Raging Strikes |
| Heartbreak Shot | Regel sperrt vorher | Battle Voice |
| Heartbreak Shot | Regel prüft | Bloodletter |
| Heartbreak Shot | Regel sperrt vorher | Empyreal Arrow |
| Heartbreak Shot | Regel sperrt vorher | Radiant Finale |
| Heavy Shot | Regel sperrt vorher | Barrage |
| Heavy Shot | Regel sperrt vorher | Blast Arrow |
| Heavy Shot | Regel sperrt vorher | Caustic Bite |
| Heavy Shot | Regel sperrt vorher | Iron Jaws |
| Heavy Shot | Regel sperrt vorher | Stormbite |
| Heavy Shot | Regel sperrt vorher | Venomous Bite |
| Heavy Shot | Regel sperrt vorher | Windbite |
| Ladonsbite | Regel sperrt vorher | Barrage |
| Ladonsbite | Regel sperrt vorher | Blast Arrow |
| Ladonsbite | Regel sperrt vorher | Quick Nock |
| Pitch Perfect | Regel sperrt vorher | Battle Voice |
| Pitch Perfect | Regel sperrt vorher | Radiant Finale |
| Quick Nock | Regel sperrt vorher | Barrage |
| Quick Nock | Regel sperrt vorher | Blast Arrow |
| Radiant Encore | StatusNeed RadiantEncoreReady | Radiant Finale |
| Radiant Finale | Regel prüft | Mage's Ballad |
| Raging Strikes | Regel prüft | Battle Voice |
| Raging Strikes | Regel prüft | Mage's Ballad |
| Raging Strikes | Regel prüft | Radiant Finale |
| Raging Strikes | Regel prüft | Straight Shot |
| Rain of Death | Regel sperrt vorher | Battle Voice |
| Rain of Death | Regel prüft | Bloodletter |
| Rain of Death | Regel sperrt vorher | Empyreal Arrow |
| Rain of Death | Regel sperrt vorher | Radiant Finale |
| Refulgent Arrow | Regel sperrt vorher | Barrage |
| Refulgent Arrow | StatusNeed Barrage | Barrage |
| Refulgent Arrow | Regel sperrt vorher | Blast Arrow |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Burst Shot |
| Refulgent Arrow | Regel sperrt vorher | Caustic Bite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Caustic Bite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Heavy Shot |
| Refulgent Arrow | Regel sperrt vorher | Iron Jaws |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Iron Jaws |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Ladonsbite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Quick Nock |
| Refulgent Arrow | Regel sperrt vorher | Stormbite |
| Refulgent Arrow | StatusNeed HawksEye_3861 | Stormbite |
| Refulgent Arrow | Regel sperrt vorher | Venomous Bite |
| Refulgent Arrow | Regel sperrt vorher | Windbite |
| Resonant Arrow | StatusNeed ResonantArrowReady | Barrage |
| Shadowbite | Regel sperrt vorher | Barrage |
| Shadowbite | StatusNeed Barrage | Barrage |
| Shadowbite | Regel sperrt vorher | Blast Arrow |
| Shadowbite | StatusNeed HawksEye_3861 | Burst Shot |
| Shadowbite | StatusNeed HawksEye_3861 | Caustic Bite |
| Shadowbite | StatusNeed HawksEye_3861 | Heavy Shot |
| Shadowbite | StatusNeed HawksEye_3861 | Iron Jaws |
| Shadowbite | StatusNeed HawksEye_3861 | Ladonsbite |
| Shadowbite | StatusNeed HawksEye_3861 | Quick Nock |
| Shadowbite | StatusNeed HawksEye_3861 | Stormbite |
| Sidewinder | Regel prüft | Battle Voice |
| Sidewinder | Regel sperrt vorher | Battle Voice |
| Sidewinder | Regel sperrt vorher | Empyreal Arrow |
| Sidewinder | Regel prüft | Radiant Finale |
| Sidewinder | Regel sperrt vorher | Radiant Finale |
| Sidewinder | Regel prüft | Raging Strikes |
| Stormbite | Regel sperrt vorher | Barrage |
| Stormbite | Regel sperrt vorher | Blast Arrow |
| Straight Shot | Regel sperrt vorher | Barrage |
| Straight Shot | StatusNeed Barrage | Barrage |
| Straight Shot | Regel sperrt vorher | Blast Arrow |
| Straight Shot | StatusNeed HawksEye_3861 | Burst Shot |
| Straight Shot | Regel sperrt vorher | Caustic Bite |
| Straight Shot | StatusNeed HawksEye_3861 | Caustic Bite |
| Straight Shot | StatusNeed HawksEye_3861 | Heavy Shot |
| Straight Shot | Regel sperrt vorher | Iron Jaws |
| Straight Shot | StatusNeed HawksEye_3861 | Iron Jaws |
| Straight Shot | StatusNeed HawksEye_3861 | Ladonsbite |
| Straight Shot | StatusNeed HawksEye_3861 | Quick Nock |
| Straight Shot | Regel prüft | Refulgent Arrow |
| Straight Shot | Regel sperrt vorher | Stormbite |
| Straight Shot | StatusNeed HawksEye_3861 | Stormbite |
| Straight Shot | Regel sperrt vorher | Venomous Bite |
| Straight Shot | Regel sperrt vorher | Windbite |
| Venomous Bite | Regel sperrt vorher | Barrage |
| Venomous Bite | Regel sperrt vorher | Blast Arrow |
| Venomous Bite | Regel prüft | Caustic Bite |
| Venomous Bite | Regel sperrt vorher | Caustic Bite |
| Venomous Bite | Regel sperrt vorher | Iron Jaws |
| Venomous Bite | Regel sperrt vorher | Stormbite |
| Venomous Bite | Regel sperrt vorher | Windbite |
| Wide Volley | Regel sperrt vorher | Barrage |
| Wide Volley | Regel sperrt vorher | Blast Arrow |
| Wide Volley | StatusNeed HawksEye_3861 | Burst Shot |
| Wide Volley | StatusNeed HawksEye_3861 | Caustic Bite |
| Wide Volley | StatusNeed HawksEye_3861 | Heavy Shot |
| Wide Volley | StatusNeed HawksEye_3861 | Iron Jaws |
| Wide Volley | StatusNeed HawksEye_3861 | Ladonsbite |
| Wide Volley | StatusNeed HawksEye_3861 | Quick Nock |
| Wide Volley | StatusNeed HawksEye_3861 | Stormbite |
| Windbite | Regel sperrt vorher | Barrage |
| Windbite | Regel sperrt vorher | Blast Arrow |
| Windbite | Regel sperrt vorher | Caustic Bite |
| Windbite | Regel prüft | Stormbite |
| Windbite | Regel sperrt vorher | Stormbite |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

keine

### Verlängerung, Aufbau, Umschalten

keine

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Soul Voice | Eigenschaft Soul Voice | Apex Arrow |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Burst Shot (1), Empyreal Arrow (1), Refulgent Arrow (1), Sidewinder (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BigShotPvE`, `DesperadoPvE`, `SagittariusArrowPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Foot Graze (`FootGrazePvE`, Ability): ungenutzt
- Leg Graze (`LegGrazePvE`, Ability): ungenutzt
