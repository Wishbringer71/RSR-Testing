# Changes since 7.5.7.10+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Mitigation and defensives

- **A tankbuster of several hits is planned as one.** The tankbuster table now adds up what all hits of one cast take on
  a tank - also hits that land under another id or from a helper, matched to the cast by its name - and keeps the sum
  under the cast the plan reads, with the time from the first hit to the last. The plan sizes its mitigation for the
  sum and counts only mitigation that still stands at the last hit, and it presses the invulnerability late enough to
  last through the series, at the latest one GCD before the cast ends. An invulnerability that runs out before the last
  hit no longer holds the rest of your defence back. Before, the plan saw one hit or none: a three-hit buster got a
  bundle for one hit, and a short mitigation had run out before the big last hit. A tankbuster of one hit is planned as
  before.
- **No spell runs into Pyretic.** A status that damages every action — Pyretic, Causality — comes with the hit that lays
  it, and an action still under way at that moment takes its damage at once. RSR learns, from every landing on you or a
  party member, which enemy casts lay such a status, and before one of them lands on you it starts no spell that would
  still be casting, presses nothing that would still be under way, and cancels a running cast that would meet it — each
  counted with your measured round trip. Instant actions and casts that finish in time go on, and an enemy cast stopped
  before its end releases at once. BossModReborn's imminent Pyretic counts too. The learned casts are kept in
  `ActionPenaltyCasts.json` beside the other learned tables; a status taken off `No-casting debuffs` is no longer learned
  or waited for.
- **A tankbuster on both tanks gets help on both.** A White Mage's Divine Benison and Aquaveil, and an Astrologian's
  barrier after Macrocosmos or Collective Unconscious, wait for each other only on a target that still carries the
  other's effect. Under a tankbuster on both tanks, Aquaveil goes on the tank without Divine Benison at once instead of
  waiting 15 s, and a tank whose barrier the last hit took gets the next help for the next hit.

- **A raidwide that lands in several hits counts mitigation only to its last hit.** RSR learns how long a series of
  hits from one area attack runs - Bodyweight Exorcism's four towers land over 9 s - and counts a party mitigation only
  where it lasts until the last hit. Before, a mitigation that ran out after the first towers counted for all of them,
  and `Skip mitigation for small area casts` could leave yours out while the last towers landed unmitigated.

## Healing — when it lands

- **Every running heal over time counts.** The heal thresholds for "Targets with Heal-over-Time" applied only to the
  heals over time on two fixed lists, so under Kerachole, Celestial Opposition, Aurora or Knight's Benediction RSR healed
  as if nothing were ticking. Any status whose own text says it regenerates HP over time counts now, from whoever laid
  it. A member's single threshold reads the one on him that runs longest; the area threshold reads how much of the party
  is under one, not only whether you are.

## The learned lists

- **The area and tankbuster tables count every reading under the game version it was taken in.** Nothing changes in a
  fight: every rule still reads the highest valid reading. Each reading is now kept, not only one that raises the value,
  in `HostileCastingAreaPotential.rev.json` and `TankbusterPotential.rev.json` beside the old files, which RSR goes on
  writing; after the next patch RSR can tell which values the new version has confirmed. To start one of these tables
  over by hand, delete both of its files with the game closed.
- **A learned table that cannot be read comes back from its backup.** Every learned table keeps a copy of its last save
  that was read back (`.bak`). A file that cannot be read is set aside as before, and the backup is loaded instead of
  starting empty - where every area cast counted as unrated for the session and every tankbuster as unmeasured. A file
  you deleted stays deleted.
- **A tankbuster nothing names is learned from its first hit.** A tankbuster that neither the tankbuster list nor a
  marker names - RSR knows it only because the enemy casts it at its target, a tank - was never measured, so the
  tankbuster plan never took it, and a lethal one killed at every repeat. Its hit on a tank is now learned like any
  tankbuster's, and from the next cast at you the plan spends what it needs, the invulnerability included. Nothing else
  is held for it: in a pull such a cast can come from one of many enemies. It does not count as a known tankbuster for
  opening the defence or for help from others, and a hit that reaches more than one member is not learned as one.
- **A raidwide landing on a tank under Hallowed Ground or Superbolide joins the area list.** The tank's hit carries no
  damage then, and the action stayed off the list - a tankbuster that a patch turned into a raidwide kept drawing the
  tankbuster plan at its next cast.
- **Nothing measured outside the content's sync is learned.** Above the content's level, or in an unrestricted party
  from the duty finder at the same level, every hit is smaller than in sync; learned, it rated the action too small for
  the next synced run, where the mitigation or the tankbuster bundle was then left out. The area table, the tankbuster
  table and the damage over time on the party skip such readings; the open world counts as in sync.
- **An area hit counts at its unmitigated size.** The damage table divides out the mitigation that stood when a raidwide
  landed — on the member and on the boss, by the figures the effect texts state — so a raidwide first seen under three
  mitigations is no longer rated small. Every rule that reads the table — `Skip mitigation for small area casts`, the
  heal ahead of the hit, the White Mage's bell — reads it under the mitigation that will still stand when the cast lands:
  what others have already laid makes the hit smaller, and your own party mitigation goes out where the rest of the hit
  would still push someone to where healing is called for, or is at least the size of a large barrier. Ratings learned
  before are read as they were until the action lands again; a barrier is not divided out.

## Rotations

- **The time an enemy has left is measured again.** The self-correcting time to kill was kept for party members only,
  so it was unknown for every enemy: the Monk's Six-sided Star as the last weaponskill came only before a downtime
  BossModReborn predicts, never before the target died, and the Scholar held Bio by its rough estimate even where the
  target's measured time was long enough. Both now read the corrected time for enemies too.
- **Red Mage: Manafication waits for Embolden only where that costs no use.** Manafication (110 s) waits for Embolden
  (120 s), so it drifts and, depending on the fight's length, the last one before the boss dies was lost. When the
  boss's measured time to kill says the wait would cost that use, Manafication now goes at once. Until then, and while
  no time to kill is measured, it waits as before.

- **Pictomancer: no motif under Starry Muse in the opener.** Below level 92 Starry Muse comes two seconds into the
  fight, inside the opener's first five seconds, and the opener could paint a motif under it - a burst GCD without
  damage. The opener's priorities now end once Starry Muse is up, as the rest of the rotation already does. From level
  92 nothing changes.

## Stability

- **No crash from RSR's windows at a loading screen.** When RSR rebuilt its window fonts after a zone change - at
  character select or a teleport - a font still being drawn could already be released, and the game crashed in the
  control window. RSR now uses RebornMaterial 7.5.7.5, which keeps each font until the frame is drawn.
