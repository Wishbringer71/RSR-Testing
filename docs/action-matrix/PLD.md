# PLD — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `PaladinRotation`
- Rotation: `RotationSolver/RebornRotations/Tank/PLD_Reborn.cs`
- Matrix als Tabelle: `PLD.csv`

## Nutzung

direkt: 43 · ungenutzt: 1 · über andere Aktion: 3

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
| Job | Atonement (`AtonementPvE`) | 16460 | Weaponskill | direkt |
| Job | Blade of Faith (`BladeOfFaithPvE`) | 25748 | Spell | über Confiteor |
| Job | Blade of Honor (`BladeOfHonorPvE`) | 36922 | Ability | direkt |
| Job | Blade of Truth (`BladeOfTruthPvE`) | 25749 | Spell | über Blade of Faith |
| Job | Blade of Valor (`BladeOfValorPvE`) | 25750 | Spell | über Blade of Truth |
| Job | Bulwark (`BulwarkPvE`) | 22 | Ability | direkt |
| Job | Circle of Scorn (`CircleOfScornPvE`) | 23 | Ability | direkt |
| Job | Clemency (`ClemencyPvE`) | 3541 | Spell | direkt |
| Job | Confiteor (`ConfiteorPvE`) | 16459 | Spell | direkt |
| Job | Cover (`CoverPvE`) | 27 | Ability | direkt |
| Job | Divine Veil (`DivineVeilPvE`) | 3540 | Ability | direkt |
| Job | Expiacion (`ExpiacionPvE`) | 25747 | Ability | direkt |
| Job | Fast Blade (`FastBladePvE`) | 9 | Weaponskill | direkt |
| Job | Fight or Flight (`FightOrFlightPvE`) | 20 | Ability | direkt |
| Job | Goring Blade (`GoringBladePvE`) | 3538 | Weaponskill | direkt |
| Job | Guardian (`GuardianPvE`) | 36920 | Ability | direkt |
| Job | Hallowed Ground (`HallowedGroundPvE`) | 30 | Ability | direkt |
| Job | Holy Circle (`HolyCirclePvE`) | 16458 | Spell | direkt |
| Job | Holy Sheltron (`HolySheltronPvE`) | 25746 | Ability | direkt |
| Job | Holy Spirit (`HolySpiritPvE`) | 7384 | Spell | direkt |
| Job | Imperator (`ImperatorPvE`) | 36921 | Ability | direkt |
| Job | Intervene (`IntervenePvE`) | 16461 | Ability | direkt |
| Job | Intervention (`InterventionPvE`) | 7382 | Ability | direkt |
| Job | Iron Will (`IronWillPvE`) | 28 | Ability | direkt |
| Job | Passage of Arms (`PassageOfArmsPvE`) | 7385 | Ability | direkt |
| Job | Prominence (`ProminencePvE`) | 16457 | Weaponskill | direkt |
| Job | Rage of Halone (`RageOfHalonePvE`) | 21 | Weaponskill | direkt |
| Job | Release Iron Will (`ReleaseIronWillPvE`) | 32065 | Ability | ungenutzt |
| Job | Requiescat (`RequiescatPvE`) | 7383 | Ability | direkt |
| Job | Riot Blade (`RiotBladePvE`) | 15 | Weaponskill | direkt |
| Job | Royal Authority (`RoyalAuthorityPvE`) | 3539 | Weaponskill | direkt |
| Job | Sentinel (`SentinelPvE`) | 17 | Ability | direkt |
| Job | Sepulchre (`SepulchrePvE`) | 36919 | Weaponskill | direkt |
| Job | Sheltron (`SheltronPvE`) | 3542 | Ability | direkt |
| Job | Shield Bash (`ShieldBashPvE`) | 16 | Weaponskill | direkt |
| Job | Shield Lob (`ShieldLobPvE`) | 24 | Weaponskill | direkt |
| Job | Spirits Within (`SpiritsWithinPvE`) | 29 | Ability | direkt |
| Job | Supplication (`SupplicationPvE`) | 36918 | Weaponskill | direkt |
| Job | Total Eclipse (`TotalEclipsePvE`) | 7381 | Weaponskill | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Atonement | braucht Atonement Ready | Eigenschaft Sword Oath |
| Atonement | Knopf wird zu | Supplication |
| Blade of Faith | Knopf wird zu | Blade of Truth |
| Blade of Faith | Combo nach | Confiteor |
| Blade of Honor | braucht Blade of Honor Ready (Erzeuger nicht im Text) | Blade of Honor Ready |
| Blade of Truth | Combo nach | Blade of Faith |
| Blade of Truth | Knopf wird zu | Blade of Valor |
| Blade of Valor | Combo nach | Blade of Truth |
| Confiteor | Knopf wird zu | Blade of Faith |
| Confiteor | braucht Confiteor Ready | Eigenschaft Enhanced Requiescat |
| Confiteor | braucht Confiteor Ready | Imperator |
| Cover | kostet | Oath |
| Goring Blade | braucht Goring Blade Ready | Eigenschaft Enhanced Fight or Flight |
| Holy Sheltron | kostet | Oath |
| Imperator | Knopf wird zu | Blade of Honor |
| Intervention | kostet | Oath |
| Prominence | Combo nach | Total Eclipse |
| Rage of Halone | Combo nach | Riot Blade |
| Rage of Halone | Ausbau (Rage of Halone Mastery) | Royal Authority |
| Requiescat | Ausbau (Requiescat Mastery) | Imperator |
| Riot Blade | Combo nach | Fast Blade |
| Royal Authority | Combo nach | Riot Blade |
| Sentinel | Ausbau (Sentinel Mastery) | Guardian |
| Sepulchre | braucht Sepulchre Ready | Supplication |
| Sheltron | Ausbau (Sheltron Mastery) | Holy Sheltron |
| Sheltron | kostet | Oath |
| Spirits Within | Ausbau (Spirits Within Mastery) | Expiacion |
| Supplication | braucht Supplication Ready | Atonement |
| Supplication | Knopf wird zu | Sepulchre |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Atonement | Regel prüft | Fight or Flight |
| Circle of Scorn | Regel prüft | Fight or Flight |
| Circle of Scorn | Regel prüft | Imperator |
| Confiteor | ActionCheck liest | Imperator |
| Confiteor | ActionCheck liest | Requiescat |
| Cover | Regel sperrt vorher | Intervention |
| Expiacion | Regel prüft | Fight or Flight |
| Expiacion | Regel prüft | Imperator |
| Fast Blade | Regel prüft | Rage of Halone |
| Fast Blade | Regel prüft | Riot Blade |
| Fast Blade | Regel prüft | Total Eclipse |
| Fight or Flight | Regel prüft | Atonement |
| Fight or Flight | Regel prüft | Fast Blade |
| Fight or Flight | Regel sperrt vorher | Intervention |
| Fight or Flight | Regel prüft | Prominence |
| Fight or Flight | Regel prüft | Rage of Halone |
| Fight or Flight | Regel prüft | Riot Blade |
| Fight or Flight | Regel prüft | Royal Authority |
| Fight or Flight | Regel prüft | Total Eclipse |
| Goring Blade | StatusNeed GoringBladeReady | Fight or Flight |
| Guardian | Regel prüft | Rampart |
| Guardian | Regel prüft | Sentinel |
| Holy Sheltron | Regel sperrt vorher | Intervention |
| Holy Spirit | Regel prüft | Supplication |
| Imperator | Regel prüft | Fight or Flight |
| Imperator | Regel sperrt vorher | Intervention |
| Intervention | StatusNeed Defiance | Status Defiance |
| Intervention | StatusNeed Grit | Status Grit |
| Intervention | StatusNeed IronWill | Status IronWill |
| Intervention | StatusNeed RoyalGuard_1833 | Status RoyalGuard_1833 |
| Prominence | ComboIds | Total Eclipse |
| Rage of Halone | ComboIds | Riot Blade |
| Rage of Halone | Regel prüft | Royal Authority |
| Rage of Halone | Regel prüft | Sepulchre |
| Rage of Halone | Regel prüft | Supplication |
| Rampart | Regel prüft | Guardian |
| Rampart | Regel prüft | Sentinel |
| Requiescat | Regel prüft | Fight or Flight |
| Requiescat | Regel prüft | Holy Circle |
| Requiescat | Regel sperrt vorher | Intervention |
| Riot Blade | ComboIds | Fast Blade |
| Riot Blade | Regel prüft | Fast Blade |
| Riot Blade | Regel prüft | Rage of Halone |
| Riot Blade | Regel prüft | Total Eclipse |
| Royal Authority | ComboIds | Riot Blade |
| Sentinel | Regel prüft | Guardian |
| Sentinel | Regel prüft | Rampart |
| Sepulchre | Regel prüft | Rage of Halone |
| Sheltron | Regel prüft | Holy Sheltron |
| Sheltron | Regel sperrt vorher | Intervention |
| Shield Bash | Regel prüft | Low Blow |
| Spirits Within | Regel prüft | Expiacion |
| Spirits Within | Regel prüft | Fight or Flight |
| Spirits Within | Regel prüft | Imperator |
| Supplication | Regel prüft | Rage of Halone |
| Total Eclipse | Regel prüft | Fast Blade |
| Total Eclipse | Regel prüft | Rage of Halone |
| Total Eclipse | Regel prüft | Riot Blade |

## Wechselwirkungen und Zeit

Aus den Wirktexten; Art je Aktion: Abwehr (bewertet in `DefensiveValues` oder Wirktext), Heilung, Angriff, sonstige. Bewertung im Konzept 14.

### Abwehr, Angriff und Heilung beenden oder sperren einander

| Aktion | Art | Befund | Gegenseite |
|---|---|---|---|
| Passage of Arms | Abwehr | endet bei jeder weiteren Aktion oder Bewegung (Kanal); Aktionssperre: PldlockCasting (aus) hält GCD, PldlockCasting (aus) hält Fähigkeit; Bewegungssperre: PosPassageOfArms (aus; wirkt nur mit PoslockCasting) | jede Aktion |

### Verlängerung, Aufbau, Umschalten

- Imperator: 4 Stapel Requiescat
- Requiescat: 4 Stapel Requiescat
- Umschalten (endet bei erneutem Einsatz): Iron Will

### Ressourcen: wer erzeugt, wer verbraucht

| Ressource | erzeugt von | verbraucht von |
|---|---|---|
| MP | Atonement, Eigenschaft Chivalry, Eigenschaft Enhanced Prominence, Expiacion, Sepulchre, Supplication | — |
| Oath | Eigenschaft Oath Mastery | Cover, Holy Sheltron, Intervention, Sheltron |

### Kandidaten für Selbsterhaltung

keine im Wirktext

### Unvollständige Beschreibungen

Der Wirktext lässt Werte aus, die eine Eigenschaft oder die Stufe setzt (Potenz, Dauer): Atonement (1), Blade of Faith (2), Blade of Truth (2), Blade of Valor (2), Confiteor (2), Fast Blade (1), Holy Spirit (1), Imperator (1), Intervention (1), Reprisal (1), Riot Blade (1), Royal Authority (1), Sepulchre (1), Sheltron (1), Supplication (1)

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `LastBastionPvE`, `ShieldWallPvE`, `StrongholdPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Release Iron Will (`ReleaseIronWillPvE`, Ability): ungenutzt
