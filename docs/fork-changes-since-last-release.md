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

## Occult Crescent

- **Phantom heals heal ahead of incoming damage.** Occult Potion, Occult Elixir, Occult Chakra,
  Occult Resuscitation, Blessing and Phantom Judgment, and the Mega Potion of the Monster Hunter
  fights, compare the health a member is heading for with their thresholds, as `Heal ahead of
  incoming damage` states for every healing threshold. With that setting off nothing changes.
