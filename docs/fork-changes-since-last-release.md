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

## Defence against hits aimed at you

- **A big cast aimed at a healer or a damage dealer opens his defence.** Until now only a listed
  tankbuster, a marker or BossModReborn did. RSR now learns, across sessions, what each enemy cast
  aimed at a player who is no tank does to him, scaled back to unmitigated - also when an invisible
  helper casts it, one per player, as in many spread mechanics. When such a cast comes at you again
  and would take as much as a large shield or leave you under your single-heal line, your own
  single-target defence opens; aimed at another member, a healer's single defence opens and its help
  goes to that member instead of the tank the boss attacks. Small hits - most trash bolts - open
  nothing. The list window shows the table under "Aimed hits on non-tanks"; `DefenseTrace.log`
  names each measurement and each cast that opened a defence.
- **Long unlisted casts on a tank are learned too.** A cast at its own target, a tank, counts as a
  tankbuster once it reaches the window where RSR takes it for one; before, only casts seen first
  within that window were learned, so none longer than about two GCDs.

## After you are raised

- **The seconds of protection after a raise are used to heal up when a hit comes right after them.**
  After a raise you are invulnerable to most damage for 5 s, until you begin any action - a cast ends
  it as it starts. RSR used to press nothing in that time. Now, when an enemy cast at you or reaching
  you lands only after those seconds, or a party member is about to die, RSR heals and protects in
  them: heals, defences and an HP potion, chosen as usual; damage, buffs and raises still wait. It
  keeps waiting while any announced hit, a marker on you or a BossModReborn prediction could land
  within the protection. `Heal and protect yourself while the raise protection lasts`, **on by
  default**. Example: raised at 21 % with a 60 % hit cast at you that lands half a second after the
  protection ends, you get those 5 s to heal instead of none.
- **RSR comes back on when you are raised in a fight.** `Turn off when you die` switched RSR off and
  left it off after a raise. `Turn back on when you are raised in combat`, **on by default**: RSR
  comes back as it was - Auto, Manual or targeting only - once you stand up while the fight goes on.
  Not after you switched it yourself, changed zones or the fight ended. `DefenseTrace.log` notes the
  switch-off, the switch-on and every decision about the protection.

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
- **A gap closer distance of 0 now means "only onto a target you touch", not "never".** `Only use
  damaging gap closers within this distance` now includes the distance itself. Set to 0, a leap such
  as Shield Charge, Intervene or Stardiver goes onto an enemy you already stand against, where it moves
  you nowhere and only deals damage; before, no leap went at all.

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
- **Capture comes first and is sure to land.** A pact needs the enemy to fall marked, and the mark
  arrives about 0.6 s after Capture goes off. Capture now comes before every other weaponskill as soon
  as waiting one more could let the enemy fall before that mark arrives - judged by the largest drop in
  health it has taken within that time, counted from its full health, or by the largest drop any enemy
  in the area took in that time, killing blows included, or by its time to kill - with the landing
  measured from your own presses. Trash that falls within seconds is marked with the pull, on the enemy
  RSR attacks anyway. The mark comes at somewhat more HP than at the very last moment, which lowers the
  chance per attempt a little; a missed mark loses the attempt altogether. `DefenseTrace.log` gives the
  reason, when the mark arrived and at how much HP, whether RSR or you pressed it, whether the enemy fell
  marked and whether the pact came - judged once the enemy's body is gone, as the game enters a pact a
  moment after the fall. The time to kill is now counted from the moment it is read for every rule that
  uses it, as Six-sided Star's last-hit check.
- **A pack that comes at you is stopped first.** With no tank alive beside you, when a pack of two or
  more enemies is fighting and a familiar on a ready horn has a Tempered Release that stops them all
  (War Cry, Petribreath, The Ram's Voice), RSR calls that familiar first, before any danger and
  whatever your horn order says. Before, the order decided between equal familiars: the Raptor came
  first, spent its Release on damage, and when the pack had you in danger 1.5 s later War Cry was in
  the shared recast. With a tank, only enemies that target you count, as before.
- **The most dangerous enemy first when you fight alone.** As a Beastmaster or a Blue Mage with no
  tank and no healer alive in your party, RSR attacks first the enemy whose danger per second is
  largest against its HP, whatever your targeting setting: the damage it has dealt to you and your
  party in this fight per second, a hit it is casting at you with the size RSR has learned for it, and
  for an enemy that heals the others, the damage its heals let them deal. Until any enemy has hit,
  that is the enemy with the least HP. The enemy you attack stays your target until another is surely
  more urgent beyond the measuring error, so the target does not jump. Example: a guard hitting for
  30 HP a second goes down before six soldiers hitting for 5 each, though each soldier has less HP.
  With a tank or a healer alive your setting applies; a Blue Mage beside you counts as the tank or
  healer his Aetheric Mimicry makes him. Other jobs keep your setting. `DefenseTrace.log` notes each
  change of the lead.
- **Shield Charge on a pack, Seedsower at once.** Shield Charge hitting more than one enemy now goes
  before Trick and the axes, with every charge, instead of after everything else, where a short pull
  rarely left it a slot. It goes on enemies within melee range (3 y) whatever `Only use damaging gap
  closers within this distance` says, never farther, on the one whose reach covers the most of the pack;
  every other gap closer keeps that setting. Seedsower goes out as soon as Borrow grants it, as Beastskin does.
  `DefenseTrace.log` counts Shield Charge, Borrow and every Beast Mode action per fight, by RSR or by
  hand, with how long Shield Charge stood ready with several enemies in range, and names the reason
  when it stands ready and does not go.
- **A familiar that covers you stays (Crucible).** After Snarl - pressed by you or by `Use Snarl and
  Challenge …` - the familiar takes all damage meant for you for 45 s. RSR no longer sends it off with
  Parting Blow, or with a Tempered Release after which it leaves, to call the next horn while that cover
  lasts; the cover ends with its time, with Challenge or with the familiar's fall. The next horn comes
  up to 45 s later. `DefenseTrace.log` notes the hold once per cover.
