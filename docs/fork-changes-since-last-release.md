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
