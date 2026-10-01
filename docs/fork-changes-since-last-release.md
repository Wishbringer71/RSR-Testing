# Changes since 7.5.6.13+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## White Mage: the second Holy waits for the first stun to end

`Stretch the Holy stun` (on by default) now does what it says on every pull: right after Holy, the
next GCD goes to another spell while Holy's stun would still be running when a second Holy lands, so
the second stun starts where the first ends instead of overwriting it. Four conditions used to let
the second Holy go out at once: no DoT left to place, one enemy in the radius not yet stunned, fewer
enemies than `Enemies in Holy's radius before holding`, and - the likeliest - a stun that had not yet
reached the enemies when the next GCD was chosen, since Holy's cast and recast are equally long.

The held GCD goes to a DoT where one is due, otherwise to Glare: one GCD of Holy damage per pull for
about 7 instead of 5.5 seconds of stun. `Enemies in Holy's radius before holding` now only applies to
holding Holy for The Blackest Night and sits under that setting.

## `DefenseTrace.log` keeps every session and names its build

The trace is no longer replaced when the plugin loads, so a rebuild or reload keeps the sessions
before it. Each session starts with its date and time and the commit the plugin was built from, so
every section can be matched to the exact code that wrote it. Delete the file to start over.

## Own cooldowns stay for hits that reach you

A tank's single-target defence opened for every tankbuster BossModReborn announced and for every cast an
enemy aimed at its own target, whoever that was - so Damnation and Rampart went out for a buster on the
other tank that BossModReborn had marked as not hitting you. Defences that protect only you - Rampart,
Damnation, Raw Intuition, Arm's Length, a barrier on yourself - now go out only when the hit that opened
the defence reaches you, in the single-target and the area defence alike and for every job. Help for others
stays: Reprisal, Shake It Off, Intervention and Heart of Corundum on the tankbuster's target are not held
back. An announced tankbuster whose targets BossModReborn does not name still counts as hitting a tank.

`DefenseTrace.log` now also names the pull rule (enemies on you) and unlisted casts at a tank's target as
sources, says whether the single hit reaches you, and records each landing of a listed area cast with
whether it reached you.
