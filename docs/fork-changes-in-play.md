# What this fork does differently in a fight

The whole distance from upstream **7.5.7.14**, as of release `7.5.7.14+wsh1`. What a single
release changed is in [`fork-changes-since-last-release.md`](fork-changes-since-last-release.md),
which is the text of the release description; this document is the standing picture and takes
each release's text over once it has shipped.

Settings are named as they appear in the configuration, and where a change sits behind a switch
its default is given; without that note it takes effect immediately. A setting starts at the value
that plays better, so the new rules are in use rather than waiting to be found. A job setting you
never changed takes the current default; a general setting is stored with your configuration once
saved, so there it keeps the value you had. Two changes are confirmed in play — the raise dispatch
and the tank pre-pull HoT; everything else is established in the code and compiled, not yet
measured at a dummy.

## Party mitigation answers raidwides

`Skip mitigation for small area casts` spares the cooldown when an area hit is too small to
matter. It asked one question only — would this hit push anyone to where healing is called for —
and a healthy party answered no to almost every raidwide, so Addle and Radiant Aegis stopped going
out on Summoner. Mitigation is meant to throttle the damage *before* a need to heal appears.

An area action costing **25 % of maximum health or more** is a big hit whatever the party's
health. The figure is read from the effect texts — it is what the largest barrier in the game
absorbs — not set by hand. Below it the buffer comparison decides, so small repeated ticks cost no
cooldown while the party is healthy. A BossModReborn raidwide that appears together with the start of
such a small cast does not open the area defence while the cast runs; one with any other cast beside
it, an unmeasured one, or one left when the cast ends is answered as before. Hits and heals above 65,535 points are read in full; before,
the low 16 bits only, and a raidwide that hit a tank harder than that could be rated small.

-  **An interruptible cast can be mitigated too** —
  `Mitigate a big area cast even when it is interruptible`, **on by default**. Area questions drop
  interruptible casts, because those are meant to be interrupted. In a dungeon where nobody does —
  and a Summoner has no interrupt — the hit lands unanswered. A cast measured at the large-barrier
  figure raises the defence anyway, within a GCD of landing.
-  **A predicted mitigation is not spent on the small hit before the big one** —
  `Hold a predicted mitigation while a small cast is running`, **on by default**. BossMod gives the
  timing of the next event, not its size, so in an opening with a small cast ahead of a heavy one
  the barrier is eaten by the first. It waits while a measured small cast runs.
- **Area casts of invisible helpers count.** Many raidwides are cast by untargetable helpers, or by
  a boss off the field — Shinryu's Meteor Impact comes from five light spheres, each hitting the
  whole party. The area recognition reads untargetable enemies too while they cast an action on the
  AoE list, so such a raidwide opens the defence without a BossModReborn module. Their listed
  tankbusters and knockbacks are read the same way: a tankbuster a helper casts at the tank opens the
  tank's defence and gets a tankbuster plan, a knockback gets Arm's Length or Surecast without a module.
- **Area defence asks whether the party is hit; your own cooldowns ask whether you are.** Whom a
  cast from the AoE list reaches is read from the game data's shape: a single-target action with a
  cast range reaches only its target, a circle cast at a player is measured around that player, a
  line only reaches what lies inside its width, and a circle around the caster is measured from the
  caster's centre, as BossModReborn draws it (a large boss's hitbox used to be added, so a 10-yalm
  point-blank reached players 18 yalms away). Ground-targeted circles, cones and charges are still
  measured from the caster: the game data does not state where they land or how wide they open. The
  area defence opens party mitigations when the hit reaches you or at least two party members.
  Defences that protect only you — Rampart, Damnation, Raw Intuition, Arm's Length, a barrier on
  yourself, Radiant Aegis, Tempera Coat, Samurai's Tengentsu and Third Eye — go out only when the hit
  that opened the defence reaches you, in the area and the single-target defence alike and for every
  job, so a tankbuster circle on the tank no longer spends a Summoner's Radiant Aegis. Help for others
  stays: Reprisal, Shake It Off, Intervention and Heart of Corundum on the tankbuster's target are not
  held back. Your own area defence command — "Def AoE" in the control window or the defence macro —
  is not second-guessed: it casts a self-only defence even when RSR does not see the hit reaching you.
- **BossModReborn's raidwides count where they reach you or your party.** BossModReborn calls more
  than raidwides "raidwide": towers, baits, spreads and charges on single players carry the same type,
  and each prediction names whom it hits. Where RSR can read that list (the prediction is the next
  one), a raidwide that reaches neither you nor more than one party member opens nothing, and one that
  reaches the party but not you spends no defence that protects only you. Where BossModReborn names
  nobody (its timeline, or another prediction comes first), the raidwide counts as hitting everyone.
- **Markers count only on you or your own party** (Duty Support companions included); a stack
  marker in another alliance group no longer opens your mitigations. A spread marker counts only when it
  sits on you: in M9 (Vamp Fatale, normal and Savage) Aetherletting's spread on another member no longer
  spends your self-only protection, and party mitigation reads it as before.
- **Skip area defence for casts that missed you** — **on by default**. An area cast that landed last
  time without reaching you does not open your area defence the next time; once it reaches you
  again, it counts again. Blocked, parried, absorbed or taken under invulnerability counts as
  reaching you. Healing ahead and the party's threat check are unaffected. Some listed casts land
  only on their caster and hurt nobody, and the hit comes under the same name right after, often from
  an invisible helper: a same-named hit on the party after such an empty landing counts as that
  landing, both for whether the cast reached you and for how hard it hits. A wind-up that nothing
  follows stays skipped.

- **Tanks: Reprisal and the party tool take turns over area attacks** — `Spread Reprisal and the party
  mitigation over a run of area attacks`, **on by default**. The second waits while the first still
  works — a barrier until it is used up —, so two area attacks in a row each get one. Both go together
  when a member is in danger or the attack, as measured, would put an unprotected member there.
- **Enemy debuffs on a pull follow their refresh setting.** Without a BossModReborn prediction, Reprisal,
  Feint and Addle are kept up on a pull of enough enemies and renewed as the action's own "Number of GCDs
  before the DOT/Status effect is reapplied" says.

- **A raidwide that lands in several hits counts mitigation only to its last hit.** RSR learns how long a series of
  hits from one area attack runs - Bodyweight Exorcism's four towers land over 9 s - and counts a party mitigation only
  where it lasts until the last hit. A mitigation that ran out after the first towers no longer counts for all of them,
  and `Skip mitigation for small area casts` no longer leaves yours out while the last towers land unmitigated.
- **Astrologian, Scholar, Sage: the limit to multihit stacks also covers series and dangerous hits.** With "Limit
  Macrocosmos / Seraphism / Panhaima to multihit party stacks" on, these tools went into the attacks that hit the party
  several times only with a marker. As for the White Mage's bell, two sub-settings (on by default) let them go into a
  learned series and before an area hit that would put a member in danger.
- **An area series of a pack ends with the pack's round.** A listed area cast that lands several times in a row is rated
  by what the whole series takes from a member, whoever cast each landing - a mechanic's helpers and a pack land on the
  party together. In a big pull of like enemies the series ran on as long as any of them filled the gap, and its sum
  grew with the pull: three enemies recasting an 8 % area cast every 8 s could be stored at 64 %, and every later cast
  of it was then mitigated and pre-healed as a near-lethal hit. A targetable enemy that casts again within a running
  series now starts a new one; mechanic objects (untargetable helpers) keep counting together, so tower sequences stay
  one series. Each split writes a line to `DefenseTrace.log`.

## Healing — when it lands

- **A pull no longer starts without healing.** For roughly the first 2.5 seconds of every
  pull there was no automatic healing at all, because the time-to-kill estimate read as "the
  fight is about to end" while it was still empty.
- **The white mage heals again while a pack is dying.** At the end of every pack all heal
  flags went out: tank below 20 %, no heal attempted, Holy instead of Cure.
- **Healing ahead of the hit instead of after it** — `Heal ahead of incoming damage`, **on by
  default**. Every healing threshold reads the health a member is heading for by the time the
  heal lands: the heal flags, the heal target choice, the area-heal thresholds and the thresholds
  inside the job rotations (Regen, Essential Dignity, Taurochole, Excogitation, Clemency, Nascent
  Flash, Second Wind and the others). Mitigation thresholds are unchanged. A GCD heal looks ahead by
  the rest of the GCD and its cast; an off-GCD heal only as far as the current animation lock, so
  Benediction and the like no longer go out a GCD before they are needed. Synastry, Krasis, Soteria
  and Emergency Tactics heal with the next GCD and look ahead like a GCD heal. The rate is net of
  mitigation, barrier and healing, and each prediction is held against the actual course every
  second and corrected. A member's fall is measured from the last time he stood at least as high, so a
  hard first hit of a pull does not blind the forecast for him. The thresholds on the party's average in
  the healer rotations read it too: Horoscope and Lady of Crowns, Holos and Pneuma, Fey Blessing,
  Indomitability and Recitation with Indomitability, and the downtime heal.
- **The GCD length holds between GCDs.** Out of range, between pulls and before the first GCD the
  game reports no GCD length, and everything measured in GCDs collapsed: heal-ahead looked nowhere,
  the one-GCD window in which a running enemy cast is answered was closed, and the first-GCDs-of-combat
  guards did not hold. The last GCD length now stands in while none runs.
- **Healing ahead of an announced area cast** — `Heal ahead of an announced area cast`, **on by
  default**. 60 % in front of a 45 % raidwide is not healthy. An existing barrier counts against
  the hit it absorbs. The area heal goes out ahead of the hit only when the hit would leave someone
  about to fall and that member is missing health; otherwise it waits and heals what the hit took, so
  no Medica or Cure III lands on a full party. The White Mage's Afflatus Rapture at full lilies waits
  for an announced raidwide to land, unless a fourth lily would otherwise be lost.
- **The big single-target heals wait for danger.** Benediction (`Benediction needs a reason`), the
  last charge of Essential Dignity, Excogitation as a heal and Taurochole as a heal go only to a
  target being attacked or cast at, with an area cast announced, or with health measurably falling.
  Each has its own setting, on by default. A player who was just raised and takes no damage gets the
  smaller heals.
- **Area heals centred on you read the need where they land.** Medica, Helios, Succor, Lux Solaris
  and the other area heals around the caster read the hurt, living party members inside the heal's
  radius, not whoever stands at 0 yalms — usually the caster. The AoE setting is about attacks only:
  on `Cleave` a healer casts group heals and party mitigations.
- **The tank walks into the pull with a HoT already ticking** — `UsePreRegen` (White Mage),
  `UsePreAspectedBenefic` (Astrologian), each with two enemy-count thresholds. Both actions are
  instant, so nothing is lost while moving. **Confirmed in play.**
- **Scholar: Recitation is not wasted.** For a tank it asks for the same danger as the Excogitation
  that follows it (`Excogitation as a heal only on a target in danger`). If its reason goes away before
  its heal comes, it goes in its last moment into an Excogitation on the tank, or with Excogitation on
  cooldown into an Adloquium barrier on him.
- **A single-target heal is broken off only once its target has been healed** —
  `Stop single-target GCD heals once the target is above the threshold (Experimental)`, **off by default**.
  It stops a cast when its target needed the heal as the cast began, has been healed since, and no
  longer needs it. A heal begun for a falling trend runs on when the trend levels out with nobody
  healed, and a barrier on a tank without need, such as the Adloquium before Recitation runs out, is
  not broken off. When the healed target was not the only one in need, the next GCD goes to the one
  still in need. `DefenseTrace.log` records each stop.
- **HP potions answer to their own settings**: the global setting, the per-item enable, its own HP
  percentage, the missing-health guard, having one in the bag, and being in combat — not the
  healing-action flag, which a damage dealer with a living healer in the party never gets. A
  tankbuster that reaches you lowers the percentage: one cast at you, or a predicted one whose targets
  name you — when BossModReborn names nobody, for a tank, or when the party has no tank alive. Of two potions the one that restores more
  goes first, and at equal healing the lower grade, read for the form that will be drunk (HQ or NQ).
  While a raise with a Phoenix Down is due and you live through its cast, the potion waits, as it would
  keep the feather off for 75 seconds; one with a tankbuster on its way to you still goes.

- **GCD heals do not wait on another healer while someone is about to fall.** With `Use GCDs to heal`
  off — the default for Sage —, and for Paladin, Red Mage and Summoner, the rotation still heals with its
  GCDs whenever a party member is in the critical class. For the Astrologian it is **on by default**, as for
  White Mage and Scholar, so its GCD heals and the heal over time against damage over time also go beside a
  second healer; a value you set yourself stays.
- **A heal over time against an enemy's damage over time** — `Heal over time against an enemy's damage
  over time`, **on by default**. When an enemy's damage over time alone will take a party member below the
  single-target heal threshold before a heal over time laid now would run out, White Mage and Astrologian
  lay Regen or Aspected Benefic at once, and Medica III or Helios Conjunction (Medica II or Aspected Helios
  below their level) when more than one member needs it. Every damage and heal tick on the party is
  measured, so a weak damage over time costs no GCD, and running heals over time from any healer count. Its
  strength and, where the status shows no timer, how long it lasts are learned per area and kept, so from
  the second pull the heal goes on the moment the debuff lands. A member Esuna is about to clear is left to
  it; each decision is written to `DefenseTrace.log`.
- **Astrologian: Essential Dignity priority no longer stops single-target healing.** On "Any charges" or
  "When capped", `Prioritize Essential Dignity over single target GCD heals when available` held every
  single-target GCD heal while a charge was left, also the last one, which waits for a target in danger;
  GCD heals now wait only while Essential Dignity can go out. `Prioritize Microcosmos over all other
  healing when available` holds only heals, not Oracle and the potion before Divination.
- **Sage: Druochole when Taurochole waits for danger.** With `Taurochole as a heal only on a target in
  danger` on (the default), a low member with nothing hitting him gets Druochole instead of neither.
- **White Mage: Liturgy of the Bell heals when the party needs it.** Its second press (`Press Liturgy of
  the Bell a second time when the party needs an area heal`, **on by default**) releases the stacks left
  at once, once the bell has healed and the party is below your area-heal thresholds. Under `Limit
  Liturgy Of The Bell to multihit party stacks` (off by default) two sub-options, both **on by
  default**, add the cases where it heals: before an announced hit that by its measured size would put a
  member in danger and that reaches you (`Also before a hit that would put a party member in danger`),
  and into an enemy attack learned to hit you and the party several times in a row (`Also into an enemy
  attack that hits in a series`) — at its first hit, or just before it when an enemy cast of it reaching
  you lands within the next GCD. An attack teaches the first time it is met.
- **Every running heal over time counts.** The heal thresholds for "Targets with Heal-over-Time" applied only to the
  heals over time on two fixed lists, so under Kerachole, Celestial Opposition, Aurora or Knight's Benediction RSR healed
  as if nothing were ticking. Any status whose own text says it regenerates HP over time counts, from whoever laid it.
  A member's single threshold reads the one on him that runs longest; the area threshold reads how much of the party is
  under one, not only whether you are.
- **The emergency heal now gets out between two hard casts.** After a 2.0 s heal (Cure II, Medica II, Cure III,
  Adloquium, Succor, Prognosis) at most half a second is left before the next GCD, and abilities are never woven into
  that last half second, so a healer chaining Cure II never reached Benediction: a tank under a whole pack fell to 43 HP
  with Benediction ready. When a party member would die before a GCD heal begun now could land (the critical class,
  "HealthForDyingTanks"), the job's heal ability for that member is now pressed the moment the GCD comes free, and the
  GCD follows after its animation lock. Which ability goes, and on whom, still follow each job's own thresholds and
  danger checks - Benediction stays the absolute emergency. Each such heal, and each critical member left without one,
  writes a line to `DefenseTrace.log`.

## Healing — who it picks

- **Choose the heal target by danger** — **on by default**. Below anyone about to fall, a healer
  or tank under their role threshold who is being attacked comes first, together with any healer or
  tank being attacked who stands as low or lower, lowest health first and a healer before a tank at
  equal health; then everyone else — while area damage is announced by the
  hit points left after what is coming, otherwise by lowest percentage. Area damage counts as
  announced from any source: a listed area cast reaching the party, a large one even when it can be
  interrupted, an area marker on the party, or a raidwide BossModReborn announces within the
  mitigation window. A damage dealer at 10 % is no longer passed over because the tank sits at 44 %.
- **A party member behind the camera can be healed again** — the "only targets in view"
  filter applied to heal targets as well.
- **A target under invulnerability** gets healing at a lowered threshold and the normal one
  shortly before the status ends, instead of none at all.
-  **Warrior: Nascent Flash reaches the party.** Its target filter read the invulnerability check
  the wrong way round and picked only invulnerable members. It goes to members who need it, never to
  one whose healing is nullified. `Nascent Flash target priority` defaults to
  `Most in danger of dying first…`. `Keep Bloodwhetting for yourself when you need it` (**on by
  default**) keeps it while a tankbuster on you comes before the shared cooldown is back, or while
  you are about to die — unless the member is about to die and is a healer, or another tank while
  you are not.
- **Gunbreaker: Aurora on `Full target usage`** picks its target like every other heal, by danger,
  not by the fewest HP points.
- **"Prefer lower-HP targets with the Big targeting type" takes effect**; among equally big targets the setting for
  small targets was read.

- **A tankbuster on several members: each help goes to the one less protected** — the target standing
  with the fewest mitigations and barriers, so a double tankbuster on both tanks no longer gets every
  help on the same one.
- **Ground heals go where the party stands** — `Place them where they cover the most party members`, **on
  by default**. Sacred Soil, Asylum, Earthly Star and Liturgy of the Bell go to the spot at a party member
  that covers more members than the stored point, arena centre, boss or tank point would: a party stacked
  at the south edge for a marker gets Sacred Soil on the stack, not in the empty middle.
- **Downtime healing heals only those below its line.** "Use GCD heals when there's nothing else to do" started when the
  lowest member fell below "When the lowest party HP is below", but then healed anybody not at full health: with the
  tank at 75 % already carrying Regen, Regen went on a black mage at 95 %, and three members at 95 % were enough for an
  area heal. It now heals only members under that line, by the same danger order as every other heal, and casts an area
  heal only when more than two stand under it. Each downtime heal writes a line to `DefenseTrace.log`.

## Shields and barriers

- **The shield list knows the common barriers** — fifteen were missing, among them Divine Benison
  and The Blackest Night.
- **A shield is not credited against the healing threshold.** A shield prevents damage, it does
  not restore health: a tank at 40 % stands at 40 % whether a barrier is running or not.
-  **Sage: a barrier on the tank through a dungeon pull** —
  `Keep Eukrasian Diagnosis on the tank through a pull`, **on by default**. In dungeons Eukrasian
  Diagnosis goes on the tank as they close in on a group and is renewed whenever the barrier runs
  out or breaks, for as long as the pull lasts; each renewal takes a GCD from Dosis, and in a large
  pull, where the barrier breaks within a few hits, that is most of the sage's GCDs. White Mage,
  Astrologian and Sage share one rule for this upkeep, and none of them spends the MP their raise
  needs on it.
- **Dancer: Improvisation ends with its barrier.** Improvised Finish was never cast, so the party
  got the regen but not the 5 % barrier.
- **White Mage and Astrologian: no barrier charge left at the cap.** At both charges Divine Benison and
  Celestial Intersection go onto the member an enemy is attacking, where none of their barrier stands; the
  second charge stays for the next danger, and the single-target defence takes it for a tankbuster — as
  The Balance's White Mage guide recommends.

## Raising

- **It happens at once instead of after a long delay.** Swiftcast was only spent on a raise
  once the raise was already reported as the next GCD — which it could not be without
  Swiftcast. It now also falls when a raise is pending and would be castable. **Confirmed in
  play.**
- **No raise race with another healer.** RSR reads another player's raise cast and its target and
  leaves that body alone; Swiftcast stays for a body nobody is raising. When both raises are already
  being cast, RSR stops its own as soon as the other lands first. A body carrying the second form of
  the Raise status counts as raised. A member another healer is casting Esuna on is not cleansed unless
  more than that one effect is left.
- **Red Mage raises in the burst**: `Prevent raising during burst combos` is **off by default**. In
  the burst, Vercure still waits under `Prevent healing during burst combos`, but no longer when no
  healer is alive or a member is about to die because the healers do not keep up.
- **Phoenix Down** is wired up and checks target eligibility. **Off by default.** Since patch 7.3 it can be
  thrown in combat in four-player content as an 8-second cast that holds every action and shares its recast
  with the medicines; it waits while you move, while a tankbuster on you would land within the cast, and
  while the heal forecast says you would fall before it ends.
- **White Mage: Thin Air no longer overrides `Hard-cast raise`.** Whether Raise is hard cast is decided
  by that setting alone, as for every healer; Thin Air still makes it cost no MP, and while moving Dia and
  the Lilies go as before.
- **No raise delay by default.** `Raise delay range` starts at no delay; when the fallen member gets up is
  his choice when he accepts. A configuration saved earlier keeps its value.
- **`Raise All.` counts the alliance's living raisers** the way `Raise All In Duty.` does: the
  healer-only hard-cast modes and the Phoenix Down wait while one of them lives. Strangers outside a
  duty never count.
- **The healer-only hard-cast modes** hold the Swiftcast reservation their text promises:
  with Swiftcast ready, nothing is hard-cast any more.
- Swiftcast is not spent for damage in the Summoner rotation; it stays reserved for raises —
  `Use Crimson Cyclone at any range…` and Swiftcast on Ruby Ruin and Ruby Outburst below Ruby
  Rite's level are off by default. Whether a raise is swiftcast follows `Raise with Swiftcast or Dualcast`
  alone, kept per job; the Summoner's own option for Swiftcast on Resurrection is gone.
- **A job that can raise keeps the MP for it.** In combat and in a party, healers, Summoner and Red Mage spend MP on
  damage only while the raise's cost - or "Don't raise below this MP" where that is higher - is left. A healer at 2600
  MP does not cast the damage spell that would take him below 2400 and leave a dead member waiting for MP; heals and
  free casts go on. Setting "Keep the MP a raise needs", on by default.
- **Swiftcast stays ready for the raise.** While a job's raise is learned and "Raise with Swiftcast or Dualcast" is on,
  damage no longer takes Swiftcast unless you switch it on yourself: Red Mage used it for Verthunder and Veraero in
  every burst and for any hard-cast Verthunder III or Veraero III; it now does so only with the new option "Use
  Swiftcast for damage" (off by default), so a dead member is raised at once even while you move. Summoner's Swiftcast
  switches already were off by default.
- **With "Raise with Swiftcast or Dualcast" off, a running Swiftcast is not wasted.** A Swiftcast or Dualcast already
  running now goes to your next spells, as the setting says, and into a pending raise only when it would otherwise run
  out unused. Before, the raise took it at once: a Summoner who pressed Swiftcast for Slipstream while moving lost it to
  Resurrection. The healers' "Swiftcast restriction" holds their other GCDs only while the raise takes it, so a healer
  who presses Swiftcast by hand at a corpse keeps casting.

## Mitigation and defensives

- **Two tanks no longer stack Reprisal**, and the second one keeps the 60 seconds of cooldown
  it used to throw away. The same applies to Addle and Feint with two casters or two melees.
- **Paladin and Warrior give Reprisal on raidwides too.**
- **The mitigation debuffs are sustained rather than applied once**, with the duration
  following the level 98 trait.
- **A buster aimed at a damage dealer gets an answer.** Twelve jobs use their reactive line —
  Feint, Addle, Troubadour, Tactician, Shield Samba, Third Eye — when a cast is aimed at them. For a
  tankbuster BossModReborn announces as the next hit, BossModReborn's own list of who is hit decides.
- **Self-healing for damage dealers** (Second Wind, Bloodbath) across ten jobs.
- **Defensive holds give way when the party is in danger.** The White Mage's and Astrologian's
  spread of party mitigation, and the burst weave holds of Dark Knight, Gunbreaker, Machinist,
  Dragoon and Viper, give way when a party member is at `HealthForDyingTanks` or the announced area
  cast would put someone there; a single-target hold gives way for you or a tank at that level. The
  White Mage and Astrologian wait as long as the effect that started the wait lasts (Astrologian 15 s
  after Macrocosmos, 10 s after Collective Unconscious). The Bard, Painter and Dancer setting "Prevent
  the use of defense abilties during burst" is **off by default**, so a raidwide inside the burst gets
  Troubadour, Shield Samba, Tempera or Addle; switched on, it holds without exception. Reaper (Soul Reaver,
  Enshroud, Executioner), Viper (a ready follow-up ability) and Samurai (Zanshin ready) hold their
  protection, self-heal, knockback protection and interrupts no longer when the party, you or a tank is in
  danger; knockback protection and interrupts wait only for a weave that does not clip.
- **Samurai: Feint stays for the hits the party takes.** The threshold `Health threshold needed to use
  Tengentsu/ThirdEye outside of AOE mit scenarios.` uses Tengentsu and Third Eye only; Feint goes out for
  announced or measured hits, as for every job.
- **Tankbuster markers**: every tank lock-on marker and shared tank laser is recognised, plus two
  more party-stack markers. A marker after which no enemy action reaches the marked member goes on
  `TankbusterMarkerWithoutHit.json` and is no longer treated as a tankbuster; a later hit takes it
  off again. A marker on a member who is casting keeps its full window, a confirmed marker is watched
  until it has gone, and an action that has once hit the whole party is not taken for a marker's hit.
-  **Channel locks hold actions only until the announced hit lands.**
  `Don't break Passage of Arms` is **on by default**: Passage
  of Arms ends with the paladin's next action, and RSR casts it only for an announced area hit, so
  without the lock the next GCD or weave often ended the channel before the hit.
  `Don't break Collective Unconscious` stays off by default.
- **Help for the one a tankbuster is aimed at.** When a tankbuster marker sits on another party
  member — most often the other tank — or an enemy casts a known tankbuster at them, every tank puts
  its help for others on them before the hit: Intervention (`Use Intervention on CoTank during
  tankbusters`), Nascent Flash (`Use Nascent Flash on another party member a tankbuster is aimed at`),
  The Blackest Night (`Use The Blackest Night on another party member a tankbuster is aimed at`), all
  **on by default**, and Heart of Corundum. Healers put their single-target mitigation on the same
  member instead of whoever the boss is attacking, and Heart of Corundum and Aurora on `Only use on
  tankbuster targets prioritizing self` reach the other tank. Intervention needs no tank stance on its
  target. Help is not given to a member who takes the hit under Hallowed Ground or Superbolide while
  `Spend only the mitigation a measured tankbuster needs` is on; with it on, a healer's single-target
  mitigation and a tank's help also stay off a tank whose measured tankbuster takes less than a large
  shield absorbs and leaves him at or above your single-target heal threshold, and a BossModReborn
  prediction beside it no longer sends them anyway. Reprisal and the party mitigation some jobs spend in the
  single-target defence wait the same way while such a tankbuster on someone else is all that is coming.
  The first time a tankbuster is met nothing changes: it is measured with that hit.
- **An enemy's cast is stopped by the job's stun first (experimental)** — `Stop an enemy's cast with the
  job's stun or a sleep (experimental)`, **on by default**. Healers and casters stop an area attack of an
  enemy that is not a boss reaching you or two party members, any cast the game marks interruptible —
  cones, rings and small areas included, which the melee interrupt leaves alone —, and a single-target
  cast, such as a debuff on you, that a sleep has stopped before, when enough of it is left to land in
  time. An enemy that fully resists a sleep you cast yourself is remembered as immune too. A White Mage uses Holy when the caster stands in its radius
  and can still be stunned; otherwise Repose or Sleep goes on the enemy. Swiftcast is never spent on it,
  a healer does not use it while a member is about to die, and with `Don't pull new enemies with AoE` on
  no area stop reaches an enemy not yet fighting. What sleeps show is kept over every session: enemy kinds
  that resist are not slept again, nor an enemy slept as often as its kind takes before it resists, nor an
  attack that went off although the sleep landed in time; one that landed too late makes the next land
  earlier. A stop counts only when no effect follows within a GCD. `DefenseTrace.log` records every stop
  and sleep and what came of it.
- **Without a living healer, your defence keeps you alive** — `Without a living healer, spend your
  defence to stay alive longer`, **on by default**. With no healer of the party alive, the single-target
  defence opens while an enemy attacks you, whatever your role, and a mitigation goes as soon as it will
  be ready again before the next tankbuster on you is due — by BossModReborn's prediction, otherwise by
  the spacing of the tankbusters on you in this fight. A tank drops the pull hold and the fixed spacing,
  and against a tankbuster everything ready goes on it.
- **Interrupt and provoke delays** do what their text says: `Interrupt delay range` and `Provoke delay
  range` delay the interrupt and the provoke, by default not at all, and an interrupt never waits past the
  cast it is meant to stop. A configuration saved earlier keeps its value.
- **Under Pyretic and similar debuffs RSR takes no action** and cancels a running cast for as long as the
  debuff lasts, recognised by the debuff's own text, with or without a BossModReborn module. A debuff that
  punishes when it runs out, such as Acceleration Bomb, holds RSR only shortly before its end.
- **The BossModReborn timeline is only read when it is switched on**, and a defence pressed for a hit
  it announces goes out only while a press made now still reaches the server before the end of the
  cast, from your measured round trip — no fixed margin.
- **An enemy cast missing from the game data is passed over.** The area attack and sleep checks skip a
  cast the action sheet has no entry for; before, looking it up failed, and for every frame of that cast
  the rest of the update step was skipped.
- **Each ability picks its own target.** A target rule set by the GCD no longer carries over to the
  commanded action, the emergency abilities, the tankbuster plan and the tank swap: the Paladin's
  emergency Intervention goes to the co-tank, not to whoever has the least HP.
- **No spell runs into Pyretic.** A status that damages every action - Pyretic, Causality - comes with the hit that
  lays it, and an action still under way at that moment takes its damage at once. RSR learns, from every landing on you
  or a party member, which enemy casts lay such a status, and before one of them lands on you it starts no spell that
  would still be casting, presses nothing that would still be under way, and cancels a running cast that would meet it
  - each counted with your measured round trip. Instant actions and casts that finish in time go on, and an enemy cast
  stopped before its end releases at once. BossModReborn's imminent Pyretic counts too. The learned casts are kept in
  `ActionPenaltyCasts.json` beside the other learned tables; a status taken off `No-casting debuffs` is no longer learned
  or waited for.
- **A tankbuster on both tanks gets help on both.** A White Mage's Divine Benison and Aquaveil, and an Astrologian's
  barrier after Macrocosmos or Collective Unconscious, wait for each other only on a target that still carries the
  other's effect. Under a tankbuster on both tanks, Aquaveil goes on the tank without Divine Benison at once instead of
  waiting 15 s, and a tank whose barrier the last hit took gets the next help for the next hit.
- **Gunbreaker: Heart of Corundum on "Full target usage" goes to who needs it.** It went to the member with the fewest
  hit points - most often a caster at full health with a small pool - while the gunbreaker took the hit himself. It goes
  to the tankbuster's target first, the gunbreaker before others, otherwise to the member the heal target order puts
  first; with a tankbuster on him before it is back, another member gets it only to save a healer or a tank in danger.
- **Delays never make an action too late.** The provoke delay does not apply when taking an enemy from a co-tank, and
  otherwise ends with the enemy's first hit on the party member it turned to. The dispel delay ends when a dangerous
  status would run out within a GCD. The interrupt delay already ended with one GCD of the cast left. A stored interrupt
  or provoke delay still at the old default of 0.5 to 1 s is set to none on load.

## Tank self-protection

- **A tankbuster on the other tank no longer spends your own cooldowns.** The single-target defence
  opened for every tankbuster BossModReborn announced and for every cast an enemy aimed at its own
  target, so Damnation and Rampart went out for a buster BossModReborn had marked as not hitting you.
  It now spends a defence that protects only you only when the hit reaches you (see "Area defence asks
  whether the party is hit"); an announced tankbuster whose targets BossModReborn does not name still
  counts as hitting a tank.
-  **A measured tankbuster gets the mitigation it needs** —
  `Spend only the mitigation a measured tankbuster needs`, **on by default**. When a tankbuster the
  table has measured is cast at you, RSR works out what it takes under what will still stand when it
  lands — your own mitigation, a co-tank's Reprisal, a healer's mitigation or barrier — against your
  HP now, and spends the cheapest set of your own ready mitigations, by cooldown, that lets you
  survive it: one if one is enough, two or more stacked if not, none if the hit is survivable as it
  stands, each pressed late enough to still stand when the hit lands. Party tools such as Shake It
  Off or Divine Veil are left to the area defence. While that tankbuster is the only one known to be
  coming at you, everything else aimed at you is held for the next one, from whichever part of the
  rotation would have used it; help for another party member, heals and area defence that reaches
  beyond you still go out. If a planned mitigation has not gone out by the last GCD before the hit,
  the hold opens. The plan is recomputed all the time, so a heal or a Reprisal that lands before the
  hit makes it smaller. A measured tankbuster cast at no other party member while a tankbuster marker
  stands on you is planned like one cast at you. A set counts only if it leaves you above `The HP% for
  tank to use invulnerability`; otherwise the plan takes the invulnerability. Reprisal joins a set only
  when no set without it survives, so it stays for the next raidwide. The plan stops pressing one
  measured round trip before the end of the cast, retries a press refused under a stun as soon as the
  stun ends, and takes the invulnerability when the presses left are too few for the cheapest set.
  Below the level of the trait that sets their duration, Reprisal, Sheltron, Nascent Flash and
  Intervention count with the duration their tooltip shows at your synced level. Tankbusters announced
  only by BossModReborn, and actions not yet measured, keep the ordinary single-target defence.
-  **The invulnerability only when nothing less survives** —
  `Use the invulnerability before a tankbuster nothing less survives`, **on by default**. Only when
  no set of your mitigations would get you through does RSR use Hallowed Ground, Holmgang, Living
  Dead or Superbolide before the hit — no earlier than its ten seconds less one GCD. With the
  mitigation setting off, only what already stands counts. It does not depend on the dying-tank
  threshold: setting that to 0 switches off only the old reactive use. Only if nothing survives and
  the invulnerability is not there to use — on cooldown, switched off or refused with no retry left
  before the last GCD before the hit — does everything go, in the hope of someone else's debuff or
  barrier; never on top of the invulnerability. Under Hallowed Ground or Superbolide past the hit,
  nothing aimed at you is spent, and a healer spends no single-target mitigation for a tankbuster on
  a tank standing under one of them past the hit.
- **A marker without a cast is met with the hit that followed it before.** RSR learns per area and marker
  which hit followed and how soon, and plans the next one like a tankbuster with a cast: the least
  mitigation that survives it, or the invulnerability if nothing less does. Mitigation is held back only
  under a planned invulnerability.
- **The time a press needs to reach the server** is measured from the moment the request actually leaves,
  matched to the effect that answers it; every press counts, your own too.
- **Pull mitigation counts what attacks you.** `Use tank mitigation when this many enemies target you`
  opens the pull defence whenever that many enemies within 3 yalms target you and you have attacked one of
  them; unpulled packs nearby, casters at range or the other tank's adds no longer keep it shut.
- **The big mitigation waits in a pull until your health needs it** — `Hold the big mitigation and
  Rampart in a pull until your health needs them`, **on by default**, with `Health the big mitigation
  waits for` (30 %). Your short mitigation, Arm's Length and Reprisal go at once; the big mitigation and
  Rampart wait until your health, at the rate measured net of every heal and mitigation, would fall below
  that line within their duration, and then each goes as soon as it is needed. A tankbuster, a cast aimed
  at you, a BossModReborn tankbuster and danger to you or a tank open them as before; a trash mob's spell
  at you no longer does — only a known tankbuster on a tank (a marker, a cast from the tankbuster list or
  one RSR has measured).
- **One big mitigation per predicted tankbuster, first in line.** For a tankbuster BossModReborn
  predicts on you, every tank casts its big mitigation (Damnation, Guardian, Shadowed Vigil, Great
  Nebula and their lower forms) before the smaller ones, and Rampart only when the big one is not
  taking that tankbuster, so with tankbusters about a minute apart they alternate instead of both
  landing on one hit and neither on the next. Reprisal in the single-target defence waits for an
  announced raidwide when the tankbuster is on you, your big mitigation or Rampart already runs, and
  the raidwide lands after Reprisal would have run out and before it is back.
-  **Shirk hands the boss to your co-tank when the next tankbuster would kill you** —
  `Shirk the co-tank after a tankbuster that leaves you in danger`, **on by default**. With another
  tank in the party, RSR Shirks that tank after a tankbuster on you when the buster left you with a
  vulnerability debuff or so low that a repeat of the hardest buster measured would kill you, and
  your invulnerability is not ready. It does so only when the party's enmity on the boss shows the
  co-tank will take it over: Shirk moves a quarter of your enmity, so below half of yours nothing
  would change. A co-tank in danger themselves gets nothing; of several candidates it picks the one
  the fewest enemies are attacking, then the one with the most HP. Once your co-tank has taken the
  next tankbuster, your debuff has run out, you are above that line again and your invulnerability
  is ready, RSR provokes the boss back; while you are still in danger, the automatic Provoke no
  longer takes the boss off a co-tank who drops low. A Shirk that did not move the boss raises the
  bar for the rest of the fight. The manual Shirk command no longer targets you yourself.
- **The Blackest Night** repays its 3000 MP only when the barrier is fully absorbed.
  `BlackestNightUsage` defaults to a tankbuster, a big pull with no other mitigation, or below the
  health threshold; in a pull it also waits until the damage you took over the barrier's duration reached
  the share its text names (`In a pull, use The Blackest Night only once the damage you take would break
  it`, **on by default**), so a soft pack keeps the MP for Edge of Shadow. On a pull it waits while a White Mage's Holy keeps the pack stunned — one GCD
  after the stun ends, longer only while a Holy is visibly being cast into the pack. The white mage
  can hold Holy while a tank carries the barrier (`HoldHolyForBlackestNight`). It and Oblation on a
  low party member are not held in the burst window, and they pick their target among the members under
  their health threshold.
- **Dark Knight: `Keep at least 3000 MP` also when Darkside runs out.** Edge of Shadow keeps what The
  Blackest Night costs, unless Dark Arts makes it free; with little MP, Darkside may lapse until there is
  enough.
- **Living Dead and Walking Dead.** `WithholdHealingForLivingDead` is **off by default**, since RSR
  also fires Living Dead as a last-ditch save: then Living Dead counts as protection nowhere RSR decides
  on healing, and the dark knight is healed normally, as the setting's text promises. With it on, a
  lowered healing threshold applies while more than two GCDs of Living Dead remain, so the death effect
  can occur; within two GCDs the normal threshold returns. Under Walking Dead the dark knight heals himself
  by attacking: healers give him a HoT, and full healing starts once the timer runs out, no enemy is
  in reach, BossMod announces a downtime, a tank limit break is up, or his own course will not reach
  full health in time.
-  **Warrior: Thrill of Battle and Bloodwhetting before the hit.**
  `Use Thrill of Battle before a tankbuster on you` (**on by default**) casts it ahead of an
  announced tankbuster on you: 20 % more maximum HP, and the healing after the hit 20 % stronger; it
  stacks with Bloodwhetting instead of waiting behind it. On a pull,
  `Use Thrill of Battle when your health will fall below its threshold within its duration` (**on by
  default**) casts it as soon as your health, falling at the measured rate, would pass
  `Thrill Of Battle Heal Threshold` within its ten seconds; where the healers hold you steady, it
  waits. `Use Bloodwhetting/Raw intuition on single enemies` is **on by default**, so Bloodwhetting
  also goes out for a lone boss's tankbuster. A running Bloodwhetting no longer stops the defence or
  counts as one of the big mitigations Rampart and Damnation wait for; those two still stagger
  against each other.
- **Warrior: Shake It Off keeps your Damnation for the tankbuster.** Shake It Off dispels Damnation,
  Bloodwhetting and Thrill of Battle. As a heal for a single party member it waits while Damnation or
  Bloodwhetting runs, or while a raidwide is announced after its barrier would end and before it is
  back; at a raidwide it waits only when a tankbuster on you lands before Damnation ends. Both give way
  when a party member is about to die. Dispelling Thrill of Battle is allowed: the barrier is sized on
  the raised maximum HP.
- **Arm's Length goes out for its Slow, not for a boss's tankbuster** (`UseArmsLengthOnPull`, all
  four tanks, **on by default**): +20 % recast on every enemy that strikes you. A pull is the global
  "Use tank mitigation when this many enemies target you" for defensive abilities, counted on ordinary enemies only — bosses do not count;
  a slowed pack is left alone, and a BossModReborn knockback in the gap keeps it. The single-target
  defence no longer casts it as a last resort on a boss alone in the arena, where it softens nothing and
  is then missing for the next knockback, and for Paladin, Warrior, Dark Knight and Gunbreaker this
  setting has the last word.
- **The co-tank Provoke** no longer pulls the boss off a tank standing under Superbolide,
  Living Dead or Holmgang.
- **Taking the enemy from a co-tank in danger** has its own setting, `Take the enemy from a co-tank in
  danger` (**on by default**): at your invulnerability HP; when BossModReborn announces his next
  tankbuster and he would take it still vulnerable or would not survive it by his HP trend — at once,
  since a cast already aimed at him or a marker on him cannot be turned away; and when he shirks to
  you. Not while you are in danger or vulnerable, not while he is impervious, not when the announced
  tankbuster hits you anyway. Your stance goes on first if it is off. If the enemy's next tankbuster
  still hits him, RSR stops taking that enemy on an announcement for the rest of the fight.
  `Provoke when there is another tank in the party` does only what its text names, and `Provoke
  anything not on the no provoke list.` respects the list.
- **A tankbuster of several hits is planned as one.** The tankbuster table adds up what all hits of one cast take on a
  tank - also hits that land under another id or from a helper, matched to the cast by its name - and keeps the sum
  under the cast the plan reads, with the time from the first hit to the last. The plan sizes its mitigation for the sum
  and counts only mitigation that still stands at the last hit, and it presses the invulnerability late enough to last
  through the series, at the latest one GCD before the cast ends. An invulnerability that runs out before the last hit
  no longer holds the rest of your defence back. A tankbuster of one hit is planned as before.

## Movement safety

With `Use BMR intergration to verify safety of movement actions…` (off by default):
- **A gap closer is not withheld when there is nothing to close** — at 0 yalms between the
  hitboxes, the distance the game shows. The check measured the line to the boss's centre, so an
  area under a large boss refused the dash, and on Summoner Crimson Strike with it.
- **Leaps count as movement**: Dragonfire Dive, Stardiver and Forked Raiju.
- **Backsteps and fixed dashes are checked**: Hell's Ingress and Egress, Hissatsu: Yaten,
  Displacement and Repelling Shot go out when their landing point, computed from the distance in the
  effect text, lies outside a danger zone. Repelling Shot jumps away from the enemy.

**Gap closers that leap onto the target keep to your distance.** `Only use damaging gap closers within
this distance` (3 yalms by default) holds for the leaps as for the dashes — Intervene, Primal Rend,
Crimson Cyclone, Stardiver, Dragonfire Dive —, together with any limit their rotation sets. A move-forward
command, your own press, the countdown and the switches marked "Dangerous" or "at any range" are not
limited; standing at the enemy, a leap is only damage.

**Monk, Viper and Dragoon dash to the target at the pull only from out of range** — `Dash to the target
at the end of the countdown when out of melee range`, **on by default**: in the last half second of the
countdown, only when the target stands beyond melee range, and only to it.

`No fall damage` under Extra (off by default): the game's fall damage function returns zero, so a
fall from a height costs no HP. The hook and its signature come from Henchman's hacks (AGPL-3.0,
credited in the file). If the game version does not have that function, the setting switches itself
off.
- **Fixed dashes check where they land.** Hell's Ingress, Hell's Egress, En Avant, Smudge, Aetherial Shift and Elusive
  Jump - the dashes without a target - went out on "Move Forward" or "Move Back" wherever they would land. With
  BossModReborn running they now wait while the landing point lies in a danger zone, for as long as the command lasts
  ("Special state duration", 3 s by default), like every dash onto a target already did. "Skip BossModReborn
  position-safety check for this movement action" in the action's settings turns it off for that action.
- **No ranged attack right after every dash to the target.** Lightning Shot after Trajectory, Writhing Snap after
  Slither, Tomahawk after Primal Rend, Piercing Talon after Stardiver or Winged Glide and Throwing Dagger after Forked
  Raiju are held for one action, as they already were after Intervene, Onslaught or Dragonfire Dive: the player stands
  at the target now. After a jump backwards (Elusive Jump) the ranged follow-up stays allowed.

## The learned lists

- **The damage table measures at all.** The effect handler read the action type four bytes wide,
  so no area action was ever rated. Measuring no longer depends on `Record enemy AoE actions` or on the
  party size; that setting decides only whether new actions are added. Every enemy-side figure is
  read, casts and hits alike — the helpers and parts through which many bosses hit included — so their
  raidwides join the AoE list and their tankbusters are measured.
- **Tankbusters are learned, from every tank they hit.** Every hit of a listed tankbuster, or of
  one whose marker was confirmed, counts on any tank — you, your co-tank, or tanks of other parties in
  large content — scaled back to unmitigated by the mitigation that stood on the target and on the boss
  (each status by its own text, Intervention, Taurochole, Third Eye and the 15 % of Troubadour,
  Tactician and Shield Samba from level 98 included), and stored per action as a share of maximum HP;
  a hit taken under a vulnerability debuff is kept apart, and hits from an enemy carrying a Damage Up
  status are not stored. A hit on a player who is not a tank lacks the tank trait and is not stored. The tank swap and the tankbuster
  plan read this table, so a tankbuster from an earlier evening or from your co-tank is known. The list
  shows each entry's figure and the store's state.
- **A series of hits counts as one attack.** A listed area cast whose landings follow faster than the
  enemy could cast it again is rated by what the series takes from a member, added up, not by its largest
  hit.
- **An area cast without damage is rated zero.** A landing at zero on at least two members of whom none
  carried a barrier rates the cast at zero, so `Skip mitigation for small area casts` lets it through; a
  later landing with damage rates it again.
- **No reading is lost on the way to disk.** Every save merges memory and file and keeps the higher
  reading; a file that cannot be read is left alone; a list whose load did not finish is not written
  on unload. The AoE, knockback, tankbuster and tankbuster marker lists are saved the same way.
- **A member who missed a kill mechanic no longer sets an area cast's size.** A hit that reaches a
  member's whole health and his barrier is not measured; one he survived only behind his barrier counts
  as his whole health. Readings above a whole maximum HP are dropped when the table loads.
- **The AoE list window** shows the store state under `Store:`, `Casts this session` by what decided
  them, `Last hit:` with the reason a cast was or was not rated, and the effect handler's counts.
- To start over after a patch, delete `HostileCastingAreaPotential.json` or `TankbusterPotential.json`
  while the game is closed.
- **A list whose load did not finish is not saved**, so a cancelled start cannot write "every entry removed"
  over your list changes; an unreadable list file is set aside and the built-in list stands.
- **The area and tankbuster tables count every reading under the game version it was taken in.** Nothing changes in a
  fight: every rule still reads the highest valid reading. Each reading is kept, not only one that raises the value, in
  `HostileCastingAreaPotential.rev.json` and `TankbusterPotential.rev.json` beside the old files, which RSR goes on
  writing; after the next patch RSR can tell which values the new version has confirmed. To start one of these tables
  over by hand, delete both of its files with the game closed.
- **A learned table that cannot be read comes back from its backup.** Every learned table keeps a copy of its last save
  that was read back (`.bak`). A file that cannot be read is set aside, and the backup is loaded instead of starting
  empty. A file you deleted stays deleted.
- **A tankbuster nothing names is learned from its first hit.** A tankbuster that neither the tankbuster list nor a
  marker names - RSR knows it only because the enemy casts it at its target, a tank - is learned from its hit on a tank
  like any tankbuster's, and from the next cast at you the plan spends what it needs, the invulnerability included.
  Nothing else is held for it, it does not count as a known tankbuster for opening the defence or for help from others,
  and a hit that reaches more than one member is not learned as one.
- **A raidwide landing on a tank under Hallowed Ground or Superbolide joins the area list,** although the tank's hit
  carries no damage then; a tankbuster that a patch turned into a raidwide no longer draws the tankbuster plan.
- **Nothing measured outside the content's sync is learned.** Above the content's level, or in an unrestricted party
  from the duty finder at the same level, every hit is smaller than in sync. The area table, the tankbuster table and the
  damage over time on the party skip such readings; the open world counts as in sync.
- **An area hit counts at its unmitigated size.** The damage table divides out the mitigation that stood when a raidwide
  landed - on the member and on the boss, by the figures the effect texts state -, so a raidwide first seen under three
  mitigations is no longer rated small. Every rule that reads the table reads it under the mitigation that will still
  stand when the cast lands: what others have already laid makes the hit smaller, and your own party mitigation goes out
  where the rest of the hit would still push someone to where healing is called for, or is at least the size of a large
  barrier. A barrier is not divided out.
- **A learned figure from an earlier game version saves no protection until it is confirmed.** The area and tankbuster
  tables now tell a figure the running game version has measured cleanly from one taken over from an earlier version
  (or from the file before the revisions). A taken-over figure still protects - a big raidwide, a lethal tankbuster, a
  long series are met as before - but it no longer leaves protection out: a small raidwide is mitigated like an unrated
  one, a small tankbuster holds nothing back beside the plan, and help for the tank is not held, until the action lands
  cleanly again. A patch that made a hit harder can no longer take the first cast's cover. Each such case writes a line
  to `DefenseTrace.log`, and the trace's head line counts the confirmed entries.
- **A hit a patch weakened lets go of its old figure.** In the area and tankbuster tables the running game version now
  refutes a figure taken over from an earlier one: a fixed hit after one clean landing more than a damage roll below it,
  a varying one after as many clean landings as the old figure had, and a figure from the file before the revisions only
  after three clean landings that agree with each other. From then on the new figure counts, protecting and saving; a
  harder hit raises it again at once. The old figure is kept, not read, and the old file still holds the highest of all
  readings. Each refutation writes a line to `DefenseTrace.log`.
- **A tankbuster marker locked in an earlier game version counts again.** A marker the fight once showed with no hit on
  the marked member is locked as "no tankbuster" only in the game version that saw it. After a patch, and on the first
  start of this build, it counts as a tankbuster until it goes without a hit again - one defence per marker and version
  for a tank that a patch may have turned it against.
- **Sleep locks from an earlier game version get one try.** An enemy kind that resisted a sleep, a sleep limit, and an
  action a sleep did not stop held for good, since those files only grow. Now each holds in the game version that saw
  it; one from an earlier version, or from before this build, gets one sleep in the running version - only when no heal
  spell is due and nobody is in the critical class, so the try never costs a heal. A resistance holds it again until the
  next patch; a sleep that lands, from anyone, ends the lock for a kind or a limit, and a stopped cast for an action.
  Each try, confirmation and refutation writes a line to `DefenseTrace.log`, and the head line counts the locks.
- **A cast that no longer lays Pyretic stops holding spells.** A learned action that lays a status damaging every
  action keeps holding spells after a patch. It lets go only when the running version sees it land on its target, a
  party member, without the status at least twice, more often than it laid it before, and never with it; it holds again
  the moment it lays the status. BossModReborn's Pyretic announcement holds regardless.
- **The learned tables can be seen.** The list window has a new section, "Learned tables", with one tab per store:
  area hits, tankbusters, area and hit series, sleep outcomes, sleep-resistant kinds and limits, damage over time,
  casts laying Pyretic and tankbuster markers. Each entry shows the value the rules read and its state in the running
  game version - confirmed, taken over, refuted, a lock with its try left or spent. Search and copy; nothing can be
  changed there.
- **The marker and lock files keep a backup.** The tankbuster marker files and the new lock files fall back to their
  last read-back copy when unreadable, so a measured hit after a marker is not lost with a broken file.

## Damage and rotation

- **Summoner: Searing Light stays with the burst.** It is released as soon as the summon will be
  ready by the next GCD, and the summon waits for it when it cannot go out ahead; a buff that fell
  behind is pulled back. As the only Summoner the slot ahead opens only for the burst demi (Solar
  Bahamut; Demi-Bahamut below level 100). With other Summoners the charge stays with Solar Bahamut
  until another Summoner takes it: Bahamut and Phoenix open only after another Summoner's Searing Light
  stood in your last Solar Bahamut window, or stands in the current one; otherwise the charge waits for
  the next Solar Bahamut, at most one demi longer. With other Summoners holding every burst phase it
  falls back to Titan — or Ifrit when you stand at your current target — and never waits for a buff
  blocked by theirs. `PreferTitanWhileMoving` (**on by default**) brings Titan forward while you are
  moving. Which demi comes next is read from the job gauge first, then the summon button, then the
  order seen, each checked against the demi that actually comes; a source that named the wrong one is
  not asked again for the session.
- **Summoner: Searing Flash no longer expires unused.** Outside a demi it no longer waits for a dying
  boss but goes out in the last weave slot before Ruby's Glimmer runs out — the case beside other
  Summoners, where Searing Light can fall into a Titan or Ifrit block and the next demi is further off
  than Ruby's Glimmer lasts. As the only Summoner it falls inside Solar Bahamut as before.
- **Summoner: the odd-minute Necrotizes wait for the two-minute window.** The two Necrotizes from the
  odd-minute Energy Drain wait until the burst summon and Searing Light are back no later than Energy
  Drain, which follows them by a weave — as The Balance's Summoner guide describes.
- **Summoner: Radiant Aegis goes out before a demi**, which replaces Carbuncle for 15 seconds; the
  big summon waits for a due shield. During a demi it is not chosen, since the game allows it only
  while Carbuncle is summoned, and Addle comes in its place.
- **Summoner: Lux Solaris** goes out only when it does some good inside its radius, the same way on
  every path: never under Scalebound or Shackled Healing with others near, or while a Dark Knight in
  its radius waits for Living Dead to trigger; when as many are hurt as its AoE count asks; when your
  own missing health or everyone else's takes a full heal; at once for someone in danger; and before
  Refulgent Lux runs out whenever anyone in the radius is hurt. The heal amount is the smallest full
  heal measured since the last zone change.
- **Summoner: Rekindle** picks by share, not by points, goes on the caster when nobody needs it, and
  its last-resort fallback waits for the end of the Phoenix phase, read from the job gauge.
- **Damage over time on several enemies: most HP first, then by expiry** — `Spread damage over time by
  HP, then by expiry`, **on by default**. Every job's damage over time, recognised by the status's own
  text, goes first on the enemy with the most HP, and once each carries it, it is renewed in the order it
  runs out; not where a rotation uses the action for something else or names the target. Direct
  single-target attacks keep the targeting type you chose, unless the action brings its own order: Red
  Mage's Corps-a-corps jumps to the nearest enemy.
- **Damage over time refreshed by its ticks** — `Time DoT refreshes from DoT ticks and time to kill
  (Experimental)`, **on by default** (off upstream). White Mage, Astrologian, Scholar, Sage, Black Mage
  and Reaper refresh on the last GCD that loses no tick, and leave a DoT off a target that will die before
  it out-damages the filler.
- **Damage areas wait until the pack stays** — `Lay damage areas only where the enemies stay`, **on by
  default**. Salted Earth, Doton and Slipstream wait until an enemy has been in their radius for a GCD and
  no fewer are in it than were at the start of that GCD; also while an enemy not yet in combat stands in
  the area or, in an instanced duty, within a tank's reach, and for a GCD after one came into combat.
  Doton gives way to Katon meanwhile.
- **Granted windows no longer run out unused.** An action that needs a status of yours is usable as
  long as the status lasts until it goes off, not locked out for the last two GCDs. Ogi Namikiri,
  Sonic Break, Reign of Beasts, Phantom Kamaitachi and Hypercharged are spent before their Ready runs
  out; Retrace only while Ley Lines outlasts the next GCD.
- **Abilities when no GCD is coming** no longer wait out the last half second of the GCD timer — while
  you move as a caster, say; that half second only keeps an ability from delaying a GCD.
- **Line attacks count the enemies on the way to their target.** Doom Spike, Draconian Fury and Coerthan
  Torment, Chain Saw, Apex Arrow and Blast Arrow, Shadowbringer and Enlightenment were counted along the way
  you faced; they are counted towards the target, and of several targets the one whose line hits the most
  is taken.
- **Samurai and Dark Knight: AoE decisions count what the AoE hits.** The Samurai's AoE Iaijutsu and
  Hagakure count the enemies within the AoE combo's radius against its AoE count; Ogi Namikiri on the AoE
  path goes when it hits more than one. Abyssal Drain takes Carve and Spit's place where it out-damages it
  on the enemies it reaches, by their tooltips.
- **Machinist: Flamethrower on a pack that stands still** — `Use Flamethrower on a pack that stands still`,
  **on by default**. It goes in place of the filler when at least two enemies stand in the cone (its AoE
  count), the pack has stopped, no enemy casts an area attack, Hypercharge is not available and no tool,
  Wildfire or Barrel Stabilizer comes ready in its ten seconds — The Balance's rule. While it channels,
  RSR keeps its own actions and medicine back; heals, defence, interrupts, cleanses, raises, provokes and
  your presses still go.
- **Machinist: Auto Crossbow only where it out-damages Blazing Shot**, weighed with the potencies the
  tooltips state and the enemies Double Check and Checkmate reach: from six enemies with all of them in
  reach, from four with one outside; the AoE count stays the lower limit.
- **Ninja: area actions from two enemies where they deal more** — Hellfrog Medium (Deathfrog Medium)
  instead of Bhavacakra (Zesho Meppo) on two enemies unless Meisui raises the single hit, Goka Mekkyaku
  instead of Hyosho Ranryu under Kassatsu, counted on the enemies Goka reaches; Doton only while the
  enemies live through its duration, otherwise Katon.
- **Black Mage: Foul from two enemies**, weighed against Xenoglossy with the tooltip potencies.
- **Black Mage: Ley Lines by default** — `Use Leylines in combat when standing still`, **on by default**. It
  goes down once you stand still in combat with an enemy in range, keeps the second charge unless `Use both
  stacks of Leylines automatically` is on, and waits while an enemy casts an area attack you may have to
  leave the circle for. If you place it yourself, turn the setting off.
- **Scholar: Bio by the measured time to kill.** The ballpark estimate of `Enable ballpark DoT time-to-kill
  estimator` holds Bio back only until a time to kill is measured for the enemy.
- **Tinctures wait for the fight or its countdown.** No rotation drinks one out of combat unless a
  countdown runs: waiting at a boss without a countdown or between two packs, a ready burst ability no
  longer sets it off before the pull.
- **Viper:** `Experimental Pot Usage(used up to 5 seconds before SerpentsIre comes off cooldown)` (off by
  default) also takes the potion while Serpent's Ire stands ready, so in the opener too.
- **Viper and Monk: "just used" no longer means "ready".** Uncoiled Fury after Serpent's Ire waits for
  Serpent's Ire instead of spending coils beside a ready one; the overcap rule still spends at the cap. The
  Monk's odd-minute Perfect Balance is no longer kept back with both charges full, and Phantom Rush no
  longer counts a ready Riddle of Fire as used.
- **Monk: both Perfect Balances of the two-minute window.** Perfect Balance goes after the opo-opo
  weaponskill one to three weaponskills before Riddle of Fire, as The Balance has it, whenever Brotherhood
  comes with that Riddle of Fire; the first no longer slips under Riddle of Fire and the second no longer
  misses Brotherhood.
- **Monk: Six-sided Star as the last hit** — `Use Six-sided Star as the last weaponskill before the target
  dies or goes untargetable`, **on by default**: when only one more weaponskill lands before the last enemy
  in range dies by its time to kill or BossModReborn's timeline predicts a downtime; a ready Fire's Reply
  or Wind's Reply goes first. It needs no open chakra, as in the game.
- **Machinist:** with `Only use Wildfire on Boss targets`, Heat is not held for Wildfire on trash;
  Wildfire cannot go out as Detonator; Queen Overdrive lands before a predicted untargetable phase.
- **Samurai:** Meditate when no enemy is within 25 yalms in combat.
- **Reaper:** Hell's Ingress and Egress cannot go out as Regress. Soulsow also goes out in combat
  while no enemy is within 25 yalms, so every Harvest Moon after the first is there; with a
  BossModReborn module running and `Use BossModReborn's timeline for proactive mitigation` on, it waits
  when its 5-second cast would not finish before the predicted end of the pause.
- **Dark Knight:** Blood is pooled for the burst — Bloodspiller and Quietus under Delirium, in the
  two-minute window or above 70 Blood.
- **Sage:** Addersgall is spent on Druochole just before it overflows.
- **Dancer:** when the dance partner dies or carries a raise's weakness, Closed Position picks a
  partner within range; a partner chosen by name is replaced only for death or weakness.
- **White mage, Holy.** `Stretch the Holy stun` (**on by default**): right after Holy, the next GCD
  goes to a DoT where one is due, otherwise to Glare, while Holy's stun would still be running when a
  second Holy lands, so the second stun starts where the first ends — about 7 instead of 5.5 seconds of
  stun per pull for one GCD of Holy damage. RSR reads from the fight which enemies a stun landed on, how
  often, and which turned it away, and holds only while the last stun would outlast the next Holy; a
  pack the game no longer stuns is not held for. The same reading decides when the White Mage's other
  two Holy holds and the Dark Knight's barrier hold see nothing left to stun. Holy is held while the
  dark knight's barrier is meant to be filled (`Enemies in Holy's radius before holding` applies to
  that hold only), and while more than half the enemies in radius are slowed (`HoldHolyWhilePackSlowed`).
- **Thin Air** is only spent under real MP pressure that Lucid Dreaming cannot answer
  (`ThinAirOnMpPressureOnly`, **on by default**). A raise still takes a charge regardless.
- **Gunbreaker: cartridges go into the pack.** In a pull a cartridge goes on Fated Circle once its number of enemies is
  met, Gnashing Fang does not start against three or more, and Sonic Break goes on up to five, at four and five only
  when the target lives its whole duration by its measured time to kill, at three half of it - as The Balance's guide
  has it. Before, most GCDs under No Mercy went into one enemy, and the damage dealers drew the rest.
- **The time an enemy has left is measured.** The self-correcting time to kill was kept for party members only. It is
  kept for enemies too: the Monk's Six-sided Star as the last weaponskill comes before the target dies, and the Scholar
  holds Bio only where the target's measured time is too short.
- **Red Mage: Manafication waits for Embolden only where that costs no use.** When the boss's measured time to kill
  says the wait would cost the last Manafication, it goes at once; until then, and while no time to kill is measured, it
  waits as before.
- **Pictomancer: no motif under Starry Muse in the opener.** Below level 92 Starry Muse comes two seconds into the
  fight, inside the opener's first five seconds; the opener's priorities end once Starry Muse is up, as the rest of the
  rotation already does.
- **Warrior and Dark Knight spend into the pack only where it pays.** Their area spenders went out from two enemies on,
  although on two or three they deal less than the single-target ones: on two enemies Decimate dealt 360 where Fell
  Cleave deals 580, Chaotic Cyclone 400 where Inner Chaos deals 700. Each now goes out from the count where it deals at
  least as much, by the tooltips of the running patch: Decimate and Chaotic Cyclone from four, Orogeny, Steel Cyclone,
  Quietus, Flood of Shadow and Impalement from three. Below that the single-target spender goes in the same place, so
  the gauge is spent as before; the area combo still holds the enemies from two on.
- **Beastmaster calls its familiars by the situation and by what you have.** Among the ready battlehorns the familiar
  whose Trick brings least against the enemies in combat around your target is called first, so an area familiar on a
  pack keeps the long stint until a horn is back. When your health is falling below the line at which RSR heals you with
  abilities, a familiar that heals you (Digest, Blood Drain) or protects you (Tempered Release or Borrow) is called first,
  one that does neither leaves by Parting Blow for it, a healing one stays out while the danger lasts, and Scaleskin goes
  up at once. Only the familiars on your horns and only the actions you have at your level count. New option "Choose the
  battlehorn by the situation", on by default.
- **Beastmaster no longer keeps a familiar out for good.** A horn set to Borrow before level 22, or to "Neither", never
  spent One with Nature, and Parting Blow waited for it: the familiar stayed and the other horns sat idle. A horn now waits
  only for the action it spends One with Nature on, and Borrow that cannot come yet is Tempered Release.
- **Beastmaster no longer rushes back into danger for a finisher.** Shield Charge, Brutal Rage and Hawkish Talons carry
  you to the target from up to 20 and 25 yalms; they now keep to "Only use damaging gap closers within this distance"
  (3 yalms by default) and check the landing point. Risen Fall, the same 1,200 without the rush, now goes before Brutal
  Rage, and Calamity before Hawkish Talons unless the rush reaches more enemies.
- **A battlehorn no longer pushes back your next weaponskill.** Its 1 s summon now starts only where it ends before the
  half second kept free ahead of the GCD; when you are in danger a healing or protecting familiar still comes at once.
- **Soul Crush interrupts.** It goes to a caster whose cast can be interrupted first: 1,000 instead of 300, and 100 TP.
- **Beastmaster keeps the Inner Compass turning clockwise.** When an axe opens a combo for the familiar's Trick to
  close, it is now the axe that follows the standing heart, or with none the one before the Trick's affinity, so the
  Trick closes an intentional combo - "greater additional damage" - instead of a plain one in most cases.
- **Beastmaster captures familiars you do not have yet.** On the last weaponskill before such an enemy falls, Capture
  marks it at the least HP it will have alive; the lower its HP when marked, the likelier the pact. It recognises the
  beast from the game data and asks the game's familiar list, so a familiar you have is never tried again. New option
  "Capture familiars you do not have yet", on by default; it costs one weaponskill per attempt.
- **Beastmaster no longer sends a familiar away with nobody to follow.** In combat a horn's 90 s start only when its
  familiar retreats, so the horn of the familiar out is never its replacement. Parting Blow, and a Tempered Release
  after which the familiar leaves (Wespe's Final Sting), now go only when another horn's familiar is ready; until then
  the familiar keeps fighting. "Only use Snarl and Challenge on Last Horn" reads the same: no other horn ready.
- **Beastmaster counts a pack stopped as protection, and an element-only shield no longer.** In a danger, a familiar
  whose Tempered Release stops the enemies it reaches - Stun (War Cry), Petrification (Petribreath), Deep Freeze (The
  Ram's Voice) - is called like one that protects, when two or more enemies attack you, all within its reach, none a
  boss and, for the stun, none stunned since its last immunity. Strut and Formic Pheromones, which lower only earth or
  water, earth and ice damage, no longer count as protection. A Release that deals damage over time to the party
  (Hypodermic Hustle) waits while you are in danger.
- **Summoner keeps summoning while a follow-up is held back.** Upstream's new "Do not summon a new primal while attuned
  or holding Ifrit's/Garuda's Favor" is removed here, as is its "Dynamic primal order": the follow-ups already go first
  whenever they can, so the block only waited where movement or the Crimson Cyclone distance held one back - with that
  distance kept, an unused Ifrit's Favor held every later primal until the next demi-summon. "Prefer Titan while
  moving" and "Summon Ifrit first while Crimson Cyclone can follow" cover the order.
- **Summoner summons Ifrit first while Crimson Cyclone can follow.** Ifrit's Favor ends with the next summon, and as
  the last primal Ifrit lost Crimson Cyclone and Crimson Strike to the demi-summon in most rounds. New option "Summon
  Ifrit first while Crimson Cyclone can follow" (on by default): while you stand within your Crimson Cyclone distance,
  Ifrit comes ahead of the configured order and both go out at once; Garuda then comes last. Not under Searing Light,
  not while moving unless "Use Crimson Cyclone when moving" is on, and not with "Use Crimson Cyclone at any range" on.
  The Cyclone never goes beyond your distance setting.
- **Blue Mage: Moon Flute only when it pays and is safe.** Moon Flute no longer goes again every time Waning Nocturne
  ends. It goes when the damage it adds to what falls into Waxing Nocturne - the cooldown spells ready by then, one per
  shared recast, read from their tooltips - is more than Waning costs; just before a fight ends it costs only its own
  GCD. The burst abilities wait for it, Whistle and Tingle go before it when Triple Trident falls into the window, and
  the wait ends once it would cost more than the window brings (for instance while you keep moving). New option "Moon
  Flute only when it is safe…" (on by default): no Moon Flute under tank or healer mimicry, while a party member is in
  danger, while your own health would fall too low over the window, or while BossModReborn announces a raidwide or a
  tankbuster on you within it. Diamondback at the end of Waxing now goes only when your health would otherwise fall
  that low during Waning; before, it ended every window early and rooted you.
- **Blue Mage: fixed and percentage damage by value.** RSR measures the damage one point of potency deals on your own
  hits. 1000 Needles, Launcher and Dimensional Shift go only where they deal more than Sonic Boom in the same time;
  none of the fixed or lethal spells goes against an enemy of higher level, and an enemy kind they have been seen to do
  nothing to is not tried again in the session. Missile now needs "Use Low Chance abilities", as its tooltip names a
  low chance; Launcher, whose tooltip does not, no longer does.
- **Blue Mage: fillers, enhancers and Knight's Tours.** Sonic Boom is the filler; another spell goes before it only
  with more potency per second over the enemies it hits, and on one target without a longer cast. Perpetual Ray is no
  filler. Whistle no longer goes before Song of Torment, where it was lost; Whistle and Tingle now go before Triple
  Trident (Tingle never went before). The Knight's Tours now read Bind instead of Blind, and the cycle can begin: it
  starts against an enemy kind not seen to resist Bind or Slow. Tatami-gaeshi now counts as a stun.
- **Blue Mage: spell chains.** Conviction Marcato now follows the fourth Winged Reprobation while Winged Redemption
  lasts (440 instead of 220; it was never cast). Bristle goes before Matra Magic under DPS mimicry. Peripheral Synthesis
  goes only on a target without Lightheaded on which it last landed, and Mustard Bomb follows on the Lightheaded
  target; before, Peripheral Synthesis went ahead of every filler. The Ram's Voice goes for Ultravibration only where
  Ultravibration can kill the target. Where you already stand within Sharpened Knife's reach, Faze, Tatami-gaeshi or
  Bomb Toss stun first. Level 5 Petrify joins the low-chance spells, and Level 5 Death and Petrify skip enemies whose
  level is not a multiple of five. Saintly Beam learns undead enemy kinds from its own hits and then goes against them
  on one target too.
- **Blue Mage: the active set improves itself from your learned spells.** New option "Improve the Blue Mage set from
  your learned spells" (on by default). Out of combat, without a target and while RSR is on, it adds Sonic Boom where it
  is missing, the partners of spell chains one part of which is in your set (Whistle and Tingle with Triple Trident,
  Conviction Marcato with Winged Reprobation, Ultravibration with the Ram's Voice, the other Knight's Tour, Mustard Bomb
  with Peripheral Synthesis, Bristle with Matra Magic or Song of Torment), and damage spells with a recast from recast
  groups your set lacks. It uses only free slots and plain fillers RSR does not cast at your level; damage cooldowns,
  heals, defence, movement, mimicry, raises, interrupts and every spell with an additional effect stay, and a slot changes
  only while its spell is ready. One slot at a time; each change is written to `DefenseTrace.log`. Once you change
  your set yourself, and inside the Masked Carnivale, RSR leaves it alone for the session. Saved sets and hotbars are
  not touched.
- **A tincture in every burst window of a hard fight where it is ready and in your inventory.** A tincture is for
  coming through a hard fight and ending it sooner, and it costs money. Only "Tincture usage" decides where (never,
  high-end duties - the default -, anywhere); high-end is what the game marks so: the current Savage tier, the current
  Extreme and Unreal trial, the Chaotic raid, the Ultimates. Older Savage and Extreme fights are not synced to their item
  level and get none, nor does a training dummy any more under "high-end duties". Gunbreaker, Dark Knight, Bard and Machinist drank one only in a countdown - with no
  countdown, and in every later burst such as the sixth minute, never; they now drink it under No Mercy, in the
  two-minute window, under the Bard's buffs and under Wildfire. Viper and Machinist had a switch of their own, off by
  default, and never drank one. The rotations' own tincture switches are removed: Summoner's "Use potion", "Potion
  timing" and "Only use potion during a big summon", White Mage's "Use Tincture/Gemdraught when about to use Presence
  of Mind", Viper's "Experimental Pot Usage", Machinist's "Use burst medicine in countdown".
- **Tinctures and potions share one recast.** "Tincture usage", "Use HP potions" and "Use MP potions" now say so: a
  tincture blocks HP and MP potions for 5 minutes, an HP potion the tincture for its own recast.
- **Potions can wait for the tincture.** New option "Hold HP and MP potions for the tincture" (per job, off by
  default): on, HP and MP potions wait where your rotation drinks tinctures, "Tincture usage" allows one and one is in
  your inventory, so the tincture always comes in its burst; an HP potion against a tankbuster on you still goes. Off,
  a potion goes at its threshold as before, even if the tincture then misses its burst. Potions of Eureka and the deep
  dungeons have a recast of their own and never wait.

## Seeing what the rules decide

- **Diagnostics window** — Settings → UI → Windows → **Show Diagnostics Window** (off by default):
  the rotation's status, the damage table's store state and last rated hit, the last area heal
  weighed, the last movement action withheld, why each HP potion is or is not used, and per hold
  whether it last held or gave way. Its close button, Escape and the gamepad back button turn it off;
  Escape and the back button turn the Control window off the same way.
- **`DefenseTrace.log`** in the plugin's config folder records every action the defensive chain chose,
  with every source standing at that moment and the game's status for that action, and every enemy hit
  on you. It keeps every session; each starts with its date and time and the commit the plugin was built
  from. Delete the file to start over. A chosen defence is followed by a `used` line when it actually
  goes out. It names the pull rule and unlisted casts at a tank's target as sources, marks an
  untargetable caster "(untargetable)", says whether the single hit reaches you, records each landing of
  a listed area cast with whether it reached you, and writes "open, nothing chosen" with the sources
  standing when a defence is open but nothing it asks for can go out. For a BossModReborn raidwide it
  names the module, the source and whether the raidwide hits you; for a tankbuster the measurement, the
  plan and every press, and for a tank swap why it held and whether the boss moved. Each decision and
  each "open, nothing chosen" ends with the actions the defence refused and the game's reason, and the
  holds standing; each HP potion is written with the branch that allowed it. After each fight a line
  gives RSR's update cost per frame and a second one splits it by step, with what the slowest cycle was
  made of and whether a garbage collection ran during it, with the size of the party in the fight. It
  names whom a chosen defence goes to, writes a line when a small area cast is let through without party
  mitigation, notes for each Dark Knight barrier whether it broke, names each Liturgy of the Bell laid into
  a series, writes one line per damage area laid, and counts the party members a listed area cast
  reached.
- **The right-click entry "Reset to Default Value." restores the default value**. Plain and per-job
  switches used to end up off instead, `Heal ahead of incoming damage` and `Raise with Swiftcast or
  Dualcast` among them.
- **Two settings that did nothing were removed**: `How early before next GCD should RSR use swiftcast for
  raise` and `Target color`; nothing had read them since upstream dropped their code.
- **The action window shows the movement check that decides in combat**, including that a dash onto a target at 0
  yalms is only damage; it asks the same check as the press.
- **A chosen sleep that never went out says why** in the trace's "sleep not cast" line: moving, or a party member in
  danger, which both come first.
- **Blue Mage: the active set in the trace.** `DefenseTrace.log` now lists the Blue Mage's 24 active spells whenever
  they change, and every own Blue Mage spell that lands although it is not in that set (a follow-up a set spell turns
  into, such as White Death, does not count).
- **Blue Mage: the Masked Carnivale briefing in the trace.** With the briefing open, `DefenseTrace.log` lists for each
  stage you select the enemy ids of every act and the name the briefing sheet gives each.
- **Beastmaster: a record of the Master's Bestiary.** While the Bestiary is open, RSR writes to `BestiaryTrace.log`
  next to `DefenseTrace.log` what the game receives from each click, every context menu with its entries, every server
  command the client sends through its general command function, and the battlehorn slots as they change. It only
  listens and passes every command on unchanged; it opens and closes with the Bestiary. `DefenseTrace.log` also
  gets a "beastmaster state" line whenever what the horn choice and Capture read changes: the familiars on the horns,
  the one out, both options, whether Capture is enabled, and whether the familiar list and catalog are loaded.

## Frame time

RSR keeps only the visual effects it reads — the stack, spread and tankbuster markers and the lock-on
markers — instead of every effect of every character, checks whether a status is your own without searching the object table per
status, and works out each enemy's time-to-kill once per frame. Most of the difference shows in
alliance content, where 24 or 48 players' effects used to be walked several times a frame.

**No crash from RSR's windows at a loading screen.** When RSR rebuilt its window fonts after a zone change - at
character select or a teleport - a font still being drawn could already be released, and the game crashed in the
control window. RSR uses RebornMaterial 7.5.7.5, which keeps each font until the frame is drawn.

## For rotation authors

- `GetCurrentMitigationPercent` counts Troubadour, Tactician and Shield Samba at 15 % and includes
  Confession; `IsPhysicalDamageIncoming` tests the physical attack types.
- `ObjectHelper.GetForecastHealthRatio` is public, with an `instant` overload for off-GCD heals;
  `ActionSetting.HealsWithNextGcd` marks an off-GCD action whose healing lands with the next GCD.
- `SpecialActionType.HostileAttackBackstep` with `ActionSetting.BackstepDistance`.
- `StunWatch` (`StunLeft`, `CanStillBeStunned`, `Applications`) reads from the fight which enemies a
  stun landed on and which can still be stunned; recognition only, no option and no threshold.
- `DataCenter.AreaHitAnnounced` answers whether area damage on the party is announced from any source,
  without rating it; `DataCenter.SingleHitReachesPlayer` whether the single hit reaches the player;
  `DataCenter.AreaCasters` holds the hostile list plus the untargetable enemies casting a listed area
  action. `DataCenter.DefaultGCDTotal` keeps the last GCD length between GCDs instead of 0, and
  before the first GCD it is the adjusted recast of a weaponskill.
- `ActionTargetInfo.FindTarget` orders the candidates of a damage-over-time action itself (most HP
  first, then by expiry; `Configs.DotTargetByHpThenExpiry`); `TargetFilter.GetDeath` leaves out the dead
  someone else is raising; the area heal flags rise ahead of an announced hit only when it endangers
  someone; `CustomRotation.TankbusterOnMe` asks whom BossModReborn's tankbuster hits;
  `DefensiveValues` reads four more mitigation phrasings and the trait-set reductions, and rates a status
  by its own text. RotationSolver.Basic references ECommons 3.2.1.26 (see `CHANGELOG.md`).
- `TargetType.Tankbuster` and `TargetType.BeAttacked` follow whom a tankbuster is aimed at;
  `CustomRotation.HelpTankbusterTarget` puts an action on another member a tankbuster is aimed at.
  `Configs.SleepToStopAreaCasts` and `SleepInterruptWatch` carry the sleep on an enemy's area cast.
  `DataCenter.AnyLivingRaiser` counts the alliance under `RaiseType.AllOutOfDuty`. The members upstream
  7.5.7.5 removed are listed in `CHANGELOG.md` under 7.5.7.6+wsh1.
- `CanUse` refuses a defence aimed at the player with no effect radius while the hit that opened the
  defence does not reach him (`ProtectsOnlyYouAndTheHitMissesYou`), and a rated defence aimed at him
  while the tankbuster plan holds them (`HeldForTheInvulnerability`) — in a derived rotation as in the
  shipped ones.
- 7.5.7.8+wsh1 (details in `CHANGELOG.md`): `DataCenter.AnyLivingHealer` and `UnhealedUnderAttack`;
  `CustomRotation.GcdHealsAllowed`; `WhiteMageRotation.LiturgyTriggersLeft` also counts a learned series
  whose first hit an enemy cast announces; an area share of 0 in
  `OtherConfiguration.HostileCastingAreaPotential` is a landing measured without damage, no entry means
  unrated; `TargetType.Tankbuster` and `TargetType.BeAttacked` prefer the least protected of several
  targets; `SleepInterruptWatch` counts a stop only when no effect follows; `Configs.SwiftcastBuffer` and
  `Configs.TargetColor` are removed.
- 7.5.7.10+wsh1 (details in `CHANGELOG.md`): `CustomRotation.HotAgainstDotByJob` and `TryHotAgainstDot`,
  `TargetType.HotAgainstDot` and `Configs.HotAgainstDot`; `ActionConfig.ShouldCheckTargetStatus` is back;
  `UseBurstMedicine` refuses out of combat without a countdown; `SleepInterruptWatch` stops every cast known
  to be stoppable; `PhoenixDownItem` waits for a cast it can finish; `TankbusterHelp` for a measured
  tankbuster by its size; `TryChargedBarrierAtCap`, `FlamethrowerFits`, `BardRotation.SongDuration`,
  `IsTimeToKillMeasured`, `MonkRotation.PerfectBalanceMaxStacks` and `RiddleOfFireDueWithinPerfectBalance`,
  `CustomRotation.LastWeaponskillOnTarget`; `ICooldown.JustUsedAfter` is false for an action not used;
  `Configs.Migrate` keeps stored values across a version change; RebornMaterial 7.5.7.2.
- 7.5.7.13+wsh1 (details in `CHANGELOG.md`): `RotationSolver.Basic` is folded into `RotationSolver` (upstream 7.5.7.11),
  `latest.zip` carries no `RotationSolver.Basic.dll`; no cast meets a learned status that damages every action
  (`NoCastingStatus`, `ActionPenaltyCasts.json`); the single-target stretch asks the target the defense has chosen; area
  hits are learned unmitigated (`AreaPotentialUnmitigated.json`) and read under the mitigation standing; every running
  heal over time lowers the heal thresholds; the corrected time to kill is kept for enemies
  (`CustomRotation.IsTimeToKillMeasured`, `LastWeaponskillOnTarget`), with `CustomRotation.HoldCostsUse`; damage keeps the
  raise's MP (`Configs.KeepMpForRaise`); `CustomRotation.HitSeriesHitsLeft` and `MultiHitAhead` for every healer.

**Defects of the original that were costing something in the fight:**

| Site | What happened in the fight |
|---|---|
| Black mage, Thunder | From level sync 92 a fresh area DoT was cut short on every cast |
| Red mage, Impact | Impact never fired |
| Reaper and Viper | Leg Sweep and Arm's Length were given up to the combo gate, and the role default then took them ungated anyway |
| Nine dispatch overrides | Defensive and healing calls silently continued in the wrong chain |
| Restricted DoT guard | Targets excluded from DoTs got them anyway |
| Movement abilities | Double call, in the wrong order against the duty rotation |
- **latest.zip holds the plugin and nothing else.** The release package of 7.5.7.13+wsh1 also carried the build's source
  generator (5.7 MB, never loaded), and a local build packed whatever earlier builds had left in its output folder, such
  as the old RotationSolver.Basic.dll. The package is now made from a fixed list - the plugin, its manifest, ECommons
  and RebornMaterial -, and a release is not published unless its zip holds exactly that.
