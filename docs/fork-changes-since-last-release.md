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

## `DefenseTrace.log` says what the defence turned down

Each defence decision, and each "open, nothing chosen", now ends with the actions the defence refused on its way and
the reason the game check gave, and with the holds that were standing. Each HP potion RSR chooses is written with the
branch that allowed it and your HP at that moment. After each fight a line gives what RSR's update cost per frame during
it: the number of cycles, the mean and the longest in milliseconds, and its share of the frame time.

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
