# Changes since 7.5.7.14+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Summoner and other pet jobs

- **A shield that still stands is not cast again after a summon.** Radiant Aegis is put on by
  Carbuncle, and every summon, a demi or a primal, replaces Carbuncle with a new one. The shield then
  no longer counted as the player's own, so the next area or single-target defence spent the second
  charge on a shield with 12 to 15 seconds left, often right before the raidwide it was cast for.
  A status now counts as the player's while its source is any object the player has owned in this
  area. This holds for every job whose statuses come from a pet or summon.
