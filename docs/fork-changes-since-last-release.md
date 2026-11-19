# Changes since 7.5.8.7+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Crucible

- **The Crawling Piece first only while it binds you.** "Crucible of the Unbroken - Siren logic" put the Crawling Piece
  ahead of every enemy all the time. Its bind lasts until it falls, so it still comes first while it holds you; unbound,
  the most dangerous enemy comes first, as everywhere else. The trace writes "crucible siren: bound - ..." once per bind.

## Every job

- **Burst buffs no longer run into a downtime.** Where BossMod's module predicts a downtime and its end, an own damage
  buff waits for the boss to come back when the downtime would fall inside its window and waiting costs no use before
  the target dies. Alone it waits for any such downtime; in a group only when the downtime begins within one GCD, as long
  as RSR does not yet measure when the group's raid buffs come. The trace writes "downtime hold: ...".
- **Out of combat, a heal over time is all.** With "Heal party members out of combat" on, RSR healed between fights as
  in a fight, with cooldowns, lilies and charges. Out of combat your natural regeneration gives 10% of your HP every
  tick, so now only a heal over time cast as a GCD without its own cooldown goes (Regen, Aspected Benefic, Medica II and
  III); everything else waits for the fight. Not while someone carries damage over time or Doom, not in the Crucible.
