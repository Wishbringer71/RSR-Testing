# Changes since 7.5.7.6+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Hits from a boss's invisible helpers are read

Most trials, raids and many dungeon bosses hit through invisible helpers. RSR did not read the kind of figure they are,
so none of their hits was measured: in a few evenings 127 of their actions went unread, up to 88 % of maximum HP on
eight party members, their raidwides never joined the AoE list, and tankbuster markers before their hits were learned
as markers no hit follows. They are read now, and the next hit takes such a marker off that list again.

## Healers and casters: an enemy's cast is stopped by the job's stun first

"Stop an enemy's cast with the job's stun or a sleep (experimental)", on by default, replaces the sleep-only setting.
It stops an area attack reaching the party, and now also a cast the game marks interruptible - what a melee stops with
Leg Sweep. A White Mage uses Holy when the caster stands in its radius and can still be stunned: it stuns every enemy
around and deals damage, where a sleep costs a GCD without. Otherwise Repose or Sleep goes on the enemy. With "Don't
attack new mobs by AoE." on, no area stop reaches an enemy that is not fighting yet.

What sleeps show is now kept over every session, like the AoE damage table: enemy kinds that resist a sleep are not
slept again, nor an enemy slept in a fight as often as its kind takes before it resists, nor an attack that went off
although the sleep landed in time. A sleep that landed too late makes the next ones land earlier instead of giving up
on the attack. The time from a sleep's cast to its effect is now measured from its cast bar. DefenseTrace.log records
every stop, every sleep and what came of it, and its first line shows what is stored.

