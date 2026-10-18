# What this fork does differently in a fight

The whole distance from upstream **7.5.7.3**, as of release `7.5.7.3+wsh1`. What a single
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
  marker in another alliance group no longer opens your mitigations.
- **Skip area defence for casts that missed you** — **on by default**. An area cast that landed last
  time without reaching you does not open your area defence the next time; once it reaches you
  again, it counts again. Blocked, parried, absorbed or taken under invulnerability counts as
  reaching you. Healing ahead and the party's threat check are unaffected. Some listed casts land
  only on their caster and hurt nobody, and the hit comes under the same name right after, often from
  an invisible helper: a same-named hit on the party after such an empty landing counts as that
  landing, both for whether the cast reached you and for how hard it hits. A wind-up that nothing
  follows stays skipped.

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
  second and corrected.
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
- **HP potions answer to their own settings**: the global setting, the per-item enable, its own HP
  percentage, the missing-health guard, having one in the bag, and being in combat — not the
  healing-action flag, which a damage dealer with a living healer in the party never gets. A
  tankbuster that reaches you lowers the percentage: one cast at you, or a predicted one whose targets
  name you — when BossModReborn names nobody, for a tank, or when the party has no tank alive. Of two potions the one that restores more
  goes first, and at equal healing the lower grade, read for the form that will be drunk (HQ or NQ).

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
- **"Prioritize Low HP … Big Target" takes effect**; among equally big targets the setting for
  small targets was read.

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
- **Phoenix Down** is wired up and checks target eligibility. **Off by default.**
- **The healer-only hard-cast modes** hold the Swiftcast reservation their text promises:
  with Swiftcast ready, nothing is hard-cast any more.
- Swiftcast is not spent for damage in the Summoner rotation; it stays reserved for raises —
  `Use Crimson Cyclone at any range…` and Swiftcast on Ruby Ruin and Ruby Outburst below Ruby
  Rite's level are off by default.

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
  Troubadour, Shield Samba, Tempera or Addle; switched on, it holds without exception.
- **Tankbuster markers**: every tank lock-on marker and shared tank laser is recognised, plus two
  more party-stack markers. A marker after which no enemy action reaches the marked member goes on
  `TankbusterMarkerWithoutHit.json` and is no longer treated as a tankbuster; a later hit takes it
  off again.
-  **Channel locks hold actions only until the announced hit lands.**
  `Lock actions when casting Passage Of Arms during AOE mitigations` is **on by default**: Passage
  of Arms ends with the paladin's next action, and RSR casts it only for an announced area hit, so
  without the lock the next GCD or weave often ended the channel before the hit.
  `Lock actions when casting Collective Unconscious during AOE mitigations` stays off by default.
- **Paladin: Intervention on the other tank works** — it accepts the tank stance from any source.
- **The BossModReborn timeline is only read when it is switched on**, and a defence pressed for a hit
  it announces goes out only while a press made now still reaches the server before the end of the
  cast, from your measured round trip — no fixed margin.
- **Each ability picks its own target.** A target rule set by the GCD no longer carries over to the
  commanded action, the emergency abilities, the tankbuster plan and the tank swap: the Paladin's
  emergency Intervention goes to the co-tank, not to whoever has the least HP.

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
  health threshold. On a pull it waits while a White Mage's Holy keeps the pack stunned — one GCD
  after the stun ends, longer only while a Holy is visibly being cast into the pack. The white mage
  can hold Holy while a tank carries the barrier (`HoldHolyForBlackestNight`). It and Oblation on a
  low party member are not held in the burst window, and they pick their target among the members under
  their health threshold.
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
  "Number of hostiles" for defensive abilities, counted on ordinary enemies only — bosses do not count;
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
  `Auto provoke when there is another tank in party` does only what its text names, and `Provoke
  anything not on the no provoke list.` respects the list.

## Movement safety

With `Use BMR intergration to verify safety of movement actions…` (off by default):
- **A gap closer is not withheld when there is nothing to close** — at 0 yalms between the
  hitboxes, the distance the game shows. The check measured the line to the boss's centre, so an
  area under a large boss refused the dash, and on Summoner Crimson Strike with it.
- **Leaps count as movement**: Dragonfire Dive, Stardiver and Forked Raiju.
- **Backsteps and fixed dashes are checked**: Hell's Ingress and Egress, Hissatsu: Yaten,
  Displacement and Repelling Shot go out when their landing point, computed from the distance in the
  effect text, lies outside a danger zone. Repelling Shot jumps away from the enemy.

`No fall damage` under Extra (off by default): the game's fall damage function returns zero, so a
fall from a height costs no HP. The hook and its signature come from Henchman's hacks (AGPL-3.0,
credited in the file). If the game version does not have that function, the setting switches itself
off.

## The learned lists

- **The damage table measures at all.** The effect handler read the action type four bytes wide,
  so no area action was ever rated. Measuring no longer depends on `Record AOE actions` or on the
  party size; that setting decides only whether new actions are added. The actions of invisible
  helpers are added and measured like any other.
- **Tankbusters are learned, from every tank they hit.** Every hit of a listed tankbuster, or of
  one whose marker was confirmed, counts on any tank — you, your co-tank, or tanks of other parties in
  large content — scaled back to unmitigated by the mitigation that stood on the target and on the boss
  (each status by its own text, Intervention, Taurochole, Third Eye and the 15 % of Troubadour,
  Tactician and Shield Samba from level 98 included), and stored per action as a share of maximum HP;
  a hit taken under a vulnerability debuff is kept apart, and hits from an enemy carrying a Damage Up
  status are not stored. A hit on a player who is not a tank lacks the tank trait and is not stored. The tank swap and the tankbuster
  plan read this table, so a tankbuster from an earlier evening or from your co-tank is known. The list
  shows each entry's figure and the store's state.
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
- **A failed download no longer leaves an empty list for good**; the next start tries again, and an
  unreadable list file is set aside and downloaded afresh.

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
  single-target attacks keep the targeting type you chose.
- **Granted windows no longer run out unused.** An action that needs a status of yours is usable as
  long as the status lasts until it goes off, not locked out for the last two GCDs. Ogi Namikiri,
  Sonic Break, Reign of Beasts, Phantom Kamaitachi and Hypercharged are spent before their Ready runs
  out; Retrace only while Ley Lines outlasts the next GCD.
- **Machinist:** with `Only use Wildfire on Boss targets`, Heat is not held for Wildfire on trash;
  Wildfire cannot go out as Detonator; Queen Overdrive lands before a predicted untargetable phase.
- **Samurai:** Meditate when no enemy is within 25 yalms in combat.
- **Reaper:** Hell's Ingress and Egress cannot go out as Regress. Soulsow also goes out in combat
  while no enemy is within 25 yalms, so every Harvest Moon after the first is there; with a
  BossModReborn module running and `Use BossModReborn timeline for proactive mitigation` on, it waits
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
  made of and whether a garbage collection ran during it.

## Frame time

RSR keeps only the visual effects it reads — the stack, spread and tankbuster markers — instead of every
effect of every character, checks whether a status is your own without searching the object table per
status, and works out each enemy's time-to-kill once per frame. Most of the difference shows in
alliance content, where 24 or 48 players' effects used to be walked several times a frame.

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
- `CanUse` refuses a defence aimed at the player with no effect radius while the hit that opened the
  defence does not reach him (`ProtectsOnlyYouAndTheHitMissesYou`), and a rated defence aimed at him
  while the tankbuster plan holds them (`HeldForTheInvulnerability`) — in a derived rotation as in the
  shipped ones.

**Defects of the original that were costing something in the fight:**

| Site | What happened in the fight |
|---|---|
| Black mage, Thunder | From level sync 92 a fresh area DoT was cut short on every cast |
| Red mage, Impact | Impact never fired |
| Reaper and Viper | Leg Sweep and Arm's Length were given up to the combo gate, and the role default then took them ungated anyway |
| Nine dispatch overrides | Defensive and healing calls silently continued in the wrong chain |
| Restricted DoT guard | Targets excluded from DoTs got them anyway |
| Movement abilities | Double call, in the wrong order against the duty rotation |
