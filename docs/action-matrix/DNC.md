# DNC — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `DancerRotation`
- Rotation: `RotationSolver/RebornRotations/Ranged/DNC_Reborn.cs`
- Matrix als Tabelle: `DNC.csv`

## Nutzung

direkt: 38 · ungenutzt: 4 · über andere Aktion: 6

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Fernkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Fernkämpfer | Foot Graze (`FootGrazePvE`) | 7553 | Ability | ungenutzt |
| Fernkämpfer | Head Graze (`HeadGrazePvE`) | 7551 | Ability | direkt |
| Fernkämpfer | Leg Graze (`LegGrazePvE`) | 7554 | Ability | ungenutzt |
| Fernkämpfer | Peloton (`PelotonPvE`) | 7557 | Ability | direkt |
| Fernkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Job | Bladeshower (`BladeshowerPvE`) | 15994 | Weaponskill | direkt |
| Job | Bloodshower (`BloodshowerPvE`) | 15996 | Weaponskill | direkt |
| Job | Cascade (`CascadePvE`) | 15989 | Weaponskill | direkt |
| Job | Closed Position (`ClosedPositionPvE`) | 16006 | Ability | direkt |
| Job | Curing Waltz (`CuringWaltzPvE`) | 16015 | Ability | direkt |
| Job | Dance of the Dawn (`DanceOfTheDawnPvE`) | 36985 | Weaponskill | direkt |
| Job | Devilment (`DevilmentPvE`) | 16011 | Ability | direkt |
| Job | Double Standard Finish (`DoubleStandardFinishPvE`) | 16192 | Weaponskill | direkt |
| Job | Double Technical Finish (`DoubleTechnicalFinishPvE`) | 16194 | Weaponskill | über Technical Step |
| Job | Emboite (`EmboitePvE`) | 15999 | Weaponskill | direkt |
| Job | En Avant (`EnAvantPvE`) | 16010 | Ability | direkt |
| Job | Ending (`EndingPvE`) | 18073 | Ability | ungenutzt |
| Job | Entrechat (`EntrechatPvE`) | 16000 | Weaponskill | direkt |
| Job | Fan Dance (`FanDancePvE`) | 16007 | Ability | direkt |
| Job | Fan Dance II (`FanDanceIiPvE`) | 16008 | Ability | direkt |
| Job | Fan Dance III (`FanDanceIiiPvE`) | 16009 | Ability | direkt |
| Job | Fan Dance IV (`FanDanceIvPvE`) | 25791 | Ability | direkt |
| Job | Finishing Move (`FinishingMovePvE`) | 36984 | Weaponskill | direkt |
| Job | Flourish (`FlourishPvE`) | 16013 | Ability | direkt |
| Job | Fountain (`FountainPvE`) | 15990 | Weaponskill | direkt |
| Job | Fountainfall (`FountainfallPvE`) | 15992 | Weaponskill | direkt |
| Job | Improvisation (`ImprovisationPvE`) | 16014 | Ability | direkt |
| Job | Improvised Finish (`ImprovisedFinishPvE`) | 25789 | Ability | ungenutzt — Knopfwechsel über Improvisation gesperrt: deren StatusProvide enthält Improvisation |
| Job | Jete (`JetePvE`) | 16001 | Weaponskill | direkt |
| Job | Last Dance (`LastDancePvE`) | 36983 | Weaponskill | direkt |
| Job | Pirouette (`PirouettePvE`) | 16002 | Weaponskill | direkt |
| Job | Quadruple Technical Finish (`QuadrupleTechnicalFinishPvE`) | 16196 | Weaponskill | direkt |
| Job | Reverse Cascade (`ReverseCascadePvE`) | 15991 | Weaponskill | direkt |
| Job | Rising Windmill (`RisingWindmillPvE`) | 15995 | Weaponskill | direkt |
| Job | Saber Dance (`SaberDancePvE`) | 16005 | Weaponskill | direkt |
| Job | Shield Samba (`ShieldSambaPvE`) | 16012 | Ability | direkt |
| Job | Single Standard Finish (`SingleStandardFinishPvE`) | 16191 | Weaponskill | über Standard Step |
| Job | Single Technical Finish (`SingleTechnicalFinishPvE`) | 16193 | Weaponskill | über Technical Step |
| Job | Standard Finish (`StandardFinishPvE`) | 16003 | Weaponskill | über Standard Step |
| Job | Standard Step (`StandardStepPvE`) | 15997 | Weaponskill | direkt |
| Job | Starfall Dance (`StarfallDancePvE`) | 25792 | Weaponskill | direkt |
| Job | Technical Finish (`TechnicalFinishPvE`) | 16004 | Weaponskill | über Technical Step |
| Job | Technical Step (`TechnicalStepPvE`) | 15998 | Weaponskill | direkt |
| Job | Tillana (`TillanaPvE`) | 25790 | Weaponskill | direkt |
| Job | Triple Technical Finish (`TripleTechnicalFinishPvE`) | 16195 | Weaponskill | über Technical Step |
| Job | Windmill (`WindmillPvE`) | 15993 | Weaponskill | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Bladeshower | Knopf wird zu | Entrechat |
| Bladeshower | Combo nach | Windmill |
| Bloodshower | Knopf wird zu | Pirouette |
| Bloodshower | braucht Silken Flow (Erzeuger nicht im Text) | Silken Flow |
| Cascade | Knopf wird zu | Emboite |
| Dance of the Dawn | braucht Dance of the Dawn Ready (Erzeuger nicht im Text) | Dance of the Dawn Ready |
| Dance of the Dawn | kostet | Esprit |
| Double Standard Finish | Bedingung (kein Status) | dancing |
| Double Technical Finish | Bedingung (kein Status) | dancing |
| Emboite | braucht Dancing (Erzeuger nicht im Text) | Dancing |
| Entrechat | braucht Dancing (Erzeuger nicht im Text) | Dancing |
| Fan Dance II | Bedingung (kein Status) | possession of Fourfold Feathers |
| Fan Dance III | braucht Threefold Fan Dance | Fan Dance |
| Fan Dance IV | braucht Fourfold Fan Dance | Eigenschaft Enhanced Flourish |
| Fan Dance | Bedingung (kein Status) | possession of Fourfold Feathers |
| Finishing Move | braucht Finishing Move Ready | Eigenschaft Enhanced Flourish II |
| Finishing Move | gemeinsame Abklingzeit | Standard Step |
| Flourish | Bedingung (kein Status) | combat |
| Fountain | Combo nach | Cascade |
| Fountain | Knopf wird zu | Entrechat |
| Fountainfall | Knopf wird zu | Pirouette |
| Fountainfall | braucht Silken Flow (Erzeuger nicht im Text) | Silken Flow |
| Improvisation | Knopf wird zu | Improvised Finish |
| Improvised Finish | braucht Improvisation | Improvisation |
| Jete | braucht Dancing (Erzeuger nicht im Text) | Dancing |
| Last Dance | braucht Last Dance Ready | Eigenschaft Enhanced Standard Finish |
| Last Dance | braucht Last Dance Ready | Finishing Move |
| Pirouette | braucht Dancing (Erzeuger nicht im Text) | Dancing |
| Quadruple Technical Finish | Bedingung (kein Status) | dancing |
| Reverse Cascade | Knopf wird zu | Jete |
| Reverse Cascade | braucht Silken Symmetry (Erzeuger nicht im Text) | Silken Symmetry |
| Rising Windmill | Knopf wird zu | Jete |
| Rising Windmill | braucht Silken Symmetry (Erzeuger nicht im Text) | Silken Symmetry |
| Saber Dance | Knopf wird zu | Dance of the Dawn |
| Saber Dance | kostet | Esprit |
| Single Standard Finish | Bedingung (kein Status) | dancing |
| Single Technical Finish | Bedingung (kein Status) | dancing |
| Standard Finish | braucht Dancing (Erzeuger nicht im Text) | Dancing |
| Standard Step | Knopf wird zu | Double Standard Finish |
| Standard Step | Knopf wird zu | Finishing Move |
| Standard Step | Knopf wird zu | Single Standard Finish |
| Standard Step | Knopf wird zu | Standard Finish |
| Starfall Dance | braucht Flourishing Starfall | Eigenschaft Enhanced Devilment |
| Technical Finish | braucht Dancing (Erzeuger nicht im Text) | Dancing |
| Technical Finish | Knopf wird zu | Tillana |
| Technical Step | Knopf wird zu | Double Technical Finish |
| Technical Step | Knopf wird zu | Quadruple Technical Finish |
| Technical Step | Knopf wird zu | Single Technical Finish |
| Technical Step | Knopf wird zu | Technical Finish |
| Technical Step | Knopf wird zu | Triple Technical Finish |
| Tillana | braucht Flourishing Finish | Eigenschaft Enhanced Technical Finish |
| Triple Technical Finish | Bedingung (kein Status) | dancing |
| Windmill | Knopf wird zu | Emboite |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Bladeshower | Regel prüft | Last Dance |
| Bloodshower | Regel prüft | Last Dance |
| Cascade | Regel prüft | Last Dance |
| Double Standard Finish | ActionCheck liest | Standard Step |
| Double Technical Finish | ActionCheck liest | Technical Step |
| Fan Dance II | Regel prüft | Devilment |
| Fan Dance II | Regel sperrt vorher | Standard Step |
| Fan Dance II | Regel sperrt vorher | Technical Step |
| Fan Dance III | Regel sperrt vorher | Standard Step |
| Fan Dance III | Regel sperrt vorher | Technical Step |
| Fan Dance IV | Regel sperrt vorher | Standard Step |
| Fan Dance IV | Regel sperrt vorher | Technical Step |
| Fan Dance | Regel prüft | Devilment |
| Fan Dance | Regel sperrt vorher | Standard Step |
| Fan Dance | Regel sperrt vorher | Technical Step |
| Finishing Move | Regel prüft | Devilment |
| Flourish | Regel sperrt vorher | Standard Step |
| Flourish | Regel prüft | Technical Step |
| Flourish | Regel sperrt vorher | Technical Step |
| Flourish | Regel prüft | Tillana |
| Fountain | ComboIds | Cascade |
| Fountain | Regel prüft | Last Dance |
| Fountainfall | Regel prüft | Last Dance |
| Quadruple Technical Finish | ActionCheck liest | Technical Step |
| Reverse Cascade | Regel prüft | Last Dance |
| Rising Windmill | Regel prüft | Last Dance |
| Saber Dance | Regel prüft | Technical Step |
| Single Technical Finish | ActionCheck liest | Technical Step |
| Starfall Dance | Regel prüft | Devilment |
| Technical Finish | ActionCheck liest | Technical Step |
| Technical Step | Regel sperrt vorher | Closed Position |
| Tillana | Regel prüft | Devilment |
| Tillana | Regel sperrt vorher | Standard Step |
| Tillana | Regel prüft | Technical Step |
| Tillana | Regel sperrt vorher | Technical Step |
| Triple Technical Finish | ActionCheck liest | Technical Step |
| Windmill | Regel prüft | Last Dance |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Finishing Move | Angriff | gemeinsame Abklingzeit (Angriff / sonstige) | Standard Step |
| Improvisation | Heilung | endet bei jeder weiteren Aktion oder Bewegung (Kanal); RSR-Sperre: PosImprovisation (aus) hält Bewegung | jede Aktion |

### Verlängerung, Aufbau, Umschalten

- Umschalten (endet bei erneutem Einsatz): Closed Position

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Esprit | Eigenschaft Enhanced Esprit, Eigenschaft Esprit, Tillana | Dance of the Dawn, Saber Dance |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Cascade (1), Emboite (1), Entrechat (1), Fan Dance (1), Fan Dance III (1), Fan Dance IV (1), Fountain (1), Fountainfall (1), Jete (1), Pirouette (1), Reverse Cascade (1), Saber Dance (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BigShotPvE`, `CrimsonLotusPvE`, `DesperadoPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Ending (`EndingPvE`, Ability): ungenutzt
- Foot Graze (`FootGrazePvE`, Ability): ungenutzt
- Improvised Finish (`ImprovisedFinishPvE`, Ability): ungenutzt — Knopfwechsel über Improvisation gesperrt: deren StatusProvide enthält Improvisation
- Leg Graze (`LegGrazePvE`, Ability): ungenutzt
