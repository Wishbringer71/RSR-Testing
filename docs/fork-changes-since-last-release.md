# Changes since 7.5.7.3+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Hits from a boss's helpers and parts are read

Many bosses hit through untargetable figures - helpers, or parts of the boss. RSR read only one kind of them, and in a
dungeon boss fight every hit on the party came from another kind: none was measured, its raidwides never joined the
AoE list, and a tankbuster marker before a hit of 89 % of your HP was learned as one that no hit follows. All enemy
kinds are read now, casts and hits alike. A marker learned wrongly this way is taken off that list by the next hit
after it.


## Help for the one a tankbuster is aimed at

When a tankbuster marker sits on another party member - most often the other tank - or an enemy casts a known
tankbuster at them, every tank now puts its help for others on them before the hit: Intervention (Paladin), Nascent
Flash (Warrior), The Blackest Night (Dark Knight), Heart of Corundum (Gunbreaker). Healers put their single-target
mitigation on the same member instead of whoever the boss is attacking. Before, Heart of Corundum and Aurora set to
"Only use on tankbuster targets prioritizing self" never reached the other tank, and reached you only with a marker;
Intervention went to whoever the boss was attacking, also on a pull, and only to a tank in stance. New options for the
Warrior and the Dark Knight, and "Use Intervention on CoTank during tankbusters", are on by default. Help is not given
to a member who takes the hit under Hallowed Ground or Superbolide while "Hold mitigation under invulnerability" is on.

## Sleep against an enemy's area attack (experimental)

Healers and casters have no interrupt. With "Use Sleep or Repose to stop an enemy's area attack (experimental)", on
by default, they put Repose or Sleep on an enemy that is not a boss while it casts an area attack reaching you or two
party members, if enough of the cast is left for the sleep to land. No game text says that a sleep interrupts;
community sources say it does, and the sleep status locks actions as a stun does. Each attempt costs one GCD. An enemy
kind that resists, and an attack that goes off although the sleep landed, are not tried again in that session.
Swiftcast is never spent on it. DefenseTrace.log records every attempt and whether the attack stopped.
