# Changes since 7.5.7.1+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Scholar: Recitation only for an Excogitation that will follow

Recitation went out for any tank below `Minimum HP percent to use Excogitation as a heal instead of a defensive buff`,
while the Excogitation it was pressed for waits for danger under `Excogitation as a heal only on a target in danger`
(on by default). A tank low but not in danger - the off-tank after a swap - got Recitation and then no Excogitation:
Recitation ran out unused, and its minute-long recast was missing at the next tankbuster. Recitation for a tank now asks
for the same danger as the Excogitation that follows it.

## Bard, Dancer, Pictomancer: mitigation in the burst by default

`Prevent the use of defense abilties during burst` is now off by default. The defence paths only run for an area hit
or a tankbuster that is announced or measured, and the setting threw exactly those away while the burst ran, so a
raidwide inside the burst got no Troubadour, Shield Samba, Tempera or Addle. The mitigation now goes out and takes
one weave slot in the burst. Switched on, it still holds without exception.

## Healing: a healer being hit no longer waits behind a tank who stands higher

`Choose the heal target by danger` heals a healer or tank under their role threshold who is being attacked first. With
the tank's threshold at 45 % and the healer's at 40 %, a tank at 44 % came before a healer at 42 % although both were
being hit. Any healer or tank being attacked who stands as low as that pick or lower now joins it, and the lowest
health goes first, a healer before a tank at equal health.

## Red Mage: raise in the burst, heal in the burst when the healers do not keep up

`Prevent raising during burst combos` is now off by default: a dead member is raised at once, as on Summoner.
`Prevent healing during burst combos` still holds Vercure in the burst, but no longer when no healer in the party is
alive or a member is about to die because the healers do not keep up.

## Dark Knight: The Blackest Night and Oblation for the party find their target

With `Use The Blackest Night on lowest HP party member during AOE scenarios` or the Oblation counterpart on, the
member with the fewest HP was picked first and the health threshold checked after. When that member stood above the
threshold - a damage dealer with a small pool, barely hurt - the barrier went to nobody, although another member was
under it. The pick is now made among the members under the threshold.

## Gunbreaker: Aurora on full target usage goes by the heal order

With `How to use Aurora` on `Full target usage`, Aurora went to whoever had the fewest HP points, often a barely hurt
damage dealer rather than the member the heal was for. It now picks its target like every other heal, by danger.

## HP potions: the emergency is a tankbuster on you, not on anyone

The emergency branch of the HP potions, which drops the potion's own HP threshold, opened for every tankbuster
BossModReborn predicted, whoever it targeted. A White Mage at 60 % drank an Ultra-Potion for a tankbuster on the
tank. It now opens only for a tankbuster that reaches you: one cast at you, or a predicted one whose targets name you;
when BossModReborn names nobody, for a tank, or when the party has no tank alive. The same reading now decides when
a rotation counts a tankbuster as on you (the Dark Knight's The Blackest Night on itself among them).

## Damage over time on several enemies: most HP first, then by expiry

With several enemies in reach, the damage over time went on all of them in the order of the targeting type - by
default lowest HP first, so the enemy that lives longest got it last. The enemies now get it in the order of their
HP, most first, and once each carries it, it is renewed in the order it runs out. Direct single-target attacks
without damage over time keep the targeting type you chose.
This holds for every job's damage over time, recognised by the status's own text, and not where a rotation uses the
action for something else (a combo finisher, an upkeep while moving) or names the target. `Spread damage over time
by HP, then by expiry` (on by default) switches it off.

## Healers: no area heal on full health before a raidwide

Before an announced raidwide RSR raised the area heal whenever the hit would take someone below the area heal
threshold, and from full health a hit of a quarter to a third of the health already did. The heal - Medica, Cure
III, Afflatus Rapture or a heal over time - landed on a full party and was gone, and the same heal was needed again
after the hit. It now heals ahead of the hit only when the hit would leave someone about to fall - a member at 55 %
before a 45 % raidwide - and that member is missing health. Otherwise it heals after the hit, on health that is
missing. `DefenseTrace.log` names each such heal ahead of a hit, the member it was for and the size of the hit.

The White Mage's Afflatus Rapture at full lilies now waits until an announced raidwide has landed, so the GCD spent
to keep the lilies from overflowing heals what the raidwide took. It goes out at once when a fourth lily would
otherwise be lost.

## Healers: no raise race with another healer

A raise shows on its target only once it lands, so while another healer was hard-casting a raise, the body was
still a candidate and RSR without Swiftcast started a raise of its own on it - two casts for one body, and the slower
one was lost. RSR now reads the other player's cast and its target and leaves that body alone; another dead member
gets the raise instead, and Swiftcast stays for a body nobody is raising. If the other cast is broken off, the body
is a candidate again at once. When both raises are already being cast - the party list shows the cast bar at the
target reddish then - RSR stops its own as soon as the other one lands first, instead of spending its MP on a body
that is already up; at equal times it keeps casting. A body carrying the second form of the Raise status (id 1140)
also counts as raised now. `DefenseTrace.log` notes each raise left to someone else and each one stopped. The same
goes for Esuna:
RSR no longer cleanses a member another healer is already casting Esuna on, unless more than that one effect is
left.

## Tanks: the tankbuster plan keeps Reprisal for the next raidwide

The single-target defence keeps Reprisal off a tankbuster when the tank is covered and BossModReborn announces a
raidwide after Reprisal would run out and before it is back. The tankbuster plan, which picks the cheapest set of own
mitigations for a measured tankbuster, did not know that rule; from level 98 Reprisal was its cheapest member, so it
went on the tankbuster and the whole party went without its 10 % at the raidwide. The plan now takes Reprisal only
when no set without it survives the tankbuster.

## Small area casts stay unmitigated when BossModReborn predicts them

`Skip mitigation for small area casts` kept a learned area cast that leaves everybody above the area heal line from
raising the party mitigation - but only on RSR's own reading of the cast. When BossModReborn predicted the same cast
as a raidwide, the prediction opened the area defence anyway, and Plenary Indulgence, Temperance, Radiant Aegis or
Addle went out on hits of a few percent. A prediction that appears together with the start of such casts no longer
opens the area defence while they are being cast; a prediction with any other cast beside it, an unmeasured one, or
one that is left when the cast ends, is answered as before. `DefenseTrace.log` names each prediction held this way.

## Area casts: a member who missed a kill mechanic no longer sets its size

RSR learns how hard each listed area cast hits, as the largest share of a member's maximum HP it has seen, and that
figure only rises. A member who missed a kill mechanic set it for good: Megadeath (Forgall) was learned at 941 % from
one without the zombie debuff, while it took 10 % from one who had it, and Necrohazard at 588 % from one near its
centre. Every rule reading the figure then took the cast for a raidwide that kills: the heal ahead of it went out
whenever anyone missed health, held defensives were released, mitigation was spent, and a tank's tankbuster plan
counted the party dead. A hit that reaches a member's whole health and the barrier he carries is no longer measured;
one he survived only behind his barrier counts as his whole health. Readings above a whole maximum HP from before are
left out when the table loads and are no longer written back to the file, so such a cast is unrated until it lands
again - and unrated casts are mitigated as before - and then rated by the members who survived it.
`DefenseTrace.log` names each hit left out.

## A potion no longer counts as an action you used

ECommons, which RSR reads the game's effects through, read an item used on a target as an action with the same
number. RSR then booked that action as the last one you used, reported it to the tankbuster plan as your own and
filed the potion's healing under it. RSR now uses ECommons 3.2.1.26, which tells items and actions apart again.

## The target update no longer breaks off on an object without cast data

The target update read the cast of every untargetable enemy, and Dalamud's cast reading fails on an object that
carries no cast data. The update then stopped before it refreshed the raise, Esuna, provoke and interrupt targets and
the time-to-kill estimate, which kept their old values for that frame; Dalamud's log showed it as
`TargetUpdater.UpdateTargets Exception` in `IsCasting`. Such objects are now left out, there and where the target lists
are built.

## `DefenseTrace.log` says what the defence turned down

Each defence decision, and each "open, nothing chosen", now ends with the actions the defence refused on its way and
the reason the game check gave, and with the holds that were standing. Each HP potion RSR chooses is written with the
branch that allowed it and your HP at that moment. After each fight a line gives what RSR's update cost per frame during
it: the number of cycles, the mean and the longest in milliseconds, its share of the frame time, and the party size.
A tankbuster measured on a player who is not a tank is marked "(not a tank)". A hit on you from a source RSR does not
read as an enemy - one the game's object table does not hold, or a figure that is neither an enemy nor a combatant -
is written as such; RSR drops those hits everywhere else.

The White Mage's Holy stretch line is now written when its decision changes or once a GCD, as it was meant to be. It
counted the remaining stun time in every frame as a change and filled more than half of the file, about ten lines a
second.

## Tanks: a tankbuster that landed under Intervention or Taurochole is learned at its real size

The learned tankbuster table, which a tank's mitigation plan reads, scales every hit back to unmitigated by the
mitigation that stood when it landed. It did not know the reduction of Intervention, Taurochole, Third Eye, Magick
Barrier or Fey Illumination, because their effect texts phrase it differently. A tankbuster that hit a tank under one of
them was learned as smaller than it is, until a hit without it raised the figure, and a plan for that tankbuster could
come out too small for a defensive to fire. They are counted now, and so are Troubadour, Tactician and Shield Samba
at 15 % from level 98, which their effect texts leave blank because a trait sets the figure.

The table also no longer divides the same mitigation out twice. It credited a mitigation to every status sharing the
action's name, so a tank carrying Temperance's heal over time beside Temperance itself had Temperance divided out
twice, and the tankbuster was learned larger than it is. Only a status whose own text reduces damage counts now.
