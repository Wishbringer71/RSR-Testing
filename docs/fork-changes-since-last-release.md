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
- **Beastmaster calls its familiars by the situation and by what you have.** Among the ready battlehorns the familiar
  whose Trick brings least against the enemies in combat around your target is called first, so an area familiar on a
  pack keeps the long stint until a horn is back. When your health is falling below the line at which RSR heals you with
  abilities, a familiar that heals you (Digest, Blood Drain) or protects you (Tempered Release or Borrow) is called first,
  one that does neither leaves by Parting Blow for it, a healing one stays out while the danger lasts, and Scaleskin goes
  up at once. Only the familiars on your horns and only the actions you have at your level count. New option "Choose the
  battlehorn by the situation", on by default.
- **Beastmaster no longer keeps a familiar out for good.** A horn set to Borrow before level 22, or to "Neither", never
  spent One with Nature, and Parting Blow waited for it: the familiar stayed and the other horns sat idle. A horn now waits
  only for the action it spends One with Nature on, and Borrow that cannot come yet is Tempered Release.
- **Beastmaster no longer rushes back into danger for a finisher.** Shield Charge, Brutal Rage and Hawkish Talons carry
  you to the target from up to 20 and 25 yalms; they now keep to "Only use damaging gap closers within this distance"
  (3 yalms by default) and check the landing point. Risen Fall, the same 1,200 without the rush, now goes before Brutal
  Rage, and Calamity before Hawkish Talons unless the rush reaches more enemies.
- **A battlehorn no longer pushes back your next weaponskill.** Its 1 s summon now starts only where it ends before the
  half second kept free ahead of the GCD; when you are in danger a healing or protecting familiar still comes at once.
- **Soul Crush interrupts.** It goes to a caster whose cast can be interrupted first: 1,000 instead of 300, and 100 TP.
- **Beastmaster keeps the Inner Compass turning clockwise.** When an axe opens a combo for the familiar's Trick to
  close, it is now the axe that follows the standing heart, or with none the one before the Trick's affinity, so the
  Trick closes an intentional combo - "greater additional damage" - instead of a plain one in most cases.
- **Beastmaster captures familiars you do not have yet.** On the last weaponskill before such an enemy falls, Capture
  marks it at the least HP it will have alive; the lower its HP when marked, the likelier the pact. It recognises the
  beast from the game data and asks the game's familiar list, so a familiar you have is never tried again. New option
  "Capture familiars you do not have yet", on by default; it costs one weaponskill per attempt.
- **Beastmaster no longer sends a familiar away with nobody to follow.** In combat a horn's 90 s start only when its
  familiar retreats, so the horn of the familiar out is never its replacement. Parting Blow, and a Tempered Release
  after which the familiar leaves (Wespe's Final Sting), now go only when another horn's familiar is ready; until then
  the familiar keeps fighting. "Only use Snarl and Challenge on Last Horn" reads the same: no other horn ready.
- **Beastmaster: a record of the Master's Bestiary.** While the Bestiary is open, RSR writes to `BestiaryTrace.log`
  next to `DefenseTrace.log` what the game receives from each click, every context menu with its entries, and the
  battlehorn slots as they change. It only listens; it opens and closes with the Bestiary.

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

## Learned tables

- **A learned figure from an earlier game version saves no protection until it is confirmed.** The area and tankbuster
  tables now tell a figure the running game version has measured cleanly from one taken over from an earlier version
  (or from the file before the revisions). A taken-over figure still protects - a big raidwide, a lethal tankbuster, a
  long series are met as before - but it no longer leaves protection out: a small raidwide is mitigated like an unrated
  one, a small tankbuster holds nothing back beside the plan, and help for the tank is not held, until the action lands
  cleanly again. A patch that made a hit harder can no longer take the first cast's cover. Each such case writes a line
  to `DefenseTrace.log`, and the trace's head line counts the confirmed entries.

## Packaging

- **latest.zip holds the plugin and nothing else.** The release package of 7.5.7.13+wsh1 also carried the build's source
  generator (5.7 MB, never loaded), and a local build packed whatever earlier builds had left in its output folder, such
  as the old RotationSolver.Basic.dll. The package is now made from a fixed list - the plugin, its manifest, ECommons
  and RebornMaterial -, and a release is not published unless its zip holds exactly that.
