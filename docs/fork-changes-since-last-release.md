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

## The learned damage table measures at all

The effect handler read the action type of every hit four bytes wide: the type sits in one byte, and
the next ones hold the number of targets. Every hit that struck anyone therefore looked like something
other than an action, and no area action was ever rated - the table stayed empty, and new area actions
were never added to the list. Only the type's own byte is read now.

## The learned damage table no longer loses readings on the way to disk

Three ways a reading could stay in memory, look recorded, and never reach the file: the table was
written from a background thread while the next reading was being added, two saves at once fought
over the same temporary file, and on unload the last save ran while new readings could still arrive.
All three are closed.

The AoE list window now says what the store actually did, under `Store:` — whether the login found a
file, found none, or found one it could not read, and after every save how many entries were read
back from disk. A save only reports success when the file holds what was written.

Below it, `Casts this session` counts every enemy cast that hit you since loading, by what decided it:
measured, not in the AoE list, cast by an untargetable enemy, and so on. The
`Last hit` line now only reports casts; before, the next auto-attack replaced a raidwide's reason
within a second. An empty AoE list, which measures nothing, is shown in red. Above it, `Effect handler` counts what the effect hook delivered since loading - all sets, those from
enemies, those that hit you - and any error the handler raised, with the first error's text.

Measuring no longer depends on `Record AOE actions` or on the party size. That setting decides whether
new actions are added to the AoE list; with it off, or in a party counted below four, the damage of
actions already on the list was not measured either, and the rules that read it treated every cast as
unrated.

A save can no longer lose what the file holds. The table only ever grows by itself, so every save now
merges memory and file, keeping the higher reading for each action. A file that exists but cannot be
read at that moment is left alone (`Store:` shows `NOT SAVED` in red). And a list whose load did not
finish in this session - a cancelled or failed start - is not written on unload, so a failed start
can no longer empty any of the lists.

The button `Forget recorded damage potential` is gone. To start over after a patch has changed how
hard these actions hit, delete `HostileCastingAreaPotential.json` in the plugin's config folder while
the game is closed; with the game running, the next save would write the readings back.
`Reset and Update AOE List` keeps the current list when the download fails, instead of replacing it
with an empty one.

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
- only when at least as many in the radius are hurt as the action's own AoE count asks for (default
  1), on every path including the heal command;
- when your own missing health takes a full heal, or everyone else in the radius does;
- at once when someone in the radius is in danger;
- before Refulgent Lux runs out, whenever anyone in the radius is hurt at all - giving way only to
  Mountain Buster when Titan's Favor would end first and Mountain Buster is enabled.

The heal amount is the smallest full heal seen since the last zone change, so a critical heal never
makes it look bigger than it surely is. A target counts only once its health actually rises by the
heal, so a reading taken after the game had already applied it never enters the figure. The rotation
status shows what the rule says right now, why and when it last chose to cast, the measured amount,
and for the last cast that landed how many targets were confirmed and how much met missing health. Rekindle obeys Scalebound
and Shackled Healing as well.

## Defensive holds give way when the party is in danger

Several rotations hold a defensive action back on purpose: the White Mage and the Astrologian
spread their party mitigation over time instead of spending it all on one hit, and the Dark Knight,
Gunbreaker, Machinist, Dragoon and Viper keep weave slots free in their burst. Every one of these
holds now gives way when a party member is already at the critical health level
(`HealthForDyingTanks`) or the announced area cast, as measured, would put someone there; a hold
of a single-target defense gives way when you or a tank is at that level. An invulnerable tank does
not count, and a tankbuster on a healthy tank is no reason by itself - it is what opens the
single-target defense in the first place. The Dark Knight's own barrier hold follows
the same rule, and so do the Gunbreaker's heals before its No Mercy opener and the Dragoon's
own heals right after Stardiver.

- White Mage and Astrologian wait for as long as the effect that started the wait lasts, taken from
  the effect text, read from the action's own recast rather than from its button. For the
  Astrologian that is 15 s after Macrocosmos and 10 s after Collective Unconscious (formerly 30 and
  20 s).
- The Dark Knight's The Blackest Night and Oblation on a low party member are no longer held in the
  burst window; their settings name no such exception.
- The Bard, Painter and Dancer setting "Prevent the use of defense abilties during burst" is left
  as it reads.
- The Diagnostics window shows, per rule, whether it last held or gave way.

## Channels and changed buttons

- **Paladin and Astrologian channel locks** (`Lock actions when casting Passage Of Arms/Collective
  Unconscious during AOE mitigations`, off by default): with the lock on, weaponskills and spells are now
  held only until the announced hit lands, as abilities already were. Before, the GCD stayed held after
  the hit until some ability broke the channel, up to 18 s.
- **Machinist:** Wildfire can no longer go out as Detonator in its last seconds, cutting the stacks
  still to come.
- **Reaper:** Hell's Ingress and Hell's Egress can no longer go out as Regress, which jumped back to
  the gate instead of dashing.

## Granted windows no longer run out unused

- **All jobs:** An action that needs a status of yours (a proc, a "Ready") was locked out during the last
  two GCDs of that status, because the check read the per-action setting `Number of GCDs before the
  DOT/Status effect is reapplied`. It is usable now as long as the status lasts until the action goes
  off and its cast ends. Procs no longer run out in their last seconds, and the existing
  "use it before it runs out" rules of Red Mage, White Mage, Scholar, Ninja and Pictomancer take effect.
- **Machinist:** With `Only use Wildfire on Boss targets`, Heat is no longer held for Wildfire on
  enemies that are not bosses; Hypercharge comes as soon as the tools allow instead of at 100 Heat.
- **Machinist:** Hypercharged from Barrel Stabilizer is spent before it runs out, also when Wildfire is
  held (for example with `Only use Wildfire on Boss targets` against trash).
- **Samurai:** Ogi Namikiri is used before Ogi Namikiri Ready runs out, also on a boss without Higanbana
  (for example when Higanbana is switched off, or kept off by adds).
- **Gunbreaker:** Sonic Break and Reign of Beasts are used before Ready to Break and Ready to Reign run
  out, also when No Mercy passed without room for them.
- **Ninja:** Phantom Kamaitachi is used before Phantom Kamaitachi Ready runs out, also when no Trick
  Attack or Mug window opens in time.
- **Black Mage:** Retrace is only used while Ley Lines lasts beyond the next GCD.

## Samurai and Machinist use a pause in the fight

- **Samurai:** When no enemy is within 25 yalms in combat, the Samurai casts Meditate while standing
  still, gaining Kenki instead of nothing.
- **Machinist:** With a BossMod module that predicts the boss going untargetable, the Machinist orders
  Queen (or Rook) Overdrive in the last GCD before the pause, if the Queen would otherwise only shut down
  during it. Its finisher then lands instead of hitting nothing.

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

## Leaps count as movement for the BMR safety check

With `Use BMR intergration to verify safety of movement actions` on, Dragonfire Dive, Stardiver and
Forked Raiju are now checked like every other gap closer: they are held when the way to the target
crosses a danger zone, and go out freely when you already stand at the target.

## White Mage: Holy stays held while the pack is slowed

With `HoldHolyWhilePackSlowed` on, Holy was held after an Arm's Length only until every enemy had a DoT
and then went out into a pack that was still slowed. It now stays held for as long as more than half
the enemies in its radius are slowed; the held GCDs go to DoTs and then Glare. The Diagnostics window
shows, under the rotation, whether the rule last held Holy and why not.

## For rotation authors: party mitigation sum and physical damage check

`GetCurrentMitigationPercent` now counts Troubadour, Tactician and Shield Samba at the 15% their effect
text states (it used 10%) and includes Confession from Plenary Indulgence. `IsPhysicalDamageIncoming`
now tests the physical attack types (slashing, piercing, blunt, shot); it used to test sound.

## Healers: the big single-target heals wait for danger

What Benediction already did now applies to every healer: the last charge of Essential Dignity,
Excogitation as a heal and Taurochole as a heal go only to a target in danger - being attacked or cast
at, an area cast announced, or health measurably falling. A player who was just raised and is taking no
damage gets the smaller heals instead. Each has its own setting, on by default.

## Heal ahead of incoming damage: area heals too

With `Heal ahead of incoming damage` on, the area-heal thresholds now read the health the party is
heading for, as the setting says every healing threshold does; they used to read the health shown.
When the party drops fast, the area heal comes about a GCD earlier. With the setting off nothing
changes.

## New setting: Choose the heal target by danger

Off by default. On, it replaces the role short-cuts in the heal target choice, below anyone about to
fall: a healer or tank under their role threshold who is being attacked comes first, lowest health
first and a healer before a tank at equal health; then everyone else - by fewest hit points while an
area cast is announced, otherwise by lowest percentage.

## Sage: a barrier on the tank through a dungeon pull

New setting `Keep Eukrasian Diagnosis on the tank through a pull`, off by default. As the tank closes
in on a group, Eukrasian Diagnosis goes on them and is renewed whenever the barrier runs out or breaks,
for as long as the pull lasts. Both steps are instant, so it works while running. In a large pull a
broken barrier is replaced again and again, and each renewal takes Eukrasia's second and a GCD from
Dosis. Not placed over Galvanize or Eukrasian Prognosis. White Mage, Astrologian and Sage now share one
rule for this upkeep, and none of them spends the MP their raise needs on it.

## Dancer: a new dance partner after a death

When the dance partner dies, the partnership ends and Closed Position picks again. While the partner
carries the weakness of a raise, someone else gets the partner buffs if anyone is available; once it
has worn off, the higher-priority member is taken back. A partner chosen by name is replaced only for
death or weakness. A swap waits until Closed Position is ready and never interrupts a dance.

## Tankbuster markers: more are recognised, and false ones are learned away

Every tank lock-on marker and every shared tank laser is now recognised, not only four named ones,
plus two more party-stack markers. When a marker turns out not to be followed by any hit on the marked
member, its path goes on a list, `TankbusterMarkerFalsified.json`, and is no longer treated as a
tankbuster; a later hit after the same marker takes it off again. The list is learned in play and
saved with the same safeguards as the damage table; delete the file to start over.

## Heal ahead of incoming damage: the jobs' own thresholds too

With `Heal ahead of incoming damage` on, the healing thresholds inside the job rotations now read the
health a member is heading for, as the setting's text says every healing threshold does: Regen,
Essential Dignity, Taurochole, Excogitation, Clemency, Nascent Flash, Second Wind and the others.
Mitigation thresholds are unchanged. With the setting off nothing changes. For rotation authors,
`ObjectHelper.GetForecastHealthRatio` is now public.

## Area casts aimed at someone else reach only those near them

Whether a cast from the AoE list can hit you is now measured from where the game data puts it. A
single-target action with a cast range counts only for the player it is cast at; one such entry,
Holy Bladedance, opened every player's area defence when it was aimed at the tank. A circle cast at
a player - stacks, tankbuster circles on the tank - is measured around that player, not around the
caster, and a line only reaches what lies inside its width. Measured from the caster with the boss's
hitbox taken off, a 6-yalm circle on the tank reached a player on the far side of a large boss and
spent a Summoner's Radiant Aegis and Addle on it. Area defence and healing ahead of the hit both
read the same check, for every job.

Ground-targeted circles, cones and charges are still measured from the caster: the game data does
not state where they land or how wide they open.

`DefenseTrace.log` in the plugin's config folder records, for one session, every action the
defensive chain chose with every source standing at that moment - markers, listed casts with their
shape and distances, BossModReborn predictions - and every enemy hit on you beside it.

## Displacement and Hissatsu: Yaten: the backstep is checked for safety

With the BMR movement safety check on, Red Mage's Displacement and Samurai's Hissatsu: Yaten are no
longer used when the spot they jump back to lies in a danger zone. Before, neither was checked at all.
For rotation authors: `SpecialActionType.HostileAttackBackstep` with `ActionSetting.BackstepDistance`.

## Paladin, Warrior, Gunbreaker: Arm's Length on a pull for its Slow

The Dark Knight's option to use Arm's Length on a group pull for its Slow now exists for every tank,
off by default. The Slow +20% on every enemy that strikes you delays auto-attacks as well as casts. A
pull is as many enemies in reach as the global "Number of hostiles" for defensive abilities; a pack
that is already slowed is left alone.

## A failed list download no longer leaves an empty list for good

When a curated list (the AoE list among them) could not be downloaded on the first start, an empty
list was written to disk and never fetched again. Now nothing is written, and the next start tries
again. An unreadable list file is set aside and downloaded afresh. Downloads during loading give up
with the plugin's load timeout instead of waiting 100 seconds each.

## Dark Knight: Blood is pooled for the burst

Bloodspiller and Quietus now go out under Delirium, in the two-minute window, or when the Blood Gauge
is above 70; otherwise the Blood is kept for the burst, entering Delirium with as much as fits (The
Balance). Before, they went out at 50 Blood whenever they could.

## Paladin: Intervention on the other tank works

"Use Intervention on CoTank during tankbusters" and the Intervention health threshold never fired:
the action required a tank stance on its target that came from the paladin himself, which the other
tank's stance never does. It now accepts the stance from any source.

## Dancer: Improvisation ends with its barrier

Improvised Finish was never cast, so the dance ended with the next action and the party got the regen
but not the barrier. The Dancer now finishes the dance at once, for the 5% barrier.

## Sage: Addersgall is spent before it overflows

At three stacks, just before the next one would be lost, the Sage now spends one on Druochole (on
whoever needs the heal, otherwise on itself) for its 7% MP. The Addersgall timer is now read as the
time elapsed, as the game counts it; it was read the other way round.

## New setting: Skip area defence for casts that missed you

Off by default. On, an area cast from the AoE list that landed last time without damaging you - you
dodged it, or it was centred on someone else - does not open your area defence the next time; once it
damages you again, it counts again. Healing ahead and the party's threat check are unaffected.

