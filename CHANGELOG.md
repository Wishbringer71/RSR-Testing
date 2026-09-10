# Changelog

Changes in this fork that a consumer has to act on, kept because the version number cannot carry
them.

`Directory.Build.props` ties `<Version>` to the upstream release the fork is based on. The package
carries the fork marker as the **pre-release label** `-wsh1`, which is part of the version identity
and survives normalization — unlike the build metadata `+wsh1`, which NuGet strips (`1.0.7+r3456` is
treated as `1.0.7`,
[Package versioning](https://learn.microsoft.com/nuget/concepts/package-versioning#normalized-version-numbers)).
The assembly and informational versions keep `+wsh1`, since that is what the settings window shows.

The number therefore identifies the fork, but it still cannot express *what changed*: the numeric
part follows the upstream release the fork tracks, not this fork's own compatibility, so a breaking
change to the package surface has nowhere to show up under Semantic Versioning's rules. That is what
this file is for.

Consuming the package means opting into pre-release versions — reference the exact version, or allow
pre-release in the client. That is the price of sharing `PackageId` with upstream.

Entries below the unreleased section start with the first release that carries one; earlier releases
are not reconstructed here.

## Unreleased

### Changed in RotationSolver.Basic

`StatusHelper.NoNeedHealingStatus` still exists with the same signature, but its contents changed,
so a derived rotation reading it sees different behaviour without any compile error to warn it.

- **Removed** `HpRecoveryDown` and `Mounted`. Neither is an invulnerability: the first only reduces
  incoming healing, the second nullifies HP recovery outright. Anything asking this list "can the
  bearer be killed right now" was getting a wrong answer for both. `Mounted` moved to the new
  `StatusHelper.HealingIneffectiveStatus`, which is what the target selection excludes.
- **Added** `HallowedGround`, `HallowedGround_1302` and `UndeadRebirth`. The first two were simply
  missing, so paladins were treated as unprotected; the third is the success phase of Living Dead.
- **Added** `StatusHelper.DeathTriggeredStatus` and `HealingIneffectiveStatus` as separate lists,
  plus `InDeathTriggerWindow`/`PlayerInDeathTriggerWindow`. All additive.
- **Added** `CustomRotation.TankbusterOnMe` — a tankbuster landing on the player, either detected
  (lock-on VFX or an action from the learned list) or predicted by BMR inside the mitigation window.
  Additive. It deliberately excludes `IsHostileCastingToTank`, which counts any enemy casting at its
  own target and therefore fires on ordinary trash casts for whoever holds the pull.
- **Added** `CustomRotation.HasMajorMitigation` (`protected static`), which reads
  `StatusHelper.RampartStatus` on the player. It names the condition under which an absorbing
  barrier is wasted in a pull: a big mitigation running alongside it drops the incoming damage below
  the rate that would spend the barrier. Additive.

A rotation that reads `NoNeedHealingStatus` to decide whether to skip a heal keeps working; one
that relied on `HpRecoveryDown` or `Mounted` being in there has to name them itself now.

`StatusHelper.ShieldStatus` grew from 22 ids to 60, again without a signature change. The list
decides, through `HasSurvivingShield` and `GetEffectiveHpPercent`, whether a shield counts toward a
target's effective health; an id that is missing inverts the answer rather than blurring it, because
`WillStatusEnd` reports an absent status as ending. Added were the 21 missing siblings of ids
already listed, Celestial Intersection, and the barriers whose PvE action creates one: Shake It Off,
Seraphic Veil, Neutral Sect, The Spire, Improvised Finish, Divine Caress, plus the Occult Crescent
party barriers. A derived rotation that reads the list sees more ids and therefore fewer targets
counted as unshielded.

`StatusHelper.RampartStatus` and `StatusHelper.ReprisalStatus` gained ids the same way, again
without a signature change, after `.github/scripts/audit/scan14.py` asked the sibling question of
every list rather than of the barriers alone.

- `RampartStatus` **added** `Rampart_1191`, `Rampart_1978`, `Rampart_4168` and
  `HallowedGround_1302`. `Rampart_1978` is the form a tank carries from level 94, so the most
  common mitigation in the game was invisible to every reader of the list — including the
  `StatusProvide` staggering that keeps Shadow Wall and Shadowed Vigil off a running Rampart.
- `ReprisalStatus` **added** `Reprisal_2101`, the form Enhanced Reprisal upgrades into at level 98
  (15% for 15s). `ReprisalPvE` carries the list as `TargetStatusProvide`, so the guard against
  re-applying the debuff never saw it on an end-game tank's target.

Both are inclusions, not removals: a derived rotation reading either list gets `true` in states
where it previously got `false`, and nothing that used to match stops matching.

- **Added** `StatusHelper.SlowStatus`, all twelve ids the game files under that display name,
  Slow+ included. Additive. Its effect text names the auto-attack delay alongside cast and recast
  time, which is what makes a slowed pack a throttled damage stream rather than a caster nuisance —
  the reason a rule can read it at all.
- **Added** `CustomRotation.SurveyHostileStatus(float, StatusID[], out int)` (`protected static`),
  a plain "how many hostiles in radius carry one of these" counter. Additive, and deliberately
  separate from `SurveyStuns`, which carries stun-specific reasoning (resistance, headroom) that no
  other effect needs.
- **Added** `StatusHelper.FullAbsorbRewardStatus`, holding The Blackest Night alone. Additive, and
  deliberately not a subset of `ShieldStatus` for callers to filter: it answers a different
  question. Every other barrier is pure protection, where an unspent remainder is a good outcome;
  this one grants Dark Arts only when the barrier is absorbed in full, so letting it expire wastes
  its cost. A rotation that would otherwise stop the damage stream — the white mage's Holy stun is
  the first caller — needs that distinction, not "is any shield up".

### Removed from RotationSolver.Basic

Both members were part of the shipped `7.5.5.41+wsh1` package. Code that overrides or reads them no
longer compiles; stored user configuration is unaffected, because Dalamud skips members it does not
recognise when deserialising.

- `CustomRotation.HasHostileCountAoeMitigation` (`public virtual bool`). Setting it kept
  `AutoStatus.DefenseArea` raised for the whole of any pull with four or more enemies in range, and
  that flag opens a job's entire defensive chain rather than the one sustain line it was named for.
  Mitigation now needs a detected area cast or a predicted raidwide again, which left the flag
  without readers. There is no replacement to migrate to. The declaration in `ICustomRotation` went
  with it, but that one was `internal` and never part of the package surface.
- `ActionConfig.ShouldCheckTargetStatus`. It had no UI and no setter, so it stayed at its default of
  `true` and made `ShouldCheckStatus` — the checkbox users actually see — unable to take effect. The
  per-action status check now runs off `ShouldCheckStatus` on both the player and the target side.
