# Changes since 7.5.7.6+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Hits from a boss's invisible helpers are read

Most trials, raids and many dungeon bosses hit through invisible helpers. RSR did not read the kind of figure they are,
so none of their hits was measured: in a few evenings 127 of their actions went unread, up to 88 % of maximum HP on
eight party members, their raidwides never joined the AoE list, and tankbuster markers before their hits were learned
as markers no hit follows. They are read now, and the next hit takes such a marker off that list again.

## A tankbuster marker on a casting member is no longer cut short

A marker counted only for as long as the marked member's own cast still ran: on a healer with 1.5 s of a spell left,
1.5 s instead of five. Defences reading the marker saw it go early, and the watch that learns which markers no hit
follows could close before the hit and learn a real tankbuster away. The marker now keeps its full window, and the
watch also waits for the casts of the boss's invisible helpers. DefenseTrace.log now names whom a chosen defence goes
to, and writes a line whenever a small area cast is let through without party mitigation.

## A tankbuster marker is no longer dropped every other time

When a marker's tankbuster had confirmed it, the same marker, still showing, was watched a second time; with no second
hit it went on the list of markers no hit follows. Its next appearance then got no defence that hung on the marker
alone, and that hit took it off the list again - every other time, for healers and tanks alike. A confirmed marker is
now watched until it has gone.

## Tanks: a marker without a cast is met with the hit that followed it before

A tankbuster marker on the tank with no cast at all got one big mitigation, and the invulnerability only once the tank
had fallen below its line - too late for a hit that takes a full tank under it. RSR measured that hit but never used
the measurement. It now learns, per area and marker, which hit followed and how soon, and plans the next one like a
tankbuster with a cast: the least mitigation that survives it, or the invulnerability if nothing less does. Mitigation
is held back only under a planned invulnerability, as the marker may announce another hit this time.

## DoTs are refreshed by their ticks, on by default

"Time DoT refreshes from DoT ticks and time to kill (Experimental)", new and off by default in upstream, is on by
default here. White Mage, Astrologian, Scholar, Sage, Black Mage and Reaper then refresh their damage over time on the
last GCD that loses no tick, and leave it off a target that will die before it out-damages the filler. Off, a DoT is
refreshed a set number of GCDs before it runs out, as before.

## Tanks: the time a press needs to reach the server is measured on the request

A tankbuster plan presses its mitigation only while it can still reach the server before the hit is decided there,
and counts how many presses fit until then. Both figures were measured from RSR's own press, which for a weaponskill
pressed shortly before the global cooldown ran out could include its wait in the game's queue - the plan then gave
up on a mitigation too early, or counted presses that no longer fit. They are now measured from the moment a request
actually leaves, matched to the effect that answers it, and every press counts, also your own.

## Tanks: Reprisal and the party tool take turns over area attacks

All four tanks put their party tool and Reprisal on the same area attack, one weave after the other, and had nothing
for the next. Now the second waits while the first still works - a barrier until it is used up -, so two area attacks
in a row each get one. Both still go together when a member is in danger or the attack, as measured, would put an
unprotected member there. Setting: "Spread Reprisal and the party mitigation over a run of area attacks", on by default.

## Dark Knight: The Blackest Night in a pull only when the damage would break it

On a pull, the barrier went up once four enemies stood in range and no other mitigation ran - an assumption that four
enemies hit hard enough to break it within its duration and return the 3000 MP as Dark Arts. It now also waits until
the damage you took over the barrier's duration reached the share its text names. A soft pack keeps the MP for Edge of
Shadow; tankbusters and low health are not affected. DefenseTrace.log notes for each of your barriers whether it broke.
Setting: "In a pull, use The Blackest Night only once the damage you take would break it", on by default.

## White Mage: Liturgy of the Bell is pressed a second time

RSR never pressed the bell again, so the stacks a single hit left healed only when the bell ran out, mostly onto a
party healed by then. Once the bell has healed at least once and the party is below your area-heal thresholds, the
second press now releases the rest at once - after a hard hit, the bell's own heal comes with the hit and the rest right
after it. Setting: "Press Liturgy of the Bell a second time when the party needs an area heal", on by default.

## Machinist: Auto Crossbow only where it out-damages Blazing Shot

Auto Crossbow went out from three enemies in its cone. Blazing Shot, though, takes 15 s off Double Check and Checkmate,
which hit every enemy around their target: on three gathered enemies it brings 692 potency against Auto Crossbow's 540.
RSR now weighs the two with the potencies the game's tooltips state and with how many enemies Double Check and
Checkmate would reach. With all of them in reach, Auto Crossbow goes from six enemies, with one outside from four - the
thresholds The Balance gives. The action's AoE count stays the lower limit; where a figure cannot be read, Auto
Crossbow goes by it as before. DefenseTrace.log notes once per level what was read.

## Ninja: area actions from two enemies where they deal more

The Ninja chose its area actions from three enemies around it. With the potencies since patch 7.5, two of its pairs
turn at two:
- Hellfrog Medium (Deathfrog Medium) instead of Bhavacakra (Zesho Meppo) on two enemies, unless Meisui raises the
  single hit.
- Goka Mekkyaku instead of Hyosho Ranryu under Kassatsu, counted on the enemies Goka reaches.

Both are weighed with the potencies the tooltips state. Doton now goes down only while the enemies live through its
duration, as The Balance advises; otherwise Katon. A Hellfrog Medium AoE count you set yourself is set to two once.

## Black Mage: Foul from two enemies

Foul went out from three enemies, Xenoglossy below that. On two enemies Foul deals 600 and 450 against Xenoglossy's
890, as The Balance says ("Foul is now a gain in 2-target situations"). Its AoE count is now two, and both Black Mage
rotations also weigh the potencies the tooltips state, which keeps Xenoglossy at levels where it still deals more. A
Foul AoE count you set yourself is set to two once: RSR carries a changed default over into saved settings.

## White Mage: Liturgy of the Bell before a dangerous hit and into a series of hits

Under "Limit Liturgy Of The Bell to multihit party stacks" (off by default), the bell went only on seven known multihit
stack markers - twice in five evenings - and sat unused through fights without one. Two sub-options, both on by default, add the two
cases where it heals when the party needs it: "Also before a hit that would put a party member in danger" lays it, last
after Plenary Indulgence, Temperance and Divine Caress, before an announced hit that by its measured size would leave a
member in critical health and that reaches you - its first heal comes with the hit, the rest right after. "Also into an
enemy attack that hits in a series" learns which enemy attacks hit you and the party several times in a row, one
trigger a second, and lays the bell at the first hit of one: a ten-hit attack a second apart heals the party after each
of the next five.

## A series of hits counts as one attack in the area damage table

A listed area cast that lands several times right after one cast was rated by its largest single hit, so a cast that hit
four times for up to 24 % each counted as a small hit, and "Skip mitigation for small area casts" left the party
mitigation out against it. Hits that follow faster than the enemy could cast again are now added up per member.

## Healers: the heal forecast no longer goes blind after a hard first hit

The forecast of how a party member will stand when a heal lands measured his fall from the first moment he was below
full health in the last four minutes. After a hard first hit of a pull every later level stood above that moment, and
the forecast saw no fall at all for that member - heals answered only his health at the time. The fall is now measured
from the last time he stood at least as high. In a model of a healer acting on the forecast over 300 random tank
fights, the change is never worse and needs no more heals; the forecast itself halves the deaths against no forecast
at low heal thresholds. Setting: "Heal ahead of incoming damage" (unchanged).

## Tanks: the big mitigation waits in a pull until your health needs it

In a pull the defence opened by the number of enemies on you, and every pull of two or more got the big mitigation and
Rampart at once - also a small pack your healer carries you through, so it was missing at the wall-to-wall pull after.
Your short mitigation, Arm's Length and Reprisal still go at once. The big mitigation and Rampart now wait until your
health, at the rate measured net of every heal and mitigation, would fall below 30 % within their duration; then each
goes as soon as it is needed, no longer spaced by fixed times. A tankbuster, a cast aimed at you or a BossModReborn
tankbuster opens them as before, and so does danger to you or a tank. In a model of a healer and a tank over 100 random
dungeon runs the deaths fell from 19 to 5 and the time under 30 % to a third; with more damage or a weaker healer the
same direction. Setting: "Hold the big mitigation and Rampart in a pull until your health needs them" (on), with
"Health the big mitigation waits for" (30 %).

## GCD heals no longer wait on another healer while someone is about to fall

"Use GCDs to heal" off - the default for Astrologian and Sage - left GCD heals entirely to a second living healer, and
for Paladin, Red Mage and Summoner to the healers, even for a party member about to fall. The rotation now heals
whenever a member is in the critical class, whatever the other healer does; otherwise the setting holds as before.
Settings: "Use GCDs to heal. (Ignored if you are the only healer in party, or while a party member is about to fall)"
for the healers, "... (Ignored if there are no healers alive in party, or while a party member is about to fall)" for
the others.

## Healers: party heals on the group's average read the forecast too

"Heal ahead of incoming damage" promises every healing threshold the health a member is heading for when the heal
lands. The thresholds on the party's average in the healer rotations still read the health right now: Horoscope and
Lady of Crowns, Holos and Pneuma, the Scholar's Fey Blessing, Indomitability and Recitation with Indomitability, and
the downtime heal ("When the lowest party HP is below"). They read the forecast now. Pneuma as a party heal, a GCD with
a cast, goes out as much earlier as the party loses before it lands; for abilities the forecast reaches only over the
animation lock and little changes. With the setting off, all of them read what they did before.

## Gap closers that leap onto the target keep to your distance

"Only use damaging gap closers within this distance" (3 yalms by default) only reached the dashes such as Onslaught.
The leaps onto the target did not read it: a Paladin standing aside for a mechanic used Intervene to jump back onto the
boss from up to 20 yalms, and a Dragoon did the same with Stardiver and Dragonfire Dive, whose own limits are 20 yalms
by default. Every leap now keeps to the setting as well as to any limit its rotation sets - Intervene, Primal Rend,
Crimson Cyclone, Stardiver, Dragonfire Dive. A move-forward command, your own press, the countdown and the rotation
switches marked "Dangerous" or "at any range" are not limited. Standing at the enemy, a leap is only damage, as before;
out of reach it waits until you are back.

## Raises no longer wait three seconds

"Raise delay range" defaulted to 3 s, a wait meant to look less mechanical that leaves the fallen member down longer.
The default is now no delay; when he gets up is his choice when he accepts, and two raisers on one body are kept apart
as before. A configuration saved earlier keeps its value.

## Damage areas wait until the pack stays

Salted Earth and Doton went out as soon as you stood, so a tank's pause in a wall-to-wall pull was enough to lay them
where the pack did not stay. Now they wait until at least one enemy has been in their radius for a GCD and no fewer are
in it than, of those still alive, were at the start of that GCD: enemies shifting about inside it, one pushing in for
another at its edge in a big pull, one arriving or one dying leave it open, a pack moving on closes it. They also wait
while an enemy not yet in combat stands in the area or, in an instanced duty, within a tank's reach, and for a GCD after
one there came into combat: a tank who stops to pull the next group with his area attack moves on as soon as it lands,
and the area is not laid onto a group nobody has pulled. Doton gives way to Katon meanwhile. Slipstream waits likewise
around its target while Emerald Rites are left. Each area you lay writes one line to `DefenseTrace.log`: how long
enemies stood in it, and how long the ones it was laid on were alive outside it.
Setting: "Lay damage areas only where the enemies stay", on by default.

## Monk, Viper and Dragoon: a dash to the target at the pull, only from out of range

At the end of a countdown the Monk used Thunderclap whatever the distance - standing at the boss, a charge for nothing -
and, with a party member as focus target, dashed to that member instead of the boss. Viper and Dragoon walked. Now all
three dash in the last half second only when the target stands beyond melee range, and only to it: the first
weaponskill lands at the pull instead of up to 3 seconds later, as The Balance's Viper opener does with Slither. Setting:
"Dash to the target at the end of the countdown when out of melee range", on by default; turn it off for a pull you
want to wait out away from the boss.

## Viper: the experimental potion also in the opener

"Experimental Pot Usage(used up to 5 seconds before SerpentsIre comes off cooldown)", off by default, took the potion
only in the last 5 s of Serpent's Ire's cooldown, never while Serpent's Ire stood ready - so never in the opener. As
its text says, it now goes from 5 s before Serpent's Ire is back and while it stands ready.

## A raidwide during a tankbuster marker is no longer learned as the tankbuster

Any enemy action that struck a marked member while the marker stood counted as its tankbuster, so a raidwide landing
at that moment went into the tankbuster table and to the tank swap as one, beside the real tankbuster - seen with three
raidwides of an alliance raid. An action that has hit the whole party once is no longer taken for the marker's hit, and
what the table already holds for it is passed over.

## Under Pyretic and similar debuffs RSR stops acting

The "No-casting debuffs" list says "Do not take action if you have one of these debuffs", but RSR only held back in the
last 3 s before such a debuff ran out, and never under one without a timer. That fits a penalty on expiry (Acceleration
Bomb), not Pyretic or Causality, where every action costs health. Under these RSR now takes no action and cancels a
running cast for as long as the debuff lasts, recognised by the debuff's own text, with or without a BossModReborn
module.

## Interrupt and provoke delays work again

"Interrupt delay range" and "Provoke delay range" had done nothing
since upstream dropped their wiring in 2024. They delay again what their text says; the default is now no delay, and an
interrupt never waits past the cast it is meant to stop. A configuration saved earlier keeps its 0.5 to 1 s.

## Enemy debuffs on a pull follow their refresh setting

Without a BossModReborn prediction, Reprisal, Feint and Addle are kept up on a pull of enough enemies, renewed once the
debuff on the target is about to run out. How early was fixed at two GCDs; the action's own "Number of GCDs before the
DOT/Status effect is reapplied" did not reach it. It does now; with that setting at its default nothing changes.

## Two settings that did nothing are removed

"How early before next GCD should RSR use swiftcast for raise" and "Target color" were removed. Nothing had read them
since upstream dropped their code in 2025 and 2024, so moving them changed nothing, and raises behave as before.

## Healers and casters: an enemy's cast is stopped by the job's stun first

"Stop an enemy's cast with the job's stun or a sleep (experimental)", on by default, replaces the sleep-only setting.
It stops an area attack reaching the party, and now also a cast the game marks interruptible - what a melee stops with
Leg Sweep. A White Mage uses Holy when the caster stands in its radius and can still be stunned: it stuns every enemy
around and deals damage, where a sleep costs a GCD without. Otherwise Repose or Sleep goes on the enemy. With "Don't
pull new enemies with AoE" on, no area stop reaches an enemy that is not fighting yet.

What sleeps show is now kept over every session, like the AoE damage table: enemy kinds that resist a sleep are not
slept again, nor an enemy slept in a fight as often as its kind takes before it resists, nor an attack that went off
although the sleep landed in time. A sleep that landed too late makes the next ones land earlier instead of giving up
on the attack. The time from a sleep's cast to its effect is now measured from its cast bar. DefenseTrace.log records
every stop, every sleep and what came of it, and its first line shows what is stored.

