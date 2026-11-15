# Changes since 7.5.8.3+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Every job

- **The AoE settings hold for every area that hits enemies.** Assize and Macrocosmos deal damage to all nearby enemies,
  but RSR counted them as friendly actions, so `AoE usage` set to Off did not stop them, and they could hit an enemy no
  area may touch, such as a sleeping one. Their effect text now marks them as attacks. `Don't pull new enemies with AoE`
  was never asked by areas around you (Overpower, Total Eclipse, Holy and every other), and a rotation that skipped the
  minimum enemy count could still pull. Now no area goes out while it would reach an enemy that is not fighting yet.

## Stability

- **No crash at a figure that left the game.** RSR held the caster of a gaze until the gaze resolved and then asked it
  whether it was still casting; once the figure was gone, the game closed (seen in the Crucible). Figures are now held
  by their id and read from the game's object table where they are used, and every figure kept from an earlier frame –
  in the party and enemy lists, the tankbuster plan, a tank swap – is dropped once it is gone, before anything reads it.
