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

- **The emergency heal now gets out between two hard casts.** After a 2.0 s heal (Cure II, Medica II, Cure III,
  Adloquium, Succor, Prognosis) at most half a second is left before the next GCD, and abilities are never woven into
  that last half second, so a healer chaining Cure II never reached Benediction: a tank under a whole pack fell to 43 HP
  with Benediction ready. When a party member would die before a GCD heal begun now could land (the critical class,
  "HealthForDyingTanks"), the job's heal ability for that member is now pressed the moment the GCD comes free, and the
  GCD follows after its animation lock. Which ability goes, and on whom, still follow each job's own thresholds and
  danger checks - Benediction stays the absolute emergency. Each such heal, and each critical member left without one,
  writes a line to `DefenseTrace.log`.
- **Downtime healing heals only those below its line.** "Use GCD heals when there's nothing else to do" started when the
  lowest member fell below "When the lowest party HP is below", but then healed anybody not at full health: with the
  tank at 75 % already carrying Regen, Regen went on a black mage at 95 %, and three members at 95 % were enough for an
  area heal. It now heals only members under that line, by the same danger order as every other heal, and casts an area
  heal only when more than two stand under it. Each downtime heal writes a line to `DefenseTrace.log`.

## Mitigation

- **An area series of a pack ends with the pack's round.** A listed area cast that lands several times in a row is rated
  by what the whole series takes from a member, whoever cast each landing - a mechanic's helpers and a pack land on the
  party together. In a big pull of like enemies the series ran on as long as any of them filled the gap, and its sum
  grew with the pull: three enemies recasting an 8 % area cast every 8 s could be stored at 64 %, and every later cast
  of it was then mitigated and pre-healed as a near-lethal hit. A targetable enemy that casts again within a running
  series now starts a new one; mechanic objects (untargetable helpers) keep counting together, so tower sequences stay
  one series. Each split writes a line to `DefenseTrace.log`.

## Packaging

- **latest.zip holds the plugin and nothing else.** The release package of 7.5.7.13+wsh1 also carried the build's source
  generator (5.7 MB, never loaded), and a local build packed whatever earlier builds had left in its output folder, such
  as the old RotationSolver.Basic.dll. The package is now made from a fixed list - the plugin, its manifest, ECommons
  and RebornMaterial -, and a release is not published unless its zip holds exactly that.
