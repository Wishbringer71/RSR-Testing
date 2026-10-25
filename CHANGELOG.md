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

### RotationSolver.Basic: a rotation's GCD heals yield to danger

New on `CustomRotation`: `GcdHealsAllowed(useGcdHeals)`, for a rotation that leaves its GCD heals to the healers - true
when no other healer is alive or a living member is in the critical class. The seven default rotations with
"Use GCDs to heal" read it.

### RotationSolver.Basic: a tank's big mitigation and Rampart in one place

New on `CustomRotation`: `StaggeredBigMitigation(big, upgraded, out act)`, the big mitigation and Rampart outside an
announced hit as the four default tank rotations carried them, and `BigMitigationHeldInPull(mitigation)`, the hold of
"Hold the big mitigation and Rampart in a pull until your health needs them". A derived tank rotation that keeps its
own copy of the old block is not affected by the setting; calling the method gives it the hold.

### Behaviour change in RotationSolver.Basic: Intervene keeps to the damaging gap closer distance

`PaladinRotation` gives `IntervenePvE` a `Setting.CanTarget` that admits a target only closer than "Only use damaging gap
closers within this distance", unless a move-forward command, the player's own press of the action or the countdown is
active. A derived Paladin rotation that used Intervene as a long-range gap closer outside the move-forward path gets it
only within that distance. New on `CustomRotation`: `GapCloserAllowed(target, rotationLimit)`, the same check for any
leap onto a target, with a rotation's own limit on top; and `PartyMembersForecastAverHP(instant)`, the party's average
health at the moment a heal begun now would land, for a party-average healing threshold under "Heal ahead of incoming
damage" (equal to `PartyMembersAverHP` with that setting off).

### Behaviour change in RotationSolver.Basic: damage areas wait until the enemies stay in them

`BaseAction.CanUse` refuses an action whose effect text lays a damage area of its own ("any enemies who enter") and that
can sit on a hotbar - Salted Earth - until, around where it lands, at least one enemy has been within its effect range
for the last GCD and no fewer living enemies are within it than were at the start of that GCD, no enemy out of combat
stands within it or, in a duty that is not a field operation, within a party tank's area attack reach, and none of those
within it or at a tank came into combat within that GCD. Governed by the new setting
`DamageAreasWhereEnemiesStay` ("Lay damage areas only where the enemies stay", on by default). `CustomRotation.DamageAreaSettled(IBaseAction area, IBattleChara?
centre = null)` is new and protected for rotations that decide such an area elsewhere: `NIN_Reborn` asks it when the
mudras for Doton begin, `SMN_Reborn` before Slipstream while Emerald Rites are left. `ActionTooltip.LeavesDamageArea`
and `ActionTooltip.TryStatedDuration` are new (internal); `NinjaRotation` and `TraitDurations` read the stated duration
through the latter.

### RotationSolver.Basic: Liturgy of the Bell into a series of hits

`WhiteMageRotation.LiturgyTriggersLeft` is new and protected: the triggers a bell laid now would still collect from an
enemy attack hitting the player and another party member in a series, learned per action in `HitSeriesOnPlayer.json`.
`WHM_Reborn` gains two sub-options of its multihit limit, both on.

### Behaviour change in RotationSolver.Basic: the area table rates a series of hits

A listed area cast that lands again within its own cast time adds its shares per member; the table stores the highest
sum, at most a whole maximum HP. Readers of `HostileCastingAreaPotential` and `DataCenter.AnnouncedAreaShare` see the
larger figure for such casts.

### Behaviour change in RotationSolver.Basic: a melee's dash at the end of a countdown

`CustomRotation.PullGapCloser(float remainTime, IBaseAction dash, IBaseAction reach, out IAction? act)` is new and
protected: in a countdown's last half second it uses `dash` when the hard target is an enemy beyond the range of
`reach`, and only if the dash goes to that target. Governed by the new setting `PullGapCloser` ("Dash to the target at
the end of the countdown when out of melee range", on by default). `MNK_Reborn` now uses Thunderclap through it - no
longer in range, no longer to a party focus target -, and `VPR_Reborn` (Slither) and `DRG_Reborn` (Winged Glide) gain
a `CountDownAction` that does the same.

### RotationSolver.Basic: the Ninja weighs its area actions

`NinjaRotation` gains three protected members: `HellfrogOutdamagesBhavacakra(int targets)` (Meisui's raise included),
`GokaOutdamagesHyosho(bool fallback)` (counts the enemies Goka Mekkyaku reaches) and `DotonOutlived` (the hostiles'
average time to kill against Doton's duration). Hellfrog Medium and Deathfrog Medium default to an AoE count of two.
`NIN_Reborn` asks all three.

### Behaviour change in RotationSolver.Basic: Foul's AoE count is two

`BlackMageRotation`'s Foul now defaults to an AoE count of two instead of three: on two enemies it out-damages
Xenoglossy at level 100. A rotation that calls `FoulPvE.CanUse` before Xenoglossy uses it on two enemies; the two
bundled Black Mage rotations also compare the tooltips' potencies before they do.

### RotationSolver.Basic: the Machinist weighs Auto Crossbow against Blazing Shot

`MachinistRotation.AutoCrossbowOutdamagesBlazingShot(int coneTargets)` is new and protected: true when Auto Crossbow on
that many enemies deals more than Blazing Shot (Heat Blast below its level) with the recast it takes off Double Check
and Checkmate (Gauss Round and Ricochet), reaching as many enemies as their own target selection finds. The figures
come from the tooltips at the synced level; where one cannot be read it answers true. `MCH_Reborn` asks it after
`AutoCrossbowPvE.CanUse`.

### Behaviour change in RotationSolver.Basic: `DataCenter.DPSTaken` counts only hits that did damage

The damage record behind `DataCenter.DPSTaken` no longer takes a zero for every enemy action that missed the player. Its
span runs from the first to the last damaging hit within five seconds, so the rate reads higher where misses used to
widen it; in a large party the fixed-size record now covers more than a few seconds.

### RotationSolver.Basic: tanks spread Reprisal and the party tool over area attacks

`CustomRotation.TankAreaDefenseStretched(params IBaseAction[] partyTools)` is new and protected: true while Reprisal or
one of the given party tools still works, with "Spread Reprisal and the party mitigation over a run of area attacks"
on and no member in danger. The four tank rotations ask it before Reprisal and their party tool.
`AreaDefenseStretched` now ends a stretch once the trigger's effect is gone from the player - a beneficial status named
like the action, a GCD after the press -, instead of always after the duration its text states: a barrier used up, Liturgy
of the Bell released by its second press. An action without such a status, Reprisal on the enemy, keeps its duration;
`SingleDefenseStretched` is unchanged.

### Behaviour change in RotationSolver.Basic: an enemy's cast is stopped by the job's stun first, and what sleeps show is kept

The healers' and casters' stop of an enemy cast (`Configs.SleepToStopAreaCasts`, name unchanged) now also covers casts
the game marks interruptible, and asks the job first: `CustomRotation.StopEnemyCastByJob` is new and protected virtual,
and `WhiteMageRotation` overrides it with Holy or Holy III. `CustomRotation.AreaReachesNewMob` is new: whether an area
action would reach an enemy that is not fighting yet while `Configs.NoNewHostiles` is on.

`SleepInterruptWatch` keeps what it learns over every session, in three learned tables beside the area table, and works
out its conclusions from them at each decision. New public members: `IsCastWorthStopping` (any stop), `IsCastToStop`
(a sleep), `IsUnstoppable` and `LandingMargin`. `CanStillBeSlept` also refuses an enemy of a kind that resisted, an enemy slept as often in this fight
as its kind takes, and one a sleep has just landed on. `IsAreaCastToStop` refuses an action stored as unstoppable
instead of one that went off this session.

### Behaviour change in RotationSolver.Basic: a boss's invisible helpers are read as the enemy side

The enemy-side recognition behind the effect handler and `DataCenter.AreaCasters` takes battle NPCs of sub kind 11, the
kind BossModReborn calls Helper. A derived rotation reading `DataCenter.AreaCasters` sees their casts.

### Behaviour change in RotationSolver.Basic: `InterruptDelay` and `ProvokeDelay` default to no delay

Both settings are read again (the target update applies them before choosing `DataCenter.InterruptTarget` and
`DataCenter.ProvokeTarget`); their defaults change from 0.5–1 s to 0–0 s. A configuration saved earlier keeps its value.

### RotationSolver.Basic: the enemy mitigation debuffs' duration comes from the game

`CustomRotation.MitigationDebuffDurationOf(params StatusID[])` is new: the duration at the player's synced level of the
debuff the statuses belong to, read through `DefensiveValues.DurationOf` of the action that lays them.
`MitigationDebuffDuration` stays and returns Reprisal's; it no longer carries the 10 s / 15 s from level 98 written in.

### Behaviour change in RotationSolver.Basic: a marker on a party member keeps the default window

`VfxNewData.Duration` is the remaining cast time of the figure an effect sits on only when that figure is on the enemy
side; for a party member or another player it is zero, so `DataCenter.VfxDataQueue` keeps the effect for the default
window. Before, a marker on a member who was casting lasted only for the rest of that member's own cast. A derived
rotation reading the queue sees such markers for the full window.

## 7.5.7.6+wsh1

### Behaviour change in RotationSolver.Basic: tankbuster targets follow whom a tankbuster is aimed at

`TargetType.Tankbuster` picks, among the action's own candidates, a member a known tankbuster is aimed at - a
tankbuster marker on him, a cast at him of a listed or measured tankbuster, or for the player BossModReborn's mask -,
the player first. It used to read only the marker list, which for a tank held only a marker on himself, and fell back
to a list entry the action could not take. `TargetType.BeAttacked`, which the single-target and area defence set for
every friendly action without its own target, narrows to such a member when one is among the candidates; otherwise it
chooses as before. Under `Configs.HoldMitigationUnderInvulnerability` both leave out a member who takes every
tankbuster aimed at him under Hallowed Ground or Superbolide. A derived rotation whose friendly single-target
mitigation goes through these target types now puts it on the tankbuster's target. `CustomRotation.HelpTankbusterTarget`
is new: an action on another member a tankbuster is aimed at, never on the player.

### Added to RotationSolver.Basic: a sleep on an enemy's area cast (experimental)

The GCD path of a healer or caster puts Repose or Sleep on an enemy that is not a boss and casts an area attack
reaching the party, while enough of the cast is left, under `Configs.SleepToStopAreaCasts` (on by default). It runs
before dispel, provoke, raise and heal; a derived rotation that wants the GCD for something else can switch the
option off. `SleepInterruptWatch` is new and public: what such a sleep did, which enemy kinds resisted and which casts
went on. `JobFilterType.Sleep` is appended to the enum.

### Removed upstream in RotationSolver.Basic: members folded into upstream's 7.5.7.5 rework

Upstream 7.5.7.5 (merged into this fork on 22.10.2026) removed these public members; the fork passes the removal on.
A derived rotation that uses one of them no longer compiles.

- `BaseAction.AnimationLockTime`, `ActionHelper.AnimationLockTime` and `ActionManagerHelper.AnimationLock` - read
  `Player.AnimationLock` (ECommons) or `DataCenter.AnimationLock` instead.
- `DataCenter.DistanceToPet` is `ObjectHelper.DistanceToPet` now.
- `OtherConfiguration.Init` - `InitAsync` is the entry point.
- `ConfigurationHelper.BadStatusAbility` and `BadStatusGCD`, `DataCenter.BMRDebugTimelineWalk`, and the constants
  `Service.AUTOCOMMAND` and `OFFCOMMAND`, without replacement.
- `ActionManagerEx.UpdateTweaks`, `UseActionWithTweaks` and `UseActionLocationWithTweaks`, and the struct
  `OffsetDelay`. The animation lock and cooldown delay tweaks now hook the client's action manager, so they apply to
  every action, whoever uses it; an ordinary `ActionManager.UseAction` is enough.
- `ActionConfig.GCDSingleHeal` is `ActionSetting.GCDSingleHeal` now: set it in the action's `Modify…` method rather
  than through `CreateConfig`. Saved action settings that still carry the key load as before - the serializer ignores
  it.
- `TargetHelper.GetTargetsByCastAndEffect` and both `GetTargetsUsableByAction` overloads - use `GetTargetsByRange`.
- The 30 encounter immunity extensions on `ObjectHelper` (`IsYmirImmune`, `IsM9SavageImmune`, `IsTOPImmune` and the
  rest) - `ObjectHelper.IsSpecialImmune` answers for all of them - and `ObjectHelper.IsBroPriority`, the M10S target
  priority, which the target selection now reads internally.

`TargetType.Capture`, also removed upstream, stays in this fork, unused: without it the value of `TargetType.TankSwap`
after it would shift for a rotation built against this package.

### Behaviour change in RotationSolver.Basic: `DataCenter.AnyLivingRaiser` counts the alliance under `AllOutOfDuty`

Since upstream 7.5.7.5 `DataCenter.AllianceMembers` holds the alliance whatever `RaiseType` is set, and strangers
outside a duty are kept apart. `AnyLivingRaiser` left `RaiseType.AllOutOfDuty` out because the list held them; under
that mode it now counts living alliance raisers as it does under `RaiseType.All`. A derived rotation that holds a hard
cast or a Phoenix Down on it waits for them as well.

## 7.5.7.3+wsh1

### Dependency: ECommons 3.2.1.26

RotationSolver.Basic now references ECommons 3.2.1.26. It reads the action type of an effect header as the one
byte the game gives it; up to 3.2.1.25 it read four bytes, and an item used with a target came through
`ActionEffectSet` as an action of the same number. A rotation built against RotationSolver.Basic needs ECommons
3.2.1.26 or later.

### Behaviour change: a damage over time chooses its target by HP, then by expiry

`ActionTargetInfo.FindTarget` now orders the candidates of a damage-over-time action itself: the one whose own
damage over time runs out first, a missing one counting as run out, and among equals the one with the most HP. The
targeting type (`DataCenter.TargetingType`, by default "Low HP") no longer decides there. It applies only where the
status check chose the candidates and no target override or command is given; `Configs.DotTargetByHpThenExpiry`
switches it off.

### Behaviour change: `TargetFilter.GetDeath` leaves out the dead someone else is raising

`GetDeath` now also skips a dead member another player is casting a raise on at that moment, recognised by the
action's `DeadTargetBehaviour`. The body was a candidate until the `Raise` status landed, which made two healers
cast on it. A rotation that picks its raise target through `GetDeath` or `DataCenter.DeathTarget` leaves such a
body to the other caster. `GetDeath` also leaves out a body carrying `Raise_1140`, the second id of the Raise
status, through the new `StatusHelper.RaiseStatus`.

### Behaviour change: the area heal flags rise ahead of an announced area hit only when it endangers someone

`AutoStatus.HealAreaAbility` and `AutoStatus.HealAreaSpell` used to rise before an announced, measured area hit
whenever it would push anyone below the area heal thresholds, which a raidwide of a quarter or a third of the
health did from full health. They now rise ahead of it only when it would leave an unprotected member at or below
`HealthForDyingTanks` with health still missing (`DataCenter.AnnouncedHitCallsForHealFirst`); otherwise the flags
follow the health after the hit. A rotation that heals on these flags heals after such a hit, not before it.
`CustomRotation.HealForResourceWaitsForAreaHit` is new: a heal spent only against a resource overflow can ask it
to wait until an announced area hit has landed.

### Behaviour change in RotationSolver.Basic: `CustomRotation.TankbusterOnMe` asks whom BossModReborn's tankbuster hits

`TankbusterOnMe` was true for every tankbuster BossModReborn predicted inside the mitigation window, whoever it
targeted. It now counts a prediction only when it reaches the player: BossModReborn's mask names him, or, without a
mask for that event, the player is a tank or the party has no living tank. A cast at the player or a tankbuster
marker on him counts as before. A derived rotation that spent a self-defence on `TankbusterOnMe` no longer does so
for a tankbuster on the other tank. The signature is unchanged.

### Behaviour change in RotationSolver.Basic: `DefensiveValues` reads four more mitigation phrasings

The generator now reads a mitigation that names its bearer ahead of "damage taken" ("Reduces target party member's
damage taken by 10%", "Reduces target's damage taken by 10%"), the one-hit form ("Reduces the amount of damage taken by
the next attack"), and a mitigation of one damage type ("Reduces magic damage taken by 10%"), carried as
`SelfPhysical`/`SelfMagical` with the other type at 0. `ByActionId` and `MitigationByStatusId` gain Intervention,
Third Eye, Taurochole, Magick Barrier, Fey Illumination and their statuses, among others. `DurationOf` for Tengentsu is
now 4 s, the duration of its own reduction, where it gave the 9 s of Tengentsu's Foresight; Lost Font of Magic and Lost
Font of Power give 15 s instead of 30 s for the same reason. No signature changes.

### Added to RotationSolver.Basic: trait-set damage reductions in `DefensiveValues`; statuses rated by their own text

`DefensiveValues.For` now gives the share a trait sets where the effect text leaves it blank - Troubadour, Tactician and
Shield Samba, 15 % from level 98 - read in the game first and from the new `TraitMitigationByActionId` otherwise
(`TraitShareOf`). `DefensiveValues.TryStatus` answers for a status from `MitigationByStatusId` or, for those three
actions' statuses (`TraitMitigatingActionByStatusId`), from the trait. `MitigationByStatusId` and
`MitigatingStatusesByActionId` now hold only statuses whose own text states a reduction or a raised maximum HP; a
status that merely shares an action's name (Temperance's heal over time, Aquaveil's barrier, Nebula's reflection, an
older form of the action) no longer carries its figure. A derived rotation that read those statuses as the mitigation
standing no longer does. No signature changes.

## 7.5.7.1+wsh1

### Added to RotationSolver.Basic: `AttackType.Shot` and `DefensiveValues.MitigationByStatusId`

`AttackType` gains `Shot = 4`, the game's AttackType row 4 (physical), which `ActionBasicInfo.AttackType`
already produced as an unnamed value. `DefensiveValues.MitigationByStatusId` maps a status to the mitigation
the action that grants it states in its effect text. Both are additive; no ordinal changed.

### Added to RotationSolver.Basic: `DefensiveValues.MitigatingStatusesByActionId`

`DefensiveValues.MitigatingStatusesByActionId` maps a defensive action to the statuses its effect text is tied
to, so a reader can tell whether that mitigation already stands. Additive.

A tank's `CanUse` now returns false for any action that carries a mitigation or barrier figure
(`DefensiveValues`), targets the player and is not part of the current tankbuster plan, while a plan for a measured
tankbuster runs or Hallowed Ground or Superbolide covers the coming hit (option "Spend only the mitigation a
measured tankbuster needs", on by default). This holds in every method of a derived rotation, not only the defence
methods; the heal paths stay free, inside `DefenseAreaAbility`/`DefenseAreaGCD` actions with an effect radius stay
free, and a commanded action is never held. `TankbusterTable.PredictedShare` and `MitigationFactor` are internal
and gained an optional horizon.

### Added to RotationSolver.Basic: `CustomRotation.Invulnerability`

`protected virtual IBaseAction? Invulnerability`, null by default; the four tank base rotations return
Hallowed Ground, Holmgang, Living Dead and Superbolide. The tank-swap rule reads it to know whether the
invulnerability could still save the player. A derived tank rotation built on its job's base class inherits
the right value; one that derives from `CustomRotation` directly can override it. `TargetType` gains
`TankSwap` at the end, so existing ordinals are unchanged.

### Removed upstream in RotationSolver.Basic: the cooldown window and the package

Both came with upstream's UI rework (merged into this fork on 04.10.2026); the fork passes them on
unchanged.

- **Removed** `IAction.IsOnCooldownWindow`, together with `ActionConfig.IsOnCooldownWindow`,
  `ItemConfig.IsOnCooldownWindow` and the implementations on `BaseAction` and `BaseItem`. The
  cooldown window they configured is gone. A derived rotation that reads or sets the property no
  longer compiles; one that implements `IAction` itself loses a member it no longer has to provide.
  Saved action settings that still carry the key load as before - the serializer ignores it.
- **No package any more.** `RotationSolver.Basic` is `IsPackable=false`, so no `.nupkg` is built
  and the pre-release label described above has nothing to label at present. Compile against
  `RotationSolver.Basic.dll` from the plugin's `latest.zip`.

### Renamed upstream in RotationSolver.Basic: `CanBeHealed`

Upstream 7.5.7.1 renamed `StatusHelper.NoNeedHealingInvuln(this IBattleChara)` to `CanBeHealed` and
`StatusHelper.PlayerNoNeedHealingInvuln()` to `PlayerCanBeHealed()`; the fork passes the rename on. The meaning
is unchanged - true while no invulnerability lasts beyond two GCDs, so also when none is up at all - and the new
name reads the way the method answers. A derived rotation that calls the old name no longer compiles; replace it
by the new one without inverting anything. A caller that negated the old name because it read "needs no healing"
was picking the protected members: read the call again rather than keep the negation.

### Behaviour change in RotationSolver.Basic: `DataCenter.DefaultGCDTotal` between GCDs

Between GCDs - out of range, between pulls, before the first GCD of a fight - the property returned 0, because
the game reports the GCD group's total only while its timer runs. It now returns the last length the game
reported, and before the first GCD the adjusted recast of a weaponskill. The signature is unchanged. Code that
took 0 to mean "no GCD is running" has to ask `DefaultGCDRemain` or the recast timer instead; code that divides
by it or measures windows in GCDs now gets a length where it used to get 0.

## 7.5.6.9+wsh1

### Behaviour change in RotationSolver.Basic: the raise dispatch

No signature changed, but a derived rotation that relies on the raise path sees different actions
come out of it.

- `CustomRotation.RaiseSpell` no longer hands **Swiftcast** back from the GCD path. It reports the
  raise itself and lets the ability path weave the instant in front of it. The old form picked
  Swiftcast only while `WeaponRemain <= 0.5f`, which is exactly the window
  `RSCommands_Actions.DoAction` refuses every ability in — and `WeaponRemain` is
  `DefaultGCDRemain`, so the two conditions excluded one another. The action reached the preview
  window and was never cast. A rotation that overrode nothing is fixed by this; one that reads the
  GCD path's output expecting an ability there will now see a GCD.
- The Swiftcast weave in `EmergencyAbility` matches against the job's own `Raise` instead of the
  four-id list `RaisePvE, EgeiroPvE, ResurrectionPvE, AscendPvE`. Red Mage's Verraise and Blue
  Mage's Angel Whisper were missing from that list and now weave like the rest. A derived rotation
  that sets `Raise` gets the weave automatically; one that raises with an action it does not
  declare as `Raise` loses it, and should declare it.
- The hard cast branches ask whether Swiftcast is still coming for the raise instead of whether it
  is in recovery. With `RaisePlayerBySwift` off, nothing ever spent Swiftcast on a raise, so it
  never entered recovery, so the hard cast branch never fired either and nothing was raised at all
  — although the setting only promises not to spend Swiftcast on raises.

Documented in `docs/rotation-flow/11-raise-dispatch.md`. `.github/scripts/audit/scan17.py` guards
the class in CI: it reads the ability window out of the execution gate and fails when the GCD path
picks an ability inside it.

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
