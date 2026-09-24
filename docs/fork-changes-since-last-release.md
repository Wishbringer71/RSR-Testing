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

## The learned damage table no longer loses readings on the way to disk

Three ways a reading could stay in memory, look recorded, and never reach the file: the table was
written from a background thread while the next reading was being added, two saves at once fought
over the same temporary file, and on unload the last save ran while new readings could still arrive.
All three are closed.

The AoE list window now says what the store actually did, under `Store:` — whether the login found a
file, found none, or found one it could not read, and after every save how many entries were read
back from disk. A save only reports success when the file holds what was written.

## Summoner: Searing Light no longer drifts away from the burst phase

Reported from play as the only Summoner in the party: Searing Light slipped further back the longer
the fight ran. Two conditions produced that between them. The buff was only released once the big
summon's cooldown had already run out — and that happens on a GCD, so the weave slot ahead of it had
passed and the summon waited a GCD. And a buff a second or two short of ready counted as done, so the
summon went without it, the buff followed inside the phase, and its next cooldown ended later still.

The buff is now released as soon as the summon will be ready by the next GCD. When it still cannot
go out ahead of the summon — a second or two short, or Ruby Rite being cast right before with no room
for it — the summon waits for it; that GCD goes to your current primal, no attunement is finished on
purpose. Solar and Searing Light move together, so your burst stays whole, and a buff that fell behind
once is pulled back instead of trailing every later Solar phase. With another Summoner in the party the
summon never waits for a buff that is still cooling down or blocked by his.

## Summoner: Radiant Aegis goes out before a demi, not never

Radiant Aegis can only be cast while Carbuncle is out, and every demi replaces Carbuncle for 15
seconds. When a raidwide was announced, Searing Light used to take the last weave slot before the
summon and the shield was locked out for the whole phase. Now a due shield goes first, and the big
summon waits for it if there is no slot. "Due" means a raidwide BossModReborn announces, or the
defense flag with no shield of yours up.

## Summoner: Searing Light stays with Solar Bahamut when you are the only Summoner

The slot ahead of the summon now only opens for the burst demi (Solar Bahamut; Demi-Bahamut below
level 100), unless another Summoner is in the party. A charge that had come loose from Solar no
longer goes ahead of Bahamut or Phoenix, and those summons no longer wait for it.

## Summoner: several Summoners — the fallback into a primal block works as intended

With every burst phase taken by other Summoners, your charge falls back into the Titan block — or
the Ifrit block when you already stand within Crimson Strike's reach of your current target. Fixes to
how the rotation decides that: Bahamut and Phoenix now count as one pair at level 100 (a
Summoner seen in one returns in the other), in level-synced duties it no longer asks for a Solar
phase that does not exist there, it only falls back once no Searing Light is running at all, and
"standing at the target" is now measured to your current target with the game's own melee reach
instead of a setting and whatever target Crimson Cyclone had last.

## New: Diagnostics window

Settings → UI → Windows → **Show Diagnostics Window** (off by default). A small window that stays open
in combat and shows, while you fight, what the fork's rules decide from: the current rotation's
status (for the Summoner: whether the next summon opens the burst, whether another Summoner is
recognised, which phases other Summoners hold, what the big summon is waiting for and for how long
this fight, and where your last Searing Light landed against the big summon),
the AoE damage table's store state and last rated hit, and for every enabled HP potion why it is or
is not used.

## The AoE list says why the last hit was not rated

`Last hit:` names the reason the last enemy action that damaged you was or was not measured — the
recording switch, a party counted below four (NPC companions only count with the NPC party-member
setting), an instant action, or an action not yet in the AoE list.
