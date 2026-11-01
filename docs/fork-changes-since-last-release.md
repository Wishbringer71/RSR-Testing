# Changes since 7.5.7.13+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Rotations

- **Warrior and Dark Knight spend into the pack only where it pays.** Their area spenders went out from two enemies on,
  although on two or three they deal less than the single-target ones: on two enemies Decimate dealt 360 where Fell
  Cleave deals 580, Chaotic Cyclone 400 where Inner Chaos deals 700. Each now goes out from the count where it deals at
  least as much, by the tooltips of the running patch: Decimate and Chaotic Cyclone from four, Orogeny, Steel Cyclone,
  Quietus, Flood of Shadow and Impalement from three. Below that the single-target spender goes in the same place, so
  the gauge is spent as before; the area combo still holds the enemies from two on.

## Healing

- **Downtime healing heals only those below its line.** "Use GCD heals when there's nothing else to do" started when
  the lowest member fell below "When the lowest party HP is below", but then healed anybody not at full health: with
  the tank at 75 % already carrying Regen, Regen went on a black mage at 95 %. It now heals only members under that
  line, by the same danger order as every other heal, so the member who started it always qualifies.

## Packaging

- **latest.zip holds the plugin and nothing else.** The release package of 7.5.7.13+wsh1 also carried the build's source
  generator (5.7 MB, never loaded), and a local build packed whatever earlier builds had left in its output folder, such
  as the old RotationSolver.Basic.dll. The package is now made from a fixed list - the plugin, its manifest, ECommons
  and RebornMaterial -, and a release is not published unless its zip holds exactly that.
