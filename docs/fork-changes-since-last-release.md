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

## Healing

- **Figures that only healing defeats are healed.** Some fights bring in friendly figures that die
  only when healed to full with single-target heals, such as the four Reiryu in Shinryu (Unreal).
  They appear at 1 HP, tether a player each and fly off at the end of their cast unless healed in
  time. RSR now recognises such a figure: it appears at 1 HP, or it tethers a party member. A healer
  then heals it with pure single-target heals, after any member about to die and before everyone
  else. The figure nearest full goes first, or the one tethering a member who is about to die.
  Party values, area heals and mitigation never count these figures. A Red Mage, Paladin or
  Summoner helps with Vercure, Clemency or Physick only when the healing so far would not finish
  them before their cast ends: `Help heal figures only healing defeats as a non-healer`, **on by
  default**.
- **No heals on players whom healing cannot reach.** The Worm's Curse (the Reiryu's tether) and
  Primordial Crust nullify all healing, as Mounted does. Their bearers are no longer single-heal
  targets and no longer raise the heal flags.

## Leaps and dashes

- **Blue Mage and Phantom leaps keep to the gap-closer distance.** J Kick and Flying Frenzy (Blue
  Mage) and Phantom Kick (Phantom Monk) leap onto the target. They now follow `Only use damaging gap
  closers within this distance` like every other damaging gap closer: from farther away they go out
  only on a move-forward command, in the countdown, or when you press them yourself. With
  `Check movement actions with BossModReborn (Experimental)` on, their path is checked too. Occult Featherfoot
  (15 yalms forward) has its landing point checked like En Avant.
- **Moves to a spot on the ground go where you look, and your own press stays yours.** Loom (Blue
  Mage), Shukuchi (Ninja) and Cloud Skim (Beastmaster) were placed like a ground heal or an area
  attack: on a move-forward command Loom went to the party or under the boss, Shukuchi onto enemies.
  They now go ahead - the full range with `Move as far as possible with ground-targeted movement
  actions`, otherwise to the farthest character in the movement cone - with their path checked, and
  RSR no longer takes over a press of yours, so the spot you place is kept. On a move-forward
  command, leaps such as Intervene or J Kick now reach the farthest target in that cone within their
  range instead of only one within 3 yalms, as `Only use damaging gap closers within this distance`
  states.

## Occult Crescent

- **Phantom heals heal ahead of incoming damage.** Occult Potion, Occult Elixir, Occult Chakra,
  Occult Resuscitation, Blessing and Phantom Judgment, and the Mega Potion of the Monster Hunter
  fights, compare the health a member is heading for with their thresholds, as `Heal ahead of
  incoming damage` states for every healing threshold. With that setting off nothing changes.

## Beastmaster

- **Familiars by the situation now work.** The familiar catalog was never built: RSR looked for the
  link from a familiar to its actions in the wrong two columns of the game data. Every rule that picks
  a horn by the situation - a healing or protecting familiar when you are in danger, the area familiar
  for the long stint on a pack, Trick by affinity, holding back a familiar whose Release would leave -
  now has the data it reads. Before, only your horn order counted.
- **Battlehorns set for the next fight.** `Set the battlehorns between fights for the next one`, **on
  by default**: out of combat - the server takes no horn change in a fight, measured on 04.11.2026 -
  RSR puts on your horns the familiars you have that suit what comes next. Before a pack the most
  damage against it, area Tricks and Tempered Releases counted by the enemies they reach; before a boss
  (the nearest enemy you have not fought yet carries a boss icon) two familiars that heal or protect
  you, one whose Trick heals among them; after a danger in the area one more, before every fight
  there. The familiar out keeps its horn, horns you changed yourself stay until the next fight ends,
  and nothing changes while the Bestiary is open or in the Crucible. `DefenseTrace.log` names every
  plan and whether the server took it.
- **Capture marks in time.** The mark arrives about 0.6 s after Capture goes off, and a pact needs the
  enemy to fall marked; RSR also read the enemy's time to kill up to a second too long. Capture now goes
  on the last weaponskill whose mark still arrives before the enemy falls, with the landing measured
  from your own presses. `DefenseTrace.log` says when the mark arrived, whether the enemy fell marked
  and whether the pact came. The time to kill is now counted from the moment it is read for every rule
  that uses it, as Six-sided Star's last-hit check.
- **Shield Charge on a pack, Seedsower at once.** Shield Charge hitting more than one enemy now goes
  before Trick and the axes instead of after everything else, where a short pull rarely left it a
  slot. Seedsower goes out as soon as Borrow grants it, as Beastskin does. `DefenseTrace.log` counts
  Shield Charge, Borrow and every Beast Mode action per fight, with how long Shield Charge stood ready
  with several enemies in range.
