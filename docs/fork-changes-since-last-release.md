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

## HP potions answer to their own settings, not to the heal flag

A potion was only offered while `AutoStatus.HealSingleAbility` stood — the flag that says whether
this job should be casting a healing *action* right now. It therefore inherited every condition
behind that flag, and one of them is the common case rather than a corner: with `Only heal as a
non-healer if there are no healers` on, a damage dealer or tank in a party with a living healer
never gets the flag at all. A Summoner cut to 1 HP by a mechanic could not reach a potion, however
its own three switches were set.

The potion now carries its own decision: the global setting, the per-item enable, its own HP
percentage, the missing-health guard and having one in the bag — plus being in combat, since out
of combat health comes back by itself. A confirmed or predicted tankbuster still drops the
percentage as before.

## The HP potion that gets used is the one that actually restores more

More healing wins, and at equal healing the lower grade wins. Both halves are now stated in the
comparison instead of following from the order the potion list happens to have.

The tie is a real case, not an edge. What a potion restores is the smaller of its own percentage
of your maximum health and its own cap — and under a level sync the percentage is what binds. Two
grades that state the same percentage then restore exactly the same amount, and drinking the
expensive one buys nothing. Where the grades differ, they differ in the figure too, and the
stronger one is picked on its merits.

The item's debug panel shows `MaxHP` per potion, which is that figure for your character as it
stands right now — so in a synced duty you can see whether two grades really are equal.

The item's debug panel now also says in words why no potion goes out — the setting, this item's
own enable switch, the health threshold, the missing-health guard, the bag, the game's own
refusal — and whether anything is asking for one at all. `CanUse: False` was one bit for six
separate conditions.
