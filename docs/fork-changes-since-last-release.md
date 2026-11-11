# Changes since 7.5.7.15+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Seeing what the rules decide

- **The recommendation windows are readable and resizable.** The Crucible team, shop, treasure, campsite and route
  windows and the Masked Carnivale set window opened as narrow as their narrowest button, broke names letter by letter,
  cut their titles off and could not be resized. Each now opens as wide as its texts need: a text of up to 75
  characters on one line, a longer one broken at 65, never narrower than 50 characters. Width and height can be dragged
  and are remembered. The height follows the content until you drag it shorter, then the window scrolls until you
  drag it back. A narrow size saved by the old windows is replaced on first opening. Card titles are a size above the
  text, each recommendation's verdict is a chip in text size, and a line's reason sits right under it with a line of
  space before the next recommendation.
- **Tooltips are broken at a readable length.** A setting's description in its tooltip (with "Show setting
  descriptions inline" off) ran on in one line up to 1200 px and was cut off there; the longest have over 2000
  characters. Every tooltip text now breaks at 65 characters (up to 75 stay on one line), and the tooltip grows with
  it. The rotation tooltip in the navigation no longer wraps its description at the width of its shortest lines.
- **The shop line names every offer.** `DefenseTrace.log` gave only what the Crucible shop recommended, not what it
  offered, so a missing piece of gear could not be told from one that was not for sale. The line now lists each offer
  with its price.

## Every job

- **Items and actions the game refuses in this place are no longer chosen.** RSR read only refusals of the moment –
  recast, no item, a status. A potion usable only in its own content stood ready everywhere else: as a Beastmaster
  without a healer, RSR chose the Pilgrim's Potion of Pilgrim's Traverse 17 times in five areas, every 2.5 s while your
  HP fell, and it never healed. RSR now reads the game's refusals for a place – not usable here, only in PvP, not in PvP,
  only in duels or Frontline – for every item and action. When the game still refuses an item, `DefenseTrace.log`
  names the item, the area and the game's answer, once per item and area.
- **A kill rule holds while the other enemy cannot be targeted.** Enemies that must fall together (the tablitaurs of
  board 2, the lamias) or only after another (morpho and abaddon, the myconids while the mindflayer lives) counted only
  while they could be targeted. The elder tablitaur cannot be targeted for 26 s of its Endless Swing; RSR let the
  younger one go then and brought it to 176 HP before the elder came back, about 3 s from its fall and the elder's
  enrage. Every enemy alive that has been targetable in the area now counts, targetable at the moment or not; the
  invisible helpers that carry the tablitaurs' names (six with 44 HP in each fight) never were and do not count. The
  trace says "it is gone" only when it is.
- **The pair's reserve keeps the highest stream measured.** The reserve a member waits at is half the survivor's
  window times your stream, read from the enemies' health histories. A history begins anew when its enemy regains
  health, and the stream could fall from 155 to 12 HP per second in under a second: on board 2 the elder was released
  at 526 HP, and the younger fell 0.85 s before the end of its Endless Slashes. The stream now keeps its highest value
  while a member of the pair lives.

- **Swiftcast for a raise comes late in the GCD, not right after it.** It waited for the first weave window after a
  GCD; when that window went to another ability – Thin Air, which the White Mage takes there for a pending raise, or
  Assize – Swiftcast came one GCD later, and the damage spell in between used up Thin Air (6.6 and 7.6 s from death to
  raise instead of about 2 s). When it fell early, almost a whole GCD lay before the raise, and five times another
  healer raised the same member instantly in that span; Swiftcast then went into a damage spell, and the next raise
  within its recast was cast in full (8.0 s instead of about 1 s). Swiftcast now falls in the late weave window: the
  raise comes with the next GCD as early as before, the first window stays for Thin Air, which the raise then takes,
  and another healer's raise has at most the late window to come first.
- **With your Swiftcast running, a slower raise by another no longer holds yours back.** RSR left a member to another
  healer's raise in progress even with its own Swiftcast already up; that raise landed 9 s later and Swiftcast went
  into a damage spell. With Swiftcast running, RSR raises the member itself when the other raise lands later than its
  next GCD plus one GCD.
- **The trace names why a dead member is not raised.** For each dead party member, `DefenseTrace.log` writes a line
  whenever the reason changes – beyond 30 y, out of sight, not targetable, moving, left to another raise, raise denied
  – with the seconds since RSR saw it dead and the distance; "a candidate" means it is the raise target.

- **A White Mage on the move heals a member under 30 % with Regen.** Below its health threshold Regen stayed away, for
  Cure II and Benediction heal more there. On the move neither Cure II nor Cure can be cast: at level 50, with
  Benediction on recast, you stood 7 s at 5 to 7 % HP while RSR cast Aero II three times. Below the threshold Regen now
  goes out while neither Cure II nor Cure can be cast; the setting is now "If a party member's health drops below this
  percentage, the Regen healing ability will not be used on them while Cure II or Cure can be cast".

- **Surecast and Arm's Length wait for a knockback that reaches you.** Any listed knockback cast in reach of RSR's
  enemy list pressed them; in the Tower of Babil Ground and Pound took Surecast for 120 s while you stood outside its
  line. A listed knockback now counts only when its shape reaches you - a circle or line exactly, a cone or ring as a
  full circle, a cast aimed at one player only when aimed at you. BossMod's timeline still counts as before. Each time
  the protection is called for, `DefenseTrace.log` names the way: the cast and its caster, BossMod's timeline, or the
  winds in Windurst.

## The Beastmaster between fights

- **A familiar left out after a fight goes before the next one.** A familiar that ended a fight without One with
  Nature stayed out until the next pull, left there by Parting Blow, and only the call after it brought a Release or
  Borrow. Out of combat, RSR now sends it away with the pet command Away – a retreat outside of combat starts no recast
  – and calls the next familiar with One with Nature before the pull. The pull opens with its Release or Borrow, and
  all three horns can be set for the next fight. Not for the familiar of your Kinship, and only from level 18, where a
  call grants One with Nature. If the game refuses the command or the familiar stays out, RSR writes it to
  `DefenseTrace.log` and keeps the old order. Not on a Crucible board: its horn screen sets the horns before every
  fight, and the familiar out goes with them.
- **Gauge is no longer used.** With `Capture familiars you do not have yet` on, RSR used Gauge twice on each beast you
  lack, to learn whether its answer counts the beast's HP. It does not: two Chimeras answered "not easy" at full HP and
  at 6 and 7 %. The stage is the beast's own difficulty, and the best moment to mark is the last safe one at every
  stage, so no rule needs it. Two weaves and four chat lines per beast are free again; Capture is unchanged.

- **Leaving a board is not a change of the horns by hand.** The game clears the horns after each fight on a board; RSR
  saw the cleared horns on leaving as your change, wrote "battlehorns changed by hand to None" after every board and set
  no familiars until the next fight ended, so a fight outside right after a board began with empty horns. RSR now
  follows the horns on the board without judging them.

## The Beastmaster on a Crucible board

- **The board's score counts.** Remnants and familiar EXP grow with the score the board gives after its final fight.
  While no item has been used in the run (the bonus "Sparsamkeit", 750 points) and your HP carry the heaviest way to the
  final fight with the loads RSR learned, items that guard against no present danger stay in the inventory: Temporal
  Sand, a Reraiser, the Auto-potion against a boss, maximum HP against a boss, damage items, tomes and buffs, healing no
  fight needs. Healing below your potion line, cures, protection against an announced hit and the familiar's care go
  as before, and once an item has gone everything does. Among safe ways the route takes the one with more points (an
  enemy space 250, an elite space 800), then, while the bonus stands, the one safe without items. The trace names when
  items are kept, when they go again and when the bonus falls.
- **The team recommendation takes as few familiars as the score rewards and the board allows.** A team of 6 or 7, 4 or
  5, or 1 to 3 familiars gives 1,250, 2,500 or 4,250 points ("In Unterzahl"); the recommendation filled every board to
  its limit. RSR now learns per board what each use of a familiar cost, the largest seen for each rank, and recommends
  the smallest of those sizes that holds every familiar it needs for the weaknesses, healing and interrupts and on
  whose familiars but one every measured use fits, largest first, without one falling. Until a run on the board was measured it recommends the full team; the window says
  which and why.
- **A team of one kin where one does as much.** A team of only one kin gives 750 points ("Freund der …"). Where the
  familiars of one kin hit every weakness of the board the mixed team hits, heal and protect as much and interrupt
  where it does, under the same size rule, the recommendation takes that team; levelling swaps only within the kin.
- **Ten items held count as well, and the run ends at full HP where it can.** On boards 2 to 5 ten crucible items held
  at the end give 1,000 points ("Sammelleidenschaft"); with ten in the inventory, items that guard against no present
  danger now stay there as for the no-item bonus, while your HP carry the way. Once no item bonus is at stake, the
  healing left in the final fight takes, just before its last enemy falls, the smallest item that brings you to full:
  250 points for full HP ("Unversehrter Bändiger") and the HP the score counts. Before that, when every other familiar
  of the team stands at full HP, a Beast Potion brings the familiar out to full for 1,000 points ("Unversehrte
  Bestien"), where those outweigh the item bonuses the use costs. The points of every bonus are read from
  the game's data for the board you are on. Each Crucible window, the score at the end among them, is written to
  `DefenseTrace.log` once per run with its values.
- **The shop and the treasure keep their bonuses too.** While nothing has been bought in the run ("Geizhals", 500) and
  your HP with the healing you hold carry the heaviest way to the final fight, the shop recommends only cover against a
  status that kills whatever your HP, such as Doom on board 2. While no beast gear has come in the run ("Mit leeren
  Händen", 750), the treasure recommends the best offer without gear, when there is one.
- **Rest and feed only where the run needs them.** While no rest has been taken ("Schlaflose Nächte", 1,000) and the run
  needs none - your HP and healing held carry the heaviest way to the final fight, and the weakest final healer has more
  than a use - the campsite window advises walking on and selects nobody. While no familiar has been fed ("Heilfasten",
  750) under the same condition, the shop recommends no feed.
- **The horn choice learns from the fight.** RSR measures each familiar's damage per second while it is out, per fight,
  and keeps its best. The horn screen weighs a familiar's estimate by how its measure stands against the others measured
  in the same fight; a familiar never measured keeps its estimate. The learned values show in the tab "Crucible
  familiars".
- **The route window says left or right, and why.** At a fork it names each way by its side as seen from the start
  towards the final boss, with the spaces up to where the ways meet again, and weighs them up to the next campsite or
  the boss after that: your HP and your final fight's healers' HP there, and what the other way would have brought. It
  appears once the fight of the space you stand on is over, not before it.
- **A treasure shows every offer with its reason.** Gear and items are weighed on one scale: the share of your HP each
  brings over the fights left on the shortest way to the boss – what gear prevents and its extra healing on the potions
  you hold, what an item heals or wards off once. What the scale does not count (movement, haste, block, critical
  hits) is named. Names are shown in your client's language.
- **The shop reads 1,000 tokens and more in every client language**, whatever separates the thousands.
- **The shop no longer recommends gear you already wear.** A shop lists it again and the game refuses the purchase; a
  piece taken from a treasure on the board counts as worn.
- **Feed only for free feed slots, and only when it helps.** RSR reads each familiar's feed slots from the familiar
  window whenever it shows and recommends feed for free slots only, and only feed that adds effective HP, or damage
  after those. Piece by piece, the healers and protectors of the final fight's plan come first, each piece where it
  helps most; while you feed, the window names the eater by the same rule, so following the shop's list in its order
  gives the same names there. Familiars are named with their HP as the familiar window shows them (Kolibri 374/487).
- **The final fight's plan is the final fight's choice.** The plan the campsite and the feed prepare is now worked out
  by the very choice the final fight's horn screen makes, again whenever HP, feed or a measured loss change, and feed
  counts in every horn choice on a board (damage dealt, damage taken). A familiar fed for the final fight stays on its
  horn there.
- **Protection is weighed by what it catches.** The shop and treasure scale counted a protective item as your whole
  heaviest fight. On board 1 fights last 61 to 148 s, and the best 30 s held 55 % of the final fight's damage, the five
  hits in a row 48 %. The scale now counts what falls within the item's duration (Blink: its stacks) in the heaviest fight
  measured, with the duration your gear gives it (Ice Shield, Hexed Hat). While you wear or choose a Crimson Ribbon, soul
  serums count as 50 % heals, and the ribbon counts the serums you hold; the Potion of Breathtaking Swiftness counts the
  three Blink it adds to a held Tome of the Impervious.
- **Feral potions under protection.** RSR read no feral potion. It now uses one like its tempered counterpart while the
  gear you wear makes you immune to the status it lays on you: the Umbral Wristlet against Stun, the Crimson Ribbon
  against Petrification and Blind, Silver Specs against Blind. Feral Magic (Nightmare) and Feral Constitution (Pollen)
  have no such gear and stay unused.
- **Damage items stop counting overkill.** A fang counted its full potency on every enemy it reached; three shadow mites
  with 522 HP took 1,304 to 1,600 each. RSR now measures what its damage items deal per potency on their hits and counts
  each enemy at most at its HP left - in a fight ahead at the most HP it saw that kind with -, so an item waits for the
  fight where it is not wasted.
- **Damage items go where they spare you the most HP.** Where RSR knows the fights' loads, a damage item is weighed by
  the load it spares: the load a fight can still cost you times the share of the enemies' HP the item takes. In the fight
  in progress that is its learned load less what it cost you so far; in a fight ahead its learned load. Potency on a
  harmless enemy with many HP spares nothing, so items no longer wait for the final fight by default. Without a learned
  load, the damage per potency or a kind's HP, potency decides as before.
- **Kill stacks count once RSR has seen them.** With Demonic Armor a kill grants a stack of High Wire (5 % less damage
  taken, up to four). A damage item that kills now also counts what its stacks spare you in the rest of the fight - but
  only after RSR has seen, in this session, that a kill by an item grants a stack; the trace names the result.
- **Tomes in the final fight, measured.** RSR never used the Spellforge and Steelsting Tomes. Like a buff they now go in
  the final fight only: unmeasured when the fight ends (they would expire, and their use measures them), at its start
  once RSR measured on that board that they raise the damage, never once they did not. RSR compares each hit a tome
  converted with the same action in its own kind on the same enemy kind, and the trace names whether the converted hit
  keeps its element.
- **No feed twice.** The game refuses a feed a familiar has already eaten. The feed list and the feeding hint no longer
  name such a feed for it, nor one the list already gives it.
- **An enemy that has not hit yet counts with its kind.** Before its first hit an enemy ranked by the mean danger of the
  fight's other enemies, even when another of its kind had hit already: a second Aetheric Charge stood like an average
  enemy. It now counts with the danger its kind showed in this area - each enemy measured until it fell -; only enemies
  that stood in the target list count, not hidden helpers carrying a boss's name.
- **The King Ahriman piece's hourglass is broken only when it is safe.** Breaking the final hourglass makes the roulette
  needle's next beat fire Death along it, and its Doom kills; left alone, Death fires when its cast ends. RSR now puts
  the hourglass before every other enemy, and once it is hit holds every attack on it - area attacks too - while a press
  could break it on a beat that points into your half. It reads the needle's heading and beat from the game, measures the
  round trip of your presses, and lets go when gear makes you immune to Doom, when the needle is not readable, or when
  waiting would outlast the cast. Setting "Kill rules of known fights".
- **The way window plans a flight.** With a Beastly Smokebomb held, the way window weighs fighting and fleeing at each
  enemy space ahead and names the fight to flee ("Flee the fight at move 5 with a smoke bomb"); elites and bosses cannot
  be fled. While the Coward's Knife is worn it tells you to take the escape when offered. A treasure counts two items
  with the Thief's Knife. On the shop scale a smoke bomb counts the heaviest enemy fight on the way no bomb you hold
  covers, the Coward's Knife its chance on the others.
- **The Beast Potion Kit has a value on the scale.** It counted nothing before. It now counts what it restores below
  its 50 % line: of the healing your potions leave you short, the part that would take you below half, within its
  duration from the start of the heaviest fight, with the chance each restore expires. Thief's Garb counts its longer
  duration, the Potion of Breathtaking Swiftness its lower chance to expire.
- **Fed familiars rest where their feed allows it.** A familiar fed a Lily Simular keeps its feed through a rest, and
  RSR now lets it rest like an unfed one; before, no fed familiar ever rested. A rest fills a familiar fed a Lassi Simular;
  without the Lily it rests only when it comes out with more effective HP than it has. The Chemist's Satchel counts the
  weakest beast potion at each campsite every way passes.
- **A campsite rest is read per board.** The rest shares a different whole on each board (board 1: 90 %, 45 % each
  with one familiar; the First Master's Board: 150 %, 75 % each). RSR now learns it per board from the rest prompt,
  caps each share at 100 %, and assumes the smallest known until it has read the board – a reading from one board no
  longer makes a rest on another look larger than it is (tab "Crucible campsites").
- **More familiars rest with you at a campsite.** RSR learns, over every session, the heaviest damage each fight of a
  board dealt you, without your gear (tab "Crucible fights"). A campsite lets familiars rest with you while your HP
  after the rest still carry the heaviest way to the next campsite – or to the final fight on a way without one – with
  each fight at its own load; only where a fight on those ways was never measured do you have to come out at full HP.
  The way window and the shop's limit for healing items use the same loads, so the final fight no longer counts as
  heavy as the heaviest fight before it.
- **The way window weighs every fight up to the final one.** Each way is walked to the end of the board, each fight at
  its learned load, with the rests on it. A way is safe when you live through all of them with the items you hold and
  the final fight's weakest healer can take one use there; RSR's reserve keeps that healer off earlier fights instead of
  counting its losses. Among safe ways the one with more spaces with items wins, so a shop every way passes no longer
  tips it to the campsite.
- **Crucible items are spent over the whole board.** Healing beyond what the fights up to the final one can cost you is
  used without overhealing, also above your potion line, and in the final fight all of it. A damage item goes where it
  brings most: in this fight if it hits as much here as in the best fight ahead, else it waits; one with knockback only
  when it hits every enemy fighting. Against a boss the Beast Tonic comes before a healing item, which then heals more.
- **A damage item waits for the enemies still coming.** A fang no longer goes the moment a second enemy is in reach:
  while an enemy outside its reach has neither hit nor cast yet – the shadow mites stand still for seconds after they
  appear, then run in – or, while it reaches only its target, a kind of the fight has not appeared yet (the final
  boss's succubi), it waits. In the final fight it goes at the latest when the fight could end before every item held
  had its turn, a knockback fang included. The trace names the items you hold at the start and the end of each fight.
- **Crucible items in the order their texts reward.** The Potion of Tempered Magic goes right before the first fang;
  Fang of Ice before Fang of Fire, which deals 1,000 potency more to a target in Deep Freeze; Celestial Sand, which
  lowers its target's fire resistance, before Fang of Fire; the Potion of Breathtaking Swiftness right before the Tome
  of the Impervious (three more Blink stacks) or the Beast Potion Kit (Auto-potion rarely expires). While you carry a
  crimson ribbon, the soul serums count as the 50 % heals they then are.
- **Gear and feed belong to the run.** A new run on the same board no longer inherits the gear and feed slots of the
  last one.
- **A familiar is healed before a first hit could fell it.** Before a familiar takes its first hit in a fight, RSR
  measures it against the largest hit any familiar took on the board so far, instead of your potion line, and gives it
  a Beast Potion when its HP do not cover that hit. Your last healing item still stays for you.
- **The shop weighs gear and items on one scale.** Once a fight is measured, gear, healing items and protective items
  are chosen together by the share of your HP each brings over the fights to the boss, piece by piece the most per
  token. A healing item counts only as far as you could still need it: the fights on the heaviest way to the boss, each
  with its learned load (one never measured as heavy as your heaviest), plus what you miss now, less the healing you
  hold. Covers against the board's statuses that cost control or deal
  damage over time now come before this scale.
- **Adds that run to their boss are met, not chased.** RSR measures which enemies run to another enemy and when each
  arrives, learning from the first of a kind to vanish where its kind arrives. Of equally urgent enemies, the one nearest
  to you comes first, so you meet the Ogre's wisps instead of running after the one that arrives next. Just before the
  first of them arrives and blows up, the Beastmaster snarls so the familiar's Cover takes the blast and its Vulnerability
  Up, unless the next loss would fell the familiar.
