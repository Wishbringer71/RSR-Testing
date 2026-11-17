# Changes since 7.5.8.3+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Every job

- **The AoE settings hold for every area that hits enemies.** Assize, Macrocosmos and Lord of Crowns deal damage to all nearby enemies,
  but RSR counted them as friendly actions, so `AoE usage` set to Off did not stop them, and they could hit an enemy no
  area may touch, such as a sleeping one. Their effect text now marks them as attacks. `Don't pull new enemies with AoE`
  was never asked by areas around you (Overpower, Total Eclipse, Holy and every other), and a rotation that skipped the
  minimum enemy count could still pull. Now no area goes out while it would reach an enemy that is not fighting yet.

- **Only gazes are treated as gazes.** An enemy attack that only shares its name with a gaze - such as the Taurus's
  Mortal Ray in the Crucible, which hits everyone with Doom - no longer turns you away or holds your actions back. A
  gaze's own helper casts, which carry no mark of their own, still count.

- **Lead times before the first GCD follow your own speed.** While no GCD runs - before the first one of a pull, between
  pulls, out of range - RSR took the last GCD it had seen, which could be a shortened one such as a dance step or an
  Enshroud GCD, or for healers and casters a weaponskill's length without their spell speed. It now takes the game's
  length of an ordinary GCD of your kind, with gear, materia, food and haste in it, so windows counted in GCDs start on
  time from the first press.

## Deep dungeons

- **Aggro control of its own.** Three options under Deep dungeon, on by default, leave every other content as it was -
  pull with areas and heal as you like there. In a deep dungeon they decide instead of "Don't pull new enemies with AoE":
  - *Heals that also deal damage don't pull new enemies:* Assize, Macrocosmos and Pneuma wait while they would hit an
    enemy that is not fighting yet; the heal alone pulls nobody, its damage does. While a party member is about to die,
    the heal goes all the same.
  - *Damage doesn't pull new enemies:* no area of yours reaches a resting enemy - the caster's Sleep included, which pulls
    as damage does - and during a fight RSR picks no resting enemy as a new target, so no add joins. The enemy you pull
    yourself stays your target.
  - *Put further enemies that attack your party to sleep:* a healer or caster whose party two or more enemies attack
    sleeps the ones nobody fights - not your target, not a tank's - with Repose or Sleep, not one about to die or taking
    damage over time, and renews a sleep just before it runs out while an awake enemy is fought. The caster's Sleep may put
    a fought enemy to sleep along; it stays the target and the next attack wakes it. Every other sleeping enemy, whoever put
    it to sleep, is spared by your areas and attacked only when no awake enemy is left - the tank takes the next sleeper
    once the other attackers are dead. A kind that resists is learned and not tried again. A caster's Sleep, which reaches every enemy within
    5 yalms of its target, goes on the enemy whose area puts the most attackers to sleep - in a pack, the one in the middle -
    and, among equal choices, on the one whose area cuts short the fewest sleeps already running.
- **Resting enemies seen by every area.** "Don't pull new enemies with AoE" looked for resting enemies only among those
  RSR would attack; with an engage setting that skips them, an area hit them anyway. Every enemy in the area counts now.

## Beastmaster in the Crucible

- **Start a board from RSR's team window.** With the team as recommended, "Start" presses the board choice's start
  button as your click does and commences the duty confirmation. If the start does not reach the board, the button is
  not offered again that session.
- **Protective items go where the run planned them.** A Tome of the Impervious bought for the heaviest fight stayed in the
  bag to the end: outside the final fight it went only against a hit announced on you. With no item bonus left to keep,
  each protective item you hold now belongs to the final fight or to one of the heaviest fights still ahead, and there it
  goes as soon as an enemy attacks you.
- **The shop keeps tokens for Rolling in Riches.** The shop never weighed the 500 points for ending a board with at least
  2,000 tokens. While you carry the way to the final fight, it now keeps as many tokens as you need to end with 2,000,
  counting what the fights still ahead earn, and buys only from what is above that. Cover against a status that kills
  still comes from all your tokens.
- **Attack stats are weighed.** Damage Dealt, Physical and Magic Damage Dealt, Critical Hit Rate, Haste and Slow now
  give a piece its pace: how much faster the team deals damage, from your share of it and what a critical hit adds,
  both measured from your own hits. The shop and treasure window names that pace for every piece. On the safety scale
  it counts for what it provably spares you: enemy auto-attacks come at a fixed delay, so a faster team takes fewer of
  them. On board 2 they made up more than half of the damage you took, so an attack piece now weighs about like the
  matching cut in damage taken.
- **Feed counts its haste and critical hits.** A feed's Damage Dealt, Physical and Magic Damage Dealt, Critical Hit
  Rate and Haste now make up how much faster its familiar fights - in the horn choice, in the feed the shop recommends,
  and in the team's pace. Physical Damage Dealt on a feed no longer reads as damage of every kind.
- **Protective items are planned along one way.** A fight is planned for a protective item when the way ahead with the
  most heavier fights holds fewer of them than you carry items, not when all branches together do.

## Stability

- **No crash at a figure that left the game.** RSR held the caster of a gaze until the gaze resolved and then asked it
  whether it was still casting; once the figure was gone, the game closed (seen in the Crucible). Figures are now held
  by their id and read from the game's object table where they are used, and every figure kept from an earlier frame –
  in the party and enemy lists, the tankbuster plan, a tank swap – is dropped once it is gone, before anything reads it.
  Whether a figure is gone is read from the object table alone, never from the figure, so logging out is safe as well.
- **What RSR learned is saved even when another program holds the file for a moment.** Saving a learned list was given
  up when the file was briefly locked, such as by a virus scanner; a kind learned immune to sleep was then forgotten and tried
  again on the next run. The save is now retried.
- **An unreadable settings file is kept as `RotationSolver_LoadFailed.json`.** Before the defaults replace a settings
  file that cannot be read, a copy is kept next to it - by upstream's backup now, which also checks that the file and its
  folder exist; it was `RotationSolver_Unreadable.json` before.
