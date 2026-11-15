# Changes since 7.5.8.3+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Every job

- **The AoE settings hold for every area that hits enemies.** Assize and Macrocosmos deal damage to all nearby enemies,
  but RSR counted them as friendly actions, so `AoE usage` set to Off did not stop them, and they could hit an enemy no
  area may touch, such as a sleeping one. Their effect text now marks them as attacks. `Don't pull new enemies with AoE`
  was never asked by areas around you (Overpower, Total Eclipse, Holy and every other), and a rotation that skipped the
  minimum enemy count could still pull. Now no area goes out while it would reach an enemy that is not fighting yet.

## Beastmaster in the Crucible

- **Protective items go where the run planned them.** A Tome of the Impervious bought for the heaviest fight stayed in the
  bag to the end: outside the final fight it went only against a hit announced on you. With no item bonus left to keep,
  each protective item you hold now belongs to the final fight or to one of the heaviest fights still ahead, and there it
  goes as soon as an enemy attacks you.
- **The shop keeps tokens for Rolling in Riches.** The shop never weighed the 500 points for ending a board with at least
  2,000 tokens. While you carry the way to the final fight, it now keeps as many tokens as you need to end with 2,000,
  counting what the fights still ahead earn, and buys only from what is above that. Cover against a status that kills
  still comes from all your tokens.
- **Attack stats count.** Shop and treasure weighed only what keeps damage off you; Damage Dealt, Physical and Magic
  Damage Dealt, Critical Hit Rate, Haste and Slow were named but worth nothing. Haste speeds your weaponskills and
  auto-attacks, and you deal about two thirds of your team's damage, so these pieces end fights sooner, and a shorter
  fight costs you less. They now count on the same scale, through the pace they give the team: your share of its damage
  and what a critical hit adds are measured from your own hits, and pieces that also apply to the summoned familiar
  speed it too. The same pace lowers each fight's load in the way's test and at the campsite.

## Stability

- **No crash at a figure that left the game.** RSR held the caster of a gaze until the gaze resolved and then asked it
  whether it was still casting; once the figure was gone, the game closed (seen in the Crucible). Figures are now held
  by their id and read from the game's object table where they are used, and every figure kept from an earlier frame –
  in the party and enemy lists, the tankbuster plan, a tank swap – is dropped once it is gone, before anything reads it.
