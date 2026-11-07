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
  window whenever it shows and recommends feed for free slots only – first one per familiar of the final fight's plan,
  healers and protectors first – and only feed that adds effective HP, or damage after those. While you feed, the
  window names the familiar with a free slot that the feed helps most, or says that none has one.
