# NIN — Abhängigkeitsmatrix

Erzeugt von `.github/scripts/audit/generate_action_matrix.py` am 2026-09-26; nicht von Hand bearbeiten. Methode und Grenzen: `docs/rotation-flow/14-action-dependency-matrix.md`.

- Basisrotation: `NinjaRotation`
- Rotation: `RotationSolver/RebornRotations/Melee/NIN_Reborn.cs`
- Matrix als Tabelle: `NIN.csv`

## Nutzung

direkt: 46 · nur gelesen: 1 · ungenutzt: 1 · über anderen Knopf: 2

| Stufe | Aktion | Id | Art | Nutzung |
|---|---|---|---|---|
| alle | Sprint (`SprintPvE`) | 3 | System | direkt |
| Nahkämpfer | Arm's Length (`ArmsLengthPvE`) | 7548 | Ability | direkt |
| Nahkämpfer | Bloodbath (`BloodbathPvE`) | 7542 | Ability | direkt |
| Nahkämpfer | Feint (`FeintPvE`) | 7549 | Ability | direkt |
| Nahkämpfer | Leg Sweep (`LegSweepPvE`) | 7863 | Ability | direkt |
| Nahkämpfer | Second Wind (`SecondWindPvE`) | 7541 | Ability | direkt |
| Nahkämpfer | True North (`TrueNorthPvE`) | 7546 | Ability | direkt |
| Job | Aeolian Edge (`AeolianEdgePvE`) | 2255 | Weaponskill | direkt |
| Job | Armor Crush (`ArmorCrushPvE`) | 3563 | Weaponskill | direkt |
| Job | Assassinate (`AssassinatePvE`) | 2246 | Ability | direkt |
| Job | Bhavacakra (`BhavacakraPvE`) | 7402 | Ability | direkt |
| Job | Bunshin (`BunshinPvE`) | 16493 | Ability | direkt |
| Job | Chi (`ChiPvE`) | 2261 | Ability | direkt |
| Job | Death Blossom (`DeathBlossomPvE`) | 2254 | Weaponskill | direkt |
| Job | Deathfrog Medium (`DeathfrogMediumPvE`) | 36959 | Ability | über Hellfrog Medium |
| Job | Dokumori (`DokumoriPvE`) | 36957 | Ability | direkt |
| Job | Doton (`DotonPvE`) | 2270 | Ability | direkt |
| Job | Dream Within a Dream (`DreamWithinADreamPvE`) | 3566 | Ability | direkt |
| Job | Fleeting Raiju (`FleetingRaijuPvE`) | 25778 | Weaponskill | direkt |
| Job | Forked Raiju (`ForkedRaijuPvE`) | 25777 | Weaponskill | direkt |
| Job | Fuma Shuriken (`FumaShurikenPvE`) | 2265 | Ability | direkt |
| Job | Goka Mekkyaku (`GokaMekkyakuPvE`) | 16491 | Ability | direkt |
| Job | Gust Slash (`GustSlashPvE`) | 2242 | Weaponskill | direkt |
| Job | Hakke Mujinsatsu (`HakkeMujinsatsuPvE`) | 16488 | Weaponskill | direkt |
| Job | Hellfrog Medium (`HellfrogMediumPvE`) | 7401 | Ability | direkt |
| Job | Hide (`HidePvE`) | 2245 | Ability | direkt |
| Job | Hollow Nozuchi (`HollowNozuchiPvE`) | 25776 | Ability | ungenutzt — nicht zuweisbar: Begleiter oder Automatik |
| Job | Huton (`HutonPvE`) | 2269 | Ability | direkt |
| Job | Hyosho Ranryu (`HyoshoRanryuPvE`) | 16492 | Ability | direkt |
| Job | Hyoton (`HyotonPvE`) | 2268 | Ability | direkt |
| Job | Jin (`JinPvE`) | 2263 | Ability | direkt |
| Job | Kassatsu (`KassatsuPvE`) | 2264 | Ability | direkt |
| Job | Katon (`KatonPvE`) | 2266 | Ability | direkt |
| Job | Kunai's Bane (`KunaisBanePvE`) | 36958 | Ability | direkt |
| Job | Meisui (`MeisuiPvE`) | 16489 | Ability | direkt |
| Job | Mug (`MugPvE`) | 2248 | Ability | direkt |
| Job | Ninjutsu (`NinjutsuPvE`) | 2260 | Ability | nur gelesen — Behälter: der Knopf wird zu anderen Aktionen |
| Job | Phantom Kamaitachi (`PhantomKamaitachiPvE`) | 25774 | Weaponskill | direkt |
| Job | Rabbit Medium (`RabbitMediumPvE`) | 2272 | Ability | direkt |
| Job | Raiton (`RaitonPvE`) | 2267 | Ability | direkt |
| Job | Shade Shift (`ShadeShiftPvE`) | 2241 | Ability | direkt |
| Job | Shukuchi (`ShukuchiPvE`) | 2262 | Ability | direkt |
| Job | Spinning Edge (`SpinningEdgePvE`) | 2240 | Weaponskill | direkt |
| Job | Suiton (`SuitonPvE`) | 2271 | Ability | direkt |
| Job | Ten (`TenPvE`) | 2259 | Ability | direkt |
| Job | Ten Chi Jin (`TenChiJinPvE`) | 7403 | Ability | direkt |
| Job | Tenri Jindo (`TenriJindoPvE`) | 36961 | Ability | direkt |
| Job | Throwing Dagger (`ThrowingDaggerPvE`) | 2247 | Weaponskill | direkt |
| Job | Trick Attack (`TrickAttackPvE`) | 2258 | Ability | direkt |
| Job | Zesho Meppo (`ZeshoMeppoPvE`) | 36960 | Ability | über Bhavacakra |

## Beziehungen

### Aus den Wirktexten (Spiel)

| Aktion | Beziehung | zu |
|---|---|---|
| Aeolian Edge | Combo nach | Gust Slash |
| Armor Crush | Combo nach | Gust Slash |
| Assassinate | Ausbau (Adept Assassination) | Dream Within a Dream |
| Bhavacakra | gemeinsame Abklingzeit | Hellfrog Medium |
| Bhavacakra | kostet | Ninki |
| Bhavacakra | Knopf wird zu | Zesho Meppo |
| Bunshin | kostet | Ninki |
| Deathfrog Medium | gemeinsame Abklingzeit | Bhavacakra |
| Deathfrog Medium | braucht Higi | Eigenschaft Enhanced Dokumori |
| Deathfrog Medium | kostet | Ninki |
| Fleeting Raiju | braucht (Erzeuger nicht im Text) | Raiju Ready |
| Forked Raiju | braucht (Erzeuger nicht im Text) | Raiju Ready |
| Goka Mekkyaku | braucht Kassatsu | Kassatsu |
| Gust Slash | Combo nach | Spinning Edge |
| Hakke Mujinsatsu | Combo nach | Death Blossom |
| Hellfrog Medium | gemeinsame Abklingzeit | Bhavacakra |
| Hellfrog Medium | Knopf wird zu | Deathfrog Medium |
| Hellfrog Medium | kostet | Ninki |
| Hyosho Ranryu | braucht Kassatsu | Kassatsu |
| Hyoton | Ausbau (Enhanced Kassatsu) | Hyosho Ranryu |
| Hyoton | Ausbau (Enhanced Kassatsu) | Kassatsu |
| Katon | Ausbau (Enhanced Kassatsu) | Goka Mekkyaku |
| Kunai's Bane | braucht (Erzeuger nicht im Text) | Hidden |
| Meisui | braucht (Erzeuger nicht im Text) | combat |
| Meisui | braucht (Erzeuger nicht im Text) | under the effect of Shadow Walker |
| Mug | Ausbau (Mug Mastery) | Dokumori |
| Ninjutsu | Knopf wird zu | Doton |
| Ninjutsu | Knopf wird zu | Fuma Shuriken |
| Ninjutsu | Knopf wird zu | Goka Mekkyaku |
| Ninjutsu | Knopf wird zu | Huton |
| Ninjutsu | Knopf wird zu | Hyosho Ranryu |
| Ninjutsu | Knopf wird zu | Hyoton |
| Ninjutsu | Knopf wird zu | Katon |
| Ninjutsu | Knopf wird zu | Raiton |
| Ninjutsu | Knopf wird zu | Suiton |
| Phantom Kamaitachi | braucht Phantom Kamaitachi Ready | Phantom Kamaitachi |
| Ten Chi Jin | Knopf wird zu | Tenri Jindo |
| Tenri Jindo | braucht Tenri Jindo Ready | Eigenschaft Enhanced Ten Chi Jin |
| Trick Attack | braucht (Erzeuger nicht im Text) | Hidden |
| Trick Attack | Ausbau (Trick Attack Mastery) | Kunai's Bane |
| Zesho Meppo | braucht Higi | Eigenschaft Enhanced Dokumori |
| Zesho Meppo | gemeinsame Abklingzeit | Hellfrog Medium |
| Zesho Meppo | kostet | Ninki |

### Aus dem Code (RSR)

| Aktion | Beziehung | zu |
|---|---|---|
| Aeolian Edge | Regel prüft | Armor Crush |
| Armor Crush | Regel prüft | Aeolian Edge |
| Armor Crush | ComboIds | Gust Slash |
| Assassinate | Regel prüft | Dream Within a Dream |
| Bhavacakra | Regel prüft | Bunshin |
| Bhavacakra | Regel prüft | Mug |
| Chi | Regel prüft | Huton |
| Chi | Regel prüft | Hyoton |
| Chi | Regel prüft | Katon |
| Death Blossom | Regel prüft | Chi |
| Death Blossom | Regel prüft | Hakke Mujinsatsu |
| Death Blossom | Regel prüft | Huton |
| Death Blossom | Regel prüft | Hyosho Ranryu |
| Death Blossom | Regel prüft | Jin |
| Death Blossom | Regel prüft | Katon |
| Death Blossom | Regel prüft | Kunai's Bane |
| Death Blossom | Regel prüft | Meisui |
| Death Blossom | Regel prüft | Raiton |
| Death Blossom | Regel prüft | Suiton |
| Death Blossom | Regel prüft | Ten |
| Death Blossom | Regel prüft | Trick Attack |
| Goka Mekkyaku | Regel sperrt vorher | Trick Attack |
| Gust Slash | ComboIds | Spinning Edge |
| Hakke Mujinsatsu | Regel prüft | Chi |
| Hakke Mujinsatsu | ComboIds | Death Blossom |
| Hakke Mujinsatsu | Regel prüft | Death Blossom |
| Hakke Mujinsatsu | Regel prüft | Hyosho Ranryu |
| Hakke Mujinsatsu | Regel prüft | Jin |
| Hakke Mujinsatsu | Regel prüft | Katon |
| Hakke Mujinsatsu | Regel prüft | Raiton |
| Hakke Mujinsatsu | Regel prüft | Ten |
| Hellfrog Medium | Regel prüft | Bhavacakra |
| Hellfrog Medium | Regel prüft | Bunshin |
| Hellfrog Medium | Regel prüft | Mug |
| Hide | Regel prüft | Ten |
| Hyosho Ranryu | Regel sperrt vorher | Trick Attack |
| Jin | Regel prüft | Death Blossom |
| Jin | Regel prüft | Doton |
| Jin | Regel prüft | Hakke Mujinsatsu |
| Jin | Regel prüft | Huton |
| Jin | Regel prüft | Kunai's Bane |
| Jin | Regel prüft | Meisui |
| Jin | Regel prüft | Suiton |
| Jin | Regel prüft | Ten Chi Jin |
| Jin | Regel prüft | Ten |
| Jin | Regel prüft | Trick Attack |
| Kassatsu | Regel prüft | Chi |
| Kassatsu | Regel prüft | Jin |
| Kassatsu | Regel prüft | Ten |
| Meisui | StatusNeed ShadowWalker | Suiton |
| Meisui | Regel prüft | Ten Chi Jin |
| Meisui | Regel prüft | Trick Attack |
| Mug | Regel prüft | Dokumori |
| Ten Chi Jin | Regel prüft | Ten |
| Ten | Regel prüft | Doton |
| Ten | Regel prüft | Fuma Shuriken |
| Ten | Regel prüft | Raiton |
| Ten | Regel prüft | Suiton |
| Tenri Jindo | Regel prüft | Bunshin |
| Tenri Jindo | Regel prüft | Mug |
| Trick Attack | StatusNeed Hidden | Hide |
| Trick Attack | Regel prüft | Kunai's Bane |
| Trick Attack | StatusNeed ShadowWalker | Suiton |

## Nicht in der Matrix: Limit Breaks

Ohne Eintrag in `ActionId.resx` und ohne Wirktext; RSR castet keine PvE-Limit-Breaks (Konzept 05). `BladedancePvE`, `BraverPvE`, `ChimatsuriPvE`

## Nicht direkt genutzt

Maschinelle Liste; die Bewertung je Eintrag steht im Konzept.

- Hollow Nozuchi (`HollowNozuchiPvE`, Ability): ungenutzt — nicht zuweisbar: Begleiter oder Automatik
- Ninjutsu (`NinjutsuPvE`, Ability): nur gelesen — Behälter: der Knopf wird zu anderen Aktionen
