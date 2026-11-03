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
- **Beastmaster counts a pack stopped as protection, and an element-only shield no longer.** In a danger, a familiar
  whose Tempered Release stops the enemies it reaches - Stun (War Cry), Petrification (Petribreath), Deep Freeze (The
  Ram's Voice) - is called like one that protects, when two or more enemies attack you, all within its reach, none a
  boss and, for the stun, none stunned since its last immunity. Strut and Formic Pheromones, which lower only earth or
  water, earth and ice damage, no longer count as protection. A Release that deals damage over time to the party
  (Hypodermic Hustle) waits while you are in danger.
- **Summoner keeps summoning while a follow-up is held back.** Upstream's new "Do not summon a new primal while attuned
  or holding Ifrit's/Garuda's Favor" is removed here, as is its "Dynamic primal order": the follow-ups already go first
  whenever they can, so the block only waited where movement or the Crimson Cyclone distance held one back - with that
  distance kept, an unused Ifrit's Favor held every later primal until the next demi-summon. "Prefer Titan while
  moving" and "Summon Ifrit first while Crimson Cyclone can follow" cover the order.
- **Summoner summons Ifrit first while Crimson Cyclone can follow.** Ifrit's Favor ends with the next summon, and as
  the last primal Ifrit lost Crimson Cyclone and Crimson Strike to the demi-summon in most rounds. New option "Summon
  Ifrit first while Crimson Cyclone can follow" (on by default): while you stand within your Crimson Cyclone distance,
  Ifrit comes ahead of the configured order and both go out at once; Garuda then comes last. Not under Searing Light,
  not while moving unless "Use Crimson Cyclone when moving" is on, and not with "Use Crimson Cyclone at any range" on.
  The Cyclone never goes beyond your distance setting.
- **Beastmaster: a record of the Master's Bestiary.** While the Bestiary is open, RSR writes to `BestiaryTrace.log`
  next to `DefenseTrace.log` what the game receives from each click, every context menu with its entries, every server
  command the client sends through its general command function, and the battlehorn slots as they change. It only
  listens and passes every command on unchanged; it opens and closes with the Bestiary. `DefenseTrace.log` also
  gets a "beastmaster state" line whenever what the horn choice and Capture read changes: the familiars on the horns,
  the one out, both options, whether Capture is enabled, and whether the familiar list and catalog are loaded.

## Tinctures and delays

- **A tincture in every burst window of a hard fight where it is ready and in your inventory.** A tincture is for
  coming through a hard fight and ending it sooner, and it costs money. Only "Tincture usage" decides where (never,
  high-end duties - the default -, anywhere); high-end is what the game marks so: the current Savage tier, the current
  Extreme and Unreal trial, the Chaotic raid, the Ultimates. Older Savage and Extreme fights are not synced to their item
  level and get none, nor does a training dummy any more under "high-end duties". Gunbreaker, Dark Knight, Bard and Machinist drank one only in a countdown - with no
  countdown, and in every later burst such as the sixth minute, never; they now drink it under No Mercy, in the
  two-minute window, under the Bard's buffs and under Wildfire. Viper and Machinist had a switch of their own, off by
  default, and never drank one. The rotations' own tincture switches are removed: Summoner's "Use potion", "Potion
  timing" and "Only use potion during a big summon", White Mage's "Use Tincture/Gemdraught when about to use Presence
  of Mind", Viper's "Experimental Pot Usage", Machinist's "Use burst medicine in countdown".
- **Tinctures and potions share one recast.** "Tincture usage", "Use HP potions" and "Use MP potions" now say so: a
  tincture blocks HP and MP potions for 5 minutes, an HP potion the tincture for its own recast.
- **Potions can wait for the tincture.** New option "Hold HP and MP potions for the tincture" (per job, off by
  default): on, HP and MP potions wait where your rotation drinks tinctures, "Tincture usage" allows one and one is in
  your inventory, so the tincture always comes in its burst; an HP potion against a tankbuster on you still goes. Off,
  a potion goes at its threshold as before, even if the tincture then misses its burst. Potions of Eureka and the deep
  dungeons have a recast of their own and never wait.
- **Swiftcast stays ready for the raise.** While a job's raise is learned and "Raise with Swiftcast or Dualcast" is on,
  damage no longer takes Swiftcast unless you switch it on yourself: Red Mage used it for Verthunder and Veraero in
  every burst and for any hard-cast Verthunder III or Veraero III; it now does so only with the new option "Use
  Swiftcast for damage" (off by default), so a dead member is raised at once even while you move. Summoner's Swiftcast
  switches already were off by default.
- **With "Raise with Swiftcast or Dualcast" off, a running Swiftcast is not wasted.** A Swiftcast or Dualcast already
  running now goes to your next spells, as the setting says, and into a pending raise only when it would otherwise run
  out unused. Before, the raise took it at once: a Summoner who pressed Swiftcast for Slipstream while moving lost it to
  Resurrection. The healers' "Swiftcast restriction" holds their other GCDs only while the raise takes it, so a healer
  who presses Swiftcast by hand at a corpse keeps casting.
- **Delays never make an action too late.** The provoke delay does not apply when taking an enemy from a co-tank, and
  otherwise ends with the enemy's first hit on the party member it turned to. The dispel delay ends when a dangerous
  status would run out within a GCD. The interrupt delay already ended with one GCD of the cast left. A stored interrupt
  or provoke delay still at the old default of 0.5 to 1 s is set to none on load.

## Movement

- **Fixed dashes check where they land.** Hell's Ingress, Hell's Egress, En Avant, Smudge, Aetherial Shift and Elusive
  Jump - the dashes without a target - went out on "Move Forward" or "Move Back" wherever they would land. With
  BossModReborn running they now wait while the landing point lies in a danger zone, for as long as the command lasts
  ("Special state duration", 3 s by default), like every dash onto a target already did. "Skip BossModReborn
  position-safety check for this movement action" in the action's settings turns it off for that action.
- **No ranged attack right after every dash to the target.** Lightning Shot after Trajectory, Writhing Snap after
  Slither, Tomahawk after Primal Rend, Piercing Talon after Stardiver or Winged Glide and Throwing Dagger after Forked
  Raiju are held for one action, as they already were after Intervene, Onslaught or Dragonfire Dive: the player stands
  at the target now. After a jump backwards (Elusive Jump) the ranged follow-up stays allowed.

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
- **A hit a patch weakened lets go of its old figure.** In the area and tankbuster tables the running game version now
  refutes a figure taken over from an earlier one: a fixed hit after one clean landing more than a damage roll below it,
  a varying one after as many clean landings as the old figure had, and a figure from the file before the revisions only
  after three clean landings that agree with each other. From then on the new figure counts, protecting and saving; a
  harder hit raises it again at once. The old figure is kept, not read, and the old file still holds the highest of all
  readings. Each refutation writes a line to `DefenseTrace.log`.
- **A tankbuster marker locked in an earlier game version counts again.** A marker the fight once showed with no hit on
  the marked member is locked as "no tankbuster" only in the game version that saw it. After a patch, and on the first
  start of this build, it counts as a tankbuster until it goes without a hit again - one defence per marker and version
  for a tank that a patch may have turned it against.
- **Sleep locks from an earlier game version get one try.** An enemy kind that resisted a sleep, a sleep limit, and an
  action a sleep did not stop held for good, since those files only grow. Now each holds in the game version that saw
  it; one from an earlier version, or from before this build, gets one sleep in the running version - only when no heal
  spell is due and nobody is in the critical class, so the try never costs a heal. A resistance holds it again until the
  next patch; a sleep that lands, from anyone, ends the lock for a kind or a limit, and a stopped cast for an action.
  Each try, confirmation and refutation writes a line to `DefenseTrace.log`, and the head line counts the locks.
- **A cast that no longer lays Pyretic stops holding spells.** A learned action that lays a status damaging every
  action keeps holding spells after a patch. It lets go only when the running version sees it land on its target, a
  party member, without the status at least twice, more often than it laid it before, and never with it; it holds again
  the moment it lays the status. BossModReborn's Pyretic announcement holds regardless.
- **The learned tables can be seen.** The list window has a new section, "Learned tables", with one tab per store:
  area hits, tankbusters, area and hit series, sleep outcomes, sleep-resistant kinds and limits, damage over time,
  casts laying Pyretic and tankbuster markers. Each entry shows the value the rules read and its state in the running
  game version - confirmed, taken over, refuted, a lock with its try left or spent. Search and copy; nothing can be
  changed there.
- **The marker and lock files keep a backup.** The tankbuster marker files and the new lock files fall back to their
  last read-back copy when unreadable, so a measured hit after a marker is not lost with a broken file.

## Packaging

- **latest.zip holds the plugin and nothing else.** The release package of 7.5.7.13+wsh1 also carried the build's source
  generator (5.7 MB, never loaded), and a local build packed whatever earlier builds had left in its output folder, such
  as the old RotationSolver.Basic.dll. The package is now made from a fixed list - the plugin, its manifest, ECommons
  and RebornMaterial -, and a release is not published unless its zip holds exactly that.
