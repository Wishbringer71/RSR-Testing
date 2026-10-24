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

"The range of random delay before interrupting hostile targets." and "Provoke random delay range." had done nothing
since upstream dropped their wiring in 2024. They delay again what their text says; the default is now no delay, and an
interrupt never waits past the cast it is meant to stop. A configuration saved earlier keeps its 0.5 to 1 s.

## Two settings that did nothing are removed

"How early before next GCD should RSR use swiftcast for raise" and "Target color" were removed. Nothing had read them
since upstream dropped their code in 2025 and 2024, so moving them changed nothing, and raises behave as before.

## Healers and casters: an enemy's cast is stopped by the job's stun first

"Stop an enemy's cast with the job's stun or a sleep (experimental)", on by default, replaces the sleep-only setting.
It stops an area attack reaching the party, and now also a cast the game marks interruptible - what a melee stops with
Leg Sweep. A White Mage uses Holy when the caster stands in its radius and can still be stunned: it stuns every enemy
around and deals damage, where a sleep costs a GCD without. Otherwise Repose or Sleep goes on the enemy. With "Don't
attack new mobs by AoE." on, no area stop reaches an enemy that is not fighting yet.

What sleeps show is now kept over every session, like the AoE damage table: enemy kinds that resist a sleep are not
slept again, nor an enemy slept in a fight as often as its kind takes before it resists, nor an attack that went off
although the sleep landed in time. A sleep that landed too late makes the next ones land earlier instead of giving up
on the attack. The time from a sleep's cast to its effect is now measured from its cast bar. DefenseTrace.log records
every stop, every sleep and what came of it, and its first line shows what is stored.

