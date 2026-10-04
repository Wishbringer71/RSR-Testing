# Changes since 7.5.6.13+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## White Mage: the second Holy waits for the first stun to end

`Stretch the Holy stun` (on by default) now does what it says on every pull: right after Holy, the
next GCD goes to another spell while Holy's stun would still be running when a second Holy lands, so
the second stun starts where the first ends instead of overwriting it. Four conditions used to let
the second Holy go out at once: no DoT left to place, one enemy in the radius not yet stunned, fewer
enemies than `Enemies in Holy's radius before holding`, and - the likeliest - a stun that had not yet
reached the enemies when the next GCD was chosen, since Holy's cast and recast are equally long.

The held GCD goes to a DoT where one is due, otherwise to Glare: one GCD of Holy damage per pull for
about 7 instead of 5.5 seconds of stun. `Enemies in Holy's radius before holding` now only applies to
holding Holy for The Blackest Night and sits under that setting.

## `DefenseTrace.log` keeps every session and names its build

The trace is no longer replaced when the plugin loads, so a rebuild or reload keeps the sessions
before it. Each session starts with its date and time and the commit the plugin was built from, so
every section can be matched to the exact code that wrote it. Delete the file to start over. A chosen defence is followed by a
`used` line when it actually goes out, so a choice the game did not carry out can be told from a second use.

## Own cooldowns stay for hits that reach you

A tank's single-target defence opened for every tankbuster BossModReborn announced and for every cast an
enemy aimed at its own target, whoever that was - so Damnation and Rampart went out for a buster on the
other tank that BossModReborn had marked as not hitting you. Defences that protect only you - Rampart,
Damnation, Raw Intuition, Arm's Length, a barrier on yourself - now go out only when the hit that opened
the defence reaches you, in the single-target and the area defence alike and for every job. Help for others
stays: Reprisal, Shake It Off, Intervention and Heart of Corundum on the tankbuster's target are not held
back. An announced tankbuster whose targets BossModReborn does not name still counts as hitting a tank.

`DefenseTrace.log` now also names the pull rule (enemies on you) and unlisted casts at a tank's target as
sources, says whether the single hit reaches you, and records each landing of a listed area cast with
whether it reached you.

## Tanks: Arm's Length no longer goes out on a boss's tankbuster

Arm's Length does not soften the hit that strikes you; it slows the attacker. When the single-target
defence found nothing else for an announced tankbuster, it cast Arm's Length anyway, with a boss alone in
the arena, and the action was then missing for the next knockback. It now goes out from that path only
for its Slow on a pack of ordinary enemies, as many as `Number of hostiles`, and bosses no longer count
towards that pack in any tank's `Use Arm's Length on a pull for its Slow`. For Paladin, Warrior, Dark
Knight and Gunbreaker that setting now has the last word: the defence no longer casts Arm's Length after
the rotation has declined it.

## Warrior: Thrill of Battle and Bloodwhetting before a tankbuster

Before an announced tankbuster on you, the Warrior's single-target defence now casts Thrill of Battle
(`Use Thrill of Battle before a tankbuster on you`, on by default). It raises your maximum HP by 20 %, and
the healing that follows the hit is 20 % stronger. It stacks with Bloodwhetting instead of waiting behind
it. Before, Thrill of Battle went out only as a heal below its threshold, after the hit.
`Use Bloodwhetting/Raw intuition on single enemies` is now on by default, so Bloodwhetting also goes out
for a lone boss's tankbuster, not only after it as a heal.

Bloodwhetting no longer holds the big mitigations back. While it ran, the defence stopped, and it counted
as one of the big mitigations that Rampart and Damnation wait for: at the start of a pull Damnation and
Reprisal came eight seconds late, and a tankbuster Bloodwhetting went out for got no Damnation or Rampart
at all. Rampart and Damnation still stagger against each other.

On a pull, Thrill of Battle now also goes out as soon as your health, falling at the measured rate, would
pass `Thrill Of Battle Heal Threshold` within its ten seconds (`Use Thrill of Battle when your health will
fall below its threshold within its duration`, on by default), so the extra health and the stronger healing
are there while the pack is at full strength. Where the healers hold you steady, it waits as before.

## Tanks: one big mitigation per predicted tankbuster, first in line

When BossModReborn predicts a tankbuster on you, every tank now casts its big mitigation (Damnation,
Guardian, Shadowed Vigil, Great Nebula and their lower forms) before the smaller ones, and Rampart only
when the big one is not taking that tankbuster. Both used to go out on the same tankbuster: with
tankbusters about a minute apart, one hit landed under both and the next under neither. Now they
alternate, and the short mitigations come on top as before.

Reprisal in the single-target defence now waits for an announced raidwide when the tankbuster is on you,
your own big mitigation or Rampart is already running, and the raidwide lands after Reprisal would have
run out and before it is back. Reprisal on the enemy covers every hit it deals for 15 s; it only waits
when it cannot cover both.

## GCD length while no GCD is running

Between GCDs - out of range, between pulls, before the first GCD - the plugin read the GCD length as 0.
Everything measured in GCDs then collapsed: heal-ahead looked nowhere, the one-GCD window in which a
running enemy cast is answered was closed, and the first-GCDs-of-combat guards did not hold. The last
GCD length now stands in while none runs.

## Warrior: Shake It Off keeps your Damnation for the tankbuster

Shake It Off dispels Damnation, Bloodwhetting and Thrill of Battle. As a heal for a single party member
it now waits while Damnation or Bloodwhetting runs, or while a raidwide is announced after its barrier
would end and before it is back. At a raidwide it waits only when a tankbuster on you lands before
Damnation ends. Both give way when a party member is about to die. Dispelling Thrill of Battle is
allowed: the barrier is sized on the raised maximum HP.

## Summoner: Searing Flash no longer expires unused

Outside a demi, Searing Flash waited for a dying boss. With other Summoners in the party, Searing Light
can fall into a Titan or Ifrit block, the next demi is further off than Ruby's Glimmer lasts, and Searing
Flash was lost. It now goes out in the last weave slot before Ruby's Glimmer runs out. As the only Summoner
nothing changes: Searing Flash falls inside Solar Bahamut as before.

## Paladin: Passage of Arms holds until the hit lands, by default

`Lock actions when casting Passage Of Arms during AOE mitigations` is now on by default. Passage of Arms
ends with the paladin's next action, and RSR casts it only for an announced area hit; without the lock the
next GCD or weave ended the channel, often before the hit, and the cooldown was spent for nothing. With the
lock, RSR holds its actions until the announced hit has landed and no longer. A configuration that was
saved before keeps its value: switch the setting on under Extra if it shows off.

## Sage: the pull barrier on the tank is on by default

`Keep Eukrasian Diagnosis on the tank through a pull` is now on by default. In dungeons Eukrasian Diagnosis
goes on the tank as they close in on a group and is renewed whenever it runs out or breaks, for as long as
the pull lasts, never below the MP for Egeiro. In a large pull, where the barrier breaks within a few hits,
that takes most of the sage's GCDs.

## Tanks: Shirk hands the boss to your co-tank when the next tankbuster would kill you

New setting `Shirk the co-tank after a tankbuster that leaves you in danger`, on by default. With another tank
in the party, RSR Shirks that tank after a tankbuster on you if both hold:
- the buster left you with a vulnerability debuff, or so low that a repeat of the hardest buster measured would
  kill you;
- your invulnerability is not ready.

It does this only when the party's enmity on the boss shows the co-tank will take it over. Shirk moves a quarter
of your enmity, so below half of yours nothing would change. A co-tank who is in danger himself - a
vulnerability, or too low to survive the same buster - gets nothing, even if he just Shirked to you. With several
candidates it picks the tank the fewest enemies are attacking, then the one with the most HP.

Once your co-tank has taken the boss's next tankbuster, your debuff has run out, you are above that line again
and your invulnerability is ready, RSR provokes the boss back. While you are still in danger, the automatic Provoke no longer takes the boss off a co-tank who drops
low. `DefenseTrace.log` names every tankbuster on you,
why a swap held, and whether the boss moved; a Shirk that did not move it raises the bar for the rest of the
fight.

The manual Shirk command no longer targets you yourself when you are the tank in stance.

## Tankbusters are learned, from every player they hit

RSR now keeps a table of tankbusters, like the one for area attacks. Every hit of a listed tankbuster, or of one
whose marker was confirmed, counts on any player: you, your co-tank, or other parties in large content.
- Each hit is scaled back to unmitigated by the mitigation that stood on the target and on the boss, and stored
  per action as a share of maximum HP.
- A hit taken under a vulnerability debuff is kept apart.

The tank swap now also uses this table to judge whether a repeat would kill you, so it knows a tankbuster from an
earlier evening or from your co-tank. The list shows each entry's figure and the store's state, and
`DefenseTrace.log` names every measurement. To start over after a patch, delete `TankbusterPotential.json` while
the game is closed.

Hits from an enemy carrying a Damage Up status are not stored: they would rate the action too high.

## Tanks: the invulnerability goes out before a tankbuster that would kill you

New setting `Use the invulnerability before a tankbuster that would kill you`, on by default. When a tankbuster is
cast at you that the table rates lethal even at full HP, with every mitigation and barrier of your own that could
still be used and everything already standing, RSR uses Hallowed Ground, Holmgang, Living Dead or Superbolide
before it lands - no earlier than its ten seconds less one GCD before the hit. Until now it went out only once your
HP had dropped under the dying threshold, which a hit that kills from full never passes. What other players might
add is not counted. Tankbusters announced only by a marker or by BossModReborn, and actions not yet measured, keep
the old behaviour: neither names the action, so there is no figure to judge by. After Living Dead the healers still
have to restore your full HP within Walking Dead.

New setting `Hold other mitigation while the invulnerability covers the hit`, on by default. While the
invulnerability is committed to such a tankbuster, or Hallowed Ground or Superbolide keeps the coming hit off you,
the single-target defence spends nothing aimed at you - no Rampart, no Vengeance, no own barrier, no Reprisal -, so
they are ready for the next tankbuster. Help for another party member still goes out, and so does area defence. If
the invulnerability has not gone out by the last GCD before the hit, the hold opens. Under Holmgang and Living Dead
a hit still takes HP down to 1, so outside the committed tankbuster nothing is held there. As a healer, RSR spends no
single-target mitigation for a tankbuster cast at a tank who stands under Hallowed Ground or Superbolide past the
hit. `DefenseTrace.log` writes the verdict for every tankbuster cast at you and what it was built on.
