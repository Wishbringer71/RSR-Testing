# RDM — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `RedMageRotation`
- Rotation: `RotationSolver/RebornRotations/Magical/RDM_Reborn.cs`
- Matrix als Tabelle: `RDM.csv`

## Nutzung

direkt: 44 · nur gelesen: 5 · ungenutzt: 1 · über andere Aktion: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Magier | Addle (`AddlePvE`) | 7560 | Ability | direkt |
| Magier | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Magier | Sleep (`SleepPvE`) | 25880 | Spell | ungenutzt |
| Magier | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Magier | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Acceleration (`AccelerationPvE`) | 7518 | Ability | direkt |
| Job | Contre Sixte (`ContreSixtePvE`) | 7519 | Ability | direkt |
| Job | Corps-a-corps (`CorpsacorpsPvE`) | 7506 | Ability | direkt |
| Job | Displacement (`DisplacementPvE`) | 7515 | Ability | direkt |
| Job | Embolden (`EmboldenPvE`) | 7520 | Ability | direkt |
| Job | Enchanted Moulinet (`EnchantedMoulinetPvE`) | 7530 | Weaponskill | direkt |
| Job | Enchanted Moulinet Deux (`EnchantedMoulinetDeuxPvE`) | 37002 | Weaponskill | direkt |
| Job | Enchanted Moulinet Trois (`EnchantedMoulinetTroisPvE`) | 37003 | Weaponskill | direkt |
| Job | Enchanted Redoublement (`EnchantedRedoublementPvE`) | 7529 | Weaponskill | direkt |
| Job | Enchanted Redoublement (`EnchantedRedoublementPvE_45962`) | 45962 | Weaponskill | direkt |
| Job | Enchanted Reprise (`EnchantedReprisePvE`) | 16528 | Weaponskill | direkt |
| Job | Enchanted Riposte (`EnchantedRipostePvE`) | 7527 | Weaponskill | direkt |
| Job | Enchanted Riposte (`EnchantedRipostePvE_45960`) | 45960 | Weaponskill | direkt |
| Job | Enchanted Zwerchhau (`EnchantedZwerchhauPvE`) | 7528 | Weaponskill | direkt |
| Job | Enchanted Zwerchhau (`EnchantedZwerchhauPvE_45961`) | 45961 | Weaponskill | direkt |
| Job | Engagement (`EngagementPvE`) | 16527 | Ability | direkt |
| Job | Fleche (`FlechePvE`) | 7517 | Ability | direkt |
| Job | Grand Impact (`GrandImpactPvE`) | 37006 | Spell | direkt |
| Job | Impact (`ImpactPvE`) | 16526 | Spell | direkt |
| Job | Jolt (`JoltPvE`) | 7503 | Spell | direkt |
| Job | Jolt II (`JoltIiPvE`) | 7524 | Spell | über Jolt |
| Job | Jolt III (`JoltIiiPvE`) | 37004 | Spell | über Jolt II |
| Job | Magick Barrier (`MagickBarrierPvE`) | 25857 | Ability | direkt |
| Job | Manafication (`ManaficationPvE`) | 7521 | Ability | direkt |
| Job | Moulinet (`MoulinetPvE`) | 7513 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Prefulgence (`PrefulgencePvE`) | 37007 | Ability | direkt |
| Job | Redoublement (`RedoublementPvE`) | 7516 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Reprise (`ReprisePvE`) | 16529 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Resolution (`ResolutionPvE`) | 25858 | Spell | direkt |
| Job | Riposte (`RipostePvE`) | 7504 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Scatter (`ScatterPvE`) | 7509 | Spell | direkt |
| Job | Scorch (`ScorchPvE`) | 16530 | Spell | direkt |
| Job | Veraero (`VeraeroPvE`) | 7507 | Spell | direkt |
| Job | Veraero II (`VeraeroIiPvE`) | 16525 | Spell | direkt |
| Job | Veraero III (`VeraeroIiiPvE`) | 25856 | Spell | direkt |
| Job | Vercure (`VercurePvE`) | 7514 | Spell | direkt |
| Job | Verfire (`VerfirePvE`) | 7510 | Spell | direkt |
| Job | Verflare (`VerflarePvE`) | 7525 | Spell | direkt |
| Job | Verholy (`VerholyPvE`) | 7526 | Spell | direkt |
| Job | Verraise (`VerraisePvE`) | 7523 | Spell | direkt |
| Job | Verstone (`VerstonePvE`) | 7511 | Spell | direkt |
| Job | Verthunder (`VerthunderPvE`) | 7505 | Spell | direkt |
| Job | Verthunder II (`VerthunderIiPvE`) | 16524 | Spell | direkt |
| Job | Verthunder III (`VerthunderIiiPvE`) | 25855 | Spell | direkt |
| Job | Vice of Thorns (`ViceOfThornsPvE`) | 37005 | Ability | direkt |
| Job | Zwerchhau (`ZwerchhauPvE`) | 7512 | Weaponskill | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Displacement | gemeinsame Abklingzeit | Engagement |
| Enchanted Moulinet Deux | kostet | Black Mana |
| Enchanted Moulinet Deux | Combo nach | Enchanted Moulinet |
| Enchanted Moulinet Deux | Knopf wird zu | Enchanted Moulinet Trois |
| Enchanted Moulinet Deux | kostet | White Mana |
| Enchanted Moulinet | kostet | Black Mana |
| Enchanted Moulinet | Knopf wird zu | Enchanted Moulinet Deux |
| Enchanted Moulinet | kostet | White Mana |
| Enchanted Moulinet Trois | kostet | Black Mana |
| Enchanted Moulinet Trois | Combo nach | Enchanted Moulinet Deux |
| Enchanted Moulinet Trois | kostet | White Mana |
| Enchanted Redoublement | kostet | Black Mana |
| Enchanted Redoublement | Combo nach | Enchanted Zwerchhau |
| Enchanted Redoublement | Combo nach | Enchanted Zwerchhau |
| Enchanted Redoublement | kostet | White Mana |
| Enchanted Redoublement | kostet | Black Mana |
| Enchanted Redoublement | Combo nach | Enchanted Zwerchhau |
| Enchanted Redoublement | Combo nach | Enchanted Zwerchhau |
| Enchanted Redoublement | kostet | White Mana |
| Enchanted Reprise | kostet | Black Mana |
| Enchanted Reprise | kostet | White Mana |
| Enchanted Riposte | kostet | Black Mana |
| Enchanted Riposte | kostet | White Mana |
| Enchanted Riposte | kostet | Black Mana |
| Enchanted Riposte | kostet | White Mana |
| Enchanted Zwerchhau | kostet | Black Mana |
| Enchanted Zwerchhau | Combo nach | Enchanted Riposte |
| Enchanted Zwerchhau | Combo nach | Enchanted Riposte |
| Enchanted Zwerchhau | Combo nach | Riposte |
| Enchanted Zwerchhau | kostet | White Mana |
| Enchanted Zwerchhau | kostet | Black Mana |
| Enchanted Zwerchhau | Combo nach | Enchanted Riposte |
| Enchanted Zwerchhau | Combo nach | Enchanted Riposte |
| Enchanted Zwerchhau | Combo nach | Riposte |
| Enchanted Zwerchhau | kostet | White Mana |
| Engagement | gemeinsame Abklingzeit | Displacement |
| Grand Impact | braucht Grand Impact Ready | Eigenschaft Enhanced Acceleration II |
| Impact | Knopf wird zu | Grand Impact |
| Impact | Knopf wird zu | Scorch |
| Jolt II | Ausbau (Red Magic Mastery III) | Jolt III |
| Jolt III | Knopf wird zu | Grand Impact |
| Jolt | Ausbau (Enhanced Jolt) | Jolt II |
| Manafication | Bedingung (kein Status) | combat |
| Moulinet | Knopf wird zu | Enchanted Moulinet |
| Prefulgence | braucht Prefulgence Ready | Eigenschaft Enhanced Manafication |
| Redoublement | Knopf wird zu | Enchanted Redoublement |
| Redoublement | Knopf wird zu | Enchanted Redoublement |
| Redoublement | Combo nach | Enchanted Zwerchhau |
| Redoublement | Combo nach | Enchanted Zwerchhau |
| Redoublement | Combo nach | Zwerchhau |
| Reprise | Knopf wird zu | Enchanted Reprise |
| Resolution | Combo nach | Scorch |
| Riposte | Knopf wird zu | Enchanted Riposte |
| Riposte | Knopf wird zu | Enchanted Riposte |
| Scatter | Ausbau (Scatter Mastery) | Impact |
| Scorch | Knopf wird zu | Resolution |
| Scorch | Combo nach | Verflare |
| Scorch | Combo nach | Verholy |
| Veraero II | Ausbau (Mana Stack) | Verholy |
| Veraero II | Knopf wird zu | Verholy |
| Veraero | Ausbau (Red Magic Mastery II) | Veraero III |
| Veraero | Ausbau (Mana Stack) | Verholy |
| Verfire | braucht Verfire Ready (Erzeuger nicht im Text) | Verfire Ready |
| Verflare | kostet | Mana Stack |
| Verholy | kostet | Mana Stack |
| Verstone | braucht Verstone Ready (Erzeuger nicht im Text) | Verstone Ready |
| Verthunder II | Knopf wird zu | Verflare |
| Verthunder | Ausbau (Red Magic Mastery II) | Verthunder III |
| Vice of Thorns | braucht Thorned Flourish | Eigenschaft Enhanced Embolden |
| Zwerchhau | Combo nach | Enchanted Riposte |
| Zwerchhau | Combo nach | Enchanted Riposte |
| Zwerchhau | Knopf wird zu | Enchanted Zwerchhau |
| Zwerchhau | Knopf wird zu | Enchanted Zwerchhau |
| Zwerchhau | Combo nach | Riposte |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Acceleration | Regel prüft | Embolden |
| Acceleration | Regel prüft | Enchanted Reprise |
| Acceleration | Regel prüft | Grand Impact |
| Acceleration | Regel prüft | Swiftcast |
| Corps-a-corps | Regel prüft | Embolden |
| Displacement | Regel prüft | Engagement |
| Enchanted Moulinet Deux | ComboIds | Enchanted Moulinet |
| Enchanted Moulinet Trois | ComboIds | Enchanted Moulinet Deux |
| Enchanted Redoublement | ComboIds | Enchanted Zwerchhau |
| Enchanted Redoublement | ComboIds | Enchanted Zwerchhau |
| Enchanted Reprise | Regel prüft | Acceleration |
| Enchanted Reprise | Regel prüft | Grand Impact |
| Enchanted Reprise | Regel prüft | Swiftcast |
| Enchanted Riposte | Regel prüft | Enchanted Riposte |
| Enchanted Zwerchhau | ComboIds | Enchanted Riposte |
| Enchanted Zwerchhau | ComboIds | Enchanted Riposte |
| Engagement | Regel prüft | Embolden |
| Grand Impact | Regel prüft | Acceleration |
| Grand Impact | Regel prüft | Enchanted Reprise |
| Grand Impact | StatusNeed GrandImpactReady | Status GrandImpactReady |
| Grand Impact | Regel prüft | Swiftcast |
| Manafication | Regel prüft | Embolden |
| Prefulgence | StatusNeed PrefulgenceReady | Status PrefulgenceReady |
| Redoublement | ComboIds | Zwerchhau |
| Resolution | Regel prüft | Riposte |
| Resolution | ComboIds | Scorch |
| Resolution | Regel prüft | Scorch |
| Resolution | Regel prüft | Zwerchhau |
| Scatter | Regel prüft | Impact |
| Scorch | Regel prüft | Resolution |
| Scorch | Regel prüft | Riposte |
| Scorch | ComboIds | Verfire |
| Scorch | ComboIds | Verholy |
| Scorch | Regel prüft | Zwerchhau |
| Swiftcast | Regel prüft | Acceleration |
| Swiftcast | Regel prüft | Embolden |
| Swiftcast | Regel prüft | Enchanted Reprise |
| Swiftcast | Regel prüft | Grand Impact |
| Swiftcast | Regel prüft | Veraero III |
| Swiftcast | Regel prüft | Veraero |
| Swiftcast | Regel prüft | Verthunder III |
| Swiftcast | Regel prüft | Verthunder |
| Veraero | Regel prüft | Veraero III |
| Vercure | Regel sperrt vorher | Resolution |
| Vercure | Regel sperrt vorher | Riposte |
| Vercure | Regel sperrt vorher | Scorch |
| Vercure | Regel sperrt vorher | Zwerchhau |
| Verfire | Regel prüft | Verstone |
| Verfire | StatusNeed VerfireReady | Verthunder III |
| Verfire | StatusNeed VerfireReady | Verthunder |
| Verflare | ComboIds | Enchanted Moulinet Trois |
| Verflare | ComboIds | Enchanted Redoublement |
| Verflare | ComboIds | Enchanted Redoublement |
| Verflare | ComboIds | Redoublement |
| Verholy | ComboIds | Enchanted Moulinet Trois |
| Verholy | ComboIds | Enchanted Redoublement |
| Verholy | ComboIds | Enchanted Redoublement |
| Verholy | ComboIds | Redoublement |
| Verraise | StatusNeed Dualcast | Jolt |
| Verraise | Regel sperrt vorher | Resolution |
| Verraise | Regel sperrt vorher | Riposte |
| Verraise | Regel sperrt vorher | Scorch |
| Verraise | StatusNeed Dualcast | Vercure |
| Verraise | Regel sperrt vorher | Zwerchhau |
| Verstone | StatusNeed VerstoneReady | Veraero III |
| Verstone | StatusNeed VerstoneReady | Veraero |
| Verthunder | Regel prüft | Verthunder III |
| Vice of Thorns | StatusNeed ThornedFlourish | Status ThornedFlourish |
| Zwerchhau | ComboIds | Riposte |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `SkyshardPvE`, `StarstormPvE`, `VermilionScourgePvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Moulinet (`MoulinetPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Redoublement (`RedoublementPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Reprise (`ReprisePvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Riposte (`RipostePvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
- Sleep (`SleepPvE`, Spell): ungenutzt
- Zwerchhau (`ZwerchhauPvE`, Weaponskill): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
