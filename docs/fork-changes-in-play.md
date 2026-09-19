# What this fork does differently in a fight

The subject is the distance to `upstream/main` at **7.5.6.9** (`83033ed79`), measured at
`096282416`. It is grouped by what happens in the game — who takes how much damage and
when, which action falls earlier or later, who survives. The origin view (what was a defect
of the original, what is an extension, what was the fork's own mistake) lives in
`docs/rotation-flow/06-fork-audit.md`; that document covers the first pass and is not
carried forward.

**Size:** 533 commits. In C# sources, 74 files, +5102/−814 lines
(`git diff --shortstat upstream/main...HEAD -- '*.cs'`); documentation and check scripts
come on top of that and do nothing in the game (§ 9).

---

## On the level of verification, up front

Only two changes are **confirmed in play**: the raise dispatch (A73, the user's own
observation that raising now happens automatically) and the tank sustain HoT (A2).
Everything else is established in the code and compiled in CI — which says that a chain
closes, not whether it is right in the game. Where a change sits behind a switch by
default, it says so; without that note it takes effect immediately.

**This build carries a known defect.** It is in § 5.3, and it is why no Addle and no
Radiant Aegis have gone out against area damage on Summoner since 17 September.

---

## 1 · Healing — when it lands

**A pull no longer starts without healing.** `DataCenter.AverageTTK` returned `0f` in the
original while no enemy carried an estimate yet. Every consumer read that as "the fight is
about to end" and switched off — so for roughly the first 2.5 seconds of every pull there
was no automatic healing at all. Fixed.

**The white mage heals again while a pack is dying.** `StateUpdater.CanUseHealAction`
applied the TTK gate `AutoHealTimeToKill` (8 s) to healers as well, although the option
hangs under `UseHealWhenNotAHealer` and means non-healers. As soon as the average enemy
time-to-kill fell below eight seconds — that is, at the end of every pack — **all** heal
flags went out: tank below 20 %, no heal attempted, Holy instead of Cure. That was the
reported cause (#54).

**Healing ahead of the hit instead of after it** — `Heal ahead of incoming damage`, **off
by default**. Every healing threshold and the heal target choice read the health a member
is heading for by the time a heal started now would land, instead of the health shown right
now. In a fight: a tank dropping fast gets his heal about one GCD earlier, and someone
falling quickly is picked ahead of someone sitting lower but steady. The rate per party
member comes from the health history the tree already keeps (`RecordedHP`, 1 Hz over four
minutes), which the fork fills for the party as well rather than only for enemies; it is
therefore net of everything — mitigation, barrier and healing included. Because the
estimate averages over the whole fight and is sluggish for a party member, `ScoreTtkForecast`
holds each previous prediction against the actual course every second and `GetCorrectedTTK`
divides the error back out.

**The emergency full heal waits for a reason** — `Benediction needs a reason`, **on by
default**. Benediction now requires, beyond the low reading, that the target is being
attacked, that an area cast is announced, or that their health is measurably falling. In a
fight: a player who was just raised holds a few percent, carries no aggro and takes no
damage — the threshold read him as the most urgent member in the party while nothing was
happening to him, and the full heal was then gone for ninety seconds.

**The tank walks into the pull with a HoT already ticking** — `UsePreRegen` (WHM),
`UsePreAspectedBenefic` (AST), each with two enemy-count thresholds. Before combat the
regen goes on the tank once enough enemies stand within gap-closer range, and it is kept up
while the pack stays large enough; below that, healing falls back to the thresholds. In a
fight: the first hits land on a tank who already has something running rather than on one
who is already low. Both actions are instant, so nothing is lost while moving. **Confirmed
in play.** The original's countdown branch for trials and raids is restored — the fork had
removed it.

---

## 2 · Healing — who it picks

**Whoever is about to die comes before the role short-cuts.**
`ActionTargetInfo.FindHealTarget` sorted by role with fixed thresholds in the original
(tank ≤ 45 %, healer ≤ 40 %) and stopped at the first short-cut. A damage dealer at 10 % was
therefore passed over as soon as the tank sat at 44 %. The fork puts a critical rank ahead
of them. That order is locked in CI (`check_heal_target_order.py`), because an upstream
merge would otherwise undo it silently.

**A party member behind the camera can be healed again.** The "only attack targets in view"
filter applied to heal targets as well in the original.

**Invulnerable targets** are treated specially rather than not at all: upstream gives a
target under invulnerability no healing whatsoever; the fork lowers the threshold to
`HealthProtectedRatio` and lets the normal one return shortly before the status ends
(§ 6.2).

---

## 3 · Shields and barriers

**The shield list did not know the most common barriers.** `ShieldStatus` carried neither
Divine Benison nor The Blackest Night; fifteen barriers were missing in total. Wherever the
tree asked "is anything still up on this target", the answer was no for most tank shields.
Filled in and locked against regression.

**Crediting the shield against the healing threshold is out again** — by the user's
decision (A85), and for the right reason: a shield prevents damage, it does not restore
health. A tank at 40 % stands at 40 % whether a barrier is running or not; if it expires
unspent, the healing withheld for it was thrown away.

**Open and deliberately left alone:** `HasSurvivingShield` measures the **shortest**
remaining time across all barriers, not the longest — a target with three barriers counts
as unshielded as soon as the smallest runs out. The error direction is "too much healing",
never "too little"; the obvious inversion trades it for the dangerous direction. Recorded in
`TODO.md`.

---

## 4 · Raising

**It now happens at once instead of after a long delay** — the one change with an
observation in play (A73). The cause was a chicken-and-egg problem: the ability path only
spent Swiftcast on a raise once the raise was already reported as the next GCD — which it
could not be, because that report itself required Swiftcast. In manual mode with the corpse
hard-targeted it worked, because the GCD stayed free there; that was the proof. The fork
adds the missing trigger: Swiftcast also falls when a raise is pending and would be
castable. The GCD path is untouched — the first attempt rewrote it and silenced, among
other things, Radiant Aegis on Summoner; it was reverted (C37).

**The Phoenix Down** is wired up, checks target eligibility through the item status and the
level of the raise trait. **Off by default.**

**The healer-only hard-cast modes** measure the right set (which healers count) and hold the
Swiftcast reservation their own text promises: with Swiftcast ready, nothing is hard-cast
any more.

Swiftcast is not spent for damage in the Summoner rotation (A77) — the user's instruction;
it stays reserved for raises.

---

## 5 · Mitigation and defensives

### 5.1 What is safely better

**Two tanks no longer stack Reprisal.** In the original Reprisal lacked the
`TargetStatusProvide` that Addle and Feint have, so the second tank threw away 60 seconds of
cooldown. The same applied to the enemy-count branches of Addle and Feint with two casters
or two melees.

**Paladin and Warrior now give Reprisal on raidwides too** — it sat only in the
single-target path, although the Warrior's own description promised it for area. Dark Knight
and Gunbreaker had it right.

**The mitigation debuffs are sustained rather than applied once**
(`ShouldSustainMitigationDebuff`, one helper instead of 25 copies). The duration follows the
level 98 trait.

**A buster aimed at a damage dealer gets an answer.** Twelve jobs use their existing
reactive line — Feint, Addle, Troubadour, Tactician, Shield Samba, and Third Eye on Samurai
— when a cast is actually aimed at them. Before that, single-target defence for damage
dealers was not staffed at all.

**Self-healing for damage dealers** (Second Wind, Bloodbath) across ten jobs: the role
actions were declared and never used, `HealSingleAbility` was empty.

**The BossModReborn timeline is only read when it is switched on.** Four helpers reacted to
it even with the option off.

### 5.2 What the area detection does today

The tree learns area actions as it goes: when an enemy action hits every member of a party
of four or more in the same effect set, its id is recorded permanently. The fork
additionally measures **how hard** it hits — the share of maximum health the impact cost is
stored per action. The list window shows both.

### 5.3 The defect this build carries

`Skip mitigation for small area casts` is **on by default**, and with that default the party
mitigation is withheld in almost every ordinary case:

- The condition reads "buffer minus measured share below the healing threshold (0.65)".
  With a healthy party the buffer is 1.0 — so mitigation only happens above a share of
  **35 %** of maximum health. An ordinary raidwide does not reach that.
- The share is only recorded **after the first impact**. So the first cast of an action
  still mitigates, and every later one does not.

In the fight that means: on Summoner, neither Addle nor Radiant Aegis has gone out against
area damage since 17 September, because `DefenseArea` is no longer set. Every party-wide
mitigation hanging off that chain is affected, and through `IsUnderThreat` so is the danger
check on the emergency full heal (§ 1).

**The handle:** switching the setting off restores the behaviour from before 17 September.
Changing the default in code is not enough — a configuration already in use carries the
stored value, and `Configs.Migrate` does not convert individual fields. How the evaluation
itself should be corrected is a decision recorded in `TODO.md`.

**Where the defect comes from:** the evaluation was introduced with its default **on**. That
breaks the fork's own rule that a behaviour change without a way to verify it keeps the
previous default and offers the new behaviour behind a switch.

---

## 6 · Tank self-protection

### 6.1 The Blackest Night

The barrier costs 3000 MP and only repays it as Dark Arts when it is **fully absorbed**. The
original's trigger is far too weak for that — two enemies in melee range, or any
uninterruptible cast aimed at you. `BlackestNightUsage` offers narrower readings with a
minimum enemy count and an emergency share below which the barrier goes up unconditionally.
**The default is the old behaviour.**

The other side of it: the white mage can hold Holy back while a tank carries the barrier
(`HoldHolyForBlackestNight`) — the stun stops exactly the hits that would spend the barrier,
and without them it expires unused.

### 6.2 Living Dead and Walking Dead

The staging follows the user's instruction: while Living Dead has more than two GCDs left,
the lowered threshold `HealthProtectedRatio` (0.15) applies — so the death effect can
occur. When the status ends within two GCDs or less, the normal threshold returns, so
healing resumes shortly before it expires. That lead-in is suspended while zero would arrive
before the window closes (`DeathStillLikely`), because otherwise it would prevent the very
death the rule exists for. `WithholdHealingForLivingDead` only sharpens the first part and
is **off by default**.

Open and recorded in `TODO.md`: during the Walking Dead phase the HoT is locked out by the
`RegenHeal` threshold (the carrier sits at 1 HP), and Benediction fires at the start of the
phase rather than at its end.

### 6.3 Arm's Length and Provoke

`UseArmsLengthOnPull` uses the action on a group pull for its Slow, not only as knockback
protection — which was the only way the plugin ever used it. The Slow of +20 % lands on every
enemy that strikes you and throttles the whole incoming stream for fifteen seconds.

The co-tank Provoke no longer pulls the boss off a tank standing under Superbolide, Living
Dead or Holmgang.

---

## 7 · Damage and rotation

**Summoner.** Searing Light's firing window is tied to the burst phase: in Solar Bahamut,
or in Bahamut at lower levels. With a second Summoner in the party the rule falls back to
the big summon, and across all established phases to Ifrit. `PreferTitanWhileMoving` (**off
by default**) brings Titan forward while you are moving — Topaz Rite and its follow-ups are
instant, while Garuda and Ifrit need you standing still and lose GCDs on the move. Titan is
only brought forward, never skipped.

**White mage, Holy.** Three rules, each separately switchable: do not overwrite the stun
while it is still running (`StretchHolyStun`, **off by default**); hold Holy while the dark
knight's barrier is meant to be filled; and hold Holy while more than half the enemies in
radius are slowed and at least the configured number carry the Slow — Slow and stun throttle
the same stream, and the stun is worth more once the Slow has run out. A ceiling on the
enemies' remaining output limits the saving.

**Thin Air** is only spent on an expensive spell when MP pressure is actually there and
Lucid Dreaming cannot answer it (`ThinAirOnMpPressureOnly`). A raise still takes a charge
regardless.

**Defects of the original with an effect in the fight:**

| Site | What was wrong | What happened in the fight |
|---|---|---|
| Black mage, Thunder | The refresh gate did not list `HighThunder` | From level sync 92 a fresh area DoT was cut short on every cast |
| Red mage, Impact | `!Impact.EnoughLevel && Impact.CanUse` | Branch unreachable, Impact never fired |
| Nine `base.X` calls | Overrides called the wrong base method, e.g. `DefenseSingleGCD` → `base.DefenseAreaGCD` | The dispatch chain continued silently in the wrong place; compiles cleanly, invisible in a diff |
| Interrupt / anti-knockback | The role default ran **before** the job override | Reaper and Viper gave up Leg Sweep and Arm's Length to their combo gate, and the default took them ungated anyway |
| Phantom job branch | `out _` instead of `out act` | The action was recognised and never returned |
| `MoveBackAbility` | Called in the `if` head **and** in the body | Double call, wrong order against the duty rotation |
| Restricted DoT guard | `continue` in the inner loop instead of the outer one | Blocked targets got DoTs anyway |
| `CalculateDamageFactor` | `foreach` over the party with no body | Dead code |

Three of these classes are locked out in CI (`check_base_calls.py`) so a merge cannot bring
them back.

**Movement slots** for Gunbreaker, White Mage, Bard and Samurai (eight lines each): they run
only under `MoveForward`/`MoveBack` and cannot reach the damage rotation.

**Long GCD chains split into named stages** (Blue Mage, Phantom, Pictomancer, Samurai,
Summoner): method boundaries inserted only, not a line moved, order and behaviour unchanged.

---

## 8 · What the fork rolled back of its own mistakes

- **Sage sustain** ran for the whole pull: the condition checked the shield status itself,
  and a shield bursts in seconds in a wall-to-wall — two GCDs per burst, mostly out of
  damage. Removed; on White Mage and Astrologian the HoTs tick down their duration, so the
  helper stays there.
- **The Weakness threshold factor** healed practically always: multiplying by 1.5 clamped
  the threshold to 1.0, so a weakened player counted as needing healing at any health below
  full — for one hundred to three hundred seconds after every raise. Removed.
- **The white mage DoT guard** checked the target of the **previous** cast, because `Target`
  is only assigned inside `CanUse`. Rolled back to the upstream form.
- **An upstream feature had been deleted**: the countdown regen for trials and raids.
  Restored.
- **The first raise attempt** rewrote `nextGCD` — 447 readers, among them Radiant Aegis on
  Summoner, which went silent. Reverted (C37); the second attempt does not touch the GCD
  path.
- **A `[WSH 16/18]` marker** in the window title implied a versioning that was never
  updated. Removed.

---

## 9 · What does nothing in the game

Roughly half the diff: `TODO.md`, `AUDIT_LOG.md` and thirteen concept documents; the German
name index and its generator (`GermanNameIndex`, which only runs with the game installed);
and the check scripts with their CI job. The last of those are not decoration — they lock
out defect classes that have occurred at least once. Thirteen run on every build: wrong
`base.` target and contradictory level predicate (`check_base_calls`), structurally broken
C# files, lost heal target order, line references in documents, German name mapping, fork
version designation, MSBuild XML, set lookups, missing explanation texts, the danger check
on the emergency heal, and three pattern scans. The rest — among them the sync measurement —
are built to be run by hand.

The package identity carries a prerelease label rather than build metadata (`-wsh1` instead
of `+wsh1`), because NuGet normalises the metadata away and the shipped package was
otherwise indistinguishable from the upstream one.

---

## 10 · What is open

In full in `TODO.md`, separated into defects and technical debt. The points with an effect in
the fight, in short:

- **The area evaluation from § 5.3** — decision pending.
- **Area healing still decides by level rather than by rate**, unlike single-target healing.
  Not fixed alongside it, because the same figures have 83 readers outside the healing chain,
  among them third-party rotations.
- **The emergency heals of Sage, Scholar and Astrologian** do not check for danger, unlike
  the White Mage. Recorded, not worked on — outside the user's job profile.
- **Walking Dead**: HoT locked out, Benediction at the wrong end of the phase.
- **Searing Light does not fire at the start of the burst phase**, as reported. The cause has
  not been found: for a single Summoner the firing condition is behaviourally identical to
  upstream and only says **whether**, never **when**. If the action misses the first weave
  slot it falls at the next free one — nothing pulls it forward.
- **`HasSurvivingShield`** measures the shortest rather than the longest barrier remaining
  (§ 3).
