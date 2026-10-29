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
