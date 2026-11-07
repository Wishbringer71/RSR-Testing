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

## The Beastmaster on a Crucible board

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
- **More familiars rest with you at a campsite.** RSR learns, over every session, the heaviest damage each fight of a
  board dealt you, without your gear (tab "Crucible fights"). A campsite lets familiars rest with you while your HP
  after the rest still carry the heaviest way to the next campsite – or to the final fight on a way without one – with
  each fight at its own load; only where a fight on those ways was never measured do you have to come out at full HP.
  The way window and the shop's limit for healing items use the same loads, so the final fight no longer counts as
  heavy as the heaviest fight before it.
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
