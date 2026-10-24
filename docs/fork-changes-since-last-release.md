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

