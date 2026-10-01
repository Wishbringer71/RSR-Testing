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
