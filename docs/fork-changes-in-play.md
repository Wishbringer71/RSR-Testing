# What this fork does differently in a fight

The whole distance from upstream **7.5.6.10**, as of release `7.5.6.10+wsh1`. What a single
release changed is in [`fork-changes-since-last-release.md`](fork-changes-since-last-release.md),
which is the text of the release description; this document is the standing picture and takes
each release's text over once it has shipped.

Settings are named as they appear in the configuration, and where a change sits behind a switch
its default is given; without that note it takes effect immediately. Two changes are confirmed
in play — the raise dispatch and the tank pre-pull HoT; everything else is established in the
code and compiled, not yet measured at a dummy.

## Party mitigation answers raidwides again

`Skip mitigation for small area casts` spares the cooldown when an area hit is too small to
matter. It asked one question only — would this hit push anyone to where healing is called for —
and a healthy party answered no to almost every raidwide, so Addle and Radiant Aegis stopped going
out on Summoner. Mitigation is meant to throttle the damage *before* a need to heal appears.

An area action costing **25 % of maximum health or more** is now a big hit whatever the party's
health. The figure is read from the effect texts — it is what the largest barrier in the game
absorbs — not set by hand. Below it the buffer comparison still decides, so small repeated ticks
keep costing no cooldown while the party is healthy.

- **An interruptible cast can be mitigated too** — `Mitigate a big area cast even when it is
  interruptible`, **off by default**. Area questions drop interruptible casts, because those are
  meant to be interrupted. In a dungeon where nobody does — and a Summoner has no interrupt — the
  hit lands unanswered. A cast measured at the large-barrier figure now raises the defence anyway,
  within a GCD of landing.
- **A predicted mitigation is not spent on the small hit before the big one** — `Hold a predicted
  mitigation while a small cast is running`, **off by default**. BossMod gives the timing of the
  next event, not its size, so in an opening with a small cast ahead of a heavy one the barrier is
  eaten by the first. It now waits while a measured small cast runs.
- **Healing ahead of an announced area cast** — `Heal ahead of an announced area cast`, **off by
  default**. Thresholds read the health a member has, not what he will have once the cast on screen
  lands: 60 % in front of a 45 % raidwide counts as healthy until it kills him. An existing barrier
  counts against the hit it absorbs.

## Healing — when it lands

- **A pull no longer starts without healing.** For roughly the first 2.5 seconds of every
  pull there was no automatic healing at all, because the time-to-kill estimate read as "the
  fight is about to end" while it was still empty.
- **The white mage heals again while a pack is dying.** At the end of every pack all heal
  flags went out: tank below 20 %, no heal attempted, Holy instead of Cure.
- **Healing ahead of the hit instead of after it** — `Heal ahead of incoming damage`, **off
  by default**. Every healing threshold and the heal target choice read the health a member
  is heading for by the time a heal started now would land. A tank dropping fast gets his
  heal about one GCD earlier, and someone falling quickly is picked ahead of someone sitting
  lower but steady. The rate is net of mitigation, barrier and healing, and each prediction
  is held against the actual course every second and corrected.
- **The emergency full heal waits for a reason** — `Benediction needs a reason`, **on by
  default**. Benediction now also requires that the target is being attacked, that an area
  cast is announced, or that their health is measurably falling. Before that, a player who
  had just been raised read as the most urgent member in the party — and the full heal was
  gone for ninety seconds.
- **The tank walks into the pull with a HoT already ticking** — `UsePreRegen` (White Mage),
  `UsePreAspectedBenefic` (Astrologian), each with two enemy-count thresholds. The first hits
  land on a tank who already has something running. Both actions are instant, so nothing is
  lost while moving. **Confirmed in play.**

## Healing — who it picks

- **Whoever is about to die comes before the role short-cuts.** A damage dealer at 10 % was
  passed over as soon as the tank sat at 44 %; a critical rank now ranks ahead of the fixed
  role thresholds.
- **A party member behind the camera can be healed again** — the "only targets in view"
  filter applied to heal targets as well.
- **A target under invulnerability** gets healing at a lowered threshold and the normal one
  shortly before the status ends, instead of none at all.

## Shields and barriers

- **The shield list did not know the most common barriers** — fifteen were missing, among
  them Divine Benison and The Blackest Night. Wherever the tree asked whether anything was
  still up on a target, the answer was no for most tank shields.
- **A shield is no longer credited against the healing threshold.** A shield prevents damage,
  it does not restore health: a tank at 40 % stands at 40 % whether a barrier is running or
  not, and healing withheld for a barrier that expires unspent is thrown away.

## Raising

- **It happens at once instead of after a long delay.** Swiftcast was only spent on a raise
  once the raise was already reported as the next GCD — which it could not be without
  Swiftcast. It now also falls when a raise is pending and would be castable. **Confirmed in
  play.**
- **Phoenix Down** is wired up and checks target eligibility. **Off by default.**
- **The healer-only hard-cast modes** hold the Swiftcast reservation their text promises:
  with Swiftcast ready, nothing is hard-cast any more.
- Swiftcast is not spent for damage in the Summoner rotation; it stays reserved for raises.

## Mitigation and defensives

- **Two tanks no longer stack Reprisal**, and the second one keeps the 60 seconds of cooldown
  it used to throw away. The same applied to Addle and Feint with two casters or two melees.
- **Paladin and Warrior now give Reprisal on raidwides too** — it sat only in the
  single-target path, although the Warrior's own description promised it for area damage.
- **The mitigation debuffs are sustained rather than applied once**, with the duration
  following the level 98 trait.
- **A buster aimed at a damage dealer gets an answer.** Twelve jobs use their existing
  reactive line — Feint, Addle, Troubadour, Tactician, Shield Samba, Third Eye — when a cast
  is actually aimed at them. Single-target defence for damage dealers was not staffed at all.
- **Self-healing for damage dealers** (Second Wind, Bloodbath) across ten jobs, where the
  role actions were declared and never used.
- **The BossModReborn timeline is only read when it is switched on.**
- **Area actions are measured by how hard they hit**, not only recognised as such; the list
  window shows the share per action, and which rules acted on it. Those readings now survive a
  logout — until this build they were never read back, so every login started unrated and the
  first save overwrote the stored ones.

## Tank self-protection

- **The Blackest Night** repays its 3000 MP only when the barrier is fully absorbed, and the
  original trigger is far too weak for that. `BlackestNightUsage` offers narrower readings
  with a minimum enemy count and an emergency share. **The default is the old behaviour.**
  The other side of it: the white mage can hold Holy while a tank carries the barrier
  (`HoldHolyForBlackestNight`), because the stun stops exactly the hits that would spend it.
- **Living Dead and Walking Dead.** While more than two GCDs of Living Dead remain, a lowered
  healing threshold applies so the death effect can occur; within two GCDs the normal
  threshold returns, so healing resumes shortly before the status expires. The lead-in is
  suspended while zero would arrive before the window closes.
  `WithholdHealingForLivingDead` sharpens the first part and is **off by default**.
- **Arm's Length on a group pull** (`UseArmsLengthOnPull`) for its Slow, not only as
  knockback protection: +20 % recast on every enemy that strikes you, for fifteen seconds.
- **The co-tank Provoke** no longer pulls the boss off a tank standing under Superbolide,
  Living Dead or Holmgang.

## Damage and rotation

- **Summoner: Searing Light covers the phase from its first GCD.** It was offered only once the
  demi was standing, so the earliest slot it could take was the one *after* the summon, and a busy
  slot then pushed it into the middle of the burst. It now fires in the slot before, and the summon
  waits for it; 20 seconds of buff cover a 15-second demi either way. The window itself is the burst
  phase; with a second Summoner in the party it widens to any big summon, and once every phase is
  taken it falls back to Titan — or to Ifrit when you are standing at the target anyway, since its
  higher figure assumes a gap closer you then do not need. `PreferTitanWhileMoving` (**off by
  default**) brings Titan forward while you are moving, never skips it.
- **Summoner: the phase heal goes out when it lands in full.** Lux Solaris hung on the area heal
  flag, which wants the party's health spread to be *small* — so one player taking a mechanic kept
  it down exactly when somebody was hurt. It costs no MP and no GCD and expires with Refulgent Lux,
  so the question is whether the cast is wasted, not whether area healing is worth it. It now fires
  once the missing health can absorb the whole heal — measured from what the heal actually restored,
  not from its potency — and in any case before the buff expires.
- **Summoner: Rekindle picks by share, not by points.** It sorted by current health *points*, and
  pools differ enough that a caster at full health can hold fewer than a tank at half — so the heal
  went to someone who needed nothing. The action works in shares itself — its follow-up arms at
  75 %. With no target it goes on the caster instead of being lost with the phase.
- **White mage, Holy.** Three separately switchable rules: do not overwrite the stun while it
  is still running (`StretchHolyStun`, **off by default**); hold Holy while the dark knight's
  barrier is meant to be filled; and hold Holy while more than half the enemies in radius are
  slowed, since the stun is worth more once the Slow has run out.
- **Thin Air** is only spent on an expensive spell under real MP pressure that Lucid Dreaming
  cannot answer (`ThinAirOnMpPressureOnly`). A raise still takes a charge regardless.

**Defects of the original that were costing something in the fight:**

| Site | What happened in the fight |
|---|---|
| Black mage, Thunder | From level sync 92 a fresh area DoT was cut short on every cast |
| Red mage, Impact | Impact never fired |
| Reaper and Viper | Leg Sweep and Arm's Length were given up to the combo gate, and the role default then took them ungated anyway |
| Nine dispatch overrides | Defensive and healing calls silently continued in the wrong chain |
| Restricted DoT guard | Targets excluded from DoTs got them anyway |
| Movement abilities | Double call, in the wrong order against the duty rotation |
