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
- **No charges lost when the boss dies.** A cooldown with charges that waits only so as not to waste them now goes when
  the boss, the last enemy in range, will fall before its charges are full and before your next burst. Not for heals,
  protection or movement. The trace writes "fight end: ... charges released".
- **Area attacks count only who lives to be hit.** An enemy that dies, by its estimated time to kill, before an area
  attack lands - its cast and the rest of the GCD - no longer counts toward the number of enemies the area needs. A
  Holy III that would have landed on two, because the third fell during the cast, becomes Glare.

- **Area attacks at a row of training dummies.** The dummies next to the one you hit counted as enemies not fighting
  yet, so every area that grazed them waited: at the dummies in Living Memory the samurai left Ogi Namikiri and Zanshin
  unused after Ikishoten and stood up to 28 s with auto-attacks only: with Zanshin ready but blocked, the samurai saved
  its Kenki up to 100, and every combo weaponskill refused itself against the overflow. Now the Kenki above Zanshin's
  cost is spent while Zanshin waits. A training dummy never fights and pulls nothing;
  areas hit past it as at a single dummy. When no weaponskill or spell comes for longer than a GCD although one could,
  the trace now writes "gcd idle: ..." with what was chosen and why each one was refused.

- **A pull without a countdown is prepared like one with it.** Attacking an enemy in the open world, in a dungeon or at a
  dummy, RSR now first uses what the rotation prepares at the end of a countdown, as long as it deals no damage and does
  not move you: the samurai opens with Meikyo Shisui and then Gekko instead of Hakaze, the machinist with Reassemble, the
  dancer with Standard Step. The first damaging weaponskill or spell follows. Not for the ninja. The trace writes
  "pull prepared: ...".

## Seeing what the enemies do

- **One right picture per enemy area (needs Splatoon).** Circles and lines an enemy casts without the game's orange
  marker are drawn through Splatoon at the place and in the direction the game keeps for the cast, until it lands -
  unless BossMod projects its running module into the world, or Splatoon's own projection already draws them. Circles
  you cannot leave before they land, such as raidwides, are not drawn. Cones and rings without a marker are left out,
  as nothing names their angle or inner circle. Setting: "Draw enemy areas without a ground marker through Splatoon", on
  by default.
- **Splatoon's wrong rings are blocked.** Splatoon's projection reads a ring's radii from the marker's name as yalms;
  the ring reaches as far as the action does. Where the two differ - Bedrock Uplift on the first Crucible board shows
  36-48 yalms at Splatoon for a ring at 18-24 - RSR adds the action to Splatoon's projection blacklist while the
  projection is on; the game's own marker stays. Actions in that blacklist are not drawn by RSR either.
