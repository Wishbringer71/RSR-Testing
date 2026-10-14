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
