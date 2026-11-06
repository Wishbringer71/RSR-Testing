# Changes since 7.5.7.14+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## When the pack is about to fall

- **Time to kill now runs until the last enemy falls.** An action's time-to-kill threshold, and every rule
  that asks how long the fight lasts (the Ninja's Mug, Dokumori and Doton, Flamethrower, a tank's invulnerability, the
  heal lock at the end of a fight), read the average of the enemies' times to kill. An enemy nobody had hit yet dropped out of
  that average - hit one of four and its 3 seconds counted as the fight's - and a boss beside a few adds averaged
  with them. It now reads the HP of every enemy in combat over their common fall per second: the time until there
  is nothing left to hit.
- **The Machinist's queen and the falling pack.** Queen Overdrive (Rook Overdrive) now goes in the last GCD before
  the pack falls when the queen would outlive it - her automatic finisher otherwise hits nothing - and the
  action's 16-second lock no longer refuses it then. RSR measures whether your Battery stays from one fight to the
  next in the same area; once it does, the queen keeps her time-to-kill lock against a pack about to fall and
  waits out a predicted pause, so her Battery pays in full in the next fight. Until then she comes as before.

## Summoner and other pet jobs

- **A shield that still stands is not cast again after a summon.** Radiant Aegis is put on by
  Carbuncle, and every summon, a demi or a primal, replaces Carbuncle with a new one. The shield then
  no longer counted as the player's own, so the next area or single-target defence spent the second
  charge on a shield with 12 to 15 seconds left, often right before the raidwide it was cast for.
  A status now counts as the player's while its source is any object the player has owned in this
  area. This holds for every job whose statuses come from a pet or summon.

## Healing

- **The game decides whether a party member is in sight.** Whether a heal, a buff or a raise reaches a
  member behind a pillar is now the game's own answer, the check the hotbar uses to mark an action out of
  range. RSR's own ray, with one eye height for every race and enemy size, decides only where the game
  gives no clear answer, such as a member out of range. Before, a heal the ray allowed and the game refused
  was chosen again every frame while the member stood hidden, and a member the ray missed got no heal at
  all. Where the two disagree, `DefenseTrace.log` writes a line.
- **A status that turns worse when it runs out is cleansed ahead of the heals.** A cleansable status
  whose text says what follows when it expires or stays (Freezing turns into a Deep Freeze, Half-asleep
  into Sleep, an Aetherochemical Bomb detonates) is now cleansed before every heal while no party member
  is about to die, and at once when it runs out within a GCD. Until now it waited behind every heal like
  any other cleanse.
- **Healed to full what only full HP ends.** A status whose text says it dissipates once fully
  healed is healed away as if its bearer stood at 1 %: until now only two Doom variants were listed;
  Gradual Petrification and Accretion are now healed to full as well.
- **Kardia follows the tank who holds the aggro.** A sage put Kardion on the first tank in stance by
  job order and never moved it, so after a tank swap it healed the tank who no longer took damage.
  Kardion now goes to the tank the most dangerous enemies target - by their measured damage per
  second, before that by their maximum HP - and moves once another tank has held that aggro for a
  GCD. The job order of `Kardia tank priority` decides only at a tie or when no enemy targets a tank.
- **A cleanse no longer comes before a heal unless it saves a life.** A healer's Esuna went before
  every heal for anything on the dispellable list - Slow and Bleeding among them -, one member per GCD
  and 400 MP each, even while a member was about to die; against Odin that was 22 casts and 8,800 MP,
  and with the MP gone there were no heals and no damage spells left. Now only a status that kills when
  it runs out (Doom, Necrosis, Throttle) is cleansed ahead of healing. Everything else waits until no
  heal or defence is needed, and keeps the MP a raise needs (`Keep the MP a raise needs` now says so).
  A damage over time on more than one member is left to the job's heal over time for the party while
  an enemy is in reach. The bard's Warden's Paean is unchanged.
- **A blue mage counts in the role of his Aetheric Mimicry.** Every question RSR asks about a party
  member's role (`IsJobCategory`) now answers a blue mage under "Aetheric Mimicry: Healer" as a healer
  and one under "Aetheric Mimicry: Tank" as a tank. With such a healer in the party, a tank or damage
  dealer no longer spends his defence on every attack as if no healer lived, and heals wait for him as
  for any healer; a blue mage tank counts as the party's tank.
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
- **No area heals spent on members whom healing barely reaches.** A status that lowers HP recovery
  (HP Recovery Down, Infirmity, Healing Magic Down and the like - every status whose text says so)
  is now measured on your own heals: how much reached its bearer against a member without one in the
  same cast. Area heals then count a bearer only with the part of his missing HP that healing can
  still close. In Windurst, after a mechanic put three members on 10 % under HP Recovery Down,
  RSR cast five area heals that brought them 1 to 2 % each; now the first one measures, and the next
  go into damage. Single heals, emergency heals and members about to die are unchanged: a member
  whom healing reaches less needs more of it.
- **A single heal is broken off only for healing by someone else.** With `Stop single-target GCD heals
  once the target is above the threshold (Experimental)` on, a heal used to stop as soon as its target
  rose above the threshold during the cast, whatever lifted him: the 1 % of maximum HP that comes back
  on its own with every server tick in combat, your own Regen, or your own heal once it had already
  gone off. Now it stops only when healing by someone else lands on the target during the cast and
  puts him above the threshold, as the setting says: another healer, an NPC, the target's own
  self-heal such as Storm's Path or Bloodbath, or a heal over time someone else put on him. A cast
  whose own effect has already arrived is no longer judged - the game has resolved it and ignores a
  cancel. `DefenseTrace.log` names each stop with the cast time it cost, and once per cast a heal
  that runs on although its target rose above the threshold without help.
- **The self-heal of an attack counts on you.** Bloodbath, Storm's Path, Abyssal Drain and the like
  were read as healing on the enemy hit; RSR now counts them on you while they are on their way, as
  it does with its own heals.
- **No heals on players whom healing cannot reach.** The Worm's Curse (the Reiryu's tether) and
  Primordial Crust nullify all healing, as Mounted does. Their bearers are no longer single-heal
  targets and no longer raise the heal flags.

## Defence against hits aimed at you

- **The first raidwide and tankbuster of a fight are known before they hit.** RSR put an area cast on
  its list only after it had hit every member of a party of four or more, and learned a tankbuster
  without a marker only after its first cast at the tank; until then only a BossModReborn
  prediction opened the defence. The shipped lists now also hold the casts cactbot announces as a
  raidwide (688) or a tankbuster (401), so their first appearance opens mitigation, the heal ahead
  and the tank's own defence where no module predicts them - Occult Crescent's field fights among
  them, where RSR never listed a cast because the owner plays them without a party.

- **The invulnerability against a hit already on its way.** The game reports a hit some tenths of
  a second before it takes the HP (0.58 s in the median in the owner's logs). Once the damage
  reported on a tank and not yet taken reaches his HP and barrier, Holmgang, Living Dead, Hallowed
  Ground or Superbolide goes out at once, ahead of every other emergency action, instead of waiting
  for an HP under the threshold that never shows. Option `Use the invulnerability when damage
  already reported would kill you`, on by default. Heals on their way are not counted.
- **A boss behind an invulnerability still counts when it casts.** A targetable enemy carrying a status
  of the invincible list (Storm Wall, Invulnerable and the like) is no target for attacks, and its
  casts went unseen as well: a listed raidwide or tankbuster from it opened no defence and no heal
  ahead. Its listed casts now count like any other enemy's; it is still not attacked.
- **Enemies with spikes are left alone.** An enemy whose status returns damage to its attackers (Blaze,
  Ice, Shock or Paralyzing Spikes, by the status text) is no target of an attack while the spikes stand,
  for every job - until now only two such statuses were listed, and the Crucible's snoll piece with Ice
  Spikes was still hit. An action that removes the spikes may still target the enemy: Quelling Wave now
  reaches the drake piece with Blaze Spikes, which it never found before. Counters and reflections count
  too, by the kind of attack they turn back: one that counters physical attacks (the barbmole piece's
  Needles Out, Counter) spares a disciple of war - the beastmaster among them - and leaves a mage free to
  attack; magic reflections spare a mage, ranged ones the ranged jobs and healers. Quelling Wave, a spell,
  may also strip Needles Out. `Do not attack enemies whose spikes damage their attackers`, **on by
  default**; `DefenseTrace.log` names each enemy left alone and which attacks it turns back.
- **An enemy the next hit kills comes first.** A petrified enemy whose status says that taking damage
  means instant death (Stone Curse - the cyclops piece under Medusa's gaze) is taken off the field with
  one hit before it recovers, for every job.
- **Enemies that must fall together, or after another, are held back.** A pair whose survivor enrages
  or raises the other when one falls (the tablitaur pair, the lamias) is brought down evenly: each is
  held at a reserve - your measured damage over half the survivor's cast - until all are there, so the
  last falls before the enrage or raise lands. A morpho piece is not killed while an abaddon piece
  lives (its death confuses you), the abaddon not while a morpho lives, and a myconid piece only in the
  mindflayer's water puddles, where its death spills no spores. Held enemies are no target, and no area
  attack catches one whose fall would harm. `Kill rules of known fights`, **on by default**;
  `DefenseTrace.log` notes each hold and release.
- **Area attacks spare what may not be hit.** An area attack no longer catches an enemy RSR must not
  attack: one with spikes, one with a stop marker while `Never attack targets with stop markers` is on
  (until now only the target was spared - the area still woke a sleeping enemy), and in the Crucible a
  zu egg while the zu piece lives. The area finds another centre or waits, by its shape - circle, cone,
  line or ring - around you, a target or a spot on the ground. The zu eggs are no target either:
  every egg broken before it hatches makes the Zu stronger. `Crucible of the Unbroken - Zu logic`,
  **on by default**.
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
- **A bard's, machinist's or dancer's party mitigation is not cast over another one.** Troubadour, Tactician
  and Shield Samba cannot stack, and the game replaces a standing one with the new one whoever cast it. Ahead of a
  raidwide or tankbuster that BossMod announces, RSR now casts yours only when none of the three still covers the hit.
  Before, with a second of these jobs in the party, yours went out over theirs as soon as your own had run out: the
  hit was covered anyway, and the next raidwide inside the 90 to 120 seconds of the cooldown came without it. With
  `Restrict mitigations to not overlap`, the machinist's Dismantle now also waits while another player's Troubadour
  or Shield Samba stands, not only your own Tactician.
- **Deployment Tactics only with someone to receive the shield.** A scholar's Deployment Tactics spreads the
  target's Galvanize to the party members within its radius; it went out on the member with the largest shield even
  when nobody stood within that radius, and its 90-second cooldown was missing at the next raidwide. It now waits until
  another party member is in reach.

## Raising

- **Red Mage raises with no enemy around.** Verraise needs Dualcast, which in a fight the next Jolt
  gives. Between two packs or while the boss cannot be targeted no spell gave it, and without
  Swiftcast the raise waited up to Swiftcast's whole recast. RSR now casts Vercure on you there (no
  enemy needed, also at full HP) and Verraise with the Dualcast it brings - where `Hard-cast raise`
  allows a hard cast, and standing.
- **No job raise where only the duty's own raises work.** In the Forked Tower, Delubrum Reginae
  (Savage) and the Baldesion Arsenal everyone carries Resurrection Restricted, and only the duty's
  raises (Revive, Occult Raise, Lost Arise, Raise L) or a healer limit break bring the dead back.
  RSR still picked the dead for Raise, Resurrection, Ascend, Egeiro, Verraise, Angel Whisper and the
  Phoenix Down, spending Swiftcast and casts on raises that bring nobody back while the next GCD
  waited. Those now leave such a body to the duty's own raise, and the mana kept back for the job's
  raise is free for heals and damage there. `DefenseTrace.log` notes each such body and when the
  rule starts and ends.

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

## Interrupts, stuns and provoke

- **Each interrupt and provoke picks an enemy it can reach.** RSR used to choose one caster per moment
  from every enemy within 48 yalms, in no particular order, and only then check whether Interject, Head
  Graze, Leg Sweep or a Phantom interrupt could reach it. When that one stood out of range - or was an
  enemy your `Engage settings` leave alone - nothing was interrupted, even with a second caster right
  in front of you. Now every enemy worth interrupting is a candidate, and each action takes the first
  one in its own range. The same for Provoke: an enemy on your healer 10 yalms away is taken even while
  another one 30 yalms away attacks a mage. `Interrupt delay range` and `Provoke delay range` work as
  before.
- **No interrupt, stun or sleep on an enemy that is not fighting.** An enemy that has not been pulled -
  in the combat state, with a target, or casting at someone - and casts for itself is left alone:
  stopping it protects nobody and pulls it, which in a deep dungeon brings a second pack. This applies
  to every interrupt, to Low Blow on a cast that cannot be interrupted, and to the healer and caster
  stop of `Stop an enemy's cast with the job's stun or a sleep (experimental)`. An area attack that
  reaches you or two party members is still stopped, whoever casts it.

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

## Black Mage

- **Xenoglossy on the move.** When no spell with a cast time can start - you move without the
  movement lock, BossMod's AI moves you, a knockback runs - and Triplecast and Paradox are spent, the
  Black Mage now uses Xenoglossy (or Foul where it hits harder) and, in Astral Fire, a Firestarter
  Fire III before Scathe. Polyglot was otherwise held until it overflowed while Scathe took the GCD for
  800 MP; the Powerdox rotation used nothing.
- **Retrace on by default.** `Use Retrace when out of Leylines in combat and standing still` is now on
  in both Black Mage rotations: once you stand outside your Ley Lines in combat, Retrace brings the
  circle back under you while it still lasts beyond the next GCD, never right after placing it.

## Melee damage dealers

- **No ranged attack while closing in.** A Samurai, Dragoon, Ninja, Reaper or Viper running at its
  target no longer throws its ranged attack when the first melee hit is only a moment away: it waits
  while reaching melee range takes less than a GCD times one minus the ranged attack's potency over
  the combo starter's, by the tooltips. Standing, stepping aside or with Enhanced Enpi or Enhanced
  Piercing Talon up, the ranged attack goes as before; tanks keep theirs for the pull.
- **Kunai's Bane spares a pack about to fall.** It now carries Trick Attack's time-to-kill threshold,
  which it replaces from level 92 with the same 60 s recast and the same debuff; it had gone onto every
  pack, also one about to fall.

## Pictomancer

- **Striking Muse no longer waits with full charges.** It still waits for Starry Muse, but only while its
  two charges would not both stand full before it; then one goes now and the other is back for Starry
  Muse. With the burst off RSR never presses Starry Muse, and Striking Muse had waited for it for the
  rest of the fight.

## Astrologian and Sage

- **Combust and Eukrasian Dosis on the move.** When no spell with a cast time can start and Lightspeed,
  Toxikon or Phlegma are not there, the Astrologian puts Combust and the Sage Eukrasian Dosis on again
  before they are due, instead of leaving the GCD empty - while at most half their duration is left
  (each MP then buys at least as much as a Malefic or a Dosis), on a target that outlives it, and not
  while `Use GCD heals when there's nothing else to do` would heal.

## Deep dungeons and Eureka

- **The regeneration potion of each deep dungeon and of Eureka is used.** Sustaining, Empyrean, Orthos,
  Pilgrim's and Eurekan Potion grant Rehabilitation and share no recast with ordinary potions. RSR now
  drinks the one of the content you are in below `Use HP potions below this HP` while no Rehabilitation
  runs, next to an ordinary potion rather than instead of it: an ordinary one first, the regeneration in
  the next window. Before, Heaven-on-High and Eureka Orthos had none, and in the Palace of the Dead and
  Pilgrim's Traverse any ordinary potion in the bag won over it, so it was hardly ever drunk.
- **Transformed in a deep dungeon, RSR attacks with the form.** As Manticore, Succubus, Kuribu,
  Dreadnaught or Bomb your job's actions are locked, and RSR used to do nothing until the form ended.
  It now uses the form's attack the game allows - Pummel, Void Fire II, Heavenly Judge, Wrecking Ball
  where enough enemies stand or Rotosmash, Big Burst - through the duty rotation "Deep Dungeon"
  (`Use duty actions`). Rocky Roll, the Mudball's 25-yalm dash, goes only on a move-forward command.
  Each transformation writes a line to `DefenseTrace.log` naming the attacks the game allowed.
- **Alone in a deep dungeon, RSR uses the pomanders that keep you alive.** New option `Use the deep
  dungeon's pomanders against bosses and in danger when alone`, on by default, for every job: Steel,
  Strength and Haste against a boss - before the pull once it stands in sight, so their lock costs no
  attack in the fight - and, when you are about to die, Raising while none is active, then Witching on
  an enemy that attacks you within its 20 yalms, once until you have left that danger. Never in a party,
  since the stock is shared. Before, Steel and Raising stayed in the stock while the boss hit and the run
  ended. Each use and each refusal writes a line to `DefenseTrace.log`.
- **Alone in a deep dungeon, RSR uses magicite, demiclones and incense.** New option `Use the deep
  dungeon's magicite, demiclones and incense against bosses and in danger when alone`, on by default,
  after the pomanders: when you are about to die, Inferno, Crag or Vortex Magicite - or Poisonfruit
  Incense off a boss floor - so the floor's enemies fall and you stand invulnerable for 10 s; against a
  boss in combat Elder Magicite or a demiclone (Unei, then Onion Knight, then Doga); Barkbalm Incense
  before a boss pull, so you enter with double HP. Mazeroot stays with you. Which stone a slot holds is
  read from the dungeon's status data, else from the game sheets; once per floor `DefenseTrace.log` notes
  what each slot was read as.

## Occult Crescent

- **Phantom heals heal ahead of incoming damage.** Occult Potion, Occult Elixir, Occult Chakra,
  Occult Resuscitation, Blessing and Phantom Judgment, and the Mega Potion of the Monster Hunter
  fights, compare the health a member is heading for with their thresholds, as `Heal ahead of
  incoming damage` states for every healing threshold. With that setting off nothing changes.

## Blue Mage

- **A set per Masked Carnivale stage, on your click.** With the Carnivale briefing open as a Blue Mage,
  the window `RSR - Masked Carnivale set` shows the spells Blue Academy's guide (mage.blue) names as
  required and recommended for the chosen stage - the first one you have learned of each point, for a
  filler example you lack a learned spell of the same element - and which are missing. `Apply` puts the
  missing ones into your active set: free slots first, then from the last slot back the slots holding a
  spell the guide does not name. Nothing changes without the click, and the set improver leaves the set
  alone afterwards. The stage name shows beside the briefing's; two buttons switch the stage.

## Beastmaster

- **Final Sting instead of Parting Blow when the wasp makes way in danger.** In danger a familiar that neither
  heals nor protects leaves for one that does; the wasp left by Parting Blow (1,000 potency) while its Final
  Sting (2,000 to 3,000, and Lingering Vantage for the next Parting Blow) was ready and retreats it the same
  way. It now leaves with Final Sting.
- **Final Sting before the last fight of a board ends.** Final Sting waits for another horn to be ready, so you
  are never left without a familiar. In a board's final fight it now goes before the enemies fall when no other
  horn would be back before then; held, it was lost, and after that fight no horn is needed.
- **The gauge is read where the game keeps it.** TP, the familiar's TP, the active horn, the compass,
  the combo count, Kinship and the instincts now come from the gauge layout Dalamud ships, not from a copy
  with fixed offsets. After a patch that moves them, the Beastmaster keeps choosing axes, horns and combos
  from the right values as soon as Dalamud updates.
- **Snarl and Challenge over the window they share.** The two share one 15 s recast, so whoever holds the
  enemies after a swap holds them for all of it. No Snarl now goes onto a familiar whose damage taken
  per second while holding, over that window, or its largest hit would fell it; Challenge takes the
  enemies back only when you would stay above your HP floor for the window, and Snarl goes when you
  would not but the familiar would. Rates are measured per fight, per familiar, and fall back to the
  enemies' measured damage before one has held for a GCD.

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
- **Between fights, RSR works with RSR off.** The horns on a Crucible board's horn screen, the team
  recommendation, the campsite and shop windows, the horns between fights and the Blue Mage set
  improvement ran only while RSR was on - and RSR turns itself off 30 s after a fight, on an area change
  and in cutscenes, so on a board none of them ever ran. They now run every frame you are loaded, on or
  off, each under its own setting; the horns between fights outside the Crucible need the target lists,
  which RSR builds while it is on or an automatic start is set. Opening the horn screen writes to
  `DefenseTrace.log` whether RSR was on and which game states were set, and the horn screen says when it
  cannot read the coming battle or the team.
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
- **Gauge asks how hard a pact is, twice.** Whether Gauge's answer counts the beast's HP is stated
  nowhere; it decides whether Capture could go out earlier at the same chance. With `Capture familiars
  you do not have yet` on, RSR now uses Gauge on a beast you lack while it fights - in a weave nothing
  else wants, once at first sight and once after your mark - and `DefenseTrace.log` writes both
  answers with the beast's HP and whether the answer moved. The first beast whose answer moves ends
  this for the session. The same line notes whether the game would take a second Capture on the
  marked beast. Gauge never goes on an enemy that is not yet fighting.
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
- **A horn waits for the enemies that come later (Crucible).** RSR now reads every fight of the
  boards from the game data: its enemies and their weaknesses, and which weakness each familiar's
  Trick and Tempered Release hits. In a fight where enemies join later - the Soulless one calls her
  succubi after about 45 s - a familiar that hits only their weakness is kept on its horn while
  another horn is ready, and called first once they appear (after danger and stopping a pack). On
  04.11.2026 the Raptor's fire went to the boss 40 s before the fire-weak succubi came. With
  `Choose the battlehorn by the situation` on. `DefenseTrace.log` says on entering a board whether the fights
  were read, names each fight with the enemies still to come, and every hold.
- **Weaknesses count, measured (Crucible).** No source states how much more a hit on a weakness
  deals, so RSR measures it on a board: each of your and your familiars' hits carries its element
  and attack type (your axes and auto-attacks strike as slashing, though the action sheet lists no
  type), and an action's damage on a weak enemy is set against its damage on others of the same
  armour grade. Until a board has measured it, +27 % counts - measured on 18 such pairs in the
  owner's logs. A Trick counts every weak enemy it hits with that bonus when the horn is chosen and
  on the horn screen, and with no tank and healer beside you an enemy whose weakness your damage
  hits counts as falling that much sooner. Before any enemy has hit, the board ranks enemies by the
  strength and intelligence the game rates them with, over their HP. `DefenseTrace.log` names the
  first measurement on each board.
- **A familiar's worth counts every source of its damage (Crucible).** Its auto-attack (the familiar's own
  action in the game data) carries about a third of its damage, Parting Blow another third, the Release
  and the Trick the rest. The weakness bonus now counts on the share whose source strikes the weakness -
  auto-attack, Trick, and the Release on a horn that uses it - when the horns are chosen, on the horn screen
  and in the team recommendation. The shares are measured from the familiars' hits.
- **A familiar that interrupts, where a fight needs one (Crucible).** RSR remembers over every session which
  enemy kinds on a board cast spells the game marks interruptible (`InterruptibleCasterKinds.json`). Before a
  fight with such a kind, a Soulkin familiar on a horn set to Borrow - its Soul Crush interrupts - counts on
  the horn screen ahead of damage, and the team recommendation includes one when a fight of the board needs it.
- **A stunning or petrifying familiar counts only against enemies it can stop (Crucible).** The game
  states for every enemy of the boards which statuses it resists; most elites and every final boss shrug
  off a stun. A Tempered Release that stuns, petrifies or deep-freezes no longer counts as protection on
  the horn screen before a fight whose enemies resist it, and is no longer called first at a pull of
  such enemies - before, RSR learned the immunity only after the first try.
- **Challenge only when the familiar could fall (Crucible).** A familiar retreating after Parting
  Blow shows 0 HP for a moment; that no longer counts as the largest hit of the fight, so Challenge no
  longer takes the damage back from a familiar at full HP every 15 s, and no Beast Potion goes into a
  full familiar.
- **Healers kept for the final fight (Crucible).** Before any fight but the board's final, the familiars
  that heal or protect in the plan the final would get sit out when one more use as costly as the largest
  measured on the board would leave them too little for the final - as long as the horns still fill and
  this fight gets as many that heal or protect as it needs. The horn screen now chooses with the HP each
  familiar really has: the team list in the game takes a fight's losses over only when the next fight
  begins, so RSR keeps the HP each familiar left its last fight with. Those healers also take no Snarl in
  an earlier fight while they are that short - under Snarl a familiar takes all that was yours, and only
  items and the campsite give its HP back - unless your HP are at the floor; and at a campsite they rest
  first.
- **No Snarl on a familiar that is leaving (Crucible).** After Parting Blow, or a Release the familiar
  leaves after, Snarl waits for the familiar that follows instead of covering you for the last few
  seconds and holding its recast from the next one.
- **Danger counts the hits on your familiar (Beastmaster, Blue Mage alone).** With no tank and healer
  beside you, an enemy's danger counts its hits on your familiar too - under Snarl it takes all of
  them -, and an enemy that has not hit yet counts with the danger estimated from those that have
  (in the Crucible scaled by the game's ratings), instead of falling behind any measured enemy.
- **The board's items that keep you and your familiar alive (Crucible).** With `Use the board's items
  that keep you and your familiar alive (Crucible only)`, **on by default**, RSR uses, one at a time:
  below `Use HP potions below this HP` in combat the Beast Potion or Crucible Ash that lifts you back
  to that line with the least (the board locks normal potions); an antidote or eye drops against
  poison or blindness on you; a Beastmaster Reraiser while no Reraise stands, the stronger first -
  only against a boss once Reraise is seen to go without a fall; Temporal Sand out of combat while no
  Sandstill stands; a Beast Potion Kit against a boss; against an announced hit on you the Tome of the
  Impervious (physical), the Tome of Reflection (magic) or Crucible Tannin; for your familiar, when it
  holds no more HP than the largest hit it took this fight, the Beast Potion that lifts it above that
  with the least - never your last healing item, which stays for you - then the same protection when
  the hit could fell it; against the statuses the fight's enemies inflict that take
  control from you or end in death (read from the board's sheets) the soul serum that names one, else a
  Beast Remedy Kit; against a hit on you whose cast carries an element the weakener of that element; a
  Crucible Needle for your petrified familiar; against a boss a Potion of Tempered Constitution. In the final fight, whose items are lost after the run, protection also goes
  whenever an enemy attacks you. Each item's kind and size come from its own text. `DefenseTrace.log`
  names each use, and a slot the game did not take is not tried again in that fight.
- **The horn screen waits until it is ready (Crucible).** The horn screen first shows the last battle's
  horns and clears them a moment later; RSR took that clearing for your hand and left the horns alone
  for the battle. It now reads the screen only once the field list is built and the battle readable.
- **A familiar for enemies still to come waits from the pull (Crucible).** The battle the horn screen
  named counts until the fight is told, so a familiar that only hits the weakness of enemies that
  come later is held at the pull as well.
- **Familiars that lag behind level up (Crucible).** `Prefer familiars that lag behind in rank for
  the Crucible team`, **on by default**: the team recommendation gives a place to the lowest-ranked
  familiar that replaces another adequately - the same weaknesses, healing or protection where
  needed, at least the rank the game recommends for the board. On the horn screen a familiar below
  that rank fights only when fewer others live than horns.
- **What to take from a treasure (Crucible).** `Recommend what to take from a Crucible treasure`, **on by
  default**: while a treasure is open, a window names the offer to take and why, in the order of the shop
  recommendation with no price - a second life, cover against a deadly status of the board, the gear that
  leaves you the most HP after a stretch of fights, cover against the other statuses, healing, then
  protective items; when none adds safety a damage item or buff, never gear that harms you. `Take` takes
  it; with a Thief's Knife the window stays open for the second pick.
- **The board's damage items and buffs (Crucible).** With `Use the board's damage items and buffs:
  damage on a pack once a fight, all of them in the final fight (Crucible only)`, **on by default**,
  RSR throws a fang or Celestial Sand on your target: every one in the final fight, the most worth
  first; in any other fight one, once it hits more enemies than your target. An enemy weak to its
  element counts the measured weakness bonus more. In the final fight - the board's items are lost
  after the run - it first uses on you every potion or item that grants you a status (strength, magic,
  intensity, evasion, haste, TP, Life Siphon), never one that inflicts a status on you.
- **Quelling Wave against a water weakness (Crucible).** On a board Quelling Wave now goes before the axe
  combo when the target is weak to water and its potency with the measured weakness bonus beats the
  combo's mean step; elsewhere it stays the filler for when no axe reaches the target.
- **The horns on the Crucible's horn screen.** The game asks for the horns before each battle on a
  board and keeps none from the last. With `Set the battlehorns between fights for the next one` (on
  by default) RSR now picks from your team for the battle about to start - the final fight counts as a
  boss, the pack is the battle's enemies, a familiar that hits their weakness counts the measured bonus,
  among equals the one with more HP left - and clicks them in one at a time. Picked by hand there,
  your horns stay; you start the battle.
- **A recommended team on the Crucible's team screen.** While you pick a board's team as a beastmaster,
  a small window `RSR - Crucible team` lists the team RSR recommends for that board - as many as it
  holds: two that heal or protect you, then those that hit the weaknesses of the most enemies across
  the board's fights - with the reason for each and who would leave. Its `Apply` button changes the
  team one familiar at a time; nothing changes without the click, and you confirm the team yourself.
- **Who rests with you at a Crucible campsite.** A rest shares its healing - alone you recover 90% of
  your HP, with one familiar each 45%, with two each 30%. With `Pick who rests with you at a Crucible
  campsite (Crucible only)`, **on by default**, RSR selects familiars only when you still come out at
  full HP, so you never come out worse than resting alone; then as many as bring them the most HP, up
  to what the campsite allows: those not fallen, not at full HP and not fed (resting ends a feed), the
  most hurt first. It reads the share from the game's prompt once it is open. A selection you change
  by hand stays; resting or leaving is your click.
- **What to buy in a Crucible shop.** With `Recommend what to buy in a Crucible shop (Crucible only)`,
  **on by default**, a small window `RSR - Crucible shop` lists what to buy and why while a board's
  shop is open, safety first as far as your tokens and slots go: a second life while you hold none;
  cover against each status of the board's fights that ends in death (Doom); protective and healing
  gear, the choice that leaves you the most HP after a stretch of fights between campsites ending with
  the heaviest - measured from your fights: the damage you took by kind, your own healing and the
  fights between campsites - so the Ring of Curing goes ahead of armour where fights wear you down,
  and armour ahead of it where a campsite comes before each fight; then cover against the board's
  other statuses that cost safety (Paralysis, Poison); healing items; protective items. Never feed,
  never gear that harms you (such as Thunder Axe or Wind Mask). A piece that slows you says so. `Buy`
  buys the list row by row; when the game asks, your answer decides. Without the click RSR buys
  nothing.
- **Snarl and Challenge are on by default (Crucible).** Without a tank the enemies attack only you,
  and your fall ends the run, while a fallen familiar costs only that familiar. `Use Snarl and
  Challenge based on your HP and your familiar's HP (Crucible only)` is now **on by default**, still on
  the last horn only. Challenge now also takes the damage back as soon as the familiar holds no more
  HP than the largest hit a familiar took in this fight, so the next such hit does not fell it.
- **A familiar that covers you stays (Crucible).** After Snarl - pressed by you or by `Use Snarl and
  Challenge …` - the familiar takes all damage meant for you for 45 s. RSR no longer sends it off with
  Parting Blow, or with a Tempered Release after which it leaves, to call the next horn while that cover
  lasts; the cover ends with its time, with Challenge or with the familiar's fall. The next horn comes
  up to 45 s later. `DefenseTrace.log` notes the hold once per cover.
