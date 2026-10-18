# Changes since 7.5.7.3+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Hits from a boss's helpers and parts are read

Many bosses hit through untargetable figures - helpers, or parts of the boss. RSR read only one kind of them, and in a
dungeon boss fight every hit on the party came from another kind: none was measured, its raidwides never joined the
AoE list, and a tankbuster marker before a hit of 89 % of your HP was learned as one that no hit follows. All enemy
kinds are read now, casts and hits alike. A marker learned wrongly this way is taken off that list by the next hit
after it.

