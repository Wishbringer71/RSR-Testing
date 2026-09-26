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

"Standing at the target" means 0 yalms between the hitboxes, the distance the game shows, not only
your centre inside the target ring. The run-up itself is unchanged and is gated by the job's own
settings - on Summoner `Use Crimson Cyclone at any range…` (on by default) and `Max distance you
can be from the target for Crimson Cyclone use`. The Diagnostics window shows the last movement
action the check withheld, and why.

## HP potions answer to their own settings, not to the heal flag

A potion was only offered while `AutoStatus.HealSingleAbility` stood — the flag that says whether
this job should be casting a healing *action* right now. It therefore inherited every condition
behind that flag - auto-heal, the heal-as-non-healer switches, the time-to-kill cut-off. With `Only
heal as a non-healer if there are no healers` on, for example, a damage dealer or tank in a party with a living healer
never gets the flag at all. A Summoner cut to 1 HP by a mechanic could not reach a potion, however
its own three switches were set.

The potion now carries its own decision: the global setting, the per-item enable, its own HP
percentage, the missing-health guard and having one in the bag — plus being in combat, since out
of combat health comes back by itself. A confirmed or predicted tankbuster still drops the
percentage as before.

## The HP potion that gets used is the one that actually restores more

More healing wins, and at equal healing the lower grade wins - read from the item level, not
the id. Both halves are now stated in the comparison instead of following from the order the
potion list happens to have. Super-, Hyper- and Ultra-Potion all restore 25 %, so wherever the
percentage binds they tie and the Super-Potion goes first. The amount is now read for the form
that will actually be drunk - HQ when the bag holds one, else NQ (20 % instead of 25 % for those
three) - so an NQ potion is no longer held back as if it healed more than it does.

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

## Summoner: several Summoners — how the fallback into a primal block decides

With every burst phase taken by other Summoners, your charge falls back into the Titan block — or
the Ifrit block when you stand at your current target, 0 yalms hitbox to hitbox, so Crimson
Cyclone does not move you. Fixes to how the rotation decides that: Bahamut and Phoenix now count
as one pair at level 100 (a Summoner seen in one returns in the other), in level-synced duties it
no longer asks for a Solar phase that does not exist there, it only falls back once no Searing
Light is running at all, and "standing at the target" is measured to your current target, the
same way the movement check measures it, instead of a setting and whatever target Crimson Cyclone
had last. The rotation status shows your distance and which block is allowed.

## Area heals centred on you read the need where they land

Medica, Helios, Succor, Lux Solaris and the other area heals around the caster only went out on
the heal path when someone within 0 yalms - usually the caster - was under the action's heal
threshold. A healer standing full a few yalms from a hurt party never cast them. The need is now
read on the hurt, living party members inside the heal's radius. The AoE setting is about attacks
only: on `Cleave` a healer now casts group heals and party mitigations again (`Off` already let them
through). The Diagnostics window shows
the last such heal that was weighed.

## Summoner: Rekindle's fallback waits for the end of the Phoenix phase

The last-resort Rekindle read Firebird Trance, a status otherwise read only for a PvP action; if
the game does not set it in PvE, the fallback fired in the first weave slot of the phase instead of
near its end. It now reads the
phase from the job gauge. Everlasting Flight and Undying Flame now count as running HoTs, so the
heal thresholds lower under them as under any other regen.

## New: Diagnostics window

Settings → UI → Windows → **Show Diagnostics Window** (off by default). A small window that stays open
in combat and shows, while you fight, what the fork's rules decide from: the current rotation's
status (for the Summoner: whether the next summon opens the burst, whether another Summoner is
recognised, which phases other Summoners hold, what the big summon is waiting for and for how long
this fight, and where your last Searing Light landed against the big summon),
the AoE damage table's store state and last rated hit, the last area heal around you that was
weighed, the last movement action the safety check withheld, and for every enabled HP potion why
it is or is not used.


The close button in the title bar now turns the window off - as do Escape and the gamepad back
button while it is focused. Before, the next frame opened it again and only the setting could close
it. The same holds for the Control and Cooldown windows.
## Dark Knight: Walking Dead is trusted to his own attacks first

Under Walking Dead the dark knight heals himself by attacking and does not drop below 1 HP from most
attacks. Healers no longer answer the 1 HP with Benediction or Cure II: at first he only gets a HoT
(Regen goes out despite its health threshold). Full healing starts once the timer is running out,
no enemy is in reach of his weaponskills, BossMod announces a downtime before the end, a tank
limit break is up on the party, or his own course measured since the window began
will not reach full health in time. The Diagnostics window
shows which of these holds.

## Summoner: Lux Solaris follows one rule on every path

Lux Solaris is a point-blank heal around you that expires unused with Refulgent Lux. It now goes out
only when it does some good, measured inside its radius, and the same way whether the heal flag, the
damage branch or the expiry asks for it:

- never under Scalebound, under Shackled Healing with others near, or while a Dark Knight in its
  radius is waiting for Living Dead to trigger (it waits for Walking Dead and then heals everyone,
  the tank included);
- when your own missing health takes a full heal, or everyone in the radius does;
- at once when someone in the radius is in danger;
- before Refulgent Lux runs out, whenever anyone in the radius is hurt at all - giving way only to
  Mountain Buster or Searing Flash when their status would end first.

The heal amount is the smallest full heal seen since the last zone change, so a critical heal never
makes it look bigger than it surely is. The rotation status shows why Lux Solaris last went out or
not, the measured amount, and how much of the last cast met missing health. Rekindle obeys Scalebound
and Shackled Healing as well.

## Hits and heals above 65,535 points are read in full

The effect handler read the low 16 bits of every damage and heal amount. A raidwide that hit a
tank for more than 65,535 points was measured far too small and could be rated a small area cast,
which is not mitigated. Amounts are now read in full; a reading stored too small corrects itself on
the next hit, because the store only ever raises a value.

## The AoE list says why the last hit was not rated

`Last hit:` names the reason the last enemy action that damaged you was or was not measured — the
recording switch, a party counted below four (NPC companions only count with the NPC party-member
setting), an instant action, an action type or category that is not rated, or an action not yet
in the AoE list - and for a listed action the share of maximum HP it was measured at, or that
every hit arrived at zero.
