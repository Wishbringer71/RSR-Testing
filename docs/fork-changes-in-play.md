# What this fork does differently in a fight

The whole distance from upstream **7.5.6.13**, as of release `7.5.6.13+wsh1`. What a single
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
cooldown while the party is healthy. Hits and heals above 65,535 points are read in full; before,
the low 16 bits only, and a raidwide that hit a tank harder than that could be rated small.

- **An interruptible cast can be mitigated too** — `Mitigate a big area cast even when it is
  interruptible`, **on by default**. Area questions drop interruptible casts, because those are
  meant to be interrupted. In a dungeon where nobody does — and a Summoner has no interrupt — the
  hit lands unanswered. A cast measured at the large-barrier figure raises the defence anyway,
  within a GCD of landing.
- **A predicted mitigation is not spent on the small hit before the big one** — `Hold a predicted
  mitigation while a small cast is running`, **on by default**. BossMod gives the timing of the
  next event, not its size, so in an opening with a small cast ahead of a heavy one the barrier is
  eaten by the first. It waits while a measured small cast runs.
- **Area defence asks whether the party is hit; self-shields ask whether you are.** Whom a cast
  from the AoE list reaches is read from the game data's shape: a single-target action with a cast
  range reaches only its target, a circle cast at a player is measured around that player, a line
  only reaches what lies inside its width, and a circle around the caster is measured from the
  caster's centre, as BossModReborn draws it (a large boss's hitbox used to be added, so a 10-yalm
  point-blank reached players 18 yalms away). The area defence opens party mitigations when the hit
  reaches you or at least two party members. Radiant Aegis, Samurai's Tengentsu and Third Eye, and a
  damage dealer's single-target defence under the area flag ask whether it reaches you, so a
  tankbuster circle on the tank no longer spends a Summoner's Radiant Aegis. Tempera Coat is
  unchanged, being what Tempera Grassa needs first. Ground-targeted circles, cones and charges are
  still measured from the caster: the game data does not state where they land or how wide they open.
- **Markers count only on you or your own party** (Duty Support companions included); a stack
  marker in another alliance group no longer opens your mitigations.
- **Skip area defence for casts that missed you** — **on by default**. An area cast that landed last
  time without reaching you does not open your area defence the next time; once it reaches you
  again, it counts again. Blocked, parried, absorbed or taken under invulnerability counts as
  reaching you. Healing ahead and the party's threat check are unaffected.

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
- **Healing ahead of an announced area cast** — `Heal ahead of an announced area cast`, **on by
  default**. 60 % in front of a 45 % raidwide is not healthy. An existing barrier counts against
  the hit it absorbs.
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
- **HP potions answer to their own settings**: the global setting, the per-item enable, its own HP
  percentage, the missing-health guard, having one in the bag, and being in combat — not the
  healing-action flag, which a damage dealer with a living healer in the party never gets. A
  confirmed or predicted tankbuster lowers the percentage. Of two potions the one that restores more
  goes first, and at equal healing the lower grade, read for the form that will be drunk (HQ or NQ).

## Healing — who it picks

- **Choose the heal target by danger** — **on by default**. Below anyone about to fall, a healer
  or tank under their role threshold who is being attacked comes first, lowest health first and a
  healer before a tank at equal health; then everyone else — by fewest hit points while an area cast
  is announced, otherwise by lowest percentage. A damage dealer at 10 % is no longer passed over
  because the tank sits at 44 %.
- **A party member behind the camera can be healed again** — the "only targets in view"
  filter applied to heal targets as well.
- **A target under invulnerability** gets healing at a lowered threshold and the normal one
  shortly before the status ends, instead of none at all.
- **Warrior: Nascent Flash reaches the party.** Its target filter read the invulnerability check
  the wrong way round and picked only invulnerable members. It goes to members who need it, never to
  one whose healing is nullified. `Nascent Flash target priority` defaults to `Most in danger of
  dying first…`. `Keep Bloodwhetting for yourself when you need it` (**on by default**) keeps it
  while a tankbuster on you comes before the shared cooldown is back, or while you are about to die —
  unless the member is about to die and is a healer, or another tank while you are not.
- **"Prioritize Low HP … Big Target" takes effect**; among equally big targets the setting for
  small targets was read.

## Shields and barriers

- **The shield list knows the common barriers** — fifteen were missing, among them Divine Benison
  and The Blackest Night.
- **A shield is not credited against the healing threshold.** A shield prevents damage, it does
  not restore health: a tank at 40 % stands at 40 % whether a barrier is running or not.
- **Sage: a barrier on the tank through a dungeon pull** — `Keep Eukrasian Diagnosis on the tank
  through a pull`, off by default. Renewed whenever the barrier runs out or breaks; each renewal
  takes a GCD from Dosis. White Mage, Astrologian and Sage share one rule for this upkeep, and none of
  them spends the MP their raise needs on it.
- **Dancer: Improvisation ends with its barrier.** Improvised Finish was never cast, so the party
  got the regen but not the 5 % barrier.

## Raising

- **It happens at once instead of after a long delay.** Swiftcast was only spent on a raise
  once the raise was already reported as the next GCD — which it could not be without
  Swiftcast. It now also falls when a raise is pending and would be castable. **Confirmed in
  play.**
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
  the use of defense abilties during burst" is left as it reads.
- **Tankbuster markers**: every tank lock-on marker and shared tank laser is recognised, plus two
  more party-stack markers. A marker after which no enemy action reaches the marked member goes on
  `TankbusterMarkerWithoutHit.json` and is no longer treated as a tankbuster; a later hit takes it
  off again.
- **Channel locks** (`Lock actions when casting Passage Of Arms/Collective Unconscious during AOE
  mitigations`, off by default) hold weaponskills and spells only until the announced hit lands.
- **Paladin: Intervention on the other tank works** — it accepts the tank stance from any source.
- **The BossModReborn timeline is only read when it is switched on.**

## Tank self-protection

- **The Blackest Night** repays its 3000 MP only when the barrier is fully absorbed.
  `BlackestNightUsage` defaults to a tankbuster, a big pull with no other mitigation, or below the
  health threshold. On a pull it waits while a White Mage's Holy keeps the pack stunned — one GCD
  after the stun ends, longer only while a Holy is visibly being cast into the pack. The white mage
  can hold Holy while a tank carries the barrier (`HoldHolyForBlackestNight`). It and Oblation on a
  low party member are not held in the burst window.
- **Living Dead and Walking Dead.** While more than two GCDs of Living Dead remain, a lowered
  healing threshold applies so the death effect can occur; within two GCDs the normal threshold
  returns. `WithholdHealingForLivingDead` sharpens the first part and is **off by default**, since
  RSR also fires Living Dead as a last-ditch save. Under Walking Dead the dark knight heals himself
  by attacking: healers give him a HoT, and full healing starts once the timer runs out, no enemy is
  in reach, BossMod announces a downtime, a tank limit break is up, or his own course will not reach
  full health in time.
- **Arm's Length on a pull for its Slow** (`UseArmsLengthOnPull`, all four tanks, **on by
  default**): +20 % recast on every enemy that strikes you. A pull is the global "Number of
  hostiles" for defensive abilities; a slowed pack is left alone, and a BossModReborn knockback in
  the gap keeps it.
- **The co-tank Provoke** no longer pulls the boss off a tank standing under Superbolide,
  Living Dead or Holmgang.

## Movement safety

With `Use BMR intergration to verify safety of movement actions…` (off by default):
- **A gap closer is not withheld when there is nothing to close** — at 0 yalms between the
  hitboxes, the distance the game shows. The check measured the line to the boss's centre, so an
  area under a large boss refused the dash, and on Summoner Crimson Strike with it.
- **Leaps count as movement**: Dragonfire Dive, Stardiver and Forked Raiju.
- **Backsteps are checked**: Red Mage's Displacement and Samurai's Hissatsu: Yaten are not used
  when the spot they jump back to lies in a danger zone.

## The learned lists

- **The damage table measures at all.** The effect handler read the action type four bytes wide,
  so no area action was ever rated. Measuring no longer depends on `Record AOE actions` or on the
  party size; that setting decides only whether new actions are added.
- **No reading is lost on the way to disk.** Every save merges memory and file and keeps the higher
  reading; a file that cannot be read is left alone; a list whose load did not finish is not written
  on unload. The AoE, knockback and tankbuster marker lists are saved the same way.
- **The AoE list window** shows the store state under `Store:`, `Casts this session` by what decided
  them, `Last hit:` with the reason a cast was or was not rated, and the effect handler's counts.
- To start over after a patch, delete `HostileCastingAreaPotential.json` while the game is closed.
- **A failed download no longer leaves an empty list for good**; the next start tries again, and an
  unreadable list file is set aside and downloaded afresh.

## Damage and rotation

- **Summoner: Searing Light stays with the burst.** It is released as soon as the summon will be
  ready by the next GCD, and the summon waits for it when it cannot go out ahead; a buff that fell
  behind is pulled back. As the only Summoner the slot ahead opens only for the burst demi (Solar
  Bahamut; Demi-Bahamut below level 100). With other Summoners holding every burst phase it falls
  back to Titan — or Ifrit when you stand at your current target — and never waits for a buff blocked
  by theirs. `PreferTitanWhileMoving` (**on by default**) brings Titan forward while you are moving.
- **Summoner: Radiant Aegis goes out before a demi**, which replaces Carbuncle for 15 seconds; the
  big summon waits for a due shield.
- **Summoner: Lux Solaris** goes out only when it does some good inside its radius, the same way on
  every path: never under Scalebound or Shackled Healing with others near, or while a Dark Knight in
  its radius waits for Living Dead to trigger; when as many are hurt as its AoE count asks; when your
  own missing health or everyone else's takes a full heal; at once for someone in danger; and before
  Refulgent Lux runs out whenever anyone in the radius is hurt. The heal amount is the smallest full
  heal measured since the last zone change.
- **Summoner: Rekindle** picks by share, not by points, goes on the caster when nobody needs it, and
  its last-resort fallback waits for the end of the Phoenix phase, read from the job gauge.
- **Granted windows no longer run out unused.** An action that needs a status of yours is usable as
  long as the status lasts until it goes off, not locked out for the last two GCDs. Ogi Namikiri,
  Sonic Break, Reign of Beasts, Phantom Kamaitachi and Hypercharged are spent before their Ready runs
  out; Retrace only while Ley Lines outlasts the next GCD.
- **Machinist:** with `Only use Wildfire on Boss targets`, Heat is not held for Wildfire on trash;
  Wildfire cannot go out as Detonator; Queen Overdrive lands before a predicted untargetable phase.
- **Samurai:** Meditate when no enemy is within 25 yalms in combat.
- **Reaper:** Hell's Ingress and Egress cannot go out as Regress.
- **Dark Knight:** Blood is pooled for the burst — Bloodspiller and Quietus under Delirium, in the
  two-minute window or above 70 Blood.
- **Sage:** Addersgall is spent on Druochole just before it overflows.
- **Dancer:** when the dance partner dies or carries a raise's weakness, Closed Position picks a
  partner within range; a partner chosen by name is replaced only for death or weakness.
- **White mage, Holy.** `StretchHolyStun` (**on by default**) does not overwrite the stun while it
  runs; Holy is held while the dark knight's barrier is meant to be filled, and while more than half
  the enemies in radius are slowed (`HoldHolyWhilePackSlowed`).
- **Thin Air** is only spent under real MP pressure that Lucid Dreaming cannot answer
  (`ThinAirOnMpPressureOnly`, **on by default**). A raise still takes a charge regardless.

## Seeing what the rules decide

- **Diagnostics window** — Settings → UI → Windows → **Show Diagnostics Window** (off by default):
  the rotation's status, the damage table's store state and last rated hit, the last area heal
  weighed, the last movement action withheld, why each HP potion is or is not used, and per hold
  whether it last held or gave way. Its close button, Escape and the gamepad back button turn it off;
  Escape and the back button turn the Control window off the same way.
- **`DefenseTrace.log`** in the plugin's config folder records, for one session, every action the
  defensive chain chose with every source standing at that moment, and every enemy hit on you.

## For rotation authors

- `GetCurrentMitigationPercent` counts Troubadour, Tactician and Shield Samba at 15 % and includes
  Confession; `IsPhysicalDamageIncoming` tests the physical attack types.
- `ObjectHelper.GetForecastHealthRatio` is public, with an `instant` overload for off-GCD heals;
  `ActionSetting.HealsWithNextGcd` marks an off-GCD action whose healing lands with the next GCD.
- `SpecialActionType.HostileAttackBackstep` with `ActionSetting.BackstepDistance`.

**Defects of the original that were costing something in the fight:**

| Site | What happened in the fight |
|---|---|
| Black mage, Thunder | From level sync 92 a fresh area DoT was cut short on every cast |
| Red mage, Impact | Impact never fired |
| Reaper and Viper | Leg Sweep and Arm's Length were given up to the combo gate, and the role default then took them ungated anyway |
| Nine dispatch overrides | Defensive and healing calls silently continued in the wrong chain |
| Restricted DoT guard | Targets excluded from DoTs got them anyway |
| Movement abilities | Double call, in the wrong order against the duty rotation |
