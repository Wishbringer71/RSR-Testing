# SGE — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `SageRotation`
- Rotation: `RotationSolver/RebornRotations/Healer/SGE_Reborn.cs`
- Matrix als Tabelle: `SGE.csv`

## Nutzung

direkt: 40 · ungenutzt: 2 · über andere Aktion: 5

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Heiler | Esuna (`EsunaPvE`) | 7568 | Spell | direkt |
| Heiler | Lucid Dreaming (`LucidDreamingPvE`) | 7562 | Ability | direkt |
| Heiler | Repose (`ReposePvE`) | 16560 | Spell | ungenutzt |
| Heiler | Rescue (`RescuePvE`) | 7571 | Ability | ungenutzt |
| Heiler | Surecast (`SurecastPvE`) | 7559 | Ability | direkt |
| Heiler | Swiftcast (`SwiftcastPvE`) | 7561 | Ability | direkt |
| Job | Diagnosis (`DiagnosisPvE`) | 24284 | Spell | direkt |
| Job | Dosis (`DosisPvE`) | 24283 | Spell | direkt |
| Job | Dosis II (`DosisIiPvE`) | 24306 | Spell | über Dosis |
| Job | Dosis III (`DosisIiiPvE`) | 24312 | Spell | über Dosis II |
| Job | Druochole (`DruocholePvE`) | 24296 | Ability | direkt |
| Job | Dyskrasia (`DyskrasiaPvE`) | 24297 | Spell | direkt |
| Job | Dyskrasia II (`DyskrasiaIiPvE`) | 24315 | Spell | über Dyskrasia |
| Job | Egeiro (`EgeiroPvE`) | 24287 | Spell | direkt |
| Job | Eukrasia (`EukrasiaPvE`) | 24290 | Spell | direkt |
| Job | Eukrasian Diagnosis (`EukrasianDiagnosisPvE`) | 24291 | Spell | direkt |
| Job | Eukrasian Dosis (`EukrasianDosisPvE`) | 24293 | Spell | direkt |
| Job | Eukrasian Dosis II (`EukrasianDosisIiPvE`) | 24308 | Spell | direkt |
| Job | Eukrasian Dosis III (`EukrasianDosisIiiPvE`) | 24314 | Spell | direkt |
| Job | Eukrasian Dyskrasia (`EukrasianDyskrasiaPvE`) | 37032 | Spell | direkt |
| Job | Eukrasian Prognosis (`EukrasianPrognosisPvE`) | 24292 | Spell | direkt |
| Job | Eukrasian Prognosis II (`EukrasianPrognosisIiPvE`) | 37034 | Spell | direkt |
| Job | Haima (`HaimaPvE`) | 24305 | Ability | direkt |
| Job | Holos (`HolosPvE`) | 24310 | Ability | direkt |
| Job | Icarus (`IcarusPvE`) | 24295 | Ability | direkt |
| Job | Ixochole (`IxocholePvE`) | 24299 | Ability | direkt |
| Job | Kardia (`KardiaPvE`) | 24285 | Ability | direkt |
| Job | Kerachole (`KeracholePvE`) | 24298 | Ability | direkt |
| Job | Krasis (`KrasisPvE`) | 24317 | Ability | direkt |
| Job | Panhaima (`PanhaimaPvE`) | 24311 | Ability | direkt |
| Job | Pepsis (`PepsisPvE`) | 24301 | Ability | direkt |
| Job | Philosophia (`PhilosophiaPvE`) | 37035 | Ability | direkt |
| Job | Phlegma (`PhlegmaPvE`) | 24289 | Spell | direkt |
| Job | Phlegma II (`PhlegmaIiPvE`) | 24307 | Spell | über Phlegma |
| Job | Phlegma III (`PhlegmaIiiPvE`) | 24313 | Spell | über Phlegma II |
| Job | Physis (`PhysisPvE`) | 24288 | Ability | direkt |
| Job | Physis II (`PhysisIiPvE`) | 24302 | Ability | direkt |
| Job | Pneuma (`PneumaPvE`) | 24318 | Spell | direkt |
| Job | Prognosis (`PrognosisPvE`) | 24286 | Spell | direkt |
| Job | Psyche (`PsychePvE`) | 37033 | Ability | direkt |
| Job | Rhizomata (`RhizomataPvE`) | 24309 | Ability | direkt |
| Job | Soteria (`SoteriaPvE`) | 24294 | Ability | direkt |
| Job | Taurochole (`TaurocholePvE`) | 24303 | Ability | direkt |
| Job | Toxikon (`ToxikonPvE`) | 24304 | Spell | direkt |
| Job | Toxikon II (`ToxikonIiPvE`) | 24316 | Spell | direkt |
| Job | Zoe (`ZoePvE`) | 24300 | Ability | direkt |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Diagnosis | Knopf wird zu | Eukrasian Diagnosis |
| Dosis II | Ausbau (Offensive Magic Mastery II) | Dosis III |
| Dosis II | Knopf wird zu | Eukrasian Dosis II |
| Dosis III | Knopf wird zu | Eukrasian Dosis III |
| Dosis | Ausbau (Offensive Magic Mastery) | Dosis II |
| Dosis | Knopf wird zu | Eukrasian Dosis |
| Druochole | kostet | Addersgall |
| Dyskrasia II | Knopf wird zu | Eukrasian Dyskrasia |
| Dyskrasia | Ausbau (Offensive Magic Mastery II) | Dyskrasia II |
| Eukrasian Diagnosis | braucht Eukrasia | Eukrasia |
| Eukrasian Dosis II | braucht Eukrasia | Eukrasia |
| Eukrasian Dosis II | Ausbau (Offensive Magic Mastery II) | Eukrasian Dosis III |
| Eukrasian Dosis III | braucht Eukrasia | Eukrasia |
| Eukrasian Dosis | braucht Eukrasia | Eukrasia |
| Eukrasian Dosis | Ausbau (Offensive Magic Mastery) | Eukrasian Dosis II |
| Eukrasian Dyskrasia | braucht Eukrasia | Eukrasia |
| Eukrasian Prognosis II | braucht Eukrasia | Eukrasia |
| Eukrasian Prognosis | braucht Eukrasia | Eukrasia |
| Eukrasian Prognosis | Ausbau (Eukrasian Prognosis Mastery) | Eukrasian Prognosis II |
| Ixochole | kostet | Addersgall |
| Kerachole | kostet | Addersgall |
| Phlegma II | Ausbau (Offensive Magic Mastery II) | Phlegma III |
| Phlegma | Ausbau (Offensive Magic Mastery) | Phlegma II |
| Physis | Ausbau (Physis Mastery) | Physis II |
| Prognosis | Knopf wird zu | Eukrasian Prognosis II |
| Prognosis | Knopf wird zu | Eukrasian Prognosis |
| Taurochole | kostet | Addersgall |
| Toxikon II | kostet | Addersting |
| Toxikon | kostet | Addersting |
| Toxikon | Ausbau (Offensive Magic Mastery II) | Toxikon II |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Druochole | Regel prüft | Taurochole |
| Dyskrasia | Regel prüft | Eukrasian Dosis II |
| Dyskrasia | Regel prüft | Eukrasian Dosis III |
| Dyskrasia | Regel prüft | Eukrasian Dosis |
| Dyskrasia | Regel prüft | Eukrasian Dyskrasia |
| Eukrasia | Regel prüft | Eukrasian Diagnosis |
| Eukrasia | Regel prüft | Eukrasian Dosis II |
| Eukrasia | Regel prüft | Eukrasian Dosis III |
| Eukrasia | Regel prüft | Eukrasian Dosis |
| Eukrasia | Regel prüft | Eukrasian Dyskrasia |
| Eukrasia | Regel prüft | Eukrasian Prognosis II |
| Eukrasia | Regel prüft | Eukrasian Prognosis |
| Eukrasian Dosis II | Regel prüft | Dyskrasia |
| Eukrasian Dosis II | Regel prüft | Eukrasian Dosis III |
| Eukrasian Dosis II | Regel prüft | Eukrasian Dyskrasia |
| Eukrasian Dosis III | Regel prüft | Dyskrasia |
| Eukrasian Dosis III | Regel prüft | Eukrasian Dyskrasia |
| Eukrasian Dosis | Regel prüft | Dyskrasia |
| Eukrasian Dosis | Regel prüft | Eukrasian Dosis II |
| Eukrasian Dosis | Regel prüft | Eukrasian Dyskrasia |
| Eukrasian Dyskrasia | Regel prüft | Dyskrasia |
| Eukrasian Dyskrasia | Regel prüft | Eukrasian Dosis II |
| Eukrasian Dyskrasia | Regel prüft | Eukrasian Dosis III |
| Eukrasian Dyskrasia | Regel prüft | Eukrasian Dosis |
| Eukrasian Prognosis | Regel prüft | Eukrasian Prognosis II |
| Kerachole | Regel prüft | Taurochole |
| Krasis | Regel prüft | Diagnosis |
| Krasis | Regel prüft | Eukrasian Diagnosis |
| Krasis | Regel prüft | Pneuma |
| Krasis | Regel prüft | Prognosis |
| Panhaima | Regel prüft | Haima |
| Pepsis | StatusNeed EukrasianPrognosis | Eukrasian Prognosis II |
| Pepsis | StatusNeed EukrasianPrognosis | Eukrasian Prognosis |
| Philosophia | Regel prüft | Diagnosis |
| Philosophia | Regel prüft | Eukrasian Prognosis II |
| Philosophia | Regel prüft | Eukrasian Prognosis |
| Philosophia | Regel prüft | Pneuma |
| Philosophia | Regel prüft | Prognosis |
| Physis II | Regel prüft | Panhaima |
| Physis | Regel prüft | Physis II |
| Pneuma | Regel prüft | Dyskrasia |
| Zoe | Regel prüft | Eukrasia |
| Zoe | Regel prüft | Eukrasian Diagnosis |
| Zoe | Regel prüft | Eukrasian Prognosis II |
| Zoe | Regel prüft | Eukrasian Prognosis |
| Zoe | Regel prüft | Pneuma |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BreathOfTheEarthPvE`, `HealingWindPvE`, `TechneMakrePvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Repose (`ReposePvE`, Spell): ungenutzt
- Rescue (`RescuePvE`, Ability): ungenutzt

## Abgleich Wirktext ↔ Code

Der Wirktext nennt eine Bedingung, die Basisrotation führt dafür weder `StatusNeed` noch `ActionCheck`. RSR verlässt sich dann auf die Nutzbarkeitsauskunft des Spiels, die `BasicCheck` nur für eine feste Liste von Ablehnungscodes liest. Kandidaten, keine Befunde.

- Eukrasian Diagnosis (`EukrasianDiagnosisPvE`): Eukrasia
- Eukrasian Dosis (`EukrasianDosisPvE`): Eukrasia
- Eukrasian Dosis II (`EukrasianDosisIiPvE`): Eukrasia
- Eukrasian Dosis III (`EukrasianDosisIiiPvE`): Eukrasia
- Eukrasian Dyskrasia (`EukrasianDyskrasiaPvE`): Eukrasia
- Eukrasian Prognosis (`EukrasianPrognosisPvE`): Eukrasia
- Eukrasian Prognosis II (`EukrasianPrognosisIiPvE`): Eukrasia
