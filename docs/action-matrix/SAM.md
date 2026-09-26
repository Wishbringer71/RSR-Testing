# SAM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `SamuraiRotation`
- Rotation: `RotationSolver/RebornRotations/Melee/SAM_Reborn.cs`
- Matrix als Tabelle: `SAM.csv`

## Nutzung

direkt: 43 · nur gelesen: 2 · über andere Aktion: 1

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Nahkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Nahkämpfer | Bloodbath (`BloodbathPvE`) | 7542 | Ability | direkt |
| Nahkämpfer | Feint (`FeintPvE`) | 7549 | Ability | direkt |
| Nahkämpfer | Leg Sweep (`LegSweepPvE`) | 7863 | Ability | direkt |
| Nahkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Nahkämpfer | True North (`TrueNorthPvE`) | 7546 | Ability | direkt |
| Job | Enpi (`EnpiPvE`) | 7486 | Weaponskill | direkt |
| Job | Fuga (`FugaPvE`) | 7483 | Weaponskill | direkt |
| Job | Fuko (`FukoPvE`) | 25780 | Weaponskill | direkt |
| Job | Gekko (`GekkoPvE`) | 7481 | Weaponskill | direkt |
| Job | Gyofu (`GyofuPvE`) | 36963 | Weaponskill | über Hakaze |
| Job | Hagakure (`HagakurePvE`) | 7495 | Ability | direkt |
| Job | Hakaze (`HakazePvE`) | 7477 | Weaponskill | direkt |
| Job | Higanbana (`HiganbanaPvE`) | 7489 | Weaponskill | direkt |
| Job | Hissatsu: Guren (`HissatsuGurenPvE`) | 7496 | Ability | direkt |
| Job | Hissatsu: Gyoten (`HissatsuGyotenPvE`) | 7492 | Ability | direkt |
| Job | Hissatsu: Kyuten (`HissatsuKyutenPvE`) | 7491 | Ability | direkt |
| Job | Hissatsu: Senei (`HissatsuSeneiPvE`) | 16481 | Ability | direkt |
| Job | Hissatsu: Shinten (`HissatsuShintenPvE`) | 7490 | Ability | direkt |
| Job | Hissatsu: Yaten (`HissatsuYatenPvE`) | 7493 | Ability | direkt |
| Job | Iaijutsu (`IaijutsuPvE`) | 7867 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Ikishoten (`IkishotenPvE`) | 16482 | Ability | direkt |
| Job | Jinpu (`JinpuPvE`) | 7478 | Weaponskill | direkt |
| Job | Kaeshi: Goken (`KaeshiGokenPvE`) | 16485 | Weaponskill | direkt |
| Job | Kaeshi: Namikiri (`KaeshiNamikiriPvE`) | 25782 | Weaponskill | direkt |
| Job | Kaeshi: Setsugekka (`KaeshiSetsugekkaPvE`) | 16486 | Weaponskill | direkt |
| Job | Kasha (`KashaPvE`) | 7482 | Weaponskill | direkt |
| Job | Mangetsu (`MangetsuPvE`) | 7484 | Weaponskill | direkt |
| Job | Meditate (`MeditatePvE`) | 7497 | Ability | direkt |
| Job | Meikyo Shisui (`MeikyoShisuiPvE`) | 7499 | Ability | direkt |
| Job | Midare Setsugekka (`MidareSetsugekkaPvE`) | 7487 | Weaponskill | direkt |
| Job | Ogi Namikiri (`OgiNamikiriPvE`) | 25781 | Weaponskill | direkt |
| Job | Oka (`OkaPvE`) | 7485 | Weaponskill | direkt |
| Job | Shifu (`ShifuPvE`) | 7479 | Weaponskill | direkt |
| Job | Shoha (`ShohaPvE`) | 16487 | Ability | direkt |
| Job | Tendo Goken (`TendoGokenPvE`) | 36965 | Weaponskill | direkt |
| Job | Tendo Kaeshi Goken (`TendoKaeshiGokenPvE`) | 36967 | Weaponskill | direkt |
| Job | Tendo Kaeshi Setsugekka (`TendoKaeshiSetsugekkaPvE`) | 36968 | Weaponskill | direkt |
| Job | Tendo Setsugekka (`TendoSetsugekkaPvE`) | 36966 | Weaponskill | direkt |
| Job | Tengentsu (`TengentsuPvE`) | 36962 | Ability | direkt |
| Job | Tenka Goken (`TenkaGokenPvE`) | 7488 | Weaponskill | direkt |
| Job | Third Eye (`ThirdEyePvE`) | 7498 | Ability | direkt |
| Job | Tsubame-gaeshi (`TsubamegaeshiPvE`) | 16483 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Yukikaze (`YukikazePvE`) | 7480 | Weaponskill | direkt |
| Job | Zanshin (`ZanshinPvE`) | 36964 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Fuga | Ausbau (Fuga Mastery) | Fuko |
| Gekko | Combo nach | Jinpu |
| Hakaze | Ausbau (Hakaze Mastery) | Gyofu |
| Hissatsu: Guren | kostet | Kenki |
| Hissatsu: Gyoten | kostet | Kenki |
| Hissatsu: Kyuten | kostet | Kenki |
| Hissatsu: Senei | gemeinsame Abklingzeit | Hissatsu: Guren |
| Hissatsu: Senei | kostet | Kenki |
| Hissatsu: Shinten | kostet | Kenki |
| Hissatsu: Yaten | kostet | Kenki |
| Iaijutsu | Knopf wird zu | Higanbana |
| Iaijutsu | Knopf wird zu | Midare Setsugekka |
| Iaijutsu | Knopf wird zu | Tendo Goken |
| Iaijutsu | Knopf wird zu | Tendo Setsugekka |
| Iaijutsu | Knopf wird zu | Tenka Goken |
| Ikishoten | Bedingung (kein Status) | combat |
| Kasha | Combo nach | Shifu |
| Ogi Namikiri | braucht Ogi Namikiri Ready | Eigenschaft Enhanced Ikishoten |
| Ogi Namikiri | Knopf wird zu | Kaeshi: Namikiri |
| Tendo Goken | braucht Tendo and after accumulating 2 Sen (Erzeuger nicht im Text) | Tendo and after accumulating 2 Sen |
| Tendo Setsugekka | braucht Tendo and after accumulating 3 Sen (Erzeuger nicht im Text) | Tendo and after accumulating 3 Sen |
| Third Eye | Ausbau (Third Eye Mastery) | Tengentsu |
| Tsubame-gaeshi | braucht Tsubame-gaeshi Ready | Eigenschaft Enhanced Iaijutsu |
| Tsubame-gaeshi | Knopf wird zu | Kaeshi: Goken |
| Tsubame-gaeshi | Knopf wird zu | Kaeshi: Setsugekka |
| Tsubame-gaeshi | braucht Tsubame-gaeshi Ready | Tendo Goken |
| Tsubame-gaeshi | Knopf wird zu | Tendo Kaeshi Goken |
| Tsubame-gaeshi | Knopf wird zu | Tendo Kaeshi Setsugekka |
| Tsubame-gaeshi | braucht Tsubame-gaeshi Ready | Tendo Setsugekka |
| Zanshin | braucht Zanshin Ready | Eigenschaft Enhanced Ikishoten II |
| Zanshin | kostet | Kenki |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Gekko | ComboIds | Jinpu |
| Hagakure | Regel prüft | Midare Setsugekka |
| Higanbana | Regel sperrt vorher | Ogi Namikiri |
| Hissatsu: Guren | Regel prüft | Hissatsu: Senei |
| Hissatsu: Guren | Regel prüft | Ikishoten |
| Hissatsu: Guren | Regel sperrt vorher | Tendo Kaeshi Setsugekka |
| Hissatsu: Kyuten | Regel prüft | Ikishoten |
| Hissatsu: Kyuten | Regel sperrt vorher | Tendo Kaeshi Setsugekka |
| Hissatsu: Senei | Regel prüft | Ikishoten |
| Hissatsu: Senei | Regel sperrt vorher | Tendo Kaeshi Setsugekka |
| Hissatsu: Shinten | Regel prüft | Ikishoten |
| Hissatsu: Shinten | Regel sperrt vorher | Tendo Kaeshi Setsugekka |
| Ikishoten | Regel sperrt vorher | Tendo Kaeshi Setsugekka |
| Jinpu | ComboIds | Gyofu |
| Jinpu | ComboIds | Hakaze |
| Kaeshi: Goken | StatusNeed TsubamegaeshiReady | Status TsubamegaeshiReady |
| Kaeshi: Setsugekka | StatusNeed Tsubamegaeshi | Tendo Setsugekka |
| Kasha | ComboIds | Shifu |
| Mangetsu | ComboIds | Fuko |
| Mangetsu | Regel prüft | Oka |
| Meikyo Shisui | Regel prüft | Higanbana |
| Midare Setsugekka | Regel prüft | Hagakure |
| Ogi Namikiri | StatusNeed OgiNamikiriReady | Ikishoten |
| Oka | ComboIds | Fuko |
| Shifu | ComboIds | Gyofu |
| Shifu | ComboIds | Hakaze |
| Shoha | Regel prüft | Ikishoten |
| Shoha | Regel sperrt vorher | Tendo Kaeshi Setsugekka |
| Tendo Goken | StatusNeed Tendo | Meikyo Shisui |
| Tendo Kaeshi Goken | StatusNeed Tsubamegaeshi_4217 | Status Tsubamegaeshi_4217 |
| Tendo Kaeshi Setsugekka | StatusNeed Tsubamegaeshi_4218 | Status Tsubamegaeshi_4218 |
| Tendo Setsugekka | StatusNeed Tendo | Meikyo Shisui |
| Yukikaze | ComboIds | Gyofu |
| Yukikaze | ComboIds | Hakaze |
| Zanshin | StatusNeed ZanshinReady_3855 | Status ZanshinReady_3855 |
| Zanshin | Regel sperrt vorher | Tendo Kaeshi Setsugekka |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Meditate | sonstige | endet bei jeder weiteren Aktion oder Bewegung (Kanal); keine Aktionssperre: RSRs nächste Aktion beendet ihn | jede Aktion |

### Verlängerung, Aufbau, Umschalten

keine

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| Kenki | Fuko, Gyofu, Ikishoten, Tengentsu | Hissatsu: Guren, Hissatsu: Gyoten, Hissatsu: Kyuten, Hissatsu: Senei, Hissatsu: Shinten, Hissatsu: Yaten, Zanshin |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Fenster, deren Verbraucher an Bedingungen hängt

Jeder Aufruf der Aktion trägt eine eigene Bedingung; hält sie an, verfällt das Fenster (Konzept 14, „Werden die Fenster genutzt"). Kandidaten, von Hand bewertet.

- Ogi Namikiri: 2 Aufruf(e), mit Rückfall vor Ablauf

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Feint (1), Gekko (3), Hakaze (1), Higanbana (1), Jinpu (2), Kaeshi: Namikiri (1), Kaeshi: Setsugekka (1), Kasha (3), Midare Setsugekka (1), Ogi Namikiri (1), Shifu (2), Shoha (1), Yukikaze (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BladedancePvE`, `BraverPvE`, `DoomOfTheLivingPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Iaijutsu (`IaijutsuPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Tsubame-gaeshi (`TsubamegaeshiPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
