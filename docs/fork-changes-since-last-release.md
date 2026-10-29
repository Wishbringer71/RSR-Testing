# Changes since 7.5.7.10+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Mitigation and defensives

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

## Healing — when it lands

- **Every running heal over time counts.** The heal thresholds for "Targets with Heal-over-Time" applied only to the
  heals over time on two fixed lists, so under Kerachole, Celestial Opposition, Aurora or Knight's Benediction RSR healed
  as if nothing were ticking. Any status whose own text says it regenerates HP over time counts now, from whoever laid
  it. A member's single threshold reads the one on him that runs longest; the area threshold reads how much of the party
  is under one, not only whether you are.

## The learned lists

- **A tankbuster nothing names is learned from its first hit.** A tankbuster that neither the tankbuster list nor a
  marker names - RSR knows it only because the enemy casts it at its target, a tank - was never measured, so the
  tankbuster plan never took it, and a lethal one killed at every repeat. Its hit on a tank is now learned like any
  tankbuster's, and from the next cast at you the plan spends what it needs, the invulnerability included. Nothing else
  is held for it: in a pull such a cast can come from one of many enemies. It does not count as a known tankbuster for
  opening the defence or for help from others, and a hit that reaches more than one member is not learned as one.
- **A raidwide landing on a tank under Hallowed Ground or Superbolide joins the area list.** The tank's hit carries no
  damage then, and the action stayed off the list - a tankbuster that a patch turned into a raidwide kept drawing the
  tankbuster plan at its next cast.
- **Nothing measured above the content's level sync is learned.** In an unrestricted party in older content every hit is
  smaller than in sync; learned, it rated the action too small for the next synced run, where the mitigation or the
  tankbuster bundle was then left out. The area table, the tankbuster table and the damage over time on the party skip
  such readings; the open world counts as in sync.
- **An area hit counts at its unmitigated size.** The damage table divides out the mitigation that stood when a raidwide
  landed — on the member and on the boss, by the figures the effect texts state — so a raidwide first seen under three
  mitigations is no longer rated small. Every rule that reads the table — `Skip mitigation for small area casts`, the
  heal ahead of the hit, the White Mage's bell — reads it under the mitigation that will still stand when the cast lands:
  what others have already laid makes the hit smaller, and your own party mitigation goes out where the rest of the hit
  would still push someone to where healing is called for, or is at least the size of a large barrier. Ratings learned
  before are read as they were until the action lands again; a barrier is not divided out.
