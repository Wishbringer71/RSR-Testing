# Changes since 7.5.6.10+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## A gap closer is no longer withheld when there is nothing to close

The BossModReborn movement check (`Use BMR intergration to verify safety of movement actions…`,
off by default) asks whether the path a dash would take is safe. Standing inside the target's
hitbox there is no path — the dash ends at the hitbox edge, which is behind you — but the check
measured the line to the boss's **centre** anyway. On a large boss that is several yalms through
its own footprint, so an area under the boss withheld the ability from a player already standing
in it.

On Summoner that costs two GCDs of the rotation rather than one: Crimson Strike needs the status
Crimson Cyclone grants, so when the dash is refused the follow-up never becomes available either,
and every Ifrit phase falls back to filler.

The run-up itself is unchanged: `Use damaging gap closer abilites if the distance to your target
is less than this` still gates it at 3 yalms by default, and as soon as any distance remains the
path is measured exactly as before.

## The strongest HP potion is the one that gets used

The choice between enabled potions took the last one examined on a tie, and the list runs from
strongest to weakest — so the weak grade won. Ties are the normal case, not an edge: a potion
heals a percentage up to its own cap, and wherever the percentage is what binds, every usable
grade answers the same figure. The potion you deliberately enabled sat in the bag while a
low-grade one was spent.

The item's debug panel now also says in words why no potion goes out — the setting, this item's
own enable switch, the health threshold, the missing-health guard, the bag, the game's own
refusal — and whether anything is asking for one at all. `CanUse: False` was one bit for six
separate conditions.
